extends "res://probes/engine_equipment_observation.gd"

# One controlled Battle04 cast through the production menu and scene.
var frames_seen: Dictionary = {}
var phases: Dictionary = {}
var reduced := OS.get_environment("SF2_BLAZE_REDUCED_FLASH") == "1"
var skip := OS.get_environment("SF2_BLAZE_SKIP_MESSAGES") == "1"
const CASTER := "ally-4"
const TARGET := "enemy-1"

func actor(state: Dictionary, id: String) -> Dictionary:
    for row in state.actors:
        if row.id == id: return row
    return {}

func settle() -> Dictionary:
    for step in range(1800):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "battle or scene error: " + str([current.failure, s.error]))
            return {}
        if current.hasBattleControl and s.phase == null and current.movement == null:
            var state := board()
            if state.stage == "Movement" and state.stopReason == "PlayerInput": return state
        if s.spell != null and s.spell.Value == "blaze":
            if not phases.has(s.phase):
                phases[s.phase] = {"token":s.waitToken, "suppressed":s.suppressed, "target":s.displayedEnemy,
                    "message":s.message, "amount":s.reactionAmount}
                check(s.displayedAlly == CASTER and s.displayedEnemy == TARGET, "current caster and recipient")
            var effect = s.attackSpell
            if s.phase == "ActionAnimation" and effect != null and effect.preparedBlaze != null:
                var flame: Dictionary = effect.preparedBlaze
                if flame.frame != null and flame.opacity > 0 and not frames_seen.has(flame.frame):
                    check(s.allyVisible and s.enemyVisible and s.weaponVisible and s.backgroundVisible and s.groundVisible,
                        "caster, target, weapon, background and ground coexist with the flame")
                    check(s.allyResource == "ally4-palette0/frame3" and s.animationIndex == 4 and flame.x == 72 and flame.y == 112,
                        "Kazin cast pose and opponent-side flame anchor")
                    check(effect.selector == 1 and effect.target == TARGET and flame.reducedFlash == reduced,
                        "BLAZE1 source selector, recipient and flash setting")
                    frames_seen[flame.frame] = {"elapsed":s.elapsed, "drawCount":effect.drawCount,
                        "ally":s.allyResource, "enemy":s.enemyResource, "weapon":s.weaponResource,
                        "background":s.background, "ground":s.ground, "flame":flame}
                    if frames_seen.size() == 1:
                        await key(KEY_ENTER)
                        check(scene().scene.waitToken == s.waitToken, "early Confirm preserves the animation wait")
        if s.requiresAcknowledgement == true and not s.suppressed:
            await key(KEY_ENTER)
    check(false, "finite spell observation deadline")
    return {}

func run() -> void:
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_BLAZE_START"))
    check(admitted == "started", "controlled private battle admission: " + str(admitted))
    view = host.find_child("BattleSessionView", true, false)
    check(view != null, "production battle view is mounted")
    if admitted != "started" or view == null: finish(); return
    view.connect("SessionResultObserved", result_observed)
    var initial := await settle()
    for turn in range(4):
        if initial.is_empty() or initial.actor == CASTER: break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        initial = await settle()
    if initial.is_empty() or initial.actor != CASTER:
        check(false, "ordinary Kazin turn reached")
        finish(); return
    var caster := actor(initial, CASTER)
    var victim := actor(initial, TARGET)
    await key(KEY_ENTER)
    await key(KEY_H)
    await key(KEY_ENTER)
    var selected := board()
    if selected.stage != "CommitReady" or selected.target != TARGET:
        check(false, "ordinary Magic selects prepared recipient: " + str([selected.stage, selected.target, selected.menu]))
        finish(); return
    check(victim.hp == 1 and actor(selected, CASTER).mp == caster.mp, "staged HP and no pre-confirm MP charge")
    await key(KEY_ENTER)
    var next := await settle()
    if next.is_empty(): finish(); return
    var after := actor(next, CASTER)
    check(frames_seen.size() == (1 if reduced else 2) and frames_seen.has("blaze-1/frame-0"), "actual prepared flame frames")
    check(after.mp == caster.mp - 2 and after.kills == caster.kills + 1 and after.exp > caster.exp,
        "one MP payment, kill and EXP")
    check(actor(next, TARGET).hp == 0 and next.gold > initial.gold, "ordinary death and gold award")
    check(events.filter(func(e): return e.Kind == "mp").size() == 1 and
        events.filter(func(e): return e.Kind == "exp").size() == 1, "one MP and EXP event")
    check(phases.has("ResultMessage") and not phases.ResultMessage.suppressed, "spell damage remains readable")
    var coin_wait: bool = phases.has("GoldMessage") and not phases.GoldMessage.suppressed
    check(coin_wait != skip, "only the eligible coin wait follows skip")
    check(next.sessionId == initial.sessionId, "same-session next player control")
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio != null and audio.error == null, "scene audio available: " + str(null if audio == null else audio.error))
    var cast_audio: Array = [] if audio == null else audio.receipts.filter(func(r): return r.Command == 77 and r.Operation == "started")
    check(cast_audio.size() == 1, "one private cast cue starts")
    samples.append({"mpBefore":caster.mp, "mpAfter":after.mp, "exp":after.exp, "kills":after.kills,
        "goldBefore":initial.gold, "goldAfter":next.gold, "nextActor":next.actor, "castAudio":cast_audio})
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "new ordinary action menu opens after the cast")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "reducedFlash":reduced, "skipMessages":skip, "frames":frames_seen, "phases":phases,
        "samples":samples, "events":events, "elapsedMs":Time.get_ticks_msec() - started}, "  "))
    output.close()
    host.free()
    quit(0 if failures.is_empty() else 1)
