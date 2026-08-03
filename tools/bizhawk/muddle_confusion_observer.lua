local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage, prompt_count = "cheat", 0
local queue = {}
local setup_done, next_case, active = false, 1, nil
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

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,"records":[',
        emu.getsystemid(), config.fixtureId,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS")))
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","status":%d,"confused":%d}',
            record.id, record.status, record.confused))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"; status("milestone:battle-test")
end, config.harness["function"].battleTestAddress, "sf2-confusion-battle", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-confusion-number", "M68K BUS")

event.on_bus_exec(function()
    pulse("B")
end, config.harness["function"].flagPromptAddress, "sf2-confusion-flag", "M68K BUS")

event.on_bus_exec(function()
    if setup_done then return end
    setup_done, stage = true, "battle"
    memory.write_u8(config.ram.autoBattleToggleAddress, 0xFF, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-confusion-setup", "M68K BUS")

event.on_bus_exec(function()
    if not setup_done or active ~= nil or next_case > #config.cases then return end
    local combatant = emu.getregister("M68K D0") & 0xFF
    local address = entry(combatant)
    local case = config.cases[next_case]
    active = {
        id = case.id,
        status = case.status,
        combatant = combatant,
        originalStatus = memory.read_u16_be(address + 44, "M68K BUS")
    }
    memory.write_u16_be(address + 44, case.status, "M68K BUS")
end, config["function"].entryAddress, "sf2-confusion-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.confused = emu.getregister("M68K D1") & 0xFFFF
    memory.write_u16_be(entry(active.combatant) + 44, active.originalStatus, "M68K BUS")
    records[#records + 1] = active
    active = nil
    next_case = next_case + 1
    if next_case > #config.cases then write_result_and_exit() end
end, config["function"].returnAddress, "sf2-confusion-return", "M68K BUS")

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
    end
    set_button(button)
    joypad.set({ Start = (stage == "ui" and memory.read_u8(
        config.harness.ram.currentBattleAddress, "M68K BUS") == 1) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,nextCase=%d,records=%d", frames, stage,
            next_case, #records))
    end
end
