extends "res://probes/host_ui_observation.gd"

# One disposable namespace; ordinary input and production readback, never screenshots.
var menu_key := KEY_ESCAPE
var cancel_key := KEY_X
var confirm_key := KEY_ENTER
var preference_path: String
var original_preferences: PackedByteArray

func run() -> void:
    var space := OS.get_environment("SF2_SAVE_NAMESPACE")
    var save_root := OS.get_environment("SF2_SAVE_ROOT")
    if output.is_empty() or not space.begins_with("issue2287-") or space.get_file() != space or not save_root.is_absolute_path():
        push_error("Select a disposable issue2287-* namespace and output"); quit(2); return
    preference_path = save_root.path_join(space).path_join("sf2-preferences/input.json")
    if FileAccess.file_exists(preference_path): original_preferences = FileAccess.get_file_as_bytes(preference_path)
    if case_name == "custom": menu_key = KEY_F6; cancel_key = KEY_Q
    if case_name in ["legacy", "swapped", "occupied"]: menu_key = KEY_F10
    if case_name == "swapped": cancel_key = KEY_ENTER; confirm_key = KEY_ESCAPE
    if case_name == "occupied": cancel_key = KEY_Q
    root.size = Vector2i(1920, 1080)
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(6)
    await key(KEY_E)
    if not require(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START")) == "started", "controlled initial admission"):
        finish(); return
    if case_name == "battle":
        await battle_inputs()
    else:
        for i in range(500):
            if state().get("canWaitAtInput", false): break
            await process_frame
        if not require(state().get("canWaitAtInput", false), "settled field"): finish(); return
        if case_name == "story": await story_inputs()
        elif case_name == "service": await service_inputs()
        else: await field_inputs()
    finish()

func require(ok: bool, message: String) -> bool:
    check(ok, message)
    return ok

func input_state() -> Dictionary:
    return observation(host, "ReadSettingsHostObservationJson")

func menu_open() -> bool:
    return find("SystemMenuView") != null

func field_open() -> bool:
    return state().get("fieldMenu", {}).get("visible", false)

func unchanged_preferences() -> void:
    if original_preferences.is_empty(): check(not FileAccess.file_exists(preference_path), "default read creates no preferences")
    else: check(FileAccess.get_file_as_bytes(preference_path) == original_preferences, "loaded preferences retained byte for byte")

func field_inputs() -> void:
    if case_name == "default":
        var before_x: float = state().entities.filter(func(e): return e.id == "entity-0")[0].x
        await key(KEY_RIGHT)
        await frames(45)
        check(state().entities.filter(func(e): return e.id == "entity-0")[0].x > before_x and not menu_open(), "ordinary directional movement remains active")
        await key(KEY_LEFT)
        await frames(45)
    var applied := input_state()
    samples.append({"label": "installed", "input": applied})
    check(applied.hints.compact.Menu == OS.get_keycode_string(menu_key), "installed Menu hint follows selected mapping")
    check((find("SystemMenuEntry") as Button).tooltip_text.begins_with(applied.hints.compact.Menu), "top-right hint matches installed Menu")
    var entry_events := InputMap.action_get_events("sf2_cancel").filter(func(e: InputEvent): return e is InputEventKey and not InputMap.event_is_action(e, "sf2_menu", true))
    check((find("FieldMenuEntry") as Label).text.begins_with(OS.get_keycode_string(entry_events[0].keycode)), "Adventure hint excludes the shared Menu input")
    await key(cancel_key)
    check(field_open() and not menu_open(), "Cancel-only binding opens field action menu")
    await edge(menu_key, true)
    await frames(15)
    check(not field_open() and not menu_open(), "held Menu backs out of field child without opening system Menu")
    await edge(menu_key, false)
    await key(menu_key)
    if not require(menu_open(), "fresh Menu opens system Menu at base"): return
    await key(confirm_key)
    if not require(find("SettingsView") != null, "system Settings opens"): return
    await activate("input")
    check(settings().page == "Controls", "Settings category entered")
    await edge(menu_key, true)
    await frames(12)
    check(settings().page == "Root", "held Menu returns exactly one Settings layer")
    await edge(menu_key, false)
    await key(menu_key)
    check(find("SettingsView") == null and menu_open(), "Settings Back returns system Menu")
    await edge(menu_key, true)
    await frames(12)
    check(not menu_open() and not field_open(), "held host close cannot enter gameplay menu")
    await edge(menu_key, false)
    unchanged_preferences()
    if case_name in ["default", "old-default", "custom"]:
        await pad(JOY_BUTTON_START)
        check(menu_open(), "Start opens Menu with pad hints")
        check(input_state().hints.device == "pad", "pad input refreshes installed hints")
        check((find("SystemMenuEntry") as Button).tooltip_text == input_state().hints.compact.Menu, "pad Menu prompt follows actual Start mapping")
        await pad(JOY_BUTTON_START)
        check(not menu_open(), "Start closes Menu")
    if case_name == "custom":
        await key(KEY_ESCAPE)
        check(not menu_open() and not field_open(), "retired Escape has no raw shortcut")
    if case_name == "default":
        await edge(menu_key, true)
        root.emit_signal("focus_exited")
        await frames()
        root.emit_signal("focus_entered")
        await frames()
        check(menu_open() and input_state().releaseBarrier, "held opener remains blocked across focus signals")
        await edge(menu_key, false)
        await key(menu_key)
        check(not menu_open() and not field_open(), "fresh Menu closes after focus recovery")
        await remap_menu()

func activate(id: String) -> void:
    for i in range(24):
        if settings().get("focus") == id:
            await key(confirm_key); return
        await key(KEY_DOWN)
    check(false, "Settings row reachable: " + id)

func remap_menu() -> void:
    await key(KEY_ESCAPE)
    await key(KEY_ENTER)
    await activate("input")
    await activate("edit")
    await activate("action-menu")
    await activate("keyboard")
    await activate("slot-0")
    await activate("replace")
    if not require(settings().page == "Capture", "Menu keyboard capture reached"): return
    await key(KEY_ENTER)
    check(settings().page == "Capture" and not settings().message.is_empty(), "Menu rejects a conflicting Confirm binding")
    await key(KEY_F6)
    if not require(settings().page == "Review", "fresh replacement captured"): return
    await key(KEY_ENTER)
    for i in range(8):
        if settings().page == "Root": break
        await key(KEY_ESCAPE)
    await activate("apply")
    check(input_state().hints.compact.Menu == "F6", "Apply refreshes Menu hint")
    await key(KEY_F6)
    check(not menu_open(), "remapped Menu closes host")
    await key(KEY_ESCAPE)
    check(field_open() and not menu_open(), "Escape resumes Cancel-only role after remap")
    await key(KEY_ESCAPE)
    await key(KEY_F6)
    check(menu_open(), "F6 opens host after remap")
    samples.append({"label": "remapped", "input": input_state()})
    var saved: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(preference_path))
    check(saved.bindings.menu.keys == ["F6"], "only explicit remap persisted")

