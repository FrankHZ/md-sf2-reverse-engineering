[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\physical-damage-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-physical-damage-fixture.schema.json'),
    [int] $TimeoutSeconds = 55
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 physical-damage fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
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
[pscustomobject] @{
    Fixture = [string] $fixture.id; Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Actor = [int] $observed.actor; Target = [int] $observed.target; BaseDamage = [int] $observed.base
    LandEffect = [int] $observed.landEffect; ReducedDamage = [int] $observed.reduced
    ArcherBonus = [bool] $observed.archerBonus; Result = [int] $observed.result; Status = 'PASS'
} | Format-List
