extends "res://probes/engine_exploration_observation.gd"

# Ordinary keys only after startup. Existing semantic observations; no state injection.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var seen: Dictionary = {}
var identity: String

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        if event.Kind.begins_with("conversion-") or event.Kind.begins_with("join-force-") or event.Kind.begins_with("ai-command") or event.Kind in ["region-program-started", "presentation-completed", "presentation-acknowledged", "round-started", "player-control", "action-committed"]:
            events.append(event)
        last_sequence = int(event.Sequence)

func active() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func actor(state: Dictionary, name: String) -> Dictionary:
    return state.actors.filter(func(a): return a.id == name)[0]

func sample(label: String, state: Dictionary) -> void:
    samples.append({"label":label, "sessionId":state.sessionId, "revision":state.revision,
        "round":state.round, "actor":state.actor, "stage":state.stage, "story":state.story,
        "flags":state.storyFlags, "aliases":state.visualAliases, "actors":state.actors, "physicalVisuals":state.physicalVisuals, "cameraFocus":state.cameraFocus, "failure":state.failure})

func settle() -> Dictionary:
    for frame in range(10000):
        await process_frame
        var state := active()
        if state.failure != null or state.scene.error != null or Time.get_ticks_msec() - started > 170000:
            sample("error", state)
            _check(false, "native session/presenter/deadline failure")
            return {}
        _check(state.sessionId == identity, "same session throughout")
        if state.story.active:
            _check(state.actor == null and state.stage == null, "story blocks manual battle input")
            if state.story.kind == "DialogueWait" and state.story.presented:
                var key := str(state.story.token)
                if not seen.has(key):
                    seen[key] = true
                    sample("dialogue", state)
                    _check(state.story.visible and not state.story.text.is_empty(), "actual dialogue node visible")
                    _check(actor(state, "enemy-jaro").hp == 0 and actor(state, "enemy-jaro").x == null, "enemy logical slot removed")
                    _check(actor(state, "ally-jaro").hp == 24 and actor(state, "ally-jaro").mp == 3, "ally retains own resources")
                    _check(actor(state, "ally-jaro").physicalAppearance == "enemy-jaro" and actor(state, "ally-jaro").visible, "ally projects retained physical alias")
                    _check(actor(state, "ally-jaro").nodeX == 122 and actor(state, "ally-jaro").nodeY == 84, "node uses retained override rather than definition start")
                    _check(actor(state, "ally-jaro").text.ends_with(" <") if seen.size() == 2 else actor(state, "ally-jaro").text.ends_with(" v"), "actual marker projects story facing")
                    _check(state.cameraFocus.size() == 1 and state.cameraFocus[0].X == 3 and state.cameraFocus[0].Y == 2, "camera transform uses live retained coordinates")
                    _check(state.story.cameraSlot == 1 and actor(state, "ally-jaro").insideMap, "actual camera frames copied alias")
                await _press(KEY_ENTER)
            continue
        var scene: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
        if state.stage == "Movement" and state.stopReason == "PlayerInput" and scene.movement == null:
            return state
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
    identity = state.sessionId
    sample("initial", state)
    _check(state.actor == "leader" and state.failure == null and actor(state, "ally-jaro").x == null, "ordinary authored startup with absent ally")
    for step in range(2):
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
    sample("converted-round", state)
    _check(state.round == 2 and seen.size() == 2 and 386.0 in state.storyFlags, "two actual dialogue acknowledgements precede new queue")
    _check(actor(state, "ally-jaro").control == "Automatic" and actor(state, "ally-jaro").activationWord == 4, "live source AI bit controls ally")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    state = await settle()
    if not state.is_empty():
        sample("after-ally-action", state)
        _check(actor(state, "sentry").hp < 100 and state.actor == "leader", "automatic ally combat then ordinary player")
        _check(events.any(func(e): return e.Kind == "ai-commandset6-ally"), "source ally decision consumed")
        await _press(KEY_DOWN)
        state = await settle()
        if not state.is_empty():
            sample("usable-movement", state)
            _check(state.previewY == 6, "usable player movement after conversion and combat")
    _check(events.filter(func(e): return e.Kind == "region-program-started").size() == 1, "played flag suppresses scene replay")
    samples.append({"events":events, "elapsedMs":Time.get_ticks_msec() - started})
    _finish()
