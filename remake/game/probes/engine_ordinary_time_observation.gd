extends "res://probes/engine_exploration_observation.gd"

# Ordinary keys and actual live state only. Controlled starts are supplied before startup.
var per_frame: Dictionary = {}
var attach_progress: Dictionary = {}
var attached := false

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", result_observed.bind(node))

func result_observed(json: String, node: Node) -> void:
    var result: Dictionary = JSON.parse_string(json)
    if result.boundary == "attach" and node.name == "ExplorationSessionView":
        attached = true
        attach_progress = JSON.parse_string(host.call("ReadPersistenceObservationJson"))
        if OS.get_environment("SF2_CLOCK_CASE").begins_with("partition-") or OS.get_environment("SF2_CLOCK_CASE") == "clock-policy":
            node.set_process(false)
    for event in result.get("observations", []):
        if event.Kind == "gameplay-wait":
            var frame := Engine.get_process_frames()
            per_frame[frame] = per_frame.get(frame, 0) + 1

func state() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String) -> Dictionary:
    var current := state()
    var compact: Dictionary = {}
    for key in ["revision", "simulationTick", "mainSeed", "canWaitAtInput", "ordinaryTime", "settledFieldEntry", "menuEntryPending", "menuEntrySteps", "wait", "cursor", "focused", "tickDebt", "failure", "entities", "gold"]:
        compact[key] = current.get(key)
    samples.append({"label":label, "state":compact})
    return current

func frames(count: int) -> void:
    for i in range(count): await process_frame

func wait_ready() -> bool:
    for i in range(300):
        var current := state()
        if current.failure != null: break
        if current.canWaitAtInput: return true
        await process_frame
    sample("readiness-failed")
    _check(false, "bounded ordinary control")
    return false

func wait_panel(name: String) -> Node:
    for i in range(180):
        var panel := host.find_child(name, true, false)
        if panel != null: return panel
        await process_frame
    _check(false, "bounded panel: " + name)
    return null

