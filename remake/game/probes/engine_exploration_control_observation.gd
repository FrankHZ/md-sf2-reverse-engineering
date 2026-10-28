extends "res://probes/engine_exploration_observation.gd"

# Small authored host witness. All gameplay arrives through mapped InputEvents.
var requests := 0
var last_sequence := -1
var unknowns: Array = []
var inputs: Array = []
var identity := ""
var began := Time.get_ticks_msec()
var ended := false
var unavailable: Array = []
var focus_requested := false
var intentional_focus := false
var interrupted_hold := false
var held_keys: Dictionary = {}
var held_axis := false

func stop_unavailable(message: String) -> bool:
    if not ended:
        unavailable.append(message)
        _finish()
    return false

func require(ok: bool, message: String) -> bool:
    if ended: return false
    if not intentional_focus and (interrupted_hold or not root.has_focus()):
        return stop_unavailable("Focus invalidated observation: " + message)
    if not ok:
        failures.append(message)
        _finish()
    return ok

func focus_ready(label: String) -> bool:
    if ended: return false
    if Time.get_ticks_msec() - began > 180000:
        return require(false, "180-second observer deadline")
    if intentional_focus: return true
    if interrupted_hold:
        return stop_unavailable("External focus loss interrupted held input: " + label)
    if not root.has_focus():
        if focus_requested: return stop_unavailable("Root lost focus again: " + label)
        focus_requested = true
        root.grab_focus() # At most one unplanned request; never fight another window.
        var deadline := Time.get_ticks_msec() + 1000
        while not root.has_focus() and Time.get_ticks_msec() < deadline:
            await process_frame
        await process_frame
        if not root.has_focus(): return stop_unavailable("Root focus unavailable: " + label)
    return true

func wait_until(test: Callable, label: String, seconds := 3.0, environment := false) -> bool:
    var deadline := Time.get_ticks_msec() + int(seconds * 1000)
    while Time.get_ticks_msec() < deadline:
        if not await focus_ready(label): return false
        if is_instance_valid(view) and not require(state().failure == null, "healthy session: " + label): return false
        if test.call(): return true
        await process_frame
    if environment: return stop_unavailable(label)
    return require(false, "Deadline waiting for " + label)

func tap(code: Key) -> bool:
    if not await key(code, true): return false
    return await key(code, false)

func text_ready() -> bool:
    return await wait_until(func(): return state().visibleCharacters < 0 or state().visibleCharacters >= state().totalCharacters, "actual text reveal")

func observe(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        if event.Kind == "movement-requested": requests += 1
        last_sequence = int(event.Sequence)

func state() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label, "requests":requests, "state":s})
    require(s.failure == null and s.sessionId == identity, "healthy same-session " + label)
    return s

func key(code: Key, pressed: bool, echo := false) -> bool:
    if not await focus_ready("mapped key edge"): return false
    inputs.append({"key":code, "pressed":pressed, "echo":echo, "focused":root.has_focus(), "frame":Engine.get_process_frames()})
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = pressed
    event.echo = echo
    if pressed: held_keys[code] = true
    else: held_keys.erase(code)
    Input.parse_input_event(event)
    await process_frame
    await process_frame
    return await focus_ready("mapped key delivery")

func rest(seconds := 0.35) -> bool:
    var deadline := Time.get_ticks_msec() + int(seconds * 1000)
    while Time.get_ticks_msec() < deadline:
        if not await focus_ready("bounded hold interval"): return false
        await process_frame
    return true

func inert(label: String) -> bool:
    var before := requests
    if not await rest(): return false
    if not require(requests == before, "no new Move after " + label): return false
    record(label)
    return not ended

func settle() -> bool:
    return await wait_until(func(): return state().canWaitAtInput, "motion completion")

func player() -> Dictionary:
    for entity in state().entities:
        if entity.id == "traveler": return entity
    return {}

func face_npc() -> bool:
    if not await settle(): return false
    for attempt in range(8):
        var actor := player()
        var x := int(actor.x) / 384
        var y := int(actor.y) / 384
        if x == 1 and y == 1: break
        if not await tap(KEY_D if x < 1 else KEY_A if x > 1 else KEY_J if y < 1 else KEY_W): return false
        if not await settle(): return false
    if not await tap(KEY_D): return false
    if not await settle(): return false
    return require(state().help.contains(": Talk"), "facing the real event shows mapped Talk")

