extends SceneTree

var samples: Array = []
var failures: Array = []
var host: Node
var view: Node

func _initialize() -> void:
    call_deferred("_run")

func _check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func _read(label: String) -> Dictionary:
    var state: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    samples.append({"label": label, "state": state})
    return state

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _run() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        _check(false, "Ordinary host starts an exploration view")
        _finish()
        return
    var initial := _read("field-input")
    _check(initial.mode == "Exploration" and initial.stop == "PlayerInput", "Authored field input is available")
    var identity: String = initial.sessionId
    await _press(KEY_ENTER)
    var dialogue := _read("dialogue")
    _check(dialogue.wait == "DialogueWait" and not dialogue.dialogue.is_empty(), "Real interaction presents authored dialogue")
    await _press(KEY_ENTER)
    _check(_read("choice").wait == "ChoiceWait", "Dialogue acknowledgement reaches the choice")
    await _press(KEY_X)
    var declined := _read("declined")
    _check(declined.stop == "PlayerInput" and declined.flags.is_empty(), "Declining returns field control without acceptance flags")
    await _press(KEY_ENTER)
    await _press(KEY_ENTER)
    await _press(KEY_Z)
    var accepted := _read("motion-started")
    _check(accepted.wait == "EntityWait", "Accepting installs actual entity motion and waits")
    var state := accepted
    for _frame in range(240):
        await process_frame
        state = JSON.parse_string(view.call("ReadObservationJson"))
        if state.wait == "DialogueWait" or state.failure != null:
            break
    state = _read("before-battle-dialogue")
    _check(state.failure == null and state.wait == "DialogueWait", "Motion, timer, call, map init and transfer reach the before-battle dialogue")
    _check(14.0 in state.flags and 15.0 in state.flags and not 20.0 in state.flags, "Map initialization and before-battle precede the intro flag setter")
    await _press(KEY_ENTER)
    view = host.get_node("BattleSessionView")
    var battle := _read("first-battle-control")
    _check(battle.failure == null and battle.round == 1 and battle.stage == "Movement", "The same host reaches ordinary first-round battle input")
    _check(battle.sessionId == identity and battle.storyFlags == [10.0, 12.0, 13.0, 14.0, 15.0, 16.0, 20.0], "Session identity and flags survive mode transition")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var continued := _read("battle-continued")
    _check(continued.failure == null and continued.sessionId == identity, "Existing battle commands continue the same session")
    _finish()

func _finish() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    if output.is_empty():
        push_error("External observer requires an output path")
        quit(2)
        return
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures, "samples": samples}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
