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
var scene_start_token := -1
var scene_event_index := 0
var initial_seed := 0
var input_checked := false
var exploration: Node
var world_boundaries: Array = []
var last_world := ""
var herb_cases: Array = []
var herb_mode := OS.get_environment("SF2_BATTLE_SCENE_HERB") == "1"
var disjoint_audio := OS.get_environment("SF2_BATTLE_SCENE_DISJOINT_AUDIO") == "1"
var audio_overlaps: Array = []
var overlap_keys: Dictionary = {}
var menu_audio := OS.get_environment("SF2_BATTLE_SCENE_MENU_AUDIO") == "1"
var menu_cases: Array = []
var partial_audio := OS.get_environment("SF2_BATTLE_SCENE_PARTIAL_AUDIO") == "1"

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
    if herb_mode:
        var board: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
        state["inventories"] = board.inventories
        state["maximumHp"] = board.actors.map(func(actor): return {"actor":actor.id, "maxHp":actor.maxHp})
    var scene: Dictionary = state.scene
    var key := str([state.revision, scene.frameIndex, scene.reactionState, scene.visibleCharacters, scene.backgroundX, scene.allyX, scene.enemyX])
    if key != last_projection:
        last_projection = key
        projections.append(state)
    _audio()
    return state

func _audio() -> void:
    if not is_instance_valid(host): return
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    if audio != null:
        for receipt in audio.receipts:
            if receipt.Sequence <= last_receipt:
                continue
            _check(last_receipt == 0 or receipt.Sequence == last_receipt + 1, "audio receipt sequence has no gap")
            last_receipt = int(receipt.Sequence)
            receipts.append(receipt)
        _check(audio.error == null, "audio remains available")
        if disjoint_audio or partial_audio:
            var playing: Array = audio.sounds.filter(func(sound): return sound.playing)
            if playing.size() > 1:
                var key := str(playing.map(func(sound): return sound.startSequence))
                if not overlap_keys.has(key):
                    overlap_keys[key] = true
                    audio_overlaps.append({"microseconds":Time.get_ticks_usec(), "revision":audio.revision,
                        "waitToken":audio.waitToken, "sounds":playing})

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

func _overlap_finished(sample: Dictionary, pair: Array) -> bool:
    for command in pair:
        var sound: Dictionary = sample.sounds.filter(func(voice): return voice.command == command)[0]
        var ending := receipts.filter(func(receipt): return receipt.Sequence > sound.startSequence and receipt.Command == command and receipt.Operation in ["finished", "stopped"])
        if ending.is_empty() or ending[0].Operation != "finished": return false
    return true

func _menu_input(key: Key, before_stage: String, after_stage: String, expected: Array, rejected := false) -> void:
    _audio()
    var begin := receipts.size()
    var before: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    _check(before.stage == before_stage, "menu input starts in " + before_stage)
    await _press(key)
    _audio()
    var after: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    var played := receipts.slice(begin).filter(func(receipt): return receipt.Operation == "started")
    _check(after.stage == after_stage, "menu input ends in " + after_stage)
    _check((after.failure != null) == rejected, "expected menu input acceptance")
    _check(played.map(func(receipt): return int(receipt.Command)) == expected, "exact semantic input audio: " + str(expected))
    var actual = JSON.parse_string(host.call("ReadAudioObservationJson"))
    if 65 in expected:
        _check(played.size() == 1 and played[0].TimerB == 189 and played[0].Playing, "one original 65/BD stream starts")
        _check(actual.sounds.any(func(sound): return sound.command == 65 and sound.playing), "menu cue has an actual playing voice")
    menu_cases.append({"key":key, "actor":before.actor, "before":before.stage, "after":after.stage,
        "failure":after.failure, "revision":after.revision, "mainSeed":after.mainSeed,
        "expected":expected, "receipts":receipts.slice(begin), "audio":actual})

func _menu_idle() -> void:
    var before: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    var begin := receipts.size()
    for tick in range(90):
        view.queue_redraw()
        await process_frame
        _audio()
        var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
        if audio.sounds.is_empty(): break
    var after: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    for field in ["actor", "stage", "revision", "observationSequence", "mainSeed", "thinkingSeed"]:
        _check(before[field] == after[field], "redraw/playback delivery preserves " + field)
    _check(not receipts.slice(begin).any(func(receipt): return receipt.Operation == "started"), "redraw does not replay audio")

