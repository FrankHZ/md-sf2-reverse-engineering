extends SceneTree

# Actual mapped input and native geometry only; controlled initial data is supplied by the caller.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var began := Time.get_ticks_msec()
var mode := OS.get_environment("SF2_CHURCH_CASE")

func _initialize() -> void:
    call_deferred("run")

func check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func state(label: String = "") -> Dictionary:
    var s: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    if not label.is_empty():
        # Retain semantic boundaries, not per-frame/full world dumps.
        samples.append({"label":label, "church":s.church, "gold":s.gold, "party":s.party,
            "revision":s.revision, "wait":s.wait, "failure":s.failure,
            "canWaitAtInput":s.canWaitAtInput, "cursor":s.cursor})
    return s

func frames(count: int = 4) -> void:
    for i in count: await process_frame

func key(code: Key, pressed: bool) -> void:
    var e := InputEventKey.new()
    e.keycode = code; e.pressed = pressed
    Input.parse_input_event(e)
    await frames(2)

func press(code: Key) -> void:
    await key(code, true); await key(code, false); await frames()

func pad(code: JoyButton) -> void:
    for pressed in [true, false]:
        var e := InputEventJoypadButton.new()
        e.device = 0; e.button_index = code; e.pressed = pressed
        Input.parse_input_event(e)
        await frames(3)

func point(rect: Dictionary) -> Vector2:
    return Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2)

func mouse(position: Vector2, pressed: bool, button: MouseButton = MOUSE_BUTTON_LEFT) -> void:
    var e := InputEventMouseButton.new()
    e.button_index = button; e.position = position; e.pressed = pressed
    Input.parse_input_event(e)
    await frames(2)

func click(rect: Dictionary) -> void:
    var pos := point(rect)
    var motion := InputEventMouseMotion.new()
    motion.position = pos; motion.relative = Vector2(7, 3)
    Input.parse_input_event(motion)
    await frames(2)
    await mouse(pos, true); await mouse(pos, false); await frames()

func geometry(label: String) -> Dictionary:
    await frames()
    var s := state(label)
    var church: Dictionary = s.church
    check(s.failure == null and church.visible, label + " visible without failure")
    var size := root.size
    for name in ["panelRect", "titleRect", "hintRect"]:
        var r: Dictionary = church[name]
        check(r.x >= 0 and r.y >= 0 and r.x + r.width <= size.x + 1 and r.y + r.height <= size.y + 1,
            label + " " + name + " inside viewport")
    for name in ["title", "hint"]:
        check(church[name + "MinimumHeight"] <= church[name + "Rect"].height + 1, label + " " + name + " wraps")
    if church.page == "services":
        var commands: Array = church.commands
        check(commands.size() == 4 and church.rows.size() == 1, "four icons plus independent Leave")
        check(commands[0].rect.y < commands[1].rect.y and commands[1].rect.x < commands[0].rect.x and
            commands[2].rect.x > commands[0].rect.x and commands[3].rect.y > commands[1].rect.y, "cardinal source slot mapping")
        check(church.selectionMinimumHeight <= church.selectionRect.height + 1, "full selected label wraps")
        for command in commands:
            check(command.rect.width == 96 and command.rect.height == 96, "unchanged integral icon geometry")
            check(command.art.mounted == (mode not in ["public", "low"]), "explicit private raster versus text fallback")
            if not command.art.mounted:
                check(command.art.captionMinimumWidth <= command.rect.width and command.art.captionMinimumHeight <= command.rect.height,
                    "fallback short caption fits its icon rectangle")
    else:
        for row in church.rows:
            check(row.rect.y >= church.titleRect.y + church.titleRect.height and
                row.rect.y + row.rect.height <= church.hintRect.y, label + " choices do not overlap text")
    return s

func phase(wanted: int, label: String) -> Dictionary:
    var s := await geometry(label)
    check(s.wait == "ChurchWait" and int(s.church.wait.Phase) == wanted, label + " typed phase")
    return s

