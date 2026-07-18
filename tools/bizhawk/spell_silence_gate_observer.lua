local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, target_supplied, playback = false, false, false
local construction = {
    silencedMessageCommands = 0,
    notSilencedEntries = 0,
    applyActionEffectCalls = 0,
    blazeEffectCalls = 0
}
local ally_reaction_calls, enemy_reaction_calls, exp_reaction_calls = 0, 0, 0
local ally_reaction = nil
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

local function silenced_flag()
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    return memory.read_u8(a2 - 11, "M68K BUS")
end

local function word_register(name)
    return emu.getregister(name) & 0xFFFF
end

local function signed_word(value)
    if value >= 0x8000 then return value - 0x10000 end
    return value
end

local function write_result_and_exit()
    local actor, target = entry(config.case.actor), entry(config.case.target)
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"action":{"type":%d,"spell":%d,"target":%d},' ..
        '"construction":{"silencedFlagAfterProperties":%d,' ..
        '"silencedFlagAtDecision":%d,"silencedMessageCommands":%d,' ..
        '"notSilencedEntries":%d,"costMpChange":%d,"costStatusCommand":%d,' ..
        '"applyActionEffectCalls":%d,"blazeEffectCalls":%d,' ..
        '"expSilencedFlag":%d,"accumulatedExp":%d,"actorMp":%d,"actorStatus":%d,' ..
        '"targetHp":%d,"targetMp":%d,"targetStatus":%d},' ..
        '"replay":{"allyReactionCalls":%d,"allyReaction":{"combatant":%d,' ..
        '"mpChange":%d,"statusCommand":%d,"mpBefore":%d,"mpAfter":%d},' ..
        '"enemyReactionCalls":%d,' ..
        '"expReactionCalls":%d,"finalActorMp":%d,"finalActorExp":%d,' ..
        '"finalActorStatus":%d,"finalTargetHp":%d,"finalTargetMp":%d,' ..
        '"finalTargetStatus":%d}}\n',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress + 2, "M68K BUS"),
        config.case.target,
        construction.silencedFlagAfterProperties, construction.silencedFlagAtDecision,
        construction.silencedMessageCommands, construction.notSilencedEntries,
        construction.costMpChange, construction.costStatusCommand,
        construction.applyActionEffectCalls, construction.blazeEffectCalls,
        construction.expSilencedFlag, construction.accumulatedExp,
        construction.actorMp, construction.actorStatus,
        construction.targetHp, construction.targetMp, construction.targetStatus,
        ally_reaction_calls, ally_reaction.combatant, ally_reaction.mpChange,
        ally_reaction.statusCommand, ally_reaction.mpBefore, ally_reaction.mpAfter,
        enemy_reaction_calls, exp_reaction_calls,
        memory.read_u8(actor + 17, "M68K BUS"), memory.read_u8(actor + 48, "M68K BUS"),
        memory.read_u16_be(actor + 44, "M68K BUS"),
        memory.read_u16_be(target + 14, "M68K BUS"), memory.read_u8(target + 17, "M68K BUS"),
        memory.read_u16_be(target + 44, "M68K BUS")))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-silence-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-silence-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-silence-flag", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor, target = entry(config.case.actor), entry(config.case.target)
    memory.write_u8(actor + 10, config.case.actorClass, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 17, config.case.actorInitialMp, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, config.case.actorInitialStatus, "M68K BUS")
    memory.write_u8(actor + 48, config.case.actorInitialExp, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    memory.write_u8(target + 11, 1, "M68K BUS")
    memory.write_u16_be(target + 12, config.case.targetInitialHp, "M68K BUS")
    memory.write_u16_be(target + 14, config.case.targetInitialHp, "M68K BUS")
    memory.write_u8(target + 16, config.case.targetInitialMp, "M68K BUS")
    memory.write_u8(target + 17, config.case.targetInitialMp, "M68K BUS")
    memory.write_u16_be(target + 26, 0, "M68K BUS")
    memory.write_u16_be(target + 28, 0, "M68K BUS")
    memory.write_u16_be(target + 44, config.case.targetInitialStatus, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(target + 55, 39, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-silence-turn", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.target, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-silence-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.case.target, "M68K BUS")
end, config["function"].initializePropertiesAddress, "sf2-silence-target", "M68K BUS")

event.on_bus_exec(function()
    if action_started then construction.silencedFlagAfterProperties = silenced_flag() end
end, config["function"].silenceFlagAppliedAddress, "sf2-silence-property", "M68K BUS")

event.on_bus_exec(function()
    if action_started then construction.silencedFlagAtDecision = silenced_flag() end
end, config["function"].silenceDecisionAddress, "sf2-silence-decision", "M68K BUS")

event.on_bus_exec(function()
    if action_started then
        construction.silencedMessageCommands = construction.silencedMessageCommands + 1
    end
end, config["function"].silenceMessageAddress, "sf2-silence-message", "M68K BUS")

event.on_bus_exec(function()
    if action_started then construction.notSilencedEntries = construction.notSilencedEntries + 1 end
end, config["function"].notSilencedAddress, "sf2-silence-allowed", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    construction.costMpChange = signed_word(word_register("M68K D2"))
    construction.costStatusCommand = word_register("M68K D1")
end, config["function"].spellCostReactionAddress, "sf2-silence-cost", "M68K BUS")

event.on_bus_exec(function()
    if action_started then
        construction.applyActionEffectCalls = construction.applyActionEffectCalls + 1
    end
end, config["function"].applyActionEffectAddress, "sf2-silence-apply", "M68K BUS")

event.on_bus_exec(function()
    if action_started then construction.blazeEffectCalls = construction.blazeEffectCalls + 1 end
end, config["function"].blazeEffectEntryAddress, "sf2-silence-blaze", "M68K BUS")

event.on_bus_exec(function()
    if action_started then construction.expSilencedFlag = silenced_flag() end
end, config["function"].endExpSilenceDecisionAddress, "sf2-silence-exp", "M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local actor, target = entry(config.case.actor), entry(config.case.target)
    construction.accumulatedExp = memory.read_u16_be(config.ram.battleSceneExpAddress, "M68K BUS")
    construction.actorMp = memory.read_u8(actor + 17, "M68K BUS")
    construction.actorStatus = memory.read_u16_be(actor + 44, "M68K BUS")
    construction.targetHp = memory.read_u16_be(target + 14, "M68K BUS")
    construction.targetMp = memory.read_u8(target + 17, "M68K BUS")
    construction.targetStatus = memory.read_u16_be(target + 44, "M68K BUS")
    playback = true
end, config["function"].battleSceneEndReturnAddress, "sf2-silence-end", "M68K BUS")

event.on_bus_exec(function()
    if not playback then return end
    ally_reaction_calls = ally_reaction_calls + 1
    local a6 = emu.getregister("M68K A6") & 0xFFFFFF
    local combatant = memory.read_u16_be(config.ram.battleSceneAllyAddress, "M68K BUS")
    ally_reaction = {
        combatant = combatant,
        mpChange = signed_word(memory.read_u16_be(a6 + 2, "M68K BUS")),
        statusCommand = memory.read_u16_be(a6 + 4, "M68K BUS"),
        mpBefore = memory.read_u8(entry(combatant) + 17, "M68K BUS")
    }
end, config["function"].allyReactionEntryAddress, "sf2-silence-ally", "M68K BUS")

event.on_bus_exec(function()
    if ally_reaction ~= nil then
        ally_reaction.mpAfter = memory.read_u8(entry(ally_reaction.combatant) + 17, "M68K BUS")
    end
end, config["function"].allyReactionAppliedAddress, "sf2-silence-ally-applied", "M68K BUS")

event.on_bus_exec(function()
    if playback then enemy_reaction_calls = enemy_reaction_calls + 1 end
end, config["function"].enemyReactionEntryAddress, "sf2-silence-enemy", "M68K BUS")

event.on_bus_exec(function()
    if playback then exp_reaction_calls = exp_reaction_calls + 1 end
end, config["function"].giveExpEntryAddress, "sf2-silence-give", "M68K BUS")

event.on_bus_exec(function()
    if playback then write_result_and_exit() end
end, config["function"].executeScriptEndAddress, "sf2-silence-script-end", "M68K BUS")

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
    elseif stage == "ui"
        and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    elseif playback and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui"
        and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1)
        or playback) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,message=%d", frames, stage,
            emu.getregister("M68K PC"), construction.silencedMessageCommands))
    end
end
