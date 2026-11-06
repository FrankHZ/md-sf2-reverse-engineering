extends "res://probes/engine_battle_save_observation.gd"

# Actual Options caller over the existing save observer and store. No state setters.
var caller: Dictionary = {}
var mode := OS.get_environment("SF2_QUIT_CASE")

func wait_battle() -> bool:
    var deadline := Time.get_ticks_msec() + 20000
    while Time.get_ticks_msec() < deadline:
        # The base observer's attach signal establishes that projection is initialized.
        if view != null and view.name == "BattleSessionView" and view.is_visible_in_tree():
            var current := state()
            if current.get("stage") == "Movement" and current.get("stopReason") == "PlayerInput": return true
        elif view != null and view.name == "ExplorationSessionView":
            var field := state()
            if field.get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        OS.delay_msec(1); await process_frame
    check(false, "Attached battle Movement reached"); return false

func slot_bytes(slot: int) -> PackedByteArray:
    return FileAccess.get_file_as_bytes(OS.get_environment("SF2_SAVE_ROOT") + "/" +
        OS.get_environment("SF2_SAVE_NAMESPACE") + "/sf2-opening/slot%d.json" % slot)

func inspection() -> Dictionary:
    return state().inspection

func returned(label: String) -> void:
    await frames(4)
    check(panel().is_empty(), label + ": modal closed")
    var current := inspection()
    check(current.active and current.page == "Options" and current.optionIndex == 3,
        label + ": same Options Quit focus")
    check(current.cursor == caller.cursor and current.actor == caller.actor and current.sessionId == caller.sessionId,
        label + ": same cursor actor session")
    check(current.revision == state().revision, label + ": current revision rebound")
    sample(label)

func enter_options() -> bool:
    await key(KEY_X)
    if not inspection().active:
        check(false, "Movement B opens inspection"); return false
    for direction in [KEY_RIGHT, KEY_DOWN, KEY_LEFT, KEY_UP]:
        await key(direction)
        var i := inspection()
        var occupied := false
        for u in i.units:
            if u.Position == i.cursor: occupied = true
        if not occupied: break
    await key(KEY_ENTER)
    check(inspection().page == "Options", "Empty-ground C opens Options")
    await key(KEY_UP)
    caller = inspection().duplicate(true)
    check(caller.optionIndex == 3 and "Quit" in caller.hint, "Quit is actual selected option")
    sample("options-quit")
    return failures.is_empty()

func open_quit() -> bool:
    await key(KEY_ENTER)
    if not await wait_panel("save-slots"): return false
    check(panel().choices.size() == 3, "Shared two slots and Cancel")
    sample("quit-two-slots")
    return true

func confirm_slot(slot: int) -> bool:
    if slot == 2: await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(panel().get("state") == "confirm-save", "Explicit slot consent")
    sample("confirm-slot%d" % slot)
    return failures.is_empty()

func save_return(slot: int) -> bool:
    if not await open_quit() or not await confirm_slot(slot): return false
    await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_panel("saved"): return false
    check(not slot_bytes(slot).is_empty(), "Actual slot%d file published" % slot)
    sample("saved-slot%d" % slot)
    # Hold the return confirmation and try another mapped action before releasing it.
    var held := InputEventKey.new(); held.keycode = KEY_ENTER; held.pressed = true
    Input.parse_input_event(held); await frames(2)
    var revision = state().revision
    await key(KEY_DOWN)
    check(state().revision == revision and inspection().page == "Options" and inspection().optionIndex == 3,
        "Held confirmation cannot leak to caller/gameplay")
    held = InputEventKey.new(); held.keycode = KEY_ENTER; held.pressed = false
    Input.parse_input_event(held)
    await returned("saved-return-slot%d" % slot)
    return failures.is_empty()

