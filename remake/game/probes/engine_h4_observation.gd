extends SceneTree

# External ordinary-input observer. The plan is never consumed by production.
# Signals retain every result; ReadObservationJson supplies live projection fields.
var host: Node
var output: FileAccess
var sequence := 0
var input_ordinal := 0
var logical_step := 0
var session_id: Variant = null
var last_state := ""
var failure := ""
var acknowledge_dialogue := false
var choices: Array = []
var choice_index := 0
var seen_texts: Dictionary = {}
var result_signals := 0
var observed_nodes: Array[Node] = []

func result_observed(raw: String) -> void:
    result_signals += 1
    sequence += 1
    var result: Dictionary = JSON.parse_string(raw)
    output.store_line(JSON.stringify({"sequence":sequence,"result":result, "inputOrdinal":input_ordinal}))
    output.flush()
    if result.get("failure") != null: failure = "session-result-failure"

func observe_node(node: Node) -> void:
    # Optional accepted-host interface; older hosts remain explicitly sparse.
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_observed):
        node.connect("SessionResultObserved", result_observed)
        observed_nodes.append(node)

func stop_observing() -> void:
    node_added.disconnect(observe_node)
    for node in observed_nodes:
        if is_instance_valid(node) and node.is_connected("SessionResultObserved", result_observed):
            node.disconnect("SessionResultObserved", result_observed)

func _initialize() -> void:
    call_deferred("run")

func state(label: String = "") -> Dictionary:
    var view := host.get_node_or_null("ExplorationSessionView")
    if view == null: view = host.get_node_or_null("BattleSessionView")
    if view == null:
        failure = "actual-view-unavailable"
        return {}
    var raw: String = view.call("ReadObservationJson")
    var s: Dictionary = JSON.parse_string(raw)
    if raw != last_state or not label.is_empty():
        sequence += 1
        output.store_line(JSON.stringify({"sequence":sequence, "inputOrdinal":input_ordinal,"logicalStep":logical_step,
            "label":label, "state":s}))
        output.flush()
        last_state = raw
    if session_id == null: session_id = s.get("sessionId")
    elif session_id != s.get("sessionId"): failure = "session-changed"
    if s.get("failure") != null: failure = "host:" + str(s.failure)
    return s

func player(s: Dictionary) -> Dictionary:
    for entity in s.get("entities", []):
        if entity.id == "entity-0": return entity
    return {}

func tick() -> Dictionary:
    await process_frame
    return state()

func press(action: String) -> void:
    var code: int = {"Left":KEY_LEFT,"Right":KEY_RIGHT,"Up":KEY_UP,"Down":KEY_DOWN,
        "Confirm":KEY_ENTER,"Cancel":KEY_ESCAPE,"Stay":KEY_SPACE}.get(action, 0)
    if code == 0:
        failure = "unmapped-logical-input:" + action
        return
    input_ordinal += 1
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = true
    Input.parse_input_event(event)
    state("input-pressed:" + action)
    await tick()
    event = InputEventKey.new()
    event.keycode = code
    event.pressed = false
    Input.parse_input_event(event)
    state("input-released:" + action)

func settle(boundary: String) -> Dictionary:
    for count in range(5000):
        var s := await tick()
        if failure != "": return s
        if s.get("stage") != null: return s
        if s.get("wait") in ["DialogueWait", "ChoiceWait"]:
            if not acknowledge_dialogue: return s
            if not seen_texts.has(s.token):
                seen_texts[s.token] = true
                state("dialogue" if s.wait == "DialogueWait" else "choice")
            if s.wait == "ChoiceWait":
                if choice_index >= choices.size():
                    failure = "original-choice-unavailable"
                    return s
                await press("Confirm" if choices[choice_index].meaning == "Yes" else "Cancel")
                choice_index += 1
            else:
                await press("Confirm")
            continue
        if boundary == "field" and s.get("stop") == "PlayerInput" and not player(s).get("moving", true):
            return s
    failure = "settle-timeout:" + boundary
    return state()

func run() -> void:
    var output_path := OS.get_environment("SF2_H4_ACTUAL").replace("\\", "/").simplify_path()
    var local_root := ProjectSettings.globalize_path("res://../../local/").replace("\\", "/").simplify_path().trim_suffix("/") + "/"
    if not output_path.to_lower().begins_with(local_root.to_lower()) or FileAccess.file_exists(output_path):
        push_error("H4 actual output must be fresh and worktree-local")
        quit(2)
        return
    output = FileAccess.open(output_path, FileAccess.WRITE)
    if output == null:
        push_error("H4 actual output is unavailable")
        quit(2)
        return
    var plan: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_H4_PLAN")))
    acknowledge_dialogue = plan.get("acknowledgeDialogue", false)
    choices = plan.get("choices", [])
    root.size = Vector2i(960, 640)
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    # Capture before the first frame/input; _Ready has constructed the session.
    state("admission")
    for step in plan.steps:
        if failure != "": break
        logical_step = step.ordinal
        var s := state("before:" + str(int(step.ordinal)))
        var p := player(s)
        if s.get("map") != "map-" + str(int(step.before.map)) or p.get("x") != step.before.x * 384 or p.get("y") != step.before.y * 384:
            failure = "route-before-mismatch:" + str(int(step.ordinal))
            break
        await press(step.action)
        await settle(step.boundary)
        state("after:" + str(int(step.ordinal)))
    if failure == "" and plan.has("firstDecision"):
        var decision: Dictionary = plan.firstDecision
        var s := state("first-control")
        if s.get("actor") != "ally-" + str(int(decision.actor)) or s.get("round") != decision.round:
            failure = "first-control-actor-mismatch"
        else:
            for move in plan.firstDecisionInputs:
                await press(move.action)
            s = state("first-destination")
            if s.get("previewX") != decision.destination.x or s.get("previewY") != decision.destination.y:
                failure = "first-destination-mismatch"
            elif decision.action != "Stay":
                failure = "first-action-normalization-unavailable"
            else:
                for action in ["Confirm", "Stay", "Confirm"]:
                    await press(action)
                    if failure != "": break
                await settle("field")
                s = state("after-first-decision")
                if s.get("actor") != "ally-" + str(int(plan.nextDecision.actor)) or s.get("round") != plan.nextDecision.round:
                    failure = "next-decision-actor-mismatch"
    state("terminal")
    stop_observing()
    output.store_line(JSON.stringify({"terminal":true,"failure":failure,"sequence":sequence,
        "inputOrdinal":input_ordinal,"sessionId":session_id,"resultSignals":result_signals,
        "resultStream":"signal" if result_signals > 0 else "latest-result-only"}))
    output.close()
    host.queue_free()
    await process_frame
    await process_frame
    OS.delay_msec(100)
    quit(0 if failure == "" else 2)
