local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local active = nil
local case_index = 1
local results = {}
local replay_state = nil
local pending_save = false
local pending_replay = false
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "w")); file:write(value .. "\n"); file:close()
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

local function entry(combatant)
    local slot = combatant
    if combatant >= 128 then slot = combatant - 96 end
    return config.ram.combatantDataAddress + slot * config.ram.combatantEntrySize
end

local function write_items(base, items)
    for slot = 0, 3 do
        memory.write_u16_be(base + 32 + slot * 2, items[slot + 1], "M68K BUS")
    end
end

local function read_items(base)
    local items = {}
    for slot = 0, 3 do
        items[#items + 1] = memory.read_u16_be(base + 32 + slot * 2, "M68K BUS") & 0x7FFF
    end
    return items
end

local function flag_value(flag)
    local address = config.ram.enemyItemDroppedFlagsAddress + math.floor(flag / 8)
    local mask = 1 << (flag % 8)
    return (memory.read_u8(address, "M68K BUS") & mask) ~= 0
end

local function set_flag(flag)
    local address = config.ram.enemyItemDroppedFlagsAddress + math.floor(flag / 8)
    local mask = 1 << (flag % 8)
    memory.write_u8(address, memory.read_u8(address, "M68K BUS") | mask, "M68K BUS")
end

local function deals_amount(item)
    local packed = memory.read_u8(config.ram.dealsItemsAddress + math.floor(item / 2), "M68K BUS")
    if item % 2 == 0 then return (packed >> 4) & 0xF end
    return packed & 0xF
end

local function set_deals_amount(item, amount)
    local address = config.ram.dealsItemsAddress + math.floor(item / 2)
    if item % 2 == 0 then
        memory.write_u8(address, amount << 4, "M68K BUS")
    else
        memory.write_u8(address, amount, "M68K BUS")
    end
end

local function json_boolean(value)
    if value then return "true" end
    return "false"
end

local function json_number_or_null(value)
    if value == nil then return "null" end
    return tostring(value)
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format('{"system":"%s","core":"Genesis Plus GX","result":{"cases":[',
        emu.getsystemid()))
    for index, result in ipairs(results) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","roll":%s,"finalFlag":%s,"finalTargetItem":%d,"finalActorItems":[%d,%d,%d,%d],"finalDealsAmount":%d}',
            result.id, json_number_or_null(result.roll), json_boolean(result.finalFlag),
            result.finalTargetItem, result.finalActorItems[1], result.finalActorItems[2],
            result.finalActorItems[3], result.finalActorItems[4], result.finalDealsAmount))
    end
    output:write("]}}\n")
    output:close()
    if replay_state ~= nil then memorysavestate.removestate(replay_state) end
    client.exitCode(0)
end

local function finish_case()
    active.finalFlag = flag_value(active.flag)
    active.finalTargetItem = memory.read_u16_be(entry(active.target) + 32, "M68K BUS") & 0x7FFF
    active.finalActorItems = read_items(entry(config.actor))
    active.finalDealsAmount = deals_amount(active.item)
    results[#results + 1] = active
    active = nil
    case_index = case_index + 1
    if case_index > #config.cases then
        write_result_and_exit()
    else
        pending_replay = true
        status("milestone:replay-case:" .. case_index)
    end
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config["function"].battleTestAddress, "sf2-drop-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config["function"].numberPromptAddress, "sf2-drop-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config["function"].flagPromptAddress, "sf2-drop-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    stage = "battle"
    local actor = entry(config.actor)
    memory.write_u16_be(actor + 12, 100, "M68K BUS")
    memory.write_u16_be(actor + 14, 100, "M68K BUS")
    memory.write_u8(actor + 19, 99, "M68K BUS")
    memory.write_u8(actor + 21, 20, "M68K BUS")
    memory.write_u8(actor + 23, 99, "M68K BUS")
    memory.write_u8(actor + 31, 0, "M68K BUS")
    memory.write_u8(actor + 49, 0x80, "M68K BUS")
    memory.write_u16_be(actor + 52, 4, "M68K BUS")

    local target = entry(128)
    memory.write_u16_be(target + 12, 100, "M68K BUS")
    memory.write_u16_be(target + 14, 100, "M68K BUS")
    memory.write_u8(target + 21, 20, "M68K BUS")
    memory.write_u8(target + 23, 1, "M68K BUS")
    memory.write_u8(target + 46, 8, "M68K BUS")
    memory.write_u8(target + 47, 17, "M68K BUS")
    memory.write_u8(target + 49, 0x60, "M68K BUS")
    memory.write_u8(config.ram.terrainDataAddress + 17 * 48 + 8, 3, "M68K BUS")
    pending_save = true
    status("milestone:battle-setup")
end, config["function"].turnOrderEntryAddress, "sf2-drop-turn-order", "M68K BUS")