func persistence() -> Dictionary:
    return JSON.parse_string(host.call("ReadPersistenceObservationJson"))

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    root.size = Vector2i(1920, 1080)
    root.grab_focus()
    await frames(3)
    if OS.get_environment("SF2_CLOCK_CASE") != "diagnostic":
        # Ordinary title -> witch menu -> Continue -> slot1.
        await _press(KEY_ENTER)
        await _press(KEY_E)
        await _press(KEY_DOWN)
        await _press(KEY_ENTER)
        if await wait_panel("SaveSlotView") == null: _finish(); return
        await _press(KEY_ENTER)
        await frames(2)
        view = host.get_node_or_null("ExplorationSessionView")
        _check(view != null and attached, "fresh Continue mounts a new session")
        samples.append({"label":"continue-before-first-ordinary-step", "persistence":attach_progress})
        if view != null: sample("continued")
        if OS.get_environment("SF2_CLOCK_CASE") == "continue": _finish(); return
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        samples.append({"label":"startup-shell", "size":str(root.size), "mode":root.mode,
            "children":host.find_children("*", "", true, false).map(func(n): return str(n.name))})
        var entry := host.find_child("SessionEntryView", true, false)
        if entry != null: samples.append({"label":"startup-error", "entry":JSON.parse_string(entry.call("ReadEntryObservationJson"))})
        _check(false, "ordinary private exploration mounted"); _finish(); return
    root.grab_focus()
    await frames(6)
    if not await wait_ready(): _finish(); return
    if OS.get_environment("SF2_CLOCK_CASE") == "clock-policy":
        await observe_clock_policy()
        _finish(); return
    if OS.get_environment("SF2_CLOCK_CASE").begins_with("partition-"):
        await observe_partitions()
        _finish(); return
    if OS.get_environment("SF2_CLOCK_CASE") == "focus":
        await observe_focus()
        _finish(); return
    if OS.get_environment("SF2_CLOCK_CASE") in ["budget", "transition", "music", "quit-pending", "pause-pending"]:
        await stress_boundary(OS.get_environment("SF2_CLOCK_CASE"))
        _finish(); return
    await frames(60)
    var initial := sample("standing-before")
    var textures: Array = []
    for i in range(32):
        await process_frame
        var projection = state().get("cameraProjection")
        if projection != null:
            for actor in projection.actors:
                if actor.slot == 0 and actor.visible and actor.texture not in textures: textures.append(actor.texture)
    var idle := sample("standing-after")
    _check(idle.simulationTick > initial.simulationTick and idle.entities[0].animationCounter != initial.entities[0].animationCounter, "standing advances without V")
    _check(textures.size() == 2, "standing player projects both real sprite textures without movement")
    samples.append({"label":"standing-projection", "textures":textures})
    var key := InputEventKey.new(); key.keycode = KEY_V; key.pressed = true; Input.parse_input_event(key)
    var wait_start := state()
    await frames(30)
    var wait_end := sample("held-V")
    key = InputEventKey.new(); key.keycode = KEY_V; Input.parse_input_event(key)
    _check(wait_end.simulationTick - wait_start.simulationTick <= 31, "ordinary V adds no second tick")
    await _press(KEY_DOWN)
    if not await wait_ready(): _finish(); return
    await _press(KEY_DOWN)
    if not await wait_ready(): _finish(); return
    var arrived := sample("player-arrived")
    _check(arrived.entities.any(func(e): return e.follower != null and e.moving), "player ready while a real follower is still moving")
    await _press(KEY_F10)
    if await wait_panel("SystemMenuView") == null: _finish(); return
    var menu := sample("menu-settled")
    _check(menu.settledFieldEntry, "menu freezes only after player/followers and sprites settle")
    _check(not host.find_child("SystemSave", true, false).disabled, "menu save available")
    var revision = menu.revision
    await frames(30)
    _check(state().revision == revision, "system menu freezes world")
    await _press(KEY_ENTER)
    await _press(KEY_UP)
    await _press(KEY_F10)
    _check(state().menuEntryPending, "committed motion enters finite menu settlement")
    await _press(KEY_ESCAPE)
    if await wait_panel("SystemMenuView") == null: _finish(); return
    var cancelled := sample("cancelled-settlement")
    _check(host.find_child("SystemSave", true, false).disabled, "cancelled settlement does not offer Save")
    revision = cancelled.revision
    await frames(12)
    _check(state().revision == revision, "Cancel freezes immediately without completing motion")
    await _press(KEY_ENTER)
    if not await wait_ready(): _finish(); return
    await _press(KEY_UP)
    await _press(KEY_SPACE)
    if await wait_panel("SaveSlotView") == null: _finish(); return
    _check(persistence().panel == "save-slots", "direct Save settles an already committed move")
    sample("direct-save-settled")
    await _press(KEY_ESCAPE)
    # This is the genuine church entity event, not a direct service command.
    await _press(KEY_ENTER)
    for i in range(300):
        if state().wait == "ChurchWait": break
        await process_frame
    var church := sample("church")
    _check(church.wait == "ChurchWait", "genuine Church caller")
    revision = church.revision
    await frames(30)
    _check(state().revision == revision, "Church does not receive ordinary neutral time")
    for i in range(3): await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    if await wait_panel("SaveSlotView") == null: _finish(); return
    _check(persistence().panel == "save-slots", "Church Save returns caller then offers save")
    sample("church-return-save")
    await _press(KEY_ENTER)
    await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    _check(persistence().panel == "saved", "disposable slot1 written")
    samples.append({"label":"saved", "persistence":persistence()})
    await _press(KEY_ENTER)
    await frames(30)
    await _press(KEY_F10)
    if await wait_panel("SystemMenuView") == null: _finish(); return
    for i in range(4): await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    var quit_panel := await wait_panel("SaveSlotView")
    if quit_panel != null:
        var quit_state: Dictionary = JSON.parse_string(quit_panel.call("ReadSaveObservationJson"))
        samples.append({"label":"conservative-quit", "panel":quit_state})
        _check(quit_state.state == "confirm-quit", "quit remains available after automatic time")
    await _press(KEY_ESCAPE)
    await _press(KEY_ESCAPE)
    if OS.get_environment("SF2_CLOCK_FOCUS") == "1": await observe_focus()
    _check(per_frame.values().all(func(value): return value <= 4), "at most four ordinary neutral steps per native callback")
    samples.append({"label":"native-clock", "framesObserved":per_frame.size(), "maxNeutralSteps":per_frame.values().max()})
    _finish()

