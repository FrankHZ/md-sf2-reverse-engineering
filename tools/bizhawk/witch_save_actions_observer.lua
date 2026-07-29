local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))

local frame_count, direct_index, control_index, precondition_selector = 0, 1, 1, 0
local active, phase = nil, "await-sram"
local direct_records, control_records = {}, {}
local checkpoint_address = config.ram.workRamScratchAddress
local thunk_address = checkpoint_address + 0x20
local flag88_byte_offset, flag88_mask = math.floor(88 / 8), 0x80 >> (88 % 8)

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

status("memory-domains:" .. table.concat(memory.getmemorydomainlist(), ",")
    .. ";sram-size=" .. memory.getmemorydomainsize("SRAM"))

local function json_escape(value)
    return tostring(value):gsub("[\\\"%z\1-\31]", function(character)
        local byte = string.byte(character)
        if character == "\\" then return "\\\\" end
        if character == "\"" then return "\\\"" end
        if character == "\b" then return "\\b" end
        if character == "\f" then return "\\f" end
        if character == "\n" then return "\\n" end
        if character == "\r" then return "\\r" end
        if character == "\t" then return "\\t" end
        return string.format("\\u%04x", byte)
    end)
end

local function pattern_byte(seed, offset)
    return (seed + offset * 13 + math.floor(offset / 7) * 29) & 0xFF
end

local function register(name)
    return emu.getregister("M68K " .. name) & 0xFFFFFFFF
end

local function slot_data_address(selector)
    if selector == 0 then return config.storage.slot1DataAddress end
    return config.storage.slot2DataAddress
end

local function slot_checksum_address(selector)
    if selector == 0 then return config.storage.slot1ChecksumAddress end
    return config.storage.slot2ChecksumAddress
end

local function sram_domain_offset(physical_address)
    local delta = physical_address - config.storage.physicalWindowBaseAddress
    assert(delta >= 0 and delta < memory.getmemorydomainsize("SRAM"),
        "source physical SRAM address is outside the emulator SRAM domain")
    return delta
end

local function read_sram_byte(physical_address)
    return memory.read_u8(sram_domain_offset(physical_address), "SRAM")
end

local function write_payload(seed)
    for offset = 0, config.storage.logicalPayloadByteCountPerSlot - 1 do
        memory.write_u8(
            config.ram.combatantDataAddress + offset,
            pattern_byte(seed, offset),
            "M68K BUS"
        )
    end
end

local function poison_payload()
    for offset = 0, config.storage.logicalPayloadByteCountPerSlot - 1 do
        memory.write_u8(config.ram.combatantDataAddress + offset, 0xFF, "M68K BUS")
    end
end

local function read_payload_samples()
    local samples = {}
    for _, offset in ipairs(config.cases.sampleOffsets) do
        samples[#samples + 1] = {
            logicalOffset = offset,
            logicalPayloadByte = memory.read_u8(
                config.ram.combatantDataAddress + offset, "M68K BUS"
            )
        }
    end
    return samples
end

local function read_slot(selector)
    local base = slot_data_address(selector)
    local checksum = 0
    local samples = {}
    for offset = 0, config.storage.logicalPayloadByteCountPerSlot - 1 do
        local address = base + offset * config.storage.physicalAddressStepPerLogicalByte
        local value = read_sram_byte(address)
        checksum = (checksum + value) & 0xFF
    end
    for _, offset in ipairs(config.cases.sampleOffsets) do
        local address = base + offset * config.storage.physicalAddressStepPerLogicalByte
        samples[#samples + 1] = {
            logicalOffset = offset,
            physicalAddress = address,
            storedPhysicalByte = read_sram_byte(address)
        }
    end
    return {
        storedChecksumByte = read_sram_byte(slot_checksum_address(selector)),
        computedChecksumByte = checksum,
        storedPayloadSamples = samples,
    }
end

local function write_jump(address, at)
    memory.write_u16_be(at, 0x4EF9, "M68K BUS")
    memory.write_u32_be(at + 2, address, "M68K BUS")
end

local function write_service_call(address, selector)
    memory.write_u16_be(thunk_address, 0x7000 | (selector & 0xFF), "M68K BUS")
    memory.write_u16_be(thunk_address + 2, 0x4EB9, "M68K BUS")
    memory.write_u32_be(thunk_address + 4, address, "M68K BUS")
    write_jump(checkpoint_address, thunk_address + 8)
end