event.on_bus_exec(function()
    memory.write_u16_be(config.ram.seedAddress, 0, "M68K BUS")
end, config["function"].criticalEntryAddress, "sf2-drop-critical-seed", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then return end
    local current_case = config.cases[case_index]
    if replay_state == nil then error("enemy drop replay state was not captured before execution") end
    local a2 = emu.getregister("M68K A2") & 0xFFFFFF
    local a5 = emu.getregister("M68K A5") & 0xFFFFFF
    local empty = { config.emptyItem, config.emptyItem, config.emptyItem, config.emptyItem }
    local target = entry(current_case.target)

    memory.write_u8(config.ram.currentBattleAddress, current_case.battle, "M68K BUS")
    memory.write_u8(a5, current_case.target, "M68K BUS")
    memory.write_u8(a2 - 4, 0xFF, "M68K BUS")
    for offset = 0, 3 do
        memory.write_u8(config.ram.enemyItemDroppedFlagsAddress + offset, 0, "M68K BUS")
    end
    for offset = 0, 63 do
        memory.write_u8(config.ram.dealsItemsAddress + offset, 0, "M68K BUS")
    end
    set_deals_amount(current_case.item, current_case.initialDealsAmount)
    if current_case.initialFlag then set_flag(current_case.flag) end
    memory.write_u16_be(config.ram.seedAddress, current_case.seed, "M68K BUS")
    memory.write_u16_be(entry(config.actor) + 14, current_case.actorHp, "M68K BUS")
    write_items(entry(config.actor), current_case.actorItems)
    write_items(target, { current_case.item, config.emptyItem, config.emptyItem, config.emptyItem })

    active = {
        id = current_case.id,
        flag = current_case.flag,
        item = current_case.item,
        target = current_case.target,
        roll = nil
    }
    status("milestone:drop-entry:" .. case_index)
end, config["function"].entryAddress, "sf2-drop-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then active.roll = emu.getregister("M68K D0") & 0xFFFF end
end, config["function"].randomResultAddress, "sf2-drop-random-result", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil then finish_case() end
end, config["function"].returnAddress, "sf2-drop-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    if pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
        status("milestone:replay-state-saved")
    elseif pending_replay then
        pending_replay = false
        memorysavestate.loadcorestate(replay_state)
    end
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.ram.debugModeAddress, "M68K BUS") == 255 then
            button = "Up"
        end
    elseif #queue > 0 then
        button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1 then
        button = "C"
    end
    set_button(button)
    joypad.set({ Start = stage == "ui" and memory.read_u8(config.ram.currentBattleAddress, "M68K BUS") == 1 }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then
        status(string.format("frame=%d,stage=%s,case=%d,active=%s,pc=%X,battle=%d", frames,
            stage, case_index, tostring(active ~= nil), emu.getregister("M68K PC"),
            memory.read_u8(config.ram.currentBattleAddress, "M68K BUS")))
    end
end
