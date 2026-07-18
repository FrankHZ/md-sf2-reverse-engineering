local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, targets_supplied, playback = false, false, false
local records, active = {}, nil
local award, target_same_side = nil, nil
local ally_reaction, active_enemy, enemy_reactions = nil, nil, {}
local exp_reaction, reaction_order = nil, {}
local construction_actor_mp = nil
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

local function target_case(combatant)
    for _, target in ipairs(config.case.targets) do
        if target.combatant == combatant then return target end
    end
    error("SLOW target is absent from fixture")
end

local function write_records(output)
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"setting":%d,"threshold":%d,"roll":%d,"success":%s,' ..
            '"setStatus":%d,"reactionStatus":%d,"accumulatedExp":%d,' ..
            '"agiPenalty":%d,"defPenalty":%d,"statusAfterConstruction":%d,' ..
            '"currentDefAfterConstruction":%d,"currentAgiAfterConstruction":%d}',
            record.combatant, record.setting, record.threshold, record.roll,
            tostring(record.success), record.setStatus, record.reactionStatus,
            record.accumulatedExp, record.agiPenalty, record.defPenalty,
            record.statusAfterConstruction, record.currentDefAfterConstruction,
            record.currentAgiAfterConstruction))
    end
end

local function write_enemy_reactions(output)
    for index, reaction in ipairs(enemy_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"statusCommand":%d,"statusBefore":%d,"statusAfter":%d,' ..
            '"defBefore":%d,"defAfter":%d,"agiBefore":%d,"agiAfter":%d}',
            reaction.combatant, reaction.statusCommand, reaction.statusBefore,
            reaction.statusAfter, reaction.defBefore, reaction.defAfter,
            reaction.agiBefore, reaction.agiAfter))
    end
end

local function write_final_targets(output)
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
        config.case.targets[1].combatant, construction_actor_mp))
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
    output:write(string.format(
        '],"allyReaction":{"combatant":%d,"mpChange":%d,"statusCommand":%d,' ..
        '"mpBefore":%d,"mpAfter":%d},"enemyReactions":[',
        ally_reaction.combatant, ally_reaction.mpChange, ally_reaction.statusCommand,
        ally_reaction.mpBefore, ally_reaction.mpAfter))
    write_enemy_reactions(output)
    output:write(string.format(
        '],"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},' ..
        '"finalActorMp":%d,"finalActorExp":%d,"finalTargets":[',
        exp_reaction.commandExp, exp_reaction.expBefore, exp_reaction.expAfter,
        memory.read_u8(actor + 17, "M68K BUS"), memory.read_u8(actor + 48, "M68K BUS")))
    write_final_targets(output)
    output:write(']}}\n')
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-slow-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-slow-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-slow-flag", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, 0, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")
    for _, target in ipairs(config.case.targets) do
        local address = entry(target.combatant)
        memory.write_u8(address + 11, 1, "M68K BUS")
        memory.write_u16_be(address + 12, 100, "M68K BUS")
        memory.write_u16_be(address + 14, 100, "M68K BUS")
        memory.write_u8(address + 20, target.baseDef, "M68K BUS")
        memory.write_u8(address + 21, target.baseDef, "M68K BUS")
        memory.write_u8(address + 22, target.baseAgi, "M68K BUS")
        memory.write_u8(address + 23, target.baseAgi, "M68K BUS")
        memory.write_u16_be(address + 26, target.resistanceWord, "M68K BUS")
        memory.write_u16_be(address + 28, target.resistanceWord, "M68K BUS")
        memory.write_u16_be(address + 44, 0, "M68K BUS")
        memory.write_u8(address + 46, 8, "M68K BUS")
        memory.write_u8(address + 47, 17, "M68K BUS")
        memory.write_u8(address + 49, 0x60, "M68K BUS")
        memory.write_u8(address + 55, 39, "M68K BUS")
    end
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-slow-turn", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.targets[1].combatant, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-slow-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or targets_supplied then return end
    targets_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, #config.case.targets, "M68K BUS")
    for index, target in ipairs(config.case.targets) do
        memory.write_u8(config.ram.targetsListAddress + index - 1, target.combatant, "M68K BUS")
    end
