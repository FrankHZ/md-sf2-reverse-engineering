[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\extractions\static-data.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\static-data.schema.json'),
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath $ManifestPath -Encoding utf8 | ConvertFrom-Json
if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot ([string] $manifest.outputPath)
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
$repeatOutput = $resolvedOutput + '.repeat'
& (Join-Path $PSScriptRoot 'Export-StaticData.ps1') -UpstreamPath $UpstreamPath -OutputPath $resolvedOutput

$json = Get-Content -Raw -LiteralPath $resolvedOutput -Encoding utf8
if (-not ($json | Test-Json -SchemaFile $SchemaPath)) {
    throw "Static extraction failed schema validation: $resolvedOutput"
}
$data = $json | ConvertFrom-Json
$actualHash = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash
if ($actualHash -ne [string] $manifest.outputSha256) {
    throw "Static extraction hash mismatch: expected $($manifest.outputSha256), got $actualHash"
}
if ($data.provenance.commit -ne [string] $manifest.upstreamCommit) {
    throw 'Static extraction upstream commit mismatch.'
}

$namedAllies = @($data.allies | Where-Object { $null -ne $_.code })
$actualCounts = [ordered] @{
    allySlots = @($data.allies).Count
    namedAllies = $namedAllies.Count
    classes = @($data.classes).Count
    items = @($data.items).Count
    spellNames = @($data.spellNames).Count
    spellDefinitions = @($data.spellDefinitions).Count
}
foreach ($key in $actualCounts.Keys) {
    if ($actualCounts[$key] -ne [int] $manifest.counts.$key) {
        throw "Static extraction count mismatch for ${key}: expected $($manifest.counts.$key), got $($actualCounts[$key])"
    }
}

$classCodes = @($data.classes.code)
$itemCodes = @($data.items.code)
$spellCodes = @($data.spellNames.code)
foreach ($ally in $data.allies) {
    if ($classCodes -notcontains $ally.startClass) { throw "Unknown ally class reference: $($ally.startClass)" }
    foreach ($item in $ally.startItems) {
        if ($itemCodes -notcontains $item.item) { throw "Unknown ally item reference: $($item.item)" }
    }
}
foreach ($item in $data.items) {
    if ($item.useSpell -ne 'NOTHING' -and $spellCodes -notcontains $item.useSpell) {
        throw "Unknown item spell reference: $($item.useSpell)"
    }
}
foreach ($spell in $data.spellDefinitions) {
    if ($spellCodes -notcontains $spell.spell) { throw "Unknown spell definition reference: $($spell.spell)" }
}

foreach ($key in $manifest.fixedRecordSizes.psobject.Properties.Name) {
    $actualSize = [int] $data.romRanges.$key.recordSizeBytes
    if ($actualSize -ne [int] $manifest.fixedRecordSizes.$key) {
        throw "Static record size mismatch for ${key}: expected $($manifest.fixedRecordSizes.$key), got $actualSize"
    }
}

$repeatPassed = $false
try {
    & (Join-Path $PSScriptRoot 'Export-StaticData.ps1') -UpstreamPath $UpstreamPath -OutputPath $repeatOutput
    $repeatHash = (Get-FileHash -LiteralPath $repeatOutput -Algorithm SHA256).Hash
    if ($repeatHash -ne $actualHash) {
        throw "Static extraction is not deterministic: $actualHash vs $repeatHash"
    }
    $repeatPassed = $true
}
finally {
    if ($repeatPassed -and (Test-Path -LiteralPath $repeatOutput)) {
        $outputDirectory = [System.IO.Path]::GetFullPath((Split-Path -Parent $resolvedOutput)).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
        $normalizedRepeat = [System.IO.Path]::GetFullPath($repeatOutput)
        if (-not $normalizedRepeat.StartsWith(
            $outputDirectory + [System.IO.Path]::DirectorySeparatorChar,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            throw "Refusing to remove repeat output outside extraction directory: $normalizedRepeat"
        }
        Remove-Item -LiteralPath $normalizedRepeat -Force
    }
}

[pscustomobject] @{
    OutputPath = $resolvedOutput
    SHA256 = $actualHash
    AllySlots = $actualCounts.allySlots
    Classes = $actualCounts.classes
    Items = $actualCounts.items
    SpellDefinitions = $actualCounts.spellDefinitions
    Deterministic = $true
    Status = 'PASS'
} | Format-List