func observe_focus() -> void:
    root.grab_focus()
    await frames(3)
    var other := Window.new(); other.hide(); other.force_native = true
    other.title = "Ordinary clock focus observation"; other.size = Vector2i(360, 240)
    root.add_child(other); other.show(); other.grab_focus()
    await frames(12)
    var lost := sample("physical-focus-lost")
    _check(not lost.focused, "actual native focus moved to another window")
    await frames(20)
    _check(state().revision == lost.revision, "focus loss freezes world")
    other.hide(); root.grab_focus(); await frames(2)
    var resumed := sample("physical-focus-return")
    _check(resumed.focused and resumed.simulationTick - lost.simulationTick <= 1, "focus return has no paused debt")
    other.queue_free()

func observe_partitions() -> void:
    _check(view.has_method("_Process"), "actual host callback is available")
    if not view.has_method("_Process"): return
    # The actual host callback consumes controlled deltas; ordinary keys still own actions.
    # Initial draw/sprite deliveries use the same single-step schedule in both runs.
    view.call("_Process", 0.0)
    for i in range(16):
        view.call("_Process", 1.0 / 60.0)
        await process_frame
    sample("partition-initial")
    var chunk := 1 if OS.get_environment("SF2_CLOCK_CASE") == "partition-a" else 3
    for phase in range(3):
        if phase > 0: await _press(KEY_DOWN if phase == 1 else KEY_UP)
        for i in range(24 / chunk):
            view.call("_Process", float(chunk) / 60.0)
            await process_frame
    var final := sample("partition-final")
    _check(final.simulationTick == 88, "matched schedule consumes exactly 88 total logical steps")
    _check(final.failure == null and final.canWaitAtInput, "partitioned actions return ordinary control")

func observe_clock_policy() -> void:
    # Freeze automatic callbacks at attach, save through the UI, then drive only actual callbacks.
    await _press(KEY_SPACE)
    if await wait_panel("SaveSlotView") == null: return
    await _press(KEY_ENTER); await _press(KEY_DOWN); await _press(KEY_ENTER)
    _check(persistence().panel == "saved", "policy setup saves its own disposable slot")
    await _press(KEY_ENTER)
    view.call("_Process", 0.0)
    var before := sample("animation-only-before")
    view.call("_Process", 1.0 / 60.0)
    var after := sample("animation-only-after")
    _check(after.revision > before.revision and after.mainSeed == before.mainSeed and after.gold == before.gold, "animation-only step changes revision without RNG or gold")
    await observe_quit_warning("animation-only-warning")
    view.call("_Process", 0.0)
    var rng_before := state()
    for i in range(120):
        view.call("_Process", 1.0 / 60.0)
        if state().mainSeed != rng_before.mainSeed: break
    _check(state().mainSeed != rng_before.mainSeed, "ordinary NPC service changes the shared seed")
    sample("rng-changing-step")
    await observe_quit_warning("rng-changing-warning")
    view.call("_Process", 0.0)
    var capped := state()
    view.call("_Process", 0.25 + 1.0 / 120.0)
    _check(state().simulationTick == capped.simulationTick + 4, "slow callback executes at most four steps")
    view.call("_Process", 0.0)
    _check(state().simulationTick == capped.simulationTick + 4, "excess whole-step backlog is dropped")
    view.call("_Process", 1.0 / 120.0 + 0.000000001)
    _check(state().simulationTick == capped.simulationTick + 5, "fractional debt is retained")
    sample("callback-cap-and-fraction")

