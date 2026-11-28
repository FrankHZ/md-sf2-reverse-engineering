extends "res://probes/engine_world_viewport_observation.gd"

# Run the production Party view with sized authored inputs, or switch F5/F6 in the real preview.
func run() -> void:
    var sizes := OS.get_environment("SF2_PARTY_RESOLUTION").split("x")
    root.size = Vector2i(int(sizes[0]), int(sizes[1]))
    var preview := case_name == "party-trade"
    if preview:
        host = load("res://probes/previews/DesignPreview.tscn").instantiate()
    else:
        host = Node.new()
        host.set_script(load("res://probes/PartyCapacitySample.cs"))
    root.add_child(host)
    await frames(8)
    if not await focus(): return
    var entry := host.find_child("SessionEntryView", true, false)
    if entry != null and entry.has_method("ReadEntryObservationJson"):
        fail(str(JSON.parse_string(entry.call("ReadEntryObservationJson")).failure))
        return
    if not check(host.find_child("ExplorationSessionView", true, false) != null, "Mounted field presenter"): return
    if preview:
        await trade()
    else:
        await capacity()

func rect(data: Dictionary) -> Rect2:
    return Rect2(data.x, data.y, data.width, data.height)

func press_mouse(point: Vector2, button: MouseButton = MOUSE_BUTTON_LEFT) -> void:
    for pressed in [true, false]:
        var event := InputEventMouseButton.new()
        event.position = root.get_stretch_transform() * point
        event.global_position = event.position
        event.button_index = button
        event.pressed = pressed
        Input.parse_input_event(event)
        await frames(3)

func readable(label: Label, area: Rect2, font: int) -> bool:
    return label.is_visible_in_tree() and area.encloses(label.get_global_rect())         and label.get_theme_font_size("font_size") == font and label.get_visible_line_count() == label.get_line_count()         and label.get_minimum_size().x <= label.size.x and label.get_minimum_size().y <= label.size.y

func inventory(party: Dictionary) -> bool:
    var details := view.find_child("MemberDetails", true, false)
    var scroll := details.find_child("Items", true, false) as ScrollContainer
    var area := scroll.get_global_rect()
    var entries: Array = party.layout.items.entries
    if not check(entries.size() == 4 and scroll.clip_contents and scroll.scroll_vertical == 0,
            "Four actual slots in the clipped inventory at rest"): return false
    for row: Dictionary in entries:
        if not check(area.encloses(rect(row.rowRect)), "The whole slot is visible: " + str(row)): return false
        if row.icon.visible and not check(rect(row.rowRect).encloses(rect(row.icon.rect)) and row.icon.scale == 3,
                "Field identity icon fits its row"): return false
    for label in scroll.find_children("Entry*", "Label", true, false):
        if not check(readable(label, area, 34), "Read all item lines at the default large size: " + label.text): return false
    return true

func summary(party: Dictionary, values: Array) -> bool:
    var record_frame := view.find_child("RecordFrame", true, false) as PanelContainer
    var gold_frame := view.find_child("GoldFrame", true, false) as PanelContainer
    var labels := [record_frame.find_child("Kills", true, false), record_frame.find_child("Defeats", true, false), gold_frame.find_child("GoldLabel", true, false)]
    var numbers := [record_frame.find_child("KillsValue", true, false), record_frame.find_child("DefeatsValue", true, false), gold_frame.find_child("Gold", true, false)]
    var area := record_frame.get_global_rect()
    var gold_area := gold_frame.get_global_rect()
    var width := gold_area.size.x
    if not check(area.position.y == gold_area.position.y and area.size == Vector2(width * 2, gold_area.size.y),
            "Matching summary frames"): return false
    for i in range(3):
        var label: Label = labels[i]
        var number: Label = numbers[i]
        var cell := Rect2(area.position, gold_area.size) if i < 2 else gold_area
        if i == 1: cell.position.x += width
        var l := label.get_global_rect()
        var n := number.get_global_rect()
        if not check(readable(label, cell, 36) and readable(number, cell, 36) and number.text == values[i]
                and number.horizontal_alignment == HORIZONTAL_ALIGNMENT_RIGHT and label.horizontal_alignment == HORIZONTAL_ALIGNMENT_LEFT
                and l.position.y == n.position.y and n.size.y == maxf(48, number.get_minimum_size().y) and l.size.y == n.size.y
                and l.position.x - cell.position.x == 24 and cell.end.x - n.end.x == 24 and n.position.x - l.end.x == 12,
                "Shared summary metrics and value: " + number.text): return false
    var chinese := OS.get_environment("SF2_PARTY_LANGUAGE") == "zh-CN"
    if not check(labels[2].text == ("队伍金币" if chinese else "Party gold"), "Gold is labelled as a party resource"): return false
    return check(party.gold == values[2], "Gold observation matches the mounted amount")

