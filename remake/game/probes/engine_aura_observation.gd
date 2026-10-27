extends "res://probes/engine_exploration_observation.gd"

# Ordinary input and lifecycle events only. No state injection or fabricated completion.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var suspended_once := false

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            if event.Kind in ["heal", "mp", "hp", "exp", "healing-recovery", "healing-exp-contribution", "player-control"] or str(event.Kind).begins_with("rng-"):
                events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, state: Dictionary) -> void:
    samples.append({"label":label, "sessionId":state.sessionId, "revision":state.revision,
        "actor":state.get("actor"), "stage":state.get("stage"), "actors":state.get("actors"),
        "mainSeed":state.mainSeed, "thinkingSeed":state.thinkingSeed, "scene":state.scene, "failure":state.failure})

func _run() -> void:
    create_timer(175).timeout.connect(func(): _check(false, "native witness deadline"); _finish())
    node_added.connect(observe_node)
    root.size = Vector2i(1280, 720)
    root.content_scale_size = Vector2i(1280, 720)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.find_child("BattleSessionView", true, false)
    var initial := active()
    sample("initial", initial)
    _check(initial.get("actor") == "z-priest" and initial.failure == null, "authored caster has ordinary control")
    if not failures.is_empty():
        _finish()
        return
    await _press(KEY_ENTER)
    await _press(KEY_H)
    await _press(KEY_ESCAPE)
    var canceled := active()
    _check(canceled.mainSeed == initial.mainSeed and canceled.actors == initial.actors, "cancel preserves resources and RNG")
    await _press(KEY_ENTER)
    # Cancellation resets the candidate; select all four learned levels ordinarily.
    for i in range(4): await _press(KEY_H)
    var selected := active()
    sample("selected", selected)
    _check(selected.spell != null and selected.spell.Level == 4 and selected.target == "z-priest", "ordinary AURA4 selects self")
    if not failures.is_empty():
        _finish()
        return
    await _press(KEY_ENTER)
    var mark := ""
    var returned := false
    var recovery_targets: Array = []
    var draw_count := 0
    while Time.get_ticks_msec() - started < 170000:
        var state := active()
        var scene: Dictionary = state.scene
        if state.failure != null or scene.get("error") != null:
            sample("failure", state)
            _check(false, "session and presenter remain error-free")
            break
        if scene.get("phase") == null:
            if state.get("stage") == "Movement" and state.get("actor") != "z-priest":
                returned = true
                sample("returned", state)
                _check(state.sessionId == initial.sessionId and state.thinkingSeed == initial.thinkingSeed, "same session and thinking stream")
                var old_x = state.previewX
                await _press(KEY_LEFT)
                var moved := active()
                sample("usable-next-turn", moved)
                _check(moved.previewX == old_x - 1 and moved.failure == null, "next actor accepts ordinary movement")
                break
            await process_frame
            continue
        var aura = scene.get("aura")
        if aura != null: draw_count = maxi(draw_count, int(aura.drawCount))
        var next_mark := str(scene.waitToken)
        if next_mark != mark:
            mark = next_mark
            sample(str(scene.phase) + ":" + mark, state)
            _check(aura != null and aura.targets == ["a-edge", "b-center", "n-neutral", "m-full", "z-priest", "f-far"], "AURA4 ordered full ally scan")
            if scene.phase == "ResultMessage":
                recovery_targets.append(aura.target)
                _check(scene.message.contains("recovered") and scene.displayedAlly == aura.target and aura.visible, "actual target-specific recovery feedback")
        if scene.phase == "SpellCost" and not suspended_once:
            suspended_once = true
            var paid_revision = state.revision
            var paid_elapsed = scene.elapsed
            view.hide()
            for i in range(8): await process_frame
            var hidden := active()
            _check(hidden.revision == paid_revision and hidden.scene.elapsed == paid_elapsed, "hidden scene retains paid prefix and wait")
            view.show()
            root.emit_signal("focus_exited")
            for i in range(8): await process_frame
            var unfocused := active()
            _check(unfocused.revision == paid_revision and unfocused.scene.suspended, "focus suspension retains wait and resources")
            sample("suspended-after-payment", unfocused)
            root.emit_signal("focus_entered")
        if scene.phase in ["ActionMessage", "ResultMessage", "RewardMessage", "GrowthMessage"]:
            await _press(KEY_ENTER)
        else: await process_frame
    _check(returned and suspended_once, "finite scene resumes to usable next turn")
    _check(draw_count > 0, "authored recovery draw consumer ran")
    _check(recovery_targets == ["a-edge", "b-center", "n-neutral", "m-full", "z-priest", "f-far"], "all six recoveries shown in order")
    _check(events.filter(func(e):return e.Kind == "mp").size() == 1, "one MP payment")
    _check(events.filter(func(e):return e.Kind == "exp").size() == 1, "one EXP award")
    _check(events.filter(func(e):return e.Kind == "healing-recovery").size() == 6, "six recovery publications including full HP")
    _check(events.filter(func(e):return str(e.Kind).begins_with("rng-")).size() == 2, "one shared award RNG pair")
    var paid = events.filter(func(e):return e.Kind == "mp")
    if paid.size() == 1: _check(paid[0].Before == 80 and paid[0].After == 60, "AURA4 pays twenty MP")
    var contributions = events.filter(func(e):return e.Kind == "healing-exp-contribution")
    _check(contributions.map(func(e):return int(e.After)) == [24,25,25,25,25,25], "source contributions share cap25")
    samples.append({"label":"events", "events":events, "elapsedMs":Time.get_ticks_msec()-started, "drawCount":draw_count})
    _finish()
