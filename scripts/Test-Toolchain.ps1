[CmdletBinding()]
param(
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [string] $UpstreamPath,
    [string] $JavaPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($ManifestPath) { $ManifestPath = [IO.Path]::GetFullPath($ManifestPath) }
if ($UpstreamPath) { $UpstreamPath = [IO.Path]::GetFullPath($UpstreamPath) }
if ($JavaPath) { $JavaPath = [IO.Path]::GetFullPath($JavaPath) }
$arguments = @('run', 'sf2', 'toolchain', 'verify', '--manifest-path', $ManifestPath)
if ($UpstreamPath) { $arguments += @('--upstream-path', $UpstreamPath) }
if ($JavaPath) { $arguments += @('--java-path', $JavaPath) }
Push-Location -LiteralPath $repoRoot
try {
    & uv @arguments
    if ($LASTEXITCODE -ne 0) { throw "Toolchain verification failed: $LASTEXITCODE" }
} finally { Pop-Location }