func geometry_key(party: Dictionary) -> Array:
    return [party.layout.rect, party.layout.items.rect, party.layout.description.rect, party.rosterRect,
        party.recordRect, party.goldRect, party.hintRect,
        party.layout.items.entries.map(func(row): return row.rowRect)]

func capacity() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "authored field control"): return
    await tap(KEY_E)
    await frames(5)
    var baseline := state()
    var party: Dictionary = baseline.partyStatus
    if not check(party.memberLayerVisible and party.focusPane == 0, "Ordinary member shortcut"): return
    if not inventory(party) or not summary(party, ["9999", "4321", "999999"]): return
    var viewport := rect(baseline.viewport)
    if not check(viewport.encloses(rect(party.layout.rect)) and viewport.encloses(rect(party.goldRect))
            and rect(party.layout.description.rect).end.x < rect(party.layout.items.rect).position.x,
            "Sheet and summaries fit and the reader clears inventory"): return
    var sizes := OS.get_environment("SF2_PARTY_RESOLUTION").split("x")
    var expected := Vector2(int(sizes[0]), int(sizes[1]))
    if not check(Vector2(root.size) == expected and (root.get_stretch_transform() * viewport.size).distance_to(expected) < 1,
            "Window and canvas mapping at " + str(expected) + " (actual " + str(root.size) + ")"): return
    var name_label := view.find_child("MemberName", true, false) as Label
    if not check(name_label.text == "WWWWWWW" and readable(name_label, rect(party.layout.rect), 38),
            "A wide legal member name fits the sheet"): return
    var stable := geometry_key(party)
    var full: Array = party.projection[0].Inventory
    if not check(full.all(func(slot): return slot.Empty == false) and full[0].Equipped and full[0].Broken,
            "One legal full inventory with equipped damaged item"): return
    var first: Dictionary = party.layout.items.entries[0]
    var chinese := OS.get_environment("SF2_PARTY_LANGUAGE") == "zh-CN"
    if not check(first.icon.visible and first.icon.error == null and first.text.contains("reinforced training grip")
            and first.text.contains("已装备" if chinese else "Equipped") and first.text.contains("损坏" if chinese else "Broken"),
            "Long name with live icon and both labels: " + first.text): return
    samples.append({"label":"full-inventory", "window":str(root.size), "party":party})

    await tap(KEY_TAB)
    await tap(KEY_TAB)
    party = state().partyStatus
    if not check(party.focusPane == 2 and int(party.layout.itemSlot) == 0 and geometry_key(party) == stable
            and party.layout.description.fontSize == 36 and party.layout.description.text.length() > 0
            and party.layout.description.maximum <= 0,
            "Selected description fits at 36px in its reserved rectangle"): return
    var body := view.find_child("ItemDescription", true, false).find_child("Body", true, false) as Label
    if not check(readable(body, rect(party.layout.description.rect), 36), "Reader body measured and uncut"): return
    var row: Dictionary = party.layout.items.entries[3]
    await press_mouse(rect(row.rowRect).get_center())
    party = state().partyStatus
    if not check(int(party.layout.itemSlot) == 3 and geometry_key(party) == stable, "Last row pointer selects visible slot 4"): return
    await tap(KEY_DOWN)
    party = state().partyStatus
    if not check(int(party.layout.itemSlot) == 0 and geometry_key(party) == stable, "Keyboard wraps from slot 4 to slot 1"): return
    await tap(KEY_TAB)
    party = state().partyStatus
    if not check(party.focusPane == 0 and geometry_key(party) == stable, "Return to member focus without reflow"): return
    await tap(KEY_RIGHT)
    party = state().partyStatus
    var missing := "不可用" if chinese else "Unavailable"
    if not inventory(party) or not summary(party, [missing, missing, "999999"]): return
    if not check(int(party.index) == 1 and party.projection[1].Inventory.slice(1).all(func(slot): return slot.Empty),
            "Changing member preserves party gold and shows three empty slots"): return
    var member_geometry := geometry_key(party)
    await press_mouse(rect(party.layout.items.entries[3].rowRect).get_center())
    party = state().partyStatus
    if not check(int(party.layout.itemSlot) == 3 and party.layout.description.text.is_empty()
            and geometry_key(party) == member_geometry, "Inspect an empty slot without stale prose or reflow"): return
    await tap(KEY_ENTER)
    if not check(state().partyStatus.preparation.visible, "Prepare opens through the displayed member"): return
    await tap(KEY_ESCAPE)
    party = state().partyStatus
    if not check(int(party.index) == 1 and party.memberLayerVisible and int(party.layout.itemSlot) == 3
            and geometry_key(party) == member_geometry,
            "Back retains member and readable geometry"): return
    var paused := state()
    if not check(paused.revision == baseline.revision and paused.party == baseline.party, "Reading publishes no gameplay change"): return
    samples.append({"label":"missing-records-and-empty-slots", "party":party})
    await tap(KEY_ESCAPE)
    var returned := state()
    if not check(not returned.partyStatus.visible and returned.failure == null and returned.party == baseline.party
            and returned.gold == baseline.gold and returned.sessionId == baseline.sessionId,
            "Read-only Party returns with the same session resources"): return
    finish(true)

