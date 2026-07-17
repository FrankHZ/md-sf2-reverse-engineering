[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\battle01-turn-order-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-battle01-turn-order-fixture.schema.json'),
    [string] $ActivationFixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\battle01-region-activation-v1.json'),
    [string] $ActivationSchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-battle01-region-activation-fixture.schema.json'),
    [int] $TimeoutSeconds = 45
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 Battle01 turn-order fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
$activationFixtureJson = Get-Content -Raw -LiteralPath $ActivationFixturePath -Encoding utf8
if (-not ($activationFixtureJson | Test-Json -SchemaFile $ActivationSchemaPath)) { throw 'H3 Battle01 activation fixture failed schema validation.' }
$activationFixture = $activationFixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 Battle01 turn-order ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 Battle01 turn-order emulator contract mismatch.' }

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Seed ([int] $fixture.seed) -Scenario baseline -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or $observed.scenario -ne 'baseline' -or
    [int] $observed.seed -ne [int] $fixture.seed -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 Battle01 turn-order execution context mismatch.'
}
if (@($observed.entries).Count -ne @($fixture.expectedEntries).Count) { throw 'H3 Battle01 turn-order entry count mismatch.' }
for ($index = 0; $index -lt @($fixture.expectedEntries).Count; $index++) {
    $expected = $fixture.expectedEntries[$index]; $actual = $observed.entries[$index]
    if ([int] $actual.combatant -ne [int] $expected.combatant -or [int] $actual.score -ne [int] $expected.score) {
        throw "H3 Battle01 turn-order mismatch at index $index."
    }
}
if ([int] $observed.activation.newlyTriggered -ne [int] $activationFixture.expected.initial.newlyTriggered) {
    throw 'H3 Battle01 initial newly-triggered-region bitfield mismatch.'
}
for ($index = 0; $index -lt 3; $index++) {
    if ([bool] $observed.activation.regionFlags[$index] -ne [bool] $activationFixture.expected.initial.regionFlags[$index]) {
        throw "H3 Battle01 initial region flag mismatch at index $index."
    }
}
for ($index = 0; $index -lt 6; $index++) {
    $expected = $activationFixture.expected.initial.enemies[$index]
    $actual = $observed.activation.enemies[$index]
    if ([int] $actual.combatant -ne [int] $expected.combatant -or [int] $actual.bitfield -ne [int] $expected.bitfield) {
        throw "H3 Battle01 initial activation bitfield mismatch at index $index."
    }
}
$allies = @($observed.entries | Where-Object { $_.combatant -lt 128 }).Count
$enemies = @($observed.entries | Where-Object { $_.combatant -ge 128 }).Count
[pscustomobject] @{
    Fixture = [string] $fixture.id; Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Battle = [int] $fixture.battleId; Seed = ('0x{0:X4}' -f [int] $fixture.seed); Entries = @($observed.entries).Count
    Allies = $allies; Enemies = $enemies; InitialRegionFlags = (@($observed.activation.regionFlags) -join ',')
    FunctionEntry = ('0x{0:X}' -f [int] $fixture.function.entryAddress); Status = 'PASS'
} | Format-List
