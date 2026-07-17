[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [switch] $SkipRebuild,
    [switch] $SkipExtraction,
    [switch] $SkipRuntime
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Output '=== H0: ROM baseline ==='
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath

Write-Output '=== Toolchain provenance ==='
& (Join-Path $PSScriptRoot 'Test-Toolchain.ps1') -UpstreamPath $UpstreamPath

if (-not $SkipRebuild) {
    Write-Output '=== H1: bit-perfect original rebuild ==='
    & (Join-Path $PSScriptRoot 'Invoke-Sf2Rebuild.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
}

if (-not $SkipExtraction) {
    Write-Output '=== H2: deterministic static-data extraction ==='
    & (Join-Path $PSScriptRoot 'Test-StaticExtraction.ps1') -UpstreamPath $UpstreamPath
    Write-Output '=== H2: ROM byte decode and source parity ==='
    & (Join-Path $PSScriptRoot 'Test-RomStaticParity.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
    Write-Output '=== H2: ally growth and spell-learning extraction ==='
    & (Join-Path $PSScriptRoot 'Test-GrowthExtraction.ps1') -UpstreamPath $UpstreamPath
    Write-Output '=== H2: promotions and enemy definitions ==='
    & (Join-Path $PSScriptRoot 'Test-EnemyPromotionExtraction.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
}

if (-not $SkipRuntime) {
    Write-Output '=== H3: original RNG runtime behavior ==='
    & (Join-Path $PSScriptRoot 'Test-H3RngFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: original stat-gain runtime behavior ==='
    & (Join-Path $PSScriptRoot 'Test-H3StatGainFixture.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
}

Write-Output '=== Repository verification: PASS ==='
