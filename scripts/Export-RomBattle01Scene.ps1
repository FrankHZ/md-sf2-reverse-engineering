[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\rom-battle01-scene.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\Sf2StackCompression.ps1')

function Get-U32BE([byte[]] $Bytes, [int] $Offset) {
    return ([int64] $Bytes[$Offset] -shl 24) -bor ([int64] $Bytes[$Offset + 1] -shl 16) -bor ([int64] $Bytes[$Offset + 2] -shl 8) -bor [int64] $Bytes[$Offset + 3]
}
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\roms\sf2-us.json') -Encoding utf8 | ConvertFrom-Json
$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$hash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
if ($hash -ne [string] $manifest.hashes.sha256) { throw 'Battle01 scene ROM hash mismatch.' }
$bytes = [IO.File]::ReadAllBytes($resolvedRom)

$terrainStart = [int] (Get-U32BE $bytes (0x1AD104 + 4))
$terrainEnd = [int] (Get-U32BE $bytes (0x1AD104 + 8))
$compressed = [byte[]] $bytes[$terrainStart..($terrainEnd - 1)]
$terrain = Expand-Sf2StackCompressedData -Data $compressed -ExpectedLength 2304
$counts = [ordered] @{}
foreach ($value in 0..255) {
    $count = @($terrain | Where-Object { $_ -eq $value }).Count
    if ($count -gt 0) { $counts[[string] $value] = $count }
}
$mapOffset = 0x7A36 + 7
$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{ romManifestId = [string] $manifest.id; romSha256 = $hash }
    battleId = 1
    map = [pscustomobject][ordered] @{
        id = [int] $bytes[$mapOffset]; x = [int] $bytes[$mapOffset + 1]; y = [int] $bytes[$mapOffset + 2]
        width = [int] $bytes[$mapOffset + 3]; height = [int] $bytes[$mapOffset + 4]
        triggerX = [int] $bytes[$mapOffset + 5]; triggerY = [int] $bytes[$mapOffset + 6]
    }
    scene = [pscustomobject][ordered] @{
        customBackground = [int] $bytes[0x1FA8A + 1]
        enemyLeaderPresent = ([sbyte] $bytes[0x47C8E + 1] -ne 0)
        halfExperience = ([int] $bytes[0xA870] -eq 1)
    }
    terrain = [pscustomobject][ordered] @{
        pointerTableOffset = 0x1AD104
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
