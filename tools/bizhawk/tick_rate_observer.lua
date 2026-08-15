-- Observes the Z80 music tick rate (channel counters per frame) for given music commands.
-- Reads config from SF2_H3_CONFIG (see src/sf2tool/h3/bizhawk.py run_observer).
-- Evidence owner: docs/design/contracts/audio-system.md frame-locked tick-rate row.

local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local domain = "Z80 RAM"

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function read_channel_field(offset)
    local values = {}
    for index = 0, config.ram.channelCount - 1 do
        local address = config.ram.channelBaseAddress + index * config.ram.channelRecordSize + offset
        values[#values + 1] = memory.read_u8(address, domain)
    end
    return values
end

local function read_channel_pointers()
    local values = {}
    for index = 0, config.ram.channelCount - 1 do
        local address = config.ram.channelBaseAddress + index * config.ram.channelRecordSize
        values[#values + 1] =
            memory.read_u8(address, domain) + memory.read_u8(address + 1, domain) * 0x100
    end
    return values
end

local function write_number_array(file, values)
    file:write("[")
    for index, value in ipairs(values) do
        if index > 1 then file:write(",") end
        file:write(tostring(value))
    end
    file:write("]")
end

local function finish(records)
    local file = assert(io.open(config.outputPath, "w"))
    file:write("{\"system\":\"" .. emu.getsystemid() .. "\",\"core\":\"Genesis Plus GX\",\"frames\":")
    file:write(config.observeFrames)
    file:write(",\"records\":[")
    for index, record in ipairs(records) do
        if index > 1 then file:write(",") end
        file:write("{\"command\":")
        file:write(tostring(record.command))
        file:write(",\"counters\":[")
        for counter_index, counter in ipairs(record.counters) do
            if counter_index > 1 then file:write(",") end
            write_number_array(file, counter)
        end
        file:write("],\"pointers\":[")
        for pointer_index, pointer in ipairs(record.pointers) do
            if pointer_index > 1 then file:write(",") end
            write_number_array(file, pointer)
        end
        file:write("]}")
    end
    file:write("]}\n")
    file:close()
    client.exitCode(0)
end

for _ = 1, config.bootFrames do emu.frameadvance() end
local replay_state = memorysavestate.savecorestate()

local records = {}
for case_index, command in ipairs(config.commands) do
    if case_index > 1 then memorysavestate.loadcorestate(replay_state) end
    memory.write_u8(config.ram.newOperationAddress, command, domain)
    status("milestone:case:" .. command)
    local counters = {}
    local pointers = {}
    for frame = 1, config.observeFrames do
        emu.frameadvance()
        counters[frame] = read_channel_field(2)
        pointers[frame] = read_channel_pointers()
    end
    records[#records + 1] = { command = command, counters = counters, pointers = pointers }
end

memorysavestate.removestate(replay_state)
finish(records)
