extends "res://probes/engine_equipment_observation.gd"

# Bounded ordinary-input desoul observation; store transitions and distinct graphics only.
var caster_id := OS.get_environment("SF2_DESOUL_CASTER")
var spell_name := OS.get_environment("SF2_DESOUL_SPELL")
var rank_ := int(OS.get_environment("SF2_DESOUL_RANK"))
var item_id := int(OS.get_environment("SF2_DESOUL_ITEM"))
var target_id := OS.get_environment("SF2_DESOUL_TARGET")
var enemy_case := OS.get_environment("SF2_DESOUL_ENEMY") == "1"
var silenced_case := OS.get_environment("SF2_DESOUL_SILENCED") == "1"
var rate_requested := false
var variation := int(OS.get_environment("SF2_DESOUL_VARIATION"))
var expected_hd := OS.get_environment("SF2_DESOUL_HD") == "1"
var fast := OS.get_environment("SF2_DESOUL_FAST") == "1"
var frames_seen := {}
var results := []
var expected_reduced := OS.get_environment("SF2_DESOUL_REDUCED") == "1"
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
        var relevant: bool = s.item == item_id if item_id > 0 else s.spell != null and s.spell.Value == spell_name and s.spell.Level == rank_
        if not relevant:
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        if silenced_case:
            check(s.attackSpell == null or not s.attackSpell.visible, "silenced cast has no desoul")
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        check(s.spellAnimationSelector == (3+32*(variation-1)), "action selector")
        if s.phase == "ActionAnimation":
            cast_index = s.animationIndex
            if fast and not rate_requested:
                rate_requested = true
                hold_rate(true)
            check(s.backgroundVisible and s.allyVisible and s.enemyVisible, "mounted actors and scenery")
        if s.desoul != null:
            var effect: Dictionary = s.desoul
            draw_seen = draw_seen or effect.drawCount > 0
            check(effect.variation == variation and not effect.targetIsAlly, "source variation and side")
            check(effect.reducedFlash == expected_reduced, "Reduced Flash reaches effect")
            if not effect.sprites.is_empty(): check(effect.ripple == not expected_reduced, "source backdrop ripple/settings")
            for sprite in effect.sprites:
                check(sprite.hd == expected_hd, "atomic graphics group")
                check(sprite.textureWidth == sprite.width * (4 if expected_hd else 1), "actual texture density")
                frames_seen[sprite.resource] = sprite
                if sprite.resource == "desoul/strike": check(s.reactionKind == "InstantDeath", "strike requires successful reaction")
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
            if s.phase in ["ResultMessage", "DeathMessage"]: results.append({"target":s.displayedAlly if enemy_case else s.displayedEnemy,"kind":s.reactionKind,"amount":s.reactionAmount})
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
        var slot: int = holder.items.map(func(word): return int(word) & 127).find(item_id)
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
        check(results.is_empty() and frames_seen.is_empty(), "silence delivers no results or DESOUL")
        check(events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == actor).size() == 1, "silenced Magic pays once")
        await key(KEY_ENTER)
        check(board().stage == "ActionChoice", "silenced return remains usable")
        return
    if fast: check(rate_seen, "held acceleration actually applied")
    check(not results.is_empty(), "ordered death/no-effect results")
    check(cast_seen and draw_seen and frames_seen.has("desoul/hover"), "mounted reaper")
    check(frames_seen.has("desoul/soul-0") == (variation == 2), "rank2 souls only")
    var kills := results.filter(func(result): return result.kind == "InstantDeath")
    check(frames_seen.has("desoul/strike") == not kills.is_empty(), "successful deaths alone strike")
    if item_id > 0:
        check(a.mp == b.mp and a.mp == 0, "silenced Item uses zero MP")
        check(cast_index == 22, "Item gesture")
        check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == 1, "one selected-copy wear draw")
        for slot in range(4):
            if int(a.items[slot]) & 127 != item_id: check(a.items[slot] == b.items[slot], "other held copies unchanged")
    else:
        var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == actor)
        check(payments.size() == 1 and a.mp-b.mp == (8 if rank_ == 1 else 13), "one Magic payment")
    var targets := results.map(func(result): return result.target)
    check(targets == (["enemy-1", "enemy-2"] if rank_ == 2 else ["enemy-1"]), "ordered selected recipients")
    var hp_events := events.filter(func(e): return e.Kind == "hp" and targets.has(e.Actor.Value))
    check(hp_events.map(func(e): return e.Actor.Value) == kills.map(func(r): return r.target), "exactly one HP application per successful target")
    for result in results:
        var initial: Dictionary = before.actors.filter(func(row): return row.id == result.target)[0]
        var final: Dictionary = after.actors.filter(func(row): return row.id == result.target)[0]
        check(final.hp == (0 if result.kind == "InstantDeath" else initial.hp), "outcome preserves HP authority")
        if result.kind == "InstantDeath":
            check(events.filter(func(e): return e.Kind == "death-cleanup" and e.Actor.Value == result.target).size() == 1, "one death cleanup")
    check(events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == caster_id).size() == 1, "one action reward")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null, "audio error-free")
    check(audio.receipts.any(func(e): return e.Command == 110 and e.Operation == "started"), "hover cue")
    check(audio.receipts.any(func(e): return e.Command == 106 and e.Operation == "started") == not kills.is_empty(), "death-only strike cue")
    samples.append({"results":results,"frames":frames_seen,"animationIndex":cast_index,"audio":audio,"rateSeen":rate_seen})
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
        "scope":"controlled desoul ordinary input and mounted frames; no natural reach or original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
