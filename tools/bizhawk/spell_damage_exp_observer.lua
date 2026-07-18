local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local targets_supplied = false
local active = nil
local case_index = 1
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

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"baseSpell":%d},"cases":[',
        emu.getsystemid(), config.battleId, config.setup.actionType, config.setup.actionSpell,
        config.setup.baseSpell))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","class":%d,"actorLevel":%d,"targetLevel":%d,' ..
            '"targetMaxHp":%d,"targetCurrentHp":%d,"finalDamage":%d,' ..
            '"levelBracketExp":%d,"initialAccumulatedExp":%d,"afterDamageExp":%d,' ..
            '"killApplied":%s,"afterKillExp":%d,"awardBattle":%d,"halvedExp":%d,' ..
            '"firstRoll":%d,"secondRoll":%d,"commandExp":%d}',
            result.id, result.class, result.actorLevel, result.targetLevel,
            result.targetMaxHp, result.targetCurrentHp, result.finalDamage,
            result.levelBracketExp, result.initialAccumulatedExp, result.afterDamageExp,
            tostring(result.killApplied), result.afterKillExp, result.awardBattle,
            result.halvedExp, result.firstRoll, result.secondRoll, result.commandExp))
    end
    output:write("]}}\n")
    output:close()
    if replay_state ~= nil then memorysavestate.removestate(replay_state) end
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-spell-exp-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-spell-exp-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-spell-exp-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.setup.actor)
    memory.write_u8(actor + 10, 0, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, 20, "M68K BUS")
    memory.write_u8(actor + 17, 20, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u16_be(actor + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 38, 0x007F, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local target = entry(config.setup.target)
    memory.write_u8(target + 11, 1, "M68K BUS")
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u16_be(target + 26, 0, "M68K BUS")
    memory.write_u16_be(target + 28, 0, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(target + 55, 39, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    pending_save = true
    status("milestone:battle-setup")
end, config.harness["function"].turnOrderEntryAddress, "sf2-spell-exp-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.setup.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.setup.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.setup.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.setup.target, "M68K BUS")
    status("milestone:spell-action:" .. case_index)
end, config["function"].writeBattleSceneScriptAddress, "sf2-spell-exp-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or targets_supplied then return end
    targets_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.setup.target, "M68K BUS")
end, config["function"].initializePropertiesAddress, "sf2-spell-exp-target", "M68K BUS")

event.on_bus_exec(function()
    if action_started then
        memory.write_u16_be(config.ram.seedAddress, config.setup.damageSeed, "M68K BUS")
    end
end, config["function"].calculateSpellDamageAddress, "sf2-spell-exp-damage-seed", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or active ~= nil then return end
    local current_case = config.cases[case_index]
    local actor_pointer = emu.getregister("M68K A4") & 0xFFFFFF
    local target_pointer = emu.getregister("M68K A5") & 0xFFFFFF
    if memory.read_u8(actor_pointer, "M68K BUS") ~= config.setup.actor then return end
    if memory.read_u8(target_pointer, "M68K BUS") ~= config.setup.target then return end

    local actor = entry(config.setup.actor)
    local target = entry(config.setup.target)
    memory.write_u8(actor + 10, current_case.class, "M68K BUS")
    memory.write_u8(actor + 11, current_case.actorLevel, "M68K BUS")
    memory.write_u8(target + 11, current_case.targetLevel, "M68K BUS")
    memory.write_u16_be(target + 12, current_case.targetMaxHp, "M68K BUS")
    memory.write_u16_be(target + 14, current_case.targetCurrentHp, "M68K BUS")
    memory.write_u16_be(config.ram.battleSceneExpAddress,
        current_case.initialAccumulatedExp, "M68K BUS")
    active = {
        id = current_case.id,
        class = current_case.class,
        actorLevel = current_case.actorLevel,
        targetLevel = current_case.targetLevel,
        targetMaxHp = memory.read_u16_be(target + 12, "M68K BUS"),
        targetCurrentHp = memory.read_u16_be(target + 14, "M68K BUS"),
        finalDamage = word_register("M68K D6"),
        initialAccumulatedExp = current_case.initialAccumulatedExp,
        killApplied = false,
        awardBattle = current_case.awardBattle
    }
    status("milestone:damage-exp-entry:" .. case_index)
end, config["function"].calculateDamageExpEntryAddress, "sf2-spell-exp-damage-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil and active.levelBracketExp == nil then
        active.levelBracketExp = word_register("M68K D5")
    end
end, config["function"].getKillExpReturnAddress, "sf2-spell-exp-bracket", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.afterDamageExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
        active.afterKillExp = active.afterDamageExp
    end
end, config["function"].calculateDamageExpReturnAddress, "sf2-spell-exp-damage-return", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.killApplied = true end
end, config["function"].addKillExpEntryAddress, "sf2-spell-exp-kill-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.afterKillExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].addKillExpReturnAddress, "sf2-spell-exp-kill-return", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    memory.write_u8(config.harness.ram.currentBattleAddress, active.awardBattle, "M68K BUS")
    memory.write_u16_be(config.ram.seedAddress, config.setup.awardSeed, "M68K BUS")
end, config["function"].awardExpEntryAddress, "sf2-spell-exp-award-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.halvedExp = word_register("M68K D1") end
end, config["function"].expHalvedAddress, "sf2-spell-exp-halved", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-spell-exp-first-roll", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-spell-exp-second-roll", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-spell-exp-final", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or active.commandExp == nil then return end
    results[#results + 1] = active
    active = nil
    case_index = case_index + 1
    if case_index > #config.cases then
        write_result_and_exit()
    else
        pending_replay = true
        status("milestone:replay-case:" .. case_index)
    end
end, config["function"].battleSceneEndReturnAddress, "sf2-spell-exp-scene-end", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
        status("milestone:replay-state-saved")
    elseif pending_replay then
        pending_replay = false
        action_started = false
        targets_supplied = false
        active = nil
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
        status(string.format("frame=%d,stage=%s,pc=%X,case=%d,action=%s", frames,
            stage, emu.getregister("M68K PC"), case_index, tostring(action_started)))
    end
end
