extends "res://probes/engine_equipment_observation.gd"

# Recovery Item/Magic uses ordinary input; observe mounted sprites and ordered results.
var caster_id := OS.get_environment("SF2_DEBUFF_CASTER")
var spell_name := OS.get_environment("SF2_DEBUFF_SPELL")
var rank_ := int(OS.get_environment("SF2_DEBUFF_RANK"))
var item_id := int(OS.get_environment("SF2_DEBUFF_ITEM"))
var target_id := OS.get_environment("SF2_DEBUFF_TARGET")
var missing_group := OS.get_environment("SF2_RECOVERY_MISSING") == "1"
var none_effect := OS.get_environment("SF2_RECOVERY_NONE") == "1"
var frames_seen := {}
var results := []
var cast_seen := false
var flashes_seen := false
var cast_index = null
var fast := OS.get_environment("SF2_DEBUFF_FAST") == "1"

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
        if (item_id > 0 and s.item != item_id) or (item_id == 0 and (s.spell == null or s.spell.Value != spell_name)):
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        if none_effect:
            check(s.healing == null and s.detox == null and s.fairySprites.is_empty(), "NONE consumable has no recovery overlay")
        else:
            check(s.spellAnimationSelector == (38 if spell_name == "detox" else 4 + 32 * (rank_-1)), "actual recovery selector")
            check(s.backgroundVisible and s.allyVisible and not s.enemyVisible, "mounted ally recovery scene")
        if s.phase == "ActionAnimation": cast_index = s.animationIndex
        for row in s.fairySprites:
            var sprite := view.find_child(row.name, true, false) as Sprite2D
            check(sprite != null and sprite.visible and sprite.texture != null, "mounted fairy sprite")
            check(row.binding.hd == (OS.get_environment("SF2_RECOVERY_FAIRY_HD") == "1"), "fairy whole group selection")
            frames_seen[row.binding.resource] = row
        if missing_group: check(s.detox == null, "absent optional DETOX group retains Item fallback")
        if s.detox != null:
            var effect := view.find_child("DetoxEffect", true, false)
            for row in s.detox.sprites:
                var sprite := effect.get_node(NodePath(row.name)) as Sprite2D
                check(sprite.visible and sprite.texture != null, "mounted DETOX mote/trail")
                check(row.hd == (OS.get_environment("SF2_DEBUFF_HD") == "1"), "DETOX whole group selection")
                frames_seen[row.resource] = row
        if not cast_seen and s.phase == "ActionAnimation" and (s.healing != null and s.healing.Remaining > 8 or s.detox != null and s.detox.flash):
            cast_seen = true
            if not fast:
                var token = s.waitToken
                await key(KEY_ENTER)
                check(scene().scene.waitToken == token, "early confirm retains active cast token")
        if not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "ResultMessage": results.append({"target":s.displayedAlly,"kind":s.reactionKind,"message":s.message})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    check(false, "bounded return to control")
    samples.append({"deadline":scene(),"board":board()})
    return {}

func boot() -> bool:
    for _frame in range(3000):
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
            if e.selectedAction != null and e.selectedAction != "Continue": await key(KEY_DOWN)
            elif e.page >= 0 and e.continueText == null: await key(KEY_E)
            else: await key(KEY_ENTER)
    samples.append({"save":JSON.parse_string(host.find_child("SaveSlotView", true, false).call("ReadSaveObservationJson"))})
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
    if fast: hold_rate(true)
    await key(KEY_ENTER)
    var after := await settle()
    if fast: hold_rate(false)
    samples.append({"after":after})
    await verify_return(after)
    finish()

func verify_return(after: Dictionary) -> void:
    if after.is_empty(): return
    var before: Dictionary = samples.filter(func(row): return row.has("before"))[0].before
    var owner_before: Dictionary = before.actors.filter(func(a): return a.id == caster_id)[0]
    var owner_after: Dictionary = after.actors.filter(func(a): return a.id == caster_id)[0]
    check(not results.is_empty(), "delivered ordered result")
    for result in results:
        var prior: Dictionary = before.actors.filter(func(a): return a.id == result.target)[0]
        var now: Dictionary = after.actors.filter(func(a): return a.id == result.target)[0]
        if spell_name == "detox":
            check(now.hp == prior.hp and (int(now.status) & 3) == 0, "cure preserves HP and clears admitted bits")
        elif item_id > 0:
            var deliveries := events.filter(func(e): return (e.Kind == "healing-recovery" and e.Target.Value == result.target) or (spell_name != "aura" and e.Kind == "hp" and e.Actor.Value == result.target))
            var recovered: float = deliveries[0].After if not deliveries.is_empty() else prior.hp
            check(recovered == min(prior.maxHp, prior.hp + (15 if spell_name == "aura" else 20 if none_effect else 10)), "authoritative recovery amount and full-HP clamp")
    if spell_name == "aura": check(results.size() > 1, "ordered area recipients")
    check(none_effect or missing_group or not frames_seen.is_empty(), "actual recovery sprites mounted")
    if item_id > 0:
        check(cast_index == (13 if item_id == 81 else 18 if item_id == 43 else 25), "actual Item gesture")
        check(owner_before.mp == owner_after.mp, "Item uses zero MP")
        check(not events.any(func(e): return e.Kind == "silenced-cast"), "Item ignores silence")
        check(not samples.any(func(row): return row.has("scene") and row.scene.phase == "SpellCost"), "Item never enters Magic payment")
        if not none_effect:
            check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == 1, "one selected-slot wear draw")
    else:
        check(events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id).size() == 1, "one learned Magic payment")
    check(none_effect or samples.any(func(row): return row.has("scene") and row.scene.phase == "SpellStop"), "effect stop before return")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null, "audio consumer error-free")
    samples.append({"results":results,"frames":frames_seen,"animationIndex":cast_index,"audio":audio})
    for _ready in range(10): await process_frame
    await key(KEY_ENTER)
    for _ready in range(10): await process_frame
    check(board().stage == "ActionChoice", "usable next ordinary action menu")

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"controlled private recovery Magic/equipment presentation and return; ordinary input; no natural reach or hardware timing claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