func _run_menu_audio() -> void:
    var actors: Array = []
    for turn in range(2):
        var initial: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
        actors.append(initial.actor)
        await _menu_input(KEY_ENTER, "Movement", "ActionChoice", [65])
        await _menu_idle() # Real natural completion before another ordinary command.
        await _menu_input(KEY_ENTER, "ActionChoice", "ActionChoice", [], true)
        await _menu_input(KEY_ESCAPE, "ActionChoice", "Movement", [65])
        await _menu_input(KEY_ESCAPE, "Movement", "Movement", [66])
        await _menu_input(KEY_ENTER, "Movement", "ActionChoice", [65])
        await _menu_input(KEY_F, "ActionChoice", "TargetChoice", [65])
        await _menu_input(KEY_ENTER, "TargetChoice", "TargetChoice", [], true)
        await _menu_input(KEY_ESCAPE, "TargetChoice", "Movement", [66])
        await _menu_input(KEY_ENTER, "Movement", "ActionChoice", [65])
        await _menu_input(KEY_SPACE, "ActionChoice", "CommitReady", [65])
        await _menu_input(KEY_SPACE, "CommitReady", "CommitReady", [], true)
        if turn == 0:
            await _press(KEY_ENTER) # Commit the real Stay and let the ordinary turn flow run.
            await _settle()
        else:
            await _menu_input(KEY_ESCAPE, "CommitReady", "Movement", [66])
    await _menu_idle()
    _check(actors[0] != actors[1], "action-menu mapping works for another actual actor")
    _check(receipts.any(func(receipt): return receipt.Command == 65 and receipt.Operation == "finished"), "actual menu voice naturally finishes")
    _check(receipts.any(func(receipt): return receipt.Command == 65 and receipt.Operation == "stopped"), "rapid menu transition legitimately replaces the same slot")

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
            if not in_scene or (state.scene.phase == "Initialize" and int(state.scene.waitToken) != scene_start_token):
                if in_scene:
                    _check(events.slice(scene_event_index).any(func(event): return event.Kind == "scene-ended"), "a chained scene follows the prior scene end")
                scene_start_token = int(state.scene.waitToken)
                scene_event_index = events.size()
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
                if not herb_mode:
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

func _chester_physical(state: Dictionary) -> Dictionary:
    # Narrow regression for the source starting Wooden Stick resource/ordinary
    # sequence binding. Walk and select an actual adjacent target with normal keys.
    var attacked := false
    for turn in range(30):
        if not failures.is_empty() or attacked: break
        if state.actor != "ally-2":
            if OS.get_environment("SF2_BATTLE_SCENE_WOUNDED") == "1" and state.actor == "ally-1":
                # Keep the healer with the advancing holder before enemies occupy the corridor.
                for step in range(5):
                    var formation: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
                    if formation.previewY <= 13: break
                    var north: bool = formation.previewY == 18 or formation.previewX >= 11
                    var next_x: int = int(formation.previewX) + (0 if north else 1)
                    var next_y: int = int(formation.previewY) - (1 if north else 0)
                    if formation.actors.any(func(actor): return actor.hp > 0 and actor.x == next_x and actor.y == next_y): break
                    await _press(KEY_W if north else KEY_D)
            state = await _stay()
            continue
        var board: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
        var enemies: Array = board.actors.filter(func(actor): return str(actor.id).begins_with("enemy-") and actor.hp > 0)
        var target: Dictionary = enemies[0]
        for step in range(7):
            board = JSON.parse_string(view.call("ReadObservationJson"))
            var x: int = int(board.previewX)
            var y: int = int(board.previewY)
            enemies.sort_custom(func(a,b): return abs(a.x-x)+abs(a.y-y) < abs(b.x-x)+abs(b.y-y))
            target = enemies[0]
            if abs(target.x-x)+abs(target.y-y) <= 1: break
            # The admitted Tower entrance route goes through column11, as in the physical observer.
            if y == 18: await _press(KEY_W)
            elif y >= 15 and x < 11: await _press(KEY_D)
            else: await _press(KEY_W if y > target.y else KEY_S if y < target.y else KEY_D if x < target.x else KEY_A)
        board = JSON.parse_string(view.call("ReadObservationJson"))
        if abs(target.x-board.previewX)+abs(target.y-board.previewY) > 1:
            state = await _stay()
            continue
        await _press(KEY_ENTER)
        await _press(KEY_F)
        for choice in range(7):
            board = JSON.parse_string(view.call("ReadObservationJson"))
            if board.failure == null and board.target == target.id: break
            await _press(KEY_TAB)
        _check(board.failure == null and board.target == target.id, "adjacent Chester physical target selected")
        if not failures.is_empty(): break
        await _press(KEY_ENTER)
        state = await _settle()
        attacked = true
    _check(attacked, "Chester executes a physical action with his live starting equipment")
    _check(projections.any(func(sample): return sample.scene.phase == "ActionAnimation" and sample.scene.displayedAlly == "ally-2" and str(sample.scene.weaponResource).begins_with("weapon56/") and sample.scene.animationIndex == 2), "Wooden Stick and ordinary KNTE sequence are consumed by a physical action")

    return state

