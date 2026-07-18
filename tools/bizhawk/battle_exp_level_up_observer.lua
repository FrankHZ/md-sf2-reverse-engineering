local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local playback = false
local award = nil
local level_up = nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
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

local function entry(combatant)
    local slot = combatant
    if combatant >= 128 then slot = combatant - 96 end
    return config.ram.combatantDataAddress + slot * config.ram.combatantEntrySize
end

local function snapshot(combatant)
    local address = entry(combatant)
    return {
        class = memory.read_u8(address + 10, "M68K BUS"),
        level = memory.read_u8(address + 11, "M68K BUS"),
        max_hp = memory.read_u16_be(address + 12, "M68K BUS"),
        current_hp = memory.read_u16_be(address + 14, "M68K BUS"),
        max_mp = memory.read_u8(address + 16, "M68K BUS"),
        current_mp = memory.read_u8(address + 17, "M68K BUS"),
        base_attack = memory.read_u8(address + 18, "M68K BUS"),
        current_attack = memory.read_u8(address + 19, "M68K BUS"),
        base_defense = memory.read_u8(address + 20, "M68K BUS"),
        current_defense = memory.read_u8(address + 21, "M68K BUS"),
        base_agility = memory.read_u8(address + 22, "M68K BUS"),
        current_agility = memory.read_u8(address + 23, "M68K BUS"),
        exp = memory.read_u8(address + 48, "M68K BUS")
    }
end

local function arguments()
    local values = {}
    for index = 0, 6 do
        values[#values + 1] = memory.read_u8(config.ram.levelUpArgumentsAddress + index, "M68K BUS")
    end
    return values
end

local function write_snapshot(output, value)
    output:write(string.format(
        '{"class":%d,"level":%d,"maxHp":%d,"currentHp":%d,"maxMp":%d,"currentMp":%d,' ..
        '"baseAttack":%d,"currentAttack":%d,"baseDefense":%d,"currentDefense":%d,' ..
        '"baseAgility":%d,"currentAgility":%d,"exp":%d}',
        value.class, value.level, value.max_hp, value.current_hp, value.max_mp, value.current_mp,
        value.base_attack, value.current_attack, value.base_defense, value.current_defense,
        value.base_agility, value.current_agility, value.exp))
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"award":{"commandExp":%d,"expBefore":%d,"expAfterIncrease":%d,"expAfterThreshold":%d},' ..
        '"levelUp":{"calls":%d,"seed":%d,"observedSeed":%d,"before":',
        emu.getsystemid(), config.case.id, memory.read_u8(config.ram.currentBattleAddress, "M68K BUS"),
        award.command_exp, award.exp_before, award.exp_after_increase, level_up.before.exp,
        level_up.calls, level_up.seed, level_up.observed_seed))
    write_snapshot(output, level_up.before)
    output:write(',"after":'); write_snapshot(output, level_up.after)
    output:write(',"arguments":[')
    for index, value in ipairs(level_up.arguments) do
        if index > 1 then output:write(",") end
        output:write(tostring(value))
    end
    output:write(']},"final":'); write_snapshot(output, snapshot(config.case.actor))
    output:write("}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config["function"].battleTestAddress, "sf2-exp-level-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config["function"].numberPromptAddress, "sf2-exp-level-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config["function"].flagPromptAddress, "sf2-exp-level-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    local input = config.case.input
    memory.write_u8(actor + 10, input.class, "M68K BUS")
    memory.write_u8(actor + 11, input.level, "M68K BUS")
    memory.write_u16_be(actor + 12, input.maxHp, "M68K BUS")
    memory.write_u16_be(actor + 14, input.currentHp, "M68K BUS")
    memory.write_u8(actor + 16, input.maxMp, "M68K BUS")
    memory.write_u8(actor + 17, input.currentMp, "M68K BUS")
    memory.write_u8(actor + 18, input.baseAttack, "M68K BUS")
    memory.write_u8(actor + 19, input.battleAttack, "M68K BUS")
    memory.write_u8(actor + 20, input.baseDefense, "M68K BUS")
    memory.write_u8(actor + 21, input.currentDefense, "M68K BUS")
    memory.write_u8(actor + 22, input.baseAgility, "M68K BUS")
    memory.write_u8(actor + 23, input.battleAgility, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for index, item in ipairs(input.items) do
        memory.write_u8(actor + 31 + index, item, "M68K BUS")
    end
    memory.write_u8(actor + 48, input.exp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local target = entry(config.case.target)
    memory.write_u8(target + 11, 1, "M68K BUS")
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u8(target + 21, 20, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(config.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    status("milestone:battle-setup")
end, config["function"].turnOrderEntryAddress, "sf2-exp-level-turn-order", "M68K BUS")

event.on_bus_exec(function()
    memory.write_u16_be(config.ram.seedAddress, 0, "M68K BUS")
end, config["function"].criticalEntryAddress, "sf2-exp-level-critical-seed", "M68K BUS")

event.on_bus_exec(function()
    playback = true
    status("milestone:damage-script-complete")
end, config["function"].damageApplicationAddress, "sf2-exp-level-damage-complete", "M68K BUS")

event.on_bus_exec(function()
    memory.write_u16_be(config.ram.seedAddress, config.case.awardSeed, "M68K BUS")
end, config["function"].expRandomizationAddress, "sf2-exp-level-award-seed", "M68K BUS")

event.on_bus_exec(function()
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    award = {
        command_exp = memory.read_u16_be(a6, "M68K BUS"),
        exp_before = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-exp-level-award-entry", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then
        award.exp_after_increase = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-exp-level-award-applied", "M68K BUS")

event.on_bus_exec(function()
    if award == nil or award.exp_after_increase == nil then return end
    local calls = 1
    if level_up ~= nil then calls = level_up.calls + 1 end
    level_up = {
        calls = calls,
        seed = config.case.levelUpSeed,
        before = snapshot(config.case.actor)
    }
    memory.write_u16_be(config.ram.seedAddress, config.case.levelUpSeed, "M68K BUS")
    status("milestone:level-up-entry")
end, config["function"].levelUpEntryAddress, "sf2-exp-level-level-up-entry", "M68K BUS")

event.on_bus_exec(function()
    if level_up == nil then return end
    level_up.after = snapshot(config.case.actor)
    level_up.arguments = arguments()
    level_up.observed_seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    status("milestone:level-up-return")
end, config["function"].levelUpReturnAddress, "sf2-exp-level-level-up-return", "M68K BUS")

event.on_bus_exec(function()
    if level_up ~= nil and level_up.after ~= nil then write_result_and_exit() end
end, config["function"].giveExpReturnAddress, "sf2-exp-level-command-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.ram.debugModeAddress, "M68K BUS") == 255 then
            button = "Up"
        end
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    elseif stage == "battle" and playback and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1) or playback) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,prompts=%d,queue=%d,battle=%d", frames, stage,
            emu.getregister("M68K PC"), prompt_count, #queue,
            memory.read_u8(config.ram.currentBattleAddress, "M68K BUS")))
    end
end
