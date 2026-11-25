extends "res://probes/engine_battle_save_observation.gd"

# The same Main scene and input sequence run with or without Movie Maker.
var inputs: Array = []
var began := 0
var recording := OS.has_feature("movie")

func key(code: int) -> void:
    if not accept(root.has_focus(), "Showcase window owns input before " + OS.get_keycode_string(code)):
        return
    inputs.append({"frame": elapsed_frames(), "key": OS.get_keycode_string(code)})
    await super.key(code)

func elapsed_frames() -> int:
    return Engine.get_process_frames() - began

func hold(movie_frames: int) -> void:
    # At 60 fps. Only the viewing pause differs between the two runs.
    await frames(movie_frames if recording else 6)

func entry() -> Dictionary:
    var page := host.find_child("SessionEntryView", true, false)
    return {} if page == null else JSON.parse_string(page.call("ReadEntryObservationJson"))

func opening() -> Dictionary:
    return JSON.parse_string(host.call("ReadOpeningObservationJson"))

func ready(condition: Callable, limit: int, message: String) -> bool:
    for i in range(limit):
        if condition.call():
            return true
        await process_frame
    check(false, message)
    return false

func movement_ready() -> bool:
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        return false
    var board := state()
    return board.get("stage") == "Movement" and board.get("stopReason") == "PlayerInput" and board.movement == null

func mark(label: String) -> void:
    var row := {"label": label, "frame": elapsed_frames(), "focused": root.has_focus()}
    check(row.focused, "Showcase window owns input at " + label)
    var page := entry() if view == null else {}
    var audio: Variant = page.get("audio") if view == null else JSON.parse_string(host.call("ReadAudioObservationJson"))
    if not accept(audio is Dictionary, "Audio observation available at " + label):
        return
    row.audio = {"cue": audio.musicCue, "playing": audio.musicPlaying, "error": audio.error,
        "sound_cue": audio.soundCue, "sequence": audio.sequence}
    check(audio.error == null, "No audio error at " + label)
    if view != null:
        var board := state()
        row.battle = {"session": board.sessionId, "map": board.map, "round": board.round,
            "actor": board.actor, "queue": board.queueCursor, "revision": board.revision,
            "stage": board.stage, "stop": board.stopReason, "preview": [board.previewX, board.previewY],
            "inspection": board.inspection.page, "menu": board.menu.page,
            "failed": board.failure, "scene_error": board.scene.error}
        check(board.failure == null and board.scene.error == null, "No battle error at " + label)
    else:
        var slots := panel()
        row.entry = {"stage": page.get("stage"), "selected": page.get("selectedAction"),
            "failure": page.get("failure"), "asset_error": page.get("assetError"),
            "slot_page": slots.get("state"), "slot_index": slots.get("selected"),
            "continue_text": page.get("continueText"), "text_characters": page.get("totalCharacters")}
        check(page.get("failure") == null and page.get("assetError") == null, "No entry error at " + label)
    samples.append(row)

func accept(ok: bool, message: String) -> bool:
    check(ok, message)
    return ok

func move_one() -> bool:
    var board := state()
    var terrain: Dictionary = JSON.parse_string(view.get_node("MapViewport").call("ReadTerrainJson"))
    var destinations: Array = terrain.movementRange.destinations
    for step in [[KEY_UP, 0, -1], [KEY_RIGHT, 1, 0], [KEY_DOWN, 0, 1], [KEY_LEFT, -1, 0]]:
        var x: int = board.previewX + step[1]
        var y: int = board.previewY + step[2]
        if destinations.any(func(cell): return cell.X == x and cell.Y == y):
            await key(step[0])
            return await ready(func():
                return movement_ready() and state().previewX == x and state().previewY == y,
                300, "Legal direction reaches the intended preview")
    return accept(false, "Published checkpoint provides a legal adjacent stop")

