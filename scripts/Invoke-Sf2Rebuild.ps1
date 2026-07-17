[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [switch] $KeepBuildArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-NativeCaptured {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Executable,

        [string[]] $Arguments = @(),

        [Parameter(Mandatory = $true)]
        [string] $WorkingDirectory
    )

    Push-Location -LiteralPath $WorkingDirectory
    try {
        $output = @(& $Executable @Arguments 2>&1 | ForEach-Object { $_.ToString() })
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if ($exitCode -ne 0) {
        $tail = $output | Select-Object -Last 30
        throw "$Executable failed with exit code $exitCode`n$($tail -join "`n")"
    }

    return ,$output
}

& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
& (Join-Path $PSScriptRoot 'Test-Toolchain.ps1') -UpstreamPath $UpstreamPath

$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$resolvedUpstream = (Resolve-Path -LiteralPath $UpstreamPath).Path
$upstreamRom = Join-Path $resolvedUpstream 'rom\sf2.bin'
$disasmDir = Join-Path $resolvedUpstream 'disasm'
$buildDir = Join-Path $resolvedUpstream 'build'

Copy-Item -LiteralPath $resolvedRom -Destination $upstreamRom -Force

$splitExe = Join-Path $resolvedUpstream 'tools\splitrom.exe'
$splitSpec = Join-Path $resolvedUpstream 'split\sf2splits.txt'
$null = Invoke-NativeCaptured -Executable $splitExe -Arguments @($upstreamRom, $splitSpec) -WorkingDirectory $disasmDir

$asw = Join-Path $resolvedUpstream 'tools\asw\asw.exe'
$p2bin = Join-Path $resolvedUpstream 'tools\asw\p2bin.exe'
$soundDriverDir = Join-Path $disasmDir 'code\common\tech\sound'
$null = Invoke-NativeCaptured -Executable $asw -Arguments @('.\sounddriver.asm') -WorkingDirectory $soundDriverDir
$null = Invoke-NativeCaptured -Executable $p2bin -Arguments @(
    '.\sounddriver.p',
    '..\..\..\..\data\sound\sounddriver.bin',
    '-k',
    '-r',
    '$0000-$1fff'
) -WorkingDirectory $soundDriverDir

foreach ($bank in @('musicbank0', 'musicbank1')) {
    $bankDir = Join-Path $disasmDir "data\sound\$bank"
    $null = Invoke-NativeCaptured -Executable $asw -Arguments @(".\$bank.asm") -WorkingDirectory $bankDir
    $null = Invoke-NativeCaptured -Executable $p2bin -Arguments @(
        ".\$bank.p",
        "..\$bank.bin",
        '-k',
        '-r',
        '$8000-$ffff'
    ) -WorkingDirectory $bankDir
}

$buildName = 'sf2build-h1-{0}-{1}' -f $PID, (Get-Date -Format 'yyyyMMddHHmmssfff')
$outputRom = Join-Path $buildDir "$buildName.bin"
$outputSym = Join-Path $buildDir "$buildName.sym"
$outputLst = Join-Path $buildDir "$buildName.lst"
$outputLog = Join-Path $buildDir "$buildName.log"
$buildArtifacts = @($outputRom, $outputSym, $outputLst, $outputLog)
$buildPassed = $false

try {
    $asm68k = Join-Path $resolvedUpstream 'tools\ASM68K.EXE'
    $asmArguments = @(
        '/e',
        'VANILLA_BUILD=1',
        '/k',
        '/m',
        '/o',
        'ae-,e+,w+',
        '/p',
        'sf2.asm,',
        "../build/$buildName.bin,",
        "../build/$buildName.sym,",
        "../build/$buildName.lst"
    )
    $asmOutput = Invoke-NativeCaptured -Executable $asm68k -Arguments $asmArguments -WorkingDirectory $disasmDir
    $asmOutput | Set-Content -LiteralPath $outputLog -Encoding utf8

    if (-not (Test-Path -LiteralPath $outputRom)) {
        throw "ASM68K did not produce $outputRom"
    }

    $fixHeader = Join-Path $resolvedUpstream 'tools\fixheader.exe'
    $null = Invoke-NativeCaptured -Executable $fixHeader -Arguments @($outputRom) -WorkingDirectory $buildDir

    $fc = (Get-Command fc.exe -ErrorAction Stop).Source
    $compareOutput = @(& $fc '/b' $resolvedRom $outputRom 2>&1 | ForEach-Object { $_.ToString() })
    $compareExitCode = $LASTEXITCODE
    if ($compareExitCode -ne 0) {
        throw "Byte comparison failed with fc.exe exit code $compareExitCode`n$($compareOutput -join "`n")"
    }

    $inputHash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash
    $outputHash = (Get-FileHash -LiteralPath $outputRom -Algorithm SHA256).Hash
    if ($inputHash -ne $outputHash) {
        throw "Rebuild hash mismatch: input $inputHash, output $outputHash"
    }

    $buildPassed = $true
    [pscustomobject] @{
        InputSHA256 = $inputHash
        OutputSHA256 = $outputHash
        SizeBytes = (Get-Item -LiteralPath $outputRom).Length
        BytePerfect = $true
        ArtifactsKept = [bool] $KeepBuildArtifacts
        OutputPath = if ($KeepBuildArtifacts) { $outputRom } else { $null }
        Status = 'PASS'
    } | Format-List
}
finally {
    if ($buildPassed -and -not $KeepBuildArtifacts) {
        $normalizedBuildDir = [System.IO.Path]::GetFullPath($buildDir).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
        foreach ($artifact in $buildArtifacts) {
            $normalizedArtifact = [System.IO.Path]::GetFullPath($artifact)
            if (-not $normalizedArtifact.StartsWith(
                $normalizedBuildDir + [System.IO.Path]::DirectorySeparatorChar,
                [System.StringComparison]::OrdinalIgnoreCase
            )) {
                throw "Refusing to clean artifact outside build directory: $normalizedArtifact"
            }
            if (Test-Path -LiteralPath $normalizedArtifact) {
                Remove-Item -LiteralPath $normalizedArtifact -Force
            }
        }
    }
}
