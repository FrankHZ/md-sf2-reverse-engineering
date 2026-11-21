#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repo,
    [Parameter(Mandatory)][ValidateSet('Prepare', 'Assemble', 'Status', 'Resolve')][string]$Operation,
    [Parameter(Mandatory)][string]$Library,
    [string]$Package,
    [string]$Destination,
    [string[]]$Family = @('All'),
    [string]$Id,
    [int]$BudgetMiB = 1024
)

$ErrorActionPreference = 'Stop'
if (![IO.Path]::IsPathFullyQualified($Repo)) { throw 'Select an absolute -Repo' }
$repoRoot = (Resolve-Path -LiteralPath $Repo).Path
$configuration = Join-Path $repoRoot 'local/private-inputs.ps1'
if (!(Test-Path -LiteralPath $configuration -PathType Leaf)) {
    $template = & git -C $repoRoot config --path --get sf2.privateInputsTemplate
    if ($LASTEXITCODE -or !(Test-Path -LiteralPath $template -PathType Leaf)) {
        throw 'Missing registered private input template'
    }
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $configuration))
    Copy-Item -LiteralPath $template -Destination $configuration
}

$selection = @($Operation.ToLowerInvariant(), '--library', $Library)
switch ($Operation) {
    'Prepare' {
        if (!$Package) { throw 'Prepare requires -Package' }
        $selection += @('--package', $Package, '--budget-mib', $BudgetMiB.ToString())
        foreach ($name in $Family) { $selection += @('--family', $name.ToLowerInvariant()) }
    }
    'Assemble' {
        if (!$Destination) { throw 'Assemble requires -Destination' }
        $selection += @('--destination', $Destination, '--budget-mib', $BudgetMiB.ToString())
    }
    'Resolve' {
        if ($Family.Count -ne 1 -or !$Id) { throw 'Resolve requires one -Family and an -Id' }
        $selection += @('--family', $Family[0].ToLowerInvariant(), '--id', $Id)
    }
}
Push-Location -LiteralPath $repoRoot
try {
    . $configuration
    $env:UV_CACHE_DIR = Join-Path $repoRoot 'local/uv-cache'
    & uv run --locked python -m sf2tool.remake_private_library @selection
    if ($LASTEXITCODE) { throw "Private asset command failed with exit $LASTEXITCODE" }
} finally {
    Pop-Location
}
