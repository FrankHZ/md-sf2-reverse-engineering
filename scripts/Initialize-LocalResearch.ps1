[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [switch] $SkipDefenderScan
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$localRoot = Join-Path $repoRoot 'local'
$resolvedManifest = (Resolve-Path -LiteralPath $ManifestPath).Path
$manifest = Get-Content -Raw -LiteralPath $resolvedManifest -Encoding utf8 | ConvertFrom-Json

& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath

$resolvedInputRom = (Resolve-Path -LiteralPath $RomPath).Path
$romDirectory = Join-Path $localRoot 'roms'
$localRom = Join-Path $romDirectory 'sf2-us.bin'
New-Item -ItemType Directory -Path $romDirectory -Force | Out-Null

if (-not [System.IO.Path]::GetFullPath($resolvedInputRom).Equals(
    [System.IO.Path]::GetFullPath($localRom),
    [System.StringComparison]::OrdinalIgnoreCase
)) {
    Copy-Item -LiteralPath $resolvedInputRom -Destination $localRom -Force
}
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $localRom

$upstreamPath = Join-Path $repoRoot ([string] $manifest.sf2disasm.localPath)
$upstreamParent = Split-Path -Parent $upstreamPath
New-Item -ItemType Directory -Path $upstreamParent -Force | Out-Null

if (-not (Test-Path -LiteralPath (Join-Path $upstreamPath '.git'))) {
    if (Test-Path -LiteralPath $upstreamPath) {
        $existing = @(Get-ChildItem -LiteralPath $upstreamPath -Force)
        if ($existing.Count -gt 0) {
            throw "Refusing to initialize a non-empty upstream path: $upstreamPath"
        }
    }
    else {
        New-Item -ItemType Directory -Path $upstreamPath -Force | Out-Null
    }

    & git -C $upstreamPath init
    if ($LASTEXITCODE -ne 0) { throw 'Unable to initialize local SF2DISASM checkout.' }
    & git -C $upstreamPath remote add origin ([string] $manifest.sf2disasm.repository)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to add SF2DISASM origin.' }
    & git -C $upstreamPath fetch --depth 1 origin ([string] $manifest.sf2disasm.commit)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to fetch pinned SF2DISASM commit.' }
    & git -C $upstreamPath checkout --detach FETCH_HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Unable to check out pinned SF2DISASM commit.' }
}

$downloadDirectory = Join-Path $localRoot 'downloads'
$archivePath = Join-Path $downloadDirectory ([string] $manifest.java.archiveName)
$javaPath = Join-Path $repoRoot ([string] $manifest.java.localJavaPath)
$extractRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $javaPath))
New-Item -ItemType Directory -Path $downloadDirectory -Force | Out-Null

if (-not (Test-Path -LiteralPath $archivePath)) {
    Invoke-WebRequest -Uri ([string] $manifest.java.archiveUrl) -OutFile $archivePath -Headers @{
        'User-Agent' = 'md-sf2-research/phase1'
    }
}

$archive = Get-Item -LiteralPath $archivePath
$archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
if ($archive.Length -ne [long] $manifest.java.archiveSizeBytes -or
    $archiveHash -ne [string] $manifest.java.archiveSha256) {
    throw "Temurin archive provenance mismatch: $archivePath"
}

if (-not (Test-Path -LiteralPath $javaPath)) {
    if (Test-Path -LiteralPath $extractRoot) {
        throw "Java extraction root exists but java.exe is missing: $extractRoot"
    }
    New-Item -ItemType Directory -Path $extractRoot -Force | Out-Null
    Expand-Archive -LiteralPath $archivePath -DestinationPath $extractRoot
}

if (-not $SkipDefenderScan -and (Get-Command Start-MpScan -ErrorAction SilentlyContinue)) {
    Start-MpScan -ScanType CustomScan -ScanPath (Join-Path $upstreamPath 'tools')
}

$bizHawkArchive = Join-Path $repoRoot ([string] $manifest.bizhawk.localArchivePath)
$bizHawkExecutable = Join-Path $repoRoot ([string] $manifest.bizhawk.localExecutablePath)
$bizHawkExtractRoot = Split-Path -Parent $bizHawkExecutable
New-Item -ItemType Directory -Path (Split-Path -Parent $bizHawkArchive) -Force | Out-Null
if (-not (Test-Path -LiteralPath $bizHawkArchive)) {
    Invoke-WebRequest -Uri ([string] $manifest.bizhawk.archiveUrl) -OutFile $bizHawkArchive -Headers @{
        'User-Agent' = 'md-sf2-research/h3'
    }
}
$bizHawkArchiveItem = Get-Item -LiteralPath $bizHawkArchive
$bizHawkArchiveHash = (Get-FileHash -LiteralPath $bizHawkArchive -Algorithm SHA256).Hash
if ($bizHawkArchiveItem.Length -ne [long] $manifest.bizhawk.archiveSizeBytes -or
    $bizHawkArchiveHash -ne [string] $manifest.bizhawk.archiveSha256) {
    throw "BizHawk archive provenance mismatch: $bizHawkArchive"
}
if (-not (Test-Path -LiteralPath $bizHawkExecutable)) {
    if (Test-Path -LiteralPath $bizHawkExtractRoot) {
        throw "BizHawk extraction root exists but EmuHawk.exe is missing: $bizHawkExtractRoot"
    }
    Expand-Archive -LiteralPath $bizHawkArchive -DestinationPath $bizHawkExtractRoot
}

Write-Warning ([string] $manifest.sf2disasm.licenseStatus)
Write-Warning ([string] $manifest.bizhawk.licenseStatus)
& (Join-Path $PSScriptRoot 'Test-Toolchain.ps1') -UpstreamPath $upstreamPath -JavaPath $javaPath

Write-Output 'Local research environment is ready. Run: pwsh ./scripts/verify.ps1'
