extends SceneTree

# Serial native witness. Starts from controlled data and drives the ordinary mapped inputs.
var host: Node
var view: Node
var samples: Dictionary = {}
var failures: Array[String] = []
var mode := OS.get_environment("SF2_ITEM_DESCRIPTION_CASE")
var long_field := OS.get_environment("SF2_ITEM_DESCRIPTION_LONG") == "1"

func _initialize() -> void:
    call_deferred("run")

func read() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func check(ok: bool, message: String) -> bool:
    if not ok:
        failures.append(message)
        push_error(message)
    return ok

func frames(count: int = 5) -> void:
    for i in count:
        await process_frame

func press(code: Key) -> void:
    for held in [true, false]:
        var event := InputEventKey.new()
        event.keycode = code
        event.pressed = held
        Input.parse_input_event(event)
        await frames(2)
    await frames()

func click(rectangle: Dictionary) -> void:
    for held in [true, false]:
        var event := InputEventMouseButton.new()
        event.button_index = MOUSE_BUTTON_LEFT
        event.position = rect(rectangle).get_center() * Vector2(root.size) / root.get_visible_rect().size
        event.pressed = held
        Input.parse_input_event(event)
        await frames(2)
    await frames()

func rect(bounds: Dictionary) -> Rect2:
    return Rect2(bounds.x, bounds.y, bounds.width, bounds.height)

func canvas_layer(node: Node) -> int:
    while node != null and not node is CanvasLayer:
        node = node.get_parent()
    return node.layer if node is CanvasLayer else 0

func description(label: String, detail: Dictionary, parent: Dictionary,
        adjacent: Array, expected: String) -> void:
    samples[label] = detail
    var area := rect(detail.rect)
    check(detail.visible and detail.fontSize == 30, label + " visible fixed body font")
    check(rect(parent).encloses(area), label + " description contained by its panel")
    check(root.get_visible_rect().encloses(area), label + " viewport fit")
    for bounds in adjacent:
        check(not area.intersects(rect(bounds)), label + " separated from controls")
    if long_field and label in ["party weapon", "field Equip weapon"]:
        check(detail.maximum > 0 and detail.scroll == 0, label + " starts at top of overflowing body")
    else:
        check(detail.minimumHeight <= area.size.y and detail.scroll == 0, label + " full prose fits")
    if expected.is_empty():
        check(detail.text.is_empty(), label + " empty reserved area")
    else:
        check(expected in detail.text, label + " effect/readiness: " + detail.text)
    check("Message unavailable" not in detail.text, label + " admitted localization")

func field_resources(state: Dictionary) -> Array:
    return [state.party, state.gold, state.flags, state.itemCustody]

func battle_resources(state: Dictionary) -> Array:
    return [state.inventories, state.gold, state.storyFlags, state.statuses, state.mainSeed, state.thinkingSeed]

func choice_geometry(page: Dictionary) -> Array:
    return [page.rect, page.readingRect, page.hintRect, page.backRect,
        page.rows.map(func(row: Dictionary): return row.rect), page.scroll, page.description.rect]

