extends "res://probes/settings_remap_observation.gd"

# Bounded production Settings transaction; only the dedicated preview case takes images.
func footer() -> void:
    check(settings().footer.has("reset-page"), "category has reset footer")
    check(not settings().rows.has("defaults") and not settings().rows.has("reset"), "no legacy reset list rows")
    check(find("Setting_reset-page").get_parent().name == "SettingsActions", "Reset is outside the scrolling options")

func reset_key() -> void: await Nav.tap(self, "spell")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue2290-"):
        quit(2); return
    prefs = OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920, 1080)
    if case_name == "visual":
        var preview = load("res://probes/previews/DesignPreview.tscn").instantiate()
        root.add_child(preview); await frames(15)
        host = preview.find_child("GameRoot", true, false)
    else:
        host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(15)
    if case_name == "battle":
        for frame in 200:
            if state().get("stage") == "PlayerDecision": break
            await frames(1)
        var before := state()
        await Nav.tap(self, "cancel"); await Nav.tap(self, "up"); await Nav.tap(self, "confirm")
        await Nav.tap(self, "down"); await Nav.tap(self, "down"); await Nav.tap(self, "confirm")
        check(settings().get("battleOnly", false), "ordinary inspection opens Battle Speed")
        if settings().is_empty(): finish(); return
        var applied: Dictionary = settings().applied.duplicate(true)
        var original := bytes(prefs)
        footer(); await activate("reset-page")
        check(not settings().is_empty() and settings().draft.TextMode == "instant" and not settings().draft.SuppressBattleHitCoinMessages, "Confirm on reset footer stages Battle page without applying")
        check(settings().draft.Bindings == applied.Bindings and settings().draft.ModernGuidance == applied.ModernGuidance, "Battle reset preserves other pages")
        await back(); check(bytes(prefs) == original, "Battle Reset Cancel preserves bytes")
        await Nav.tap(self, "confirm"); await reset_key(); await Nav.tap(self, "attack")
        check(settings().is_empty() and state().inspection.page == "Options", "Battle Reset Apply returns to its caller")
        var after := state()
        check(before.sessionId == after.sessionId and before.revision == after.revision and before.mainSeed == after.mainSeed, "Battle reset leaves gameplay unchanged")
        finish(); return
    await open_witch_settings()
    if settings().is_empty(): finish(); return
    var original := bytes(prefs)
    var installed: Dictionary = settings().applied.duplicate(true)
    footer()
    if case_name == "visual":
        for language in ["zh", "en"]:
            root.grab_focus(); await frames(8)
            if language == "en":
                await activate("display"); await activate("language"); await back(); await activate("apply")
                await open_witch_settings()
            await Nav.row(find("SettingsView"), "reset-page"); await frames(8)
            await RenderingServer.frame_post_draw
            root.get_texture().get_image().save_png(output.get_base_dir().path_join(language + "-root.png"))
            root.grab_focus(); await frames(8)
            await activate("input"); await Nav.row(find("SettingsView"), "reset-page"); await pad(JOY_BUTTON_Y)
            await frames(8); await RenderingServer.frame_post_draw
            root.get_texture().get_image().save_png(output.get_base_dir().path_join(language + "-controls-pad.png"))
            sample(language + "-preview"); await back()
        finish(); return
    if case_name == "override":
        var override_path: String = OS.get_cmdline_user_args()[-1]
        var override_bytes := bytes(override_path)
        check(settings().readOnly and find("Setting_reset-page").disabled, "read-only footer disabled")
        await reset_key(); await activate("input"); await reset_key(); await activate("help"); await reset_key()
        check(settings().draft == installed and settings().help, "read-only and help refuse reset")
        await back(); await back(); await back()
        check(bytes(prefs) == original and bytes(override_path) == override_bytes, "override and ordinary files untouched")
        finish(); return
    if case_name == "recovery":
        await activate("input"); await reset_key(); await back(); await activate("apply")
        check(not settings().is_empty() and bytes(prefs) == original, "page reset cannot authorize invalid-file recovery")
        await reset_key(); await activate("apply")
        var backups := Array(DirAccess.get_files_at(prefs.get_base_dir())).filter(func(p): return p.ends_with(".invalid.json"))
        check(settings().is_empty() and backups.size() == 1 and bytes(prefs.get_base_dir().path_join(backups[0])) == original, "all reset Apply recovers with exact invalid-byte backup")
        finish(); return
    if case_name == "restart":
        check(installed.Bindings.is_empty() and installed.ModernGuidance and installed.TextMode == "instant", "cold restart reads reset preferences")
        await back(); finish(); return
    # Page scope, installed hints and keyboard/pad both use the actual remappable Spell action.
    check("Q" in find("Setting_reset-page").text, "footer uses installed replacement Q")
    for page in ["display", "assist", "input", "debug"]:
        await activate(page); footer(); await back()
    await activate("assist")
    await key(KEY_H)
    check(settings().draft == installed, "retired default shortcut does nothing")
    var reset_button := find("Setting_reset-page") as Control
    await mouse(reset_button, true); await mouse(reset_button, false)
    check(settings().draft.ModernGuidance and not settings().draft.ReducedFlash, "page reset restores gameplay aids")
    check(settings().draft.TextMode == installed.TextMode and settings().draft.Bindings == installed.Bindings, "page reset preserves unrelated draft")
    check(bytes(prefs) == original and settings().applied == installed, "reset is staged only")
    await back(); await activate("input")
    await activate("help"); var before: Dictionary = settings().draft.duplicate(true)
    await reset_key(); check(settings().help and settings().draft == before, "help owns reset shortcut")
    await back(); await activate("edit"); await activate("action-accelerate"); await activate("keyboard")
    await activate("slot-0"); await activate("replace")
    await key(KEY_Q)
    check(settings().page == "Capture" and settings().draft == before, "capture handles reset key as conflicting binding, never resets")
    await key(KEY_F6)
    check(settings().page == "Review", "fresh non-conflicting key reaches review")
    await reset_key(); check(settings().page == "Review" and settings().draft == before, "review owns its layer")
    await back(); await to_root()
    await reset_key()
    check(settings().reset and settings().draft.Bindings.is_empty() and settings().draft.TextMode == "instant", "root reset includes preferences and bindings")
    check("Q" in find("Setting_reset-page").text, "staged reset retains installed hint")
    await back(); check(bytes(prefs) == original, "Reset then Cancel preserves exact bytes")
    await open_witch_settings(); check(settings().draft == installed and not settings().reset, "return has no repeated activation")
    await activate("assist"); await reset_key(); await back(); await activate("apply")
    var saved: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(prefs))
    check(saved.modernGuidance and saved.textMode == "adjustable" and saved.bindings == JSON.parse_string(original.get_string_from_utf8()).bindings, "page Reset Apply persists only its scope")
    await open_witch_settings(); await activate("input")
    for value in [-1.0, 0.0]:
        var event := InputEventJoypadMotion.new(); event.device = 0; event.axis = JOY_AXIS_RIGHT_Y; event.axis_value = value
        Input.parse_input_event(event); await frames()
    check(settings().draft.Bindings.is_empty(), "remapped pad reset clears Controls draft")
    check("Q" in settings().bindings.Spell and find("Setting_reset-page").text != "", "pad hint remains installed before Apply")
    sample("pad-controls-reset")
    await back(); await reset_key(); await activate("apply")
    saved = JSON.parse_string(FileAccess.get_file_as_string(prefs))
    check(saved.bindings.is_empty() and saved.textMode == "instant" and saved.charactersPerSecond == 40, "all Reset Apply persists defaults")
    await open_witch_settings(); await activate("assist"); await activate("guidance")
    await pad(JOY_BUTTON_Y)
    check(settings().draft.ModernGuidance, "default North button resets page after binding reset")
    await back(); await back()
    sample("completed"); finish()
