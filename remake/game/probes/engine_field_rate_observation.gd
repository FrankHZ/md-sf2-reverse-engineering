extends SceneTree

# Bounded actual-host witness. No screenshots, save editing or per-frame state log.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var shifts: Dictionary = {}
var counts: Dictionary = {}
var last_sequence := -1
var opportunities := 0
var goal := 0
var moves := 0
var began := 0
var elapsed := 0
var ended := false
var service_frame := -1
var services_in_frame := 0
var max_services_in_frame := 0

func _initialize() -> void:
    create_timer(30).timeout.connect(func(): finish("deadline"))
    call_deferred("run")

func frames(n: int) -> void:
    for i in n: await process_frame

func state() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func check(ok: bool, reason: String) -> void:
    if not ok: failures.append(reason)

func snapshot(label: String, all_entities := false) -> Dictionary:
    var s := state()
    check(s.failure == null and s.presentation.error == null, "healthy " + label)
    var row := {}
    for k in ["simulationTick", "mainSeed", "thinkingSeed", "map", "wait", "token", "flags", "acceleration", "tickDebt", "movementDirection", "canWaitAtInput", "textId", "visibleCharacters", "totalCharacters", "gold"]:
        row[k] = s.get(k)
    row["label"] = label
    row["entities"] = []
    for entity in (s.entities if all_entities else s.entities.slice(0, 1)):
        var actor := {}
        for k in ["id", "x", "y", "targetX", "targetY", "moving", "facing", "animationCounter", "actionCursor", "waitTimer", "speedX", "speedY"]:
            actor[k] = entity.get(k)
        row.entities.append(actor)
    row["audio"] = JSON.parse_string(host.call("ReadAudioObservationJson"))
    row["pitch"] = []
    for player in host.find_children("*", "AudioStreamPlayer", true, false): row.pitch.append(player.pitch_scale)
    samples.append(row)
    return row

func observed(raw: String) -> void:
    var result: Dictionary = JSON.parse_string(raw)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        last_sequence = int(event.Sequence)
        counts[event.Kind] = int(counts.get(event.Kind, 0)) + 1
        if event.Kind == "movement-requested": moves += 1
        if event.Kind in ["simulation-tick", "gameplay-wait"]: opportunities += 1
        if event.Kind in ["simulation-tick", "gameplay-wait"]:
            if service_frame != Engine.get_process_frames():
                service_frame = Engine.get_process_frames()
                services_in_frame = 0
            services_in_frame += 1
            max_services_in_frame = maxi(max_services_in_frame, services_in_frame)
    if goal > 0 and opportunities >= goal:
        elapsed = Time.get_ticks_usec() - began
        check(opportunities == goal, "exact opportunity endpoint")
        goal = 0
        view.process_mode = Node.PROCESS_MODE_DISABLED

func edge(code: Key, pressed: bool, location := 0, echo := false) -> void:
    if code == KEY_SHIFT:
        if pressed and not echo: shifts[location] = true
        elif not pressed: shifts.erase(location)
    var event := InputEventKey.new()
    event.keycode = code
    event.physical_keycode = code
    event.location = location
    event.pressed = pressed
    event.echo = echo
    event.shift_pressed = not shifts.is_empty()
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func key(code: Key, pressed: bool, location := 0, echo := false) -> void:
    edge(code, pressed, location, echo)
    await frames(2)

func tap(code: Key) -> void:
    await key(code, true)
    await key(code, false)

func panel() -> Dictionary:
    var node := host.find_child("SaveSlotView", true, false)
    return JSON.parse_string(node.call("ReadSaveObservationJson")) if node else {}

func until_panel(wanted: String) -> bool:
    for i in 300:
        if panel().get("state") == wanted: return true
        await process_frame
    failures.append("missing save panel " + wanted)
    return false

func mount() -> void:
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(3)

