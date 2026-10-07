extends "res://probes/engine_exploration_observation.gd"

# Ordinary startup/input and existing view readback only. Expectations never select game rules.
var facts: Array = []
var last_sequence := -1
var victory := true
var authored := false
var deadline: int

func _observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", _result):
        node.connect("SessionResultObserved", _result)

func _result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            facts.append(event)
            last_sequence = int(event.Sequence)

func _active() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    var battle := host.find_child("BattleSessionView", true, false)
    view = field if field != null and field.is_visible_in_tree() else battle
    return JSON.parse_string(view.call("ReadObservationJson"))

func _run() -> void:
    deadline = Time.get_ticks_msec() + 120000
    victory = OS.get_environment("SF2_OBSERVATION_EXPECT_OUTCOME") == "victory"
    authored = OS.get_environment("SF2_OBSERVATION_EXPECT_POLICY") == "authored"
    node_added.connect(_observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := _active()
    _check(initial.get("failure") == null and initial.get("mode") == "Exploration", "Ordinary authored field startup")
    if not failures.is_empty():
        _finish()
        return
    _read("field-start")
    var identity: String = initial.sessionId
    await _press(KEY_ENTER)
    var before := _active()
    _check(before.get("wait") == "DialogueWait", "Real field interaction reaches before-battle dialogue")
    _read("before-battle")
    await _press(KEY_ENTER)
    var battle := _active()
    _check(battle.get("actor") == "leader" and battle.sessionId == identity, "Same session reaches leader control")
    _check(battle.mainSeed == (3361018420 if victory else 1182405172), "Independent first-round seed")
    _read("battle-start")
    await _press(KEY_ENTER)
    await _press(KEY_F if victory else KEY_SPACE)
    if victory:
        await _press(KEY_TAB)
    _read("action-selected")
    await _press(KEY_ENTER)
    var phase := ""
    var seen_reward := false
    var seen_growth := false
    var seen_outcome := false
    var returned: Dictionary = {}
    while Time.get_ticks_msec() < deadline:
        var state := _active()
        if state.get("failure") != null:
            _check(false, "Actual consumer failure: " + str(state.failure))
            _read("failure")
            break
        if state.has("scene"):
            var scene: Dictionary = state.scene
            var mark: String = str(scene.phase) + ":" + str(scene.waitToken)
            if mark != phase:
                phase = mark
                _read("battle:" + mark)
            if scene.phase == "RewardMessage":
                seen_reward = true
                _check(state.actors[0].level == 1 and state.actors[0].exp == (106 if authored else 148),
                    "EXP credits once before growth, while level stays1")
                _check(scene.message.contains(str(7 if authored else 49)), "Actual reward label contains selected award")
                _check(state.mainSeed == (147919412 if authored else 3529183796), "Growth awaits the post-reaction live seed")
            if scene.phase == "GrowthMessage":
                seen_growth = true
                _check(state.actors[0].level == 2 and state.actors[0].exp == (6 if authored else 48)
                    and state.actors[0].maxHp == 41 and state.actors[0].maxMp == 1
                    and state.actors[0].attack == 31 and state.actors[0].defense == 5,
                    "Actual growth publishes independently derived five +1 gains")
                _check(state.actors[0].hp == 10 and state.actors[0].mp == 0, "Growth does not heal current resources")
                _check(state.mainSeed == (2535658036 if authored else 3330085428), "Ten live growth draws carry the expected seed")
            if scene.visible and scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        else:
            var mark: String = str(state.get("continuation")) + ":" + str(state.get("token"))
            if mark != phase:
                phase = mark
                _read("field:" + mark)
            if state.get("wait") == "DialogueWait":
                seen_outcome = true
                _check(state.dialogue.contains("Victory" if victory else "Defeat"), "Actual outcome program dialogue is consumed")
                if victory:
                    _check(state.party[0].Hp == 41 and state.party[1].Hp == 0, "Victory initially recovers living party only")
                await _press(KEY_ENTER)
            elif state.get("stop") == "PlayerInput":
                returned = state
                break
            else:
                await process_frame
    _check(not returned.is_empty(), "Actual program and presenter return within the bounded deadline")
    if not returned.is_empty():
        _check(seen_outcome and (not victory or seen_reward and seen_growth), "Required actual messages were reached")
        _check(returned.sessionId == identity and returned.map == ("arena" if victory else "quay"), "Same-session selected map return")
        _check(returned.gold == (84 if victory else 63 if authored else 36), "Actual returned gold follows selected policy")
        _check((501.0 in returned.flags) == victory and (1.0 in returned.flags) == victory, "Only victory publishes completion and join")
        _check(returned.party[0].Hp == (41 if victory else 40) and returned.party[1].Hp == 12, "Return recovers leader and admitted party")
        _check(returned.party[0].Kills == (1 if victory else 0) and returned.party[0].Defeats == (0 if victory else 1), "Death accounting is published exactly once")
        _check(returned.mainSeed == ((2535658036 if authored else 3330085428) if victory else 2930119220), "Returned seed retains completed action and growth")
        _check(returned.enteringBattle == null and returned.callers.is_empty(), "No outcome program or caller remains")
        _read("returned-input")
        await process_frame
        var stable := _active()
        _read("returned-stable")
        _check(stable.revision == returned.revision and stable.mainSeed == returned.mainSeed, "Returned control is stable")
        await _press(KEY_LEFT)
        for frame in range(120):
            await process_frame
            stable = _active()
            if stable.get("stop") == "PlayerInput" and stable.get("wait") == null:
                break
        _read("returned-moved")
        _check(stable.revision > returned.revision and stable.gold == returned.gold and stable.failure == null,
            "Real directional input moves after the outcome without changing accounting")
        _check(stable.entities[0].x == returned.entities[0].x - 384 and stable.entities[0].y == returned.entities[0].y,
            "Actual player projection moves one tile west")
        var ordered: Array = []
        var included := ["exp", "level", "kills", "defeats", "outcome-program-started", "after-battle-join", "battle-unlock-cleared", "battle-completed-set", "defeat-recovered", "battle-returned"]
        for event in facts:
            if event.Kind in included:
                ordered.append(event.Kind)
        var expected := ["exp", "level", "kills", "outcome-program-started", "after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned"] if victory else ["defeats", "outcome-program-started", "defeat-recovered", "battle-returned"]
        _check(ordered == expected, "Observed publication order has no repeated reward or outcome tail")
    samples.append({"label": "ordered-result-events", "events": facts})
    _finish()
