local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local active = nil

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
end

local function combatant_address(ally)
    return config.ram.combatantDataAddress + ally * config.ram.combatantEntrySize
end

local function write_input(ally, value)
    local target = combatant_address(ally)
    memory.write_u8(target + 10, value.class, "M68K BUS")
    memory.write_u8(target + 11, value.level, "M68K BUS")
    memory.write_u16_be(target + 12, value.maxHp, "M68K BUS")
    memory.write_u16_be(target + 14, 7, "M68K BUS")
    memory.write_u8(target + 16, value.maxMp, "M68K BUS")
    memory.write_u8(target + 17, 0, "M68K BUS")
    memory.write_u8(target + 18, value.baseAttack, "M68K BUS")
    memory.write_u8(target + 19, 1, "M68K BUS")
    memory.write_u8(target + 20, value.baseDefense, "M68K BUS")
    memory.write_u8(target + 21, 2, "M68K BUS")
    memory.write_u8(target + 22, value.baseAgility, "M68K BUS")
    memory.write_u8(target + 23, 3, "M68K BUS")
    memory.write_u8(target + 24, value.baseMove, "M68K BUS")
    memory.write_u8(target + 25, 1, "M68K BUS")
    for index, item in ipairs(value.items) do
        memory.write_u16_be(target + 30 + index * 2, item, "M68K BUS")
    end
    memory.write_u16_be(target + 44, value.status, "M68K BUS")
    memory.write_u8(target + 48, 0, "M68K BUS")
end

local function snapshot(ally)
    local source = combatant_address(ally)
    return {
        level = memory.read_u8(source + 11, "M68K BUS"),
        baseAttack = memory.read_u8(source + 18, "M68K BUS"),
        currentAttack = memory.read_u8(source + 19, "M68K BUS"),
        baseDefense = memory.read_u8(source + 20, "M68K BUS"),
        currentDefense = memory.read_u8(source + 21, "M68K BUS"),
        baseAgility = memory.read_u8(source + 22, "M68K BUS"),
        currentAgility = memory.read_u8(source + 23, "M68K BUS"),
        baseMove = memory.read_u8(source + 24, "M68K BUS"),
        currentMove = memory.read_u8(source + 25, "M68K BUS"),
        status = memory.read_u16_be(source + 44, "M68K BUS")
    }
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"id":"%s","ally":%d,"operations":[',
        emu.getsystemid(), active.id, active.ally))
    for index, operation in ipairs(active.operations) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","before":%d,"amount":%d,"after":%d}',
            operation.id, operation.before, operation.amount, operation.after))
    end
    output:write(string.format(
        '],"helpersObserved":{"increaseByte":%s,"increase7Bits":%s,"decreaseByte":%s},"after":',
        tostring(active.helpers.increaseByte), tostring(active.helpers.increase7Bits),
        tostring(active.helpers.decreaseByte)))
    local value = active.after
    output:write(string.format(
        '{"level":%d,"baseAttack":%d,"currentAttack":%d,"baseDefense":%d,"currentDefense":%d,' ..
        '"baseAgility":%d,"currentAgility":%d,"baseMove":%d,"currentMove":%d,"status":%d}}}\n',
        value.level, value.baseAttack, value.currentAttack, value.baseDefense, value.currentDefense,
        value.baseAgility, value.currentAgility, value.baseMove, value.currentMove, value.status))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    if reg("D0") ~= config.case.ally or active ~= nil then return end
    write_input(config.case.ally, config.case.input)
    memory.write_u16_be(config.ram.seedAddress, config.case.seed, "M68K BUS")
    active = {
        id = config.case.id, ally = config.case.ally, operations = {},
        helpers = { increaseByte = false, increase7Bits = false, decreaseByte = false }
    }
end, config["function"].levelUpEntryAddress, "sf2-stat-clamp-level-up", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.helpers.increaseByte = true end
end, config["function"].increaseAndClampByteAddress, "sf2-stat-clamp-increase-byte", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.helpers.increase7Bits = true end
end, config["function"].increaseAndClamp7BitsAddress, "sf2-stat-clamp-increase-7bits", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.helpers.decreaseByte = true end
end, config["function"].decreaseAndClampByteAddress, "sf2-stat-clamp-decrease-byte", "M68K BUS")

for index, configured in ipairs(config.case.operations) do
    local operation_index = index
    local operation = configured
    event.on_bus_exec(function()
        if active == nil then return end
        if reg("D0") ~= active.ally then return end
        if (reg("D1") & 0xFF) ~= operation.amount then return end
        if active.operations[operation_index] ~= nil then
            memory.write_u8(
                combatant_address(active.ally) + operation.fieldOffset,
                operation.before,
                "M68K BUS")
            return
        end
        local target = combatant_address(active.ally) + operation.fieldOffset
        memory.write_u8(target, operation.before, "M68K BUS")
        active.operations[operation_index] = {
            id = operation.id,
            before = memory.read_u8(target, "M68K BUS"),
            amount = reg("D1") & 0xFF,
            after = nil
        }
    end, config["function"][operation.entryKey .. "EntryAddress"],
        "sf2-stat-clamp-entry-" .. operation.id, "M68K BUS")

    event.on_bus_exec(function()
        if active == nil then return end
        local observed = active.operations[operation_index]
        if observed == nil or observed.after ~= nil then return end
        observed.after = memory.read_u8(
            combatant_address(active.ally) + operation.fieldOffset, "M68K BUS")
    end, config["function"][operation.entryKey .. "ReturnAddress"],
        "sf2-stat-clamp-return-" .. operation.id, "M68K BUS")
end

event.on_bus_exec(function()
    if active == nil then return end
    for index = 1, #config.case.operations do
        local operation = active.operations[index]
        if operation == nil or operation.after == nil then return end
    end
    active.after = snapshot(active.ally)
    write_result_and_exit()
end, config["function"].updateStatsReturnAddress, "sf2-stat-clamp-update-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format("frame=%d", frames)) end
end
