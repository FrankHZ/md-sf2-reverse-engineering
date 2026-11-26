extends "res://probes/engine_equipment_observation.gd"

# One controlled Battle19 ordinary AI cast; no authoritative state mutation.
var results := []
var fairy_seen := {}
var idle := {}
var restored := false
var bodies := {"enemy-2":"enemy0-palette2/", "enemy-4":"enemy42-palette1/", "enemy-12":"enemy9-palette0/"}

func settle() -> Dictionary:
    for _frame in range(7000):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            samples.append({"failure":current}); check(false,"native failure"); return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.spell != null and s.spell.Value == "aura":
            check(s.spell.Level == 1 and s.spellAnimationSelector == 4,"existing AURA1 selector")
            if not seen_tokens.has(s.waitToken):
                seen_tokens[s.waitToken] = true
                samples.append({"scene":s})
                if s.phase == "ResultMessage":
                    results.append(s.reactionTarget)
                    check(s.displayedEnemy == s.reactionTarget and s.enemyVisible,"actual enemy recipient")
                    check(s.enemy != null,"mounted recipient texture")
                    check(s.enemyResource.begins_with(bodies[s.reactionTarget]),"recipient species body")
                    check(s.enemyX == 16 and s.enemyY == 48,"recipient scene origin")
                    check(not s.message.contains("enemy-"),"localized readable recipient")
                if s.phase == "ActorEnter": restored = s.displayedEnemy == "enemy-4"
            if s.healing != null and s.healing.Caller == "bsc05:ally-idle-sleep":
                idle[s.reactionTarget] = max(idle.get(s.reactionTarget,0),s.healing.Remaining)
            for row in s.fairySprites:
                if fairy_seen.has(row.binding.resource): continue
                fairy_seen[row.binding.resource] = true
                check(row.binding.resource.begins_with("healing-enemy/"),"evil fairy bank")
                check(row.binding.hd == (OS.get_environment("SF2_AURA_HD") == "1"),"selected graphics group")
                var sprite := view.find_child(row.name,true,false) as Sprite2D
                check(sprite != null and sprite.texture != null,"actual fairy texture")
                if sprite != null:
                    var index := int(row.name.trim_prefix("FairyBody").trim_prefix("FairyWings").trim_prefix("FairyDust"))
                    var particle: Dictionary = s.healing.Fairy.Dust[index] if row.name.begins_with("FairyDust") else s.healing.Fairy.Fairies[index]
                    var expected := Vector2(particle.X-128,particle.Y-128)-sprite.get_rect().position*sprite.scale
                    check(sprite.position.is_equal_approx(expected),"source fairy placement")
                samples.append({"fairy":row,"state":s.healing.Fairy})
        if s.requiresAcknowledgement and not s.suppressed:
            if s.healing == null or s.healing.AtTimedInput or s.healing.LogicalComplete: await key(KEY_ENTER)
    samples.append({"timeout":scene(),"board":board()}); check(false,"bounded return"); return {}

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    for _frame in range(2400):
        OS.delay_msec(10)
        await process_frame
        view = host.find_child("BattleSessionView",true,false)
        if view != null: break
        var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
        if opening.active: await key(KEY_E)
    if view == null:
        check(false,"ordinary Continue admitted"); finish(); return
    view.connect("SessionResultObserved",result_observed)
    var initial := await settle()
    samples.append({"initial":initial})
    for _turn in range(12):
        if results.size() > 0 or initial.is_empty(): break
        await key(KEY_ENTER); await key(KEY_SPACE)
        initial = await settle()
    check(results == ["enemy-2","enemy-4","enemy-12"],"ordered mixed-species recipients")
    check(restored,"caster return")
    check(not initial.is_empty(),"usable next player input")
    var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == "enemy-4")
    check(payments.size() == 1 and payments[0].Before-payments[0].After == 7,"one AURA1 payment")
    check(events.all(func(e): return e.Kind != "exp"),"no enemy reward")
    var recoveries := events.filter(func(e): return e.Kind == "healing-recovery")
    check(recoveries.map(func(e): return e.Target.Value) == results,"one ordered recovery per recipient")
    check(recoveries.map(func(e): return int(e.After-e.Before)) == [18,0,18],"actual capped recovery")
    check(fairy_seen.size() == 9,"complete evil fairy resources")
    samples.append({"returned":initial,"idle":idle})
    if not initial.is_empty():
        await key(KEY_ENTER)
        check(board().stage == "ActionChoice","next ordinary action input")
        await key(KEY_ESCAPE)
    finish()

func finish() -> void:
    var file := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples,"events":events},"  "))
    file.close(); host.queue_free()
    await process_frame
    await process_frame
    quit(0 if failures.is_empty() else 1)
