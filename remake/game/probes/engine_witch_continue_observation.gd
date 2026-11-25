extends SceneTree

# Drives production Main with ordinary input. Only the receipt is written here.
var host: Node
var helper: Node
var samples: Array = []
var failures: Array[String] = []
var path := OS.get_environment("SF2_WITCH_CONTINUE_OUTPUT")
var mode := OS.get_environment("SF2_WITCH_CONTINUE_CASE")
var locale := OS.get_environment("SF2_WITCH_CONTINUE_LOCALE")
var slot_index := 1 if OS.get_environment("SF2_WITCH_CONTINUE_SLOT_INDEX").is_empty() else int(OS.get_environment("SF2_WITCH_CONTINUE_SLOT_INDEX"))
var frame_count := 0
var maximum_frame_ms := 0
var previous_frame := 0
var hero := OS.get_environment("SF2_WITCH_CONTINUE_HERO")

func _initialize() -> void:
    process_frame.connect(func():
        var now := Time.get_ticks_msec()
        if previous_frame > 0: maximum_frame_ms = maxi(maximum_frame_ms, now - previous_frame)
        previous_frame = now
        frame_count += 1)
    call_deferred("run")

func read_node(name: String, method: String) -> Dictionary:
    var node := host.find_child(name, true, false)
    return {} if node == null else JSON.parse_string(node.call(method))

func entry() -> Dictionary:
    return read_node("SessionEntryView", "ReadEntryObservationJson")

func slots() -> Dictionary:
    return read_node("SaveSlotView", "ReadSaveObservationJson")

func progress() -> Dictionary:
    return JSON.parse_string(host.call("ReadPersistenceObservationJson"))

func field_state() -> Dictionary:
    return read_node("ExplorationSessionView", "ReadObservationJson")

func release_barrier() -> bool:
    return JSON.parse_string(host.call("ReadSettingsHostObservationJson")).releaseBarrier

func attach_sample() -> void:
    sample("attached")
    samples[-1].snapshot = JSON.parse_string(helper.call("CaptureSettledField", host))

func frames(count: int) -> void:
    for i in range(count): await process_frame

func edge(key: Key, down: bool, echo := false) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.physical_keycode = key
    event.pressed = down
    event.echo = echo
    Input.parse_input_event(event)

func press(key: Key) -> void:
    edge(key, true); await frames(2)
    edge(key, false); await frames(3)

func check(ok: bool, reason: String) -> bool:
    if not ok: failures.append(reason)
    return ok

func until(ready: Callable, label: String) -> bool:
    var end := Time.get_ticks_msec() + 15000
    while Time.get_ticks_msec() < end:
        if ready.call(): return true
        await process_frame
    sample("timeout-" + label)
    return check(false, label)

func sample(label: String) -> void:
    samples.append({"label": label, "milliseconds": Time.get_ticks_msec(), "frames": frame_count,
        "maximumFrameMs": maximum_frame_ms, "preparation": JSON.parse_string(host.call("ReadEntryPreparationObservationJson")),
        "entry": entry(), "slots": slots(), "progress": progress(),
        "opening": JSON.parse_string(host.call("ReadOpeningObservationJson"))})

func text_at(id: int) -> bool:
    var page := entry()
    if not check(page.get("continueText") == id and progress().sessionId == null, "Text precedes session mount"):
        return false
    check(page.displayLocale == locale and page.displayFallback == null, "Direct locale binding at %d" % id)
    var dialogue: Dictionary = page.dialogue
    check(dialogue.visible and dialogue.lines > 0 and not dialogue.moreBelow, "Readable shaped text at %d" % id)
    if id == 226:
        check(hero != "" and page.message.count(hero) == 1 and not page.message.contains("{"),
            "Recognition uses the admitted protagonist name exactly once")
    sample("witch-%d" % id)
    return true

