[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\lethal-followup-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-lethal-followup-fixture.schema.json'),
    [int] $TimeoutSeconds = 50
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -Encoding utf8 -LiteralPath $FixturePath
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 lethal follow-up fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 lethal follow-up ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 lethal follow-up emulator contract mismatch.' }

$base = [Math]::Max([int] $fixture.setup.actor.attack - [int] $fixture.setup.target.defense, 1)
$reduced = [int] [Math]::Floor($base * 205 / 256)
$range = ($reduced -shr 3) + 1
$minimumDamage = $reduced - 2 * ($range - 1)
if ($minimumDamage -ne [int] $fixture.expected.minimumDamage -or
    $minimumDamage -lt [int] $fixture.setup.target.hp) {
    throw 'Static lethal-damage lower-bound model mismatch.'
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario lethal -TimeoutSeconds $TimeoutSeconds) -join [Environment]::NewLine
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 lethal follow-up execution context mismatch.'
}
foreach ($field in @('targetDies', 'doubleBefore', 'doubleAfter', 'counterBefore', 'counterAfter')) {
    if ([string] $observed.validation.$field -ne [string] $fixture.expected.validation.$field) {
        throw "H3 lethal follow-up validation mismatch: $field."
    }
}
if ([int] $observed.damageCalls -ne [int] $fixture.expected.damageCalls -or
    [int] $observed.targetHp -ne [int] $fixture.expected.targetHp) {
    throw 'H3 lethal follow-up executed an unexpected damage path.'
}

[pscustomobject]@{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    TargetDies = [bool] $observed.validation.targetDies
    Double = "$( [bool] $observed.validation.doubleBefore )->$( [bool] $observed.validation.doubleAfter )"
    Counter = "$( [bool] $observed.validation.counterBefore )->$( [bool] $observed.validation.counterAfter )"
    DamageCalls = [int] $observed.damageCalls
    TargetHp = [int] $observed.targetHp
    Status = 'PASS'
} | Format-List
