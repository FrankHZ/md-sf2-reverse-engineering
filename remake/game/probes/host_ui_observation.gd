extends SceneTree

# Finite production-host witness. Real input/resize; no active-state setters or images.
var host: Node
var failures: Array = []
var samples: Array = []
var checks := 0
var ended := false
var restored: Dictionary = {}
var case_name := OS.get_environment("SF2_HOST_UI_CASE")
var output := OS.get_environment("SF2_HOST_UI_OUTPUT")

func _initialize() -> void:
    create_timer(55).timeout.connect(func(): check(false, "55 second deadline"); finish())
    call_deferred("run")

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", func(json: String):
            var result: Dictionary = JSON.parse_string(json)
            if result.boundary == "attach": restored = observation(host, "ReadPersistenceObservationJson"))

func check(ok: bool, message: String) -> void:
    checks += 1
    if not ok: failures.append(message); push_error(message)

func frames(count: int = 3) -> void:
    for i in range(count): await process_frame

func edge(code: int, pressed: bool) -> void:
    var e := InputEventKey.new(); e.keycode = code; e.pressed = pressed
    Input.parse_input_event(e); await frames()

func key(code: int) -> void:
    await edge(code, true); await edge(code, false)

func pad(button: int) -> void:
    for pressed in [true, false]:
        var e := InputEventJoypadButton.new(); e.device = 0; e.button_index = button; e.pressed = pressed
        Input.parse_input_event(e); await frames()

func mouse(control: Control, pressed: bool) -> void:
    var e := InputEventMouseButton.new(); e.button_index = MOUSE_BUTTON_LEFT; e.position = control.get_global_rect().get_center(); e.pressed = pressed
    Input.parse_input_event(e); await frames()

func find(name_: String) -> Node: return host.find_child(name_, true, false)
func observation(node: Node, method: String) -> Dictionary:
    return {} if node == null else JSON.parse_string(node.call(method))
func settings() -> Dictionary: return observation(find("SettingsView"), "ReadSettingsObservationJson")
func slots() -> Dictionary: return observation(find("SaveSlotView"), "ReadSaveObservationJson")
func window() -> Dictionary: return observation(host, "ReadWindowSupportObservationJson")
func state() -> Dictionary:
    var view := find("ExplorationSessionView")
    if view == null or not (view as Control).is_visible_in_tree(): view = find("BattleSessionView")
    return observation(view, "ReadObservationJson")
func selected_menu() -> String:
    for name_ in ["Resume", "Save", "Settings", "NewGame", "Quit"]:
        var button := find("System" + name_) as Button
        if button != null and button.theme_type_variation == "EntrySelectedButton": return name_
    return ""

func open_witch_settings() -> void:
    var entry := find("SessionEntryView")
    if observation(entry, "ReadEntryObservationJson").get("stage") == "title":
        await key(KEY_ENTER)
        await key(KEY_E)
    var menu := observation(entry, "ReadEntryObservationJson")
    if menu.get("stage") != "menu":
        check(false, "Witch menu reachable")
        return
    if menu.visibleCharacters >= 0 and menu.visibleCharacters < menu.totalCharacters:
        await key(KEY_ENTER)
    var choice := entry.find_child("Settings", true, false) as Button
    for i in range(menu.buttons.size()):
        if choice.theme_type_variation == "EntrySelectedButton": break
        await key(KEY_UP)
    check(choice.is_visible_in_tree() and choice.theme_type_variation == "EntrySelectedButton", "Witch Settings choice visible and selected")
    await key(KEY_ENTER)
    check(find("SettingsView") != null, "Witch Settings opens")

func sample(label: String) -> void:
    var row := {"label": label, "window": window(), "settings": settings(), "slots": slots(), "menu": selected_menu()}
    var s := state()
    if find("SystemMenuView") != null:
        row.reasons = {}
        for name_ in ["Save", "Settings", "NewGame"]: row.reasons[name_] = (find("Reason" + name_) as Label).text
    for k in ["sessionId", "revision", "map", "stage", "actor", "wait", "failure", "scene", "canWaitAtInput", "canWaitForText", "textId", "mainSeed", "thinkingSeed"]:
        if s.has(k): row[k] = s[k]
    row.controls = []
    for name_ in ["SystemMenuWindow", "SystemMenuReading", "SystemMenuHint", "SettingsWindow", "SettingsReading", "SettingsActions", "SettingsHint", "HelpWindow", "HelpScroll", "HelpHint", "SlotWindow", "SlotReading", "SlotHint"]:
        var c := find(name_) as Control
        if c != null and c.is_visible_in_tree(): row.controls.append({"name": name_, "rect": rect(c.get_global_rect())})
    samples.append(row)

