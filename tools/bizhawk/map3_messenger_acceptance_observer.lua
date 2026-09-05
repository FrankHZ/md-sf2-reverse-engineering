local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local extension_enabled = config.extension ~= nil
local OWNER = extension_enabled and config.extension.owner or "map3-messenger-acceptance"

-- R2a consumes the accepted controlled R1/R2 setup, then continues through
-- the original messenger body only.  It never advances into the Castle route.
local phase, frame_count, route_index = "await-check-sram", 0, 1
local active, scope, saved_state = nil, nil, nil
local callbacks, callback_order = {}, {}
local pending_core_snapshot, pending_failure, finish_pending = false, nil, false
local last_input, input_trace, chronology, map_transitions, script_trace = nil, {}, {}, {}, {}
local route_started, initial_wait_seen, route_control_ready, wait_after_warp = false, false, false, false
local append_trace
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
    pending_failure = { role = role, expectedPc = expected_pc, actualPc = reg("PC") & 0xFFFFFF,
        phase = phase, message = tostring(message), restoration = restoration, mismatch = mismatch }
end

local function write_failure(restoration, mismatch, cleared, output_removed)
    local p = pending_failure
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
    client.exitCode(config.observerFailureContract.exitCode)
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
    assert(callbacks[address] == nil, "more than one callback registered at physical PC " .. string.format("%X", address))
    callbacks[address] = { role = role, id = event.on_bus_exec(function()
        if pending_failure then return end
        if route_started or extension_progress_frame then
            last_callback_role, last_callback_pc = role, address
        end
        local ok, message = pcall(handler)
        if not ok then fail(role, address, message) end
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

local function json_write(file, value)
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

local function write_observation(restoration)
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
    client.exitCode(0)
end

status("milestone:observer-started")
while true do
    frame_count = frame_count + 1
    if pending_failure then finalize_failure(); return end
    if pending_core_snapshot then
        pending_core_snapshot = false
        saved_state = memorysavestate.savecorestate()
        phase = "await-checkpoint"
        status("milestone:r1-core-state-saved-outside-callback")
    end
    if finish_pending then
        local captured, capture_message
        if extension_enabled then captured, capture_message = pcall(capture_extension_result)
        else captured, capture_message = pcall(capture_messenger_result) end
        if not captured then
            fail(extension_enabled and "player-ready" or "follower-ready-wait", nil, "terminal result capture exception: " .. tostring(capture_message))
        else
            local ok, message = pcall(finalize_success)
            if not ok then fail("restoration", nil, "success finalization exception: " .. tostring(message)) end
        end
        if pending_failure then finalize_failure() end
        return
    end
    if frame_count > config.r1.harness.bootstrapFrameBudget + config.cases[1].frameBudget then
        fail((phase == "await-check-sram" or phase == "await-safe-core-snapshot" or phase == "await-checkpoint") and "bootstrap-watchdog" or "case-watchdog", nil, "frame budget exceeded at phase " .. phase)
    end
    enforce_route_phase_watchdog()
    if phase == "await-check-sram" then
        set_input("Start", "bootstrap")
    else
        local ok, message = pcall(route_input)
        if not ok then fail("case-watchdog", nil, message) end
    end
    emu.frameadvance()
end
