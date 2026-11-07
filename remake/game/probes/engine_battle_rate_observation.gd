extends SceneTree

# Selected real consumer endpoints and semantic counts, never a frame trace.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var counts: Dictionary = {}
var work: Dictionary = {}
var phases: Array = []
var last_sequence := 0
var shift := false
var ended := false
var began := Time.get_ticks_usec()
var factor := OS.get_environment("SF2_BATTLE_RATE_FACTOR")
var case_name := OS.get_environment("SF2_BATTLE_RATE_CASE")
var markers: Dictionary = {}
var movement_marks: Array = []
var field_ack := ""
var cue_finished := 0

func _initialize() -> void:
    create_timer(45).timeout.connect(func(): finish("45-second deadline"))
    call_deferred("run")

func frames(n := 1) -> void:
    for i in n: await process_frame

func check(ok: bool, reason: String) -> void:
    if not ok: failures.append(reason)

func edge(code: Key, pressed: bool, echo := false) -> void:
    if code == KEY_SHIFT and not echo: shift = pressed
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = pressed
    event.echo = echo
    event.shift_pressed = shift
    if code == KEY_SHIFT: event.location = KEY_LOCATION_LEFT
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func key(code: Key, pressed: bool) -> void:
    edge(code, pressed)
    await frames(2)

func tap(code: Key) -> void:
    await key(code, true)
    await key(code, false)

func board() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func scene() -> Dictionary:
    return JSON.parse_string(view.call("ReadSceneObservationJson"))

func observed(raw: String) -> void:
    var result: Dictionary = JSON.parse_string(raw)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        check(last_sequence == 0 or int(event.Sequence) == last_sequence + 1, "semantic sequence gap")
        last_sequence = int(event.Sequence)
        counts[event.Kind] = int(counts.get(event.Kind, 0)) + 1
        if event.Kind == "scene-logical-step": work[event.Detail] = int(work.get(event.Detail, 0)) + 1
        if event.Kind == "scene-step-completed": phases.append(event.Detail)
        if event.Kind in ["battle-movement-segment-started", "battle-movement-segment-arrived"] and movement_marks.size() < 12:
            movement_marks.append({"kind":event.Kind,"usec":Time.get_ticks_usec(),"from":event.From,"to":event.To})
        if event.Kind == "retreat-effect-requested": markers.effectStartUsec = Time.get_ticks_usec()
        if event.Kind == "retreat-close-requested":
            markers.effectCloseUsec = Time.get_ticks_usec()
            var cue := host.find_child("AuthoredRetreatCue", true, false)
            markers.cuePlayingAtClose = cue.playing if cue else null

func snapshot(label: String) -> Dictionary:
    var s := board()
    var row := {}
    for field in ["mainSeed", "thinkingSeed", "gold", "round", "actor", "queueCursor", "stage", "selection", "inventories", "statuses", "failure", "pacing"]:
        row[field] = s.get(field)
    row["label"] = label
    row["elapsedUsec"] = Time.get_ticks_usec() - began
    row["actors"] = []
    for actor in s.get("actors", []):
        var a := {}
        for field in ["id", "x", "y", "hp", "mp", "exp", "kills", "defeats", "status", "items"]: a[field] = actor.get(field)
        row.actors.append(a)
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    row["audio"] = {}
    if audio != null:
        for field in ["error", "musicCue", "musicPaused", "musicPlaying", "musicFinished", "soundCue", "soundPlaying"]:
            if audio.has(field): row.audio[field] = audio[field]
    row["pitch"] = []
    for player in host.find_children("*", "AudioStreamPlayer", true, false): row.pitch.append(player.pitch_scale)
    samples.append(row)
    check(s.failure == null, "healthy " + label + ": " + str(s.failure))
    return s

func arm() -> void:
    if factor != "1.5": return
    if shift: await key(KEY_SHIFT, false)
    await key(KEY_SHIFT, true)

func ready() -> bool:
    for i in 180:
        var s := board()
        if s.stage != null and s.movement == null and not s.scene.visible and s.failure == null: return true
        await frames()
    check(false, "stable player selection")
    return false

