local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, target_supplied, after_turn_started = false, false, false
local rng = { seed=config.case.seed }
local branches = { silenceExpiredEntries=0, silenceDecrementEntries=0, updateStatsEntries=0 }
local messages = { silence=0, slow=0, attack=0, boost=0 }
local status_values = {}
local stats = {}
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

local function read_status()
    return memory.read_u16_be(entry(config.case.combatant) + 44, "M68K BUS")
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"combatant":%d,"rng":{"seed":%d,"range":%d,"observedSeed":%d,' ..
        '"rawRoll":%d,"maskedRoll":%d},' ..
        '"branches":{"silenceExpiredEntries":%d,"silenceDecrementEntries":%d,' ..
        '"updateStatsEntries":%d},' ..
        '"messages":{"silence":%d,"slow":%d,"attack":%d,"boost":%d},' ..
        '"status":{"initial":%d,"afterSilence":%d,"afterSlow":%d,' ..
        '"afterAttack":%d,"afterBoost":%d,"final":%d},' ..
        '"stats":{"initialAttack":%d,"initialDefense":%d,"initialAgility":%d,' ..
        '"finalAttack":%d,"finalDefense":%d,"finalAgility":%d}}\n',
        emu.getsystemid(), config.case.id,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        config.case.combatant, rng.seed, rng.range, rng.observedSeed,
        rng.rawRoll, rng.maskedRoll,
        branches.silenceExpiredEntries, branches.silenceDecrementEntries,
        branches.updateStatsEntries,
        messages.silence, messages.slow, messages.attack, messages.boost,
        status_values.initial, status_values.afterSilence, status_values.afterSlow,
        status_values.afterAttack, status_values.afterBoost, status_values.final,
        stats.initialAttack, stats.initialDefense, stats.initialAgility,
        stats.finalAttack, stats.finalDefense, stats.finalAgility))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-after-turn-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    if prompt_count == 1 then pulse("Right"); pulse("C")
    elseif prompt_count == 2 then pulse("C") end
end, config.harness["function"].numberPromptAddress, "sf2-after-turn-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-after-turn-flag", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor, target = entry(config.case.combatant), entry(config.case.actionTarget)
    memory.write_u8(actor + 10, 0, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, 20, "M68K BUS")
    memory.write_u8(actor + 17, 20, "M68K BUS")
    memory.write_u8(actor + 18, config.case.baseAttack, "M68K BUS")
    memory.write_u8(actor + 19, 99, "M68K BUS")
    memory.write_u8(actor + 20, config.case.baseDefense, "M68K BUS")
    memory.write_u8(actor + 21, config.case.baseDefense, "M68K BUS")
    memory.write_u8(actor + 22, config.case.baseAgility, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for index, item in ipairs(config.case.itemEntries) do
        memory.write_u16_be(actor + 30 + index * 2, item, "M68K BUS")
    end
    memory.write_u16_be(actor + 44, 0, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    memory.write_u8(target + 11, 1, "M68K BUS")
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u16_be(target + 26, 0, "M68K BUS")
    memory.write_u16_be(target + 28, 0, "M68K BUS")
    memory.write_u16_be(target + 44, 0, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(target + 55, 39, "M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-after-turn-order", "M68K BUS")

event.on_bus_exec(function()
    if stage ~= "battle" or action_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.combatant then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.case.actionType, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.case.actionSpell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.case.actionTarget, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-after-turn-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.case.actionTarget, "M68K BUS")
end, config["function"].initializePropertiesAddress, "sf2-after-turn-target", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or after_turn_started then return end
    if (emu.getregister("M68K D0") & 0xFF) ~= config.case.combatant then return end
    after_turn_started = true
    local actor = entry(config.case.combatant)
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 18, config.case.baseAttack, "M68K BUS")
    memory.write_u8(actor + 19, config.case.initialCurrentAttack, "M68K BUS")
    memory.write_u8(actor + 20, config.case.baseDefense, "M68K BUS")
    memory.write_u8(actor + 21, config.case.initialCurrentDefense, "M68K BUS")
    memory.write_u8(actor + 22, config.case.baseAgility, "M68K BUS")
    memory.write_u8(actor + 23, config.case.initialCurrentAgility, "M68K BUS")
    for index, item in ipairs(config.case.itemEntries) do
        memory.write_u16_be(actor + 30 + index * 2, item, "M68K BUS")
    end
    memory.write_u16_be(actor + 44, config.case.initialStatus, "M68K BUS")
    status_values.initial = read_status()
    stats.initialAttack = memory.read_u8(actor + 19, "M68K BUS")
    stats.initialDefense = memory.read_u8(actor + 21, "M68K BUS")
    stats.initialAgility = memory.read_u8(actor + 23, "M68K BUS")
    status("milestone:after-turn")
end, config["function"].entryAddress, "sf2-after-turn-entry", "M68K BUS")

event.on_bus_exec(function()
    if not after_turn_started then return end
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    rng.range = word_register("M68K D6")
end, config["function"].silenceRngAddress, "sf2-after-turn-silence-rng", "M68K BUS")

event.on_bus_exec(function()
    if not after_turn_started then return end
    rng.observedSeed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    rng.rawRoll = word_register("M68K D7")
end, config["function"].silenceRngReturnAddress, "sf2-after-turn-silence-rng-return", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then rng.maskedRoll = word_register("M68K D7") end
end, config["function"].silenceBranchAddress, "sf2-after-turn-silence-branch", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then
        branches.silenceExpiredEntries = branches.silenceExpiredEntries + 1
        messages.silence = messages.silence + 1
    end
end, config["function"].silenceMessageAddress, "sf2-after-turn-silence-message", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then
        branches.silenceDecrementEntries = branches.silenceDecrementEntries + 1
    end
end, config["function"].silenceDecrementAddress, "sf2-after-turn-silence-decrement", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then status_values.afterSilence = read_status() end
end, config["function"].afterSilenceAddress, "sf2-after-turn-after-silence", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then messages.slow = messages.slow + 1 end
end, config["function"].slowMessageAddress, "sf2-after-turn-slow-message", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then status_values.afterSlow = read_status() end
end, config["function"].afterSlowAddress, "sf2-after-turn-after-slow", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then messages.attack = messages.attack + 1 end
end, config["function"].attackMessageAddress, "sf2-after-turn-attack-message", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then status_values.afterAttack = read_status() end
end, config["function"].afterAttackAddress, "sf2-after-turn-after-attack", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then messages.boost = messages.boost + 1 end
end, config["function"].boostMessageAddress, "sf2-after-turn-boost-message", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then status_values.afterBoost = read_status() end
end, config["function"].afterBoostAddress, "sf2-after-turn-after-boost", "M68K BUS")

event.on_bus_exec(function()
    if after_turn_started then branches.updateStatsEntries = branches.updateStatsEntries + 1 end
end, config["function"].updateStatsAddress, "sf2-after-turn-update-stats", "M68K BUS")

event.on_bus_exec(function()
    if not after_turn_started then return end
    local actor = entry(config.case.combatant)
    status_values.final = read_status()
    stats.finalAttack = memory.read_u8(actor + 19, "M68K BUS")
    stats.finalDefense = memory.read_u8(actor + 21, "M68K BUS")
    stats.finalAgility = memory.read_u8(actor + 23, "M68K BUS")
    write_result_and_exit()
end, config["function"].statsAppliedAddress, "sf2-after-turn-stats-applied", "M68K BUS")

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
    elseif action_started and frames % 12 < 4 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = ((stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1) or action_started) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,afterTurn=%s", frames, stage,
            emu.getregister("M68K PC"), tostring(after_turn_started)))
    end
end
