extends "res://probes/engine_exploration_observation.gd"

# Ordinary startup and mapped keyboard events only. Bounded state samples; no rendered captures.
var began := Time.get_ticks_msec()
var identity := ""
var counts: Dictionary = {}
var last_sequence := -1
var maximum_operations := 0
var skip_frames := 0
var mounts := 0
var final_field: Dictionary = {}
var audio_case := OS.get_environment("SF2_ENTRY_CASE") == "audio"

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        counts[event.Kind] = int(counts.get(event.Kind, 0)) + 1
        last_sequence = int(event.Sequence)

func state() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    view = field if field != null and not field.is_queued_for_deletion() else host.find_child("BattleSessionView", true, false)
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, current: Dictionary) -> void:
    var row := {"label":label, "owner":str(view.name), "processFrame":Engine.get_process_frames()}
    for key in ["sessionId", "revision", "mode", "round", "actor", "stage", "stop", "stopReason", "wait", "token", "continuation", "failure", "help", "helpVisible", "simulationTick", "cursor", "entrySkip", "battleHandoffPending", "skipRequiresRelease", "textWindow", "portraitWindow", "logicalText", "logicalView", "display", "presentation", "battleMounted", "fieldText", "music"]:
        if current.has(key): row[key] = current[key]
    samples.append(row)

func healthy(current: Dictionary) -> bool:
    if current.get("failure") != null:
        sample("failure", current)
        _check(false, "session/presenter failure: " + str(current.failure))
        return false
    if Time.get_ticks_msec() - began > 55000:
        sample("deadline", current)
        _check(false, "55-second native budget")
        return false
    _check(current.sessionId == identity, "same session")
    return true

func key_event(key: Key, pressed: bool, echo := false) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = pressed
    event.echo = echo
    Input.parse_input_event(event)

