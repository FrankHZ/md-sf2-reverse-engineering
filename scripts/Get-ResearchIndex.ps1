[CmdletBinding()]
param(
    [string] $Query,
    [string] $Subsystem,
    [ValidateSet('confirmed', 'inferred', 'unknown')]
    [string] $Status,
    [string] $Fixture,
    [switch] $Summary,
    [switch] $Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$indexPath = Join-Path $root 'manifests\research-index.json'
$index = Get-Content -Raw -Encoding utf8 -LiteralPath $indexPath | ConvertFrom-Json
$records = @($index.records)

if ($Query) {
    $records = @($records | Where-Object {
        ([string] $_.id).Contains($Query, [System.StringComparison]::OrdinalIgnoreCase) -or
        ([string] $_.symbol).Contains($Query, [System.StringComparison]::OrdinalIgnoreCase) -or
        ([string] $_.subsystem).Contains($Query, [System.StringComparison]::OrdinalIgnoreCase)
    })
}
if ($Subsystem) {
    $records = @($records | Where-Object { ([string] $_.subsystem).StartsWith($Subsystem, [System.StringComparison]::OrdinalIgnoreCase) })
}
if ($Status) {
    $records = @($records | Where-Object status -eq $Status)
}
if ($Fixture) {
    $records = @($records | Where-Object { @($_.evidence.fixture) -match [regex]::Escape($Fixture) })
}

if ($Summary) {
    $allFixtures = @($records.evidence.fixture | Sort-Object -Unique)
    $result = [pscustomobject]@{
        Records = $records.Count
        Confirmed = @($records | Where-Object status -eq 'confirmed').Count
        Inferred = @($records | Where-Object status -eq 'inferred').Count
        Unknown = @($records | Where-Object status -eq 'unknown').Count
        Fixtures = $allFixtures.Count
        Subsystems = @($records.subsystem | Sort-Object -Unique)
    }
    if ($Json) { $result | ConvertTo-Json -Depth 10 } else { $result }
    return
}

$result = @($records | ForEach-Object {
    $entry = @($_.addresses | Where-Object { $_.space -eq 'rom' -and $_.kind -eq 'symbol' })[0]
    [pscustomobject]@{
        Id = $_.id
        Subsystem = $_.subsystem
        Status = $_.status
        Symbol = $_.symbol
        Entry = ('0x{0:X}' -f [int] $entry.value)
        Fixtures = @($_.evidence.fixture | Sort-Object -Unique).Count
        Documents = @($_.documents).Count
    }
})

if ($Json) { $result | ConvertTo-Json -Depth 10 } else { $result }
