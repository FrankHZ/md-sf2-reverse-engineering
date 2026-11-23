extends "res://probes/engine_exploration_observation.gd"

# Ordinary host keys after authored startup; read actual nodes/state/errors only.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var seen: Dictionary = {}
var stages: Dictionary = {}
var intermediate_fade := false
var focus_checked := false
var language: String
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

func check_notice(state: Dictionary) -> void:
    var appearance: Dictionary = state.appearance
    var panel := view.find_child("AppearanceNotice", true, false) as PanelContainer
    var label := panel.get_node("Help") as Label
    var name_text: String = {"returner":"Returner", "hidden":"Hidden"}[appearance.actor]
    var expected := ("%s出现了！" if language == "zh-CN" else "%s emerges!") % name_text
    _check(appearance.noticeVisible and label.is_visible_in_tree() and label.text == expected,
        "visible localized appearance name and notice")
    var frame := panel.get_global_rect()
    var body := label.get_global_rect()
    var font_size := label.get_theme_font_size("font_size")
    _check(root.get_visible_rect().encloses(frame) and frame.encloses(body) and body.has_area()
        and body.size.y >= label.get_line_count() * label.get_line_height() and font_size == 24,
        "shared hint text fits the visible frame")
    samples.append({"label":"notice-layout", "text":label.text, "language":language,
        "frame":str(frame), "body":str(body), "fontSize":font_size})
    if focus_checked: return
    focus_checked = true
    root.focus_exited.emit()
    for i in 6: await process_frame
    var paused := active()
    _check(paused.appearance.suspended and paused.appearance.elapsed == appearance.elapsed
        and paused.appearance.token == appearance.token, "focus loss pauses appearance clock")
    root.focus_entered.emit()
    await _press(KEY_ENTER)
    await _press(KEY_RIGHT)
    var resumed := active()
    _check(resumed.appearance.token == appearance.token and resumed.appearance.elapsed > appearance.elapsed
        and resumed.revision == state.revision and resumed.stage == null, "input cannot skip or move during notice")

func settle() -> Dictionary:
    var last_appearance: Dictionary = {}
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
        if not last_appearance.is_empty() and appearance.token != last_appearance.token:
            _check(last_appearance.stage == "notice" and float(last_appearance.elapsed) < 1.5
                and float(last_appearance.elapsed) >= 1.5 - 1.0 / 60 - 0.00001, "completion crosses 1.5 seconds")
        last_appearance = appearance if appearance.active else {}
        if appearance.active:
            var mark := str(appearance.token) + ":" + str(appearance.stage)
            stages[appearance.stage] = true
            var live_actor: Dictionary = state.actors.filter(func(a): return a.id == appearance.actor)[0]
            if appearance.stage == "spin" and live_actor.visible and live_actor.nodeAlpha > 0.05 and live_actor.nodeAlpha < 0.95:
                if not intermediate_fade:
                    sample("actual-intermediate-fade", state)
                    _check(is_zero_approx(live_actor.nodeRotation) if state.scene.reducedFlash else abs(live_actor.nodeRotation) > 0.1,
                        "intermediate rotation follows reduced flash")
                intermediate_fade = true
            if appearance.stage == "settle" and live_actor.visible and is_equal_approx(float(live_actor.nodeAlpha), 1.0) and is_zero_approx(fmod(float(live_actor.nodeRotation), TAU)):
                settled_marker = true
            if not seen.has(mark):
                seen[mark] = true
                sample(mark, state)
                _check(state.actor == null and state.stage == null, "appearance blocks player input")
                var actor: Dictionary = state.actors.filter(func(a): return a.id == appearance.actor)[0]
                _check(actor.hp > 0 and actor.initialized and actor.insideMap, "reset actor focused in actual map")
                match appearance.stage:
                    "spin": _check(not appearance.noticeVisible and appearance.elapsed < 0.6, "notice hidden during spin")
                    "settle": _check(not appearance.noticeVisible and is_equal_approx(appearance.elapsed, 0.6), "settle starts at 0.6 seconds")
                    "notice":
                        _check(is_equal_approx(appearance.elapsed, 0.8), "notice starts at 0.8 seconds")
                        await check_notice(state)
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
    root.size = Vector2i(1600, 900)
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.find_child("BattleSessionView", true, false)
    if view == null:
        samples.append({"label":"startup", "window":JSON.parse_string(host.call("ReadWindowSupportObservationJson"))})
        _check(false, "ordinary battle did not mount")
        _finish()
        return
    language = JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied.Language
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
    state = await settle()
    if state.is_empty():
        _finish()
        return
    sample("appeared-control", state)
    _check(state.round == 2, "queue generated after appearances")
    _check(events.filter(func(e): return e.Kind == "appearance-completed").map(func(e): return e.Actor.Value) == ["returner", "hidden"], "source slot appearance order")
    await _press(KEY_ENTER)
    await _press(KEY_F)
    for i in state.actors.size():
        if active().target == "returner": break
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
    _check(intermediate_fade and settled_marker, "actual marker rotation/alpha animate and settle")
    _check(stages.has("spin") and stages.has("settle") and stages.has("notice"), "finite appearance phases delivered")
    samples.append({"label":"events", "events":events, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
