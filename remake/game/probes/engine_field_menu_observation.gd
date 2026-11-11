extends SceneTree

# Actual mapped input and native geometry only; controlled initial data is supplied by the caller.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var back_key: Key = KEY_ESCAPE
var confirm_key: Key = KEY_ENTER
var began := Time.get_ticks_msec()
var mode := OS.get_environment("SF2_FIELD_MENU_CASE")

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
        samples.append({"label":label, "fieldMenu":s.fieldMenu, "partyStatus":s.partyStatus, "tick":s.simulationTick, "gold":s.gold, "party":s.party,
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

func finish() -> void:
    var path := OS.get_environment("SF2_FIELD_MENU_OUTPUT")
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify({"pass":failures.is_empty(), "case":mode, "elapsedMs":Time.get_ticks_msec()-began,
        "failures":failures, "samples":samples}, "\t")); file.close()
    host.queue_free(); await frames(4)
    quit(0 if failures.is_empty() else 1)

func geometry(label: String) -> Dictionary:
    await frames()
    var s := state(label)
    var menu: Dictionary = s.fieldMenu.menu
    check(s.fieldMenu.visible and s.failure == null, label + " usable root")
    for n in ["rect", "readingRect", "backRect"]:
        var rect: Dictionary = menu[n]
        check(rect.x >= 32 and rect.y >= 32 and rect.x + rect.width <= root.size.x - 31 and rect.y + rect.height <= root.size.y - 31, label + " bounds " + n)
    var commands: Array = menu.commands
    for i in range(commands.size()):
        var command: Dictionary = commands[i]
        check(command.rect.height >= 80 and command.rect.width == 528, "shared compact row geometry")
        if i > 0: check(command.rect.y >= commands[i-1].rect.y + commands[i-1].rect.height + 8, "vertical command axis")
        if command.art.mounted:
            check(command.art.width == 24 and command.art.height == 24 and command.art.scale == 3 and command.art.filter == "Nearest", "original pixels at integral3x")
        check(menu.list.rows[i].minimumHeight <= menu.list.rows[i].labelRect.height + 1,"full command caption wraps")

    return s

func focus_command(index: int) -> void:
    for i in range(5):
        if int(state().fieldMenu.index) == index: return
        await press(KEY_DOWN)
    check(false, "field command focus " + str(index))

func extended() -> void:
    await click(state().fieldMenu.entryRect)
    var menu := await geometry("pointer-entry")
    var revision = menu.revision
    await focus_command(0)
    await key(confirm_key, true); await frames(16)
    check(state().partyStatus.visible and not state().partyStatus.preparation.visible, "held root confirm opens only member page")
    await key(confirm_key, false); await press(back_key)
    check(state().fieldMenu.visible, "member returns to root")
    var rect: Dictionary = state().fieldMenu.menu.commands[1].rect
    await mouse(point(rect), true)
    root.emit_signal("focus_exited")
    await mouse(point(rect), false)
    check(state().fieldMenu.visible and not state().partyStatus.visible, "focus signal invalidates unfinished pointer press")
    if state().fieldMenu.menu.commands[1].enabled:
        await click(state().fieldMenu.menu.commands[1].rect)
        check(state().partyStatus.visible and state().partyStatus.focusPane == 1, "fresh complete click opens magic member")
        await press(back_key)
    else:
        await click(state().fieldMenu.menu.commands[0].rect)
        check(state().partyStatus.visible, "fresh complete click opens supported Member")
        await press(back_key)
    # Menu/Save remain unavailable under gameplay pages.
    await press(KEY_F10); await press(KEY_SPACE)
    check(state().fieldMenu.visible and host.find_child("SystemMenuView", true, false) == null, "system and save cannot cover gameplay menu")
    for size in [Vector2i(2560,1440), Vector2i(1920,1080)]:
        root.size = size
        await focus_command(2); await geometry("item-help-" + str(size))
    await pad(JOY_BUTTON_DPAD_DOWN)
    await geometry("pad-search-focus")
    await pad(JOY_BUTTON_B if mode != "swapped" else JOY_BUTTON_A)
    check(not state().fieldMenu.visible, "pad cancels menu")
    await pad(JOY_BUTTON_START)
    check(state().partyStatus.visible, "existing pad Party shortcut retained")
    await pad(JOY_BUTTON_B if mode != "swapped" else JOY_BUTTON_A)
    await press(KEY_F10)
    check(host.find_child("SystemMenuView", true, false) != null, "existing system Menu remains independently reachable")
    await press(back_key)
    await press(back_key); await focus_command(3)
    menu = state()
    if menu.fieldMenu.menu.commands[3].enabled:
        await press(confirm_key)
        var searched := state("menu-search")
        check(searched.wait == "FieldSearchWait" and not searched.fieldMenu.visible, "Search submits existing search, not Talk")
        await press(confirm_key)
        check(not state().fieldMenu.visible, "Search result owns input until acknowledged")
    else:
        var before := state()
        await press(confirm_key)
        check(state().fieldMenu.visible and state().revision == before.revision and state().party == before.party, "disabled Search submits nothing")
        await press(back_key)

func transaction() -> void:
    await press(KEY_E)
    var before := state("transaction-party")
    check(before.partyStatus.visible, "shortcut reads same party")
    if mode == "shortcut":
        await press(confirm_key)
        for i in 4: await press(KEY_DOWN)
    else:
        await press(back_key); await press(back_key); await focus_command(2); await press(confirm_key); await press(confirm_key)
        for i in 3: await press(KEY_DOWN)
    await press(confirm_key)
    var items := state("drop-items")
    check(items.partyStatus.preparation.stage == "drop-item", "both routes use same drop consumer")
    await press(KEY_DOWN); await press(confirm_key)
    check(state().partyStatus.preparation.stage == "confirm", "explicit item confirmation")
    await mouse(Vector2(500,500), true, MOUSE_BUTTON_RIGHT); await mouse(Vector2(500,500), false, MOUSE_BUTTON_RIGHT)
    check(state().partyStatus.preparation.stage == "drop-item" and state().partyStatus.preparation.index == 1, "pointer Back restores same item and focus")
    await press(confirm_key)
    var offered := state()
    await key(confirm_key, true); await frames(20)
    var paid := state("drop-held")
    check(paid.partyStatus.preparation.stage == "result" and paid.revision == offered.revision + 1, "held confirmation submits exactly once")
    await frames(20)
    check(state().revision == paid.revision and state().party == paid.party, "held input cannot repeat resource mutation")
    check(paid.gold == before.gold, "Drop preserves gold")
    await key(confirm_key, false)
    await press(confirm_key)
    await press(back_key); await press(back_key)
    if state().fieldMenu.visible: await press(back_key)
    check(not state().partyStatus.visible and not state().fieldMenu.visible, "transaction returns usable field")

func run() -> void:
    if mode == "remap": back_key = KEY_F6; confirm_key = KEY_F5
    if mode == "swapped": back_key = KEY_ENTER; confirm_key = KEY_ESCAPE
    root.size = Vector2i(1920,1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host); await frames(8)
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        samples.append({"labels":host.find_children("*", "Label", true, false).map(func(n): return n.text)})
        check(false, "ordinary host starts exploration"); await finish(); return
    if mode == "bad-png":
        var bad := state("bad-png")
        check(bad.failure == "field-menu-resource-invalid" and not bad.fieldMenu.visible, "bad native PNG stops presentation")
        var revision = bad.revision
        await frames(30); await press(confirm_key)
        check(state().revision == revision and state().failure == bad.failure, "no repeated decode or gameplay after native failure")
        await finish(); return
    if mode in ["transaction", "shortcut"]:
        await transaction(); await finish(); return
    await press(back_key)
    var opened := state("opened")
    check(opened.failure == null and opened.fieldMenu.visible, "Cancel opens gameplay list")
    var revision = opened.revision
    var tick = opened.simulationTick
    await frames(25)
    check(state().revision == revision and state().simulationTick == tick, "menu idle does not advance world")
    check(opened.fieldMenu.menu.commands.size() == 4, "four labeled commands")
    await press(confirm_key)
    var member := state("member")
    check(member.partyStatus.visible and not member.fieldMenu.visible, "Member uses existing Party Status")
    await press(back_key)
    check(state().fieldMenu.visible, "Party back returns to root")
    if mode == "real":
        for index in [1, 2]:
            await focus_command(index)
            var before := await geometry("unsupported-private-preparation")
            check(not before.fieldMenu.menu.commands[before.fieldMenu.index].enabled, "missing private preparation is disabled")
            await press(confirm_key)
            check(state().revision == revision and state().fieldMenu.visible, "disabled preparation preserves resources")
        await press(back_key); await extended(); await finish(); return
    await focus_command(1); await press(confirm_key)
    var magic := state("magic-member")
    check(magic.partyStatus.visible and magic.partyStatus.focusPane == 1, "Magic starts existing magic member pane")
    await press(confirm_key)
    check(state("magic-spells").partyStatus.preparation.stage == "magic", "Magic directly opens spells")
    await press(back_key); await press(back_key)
    check(state().fieldMenu.visible, "Magic unwinds to same root")
    await focus_command(2); await press(confirm_key); await press(confirm_key)
    check(state("item-actions").partyStatus.preparation.stage == "item-actions", "Item opens four existing operations")
    await press(back_key); await press(back_key)
    check(state().revision == revision and state().simulationTick == tick, "all provisional pages retain world revision and clock")
    var close_event := InputEventKey.new()
    close_event.keycode = back_key; close_event.pressed = true
    Input.parse_input_event(close_event)
    Input.flush_buffered_events()
    var immediate := state("close-edge")
    check(immediate.revision == revision and not immediate.fieldMenu.visible, "closing edge submits no command")
    await frames(2); await key(back_key, false)
    var closed := state("closed")
    check(not closed.fieldMenu.visible and not closed.partyStatus.visible and closed.party == opened.party and closed.gold == opened.gold, "cancelled menus preserve all state")
    await extended()
    await finish()
