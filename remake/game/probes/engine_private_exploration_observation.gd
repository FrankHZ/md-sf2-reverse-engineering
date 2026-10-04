extends SceneTree

var samples: Array = []
var failures: Array = []
var host: Node
var view: Node
var case_name := OS.get_environment("SF2_PRIVATE_EXPLORATION_CASE")
var door_events: Array = []
var fade_events: Array = []
var layout_plan: Dictionary

func _same_words(actual: Array, expected: Array) -> bool:
    if actual.size() != expected.size(): return false
    for index in range(actual.size()):
        # JSON numbers and direct CLR dictionaries use different Variant numeric types.
        if float(actual[index]) != float(expected[index]): return false
    return true

func _layout_read(label: String, rect: Array, expected: Array = [], require_draw := true) -> Dictionary:
    var request: int = view.call("ObserveLayoutRegion", rect[0], rect[1], rect[2], rect[3])
    var state: Dictionary = {}
    for tick in range(12):
        view.queue_redraw()
        await process_frame
        state = view.call("ReadLayoutRegion", rect[0], rect[1], rect[2], rect[3])
        if state.draw != null and state.draw.request == request and state.draw.revision == state.revision: break
    samples.append({"label":label,"layout":state})
    _check(state.draw != null, label + ": actual draw is available")
    if state.draw == null: return state
    var draw: Dictionary = state.draw
    _check(draw.request == request and draw.sessionId == state.sessionId and draw.map == state.map and draw.revision == state.revision and draw.observationSequence == state.observationSequence,
        label + ": draw matches request and exact live snapshot")
    _check(not draw.overflow, label + ": bounded draw did not overflow")
    if not expected.is_empty(): _check(_same_words(state.words, expected), label + ": independent source operation matches working words")
    var basis: Array = expected if not expected.is_empty() else state.words
    for use in draw.uses:
        var index := int(use.sourceY - rect[1]) * int(rect[2]) + int(use.sourceX - rect[0])
        _check(index >= 0 and index < basis.size(), label + ": used cell is in requested rectangle")
        if index < 0 or index >= basis.size(): continue
        var block := int(basis[index]) & 1023
        _check(use.block == block and use.selector.block == block and use.selector.map == state.map and use.resourceIdentity != "", label + ": actual texture uses independently selected block")
        var words: Array = layout_plan.blocks[block] if use.tile == null else [layout_plan.blocks[block][int(use.tile)]]
        _check(_same_words(use.words, words) and use.width > 0 and use.height > 0 and use.textureWidth > 0 and use.textureHeight > 0, label + ": drawn tile words and clipped region")
    _check(not require_draw or not draw.uses.is_empty(), label + ": visible changed region has actual draw use")
    return state

func _layout_stable(label: String, rect: Array, before: Dictionary, expected: Array, require_draw := true) -> Dictionary:
    var after := await _layout_read(label, rect, expected, require_draw)
    _check(after.sessionId == before.sessionId and after.revision == before.revision and after.observationSequence == before.observationSequence and after.words == before.words,
        label + ": repeat read and redraw do not mutate state")
    if after.draw == null or before.draw == null: return after
    _check(after.draw.request != before.draw.request and after.draw.drawSequence > before.draw.drawSequence, label + ": fresh draw cannot reuse old request")
    return after

