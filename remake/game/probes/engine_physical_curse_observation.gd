extends SceneTree

# Ordinary key input and read-only projection from a controlled authored start.
var host: Node
var view: Node
var samples: Array = []
var events: Array = []
var failures: Array = []
var last_token = null
var mode := OS.get_environment("SF2_CURSE_CASE")
var started := Time.get_ticks_msec()

func _initialize() -> void:
    call_deferred("run")

func check(value: bool, failure: String) -> void:
    if not value: failures.append(failure)

func input(action: String) -> void:
    var keys := InputMap.action_get_events("sf2_" + action).filter(func(event): return event is InputEventKey)
    var event: InputEventKey = keys[0].duplicate()
    event.pressed = true
    Input.parse_input_event(event)
    for frame in range(2): await process_frame
    event.pressed = false
    Input.parse_input_event(event)
    for frame in range(2): await process_frame

func state() -> Dictionary:
    return JSON.parse_string(view.call("ReadSceneObservationJson"))

func actor(snapshot: Dictionary, id: String) -> Dictionary:
    return snapshot.actors.filter(func(unit): return unit.actor == id)[0]

func observe(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if event.Kind.begins_with("rng-curse") or event.Kind in [
            "curse-inaction", "curse-recoil", "physical-first", "physical-second", "physical-counter",
            "action-committed", "exp", "death-cleanup"
        ]: events.append(event)

func run() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    # The headless display reports no usable desktop. Resize the test window itself.
    root.size = Vector2i(1920, 1080)
    for frame in range(180):
        await process_frame
        view = host.find_child("BattleSessionView", true, false)
        if view != null and view.is_visible_in_tree(): break
    if view == null:
        var entry := host.find_child("SessionEntryView", true, false)
        var failure = entry.call("ReadEntryObservationJson") if entry != null else "no entry projection"
        check(false, "ordinary battle view unavailable: " + str(failure))
        finish()
        return
    view.connect("SessionResultObserved", observe)
    var initial := state()
    if initial.failure != null or initial.actor != "swordsman":
        check(false, "initial player control: " + str(initial.failure))
        finish()
        return
    await input("confirm")
    await input("attack")
    for press in range(4):
        if state().scene.phase != null: break
        await input("confirm")

    var returned := false
    while Time.get_ticks_msec() - started < 20000:
        var live := state()
        var shown: Dictionary = live.scene
        if live.failure != null or shown.error != null:
            check(false, "native scene failure: " + str(live.failure) + "/" + str(shown.error))
            break
        if shown.waitToken != null and shown.waitToken != last_token:
            last_token = shown.waitToken
            samples.append({"phase":shown.phase, "kind":shown.actionKind, "reaction":shown.reactionKind,
                "message":shown.message, "ally":shown.displayedAlly, "enemy":shown.displayedEnemy,
                "hp":actor(live, "swordsman").hp, "enemyHp":actor(live, "raider").hp})
        if shown.requiresAcknowledgement == true:
            await input("confirm")
        elif not samples.is_empty() and live.hasBattleControl and live.movement == null:
            returned = true
            break
        else:
            await process_frame

    var final := state()
    check(returned and final.actor == "lookout", "next ally owns ordinary input")
    check(events.filter(func(event): return event.Kind == "action-committed").size() == 1, "one action publication")
    check(not events.any(func(event): return event.Kind == "exp"), "no experience for inaction or lethal recoil")
    if mode == "inaction":
        check(samples.any(func(sample): return (sample.phase == "PhysicalInactionMessage" and
            sample.kind == "curse-inaction" and "curse" in sample.message and sample.enemy == "raider")), "inaction projected")
        check(not samples.any(func(sample): return sample.phase == "ActionAnimation"), "no strike animation")
        check(actor(final, "swordsman").hp == 35 and actor(final, "raider").hp == 60, "inaction keeps both HP")
    elif mode == "lethal":
        check(samples.any(func(sample): return (sample.phase == "ResultMessage" and sample.reaction == "CurseRecoil"
            and "4 damage" in sample.message and sample.ally == "swordsman" and sample.enemy == "raider"
            and sample.hp == 0 and sample.enemyHp == 32)), "self damage and both participants projected")
        check(samples.any(func(sample): return (sample.phase == "DeathMessage" and
            sample.kind == "curse-recoil" and "can no longer fight" in sample.message)), "lethal recoil death message")
        check(samples.any(func(sample): return sample.phase == "FieldSpin"), "normal field death delivery")
        check(events.filter(func(event): return event.Kind == "death-cleanup").size() == 1, "one death cleanup")
        check(actor(final, "swordsman").hp == 0 and actor(final, "swordsman").defeats == 1, "lethal accounting")
    else:
        check(false, "case must be inaction or lethal")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"case":mode, "passed":failures.is_empty(), "failures":failures,
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
