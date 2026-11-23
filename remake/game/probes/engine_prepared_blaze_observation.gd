extends "res://probes/engine_equipment_observation.gd"

# One controlled cast through the production scene, from Magic or enemy AI.
var frames_seen: Dictionary = {}
var flights: Dictionary = {}
var impacts: Dictionary = {}
var reactions: Array = []
var animation_token = null
var first_projection: Dictionary = {}
var last_effect_age := 0.0
var effect_draw_count := 0
var rate_seen := false
var shift := false
var rank_two := OS.get_environment("SF2_BLAZE_RANK") == "2"
var held_rate := OS.get_environment("SF2_BLAZE_HELD_RATE") == "1"
const SECOND_TARGET = "enemy-2"
var phases: Dictionary = {}
var reduced := OS.get_environment("SF2_BLAZE_REDUCED_FLASH") == "1"
var skip := OS.get_environment("SF2_BLAZE_SKIP_MESSAGES") == "1"
var enemy_cast := OS.get_environment("SF2_BLAZE_ENEMY") == "1"
var caster_id := "enemy-2" if enemy_cast else "ally-4"
var target_id := "ally-0" if enemy_cast else "enemy-1"

func actor(state: Dictionary, id: String) -> Dictionary:
    for row in state.actors:
        if row.id == id: return row
    return {}