func lifecycle(kind: String) -> bool:
    if not await key(KEY_J, true): return false
    if not await rest(0.3): return false
    if kind == "hidden": view.hide()
    else: paused = true
    var before := requests
    if not await rest(): return false
    if not require(requests == before, kind + " suspends movement"): return false
    if kind == "hidden": view.show()
    else: paused = false
    if not await inert(kind + " return with old key held"): return false
    if not await key(KEY_J, false): return false
    before = requests
    if not await tap(KEY_W): return false
    if not await settle(): return false
    return require(requests == before + 1, kind + " fresh direction restores movement")

func native_focus() -> bool:
    if not await key(KEY_J, true): return false
    if not await rest(0.3): return false
    var other := Window.new()
    other.hide()
    other.force_native = true
    other.transient = true
    other.title = "Exploration control focus witness"
    other.size = Vector2i(240, 100)
    root.add_child(other)
    intentional_focus = true
    other.show()
    other.grab_focus()
    if not await wait_until(func(): return other.has_focus() and not root.has_focus(), "native focus transfer unavailable", 1.0, true): return false
    if not await inert("observed native focus loss"): return false
    other.hide()
    root.grab_focus() # Planned return from the focus witness window.
    if not await wait_until(func(): return root.has_focus(), "native root focus return unavailable", 1.0, true): return false
    intentional_focus = false
    other.queue_free()
    if not await inert("native focus return with old injected key held"): return false
    if not await key(KEY_J, false): return false
    var before := requests
    if not await tap(KEY_W): return false
    if not await settle(): return false
    if not require(requests == before + 1, "fresh input after focus return"): return false
    unknowns.append("Keys/pad events are injected; physical device release across OS focus loss is not observed")
    return true