func _run_herbs(state: Dictionary, requests: Array = [{"actor":"ally-2", "target":"ally-2"}, {"actor":"ally-1", "target":"ally-0"}]) -> void:
    # Controlled starting party only; action legality and effects use ordinary keys.
    for request in requests:
        for turn in range(8):
            if state.actor == request.actor or not failures.is_empty(): break
            state = await _stay()
        _check(state.actor == request.actor, "controlled herb holder reaches ordinary input")
        if not failures.is_empty(): return
        var before := state.duplicate(true)
        await _press(KEY_ENTER)
        for slot in range(4):
            await _press(KEY_I)
            var selection: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
            if selection.failure == null and selection.itemSlot != null: break
        for target in range(3):
            var selection: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
            if selection.failure == null and selection.target == request.target: break
            await _press(KEY_TAB)
        var selected: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
        _check(selected.failure == null and selected.target == request.target, "ordinary herb and target selection succeeds")
        if not failures.is_empty(): return
        var begin_event := events.size()
        var begin_projection := projections.size()
        await _press(KEY_ENTER)
        state = await _settle()
        # The end command may immediately start automatic enemy scenes before control.
        # Bound item assertions at its own scene/after-turn, keeping later events intact.
        var observed := projections.slice(begin_projection).filter(func(sample): return sample.scene.actionKind == "item-use")
        var action_events: Array = []
        for event in events.slice(begin_event):
            action_events.append(event)
            if event.Kind == "after-turn" and event.Actor.Value == request.actor: break
        _check(not observed.is_empty(), "the selected item scene is observed")
        if observed.is_empty(): return
        var scene_end: Dictionary = observed[-1]
        _check(scene_end.scene.phase == "End", "item assertions end at its final scene phase")
        herb_cases.append({"request":request,"before":before,"after":state,"sceneEnd":scene_end,"events":action_events})
        _check(not in_scene and state.failure == null, "herb scene ends and releases input")
        _check(action_events.filter(func(event): return event.Kind == "item-consumed").size() == 1, "one herb slot consumed")
        _check(action_events.filter(func(event): return event.Kind == "after-turn" and event.Actor.Value == request.actor).size() == 1, "holder turn consumed once at end")
        _check(not action_events.any(func(event): return str(event.Kind).begins_with("rng-reaction-")), "recovery adds no physical recoil RNG")
        _check(observed.all(func(sample): return not sample.scene.visible or (sample.scene.displayedEnemy == null and not sample.scene.enemyVisible)), "single-side scene never invents an enemy")
        _check(observed.any(func(sample): return sample.scene.phase == "ActionAnimation" and sample.scene.spellAnimationSelector == 0), "use-item selects NONE rather than fairy")
        _check(observed.any(func(sample): return sample.scene.phase == "Reaction" and sample.scene.reactionKind == "Recovery" and sample.scene.displayedAlly == request.target), "recovery uses the actual target projection")
        _check(observed.any(func(sample): return sample.scene.phase == "RewardMessage" and sample.scene.displayedAlly == request.actor), "reward follows restoration of the actor")
        _check(observed.any(func(sample): return sample.scene.phase == "TargetEnter") == (request.actor != request.target), "only other-ally healing switches targets")
        var before_target: Dictionary = before.actors.filter(func(actor): return actor.actor == request.target)[0]
        var after_target: Dictionary = scene_end.actors.filter(func(actor): return actor.actor == request.target)[0]
        var maximum: float = before.maximumHp.filter(func(actor): return actor.actor == request.target)[0].maxHp
        if OS.get_environment("SF2_BATTLE_SCENE_WOUNDED") == "1":
            _check(before_target.hp < maximum and after_target.hp > before_target.hp, "actual battle injury precedes positive herb recovery")
        _check(after_target.hp == min(maximum, before_target.hp + 10) and after_target.mp == before_target.mp, "herb recovery clamps to live HP and preserves MP")
        for sample in observed:
            if sample.scene.phase in ["Initialize", "ActionMessage", "ActionAnimation", "TargetExit", "TargetEnter"]:
                _check(sample.actors.filter(func(actor): return actor.actor == request.target)[0].hp == before_target.hp, "HP stays unchanged until the recovery command")
    if OS.get_environment("SF2_BATTLE_SCENE_WORLD") == "1":
        _check(receipts.filter(func(receipt): return receipt.Operation == "started" and receipt.Command == 113).size() == 2, "both actual recovery SFX start")
    _check(not receipts.any(func(receipt): return receipt.Operation == "started" and receipt.Command == 77), "no spellcast SFX from herb NONE animation")
    if OS.get_environment("SF2_BATTLE_SCENE_WORLD") == "1":
        _check(projections.any(func(sample): return sample.scene.phase == "GrowthMessage"), "controlled priest reward consumes growth messages")
    _check(input_checked, "ordinary cancel/movement cannot release a pending herb scene")

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
    if disjoint_audio or menu_audio or partial_audio: process_frame.connect(_audio)
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
    if menu_audio:
        await _run_menu_audio()
        _finish()
        return
    if OS.get_environment("SF2_BATTLE_SCENE_WOUNDED") == "1":
        state = await _chester_physical(state)
        # Bring Sarah next to the actually injured Chester through the same entrance.
        for turn in range(12):
            if not failures.is_empty(): break
            if state.actor == "ally-1":
                for step in range(5):
                    var board: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
                    var chester: Dictionary = board.actors.filter(func(actor): return actor.id == "ally-2")[0]
                    var x: int = int(board.previewX)
                    var y: int = int(board.previewY)
                    if abs(chester.x-x)+abs(chester.y-y) == 1: break
                    await _press(KEY_W if y == 18 or x >= 11 else KEY_D)
                var board: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
                var chester: Dictionary = board.actors.filter(func(actor): return actor.id == "ally-2")[0]
                if abs(chester.x-board.previewX)+abs(chester.y-board.previewY) == 1: break
            state = await _stay()
        _check(state.actor == "ally-1", "Sarah reaches adjacent ordinary herb choice")
        if failures.is_empty():
            await _run_herbs(state, [{"actor":"ally-1","target":"ally-2"},{"actor":"ally-1","target":"ally-1"}])
        _finish()
        return
    if OS.get_environment("SF2_BATTLE_SCENE_CHESTER") == "1":
        await _chester_physical(state)
        _finish()
        return
    if herb_mode:
        await _run_herbs(state)
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
    if menu_audio and process_frame.is_connected(_audio): process_frame.disconnect(_audio)
    if disjoint_audio or partial_audio:
        # Input is already released. Observe the remaining playback tail without
        # adding a product wait or submitting further gameplay input.
        var before_tail: String = view.call("ReadObservationJson") if is_instance_valid(view) else ""
        for tick in range(120):
            _audio()
            var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
            if audio == null or audio.sounds.is_empty(): break
            await process_frame
        if is_instance_valid(view):
            _check(str(view.call("ReadObservationJson")) == before_tail, "playback tail adds no gameplay work after input release")
        _audio()
    if partial_audio:
        var partials := audio_overlaps.filter(func(sample):
            return sample.sounds.any(func(sound): return sound.command == 113) and sample.sounds.any(func(sound): return sound.command == 65))
        _check(not partials.is_empty(), "ordinary Herb tail and menu 65 play concurrently")
        _check(receipts.any(func(receipt): return receipt.Command == 113 and receipt.Operation == "finished"), "Herb tail reaches actual Finished")
        _check(not receipts.any(func(receipt): return receipt.Command == 113 and receipt.Operation == "stopped"), "menu input preserves whole Herb tail")
        for sample in partials:
            for sound in sample.sounds.filter(func(voice): return voice.command in [113,65]):
                _check(receipts.any(func(receipt): return receipt.Sequence > sound.startSequence and receipt.Command == sound.command and receipt.Operation in ["finished", "stopped"]), "overlapping voice ends or is replaced")
        if process_frame.is_connected(_audio): process_frame.disconnect(_audio)
    if disjoint_audio:
        var pair := [113,67] if herb_mode else [83,102]
        var overlaps := audio_overlaps.filter(func(sample):
            return sample.sounds.any(func(sound): return sound.command == pair[0]) and sample.sounds.any(func(sound): return sound.command == pair[1]))
        _check(not overlaps.is_empty(), "ordinary scene actually plays both disjoint effects concurrently: " + str(pair))
        # Rapid ordinary UI input may legitimately replace a tone with another tone.
        # Require an observed concurrent pair whose two instances both finish naturally.
        _check(overlaps.any(func(sample): return _overlap_finished(sample, pair)), "both instances of a concurrent disjoint pair reach actual Finished: " + str(pair))
        _check(not receipts.any(func(receipt): return receipt.Command == pair[0] and receipt.Operation == "stopped"), "independent reaction effect is not truncated by UI input")
        if process_frame.is_connected(_audio): process_frame.disconnect(_audio)
    var result := {"passed": failures.is_empty(), "failures": failures, "elapsedMs": Time.get_ticks_msec()-started,
        "scope": "controlled-action-menu-audio-no-original-window-parity" if menu_audio else "controlled-herb-source-assets-no-original-runtime-parity" if herb_mode else "controlled-physical-source-assets-no-original-runtime-parity", "herbCases":herb_cases, "scenes": scenes,
        "events": events, "projections": projections, "audioReceipts": receipts,
        "worldBoundaries": world_boundaries, "audioOverlaps": audio_overlaps, "menuAudioCases": menu_cases,
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
