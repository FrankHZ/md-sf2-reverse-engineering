[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\physical-damage-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-physical-damage-fixture.schema.json'),
    [string] $ApplicationFixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\physical-damage-application-v1.json'),
    [string] $ApplicationSchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-physical-damage-application-fixture.schema.json'),
    [string] $ReplayFixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\battle-scene-replay-v1.json'),
    [string] $ReplaySchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-battle-scene-replay-fixture.schema.json'),
    [int] $TimeoutSeconds = 55
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 physical-damage fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
$applicationJson = Get-Content -Raw -LiteralPath $ApplicationFixturePath -Encoding utf8
if (-not ($applicationJson | Test-Json -SchemaFile $ApplicationSchemaPath)) { throw 'H3 physical-damage application fixture failed schema validation.' }
$application = $applicationJson | ConvertFrom-Json
$replayJson = Get-Content -Raw -LiteralPath $ReplayFixturePath -Encoding utf8
if (-not ($replayJson | Test-Json -SchemaFile $ReplaySchemaPath)) { throw 'H3 battle-scene replay fixture failed schema validation.' }
$replay = $replayJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 physical-damage ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 physical-damage emulator contract mismatch.' }

$base = [Math]::Max([int] $fixture.setup.attack - [int] $fixture.setup.defense, 1)
$multiplier = switch ([int] $fixture.setup.landEffect) { 0 { 256 } 1 { 230 } default { 205 } }
$reduced = [int] [Math]::Floor($base * $multiplier / 256)
$bonus = [int] [Math]::Floor($reduced / 4)
$result = $reduced + $bonus
if ($base -ne [int] $fixture.expected.base -or $multiplier -ne [int] $fixture.expected.multiplier -or
    $reduced -ne [int] $fixture.expected.reduced -or $result -ne [int] $fixture.expected.result) { throw 'Static physical-damage model mismatch.' }

function Invoke-OriginalRng([int] $Seed, [int] $Range) {
    $next = ($Seed * 13 + 7) -band 0xFFFF
    return [pscustomobject] @{ Seed = $next; Result = [int] [Math]::Floor($next * $Range / 65536) }
}
$criticalRng = Invoke-OriginalRng ([int] $application.setup.criticalSeed) ([int] $application.setup.criticalChanceRange)
$criticalDamage = $result + ($result -shr [int] $application.setup.criticalDamageShift)
$varianceRange = ($criticalDamage -shr 3) + 1
$varianceFirst = Invoke-OriginalRng $criticalRng.Seed $varianceRange
$afterFirst = $criticalDamage - $varianceFirst.Result
$varianceSecond = Invoke-OriginalRng $varianceFirst.Seed $varianceRange
$finalDamage = [Math]::Max($afterFirst - $varianceSecond.Result, 1)
$hpAfter = [Math]::Max([int] $application.setup.targetCurrentHp - $finalDamage, 0)
$killValue = if (([int] $application.setup.actorLevel - [int] $application.setup.targetLevel) -lt 3) { 50 } else { throw 'Fixture level difference left modeled EXP range.' }
$damageExp = [int] [Math]::Floor($killValue * $finalDamage / [int] $application.setup.targetMaxHp)
$damageExp = [Math]::Min($damageExp, [int] $application.setup.perActionExpCap)
$finalExp = [Math]::Min($damageExp + $(if ($hpAfter -eq 0) { $killValue } else { 0 }), [int] $application.setup.perActionExpCap)
if ($criticalRng.Result -ne [int] $application.expected.critical.roll -or $criticalDamage -ne [int] $application.expected.critical.after -or
    $varianceRange -ne [int] $application.expected.variance.range -or $varianceFirst.Result -ne [int] $application.expected.variance.first -or
    $varianceSecond.Result -ne [int] $application.expected.variance.second -or $finalDamage -ne [int] $application.expected.variance.final -or
    $hpAfter -ne [int] $application.expected.hp.after -or $damageExp -ne [int] $application.expected.exp.afterDamage -or
    $finalExp -ne [int] $application.expected.exp.final) { throw 'Static physical-damage application model mismatch.' }

$halvedExp = if ([bool] $replay.setup.halvedBattle) { $finalExp -shr 1 } else { $finalExp }
$expFirst = Invoke-OriginalRng ([int] $replay.setup.startingSeed) ([int] $replay.setup.randomRange)
$awardExp = $halvedExp + $(if ($expFirst.Result -eq 0) { 1 } else { 0 })
$expSecond = Invoke-OriginalRng $expFirst.Seed ([int] $replay.setup.randomRange)
$awardExp -= $(if ($expSecond.Result -eq 0) { 1 } else { 0 })
$awardExp = [Math]::Max($awardExp, 1)
if ($halvedExp -ne [int] $replay.expected.awardCalculation.halved -or
    $expFirst.Result -ne [int] $replay.expected.awardCalculation.first -or
    $expSecond.Result -ne [int] $replay.expected.awardCalculation.second -or
    $awardExp -ne [int] $replay.expected.awardCalculation.final -or
    ([int] $replay.setup.actorInitialExp + $awardExp) -ne [int] $replay.expected.award.actorExpAfter) {
    throw 'Static battle-scene replay model mismatch.'
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario damage -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or $observed.scenario -ne 'damage' -or
    [int] $observed.battle -ne [int] $fixture.battleId -or [int] $observed.actor -ne [int] $fixture.setup.actor -or
    [int] $observed.target -ne [int] $fixture.setup.target) { throw 'H3 physical-damage execution context mismatch.' }
foreach ($field in @('base', 'multiplier', 'reduced', 'result')) {
    if ([int] $observed.$field -ne [int] $fixture.expected.$field) { throw "H3 physical-damage mismatch: $field." }
}
if ([int] $observed.landEffect -ne [int] $fixture.setup.landEffect -or [bool] $observed.archerBonus -ne [bool] $fixture.expected.archerBonus) {
    throw 'H3 physical-damage branch mismatch.'
}
foreach ($field in @('roll', 'before', 'after')) {
    if ([int] $observed.critical.$field -ne [int] $application.expected.critical.$field) { throw "H3 critical-hit mismatch: $field." }
}
if ([int] $observed.critical.seed -ne [int] $application.setup.criticalSeed -or
    [int] $observed.critical.prowess -ne [int] $application.setup.criticalProwess -or
    [int] $observed.critical.range -ne [int] $application.setup.criticalChanceRange -or
    [int] $observed.inflictEntry -ne [int] $application.expected.critical.after) { throw 'H3 critical-hit execution boundary mismatch.' }
foreach ($field in @('range', 'first', 'afterFirst', 'second', 'final')) {
    if ([int] $observed.variance.$field -ne [int] $application.expected.variance.$field) { throw "H3 damage-variance mismatch: $field." }
}
foreach ($field in @('before', 'after', 'targetDies')) {
    if ([string] $observed.hp.$field -ne [string] $application.expected.hp.$field) { throw "H3 HP-application mismatch: $field." }
}
foreach ($field in @('afterDamage', 'final')) {
    if ([int] $observed.exp.$field -ne [int] $application.expected.exp.$field) { throw "H3 damage-EXP mismatch: $field." }
}
if ([int] $observed.hp.restored -ne [int] $replay.expected.restoredHp) { throw 'H3 HP restoration mismatch.' }
if ([int] $observed.awardCalculation.seed -ne [int] $replay.setup.startingSeed) { throw 'H3 final EXP seed boundary mismatch.' }
foreach ($field in @('halved', 'first', 'second', 'final')) {
    if ([int] $observed.awardCalculation.$field -ne [int] $replay.expected.awardCalculation.$field) { throw "H3 final EXP calculation mismatch: $field." }
}
foreach ($field in @('combatant', 'hpChange', 'hpAfter')) {
    if ([int] $observed.reaction.$field -ne [int] $replay.expected.reaction.$field) { throw "H3 persistent HP replay mismatch: $field." }
}
foreach ($field in @('commandExp', 'actorExpBefore', 'actorExpAfter')) {
    if ([int] $observed.award.$field -ne [int] $replay.expected.award.$field) { throw "H3 persistent EXP replay mismatch: $field." }
}
[pscustomobject] @{
    Fixture = [string] $fixture.id; Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    ApplicationFixture = [string] $application.id
    ReplayFixture = [string] $replay.id
    Actor = [int] $observed.actor; Target = [int] $observed.target; BaseDamage = [int] $observed.base
    LandEffect = [int] $observed.landEffect; ReducedDamage = [int] $observed.reduced
    ArcherBonus = [bool] $observed.archerBonus; BaseResult = [int] $observed.result
    CriticalDamage = [int] $observed.critical.after; VarianceDamage = [int] $observed.variance.final
    TargetHp = "$( [int] $observed.hp.before )->$( [int] $observed.hp.after )"; PersistentHp = [int] $observed.reaction.hpAfter
    ExpAccumulator = [int] $observed.exp.final; ExpAwarded = [int] $observed.award.actorExpAfter
    Status = 'PASS'
} | Format-List
