[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\local\derived\static-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-AsmInteger {
    param([Parameter(Mandatory = $true)][string] $Value)

    $trimmed = $Value.Trim()
    if ($trimmed -match '^\$([0-9A-Fa-f]+)$') {
        return [Convert]::ToInt32($Matches[1], 16)
    }
    if ($trimmed -match '^0x([0-9A-Fa-f]+)$') {
        return [Convert]::ToInt32($Matches[1], 16)
    }
    if ($trimmed -match '^-?\d+$') {
        return [int] $trimmed
    }

    throw "Unsupported assembly integer: $Value"
}

function Get-Argument {
    param(
        [AllowEmptyString()][string[]] $Lines,
        [Parameter(Mandatory = $true)][string] $Keyword
    )

    $matches = @($Lines | Where-Object { $_ -match "^\s*$([regex]::Escape($Keyword))\s+" })
    if ($matches.Count -ne 1) {
        throw "Expected one '$Keyword' row, found $($matches.Count)."
    }

    return (($matches[0] -replace '^\s*\S+\s+', '') -replace '\s*;.*$', '').Trim()
}

function Split-Expression {
    param([Parameter(Mandatory = $true)][string] $Expression)

    $trimmed = $Expression.Trim()
    if ($trimmed -eq 'NONE') {
        return @()
    }
    return @($trimmed.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
}

function Get-NameRows {
    param(
        [AllowEmptyString()][string[]] $Lines,
        [Parameter(Mandatory = $true)][string] $Macro
    )

    $rows = [System.Collections.Generic.List[object]]::new()
    $pattern = "(?:^|:)\s*$([regex]::Escape($Macro))\s+(.+)$"
    foreach ($line in $Lines) {
        $match = [regex]::Match($line, $pattern)
        if (-not $match.Success) { continue }

        $expression = $match.Groups[1].Value.Trim()
        $quotedParts = @([regex]::Matches($expression, '"([^"]*)"') | ForEach-Object { $_.Groups[1].Value })
        $rows.Add([pscustomobject][ordered] @{
            id = $rows.Count
            displayName = $quotedParts -join ' '
            expression = $expression
        })
    }
    return $rows.ToArray()
}

function Get-RomRange {
    param(
        [AllowEmptyString()][string[]] $Lines,
        [Parameter(Mandatory = $true)][string] $SourcePath,
        [Parameter(Mandatory = $true)][int] $RecordCount,
        [bool] $FixedSize
    )

    $rangeLine = $Lines | Where-Object { $_ -match '0x[0-9A-Fa-f]+\.\.0x[0-9A-Fa-f]+' } | Select-Object -First 1
    if (-not $rangeLine -or $rangeLine -notmatch '0x([0-9A-Fa-f]+)\.\.0x([0-9A-Fa-f]+)\s*:\s*(.+)$') {
        throw "ROM range header not found in $SourcePath"
    }

    $start = [Convert]::ToInt32($Matches[1], 16)
    $endExclusive = [Convert]::ToInt32($Matches[2], 16)
    $length = $endExclusive - $start
    if ($length -le 0) { throw "Invalid ROM range in $SourcePath" }
    if ($FixedSize -and $length % $RecordCount -ne 0) {
        throw "ROM range length is not divisible by record count in $SourcePath"
    }

    return [pscustomobject][ordered] @{
        sourcePath = $SourcePath.Replace('\', '/')
        description = $Matches[3].Trim()
        start = $start
        endExclusive = $endExclusive
        lengthBytes = $length
        recordCount = $RecordCount
        recordSizeBytes = if ($FixedSize) { [int] ($length / $RecordCount) } else { $null }
    }
}

function Get-CommentBlocks {
    param([AllowEmptyString()][string[]] $Lines)

    $markers = [System.Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^\s*;\s*(\d+):\s*(.+?)\s*$') {
            $markers.Add([pscustomobject] @{ index = $index; id = [int] $Matches[1]; name = $Matches[2].Trim() })
        }
    }

    $blocks = [System.Collections.Generic.List[object]]::new()
    for ($markerIndex = 0; $markerIndex -lt $markers.Count; $markerIndex++) {
        $start = $markers[$markerIndex].index
        $end = if ($markerIndex + 1 -lt $markers.Count) { $markers[$markerIndex + 1].index - 1 } else { $Lines.Count - 1 }
        $blocks.Add([pscustomobject] @{
            id = $markers[$markerIndex].id
            name = $markers[$markerIndex].name
            lines = [string[]] $Lines[$start..$end]
        })
    }
    return $blocks.ToArray()
}

function Get-LevelFromExpression {
    param([Parameter(Mandatory = $true)][string] $Expression)

    $levelToken = Split-Expression $Expression | Where-Object { $_ -match '^LV([1-4])$' } | Select-Object -First 1
    if ($levelToken) {
        return [int] $levelToken.Substring(2)
    }
    return 1
}

function Get-EnumCodes {
    param(
        [AllowEmptyString()][string[]] $Lines,
        [Parameter(Mandatory = $true)][string] $Prefix,
        [Parameter(Mandatory = $true)][int] $ExpectedCount
    )

    $codes = [object[]]::new($ExpectedCount)
    foreach ($line in $Lines) {
        if ($line -notmatch "^$([regex]::Escape($Prefix))_([A-Z0-9_]+):\s+equ\s+([^\s;]+)") { continue }
        try {
            $value = Convert-AsmInteger $Matches[2]
        }
        catch {
            continue
        }
        if ($value -lt 0 -or $value -ge $ExpectedCount -or $null -ne $codes[$value]) { continue }
        $codes[$value] = $Matches[1]
    }

    for ($index = 0; $index -lt $codes.Count; $index++) {
        if ($null -eq $codes[$index]) { throw "Missing $Prefix enum for index $index" }
    }
    return $codes
}

$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$disasmRoot = Join-Path $resolvedUpstream 'disasm'
$toolchainManifest = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$actualCommit = (& git -C $resolvedUpstream rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne [string] $toolchainManifest.sf2disasm.commit) {
    throw 'Static extraction requires the pinned SF2DISASM commit.'
}

$sourcePaths = [ordered] @{
    enums = 'sf2enums.asm'
    allyNames = 'data/stats/allies/allynames.asm'
    allyStartDefinitions = 'data/stats/allies/allystartdefs.asm'
    classNames = 'data/stats/allies/classes/classnames.asm'
    classDefinitions = 'data/stats/allies/classes/classdefs.asm'
    itemNames = 'data/stats/items/itemnames.asm'
    itemDefinitions = 'data/stats/items/itemdefs.asm'
    spellNames = 'data/stats/spells/spellnames.asm'
    spellDefinitions = 'data/stats/spells/spelldefs.asm'
}
$sourceLines = @{}
$sourceRefs = [System.Collections.Generic.List[object]]::new()
foreach ($key in $sourcePaths.Keys) {
    $relativePath = [string] $sourcePaths[$key]
    $absolutePath = Join-Path $disasmRoot $relativePath
    $sourceLines[$key] = [string[]] (Get-Content -LiteralPath $absolutePath -Encoding utf8)
    $sourceRefs.Add([pscustomobject][ordered] @{
        path = $relativePath
        sha256 = (Get-FileHash -LiteralPath $absolutePath -Algorithm SHA256).Hash
    })
}

$allyNames = @(Get-NameRows -Lines $sourceLines.allyNames -Macro 'allyName')
$classNames = @(Get-NameRows -Lines $sourceLines.classNames -Macro 'className')
$itemNames = @(Get-NameRows -Lines $sourceLines.itemNames -Macro 'itemName')
$spellNames = @(Get-NameRows -Lines $sourceLines.spellNames -Macro 'spellName')
if ($allyNames.Count -ne 30 -or $classNames.Count -ne 32 -or $itemNames.Count -ne 128 -or $spellNames.Count -ne 44) {
    throw "Unexpected name counts: allies=$($allyNames.Count), classes=$($classNames.Count), items=$($itemNames.Count), spells=$($spellNames.Count)"
}
$allyCodes = @(Get-EnumCodes -Lines $sourceLines.enums -Prefix 'ALLY' -ExpectedCount 30)
$classCodes = @(Get-EnumCodes -Lines $sourceLines.enums -Prefix 'CLASS' -ExpectedCount 32)
$itemCodes = @(Get-EnumCodes -Lines $sourceLines.enums -Prefix 'ITEM' -ExpectedCount 128)
$spellCodes = @(Get-EnumCodes -Lines $sourceLines.enums -Prefix 'SPELL' -ExpectedCount 44)

$allies = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $sourceLines.allyStartDefinitions.Count; $index++) {
    $line = $sourceLines.allyStartDefinitions[$index]
    if ($line -notmatch '^\s*startClass\s+([^;\s]+)(?:\s*;\s*(\d+):\s*(.+))?') { continue }

    $id = $allies.Count
    if ($Matches[2] -and [int] $Matches[2] -ne $id) { throw "Ally slot order mismatch at $id" }
    $startClass = $Matches[1].Trim()
    $commentName = if ($Matches[3]) { $Matches[3].Trim() } else { $null }
    $blockEnd = $index + 1
    while ($blockEnd -lt $sourceLines.allyStartDefinitions.Count -and
        $sourceLines.allyStartDefinitions[$blockEnd] -notmatch '^\s*startClass\s+') {
        $blockEnd++
    }
    $blockEnd--
    $block = [string[]] $sourceLines.allyStartDefinitions[$index..$blockEnd]
    $startLevel = Convert-AsmInteger (Get-Argument -Lines $block -Keyword 'startLevel')
    $itemsStart = $index + 1
    while ($itemsStart -le $blockEnd -and $sourceLines.allyStartDefinitions[$itemsStart] -notmatch '^\s*startItems\s+') { $itemsStart++ }
    if ($itemsStart -gt $blockEnd) { throw "startItems not found for ally slot $id" }

    $items = [System.Collections.Generic.List[object]]::new()
    for ($itemLine = $itemsStart + 1; $itemLine -le $blockEnd -and $items.Count -lt 4; $itemLine++) {
        $expression = (($sourceLines.allyStartDefinitions[$itemLine] -replace '\s*;.*$', '') -replace '[,&]', '').Trim()
        if (-not $expression) { continue }
        $tokens = @($expression.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
        $items.Add([pscustomobject][ordered] @{
            expression = $expression
            item = $tokens[0]
            equipped = $tokens -contains 'EQUIPPED'
        })
    }
    if ($items.Count -ne 4) { throw "Expected four starting items for ally slot $id" }

    $name = if ($id -lt $allyNames.Count) { $allyNames[$id].displayName } else { $null }
    if ($commentName -and $commentName -ne $name) { throw "Ally name mismatch at slot $id" }
    if ($id -lt $allyCodes.Count -and $name -ne $allyCodes[$id]) { throw "Ally enum mismatch at slot $id" }
    if ($classCodes -notcontains $startClass) { throw "Unknown starting class '$startClass' at ally slot $id" }
    foreach ($item in $items) {
        if ($itemCodes -notcontains $item.item) { throw "Unknown starting item '$($item.item)' at ally slot $id" }
    }
    $allies.Add([pscustomobject][ordered] @{
        id = $id
        code = if ($id -lt $allyCodes.Count) { $allyCodes[$id] } else { $null }
        displayName = $name
        nameExpression = if ($id -lt $allyNames.Count) { $allyNames[$id].expression } else { $null }
        startClass = $startClass
        startLevel = $startLevel
        startItems = $items.ToArray()
    })
}
if ($allies.Count -ne 32) { throw "Expected 32 ally start slots, found $($allies.Count)" }

$classes = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $sourceLines.classDefinitions.Count; $index++) {
    $line = $sourceLines.classDefinitions[$index]
    if ($line -notmatch '^\s*mov\s+(\S+)\s*;\s*(\d+):\s*(\S+)') { continue }

    $id = [int] $Matches[2]
    $code = $Matches[3]
    $movement = Convert-AsmInteger $Matches[1]
    if ($id -ne $classes.Count -or $classNames[$id].displayName -ne $code -or $classCodes[$id] -ne $code) { throw "Class order mismatch at $id" }
    $blockEnd = $index + 1
    while ($blockEnd -lt $sourceLines.classDefinitions.Count -and
        $sourceLines.classDefinitions[$blockEnd] -notmatch '^\s*mov\s+') {
        $blockEnd++
    }
    $blockEnd--
    $block = [string[]] $sourceLines.classDefinitions[$index..$blockEnd]
    $resistance = Get-Argument -Lines $block -Keyword 'resistance'
    $movementType = Get-Argument -Lines $block -Keyword 'movetype'
    $prowess = Get-Argument -Lines $block -Keyword 'prowess'
    $classes.Add([pscustomobject][ordered] @{
        id = $id
        code = $code
        nameExpression = $classNames[$id].expression
        movement = $movement
        resistanceExpression = $resistance
        resistanceTokens = @(Split-Expression $resistance)
        movementType = $movementType
        prowessExpression = $prowess
        prowessTokens = @(Split-Expression $prowess)
    })
}
if ($classes.Count -ne 32) { throw "Expected 32 classes, found $($classes.Count)" }

$items = [System.Collections.Generic.List[object]]::new()
foreach ($block in (Get-CommentBlocks -Lines $sourceLines.itemDefinitions)) {
    if ($block.id -ne $items.Count) { throw "Item order mismatch at $($block.id)" }
    $equipFlags = Get-Argument -Lines $block.lines -Keyword 'equipFlags'
    $rangeParts = (Get-Argument -Lines $block.lines -Keyword 'range').Split(',')
    $itemType = Get-Argument -Lines $block.lines -Keyword 'itemType'
    $useSpell = Get-Argument -Lines $block.lines -Keyword 'useSpell'
    $effectStart = -1
    for ($lineIndex = 0; $lineIndex -lt $block.lines.Count; $lineIndex++) {
        if ($block.lines[$lineIndex] -match '^\s*equipEffects\s+') { $effectStart = $lineIndex; break }
    }
    if ($effectStart -lt 0) { throw "equipEffects not found for item $($block.id)" }
    $effectEnd = [Math]::Min($effectStart + 2, $block.lines.Count - 1)
    $effectText = (($block.lines[$effectStart..$effectEnd] -join ' ') -replace '^.*?equipEffects\s+', '') -replace '&', ''
    $effectMatches = @([regex]::Matches($effectText, '([A-Z0-9_]+)\s*,\s*(-?(?:\$[0-9A-Fa-f]+|\d+))'))
    if ($effectMatches.Count -ne 3) { throw "Expected three equip effects for item $($block.id)" }
    $effects = @($effectMatches | ForEach-Object {
        [pscustomobject][ordered] @{
            type = $_.Groups[1].Value
            parameter = Convert-AsmInteger $_.Groups[2].Value
        }
    })

    $items.Add([pscustomobject][ordered] @{
        id = $block.id
        code = $itemCodes[$block.id]
        displayName = $block.name
        nameExpression = $itemNames[$block.id].expression
        equipFlagsExpression = $equipFlags
        equipFlagTokens = @(Split-Expression $equipFlags)
        range = [pscustomobject][ordered] @{
            min = Convert-AsmInteger $rangeParts[0]
            max = Convert-AsmInteger $rangeParts[1]
        }
        price = Convert-AsmInteger (Get-Argument -Lines $block.lines -Keyword 'price')
        itemTypeExpression = $itemType
        itemTypeTokens = @(Split-Expression $itemType)
        useSpellExpression = $useSpell
        useSpell = (Split-Expression $useSpell | Select-Object -First 1)
        useSpellLevel = Get-LevelFromExpression $useSpell
        equipEffects = $effects
    })
}
if ($items.Count -ne 128) { throw "Expected 128 items, found $($items.Count)" }

$spellNameRecords = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $spellNames.Count; $index++) {
    $spellNameRecords.Add([pscustomobject][ordered] @{
        id = $index
        code = $spellCodes[$index]
        displayName = $spellNames[$index].displayName
        expression = $spellNames[$index].expression
    })
}