func shop_phase(phase: int) -> bool:
    return await wait_for(func(s): return s.get("shop", {}).get("visible", false) and int(s.shop.wait.Phase) == phase,
        "shop phase " + str(phase), 3.0)

func trade() -> void:
    if not await wait_for(func(s): return s.get("partyStatus", {}).get("memberLayerVisible", false), "preview F5"): return
    if OS.get_environment("SF2_PARTY_LANGUAGE") == "en":
        await tap(KEY_L)
        await frames(8)
    var initial := state()
    if not summary(initial.partyStatus, ["0", "0", str(int(initial.gold))]): return
    await tap(KEY_F6)
    if not await shop_phase(0): return
    await tap(KEY_ENTER)
    if not await shop_phase(1): return
    await tap(KEY_ENTER)
    if not await shop_phase(2): return
    await tap(KEY_ENTER)
    if not await shop_phase(4): return
    var price: int = state().shop.wait.Offer.Price
    await tap(KEY_ENTER)
    if not await shop_phase(5): return
    var bought := state()
    if not check(int(bought.shop.wait.Result) == 0 and bought.gold == initial.gold - price and bought.party != initial.party,
            "Real Buy result pays and adds an item"): return
    await tap(KEY_F5)
    if not await wait_for(func(s): return s.partyStatus.memberLayerVisible, "F5 after purchase"): return
    if not summary(state().partyStatus, ["0", "0", str(int(bought.gold))]): return
    await tap(KEY_F6)
    if not await shop_phase(0): return
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    if not await shop_phase(2): return
    await tap(KEY_ENTER)
    if not await shop_phase(3): return
    await tap(KEY_ENTER)
    if not await shop_phase(4): return
    var gain: int = state().shop.wait.Offer.Price
    await tap(KEY_ENTER)
    if not await shop_phase(5): return
    var sold := state()
    if not check(int(sold.shop.wait.Result) == 1 and sold.gold == bought.gold + gain and sold.party != bought.party,
            "Real Sell result pays and removes an item"): return
    await tap(KEY_F5)
    if not await wait_for(func(s): return s.partyStatus.memberLayerVisible, "F5 after sale"): return
    var after := state()
    if not summary(after.partyStatus, ["0", "0", str(int(sold.gold))]): return
    if not check(after.failure == null and after.party == sold.party and after.sessionId == initial.sessionId,
            "F5 uses the same live party after closing the shop"): return
    samples.append({"label":"same-session-preview-trade", "startingGold":initial.gold, "afterBuy":bought.gold, "afterSell":sold.gold})
    finish(true)
