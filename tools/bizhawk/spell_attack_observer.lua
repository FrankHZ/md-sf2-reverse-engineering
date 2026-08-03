local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local targets_supplied = false
local active = nil
local records = {}
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
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,"records":[',
        emu.getsystemid(), config.case.id, config.battleId))
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"initialStatus":%d,"setStatus":%d,"reapplication":%s,' ..
            '"threshold":%d,"roll":%d,"success":%s,"reactionStatus":%d,' ..
            '"accumulatedExp":%d,"attackBonus":%d,"statusAfterConstruction":%d,' ..
            '"currentAttAfterConstruction":%d}',
            record.combatant, record.initialStatus, record.setStatus,
            tostring(record.reapplication), record.threshold, record.roll,
            tostring(record.success), record.reactionStatus, record.accumulatedExp,
            record.attackBonus, record.statusAfterConstruction,
            record.currentAttAfterConstruction))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-attack-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-attack-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-attack-flag", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    local fresh = config.case.targets[1]
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, 20, "M68K BUS")
    memory.write_u8(actor + 17, 20, "M68K BUS")
    memory.write_u8(actor + 18, fresh.baseAtt, "M68K BUS")
    memory.write_u8(actor + 19, fresh.initialCurrentAtt, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, fresh.initialStatus, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local recast = config.case.targets[2]
    local ally = entry(recast.combatant)
    memory.write_u8(ally + 10, 1, "M68K BUS")
    memory.write_u8(ally + 11, 1, "M68K BUS")
    memory.write_u16_be(ally + 12, 100, "M68K BUS")
    memory.write_u16_be(ally + 14, 100, "M68K BUS")
    memory.write_u8(ally + 18, recast.baseAtt, "M68K BUS")
    memory.write_u8(ally + 19, recast.initialCurrentAtt, "M68K BUS")
    memory.write_u16_be(ally + 44, recast.initialStatus, "M68K BUS")

    local enemy = entry(config.case.schedulingTarget)
    memory.write_u8(enemy + 11, 1, "M68K BUS")
    memory.write_u16_be(enemy + 12, 100, "M68K BUS")
    memory.write_u16_be(enemy + 14, 100, "M68K BUS")
    memory.write_u8(enemy + 46, 8, "M68K BUS")
    memory.write_u8(enemy + 47, 17, "M68K BUS")
    memory.write_u8(enemy + 49, 0x60, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-attack-turn", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u8(entry(config.case.actor) + 23, 23, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.actor, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-attack-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or targets_supplied then return end
    targets_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, #config.case.targets, "M68K BUS")
    for index, target in ipairs(config.case.targets) do
        memory.write_u8(config.ram.targetsListAddress + index - 1, target.combatant, "M68K BUS")
    end
end, config["function"].initializePropertiesAddress, "sf2-attack-targets", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local combatant = memory.read_u8(emu.getregister("M68K A5") & 0xFFFFFF, "M68K BUS")
    local address = entry(combatant)
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    active = {
        combatant=combatant,
        initialStatus=memory.read_u16_be(address + 44, "M68K BUS"),
        setStatus=0, reapplication=false, threshold=0, roll=-1,
        success=true, reactionStatus=0,
        accumulatedExp=memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS"),
        attackBonus=0
    }
end, config["function"].attackEffectEntryAddress, "sf2-attack-effect", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.setStatus = memory.read_u16_be(entry(active.combatant) + 44, "M68K BUS")
    active.reapplication = active.initialStatus ~= 0
end, config["function"].attackStatusSetAddress, "sf2-attack-status", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.threshold = emu.getregister("M68K D2") & 0xFFFF end
end, config["function"].attackThresholdAddress, "sf2-attack-threshold", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.roll = emu.getregister("M68K D0") & 0xFFFF end
end, config["function"].effectivenessRollAddress, "sf2-attack-roll", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.reactionStatus = emu.getregister("M68K D1") & 0xFFFF end
end, config["function"].attackReactionAddress, "sf2-attack-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].statusExpAppliedAddress, "sf2-attack-exp", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.attackBonus = emu.getregister("M68K D1") & 0xFFFF end
end, config["function"].attackBonusAddress, "sf2-attack-bonus", "M68K BUS")

local function finish_active(success)
    if active == nil then return end
    active.success = success
    local address = entry(active.combatant)
    active.statusAfterConstruction = memory.read_u16_be(address + 44, "M68K BUS")
    active.currentAttAfterConstruction = memory.read_u8(address + 19, "M68K BUS")
    records[#records + 1] = active
    active = nil
    if #records == #config.case.targets then write_result_and_exit() end
end

event.on_bus_exec(function()
    finish_active(false)
end, config["function"].effectivenessFailureAddress, "sf2-attack-failure", "M68K BUS")

event.on_bus_exec(function()
    finish_active(true)
end, config["function"].attackEffectReturnAddress, "sf2-attack-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.harness.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress, "M68K BUS") == 255 then button = "Up" end
    elseif #queue > 0 then button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then button = "C" end
    set_button(button)
    joypad.set({ Start = stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d,stage=%s,pc=%X,records=%d", frames, stage, emu.getregister("M68K PC"), #records)) end
end
