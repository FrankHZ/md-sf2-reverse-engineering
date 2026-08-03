local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage, prompt_count = "cheat", 0
local queue = {}
local setup_done, instrumented = false, false
local records = {}
local records_ready, conversion = false, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "a")); file:write(value .. "\n"); file:close()
end
local function enqueue(name, count) for _ = 1, count do queue[#queue + 1] = name end end
local function pulse(name) enqueue("", 30); enqueue(name, 4); enqueue("", 8) end
local function set_button(name)
    local buttons = {}; if name ~= nil and name ~= "" then buttons[name] = true end
    joypad.set(buttons, 1)
end

local function read_hex(address, length, domain)
    local bytes = {}
    for offset = 0, length - 1 do
        bytes[#bytes + 1] = string.format("%02X", memory.read_u8(address + offset, domain))
    end
    return table.concat(bytes)
end

local function write_result_and_exit()
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,"records":[',
        emu.getsystemid(), config.fixtureId,
        memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS")))
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"id":"%s","actor":%d,"target":%d,"seed":%d,"rngCalls":%d,"roll":%d,"inaction":%d}',
            record.id, record.actor, record.target, record.seed,
            record.rngCalls, record.roll, record.inaction))
    end
    output:write(string.format(
        '],"conversion":{"sourceCase":"%s","inaction":%d,"battleAction":%d}}\n',
        conversion.sourceCase, conversion.inaction, conversion.battleAction))
    output:close()
    client.exitCode(0)
end

event.on_bus_exec(function()
    stage = "ui"
    status("milestone:battle-test")
end,
    config.harness["function"].battleTestAddress, "sf2-muddle-guard-battle", "M68K BUS")
event.on_bus_exec(function()
    prompt_count = prompt_count + 1
    bootstrap.battle01_intro_skip(config.bootstrap.profile, prompt_count, pulse)
end, config.harness["function"].numberPromptAddress, "sf2-muddle-guard-number", "M68K BUS")
event.on_bus_exec(function() pulse("B") end,
    config.harness["function"].flagPromptAddress, "sf2-muddle-guard-flag", "M68K BUS")
event.on_bus_exec(function()
    if setup_done then return end
    setup_done, stage = true, "battle"
    memory.write_u8(config.ram.autoBattleToggleAddress, 0xFF, "M68K BUS")
end, config.harness["function"].turnOrderEntryAddress, "sf2-muddle-guard-setup", "M68K BUS")

event.on_bus_exec(function()
    if not setup_done or instrumented then return end
    local patch = config.instrumentation
    local call_bytes = read_hex(patch.callSiteAddress, #patch.callSiteOriginalHex // 2, "ROM")
    if call_bytes ~= patch.callSitePatchedHex then error("MUDDLE derived call bytes drifted") end
    local stub_bytes = read_hex(patch.stubAddress, #patch.stubOriginalHex // 2, "ROM")
    if stub_bytes ~= patch.stubHex then error("MUDDLE derived stub bytes drifted") end
    for index, case in ipairs(config.cases) do
        local slot = patch.ramInputAddress + (index - 1) * patch.recordSize
        memory.write_u16_be(slot, case.actor, "M68K BUS")
        memory.write_u16_be(slot + 2, case.target, "M68K BUS")
        memory.write_u16_be(slot + 4, case.seed, "M68K BUS")
        memory.write_u16_be(slot + 6, 0, "M68K BUS")
        memory.write_u16_be(slot + 8, 0, "M68K BUS")
    end
    instrumented = true
    status("milestone:instrumented")
end, config.instrumentation.patchTriggerAddress, "sf2-muddle-guard-patch", "M68K BUS")

event.on_bus_exec(function()
    if not instrumented then return end
    local patch = config.instrumentation
    for index, case in ipairs(config.cases) do
        local slot = patch.ramInputAddress + (index - 1) * patch.recordSize
        local final_seed = memory.read_u16_be(slot + 8, "M68K BUS")
        local rng_calls = final_seed == case.seed and 0 or 1
        local roll = -1
        if rng_calls == 1 then roll = math.floor(final_seed * 2 / 65536) end
        records[#records + 1] = { id=case.id, actor=case.actor, target=case.target,
            seed=case.seed, rngCalls=rng_calls, roll=roll,
            inaction=memory.read_u16_be(slot + 6, "M68K BUS") }
    end
    status("milestone:results")
    records_ready = true
end, config.instrumentation.resultAddress, "sf2-muddle-guard-results", "M68K BUS")

event.on_bus_exec(function()
    if not records_ready then return end
    conversion = {
        sourceCase=config.conversion.sourceCase,
        inaction=records[#records].inaction,
        battleAction=memory.read_u16_be(config.ram.currentBattleActionAddress, "M68K BUS")
    }
    status("milestone:muddled-action")
    write_result_and_exit()
end, config["function"].muddledActionAppliedAddress,
    "sf2-muddle-guard-action-applied", "M68K BUS")

local frames = 0
while true do
    frames = frames + 1
    local button = nil
    if stage == "cheat" then
        local pointer = memory.read_u32_be(config.harness.ram.cheatPointerAddress, "M68K BUS")
        if pointer >= 0x28FF0 and pointer < 0x29000 then
            button = names[cheat[pointer - 0x28FF0 + 1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress, "M68K BUS") == 255 then button = "Up" end
    elseif #queue > 0 then button = table.remove(queue, 1)
    elseif stage == "ui" and memory.read_u8(config.harness.ram.currentBattleAddress, "M68K BUS") == 1 then button = "C" end
    set_button(button)
    joypad.set({ Start = (stage == "ui" and memory.read_u8(
        config.harness.ram.currentBattleAddress, "M68K BUS") == 1) }, 2)
    emu.frameadvance()
    if frames % 600 == 0 then status(string.format(
        "frame=%d,stage=%s,instrumented=%s,records=%d",
        frames, stage, tostring(instrumented), #records)) end
end
