[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\rom-battle01-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Get-U16BE([byte[]] $Bytes, [int] $Offset) { return ([int] $Bytes[$Offset] -shl 8) -bor [int] $Bytes[$Offset + 1] }

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$romManifest = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\roms\sf2-us.json') -Encoding utf8 | ConvertFrom-Json
$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$hash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
if ($hash -ne [string] $romManifest.hashes.sha256) { throw 'Battle01 ROM hash mismatch.' }
$bytes = [IO.File]::ReadAllBytes($resolvedRom)
$start = 0x1B32E2
$end = 0x1B3376
$cursor = $start
$allyCount = [int] $bytes[$cursor]; $cursor++
$enemyCount = [int] $bytes[$cursor]; $cursor++
$regionCount = [int] $bytes[$cursor]; $cursor++
$pointCount = [int] $bytes[$cursor]; $cursor++
$entities = [System.Collections.Generic.List[object]]::new()
for ($id = 0; $id -lt $allyCount + $enemyCount; $id++) {
    $offset = $cursor
    $entities.Add([pscustomobject][ordered] @{
        id = $id
        offset = $offset
        kind = if ($id -lt $allyCount) { 'ally' } else { 'enemy' }
        identity = [int] $bytes[$offset]
        x = [int] $bytes[$offset + 1]
        y = [int] $bytes[$offset + 2]
        aiCommandset = [int] $bytes[$offset + 3]
        item = Get-U16BE $bytes ($offset + 4)
        behavior = [pscustomobject][ordered] @{
            primaryOrder = [int] $bytes[$offset + 6]
            primaryRegion = [int] $bytes[$offset + 7]
            secondaryOrder = [int] $bytes[$offset + 8]
            secondaryRegion = [int] $bytes[$offset + 9]
            filler = [int] $bytes[$offset + 10]
            spawn = [int] $bytes[$offset + 11]
        }
    })
    $cursor += 12
}
$regions = [System.Collections.Generic.List[object]]::new()
for ($id = 0; $id -lt $regionCount; $id++) {
    $offset = $cursor
    $vertexCount = [int] $bytes[$cursor]; $cursor++
    $unknown = [int] $bytes[$cursor]; $cursor++
    $vertices = for ($vertex = 0; $vertex -lt $vertexCount; $vertex++) {
        $x = [int] $bytes[$cursor]; $y = [int] $bytes[$cursor + 1]; $cursor += 2
        [pscustomobject][ordered] @{ x = $x; y = $y }
    }
    $regions.Add([pscustomobject][ordered] @{
        id = $id
        offset = $offset
        vertexCount = $vertexCount
        unknown = $unknown
        vertices = @($vertices)
        trailingBytes = @([int] $bytes[$cursor], [int] $bytes[$cursor + 1])
    })
    $cursor += 2
}
$points = for ($id = 0; $id -lt $pointCount; $id++) {
    $point = [pscustomobject][ordered] @{ x = [int] $bytes[$cursor]; y = [int] $bytes[$cursor + 1] }
    $cursor += 2
    $point
}
if ($cursor -ne $end) { throw "Battle01 decoder ended at 0x$('{0:X}' -f $cursor), expected 0x$('{0:X}' -f $end)." }
$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{ romManifestId = [string] $romManifest.id; romSha256 = $hash }
    battleId = 1
    romRange = [pscustomobject][ordered] @{ start = $start; endExclusive = $end; lengthBytes = $end - $start }
    counts = [pscustomobject][ordered] @{ allies = $allyCount; enemies = $enemyCount; aiRegions = $regionCount; aiPoints = $pointCount }
    entities = $entities.ToArray()
    aiRegions = $regions.ToArray()
    aiPoints = @($points)
}
$resolvedOutput = [IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [Text.UTF8Encoding]::new($false))
[pscustomobject] @{ OutputPath = $resolvedOutput; SHA256 = (Get-FileHash $resolvedOutput -Algorithm SHA256).Hash; Entities = $entities.Count; Regions = $regions.Count; Status = 'PASS' } | Format-List
