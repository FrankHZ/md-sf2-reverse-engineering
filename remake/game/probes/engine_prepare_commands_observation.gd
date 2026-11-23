extends "res://probes/engine_world_viewport_observation.gd"

# Reads the production field presenter and sends ordinary keyboard, pointer and mapped pad events.
const IDS := ["Equip", "Give", "Use", "Magic", "Drop"]
const RESOURCES := ["item-menu.equip", "item-menu.give", "item-menu.use", "field-menu.magic", "item-menu.drop"]
const PAGES := ["equipment", "give-item", "use-item", "magic", "drop-item"]

var chinese := OS.get_environment("SF2_PREPARE_LANGUAGE") == "zh-CN"
var reduced := OS.get_environment("SF2_PREPARE_REDUCED") == "1"

func rect(bounds: Dictionary) -> Rect2:
    return Rect2(bounds.x, bounds.y, bounds.width, bounds.height)

func click(bounds: Dictionary, button: MouseButton = MOUSE_BUTTON_LEFT) -> void:
    var point := root.get_stretch_transform() * rect(bounds).get_center()
    for pressed in [true, false]:
        var event := InputEventMouseButton.new()
        event.position = point
        event.button_index = button
        event.pressed = pressed
        Input.parse_input_event(event)
        await frames(3)

func pad(button: JoyButton) -> void:
    for pressed in [true, false]:
        var event := InputEventJoypadButton.new()
        event.button_index = button
        event.pressed = pressed
        Input.parse_input_event(event)
        await frames(3)

func preparation() -> Dictionary:
    return state().partyStatus.preparation

func resources(s: Dictionary) -> Array:
    return [s.sessionId, s.revision, s.party, s.gold, s.flags, s.itemCustody]

func commands(s: Dictionary, index: int, mounted: bool) -> bool:
    var p: Dictionary = s.partyStatus.preparation
    if not check(p.visible and p.commands.visible and not p.page.visible and p.failure == null
            and p.index == index and p.commands.commands.map(func(row): return row.id) == IDS,
            "Five production Prepare commands and one active page"): return false
    var rows: Array = p.commands.list.rows
    var labels := ["装备", "交付／交换", "使用道具", "魔法", "丢弃"] if chinese else ["Equip", "Give / exchange", "Use item", "Magic", "Drop"]
    if not check(rows.map(func(row): return row.label) == labels,
            "Always-visible localized captions"): return false
    for i in rows.size():
        var row: Dictionary = rows[i]
        var bounds := rect(row.rect)
        var art: Dictionary = row.art
        if not check(row.enabled and row.selected == (i == index) and art.mounted == mounted and
                bounds.size.x == 528 and bounds.size.y >= 80 and rect(s.viewport).encloses(bounds) and
                bounds.encloses(rect(row.labelRect)) and row.minimumHeight <= row.labelRect.height,
                "Visible, enabled and complete hit row: " + IDS[i]): return false
        if mounted and not check(art.width == 24 and art.height == 24 and art.scale == 3 and art.filter == "Nearest" and
                art.resource in [RESOURCES[i] + ".normal", RESOURCES[i] + ".selected"] and
                (i == index or art.resource.ends_with(".normal")),
                "Actual resource and integral scale: " + IDS[i]): return false
    return true

func observe_frames() -> bool:
    var variants: Array = []
    var limit := create_timer(0.60)
    while limit.time_left > 0 and not ended:
        var art: Dictionary = preparation().commands.list.rows[0].art
        if not check(art.reducedFlash == reduced, "Mounted command uses the selected motion preference"): return false
        if art.resource not in variants:
            variants.append(art.resource)
        await frames(3)
    samples.append({"label":"equip-frames", "reducedFlash":reduced, "resources":variants})
    var selected := RESOURCES[0] + ".selected"
    return check(variants == [selected] if reduced else selected in variants and RESOURCES[0] + ".normal" in variants,
        "Selected frame holds with Reduced Flash and alternates in normal mode")

