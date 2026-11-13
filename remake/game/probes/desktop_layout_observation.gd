extends "res://probes/host_ui_observation.gd"

# --resolution selects an actual client. Inputs enter Godot in client pixels, including
# entry letterboxing. Observe mounted production controls, never inject logical GUI events.
func pointer(at: Vector2, click := true) -> void:
    var motion := InputEventMouseMotion.new()
    motion.position = at
    motion.global_position = at
    motion.relative = Vector2(8, 4)
    Input.parse_input_event(motion)
    await frames()
    if click:
        for pressed in [true, false]:
            var button := InputEventMouseButton.new()
            button.position = at
            button.global_position = at
            button.button_index = MOUSE_BUTTON_LEFT
            button.pressed = pressed
            Input.parse_input_event(button)
            await frames()

func hit(control: Control) -> void:
    var drawn := root.get_final_transform() * control.get_global_transform_with_canvas() * Rect2(Vector2.ZERO, control.size)
    check(control.is_visible_in_tree() and Rect2(Vector2.ZERO, root.size).grow(1).encloses(drawn),
        str(control.name) + " visible inside client")
    samples.append({"control":str(control.name), "clientRect":rect(drawn)})
    await pointer(drawn.get_center())

func canvas(label: String, entry: bool) -> void:
    var design := Vector2(1920,1080)
    var client := Vector2(root.size)
    var expected := design if entry or client.x < design.x or client.y < design.y else client
    var ratio := minf(client.x / expected.x, client.y / expected.y)
    var projected := root.get_final_transform() * Rect2(Vector2.ZERO, expected)
    check(root.get_visible_rect().size == expected, label + " logical dimensions")
    check(projected.size.distance_to(expected * ratio) < 2 and projected.get_center().distance_to(client/2) < 2,
        label + " uniform centred physical projection")
    check(window().allowed and window().recovery == null, label + " admitted client")
    samples.append({"page":label, "client":str(root.size), "canvas":rect(root.get_visible_rect()),
        "projected":rect(projected), "physicalCell":96 * ratio})

func until_view(name_: String) -> bool:
    for i in 1500:
        if find(name_) != null: return true
        await process_frame
    check(false, "mounted " + name_)
    return false

func field_pages() -> void:
    if not await until_view("ExplorationSessionView"): return
    await frames(10)
    canvas("field", false)
    check(state().worldGeometry.cellSize == 96 and state().failure == null, "field cell and session")
    await key(KEY_ESCAPE)
    await key(KEY_ENTER)
    if not state().partyStatus.visible:
        check(false, "ordinary party member page")
        return
    var sheet := find("MemberLayer")
    var footer := sheet.find_child("PartyFooter", true, false) as Control
    var close := sheet.find_child("Close", true, false) as Control
    var prepare := sheet.find_child("Prepare", true, false) as Control
    var hint := sheet.find_child("PartyHint", true, false) as Control
    check(footer.get_global_rect().encloses(prepare.get_global_rect()) and
        footer.get_global_rect().encloses(close.get_global_rect()) and
        hint.get_global_rect().end.x <= prepare.get_global_rect().position.x and
        prepare.get_global_rect().end.x <= close.get_global_rect().position.x, "party footer actions fit to right of hint")
    for label in [hint, prepare, close]:
        var text_label := label as Label
        var bounds := Rect2(Vector2.ZERO,text_label.size).grow(1)
        var glyphs := Rect2()
        for index in text_label.text.length():
            var glyph := text_label.get_character_bounds(index)
            if glyph.has_area(): glyphs = glyphs.merge(glyph) if glyphs.has_area() else glyph
        check(glyphs.has_area() and bounds.encloses(glyphs), str(label.name) + " shaped glyphs fit")
    check(hint.get_theme_font_size("font_size") == 24, "party hint retains design type size")
    await hit(close)
    check(not state().partyStatus.visible, "scaled party Close hit")
    if state().fieldMenu.visible: await key(KEY_ESCAPE)
    await hit(find("SystemMenuEntry"))
    if find("SystemMenuView") == null:
        check(false, "scaled top-right Menu hit")
        return
    geometry("SystemMenuWindow")
    await hit(find("SystemSettings"))
    if find("SettingsView") == null:
        check(false, "pointer opens field Settings")
        return
    geometry("SettingsWindow")
    var size_before := root.size
    var before := settings()
    var frozen := state()
    root.size = Vector2i(1599,900)
    await frames(8)
    check(not window().allowed and window().recovery != null, "one pixel below width enters recovery")
    geometry("WindowRecoveryPanel")
    await key(KEY_ESCAPE)
    check(find("WindowQuitView") != null, "recovery Quit reachable")
    await key(KEY_ESCAPE)
    root.size = Vector2i(1600,899)
    await frames(5)
    check(window().recovery != null, "one pixel below height retains recovery")
    if size_before == Vector2i(1600,900) and not find("Recovery0").disabled:
        root.size = Vector2i(1280,720)
        await frames(5)
        await hit(find("Recovery0"))
        check(root.size == size_before, "Restore button returns a 1600x900 client")
        root.position = Vector2i(-20000,-20000)
    else:
        root.size = size_before
    await frames(8)
    canvas("restored settings", false)
    geometry("SettingsWindow")
    check(state().sessionId == frozen.sessionId and state().revision == frozen.revision and
        settings().selected == before.selected and settings().draft == before.draft,
        "recovery returns to same Settings selection, draft and paused session")
    await key(KEY_ESCAPE)
    check(selected_menu() == "Settings", "Settings returns to its Menu row")
    await hit(find("SystemSave"))
    if await wait_slots("save-slots"):
        geometry("SlotWindow")
        await key(KEY_ESCAPE)
        check(selected_menu() == "Save", "slot Back restores Save selection")
    await key(KEY_ESCAPE)

