extends "res://probes/host_ui_observation.gd"

# Only the dedicated numbered-save review. Main owns the root window, slots and input.
var storage: String

func run() -> void:
    var save_root := OS.get_environment("SF2_SAVE_ROOT")
    var space := OS.get_environment("SF2_SAVE_NAMESPACE")
    storage = save_root.path_join(space).path_join("sf2-opening")
    if output.is_empty() or not save_root.is_absolute_path() or not space.begins_with("ui-host-review-") \
            or space.get_file() != space or not DirAccess.dir_exists_absolute(storage) \
            or case_name not in ["seed", "stage", "continue", "save", "summary"] or not OS.get_cmdline_user_args().is_empty():
        push_error("Use the dedicated numbered-save review recipe and a fresh disposable namespace")
        quit(2)
        return
    if DisplayServer.get_name() == "headless":
        push_error("Numbered-save review needs a native window")
        quit(2)
        return
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(6)
    if not require(window().allowed, "initial native client is supported"):
        finish()
        return
    await edge(KEY_E, false)
    if find("SessionEntryView") == null: await key(KEY_E)
    if not require(find("SessionEntryView") != null, "ordinary opening Skip reaches title"):
        finish()
        return
    if case_name == "seed":
        var start := OS.get_environment("SF2_HOST_UI_START")
        if require(host.call("StartPreVoyageLocalizationObservation", start) == "started", "controlled Map3 start admitted"):
            if await settled():
                await write_slot(2)
    elif case_name == "continue":
        await recovery_flow()
    elif case_name == "summary":
        await summary_review()
    elif await open_continue():
        await choose("2")
        if await attached(2, slot_bytes(2)):
            if case_name == "stage":
                for slot in range(3, 8):
                    if not await write_slot(slot): break
                if failures.is_empty() and await write_slot(7, true):
                    check(not slot_bytes(7, "bak").is_empty(), "production writer created slot 7 backup")
            else:
                await save_review()
    finish()

func require(ok: bool, message: String) -> bool:
    check(ok, message)
    return ok

func summary_review() -> void:
    var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_SLOT_SUMMARY_CASES")))
    var before := history()
    if not await open_continue(): return
    for row in manifest.records:
        var actual: Dictionary = JSON.parse_string(host.call("ReadSaveSummaryObservationJson", row.path, row.get("savedAt", "")))
        samples.append({"label": row.id, "summary": actual})
        for field in row.expected:
            check(actual.get(field) == row.expected[field], row.id + " " + field)
    check(slots().selected == 0, "history starts at first row")
    for i in range(manifest.choices.size()):
        check(slots().choices[i] == manifest.choices[i], "history row %d" % (i + 1))
    var reading := find("SlotReading") as Control
    for button in find("SaveSlotView").find_children("*", "Button", true, false):
        check(button.size.x <= reading.size.x, "full row fits reading width")
    slot_sample("summary-page-1")
    if not await second_page("continue-slots"): return
    slot_sample("summary-page-2")
    if not await choose("Previous saves" if "Previous saves" in slots().choices else "上一页存档"): return
    if not await wait_slots("continue-slots"): return
    check(slots().selected == 0 and slots().choices[1] == manifest.choices[1], "Previous retains exact summary and resets focus")
    for number in [1, 3, 4, 5]:
        if not await choose(("Slot " if manifest.language == "en" else "槽位 ") + str(number)): return
        if not await wait_slots("recover-backup" if number == 5 else "error"): return
        check(slots().selected == 0, "exception starts on safe action")
        check(not (find("SlotDetail") as Label).text.is_empty(), "exception retains independent detail")
        if number == 5:
            check("WWWWWWW" in slots().message and "41" in slots().message and "2025-12-31 20:30" in slots().message,
                "recovery separately identifies backup name, level and local time")
        slot_sample("summary-exception-%d" % number)
        await key(KEY_ESCAPE)
        if not await open_continue(): return
    check(history() == before, "paging and exceptional cancellation preserve all bytes")
    if not await choose("2"): return
    if not await attached(2, slot_bytes(2)): return
    if not await open_save(): return
    check(slots().choices[1] == manifest.choices[1], "ordinary Save shares Witch summary")
    if not await choose("2"): return
    check(slots().state == "confirm-save" and slots().selected == 0, "ordinary overwrite starts on Cancel")
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    check(slots().selected == 1 and slots().choices[1] == manifest.choices[1], "overwrite Cancel restores selected row")
    check(history() == before, "summary review leaves storage unchanged")

func choose(label: String) -> bool:
    var page := slots()
    var choices: Array = page.get("choices", [])
    for i in range(choices.size()):
        if page.selected >= choices.size(): break
        if label in choices[int(page.selected)]:
            await key(KEY_ENTER)
            return true
        await key(KEY_DOWN)
        page = slots()
    return require(false, "missing choice %s: %s" % [label, choices])