func _run() -> void:
    node_added.connect(observe)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        require(false, "ordinary exploration host mounted")
        return
    if not await focus_ready("startup"): return
    root.focus_exited.connect(func():
        if not intentional_focus and (not held_keys.is_empty() or held_axis): interrupted_hold = true)
    identity = state().sessionId
    if not require(state().canWaitAtInput, "authored field input"): return
    if not require(state().help.contains("J") and state().help.contains(": Talk"), "mapping-derived initial prompt"): return
    if not await tap(KEY_S): return
    if not require(requests == 0, "old down binding removed"): return
    if not await key(KEY_J, true): return
    if not await rest(0.55): return
    if not require(requests >= 3, "single mapped press walks multiple steps through ordinary motion waits"): return
    if not require(not state().help.contains(": Talk"), "hint disappears after leaving target"): return
    if not await wait_until(func(): return player().y == 5 * 384 and state().canWaitAtInput, "actual map obstruction"): return
    var blocked := requests
    if not await rest(0.4): return
    if not require(requests == blocked, "collision stops at boundary without queued moves"): return
    if not await key(KEY_J, false): return
    if not await inert("release"): return
    if not await face_npc(): return
    var axis := InputEventJoypadMotion.new()
    axis.device = 0
    axis.axis = JOY_AXIS_LEFT_Y
    axis.axis_value = 0.8
    var axis_before := requests
    if not await focus_ready("mapped stick deflection"): return
    held_axis = true
    Input.parse_input_event(axis)
    if not await rest(0.4): return
    if not require(requests >= axis_before + 2, "mapped stick deflection repeats movement"): return
    axis = InputEventJoypadMotion.new()
    axis.device = 0
    axis.axis = JOY_AXIS_LEFT_Y
    axis.axis_value = 0.0
    if not await focus_ready("mapped stick neutral"): return
    held_axis = false
    Input.parse_input_event(axis)
    await process_frame
    if not await inert("stick neutral"): return
    if not await settle(): return
    if not await face_npc(): return
    # Last fresh direction wins; release falls back to the remaining held direction.
    if not await key(KEY_J, true): return
    if not await rest(0.25): return
    if not await key(KEY_A, true): return
    if not require(state().movementDirection == "Left", "newest direction wins"): return
    if not await key(KEY_A, false): return
    if not require(state().movementDirection == "Down", "release falls back to held direction"): return
    if not await key(KEY_J, false): return
    if not await settle(): return
    if not await face_npc(): return
    # Leave facing west, then return facing the NPC.
    if not await tap(KEY_A): return
    if not await settle(): return
    if not require(not state().help.contains(": Talk"), "turning away removes hint"): return
    if not await tap(KEY_D): return
    if not await settle(): return
    if not require(state().help.contains(": Talk"), "turning back restores hint"): return
    if not await lifecycle("hidden"): return
    if not await face_npc(): return
    if not await lifecycle("paused"): return
    if not await face_npc(): return
    if not await native_focus(): return
    if not await face_npc(): return
    if not await tap(KEY_ENTER): return
    if not require(state().wait == "DialogueWait" and not state().help.contains(": Talk"), "Confirm enters existing target dialogue and removes hint"): return
    if not await key(KEY_J, true): return
    if not await tap(KEY_ENTER): return # Actual reveal, not acknowledgement.
    if not require(state().wait == "DialogueWait" and (state().visibleCharacters < 0 or state().visibleCharacters >= state().totalCharacters), "Confirm reveals current text"): return
    if not await tap(KEY_ENTER): return
    if not require(state().wait == "ChoiceWait", "next Confirm acknowledges into Yes/No"): return
    if not await text_ready(): return
    if not await inert("direction held through choice"): return
    if not await tap(KEY_X): return
    if not require(state().canWaitAtInput and state().flags.is_empty(), "No returns usable field without acceptance flag"): return
    if not await inert("dialogue completion with old direction held"): return
    if not await key(KEY_J, false): return
    var before := requests
    if not await tap(KEY_J): return
    if not await settle(): return
    if not require(requests == before + 1, "new mapped direction after dialogue"): return
    if not await face_npc(): return
    if not await tap(KEY_ENTER): return
    if not await text_ready(): return
    if not await tap(KEY_ENTER): return
    if not require(state().wait == "ChoiceWait", "second interaction choice"): return
    if not await key(KEY_J, true): return
    if not await text_ready(): return
    if not await tap(KEY_Z): return # Yes; holding a direction cannot repeat into this choice.
    if not await wait_until(func(): return state().map == "yard-map" and state().wait == "DialogueWait", "actual map transfer", 5.0): return
    var transferred := record("map-transfer-before-battle")
    if not require(transferred.map == "yard-map" and transferred.wait == "DialogueWait", "actual program transfer reaches pre-battle text"): return
    if not await inert("map transfer with old direction held"): return
    if not await text_ready(): return
    if not await tap(KEY_ENTER): return
    view = host.get_node_or_null("BattleSessionView")
    if not require(view != null, "battle owns completed handoff"): return
    if view != null:
        var battle := record("battle-control-with-old-key-held")
        if not await key(KEY_J, true, true): return
        if not await rest(): return
        if not require(state().stage == battle.stage and state().previewX == battle.previewX and state().previewY == battle.previewY, "old direction/echo cannot move or select in battle"): return
        if not await key(KEY_J, false): return
        if not await tap(KEY_ENTER): return
        if not require(state().stage == "ActionChoice", "fresh Confirm enters ordinary accepted battle menu"): return
        record("fresh-battle-menu")
        if not await tap(KEY_ESCAPE): return
        if not require(state().stage == "Movement", "fresh Cancel restores battle movement"): return
        record("fresh-battle-control")
    _finish()

func _finish() -> void:
    if ended: return
    ended = true
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    var terminal: Dictionary = state() if is_instance_valid(view) else {}
    file.store_string(JSON.stringify({"terminal":terminal, "rootFocused":root.has_focus(), "passed":failures.is_empty() and unavailable.is_empty(), "status":"environment-unavailable" if not unavailable.is_empty() else "failed" if not failures.is_empty() else "passed", "unavailable":unavailable, "elapsedMs":Time.get_ticks_msec()-began, "failures":failures, "unknowns":unknowns, "inputs":inputs, "samples":samples}, "  "))
    file.close()
    quit(2 if not unavailable.is_empty() else 0 if failures.is_empty() else 1)