func open_slots() -> bool:
    await press(KEY_ENTER)
    if not text_at(225): return false
    sample("question-acknowledged")
    await press(KEY_ENTER)
    var ready := await until(func(): return slots().get("state") == "continue-slots", "Continue slots")
    sample("slots-ready")
    return ready

func default_menu(action: String) -> bool:
    return await until(func(): return entry().get("selectedAction") == action, "Default " + action)

func usable_field() -> bool:
    return field_state().get("canWaitAtInput", false) and progress().get("progress") != null

func route() -> void:
    if mode == "seeded":
        if await until(usable_field, "Direct seed attaches"):
            check(entry().is_empty(), "Seeded entry bypasses Witch dialogue")
            attach_sample()
        return
    if mode == "retry":
        if not await until(func(): return JSON.parse_string(host.call("ReadEntryPreparationObservationJson")).completed > 0,
            "Locked-input preparation completed"): return
        helper.call("ReleaseEntryInput")
    sample("startup")
    var dwell := float(OS.get_environment("SF2_WITCH_CONTINUE_DWELL"))
    if dwell > 0: await create_timer(dwell).timeout
    sample("startup-before-skip")
    var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
    if opening.active: await press(KEY_E)
    if not await until(func(): return entry().get("stage") == "title", "Title after startup"): return
    await press(KEY_ENTER)
    var greeting := entry()
    check(greeting.get("stage") == "dialogue" and greeting.get("page") == 0, "Shared greeting starts at 216")
    sample("greeting")
    await press(KEY_E)
    var expected := "Continue" if mode in ["valid", "changed", "content-changed", "retry"] else "NewGame"
    if not await default_menu(expected): return
    sample("menu")
    if mode == "empty": return
    if mode == "newgame":
        await press(KEY_ENTER)
        if not await until(func(): return slots().get("state") == "new-game-slots", "New Game slots"): return
        await press(KEY_ENTER)
        if not await until(func(): return host.find_child("NameEntryView", true, false) != null, "Name entry"): return
        await press(KEY_ENTER)
        if not await until(func(): return slots().get("state") == "confirm-new-game", "Initial save confirmation"): return
        await press(KEY_DOWN); await press(KEY_ENTER)
        if await until(func(): return progress().sessionId != null, "Fully initialized New Game mounts after save"):
            check(entry().is_empty(), "New Game closes entry after publication")
            sample("new-game-mounted")
        return
    if mode in ["abnormal", "corrupt"]: await press(KEY_DOWN)
    if not await open_slots(): return
    if mode in ["changed", "content-changed"]:
        var changed := OS.get_environment("SF2_WITCH_CONTINUE_CHANGE_FILE")
        var allowed := ProjectSettings.globalize_path("res://../../local/").simplify_path().trim_suffix("/") + "/"
        if mode == "changed":
            var mutation_space := OS.get_environment("SF2_WITCH_CONTINUE_MUTATION_NAMESPACE")
            if not check(not mutation_space.is_empty() and mutation_space == OS.get_environment("SF2_SAVE_NAMESPACE"),
                "Explicit disposable mutation namespace required"): return
            allowed = OS.get_environment("SF2_SAVE_ROOT").path_join(mutation_space).path_join("sf2-opening/").simplify_path().trim_suffix("/") + "/"
        if not check(changed.replace("\\", "/").simplify_path().begins_with(allowed.replace("\\", "/")),
            "Mutation file stays inside its controlled copy directory"): return
        var file := FileAccess.open(changed, FileAccess.READ_WRITE)
        if not check(file != null, "Disposable mutation file opens"): return
        if mode == "changed": file.store_string("{broken")
        else:
            file.seek_end(); file.store_string(" ")
        file.close()
        for step in range(slot_index): await press(KEY_DOWN)
        await press(KEY_ENTER)
        if await until(func(): return slots().get("state") == "error", "Changed input rejected"):
            check(progress().sessionId == null and entry().get("continueText") != 226, "Changed input never mounts/farewells")
            sample("changed-input")
        return
    if mode == "corrupt":
        await press(KEY_ENTER)
        if await until(func(): return slots().get("state") == "error", "Corrupt slot rejected"): sample("corrupt-rejected")
        return
    if mode == "abnormal":
        await press(KEY_ENTER)
        if not await until(func(): return slots().get("state") == "recover-backup", "Backup offer is reachable"): return
        sample("backup")
        await press(KEY_ESCAPE)
        if not await default_menu("NewGame"): return
        await press(KEY_DOWN)
        if not await open_slots(): return
        await press(KEY_DOWN)
        await press(KEY_ENTER)
        if not await until(func(): return slots().get("state") == "error", "Incompatible slot diagnosis"): return
        check(entry().get("continueText") == null and progress().sessionId == null, "Abnormal slots cannot start farewell")
        sample("incompatible")
        await press(KEY_ESCAPE)
        if await default_menu("NewGame"): sample("returned-menu")
        return
    await press(KEY_ESCAPE)
    if not await default_menu("Continue"): return
    check(entry().get("continueText") == null and progress().sessionId == null, "Slot cancel leaves no admitted candidate")
    sample("cancelled-slots")
    if not await open_slots(): return
    for step in range(slot_index): await press(KEY_DOWN)
    sample("slot-selected")
    edge(KEY_ENTER, true)
    if not await until(func(): return entry().get("continueText") == 226, "Recognize after the candidate is ready"):
        edge(KEY_ENTER, false)
        return
    sample("recognition-ready")
    edge(KEY_ENTER, true, true); await frames(6)
    check(entry().get("continueText") == 226, "Held Confirm and echo cannot skip recognition")
    edge(KEY_ENTER, false); await frames(4)
    if not text_at(226): return
    await press(KEY_ENTER)
    if not text_at(224): return
    edge(KEY_ENTER, true)
    if not await until(usable_field, "Farewell mounts usable field state"):
        edge(KEY_ENTER, false)
        return
    attach_sample()
    var landing: Dictionary = progress().availability.Landing
    check(release_barrier(), "Mount arms release barrier while farewell Confirm is held")
    edge(KEY_ENTER, true, true); await frames(6)
    check(release_barrier() and usable_field() and progress().availability.Landing == landing,
        "Held farewell Confirm does not reach gameplay")
    edge(KEY_ENTER, false); await frames(4)
    check(not release_barrier() and usable_field(), "Release rearms without submitting Confirm")
    sample("released")

