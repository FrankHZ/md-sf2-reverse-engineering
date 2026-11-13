extends "res://probes/settings_remap_observation.gd"

func joy_axis(value: float) -> void:
    var e := InputEventJoypadMotion.new(); e.device = 4; e.axis = JOY_AXIS_RIGHT_Y; e.axis_value = value
    Input.parse_input_event(e); await frames(2)

func confirm_review() -> void:
    if settings().page != "Review": check(false,"expected review: " + settings().message); return
    await activate("accept")

func basic_edges() -> void:
    await pad(JOY_BUTTON_DPAD_DOWN); await pad(JOY_BUTTON_DPAD_DOWN); await pad(JOY_BUTTON_A)
    check(settings().page == "Controls", "installed pad navigates categories")
    await go_setting("edit")
    check(settings().rows.size() == 18,"17 actions plus Back")
    await activate("action-accelerate"); await activate("keyboard")
    await activate("slot-0"); await activate("replace")
    await key(KEY_Z)
    check(settings().page == "Capture" and "Z" in settings().message and ("Confirm" in settings().message or "确认" in settings().message),"inherited conflict identifies current owner")
    await key(KEY_ESCAPE)
    check(settings().page == "Operation","physical Escape aborts one capture")
    await activate("replace")
    var button := find("Setting_back") as Button
    await mouse(button,true); await mouse(button,false)
    check(settings().page == "Operation","complete pointer Cancel aborts capture")
    await activate("replace"); await key(KEY_Q)
    root.focus_exited.emit(); await frames(2); root.focus_entered.emit(); await frames(2)
    check(settings().page == "Operation" and settings().draft.Bindings.is_empty(),"focus loss abandons reviewed proposal")
    await activate("replace"); await key(KEY_Q); await confirm_review()
    await back(); await activate("pad")
    await activate("add"); await joy_axis(-0.3); await joy_axis(-0.51)
    check(settings().page == "Review","negative right stick captured")
    await joy_axis(-0.42); await Nav.tap(self,"confirm")
    check(settings().page == "Review","axis chatter cannot release review")
    await joy_axis(0.2)
    Input.joy_connection_changed.emit(4,false); await frames()
    check(settings().page == "Alternatives" and settings().draft.Bindings.accelerate.Axes.is_empty(),"capturing pad disconnect discards unpublished review")
    await activate("reserved")
    check(settings().page == "Alternatives" and not settings().message.is_empty(),"explicit controller Back reports Item conflict")
    await activate("add"); await pad(JOY_BUTTON_BACK)
    check(settings().page == "Alternatives","physical pad Back aborts")
    await activate("add"); await joy_axis(0); await joy_axis(-0.8); await joy_axis(0); await confirm_review()
    check(settings().draft.Bindings.accelerate.Axes == ["RightY-"],"signed axis staged alongside Q")
    await activate("add"); await joy_axis(-0.8); await joy_axis(0)
    check(settings().page == "Capture" and not settings().message.is_empty(),"same-row duplicate refused")
    await key(KEY_ESCAPE); await to_root()
    # Nonbinding Defaults retains edits; Reset bindings retains the rest of the draft, in either order.
    await go_setting("flash"); await go_setting("defaults")
    check(settings().draft.Bindings.accelerate.Keys == ["Q"] and not settings().draft.ReducedFlash,"Defaults keeps pending bindings")
    await go_setting("convention"); await go_setting("reset")
    check(settings().draft.Bindings.is_empty() and settings().draft.ConfirmCancel == "swapped","Reset keeps staged convention")
    await go_setting("defaults")
    check(settings().draft.Bindings.is_empty() and settings().draft.ConfirmCancel == "standard","reverse order remains a draft")
    await to_root()
    await alternatives("confirm","keyboard")
    await activate("slot-1"); await activate("replace"); await key(KEY_SHIFT)
    check(settings().page == "Capture" and "Shift" in settings().message, "Shift is restricted to Accelerate")
    await key(KEY_Q); await confirm_review()
    check(settings().draft.Bindings.confirm.Keys == ["Enter","Q","Z"],"replace preserves order of siblings")
    await activate("add"); await key(KEY_P); await confirm_review()
    check(settings().draft.Bindings.confirm.Keys == ["Enter","Q","Z","P"],"append preserves order")
    await activate("slot-2"); await activate("remove"); await confirm_review()
    check(settings().draft.Bindings.confirm.Keys == ["Enter","Q","P"],"remove preserves other keys")
    await back(); await activate("pad")
    await activate("slot-0"); await activate("remove"); await confirm_review()
    await to_root()
    check((find("Setting_apply") as Button).disabled and not settings().message.is_empty(),"missing controller disables Apply")
    await alternatives("cancel","keyboard")
    await activate("slot-0"); await activate("replace"); await key(KEY_B); await confirm_review()
    await to_root()
    await alternatives("confirm","pad"); await activate("add"); await pad(JOY_BUTTON_A); await confirm_review()
    await back(); await activate("keyboard"); await activate("reserved"); await confirm_review()
    check("Escape" in settings().draft.Bindings.confirm.Keys,"explicit Escape assignable")
    await to_root(); await go_setting("convention"); await to_root()
    check(await Nav.setting(find("SettingsView"),"apply"),"Apply reachable with installed Confirm")
    await edge(KEY_ENTER,true); await frames(6)
    check(mounted().remapRelease != null and settings().is_empty(),"Apply drains old effective Confirm through release")
    await edge(KEY_ENTER,false)
    check(settings().is_empty() and mounted().hints.compact.Cancel == "Enter" and mounted().hints.compact.Confirm == "B",
        "repair and staged swap publish base rows once")
    await key(KEY_B)
    check(settings().page == "Root","new Confirm operates Witch Settings")
    await key(KEY_ENTER)
    check(settings().is_empty(),"new effective Back closes Settings")

