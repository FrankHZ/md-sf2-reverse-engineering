extends "res://probes/engine_equipment_observation.gd"

# Prepared private wind cast through the ordinary UI and scene completion.
const CASTER = "ally-1"
const TARGET = "enemy-1"
var animation_token = null
var effect_draws := 0
var reaction_count := 0
var result_count := 0
var frames_seen: Dictionary = {}
var last_age := 0.0
var held := false
var rate_seen := false
var coin_seen := false
var reduced := OS.get_environment("SF2_BLAST_REDUCED_FLASH") == "1"
var fast := OS.get_environment("SF2_BLAST_HELD_RATE") == "1"
var skip := OS.get_environment("SF2_BLAST_SKIP_MESSAGES") == "1"

func hold_rate(pressed: bool) -> void:
    held = pressed
    var event := InputEventKey.new()
    event.keycode = KEY_SHIFT
    event.location = KEY_LOCATION_LEFT
    event.shift_pressed = pressed
    event.pressed = pressed
    Input.parse_input_event(event)
    Input.flush_buffered_events()


func actor(state: Dictionary, id: String) -> Dictionary:
    for row in state.actors:
        if row.id == id: return row
    return {}

func settle() -> Dictionary:
    for frame in range(1800):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "battle or scene error: " + str([current.failure, s.error]))
            return {}
        if current.hasBattleControl and s.phase == null and current.movement == null:
            var state := board()
            if state.stage == "Movement" and state.stopReason == "PlayerInput": return state
        if held and s.phase != "ActionAnimation": hold_rate(false)
        if s.phase == "GoldMessage" and not s.suppressed: coin_seen = true
        if s.spell != null and s.spell.Value == "blast":
            if s.phase == "ActionAnimation" and s.attackSpell != null:
                effect_draws = maxi(effect_draws, int(s.attackSpell.drawCount))
                var prepared = s.attackSpell.preparedBlast
                check(prepared != null, "prepared BLAST mount")
                if prepared == null: return {}
                if not prepared.sprites.is_empty():
                    last_age = s.attackSpell.elapsed
                    for sprite in prepared.sprites: frames_seen[sprite.resource] = sprite
                rate_seen = rate_seen or current.pacing.input.rate == 1.5
                if animation_token == null and not prepared.sprites.is_empty():
                    check(s.allyVisible and s.enemyVisible and s.weaponVisible and s.backgroundVisible and s.groundVisible,
                        "caster, recipient, weapon, backdrop coexist with prepared wind")
                    check(prepared.reducedFlash == reduced and not prepared.targetIsAlly, "allied cast branch and flash setting")
                    animation_token = s.waitToken
                    check(s.attackSpell.selector == 5 and s.attackSpell.variation == 1 and
                        s.attackSpell.targets == [TARGET] and s.attackSpell.element == "Wind",
                        "private rank-1 source selector and single target")
                    samples.append({"phase":s.phase, "token":s.waitToken, "effect":s.attackSpell,
                        "allyVisible":s.allyVisible, "enemyVisible":s.enemyVisible,
                        "weaponVisible":s.weaponVisible, "backgroundVisible":s.backgroundVisible,
                        "groundVisible":s.groundVisible})
                    await key(KEY_ENTER)
                    check(scene().scene.waitToken == animation_token, "early Confirm retains animation token")
                    if fast: hold_rate(true)
            if s.waitToken != null and not seen_tokens.has(s.waitToken):
                seen_tokens[s.waitToken] = true
                if s.phase == "Reaction":
                    reaction_count += 1
                    check(s.displayedEnemy == TARGET and s.reactionKind == "Damage", "damage reaction targets the selected enemy")
                if s.phase == "ResultMessage":
                    result_count += 1
                    check(not s.suppressed and s.reactionAmount > 0, "damage result remains readable")
                    samples.append({"phase":s.phase, "amount":s.reactionAmount})
        if s.requiresAcknowledgement == true and not s.suppressed:
            await key(KEY_ENTER)
    samples.append({"timeoutScene":scene(), "timeoutBoard":board(), "audio":JSON.parse_string(host.call("ReadAudioObservationJson"))})
    check(false, "finite spell observation deadline")
    return {}

func run() -> void:
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_BLAST_START"))
    view = host.find_child("BattleSessionView", true, false)
    check(admitted == "started" and view != null, "private battle admission: " + str(admitted))
    if admitted != "started" or view == null: finish(); return
    view.connect("SessionResultObserved", result_observed)
    var initial := await settle()
    for turn in range(4):
        if initial.is_empty() or initial.actor == CASTER: break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        initial = await settle()
    if initial.is_empty() or initial.actor != CASTER:
        check(false, "ordinary priest turn reached")
        finish(); return
    var caster := actor(initial, CASTER)
    check(caster.level == 10 and actor(initial, TARGET).hp == 1, "controlled level and target HP")
    await key(KEY_ENTER)
    await key(KEY_H)
    for row in range(3): await key(KEY_DOWN)
    await key(KEY_ENTER)
    var selected := board()
    if selected.stage != "CommitReady" or selected.target != TARGET or selected.spell == null:
        check(false, "ordinary Magic selection: " + str([selected.stage, selected.target, selected.spell]))
        finish(); return
    check(selected.spell.Value == "blast" and int(selected.spell.Level) == 1 and
        actor(selected, CASTER).mp == caster.mp, "rank 1 selected before payment")
    await key(KEY_ENTER)
    var next := await settle()
    if next.is_empty(): finish(); return
    var after := actor(next, CASTER)
    check(last_age > 1.35, "completion token covers full prepared duration")
    check(frames_seen.size() == (5 if reduced else 15), "actual prepared particle and slash frames")
    check(not fast or rate_seen, "held rate reached 1.5x")
    check(coin_seen != skip, "coin skip selection")
    var audio = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null and audio.receipts.filter(func(r): return r.Command == 77 and r.Operation == "started").size() == 1,
        "one cast cue")
    check(audio.receipts.filter(func(r): return r.Command == 114 and r.Operation == "started").size() == 1, "one wind cue")
    samples.append({"frames":frames_seen, "lastAge":last_age, "rateSeen":rate_seen, "audio":audio})
    check(effect_draws > 0 and reaction_count == 1 and result_count == 1, "draw and one damage result")
    check(after.mp == caster.mp - 2 and after.exp > caster.exp and after.kills == caster.kills + 1,
        "one 2 MP payment, EXP and kill")
    check(actor(next, TARGET).hp == 0 and next.gold > initial.gold, "ordinary death and gold")
    check(events.filter(func(e): return e.Kind == "mp").size() == 1 and
        events.filter(func(e): return e.Kind == "exp").size() == 1, "one MP and EXP publication")
    check(next.sessionId == initial.sessionId, "same-session control")
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "ordinary action menu opens after the cast")
    samples.append({"mpBefore":caster.mp, "mpAfter":after.mp, "exp":after.exp, "kills":after.kills,
        "goldBefore":initial.gold, "goldAfter":next.gold, "nextActor":next.actor})
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"prepared private BLAST1, ordinary input, modern scheduling",
        "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