func _layout_case() -> void:
    layout_plan = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_LAYOUT_OPERANDS")))
    var door_rect := [4, 8, 1, 1]
    var roof_rect := [2, 32, 7, 8]
    var flag_rect := [28, 22, 1, 2]
    var flag_pre: Array = []
    var flag_source: Array = []
    # Each host reads a declared startup file through the real GameRoot. No live state setter.
    var live_start := OS.get_environment("SF2_LAYOUT_CONTROLLED_START")
    _check(OS.get_cmdline_user_args().has(live_start), "controlled input is the actual startup argument")
    var selected := OS.get_environment("SF2_LAYOUT_CASES").split(",", false)
    for index in range(layout_plan.starts.size()):
        if not selected.is_empty() and not str(index) in selected: continue
        var started := Time.get_ticks_msec()
        var previous_bytes := JSON.stringify(samples).to_utf8_buffer().size()
        var file := FileAccess.open(live_start, FileAccess.WRITE)
        file.store_string(FileAccess.get_file_as_string(layout_plan.starts[index]))
        file.close()
        host = (load("res://Main.tscn") as PackedScene).instantiate()
        root.add_child(host)
        await process_frame
        var initial := await _door_settle()
        samples.append({"label":"layout-start-" + str(index), "state":initial})
        _check(initial.map == "map-3" and initial.failure == null, "local layout controlled start succeeds")
        if not _save(false): return
        if initial.failure != null or not view.has_method("ObserveLayoutRegion"):
            host.queue_free()
            await process_frame
            break
        if index == 0:
            var source: Dictionary = view.call("ReadLayoutRegion", 62, 0, 1, 1)
            await _layout_read("door-before", door_rect)
            var hidden: Dictionary = view.call("ReadLayoutRegion", 2, 32, 7, 8)
            _check(hidden.roof != null, "inside-house controlled load retains saved roof")
            var restored: Array = layout_plan.roofBaseWords
            _check(_same_words(hidden.roof.saved.map(func(cell): return cell.word), restored), "saved roof matches independent source pre-load words")
            var clear: Array = []; clear.resize(56); clear.fill(0)
            _check(hidden.words == clear, "source roof-on-load clear retains exact saved pre-state")
            await _press(KEY_DOWN)
            await _door_settle()
            var opened := await _layout_read("door-opened", door_rect, source.words)
            await _layout_stable("door-repeat-read", door_rect, opened, source.words)
            await _press(KEY_DOWN)
            await _door_settle()
            var roof := await _layout_read("roof-restored", roof_rect, restored)
            await _layout_stable("roof-repeat-read", roof_rect, roof, restored)
            await _press(KEY_UP)
            await _door_settle()
            var cleared := await _layout_read("roof-cleared", roof_rect, clear, false)
            if cleared.draw == null:
                _finish()
                return
            _check(cleared.draw.uses.is_empty(), "cleared overlay cells issue no texture draw")
            var plane: Dictionary = cleared.draw.foreground
            var left := floori(plane.x / 24) + int(plane.offsetX)
            var top := floori(plane.y / 24) + int(plane.offsetY)
            _check(plane.enabled and left < 9 and left + 14 > 2 and top < 40 and top + 9 > 32,
                "cleared roof still intersects the actual foreground viewport")
            _check(cleared.roof.saved == hidden.roof.saved, "roof reactivation saves the restored words")
            await _layout_stable("roof-clear-repeat", roof_rect, cleared, clear, false)
            await _layout_read("door-revisited", door_rect, source.words)
            await _press(KEY_DOWN)
            await _door_settle()
            await _layout_read("roof-restored-again", roof_rect, restored)
        elif index == 1:
            var before := await _layout_read("flag-off-load", flag_rect)
            flag_pre = before.words
            var source: Dictionary = view.call("ReadLayoutRegion", 23, 23, 1, 2)
            flag_source = source.words
            await _layout_stable("flag-off-repeat", flag_rect, before, flag_pre)
        else:
            _check(flag_pre != flag_source, "flag copy changes its destination pre-state")
            var after := await _layout_read("flag-on-load", flag_rect, flag_source)
            _check(506.0 in after.flags, "flag506 selected at the controlled load")
            await _layout_stable("flag-on-repeat", flag_rect, after, flag_source)
        _check(Time.get_ticks_msec() - started < 90000, "each local case stays within 90 seconds")
        _check(JSON.stringify(samples).to_utf8_buffer().size() - previous_bytes <= 10 * 1024 * 1024, "case output fits 10MiB")
        if not _save(false): return
        view.call("StopLayoutObservation")
        host.queue_free()
        await process_frame
        await process_frame
        if not failures.is_empty(): break
    _finish()

