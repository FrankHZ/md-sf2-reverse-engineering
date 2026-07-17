[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string] $RomPath,

    [string] $ManifestPath = (Join-Path $PSScriptRoot '..\manifests\roms\sf2-us.json'),

    [switch] $PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-HeaderAscii {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]] $Bytes,

        [Parameter(Mandatory = $true)]
        [int] $Offset,

        [Parameter(Mandatory = $true)]
        [int] $Length
    )

    return ([System.Text.Encoding]::ASCII.GetString($Bytes, $Offset, $Length)).Trim([char] 0, [char] 32)
}

function Get-MegaDriveChecksum {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]] $Bytes
    )

    [uint32] $sum = 0
    for ($offset = 0x200; $offset -lt $Bytes.Length; $offset += 2) {
        [uint16] $word = [uint16] $Bytes[$offset] -shl 8
        if ($offset + 1 -lt $Bytes.Length) {
            $word = $word -bor [uint16] $Bytes[$offset + 1]
        }

        $sum = ($sum + $word) -band 0xFFFF
    }

    return ('{0:X4}' -f $sum)
}

$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$resolvedManifest = (Resolve-Path -LiteralPath $ManifestPath).Path
$schemaPath = Join-Path (Split-Path -Parent $resolvedManifest) '..\..\schemas\rom-manifest.schema.json'
$resolvedSchema = (Resolve-Path -LiteralPath $schemaPath).Path

$manifestJson = Get-Content -Raw -LiteralPath $resolvedManifest -Encoding utf8
if (-not ($manifestJson | Test-Json -SchemaFile $resolvedSchema)) {
    throw "ROM manifest failed schema validation: $resolvedManifest"
}

$manifest = $manifestJson | ConvertFrom-Json
$bytes = [System.IO.File]::ReadAllBytes($resolvedRom)
if ($bytes.Length -lt 0x200) {
    throw "File is too small to contain a Mega Drive ROM header: $resolvedRom"
}

[uint16] $storedChecksum = ([uint16] $bytes[0x18E] -shl 8) -bor [uint16] $bytes[0x18F]
$actual = [ordered] @{
    sizeBytes = $bytes.Length
    sha256 = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
    sha1 = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA1).Hash
    md5 = (Get-FileHash -LiteralPath $resolvedRom -Algorithm MD5).Hash
    console = Read-HeaderAscii -Bytes $bytes -Offset 0x100 -Length 16
    domesticTitle = Read-HeaderAscii -Bytes $bytes -Offset 0x120 -Length 48
    overseasTitle = Read-HeaderAscii -Bytes $bytes -Offset 0x150 -Length 48
    serial = Read-HeaderAscii -Bytes $bytes -Offset 0x180 -Length 14
    storedChecksum = '{0:X4}' -f $storedChecksum
    computedChecksum = Get-MegaDriveChecksum -Bytes $bytes
    regions = Read-HeaderAscii -Bytes $bytes -Offset 0x1F0 -Length 16
}

$expected = [ordered] @{
    sizeBytes = [long] $manifest.sizeBytes
    sha256 = [string] $manifest.hashes.sha256
    sha1 = [string] $manifest.hashes.sha1
    md5 = [string] $manifest.hashes.md5
    console = [string] $manifest.header.console
    domesticTitle = [string] $manifest.header.domesticTitle
    overseasTitle = [string] $manifest.header.overseasTitle
    serial = [string] $manifest.header.serial
    storedChecksum = [string] $manifest.header.checksum
    computedChecksum = [string] $manifest.header.checksum
    regions = [string] $manifest.header.regions
}

$failures = [System.Collections.Generic.List[string]]::new()
foreach ($field in $expected.Keys) {
    if ($actual[$field] -cne $expected[$field]) {
        $failures.Add("${field}: expected '$($expected[$field])', got '$($actual[$field])'")
    }
}

$result = [pscustomobject] @{
    ManifestId = [string] $manifest.id
    RomPath = $resolvedRom
    SizeBytes = $actual.sizeBytes
    SHA256 = $actual.sha256
    HeaderChecksum = $actual.storedChecksum
    ComputedChecksum = $actual.computedChecksum
    Status = if ($failures.Count -eq 0) { 'PASS' } else { 'FAIL' }
}

$result | Format-List

if ($failures.Count -gt 0) {
    $message = "ROM baseline verification failed:`n - " + ($failures -join "`n - ")
    throw $message
}

if ($PassThru) {
    Write-Output $result
}
