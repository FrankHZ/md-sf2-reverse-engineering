extends "res://probes/settings_remap_observation.gd"

func battle_state() -> Array:
    var s := state()
    var vitals: Array = s.actors.map(func(actor): return [actor.id, actor.hp, actor.mp, actor.exp])
    return [s.sessionId, s.revision, s.observationSequence, s.round, s.queueCursor, s.turnOrder,
        s.selection, s.candidate, s.menu.page, s.menu.row, s.menu.command, s.mainSeed, s.thinkingSeed,
        s.gold, vitals, s.movement, s.scene.waitToken, s.scene.phase, s.scene.elapsed, s.failure]

func open_general() -> bool:
    await Nav.tap(self, "menu")
    var row := find("SystemSettings") as Button
    if row == null or row.disabled:
        check(false, "battle system Menu offers Settings"); return false
    for i in range(5):
        if selected_menu() == "Settings": break
        await Nav.tap(self, "down")
    await Nav.tap(self, "confirm")
    var opened: bool = settings().get("page") == "Root" and not settings().battleOnly
    check(opened, "battle opens the shared Settings categories")
    return opened

func target_choice() -> bool:
    await Nav.tap(self, "confirm")
    await Nav.tap(self, "attack")
    var first: Variant = state().candidate
    await Nav.tap(self, "target")
    var ready: bool = state().menu.page == "Target" and state().candidate != null and state().candidate != first
    check(ready, "real targeting offers a second candidate")
    return ready

func settings_layout(ids: Array) -> Array:
    var measured: Array = [settings().scroll]
    for name_ in ["SettingsReading", "SettingsDetails", "SettingsActions", "SettingsHint"]:
        var control := find(name_) as Control
        measured.append(control.get_global_rect() if control != null else Rect2())
    for id in ids: measured.append((find("Setting_" + id) as Button).get_global_rect())
    return measured

func check_display_layout(locale: String) -> void:
    await go_setting("display")
    var ids := ["language", "graphics", "text", "speed"]
    var shown := settings_layout(ids)
    var message := find("SettingsMessage") as Label
    check(settings().locale == locale and settings().focus == "language" and message.visible
        and message.get_theme_font_size("font_size") == 30, locale + " description uses fixed reading text")
    for i in range(3): await Nav.tap(self, "down")
    check(settings().focus == "speed" and not message.visible and settings_layout(ids) == shown,
        locale + " switching to a row without a description preserves positions and row scroll")
    await Nav.tap(self, "up")
    check(settings().focus == "text" and message.visible and settings_layout(ids) == shown,
        locale + " switching back to a row with a description preserves positions and row scroll")

