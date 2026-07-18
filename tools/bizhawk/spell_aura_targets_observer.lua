local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local case_index = 1
local active = nil
local current_effect = nil
local results = {}
local replay_state = nil
local pending_save = false
local pending_replay = false
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

local function word_register(name)
    return emu.getregister(name) & 0xFFFF
end

local function write_number_array(output, values)
    output:write("[")
    for index, value in ipairs(values) do
        if index > 1 then output:write(",") end
        output:write(tostring(value))
    end
    output:write("]")
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"battle":%d,"cases":[',
        emu.getsystemid(), config.battleId))
    for case_position, result in ipairs(results) do
        if case_position > 1 then output:write(",") end
        output:write(string.format('{"id":"%s","actionSpell":%d,"targets":', result.id, result.actionSpell))
        write_number_array(output, result.targets)
        output:write(',"effects":[')
        for effect_position, effect in ipairs(result.effects) do
            if effect_position > 1 then output:write(",") end
            output:write(string.format(
                '{"target":%d,"maxHp":%d,"currentHp":%d,"preCapPower":%d,"recovery":%d,"initialAccumulator":%d,"finalAccumulator":%d}',
                effect.target, effect.maxHp, effect.currentHp, effect.preCapPower,
                effect.recovery, effect.initialAccumulator, effect.finalAccumulator))
        end
        output:write("]}")
    end
    output:write("]}}\n")
    output:close()
    if replay_state ~= nil then memorysavestate.removestate(replay_state) end
    client.exitCode(0)
end

local function configure_ally(ally)
    local combatant = entry(ally.combatant)
    memory.write_u8(combatant + 10, 0, "M68K BUS")
    memory.write_u8(combatant + 11, 1, "M68K BUS")
    memory.write_u16_be(combatant + 12, ally.maxHp, "M68K BUS")
    memory.write_u16_be(combatant + 14, ally.currentHp, "M68K BUS")
    memory.write_u8(combatant + 16, ally.combatant == config.setup.actor and 99 or 0, "M68K BUS")
    memory.write_u8(combatant + 17, ally.combatant == config.setup.actor and 99 or 0, "M68K BUS")
    memory.write_u8(combatant + 23, ally.combatant == config.setup.actor and 99 or 0, "M68K BUS")
    memory.write_u8(combatant + 31, 0, "M68K BUS")
    memory.write_u16_be(combatant + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(combatant + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(combatant + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(combatant + 38, 0x007F, "M68K BUS")
    memory.write_u8(combatant + 46, ally.x, "M68K BUS")
    memory.write_u8(combatant + 47, ally.y, "M68K BUS")
    memory.write_u8(combatant + 49, ally.combatant == config.setup.actor and 0x80 or 0, "M68K BUS")
    memory.write_u16_be(combatant + 52, 0, "M68K BUS")
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-aura-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-aura-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-aura-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.setup.actor)
    memory.write_u8(actor + 10, 0, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, 99, "M68K BUS")
    memory.write_u8(actor + 17, 99, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u16_be(actor + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 38, 0x007F, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local enemy = entry(config.setup.schedulingTarget)
    memory.write_u8(enemy + 10, 4, "M68K BUS")
    memory.write_u8(enemy + 11, 1, "M68K BUS")
    memory.write_u16_be(enemy + 12, 100, "M68K BUS")
    memory.write_u16_be(enemy + 14, 100, "M68K BUS")
    memory.write_u8(enemy + 46, 8, "M68K BUS")
    memory.write_u8(enemy + 47, 17, "M68K BUS")
    memory.write_u8(enemy + 49, 0x60, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    pending_save = true
    status("milestone:battle-setup")
end, config.harness["function"].turnOrderEntryAddress, "sf2-aura-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.setup.actor then return end
    local current_case = config.cases[case_index]
    local actor = entry(config.setup.actor)
    action_started = true
    for combatant_index = 0, 29 do
        local combatant = entry(combatant_index)
        memory.write_u16_be(combatant + 12, 0, "M68K BUS")
        memory.write_u16_be(combatant + 14, 0, "M68K BUS")
        memory.write_u8(combatant + 46, 0xFF, "M68K BUS")
        memory.write_u8(combatant + 47, 0xFF, "M68K BUS")
        memory.write_u8(combatant + 49, 0, "M68K BUS")
    end
    for _, ally in ipairs(config.setup.allies) do configure_ally(ally) end
    memory.write_u8(actor + 10, config.setup.healerClass, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.setup.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, current_case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.setup.target, "M68K BUS")
    status("milestone:aura-action:" .. case_index)
end, config["function"].writeBattleSceneScriptAddress, "sf2-aura-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or active ~= nil then return end
    local current_case = config.cases[case_index]
    local length = memory.read_u16_be(config.ram.targetsListLengthAddress, "M68K BUS")
    active = { id=current_case.id, actionSpell=current_case.actionSpell, targets={}, effects={} }
    for target_index = 0, length - 1 do
        active.targets[#active.targets + 1] = memory.read_u8(config.ram.targetsListAddress + target_index, "M68K BUS")
    end
    status("milestone:targets:" .. case_index .. ":" .. length)
end, config["function"].targetListReadyAddress, "sf2-aura-targets-ready", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    local a5 = emu.getregister("M68K A5") & 0xFFFFFF
    local target = memory.read_u8(a5, "M68K BUS")
    local target_entry = entry(target)
    current_effect = {
        target=target,
        maxHp=memory.read_u16_be(target_entry + 12, "M68K BUS"),
        currentHp=memory.read_u16_be(target_entry + 14, "M68K BUS")
    }
end, config["function"].healEffectEntryAddress, "sf2-aura-heal-effect", "M68K BUS")

event.on_bus_exec(function()
    if current_effect ~= nil then current_effect.preCapPower = word_register("M68K D6") end
end, config["function"].preCapPowerAddress, "sf2-aura-pre-cap", "M68K BUS")

event.on_bus_exec(function()
    if current_effect ~= nil then current_effect.recovery = word_register("M68K D6") end
end, config["function"].cappedRecoveryAddress, "sf2-aura-recovery", "M68K BUS")

event.on_bus_exec(function()
    if current_effect ~= nil then
        current_effect.initialAccumulator = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].healingExpEntryAddress, "sf2-aura-exp-entry", "M68K BUS")

event.on_bus_exec(function()
    if current_effect == nil or active == nil then return end
    current_effect.finalAccumulator = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    active.effects[#active.effects + 1] = current_effect
    current_effect = nil
    if #active.effects == #active.targets then
        results[#results + 1] = active
        active = nil
        case_index = case_index + 1
        if case_index > #config.cases then
            write_result_and_exit()
        else
            pending_replay = true
            status("milestone:replay-case:" .. case_index)
        end
    end
end, config["function"].healingExpReturnAddress, "sf2-aura-exp-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
    elseif pending_replay then
        pending_replay = false
        action_started = false
        active = nil
        current_effect = nil
        memorysavestate.loadcorestate(replay_state)
    end
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.harness.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress, "M68K BUS") == 255 then
            button = "Up"
        end
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = stage == "ui" and
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,case=%d", frames, stage,
            emu.getregister("M68K PC"), case_index))
    end
end
