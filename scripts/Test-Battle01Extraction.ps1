[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\extractions\battle01-data.json'),
    [string] $SourceSchemaPath = (Join-Path $PSScriptRoot '..\schemas\battle01-data.schema.json'),
    [string] $RomSchemaPath = (Join-Path $PSScriptRoot '..\schemas\rom-battle01-data.schema.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Convert-AsmInteger([string] $Value) {
    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') { return [Convert]::ToUInt64($Matches[1], 16) }
    if ($trimmed -match '^-?\d+$') { return [long] $trimmed }
    throw "Unsupported assembly integer: $Value"
}
function Get-EnumMap([string] $Path) {
    $map = @{}
    foreach ($line in (Get-Content -LiteralPath $Path -Encoding utf8)) {
        if ($line -notmatch '^([A-Z][A-Z0-9_]+):\s+equ\s+([^\s;]+)') { continue }
        try { $map[$Matches[1]] = Convert-AsmInteger $Matches[2] } catch { }
    }
    return $map
}
function Resolve-Expression([string] $Expression, [string] $Prefix, [hashtable] $Enums) {
    [uint64] $value = 0
    foreach ($token in $Expression.Split('|', [StringSplitOptions]::RemoveEmptyEntries)) {
        $part = $token.Trim()
        try { $partValue = Convert-AsmInteger $part }
        catch {
            $key = "${Prefix}_${part}"
            if (-not $Enums.ContainsKey($key)) { throw "Enum not found: $key" }
            $partValue = $Enums[$key]
        }
        $value = $value -bor [uint64] $partValue
    }
    return $value
}

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath $ManifestPath -Encoding utf8 | ConvertFrom-Json
$sourcePath = Join-Path $repoRoot ([string] $manifest.sourceOutputPath)
$romOutputPath = Join-Path $repoRoot ([string] $manifest.romOutputPath)
$sourceRepeat = $sourcePath + '.repeat'
$romRepeat = $romOutputPath + '.repeat'
& (Join-Path $PSScriptRoot 'Export-Battle01Data.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourcePath
& (Join-Path $PSScriptRoot 'Export-RomBattle01Data.ps1') -RomPath $RomPath -OutputPath $romOutputPath
$sourceJson = Get-Content -Raw -LiteralPath $sourcePath -Encoding utf8
$romJson = Get-Content -Raw -LiteralPath $romOutputPath -Encoding utf8
if (-not ($sourceJson | Test-Json -SchemaFile $SourceSchemaPath)) { throw 'Battle01 source schema validation failed.' }
if (-not ($romJson | Test-Json -SchemaFile $RomSchemaPath)) { throw 'Battle01 ROM schema validation failed.' }
$sourceHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
$romHash = (Get-FileHash -LiteralPath $romOutputPath -Algorithm SHA256).Hash
if ($sourceHash -ne [string] $manifest.sourceOutputSha256 -or $romHash -ne [string] $manifest.romOutputSha256) {
    throw "Battle01 golden hash mismatch: $sourceHash / $romHash"
}
$repeatOk = $false
try {
    & (Join-Path $PSScriptRoot 'Export-Battle01Data.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourceRepeat
    & (Join-Path $PSScriptRoot 'Export-RomBattle01Data.ps1') -RomPath $RomPath -OutputPath $romRepeat
    if ((Get-FileHash $sourceRepeat -Algorithm SHA256).Hash -ne $sourceHash -or
        (Get-FileHash $romRepeat -Algorithm SHA256).Hash -ne $romHash) { throw 'Battle01 extraction is not deterministic.' }
    $repeatOk = $true
}
finally {
    if ($repeatOk) { Remove-Item -LiteralPath $sourceRepeat, $romRepeat -Force -ErrorAction SilentlyContinue }
}

$source = $sourceJson | ConvertFrom-Json
$raw = $romJson | ConvertFrom-Json
$enums = Get-EnumMap (Join-Path $UpstreamPath 'disasm\sf2enums.asm')
$mismatches = [Collections.Generic.List[string]]::new()
$fieldsCompared = 0
function Test-Field([string] $Owner, [string] $Field, $Expected, $Actual) {
    $script:fieldsCompared++
    if ([string] $Expected -ne [string] $Actual) { $script:mismatches.Add("${Owner}.${Field}: source=$Expected ROM=$Actual") }
}

foreach ($field in @('allies', 'enemies', 'aiRegions', 'aiPoints')) {
    Test-Field 'counts' $field $source.counts.$field $raw.counts.$field
}
for ($id = 0; $id -lt @($source.entities).Count; $id++) {
    $expected = $source.entities[$id]
    $actual = $raw.entities[$id]
    $identity = if ($expected.kind -eq 'ally') {
        Convert-AsmInteger $expected.identityExpression
    } else {
        Resolve-Expression $expected.identityExpression 'ENEMY' $enums
    }
    Test-Field "entities[$id]" 'kind' $expected.kind $actual.kind
    Test-Field "entities[$id]" 'identity' $identity $actual.identity
    Test-Field "entities[$id]" 'x' $expected.x $actual.x
    Test-Field "entities[$id]" 'y' $expected.y $actual.y
    Test-Field "entities[$id]" 'aiCommandset' (Resolve-Expression $expected.aiCommandsetExpression 'AICOMMANDSET' $enums) $actual.aiCommandset
    Test-Field "entities[$id]" 'item' (Resolve-Expression $expected.itemExpression 'ITEM' $enums) $actual.item
    Test-Field "entities[$id]" 'primaryOrder' (Resolve-Expression $expected.behavior.primaryOrderExpression 'AIORDER' $enums) $actual.behavior.primaryOrder
    Test-Field "entities[$id]" 'primaryRegion' $expected.behavior.primaryRegion $actual.behavior.primaryRegion
    Test-Field "entities[$id]" 'secondaryOrder' (Resolve-Expression $expected.behavior.secondaryOrderExpression 'AIORDER' $enums) $actual.behavior.secondaryOrder
    Test-Field "entities[$id]" 'secondaryRegion' $expected.behavior.secondaryRegion $actual.behavior.secondaryRegion
    Test-Field "entities[$id]" 'filler' $expected.behavior.filler $actual.behavior.filler
    Test-Field "entities[$id]" 'spawn' (Resolve-Expression $expected.behavior.spawnExpression 'SPAWN' $enums) $actual.behavior.spawn
}
for ($id = 0; $id -lt @($source.aiRegions).Count; $id++) {
    $expected = $source.aiRegions[$id]
    $actual = $raw.aiRegions[$id]
    Test-Field "aiRegions[$id]" 'vertexCount' $expected.vertexCount $actual.vertexCount
    Test-Field "aiRegions[$id]" 'unknown' $expected.unknown $actual.unknown
    for ($vertex = 0; $vertex -lt @($expected.vertices).Count; $vertex++) {
        Test-Field "aiRegions[$id]" "vertices[$vertex].x" $expected.vertices[$vertex].x $actual.vertices[$vertex].x
        Test-Field "aiRegions[$id]" "vertices[$vertex].y" $expected.vertices[$vertex].y $actual.vertices[$vertex].y
    }
    Test-Field "aiRegions[$id]" 'trailing[0]' $expected.trailingBytes[0] $actual.trailingBytes[0]
    Test-Field "aiRegions[$id]" 'trailing[1]' $expected.trailingBytes[1] $actual.trailingBytes[1]
}
if ($mismatches.Count -gt 0) {
    throw "Battle01 source-ROM parity failed with $($mismatches.Count) mismatch(es):`n$(($mismatches | Select-Object -First 20) -join "`n")"
}
[pscustomobject] @{
    SourceSHA256 = $sourceHash
    RomSHA256 = $romHash
    Entities = @($source.entities).Count
    AiRegions = @($source.aiRegions).Count
    FieldsCompared = $fieldsCompared
    FieldMismatches = 0
    Deterministic = $true
    Status = 'PASS'
} | Format-List
