extends "res://probes/settings_remap_observation.gd"

# Production players into Master, with short peak-only samples. No audio files or screenshots.
var capture := AudioEffectCapture.new()
var driver := AudioServer.get_driver_name()

func audio() -> Dictionary:
    var entry := find("SessionEntryView")
    var active = observation(entry, "ReadEntryObservationJson").get("audio")
    if active == null: active = observation(host, "ReadOpeningObservationJson").get("audio")
    if active == null: active = JSON.parse_string(host.call("ReadAudioObservationJson"))
    return active if active is Dictionary else {}

func peak_for(label: String, seconds: float, audible: bool) -> void:
    var length := 0
    var peak := 0.0
    var started := Time.get_ticks_msec()
    while Time.get_ticks_msec() - started < seconds * 1000:
        await process_frame
        var count := capture.get_frames_available()
        for frame in capture.get_buffer(count) if count > 0 else PackedVector2Array():
            peak = maxf(peak, maxf(absf(frame.x), absf(frame.y)))
        length += count
    var playback := audio()
    samples.append({"label":label, "driver":driver, "peak":peak, "frames":length,
        "cue":playback.get("musicCue"), "musicPosition":playback.get("musicPosition"),
        "musicFinished":playback.get("musicFinished"), "voices":playback.get("sounds")})
    check(length > 0 and (peak > 0.0001 if audible else peak < 0.000001), label)

func entry_ready() -> bool:
    for i in range(360):
        if audio().get("musicPlaying", false): return true
        await process_frame
    check(false, "entry audio has started")
    return false

func entry_flow(restart: bool) -> void:
    if not await entry_ready(): return
    if observation(host, "ReadOpeningObservationJson").active:
        capture.clear_buffer()
        await peak_for("opening Master", 0.45, not restart)
        await key(KEY_E)
        if not await entry_ready(): return
    var playback := audio()
    check(playback.get("musicCue") == "MUSIC_TITLE" and playback.get("musicBus") == "Music", "title music routed")
    capture.clear_buffer()
    await peak_for("title Master", 0.45, not restart)
    if restart:
        var installed: Dictionary = mounted().applied
        check(installed.MusicVolume == 0 and installed.SystemEffectsVolume == 100
            and installed.BattleEffectsVolume == 100 and installed.Language == "zh-CN", "cold ordinary preference load")
        var deadline := Time.get_ticks_msec() + 13000
        while not audio().get("musicFinished", false) and Time.get_ticks_msec() < deadline:
            await process_frame
        check(audio().get("receipts", []).filter(func(row): return row.Operation == "finished").size() == 1,
            "muted finite title Finished exactly once")
    await open_witch_settings()
    if settings().is_empty(): return
    var original := bytes(prefs)
    var installed: Dictionary = settings().applied
    check(installed.SystemEffectsVolume == 100 and installed.BattleEffectsVolume == 100, "missing effects fields use unity")
    await activate("audio")
    check(settings().rows == ["music-volume", "system-effects-volume", "battle-effects-volume", "reset-page"],
        "only three Audio controls")
    check(settings().labels[0].begins_with("音乐" if restart else "Music"), "localized Audio page")
    sample("audio-page")
    if restart:
        await Nav.tap(self, "spell")
        check(settings().draft.MusicVolume == 100 and settings().applied.MusicVolume == 0, "Audio reset is staged")
        await back()
        await activate("apply")
        check(mounted().applied.MusicVolume == 100 and mounted().applied.Language == "zh-CN", "Audio reset applies and keeps locale")
        return
    check(installed.MusicVolume == 100, "missing music field uses unity")
    await Nav.tap(self, "left")
    check(settings().draft.MusicVolume == 90, "mapped keyboard decrements")
    await pad(JOY_BUTTON_DPAD_RIGHT)
    check(settings().draft.MusicVolume == 100, "mapped pad increments")
    await Nav.tap(self, "confirm")
    check(settings().draft.MusicVolume == 0 and settings().applied.MusicVolume == 100, "100 cycles to mute in draft")
    await Nav.tap(self, "spell")
    check(settings().draft == installed, "page reset restores defaults")
    await Nav.tap(self, "confirm")
    await back(); await back()
    check(bytes(prefs) == original and mounted().applied.MusicVolume == 100, "Cancel retains mix and exact preferences")
    await open_witch_settings()
    await activate("audio")
    var music := find("Setting_music-volume") as Control
    await mouse(music, true); await mouse(music, false)
    check(settings().draft.MusicVolume == 0, "completed mouse click cycles music")
    await back()
    await activate("display"); await activate("language"); await back()
    await activate("apply")
    check(mounted().applied.MusicVolume == 0 and mounted().applied.Language == "zh-CN", "Apply installs ordinary Audio and language preferences")
    await frames(8)
    capture.clear_buffer()
    await peak_for("Witch Master after Apply", 0.45, false)
    check(audio().get("musicCue") == "MUSIC_WITCH" and audio().get("musicPlaying", false), "Witch music still playing")

