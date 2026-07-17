[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\turn-order-boundaries-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-turn-order-boundaries-fixture.schema.json'),
    [int] $TimeoutSeconds = 45
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 turn-order boundaries fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 turn-order boundaries ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 turn-order boundaries emulator contract mismatch.' }

function Get-NextRandom([ref] $Seed, [int] $Range) {
    $Seed.Value = (([long] $Seed.Value * 13 + 7) -band 0xFFFF)
    return [int] [Math]::Floor($Seed.Value * $Range / 65536)
}
function Add-TurnEntries([Collections.Generic.List[object]] $Entries, [int] $Combatant, [int] $Agi, [ref] $Seed) {
    $base = $Agi -band 0x7F; $range = $base -shr 3
    $score = $base + (Get-NextRandom $Seed $range) - (Get-NextRandom $Seed $range) + (Get-NextRandom $Seed 3) - 1
    $Entries.Add([pscustomobject] @{ combatant = $Combatant; score = $score -band 0xFF })
    if ($Agi -ge 128) {
        $secondBase = [int] [Math]::Floor($base * 5 / 6); $secondRange = $secondBase -shr 3
        $secondScore = $secondBase + (Get-NextRandom $Seed $secondRange) - (Get-NextRandom $Seed $secondRange)
        $Entries.Add([pscustomobject] @{ combatant = $Combatant; score = $secondScore -band 0xFF })
    }
}
$modelSeed = [int] $fixture.seed
$modelEntries = [Collections.Generic.List[object]]::new()
Add-TurnEntries $modelEntries 0 128 ([ref] $modelSeed)
Add-TurnEntries $modelEntries 1 127 ([ref] $modelSeed)
foreach ($enemy in 129..133) { Add-TurnEntries $modelEntries $enemy 5 ([ref] $modelSeed) }
$indexed = for ($index = 0; $index -lt $modelEntries.Count; $index++) {
    $entry = $modelEntries[$index]
    $signedScore = if ($entry.score -ge 128) { $entry.score - 256 } else { $entry.score }
    [pscustomobject] @{ combatant = $entry.combatant; score = $entry.score; signedScore = $signedScore; sourceIndex = $index }
}
$model = @($indexed | Sort-Object -Property @{ Expression = 'signedScore'; Descending = $true }, @{ Expression = 'sourceIndex'; Ascending = $true })
for ($index = 0; $index -lt $model.Count; $index++) {
    if ([int] $model[$index].combatant -ne [int] $fixture.expectedEntries[$index].combatant -or
        [int] $model[$index].score -ne [int] $fixture.expectedEntries[$index].score) { throw "Static turn-order model mismatch at index $index." }
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Seed ([int] $fixture.seed) -Scenario boundaries -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or $observed.scenario -ne 'boundaries' -or
    [int] $observed.seed -ne [int] $fixture.seed -or [int] $observed.battle -ne [int] $fixture.battleId) { throw 'H3 turn-order boundaries context mismatch.' }
if (@($observed.entries).Count -ne @($fixture.expectedEntries).Count) { throw 'H3 turn-order boundaries entry count mismatch.' }
for ($index = 0; $index -lt @($fixture.expectedEntries).Count; $index++) {
    $expected = $fixture.expectedEntries[$index]; $actual = $observed.entries[$index]
    if ([int] $actual.combatant -ne [int] $expected.combatant -or [int] $actual.score -ne [int] $expected.score) {
        throw "H3 turn-order boundaries mismatch at index $index."
    }
}
if (@($observed.entries | Where-Object combatant -eq 0).Count -ne 2 -or
    @($observed.entries | Where-Object combatant -eq 2).Count -ne 0 -or
    @($observed.entries | Where-Object combatant -eq 128).Count -ne 0) { throw 'H3 turn-order filter/second-turn assertion failed.' }
[pscustomobject] @{
    Fixture = [string] $fixture.id; Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Seed = ('0x{0:X4}' -f [int] $fixture.seed); Entries = @($observed.entries).Count; SecondTurns = 1
    DeadSkipped = 1; UnplacedSkipped = 1; SignedWrappedScores = @($observed.entries | Where-Object score -ge 128).Count
    Status = 'PASS'
} | Format-List
