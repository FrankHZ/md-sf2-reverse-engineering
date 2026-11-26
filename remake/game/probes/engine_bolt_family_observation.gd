extends "res://probes/engine_equipment_observation.gd"

# Bounded ordinary-input bolt observation; store transitions and distinct graphics only.
var caster_id := OS.get_environment("SF2_BOLT_CASTER")
var spell_name := OS.get_environment("SF2_BOLT_SPELL")
var rank_ := int(OS.get_environment("SF2_BOLT_RANK"))
var item_id := int(OS.get_environment("SF2_BOLT_ITEM"))
var target_id := OS.get_environment("SF2_BOLT_TARGET")
var enemy_case := OS.get_environment("SF2_BOLT_ENEMY") == "1"
var silenced_case := OS.get_environment("SF2_BOLT_SILENCED") == "1"
var rate_requested := false
var variation := int(OS.get_environment("SF2_BOLT_VARIATION"))
var expected_hd := OS.get_environment("SF2_BOLT_HD") == "1"
var expected_reduced := OS.get_environment("SF2_BOLT_REDUCED") == "1"
var fast := OS.get_environment("SF2_BOLT_FAST") == "1"
var frames_seen := {}
var results := []
var sprite_slots := {}
var draw_seen := false
var cast_seen := false
var cast_index = null
var rate_seen := false

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
        var relevant: bool = s.item == item_id if item_id > 0 and not enemy_case else s.spell != null and s.spell.Value == spell_name and s.spell.Level == rank_
        if not relevant:
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        if silenced_case:
            check(s.attackSpell == null or not s.attackSpell.visible, "silenced cast has no bolt")
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        check(s.spellAnimationSelector == 7+32*(variation-1), "action selector")
        if s.phase == "ActionAnimation":
            cast_index = s.animationIndex
            if fast and not rate_requested:
                rate_requested = true
                hold_rate(true)
            check(s.backgroundVisible and s.groundVisible and s.allyVisible and s.enemyVisible, "mounted actors and scenery")
            if s.attackSpell != null and s.attackSpell.preparedBolt != null:
                draw_seen = draw_seen or s.attackSpell.drawCount > 0
                var effect: Dictionary = s.attackSpell.preparedBolt
                check(effect.variation == variation and effect.targetIsAlly == enemy_case, "source variation and side")
                check(effect.reducedFlash == expected_reduced, "Reduced Flash reaches effect")
                check(s.weaponVisible and s.weapon.texturePresent, "equipped weapon stays composed")
                for sprite in effect.sprites:
                    check(sprite.hd == expected_hd, "complete graphics group selection")
                    check(sprite.textureWidth == sprite.width * (4 if expected_hd else 1), "actual mounted texture density")
                    sprite_slots[str(sprite.slot)] = true
                    if not frames_seen.has(sprite.resource): frames_seen[sprite.resource] = sprite
                if not cast_seen and not effect.sprites.is_empty():
                    cast_seen = true
                    samples.append({"mounted":s})
                    var token = s.waitToken
                    if not fast:
                        await key(KEY_ENTER)
                        check(scene().scene.waitToken == token, "early confirm retains animation token")
                if fast and current.pacing.input.rate == 1.5: rate_seen = true
        if not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "ResultMessage": results.append({"target":s.displayedAlly if enemy_case else s.displayedEnemy,"kind":s.reactionKind,"amount":s.reactionAmount})
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
    if enemy_case:
        events.clear()
        await key(KEY_SPACE)
        var after := await settle()
        samples.append({"after":after})
        await verify_return(after)
        finish()
        return
    if item_id > 0:
        await key(KEY_I)
        var holder: Dictionary = before.actors.filter(func(a): return a.id == caster_id)[0]
        var slot: int = holder.items.map(func(word): return int(word)).find(item_id)
        check(slot >= 0, "selected held copy")
        for _slot in range(slot): await key(KEY_DOWN)
        await key(KEY_ENTER)
    else:
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
    var actor: String = target_id if enemy_case else caster_id
    var a: Dictionary = before.actors.filter(func(row): return row.id == actor)[0]
    var b: Dictionary = after.actors.filter(func(row): return row.id == actor)[0]
    if silenced_case:
        check(results.is_empty() and frames_seen.is_empty(), "silence delivers no damage or bolt")
        check(events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == actor).size() == 1, "silenced Magic pays once")
        await key(KEY_ENTER)
        check(board().stage == "ActionChoice", "silenced return remains usable")
        return
    if fast: check(rate_seen, "held acceleration actually applied")
    check(not results.is_empty(), "ordered damage result")
    check(cast_seen and draw_seen and not frames_seen.is_empty(), "mounted bolt frames")
    check(sprite_slots.size() == variation + (4 if variation == 4 else variation-1), "source strike and orb counts")
    if variation > 1: check(frames_seen.has("bolt/frame-22"), "source orb group")
    if item_id > 0:
        check(a.mp == b.mp, "Item uses no MP")
        check(not samples.any(func(row): return row.has("scene") and row.scene.phase == "SpellCost"), "Item keeps its own payment path")
        check(cast_index == int(OS.get_environment("SF2_BOLT_ITEM_ANIMATION")), "source body Item sequence")
        check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == (0 if enemy_case else 1), "one allied wear draw or enemy no-wear")
    else:
        check(events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == actor).size() == 1 and a.mp > b.mp, "one Magic payment")
    var targets := results.map(func(result): return result.target)
    var hp_events := events.filter(func(e): return e.Kind == "hp" and targets.has(e.Actor.Value))
    check(hp_events.map(func(e): return e.Actor.Value) == targets, "ordered result application exactly once")
    for result in results:
        var target_before: Dictionary = before.actors.filter(func(row): return row.id == result.target)[0]
        var target_after: Dictionary = after.actors.filter(func(row): return row.id == result.target)[0]
        check(target_after.hp == maxi(0, int(target_before.hp-result.amount)), "displayed damage agrees with session HP")
        if target_after.hp == 0:
            check(events.any(func(e): return e.Kind == "death-cleanup" and e.Actor.Value == result.target), "lethal recipient cleanup")
    if not enemy_case:
        check(events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == caster_id).size() == 1, "one action reward")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null, "audio error-free")
    check(audio.receipts.any(func(e): return e.Command == 119 and e.Operation == "started"), "prepared BOLT cue played")
    samples.append({"results":results,"frames":frames_seen,"sprite_slots":sprite_slots.size(),"animationIndex":cast_index,"audio":audio,"rateSeen":rate_seen})
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
        "scope":"controlled bolt ordinary input and mounted frames; no natural reach or original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
