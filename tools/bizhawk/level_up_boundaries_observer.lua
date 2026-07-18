local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local active = nil
local results = {}
local seen = {}

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

local function address(ally)
    return config.ram.combatantDataAddress + ally * config.ram.combatantEntrySize
end

local function write_input(ally, input)
    local target = address(ally)
    memory.write_u8(target + 10, input.class, "M68K BUS")
    memory.write_u8(target + 11, input.level, "M68K BUS")
    memory.write_u16_be(target + 12, input.hp, "M68K BUS")
    memory.write_u8(target + 16, input.mp, "M68K BUS")
    memory.write_u8(target + 18, input.attack, "M68K BUS")
    memory.write_u8(target + 20, input.defense, "M68K BUS")
    memory.write_u8(target + 22, input.agility, "M68K BUS")
    memory.write_u8(target + 48, input.exp, "M68K BUS")
    for index, spell in ipairs(input.spells) do
        memory.write_u8(target + 39 + index, spell, "M68K BUS")
    end
end

local function snapshot(ally)
    local source = address(ally)
    local spells = {}
    for index = 0, 3 do spells[#spells + 1] = memory.read_u8(source + 40 + index, "M68K BUS") end
    return {
        class = memory.read_u8(source + 10, "M68K BUS"),
        level = memory.read_u8(source + 11, "M68K BUS"),
        hp = memory.read_u16_be(source + 12, "M68K BUS"),
        mp = memory.read_u8(source + 16, "M68K BUS"),
        attack = memory.read_u8(source + 18, "M68K BUS"),
        defense = memory.read_u8(source + 20, "M68K BUS"),
        agility = memory.read_u8(source + 22, "M68K BUS"),
        exp = memory.read_u8(source + 48, "M68K BUS"),
        spells = spells
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
        '{"class":%d,"level":%d,"hp":%d,"mp":%d,"attack":%d,"defense":%d,"agility":%d,"exp":%d,"spells":[',
        value.class, value.level, value.hp, value.mp, value.attack, value.defense, value.agility, value.exp))
    for index, spell in ipairs(value.spells) do
        if index > 1 then output:write(",") end
        output:write(tostring(spell))
    end
    output:write("]}")
end

local function write_results_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","results":[', emu.getsystemid()))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format('{"id":"%s","ally":%d,"seed":%d,"before":',
            result.id, result.ally, result.seed))
        write_snapshot(output, result.before)
        output:write(',"after":'); write_snapshot(output, result.after)
        output:write(string.format(',"capExit":%s,"extraLevelBranch":%s,"levelBeforeExtra":%d,' ..
            '"effectiveLevel":%d,"observedSeed":%d,"arguments":[', tostring(result.cap_exit),
            tostring(result.extra_level_branch), result.level_before_extra or -1,
            result.effective_level, result.observed_seed))
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
    local case = find_case(ally)
    if case == nil or active ~= nil or seen[ally] then return end
    write_input(ally, case.input)
    active = {
        id = case.id, ally = ally, seed = case.seed, before = snapshot(ally), cap_exit = false,
        extra_level_branch = false, effective_level = -1
    }
    memory.write_u16_be(config.ram.seedAddress, case.seed, "M68K BUS")
    seen[ally] = true
end, config["function"].entryAddress, "sf2-level-boundary-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.cap_exit = true end
end, config["function"].capExitAddress, "sf2-level-boundary-cap", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.extra_level_branch = true
        active.level_before_extra = reg("D5")
    end
end, config["function"].extraLevelAddress, "sf2-level-boundary-extra", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.effective_level = reg("D5") end
end, config["function"].spellScanAddress, "sf2-level-boundary-spell", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.after = snapshot(active.ally)
    active.arguments = arguments()
    active.observed_seed = memory.read_u16_be(config.ram.seedAddress, "M68K BUS")
    results[#results + 1] = active
    active = nil
    if #results == #config.cases then write_results_and_exit() end
end, config["function"].returnAddress, "sf2-level-boundary-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d,results=%d", frames, #results)) end
end
