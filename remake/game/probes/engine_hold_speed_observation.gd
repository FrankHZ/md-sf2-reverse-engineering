extends SceneTree

# Observe the installed adapter through its normal input and Settings routes.
var host: Node
var view: Node
var samples: Dictionary = {}
var failures: Array[String] = []
var case_name := OS.get_environment("SF2_HOLD_SPEED_CASE")

func _initialize() -> void:
    call_deferred("run")
    create_timer(25).timeout.connect(func(): finish("deadline"))

func frames(count := 3) -> void:
    for i in count: await process_frame

func input_event(event: InputEvent) -> void:
    Input.parse_input_event(event)
    Input.flush_buffered_events()
    await frames()

func key(code: Key, pressed: bool) -> void:
    var event := InputEventKey.new()
    event.keycode = code
    event.physical_keycode = code
    event.pressed = pressed
    if code == KEY_SHIFT:
        event.location = KEY_LOCATION_LEFT
        event.shift_pressed = pressed
    await input_event(event)

func tap(code: Key) -> void:
    await key(code, true)
    await key(code, false)

func pad(value: float) -> void:
    var event := InputEventJoypadMotion.new()
    event.device = 3
    event.axis = JOY_AXIS_RIGHT_Y
    event.axis_value = value
    await input_event(event)

func check(ok: bool, label: String) -> bool:
    if not ok: failures.append(label)
    return ok

func read(node: Node, method: String) -> Dictionary:
    return JSON.parse_string(node.call(method)) if node else {}

func settings() -> Dictionary:
    return read(host.find_child("SettingsView", true, false), "ReadSettingsObservationJson")

func applied() -> Dictionary:
    return read(host, "ReadSettingsHostObservationJson")

func game() -> Dictionary:
    return read(view, "ReadObservationJson")

func rate(label: String, expected: float) -> void:
    var state := game()
    var actual = state.get("acceleration") if case_name != "battle" else state.get("pacing", {}).get("input")
    samples[label] = actual
    check(actual != null and actual.rate == expected and state.failure == null, label)

func open_assist() -> bool:
    await tap(KEY_F10)
    await frames(8)
    await tap(KEY_ENTER)
    var state := settings()
    if not check(state.get("page") == "Root" and "acceleration" not in state.rows, "unchanged Settings root"):
        return false
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    return check(settings().get("page") == "Assist", "Gameplay aids opens from Settings")

func select_speed() -> bool:
    await tap(KEY_DOWN)
    await tap(KEY_DOWN)
    var state := settings()
    return check(state.get("focus") == "acceleration", "Hold speed row selected")

func apply_from_root() -> bool:
    for i in settings().get("rows", []).size():
        if settings().get("focus") == "apply": break
        await tap(KEY_UP)
    if not check(settings().get("focus") == "apply", "Apply selected"):
        return false
    await tap(KEY_ENTER)
    if not check(settings().is_empty(), "Apply succeeds"):
        return false
    await tap(KEY_F10)
    return true

func field() -> void:
    check(applied().applied.AccelerationMultiplier == 1.5, "legacy preference defaults to 1.5")
    rate("legacy idle", 1)
    await key(KEY_Q, true)
    rate("legacy remapped key", 1.5)
    await key(KEY_Q, false)
    rate("legacy release", 1)
    if not await open_assist() or not await select_speed(): return
    check(settings().message.contains("Q"), "details show installed remap")
    for factor in [2.0, 2.5, 3.0]:
        await tap(KEY_RIGHT)
        var draft := settings()
        check(draft.draft.AccelerationMultiplier == factor and draft.applied.AccelerationMultiplier == 1.5,
            "cycle draft only to " + str(factor))
    await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    check(settings().is_empty() and applied().applied.AccelerationMultiplier == 1.5, "Cancel discards 3")
    await tap(KEY_F10)

    if not await open_assist() or not await select_speed(): return
    await tap(KEY_LEFT)
    await tap(KEY_LEFT)
    check(settings().draft.AccelerationMultiplier == 2.5, "reverse cycle wraps through 3 to 2.5")
    await tap(KEY_ESCAPE)
    if not await apply_from_root(): return
    await key(KEY_Q, true)
    rate("applied intermediate key", 2.5)
    await tap(KEY_F10)
    rate("field menu disarms", 1)
    var tick = game().simulationTick
    await frames(8)
    check(game().simulationTick == tick, "field menu pauses the clock")
    await tap(KEY_F10)
    rate("menu return while held", 1)
    await key(KEY_Q, false)

    if not await open_assist() or not await select_speed(): return
    await tap(KEY_RIGHT)
    check(settings().labels[settings().selected] == "Hold speed: 3×", "English label uses draft multiplier")
    await tap(KEY_ESCAPE)
    await tap(KEY_UP)
    await tap(KEY_ENTER)
    await tap(KEY_RIGHT)
    await tap(KEY_ESCAPE)
    if not await apply_from_root(): return
    check(applied().applied.Language == "zh-CN", "Chinese applied in same transaction")
    await key(KEY_Q, true)
    rate("applied maximum key", 3)
    await pad(-1)
    await key(KEY_Q, false)
    rate("pad retains maximum after key release", 3)
    await pad(0)
    rate("last source restores normal", 1)

