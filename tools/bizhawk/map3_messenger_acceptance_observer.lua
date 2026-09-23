local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local candidate = config.candidate and { index = 1, pending = 0, gates = {}, returns = {} } or nil
local acquisition = candidate and config.candidate.interactive
local natural = candidate and config.candidate.natural
local segment = candidate and config.candidate.segment
if candidate then candidate.diagnostic = config.candidate.diagnostic end
assert(not candidate or not candidate.diagnostic or (acquisition and natural and segment and segment.resume
    and candidate.diagnostic.kind == "heal1-consumer-diagnostic" and candidate.diagnostic.inputPolicy == "neutral-scene"),
    "HEAL diagnostic requires an explicit loaded parent and neutral scene")
assert(not candidate or not config.extension, "candidate cannot use the R2d bridge")
local extension_enabled = config.extension ~= nil
local OWNER = extension_enabled and config.extension.owner or "map3-messenger-acceptance"

-- R2a consumes the accepted controlled R1/R2 setup, then continues through
-- the original messenger body only.  It never advances into the Castle route.
local phase, frame_count, route_index = "await-check-sram", 0, 1
local active, scope, saved_state = nil, nil, nil
local callbacks, callback_order = {}, {}
local callback_active = false
local pending_core_snapshot, pending_failure, finish_pending = false, nil, false
local last_input, input_trace, chronology, map_transitions, script_trace = nil, {}, {}, {}, {}
local route_started, initial_wait_seen, route_control_ready, wait_after_warp = false, false, false, false
local append_trace
local json_write
local route_stall_key, route_stall_frames = nil, 0
local route_progress_frame = nil
local messenger_progress_frame = nil
local planned_input_index = 1
local planned_input_committed = false
local recorded_input_plan_index = 1
local recorded_sarah_action, recorded_entity142_face, recorded_entity142_action = false, false, false
local zone_admissions_seen = {}
local first_logical_duplicate, first_logical_unmodeled = nil, nil
local automation_epoch, automation_expected_input, automation_expected_mask = 0, "", 0
local observed_warp_id = nil
local controller_seen, player_controller_seen = false, false
local controller_p1_input, controller_current_input, controller_entity_address, controller_d7 = nil, nil, nil, nil
local controller_move_commit_seen, controller_move_d2, controller_move_d3, controller_move_d4, controller_move_d5 = false, nil, nil, nil, nil
local entity142_face_left_seen = false
local sarah_face_up_seen, sarah_action_seen, sarah_dispatch_seen = false, false, false
local sarah_event_seen, sarah_program_seen = false, false
local entity142_reinit_seen = false
local map3_init_seen = false
local astral_zone_intro_seen, astral_zone_event_seen, astral_zone_program_seen = false, false, false
local pending_zone_admission = nil
local messenger_zone_admission = nil
local route_start = nil
local last_callback_role, last_callback_pc = nil, nil
local messenger_started, prompt_story_seen, prompt_entry_seen = false, false, false
local prompt_flag_seen, prompt_branch_seen, prompt_accepted = false, false, false
local join_command_seen, join_sarah_seen, join_chester_seen = false, false, false
local update_force_seen, join_party_seen, zone_return_seen = false, false, false
local follower_wait_seen = false
local messenger_entry_seen, prompt_return_seen = false, false
local text_ids, speaker_operands, follow_commands = {}, {}, {}
local follower_command_seen, follower_service_seen = false, false
local messenger_result = nil
local extension_plan_index, extension_warp_index = 1, 1
local extension_control_ready, extension_move_committed = false, false
local extension_wait_after_warp, extension_bridge_pending = false, false
local extension_bridge_seeded, extension_route_complete = false, false
local extension_progress_frame = nil
local extension_chronology, extension_turn_entries = {}, {}
local extension_before_seen, extension_before_script_seen = false, false
local extension_load_seen, extension_start_seen, extension_start_script_seen = false, false, false
local extension_activate_seen, extension_region_seen, extension_spawn_seen = false, false, false
local extension_turn_order_seen, extension_player_control_seen = false, false
local extension_control_entity_seen, extension_ready_seen = false, false
local extension_player_actor, extension_bridge_result, extension_result = nil, nil, nil
local extension_action_character, extension_action_physical, extension_action_address = nil, nil, nil
-- Keep every source-plan transition and original interaction well inside the
-- host-side 300-second launch timeout. This is a harness watchdog only, not
-- an original timing assertion.
local ROUTE_MOVE_STALL_FRAME_LIMIT = 120
-- This progress watchdog is deliberately independent of host timeout.  It is
-- reset only by an accepted route transition/event, never by repeated input
-- controller callbacks, so an unproductive loop closes through typed cleanup.
local ROUTE_PHASE_WATCHDOG_FRAME_LIMIT = 1200
local MESSENGER_PHASE_WATCHDOG_FRAME_LIMIT = 7200
local EXTENSION_PHASE_WATCHDOG_FRAME_LIMIT = 1800

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function reg(name) return emu.getregister("M68K " .. name) & 0xFFFFFFFF end

