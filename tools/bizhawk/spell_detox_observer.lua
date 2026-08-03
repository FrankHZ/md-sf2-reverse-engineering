local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local target_supplied = false
local case_index = 1
local active = nil
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

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"battle":%d,"cases":[',
        emu.getsystemid(), config.battleId))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","actionSpell":%d,"initialStatus":%d,"curedFlags":%d,' ..
            '"resultStatus":%d,"reaction":%s,"exp":%d,"curseUnequipped":%s,' ..
            '"ineffective":%s,"finalStatus":%d,"finalItem":%d}',
            result.id, result.actionSpell, result.initialStatus, result.curedFlags,
            result.resultStatus, tostring(result.reaction), result.exp,
            tostring(result.curseUnequipped), tostring(result.ineffective),
            result.finalStatus, result.finalItem))
    end
    output:write("]}}\n")
    output:close()
    if replay_state ~= nil then memorysavestate.removestate(replay_state) end
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-detox-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-detox-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-detox-flag", "M68K BUS")

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
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, 0, "M68K BUS")
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
end, config.harness["function"].turnOrderEntryAddress, "sf2-detox-turn", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.setup.actor then return end
    local current_case = config.cases[case_index]
    local target = entry(config.setup.target)
    action_started = true
    memory.write_u8(target + 10, 1, "M68K BUS")
    memory.write_u8(target + 11, 1, "M68K BUS")
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u8(target + 18, 10, "M68K BUS")
    memory.write_u8(target + 19, 10, "M68K BUS")
    memory.write_u8(target + 20, 10, "M68K BUS")
    memory.write_u8(target + 21, 10, "M68K BUS")
    memory.write_u8(target + 22, 10, "M68K BUS")
    memory.write_u8(target + 23, 10, "M68K BUS")
    memory.write_u16_be(target + 32, config.setup.equippedCursedItem, "M68K BUS")
    for offset = 34, 38, 2 do memory.write_u16_be(target + offset, config.setup.emptyItem, "M68K BUS") end
    memory.write_u16_be(target + 44, current_case.initialStatus, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.setup.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, current_case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.setup.target, "M68K BUS")
    status("milestone:detox-action:" .. case_index)
end, config["function"].writeBattleSceneScriptAddress, "sf2-detox-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.setup.target, "M68K BUS")
end, config["function"].initializePropertiesAddress, "sf2-detox-target", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local current_case = config.cases[case_index]
    active = {
        id=current_case.id, actionSpell=current_case.actionSpell,
        initialStatus=memory.read_u16_be(entry(config.setup.target) + 44, "M68K BUS"),
        curedFlags=0, resultStatus=0, reaction=false, exp=0,
        curseUnequipped=false, ineffective=false
    }
end, config["function"].detoxEffectEntryAddress, "sf2-detox-effect", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.curedFlags = emu.getregister("M68K D2") & 0xFF
        active.resultStatus = emu.getregister("M68K D1") & 0xFFFF
    end
end, config["function"].cureResultAddress, "sf2-detox-result", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.reaction = true end
end, config["function"].reactionAddress, "sf2-detox-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.exp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].statusExpAppliedAddress, "sf2-detox-exp", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.curseUnequipped = true end
end, config["function"].unequipCursedAddress, "sf2-detox-unequip", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.ineffective = true end
end, config["function"].ineffectiveAddress, "sf2-detox-ineffective", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or not active.ineffective then return end
    local target = entry(config.setup.target)
    active.finalStatus = memory.read_u16_be(target + 44, "M68K BUS")
    active.finalItem = memory.read_u16_be(target + 32, "M68K BUS")
    results[#results + 1] = active
    active = nil
    case_index = case_index + 1
    if case_index > #config.cases then write_result_and_exit() end
end, config["function"].effectivenessFailureAddress, "sf2-detox-failure", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    local target = entry(config.setup.target)
    active.finalStatus = memory.read_u16_be(target + 44, "M68K BUS")
    active.finalItem = memory.read_u16_be(target + 32, "M68K BUS")
    results[#results + 1] = active
    active = nil
    case_index = case_index + 1
    if case_index > #config.cases then
        write_result_and_exit()
    else
        pending_replay = true
        status("milestone:replay-case:" .. case_index)
    end
end, config["function"].detoxEffectReturnAddress, "sf2-detox-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
    elseif pending_replay then
        pending_replay = false
        action_started = false
        target_supplied = false
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
        status(string.format("frame=%d,stage=%s,pc=%X,case=%d", frames, stage,
            emu.getregister("M68K PC"), case_index))
    end
end