func run() -> void:
    if path.is_empty() or not mode in ["empty", "valid", "abnormal", "seeded", "corrupt", "changed", "content-changed", "newgame", "retry"]:
        quit(2); return
    # Use the same full-size canvas with a non-windowed headless display driver.
    if DisplayServer.get_name() == "headless": root.mode = Window.MODE_FULLSCREEN
    root.size = Vector2i(1920, 1080)
    helper = load("res://probes/OpeningStartupControl.cs").new()
    root.add_child(helper)
    if mode == "retry": helper.call("HoldEntryInput")
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.size = Vector2i(1920, 1080)
    await frames(4)
    await route()
    helper.call("ReleaseEntryInput")
    helper.call("RetainEntryHost", host)
    host.queue_free()
    await frames(2)
    var retired: Dictionary = JSON.parse_string(helper.call("RetiredEntryPreparation"))
    var deadline := Time.get_ticks_msec() + 10000
    while retired.completed < retired.reads and Time.get_ticks_msec() < deadline:
        await process_frame
        retired = JSON.parse_string(helper.call("RetiredEntryPreparation"))
    check(retired.retired and not retired.retained and retired.completed == retired.reads,
        "Disposed owner releases definition and detached read completes without publication")
    var final_output := FileAccess.open(path, FileAccess.WRITE)
    final_output.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures,
        "case": mode, "locale": locale, "samples": samples, "retired": retired}, "  "))
    final_output.close()
    helper.queue_free()
    await frames(2)
    quit(0 if failures.is_empty() else 1)
