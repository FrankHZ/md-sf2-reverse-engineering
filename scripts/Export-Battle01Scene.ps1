[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\battle01-scene.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\Sf2StackCompression.ps1')

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$actualCommit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne [string] $toolchain.sf2disasm.commit) { throw 'Battle01 scene extraction requires pinned SF2DISASM.' }

$terrainRelative = 'data/battles/entries/battle01/terrain.bin'
$terrainPath = Join-Path $resolvedUpstream "disasm\$($terrainRelative.Replace('/', '\'))"
$compressed = [IO.File]::ReadAllBytes($terrainPath)
$terrain = Expand-Sf2StackCompressedData -Data $compressed -ExpectedLength 2304
$splitLine = Get-Content -LiteralPath (Join-Path $resolvedUpstream 'split\sf2splits.txt') -Encoding utf8 |
    Where-Object { $_ -match 'data/battles/entries/battle01/terrain\.bin\s*$' } | Select-Object -First 1
if ($splitLine -notmatch '^#split\s+0x([0-9A-Fa-f]+),0x([0-9A-Fa-f]+),') { throw 'Battle01 terrain split range was not found.' }
$terrainStart = [Convert]::ToInt32($Matches[1], 16)
$terrainEnd = [Convert]::ToInt32($Matches[2], 16)
if ($terrainEnd - $terrainStart -ne $compressed.Length) { throw 'Battle01 terrain split range length mismatch.' }
$counts = [ordered] @{}
foreach ($value in 0..255) {
    $count = @($terrain | Where-Object { $_ -eq $value }).Count
    if ($count -gt 0) { $counts[[string] $value] = $count }
}

$battleMapLine = (Get-Content -LiteralPath (Join-Path $resolvedUpstream 'disasm\data\battles\global\battlemapcoords.asm') -Encoding utf8 |
    Where-Object { $_ -match '^\s*battleMapCoordinates\s+' } | Select-Object -Index 1)
if ($battleMapLine -notmatch 'battleMapCoordinates\s+(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+)') { throw 'Battle01 map coordinates were not found.' }
$mapValues = 1..7 | ForEach-Object { [int] $Matches[$_] }

$customLine = (Get-Content -LiteralPath (Join-Path $resolvedUpstream 'disasm\data\battles\global\custombackgrounds.asm') -Encoding utf8 |
    Where-Object { $_ -match '^\s*background\s+' } | Select-Object -Index 1)
if ($customLine -notmatch '^\s*background\s+([A-Z0-9_]+)') { throw 'Battle01 custom background was not found.' }
$background = $Matches[1]
$leaderLine = (Get-Content -LiteralPath (Join-Path $resolvedUpstream 'disasm\data\battles\global\enemyleaderpresence.asm') -Encoding utf8 |
    Where-Object { $_ -match '^\s*dc\.b\s+' } | Select-Object -Index 1)
if ($leaderLine -notmatch '^\s*dc\.b\s+(-?\d+)') { throw 'Battle01 leader flag was not found.' }
$leaderFlag = [int] $Matches[1]
$halfExpText = Get-Content -Raw -LiteralPath (Join-Path $resolvedUpstream 'disasm\data\battles\global\halvedexpearnedbattles.asm') -Encoding utf8

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{
        repository = [string] $toolchain.sf2disasm.repository
        commit = $actualCommit
        terrainSourcePath = $terrainRelative
        terrainSourceSha256 = (Get-FileHash -LiteralPath $terrainPath -Algorithm SHA256).Hash
    }
    battle = [pscustomobject][ordered] @{ id = 1; code = 'INSIDE_ANCIENT_TOWER' }
    map = [pscustomobject][ordered] @{
        id = $mapValues[0]; x = $mapValues[1]; y = $mapValues[2]; width = $mapValues[3]; height = $mapValues[4]
        triggerX = $mapValues[5]; triggerY = $mapValues[6]
    }
    scene = [pscustomobject][ordered] @{
        customBackgroundExpression = $background
        enemyLeaderPresent = ($leaderFlag -ne 0)
        halfExperience = ($halfExpText -match '(?m)^\s*battle\s+INSIDE_ANCIENT_TOWER\s*$')
    }
    terrain = [pscustomobject][ordered] @{
        compressedRange = [pscustomobject][ordered] @{ start = $terrainStart; endExclusive = $terrainEnd; lengthBytes = $terrainEnd - $terrainStart }
        compressedSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($compressed))
        decompressedWidth = 48; decompressedHeight = 48; decompressedLengthBytes = $terrain.Length
        decompressedSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($terrain))
        valueCounts = [pscustomobject] $counts
    }
}
$resolvedOutput = [IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 12) + "`n", [Text.UTF8Encoding]::new($false))
[pscustomobject] @{ OutputPath = $resolvedOutput; SHA256 = (Get-FileHash $resolvedOutput -Algorithm SHA256).Hash; TerrainBytes = $terrain.Length; Status = 'PASS' } | Format-List
