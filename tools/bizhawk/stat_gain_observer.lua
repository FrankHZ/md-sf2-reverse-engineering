local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local current = 1
local active = nil
local results = {}
local done = false

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
end

local function write_results_and_exit()
    if done then return end
    done = true
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","results":[', emu.getsystemid()))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","input":{"current":%d,"curve":%d,"start":%d,"projected":%d,"level":%d},"rng":[',
            result.id, result.current, result.curve, result.start, result.projected, result.level))
        for rng_index, rng in ipairs(result.rng) do
            if rng_index > 1 then output:write(",") end
            output:write(string.format('{"observedSeed":%d,"observedValue":%d}', rng.seed, rng.value))
        end
        output:write(string.format('],"path":"%s","pity":%s,"gain":%d}',
            result.path, tostring(result.pity), result.gain))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    if done or active ~= nil or current > #config.cases then return end
    active = {
        id = config.cases[current].id,
        current = reg("D1"), curve = reg("D2"), start = reg("D3"),
        projected = reg("D4"), level = reg("D5"), rng = {}, rng_calls = 0, pity = false
    }
end, config["function"].entryAddress, "sf2-stat-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.rng_calls = active.rng_calls + 1
    local seed = config.cases[current].seeds[active.rng_calls]
    if seed ~= nil then memory.write_u16_be(config["function"].seedAddress, seed, "M68K BUS") end
end, config["function"].rngEntryAddress, "sf2-stat-rng-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or active.rng_calls < 1 then return end
    active.rng[active.rng_calls] = {
        seed = memory.read_u16_be(config["function"].seedAddress, "M68K BUS"), value = reg("D7")
    }
end, config["function"].rngObserveAddress, "sf2-stat-rng-observe", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.pity = true end
end, config["function"].pityAddress, "sf2-stat-pity", "M68K BUS")

local function finish(path)
    if active == nil then return end
    active.path = path
    active.gain = reg("D1")
    active.rng_calls = nil
    results[#results + 1] = active
    active = nil
    current = current + 1
    if current > #config.cases then write_results_and_exit() end
end

event.on_bus_exec(function() finish("none") end, config["function"].noneReturnAddress, "sf2-stat-none-return", "M68K BUS")
event.on_bus_exec(function() finish("growth") end, config["function"].growthReturnAddress, "sf2-stat-growth-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d,cases=%d", frames, current - 1)) end
end
