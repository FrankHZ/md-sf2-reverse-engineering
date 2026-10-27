extends SceneTree

# Ordinary host keys and read-only state/projection. No session setters, synthetic
# completions, screenshots, source assets or original-game trajectory claim.
var host: Node
var view: Node
var failures: Array = []
var events: Array = []
var samples: Array = []
var seen_tokens: Dictionary = {}
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

func sample(label: String) -> void:
    var state := board()
    samples.append({"label":label, "actor":state.actor, "stage":state.stage,
        "mainSeed":state.mainSeed, "thinkingSeed":state.thinkingSeed,
        "previewX":state.previewX, "previewY":state.previewY, "gold":state.gold,
        "actors":state.actors, "inventories":state.inventories, "equipment":state.equipmentActors,
        "equipmentText":state.equipmentText, "equipmentSlot":state.equipmentSlot, "scene":scene()})

func settle() -> Dictionary:
    for frame in range(10000):
        await process_frame
        if Time.get_ticks_msec() - started > 170000:
            check(false, "native observation deadline")
            return {}
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false, "native session/presenter error:" + str(current))
            return {}
        var token = current.scene.waitToken
        if token != null and not seen_tokens.has(token):
            seen_tokens[token] = true
            samples.append({"label":"scene-boundary", "state":current})
        var state := board()
        if state.stage != null and state.stopReason == "PlayerInput" and current.movement == null:
            return state
        if current.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            await key(KEY_ENTER)
    check(false, "settle bound")
    return {}

func equipment_actor() -> Dictionary:
    return board().equipmentActors.filter(func(a): return a.actor == "champion")[0]

func attack() -> Dictionary:
    await key(KEY_ENTER)
    await key(KEY_F)
    await key(KEY_TAB)
    check(board().target == "guardian" and board().stage == "CommitReady", "ordinary guardian target")
    await key(KEY_ENTER)
    return await settle()

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
    var initial := await settle()
    if initial.is_empty():
        finish()
        return
    check(initial.actor == "champion", "bound actor has ordinary player control")
    sample("initial-carried-achilles")
    var after_carried := await attack()
    if after_carried.is_empty():
        finish()
        return
    check(after_carried.actors.filter(func(a): return a.id == "guardian")[0].hp == 500, "carried Achilles leaves Taros HP intact")
    check(events.any(func(e): return e.Kind == "ineffective"), "ordinary carried attack produces ineffective result")
    check(after_carried.actor == "champion", "next player turn is usable")
    await key(KEY_ENTER)
    var before := board()
    await key(KEY_E)
    check(board().stage == "EquipmentChoice" and board().equipmentSlot == 0, "equipment opens at equipped slot")
    await key(KEY_DOWN)
    check(board().equipmentSlot == 1, "ordinary next selects carried Achilles")
    check(equipment_actor().attack == 25 and equipment_actor().equippedSlot == 0, "preview is not authoritative equip")
    check(board().equipmentVisible and str(board().equipmentText).contains("> slot 2: Achilles · ATT 30 · range 1–1"), "visible prospective ATT and range")
    sample("achilles-preview")
    # Focus and hide affect adapter input only; no gameplay or authoritative setter.
    var preview_revision = board().revision
    root.emit_signal("focus_exited")
    await key(KEY_ENTER)
    check(board().revision == preview_revision, "focus suspension blocks equipment confirmation")
    root.emit_signal("focus_entered")
    view.hide()
    await key(KEY_DOWN)
    check(board().revision == preview_revision, "hidden view blocks equipment selection")
    view.show()
    await key(KEY_ESCAPE)
    check(board().stage == "ActionChoice", "equipment cancel returns to battle actions")
    check(board().inventories == before.inventories and board().mainSeed == before.mainSeed and board().thinkingSeed == before.thinkingSeed, "preview cancel preserves authoritative inventory and RNG")
    sample("cancelled-equipment")
    await key(KEY_E)
    await key(KEY_UP)
    check(board().equipmentSlot == 4, "ordinary previous selects admitted Unarmed")
    await key(KEY_ENTER)
    check(equipment_actor().attack == 20 and equipment_actor().maximumRange == 1 and equipment_actor().equippedSlot == 4, "unarmed commit refreshes ATT and range")
    check(board().stage == "ActionChoice", "equipment is not a consuming battle action")
    sample("unarmed-confirmed")
    await key(KEY_E)
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(equipment_actor().attack == 25 and equipment_actor().maximumRange == 2, "alternate weapon restores its own contribution and range")
    sample("ranged-weapon-confirmed")
    await key(KEY_E)
    await key(KEY_DOWN)
    var confirm_revision = board().revision
    await key(KEY_ENTER)
    check(board().revision == confirm_revision + 1 and board().stage == "ActionChoice", "equip confirms in one publication")
    check(equipment_actor().attack == 30 and equipment_actor().equippedSlot == 1, "live Achilles equips")
    check(board().queueCursor == before.queueCursor and board().round == before.round and board().mainSeed == before.mainSeed, "all equipment choices keep same turn and RNG")
    sample("achilles-confirmed")
    await key(KEY_ESCAPE)
    await settle()
    await key(KEY_S)
    await settle()
    await key(KEY_ESCAPE)
    await settle()
    check(equipment_actor().equippedSlot == 1 and equipment_actor().attack == 30, "movement cancel retains confirmed equipment")
    var after_equipped := await attack()
    if after_equipped.is_empty():
        finish()
        return
    check(after_equipped.actors.filter(func(a): return a.id == "guardian")[0].hp < 500, "ordinary equipped Achilles damages Taros")
    check(events.any(func(e): return e.Kind == "hp" and e.Actor.Value == "guardian"), "actual effective damage delivery")
    check(after_equipped.stage == "Movement" and after_equipped.actor == "champion", "continued player control after second attack")
    await key(KEY_S)
    await settle()
    await key(KEY_ESCAPE)
    await settle()
    sample("continued-usable-control")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"standalone authored uncursed ATT-only weapon equipment and Taros physical gate; ordinary input; no original presentation claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
