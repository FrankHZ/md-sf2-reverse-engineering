extends SceneTree

var samples: Array = []
var failures: Array = []
var host: Node
var view: Node
var case_name := OS.get_environment("SF2_PRIVATE_EXPLORATION_CASE")
var door_events: Array = []
var fade_events: Array = []

func _fade_result(payload: String) -> void:
    fade_events.append_array(JSON.parse_string(payload).observations)

func _fade_case() -> void:
    view.connect("SessionResultObserved", _fade_result)
    var initial := _read("authored-sound-fade-pending")
    var token = initial.token
    _check(initial.wait == "PresentationWait" and not 7.0 in initial.flags, "authored SoundFade owns a pending token")
    await _press(KEY_ENTER)
    var early := _read("early-input")
    _check(early.token == token and early.wait == "PresentationWait" and not 7.0 in early.flags, "early input cannot complete the fade")
    var gained := false
    for tick in range(120):
        var state := _state()
        _check(state.failure == null and state.audio.error == null, "fade consumer remains available")
        if state.audio.fading and state.audio.musicVolumeDb < 0: gained = true
        if state.stop == "PlayerInput": break
        view.queue_redraw()
        await process_frame
    var completed := _read("authored-sound-fade-complete")
    _check(gained and not completed.audio.musicPlaying and not completed.audio.fading, "actual music gain and stop precede service completion")
    _check(completed.stop == "PlayerInput" and 7.0 in completed.flags, "authored continuation runs after completion")
    _check(completed.mainSeed == initial.mainSeed, "authored fade consumes no RNG")
    _check(completed.simulationTick - initial.simulationTick == fade_events.filter(func(event): return event.Kind == "simulation-tick").size(), "existing host ticks remain explicitly observed")
    _check(fade_events.filter(func(event): return event.Kind == "presentation-completed").size() == 1, "one completion for the token")
    for tick in range(8):
        view.queue_redraw()
        await process_frame
    var repeated := _read("fade-repeated-projection")
    _check(repeated.revision == completed.revision and repeated.audio.receipts.filter(func(receipt): return receipt.Command == 253).size() == 1, "projection cannot restart or replay253")
    samples.append({"label":"fade-events", "events":fade_events})
    view.disconnect("SessionResultObserved", _fade_result)

func _door_result(payload: String) -> void:
    var result: Dictionary = JSON.parse_string(payload)
    door_events.append_array(result.observations)

func _door_settle() -> Dictionary:
    for tick in range(600):
        var state := _state()
        if state.failure != null:
            _check(false, "ordinary door consumer has no host failure")
            return state
        var player := _entity(state, "entity-0")
        if state.stop == "PlayerInput" and not player.busy and not player.moving: return state
        if state.wait == "DialogueWait": await _press(KEY_ENTER)
        else: await process_frame
    _check(false, "ordinary door input returns within bounded observation")
    return _state()