func open_continue() -> bool:
    var entry := find("SessionEntryView")
    if observation(entry, "ReadEntryObservationJson").get("stage") == "title":
        await key(KEY_ENTER)
        await key(KEY_E)
    for i in range(1600):
        if observation(entry, "ReadEntryObservationJson").get("stage") == "menu": break
        await process_frame
    var page := observation(entry, "ReadEntryObservationJson")
    if not require(page.get("stage") == "menu", "Witch menu reachable"): return false
    if page.visibleCharacters >= 0 and page.visibleCharacters < page.totalCharacters: await key(KEY_ENTER)
    var button := entry.find_child("Continue", true, false) as Button
    for i in range(page.buttons.size()):
        if button.theme_type_variation == "EntrySelectedButton": break
        await key(KEY_DOWN)
    if not require(button.theme_type_variation == "EntrySelectedButton", "Witch Continue selected"): return false
    await key(KEY_ENTER)
    for i in range(1600):
        if slots().get("state") == "continue-slots": break
        if observation(entry, "ReadEntryObservationJson").get("stage") == "continue-dialogue":
            await key(KEY_ENTER)
        await process_frame
    return await wait_slots("continue-slots")

func settled() -> bool:
    var read := {}
    for i in range(300):
        if state().get("canWaitAtInput", false) and state().get("map") == "map-3":
            read = observation(host, "ReadPersistenceObservationJson")
            if read.get("availability", {}).get("Allowed", false): return true
        await process_frame
    samples.append({"label": "unsettled", "availability": read.get("availability"), "entityWork": read.get("entityWork")})
    return require(false, "saveable Map3 walking unavailable")

func attached(slot: int, expected: PackedByteArray) -> bool:
    for i in range(1200):
        if not restored.is_empty(): break
        var entry := find("SessionEntryView")
        if observation(entry, "ReadEntryObservationJson").get("stage") == "continue-dialogue":
            await key(KEY_ENTER)
        await process_frame
    var envelope = JSON.parse_string(expected.get_string_from_utf8())
    if not require(envelope is Dictionary and not restored.is_empty(), "ordinary Continue attached from a real record"):
        return false
    var record: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
    if not require(record.slot == slot and record.formatVersion == 6, "current field record belongs to selected slot"): return false
    if not await settled(): return false
    var visit: Dictionary = record.data.visit
    var actor_slot: int = visit.aliases[visit.player]
    var actors: Array = visit.actors.filter(func(actor): return actor.slot == actor_slot)
    if not require(actors.size() == 1, "record identifies the saved leader"): return false
    var motion: Dictionary = actors[0].motion
    var expected_progress := {"landing": {"map": {"value": visit.body.map}, "facing": motion.facing,
        "position": {"x": motion.x / 384, "y": motion.y / 384}}, "names": record.data.registers.names}
    var actual_progress = normalized(restored.get("progress"))
    var live := state()
    var saved_flags: Array = record.data.registers.flags.duplicate()
    saved_flags.sort()
    var live_flags: Array = live.flags.duplicate()
    live_flags.sort()
    if not require(actual_progress == expected_progress and live.gold == record.data.party.gold and live_flags == saved_flags,
        "attached landing, names, gold and flags match selected record"):
        samples.append({"label": "attach-mismatch", "expected": expected_progress, "actual": actual_progress,
            "savedGold": record.data.party.gold, "liveGold": live.gold, "savedFlags": saved_flags, "liveFlags": live_flags})
        return false
    samples.append({"label": "cold-attach", "slot": slot, "window": window(), "progress": actual_progress,
        "gold": live.gold, "flags": live_flags, "revision": restored.revision, "sessionId": restored.sessionId})
    return true

func open_save() -> bool:
    root.grab_focus()
    await edge(KEY_F10, false)
    if not require(root.has_focus(), "native client focused before Menu input"): return false
    await key(KEY_F10)
    for i in range(300):
        if find("SystemMenuView") != null: break
        await process_frame
    await choose_system_action("Save")
    return await wait_slots("save-slots")

func publish() -> bool:
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_slots("saved"): return false
    await key(KEY_ENTER)
    return require(find("SaveSlotView") == null and find("SystemMenuView") == null
        and state().get("canWaitAtInput", false), "saved acknowledgement returns to walking")

func write_slot(slot: int, later_page: bool = false) -> bool:
    if not await open_save(): return false
    if later_page and not await second_page("save-slots"): return false
    if not await choose("Slot %d" % slot): return false
    if not require(slots().get("state") == "confirm-save" and slots().get("selected") == 0,
        "slot %d confirms with Cancel first" % slot): return false
    if not await publish(): return false
    return require(not slot_bytes(slot).is_empty(), "production writer published slot %d" % slot)