local function write_observation()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"%s","id":"%s","directServiceResults":[',
        json_escape(emu.getsystemid()), json_escape(config.core), json_escape(config.fixtureId)
    ))
    for index, record in ipairs(direct_records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","operation":"%s","selector":%d,"selectedSlot":"%s",',
            json_escape(record.id), json_escape(record.operation), record.selector,
            json_escape(record.selectedSlot)
        ))
        if record.sourceSlot ~= nil then
            output:write(string.format(
                '"sourceSlot":"%s","destinationSlot":"%s","observedDestinationSelector":%d,',
                json_escape(record.sourceSlot), json_escape(record.destinationSlot),
                record.observedDestinationSelector
            ))
        end
        output:write(string.format(
            '"saveFlagsByte":%d,"storedChecksumByte":%d,"computedChecksumByte":%d,',
            record.saveFlagsByte, record.storedChecksumByte, record.computedChecksumByte
        ))
        output:write('"storedPayloadSamples":[')
        for sample_index, sample in ipairs(record.storedPayloadSamples) do
            if sample_index > 1 then output:write(",") end
            output:write(string.format(
                '{"logicalOffset":%d,"physicalAddress":%d,"storedPhysicalByte":%d}',
                sample.logicalOffset, sample.physicalAddress, sample.storedPhysicalByte
            ))
        end
        output:write('],"restoredPayloadSamples":[')
        for sample_index, sample in ipairs(record.restoredPayloadSamples) do
            if sample_index > 1 then output:write(",") end
            output:write(string.format(
                '{"logicalOffset":%d,"logicalPayloadByte":%d}',
                sample.logicalOffset, sample.logicalPayloadByte
            ))
        end
        output:write("]}")
    end
    output:write('],"loadControlFlowResults":[')
    for index, record in ipairs(control_records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","flag88Set":%s,"instructionTarget":"%s",'
                .. '"instructionTargetAddress":%d,"effectiveTarget":"%s",'
                .. '"effectiveTargetAddress":%d}',
            json_escape(record.id), tostring(record.flag88Set),
            json_escape(record.instructionTarget), record.instructionTargetAddress,
            json_escape(record.effectiveTarget), record.effectiveTargetAddress
        ))
    end
    output:write("]}\n")
    output:close()
    client.exitCode(0)
end

local function slot_name(selector)
    if selector == 0 then return "slot1" end
    return "slot2"
end

