extends "res://probes/engine_attack_spell_observation.gd"

# Bounded actual host witness: ordinary Stay dispatches the source enemy decisions;
# normal presenter completions and message keys return control without state setters.
var delivered: Array = []
var paid: Array = []
var heal_projected := false

func picture(label: String) -> void:
    var b := board()
    var s: Dictionary = scene().scene
    samples.append({"label":label,"actor":b.actor,"stage":b.stage,"sessionId":b.sessionId,
        "actors":b.actors.map(func(a): return {"id":a.id,"hp":a.hp,"mp":a.mp,"exp":a.exp,"x":a.x,"y":a.y}),
        "gold":b.gold,"mainSeed":b.mainSeed,"thinkingSeed":b.thinkingSeed,
        "scene":{"phase":s.phase,"spell":s.spell,"kind":s.actionKind,"reactionKind":s.reactionKind,
            "amount":s.reactionAmount,"message":s.message,"visible":s.visible,
            "displayedAlly":s.displayedAlly,"displayedEnemy":s.displayedEnemy,"attackSpell":s.attackSpell}})

func settle() -> Dictionary:
    for frame in range(5000):
        await process_frame
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false,"session/presenter error: " + str(current)); return {}
        var b := board()
        if b.stage == "Movement" and b.stopReason == "PlayerInput" and current.movement == null:
            return b
        var s: Dictionary = current.scene
        if s.healing != null:
            heal_projected = heal_projected or s.healing.Delivered
        if s.attackSpell != null:
            effect_draws = maxi(effect_draws, int(s.attackSpell.drawCount))
        var token = s.waitToken
        if token != null and not seen_tokens.has(token):
            seen_tokens[token] = true
            # HEAL's logical animation has many internal waits; sample finite action boundaries only.
            if s.phase in ["SpellCost","ResultMessage"] or s.phase == "ActionMessage" and s.healing == null:
                picture("automatic-" + str(s.phase))
            if s.phase == "SpellCost":
                paid.append(s.spell.Value)
                check(b.actors.filter(func(a): return a.id == ("mage" if s.spell.Value == "blaze" else "cleric"))[0].mp == (3 if s.spell.Value == "blaze" else 4),"actual MP cost")
            if s.phase == "ResultMessage":
                delivered.append(s.spell.Value)
                check(s.visible and s.reactionAmount > 0,"visible spell result")
                check(s.displayedAlly == ("scout" if s.spell.Value == "blaze" else null),"real faction projection")
                check(s.displayedEnemy == ("mage" if s.spell.Value == "blaze" else "wounded"),"actual enemy scene side")
                visible_text("visible-" + str(s.spell.Value))
        if s.phase in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
            await key(KEY_ENTER)
    check(false,"finite native bound"); return {}

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    await process_frame
    view = host.find_child("BattleSessionView",true,false)
    if view == null: check(false,"ordinary battle view"); finish(); return
    view.connect("SessionResultObserved",result_observed)
    var initial := await settle()
    if initial.is_empty(): finish(); return
    check(initial.actor == "scout","ordinary initial player control")
    picture("initial"); visible_text("visible-initial")
    await key(KEY_ENTER); await key(KEY_SPACE); await key(KEY_ENTER)
    var next := await settle()
    if next.is_empty(): finish(); return
    check(delivered == ["blaze","heal"],"both automatic spell results exactly once")
    check(paid == ["blaze","heal"],"both automatic spell costs exactly once")
    check(effect_draws > 0 and heal_projected,"actual fire and healing consumers")
    check(next.actor == "scout" and next.sessionId == initial.sessionId,"same-session returned player control")
    check(next.actors.filter(func(a): return a.id == "scout")[0].hp < 60,"actual enemy damage")
    check(next.actors.filter(func(a): return a.id == "wounded")[0].hp == 23,"actual same-faction healing")
    check(next.gold == initial.gold and next.actors.all(func(a): return a.exp == 0),"no enemy reward")
    check(events.filter(func(e): return e.Kind == "mp").size() == 2,"one payment per action")
    picture("returned-player")
    var old_x = next.previewX
    await key(KEY_RIGHT); await settle()
    check(board().previewX == old_x + 1,"subsequent ordinary movement")
    picture("usable-follow-up"); visible_text("visible-follow-up")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
        "scope":"authored source enemy BLAZE1/HEAL1 movement/payment/effect/end/player control; modern presentation; private binding separate",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close(); host.queue_free(); quit(0 if failures.is_empty() else 1)