func battle_pages() -> void:
    if not await until_view("BattleSessionView"): return
    await frames(12)
    canvas("battle", false)
    check(state().stage == "Movement", "saved player movement")
    await key(KEY_ENTER)
    await frames(6)
    var menu: Dictionary = state().menu
    check(menu.visible, "battle action choices")
    for part in [menu.layout.description, menu.layout.actor, menu.layout.targetCard.rect]:
        var bounds := Rect2(part.x, part.y, part.width, part.height)
        check(root.get_visible_rect().grow(1).encloses(bounds), "battle HUD inside canvas")
    for label in menu.layout.descriptionLabels:
        var bounds := Rect2(label.rect.x,label.rect.y,label.rect.width,label.rect.height)
        var parent: Dictionary = menu.layout.description
        check(Rect2(parent.x,parent.y,parent.width,parent.height).grow(1).encloses(bounds), "battle command label fits panel")
    await key(KEY_ESCAPE)
    await key(KEY_ESCAPE)
    var inspection: Dictionary = state().inspection
    if inspection.get("page") != "Cursor":
        check(false, "battle inspection Cursor")
        return
    var map_view := find("BattleSessionView").find_child("MapViewport", true, false)
    for unit in inspection.units:
        var cell: Dictionary = unit.Position
        if unit.Actor.Value == state().actor: continue
        var board := map_view.get_node("Board") as Control
        var logical := board.get_global_transform_with_canvas() * ((Vector2(cell.X,cell.Y) + Vector2(0.5,0.5)) * 96)
        if not root.get_visible_rect().grow(-40).has_point(logical): continue
        var physical := root.get_final_transform() * logical
        await pointer(physical, false)
        var hover: Dictionary = state().inspection.hover
        check(hover.unit == unit.Actor.Value, "physical hover finds actual map unit")
        await pointer(physical)
        check(state().inspection.page == "Full", "same map hit opens unit details")
        await key(KEY_ESCAPE)
        check(state().inspection.cursor == cell, "Back retains inspected cell")
        samples.append({"mapCell":cell, "logicalPoint":str(logical), "clientPoint":str(physical)})
        return
    check(false, "second on-screen unit available")

func shop_pages() -> void:
    var start := OS.get_environment("SF2_HOST_UI_START")
    if host.call("StartPreVoyageLocalizationObservation", start) != "started":
        check(false, "controlled counter admitted")
        return
    await frames(12)
    canvas("counter", false)
    await key(KEY_UP)
    await frames(20)
    await key(KEY_ENTER)
    var story_seen := false
    for i in 850:
        var s := state()
        if s.shop.visible: break
        if s.dialoguePanel.visible and s.wait == "FieldTextWait" and not story_seen:
            story_seen = true
            samples.append({"text":s.textId, "wait":s.wait, "fontSize":s.dialoguePanel.fontSize, "body":s.dialoguePanel.body})
            check(s.textId == 158 and s.dialoguePanel.fontSize == 30, "merchant greeting retains body type size")
            var p: Dictionary = s.dialoguePanel.panel
            check(root.get_visible_rect().encloses(Rect2(p.x,p.y,p.width,p.height)), "story text inside canvas")
        if s.canWaitForText: await key(KEY_ENTER)
        await process_frame
    if not state().shop.visible:
        samples.append({"map":state().map, "stop":state().stop, "wait":state().wait, "text":state().textId, "failure":state().failure, "player":state().entities[0]})
        check(false, "counter opens real shop")
        return
    check(story_seen, "merchant greeting shown before stock")
    await frames(5)
    var buy: Dictionary = state().shop.services.commands[0].rect
    var row := Rect2(buy.x,buy.y,buy.width,buy.height)
    check(root.get_visible_rect().encloses(row), "shop Buy row in canvas")
    await pointer(root.get_final_transform() * row.get_center())
    check(state().shop.wait.Phase == 1, "Buy opens stock")
    await key(KEY_ESCAPE)
    await key(KEY_ESCAPE)
    check(not state().shop.visible and state().failure == null, "shop returns to caller")

func run() -> void:
    if output.is_empty() or case_name not in ["field","battle","shop"]:
        push_error("Select field, battle or shop with a local report")
        quit(2)
        return
    var requested := root.size
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.position = Vector2i(-20000,-20000)
    root.grab_focus()
    await frames(8)
    check(root.has_focus() and root.size == requested, "startup keeps requested client and accepts input")
    canvas("entry", true)
    if case_name == "shop":
        await shop_pages()
    else:
        var slot := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot1.json")
        var bytes := FileAccess.get_file_as_bytes(slot)
        if bytes.is_empty():
            check(false, "supply a populated disposable first slot")
            finish()
            return
        await hit(find("Start"))
        await key(KEY_E)
        var entry := find("SessionEntryView")
        var info := observation(entry, "ReadEntryObservationJson")
        if info.visibleCharacters >= 0 and info.visibleCharacters < info.totalCharacters: await key(KEY_ENTER)
        await hit(entry.find_child("Continue", true, false))
        if await wait_slots("continue-slots"):
            geometry("SlotWindow")
            await key(KEY_ENTER)
            if case_name == "battle": await battle_pages()
            else: await field_pages()
        check(bytes == FileAccess.get_file_as_bytes(slot), "slot bytes retained")
    host.queue_free()
    await frames(6)
    finish()