func controlled() -> void:
    check(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_RATE_START")) == "started", "controlled final-content field")
    view = host.find_child("ExplorationSessionView", true, false)

func continue_save() -> void:
    await frames(15)
    await tap(KEY_F10)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    if not await until_panel("save-slots"): return
    await tap(KEY_ENTER)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    if not await until_panel("saved"): return
    root.remove_child(host)
    host.free()
    await frames(3)
    await mount()
    await tap(KEY_ENTER)
    await tap(KEY_E)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    if not host.find_child("SaveSlotView", true, false): await tap(KEY_ENTER)
    if not await until_panel("continue-slots"): return
    await tap(KEY_ENTER)
    for i in 400:
        view = host.find_child("ExplorationSessionView", true, false)
        if view: return
        await process_frame
    failures.append("ordinary Continue field")

func flow() -> void:
    check(state().acceleration.rate == 1, "Continue starts at 1x")
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(state().acceleration.rate == 1.5 and state().movementDirection == null, "Shift alone arms without movement")
    await key(KEY_LEFT, true)
    await create_timer(0.22).timeout
    await key(KEY_LEFT, false)
    await create_timer(0.25).timeout
    snapshot("Shift then direction")
    check(moves >= 1 and state().canWaitAtInput, "actual motion completes after direction release")
    await key(KEY_SHIFT, true, KEY_LOCATION_RIGHT)
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    check(state().acceleration.rate == 1.5, "second Shift retains rate")
    await key(KEY_SHIFT, false, KEY_LOCATION_RIGHT)
    check(state().acceleration.rate == 1, "last Shift release restores 1x")
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT, true)
    check(state().acceleration.rate == 1, "echo cannot arm")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    await key(KEY_RIGHT, true)
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(state().movementDirection == "Right" and state().acceleration.rate == 1.5, "direction then Shift preserves direction")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    check(state().movementDirection == "Right" and state().acceleration.rate == 1, "rate release preserves direction")
    await key(KEY_RIGHT, false)
    await create_timer(0.3).timeout
    snapshot("direction then Shift and release")
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    await tap(KEY_F10)
    check(state().acceleration.rate == 1, "host menu disarms")
    var tick = state().simulationTick
    await create_timer(0.15).timeout
    check(state().simulationTick == tick, "host menu pauses ordinary time")
    await tap(KEY_F10)
    check(state().acceleration.rate == 1, "held Shift cannot rearm on modal return")
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT, true)
    check(state().acceleration.rate == 1, "modal return rejects echo")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(state().acceleration.rate == 1.5, "fresh Shift after modal")
    await tap(KEY_E)
    check(state().acceleration.rate == 1, "Party disarms")
    await tap(KEY_E)
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    snapshot("modal and Party returns")
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    await key(KEY_F10, true)
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    check(state().acceleration.held == false, "release observed inside host action barrier")
    await key(KEY_F10, false)
    await tap(KEY_F10)
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(state().acceleration.rate == 1.5, "first new press after modal release")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    snapshot("modal release barrier")

func transfer_case() -> void:
    var transfers := int(counts.get("map-transferred", 0))
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    await key(KEY_UP, true)
    for i in 160:
        if int(counts.get("map-transferred", 0)) > transfers: break
        await process_frame
    await key(KEY_UP, false)
    check(int(counts.get("map-transferred", 0)) > transfers, "real castle transfer")
    check(state().acceleration.rate == 1, "transfer disarms held acceleration")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    snapshot("castle transfer")

func focus_case() -> void:
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    var other := Window.new()
    other.hide()
    other.force_native = true
    other.transient = true
    other.title = "Field rate focus witness"
    other.size = Vector2i(240, 100)
    root.add_child(other)
    other.show()
    other.grab_focus()
    await create_timer(0.25).timeout
    check(other.has_focus() and not root.has_focus(), "actual native focus transferred")
    var tick = state().simulationTick
    await create_timer(0.15).timeout
    check(state().simulationTick == tick and state().acceleration.rate == 1, "focus suspends clock/rate")
    other.hide()
    root.grab_focus()
    await frames(4)
    check(root.has_focus() and state().acceleration.rate == 1, "focus return rejects held Shift")
    other.queue_free()
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(state().acceleration.rate == 1.5, "fresh activation after focus")
    other = Window.new()
    other.hide()
    other.force_native = true
    other.transient = true
    other.title = "Field rate release outside input route"
    root.add_child(other)
    other.show()
    other.grab_focus()
    await create_timer(0.2).timeout
    check(other.has_focus() and not root.has_focus(), "second native focus transfer")
    # Model the actual missing-key-up boundary: Godot gets the release while the
    # old host input route is unavailable. Return must reconcile neutral state.
    host.set_process_input(false)
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    host.set_process_input(true)
    other.hide()
    root.grab_focus()
    await frames(4)
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    check(root.has_focus() and state().acceleration.rate == 1.5, "first fresh press after outside release")
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    other.queue_free()
    snapshot("native focus return")

