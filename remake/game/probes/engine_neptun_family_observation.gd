extends "res://probes/engine_equipment_observation.gd"

# Bounded ordinary-input neptun observation; store transitions and distinct graphics only.
var multi := OS.get_environment("SF2_NEPTUN_MULTI") == "1"
var caster_id := OS.get_environment("SF2_NEPTUN_CASTER")
var spell_name := OS.get_environment("SF2_NEPTUN_SPELL")
var rank_ := int(OS.get_environment("SF2_NEPTUN_RANK"))
var target_id := OS.get_environment("SF2_NEPTUN_TARGET")
var rate_requested := false
var variation := int(OS.get_environment("SF2_NEPTUN_VARIATION"))
var expected_hd := OS.get_environment("SF2_NEPTUN_HD") == "1"
var fast := OS.get_environment("SF2_NEPTUN_FAST") == "1"
var frames_seen := {}
var results := []
var expected_reduced := OS.get_environment("SF2_NEPTUN_REDUCED") == "1"
var draw_seen := false
var cast_seen := false
var cast_index = null
var rate_seen := false
var rise_min := 1000.0
var rise_max := -1000.0
var rain_seen := false

func hold_rate(pressed: bool) -> void:
    var event := InputEventKey.new()
    event.keycode = KEY_SHIFT
    event.location = KEY_LOCATION_LEFT
    event.shift_pressed = pressed
    event.pressed = pressed
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func settle() -> Dictionary:
    for _frame in range(12000):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native error: " + str(current))
            return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.phase == null: continue
        var relevant: bool = s.spell != null and s.spell.Value == spell_name and s.spell.Level == rank_
        if not relevant:
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        check(s.spellAnimationSelector == (19+32*(variation-1)), "action selector")
        if s.phase in ["Reward", "End"]:
            check(s.neptun == null and not s.daoDistortion and s.allyVisible, "summon end restores caster and target")
        if s.phase == "ActionAnimation":
            cast_index = s.animationIndex
            if fast and not rate_requested:
                rate_requested = true
                hold_rate(true)
            check(s.backgroundVisible and s.enemyVisible, "mounted target and scenery")
        if s.neptun != null:
            var effect: Dictionary = s.neptun
            draw_seen = draw_seen or effect.drawCount > 0
            check(effect.variation == variation, "source variation and side")
            check(effect.reducedFlash == expected_reduced, "Reduced Flash reaches effect")
            if effect.replacesCaster: check(not s.allyVisible and not s.weaponVisible and not s.groundVisible, "invocation replaces caster body/weapon/ground")
            var rain_top: Array = effect.sprites.filter(func(sprite): return sprite.resource == "neptun/rain-top")
            if not rain_top.is_empty():
                rain_seen = true
                var rain_bottom: Array = effect.sprites.filter(func(sprite): return sprite.resource == "neptun/rain-bottom")
                check(rain_top.size() == 19 and rain_bottom.size() == 19, "separate nineteen-part rain layers")
                check(not effect.sprites.any(func(sprite): return sprite.resource.begins_with("neptun/summon-")), "body hidden before rain")
                for i in range(min(rain_top.size(), rain_bottom.size())):
                    check(rain_bottom[i].x == rain_top[i].x and rain_bottom[i].y == rain_top[i].y + 32, "paired rain geometry")
            for sprite in effect.sprites:
                if sprite.resource.begins_with("neptun/rise-"):
                    rise_min = min(rise_min, sprite.y)
                    rise_max = max(rise_max, sprite.y)
                check(sprite.hd == expected_hd, "atomic graphics group")
                check(sprite.textureWidth == sprite.width * (4 if expected_hd else 1), "actual texture density")
                frames_seen[sprite.resource] = sprite

            if not cast_seen and not effect.sprites.is_empty():
                cast_seen = true
                samples.append({"mounted":s})
                if s.phase == "ActionAnimation" and not fast:
                    var token = s.waitToken
                    await key(KEY_ENTER)
                    check(scene().scene.waitToken == token, "early Confirm retains animation")
            if fast and current.pacing.input.rate == 1.5: rate_seen = true
        if not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "ResultMessage": results.append({"target":s.displayedEnemy,"kind":s.reactionKind,"amount":s.reactionAmount})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    samples.append({"timeoutScene":scene(),"timeoutBoard":board(),"focus":root.has_focus(),"viewVisible":view.is_visible_in_tree(),"viewProcess":view.can_process(),"labels":host.find_children("*", "Label", true, false).map(func(n):return n.text)})
    check(false, "bounded return to control")
    return {}

