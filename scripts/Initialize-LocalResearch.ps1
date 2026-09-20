[CmdletBinding()]
param(
    [string] $RomPath,
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [switch] $SkipDefenderScan
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($ManifestPath) { $ManifestPath = [IO.Path]::GetFullPath($ManifestPath) }
if ($RomPath) { $RomPath = [IO.Path]::GetFullPath($RomPath) }
$arguments = @('run', 'sf2', 'init', '--manifest-path', $ManifestPath)
if ($RomPath) { $arguments += @('--rom-path', $RomPath) }
if ($SkipDefenderScan) { $arguments += '--skip-defender-scan' }
Push-Location -LiteralPath $repoRoot
try {
    & uv @arguments
    if ($LASTEXITCODE -ne 0) { throw "Research initialization failed: $LASTEXITCODE" }
} finally { Pop-Location }