func rect(r: Rect2) -> Array: return [r.position.x, r.position.y, r.size.x, r.size.y]
func normalized(value: Variant) -> Variant:
    if value is Dictionary:
        var result := {}
        for k in value:
            var name_: String = str(k)
            result[name_.left(1).to_lower() + name_.substr(1)] = normalized(value[k])
        return result
    if value is Array:
        return value.map(normalized)
    return value
func geometry(panel_name: String) -> void:
    var panel := find(panel_name) as Control
    check(panel != null and panel.is_visible_in_tree(), panel_name + " visible")
    if panel == null: return
    var bounds := Rect2(Vector2.ZERO, root.get_visible_rect().size)
    check(bounds.grow(1).encloses(panel.get_global_rect()), panel_name + " within client")
    for pair in [["SettingsReading", "SettingsActions"], ["SettingsActions", "SettingsHint"], ["HelpScroll", "HelpHint"], ["SlotReading", "SlotHint"], ["SystemMenuReading", "SystemMenuHint"]]:
        var a := find(pair[0]) as Control; var b := find(pair[1]) as Control
        if a != null and b != null and a.is_visible_in_tree() and b.is_visible_in_tree():
            check(not a.get_global_rect().intersects(b.get_global_rect()), str(pair) + " disjoint")

func wait_slots(expected: String) -> bool:
    for i in range(1600):
        if slots().get("state") == expected: return true
        await process_frame
        OS.delay_msec(1)
    check(false, "slot state " + expected); sample("slot-timeout"); return false

func recover(panel_name: String) -> void:
    var before := state(); var s := settings(); var p := slots(); var menu := selected_menu()
    var pending: Control = null
    if panel_name == "SlotWindow":
        pending = find("SaveSlotView").find_children("*", "Button", true, false)[0]
        await mouse(pending, true)
    root.size = Vector2i(1280, 720); await frames(5)
    check(window().recovery != null and not window().allowed, "undersized client enters recovery")
    for name_ in ["SettingsView", "SystemMenuView", "SaveSlotView"]:
        var c := find(name_) as Control
        if c != null: check(not c.is_visible_in_tree(), name_ + " actually hidden by recovery")
    check(state().get("revision") == before.get("revision"), "recovery retains revision")
    await key(KEY_ESCAPE)
    var quit_view := find("WindowQuitView")
    check(quit_view != null, "recovery Quit has separate consequence panel")
    if quit_view != null:
        var q := observation(quit_view, "ReadSaveObservationJson")
        check(q.selected == 0, "recovery Quit defaults Cancel")
        await key(KEY_ESCAPE)
    root.size = Vector2i(1920, 1080); await frames(5)
    if pending != null: await mouse(pending, false)
    check(window().recovery == null and window().allowed, "supported resize restores covered page")
    check(state().get("revision") == before.get("revision"), "resize and Quit Cancel retain revision")
    check(settings().get("draft") == s.get("draft") and settings().get("selected") == s.get("selected"), "settings draft/focus retained through recovery")
    check(slots() == p and selected_menu() == menu, "slot/page/menu selection retained through recovery")
    geometry(panel_name); sample("recovered-" + panel_name)