func party_flow(is_zh: bool) -> void:
    var baseline := field_resources(read())
    await press(KEY_ESCAPE)
    await press(KEY_ENTER)
    await press(KEY_TAB)
    await press(KEY_TAB)
    var party: Dictionary = read().partyStatus
    if not check(party.visible and party.focusPane == 2 and party.layout.itemSlot == 0, "Member / Items opens slot 1"):
        return
    var layout: Dictionary = party.layout
    description("party weapon", layout.description, layout.rect, [layout.items.rect, party.hintRect],
        "装备后" if is_zh else "When equipped")
    var item_area: Dictionary = layout.items.rect
    var description_area: Dictionary = layout.description.rect
    await click(layout.items.entries[1].rowRect)
    party = read().partyStatus
    check(party.layout.itemSlot == 1 and not party.preparation.visible, "complete row click inspects without opening actions")
    description("party consumable", party.layout.description, party.layout.rect,
        [item_area, party.hintRect], "10 点 HP" if is_zh else "10 HP")
    check(party.layout.items.scroll == layout.items.scroll, "fully visible row selection keeps inventory scroll")
    await press(KEY_DOWN)
    party = read().partyStatus
    description("party empty", party.layout.description, party.layout.rect,
        [item_area, party.hintRect], "")
    check(party.layout.itemSlot == 2 and party.layout.items.rect == item_area and
        party.layout.description.rect == description_area, "empty slot keeps reading and list bounds")
    var actor = party.card.actor
    var scroll = party.layout.items.scroll
    await press(KEY_RIGHT)
    party = read().partyStatus
    check(party.card.actor != actor and party.layout.itemSlot == 2 and party.layout.items.scroll == scroll and
        party.layout.description.rect == description_area, "change holder keeps selected position and scroll")
    await press(KEY_ENTER)
    await press(KEY_ENTER)
    party = read().partyStatus
    if not check(party.preparation.stage == "equipment", "existing field Equip choices"):
        return
    var page: Dictionary = party.preparation.page
    description("field Equip weapon", page.description, page.rect,
        [page.readingRect, page.hintRect, page.backRect], "装备后" if is_zh else "When equipped")
    if long_field:
        var before: Dictionary = read().partyStatus.preparation
        var read_before := read()
        await press(KEY_RIGHT)
        var after: Dictionary = read().partyStatus.preparation
        samples["field mapped read"] = after.page.description
        check(after.page.description.scroll > 0 and after.page.description.hint != null and
            after.index == before.index and after.pack.scroll == before.pack.scroll and
            choice_geometry(after.page) == choice_geometry(before.page) and
            read().revision == read_before.revision and field_resources(read()) == field_resources(read_before),
            "Field mapped reading moves only the description")
    await press(KEY_DOWN)
    var next: Dictionary = read().partyStatus.preparation.page
    description("field Equip consumable", next.description, next.rect,
        [next.readingRect, next.hintRect, next.backRect], "10 点 HP" if is_zh else "10 HP")
    check(choice_geometry(next) == choice_geometry(page) and next.description.hint == null,
        "Short field description removes the scroll hint and keeps the choices in place")
    await press(KEY_ESCAPE)
    await press(KEY_ESCAPE)
    await press(KEY_ESCAPE)
    check(read().fieldMenu.visible and read().fieldMenu.index == 0, "Party Back restores Member focus")
    await press(KEY_ESCAPE)
    check(not read().partyStatus.visible and not read().fieldMenu.visible and
        field_resources(read()) == baseline and read().failure == null, "field return preserves packs, gold and flags")

func wait_shop(entering: bool) -> bool:
    for i in 180:
        var state := read()
        if state.failure != null:
            return check(false, "service failure: " + str(state.failure))
        if (entering and state.shop.visible) or (not entering and state.canWaitAtInput and not state.shop.visible):
            return true
        if state.get("canWaitForText", false):
            await press(KEY_ENTER)
        else:
            await frames(3)
    return check(false, "shop wait did not settle: " + str(read().wait))

func shop_phase(phase: int) -> bool:
    var shop: Dictionary = read().shop
    return check(shop.visible and shop.wait.Phase == phase, "shop phase %s" % phase)

func focus_shop(index: int) -> void:
    for i in 10:
        if read().shop.index == index:
            return
        await press(KEY_DOWN)
    check(false, "shop focus %s" % index)

