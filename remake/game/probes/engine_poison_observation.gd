extends SceneTree

# Ordinary input and actual state/text only; no gameplay injection or capture.
var host: Node
var view: Node
var failures: Array = []
var events: Array = []
var samples: Array = []
var started := Time.get_ticks_msec()
var last_sequence := 0

func _initialize() -> void:
    call_deferred("run")

func check(value: bool, message: String) -> void:
    if not value: failures.append(message)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if int(event.Sequence) > last_sequence:
            events.append(event)
            last_sequence = int(event.Sequence)

func board() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func scene() -> Dictionary:
    return JSON.parse_string(view.call("ReadSceneObservationJson"))

func actor(id: String) -> Dictionary:
    return board().actors.filter(func(a): return a.id == id)[0]

func key(code: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = code
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func sample(label: String) -> void:
    var state := board()
    samples.append({"label":label, "revision":state.revision, "actor":state.actor, "stage":state.stage,
        "cursor":state.queueCursor, "mainSeed":state.mainSeed, "thinkingSeed":state.thinkingSeed,
        "actors":state.actors, "status":state.status, "scene":scene().scene})

func until(phase: String) -> bool:
    for frame in range(10000):
        await process_frame
        var state := board()
        var shown := scene()
        if state.failure != null or shown.scene.error != null:
            check(false, "session or presenter error: " + str(state.failure) + "/" + str(shown.scene.error))
            return false
        if Time.get_ticks_msec() - started > 170000:
            check(false, "native deadline")
            return false
        if phase == "control" and state.stage == "Movement" and state.stopReason == "PlayerInput" and shown.movement == null:
            return true
        if shown.scene.phase == phase: return true
    check(false, "phase bound: " + phase)
    return false

func suspended() -> void:
    var before := board()
    root.emit_signal("focus_exited")
    await key(KEY_ENTER)
    for frame in range(40): await process_frame
    check(board().revision == before.revision and actor("champion").hp == 3, "focus pauses poison delivery and input")
    root.emit_signal("focus_entered")
    view.hide()
    await key(KEY_ENTER)
    for frame in range(40): await process_frame
    check(board().revision == before.revision and actor("champion").hp == 3, "hidden host pauses poison delivery and input")
    view.show()

func stay() -> void:
    await key(KEY_ENTER)
    await key(KEY_SPACE)
    await key(KEY_ENTER)

func run() -> void:
    root.size = Vector2i(1280, 720)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        check(false, "ordinary battle view exists")
        finish()
        return
    view.connect("SessionResultObserved", result_observed)
    if not await until("control"):
        finish()
        return
    check(board().actor == "champion", "initial ordinary control")
    sample("initial")
    await key(KEY_ENTER)
    await key(KEY_ESCAPE)
    check(actor("champion").hp == 3, "menu/cancel is free")
    await stay()
    if not await until("PoisonDamageMessage"):
        finish()
        return
    check(scene().scene.message == "champion takes 2 poison damage.", "real damage feedback")
    check(actor("champion").hp == 3, "notice precedes damage")
    sample("nonlethal-notice")
    await suspended()
    if not await until("control"):
        finish()
        return
    check(actor("champion").hp == 1 and actor("champion").status == 2, "fixed damage and retained poison")
    check(board().actor == "receiver", "one consumed slot reaches next actor")
    sample("nonlethal-completed")
    await stay()
    if not await until("PoisonExhaustedMessage"):
        finish()
        return
    check(scene().scene.message == "receiver is exhausted.", "real exhaustion feedback")
    check(actor("receiver").hp == 0 and actor("receiver").status == 2, "HP published before cleanup")
    sample("lethal-exhaustion")
    if not await until("FieldSpin"):
        finish()
        return
    sample("field-death")
    if not await until("control"):
        finish()
        return
    check(board().actor == "champion", "usable next-round control")
    check(actor("receiver").status == 0 and actor("receiver").defeats == 1, "one death cleanup/accounting")
    check(actor("champion").hp == 1, "no repeated poison on next control")
    await key(KEY_DOWN)
    await until("control")
    await key(KEY_ESCAPE)
    await until("control")
    check(actor("champion").hp == 1, "movement/cancel remains usable and free")
    check(events.filter(func(e): return e.Kind == "poison-hp").size() == 2, "exactly two poison ticks")
    check(events.filter(func(e): return e.Kind == "action-committed").size() == 2, "exactly two action publications")
    check(events.filter(func(e): return e.Kind == "death-cleanup").size() == 1, "exactly one cleanup")
    sample("continued-control")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"standalone poison; ordinary keys and actual delivery; no original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
