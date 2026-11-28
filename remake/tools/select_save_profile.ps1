# Select an existing player profile for an ordinary development or packaged launch.
param([Parameter(Mandatory)][string]$ProfileDirectory)

$ErrorActionPreference = 'Stop'
if (![IO.Path]::IsPathFullyQualified($ProfileDirectory)) { throw 'Select an absolute profile directory.' }
$profileItem = Get-Item -LiteralPath $ProfileDirectory
if (!$profileItem.PSIsContainer -or $profileItem.Name -cnotmatch '^[A-Za-z0-9_-]{1,64}$' -or
    $profileItem.Parent.Name -ne 'saves') { throw 'Select an existing saves/<profile> directory.' }
for ($parent = $profileItem; $parent; $parent = $parent.Parent) {
    if (($parent.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        (Test-Path -LiteralPath (Join-Path $parent.FullName '.git'))) {
        throw 'The profile and its ancestors must be real directories outside Git.'
    }
}
$env:SF2_SAVE_ROOT = $profileItem.Parent.FullName
$env:SF2_SAVE_NAMESPACE = $profileItem.Name
