[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\dodge-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-dodge-fixture.schema.json'),
    [int] $TimeoutSeconds = 60
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 dodge fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 dodge ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 dodge emulator contract mismatch.' }
function Invoke-OriginalRng([int] $Seed, [int] $Range) {
    $next = ($Seed * 13 + 7) -band 0xFFFF
    return [pscustomobject] @{ Seed = $next; Result = [int] [Math]::Floor($next * $Range / 65536) }
}
$dodge = Invoke-OriginalRng ([int] $fixture.setup.dodgeSeed) 8
$double = Invoke-OriginalRng ([int] $fixture.setup.followupSeed) 32
$counter = Invoke-OriginalRng $double.Seed 32
if ($dodge.Result -ne [int] $fixture.expected.roll -or $double.Result -ne [int] $fixture.expected.doubleRoll -or $counter.Result -ne [int] $fixture.expected.counterRoll) { throw 'Static dodge/follow-up RNG model mismatch.' }
$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario dodge -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or $observed.scenario -ne 'dodge' -or [int] $observed.battle -ne 1 -or [int] $observed.actor -ne 0 -or [int] $observed.target -ne 128) { throw 'H3 dodge execution context mismatch.' }
foreach ($field in @('range','roll','dodge','calculateDamageCalls','doubleRoll','counterRoll','double','counter','actorHp','targetHp')) {
    if ([string] $observed.$field -ne [string] $fixture.expected.$field) { throw "H3 dodge mismatch: $field." }
}
[pscustomobject] @{ Fixture = [string] $fixture.id; Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"; Dodge = "$($observed.roll)/$($observed.range)"; DamageCalls = [int] $observed.calculateDamageCalls; Followup = "double=$($observed.double), counter=$($observed.counter)"; Hp = "$($observed.actorHp)/$($observed.targetHp)"; Status = 'PASS' } | Format-List
