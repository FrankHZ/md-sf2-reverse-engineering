extends "res://probes/engine_exploration_observation.gd"

# Expectations belong only to this external observer. Ordinary startup/input supplies all state.
var facts: Array = []
var controls: Array = []
var last_sequence := -1
var deadline: int

func _state() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func _entity(state: Dictionary, id: String) -> Dictionary:
    for entity in state.get("entities", []):
        if entity.id == id:
            return entity
    return {}

func _result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            facts.append(event)
            last_sequence = int(event.Sequence)
    controls.append_array(result.get("programControlReads", []))

func _wait_for(kind: String) -> Dictionary:
    while Time.get_ticks_msec() < deadline:
        var state := _state()
        if state.get("failure") != null or state.get("wait") == kind:
            return state
        await process_frame
    return {}

func _timeout() -> void:
    _check(false, "Story observation exceeded its110-second deadline")
    _finish()

func _run() -> void:
    deadline = Time.get_ticks_msec() + 110000
    create_timer(110.0).timeout.connect(_timeout)
    var variant := OS.get_environment("SF2_OBSERVATION_EXPECT_STORY")
    _check(variant in ["a", "b"], "External A/B expectations are explicitly selected")
    if not failures.is_empty():
        _finish()
        return
    var flag := 41 if variant == "a" else 51
    var first := Vector2i(4, 2) if variant == "a" else Vector2i(3, 4)
    var again := Vector2i(3, 2) if variant == "a" else Vector2i(4, 4)
    var text := "The lantern" if variant == "a" else "The banner"
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        _check(false, "Ordinary host creates the exploration view")
        _finish()
        return
    if not view.is_connected("SessionResultObserved", _result):
        view.connect("SessionResultObserved", _result)
    var initial := _read("field-input")
    _check(initial.failure == null and initial.stop == "PlayerInput" and initial.flags.is_empty(), "Initial ordinary field control")
    var identity: String = initial.sessionId
    for branch in range(2):
        var pre := flag + branch * 2
        var called := ("first-" if branch == 0 else "again-") + variant
        var caller := ("invitation" if branch == 0 else "repeat") if variant == "a" else ("first-visit-b" if branch == 0 else "invitation")
        var return_pc := (2 if branch == 0 else 1) if variant == "a" else (1 if branch == 0 else 2)
        await _press(KEY_ENTER)
        var dialogue := await _wait_for("DialogueWait")
        if dialogue.is_empty() or dialogue.get("failure") != null:
            _check(false, "Real interaction reaches the nested dialogue")
            break
        _read("branch%d:dialogue" % branch)
        _check(dialogue.sessionId == identity and dialogue.dialogue.begins_with(text), "Same session projects the called program's actual text")
        _check(float(pre) in dialogue.flags and not float(pre + 1) in dialogue.flags and (40.0 in dialogue.flags) == (branch == 1), "Branch effects precede later flag effects")
        _check(dialogue.cursor.Program == "pause-" + variant and dialogue.cursor.Instruction == 1, "Dialogue retains its actual PC")
        var callers: Array = dialogue.callers
        _check(callers.size() == 2 and callers[0].Program == caller and
            int(callers[0].Instruction) == return_pc and callers[1].Program == called and
            int(callers[1].Instruction) == 3, "Two nested call return addresses remain live")
        var marker := _entity(dialogue, "marker")
        var before := Vector2i(4, 3) if branch == 0 else first
        _check(marker.Visible == (branch == 1) and marker.x == before.x * 384 and marker.y == before.y * 384,
            "Visibility changes before presentation; position stays before the wait")
        await _press(KEY_ENTER)
        var ticks := await _wait_for("TickWait")
        if ticks.is_empty() or ticks.get("failure") != null:
            _check(false, "Acknowledgement reaches the real tick wait")
            break
        _read("branch%d:tick-wait" % branch)
        _check(not float(pre + 1) in ticks.flags and ticks.cursor.Program == "pause-" + variant and ticks.cursor.Instruction == 2,
            "Acknowledgement cannot run effects after the timer")
        _check(ticks.callers == callers, "Tick wait retains both nested callers")
        var start_tick: int = dialogue.simulationTick
        var settled: Dictionary = {}
        var reached_end := false
        while Time.get_ticks_msec() < deadline:
            var state := _state()
            if state.get("failure") != null:
                _check(false, "Actual story consumer failure: " + str(state.failure))
                break
            if state.wait == "TickWait":
                _check(not float(pre + 1) in state.flags and _entity(state, "marker").x == before.x * 384,
                    "No later effect executes while the actual timer is pending")
            if state.stop == "PlayerInput" and state.wait == null and state.cursor == null:
                settled = state
                reached_end = true
                break
            await process_frame
        _check(reached_end, "Nested returns restore actual field control")
        if not reached_end:
            break
        _read("branch%d:returned" % branch)
        var target := first if branch == 0 else again
        marker = _entity(settled, "marker")
        _check(settled.simulationTick >= start_tick + 30 and float(pre + 1) in settled.flags and (40.0 in settled.flags) == (branch == 0),
            "Thirty real services precede the post-wait flag and next branch selection")
        _check(marker.x == target.x * 384 and marker.y == target.y * 384 and marker.Visible == (branch == 0),
            "Called A/B program supplies the expected entity effect")
        _check(settled.callers.is_empty() and settled.sessionId == identity, "All nested callers return in the same session")
        await process_frame
        var stable := _read("branch%d:stable" % branch)
        _check(stable.revision == settled.revision and stable.flags == settled.flags, "Idle returned control applies no duplicate effects")
    if failures.is_empty():
        var before_move := _state()
        await _press(KEY_LEFT)
        var moved: Dictionary = {}
        while Time.get_ticks_msec() < deadline:
            moved = _state()
            if moved.failure != null or moved.stop == "PlayerInput" and moved.wait == null:
                break
            await process_frame
        _read("returned-moved")
        _check(moved.failure == null and moved.sessionId == identity and moved.flags == before_move.flags,
            "Real directional input keeps story and session authority")
        _check(_entity(moved, "traveler").x == _entity(before_move, "traveler").x - 384 and
            _entity(moved, "traveler").y == _entity(before_move, "traveler").y, "Player actually moves one tile west after both branches")
        var operations: Array = []
        for control in controls:
            operations.append(control.Operation)
        _check(operations == ["CallProgram", "CallProgram", "ReturnProgram", "ReturnProgram", "EndProgram",
            "CallProgram", "CallProgram", "ReturnProgram", "ReturnProgram", "EndProgram"], "Observed call/return order executes exactly once per branch")
    samples.append({"label": "ordered-program-results", "events": facts, "controls": controls})
    _finish()
