[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\growth-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-AsmInteger([string] $Value) {
    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') { return [Convert]::ToInt32($Matches[1], 16) }
    if ($trimmed -match '^\d+$') { return [int] $trimmed }
    throw "Unsupported assembly integer: $Value"
}

function Get-Argument([string[]] $Lines, [string] $Keyword) {
    $row = @($Lines | Where-Object { $_ -match "^\s*$([regex]::Escape($Keyword))\s+" })
    if ($row.Count -ne 1) { throw "Expected one '$Keyword' row, found $($row.Count)." }
    return (($row[0] -replace '^\s*\S+\s+', '') -replace '\s*;.*$', '').Trim()
}

function Parse-Growth([string] $Expression) {
    $parts = @($Expression.Split(',') | ForEach-Object { $_.Trim() })
    if ($parts.Count -ne 3) { throw "Invalid growth expression: $Expression" }
    return [pscustomobject][ordered] @{ start = Convert-AsmInteger $parts[0]; projected = Convert-AsmInteger $parts[1]; curve = $parts[2] }
}

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$disasm = Join-Path $resolvedUpstream 'disasm'
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$commit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $commit -ne [string] $toolchain.sf2disasm.commit) { throw 'Growth extraction requires the pinned SF2DISASM commit.' }

$curvePath = Join-Path $disasm 'data\stats\allies\growthcurves.asm'
$curveLines = [string[]] (Get-Content -LiteralPath $curvePath -Encoding utf8)
$curveOrder = @('LINEAR', 'LATE', 'EARLY', 'MIDDLE', 'EARLYANDLATE')
$curves = [System.Collections.Generic.List[object]]::new()
$current = -1
foreach ($line in $curveLines) {
    if ($line -match '^\s*;\s*(Linear|Late|Early|Middle|Early and late)\s*$') {
        $current++
        continue
    }
    if ($current -lt 0 -or $line -notmatch '^\s*dc\.w\s+(\d+)\s*,\s*(\d+)\s*;\s*level\s+(\d+)') { continue }
    if ($curves.Count -le $current) {
        $curves.Add([pscustomobject][ordered] @{ id = $current + 1; code = $curveOrder[$current]; levels = [System.Collections.Generic.List[object]]::new() })
    }
    $curves[$current].levels.Add([pscustomobject][ordered] @{ level = [int] $Matches[3]; total256 = [int] $Matches[1]; gain256 = [int] $Matches[2] })
}
if ($curves.Count -ne 5) { throw "Expected five growth curves, found $($curves.Count)" }
foreach ($curve in $curves) {
    if ($curve.levels.Count -ne 29 -or $curve.levels[-1].level -ne 30 -or $curve.levels[-1].total256 -ne 256) {
        throw "Invalid curve shape: $($curve.code)"
    }
    $curve.levels = $curve.levels.ToArray()
}

$allyNamesPath = Join-Path $disasm 'data\stats\allies\allynames.asm'
$allyNames = [System.Collections.Generic.List[string]]::new()
foreach ($line in (Get-Content -LiteralPath $allyNamesPath -Encoding utf8)) {
    if ($line -match '(?:^|:)\s*allyName\s+"([^"]+)"') { $allyNames.Add($Matches[1]) }
}
if ($allyNames.Count -ne 30) { throw "Expected 30 ally names, found $($allyNames.Count)" }

$allies = [System.Collections.Generic.List[object]]::new()
$sourceRefs = [System.Collections.Generic.List[object]]::new()
$sourceRefs.Add([pscustomobject][ordered] @{ path = 'data/stats/allies/growthcurves.asm'; sha256 = (Get-FileHash -LiteralPath $curvePath -Algorithm SHA256).Hash })
$sourceRefs.Add([pscustomobject][ordered] @{ path = 'data/stats/allies/allynames.asm'; sha256 = (Get-FileHash -LiteralPath $allyNamesPath -Algorithm SHA256).Hash })
for ($allyId = 0; $allyId -lt 30; $allyId++) {
    $relative = 'data/stats/allies/stats/allystats{0:D2}.asm' -f $allyId
    $path = Join-Path $disasm ($relative.Replace('/', '\'))
    $lines = [string[]] (Get-Content -LiteralPath $path -Encoding utf8)
    $sourceRefs.Add([pscustomobject][ordered] @{ path = $relative; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash })
    $classes = [System.Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -notmatch '(?:^|:)\s*forClass\s+([A-Z0-9_]+)') { continue }
        $classCode = $Matches[1]
        $end = $index + 1
        while ($end -lt $lines.Count -and $lines[$end] -notmatch '(?:^|:)\s*forClass\s+') { $end++ }
        $block = [string[]] $lines[$index..($end - 1)]
        $inherit = @($block | Where-Object { $_ -match '^\s*useFirstSpellList' }).Count -eq 1
        $spells = [System.Collections.Generic.List[object]]::new()
        if (-not $inherit) {
            $spellStart = -1
            for ($lineIndex = 0; $lineIndex -lt $block.Count; $lineIndex++) {
                if ($block[$lineIndex] -match '^\s*spellList(?:\s|$)') { $spellStart = $lineIndex; break }
            }
            if ($spellStart -lt 0) { throw "Missing spell list for ally $allyId class $classCode" }
            $spellText = $block[$spellStart..($block.Count - 1)] -join ' '
            foreach ($match in [regex]::Matches($spellText, '(\d+)\s*,\s*([A-Z0-9_]+(?:\|LV[1-4])?)')) {
                $expression = $match.Groups[2].Value
                $tokens = $expression.Split('|')
                $level = if ($tokens.Count -gt 1) { [int] $tokens[1].Substring(2) } else { 1 }
                $spells.Add([pscustomobject][ordered] @{ learnLevel = [int] $match.Groups[1].Value; spell = $tokens[0]; spellLevel = $level; expression = $expression })
            }
        }
        $classes.Add([pscustomobject][ordered] @{
            class = $classCode
            stats = [pscustomobject][ordered] @{
                hp = Parse-Growth (Get-Argument $block 'hpGrowth')
                mp = Parse-Growth (Get-Argument $block 'mpGrowth')
                attack = Parse-Growth (Get-Argument $block 'attGrowth')
                defense = Parse-Growth (Get-Argument $block 'defGrowth')
                agility = Parse-Growth (Get-Argument $block 'agiGrowth')
            }
            spellListMode = if ($inherit) { 'inherit-first' } else { 'explicit' }
            spells = $spells.ToArray()
        })
        $index = $end - 1
    }
    if ($classes.Count -lt 1) { throw "No class records for ally $allyId" }
    $allies.Add([pscustomobject][ordered] @{ id = $allyId; code = $allyNames[$allyId]; classes = $classes.ToArray() })
}

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{ repository = [string] $toolchain.sf2disasm.repository; commit = $commit; sources = $sourceRefs.ToArray() }
    growthScale = 256
    growthProjectionLevel = 30
    pointerSlots = [pscustomobject][ordered] @{ count = 32; namedAllies = 30; slots30And31ReuseAlly = 29 }
    curves = $curves.ToArray()
    allies = $allies.ToArray()
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[System.IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [System.Text.UTF8Encoding]::new($false))

$classRecords = @($allies | ForEach-Object { $_.classes }).Count
$spellEntries = @($allies | ForEach-Object { $_.classes } | ForEach-Object { $_.spells }).Count
[pscustomobject] @{ OutputPath = $resolvedOutput; SHA256 = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash; Allies = 30; ClassRecords = $classRecords; SpellEntries = $spellEntries; Status = 'PASS' } | Format-List
