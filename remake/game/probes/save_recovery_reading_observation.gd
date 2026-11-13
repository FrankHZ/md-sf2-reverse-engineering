extends "res://probes/host_ui_observation.gd"

# Ordinary Continue, then field Save on the restored copy. The launcher owns fixture setup.
var directory: String
var stress: bool
var missing_primary: bool
var backup: PackedByteArray
var initial_primary: PackedByteArray

func slot_bytes(name_: String) -> PackedByteArray:
    var path := directory.path_join(name_)
    return FileAccess.get_file_as_bytes(path) if FileAccess.file_exists(path) else PackedByteArray()

func check_files(repaired: bool) -> void:
    var primary_path := directory.path_join("slot1.json")
    check(FileAccess.file_exists(primary_path) == (repaired or not missing_primary), "primary presence")
    check(slot_bytes("slot1.json") == (backup if repaired else initial_primary), "exact primary bytes")
    check(slot_bytes("slot1.bak") == backup, "exact backup bytes")

func check_glyphs(control: Control, value: String) -> void:
    var font := control.get_theme_font("font")
    var missing := []
    for index in value.length():
        var code := value.unicode_at(index)
        if code > 32 and not font.has_char(code): missing.append(code)
    check(not font.allow_system_fallback and missing.is_empty(), str(control.name) + " bundled glyphs " + str(missing))

func label_bounds(label: Label) -> Rect2:
    var bounds := Rect2()
    for index in label.text.length():
        var character := label.get_character_bounds(index)
        if character.has_area():
            bounds = bounds.merge(character) if bounds.has_area() else character
    return label.get_global_transform() * bounds

func inspect_page(page: String) -> void:
    await frames(5)
    geometry("SlotWindow")
    var panel := (find("SlotWindow") as Control).get_global_rect()
    var reading := find("SlotReading") as ScrollContainer
    var viewport := reading.get_global_rect()
    var hint := find("SlotHint") as Label
    var hint_rect := hint.get_global_rect()
    check(root.size == Vector2i(1920,1080) and panel.encloses(hint_rect) and
        viewport.end.y <= hint_rect.position.y, page + " fixed footer in 1080p")
    check(hint_rect.encloses(label_bounds(hint)) and "向下阅读" in hint.text, page + " full mapped footer")
    check_glyphs(hint, hint.text)
    var rows := []
    var previous_end := -INF
    var bar := reading.get_v_scroll_bar()
    var overflow := maxi(0, ceili(bar.max_value - bar.page))
    for name_ in ["SlotTitle", "SlotMessage", "SlotDetail", "SlotCoverage"]:
        var label := find(name_) as Label
        if name_ == "SlotCoverage":
            check(label.is_visible_in_tree() == stress, page + " coverage only on fallback")
            if stress: check("英文回退" in label.text and "missing-translation: slot.matching" in label.text, page + " exact fallback reason")
        if not label.is_visible_in_tree(): continue
        var size_ := label.get_theme_font_size("font_size")
        if name_ in ["SlotDetail", "SlotCoverage"]: check(size_ == 24, name_ + " metadata 24px")
        check_glyphs(label, label.text)
        var bounds := label_bounds(label)
        var box := label.get_global_rect()
        check(bounds.has_area() and box.grow(1).encloses(bounds) and viewport.position.x <= bounds.position.x and
            bounds.end.x <= viewport.end.x and label.get_visible_line_count() == label.get_line_count(), name_ + " entire wrapped text")
        check(box.position.y >= previous_end, page + " reading order at " + name_)
        check(bounds.position.y >= viewport.position.y and bounds.end.y <= viewport.end.y + overflow + 1,
            name_ + " first and final glyph reachable")
        previous_end = box.end.y
        rows.append({"row":name_, "text":label.text, "size":size_, "lines":label.get_line_count(), "glyphs":rect(bounds)})
    var buttons := (find("SaveSlotView") as Control).find_children("*", "Button", true, false)
    previous_end = viewport.end.y
    for button: Button in buttons:
        var box := button.get_global_rect()
        var style := button.get_theme_stylebox("normal")
        var content := button.size - style.get_minimum_size()
        var text_size := button.get_theme_font("font").get_multiline_string_size(button.text, HORIZONTAL_ALIGNMENT_LEFT,
            content.x, button.get_theme_font_size("font_size"), -1, TextServer.BREAK_MANDATORY | TextServer.BREAK_WORD_BOUND | TextServer.BREAK_ADAPTIVE)
        check_glyphs(button, button.text)
        check(content.x + 1 >= text_size.x and content.y + 1 >= text_size.y and panel.encloses(box) and
            box.position.y >= viewport.end.y and box.end.y <= hint_rect.position.y,
            page + " full action and caption " + button.text)
        check(box.position.y >= previous_end, page + " explanation before " + button.text)
        previous_end = box.end.y
        rows.append({"action":button.text, "rect":rect(box)})
    check((buttons[slots().selected] as Button).get_node("FocusMarker").is_visible_in_tree(), page + " visible selection")
    samples.append({"page":page, "window":rect(panel), "viewport":rect(viewport), "footer":rect(hint_rect), "rows":rows,
        "scroll":reading.scroll_vertical})
    if stress and page == "recovery":
        var selection: int = slots().selected
        var recover_rect := (buttons[1] as Button).get_global_rect()
        var message := find("SlotMessage") as Label
        check(viewport.encloses(label_bounds(message)) and reading.scroll_vertical == 0, "read recovery from first glyph")
        for step in ceili(overflow / 112.0): await key(KEY_RIGHT)
        check(reading.scroll_vertical > 0 and slots().selected == selection and slots().state == "recover-backup",
            "scroll long details without activating recovery")
        check(viewport.encloses(label_bounds(find("SlotCoverage") as Label)), "last fallback glyph visible at end of reading")
        for step in ceili(overflow / 112.0): await key(KEY_LEFT)
        check(viewport.encloses(label_bounds(message)), "Left returns to backup identity")
        check(hint.get_global_rect() == hint_rect, "reading keeps footer fixed")
        await key(KEY_DOWN)
        check((buttons[1] as Button).get_global_rect() == recover_rect, "reading and selection keep Recover in place")
        await key(KEY_UP)

