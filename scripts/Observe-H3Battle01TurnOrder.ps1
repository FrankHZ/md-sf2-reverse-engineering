[CmdletBinding()]
param(
    [string] $RomPath = (Join-Path $PSScriptRoot '..\local\roms\sf2-us.bin'),
    [int] $Seed = 0x1234,
    [ValidateSet('baseline', 'boundaries', 'activation', 'damage', 'chain', 'dodge')] [string] $Scenario = 'baseline',
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
local damage = nil
local damage_playback = false
local chain = nil
local chain_playback = false
local dodge_case = nil
local activation = nil
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
    if scenario ~= "activation" then return end
    local function entry(combatant)
        local slot = combatant
        if combatant >= 128 then slot = combatant - 96 end
        return 0xFFE800 + slot * 56
    end
    memory.write_u8(entry(0) + 46, 8, "M68K BUS")
    memory.write_u8(entry(0) + 47, 12, "M68K BUS")
    status("milestone:activation-boundary-setup")
end, 0x2550C, "sf2-activation-boundary-setup", "M68K BUS")

local function capture_activation()
    local function entry(combatant)
        local slot = combatant
        if combatant >= 128 then slot = combatant - 96 end
        return 0xFFE800 + slot * 56
    end
    local function flag_set(flag)
        local byte = memory.read_u8(0xFFF686 + math.floor(flag / 8), "M68K BUS")
        local mask = 0x80 >> (flag % 8)
        return (byte & mask) ~= 0
    end
    local snapshot = {
        newly_triggered = memory.read_u16_be(0xFFB20C, "M68K BUS"),
        region_flags = { flag_set(90), flag_set(91), flag_set(92) },
        enemies = {}
    }
    for combatant = 128, 133 do
        snapshot.enemies[#snapshot.enemies + 1] = {
            combatant = combatant,
            bitfield = memory.read_u16_be(entry(combatant) + 52, "M68K BUS")
        }
    end
    return snapshot
end

