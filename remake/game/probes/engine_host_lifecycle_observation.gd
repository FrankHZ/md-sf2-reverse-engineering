extends "res://probes/engine_exploration_observation.gd"

# Bounded F1 host witnesses. Variants are authored before startup; only ordinary keys afterward.
var events: Array = []
var boundaries: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var identity := ""
var case_name := OS.get_environment("SF2_HOST_LIFECYCLE_CASE")
var seen: Dictionary = {}
var field_stages: Dictionary = {}
var spin := false
var notice := false
var battle_loads := 0

func observe_node(node: Node) -> void:
    var callback := result_observed.bind(node)
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", callback):
        node.connect("SessionResultObserved", callback)

func result_observed(json: String, node: Node) -> void:
    var result: Dictionary = JSON.parse_string(json)
    if result.boundary == "attach":
        boundaries.append({"owner":str(node.name), "mode":result.mode, "sequence":result.observationSequence})
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        if event.Kind in ["appearance-started", "appearance-completed", "round-started", "player-control", "region-program-started", "presentation-completed", "presentation-acknowledged", "battle-selected", "battle-initialized", "battle-loaded", "outcome-program-started", "battle-returned", "action-committed"]:
            events.append(event)
        last_sequence = int(event.Sequence)

func active() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    var battle := host.find_child("BattleSessionView", true, false)
    view = field if field != null and not field.is_queued_for_deletion() and field.is_visible_in_tree() else battle
    return JSON.parse_string(view.call("ReadObservationJson"))

func is_field() -> bool:
    return view.name == "ExplorationSessionView"

func sample(label: String, state: Dictionary) -> void:
    var row := {"label":label, "owner":str(view.name), "visible":view.is_visible_in_tree()}
    for key in ["sessionId", "revision", "mode", "round", "actor", "stage", "stop", "stopReason", "wait", "token", "continuation", "dialogue", "failure", "failureVisible", "status", "previewX", "previewY", "x", "y", "appearance", "story", "turnOrder", "battleMounted", "entities", "presentationWait"]:
        if state.has(key): row[key] = state[key]
    samples.append(row)

func healthy(state: Dictionary) -> bool:
    if state.get("failure") != null or state.get("scene", {}).get("error") != null:
        sample("unexpected-error", state)
        _check(false, "actual session/presenter error")
        return false
    if Time.get_ticks_msec() - started > 120000:
        _check(false, "120-second native deadline")
        return false
    _check(state.get("sessionId") == identity, "same session across handoffs")
    return true

func record_wait(state: Dictionary) -> void:
    var story: Dictionary = state.get("story", {})
    var appearance: Dictionary = state.get("appearance", {})
    var mark := str(view.name) + ":" + str(state.get("continuation")) + ":" + str(state.get("wait", story.get("kind"))) + ":" + str(state.get("token", story.get("token"))) + ":" + str(appearance.get("stage"))
    if not seen.has(mark):
        seen[mark] = true
        sample(mark, state)
    if is_field() and state.get("mode") == "Battle":
        field_stages[state.continuation] = true
        _check(state.continuation in ["BattleLoadFinished", "BattleStartFinished"], "field owns only unfinished battle entry")
        if state.wait == "DialogueWait":
            _check(not state.dialogue.is_empty(), "entry text really mounted on field")
        battle_loads = maxi(battle_loads, int(state.presentation.battleLoads))
        if state.battleMounted:
            _check(host.find_child("BattleSessionView", true, false).get_parent() == view, "prepared battle remains beneath field owner")
    if appearance.get("active", false):
        _check(not is_field() and state.actor == null and state.turnOrder.is_empty(), "battle appearance blocks control and queue generation")
        spin = spin or appearance.stage == "spin"
        notice = notice or appearance.stage == "notice" and appearance.noticeVisible and not appearance.text.is_empty()