func finish() -> void:
    var path := OS.get_environment("SF2_CHURCH_OUTPUT")
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify({"pass":failures.is_empty(), "case":mode, "elapsedMs":Time.get_ticks_msec()-began,
        "failures":failures, "samples":samples}, "\t")); file.close()
    host.queue_free(); await frames(4)
    quit(0 if failures.is_empty() else 1)

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host); await frames(8)
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        samples.append({"label":"startup-labels", "labels":host.find_children("*", "Label", true, false).map(func(n): return n.text)})
        check(false, "ordinary host starts exploration"); await finish(); return
    if mode == "bad-png":
        var bad := state("bad-png-stopped")
        check(bad.failure == "church-menu-resource-invalid" and not bad.church.visible, "malformed decoded image stops once")
        var revision = bad.revision
        await frames(40); await press(KEY_ENTER)
        bad = state("bad-png-retained")
        check(bad.revision == revision and bad.failure == "church-menu-resource-invalid", "error remains stable without per-frame retry")
        await finish(); return
    var initial := state("initial")
    await press(KEY_ENTER)
    var menu := await phase(0, "opened-by-interaction")
    if not failures.is_empty(): await finish(); return
    if mode == "real":
        await press(KEY_LEFT); await press(KEY_ENTER)
        var none := await phase(2, "real-healthy-cure")
        check(none.gold == initial.gold and int(none.church.wait.Result.Kind) == 0, "real no-applicable does not charge")
        await press(KEY_ENTER); await press(KEY_ESCAPE)
        for i in 240:
            if state().canWaitAtInput: break
            if state().get("canWaitForText", false): await press(KEY_ENTER)
            else: await process_frame
        check(state().canWaitAtInput, "real priest caller completes on Leave")
        await press(KEY_ENTER)
        for i in 240:
            if state().wait == "ChurchWait": break
            if state().get("canWaitForText", false): await press(KEY_ENTER)
            else: await process_frame
        await phase(0, "real-priest-reopened")
        await press(KEY_DOWN); await press(KEY_ENTER)
        for i in 300:
            if state().canWaitAtInput: break
            if state().get("canWaitForText", false): await press(KEY_ENTER)
            else: await process_frame
        var returned := state("real-save-caller-return")
        check(returned.wait != "ChurchWait" and returned.canWaitAtInput and returned.gold == initial.gold, "real Save caller settles with no payment")
        samples.append({"label":"real-shared-save", "persistence":JSON.parse_string(host.call("ReadPersistenceObservationJson"))})
        await finish(); return
    if mode == "low":
        await press(KEY_UP); await press(KEY_ENTER)
        await phase(1, "insufficient-offer")
        await press(KEY_ENTER)
        var low := await phase(2, "insufficient-result")
        check(low.gold == initial.gold and int(low.church.wait.Result.Kind) == 2 and low.party == initial.party, "insufficient funds cannot charge or recover")
        await press(KEY_ENTER); await press(KEY_ESCAPE)
        check(state().canWaitAtInput, "insufficient return is usable")
        await finish(); return
    await press(KEY_RIGHT); await press(KEY_ENTER)
    var refused := await phase(0, "promotion-refusal")
    check(refused.revision == menu.revision and refused.gold == initial.gold and refused.church.error != null, "unsupported service cannot mutate resources")
    await press(KEY_UP)
    check(state().church.error == null, "new focus clears refusal")
    # Pointer gesture cannot survive the real host pause boundary.
    var pos := point(state().church.commands[0].rect)
    await mouse(pos, true); await press(KEY_F10); await press(KEY_ESCAPE); await mouse(pos, false)
    check(state().revision == menu.revision and state().church.page == "services", "old paused click is rejected")
    await click(state().church.commands[0].rect)
    var offer := await phase(1, "fresh-pointer-raise-offer")
    check(offer.gold == initial.gold, "offer does not charge")
    await press(KEY_ESCAPE)
    var declined := await phase(2, "raise-declined")
    check(declined.gold == initial.gold, "decline does not charge")
    await press(KEY_ENTER); await press(KEY_UP); await press(KEY_ENTER)
    await phase(1, "raise-offer-again")
    await key(KEY_ENTER, true); await frames(12)
    var paid := await phase(2, "raise-paid-held-confirm")
    check(paid.gold == initial.gold - 10, "one typed offer charged exactly once")
    await frames(8)
    check(state().revision == paid.revision and state().gold == paid.gold, "held Confirm cannot pay or continue twice")
    await key(KEY_ENTER, false); await press(KEY_ENTER)
    await phase(0, "returned-service-focus")
    await pad(JOY_BUTTON_DPAD_LEFT); await pad(JOY_BUTTON_A)
    await phase(1, "pad-poison-offer")
    await pad(JOY_BUTTON_B)
    var poison_declined := await phase(2, "pad-poison-declined")
    check(poison_declined.gold == paid.gold, "pad refusal preserves money")
    await pad(JOY_BUTTON_A)
    await phase(1, "next-ordered-stun-offer")
    await pad(JOY_BUTTON_A)
    var cured := await phase(2, "stun-paid")
    check(cured.gold == paid.gold - 20, "ordered stun cost")
    await press(KEY_ENTER)
    await phase(0, "cure-return")
    await press(KEY_UP); await press(KEY_ENTER)
    var none := await phase(2, "no-applicable-raise")
    check(none.gold == cured.gold and int(none.church.wait.Result.Kind) == 0, "no applicable target preserves money")
    await press(KEY_ENTER)
    for size in [Vector2i(2560,1440), Vector2i(1920,1080)]:
        root.size = size; await frames(6)
        await press(KEY_DOWN); await geometry("save-help-" + str(size))
    # Cancel closes the real service caller; a fresh interaction enters again.
    await press(KEY_ESCAPE); await frames(8)
    var left := state("leave-return")
    check(left.canWaitAtInput and left.wait != "ChurchWait" and left.gold == cured.gold, "Leave returns usable field")
    await press(KEY_ENTER); await phase(0, "reopened")
    await press(KEY_DOWN); await press(KEY_ENTER)
    for i in 60:
        if state().wait != "ChurchWait": break
        await process_frame
    var save := state("church-save-caller-return")
    check(save.wait != "ChurchWait" and save.gold == cured.gold, "Save closes caller without payment")
    var persistence: Dictionary = JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    check(persistence.panel == "error" and persistence.availability.Error == "save-profile-unavailable", "authored Save shares eligibility failure")
    await press(KEY_ESCAPE); await frames()
    check(state().canWaitAtInput and state().wait != "ChurchWait", "Save cancellation returns field without service wait")
    samples.append({"label":"shared-save-host", "persistence":persistence})
    await finish()