event.on_bus_exec(function()
    stage = "battle"
    status("milestone:turn-order-entry")
    print("milestone:turn-order-entry")
    activation = capture_activation()
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
    elseif scenario == "damage" then
        local function entry(combatant)
            local slot = combatant
            if combatant >= 128 then slot = combatant - 96 end
            return 0xFFE800 + slot * 56
        end
        memory.write_u8(entry(0) + 19, 99, "M68K BUS")
        memory.write_u8(entry(0) + 10, 0, "M68K BUS")
        memory.write_u8(entry(0) + 11, 1, "M68K BUS")
        memory.write_u8(entry(0) + 31, 0, "M68K BUS")
        memory.write_u8(entry(0) + 48, 0, "M68K BUS")
        memory.write_u8(entry(0) + 49, 0x80, "M68K BUS")
        memory.write_u16_be(entry(0) + 52, 4, "M68K BUS")
        memory.write_u8(entry(128) + 11, 1, "M68K BUS")
        memory.write_u16_be(entry(128) + 12, 100, "M68K BUS")
        memory.write_u16_be(entry(128) + 14, 100, "M68K BUS")
        memory.write_u8(entry(128) + 21, 20, "M68K BUS")
        memory.write_u8(entry(128) + 46, 8, "M68K BUS")
        memory.write_u8(entry(128) + 47, 17, "M68K BUS")
        memory.write_u8(entry(128) + 49, 0x60, "M68K BUS")
        memory.write_u8(0xFF5F00 + 17 * 48 + 8, 3, "M68K BUS")
    elseif scenario == "chain" then
        local function entry(combatant)
            local slot = combatant
            if combatant >= 128 then slot = combatant - 96 end
            return 0xFFE800 + slot * 56
        end
        memory.write_u16_be(entry(0) + 12, 200, "M68K BUS")
        memory.write_u16_be(entry(0) + 14, 200, "M68K BUS")
        memory.write_u8(entry(0) + 19, 50, "M68K BUS")
        memory.write_u8(entry(0) + 21, 30, "M68K BUS")
        memory.write_u8(entry(0) + 31, 0x38, "M68K BUS")
        memory.write_u8(entry(0) + 49, 0x00, "M68K BUS")
        memory.write_u16_be(entry(0) + 52, 4, "M68K BUS")
        memory.write_u16_be(entry(128) + 12, 200, "M68K BUS")
        memory.write_u16_be(entry(128) + 14, 200, "M68K BUS")
        memory.write_u8(entry(128) + 19, 40, "M68K BUS")
        memory.write_u8(entry(128) + 21, 20, "M68K BUS")
        memory.write_u8(entry(128) + 31, 0xC8, "M68K BUS")
        memory.write_u8(entry(128) + 46, 8, "M68K BUS")
        memory.write_u8(entry(128) + 47, 17, "M68K BUS")
        memory.write_u8(entry(128) + 49, 0x60, "M68K BUS")
        memory.write_u8(0xFF5F00 + 17 * 48 + 8, 3, "M68K BUS")
        memory.write_u8(0xFF5F00 + 18 * 48 + 8, 1, "M68K BUS")
        chain = { attacks = {}, reactions = {}, decision = nil, decision_active = false }
    elseif scenario == "dodge" then
        local function entry(combatant)
            local slot = combatant
            if combatant >= 128 then slot = combatant - 96 end
            return 0xFFE800 + slot * 56
        end
        memory.write_u16_be(entry(0) + 12, 100, "M68K BUS")
        memory.write_u16_be(entry(0) + 14, 100, "M68K BUS")
        memory.write_u8(entry(0) + 19, 50, "M68K BUS")
        memory.write_u8(entry(0) + 31, 8, "M68K BUS")
        memory.write_u8(entry(0) + 49, 0x00, "M68K BUS")
        memory.write_u16_be(entry(0) + 52, 4, "M68K BUS")
        memory.write_u16_be(entry(128) + 12, 100, "M68K BUS")
        memory.write_u16_be(entry(128) + 14, 100, "M68K BUS")
        memory.write_u8(entry(128) + 21, 20, "M68K BUS")
        memory.write_u8(entry(128) + 31, 8, "M68K BUS")
        memory.write_u8(entry(128) + 46, 8, "M68K BUS")
        memory.write_u8(entry(128) + 47, 17, "M68K BUS")
        memory.write_u8(entry(128) + 49, 0x60, "M68K BUS")
        dodge_case = { calculate_calls = 0 }
    end
end, 0x25544, "sf2-turn-order-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    local a4 = emu.getregister("M68K A4"); local a5 = emu.getregister("M68K A5")
    dodge_case.actor = memory.read_u8(a4 & 0xFFFFFF, "M68K BUS")
    dodge_case.target = memory.read_u8(a5 & 0xFFFFFF, "M68K BUS")
end, 0xAAFC, "sf2-dodge-case-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    memory.write_u16_be(0xFFDEA4, 0, "M68K BUS")
    dodge_case.range = emu.getregister("M68K D2") & 0xFFFF
end, 0xAB7C, "sf2-dodge-case-seed", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    dodge_case.roll = emu.getregister("M68K D0") & 0xFFFF
end, 0xAB82, "sf2-dodge-case-roll", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    dodge_case.calculate_calls = dodge_case.calculate_calls + 1
end, 0xABBE, "sf2-dodge-case-unexpected-damage", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    local a2 = emu.getregister("M68K A2")
    dodge_case.flag = memory.read_u8((a2 - 5) & 0xFFFFFF, "M68K BUS") ~= 0
end, 0xABBC, "sf2-dodge-case-return", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    memory.write_u16_be(0xFFDEA4, 0xFFFF, "M68K BUS")
end, 0xB00E, "sf2-dodge-followup-seed", "M68K BUS")

event.on_bus_exec(function()
    if scenario == "dodge" and dodge_case ~= nil then dodge_case.double_roll = emu.getregister("M68K D0") & 0xFFFF end
end, 0xB03C, "sf2-dodge-followup-double", "M68K BUS")

