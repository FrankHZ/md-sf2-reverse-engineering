extends SceneTree

# Ordinary host keys and read-only state/projection. No session setters, synthetic
# completions, screenshots, source assets or original-game trajectory claim.
var host: Node
var view: Node
var failures: Array = []
var events: Array = []
var samples: Array = []
var seen_tokens: Dictionary = {}
var started := Time.get_ticks_msec()
var last_sequence := 0

func _initialize() -> void:
    call_deferred("run")

func check(value: bool, message: String) -> void:
    if not value: failures.append(message)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if int(event.Sequence) > last_sequence:
            events.append(event)
            last_sequence = int(event.Sequence)

func board() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func scene() -> Dictionary:
    return JSON.parse_string(view.call("ReadSceneObservationJson"))

func key(code: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = code
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func sample(label: String) -> void:
    var state := board()
    samples.append({"label":label, "actor":state.actor, "stage":state.stage,
        "mainSeed":state.mainSeed, "thinkingSeed":state.thinkingSeed,
        "previewX":state.previewX, "previewY":state.previewY, "gold":state.gold,
        "actors":state.actors, "inventories":state.inventories, "scene":scene()})

func settle() -> Dictionary:
    for frame in range(10000):
        await process_frame
        if Time.get_ticks_msec() - started > 170000:
            check(false, "native observation deadline")
            return {}
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false, "native session/presenter error:" + str(current))
            return {}
        var token = current.scene.waitToken
        if token != null and not seen_tokens.has(token):
            seen_tokens[token] = true
            samples.append({"label":"scene-boundary", "state":current})
        var state := board()
        if state.stage != null and state.stopReason == "PlayerInput" and current.movement == null:
            return state
        if current.scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            await key(KEY_ENTER)
    check(false, "settle bound")
    return {}

func attack(expected_actor: String) -> Dictionary:
    var state := board()
    check(state.actor == expected_actor and state.stage == "Movement", "usable actor before ATTACK:" + expected_actor)
    await key(KEY_W)
    state = await settle()
    if state.is_empty(): return {}
    check(state.previewY == 2, "ordinary movement preview reaches adjacent row")
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "ordinary movement confirmation")
    await key(KEY_F)
    check(board().stage == "TargetChoice", "ordinary ATTACK selection")
    await key(KEY_TAB)
    check(board().target == "guardian" and board().stage == "CommitReady", "ordinary target selection")
    sample("before-confirm-" + expected_actor)
    await key(KEY_ENTER)
    state = await settle()
    if not state.is_empty(): sample("after-delivery-" + expected_actor)
    return state

func run() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        check(false, "ordinary battle view exists")
        finish()
        return
    view.connect("SessionResultObserved", result_observed)
    var initial := await settle()
    if initial.is_empty():
        finish()
        return
    sample("initial-controlled-authored-package")
    var after_wrong := await attack("apprentice")
    if after_wrong.is_empty():
        finish()
        return
    var guardian: Dictionary = after_wrong.actors.filter(func(a): return a.id == "guardian")[0]
    check(guardian.hp == 500, "ineffective attack preserves guardian HP")
    check(after_wrong.gold == initial.gold, "ineffective action preserves gold")
    check(after_wrong.actors.all(func(a): return a.status == 0), "ordinary critical profile has no invented sleep")
    var ineffective := samples.filter(func(s): return s.get("state",{}).get("scene",{}).get("phase") == "ResultMessage" and s.state.scene.reactionKind == "Ineffective")
    check(not ineffective.is_empty(), "actual ineffective result projection exists")
    check(ineffective.any(func(s): return str(s.state.scene.message).contains("ineffective") and s.state.scene.visible and s.state.scene.reactionAmount == 0), "actual visible ineffective message")
    var split := events.size()
    check(events.any(func(e): return e.Kind == "ineffective"), "ineffective delivery observation")
    check(not events.any(func(e): return e.Kind.begins_with("rng-reaction-")), "ineffective scene has no damage recoil draws")
    var after_right := await attack("champion")
    if after_right.is_empty():
        finish()
        return
    guardian = after_right.actors.filter(func(a): return a.id == "guardian")[0]
    check(guardian.hp < 500 and guardian.hp > 0, "equipped bound actor deals normal damage")
    check(events.slice(split).any(func(e): return e.Kind == "hp" and e.Actor.Value == "guardian"), "actual effective HP delivery")
    check(events.slice(split).any(func(e): return e.Kind.begins_with("rng-reaction-")), "effective damage uses normal reaction RNG")
    check(after_right.stage == "Movement" and after_right.stopReason == "PlayerInput", "both scenes return to usable battle control")
    # Actual next movement and cancellation prove control is usable after delivery.
    var before_cancel := board()
    await key(KEY_S)
    await settle()
    await key(KEY_ESCAPE)
    var cancelled := await settle()
    check(not cancelled.is_empty() and cancelled.mainSeed == before_cancel.mainSeed and cancelled.thinkingSeed == before_cancel.thinkingSeed, "post-scene movement/cancel preserves seeds")
    sample("usable-control-after-both-results")
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "scope":"authored Taros physical effectiveness; ordinary keyboard and actual semantic feedback; no original Battle18 parity",
        "elapsedMs":Time.get_ticks_msec()-started, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