end, config["function"].initializePropertiesAddress, "sf2-slow-targets", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local combatant = memory.read_u8(emu.getregister("M68K A5") & 0xFFFFFF, "M68K BUS")
    local target = target_case(combatant)
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    active = {
        combatant = combatant, setting = target.setting, threshold = 0, roll = -1,
        success = false, setStatus = 0, reactionStatus = 0,
        accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS"),
        agiPenalty = 0, defPenalty = 0
    }
    records[#records + 1] = active
end, config["function"].slowEffectEntryAddress, "sf2-slow-effect", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.threshold = word_register("M68K D2") end
end, config["function"].slowThresholdAddress, "sf2-slow-threshold", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.roll = word_register("M68K D0") end
end, config["function"].effectivenessRollAddress, "sf2-slow-roll", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.success = false
    active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    active = nil
end, config["function"].effectivenessFailureAddress, "sf2-slow-failure", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.success = true end
end, config["function"].effectivenessSuccessAddress, "sf2-slow-success", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.setStatus = memory.read_u16_be(entry(active.combatant) + 44, "M68K BUS")
    end
end, config["function"].slowStatusSetAddress, "sf2-slow-status-set", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.reactionStatus = word_register("M68K D1") end
end, config["function"].slowReactionAddress, "sf2-slow-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].statusExpAppliedAddress, "sf2-slow-exp", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.agiPenalty = word_register("M68K D1") end
end, config["function"].slowAgiBonusAddress, "sf2-slow-agi", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.defPenalty = word_register("M68K D1") end
end, config["function"].slowDefBonusAddress, "sf2-slow-def", "M68K BUS")

event.on_bus_exec(function()
    active = nil
end, config["function"].slowEffectReturnAddress, "sf2-slow-return", "M68K BUS")

event.on_bus_exec(function()
    if #records ~= #config.case.targets then return end
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    target_same_side = memory.read_u8(a2 - 7, "M68K BUS") ~= 0
end, config["function"].sameSideDecisionAddress, "sf2-slow-side", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    award = { seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS"), halved = word_register("M68K D1") }
end, config["function"].expHalvedAddress, "sf2-slow-half", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-slow-first", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-slow-second", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-slow-final", "M68K BUS")

event.on_bus_exec(function()
    if #records ~= #config.case.targets then return end
    for _, record in ipairs(records) do
        local address = entry(record.combatant)
        record.statusAfterConstruction = memory.read_u16_be(address + 44, "M68K BUS")
        record.currentDefAfterConstruction = memory.read_u8(address + 21, "M68K BUS")
        record.currentAgiAfterConstruction = memory.read_u8(address + 23, "M68K BUS")
    end
    construction_actor_mp = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    playback = true
end, config["function"].battleSceneEndReturnAddress, "sf2-slow-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local combatant = memory.read_u16_be(config.ram.battleSceneAllyAddress, "M68K BUS")
    local address = entry(combatant)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    ally_reaction = {
        combatant = combatant,
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        statusCommand = memory.read_u16_be(a6 + 4, "M68K BUS"),
        mpBefore = memory.read_u8(address + 17, "M68K BUS")
    }
    reaction_order[#reaction_order + 1] = string.format(
        "ally:%d:%d", ally_reaction.mpChange, ally_reaction.statusCommand)
end, config["function"].allyReactionEntryAddress, "sf2-slow-ally", "M68K BUS")

event.on_bus_exec(function()
    if ally_reaction ~= nil then
        ally_reaction.mpAfter = memory.read_u8(entry(ally_reaction.combatant) + 17, "M68K BUS")
    end
end, config["function"].allyReactionAppliedAddress, "sf2-slow-ally-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local combatant = memory.read_u16_be(config.ram.battleSceneEnemyAddress, "M68K BUS")
    local address = entry(combatant)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    active_enemy = {
        combatant = combatant,
        statusCommand = memory.read_u16_be(a6 + 4, "M68K BUS"),
        statusBefore = memory.read_u16_be(address + 44, "M68K BUS"),
        defBefore = memory.read_u8(address + 21, "M68K BUS"),
        agiBefore = memory.read_u8(address + 23, "M68K BUS")
    }
    reaction_order[#reaction_order + 1] = string.format(
        "enemy:%d:%d", combatant, active_enemy.statusCommand)
end, config["function"].enemyReactionEntryAddress, "sf2-slow-enemy", "M68K BUS")

event.on_bus_exec(function()
    if active_enemy == nil then return end
    local address = entry(active_enemy.combatant)
    active_enemy.statusAfter = memory.read_u16_be(address + 44, "M68K BUS")
    active_enemy.defAfter = memory.read_u8(address + 21, "M68K BUS")
    active_enemy.agiAfter = memory.read_u8(address + 23, "M68K BUS")
    enemy_reactions[#enemy_reactions + 1] = active_enemy
    active_enemy = nil
end, config["function"].enemyStatsAppliedAddress, "sf2-slow-enemy-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local actor = entry(config.case.actor)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    exp_reaction = {
        commandExp = memory.read_u16_be(a6, "M68K BUS"),
        expBefore = memory.read_u8(actor + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-slow-give", "M68K BUS")

event.on_bus_exec(function()
    if exp_reaction ~= nil then
        exp_reaction.expAfter = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-slow-give-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback and #enemy_reactions == 3 and exp_reaction ~= nil and exp_reaction.expAfter ~= nil then
        write_result_and_exit()
    end
end, config["function"].executeScriptEndAddress, "sf2-slow-script-end", "M68K BUS")

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
            emu.getregister("M68K PC"), #records, #enemy_reactions))
    end
end
