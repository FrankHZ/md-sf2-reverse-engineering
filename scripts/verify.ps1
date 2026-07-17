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

Write-Output '=== Documentation: design-contract traceability ==='
& (Join-Path $PSScriptRoot 'Test-DesignContracts.ps1')

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
    Write-Output '=== H2: battle 01 placement and AI regions ==='
    & (Join-Path $PSScriptRoot 'Test-Battle01Extraction.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
    Write-Output '=== H2: battle 01 terrain and scene metadata ==='
    & (Join-Path $PSScriptRoot 'Test-Battle01SceneExtraction.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
}

if (-not $SkipRuntime) {
    Write-Output '=== H3: original RNG runtime behavior ==='
    & (Join-Path $PSScriptRoot 'Test-H3RngFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: original stat-gain runtime behavior ==='
    & (Join-Path $PSScriptRoot 'Test-H3StatGainFixture.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
    Write-Output '=== H3: battle 01 initialization and turn order ==='
    & (Join-Path $PSScriptRoot 'Test-H3Battle01TurnOrderFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: battle 01 activation regions and enemy AI state ==='
    & (Join-Path $PSScriptRoot 'Test-H3Battle01RegionActivationFixture.ps1') -RomPath $RomPath -UpstreamPath $UpstreamPath
    Write-Output '=== H3: turn-order boundary behavior ==='
    & (Join-Path $PSScriptRoot 'Test-H3TurnOrderBoundariesFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: physical damage construction and persistent battle-scene replay ==='
    & (Join-Path $PSScriptRoot 'Test-H3PhysicalDamageFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: dodge, double attack, counter, and counter half-damage ==='
    & (Join-Path $PSScriptRoot 'Test-H3AttackChainFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: successful airborne dodge and no-damage path ==='
    & (Join-Path $PSScriptRoot 'Test-H3DodgeFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: lethal target rejects double and counter follow-ups ==='
    & (Join-Path $PSScriptRoot 'Test-H3LethalFollowupFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: out-of-range target rejects counter follow-up ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterRangeFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: sleeping target rejects counter follow-up ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterSleepFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: stunned target rejects counter follow-up ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterStunFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: same-side target rejects counter follow-up ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterSameSideFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: Burst Rock rejects counter follow-up ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterBurstRockFixture.ps1') -RomPath $RomPath
    Write-Output '=== H3: remaining special-enemy counter exclusions ==='
    & (Join-Path $PSScriptRoot 'Test-H3CounterSpecialEnemiesFixture.ps1') -RomPath $RomPath
}

Write-Output '=== Repository verification: PASS ==='