func boot() -> bool:
    for _frame in range(2400):
        OS.delay_msec(10)
        await process_frame
        view = host.find_child("BattleSessionView", true, false)
        if view != null: return true
        var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
        if opening.active: await key(KEY_E); continue
        var entry := host.find_child("SessionEntryView", true, false)
        var panel := host.find_child("SaveSlotView", true, false)
        if panel != null:
            var save: Dictionary = JSON.parse_string(panel.call("ReadSaveObservationJson"))
            if save.get("state") == "entry-loading": continue
            if save.get("state") == "continue-slots":
                samples.append({"slotSelection":save})
                await key(KEY_ENTER)
                continue
        if entry != null and entry.is_visible_in_tree():
            var e: Dictionary = JSON.parse_string(entry.call("ReadEntryObservationJson"))
            if _frame % 300 == 0: samples.append({"entry":e,"preparation":JSON.parse_string(host.call("ReadEntryPreparationObservationJson"))})
            if e.selectedAction != null and e.selectedAction != "Continue": await key(KEY_DOWN)
            elif e.page >= 0 and e.continueText == null: await key(KEY_E)
            else: await key(KEY_ENTER)
    var slots := host.find_child("SaveSlotView", true, false)
    if slots != null: samples.append({"save":JSON.parse_string(slots.call("ReadSaveObservationJson"))})
    samples.append({"startup":host.find_children("*", "Label", true, false).map(func(node): return node.text)})
    check(false, "ordinary Continue admits controlled record")
    return false

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    if not await boot(): finish(); return
    view.connect("SessionResultObserved", result_observed)
    var before := await settle()
    check(not before.is_empty() and before.actor == caster_id, "controlled holder turn")
    if not failures.is_empty(): finish(); return
    samples.append({"before":before})
    await key(KEY_ENTER)
    await key(KEY_H)
    for _row in range(18):
        await key(KEY_ENTER)
        var b := board()
        if b.spell != null and b.spell.Value == spell_name and b.spell.Level == rank_: break
        await key(KEY_ESCAPE)
        await key(KEY_DOWN)
    for _target in range(40):
        if board().target == target_id: break
        await key(KEY_TAB)
    var selected := board()
    samples.append({"selected":selected})
    check(selected.stage == "CommitReady" and selected.target == target_id, "ordinary target selection")
    if not failures.is_empty(): finish(); return
    events.clear()
    await key(KEY_ENTER)
    var after := await settle()
    if fast: hold_rate(false)
    samples.append({"after":after})
    await verify_return(after)
    finish()

func verify_return(after: Dictionary) -> void:
    if after.is_empty(): return
    var before: Dictionary = samples.filter(func(row): return row.has("before"))[0].before
    var a: Dictionary = before.actors.filter(func(row): return row.id == caster_id)[0]
    var b: Dictionary = after.actors.filter(func(row): return row.id == caster_id)[0]
    if fast: check(rate_seen, "held acceleration actually applied")
    check(cast_seen and draw_seen and frames_seen.has("neptun/summon-0"), "mounted final summon frame")
    for frame in range(2): check(frames_seen.has("neptun/summon-"+str(frame)), "source summon pose " + str(frame))
    for kind in ["rise", "sizzle"]:
        for frame in range(3): check(frames_seen.has("neptun/"+kind+"-"+str(frame)), "source water " + kind + str(frame))
    for kind in ["rain-top", "rain-bottom"]: check(frames_seen.has("neptun/"+kind), "rain layer " + kind)
    check(rain_seen and rise_max == 64 and rise_min <= -16, "water rise and rain transition")
    var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id)
    check(payments.size() == 1 and a.mp-b.mp == (12 if rank_ == 1 else 19), "one Magic payment")
    var hits := results.filter(func(result): return result.kind == "Damage")
    var targets := hits.map(func(result): return result.target)
    check(targets == (["enemy-1", "enemy-2"] if multi else ["enemy-1"]), "ordered actual recipients")
    var hp_events := events.filter(func(e): return e.Kind == "hp" and targets.has(e.Actor.Value))
    check(hp_events.map(func(e): return e.Actor.Value) == targets, "one ordered HP application per recipient")
    for result in hits:
        var initial: Dictionary = before.actors.filter(func(row): return row.id == result.target)[0]
        var final: Dictionary = after.actors.filter(func(row): return row.id == result.target)[0]
        check(final.hp == max(0, initial.hp-result.amount), "HP authority")
        check(events.filter(func(e): return e.Kind == "death-cleanup" and e.Actor.Value == result.target).size() == (1 if final.hp == 0 else 0), "death cleanup")
    check(events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == caster_id).size() == 1, "one action reward")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null, "audio error-free")
    for cue in [77,108,110,115]: check(audio.receipts.any(func(e): return e.Command == cue and e.Operation == "started"), "summon cue " + str(cue))
    samples.append({"results":results,"frames":frames_seen,"animationIndex":cast_index,"audio":audio,"rateSeen":rate_seen,"riseMin":rise_min,"riseMax":rise_max,"rainSeen":rain_seen})
    for _ready in range(10): await process_frame
    await key(KEY_ENTER)
    for _ready in range(10): await process_frame
    check(board().stage == "ActionChoice", "usable next action menu")

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
        "scope":"controlled neptun ordinary input and mounted frames; no natural reach or original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