local function complete_direct_case()
    local case = active
    local selected_slot = case.selector
    local storage = read_slot(selected_slot)
    local record = {
        id = case.id,
        operation = case.operation,
        selector = case.selector,
        selectedSlot = slot_name(selected_slot),
        saveFlagsByte = read_sram_byte(config.storage.saveFlagsAddress),
        storedChecksumByte = storage.storedChecksumByte,
        computedChecksumByte = storage.computedChecksumByte,
        storedPayloadSamples = storage.storedPayloadSamples,
        restoredPayloadSamples = read_payload_samples(),
    }
    if case.operation == "copy" then
        record.sourceSlot = slot_name(case.selector)
        record.destinationSlot = slot_name(case.observedDestinationSelector)
        record.observedDestinationSelector = case.observedDestinationSelector
        local destination_storage = read_slot(case.observedDestinationSelector)
        record.selectedSlot = record.destinationSlot
        record.storedChecksumByte = destination_storage.storedChecksumByte
        record.computedChecksumByte = destination_storage.computedChecksumByte
        record.storedPayloadSamples = destination_storage.storedPayloadSamples
    end
    direct_records[#direct_records + 1] = record
    active = nil
    direct_index = direct_index + 1
    if direct_index > #config.cases.directService then
        phase = "load-control"
    end
end

local function start_direct_case()
    local case = config.cases.directService[direct_index]
    active = {
        id = case.id,
        operation = case.operation,
        selector = case.selector,
    }
    if case.operation == "save" then write_payload(case.patternSeed) end
    if case.operation == "load" then poison_payload() end
    if case.operation == "save" then
        write_service_call(config["function"].saveGameAddress, case.selector)
    elseif case.operation == "load" then
        write_service_call(config["function"].loadGameAddress, case.selector)
    elseif case.operation == "copy" then
        write_service_call(config["function"].copySaveAddress, case.selector)
    elseif case.operation == "delete" then
        write_service_call(config["function"].clearSaveSlotFlagAddress, case.selector)
    else
        error("unknown direct service operation: " .. tostring(case.operation))
    end
    status("milestone:direct:" .. case.id)
end

local function start_precondition()
    write_service_call(config["function"].clearSaveSlotFlagAddress, precondition_selector)
    status("milestone:precondition-delete-slot" .. tostring(precondition_selector + 1))
end

local function set_flag88(value)
    local address = config.ram.gameFlagsAddress + flag88_byte_offset
    local current = memory.read_u8(address, "M68K BUS")
    if value then
        memory.write_u8(address, current | flag88_mask, "M68K BUS")
    else
        memory.write_u8(address, current & ((~flag88_mask) & 0xFF), "M68K BUS")
    end
end

local function start_control_case()
    local case = config.cases.loadControlFlow[control_index]
    phase = "load-control"
    active = { id = case.id, flag88Set = case.flag88Set }
    set_flag88(case.flag88Set)
    write_jump(config["function"].loadFlagTrapAddress, thunk_address)
    status("milestone:load-control:" .. case.id)
end

local function complete_control_case(instruction_target, instruction_address, effective_target, effective_address)
    control_records[#control_records + 1] = {
        id = active.id,
        flag88Set = active.flag88Set,
        instructionTarget = instruction_target,
        instructionTargetAddress = instruction_address,
        effectiveTarget = effective_target,
        effectiveTargetAddress = effective_address,
    }
    active = nil
    control_index = control_index + 1
    if control_index > #config.cases.loadControlFlow then
        write_observation()
    end
end

local function advance_after_normal_return()
    complete_control_case(
        config["function"].normalInstructionTarget,
        active.instructionTargetAddress,
        config["function"].normalEffectiveTarget,
        active.effectiveTargetAddress
    )
    if control_index <= #config.cases.loadControlFlow then start_control_case() end
end

event.on_bus_exec(function()
    if phase == "direct" and active ~= nil then
        status(string.format("milestone:service-entry:save:%s:d0=%d", active.id, register("D0") & 0xFF))
    end
    if phase == "direct" and active ~= nil and active.operation == "copy" then
        active.observedDestinationSelector = register("D0") & 1
    end
end, config["function"].saveGameAddress, "sf2-witch-save-save", "M68K BUS")

event.on_bus_exec(function()
    if phase == "direct" and active ~= nil then
        status(string.format("milestone:service-entry:load:%s:d0=%d", active.id, register("D0") & 0xFF))
    end
    if phase == "direct" and active ~= nil and active.operation == "copy" then
        active.observedSourceSelector = register("D0") & 1
    end
end, config["function"].loadGameAddress, "sf2-witch-save-load", "M68K BUS")

event.on_bus_exec(function()
    if phase == "await-checkpoint" then
        phase = "precondition"
        start_precondition()
    elseif phase == "precondition" then
        if precondition_selector == 0 then
            precondition_selector = 1
            start_precondition()
        else
            phase = "direct"
            start_direct_case()
        end
    elseif phase == "direct" and active ~= nil then
        complete_direct_case()
        if phase == "direct" then
            start_direct_case()
        else
            start_control_case()
        end
    elseif phase == "normal-return" and active ~= nil then
        advance_after_normal_return()
    end
end, checkpoint_address, "sf2-witch-save-checkpoint", "M68K BUS")

event.on_bus_exec(function()
    if phase ~= "await-sram" then return end
    local return_address = register("A7") & 0xFFFFFF
    memory.write_u32_be(return_address, checkpoint_address, "M68K BUS")
    write_jump(thunk_address, checkpoint_address)
    write_jump(checkpoint_address, thunk_address)
    phase = "await-checkpoint"
    status(string.format("milestone:check-sram-return-replaced:sp=%X,return=%X", return_address,
        memory.read_u32_be(return_address, "M68K BUS")))
end, config["function"].checkSramAddress, "sf2-witch-save-check-sram", "M68K BUS")

event.on_bus_exec(function()
    if phase == "load-control" and active ~= nil and not active.flag88Set then
        active.instructionTargetAddress = register("PC")
        active.effectiveTargetAddress = register("PC")
        memory.write_u32_be(register("A7") & 0xFFFFFF, checkpoint_address, "M68K BUS")
        phase = "normal-return"
    end
end, config["function"].normalInstructionTargetAddress, "sf2-witch-save-normal", "M68K BUS")

event.on_bus_exec(function()
    if phase == "load-control" and active ~= nil and active.flag88Set then
        active.instructionTargetAddress = register("PC")
    end
end, config["function"].suspendInstructionTargetAddress, "sf2-witch-save-suspend-alias", "M68K BUS")

event.on_bus_exec(function()
    if phase == "load-control" and active ~= nil and active.flag88Set then
        complete_control_case(
            config["function"].suspendInstructionTarget,
            active.instructionTargetAddress,
            config["function"].suspendEffectiveTarget,
            register("PC")
        )
    end
end, config["function"].suspendEffectiveTargetAddress, "sf2-witch-save-suspend-effective", "M68K BUS")

while true do
    frame_count = frame_count + 1
    joypad.set({ Start = true }, 1)
    joypad.set({}, 2)
    emu.frameadvance()
    if frame_count % 600 == 0 then
        status(string.format("frame=%d,phase=%s,direct=%d,control=%d,pc=%X", frame_count,
            phase, direct_index, control_index, register("PC")))
    end
end
