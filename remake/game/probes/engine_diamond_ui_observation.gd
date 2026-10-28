extends SceneTree

# Bounded real-keyboard check of the ordinary host. No state injection or screenshots.
var view: Node
var host: Node
var failures: Array = []
var samples: Array = []

func _initialize() -> void:
    create_timer(150).timeout.connect(func():
        _check(false, "Observation exceeded 150 seconds")
        _finish())
    call_deferred("_run")

func _state(label: String = "") -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        view = host.get_node("BattleSessionView")
    var result: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    if not label.is_empty():
        samples.append({"label": label, "state": result})
    return result

func _check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _settle() -> Dictionary:
    var deadline := Time.get_ticks_msec() + 40000
    while Time.get_ticks_msec() < deadline:
        var state := _state()
        if state.failure != null:
            _check(false, "Consumer failure: " + str(state.failure))
            return state
        if not state.scene.visible and state.movement == null and state.stopReason == "PlayerInput":
            return state
        if state.scene.visible and state.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            var healing = state.scene.healing
            if healing == null or healing.AtTimedInput or healing.LogicalComplete:
                await _press(KEY_ENTER)
                continue
        await process_frame
    _check(false, "Bounded consumer did not settle")
    return _state()

func _inside(rect: Dictionary, size: Vector2i) -> bool:
    return rect.x >= 0 and rect.y >= 0 and rect.x + rect.width <= size.x + 1 and rect.y + rect.height <= size.y + 1

func _layout() -> void:
    for size in [Vector2i(960, 540), Vector2i(1280, 720), Vector2i(1920, 1080), Vector2i(640, 540), Vector2i(360, 640)]:
        root.size = size
        for frame in range(4):
            await process_frame
        var state := _state("layout-" + str(size))
        var layout: Dictionary = state.menu.layout
        _check(_inside(layout.diamond, size) and _inside(layout.description, size), "Menu and description fit " + str(size))
        _check(_inside(layout.actor, size) and _inside(layout.context, size), "Actor and context panels fit " + str(size))
        for label in layout.descriptionLabels:
            _check(_inside(label.rect, size), "Description text fits " + str(size))
        _check(_inside(state.mapViewport, size) and state.previewInsideMap, "Projected focus fits map " + str(size))
        for command in layout.commands:
            _check(_inside(command.rect, size), "Command fits " + str(size))
            _check(command.art.shape == "square" and abs(command.rect.width - command.rect.height) < 1, "Cardinal command is square")
            if OS.get_environment("SF2_DIAMOND_ORIGINAL_ICONS") == "1" and layout.diamondVisible:
                _check(command.art.mounted and command.art.width == 24 and command.art.height == 24 and command.art.filter == "Nearest", "Original texture is mounted at native size with nearest sampling")
        if size.x >= 700:
            _check(abs(layout.description.x + layout.description.width - size.x + 16) <= 1, "Description hugs right safe edge")
        else:
            _check(layout.description.y >= layout.diamond.y + layout.diamond.height, "Narrow description follows menu")
    root.size = Vector2i(1280, 720)
    for frame in range(4):
        await process_frame

func _run() -> void:
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    # The private tower entry runs its ordinary before-battle program and text.
    # No session checkpoint, frame count or battle setter skips that admission.
    while not _state().has("stage"):
        var field := _state()
        if field.get("failure") != null:
            var expected := OS.get_environment("SF2_DIAMOND_EXPECT_FAILURE")
            _check(not expected.is_empty() and str(field.failure) == expected, "Field consumer failure matches the selected negative case")
            _state("field-admission-failure")
            _finish()
            return
        if field.get("canWaitForText", false) or field.get("wait") == "ChoiceWait":
            await _press(KEY_ENTER)
        else:
            await process_frame
    await _settle()
    var initial := _state("initial")
    _check(OS.get_environment("SF2_DIAMOND_EXPECT_FAILURE").is_empty(), "Negative case must not admit battle control")
    _check(initial.failure == null and initial.stage == "Movement", "Ordinary host starts at movement")
    if initial.failure != null:
        _finish()
        return
    await _press(KEY_ENTER)
    await _layout()
    if OS.get_environment("SF2_DIAMOND_CASE") in ["equipment", "give", "discard"]:
        await _supplementary(initial)
    elif OS.get_environment("SF2_DIAMOND_CASE") == "private":
        await _private_menus(initial)
    elif OS.get_environment("SF2_DIAMOND_CASE") == "physical":
        await _physical(initial)
    else:
        await _menus(initial)
    _finish()

func _supplementary(initial: Dictionary) -> void:
    var extra: Array = {"equipment": [KEY_E, "EquipmentChoice"], "give": [KEY_G, "GiveItemChoice"],
        "discard": [KEY_R, "DiscardItemChoice"]}[OS.get_environment("SF2_DIAMOND_CASE")]
    await _press(extra[0])
    var opened_extra := _state("supplementary-" + extra[1])
    _check(opened_extra.stage == extra[1] and not opened_extra.menu.visible and opened_extra.failure == null,
        "Existing supplementary command remains reachable")
    await _press(KEY_ESCAPE)
    var back := _state("supplementary-cancelled")
    _check(back.stage == "ActionChoice" and back.menu.visible and back.mainSeed == initial.mainSeed and back.inventories == initial.inventories,
        "Supplementary cancel returns to diamond without spending resources")

