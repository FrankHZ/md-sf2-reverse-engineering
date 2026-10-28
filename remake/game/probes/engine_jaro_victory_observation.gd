extends "res://probes/engine_exploration_observation.gd"

# One ordinary win, visible recruitment, reserve return and usable field input.
var events: Array = []
var last_sequence := -1
var started := Time.get_ticks_msec()
var seen_texts: Dictionary = {}

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)

func result_observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) > last_sequence:
            events.append(event)
            last_sequence = int(event.Sequence)

func active() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    var battle := host.find_child("BattleSessionView", true, false)
    view = field if field != null and field.is_visible_in_tree() else battle
    return JSON.parse_string(view.call("ReadObservationJson"))

func jaro(state: Dictionary) -> Dictionary:
    return state.party.filter(func(a): return a.Actor.Value == "ally-jaro")[0]

func _run() -> void:
    root.size = Vector2i(1280, 720)
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := active()
    _read("before-battle")
    _check(initial.get("wait") == "DialogueWait" and initial.get("failure") == null, "admitted before-battle dialogue")
    if not failures.is_empty():
        _finish()
        return
    var identity: String = initial.sessionId
    for frame in range(180):
        var shown := active()
        if shown.visibleCharacters < 0 or shown.visibleCharacters >= shown.totalCharacters: break
        await process_frame
    await _press(KEY_ENTER)
    var state := active()
    _check(state.get("actor") == "leader" and state.get("stage") == "Movement", "ordinary player battle control")
    if not failures.is_empty():
        _finish()
        return
    _read("battle-start")
    await _press(KEY_ENTER)
    await _press(KEY_F)
    for attempt in range(4):
        await _press(KEY_TAB)
        if active().get("target") == "enemy": break
    _check(active().get("target") == "enemy", "ordinary target selection")
    await _press(KEY_ENTER)
    var returned: Dictionary = {}
    while Time.get_ticks_msec() - started < 170000:
        state = active()
        if state.get("failure") != null:
            _check(false, "session failure: " + str(state.failure))
            break
        if state.has("scene"):
            var scene: Dictionary = state.scene
            if scene.get("error") != null:
                _check(false, "presenter failure: " + str(scene.error))
                break
            if scene.phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        elif state.get("wait") == "DialogueWait":
            var text_id := int(state.textId)
            if not seen_texts.has(text_id):
                # Wait for actual label reveal before recording the visible ceremony.
                if state.visibleCharacters >= 0 and state.visibleCharacters < state.totalCharacters:
                    await process_frame
                    continue
                seen_texts[text_id] = true
                _read("recruitment:" + str(text_id))
                _check(23.0 in state.flags and 55.0 in state.flags, "joined and active before leave")
                _check(jaro(state).Hp == 24 and jaro(state).Mp == 3, "ordinary victory recovery")
                _check(jaro(state).Exp == 17 and jaro(state).Kills == 3 and jaro(state).Defeats == 2, "retained actor accounting")
                _check(jaro(state).SourceLoadout.Items == [0.0,127.0,127.0,127.0], "retained live inventory")
                if text_id == 101:
                    _check(state.dialogue == "Jaro has joined the force.", "visible authored recruitment")
                    _check(jaro(state).ActivationWord == 32773, "AI clear waits for recruitment acknowledgement")
                    var revision = state.revision
                    view.hide()
                    await _press(KEY_ENTER)
                    for frame in range(12): await process_frame
                    _check(JSON.parse_string(view.call("ReadObservationJson")).revision == revision, "hidden field view preserves dialogue ownership")
                    view.show()
                elif text_id == 103:
                    _check(state.dialogue == "Thank you. I will stand ready in reserve.", "visible authored reply")
                    _check(jaro(state).ActivationWord == 32769, "only AI bit2 cleared")
                    _check(23.0 in state.partyLists.Active, "zero AI operation did not rebuild lists")
                else:
                    _check(false, "unexpected outcome dialogue")
            await _press(KEY_ENTER)
        elif state.get("stop") == "PlayerInput":
            returned = state
            break
        else:
            await process_frame
    _check(not returned.is_empty(), "bounded usable return")
    _check(seen_texts.has(101) and seen_texts.has(103), "both actual authored text waits consumed")
    if not returned.is_empty():
        _read("returned-field")
        _check(returned.sessionId == identity and returned.map == "arena", "same-session saved return")
        _check(532.0 in returned.flags and not 432.0 in returned.flags, "completion flags")
        _check(23.0 in returned.flags and not 55.0 in returned.flags, "joined reserve membership")
        _check(23.0 in returned.partyLists.Reserve and not 23.0 in returned.partyLists.Active, "later join0 rebuilt reserve list")
        _check(jaro(returned).ActivationWord == 32769 and jaro(returned).Hp == 24, "retained final actor")
        var prior = returned.entities[0]
        await _press(KEY_LEFT)
        for frame in range(80): await process_frame
        state = active()
        _check(state.get("failure") == null and state.entities[0].x == prior.x - 384 and state.entities[0].y == prior.y, "ordinary field movement after return")
        _read("continued-control")
    for detail in ["JoinPartyMember", "ClearOutcomePartyAi", "LeaveOutcomeBattleParty"]:
        _check(events.filter(func(e): return e.get("Detail") == detail).size() == 1, "exactly one " + detail)
    for kind in ["after-battle-join", "battle-unlock-cleared", "battle-completed-set"]:
        _check(events.filter(func(e): return e.Kind == kind).size() == 1, "exactly one " + kind)
    samples.append({"label":"ordered-events", "events":events, "elapsedMs":Time.get_ticks_msec()-started})
    _finish()
