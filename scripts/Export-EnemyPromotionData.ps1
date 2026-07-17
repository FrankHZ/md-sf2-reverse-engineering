[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\enemy-promotion-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-AsmInteger([string] $Value) {
    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') { return [Convert]::ToInt32($Matches[1], 16) }
    if ($trimmed -match '^0x([0-9A-Fa-f]+)$') { return [Convert]::ToInt32($Matches[1], 16) }
    if ($trimmed -match '^-?\d+$') { return [int] $trimmed }
    throw "Unsupported assembly integer: $Value"
}

function Get-Argument([string[]] $Lines, [string] $Keyword) {
    $matches = @($Lines | Where-Object { $_ -match "^\s*$([regex]::Escape($Keyword))\s+" })
    if ($matches.Count -ne 1) { throw "Expected one '$Keyword' row, found $($matches.Count)." }
    return (($matches[0] -replace '^\s*\S+\s+', '') -replace '\s*;.*$', '').Trim()
}

function Split-Expression([string] $Expression) {
    if ($Expression.Trim() -eq 'NONE') { return @() }
    return @($Expression.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
}

function Get-LevelFromExpression([string] $Expression) {
    $token = Split-Expression $Expression | Where-Object { $_ -match '^LV([1-4])$' } | Select-Object -First 1
    if ($token) { return [int] $token.Substring(2) }
    return 1
}

function Get-EnumCodes([string[]] $Lines, [string] $Prefix, [int] $ExpectedCount) {
    $codes = [object[]]::new($ExpectedCount)
    foreach ($line in $Lines) {
        if ($line -notmatch "^$([regex]::Escape($Prefix))_([A-Z0-9_]+):\s+equ\s+([^\s;]+)") { continue }
        try { $value = Convert-AsmInteger $Matches[2] } catch { continue }
        if ($value -ge 0 -and $value -lt $ExpectedCount -and $null -eq $codes[$value]) { $codes[$value] = $Matches[1] }
    }
    for ($id = 0; $id -lt $ExpectedCount; $id++) {
        if ($null -eq $codes[$id]) { throw "Missing $Prefix enum for index $id" }
    }
    return $codes
}

function Get-ExpressionList([string[]] $Lines, [string] $Keyword, [int] $ExpectedCount) {
    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match "^\s*$([regex]::Escape($Keyword))\s+") { $start = $index; break }
    }
    if ($start -lt 0) { throw "List '$Keyword' was not found." }
    $parts = [System.Collections.Generic.List[string]]::new()
    for ($index = $start; $index -lt $Lines.Count -and $parts.Count -lt $ExpectedCount; $index++) {
        $line = ($Lines[$index] -replace '\s*;.*$', '')
        if ($index -eq $start) { $line = $line -replace "^\s*$([regex]::Escape($Keyword))\s+", '' }
        $line = $line.Replace('&', '').Trim()
        foreach ($part in $line.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries)) {
            if ($part.Trim()) { $parts.Add($part.Trim()) }
        }
    }
    if ($parts.Count -ne $ExpectedCount) { throw "Expected $ExpectedCount values for '$Keyword', found $($parts.Count)." }
    return $parts.ToArray()
}

function Get-PromotionLists([string[]] $Lines) {
    $starts = [System.Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^\s*(promotionSection|promotionItems)\s+') {
            $starts.Add([pscustomobject] @{ Index = $index; Keyword = $Matches[1] })
        }
    }
    if ($starts.Count -ne 5) { throw "Expected five promotion lists, found $($starts.Count)." }
    $lists = [System.Collections.Generic.List[object]]::new()
    for ($listIndex = 0; $listIndex -lt $starts.Count; $listIndex++) {
        $start = $starts[$listIndex].Index
        $end = if ($listIndex + 1 -lt $starts.Count) { $starts[$listIndex + 1].Index - 1 } else { $Lines.Count - 1 }
        $tokens = [System.Collections.Generic.List[string]]::new()
        for ($lineIndex = $start; $lineIndex -le $end; $lineIndex++) {
            $line = ($Lines[$lineIndex] -replace '\s*;.*$', '')
            if ($lineIndex -eq $start) { $line = $line -replace '^\s*(promotionSection|promotionItems)\s+', '' }
            $line = $line.Replace('&', '').Trim()
            foreach ($token in $line.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries)) {
                if ($token.Trim()) { $tokens.Add($token.Trim()) }
            }
        }
        $lists.Add([pscustomobject] @{ Keyword = $starts[$listIndex].Keyword; Tokens = $tokens.ToArray() })
    }
    return $lists.ToArray()
}