func slot_bytes(slot: int, extension: String = "json") -> PackedByteArray:
    var path := storage.path_join("slot%d.%s" % [slot, extension])
    return FileAccess.get_file_as_bytes(path) if FileAccess.file_exists(path) else PackedByteArray()

func history() -> Dictionary:
    var result := {}
    for slot in range(1, 9):
        for extension in ["json", "bak"]:
            result["slot%d.%s" % [slot, extension]] = slot_bytes(slot, extension)
    return result

func second_page(state_name: String) -> bool:
    if not await choose("More saves" if "More saves" in slots().choices else "下一页存档"): return false
    if not await wait_slots(state_name): return false
    var page := slots()
    return require(page.selected == 0 and ("Slot 7" in page.choices[0] or "槽位 7" in page.choices[0])
        and page.choices[-1] in ["Previous saves", "上一页存档"], "page 2 opens at slot 7 and offers Previous")

func slot_sample(label: String, view: String = "SaveSlotView") -> void:
    var owner := find(view)
    var panel := owner.find_child("SlotWindow", true, false) as Control
    var reading := owner.find_child("SlotReading", true, false) as Control
    var hint := owner.find_child("SlotHint", true, false) as Control
    var detail := owner.find_child("SlotDetail", true, false) as Label
    var row := {"label": label, "window": window(), "page": observation(owner, "ReadSaveObservationJson"),
        "detail": detail.text, "panel": rect(panel.get_global_rect()), "reading": rect(reading.get_global_rect()),
        "hint": rect(hint.get_global_rect()), "actions": []}
    check(owner.is_visible_in_tree() and Rect2(Vector2.ZERO, root.get_visible_rect().size).grow(1)
        .encloses(panel.get_global_rect()), label + " panel visible inside client")
    for button in owner.find_children("*", "Button", true, false):
        if reading.is_ancestor_of(button): continue
        var bounds: Rect2 = button.get_global_rect()
        check(panel.get_global_rect().encloses(bounds) and not bounds.intersects(reading.get_global_rect())
            and not bounds.intersects(hint.get_global_rect()), label + " fixed action visible: " + button.text)
        row.actions.append({"text": button.text, "rect": rect(bounds)})
    samples.append(row)

func still(label: String) -> void:
    if OS.get_environment("SF2_HOST_UI_STILLS") != "1": return
    await RenderingServer.frame_post_draw
    var image := root.get_texture().get_image()
    check(image != null and image.save_png(output.get_base_dir().path_join(label + ".png")) == OK,
        "dedicated still written: " + label)

func recovery_flow() -> void:
    var before := history()
    var backup := slot_bytes(7, "bak")
    if not require(slot_bytes(7) == "{".to_utf8_buffer() and not backup.is_empty(), "damaged slot 7 and staged backup"):
        return
    for cancel in [true, false]:
        if not await open_continue() or not await second_page("continue-slots"): return
        if cancel:
            slot_sample("continue-page-2")
            await still("continue-page-2")
        await key(KEY_ENTER)
        if not await wait_slots("recover-backup"): return
        check(slots().selected == 0 and slots().choices.size() == 2, "recovery requires explicit choice after Cancel")
        check("save-malformed" in (find("SlotDetail") as Label).text, "recovery identifies actual malformed primary")
        if cancel:
            slot_sample("backup-offer")
            await still("backup-offer")
            await key(KEY_ENTER)
            check(find("SaveSlotView") == null and observation(find("SessionEntryView"),
                "ReadEntryObservationJson").get("stage") == "menu", "Cancel restores Witch")
            check(history() == before, "Continue Cancel keeps files byte-identical")
        else:
            await key(KEY_DOWN)
            await key(KEY_ENTER)
            if await attached(7, backup):
                before["slot7.json"] = backup
                check(history() == before, "explicit recovery replaces only primary with exact backup bytes")

