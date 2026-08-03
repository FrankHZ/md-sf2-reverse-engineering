local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage, prompt_count = "cheat", 0
local queue = {}
local battle_setup, action_started, target_supplied = false, false, false
local next_case, active_case, current = 1, nil, nil
local records = {}
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
    return memory.read_u16_be(entry(active_case.combatant) + 44, "M68K BUS")
end

local function record_json()
    return string.format(
        '{"id":"%s","combatant":%d,' ..
        '"muddleRng":{"seed":%d,"range":%d,"observedSeed":%d,' ..
        '"rawRoll":%d,"maskedRoll":%d},' ..
        '"rng":{"seed":%d,"range":%d,"observedSeed":%d,' ..
        '"rawRoll":%d,"maskedRoll":%d},' ..
        '"branches":{"muddleExpiredEntries":%d,"muddleDecrementEntries":%d,' ..
        '"silenceExpiredEntries":%d,"silenceDecrementEntries":%d,' ..
        '"updateStatsEntries":%d},' ..
        '"messages":{"muddle":%d,"silence":%d,"slow":%d,"attack":%d,"boost":%d},' ..
        '"status":{"initial":%d,"afterMuddle":%d,"afterSilence":%d,"afterSlow":%d,' ..
        '"afterAttack":%d,"afterBoost":%d,"final":%d},' ..
        '"stats":{"initialAttack":%d,"initialDefense":%d,"initialAgility":%d,' ..
        '"finalAttack":%d,"finalDefense":%d,"finalAgility":%d}}',
        active_case.id, active_case.combatant,
        current.muddleRng.seed, current.muddleRng.range, current.muddleRng.observedSeed,
        current.muddleRng.rawRoll, current.muddleRng.maskedRoll,
        current.rng.seed, current.rng.range, current.rng.observedSeed,
        current.rng.rawRoll, current.rng.maskedRoll,
        current.branches.muddleExpiredEntries, current.branches.muddleDecrementEntries,
        current.branches.silenceExpiredEntries, current.branches.silenceDecrementEntries,
        current.branches.updateStatsEntries,
        current.messages.muddle,
        current.messages.silence, current.messages.slow,
        current.messages.attack, current.messages.boost,
        current.status.initial, current.status.afterMuddle,
        current.status.afterSilence, current.status.afterSlow,
        current.status.afterAttack, current.status.afterBoost, current.status.final,
        current.stats.initialAttack, current.stats.initialDefense, current.stats.initialAgility,
        current.stats.finalAttack, current.stats.finalDefense, current.stats.finalAgility)
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,' ..
        '"records":[%s]}\n',
        emu.getsystemid(), config.fixtureId,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS"),
        table.concat(records, ",")))
    output:close()
    client.exitCode(0)
end

local function begin_case(case)
    active_case = case
    current = {
        muddleRng = { seed=case.seed }, rng = { seed=case.seed },
        branches = { muddleExpiredEntries=0, muddleDecrementEntries=0,
            silenceExpiredEntries=0, silenceDecrementEntries=0, updateStatsEntries=0 },
        messages = { muddle=0, silence=0, slow=0, attack=0, boost=0 },
        status = {},
        stats = {}
    }
    local combatant = entry(case.combatant)
    memory.write_u16_be(combatant + 12, 100, "M68K BUS")
    memory.write_u16_be(combatant + 14, 100, "M68K BUS")
    memory.write_u8(combatant + 18, case.baseAttack, "M68K BUS")
    memory.write_u8(combatant + 19, case.initialCurrentAttack, "M68K BUS")
    memory.write_u8(combatant + 20, case.baseDefense, "M68K BUS")
    memory.write_u8(combatant + 21, case.initialCurrentDefense, "M68K BUS")
    memory.write_u8(combatant + 22, case.baseAgility, "M68K BUS")
    memory.write_u8(combatant + 23, case.initialCurrentAgility, "M68K BUS")
    for index, item in ipairs(config.constants.itemEntries) do
        memory.write_u16_be(combatant + 30 + index * 2, item, "M68K BUS")
    end
    memory.write_u16_be(combatant + 44, case.initialStatus, "M68K BUS")
    current.status.initial = read_status()
    current.stats.initialAttack = memory.read_u8(combatant + 19, "M68K BUS")
    current.stats.initialDefense = memory.read_u8(combatant + 21, "M68K BUS")
    current.stats.initialAgility = memory.read_u8(combatant + 23, "M68K BUS")
    status(string.format("milestone:after-turn:%s", case.id))
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-after-turn-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-after-turn-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-after-turn-flag", "M68K BUS")

