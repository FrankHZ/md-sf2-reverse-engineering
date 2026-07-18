local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, target_supplied, playback = false, false, false
local records, active_record = {}, nil
local award, exp_reaction = nil, nil
local ally_reactions, active_reaction, reaction_order = {}, nil, {}
local construction_actor_mp, target_same_side = nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "a")); file:write(value .. "\n"); file:close()
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

local function write_records(output)
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"initialStatus":%d,"setStatus":%d,"reapplication":%s,' ..
            '"threshold":%d,"roll":%d,"success":%s,"reactionStatus":%d,' ..
            '"accumulatedExp":%d,"agiBonus":%d,"defBonus":%d,' ..
            '"statusAfterConstruction":%d,"currentDefAfterConstruction":%d,' ..
            '"currentAgiAfterConstruction":%d}',
            record.combatant, record.initialStatus, record.setStatus,
            tostring(record.reapplication), record.threshold, record.roll,
            tostring(record.success), record.reactionStatus, record.accumulatedExp,
            record.agiBonus, record.defBonus, record.statusAfterConstruction,
            record.currentDefAfterConstruction, record.currentAgiAfterConstruction))
    end
end

local function write_reactions(output)
    for index, reaction in ipairs(ally_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"mpChange":%d,"statusCommand":%d,"mpBefore":%d,' ..
            '"mpAfter":%d,"statusBefore":%d,"statusAfter":%d,"defBefore":%d,' ..
            '"defAfter":%d,"agiBefore":%d,"agiAfter":%d}',
            reaction.combatant, reaction.mpChange, reaction.statusCommand,
            reaction.mpBefore, reaction.mpAfter, reaction.statusBefore,
            reaction.statusAfter, reaction.defBefore, reaction.defAfter,
            reaction.agiBefore, reaction.agiAfter))
    end
end

local function write_targets(output)
    for index, target in ipairs(config.case.targets) do
        if index > 1 then output:write(",") end
        local address = entry(target.combatant)
        output:write(string.format(
            '{"combatant":%d,"status":%d,"currentDef":%d,"currentAgi":%d}',
            target.combatant,
            memory.read_u16_be(address + 44, "M68K BUS"),
            memory.read_u8(address + 21, "M68K BUS"),
            memory.read_u8(address + 23, "M68K BUS")))
    end
end