event.on_bus_exec(function()
    if scenario == "dodge" and dodge_case ~= nil then dodge_case.counter_roll = emu.getregister("M68K D0") & 0xFFFF end
end, 0xB074, "sf2-dodge-followup-counter", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "dodge" or dodge_case == nil then return end
    local a2 = emu.getregister("M68K A2")
    dodge_case.double = memory.read_u8((a2 - 13) & 0xFFFFFF, "M68K BUS") ~= 0
    dodge_case.counter = memory.read_u8((a2 - 12) & 0xFFFFFF, "M68K BUS") ~= 0
    local output = assert(io.open(output_path, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","scenario":"dodge","battle":%d,"actor":%d,"target":%d,"range":%d,"roll":%d,"dodge":%s,"calculateDamageCalls":%d,"doubleRoll":%d,"counterRoll":%d,"double":%s,"counter":%s,"actorHp":%d,"targetHp":%d}\n',
        emu.getsystemid(), memory.read_u8(0xFFF712, "M68K BUS"), dodge_case.actor, dodge_case.target,
        dodge_case.range, dodge_case.roll, tostring(dodge_case.flag), dodge_case.calculate_calls,
        dodge_case.double_roll, dodge_case.counter_roll, tostring(dodge_case.double), tostring(dodge_case.counter),
        memory.read_u16_be(0xFFE800 + 14, "M68K BUS"), memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
    ))
    output:close(); client.exitCode(0)
end, 0xB07E, "sf2-dodge-case-observe", "M68K BUS")

local function chain_attack()
    if chain == nil or #chain.attacks == 0 then return nil end
    return chain.attacks[#chain.attacks]
end

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil then return end
    local a4 = emu.getregister("M68K A4")
    local a5 = emu.getregister("M68K A5")
    chain.attacks[#chain.attacks + 1] = {
        attack_type = memory.read_u16_be(0xFFB636, "M68K BUS"),
        actor = memory.read_u8(a4 & 0xFFFFFF, "M68K BUS"),
        target = memory.read_u8(a5 & 0xFFFFFF, "M68K BUS")
    }
end, 0xAAFC, "sf2-chain-attack-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" then return end
    local attack = chain_attack(); if attack == nil then return end
    memory.write_u16_be(0xFFDEA4, 0xFFFF, "M68K BUS")
    attack.dodge_range = emu.getregister("M68K D2") & 0xFFFF
end, 0xAB7C, "sf2-chain-dodge-seed", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.dodge_roll = emu.getregister("M68K D0") & 0xFFFF
end, 0xAB82, "sf2-chain-dodge-roll", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.base_damage = emu.getregister("M68K D6") & 0xFFFF
end, 0xAC4C, "sf2-chain-base-damage", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.inflict_entry = emu.getregister("M68K D6") & 0xFFFF
end, 0xACEA, "sf2-chain-inflict-entry", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.pre_variance = emu.getregister("M68K D6") & 0xFFFF
end, 0xAD3E, "sf2-chain-pre-variance", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.variance_range = emu.getregister("M68K D0") & 0xFFFF
end, 0xAD46, "sf2-chain-variance-range", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.first_roll = emu.getregister("M68K D0") & 0xFFFF
    attack.after_first = emu.getregister("M68K D6") & 0xFFFF
end, 0xAD4C, "sf2-chain-variance-first", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.second_roll = emu.getregister("M68K D0") & 0xFFFF
end, 0xAD52, "sf2-chain-variance-second", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    attack.final_damage = emu.getregister("M68K D6") & 0xFFFF
end, 0xAD58, "sf2-chain-variance-final", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    local target = attack.target
    local slot = target; if target >= 128 then slot = target - 96 end
    attack.hp_before = memory.read_u16_be(0xFFE800 + slot * 56 + 14, "M68K BUS")
end, 0xAD74, "sf2-chain-hp-before", "M68K BUS")

event.on_bus_exec(function()
    local attack = chain_attack(); if scenario ~= "chain" or attack == nil then return end
    local target = attack.target
    local slot = target; if target >= 128 then slot = target - 96 end
    attack.hp_after = memory.read_u16_be(0xFFE800 + slot * 56 + 14, "M68K BUS")
end, 0xAD7E, "sf2-chain-hp-after", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil or chain.decision ~= nil then return end
    memory.write_u16_be(0xFFDEA4, 0, "M68K BUS")
    chain.decision = {}; chain.decision_active = true
end, 0xB00E, "sf2-chain-decision-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or not chain.decision_active then return end
    chain.decision.double_roll = emu.getregister("M68K D0") & 0xFFFF
end, 0xB03C, "sf2-chain-double-roll", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or not chain.decision_active then return end
    chain.decision.counter_roll = emu.getregister("M68K D0") & 0xFFFF
end, 0xB074, "sf2-chain-counter-roll", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or not chain.decision_active then return end
    local a2 = emu.getregister("M68K A2")
    chain.decision.double = memory.read_u8((a2 - 13) & 0xFFFFFF, "M68K BUS") ~= 0
    chain.decision.counter = memory.read_u8((a2 - 12) & 0xFFFFFF, "M68K BUS") ~= 0
    chain.decision_active = false
end, 0xB07E, "sf2-chain-decision-return", "M68K BUS")