func remapped() -> void:
    await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
    await key(KEY_LEFT, true)
    check(moves == 0 and state().movementDirection == null, "Shift is not exempt after remapping acceleration away")
    await key(KEY_LEFT, false)
    await key(KEY_Q, true)
    check(state().acceleration.rate == 1, "Shift+Q is still an unbound chord")
    await key(KEY_Q, false)
    await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
    await key(KEY_Q, true)
    check(state().acceleration.rate == 1.5, "custom acceleration key")
    await key(KEY_J, true)
    check(moves == 1 and state().movementDirection == "Right", "legacy custom movement binding")
    await key(KEY_J, false)
    var axis := InputEventJoypadMotion.new()
    axis.device = 3
    axis.axis = JOY_AXIS_RIGHT_Y
    axis.axis_value = -1
    Input.parse_input_event(axis)
    Input.flush_buffered_events()
    await frames(2)
    await key(KEY_Q, false)
    check(state().acceleration.rate == 1.5, "optional mapped pad hold survives keyboard release")
    axis = InputEventJoypadMotion.new()
    axis.device = 3
    axis.axis = JOY_AXIS_RIGHT_Y
    axis.axis_value = 0
    Input.parse_input_event(axis)
    Input.flush_buffered_events()
    await frames(2)
    check(state().acceleration.rate == 1, "mapped neutral restores 1x")
    snapshot("remapped keyboard and pad")

func run() -> void:
    var output := OS.get_environment("SF2_RATE_OUTPUT")
    if output.is_empty() or OS.get_environment("SF2_SAVE_NAMESPACE") != OS.get_environment("SF2_RATE_NAMESPACE"):
        finish("explicit disposable namespace/output required")
        return
    root.size = Vector2i(1920, 1080)
    await mount()
    if OS.get_environment("SF2_RATE_CASE") == "battle":
        view = host.find_child("BattleSessionView", true, false)
        check(view != null, "actual authored battle")
        if view:
            await frames(8)
            await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
            var pacing: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
            check(pacing.acceleration.rate == 1, "battle acceleration remains disabled in A")
            await tap(KEY_UP)
            await create_timer(0.25).timeout
            var battle: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
            check(battle.failure == null, "battle 1x remains healthy")
            samples.append({"label":"battle remains 1x", "pacing":pacing.acceleration, "battle":battle})
            await key(KEY_SHIFT, false, KEY_LOCATION_LEFT)
        finish()
        return
    if OS.get_environment("SF2_RATE_CASE") != "automatic": controlled()
    else: view = host.find_child("ExplorationSessionView", true, false)
    if not view:
        finish("field did not mount")
        return
    if OS.get_environment("SF2_RATE_CASE") == "continue":
        await continue_save()
        if not failures.is_empty(): finish(); return
    view.connect("SessionResultObserved", observed)
    snapshot("initial", true)
    match OS.get_environment("SF2_RATE_CASE"):
        "clock":
            began = Time.get_ticks_usec()
            goal = 60
            if OS.get_environment("SF2_RATE_FACTOR") == "1.5": edge(KEY_SHIFT, true, KEY_LOCATION_LEFT)
            while goal > 0 and failures.is_empty(): await process_frame
            await frames(2)
            snapshot("matched 60 opportunities", true)
        "continue", "flow": await flow()
        "focus": await focus_case()
        "remapped": await remapped()
        "transfer": await transfer_case()
        "stall":
            await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
            var before := opportunities
            OS.delay_msec(350)
            await frames(4)
            check(opportunities - before <= 10 and max_services_in_frame <= 4, "stall drops whole debt and caps service")
            snapshot("350ms render stall")
        "automatic":
            await key(KEY_SHIFT, true, KEY_LOCATION_LEFT)
            check(state().acceleration.rate == 1.5, "automatic work accepts held rate")
            await tap(KEY_ENTER)
            await create_timer(0.9).timeout
            var reading := snapshot("automatic wait to reading")
            check(state().wait == "DialogueWait" and state().acceleration.rate == 1, "input boundary disarms")
            var token = state().token
            await create_timer(0.2).timeout
            await key(KEY_SHIFT, true, KEY_LOCATION_LEFT, true)
            check(state().token == token and state().acceleration.rate == 1, "Shift cannot acknowledge text")
            var after := snapshot("unscaled reading")
            check(after.visibleCharacters - reading.visibleCharacters <= 9, "reveal retains 30 cps budget")
    finish()

func finish(reason := "") -> void:
    if ended: return
    ended = true
    if not reason.is_empty(): failures.append(reason)
    var report := {"case":OS.get_environment("SF2_RATE_CASE"), "factor":OS.get_environment("SF2_RATE_FACTOR"), "focused":root.has_focus(), "passed":failures.is_empty(), "failures":failures, "opportunities":opportunities, "maxServicesInFrame":max_services_in_frame, "moves":moves, "elapsedUsec":elapsed, "events":counts, "samples":samples, "panel":panel() if is_instance_valid(host) else {}}
    var bytes := JSON.stringify(report, "  ").to_utf8_buffer()
    if bytes.size() > 100 * 1024: failures.append("100 KiB report budget exceeded")
    var path := OS.get_environment("SF2_RATE_OUTPUT")
    if not path.is_empty():
        var file := FileAccess.open(path, FileAccess.WRITE)
        file.store_buffer(bytes)
        file.close()
    quit(0 if failures.is_empty() else 1)
