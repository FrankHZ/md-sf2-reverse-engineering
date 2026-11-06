extends SceneTree

# Finite ordinary-host input/consumer observation; no gameplay state injection.
# Caller supplies an ignored report path and ordinary --authored-package/settings.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var events: Dictionary = {}
var submissions := 0
var route := OS.get_environment("SF2_BATTLE_NAVIGATION_ROUTE")

func _initialize() -> void:
    create_timer(35).timeout.connect(func(): push_error("Battle navigation observation timed out"); quit(2))
    call_deferred("run")

func check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func wait_ms(duration: int) -> void:
    var deadline := Time.get_ticks_msec() + duration
    while Time.get_ticks_msec() < deadline:
        await process_frame

func state(label: String = "") -> Dictionary:
    var s: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    s["submissions"] = submissions
    if not label.is_empty():
        var row := {"label": label}
        for key in ["revision", "actor", "stage", "stopReason", "previewX", "previewY",
                "failure", "mainSeed", "thinkingSeed", "queueCursor", "movement", "previewRect",
                "equipmentSlot", "giveMenu", "discardMenu", "returnSlot", "submissions"]:
            row[key] = s.get(key)
        row["menu"] = {"page": s.menu.page, "command": s.menu.command, "row": s.menu.row,
            "candidate": s.menu.candidate, "visible": s.menu.visible}
        samples.append(row)
    return s

func send(action: String, pressed: bool) -> void:
    var e: InputEvent = events[action].duplicate()
    if e is InputEventJoypadMotion or e is InputEventJoypadButton:
        e.device = 0
    if e is InputEventJoypadMotion:
        e.axis_value = events[action].axis_value if pressed else 0.0
    else:
        e.pressed = pressed
    Input.parse_input_event(e)
    await process_frame
    await process_frame

func tap(action: String) -> void:
    await send(action, true)
    await send(action, false)

func settle() -> Dictionary:
    var deadline := Time.get_ticks_msec() + 6000
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.movement == null and s.stage != null and s.stopReason in ["PlayerInput", "Rejected"]:
            return s
        await process_frame
    check(false, "Real battle movement/automatic consumer did not settle")
    return state("settle-timeout")

func restart() -> void:
    if is_instance_valid(host):
        root.remove_child(host)
        host.queue_free()
        await process_frame
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        push_error("Battle host unavailable: " + str(host.find_children("*", "Label", true, false).map(func(n): return n.text)))
        quit(2)
        await process_frame
        return
    await settle()
    submissions = 0
    view.connect("SessionResultObserved", func(json: String):
        var result: Dictionary = JSON.parse_string(json)
        if result.boundary == "submit": submissions += 1)
    events.clear()
    for action in ["up", "right", "down", "left", "confirm", "cancel", "menu", "equipment", "give", "discard"]:
        var mapped := InputMap.action_get_events("sf2_" + action)
        for e in mapped:
            if route == "pad" and e is InputEventJoypadButton or route == "axis" and e is InputEventJoypadMotion:
                events[action] = e
                break
        if not events.has(action):
            for e in mapped:
                if e is InputEventKey:
                    events[action] = e
                    break

func same_position(a: Dictionary, b: Dictionary) -> bool:
    return a.previewX == b.previewX and a.previewY == b.previewY

func movement_checks() -> void:
    await restart()
    var initial := state("movement-initial")
    if events.right is InputEventKey and events.right.keycode != KEY_RIGHT:
        var removed := InputEventKey.new()
        removed.keycode = KEY_RIGHT
        removed.pressed = true
        Input.parse_input_event(removed)
        await wait_ms(180)
        removed = InputEventKey.new()
        removed.keycode = KEY_RIGHT
        removed.pressed = false
        Input.parse_input_event(removed)
        check(same_position(initial, state()), "Removed default direction cannot bypass installed remapping")
    await send("right", true)
    var first := await settle()
    await wait_ms(330)
    await send("right", false)
    var moved := await settle()
    state("movement-repeated-released")
    check(moved.previewX > first.previewX, "Held direction repeats through real movement deliveries")
    check(moved.mainSeed == initial.mainSeed and moved.thinkingSeed == initial.thinkingSeed,
        "Provisional held movement preserves both RNG seeds")
    await wait_ms(240)
    check(same_position(moved, state()), "Release stops future movement")
    await restart()
    await send("left", true)
    await settle()
    await send("down", true)
    var turn_deadline := Time.get_ticks_msec() + 900
    while state().previewY == initial.previewY and Time.get_ticks_msec() < turn_deadline:
        await process_frame
    await send("down", false)
    var turned := await settle()
    state("turned-newest-direction-released")
    await wait_ms(240)
    check(same_position(turned, state()), "Releasing newest direction does not resume an older held direction")
    check(turned.previewY > initial.previewY, "Fresh turn selects the new direction")
    check(turned.failure == null, "Turn/release isolation is observed before any range rejection")
    await send("left", false)
    await restart()
    await send("right", true)
    await wait_ms(1700)
    var edge := state("range-rejected")
    check(edge.failure != null, "Range edge reaches the actual rejection")
    await wait_ms(300)
    check(submissions == edge.submissions, "Rejected hold stops rather than repeatedly submitting failures")
    await send("right", false)
    await tap("left")
    await settle()
    check(state().failure == null, "Fresh direction recovers after range rejection")

