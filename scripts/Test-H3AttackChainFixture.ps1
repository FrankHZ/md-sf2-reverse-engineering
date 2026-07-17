[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\attack-chain-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-attack-chain-fixture.schema.json'),
    [int] $TimeoutSeconds = 70
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 attack-chain fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 attack-chain ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 attack-chain emulator contract mismatch.' }

function Invoke-OriginalRng([int] $Seed, [int] $Range) {
    $next = ($Seed * 13 + 7) -band 0xFFFF
    return [pscustomobject] @{ Seed = $next; Result = [int] [Math]::Floor($next * $Range / 65536) }
}

function Get-ModeledAttack($Actor, $Target, [int] $AttackType, [int] $LandEffect, [int] $HpBefore) {
    $dodgeRange = if ([int] $Target.movetype -in @(5, 6) -and [int] $Actor.movetype -notin @(4, 8, 9, 10)) { 8 } else { 32 }
    $dodge = Invoke-OriginalRng ([int] $fixture.setup.dodgeSeed) $dodgeRange
    $raw = [Math]::Max([int] $Actor.attack - [int] $Target.defense, 1)
    $multiplier = switch ($LandEffect) { 0 { 256 } 1 { 230 } default { 205 } }
    $base = [int] [Math]::Floor($raw * $multiplier / 256)
    $preVariance = if ($AttackType -eq 2) { $base -shr 1 } else { $base }
    $range = ($preVariance -shr 3) + 1
    $first = Invoke-OriginalRng $dodge.Seed $range
    $afterFirst = $preVariance - $first.Result
    $second = Invoke-OriginalRng $first.Seed $range
    $final = [Math]::Max($afterFirst - $second.Result, 1)
    return [pscustomobject][ordered] @{
        attackType = $AttackType; actor = [int] $Actor.combatant; target = [int] $Target.combatant
        dodgeRange = $dodgeRange; dodgeRoll = $dodge.Result; baseDamage = $base; inflictEntry = $base
        preVariance = $preVariance; varianceRange = $range; firstRoll = $first.Result; afterFirst = $afterFirst
        secondRoll = $second.Result; finalDamage = $final; hpBefore = $HpBefore; hpAfter = [Math]::Max($HpBefore - $final, 0)
    }
}

$decisionDouble = Invoke-OriginalRng ([int] $fixture.setup.decisionSeed) 4
$decisionCounter = Invoke-OriginalRng $decisionDouble.Seed 4
if ($decisionDouble.Result -ne [int] $fixture.expected.decision.doubleRoll -or
    $decisionCounter.Result -ne [int] $fixture.expected.decision.counterRoll) { throw 'Static double/counter decision model mismatch.' }

$first = Get-ModeledAttack $fixture.setup.actor $fixture.setup.target 0 ([int] $fixture.setup.targetLandEffect) ([int] $fixture.setup.target.hp)
$second = Get-ModeledAttack $fixture.setup.actor $fixture.setup.target 1 ([int] $fixture.setup.targetLandEffect) ([int] $first.hpAfter)
$counter = Get-ModeledAttack $fixture.setup.target $fixture.setup.actor 2 ([int] $fixture.setup.actorLandEffect) ([int] $fixture.setup.actor.hp)
$modeled = @($first, $second, $counter)
$attackFields = @('attackType','actor','target','dodgeRange','dodgeRoll','baseDamage','inflictEntry','preVariance','varianceRange','firstRoll','afterFirst','secondRoll','finalDamage','hpBefore','hpAfter')
for ($index = 0; $index -lt 3; $index++) {
    foreach ($field in $attackFields) {
        if ([int] $modeled[$index].$field -ne [int] $fixture.expected.attacks[$index].$field) { throw "Static attack-chain mismatch at attack $index field $field." }
    }
    $expectedReaction = $fixture.expected.reactions[$index]
    if ([int] $expectedReaction.hpChange -ne -[int] $modeled[$index].finalDamage -or
        [int] $expectedReaction.hpAfter -ne [int] $modeled[$index].hpAfter) { throw "Static reaction mismatch at index $index." }
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario chain -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 attack-chain execution context mismatch.'
}
foreach ($field in @('doubleRoll','counterRoll','double','counter')) {
    if ([string] $observed.decision.$field -ne [string] $fixture.expected.decision.$field) { throw "H3 attack decision mismatch: $field." }
}
foreach ($field in @('allyHp','enemyHp')) {
    if ([int] $observed.restored.$field -ne [int] $fixture.expected.restored.$field) { throw "H3 attack-chain restoration mismatch: $field." }
}
if (@($observed.attacks).Count -ne 3 -or @($observed.reactions).Count -ne 3) { throw 'H3 attack-chain event count mismatch.' }
for ($index = 0; $index -lt 3; $index++) {
    foreach ($field in $attackFields) {
        if ([int] $observed.attacks[$index].$field -ne [int] $fixture.expected.attacks[$index].$field) { throw "H3 attack-chain mismatch at attack $index field $field." }
    }
    foreach ($field in @('kind','combatant','hpChange','hpAfter')) {
        if ([string] $observed.reactions[$index].$field -ne [string] $fixture.expected.reactions[$index].$field) { throw "H3 attack reaction mismatch at index $index field $field." }
    }
}

[pscustomobject] @{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    DodgeRolls = (@($observed.attacks | ForEach-Object { "$($_.dodgeRoll)/$($_.dodgeRange)" }) -join ', ')
    Double = [bool] $observed.decision.double
    Counter = [bool] $observed.decision.counter
    AttackTypes = (@($observed.attacks.attackType) -join ',')
    Damage = (@($observed.attacks.finalDamage) -join ',')
    PersistentHp = "ally=$($observed.reactions[2].hpAfter), enemy=$($observed.reactions[1].hpAfter)"
    Status = 'PASS'
} | Format-List
