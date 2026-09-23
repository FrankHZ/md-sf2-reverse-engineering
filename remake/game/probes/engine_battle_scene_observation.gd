extends SceneTree

# Real host input and read-only scene/node observations. Persist new events and
# changed projections only; no frame-by-frame whole-world JSON or screenshots.
var host: Node
var output: FileAccess
var view: Node
var failures: Array = []
var events: Array = []
var projections: Array = []
var receipts: Array = []
var last_event := 0
var last_receipt := 0
var last_projection := ""
var started := Time.get_ticks_msec()
var scenes := 0
var in_scene := false
var start_cursor := -1
var initial_seed := 0
var input_checked := false
var exploration: Node
var world_boundaries: Array = []
var last_world := ""

func _initialize() -> void:
    call_deferred("_run")

func _check(value: bool, message: String) -> void:
    if not value:
        failures.append(message)
        push_error(message)

func _result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if event.Sequence <= last_event:
            continue
        _check(last_event == 0 or event.Sequence == last_event + 1, "session event sequence has no gap")
        last_event = int(event.Sequence)
        events.append(event)

func _read() -> Dictionary:
    var state: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
    var scene: Dictionary = state.scene
    var key := str([state.revision, scene.frameIndex, scene.reactionState, scene.visibleCharacters, scene.backgroundX, scene.allyX, scene.enemyX])
    if key != last_projection:
        last_projection = key
        projections.append(state)
    _audio()
    return state

func _audio() -> void:
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    if audio != null:
        for receipt in audio.receipts:
            if receipt.Sequence <= last_receipt:
                continue
            _check(last_receipt == 0 or receipt.Sequence == last_receipt + 1, "audio receipt sequence has no gap")
            last_receipt = int(receipt.Sequence)
            receipts.append(receipt)
        _check(audio.error == null, "audio remains available")

func _world_settle() -> Dictionary:
    for tick in range(6000):
        if not is_instance_valid(exploration):
            return {}
        var state: Dictionary = JSON.parse_string(exploration.call("ReadObservationJson"))
        _audio()
        var key := str([state.map, state.wait, state.token, state.stop, state.failure])
        if key != last_world:
            last_world = key
            world_boundaries.append({"map":state.map,"wait":state.wait,"token":state.token,"stop":state.stop,"failure":state.failure})
        if state.failure != null:
            _check(false, "world admission: " + str(state.failure))
            return state
        if state.wait in ["DialogueWait", "ChoiceWait"]:
            await _press(KEY_ENTER)
        elif state.stop == "PlayerInput" and state.mode == "Exploration":
            var players: Array = state.entities.filter(func(entity): return entity.id == "entity-0")
            if not players.is_empty() and not players[0].moving:
                return state
            await process_frame
        else:
            await process_frame
    _check(false, "world admission bounded wait")
    return {}

func _admit_world() -> void:
    exploration = host.get_node_or_null("ExplorationSessionView")
    if exploration == null: return
    exploration.connect("SessionResultObserved", _result)
    await _world_settle()
    var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../../tests/fixtures/h2/map3-battle01-admission-static-v1.json"))
    for segment in fixture.static.extensionRoute.segments:
        if segment.kind != "navigation" or int(segment.map) != 40: continue
        for input in segment.inputs:
            if not failures.is_empty(): return
            await _press({"Up":KEY_UP,"Right":KEY_RIGHT,"Down":KEY_DOWN,"Left":KEY_LEFT}[input])
            await _world_settle()
    _check(not is_instance_valid(exploration), "ordinary map warp and admitted battle startup hand off to battle")

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

func _settle() -> Dictionary:
    for tick in range(2400):
        var state := _read()
        if state.failure != null:
            _check(false, "host failure: " + str(state.failure))
            return state
        if state.scene.visible:
            if not in_scene:
                in_scene = true
                scenes += 1
                start_cursor = int(state.cursor)
                initial_seed = int(state.mainSeed)
            _check(int(state.cursor) == start_cursor, "scene retains the current turn cursor")
            if state.scene.phase == "ActionMessage" and not input_checked:
                var before: int = int(state.scene.waitToken)
                await _press(KEY_ESCAPE)
                await _press(KEY_W)
                state = _read()
                _check(int(state.scene.waitToken) == before, "cancel and movement cannot release the scene wait")
                input_checked = true
            if state.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        else:
            if in_scene:
                in_scene = false
                _check(int(state.mainSeed) != initial_seed, "source scene RNG changes the shared main image")
                return state
            if state.actor != null:
                return state
            await process_frame
    _check(false, "bounded physical scene did not reach control")
    return _read()

func _stay() -> Dictionary:
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    return await _settle()