func heal() -> void:
    if not await ready(): return
    snapshot("initial")
    await tap(KEY_ENTER)
    await tap(KEY_H)
    for i in 6:
        if int(counts.get("scene-prepared", 0)) > 0: break
        await tap(KEY_ENTER)
    check(int(counts.get("scene-prepared", 0)) == 1, "one real HEAL committed")
    await arm()
    for i in 2500:
        var s := scene()
        if s.failure != null:
            check(false, "scene failure " + str(s.failure))
            break
        if int(counts.get("scene-ended", 0)) > 0:
            if await ready(): snapshot("after one HEAL")
            return
        if s.scene.requiresAcknowledgement:
            var healing = s.scene.healing
            if healing == null or s.scene.phase not in ["ActionMessage", "ResultMessage"] or (healing.AtTimedInput or healing.LogicalComplete) and healing.Delivered:
                await tap(KEY_ENTER)
                await arm()
                continue
        await frames()
    check(false, "HEAL finite completion")

func movement() -> void:
    if not await ready(): return
    var initial := snapshot("initial movement")
    await arm()
    await tap(KEY_UP)
    if not await ready(): return
    var moved := snapshot("one Up delivered")
    check(moved.previewX == 3 and moved.previewY == 2, "one legal provisional cell")
    await tap(KEY_UP)
    if not await ready(): return
    snapshot("second warmed Up delivered")
    root.emit_signal("focus_exited")
    var paused := board()
    await create_timer(0.15).timeout
    check(board().pacing.steps == paused.pacing.steps and paused.pacing.input.rate == 1, "focus handler stops battle and resets hold")
    root.emit_signal("focus_entered")
    await frames(3)
    check(board().pacing.input.rate == 1, "focus handler cannot rearm held Shift")
    OS.delay_msec(120)
    await frames(2)
    check(board().pacing.maxSteps <= 4 and board().pacing.debt < 1.0/60, "stall caps service and discards whole debt")
    await tap(KEY_LEFT)
    if shift: await key(KEY_SHIFT, false)
    check(board().pacing.input.rate == 1, "release restores rate during motion")
    if not await ready(): return
    await tap(KEY_ESCAPE)
    for i in 180:
        if board().inspection.active: break
        await frames()
    var after := snapshot("actual return and inspection")
    check(after.inspection.active and after.pacing.input.rate == 1, "inspection handoff resets")
    for field in ["mainSeed", "thinkingSeed", "gold", "inventories", "statuses"]:
        check(initial[field] == after[field], "movement/return preserves " + field)
    check(int(counts.get("action-committed", 0)) == 0, "navigation never spends an action")

func reading() -> void:
    if not await ready(): return
    await tap(KEY_ENTER)
    await tap(KEY_F)
    for i in 4:
        if int(counts.get("scene-prepared", 0)) > 0: break
        await tap(KEY_ENTER)
    for i in 180:
        if scene().scene.requiresAcknowledgement: break
        await frames()
    await arm()
    var before := scene()
    check(before.scene.requiresAcknowledgement, "real action message")
    var usec := Time.get_ticks_usec()
    await create_timer(0.4).timeout
    var after := scene()
    markers.reading = {"before":before.scene.visibleCharacters,"after":after.scene.visibleCharacters,"usec":Time.get_ticks_usec()-usec,"cps":15}
    check(after.scene.waitToken == before.scene.waitToken and after.mainSeed == before.mainSeed, "Shift cannot acknowledge or draw RNG")
    check(after.scene.visibleCharacters-before.scene.visibleCharacters <= 8, "reading stays at15 cps")
    check(board().pacing.input.rate == 1, "message hold cannot arm rate")
    snapshot("real reading and confirmation boundary")

func field_snapshot(label: String) -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    if not field: return {}
    var s: Dictionary = JSON.parse_string(field.call("ReadObservationJson"))
    var row := {"label":label}
    for k in ["sessionId", "map", "mode", "wait", "token", "canWaitAtInput", "flags", "gold", "mainSeed", "thinkingSeed", "acceleration", "party", "worldGeometry"]: row[k] = s.get(k)
    row.entities = []
    if s.get("entities") != null:
        for entity in s.entities.slice(0, 1): row.entities.append({"x":entity.x,"y":entity.y})
    samples.append(row)
    check(s.failure == null and s.presentation.error == null, "healthy " + label)
    return s

