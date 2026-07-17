[CmdletBinding()]
param(
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [string] $UpstreamPath,
    [string] $JavaPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$resolvedManifest = (Resolve-Path -LiteralPath $ManifestPath).Path
$manifest = Get-Content -Raw -LiteralPath $resolvedManifest -Encoding utf8 | ConvertFrom-Json

if ($manifest.schemaVersion -ne 1) {
    throw "Unsupported toolchain manifest version: $($manifest.schemaVersion)"
}

if (-not $UpstreamPath) {
    $UpstreamPath = Join-Path $repoRoot ([string] $manifest.sf2disasm.localPath)
}
if (-not $JavaPath) {
    $JavaPath = Join-Path $repoRoot ([string] $manifest.java.localJavaPath)
}

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$resolvedJava = (Resolve-Path -LiteralPath $JavaPath).Path
$bizHawkArchive = (Resolve-Path -LiteralPath (Join-Path $repoRoot ([string] $manifest.bizhawk.localArchivePath))).Path
$bizHawkExecutable = (Resolve-Path -LiteralPath (Join-Path $repoRoot ([string] $manifest.bizhawk.localExecutablePath))).Path

$actualRemote = (& git -C $resolvedUpstream remote get-url origin).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Unable to read SF2DISASM origin at $resolvedUpstream"
}
$actualCommit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Unable to read SF2DISASM commit at $resolvedUpstream"
}
$trackedChanges = @(& git -C $resolvedUpstream status --porcelain --untracked-files=no)
if ($LASTEXITCODE -ne 0) {
    throw "Unable to inspect SF2DISASM worktree at $resolvedUpstream"
}

if ($actualRemote -ne [string] $manifest.sf2disasm.repository) {
    throw "SF2DISASM remote mismatch: expected '$($manifest.sf2disasm.repository)', got '$actualRemote'"
}
if ($actualCommit -ne [string] $manifest.sf2disasm.commit) {
    throw "SF2DISASM commit mismatch: expected '$($manifest.sf2disasm.commit)', got '$actualCommit'"
}
if ($trackedChanges.Count -gt 0) {
    throw "SF2DISASM has tracked local changes:`n$($trackedChanges -join "`n")"
}

$toolResults = foreach ($tool in $manifest.sf2disasm.buildTools) {
    $toolPath = Join-Path $resolvedUpstream ([string] $tool.path)
    $item = Get-Item -LiteralPath $toolPath
    $actualHash = (Get-FileHash -LiteralPath $toolPath -Algorithm SHA256).Hash
    if ($item.Length -ne [long] $tool.sizeBytes -or $actualHash -ne [string] $tool.sha256) {
        throw "Build tool provenance mismatch: $($tool.path)"
    }

    [pscustomobject] @{
        Path = [string] $tool.path
        SizeBytes = $item.Length
        SHA256 = $actualHash
        Status = 'PASS'
    }
}

$javaOutput = @(& $resolvedJava -version 2>&1 | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "Java failed with exit code $LASTEXITCODE at $resolvedJava"
}
$expectedJavaVersion = ([string] $manifest.java.version).Split('+')[0]
if (($javaOutput -join "`n") -notmatch [regex]::Escape($expectedJavaVersion)) {
    throw "Java version mismatch: expected $($manifest.java.version), got $($javaOutput -join '; ')"
}

$bizHawkArchiveItem = Get-Item -LiteralPath $bizHawkArchive
$bizHawkArchiveHash = (Get-FileHash -LiteralPath $bizHawkArchive -Algorithm SHA256).Hash
if ($bizHawkArchiveItem.Length -ne [long] $manifest.bizhawk.archiveSizeBytes -or
    $bizHawkArchiveHash -ne [string] $manifest.bizhawk.archiveSha256) {
    throw "BizHawk archive provenance mismatch: $bizHawkArchive"
}
$bizHawkExecutableItem = Get-Item -LiteralPath $bizHawkExecutable
$bizHawkExecutableHash = (Get-FileHash -LiteralPath $bizHawkExecutable -Algorithm SHA256).Hash
if ($bizHawkExecutableItem.Length -ne [long] $manifest.bizhawk.executableSizeBytes -or
    $bizHawkExecutableHash -ne [string] $manifest.bizhawk.executableSha256) {
    throw "BizHawk executable provenance mismatch: $bizHawkExecutable"
}
[pscustomobject] @{
    UpstreamPath = $resolvedUpstream
    UpstreamCommit = $actualCommit
    BuildToolsVerified = @($toolResults).Count
    JavaPath = $resolvedJava
    JavaVersion = [string] $manifest.java.version
    BizHawkPath = $bizHawkExecutable
    BizHawkVersion = [string] $manifest.bizhawk.release
    Status = 'PASS'
} | Format-List