func shop_flow(is_zh: bool) -> void:
    var baseline := field_resources(read())
    await press(KEY_ENTER)
    if not await wait_shop(true):
        return
    await press(KEY_ENTER)
    if not shop_phase(1):
        return
    var page: Dictionary = read().shop.page
    description("buy Herb", page.description, page.rect, [page.readingRect, page.hintRect, page.backRect],
        "10 点 HP" if is_zh else "10 HP")
    await press(KEY_DOWN)
    var next: Dictionary = read().shop.page
    description("buy Seed", next.description, next.rect, [next.readingRect, next.hintRect, next.backRect],
        "20 点 HP" if is_zh else "20 HP")
    check(choice_geometry(next) == choice_geometry(page), "stock selection only changes marker and prose")
    await press(KEY_ENTER)
    var member: Dictionary = read().shop
    check(member.wait.Phase == 2 and member.page.description.text == next.description.text, "purchase recipient retains selected item")
    await press(KEY_DOWN)
    check(read().shop.pack.title != member.pack.title and read().shop.page.description.rect == member.page.description.rect,
        "purchase recipient changes pack without changing description bounds")
    await press(KEY_ENTER)
    if not shop_phase(4):
        return
    check(not read().shop.page.description.visible, "buy quote has its own confirmation")
    await press(KEY_ESCAPE)
    await press(KEY_ESCAPE)
    check(shop_phase(1) and read().shop.index == 1 and read().shop.page.description.text == next.description.text,
        "Back to the same selected stock item")
    await press(KEY_ESCAPE)

    for service in [1, 2]:
        await focus_shop(service)
        await press(KEY_ENTER)
        if not shop_phase(2):
            return
        await focus_shop(0)
        var owner: Dictionary = read().shop.pack
        await press(KEY_DOWN)
        check(read().shop.pack.title != owner.title, "browse different service holders")
        await press(KEY_UP)
        await press(KEY_ENTER)
        if not shop_phase(3):
            return
        await focus_shop(0)
        var held: Dictionary = read().shop.page
        var kind := "sell" if service == 1 else "repair"
        description(kind + " equipped", held.description, held.rect, [held.readingRect, held.hintRect, held.backRect],
            "装备后" if is_zh else "When equipped")
        await press(KEY_DOWN)
        next = read().shop.page
        description(kind + " consumable", next.description, next.rect, [next.readingRect, next.hintRect, next.backRect],
            "10 点 HP" if is_zh else "10 HP")
        check(choice_geometry(next) == choice_geometry(held), kind + " row selection preserves layout and scroll")
        await press(KEY_DOWN)
        next = read().shop.page
        description(kind + " empty", next.description, next.rect, [next.readingRect, next.hintRect, next.backRect], "")
        await press(KEY_ESCAPE)
        await press(KEY_ESCAPE)
        if not shop_phase(0):
            return
    await press(KEY_ESCAPE)
    check(await wait_shop(false) and field_resources(read()) == baseline,
        "Leave returns to field with packs, gold, custody and flags preserved")

func battle_flow(is_zh: bool) -> void:
    for i in 100:
        if read().get("stage") == "Movement":
            break
        await frames(3)
    if not check(read().get("stage") == "Movement", "settled player Battle movement"):
        return
    await press(KEY_ENTER)
    var baseline := battle_resources(read())
    await press(KEY_I)
    var menu: Dictionary = read().menu
    if not check(menu.page == "Items" and menu.layout.rows.size() == 4, "four battle item slots"):
        return
    var layout: Dictionary = menu.layout
    description("battle weapon", layout.reader, layout.description,
        [layout.list, layout.actor], "装备后" if is_zh else "When equipped")
    await press(KEY_DOWN)
    var ring: Dictionary = read().menu.layout
    description("battle damaged ring", ring.reader, ring.description,
        [ring.list, ring.actor], "道具会消失" if is_zh else "destroys it")
    check(ring.reader.rect == layout.reader.rect and ring.navigation == layout.navigation,
        "the long equipment description retains its panel and navigation")
    await press(KEY_DOWN)
    var item: Dictionary = read().menu.layout
    description("battle Herb", item.reader, item.description,
        [item.list, item.actor], "10 点 HP" if is_zh else "10 HP")
    check(item.list == layout.list and item.rowRects == layout.rowRects and item.scroll == layout.scroll and
        item.reader.rect == layout.reader.rect and item.navigation == layout.navigation,
        "Battle Item rows, Back, scroll and reading bounds are fixed")
    await press(KEY_F10)
    var system := host.find_child("SystemMenuView", true, false) as Control
    var reading := view.find_child("CommandMenu", true, false).find_child("ReadingArea", true, false)
    if check(system != null and system.is_visible_in_tree(), "system Menu covers the open Item page"):
        check(canvas_layer(system) > canvas_layer(reading), "host system prompt draws above item descriptions")
        await press(KEY_DOWN)
        check(read().menu.row == 2 and battle_resources(read()) == baseline, "covered item list has no input")
        await press(KEY_ESCAPE)
        check(read().menu.row == 2 and read().menu.layout.reader.rect == item.reader.rect,
            "Resume restores item focus and layout")
    await press(KEY_ENTER)
    if check(read().menu.page == "Target", "Item Confirm enters real target choice"):
        check(read().menu.layout.reader.text != item.reader.text, "target reader replaces selected-offer prose")
        await press(KEY_ESCAPE)
        check(read().menu.page == "Items" and read().menu.row == 2 and
            read().menu.layout.reader.text == item.reader.text, "target Back restores item focus")
    await press(KEY_DOWN)
    var empty: Dictionary = read().menu.layout
    description("battle empty", empty.reader, empty.description, [empty.list, empty.actor],
        "栏" if is_zh else "slot is empty")
    await press(KEY_ENTER)
    check(read().menu.page == "Items" and battle_resources(read()) == baseline, "empty slot submits no gameplay")
    await press(KEY_ESCAPE)
    check(read().menu.page == "Main" and read().menu.command == 2, "Battle Item Back retains root selection")
    await press(KEY_ESCAPE)
    check(read().stage == "Movement" and read().failure == null and battle_resources(read()) == baseline,
        "battle return preserves packs, gold, flags and seeds")