func native_root() -> void:
    if not await target_choice() or not await open_general(): return
    var covered := battle_state()
    check(root.gui_get_focus_owner() == find("Setting_display"), "Display owns actual focus on entry")
    await Nav.tap(self, "down")
    check(root.gui_get_focus_owner() == find("Setting_assist"), "mapped keyboard follows the root neighbour")
    var down: InputEventJoypadButton = InputMap.action_get_events("sf2_down").filter(func(e): return e is InputEventJoypadButton)[0]
    await pad(down.button_index)
    check(root.gui_get_focus_owner() == find("Setting_input"), "mapped controller follows the same neighbour")
    await key(KEY_TAB)
    check(root.gui_get_focus_owner() == find("Setting_input"), "mapped Read details cannot also use built-in Tab navigation")
    var display := find("Setting_display") as Button
    var hover := InputEventMouseMotion.new()
    hover.position = display.get_global_rect().get_center(); hover.relative = Vector2(12, 0)
    Input.parse_input_event(hover); await frames()
    check(root.gui_get_focus_owner() == display, "deliberate GUI motion moves reading focus")
    await Nav.tap(self, "down")
    check(root.gui_get_focus_owner() == find("Setting_assist"), "stationary pointer leaves keyboard focus alone")
    await mouse(display, true)
    check(settings().get("page") == "Root", "pointer press cannot open a page")
    await mouse(display, false)
    check(settings().get("page") == "Display", "GUI-picked release opens Display once")
    var language := find("Setting_language") as Button
    var draft: Dictionary = settings().draft
    await mouse(language, true)
    root.size = Vector2i(1280,720); await frames()
    check(window().recovery != null and not find("SettingsView").is_visible_in_tree(), "undersized client covers Settings")
    root.size = Vector2i(1920,1080); await frames()
    await mouse(language, false)
    check(settings().draft == draft and root.gui_get_focus_owner() == language,
        "window recovery restores focus and vetoes the old release")
    await mouse(language, true)
    root.focus_exited.emit(); await frames()
    root.focus_entered.emit(); await frames()
    await mouse(language, false)
    check(settings().draft == draft, "focus loss during a press vetoes activation")
    await mouse(language, true)
    for position in [language.get_global_rect().get_center() + Vector2(0, 95), language.get_global_rect().get_center()]:
        var motion := InputEventMouseMotion.new()
        motion.position = position
        motion.relative = Vector2(0, 95)
        motion.button_mask = MOUSE_BUTTON_MASK_LEFT
        Input.parse_input_event(motion); await frames()
    await mouse(language, false)
    check(settings().draft == draft, "dragging out and returning cannot rearm a native button")
    await mouse(language, true); await mouse(language, false)
    check(find("Setting_language") == language and settings().draft.Language != draft.Language,
        "fresh GUI click edits the focused row in place")
    await back()
    check(root.gui_get_focus_owner() == find("Setting_display"), "Back restores native focus to Display")
    await back(); await back()
    check(battle_state() == covered, "cancel and resume preserve the target")

