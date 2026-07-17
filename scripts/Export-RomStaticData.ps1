[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $LayoutPath = (Join-Path $PSScriptRoot '..\manifests\extractions\rom-static-layout.json'),
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-U16BE([byte[]] $Bytes, [int] $Offset) {
    return ([int] $Bytes[$Offset] -shl 8) -bor [int] $Bytes[$Offset + 1]
}

function Get-U32BE([byte[]] $Bytes, [int] $Offset) {
    return ([uint32] $Bytes[$Offset] -shl 24) -bor
        ([uint32] $Bytes[$Offset + 1] -shl 16) -bor
        ([uint32] $Bytes[$Offset + 2] -shl 8) -bor
        [uint32] $Bytes[$Offset + 3]
}

function Get-RawHex([byte[]] $Bytes, [int] $Offset, [int] $Length) {
    return [Convert]::ToHexString($Bytes[$Offset..($Offset + $Length - 1)])
}

function New-Record([int] $Id, [int] $Offset, [int] $Size, [byte[]] $Bytes, [object] $Decoded) {
    return [pscustomobject][ordered] @{
        id = $Id
        offset = $Offset
        rawHex = Get-RawHex $Bytes $Offset $Size
        decoded = $Decoded
    }
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$layout = Get-Content -Raw -LiteralPath $LayoutPath -Encoding utf8 | ConvertFrom-Json
$romManifestPath = Join-Path $repoRoot ([string] $layout.romManifest)
$romManifest = Get-Content -Raw -LiteralPath $romManifestPath -Encoding utf8 | ConvertFrom-Json
$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$bytes = [System.IO.File]::ReadAllBytes($resolvedRom)
$actualHash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
if ($actualHash -ne [string] $romManifest.hashes.sha256) {
    throw "ROM hash mismatch: expected $($romManifest.hashes.sha256), got $actualHash"
}
if (-not $OutputPath) { $OutputPath = Join-Path $repoRoot ([string] $layout.outputPath) }

$tables = [ordered] @{}

$records = [System.Collections.Generic.List[object]]::new()
$spec = $layout.tables.allyStartDefinitions
for ($id = 0; $id -lt [int] $spec.recordCount; $id++) {
    $offset = [int] $spec.start + $id * [int] $spec.recordSize
    $items = for ($slot = 0; $slot -lt 4; $slot++) {
        $raw = [int] $bytes[$offset + 2 + $slot]
        [pscustomobject][ordered] @{ raw = $raw; itemId = $raw -band 127; equipped = [bool] ($raw -band 128) }
    }
    $decoded = [pscustomobject][ordered] @{ classId = [int] $bytes[$offset]; level = [int] $bytes[$offset + 1]; items = @($items) }
    $records.Add((New-Record $id $offset ([int] $spec.recordSize) $bytes $decoded))
}
$tables.allyStartDefinitions = $records.ToArray()

$records = [System.Collections.Generic.List[object]]::new()
$spec = $layout.tables.classDefinitions
for ($id = 0; $id -lt [int] $spec.recordCount; $id++) {
    $offset = [int] $spec.start + $id * [int] $spec.recordSize
    $rawMoveType = [int] $bytes[$offset + 3]
    $decoded = [pscustomobject][ordered] @{
        movement = [int] $bytes[$offset]
        resistance = Get-U16BE $bytes ($offset + 1)
        movementType = ($rawMoveType -band 240) -shr 4
        movementTypeRaw = $rawMoveType
        prowess = [int] $bytes[$offset + 4]
    }
    $records.Add((New-Record $id $offset ([int] $spec.recordSize) $bytes $decoded))
}
$tables.classDefinitions = $records.ToArray()

$records = [System.Collections.Generic.List[object]]::new()
$spec = $layout.tables.itemDefinitions
for ($id = 0; $id -lt [int] $spec.recordCount; $id++) {
    $offset = [int] $spec.start + $id * [int] $spec.recordSize
    $spellRaw = [int] $bytes[$offset + 9]
    $effects = for ($effect = 0; $effect -lt 3; $effect++) {
        [pscustomobject][ordered] @{
            type = [int] $bytes[$offset + 10 + $effect * 2]
            parameter = [int] $bytes[$offset + 11 + $effect * 2]
        }
    }
    $decoded = [pscustomobject][ordered] @{
        equipFlags = Get-U32BE $bytes $offset
        range = [pscustomobject][ordered] @{ min = [int] $bytes[$offset + 5]; max = [int] $bytes[$offset + 4] }
        price = Get-U16BE $bytes ($offset + 6)
        itemType = [int] $bytes[$offset + 8]
        useSpell = [pscustomobject][ordered] @{ raw = $spellRaw; spellId = $spellRaw -band 63; level = (($spellRaw -shr 6) + 1) }
        equipEffects = @($effects)
    }
    $records.Add((New-Record $id $offset ([int] $spec.recordSize) $bytes $decoded))
}
$tables.itemDefinitions = $records.ToArray()

$records = [System.Collections.Generic.List[object]]::new()
$spec = $layout.tables.spellDefinitions
for ($id = 0; $id -lt [int] $spec.recordCount; $id++) {
    $offset = [int] $spec.start + $id * [int] $spec.recordSize
    $entryRaw = [int] $bytes[$offset]
    $animationRaw = [int] $bytes[$offset + 2]
    $decoded = [pscustomobject][ordered] @{
        entry = [pscustomobject][ordered] @{ raw = $entryRaw; spellId = $entryRaw -band 63; level = (($entryRaw -shr 6) + 1) }
        mpCost = [int] $bytes[$offset + 1]
        animation = [pscustomobject][ordered] @{
            raw = $animationRaw
            index = $animationRaw -band 31
            variation = (($animationRaw -band 96) -shr 5) + 1
            mirrored = [bool] ($animationRaw -band 128)
        }
        properties = [int] $bytes[$offset + 3]
        range = [pscustomobject][ordered] @{ min = [int] $bytes[$offset + 5]; max = [int] $bytes[$offset + 4] }
        radius = [int] $bytes[$offset + 6]
        power = [int] $bytes[$offset + 7]
    }
    $records.Add((New-Record $id $offset ([int] $spec.recordSize) $bytes $decoded))
}
$tables.spellDefinitions = $records.ToArray()

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{ romManifestId = [string] $romManifest.id; romSha256 = $actualHash }
    layoutId = [string] $layout.id
    tables = [pscustomobject] $tables
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[System.IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [System.Text.UTF8Encoding]::new($false))

[pscustomobject] @{ OutputPath = $resolvedOutput; SHA256 = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash; Status = 'PASS' } | Format-List
