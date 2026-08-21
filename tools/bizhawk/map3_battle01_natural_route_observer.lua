local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))

-- R2 begins at the accepted controlled R1 boundary, then supplies only player
-- controller input through the opening Map 3 messenger-program admission. It
-- finalizes at original cs_5149A entry, before that program's body mutates F603.
local phase, frame_count, route_index = "await-check-sram", 0, 1
local active, scope, saved_state = nil, nil, nil
local callbacks, callback_order = {}, {}
local pending_core_snapshot, pending_failure, finish_pending = false, nil, false
local last_input, input_trace, chronology, map_transitions, script_trace = nil, {}, {}, {}, {}
local route_started, initial_wait_seen, route_control_ready, wait_after_warp = false, false, false, false
local append_trace
local route_stall_key, route_stall_frames = nil, 0
local route_progress_frame = nil
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
-- Keep every source-plan transition and original interaction well inside the
-- host-side 300-second launch timeout. This is a harness watchdog only, not
-- an original timing assertion.
local ROUTE_MOVE_STALL_FRAME_LIMIT = 120
-- This progress watchdog is deliberately independent of host timeout.  It is
-- reset only by an accepted route transition/event, never by repeated input
-- controller callbacks, so an unproductive loop closes through typed cleanup.
local ROUTE_PHASE_WATCHDOG_FRAME_LIMIT = 1200

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function reg(name) return emu.getregister("M68K " .. name) & 0xFFFFFFFF end

local function read_span(address, length)
    local values = {}
    for offset = 0, length - 1 do values[#values + 1] = memory.read_u8(address + offset, "M68K BUS") end
    return values
end

local function restore_span(address, expected)
    for offset, value in ipairs(expected) do memory.write_u8(address + offset - 1, value, "M68K BUS") end
end

local function first_mismatch(domain, address, expected)
    for offset, value in ipairs(expected) do
        local actual = memory.read_u8(address + offset - 1, "M68K BUS")
        if actual ~= value then return { domain = domain, address = address + offset - 1, expected = value, actual = actual } end
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
        playerEntity = false, gold = false, generatedRam = false, sessionCartPatches = false,
        sessionStateRestored = false, callbacksCleared = false, outputRemoved = false }
end

local function restore_scope()
    local restoration = blank_restoration(scope ~= nil and saved_state ~= nil)
    if not restoration.scopeArmed then return restoration, {domain="scope",address=config.r1.harness.checkpointAddress,expected=1,actual=0} end
    local loaded = pcall(function()
        memorysavestate.loadcorestate(saved_state)
        restore_span(config.r1.harness.checkpointAddress, scope.generatedRam)
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
        {"gold", config.ram.CURRENT_GOLD, scope.gold},
        {"generatedRam", config.r1.harness.checkpointAddress, scope.generatedRam},
    }
    for _, check in ipairs(checks) do
        local mismatch = first_mismatch(check[1], check[2], check[3])
        if mismatch then return restoration, mismatch end
        restoration[check[1]] = true
    end
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
    file:write(string.format('failure:observer-callback:{"owner":"%s","caseId":"%s","phase":"%s","role":"%s","actualPc":%d,"expectedPc":%s,"callbackCount":%d,"callbacksCleared":%s,"outputRemoved":%s,"restoration":{"scopeArmed":%s,"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,"playerEntity":%s,"gold":%s,"generatedRam":%s,"sessionCartPatches":%s,"sessionStateRestored":%s,"callbacksCleared":%s,"outputRemoved":%s},"restorationMismatch":%s,"error":"%s"}\n',
        OWNER or "map3-battle01-natural-route", active and active.caseId or "bootstrap", json_escape(p.phase), json_escape(p.role), p.actualPc,
        p.expectedPc and tostring(p.expectedPc) or "null", #callback_order, tostring(cleared), tostring(output_removed),
        tostring(restoration.scopeArmed), tostring(restoration.gameFlags), tostring(restoration.combatantAllyRecords), tostring(restoration.mapAndBattleState),
        tostring(restoration.playerEntity), tostring(restoration.gold), tostring(restoration.generatedRam), tostring(restoration.sessionCartPatches),
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

local function enforce_route_phase_watchdog()
    if not route_started or finish_pending or pending_failure then return end
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

local function route_input()
    if not route_started or finish_pending then set_input("", "idle"); return end
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
        if route_started then last_callback_role, last_callback_pc = role, address end
        local ok, message = pcall(handler)
        if not ok then fail(role, address, message) end
    end, address, "sf2-map3-battle01-natural-route-" .. role, "M68K BUS") }
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

