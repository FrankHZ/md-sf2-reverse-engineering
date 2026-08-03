local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local active = nil
local case_index = 1
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
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","result":{"battle":%d,"cases":[',
        emu.getsystemid(), memory.read_u8(config.ram.currentBattleAddress, "M68K BUS")))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","actor":%d,"target":%d,"class":%d,"actorLevel":%d,' ..
            '"targetLevel":%d,"killExp":%d}',
            result.id, result.actor, result.target, result.class, result.actorLevel,
            result.targetLevel, result.killExp))
    end
    output:write("]}}\n")
    output:close()
    if replay_state ~= nil then memorysavestate.removestate(replay_state) end
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config["function"].battleTestAddress, "sf2-kill-exp-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config["function"].numberPromptAddress, "sf2-kill-exp-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config["function"].flagPromptAddress, "sf2-kill-exp-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local first_case = config.cases[1]
    local actor = entry(first_case.actor)
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 19, 99, "M68K BUS")
    memory.write_u8(actor + 21, 20, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local target = entry(first_case.target)
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u8(target + 21, 20, "M68K BUS")
    memory.write_u8(target + 23, 1, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(config.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    pending_save = true
    status("milestone:battle-setup")
end, config["function"].turnOrderEntryAddress, "sf2-kill-exp-turn-order", "M68K BUS")

event.on_bus_exec(function()
    memory.write_u16_be(config.ram.seedAddress, 0, "M68K BUS")
end, config["function"].criticalEntryAddress, "sf2-kill-exp-critical-seed", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then return end
    local current_case = config.cases[case_index]
    local actor_pointer = emu.getregister("M68K A4") & 0xFFFFFF
    local target_pointer = emu.getregister("M68K A5") & 0xFFFFFF
    local actor = memory.read_u8(actor_pointer, "M68K BUS")
    local target = memory.read_u8(target_pointer, "M68K BUS")
    if actor ~= current_case.actor or target ~= current_case.target then return end
    if replay_state == nil then error("kill EXP replay state was not captured before execution") end
    memory.write_u8(entry(actor) + 10, current_case.class, "M68K BUS")
    memory.write_u8(entry(actor) + 11, current_case.actorLevel, "M68K BUS")
    memory.write_u8(entry(target) + 11, current_case.targetLevel, "M68K BUS")
    active = {
        id = current_case.id,
        actor = actor,
        target = target,
        class = memory.read_u8(entry(actor) + 10, "M68K BUS"),
        actorLevel = memory.read_u8(entry(actor) + 11, "M68K BUS"),
        targetLevel = memory.read_u8(entry(target) + 11, "M68K BUS")
    }
    status("milestone:get-kill-exp-entry")
end, config["function"].entryAddress, "sf2-kill-exp-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.killExp = emu.getregister("M68K D5") & 0xFFFF
    results[#results + 1] = active
    active = nil
    case_index = case_index + 1
    if case_index > #config.cases then
        write_result_and_exit()
    else
        pending_replay = true
        status("milestone:replay-case:" .. case_index)
    end
end, config["function"].returnAddress, "sf2-kill-exp-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
        status("milestone:replay-state-saved")
    elseif pending_replay then
        pending_replay = false
        memorysavestate.loadcorestate(replay_state)
    end
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.ram.debugModeAddress, "M68K BUS") == 255 then
            button = "Up"
        end
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1 }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,pc=%X,prompts=%d,queue=%d,battle=%d", frames,
            stage, emu.getregister("M68K PC"), prompt_count, #queue,
            memory.read_u8(config.ram.currentBattleAddress, "M68K BUS")))
    end
end
