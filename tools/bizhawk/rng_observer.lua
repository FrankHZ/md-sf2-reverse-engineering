local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))

local results = {}
local current = 1
local active = false
local entry = nil
local rng_fallback = false
local stage = config.mode == "debug" and "cheat" or "observe"
local prompt_count = 0
local queue = {}
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "w"))
    file:write(value .. "\n")
    file:close()
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

local function write_results_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","mode":"%s","results":[',
        emu.getsystemid(), config.mode))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","seed":%d,"entryRange":%d,"observedSeed":%d,"observedValue":%d,' ..
            '"rngFallback":%s,"d6Before":%d,"d7Before":%d,"d6After":%d,"d7After":%d}',
            result.id, result.seed, result.entryRange, result.observedSeed, result.observedValue,
            tostring(result.rngFallback),
            result.d6Before, result.d7Before, result.d6After, result.d7After))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

if config.mode == "debug" then
    event.on_bus_exec(function()
        stage = "ui"
        status("milestone:battle-test")
    end, 0x769C, "sf2-rng-battle-test", "M68K BUS")

    event.on_bus_exec(function()
        prompt_count = prompt_count + 1
        status("milestone:number-prompt:" .. prompt_count)
        if prompt_count == 1 then pulse("Right"); pulse("C")
        elseif prompt_count == 2 then pulse("C") end
    end, 0x16282, "sf2-rng-number-prompt", "M68K BUS")

    event.on_bus_exec(function()
        status("milestone:flag-prompt")
        pulse("B")
    end, 0x163BC, "sf2-rng-flag-prompt", "M68K BUS")

    event.on_bus_exec(function()
        stage = "battle"
        local function combatant_entry(combatant)
            local slot = combatant
            if combatant >= 128 then slot = combatant - 96 end
            return 0xFFE800 + slot * 56
        end
        memory.write_u8(combatant_entry(0) + 19, 99, "M68K BUS")
        memory.write_u8(combatant_entry(0) + 10, 0, "M68K BUS")
        memory.write_u8(combatant_entry(0) + 11, 1, "M68K BUS")
        memory.write_u8(combatant_entry(0) + 31, 0, "M68K BUS")
        memory.write_u8(combatant_entry(0) + 48, 0, "M68K BUS")
        memory.write_u8(combatant_entry(0) + 49, 0x80, "M68K BUS")
        memory.write_u16_be(combatant_entry(0) + 52, 4, "M68K BUS")
        memory.write_u8(combatant_entry(128) + 11, 1, "M68K BUS")
        memory.write_u16_be(combatant_entry(128) + 12, 100, "M68K BUS")
        memory.write_u16_be(combatant_entry(128) + 14, 100, "M68K BUS")
        memory.write_u8(combatant_entry(128) + 21, 20, "M68K BUS")
        memory.write_u8(combatant_entry(128) + 46, 8, "M68K BUS")
        memory.write_u8(combatant_entry(128) + 47, 17, "M68K BUS")
        memory.write_u8(combatant_entry(128) + 49, 0x60, "M68K BUS")
        memory.write_u8(0xFF5F00 + 17 * 48 + 8, 3, "M68K BUS")
        status("milestone:turn-order-entry")
    end, 0x25544, "sf2-rng-turn-order-entry", "M68K BUS")
end

event.on_bus_exec(function()
    if current > #config.cases or active then return end
    local case = config.cases[current]
    entry = {
        range = emu.getregister(config.rangeRegister) & 0xFFFF,
        d6 = emu.getregister("M68K D6") & 0xFFFFFFFF,
        d7 = emu.getregister("M68K D7") & 0xFFFFFFFF,
        debug = config.debugModeAddress and memory.read_u8(config.debugModeAddress, "M68K BUS") or 0,
        input = config.playerInputAddress and memory.read_u8(config.playerInputAddress, "M68K BUS") or 0
    }
    memory.write_u16_be(config.seedAddress, case.seed, "M68K BUS")
    if config.mode == "debug" then
        memory.write_u8(config.debugModeAddress, case.debugEnabled and 255 or 0, "M68K BUS")
        memory.write_u8(config.playerInputAddress, case.inputMask, "M68K BUS")
    end
    rng_fallback = false
    active = true
end, config.entryAddress, "sf2-rng-entry", "M68K BUS")

if config.fallbackAddress ~= nil then
    event.on_bus_exec(function()
        if active then rng_fallback = true end
    end, config.fallbackAddress, "sf2-rng-fallback", "M68K BUS")
end

event.on_bus_exec(function()
    if not active then return end
    local case = config.cases[current]
    results[#results + 1] = {
        id = case.id,
        seed = case.seed,
        entryRange = entry.range,
        observedSeed = memory.read_u16_be(config.seedAddress, "M68K BUS"),
        observedValue = emu.getregister(config.resultRegister) & 0xFFFF,
        rngFallback = rng_fallback,
        d6Before = entry.d6,
        d7Before = entry.d7,
        d6After = emu.getregister("M68K D6") & 0xFFFFFFFF,
        d7After = emu.getregister("M68K D7") & 0xFFFFFFFF
    }
    if config.mode == "debug" then
        memory.write_u8(config.debugModeAddress, entry.debug, "M68K BUS")
        memory.write_u8(config.playerInputAddress, entry.input, "M68K BUS")
    end
    active = false
    current = current + 1
    if current > #config.cases then write_results_and_exit() end
end, config.returnAddress, "sf2-rng-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if config.mode == "debug" then
        local button = nil
        if stage == "cheat" then
            local pointer = memory.read_u32_be(0xFFB1A0, "M68K BUS")
            if pointer >= 0x28FF0 and pointer < 0x29000 then
                button = names[cheat[pointer - 0x28FF0 + 1]]
            elseif memory.read_u8(0xFFB0A9, "M68K BUS") == 255 then
                button = "Up"
            end
        elseif #queue > 0 then
            button = table.remove(queue, 1)
        elseif stage == "ui" and memory.read_u8(0xFFF712, "M68K BUS") == 1 then
            button = "C"
        end
        set_button(button)
        joypad.set({ Start = stage == "ui" and memory.read_u8(0xFFF712, "M68K BUS") == 1 }, 2)
    end
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d,stage=%s,cases=%d", frames, stage, current - 1)) end
end
