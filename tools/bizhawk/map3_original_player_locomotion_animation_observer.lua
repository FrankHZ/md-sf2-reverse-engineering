local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local json = assert(loadfile(config.jsonModulePath))()

-- Controlled original Witch/New admission is retained only to reach the first
-- original Map 3 WaitForEvent.  All movement and sprite observations below are
-- produced by original enabled-VInt code from one replayable admitted state.
local phase, frame_count = "await-check-sram", 0
local callbacks, callback_order = {}, {}
local scope, bootstrap_state, admission_state = nil, nil, nil
local pending_bootstrap_save, pending_admission_save = false, false
local pending_failure, finish_pending = nil, false
local active_case, case_index, records = nil, 0, {}
local current_tick, admission_sprite = nil, nil
local admission = nil

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function reg(name)
    return emu.getregister("M68K " .. name) & 0xFFFFFFFF
end

local function read_span(address, length)
    local values = {}
    for offset = 0, length - 1 do
        values[#values + 1] = memory.read_u8(address + offset, "M68K BUS")
    end
    return values
end

local function restore_span(address, values)
    for offset, value in ipairs(values) do
        memory.write_u8(address + offset - 1, value, "M68K BUS")
    end
end

local function first_mismatch(domain, address, expected)
    for offset, value in ipairs(expected) do
        local actual = memory.read_u8(address + offset - 1, "M68K BUS")
        if actual ~= value then
            return {domain = domain, address = address + offset - 1, expected = value, actual = actual}
        end
    end
    return nil
end

local function signed_word(address)
    local value = memory.read_u16_be(address, "M68K BUS")
    return value >= 0x8000 and value - 0x10000 or value
end

local function read_entity()
    local base, offset = config.ram.ENTITY_DATA, config.ram
    return {
        x = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_X, "M68K BUS"),
        y = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_Y, "M68K BUS"),
        xVelocity = signed_word(base + offset.ENTITYDEF_OFFSET_XVELOCITY),
        yVelocity = signed_word(base + offset.ENTITYDEF_OFFSET_YVELOCITY),
        xTravel = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_XTRAVEL, "M68K BUS"),
        yTravel = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_YTRAVEL, "M68K BUS"),
        xDest = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_XDEST, "M68K BUS"),
        yDest = memory.read_u16_be(base + offset.ENTITYDEF_OFFSET_YDEST, "M68K BUS"),
        facing = memory.read_u8(base + offset.ENTITYDEF_OFFSET_FACING, "M68K BUS"),
        animCounter = memory.read_u8(base + offset.ENTITYDEF_OFFSET_ANIMCOUNTER, "M68K BUS"),
    }
end

local function write_ram_jump(address, target)
    memory.write_u16_be(address, 0x4EF9, "M68K BUS")
    memory.write_u32_be(address + 2, target, "M68K BUS")
end

local function write_ram_rts(address)
    memory.write_u16_be(address, 0x4E75, "M68K BUS")
end

