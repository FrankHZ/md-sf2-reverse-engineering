extends SceneTree

var samples: Array = []
var failures: Array = []
var host: Node
var view: Node
var case_name := OS.get_environment("SF2_PRIVATE_EXPLORATION_CASE")

func _initialize() -> void:
    call_deferred("_run")

func _check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func _state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        view = host.get_node_or_null("BattleSessionView")
    return JSON.parse_string(view.call("ReadObservationJson"))

func _read(label: String) -> Dictionary:
    var state := _state()
    samples.append({"label": label, "state": state})
    return state

func _entity(state: Dictionary, identity: String) -> Dictionary:
    for entity in state.entities:
        if entity.id == identity:
            return entity
    return {}

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _run() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var initial := _read("controlled-source-start")
    if not initial.has("mode"):
        _check(false, "Private ordinary exploration startup succeeds")
        _finish()
        return
    var identity: String = initial.sessionId
    if case_name == "authored-presentation":
        await _press(KEY_ENTER)
        var pending := _read("unavailable-presentation")
        _check(pending.failureKind == "AdapterError" and pending.wait == "PresentationWait", "Unavailable rendering reports an adapter failure with the original wait pending")
        await _press(KEY_ENTER)
        var retained := _read("acknowledgement-not-fabricated")
        _check(retained.revision == pending.revision and retained.token == pending.token and retained.flags == [7.0], "Actual Enter cannot acknowledge an unperformed presentation or publish its later flag")
        _finish()
        return
    if case_name.begins_with("map40"):
        var file := FileAccess.open(OS.get_environment("SF2_PRIVATE_EXPLORATION_PLAN"), FileAccess.READ)
        var fixture: Dictionary = JSON.parse_string(file.get_as_text())
        file.close()
        for row in fixture.static.inputPlan:
            if row.from.map != 40:
                continue
            var key: Key = {"Up": KEY_UP, "Down": KEY_DOWN, "Left": KEY_LEFT, "Right": KEY_RIGHT}[row.input]
            await _press(key)
            for _frame in range(60):
                var state := _state()
                if state.get("mode", "Battle") == "Battle" or state.stop != "SimulationWait":
                    break
                await process_frame
            _read("source-route-input")
    else:
        var dialogue_count := 0
        for _frame in range(2200):
            var state := _state()
            if state.stop in ["PlayerInput", "Unsupported", "Faulted"]:
                break
            if state.wait == "DialogueWait":
                dialogue_count += 1
                _read("source-dialogue-request")
                await _press(KEY_ENTER)
            else:
                await process_frame
        _check(dialogue_count == (6 if case_name == "map3-gate" else 4 if case_name == "map3-messenger" else 0), "Expected source dialogue requests were acknowledged by real input")
    var terminal := _read("terminal")
    _check(terminal.sessionId == identity, "The same common session owns every transition")
    match case_name:
        "map3-sarah":
            var sarah := _entity(terminal, "entity-1")
            _check(terminal.stop == "PlayerInput" and sarah.x == 41 * 384 and sarah.y == 7 * 384, "Source Sarah actions traverse to the declared endpoint")
            _check(terminal.simulationTick == 25, "Native fixed ticks match the engine movement projection")
        "map3-gate":
            _check(terminal.stop == "PlayerInput", "The complete source gate program returns control")
            _check(_entity(terminal, "entity-138").x == 27 * 384 and _entity(terminal, "entity-139").x == 31 * 384, "Both guards return through action traversal")
        "map3-messenger":
            _check(terminal.failure == "program-opcode" and terminal.failureField.ends_with("cs_5149A[39]:nod"), "Messenger stops at the actual unimplemented gesture")
            _check(_entity(terminal, "entity-143").speedX == 48 and not 600.0 in terminal.flags, "Prior speed change survives; later acceptance flag is absent")
        "map3-sprite-init":
            _check(terminal.failure == "entity-sprite-refresh" and terminal.spriteSize == 24, "Global size is committed before the unsupported sprite refresh")
        "map21-guard":
            _check(terminal.failure == "program-entity" and not 401.0 in terminal.flags, "Unknown entity135 prevents the later unlock")
            _check(_entity(terminal, "entity-128").x == 6 * 384, "Completed guard motion survives the later failure")
        "map40-intro":
            _check(terminal.map == "map-57" and terminal.failureField.ends_with("bbcs_01[1]:loadMapFadeIn"), "Source warp reaches the actual before-battle native frontier")
            _check(not 451.0 in terminal.flags, "The unfinished before program does not publish the intro flag")
        "map40-seen":
            _check(terminal.round == 1 and terminal.stage == "Movement" and terminal.failure == null, "Explicit seen-intro content reaches ordinary first player control")
            _check(terminal.storyFlags == [401.0, 451.0], "Original routing flags survive battle entry")
        _:
            _check(false, "Unknown external reference case")
    _finish()

func _finish() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file == null:
        push_error("External observer requires a writable output path")
        quit(2)
        return
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
