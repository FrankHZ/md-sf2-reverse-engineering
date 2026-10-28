extends "res://probes/engine_exploration_observation.gd"

# One ordinary authored battle -> finite field removal -> usable exploration.
# Keys drive the real host; readback never injects state or successful completions.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var phases: Dictionary = {}

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    var battle := host.find_child("BattleSessionView", true, false)
    view = field if field != null and field.is_visible_in_tree() else battle
    return JSON.parse_string(view.call("ReadObservationJson"))

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := active()
    _read("initial-field-diagnostic")
    _check(initial.get("mode") == "Exploration" and initial.get("failure") == null, "ordinary authored field startup")
    if not failures.is_empty():
        _finish()
        return
    var identity: String = initial.sessionId
    _read("initial-field")
    await _press(KEY_ENTER)
    var state := active()
    _check(state.get("wait") == "DialogueWait", "ordinary interaction reaches before-battle text")
    await _press(KEY_ENTER)
    state = active()
    _check(state.get("actor") == "leader" and state.get("stage") == "Movement", "ordinary battle control")
    if not failures.is_empty():
        _finish()
        return
    _read("battle-start")
    await _press(KEY_ENTER)
    await _press(KEY_F)
    for attempt in range(4):
        await _press(KEY_TAB)
        if active().get("target") == "enemy": break
    _check(active().get("target") == "enemy", "ordinary target input selects designated gate")
    await _press(KEY_ENTER)
    var last_mark := ""
    var returned: Dictionary = {}
    while Time.get_ticks_msec() - started < 170000:
        state = active()
        if state.get("failure") != null:
            _check(false, "session failure: " + str(state.failure))
            break
        if state.has("scene"):
            var scene: Dictionary = state.scene
            if scene.get("error") != null:
                _check(false, "presenter failure: " + str(scene.error))
                break
            var mark := str(scene.phase) + ":" + str(scene.waitToken)
            if mark != last_mark:
                last_mark = mark
                _read("scene:" + mark)
                if scene.phase in ["FieldSpin", "FieldExit", "FieldSettle"]:
                    var projection: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
                    samples.append({"label":"field-death-projection", "state":projection})
                    phases[scene.phase] = true
                    _check(scene.fieldDeath.actors == ["enemy", "survivor"], "ordered field worklist contains damaged gate then living survivor")
                    var survivor: Dictionary = state.actors.filter(func(a): return a.id == "survivor")[0]
                    var gate: Dictionary = state.actors.filter(func(a): return a.id == "enemy")[0]
                    _check(gate.hp == 0 and survivor.hp == 20, "removal preserves survivor HP")
                    _check(not projection.hasBattleControl, "field delivery retains input ownership")
                    if scene.phase == "FieldSettle":
                        _check(not survivor.visible and not gate.visible and survivor.x == null and gate.x == null, "actual field presenter removes both markers")
                        var leader: Dictionary = state.actors.filter(func(a): return a.id == "leader")[0]
                        _check(leader.kills == 2 and state.gold == 84, "real cleanup credits both kills and only action gold")
                    else:
                        _check(survivor.visible and gate.visible, "both actors remain presented until exit finishes")
            if scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        else:
            if state.get("wait") == "DialogueWait":
                _read("outcome-text")
                _check(state.dialogue.contains("Victory"), "ordinary victory program is delivered")
                await _press(KEY_ENTER)
            elif state.get("stop") == "PlayerInput":
                returned = state
                break
            else:
                await process_frame
    _check(not returned.is_empty(), "bounded return to exploration")
    _check(phases.has("FieldSpin") and phases.has("FieldExit") and phases.has("FieldSettle"), "all actual field phases reached")
    if not returned.is_empty():
        _read("returned-field")
        _check(returned.sessionId == identity and returned.map == "arena", "same-session battle-to-field return")
        var leader: Dictionary = returned.party.filter(func(a): return a.Actor.Value == "leader")[0]
        _check(leader.Kills == 2 and returned.gold == 84 and 501.0 in returned.flags, "return retains cleanup accounting and completion")
        await _press(KEY_LEFT)
        for frame in range(120):
            await process_frame
            state = active()
            if state.get("stop") == "PlayerInput" and state.get("wait") == null: break
        _read("usable-field-movement")
        _check(state.entities[0].x == returned.entities[0].x - 384 and state.entities[0].y == returned.entities[0].y,
            "ordinary directional input moves the returned player one tile west")
        _check(state.gold == returned.gold and state.failure == null, "movement preserves accounting without errors")
    var ordered: Array = []
    for event in events:
        if event.Kind in ["enemy-defeated-program-none", "field-death-started", "kills", "death-cleanup", "field-death-ended", "battle-outcome", "outcome-program-started"]:
            ordered.append(event.Kind)
    _check(ordered == ["enemy-defeated-program-none", "field-death-started", "kills", "death-cleanup", "kills", "death-cleanup", "field-death-ended", "battle-outcome", "outcome-program-started"],
        "observed hook, field delivery, bookkeeping and outcome publish once in order")
    _check(events.filter(func(e): return e.Kind == "exp").size() == 1, "only the ordinary action receives EXP")
    samples.append({"label":"ordered-events", "events":events, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