func run() -> void:
    var start := OS.get_environment("SF2_ITEM_DESCRIPTION_START")
    if not check(mode == "battle" or (mode in ["party", "shop"] and not start.is_empty()), "select case and controlled start"):
        finish()
        return
    var resolution := OS.get_environment("SF2_ITEM_DESCRIPTION_RESOLUTION").split("x")
    root.size = Vector2i(int(resolution[0]), int(resolution[1]))
    if long_field:
        var chinese := OS.get_environment("SF2_ITEM_DESCRIPTION_LANGUAGE") == "zh-CN"
        var catalog: Translation = load("res://localization/zh_CN.po" if chinese else "res://localization/en.po")
        var extra := "装备前请查看所选道具的说明。" if chinese else "Read the selected item description before equipping. "
        var key := "item-description.weapon"
        catalog.add_message(key, str(catalog.get_message(key)) + "\n" + extra.repeat(24))
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(10)
    root.grab_focus()
    var slot_path := OS.get_environment("SF2_SAVE_ROOT").path_join(
        OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot1.json")
    var slot_before := FileAccess.get_file_as_bytes(slot_path) if mode == "battle" else PackedByteArray()
    if mode == "battle":
        await press(KEY_ENTER)
        await press(KEY_E)
        await press(KEY_DOWN)
        await press(KEY_ENTER)
        for i in 180:
            var slots := host.find_child("SaveSlotView", true, false)
            if slots != null and slots.is_visible_in_tree():
                var history: Dictionary = JSON.parse_string(slots.call("ReadSaveObservationJson"))
                if history.state == "continue-slots":
                    samples.continue = history
                    break
            await frames()
        await frames()
        await press(KEY_ENTER)
    else:
        var admission: String = host.call("StartPreVoyageLocalizationObservation", start)
        if not check(admission == "started", "controlled admission: " + admission):
            samples.startup = host.find_children("*", "Label", true, false).filter(
                func(label: Label): return label.is_visible_in_tree()).map(func(label: Label): return label.text)
            finish()
            return
    var view_name := "BattleSessionView" if mode == "battle" else "ExplorationSessionView"
    for i in 100:
        view = host.find_child(view_name, true, false)
        if view != null:
            break
        await frames(3)
    if not check(view != null, "mounted " + view_name):
        samples.startup = host.find_children("*", "Label", true, false).filter(
            func(label: Label): return label.is_visible_in_tree()).map(func(label: Label): return label.text)
        finish()
        return
    await frames(15)
    var is_zh := OS.get_environment("SF2_ITEM_DESCRIPTION_LANGUAGE") == "zh-CN"
    samples.window = {"width": root.size.x, "height": root.size.y, "language": is_zh}
    if mode == "party":
        await party_flow(is_zh)
    elif mode == "shop":
        await shop_flow(is_zh)
    else:
        await battle_flow(is_zh)
        check(not slot_before.is_empty() and FileAccess.get_file_as_bytes(slot_path) == slot_before,
            "disposable save remains byte-identical")
    host.queue_free()
    await frames()
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_ITEM_DESCRIPTION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    output.close()
    quit(0 if failures.is_empty() else 1)
