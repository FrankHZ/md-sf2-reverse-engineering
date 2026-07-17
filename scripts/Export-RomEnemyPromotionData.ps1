[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $LayoutPath = (Join-Path $PSScriptRoot '..\manifests\extractions\enemy-promotion-rom-layout.json'),
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-U16BE([byte[]] $Bytes, [int] $Offset) {
    return ([int] $Bytes[$Offset] -shl 8) -bor [int] $Bytes[$Offset + 1]
}

function Get-RawHex([byte[]] $Bytes, [int] $Offset, [int] $Length) {
    return [Convert]::ToHexString($Bytes[$Offset..($Offset + $Length - 1)])
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$layout = Get-Content -Raw -LiteralPath $LayoutPath -Encoding utf8 | ConvertFrom-Json
$romManifest = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ([string] $layout.romManifest)) -Encoding utf8 | ConvertFrom-Json
$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$bytes = [System.IO.File]::ReadAllBytes($resolvedRom)
$actualHash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
if ($actualHash -ne [string] $romManifest.hashes.sha256) { throw 'ROM hash mismatch for enemy/promotion decode.' }
if (-not $OutputPath) { $OutputPath = Join-Path $repoRoot ([string] $layout.outputPath) }

$promotionSpec = $layout.tables.promotions
$promotionOffset = [int] $promotionSpec.start
$promotionSections = [System.Collections.Generic.List[object]]::new()
for ($section = 0; $section -lt [int] $promotionSpec.sectionCount; $section++) {
    $sectionOffset = $promotionOffset
    $count = [int] $bytes[$promotionOffset]
    $promotionOffset++
    $values = @($bytes[$promotionOffset..($promotionOffset + $count - 1)] | ForEach-Object { [int] $_ })
    $promotionOffset += $count
    $promotionSections.Add([pscustomobject][ordered] @{
        id = $section
        offset = $sectionOffset
        count = $count
        values = $values
    })
}
if ($promotionOffset -ne [int] $promotionSpec.endExclusive) {
    throw "Promotion table ended at 0x$('{0:X}' -f $promotionOffset), expected 0x$('{0:X}' -f [int] $promotionSpec.endExclusive)."
}

$nameSpec = $layout.tables.enemyNames
$nameOffset = [int] $nameSpec.start
$enemyNames = [System.Collections.Generic.List[object]]::new()
for ($id = 0; $id -lt [int] $nameSpec.recordCount; $id++) {
    $recordOffset = $nameOffset
    $length = [int] $bytes[$nameOffset]
    $nameOffset++
    $payload = [byte[]] $bytes[$nameOffset..($nameOffset + $length - 1)]
    $nameOffset += $length
    $trailingNull = $length -gt 0 -and $payload[$payload.Length - 1] -eq 0
    $textLength = if ($trailingNull) { $payload.Length - 1 } else { $payload.Length }
    $displayName = if ($textLength -gt 0) { [Text.Encoding]::ASCII.GetString($payload, 0, $textLength) } else { '' }
    $enemyNames.Add([pscustomobject][ordered] @{
        id = $id
        offset = $recordOffset
        encodedLength = $length
        payloadHex = [Convert]::ToHexString($payload)
        displayName = $displayName
        trailingNull = $trailingNull
    })
}
if ($nameOffset -ne [int] $nameSpec.endExclusive) {
    throw "Enemy-name table ended at 0x$('{0:X}' -f $nameOffset), expected 0x$('{0:X}' -f [int] $nameSpec.endExclusive)."
}

$definitionSpec = $layout.tables.enemyDefinitions
$enemyDefinitions = [System.Collections.Generic.List[object]]::new()
for ($id = 0; $id -lt [int] $definitionSpec.recordCount; $id++) {
    $offset = [int] $definitionSpec.start + $id * [int] $definitionSpec.recordSize
    $items = for ($slot = 0; $slot -lt 4; $slot++) {
        $raw = Get-U16BE $bytes ($offset + 32 + $slot * 2)
        [pscustomobject][ordered] @{ raw = $raw; itemId = $raw -band 127; equipped = [bool] ($raw -band 128) }
    }
    $spells = for ($slot = 0; $slot -lt 4; $slot++) {
        $raw = [int] $bytes[$offset + 40 + $slot]
        [pscustomobject][ordered] @{ raw = $raw; spellId = $raw -band 63; level = ($raw -shr 6) + 1 }
    }
    $reservedOffsets = @(1..9) + @(14, 15, 17, 19, 21, 23, 25, 28, 29, 31, 46, 47, 48, 50, 51, 54, 55)
    $nonzeroReserved = @($reservedOffsets | Where-Object { $bytes[$offset + $_] -ne 0 })
    $movementTypeRaw = [int] $bytes[$offset + 49]
    $enemyDefinitions.Add([pscustomobject][ordered] @{
        id = $id
        offset = $offset
        rawHex = Get-RawHex $bytes $offset ([int] $definitionSpec.recordSize)
        decoded = [pscustomobject][ordered] @{
            unknownByte = [int] $bytes[$offset]
            spellPower = [int] $bytes[$offset + 10]
            level = [int] $bytes[$offset + 11]
            maxHp = Get-U16BE $bytes ($offset + 12)
            maxMp = [int] $bytes[$offset + 16]
            baseAttack = [int] $bytes[$offset + 18]
            baseDefense = [int] $bytes[$offset + 20]
            baseAgility = [int] $bytes[$offset + 22]
            baseMovement = [int] $bytes[$offset + 24]
            resistance = Get-U16BE $bytes ($offset + 26)
            prowess = [int] $bytes[$offset + 30]
            items = @($items)
            spells = @($spells)
            initialStatus = Get-U16BE $bytes ($offset + 44)
            movementTypeRaw = $movementTypeRaw
            movementType = ($movementTypeRaw -band 240) -shr 4
            aiBitfield = Get-U16BE $bytes ($offset + 52)
            reservedBytesZero = $nonzeroReserved.Count -eq 0
        }
    })
}
$definitionsEnd = [int] $definitionSpec.start + [int] $definitionSpec.recordCount * [int] $definitionSpec.recordSize
if ($definitionsEnd -ne [int] $definitionSpec.endExclusive) { throw 'Enemy-definition layout length mismatch.' }

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{ romManifestId = [string] $romManifest.id; romSha256 = $actualHash }
    layoutId = [string] $layout.id
    promotions = $promotionSections.ToArray()
    enemyNames = $enemyNames.ToArray()
    enemyDefinitions = $enemyDefinitions.ToArray()
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[System.IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [System.Text.UTF8Encoding]::new($false))

[pscustomobject] @{
    OutputPath = $resolvedOutput
    SHA256 = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash
    PromotionSections = $promotionSections.Count
    EnemyNames = $enemyNames.Count
    EnemyDefinitions = $enemyDefinitions.Count
    Status = 'PASS'
} | Format-List