func menu_checks() -> void:
    await restart()
    await send("confirm", true)
    var duplicate: InputEvent = events.confirm.duplicate()
    if duplicate is InputEventKey:
        duplicate.pressed = true
        duplicate.echo = true
    elif duplicate is InputEventJoypadButton:
        duplicate.device = 0
        duplicate.pressed = true
    Input.parse_input_event(duplicate)
    await wait_ms(330)
    check(state().stage == "ActionChoice" and state().menu.page == "Main", "Held Confirm only opens actions")
    await send("confirm", false)
    await tap("left")
    await tap("confirm")
    var list := state("magic-list-entered")
    await send("down", true)
    var first := state()
    await wait_ms(160)
    var repeated := state("magic-list-held")
    check(first.menu.row != repeated.menu.row, "Held direction repeats list row navigation")
    await send("down", false)
    await wait_ms(200)
    check(state().menu.row == repeated.menu.row, "Release stops list navigation")
    check(state().mainSeed == list.mainSeed and state().thinkingSeed == list.thinkingSeed,
        "Menu navigation preserves RNG")
    await tap("confirm")
    check(state().menu.page == "Target", "Fresh Confirm enters target selection")
    await send("right", true)
    var target := state()
    await wait_ms(160)
    var next := state("target-held")
    check(target.menu.candidate != next.menu.candidate, "Held direction cycles actual offered target candidates")
    check(next.scene.visible == false, "Target navigation cannot commit an action")
    await send("cancel", true)
    await wait_ms(260)
    check(state().menu.page == "Magic", "Held Cancel returns once to the owning list")
    var returned := state("target-back-held-direction")
    await wait_ms(200)
    check(state().menu.row == returned.menu.row, "Old target direction cannot spill into its parent list")
    await send("right", false)
    await send("cancel", false)
    await tap("cancel")
    await tap("cancel")
    await settle()
    check(state().stage == "Movement", "Fresh cancellations return through Main to Movement")

func lifecycle_checks() -> void:
    await restart()
    await send("right", true)
    await settle()
    root.focus_exited.emit()
    var suspended := await settle()
    await wait_ms(240)
    root.focus_entered.emit()
    await wait_ms(240)
    check(same_position(suspended, state()), "Focus handler clears held input and prevents resumed movement")
    state("focus-return-held")
    await send("right", false)
    await tap("down")
    await settle()
    check(state().previewY > suspended.previewY, "Fresh direction resumes after focus return")
    await restart()
    await send("right", true)
    await settle()
    await tap("menu")
    var paused: Dictionary = JSON.parse_string(host.call("ReadWindowSupportObservationJson"))
    check(paused.battlePaused == "Disabled", "Ordinary system menu actually pauses its battle consumer")
    var covered := state("host-menu-paused")
    await wait_ms(250)
    check(state().revision == covered.revision, "Host pause prevents movement delivery and repeat")
    await tap("cancel")
    await send("right", false)
    await tap("cancel")
    var resumed := await settle()
    await wait_ms(250)
    check(same_position(resumed, state()), "Closing host menu cannot restart the previous direction")
    await restart()
    await send("right", true)
    await settle()
    view.hide()
    await wait_ms(160)
    view.show()
    var shown := await settle()
    await wait_ms(240)
    check(same_position(shown, state()), "Hide/show does not resume held navigation")
    await send("right", false)
    await restart()
    var original_actor = state().actor
    await tap("confirm")
    await send("down", true)
    await send("confirm", true)
    await wait_ms(260)
    check(state().menu.page == "Commit", "Held Confirm cannot commit Stay across the mode transition")
    await send("confirm", false)
    await tap("confirm")
    var next_actor := await settle()
    check(next_actor.actor != original_actor, "Stay actually advances to another actor before isolation is checked")
    state("next-actor-held-old-direction")
    await wait_ms(300)
    check(same_position(next_actor, state()) and state().stage == "Movement",
        "Old held direction cannot move or choose actions for the next actor")
    await send("down", false)

func auxiliary_checks(kind: String) -> void:
    await restart()
    await tap("confirm")
    await tap(kind)
    var initial := state("auxiliary-opened")
    check(initial.stage in ["EquipmentChoice", "GiveItemChoice", "DiscardItemChoice"], "Real auxiliary selection is admitted")
    await send("down", true)
    var first := state()
    await wait_ms(160)
    var repeated := state("auxiliary-held")
    check(repeated.revision > first.revision, "Held direction reaches the actual auxiliary selector again")
    check(repeated.equipmentSlot != first.equipmentSlot or repeated.giveMenu != first.giveMenu or
        repeated.discardMenu != first.discardMenu, "Repeat changes the projected auxiliary candidate")
    await send("down", false)
    var released := state()
    await wait_ms(200)
    check(state().revision == released.revision, "Release stops auxiliary navigation")
    await tap("cancel")
    check(state().stage == "ActionChoice" and state().inventories == initial.inventories and state().mainSeed == initial.mainSeed and
        state().thinkingSeed == initial.thinkingSeed, "Auxiliary Back preserves resources/RNG and returns to actions")

func run() -> void:
    root.size = Vector2i(1920, 1080)
    var auxiliary := OS.get_environment("SF2_BATTLE_NAVIGATION_AUX")
    if auxiliary.is_empty():
        await movement_checks()
        await menu_checks()
        await lifecycle_checks()
    else:
        await auxiliary_checks(auxiliary)
    root.remove_child(host)
    host.queue_free()
    await wait_ms(150)
    var file := FileAccess.open(OS.get_environment("SF2_BATTLE_NAVIGATION_OUTPUT"), FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures,
        "route": route, "samples": samples, "focusBoundary": "window signals; physical OS focus remains unclaimed"}, "  "))
    file.close()
    print("Battle navigation observation passed: ", failures.is_empty())
    quit(0 if failures.is_empty() else 1)
