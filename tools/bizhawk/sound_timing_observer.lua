local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local domain = "Z80 RAM"
local records = {}

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function read_pointer(address)
    return memory.read_u8(address, domain) + memory.read_u8(address + 1, domain) * 0x100
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
        values[#values + 1] = read_pointer(address)
    end
    return values
end

local function snapshot(frame)
    return {
        frame = frame,
        operation = memory.read_u8(config.ram.newOperationAddress, domain),
        musicBank = memory.read_u8(config.ram.musicBankAddress, domain),
        dacDisabled = memory.read_u8(config.ram.musicDacModeAddress, domain),
        fadeInTimer = memory.read_u8(config.ram.fadeInTimerAddress, domain),
        newSample = memory.read_u8(config.ram.newSampleAddress, domain),
        pointers = read_channel_pointers(),
        timeCounters = read_channel_field(2),
        inactive = read_channel_field(3),
    }
end

local function write_number_array(file, values)
    file:write("[")
    for index, value in ipairs(values) do
        if index > 1 then file:write(",") end
        file:write(tostring(value))
    end
    file:write("]")
end

local function write_snapshot(file, value)
    file:write(string.format(
        '{"frame":%d,"operation":%d,"musicBank":%d,"dacDisabled":%d,"fadeInTimer":%d,"newSample":%d,"pointers":',
        value.frame, value.operation, value.musicBank, value.dacDisabled, value.fadeInTimer, value.newSample
    ))
    write_number_array(file, value.pointers)
    file:write(',"timeCounters":')
    write_number_array(file, value.timeCounters)
    file:write(',"inactive":')
    write_number_array(file, value.inactive)
    file:write("}")
end

local function finish()
    local file = assert(io.open(config.outputPath, "w"))
    file:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","bootFrames":%d,"records":[',
        emu.getsystemid(), config.fixtureId, config.bootFrames
    ))
    for record_index, record in ipairs(records) do
        if record_index > 1 then file:write(",") end
        file:write(string.format('{"id":"%s","command":%d,"checkpoints":[', record.id, record.command))
        for checkpoint_index, checkpoint in ipairs(record.checkpoints) do
            if checkpoint_index > 1 then file:write(",") end
            write_snapshot(file, checkpoint)
        end
        file:write("]}")
    end
    file:write("]}\n")
    file:close()
    client.exitCode(0)
end

for _ = 1, config.bootFrames do emu.frameadvance() end
local replay_state = memorysavestate.savecorestate()
local checkpoint_set = {}
local final_frame = 0
for _, frame in ipairs(config.checkpointFrames) do
    checkpoint_set[frame] = true
    if frame > final_frame then final_frame = frame end
end

for case_index, case in ipairs(config.cases) do
    if case_index > 1 then memorysavestate.loadcorestate(replay_state) end
    local record = { id = case.id, command = case.command, checkpoints = {} }
    records[#records + 1] = record
    memory.write_u8(config.ram.newOperationAddress, case.command, domain)
    status("milestone:case:" .. case.id)
    for frame = 1, final_frame do
        emu.frameadvance()
        if checkpoint_set[frame] then record.checkpoints[#record.checkpoints + 1] = snapshot(frame) end
    end
end

memorysavestate.removestate(replay_state)
finish()
