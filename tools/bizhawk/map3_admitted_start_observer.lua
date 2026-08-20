local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))

-- This is a controlled seam, not a natural Map 3 route.  It returns from the
-- original CheckSram path into a generated RAM checkpoint, admits exactly one
-- original Witch/New action, then permits the original MainLoop and exploration
-- setup code to reach its first WaitForEvent entry.
local phase, frame_count, bootstrap_new_game_hits = "await-check-sram", 0, 0
local active, saved_state, scope = nil, nil, nil
local callbacks, callback_order = {}, {}
local finish_pending, failed = false, false
local pending_core_snapshot, pending_failure = false, nil
local write_observation

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

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

status("milestone:observer-started")

local function register(name)
    return emu.getregister("M68K " .. name) & 0xFFFFFFFF
end

local function read_span(address, length)
    local values = {}
    for offset = 0, length - 1 do
        values[#values + 1] = memory.read_u8(address + offset, "M68K BUS")
    end
    return values
end

local function equal_span(address, expected)
    for offset, value in ipairs(expected) do
        if memory.read_u8(address + offset - 1, "M68K BUS") ~= value then return false end
    end
    return true
end

local function restore_span(address, expected)
    for offset, value in ipairs(expected) do
        memory.write_u8(address + offset - 1, value, "M68K BUS")
    end
end

local function write_work_ram_jump(target, address)
    memory.write_u16_be(address, 0x4EF9, "M68K BUS")
    memory.write_u32_be(address + 2, target, "M68K BUS")
end

local function write_work_ram_rts(address)
    memory.write_u16_be(address, 0x4E75, "M68K BUS")
end

local function write_cart_patch(patch)
    local bytes = {}
    for index = 1, #patch.hex, 2 do bytes[#bytes + 1] = tonumber(patch.hex:sub(index, index + 1), 16) end
    for offset, value in ipairs(bytes) do
        memory.write_u8(patch.address + offset - 1, value, "M68K BUS")
        memory.write_u8(patch.address + offset - 1, value, config.harness.romPatchDomain)
        assert(memory.read_u8(patch.address + offset - 1, config.harness.romPatchDomain) == value,
            "session patch did not read back: " .. patch.purpose)
    end
end

local function restore_cart_patch(patch)
    for index = 1, #patch.originalHex, 2 do
        local value = tonumber(patch.originalHex:sub(index, index + 1), 16)
        local address = patch.address + (index - 1) / 2
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
            local unregistered, unregister_error = pcall(event.unregisterbyid, callback.id)
            if unregistered then
                callbacks[address] = nil
            else
                retained[#retained + 1] = address
                if first_error == nil then first_error = tostring(unregister_error) end
            end
        end
    end
    callback_order = retained
    local cleared = next(callbacks) == nil
    if not cleared and first_error == nil then first_error = "residual callback registration" end
    return cleared, first_error
end

local function restoration_facts(armed)
    return {
        scopeArmed = armed,
        gameFlags = false,
        combatantAllyRecords = false,
        mapAndBattleState = false,
        playerEntity = false,
        gold = false,
        timeState = false,
        generatedRam = false,
        sessionCartPatches = false,
        sessionStateRestored = false,
        callbacksCleared = false,
        outputRemoved = false
    }
end

local function first_span_mismatch(domain, address, expected)
    for offset, value in ipairs(expected) do
        local actual = memory.read_u8(address + offset - 1, "M68K BUS")
        if actual ~= value then
            return {domain = domain, address = address + offset - 1, expected = value, actual = actual}
        end
    end
    return nil
end

local function restore_scope()
    local restoration = restoration_facts(scope ~= nil and saved_state ~= nil)
    if not restoration.scopeArmed then
        return restoration, {domain = "scope", address = config.harness.checkpointAddress, expected = 1, actual = 0}
    end
    local ok, restore_error = pcall(function()
        memorysavestate.loadcorestate(saved_state)
        restore_span(config.harness.checkpointAddress, scope.generatedRam)
    end)
    if not ok then
        return restoration, {domain = "scope", address = config.harness.checkpointAddress, expected = 1, actual = 0}
    end
    for _, patch in ipairs(config.sessionPatches) do
        local restored, mismatch = restore_cart_patch(patch)
        if not restored then return restoration, mismatch end
    end
    restoration.sessionCartPatches = true
    local checks = {
        {key = "gameFlags", address = config.ram.GAME_FLAGS, expected = scope.gameFlags},
        {key = "combatantAllyRecords", address = config.ram.COMBATANT_DATA, expected = scope.combatantAllyRecords},
        {key = "playerEntity", address = config.ram.ENTITY_DATA, expected = scope.playerEntity},
        {key = "gold", address = config.ram.CURRENT_GOLD, expected = scope.gold},
        {key = "mapAndBattleState", address = config.ram.CURRENT_MAP, expected = scope.mapAndBattleState},
        {key = "timeState", address = config.ram.FRAME_COUNTER, expected = scope.timeState.frameCounter},
        {key = "timeState", address = config.ram.RANDOM_SEED, expected = scope.timeState.randomSeed},
        {key = "timeState", address = config.ram.SECONDS_COUNTER, expected = scope.timeState.secondsCounter},
        {key = "timeState", address = config.ram.SECONDS_COUNTER_FRAMES, expected = scope.timeState.secondsCounterFrames},
        {key = "generatedRam", address = config.harness.checkpointAddress, expected = scope.generatedRam}
    }
    for _, check in ipairs(checks) do
        local mismatch = first_span_mismatch(check.key, check.address, check.expected)
        if mismatch ~= nil then return restoration, mismatch end
        restoration[check.key] = true
    end
    restoration.sessionStateRestored = true
    return restoration, nil
end

local function normalize_vint_time_outside_callback()
    assert(active ~= nil and active.rawVintTime ~= nil, "VInt time snapshot missing")
    memory.write_u8(config.ram.FRAME_COUNTER, 0, "M68K BUS")
    memory.write_u32_be(config.ram.SECONDS_COUNTER, 0, "M68K BUS")
    memory.write_u8(config.ram.SECONDS_COUNTER_FRAMES, 0, "M68K BUS")
    assert(memory.read_u8(config.ram.FRAME_COUNTER, "M68K BUS") == 0, "FRAME_COUNTER normalization readback drift")
    assert(memory.read_u32_be(config.ram.SECONDS_COUNTER, "M68K BUS") == 0, "SECONDS_COUNTER normalization readback drift")
    assert(memory.read_u8(config.ram.SECONDS_COUNTER_FRAMES, "M68K BUS") == 0, "SECONDS_COUNTER_FRAMES normalization readback drift")
    active.scenarioState.vintTime = {
        normalization = "post-boundary-controlled-zeroed-vint-counters",
        frameCounter = 0,
        secondsCounter = 0,
        secondsCounterFrames = 0
    }
    status("milestone:vint-time-normalized-outside-callback")
end

local function fail(role, expected_pc, message, checked_restoration, checked_mismatch)
    if failed then return end
    failed = true
    pending_failure = {
        role = role,
        expectedPc = expected_pc,
        actualPc = register("PC") & 0xFFFFFF,
        phase = phase,
        message = tostring(message),
        checkedRestoration = checked_restoration,
        checkedMismatch = checked_mismatch,
    }
end

local function finalize_failure()
    assert(pending_failure ~= nil, "failure finalization missing payload")
    local restoration, mismatch
    if pending_failure.checkedRestoration ~= nil then
        restoration = pending_failure.checkedRestoration
        mismatch = pending_failure.checkedMismatch
    else
        restoration, mismatch = restore_scope()
    end
    local cleanup_call_ok, cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_call_ok then
        cleanup_error = cleared
        cleared = false
    end
    os.remove(config.outputPath)
    local output = io.open(config.outputPath, "r")
    local output_removed = output == nil
    if output ~= nil then output:close() end
    restoration.callbacksCleared = cleared
    restoration.outputRemoved = output_removed
    local mismatch_json = "null"
    if mismatch ~= nil then
        mismatch_json = string.format(
            '{"domain":"%s","address":%d,"expected":%d,"actual":%d}',
            mismatch.domain, mismatch.address, mismatch.expected, mismatch.actual
        )
    end
    local payload = string.format(
        '{"owner":"map3-admitted-start","caseId":"%s","phase":"%s","role":"%s",'
            .. '"actualPc":%d,"expectedPc":%s,"callbackCount":%d,"callbacksCleared":%s,'
            .. '"outputRemoved":%s,"restoration":{'
            .. '"scopeArmed":%s,"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,'
            .. '"playerEntity":%s,"gold":%s,"timeState":%s,"generatedRam":%s,'
            .. '"sessionCartPatches":%s,"sessionStateRestored":%s,"callbacksCleared":%s,"outputRemoved":%s},'
            .. '"restorationMismatch":%s,"error":"%s"}',
        json_escape(active and active.caseId or "bootstrap"), json_escape(pending_failure.phase), json_escape(pending_failure.role),
        pending_failure.actualPc,
        pending_failure.expectedPc == nil and "null" or tostring(pending_failure.expectedPc), #callback_order,
        tostring(cleared), tostring(output_removed), tostring(restoration.scopeArmed), tostring(restoration.gameFlags),
        tostring(restoration.combatantAllyRecords), tostring(restoration.mapAndBattleState), tostring(restoration.playerEntity),
        tostring(restoration.gold), tostring(restoration.timeState), tostring(restoration.generatedRam),
        tostring(restoration.sessionCartPatches), tostring(restoration.sessionStateRestored), tostring(restoration.callbacksCleared),
        tostring(restoration.outputRemoved), mismatch_json, json_escape(pending_failure.message)
    )
    status(config.observerFailureContract.statusPrefix .. payload)
    client.exitCode(config.observerFailureContract.exitCode)
end

local function finalize_success()
    finish_pending = false
    local normalized, normalize_error = pcall(normalize_vint_time_outside_callback)
    if not normalized then
        fail("restoration", nil, "VInt time normalization: " .. tostring(normalize_error))
        return false
    end
    local restoration, mismatch = restore_scope()
    if mismatch ~= nil or not restoration.sessionStateRestored then
        local diagnostic = mismatch == nil
            and "scoped state restoration did not complete"
            or string.format(
                "scoped state restoration mismatch: domain=%s,address=%d,expected=%d,actual=%d",
                mismatch.domain, mismatch.address, mismatch.expected, mismatch.actual
            )
        fail("restoration", nil, diagnostic, restoration, mismatch)
        return false
    end
    local cleanup_call_ok, callbacks_cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_call_ok then
        fail(
            "callback-cleanup", nil,
            "callback cleanup exception: " .. tostring(callbacks_cleared),
            restoration, nil
        )
        return false
    end
    if not callbacks_cleared then
        fail(
            "callback-cleanup", nil,
            "callback cleanup failed: " .. tostring(cleanup_error),
            restoration, nil
        )
        return false
    end
    restoration.callbacksCleared = callbacks_cleared
    write_observation(restoration)
    status("milestone:callbacks-cleared:0")
    status("milestone:observer-finished")
    client.exitCode(0)
    return true
end

local function add_callback(address, role, handler)
    assert(callbacks[address] == nil, "more than one callback registered at physical PC " .. string.format("%X", address))
    local entry = { role = role, expectedPc = address }
    callbacks[address] = entry
    callback_order[#callback_order + 1] = address
    entry.id = event.on_bus_exec(function()
        if failed then return end
        local ok, result = pcall(handler)
        if not ok then fail(role, address, result) end
    end, address, "sf2-map3-admitted-start-" .. role, "M68K BUS")
end

local function write_menu_thunk(case)
    local address = config.harness.menuThunkAddress
    memory.write_u16_be(address, 0x0C41, "M68K BUS")
    memory.write_u16_be(address + 2, config.witchNewAction.initialMenuPage, "M68K BUS")
    memory.write_u16_be(address + 4, 0x6604, "M68K BUS")
    memory.write_u16_be(address + 6, 0x7000 | (case.injectedInitialMenuReturn & 0xFF), "M68K BUS")
    write_work_ram_rts(address + 8)
    memory.write_u16_be(address + 10, 0x7000 | (case.injectedDifficultyMenuReturn & 0xFF), "M68K BUS")
    write_work_ram_rts(address + 12)
end

local function flag_is_set(flag)
    local address = config.ram.GAME_FLAGS + math.floor(flag / 8)
    return (memory.read_u8(address, "M68K BUS") & (0x80 >> (flag % 8))) ~= 0
end

local function read_ally(id)
    local base = config.ram.COMBATANT_DATA + id * config.ram.COMBATANT_DATA_ENTRY_SIZE
    local function byte(offset) return memory.read_u8(base + offset, "M68K BUS") end
    local function word(offset) return memory.read_u16_be(base + offset, "M68K BUS") end
    return {
        id = id, class = byte(config.ram.COMBATANT_OFFSET_CLASS), level = byte(config.ram.COMBATANT_OFFSET_LEVEL),
        hpMax = word(config.ram.COMBATANT_OFFSET_HP_MAX), hpCurrent = word(config.ram.COMBATANT_OFFSET_HP_CURRENT),
        mpMax = byte(config.ram.COMBATANT_OFFSET_MP_MAX), mpCurrent = byte(config.ram.COMBATANT_OFFSET_MP_CURRENT),
        attack = byte(config.ram.COMBATANT_OFFSET_ATT_CURRENT), defense = byte(config.ram.COMBATANT_OFFSET_DEF_CURRENT),
        agility = byte(config.ram.COMBATANT_OFFSET_AGI_CURRENT), move = byte(config.ram.COMBATANT_OFFSET_MOV_CURRENT),
        items = { byte(config.ram.COMBATANT_OFFSET_ITEM_0), byte(config.ram.COMBATANT_OFFSET_ITEM_0 + 1), byte(config.ram.COMBATANT_OFFSET_ITEM_0 + 2), byte(config.ram.COMBATANT_OFFSET_ITEM_0 + 3) },
        spells = { byte(config.ram.COMBATANT_OFFSET_SPELLS), byte(config.ram.COMBATANT_OFFSET_SPELLS + 1), byte(config.ram.COMBATANT_OFFSET_SPELLS + 2), byte(config.ram.COMBATANT_OFFSET_SPELLS + 3) }
    }
end

local function append_number_array(output, values)
    output:write("[")
    for index, value in ipairs(values) do if index > 1 then output:write(",") end output:write(tostring(value)) end
    output:write("]")
end

local function append_boolean_array(output, values)
    output:write("[")
    for index, value in ipairs(values) do if index > 1 then output:write(",") end output:write(tostring(value)) end
    output:write("]")
end

write_observation = function(restoration)
    local output = assert(io.open(config.outputPath, "w"))
    local record = active
    output:write('{"system":"sf2-map3-admitted-start-runtime-v1","caseOrder":["controlled-new-map3-default"],"records":[{')
    output:write('"caseId":"' .. json_escape(record.caseId) .. '","chronology":[')
    for index, role in ipairs(record.chronology) do
        if index > 1 then output:write(",") end
        output:write('"' .. json_escape(role) .. '"')
    end
    output:write('],"handoff":' .. string.format('{"currentMap":%d,"egressMap":%d,"d0":%d,"d1":%d,"d2":%d,"d3":%d,"d4":%d}', record.handoff.currentMap, record.handoff.egressMap, record.handoff.d0, record.handoff.d1, record.handoff.d2, record.handoff.d3, record.handoff.d4))
    output:write(',"selectedSetup":' .. string.format('{"setupAddress":%d,"initAddress":%d,"setupResolutionReturnPc":%d,"initCallPc":%d,"initReturnPc":%d}', record.selectedSetup.setupAddress, record.selectedSetup.initAddress, record.selectedSetup.setupResolutionReturnPc, record.selectedSetup.initCallPc, record.selectedSetup.initReturnPc))
    local state = record.scenarioState
    output:write(',"scenarioState":{"playerEntity":' .. string.format('{"x":%d,"y":%d,"facing":%d}', state.playerEntity.x, state.playerEntity.y, state.playerEntity.facing) .. ',"gold":' .. tostring(state.gold) .. ',"difficultyFlags":')
    append_boolean_array(output, state.difficultyFlags)
    output:write(',"joinedFlags":'); append_boolean_array(output, state.joinedFlags)
    output:write(',"activeFlags":'); append_boolean_array(output, state.activeFlags)
    output:write(',"allies":[')
    for index, ally in ipairs(state.allies) do
        if index > 1 then output:write(",") end
        output:write(string.format('{"id":%d,"class":%d,"level":%d,"hpMax":%d,"hpCurrent":%d,"mpMax":%d,"mpCurrent":%d,"attack":%d,"defense":%d,"agility":%d,"move":%d,"items":', ally.id, ally.class, ally.level, ally.hpMax, ally.hpCurrent, ally.mpMax, ally.mpCurrent, ally.attack, ally.defense, ally.agility, ally.move))
        append_number_array(output, ally.items)
        output:write(',"spells":'); append_number_array(output, ally.spells); output:write("}")
    end
    output:write('],"rngSeed":' .. tostring(state.rngSeed) .. ',"vintTime":' .. string.format('{"normalization":"%s","frameCounter":%d,"secondsCounter":%d,"secondsCounterFrames":%d}', json_escape(state.vintTime.normalization), state.vintTime.frameCounter, state.vintTime.secondsCounter, state.vintTime.secondsCounterFrames) .. '},"programRequest":"none"}],')
    output:write('"callbacksCleared":' .. tostring(restoration.callbacksCleared) .. ',"restoration":' .. string.format(
        '{"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,"playerEntity":%s,"gold":%s,"timeState":%s,"generatedRam":%s,"callbacksCleared":%s,"sessionCartPatches":%s,"sessionRomDeleted":false}',
        tostring(restoration.gameFlags), tostring(restoration.combatantAllyRecords),
        tostring(restoration.mapAndBattleState), tostring(restoration.playerEntity), tostring(restoration.gold),
        tostring(restoration.timeState), tostring(restoration.generatedRam), tostring(restoration.callbacksCleared),
        tostring(restoration.sessionCartPatches)
    ) .. '}\n')
    output:close()
end

add_callback(config.functions.checkSramAddress, "bootstrap-check-sram", function()
    if phase ~= "await-check-sram" then return end
    local ally_bytes = (config.ram.COMBATANT_ALLIES_COUNTER + 1) * config.ram.COMBATANT_DATA_ENTRY_SIZE
    scope = {
        gameFlags = read_span(config.ram.GAME_FLAGS, (config.ram.LONGWORD_GAMEFLAGS_COUNTER + 1) * 4),
        combatantAllyRecords = read_span(config.ram.COMBATANT_DATA, ally_bytes),
        mapAndBattleState = read_span(config.ram.CURRENT_MAP, 9),
        playerEntity = read_span(config.ram.ENTITY_DATA, config.ram.ENTITYDEF_SIZE),
        gold = read_span(config.ram.CURRENT_GOLD, 2),
        timeState = {
            frameCounter = read_span(config.ram.FRAME_COUNTER, 1),
            randomSeed = read_span(config.ram.RANDOM_SEED, 4),
            secondsCounter = read_span(config.ram.SECONDS_COUNTER, 4),
            secondsCounterFrames = read_span(config.ram.SECONDS_COUNTER_FRAMES, 1),
        },
        generatedRam = read_span(config.harness.checkpointAddress, config.harness.generatedRamBytes),
    }
    local return_address = register("A7") & 0xFFFFFF
    memory.write_u32_be(return_address, config.harness.checkpointAddress, "M68K BUS")
    write_work_ram_jump(config.harness.checkpointAddress, config.harness.checkpointAddress)
    for _, patch in ipairs(config.sessionPatches) do write_cart_patch(patch) end
    phase = "await-safe-core-snapshot"
    pending_core_snapshot = true
    active = { caseId = config.cases[1].caseId, chronology = {"check-sram"} }
    status("milestone:scope-snapshotted-before-write")
end)

add_callback(config.harness.checkpointAddress, "checkpoint", function()
    if phase ~= "await-checkpoint" then return end
    write_menu_thunk(config.cases[1])
    write_work_ram_jump(config.functions.newActionAddress, config.harness.checkpointAddress)
    phase = "await-witch-new-action"
    status("milestone:controlled-new-admission-started")
end)

add_callback(config.functions.newActionAddress, "witch-new-action", function()
    if phase ~= "await-witch-new-action" then return end
    active.chronology[#active.chronology + 1] = "witch-new-action"
    phase = "await-new-game"
    status("milestone:original-witch-new-entered")
end)

add_callback(config.functions.newGameAddress, "new-game", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" then
        bootstrap_new_game_hits = bootstrap_new_game_hits + 1
        if bootstrap_new_game_hits == 1 then
            status("milestone:bootstrap-new-game-before-admission")
        end
        return
    end
    assert(phase == "await-new-game", "NewGame did not follow original Witch/New entry")
    active.chronology[#active.chronology + 1] = "new-game"
    phase = "await-save-game"
    status("milestone:original-new-game-entered")
end)

add_callback(config.functions.saveGameAddress, "save-game", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" then return end
    assert(phase == "await-save-game", "SaveGame did not follow original NewGame")
    active.chronology[#active.chronology + 1] = "save-game"
    phase = "await-main-loop"
    status("milestone:original-save-game-entered")
end)

add_callback(config.functions.mainLoopAddress, "main-loop", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" then return end
    assert(phase == "await-main-loop", "MainLoop reached outside controlled Witch/New handoff")
    active.chronology[#active.chronology + 1] = "main-loop"
    active.handoff = {currentMap = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS"), egressMap = memory.read_u8(config.ram.EGRESS_MAP, "M68K BUS"), d0 = register("D0") & 0xFFFF, d1 = register("D1") & 0xFFFF, d2 = register("D2") & 0xFFFF, d3 = register("D3") & 0xFFFF, d4 = register("D4") & 0xFFFF}
    assert(active.handoff.currentMap == config.map3.mapIndex and active.handoff.egressMap == config.map3.mapIndex, "Witch/New did not hand Map 3 to original MainLoop")
    phase = "await-exploration-loop"
    status("milestone:original-main-loop-entered")
end)

add_callback(config.functions.explorationLoopAddress, "exploration-loop", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" then return end
    assert(phase == "await-exploration-loop", "ExplorationLoop reached outside original MainLoop handoff")
    active.chronology[#active.chronology + 1] = "exploration-loop"
    phase = "await-map-setup-wrapper"
    status("milestone:original-exploration-loop-entered")
end)

add_callback(config.functions.runMapSetupInitFunctionAddress, "map-setup-wrapper-entry", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" then return end
    assert(phase == "await-map-setup-wrapper", "RunMapSetupInitFunction entry phase drift")
    active.chronology[#active.chronology + 1] = "map-setup-wrapper-entry"
    phase = "await-setup-resolution-return"
    status("milestone:original-map-setup-wrapper-entered")
end)

add_callback(config.functions.setupResolutionReturnAddress, "setup-resolution-return", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" or phase == "await-map-setup-wrapper" then return end
    assert(phase == "await-setup-resolution-return", "Map setup resolution return phase drift")
    active.chronology[#active.chronology + 1] = "setup-resolution-return"
    active.selectedSetup = {setupAddress = register("A0") & 0xFFFFFF, setupResolutionReturnPc = config.functions.setupResolutionReturnAddress}
    assert(active.selectedSetup.setupAddress == config.map3.defaultSetupAddress, "controlled admission selected a non-default Map 3 setup row")
    phase = "await-init-call"
    status("milestone:setup-resolution-return-observed")
end)

add_callback(config.functions.initCallAddress, "init-call", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" or phase == "await-map-setup-wrapper" or phase == "await-setup-resolution-return" then return end
    assert(phase == "await-init-call", "Map init call phase drift")
    active.chronology[#active.chronology + 1] = "init-call"
    active.selectedSetup.initAddress = register("A0") & 0xFFFFFF
    active.selectedSetup.initCallPc = config.functions.initCallAddress
    assert(active.selectedSetup.initAddress == config.map3.selectedInitAddress, "Map setup init pointer drift")
    phase = "await-selected-init"
    status("milestone:original-init-call-observed")
end)

add_callback(config.functions.selectedInitAddress, "selected-init", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" or phase == "await-map-setup-wrapper" or phase == "await-setup-resolution-return" or phase == "await-init-call" then return end
    assert(phase == "await-selected-init", "selected Map 3 init entry phase drift")
    active.chronology[#active.chronology + 1] = "selected-init"
    phase = "await-init-return"
    status("milestone:original-selected-init-entered")
end)

add_callback(config.functions.initReturnAddress, "init-return", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" or phase == "await-map-setup-wrapper" or phase == "await-setup-resolution-return" or phase == "await-init-call" or phase == "await-selected-init" then return end
    assert(phase == "await-init-return", "Map init return phase drift")
    active.chronology[#active.chronology + 1] = "init-return"
    active.selectedSetup.initReturnPc = config.functions.initReturnAddress
    phase = "await-wait-for-event"
    status("milestone:original-selected-init-return-observed")
end)

for _, address in ipairs(config.functions.unexpectedScriptAddresses) do
    add_callback(address, "unexpected-script", function()
        if phase == "await-init-return" then
            error("default admitted Map 3 init requested a guarded script/program")
        end
    end)
end

add_callback(config.functions.unexpectedInitEffectAddress, "unexpected-init-effect", function()
    if phase == "await-init-return" then
        error("default admitted Map 3 init requested MoveEntityOutOfMap")
    end
end)

add_callback(config.functions.waitForEventAddress, "wait-for-event", function()
    if phase == "await-check-sram" or phase == "await-checkpoint" or phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" or phase == "await-main-loop" or phase == "await-map-setup-wrapper" or phase == "await-setup-resolution-return" or phase == "await-init-call" or phase == "await-selected-init" or phase == "await-init-return" then return end
    assert(phase == "await-wait-for-event", "WaitForEvent reached before selected init return")
    active.chronology[#active.chronology + 1] = "wait-for-event"
    local joined, active_flags, allies = {}, {}, {}
    for id = 0, config.ram.COMBATANT_ALLIES_COUNTER do
        joined[#joined + 1] = flag_is_set(config.ram.FORCEMEMBER_JOINED_FLAGS_START + id)
        active_flags[#active_flags + 1] = flag_is_set(config.ram.FORCEMEMBER_ACTIVE_FLAGS_START + id)
        allies[#allies + 1] = read_ally(id)
    end
    active.scenarioState = {
        playerEntity = {x = memory.read_u16_be(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS"), y = memory.read_u16_be(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS"), facing = memory.read_u8(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS")},
        gold = memory.read_u16_be(config.ram.CURRENT_GOLD, "M68K BUS"),
        difficultyFlags = {flag_is_set(config.ram.FLAG_INDEX_DIFFICULTY1), flag_is_set(config.ram.FLAG_INDEX_DIFFICULTY2)},
        joinedFlags = joined, activeFlags = active_flags, allies = allies,
        rngSeed = memory.read_u32_be(config.ram.RANDOM_SEED, "M68K BUS"),
        vintTime = nil
    }
    active.rawVintTime = {
        frameCounter = memory.read_u8(config.ram.FRAME_COUNTER, "M68K BUS"),
        secondsCounter = memory.read_u32_be(config.ram.SECONDS_COUNTER, "M68K BUS"),
        secondsCounterFrames = memory.read_u8(config.ram.SECONDS_COUNTER_FRAMES, "M68K BUS")
    }
    status("milestone:raw-vint-time-read-at-wait")
    finish_pending = true
    status("milestone:first-wait-for-event-observed")
end)

while true do
    frame_count = frame_count + 1
    if frame_count % 600 == 0 then
        status(string.format(
            "milestone:heartbeat:frame=%d,phase=%s,pc=%X,bootstrapNewGameHits=%d",
            frame_count, phase, register("PC") & 0xFFFFFF, bootstrap_new_game_hits
        ))
    end
    if frame_count > config.harness.bootstrapFrameBudget + config.harness.caseFrameBudget then
        local watchdog = (phase == "await-check-sram" or phase == "await-checkpoint")
            and "bootstrap-watchdog" or "case-watchdog"
        fail(watchdog, nil, "frame budget exceeded at phase " .. phase)
        return
    end
    if pending_failure ~= nil then
        finalize_failure()
        return
    end
    if pending_core_snapshot then
        pending_core_snapshot = false
        saved_state = memorysavestate.savecorestate()
        phase = "await-checkpoint"
        status("milestone:core-state-saved-outside-callback")
    elseif finish_pending then
        if finalize_success() then return end
    end
    if pending_failure ~= nil then
        finalize_failure()
        return
    end
    if phase == "await-check-sram" then
        joypad.set({Start = true}, 1)
    elseif phase == "await-witch-new-action" or phase == "await-new-game" or phase == "await-save-game" then
        local pulse = frame_count % 42
        joypad.set((pulse >= 30 and pulse < 34) and {C = true} or {}, 1)
    else
        joypad.set({}, 1)
    end
    joypad.set({}, 2)
    emu.frameadvance()
end