local function write_result_and_exit()
    local actor = entry(config.case.actor)
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"target":%d},' ..
        '"construction":{"actorMp":%d,"records":[',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress + 2, "M68K BUS"),
        config.case.actor, construction_actor_mp))
    write_records(output)
    output:write(string.format(
        '],"targetSameSide":%s,"award":{"seed":%d,"halved":%d,' ..
        '"firstRoll":%d,"secondRoll":%d,"commandExp":%d}},' ..
        '"replay":{"reactionOrder":[',
        tostring(target_same_side), award.seed, award.halved,
        award.firstRoll, award.secondRoll, award.commandExp))
    for index, item in ipairs(reaction_order) do
        if index > 1 then output:write(",") end
        output:write('"' .. item .. '"')
    end
    output:write('],"allyReactions":[')
    write_reactions(output)
    output:write(string.format(
        '],"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},' ..
        '"finalActorMp":%d,"finalActorExp":%d,"finalTargets":[',
        exp_reaction.commandExp, exp_reaction.expBefore, exp_reaction.expAfter,
        memory.read_u8(actor + 17, "M68K BUS"), memory.read_u8(actor + 48, "M68K BUS")))
    write_targets(output)
    output:write(']}}\n')
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-boost-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-boost-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-boost-flag", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    local fresh = config.case.targets[1]
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 20, fresh.baseDef, "M68K BUS")
    memory.write_u8(actor + 21, fresh.initialCurrentDef, "M68K BUS")
    memory.write_u8(actor + 22, fresh.baseAgi, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, fresh.initialStatus, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local recast = config.case.targets[2]
    local ally = entry(recast.combatant)
    memory.write_u8(ally + 10, 1, "M68K BUS")
    memory.write_u8(ally + 11, 1, "M68K BUS")
    memory.write_u16_be(ally + 12, 100, "M68K BUS")
    memory.write_u16_be(ally + 14, 100, "M68K BUS")
    memory.write_u8(ally + 16, 10, "M68K BUS")
    memory.write_u8(ally + 17, 10, "M68K BUS")
    memory.write_u8(ally + 20, recast.baseDef, "M68K BUS")
    memory.write_u8(ally + 21, recast.initialCurrentDef, "M68K BUS")
    memory.write_u8(ally + 22, recast.baseAgi, "M68K BUS")
    memory.write_u8(ally + 23, recast.initialCurrentAgi, "M68K BUS")
    memory.write_u8(ally + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(ally + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(ally + 44, recast.initialStatus, "M68K BUS")
    memory.write_u8(ally + 48, 0, "M68K BUS")

    local enemy = entry(config.case.schedulingTarget)
    memory.write_u8(enemy + 11, 1, "M68K BUS")
    memory.write_u16_be(enemy + 12, 100, "M68K BUS")
    memory.write_u16_be(enemy + 14, 100, "M68K BUS")
    memory.write_u16_be(enemy + 26, 0, "M68K BUS")
    memory.write_u16_be(enemy + 28, 0, "M68K BUS")
    memory.write_u8(enemy + 46, 8, "M68K BUS")
    memory.write_u8(enemy + 47, 17, "M68K BUS")
    memory.write_u8(enemy + 49, 0x60, "M68K BUS")
    memory.write_u8(enemy + 55, 39, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-boost-turn", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    local actor = entry(config.case.actor)
    local fresh = config.case.targets[1]
    memory.write_u8(actor + 21, fresh.initialCurrentDef, "M68K BUS")
    memory.write_u8(actor + 23, fresh.initialCurrentAgi, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.actor, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-boost-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, #config.case.targets, "M68K BUS")
    for index, target in ipairs(config.case.targets) do
        memory.write_u8(config.ram.targetsListAddress + index - 1, target.combatant, "M68K BUS")
    end
end, config["function"].initializePropertiesAddress, "sf2-boost-targets", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local target = config.case.targets[#records + 1]
    if target == nil then return end
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    local address = entry(target.combatant)
    active_record = {
        combatant = target.combatant,
        initialStatus = memory.read_u16_be(address + 44, "M68K BUS"),
        setStatus = 0,
        reapplication = false,
        threshold = 0,
        roll = -1,
        success = true,
        reactionStatus = 0,
        accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS"),
        agiBonus = 0,
        defBonus = 0
    }
end, config["function"].boostEffectEntryAddress, "sf2-boost-effect", "M68K BUS")

event.on_bus_exec(function()
    if active_record == nil then return end
    local target = entry(active_record.combatant)
    active_record.setStatus = memory.read_u16_be(target + 44, "M68K BUS")
    active_record.reapplication = (active_record.initialStatus & config.case.statusMask) ~= 0
end, config["function"].boostStatusSetAddress, "sf2-boost-status-set", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil and active_record.reapplication then
        active_record.threshold = word_register("M68K D2")
    end
end, config["function"].effectivenessEntryAddress, "sf2-boost-effective", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil and active_record.reapplication then
        active_record.roll = word_register("M68K D0")
    end
end, config["function"].effectivenessRollAddress, "sf2-boost-roll", "M68K BUS")

event.on_bus_exec(function()
    if active_record == nil or not active_record.reapplication then return end
    active_record.success = false
    active_record.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    records[#records + 1] = active_record
    active_record = nil
end, config["function"].effectivenessFailureAddress, "sf2-boost-failure", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil then active_record.reactionStatus = word_register("M68K D1") end
end, config["function"].boostReactionAddress, "sf2-boost-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil then
        active_record.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].statusExpAppliedAddress, "sf2-boost-exp", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil then active_record.agiBonus = word_register("M68K D1") end
end, config["function"].boostAgiBonusAddress, "sf2-boost-agi", "M68K BUS")

event.on_bus_exec(function()
    if active_record ~= nil then active_record.defBonus = word_register("M68K D1") end
end, config["function"].boostDefBonusAddress, "sf2-boost-def", "M68K BUS")

event.on_bus_exec(function()
    if active_record == nil then return end
    records[#records + 1] = active_record
    active_record = nil
end, config["function"].boostEffectReturnAddress, "sf2-boost-effect-return", "M68K BUS")

event.on_bus_exec(function()
    if #records ~= #config.case.targets then return end
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    target_same_side = memory.read_u8(a2 - 7, "M68K BUS") ~= 0
end, config["function"].sameSideDecisionAddress, "sf2-boost-side", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    award = { seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS"), halved = word_register("M68K D1") }
end, config["function"].expHalvedAddress, "sf2-boost-half", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-boost-first", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-boost-second", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-boost-final", "M68K BUS")

event.on_bus_exec(function()
    if #records ~= #config.case.targets then return end
    for index, record in ipairs(records) do
        local target = entry(record.combatant)
        record.statusAfterConstruction = memory.read_u16_be(target + 44, "M68K BUS")
        record.currentDefAfterConstruction = memory.read_u8(target + 21, "M68K BUS")
        record.currentAgiAfterConstruction = memory.read_u8(target + 23, "M68K BUS")
    end
    construction_actor_mp = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    playback = true
end, config["function"].battleSceneEndReturnAddress, "sf2-boost-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local combatant = memory.read_u16_be(config.ram.battleSceneAllyAddress, "M68K BUS")
    local address = entry(combatant)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    active_reaction = {
        combatant = combatant,
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        statusCommand = memory.read_u16_be(a6 + 4, "M68K BUS"),
        mpBefore = memory.read_u8(address + 17, "M68K BUS"),
        statusBefore = memory.read_u16_be(address + 44, "M68K BUS"),
        defBefore = memory.read_u8(address + 21, "M68K BUS"),
        agiBefore = memory.read_u8(address + 23, "M68K BUS")
    }
    reaction_order[#reaction_order + 1] = string.format(
        "ally:%d:%d", active_reaction.mpChange, active_reaction.statusCommand)
end, config["function"].allyReactionEntryAddress, "sf2-boost-ally", "M68K BUS")

event.on_bus_exec(function()
    if active_reaction == nil then return end
    local address = entry(active_reaction.combatant)
    active_reaction.mpAfter = memory.read_u8(address + 17, "M68K BUS")
    active_reaction.statusAfter = memory.read_u16_be(address + 44, "M68K BUS")
    active_reaction.defAfter = memory.read_u8(address + 21, "M68K BUS")
    active_reaction.agiAfter = memory.read_u8(address + 23, "M68K BUS")
    ally_reactions[#ally_reactions + 1] = active_reaction
    active_reaction = nil
end, config["function"].allyStatsAppliedAddress, "sf2-boost-ally-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local actor = entry(config.case.actor)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    exp_reaction = {
        commandExp = memory.read_u16_be(a6, "M68K BUS"),
        expBefore = memory.read_u8(actor + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-boost-give", "M68K BUS")

event.on_bus_exec(function()
    if exp_reaction ~= nil then
        exp_reaction.expAfter = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-boost-give-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback and #ally_reactions == 2 and exp_reaction ~= nil and exp_reaction.expAfter ~= nil then
        write_result_and_exit()
    end
end, config["function"].executeScriptEndAddress, "sf2-boost-script-end", "M68K BUS")

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
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    elseif playback and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1) or playback) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,records=%d,reactions=%d", frames, stage,
            emu.getregister("M68K PC"), #records, #ally_reactions))
    end
end
