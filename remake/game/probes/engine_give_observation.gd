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
        "inventories":state.inventories, "equipment":state.equipmentActors,
        "menu":state.giveMenu, "give":state.give, "failure":state.failure})

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
        if state.give.active:
            if state.give.revealed: await key(KEY_ENTER)
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
    if before.give.active:
        check(board().give.visibleCharacters == before.give.visibleCharacters, label + " focus pauses reveal")
    root.emit_signal("focus_entered")
    view.hide()
    await key(KEY_ENTER)
    for frame in range(10): await process_frame
    check(board().revision == before.revision, label + " hide blocks input/delivery")
    view.show()
    sample(label + "-resumed")

func choose_target() -> void:
    await key(KEY_ENTER)
    await key(KEY_TAB)
    check(board().target == "receiver", "ordinary adjacent recipient selection")
    await key(KEY_ENTER)

func run() -> void:
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
    check(board().actor == "champion", "giver starts under ordinary control")
    sample("initial")
    await key(KEY_DOWN)
    await settle()
    await key(KEY_RIGHT)
    await settle()
    await key(KEY_ENTER)
    var before := board()
    await key(KEY_G)
    check(board().stage == "GiveItemChoice" and board().itemSlot == 0, "Give opens donor slots")
    await suspended("donor-choice")
    await key(KEY_DOWN)
    await key(KEY_ESCAPE)
    check(board().stage == "ActionChoice" and board().inventories == before.inventories, "donor cancellation is free")
    await key(KEY_G)
    await key(KEY_ENTER)
    await suspended("recipient-choice")
    await key(KEY_ESCAPE)
    check(board().stage == "GiveItemChoice", "recipient cancellation returns to item")
    await choose_target()
    check(board().stage == "GiveCommitReady" and str(board().giveMenu).contains("ends this action"), "normal confirmation states its cost")
    await key(KEY_ESCAPE)
    await key(KEY_ENTER)
    await suspended("normal-confirmation")
    check(board().inventories == before.inventories and board().mainSeed == before.mainSeed and board().thinkingSeed == before.thinkingSeed, "selection preserves custody and RNG")
    await key(KEY_ENTER)
    check(board().give.active and board().give.visible, "normal result visibly pending")
    check(str(board().give.text).contains("champion gave Practice blade to receiver"), "readable authored transfer feedback")
    # JSON.parse_string represents numbers as floats; Array equality is type-sensitive.
    check(inventory("champion") == [0.0,50.0,127.0,127.0] and inventory("receiver") == [189.0,0.0,0.0,50.0], "normal compaction/unequipped receipt")
    check(equipment("champion").attack == 20 and equipment("receiver").attack == 25, "normal donor refresh and unchanged recipient equipment")
    check(board().queueCursor == before.queueCursor and board().mainSeed == before.mainSeed, "delivery precedes action suffix without extra RNG")
    await suspended("normal-delivery")
    sample("normal-transfer")
    if (await settle()).is_empty():
        finish()
        return
    check(board().actor == "receiver", "next receiver control after normal Give")
    await key(KEY_ENTER)
    await key(KEY_SPACE)
    await key(KEY_ENTER)
    await settle()
    check(board().actor == "champion", "giver returns through ordinary round")
    await key(KEY_ENTER)
    await key(KEY_E)
    await key(KEY_UP)
    await key(KEY_ENTER)
    check(equipment("champion").attack == 25, "giver equips remaining blade")
    before = board()
    await key(KEY_G)
    await key(KEY_DOWN)
    check(board().itemSlot == 1, "second Give selects live compacted slot")
    await choose_target()
    check(board().stage == "GiveExchangeChoice" and str(board().giveMenu).contains("hands are full"), "full recipient presents return choices")
    await suspended("exchange-choice")
    await key(KEY_DOWN)
    await key(KEY_ESCAPE)
    check(board().stage == "GiveTargetChoice", "return-slot cancel returns to recipient")
    await key(KEY_ENTER)
    check(board().returnSlot == 0, "return slot remains exact equipped heavy blade")
    await key(KEY_ENTER)
    await key(KEY_ESCAPE)
    check(board().stage == "GiveExchangeChoice", "exchange confirmation cancels without custody change")
    await key(KEY_ENTER)
    check(board().inventories == before.inventories and board().mainSeed == before.mainSeed, "exchange selection leaves both loadouts intact")
    await key(KEY_ENTER)
    check(str(board().give.text).contains("received Heavy blade"), "readable exchange feedback")
    check(inventory("champion") == [0.0,61.0,127.0,127.0] and inventory("receiver") == [0.0,0.0,50.0,50.0], "source exchange ordering and both unequipped receipts")
    check(equipment("champion").attack == 20 and equipment("receiver").attack == 15, "both equipped removals refresh ATT")
    sample("full-exchange")
    if (await settle()).is_empty():
        finish()
        return
    check(board().actor == "receiver", "exchange returns receiver control")
    await key(KEY_ENTER)
    await key(KEY_E)
    await key(KEY_UP)
    check(board().equipmentSlot == 3, "recipient selects newly received duplicate slot")
    await key(KEY_ENTER)
    check(equipment("receiver").attack == 20 and equipment("receiver").maximumRange == 2, "received weapon can equip with current base/range")
    sample("received-blade-equipped")
    await key(KEY_I)
    await key(KEY_ENTER)
    if (await settle()).is_empty():
        finish()
        return
    check(board().actors.filter(func(a): return a.id == "receiver")[0].hp == 110, "later ordinary Herb use works")
    check(inventory("receiver") == [0.0,50.0,178.0,127.0] and equipment("receiver").attack == 20, "use compaction retains received equipped blade")
    check(board().actor == "champion", "continued next-round control")
    await key(KEY_ENTER)
    await key(KEY_E)
    await key(KEY_UP)
    await key(KEY_ENTER)
    check(equipment("champion").attack == 30, "exchanged heavy blade equips from current base")
    await key(KEY_ESCAPE)
    await settle()
    await key(KEY_DOWN)
    await settle()
    await key(KEY_ESCAPE)
    await settle()
    check(board().stage == "Movement" and equipment("champion").attack == 30, "later movement/cancel retains equipment and usable control")
    check(events.filter(func(e): return e.Kind == "give-committed").size() == 2, "exactly two Give publications")
    sample("continued-control")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"standalone authored Give/recipient-full exchange; ordinary input; modern result text; no original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
