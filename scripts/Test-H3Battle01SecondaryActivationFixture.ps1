[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\battle01-secondary-activation-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-battle01-secondary-activation-fixture.schema.json'),
    [int] $TimeoutSeconds = 50
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -Encoding utf8 -LiteralPath $FixturePath
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 Battle01 secondary-activation fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 Battle01 secondary-activation ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 Battle01 secondary-activation emulator contract mismatch.' }

$battleDataPath = Join-Path $repoRoot 'local\derived\h3\battle01-secondary-activation-static.json'
& (Join-Path $PSScriptRoot 'Export-Battle01Data.ps1') -UpstreamPath $UpstreamPath -OutputPath $battleDataPath
$battle = Get-Content -Raw -Encoding utf8 -LiteralPath $battleDataPath | ConvertFrom-Json
$sourceEnemy = @($battle.entities | Where-Object { $_.kind -eq 'enemy' })[0]
if ([int] $sourceEnemy.behavior.primaryRegion -ne [int] $fixture.setup.enemy.naturalPrimaryRegion -or
    [int] $sourceEnemy.behavior.secondaryRegion -ne [int] $fixture.setup.enemy.naturalSecondaryRegion) {
    throw 'Static Battle01 secondary-activation source contract drifted.'
}
$packedRegions = (([int] $fixture.setup.enemy.controlledPrimaryRegion -band 15) -shl 4) -bor
    ([int] $fixture.setup.enemy.controlledSecondaryRegion -band 15)
if ($packedRegions -ne [int] $fixture.setup.enemy.controlledTriggerRegions) { throw 'Static secondary trigger-region packing mismatch.' }
$modeBits = 1 -bor 2
$modeledBitfield = ([int] $fixture.setup.enemy.initialActivationBitfield -band (-bnot $modeBits)) -bor $modeBits
if ($modeledBitfield -ne [int] $fixture.expected.enemy.activationBitfield) { throw 'Static secondary activation-bitfield model mismatch.' }

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario activation-secondary -TimeoutSeconds $TimeoutSeconds) -join [Environment]::NewLine
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 Battle01 secondary-activation execution context mismatch.'
}
if ([int] $observed.activation.newlyTriggered -ne [int] $fixture.expected.newlyTriggered -or
    (@($observed.activation.regionFlags) -join ',') -ne (@($fixture.expected.regionFlags) -join ',')) {
    throw 'H3 Battle01 secondary-activation region flags mismatch.'
}
$observedEnemy = @($observed.activation.enemies | Where-Object { [int] $_.combatant -eq [int] $fixture.expected.enemy.combatant })[0]
if ([int] $observedEnemy.triggerRegions -ne [int] $fixture.expected.enemy.triggerRegions -or
    [int] $observedEnemy.bitfield -ne [int] $fixture.expected.enemy.activationBitfield) {
    throw 'H3 Battle01 controlled secondary enemy mismatch.'
}
foreach ($expectedEnemy in $fixture.expected.otherPrimaryOnly) {
    $actual = @($observed.activation.enemies | Where-Object { [int] $_.combatant -eq [int] $expectedEnemy.combatant })[0]
    if ([int] $actual.bitfield -ne [int] $expectedEnemy.activationBitfield) {
        throw "H3 Battle01 primary-only regression for combatant $($expectedEnemy.combatant)."
    }
}

[pscustomobject]@{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    ControlledEnemy = [int] $observedEnemy.combatant
    TriggerRegions = ('0x{0:X2}' -f [int] $observedEnemy.triggerRegions)
    ActivationBitfield = ('0x{0:X4}' -f [int] $observedEnemy.bitfield)
    PrimaryActive = (([int] $observedEnemy.bitfield -band 1) -ne 0)
    SecondaryActive = (([int] $observedEnemy.bitfield -band 2) -ne 0)
    OtherPrimaryOnly = @($fixture.expected.otherPrimaryOnly).Count
    Status = 'PASS'
} | Format-List