func play() -> void:
    if not await ready(func(): return root.has_focus() and opening().active,
            300, "Focused production opening is available"):
        return
    await key(KEY_E)
    if not await ready(func():
            var start := opening()
            return start.result == "skipped" and not start.releaseBarrier and entry().get("stage") == "title",
            300, "Ordinary Skip hands off to a neutral title"):
        return
    mark("title")
    samples[-1].opening_result = opening().result
    await hold(120)
    await key(KEY_ENTER)
    if not accept(entry().get("stage") == "dialogue", "Title opens the Witch greeting"):
        return
    mark("witch")
    await hold(210)
    await key(KEY_E)
    if not await ready(func(): return entry().get("selectedAction") == "Continue", 600,
            "Witch defaults to Continue for the admitted slot"):
        return
    mark("continue-action")
    await hold(40)
    await key(KEY_ENTER)
    if not await ready(func(): return entry().get("continueText") == 225, 300,
            "Continue opens the Witch's slot question"):
        return
    mark("continue-question")
    await hold(205)
    await key(KEY_ENTER)
    if not await ready(func(): return panel().get("state") == "continue-slots", 300, "Continue opens the slot list"):
        return
    mark("continue-slots")
    if not accept(panel().get("selected") == 0, "Continue highlights the admitted Slot 1"):
        return
    await hold(75)
    await key(KEY_ENTER)
    if not await ready(func(): return entry().get("continueText") == 226, 600,
            "Slot 1 admission opens recognition"):
        return
    mark("continue-recognition")
    await hold(240)
    await key(KEY_ENTER)
    if not await ready(func(): return entry().get("continueText") == 224, 300,
            "Recognition leads to farewell"):
        return
    mark("continue-farewell")
    await hold(180)
    await key(KEY_ENTER)
    if not await ready(movement_ready, 600, "Slot 1 mounts stable battle Movement"):
        return
    var first := state()
    if not accept(first.map == "map57" and first.round == 1 and first.actor == "ally-2",
            "Continue restores the selected Battle01 Chester turn"):
        return
    mark("battle-movement")
    await hold(130)
    await key(KEY_X)
    if not accept(state().inspection.active and state().inspection.page == "Cursor", "Cancel opens battle inspection"):
        return
    mark("inspection")
    await hold(135)
    await key(KEY_X)
    if not accept(movement_ready() and not state().inspection.active and state().actor == first.actor
            and state().queueCursor == first.queueCursor, "Back returns to the unspent turn"):
        return
    mark("inspection-back")
    await hold(35)
    if not await move_one():
        return
    var moved := state()
    mark("preview")
    await hold(60)
    await key(KEY_ENTER)
    if not accept(state().stage == "ActionChoice" and state().menu.visible and state().menu.page == "Main",
            "Confirm opens the ordinary battle command list"):
        return
    mark("action-list")
    await hold(130)
    await key(KEY_SPACE)
    mark("stay-submitted")
    if not await ready(func(): return movement_ready() and state().actor != first.actor,
            600, "Stay returns to another player's Movement"):
        return
    var next := state()
    var prior: Array = next.actors.filter(func(actor): return actor.id == first.actor)
    if not accept(next.sessionId == first.sessionId and next.round == first.round
            and next.queueCursor > first.queueCursor and prior.size() == 1
            and prior[0].x == moved.previewX and prior[0].y == moved.previewY,
            "Stay commits the chosen tile and advances the live turn queue"):
        return
    mark("next-player")
    await hold(65)
    if not await move_one():
        return
    mark("next-player-preview")
    await hold(105)

func run() -> void:
    began = Engine.get_process_frames()
    if DisplayServer.get_name() == "headless":
        root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await play()
    finish()

func write_report() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_SAVE_REPORT"), FileAccess.WRITE)
    if output == null:
        check(false, "Showcase report path is unavailable")
        return
    output.store_string(JSON.stringify({"pid": OS.get_process_id(), "recording": recording,
        "frames": elapsed_frames(), "extent": [root.size.x, root.size.y], "inputs": inputs,
        "samples": samples, "failures": failures}, "\t"))