func story_inputs() -> void:
    await key(KEY_LEFT)
    await frames(30)
    await key(KEY_ENTER)
    for i in range(600):
        if state().get("canWaitForText", false): break
        await process_frame
    if not require(state().get("canWaitForText", false), "ordinary NPC Talk reaches text"): return
    var before: Variant = state().get("revision")
    await pad(JOY_BUTTON_START)
    check(not menu_open() and state().get("revision") == before, "Menu-only Start neither covers nor acknowledges text")
    await key(KEY_ESCAPE)
    check(not menu_open(), "Escape retains text Cancel semantics without opening host")
    sample("text-cancel")
    unchanged_preferences()

func service_inputs() -> void:
    await key(KEY_ENTER)
    for i in range(500):
        if state().get("shop", {}).get("visible", false): break
        if state().get("canWaitForText", false): await key(KEY_ENTER)
        await process_frame
    if not require(state().shop.visible, "ordinary Talk opens Galam shop"): return
    await key(KEY_ENTER)
    samples.append({"label": "shop-child", "shop": state().shop})
    await edge(KEY_ESCAPE, true)
    await frames(12)
    check(not menu_open() and state().shop.visible, "held Escape returns shop child without opening host")
    await edge(KEY_ESCAPE, false)
    await pad(JOY_BUTTON_START)
    check(not menu_open(), "Start follows service Cancel without opening host")
    sample("service-back")
    unchanged_preferences()

func battle_inputs() -> void:
    for i in range(2400):
        if state().get("stage") == "Movement" and state().get("movement") == null: break
        if state().get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        await process_frame
        OS.delay_msec(1)
    if not require(state().get("stage") == "Movement", "connected battle reaches Movement"): return
    await key(KEY_ESCAPE)
    check(menu_open(), "Escape opens host from Movement")
    await key(KEY_ESCAPE)
    await pad(JOY_BUTTON_START)
    check(menu_open(), "Start opens host from Movement")
    await pad(JOY_BUTTON_START)
    await key(KEY_ENTER)
    check(state().stage == "ActionChoice", "Confirm still opens battle actions")
    samples.append({"label": "battle-actions", "actor": state().actor, "choices": state().battleChoices})
    await key(KEY_I)
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    samples.append({"label": "target", "stage": state().stage, "menu": state().menu})
    if not require(state().stage in ["TargetChoice", "CommitReady"], "offered Medical Herb reaches target selection"): return
    await edge(KEY_ESCAPE, true)
    await frames(12)
    check(state().stage == "ActionChoice" and not menu_open(), "held target cancel returns actions without host")
    await edge(KEY_ESCAPE, false)
    await pad(JOY_BUTTON_START)
    check(state().stage == "ActionChoice" and state().menu.page == "Main" and not menu_open(), "Start returns Item list to action root")
    await key(KEY_ESCAPE)
    check(state().stage == "Movement" and not menu_open(), "Escape returns ActionChoice to Movement")
    await key(KEY_X)
    if not require(state().inspection.active, "X retains battle inspection entry"): return
    await key(KEY_ENTER)
    samples.append({"label": "inspection-child", "inspection": state().inspection})
    await edge(KEY_ESCAPE, true)
    await frames(12)
    check(not menu_open(), "inspection child cancellation cannot open host")
    await edge(KEY_ESCAPE, false)
    for i in range(4):
        if not state().inspection.active: break
        await key(KEY_ESCAPE)
        check(not menu_open(), "each inspection Back stays in gameplay")
    check(not state().inspection.active and state().stage == "Movement", "inspection returns Movement")
    await key(KEY_ESCAPE)
    check(menu_open(), "fresh Escape opens Menu after inspection release")
    sample("battle-input")
    unchanged_preferences()