func _door_case() -> void:
    view.connect("SessionResultObserved", _door_result)
    var initial := await _door_settle()
    var player := _entity(initial, "entity-0")
    _check(initial.map == "map-3" and player.x == 4 * 384 and player.y == 7 * 384, "controlled approach to existing Map3 door")
    await _press(KEY_DOWN)
    var opened := _read("door-input")
    var starts: Array = opened.audio.receipts.filter(func(receipt): return receipt.Command == 92 and receipt.Operation == "started")
    _check(starts.size() == 1, "ordinary door starts exactly one 92")
    if not starts.is_empty():
        _check(starts[0].TimerB == 203 and starts[0].Playing, "92 actually starts in inherited CB context")
    _check(opened.audio.sounds.any(func(sound): return sound.command == 92 and sound.playing), "92 has an actual playing voice")
    var settled := await _door_settle()
    player = _entity(settled, "entity-0")
    _check(player.x == 4 * 384 and player.y == 8 * 384 and settled.canWaitAtInput, "post-copy traversal reaches door cell and releases input")
    var stable_revision = settled.revision
    var stable_seed = settled.mainSeed
    for tick in range(120):
        view.queue_redraw()
        var projected := _state()
        _check(projected.revision == stable_revision and projected.mainSeed == stable_seed, "repeated projection and audio delivery add no gameplay work")
        if projected.audio.sounds.is_empty(): break
        await process_frame
    await _press(KEY_UP)
    await _door_settle()
    await _press(KEY_DOWN)
    var repeated := await _door_settle()
    _read("revisited-open-door")
    _check(door_events.filter(func(event): return event.Kind == "door-opened").size() == 1, "already opened door emits no duplicate event")
    _check(repeated.audio.receipts.filter(func(receipt): return receipt.Command == 92 and receipt.Operation == "started").size() == 1, "repeated projection and traversal do not replay 92")
    _check(repeated.audio.receipts.any(func(receipt): return receipt.Command == 92 and receipt.Operation == "finished"), "door voice reaches actual Finished")
    _check(repeated.failure == null and repeated.audio.error == null and repeated.canWaitAtInput, "ordinary input remains available")
    samples.append({"label":"door-events", "events":door_events})
    view.disconnect("SessionResultObserved", _door_result)

func _initialize() -> void:
    call_deferred("_run")

func _check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func _state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        view = host.get_node_or_null("BattleSessionView")
    return JSON.parse_string(view.call("ReadObservationJson"))

func _read(label: String) -> Dictionary:
    var state := _state()
    samples.append({"label": label, "state": state})
    return state

func _entity(state: Dictionary, identity: String) -> Dictionary:
    for entity in state.entities:
        if entity.id == identity:
            return entity
    return {}

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _run() -> void:
    if case_name == "sound-fade":
        # An explicitly supplied authored host reuses admitted PCM; no natural-route claim.
        host = load(OS.get_environment("SF2_FADE_FIXTURE_HOST")).new()
    else:
        host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var initial := _read("controlled-source-start")
    if not initial.has("mode"):
        _check(false, "Private ordinary exploration startup succeeds")
        _finish()
        return
    var identity: String = initial.sessionId
    if case_name == "sound-fade":
        await _fade_case()
        _finish()
        return
    if case_name == "map3-door":
        await _door_case()
        _finish()
        return
    if case_name == "authored-presentation":
        await _press(KEY_ENTER)
        var pending := _read("unavailable-presentation")
        _check(pending.failureKind == "AdapterError" and pending.wait == "PresentationWait", "Unavailable rendering reports an adapter failure with the original wait pending")
        await _press(KEY_ENTER)
        var retained := _read("acknowledgement-not-fabricated")
        _check(retained.revision == pending.revision and retained.token == pending.token and retained.flags == [7.0], "Actual Enter cannot acknowledge an unperformed presentation or publish its later flag")
        _finish()
        return
    if case_name == "map21-guard":
        var dialogue_count := 0
        for _frame in range(2200):
            var state := _state()
            if state.stop in ["PlayerInput", "Unsupported", "Faulted"]:
                break
            if state.wait == "DialogueWait":
                dialogue_count += 1
                _read("source-dialogue-request")
                await _press(KEY_ENTER)
            else:
                await process_frame
        _check(dialogue_count == 0, "Expected source dialogue requests were acknowledged by real input")
    var terminal := _read("terminal")
    _check(terminal.sessionId == identity, "The same common session owns every transition")
    match case_name:
        "map21-guard":
            _check(terminal.failure == null and terminal.stop == "PlayerInput" and 401.0 in terminal.flags, "Source guard program resumes after the physical player sprite service")
            _check(not 256.0 in terminal.flags, "Direct program start has no event caller to write F256")
            _check(_entity(terminal, "entity-128").x == 6 * 384 and _entity(terminal, "entity-0").facing == 3, "Guard moves and unassigned135 faces the physical player")
        _:
            _check(false, "Unknown external reference case")
    _finish()

func _finish() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file == null:
        push_error("External observer requires a writable output path")
        quit(2)
        return
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