func battle() -> void:
    check(applied().applied.AccelerationMultiplier == 3, "cold launch restores applied maximum")
    var before := game()
    rate("battle cold idle", 1)
    var step_start: int = before.pacing.steps
    await frames(12)
    var normal_steps: int = int(game().pacing.steps) - step_start
    await key(KEY_SHIFT, true)
    rate("unbound Shift is inert", 1)
    await key(KEY_SHIFT, false)
    await pad(-1)
    rate("remapped pad alone accelerates battle", 3)
    step_start = game().pacing.steps
    await frames(12)
    var held_steps := int(game().pacing.steps) - step_start
    samples["battle pacing steps"] = {"normal":normal_steps, "held":held_steps}
    check(normal_steps > 0 and held_steps > normal_steps, "battle consumer services more steps while held")
    await pad(0)
    await key(KEY_Q, true)
    rate("remapped key accelerates battle", 3)
    var other: Window
    if DisplayServer.get_name() == "headless":
        root.emit_signal("focus_exited")
    else:
        other = Window.new()
        other.hide()
        other.force_native = true
        other.transient = true
        other.title = "Hold speed focus witness"
        other.size = Vector2i(240, 100)
        root.add_child(other)
        other.show()
        other.grab_focus()
        await create_timer(0.25).timeout
        check(other.has_focus() and not root.has_focus(), "native focus transferred")
    rate("battle focus loss disarms", 1)
    var steps = game().pacing.steps
    await frames(8)
    check(game().pacing.steps == steps, "battle pacing suspended on focus loss")
    if other:
        other.hide()
        root.grab_focus()
        await create_timer(0.2).timeout
        check(root.has_focus(), "native focus restored")
        other.queue_free()
    else:
        root.emit_signal("focus_entered")
    await frames()
    rate("focus return while held", 1)
    await key(KEY_Q, false)
    await key(KEY_Q, true)
    rate("fresh press after focus return", 3)
    await key(KEY_Q, false)
    rate("battle final release", 1)
    var after := game()
    for field in ["mainSeed", "thinkingSeed", "gold", "round", "actor", "queueCursor", "stage", "selection"]:
        check(before[field] == after[field], "acceleration alone preserves battle " + field)

    if not await open_assist() or not await select_speed(): return
    check(settings().labels[settings().selected] == "按住加速：3×", "Chinese label shows persisted multiplier")
    var row := host.find_child("Setting_acceleration", true, false) as Button
    var hint := host.find_child("SettingsHint", true, false) as Label
    check(row.get_theme_font_size("font_size") == 30 and hint.get_theme_font_size("font_size") == 24, "production Body/Hint sizes")
    await tap(KEY_ESCAPE)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    await tap(KEY_DOWN)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    await tap(KEY_RIGHT)
    await tap(KEY_RIGHT)
    var help := read(host.find_child("ControlsHelpView", true, false), "ReadControlsHelpObservationJson")
    check(help.get("page") == 2 and help.body.contains("3×") and help.body.contains("Q"),
        "Controls Help shows installed multiplier and remap")
    await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    await tap(KEY_UP)
    await tap(KEY_ENTER)
    if not await select_speed(): return
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    check(settings().draft.AccelerationMultiplier == 1.5 and settings().applied.AccelerationMultiplier == 3,
        "Defaults stages 1.5")
    await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    check(applied().applied.AccelerationMultiplier == 3, "Cancel discards Defaults")
    await tap(KEY_F10)
    if not await open_assist(): return
    await tap(KEY_UP)
    await tap(KEY_ENTER)
    await tap(KEY_ESCAPE)
    if not await apply_from_root(): return
    check(applied().applied.AccelerationMultiplier == 1.5, "Apply installs Defaults")
    await key(KEY_Q, true)
    rate("defaults with retained binding", 1.5)
    await key(KEY_Q, false)

func run() -> void:
    if not check(case_name in ["field", "battle", "default-shift"], "explicit case required") or not check(
        OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue2168-"), "disposable namespace required"):
        finish()
        return
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames()
    if case_name != "battle":
        var start = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOLD_SPEED_START"))
        if not check(start == "started", "controlled private world start: " + str(start)):
            finish()
            return
    view = host.find_child("BattleSessionView" if case_name == "battle" else "ExplorationSessionView", true, false)
    if not check(view != null, "gameplay view mounted"):
        finish()
        return
    await frames(12)
    match case_name:
        "field": await field()
        "battle": await battle()
        "default-shift":
            await key(KEY_SHIFT, true)
            rate("fresh settings default Shift", 1.5)
            await key(KEY_SHIFT, false)
            rate("fresh settings Shift release", 1)
    finish()

func finish(reason := "") -> void:
    if not reason.is_empty(): failures.append(reason)
    print(JSON.stringify({"case":case_name, "passed":failures.is_empty(), "failures":failures, "samples":samples}))
    quit(0 if failures.is_empty() else 1)
