#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repo,
    [ValidateSet('Prepare','Mount','Coverage')][string]$Operation = 'Prepare',
    [string[]]$Family = @('All'),
    [int[]]$Command,
    [string]$Upstream,
    [string]$Listing,
    [string]$Library,
    [string]$Package,
    [string]$Destination,
    [string]$PreparationRoot
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
Push-Location -LiteralPath $repoRoot
try {
    . $configuration
    [void][IO.Directory]::CreateDirectory($env:TEMP)
    if (!$PreparationRoot) { $PreparationRoot = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1' }
    if (![IO.Path]::IsPathFullyQualified($PreparationRoot)) { throw 'Select an absolute -PreparationRoot' }
    if (!$Library) {
        $Library = if ($Operation -eq 'Prepare') {
            Join-Path $repoRoot 'local/retail-prepare/retail-v1'
        } else {
            Join-Path $PreparationRoot 'retail-v1'
        }
    }
    if (!$Package) { $Package = Join-Path $PreparationRoot 'issue1966-original' }
    $selection = @('--root',$Library,'--base',$Package)
    foreach ($name in $Family) { $selection += @('--family',$name.ToLowerInvariant()) }
    foreach ($id in $Command) { $selection += @('--command',$id.ToString()) }
    if ($Operation -eq 'Coverage') {
        if (!$Destination) { throw 'Coverage requires a fresh absolute CSV filename' }
        $selection += @('--coverage',$Destination,'--inventory',(Join-Path $PreparationRoot 'retail-source-and-coverage.csv'))
    } elseif ($Operation -eq 'Mount') {
        if (!$Destination) { throw 'Mount requires a fresh absolute -Destination' }
        $selection += @('--mount',$Destination)
        if ($Upstream) { $selection += @('--upstream',$Upstream) }
    } else {
        if (!$Upstream) { $Upstream = Join-Path (Split-Path -Parent $env:SF2_SHARED_INPUT_ROOT) 'upstream/SF2DISASM' }
        if (!$Listing) { $Listing = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/upstream/SF2DISASM/build/sf2build-h1.lst' }
        $selection += @('--upstream',$Upstream,'--listing',$Listing,'--inventory',(Join-Path $PreparationRoot 'retail-source-and-coverage.csv'))
    }
    & uv run --locked python -m sf2tool.remake_retail_assets @selection
    if ($LASTEXITCODE) { throw "Retail asset preparation failed with exit $LASTEXITCODE" }
} finally {
    Pop-Location
}
