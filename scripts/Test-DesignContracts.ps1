[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$documentPath = Join-Path $root 'docs\design\combat-resolution.md'
$indexPath = Join-Path $root 'docs\README.md'

if (-not (Test-Path -LiteralPath $documentPath -PathType Leaf)) {
    throw "Missing design contract: $documentPath"
}

$document = Get-Content -Raw -Encoding utf8 -LiteralPath $documentPath
$index = Get-Content -Raw -Encoding utf8 -LiteralPath $indexPath
if (-not $index.Contains('./design/combat-resolution.md')) {
    throw 'docs/README.md does not index the physical combat contract.'
}

$references = @(
    @{ Path = 'tests/fixtures/h3/physical-damage-v1.json'; Id = 'sf2-physical-damage-land-archer-v1' }
    @{ Path = 'tests/fixtures/h3/physical-damage-application-v1.json'; Id = 'sf2-physical-damage-application-v1' }
    @{ Path = 'tests/fixtures/h3/battle-scene-replay-v1.json'; Id = 'sf2-battle-scene-replay-v1' }
    @{ Path = 'tests/fixtures/h3/attack-chain-v1.json'; Id = 'sf2-attack-chain-double-counter-v1' }
    @{ Path = 'tests/fixtures/h3/dodge-v1.json'; Id = 'sf2-successful-airborne-dodge-v1' }
    @{ Path = 'tests/fixtures/h3/lethal-followup-v1.json'; Id = 'sf2-lethal-followup-validation-v1' }
    @{ Path = 'tests/fixtures/h3/counter-range-v1.json'; Id = 'sf2-counter-range-validation-v1' }
    @{ Path = 'tests/fixtures/h3/counter-sleep-v1.json'; Id = 'sf2-counter-sleep-validation-v1' }
    @{ Path = 'tests/fixtures/h3/counter-stun-v1.json'; Id = 'sf2-counter-stun-validation-v1' }
    @{ Path = 'tests/fixtures/h3/counter-same-side-v1.json'; Id = 'sf2-counter-same-side-validation-v1' }
    @{ Path = 'tests/fixtures/h3/counter-burst-rock-v1.json'; Id = 'sf2-counter-burst-rock-validation-v1' }
)

foreach ($reference in $references) {
    $fixturePath = Join-Path $root ($reference.Path -replace '/', '\')
    if (-not (Test-Path -LiteralPath $fixturePath -PathType Leaf)) {
        throw "Missing referenced fixture: $($reference.Path)"
    }

    $fixture = Get-Content -Raw -Encoding utf8 -LiteralPath $fixturePath | ConvertFrom-Json
    if ($fixture.id -ne $reference.Id) {
        throw "Fixture ID mismatch at $($reference.Path): expected $($reference.Id), got $($fixture.id)"
    }
    if (-not $document.Contains($reference.Path) -or -not $document.Contains($reference.Id)) {
        throw "Design contract does not trace $($reference.Id) to $($reference.Path)"
    }
}

if ($document -notmatch '\*\*Confirmed' -or $document -notmatch '\*\*Unknown') {
    throw 'Design contract must preserve explicit Confirmed and Unknown evidence labels.'
}

[pscustomobject]@{
    Document = 'docs/design/combat-resolution.md'
    FixtureReferences = $references.Count
    EvidenceLabels = 'Confirmed,Unknown'
    Status = 'PASS'
}
