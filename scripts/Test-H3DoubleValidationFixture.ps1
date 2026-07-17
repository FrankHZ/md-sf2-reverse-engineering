[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\double-validation-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-double-validation-fixture.schema.json'),
    [int] $TimeoutSeconds = 50
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -Encoding utf8 -LiteralPath $FixturePath
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 double-validation fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 double-validation ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 double-validation emulator contract mismatch.' }

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
    throw 'Static double-validation damage model mismatch.'
}

$results = foreach ($case in $fixture.cases) {
    $observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario double-validation -DoubleValidationCase ([string] $case.name) -TimeoutSeconds $TimeoutSeconds) -join [Environment]::NewLine
    $observed = $observedJson | ConvertFrom-Json
    if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
        $observed.scenario -ne [string] $fixture.scenario -or $observed.case -ne [string] $case.name -or
        [int] $observed.battle -ne [int] $fixture.battleId) {
        throw "H3 double-validation execution context mismatch: $($case.name)."
    }
    foreach ($field in @('muddled', 'sameSide')) {
        if ([string] $observed.$field -ne [string] $case.$field) {
            throw "H3 double-validation mismatch: $($case.name).$field."
        }
    }
    foreach ($field in @('targetDies', 'naturalMuddled', 'naturalSameSide', 'doubleBefore', 'doubleAfter', 'damageCalls', 'targetHp')) {
        if ([string] $observed.$field -ne [string] $fixture.expected.$field) {
            throw "H3 double-validation mismatch: $($case.name).$field."
        }
    }
    [string] $case.name
}

[pscustomobject]@{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Cases = $results.Count
    Gates = $results -join ', '
    TargetDies = $false
    Double = 'True->False'
    DamageCallsPerCase = 1
    TargetHp = 182
    Status = 'PASS'
} | Format-List
