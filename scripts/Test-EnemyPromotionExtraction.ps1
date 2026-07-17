[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\extractions\enemy-promotion-data.json'),
    [string] $LayoutPath = (Join-Path $PSScriptRoot '..\manifests\extractions\enemy-promotion-rom-layout.json'),
    [string] $SourceSchemaPath = (Join-Path $PSScriptRoot '..\schemas\enemy-promotion-data.schema.json'),
    [string] $RomSchemaPath = (Join-Path $PSScriptRoot '..\schemas\rom-enemy-promotion-data.schema.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-AsmInteger([string] $Value) {
    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') { return [Convert]::ToUInt64($Matches[1], 16) }
    if ($trimmed -match '^0x([0-9A-Fa-f]+)$') { return [Convert]::ToUInt64($Matches[1], 16) }
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
    foreach ($token in $Expression.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $part = $token.Trim()
        try { $partValue = Convert-AsmInteger $part }
        catch {
            $key = "${Prefix}_${part}"
            if (-not $Enums.ContainsKey($key)) { throw "Enum not found: $key (from '$Expression')" }
            $partValue = $Enums[$key]
        }
        $value = $value -bor [uint64] $partValue
    }
    return $value
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath $ManifestPath -Encoding utf8 | ConvertFrom-Json
$layout = Get-Content -Raw -LiteralPath $LayoutPath -Encoding utf8 | ConvertFrom-Json
$sourcePath = Join-Path $repoRoot ([string] $manifest.sourceOutputPath)
$romPathOut = Join-Path $repoRoot ([string] $manifest.romOutputPath)
$sourceRepeat = $sourcePath + '.repeat'
$romRepeat = $romPathOut + '.repeat'

& (Join-Path $PSScriptRoot 'Export-EnemyPromotionData.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourcePath
& (Join-Path $PSScriptRoot 'Export-RomEnemyPromotionData.ps1') -RomPath $RomPath -LayoutPath $LayoutPath -OutputPath $romPathOut

$sourceJson = Get-Content -Raw -LiteralPath $sourcePath -Encoding utf8
$romJson = Get-Content -Raw -LiteralPath $romPathOut -Encoding utf8
if (-not ($sourceJson | Test-Json -SchemaFile $SourceSchemaPath)) { throw 'Enemy/promotion source extraction failed schema validation.' }
if (-not ($romJson | Test-Json -SchemaFile $RomSchemaPath)) { throw 'Enemy/promotion ROM extraction failed schema validation.' }
$sourceHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
$romHash = (Get-FileHash -LiteralPath $romPathOut -Algorithm SHA256).Hash
if ($sourceHash -ne [string] $manifest.sourceOutputSha256) { throw "Source extraction hash mismatch: $sourceHash" }
if ($romHash -ne [string] $manifest.romOutputSha256 -or $romHash -ne [string] $layout.outputSha256) {
    throw "ROM extraction hash mismatch: $romHash"
}

$repeatOk = $false
try {
    & (Join-Path $PSScriptRoot 'Export-EnemyPromotionData.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourceRepeat
    & (Join-Path $PSScriptRoot 'Export-RomEnemyPromotionData.ps1') -RomPath $RomPath -LayoutPath $LayoutPath -OutputPath $romRepeat
    if ((Get-FileHash -LiteralPath $sourceRepeat -Algorithm SHA256).Hash -ne $sourceHash -or
        (Get-FileHash -LiteralPath $romRepeat -Algorithm SHA256).Hash -ne $romHash) {
        throw 'Enemy/promotion extraction is not deterministic.'
    }
    $repeatOk = $true
}
finally {
    if ($repeatOk) {
        Remove-Item -LiteralPath $sourceRepeat, $romRepeat -Force -ErrorAction SilentlyContinue
    }
}

$source = $sourceJson | ConvertFrom-Json
$raw = $romJson | ConvertFrom-Json
$enums = Get-EnumMap (Join-Path $UpstreamPath 'disasm\sf2enums.asm')
$mismatches = [System.Collections.Generic.List[string]]::new()
$fieldsCompared = 0

function Test-Field([string] $Owner, [string] $Field, $Expected, $Actual) {
    $script:fieldsCompared++
    if ([string] $Expected -ne [string] $Actual) { $script:mismatches.Add("${Owner}.${Field}: source=$Expected ROM=$Actual") }
}

$promotionProperties = @('regularBaseClasses', 'regularPromotedClasses', 'specialBaseClasses', 'specialPromotedClasses', 'specialPromotionItems')
for ($section = 0; $section -lt $promotionProperties.Count; $section++) {
    $property = $promotionProperties[$section]
    $expectedRows = @($source.promotions.$property)
    $actual = $raw.promotions[$section]
    Test-Field "promotions[$section]" 'count' $expectedRows.Count $actual.count
    for ($index = 0; $index -lt $expectedRows.Count; $index++) {
        Test-Field "promotions[$section]" "values[$index]" $expectedRows[$index].id $actual.values[$index]
    }
}

for ($id = 0; $id -lt @($source.enemies).Count; $id++) {
    $expected = $source.enemies[$id]
    $name = $raw.enemyNames[$id]
    $actual = $raw.enemyDefinitions[$id].decoded
    $expectedNameBytes = [System.Collections.Generic.List[byte]]::new()
    $expectedNameBytes.AddRange([Text.Encoding]::ASCII.GetBytes([string] $expected.displayName))
    foreach ($suffix in $expected.nameSuffixBytes) { $expectedNameBytes.Add([byte] $suffix) }
    Test-Field "enemyNames[$id]" 'displayName' $expected.displayName $name.displayName
    Test-Field "enemyNames[$id]" 'encodedLength' $expected.nameEncodedLength $name.encodedLength
    Test-Field "enemyNames[$id]" 'payloadHex' ([Convert]::ToHexString($expectedNameBytes.ToArray())) $name.payloadHex
    Test-Field "enemies[$id]" 'unknownByte' $expected.unknownByte $actual.unknownByte
    Test-Field "enemies[$id]" 'spellPower' (Resolve-Expression $expected.spellPower 'SPELLPOWER' $enums) $actual.spellPower
    Test-Field "enemies[$id]" 'level' $expected.level $actual.level
    Test-Field "enemies[$id]" 'maxHp' $expected.maxHp $actual.maxHp
    Test-Field "enemies[$id]" 'maxMp' $expected.maxMp $actual.maxMp
    Test-Field "enemies[$id]" 'baseAttack' $expected.baseAttack $actual.baseAttack
    Test-Field "enemies[$id]" 'baseDefense' $expected.baseDefense $actual.baseDefense
    Test-Field "enemies[$id]" 'baseAgility' $expected.baseAgility $actual.baseAgility
    Test-Field "enemies[$id]" 'baseMovement' $expected.baseMovement $actual.baseMovement
    Test-Field "enemies[$id]" 'resistance' (Resolve-Expression $expected.resistanceExpression 'RESISTANCE' $enums) $actual.resistance
    Test-Field "enemies[$id]" 'prowess' (Resolve-Expression $expected.prowessExpression 'PROWESS' $enums) $actual.prowess
    for ($slot = 0; $slot -lt 4; $slot++) {
        Test-Field "enemies[$id]" "items[$slot]" (Resolve-Expression $expected.items[$slot].expression 'ITEM' $enums) $actual.items[$slot].raw
        Test-Field "enemies[$id]" "spells[$slot]" (Resolve-Expression $expected.spells[$slot].expression 'SPELL' $enums) $actual.spells[$slot].raw
    }
    Test-Field "enemies[$id]" 'initialStatus' (Resolve-Expression $expected.initialStatusExpression 'STATUSEFFECT' $enums) $actual.initialStatus
    Test-Field "enemies[$id]" 'movementTypeRaw' ((Resolve-Expression $expected.movementType 'MOVETYPE' $enums) -shl 4) $actual.movementTypeRaw
    Test-Field "enemies[$id]" 'aiBitfield' (Resolve-Expression $expected.aiBitfieldExpression 'AIBITFIELD' $enums) $actual.aiBitfield
    Test-Field "enemies[$id]" 'reservedBytesZero' $true $actual.reservedBytesZero
}

if ($mismatches.Count -gt 0) {
    $preview = $mismatches | Select-Object -First 20
    throw "Enemy/promotion source-ROM parity failed with $($mismatches.Count) mismatch(es):`n$($preview -join "`n")"
}

[pscustomobject] @{
    SourceSHA256 = $sourceHash
    RomSHA256 = $romHash
    PromotionValues = 39
    EnemyRecords = @($source.enemies).Count
    FieldsCompared = $fieldsCompared
    FieldMismatches = 0
    Deterministic = $true
    Status = 'PASS'
} | Format-List
