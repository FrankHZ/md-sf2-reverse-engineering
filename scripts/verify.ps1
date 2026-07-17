[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [switch] $SkipRebuild
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

Write-Output '=== Phase 1 verification: PASS ==='
