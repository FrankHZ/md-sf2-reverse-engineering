local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local active = nil
local results = {}
local seen = {}
local initialization = {}

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
end

local function find_case(ally)
    for _, case in ipairs(config.cases) do
        if case.ally == ally then return case end
    end
    return nil
end

local function snapshot(ally)
    local address = config.ram.combatantDataAddress + ally * config.ram.combatantEntrySize
    return {
        class = memory.read_u8(address + 10, "M68K BUS"),
        level = memory.read_u8(address + 11, "M68K BUS"),
        hp = memory.read_u16_be(address + 12, "M68K BUS"),
        mp = memory.read_u8(address + 16, "M68K BUS"),
        attack = memory.read_u8(address + 18, "M68K BUS"),
        defense = memory.read_u8(address + 20, "M68K BUS"),
        agility = memory.read_u8(address + 22, "M68K BUS"),
        exp = memory.read_u8(address + 48, "M68K BUS")
    }
end

local function arguments()
    local values = {}
    for index = 0, 6 do
        values[#values + 1] = memory.read_u8(config.ram.levelUpArgumentsAddress + index, "M68K BUS")
    end
    return values
end

local function write_snapshot(output, value)
    output:write(string.format(
        '{"class":%d,"level":%d,"hp":%d,"mp":%d,"attack":%d,"defense":%d,"agility":%d,"exp":%d}',
        value.class, value.level, value.hp, value.mp, value.attack, value.defense, value.agility, value.exp))
end

local function write_results_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","results":[', emu.getsystemid()))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format('{"id":"%s","ally":%d,"seed":%d,"initialization":' ..
            '{"startLevel":%d,"extraLevelBranch":%s,"levelBeforeExtra":%d,"effectiveLevel":%d},"before":',
            result.id, result.ally, result.seed, result.initialization.start_level,
            tostring(result.initialization.extra_level_branch), result.initialization.level_before_extra or -1,
            result.initialization.effective_level))
        write_snapshot(output, result.before)
        output:write(',"after":'); write_snapshot(output, result.after)
        output:write(string.format(',"levelUpExtraBranch":%s,"levelBeforeExtra":%d,' ..
            '"effectiveLevel":%d,"observedSeed":%d,"arguments":[',
            tostring(result.extra_level_branch), result.level_before_extra or -1, result.effective_level,
            result.observed_seed))
        for argument_index, value in ipairs(result.arguments) do
            if argument_index > 1 then output:write(",") end
            output:write(tostring(value))
        end
        output:write("]}")
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    local ally = reg("D0")
    if find_case(ally) == nil then return end
    initialization[ally] = {
        start_level = reg("D1"), extra_level_branch = false, effective_level = -1
    }
end, config["function"].initializeEntryAddress, "sf2-init-entry", "M68K BUS")

event.on_bus_exec(function()
    local ally = reg("D0")
    local value = initialization[ally]
    if value ~= nil then
        value.extra_level_branch = true
        value.level_before_extra = reg("D5")
    end
end, config["function"].initializeExtraLevelAddress, "sf2-init-extra", "M68K BUS")

event.on_bus_exec(function()
    local ally = reg("D0")
    local value = initialization[ally]
    if value ~= nil then value.effective_level = reg("D5") end
end, config["function"].initializeStatsScanAddress, "sf2-init-stats-scan", "M68K BUS")

event.on_bus_exec(function()
    local ally = reg("D0")
    local case = find_case(ally)
    if case == nil or active ~= nil or seen[ally] then return end
    active = {
        id = case.id, ally = ally, seed = case.seed, before = snapshot(ally),
        initialization = initialization[ally], extra_level_branch = false, effective_level = -1
    }
    memory.write_u16_be(config.ram.seedAddress, case.seed, "M68K BUS")
    seen[ally] = true
end, config["function"].levelUpEntryAddress, "sf2-level-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.extra_level_branch = true
        active.level_before_extra = reg("D5")
    end
end, config["function"].levelUpExtraLevelAddress, "sf2-level-extra", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.effective_level = reg("D5") end
end, config["function"].levelUpSpellScanAddress, "sf2-level-spell-scan", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.after = snapshot(active.ally)
    active.arguments = arguments()
    active.observed_seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    results[#results + 1] = active
    active = nil
    if #results == #config.cases then write_results_and_exit() end
end, config["function"].levelUpReturnAddress, "sf2-level-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d,results=%d", frames, #results)) end
end