func settings_flow() -> void:
    geometry("SettingsWindow")
    var initial := settings()
    var read_only: bool = initial.readOnly
    if read_only:
        for i in range(8): check((find("Setting" + str(i)) as Button).disabled, "override prevents edit/apply " + str(i))
    if not read_only:
        await key(KEY_ENTER)
        check(settings().draft != initial.draft, "setting edit remains a draft")
    # Help is a child, with the same draft and focus on Back.
    var help_button := find("Setting9") as Control
    await mouse(help_button, true); await mouse(help_button, false)
    check(settings().help, "pointer enters full controls help")
    geometry("HelpWindow")
    if settings().help:
        var help := find("ControlsHelpView")
        var h := observation(help, "ReadControlsHelpObservationJson")
        for i in range(h.count):
            geometry("HelpWindow")
            var page := observation(help, "ReadControlsHelpObservationJson")
            check(not page.body.is_empty() and page.rows != null, "full help page " + str(i))
            check(not "host.help" in page.body and (not settings().locale == "zh-CN" or "操作帮助" in page.title), "help uses selected catalog " + str(i))
            await key(KEY_DOWN); await key(KEY_RIGHT)
        await key(KEY_ESCAPE)
    check(not settings().help and settings().selected == 9, "help Back restores settings row")
    if find("SystemMenuView") != null: await recover("SettingsWindow")
    # Orphan pointer release after relayout cannot apply or close settings.
    var cancel := find("Setting8") as Control
    await mouse(cancel, true)
    root.size = Vector2i(2560, 1440); await frames()
    root.size = Vector2i(1920, 1080); await frames()
    await mouse(cancel, false)
    check(find("SettingsView") != null, "relayout cancels pending click")
    for size_ in [Vector2i(2560, 1440), Vector2i(3840, 2160), Vector2i(2560, 1080)]:
        root.size = size_; await frames(); geometry("SettingsWindow"); sample("settings-size-" + str(size_))
    root.size = Vector2i(1920, 1080); await frames()
    await key(KEY_ESCAPE)
    var applied: Dictionary = observation(host, "ReadSettingsHostObservationJson").applied
    check(applied == initial.applied, "Settings Cancel keeps applied preferences")
    sample("settings-cancelled")

func apply_flow() -> void:
    var before := state()
    await key(KEY_ENTER)
    var initial := settings()
    check(not initial.is_empty(), "reopen Settings at original row")
    if initial.is_empty(): return
    await key(KEY_ENTER)
    var draft: Dictionary = settings().draft
    for i in range(7): await key(KEY_DOWN)
    check(settings().selected == 7, "fixed Apply receives keyboard focus")
    await edge(KEY_ENTER, true); await frames(12)
    check(find("SettingsView") == null and selected_menu() == "Settings", "held Apply returns once to originating Menu")
    var applied: Dictionary = observation(host, "ReadSettingsHostObservationJson").applied
    check(applied == draft and applied != initial.applied, "Apply publishes exactly the draft")
    check(state().revision == before.revision, "settings publication preserves paused gameplay")
    var preference_path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    var persisted: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(preference_path))
    check(persisted.textMode == applied.TextMode, "real preference file records Apply")
    await edge(KEY_ENTER, false); sample("settings-applied")

func save_flow() -> void:
    await key(KEY_F10)
    check(selected_menu() == "Resume", "new system menu starts Resume")
    geometry("SystemMenuWindow")
    var frozen := state(); await frames(12)
    check(state().revision == frozen.revision, "system menu freezes gameplay")
    await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    geometry("SlotWindow")
    await key(KEY_DOWN); await key(KEY_ENTER)
    check(slots().state == "confirm-save" and slots().selected == 0, "second slot has separate Cancel-first confirmation")
    await recover("SlotWindow")
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    check(slots().selected == 1, "cancel confirmation restores second slot")
    await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
    if case_name == "failure":
        if await wait_slots("error"):
            geometry("SlotWindow"); sample("real-storage-refusal")
            await key(KEY_ESCAPE); check(selected_menu() == "Save", "failure Back restores system Save")
            await key(KEY_ESCAPE)
        return
    if not await wait_slots("saved"): return
    sample("saved-second-slot"); geometry("SlotWindow")
    var slot_path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot2.json")
    var saved_bytes := FileAccess.get_file_as_string(slot_path)
    await key(KEY_ENTER)
    check(find("SystemMenuView") == null and find("SaveSlotView") == null, "saved Return closes host to play")
    # Write again through occupied slot; cancellation never publishes.
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    await key(KEY_DOWN); await key(KEY_ENTER)
    check(slots().state == "confirm-save" and slots().selected == 0, "occupied slot Cancel-first overwrite")
    sample("occupied-overwrite")
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    check(FileAccess.get_file_as_string(slot_path) == saved_bytes, "overwrite Cancel leaves exact slot bytes unchanged")
    await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("saved"): return
    check(FileAccess.file_exists(slot_path.trim_suffix(".json") + ".bak"), "second real write preserves backup")
    await key(KEY_ENTER)
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    await key(KEY_ESCAPE)
    check(selected_menu() == "Save", "slots Back restores originating Save row")
    # Settings is legal at this retained settled field boundary.
    await key(KEY_DOWN); await key(KEY_ENTER)
    check(find("SettingsView") != null, "settled field offers settings")
    if find("SettingsView") != null: await settings_flow()
    check(selected_menu() == "Settings", "settings Back restores originating row")
    await apply_flow()
    await key(KEY_F10)
    check(find("SystemMenuView") == null, "Menu toggles only its root")
    await key(KEY_F10)
    # Dirty/clean consequence always starts Cancel; supported pad Back returns intact.
    await key(KEY_UP); await key(KEY_ENTER)
    check(slots().state == "confirm-quit" and slots().selected == 0, "Quit consequence defaults Cancel")
    sample("quit-confirmation")
    await pad(JOY_BUTTON_B)
    check(selected_menu() == "Quit", "pad Cancel restores Quit row")
    await key(KEY_UP); await key(KEY_ENTER)
    var entry := observation(find("SessionEntryView"), "ReadEntryObservationJson")
    check(entry.get("replacement", false) and entry.get("cancelSelected", false), "session replacement starts Cancel")
    var identity: String = state().sessionId
    await key(KEY_ESCAPE)
    check(selected_menu() == "NewGame" and state().sessionId == identity, "replacement Cancel restores original journey and row")
    await key(KEY_ESCAPE)

