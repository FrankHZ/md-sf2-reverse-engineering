[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [int] $Seed = 0x1234,
    [ValidateSet('baseline', 'boundaries')] [string] $Scenario = 'baseline',
    [int] $TimeoutSeconds = 45
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'manifests\toolchain.json') -Encoding utf8 | ConvertFrom-Json
$bizHawk = (Resolve-Path -LiteralPath (Join-Path $repoRoot ([string] $toolchain.bizhawk.localExecutablePath))).Path
$derived = Join-Path $repoRoot 'local\derived\h3'
$luaPath = Join-Path $derived 'battle01-turn-order-observe.generated.lua'
$outputPath = Join-Path $derived 'battle01-turn-order-observed.json'
$statusPath = Join-Path $derived 'battle01-turn-order-status.txt'
New-Item -ItemType Directory -Path $derived -Force | Out-Null
Remove-Item -LiteralPath $outputPath, $statusPath -Force -ErrorAction SilentlyContinue
$luaOutput = $outputPath.Replace('\', '/')
$luaStatus = $statusPath.Replace('\', '/')
$luaScenario = $Scenario
$lua = @'
local output_path = "__OUTPUT__"
local status_path = "__STATUS__"
local seed = __SEED__
local scenario = "__SCENARIO__"
local stage = "cheat"
local prompt_count = 0
local queue = {}
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }
local function status(value)
    local file = assert(io.open(status_path, "w")); file:write(value .. "\n"); file:close()
end

local function enqueue(name, count)
    for _ = 1, count do queue[#queue + 1] = name end
end
local function pulse(name)
    enqueue("", 30); enqueue(name, 4); enqueue("", 8)
end
local function set_button(name)
    local buttons = {}
    if name ~= nil and name ~= "" then buttons[name] = true end
    joypad.set(buttons, 1)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
    print("milestone:battle-test")
end, 0x769C, "sf2-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    print("milestone:number-prompt:" .. prompt_count)
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, 0x16282, "sf2-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, 0x163BC, "sf2-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    status("milestone:turn-order-entry")
    print("milestone:turn-order-entry")
    memory.write_u16_be(0xFFDEA4, seed, "M68K BUS")
    if scenario == "boundaries" then
        local function entry(combatant)
            local slot = combatant
            if combatant >= 128 then slot = combatant - 96 end
            return 0xFFE800 + slot * 56
        end
        memory.write_u8(entry(0) + 23, 128, "M68K BUS")
        memory.write_u8(entry(1) + 23, 127, "M68K BUS")
        memory.write_u16_be(entry(2) + 14, 0, "M68K BUS")
        memory.write_u8(entry(128) + 46, 255, "M68K BUS")
    end
end, 0x25544, "sf2-turn-order-entry", "M68K BUS")

event.on_bus_exec(function()
    local output = assert(io.open(output_path, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","scenario":"%s","seed":%d,"battle":%d,"entries":[',
        emu.getsystemid(), scenario, seed, memory.read_u8(0xFFF712, "M68K BUS")))
    local first = true
    for index = 0, 63 do
        local combatant = memory.read_u8(0xFFF71A + index * 2, "M68K BUS")
        local score = memory.read_u8(0xFFF71B + index * 2, "M68K BUS")
        if combatant ~= 255 then
            if not first then output:write(",") end
            first = false
            output:write(string.format('{"combatant":%d,"score":%d}', combatant, score))
        end
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end, 0x2559E, "sf2-turn-order-observe", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(0xFFB1A0, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(0xFFB0A9, "M68K BUS") == 255 then button = "Up" end
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(0xFFF712, "M68K BUS") == 1 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = (stage == "ui" and memory.read_u8(0xFFF712, "M68K BUS") == 1) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,pointer=%X,debug=%d,prompts=%d,queue=%d,battle=%d", frames, stage,
            emu.getregister("M68K PC"), memory.read_u32_be(0xFFB1A0, "M68K BUS"), memory.read_u8(0xFFB0A9, "M68K BUS"),
            prompt_count, #queue, memory.read_u8(0xFFF712, "M68K BUS")))
        print(string.format("status:frame=%d,stage=%s,pointer=%X,debug=%d,prompts=%d,queue=%d", frames, stage,
            memory.read_u32_be(0xFFB1A0, "M68K BUS"), memory.read_u8(0xFFB0A9, "M68K BUS"), prompt_count, #queue))
    end
end
'@.Replace('__OUTPUT__', $luaOutput).Replace('__STATUS__', $luaStatus).Replace('__SEED__', [string] $Seed).Replace('__SCENARIO__', $luaScenario)
Set-Content -LiteralPath $luaPath -Value $lua -Encoding utf8

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $bizHawk; $start.WorkingDirectory = Split-Path -Parent $bizHawk
$start.UseShellExecute = $false; $start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
$start.ArgumentList.Add("--lua=$luaPath"); $start.ArgumentList.Add((Resolve-Path -LiteralPath $RomPath).Path)
$process = [Diagnostics.Process]::new(); $process.StartInfo = $start
try {
    if (-not $process.Start()) { throw 'Unable to start BizHawk.' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync(); $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $process.Kill($true); $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult(); $stderr = $stderrTask.GetAwaiter().GetResult()
        $status = if (Test-Path -LiteralPath $statusPath) { Get-Content -Raw -LiteralPath $statusPath } else { 'no status' }
        throw "Turn-order observation timed out after $TimeoutSeconds seconds ($status)."
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult(); $stderr = $stderrTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) { throw "BizHawk failed: $($process.ExitCode)`n$stdout`n$stderr" }
}
finally { $process.Dispose() }
if (-not (Test-Path -LiteralPath $outputPath)) { throw 'Turn-order observation was not written.' }
Get-Content -Raw -LiteralPath $outputPath -Encoding utf8
