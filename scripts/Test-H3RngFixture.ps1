[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\rng-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-rng-fixture.schema.json'),
    [string] $ToolchainManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [int] $TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath $ToolchainManifestPath -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 RNG fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json

& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) {
    throw 'H3 RNG fixture ROM mismatch.'
}
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $fixture.emulator.name -ne 'BizHawk' -or
    [string] $fixture.emulator.core -ne [string] $toolchain.bizhawk.core) {
    throw 'H3 RNG fixture execution-engine contract mismatch.'
}

$bizHawkPath = (Resolve-Path -LiteralPath (Join-Path $repoRoot ([string] $toolchain.bizhawk.localExecutablePath))).Path
$derivedDirectory = Join-Path $repoRoot 'local\derived\h3'
$luaPath = Join-Path $derivedDirectory 'rng-fixture.generated.lua'
$outputPath = Join-Path $derivedDirectory 'rng-observed.json'
New-Item -ItemType Directory -Path $derivedDirectory -Force | Out-Null
if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }

function ConvertTo-LuaString([string] $Value) {
    return '"' + $Value.Replace('\', '\\').Replace('"', '\"') + '"'
}

$caseLines = foreach ($case in $fixture.cases) {
    '    {{ id = {0}, seed = {1}, range = {2} }},' -f
        (ConvertTo-LuaString ([string] $case.id)), [int] $case.seed, [int] $case.range
}
$luaOutputPath = $outputPath.Replace('\', '/')
$luaTemplate = @'
local output_path = __OUTPUT_PATH__
local entry_address = __ENTRY_ADDRESS__
local observe_address = __OBSERVE_ADDRESS__
local seed_address = __SEED_ADDRESS__
local cases = {
__CASES__
}

local results = {}
local current = 1
local active = false
local entry_range = nil

local function write_results_and_exit(exit_code)
    local output = assert(io.open(output_path, "w"))
    output:write('{"system":"' .. emu.getsystemid() .. '","core":"Genesis Plus GX","results":[')
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","seed":%d,"range":%d,"entryRange":%d,"observedSeed":%d,"observedValue":%d,"restoredRange":%d}',
            result.id, result.seed, result.range, result.entry_range, result.observed_seed,
            result.observed_value, result.restored_range
        ))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(exit_code)
end

event.on_bus_exec(function()
    if current > #cases or active then return end
    local case = cases[current]
    entry_range = emu.getregister("M68K D6") & 0xFFFF
    memory.write_u16_be(seed_address, case.seed, "M68K BUS")
    active = true
end, entry_address, "sf2-rng-entry", "M68K BUS")

event.on_bus_exec(function()
    if not active then return end
    local case = cases[current]
    results[#results + 1] = {
        id = case.id,
        seed = case.seed,
        range = case.range,
        entry_range = entry_range,
        observed_seed = memory.read_u16_be(seed_address, "M68K BUS"),
        observed_value = emu.getregister("M68K D7") & 0xFFFF,
        restored_range = emu.getregister("M68K D6") & 0xFFFF
    }
    active = false
    current = current + 1
    if current > #cases then write_results_and_exit(0) end
end, observe_address, "sf2-rng-observe", "M68K BUS")

while true do emu.frameadvance() end
'@
$lua = $luaTemplate.
    Replace('__OUTPUT_PATH__', (ConvertTo-LuaString $luaOutputPath)).
    Replace('__ENTRY_ADDRESS__', ([string] [int] $fixture.function.entryAddress)).
    Replace('__OBSERVE_ADDRESS__', ([string] [int] $fixture.function.observeAddress)).
    Replace('__SEED_ADDRESS__', ([string] [int] $fixture.function.seedAddress)).
    Replace('__CASES__', ($caseLines -join "`n"))
Set-Content -LiteralPath $luaPath -Value $lua -Encoding utf8

$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $bizHawkPath
$startInfo.WorkingDirectory = Split-Path -Parent $bizHawkPath
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.ArgumentList.Add("--lua=$luaPath")
$startInfo.ArgumentList.Add((Resolve-Path -LiteralPath $RomPath).Path)

$process = [System.Diagnostics.Process]::new()
$process.StartInfo = $startInfo
try {
    if (-not $process.Start()) { throw 'Unable to start BizHawk.' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $process.Kill($true)
        throw "BizHawk H3 fixture timed out after $TimeoutSeconds seconds."
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) {
        throw "BizHawk H3 fixture failed with exit code $($process.ExitCode).`nSTDOUT:`n$stdout`nSTDERR:`n$stderr"
    }
}
finally {
    $process.Dispose()
}

if (-not (Test-Path -LiteralPath $outputPath)) { throw 'BizHawk did not write the H3 observation file.' }
$observed = Get-Content -Raw -LiteralPath $outputPath -Encoding utf8 | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core) {
    throw 'Unexpected H3 RNG execution system or core.'
}
if (@($observed.results).Count -ne @($fixture.cases).Count) { throw 'H3 RNG observation count mismatch.' }
for ($index = 0; $index -lt @($fixture.cases).Count; $index++) {
    $expected = $fixture.cases[$index]
    $actual = $observed.results[$index]
    if ($actual.id -ne $expected.id -or $actual.seed -ne $expected.seed -or $actual.range -ne $expected.range -or
        $actual.entryRange -ne $expected.range -or $actual.observedSeed -ne $expected.expectedSeed -or
        $actual.observedValue -ne $expected.expectedValue -or $actual.restoredRange -ne $expected.range) {
        throw "H3 RNG mismatch for $($expected.id): expected range/seed/value $($expected.range)/$($expected.expectedSeed)/$($expected.expectedValue), got $($actual.entryRange)/$($actual.observedSeed)/$($actual.observedValue)"
    }
}

[pscustomobject] @{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Cases = @($fixture.cases).Count
    NaturalCallRange = [int] $fixture.cases[0].range
    FunctionEntry = ('0x{0:X}' -f [int] $fixture.function.entryAddress)
    ObserveBefore = ('0x{0:X}' -f [int] $fixture.function.observeAddress)
    Status = 'PASS'
} | Format-List
