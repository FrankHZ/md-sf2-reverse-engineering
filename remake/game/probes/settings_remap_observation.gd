extends "res://probes/host_ui_observation.gd"

const Nav = preload("res://probes/settings_navigation.gd")
var prefs: String
var slot: String
var slot_bytes: PackedByteArray
func activate(id: String) -> void:
    if not await Nav.row(find("SettingsView"), id):
        check(false, "reachable row: " + id + " on " + str(settings().get("page"))); return
    await Nav.tap(self, "confirm")

func go_setting(id: String) -> void:
    if not await Nav.setting(find("SettingsView"), id):
        check(false, "reachable setting: " + id); return
    await Nav.tap(self, "confirm")

func back() -> void: await Nav.tap(self, "cancel")

func bytes(path: String) -> PackedByteArray:
    return FileAccess.get_file_as_bytes(path) if FileAccess.file_exists(path) else PackedByteArray()

func alternatives(action: String, device: String) -> void:
    await go_setting("edit")
    await activate("action-" + action)
    await activate(device)

func field_settings() -> void:
    await Nav.tap(self, "menu")
    for i in 6:
        if selected_menu() == "Settings": break
        await Nav.tap(self, "down")
    await Nav.tap(self, "confirm")
    check(settings().get("page") == "Root", "settled field opens Settings")

func continue_slot() -> bool:
    var entry := find("SessionEntryView")
    if observation(entry, "ReadEntryObservationJson").stage == "title":
        await key(KEY_ENTER); await key(KEY_E)
    var choice := find("Continue") as Button
    for i in 5:
        if choice.theme_type_variation == "EntrySelectedButton": break
        await Nav.tap(self, "down")
    await Nav.tap(self, "confirm")
    if not await wait_slots("continue-slots"): return false
    await Nav.tap(self,"confirm")
    var deadline := Time.get_ticks_msec() + 20000
    while not state().get("canWaitAtInput", false) and Time.get_ticks_msec() < deadline:
        await frames(3)
    check(state().get("canWaitAtInput",false), "ordinary Continue reaches Ribble")
    return state().get("canWaitAtInput",false)

func to_root() -> void:
    for level in 8:
        if settings().get("page") == "Root": return
        await back()
    check(false, "Back reaches categories")

func remap_q() -> void:
    await alternatives("accelerate","keyboard")
    await activate("slot-0"); await activate("replace")
    check(settings().page == "Capture","opening Confirm cannot bind itself")
    await edge(KEY_Q,true)
    check(settings().page == "Review" and "Q" in settings().message,"Q is a proposal")
    await edge(KEY_ENTER,true); await edge(KEY_ENTER,false)
    check(settings().page == "Review","held Q cannot accept a review")
    await edge(KEY_Q,false); await activate("accept")
    check(settings().draft.Bindings.accelerate.Keys == ["Q"] and "Shift" in settings().bindings.Accelerate,"accepted Q is draft only")
    await to_root(); await go_setting("help")
    var help := find("ControlsHelpView")
    check(help != null,"Help available with staged Q")
    for i in 16:
        var page := observation(help,"ReadControlsHelpObservationJson")
        if page.rows.any(func(r): return r.action == "Accelerate"):
            check("Shift" in page.body and not "Q" in page.rows.filter(func(r): return r.action == "Accelerate")[0].binding,"Help still reads installed Shift")
            break
        await Nav.tap(self,"right")
    await back()
    check(settings().focus == "help" and settings().draft.Bindings.accelerate.Keys == ["Q"],"Help Back keeps focus and pending Q")
    await to_root()

func hold(code: int, expected: float) -> void:
    await edge(code,true)
    check(state().acceleration.rate == expected, "field hold " + OS.get_keycode_string(code) + " = " + str(expected))
    await edge(code,false)
    check(state().acceleration.rate == 1,"field release restores normal speed")

