extends "res://probes/host_ui_observation.gd"

# Ordinary Continue from an occupied first test slot. No capture or save publication.
func hints() -> Dictionary:
    return observation(host, "ReadSettingsHostObservationJson").hints

func field_chrome(label: String) -> void:
    var panel := find("ContextHint") as PanelContainer
    var entry := find("FieldMenuEntry") as Label
    var help := panel.find_child("Help", true, false) as Label
    var outer := panel.get_global_rect()
    var style := panel.get_theme_stylebox("panel")
    var inner := Rect2(outer.position + style.get_offset(), outer.size - style.get_minimum_size())
    var glyphs := shaped_bounds(entry)
    var help_glyphs := shaped_bounds(help)
    check(help.is_visible_in_tree() and hints().compact.Accelerate in help.text and
        inner.encloses(help_glyphs) and help.get_global_rect().encloses(help_glyphs),
        label + " mapped acceleration text fits beside the action")
    samples.append({"label":label, "entry":entry.text, "frame":rect(outer), "inner":rect(inner),
        "entryRect":rect(entry.get_global_rect()), "glyphs":rect(glyphs), "help":help.text,
        "helpRect":rect(help.get_global_rect()), "helpGlyphs":rect(help_glyphs), "accelerate":hints().compact.Accelerate})
    check(panel.is_visible_in_tree() and entry.is_visible_in_tree(), label + " visible frame and action")
    check(panel.is_ancestor_of(entry) and inner.encloses(entry.get_global_rect()) and glyphs.has_area() and inner.encloses(glyphs),
        label + " full action and shaped characters inside frame insets")
    check(entry.get_global_rect().encloses(glyphs) and hints().compact.Cancel in entry.text,
        label + " action contains active binding and glyphs")
    check(not help.is_visible_in_tree() or inner.encloses(help.get_global_rect()) and not
        help.get_global_rect().intersects(entry.get_global_rect()), label + " separate reading and action")
    check(entry.get_theme_font_size("font_size") == 24, label + " native hint size")

func shaped_bounds(entry: Label) -> Rect2:
    var glyphs := Rect2()
    for index in entry.text.length():
        var character := entry.get_character_bounds(index)
        if character.has_area():
            character = entry.get_global_transform() * character
            glyphs = glyphs.merge(character) if glyphs.has_area() else character
    return glyphs

func corner_check() -> void:
    var button := find("SystemMenuEntry") as Button
    check(button.is_visible_in_tree() and not button.disabled, "available system entry")
    var radii := {}
    check(button.get_theme_font("font").resource_path == "res://fonts/localization/NotoSansCJKsc-Regular.otf"
        and button.get_theme_font_size("font_size") == 24, "system entry inherits bundled type at hint size")
    for key_ in ["normal", "hover", "pressed", "disabled", "focus"]:
        var box := button.get_theme_stylebox(key_) as StyleBoxFlat
        if box == null:
            check(false, "missing " + key_ + " action style"); continue
        var corners := [box.corner_radius_top_left, box.corner_radius_top_right,
            box.corner_radius_bottom_left, box.corner_radius_bottom_right]
        radii[key_] = corners
        check(corners == [0,0,0,0], "square system entry " + key_)
    samples.append({"label":"system-entry", "text":button.text, "rect":rect(button.get_global_rect()), "corners":radii})

func hover_system_entry() -> void:
    var button := find("SystemMenuEntry") as Button
    var motion := InputEventMouseMotion.new()
    motion.position = button.get_global_rect().get_center()
    motion.global_position = motion.position; motion.relative = Vector2(5,3)
    Input.parse_input_event(motion)
    var deadline := Time.get_ticks_msec() + 2000
    while Time.get_ticks_msec() < deadline:
        await process_frame
        for label in root.find_children("*", "Label", true, false):
            var popup := label.get_parent() as PopupPanel
            if popup == null or not popup.visible or label.text != button.tooltip_text: continue
            var box := popup.get_theme_stylebox("panel") as StyleBoxFlat
            check(box != null and [box.corner_radius_top_left, box.corner_radius_top_right,
                box.corner_radius_bottom_left, box.corner_radius_bottom_right] == [0,0,0,0],
                "mounted binding tooltip uses square frame")
            var inner := Rect2(box.get_offset(), Vector2(popup.size) - box.get_minimum_size())
            check(label.get_theme_font_size("font_size") == 24 and inner.encloses(shaped_bounds(label)),
                "mounted tooltip encloses its 24px glyphs")
            return
    check(false, "hover opens the actual binding tooltip")