local function read_span(address, length)
    local values = {}
    for offset = 0, length - 1 do
        values[#values + 1] = memory.read_u8((address + offset) & 0xFFFFFF, "M68K BUS")
    end
    return values
end

local function restore_span(address, expected)
    for offset, value in ipairs(expected) do
        memory.write_u8((address + offset - 1) & 0xFFFFFF, value, "M68K BUS")
    end
end

local function first_mismatch(domain, address, expected)
    for offset, value in ipairs(expected) do
        local current = (address + offset - 1) & 0xFFFFFF
        local actual = memory.read_u8(current, "M68K BUS")
        if actual ~= value then return { domain = domain, address = current, expected = value, actual = actual } end
    end
    return nil
end

local function json_escape(value)
    return tostring(value):gsub("[\\\"%z\1-\31]", function(character)
        local byte = string.byte(character)
        if character == "\\" then return "\\\\" end
        if character == "\"" then return "\\\"" end
        if character == "\n" then return "\\n" end
        if character == "\r" then return "\\r" end
        if character == "\t" then return "\\t" end
        return string.format("\\u%04x", byte)
    end)
end

local function append_number_array(output, values)
    output:write("[")
    for index, value in ipairs(values) do if index > 1 then output:write(",") end output:write(tostring(value)) end
    output:write("]")
end

local function write_jump(address, target)
    memory.write_u16_be(address, 0x4EF9, "M68K BUS")
    memory.write_u32_be(address + 2, target, "M68K BUS")
end

local function write_rts(address) memory.write_u16_be(address, 0x4E75, "M68K BUS") end

local function patch_cart(patch)
    for index = 1, #patch.hex, 2 do
        local value, address = tonumber(patch.hex:sub(index, index + 1), 16), patch.address + (index - 1) / 2
        memory.write_u8(address, value, "M68K BUS")
        memory.write_u8(address, value, config.r1.harness.romPatchDomain)
        assert(memory.read_u8(address, config.r1.harness.romPatchDomain) == value, "session patch readback drift")
    end
end

local function restore_cart(patch)
    for index = 1, #patch.originalHex, 2 do
        local value, address = tonumber(patch.originalHex:sub(index, index + 1), 16), patch.address + (index - 1) / 2
        memory.write_u8(address, value, "M68K BUS")
        memory.write_u8(address, value, config.r1.harness.romPatchDomain)
        local actual = memory.read_u8(address, config.r1.harness.romPatchDomain)
        if actual ~= value then return false, { domain = "sessionCartPatches", address = address, expected = value, actual = actual } end
    end
    return true, nil
end

local function cleanup_callbacks()
    local retained, first_error = {}, nil
    for _, address in ipairs(callback_order) do
        local callback = callbacks[address]
        if callback then
            local ok, result = pcall(event.unregisterbyid, callback.id)
            if ok and result ~= false then callbacks[address] = nil
            else
                retained[#retained + 1] = address
                if not first_error then first_error = tostring(result) end
            end
        end
    end
    callback_order = retained
    local cleared = next(callbacks) == nil
    if not cleared and not first_error then first_error = "residual callback registration" end
    return cleared, first_error
end

local function blank_restoration(armed)
    return { scopeArmed = armed, gameFlags = false, combatantAllyRecords = false, mapAndBattleState = false,
        playerEntity = false, forceAndParty = false, followerState = false, touchedEntities = false,
        dialogueAndInput = false, cameraState = false, bootstrapFrame = false, gold = false,
        generatedRam = false, sessionCartPatches = false, sessionStateRestored = false,
        callbacksCleared = false, outputRemoved = false }
end

local function restore_touched_entities(records)
    for _, record in ipairs(records) do restore_span(record.address, record.values) end
end

local function first_entity_mismatch(records)
    for _, record in ipairs(records) do
        local mismatch = first_mismatch("touchedEntities", record.address, record.values)
        if mismatch then return mismatch end
    end
    return nil
end

local function restore_scope()
    if segment and segment.resume then
        -- Resume has no R1 bootstrap intervention. Roll back only to its loaded entry.
        local restoration = blank_restoration(saved_state ~= nil)
        restoration.kind = "loaded-segment-entry"
        restoration.sessionStateRestored = saved_state ~= nil and pcall(memorysavestate.loadcorestate, saved_state)
        if restoration.sessionStateRestored then return restoration, nil end
        return restoration, {domain="segment-entry"}
    end
    local restoration = blank_restoration(scope ~= nil and saved_state ~= nil)
    if not restoration.scopeArmed then return restoration, {domain="scope",address=config.r1.harness.checkpointAddress,expected=1,actual=0} end
    local loaded = pcall(function()
        memorysavestate.loadcorestate(saved_state)
        restore_span(config.r1.harness.checkpointAddress, scope.generatedRam)
        restore_span(config.ram.TARGETS_LIST_LENGTH, scope.forceAndParty)
        restore_span(config.ram.FOLLOWERS_LIST, scope.followerState)
        restore_touched_entities(scope.touchedEntities)
        restore_span(config.ram.CUTSCENE_DIALOG_INDEX, scope.dialogue)
        restore_span(config.ram.PLAYER_1_INPUT, scope.input)
        restore_span(config.ram.VIEW_TARGET_ENTITY, scope.cameraState)
        restore_span(scope.bootstrapFrame.a7, scope.bootstrapFrame.stack)
        emu.setregister("M68K A7", scope.bootstrapFrame.a7)
        emu.setregister("M68K A6", scope.bootstrapFrame.a6)
    end)
    if not loaded then return restoration, {domain="scope",address=config.r1.harness.checkpointAddress,expected=1,actual=0} end
    for _, patch in ipairs(config.r1.sessionPatches) do
        local ok, mismatch = restore_cart(patch)
        if not ok then return restoration, mismatch end
    end
    restoration.sessionCartPatches = true
    local checks = {
        {"gameFlags", config.ram.GAME_FLAGS, scope.gameFlags},
        {"combatantAllyRecords", config.ram.COMBATANT_DATA, scope.combatantAllyRecords},
        {"mapAndBattleState", config.ram.CURRENT_MAP, scope.mapAndBattleState},
        {"playerEntity", config.ram.ENTITY_DATA, scope.playerEntity},
        {"forceAndParty", config.ram.TARGETS_LIST_LENGTH, scope.forceAndParty},
        {"followerState", config.ram.FOLLOWERS_LIST, scope.followerState},
        {"dialogueAndInput", config.ram.CUTSCENE_DIALOG_INDEX, scope.dialogue},
        {"dialogueAndInput", config.ram.PLAYER_1_INPUT, scope.input},
        {"cameraState", config.ram.VIEW_TARGET_ENTITY, scope.cameraState},
        {"bootstrapFrame", scope.bootstrapFrame.a7, scope.bootstrapFrame.stack},
        {"gold", config.ram.CURRENT_GOLD, scope.gold},
        {"generatedRam", config.r1.harness.checkpointAddress, scope.generatedRam},
    }
    for _, check in ipairs(checks) do
        local mismatch = first_mismatch(check[1], check[2], check[3])
        if mismatch then return restoration, mismatch end
        restoration[check[1]] = true
    end
    local entity_mismatch = first_entity_mismatch(scope.touchedEntities)
    if entity_mismatch then return restoration, entity_mismatch end
    restoration.touchedEntities = true
    restoration.sessionStateRestored = true
    return restoration, nil
end

local function fail(role, expected_pc, message, restoration, mismatch)
    if pending_failure then return end
    if natural and not candidate.failureReason then candidate.failureReason = "callback-source-order-readback-failure" end
    pending_failure = { role = role, expectedPc = expected_pc, actualPc = reg("PC") & 0xFFFFFF,
        phase = phase, message = tostring(message), restoration = restoration, mismatch = mismatch }
    if candidate and candidate.record then
        pcall(candidate.record, "failure", { role = role, expectedPc = expected_pc,
            actualPc = pending_failure.actualPc, lastCheckpoint = candidate.lastCheckpoint,
            pendingReturns = candidate.pending, error = tostring(message) })
    end
end

local function write_failure(restoration, mismatch, cleared, output_removed)
    local p = pending_failure
    if candidate then
        local file = assert(io.open(config.statusPath, "a"))
        file:write("failure:observer-callback:")
        json_write(file, { kind = "map3-candidate-callback-failure", owner = OWNER,
            caseId = "candidate-r1-through-first-map19", phase = p.phase, role = p.role,
            actualPc = p.actualPc, expectedPc = p.expectedPc,
            callbackCount = #callback_order, callbacksCleared = cleared, outputRemoved = output_removed,
            restoration = restoration, restorationMismatch = mismatch, error = p.message })
        file:write("\n"); file:close()
        return
    end
    local mismatch_json = "null"
    if mismatch then mismatch_json = string.format('{"domain":"%s","address":%d,"expected":%d,"actual":%d}', json_escape(mismatch.domain), mismatch.address, mismatch.expected, mismatch.actual) end
    local file = assert(io.open(config.statusPath, "a"))
    file:write(string.format('failure:observer-callback:{"owner":"%s","caseId":"%s","phase":"%s","role":"%s","actualPc":%d,"expectedPc":%s,"callbackCount":%d,"callbacksCleared":%s,"outputRemoved":%s,"restoration":{"scopeArmed":%s,"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,"playerEntity":%s,"forceAndParty":%s,"followerState":%s,"touchedEntities":%s,"dialogueAndInput":%s,"cameraState":%s,"bootstrapFrame":%s,"gold":%s,"generatedRam":%s,"sessionCartPatches":%s,"sessionStateRestored":%s,"callbacksCleared":%s,"outputRemoved":%s},"restorationMismatch":%s,"error":"%s"}\n',
        OWNER or "map3-messenger-acceptance", active and active.caseId or "bootstrap", json_escape(p.phase), json_escape(p.role), p.actualPc,
        p.expectedPc and tostring(p.expectedPc) or "null", #callback_order, tostring(cleared), tostring(output_removed),
        tostring(restoration.scopeArmed), tostring(restoration.gameFlags), tostring(restoration.combatantAllyRecords), tostring(restoration.mapAndBattleState),
        tostring(restoration.playerEntity), tostring(restoration.forceAndParty), tostring(restoration.followerState),
        tostring(restoration.touchedEntities), tostring(restoration.dialogueAndInput), tostring(restoration.cameraState),
        tostring(restoration.bootstrapFrame), tostring(restoration.gold), tostring(restoration.generatedRam), tostring(restoration.sessionCartPatches),
        tostring(restoration.sessionStateRestored), tostring(restoration.callbacksCleared), tostring(restoration.outputRemoved), mismatch_json, json_escape(p.message)))
    file:close()
end

local function finalize_failure()
    local restoration, mismatch = pending_failure.restoration, pending_failure.mismatch
    if not restoration then restoration, mismatch = restore_scope() end
    local cleanup_ok, cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_ok then cleared = false end
    os.remove(config.outputPath)
    local output = io.open(config.outputPath, "r")
    local output_removed = output == nil
    if output then output:close() end
    restoration.callbacksCleared, restoration.outputRemoved = cleared, output_removed
    write_failure(restoration, mismatch, cleared, output_removed)
    if acquisition and candidate.close then
        pcall(candidate.close, false, pending_failure.message)
    end
    client.exitCode(config.observerFailureContract.exitCode)
end

local function finish_failure_safely()
    if not candidate then finalize_failure(); return end
    local ok, message = pcall(finalize_failure)
    if not ok then
        pcall(status, "failure:cleanup:" .. tostring(message))
        if candidate.record then
            pcall(candidate.record, "cleanup:error", { error = tostring(message), callbackCount = #callback_order })
        end
        pcall(cleanup_callbacks)
        client.exitCode(config.observerFailureContract.exitCode)
    end
end

local function current_position()
    local tile = config.ram.MAP_TILE_SIZE
    return memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS"),
        math.floor(memory.read_u16_be(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS") / tile),
        math.floor(memory.read_u16_be(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS") / tile),
        memory.read_u8(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS"),
        memory.read_u8(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_LAYER, "M68K BUS")
end

local function note_route_progress()
    route_progress_frame = frame_count
end

local function note_messenger_progress()
    messenger_progress_frame = frame_count
end

local function enforce_route_phase_watchdog()
    if finish_pending or pending_failure then return end
    if natural and candidate.epoch then return end
    if extension_enabled and extension_progress_frame then
        if frame_count - extension_progress_frame > EXTENSION_PHASE_WATCHDOG_FRAME_LIMIT then
            local entity = config.ram.ENTITY_DATA
            local tile = config.ram.MAP_TILE_SIZE
            local map = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS")
            local x = math.floor(memory.read_u16_be(entity + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS") / tile)
            local y = math.floor(memory.read_u16_be(entity + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS") / tile)
            local facing = memory.read_u8(entity + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS")
            local action_script, action_wait = nil, nil
            if extension_action_address then
                action_script = memory.read_u32_be(
                    extension_action_address + config.ram.ENTITYDEF_OFFSET_ACTSCRIPTADDR, "M68K BUS"
                ) & 0xFFFFFF
                action_wait = memory.read_u8(
                    extension_action_address + config.ram.ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER, "M68K BUS"
                )
            end
            fail("extension-phase-watchdog", nil, string.format(
                "player-ready extension stalled: phase=%s map=%d x=%d y=%d facing=%d plan=%d/%d warp=%d/%d lastCallback=%s/%s actionCharacter=%s actionPhysical=%s actionScript=%s actionWait=%s",
                phase, map, x, y, facing, extension_plan_index, #config.extension.inputPlan,
                extension_warp_index, #config.extension.warps,
                tostring(last_callback_role), tostring(last_callback_pc),
                tostring(extension_action_character), tostring(extension_action_physical),
                tostring(action_script), tostring(action_wait)
            ))
        end
        return
    end
    if not route_started then return end
    if phase == "messenger" then
        if messenger_progress_frame and frame_count - messenger_progress_frame <= MESSENGER_PHASE_WATCHDOG_FRAME_LIMIT then return end
        fail("case-watchdog", nil, string.format(
            "messenger body progress watchdog: texts=%d follows=%d prompt=%s joins=%s/%s zoneReturn=%s lastCallback=%s/%s",
            #text_ids, #follow_commands, tostring(prompt_accepted), tostring(join_sarah_seen),
            tostring(join_chester_seen), tostring(zone_return_seen), tostring(last_callback_role), tostring(last_callback_pc)
        ))
        return
    end
    assert(route_progress_frame ~= nil, "natural route started without progress watchdog frame")
    if frame_count - route_progress_frame <= ROUTE_PHASE_WATCHDOG_FRAME_LIMIT then return end
    local waypoint = config.route.waypoints[route_index]
    local map, x, y, facing = current_position()
    fail("route-phase-watchdog", nil, string.format(
        "natural route progress watchdog: phase=%s waypoint=%s map=%d x=%d y=%d lastInput=%s "
            .. "plannedInputIndex=%d routeIndex=%d pendingWarp=%s waitAfterWarp=%s routeControlReady=%s "
            .. "automationEpoch=%d marker=%d expectedInput=%s expectedMask=%d lastCallback=%s/%s",
        phase, waypoint and waypoint.id or "complete", map, x, y, last_input or "none",
        planned_input_index, route_index, tostring(observed_warp_id), tostring(wait_after_warp),
        tostring(route_control_ready), automation_epoch,
        memory.read_u8(config.automation.markerAddress, "M68K BUS"), automation_expected_input,
        automation_expected_mask, tostring(last_callback_role), tostring(last_callback_pc)
    ))
end

-- The movement controller calls WarpIfSetAtPoint with ENTITYDEF XDEST/YDEST
-- before the VInt movement update commits that destination into X/Y.  Thus a
-- warp callback must validate both the live source coordinate and the
-- source-derived target coordinate rather than treating either one alone as
-- the warp-record coordinate.
local function current_destination()
    local tile = config.ram.MAP_TILE_SIZE
    return math.floor(memory.read_u16_be(
        config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS"
    ) / tile), math.floor(memory.read_u16_be(
        config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS"
    ) / tile)
end

local function position_diagnostic(position)
    return string.format("map=%d x=%d y=%d facing=%d layer=%d input=%d", position.map, position.x, position.y, position.facing, position.layer, position.input)
end

local function current_position_diagnostic()
    local map, x, y, facing, layer = current_position()
    return { map = map, x = x, y = y, facing = facing, layer = layer,
        input = memory.read_u8(config.ram.PLAYER_1_INPUT, "M68K BUS") }
end

local function flag_is_set(flag)
    local address = config.ram.GAME_FLAGS + math.floor(flag / 8)
    return (memory.read_u8(address, "M68K BUS") & (0x80 >> (flag % 8))) ~= 0
end

local function write_flag(flag, value)
    local address = config.ram.GAME_FLAGS + math.floor(flag / 8)
    local mask = 0x80 >> (flag % 8)
    local current = memory.read_u8(address, "M68K BUS")
    if value then current = current | mask else current = current & ((~mask) & 0xFF) end
    memory.write_u8(address, current, "M68K BUS")
    assert(flag_is_set(flag) == value, "harness flag seed readback drift for F" .. flag)
end

local function append_extension_trace(value)
    extension_chronology[#extension_chronology + 1] = value
    extension_progress_frame = frame_count
end

local function seed_extension_terminal()
    assert(extension_enabled and not extension_bridge_seeded, "R2b terminal bridge seeded more than once")
    local bridge = config.extension.bridge
    local map, x, y, facing = current_position()
    assert(
        map == bridge.map and x == bridge.player.x and y == bridge.player.y
            and (facing & config.ram.DIRECTION_MASK) == bridge.player.facing,
        string.format(
            "harness bridge landing drift: expected=(%d,%d,%d,%d) actual=(%d,%d,%d,%d)",
            bridge.map, bridge.player.x, bridge.player.y, bridge.player.facing,
            map, x, y, facing & config.ram.DIRECTION_MASK
        )
    )
    for _, flag in ipairs(bridge.setFlags) do write_flag(flag, true) end
    for _, flag in ipairs(bridge.clearFlags) do write_flag(flag, false) end

    local physical = memory.read_u8(
        config.ram.ENTITY_INDEX_LIST + bridge.guard.entityIndexSelector, "M68K BUS"
    )
    local address = config.ram.ENTITY_DATA + physical * config.ram.ENTITYDEF_SIZE
    local tile = config.ram.MAP_TILE_SIZE
    memory.write_u16_be(address + config.ram.ENTITYDEF_OFFSET_X, bridge.guard.x * tile, "M68K BUS")
    memory.write_u16_be(address + config.ram.ENTITYDEF_OFFSET_Y, bridge.guard.y * tile, "M68K BUS")
    memory.write_u16_be(address + config.ram.ENTITYDEF_OFFSET_XDEST, bridge.guard.x * tile, "M68K BUS")
    memory.write_u16_be(address + config.ram.ENTITYDEF_OFFSET_YDEST, bridge.guard.y * tile, "M68K BUS")
    local raw_facing = memory.read_u8(address + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS")
    memory.write_u8(
        address + config.ram.ENTITYDEF_OFFSET_FACING,
        (raw_facing & ((~config.ram.DIRECTION_MASK) & 0xFF)) | bridge.guard.facing,
        "M68K BUS"
    )

    extension_bridge_result = {
        map = map,
        player = { x = x, y = y, facing = facing & config.ram.DIRECTION_MASK },
        guard = { character = bridge.guard.character, physicalEntity = physical,
            x = bridge.guard.x, y = bridge.guard.y, facing = bridge.guard.facing },
        setFlags = bridge.setFlags,
        clearFlags = bridge.clearFlags,
    }

    memory.write_u32_be(config.ram.RANDOM_SEED, bridge.randomSeed, "M68K BUS")
    memory.write_u32_be(config.ram.RANDOM_SEED_COPY, bridge.randomSeedCopy, "M68K BUS")
    memory.write_u8(config.ram.FRAME_COUNTER, bridge.frameCounter, "M68K BUS")
    memory.write_u32_be(config.ram.SECONDS_COUNTER, bridge.secondsCounter, "M68K BUS")
    memory.write_u8(config.ram.SECONDS_COUNTER_FRAMES, bridge.secondsCounterFrames, "M68K BUS")

    extension_bridge_seeded, extension_control_ready, phase = true, true, "extension-route"
    append_extension_trace("harness-bridge:seed-r2b-terminal")
    status("milestone:r2b-terminal-bridge-seeded")
end

local function logical_input_edge(map, x, y, input, waypoint)
    return { map = map, x = x, y = y, input = input, waypoint = waypoint }
end

local function logical_input_edge_text(edge)
    if not edge then return "complete" end
    return string.format(
        "map=%d x=%d y=%d input=%s waypoint=%s",
        edge.map, edge.x, edge.y, edge.input, edge.waypoint
    )
end

local function same_logical_input_edge(left, right)
    return left.map == right.map and left.x == right.x and left.y == right.y
        and left.input == right.input and left.waypoint == right.waypoint
end

local function source_interaction_inputs(waypoint)
    if waypoint == "map3-entity142" then return { "Left", "C" } end
    if waypoint == "map3-sarah-classroom" then return { "C" } end
    return {}
end

-- Derive the compact expected trace from the source-backed movement plan and
-- reached interaction identities.  This is diagnostic accounting, not an
-- accepted-output corpus: polling/release scheduler frames never enter it.
local function source_logical_input_edge(index)
    local count, plan = 0, config.route.navigation.inputPlan
    for plan_index, step in ipairs(plan) do
        count = count + 1
        if count == index then
            return logical_input_edge(step.from.map, step.from.x, step.from.y, step.input, step.waypoint)
        end
        local next_step = plan[plan_index + 1]
        if not next_step or next_step.waypoint ~= step.waypoint then
            for _, input in ipairs(source_interaction_inputs(step.waypoint)) do
                count = count + 1
                if count == index then
                    return logical_input_edge(
                        step.to.map,
                        step.to.x,
                        step.to.y,
                        input,
                        step.waypoint .. (input == "C" and "" or "-face")
                    )
                end
            end
        end
    end
    return nil
end

local function trace_contains_logical_input(edge)
    for _, observed in ipairs(input_trace) do
        if same_logical_input_edge(observed, edge) then return true end
    end
    return false
end

local function append_logical_input(edge)
    local expected = source_logical_input_edge(#input_trace + 1)
    if not expected or not same_logical_input_edge(expected, edge) then
        first_logical_unmodeled = edge
        error(
            "unmodeled source-derived logical input edge: actual="
                .. logical_input_edge_text(edge)
                .. " expected=" .. logical_input_edge_text(expected)
        )
    end
    input_trace[#input_trace + 1] = edge
end

local function logical_input_closure_diagnostic()
    local missing = source_logical_input_edge(#input_trace + 1)
    local plan_missing = config.route.navigation.inputPlan[recorded_input_plan_index]
    return string.format(
        "firstMissing=%s firstDuplicate=%s firstUnmodeled=%s planIndex=%d/%d "
            .. "sarahC=%s entity142Left=%s entity142C=%s houseZoneAdmission=%s zone7IntroAdmission=%s zone7PostAdmission=%s messengerZoneAdmission=%s",
        logical_input_edge_text(missing),
        logical_input_edge_text(first_logical_duplicate),
        logical_input_edge_text(first_logical_unmodeled),
        recorded_input_plan_index, #config.route.navigation.inputPlan + 1,
        tostring(recorded_sarah_action), tostring(recorded_entity142_face), tostring(recorded_entity142_action),
        tostring(zone_admissions_seen["map3-house-exit-zone"]),
        tostring(zone_admissions_seen["map3-astral-zone-introduction"]),
        tostring(zone_admissions_seen["map3-astral-zone"]),
        tostring(zone_admissions_seen["map3-zone-messenger"])
    ) .. " nextPlan=" .. logical_input_edge_text(plan_missing and logical_input_edge(
        plan_missing.from.map, plan_missing.from.x, plan_missing.from.y,
        plan_missing.input, plan_missing.waypoint
    ) or nil)
end

local function logical_input_trace_closed()
    return source_logical_input_edge(#input_trace + 1) == nil
        and recorded_input_plan_index == #config.route.navigation.inputPlan + 1
        and not first_logical_duplicate and not first_logical_unmodeled
        and recorded_sarah_action and recorded_entity142_face and recorded_entity142_action
end

local function zone_admissions_closed()
    return zone_admissions_seen["map3-house-exit-zone"]
        and zone_admissions_seen["map3-astral-zone-introduction"]
        and zone_admissions_seen["map3-astral-zone"]
        and zone_admissions_seen["map3-zone-messenger"]
end

local function record_input(input, waypoint)
    if input == last_input then return end
    local map, x, y = current_position()
    -- Keep only original logical input edges.  Release/idle frames are driven
    -- by the harness scheduler and are not source route semantics; including
    -- their frame count would turn emulator timing into an accidental golden.
    if route_started and input ~= "" then
        local actual = logical_input_edge(map, x, y, input, waypoint)
        local repeated_interaction_poll = input == "C" and (
            (waypoint == "map3-sarah-classroom" and recorded_sarah_action)
            or (waypoint == "map3-entity142" and recorded_entity142_action)
        )
        if repeated_interaction_poll then
            last_input = input
            return
        end
        if trace_contains_logical_input(actual) then
            first_logical_duplicate = actual
            error(
                "duplicate source-derived logical input edge: actual="
                    .. logical_input_edge_text(actual)
                    .. " expected=" .. logical_input_edge_text(source_logical_input_edge(#input_trace + 1))
            )
        end
        local expected = config.route.navigation.inputPlan[recorded_input_plan_index]
        if expected and input == expected.input and waypoint == expected.waypoint
            and map == expected.from.map and x == expected.from.x and y == expected.from.y then
            append_logical_input(actual)
            recorded_input_plan_index = recorded_input_plan_index + 1
        elseif waypoint == "map3-sarah-classroom" and input == "C" and not recorded_sarah_action
            and map == 3 and x == 42 and y == 9 then
            append_logical_input(actual)
            recorded_sarah_action = true
        elseif waypoint == "map3-entity142-face" and input == "Left" and not recorded_entity142_face
            and map == 3 and x == 55 and y == 17 then
            append_logical_input(actual)
            recorded_entity142_face = true
        elseif waypoint == "map3-entity142" and input == "C" and not recorded_entity142_action
            and map == 3 and x == 55 and y == 17 and recorded_entity142_face then
            append_logical_input(actual)
            recorded_entity142_action = true
        else
            first_logical_unmodeled = actual
            error(
                "unmodeled source-derived logical input edge: actual="
                    .. logical_input_edge_text(actual)
                    .. " expected=" .. logical_input_edge_text(source_logical_input_edge(#input_trace + 1))
            )
        end
    end
    last_input = input
end

local function input_mask(input)
    if input == "" then return 0 end
    local bit = ({
        Up = config.ram.INPUT_BIT_UP,
        Down = config.ram.INPUT_BIT_DOWN,
        Left = config.ram.INPUT_BIT_LEFT,
        Right = config.ram.INPUT_BIT_RIGHT,
        C = config.ram.INPUT_BIT_C,
    })[input]
    assert(bit ~= nil, "unsupported automation input: " .. tostring(input))
    return 1 << bit
end

local function set_input(input, waypoint)
    local buttons = {}
    if input ~= "" then
        buttons[input] = true
    end
    if route_started then
        automation_epoch = (automation_epoch + 1) & 0xFF
        if automation_epoch == 0 then automation_epoch = 1 end
        automation_expected_input, automation_expected_mask = input, input_mask(input)
        memory.write_u8(config.automation.markerAddress, automation_epoch, "M68K BUS")
        assert(
            memory.read_u8(config.automation.markerAddress, "M68K BUS") == automation_epoch,
            "automation source marker write/readback drift"
        )
    end
    joypad.set(buttons, 1)
    joypad.set({}, 2)
    record_input(input, waypoint)
end

-- The retained R2 route matrix is closed at cs_5149A.  The messenger body
-- uses ordinary controller input, but those UI presses are deliberately not
-- promoted into that earlier field-navigation corpus.
local function set_messenger_input(input)
    local buttons = {}
    if input ~= "" then buttons[input] = true end
    joypad.set(buttons, 1)
    joypad.set({}, 2)
    last_input = input
end

local function waypoint_completed(waypoint)
    if waypoint.completionFlag then return flag_is_set(waypoint.completionFlag) end
    if waypoint.completionEvent == "Map3_ZoneEvent7" then return astral_zone_intro_seen end
    local map, x, y = current_position()
    if waypoint.interaction == "step" then return map == waypoint.map and x == waypoint.x and y == waypoint.y end
    if waypoint.completionDestination then
        local destination = waypoint.completionDestination
        return map == destination.map and x == destination.x and y == destination.y
    end
    return waypoint.interaction == "warp" and map ~= waypoint.map
end

local function advance_completed_waypoints()
    while route_index <= #config.route.waypoints and waypoint_completed(config.route.waypoints[route_index]) do
        local waypoint = config.route.waypoints[route_index]
        if waypoint.interaction == "warp" then
            assert(observed_warp_id == waypoint.id, "completed warp without matching original warp callback: " .. waypoint.id)
            local planned = config.route.navigation.inputPlan[planned_input_index]
            assert(
                planned and planned.waypoint == waypoint.id
                    and planned.to.map == waypoint.map and planned.to.x == waypoint.x and planned.to.y == waypoint.y,
                "completed warp has no exact final source-derived transition: " .. waypoint.id
            )
            planned_input_index = planned_input_index + 1
            observed_warp_id = nil
        end
        if waypoint.id == "map3-sarah-classroom" then
            local program = config.route.navigation.schoolSarahProgram
            assert(
                sarah_face_up_seen and sarah_action_seen and sarah_dispatch_seen
                    and sarah_event_seen and sarah_program_seen,
                "classroom Sarah completion flag set without complete original action/event/program chronology"
            )
            local address = config.ram.ENTITY_DATA + program.entityTarget.id * config.ram.ENTITYDEF_SIZE
            local x = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS") / config.ram.MAP_TILE_SIZE)
            local y = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS") / config.ram.MAP_TILE_SIZE)
            assert(
                memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS") == program.postProgramPosition.map
                    and x == program.postProgramPosition.x and y == program.postProgramPosition.y,
                string.format(
                    "classroom Sarah post-program occupancy drift: expected map=%d x=%d y=%d, actual map=%d x=%d y=%d",
                    program.postProgramPosition.map, program.postProgramPosition.x, program.postProgramPosition.y,
                    memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS"), x, y
                )
            )
            append_trace("state", "Sarah:post-cs_513D6")
        elseif waypoint.id == "map3-astral-zone-introduction" then
            assert(
                astral_zone_intro_seen and not flag_is_set(config.route.flags.afterEntity142)
                    and not flag_is_set(config.route.flags.afterAstralZone)
                    and not flag_is_set(config.route.flags.afterMessenger),
                "Astral Zone7 introduction did not retain exact pre-entity142 state"
            )
            append_trace("state", "Map3:Zone7-pre-entity142")
        elseif waypoint.id == "map3-astral-zone" then
            local program = config.route.navigation.astralZoneProgram
            assert(
                astral_zone_event_seen and astral_zone_program_seen,
                "Astral zone completion flag set without complete original zone/program chronology"
            )
            for _, expected in ipairs(program.postProgramPositions) do
                -- ``setpos`` accepts a raw character byte.  The original
                -- csc19 command delegates through GetEntityAddressFromCharacter:
                -- enemy-coded 0x80 becomes selector 0x20, and that selector
                -- resolves through ENTITY_INDEX_LIST before ENTITY_DATA.  Do
                -- not confuse the source operand with a physical entity slot.
                local resolved_entity = memory.read_u8(
                    config.ram.ENTITY_INDEX_LIST + expected.entityIndexSelector,
                    "M68K BUS"
                )
                local address = config.ram.ENTITY_DATA + resolved_entity * config.ram.ENTITYDEF_SIZE
                local x = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS") / config.ram.MAP_TILE_SIZE)
                local y = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS") / config.ram.MAP_TILE_SIZE)
                local facing = memory.read_u8(address + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS")
                assert(
                    memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS") == expected.map
                        and x == expected.x and y == expected.y
                        and (facing & config.ram.DIRECTION_MASK) == config.ram[expected.facing:upper()],
                    string.format(
                        "Astral zone post-program occupancy drift for rawCharacter=%d selector=%d resolvedEntity=%d: expected map=%d x=%d y=%d facing=%s, actual map=%d x=%d y=%d rawFacing=%d",
                        expected.rawCharacter, expected.entityIndexSelector, resolved_entity,
                        expected.map, expected.x, expected.y, expected.facing,
                        memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS"), x, y, facing
                    )
                )
            end
            append_trace("state", "Map3:post-cs_5148C")
        end
        route_index = route_index + 1
        note_route_progress()
    end
end

-- The initial Map 3 portion is not steered by the generic target heuristic.
-- Python derives this compact transition table from the original movement seam,
-- Map 3's default area, the map-offset hash, and the decoded layout. Consume
-- only transitions whose observed destination has occurred; a stale or
-- unexpected coordinate fails before another input is synthesized.
local function advance_planned_inputs(map, x, y)
    local plan = config.route.navigation.inputPlan
    while planned_input_index <= #plan do
        local transition = plan[planned_input_index]
        local destination = transition.to
        if map ~= destination.map or x ~= destination.x or y ~= destination.y then return end
        planned_input_index = planned_input_index + 1
        planned_input_committed = false
        note_route_progress()
    end
end

local function bounded_route_wait(waypoint, map, x, y, reason)
    local key = string.format("%s:%d:%d:%d:%s", waypoint.id, map, x, y, reason)
    if key == route_stall_key then route_stall_frames = route_stall_frames + 1
    else route_stall_key, route_stall_frames = key, 0 end
    if route_stall_frames > ROUTE_MOVE_STALL_FRAME_LIMIT then
        local destination_x, destination_y = current_destination()
        error(string.format(
            "route %s stalled before %s: expected map=%d x=%d y=%d, actual map=%d x=%d y=%d, start=%s, lastInput=%s",
            reason, waypoint.id, waypoint.map, waypoint.x, waypoint.y, map, x, y,
            route_start and position_diagnostic(route_start) or "unavailable", last_input or "none"
        ) .. string.format(
            ", controllerSeen=%s playerControllerSeen=%s controllerEntity=%s playerInput=%s currentPlayerInput=%s moveCommitSeen=%s d2=%s d3=%s d4=%s d5=%s flagsA=%s destination=(%d,%d) layer1=(%d,%d)-(%d,%d)",
            tostring(controller_seen), tostring(player_controller_seen), tostring(controller_entity_address),
            tostring(controller_p1_input), tostring(controller_current_input), tostring(controller_move_commit_seen),
            tostring(controller_move_d2), tostring(controller_move_d3), tostring(controller_move_d4), tostring(controller_move_d5),
            tostring(memory.read_u8(config.ram.ENTITY_DATA + config.ram.ENTITYDEF_OFFSET_FLAGS_A, "M68K BUS")),
            destination_x, destination_y,
            math.floor(memory.read_u16_be(config.ram.MAP_AREA_LAYER1_STARTX, "M68K BUS") / config.ram.MAP_TILE_SIZE),
            math.floor(memory.read_u16_be(config.ram.MAP_AREA_LAYER1_STARTY, "M68K BUS") / config.ram.MAP_TILE_SIZE),
            math.floor(memory.read_u16_be(config.ram.MAP_AREA_LAYER1_ENDX, "M68K BUS") / config.ram.MAP_TILE_SIZE),
            math.floor(memory.read_u16_be(config.ram.MAP_AREA_LAYER1_ENDY, "M68K BUS") / config.ram.MAP_TILE_SIZE)
        ) .. string.format(
            ", controllerD7=%s automationEpoch=%d marker=%d expectedInput=%s expectedMask=%d",
            tostring(controller_d7), automation_epoch,
            memory.read_u8(config.automation.markerAddress, "M68K BUS"),
            automation_expected_input, automation_expected_mask
        ))
    end
end

local function set_extension_input(input)
    local buttons = {}
    if input ~= "" then buttons[input] = true end
    joypad.set(buttons, 1)
    joypad.set({}, 2)
    last_input = input
end

local function extension_input()
    if phase == "extension-bridge-requested" then
        local bridge = config.extension.bridge
        memory.write_u16_be(config.ram.MAP_EVENT_TYPE, bridge.eventType, "M68K BUS")
        memory.write_u8(config.ram.MAP_EVENT_PARAM_1, bridge.eventParam1, "M68K BUS")
        memory.write_u8(config.ram.MAP_EVENT_PARAM_2, bridge.map, "M68K BUS")
        memory.write_u8(config.ram.MAP_EVENT_PARAM_3, bridge.player.x, "M68K BUS")
        memory.write_u8(config.ram.MAP_EVENT_PARAM_4, bridge.player.y, "M68K BUS")
        memory.write_u8(config.ram.MAP_EVENT_PARAM_5, bridge.player.facing, "M68K BUS")
        phase = "extension-bridge-injected"
        append_extension_trace("harness-bridge:event-word-injected")
        status("milestone:r2b-terminal-bridge-event-injected")
        set_extension_input("")
        return
    end
    if phase == "extension-route" then
        if not extension_control_ready then set_extension_input(""); return end
        local map, x, y = current_position()
        local transition = config.extension.inputPlan[extension_plan_index]
        if transition and map == transition.to.map and x == transition.to.x and y == transition.to.y then
            assert(extension_move_committed, "R2c route reached a destination without original movement commit")
            extension_plan_index = extension_plan_index + 1
            extension_move_committed = false
            append_extension_trace("original:field-input:" .. transition.waypoint .. ":" .. transition.input)
            transition = config.extension.inputPlan[extension_plan_index]
        end
        if not transition then
            if map == config.extension.admission.map then
                extension_route_complete, phase = true, "extension-battle"
                append_extension_trace("original:r2c-route-complete")
                status("milestone:r2c-natural-extension-complete")
            end
            set_extension_input("")
            return
        end
        if map == transition.from.map and x == transition.from.x and y == transition.from.y then
            if extension_move_committed then set_extension_input("")
            else set_extension_input(transition.input) end
            return
        end
        set_extension_input("")
        return
    end
    if phase == "extension-battle" or phase == "extension-before"
        or phase == "extension-load" or phase == "extension-start"
        or phase == "extension-turns" then
        set_extension_input((frame_count % 12 < 4) and "C" or "")
        return
    end
    set_extension_input("")
end

local function route_input()
    if extension_enabled and extension_progress_frame then extension_input(); return end
    if not route_started or finish_pending then set_input("", "idle"); return end
    if phase == "messenger" then
        -- The original dialogue and Yes/No UI consume normal controller C.
        -- The default cursor is zero; no return/result value is injected.
        set_messenger_input((frame_count % 12 < 4) and "C" or "")
        return
    end
    if phase == "follower-ready" then set_messenger_input(""); return end
    if not route_control_ready then set_input("", "await-map-settle"); return end
    local map, x, y, facing = current_position()
    advance_planned_inputs(map, x, y)
    if planned_input_committed then set_input("", "await-move-settle"); return end
    advance_completed_waypoints()
    if route_index > #config.route.waypoints then set_input("", "await-battle"); return end
    local waypoint = config.route.waypoints[route_index]
    map, x, y = current_position()
    if map ~= waypoint.map then
        bounded_route_wait(waypoint, map, x, y, "map-transition")
        set_input("", waypoint.id)
        return
    end

    local planned = config.route.navigation.inputPlan[planned_input_index]
    if planned and planned.waypoint == waypoint.id then
        local source = planned.from
        if map ~= source.map or x ~= source.x or y ~= source.y then
            error(string.format(
                "source-derived Map 3 input plan diverged before %s: expected from map=%d x=%d y=%d, actual map=%d x=%d y=%d",
                waypoint.id, source.map, source.x, source.y, map, x, y
            ))
        end
        if planned.waypoint == "map3-house-exit-zone" and x == 3 and y == 3 then
            local landing = config.route.navigation.postWarpLanding
            local actual_word = memory.read_u16_be(
                config.ram.FF0000_RAM_START + landing.layoutOffsetBytes, "M68K BUS"
            )
            assert(
                map == landing.map and actual_word == landing.layoutWord,
                string.format(
                    "post-warp Map 3 layout drift: expected map=%d word=%04X, actual map=%d word=%04X",
                    landing.map, landing.layoutWord, map, actual_word
                )
            )
        end
        bounded_route_wait(waypoint, map, x, y, "input-transition")
        set_input(planned.input, waypoint.id)
        return
    end
    if x == waypoint.x and y == waypoint.y then
        if waypoint.interaction == "entity" then
            assert(waypoint.entityTarget ~= nil, "entity waypoint omitted source-derived target")
            local facing_direction = facing & config.ram.DIRECTION_MASK
            local desired_facing = ({
                Up = config.ram.UP,
                Left = config.ram.LEFT,
            })[waypoint.facing]
            local facing_input = ({
                [config.ram.UP] = "Up",
                [config.ram.LEFT] = "Left",
            })[desired_facing]
            assert(desired_facing ~= nil and facing_input ~= nil, "unsupported source-derived entity facing")
            if facing_direction ~= desired_facing then
                -- GetActivatedEntity offsets the player coordinate from the
                -- player-facing direction.  Both reached classroom actions
                -- therefore first bind their source-defined adjacent-facing
                -- input before C can invoke the original action dispatcher.
                bounded_route_wait(waypoint, map, x, y, "entity-facing")
                set_input(facing_input, waypoint.id .. "-face")
                return
            end
            if waypoint.id == "map3-sarah-classroom" then sarah_face_up_seen = true end
            if waypoint.id == "map3-entity142" then entity142_face_left_seen = true end
            bounded_route_wait(waypoint, map, x, y, "entity-interaction")
            set_input((frame_count % 12 < 4) and "C" or "", waypoint.id)
        elseif waypoint.interaction == "zone" then
            -- Zones are admitted by the original movement/event path.  Once
            -- a source-planned move reaches one, keep the controller neutral
            -- while the pending raw-coordinate callback advances the route;
            -- a synthetic C here would turn a scheduler artifact into a
            -- claimed original logical input edge.
            bounded_route_wait(waypoint, map, x, y, "zone-admission")
            set_input("", waypoint.id)
        else
            bounded_route_wait(waypoint, map, x, y, "map-transition")
            set_input("", waypoint.id)
        end
        return
    end
    error(string.format(
        "no source-derived input transition before %s: actual map=%d x=%d y=%d, start=%s, lastInput=%s",
        waypoint.id, map, x, y, route_start and position_diagnostic(route_start) or "unavailable",
        last_input or "none"
    ))
end

append_trace = function(kind, value)
    chronology[#chronology + 1] = kind .. ":" .. value
end

local function add_callback(address, role, handler)
    -- Closed Map19-and-later continuation needs none of the R1/messenger locals.
    if segment and segment.resume and not role:match("^candidate:") then
        if segment.resume.observer.r2a or role:match("^r1%-")
            or role == "bootstrap-check-sram" or role == "checkpoint"
            or role == "map3-init-dispatch" then return end
    end
    if candidate and not (role:match("^candidate:") or role:match("^r1%-")
        or role:match("^prompt%-") or role:match("^join%-") or role:match("^update%-force%-")
        or role == "bootstrap-check-sram" or role == "checkpoint" or role == "map3-init-dispatch"
        or role == "messenger-text-command" or role == "follower-command"
        or role == "follower-service" or role == "zone-event8-return") then return end
    if candidate and callbacks[address] then
        table.insert(callbacks[address].handlers, handler)
        return
    end
    assert(callbacks[address] == nil, "more than one callback registered at physical PC " .. string.format("%X", address))
    callbacks[address] = { role = role, handlers = { handler }, id = event.on_bus_exec(function()
        if pending_failure or (acquisition and finish_pending and not natural) then return end
        if route_started or extension_progress_frame then
            last_callback_role, last_callback_pc = role, address
        end
        callback_active = true
        local ok, message = pcall(function()
            if natural and finish_pending then
                candidate.record("after-stop:callback", {role=role, address=address})
                return
            end
            for _, dispatch in ipairs(callbacks[address].handlers) do dispatch() end
        end)
        if not ok then fail(role, address, message) end
        callback_active = false
    end, address, "sf2-" .. OWNER .. "-" .. role, "M68K BUS") }
    callback_order[#callback_order + 1] = address
end

local function write_menu_thunk(case)
    local address = config.r1.harness.menuThunkAddress
    memory.write_u16_be(address, 0x0C41, "M68K BUS")
    memory.write_u16_be(address + 2, 1, "M68K BUS")
    memory.write_u16_be(address + 4, 0x6604, "M68K BUS")
    memory.write_u16_be(address + 6, 0x7000 | (case.injectedInitialMenuReturn & 0xFF), "M68K BUS")
    write_rts(address + 8)
    memory.write_u16_be(address + 10, 0x7000 | (case.injectedDifficultyMenuReturn & 0xFF), "M68K BUS")
    write_rts(address + 12)
end

local function expected_script(symbol)
    for _, value in ipairs(config.route.scriptSymbols) do if value == symbol then return true end end
    return false
end

local function snapshot_touched_entities()
    local records = {}
    -- 138/139 are enemy-coded selectors resolved through the index list to
    -- physical slots 42/43; retain both those slots and the directly named
    -- map entities rather than treating selector values as raw record IDs.
    for _, entity in ipairs({ 0, 1, 2, 42, 43, 142, 143 }) do
        local address = config.ram.ENTITY_DATA + entity * config.ram.ENTITYDEF_SIZE
        records[#records + 1] = {
            address = address,
            values = read_span(address, config.ram.ENTITYDEF_SIZE),
        }
    end
    return records
end

-- "bootstrap-check-sram"
add_callback(config.r1.functions.checkSramAddress, "bootstrap-check-sram", function()
    if phase ~= "await-check-sram" then return end
    local ally_bytes = (config.ram.COMBATANT_ALLIES_COUNTER + 1) * config.ram.COMBATANT_DATA_ENTRY_SIZE
    local party_end = config.ram.RESERVE_MEMBERS + 30
    local a7 = reg("A7") & 0xFFFFFF
    scope = {
        gameFlags = read_span(config.ram.GAME_FLAGS, (config.ram.LONGWORD_GAMEFLAGS_COUNTER + 1) * 4),
        combatantAllyRecords = read_span(config.ram.COMBATANT_DATA, ally_bytes),
        mapAndBattleState = read_span(config.ram.CURRENT_MAP, 10),
        playerEntity = read_span(config.ram.ENTITY_DATA, config.ram.ENTITYDEF_SIZE),
        forceAndParty = read_span(config.ram.TARGETS_LIST_LENGTH, party_end - config.ram.TARGETS_LIST_LENGTH),
        followerState = read_span(config.ram.FOLLOWERS_LIST, 32),
        touchedEntities = snapshot_touched_entities(),
        dialogue = read_span(config.ram.CUTSCENE_DIALOG_INDEX, 2),
        input = read_span(config.ram.PLAYER_1_INPUT, config.ram.CURRENT_PLAYER_INPUT - config.ram.PLAYER_1_INPUT + 1),
        cameraState = read_span(config.ram.VIEW_TARGET_ENTITY, 32),
        bootstrapFrame = { a7 = a7, a6 = reg("A6") & 0xFFFFFF, stack = read_span(a7, 32) },
        gold = read_span(config.ram.CURRENT_GOLD, 2),
        generatedRam = read_span(config.r1.harness.checkpointAddress, 64),
    }
    memory.write_u32_be(reg("A7") & 0xFFFFFF, config.r1.harness.checkpointAddress, "M68K BUS")
    write_jump(config.r1.harness.checkpointAddress, config.r1.harness.checkpointAddress)
    for _, patch in ipairs(config.r1.sessionPatches) do patch_cart(patch) end
    active = { caseId = config.caseOrder[1] }
    -- The checkpoint self-jump can execute again in this same emulated frame.
    -- Keep that first hit inert until the outer loop has captured the core
    -- state; only then does it explicitly arm controlled admission.
    pending_core_snapshot, phase = true, "await-safe-core-snapshot"
    status("milestone:r1-scope-snapshotted-before-write")
end)

-- "checkpoint"
add_callback(config.r1.harness.checkpointAddress, "checkpoint", function()
    if phase == "await-safe-core-snapshot" then return end
    assert(phase == "await-checkpoint", "R1 checkpoint phase drift")
    write_menu_thunk(config.cases[1])
    write_jump(config.r1.harness.checkpointAddress, config.r1.functions.newActionAddress)
    phase = "await-r1-new-action"
    status("milestone:r1-controlled-admission-started")
end)

-- "r1-witch-new-action"
add_callback(config.r1.functions.newActionAddress, "r1-witch-new-action", function()
    if phase == "await-r1-new-action" then phase = "await-r1-new-game"; append_trace("r1", "witch-new-action") end
end)

-- "r1-new-game"
add_callback(config.r1.functions.newGameAddress, "r1-new-game", function()
    if phase == "await-r1-new-game" then phase = "await-r1-save-game"; append_trace("r1", "new-game") end
end)

-- "r1-save-game"
add_callback(config.r1.functions.saveGameAddress, "r1-save-game", function()
    if phase == "await-r1-save-game" then phase = "await-r1-main-loop"; append_trace("r1", "save-game") end
end)

-- "r1-main-loop"
add_callback(config.r1.functions.mainLoopAddress, "r1-main-loop", function()
    if phase == "await-r1-main-loop" then
        phase = "await-r1-exploration"
        append_trace("r1", "main-loop")
    elseif extension_enabled and phase == "extension-bridge-injected" then
        phase = "extension-bridge-loading"
        append_extension_trace("original:MainLoop-after-harness-bridge")
    end
end)

-- "r1-exploration-loop"
add_callback(config.r1.functions.explorationLoopAddress, "r1-exploration-loop", function()
    local map = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS")
    if phase == "await-r1-exploration" then
        assert(map == config.r1.selectedMap, "R1 start did not enter Map 3")
        phase = "await-r1-setup"
        append_trace("r1", "exploration-loop")
    elseif route_started then
        local _, x, y, facing = current_position()
        map_transitions[#map_transitions + 1] = { map = map, x = x, y = y, facing = facing }
        append_trace("exploration", tostring(map))
    end
end)

-- "r1-map-setup-wrapper-entry"
add_callback(config.r1.functions.runMapSetupInitFunctionAddress, "r1-map-setup-wrapper-entry", function()
    if phase == "await-r1-exploration" then phase = "await-r1-setup" end
end)

-- "r1-setup-resolution-return"
add_callback(config.r1.functions.setupResolutionReturnAddress, "r1-setup-resolution-return", function()
    if phase == "await-r1-setup" then phase = "await-r1-init-call" end
end)

-- "r1-init-call"
add_callback(config.r1.functions.initCallAddress, "r1-init-call", function()
    if phase == "await-r1-init-call" then phase = "await-r1-init" end
end)

-- `selectedInitAddress` aliases the source/H1-derived `ms_map3_InitFunction`
-- physical PC.  It is registered exactly once: the bootstrap phase advances
-- the accepted R1 seam, and a later route-phase invocation proves the original
-- F602-gated re-init caller for `cs_513A0`.  A second callback at this PC would
-- make callback order host-dependent and must fail preflight instead.
add_callback(config.r1.functions.selectedInitAddress, "map3-init-dispatch", function()
    if phase == "await-r1-init" then
        phase = "await-r1-init-return"
    elseif route_started then
        assert(
            memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS") == 3,
            "Map3 init callback occurred outside the original Map 3 route context"
        )
        map3_init_seen = true
        append_trace("map-init", "ms_map3_InitFunction")
    end
end)

-- "r1-init-return"
add_callback(config.r1.functions.initReturnAddress, "r1-init-return", function()
    if phase == "await-r1-init-return" then phase = "await-r1-wait" end
end)

-- One original WaitForEvent PC serves both the retained R1 boundary and this
-- slice's terminal state.  The phase dispatch is deliberate and unique.
add_callback(config.r1.functions.waitForEventAddress, "follower-ready-wait", function()
    if phase == "await-r1-wait" and not initial_wait_seen then
        initial_wait_seen, route_started, route_control_ready, phase = true, true, true, "route"
        note_route_progress()
        local map, x, y, facing = current_position()
        route_start = current_position_diagnostic()
        map_transitions[#map_transitions + 1] = { map = map, x = x, y = y, facing = facing }
        append_trace("r1", "wait-for-event")
        status("milestone:r1-first-wait-for-event-observed")
        status("milestone:natural-route-input-started")
    elseif route_started and wait_after_warp then
        wait_after_warp, route_control_ready = false, true
        note_route_progress()
        append_trace("route", "post-warp-wait-for-event")
    elseif phase == "messenger" and flag_is_set(config.route.flags.afterMessenger) then
        assert(
            prompt_accepted and join_command_seen and join_sarah_seen and join_chester_seen
                and update_force_seen and join_party_seen and zone_return_seen
                and follower_command_seen and follower_service_seen,
            "follower-ready wait lacked prompt/join service callback closure"
        )
        zone_return_seen, follower_wait_seen, phase = true, true, "follower-ready"
        status("milestone:messenger-followers-ready")
        if extension_enabled then
            route_started, route_control_ready = false, false
            extension_bridge_pending, phase = true, "extension-bridge-requested"
            extension_progress_frame = frame_count
            append_extension_trace("r2a:follower-ready")
            append_extension_trace("harness-bridge:request-map21-terminal")
            status("milestone:r2b-terminal-bridge-requested")
        else
            finish_pending = true
        end
    elseif extension_enabled and phase == "extension-bridge-loading" then
        seed_extension_terminal()
    elseif extension_enabled and phase == "extension-route" and extension_wait_after_warp then
        extension_wait_after_warp, extension_control_ready = false, true
        append_extension_trace("original:post-warp-WaitForEvent")
    end
end)

-- `esc02_controlCharacter` samples either CURRENT_PLAYER_INPUT or PLAYER_1_INPUT
-- before it can produce the source-derived movement destination.  Observe that
-- original seam so a planner failure cannot be mistaken for an unlatched pad.
add_callback(config.functions.esc02_controlCharacter, "input-controller", function()
    if route_started and route_control_ready then
        controller_seen = true
        controller_entity_address = reg("A0") & 0xFFFFFF
        controller_d7 = reg("D7") & 0xFFFF
        controller_p1_input = memory.read_u8(config.ram.PLAYER_1_INPUT, "M68K BUS")
        controller_current_input = memory.read_u8(config.ram.CURRENT_PLAYER_INPUT, "M68K BUS")
        if controller_entity_address == config.ram.ENTITY_DATA then
            player_controller_seen = true
            assert(automation_epoch ~= 0, "player controller observed before automation marker")
            assert(
                memory.read_u8(config.automation.markerAddress, "M68K BUS") == automation_epoch,
                "automation source marker drift at player controller"
            )
            local actual_input = controller_d7 == 0 and controller_current_input or controller_p1_input
            assert(
                actual_input == automation_expected_mask,
                string.format(
                    "external or stale controller input: expected=%s/%d actual=%d d7=%d epoch=%d",
                    automation_expected_input, automation_expected_mask, actual_input, controller_d7,
                    automation_epoch
                )
            )
        end
    elseif extension_enabled and phase == "extension-route" and extension_control_ready then
        local transition = config.extension.inputPlan[extension_plan_index]
        if transition and (reg("A0") & 0xFFFFFF) == config.ram.ENTITY_DATA then
            local actual_input = (reg("D7") & 0xFFFF) == 0
                and memory.read_u8(config.ram.CURRENT_PLAYER_INPUT, "M68K BUS")
                or memory.read_u8(config.ram.PLAYER_1_INPUT, "M68K BUS")
            if actual_input ~= 0 then
                assert(
                    actual_input == input_mask(transition.input),
                    "R2c extension controller latched an unplanned non-neutral field input"
                )
            end
        end
    end
end)

-- This instruction-scoped seam is reached only after esc02 has accepted a
-- movement candidate through its map-word/flags gate.  It distinguishes an
-- original collision rejection from a later entity-update divergence.
add_callback(config.functions.loc_52E8, "input-controller-move-commit", function()
    if route_started and route_control_ready and (reg("A0") & 0xFFFFFF) == config.ram.ENTITY_DATA then
        controller_move_commit_seen = true
        controller_move_d2, controller_move_d3 = reg("D2") & 0xFFFF, reg("D3") & 0xFFFF
        controller_move_d4, controller_move_d5 = reg("D4") & 0xFFFF, reg("D5") & 0xFFFF
        local planned = config.route.navigation.inputPlan[planned_input_index]
        if planned and last_input == planned.input then
            planned_input_committed = true
            set_input("", "movement-commit")
        end
    elseif extension_enabled and phase == "extension-route" and extension_control_ready
        and (reg("A0") & 0xFFFFFF) == config.ram.ENTITY_DATA then
        local transition = config.extension.inputPlan[extension_plan_index]
        if transition then
            extension_move_committed = true
            set_extension_input("")
        end
    end
end)

-- The original C/A action path selects an entity only after ProcessPlayerAction
-- enters GetActivatedEntity.  Observe all three original seams so a route
-- stall identifies whether the action edge, view target, or entity dispatch
-- diverged rather than silently inferring an interaction from controller input.
add_callback(config.functions.ProcessPlayerAction, "player-action", function()
    if not route_started then return end
    local waypoint = config.route.waypoints[route_index]
    if waypoint and (waypoint.id == "map3-sarah-classroom" or waypoint.id == "map3-entity142") then
        local map, x, y = current_position()
        assert(
            last_input == "C" and map == waypoint.map and x == waypoint.x and y == waypoint.y,
            "classroom entity original player action diverged before activation"
        )
        if waypoint.id == "map3-sarah-classroom" then sarah_action_seen = true end
        append_trace("action", "ProcessPlayerAction:" .. waypoint.id)
    end
end)

add_callback(config.functions.GetActivatedEntity, "activated-entity", function()
    if not route_started then return end
    local waypoint = config.route.waypoints[route_index]
    if waypoint and (waypoint.id == "map3-sarah-classroom" or waypoint.id == "map3-entity142") then
        local map, x, y, facing = current_position()
        local view_target = memory.read_u8(config.ram.VIEW_TARGET_ENTITY, "M68K BUS")
        local expected_facing = waypoint.facing == "Up" and config.ram.UP or config.ram.LEFT
        assert(
            map == waypoint.map and x == waypoint.x and y == waypoint.y
                and (facing & config.ram.DIRECTION_MASK) == expected_facing and view_target == 0,
            "classroom entity activation seam target/facing drift: viewTarget=" .. view_target
                .. " rawFacing=" .. facing .. " maskedFacing=" .. (facing & config.ram.DIRECTION_MASK)
        )
        append_trace("action", "GetActivatedEntity:" .. waypoint.id)
    end
end)

add_callback(config.functions.RunMapSetupEntityEvent, "entity-dispatch", function()
    if not route_started then return end
    local waypoint = config.route.waypoints[route_index]
    if waypoint and (waypoint.id == "map3-sarah-classroom" or waypoint.id == "map3-entity142") then
        assert(
            (reg("D0") & 0xFF) == waypoint.entityTarget.id,
            "classroom entity dispatch index drift: actual=" .. (reg("D0") & 0xFF)
        )
        if waypoint.id == "map3-sarah-classroom" then sarah_dispatch_seen = true end
        append_trace("action", "RunMapSetupEntityEvent:" .. waypoint.id)
    end
end)

-- "entity-event"
for _, symbol in ipairs({"Map3_EntityEvent0", "Map3_EntityEvent15"}) do
    add_callback(config.functions[symbol], "entity-event", function()
        if not route_started then return end
        if symbol == "Map3_EntityEvent0" then
            local waypoint = config.route.waypoints[route_index]
            local map, x, y, facing = current_position()
            assert(
                waypoint and waypoint.id == "map3-sarah-classroom"
                    and map == waypoint.map and x == waypoint.x and y == waypoint.y
                    and (facing & config.ram.DIRECTION_MASK) == config.ram.UP and sarah_face_up_seen
                    and sarah_action_seen and sarah_dispatch_seen,
                "classroom Sarah event did not follow source-derived adjacent Up-facing interaction"
            )
            sarah_event_seen = true
        elseif symbol == "Map3_EntityEvent15" then
            local waypoint = config.route.waypoints[route_index]
            local map, x, y, facing = current_position()
            assert(
                waypoint and waypoint.id == "map3-entity142"
                    and map == waypoint.map and x == waypoint.x and y == waypoint.y
                    and (facing & config.ram.DIRECTION_MASK) == config.ram.LEFT and entity142_face_left_seen,
                "entity142 event did not follow source-derived adjacent Left-facing lower-school interaction: rawFacing="
                    .. facing .. " maskedFacing=" .. (facing & config.ram.DIRECTION_MASK)
            )
        end
        append_trace("entity", symbol)
    end)
end

-- "zone-event"
-- ProcessMapEventType6 receives the source target in MAP_EVENT_PARAM_1/3;
-- the live player entity can still hold the preceding tile at this seam. Bind
-- the target here and consume it at the dispatched Map3 zone handler instead
-- of conflating source target with later movement completion.
add_callback(config.functions.ProcessMapEventType6_ZoneEvent, "zone-admission", function()
    if not route_started then return end
    local waypoint = config.route.waypoints[route_index]
    local map, x, y = current_position()
    local event_x = memory.read_u16_be(config.ram.MAP_EVENT_PARAM_1, "M68K BUS")
    local event_y = memory.read_u16_be(config.ram.MAP_EVENT_PARAM_3, "M68K BUS")
    if not (
        waypoint and waypoint.interaction == "zone" and map == waypoint.map
            and event_x == waypoint.x and event_y == waypoint.y
    ) then
        error(string.format(
            "natural zone admission raw-coordinate drift: waypoint=%s live=(%d,%d,%d) raw=(%d,%d)",
            waypoint and waypoint.id or "none", map, x, y, event_x, event_y
        ))
    end
    pending_zone_admission = { id = waypoint.id, x = event_x, y = event_y }
    append_trace("zone-admission", waypoint.id)
end)

for _, symbol in ipairs({"Map3_ZoneEvent0", "Map3_ZoneEvent6", "Map3_ZoneEvent7", "Map3_ZoneEvent8"}) do
    add_callback(config.functions[symbol], "zone-event", function()
        if not route_started then return end
        local waypoint = config.route.waypoints[route_index]
        if symbol == "Map3_ZoneEvent0" then
            error("unexpected Map3 Zone0/guard route event before Astral interaction")
        elseif symbol == "Map3_ZoneEvent7" then
            local waypoint = config.route.waypoints[route_index]
            if not (
                waypoint and pending_zone_admission and pending_zone_admission.id == waypoint.id
                    and pending_zone_admission.x == waypoint.x and pending_zone_admission.y == waypoint.y
                    and not flag_is_set(config.route.flags.afterMessenger)
            ) then
                error(string.format(
                    "Astral Zone7 event admission/state drift: waypoint=%s pending=%s raw=(%s,%s) F602=%s F260=%s F603=%s",
                    waypoint and waypoint.id or "none", pending_zone_admission and pending_zone_admission.id or "none",
                    pending_zone_admission and pending_zone_admission.x or "none",
                    pending_zone_admission and pending_zone_admission.y or "none",
                    tostring(flag_is_set(config.route.flags.afterEntity142)),
                    tostring(flag_is_set(config.route.flags.afterAstralZone)),
                    tostring(flag_is_set(config.route.flags.afterMessenger))
                ))
            end
            if waypoint.id == "map3-astral-zone-introduction" then
                assert(
                    not flag_is_set(config.route.flags.afterEntity142)
                        and not flag_is_set(config.route.flags.afterAstralZone),
                    "Astral Zone7 introduction did not preserve F602/F260-clear branch"
                )
                astral_zone_intro_seen = true
            elseif waypoint.id == "map3-astral-zone" then
                assert(
                    astral_zone_intro_seen and flag_is_set(config.route.flags.afterEntity142)
                        and not flag_is_set(config.route.flags.afterAstralZone),
                    "Astral Zone7 mutation path did not follow exact introduction/entity142 chronology"
                )
                astral_zone_event_seen = true
            else
                error("Astral Zone7 event occurred at unexpected route waypoint: " .. waypoint.id)
            end
        elseif symbol == "Map3_ZoneEvent6" or symbol == "Map3_ZoneEvent8" then
            local waypoint = config.route.waypoints[route_index]
            assert(
                pending_zone_admission and pending_zone_admission.id == waypoint.id,
                "Map3 zone handler executed without matching original zone-admission seam"
            )
            if symbol == "Map3_ZoneEvent8" then
                local map = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS")
                messenger_zone_admission = {
                    sourceTarget = { map = map, x = pending_zone_admission.x, y = pending_zone_admission.y },
                }
            end
        end
        zone_admissions_seen[waypoint.id] = true
        pending_zone_admission = nil
        append_trace("zone", symbol)
    end)
end

-- "step-door" -- source-derived Map 3 step rows; D0/D1 are the original
-- pixel-coordinate arguments at the OpenDoor entry and are scaled by 384.
add_callback(config.functions.OpenDoor, "step-door", function()
    if not route_started then return end
    local x = math.floor((reg("D0") & 0xFFFF) / config.ram.MAP_TILE_SIZE)
    local y = math.floor((reg("D1") & 0xFFFF) / config.ram.MAP_TILE_SIZE)
    for _, waypoint in ipairs(config.route.waypoints) do
        if waypoint.interaction == "step" and x == waypoint.x and y == waypoint.y then
            append_trace("step", waypoint.id)
            return
        end
    end
    error(string.format("unexpected natural-route step door at map=%d x=%d y=%d", memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS"), x, y))
end)

-- One ExecuteMapScript PC dispatches the retained Map 3 prefix and the R2a
-- boundary; never register a callback on cs_5149A because it is script data.
add_callback(config.functions.ExecuteMapScript, "messenger-script-entry", function()
    if extension_enabled and extension_progress_frame and not route_started then
        local target = reg("A0") & 0xFFFFFF
        if phase == "extension-before" and target == config.extension.functions.bbcs_01 then
            extension_before_script_seen = true
            append_extension_trace("original:ExecuteMapScript:bbcs_01")
        elseif phase == "extension-start" and target == config.extension.functions.ms_Empty then
            extension_start_script_seen = true
            append_extension_trace("original:ExecuteMapScript:ms_Empty")
        end
        return
    end
    if not route_started then return end
    local target = reg("A0") & 0xFFFFFF
    for _, symbol in ipairs(config.route.scriptSymbols) do
        if target == config.functions[symbol] then
            script_trace[#script_trace + 1] = symbol
            append_trace("script", symbol)
            if symbol == "cs_513D6" then
                local waypoint = config.route.waypoints[route_index]
                assert(
                    waypoint and waypoint.id == "map3-sarah-classroom" and sarah_event_seen,
                    "classroom Sarah movement program entered outside original entity-event chronology"
                )
                sarah_program_seen = true
            elseif symbol == "cs_513A0" then
                assert(
                    map3_init_seen and memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS") == 3,
                    "Map3 entity142 re-init program lacked original Map3-init caller chronology"
                )
                entity142_reinit_seen = true
                append_trace("state", "Map3:entity142-reinit-cs_513A0")
            elseif symbol == "cs_5148C" then
                local waypoint = config.route.waypoints[route_index]
                if not (
                    waypoint and waypoint.id == "map3-astral-zone" and astral_zone_event_seen
                        and flag_is_set(config.route.flags.afterEntity142)
                        and not flag_is_set(config.route.flags.afterAstralZone)
                        and not flag_is_set(config.route.flags.afterMessenger)
                ) then
                    error(string.format(
                        "Astral zone occupancy program entered outside original post-entity142 chronology: waypoint=%s Zone7=%s F602=%s F260=%s F603=%s",
                        waypoint and waypoint.id or "none", tostring(astral_zone_event_seen),
                        tostring(flag_is_set(config.route.flags.afterEntity142)),
                        tostring(flag_is_set(config.route.flags.afterAstralZone)),
                        tostring(flag_is_set(config.route.flags.afterMessenger))
                    ))
                end
                astral_zone_program_seen = true
            elseif symbol == config.route.endpoint.program then
                local endpoint = config.route.endpoint
                local map, x, y = current_position()
                assert(
                    messenger_zone_admission
                        and messenger_zone_admission.sourceTarget.map == endpoint.sourceTarget.map
                        and messenger_zone_admission.sourceTarget.x == endpoint.sourceTarget.x
                        and messenger_zone_admission.sourceTarget.y == endpoint.sourceTarget.y,
                    "Map 3 messenger program lacked the original ZoneEvent8 raw-target admission"
                )
                assert(
                    map == endpoint.sourceTarget.map,
                    "Map 3 messenger program entered outside the original Map 3 ZoneEvent8 admission"
                )
                assert(
                    not flag_is_set(endpoint.notYetMutatedFlag),
                    "Map 3 messenger completion flag mutated before cs_5149A body"
                )
                assert(
                    flag_is_set(config.route.flags.afterHouseExit)
                        and flag_is_set(config.route.flags.classroomSarah)
                        and flag_is_set(config.route.flags.afterEntity142)
                        and flag_is_set(config.route.flags.afterAstralZone),
                    "Map 3 messenger program entered before opening state flags closed"
                )
                active.openingMap3 = {
                    sourceTarget = endpoint.sourceTarget,
                    program = endpoint.program,
                    afterHouseExit = flag_is_set(config.route.flags.afterHouseExit),
                    classroomSarah = flag_is_set(config.route.flags.classroomSarah),
                    afterEntity142 = flag_is_set(config.route.flags.afterEntity142),
                    afterAstralZone = flag_is_set(config.route.flags.afterAstralZone),
                    afterMessenger = flag_is_set(endpoint.notYetMutatedFlag),
                }
                assert(
                    logical_input_trace_closed() and zone_admissions_closed(),
                    "opening Map 3 logical-input trace did not close at the complete source-derived route matrix: "
                        .. logical_input_closure_diagnostic()
                )
                append_trace("endpoint", "cs_5149A-entry-before-body")
                messenger_started, messenger_entry_seen, phase = true, true, "messenger"
                note_messenger_progress()
                status("milestone:messenger-body-started")
            end
            return
        end
    end
    error("unexpected map script target on natural route: " .. string.format("%X", target))
end)

-- The body callbacks are all original code.  They are phase-scoped so the
-- retained R1/R2 prefix cannot satisfy an R2a assertion by coincidence.
local function observe_messenger_text()
    if extension_enabled and phase == "extension-before" then
        extension_progress_frame = frame_count
        return
    end
    if phase ~= "messenger" then return end
    assert(messenger_entry_seen, "text command occurred before cs_5149A body entry")
    local text_id = memory.read_u16_be(config.ram.CUTSCENE_DIALOG_INDEX, "M68K BUS")
    local raw_speaker = memory.read_u16_be(reg("A6") & 0xFFFFFF, "M68K BUS")
    text_ids[#text_ids + 1] = text_id
    speaker_operands[#speaker_operands + 1] = raw_speaker == 0xFFFF and false or raw_speaker
    note_messenger_progress()
end

add_callback(config.functions.csc00_displaySingleTextbox, "messenger-text-command", observe_messenger_text)
add_callback(config.functions.csc02_displayTextbox, "messenger-text-command", observe_messenger_text)

add_callback(config.functions.csc11_promptYesNoForStoryFlow, "prompt-story-flow", function()
    if phase ~= "messenger" then return end
    assert(messenger_entry_seen, "story-flow prompt occurred before messenger body")
    prompt_story_seen = true
    note_messenger_progress()
end)

add_callback(config.functions.YesNoPrompt, "prompt-yes-no", function()
    if phase ~= "messenger" then return end
    assert(prompt_story_seen, "YesNoPrompt occurred outside csc11 story-flow callback")
    prompt_entry_seen = true
    note_messenger_progress()
end)

-- This is the original csc11 continuation immediately after YesNoPrompt.
-- It observes D0 before csc11 branches to SetFlag/ClearFlag; no result is
-- written or injected by the harness.
add_callback(config.functions.csc11_promptYesNoForStoryFlow + 10, "prompt-return", function()
    if phase ~= "messenger" then return end
    assert(prompt_entry_seen and (reg("D0") & 0xFFFF) == 0, "YesNoPrompt did not return original default zero")
    prompt_return_seen = true
    note_messenger_progress()
end)

add_callback(config.functions.SetFlag, "prompt-set-flag", function()
    if phase ~= "messenger" then return end
    if (reg("D1") & 0xFFFF) == 89 then
        assert(prompt_return_seen, "prompt flag set before original YesNoPrompt zero return")
        prompt_flag_seen = true
        note_messenger_progress()
    end
end)

add_callback(config.functions.csc0C_jumpIfFlagSet, "prompt-branch", function()
    if phase ~= "messenger" then return end
    local operand = memory.read_u16_be(reg("A6") & 0xFFFFFF, "M68K BUS")
    if operand == 89 then
        assert(prompt_flag_seen and flag_is_set(89), "csc0C did not receive the accepted prompt flag")
        prompt_branch_seen, prompt_accepted = true, true
        note_messenger_progress()
        status("milestone:messenger-prompt-accepted")
    end
end)

add_callback(config.functions.csc08_joinForce, "join-force-command", function()
    if phase ~= "messenger" then return end
    local selector = memory.read_u16_be(reg("A6") & 0xFFFFFF, "M68K BUS")
    assert(prompt_branch_seen and selector == 128, "csc08 selector/prompt branch drift")
    join_command_seen = true
    text_ids[#text_ids + 1] = 447
    speaker_operands[#speaker_operands + 1] = false
    note_messenger_progress()
end)

add_callback(config.functions.JoinForce, "join-force-service", function()
    if phase ~= "messenger" or not join_command_seen then return end
    local ally = reg("D0") & 0xFF
    if ally == 1 then join_sarah_seen = true
    elseif ally == 2 then assert(join_sarah_seen, "Chester joined before Sarah"); join_chester_seen = true
    else error("selector-128 JoinForce unexpected ally " .. ally) end
    note_messenger_progress()
end)

add_callback(config.functions.UpdateForce, "update-force-service", function()
    if phase == "messenger" and (join_sarah_seen or join_chester_seen) then update_force_seen = true end
    if phase == "messenger" then note_messenger_progress() end
end)

-- These original return PCs prove that the nested service calls completed;
-- entry callbacks alone would not establish a completed update/join.
add_callback(config.functions.JoinForce + 16, "update-force-return", function()
    if phase == "messenger" and join_sarah_seen then update_force_seen = true end
    if phase == "messenger" then note_messenger_progress() end
end)

add_callback(config.functions.JoinBattleParty, "join-battle-party-service", function()
    if phase == "messenger" and (join_sarah_seen or join_chester_seen) then join_party_seen = true end
    if phase == "messenger" then note_messenger_progress() end
end)

add_callback(config.functions.JoinForce + 28, "join-battle-party-return", function()
    if phase == "messenger" and join_chester_seen then join_party_seen = true end
    if phase == "messenger" then note_messenger_progress() end
end)

add_callback(config.functions.csc2C_followEntity, "follower-command", function()
    if phase ~= "messenger" then return end
    local address = reg("A6") & 0xFFFFFF
    local follower = memory.read_u16_be(address, "M68K BUS")
    local leader = memory.read_u16_be(address + 2, "M68K BUS")
    local distance = memory.read_u16_be(address + 4, "M68K BUS")
    local expected = (#follow_commands == 0) and { follower = 1, leader = 0 } or { follower = 2, leader = 1 }
    assert(
        follower == expected.follower and leader == expected.leader and distance == 2,
        "original csc2C follower operand chain drift"
    )
    follow_commands[#follow_commands + 1] = { follower = follower, leader = leader, distance = distance }
    follower_command_seen = #follow_commands == 2
    note_messenger_progress()
end)

add_callback(config.functions.AddFollower, "follower-service", function()
    if phase == "messenger" and #follow_commands > 0 then follower_service_seen = true end
    if phase == "messenger" then note_messenger_progress() end
end)

-- Map3_ZoneEvent8 is source/H1/ROM-bound to this 24-byte extent.  The last
-- word is its original RTS after the F603 set-flag command.
add_callback(config.functions.Map3_ZoneEvent8 + 22, "zone-event8-return", function()
    if phase ~= "messenger" then return end
    assert(
        prompt_accepted and join_party_seen and flag_is_set(600) and flag_is_set(66)
            and flag_is_set(603) and #follow_commands == 2,
        "Map3 ZoneEvent8 returned before completed messenger acceptance state"
    )
    zone_return_seen = true
    note_messenger_progress()
end)

-- "warp"
if extension_enabled then
    add_callback(config.functions.ProcessMapEvent, "extension-map-event-dispatch", function()
        if not extension_bridge_pending then return end
        assert(
            phase == "extension-bridge-injected" and (reg("D0") & 0xFFFF) == config.extension.bridge.eventType,
            "explicit harness bridge did not reach original ProcessMapEvent as event type 1"
        )
        append_extension_trace("original:ProcessMapEvent:harness-bridge")
        status("milestone:r2b-terminal-bridge-event-dispatched")
    end)
end

add_callback(config.functions.ProcessMapEventType1_Warp, "warp", function()
    if extension_enabled and extension_bridge_pending then
        local bridge = config.extension.bridge
        status("milestone:r2b-terminal-bridge-warp-handler-entered")
        assert(
            memory.read_u8(config.ram.MAP_EVENT_PARAM_1, "M68K BUS") == bridge.eventParam1
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_2, "M68K BUS") == bridge.map
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_3, "M68K BUS") == bridge.player.x
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_4, "M68K BUS") == bridge.player.y
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_5, "M68K BUS") == bridge.player.facing,
            "explicit R2a-to-R2b harness bridge parameters drift"
        )
        extension_bridge_pending, phase = false, "extension-bridge-loading"
        append_extension_trace("original:ProcessMapEventType1_Warp:harness-bridge")
    elseif extension_enabled and phase == "extension-route" then
        local expected = config.extension.warps[extension_warp_index]
        local map = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS")
        local destination_x, destination_y = current_destination()
        local transition = config.extension.inputPlan[extension_plan_index]
        assert(
            expected and map == expected.from.map
                and destination_x == expected.from.x and destination_y == expected.from.y
                and transition and transition.to.map == expected.from.map
                and transition.to.x == expected.from.x and transition.to.y == expected.from.y
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_2, "M68K BUS") == expected.to.map
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_3, "M68K BUS") == expected.to.x
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_4, "M68K BUS") == expected.to.y
                and memory.read_u8(config.ram.MAP_EVENT_PARAM_5, "M68K BUS") == expected.to.facing,
            "R2c original warp parameters or source destination drift"
        )
        append_extension_trace("original:field-input:" .. transition.waypoint .. ":" .. transition.input)
        extension_plan_index = extension_plan_index + 1
        extension_warp_index = extension_warp_index + 1
        extension_control_ready, extension_wait_after_warp = false, true
        extension_move_committed = false
        append_extension_trace("original:warp:" .. expected.id)
    elseif route_started then
        local waypoint = config.route.waypoints[route_index]
        local map, x, y = current_position()
        local destination = waypoint and waypoint.completionDestination
        local planned = config.route.navigation.inputPlan[planned_input_index]
        local event_map = memory.read_u8(config.ram.MAP_EVENT_PARAM_2, "M68K BUS")
        local event_x = memory.read_u8(config.ram.MAP_EVENT_PARAM_3, "M68K BUS")
        local event_y = memory.read_u8(config.ram.MAP_EVENT_PARAM_4, "M68K BUS")
        local source_warp = nil
        for _, candidate in ipairs(config.route.warps) do
            if waypoint and candidate.fromMap == waypoint.map and candidate.y == waypoint.y
                and (candidate.x == waypoint.x or candidate.x == 255) then
                assert(source_warp == nil, "ambiguous source warp for natural-route waypoint: " .. waypoint.id)
                source_warp = candidate
            end
        end
        assert(
            waypoint and waypoint.interaction == "warp"
                and destination
                and planned and planned.waypoint == waypoint.id
                -- The original warp handler runs while the player entity still
                -- holds the source-side position; the event candidate is the
                -- planned destination.  The later WaitForEvent check verifies
                -- the resolved post-warp position instead.
                and map == planned.from.map and x == planned.from.x and y == planned.from.y
                and planned.to.map == waypoint.map and planned.to.x == waypoint.x and planned.to.y == waypoint.y
                and source_warp
                and event_map == source_warp.eventDestinationMap
                and event_x == source_warp.destinationX and event_y == source_warp.destinationY,
            "unexpected original warp callback: current map=" .. map .. " x=" .. x .. " y=" .. y
                .. " event destination map=" .. event_map .. " x=" .. event_x .. " y=" .. event_y
        )
        observed_warp_id = waypoint.id
        -- A commit observed before the warp belongs to the previous map's
        -- final input.  Clear it so a post-warp collision diagnostic cannot
        -- misattribute that prior accepted movement to the next waypoint.
        planned_input_committed = false
        controller_move_commit_seen = false
        controller_move_d2, controller_move_d3 = nil, nil
        controller_move_d4, controller_move_d5 = nil, nil
        route_control_ready, wait_after_warp = false, true
        note_route_progress()
        append_trace("map-event", "warp:" .. waypoint.id)
    end
end)

if extension_enabled then
    add_callback(config.extension.functions.CheckBattle, "extension-check-battle", function()
        -- MainLoop carries the post-warp map in D0.  CURRENT_MAP still holds
        -- the source map at this entry and is committed later by battle load.
        local map = reg("D0") & 0xFF
        if not extension_progress_frame or map ~= config.extension.admission.map then return end
        assert(
            (phase == "extension-route" or phase == "extension-battle")
                and extension_bridge_seeded and map == config.extension.admission.map
                and flag_is_set(config.extension.admission.unlockedFlag)
                and not flag_is_set(config.extension.admission.completedFlag),
            "Battle01 CheckBattle did not follow the bridged R2c route state"
        )
        if not extension_route_complete then
            extension_route_complete, phase = true, "extension-battle"
            append_extension_trace("original:r2c-route-complete")
            status("milestone:r2c-natural-extension-complete")
        end
        append_extension_trace("original:CheckBattle")
    end)

    add_callback(config.extension.functions.BattleLoop, "extension-battle-loop", function()
        assert(extension_route_complete, "BattleLoop entered before R2c route closure")
        assert((reg("D0") & 0xFF) == config.extension.admission.map, "BattleLoop map argument drift")
        assert((reg("D1") & 0xFF) == config.extension.admission.battle, "BattleLoop battle argument drift")
        phase = "extension-before"
        append_extension_trace("original:BattleLoop")
        status("milestone:battle01-loop-entered")
    end)

    add_callback(config.extension.functions.ExecuteBeforeBattleCutscene, "extension-before-battle", function()
        assert(phase == "extension-before", "before-battle cutscene entered outside new-battle branch")
        extension_before_seen = true
        append_extension_trace("original:ExecuteBeforeBattleCutscene")
    end)

    add_callback(config.extension.functions.csc15_setEntityActscript, "extension-before-set-actscript", function()
        if phase == "extension-before" then extension_progress_frame = frame_count end
    end)

    add_callback(config.extension.functions.csc2A_entityShiver, "extension-before-entity-shiver", function()
        if phase == "extension-before" then extension_progress_frame = frame_count end
    end)

    add_callback(config.extension.functions.csc2D_entityActionSequence, "extension-before-entity-actions", function()
        if phase ~= "extension-before" then return end
        extension_progress_frame = frame_count
        local character = memory.read_u8(reg("A6") & 0xFFFFFF, "M68K BUS")
        local selector = character
        if selector >= 0x80 then selector = selector - 0x60 end
        local physical = memory.read_u8(config.ram.ENTITY_INDEX_LIST + selector, "M68K BUS")
        extension_action_character, extension_action_physical = character, physical
        extension_action_address = config.ram.ENTITY_DATA + physical * config.ram.ENTITYDEF_SIZE
    end)

    add_callback(config.extension.functions.LoadBattle, "extension-load-battle", function()
        assert(
            extension_before_seen and extension_before_script_seen,
            "LoadBattle entered before the original Battle01 before-cutscene returned"
        )
        extension_load_seen, phase = true, "extension-load"
        append_extension_trace("original:LoadBattle")
    end)

    add_callback(config.extension.functions.ExecuteBattleStartCutscene, "extension-battle-start", function()
        assert(extension_load_seen, "battle-start cutscene entered before LoadBattle returned")
        extension_start_seen, phase = true, "extension-start"
        append_extension_trace("original:ExecuteBattleStartCutscene")
    end)

    add_callback(config.extension.functions.ActivateEnemies, "extension-activate-enemies", function()
        assert(
            extension_start_seen and extension_start_script_seen,
            "ActivateEnemies entered before the original empty battle-start script returned"
        )
        extension_activate_seen, phase = true, "extension-turns"
        append_extension_trace("original:ActivateEnemies")
    end)

    add_callback(config.extension.functions.ExecuteBattleRegionCutscene, "extension-region-cutscene", function()
        assert(extension_activate_seen, "battle-region cutscene preceded enemy activation")
        extension_region_seen = true
        append_extension_trace("original:ExecuteBattleRegionCutscene")
    end)

    add_callback(config.extension.functions.PopulateTargetsListWithSpawningEnemies, "extension-spawn-list", function()
        assert(extension_region_seen, "spawning list preceded battle-region cutscene return")
        extension_spawn_seen = true
        append_extension_trace("original:PopulateTargetsListWithSpawningEnemies")
    end)

    add_callback(config.extension.functions.GenerateBattleTurnOrder, "extension-turn-order", function()
        assert(extension_spawn_seen, "turn order preceded spawning-list return")
        extension_turn_order_seen = true
        append_extension_trace("original:GenerateBattleTurnOrder")
        status("milestone:battle01-turn-order-entered")
    end)

    add_callback(config.extension.functions.ExecuteIndividualTurn, "extension-individual-turn", function()
        assert(extension_turn_order_seen, "individual turn preceded original turn-order generation")
        local actor = reg("D0") & 0xFF
        extension_turn_entries[#extension_turn_entries + 1] = actor
        append_extension_trace("original:ExecuteIndividualTurn:" .. actor)
    end)

    add_callback(config.extension.functions.ProcessBattleEntityControlPlayerInput, "extension-player-control", function()
        assert(extension_turn_order_seen, "player control preceded original turn-order generation")
        local offset = memory.read_u8(config.ram.CURRENT_BATTLE_TURN, "M68K BUS")
        extension_player_actor = memory.read_u8(config.ram.BATTLE_TURN_ORDER + offset, "M68K BUS")
        assert(extension_player_actor < config.ram.COMBATANT_ENEMIES_START, "first player controller actor was not an ally")
        extension_player_control_seen, phase = true, "extension-await-player-ready"
        append_extension_trace("original:ProcessBattleEntityControlPlayerInput:" .. extension_player_actor)
        status("milestone:battle01-player-control-entered")
    end)

    add_callback(config.extension.functions.ControlBattleEntity, "extension-control-battle-entity", function()
        assert(extension_player_control_seen, "ControlBattleEntity preceded player-control dispatch")
        extension_control_entity_seen = true
        append_extension_trace("original:ControlBattleEntity")
    end)

    add_callback(config.extension.functions.playerReadyPc, "extension-player-ready", function()
        assert(
            phase == "extension-await-player-ready" and extension_control_entity_seen,
            "player-ready input read preceded original player-control setup"
        )
        assert(
            memory.read_u8(config.ram.CURRENT_PLAYER_INPUT, "M68K BUS") == 0,
            "player-ready seam did not observe neutral current input"
        )
        extension_ready_seen, finish_pending = true, true
        append_extension_trace("original:ControlBattleEntity:after-WaitForVInt-before-input-read")
        status("milestone:battle01-player-ready")
    end)
end

local function entity_position(entity)
    local address = config.ram.ENTITY_DATA + entity * config.ram.ENTITYDEF_SIZE
    return {
        id = entity,
        x = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_X, "M68K BUS") / config.ram.MAP_TILE_SIZE),
        y = math.floor(memory.read_u16_be(address + config.ram.ENTITYDEF_OFFSET_Y, "M68K BUS") / config.ram.MAP_TILE_SIZE),
        facing = memory.read_u8(address + config.ram.ENTITYDEF_OFFSET_FACING, "M68K BUS") & config.ram.DIRECTION_MASK,
    }
end

local function entity_position_from_character(character)
    -- csc19 resolves enemy-coded 0x80+ character operands through the
    -- 0x60-subtracted ENTITY_INDEX_LIST selector, then through ENTITY_DATA.
    local selector = character & 0xFF
    if selector >= 0x80 then selector = selector - 0x60 end
    local physical = memory.read_u8(config.ram.ENTITY_INDEX_LIST + selector, "M68K BUS")
    local position = entity_position(physical)
    position.id = character
    return position
end

local function active_party_has(ally)
    -- JoinForce rebuilds the party list before its nested JoinBattleParty
    -- call.  The final Chester list entry is therefore stale at this exact
    -- seam; the original joined/active story flags are the observed result.
    return flag_is_set(ally) and flag_is_set(32 + ally)
end

local function write_speakers(file, values)
    file:write("[")
    for index = 1, #values do
        if index > 1 then file:write(",") end
        if values[index] == false then file:write("null") else file:write(tostring(values[index])) end
    end
    file:write("]")
end

local function write_followers(file, values)
    file:write("[")
    for index, value in ipairs(values) do
        if index > 1 then file:write(",") end
        file:write(string.format('{"follower":%d,"leader":%d,"distance":%d}', value.follower, value.leader, value.distance))
    end
    file:write("]")
end

json_write = function(file, value)
    local kind = type(value)
    if kind == "nil" then file:write("null"); return end
    if kind == "boolean" or kind == "number" then file:write(tostring(value)); return end
    if kind == "string" then file:write('"' .. json_escape(value) .. '"'); return end
    assert(kind == "table", "unsupported JSON value type " .. kind)
    local count, max_index, array = 0, 0, true
    for key, _ in pairs(value) do
        count = count + 1
        if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then array = false
        elseif key > max_index then max_index = key end
    end
    if array and max_index == count then
        file:write("[")
        for index = 1, max_index do
            if index > 1 then file:write(",") end
            json_write(file, value[index])
        end
        file:write("]")
        return
    end
    file:write("{")
    local keys = {}
    for key, _ in pairs(value) do keys[#keys + 1] = key end
    table.sort(keys)
    for index, key in ipairs(keys) do
        if index > 1 then file:write(",") end
        file:write('"' .. json_escape(key) .. '":')
        json_write(file, value[key])
    end
    file:write("}")
end

local function extension_combatant(combatant)
    local index = combatant
    if combatant >= config.ram.COMBATANT_ENEMIES_START then
        index = combatant - config.ram.ENTITY_ENEMY_INDEX_DIFFERENCE
    end
    local base = config.ram.COMBATANT_DATA + index * config.ram.COMBATANT_DATA_ENTRY_SIZE
    local function byte(offset) return memory.read_u8(base + offset, "M68K BUS") end
    local function word(offset) return memory.read_u16_be(base + offset, "M68K BUS") end
    return {
        id = combatant,
        class = byte(config.ram.COMBATANT_OFFSET_CLASS),
        level = byte(config.ram.COMBATANT_OFFSET_LEVEL),
        hpMax = word(config.ram.COMBATANT_OFFSET_HP_MAX),
        hpCurrent = word(config.ram.COMBATANT_OFFSET_HP_CURRENT),
        mpMax = byte(config.ram.COMBATANT_OFFSET_MP_MAX),
        mpCurrent = byte(config.ram.COMBATANT_OFFSET_MP_CURRENT),
        attack = byte(config.ram.COMBATANT_OFFSET_ATT_CURRENT),
        defense = byte(config.ram.COMBATANT_OFFSET_DEF_CURRENT),
        agility = byte(config.ram.COMBATANT_OFFSET_AGI_CURRENT),
        move = byte(config.ram.COMBATANT_OFFSET_MOV_CURRENT),
        items = {
            word(config.ram.COMBATANT_OFFSET_ITEM_0),
            word(config.ram.COMBATANT_OFFSET_ITEM_0 + 2),
            word(config.ram.COMBATANT_OFFSET_ITEM_0 + 4),
            word(config.ram.COMBATANT_OFFSET_ITEM_0 + 6),
        },
        spells = {
            byte(config.ram.COMBATANT_OFFSET_SPELLS),
            byte(config.ram.COMBATANT_OFFSET_SPELLS + 1),
            byte(config.ram.COMBATANT_OFFSET_SPELLS + 2),
            byte(config.ram.COMBATANT_OFFSET_SPELLS + 3),
        },
        statusEffects = word(config.ram.COMBATANT_OFFSET_STATUSEFFECTS),
        x = byte(config.ram.COMBATANT_OFFSET_X),
        y = byte(config.ram.COMBATANT_OFFSET_Y),
        activationBitfield = word(config.ram.COMBATANT_OFFSET_ACTIVATION_BITFIELD),
    }
end

local function capture_extension_result()
    assert(extension_enabled and extension_ready_seen, "player-ready result captured before terminal seam")
    assert(
        extension_before_seen and extension_before_script_seen and extension_load_seen
            and extension_start_seen and extension_start_script_seen and extension_activate_seen
            and extension_region_seen and extension_spawn_seen and extension_turn_order_seen
            and extension_player_control_seen and extension_control_entity_seen,
        "player-ready lifecycle callback closure drift"
    )
    local admission = config.extension.admission
    local map = memory.read_u8(config.ram.CURRENT_MAP, "M68K BUS")
    local battle = memory.read_u8(config.ram.CURRENT_BATTLE, "M68K BUS")
    local area = {
        memory.read_u8(config.ram.BATTLE_AREA_X, "M68K BUS"),
        memory.read_u8(config.ram.BATTLE_AREA_Y, "M68K BUS"),
        memory.read_u8(config.ram.BATTLE_AREA_WIDTH, "M68K BUS"),
        memory.read_u8(config.ram.BATTLE_AREA_HEIGHT, "M68K BUS"),
    }
    assert(
        map == admission.map and battle == admission.battle
            and area[1] == admission.area[1] and area[2] == admission.area[2]
            and area[3] == admission.area[3] and area[4] == admission.area[4],
        "stable player-ready map/battle/area drift"
    )
    local region_flags = {}
    for flag = admission.regionFlagStart, admission.regionFlagEnd do
        region_flags[#region_flags + 1] = flag_is_set(flag)
        assert(not region_flags[#region_flags], "battle region flag remained set at first player-ready seam")
    end
    assert(
        flag_is_set(admission.unlockedFlag) and not flag_is_set(admission.completedFlag)
            and flag_is_set(admission.introFlag),
        "Battle01 unlock/completion/intro flags drift at player-ready seam"
    )

    local active_party, party_count = {}, memory.read_u16_be(config.ram.BATTLE_PARTY_MEMBERS_NUMBER, "M68K BUS")
    for index = 0, party_count - 1 do
        active_party[#active_party + 1] = memory.read_u8(config.ram.BATTLE_PARTY_MEMBERS + index, "M68K BUS")
    end
    assert(
        party_count == 3 and active_party[1] == 0 and active_party[2] == 1 and active_party[3] == 2,
        "Battle01 active party did not retain Bowie/Sarah/Chester"
    )
    local combatants = {}
    for _, combatant in ipairs(config.extension.participatingCombatants) do
        local record = extension_combatant(combatant)
        assert(record.x ~= 0xFF and record.y ~= 0xFF and record.hpCurrent > 0, "participating combatant was not placed and living")
        combatants[#combatants + 1] = record
    end

    local order = {}
    for index = 0, config.extension.turnOrderEntries - 1 do
        local actor = memory.read_u8(config.ram.BATTLE_TURN_ORDER + index * 2, "M68K BUS")
        if actor == 0xFF then break end
        order[#order + 1] = {
            actor = actor,
            score = memory.read_u8(config.ram.BATTLE_TURN_ORDER + index * 2 + 1, "M68K BUS"),
        }
    end
    local current_offset = memory.read_u8(config.ram.CURRENT_BATTLE_TURN, "M68K BUS")
    local current_actor = memory.read_u8(config.ram.BATTLE_TURN_ORDER + current_offset, "M68K BUS")
    local moving_actor = memory.read_u16_be(config.ram.MOVING_BATTLE_ENTITY_INDEX, "M68K BUS")
    assert(
        #order == #config.extension.participatingCombatants
            and current_actor == extension_player_actor and moving_actor == extension_player_actor,
        "turn-order/current-player closure drift at semantic input boundary"
    )

    extension_result = {
        caseId = config.caseOrder[1],
        retained = config.extension.retained,
        continuity = {
            kind = "controlled-harness-bridge",
            naturalR2bContinuity = false,
            bridge = assert(extension_bridge_result),
        },
        chronology = extension_chronology,
        admission = {
            map = map,
            battle = battle,
            area = area,
            flags = {
                f401 = flag_is_set(401),
                f501 = flag_is_set(501),
                f451 = flag_is_set(451),
            },
            regionFlags90Through105 = region_flags,
        },
        scenario = { activeParty = active_party, combatants = combatants },
        turnState = {
            entries = order,
            currentTurnOffset = current_offset,
            currentActor = current_actor,
            executedActorsBeforeReady = extension_turn_entries,
        },
        deterministicState = {
            seeded = {
                randomSeed = config.extension.bridge.randomSeed,
                randomSeedCopy = config.extension.bridge.randomSeedCopy,
                frameCounter = config.extension.bridge.frameCounter,
                secondsCounter = config.extension.bridge.secondsCounter,
                secondsCounterFrames = config.extension.bridge.secondsCounterFrames,
            },
            ready = {
                randomSeed = memory.read_u32_be(config.ram.RANDOM_SEED, "M68K BUS") & 0xFFFFFFFF,
                randomSeedCopy = memory.read_u32_be(config.ram.RANDOM_SEED_COPY, "M68K BUS") & 0xFFFFFFFF,
                frameCounter = memory.read_u8(config.ram.FRAME_COUNTER, "M68K BUS"),
                secondsCounter = memory.read_u32_be(config.ram.SECONDS_COUNTER, "M68K BUS") & 0xFFFFFFFF,
                secondsCounterFrames = memory.read_u8(config.ram.SECONDS_COUNTER_FRAMES, "M68K BUS"),
            },
        },
        readiness = {
            boundary = "ControlBattleEntity.after-WaitForVInt-before-input-read",
            pc = config.extension.functions.playerReadyPc,
            semanticInputMode = "battle-entity-movement",
            currentPlayerInput = memory.read_u8(config.ram.CURRENT_PLAYER_INPUT, "M68K BUS"),
            movingBattleEntity = moving_actor,
            viewTargetEntity = memory.read_u8(config.ram.VIEW_TARGET_ENTITY, "M68K BUS"),
            currentBattleAction = memory.read_u16_be(config.ram.CURRENT_BATTLEACTION, "M68K BUS"),
            isTargeting = memory.read_u8(config.ram.IS_TARGETING, "M68K BUS"),
            mapEventType = memory.read_u8(config.ram.MAP_EVENT_TYPE, "M68K BUS"),
            beforeBattleScriptReturned = extension_load_seen,
            battleStartScriptReturned = extension_activate_seen,
            turnOrderReturned = #extension_turn_entries > 0,
            transferPending = memory.read_u8(config.ram.MAP_EVENT_TYPE, "M68K BUS") ~= 0,
            cutsceneOrMenuModal = false,
        },
    }
    assert(
        extension_result.readiness.currentPlayerInput == 0
            and not extension_result.readiness.transferPending
            and extension_result.readiness.isTargeting == 0,
        "stable player-ready input/modal state drift"
    )
end

local function capture_messenger_result()
    assert(messenger_result == nil, "messenger result captured more than once")
    local map, x, y, facing = current_position()
    local sarah, chester = active_party_has(1), active_party_has(2)
    local guard_138 = entity_position_from_character(138)
    local guard_139 = entity_position_from_character(139)
    assert(
        follower_wait_seen and zone_return_seen and #text_ids == 18 and #speaker_operands == 18
            and #follow_commands == 2 and sarah and chester,
        string.format(
            "messenger observation closure drift before output: wait=%s zone=%s texts=%d speakers=%d follows=%d sarah=%s chester=%s",
            tostring(follower_wait_seen), tostring(zone_return_seen), #text_ids, #speaker_operands,
            #follow_commands, tostring(sarah), tostring(chester)
        )
    )
    assert(
        map == 3 and x == 43 and y == 10 and facing == config.ram.DOWN
            and guard_138.x == 27 and guard_138.y == 3 and guard_138.facing == config.ram.DOWN
            and guard_139.x == 31 and guard_139.y == 3 and guard_139.facing == config.ram.DOWN,
        string.format(
            "post-zone follower-ready map/entity state drift: player=(%d,%d,%d,%d) guard138=(%d,%d,%d) guard139=(%d,%d,%d)",
            map, x, y, facing, guard_138.x, guard_138.y, guard_138.facing,
            guard_139.x, guard_139.y, guard_139.facing
        )
    )
    messenger_result = {
        flags = { f600 = flag_is_set(600), f66 = flag_is_set(66), f603 = flag_is_set(603) },
        endpoint = { map = map, x = x, y = y, facing = facing },
        guards = { guard_138, guard_139 },
    }
end

-- The candidate shares bootstrap, dispatch, readback and finalization with R2a.
-- Only this frozen frame table supplies input after admission. Observation
-- callbacks may fail/stop, but cannot choose, delay or repair an input edge.
local function install_candidate()
    local c, f, ram = candidate, config.candidate.functions, config.ram
    local victory = natural and natural.victory
    c.order, c.consumers = 0, {}
    local function poll(kind, pc)
        if not acquisition or not c.epoch then return end
        if c.consumerPoll and c.consumerPoll.kind == kind and c.consumerPoll.frame == frame_count then return end
        c.consumerPoll = {kind=kind, frame=frame_count, emulatorFrame=emu.framecount(), pc=pc}
        if kind == "WaitForEvent-action" and c.fieldMenu and c.fieldMenu.stage == "returned" then
            c.record("field-menu:restored-field-poll", {entry=c.fieldMenu.entry, poll=c.consumerPoll})
            c.fieldMenu, c.pauseBatch = nil, true
        end
    end
    local function sample()
        local result = current_position_diagnostic()
        result.rawX = memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_X, "M68K BUS")
        result.rawY = memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_Y, "M68K BUS")
        result.mapEventWord = memory.read_u16_be(ram.MAP_EVENT_TYPE, "M68K BUS")
        result.typewriting = memory.read_u8(ram.CURRENTLY_TYPEWRITING, "M68K BUS")
        result.windowState = memory.read_u8(ram.WINDOW_IS_PRESENT, "M68K BUS")
        result.portrait = memory.read_u16_be(ram.CURRENT_PORTRAIT, "M68K BUS")
        result.speechSfx = memory.read_u16_be(ram.CURRENT_SPEECH_SFX, "M68K BUS")
        result.rngBytes = read_span(ram.RANDOM_SEED, 4)
        result.rngCopyByte = memory.read_u8(ram.RANDOM_SEED_COPY, "M68K BUS")
        result.rawTime = { frame = memory.read_u8(ram.FRAME_COUNTER, "M68K BUS"),
            seconds = memory.read_u32_be(ram.SECONDS_COUNTER, "M68K BUS"),
            secondsFrames = memory.read_u8(ram.SECONDS_COUNTER_FRAMES, "M68K BUS") }
        result.flags = {}
        for _, flag in ipairs({ 66, 600, 601, 602, 603, 604, 605, 607, 608, 401, 256, 501, 507, 982 }) do
            result.flags[tostring(flag)] = flag_is_set(flag)
        end
        if acquisition then
            result.pendingReturns, result.activeConsumers = c.pending, c.consumers
            result.lastConsumerPoll = c.consumerPoll or false
            if natural then
                for _, flag in ipairs({88, 89, 451}) do result.flags[tostring(flag)] = flag_is_set(flag) end
                result.promptChoice = (c.consumers.prompt or 0) > 0
                    and memory.read_u8(ram.CURRENT_DIAMOND_MENU_CHOICE, "M68K BUS") or false
                result.completed = c.completed
                result.fieldMenuRecovery = c.fieldMenu or false
                result.progressStalled = c.progressStalled or false
                result.stopReason = c.stopReason or c.failureReason or false
                result.battle = memory.read_u8(ram.CURRENT_BATTLE, "M68K BUS")
                result.lastMusicOrControlCommand = c.activeMusic or false
                if victory then
                    result.battlePoll = c.battlePoll or false
                    result.battleReturns = c.battleReturns
                    result.turnNumber, result.roundNumber, result.currentActor = c.turnNumber, c.roundNumber, c.currentActor
                end
            end
            result.readiness = "Unknown; use source consumer events, not typewriting/script-return alone"
        end
        return result
    end
    function c.record(kind, facts)
        c.lastCheckpoint = kind
        if natural and (kind == "operation:entry" or kind:match(":return$")
            or kind == "text:acknowledgement-read" or kind == "input:original-movement-acceptance"
            or kind:match("^natural:")) then c.progressFrame = frame_count end
        c.order = c.order + 1
        local file = assert(io.open(config.candidate.checkpointPath, "a"))
        json_write(file, { kind = kind, frame = frame_count, pc = reg("PC") & 0xFFFFFF,
            inputFrame = c.epoch and frame_count - c.epoch or false,
            facts = facts, state = sample(),
            afterStop = natural and c.stopReason or nil,
            order = acquisition and c.order or nil,
            boundary = acquisition and (callback_active and "callback-time" or "host-loop") or nil,
            emulatorFrame = acquisition and emu.framecount() or nil })
        file:write("\n"); file:close()
    end
    local function returned(kind, target, on_return)
        local stack = reg("A7") & 0xFFFFFF
        local pc = memory.read_u32_be(stack, "M68K BUS") & 0xFFFFFF
        assert(pc < 0x200000 and pc % 2 == 0, "original return is outside canonical code")
        local pending = true
        c.pending = c.pending + 1
        c.consumers[kind] = (c.consumers[kind] or 0) + 1
        c.record(kind .. ":entry", { target = target, returnPc = pc, stack = stack })
        add_callback(pc, "candidate:return", function()
            if not pending or (reg("A7") & 0xFFFFFF) ~= ((stack + 4) & 0xFFFFFF) then return end
            pending, c.pending = false, c.pending - 1
            c.consumers[kind] = c.consumers[kind] - 1
            c.record(kind .. ":return", { target = target, returnPc = pc, d0 = reg("D0") & 0xFFFF })
            if on_return then on_return() end
        end)
    end
    local function entity(character)
        local selector = character >= 128 and character - ram.ENTITY_ENEMY_INDEX_DIFFERENCE or character
        local physical = memory.read_u8(ram.ENTITY_INDEX_LIST + selector, "M68K BUS")
        if physical >= 48 then return {character=character, selector=selector, physical=physical} end
        local address = ram.ENTITY_DATA + physical * ram.ENTITYDEF_SIZE
        return { character = character, selector = selector, physical = physical,
            address = address, bytes = read_span(address, ram.ENTITYDEF_SIZE) }
    end
    local function camera_state()
        local function word(name) return memory.read_u16_be(ram[name], "M68K BUS") end
        local camera = {planeA={x=word("VIEW_PLANE_A_PIXEL_X"), y=word("VIEW_PLANE_A_PIXEL_Y"),
                destinationX=word("VIEW_PLANE_A_PIXEL_X_DEST"), destinationY=word("VIEW_PLANE_A_PIXEL_Y_DEST")},
            planeB={x=word("VIEW_PLANE_B_PIXEL_X"), y=word("VIEW_PLANE_B_PIXEL_Y"),
                destinationX=word("VIEW_PLANE_B_PIXEL_X_DEST"), destinationY=word("VIEW_PLANE_B_PIXEL_Y_DEST")},
            scrollingPlanes=memory.read_u8(ram.VIEW_SCROLLING_PLANES_BITFIELD, "M68K BUS"),
            layerType=memory.read_u8(ram.MAP_AREA_LAYER_TYPE, "M68K BUS"),
            layer1AutoscrollXY=word("MAP_AREA_LAYER1_AUTOSCROLL_X"),
            layer2AutoscrollXY=word("MAP_AREA_LAYER2_AUTOSCROLL_X")}
        local scrolling = camera.scrollingPlanes
        if camera.layer1AutoscrollXY ~= 0 then scrolling = scrolling & 3 end
        if camera.layer2AutoscrollXY ~= 0 then scrolling = scrolling & 12 end
        camera.effectiveScrollingPlanes = scrolling
        return camera
    end
    if natural then
        local nf, completed = natural.functions, {}
        c.completed, c.programs, c.nextWarp = completed, {}, 1
        if victory then c.battleReturns, c.turnNumber, c.roundNumber = {}, 0, 0 end
        local function byte(name) return memory.read_u8(ram[name], "M68K BUS") end
        local function word(name) return memory.read_u16_be(ram[name], "M68K BUS") end
        function c.stop(reason, facts)
            if c.stopReason then return end
            c.stopReason = reason
            c.record("stop:" .. reason, facts or {})
            c.terminal = sample()
            c.terminal.accounting = c.accounting()
            c.terminal.stop = {reason=reason, facts=facts or {}, boundary=callback_active and "callback-time" or "host-loop",
                pc=reg("PC") & 0xFFFFFF, frame=frame_count, emulatorFrame=emu.framecount(), order=c.order}
            finish_pending = true
        end
        function c.accounting()
            local allies, joined, active, party, combatants, order, regions = {}, {}, {}, {}, {}, {}, {}
            for id = 0, ram.COMBATANT_ALLIES_NUMBER - 1 do
                allies[#allies + 1] = extension_combatant(id)
                joined[#joined + 1] = flag_is_set(ram.FORCEMEMBER_JOINED_FLAGS_START + id)
                active[#active + 1] = flag_is_set(ram.FORCEMEMBER_ACTIVE_FLAGS_START + id)
            end
            local count = word("BATTLE_PARTY_MEMBERS_NUMBER")
            assert(count <= ram.COMBATANT_ALLIES_NUMBER, "party count out of bounds")
            for i = 0, count - 1 do
                party[#party + 1] = memory.read_u8(ram.BATTLE_PARTY_MEMBERS + i, "M68K BUS")
            end
            for _, id in ipairs(party) do combatants[#combatants + 1] = extension_combatant(id) end
            for id = ram.COMBATANT_ENEMIES_START, ram.COMBATANT_ENEMIES_START + ram.COMBATANT_ENEMIES_NUMBER - 1 do
                local entry = extension_combatant(id)
                if entry.x ~= 255 and entry.y ~= 255 then combatants[#combatants + 1] = entry end
            end
            for i = 0, natural.turnOrderEntries - 1 do
                local address = ram.BATTLE_TURN_ORDER + i * 2
                local actor = memory.read_u8(address, "M68K BUS")
                if actor == 255 then break end
                order[#order + 1] = {actor=actor, score=memory.read_u8(address + 1, "M68K BUS")}
            end
            for flag = 90, 105 do regions[#regions + 1] = flag_is_set(flag) end
            return {allies=allies, joined=joined, active=active, party=party, combatants=combatants,
                gold=memory.read_u32_be(ram.CURRENT_GOLD, "M68K BUS"), turnOrder=order, turnOffset=byte("CURRENT_BATTLE_TURN"),
                regionFlags=regions, actor=c.firstActor or false, entity135=entity(135),
                rngBytes=read_span(ram.RANDOM_SEED, 4), rngCopyByte=byte("RANDOM_SEED_COPY"),
                rawTime=sample().rawTime}
        end
        function c.checkpoint(name)
            completed[name] = true
            c.record("natural:" .. name, c.accounting())
        end
        if victory then
            function c.battle_observation(grid)
                local facts = {actor=c.currentActor, turn=c.turnNumber, round=c.roundNumber,
                    action=word("CURRENT_BATTLEACTION"), itemOrSpell=word("BATTLEACTION_ITEM_OR_SPELL"),
                    itemSlot=word("BATTLEACTION_ITEM_SLOT"), attackType=word("BATTLESCENE_ATTACK_TYPE"),
                    sceneItem=word("BATTLESCENE_ITEM"), diamondMenuIndex=c.battleDiamond or false,
                    sceneActor=byte("BATTLESCENE_ACTOR"), sceneExp=word("BATTLESCENE_EXP"), sceneGold=word("BATTLESCENE_GOLD"),
                    actorX=word("BATTLE_ACTOR_X"), actorY=word("BATTLE_ACTOR_Y"),
                    targetX=word("BATTLE_TARGET_X"), targetY=word("BATTLE_TARGET_Y"),
                    chosenX=byte("BATTLE_ENTITY_CHOSEN_X"), chosenY=byte("BATTLE_ENTITY_CHOSEN_Y"),
                    menuChoice=byte("CURRENT_DIAMOND_MENU_CHOICE"), targeting=byte("IS_TARGETING"),
                    registers={d0=reg("D0"), d1=reg("D1"), d2=reg("D2"), d3=reg("D3"),
                        d4=reg("D4"), d5=reg("D5"), d6=reg("D6"), d7=reg("D7"), a0=reg("A0"), a1=reg("A1")},
                    accounting=c.accounting(), records={}}
                local count = word("TARGETS_LIST_LENGTH")
                -- The list shares storage across scene/field phases. Retain its raw length;
                -- decode only within the original combatant capacity.
                facts.targetCount = count
                facts.heal = natural.victory.heal
                facts.herb = natural.victory.herb
                facts.displayedSpells = {}
                for slot=0,3 do
                    facts.displayedSpells[#facts.displayedSpells + 1] = memory.read_u16_be(ram.DISPLAYED_ICON_1 + slot * 2, "M68K BUS")
                end
                facts.displayedItems = facts.displayedSpells
                if count <= ram.COMBATANT_ALLIES_NUMBER + ram.COMBATANT_ENEMIES_NUMBER then
                    facts.targets = read_span(ram.TARGETS_LIST, count)
                end
                local function record(id)
                    local index = id >= ram.COMBATANT_ENEMIES_START and id - ram.ENTITY_ENEMY_INDEX_DIFFERENCE or id
                    local address = ram.COMBATANT_DATA + index * ram.COMBATANT_DATA_ENTRY_SIZE
                    local target = entity(id)
                    if target.address then
                        target.x = memory.read_u16_be(target.address + ram.ENTITYDEF_OFFSET_X, "M68K BUS")
                        target.y = memory.read_u16_be(target.address + ram.ENTITYDEF_OFFSET_Y, "M68K BUS")
                        target.destinationX = memory.read_u16_be(target.address + ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS")
                        target.destinationY = memory.read_u16_be(target.address + ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS")
                        target.facing = memory.read_u8(target.address + ram.ENTITYDEF_OFFSET_FACING, "M68K BUS")
                    end
                    facts.records[#facts.records + 1] = {id=id, exp=memory.read_u8(address + ram.COMBATANT_OFFSET_EXP, "M68K BUS"),
                        bytes=read_span(address, ram.COMBATANT_DATA_ENTRY_SIZE), entity=target}
                end
                for id=0,ram.COMBATANT_ALLIES_NUMBER-1 do record(id) end
                for id=ram.COMBATANT_ENEMIES_START,ram.COMBATANT_ENEMIES_START+ram.COMBATANT_ENEMIES_NUMBER-1 do record(id) end
                if grid then
                    facts.movableGrid = read_span(ram.FF4D00_LOADING_SPACE, ram.MAP_ARRAY_BYTESIZE)
                    facts.gridWidth, facts.gridHeight, facts.tileSize = ram.MAP_SIZE_MAX_TILEWIDTH, ram.MAP_SIZE_MAX_TILEHEIGHT, ram.MAP_TILE_SIZE
                end
                return facts
            end
            -- Only these long-lived calls may cross a movement save. Their original
            -- return descriptors are data; the callback behavior is rebuilt on load.
            function c.arm_battle_return(kind, target, restored)
                assert(kind == "loop" or kind == "turn" or kind == "player", "unknown persistent battle return")
                local entry = restored or {kind=kind, target=target, stack=reg("A7") & 0xFFFFFF}
                if not restored then entry.pc = memory.read_u32_be(entry.stack, "M68K BUS") & 0xFFFFFF end
                assert(entry.kind == kind and entry.target == target and entry.pc < 0x200000 and entry.pc % 2 == 0
                    and (memory.read_u32_be(entry.stack, "M68K BUS") & 0xFFFFFF) == entry.pc,
                    "battle return descriptor/readback mismatch")
                assert(not c.battleReturns[kind], "overlapping persistent battle return")
                c.battleReturns[kind] = entry
                add_callback(entry.pc, "candidate:battle-return:" .. kind, function()
                    if c.battleReturns[kind] ~= entry or (reg("A7") & 0xFFFFFF) ~= ((entry.stack + 4) & 0xFFFFFF) then return end
                    c.battleReturns[kind], c.battlePoll = nil, nil
                    c.record("battle:" .. kind .. "-return", c.battle_observation())
                    if kind == "loop" then
                        assert(completed.victory and completed.afterProgram and completed.afterReturn and completed.flagSet
                            and not flag_is_set(401) and flag_is_set(501) and (reg("D4") & 0xFFFF) == 1,
                            "BattleLoop returned without the observed victory/program/flag sequence")
                        completed.battleReturn = true
                    end
                end)
            end
            function c.restore_battle_returns()
                local entries = c.battleReturns
                c.battleReturns = {}
                local targets = {loop=nf.BattleLoop, turn=nf.ExecuteIndividualTurn, player=nf.ProcessBattleEntityControlPlayerInput}
                for kind, entry in pairs(entries) do
                    assert(targets[kind], "unsupported saved battle closure")
                    c.arm_battle_return(kind, targets[kind], entry)
                end
            end
            local function battle_call(name, after)
                add_callback(nf[name], "candidate:battle-observe:" .. name, function()
                    if not completed.admission or c.stopReason then return end
                    c.record("battle:" .. name .. ":before", c.battle_observation())
                    returned("battle:" .. name, nf[name], function()
                        c.record("battle:" .. name .. ":after", c.battle_observation())
                        if after then after() end
                    end)
                end)
            end
            for _, name in ipairs({"StartAiControl", "ExecuteAiControl", "ExecuteAiCommand", "WriteBattlesceneScript",
                "battlesceneScript_ApplyActionEffect", "battlesceneScript_DropEnemyItem", "battlesceneScript_End",
                "InitializeBattlescene", "ExecuteBattlesceneScript", "EndBattlescene",
                "ProcessAfterTurnEffects", "ProcessKilledCombatants", "CountRemainingCombatants",
                "battlesceneScript_UseItem", "battlesceneScript_BreakUsedItem", "RemoveItemBySlot"}) do battle_call(name) end
            for _, name in ipairs({"GenerateRandomNumber", "GenerateRandomOrDebugNumber"}) do
                add_callback(nf[name], "candidate:battle-rng", function()
                    if not completed.admission or c.stopReason then return end
                    local before = {seed=read_span(ram.RANDOM_SEED, 4), d0=reg("D0"), d6=reg("D6")}
                    returned("rng:" .. name, nf[name], function()
                        c.record("rng:draw", {source=name, before=before, seed=read_span(ram.RANDOM_SEED, 4), d0=reg("D0"), d7=reg("D7")})
                    end)
                end)
            end
            battle_call("ExecuteAfterBattleCutscene", function()
                assert(completed.afterProgram and completed.afterTail and flag_is_set(401) and not flag_is_set(501), "after-program return/flags drift")
                completed.afterReturn = true
            end)
            -- This is a shared function tail reached by BRA, with saved D0/D1
            -- above the caller's return address. The enclosing call owns return.
            add_callback(nf.EndAfterBattleCutscene, "candidate:after-tail", function()
                if not completed.admission or c.stopReason then return end
                assert(completed.afterProgram and not completed.afterTail
                    and c.consumers["battle:ExecuteAfterBattleCutscene"] == 1, "after-cutscene tail without owning call")
                completed.afterTail = true
                c.record("battle:after-tail", {stack=reg("A7") & 0xFFFFFF, target=nf.EndAfterBattleCutscene})
            end)
            for _, name in ipairs({"BattlefieldMenu",
                "ExecuteBattleaction_Egress", "ExecuteBattleaction_AngelWing"}) do
                add_callback(nf[name], "candidate:unsupported-battle-input", function()
                    if completed.admission then c.stop("unsupported-battle-input", {source=name, observation=c.battle_observation()}) end
                end)
            end
            for _, name in ipairs({"ClearFlag", "SetFlag"}) do
                add_callback(nf[name], "candidate:victory-flag", function()
                    if not completed.victory then return end
                    local flag = reg("D1") & 0xFFFF
                    if flag ~= 401 and flag ~= 501 then return end
                    assert(completed.afterReturn and ((name == "ClearFlag" and flag == 401 and not completed.flagClear)
                        or (name == "SetFlag" and flag == 501 and completed.flagClear and not completed.flagSet)), "victory flag order drift")
                    returned("victory:" .. name, nf[name], function()
                        assert(flag_is_set(flag) == (name == "SetFlag"), "victory flag write failed")
                        completed[name == "ClearFlag" and "flagClear" or "flagSet"] = true
                    end)
                end)
            end
            add_callback(nf.SwitchMap, "candidate:victory-switch-map", function()
                if not completed.battleReturn then return end
                assert(not completed.switchReturn, "repeated post-victory SwitchMap")
                returned("victory:SwitchMap", nf.SwitchMap, function() completed.switchReturn = true end)
            end)
            add_callback(nf.ExplorationLoop, "candidate:victory-exploration", function()
                if not completed.battleReturn then return end
                assert(completed.switchReturn, "exploration before SwitchMap return")
                completed.exploration = true
                c.record("victory:exploration-entry", c.accounting())
            end)
            function c.battle_selection_supported(kind)
                local choice = byte("CURRENT_DIAMOND_MENU_CHOICE")
                if kind == "battle-item-action" then return choice == 0 end
                if kind == "battle-item" then
                    return choice < 4 and (memory.read_u16_be(ram.DISPLAYED_ICON_1 + choice * 2, "M68K BUS")
                        & ram.ITEMENTRY_MASK_INDEX) == ram.ITEM_MEDICAL_HERB
                end
                return true
            end
            if candidate.diagnostic then
                -- Measurement locals are never serialized, and never alter the ordinary
                -- pending/consumer/continuation state. A fixed return dispatcher avoids
                -- accumulating one callback closure for every interrupt/update.
                local d = {loaded=false, active=false, seen={}, returns={}, registered={},
                    interrupts=0, vints={}, context={vint=0, parent=0, service=false},
                    draws=0, updates=0, zeroDrawUpdates=0}
                c.heal = d
                local hf, hr = candidate.diagnostic.functions, candidate.diagnostic.ram
                local function hb(name) return memory.read_u8(hr[name], "M68K BUS") end
                local function hw(name) return memory.read_u16_be(hr[name], "M68K BUS") end
                local function facts()
                    local slots = {}
                    for i=0,7 do slots[#slots+1]=memory.read_u32_be(hr.VINT_FUNC_ADDRS + 4*i, "M68K BUS") end
                    return {vintCount=d.interrupts, vint=d.context.vint, vintParent=d.context.parent,
                        vintDepth=#d.vints, inVint=#d.vints > 0, service=d.context.service,
                        vintParameters=hb("VINT_PARAMETERS"), vintEnabled=hb("VINT_ENABLED"),
                        enabledSlots=hb("VINT_FUNCS_ENABLED_BITFIELD"), slots=slots,
                        messageSpeed=hb("MESSAGE_SPEED"), noMessages=hb("NO_BATTLE_MESSAGES_TOGGLE"),
                        toggle=hb("UPDATE_SPELLANIMATION_TOGGLE"), control=hb("byte_FFB585"),
                        lifetime=hw("byte_FFB404"), animation=hb("CURRENT_SPELLANIMATION"),
                        properties=read_span(hr.SPELLANIMATION_PROPERTIES, hr.byte_FFB532-hr.SPELLANIMATION_PROPERTIES),
                        fairy=read_span(hr.byte_FFB532, 16), draws=d.draws,
                        a6=reg("A6"), d0=reg("D0"), input=memory.read_u8(ram.PLAYER_1_INPUT, "M68K BUS")}
                end
                local function record(kind, extra)
                    local value=facts()
                    if extra then for key,item in pairs(extra) do value[key]=item end end
                    d.seen[kind]=(d.seen[kind] or 0)+1
                    c.record("heal:" .. kind, value)
                end
                local function after_call(done)
                    local stack=reg("A7") & 0xFFFFFF
                    local pc=memory.read_u32_be(stack, "M68K BUS") & 0xFFFFFF
                    assert(pc < 0x200000 and pc % 2 == 0, "HEAL diagnostic return outside code")
                    d.returns[pc]=d.returns[pc] or {}
                    d.returns[pc][stack]=d.returns[pc][stack] or {}
                    table.insert(d.returns[pc][stack], done)
                    if not d.registered[pc] then
                        d.registered[pc]=true
                        add_callback(pc, "candidate:heal-return", function()
                            local key=((reg("A7") & 0xFFFFFF)-4) & 0xFFFFFF
                            local actions=d.returns[pc][key]
                            if actions then
                                d.returns[pc][key]=nil
                                for i=#actions,1,-1 do actions[i]() end
                            end
                        end)
                    end
                end
                local function hook(name, handler)
                    add_callback(hf[name] or nf[name], "candidate:heal:" .. name, function()
                        if d.loaded and d.active then handler() end
                    end)
                end
                add_callback(nf.WriteBattlesceneScript, "candidate:heal-action", function()
                    if not d.loaded then return end
                    assert(not d.active, "HEAL diagnostic reached another action")
                    local action=c.battle_observation()
                    assert(action.actor == 1 and action.action == 1 and action.itemOrSpell == 0,
                        "HEAL diagnostic requires Sarah HEAL1 as first action")
                    assert(hb("MESSAGE_SPEED") == 2 and hb("NO_BATTLE_MESSAGES_TOGGLE") == 0,
                        "HEAL diagnostic retained settings drift")
                    d.active, d.actionFrame=true, frame_count
                    record("action", {battle=action})
                end)
                hook("battlesceneScript_ApplyActionEffect", function()
                    local action=c.battle_observation()
                    assert(action.targetCount == 1 and action.targets[1] == 1, "HEAL diagnostic requires self target")
                    d.selfTarget=true
                    record("target", {battle=action})
                end)
                hook("ExecuteBattlesceneScript", function()
                    assert(d.selfTarget and not d.playback, "HEAL diagnostic playback identity")
                    d.playback=true
                    record("playback")
                end)
                for _, name in ipairs(candidate.diagnostic.commands) do
                    hook(name, function()
                        if not d.playback then return end
                        local stream=reg("A6") & 0xFFFFFF
                        record("bsc:before", {name=name, opcode=memory.read_u16_be((stream-2)&0xFFFFFF,"M68K BUS")})
                        after_call(function() record("bsc:after", {name=name}) end)
                    end)
                end
                hook("VInt", function()
                    -- Cleanup can tail-call WaitForVInt inside graphics. A new
                    -- activation starts outside any service, even when nested.
                    local interrupted=d.context
                    table.insert(d.vints, interrupted)
                    d.interrupts=d.interrupts+1
                    d.context={vint=d.interrupts, parent=interrupted.vint, service=false}
                    record("vint", {interruptedService=interrupted.service})
                end)
                hook("vintReturn", function()
                    assert(#d.vints > 0 and not d.context.service, "HEAL diagnostic unpaired VInt/service return")
                    record("vint-return")
                    d.context=table.remove(d.vints)
                end)
                for _, entry in ipairs({{"VInt_UpdateBattlesceneGraphics","graphics"},{"VInt_UpdateWindows","windows"}}) do
                    local name, role=entry[1],entry[2]
                    hook(name,function()
                        local context=d.context
                        local prior=context.service
                        context.service=role
                        record(role .. ":before", {slot=reg("D6") & 0xFFFF})
                        after_call(function()
                            assert(d.context == context and context.service == role,
                                "HEAL diagnostic service return context drift")
                            record(role .. ":after")
                            context.service=prior
                        end)
                    end)
                end
                for _, entry in ipairs({{"spellanimationSetup_HealingFairy","setup"},
                    {"UpdateSpellanimation","update"},{"spellanimationUpdate_HealingFairy","fairy"},
                    {"ReinitializeSceneAfterSpell","cleanup"}}) do
                    local name,role=entry[1],entry[2]
                    hook(name,function()
                        local draws=d.draws
                        record(role .. ":before")
                        after_call(function()
                            if role == "fairy" then
                                d.updates=d.updates+1
                                if d.draws == draws then d.zeroDrawUpdates=d.zeroDrawUpdates+1 end
                            end
                            record(role .. ":after", {drawCount=d.draws-draws})
                        end)
                    end)
                end
                hook("GenerateRandomNumber", function() d.draws=d.draws+1 end)
                hook("bsc0D_endAnimation", function()
                    d.stopControl=false
                    record("stop-request")
                    after_call(function() record("stop:after") end)
                end)
                hook("loc_190C4", function()
                    if not d.stopControl then d.stopControl=true; record("stop-control") end
                end)
                hook("setupEnabled", function() record("setup-enabled") end)
                hook("cleanupCleared", function() record("cleanup-cleared") end)
                hook("loc_1921A", function() record("timed-input-read") end)
                hook("return_19228", function() record("timed-input-end") end)
                hook("EndBattlescene", function()
                    after_call(function()
                        record("scene:after", {battle=c.battle_observation()})
                        c.stop("heal-diagnostic-complete", {diagnostic=candidate.diagnostic.kind})
                    end)
                end)
                function c.heal_summary()
                    local pending=0
                    for _, stacks in pairs(d.returns) do for _, actions in pairs(stacks) do pending=pending+#actions end end
                    return {kind=candidate.diagnostic.kind, loadedBeforeInput=d.loaded, selfTarget=d.selfTarget,
                        actionFrame=d.actionFrame, seen=d.seen, updates=d.updates, zeroDrawUpdates=d.zeroDrawUpdates,
                        pendingMeasurementReturns=pending, pendingVIntContexts=#d.vints, service=d.context.service}
                end
            end
            for _, item in ipairs({{"ExecuteDiamondMenu", "diamondInputPc", "battle-menu"},
                {"ExecuteBattlefieldMagicMenu", "magicInputPc", "battle-magic"},
                {"SelectSpellLevel", "spellLevelInputPc", "battle-spell-level"},
                {"ExecuteBattlefieldItemMenu", "itemInputPc", "battle-item"},
                {"ControlCursorEntity_ChooseTarget", "targetInputPc", "battle-target"}}) do
                local function consumer_kind()
                    return item[1] == "ExecuteDiamondMenu" and c.battleDiamond == ram.MENU_ITEM
                        and "battle-item-action" or item[3]
                end
                add_callback(nf[item[1]], "candidate:battle-input-consumer", function()
                    if not c.battleReturns.player or #c.programs > 0 then return end
                    c.battlePoll = nil
                    if item[1] == "ExecuteDiamondMenu" then
                        c.battleDiamond, c.itemUseSelected = reg("D2") & 0xFFFF, false
                        if c.battleDiamond ~= ram.MENU_ITEM and c.battleDiamond ~= ram.MENU_BATTLE_WITH_STAY
                            and c.battleDiamond ~= ram.MENU_BATTLE_WITH_SEARCH then
                            c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                            return
                        end
                    elseif item[1] == "ExecuteBattlefieldItemMenu" and (not c.itemUseSelected
                        or (reg("A0") & 0xFFFFFF) ~= nf.CreatePulsatingItemRangeGrid) then
                        c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                        return
                    end
                    if item[1] == "SelectSpellLevel" and ((c.consumers["battle-magic"] or 0) ~= 1
                        or (reg("D0") & 0xFFFF) ~= ram.SPELL_HEAL) then
                        c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                        return
                    end
                    local kind = consumer_kind()
                    c.record(kind .. ":selected", c.battle_observation())
                    returned(kind, nf[item[1]], function()
                        c.battlePoll = nil
                        c.record(kind .. ":result", c.battle_observation())
                        local result = reg("D0") & 0xFFFF
                        if kind == "battle-item-action" then
                            c.itemUseSelected = result == 0
                            if result ~= 0 and result ~= 0xFFFF then
                                c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                            end
                        elseif kind == "battle-item" then
                            c.itemUseSelected = false
                            if result ~= 0xFFFF then
                                local slot = reg("D1") & 0xFFFF
                                local actor = extension_combatant(c.currentActor)
                                if (result & ram.ITEMENTRY_MASK_INDEX) ~= ram.ITEM_MEDICAL_HERB then
                                    c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                                else
                                    assert(slot < 4 and actor.items[slot + 1] == result, "Medical Herb selected slot/inventory mismatch")
                                end
                            end
                        end
                        if item[1] == "ExecuteDiamondMenu" then c.battleDiamond = nil end
                        c.pauseBatch = true
                    end)
                end)
                add_callback(nf[item[2]], "candidate:battle-input-poll", function()
                    local kind = consumer_kind()
                    if (c.consumers[kind] or 0) == 0 then return end
                    local input = byte("CURRENT_PLAYER_INPUT")
                    if (input & (ram.INPUT_A | ram.INPUT_C)) ~= 0 and not c.battle_selection_supported(kind) then
                        c.stop("unsupported-battle-input", {source=item[1], observation=c.battle_observation()})
                        return
                    end
                    c.battlePoll = {kind=kind, frame=frame_count, pc=nf[item[2]], turn=c.turnNumber,
                        menuIndex=c.battleDiamond or false,
                        targetIndex=reg("D1") & 0xFFFF, targetCount=reg("D7") & 0xFFFF}
                    if byte("CURRENT_PLAYER_INPUT") == 0 then c.pauseBatch = true end
                    c.record("battle:input-read", {poll=c.battlePoll, input=byte("CURRENT_PLAYER_INPUT"), choice=byte("CURRENT_DIAMOND_MENU_CHOICE")})
                    if byte("CURRENT_PLAYER_INPUT") ~= 0 then c.battlePoll, c.pauseBatch = nil, true end
                end)
            end
        end
        function c.program_entry(target)
            if target == nf.cs_52F0C then
                assert(completed.royal and not flag_is_set(607), "off-route Astral repeat/premature prompt")
            elseif target == nf.cs_53996 then
                assert(c.map19Captured and not flag_is_set(605) and not flag_is_set(507), "unexpected royal program caller")
            elseif target == nf.cs_53EF4 then
                assert(completed.astral and flag_is_set(608) and not flag_is_set(256), "guard caller flags drift")
            elseif target == nf.bbcs_01 then
                assert(completed.admission and not completed.beforeScript, "before program out of order")
            elseif victory and target == nf.abcs_battle01 then
                assert(completed.victory and not completed.afterProgram, "after-program before natural victory/repeated")
            end
            c.programs[#c.programs + 1] = {target=target, stack=reg("A7") & 0xFFFFFF}
        end
        function c.program_return(target)
            local program = table.remove(c.programs)
            assert(program and program.target == target and not program.operation, "program/operation return stack mismatch")
            if target == nf.cs_53996 then completed.royalScript = true end
            if target == nf.cs_52F0C then
                assert(flag_is_set(89) and flag_is_set(608), "off-route Astral decline")
                completed.astralScript = true
            end
            if target == nf.cs_53EF4 then
                assert(flag_is_set(401) and not flag_is_set(256), "guard script/caller flag order drift")
                completed.guardScript = true
            end
            if target == nf.bbcs_01 then completed.beforeScript = true end
            if target == nf.ms_Empty and completed.load then completed.startScript = true end
            if victory and target == nf.abcs_battle01 then completed.afterProgram = true end
        end
        -- A completed dispatcher iteration proves only that operation's blocking work returned.
        -- Non-waited entity scripts remain live and are not declared complete here.
        add_callback(nf.loc_47140, "candidate:operation-return", function()
            if not c.epoch then return end
            local program = c.programs[#c.programs]
            assert(program, "script dispatcher without an observed program")
            if program.operation then
                if program.awaited then
                    local target = entity(program.entity)
                    assert(memory.read_u32_be(target.address + ram.ENTITYDEF_OFFSET_ACTSCRIPTADDR, "M68K BUS") == f.eas_Idle,
                        "awaited operation returned with a non-idle entity")
                end
                c.record("operation:return", {program=program.target, operation=program.operation,
                    nextCursor=reg("A6") & 0xFFFFFF, entity=program.entity and entity(program.entity) or false})
                if program.target == nf.cs_52F0C and program.operation.opcode == 0x0C
                    and memory.read_u16_be(program.operation.pc + 2, "M68K BUS") == 89
                    and not flag_is_set(89) then
                    c.stop("selected-prompt-decline", {program=program.target, operation=program.operation})
                end
                program.operation, program.entity, program.awaited = nil, nil, nil
            end
        end)
        add_callback(nf.loc_47156, "candidate:operation-entry", function()
            if not c.epoch then return end
            local program, cursor = c.programs[#c.programs], reg("A6") & 0xFFFFFF
            assert(program and not program.operation, "overlapping script operation")
            local opcode = memory.read_u16_be(cursor, "M68K BUS")
            if cursor == nf.cs_52F24 then error("off-route Astral repeat") end
            if cursor == nf.cs_52F40 then assert(flag_is_set(89), "Astral accepted branch without F89") end
            if opcode == 65535 then return end
            program.operation = {pc=cursor, opcode=opcode}
            local facts = {program=program.target, operation=program.operation,
                operands=read_span(cursor + 2, 8), inputBlocked=true}
            if opcode == 0x14 or opcode == 0x15 or opcode == 0x16 or opcode == 0x23 then
                local character = memory.read_u16_be(cursor + 2, "M68K BUS")
                -- Explicit grouping: both actscript commands encode a byte character.
                if opcode == 0x14 or opcode == 0x15 or opcode == 0x23 then character = memory.read_u8(cursor + 2, "M68K BUS") end
                program.entity = character
                facts.entity = entity(character)
                facts.awaited = opcode == 0x16 or ((opcode == 0x14 or opcode == 0x15)
                    and memory.read_u8(cursor + 3, "M68K BUS") ~= 0)
            end
            program.awaited = facts.awaited
            c.record("operation:entry", facts)
        end)
        add_callback(nf.GetEntityAddressFromCharacter, "candidate:entity-lookup", function()
            local program = c.programs[#c.programs]
            if not c.epoch or not program or not program.entity then return end
            local character = reg("D0") & 0xFF
            local expected = entity(character)
            c.record("entity:lookup-before", expected)
            returned("entity:lookup", nf.GetEntityAddressFromCharacter, function()
                local target = reg("A5") & 0xFFFFFF
                c.record("entity:lookup-return", {character=character, selector=expected.selector,
                    physical=reg("D0") & 0xFF, target=target,
                    bytes=read_span(target, ram.ENTITYDEF_SIZE)})
                assert(target == expected.address, "entity lookup source/readback drift")
            end)
        end)
        add_callback(config.r1.functions.setupResolutionReturnAddress, "candidate:setup-selection", function()
            if not c.epoch then return end
            local pointer = reg("A0") & 0xFFFFFF
            local map = byte("CURRENT_MAP")
            local expected = natural.setups[tostring(map)]
            if victory and completed.exploration and map == natural.postVictorySetup.map then
                assert(completed.afterReturn and completed.battleReturn and completed.flagClear and completed.flagSet
                    and not flag_is_set(401) and flag_is_set(501)
                    and byte("CURRENT_BATTLE") == ram.NOT_CURRENTLY_IN_BATTLE, "post-victory setup before field return")
                expected = natural.postVictorySetup.pointer
            end
            assert(expected and pointer == expected, "unexpected selected map setup")
            c.record("setup:selected", {map=map, pointer=pointer})
        end)
        add_callback(config.r1.functions.initCallAddress, "candidate:init-selection", function()
            if not c.epoch then return end
            local target = reg("A0") & 0xFFFFFF
            c.record("init:selected", {target=target, stack=reg("A7") & 0xFFFFFF})
            c.initCallbacks = c.initCallbacks or {}
            if c.initCallbacks[target] then return end
            c.initCallbacks[target] = true
            add_callback(target, "candidate:init-entry", function()
                if not c.epoch or (reg("A0") & 0xFFFFFF) ~= target then return end
                returned("natural:init", target, function()
                    if target == nf.ms_map20_InitFunction and completed.royalScript and not completed.royal then
                        assert(flag_is_set(605), "royal caller returned before F605")
                        c.checkpoint("royal")
                    end
                end)
            end)
        end)
        for _, name in ipairs({"Map19_EntityEvent12", "Map21_EntityEvent0"}) do
            add_callback(nf[name], "candidate:caller", function()
                assert(c.epoch and c.map19Captured, "natural caller before Map19")
                if name == "Map19_EntityEvent12" then
                    assert(completed.royal and not flag_is_set(607), "off-route Astral caller")
                else
                    assert(completed.astral and flag_is_set(608) and not flag_is_set(256), "off-route guard caller")
                end
                returned("caller:" .. name, nf[name], function()
                    if name == "Map19_EntityEvent12" then
                        assert(completed.astralScript and flag_is_set(607), "Astral caller/flag return mismatch")
                        c.checkpoint("astral")
                    else
                        assert(completed.guardScript and flag_is_set(256), "guard caller/flag return mismatch")
                        c.checkpoint("guard")
                    end
                end)
            end)
        end
        add_callback(nf.loc_4756A, "candidate:zone-target", function()
            if not c.map19Captured then return end
            local target = reg("A0") & 0xFFFFFF
            assert(byte("CURRENT_MAP") == 19 and target == nf.Map19_DefaultZoneEvent,
                "unexpected continuation zone")
            c.record("zone:selected-target", {target=target})
        end)
        add_callback(nf.entityCallPc, "candidate:entity-target", function()
            if not c.map19Captured then return end
            local target = reg("A0") & 0xFFFFFF
            assert(target == nf.Map19_EntityEvent12 or target == nf.Map21_EntityEvent0,
                "off-route entity interaction")
            c.record("entity:caller-selected", {target=target})
        end)
        add_callback(nf.CheckBattle, "candidate:battle-check", function()
            if not c.epoch then return end
            local incoming = reg("D0") & 0xFF
            c.record("battle:check", {incomingMap=incoming})
            if victory and completed.victory then
                returned("victory:CheckBattle", nf.CheckBattle, function()
                    c.record("victory:battle-check-result", {battleIndex=reg("D7") & 0xFFFF})
                end)
                return
            end
            if incoming ~= natural.admission.map then return end
            assert(completed.guard and c.nextWarp > #natural.warps and flag_is_set(401)
                and not flag_is_set(501), "premature battle admission")
            returned("battle:check", nf.CheckBattle, function()
                assert((reg("D7") & 0xFFFF) == natural.admission.battle, "CheckBattle index drift")
                c.record("battle:check-result", {battleIndex=reg("D7") & 0xFFFF})
                c.checkpoint("admission")
            end)
        end)
        add_callback(nf.BattleLoop, "candidate:battle-loop", function()
            assert(completed.admission and not flag_is_set(88) and (reg("D1") & 0xFF) == natural.admission.battle,
                "premature battle loop")
            c.record("battle:loop", {d0=reg("D0"), d1=reg("D1"), d2=reg("D2"), d3=reg("D3"), d4=reg("D4"), f88=flag_is_set(88)})
            if victory then c.arm_battle_return("loop", nf.BattleLoop) end
        end)
        local chain = {
            {"ExecuteBeforeBattleCutscene", "admission", "before"},
            {"LoadBattle", "before", "load"},
            {"ExecuteBattleStartCutscene", "load", "start"},
            {"ActivateEnemies", "start", "activate"},
            {"ExecuteBattleRegionCutscene", "activate", "region"},
            {"PopulateTargetsListWithSpawningEnemies", "region", "spawn"},
            {"GenerateBattleTurnOrder", "spawn", "generation"},
        }
        for _, row in ipairs(chain) do
            add_callback(nf[row[1]], "candidate:battle-lifecycle", function()
                assert(completed[row[2]] and (not completed[row[3]] or victory), "battle lifecycle order/repetition drift: " .. row[1])
                if row[3] == "load" then assert(completed.beforeScript, "before program did not return") end
                if row[3] == "activate" then assert(completed.startScript and flag_is_set(451), "battle-start selection/flag drift") end
                c.record("battle:" .. row[3] .. ":before", c.accounting())
                returned("battle:" .. row[3], nf[row[1]], function()
                    if victory and row[3] == "generation" then c.roundNumber = c.roundNumber + 1 end
                    c.checkpoint(row[3])
                end)
            end)
        end
        add_callback(nf.ExecuteIndividualTurn, "candidate:first-dispatch", function()
            assert(completed.generation and (not c.firstActor or victory), "unexpected individual-turn dispatch")
            if victory then
                c.currentActor, c.turnNumber, c.battlePoll = reg("D0") & 0xFF, c.turnNumber + 1, nil
                completed.playerControl = false
                c.arm_battle_return("turn", nf.ExecuteIndividualTurn)
                c.record("battle:turn-dispatch", c.battle_observation())
            end
            c.firstActor = c.firstActor or (reg("D0") & 0xFF)
            local actor = extension_combatant(c.firstActor)
            if not victory or c.turnNumber == 1 then c.checkpoint("firstDispatch") end
            if not victory and (c.firstActor >= ram.COMBATANT_ENEMIES_START
                or (actor.statusEffects & ram.STATUSEFFECT_MUDDLE) ~= 0
                or (actor.activationBitfield & ram.AIBITFIELD_AI_CONTROLLED) ~= 0
                or byte("AUTO_BATTLE_TOGGLE") ~= 0) then
                c.stop("out-of-scope-before-player-ready", {cause="non-player-first-dispatch"})
            end
        end)
        for _, name in ipairs({"StartAiControl", "ExecuteAiControl", "battlesceneScript_ApplyActionEffect"}) do
            add_callback(nf[name], "candidate:unsupported-action", function()
                if c.epoch and not victory then c.stop("out-of-scope-before-player-ready", {cause=name}) end
            end)
        end
        for _, name in ipairs({"BattleLoop_Victory", "BattleLoop_Defeat"}) do
            add_callback(nf[name], "candidate:premature-outcome", function()
                if not c.epoch then return end
                if not victory then c.stop("premature-battle-outcome", {cause=name})
                elseif name == "BattleLoop_Defeat" then c.stop("observed-defeat", c.battle_observation())
                else
                    assert(completed.generation and not completed.victory, "victory outside the natural battle")
                    completed.victory, c.battlePoll = true, nil
                    c.record("battle:natural-victory", c.battle_observation())
                end
            end)
        end
        add_callback(nf.ProcessBattleEntityControlPlayerInput, "candidate:player-control", function()
            assert(c.firstActor and completed.generation, "player control before first dispatch")
            completed.playerControl = true
            if victory then
                c.arm_battle_return("player", nf.ProcessBattleEntityControlPlayerInput)
                c.record("battle:player-branch", c.battle_observation())
            end
        end)
        add_callback(nf.playerReadyPc, "candidate:player-ready", function()
            if c.stopReason then return end
            local state = sample()
            local offset = byte("CURRENT_BATTLE_TURN")
            local actor = memory.read_u8(ram.BATTLE_TURN_ORDER + offset, "M68K BUS")
            local mapped = memory.read_u8(ram.ENTITY_INDEX_LIST + actor, "M68K BUS")
            local area = {byte("BATTLE_AREA_X"), byte("BATTLE_AREA_Y"), byte("BATTLE_AREA_WIDTH"), byte("BATTLE_AREA_HEIGHT")}
            if victory then
                assert(completed.playerControl and c.battleReturns.player and actor == c.currentActor, "battle movement without player owner")
                c.battlePoll = {kind="battle-movement", frame=frame_count, pc=nf.playerReadyPc, turn=c.turnNumber}
                if byte("CURRENT_PLAYER_INPUT") == 0 then c.pauseBatch = true end
                c.record("battle:movement-input", {poll=c.battlePoll, input=byte("CURRENT_PLAYER_INPUT"),
                    entity=entity(actor), chosenX=byte("BATTLE_ENTITY_CHOSEN_X"), chosenY=byte("BATTLE_ENTITY_CHOSEN_Y")})
                if (byte("CURRENT_PLAYER_INPUT") & (ram.INPUT_A | ram.INPUT_B | ram.INPUT_C)) ~= 0 then
                    c.battlePoll, c.pauseBatch = nil, true
                end
                completed.ready = true
                return
            end
            assert(completed.playerControl and completed.beforeScript and completed.startScript and completed.generation
                and #c.programs == 0 and c.pending == 0, "player-ready lifecycle/consumer mismatch")
            assert(state.map == natural.admission.map and byte("CURRENT_BATTLE") == natural.admission.battle
                and flag_is_set(401) and not flag_is_set(501) and flag_is_set(451), "player-ready admission mismatch")
            for i, value in ipairs(area) do assert(value == natural.admission.area[i], "battle area mismatch") end
            assert(actor == c.firstActor and actor < ram.COMBATANT_ENEMIES_START
                and word("MOVING_BATTLE_ENTITY_INDEX") == actor and byte("VIEW_TARGET_ENTITY") == mapped,
                "player-ready actor/turn/moving/view mismatch")
            -- WINDOW_IS_PRESENT is a count: the normal battlefield mini status
            -- window increments it before ControlBattleEntity accepts movement.
            local readiness = {windowCount=state.windowState, typewriting=state.typewriting,
                dialogueWindow=word("DIALOGUE_WINDOW_INDEX"), portraitWindow=word("PORTRAIT_WINDOW_INDEX"),
                currentPlayerInput=byte("CURRENT_PLAYER_INPUT"), isTargeting=byte("IS_TARGETING"),
                currentBattleAction=word("CURRENT_BATTLEACTION"), mapEventWord=state.mapEventWord,
                fading=byte("FADING_SETTING"), effectiveScrollingPlanes=camera_state().effectiveScrollingPlanes}
            local modal = #c.programs > 0 or c.pending > 0 or readiness.typewriting ~= 0
                or readiness.dialogueWindow ~= 0 or readiness.portraitWindow ~= 0
            readiness.cutsceneOrMenuModal = modal
            -- CreatePulsatingBlocksForGrid selects PULSATING_1 (5) before player
            -- movement; its palette cycle is not a blocking screen transition.
            readiness.paletteModeAllowed = readiness.fading == 0 or readiness.fading == 5
            c.record("battle:player-ready-check", readiness)
            assert(not modal and readiness.currentPlayerInput == 0 and readiness.isTargeting == 0
                and readiness.currentBattleAction == 0 and readiness.mapEventWord == 0
                and readiness.paletteModeAllowed and readiness.effectiveScrollingPlanes == 0,
                "player-ready input/modal/transfer mismatch")
            c.checkpoint("ready")
            c.stop("player-ready", {area=area, actor=actor, movingActor=word("MOVING_BATTLE_ENTITY_INDEX"),
                viewEntity=mapped, cutsceneOrMenuModal=modal, pendingBlockingConsumers=c.pending,
                boundary="ControlBattleEntity.after-WaitForVInt-before-input-read"})
        end)
        add_callback(nf.Trap0_SoundCommand, "candidate:audio-command", function()
            if not c.epoch then return end
            local operand = memory.read_u32_be((reg("A7") & 0xFFFFFF) + 2, "M68K BUS") & 0xFFFFFF
            local command = memory.read_u16_be(operand, "M68K BUS")
            if command == 65535 then command = reg("D0") & 0xFFFF end
            local program = c.programs[#c.programs]
            c.record("audio:request", {command=command, sourcePc=operand - 2,
                program=program and program.target or false, operation=program and program.operation or false,
                disabled=byte("SOUND_COMMANDS_DEACTIVATED")})
        end)
        for _, site in ipairs(natural.soundDispatch) do
            add_callback(site.pc, "candidate:audio-dispatch", function()
                if not c.epoch then return end
                local command = site.previousMusic and byte("MUSIC_STACK") or (reg("D0") & 0xFF)
                c.record("audio:consumer-dispatch", {command=command, previous=c.activeMusic or false,
                    sourcePc=site.pc, boundary="before original Z80 mailbox write"})
                c.audioPending = {pc=site.pc, stack=reg("A7"), command=command}
            end)
            add_callback(site.pc + site.width, "candidate:audio-dispatch-return", function()
                local pending = c.audioPending
                if not pending or pending.pc ~= site.pc or pending.stack ~= reg("A7") then return end
                c.record("audio:mailbox-written", pending)
                local command = pending.command
                -- Raw last music/control dispatch; no assertion of audible output or track completion.
                if command <= 0x40 or command >= 0x80 then c.activeMusic = command end
                c.audioPending = nil
            end)
        end
        function c.natural_frame()
            if not c.epoch then return end
            local state = sample()
            if victory and completed.exploration then
                local camera = camera_state()
                local ready = completed.afterProgram and completed.afterReturn and completed.flagClear and completed.flagSet
                    and completed.battleReturn and completed.switchReturn and not next(c.battleReturns)
                    and byte("CURRENT_BATTLE") == ram.NOT_CURRENTLY_IN_BATTLE
                    and c.pending == 0 and #c.programs == 0 and not c.audioPending and not c.fieldMenu
                    and not flag_is_set(401) and flag_is_set(501) and state.mapEventWord == 0
                    and state.typewriting == 0 and word("DIALOGUE_WINDOW_INDEX") == 0 and word("PORTRAIT_WINDOW_INDEX") == 0
                    and byte("FADING_SETTING") == 0 and camera.effectiveScrollingPlanes == 0
                    and c.appliedButton == "neutral" and byte("CURRENT_PLAYER_INPUT") == 0 and byte("PLAYER_1_INPUT") == 0
                    and state.rawX == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS")
                    and state.rawY == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS")
                    and c.explorationPoll and c.explorationPoll.frame == frame_count
                    and c.consumerPoll and c.consumerPoll.kind == "WaitForEvent-action" and frame_count - c.consumerPoll.frame <= 1
                for _, count in pairs(c.consumers) do if count ~= 0 then ready = false end end
                c.explorationReady = ready
                local key = table.concat({state.map, state.rawX, state.rawY, state.facing}, ":")
                if ready and c.stableExploration and c.stableExploration.key == key and c.stableExploration.frame == frame_count - 1 then
                    local facts = {firstFrame=c.stableExploration.frame, completedFrame=frame_count,
                        inputPoll=c.explorationPoll, camera=camera, accounting=c.battle_observation(),
                        flags=read_span(ram.GAME_FLAGS, 128), player=entity(0), state=state,
                        pending=c.pending, consumers={},
                        boundary="two neutral completed exploration frames"}
                    for name, count in pairs(c.consumers) do facts.consumers[name] = count end
                    if not c.postVictoryReady then
                        c.postVictoryReady = facts
                        c.record("field:post-victory-ready", facts)
                        c.pauseBatch = true
                    elseif c.postVictoryInput and c.postVictoryInput.poll and c.postVictoryInput.acceptance
                        and state.map == c.postVictoryReady.state.map
                        and (state.rawX ~= c.postVictoryReady.state.rawX or state.rawY ~= c.postVictoryReady.state.rawY) then
                        facts.beforeInput, facts.input = c.postVictoryReady, c.postVictoryInput
                        c.stop("controllable-5b", facts)
                    end
                end
                c.stableExploration = ready and {key=key, frame=frame_count} or nil
            end
            if c.map19Captured and not completed.map19Displacement and state.map == 19 and state.x == 26 and state.y == 29 then
                c.checkpoint("map19Displacement")
            end
            if completed.guard and not completed.guardWait and state.map == 21 and state.x == 5 and state.y == 15
                and (state.facing & ram.DIRECTION_MASK) == ram.DOWN and c.appliedButton == "neutral"
                and state.mapEventWord == 0 and state.typewriting == 0 and c.pending == 0
                and state.rawX == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS")
                and state.rawY == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS")
                and c.consumerPoll and c.consumerPoll.kind == "WaitForEvent-action"
                and frame_count - c.consumerPoll.frame <= 1 then
                c.checkpoint("guardWait")
            end
            local values = {state.map, state.rawX, state.rawY, state.facing}
            for _, name in ipairs({"VIEW_PLANE_A_PIXEL_X", "VIEW_PLANE_A_PIXEL_Y", "FADING_POINTER", "FADING_COUNTER"}) do
                values[#values + 1] = word(name)
            end
            -- Only the entity currently bound to a reached operation counts, not idle NPC churn.
            local program = c.programs[#c.programs]
            if program and program.entity then
                local e = entity(program.entity)
                values[#values + 1] = memory.read_u16_be(e.address + ram.ENTITYDEF_OFFSET_X, "M68K BUS")
                values[#values + 1] = memory.read_u16_be(e.address + ram.ENTITYDEF_OFFSET_Y, "M68K BUS")
                values[#values + 1] = memory.read_u32_be(e.address + ram.ENTITYDEF_OFFSET_ACTSCRIPTADDR, "M68K BUS")
                values[#values + 1] = memory.read_u8(e.address + ram.ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER, "M68K BUS")
            end
            if (c.consumers.prompt or 0) > 0 then values[#values + 1] = byte("CURRENT_DIAMOND_MENU_CHOICE") end
            local key = table.concat(values, ":")
            if key ~= c.progressState then c.progressState, c.progressFrame = key, frame_count end
            c.progressStalled = frame_count - (c.progressFrame or c.epoch) >= acquisition.progressFrames
        end
    end

    local function admission()
        local declared, state = config.candidate.admission, sample()
        c.record("r1:first-wait-before-restoration", state)
        assert(state.map == 3 and state.rawX == declared.playerEntity.x
            and state.rawY == declared.playerEntity.y and state.facing == declared.playerEntity.facing
            and state.mapEventWord == 0, "controlled R1 map/player/event admission mismatch")
        assert(memory.read_u16_be(ram.CURRENT_GOLD, "M68K BUS") == declared.gold, "R1 gold drift")
        assert(flag_is_set(ram.FLAG_INDEX_DIFFICULTY1) == declared.difficultyFlags[1]
            and flag_is_set(ram.FLAG_INDEX_DIFFICULTY2) == declared.difficultyFlags[2], "R1 difficulty drift")
        for _, flag in ipairs({ 600, 601, 602, 603, 604, 605, 607, 608, 401, 256, 501, 507, 982 }) do
            assert(not flag_is_set(flag), "R1 source-proved route guard set: " .. flag)
        end
        local allies = {}
        local fields = {
            { "class", "COMBATANT_OFFSET_CLASS", 1 }, { "level", "COMBATANT_OFFSET_LEVEL", 1 },
            { "hpMax", "COMBATANT_OFFSET_HP_MAX", 2 }, { "hpCurrent", "COMBATANT_OFFSET_HP_CURRENT", 2 },
            { "mpMax", "COMBATANT_OFFSET_MP_MAX", 1 }, { "mpCurrent", "COMBATANT_OFFSET_MP_CURRENT", 1 },
            { "attack", "COMBATANT_OFFSET_ATT_CURRENT", 1 }, { "defense", "COMBATANT_OFFSET_DEF_CURRENT", 1 },
            { "agility", "COMBATANT_OFFSET_AGI_CURRENT", 1 }, { "move", "COMBATANT_OFFSET_MOV_CURRENT", 1 },
        }
        for _, expected in ipairs(declared.allies) do
            local base = ram.COMBATANT_DATA + expected.id * ram.COMBATANT_DATA_ENTRY_SIZE
            local statusAddress = base + ram.COMBATANT_OFFSET_STATUSEFFECTS
            local statusWord = memory.read_u16_be(statusAddress, "M68K BUS")
            assert(flag_is_set(ram.FORCEMEMBER_JOINED_FLAGS_START + expected.id) == declared.joinedFlags[expected.id + 1], "R1 joined flag drift")
            assert(flag_is_set(ram.FORCEMEMBER_ACTIVE_FLAGS_START + expected.id) == declared.activeFlags[expected.id + 1], "R1 active flag drift")
            allies[#allies + 1] = { id = expected.id, address = statusAddress, widthBytes = 2,
                statusWord = statusWord, poisonMask = ram.STATUSEFFECT_POISON,
                poison = (statusWord & ram.STATUSEFFECT_POISON) ~= 0 }
            for _, field in ipairs(fields) do
                local value = field[3] == 2 and memory.read_u16_be(base + ram[field[2]], "M68K BUS")
                    or memory.read_u8(base + ram[field[2]], "M68K BUS")
                assert(value == expected[field[1]], "R1 ally field drift: " .. expected.id .. ":" .. field[1])
            end
            -- R1's four item observations are consecutive bytes, not four words.
            for index = 0, 3 do
                assert(memory.read_u8(base + ram.COMBATANT_OFFSET_ITEM_0 + index, "M68K BUS") == expected.items[index + 1], "R1 item storage drift")
                assert(memory.read_u8(base + ram.COMBATANT_OFFSET_SPELLS + index, "M68K BUS") == expected.spells[index + 1], "R1 spell drift")
            end
        end
        local npcs = {}
        for physical = 0, ram.ENTITIES_COUNTER - 1 do
            local base = ram.ENTITY_DATA + physical * ram.ENTITYDEF_SIZE
            npcs[#npcs + 1] = { physical = physical, address = base, widthBytes = ram.ENTITYDEF_SIZE,
                bytes = read_span(base, ram.ENTITYDEF_SIZE) }
        end
        c.record("r1:inherited-status-and-live-entities", { allies = allies, entities = npcs,
            entityIndexBytes = read_span(ram.ENTITY_INDEX_LIST, 64), rawTimeNormalized = false })
        for _, patch in ipairs(config.r1.sessionPatches) do
            local ok, mismatch = restore_cart(patch)
            assert(ok, "admission service restoration mismatch: " .. patch.purpose)
            local original = {}
            for index = 1, #patch.originalHex, 2 do
                original[#original + 1] = tonumber(patch.originalHex:sub(index, index + 1), 16)
            end
            assert(not first_mismatch("restored-service-bus", patch.address, original), "service bus readback failed")
        end
        -- The CheckSram return redirect was consumed to enter the bootstrap
        -- trampoline. Original main-loop calls now own the live stack. Do not
        -- overwrite that stack with the pre-bootstrap snapshot.
        restore_span(config.r1.harness.checkpointAddress, scope.generatedRam)
        assert(not first_mismatch("bootstrap-scratch", config.r1.harness.checkpointAddress, scope.generatedRam), "bootstrap scratch not restored")
        c.epoch, phase = frame_count, "candidate-route"
        c.emulatorEpoch = emu.framecount()
        if natural then c.checkpoint("r1") end
        c.record("r1:controlled-admission-ended", { patchesRestored = true, scratchRestored = true,
            retained = "NewGame/SaveGame/default Map3 state and inherited live NPC/RNG/raw time",
            inputIdentity = config.candidate.inputIdentity })
    end
    add_callback(config.r1.functions.waitForEventAddress, "candidate:wait", function()
        if not c.epoch then
            assert(phase == "await-r1-wait", "candidate first wait bypassed controlled bootstrap")
            admission()
        elseif phase == "messenger" and zone_return_seen then
            assert(c.pending == 0, "R2a wait with pending consumer/program returns")
            follower_wait_seen = true
            capture_messenger_result()
            c.r2a, phase = true, "candidate-gate-route"
            c.record("r2a:follower-ready", { endpoint = messenger_result,
                guard138 = entity(138), guard139 = entity(139), followers = read_span(ram.FOLLOWERS_LIST, 32) })
        elseif memory.read_u8(ram.CURRENT_MAP, "M68K BUS") == 19 and not c.map19Captured then
            assert(c.gates.commit and c.gates.returned and c.gates.warp and c.gates.initReturned
                and c.gates.map19ProgramReturned and c.pending == 0, "Map19 wait lacks original gate/warp/init/consumer closure")
            assert(memory.read_u16_be(ram.MAP_EVENT_TYPE, "M68K BUS") == 0, "Map19 pending event")
            c.map19Wait = true
            c.record("map19:first-wait", { boundary = "before original controller installation" })
        end
    end)
    add_callback(config.functions.ExecuteMapScript, "candidate:script", function()
        if not c.epoch then return end
        local target, known = reg("A0") & 0xFFFFFF, false
        for _, name in ipairs(config.route.scriptSymbols) do
            if target == config.functions[name] then known = true end
        end
        if natural then
            for _, address in ipairs(natural.programs) do if target == address then known = true end end
        end
        assert(known or target == f.cs_51652 or target == f.cs_53104, "unexpected program beyond bounded route")
        if target == config.functions.cs_5149A then
            assert(c.messengerZone and not flag_is_set(603), "R2 messenger admission missing/already committed")
            for _, name in ipairs({ "afterHouseExit", "classroomSarah", "afterEntity142", "afterAstralZone" }) do
                assert(flag_is_set(config.route.flags[name]), "R2 opening guard missing: " .. name)
            end
            messenger_started, messenger_entry_seen, phase = true, true, "messenger"
            c.record("r2:messenger-before-body", { target = target })
        elseif target == f.cs_51652 then
            assert(c.r2a and c.gates.entry and not flag_is_set(604), "gate program skipped original admission")
            phase = "candidate-gate-program"
        end
        if natural then c.program_entry(target) end
        returned("script", target, function()
            if natural then c.program_return(target) end
            if target == f.cs_51652 then c.gates.programReturned = true end
            if target == f.cs_53104 then c.gates.map19ProgramReturned = true end
        end)
    end)
    add_callback(config.functions.ProcessMapEventType6_ZoneEvent, "candidate:zone-dispatch", function()
        if c.epoch then
            c.zoneTarget = { x = memory.read_u16_be(ram.MAP_EVENT_PARAM_1, "M68K BUS"),
                y = memory.read_u16_be(ram.MAP_EVENT_PARAM_3, "M68K BUS") }
            c.record("zone:original-dispatch", c.zoneTarget)
        end
    end)
    add_callback(config.functions.Map3_ZoneEvent8, "candidate:messenger-zone", function()
        local expected = config.route.endpoint.sourceTarget
        assert(c.zoneTarget and c.zoneTarget.x == expected.x and c.zoneTarget.y == expected.y
            and memory.read_u8(ram.CURRENT_MAP, "M68K BUS") == expected.map, "R2 messenger raw zone target drift")
        c.messengerZone = true
        c.record("r2:original-zone8-entry", c.zoneTarget)
    end)
    add_callback(f.Map3_ZoneEvent4, "candidate:gate-entry", function()
        assert(c.r2a and not flag_is_set(604), "gate entered outside follower-ready prefix")
        assert(c.zoneTarget and c.zoneTarget.x == config.candidate.gatePoint[1]
            and c.zoneTarget.y == config.candidate.gatePoint[2], "gate raw zone target drift")
        c.gates.entry = true
        c.record("gate:entry", { guard138 = entity(138), guard139 = entity(139) })
    end)
    add_callback(f.gateCommit, "candidate:f604-before", function()
        assert(c.gates.programReturned and c.pending == 0 and not flag_is_set(604), "F604 before original script return")
        c.gates.commit = true
        c.record("gate:f604-before-original-trap", {})
    end)
    add_callback(f.return_50E42, "candidate:gate-return", function()
        assert(c.gates.commit and flag_is_set(604), "F604 missing after original trap")
        c.gates.returned = true
        c.record("gate:f604-after-original-trap", { guard138 = entity(138), guard139 = entity(139) })
    end)
    add_callback(f.csc14_setEntityActscriptManual, "candidate:entity-actions", function()
        if phase ~= "candidate-gate-program" then return end
        local operand = reg("A6") & 0xFFFFFF
        local character = memory.read_u8(operand, "M68K BUS")
        local awaited = memory.read_u8(operand + 1, "M68K BUS") ~= 0
        assert((character == 138 and not awaited) or (character == 139 and awaited), "gate await flag drift")
        c.record("gate:entity-command-before-write", { entity = entity(character), awaited = awaited, operandPc = operand })
        c.action = { character = character, awaited = awaited }
        returned("gate:entity-actions", character, function()
            local facts = entity(character)
            facts.awaited = awaited
            if awaited then
                assert(memory.read_u32_be(facts.address + ram.ENTITYDEF_OFFSET_ACTSCRIPTADDR, "M68K BUS") == f.eas_Idle, "awaited guard not idle")
            end
            c.record("gate:entity-command-return", facts)
        end)
    end)
    for _, name in ipairs({ "loc_46966", "loc_46970" }) do
        add_callback(f[name], "candidate:entity-script-boundary", function()
            if phase ~= "candidate-gate-program" or not c.action or c.action[name] then return end
            c.action[name] = true
            c.record("gate:" .. name, { awaited = c.action.awaited,
                entity = entity(c.action.character), otherGuard = entity(c.action.character == 138 and 139 or 138) })
        end)
    end
    add_callback(config.functions.ProcessMapEventType1_Warp, "candidate:warp", function()
        if not c.epoch then return end
        local operands = read_span(ram.MAP_EVENT_PARAM_1, 5)
        local map, x, y = current_position()
        local target_x, target_y = current_destination()
        -- MAP_CURRENT retains CURRENT_MAP only on the original no-scroll path.
        -- This is an admission calculation, never a write or replacement warp.
        local effective = operands[2]
        if operands[1] == 0 and effective == config.candidate.currentMapOperand then
            effective = map
        end
        c.record("warp:original-handler", { operands = operands, currentMap = map,
            effectiveDestinationMap = effective, source = { x = x, y = y },
            target = { x = target_x, y = target_y } })
        if natural and c.gates.warp then
            local warp = natural.warps[c.nextWarp]
            assert(c.map19Captured and warp and map == warp.fromMap
                and target_x == warp.target[1] and target_y == warp.target[2]
                and x == warp.source[1] and y == warp.source[2]
                and operands[1] == warp.scrollMode and effective == warp.targetMap
                and operands[3] == warp.destination.x and operands[4] == warp.destination.y
                and operands[5] == warp.facing, "unexpected continuation warp")
            assert(c.completed.map19Displacement, "missing actual Map19 displacement")
            if c.nextWarp >= 3 then assert(c.completed.astral, "tower warp before Astral acceptance") end
            if c.nextWarp >= 5 then assert(c.completed.guardWait, "tower exit before selected guard wait") end
            c.record("natural:warp", {id=warp.id, ordinal=c.nextWarp})
            c.nextWarp = c.nextWarp + 1
            return
        end
        assert(not c.gates.warp, "warp beyond first Map19 admission")
        local function matches(warp)
            return map == warp.fromMap and effective == warp.toMap
                and operands[1] == warp.scrollMode and operands[2] == warp.eventDestinationMap
                and operands[3] == warp.destinationX and operands[4] == warp.destinationY
                and operands[5] == warp.facing
        end
        if effective == config.candidate.northWarp.to.map then
            assert(c.gates.returned and flag_is_set(604), "north warp bypassed gate")
            local warp, source = config.candidate.northWarp, config.candidate.northWarpSource
            assert(matches(config.candidate.northWarpOperands)
                and x == source[1] and y == source[2]
                and target_x == warp.from.point[1] and target_y == warp.from.point[2],
                "original north warp operands/source target drift")
            c.gates.warp = true
        else
            local admitted = false
            for _, warp in ipairs(config.candidate.prefixWarps) do
                if matches(warp) and x == warp.source.x and y == warp.source.y
                    and target_x == warp.target.x and target_y == warp.target.y then
                    admitted = true
                end
            end
            assert(admitted, "warp beyond source-bound Map3 prefix")
        end
    end)
    add_callback(f.ms_map19_InitFunction, "candidate:map19-init", function()
        assert(c.gates.warp, "Map19 init without original north warp")
        returned("map19:init", f.ms_map19_InitFunction, function() c.gates.initReturned = true end)
    end)
    for _, name in ipairs({ "DisplayText", "CloseDialogueWindow" }) do
        add_callback(f[name], "candidate:consumer", function()
            if c.epoch then returned(name, reg("D0") & 0xFFFF) end
        end)
    end
    for _, name in ipairs({ "csc00_displaySingleTextbox", "csc02_displayTextbox" }) do
        add_callback(config.functions[name], "candidate:text-command", function()
            if not c.epoch then return end
            c.record("text:original-command", { command = name, cursorPc = reg("A6") & 0xFFFFFF,
                textId = memory.read_u16_be(ram.CUTSCENE_DIALOG_INDEX, "M68K BUS"),
                packedSpeaker = memory.read_u16_be(reg("A6") & 0xFFFFFF, "M68K BUS") })
        end)
    end
    add_callback(config.functions.YesNoPrompt, "candidate:prompt", function()
        if c.epoch then returned("prompt", config.functions.YesNoPrompt) end
    end)
    add_callback(f.symbol_wait1, "candidate:text-wait", function()
        if c.epoch then c.record("text:wait1", { textCursor = memory.read_u16_be(ram.CUTSCENE_DIALOG_INDEX, "M68K BUS") }) end
    end)
    add_callback(f.FieldMenu, "candidate:unexpected-field-menu", function()
        if c.epoch then
            c.record("field-menu:reached", {})
            if not natural then error("frozen input reached FieldMenu outside the declared route") end
            assert(not c.fieldMenu, "nested FieldMenu recovery")
            c.fieldMenu, c.pauseBatch = {stage="entered", entry=frame_count}, true
            returned("FieldMenu", f.FieldMenu, function()
                if c.fieldMenu.stage == "cancel-return" then c.fieldMenu.stage = "returned"
                else c.fieldMenu.stage = "unsupported-return" end
                c.record("field-menu:original-return", c.fieldMenu)
                c.pauseBatch = true
            end)
        end
    end)
    add_callback(f.loc_65B4, "candidate:text-ack", function()
        poll("text-wait1", f.loc_65B4)
        if c.epoch and memory.read_u8(ram.CURRENT_PLAYER_INPUT, "M68K BUS") ~= 0 then
            c.record("text:acknowledgement-read", { input = memory.read_u8(ram.CURRENT_PLAYER_INPUT, "M68K BUS") })
        end
    end)
    if natural then
        add_callback(natural.functions.ExecuteDiamondMenu, "candidate:field-diamond", function()
            if not c.fieldMenu then return end
            if c.fieldMenu.stage ~= "entered" then
                c.fieldMenu.stage, c.pauseBatch = "unsupported-submenu", true
                return
            end
            c.fieldMenu.stage = "diamond"
            returned("FieldDiamond", natural.functions.ExecuteDiamondMenu, function()
                c.fieldMenu.stage = (reg("D0") & 0xFFFF) == 0xFFFF and "cancel-return" or "unsupported-choice"
                c.record("field-menu:diamond-result", {stage=c.fieldMenu.stage, choice=reg("D0") & 0xFFFF})
                c.pauseBatch = true
            end)
        end)
        add_callback(natural.functions.WaitForPlayerInput, "candidate:wait-player-input", function()
            if not c.epoch then return end
            if (c.consumers.WaitForPlayerInput or 0) == 0 then
                returned("WaitForPlayerInput", natural.functions.WaitForPlayerInput)
            end
            poll("WaitForPlayerInput", natural.functions.WaitForPlayerInput)
        end)
    end
    if acquisition then
        add_callback(f.loc_2593C, "candidate:field-action-poll", function()
            poll("WaitForEvent-action", f.loc_2593C)
        end)
        add_callback(f.loc_6472, "candidate:text-wait2-poll", function()
            poll("text-wait2-loop", f.loc_6472)
        end)
        add_callback(f.loc_1530C, "candidate:prompt-release-poll", function()
            poll("YesNoPrompt-release", f.loc_1530C)
        end)
        add_callback(f.loc_15314, "candidate:prompt-choice-poll", function()
            poll("YesNoPrompt-choice", f.loc_15314)
        end)
    end
    add_callback(config.functions.loc_52E8, "candidate:first-map19-control", function()
        if not c.epoch or (reg("A0") & 0xFFFFFF) ~= ram.ENTITY_DATA then return end
        c.record("input:original-movement-acceptance", { d2 = reg("D2"), d3 = reg("D3"), d4 = reg("D4"), d5 = reg("D5") })
        if victory and c.postVictoryInput and c.postVictoryInput.poll and not c.postVictoryInput.acceptance then
            assert(c.postVictoryInput.poll.frame == frame_count, "post-victory acceptance without same-frame input read")
            c.postVictoryInput.acceptance = {frame=frame_count, pc=reg("PC") & 0xFFFFFF,
                d2=reg("D2"), d3=reg("D3"), d4=reg("D4"), d5=reg("D5"), player=entity(0)}
            c.record("field:post-victory-input-accepted", c.postVictoryInput.acceptance)
        end
        if not c.map19Wait or c.map19Captured then return end
        assert(c.pending == 0 and flag_is_set(604), "first Map19 accepted input with pending consumer")
        assert(memory.read_u8(ram.CURRENT_MAP, "M68K BUS") == 19
            and memory.read_u16_be(ram.MAP_EVENT_TYPE, "M68K BUS") == 0
            and memory.read_u8(ram.CURRENTLY_TYPEWRITING, "M68K BUS") == 0,
            "Map19 input boundary has pending transfer/typewriting")
        if not natural then c.terminal = sample() end
        c.record("map19:first-original-movement-acceptance", { d2 = reg("D2"), d3 = reg("D3"), d4 = reg("D4"), d5 = reg("D5") })
        if natural then c.map19Captured = true; phase = "candidate-natural-route"; c.checkpoint("map19")
        else finish_pending = true end
    end)
    add_callback(config.functions.esc02_controlCharacter, "candidate:input-read", function()
        if not c.epoch or (reg("A0") & 0xFFFFFF) ~= ram.ENTITY_DATA then return end
        local d7 = reg("D7") & 0xFFFF
        local address = d7 == 0 and ram.CURRENT_PLAYER_INPUT or ram.PLAYER_1_INPUT
        local value = memory.read_u8(address, "M68K BUS")
        if victory and c.completed.exploration then
            c.explorationPoll = {frame=frame_count, pc=reg("PC") & 0xFFFFFF, address=address, value=value, d7=d7}
            if c.postVictoryInput and not c.postVictoryInput.poll and value ~= 0 then
                assert(frame_count == c.postVictoryInput.frame and c.appliedButton == c.postVictoryInput.button
                    and value == ram["INPUT_" .. c.appliedButton:upper()], "post-victory input read differs from delivered request")
                c.postVictoryInput.poll = c.explorationPoll
                c.record("field:post-victory-input-read", c.explorationPoll)
            end
        end
        if value ~= 0 or c.map19Wait then
            c.record("input:original-controller-read", { address = address, widthBytes = 1, value = value, d7 = d7 })
        end
    end)
    function c.input()
        if acquisition then return end -- fixed mechanical delivery below owns this mode
        if not c.epoch then set_messenger_input(""); return end
        local index = frame_count - c.epoch
        assert(index >= 1 and index <= #config.candidate.frames, "candidate input exhausted before Map19 control")
        local button = config.candidate.frames[index]
        set_messenger_input(button)
        if button ~= c.lastButton then
            c.record("input:frozen-frame-edge", { input = button, inputFrame = index })
            c.lastButton = button
        end
    end
    if acquisition then
        local bridge = assert(loadfile(acquisition.bridgePath))("library")
        local batch, previous_id, batches, connected = nil, 0, segment and segment.priorBatches or 0, false
        local frame_offset = segment and (segment.priorFrames - (segment.resume and segment.resume.observer.frame or 0)) or 0
        local function delivered_frames() return frame_offset + frame_count end
        local clock, launch, idle_since
        if natural then
            luanet.load_assembly("System")
            local stopwatch = assert(luanet.import_type("System.Diagnostics.Stopwatch"), "Stopwatch unavailable")
            local utc = assert(luanet.import_type("System.DateTimeOffset"), "DateTimeOffset unavailable")
            clock = stopwatch.StartNew()
            launch = tonumber(tostring(utc.UtcNow:ToUnixTimeMilliseconds())) / 1000
                - assert(tonumber(os.getenv("SF2_BRIDGE_LAUNCH_EPOCH")))
        end
        local function elapsed()
            return (segment and segment.priorActiveSeconds or 0) + launch + tonumber(clock.Elapsed.TotalSeconds)
        end
        function c.input_readiness(button)
            local state = sample()
            local result = {ready=true, button=button, unmetReasons={}, state=state}
            local function need(condition, reason)
                if not condition then result.ready=false; result.unmetReasons[#result.unmetReasons+1]=reason end
            end
            local poll = c.consumerPoll
            local fresh = poll and frame_count - poll.frame <= 1
            if candidate.diagnostic and c.heal.active and button ~= "neutral" then
                result.consumer="heal-diagnostic-neutral-scene"
                need(false, "HEAL-diagnostic-scene-input-not-admitted")
                return result
            end
            if c.fieldMenu then
                result.consumer = "FieldMenu-recovery"
                need(not c.fieldMenu.stage:match("^unsupported"), "unsupported-field-menu-state")
                need(button == "neutral" or button == "B", "field-menu-recovery-neutral-or-B-only")
                return result
            end
            if button == "neutral" then return result end
            if victory and c.completed.exploration then
                result.consumer = "post-victory-field"
                need(button == "Up" or button == "Down" or button == "Left" or button == "Right", "post-victory-direction-only")
                need(c.postVictoryReady and c.explorationReady and not c.postVictoryInput, "post-victory-ready-or-input-already-delivered")
                need(c.appliedButton == "neutral" and memory.read_u8(ram.CURRENT_PLAYER_INPUT, "M68K BUS") == 0
                    and memory.read_u8(ram.PLAYER_1_INPUT, "M68K BUS") == 0, "release-before-field-input")
                return result
            end
            if victory and c.completed.admission and not c.completed.victory
                and (c.consumers.DisplayText or 0) == 0 and (c.consumers.WaitForPlayerInput or 0) == 0 then
                local current = c.battlePoll
                result.consumer, result.poll = current and current.kind or "battle-not-ready", current or false
                need(button ~= "Start", "battle-start-unsupported")
                if current and (button == "A" or button == "C") then
                    need(c.battle_selection_supported(current.kind), "unsupported-battle-selection")
                end
                need(current and current.turn == c.turnNumber and frame_count - current.frame <= 1, "battle-poll-not-recent")
                need(c.battleReturns.player and #c.programs == 0 and not c.audioPending, "battle-player-consumer-not-active")
                local blocking = c.pending
                if current and current.kind ~= "battle-movement" then blocking = blocking - (c.consumers[current.kind] or 0) end
                if current and current.kind == "battle-spell-level" then
                    blocking = blocking - (c.consumers["battle-magic"] or 0)
                end
                need(blocking == 0 and state.typewriting == 0, "battle-blocking-consumer")
                need(memory.read_u16_be(ram.DIALOGUE_WINDOW_INDEX, "M68K BUS") == 0
                    and memory.read_u16_be(ram.PORTRAIT_WINDOW_INDEX, "M68K BUS") == 0
                    and state.mapEventWord == 0 and camera_state().effectiveScrollingPlanes == 0, "battle-modal-or-transfer")
                need(c.appliedButton == "neutral" and memory.read_u8(ram.CURRENT_PLAYER_INPUT, "M68K BUS") == 0
                    and memory.read_u8(ram.PLAYER_1_INPUT, "M68K BUS") == 0, "release-before-battle-input")
                return result
            end
            need(button ~= "A" and button ~= "B" and button ~= "Start", "input-outside-declared-route")
            if button ~= "C" then return result end
            need(c.appliedButton == "neutral" and memory.read_u8(ram.CURRENT_PLAYER_INPUT, "M68K BUS") == 0
                and memory.read_u8(ram.PLAYER_1_INPUT, "M68K BUS") == 0, "release-before-C")
            if (c.consumers.prompt or 0) > 0 then
                result.consumer = "prompt"
                need(fresh and poll.kind == "YesNoPrompt-choice", "prompt-choice-not-ready")
            elseif (c.consumers.DisplayText or 0) > 0 then
                result.consumer = "dialogue"
                need(state.typewriting == 0 and fresh
                    and (poll.kind == "text-wait1" or poll.kind == "text-wait2-loop"), "dialogue-not-ready")
            elseif (c.consumers.WaitForPlayerInput or 0) > 0 then
                result.consumer = "WaitForPlayerInput"
                need(fresh and poll.kind == "WaitForPlayerInput", "input-consumer-not-ready")
            else
                result.consumer = "entity"
                need(c.pending == 0 and #c.programs == 0 and not c.audioPending, "active-consumer-or-program")
                for kind, count in pairs(c.consumers) do need(count == 0, "active-consumer:" .. kind) end
                need(fresh and poll.kind == "WaitForEvent-action", "field-poll-not-recent")
                need(state.mapEventWord == 0 and state.typewriting == 0 and state.windowState ~= 2
                    and memory.read_u16_be(ram.DIALOGUE_WINDOW_INDEX, "M68K BUS") == 0
                    and memory.read_u16_be(ram.PORTRAIT_WINDOW_INDEX, "M68K BUS") == 0
                    and memory.read_u8(ram.FADING_SETTING, "M68K BUS") == 0, "field-modal-or-event")
                need(state.rawX == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS")
                    and state.rawY == memory.read_u16_be(ram.ENTITY_DATA + ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS")
                    and camera_state().effectiveScrollingPlanes == 0, "field-motion")
                local point
                for _, waypoint in ipairs(natural.interactions) do
                    if waypoint.map == state.map and waypoint.x == state.x and waypoint.y == state.y then point=waypoint end
                end
                need(point, "no-declared-entity-waypoint")
                if point then
                    result.waypoint = point
                    need((state.facing & ram.DIRECTION_MASK) == ram[point.facing:upper()], "wrong-player-facing")
                    local target = entity(point.entityTarget.id)
                    result.target = target
                    need(target.address, "target-not-loaded")
                    if target.address then
                        local function value(offset) return memory.read_u16_be(target.address + ram[offset], "M68K BUS") end
                        target.x, target.y = value("ENTITYDEF_OFFSET_X"), value("ENTITYDEF_OFFSET_Y")
                        target.destinationX, target.destinationY = value("ENTITYDEF_OFFSET_XDEST"), value("ENTITYDEF_OFFSET_YDEST")
                        target.facing = memory.read_u8(target.address + ram.ENTITYDEF_OFFSET_FACING, "M68K BUS") & ram.DIRECTION_MASK
                        need(point.entityTarget.map == state.map and target.x == point.entityTarget.x * ram.MAP_TILE_SIZE
                            and target.y == point.entityTarget.y * ram.MAP_TILE_SIZE, "target-position-mismatch")
                        need(target.x == target.destinationX and target.y == target.destinationY, "target-moving")
                        need(not point.entityTarget.facing or target.facing == ram[point.entityTarget.facing:upper()], "target-facing-mismatch")
                    end
                end
            end
            return result
        end
        local function budget()
            if not natural then return end
            local now = elapsed()
            if candidate.diagnostic then
                assert(now - segment.priorActiveSeconds < candidate.diagnostic.limits.activeSeconds,
                    "HEAL diagnostic attempt active-time limit")
                assert(delivered_frames() - segment.priorFrames <= candidate.diagnostic.limits.frames,
                    "HEAL diagnostic attempt frame limit")
                assert(batches - segment.priorBatches <= candidate.diagnostic.limits.batches,
                    "HEAL diagnostic attempt batch limit")
            end
            if c.stopReason then return end
            if idle_since and now - idle_since >= acquisition.idleSeconds then c.stop("operator-idle-limit") end
        end
        local function receive_timeout()
            local now = elapsed()
            local remaining = acquisition.idleSeconds
            if idle_since then remaining = math.min(remaining, acquisition.idleSeconds - now + idle_since) end
            if candidate.diagnostic then remaining=math.min(remaining,
                candidate.diagnostic.limits.activeSeconds - now + segment.priorActiveSeconds) end
            comm.socketServerSetTimeout(math.max(1, math.floor(remaining * 1000)))
        end
        local function log(value)
            c.order = c.order + 1
            value.order, value.frame = c.order, frame_count
            value.deliveredFrames = delivered_frames()
            value.emulatorFrame, value.r1Epoch = emu.framecount(), c.epoch or false
            value.r1EmulatorEpoch = c.emulatorEpoch or false
            local file = assert(io.open(acquisition.inputLogPath, "a"))
            json_write(file, value)
            file:write("\n"); file:close()
        end
        local save_readiness
        local function snapshot(readiness)
            return {boundary="frame-end", frame=frame_count, emulatorFrame=emu.framecount(),
                r1Epoch=c.epoch or false, r1EmulatorEpoch=c.emulatorEpoch or false,
                state=sample(), paused=client.ispaused(), batches=batches,
                deliveredFrames=delivered_frames(), activeSeconds=natural and elapsed() or false,
                inputReadiness=natural and c.input_readiness("C") or false,
                postVictoryInputReady=victory and c.postVictoryReady and c.explorationReady and not c.postVictoryInput or nil,
                saveReadiness=readiness or (segment and (segment.ordinal < 4 or victory) and save_readiness(segment.ordinal) or false),
                battlefield=victory and c.completed.admission and c.battle_observation(true) or nil,
                totalFrameLimit=acquisition.totalFrames, phase=phase}
        end
        -- Only these data facts survive a closed field boundary. Dynamic return
        -- closures and init-registration caches are rebuilt, never serialized.
        local continuation_keys = {"epoch", "emulatorEpoch", "order", "gates", "r2a",
            "map19Wait", "map19Captured", "nextWarp", "consumerPoll", "activeMusic",
            "progressFrame", "progressState", "consumers", "programs", "pending", "zoneTarget", "messengerZone"}
        if victory then
            for _, key in ipairs({"battleReturns", "battlePoll", "turnNumber", "roundNumber", "currentActor", "firstActor"}) do
                continuation_keys[#continuation_keys + 1] = key
            end
        end
        local function same(left, right)
            if type(left) ~= type(right) then return false end
            if type(left) ~= "table" then return left == right end
            for key, value in pairs(left) do if not same(value, right[key]) then return false end end
            for key, _ in pairs(right) do if left[key] == nil then return false end end
            return true
        end
        local function original_state()
            local bytes = {}
            for i = 0, 65535 do bytes[#bytes + 1] = string.format("%02X", memory.read_u8(i, "68K RAM")) end
            return {ramHex=table.concat(bytes), registers=emu.getregisters(), emulatorFrame=emu.framecount()}
        end
        local function core_check()
            local core = bridge.core_identity()
            assert(client.getversion() == "2.11.1" and emu.getsystemid() == "GEN"
                and core.name == "Genplus-gx" and core.type == "BizHawk.Emulation.Cores.Consoles.Sega.gpgx.GPGX",
                "segment native runtime/core mismatch")
            return core
        end
        save_readiness = function(ordinal, loading)
            assert(ordinal >= 1 and (ordinal <= 3 or victory), "no resumable boundary for this segment")
            local state = sample()
            local function word(name) return memory.read_u16_be(ram[name], "M68K BUS") end
            local function byte(name) return memory.read_u8(ram[name], "M68K BUS") end
            local camera = camera_state()
            local scrolling = camera.effectiveScrollingPlanes
            local player = {}
            for key, name in pairs({x="ENTITYDEF_OFFSET_X", y="ENTITYDEF_OFFSET_Y",
                destinationX="ENTITYDEF_OFFSET_XDEST", destinationY="ENTITYDEF_OFFSET_YDEST"}) do
                player[key] = memory.read_u16_be(ram.ENTITY_DATA + ram[name], "M68K BUS")
            end
            local raw = {camera=camera, player=player, map=state.map, x=state.x, y=state.y, facing=state.facing,
                phase=phase, r1Admitted=not not c.epoch, map19Captured=not not c.map19Captured,
                pendingReturns=c.pending, programDepth=#c.programs, audioPending=not not c.audioPending,
                activeConsumers=c.consumers, appliedButton=c.appliedButton, mapEventWord=state.mapEventWord,
                typewriting=state.typewriting, windowState=state.windowState,
                currentPlayerInput=byte("CURRENT_PLAYER_INPUT"), player1Input=byte("PLAYER_1_INPUT"),
                dialogueWindow=word("DIALOGUE_WINDOW_INDEX"), portraitWindow=word("PORTRAIT_WINDOW_INDEX"),
                fading=byte("FADING_SETTING"), fieldPoll=c.consumerPoll or false,
                completed=c.completed, nextWarp=c.nextWarp, flags=state.flags,
                deliveredFrames=delivered_frames(), batches=batches}
            local result = {eligibleSegment=ordinal, saveReady=false, unmetReasons={}, raw=raw, hardFailure=false}
            local function need(condition, reason, fatal)
                if not condition then
                    result.unmetReasons[#result.unmetReasons + 1] = reason
                    if fatal then result.hardFailure = reason end
                end
            end
            need(not callback_active and client.ispaused() and not pending_failure and not finish_pending,
                "not-paused-completed-frame", true)
            need(c.epoch and (phase == "candidate-route" or phase == "candidate-gate-route"
                or phase == "candidate-natural-route"), "route-boundary-not-reached")
            need(not c.fieldMenu, "field-menu-recovery")
            need(c.pending == 0, "pending-returns")
            need(#c.programs == 0, "active-program")
            need(not c.audioPending, "pending-audio-dispatch")
            for kind, count in pairs(c.consumers) do need(count == 0, "active-consumer:" .. kind) end
            need(c.appliedButton == "neutral" and raw.currentPlayerInput == 0 and raw.player1Input == 0,
                "input-not-neutral")
            need(state.mapEventWord == 0, "pending-map-event")
            need(state.typewriting == 0, "text-typewriting")
            local battleSave = victory and ordinal >= 4
            need((battleSave or state.windowState ~= 2) and raw.dialogueWindow == 0 and raw.portraitWindow == 0, "open-window")
            need(raw.fading == 0 or (battleSave and raw.fading == 5), "active-fade")
            need(battleSave or (player.x == player.destinationX and player.y == player.destinationY), "player-unsettled")
            need(scrolling == 0, "original-view-scrolling")
            need(battleSave or (c.consumerPoll and c.consumerPoll.kind == "WaitForEvent-action"
                and frame_count - c.consumerPoll.frame <= 1), "field-poll-not-recent")
            if ordinal == 1 then
                need(c.nextWarp <= 1 and not c.completed.royal, "segment-endpoint-passed", true)
                for _, point in ipairs(natural.checkpoints) do
                    if state.map == point.map and state.x == point.x and state.y == point.y
                        and (not point.flag or flag_is_set(point.flag))
                        and (not point.beforeFlag or not flag_is_set(point.beforeFlag))
                        and phase == point.phase and (not point.r2a or c.r2a)
                        and (point.rank ~= 5 or c.completed.map19Displacement) then
                        result.checkpoint = {name=point.id, rank=point.rank}
                    end
                end
                need(result.checkpoint, "named-checkpoint-not-reached")
            elseif ordinal == 2 then
                result.checkpoint = {name="royal-closed-field", rank=6}
                need(c.nextWarp <= 2 and not c.completed.astral, "segment-endpoint-passed", true)
                need(c.completed.royal and c.completed.royalScript and flag_is_set(605)
                    and state.map == 20 and state.x == 23 and state.y == 39
                    and (state.facing & ram.DIRECTION_MASK) == ram.DOWN
                    and c.nextWarp == 2, "royal-boundary-not-reached")
            elseif ordinal == 3 then
                result.checkpoint = {name="guard-closed-field", rank=7}
                need(c.nextWarp <= 5, "segment-endpoint-passed", true)
                need(c.completed.guardWait and c.completed.guard and flag_is_set(401) and flag_is_set(256)
                    and state.map == 21 and state.x == 5 and state.y == 15
                    and (state.facing & ram.DIRECTION_MASK) == ram.DOWN and c.nextWarp == 5,
                    "guard-boundary-not-reached")
            elseif battleSave then
                result.checkpoint = {name="battle-player-movement", rank=8 + c.turnNumber}
                raw.battleReturns, raw.battlePoll = c.battleReturns, c.battlePoll
                need(c.completed.ready and not c.completed.victory and c.completed.playerControl
                    and flag_is_set(401) and not flag_is_set(501) and byte("CURRENT_BATTLE") == natural.admission.battle
                    and state.map == natural.admission.map, "not-active-battle-player")
                need(c.battlePoll and c.battlePoll.kind == "battle-movement" and c.battlePoll.turn == c.turnNumber
                    and c.battlePoll.frame == frame_count and byte("IS_TARGETING") == 0, "not-completed-movement-poll")
                need(c.battleReturns.loop and c.battleReturns.turn and c.battleReturns.player, "missing-battle-return-descriptor")
                local targets = {loop=natural.functions.BattleLoop, turn=natural.functions.ExecuteIndividualTurn,
                    player=natural.functions.ProcessBattleEntityControlPlayerInput}
                for kind, entry in pairs(c.battleReturns) do
                    need(targets[kind] == entry.target and entry.kind == kind
                        and (memory.read_u32_be(entry.stack, "M68K BUS") & 0xFFFFFF) == entry.pc, "battle-return-readback-mismatch", true)
                end
                local actor = c.currentActor and entity(c.currentActor) or {}
                raw.actor = actor
                need(actor.address and word("MOVING_BATTLE_ENTITY_INDEX") == c.currentActor
                    and byte("VIEW_TARGET_ENTITY") == actor.physical, "battle-actor-view-mismatch")
                if actor.address then
                    need(memory.read_u16_be(actor.address + ram.ENTITYDEF_OFFSET_X, "M68K BUS") == memory.read_u16_be(actor.address + ram.ENTITYDEF_OFFSET_XDEST, "M68K BUS")
                        and memory.read_u16_be(actor.address + ram.ENTITYDEF_OFFSET_Y, "M68K BUS") == memory.read_u16_be(actor.address + ram.ENTITYDEF_OFFSET_YDEST, "M68K BUS"), "battle-actor-unsettled")
                end
            end
            if segment.resume and not loading then
                need(result.checkpoint and result.checkpoint.rank > segment.resume.checkpoint.rank
                    and frame_count > segment.resume.observer.frame, "parent-checkpoint-not-advanced")
            end
            result.saveReady = #result.unmetReasons == 0
            return result
        end
        function c.save_segment(terminal)
            assert(not candidate.diagnostic, "HEAL diagnostic cannot save a segment")
            assert(segment and not callback_active and client.ispaused(), "segment save outside host pause")
            local readiness
            if not terminal then
                readiness = save_readiness(segment.ordinal)
                assert(not readiness.hardFailure, readiness.hardFailure)
                if not readiness.saveReady then return {status="not-ready", readiness=readiness} end
            else
                assert(((not victory and segment.ordinal == 4 and c.stopReason == "player-ready")
                    or (victory and c.stopReason == "controllable-5b"))
                    and c.appliedButton == "neutral", "invalid/non-neutral final segment")
            end
            local core = core_check()
            for _, patch in ipairs(config.r1.sessionPatches) do
                for index = 1, #patch.originalHex, 2 do
                    assert(memory.read_u8(patch.address + (index - 1) // 2, "M68K BUS")
                        == tonumber(patch.originalHex:sub(index, index + 1), 16), "segment retains a bootstrap ROM patch")
                end
            end
            local path = segment.statePath
            local existing = io.open(path, "rb")
            if existing then existing:close(); error("segment state already exists") end
            assert(savestate.save(path, true) == true, "native savestate.save failed")
            local file = assert(io.open(path, "rb"), "native save produced no file")
            local size = file:seek("end"); file:close()
            assert(size and size > 0, "native save produced an empty file")
            c.record("segment:saved-frame", {ordinal=segment.ordinal, resumable=not terminal})
            local observer = {completed=c.completed, phase=phase, frame=frame_count}
            for _, key in ipairs(continuation_keys) do observer[key] = c[key] end
            local metadata = {ordinal=segment.ordinal, resumable=not terminal, observer=observer,
                checkpoint=readiness and readiness.checkpoint or {name=victory and "controllable-5b" or "player-ready", rank=victory and 9 + c.turnNumber or 8},
                original=original_state(), core=core, batches=batches, deliveredFrames=delivered_frames(), activeSecondsAtSave=elapsed(),
                stateBytes=size, boundary="neutral-completed-frame", finalCallback=terminal and c.terminal or nil}
            local output = assert(io.open(segment.metadataPath, "w"))
            json_write(output, metadata); output:write("\n"); output:close()
            if not terminal then c.stop("segment-saved", {ordinal=segment.ordinal}) end
            return {status="saved"}
        end
        if segment and segment.resume then
            local restored = segment.resume
            core_check()
            assert(savestate.load(segment.loadPath, true) == true, "native savestate.load failed")
            client.pause()
            bridge.set_button("neutral")
            assert(same(original_state(), restored.original), "loaded native state readback mismatch")
            for _, key in ipairs(continuation_keys) do c[key] = restored.observer[key] end
            for key, value in pairs(restored.observer.completed) do c.completed[key] = value end
            frame_count, phase, c.appliedButton = restored.observer.frame, restored.observer.phase, "neutral"
            if victory then c.restore_battle_returns() end
            local readiness = save_readiness(restored.ordinal, true)
            assert(same(readiness.checkpoint, restored.checkpoint), "loaded checkpoint identity mismatch")
            assert(readiness.saveReady, "loaded segment not ready: " .. table.concat(readiness.unmetReasons, ","))
            saved_state = memorysavestate.savecorestate()
            assert(saved_state ~= nil, "resume cleanup snapshot failed")
            c.record("segment:loaded-before-input", {ordinal=segment.ordinal, parent=restored.ordinal})
            if candidate.diagnostic then
                for _, binding in pairs(candidate.diagnostic.sourceBindings.instructions) do
                    assert(memory.read_u16_be(binding.pc, "M68K BUS") == tonumber(binding.hex, 16),
                        "HEAL diagnostic loaded instruction mismatch")
                end
                c.heal.loaded=true
            end
        end
        function c.capture_frame() c.frameEnd = snapshot() end
        local function reply(ok, message, terminal, save, input)
            local terminalCallback = c.terminal or false
            if victory and c.terminal then
                -- Full callback facts already live in checkpoints and observer.observed.json.
                -- Repeating combatant records here can exceed the bridge's 64 KiB frame.
                local stop = c.terminal.stop
                terminalCallback = {observationFile="observer.observed.json", stop={
                    reason=stop.reason, boundary=stop.boundary, pc=stop.pc,
                    frame=stop.frame, emulatorFrame=stop.emulatorFrame, order=stop.order}}
            end
            local result = {state=c.frameEnd or snapshot(), advanced=batch and batch.applied or 0,
                terminal=terminal or false, terminalCallback=terminalCallback,
                stopReason=natural and (c.stopReason or c.failureReason or false) or nil,
                stopBoundary="frame-end", actualInputLog="actual-inputs.jsonl", save=save, input=input}
            log({kind="result", id=previous_id, ok=ok, result=result, error=message or false})
            bridge.send({id=previous_id, ok=ok, result=result, error=message or false})
        end
        function c.close(ok, message)
            -- Snapshot was captured before restoration; it is never replaced by restored state.
            bridge.status(ok and "closed" or "failed", message or c.stopReason or "first Map19 control")
            if connected then reply(ok, message, ok) end
        end
        function c.prepare_frame()
            client.pause()
            budget()
            if finish_pending then return end
            if c.epoch then
                if not connected then
                    c.frameEnd = snapshot()
                    bridge.connect((natural and acquisition.idleSeconds or acquisition.wallSeconds) * 1000, c.frameEnd)
                    connected = true
                    if natural then idle_since = elapsed() end
                end
                if batch and batch.applied == batch.requested then
                    reply(true)
                    batch = nil
                    if natural then idle_since = elapsed() end
                end
                while not batch do
                    budget()
                    if finish_pending then c.frameEnd = snapshot(); return end
                    if natural then receive_timeout() end
                    local received, command, id = pcall(bridge.receive, previous_id)
                    budget()
                    if finish_pending then c.frameEnd = snapshot(); return end
                    if natural and not received then c.failureReason = "transport-disconnect-or-exchange" end
                    assert(received, "transport-disconnect-or-exchange: " .. tostring(command))
                    previous_id = id
                    log({kind="command", id=id, fields=command})
                    local op = command[2]
                    if natural then c.failureReason = "malformed-input" end
                    if op == "state" or op == "ping" then
                        assert(#command == 2, "wrong argument count")
                        c.failureReason = nil
                        c.frameEnd = snapshot()
                        reply(true)
                    elseif op == "abort" then
                        assert(#command == 2, "wrong argument count")
                        if natural then c.failureReason = "operator-abort" end
                        error("operator aborted interactive acquisition")
                    elseif op == "save" then
                        assert(not candidate.diagnostic, "HEAL diagnostic save is prohibited")
                        assert(#command == 2 and segment and (segment.ordinal < 4 or victory), "save requires a resumable segment")
                        c.failureReason = "segment-save-failure"
                        local outcome = c.save_segment(false)
                        c.failureReason = nil
                        c.frameEnd = snapshot(outcome.readiness)
                        if outcome.status == "not-ready" then reply(true, nil, false, outcome)
                        else return end
                    elseif op == "step" then
                        local count, button = bridge.step_arguments(command)
                        c.failureReason = nil
                        local readiness = natural and c.input_readiness(button) or {ready=true}
                        if victory and c.completed.admission and button ~= "neutral" and count ~= 1 then
                            readiness.ready = false
                            readiness.unmetReasons[#readiness.unmetReasons + 1] = "battle-continuation-input-requires-one-frame"
                        end
                        if not readiness.ready then
                            c.record("input:not-ready", {button=button, readiness=readiness})
                            c.frameEnd = snapshot()
                            reply(true, nil, false, nil, {status="not-ready", readiness=readiness})
                        else
                            if candidate.diagnostic then
                                assert(batches-segment.priorBatches < candidate.diagnostic.limits.batches,
                                    "HEAL diagnostic attempt batch limit")
                                assert(count <= candidate.diagnostic.limits.frames-delivered_frames()+segment.priorFrames,
                                    "HEAL diagnostic attempt frame limit")
                            end
                            if not natural then
                                assert(batches < acquisition.maxBatches, "input batch budget exhausted")
                                assert(count <= acquisition.totalFrames - delivered_frames(), "total frame budget exceeded")
                            end
                            batches = batches + 1
                            if victory and c.completed.exploration and button ~= "neutral" then
                                c.postVictoryInput = {button=button, frame=frame_count + 1, requestId=id, batch=batches}
                                c.record("field:post-victory-input-request", c.postVictoryInput)
                            end
                            batch = {id=id, requested=count, applied=0, button=button}
                            idle_since = nil
                        end
                    else error("unsupported acquisition command") end
                    emu.yield()
                end
            end
            c.appliedButton = batch and batch.button or (phase == "await-check-sram" and "Start" or "neutral")
            c.beforeFrame = emu.framecount()
            bridge.set_button(c.appliedButton)
            log({kind="applying", id=batch and batch.id or 0, button=c.appliedButton,
                beforeFrame=c.beforeFrame, inputFrame=c.epoch and frame_count + 1 - c.epoch or false,
                bootstrap=not c.epoch})
        end
        function c.after_frame()
            client.pause()
            if natural then c.natural_frame(); budget() end
            local after = emu.framecount()
            -- A completed-frame entry is emitted only after frameadvance returned.
            log({kind="frame", id=batch and batch.id or 0, button=c.appliedButton,
                beforeFrame=c.beforeFrame, afterFrame=after,
                inputFrame=c.epoch and frame_count - c.epoch or false,
                bootstrap=not batch})
            if batch then
                batch.applied = batch.applied + 1
                if c.pauseBatch then batch.requested = batch.applied end
            end
            c.pauseBatch = nil
            c.frameEnd = snapshot()
            bridge.set_button("neutral")
            assert(after == c.beforeFrame + 1, "interactive frame advance drift")
            assert(natural or delivered_frames() < acquisition.totalFrames or finish_pending,
                "total frame budget exhausted before terminal")
        end
    end
end

local function write_observation(restoration)
    if candidate then
        local file = assert(io.open(config.outputPath, "w"))
        json_write(file, { kind = "bounded-original-observation", terminal = assert(candidate.terminal),
            inputIdentity = config.candidate.inputIdentity, restoration = restoration,
            mode = acquisition and "interactive-acquisition" or nil,
            continuation = natural and natural.selection or nil,
            stopReason = natural and candidate.stopReason or nil,
            diagnostic = candidate.diagnostic and candidate.heal_summary() or nil,
            completedFrame = natural and candidate.frameEnd or nil,
            inputIdentityMeaning = acquisition and "mode declaration; actual inputs in actual-inputs.jsonl" or nil })
        file:write("\n"); file:close()
        return
    end
    if extension_enabled then
        local result = assert(extension_result, "player-ready result was not captured before restoration")
        local file = assert(io.open(config.outputPath, "w"))
        json_write(file, {
            system = config.fixtureId,
            caseOrder = config.caseOrder,
            records = { result },
            callbacksCleared = restoration.callbacksCleared,
            restoration = {
                gameFlags = restoration.gameFlags,
                combatantAllyRecords = restoration.combatantAllyRecords,
                mapAndBattleState = restoration.mapAndBattleState,
                playerEntity = restoration.playerEntity,
                forceAndParty = restoration.forceAndParty,
                followerState = restoration.followerState,
                touchedEntities = restoration.touchedEntities,
                dialogueAndInput = restoration.dialogueAndInput,
                cameraState = restoration.cameraState,
                bootstrapFrame = restoration.bootstrapFrame,
                gold = restoration.gold,
                generatedRam = restoration.generatedRam,
                callbacksCleared = restoration.callbacksCleared,
                sessionCartPatches = restoration.sessionCartPatches,
                sessionRomDeleted = false,
            },
        })
        file:write("\n")
        file:close()
        return
    end
    local result = assert(messenger_result, "messenger result was not captured before restoration")
    local file = assert(io.open(config.outputPath, "w"))
    file:write('{"system":"' .. config.fixtureId .. '","caseOrder":["' .. config.caseOrder[1] .. '"],"records":[{"caseId":"' .. config.caseOrder[1] .. '","r1FixtureId":"sf2-map3-admitted-start-runtime-v1","r2FixtureId":"sf2-map3-battle01-natural-route-runtime-v1","textIds":')
    append_number_array(file, text_ids)
    file:write(',"speakerOperands":')
    write_speakers(file, speaker_operands)
    file:write(',"promptReturn":0,"promptFlag89":true,"joinSelector":128,"joined":[1,2],"followers":')
    write_followers(file, follow_commands)
    file:write(',"guards":[' .. string.format('{"id":%d,"x":%d,"y":%d,"facing":%d}', result.guards[1].id, result.guards[1].x, result.guards[1].y, result.guards[1].facing) .. ',' .. string.format('{"id":%d,"x":%d,"y":%d,"facing":%d}', result.guards[2].id, result.guards[2].x, result.guards[2].y, result.guards[2].facing) .. '],"flags":' .. string.format('{"f600":%s,"f66":%s,"f603":%s}', tostring(result.flags.f600), tostring(result.flags.f66), tostring(result.flags.f603)) .. ',"endpoint":' .. string.format('{"map":%d,"x":%d,"y":%d,"facing":%d}', result.endpoint.map, result.endpoint.x, result.endpoint.y, result.endpoint.facing) .. ',"terminal":"WaitForEvent"}],"callbacksCleared":' .. tostring(restoration.callbacksCleared) .. ',"restoration":' .. string.format('{"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,"playerEntity":%s,"forceAndParty":%s,"followerState":%s,"touchedEntities":%s,"dialogueAndInput":%s,"cameraState":%s,"bootstrapFrame":%s,"gold":%s,"generatedRam":%s,"callbacksCleared":%s,"sessionCartPatches":%s,"sessionRomDeleted":false}', tostring(restoration.gameFlags), tostring(restoration.combatantAllyRecords), tostring(restoration.mapAndBattleState), tostring(restoration.playerEntity), tostring(restoration.forceAndParty), tostring(restoration.followerState), tostring(restoration.touchedEntities), tostring(restoration.dialogueAndInput), tostring(restoration.cameraState), tostring(restoration.bootstrapFrame), tostring(restoration.gold), tostring(restoration.generatedRam), tostring(restoration.callbacksCleared), tostring(restoration.sessionCartPatches)) .. '}\n')
    file:close()
end

local function finalize_success()
    if segment and (candidate.stopReason == "player-ready" or candidate.stopReason == "controllable-5b") then candidate.save_segment(true) end
    finish_pending = false
    local restoration, mismatch = restore_scope()
    if mismatch or not restoration.sessionStateRestored then
        fail("restoration", nil, mismatch and ("scoped restoration mismatch: " .. mismatch.domain) or "scoped restoration incomplete", restoration, mismatch)
        return
    end
    local cleanup_ok, cleared, cleanup_error = pcall(cleanup_callbacks)
    if not cleanup_ok then fail("callback-cleanup", nil, "callback cleanup exception: " .. tostring(cleared), restoration, nil); return end
    if not cleared then fail("callback-cleanup", nil, "callback cleanup failed: " .. tostring(cleanup_error), restoration, nil); return end
    restoration.callbacksCleared = true
    write_observation(restoration)
    status("milestone:callbacks-cleared:0")
    status("milestone:observer-finished")
    if acquisition then candidate.close(true) end
    client.exitCode(0)
end

if candidate then
    local ok, message = pcall(install_candidate)
    if not ok then fail("candidate:registration", nil, message) end
end
status("milestone:observer-started")
while true do
    if not acquisition then frame_count = frame_count + 1 end
    if pending_failure then
        finish_failure_safely()
        return
    end
    if pending_core_snapshot then
        pending_core_snapshot = false
        if candidate then
            local ok, value = pcall(memorysavestate.savecorestate)
            if not ok then fail("candidate:core-snapshot", nil, value)
            else saved_state = value end
        else saved_state = memorysavestate.savecorestate() end
        if not pending_failure then
            phase = "await-checkpoint"
            status("milestone:r1-core-state-saved-outside-callback")
        end
    end
    if finish_pending then
        local captured, capture_message
        if candidate then captured, capture_message = candidate.terminal ~= nil, "candidate terminal missing"
        elseif extension_enabled then captured, capture_message = pcall(capture_extension_result)
        else captured, capture_message = pcall(capture_messenger_result) end
        if not captured then
            fail(extension_enabled and "player-ready" or "follower-ready-wait", nil, "terminal result capture exception: " .. tostring(capture_message))
        else
            local ok, message = pcall(finalize_success)
            if not ok then fail("restoration", nil, "success finalization exception: " .. tostring(message)) end
        end
        if pending_failure then finish_failure_safely() end
        return
    end
    if acquisition then
        if pending_failure then finish_failure_safely(); return end
        local ok, message = pcall(candidate.prepare_frame)
        if not ok then fail("interactive:command", nil, message) end
        if pending_failure then finish_failure_safely(); return end
        if finish_pending and natural then candidate.capture_frame() end
        if not finish_pending then frame_count = frame_count + 1 end
    end
    if not finish_pending then
    local frame_limit = natural and config.r1.harness.bootstrapFrameBudget
        or config.r1.harness.bootstrapFrameBudget + config.cases[1].frameBudget
    if not (natural and candidate.epoch) and frame_count > frame_limit then
        fail((phase == "await-check-sram" or phase == "await-safe-core-snapshot" or phase == "await-checkpoint") and "bootstrap-watchdog" or "case-watchdog", nil, "frame budget exceeded at phase " .. phase)
    end
    enforce_route_phase_watchdog()
    if acquisition then
        -- No route policy or post-R1 memory/register writes: explicit controller only.
    elseif phase == "await-check-sram" then
        set_input("Start", "bootstrap")
    else
        local ok, message = pcall(candidate and candidate.input or route_input)
        if not ok then fail("case-watchdog", nil, message) end
    end
    if acquisition then
        if pending_failure then finish_failure_safely(); return end
        local ok, message = pcall(function()
            client.unpause()
            emu.frameadvance()
            candidate.after_frame()
        end)
        if not ok then fail("interactive:frame", nil, message) end
    else emu.frameadvance() end
    end
end
