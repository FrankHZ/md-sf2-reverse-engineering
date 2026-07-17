[CmdletBinding()]
param(
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$indexPath = Join-Path $root 'manifests\research-index.json'
$schemaPath = Join-Path $root 'schemas\research-index.schema.json'
$toolchainPath = Join-Path $root 'manifests\toolchain.json'

$indexJson = Get-Content -Raw -Encoding utf8 -LiteralPath $indexPath
if (-not ($indexJson | Test-Json -SchemaFile $schemaPath)) {
    throw 'Research index failed schema validation.'
}

$index = $indexJson | ConvertFrom-Json
$toolchain = Get-Content -Raw -Encoding utf8 -LiteralPath $toolchainPath | ConvertFrom-Json
if ([string] $index.upstream.repository -ne [string] $toolchain.sf2disasm.repository -or
    [string] $index.upstream.commit -ne [string] $toolchain.sf2disasm.commit) {
    throw 'Research index upstream provenance does not match manifests/toolchain.json.'
}

function Resolve-RepositoryPath {
    param([Parameter(Mandatory)][string] $RelativePath)

    if ($RelativePath.Contains('\') -or $RelativePath.StartsWith('/') -or $RelativePath -match '^[A-Za-z]:') {
        throw "Research index paths must use repository-relative forward slashes: $RelativePath"
    }
    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $root ($RelativePath -replace '/', '\')))
    $rootPrefix = [System.IO.Path]::GetFullPath($root).TrimEnd('\') + '\'
    if (-not $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Research index path escapes the repository: $RelativePath"
    }
    return $fullPath
}

function Get-NestedValue {
    param(
        [Parameter(Mandatory)] $Object,
        [Parameter(Mandatory)][string] $PropertyPath
    )

    $value = $Object
    foreach ($segment in $PropertyPath.Split('.')) {
        $property = $value.PSObject.Properties[$segment]
        if ($null -eq $property) {
            throw "Missing fixture field '$PropertyPath'."
        }
        $value = $property.Value
    }
    return $value
}

$recordIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$fixtureIds = @{}
$fixtureObjects = @{}
$fixtureBindings = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$documentPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$contractPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$bindingCount = 0

$resolvedUpstream = [System.IO.Path]::GetFullPath($UpstreamPath)
$sourceRoot = Join-Path $resolvedUpstream ([string] $index.upstream.sourceRoot)
$listingPath = Join-Path $resolvedUpstream (([string] $index.upstream.listingPath) -replace '/', '\')
$hasUpstreamSource = Test-Path -LiteralPath $sourceRoot -PathType Container
$hasListing = Test-Path -LiteralPath $listingPath -PathType Leaf
$listing = if ($hasListing) { Get-Content -Raw -Encoding utf8 -LiteralPath $listingPath } else { $null }

foreach ($record in @($index.records)) {
    if (-not $recordIds.Add([string] $record.id)) {
        throw "Duplicate research record ID: $($record.id)"
    }

    $addressMap = @{}
    $symbolAddresses = @($record.addresses | Where-Object { $_.space -eq 'rom' -and $_.kind -eq 'symbol' })
    if ($symbolAddresses.Count -ne 1) {
        throw "Research record $($record.id) must define exactly one ROM symbol address."
    }
    foreach ($address in @($record.addresses)) {
        if ($addressMap.ContainsKey([string] $address.id)) {
            throw "Duplicate address ID '$($address.id)' in $($record.id)."
        }
        $addressMap[[string] $address.id] = $address
    }

    if ($hasUpstreamSource) {
        $sourcePath = Join-Path $sourceRoot (([string] $record.sourcePath) -replace '/', '\')
        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            throw "Missing indexed upstream source: $($record.sourcePath)"
        }
        $sourceText = Get-Content -Raw -Encoding utf8 -LiteralPath $sourcePath
        $sourcePattern = '(?m)^' + [regex]::Escape([string] $record.symbol) + ':\s*$'
        if ($sourceText -notmatch $sourcePattern) {
            throw "Indexed symbol $($record.symbol) is absent from $($record.sourcePath)."
        }
    }

    if ($hasListing) {
        $listingPattern = '(?m)^(?<address>[0-9A-F]{8})\s+' + [regex]::Escape([string] $record.symbol) + ':\s*$'
        $match = [regex]::Match($listing, $listingPattern)
        if (-not $match.Success) {
            throw "Indexed symbol $($record.symbol) is absent from the H1 assembler listing."
        }
        $listingAddress = [Convert]::ToInt32($match.Groups['address'].Value, 16)
        if ($listingAddress -ne [int] $symbolAddresses[0].value) {
            $indexedHex = ([int] $symbolAddresses[0].value).ToString('X')
            throw "H1 listing address drift for $($record.symbol): index 0x$indexedHex, listing 0x$($listingAddress.ToString('X'))."
        }
    }

    foreach ($document in @($record.documents)) {
        $documentPath = Resolve-RepositoryPath ([string] $document)
        if (-not (Test-Path -LiteralPath $documentPath -PathType Leaf)) {
            throw "Missing indexed research document: $document"
        }
        [void] $documentPaths.Add([string] $document)
    }
    $designContractsProperty = $record.PSObject.Properties['designContracts']
    $designContracts = if ($null -eq $designContractsProperty) { @() } else { @($designContractsProperty.Value) }
    foreach ($contract in $designContracts) {
        $contractPath = Resolve-RepositoryPath ([string] $contract)
        if (-not (Test-Path -LiteralPath $contractPath -PathType Leaf)) {
            throw "Missing indexed design contract: $contract"
        }
        [void] $contractPaths.Add([string] $contract)
    }

    foreach ($evidence in @($record.evidence)) {
        $fixtureRelative = [string] $evidence.fixture
        $fixturePath = Resolve-RepositoryPath $fixtureRelative
        $verifierPath = Resolve-RepositoryPath ([string] $evidence.verifier)
        if (-not (Test-Path -LiteralPath $fixturePath -PathType Leaf)) {
            throw "Missing indexed fixture: $fixtureRelative"
        }
        if (-not (Test-Path -LiteralPath $verifierPath -PathType Leaf)) {
            throw "Missing indexed verifier: $($evidence.verifier)"
        }

        if (-not $fixtureObjects.ContainsKey($fixtureRelative)) {
            $fixtureObjects[$fixtureRelative] = Get-Content -Raw -Encoding utf8 -LiteralPath $fixturePath | ConvertFrom-Json
        }
        $fixture = $fixtureObjects[$fixtureRelative]
        if ([string] $fixture.id -ne [string] $evidence.fixtureId) {
            throw "Fixture ID drift at $($fixtureRelative): index '$($evidence.fixtureId)', fixture '$($fixture.id)'."
        }
        if ([string] $fixture.romSha256 -ne [string] $index.rom.sha256) {
            throw "ROM identity drift at $fixtureRelative."
        }
        if ($fixtureIds.ContainsKey($fixtureRelative) -and $fixtureIds[$fixtureRelative] -ne [string] $evidence.fixtureId) {
            throw "Conflicting fixture IDs indexed for $fixtureRelative."
        }
        $fixtureIds[$fixtureRelative] = [string] $evidence.fixtureId

        foreach ($binding in @($evidence.bindings)) {
            $addressId = [string] $binding.addressId
            if (-not $addressMap.ContainsKey($addressId)) {
                throw "Binding in $($record.id) refers to missing address ID '$addressId'."
            }
            $fixtureValue = Get-NestedValue -Object $fixture -PropertyPath ([string] $binding.fixtureField)
            if ([int64] $fixtureValue -ne [int64] $addressMap[$addressId].value) {
                throw "Address drift at $($fixtureRelative)::$($binding.fixtureField): index $($addressMap[$addressId].value), fixture $fixtureValue."
            }
            [void] $fixtureBindings.Add("$($fixtureRelative)::$($binding.fixtureField)")
            $bindingCount++
        }
    }
}

$h3FixtureRoot = Join-Path $root 'tests\fixtures\h3'
$h3Fixtures = @(Get-ChildItem -LiteralPath $h3FixtureRoot -Filter '*.json' -File | Sort-Object Name)
foreach ($fixtureFile in $h3Fixtures) {
    $fixtureRelative = 'tests/fixtures/h3/' + $fixtureFile.Name
    if (-not $fixtureIds.ContainsKey($fixtureRelative)) {
        throw "H3 fixture is missing from the research index: $fixtureRelative"
    }
    $fixture = $fixtureObjects[$fixtureRelative]
    foreach ($sectionName in @('function', 'ram')) {
        $section = $fixture.PSObject.Properties[$sectionName]
        if ($null -eq $section -or $null -eq $section.Value) { continue }
        foreach ($property in $section.Value.PSObject.Properties) {
            if ($property.Name.EndsWith('Address', [System.StringComparison]::Ordinal)) {
                $bindingKey = "$($fixtureRelative)::$sectionName.$($property.Name)"
                if (-not $fixtureBindings.Contains($bindingKey)) {
                    throw "Fixture address is not bound by the research index: $bindingKey"
                }
            }
        }
    }
}

[pscustomobject]@{
    Index = 'manifests/research-index.json'
    Records = @($index.records).Count
    Confirmed = @($index.records | Where-Object status -eq 'confirmed').Count
    H3Fixtures = $fixtureIds.Count
    H3FixtureFiles = $h3Fixtures.Count
    AddressBindings = $bindingCount
    ResearchDocuments = $documentPaths.Count
    DesignContracts = $contractPaths.Count
    UpstreamSourcesChecked = $hasUpstreamSource
    H1ListingChecked = $hasListing
    Status = 'PASS'
}
