extends "res://probes/engine_equipment_observation.gd"
var seen := {}
var healed := false

func settle() -> Dictionary:
    for frame in range(3500):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native failure: " + str([current.failure,s.error])); return {}
        for row in s.fairySprites:
            var key_: String = row.binding.resource
            if seen.has(key_): continue
            seen[key_] = true
            var sprite := view.find_child(row.name,true,false) as Sprite2D
            check(sprite != null and sprite.texture != null, "real mounted fairy texture")
            check(key_.begins_with("healing-enemy/"), "enemy resource selected")
            check(row.binding.hd == (OS.get_environment("SF2_HEAL_EXPECT_HD") != "0"), "complete-group HD selection")
            if sprite != null:
                var index := int(row.name.trim_prefix("FairyBody").trim_prefix("FairyWings").trim_prefix("FairyDust"))
                var particle: Dictionary = s.healing.Fairy.Dust[index] if row.name.begins_with("FairyDust") else s.healing.Fairy.Fairies[index]
                var expected := Vector2(particle.X-128,particle.Y-128)-sprite.get_rect().position*sprite.scale
                check(sprite.position.is_equal_approx(expected), "source coordinate projection")
                check(sprite.texture.get_size()*sprite.scale == Vector2(row.binding.width,row.binding.height), "logical footprint")
            samples.append({"fairy":row,"state":s.healing.Fairy,"displayedEnemy":s.displayedEnemy})
        if s.phase == "ResultMessage" and s.spell != null and s.spell.Value == "heal":
            healed = true
            if not seen.has("result"):
                seen.result = true
                samples.append({"result":s,"board":board()})
        if current.hasBattleControl and s.phase == null and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.requiresAcknowledgement and not s.suppressed:
            if s.healing == null or s.healing.AtTimedInput or s.healing.LogicalComplete: await key(KEY_ENTER)
    check(false,"bounded return to player"); return {}

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_HEAL_START"))
    check(admitted == "started","controlled start: "+str(admitted))
    view = host.find_child("BattleSessionView",true,false)
    if view == null:
        samples.append({"labels":host.find_children("*","Label",true,false).map(func(n): return n.text)})
        finish(); return
    view.connect("SessionResultObserved",result_observed)
    var initial := await settle()
    samples.append({"initial":initial})
    for turn in range(8):
        if healed: break
        if initial.is_empty(): break
        await key(KEY_ENTER); await key(KEY_SPACE)
        initial = await settle()
        if healed: break
    check(healed,"production enemy AI selected HEAL")
    check(not initial.is_empty(), "returned player control")
    var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == "enemy-0")
    var recovery := events.filter(func(e): return e.Kind == "hp" and e.Actor.Value == "enemy-8")
    check(payments.size() == 1 and payments[0].Before-payments[0].After == 3, "one HEAL1 MP payment")
    check(recovery.size() == 1 and recovery[0].Before == 1 and recovery[0].After == 11, "capped recipient recovery")
    check(events.all(func(e): return e.Kind != "exp"), "no enemy EXP")
    check(seen.keys().filter(func(k): return k.begins_with("healing-enemy/")).size() == 9, "all nine actual enemy resources")
    samples.append({"returned":initial})
    finish()

func finish() -> void:
    var file := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples,"events":events},"  "))
    file.close(); host.queue_free()
    await process_frame
    await process_frame
    quit(0 if failures.is_empty() else 1)
