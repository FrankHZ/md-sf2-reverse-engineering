extends "res://probes/engine_exploration_observation.gd"

# Small authored host witness. All gameplay arrives through mapped InputEvents.
var requests := 0
var last_sequence := -1
var unknowns: Array = []
var inputs: Array = []
var identity := ""

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
    _check(s.failure == null and s.sessionId == identity, "healthy same-session " + label)
    return s

func key(code: Key, pressed: bool, echo := false) -> void:
    inputs.append({"key":code, "pressed":pressed, "echo":echo, "frame":Engine.get_process_frames()})
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = pressed
    event.echo = echo
    Input.parse_input_event(event)
    await process_frame
    await process_frame

func rest(seconds := 0.35) -> void:
    await create_timer(seconds).timeout

func inert(label: String) -> void:
    var before := requests
    await rest()
    _check(requests == before, "no new Move after " + label)
    record(label)

func settle() -> void:
    for frame in range(180):
        if state().canWaitAtInput: return
        await process_frame
    _check(false, "bounded motion completion")

func player() -> Dictionary:
    for entity in state().entities:
        if entity.id == "traveler": return entity
    return {}

func face_npc() -> void:
    await settle()
    for attempt in range(8):
        var actor := player()
        var x := int(actor.x) / 384
        var y := int(actor.y) / 384
        if x == 1 and y == 1: break
        await _press(KEY_D if x < 1 else KEY_A if x > 1 else KEY_J if y < 1 else KEY_W)
        await settle()
    await _press(KEY_D)
    _check(state().help.contains(": Talk"), "facing the real event shows mapped Talk")

func lifecycle(kind: String) -> void:
    await key(KEY_J, true)
    await rest(0.3)
    if kind == "hidden": view.hide()
    else: paused = true
    var before := requests
    await rest()
    _check(requests == before, kind + " suspends movement")
    if kind == "hidden": view.show()
    else: paused = false
    await inert(kind + " return with old key held")
    await key(KEY_J, false)
    before = requests
    await _press(KEY_W)
    await settle()
    _check(requests == before + 1, kind + " fresh direction restores movement")

func native_focus() -> void:
    await key(KEY_J, true)
    await rest(0.3)
    var other := Window.new()
    other.hide()
    other.force_native = true
    other.transient = true
    other.title = "Exploration control focus witness"
    other.size = Vector2i(240, 100)
    root.add_child(other)
    other.show()
    other.grab_focus()
    await rest(0.15)
    if not other.has_focus() or root.has_focus():
        unknowns.append("Native window focus transfer unavailable")
    else:
        await inert("observed native focus loss")
    other.hide()
    root.grab_focus()
    await rest(0.15)
    other.queue_free()
    if not root.has_focus(): unknowns.append("Native root focus return unavailable")
    else: await inert("native focus return with old injected key held")
    await key(KEY_J, false)
    var before := requests
    await _press(KEY_W)
    await settle()
    _check(requests == before + 1, "fresh input after focus return")
    unknowns.append("Keys/pad events are injected; physical device release across OS focus loss is not observed")