func advance_wait(state: Dictionary) -> void:
    var story: Dictionary = state.get("story", {})
    if is_field() and state.get("wait") == "DialogueWait":
        await _press(KEY_ENTER)
    elif story.get("active", false) and story.get("kind") == "DialogueWait" and story.get("presented", false):
        _check(story.visible and not story.text.is_empty(), "battle program text really mounted")
        await _press(KEY_ENTER)
    elif state.get("scene", {}).get("phase") in ["ActionMessage", "ResultMessage", "DeathMessage", "ItemDroppedMessage", "ItemReceivedMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
        await _press(KEY_ENTER)
    else:
        await process_frame

func control() -> Dictionary:
    for frame in range(12000):
        var state := active()
        if not healthy(state): return {}
        record_wait(state)
        if not is_field() and state.get("stage") == "Movement" and state.stopReason == "PlayerInput":
            var scene: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
            if scene.movement == null:
                _check(host.find_child("ExplorationSessionView", true, false) == null, "field detached after handoff")
                return state
        await advance_wait(state)
    _check(false, "bounded control wait")
    return {}

func exercise_control() -> void:
    var before := active()
    var destination := [before.previewX, before.previewY]
    await _press(KEY_ENTER)
    _check(active().stage == "ActionChoice", "ordinary battle input reaches action menu")
    await _press(KEY_ESCAPE)
    var after := active()
    _check(after.stage == "Movement" and [after.previewX, after.previewY] == destination, "ordinary cancel restores usable movement")
    sample("usable-battle-control", after)

func return_to_field() -> Dictionary:
    for frame in range(12000):
        var state := active()
        if not healthy(state): return {}
        record_wait(state)
        if is_field() and state.get("mode") == "Exploration" and state.stop == "PlayerInput" and state.wait == null:
            return state
        await advance_wait(state)
    _check(false, "bounded return wait")
    return {}

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := active()
    identity = str(initial.get("sessionId"))
    sample("initial", initial)
    if case_name == "battle-failure":
        _check(not is_field() and view.is_visible_in_tree(), "initial failed battle has battle owner")
        _check(initial.failure == "battle-program-instruction" and initial.status.contains("battle-program-instruction"), "reached engine error displayed")
        var revision = initial.revision
        await _press(KEY_ENTER)
        await _press(KEY_RIGHT)
        _check(active().revision == revision, "faulted session cannot progress")
    elif case_name == "terminal":
        _check(not is_field() and initial.stopReason == "BattleEnded" and initial.status == "Battle ended: Defeat", "initial terminal outcome displayed")
        var revision = initial.revision
        await _press(KEY_ENTER)
        _check(active().revision == revision, "terminal input cannot restart battle")
    else:
        if not healthy(initial):
            _finish()
            return
        if case_name in ["initial-appearance", "initial-program"]:
            _check(not is_field() and initial.round == 0 and initial.turnOrder.is_empty(), "initial pending round has battle owner before queue")
        if case_name == "initial-appearance":
            _check(initial.appearance.active and initial.appearance.list == ["returner", "hidden"], "initial ordered appearance list")
            var token = initial.appearance.token
            await _press(KEY_ENTER)
            await _press(KEY_RIGHT)
            _check(active().appearance.token == token and active().stage == null, "pending appearance rejects ordinary input without skipping token")
        if case_name == "initial-program":
            _check(initial.story.active and initial.actor == null, "initial region program mounted before player control")
        if case_name == "connected-appearance":
            _check(is_field() and initial.mode == "Exploration", "connected package starts at ordinary field input")
            await _press(KEY_ENTER)
        if case_name == "initial-prebattle":
            _check(is_field() and initial.mode == "Battle" and initial.continuation == "BattleLoadFinished", "initial ActiveBattle load still belongs to field")
        var ready := await control()
        if ready.is_empty():
            _finish()
            return
        sample("first-control", ready)
        await exercise_control()
        if case_name in ["connected-appearance", "initial-prebattle"]:
            _check(field_stages.has("BattleLoadFinished") and field_stages.has("BattleStartFinished"), "load and start delivery remain field-owned")
            _check(battle_loads == 1, "actual BattleLoad mounts once before field relinquishes entry")
        if case_name in ["initial-appearance", "connected-appearance", "initial-prebattle"]:
            var completed := events.filter(func(e): return e.Kind == "appearance-completed")
            var expected: Array = ["returner", "hidden"] if case_name == "initial-appearance" else ["survivor"]
            _check(completed.map(func(e): return e.Actor.Value) == expected, "exactly-once ordered appearance delivery")
            var rounds := events.filter(func(e): return e.Kind == "round-started")
            _check(rounds.size() == 1 and rounds[0].Sequence > completed[-1].Sequence, "queue generated once after appearance completion")
            _check(spin and notice, "actual finite appearance phases visible")
        if case_name == "initial-program":
            _check(events.filter(func(e): return e.Kind == "region-program-started").size() == 1, "initial program runs exactly once")
            _check(events.filter(func(e): return e.Kind == "presentation-acknowledged").size() == 2, "both real program texts acknowledged once")
        if case_name == "ordinary-control":
            await _press(KEY_ENTER)
            await _press(KEY_SPACE)
            await _press(KEY_ENTER)
            ready = await control()
            _check(not ready.is_empty() and ready.round == 2, "automatic actors and next player turn remain usable")
        if case_name == "direct-return":
            await _press(KEY_ENTER)
            await _press(KEY_F)
            for attempt in range(4):
                await _press(KEY_TAB)
                if active().get("target") == "enemy": break
            _check(active().get("target") == "enemy", "ordinary attack selects real enemy")
            await _press(KEY_ENTER)
            var returned := await return_to_field()
            if not returned.is_empty():
                sample("usable-return", returned)
                var position: Array = returned.entities.map(func(e): return [e.id, e.x, e.y])
                await _press(KEY_RIGHT)
                var moved := active()
                sample("field-moved", moved)
                _check(is_field() and moved.mode == "Exploration" and moved.failure == null and moved.entities.map(func(e): return [e.id, e.x, e.y]) != position, "same-session field movement usable after direct battle victory")
                _check(events.filter(func(e): return e.Kind == "outcome-program-started" and e.Detail == "Victory").size() == 1, "one ordinary victory")
                _check(events.filter(func(e): return e.Kind == "battle-returned").size() == 1, "one completed field return")
                _check(boundaries.filter(func(b): return b.owner == "ExplorationSessionView").size() == 1, "one return view attached")
                _check(host.find_children("ExplorationSessionView", "", true, false).size() == 1, "no duplicate field view")
    samples.append({"label":"delivery-summary", "events":events, "attachments":boundaries,
        "fieldStages":field_stages.keys(), "battleLoads":battle_loads, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