event.on_bus_exec(function()
    if scenario == "damage" or scenario == "chain" or scenario == "dodge" then return end
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
    output:write('],"activation":{"newlyTriggered":' .. activation.newly_triggered .. ',"regionFlags":[')
    for index = 1, #activation.region_flags do
        if index > 1 then output:write(",") end
        output:write(tostring(activation.region_flags[index]))
    end
    output:write('],"enemies":[')
    for index = 1, #activation.enemies do
        if index > 1 then output:write(",") end
        local enemy = activation.enemies[index]
        output:write(string.format('{"combatant":%d,"bitfield":%d}', enemy.combatant, enemy.bitfield))
    end
    output:write("]}}\n")
    output:close()
    client.exitCode(0)
end, 0x2559E, "sf2-turn-order-observe", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" then return end
    damage = { bonus = false }
end, 0xABBE, "sf2-damage-entry", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.actor = emu.getregister("M68K D0") & 0xFF end
end, 0xABC0, "sf2-damage-actor", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.target = emu.getregister("M68K D0") & 0xFF end
end, 0xABCA, "sf2-damage-target", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.land_effect = emu.getregister("M68K D1") & 0xFF end
end, 0xABE0, "sf2-damage-land-effect", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.base = emu.getregister("M68K D6") & 0xFFFF; damage.multiplier = emu.getregister("M68K D3") & 0xFFFF end
end, 0xABFA, "sf2-damage-before-land", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.reduced = emu.getregister("M68K D6") & 0xFFFF end
end, 0xABFE, "sf2-damage-after-land", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.bonus = true end
end, 0xAC46, "sf2-damage-archer-bonus", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    damage.result = emu.getregister("M68K D6") & 0xFFFF
end, 0xAC4C, "sf2-damage-return", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    memory.write_u16_be(0xFFDEA4, 0, "M68K BUS")
    damage.critical = {
        seed = 0,
        prowess = memory.read_u8(0xFFE800 + 31, "M68K BUS"),
        before = emu.getregister("M68K D6") & 0xFFFF
    }
end, 0xAC4E, "sf2-critical-entry", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.critical ~= nil then damage.critical.range = emu.getregister("M68K D0") & 0xFFFF end
end, 0xAC78, "sf2-critical-range", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.critical ~= nil then damage.critical.roll = emu.getregister("M68K D0") & 0xFFFF end
end, 0xAC7C, "sf2-critical-roll", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.critical ~= nil then damage.critical.after = emu.getregister("M68K D6") & 0xFFFF end
end, 0xAC8C, "sf2-critical-damage", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    damage.inflict_entry = emu.getregister("M68K D6") & 0xFFFF
end, 0xACEA, "sf2-inflict-entry", "M68K BUS")

event.on_bus_exec(function()
    if damage == nil then return end
    damage.variance = { range = emu.getregister("M68K D0") & 0xFFFF, before = emu.getregister("M68K D6") & 0xFFFF }
end, 0xAD46, "sf2-variance-range", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.variance ~= nil then
        damage.variance.first = emu.getregister("M68K D0") & 0xFFFF
        damage.variance.after_first = emu.getregister("M68K D6") & 0xFFFF
    end
end, 0xAD4C, "sf2-variance-first", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.variance ~= nil then damage.variance.second = emu.getregister("M68K D0") & 0xFFFF end
end, 0xAD52, "sf2-variance-second", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.variance ~= nil then damage.variance.final = emu.getregister("M68K D6") & 0xFFFF end
end, 0xAD58, "sf2-variance-final", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil then damage.exp_after_damage = memory.read_u16_be(0xFFB62C, "M68K BUS") end
end, 0xAD5E, "sf2-damage-exp", "M68K BUS")