func field_input(s: Dictionary) -> void:
    var panel: Dictionary = s.get("dialoguePanel", {})
    var id := str([s.token,s.get("textId"),panel.get("readThrough"),panel.get("scroll")])
    if (s.canWaitForText or s.canWaitForChoice) and id != field_ack:
        field_ack = id
        await tap(KEY_ENTER)
        await arm()
    else: await frames()

func settle_selection() -> bool:
    for i in 2000:
        var s := board()
        if not s.has("stage"): return false
        if s.story.active and s.story.kind == "TickWait":
            if not markers.has("storyWaitStartUsec"): markers.storyWaitStartUsec = Time.get_ticks_usec()
        elif markers.has("storyWaitStartUsec") and not markers.has("storyWaitEndUsec"):
            markers.storyWaitEndUsec = Time.get_ticks_usec()
        if s.stage != null and s.movement == null and s.scene.phase == null and not s.retreat.active and not s.story.active: return true
        if s.scene.requiresAcknowledgement:
            var h = s.scene.healing
            if h == null or s.scene.phase not in ["ActionMessage", "ResultMessage"] or (h.AtTimedInput or h.LogicalComplete) and h.Delivered:
                await tap(KEY_ENTER)
                await arm()
                continue
        if s.story.active and s.story.kind == "DialogueWait":
            await tap(KEY_ENTER)
            await arm()
            continue
        await frames()
    check(false, "finite selected battle continuation")
    return false

func private_egress() -> void:
    var field := host.find_child("ExplorationSessionView", true, false)
    if not field:
        check(false, "controlled field entry exists")
        return
    for i in 180:
        field = host.get_node_or_null("ExplorationSessionView")
        if not field: await frames(); continue
        var pending: Dictionary = JSON.parse_string(field.call("ReadObservationJson"))
        if pending.sessionId != null and pending.map != null: break
        await frames()
    view = host.get_node("BattleSessionView")
    var initial := field_snapshot("field before Battle02")
    last_sequence = int(initial.observationSequence)
    field.connect("SessionResultObserved", observed)
    view.connect("SessionResultObserved", func(raw):
        if JSON.parse_string(raw).boundary == "attach": markers.battleAttached = true
        observed(raw))
    await arm()
    markers.fieldRateBeforeApproach = JSON.parse_string(field.call("ReadObservationJson")).acceleration.rate
    check(initial.map == "map-3", "real field start precedes battle")
    await key(KEY_LEFT, true)
    for i in 1600:
        if markers.get("battleAttached", false) and view.is_visible_in_tree():
            var s := board()
            if s.has("stage") and s.stage != null and s.movement == null and s.scene.phase == null: break
        field = host.find_child("ExplorationSessionView", true, false)
        if field:
            var f: Dictionary = JSON.parse_string(field.call("ReadObservationJson"))
            check(f.failure == null, "field entry stays healthy")
            await field_input(f)
        else: await frames()
    await key(KEY_LEFT, false)
    if not markers.get("battleAttached", false):
        check(false, "finite real field approach reaches battle")
        return
    if not await settle_selection(): return
    var entry := snapshot("actual Battle02 control")
    check(entry.pacing.input.rate == 1, "field to battle clears rate")
    for i in 12:
        if board().actor == "ally-0": break
        await tap(KEY_ENTER)
        await tap(KEY_SPACE)
        await tap(KEY_ENTER)
        if not await settle_selection(): return
    check(board().actor == "ally-0", "ordinary queue reaches learned EGRESS caster")
    if not failures.is_empty(): return
    var cue := host.find_child("AuthoredRetreatCue", true, false)
    check(cue != null, "real retreat audio node")
    if cue:
        cue.finished.connect(func():
            cue_finished += 1
            markers.cueFinishedUsec = Time.get_ticks_usec()
            markers.cuePitch = cue.pitch_scale)
    snapshot("before EGRESS")
    await tap(KEY_ENTER)
    await tap(KEY_H)
    for i in 4:
        if board().retreat.active: break
        await tap(KEY_ENTER)
    for i in 180:
        if board().retreat.revealed: break
        await frames()
    check(board().retreat.active and board().retreat.phase == "Text", "paid retreat message")
    snapshot("EGRESS paid text")
    await tap(KEY_ENTER)
    await arm()
    for i in 500:
        if not view.is_visible_in_tree(): break
        await frames()
    check(cue_finished == 1 and markers.get("cuePlayingAtClose") == false and markers.get("cuePitch") == 1, "natural unpitched cue completes before close")
    check(markers.get("cueFinishedUsec",0)-markers.get("effectStartUsec",0) > 300000, "audio was not mechanically accelerated")
    for i in 800:
        field = host.find_child("ExplorationSessionView", true, false)
        if not field: await frames(); continue
        var f: Dictionary = JSON.parse_string(field.call("ReadObservationJson"))
        if f.canWaitAtInput: break
        await field_input(f)
    var returned := field_snapshot("same-session field return")
    check(not returned.is_empty() and returned.sessionId == initial.sessionId and returned.map == "map-3", "actual bounded field battle field continuity")
    check(returned.acceleration.rate == 1, "battle to field clears rate")
    if shift: await key(KEY_SHIFT, false)
    await tap(KEY_DOWN)
    await create_timer(0.25).timeout
    var moved := field_snapshot("fresh field input")
    check(samples[-1].entities[0].y != samples[-2].entities[0].y, "return is playable")
    check(int(counts.get("retreat-mp-paid",0)) == 1 and int(counts.get("retreat-exploration-entry",0)) == 1, "one payment and one field handoff")