func all_rows() -> void:
    var keys := [KEY_F1,KEY_F2,KEY_F3,KEY_F4,KEY_F5,KEY_F6,KEY_F7,KEY_F8,KEY_F9,KEY_F11,KEY_F12,KEY_F13,KEY_F14,KEY_F15,KEY_F16,KEY_F17,KEY_F18]
    await go_setting("edit")
    var names: Array = settings().rows.filter(func(id): return id.begins_with("action-"))
    for i in names.size():
        await activate(names[i]); await activate("keyboard"); await activate("slot-0"); await activate("replace")
        await key(keys[i]); await confirm_review()
        await back(); await back()
    await to_root(); await go_setting("apply")
    for i in names.size():
        var action: String = names[i].trim_prefix("action-")
        var events := InputMap.action_get_events("sf2_" + action)
        check(events[0] is InputEventKey and events[0].keycode == keys[i],"edited binding installed: " + action)
    check(mounted().applied.Bindings.size() == 17,"all seventeen edited base rows persisted")

func consumer_setup() -> void:
    for edit in [["accelerate",KEY_Q],["confirm",KEY_F5],["cancel",KEY_F6],["down",KEY_J]]:
        await alternatives(edit[0],"keyboard"); await activate("slot-0"); await activate("replace")
        await key(edit[1]); await confirm_review(); await to_root()
    await alternatives("accelerate","pad"); await activate("add"); await joy_axis(0); await joy_axis(-1); await joy_axis(0); await confirm_review()
    await to_root(); await go_setting("help")
    var help := find("ControlsHelpView")
    var old := observation(help,"ReadControlsHelpObservationJson")
    await pad(JOY_BUTTON_GUIDE); var new := observation(help,"ReadControlsHelpObservationJson")
    check(old.hint != new.hint and old.page == new.page and old.scroll == new.scroll,"Help family switch retains reading")
    var m := InputEventMouseMotion.new(); m.position = Vector2(530,430); m.relative = Vector2(12,0); Input.parse_input_event(m); await frames(3)
    check(observation(help,"ReadControlsHelpObservationJson").hint == old.hint,"mouse restores installed keyboard cues")
    await back(); await to_root(); await go_setting("apply")
    check(mounted().hints.compact.Confirm == "F5" and mounted().hints.compact.Cancel == "F6","edited confirm/cancel installed for downstream routes")

func boundary32() -> void:
    await alternatives("accelerate","keyboard"); await activate("add"); await key(KEY_Q); await confirm_review()
    check(settings().draft.Bindings.accelerate.Keys.size() == 32,"32nd key admitted")
    await activate("add"); await key(KEY_B)
    check(settings().page == "Capture" and "32" in settings().message and settings().draft.Bindings.accelerate.Keys.size() == 32,"33rd refused without mutation")
    await key(KEY_ESCAPE); await to_root(); await go_setting("apply")
    check(mounted().applied.Bindings.accelerate.Keys.size() == 32,"all 32 installed and persisted")

func compatibility() -> void:
    var original: Dictionary = mounted().applied
    check(not original.Bindings.has("menu") and InputMap.action_get_events("sf2_menu").size() == 1,"legacy Menu is implicitly keyboard-only")
    var menu_key: String = mounted().hints.compact.Menu
    await go_setting("flash"); await go_setting("apply")
    check(not JSON.parse_string(FileAccess.get_file_as_string(prefs)).bindings.has("menu") and
        InputMap.action_get_events("sf2_menu").size() == 1 and mounted().hints.compact.Menu == menu_key,"unrelated edit preserves implicit Menu fallback")