func adjust_field_music(action: String, expected: int) -> void:
    var old := audio()
    await field_settings()
    await activate("audio"); await Nav.tap(self, action); await back(); await activate("apply"); await back()
    await frames(8)
    var now := audio()
    check(mounted().applied.MusicVolume == expected
        and now.get("musicCue") == old.get("musicCue")
        and now.get("musicPosition", 0) >= old.get("musicPosition", 0)
        and now.get("receipts", []).filter(func(row): return row.Sequence > old.sequence and row.Command < 65).is_empty(),
        "field music gain change preserves cue and advancing playback")

func field_flow(audible: bool) -> void:
    for i in range(600):
        if state().get("canWaitAtInput", false): break
        await process_frame
    if not state().get("canWaitAtInput", false):
        check(false, "church seed admitted"); return
    if not audible:
        await field_settings()
        await activate("audio")
        await activate("system-effects-volume"); await back(); await activate("apply"); await back()
        check(mounted().applied.SystemEffectsVolume == 0 and mounted().applied.BattleEffectsVolume == 100,
            "ordinary field Apply mutes only system effects")
        await frames(8)
    capture.clear_buffer()
    await peak_for("field loop before interaction", 0.35, false)
    if audible:
        await adjust_field_music("right", 10)
        capture.clear_buffer()
        await peak_for("field loop at 10%", 0.35, true)
        await adjust_field_music("left", 0)
        capture.clear_buffer()
    var before := audio()
    await Nav.tap(self, "confirm")
    await peak_for("church dialogue Master", 0.9, audible)
    var after := audio()
    check(after.get("sequence", 0) > before.get("sequence", 0)
        and after.get("receipts", []).any(func(row): return row.Sequence > before.sequence and row.Operation == "started" and row.Command >= 65),
        "ordinary field interaction delivered effects")

func battle_mix() -> void:
    for i in range(720):
        if state().get("stage") == "Movement": break
        await process_frame
    if state().get("stage") != "Movement":
        check(false, "BLAZE seed admitted"); return
    await field_settings()
    await activate("audio")
    check(settings().get("page") == "Audio" and settings().get("battleOnly") == false, "battle opens shared Audio page")
    if case_name == "battle-menu":
        await activate("battle-effects-volume"); await back(); await activate("apply"); await back()
        check(mounted().applied.BattleEffectsVolume == 0 and mounted().applied.SystemEffectsVolume == 100,
            "ordinary battle Apply leaves system effects enabled")
        await frames(8)
    else:
        await back(); await back(); await back()
    var old := audio()
    capture.clear_buffer()
    # The companion uses only ordinary debug-port battle commands from the saved turn.
    var marker := FileAccess.open(output + ".ready", FileAccess.WRITE)
    marker.close()
    await peak_for("battle sequence Master", 12.0, case_name != "battle-muted")
    var now := audio()
    var receipts: Array = now.get("receipts", []).filter(func(row): return row.Sequence > old.sequence)
    var commands := receipts.filter(func(row): return row.Operation == "started").map(func(row): return row.Command)
    var fade_commands := receipts.filter(func(row): return row.Operation == "fade-command").size()
    samples.append({"label":"battle playback", "commands":commands, "fadeCommands":fade_commands})
    check(fade_commands >= 2 and now.musicCue == old.musicCue and now.musicPlaying,
        "muted battle music still fades into the action and returns")
    check(commands.has(77.0) and commands.has(83.0),
        "ordinary action delivered a cast or hit")
    check(commands.has(65.0),
        "ordinary battle command list delivered a menu effect")
    check(not now.is_empty() and now.error == null and state().get("failure") == null, "battle and audio have no error")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue2361-"):
        quit(2); return
    check(driver == "WASAPI", "native WASAPI driver")
    root.size = Vector2i(1920, 1080)
    prefs = OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    var source := OS.get_environment("SF2_AUDIO_VOLUME_SEED")
    var before := bytes(source)
    capture.buffer_length = 0.2
    AudioServer.add_bus_effect(0, capture)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    await frames(10)
    match case_name:
        "entry", "restart": await entry_flow(case_name == "restart")
        "field", "field-muted": await field_flow(case_name == "field")
        "battle", "battle-muted", "battle-menu": await battle_mix()
        _: check(false, "select a named case")
    check(source.is_empty() or bytes(source) == before, "seed exact bytes retained")
    finish()