event.on_bus_exec(function()
    if damage == nil then return end
    damage.hp_before = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
end, 0xAD74, "sf2-hp-before", "M68K BUS")

event.on_bus_exec(function()
    if damage == nil then return end
    damage.hp_after = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
end, 0xAD7E, "sf2-hp-after", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    local a2 = emu.getregister("M68K A2")
    damage.target_dies = memory.read_u8((a2 - 4) & 0xFFFFFF, "M68K BUS") ~= 0
    damage.exp_final = memory.read_u16_be(0xFFB62C, "M68K BUS")
    damage_playback = true
    status("milestone:damage-script-complete")
end, 0xAD92, "sf2-damage-application", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    damage.restored_hp = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
end, 0xA3E6, "sf2-damage-hp-restored", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil then return end
    chain.restored_ally_hp = memory.read_u16_be(0xFFE800 + 14, "M68K BUS")
    chain.restored_enemy_hp = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
    chain_playback = true
    status("milestone:chain-script-complete")
end, 0xA3E6, "sf2-chain-hp-restored", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    damage.award_calculation = {
        seed = memory.read_u16_be(0xFFDEA4, "M68K BUS"),
        halved = emu.getregister("M68K D1") & 0xFFFF
    }
end, 0xA81E, "sf2-exp-halved", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.award_calculation ~= nil then
        damage.award_calculation.first = emu.getregister("M68K D0") & 0xFFFF
    end
end, 0xA826, "sf2-exp-first-roll", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.award_calculation ~= nil then
        damage.award_calculation.second = emu.getregister("M68K D0") & 0xFFFF
    end
end, 0xA834, "sf2-exp-second-roll", "M68K BUS")

