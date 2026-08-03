local config_path = assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")
local config = assert(dofile(config_path))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage = "cheat"
local prompt_count = 0
local queue = {}
local active = nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFF
end

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

local function slot_address(slot)
    return config.ram.combatantDataAddress + slot * config.ram.combatantEntrySize
end

local function write_input(slot, value)
    local target = slot_address(slot)
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
    memory.write_u16_be(target + 44, value.status, "M68K BUS")
end

local function snapshot(slot)
    local source = slot_address(slot)
    return {
        currentAttack = memory.read_u8(source + 19, "M68K BUS"),
        currentDefense = memory.read_u8(source + 21, "M68K BUS"),
        currentAgility = memory.read_u8(source + 23, "M68K BUS"),
        currentMove = memory.read_u8(source + 25, "M68K BUS"),
        currentResistance = memory.read_u16_be(source + 28, "M68K BUS"),
        currentProwess = memory.read_u8(source + 31, "M68K BUS"),
        status = memory.read_u16_be(source + 44, "M68K BUS")
    }
end

local function write_result_and_exit()
    local value = active.after
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","result":{"id":"%s","combatant":%d,' ..
        '"battle":%d,"after":{"currentAttack":%d,"currentDefense":%d,' ..
        '"currentAgility":%d,"currentMove":%d,"currentResistance":%d,"currentProwess":%d,' ..
        '"status":%d},"applyItemCalls":%d,"enemyBranchObserved":%s}}\n',
        emu.getsystemid(), active.id, active.combatant,
        memory.read_u8(config.ram.currentBattleAddress, "M68K BUS"),
        value.currentAttack, value.currentDefense, value.currentAgility, value.currentMove,
        value.currentResistance, value.currentProwess, value.status, active.applyItemCalls,
        tostring(active.enemyBranchObserved)))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end, config["function"].battleTestAddress, "sf2-enemy-curse-battle-test", "M68K BUS")

event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    status("milestone:number-prompt:" .. prompt_count)
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config["function"].numberPromptAddress, "sf2-enemy-curse-number-prompt", "M68K BUS")

event.on_bus_exec(function()
    status("milestone:flag-prompt")
    pulse("B")
end, config["function"].flagPromptAddress, "sf2-enemy-curse-flag-prompt", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil or reg("D0") ~= config.case.combatant then return end
    stage = "enemy-init"
    active = {
        id = config.case.id, combatant = config.case.combatant,
        applyItemCalls = 0, enemyBranchObserved = false, inputWritten = false
    }
    status("milestone:enemy-init")
end, config["function"].initializeEnemyStatsAddress, "sf2-enemy-curse-enemy-init", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or active.inputWritten or reg("D0") ~= active.combatant then return end
    write_input(config.case.ramSlot, config.case.input)
    active.inputWritten = true
    status("milestone:enemy-update")
end, config["function"].updateStatsEntryAddress, "sf2-enemy-curse-update-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil and reg("D0") == active.combatant then
        active.applyItemCalls = active.applyItemCalls + 1
    end
end, config["function"].applyItemEntryAddress, "sf2-enemy-curse-item-entry", "M68K BUS")

event.on_bus_exec(function()
    if active ~= nil and reg("D0") == active.combatant then
        active.enemyBranchObserved = true
    end
end, config["function"].enemyBranchAddress, "sf2-enemy-curse-enemy-branch", "M68K BUS")

event.on_bus_exec(function()
    if active == nil or not active.inputWritten then return end
    active.after = snapshot(config.case.ramSlot)
    write_result_and_exit()
end, config["function"].updateStatsReturnAddress, "sf2-enemy-curse-update-return", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
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
        status(string.format("frame=%d,stage=%s,pc=%X,prompts=%d,queue=%d,battle=%d", frames,
            stage, emu.getregister("M68K PC"), prompt_count, #queue,
            memory.read_u8(config.ram.currentBattleAddress, "M68K BUS")))
    end
end