func round_program() -> void:
    if not await ready(): return
    snapshot("before finite round")
    await arm()
    var steps := 2 if case_name == "story" else 1
    for i in steps:
        await tap(KEY_UP)
        if not await ready(): return
    await tap(KEY_ENTER)
    await tap(KEY_SPACE)
    await tap(KEY_ENTER)
    if not await settle_selection(): return
    var s := snapshot("after finite round")
    check(s.round == 2, "one round generated after finite consumers")
    if case_name == "story":
        check(markers.has("storyWaitEndUsec"), "admitted60tick wait completed")
        check(int(counts.get("region-program-started",0)) == 1 and 386.0 in s.storyFlags, "one converted story before queue")
    else:
        check(int(counts.get("appearance-completed",0)) == 2, "two ordered appearances delivered")
        await tap(KEY_ENTER)
        await tap(KEY_F)
        await tap(KEY_TAB)
        await tap(KEY_ENTER)
        await arm()
        if not await settle_selection(): return
        snapshot("after real death")
        check(int(counts.get("death-cleanup",0)) == 1 and int(counts.get("field-death-ended",0)) == 1, "one death and finite field exit")

func run() -> void:
    if OS.get_environment("SF2_BATTLE_RATE_OUTPUT").is_empty():
        finish("explicit output required")
        return
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(3)
    if case_name == "private-egress":
        var started = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_BATTLE_RATE_START"))
        markers.controlledStart = started
        check(started == "started", "validated controlled full-world field start")
    view = host.find_child("BattleSessionView", true, false)
    if not view:
        markers.startupLabels = []
        for label in host.find_children("*", "Label", true, false):
            if not label.text.is_empty(): markers.startupLabels.append({"name":str(label.name),"text":label.text})
        finish("battle did not mount")
        return
    if case_name == "private-egress":
        await private_egress()
        finish()
        return
    last_sequence = int(board().observationSequence)
    view.connect("SessionResultObserved", observed)
    if case_name == "heal": await heal()
    elif case_name == "movement": await movement()
    elif case_name == "reading": await reading()
    elif case_name in ["story", "appearance"]: await round_program()
    else: check(false, "unknown bounded case")
    finish()

func finish(reason := "") -> void:
    if ended: return
    ended = true
    if not reason.is_empty(): failures.append(reason)
    var report := {"case":case_name, "factor":factor, "passed":failures.is_empty(), "failures":failures, "elapsedUsec":Time.get_ticks_usec()-began, "counts":counts, "work":work, "phases":phases, "markers":markers, "movement":movement_marks, "samples":samples}
    var bytes := JSON.stringify(report, "  ").to_utf8_buffer()
    if bytes.size() > 100*1024:
        failures.append("100 KiB report budget")
        bytes = JSON.stringify({"passed":false,"failures":failures}).to_utf8_buffer()
    var path := OS.get_environment("SF2_BATTLE_RATE_OUTPUT")
    if not path.is_empty():
        var file := FileAccess.open(path, FileAccess.WRITE)
        file.store_buffer(bytes)
        file.close()
    quit(0 if failures.is_empty() else 1)
