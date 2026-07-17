[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\counter-range-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-counter-range-fixture.schema.json'),
    [int] $TimeoutSeconds = 50
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -Encoding utf8 -LiteralPath $FixturePath
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 counter-range fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 counter-range ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 counter-range emulator contract mismatch.' }

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
$positions = $fixture.setup.validationPositions
$distance = [Math]::Abs([int] $positions.actorX - [int] $positions.targetX) +
    [Math]::Abs([int] $positions.actorY - [int] $positions.targetY)
if ($damage -ne [int] $fixture.expected.firstDamage -or $distance -ne [int] $fixture.expected.distance -or
    ([int] $fixture.setup.target.hp - $damage) -ne [int] $fixture.expected.targetHp) {
    throw 'Static counter-range scenario model mismatch.'
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario counter-range -TimeoutSeconds $TimeoutSeconds) -join [Environment]::NewLine
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 counter-range execution context mismatch.'
}
foreach ($field in @('targetDies', 'distance', 'counterBefore', 'counterAfter', 'damageCalls', 'targetHp')) {
    if ([string] $observed.$field -ne [string] $fixture.expected.$field) {
        throw "H3 counter-range mismatch: $field."
    }
}

[pscustomobject]@{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Distance = [int] $observed.distance
    TargetDies = [bool] $observed.targetDies
    Counter = "$( [bool] $observed.counterBefore )->$( [bool] $observed.counterAfter )"
    DamageCalls = [int] $observed.damageCalls
    TargetHp = [int] $observed.targetHp
    Status = 'PASS'
} | Format-List