func mounted() -> Dictionary: return observation(host,"ReadSettingsHostObservationJson")
func progress() -> Variant: return observation(host,"ReadPersistenceObservationJson").progress
func native_field() -> void:
    await field_settings()
    if settings().is_empty(): return
    var field := state()
    var field_progress: Variant = progress()
    await go_setting("graphics")
    var details := find("SettingsMessage") as Label
    check(details.is_visible_in_tree() and "HD" in details.text and details.get_line_count() > 1,
        "F10 Display shows the wrapped field graphics explanation")
    var row := find("Setting_graphics") as Button
    var draft: Dictionary = settings().draft
    await mouse(row, true); await mouse(row, false)
    check(find("Setting_graphics") == row and settings().draft.GraphicsMode != draft.GraphicsMode,
        "pointer edits the same mounted value row")
    await back(); await go_setting("help")
    var help := find("ControlsHelpView")
    var next := find("HelpNext") as Button
    await mouse(next, true); await mouse(next, false)
    check(observation(help, "ReadControlsHelpObservationJson").page == 1 and root.gui_get_focus_owner() == next,
        "native Help Next turns once and owns focus")
    await pad(JOY_BUTTON_DPAD_LEFT)
    check(observation(help, "ReadControlsHelpObservationJson").page == 0 and root.gui_get_focus_owner() == find("HelpPrevious"),
        "mapped pad turns the Help page back")
    for pressed in [true, false]:
        var click := InputEventMouseButton.new()
        click.position = next.get_global_rect().get_center()
        click.button_index = MOUSE_BUTTON_RIGHT; click.pressed = pressed
        Input.parse_input_event(click); await frames()
    check(find("ControlsHelpView") == null and root.gui_get_focus_owner() == find("Setting_help"),
        "matched right-click over a native button restores Help's caller")
    await to_root()
    var apply := find("Setting_apply") as Button
    await mouse(apply, true); await mouse(apply, false)
    check(settings().is_empty() and selected_menu() == "Settings" and mounted().remapRelease == null,
        "released mouse Apply returns to Menu without arming a key drain")
    check(state().revision == field.revision, "Apply preserves the covered field revision")
    await back()
    check(state().sessionId == field.sessionId and state().map == field.map and progress() == field_progress
        and state().movementDirection == null, "resume keeps the field caller without admitting movement")
    check(bytes(slot) == slot_bytes, "field settings leave journey bytes unchanged")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1774-"):
        push_error("Choose a disposable issue1774-* save copy and explicit output"); quit(2); return
    var directory := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    prefs = directory.path_join("sf2-preferences/input.json"); slot = directory.path_join("sf2-opening/slot1.json")
    var before := bytes(prefs); slot_bytes = bytes(slot)
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(8)
    if case_name == "native-field":
        await native_field(); finish(); return
    if case_name != "restart":
        await open_witch_settings()
        if settings().is_empty(): finish(); return
        check(settings().rows == ["display","assist","input","debug","audio","apply","reset-page"] and settings().message.is_empty(),"root shows only categories and actions")
        await remap_q(); await back()
        check(bytes(prefs) == before, "cancel at Witch retains exact preferences")
    if not await continue_slot(): finish(); return
    if case_name == "restart":
        check(mounted().applied.Bindings.accelerate.Keys == ["Q"],"fresh process loads edited row")
    else:
        await hold(KEY_SHIFT,1.5); await hold(KEY_Q,1)
        await field_settings()
        var paused: Variant = progress(); var revision: Variant = state().revision; var session: Variant = state().sessionId
        await remap_q()
        await go_setting("apply")
        check(settings().is_empty() and selected_menu() == "Settings","Apply returns to same field menu choice")
        check(progress() == paused and state().revision == revision and state().sessionId == session, "Apply preserves session, revision and save projection")
        await back()
        check(JSON.parse_string(FileAccess.get_file_as_string(prefs)).bindings.accelerate.keys == ["Q"],"only applied binding persisted")
        await field_settings()
        var covered_progress: Variant = progress()
        var paused_state := state()
        await edge(KEY_V,true); await edge(KEY_Q,true); await edge(KEY_RIGHT,true)
        await back(); await edge(KEY_RIGHT,false); await back(); await frames(8)
        var returned := state()
        samples.append({"label":"held return","menu":selected_menu(),"movement":returned.movementDirection,"rate":returned.acceleration.rate,
            "revision":[paused_state.revision,returned.revision],"rng":[paused_state.mainSeed,returned.mainSeed,paused_state.thinkingSeed,returned.thinkingSeed],
            "progressChanged":progress() != covered_progress})
        check(find("SystemMenuView") == null and returned.movementDirection == null and returned.acceleration.rate == 1, "held controls cannot arm on covered return")
        check(not returned.waitingAtInput and progress() == covered_progress and returned.mainSeed == paused_state.mainSeed
            and returned.thinkingSeed == paused_state.thinkingSeed, "resumed walking clock does not admit covered Wait, progress or RNG changes")
        await edge(KEY_Q,false); await edge(KEY_V,false)
    await hold(KEY_Q,1.5); await hold(KEY_SHIFT,1)
    var hint := find("ContextHint").find_child("Help",true,false) as Label
    check("Q" in hint.text and not "Shift" in hint.text and hint.get_theme_font_size("font_size") == 24, "field advertises installed 24px Q")
    check(bytes(slot) == slot_bytes,"journey slot byte-for-byte unchanged")
    samples.append({"case":case_name,"field":state().map,"hint":hint.text,"bindings":mounted().applied.Bindings})
    finish()
