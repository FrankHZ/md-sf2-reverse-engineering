[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\counter-burst-rock-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-counter-burst-rock-fixture.schema.json'),
    [int] $TimeoutSeconds = 50
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -Encoding utf8 -LiteralPath $FixturePath
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 counter-burst-rock fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 counter-burst-rock ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 counter-burst-rock emulator contract mismatch.' }

function Invoke-OriginalRng([int] $Seed, [int] $Range) {
    $next = ($Seed * 13 + 7) -band 0xFFFF
    return [pscustomobject] @{ Seed = $next; Result = [int] [Math]::Floor($next * $Range / 65536) }
}
$base = [Math]::Max([int] $fixture.setup.actor.attack - [int] $fixture.setup.target.defense, 1)
$reduced = [int] [Math]::Floor($base * 205 / 256)
$dodge = Invoke-OriginalRng ([int] $fixture.setup.dodgeSeed) 8
$spreadRange = ($reduced -shr 3) + 1
$first = Invoke-OriginalRng $dodge.Seed $spreadRange
$second = Invoke-OriginalRng $first.Seed $spreadRange
$damage = $reduced - $first.Result - $second.Result
if ($damage -ne [int] $fixture.expected.firstDamage -or
    ([int] $fixture.setup.target.hp - $damage) -ne [int] $fixture.expected.targetHp) {
    throw 'Static counter-burst-rock damage model mismatch.'
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario counter-burst-rock -TimeoutSeconds $TimeoutSeconds) -join [Environment]::NewLine
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 counter-burst-rock execution context mismatch.'
}
foreach ($field in @('targetDies', 'naturalEnemy', 'enemy', 'counterBefore', 'counterAfter', 'damageCalls', 'targetHp')) {
    if ([string] $observed.$field -ne [string] $fixture.expected.$field) {
        throw "H3 counter-burst-rock mismatch: $field."
    }
}

[pscustomobject]@{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    NaturalEnemy = [int] $observed.naturalEnemy
    ForcedEnemy = [int] $observed.enemy
    TargetDies = [bool] $observed.targetDies
    Counter = "$( [bool] $observed.counterBefore )->$( [bool] $observed.counterAfter )"
    DamageCalls = [int] $observed.damageCalls
    TargetHp = [int] $observed.targetHp
    Status = 'PASS'
} | Format-List
