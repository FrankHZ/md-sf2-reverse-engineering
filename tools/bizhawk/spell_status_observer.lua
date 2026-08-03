local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local targets_supplied = false
local playback = false
local records = {}
local active = nil
local construction_actor_mp = nil
local award = nil
local ally_reaction = nil
local active_enemy_reaction = nil
local enemy_reactions = {}
local exp_reaction = nil
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

local function signed_word(value)
    if value >= 0x8000 then return value - 0x10000 end
    return value
end

local function write_result_and_exit()
    local actor = entry(config.case.actor)
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"targetCount":%d},' ..
        '"construction":{"actorMp":%d,"records":[',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress + 2, "M68K BUS"),
        #records, construction_actor_mp))
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"setting":%d,"threshold":%d,"roll":%d,' ..
            '"success":%s,"accumulatedExp":%d,"statusDuringConstruction":%d}',
            record.combatant, record.setting, record.threshold, record.roll,
            tostring(record.success), record.accumulatedExp, record.statusDuringConstruction))
    end
    output:write(string.format(
        '],"award":{"accumulatedExp":%d,"seed":%d,"halved":%d,"firstRoll":%d,' ..
        '"secondRoll":%d,"commandExp":%d}},"replay":{' ..
        '"allyReaction":{"mpChange":%d,"mpBefore":%d,"mpAfter":%d},' ..
        '"enemyReactions":[',
        award.accumulatedExp, award.seed, award.halved, award.firstRoll,
        award.secondRoll, award.commandExp,
        ally_reaction.mpChange, ally_reaction.mpBefore, ally_reaction.mpAfter))
    for index, reaction in ipairs(enemy_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"statusBefore":%d,"statusAfter":%d}',
            reaction.combatant, reaction.statusBefore, reaction.statusAfter))
    end
    output:write(string.format(
        '],"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},' ..
        '"finalActorMp":%d,"finalActorExp":%d,"finalTargetStatus":[',
        exp_reaction.commandExp, exp_reaction.expBefore, exp_reaction.expAfter,
        memory.read_u8(actor + 17, "M68K BUS"), memory.read_u8(actor + 48, "M68K BUS")))
    for index, target in ipairs(config.case.targets) do
        if index > 1 then output:write(",") end
        output:write(tostring(memory.read_u16_be(entry(target.combatant) + 44, "M68K BUS")))
    end
    output:write("]}}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-status-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-status-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt"); pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-status-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, config.case.initialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.initialMp, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u16_be(actor + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 38, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 44, 0, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")
    for _, target in ipairs(config.case.targets) do
        local address = entry(target.combatant)
        memory.write_u8(address + 11, 1, "M68K BUS")
        memory.write_u16_be(address + 12, 100, "M68K BUS")
        memory.write_u16_be(address + 14, 100, "M68K BUS")
        memory.write_u16_be(address + 26, target.resistanceWord, "M68K BUS")
        memory.write_u16_be(address + 28, target.resistanceWord, "M68K BUS")
        memory.write_u16_be(address + 44, target.initialStatus or 0, "M68K BUS")
        memory.write_u8(address + 46, 8, "M68K BUS")
        memory.write_u8(address + 47, 17, "M68K BUS")
        memory.write_u8(address + 49, 0x60, "M68K BUS")
        memory.write_u8(address + 55, 39, "M68K BUS")
    end
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    status("milestone:battle-setup")
end, config.harness["function"].turnOrderEntryAddress, "sf2-status-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.targets[1].combatant, "M68K BUS")
    status("milestone:sleep-action")
end, config["function"].writeBattleSceneScriptAddress, "sf2-status-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or targets_supplied then return end
    targets_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, #config.case.targets, "M68K BUS")
    for index, target in ipairs(config.case.targets) do
        memory.write_u8(config.ram.targetsListAddress + index - 1, target.combatant, "M68K BUS")
    end
end, config["function"].initializePropertiesAddress, "sf2-status-targets", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local combatant = memory.read_u8(emu.getregister("M68K A5") & 0xFFFFFF, "M68K BUS")
    local target_case = nil
    for _, target in ipairs(config.case.targets) do
        if target.combatant == combatant then target_case = target; break end
    end
    assert(target_case ~= nil, "status target is absent from fixture cases")
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    active = { combatant = combatant, setting = target_case.setting }
    records[#records + 1] = active
end, config["function"].sleepEffectEntryAddress, "sf2-status-sleep", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.threshold = word_register("M68K D2") end
end, config["function"].effectivenessEntryAddress, "sf2-status-effectiveness", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.roll = word_register("M68K D0") end
end, config["function"].effectivenessRollAddress, "sf2-status-roll", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.success = false
        active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].effectivenessFailureAddress, "sf2-status-failure", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.success = true end
end, config["function"].effectivenessSuccessAddress, "sf2-status-success", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].statusExpAppliedAddress, "sf2-status-exp", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    award = {
        accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS"),
        seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS"),
        halved = word_register("M68K D1")
    }
end, config["function"].expHalvedAddress, "sf2-status-award-halved", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-status-award-first", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-status-award-second", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-status-award-final", "M68K BUS")

event.on_bus_exec(function()
    if #records ~= #config.case.targets then return end
    for _, record in ipairs(records) do
        record.statusDuringConstruction = memory.read_u16_be(entry(record.combatant) + 44, "M68K BUS")
    end
    construction_actor_mp = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    playback = true
end, config["function"].battleSceneEndReturnAddress, "sf2-status-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    ally_reaction = {
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        mpBefore = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    }
end, config["function"].allyReactionEntryAddress, "sf2-status-ally", "M68K BUS")

event.on_bus_exec(function()
    if ally_reaction ~= nil then
        ally_reaction.mpAfter = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    end
end, config["function"].allyReactionAppliedAddress, "sf2-status-ally-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local combatant = memory.read_u16_be(config.ram.battleSceneEnemyAddress, "M68K BUS")
    active_enemy_reaction = {
        combatant = combatant,
        statusBefore = memory.read_u16_be(entry(combatant) + 44, "M68K BUS")
    }
end, config["function"].enemyReactionEntryAddress, "sf2-status-enemy", "M68K BUS")

event.on_bus_exec(function()
    if active_enemy_reaction == nil then return end
    active_enemy_reaction.statusAfter = memory.read_u16_be(
        entry(active_enemy_reaction.combatant) + 44, "M68K BUS")
    enemy_reactions[#enemy_reactions + 1] = active_enemy_reaction
    active_enemy_reaction = nil
end, config["function"].statusReactionAppliedAddress, "sf2-status-enemy-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    exp_reaction = {
        commandExp = memory.read_u16_be(a6, "M68K BUS"),
        expBefore = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-status-give-exp", "M68K BUS")

event.on_bus_exec(function()
    if exp_reaction ~= nil then
        exp_reaction.expAfter = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-status-give-exp-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback and ally_reaction ~= nil and ally_reaction.mpAfter ~= nil
        and #enemy_reactions == 3 and exp_reaction ~= nil and exp_reaction.expAfter ~= nil then
        write_result_and_exit()
    end
end, config["function"].executeScriptEndAddress, "sf2-status-script-end", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.harness.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress, "M68K BUS") == 255 then
            button = "Up"
        end
    elseif #queue > 0 then button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then button = "C"
    elseif stage == "battle" and playback and frames % 12 < 4 then button = "C" end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1) or playback) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,records=%d,reactions=%d", frames, stage,
            emu.getregister("M68K PC"), #records, #enemy_reactions))
    end
end
