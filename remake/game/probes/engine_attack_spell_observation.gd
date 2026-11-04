extends "res://probes/engine_equipment_observation.gd"

# Ordinary host menu/target/confirm inputs, existing read-only projection and events.
# This is a bounded delivery witness, not source trajectory or original effect capture.
var hit_targets: Array = []
var effect_draws := 0
var payment_checks := 0
var kind := OS.get_environment("SF2_ATTACK_SPELL_KIND")

func picture(label: String) -> void:
    var b := board()
    var projection: Dictionary = scene().scene
    samples.append({"label":label, "actor":b.actor, "stage":b.stage, "sessionId":b.sessionId,
        "actors":b.actors.map(func(a): return {"id":a.id, "hp":a.hp, "mp":a.mp, "exp":a.exp,
            "kills":a.kills, "level":a.level, "x":a.x, "y":a.y, "visible":a.visible}),
        "gold":b.gold, "mainSeed":b.mainSeed, "thinkingSeed":b.thinkingSeed,
        "menu":b.menu if label in ["magic-list", "selected-target"] else null,
        "scene":{"phase":projection.phase, "message":projection.message, "attackSpell":projection.attackSpell,
            "actorCard":projection.actorCard, "targetCard":projection.targetCard, "messageFont":projection.messageFont}})

func visible_text(label: String) -> void:
    var labels: Array = []
    for n in host.find_children("*", "Control", true, false):
        if not n.is_visible_in_tree() or not (n is Label or n is Button): continue
        var font: Font = n.get_theme_font("font")
        for i in range(n.text.length()):
            if n.text.unicode_at(i) > 32: check(font.has_char(n.text.unicode_at(i)), "visible glyph " + str(n.text.unicode_at(i)))
        check(n.get_minimum_size().y <= n.size.y + 1, "visible minimum height " + str(n.name))
        check(Rect2(Vector2.ZERO, Vector2(root.size)).encloses(n.get_global_rect()), "visible viewport " + str(n.name))
        labels.append({"node":str(n.name), "text":n.text})
    samples.append({"label":label, "labels":labels})

func settle() -> Dictionary:
    for frame in range(2500):
        await process_frame
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false, "session/presenter error: " + str(current))
            return {}
        var state := board()
        if state.stage == "Movement" and state.stopReason == "PlayerInput" and current.movement == null:
            return state
        var s: Dictionary = current.scene
        var token = s.waitToken
        if token != null and not seen_tokens.has(token):
            seen_tokens[token] = true
            picture("scene-" + str(s.phase))
            var effect = s.get("attackSpell")
            if effect != null:
                effect_draws = maxi(effect_draws, int(effect.drawCount))
                var expected: Array = ["wing", "center"] if kind == "blaze" else ["center"]
                check(effect.targets == expected, "live ordered spell targets")
                check(effect.element == ("Fire" if kind == "blaze" else "Wind"), "distinct fire/wind projection")
                if s.phase == "ResultMessage":
                    hit_targets.append(effect.target)
                    check(effect.visible and s.reactionKind == "Damage" and s.reactionAmount > 0, "visible damaging effect/result")
                    check(str(s.message).contains("damage") if kind == "blaze" else str(s.message).contains("伤害"), "localized damage message")
                    visible_text("visible-damage-result")
                if s.phase == "SpellCost":
                    var caster = state.actors.filter(func(a): return a.id == "mage")[0]
                    check(caster.mp == (14 if kind == "blaze" else 18), "real one-time MP payment")
                    payment_checks += 1
            if s.phase == "ActionMessage":
                check(str(s.message).contains("BLAZE") if kind == "blaze" else str(s.message).contains("BLAST"), "visible selected spell announcement")
        if s.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage", "ItemDroppedMessage", "ItemReceivedMessage"]:
            await key(KEY_ENTER)
    check(false, "finite native settle bound")
    return {}

func run() -> void:
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    await process_frame
    view = host.find_child("BattleSessionView", true, false)
    if view == null: check(false, "ordinary host battle view"); finish(); return
    view.connect("SessionResultObserved", result_observed)
    var initial := await settle()
    if initial.is_empty(): finish(); return
    check(initial.actor == "mage", "learned caster has ordinary control")
    picture("initial")
    await key(KEY_ENTER); await key(KEY_H)
    picture("magic-list"); visible_text("visible-magic-list")
    await key(KEY_ESCAPE); await key(KEY_ESCAPE)
    check(board().actors == initial.actors and board().mainSeed == initial.mainSeed, "cancel keeps resources and RNG")
    await key(KEY_ENTER); await key(KEY_H); await key(KEY_DOWN)
    if kind == "blast": await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(board().target == "center" and board().stage == "CommitReady", "ordinary legal center selection")
    picture("selected-target"); visible_text("visible-target")
    await key(KEY_ENTER)
    var next := await settle()
    if next.is_empty(): finish(); return
    check(payment_checks == 1 and effect_draws > 0, "one payment and actual effect drawing")
    check(hit_targets == (["wing", "center"] if kind == "blaze" else ["center"]), "all target results delivered once")
    check(next.actor == "guard" and next.stage == "Movement" and next.sessionId == initial.sessionId, "same-session usable next actor")
    var caster = next.actors.filter(func(a): return a.id == "mage")[0]
    var dead = next.actors.filter(func(a): return a.id == "center")[0]
    check(caster.mp == (14 if kind == "blaze" else 18) and caster.exp > 0, "persistent MP and EXP")
    check(dead.hp == 0 and next.gold == 107, "spell death and gold accounting")
    check(events.filter(func(e): return e.Kind == "mp").size() == 1, "one MP publication")
    check(events.filter(func(e): return e.Kind == "exp").size() == 1, "one EXP publication")
    picture("completed-action-and-control")
    var old_x = next.previewX
    await key(KEY_RIGHT); await settle()
    check(board().previewX == old_x + 1, "fresh ordinary movement after spell")
    picture("usable-follow-up"); visible_text("visible-follow-up")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"authored player attack spell menu/payment/damage/death/reward/control; modern presentation; private admission separate",
        "kind":kind, "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close(); host.queue_free(); quit(0 if failures.is_empty() else 1)