-- "bootstrap-check-sram"
add_callback(config.r1.functions.checkSramAddress, "bootstrap-check-sram", function()
    if phase ~= "await-check-sram" then return end
    local ally_bytes = (config.ram.COMBATANT_ALLIES_COUNTER + 1) * config.ram.COMBATANT_DATA_ENTRY_SIZE
    scope = { gameFlags = read_span(config.ram.GAME_FLAGS, (config.ram.LONGWORD_GAMEFLAGS_COUNTER + 1) * 4),
        combatantAllyRecords = read_span(config.ram.COMBATANT_DATA, ally_bytes), mapAndBattleState = read_span(config.ram.CURRENT_MAP, 10),
        playerEntity = read_span(config.ram.ENTITY_DATA, config.ram.ENTITYDEF_SIZE), gold = read_span(config.ram.CURRENT_GOLD, 2),
        generatedRam = read_span(config.r1.harness.checkpointAddress, 64) }
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
    if phase == "await-r1-main-loop" then phase = "await-r1-exploration"; append_trace("r1", "main-loop") end
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

-- "r1-wait-for-event"
add_callback(config.r1.functions.waitForEventAddress, "r1-wait-for-event", function()
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

-- "map-script"
add_callback(config.functions.ExecuteMapScript, "map-script", function()
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
                finish_pending = true
                status("milestone:natural-map3-messenger-program-entry-observed")
            end
            return
        end
    end
    error("unexpected map script target on natural route: " .. string.format("%X", target))
end)

-- "warp"
add_callback(config.functions.ProcessMapEventType1_Warp, "warp", function()
    if route_started then
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

local function write_observation(restoration)
    local file = assert(io.open(config.outputPath, "w"))
    local opening = assert(active.openingMap3, "opening Map 3 endpoint missing before observation")
    file:write('{"system":"' .. config.fixtureId .. '","caseOrder":["' .. config.caseOrder[1] .. '"],"records":[{"caseId":"' .. config.caseOrder[1] .. '","r1Start":{"fixtureId":"sf2-map3-admitted-start-runtime-v1","map":3},"chronology":[')
    for index, value in ipairs(chronology) do if index > 1 then file:write(",") end file:write('"' .. json_escape(value) .. '"') end
    file:write('],"logicalInputTrace":[')
    for index, row in ipairs(input_trace) do
        if index > 1 then file:write(",") end
        file:write(string.format('{"map":%d,"x":%d,"y":%d,"input":"%s","waypoint":"%s"}', row.map, row.x, row.y, json_escape(row.input), json_escape(row.waypoint)))
    end
    file:write('],"mapTransitions":[')
    for index, row in ipairs(map_transitions) do if index > 1 then file:write(",") end file:write(string.format('{"map":%d,"x":%d,"y":%d,"facing":%d}', row.map, row.x, row.y, row.facing)) end
    file:write('],"scriptTrace":[')
    for index, value in ipairs(script_trace) do if index > 1 then file:write(",") end file:write('"' .. json_escape(value) .. '"') end
    file:write('],"fieldMenu":"not-reached","openingMap3":' .. string.format('{"sourceTarget":{"map":%d,"x":%d,"y":%d},"program":"%s","afterHouseExit":%s,"classroomSarah":%s,"afterEntity142":%s,"afterAstralZone":%s,"afterMessenger":%s}', opening.sourceTarget.map, opening.sourceTarget.x, opening.sourceTarget.y, json_escape(opening.program), tostring(opening.afterHouseExit), tostring(opening.classroomSarah), tostring(opening.afterEntity142), tostring(opening.afterAstralZone), tostring(opening.afterMessenger)) .. '}],"callbacksCleared":' .. tostring(restoration.callbacksCleared) .. ',"restoration":' .. string.format('{"gameFlags":%s,"combatantAllyRecords":%s,"mapAndBattleState":%s,"playerEntity":%s,"gold":%s,"generatedRam":%s,"callbacksCleared":%s,"sessionCartPatches":%s,"sessionRomDeleted":false}', tostring(restoration.gameFlags), tostring(restoration.combatantAllyRecords), tostring(restoration.mapAndBattleState), tostring(restoration.playerEntity), tostring(restoration.gold), tostring(restoration.generatedRam), tostring(restoration.callbacksCleared), tostring(restoration.sessionCartPatches)) .. '}\n')
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
    if finish_pending then finalize_success(); if pending_failure then finalize_failure() end; return end
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
