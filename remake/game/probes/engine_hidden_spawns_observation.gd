extends "res://probes/engine_exploration_observation.gd"

# Ordinary host keys after authored startup; read actual nodes/state/errors only.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var seen: Dictionary = {}
var stages: Dictionary = {}
var intermediate_spin := false
var settled_marker := false

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            if event.Kind in ["spawn-reset", "spawn-refused", "spawn-list-ready", "appearance-started", "appearance-completed", "round-started", "death-cleanup", "player-control"]:
                events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, state: Dictionary) -> void:
    samples.append({"label":label, "revision":state.revision, "round":state.round,
        "actor":state.actor, "actors":state.actors, "appearance":state.appearance,
        "scenePhase":state.scene.phase, "queue":state.turnOrder, "failure":state.failure})

func settle() -> Dictionary:
    for frame in range(10000):
        await process_frame
        var state := active()
        if state.failure != null or state.scene.error != null:
            sample("error", state)
            _check(false, "session/presenter error")
            return {}
        if Time.get_ticks_msec() - started > 170000:
            _check(false, "native deadline")
            return {}
        var appearance: Dictionary = state.appearance
        if appearance.active:
            var mark := str(appearance.token) + ":" + str(appearance.stage)
            stages[appearance.stage] = true
            var live_actor: Dictionary = state.actors.filter(func(a): return a.id == appearance.actor)[0]
            if appearance.stage == "spin" and live_actor.visible and live_actor.nodeAlpha > 0.05 and live_actor.nodeAlpha < 0.95 and abs(live_actor.nodeRotation) > 0.1:
                if not intermediate_spin: sample("actual-intermediate-spin-fade", state)
                intermediate_spin = true
            if appearance.stage == "settle" and live_actor.visible and is_equal_approx(float(live_actor.nodeAlpha), 1.0) and is_zero_approx(fmod(float(live_actor.nodeRotation), TAU)):
                settled_marker = true
            if not seen.has(mark):
                seen[mark] = true
                sample(mark, state)
                _check(state.actor == null and state.stage == null, "appearance blocks player input")
                var actor: Dictionary = state.actors.filter(func(a): return a.id == appearance.actor)[0]
                _check(actor.hp > 0 and actor.initialized and actor.insideMap, "reset actor focused in actual map")
                if appearance.stage == "notice": _check(appearance.noticeVisible and appearance.text.contains("emerges"), "actual appearance notice")
                for pending in appearance.list.slice(int(appearance.cursor) + 1):
                    _check(not state.actors.filter(func(a): return a.id == pending)[0].visible, "later appearance remains visually absent")
        if state.stage == "Movement" and state.stopReason == "PlayerInput" and not appearance.active:
            var scene: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
            if scene.movement == null: return state
        if state.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            await _press(KEY_ENTER)
    _check(false, "settle bound")
    return {}

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.find_child("BattleSessionView", true, false)
    var state := active()
    sample("initial", state)
    _check(state.actor == "leader" and state.failure == null, "authored ordinary startup")
    _check(state.actors.filter(func(a): return a.id in ["hidden", "returner"] and a.hp == 0 and a.maxHp == 0 and not a.visible).size() == 2, "dormant actors are absent")
    await _press(KEY_UP)
    state = await settle()
    if state.is_empty():
        _finish()
        return
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    state = await settle()
    if state.is_empty():
        _finish()
        return
    sample("appeared-control", state)
    _check(state.round == 2, "queue generated after appearances")
    _check(events.filter(func(e): return e.Kind == "appearance-completed").map(func(e): return e.Actor.Value) == ["returner", "hidden"], "source slot appearance order")
    await _press(KEY_ENTER)
    await _press(KEY_F)
    await _press(KEY_TAB)
    _check(active().target == "returner", "ordinary attack selects returning enemy")
    await _press(KEY_ENTER)
    state = await settle()
    if not state.is_empty():
        sample("returned-again", state)
        _check(events.filter(func(e): return e.Kind == "death-cleanup" and e.Actor.Value == "returner").size() == 1, "ordinary death actually removed enemy")
        _check(events.filter(func(e): return e.Kind == "appearance-completed" and e.Actor.Value == "returner").size() == 2, "returning enemy actually appears again")
        _check(state.actors.filter(func(a): return a.id == "returner")[0].hp == 1, "returner restored from definition")
        await _press(KEY_DOWN)
        state = await settle()
        if not state.is_empty():
            _check(state.previewY == 4 and state.failure == null, "usable ordinary movement after return")
            sample("usable-movement", state)
    _check(intermediate_spin and settled_marker, "actual marker rotation/alpha animate and settle")
    _check(stages.has("spin") and stages.has("settle") and stages.has("notice"), "finite appearance phases delivered")
    samples.append({"label":"events", "events":events, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