func _fade_result(payload: String) -> void:
    fade_events.append_array(JSON.parse_string(payload).observations)

func _fade_case() -> void:
    view.connect("SessionResultObserved", _fade_result)
    var initial := _read("authored-sound-fade-pending")
    var token = initial.token
    _check(initial.wait == "PresentationWait" and not 7.0 in initial.flags, "authored SoundFade owns a pending token")
    await _press(KEY_ENTER)
    var early := _read("early-input")
    _check(early.token == token and early.wait == "PresentationWait" and not 7.0 in early.flags, "early input cannot complete the fade")
    var gained := false
    for tick in range(120):
        var state := _state()
        _check(state.failure == null and state.audio.error == null, "fade consumer remains available")
        if state.audio.fading and state.audio.musicVolumeDb < 0: gained = true
        if state.stop == "PlayerInput": break
        view.queue_redraw()
        await process_frame
    var completed := _read("authored-sound-fade-complete")
    _check(gained and not completed.audio.musicPlaying and not completed.audio.fading, "actual music gain and stop precede service completion")
    _check(completed.stop == "PlayerInput" and 7.0 in completed.flags, "authored continuation runs after completion")
    _check(completed.mainSeed == initial.mainSeed, "authored fade consumes no RNG")
    _check(completed.simulationTick - initial.simulationTick == fade_events.filter(func(event): return event.Kind == "simulation-tick").size(), "existing host ticks remain explicitly observed")
    _check(fade_events.filter(func(event): return event.Kind == "presentation-completed").size() == 1, "one completion for the token")
    for tick in range(8):
        view.queue_redraw()
        await process_frame
    var repeated := _read("fade-repeated-projection")
    _check(repeated.revision == completed.revision and repeated.audio.receipts.filter(func(receipt): return receipt.Command == 253).size() == 1, "projection cannot restart or replay253")
    samples.append({"label":"fade-events", "events":fade_events})
    view.disconnect("SessionResultObserved", _fade_result)

func _door_result(payload: String) -> void:
    var result: Dictionary = JSON.parse_string(payload)
    door_events.append_array(result.observations)

func _door_settle() -> Dictionary:
    for tick in range(600):
        var state := _state()
        if state.failure != null:
            _check(false, "ordinary door consumer has no host failure")
            return state
        var player := _entity(state, "entity-0")
        if state.stop == "PlayerInput" and not player.busy and not player.moving: return state
        if state.wait == "DialogueWait": await _press(KEY_ENTER)
        else: await process_frame
    _check(false, "ordinary door input returns within bounded observation")
    return _state()

func _door_case() -> void:
    view.connect("SessionResultObserved", _door_result)
    var initial := await _door_settle()
    var player := _entity(initial, "entity-0")
    _check(initial.map == "map-3" and player.x == 4 * 384 and player.y == 7 * 384, "controlled approach to existing Map3 door")
    await _press(KEY_DOWN)
    var opened := _read("door-input")
    var starts: Array = opened.audio.receipts.filter(func(receipt): return receipt.Command == 92 and receipt.Operation == "started")
    _check(starts.size() == 1, "ordinary door starts exactly one 92")
    if not starts.is_empty():
        _check(starts[0].TimerB == 203 and starts[0].Playing, "92 actually starts in inherited CB context")
    _check(opened.audio.sounds.any(func(sound): return sound.command == 92 and sound.playing), "92 has an actual playing voice")
    var settled := await _door_settle()
    player = _entity(settled, "entity-0")
    _check(player.x == 4 * 384 and player.y == 8 * 384 and settled.canWaitAtInput, "post-copy traversal reaches door cell and releases input")
    var stable_revision = settled.revision
    var stable_seed = settled.mainSeed
    for tick in range(120):
        view.queue_redraw()
        var projected := _state()
        _check(projected.revision == stable_revision and projected.mainSeed == stable_seed, "repeated projection and audio delivery add no gameplay work")
        if projected.audio.sounds.is_empty(): break
        await process_frame
    await _press(KEY_UP)
    await _door_settle()
    await _press(KEY_DOWN)
    var repeated := await _door_settle()
    _read("revisited-open-door")
    _check(door_events.filter(func(event): return event.Kind == "door-opened").size() == 1, "already opened door emits no duplicate event")
    _check(repeated.audio.receipts.filter(func(receipt): return receipt.Command == 92 and receipt.Operation == "started").size() == 1, "repeated projection and traversal do not replay 92")
    _check(repeated.audio.receipts.any(func(receipt): return receipt.Command == 92 and receipt.Operation == "finished"), "door voice reaches actual Finished")
    _check(repeated.failure == null and repeated.audio.error == null and repeated.canWaitAtInput, "ordinary input remains available")
    samples.append({"label":"door-events", "events":door_events})
    view.disconnect("SessionResultObserved", _door_result)