event.on_bus_exec(function()
    if battle_setup then return end
    battle_setup, stage = true, "battle"
    local actor, target = entry(config.action.actor), entry(config.action.target)
    memory.write_u8(actor + 10, 0, "M68K BUS")
    memory.write_u8(actor + 11, 1, "M68K BUS")
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 16, 20, "M68K BUS")
    memory.write_u8(actor + 17, 20, "M68K BUS")
    memory.write_u8(actor + 19, 99, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    for offset = 32, 38, 2 do memory.write_u16_be(actor + offset, 0x007F, "M68K BUS") end
    memory.write_u16_be(actor + 44, 0, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")
    for ally = 0, 2 do
        local control = memory.read_u8(entry(ally) + 49, "M68K BUS")
        memory.write_u8(entry(ally) + 49, control | 0x80, "M68K BUS")
    end

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
    if (emu.getregister("M68K D0") & 0xFF) ~= config.action.actor then return end
    action_started = true
    memory.write_u16_be(config.ram.currentBattleActionAddress, config.action.type, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 2, config.action.spell, "M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress + 4, config.action.target, "M68K BUS")
end, config["function"].writeBattleSceneScriptAddress, "sf2-after-turn-action", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or target_supplied then return end
    target_supplied = true
    memory.write_u16_be(config.ram.targetsListLengthAddress, 1, "M68K BUS")
    memory.write_u8(config.ram.targetsListAddress, config.action.target, "M68K BUS")
end, config["function"].initializePropertiesAddress, "sf2-after-turn-target", "M68K BUS")

event.on_bus_exec(function()
    if not action_started or active_case ~= nil or next_case > #config.cases then return end
    local scheduled = emu.getregister("M68K D0") & 0xFF
    local candidate = config.cases[next_case]
    if scheduled == candidate.combatant then begin_case(candidate) end
end, config["function"].entryAddress, "sf2-after-turn-entry", "M68K BUS")

event.on_bus_exec(function()
    if active_case == nil then return end
    memory.write_u16_be(config.ram.seedAddress, active_case.seed, "M68K BUS")
    current.muddleRng.range = word_register("M68K D6")
end, config["function"].muddleRngAddress, "sf2-after-turn-muddle-rng", "M68K BUS")

event.on_bus_exec(function()
    if active_case == nil then return end
    current.muddleRng.observedSeed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    current.muddleRng.rawRoll = word_register("M68K D7")
end, config["function"].muddleRngReturnAddress, "sf2-after-turn-muddle-rng-return", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.muddleRng.maskedRoll = word_register("M68K D7") end
end, config["function"].muddleBranchAddress, "sf2-after-turn-muddle-branch", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then
        current.branches.muddleExpiredEntries = current.branches.muddleExpiredEntries + 1
        current.messages.muddle = current.messages.muddle + 1
    end
end, config["function"].muddleMessageAddress, "sf2-after-turn-muddle-message", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then
        current.branches.muddleDecrementEntries = current.branches.muddleDecrementEntries + 1
    end
end, config["function"].muddleDecrementAddress, "sf2-after-turn-muddle-decrement", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.status.afterMuddle = read_status() end
end, config["function"].afterMuddleAddress, "sf2-after-turn-after-muddle", "M68K BUS")

event.on_bus_exec(function()
    if active_case == nil then return end
    memory.write_u16_be(config.ram.seedAddress, active_case.seed, "M68K BUS")
    current.rng.range = word_register("M68K D6")
end, config["function"].silenceRngAddress, "sf2-after-turn-silence-rng", "M68K BUS")

event.on_bus_exec(function()
    if active_case == nil then return end
    current.rng.observedSeed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    current.rng.rawRoll = word_register("M68K D7")
end, config["function"].silenceRngReturnAddress, "sf2-after-turn-silence-rng-return", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.rng.maskedRoll = word_register("M68K D7") end
end, config["function"].silenceBranchAddress, "sf2-after-turn-silence-branch", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then
        current.branches.silenceExpiredEntries = current.branches.silenceExpiredEntries + 1
        current.messages.silence = current.messages.silence + 1
    end
end, config["function"].silenceMessageAddress, "sf2-after-turn-silence-message", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then
        current.branches.silenceDecrementEntries = current.branches.silenceDecrementEntries + 1
    end
end, config["function"].silenceDecrementAddress, "sf2-after-turn-silence-decrement", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.status.afterSilence = read_status() end
end, config["function"].afterSilenceAddress, "sf2-after-turn-after-silence", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.messages.slow = current.messages.slow + 1 end
end, config["function"].slowMessageAddress, "sf2-after-turn-slow-message", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.status.afterSlow = read_status() end
end, config["function"].afterSlowAddress, "sf2-after-turn-after-slow", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.messages.attack = current.messages.attack + 1 end
end, config["function"].attackMessageAddress, "sf2-after-turn-attack-message", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.status.afterAttack = read_status() end
end, config["function"].afterAttackAddress, "sf2-after-turn-after-attack", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.messages.boost = current.messages.boost + 1 end
end, config["function"].boostMessageAddress, "sf2-after-turn-boost-message", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then current.status.afterBoost = read_status() end
end, config["function"].afterBoostAddress, "sf2-after-turn-after-boost", "M68K BUS")

event.on_bus_exec(function()
    if active_case ~= nil then
        current.branches.updateStatsEntries = current.branches.updateStatsEntries + 1
    end
end, config["function"].updateStatsAddress, "sf2-after-turn-update-stats", "M68K BUS")

event.on_bus_exec(function()
    if active_case == nil then return end
    local combatant = entry(active_case.combatant)
    current.status.final = read_status()
    current.stats.finalAttack = memory.read_u8(combatant + 19, "M68K BUS")
    current.stats.finalDefense = memory.read_u8(combatant + 21, "M68K BUS")
    current.stats.finalAgility = memory.read_u8(combatant + 23, "M68K BUS")
    records[#records + 1] = record_json()
    active_case, current = nil, nil
    next_case = next_case + 1
    if next_case > #config.cases then write_result_and_exit() end
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
        status(string.format("frame=%d,stage=%s,pc=%X,nextCase=%d,records=%d", frames, stage,
            emu.getregister("M68K PC"), next_case, #records))
    end
end
