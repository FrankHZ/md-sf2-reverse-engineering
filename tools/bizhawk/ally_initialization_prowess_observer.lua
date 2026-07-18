local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local active = nil

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
end

local function prowess(ally)
    local address = config.ram.combatantDataAddress
        + ally * config.ram.combatantEntrySize + config.ram.baseProwessOffset
    return memory.read_u8(address, "M68K BUS")
end

local function write_result_and_exit(result)
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"id":"%s","ally":%d,' ..
        '"startingLevel":%d,"effectiveLevel":%d,"spell":%d,' ..
        '"baseProwessBefore":%d,"baseProwessAfter":%d}}\n',
        emu.getsystemid(), result.id, result.ally, result.starting_level,
        result.effective_level, result.spell, result.before, result.after))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    if reg("D0") ~= config.case.ally then return end
    active = {
        id = config.case.id,
        ally = config.case.ally,
        starting_level = reg("D1")
    }
end, config["function"].entryAddress, "sf2-karna-init-entry", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or (reg("D1") & 0xFF) ~= config.case.spell then return end
    active.effective_level = reg("D5")
    active.spell = reg("D1") & 0xFF
    active.before = prowess(active.ally)
end, config["function"].heal3CheckAddress, "sf2-karna-heal3-check", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or active.before == nil then return end
    active.after = prowess(active.ally)
    write_result_and_exit(active)
end, config["function"].prowessWrittenAddress, "sf2-karna-prowess-written", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d", frames)) end
end