func battle_flow() -> void:
    for i in range(2400):
        var s := state()
        if s.get("stage") == "Movement" and s.get("movement") == null: break
        if s.get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        await process_frame
        OS.delay_msec(1)
    check(state().get("stage") == "Movement", "real battle reaches player Movement")
    for phase in ["Movement", "ActionChoice"]:
        if phase == "ActionChoice": await key(KEY_ENTER)
        check(state().stage == phase, "actual phase " + phase)
        await key(KEY_F10)
        var before := state(); await frames(15)
        check(state().revision == before.revision, phase + " paused")
        check((find("SystemSettings") as Button).disabled, phase + " settings prohibited with reason")
        check(not (find("ReasonSettings") as Label).text.is_empty(), phase + " settings reason visible")
        check((find("SystemSave") as Button).disabled == (phase != "Movement"), phase + " exact save predicate")
        sample("battle-" + phase); geometry("SystemMenuWindow")
        await recover("SystemMenuWindow")
        if phase == "Movement":
            await key(KEY_DOWN); await key(KEY_ENTER)
            if not await wait_slots("save-slots"): return
            geometry("SlotWindow"); sample("real-battle-slot-offer")
            await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
            if not await wait_slots("saved"): return
            geometry("SlotWindow"); sample("real-battle-saved")
            await key(KEY_ENTER)
            check(state().stage == "Movement" and state().actor == before.actor, "battle save returns to actual actor Movement")
            await key(KEY_F10)
            before = state()
        await edge(KEY_ENTER, true); await frames(12)
        check(state().revision == before.revision and state().stage == phase, "held Resume cannot advance " + phase)
        await edge(KEY_ENTER, false)
        check(find("SystemMenuView") == null, "Resume returns to retained " + phase)

