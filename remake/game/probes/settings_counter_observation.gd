extends "res://probes/engine_pre_voyage_guidance_observation.gd"

func key(code: int) -> void:
    var action: String = {KEY_ENTER:"confirm",KEY_DOWN:"down",KEY_ESCAPE:"cancel"}.get(code,"")
    if not action.is_empty(): code = InputMap.action_get_events("sf2_"+action).filter(func(e):return e is InputEventKey)[0].keycode
    await super.key(code)

func state() -> Dictionary: return observation(find("ExplorationSessionView"), "ReadObservationJson")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1774-"): quit(2); return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(12)
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): finish(); return
    await key(KEY_ENTER)
    for frame in 1800:
        if state().get("canWaitAtInput",false): break
        await frames(1)
    if state().get("map") != "map-8": check(false,"Ribble save required"); finish(); return
    await walk(Vector2i(5,13),"Ribble weapon counter")
    if not issue.is_empty(): check(false,issue); finish(); return
    await key(KEY_UP); await frames(16)
    var frame := find("ContextHint") as Control
    var caption := frame.find_child("Help", true, false) as Label
    var adventure := find("FieldMenuEntry") as Label
    var keyboard := caption.text
    check("F5" in keyboard and "Q" in keyboard and caption.get_theme_font_size("font_size") == 24,"editor-installed Talk and Accelerate at counter")
    var motion := InputEventJoypadMotion.new(); motion.device = 4; motion.axis = JOY_AXIS_RIGHT_Y; motion.axis_value = -1
    Input.parse_input_event(motion); await frames(4)
    check(caption.text != keyboard and state().acceleration.rate == 1.5 and "↑" in caption.text,"signed controller alternative actually accelerates at counter")
    check(frame.get_global_rect().encloses(caption.get_global_rect()) and not adventure.get_global_rect().intersects(caption.get_global_rect()),"mapped 24px counter cues fit beside Adventure")
    motion.axis_value = 0; Input.parse_input_event(motion); await frames(4)
    var mouse_motion := InputEventMouseMotion.new(); mouse_motion.position = Vector2(68,85); mouse_motion.relative = Vector2(13,2)
    Input.parse_input_event(mouse_motion); await frames(4)
    check(caption.text == keyboard and state().acceleration.rate == 1,"mouse restores keyboard hints without interacting")
    await key(KEY_ENTER)
    for frame_index in 300:
        if state().get("wait") == "ShopWait": break
        await frames(2)
    check(state().get("wait") == "ShopWait","displayed remapped Confirm opens actual Ribble merchant")
    finish()