func transaction() -> void:
    if not await target_choice(): return
    if not await open_general(): return
    var original := battle_state()
    var candidate: Variant = state().candidate
    var before := bytes(prefs)
    await check_display_layout("en")
    await go_setting("language"); await to_root()
    await go_setting("help")
    check(find("ControlsHelpView") != null, "Controls Help opens over paused targeting")
    await back(); await to_root()
    await back()
    check(selected_menu() == "Settings" and battle_state() == original, "Cancel returns to the same target and menu row")
    check(bytes(prefs) == before and mounted().applied.Language == "en", "Cancel leaves preferences unchanged")
    await back()

    if not await open_general(): return
    await go_setting("language")
    await go_setting("graphics")
    check("HD artwork where installed" in settings().message, "graphics explains battle artwork coverage")
    await go_setting("text")
    check("next message" in settings().message, "text mode explains its message boundary")
    await go_setting("speed")
    await go_setting("flash")
    await go_setting("guidance")
    await go_setting("diagnostics")
    await alternatives("confirm", "keyboard")
    await activate("slot-0"); await activate("replace")
    await edge(KEY_F5, true)
    check(settings().page == "Review", "replacement Confirm is captured in the draft")
    root.size = Vector2i(1280,720); await frames()
    check(window().recovery != null and not find("SettingsView").is_visible_in_tree(), "undersized client covers Settings")
    root.focus_exited.emit(); await frames()
    root.size = Vector2i(1920,1080); await frames()
    root.focus_entered.emit(); await edge(KEY_F5, false)
    check(root.gui_get_focus_owner() == find("Setting_replace"), "recovery restores the rebuilt editor row after capture abort")
    if root.gui_get_focus_owner() != find("Setting_replace"): return
    check(settings().page == "Operation" and settings().draft.Bindings.is_empty(), "focus loss discards the capture without moving the target")
    await activate("replace"); await key(KEY_F5); await activate("accept")
    await to_root()
    check(await Nav.row(find("SettingsView"), "apply"), "Apply remains reachable with old Confirm")
    await frames(18)
    check(battle_state() == original, "covered battle preserves selection, turn, vitals and both RNG seeds")
    await edge(KEY_ENTER, true)
    if not settings().is_empty():
        check(false, "Apply publishes the candidate: " + settings().message)
        await edge(KEY_ENTER, false); return
    check(mounted().remapRelease != null and selected_menu() == "Settings", "Apply restores the menu and tracks old Confirm release")
    await key(KEY_ESCAPE)
    check(find("SystemMenuView") != null and battle_state() == original, "another action while old Confirm is held cannot leave Menu or commit")
    await edge(KEY_ENTER, false)
    check(mounted().remapRelease == null, "physical release retires the remap barrier")

    var menu_title := find("SystemMenuTitle") as Label
    check(menu_title.text == "游戏菜单", "visible menu refreshes to Chinese")
    var installed: Dictionary = mounted().applied
    check(installed.Language == "zh-CN" and installed.ReducedFlash and installed.GraphicsMode == "hd"
        and installed.TextMode == "adjustable" and installed.CharactersPerSecond == 50
        and not installed.ModernGuidance and installed.ShowDebugDiagnostics, "Apply publishes all edited categories")
    var disk: Variant = JSON.parse_string(FileAccess.get_file_as_string(prefs))
    check(disk != null and disk.language == "zh-CN" and disk.bindings.confirm.keys[0] == "F5",
        "ordinary preference file contains language and remap")

    await edge(KEY_RIGHT, true)
    await back()
    check(find("SystemMenuView") == null and state().candidate == candidate, "Resume preserves the non-default target")
    var target: Dictionary = state().menu.layout
    check(target.heading == "选择目标" and "F5" in target.help, "resumed target shows Chinese and installed Confirm")
    await frames(15); await edge(KEY_RIGHT, false); await key(KEY_ENTER)
    check(battle_state() == original, "held direction and retired Confirm cannot move or commit the target")
    await Nav.tap(self, "confirm")
    for i in range(4):
        if state().scene.waitToken != null: break
        await Nav.tap(self, "confirm")
    for i in range(300):
        if state().scene.phase == "ActionMessage": break
        await frames(1)
    if state().scene.phase != "ActionMessage":
        check(false, "fresh mapped Confirm reaches an actual action message"); return
    if not await open_general(): return
    var reading := battle_state()
    var scene_before: Dictionary = state().scene
    await check_display_layout("zh-CN")
    await go_setting("language"); await to_root(); await go_setting("apply")
    var scene_after: Dictionary = state().scene
    check(battle_state() == reading and scene_after.message != scene_before.message,
        "action message localizes on Apply while its token, phase, elapsed time and gameplay stay fixed")
    await back()
    check(state().scene.waitToken == scene_before.waitToken, "resuming an action message still requires its ordinary input")
    samples.append({"target": original, "localizedHeading": target.heading, "cue": scene_after.phase, "installed": mounted().applied})

