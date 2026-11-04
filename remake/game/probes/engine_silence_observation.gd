extends "res://probes/engine_attack_spell_observation.gd"

# Ordinary device input and running host projection only. No session writes or synthetic delivery.
var support_paid := 0
var suppressed_paid := 0
var suppressed_message := false
var expiry_message := false
var support_results := 0

func picture(label: String) -> void:
    var b := board()
    var s: Dictionary = scene().scene
    samples.append({"label":label,"actor":b.actor,"stage":b.stage,"round":b.round,
        "sessionId":b.sessionId,"statuses":b.statuses,
        "actors":b.actors.map(func(a):return {"id":a.id,"hp":a.hp,"mp":a.mp,"exp":a.exp}),
        "scene":{"phase":s.phase,"spell":s.spell,"kind":s.actionKind,"message":s.message,
            "reactionKind":s.reactionKind,"actorCard":s.actorCard,"targetCard":s.targetCard}})

func silence(b: Dictionary, actor: String) -> int:
    return int(b.statuses.filter(func(a): return a.actor == actor)[0].status) & 0x300

func actor(b: Dictionary, id: String) -> Dictionary:
    return b.actors.filter(func(a):return a.id == id)[0]

func settle() -> Dictionary:
    for frame in range(1600):
        await process_frame
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false,"session/presenter error: " + str(current));return {}
        var b := board()
        if b.stage == "Movement" and b.stopReason == "PlayerInput" and current.movement == null:return b
        var s: Dictionary = current.scene
        if s.waitToken != null and not seen_tokens.has(s.waitToken):
            seen_tokens[s.waitToken] = true
            if s.phase in ["ActionMessage","SpellCost","ResultMessage","SilencedCastMessage","SilenceTurnMessage"]:
                picture("delivered-" + str(s.phase))
            if s.phase == "SpellCost" and s.supportSpell != null:
                support_paid += 1;check(actor(b,"smoke").mp == 0,"support paid exactly5")
            if s.phase == "SpellCost" and s.silencedSpell != null:
                suppressed_paid += 1;check(actor(b,"mage").mp == 18,"suppressed spell still paid2")
            if s.phase == "ResultMessage" and s.supportSpell != null:
                support_results += 1;check(s.visible,"support result visible");visible_text("visible-support")
            if s.phase == "SilencedCastMessage":
                suppressed_message = true
                check(str(s.message).contains("MP"),"localized cost-without-effect message")
                visible_text("visible-silenced-cast")
            if s.phase == "SilenceTurnMessage":
                expiry_message = true;visible_text("visible-expiry")
        if s.phase in ["ActionMessage","ResultMessage","SilencedCastMessage","SilenceMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
            await key(KEY_ENTER)
    check(false,"bounded native settle");return {}

func stay() -> Dictionary:
    await key(KEY_ENTER);await key(KEY_SPACE);await key(KEY_ENTER)
    return await settle()

func cast() -> Dictionary:
    await key(KEY_ENTER);await key(KEY_H)
    picture("magic-list");visible_text("visible-magic")
    await key(KEY_ENTER);await key(KEY_ENTER)
    return await settle()

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate();root.add_child(host)
    await process_frame
    view = host.find_child("BattleSessionView",true,false)
    if view == null:check(false,"ordinary battle view");finish();return
    view.connect("SessionResultObserved",result_observed)
    var initial := await settle()
    if initial.is_empty():finish();return
    check(initial.actor == "mage","initial player control")
    picture("initial")
    await stay()
    var b := await stay()
    if b.is_empty():finish();return
    check(b.actor == "mage" and silence(b,"mage") != 0,"actual support caused silence before player control")
    check(support_paid == 1 and support_results == 2,"one paid area spell, two target results")
    picture("silenced-control")
    # Selecting/cancelling still leaves the spell selectable and costs nothing.
    await key(KEY_ENTER);await key(KEY_H);await key(KEY_ESCAPE);await key(KEY_ESCAPE)
    check(actor(board(),"mage").mp == 20 and silence(board(),"mage") != 0,"cancel is free while silenced")
    b = await cast()
    if b.is_empty():finish();return
    check(suppressed_paid == 1 and suppressed_message,"real paid silence gate")
    check(actor(b,"smoke").hp == 80 and actor(b,"mage").exp == 0,"no damage or EXP from suppressed cast")
    for turn in range(8):
        if b.actor == "mage" and silence(b,"mage") == 0:break
        b = await stay()
        if b.is_empty():finish();return
    check(b.actor == "mage" and silence(b,"mage") == 0 and expiry_message,"real lifecycle recovered casting")
    picture("recovered-caster")
    b = await cast()
    if b.is_empty():finish();return
    check(actor(b,"smoke").hp < 80 and actor(b,"mage").mp == 16,"subsequent spell now has effect")
    check(b.sessionId == initial.sessionId,"same authoritative session")
    var x = b.previewX
    await key(KEY_LEFT);await settle()
    check(board().previewX == x - 1,"subsequent ordinary movement input")
    picture("usable-follow-up");visible_text("visible-follow-up")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
        "scope":"authored SUPPORT area payment/status, marked spell paid suppression, expiry and effective later cast/input; private/story separate",
        "elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    output.close();host.queue_free();quit(0 if failures.is_empty() else 1)
