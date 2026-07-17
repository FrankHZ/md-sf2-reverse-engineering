[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $LayoutPath = (Join-Path $PSScriptRoot '..\manifests\extractions\rom-static-layout.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\rom-static-data.schema.json')
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
        try {
            $partValue = Convert-AsmInteger $part
        }
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
$layout = Get-Content -Raw -LiteralPath $LayoutPath -Encoding utf8 | ConvertFrom-Json
$sourcePath = Join-Path $repoRoot 'local\derived\static-data.json'
$romOutputPath = Join-Path $repoRoot ([string] $layout.outputPath)
$romRepeatPath = $romOutputPath + '.repeat'

& (Join-Path $PSScriptRoot 'Export-StaticData.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourcePath
& (Join-Path $PSScriptRoot 'Export-RomStaticData.ps1') -RomPath $RomPath -LayoutPath $LayoutPath -OutputPath $romOutputPath

$rawJson = Get-Content -Raw -LiteralPath $romOutputPath -Encoding utf8
if (-not ($rawJson | Test-Json -SchemaFile $SchemaPath)) { throw 'ROM static extraction failed schema validation.' }
$rawHash = (Get-FileHash -LiteralPath $romOutputPath -Algorithm SHA256).Hash
if ($rawHash -ne [string] $layout.outputSha256) {
    throw "ROM static extraction hash mismatch: expected $($layout.outputSha256), got $rawHash"
}
$repeatOk = $false
try {
    & (Join-Path $PSScriptRoot 'Export-RomStaticData.ps1') -RomPath $RomPath -LayoutPath $LayoutPath -OutputPath $romRepeatPath
    if ((Get-FileHash -LiteralPath $romRepeatPath -Algorithm SHA256).Hash -ne $rawHash) {
        throw 'ROM static extraction is not deterministic.'
    }
    $repeatOk = $true
}
finally {
    if ($repeatOk -and (Test-Path -LiteralPath $romRepeatPath)) { Remove-Item -LiteralPath $romRepeatPath -Force }
}

$source = Get-Content -Raw -LiteralPath $sourcePath -Encoding utf8 | ConvertFrom-Json
$raw = $rawJson | ConvertFrom-Json
$enumPath = Join-Path $UpstreamPath 'disasm\sf2enums.asm'
$enums = Get-EnumMap $enumPath
$mismatches = [System.Collections.Generic.List[string]]::new()

function Test-Field([string] $Table, [int] $Id, [string] $Field, $Expected, $Actual) {
    if ([string] $Expected -ne [string] $Actual) {
        $script:mismatches.Add("$Table[$Id].${Field}: source=$Expected ROM=$Actual")
    }
}

for ($id = 0; $id -lt @($source.allies).Count; $id++) {
    $expected = $source.allies[$id]
    $actual = $raw.tables.allyStartDefinitions[$id].decoded
    Test-Field 'allies' $id 'classId' (Resolve-Expression $expected.startClass 'CLASS' $enums) $actual.classId
    Test-Field 'allies' $id 'level' $expected.startLevel $actual.level
    for ($slot = 0; $slot -lt 4; $slot++) {
        Test-Field 'allies' $id "items[$slot].raw" (Resolve-Expression $expected.startItems[$slot].expression 'ITEM' $enums) $actual.items[$slot].raw
    }
}

for ($id = 0; $id -lt @($source.classes).Count; $id++) {
    $expected = $source.classes[$id]
    $actual = $raw.tables.classDefinitions[$id].decoded
    Test-Field 'classes' $id 'movement' $expected.movement $actual.movement
    Test-Field 'classes' $id 'resistance' (Resolve-Expression $expected.resistanceExpression 'RESISTANCE' $enums) $actual.resistance
    Test-Field 'classes' $id 'movementTypeRaw' ((Resolve-Expression $expected.movementType 'MOVETYPE' $enums) -shl 4) $actual.movementTypeRaw
    Test-Field 'classes' $id 'prowess' (Resolve-Expression $expected.prowessExpression 'PROWESS' $enums) $actual.prowess
}

for ($id = 0; $id -lt @($source.items).Count; $id++) {
    $expected = $source.items[$id]
    $actual = $raw.tables.itemDefinitions[$id].decoded
    Test-Field 'items' $id 'equipFlags' (Resolve-Expression $expected.equipFlagsExpression 'EQUIPFLAG' $enums) $actual.equipFlags
    Test-Field 'items' $id 'range.min' $expected.range.min $actual.range.min
    Test-Field 'items' $id 'range.max' $expected.range.max $actual.range.max
    Test-Field 'items' $id 'price' $expected.price $actual.price
    Test-Field 'items' $id 'itemType' (Resolve-Expression $expected.itemTypeExpression 'ITEMTYPE' $enums) $actual.itemType
    Test-Field 'items' $id 'useSpell.raw' (Resolve-Expression $expected.useSpellExpression 'SPELL' $enums) $actual.useSpell.raw
    for ($effect = 0; $effect -lt 3; $effect++) {
        Test-Field 'items' $id "equipEffects[$effect].type" (Resolve-Expression $expected.equipEffects[$effect].type 'EQUIPEFFECT' $enums) $actual.equipEffects[$effect].type
        Test-Field 'items' $id "equipEffects[$effect].parameter" ([int] $expected.equipEffects[$effect].parameter -band 255) $actual.equipEffects[$effect].parameter
    }
}

for ($id = 0; $id -lt @($source.spellDefinitions).Count; $id++) {
    $expected = $source.spellDefinitions[$id]
    $actual = $raw.tables.spellDefinitions[$id].decoded
    Test-Field 'spells' $id 'entry.raw' (Resolve-Expression $expected.entryExpression 'SPELL' $enums) $actual.entry.raw
    Test-Field 'spells' $id 'mpCost' $expected.mpCost $actual.mpCost
    Test-Field 'spells' $id 'animation.raw' (Resolve-Expression $expected.animationExpression 'SPELLANIMATION' $enums) $actual.animation.raw
    Test-Field 'spells' $id 'properties' (Resolve-Expression $expected.propertiesExpression 'SPELLPROPS' $enums) $actual.properties
    Test-Field 'spells' $id 'range.min' $expected.range.min $actual.range.min
    Test-Field 'spells' $id 'range.max' $expected.range.max $actual.range.max
    Test-Field 'spells' $id 'radius' $expected.radius $actual.radius
    Test-Field 'spells' $id 'power' $expected.power $actual.power
}

if ($mismatches.Count -gt 0) {
    $preview = $mismatches | Select-Object -First 20
    throw "Source/ROM static parity failed with $($mismatches.Count) mismatch(es):`n$($preview -join "`n")"
}

[pscustomobject] @{
    RomOutputPath = $romOutputPath
    RomOutputSHA256 = $rawHash
    RecordsCompared = 32 + 32 + 128 + 89
    FieldMismatches = 0
    Deterministic = $true
    Status = 'PASS'
} | Format-List