local function patch_bytes(patch, field)
    local values = {}
    local encoded = patch[field]
    for index = 1, #encoded, 2 do
        values[#values + 1] = tonumber(encoded:sub(index, index + 1), 16)
    end
    return values
end

local function write_cart_patch(patch)
    for offset, value in ipairs(patch_bytes(patch, "hex")) do
        local address = patch.address + offset - 1
        memory.write_u8(address, value, "M68K BUS")
        memory.write_u8(address, value, config.harness.romPatchDomain)
        assert(memory.read_u8(address, config.harness.romPatchDomain) == value,
            "session patch readback drift: " .. patch.purpose)
    end
end

local function restore_cart_patch(patch)
    for offset, value in ipairs(patch_bytes(patch, "originalHex")) do
        local address = patch.address + offset - 1
        memory.write_u8(address, value, "M68K BUS")
        memory.write_u8(address, value, config.harness.romPatchDomain)
        local actual = memory.read_u8(address, config.harness.romPatchDomain)
        if actual ~= value then
            return false, {domain = "sessionCartPatches", address = address, expected = value, actual = actual}
        end
    end
    return true, nil
end

local function cleanup_callbacks()
    local retained, first_error = {}, nil
    for _, address in ipairs(callback_order) do
        local callback = callbacks[address]
        if callback ~= nil then
            local ok, message = pcall(event.unregisterbyid, callback.id)
            if ok then callbacks[address] = nil else
                retained[#retained + 1] = address
                if first_error == nil then first_error = tostring(message) end
            end
        end
    end
    callback_order = retained
    local cleared = next(callbacks) == nil
    return cleared, first_error
end

local function restoration_facts(armed)
    return {
        scopeArmed = armed,
        bootstrapStateRestored = false,
        playerEntity = false,
        mapAndBattleState = false,
        generatedRam = false,
        sessionCartPatches = false,
        callbacksCleared = false,
        outputRemoved = false,
        sessionRomDeleted = false,
    }
end

local function restore_scope()
    local facts = restoration_facts(scope ~= nil and bootstrap_state ~= nil)
    if not facts.scopeArmed then
        return facts, {domain = "scope", address = config.harness.checkpointAddress, expected = 1, actual = 0}
    end
    local ok = pcall(memorysavestate.loadcorestate, bootstrap_state)
    if not ok then
        return facts, {domain = "scope", address = config.harness.checkpointAddress, expected = 1, actual = 0}
    end
    facts.bootstrapStateRestored = true
    restore_span(config.harness.checkpointAddress, scope.generatedRam)
    local mismatch = first_mismatch("generatedRam", config.harness.checkpointAddress, scope.generatedRam)
    if mismatch ~= nil then return facts, mismatch end
    facts.generatedRam = true
    for _, patch in ipairs(config.sessionPatches) do
        local restored, patch_mismatch = restore_cart_patch(patch)
        if not restored then return facts, patch_mismatch end
    end
    facts.sessionCartPatches = true
    mismatch = first_mismatch("playerEntity", config.ram.ENTITY_DATA, scope.playerEntity)
    if mismatch ~= nil then return facts, mismatch end
    facts.playerEntity = true
    mismatch = first_mismatch("mapAndBattleState", config.ram.CURRENT_MAP, scope.mapAndBattleState)
    if mismatch ~= nil then return facts, mismatch end
    facts.mapAndBattleState = true
    return facts, nil
end

local function fail(role, expected_pc, message)
    if pending_failure ~= nil then return end
    pending_failure = {
        role = role,
        expectedPc = expected_pc,
        actualPc = reg("PC") & 0xFFFFFF,
        phase = phase,
        message = tostring(message),
    }
end

local function finalize_failure()
    local facts, mismatch = restore_scope()
    local cleanup_ok, cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_ok then cleanup_error, cleared = cleared, false end
    os.remove(config.outputPath)
    local output = io.open(config.outputPath, "r")
    local output_removed = output == nil
    if output ~= nil then output:close() end
    facts.callbacksCleared, facts.outputRemoved = cleared, output_removed
    local payload = {
        owner = config.observerFailureContract.owner,
        caseId = active_case and active_case.caseId or (admission and "admission" or "bootstrap"),
        phase = pending_failure.phase,
        role = pending_failure.role,
        actualPc = pending_failure.actualPc,
        expectedPc = pending_failure.expectedPc or json.null,
        callbackCount = #callback_order,
        callbacksCleared = cleared,
        outputRemoved = output_removed,
        restoration = facts,
        restorationMismatch = mismatch or json.null,
        error = pending_failure.message .. (cleanup_error and ("; cleanup=" .. cleanup_error) or ""),
    }
    status(config.observerFailureContract.statusPrefix .. json.encode(payload))
    client.exitCode(config.observerFailureContract.exitCode)
end

local function add_callback(address, role, handler)
    assert(callbacks[address] == nil,
        "duplicate physical-PC callback at " .. string.format("%X", address))
    local entry = {role = role, expectedPc = address}
    callbacks[address] = entry
    callback_order[#callback_order + 1] = address
    entry.id = event.on_bus_exec(function()
        if pending_failure ~= nil then return end
        local ok, message = pcall(handler)
        if not ok then fail(role, address, message) end
    end, address, "sf2-map3-player-locomotion-" .. role, "M68K BUS")
end

local function write_menu_thunk(case)
    local address = config.harness.menuThunkAddress
    memory.write_u16_be(address, 0x0C41, "M68K BUS")
    memory.write_u16_be(address + 2, config.witchNewAction.initialMenuPage, "M68K BUS")
    memory.write_u16_be(address + 4, 0x6604, "M68K BUS")
    memory.write_u16_be(address + 6, 0x7001, "M68K BUS")
    write_ram_rts(address + 8)
    memory.write_u16_be(address + 10, 0x7000, "M68K BUS")
    write_ram_rts(address + 12)
end

local function begin_case(index, load_replay)
    case_index = index
    if case_index > #config.cases then
        finish_pending, phase = true, "finish-pending"
        return
    end
    if load_replay then memorysavestate.loadcorestate(admission_state) end
    local case = config.cases[case_index]
    active_case = {
        caseId = case.caseId,
        direction = case.direction,
        seed = read_entity(),
        ticks = {},
        inputObserved = false,
        inputTick = nil,
        motionInstalled = false,
    }
    current_tick = nil
    phase = "case-running"
end

local function finish_case(outcome)
    assert(active_case ~= nil, "case completion without active case")
    active_case.outcome = outcome
    active_case.settled = read_entity()
    records[#records + 1] = active_case
    status("milestone:case-finished:" .. active_case.caseId)
    active_case = nil
    begin_case(case_index + 1, true)
end

local function maybe_finish_case()
    if phase ~= "case-running" or active_case == nil or not active_case.inputObserved then return end
    local ticks = active_case.ticks
    if #ticks == 0 then return end
    local state = ticks[#ticks].afterMovement
    if state == nil then return end
    local seed = active_case.seed
    local distance = math.abs(state.x - seed.x) + math.abs(state.y - seed.y)
    if active_case.motionInstalled then
        if distance == 384 and state.x == state.xDest and state.y == state.yDest
            and state.xTravel == 0 and state.yTravel == 0 then
            finish_case("moved-one-tile")
        end
    elseif #ticks >= active_case.inputTick
        and state.x == seed.x and state.y == seed.y
        and state.xDest == seed.xDest and state.yDest == seed.yDest
        and state.xTravel == 0 and state.yTravel == 0
        and ticks[#ticks].inputAttempt ~= json.null
        and ticks[#ticks].inputAttempt.after.facing == config.cases[case_index].facing then
        finish_case("blocked-no-movement")
    end
end

local function finalize_success()
    local facts, mismatch = restore_scope()
    if mismatch ~= nil then
        fail("restoration", nil, "restoration mismatch: " .. json.encode(mismatch))
        return false
    end
    local cleanup_ok, cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_ok or not cleared then
        fail("callback-cleanup", nil, "callback cleanup failed: " .. tostring(cleanup_error or cleared))
        return false
    end
    facts.callbacksCleared = true
    local observation = {
        system = config.fixtureId,
        caseOrder = config.caseOrder,
        admission = admission,
        records = records,
        callbacksCleared = true,
        restoration = facts,
    }
    json.write(config.outputPath, observation)
    status("milestone:callbacks-cleared:0")
    status("milestone:observer-finished")
    client.exitCode(0)
    return true
end

status("milestone:observer-started")

add_callback(config.functions.checkSramAddress, "bootstrap-check-sram", function()
    if phase ~= "await-check-sram" then return end
    scope = {
        playerEntity = read_span(config.ram.ENTITY_DATA, config.ram.ENTITYDEF_SIZE),
        mapAndBattleState = read_span(config.ram.CURRENT_MAP, 9),
        generatedRam = read_span(config.harness.checkpointAddress, config.harness.generatedRamBytes),
    }
    memory.write_u32_be(reg("A7") & 0xFFFFFF, config.harness.checkpointAddress, "M68K BUS")
    write_ram_jump(config.harness.checkpointAddress, config.harness.checkpointAddress)
    for _, patch in ipairs(config.sessionPatches) do write_cart_patch(patch) end
    pending_bootstrap_save, phase = true, "await-safe-core-snapshot"
    status("milestone:scope-snapshotted-before-write")
end)

add_callback(config.harness.checkpointAddress, "checkpoint", function()
    if phase ~= "await-checkpoint" then return end
    write_menu_thunk(config.cases[1])
    write_ram_jump(config.harness.checkpointAddress, config.functions.newActionAddress)
    phase = "await-witch-new-action"
    status("milestone:controlled-new-admission-started")
end)

add_callback(config.functions.newActionAddress, "witch-new-action", function()
    if phase == "await-witch-new-action" then phase = "await-wait-for-event" end
end)

add_callback(config.functions.waitForEvent, "wait-for-event", function()
    if phase ~= "await-wait-for-event" then return end
    assert(memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS") == config.map3.mapIndex,
        "controlled admission did not reach Map 3")
    assert(admission_sprite ~= nil and admission_sprite.counterAfter ~= nil,
        "no original VInt sprite branch preceded first WaitForEvent")
    admission = {
        boundary = "first-original-WaitForEvent-entry",
        entity = read_entity(),
        sprite = admission_sprite,
    }
    pending_admission_save, phase = true, "await-admission-state-save"
    status("milestone:first-wait-for-event-observed")
end)

add_callback(config.functions.vintUpdateEntities, "vint-update-entities", function()
    if phase ~= "case-running" then return end
    assert(current_tick == nil, "enabled VInt began before prior sprite observation completed")
    current_tick = {
        tick = #active_case.ticks + 1,
        beforeEntities = read_entity(),
        afterMovement = nil,
        inputAttempt = json.null,
        sprite = nil,
    }
end)

add_callback(config.functions.updateEntityDataReturn, "update-entity-return", function()
    if phase ~= "case-running" or (reg("A0") & 0xFFFFFF) ~= config.ram.ENTITY_DATA then return end
    assert(current_tick ~= nil and current_tick.afterMovement == nil,
        "controlled entity movement-return chronology drift")
    current_tick.afterMovement = read_entity()
end)

add_callback(config.functions.controlCharacter, "control-character", function()
    if phase ~= "case-running" or (reg("A0") & 0xFFFFFF) ~= config.ram.ENTITY_DATA then return end
    assert(current_tick ~= nil and current_tick.afterMovement ~= nil
        and current_tick.inputAttempt == json.null,
        "controlled entity input callback chronology drift")
    current_tick.inputAttempt = {
        before = read_entity(),
        playerInput = memory.read_u8(config.ram.CURRENT_PLAYER_INPUT, "M68K BUS"),
        after = json.null,
    }
    if current_tick.inputAttempt.playerInput ~= 0 and not active_case.inputObserved then
        active_case.inputObserved = true
        active_case.inputTick = current_tick.tick
    end
end)

add_callback(config.functions.nextEntity, "next-entity", function()
    if phase ~= "case-running" or (reg("A0") & 0xFFFFFF) ~= config.ram.ENTITY_DATA then return end
    if current_tick == nil or current_tick.inputAttempt == json.null
        or current_tick.inputAttempt.after ~= json.null then return end
    current_tick.inputAttempt.after = read_entity()
    if active_case.inputObserved and current_tick.tick == active_case.inputTick then
        local state, seed = current_tick.inputAttempt.after, active_case.seed
        active_case.motionInstalled = state.xDest ~= seed.xDest or state.yDest ~= seed.yDest
            or state.xTravel ~= 0 or state.yTravel ~= 0
    end
end)

local function observe_half(selected_half)
    if (reg("A0") & 0xFFFFFF) ~= config.ram.ENTITY_DATA then return end
    local sprite = {
        selectedHalf = selected_half,
        counterAtSelection = memory.read_u8(
            config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_ANIMCOUNTER, "M68K BUS"),
        counterAfter = nil,
    }
    if phase == "case-running" then
        -- A replay resumes at the exact admitted CPU point, which can be after
        -- this frame's entity updater.  Do not manufacture a partial tick.
        if current_tick == nil then return end
        assert(current_tick ~= nil and current_tick.sprite == nil, "sprite half callback chronology drift")
        current_tick.sprite = sprite
    elseif phase == "await-wait-for-event" then
        admission_sprite = sprite
    end
end

add_callback(config.functions.spriteHalf0, "sprite-half-0", function() observe_half(0) end)
add_callback(config.functions.spriteHalf1, "sprite-half-1", function() observe_half(1) end)

add_callback(config.functions.spriteCounterAfter, "sprite-counter-after", function()
    if (reg("A0") & 0xFFFFFF) ~= config.ram.ENTITY_DATA then return end
    local counter = memory.read_u8(
        config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_ANIMCOUNTER, "M68K BUS")
    if phase == "case-running" then
        if current_tick == nil then return end
        assert(current_tick ~= nil and current_tick.sprite ~= nil, "counter callback preceded half selection")
        if current_tick.afterMovement == nil then
            -- The admitted replay point can resume after this frame's entity
            -- updater.  Exclude that setup-only partial VInt.
            current_tick = nil
            return
        end
        if current_tick.inputAttempt ~= json.null and current_tick.inputAttempt.after == json.null then
            error("sprite callback preceded controlled-input return")
        end
        current_tick.sprite.counterAfter = counter
        active_case.ticks[#active_case.ticks + 1] = current_tick
        current_tick = nil
    elseif phase == "await-wait-for-event" and admission_sprite ~= nil then
        admission_sprite.counterAfter = counter
    end
end)

while true do
    frame_count = frame_count + 1
    if frame_count > config.harness.bootstrapFrameBudget + config.harness.caseFrameBudget then
        local role = phase:find("case", 1, true) and "case-watchdog" or "bootstrap-watchdog"
        fail(role, nil, "frame budget exceeded at phase " .. phase)
    end
    if pending_failure ~= nil then finalize_failure(); return end
    if pending_bootstrap_save then
        pending_bootstrap_save = false
        bootstrap_state = memorysavestate.savecorestate()
        phase = "await-checkpoint"
        status("milestone:core-state-saved-outside-callback")
    elseif pending_admission_save then
        pending_admission_save = false
        admission_state = memorysavestate.savecorestate()
        status("milestone:admission-state-saved-outside-callback")
        begin_case(1, false)
    elseif finish_pending then
        if finalize_success() then return end
    else
        maybe_finish_case()
    end
    if pending_failure ~= nil then finalize_failure(); return end
    local buttons = {}
    if phase == "await-check-sram" then
        buttons.Start = true
    elseif phase == "await-witch-new-action" or phase == "await-wait-for-event" then
        local pulse = frame_count % 42
        if pulse >= 30 and pulse < 34 then buttons.C = true end
    elseif phase == "case-running" and active_case ~= nil and not active_case.inputObserved then
        buttons[config.cases[case_index].joypadName] = true
    end
    joypad.set(buttons, 1)
    joypad.set({}, 2)
    emu.frameadvance()
end