func _initialize() -> void:
    call_deferred("_run")

func _check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func _state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        view = host.get_node_or_null("BattleSessionView")
    return JSON.parse_string(view.call("ReadObservationJson"))

func _read(label: String) -> Dictionary:
    var state := _state()
    samples.append({"label": label, "state": state})
    return state

func _entity(state: Dictionary, identity: String) -> Dictionary:
    for entity in state.entities:
        if entity.id == identity:
            return entity
    return {}

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _run() -> void:
    if case_name == "map-layout":
        await _layout_case()
        return
    if case_name == "sound-fade":
        # An explicitly supplied authored host reuses admitted PCM; no natural-route claim.
        host = load(OS.get_environment("SF2_FADE_FIXTURE_HOST")).new()
    else:
        host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var initial := _read("controlled-source-start")
    if not initial.has("mode"):
        _check(false, "Private ordinary exploration startup succeeds")
        _finish()
        return
    var identity: String = initial.sessionId
    if case_name == "sound-fade":
        await _fade_case()
        _finish()
        return
    if case_name == "map3-door":
        await _door_case()
        _finish()
        return
    if case_name == "authored-presentation":
        await _press(KEY_ENTER)
        var pending := _read("unavailable-presentation")
        _check(pending.failureKind == "AdapterError" and pending.wait == "PresentationWait", "Unavailable rendering reports an adapter failure with the original wait pending")
        await _press(KEY_ENTER)
        var retained := _read("acknowledgement-not-fabricated")
        _check(retained.revision == pending.revision and retained.token == pending.token and retained.flags == [7.0], "Actual Enter cannot acknowledge an unperformed presentation or publish its later flag")
        _finish()
        return
    if case_name == "map21-guard":
        var dialogue_count := 0
        for _frame in range(2200):
            var state := _state()
            if state.stop in ["PlayerInput", "Unsupported", "Faulted"]:
                break
            if state.wait == "DialogueWait":
                dialogue_count += 1
                _read("source-dialogue-request")
                await _press(KEY_ENTER)
            else:
                await process_frame
        _check(dialogue_count == 0, "Expected source dialogue requests were acknowledged by real input")
    var terminal := _read("terminal")
    _check(terminal.sessionId == identity, "The same common session owns every transition")
    match case_name:
        "map21-guard":
            _check(terminal.failure == null and terminal.stop == "PlayerInput" and 401.0 in terminal.flags, "Source guard program resumes after the physical player sprite service")
            _check(not 256.0 in terminal.flags, "Direct program start has no event caller to write F256")
            _check(_entity(terminal, "entity-128").x == 6 * 384 and _entity(terminal, "entity-0").facing == 3, "Guard moves and unassigned135 faces the physical player")
        _:
            _check(false, "Unknown external reference case")
    _finish()

func _save(complete: bool) -> bool:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file == null:
        push_error("External observer requires a writable output path")
        quit(2)
        return false
    file.store_string(JSON.stringify({"complete": complete, "passed": complete and failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    file.close()
    return true

func _finish() -> void:
    if not _save(true): return
    quit(0 if failures.is_empty() else 1)