func _private_menus(initial: Dictionary) -> void:
    for key in [KEY_UP, KEY_LEFT, KEY_RIGHT, KEY_DOWN]:
        await _press(key)
        if OS.get_environment("SF2_DIAMOND_ORIGINAL_ICONS") == "1":
            var commands: Array = _state().menu.layout.commands
            for command in commands:
                var suffix := ".selected" if command.selected else ".normal"
                _check(command.art.resource == "command-menu." + command.label.to_lower() + suffix, "Actual draw consumes the original normal/selected icon")
        await _press(KEY_ENTER)
        var opened := _state("private-choice-" + str(key))
        _check(opened.failure == null, "Private menu consumes actual admitted choices")
        if opened.menu.page != "Main":
            await _press(KEY_ESCAPE)
        if _state().menu.page != "Main":
            await _press(KEY_ESCAPE)
    await _press(KEY_ESCAPE)
    var cancelled := _state("private-menu-cancelled")
    _check(cancelled.stage == "Movement" and cancelled.mainSeed == initial.mainSeed and cancelled.inventories == initial.inventories,
        "Private menu navigation returns without spending resources")

func _physical(initial: Dictionary) -> void:
    await _press(KEY_UP)
    await _press(KEY_ENTER)
    var target := _state("attack-target")
    _check(target.menu.page == "Target" and target.target != null and target.failure == null, "Attack selects engine-admitted target")
    await _press(KEY_ESCAPE)
    _check(_state().menu.page == "Main" and _state().stage == "ActionChoice", "Attack back preserves action menu")
    await _press(KEY_ENTER)
    await _press(KEY_ENTER)
    await _settle()
    var done := _state("attack-committed")
    _check(done.mainSeed != initial.mainSeed and done.failure == null, "Actual attack consumer completes")

func _menus(initial: Dictionary) -> void:
    var open := _state("diamond")
    _check(open.menu.layout.diamondVisible and open.menu.command == 0, "Action choice shows four commands")
    await _press(KEY_ENTER)
    _check(_state().revision == open.revision and _state().menu.layout.detail.contains("Unavailable"), "Disabled attack explains without submitting")
    await _press(KEY_LEFT)
    await _press(KEY_ENTER)
    var magic := _state("magic-list")
    _check(magic.menu.page == "Magic" and magic.menu.layout.rows.size() == 2, "Magic lists learned ranks")
    await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    var disabled := _state("disabled-spell")
    _check(disabled.revision == magic.revision and disabled.menu.layout.detail.contains("Not enough MP"), "Unaffordable spell retains reason and does not mutate")
    await _layout()
    await _press(KEY_UP)
    await _press(KEY_ENTER)
    var selected := _state("spell-target")
    _check(selected.menu.page == "Target" and selected.target == initial.actor, "Spell starts with offered self target")
    await _press(KEY_TAB)
    await _press(KEY_ENTER)
    var rejected := _state("disabled-target")
    _check(rejected.revision == selected.revision and rejected.target == initial.actor and rejected.menu.layout.detail.contains("Out of range"), "Disabled target blocks commit of previous accepted target")
    await _layout()
    await _press(KEY_ESCAPE)
    _check(_state().stage == "ActionChoice" and _state().menu.page == "Magic", "Target back returns to spell list")
    await _press(KEY_ESCAPE)
    _check(_state().menu.page == "Main" and _state().menu.command == 1, "List back remembers cardinal focus")
    await _press(KEY_RIGHT)
    await _press(KEY_ENTER)
    var items := _state("items-list")
    _check(items.menu.page == "Items" and items.menu.layout.rows.size() == 4, "Inventory lists real slots")
    await _press(KEY_DOWN)
    await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    _check(_state().revision == items.revision and _state().menu.layout.detail.contains("This slot is empty"), "Empty slot stays inspectable and cannot select")
    await _press(KEY_UP)
    await _press(KEY_UP)
    await _press(KEY_ENTER)
    _check(_state().itemSlot == 0 and _state().target == initial.actor, "Item selects its own offered target")
    await _press(KEY_ESCAPE)
    await _press(KEY_ESCAPE)
    await _press(KEY_DOWN)
    await _press(KEY_ENTER)
    _check(_state().menu.page == "Commit" and _state().stage == "CommitReady", "Stay asks for confirmation")
    await _press(KEY_ESCAPE)
    await _press(KEY_ESCAPE)
    var cancel := _state("cancel-to-movement")
    _check(cancel.stage == "Movement" and cancel.mainSeed == initial.mainSeed and cancel.inventories == initial.inventories, "Nested cancellation consumes no resources")
    await _press(KEY_ENTER)
    await _press(KEY_H)
    await _press(KEY_ENTER)
    await _press(KEY_ENTER)
    await _settle()
    var healed := _state("heal-committed")
    _check(healed.actors[0].mp == initial.actors[0].mp - 3 and healed.actors[0].hp > initial.actors[0].hp, "Actual spell consumer spends MP and restores HP")
    _check(healed.menu.page == "Main" and healed.menu.candidate == null, "Next actor has fresh menu focus")
    await _press(KEY_ENTER)
    await _press(KEY_I)
    await _press(KEY_ENTER)
    await _press(KEY_ENTER)
    await _settle()
    var used := _state("item-committed")
    _check(used.inventories[1].items == [127.0, 127.0, 127.0, 127.0] and used.actors[1].hp == 12, "Another actor consumes its own item and heals")

func _finish() -> void:
    var file := FileAccess.open(OS.get_environment("SF2_DIAMOND_OUTPUT"), FileAccess.WRITE)
    if file == null:
        push_error("Observation output unavailable")
        quit(2)
        return
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    file.close()
    print("Diamond UI observation passed: ", failures.is_empty())
    quit(0 if failures.is_empty() else 1)
