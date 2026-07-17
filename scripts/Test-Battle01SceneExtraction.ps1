[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\extractions\battle01-scene.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath $ManifestPath -Encoding utf8 | ConvertFrom-Json
$sourcePath = Join-Path $repoRoot ([string] $manifest.sourceOutputPath)
$romOutputPath = Join-Path $repoRoot ([string] $manifest.romOutputPath)
$sourceRepeat = $sourcePath + '.repeat'
$romRepeat = $romOutputPath + '.repeat'

& (Join-Path $PSScriptRoot 'Export-Battle01Scene.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourcePath
& (Join-Path $PSScriptRoot 'Export-RomBattle01Scene.ps1') -RomPath $RomPath -OutputPath $romOutputPath
$sourceJson = Get-Content -Raw -LiteralPath $sourcePath -Encoding utf8
$romJson = Get-Content -Raw -LiteralPath $romOutputPath -Encoding utf8
if (-not ($sourceJson | Test-Json -SchemaFile (Join-Path $repoRoot 'schemas\battle01-scene.schema.json'))) { throw 'Battle01 scene source schema validation failed.' }
if (-not ($romJson | Test-Json -SchemaFile (Join-Path $repoRoot 'schemas\rom-battle01-scene.schema.json'))) { throw 'Battle01 scene ROM schema validation failed.' }
$sourceHash = (Get-FileHash $sourcePath -Algorithm SHA256).Hash
$romHash = (Get-FileHash $romOutputPath -Algorithm SHA256).Hash
if ($sourceHash -ne [string] $manifest.sourceOutputSha256 -or $romHash -ne [string] $manifest.romOutputSha256) { throw 'Battle01 scene golden hash mismatch.' }

$repeatOk = $false
try {
    & (Join-Path $PSScriptRoot 'Export-Battle01Scene.ps1') -UpstreamPath $UpstreamPath -OutputPath $sourceRepeat
    & (Join-Path $PSScriptRoot 'Export-RomBattle01Scene.ps1') -RomPath $RomPath -OutputPath $romRepeat
    if ((Get-FileHash $sourceRepeat -Algorithm SHA256).Hash -ne $sourceHash -or (Get-FileHash $romRepeat -Algorithm SHA256).Hash -ne $romHash) {
        throw 'Battle01 scene extraction is not deterministic.'
    }
    $repeatOk = $true
}
finally { if ($repeatOk) { Remove-Item -LiteralPath $sourceRepeat, $romRepeat -Force -ErrorAction SilentlyContinue } }

$source = $sourceJson | ConvertFrom-Json
$raw = $romJson | ConvertFrom-Json
$fields = @('id', 'x', 'y', 'width', 'height', 'triggerX', 'triggerY')
foreach ($field in $fields) {
    if ([int] $source.map.$field -ne [int] $raw.map.$field) { throw "Battle01 map parity failed for $field." }
}
if ($source.scene.customBackgroundExpression -ne 'TOWER_INTERIOR' -or [int] $raw.scene.customBackground -ne 9 -or
    [bool] $source.scene.enemyLeaderPresent -ne [bool] $raw.scene.enemyLeaderPresent -or
    [bool] $source.scene.halfExperience -ne [bool] $raw.scene.halfExperience) { throw 'Battle01 scene metadata parity failed.' }
foreach ($field in @('start', 'endExclusive', 'lengthBytes')) {
    if ([int] $source.terrain.compressedRange.$field -ne [int] $raw.terrain.compressedRange.$field) { throw "Battle01 terrain range parity failed for $field." }
}
if ($source.terrain.compressedSha256 -ne $raw.terrain.compressedSha256 -or
    $source.terrain.decompressedSha256 -ne $raw.terrain.decompressedSha256 -or
    (($source.terrain.valueCounts | ConvertTo-Json -Compress) -ne ($raw.terrain.valueCounts | ConvertTo-Json -Compress))) {
    throw 'Battle01 terrain source-ROM parity failed.'
}
[pscustomobject] @{
    SourceSHA256 = $sourceHash; RomSHA256 = $romHash; Map = [int] $source.map.id
    Area = "$($source.map.width)x$($source.map.height)"; TerrainBytes = [int] $source.terrain.decompressedLengthBytes
    TerrainValues = @($source.terrain.valueCounts.psobject.Properties).Count; Deterministic = $true; Status = 'PASS'
} | Format-List
