[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\extractions\growth-data.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\growth-data.schema.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath $ManifestPath -Encoding utf8 | ConvertFrom-Json
$output = Join-Path $repoRoot ([string] $manifest.outputPath)
$repeat = $output + '.repeat'

& (Join-Path $PSScriptRoot 'Export-GrowthData.ps1') -UpstreamPath $UpstreamPath -OutputPath $output
$json = Get-Content -Raw -LiteralPath $output -Encoding utf8
if (-not ($json | Test-Json -SchemaFile $SchemaPath)) { throw 'Growth extraction failed schema validation.' }
$data = $json | ConvertFrom-Json
$hash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
if ($hash -ne [string] $manifest.outputSha256) { throw "Growth extraction hash mismatch: expected $($manifest.outputSha256), got $hash" }
if ($data.provenance.commit -ne [string] $manifest.upstreamCommit) { throw 'Growth extraction upstream commit mismatch.' }

$classRecords = @($data.allies | ForEach-Object { $_.classes })
$spellEntries = @($classRecords | ForEach-Object { $_.spells })
$actual = @{ curves = @($data.curves).Count; allies = @($data.allies).Count; classRecords = $classRecords.Count; spellEntries = $spellEntries.Count }
foreach ($key in $actual.Keys) {
    if ($actual[$key] -ne [int] $manifest.counts.$key) { throw "Growth count mismatch for ${key}: $($actual[$key])" }
}
foreach ($curve in $data.curves) {
    if (@($curve.levels).Count -ne [int] $manifest.counts.levelsPerCurve) { throw "Growth level count mismatch: $($curve.code)" }
    $previous = 0
    foreach ($level in $curve.levels) {
        if ($level.total256 - $previous -ne $level.gain256) { throw "Growth cumulative invariant failed: $($curve.code) level $($level.level)" }
        $previous = $level.total256
    }
}

$staticPath = Join-Path $repoRoot 'local\derived\static-data.json'
if (-not (Test-Path -LiteralPath $staticPath)) { & (Join-Path $PSScriptRoot 'Export-StaticData.ps1') -UpstreamPath $UpstreamPath -OutputPath $staticPath }
$static = Get-Content -Raw -LiteralPath $staticPath -Encoding utf8 | ConvertFrom-Json
$classCodes = @($static.classes.code)
$spellCodes = @($static.spellNames.code)
foreach ($ally in $data.allies) {
    if ($ally.classes[0].spellListMode -ne 'explicit') { throw "First class record must own spell list: $($ally.code)" }
    foreach ($class in $ally.classes) {
        if ($classCodes -notcontains $class.class) { throw "Unknown growth class: $($class.class)" }
        foreach ($spell in $class.spells) { if ($spellCodes -notcontains $spell.spell) { throw "Unknown learned spell: $($spell.spell)" } }
    }
}

$repeatOk = $false
try {
    & (Join-Path $PSScriptRoot 'Export-GrowthData.ps1') -UpstreamPath $UpstreamPath -OutputPath $repeat
    if ((Get-FileHash -LiteralPath $repeat -Algorithm SHA256).Hash -ne $hash) { throw 'Growth extraction is not deterministic.' }
    $repeatOk = $true
}
finally {
    if ($repeatOk -and (Test-Path -LiteralPath $repeat)) { Remove-Item -LiteralPath $repeat -Force }
}

[pscustomobject] @{ OutputPath = $output; SHA256 = $hash; Curves = 5; Allies = 30; ClassRecords = $classRecords.Count; SpellEntries = $spellEntries.Count; Deterministic = $true; Status = 'PASS' } | Format-List