func run() -> void:
    create_timer(50).timeout.connect(func(): check(false, "50-second deadline"); finish())
    node_added.connect(node_added_observe)
    root.size = Vector2i(1920,1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
    root.grab_focus(); await frames(3)
    if mode == "entry":
        samples.append({"label":"controlled-field-approach", "result":host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_BATTLE_SAVE_START"))})
        if not await wait_battle(): finish(); return
    elif mode == "unavailable":
        if not await wait_battle(): finish(); return
    elif not await continue_title(): finish(); return
    if mode == "resume":
        sample("quit-save-continued"); finish(); return
    var before := state()
    if not await enter_options(): finish(); return
    check(state().revision == before.revision, "Inspection/Quit offer browsing is pure before explicit Save")
    if mode == "unavailable":
        await key(KEY_ENTER)
        if not await wait_panel("error"): finish(); return
        sample("unavailable-profile")
        await key(KEY_X); await returned("unavailable-back")
        check(state().revision == before.revision, "Unavailable entry does not alter gameplay")
        finish(); return
    if mode == "entry":
        if not await open_quit(): finish(); return
        await key(KEY_X); await returned("slot-cancel")
        if not await save_return(1): finish(); return
        if not await save_return(2): finish(); return
        var bytes := slot_bytes(1)
        if not await open_quit() or not await confirm_slot(1): finish(); return
        check("Overwrite" in panel().message, "Occupied slot asks overwrite consent")
        await key(KEY_ENTER)
        if not await wait_panel("save-slots"): finish(); return
        await key(KEY_X); await returned("overwrite-cancel")
        check(slot_bytes(1) == bytes, "Overwrite Cancel preserves actual bytes")
        # Existing host Menu remains a separate usable entry with a pure pause/return.
        var rev = state().revision
        await key(KEY_F10)
        check(host.find_child("SystemMenuView",true,false) != null, "Host Menu retained")
        await frames(8); await key(KEY_X)
        check(state().revision == rev and inspection().page == "Options", "Host Menu pause/return preserves inspection")
        await key(KEY_X)
        check(inspection().page == "Cursor" and inspection().cursor == caller.cursor, "Options B restores same legal cursor")
        await key(KEY_X)
        check(not inspection().active and state().stage == "Movement", "Cursor B restores Movement")
        # Standalone authored save refusal is observed in its separate mode below.
        await key(KEY_UP); await frames(30)
        await key(KEY_F10)
        var reason := host.find_child("ReasonSave", true, false)
        check(reason != null and "Cancel movement" in reason.text, "Preview remains ineligible via host Menu")
        sample("preview-refused")
        await key(KEY_X)
    else:
        if not await open_quit() or not await confirm_slot(1): finish(); return
        await key(KEY_DOWN); await key(KEY_DOWN)
        sample("savequit-confirmed"); write_report()
        await key(KEY_ENTER)
        if mode == "save-quit":
            for n in range(10000):
                if panel().get("state") in ["error","save-uncertain"]:
                    check(false,"SaveQuit publication succeeded"); finish(); return
                OS.delay_msec(1); await process_frame
            check(false,"SaveQuit exits"); finish(); return
        var expected := "error" if mode == "storage-fail" else "save-uncertain"
        if not await wait_panel(expected): finish(); return
        sample("failed-publication")
        await key(KEY_X); await returned("failure-back")
        if mode == "uncertain":
            await key(KEY_ENTER)
            if not await wait_panel("save-uncertain"): finish(); return
            sample("retry-blocked")
            await key(KEY_ENTER)
            if not await wait_panel("continue-slots"): finish(); return
            sample("uncertain-inspect")
            await key(KEY_X); await returned("inspection-back")
        check(state().sessionId == before.sessionId and state().mainSeed == before.mainSeed and
            state().thinkingSeed == before.thinkingSeed, "Failure/inspection retain live session and RNG")
    finish()

func finish() -> void:
    if done: return
    done = true
    write_report()
    view = null
    if host != null: host.free(); host = null
    quit(0 if failures.is_empty() else 1)
