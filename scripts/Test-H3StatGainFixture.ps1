[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [string] $UpstreamPath = (Join-Path $PSScriptRoot '..\local\upstream\SF2DISASM'),
    [string] $FixturePath = (Join-Path $PSScriptRoot '..\tests\fixtures\h3\stat-gain-v1.json'),
    [string] $SchemaPath = (Join-Path $PSScriptRoot '..\schemas\h3-stat-gain-fixture.schema.json'),
    [string] $ToolchainManifestPath = (Join-Path $PSScriptRoot '..\manifests\toolchain.json'),
    [int] $TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath $ToolchainManifestPath -Encoding utf8 | ConvertFrom-Json
$fixtureJson = Get-Content -Raw -LiteralPath $FixturePath -Encoding utf8
if (-not ($fixtureJson | Test-Json -SchemaFile $SchemaPath)) { throw 'H3 stat-gain fixture failed schema validation.' }
$fixture = $fixtureJson | ConvertFrom-Json

& (Join-Path $PSScriptRoot 'Test-RomBaseline.ps1') -RomPath $RomPath
if ((Get-FileHash -LiteralPath $RomPath -Algorithm SHA256).Hash -ne [string] $fixture.romSha256) {
    throw 'H3 stat-gain fixture ROM mismatch.'
}
if ([string] $toolchain.bizhawk.release -ne [string] $fixture.emulator.version -or
    [string] $fixture.emulator.name -ne 'BizHawk' -or
    [string] $fixture.emulator.core -ne [string] $toolchain.bizhawk.core) {
    throw 'H3 stat-gain execution-engine contract mismatch.'
}

$growthPath = Join-Path $repoRoot 'local\derived\growth-data.json'
& (Join-Path $PSScriptRoot 'Export-GrowthData.ps1') -UpstreamPath $UpstreamPath -OutputPath $growthPath
$growth = Get-Content -Raw -LiteralPath $growthPath -Encoding utf8 | ConvertFrom-Json
foreach ($case in $fixture.cases) {
    foreach ($rng in $case.rng) {
        $calculatedSeed = ([long] $rng.seed * 13 + 7) -band 0xFFFF
        $calculatedValue = [Math]::Floor($calculatedSeed * 128 / 65536)
        if ($calculatedSeed -ne $rng.expectedSeed -or $calculatedValue -ne $rng.expectedValue) {
            throw "Fixture RNG model mismatch for $($case.id)."
        }
    }
    if ($case.input.curve -eq 0) {
        if ($case.expectedPath -ne 'none' -or @($case.rng).Count -ne 0 -or $case.expectedPity -or $case.expectedGain -ne 0) {
            throw "Invalid no-growth model for $($case.id)."
        }
        continue
    }
    if ($case.expectedPath -ne 'growth' -or @($case.rng).Count -ne 2) { throw "Invalid growth path for $($case.id)." }
    $curveId = [int] $case.input.curve -band 7
    $curve = @($growth.curves | Where-Object { $_.id -eq $curveId })
    if ($curve.Count -ne 1) { throw "Growth curve $curveId not found for $($case.id)." }
    $levelRow = @($curve[0].levels | Where-Object { $_.level -eq ([int] $case.input.level + 1) })
    if ($levelRow.Count -ne 1) { throw "Growth level not found for $($case.id)." }
    $projection = [int] $case.input.projected - [int] $case.input.start
    $randomizedGain = [Math]::Floor(($projection * [int] $levelRow[0].gain256 +
        [int] $case.rng[0].expectedValue - [int] $case.rng[1].expectedValue + 128) / 256)
    $expectedMinimum = [Math]::Floor(($projection * [int] $levelRow[0].total256 + 128) / 256) + [int] $case.input.start
    $pity = ([int] $case.input.current + $randomizedGain) -lt $expectedMinimum
    $modelGain = $randomizedGain + $(if ($pity) { 1 } else { 0 })
    if ($pity -ne [bool] $case.expectedPity -or $modelGain -ne [int] $case.expectedGain) {
        throw "Static stat-gain model mismatch for $($case.id)."
    }
}

$bizHawkPath = (Resolve-Path -LiteralPath (Join-Path $repoRoot ([string] $toolchain.bizhawk.localExecutablePath))).Path
$derivedDirectory = Join-Path $repoRoot 'local\derived\h3'
$luaPath = Join-Path $derivedDirectory 'stat-gain-fixture.generated.lua'
$outputPath = Join-Path $derivedDirectory 'stat-gain-observed.json'
New-Item -ItemType Directory -Path $derivedDirectory -Force | Out-Null
if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }

function ConvertTo-LuaString([string] $Value) {
    return '"' + $Value.Replace('\', '\\').Replace('"', '\"') + '"'
}

$caseLines = foreach ($case in $fixture.cases) {
    $seeds = @($case.rng | ForEach-Object { [string] [int] $_.seed }) -join ', '
    '    {{ id = {0}, seeds = {{ {1} }} }},' -f (ConvertTo-LuaString ([string] $case.id)), $seeds
}
$luaOutputPath = $outputPath.Replace('\', '/')
$luaTemplate = @'
local output_path = __OUTPUT_PATH__
local cases = {
__CASES__
}
local current = 1
local active = nil
local results = {}
local done = false

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function write_results_and_exit()
    if done then return end
    done = true
    local output = assert(io.open(output_path, "w"))
    output:write('{"system":"' .. emu.getsystemid() .. '","core":"Genesis Plus GX","results":[')
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","input":{"current":%d,"curve":%d,"start":%d,"projected":%d,"level":%d},"rng":[',
            result.id, result.current, result.curve, result.start, result.projected, result.level
        ))
        for rng_index, rng in ipairs(result.rng) do
            if rng_index > 1 then output:write(",") end
            output:write(string.format('{"observedSeed":%d,"observedValue":%d}', rng.seed, rng.value))
        end
        output:write(string.format(
            '],"path":"%s","pity":%s,"gain":%d}',
            result.path, tostring(result.pity), result.gain
        ))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    if done or active ~= nil or current > #cases then return end
    active = {
        id = cases[current].id,
        current = reg("D1"), curve = reg("D2"), start = reg("D3"), projected = reg("D4"), level = reg("D5"),
        rng = {}, rng_calls = 0, pity = false
    }