func open_continue() -> bool:
    var choice := find("Continue") as Button
    check(choice != null and choice.is_visible_in_tree(), "ordinary Witch menu")
    await key(KEY_DOWN); await key(KEY_ENTER)
    return await wait_slots("continue-slots")

func run() -> void:
    var space := OS.get_environment("SF2_SAVE_NAMESPACE")
    if output.is_empty() or OS.get_environment("SF2_SAVE_ROOT").is_empty() or not space.begins_with("issue1769-") \
            or case_name not in ["corrupt", "corrupt-stress", "missing-stress"]:
        push_error("Select fresh issue1769-* validation slots, output and a slot reading case"); quit(2); return
    stress = case_name.ends_with("-stress")
    missing_primary = case_name == "missing-stress"
    directory = OS.get_environment("SF2_SAVE_ROOT").path_join(space).path_join("sf2-opening")
    backup = slot_bytes("slot1.bak"); initial_primary = slot_bytes("slot1.json")
    var second := slot_bytes("slot2.json")
    var primary_exists := FileAccess.file_exists(directory.path_join("slot1.json"))
    var second_exists := FileAccess.file_exists(directory.path_join("slot2.json"))
    if backup.is_empty() or primary_exists == missing_primary or second_exists != stress \
            or (primary_exists and initial_primary.get_string_from_utf8() != "{") \
            or (second_exists and second.get_string_from_utf8() != "{") or FileAccess.file_exists(directory.path_join("slot2.bak")):
        push_error("Unexpected disposable slot fixture"); quit(2); return
    # Retain the Translation resource until the host prepares its localization candidate.
    var chinese := load("res://localization/zh_CN.po") as Translation
    if stress:
        chinese.erase_message("slot.matching")
        chinese.add_message("save.details", "详情：{0}。\n" + "请检查此槽位的主存档及备份，再决定是否恢复。".repeat(12))
    root.size = Vector2i(1920,1080)
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(8)
    await key(KEY_ENTER); await key(KEY_E)
    if not await open_continue(): finish(); return
    var status := "主存档缺失" if missing_primary else "存档不可用"
    check(status in slots().choices[0] and "有可用备份" in slots().choices[0], "slot 1 explains the primary and available backup")
    check(("存档不可用" if stress else "空槽位") in slots().choices[1], "slot 2 status")
    await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("error"): finish(); return
    var error := "save-malformed" if stress else "save-slot-empty"
    check(error in (find("SlotDetail") as Label).text and slots().choices == ["返回"], "unrecoverable slot has reason and Back")
    await inspect_page("error")
    await key(KEY_ESCAPE)
    if not await open_continue(): finish(); return
    await key(KEY_ENTER)
    if not await wait_slots("recover-backup"): finish(); return
    check("恢复此备份？" in slots().message and "槽位 1" in slots().message and " UTC" in slots().message
        and slots().choices == ["取消", "恢复备份"] and slots().selected == 0, "backup identity and Cancel-first actions")
    check(("主存档缺失" if missing_primary else "save-malformed") in (find("SlotDetail") as Label).text, "specific primary failure")
    await inspect_page("recovery")
    await key(KEY_ENTER)
    check_files(false)
    if not await open_continue(): finish(); return
    await key(KEY_ENTER)
    if not await wait_slots("recover-backup"): finish(); return
    await key(KEY_DOWN); await key(KEY_ENTER)
    var deadline := Time.get_ticks_msec() + 16000
    while not state().get("canWaitAtInput", false) and Time.get_ticks_msec() < deadline: await frames(3)
    if not state().get("canWaitAtInput", false):
        check(false, "backup restores usable field"); finish(); return
    var envelope: Dictionary = JSON.parse_string(backup.get_string_from_utf8())
    var payload: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
    var actual = normalized(restored.get("progress"))
    check(actual == payload.data, "field at attach matches backup progress")
    check_files(true)
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): finish(); return
    if stress:
        await key(KEY_DOWN); await key(KEY_ENTER)
        if not await wait_slots("confirm-save"): finish(); return
        await key(KEY_DOWN); await key(KEY_ENTER)
        if not await wait_slots("error"): finish(); return
        check("save-primary-invalid-use-recovery" in (find("SlotDetail") as Label).text and
            slots().choices == ["返回"], "field refuses to publish over a corrupt primary")
        await inspect_page("field-error")
        await key(KEY_ENTER)
        check(selected_menu() == "Save", "field failure Back returns to original Save row")
        await key(KEY_ENTER)
        if not await wait_slots("save-slots"): finish(); return
    await key(KEY_ENTER)
    if not await wait_slots("confirm-save"): finish(); return
    check("覆盖槽位 1" in slots().message and "成为备份" in slots().message and slots().selected == 0
        and slots().choices == ["取消", "保存并返回游戏", "保存并退出"], "field overwrite explains effect and starts safe")
    await inspect_page("overwrite")
    await key(KEY_ESCAPE)
    if not await wait_slots("save-slots"): finish(); return
    check(slots().selected == 0, "overwrite Back restores chosen slot")
    await key(KEY_ESCAPE)
    check(selected_menu() == "Save", "Back returns to Save row")
    await key(KEY_ESCAPE)
    check(find("SystemMenuView") == null and state().get("canWaitAtInput", false), "field regains control")
    var player: Dictionary = state().entities[0]
    await key(KEY_DOWN); await frames(30)
    var moved: Dictionary = state().entities[0]
    check(moved.x == player.x and moved.y == player.y + 384 and state().get("canWaitAtInput", false),
        "first field input walks one tile")
    check_files(true)
    check(slot_bytes("slot2.json") == second and FileAccess.file_exists(directory.path_join("slot2.json")) == stress,
        "other slot preserved")
    finish()