function Get-RomRange([string[]] $Lines, [string] $SourcePath, [int] $RecordCount, $RecordSizeBytes) {
    $rangeLine = $Lines | Where-Object { $_ -match '0x[0-9A-Fa-f]+\.\.0x[0-9A-Fa-f]+' } | Select-Object -First 1
    if (-not $rangeLine -or $rangeLine -notmatch '0x([0-9A-Fa-f]+)\.\.0x([0-9A-Fa-f]+)\s*:\s*(.+)$') {
        throw "ROM range header not found in $SourcePath"
    }
    $start = [Convert]::ToInt32($Matches[1], 16)
    $end = [Convert]::ToInt32($Matches[2], 16)
    return [pscustomobject][ordered] @{
        sourcePath = $SourcePath
        description = $Matches[3].Trim()
        start = $start
        endExclusive = $end
        lengthBytes = $end - $start
        recordCount = $RecordCount
        recordSizeBytes = $RecordSizeBytes
    }
}

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$disasmRoot = Join-Path $resolvedUpstream 'disasm'
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$actualCommit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne [string] $toolchain.sf2disasm.commit) {
    throw 'Enemy/promotion extraction requires the pinned SF2DISASM commit.'
}

$sourcePaths = [ordered] @{
    enums = 'sf2enums.asm'
    promotions = 'data/stats/allies/classes/promotions.asm'
    enemyNames = 'data/stats/enemies/enemynames.asm'
    enemyDefinitions = 'data/stats/enemies/enemydefs.asm'
}
$sourceLines = @{}
$sourceRefs = [System.Collections.Generic.List[object]]::new()
foreach ($key in $sourcePaths.Keys) {
    $relative = [string] $sourcePaths[$key]
    $absolute = Join-Path $disasmRoot $relative
    $sourceLines[$key] = [string[]] (Get-Content -LiteralPath $absolute -Encoding utf8)
    $sourceRefs.Add([pscustomobject][ordered] @{ path = $relative; sha256 = (Get-FileHash -LiteralPath $absolute -Algorithm SHA256).Hash })
}

$classCodes = @(Get-EnumCodes $sourceLines.enums 'CLASS' 32)
$itemCodes = @(Get-EnumCodes $sourceLines.enums 'ITEM' 128)
$spellCodes = @(Get-EnumCodes $sourceLines.enums 'SPELL' 44)
$enemyCodes = @(Get-EnumCodes $sourceLines.enums 'ENEMY' 103)

$promotionLists = @(Get-PromotionLists $sourceLines.promotions)
$promotionNames = @('regularBaseClasses', 'regularPromotedClasses', 'specialBaseClasses', 'specialPromotedClasses', 'specialPromotionItems')
$expectedPromotionCounts = @(12, 12, 5, 5, 5)
$promotions = [ordered] @{}
for ($listIndex = 0; $listIndex -lt 5; $listIndex++) {
    $tokens = @($promotionLists[$listIndex].Tokens)
    if ($tokens.Count -ne $expectedPromotionCounts[$listIndex]) {
        throw "Unexpected promotion-list count for $($promotionNames[$listIndex]): $($tokens.Count)"
    }
    $codes = if ($listIndex -eq 4) { $itemCodes } else { $classCodes }
    $promotions[$promotionNames[$listIndex]] = @($tokens | ForEach-Object {
        $id = [Array]::IndexOf($codes, $_)
        if ($id -lt 0) { throw "Unknown promotion token: $_" }
        [pscustomobject][ordered] @{ expression = $_; id = $id; code = $_ }
    })
}

$enemyNames = [System.Collections.Generic.List[object]]::new()
foreach ($line in $sourceLines.enemyNames) {
    if ($line -notmatch '^\s*enemyName\s+(.+?)(?:\s*;.*)?$') { continue }
    $expression = $Matches[1].Trim()
    $nameMatch = [regex]::Match($expression, '^"([^"]*)"')
    if (-not $nameMatch.Success) { throw "Unable to parse enemy name: $expression" }
    $suffix = $expression.Substring($nameMatch.Length).Trim()
    $suffixBytes = @()
    if ($suffix) {
        $suffixBytes = @($suffix.TrimStart(',').Split(',', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { Convert-AsmInteger $_ })
    }
    $enemyNames.Add([pscustomobject][ordered] @{
        id = $enemyNames.Count
        displayName = $nameMatch.Groups[1].Value
        expression = $expression
        encodedLength = $nameMatch.Groups[1].Value.Length + $suffixBytes.Count
        suffixBytes = @($suffixBytes)
    })
}
if ($enemyNames.Count -ne 103) { throw "Expected 103 enemy names, found $($enemyNames.Count)." }

$markers = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $sourceLines.enemyDefinitions.Count; $index++) {
    if ($sourceLines.enemyDefinitions[$index] -match '^\s*unknownByte\s+([^;\s]+)\s*;\s*(\d+):\s*(.+?)\s*$') {
        $markers.Add([pscustomobject] @{ Index = $index; Value = $Matches[1]; Id = [int] $Matches[2]; Name = $Matches[3].Trim() })
    }
}
if ($markers.Count -ne 103) { throw "Expected 103 enemy definition blocks, found $($markers.Count)." }