func restart() -> void:
    var applied: Dictionary = mounted().applied
    check(applied.Language == "en" and applied.Bindings.confirm.Keys[0] == "F5" and applied.ReducedFlash
        and applied.GraphicsMode == "hd" and applied.TextMode == "adjustable" and applied.CharactersPerSecond == 50
        and not applied.ModernGuidance and applied.ShowDebugDiagnostics, "cold host reloads edited preferences")
    await back()
    await Nav.tap(self, "up")
    await Nav.tap(self, "confirm")
    await Nav.tap(self, "down"); await Nav.tap(self, "down")
    var inspection: Dictionary = state().inspection
    if not await open_general(): return
    var display := find("Setting_display")
    await key(KEY_ENTER)
    check(settings().get("page") == "Root" and root.gui_get_focus_owner() == display,
        "retired Enter cannot reach native ui_accept in Settings")
    await Nav.tap(self, "confirm")
    check(settings().get("page") == "Display", "installed Confirm opens the focused page")
    await back()
    await back(); await back()
    var returned: Dictionary = state().inspection
    check(returned.page == "Options" and returned.optionIndex == 2 and returned.cursor == inspection.cursor,
        "shared Settings preserves the underlying inspection page and cursor")
    await Nav.tap(self, "confirm")
    check(settings().get("battleOnly", false) and settings().rows == ["battle-speed", "messages"],
        "ordinary inspection Options still opens the two-row Battle Speed")
    if settings().is_empty(): return
    var before := bytes(prefs)
    await Nav.tap(self, "right"); await back()
    check(state().inspection.page == "Options" and state().inspection.optionIndex == 2 and bytes(prefs) == before,
        "Battle Speed Cancel preserves its Options row and preferences")
    await Nav.tap(self, "confirm")
    await Nav.tap(self, "right")
    await Nav.tap(self, "confirm")
    check(settings().is_empty() and state().inspection.optionIndex == 2
        and mounted().applied.CharactersPerSecond == 15 and bytes(prefs) != before,
        "Battle Speed Apply saves its preset and returns to the same Options row")

func auto_win() -> void:
    if not await open_general(): return
    var paused := battle_state()
    var seed: Variant = state().mainSeed
    await go_setting("victory")
    check(settings().draft.DebugAutoVictory and "resume" in settings().message, "debug auto-win remains editable and explains when it can act")
    await to_root(); await go_setting("apply")
    await frames(18)
    check(mounted().applied.DebugAutoVictory and battle_state() == paused, "enabling auto-win does not act while Menu owns the pause")
    await back(); await frames(8)
    check(state().stopReason == "BattleEnded" and state().mainSeed == seed, "auto-win acts through the existing legal turn after Resume")

func layout_error() -> void:
    if not await open_general(): return
    await go_setting("language"); await to_root()
    check(await Nav.row(find("SettingsView"), "apply"), "Apply is focused before storage conflict")
    var ids := ["display", "assist", "input", "debug", "apply"]
    var shown := settings_layout(ids)
    var paused := battle_state()
    DirAccess.make_dir_recursive_absolute(prefs.get_base_dir())
    var external := FileAccess.open(prefs, FileAccess.WRITE)
    if external == null: check(false, "disposable external preferences can be written"); return
    external.store_string('{"formatVersion": 1}')
    external.close()
    var before := bytes(prefs)
    await Nav.tap(self, "confirm")
    var message := find("SettingsMessage") as Label
    check("changed outside" in message.text and message.is_visible_in_tree()
        and message.get_theme_font_size("font_size") == 30, "real storage refusal appears in details at normal font size")
    check(settings().focus == "apply" and settings_layout(ids) == shown and mounted().applied.Language == "en"
        and bytes(prefs) == before and battle_state() == paused, "refusal keeps actions, focus, row scroll, preferences and battle")
    await back(); await back()
    check(find("SettingsView") == null and find("SystemMenuView") == null, "Cancel and Resume remain usable after refusal")

func run() -> void:
    var save_namespace := OS.get_environment("SF2_SAVE_NAMESPACE")
    if output.is_empty() or not save_namespace.begins_with("issue2017-"): quit(2); return
    prefs = OS.get_environment("SF2_SAVE_ROOT").path_join(save_namespace).path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    for i in range(300):
        if state().get("stage") == "Movement" and state().get("movement") == null: break
        await frames(1)
    check(state().get("stage") == "Movement" and state().get("failure") == null, "authored Stone Court reaches a real player turn")
    if failures.is_empty():
        match case_name:
            "native-root": await native_root()
            "transaction": await transaction()
            "restart": await restart()
            "auto-win": await auto_win()
            "layout-error": await layout_error()
            _: check(false, "select a supported observation case")
    check(state().get("failure") == null and state().get("scene", {}).get("error") == null, "no battle adapter error")
    root.remove_child(host); host.queue_free(); await frames(2)
    finish()