func _run() -> void:
    node_added.connect(observe)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    root.grab_focus()
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        _check(false, "ordinary exploration host mounted")
        _finish()
        return
    identity = state().sessionId
    _check(state().canWaitAtInput, "authored field input")
    _check(state().help.contains("J") and state().help.contains(": Talk"), "mapping-derived initial prompt")
    await _press(KEY_S)
    _check(requests == 0, "old down binding removed")
    await key(KEY_J, true)
    await rest(0.55)
    _check(requests >= 3, "single mapped press walks multiple steps through ordinary motion waits")
    _check(not state().help.contains(": Talk"), "hint disappears after leaving target")
    await rest(0.4)
    var blocked := requests
    await rest(0.4)
    _check(requests == blocked, "collision stops at boundary without queued moves")
    await key(KEY_J, false)
    await inert("release")
    await face_npc()
    var axis := InputEventJoypadMotion.new()
    axis.device = 0
    axis.axis = JOY_AXIS_LEFT_Y
    axis.axis_value = 0.8
    var axis_before := requests
    Input.parse_input_event(axis)
    await rest(0.4)
    _check(requests >= axis_before + 2, "mapped stick deflection repeats movement")
    axis = InputEventJoypadMotion.new()
    axis.device = 0
    axis.axis = JOY_AXIS_LEFT_Y
    axis.axis_value = 0.0
    Input.parse_input_event(axis)
    await process_frame
    await inert("stick neutral")
    await settle()
    await face_npc()
    # Last fresh direction wins; release falls back to the remaining held direction.
    await key(KEY_J, true)
    await rest(0.25)
    await key(KEY_A, true)
    _check(state().movementDirection == "Left", "newest direction wins")
    await key(KEY_A, false)
    _check(state().movementDirection == "Down", "release falls back to held direction")
    await key(KEY_J, false)
    await settle()
    await face_npc()
    # Leave facing west, then return facing the NPC.
    await _press(KEY_A)
    await settle()
    _check(not state().help.contains(": Talk"), "turning away removes hint")
    await _press(KEY_D)
    await settle()
    _check(state().help.contains(": Talk"), "turning back restores hint")
    await lifecycle("hidden")
    await face_npc()
    await lifecycle("paused")
    await face_npc()
    await native_focus()
    await face_npc()
    await _press(KEY_ENTER)
    _check(state().wait == "DialogueWait" and not state().help.contains(": Talk"), "Confirm enters existing target dialogue and removes hint")
    await key(KEY_J, true)
    await rest(0.12)
    await _press(KEY_ENTER) # Actual reveal, not acknowledgement.
    _check(state().wait == "DialogueWait" and (state().visibleCharacters < 0 or state().visibleCharacters >= state().totalCharacters), "Confirm reveals current text")
    await _press(KEY_ENTER)
    _check(state().wait == "ChoiceWait", "next Confirm acknowledges into Yes/No")
    await inert("direction held through choice")
    await _press(KEY_X)
    _check(state().canWaitAtInput and state().flags.is_empty(), "No returns usable field without acceptance flag")
    await inert("dialogue completion with old direction held")
    await key(KEY_J, false)
    var before := requests
    await _press(KEY_J)
    await settle()
    _check(requests == before + 1, "new mapped direction after dialogue")
    await face_npc()
    await _press(KEY_ENTER)
    await rest(0.6)
    await _press(KEY_ENTER)
    _check(state().wait == "ChoiceWait", "second interaction choice")
    await key(KEY_J, true)
    await rest(0.4) # The choice question has its own real reveal boundary.
    await _press(KEY_Z) # Yes; holding a direction cannot repeat into this choice.
    for frame in range(360):
        if state().wait == "DialogueWait" or state().failure != null: break
        await process_frame
    var transferred := record("map-transfer-before-battle")
    _check(transferred.map == "yard-map" and transferred.wait == "DialogueWait", "actual program transfer reaches pre-battle text")
    await inert("map transfer with old direction held")
    await rest(0.6)
    await _press(KEY_ENTER)
    view = host.get_node_or_null("BattleSessionView")
    _check(view != null, "battle owns completed handoff")
    if view != null:
        var battle := record("battle-control-with-old-key-held")
        await key(KEY_J, true, true)
        await rest()
        _check(state().stage == battle.stage and state().previewX == battle.previewX and state().previewY == battle.previewY, "old direction/echo cannot move or select in battle")
        await key(KEY_J, false)
        await _press(KEY_ENTER)
        _check(state().stage == "ActionChoice", "fresh Confirm enters ordinary accepted battle menu")
        record("fresh-battle-menu")
        await _press(KEY_ESCAPE)
        _check(state().stage == "Movement", "fresh Cancel restores battle movement")
        record("fresh-battle-control")
    _finish()

func _finish() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures, "unknowns":unknowns, "inputs":inputs, "samples":samples}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
