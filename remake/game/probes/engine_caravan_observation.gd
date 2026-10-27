extends "res://probes/engine_exploration_observation.gd"

# Ordinary host and keys; visibility suspension changes only the view, never gameplay state.
var started := Time.get_ticks_msec()
var identity: String
var sizes: Dictionary = {}
var phases: Dictionary = {}
var events: Array = []
var last_sequence := -1
var suspended := false
var saw_motion := false
var saw_vehicle := false

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        if event.Kind.begins_with("caravan-") or event.Kind in ["zone-entered", "entity-sound"] or event.Detail == "CommitCaravanMode":
            events.append(event)
        last_sequence = int(event.Sequence)

func active() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, state: Dictionary) -> void:
    samples.append({"label": label, "sessionId": state.sessionId, "revision": state.revision,
        "mode": state.travelMode, "tick": state.simulationTick, "wait": state.wait,
        "cursor": state.cursor, "counter": state.stepCounter, "flags": state.flags,
        "list": state.currentEntities, "draws": state.authoredDraws,
        "entities": state.entities.map(func(a): return {"id":a.id,"slot":a.slot,"sprite":a.sprite,
            "x":a.x,"y":a.y,"facing":a.facing,"moving":a.moving,"size":a.renderSize}),
        "failure": state.failure})

func settle() -> Dictionary:
    for frame in range(5000):
        await process_frame
        var state := active()
        if state.failure != null or state.presentation.error != null or Time.get_ticks_msec() - started > 170000:
            sample("error", state)
            _check(false, "native session/presentation/deadline")
            return {}
        if state.sessionId != identity:
            _check(false, "same session")
            return {}
        for actor in state.entities:
            saw_motion = saw_motion or actor.moving
        for draw in state.authoredDraws:
            if draw.Visible:
                sizes[str(int(draw.Size))] = true
                if draw.Slot == 0 and draw.Sprite == 62: saw_vehicle = true
        var stage := str(state.travelMode) + ":" + str(state.cursor) + ":" + str(state.wait)
        if not phases.has(stage):
            phases[stage] = true
            sample("stage", state)
        if not suspended and state.wait == "EntityWait" and state.cursor != null:
            var tick = state.simulationTick
            var revision = state.revision
            view.hide()
            for pause_frame in range(6): await process_frame
            var held := active()
            _check(held.simulationTick == tick and held.revision == revision, "hidden view suspends ticks and completions")
            view.show()
            suspended = true
        if state.stop == "PlayerInput" and state.wait == null and not state.entities[0].busy:
            return state
    _check(false, "bounded settle")
    return {}

func step(key: Key) -> Dictionary:
    await _press(key)
    return await settle()

func _run() -> void:
    # A custom headless SceneTree does not inherit the main scene's window extent.
    root.size = Vector2i(1280, 720)
    root.content_scale_size = Vector2i(960, 540)
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        _check(false, "authored exploration host")
        _finish()
        return
    var state := active()
    identity = state.sessionId
    sample("initial", state)
    _check(state.travelMode == "Walking" and state.failure == null, "walking acquired start")
    state = await step(KEY_RIGHT)
    if state.is_empty():
        _finish()
        return
    sample("boarded", state)
    _check(state.authoredDraws.any(func(draw): return draw.Slot == 0 and draw.Visible and draw.Radius >= 3), "player marker drawn at usable viewport scale")
    _check(state.travelMode == "Caravan" and state.entities[0].sprite == 62 and state.stepCounter == 9, "completed boarding commits mode1 without walking counter")
    state = await step(KEY_ENTER)
    if state.is_empty():
        _finish()
        return
    sample("driving-confirm", state)
    _check(state.travelMode == "Caravan" and state.stepCounter == 9 and state.failure == null, "ordinary Confirm with parked physical Caravan preserves mode/counter without exception")
    for travel in range(5):
        state = await step(KEY_RIGHT)
        if state.is_empty():
            _finish()
            return
        _check(state.travelMode == "Caravan" and state.stepCounter == 9 and not 90.0 in state.flags, "travel stays outside walking zone/counter")
    sample("travelled", state)
    state = await step(KEY_RIGHT)
    if state.is_empty():
        _finish()
        return
    sample("landed", state)
    _check(state.travelMode == "Walking" and state.entities[0].sprite == 7 and state.entities[0].x == 3456 and state.stepCounter == 9, "landing restores live appearance and walking without zone/counter")
    _check(not 90.0 in state.flags and state.currentEntities == [1.0,2.0,3.0], "landing preserves current list")
    state = await step(KEY_DOWN)
    if not state.is_empty(): state = await step(KEY_UP)
    if not state.is_empty():
        sample("ordinary-zone", state)
        _check(90.0 in state.flags and state.stepCounter == 7 and state.travelMode == "Walking", "later walking reaches ordinary zone")
    _check(suspended and saw_motion and saw_vehicle, "actual suspension, motion and vehicle drawing")
    for size in [12,14,16,18,20,22,24]: _check(sizes.has(str(size)), "drawn size " + str(size))
    _check(events.filter(func(e): return e.Detail == "CommitCaravanMode").size() == 2, "exactly two mode commits")
    samples.append({"events":events,"elapsedMs":Time.get_ticks_msec()-started})
    _finish()