func switch_language(cancel_key: int) -> void:
    for i in range(5):
        if selected_menu() == "Settings": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    if settings().is_empty():
        check(false, "settled field Settings opens"); return
    var previous: String = settings().locale
    await select_setting("help")
    await key(KEY_ENTER)
    var help := find("ControlsHelpView")
    if help == null:
        check(false, "full controls Help opens"); return
    await key(KEY_LEFT)
    var page := observation(help, "ReadControlsHelpObservationJson")
    check(page.page == page.count - 1 and
        ("键盘" in page.body if previous == "zh-CN" else "keyboard" in page.body.to_lower()) and
        not ("七" in page.body or "7" in page.body), "name Help gives a short keyboard instruction")
    await key(cancel_key)
    for id in ["language", "apply"]:
        await select_setting(id); await key(KEY_ENTER)
    check(find("SettingsView") == null and selected_menu() == "Settings", "Apply restores host selection")
    await key(cancel_key)
    var changed: String = observation(host, "ReadSettingsHostObservationJson").applied.Language
    check(changed != previous, "Settings applied the other locale")
    field_chrome("after Apply " + changed)
    var caption := find("FieldMenuEntry") as Label
    check(("探索菜单" in caption.text) == (changed == "zh-CN"), "mounted caption uses applied language")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1764-"):
        push_error("Select SF2_HOST_UI_OUTPUT and a fresh issue1764-* test slot copy")
        quit(2); return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(12)
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN)
    var choice := find("Continue") as Button
    check(choice.is_visible_in_tree() and choice.get_node("FocusMarker").is_visible_in_tree(),
        "Witch focus marker follows Continue")
    check((choice.get_theme_stylebox("normal") as StyleBoxFlat).corner_radius_top_left == 0,
        "selected Continue uses square style")
    var reading := find("EntryActions") as ScrollContainer
    var hint := find("EntryHint") as Label
    check(reading.get_global_rect().end.y + 24 <= hint.get_global_rect().position.y,
        "Witch action list leaves room for the measured footer")
    await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): finish(); return
    geometry("SlotWindow")
    await key(KEY_ENTER)
    var deadline := Time.get_ticks_msec() + 18000
    while not state().get("canWaitAtInput", false) and Time.get_ticks_msec() < deadline:
        await frames(3)
    if not state().get("canWaitAtInput", false):
        check(false, "Continue reaches usable field"); finish(); return
    await frames(8)
    field_chrome("keyboard at 1080p")
    corner_check()
    var speed_key := InputMap.action_get_events("sf2_accelerate").filter(func(e): return e is InputEventKey)[0] as InputEventKey
    await edge(speed_key.keycode, true)
    check(state().acceleration.rate == 1.5, "advertised keyboard hold accelerates field")
    await edge(speed_key.keycode, false)
    check(state().acceleration.rate == 1, "release returns normal field speed")
    var keyboard_caption: String = (find("FieldMenuEntry") as Label).text
    await pad(JOY_BUTTON_MISC1)
    check(hints().device == "pad" and (find("FieldMenuEntry") as Label).text != keyboard_caption,
        "native pad event changes the mounted action cue")
    field_chrome("pad")
    for binding in InputMap.action_get_events("sf2_accelerate"):
        if binding is InputEventJoypadMotion:
            var axis := binding.duplicate() as InputEventJoypadMotion
            axis.device = 0; Input.parse_input_event(axis); await frames()
            check(state().acceleration.rate == 1.5, "mapped pad axis accelerates")
            axis = axis.duplicate(); axis.axis_value = 0; Input.parse_input_event(axis); await frames()
            check(state().acceleration.rate == 1, "pad neutral restores speed")
    root.size = Vector2i(2560,1440); await frames(8)
    field_chrome("pad after resize")
    root.size = Vector2i(1920,1080); await frames(8)
    await key(KEY_F12); field_chrome("keyboard restored")
    var entry := find("FieldMenuEntry") as Label
    await mouse(entry, true)
    root.size = Vector2i(2560,1440); await frames(4)
    root.size = Vector2i(1920,1080); await frames(4)
    await mouse(entry, false)
    check(not state().fieldMenu.visible, "relayout cancels pressed field entry even after return to original size")
    await mouse(entry, true)
    check(not state().fieldMenu.visible, "press waits for release")
    await mouse(entry, false)
    check(state().fieldMenu.visible, "complete field entry click opens commands")
    var cancel := InputMap.action_get_events("sf2_cancel").filter(func(e): return e is InputEventKey)[0] as InputEventKey
    await key(KEY_ENTER)
    var party_hint := find("PartyHint") as Label
    check(party_hint.get_theme_font_size("font_size") == 24 and
        root.get_visible_rect().encloses(party_hint.get_global_rect()), "member footer fits at fixed role size")
    check(not (find("ContextHint") as Control).is_visible_in_tree(), "member page retires field cues")
    check(party_hint.is_visible_in_tree() and (find("SystemMenuEntry") as Button).disabled,
        "member sheet owns input and disables system action")
    samples.append({"label":"member footer", "font":party_hint.get_theme_font_size("font_size"),
        "rect":rect(party_hint.get_global_rect()), "lines":party_hint.get_line_count()})
    await key(cancel.keycode)
    check(state().fieldMenu.visible, "Back from Member restores commands")
    await key(KEY_DOWN); await key(KEY_DOWN); await key(KEY_ENTER)
    await key(KEY_ENTER)
    var items: Dictionary = state().partyStatus.preparation
    check(items.get("stage") == "item-actions", "Item opens four labeled operations from the selected member")
    await key(cancel.keycode); await key(cancel.keycode)
    check(state().fieldMenu.visible and state().fieldMenu.index == 2, "Back restores the Item row")
    await key(cancel.keycode); await frames()
    check(not state().fieldMenu.visible and entry.is_visible_in_tree(), "mapped Back restores field entry")
    var system_entry := find("SystemMenuEntry") as Button
    await mouse(system_entry, true); await mouse(system_entry, false)
    check(selected_menu() == "Resume", "system menu opened at Resume")
    geometry("SystemMenuWindow")
    check(not (find("ContextHint") as Control).is_visible_in_tree(), "host cover retires field cues")
    await switch_language(cancel.keycode)
    check(find("SystemMenuView") == null and state().get("failure") == null, "system Back restores healthy field")
    await hover_system_entry()
    finish()
