[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\battle01-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-AsmInteger([string] $Value) {
    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') { return [Convert]::ToInt32($Matches[1], 16) }
    if ($trimmed -match '^-?\d+$') { return [int] $trimmed }
    throw "Unsupported battle01 integer: $Value"
}

function Split-Arguments([string] $Expression, [int] $ExpectedCount) {
    $parts = @($Expression.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
    if ($parts.Count -ne $ExpectedCount) { throw "Expected $ExpectedCount arguments in '$Expression'." }
    return $parts
}

function Get-NextMacro([string[]] $Lines, [int] $Start, [string] $Macro) {
    for ($index = $Start; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match "^\s*$([regex]::Escape($Macro))\s+(.+?)\s*$") {
            return [pscustomobject] @{ Index = $index; Arguments = $Matches[1].Trim() }
        }
        if ($Lines[$index] -match '^\s*(allyCombatant|enemyCombatant)\s+') { break }
    }
    throw "Macro '$Macro' not found after line $Start."
}

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$actualCommit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne [string] $toolchain.sf2disasm.commit) { throw 'Battle01 extraction requires pinned SF2DISASM.' }
$sourceRelative = 'data/battles/spritesets/spriteset01.asm'
$sourcePath = Join-Path $resolvedUpstream "disasm\$($sourceRelative.Replace('/', '\'))"
$lines = [string[]] (Get-Content -LiteralPath $sourcePath -Encoding utf8)

$headerValues = [System.Collections.Generic.List[int]]::new()
foreach ($line in $lines) {
    if ($line -match '^\s*dc\.b\s+(\d+)\s*$') {
        $headerValues.Add([int] $Matches[1])
        if ($headerValues.Count -eq 4) { break }
    }
}
if ($headerValues.Count -ne 4) { throw 'Battle01 header was not found.' }
$allyCount, $enemyCount, $regionCount, $pointCount = $headerValues.ToArray()

$entities = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $lines.Count; $index++) {
    if ($lines[$index] -notmatch '^\s*(allyCombatant|enemyCombatant)\s+(.+?)\s*$') { continue }
    $kind = if ($Matches[1] -eq 'allyCombatant') { 'ally' } else { 'enemy' }
    $identity = Split-Arguments $Matches[2] 3
    $ai = Get-NextMacro $lines ($index + 1) 'combatantAiAndItem'
    $aiArgs = Split-Arguments $ai.Arguments 2
    $behavior = Get-NextMacro $lines ($ai.Index + 1) 'combatantBehavior'
    $behaviorArgs = Split-Arguments $behavior.Arguments 6
    $entities.Add([pscustomobject][ordered] @{
        id = $entities.Count
        kind = $kind
        identityExpression = $identity[0]
        x = Convert-AsmInteger $identity[1]
        y = Convert-AsmInteger $identity[2]
        aiCommandsetExpression = $aiArgs[0]
        itemExpression = $aiArgs[1]
        behavior = [pscustomobject][ordered] @{
            primaryOrderExpression = $behaviorArgs[0]
            primaryRegion = Convert-AsmInteger $behaviorArgs[1]
            secondaryOrderExpression = $behaviorArgs[2]
            secondaryRegion = Convert-AsmInteger $behaviorArgs[3]
            filler = Convert-AsmInteger $behaviorArgs[4]
            spawnExpression = $behaviorArgs[5]
        }
    })
}
if ($entities.Count -ne $allyCount + $enemyCount) { throw "Battle01 entity count mismatch: $($entities.Count)." }

$regionStart = -1
$pointStart = -1
for ($index = 0; $index -lt $lines.Count; $index++) {
    if ($lines[$index] -match '^\s*;\s*AI Regions\s*$') { $regionStart = $index }
    if ($lines[$index] -match '^\s*;\s*AI Points\s*$') { $pointStart = $index }
}
if ($pointStart -lt 0 -and $pointCount -eq 0) { $pointStart = $lines.Count }
if ($regionStart -lt 0 -or $pointStart -lt 0 -or $pointStart -le $regionStart) { throw 'Battle01 AI sections not found.' }
$regionBytes = [System.Collections.Generic.List[int]]::new()
for ($index = $regionStart + 1; $index -lt $pointStart; $index++) {
    if ($lines[$index] -notmatch '^\s*dc\.b\s+(.+?)\s*$') { continue }
    foreach ($part in $Matches[1].Split(',')) { $regionBytes.Add((Convert-AsmInteger $part)) }
}
$cursor = 0
$regions = [System.Collections.Generic.List[object]]::new()
for ($regionId = 0; $regionId -lt $regionCount; $regionId++) {
    $vertices = $regionBytes[$cursor]; $cursor++
    $unknown = $regionBytes[$cursor]; $cursor++
    $points = for ($vertex = 0; $vertex -lt $vertices; $vertex++) {
        $x = $regionBytes[$cursor]; $y = $regionBytes[$cursor + 1]; $cursor += 2
        [pscustomobject][ordered] @{ x = $x; y = $y }
    }
    $regions.Add([pscustomobject][ordered] @{
        id = $regionId
        vertexCount = $vertices
        unknown = $unknown
        vertices = @($points)
        trailingBytes = @($regionBytes[$cursor], $regionBytes[$cursor + 1])
    })
    $cursor += 2
}
if ($cursor -ne $regionBytes.Count) { throw 'Battle01 AI-region byte count mismatch.' }

$aiPoints = [System.Collections.Generic.List[object]]::new()
for ($index = $pointStart + 1; $index -lt $lines.Count; $index++) {
    if ($lines[$index] -notmatch '^\s*dc\.b\s+(.+?)\s*$') { continue }
    $parts = Split-Arguments $Matches[1] 2
    $aiPoints.Add([pscustomobject][ordered] @{ x = Convert-AsmInteger $parts[0]; y = Convert-AsmInteger $parts[1] })
}
if ($aiPoints.Count -ne $pointCount) { throw 'Battle01 AI-point count mismatch.' }

$rangeLine = $lines | Where-Object { $_ -match '0x[0-9A-Fa-f]+\.\.0x[0-9A-Fa-f]+' } | Select-Object -First 1
if ($rangeLine -notmatch '0x([0-9A-Fa-f]+)\.\.0x([0-9A-Fa-f]+)') { throw 'Battle01 ROM range not found.' }
$start = [Convert]::ToInt32($Matches[1], 16)
$end = [Convert]::ToInt32($Matches[2], 16)
$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{
        repository = [string] $toolchain.sf2disasm.repository
        commit = $actualCommit
        sourcePath = $sourceRelative
        sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
    }
    battle = [pscustomobject][ordered] @{ id = 1; code = 'INSIDE_ANCIENT_TOWER' }
    romRange = [pscustomobject][ordered] @{ start = $start; endExclusive = $end; lengthBytes = $end - $start }
    counts = [pscustomobject][ordered] @{ allies = $allyCount; enemies = $enemyCount; aiRegions = $regionCount; aiPoints = $pointCount }
    entities = $entities.ToArray()
    aiRegions = $regions.ToArray()
    aiPoints = $aiPoints.ToArray()
}
$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [Text.UTF8Encoding]::new($false))
[pscustomobject] @{ OutputPath = $resolvedOutput; SHA256 = (Get-FileHash $resolvedOutput -Algorithm SHA256).Hash; Entities = $entities.Count; Regions = $regions.Count; Status = 'PASS' } | Format-List
