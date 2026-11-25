extends "res://probes/engine_equipment_observation.gd"

# BUFF has no spell tiles. Observe its mounted body, backdrop and source sound cadence.
var caster_id := OS.get_environment("SF2_DEBUFF_CASTER")
var spell_name := OS.get_environment("SF2_DEBUFF_SPELL")
var rank_ := int(OS.get_environment("SF2_DEBUFF_RANK"))
var item_id := int(OS.get_environment("SF2_DEBUFF_ITEM"))
var target_id := OS.get_environment("SF2_DEBUFF_TARGET")
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
    for _frame in range(3000):
        OS.delay_msec(1)
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native error: " + str(current))
            return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.buff != null:
            check(s.spell.Value == spell_name and s.spell.Level == rank_, "selected BUFF action")
            check(s.spellAnimationSelector == (40 if rank_ == 2 else 8) and s.backgroundVisible and s.allyVisible and not s.enemyVisible, "BUFF selector and ally-only composition")
            var body := view.find_child("Ally", true, false) as Sprite2D
            check(body.texture != null and body.visible, "actual body raster mounted")
            frames_seen[s.ally.resource] = s.ally
            if item_id > 0:
                check(s.ally.hd == (OS.get_environment("SF2_DEBUFF_HD") == "1"), "actual complete body group selection")
            if s.buff.flash:
                flashes_seen = true
                check(s.buffBackdrop == ("000000ff" if s.reducedFlash else "24db24ff"), "source backdrop flash and Reduced Flash")
            if s.phase == "ActionAnimation":
                cast_index = s.animationIndex
                if not cast_seen and s.buff.pulses > 0:
                    cast_seen = true
                    if not fast:
                        var token = s.waitToken
                        await key(KEY_ENTER)
                        check(scene().scene.waitToken == token, "early confirm retains cast token")
        if s.spell != null and s.spell.Value == spell_name and not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "ResultMessage":
                results.append({"target":s.displayedAlly,"kind":s.reactionKind,"message":s.message})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    check(false, "bounded return to control")
    samples.append({"deadline":scene(),"board":board(),"native":{"focus":root.has_focus(),"visible":view.is_visible_in_tree(),"process":view.can_process()}})
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
    check(not after.is_empty() and not results.is_empty(), "results and control return")
    var before: Dictionary = samples.filter(func(row): return row.has("before"))[0].before
    for result in results:
        var prior: Dictionary = before.actors.filter(func(row): return row.id == result.target)[0]
        var current: Dictionary = after.actors.filter(func(row): return row.id == result.target)[0]
        if result.kind == "Status":
            check(current.attack > prior.attack if spell_name == "attack" else current.defense > prior.defense and current.agility > prior.agility, "actual stat increase")
        else:
            check(current.attack == prior.attack and current.defense == prior.defense and current.agility == prior.agility, "ineffective recast preserves stats")
    check(not frames_seen.is_empty() and cast_seen and flashes_seen, "actual raster, cast and flash boundary")
    var charges := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id)
    check(charges.is_empty() if item_id > 0 else charges.size() == 1 and charges[0].Before-charges[0].After == (2 if rank_ == 1 else 5), "item free or single rank MP payment")
    var targets := results.map(func(row): return row.target)
    var expected: Array = ["ally-7"] if spell_name == "attack" else (["ally-0","ally-4","ally-7","ally-9","ally-11","ally-13","ally-15","ally-18"] if rank_ == 2 else (["ally-4","ally-7","ally-11","ally-15"] if item_id == 0 else ["ally-1","ally-4","ally-7","ally-11"]))
    check(targets == expected, "ordered recipients")
    check(cast_index == (13 if item_id == 80 else 25 if item_id > 0 else 1), "actual source Magic or Item gesture")
    check(targets.size() == 1 if spell_name == "attack" else targets.size() >= 2, "actual single or area recipients")
    check(results.any(func(row): return row.kind == "Status"), "successful recipient")
    if OS.get_environment("SF2_DEBUFF_MIXED") == "1":
        check(results.any(func(row): return row.kind == "Ineffective"), "recast ineffective recipient")
    if item_id > 0:
        check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == 1, "single equipment wear draw")
        var holder: Dictionary = after.actors.filter(func(row): return row.id == caster_id)[0]
        check(holder.mp == 0, "equipment uses zero MP")
        var damage := events.filter(func(e): return e.Kind == "item-damaged")
        check(damage.size() == 1 and damage[0].Before == item_id and damage[0].After == item_id + 256, "selected copy damaged once")
        check(holder.items.any(func(word): return int(word) == item_id + 256), "actual held damaged copy")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null and audio.receipts.filter(func(e): return e.Command == 77 and e.Operation == "started").size() == 1, "one cast cue")
    check(audio.receipts.filter(func(e): return e.Command == 97 and e.Operation == "started").size() == 14, "fourteen accelerating PSH cues")
    check(scene().scene.buff == null and scene().scene.buffBackdrop == "000000ff", "effect retired and black restored")
    samples.append({"after":after,"results":results,"frames":frames_seen,"charges":charges,"animationIndex":cast_index,"audio":audio})
    if not after.is_empty():
        for _ready in range(10): await process_frame
        await key(KEY_ENTER)
        for _ready in range(10): await process_frame
        check(board().stage == "ActionChoice", "next ordinary action menu")
        samples.append({"nextControl":board().stage})

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"controlled private BUFF Magic/equipment presentation and return; ordinary input; no natural reach or hardware timing claim",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