$spellDefinitions = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $sourceLines.spellDefinitions.Count; $index++) {
    $line = $sourceLines.spellDefinitions[$index]
    if ($line -notmatch '^\s*entry\s+([^;]+?)\s*;\s*(.+?)\s*$') { continue }
    $entryExpression = $Matches[1].Trim()
    $displayName = $Matches[2].Trim()
    $blockEnd = $index + 1
    while ($blockEnd -lt $sourceLines.spellDefinitions.Count -and
        $sourceLines.spellDefinitions[$blockEnd] -notmatch '^\s*entry\s+') {
        $blockEnd++
    }
    $blockEnd--
    $block = [string[]] $sourceLines.spellDefinitions[$index..$blockEnd]
    $animation = Get-Argument -Lines $block -Keyword 'animation'
    $properties = Get-Argument -Lines $block -Keyword 'properties'
    $rangeParts = (Get-Argument -Lines $block -Keyword 'range').Split(',')
    $entryTokens = @($entryExpression.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
    if ($spellCodes -notcontains $entryTokens[0]) { throw "Unknown spell enum '$($entryTokens[0])'" }
    $spellDefinitions.Add([pscustomobject][ordered] @{
        id = $spellDefinitions.Count
        displayName = $displayName
        entryExpression = $entryExpression
        spell = $entryTokens[0]
        level = Get-LevelFromExpression $entryExpression
        mpCost = Convert-AsmInteger (Get-Argument -Lines $block -Keyword 'mpCost')
        animationExpression = $animation
        animationTokens = @(Split-Expression $animation)
        propertiesExpression = $properties
        propertyTokens = @(Split-Expression $properties)
        range = [pscustomobject][ordered] @{
            min = Convert-AsmInteger $rangeParts[0]
            max = Convert-AsmInteger $rangeParts[1]
        }
        radius = Convert-AsmInteger (Get-Argument -Lines $block -Keyword 'radius')
        power = Convert-AsmInteger (Get-Argument -Lines $block -Keyword 'power')
    })
}
if ($spellDefinitions.Count -ne 89) { throw "Expected 89 spell definitions, found $($spellDefinitions.Count)" }

$romRanges = [ordered] @{
    allyNames = Get-RomRange -Lines $sourceLines.allyNames -SourcePath $sourcePaths.allyNames -RecordCount $allyNames.Count -FixedSize:$false
    allyStartDefinitions = Get-RomRange -Lines $sourceLines.allyStartDefinitions -SourcePath $sourcePaths.allyStartDefinitions -RecordCount $allies.Count -FixedSize:$true
    classNames = Get-RomRange -Lines $sourceLines.classNames -SourcePath $sourcePaths.classNames -RecordCount $classNames.Count -FixedSize:$false
    classDefinitions = Get-RomRange -Lines $sourceLines.classDefinitions -SourcePath $sourcePaths.classDefinitions -RecordCount $classes.Count -FixedSize:$true
    itemNames = Get-RomRange -Lines $sourceLines.itemNames -SourcePath $sourcePaths.itemNames -RecordCount $itemNames.Count -FixedSize:$false
    itemDefinitions = Get-RomRange -Lines $sourceLines.itemDefinitions -SourcePath $sourcePaths.itemDefinitions -RecordCount $items.Count -FixedSize:$true
    spellNames = Get-RomRange -Lines $sourceLines.spellNames -SourcePath $sourcePaths.spellNames -RecordCount $spellNameRecords.Count -FixedSize:$false
    spellDefinitions = Get-RomRange -Lines $sourceLines.spellDefinitions -SourcePath $sourcePaths.spellDefinitions -RecordCount $spellDefinitions.Count -FixedSize:$true
}

$document = [pscustomobject][ordered] @{
    schemaVersion = 1
    provenance = [pscustomobject][ordered] @{
        repository = [string] $toolchainManifest.sf2disasm.repository
        commit = $actualCommit
        sources = $sourceRefs.ToArray()
    }
    romRangeConvention = '[start, endExclusive)'
    romRanges = [pscustomobject] $romRanges
    allies = $allies.ToArray()
    classes = $classes.ToArray()
    items = $items.ToArray()
    spellNames = $spellNameRecords.ToArray()
    spellDefinitions = $spellDefinitions.ToArray()
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
$outputDirectory = Split-Path -Parent $resolvedOutput
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$json = $document | ConvertTo-Json -Depth 20
[System.IO.File]::WriteAllText($resolvedOutput, $json + "`n", [System.Text.UTF8Encoding]::new($false))

[pscustomobject] @{
    OutputPath = $resolvedOutput
    SHA256 = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash
    Allies = $allies.Count
    Classes = $classes.Count
    Items = $items.Count
    SpellNames = $spellNameRecords.Count
    SpellDefinitions = $spellDefinitions.Count
    Status = 'PASS'
} | Format-List
