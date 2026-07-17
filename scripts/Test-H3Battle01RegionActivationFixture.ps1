[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\battle01-region-activation-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-battle01-region-activation-fixture.schema.json'),
    [int] $TimeoutSeconds = 45
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 Battle01 activation fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json

& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) { throw 'H3 Battle01 activation ROM mismatch.' }
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $toolchain.bizhawk.core -ne [string] $fixture.emulator.core) { throw 'H3 Battle01 activation emulator contract mismatch.' }

function Get-CrossProduct($A, $B, $Point) {
    return [long] ([long] ($B.x - $A.x) * [long] ($Point.y - $A.y) - [long] ($B.y - $A.y) * [long] ($Point.x - $A.x))
}

function Test-PointInTriangle($Point, $A, $B, $C) {
    $crosses = @(
        (Get-CrossProduct $A $B $Point),
        (Get-CrossProduct $B $C $Point),
        (Get-CrossProduct $C $A $Point)
    )
    $hasNegative = @($crosses | Where-Object { $_ -lt 0 }).Count -gt 0
    $hasPositive = @($crosses | Where-Object { $_ -gt 0 }).Count -gt 0
    return -not ($hasNegative -and $hasPositive)
}

function Test-PointInRegion($Point, $Region) {
    $vertices = @($Region.vertices)
    if ($vertices.Count -eq 3) { return Test-PointInTriangle $Point $vertices[0] $vertices[1] $vertices[2] }
    if ($vertices.Count -ne 4) { throw "Unsupported activation polygon with $($vertices.Count) vertices." }
    return (Test-PointInTriangle $Point $vertices[0] $vertices[1] $vertices[3]) -or
        (Test-PointInTriangle $Point $vertices[2] $vertices[1] $vertices[3])
}

function Get-TriggeredRegions($Battle, $ControlledPoint) {
    $allies = @($Battle.entities | Where-Object { $_.kind -eq 'ally' } | ForEach-Object {
        [pscustomobject] @{ id = [int] $_.id; x = [int] $_.x; y = [int] $_.y }
    })
    if ($null -ne $ControlledPoint) {
        $controlled = $allies | Where-Object { $_.id -eq [int] $fixture.boundarySetup.combatant }
        $controlled.x = [int] $ControlledPoint.x; $controlled.y = [int] $ControlledPoint.y
    }
    return @($Battle.aiRegions | ForEach-Object {
        $region = $_
        [bool] (@($allies | Where-Object { Test-PointInRegion $_ $region }).Count -gt 0)
    })
}

function Assert-Snapshot($Actual, $Expected, [string] $Label) {
    if ([int] $Actual.newlyTriggered -ne [int] $Expected.newlyTriggered) { throw "$Label newly-triggered-region bitfield mismatch." }
    for ($index = 0; $index -lt 3; $index++) {
        if ([bool] $Actual.regionFlags[$index] -ne [bool] $Expected.regionFlags[$index]) { throw "$Label region flag mismatch at index $index." }
    }
    for ($index = 0; $index -lt 6; $index++) {
        $actualEnemy = $Actual.enemies[$index]; $expectedEnemy = $Expected.enemies[$index]
        if ([int] $actualEnemy.combatant -ne [int] $expectedEnemy.combatant -or [int] $actualEnemy.bitfield -ne [int] $expectedEnemy.bitfield) {
            throw "$Label enemy activation mismatch at index $index."
        }
    }
}

$battleDataPath = Join-Path $repoRoot 'local\derived\h3\battle01-activation-static.json'
& (Join-Path $PSScriptRoot 'Export-Battle01Data.ps1') -UpstreamPath $UpstreamPath -OutputPath $battleDataPath
$battle = Get-Content -Raw -LiteralPath $battleDataPath -Encoding utf8 | ConvertFrom-Json
$initialAlly = @($battle.entities | Where-Object { $_.kind -eq 'ally' -and [int] $_.id -eq [int] $fixture.boundarySetup.combatant })[0]
if ([int] $initialAlly.x -ne [int] $fixture.boundarySetup.initial.x -or [int] $initialAlly.y -ne [int] $fixture.boundarySetup.initial.y) {
    throw 'H3 Battle01 activation initial-point contract drifted.'
}
$initialFlags = Get-TriggeredRegions $battle $null
$controlledFlags = Get-TriggeredRegions $battle $fixture.boundarySetup.controlled
for ($index = 0; $index -lt 3; $index++) {
    if ([bool] $initialFlags[$index] -ne [bool] $fixture.expected.initial.regionFlags[$index]) { throw "Static initial-region model mismatch at index $index." }
    if ([bool] $controlledFlags[$index] -ne [bool] $fixture.expected.controlled.regionFlags[$index]) { throw "Static controlled-region model mismatch at index $index." }
}

$sourceEnemies = @($battle.entities | Where-Object { $_.kind -eq 'enemy' })
for ($index = 0; $index -lt 6; $index++) {
    $initial = $fixture.expected.initial.enemies[$index]; $controlled = $fixture.expected.controlled.enemies[$index]
    if ([int] $initial.combatant -ne 128 + $index -or [int] $initial.primaryRegion -ne [int] $sourceEnemies[$index].behavior.primaryRegion) {
        throw "Static enemy-region mapping mismatch at index $index."
    }
    $expectedControlled = [int] $initial.bitfield
    if ($controlledFlags[[int] $initial.primaryRegion]) { $expectedControlled = $expectedControlled -bor 1 }
    if ([int] $controlled.bitfield -ne $expectedControlled) { throw "Static primary-activation model mismatch at index $index." }
}

$observedJson = (& (Join-Path $PSScriptRoot 'Observe-H3Battle01TurnOrder.ps1') -RomPath $RomPath -Scenario activation -TimeoutSeconds $TimeoutSeconds) -join "`n"
$observed = $observedJson | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core -or
    $observed.scenario -ne [string] $fixture.scenario -or [int] $observed.battle -ne [int] $fixture.battleId) {
    throw 'H3 Battle01 activation execution context mismatch.'
}
Assert-Snapshot $observed.activation $fixture.expected.controlled 'H3 Battle01 controlled activation'

[pscustomobject] @{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    InitialPoint = "($($fixture.boundarySetup.initial.x),$($fixture.boundarySetup.initial.y))"
    ControlledPoint = "($($fixture.boundarySetup.controlled.x),$($fixture.boundarySetup.controlled.y))"
    TriggeredRegions = (@($observed.activation.regionFlags) -join ',')
    ActivatedEnemies = @($observed.activation.enemies | Where-Object { ([int] $_.bitfield -band 1) -ne 0 }).Count
    FunctionEntry = ('0x{0:X}' -f [int] $fixture.function.entryAddress)
    Status = 'PASS'
} | Format-List