end, __STAT_ENTRY__, "sf2-stat-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.rng_calls = active.rng_calls + 1
    local seed = cases[current].seeds[active.rng_calls]
    if seed ~= nil then memory.write_u16_be(__SEED_ADDRESS__, seed, "M68K BUS") end
end, __RNG_ENTRY__, "sf2-stat-rng-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or active.rng_calls < 1 then return end
    active.rng[active.rng_calls] = {
        seed = memory.read_u16_be(__SEED_ADDRESS__, "M68K BUS"),
        value = reg("D7")
    }
end, __RNG_OBSERVE__, "sf2-stat-rng-observe", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.pity = true end
end, __PITY_ADDRESS__, "sf2-stat-pity", "M68K BUS")

local function finish(path)
    if active == nil then return end
    active.path = path
    active.gain = reg("D1")
    active.rng_calls = nil
    results[#results + 1] = active
    active = nil
    current = current + 1
    if current > #cases then write_results_and_exit() end
end

event.on_bus_exec(function() finish("none") end, __NONE_RETURN__, "sf2-stat-none-return", "M68K BUS")
event.on_bus_exec(function() finish("growth") end, __GROWTH_RETURN__, "sf2-stat-growth-return", "M68K BUS")

while true do emu.frameadvance() end
'@
$lua = $luaTemplate.
    Replace('__OUTPUT_PATH__', (ConvertTo-LuaString $luaOutputPath)).
    Replace('__CASES__', ($caseLines -join "`n")).
    Replace('__STAT_ENTRY__', ([string] [int] $fixture.function.entryAddress)).
    Replace('__NONE_RETURN__', ([string] [int] $fixture.function.noneReturnAddress)).
    Replace('__GROWTH_RETURN__', ([string] [int] $fixture.function.growthReturnAddress)).
    Replace('__PITY_ADDRESS__', ([string] [int] $fixture.function.pityAddress)).
    Replace('__RNG_ENTRY__', ([string] [int] $fixture.function.rngEntryAddress)).
    Replace('__RNG_OBSERVE__', ([string] [int] $fixture.function.rngObserveAddress)).
    Replace('__SEED_ADDRESS__', ([string] [int] $fixture.function.seedAddress))
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
        throw "BizHawk H3 stat-gain fixture timed out after $TimeoutSeconds seconds."
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) {
        throw "BizHawk H3 stat-gain fixture failed with exit code $($process.ExitCode).`nSTDOUT:`n$stdout`nSTDERR:`n$stderr"
    }
}
finally { $process.Dispose() }

if (-not (Test-Path -LiteralPath $outputPath)) { throw 'BizHawk did not write the H3 stat-gain observation file.' }
$observed = Get-Content -Raw -LiteralPath $outputPath -Encoding utf8 | ConvertFrom-Json
if ($observed.system -ne 'GEN' -or $observed.core -ne [string] $fixture.emulator.core) { throw 'Unexpected stat-gain execution system/core.' }
if (@($observed.results).Count -ne @($fixture.cases).Count) { throw 'H3 stat-gain observation count mismatch.' }
for ($index = 0; $index -lt @($fixture.cases).Count; $index++) {
    $expected = $fixture.cases[$index]
    $actual = $observed.results[$index]
    foreach ($field in @('current', 'curve', 'start', 'projected', 'level')) {
        if ([int] $actual.input.$field -ne [int] $expected.input.$field) { throw "H3 stat-gain input mismatch for $($expected.id): $field" }
    }
    if ($actual.id -ne $expected.id -or $actual.path -ne $expected.expectedPath -or
        [bool] $actual.pity -ne [bool] $expected.expectedPity -or [int] $actual.gain -ne [int] $expected.expectedGain -or
        @($actual.rng).Count -ne @($expected.rng).Count) {
        throw "H3 stat-gain result mismatch for $($expected.id)."
    }
    for ($rngIndex = 0; $rngIndex -lt @($expected.rng).Count; $rngIndex++) {
        if ([int] $actual.rng[$rngIndex].observedSeed -ne [int] $expected.rng[$rngIndex].expectedSeed -or
            [int] $actual.rng[$rngIndex].observedValue -ne [int] $expected.rng[$rngIndex].expectedValue) {
            throw "H3 stat-gain RNG mismatch for $($expected.id), call $rngIndex."
        }
    }
}

[pscustomobject] @{
    Fixture = [string] $fixture.id
    Engine = "BizHawk $($fixture.emulator.version) / $($fixture.emulator.core)"
    Cases = @($fixture.cases).Count
    PityCases = @($fixture.cases | Where-Object expectedPity).Count
    NoGrowthCases = @($fixture.cases | Where-Object expectedPath -eq 'none').Count
    FunctionEntry = ('0x{0:X}' -f [int] $fixture.function.entryAddress)
    Status = 'PASS'
} | Format-List
