extends "res://probes/engine_equipment_observation.gd"

# Ordinary Item input and actual scene projection; no gameplay setters or captures.
var caster_id := OS.get_environment("SF2_ABSORB_CASTER")
var item_id := int(OS.get_environment("SF2_ABSORB_ITEM"))
var target_id := OS.get_environment("SF2_ABSORB_TARGET")
var expected_hd := OS.get_environment("SF2_ABSORB_HD") == "1"
var fast := OS.get_environment("SF2_ABSORB_FAST") == "1"
var expected_reduced := OS.get_environment("SF2_ABSORB_REDUCED") == "1"
var rate_requested := false
var rate_seen := false
var cast_seen := false
var flashes := {}
var results := []
var cast_index = null
func hold_rate(pressed: bool) -> void:
    var event := InputEventKey.new()
    event.keycode = KEY_SHIFT
    event.location = KEY_LOCATION_LEFT
    event.shift_pressed = pressed
    event.pressed = pressed
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func settle() -> Dictionary:
    for _frame in range(6000):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native error: " + str(current))
            return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.item != item_id:
            if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
            continue
        check(s.spellAnimationSelector == 10, "source ABSORB selector")
        check(s.phase != "SpellCost", "Item has no cast payment phase")
        if s.absorb != null:
            check(s.phase == "ActionAnimation", "flash only accompanies Item gesture")
            cast_index = s.animationIndex
            check(s.backgroundVisible and s.allyVisible and s.enemyVisible, "actual actors/scenery retained")
            var body := view.find_child("Ally", true, false) as Sprite2D
            check(body != null and body.texture != null and body.visible, "actual body raster")
            check(s.ally.hd == expected_hd, "atomic body graphics group")
            check(s.reducedFlash == expected_reduced, "Reduced Flash setting")
            if fast and not rate_requested:
                rate_requested = true
                hold_rate(true)
            if current.pacing.input.rate == 1.5: rate_seen = true
            if s.absorb.flash:
                check(s.absorbBackdrop == ("000000ff" if expected_reduced else "ff0000ff"), "red backdrop flash or suppression")
                flashes[int((s.absorb.tick-s.absorb.trigger)/7)] = true
                if not cast_seen:
                    cast_seen = true
                    samples.append({"flash":s})
                    if not fast:
                        var token = s.waitToken
                        await key(KEY_ENTER)
                        check(scene().scene.waitToken == token, "early Confirm does not finish Item gesture")
            else: check(s.absorbBackdrop == "000000ff", "black between pulses")
        if not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "Reaction":
                results.append({"actor":s.reactionTarget,"amount":s.reactionAmount,"kind":s.reactionKind,"board":board()})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
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
    await key(KEY_I)
    var holder: Dictionary = before.actors.filter(func(a): return a.id == caster_id)[0]
    var slot: int = holder.items.map(func(word): return int(word) & 127).find(item_id)
    check(slot >= 0, "selected held copy")
    for _slot in range(slot): await key(KEY_DOWN)
    await key(KEY_ENTER)
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
    if after.is_empty(): finish(); return
    var a: Dictionary = before.actors.filter(func(row): return row.id == caster_id)[0]
    var b: Dictionary = after.actors.filter(func(row): return row.id == caster_id)[0]
    var ta: Dictionary = before.actors.filter(func(row): return row.id == target_id)[0]
    var tb: Dictionary = after.actors.filter(func(row): return row.id == target_id)[0]
    check(cast_seen and flashes.size() == 4, "four source flash pulses")
    check(cast_index == int(OS.get_environment("SF2_ABSORB_ITEM_ANIMATION")), "body-specific Item gesture")
    if fast: check(rate_seen, "held acceleration applied")
    check(results.size() == 2, "target then caster reactions")
    check(results.map(func(r): return r.actor) == [target_id,caster_id], "actual ordered reaction targets")
    if results.size() == 2:
        var drained: int = min(5,int(ta.mp))
        check(results[0].amount == drained and results[1].amount == drained, "ordered drain magnitude")
        check(tb.mp == ta.mp-drained and b.mp == min(60,a.mp+drained), "independent target/caster caps")
        var at_target: Dictionary = results[0].board.actors.filter(func(row): return row.id == caster_id)[0]
        check(at_target.mp == a.mp, "caster unchanged during target reaction")
    check(events.filter(func(e): return e.Kind == "rng-mp-absorb").size() == 1, "one drain draw")
    var changed: Array = []
    if ta.mp != tb.mp: changed.append(target_id)
    if a.mp != b.mp: changed.append(caster_id)
    check(events.filter(func(e): return e.Kind == "mp").map(func(e): return e.Actor.Value) == changed, "only actual MP changes, target before caster and no cast payment")
    check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == 1, "one wear draw")
    check(events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == caster_id).size() == 1, "one reward")
    check(a.hp == b.hp and ta.hp == tb.hp, "MP effect preserves HP")
    check(int(b.items[slot]) == item_id+256, "selected copy damaged once")
    for i in range(4):
        if i != slot: check(a.items[i] == b.items[i], "unselected copy unchanged")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null, "audio error-free")
    check(audio.receipts.filter(func(e): return e.Command == 89 and e.Operation == "started").size() == 1, "one source warp cue")
    samples.append({"results":results,"audio":audio,"flashes":flashes,"rateSeen":rate_seen})
    for _ready in range(10): await process_frame
    await key(KEY_ENTER)
    for _ready in range(10): await process_frame
    check(board().stage == "ActionChoice", "usable next action menu")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
        "scope":"controlled ABSORB Item input and projection; no natural reach or original timing claim",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