func hold_rate(pressed: bool) -> void:
    shift = pressed
    var event := InputEventKey.new()
    event.keycode = KEY_SHIFT
    event.location = KEY_LOCATION_LEFT
    event.shift_pressed = pressed
    event.pressed = pressed
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func observe_rank_two(s: Dictionary) -> void:
    var effect: Dictionary = s.attackSpell
    var prepared = effect.preparedBlazeTwo
    if prepared == null or prepared.sprites.is_empty(): return
    last_effect_age = effect.elapsed
    effect_draw_count = int(effect.drawCount)
    for sprite in prepared.sprites:
        if sprite.opacity <= 0: continue
        if not frames_seen.has(sprite.resource): frames_seen[sprite.resource] = sprite
        if sprite.meteor != null:
            if "/meteor-" in sprite.resource:
                if not flights.has(int(sprite.meteor)):
                    check(sprite.flip and sprite.width == 16 and sprite.height == 16, "leftward small meteor raster")
                flights[int(sprite.meteor)] = true
            else:
                impacts[int(sprite.meteor)] = true
    if animation_token != null: return
    check(s.allyVisible and s.enemyVisible and s.weaponVisible and s.backgroundVisible and s.groundVisible,
        "caster, first recipient, weapon, background and ground remain visible")
    check(effect.selector == 33 and effect.variation == 2 and effect.target == target_id and
        effect.targets == [target_id, SECOND_TARGET], "source BLAZE2 selector and ordered recipients")
    check(s.allyResource == "ally4-palette0/frame3" and s.animationIndex == 4 and not prepared.targetIsAlly and
        prepared.reducedFlash == reduced, "Kazin's ordinary pose and opponent-side composition")
    animation_token = s.waitToken
    first_projection = {"ally":s.allyResource, "enemy":s.enemyResource, "weapon":s.weaponResource,
        "background":s.background, "ground":s.ground, "duration":prepared.duration}
    await key(KEY_ENTER)
    check(scene().scene.waitToken == animation_token, "early Confirm preserves the BLAZE2 animation token")
    if held_rate: hold_rate(true)

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
        if shift and s.phase != "ActionAnimation": hold_rate(false)
        if s.spell != null and s.spell.Value == "blaze":
            if not phases.has(s.phase):
                phases[s.phase] = {"token":s.waitToken, "suppressed":s.suppressed, "target":s.displayedEnemy,
                    "message":s.message, "amount":s.reactionAmount}
                if not rank_two:
                    check(s.displayedAlly == (target_id if enemy_cast else caster_id) and
                        s.displayedEnemy == (caster_id if enemy_cast else target_id), "current caster and recipient")
            if rank_two and s.phase == "Reaction" and (reactions.is_empty() or reactions.back().token != s.waitToken):
                check(s.displayedAlly == caster_id and s.allyVisible and s.enemyVisible and s.backgroundVisible and
                    s.groundVisible and s.attackSpell.preparedBlazeTwo == null, "live reaction follows the current recipient")
                reactions.append({"token":s.waitToken, "target":s.displayedEnemy, "raster":s.enemyResource,
                    "amount":s.reactionAmount})
            if rank_two and s.phase == "ActionAnimation" and s.attackSpell != null:
                rate_seen = rate_seen or current.pacing.input.rate == 1.5
                await observe_rank_two(s)
            var effect = s.attackSpell
            if s.phase == "ActionAnimation" and effect != null and effect.preparedBlaze != null:
                var flame: Dictionary = effect.preparedBlaze
                if flame.frame != null and flame.opacity > 0 and not frames_seen.has(flame.frame):
                    check(s.allyVisible and s.enemyVisible and s.weaponVisible and s.backgroundVisible and s.groundVisible,
                        "caster, target, weapon, background and ground coexist with the flame")
                    if enemy_cast:
                        check(s.enemyResource == "enemy36/frame3" and s.animationIndex == 36 and flame.x == 176 and flame.y == 128,
                            "Galam Mage ordinary casting pose and ally-side flame anchor")
                    else:
                        check(s.allyResource == "ally4-palette0/frame3" and s.animationIndex == 4 and flame.x == 72 and flame.y == 112,
                            "Kazin cast pose and opponent-side flame anchor")
                    check(effect.selector == 1 and effect.target == target_id and flame.reducedFlash == reduced,
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
    if enemy_cast:
        if not initial.is_empty(): await observe_enemy_cast(initial)
        finish(); return
    for turn in range(4):
        if initial.is_empty() or initial.actor == caster_id: break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        initial = await settle()
    if initial.is_empty() or initial.actor != caster_id:
        check(false, "ordinary Kazin turn reached")
        finish(); return
    var caster := actor(initial, caster_id)
    var victim := actor(initial, target_id)
    await key(KEY_ENTER)
    await key(KEY_H)
    if rank_two: await key(KEY_DOWN)
    await key(KEY_ENTER)
    var selected := board()
    if selected.stage != "CommitReady" or selected.target != target_id:
        check(false, "ordinary Magic selects prepared recipient: " + str([selected.stage, selected.target, selected.menu]))
        finish(); return
    check(selected.spell.Value == "blaze" and int(selected.spell.Level) == (2 if rank_two else 1),
        "ordinary selected BLAZE rank")
    check(victim.hp == 1 and actor(selected, caster_id).mp == caster.mp, "staged HP and no pre-confirm MP charge")
    await key(KEY_ENTER)
    var next := await settle()
    if next.is_empty(): finish(); return
    var after := actor(next, caster_id)
    if rank_two:
        check_rank_two(initial, next)
    else:
        check(frames_seen.size() == (1 if reduced else 2) and frames_seen.has("blaze-1/frame-0"), "actual prepared flame frames")
    check(after.mp == caster.mp - (6 if rank_two else 2) and after.kills == caster.kills + 1 and after.exp > caster.exp,
        "one MP payment, kill and EXP")
    check(actor(next, target_id).hp == 0 and next.gold > initial.gold, "ordinary death and gold award")
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

func check_rank_two(initial: Dictionary, next: Dictionary) -> void:
    check(frames_seen.size() == (3 if reduced else 6), "composed flame, falling meteors and impacts use the prepared frames")
    for kind in ["flame", "meteor", "impact"]:
        check(frames_seen.has("blaze-2/" + kind + "-0") and
            frames_seen.has("blaze-2/" + kind + "-1") != reduced, kind + " follows Reduced Flash")
    var flame: Dictionary = frames_seen.get("blaze-2/flame-0", {})
    check(flame.get("x") == 68 and flame.get("y") == 96 and flame.get("width") == 24 and flame.get("height") == 48,
        "medium source flame occupies both stacked sprite rectangles")
    check(effect_draw_count > 0, "prepared BLAZE2 draw callback ran")
    check(flights.size() == 10 and impacts.size() == 10, "all ten meteors fall and reach impact")
    check(last_effect_age > first_projection.get("duration", INF) - 0.05, "animation token covers the final impact")
    check(rate_seen == held_rate and next.pacing.input.rate == 1, "held acceleration stays in playback and releases")
    var hits := events.filter(func(e): return e.Kind == "hp")
    var commits := events.filter(func(e): return e.Kind == "action-committed" and e.Actor.Value == caster_id)
    check(reactions.map(func(r): return r.target) == [target_id, SECOND_TARGET] and
        reactions.map(func(r): return r.raster).size() == 2 and reactions[0].raster != reactions[1].raster,
        "two ordered reactions show distinct recipient art")
    check(hits.map(func(e): return e.Actor.Value) == [target_id, SECOND_TARGET] and commits.size() == 1,
        "one action commits each recipient's damage once and in order")
    var before := actor(initial, SECOND_TARGET)
    var after := actor(next, SECOND_TARGET)
    check(after.hp > 0 and after.hp < before.hp, "second recipient survives the area hit")
    samples.append({"firstProjection":first_projection, "flights":flights.size(), "impacts":impacts.size(),
        "lastEffectAge":last_effect_age, "drawCount":effect_draw_count, "heldRateSeen":rate_seen, "reactions":reactions, "hits":hits})

func observe_enemy_cast(initial: Dictionary) -> void:
    var next := initial
    if phases.is_empty():
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        next = await settle()
        if next.is_empty(): return
    check(frames_seen.has("blaze-1/frame-0") and frames_seen.has("blaze-1/frame-1"), "both prepared flame frames on the ally side")
    check(phases.has("ResultMessage") and phases.ResultMessage.amount > 0, "real hostile spell result")
    check(next.actor == target_id and next.sessionId == initial.sessionId, "Bowie receives control in the same session")
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "ordinary action menu opens without redispatching the mage")
    var casts := events.filter(func(e): return e.Kind == "attack-spell" and e.Actor.Value == caster_id and e.Target.Value == target_id)
    var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id)
    var hits := events.filter(func(e): return e.Kind == "hp" and e.Actor.Value == target_id)
    var turns := events.filter(func(e): return e.Kind == "action-committed" and e.Actor.Value == caster_id)
    check(casts.size() == 1 and payments.size() == 1 and hits.size() == 1 and turns.size() == 1,
        "one enemy cast, MP payment, damage, and turn commit")
    if payments.size() == 1 and hits.size() == 1:
        check(payments[0].Before - payments[0].After == 2 and payments[0].After == actor(next, caster_id).mp,
            "exactly two MP spent by the Galam Mage")
        check(hits[0].Before - hits[0].After == phases.get("ResultMessage", {}).get("amount") and
            hits[0].After == actor(next, target_id).hp and hits[0].After > 0, "one surviving target receives reported damage")
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio != null and audio.error == null, "scene audio available: " + str(null if audio == null else audio.error))
    var cast_audio: Array = [] if audio == null else audio.receipts.filter(func(r): return r.Command == 77 and r.Operation == "started")
    check(cast_audio.size() == 1, "one private cast cue starts")
    samples.append({"nextActor":next.actor, "payments":payments, "hits":hits, "castAudio":cast_audio})

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "enemyCast":enemy_cast, "rank":2 if rank_two else 1, "reducedFlash":reduced,
        "skipMessages":skip, "heldRate":held_rate, "frames":frames_seen, "phases":phases,
        "samples":samples, "events":events, "elapsedMs":Time.get_ticks_msec() - started}, "  "))
    output.close()
    host.free()
    quit(0 if failures.is_empty() else 1)