func recovery() -> void:
    var initial := bytes(prefs)
    await go_setting("reset"); await go_setting("apply")
    check(settings().page == "Root" and bytes(prefs) == initial and "Restore defaults" in settings().message,"resetting bindings alone cannot recover corrupt file")
    await go_setting("defaults"); await go_setting("apply")
    var backups := Array(DirAccess.get_files_at(prefs.get_base_dir())).filter(func(p): return p.ends_with(".invalid.json"))
    check(settings().is_empty() and backups.size() == 1 and bytes(prefs.get_base_dir().path_join(backups[0])) == initial,"explicit recovery retains exact corrupt bytes")
    check(JSON.parse_string(FileAccess.get_file_as_string(prefs)).bindings.is_empty(),"recovered ordinary preferences readable")

func refuse() -> void:
    var initial := bytes(prefs); var installed: Dictionary = mounted().applied
    await alternatives("accelerate","keyboard"); await activate("slot-0"); await activate("replace"); await key(KEY_Q); await confirm_review(); await to_root()
    if case_name == "external":
        var changed := FileAccess.open(prefs, FileAccess.WRITE); changed.store_string("external owner"); changed.close()
        initial = bytes(prefs)
    await go_setting("apply")
    check(settings().page == "Root" and not settings().message.is_empty() and settings().draft.Bindings.accelerate.Keys == ["Q"],"write refusal retains editable draft and reason")
    check(bytes(prefs) == initial and mounted().applied == installed,"refusal preserves exact existing bytes and live map")
    await back()

func reader() -> void:
    root.size = Vector2i(1280,720); await frames(6)
    geometry("SettingsWindow")
    await go_setting("edit"); await activate("action-accelerate"); await activate("keyboard"); await activate("slot-0"); await activate("replace")
    await key(KEY_Q)
    geometry("SettingsWindow")
    var msg := find("SettingsMessage") as Label
    var scroll := find("SettingsReading") as Control
    check(scroll.get_global_rect().size.x >= msg.get_global_rect().size.x and msg.get_line_count() >= 3 and
        (find("SettingsHint") as Label).get_theme_font_size("font_size") == 24, "720p wraps the binding review with fixed hint size")
    await confirm_review(); await to_root(); await go_setting("help")
    geometry("HelpWindow")
    check((find("HelpHint") as Label).get_theme_font_size("font_size") == 24,"720p reader keeps native help hint size")
    await back(); await to_root(); await back()
    check(mounted().applied.Bindings.is_empty(),"reader Cancel retains installed bindings")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1774-"): quit(2); return
    var directory := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    prefs = directory.path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920,1080)
    if case_name == "reader":
        for path in ["res://localization/en.po","res://localization/zh_CN.po"]:
            var catalog := load(path) as Translation
            catalog.add_message("settings.pending","Draft / 草稿 ".repeat(24) + "{0}")
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(10)
    if case_name == "swap-restart":
        var hints: Dictionary = mounted().hints.compact
        check(hints.Confirm == "B" and hints.Cancel == "Enter","restart retains effective custom base pair and convention")
        await key(KEY_B); await key(KEY_E)
        for i in 5:
            if (find("Settings") as Button).theme_type_variation == "EntrySelectedButton": break
            await Nav.tap(self,"up")
        await key(KEY_B); check(settings().get("page") == "Root","persisted Confirm opens Settings"); await key(KEY_ENTER)
        check(settings().is_empty(),"persisted Cancel closes Settings")
        finish(); return
    await open_witch_settings()
    match case_name:
        "reader": await reader()
        "edges": await basic_edges()
        "all": await all_rows()
        "consumer": await consumer_setup()
        "32": await boundary32()
        "compat": await compatibility()
        "recovery": await recovery()
        "locked","external": await refuse()
        "override":
            var before := bytes(prefs); var override_path: String = OS.get_cmdline_user_args()[-1]; var ov := bytes(override_path)
            check(settings().readOnly and (find("Setting_apply") as Button).disabled,"override cannot apply")
            await activate("input"); check((find("Setting_edit") as Button).disabled and (find("Setting_reset") as Button).disabled,"override cannot edit or reset bindings")
            await activate("help"); check(settings().help,"read-only Help remains available")
            await back(); await back(); await back()
            check(settings().is_empty() and bytes(prefs) == before and bytes(override_path) == ov,"override and ordinary file remain untouched")
    finish()
