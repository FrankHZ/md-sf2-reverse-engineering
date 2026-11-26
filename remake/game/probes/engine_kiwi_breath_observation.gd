extends "res://probes/engine_equipment_observation.gd"

# Accepted controlled Monster record; ordinary Continue and physical Attack input.
var rank_ := int(OS.get_environment("SF2_KIWI_RANK"))
var expected_hd := OS.get_environment("SF2_KIWI_HD") == "1"
var reduced := OS.get_environment("SF2_KIWI_REDUCED") == "1"
var fast := OS.get_environment("SF2_KIWI_FAST") == "1"
var frames_seen := {}
var results := []
var draw_seen := false
var rate_seen := false
var attempted := false

func settle() -> Dictionary:
    for _frame in range(3000):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            samples.append({"failure":current}); check(false,"native failure"); return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.phase == null: continue
        if s.spell != null and s.spell.Value == "kiwi":
            check(s.spell.Level == rank_ and s.spellAnimationSelector == 76,"rank/selector")
            if s.attackSpell != null and s.attackSpell.preparedKiwi != null:
                var effect: Dictionary = s.attackSpell.preparedKiwi
                check(effect.casterOnRight and not effect.targetIsAlly and effect.variation == 3,"actual ally side")
                check(effect.reducedFlash == reduced,"Reduced Flash binding")
                check(s.allyVisible and s.enemyVisible and s.backgroundVisible,"real bodies and terrain")
                check(s.allyResource.begins_with("equipment/ally27-palette0/"),"MNST body resource")
                check(s.animationIndex==84 and s.monsterBreath,"MNST dedicated pose")
                draw_seen = draw_seen or s.attackSpell.drawCount > 0
                for sprite in effect.sprites:
                    check(sprite.hd == expected_hd,"atomic HD selection")
                    check(sprite.textureWidth == sprite.width*(4 if expected_hd else 1),"texture density")
                    frames_seen[sprite.resource] = sprite
                if not attempted and not effect.sprites.is_empty():
                    attempted = true
                    var token = s.waitToken
                    samples.append({"mounted":s})
                    await key(KEY_ENTER)
                    check(scene().scene.waitToken == token,"Confirm cannot skip animation")
                    if fast:
                        var event := InputEventKey.new()
                        event.keycode=KEY_SHIFT; event.location=KEY_LOCATION_LEFT; event.shift_pressed=true; event.pressed=true
                        Input.parse_input_event(event); Input.flush_buffered_events()
                if fast and current.pacing.input.rate == 1.5: rate_seen = true
            if not seen_tokens.has(s.waitToken):
                seen_tokens[s.waitToken] = true
                samples.append({"scene":s})
                if s.phase == "ResultMessage": results.append({"target":s.reactionTarget,"amount":s.reactionAmount})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    samples.append({"timeout":scene(),"board":board()})
    check(false,"bounded return"); return {}

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
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    if not await boot(): finish(); return
    view.connect("SessionResultObserved",result_observed)
    var before := await settle()
    samples.append({"before":before})
    if before.is_empty(): finish(); return
    await key(KEY_ENTER); await key(KEY_F)
    check(board().target == "enemy-12", "committed adjacent target")
    await key(KEY_ENTER)
    var after := await settle()
    samples.append({"after":after})
    if after.is_empty(): finish(); return
    check(results.size()==1,"one converted physical Attack result")
    check(draw_seen,"actual breath draws")
    for frame in range(6): check(frames_seen.has("kiwi-flame/frame-"+str(frame)),"frame "+str(frame))
    check(after.stage=="Movement","returned player control")
    var caster := "ally-6"
    check(before.actors.filter(func(a): return a.id==caster)[0].mp==after.actors.filter(func(a): return a.id==caster)[0].mp,"zero MP cost preserved")
    check(events.filter(func(e):return e.Kind=="exp").size()==1,"one reward")
    if results.size()==1:
        var first: Dictionary=before.actors.filter(func(a):return a.id==results[0].target)[0]
        var last: Dictionary=after.actors.filter(func(a):return a.id==results[0].target)[0]
        check(last.hp==maxi(0,first.hp-results[0].amount),"single damage authority")
    var audio: Dictionary=JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error==null,"audio error-free")
    check(audio.receipts.any(func(e):return e.Command==92 and e.Operation=="started"),"breath cue")
    if fast: check(rate_seen,"held speed applied")
    samples.append({"frames":frames_seen,"results":results,"audio":audio,"rateSeen":rate_seen})
    await key(KEY_ENTER)
    check(board().stage=="ActionChoice","usable next menu")
    finish()

func finish() -> void:
    var output:=FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples,"events":events,"elapsedMs":Time.get_ticks_msec()-started},"  "))
    output.close(); host.queue_free(); quit(0 if failures.is_empty() else 1)