func prepare_flow() -> bool:
    var before := state()
    if not commands(before, 0, true): return false
    if not await observe_frames(): return false
    samples.append({"label":"prepare", "commands":before.partyStatus.preparation.commands})
    for i in IDS.size():
        var p := preparation()
        if i == 1:
            await click(p.commands.list.rows[i].labelRect)
        elif i == 3:
            await pad(JOY_BUTTON_DPAD_DOWN)
            if not commands(state(), i, true): return false
            await pad(JOY_BUTTON_A)
        else:
            if i > 0: await tap(KEY_DOWN)
            await tap(KEY_ENTER)
        p = preparation()
        if not check(p.stage == PAGES[i] and p.page.visible and not p.commands.visible and p.failure == null,
                "Command opens its production choices: " + IDS[i]): return false
        samples.append({"label":PAGES[i], "rows":p.page.rows.map(func(row): return row.text)})
        if i == 2:
            var actions := view.find_child("FieldActions", true, false)
            var choices := actions.find_children("Choice*", "Label", true, false).filter(func(label): return label.is_visible_in_tree())
            var colors := choices.map(func(label): return label.get_theme_color("font_color").to_html(false))
            if not check(colors.size() == 4 and colors[0] == "8e9bab" and colors.any(func(color): return color != "8e9bab"),
                    "Use retains muted unavailable focus and an available slot"): return false
            samples.append({"label":"use-availability", "colors":colors})
            await tap(KEY_ENTER)
            if not check(preparation().stage == "result" and resources(state()) == resources(before),
                    "Unavailable Use explains the refusal without submitting"): return false
            await tap(KEY_ESCAPE)
            if not check(preparation().stage == "use-item" and preparation().index == 0, "Refusal Back retains item focus"): return false
        if i == 3:
            await pad(JOY_BUTTON_B)
        else:
            await tap(KEY_ESCAPE)
        if not commands(state(), i, true): return false
    await tap(KEY_DOWN)
    if not commands(state(), 0, true): return false
    await click(before.partyStatus.preparation.commands.rect, MOUSE_BUTTON_RIGHT)
    return true

func run() -> void:
    root.size = Vector2i(1600,900) if chinese else Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(8)
    if not await focus(): return
    if case_name == "prepare-authored":
        if not check(host.find_child("ExplorationSessionView", true, false) != null, "Authored field presenter mounted"): return
        if not await wait_for(func(s): return s.canWaitAtInput, "authored field control"): return
        await tap(KEY_E)
        await tap(KEY_ENTER)
        var authored := state()
        samples.append({"label":"authored-prepare", "preparation":authored.partyStatus.preparation})
        if not commands(authored, 0, false): return
        await tap(KEY_ESCAPE)
        if not check(state().partyStatus.memberLayerVisible and not preparation().visible,
                "Authored text fallback returns to the member"): return
        await close_host()
        return

    var admitted: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_PREPARE_START"))
    if not check(admitted == "started", "Published field admission: " + admitted): return
    if not await wait_for(func(s): return s.canWaitAtInput, "field control"): return
    var initial := state()
    await tap(KEY_ESCAPE)
    await tap(KEY_ENTER)
    await tap(KEY_RIGHT)
    await tap(KEY_TAB)
    await tap(KEY_TAB)
    await tap(KEY_DOWN)
    var member: Dictionary = state().partyStatus
    if not check(member.memberLayerVisible and member.index == 1 and member.focusPane == 2 and member.layout.itemSlot == 1,
            "Member route retains a nondefault actor and slot"): return
    await click(member.prepareRect)
    if not await prepare_flow(): return
    var returned: Dictionary = state().partyStatus
    if not check(returned.memberLayerVisible and not returned.preparation.visible and returned.card.actor == member.card.actor and
            returned.focusPane == member.focusPane and returned.layout.itemSlot == member.layout.itemSlot and returned.rosterRect == member.rosterRect,
            "Pointer Back restores the same member, roster, pane and slot"): return
    await tap(KEY_ESCAPE)
    for entry in ["magic", "item-actions"]:
        await tap(KEY_DOWN)
        await tap(KEY_ENTER)
        await tap(KEY_ENTER)
        var p := preparation()
        if not check(p.visible and p.stage == entry and not p.commands.visible and p.failure == null,
                "Direct field entry keeps its existing caller: " + entry): return
        if entry == "item-actions":
            if not check(p.operations.commands.map(func(row): return row.id) == ["Use", "Give", "Equip", "Drop"] and
                    p.operations.commands.all(func(row): return row.art.mounted and row.art.resource.begins_with("item-menu.")),
                    "Item retains its own four original command bindings"): return
            await tap(KEY_UP)
            if not check(preparation().index == 3, "Item wraps directly to Drop"): return
        await tap(KEY_ESCAPE)
        await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    var final := state()
    if not check(final.failure == null and final.canWaitAtInput and not final.fieldMenu.visible and
            not final.partyStatus.visible and resources(final) == resources(initial),
            "All Back routes retain the original session and resources"): return
    await close_host()

func close_host() -> void:
    host.queue_free()
    await frames(5)
    finish(true)
