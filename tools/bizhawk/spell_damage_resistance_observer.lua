local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local targets_supplied = false
local playback = false
local resistance_calls = 0
local division_calls = 0
local records = {}
local active = nil
local ally_reactions = {}
local enemy_reactions = {}
local active_ally_reaction = nil
local active_enemy_reaction = nil
local construction_actor_mp = nil
local award = nil
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
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"baseSpell":%d,"targetCount":%d},' ..
        '"construction":{"resistanceCalls":%d,"divisionCalls":%d,"actorMp":%d,"records":[',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress + 2, "M68K BUS"),
        memory.read_u16_be(config.ram.battleSceneSpellIndexAddress, "M68K BUS"),
        #records, resistance_calls, division_calls, construction_actor_mp))
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format('{"combatant":%d', record.combatant))
        if record.divisionSpell ~= nil then
            output:write(string.format(',"divisionSpell":%d', record.divisionSpell))
        end
        output:write(string.format(
            ',"setting":%d,"casterClass":%d,"preDivisionPower":%d,"adjustedPower":%d,"quarterPower":%d,' ..
            '"postResistance":%d,"criticalRoll":%d,"criticalFlag":%d,"preVariance":%d,"varianceRange":%d,' ..
            '"varianceRolls":[%d,%d],"finalDamage":%d,"accumulatedExp":%d,"temporaryHp":%d,"restoredHp":%d}',
            record.setting, record.casterClass, record.preDivisionPower,
            record.adjustedPower, record.quarterPower,
            record.postResistance, record.criticalRoll, record.criticalFlag,
            record.preVariance, record.varianceRange,
            record.varianceRolls[1], record.varianceRolls[2], record.finalDamage,
            record.accumulatedExp, record.temporaryHp, record.restoredHp))
    end
    output:write(string.format(
        '],"award":{"accumulatedExp":%d,"seed":%d,"halved":%d,"firstRoll":%d,' ..
        '"secondRoll":%d,"commandExp":%d}},"replay":{"allyReactions":[',
        award.accumulatedExp, award.seed, award.halved, award.firstRoll,
        award.secondRoll, award.commandExp))
    for index, reaction in ipairs(ally_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"hpChange":%d,"mpChange":%d,"mpBefore":%d,"mpAfter":%d}',
            reaction.combatant, reaction.hpChange, reaction.mpChange,
            reaction.mpBefore, reaction.mpAfter))
    end
    output:write('],"enemyReactions":[')
    for index, reaction in ipairs(enemy_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"combatant":%d,"hpChange":%d,"hpBefore":%d,"hpAfter":%d}',
            reaction.combatant, reaction.hpChange, reaction.hpBefore, reaction.hpAfter))
    end
    output:write(string.format(
        '],"finalActorMp":%d,"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},' ..
        '"finalActorExp":%d,"finalTargetHp":[',
        memory.read_u8(entry(config.case.actor) + 17, "M68K BUS"),
        exp_reaction.commandExp, exp_reaction.expBefore, exp_reaction.expAfter,
        memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")))
    for index, target in ipairs(config.case.targets) do
        if index > 1 then output:write(",") end
        output:write(tostring(memory.read_u16_be(entry(target.combatant) + 14, "M68K BUS")))
    end
    output:write("]}}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-spell-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-spell-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-spell-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, config.case.initialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.initialMp, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u16_be(actor + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 38, 0x007F, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")
    for _, target in ipairs(config.case.targets) do
        local address = entry(target.combatant)
        memory.write_u8(address + 11, 1, "M68K BUS")
        memory.write_u16_be(address + 12, 100, "M68K BUS")
        memory.write_u16_be(address + 14, 100, "M68K BUS")
        memory.write_u16_be(address + 26, target.resistanceWord, "M68K BUS")
        memory.write_u16_be(address + 28, target.resistanceWord, "M68K BUS")
        memory.write_u8(address + 46, 8, "M68K BUS")
        memory.write_u8(address + 47, 17, "M68K BUS")
        memory.write_u8(address + 49, 0x60, "M68K BUS")
        memory.write_u8(address + 55, 39, "M68K BUS")
    end
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    status("milestone:battle-setup")
end, config.harness["function"].turnOrderEntryAddress, "sf2-spell-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.targets[1].combatant, "M68K BUS")
    status("milestone:spell-action")
end, config["function"].writeBattleSceneScriptAddress, "sf2-spell-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or targets_supplied then return end
    targets_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, #config.case.targets, "M68K BUS")
    for index, target in ipairs(config.case.targets) do
        memory.write_u8(config.ram.targetsListAddress + index - 1, target.combatant, "M68K BUS")
    end
    status("milestone:targets-supplied")
end, config["function"].initializePropertiesAddress, "sf2-spell-targets", "M68K BUS")

event.on_bus_exec(function()
    if action_started then resistance_calls = resistance_calls + 1 end
end, config["function"].getResistanceAddress, "sf2-spell-resistance", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local a5 = emu.getregister("M68K A5") & 0xFFFFFF
    local combatant = memory.read_u8(a5, "M68K BUS")
    local target_case = nil
    for _, target in ipairs(config.case.targets) do
        if target.combatant == combatant then target_case = target; break end
    end
    assert(target_case ~= nil, "spell target is absent from fixture cases")
    memory.write_u8(entry(config.case.actor) + 10, target_case.casterClass, "M68K BUS")
    if target_case.divisionSpell ~= nil then
        memory.write_u16_be(config.ram.battleSceneSpellIndexAddress, target_case.divisionSpell, "M68K BUS")
    end
    active = {
        combatant = combatant,
        setting = word_register("M68K D2"),
        casterClass = target_case.casterClass,
        varianceRolls = {}
    }
    memory.write_u16_be(config.ram.seedAddress, target_case.seed, "M68K BUS")
end, config["function"].calculateSpellDamageAddress, "sf2-spell-calculate", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.adjustedPower = word_register("M68K D6")
    if active.preDivisionPower == nil then active.preDivisionPower = active.adjustedPower end
    active.quarterPower = active.adjustedPower >> 2
end, config["function"].adjustedPowerAddress, "sf2-spell-adjusted", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    division_calls = division_calls + 1
    active.divisionSpell = memory.read_u16_be(config.ram.battleSceneSpellIndexAddress, "M68K BUS")
    active.preDivisionPower = word_register("M68K D6")
end, config["function"].divisionEntryAddress, "sf2-spell-division", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.postResistance = word_register("M68K D6") end
end, config["function"].postResistanceAddress, "sf2-spell-post-resistance", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.criticalRoll = word_register("M68K D0")
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    active.criticalFlag = memory.read_u8(a2 - 3, "M68K BUS")
    active.preVariance = word_register("M68K D6")
    active.varianceRange = (active.preVariance >> 3) + 1
end, config["function"].preVarianceAddress, "sf2-spell-pre-variance", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.varianceRolls[1] = word_register("M68K D0") end
end, config["function"].varianceFirstRollAddress, "sf2-spell-variance-1", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.varianceRolls[2] = word_register("M68K D0") end
end, config["function"].varianceSecondRollAddress, "sf2-spell-variance-2", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].damageExpAppliedAddress, "sf2-spell-damage-exp", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.finalDamage = word_register("M68K D6")
    active.temporaryHp = memory.read_u16_be(entry(active.combatant) + 14, "M68K BUS")
    if active.divisionSpell ~= nil then
        memory.write_u16_be(config.ram.battleSceneSpellIndexAddress, config.case.baseSpell, "M68K BUS")
    end
    records[#records + 1] = active
    active = nil
    status("milestone:damage:" .. #records)
end, config["function"].damageAppliedAddress, "sf2-spell-damage-applied", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or #records ~= #config.case.targets or playback then return end
    for _, record in ipairs(records) do
        record.restoredHp = memory.read_u16_be(entry(record.combatant) + 14, "M68K BUS")
    end
    construction_actor_mp = memory.read_u8(entry(config.case.actor) + 17, "M68K BUS")
    playback = true
    status("milestone:playback")
end, config["function"].battleSceneEndReturnAddress, "sf2-spell-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    local combatant = memory.read_u16_be(config.ram.battleSceneAllyAddress, "M68K BUS")
    active_ally_reaction = {
        combatant = combatant,
        hpChange = signed_word(memory.read_u16_be(a6, "M68K BUS")),
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        mpBefore = memory.read_u8(entry(combatant) + 17, "M68K BUS")
    }
end, config["function"].allyReactionEntryAddress, "sf2-spell-ally-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active_ally_reaction == nil then return end
    active_ally_reaction.mpAfter = memory.read_u8(entry(active_ally_reaction.combatant) + 17, "M68K BUS")
    ally_reactions[#ally_reactions + 1] = active_ally_reaction
    active_ally_reaction = nil
end, config["function"].allyReactionAppliedAddress, "sf2-spell-ally-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    local combatant = memory.read_u16_be(config.ram.battleSceneEnemyAddress, "M68K BUS")
    active_enemy_reaction = {
        combatant = combatant,
        hpChange = signed_word(memory.read_u16_be(a6, "M68K BUS")),
        hpBefore = memory.read_u16_be(entry(combatant) + 14, "M68K BUS")
    }
end, config["function"].enemyReactionEntryAddress, "sf2-spell-enemy-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active_enemy_reaction == nil then return end
    active_enemy_reaction.hpAfter = memory.read_u16_be(
        entry(active_enemy_reaction.combatant) + 14, "M68K BUS")
    enemy_reactions[#enemy_reactions + 1] = active_enemy_reaction
    active_enemy_reaction = nil
end, config["function"].enemyReactionAppliedAddress, "sf2-spell-enemy-applied", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    award = {
        accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS"),
        seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS"),
        halved = word_register("M68K D1")
    }
end, config["function"].expHalvedAddress, "sf2-spell-exp-halved", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-spell-exp-first", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-spell-exp-second", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-spell-exp-final", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    exp_reaction = {
        commandExp = memory.read_u16_be(a6, "M68K BUS"),
        expBefore = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-spell-give-exp", "M68K BUS")

event.on_bus_exec(function()
    if exp_reaction ~= nil then
        exp_reaction.expAfter = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-spell-exp-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback and #ally_reactions == 1 and #enemy_reactions == #config.case.targets
        and exp_reaction ~= nil and exp_reaction.expAfter ~= nil then
        write_result_and_exit()
    end
end, config["function"].executeScriptEndAddress, "sf2-spell-script-end", "M68K BUS")

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
    elseif stage == "battle" and playback and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1) or playback) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,records=%d,action=%d", frames, stage,
            emu.getregister("M68K PC"), #records,
            memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS")))
    end
end