func _open_output() -> bool:
    var output_path := OS.get_environment("SF2_BATTLE_SCENE_OBSERVATION_OUTPUT").replace("\\", "/").simplify_path()
    var local_root := ProjectSettings.globalize_path("res://../../local/").replace("\\", "/").simplify_path().trim_suffix("/") + "/"
    if not output_path.to_lower().begins_with(local_root.to_lower()) or FileAccess.file_exists(output_path):
        push_error("Battle scene output must be fresh and worktree-local")
        quit(2)
        return false
    output = FileAccess.open(output_path, FileAccess.WRITE)
    if output == null:
        push_error("Battle scene output is unavailable")
        quit(2)
        return false
    return true

func _run() -> void:
    if not _open_output(): return
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        _check(false, "ordinary battle view exists")
        _finish()
        return
    view.connect("SessionResultObserved", _result)
    if OS.get_environment("SF2_BATTLE_SCENE_WORLD") == "1":
        await _admit_world()
        if not failures.is_empty():
            _finish()
            return
    var state := await _settle()
    if state.failure != null:
        _finish()
        return
    # Existing controlled Battle01 entry; ordinary player moves invite the first
    # enemy attack. No runtime state edits or RNG reset and no old final seed golden.
    var bowie_turns := 0
    for turn in range(36):
        if scenes > 0 or not failures.is_empty():
            break
        if state.actor == "ally-0":
            bowie_turns += 1
            if bowie_turns == 1:
                await _press(KEY_W)
            elif bowie_turns == 2:
                for step in range(3): await _press(KEY_D)
                for step in range(2): await _press(KEY_W)
        state = await _stay()
    _check(scenes > 0 and not in_scene, "one actual physical scene completes")
    if OS.get_environment("SF2_BATTLE_SCENE_REWARD") == "1":
        # A separately declared controlled party exercises reward/growth rendering.
        # Ordinary input still selects and commits the actual adjacent target.
        for turn in range(12):
            if state.actor == "ally-0" or not failures.is_empty(): break
            state = await _stay()
        _check(state.actor == "ally-0", "controlled reward case reaches Bowie input")
        if failures.is_empty():
            await _press(KEY_ENTER)
            await _press(KEY_F)
            for target in range(5): await _press(KEY_TAB)
            await _press(KEY_ENTER)
            state = await _settle()
        _check(projections.any(func(sample): return sample.scene.phase == "DeathMessage"), "actual enemy death message is consumed")
        _check(projections.any(func(sample): return sample.scene.phase == "RewardMessage"), "actual earned experience message is consumed")
        _check(projections.any(func(sample): return sample.scene.phase == "GrowthMessage"), "actual level and stat messages are consumed")
        _check(projections.any(func(sample): return sample.scene.phase == "GoldMessage"), "actual gold message is consumed")
        _check(receipts.any(func(receipt): return receipt.Operation == "started" and receipt.Command == 102), "level SFX actually starts")
        _check(events.any(func(event): return event.Kind == "kills" and event.Actor.Value == "ally-0"), "kill accounting executes after scene messages")
    _check(input_checked, "pending message rejects unrelated physical input")
    var draws := events.filter(func(event): return str(event.Kind).begins_with("rng-reaction-"))
    _check(draws.size() >= 24, "actual consumer crosses the source reaction RNG boundary")
    _check(projections.any(func(sample): return sample.scene.textureCount >= 5 and sample.scene.allyResource != null and sample.scene.enemyResource != null), "original role, weapon and environment textures are consumed")
    if OS.get_environment("SF2_BATTLE_SCENE_WORLD") == "1":
        var scene_music := receipts.filter(func(receipt): return receipt.Operation == "started" and receipt.Command in [2.0,5.0])
        _check(not scene_music.is_empty(), "source-selected scene music actually starts")
        _check(receipts.any(func(receipt): return receipt.Command == 253 and receipt.Operation == "fade-command"), "253 reaches actual shared audio consumer")
        _check(receipts.any(func(receipt): return receipt.Operation == "started" and receipt.Command in [81.0,83.0]), "source reaction SFX actually starts")
    _finish()

func _finish() -> void:
    var result := {"passed": failures.is_empty(), "failures": failures, "elapsedMs": Time.get_ticks_msec()-started,
        "scope": "controlled-physical-source-assets-no-original-runtime-parity", "scenes": scenes,
        "events": events, "projections": projections, "audioReceipts": receipts,
        "worldBoundaries": world_boundaries,
        "audioDriver": AudioServer.get_driver_name()}
    output.store_string(JSON.stringify(result))
    output.flush()
    var write_error := output.get_error()
    output.close()
    if write_error != OK:
        _check(false, "Battle scene output could not be written")
    if is_instance_valid(view) and view.is_connected("SessionResultObserved", _result):
        view.disconnect("SessionResultObserved", _result)
    if is_instance_valid(exploration) and exploration.is_connected("SessionResultObserved", _result):
        exploration.disconnect("SessionResultObserved", _result)
    if is_instance_valid(host):
        host.queue_free()
        await process_frame
        await process_frame
    quit(0 if failures.is_empty() else 1)
