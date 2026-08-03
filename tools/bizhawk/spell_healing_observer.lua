local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local action_started = false
local target_supplied = false
local playback = false
local construction = nil
local award = nil
local ally_reactions = {}
local active_reaction = nil
local exp_reaction = nil
local heal_effect_calls = 0
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

local function write_result_and_exit()
    local actor = entry(config.case.actor)
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"target":%d},' ..
        '"construction":{"missingHp":%d,"basePower":%d,"adjustedPower":%d,' ..
        '"cappedRecovery":%d,"accumulatedExp":%d,"targetSameSide":%s,"actorHp":%d,"actorMp":%d,' ..
        '"award":{"seed":%d,"halved":%d,"firstRoll":%d,"secondRoll":%d,"commandExp":%d}},' ..
        '"replay":{"allyReactions":[',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress + 2, "M68K BUS"),
        config.case.target,
        construction.missingHp, construction.basePower, construction.adjustedPower,
        construction.cappedRecovery, construction.accumulatedExp,
        tostring(construction.targetSameSide),
        construction.actorHp, construction.actorMp,
        award.seed, award.halved, award.firstRoll, award.secondRoll, award.commandExp))
    for index, reaction in ipairs(ally_reactions) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"hpChange":%d,"mpChange":%d,"hpBefore":%d,"hpAfter":%d,' ..
            '"mpBefore":%d,"mpAfter":%d}',
            reaction.hpChange, reaction.mpChange, reaction.hpBefore, reaction.hpAfter,
            reaction.mpBefore, reaction.mpAfter))
    end
    output:write(string.format(
        '],"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},' ..
        '"finalActorHp":%d,"finalActorMp":%d,"finalActorExp":%d}}\n',
        exp_reaction.commandExp, exp_reaction.expBefore, exp_reaction.expAfter,
        memory.read_u16_be(actor + 14, "M68K BUS"),
        memory.read_u8(actor + 17, "M68K BUS"),
        memory.read_u8(actor + 48, "M68K BUS")))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-heal-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-heal-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-heal-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.case.actor)
    memory.write_u8(actor + 10, config.case.schedulingClass, "M68K BUS")
    memory.write_u8(actor + 11, config.case.actorLevel, "M68K BUS")
    memory.write_u16_be(actor + 12, config.case.actorMaxHp, "M68K BUS")
    memory.write_u16_be(actor + 14, config.case.schedulingHp, "M68K BUS")
    memory.write_u8(actor + 16, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u16_be(actor + 32, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 34, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 36, 0x007F, "M68K BUS")
    memory.write_u16_be(actor + 38, 0x007F, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")
    local scheduling_target = entry(config.case.schedulingTarget)
    memory.write_u8(scheduling_target + 11, 1, "M68K BUS")
    memory.write_u16_be(scheduling_target + 12, 100, "M68K BUS")
    memory.write_u16_be(scheduling_target + 14, 100, "M68K BUS")
    memory.write_u16_be(scheduling_target + 26, 0, "M68K BUS")
    memory.write_u16_be(scheduling_target + 28, 0, "M68K BUS")
    memory.write_u8(scheduling_target + 46, 8, "M68K BUS")
    memory.write_u8(scheduling_target + 47, 17, "M68K BUS")
    memory.write_u8(scheduling_target + 49, 0x60, "M68K BUS")
    memory.write_u8(scheduling_target + 55, 39, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    status("milestone:battle-setup")
end, config.harness["function"].turnOrderEntryAddress, "sf2-heal-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u16_be(entry(config.case.actor) + 14, config.case.actorInitialHp, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.target, "M68K BUS")
    status("milestone:heal-action")
end, config["function"].writeBattleSceneScriptAddress, "sf2-heal-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.case.target, "M68K BUS")
    status("milestone:target-supplied")
end, config["function"].initializePropertiesAddress, "sf2-heal-target", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    heal_effect_calls = heal_effect_calls + 1
    local actor = entry(config.case.actor)
    memory.write_u8(actor + 10, config.case.healerClass, "M68K BUS")
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    construction = {
        missingHp = config.case.actorMaxHp - config.case.actorInitialHp,
        basePower = config.case.spellPower,
        actorHp = memory.read_u16_be(actor + 14, "M68K BUS"),
        actorMp = memory.read_u8(actor + 17, "M68K BUS")
    }
end, config["function"].healEffectEntryAddress, "sf2-heal-effect", "M68K BUS")

event.on_bus_exec(function()
    if construction ~= nil then construction.adjustedPower = word_register("M68K D6") end
end, config["function"].adjustedPowerAddress, "sf2-heal-adjusted", "M68K BUS")

event.on_bus_exec(function()
    if construction ~= nil then construction.cappedRecovery = word_register("M68K D6") end
end, config["function"].cappedRecoveryAddress, "sf2-heal-capped", "M68K BUS")

event.on_bus_exec(function()
    if construction ~= nil then status("milestone:healing-exp") end
end, config["function"].healingExpEntryAddress, "sf2-heal-exp-entry", "M68K BUS")

event.on_bus_exec(function()
    if construction ~= nil then
        construction.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    end
end, config["function"].healingExpAppliedAddress, "sf2-heal-exp-applied", "M68K BUS")

event.on_bus_exec(function()
    if construction == nil then return end
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    construction.targetSameSide = memory.read_u8(a2 - 7, "M68K BUS") ~= 0
end, config["function"].sameSideDecisionAddress, "sf2-heal-same-side", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    award = {
        seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS"),
        halved = word_register("M68K D1")
    }
end, config["function"].expHalvedAddress, "sf2-heal-award-halved", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.firstRoll = word_register("M68K D0") end
end, config["function"].expFirstRollAddress, "sf2-heal-award-first", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.secondRoll = word_register("M68K D0") end
end, config["function"].expSecondRollAddress, "sf2-heal-award-second", "M68K BUS")

event.on_bus_exec(function()
    if award ~= nil then award.commandExp = word_register("M68K D1") end
end, config["function"].expFinalAddress, "sf2-heal-award-final", "M68K BUS")

event.on_bus_exec(function()
    if construction == nil then return end
    playback = true
    status("milestone:playback")
end, config["function"].battleSceneEndReturnAddress, "sf2-heal-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local actor = entry(config.case.actor)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    active_reaction = {
        hpChange = signed_word(memory.read_u16_be(a6, "M68K BUS")),
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        hpBefore = memory.read_u16_be(actor + 14, "M68K BUS"),
        mpBefore = memory.read_u8(actor + 17, "M68K BUS")
    }
end, config["function"].allyReactionEntryAddress, "sf2-heal-reaction", "M68K BUS")

event.on_bus_exec(function()
    if active_reaction == nil then return end
    local actor = entry(config.case.actor)
    active_reaction.hpAfter = memory.read_u16_be(actor + 14, "M68K BUS")
    active_reaction.mpAfter = memory.read_u8(actor + 17, "M68K BUS")
    ally_reactions[#ally_reactions + 1] = active_reaction
    active_reaction = nil
end, config["function"].allyReactionAppliedAddress, "sf2-heal-reaction-applied", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    local actor = entry(config.case.actor)
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    exp_reaction = {
        commandExp = memory.read_u16_be(a6, "M68K BUS"),
        expBefore = memory.read_u8(actor + 48, "M68K BUS")
    }
end, config["function"].giveExpEntryAddress, "sf2-heal-give-exp", "M68K BUS")

event.on_bus_exec(function()
    if exp_reaction ~= nil then
        exp_reaction.expAfter = memory.read_u8(entry(config.case.actor) + 48, "M68K BUS")
    end
end, config["function"].giveExpAppliedAddress, "sf2-heal-give-exp-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback and #ally_reactions == 2 and exp_reaction ~= nil
        and exp_reaction.expAfter ~= nil then
        write_result_and_exit()
    end
end, config["function"].executeScriptEndAddress, "sf2-heal-script-end", "M68K BUS")

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
        status(string.format(
            "frame=%d,stage=%s,pc=%X,reactions=%d,action=%d,started=%s,target=%s,heal=%d,playback=%s",
            frames, stage, emu.getregister("M68K PC"), #ally_reactions,
            memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
            tostring(action_started), tostring(target_supplied), heal_effect_calls, tostring(playback)))
    end
end
