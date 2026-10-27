extends "res://probes/engine_exploration_observation.gd"

# Ordinary inputs only after the authored startup. Actual view/tokens/HP/events,
# no screenshots, state writes or fabricated presentation completion.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var phases: Dictionary = {}
var stages: Dictionary = {}
var sides: Dictionary = {}

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            if event.Kind in ["prism-laser", "rng-prism-damage", "hp", "defeats", "kills", "death-cleanup", "exp", "gold", "item-received", "field-death-ended", "player-control"]:
                events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, state: Dictionary) -> void:
    samples.append({"label":label, "revision":state.get("revision"), "round":state.get("round"),
        "actor":state.get("actor"), "mainSeed":state.get("mainSeed"), "thinkingSeed":state.get("thinkingSeed"),
        "gold":state.get("gold"), "credit":state.get("deathCredit"), "actors":state.get("actors"),
        "scene":state.get("scene"), "failure":state.get("failure")})

func stay() -> void:
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)

func move_up() -> void:
    await _press(KEY_UP)
    for frame in range(120):
        await process_frame
        var state: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
        if state.get("hasBattleControl", false): return
    _check(false, "ordinary movement finishes")

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.find_child("BattleSessionView", true, false)
    var initial := active()
    sample("initial", initial)
    _check(initial.get("actor") == "leader" and initial.get("failure") == null, "ordinary authored startup")
    if not failures.is_empty():
        _finish()
        return
    var thinking = initial.thinkingSeed
    await stay()
    await stay()
    var state := active()
    sample("off-ray-round", state)
    _check(state.round == 2 and state.actor == "leader", "off-ray automatic turn returns ordinary control")
    _check(events.filter(func(e): return e.Kind in ["prism-laser", "rng-prism-damage"]).is_empty(), "enemy-only ray stays without laser construction RNG")
    await move_up()
    await stay()
    await move_up()
    await stay()
    var mark := ""
    var returned := false
    while Time.get_ticks_msec() - started < 170000:
        state = active()
        var scene: Dictionary = state.get("scene", {})
        if state.get("failure") != null or scene.get("error") != null:
            sample("failure", state)
            _check(false, "session/presenter error")
            break
        if scene.get("phase") == null:
            if state.get("stage") == "Movement" and state.get("actor") == "leader":
                returned = true
                break
            await process_frame
            continue
        var laser = scene.get("laser")
        var stage: String = str(laser.stage) if laser != null else ""
        var next_mark := str(scene.phase) + ":" + str(scene.waitToken) + ":" + stage
        if next_mark != mark:
            mark = next_mark
            sample(mark, state)
            phases[scene.phase] = true
            if laser != null:
                stages[stage] = true
                _check(laser.targets == ["companion", "leader", "sentry"], "shot hits source-ordered allies then enemy")
                if laser.get("targetSide") != null and scene.phase == "Reaction": sides[laser.targetSide] = true
                if stage == "fire": _check(laser.beamVisible, "real authored beam projection visible")
            if scene.phase == "ActionMessage":
                _check(scene.message.contains("prism laser") and scene.actionKind == "prism-laser", "laser-specific announcement")
            if scene.phase in ["FieldSpin", "FieldExit", "FieldSettle"]:
                _check(scene.fieldDeath.actors == ["companion", "sentry"], "ordered field death worklist")
        if scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage"]:
            await _press(KEY_ENTER)
        else:
            await process_frame
    _check(returned, "shot completes to usable control")
    _check(stages.has("charge") and stages.has("fire") and stages.has("settle"), "actual finite charge/fire/settle delivered")
    _check(sides.has("Ally") and sides.has("Enemy"), "actual reactions change target side")
    _check(phases.has("FieldSpin") and phases.has("FieldExit") and phases.has("FieldSettle"), "actual death delivery completes")
    if returned:
        sample("returned", state)
        _check(state.deathCredit != null and state.deathCredit.Ally == null, "known-none scene credit")
        _check(state.gold == 73 and state.thinkingSeed == thinking, "gold and thinking RNG unchanged")
        var companion: Dictionary = state.actors.filter(func(a): return a.id == "companion")[0]
        var sentry: Dictionary = state.actors.filter(func(a): return a.id == "sentry")[0]
        var leader: Dictionary = state.actors.filter(func(a): return a.id == "leader")[0]
        _check(companion.hp == 0 and companion.defeats == 1 and not companion.visible, "ally defeat accounted and removed")
        _check(sentry.hp == 0 and not sentry.visible and leader.kills == 0, "enemy friendly fire removed without ally kill")
        var old_y = state.previewY
        await _press(KEY_DOWN)
        for frame in range(120):
            await process_frame
            state = active()
            if state.get("previewY") == old_y + 1: break
        sample("usable-movement", state)
        _check(state.previewY == old_y + 1 and state.failure == null, "ordinary movement works after laser")
    _check(events.filter(func(e): return e.Kind == "prism-laser").size() == 1, "one announcement/action")
    _check(events.filter(func(e): return e.Kind == "rng-prism-damage").size() == 6, "two construction draws per ordinary target")
    _check(events.filter(func(e): return e.Kind in ["kills", "exp", "gold", "item-received"]).is_empty(), "no physical awards")
    samples.append({"label":"events", "events":events, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
