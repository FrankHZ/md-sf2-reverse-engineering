extends "res://probes/engine_equipment_observation.gd"

# Ordinary input and real node readback; controlled records are admitted before play.
var caster_id := OS.get_environment("SF2_DEBUFF_CASTER")
var enemy_cast := caster_id.begins_with("enemy-")
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
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native error: " + str(current))
            return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.debuff != null:
            check(s.spell.Value == spell_name and s.spell.Level == rank_, "selected effect owns art")
            check(s.spellAnimationSelector == (41 if spell_name == "slow" and rank_ == 2 else 9)
                and s.backgroundVisible and s.allyVisible and s.enemyVisible, "selector and mounted scene actors")
            if s.debuff.flash:
                flashes_seen = true
                var caster := view.find_child("Enemy" if enemy_cast else "Ally", true, false) as Sprite2D
                check((caster.modulate == Color.WHITE) == s.reducedFlash, "Reduced Flash cast tint")
            var effect := view.find_child("DebuffEffect", true, false)
            for row in s.debuff.sprites:
                var cloud := effect.get_node(NodePath(row.name)) as Sprite2D
                check(cloud.visible and cloud.texture != null and cloud.z_index == 4, "mounted foreground cloud")
                check(cloud.texture.get_size() * cloud.scale == Vector2(32,32), "logical cloud size")
                check(row.hd == (OS.get_environment("SF2_DEBUFF_HD") == "1"), "complete group selection")
                check(row.x >= (168 if enemy_cast else 64) and row.x < (200 if enemy_cast else 96) and row.y >= (48 if enemy_cast else 32) and row.y < (80 if enemy_cast else 64), "enemy receiver coordinates")
                frames_seen[row.resource] = row
            if s.phase == "ActionAnimation":
                cast_index = s.animationIndex
                if not cast_seen and not s.debuff.sprites.is_empty():
                    cast_seen = true
                    if not fast:
                        var token = s.waitToken
                        await key(KEY_ENTER)
                        check(scene().scene.waitToken == token, "early confirm retains cast token")
        if s.spell != null and s.spell.Value == spell_name and not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            samples.append({"scene":s})
            if s.phase == "ResultMessage":
                results.append({"target":s.displayedAlly if enemy_cast else s.displayedEnemy,"kind":s.reactionKind,"message":s.message})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    check(false, "bounded return to control")
    samples.append({"deadline":scene(),"board":board()})
    return {}

func boot() -> bool:
    if enemy_cast:
        await process_frame
        for _opening in range(600):
            var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
            if not opening.active: break
            await key(KEY_E)
        var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_DEBUFF_START"))
        view = host.find_child("BattleSessionView", true, false)
        check(admitted == "started" and view != null, "controlled enemy start: " + str(admitted))
        return failures.is_empty()
    for _frame in range(3000):
        OS.delay_msec(10)
        await process_frame
        view = host.find_child("BattleSessionView", true, false)
        if view != null: return true
        var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
        if opening.active: await key(KEY_E); continue
        var entry := host.find_child("SessionEntryView", true, false)
        var panel := host.find_child("SaveSlotView", true, false)
        if panel != null and JSON.parse_string(panel.call("ReadSaveObservationJson")).get("state") == "continue-slots":
            await key(KEY_ENTER)
        elif entry != null and entry.is_visible_in_tree():
            var e: Dictionary = JSON.parse_string(entry.call("ReadEntryObservationJson"))
            if e.selectedAction != null and e.selectedAction != "Continue": await key(KEY_DOWN)
            elif e.page >= 0 and e.continueText == null: await key(KEY_E)
            else: await key(KEY_ENTER)
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
    if enemy_cast:
        for _turn in range(12):
            if before.is_empty() or not results.is_empty(): break
            await key(KEY_ENTER)
            await key(KEY_SPACE)
            before = await settle()
        await verify_return(before)
        finish(); return
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
    check(frames_seen.size() == 3 and cast_seen and flashes_seen, "three source frames, cast and flash boundary")
    var charges := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id)
    var costs := {"slow":[3,6],"muddle":[6,11],"sleep":[4],"dispel":[5]}
    check(charges.is_empty() if item_id > 0 else charges.size() == 1 and charges[0].Before-charges[0].After == costs[spell_name][rank_-1], "item free or single rank MP payment")
    var expected_targets: Array = ["ally-1", "ally-4"] if enemy_cast else ["enemy-1", "enemy-2"]
    check(results.map(func(row): return row.target) == expected_targets, "ordered recipients")
    if not enemy_cast:
        var mixed := OS.get_environment("SF2_DEBUFF_MIXED") == "1"
        var expected: Array = ["Ineffective", "Status"] if mixed else (["Status", "Status"] if spell_name == "slow" else ["Ineffective", "Ineffective"])
        check(results.map(func(row): return row.kind) == expected, "authoritative effective/ineffective delivery")
        var mask: int = {"slow":3072,"muddle":56 if rank_ == 2 else 48,"sleep":192,"dispel":768}[spell_name]
        for result in results:
            var status: int = int(after.statuses.filter(func(row): return row.actor == result.target)[0].status)
            if result.kind == "Status": check((status & mask) == mask, "actual effect word after reaction")
        if item_id > 0:
            check(events.filter(func(e): return e.Kind == "rng-item-wear").size() == 1, "single equipment wear draw")
            var damage := events.filter(func(e): return e.Kind == "item-damaged")
            check(damage.size() == 1 and int(damage[0].Before) == item_id and int(damage[0].After) == item_id + 256, "selected copy damaged once")
            var holder: Dictionary = after.actors.filter(func(row): return row.id == caster_id)[0]
            check(holder.mp == 0 and holder.items.any(func(word): return int(word) == item_id + 256), "zero MP and actual held damaged copy")
            check(cast_index == (11 if item_id == 54 else 19), "source Item gesture, not Magic cast")
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    check(audio.error == null and audio.receipts.filter(func(e): return e.Command == 77 and e.Operation == "started").size() == 1, "one cast cue")
    check(scene().scene.debuff == null, "effect retired")
    samples.append({"after":after,"results":results,"frames":frames_seen,"charges":charges,"animationIndex":cast_index})
    if not after.is_empty():
        await key(KEY_ENTER)
        check(board().stage == "ActionChoice", "usable subsequent action menu")

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples,"events":events}, "  "))
    output.close()
    host.queue_free()
    await process_frame
    await process_frame
    quit(0 if failures.is_empty() else 1)