func observe_quit_warning(label: String) -> void:
    host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
    if await wait_panel("SaveSlotView") == null: return
    var panel: Dictionary = JSON.parse_string(host.find_child("SaveSlotView", true, false).call("ReadSaveObservationJson"))
    samples.append({"label":label, "panel":panel})
    _check(panel.state == "confirm-quit" and str(panel).contains("may be unsaved changes"), "conservative warning is readable and does not assert lost story progress")
    await _press(KEY_ESCAPE)

func stress_boundary(case_name: String) -> void:
    await frames(30)
    if case_name == "transition":
        # Both physical intents arrive before the next ordinary service opportunity.
        for code in [KEY_RIGHT, KEY_F10]:
            var event := InputEventKey.new(); event.keycode = code; event.pressed = true; Input.parse_input_event(event)
        await frames(2)
        for code in [KEY_RIGHT, KEY_F10]:
            var event := InputEventKey.new(); event.keycode = code; Input.parse_input_event(event)
        if await wait_panel("SystemMenuView") == null: return
        sample("unexpected-caller-paused")
        var reason := host.find_child("ReasonSave", true, false)
        _check(reason.text.contains("scene changed"), "unexpected caller stops settlement with an honest reason")
    else:
        await _press(KEY_ENTER)
        if case_name == "music":
            await observe_music()
            return
        if not await wait_ready(): return
        await frames(3)
        sample("distant-follower")
        await _press(KEY_SPACE)
        _check(state().menuEntryPending, "distant follower starts bounded save settlement")
        if case_name == "quit-pending":
            # Exercise the existing WM-close consumer; no gameplay state or completion is injected.
            host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
            if await wait_panel("SaveSlotView") == null: return
            var panel: Dictionary = JSON.parse_string(host.find_child("SaveSlotView", true, false).call("ReadSaveObservationJson"))
            _check(panel.state == "confirm-quit" and not state().menuEntryPending, "Quit immediately escapes pending settlement")
            sample("quit-during-settlement")
        elif case_name == "pause-pending":
            paused = true
            _check(not state().menuEntryPending, "tree pause immediately cancels settlement")
            paused = false
            if await wait_panel("SystemMenuView") == null: return
            sample("pause-during-settlement")
        else:
            if await wait_panel("SystemMenuView") == null: return
            var stopped := sample("budget-exhausted")
            _check(stopped.menuEntrySteps == 120 and not stopped.settledFieldEntry, "settlement stops exactly at 120 without saving motion")
            _check(host.find_child("ReasonSave", true, false).text.contains("did not settle"), "budget failure has a readable explanation")
    var before := state()
    await frames(25)
    _check(state().revision == before.revision, "failed/cancelled settlement stays frozen without retry")

func observe_music() -> void:
    var ready := false
    for i in range(300):
        var current := state()
        if current.get("canWaitForText", false) and current.visibleCharacters < 0:
            ready = true; break
        await process_frame
    _check(ready, "real field text reached its revealed input boundary")
    var text := sample("text-idle-before")
    await frames(30)
    _check(state().simulationTick == text.simulationTick and state().mainSeed == text.mainSeed, "reading text adds no ordinary opportunities")
    await _press(KEY_ENTER)
    var logical := false
    var finished := false
    for i in range(2400):
        var current := state()
        if current.failure != null: _check(false, "music consumer failed"); break
        if current.wait == "MusicWait" and current.musicWait.LogicalDone and not logical:
            logical = true
            var held := sample("music-logical-end")
            await frames(5)
            _check(state().wait == "MusicWait" and state().simulationTick == held.simulationTick and state().tickDebt == 0, "logical end waits for actual finite audio without field debt")
        if logical and current.canWaitAtInput:
            finished = true
            sample("finite-audio-return")
            break
        await process_frame
    _check(logical and finished, "logical and actual finite-music gates both complete before field return")