func story_flow() -> void:
    for direction in [KEY_DOWN, KEY_RIGHT]:
        for i in range(800):
            if state().get("canWaitAtInput", false): break
            await process_frame
        await key(direction)
    for i in range(1600):
        if state().get("canWaitForText", false) or state().get("canWaitForChoice", false): break
        await process_frame; OS.delay_msec(1)
    var initial := state()
    check(initial.get("wait") in ["FieldTextWait", "DialogueWait", "W1TextWait", "ChoiceWait"], "actual story reading boundary")
    await key(KEY_F10)
    for name_ in ["Save", "Settings", "NewGame"]:
        check((find("System" + name_) as Button).disabled, "story prohibits " + name_)
        check(not (find("Reason" + name_) as Label).text.is_empty(), "story explains " + name_)
    var before := state(); await frames(20)
    check(state().revision == before.revision and state().wait == before.wait, "host freezes story reading")
    geometry("SystemMenuWindow"); sample("story-paused")
    if OS.get_environment("SF2_HOST_UI_STRESS") == "1":
        var reading := find("SystemMenuReading") as ScrollContainer
        await key(KEY_RIGHT)
        check(reading.scroll_vertical > 0, "keyboard reads long unavailable reasons without selecting them")
        await key(KEY_LEFT)
    await recover("SystemMenuWindow")
    await edge(KEY_ENTER, true); await frames(10)
    sample("story-held-resume")
    check(state().revision == before.revision and state().wait == before.wait, "held Resume cannot acknowledge story")
    await edge(KEY_ENTER, false)

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1688-"):
        push_error("Use SF2_HOST_UI_OUTPUT and a disposable issue1688-* namespace"); quit(2); return
    root.size = Vector2i(1920, 1080)
    node_added.connect(observe_node)
    if OS.get_environment("SF2_HOST_UI_STRESS") == "1":
        for path in ["res://localization/en.po", "res://localization/zh_CN.po"]:
            var catalog := load(path) as Translation
            for key_ in ["system.settings-reason", "system.new-reason", "save.error.battle-selection", "host.setting-description-0", "host.help-text-1", "save.quit-dirty"]:
                catalog.add_message(key_, str(catalog.get_message(key_)).repeat(20))
            catalog.add_message("slot.record", str(catalog.get_message("slot.record")) + "\n" + "Long complete slot description · 完整的长存档说明。".repeat(30))
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames()
    if case_name == "title":
        var entry := find("SessionEntryView")
        var visible_buttons := entry.find_children("*", "Button", true, false).filter(func(b): return b.is_visible_in_tree())
        check(visible_buttons == [find("Start")], "Start is the only visible title action")
        await key(KEY_ESCAPE)
        check(observation(entry, "ReadEntryObservationJson").get("stage") == "title" and find("SettingsView") == null, "Cancel leaves the title in place")
        await open_witch_settings()
        if find("SettingsView") != null:
            await settings_flow()
            var choice := entry.find_child("Settings", true, false) as Button
            check(observation(entry, "ReadEntryObservationJson").get("stage") == "menu" and choice.is_visible_in_tree() and choice.theme_type_variation == "EntrySelectedButton", "Back restores the Witch Settings choice")
    elif case_name in ["continue", "backup", "battle-continue"]:
        await key(KEY_ENTER); await key(KEY_E)
        var entry := observation(find("SessionEntryView"), "ReadEntryObservationJson")
        if entry.visibleCharacters >= 0 and entry.visibleCharacters < entry.totalCharacters: await key(KEY_ENTER)
        await key(KEY_DOWN); await key(KEY_ENTER)
        if await wait_slots("continue-slots"):
            if case_name != "battle-continue": await key(KEY_DOWN)
            await key(KEY_ENTER)
            if case_name == "backup" and await wait_slots("recover-backup"):
                check(slots().selected == 0, "backup recovery defaults Cancel")
                geometry("SlotWindow"); sample("corrupt-primary-backup-offer")
                await key(KEY_DOWN); await key(KEY_ENTER)
            for i in range(1600):
                if (find("BattleSessionView") if case_name == "battle-continue" else find("ExplorationSessionView")) != null: break
                await process_frame; OS.delay_msec(1)
            if case_name == "battle-continue":
                check(state().get("stage") == "Movement", "ordinary Continue restores saved battle turn")
                var battle_path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot1.json")
                var envelope: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(battle_path))
                var payload: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
                check(state().get("actor") == payload.data.battle.queue[payload.data.battle.cursor].actor.value, "ordinary Continue restores exact queued actor")
                sample("continued-battle"); finish(); return
            check(find("ExplorationSessionView") != null, "fresh ordinary Continue restores actual saved session")
            var slot_path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot2.json")
            var record: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(slot_path))
            var payload: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(record.payload))
            check(normalized(restored.get("progress")) == payload.data, "fresh Continue restores every saved progress field at attach")
            sample("continued-field")
    else:
        var start := OS.get_environment("SF2_HOST_UI_START")
        check(host.call("StartPreVoyageLocalizationObservation", start) == "started", "existing controlled startup admits real world")
        if case_name == "battle": await battle_flow()
        elif case_name == "story": await story_flow()
        else:
            for i in range(300):
                if state().get("canWaitAtInput", false): break
                await process_frame
            check(state().get("canWaitAtInput", false), "settled connected field")
            await save_flow()
    finish()

func finish() -> void:
    if ended: return
    ended = true
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file != null: file.store_string(JSON.stringify({"passed": failures.is_empty(), "checks": checks, "failures": failures, "case": case_name, "samples": samples}, "\t")); file.close()
    quit(0 if failures.is_empty() else 1)
