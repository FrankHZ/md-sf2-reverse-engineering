extends SceneTree

# Ordinary keys plus read-only state/labels. No gameplay injection or screenshots.
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

func inventory(actor: String) -> Array:
    return board().inventories.filter(func(a): return a.actor == actor)[0].items

func equipment(actor: String) -> Dictionary:
    return board().equipmentActors.filter(func(a): return a.actor == actor)[0]

func sample(label: String) -> void:
    var state := board()
    samples.append({"label":label, "revision":state.revision, "actor":state.actor, "stage":state.stage,
        "mainSeed":state.mainSeed, "thinkingSeed":state.thinkingSeed, "cursor":state.queueCursor,
        "previewX":state.previewX, "previewY":state.previewY, "actors":state.actors,
        "inventories":state.inventories, "equipment":state.equipmentActors, "custody":state.custody,
        "menu":state.discardMenu, "discard":state.discard, "failure":state.failure})

func settle() -> Dictionary:
    for frame in range(10000):
        await process_frame
        if Time.get_ticks_msec() - started > 170000:
            check(false, "native observation deadline")
            return {}
        var current := scene()
        var state := board()
        if current.failure != null or current.scene.error != null or state.failure != null:
            check(false, "native session/presenter error: " + str(current.failure) + "/" + str(state.failure))
            return {}
        if state.discard.active:
            if state.discard.revealed: await key(KEY_ENTER)
        elif state.stage != null and state.stopReason == "PlayerInput" and current.movement == null:
            return state
        elif current.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            await key(KEY_ENTER)
    check(false, "settle bound")
    return {}

func suspended(label: String) -> void:
    var before := board()
    root.emit_signal("focus_exited")
    await key(KEY_ENTER)
    for frame in range(10): await process_frame
    check(board().revision == before.revision, label + " focus blocks input/delivery")
    if before.discard.active:
        check(board().discard.visibleCharacters == before.discard.visibleCharacters, label + " focus pauses reveal")
    root.emit_signal("focus_entered")
    view.hide()
    await key(KEY_ENTER)
    for frame in range(10): await process_frame
    check(board().revision == before.revision, label + " hide blocks input/delivery")
    view.show()
    sample(label + "-resumed")

func same_action(before: Dictionary, label: String) -> void:
    var state := board()
    check(state.actor == "champion" and state.stage == "ActionChoice", label + " same actor/action")
    check(state.previewX == before.previewX and state.previewY == before.previewY, label + " provisional movement preserved")
    check(state.queueCursor == before.queueCursor and state.round == before.round, label + " turn retained")
    check(state.mainSeed == before.mainSeed and state.thinkingSeed == before.thinkingSeed, label + " no RNG")
    for i in range(state.actors.size()):
        for field in ["id", "x", "y", "anchorX", "anchorY", "hp", "mp", "exp", "status", "activationWord", "kills", "defeats", "lastTarget", "spells", "learned", "level", "defense", "control"]:
            check(state.actors[i][field] == before.actors[i][field], label + " preserved " + str(i) + "." + field)

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
    if (await settle()).is_empty():
        finish()
        return
    check(board().actor == "champion", "ordinary player control")
    await key(KEY_DOWN)
    await settle()
    await key(KEY_ENTER)
    var before := board()
    sample("action-preview")
    await key(KEY_R)
    check(board().stage == "DiscardItemChoice" and board().itemSlot == 0, "Drop opens exact held slots")
    check(str(board().discardMenu).contains("slot 4: Medical Herb"), "duplicate identity has separate visible slot")
    await key(KEY_DOWN)
    await key(KEY_DOWN)
    check(board().itemSlot == 2 and str(board().discardMenu).contains("protected: cannot discard"), "protected rare item is visible")
    await key(KEY_ENTER)
    check(board().failure == "discard-unsellable", "ordinary protected refusal")
    check(board().inventories == before.inventories and board().custody == before.custody, "refusal preserves custody")
    sample("protected-refusal")
    await key(KEY_ESCAPE)
    same_action(before, "refusal cancel")
    await key(KEY_R)
    await suspended("item-choice")
    await key(KEY_ENTER)
    check(board().stage == "DiscardCommitReady" and str(board().discardMenu).contains("Your action and movement preview remain available"), "confirmation states free continuation")
    await key(KEY_ESCAPE)
    same_action(before, "confirmation no")
    check(board().inventories == before.inventories and board().custody == before.custody, "cancel keeps both stores")
    await key(KEY_R)
    await key(KEY_ENTER)
    await key(KEY_ENTER)
    check(board().discard.active and board().discard.visible, "normal result visibly pending")
    check(str(board().discard.text).contains("champion discarded Medical Herb"), "readable normal feedback")
    check(inventory("champion") == [189.0,113.0,0.0,127.0], "normal selected slot compacted once")
    check(board().custody == before.custody, "normal discard leaves Deals unchanged")
    await suspended("normal-result")
    sample("normal-result")
    if (await settle()).is_empty():
        finish()
        return
    same_action(before, "normal completion")
    await key(KEY_R)
    await key(KEY_ENTER)
    await suspended("rare-confirmation")
    await key(KEY_ENTER)
    check(board().discard.active and str(board().discard.text).contains("Achilles Sword"), "readable equipped rare result")
    check(inventory("champion") == [113.0,0.0,127.0,127.0], "equipped rare compacted")
    check(inventory("receiver") == before.inventories.filter(func(a): return a.actor == "receiver")[0].items, "foreign loadout retained")
    check(equipment("champion").attack == 20 and equipment("champion").maximumRange == 1, "current unarmed ATT/range")
    check(board().custody.Deals["61"] == 15 and board().custody.Deals["113"] == 2, "rare enters existing Deals with independent count")
    check(board().custody.OnceFlags == before.custody.OnceFlags, "enemy once flags retained")
    await suspended("rare-result")
    sample("rare-result")
    if (await settle()).is_empty():
        finish()
        return
    same_action(before, "rare completion")
    await key(KEY_ESCAPE)
    await settle()
    check(board().stage == "Movement" and board().previewX == 1 and board().previewY == 2, "outer cancel returns movement")
    check(inventory("champion") == [113.0,0.0,127.0,127.0] and board().custody.Deals["61"] == 15, "outer cancel retains accepted discards")
    await key(KEY_ENTER)
    await key(KEY_SPACE)
    await key(KEY_ENTER)
    await settle()
    check(board().actor == "receiver", "later ordinary STAY consumes the action")
    await key(KEY_ENTER)
    await key(KEY_I)
    await key(KEY_I)
    await key(KEY_ENTER)
    await settle()
    check(board().actor == "champion", "next round remains usable")
    await key(KEY_DOWN)
    await settle()
    await key(KEY_ESCAPE)
    await settle()
    check(board().stage == "Movement" and equipment("champion").attack == 20, "later movement/cancel uses accepted loadout")
    check(events.filter(func(e): return e.Kind == "discard-committed").size() == 2, "exactly two discard publications")
    sample("continued-control")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"standalone authored discard; ordinary keys; actual state and result delivery; no original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