func save_review() -> void:
    var before := history()
    if not await open_save() or not await second_page("save-slots"): return
    await key(KEY_ENTER)
    if not await wait_slots("error"): return
    check("save-malformed" in (find("SlotDetail") as Label).text and slots().choices == ["Back", "Create a new save (Slot 8)"],
        "protected slot offers Back and fresh slot 8")
    slot_sample("protected-write")
    await still("protected-write")
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    if not require(slots().selected == 0 and "Slot 7" in slots().choices[0], "Back restores slot 7 on page 2"): return
    await window_review(before)
    if not failures.is_empty(): return
    await key(KEY_ESCAPE)
    check(selected_menu() == "Save", "Save Back restores Menu's Save row")
    await key(KEY_ESCAPE)
    check(find("SystemMenuView") == null and state().canWaitAtInput, "Menu Back restores walking")
    check(history() == before, "protected write and window recovery leave exact history")
    if not await open_save() or not await choose("Slot 6"): return
    if not require(slots().get("state") == "confirm-save" and slots().get("selected") == 0
        and "becomes its backup" in slots().message, "ordinary overwrite names the backup consequence with Cancel first"): return
    slot_sample("overwrite")
    await still("overwrite")
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    check("Slot 6" in slots().choices[int(slots().selected)] and history() == before,
        "overwrite Cancel restores slot 6 and exact files")
    await key(KEY_ENTER)
    if not await publish(): return
    before["slot6.bak"] = before["slot6.json"]
    before["slot6.json"] = slot_bytes(6)
    check(not slot_bytes(6).is_empty() and history() == before, "overwrite changes only slot 6 pair and retains displaced primary")

func window_review(before: Dictionary) -> void:
    var original := window()
    var persistence := observation(host, "ReadPersistenceObservationJson")
    var page := slots()
    var button := find("SaveSlotView").find_children("*", "Button", true, false)[0] as Button
    var point := root.get_final_transform() * button.get_global_transform_with_canvas() * (button.size / 2)
    var usable := DisplayServer.screen_get_usable_rect(root.current_screen)
    samples.append({"label": "before-shrink", "window": original, "usable": rect(usable), "page": page})
    if not require(original.mode == "Windowed" and root.has_focus()
            and Rect2(Vector2.ZERO, root.size).has_point(point), "windowed client focused and selected slot in bounds"): return
    var presses := []
    button.button_down.connect(func(): presses.append(true), CONNECT_ONE_SHOT)
    await pointer_edge(point, true)
    check(presses.size() == 1 and slots() == page, "pointer press reaches the selected row without activating it")
    root.size = Vector2i(1280, 720)
    await frames(12)
    var covered := window()
    samples.append({"label": "native-recovery", "window": covered})
    if not require(covered.client.width == 1280 and covered.client.height == 720 and covered.mode == "Windowed"
            and not covered.allowed and covered.recovery != null, "native 1280x720 enters window recovery"): return
    check(not (find("SaveSlotView") as Control).is_visible_in_tree() and covered.fieldPaused == "Disabled"
        and covered.slotsPaused == "Disabled" and slots() == page, "recovery hides and freezes Save and field")
    await still("native-recovery")
    await key(KEY_RIGHT)
    check(window().recovery.scroll > 0 and slots() == page, "recovery alone scrolls the explanation")
    await key(KEY_LEFT)
    await key(KEY_ESCAPE)
    var quit_view := find("WindowQuitView")
    if not require(window().quitting and observation(quit_view, "ReadSaveObservationJson").get("selected") == 0,
        "native Quit starts on Cancel"): return
    slot_sample("recovery-quit", "WindowQuitView")
    await key(KEY_ENTER)
    await frames()
    var choice = window().recovery
    if not require(not window().quitting and choice != null and choice.selected == 0 and choice.buttons[0].enabled,
        "Quit Cancel returns to enabled Restore Window"): return
    await key(KEY_ENTER)
    await frames(12)
    var after := window()
    var outer := Rect2i(root.get_position_with_decorations(), root.get_size_with_decorations())
    check(after.allowed and after.recovery == null and after.mode == "Windowed" and usable.encloses(outer),
        "Restore Window returns supported decorated native client inside work area")
    await pointer_edge(point, false)
    var actual := observation(host, "ReadPersistenceObservationJson")
    check(slots() == page and (find("SaveSlotView") as Control).is_visible_in_tree(),
        "same Save page/focus retained and pre-resize pointer release expires")
    check(actual.sessionId == persistence.sessionId and actual.revision == persistence.revision
        and actual.progress == persistence.progress and history() == before, "same progress, revision, session and files after recovery")
    check(after.fieldPaused == "Disabled" and after.slotsPaused != "Disabled", "restored Save owns input over paused field")
    samples.append({"label": "after-restore", "window": after, "outer": rect(outer), "page": slots()})
    await key(KEY_ENTER)
    if await wait_slots("error"):
        check(slots().choices == ["Back", "Create a new save (Slot 8)"], "fresh Confirm activates retained slot")
        await key(KEY_ENTER)
        await wait_slots("save-slots")

func pointer_edge(point: Vector2, pressed: bool) -> void:
    var event := InputEventMouseButton.new()
    event.button_index = MOUSE_BUTTON_LEFT
    event.position = point
    event.global_position = point
    event.pressed = pressed
    Input.parse_input_event(event)
    await frames()
