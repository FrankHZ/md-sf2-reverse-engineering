extends "res://probes/engine_equipment_observation.gd"

var caster_id := OS.get_environment("SF2_DISPEL_CASTER")
var enemy_cast := caster_id.begins_with("enemy-")
var suppressed_cast := OS.get_environment("SF2_DISPEL_SILENCED") == "1"
var frames_seen := {}
var results := []
var cast_seen := false

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
            check(not suppressed_cast and s.spell.Value == "dispel", "DISPEL owns art")
            check(s.spellAnimationSelector == 9 and s.backgroundVisible and s.allyVisible and s.enemyVisible,
                "selector and both scene actors")
            if s.debuff.flash:
                var caster := view.find_child("Enemy" if enemy_cast else "Ally", true, false) as Sprite2D
                check((caster.modulate == Color.WHITE) == s.reducedFlash, "Reduced Flash controls cast tint")
            var effect := view.find_child("DebuffEffect", true, false)
            for row in s.debuff.sprites:
                var cloud := effect.get_node(NodePath(row.name)) as Sprite2D
                check(cloud.visible and cloud.texture != null and cloud.z_index == 4, "foreground mounted cloud")
                check(cloud.texture.get_size() * cloud.scale == Vector2(32,32), "logical cloud size")
                check(row.hd == (OS.get_environment("SF2_DISPEL_EXPECT_HD") == "1"), "whole group selection")
                check(row.y >= (48 if enemy_cast else 32) and row.y < (80 if enemy_cast else 64), "target side y")
                check(row.x >= (168 if enemy_cast else 64) and row.x < (200 if enemy_cast else 96), "target side x")
                frames_seen[row.resource] = true
            if s.phase == "ActionAnimation" and not cast_seen and not s.debuff.sprites.is_empty():
                cast_seen = true
                var token = s.waitToken
                await key(KEY_ENTER)
                check(scene().scene.waitToken == token, "early confirm cannot finish cast")
        if s.spell != null and s.spell.Value == "dispel" and s.phase in ["ResultMessage", "SilencedCastMessage"] and not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            results.append({"target":s.displayedAlly if enemy_cast else s.displayedEnemy,
                "kind":s.reactionKind,"message":s.message,"phase":s.phase})
        if s.requiresAcknowledgement == true and not s.suppressed: await key(KEY_ENTER)
    check(false, "bounded control return")
    samples.append({"deadline":scene(),"board":board()})
    return {}

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_DISPEL_START"))
    view = host.find_child("BattleSessionView", true, false)
    check(admitted == "started" and view != null, "private mount: " + str(admitted))
    if not failures.is_empty():
        samples.append({"startup":host.find_children("*", "Label", true, false).map(func(node): return node.text)})
        finish(); return
    view.connect("SessionResultObserved", result_observed)
    var before := await settle()
    if enemy_cast:
        for _turn in range(12):
            if before.is_empty() or not results.is_empty(): break
            await key(KEY_ENTER)
            await key(KEY_SPACE)
            before = await settle()
    else:
        for _turn in range(8):
            if before.is_empty() or before.actor == caster_id: break
            await key(KEY_ENTER)
            await key(KEY_SPACE)
            before = await settle()
        check(not before.is_empty() and before.actor == caster_id, "ordinary Kazin turn")
        if not failures.is_empty(): finish(); return
        await key(KEY_ENTER)
        await key(KEY_H)
        for _row in range(5):
            await key(KEY_ENTER)
            var selected := board()
            if selected.spell != null and selected.spell.Value == "dispel": break
            await key(KEY_ESCAPE)
            await key(KEY_DOWN)
        check(board().stage == "CommitReady" and board().spell.Value == "dispel", "legal learned Magic selection")
        samples.append({"selection":board()})
        if not failures.is_empty(): finish(); return
        await key(KEY_ENTER)
        before = await settle()
    check(not before.is_empty() and not results.is_empty(), "results and ordinary control")
    check(frames_seen.is_empty() if suppressed_cast else frames_seen.size() == 3 and cast_seen, "all cloud frames or silence suppression")
    var charges := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == caster_id)
    check(charges.size() == 1 and charges[0].Before - charges[0].After == 5, "single DISPEL MP payment")
    var writes := events.filter(func(e): return e.Kind == "status")
    if OS.get_environment("SF2_DISPEL_SUCCESS") == "1":
        check(results.size() == 2 and results[0].target == "enemy-2" and results[1].target == "enemy-3"
            and results[0].kind == "Ineffective" and results[1].kind == "Status", "ordered mixed results")
        check(writes.size() == 1 and int(writes[0].After) == (int(writes[0].Before) | 768), "existing silence status write")
    if suppressed_cast:
        check(writes.is_empty() and events.filter(func(e): return e.Kind == "rng-dispel").is_empty(), "silence skips effect RNG and writes")
    check(scene().scene.debuff == null, "effect retired")
    if not before.is_empty():
        await key(KEY_ENTER)
        check(board().stage == "ActionChoice", "next ordinary menu")
    samples.append({"results":results,"frames":frames_seen,"charges":charges})
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples,"events":events}, "  "))
    output.close()
    host.queue_free()
    await process_frame
    await process_frame
    quit(0 if failures.is_empty() else 1)