event.on_bus_exec(function()
    if damage ~= nil and damage.award_calculation ~= nil then
        damage.award_calculation.final = emu.getregister("M68K D1") & 0xFFFF
    end
end, 0xA840, "sf2-exp-final", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    local a6 = emu.getregister("M68K A6")
    damage.reaction = {
        combatant = memory.read_u16_be(0xFFB3CE, "M68K BUS"),
        hp_change = memory.read_s16_be(a6 & 0xFFFFFF, "M68K BUS")
    }
end, 0x18F4E, "sf2-damage-reaction-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil then return end
    local a6 = emu.getregister("M68K A6")
    chain.reactions[#chain.reactions + 1] = {
        kind = "enemy",
        combatant = memory.read_u16_be(0xFFB3CE, "M68K BUS"),
        hp_change = memory.read_s16_be(a6 & 0xFFFFFF, "M68K BUS")
    }
end, 0x18F4E, "sf2-chain-enemy-reaction-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil or damage.reaction == nil then return end
    damage.reaction.hp_after = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
end, 0x18F7E, "sf2-damage-reaction-applied", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil or #chain.reactions == 0 then return end
    local reaction = chain.reactions[#chain.reactions]
    if reaction.kind == "enemy" then
        reaction.hp_after = memory.read_u16_be(0xFFE800 + 32 * 56 + 14, "M68K BUS")
    end
end, 0x18F7E, "sf2-chain-enemy-reaction-applied", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil then return end
    local a6 = emu.getregister("M68K A6")
    chain.reactions[#chain.reactions + 1] = {
        kind = "ally",
        combatant = memory.read_u16_be(0xFFB3D4, "M68K BUS"),
        hp_change = memory.read_s16_be(a6 & 0xFFFFFF, "M68K BUS")
    }
end, 0x18DBE, "sf2-chain-ally-reaction-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "chain" or chain == nil or #chain.reactions == 0 then return end
    local reaction = chain.reactions[#chain.reactions]
    if reaction.kind ~= "ally" then return end
    reaction.hp_after = memory.read_u16_be(0xFFE800 + 14, "M68K BUS")
    local output = assert(io.open(output_path, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","scenario":"chain","battle":%d,"decision":{"doubleRoll":%d,"counterRoll":%d,"double":%s,"counter":%s},"restored":{"allyHp":%d,"enemyHp":%d},"attacks":[',
        emu.getsystemid(), memory.read_u8(0xFFF712, "M68K BUS"), chain.decision.double_roll,
        chain.decision.counter_roll, tostring(chain.decision.double), tostring(chain.decision.counter),
        chain.restored_ally_hp, chain.restored_enemy_hp
    ))
    for index = 1, #chain.attacks do
        if index > 1 then output:write(",") end
        local attack = chain.attacks[index]
        output:write(string.format(
            '{"attackType":%d,"actor":%d,"target":%d,"dodgeRange":%d,"dodgeRoll":%d,"baseDamage":%d,"inflictEntry":%d,"preVariance":%d,"varianceRange":%d,"firstRoll":%d,"afterFirst":%d,"secondRoll":%d,"finalDamage":%d,"hpBefore":%d,"hpAfter":%d}',
            attack.attack_type, attack.actor, attack.target, attack.dodge_range, attack.dodge_roll,
            attack.base_damage, attack.inflict_entry, attack.pre_variance, attack.variance_range,
            attack.first_roll, attack.after_first, attack.second_roll, attack.final_damage,
            attack.hp_before, attack.hp_after
        ))
    end
    output:write('],"reactions":[')
    for index = 1, #chain.reactions do
        if index > 1 then output:write(",") end
        local item = chain.reactions[index]
        output:write(string.format('{"kind":"%s","combatant":%d,"hpChange":%d,"hpAfter":%d}',
            item.kind, item.combatant, item.hp_change, item.hp_after))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end, 0x18DE8, "sf2-chain-ally-reaction-applied", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil then return end
    local a6 = emu.getregister("M68K A6")
    damage.award = {
        command_exp = memory.read_u16_be(a6 & 0xFFFFFF, "M68K BUS"),
        actor_exp_before = memory.read_u8(0xFFE800 + 48, "M68K BUS")
    }
end, 0x190DC, "sf2-exp-command-entry", "M68K BUS")

event.on_bus_exec(function()
    if scenario ~= "damage" or damage == nil or damage.award == nil then return end
    damage.award.actor_exp_after = memory.read_u8(0xFFE800 + 48, "M68K BUS")
    local output = assert(io.open(output_path, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","scenario":"damage","battle":%d,"actor":%d,"target":%d,"base":%d,"landEffect":%d,"multiplier":%d,"reduced":%d,"archerBonus":%s,"result":%d,"critical":{"seed":%d,"prowess":%d,"range":%d,"roll":%d,"before":%d,"after":%d},"inflictEntry":%d,"variance":{"range":%d,"first":%d,"afterFirst":%d,"second":%d,"final":%d},"hp":{"before":%d,"after":%d,"targetDies":%s,"restored":%d},"exp":{"afterDamage":%d,"final":%d},"awardCalculation":{"seed":%d,"halved":%d,"first":%d,"second":%d,"final":%d},"reaction":{"combatant":%d,"hpChange":%d,"hpAfter":%d},"award":{"commandExp":%d,"actorExpBefore":%d,"actorExpAfter":%d}}\n',
        emu.getsystemid(), memory.read_u8(0xFFF712, "M68K BUS"), damage.actor, damage.target, damage.base,
        damage.land_effect, damage.multiplier, damage.reduced, tostring(damage.bonus), damage.result,
        damage.critical.seed, damage.critical.prowess, damage.critical.range, damage.critical.roll,
        damage.critical.before, damage.critical.after, damage.inflict_entry,
        damage.variance.range, damage.variance.first, damage.variance.after_first, damage.variance.second, damage.variance.final,
        damage.hp_before, damage.hp_after, tostring(damage.target_dies), damage.restored_hp,
        damage.exp_after_damage, damage.exp_final, damage.award_calculation.seed, damage.award_calculation.halved, damage.award_calculation.first,
        damage.award_calculation.second, damage.award_calculation.final, damage.reaction.combatant, damage.reaction.hp_change,
        damage.reaction.hp_after, damage.award.command_exp, damage.award.actor_exp_before, damage.award.actor_exp_after
    ))
    output:close()
    client.exitCode(0)
end, 0x190F8, "sf2-exp-command-applied", "M68K BUS")

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
    elseif stage == "battle" and (damage_playback or chain_playback) and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(0xFFF712, "M68K BUS") == 1) or damage_playback or chain_playback) }, 2)
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
