extends "res://probes/engine_exploration_observation.gd"

# Ordinary keys and actual host readback only. Compact semantic samples, no images.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var identity := ""
var full := false
var phases: Array = []

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            if event.Kind in ["rng-item-drop", "drop-once-set", "enemy-item-removed", "item-received", "item-to-deals", "item-discarded", "exp", "gold", "kills", "death-cleanup", "battle-outcome", "battle-initialized"]:
                events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    var battle := host.find_child("BattleSessionView", true, false)
    view = field if field != null and field.is_visible_in_tree() else battle
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String, state: Dictionary) -> void:
    var scene: Dictionary = state.get("scene", {})
    samples.append({"label":label, "sessionId":state.get("sessionId"), "revision":state.get("revision"),
        "gold":state.get("gold"), "mainSeed":state.get("mainSeed"), "custody":state.get("itemCustody"),
        "inventories":state.get("inventories", state.get("party")), "actors":state.get("actors"),
        "phase":scene.get("phase"), "token":scene.get("waitToken"), "message":scene.get("message"),
        "elapsed":scene.get("elapsed"), "completed":scene.get("completed"), "fieldDeath":scene.get("fieldDeath"),
        "wait":state.get("wait"), "failure":state.get("failure"), "presenterError":scene.get("error")})

func check_custody(state: Dictionary) -> void:
    _check(state.sessionId == identity, "same session retained")
    _check(state.itemCustody.OnceFlags == [24.0], "drop-once bit retained independently of story flag24")
    _check(state.itemCustody.Deals["110"] == (15.0 if full else 14.0), "expected independent Deals stock retained")
    _check(state.itemCustody.Deals["61"] == 3.0, "neighbor stock unaffected; sweep awards no drop")

func attack() -> void:
    _check(active().get("actor") == "leader", "living acting ally has real control")
    await _press(KEY_ENTER)
    await _press(KEY_F)
    for attempt in range(4):
        await _press(KEY_TAB)
        if active().get("target") == "enemy": break
    _check(active().get("target") == "enemy", "ordinary target selection")
    await _press(KEY_ENTER)

func return_to_field(pass_index: int) -> Dictionary:
    var mark := ""
    while Time.get_ticks_msec() - started < 170000:
        var state := active()
        if state.get("failure") != null or state.get("scene", {}).get("error") != null:
            sample("failure", state)
            _check(false, "actual session/presenter failure")
            return {}
        if state.has("scene"):
            var scene: Dictionary = state.scene
            var next_mark := str(scene.phase) + ":" + str(scene.waitToken)
            if next_mark != mark:
                mark = next_mark
                sample(str(pass_index) + ":" + mark, state)
                phases.append(scene.phase)
                check_custody(state)
                if scene.phase == "ItemDroppedMessage":
                    _check(not full and pass_index == 0 and scene.message == "enemy dropped Taros Sword.", "real committed drop message")
                if scene.phase == "ItemReceivedMessage":
                    _check(not full and pass_index == 0 and scene.message == "leader received Taros Sword.", "real committed receipt message")
                if scene.phase == "FieldSettle":
                    var enemy: Dictionary = state.actors.filter(func(a): return a.id == "enemy")[0]
                    var survivor: Dictionary = state.actors.filter(func(a): return a.id == "survivor")[0]
                    _check(not enemy.visible and not survivor.visible and survivor.hp == 20, "ordered removal retains sweep HP")
            if scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "ItemDroppedMessage", "ItemReceivedMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        elif state.get("wait") == "DialogueWait":
            await _press(KEY_ENTER)
        elif state.get("stop") == "PlayerInput":
            sample(str(pass_index) + ":returned-field", state)
            check_custody(state)
            return state
        else:
            await process_frame
    _check(false, "170-second bounded observation deadline")
    return {}

func _run() -> void:
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := active()
    sample("initial", initial)
    _check(initial.get("mode") == "Exploration" and initial.get("failure") == null, "ordinary authored startup")
    if not failures.is_empty():
        _finish()
        return
    identity = initial.sessionId
    await _press(KEY_ENTER)
    await _press(KEY_ENTER)
    var state := active()
    full = state.inventories.filter(func(a): return a.actor == "leader")[0].items == [0.0, 0.0, 0.0, 0.0]
    sample("first-control", state)
    await attack()
    sample("construction-committed", active())
    var returned := await return_to_field(0)
    if returned.is_empty():
        _finish()
        return
    var leader: Dictionary = returned.party.filter(func(a): return a.Actor.Value == "leader")[0]
    var expected_items: Array = [0.0, 0.0, 0.0, 0.0] if full else [110.0, 127.0, 127.0, 127.0]
    _check(leader.SourceLoadout.Items == expected_items and leader.Kills == 2 and returned.gold == 84, "live inventory/accounting reaches usable field")
    _check(phases.has("ItemDroppedMessage") == not full and phases.has("ItemReceivedMessage") == not full, "only direct receipt creates acknowledgement messages")
    _check(phases.has("FieldSpin") and phases.has("FieldExit") and phases.has("FieldSettle"), "actual field delivery waits observed")
    await _press(KEY_LEFT)
    for frame in range(600):
        await process_frame
        state = active()
        if state.get("stage") == "Movement" or state.get("failure") != null: break
    sample("walking-reentry", state)
    _check(state.get("stage") == "Movement", "ordinary walking enters the encounter again")
    if not failures.is_empty():
        _finish()
        return
    check_custody(state)
    _check(state.inventories.filter(func(a): return a.actor == "enemy")[0].items[0] == 494, "fresh enemy initial inventory")
    _check(state.inventories.filter(func(a): return a.actor == "leader")[0].items == expected_items, "live party inventory overrides start on reentry")
    await attack()
    var again := await return_to_field(1)
    _check(not again.is_empty(), "repeat kill returns without duplicate receipt")
    var rolls := events.filter(func(e): return e.Kind == "rng-item-drop")
    _check(rolls.size() == 2 and rolls.all(func(e): return e.RandomValue == 0), "both reached zero draws, including held once bit")
    for kind in ["drop-once-set", "enemy-item-removed", "item-to-deals" if full else "item-received"]:
        _check(events.filter(func(e): return e.Kind == kind).size() == 1, kind + " publishes exactly once")
    samples.append({"label":"events", "events":events, "elapsedMs":Time.get_ticks_msec()-started, "full":full})
    _finish()