func field_input() -> Dictionary:
    for frame in range(1800):
        var current := state()
        if not healthy(current): return {}
        if current.get("canWaitAtInput", false) or current.get("enteringBattle") != null: return current
        await process_frame
    _check(false, "bounded field navigation")
    return {}

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := state()
    identity = initial.sessionId
    sample("start", initial)
    if not OS.get_environment("SF2_ENTRY_ROUTE").is_empty():
        var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_ENTRY_ROUTE")))
        var plan: Array = fixture.static.inputPlan
        _check(plan.size() <= 64, "bounded pre-start declared route")
        for step in plan:
            var before := await field_input()
            if before.is_empty(): _finish(); return
            if before.get("enteringBattle") != null: break
            await _press(KEY_UP if step.input == "Up" else KEY_RIGHT if step.input == "Right" else KEY_DOWN if step.input == "Down" else KEY_LEFT)
    var entry := state()
    for frame in range(1800):
        if not healthy(entry): _finish(); return
        if "Skip battle intro" in str(entry.get("help", "")): break
        await process_frame
        entry = state()
    sample("eligible-hint", entry)
    _check(entry.get("helpVisible", false) and "Skip battle intro" in str(entry.get("help", "")), "contextual skip hint mounted")
    var cancel_key: Key = KEY_NONE
    for binding in InputMap.action_get_events("sf2_cancel"):
        if binding is InputEventKey: cancel_key = binding.keycode; break
    _check(cancel_key != KEY_NONE and OS.get_keycode_string(cancel_key) in entry.help, "advertised actual mapped key")
    key_event(cancel_key, true)
    await process_frame
    await process_frame
    sample("requested", state())
    if audio_case:
        for frame in range(120):
            var choice := state()
            if not healthy(choice): _finish(); return
            if choice.get("wait") == "ChoiceWait": break
            await process_frame
        var choice := state()
        var quiet: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
        sample("finite-consumed-choice", choice)
        _check(choice.get("wait") == "ChoiceWait" and choice.get("entrySkip") == null, "choice disarms skip")
        _check(choice.presentation.whiteOpacity == 1.0, "unstarted skipped flash preserves inherited opaque white")
        _check(quiet.musicCue == "join" and not quiet.musicPlaying, "omitted finite SoundWait remains silent at choice")
        _check(quiet.receipts.any(func(row): return row.Operation == "battle-entry-skip-consumed"), "finite silence has explicit skipped disposition")
        key_event(cancel_key, true, true)
        await process_frame
        _check(state().wait == "ChoiceWait", "held repeated Cancel cannot select choice")
        key_event(cancel_key, false)
        await process_frame
        await process_frame
        await _press(KEY_ENTER)
        if state().get("wait") == "ChoiceWait": await _press(KEY_ENTER)
        _check(state().get("wait") != "ChoiceWait", "ordinary confirm reveals then answers choice")
        _check(state().get("entrySkip") == null, "choice answer does not restart acceleration")
        for frame in range(30):
            if state().get("wait") == "TickWait": break
            await process_frame
        var repeated := state()
        sample("repeated-finite-wait", repeated)
        var consumed: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
        _check(repeated.get("wait") == "TickWait" and repeated.get("entrySkip") == null, "ordinary repeated finite wait consumes prior skipped disposition")
        _check(consumed.musicDeliverySkipped and not consumed.musicFinished and not consumed.musicPlaying, "skipped readiness is distinct from natural Finished")
        key_event(cancel_key, true)
        await process_frame
    # Keep initiating key held through the entire entry; repeats cannot cross the release barrier.
    for frame in range(3600):
        var current := state()
        if not healthy(current): _finish(); return
        if view.name != "ExplorationSessionView":
            sample("unexpected-handoff", current)
            _check(false, "held skip cannot hand input to combat")
            _finish()
            return
        if current.get("entrySkip") != null: maximum_operations = maxi(maximum_operations, int(current.entrySkip.Operations))
        mounts = maxi(mounts, int(current.get("presentation", {}).get("battleLoads", 0)))
        skip_frames += 1
        if current.get("battleHandoffPending", false):
            final_field = current
            break
        if frame == 5: key_event(cancel_key, true, true)
        await process_frame
    sample("entry-exit-held", state())
    _check(not final_field.is_empty(), "entry ends before held-input handoff")
    _check(final_field.get("entrySkip") == null, "skip cleared before combat")
    _check(final_field.get("presentation", {}).get("paletteBrightness") == 1.0, "final black modulation restored")
    _check(final_field.get("presentation", {}).get("whiteOpacity") == 0.0, "final white overlay restored")
    key_event(cancel_key, false)
    await process_frame
    await process_frame
    var battle := state()
    sample("battle", battle)
    _check(view.name == "BattleSessionView" and battle.get("stage") == "Movement", "ordinary usable first battle control")
    _check(counts.get("battle-entry-skip-requested", 0) == (2 if audio_case else 1), "one request despite repeat")
    await _press(KEY_ENTER)
    _check(state().get("stage") == "ActionChoice", "Confirm opens ordinary battle menu")
    await _press(cancel_key)
    _check(state().get("stage") == "Movement", "fresh Cancel has ordinary combat meaning")
    sample("usable-combat", state())
    var final_audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    if not OS.get_environment("SF2_ENTRY_ROUTE").is_empty():
        _check(final_audio.error == null and final_audio.musicCue == "MUSIC_BATTLE_THEME_1" and final_audio.musicPlaying and final_audio.musicVolumeDb == 0, "final Battle01 authoritative music plays at restored volume")
        _check(mounts == 1, "one actual connected BattleLoad mount")
    if audio_case:
        _check(final_audio.error == null and final_audio.musicCue == "town" and final_audio.musicPlaying, "PreviousMusic restores retained suppressed history")
    _finish()

func _finish() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson")) if host != null else {}
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures, "samples":samples,
        "counts":counts, "maximumOperations":maximum_operations, "skipFrames":skip_frames,
        "battleLoads":mounts, "elapsedMs":Time.get_ticks_msec()-began, "audio":audio}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
