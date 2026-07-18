local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local active = nil

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
end

local function address(ally)
    return config.ram.combatantDataAddress + ally * config.ram.combatantEntrySize
end

local function write_input(ally, value)
    local target = address(ally)
    memory.write_u8(target + 10, value.class, "M68K BUS")
    memory.write_u8(target + 11, value.level, "M68K BUS")
    memory.write_u16_be(target + 12, value.maxHp, "M68K BUS")
    memory.write_u16_be(target + 14, value.currentHp, "M68K BUS")
    memory.write_u8(target + 16, value.maxMp, "M68K BUS")
    memory.write_u8(target + 17, value.currentMp, "M68K BUS")
    memory.write_u8(target + 18, value.baseAttack, "M68K BUS")
    memory.write_u8(target + 19, value.currentAttack, "M68K BUS")
    memory.write_u8(target + 20, value.baseDefense, "M68K BUS")
    memory.write_u8(target + 21, value.currentDefense, "M68K BUS")
    memory.write_u8(target + 22, value.baseAgility, "M68K BUS")
    memory.write_u8(target + 23, value.currentAgility, "M68K BUS")
    memory.write_u8(target + 24, value.baseMove, "M68K BUS")
    memory.write_u8(target + 25, value.currentMove, "M68K BUS")
    memory.write_u16_be(target + 26, value.baseResistance, "M68K BUS")
    memory.write_u16_be(target + 28, value.currentResistance, "M68K BUS")
    memory.write_u8(target + 30, value.baseProwess, "M68K BUS")
    memory.write_u8(target + 31, value.currentProwess, "M68K BUS")
    for index, item in ipairs(value.items) do
        memory.write_u16_be(target + 30 + index * 2, item, "M68K BUS")
    end
    for index, spell in ipairs(value.spells) do
        memory.write_u8(target + 39 + index, spell, "M68K BUS")
    end
    memory.write_u16_be(target + 44, value.status, "M68K BUS")
    memory.write_u8(target + 48, value.exp, "M68K BUS")
end

local function snapshot(ally)
    local source = address(ally)
    local items = {}
    local spells = {}
    for index = 0, 3 do items[#items + 1] = memory.read_u16_be(source + 32 + index * 2, "M68K BUS") end
    for index = 0, 3 do spells[#spells + 1] = memory.read_u8(source + 40 + index, "M68K BUS") end
    return {
        class = memory.read_u8(source + 10, "M68K BUS"), level = memory.read_u8(source + 11, "M68K BUS"),
        maxHp = memory.read_u16_be(source + 12, "M68K BUS"), currentHp = memory.read_u16_be(source + 14, "M68K BUS"),
        maxMp = memory.read_u8(source + 16, "M68K BUS"), currentMp = memory.read_u8(source + 17, "M68K BUS"),
        baseAttack = memory.read_u8(source + 18, "M68K BUS"), currentAttack = memory.read_u8(source + 19, "M68K BUS"),
        baseDefense = memory.read_u8(source + 20, "M68K BUS"), currentDefense = memory.read_u8(source + 21, "M68K BUS"),
        baseAgility = memory.read_u8(source + 22, "M68K BUS"), currentAgility = memory.read_u8(source + 23, "M68K BUS"),
        baseMove = memory.read_u8(source + 24, "M68K BUS"), currentMove = memory.read_u8(source + 25, "M68K BUS"),
        baseResistance = memory.read_u16_be(source + 26, "M68K BUS"), currentResistance = memory.read_u16_be(source + 28, "M68K BUS"),
        baseProwess = memory.read_u8(source + 30, "M68K BUS"), currentProwess = memory.read_u8(source + 31, "M68K BUS"),
        items = items, spells = spells, status = memory.read_u16_be(source + 44, "M68K BUS"),
        exp = memory.read_u8(source + 48, "M68K BUS")
    }
end

local function write_array(output, values)
    output:write("[")
    for index, value in ipairs(values) do
        if index > 1 then output:write(",") end
        output:write(tostring(value))
    end
    output:write("]")
end

local function write_snapshot(output, value)
    output:write(string.format(
        '{"class":%d,"level":%d,"maxHp":%d,"currentHp":%d,"maxMp":%d,"currentMp":%d,' ..
        '"baseAttack":%d,"currentAttack":%d,"baseDefense":%d,"currentDefense":%d,' ..
        '"baseAgility":%d,"currentAgility":%d,"baseMove":%d,"currentMove":%d,' ..
        '"baseResistance":%d,"currentResistance":%d,"baseProwess":%d,"currentProwess":%d,"items":',
        value.class, value.level, value.maxHp, value.currentHp, value.maxMp, value.currentMp,
        value.baseAttack, value.currentAttack, value.baseDefense, value.currentDefense,
        value.baseAgility, value.currentAgility, value.baseMove, value.currentMove,
        value.baseResistance, value.currentResistance, value.baseProwess, value.currentProwess))
    write_array(output, value.items)
    output:write(',"spells":'); write_array(output, value.spells)
    output:write(string.format(',"status":%d,"exp":%d}', value.status, value.exp))
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","result":{"id":"%s","ally":%d,"seed":%d,"before":',
        emu.getsystemid(), active.id, active.ally, active.seed))
    write_snapshot(output, active.before)
    output:write(',"after":'); write_snapshot(output, active.after)
    output:write(string.format(',"updateStatsCallObserved":%s,"updateStatsEntryObserved":%s}}\n',
        tostring(active.update_stats_call_observed), tostring(active.update_stats_entry_observed)))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    if reg("D0") ~= config.case.ally or active ~= nil then return end
    write_input(config.case.ally, config.case.input)
    active = {
        id = config.case.id, ally = config.case.ally, seed = config.case.seed,
        before = snapshot(config.case.ally),
        update_stats_call_observed = false, update_stats_entry_observed = false
    }
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
end, config["function"].entryAddress, "sf2-level-refresh-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then
        active.update_stats_call_observed = true
    end
end, config["function"].updateStatsCallAddress, "sf2-level-refresh-update-call", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.update_stats_entry_observed = true end
end, config["function"].updateStatsEntryAddress, "sf2-level-refresh-update-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil then return end
    active.after = snapshot(active.ally)
    write_result_and_exit()
end, config["function"].returnAddress, "sf2-level-refresh-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d", frames)) end
end
