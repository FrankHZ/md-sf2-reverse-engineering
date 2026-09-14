extends SceneTree
var host: Node
var view: Node
var trace: Array
var edges: Array = []
var texts: Array = []
var programs: Array = []
var recent_program := ""
var seen_texts: Dictionary = {}
var index := 0
var frames := 0
var terminal: Dictionary = {}
var issue := ""
var case_name := OS.get_environment("SF2_PRIVATE_EXPLORATION_CASE")
var decline := case_name == "map3-decline"
var choices := 0
var declined: Dictionary = {}
func _initialize() -> void:
    call_deferred("run")
func state() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))
func player(s: Dictionary) -> Dictionary:
    for entity in s.get("entities", []):
        if entity.id == "entity-0": return entity
    return {}
func key(code: int) -> void:
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    frames += 1
    event = InputEventKey.new()
    event.keycode = code
    event.pressed = false
    Input.parse_input_event(event)
func run() -> void:
    var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_PRIVATE_EXPLORATION_PLAN")))
    trace = fixture.expectedObservation.records[0].logicalInputTrace
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        view = host.get_node("BattleSessionView")
        issue = "startup"
        await finish(state())
        return
    if case_name not in ["map3-opening", "map3-decline"]:
        issue = "unknown-case"
        await finish(state())
        return
    var identity: String = state().sessionId
    while frames < 12000:
        await process_frame
        frames += 1
        var s := state()
        if s.failure != null or s.sessionId != identity:
            issue = "session-or-presentation-failure"
            await finish(s)
            return
        if s.cursor != null:
            var program = s.cursor.Program
            if program != recent_program:
                programs.append({"program":program, "tick":s.simulationTick})
                recent_program = program
        if s.wait == "DialogueWait":
            if not seen_texts.has(s.token):
                texts.append({"id":s.textId,"speaker":s.speaker,"flags":s.speakerFlags,"program":s.cursor,"tick":s.simulationTick})
                seen_texts[s.token] = true
                await key(KEY_ENTER)
        elif s.wait == "ChoiceWait":
            await key(KEY_N if decline and choices == 0 else KEY_ENTER)
            choices += 1
        elif s.stop == "PlayerInput":
            if s.flags.has(603.0):
                if decline and not s.flags.has(66.0):
                    declined = s
                    await key(KEY_LEFT)
                    for i in range(40): await process_frame
                    await key(KEY_LEFT)
                    for i in range(40): await process_frame
                    await key(KEY_C)
                    continue
                if s.presentation.gestureDraws < 2 or s.presentation.soundStarts != 1 or s.presentation.soundFades != 1:
                    issue = "presentation-services"
                    await finish(s)
                    return
                terminal = s
                var terminal_player := player(s)
                await key(KEY_DOWN)
                for i in range(40): await process_frame
                var moved := state()
                var moved_player := player(moved)
                if moved.stop != "PlayerInput" or moved_player.x != terminal_player.x or moved_player.y != terminal_player.y + 384:
                    issue = "field-input-down"
                    await finish(moved)
                    return
                await key(KEY_UP)
                for i in range(40): await process_frame
                var returned := state()
                var returned_player := player(returned)
                if returned.stop != "PlayerInput" or returned_player.x != terminal_player.x or returned_player.y != terminal_player.y:
                    issue = "field-input-up"
                await finish(returned)
                return
            if index >= trace.size():
                issue = "route-ended-before-f603"
                await finish(s)
                return
            var edge: Dictionary = trace[index]
            var p := player(s)
            if p.x != edge.x * 384 or p.y != edge.y * 384:
                issue = "route-position-" + str(index)
                await finish(s)
                return
            var code = {"Left":KEY_LEFT,"Right":KEY_RIGHT,"Up":KEY_UP,"Down":KEY_DOWN,"C":KEY_C}.get(edge.input)
            if code == null:
                issue = "unknown-input-" + str(edge.input)
                await finish(s)
                return
            edges.append({"index":index,"input":edge.input,"x":edge.x,"y":edge.y,"tick":s.simulationTick})
            index += 1
            await key(code)
    issue = "frame-budget"
    await finish(state())
func finish(s: Dictionary) -> void:
    var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":issue == "","case":case_name,"issue":issue,"index":index,"frames":Engine.get_process_frames(),"edges":edges,"texts":texts,"programs":programs,"terminal":terminal,"declined":declined,"choices":choices,"final":s},"  "))
    file.close()
    host.queue_free()
    await process_frame
    await process_frame
    # Headless fixed-fps simulation outruns the independent audio mixing thread.
    # Allow its stop/free queue to drain before exiting; this does not advance game state.
    OS.delay_msec(100)
    quit(0 if issue == "" else 2)