$enemies = [System.Collections.Generic.List[object]]::new()
for ($markerIndex = 0; $markerIndex -lt $markers.Count; $markerIndex++) {
    $marker = $markers[$markerIndex]
    $end = if ($markerIndex + 1 -lt $markers.Count) { $markers[$markerIndex + 1].Index - 1 } else { $sourceLines.enemyDefinitions.Count - 1 }
    $block = [string[]] $sourceLines.enemyDefinitions[$marker.Index..$end]
    if ($marker.Id -ne $markerIndex -or $marker.Name -ne $enemyNames[$markerIndex].displayName) {
        throw "Enemy name/order mismatch at $markerIndex."
    }
    $items = @(Get-ExpressionList $block 'items' 4 | ForEach-Object {
        $tokens = @(Split-Expression $_)
        [pscustomobject][ordered] @{ expression = $_; item = $tokens[0]; equipped = [bool] ($tokens -contains 'EQUIPPED') }
    })
    $spells = @(Get-ExpressionList $block 'spells' 4 | ForEach-Object {
        $tokens = @(Split-Expression $_)
        [pscustomobject][ordered] @{ expression = $_; spell = $tokens[0]; level = Get-LevelFromExpression $_ }
    })
    foreach ($item in $items) { if ($itemCodes -notcontains $item.item) { throw "Unknown item '$($item.item)' at enemy $markerIndex" } }
    foreach ($spell in $spells) {
        if ($spell.spell -ne 'NOTHING' -and $spellCodes -notcontains $spell.spell) {
            throw "Unknown spell '$($spell.spell)' at enemy $markerIndex"
        }
    }
    $resistance = Get-Argument $block 'baseResistance'
    $prowess = Get-Argument $block 'baseProwess'
    $ai = Get-Argument $block 'aiBitfield'
    $enemies.Add([pscustomobject][ordered] @{
        id = $markerIndex
        code = $enemyCodes[$markerIndex]
        displayName = $enemyNames[$markerIndex].displayName
        nameExpression = $enemyNames[$markerIndex].expression
        nameEncodedLength = $enemyNames[$markerIndex].encodedLength
        nameSuffixBytes = $enemyNames[$markerIndex].suffixBytes
        unknownByte = Convert-AsmInteger $marker.Value
        spellPower = Get-Argument $block 'spellPower'
        level = Convert-AsmInteger (Get-Argument $block 'level')
        maxHp = Convert-AsmInteger (Get-Argument $block 'maxHp')
        maxMp = Convert-AsmInteger (Get-Argument $block 'maxMp')
        baseAttack = Convert-AsmInteger (Get-Argument $block 'baseAtt')
        baseDefense = Convert-AsmInteger (Get-Argument $block 'baseDef')
        baseAgility = Convert-AsmInteger (Get-Argument $block 'baseAgi')
        baseMovement = Convert-AsmInteger (Get-Argument $block 'baseMov')
        resistanceExpression = $resistance
        resistanceTokens = @(Split-Expression $resistance)
        prowessExpression = $prowess
        prowessTokens = @(Split-Expression $prowess)
        items = $items
        spells = $spells
        initialStatusExpression = Get-Argument $block 'initialStatus'
        movementType = Get-Argument $block 'movetype'
        aiBitfieldExpression = $ai
        aiBitfieldTokens = @(Split-Expression $ai)
    })
}

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{
        repository = [string] $toolchain.sf2disasm.repository
        commit = $actualCommit
        sources = $sourceRefs.ToArray()
    }
    romRangeConvention = '[start, endExclusive)'
    romRanges = [pscustomobject][ordered] @{
        promotions = Get-RomRange $sourceLines.promotions $sourcePaths.promotions 5 $null
        enemyNames = Get-RomRange $sourceLines.enemyNames $sourcePaths.enemyNames 103 $null
        enemyDefinitions = Get-RomRange $sourceLines.enemyDefinitions $sourcePaths.enemyDefinitions 103 56
    }
    promotions = [pscustomobject] $promotions
    enemies = $enemies.ToArray()
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[System.IO.File]::WriteAllText($resolvedOutput, ($document | ConvertTo-Json -Depth 20) + "`n", [System.Text.UTF8Encoding]::new($false))

[pscustomobject] @{
    OutputPath = $resolvedOutput
    SHA256 = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash
    PromotionLinks = 17
    SpecialPromotionItems = 5
    Enemies = $enemies.Count
    Status = 'PASS'
} | Format-List
