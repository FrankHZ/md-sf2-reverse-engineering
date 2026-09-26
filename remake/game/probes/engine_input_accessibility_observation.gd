extends "res://probes/engine_battle01_outcome_observation.gd"

# External native observation. Authored variations are written only to the caller's
# ignored destination, then admitted by the ordinary Main/startup path.
var samples: Array = []
var failures: Array = []
var input_case := OS.get_environment("SF2_INPUT_CASE")
var use_pad := "gamepad" in input_case
var remapped := "remapped" in input_case
var private_route := "private" in input_case
var maximum_white := 0.0
var completed_white: Array = []
var white_tokens: Dictionary = {}
var wait_receipts: Array = []
var release_at := -1
var wait_axis := input_case == "field-wait-remapped-axis-gamepad"
var field_paths: Dictionary = {}
var field_created: Dictionary = {}
var field_main_started := false
var field_output: FileAccess
var field_unavailable: Array[String] = []
var guarded_wait_case := "portrait-event" in input_case or "field-projection" in input_case or "field-wait" in input_case or "text-wait" in input_case or "warp-transition" in input_case or "w1-private" in input_case or "opening-private" in input_case
var warp_records: Array = []
var nod_pause_checked := false
var camera_pause_checked := false
var camera_draws: Array = []
var camera_draw_seen: Dictionary = {}

func record_camera_draw() -> void:
    if not is_instance_valid(view): return
    var s := state()
    var p = s.get("cameraProjection")
    if p == null or s.logicalView == null or p.simulationTick != s.simulationTick or p.token != s.token: return
    var identity := str([s.simulationTick, s.token])
    if camera_draw_seen.has(identity): return
    camera_draw_seen[identity] = true
    camera_draws.append({"projection":p, "logicalView":s.logicalView, "entities":s.entities})

func check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func read_sample(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label, "state":s})
    check(s.failure == null, label + ": no application/adapter error")
    return s

func state() -> Dictionary:
    var s := super.state()
    if s.has("presentation"):
        maximum_white = maxf(maximum_white, s.presentation.whiteOpacity)
        var p: Dictionary = s.presentation
        if p.activeCue in ["FadeIn", "FadeOut"]:
            white_tokens[s.token] = p.activeCue
        if p.completedCueToken != null and white_tokens.has(p.completedCueToken):
            var receipt := {"token":p.completedCueToken, "kind":p.completedCueKind}
            if not completed_white.has(receipt): completed_white.append(receipt)
    return s

func physical(code: int, pressed: bool) -> void:
    var action: String = {KEY_UP:"up", KEY_W:"up", KEY_RIGHT:"right", KEY_D:"right",
        KEY_DOWN:"down", KEY_S:"down", KEY_LEFT:"left", KEY_A:"left",
        KEY_ENTER:"confirm", KEY_Z:"confirm", KEY_ESCAPE:"cancel", KEY_X:"cancel",
        KEY_F:"attack", KEY_H:"spell", KEY_TAB:"target", KEY_SPACE:"stay", KEY_V:"wait"}[code]
    if wait_axis and action == "wait":
        field_axis(0.8 if pressed else 0.0)
    elif use_pad:
        var buttons := {"up":JOY_BUTTON_DPAD_UP,"right":JOY_BUTTON_DPAD_RIGHT,
            "down":JOY_BUTTON_DPAD_DOWN,"left":JOY_BUTTON_DPAD_LEFT,
            "confirm":JOY_BUTTON_A,"cancel":JOY_BUTTON_B,"attack":JOY_BUTTON_X,
            "spell":JOY_BUTTON_Y,"target":JOY_BUTTON_RIGHT_SHOULDER,"stay":JOY_BUTTON_LEFT_SHOULDER,"wait":JOY_BUTTON_RIGHT_STICK}
        if remapped:
            buttons.merge({"confirm":JOY_BUTTON_Y,"cancel":JOY_BUTTON_X,"attack":JOY_BUTTON_A,
                "spell":JOY_BUTTON_B,"target":JOY_BUTTON_LEFT_SHOULDER,"stay":JOY_BUTTON_RIGHT_SHOULDER,"wait":JOY_BUTTON_START}, true)
        var event := InputEventJoypadButton.new()
        event.button_index = buttons[action]
        event.device = 0
        event.pressed = pressed
        Input.parse_input_event(event)
    else:
        var event := InputEventKey.new()
        event.keycode = code
        if remapped:
            event.keycode = {"up":KEY_I,"right":KEY_L,"down":KEY_K,"left":KEY_J,
                "confirm":KEY_E,"cancel":KEY_Q,"attack":KEY_R,"spell":KEY_T,"target":KEY_U,"stay":KEY_O,"wait":KEY_B}[action]
        event.pressed = pressed
        Input.parse_input_event(event)

    if guarded_wait_case: Input.flush_buffered_events()

func key(code: int) -> void:
    physical(code, true)
    await process_frame
    frames += 1
    physical(code, false)
    # Preserve the established private navigation driver's single-frame cadence.
    if not private_route: await process_frame

func axis(code: int, value: float) -> void:
    var event := InputEventJoypadMotion.new()
    event.axis = code
    event.axis_value = value
    event.device = 0
    Input.parse_input_event(event)
    await process_frame
    await process_frame

func settle_authored() -> Dictionary:
    for frame in range(600):
        await process_frame
        var s := state()
        if s.failure != null or s.has("stage"): return s
        if s.wait in ["DialogueWait", "ChoiceWait"]: return s
        if s.stop == "PlayerInput" and not s.entities.any(func(e): return e.moving): return s
    check(false, "Bounded authored wait completed")
    return state()

func acknowledge() -> void:
    var before := state()
    if before.visibleCharacters >= 0 and before.visibleCharacters < before.totalCharacters:
        await key(KEY_ENTER)
        var revealed := read_sample("reveal-keeps-real-wait")
        check(revealed.token == before.token and revealed.revision == before.revision,
            "Reveal does not acknowledge or choose")
        check(revealed.visibleCharacters == revealed.totalCharacters, "Confirm reveals all remaining text")
    await key(KEY_ENTER)

func make_inputs() -> void:
    var args := OS.get_cmdline_user_args()
    var package_path := args[args.find("--authored-package") + 1]
    var alternate := OS.get_environment("SF2_INPUT_VARIANT") == "hill"
    var source := "hill-passage" if alternate else "harbor-arrival"
    var package: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/" + source + ".json"))
    var battle: Dictionary = package.battle
    var physical_package: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/stone-court.json"))
    battle.encounters[0].rewards = physical_package.encounters[0].rewards
    for actor in battle.actors:
        actor.physical = {"movementType":"regular","critical":{"chance":"one-in-16","damageBonus":"quarter"},
            "promoted":false,"leader":false,"gold":1,"special":"none"}
    battle.actors[2].maxHp = 100
    battle.start.actors[2].hp = 100
    battle.start.actors[0].hp = 5
    var placements: Array = battle.encounters[0].placements
    placements[2].x = placements[0].x + 1
    placements[2].y = placements[0].y
    placements[1].x = placements[0].x
    placements[1].y = placements[0].y + 1
    var spell: Dictionary = battle.spells[0].duplicate(true)
    spell.id = "restore"
    spell.level = 2
    spell.mpCost = 5
    spell.maximumRange = 2
    battle.spells.append(spell)
    battle.actors[0].spells.append({"id":"restore","level":2})
    var invitation: Dictionary = package.world.programs[0]
    # Keep the same visible question through its choice; only the adapter reveals text.
    invitation.instructions[1].mode = "continued"
    var acceptance: Dictionary = package.world.programs[1]
    acceptance.instructions.insert(0, {"op":"present","kind":"FadeIn","resource":"white","entity":null,"position":null})
    acceptance.instructions.insert(0, {"op":"present","kind":"FadeOut","resource":"white","entity":null,"position":null})
    var file := FileAccess.open(package_path, FileAccess.WRITE)
    file.store_string(JSON.stringify(integer_numbers(package)))
    file.close()
    if args.has("--input-settings"):
        write_settings(args[args.find("--input-settings") + 1])

# Godot parses JSON numbers as floats; the existing Content contract requires integers.
func integer_numbers(value: Variant) -> Variant:
    if value is Dictionary:
        for name in value: value[name] = integer_numbers(value[name])
    elif value is Array:
        for index in value.size(): value[index] = integer_numbers(value[index])
    elif value is float and value == floor(value): return int(value)
    return value

func write_settings(path: String) -> bool:
    var settings := {"formatVersion":1,"confirmCancel":"swapped" if remapped else "standard",
        "textMode":"adjustable" if remapped else "instant", "charactersPerSecond":20,
        "reducedFlash":remapped,"bindings":{}}
    if private_route:
        settings.textMode = "instant"
        settings.reducedFlash = false
    if "portrait-event" in input_case or "field-projection" in input_case or "w1-private" in input_case or "opening-private" in input_case:
        settings.textMode = "instant" if "instant" in input_case else "adjustable"
        settings.charactersPerSecond = 40
    if "warp-transition" in input_case: settings.reducedFlash = "reduced" in input_case
    if OS.get_environment("SF2_INPUT_RATE") != "": settings.charactersPerSecond = int(OS.get_environment("SF2_INPUT_RATE"))
    if remapped:
        var keys := {"up":"I","right":"L","down":"K","left":"J","confirm":"Q","cancel":"E",
            "attack":"R","spell":"T","target":"U","stay":"O","item":"P","wait":"B"}
        var buttons := {"up":"DpadUp","right":"DpadRight","down":"DpadDown","left":"DpadLeft",
            "confirm":"West","cancel":"North","attack":"South","spell":"East","target":"LeftShoulder","stay":"RightShoulder","item":"Back","wait":"Start"}
        var axes := {"up":["RightY-"],"right":["RightX+"],"down":["RightY+"],"left":["RightX-"]}
        for action in keys:
            settings.bindings[action] = {"keys":[keys[action]],"buttons":[buttons[action]],"axes":axes.get(action, [])}
    if wait_axis: settings.bindings.wait.axes = ["LeftX+"]
    if not field_paths.is_empty(): return write_field_file("settings", JSON.stringify(settings))
    # Legacy cases retain their existing lifecycle; Wait cases use the fresh-path guard.
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify(settings))
    file.close()
    return true

func run() -> void:
    if "portrait-event" in input_case:
        await run_portrait_event()
        return
    if "field-projection" in input_case:
        await run_field_projection()
        return
    if "opening-private" in input_case:
        await run_opening_text()
        return
    if "w1-private" in input_case or "opening-private" in input_case:
        await run_w1_interaction()
        return
    if "warp-transition" in input_case:
        await run_warp_transition()
        return
    if "text-wait" in input_case:
        await run_text_wait()
        return
    if "field-wait" in input_case:
        await run_field_wait()
        return
    if private_route:
        var args := OS.get_cmdline_user_args()
        write_settings(args[args.find("--input-settings") + 1])
        await super.run()
        return
    root.size = Vector2i(960, 640)
    make_inputs()
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    var initial := read_sample("field-input")
    if initial.failure != null:
        finish_public()
        return
    var identity: String = initial.sessionId
    var field_player: Dictionary = initial.entities.filter(func(e): return e.id == "traveler")[0]
    var hill := OS.get_environment("SF2_INPUT_VARIANT") == "hill"
    await key(KEY_S if hill else KEY_A)
    var moved_field := await settle_authored()
    var moved_player: Dictionary = moved_field.entities.filter(func(e): return e.id == "traveler")[0]
    check(moved_player.x != field_player.x or moved_player.y != field_player.y, "Logical movement moves the actual field actor")
    await key(KEY_W if hill else KEY_D)
    await settle_authored()
    if remapped:
        var ignored := InputEventKey.new()
        ignored.keycode = KEY_ENTER
        ignored.pressed = true
        var revision = state().revision
        Input.parse_input_event(ignored)
        await process_frame
        ignored.pressed = false
        Input.parse_input_event(ignored)
        check(state().revision == revision, "Replaced default key does not silently keep its old action")
    # All physical aliases enter the common route; release events do not submit.
    await key(KEY_Z)
    var dialogue := read_sample("dialogue")
    check(dialogue.wait == "DialogueWait", "Confirm interacts")
    check((dialogue.visibleCharacters >= 0) == remapped, "Configured text mode reaches actual Label")
    await create_timer(0.15).timeout
    var progressed := read_sample("text-progress")
    check(progressed.token == dialogue.token and progressed.revision == dialogue.revision, "Elapsed reveal time never acknowledges")
    if remapped:
        check(progressed.visibleCharacters > dialogue.visibleCharacters and progressed.visibleCharacters < progressed.totalCharacters,
            "Adjustable text progresses partially at configured rate")
    await key(KEY_X)
    check(state().token == dialogue.token, "Cancel does not acknowledge dialogue")
    await acknowledge()
    check(state().wait == "ChoiceWait", "Acknowledgement reaches actual choice")
    await key(KEY_X)
    check(state().stop == "PlayerInput" and state().flags.is_empty(), "Cancel declines with no acceptance flag")
    await key(KEY_ENTER)
    await acknowledge()
    await key(KEY_Z)
    var cue := read_sample("actual-white-cue")
    check(cue.wait == "PresentationWait", "Accepted choice reaches real white presentation wait")
    var token = cue.token
    await key(KEY_ENTER)
    check(state().token == token, "Confirm cannot complete a presentation wait")
    var before_battle := await settle_authored()
    read_sample("white-cues-completed")
    check(completed_white.size() == 2 and completed_white[0].token == token,
        "Both actual white cue tokens complete through the presentation service")
    check(maximum_white == 0.0 if remapped else maximum_white > 0.95,
        "Reduced-flash suppresses white projection; standard mode performs it")
    check(before_battle.presentation.suppressedWhiteCues == (2 if remapped else 0), "Suppression recorded as product deviation")
    check(before_battle.wait == "DialogueWait" and 15.0 in before_battle.flags, "Same acceptance route reaches before-battle dialogue")
    if remapped:
        var rate := 20.0 if OS.get_environment("SF2_INPUT_RATE").is_empty() else float(OS.get_environment("SF2_INPUT_RATE"))
        await create_timer(float(before_battle.totalCharacters) / rate + 0.1).timeout
        var fully_revealed := read_sample("full-reveal-still-needs-acknowledgement")
        check(fully_revealed.visibleCharacters == fully_revealed.totalCharacters and fully_revealed.token == before_battle.token,
            "Completed timed reveal leaves the real dialogue wait pending")
    await acknowledge()
    var battle := read_sample("first-battle-input")
    check(battle.sessionId == identity and battle.stage == "Movement" and battle.revision > before_battle.revision,
        "Mode transition keeps session and consumes Confirm once")
    var first_actor: String = battle.actor
    if use_pad:
        var moving_axis := JOY_AXIS_RIGHT_Y if remapped else JOY_AXIS_LEFT_Y
        await axis(moving_axis, -0.8)
        var moved := read_sample("stick-edge")
        check(moved.previewY == battle.previewY - 1, "Mapped stick edge moves once")
        await axis(moving_axis, -0.9)
        await axis(JOY_AXIS_TRIGGER_LEFT, 0.8)
        await axis(JOY_AXIS_TRIGGER_LEFT, 0.0)
        await axis(moving_axis, 0.0)
        check(state().revision == moved.revision and state().previewY == moved.previewY,
            "Held stick, unrelated axis and releases produce no duplicate command")
        await axis(moving_axis, 0.8)
        await axis(moving_axis, 0.0)
    else:
        await key(KEY_UP)
        check(state().previewY == battle.previewY - 1, "Keyboard movement enters common preview")
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(state().stage == "ActionChoice", "Confirm enters action choice")
    await key(KEY_ESCAPE)
    check(state().stage == "Movement", "Cancel restores movement without attack conflict")
    await key(KEY_ENTER)
    await key(KEY_H)
    var spell_one := state()
    await key(KEY_H)
    var spell_two := read_sample("second-spell")
    check(spell_two.spell.Value == "restore" and spell_two.spell != spell_one.spell, "All learned spells can be cycled")
    await key(KEY_TAB)
    check(state().target != first_actor and state().failure == null, "Target cycle reaches another living ally")
    await key(KEY_X)
    await key(KEY_ENTER)
    await key(KEY_H)
    await key(KEY_ENTER)
    var healed := read_sample("heal-committed")
    check(healed.actors[0].hp > battle.actors[0].hp and healed.actors[0].mp < battle.actors[0].mp,
        "Actual spell commits resources and advances control")
    check(healed.actor != first_actor, "Another supported actor receives control")
    await key(KEY_ENTER)
    await key(KEY_SPACE)
    await key(KEY_ENTER)
    var stayed := read_sample("stay-committed")
    check(stayed.actor == first_actor and stayed.round > battle.round, "Stay and automatic work reach next round")
    await key(KEY_ENTER)
    await key(KEY_F)
    await key(KEY_TAB)
    var target := read_sample("physical-target")
    check(target.failure == null and target.target == battle.actors[2].id, "Physical action and target remain reachable")
    await key(KEY_ENTER)
    var attacked := read_sample("attack-committed")
    check(attacked.actors[2].hp < stayed.actors[2].hp and attacked.actor != first_actor, "Actual attack commits and hands off")
    check(attacked.help.contains("Pad South" if remapped else "Pad West"), "Battle help projects effective attack binding")
    if remapped:
        check(attacked.help.contains("E / Pad North: choose action"), "Swapped help projects semantic Confirm")
    finish_public()

func field_signature(s: Dictionary) -> Array:
    return [s.revision, s.simulationTick, s.mainSeed, s.entities]

func observe_wait(result_json: String) -> void:
    var result: Dictionary = JSON.parse_string(result_json)
    check(result.failure == null, "Every submitted field command succeeds")
    for observation in result.observations:
        if observation.Kind == "gameplay-wait":
            wait_receipts.append({"frame":Engine.get_process_frames(), "micros":Time.get_ticks_usec(),
                "revision":result.revision})
            if wait_receipts.size() == release_at:
                physical(KEY_V, false)

func assert_field_paused(label: String) -> void:
    var before := state()
    await create_timer(0.12).timeout
    var after := read_sample(label)
    check(field_signature(after) == field_signature(before), label + ": no revision, tick, RNG or entity progress")

func tap_wait() -> void:
    var count := wait_receipts.size()
    physical(KEY_V, true)
    physical(KEY_V, false)
    await process_frame
    check(wait_receipts.size() == count + 1, "A fresh tap admits exactly one Wait")

func field_io_failure(reason: String) -> bool:
    if field_output != null:
        field_output.close()
        field_output = null
    push_error("field-wait-io-failure: " + reason + "; main-started=" + str(field_main_started) +
        "; created=" + str(field_created.keys()))
    quit(2)
    return false

func admit_field_paths() -> bool:
    # Same fresh/local boundary as the scene and H4 probes, applied to all three destinations
    # before touching any of them. Only this entry owns generated input rewrites.
    var args := OS.get_cmdline_user_args()
    var retained_start := "portrait-event" in input_case or "w1-private" in input_case or "opening-private" in input_case
    var entry := "--private-exploration-start" if private_route and ("warp-transition" in input_case or retained_start) else "--authored-package"
    if args.size() != 4 or args.count(entry) != 1 or args.count("--input-settings") != 1:
        return field_io_failure("required unique startup arguments")
    var destinations := {"output":OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")}
    for index in [0, 2]:
        if args[index] not in [entry, "--input-settings"]:
            return field_io_failure("invalid startup argument order")
        destinations["package" if args[index] == entry else "settings"] = args[index + 1]
    var local_root := ProjectSettings.globalize_path("res://../../local/").replace("\\", "/").simplify_path().trim_suffix("/")
    var identities: Array[String] = []
    for name in destinations:
        var path: String = destinations[name].replace("\\", "/").simplify_path()
        if not path.is_absolute_path() or not path.to_lower().begins_with(local_root.to_lower() + "/"):
            return field_io_failure(name + " must be absolute and worktree-local under ignored local/")
        for component in path.substr(local_root.length() + 1).split("/"):
            if not component.is_valid_filename() or component.ends_with(".") or component.ends_with(" "):
                return field_io_failure(name + " has an invalid path component")
        var read_only: bool = retained_start and name == "package"
        if read_only and not FileAccess.file_exists(path): return field_io_failure("retained start must exist")
        if identities.has(path.to_lower()) or (FileAccess.file_exists(path) and not read_only) or DirAccess.dir_exists_absolute(path):
            return field_io_failure(name + " must be fresh and distinct")
        identities.append(path.to_lower())
        var parent := path.get_base_dir()
        if not DirAccess.dir_exists_absolute(parent): return field_io_failure(name + " parent must exist")
        var parent_directory := DirAccess.open(parent)
        if parent_directory == null or parent_directory.is_link(path.get_file()):
            return field_io_failure(name + " must be a fresh regular-file destination")
        # A link/junction ancestor would defeat the lexical local boundary.
        var ancestor := parent
        while ancestor.length() >= local_root.length():
            var directory := DirAccess.open(ancestor.get_base_dir())
            if directory == null or directory.is_link(ancestor.get_file()):
                return field_io_failure(name + " parent must be an accessible real local directory")
            ancestor = ancestor.get_base_dir()
        destinations[name] = path
    field_paths = destinations
    # As in the scene/H4 observers, retain the fresh output handle. Actual I/O can still fail
    # after preflight; report the run-owned partial files, without a rollback/transaction scheme.
    field_output = FileAccess.open(field_paths.output, FileAccess.WRITE)
    if field_output == null: return field_io_failure("output open failed: " + str(FileAccess.get_open_error()))
    field_created.output = true
    return true

func write_field_file(name: String, contents: String) -> bool:
    if not field_paths.has(name): return field_io_failure("destination was not admitted")
    var path: String = field_paths[name]
    if field_created.has(name):
        if name != "package": return field_io_failure("only this run's package may be rewritten")
    elif FileAccess.file_exists(path) or DirAccess.dir_exists_absolute(path):
        return field_io_failure(name + " is no longer fresh")
    var stream := FileAccess.open(path, FileAccess.WRITE)
    if stream == null: return field_io_failure(name + " open failed: " + str(FileAccess.get_open_error()))
    field_created[name] = true
    stream.store_string(contents)
    stream.flush()
    var error := stream.get_error()
    stream.close()
    if error != OK: return field_io_failure(name + " write failed: " + str(error))
    return true

func field_axis(value: float) -> void:
    var event := InputEventJoypadMotion.new()
    event.axis = JOY_AXIS_LEFT_X
    event.axis_value = value
    event.device = 0
    Input.parse_input_event(event)
    Input.flush_buffered_events()

func run_field_wait() -> void:
    if not admit_field_paths(): return
    # Use the same owned project and actual startup path. Native focus requires a visible window.
    root.size = Vector2i(960, 640)
    Engine.max_fps = 30 if remapped else 120
    var package: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/harbor-arrival.json"))
    var collision := OS.get_environment("SF2_INPUT_VARIANT") == "collision"
    package.battle.start.mainSeed = 0xC632A55A
    var entities: Array = package.world.maps[0].entities
    entities[0].actions = [{"op":"wait","ticks":2},{"op":"random-walk","x":2,"y":1,"radius":1}]
    if collision:
        entities[0].actions = [{"op":"move","x":1,"y":0}]
        entities.append({"id":"other","position":{"x":4,"y":1},"facing":2,"speed":96,
            "visible":true,"obstruction":true,"actions":[{"op":"move","x":-1,"y":0}]})
    if not write_field_file("package", JSON.stringify(integer_numbers(package))): return
    if not write_settings(field_paths.settings): return
    field_main_started = true
    print("field-wait-main-started pid=", OS.get_process_id())
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await create_timer(0.2).timeout
    var initial := read_sample("field-input")
    check(initial.canWaitAtInput, "Actual eligible field consumer")
    if not initial.focused: field_unavailable.append("Initial OS focus unavailable on this display")
    if not initial.focused or initial.failure != null:
        finish_public()
        return
    view.connect("SessionResultObserved", observe_wait)
    check(initial.help.contains("B / Pad Start" if remapped else "V / Pad RightStick") and
        initial.help.contains("including NPCs"), "Help uses effective Wait binding and explains NPC pause")
    await assert_field_paused("idle")
    await tap_wait()
    var tap := read_sample("tap")
    check(tap.simulationTick == initial.simulationTick + 1, "Tap advances one logical opportunity")
    if collision:
        check(tap.entities[1].targetX == 1152 and tap.entities[2].targetX == 1536,
            "Competing destination follows actual physical-slot collision order")
    else:
        check(tap.mainSeed == 0xC632A55A, "First opportunity remains in the NPC wait phase")
    await assert_field_paused("tap-released")
    var first_repeat := wait_receipts.size()
    release_at = first_repeat + 3
    physical(KEY_V, true)
    var pressed_count := wait_receipts.size()
    physical(KEY_V, true)
    check(wait_receipts.size() == pressed_count, "Repeated physical press while held is not a fresh Wait")
    # Simulate missed host callbacks, then observe the next actual callbacks; no deadline replay.
    OS.delay_msec(180)
    var deadline := Time.get_ticks_msec() + 3000
    while wait_receipts.size() < release_at and Time.get_ticks_msec() < deadline:
        await process_frame
    check(wait_receipts.size() == release_at, "Hold stops on exactly three accepted semantic Waits")
    for i in range(first_repeat + 1, wait_receipts.size()):
        check(wait_receipts[i].frame > wait_receipts[i - 1].frame and
            wait_receipts[i].micros - wait_receipts[i - 1].micros >= 16667,
            "Held repeat admits at most one per callback and obeys the monotonic 60/sec cap")
    release_at = -1
    var held := read_sample("semantic-four-waits")
    check(held.entities[1].moving, "Released NPC is actually partway through movement")
    if not collision: check(held.mainSeed == 0x1091A55A, "Same semantic N preserves shared RNG")
    await assert_field_paused("held-released-no-debt")
    if wait_axis:
        var count := wait_receipts.size()
        field_axis(0.8)
        check(wait_receipts.size() == count + 1, "Fresh positive deflection admits one Wait")
        field_axis(0.9)
        check(wait_receipts.size() == count + 1, "Held axis duplicate does not admit another fresh Wait")
        field_axis(-0.8)
        await assert_field_paused("opposite-axis-is-not-wait")
        check(wait_receipts.size() == count + 1, "Opposite direction releases rather than submitting positive Wait")
        field_axis(0.0)
        await tap_wait()
    # Another semantic action cancels hold even if that action has no field command.
    physical(KEY_V, true)
    physical(KEY_ESCAPE, true)
    physical(KEY_ESCAPE, false)
    await assert_field_paused("other-action-disarms")
    physical(KEY_V, false)
    await tap_wait()
    physical(KEY_V, true)
    view.hide()
    view.show()
    await assert_field_paused("hide-show-disarms")
    physical(KEY_V, false)
    await tap_wait()
    physical(KEY_V, true)
    paused = true
    await create_timer(0.1, true).timeout
    paused = false
    if wait_axis: field_axis(0.9)
    await assert_field_paused("tree-pause-disarms")
    physical(KEY_V, false)
    await tap_wait()
    # An already committed player move continues and discards budget at its new input boundary.
    physical(KEY_V, true)
    await key(KEY_A)
    for frame in range(120):
        await process_frame
        if state().canWaitAtInput: break
    var moved := read_sample("mandatory-player-move")
    check(moved.canWaitAtInput and moved.entities[0].x == 0, "Mandatory player movement reaches new field input")
    await assert_field_paused("movement-boundary-no-debt")
    physical(KEY_V, false)
    # Restart only the actual authored host to observe the initial dialogue consumer independently.
    host.free()
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    state()
    view.connect("SessionResultObserved", observe_wait)
    await key(KEY_ENTER)
    var dialogue := read_sample("dialogue-consumer")
    var count := wait_receipts.size()
    physical(KEY_V, true)
    await create_timer(0.1).timeout
    check(wait_receipts.size() == count, "Wait at dialogue does not produce a field receipt")
    if remapped:
        await key(KEY_ENTER)
        var revealed := read_sample("reveal-only")
        check(revealed.token == dialogue.token and revealed.visibleCharacters == revealed.totalCharacters and
            wait_receipts.size() == count, "Reveal-only Confirm keeps dialogue token and produces no Wait")
    await key(KEY_ENTER)
    if state().visibleCharacters >= 0 and state().visibleCharacters < state().totalCharacters:
        await key(KEY_ESCAPE) # The new choice text also needs reveal before a choice is submitted.
    await key(KEY_ESCAPE)
    check(state().canWaitAtInput, "Declining restores the field consumer")
    await assert_field_paused("consumer-return-disarmed")
    check(wait_receipts.size() == count, "Held Wait never rearms on consumer return")
    physical(KEY_V, false)
    await tap_wait()
    # Delivery may finish while Wait is held; a new field consumer still requires a fresh input.
    host.free()
    package.world.programs[0].instructions = [
        {"op":"present","kind":"FadeOut","resource":"white","entity":null,"position":null},
        {"op":"present","kind":"FadeIn","resource":"white","entity":null,"position":null},
        {"op":"end"}]
    if not write_field_file("package", JSON.stringify(integer_numbers(package))): return
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    state()
    view.connect("SessionResultObserved", observe_wait)
    await key(KEY_ENTER)
    check(state().wait == "PresentationWait", "Actual presentation consumer is pending")
    count = wait_receipts.size()
    physical(KEY_V, true)
    deadline = Time.get_ticks_msec() + 3000
    while not state().canWaitAtInput and Time.get_ticks_msec() < deadline:
        await process_frame
    check(state().canWaitAtInput and wait_receipts.size() == count,
        "Actual presentation completes without queuing a field Wait")
    await assert_field_paused("presentation-return-disarmed")
    physical(KEY_V, false)
    await tap_wait()
    # A real native sibling window transfers OS focus; never emit a focus notification manually.
    physical(KEY_V, true)
    var other := Window.new()
    other.hide()
    other.force_native = true
    other.transient = true
    other.title = "Field Wait focus observation"
    other.size = Vector2i(240, 100)
    root.add_child(other)
    other.show()
    other.grab_focus()
    await create_timer(0.15).timeout
    if not other.has_focus() or root.has_focus():
        field_unavailable.append("OS focus did not transfer to the native observation window")
        finish_public()
        return
    await assert_field_paused("unfocused")
    other.hide()
    await process_frame
    root.grab_focus()
    var focus_deadline := Time.get_ticks_msec() + 2000
    while not root.has_focus() and Time.get_ticks_msec() < focus_deadline:
        await process_frame
    if not root.has_focus():
        field_unavailable.append("OS focus did not return; focus rearm check was not reached")
        finish_public()
        return
    physical(KEY_V, true)
    await assert_field_paused("refocused-still-disarmed")
    other.queue_free()
    physical(KEY_V, false)
    await tap_wait()
    finish_public()

func projection_digest(bytes: PackedByteArray) -> String:
    var digest := HashingContext.new()
    digest.start(HashingContext.HASH_SHA256)
    digest.update(bytes)
    return digest.finish().hex_encode()

func projection_raster(width: int, height: int) -> Dictionary:
    var bytes := PackedByteArray()
    bytes.resize(width * height * 4)
    return {"width":width,"height":height,"format":"rgba8","data":Marshalls.raw_to_base64(bytes),
        "sha256":projection_digest(bytes)}

func projection_speech_count(s: Dictionary) -> int:
    return s.audio.receipts.filter(func(r): return r.Command == 65 and r.Operation == "started").size()

func projection_retained(s: Dictionary, delivered: Dictionary) -> bool:
    return s.dialogue == delivered.dialogue and s.visibleCharacters == delivered.visibleCharacters and \
        projection_speech_count(s) == projection_speech_count(delivered)

func run_field_projection() -> void:
    if not admit_field_paths(): return
    # Entirely authored input: no original graphics/audio or private comparison input.
    var package: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/harbor-arrival.json"))
    package.world.memberNames = ["Wrong", "Name{W2}", "Tail"]
    package.world.partyFlags = {"memberCount":3,"joinedStart":10,"activeStart":20,"capacity":1}
    package.start.flags = [11,21]
    package.start.textSettings = {"messageSpeed":2,"mouthControl":0,"viewSpeed":0}
    package.start.program = {"program":"projection","instruction":0}
    var mapping: Array = []
    mapping.resize(256)
    mapping.fill(1)
    var advances: Array = []
    advances.resize(80)
    advances.fill(6)
    package.world.textFont = {"asciiToSymbol":mapping,"advances":advances}
    package.world.texts[0].text = "{LEADER}{W1}"
    package.world.texts[1].text = "{LEADER}!{W1}"
    var maps: Array = []
    for map in package.world.maps:
        var layout: Array = []
        for row in range(31):
            var cells: Array = []
            cells.resize(31)
            cells.fill(0)
            layout.append(cells)
        map.layout = layout
        map.areas = [{"minX":0,"minY":0,"maxX":30,"maxY":30,"view":{"foregroundX":0,"foregroundY":0,
            "backgroundX":0,"backgroundY":0,"parallaxAX":256,"parallaxAY":256,"parallaxBX":256,"parallaxBY":256,
            "autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":0}}]
        for entity in map.entities: entity.sprite = 30
        maps.append({"map":map.id,"atlas":projection_raster(128,320),"scale":1,"blocks":[[0,0,0,0,0,0,0,0,0]],
            "music":[{"field":1,"battle":1}]})
    var pcm := PackedByteArray()
    pcm.resize(1600)
    for sample in range(800): pcm.encode_s16(sample * 2, 200 if sample % 20 < 10 else -200)
    package.world.presentation = {"maps":maps,"portraits":[],"sprites":[{"sprite":30,"portrait":null,"speech":65,
        "directions":[projection_raster(48,24),projection_raster(48,24),projection_raster(48,24)]}],
        "audio":[{"cue":"authored-speech","command":65,"timerB":0,"sampleRate":8000,"channels":1,"sampleFrames":800,
            "pcm16":Marshalls.raw_to_base64(pcm),"sha256":projection_digest(pcm),
            "loopBegin":null,"loopEnd":null}]}
    var music: Dictionary = package.world.presentation.audio[0].duplicate(true)
    music.cue = "authored-music"
    music.command = 1
    package.world.presentation.audio.append(music)
    package.world.programs.append({"id":"projection","entitiesRunning":false,"instructions":[
        {"op":"sprite","entity":"traveler","sprite":30},
        {"op":"text-cursor","text":100},
        {"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
        {"op":"wait-view"},{"op":"wait-ticks","ticks":6},
        {"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
        {"op":"close-text"},{"op":"end"}]})
    if not write_field_file("package", JSON.stringify(integer_numbers(package))): return
    if not write_settings(field_paths.settings): return
    root.size = Vector2i(960,640)
    Engine.max_fps = 60
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await process_frame
    var initial := read_sample("projection-initial")
    if initial.failure != null:
        finish_public()
        return
    view.connect("SessionResultObserved", record_warp_result)
    for occurrence in range(2):
        for frame in range(1000):
            if state().canWaitForText or state().failure != null: break
            await process_frame
        var delivered := read_sample("projection-ready-" + str(occurrence))
        if not delivered.canWaitForText:
            check(false, "Authored text must reach actual delivery and logical input")
            finish_public()
            return
        check(delivered.dialogue == ("Name{W2}" if occurrence == 0 else "Name{W2}!"),
            "Active leader and source-looking name characters are projected literally")
        if "instant" not in input_case:
            check(projection_speech_count(delivered) > 0, "Natural authored reveal starts actual speech audio")
        physical(KEY_ENTER, true)
        physical(KEY_ENTER, false)
        var transition := read_sample("projection-transition-" + str(occurrence))
        check(transition.wait == ("ViewWait" if occurrence == 0 else "TextCloseWait"), "Ack enters the owning pending transition")
        var preserved := projection_retained(transition, delivered)
        var arrived := false
        for frame in range(120):
            await process_frame
            var s := read_sample("projection-continuation-" + str(occurrence) + "-" + str(frame))
            if (occurrence == 0 and s.textId == 101) or (occurrence == 1 and s.canWaitAtInput):
                arrived = true
                break
            preserved = preserved and projection_retained(s, delivered)
        check(arrived, "Transition finishes by its actual new text or field-input state")
        check(preserved, "Same open window preserves text, visible characters and speech starts throughout continuation")
        var next := read_sample("projection-transition-finished-" + str(occurrence))
        if occurrence == 0:
            check(next.dialogue == "Name{W2}!" and next.textWindow == "OpenTextWindow", "New display replaces the retained projection")
            if "instant" not in input_case: check(next.visibleCharacters < next.totalCharacters, "New display begins its own reveal")
        else:
            check(next.textWindow == "ClosedTextWindow" and next.dialogue == "", "Actual close releases the retained projection")
    finish_public()

func run_text_wait() -> void:
    if not admit_field_paths(): return
    # Read the small producer output plus existing private asset subset; never rewrite it.
    var source_path := OS.get_environment("SF2_PORTRAIT_WAIT_INPUT")
    if not FileAccess.file_exists(source_path):
        field_io_failure("missing producer/portrait observation input")
        return
    var source: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(source_path))
    var package: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/harbor-arrival.json"))
    var collision := OS.get_environment("SF2_INPUT_VARIANT") == "collision"
    package.battle.start.mainSeed = 0xC632A55A
    package.start.program = {"program":"portrait-input","instruction":0}
    var visuals: Dictionary = source.presentation
    var map_visual: Dictionary = visuals.maps[0]
    visuals.maps = []
    for map in package.world.maps:
        var visual: Dictionary = map_visual.duplicate(true)
        visual.map = map.id
        visual.erase("music")
        visuals.maps.append(visual)
        for entity in map.entities: entity.sprite = 30
    package.world.presentation = visuals
    package.world.texts.append(source.sourceText)
    package.world.texts.append({"id":900,"text":"Plain input: delivery and reveal must leave this world unchanged."})
    package.world.texts.append({"id":901,"text":"The next input consumer requires a fresh Wait press."})
    package.world.maps[0].entities.append({"id":"entity-1","position":{"x":3,"y":2},"facing":2,
        "speed":96,"visible":true,"obstruction":true,"sprite":2})
    var actions := [{"op":"wait","ticks":2},{"op":"random-walk","x":2,"y":1,"radius":1}]
    if collision:
        actions = [{"op":"move","x":1,"y":0}]
        package.world.maps[0].entities.append({"id":"other","position":{"x":4,"y":1},"facing":2,
            "speed":96,"visible":true,"obstruction":true,"sprite":30})
    var instructions := [
        {"op":"sprite","entity":"traveler","sprite":30},
        {"op":"close-portrait"},{"op":"open-portrait","entity":"ferryman","flags":192},
        {"op":"text-cursor","text":100},
        {"op":"show-text","mode":"continued","speaker":"ferryman","explicitWindows":true},
        {"op":"close-text"},{"op":"open-portrait","entity":null,"flags":0},
        {"op":"text-cursor","text":900},
        {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
        {"op":"wait-text-input"},
        {"op":"close-portrait"},{"op":"close-text"},
        {"op":"call","target":{"program":source.sourceProgram.id,"instruction":0}},
        {"op":"motion","entity":"ferryman","wait":false,"actions":actions}]
    if collision:
        instructions.append({"op":"motion","entity":"other","wait":false,"actions":[{"op":"move","x":-1,"y":0}]})
    instructions.append_array([
        {"op":"text-cursor","text":900},
        {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
        {"op":"wait-text-input"},{"op":"close-text"},{"op":"text-cursor","text":901},
        {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
        {"op":"wait-text-input"},{"op":"close-text"},{"op":"end"}])
    package.world.programs.append({"id":"portrait-input","entitiesRunning":true,"instructions":instructions})
    package.world.programs.append(source.sourceProgram)
    if not write_field_file("package", JSON.stringify(integer_numbers(package))): return
    if not write_settings(field_paths.settings): return
    root.size = Vector2i(960, 640)
    Engine.max_fps = 30 if remapped else 120
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await settle_authored()
    root.grab_focus()
    await create_timer(0.15).timeout
    var initial := read_sample("portrait-open")
    if not initial.focused: field_unavailable.append("Initial OS focus unavailable on this display")
    if not initial.focused or initial.failure != null:
        finish_public()
        return
    view.connect("SessionResultObserved", observe_wait)
    check(initial.portraitId == 13 and initial.portraitFlags == 192 and initial.portraitProjection.id == 13,
        "Actual draw projects the persistent portrait and flags")
    await acknowledge()
    var preserved := read_sample("raw-text-preserves-portrait")
    check(preserved.portraitId == 13 and preserved.portraitProjection.id == 13 and not preserved.canWaitForText,
        "Text-only close, skipped open and speakerless raw text preserve portrait and reject Wait")
    physical(KEY_V, true)
    physical(KEY_V, false)
    await assert_field_paused("active-portrait-no-wait")
    await acknowledge()
    var single := read_sample("producer-single-text")
    check(single.portraitId == 1 and single.portraitProjection.id == 1 and not single.canWaitForText,
        "Real producer single-text caller opens its own portrait after explicit close")
    await acknowledge()
    var deadline := Time.get_ticks_msec() + 3000
    while not state().canWaitForText and Time.get_ticks_msec() < deadline: await process_frame
    var input := read_sample("plain-input-before-reveal")
    check(input.canWaitForText and input.portraitWindow == "ClosedPortraitWindow" and input.portraitProjection.id == -1,
        "Real producer close tail and mandatory sleep reach proven-Closed plain input")
    check(input.simulationTick == 10 and input.mainSeed == 0xC632A55A,
        "Source sleep ran exactly ten opportunities before NPC actions began")
    await assert_field_paused("plain-delivery-idle")
    if input.textMode != "instant":
        physical(KEY_V, true)
        await process_frame
        check(wait_receipts.is_empty(), "Wait during incomplete reveal is discarded")
        var before := state()
        await key(KEY_ENTER)
        var revealed := read_sample("plain-reveal-only")
        check(field_signature(revealed) == field_signature(before) and revealed.token == before.token,
            "Actual reveal Confirm adds no command, tick, RNG or entity update")
        physical(KEY_V, true)
        await assert_field_paused("reveal-return-disarmed")
        physical(KEY_V, false)
    await tap_wait()
    await assert_field_paused("plain-one-wait")
    release_at = 4
    physical(KEY_V, true)
    deadline = Time.get_ticks_msec() + 2000
    while wait_receipts.size() < 4 and Time.get_ticks_msec() < deadline: await process_frame
    physical(KEY_V, false)
    var four := read_sample("plain-four-waits")
    check(wait_receipts.size() == 4 and four.simulationTick == 14, "Exactly four semantic Waits at the same consumer")
    check(four.mainSeed == (0xC632A55A if collision else 0x1091A55A), "Only actual enabled NPC rules advance the shared RNG")
    await assert_field_paused("plain-motion-paused")
    var before_ack := state()
    await key(KEY_ENTER)
    var next := read_sample("next-plain-consumer")
    check(next.canWaitForText and next.token != before_ack.token and next.simulationTick == before_ack.simulationTick and
        next.mainSeed == before_ack.mainSeed and next.entities == before_ack.entities,
        "Input-first Ack adds no accepting poll or entity update")
    if next.visibleCharacters >= 0 and next.visibleCharacters < next.totalCharacters: await key(KEY_ENTER)
    physical(KEY_V, true)
    release_at = 6
    await key(KEY_ENTER)
    var returned := read_sample("plain-to-field-disarmed")
    check(returned.canWaitAtInput and not returned.waitingAtInput, "Consumer departure disarms the held Wait")
    physical(KEY_V, true)
    await assert_field_paused("field-return-no-held-debt")
    physical(KEY_V, false)
    finish_public()

func finish_public() -> void:
    var contents := JSON.stringify({"passed":failures.is_empty() and field_unavailable.is_empty(),
        "case":input_case,"failures":failures,"unavailable":field_unavailable,
        "maximumWhite":maximum_white,"completedWhite":completed_white,"samples":samples,"waitReceipts":wait_receipts,
        "warpRecords":warp_records,"cameraDraws":camera_draws}, "  ")
    if guarded_wait_case:
        field_output.store_string(contents)
        field_output.flush()
        var error := field_output.get_error()
        field_output.close()
        field_output = null
        if error != OK:
            field_io_failure("output write failed: " + str(error))
            return
        quit(1 if not failures.is_empty() else (0 if field_unavailable.is_empty() else 2))
        return
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(contents)
    file.close()
    quit(0 if failures.is_empty() else 1)


func record_warp_result(payload: String) -> void:
    var result: Dictionary = JSON.parse_string(payload)
    warp_records.append({"result":result, "state":state()})

func w1_settle_legacy() -> bool:
    # Only the already accepted three-input opening setup; its text is not W1 evidence.
    for frame in range(2000):
        await process_frame
        var s := state()
        if s.failure != null: return false
        if s.get("canWaitAtInput", false): return true
        if s.wait == "DialogueWait":
            # This legacy prefix still services entities during display. Preserve its
            # receipts rather than applying the admitted W1 no-service assertion.
            if s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters:
                await key(KEY_ENTER)
            await key(KEY_ENTER)
    return false

func w1_poll_signature(s: Dictionary) -> Array:
    return [s.simulationTick, s.mainSeed, s.randomSeedCopy, s.entities]

# The route ends at ordinary opening caller return, selected from actual state.
# The loop bounds only report a timeout; they never stand in for completion.
func opening_semantic(s: Dictionary) -> Array:
    return [s.simulationTick, s.mainSeed, s.randomSeedCopy, s.entities, s.flags, s.cursor, s.logicalText, s.logicalView,
        s.portraitWork, s.typewriting, s.entitiesRunning]

func opening_settle() -> bool:
    var seen: Dictionary = {}
    var phases: Dictionary = {}
    for frame in range(8000 if "camera" in input_case else 5000):
        var s := state()
        if s.failure != null: return false
        if s.get("canWaitAtInput", false): return true
        if "zone-nod" in input_case and s.wait == "NodWait":
            read_sample("nod-phase-" + str(s.token) + "-" + str(s.nod.Elapsed))
            if not nod_pause_checked and s.nod.Elapsed >= 12:
                nod_pause_checked = true
                var before_pause := opening_semantic(s)
                physical(KEY_ENTER, true)
                physical(KEY_ENTER, false)
                physical(KEY_V, true)
                physical(KEY_V, false)
                check(opening_semantic(state()) == before_pause, "Nod rejects field Wait and Ack input")
                view.hide()
                for hidden_frame in range(6): await process_frame
                check(opening_semantic(state()) == before_pause and state().tickDebt == 0, "Hidden nod has no service or clock debt")
                view.show()
                if OS.get_environment("SF2_NOD_FOCUS") == "1":
                    var other := Window.new()
                    other.hide()
                    other.force_native = true
                    other.transient = true
                    other.title = "Nod focus observation"
                    other.size = Vector2i(240, 100)
                    root.add_child(other)
                    other.show()
                    other.grab_focus()
                    await create_timer(0.15).timeout
                    if not other.has_focus() or root.has_focus():
                        field_unavailable.append("Nod OS focus transfer unavailable")
                        return false
                    var unfocused := opening_semantic(state())
                    for focus_frame in range(6): await process_frame
                    check(opening_semantic(state()) == unfocused and state().tickDebt == 0, "Unfocused nod adds no logical work")
                    read_sample("nod-unfocused")
                    other.hide()
                    root.grab_focus()
                    var deadline := Time.get_ticks_msec() + 2000
                    while not root.has_focus() and Time.get_ticks_msec() < deadline: await process_frame
                    if not root.has_focus():
                        field_unavailable.append("Nod OS focus return unavailable")
                        return false
                    other.queue_free()
                read_sample("nod-resumed")
        if "camera" in input_case and s.wait == "ViewWait" and s.logicalView.TargetSlot == null:
            read_sample("camera-wait-" + str(s.token))
            if not camera_pause_checked and s.logicalView.Scrolling:
                camera_pause_checked = true
                var before_pause := opening_semantic(s)
                physical(KEY_ENTER, true)
                physical(KEY_ENTER, false)
                physical(KEY_V, true)
                physical(KEY_V, false)
                check(opening_semantic(state()) == before_pause, "Camera wait rejects field Wait and Ack")
                view.hide()
                for hidden_frame in range(6): await process_frame
                check(opening_semantic(state()) == before_pause and state().tickDebt == 0, "Hidden camera has no service or clock debt")
                view.show()
                if OS.get_environment("SF2_NOD_FOCUS") == "1":
                    var other := Window.new()
                    other.hide()
                    other.force_native = true
                    other.transient = true
                    other.title = "Camera focus observation"
                    other.size = Vector2i(240, 100)
                    root.add_child(other)
                    other.show()
                    other.grab_focus()
                    await create_timer(0.15).timeout
                    if not other.has_focus() or root.has_focus():
                        field_unavailable.append("Camera OS focus transfer unavailable")
                        return false
                    var unfocused := opening_semantic(state())
                    for focus_frame in range(6): await process_frame
                    check(opening_semantic(state()) == unfocused and state().tickDebt == 0, "Unfocused camera adds no logical work")
                    read_sample("camera-unfocused")
                    other.hide()
                    root.grab_focus()
                    var deadline := Time.get_ticks_msec() + 2000
                    while not root.has_focus() and Time.get_ticks_msec() < deadline: await process_frame
                    if not root.has_focus():
                        field_unavailable.append("Camera OS focus return unavailable")
                        return false
                    other.queue_free()
                read_sample("camera-resumed")
        if "camera" in input_case and s.textId == 531 and s.get("canWaitForText", false):
            var endpoint := read_sample("camera-text531-input")
            check(not endpoint.fieldText.Wait2 and not endpoint.canWaitAtInput and endpoint.eventCaller == "ZoneEventContext" and
                endpoint.cursor != null and endpoint.cursor.Instruction == 127 and not 603.0 in endpoint.flags,
                "Text531 input retains the Messenger zone/script caller before yes-no")
            check(endpoint.logicalView.TargetSlot == null and not endpoint.logicalView.Scrolling, "Camera remains held after both waits")
            var held := opening_semantic(endpoint)
            for idle_frame in range(5): await process_frame
            check(opening_semantic(state()) == held and state().tickDebt == 0, "Text531 stops before any Wait or Ack")
            return true
        if "zone-nod" in input_case and "camera" not in input_case and s.textId == 521 and s.get("canWaitForText", false):
            var endpoint := read_sample("nod-text521-input")
            var returns: Array = warp_records.filter(func(r): return r.result.observations.any(func(o): return o.Kind == "nod-returned"))
            check(returns.size() == 2, "Both nods returned before the genuine text521 W1 input")
            check(not endpoint.fieldText.Wait2 and not endpoint.canWaitAtInput and endpoint.eventCaller == "ZoneEventContext" and
                endpoint.cursor != null and not 603.0 in endpoint.flags, "Text521 input retains the Messenger zone/script caller")
            var held := opening_semantic(endpoint)
            for idle_frame in range(5): await process_frame
            check(opening_semantic(state()) == held and state().tickDebt == 0, "Text521 stops before any Wait or Ack")
            return true
        if "portrait-event" in input_case and s.eventCaller != null:
            var phase := str(s.wait) + ":" + str(s.textId) + ":" + str(s.fieldText.Phase if s.fieldText != null else "")
            if s.portraitWork != null:
                phase += ":" + str([s.portraitWork.Registered, s.portraitWork.Moving, s.portraitWork.Closing,
                    s.portraitWork.Movement, s.portraitWork.EyesClosed, s.portraitWork.MouthOpen])
            if not phases.has(phase):
                phases[phase] = true
                read_sample("portrait-phase-" + phase)
        if s.wait == "FieldTextWait":
            var token := str(s.token)
            if not seen.has(token):
                seen[token] = true
                read_sample("opening-text-entry-" + str(s.textId))
            if "reveal" in input_case and s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters:
                var before := opening_semantic(s)
                physical(KEY_ENTER, true)
                physical(KEY_ENTER, false)
                check(opening_semantic(state()) == before, "Reveal-only input does not service text or entities")
                read_sample("opening-reveal-" + str(s.textId))
            if s.get("canWaitForText", false):
                var ready := read_sample("opening-ready-" + str(s.textId))
                var before := opening_semantic(ready)
                for idle_frame in range(5): await process_frame
                check(opening_semantic(state()) == before and state().tickDebt == 0, "Input boundary has no wall-clock debt")
                physical(KEY_V, true)
                physical(KEY_V, false)
                var polled := read_sample("opening-poll-" + str(s.textId))
                check(polled.simulationTick == ready.simulationTick + 1, "One explicit Wait owns one service opportunity")
                check((polled.entities != ready.entities) if ready.entitiesRunning else (polled.entities == ready.entities),
                    "A poll follows the current enabled or suppressed entity service")
                var audio_sequence: int = polled.audio.sequence
                physical(KEY_ENTER, true)
                physical(KEY_ENTER, false)
                var accepted := read_sample("opening-accepted-" + str(s.textId))
                var validation: bool = accepted.audio.receipts.any(func(r): return r.Sequence > audio_sequence and r.Command == 67 and r.Operation == "started")
                check(validation == bool(ready.fieldText.Wait2), "Only accepted W2 requests validation67")
        await process_frame
    return false

func run_portrait_event() -> void:
    if not admit_field_paths(): return
    if not write_settings(field_paths.settings): return
    root.size = Vector2i(960,640)
    Engine.max_fps = 60
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await process_frame
    var initial := read_sample("portrait-route-initial")
    if initial.failure != null or not initial.canWaitAtInput:
        check(false, "Retained bound start has actual field control")
        finish_public()
        return
    view.connect("SessionResultObserved", record_warp_result)
    if "camera" in input_case: RenderingServer.frame_post_draw.connect(record_camera_draw)
    var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_PRIVATE_EXPLORATION_PLAN")))
    # Existing accepted spatial edges only, never its historical frame/acknowledgement counts.
    var navigation: Array = fixture.expectedObservation.records[0].logicalInputTrace
    for edge in navigation:
        var before := state()
        var player: Dictionary = before.entities.filter(func(e): return e.id == "entity-0")[0]
        if before.map != "map-" + str(int(edge.map)) or player.x != edge.x * 384 or player.y != edge.y * 384:
            read_sample("portrait-route-position-discrepancy")
            check(false, "Live position differs from accepted spatial edge; no entry normalization")
            finish_public()
            return
        var interaction: bool = edge.waypoint == "map3-sarah-classroom" and edge.input == "C"
        var astral: bool = "zone-nod" in input_case and edge.waypoint == "map3-entity142" and edge.input == "C"
        if edge.input == "C" and not interaction and not astral:
            check(false, "Only the selected classroom and entity142 interactions are admitted")
            finish_public()
            return
        var actor_before: Dictionary = {}
        if interaction:
            actor_before = before.entities.filter(func(e): return e.id == "entity-1")[0]
            check(absi(int(actor_before.x - player.x)) + absi(int(actor_before.y - player.y)) == 384,
                "Live Sarah occupancy is adjacent before the ordinary interaction")
            read_sample("portrait-event-entry")
        await key({"Left":KEY_LEFT,"Right":KEY_RIGHT,"Up":KEY_UP,"Down":KEY_DOWN,"C":KEY_ENTER}[edge.input])
        if not await opening_settle():
            read_sample("portrait-route-stopped")
            check(false, "Bound route or portrait event stopped before actual caller return")
            finish_public()
            return
        if astral:
            var returned := read_sample("entity142-return")
            check(returned.canWaitAtInput and returned.eventCaller == null and 602.0 in returned.flags and 261.0 in returned.flags,
                "Entity142 caller returns after setting flags261/602")
        if "zone-nod" in input_case and edge.waypoint == "map3-astral-zone" and edge.x == 57 and edge.y == 13:
            var returned := read_sample("second-zone7-return")
            check(returned.canWaitAtInput and returned.eventCaller == null and 260.0 in returned.flags and not 603.0 in returned.flags,
                "Second Zone7 returns before Messenger")
        if "zone-nod" in input_case and edge.waypoint == "map3-school-stairs-up":
            read_sample("nod-stair-reload-return")
        if "zone-nod" in input_case and state().textId == (531 if "camera" in input_case else 521) and state().canWaitForText:
            finish_public()
            return
        if interaction:
            await process_frame
            var returned := read_sample("portrait-event-return")
            var actor: Dictionary = returned.entities.filter(func(e): return e.id == "entity-1")[0]
            check(returned.canWaitAtInput and returned.entityEvent == null and returned.textWindow == "ClosedTextWindow" and
                returned.portraitWindow == "ClosedPortraitWindow" and not returned.logicalText.Open,
                "Actual wrapper return closes both windows and releases control")
            check(256.0 in returned.flags and actor.x == 41 * 384 and actor.y == 7 * 384 and actor.facing == actor_before.facing,
                "Source movement, flag and facing restoration finish before control returns")
            check(returned.entitiesRunning == true, "Trap6 activation survives the script and close tail")
            var ready_ids: Array = samples.filter(func(r): return r.label.begins_with("opening-ready-")).map(func(r): return int(r.state.textId))
            check(ready_ids == [510,511,483,512,481], "Only reached W controls receive one Wait and Ack; trailing480 auto-completes")
            check(returned.audio.error == null, "Actual window/audio playback has no adapter error")
            if "zone" not in input_case:
                finish_public()
                return
        if "zone" in input_case and edge.waypoint == "map3-astral-zone-introduction":
            await process_frame
            var returned := read_sample("zone-introduction-return")
            check(returned.canWaitAtInput and returned.eventCaller == null and returned.textWindow == "ClosedTextWindow" and
                returned.portraitWindow == "ClosedPortraitWindow" and not returned.logicalText.Open,
                "Zone caller closes both windows and releases ordinary control")
            var ready_ids: Array = samples.filter(func(r): return r.label.begins_with("opening-ready-")).map(func(r): return int(r.state.textId))
            check(ready_ids == [510,511,483,512,481,513], "Spatial route reaches the first introduction through actual W controls")
            check(returned.entitiesRunning == true and returned.audio.error == null, "Zone service and actual audio remain admitted")
            check(not 602.0 in returned.flags and not 603.0 in returned.flags, "Stop before later Astral interactions and story flags")
            var zone_entries: Array = warp_records.filter(func(r): return r.result.observations.any(func(o): return o.Kind == "zone-entered"))
            check(zone_entries.size() >= 2, "Opening zone and introduction both use the generic source caller")
            var introduction: Dictionary = zone_entries.back().state if not zone_entries.is_empty() else {}
            if not introduction.is_empty():
                var mover: Dictionary = introduction.entities.filter(func(e): return e.id == "entity-0")[0]
                check(mover.moving and mover.actionCursor == 0 and introduction.eventCaller == "ZoneEventContext" and
                    introduction.entitiesRunning, "Zone handler enters with preserved player movement and pending init actions")
            if "zone-nod" not in input_case:
                finish_public()
                return
        read_sample("portrait-navigation-" + str(edge.waypoint) + "-" + str(int(edge.x)) + "-" + str(int(edge.y)))
    check(false, "Spatial route ended before the classroom event")
    finish_public()

func run_opening_text() -> void:
    if not admit_field_paths(): return
    if not write_settings(field_paths.settings): return
    root.size = Vector2i(960, 640)
    Engine.max_fps = 60
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await process_frame
    var initial := read_sample("opening-initial")
    if initial.failure != null or not initial.get("canWaitAtInput", false):
        check(false, "Bound start must reach ordinary control")
        finish_public()
        return
    view.connect("SessionResultObserved", record_warp_result)
    for direction in [KEY_LEFT, KEY_LEFT, KEY_RIGHT]:
        await key(direction)
        if not await opening_settle():
            read_sample("opening-stopped")
            check(false, "Ordinary opening failed to settle")
            finish_public()
            return
        read_sample("opening-field-return")
    var returned := read_sample("opening-returned")
    check(601.0 in returned.flags and returned.textWindow == "ClosedTextWindow" and returned.portraitWindow == "ClosedPortraitWindow",
        "Opening closes windows and completes the ordinary zone caller once")
    check(not returned.logicalText.Open and returned.canWaitAtInput, "Logical close joins actual field control")
    if "suppressed" in input_case:
        var adjacent := false
        for step in range(12):
            var s := state()
            var player: Dictionary = s.entities.filter(func(e): return e.id == "entity-0")[0]
            var actor: Dictionary = s.entities.filter(func(e): return e.id == "entity-128")[0]
            var dx := int(actor.x / 384) - int(player.x / 384)
            var dy := int(actor.y / 384) - int(player.y / 384)
            if absi(dx) + absi(dy) > 3: break
            await key(KEY_RIGHT if dx > 0 else (KEY_LEFT if dx < 0 else (KEY_DOWN if dy > 0 else KEY_UP)))
            if not await opening_settle(): break
            if absi(dx) + absi(dy) == 1:
                adjacent = true
                break
        check(adjacent, "Ordinary input faces the nearby actor")
        if adjacent:
            await key(KEY_ENTER)
            var saw_suppressed := false
            for frame in range(120):
                if state().wait == "FieldTextWait":
                    var entry := read_sample("suppressed-entry")
                    saw_suppressed = entry.entitiesRunning == false and entry.textId == 483 and entry.entityEvent != null
                    break
                await process_frame
            check(saw_suppressed, "Ordinary interaction admits the suppressed field-text consumer")
            check(await opening_settle(), "Suppressed interaction returns ordinary control")
            var closed := read_sample("suppressed-returned")
            check(not closed.logicalText.Open and closed.textWindow == "ClosedTextWindow" and closed.entityEvent == null,
                "Suppressed wrapper completes logical/projected close before returning control")
    finish_public()

func run_w1_interaction() -> void:
    if not admit_field_paths(): return
    if not write_settings(field_paths.settings): return
    root.size = Vector2i(960, 640)
    Engine.max_fps = 60
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await process_frame
    var initial := read_sample("legacy-setup-entry")
    if not initial.get("focused", false): field_unavailable.append("OS focus required for W1 input observation")
    if initial.failure != null or not initial.get("canWaitAtInput", false) or not field_unavailable.is_empty():
        check(initial.get("canWaitAtInput", false), "Existing retained start has field control")
        finish_public()
        return
    view.connect("SessionResultObserved", record_warp_result)
    for direction in [KEY_LEFT, KEY_LEFT, KEY_RIGHT]:
        await key(direction)
        if not await w1_settle_legacy():
            check(false, "Legacy three-input setup did not settle")
            finish_public()
            return
        read_sample("legacy-setup-step")
    var before_interaction := read_sample("ordinary-interaction-entry")
    check(before_interaction.portraitWindow == "ClosedPortraitWindow" and 602 not in before_interaction.flags,
        "Ordinary callback entry has the existing close and F602 clear")
    # Follow only this live nearby actor; no additional story route or injected state.
    var adjacent := false
    for frame in range(120):
        var s := state()
        var player: Dictionary = s.entities.filter(func(e): return e.id == "entity-0")[0]
        var actor: Dictionary = s.entities.filter(func(e): return e.id == "entity-128")[0]
        var dx := int(actor.x / 384) - int(player.x / 384)
        var dy := int(actor.y / 384) - int(player.y / 384)
        if absi(dx) + absi(dy) > 3 or s.map != "map-3": break
        var direction := KEY_RIGHT if dx > 0 else (KEY_LEFT if dx < 0 else (KEY_DOWN if dy > 0 else KEY_UP))
        await key(direction)
        if not await w1_settle_legacy(): break
        if absi(dx) + absi(dy) == 1:
            adjacent = true
            break
    if not adjacent:
        check(false, "Existing nearby entity could not be faced within this interaction")
        finish_public()
        return
    await key(KEY_ENTER)
    for frame in range(120):
        if state().wait == "W1TextWait" or state().failure != null: break
        await process_frame
    var entry := read_sample("w1-entry")
    check(entry.wait == "W1TextWait" and entry.textId == 483 and entry.entityEvent.Entity.Value == "entity-128" and
        entry.entitiesRunning == false and entry.portraitWindow == "ClosedPortraitWindow",
        "Ordinary Map3 entity event reaches admitted raw text483 with suppressed services and closed portrait")
    if entry.wait != "W1TextWait" or entry.failure != null:
        finish_public()
        return
    var before_reveal := state()
    # The optional early Wait is rejected during reveal, but avoid injecting it after instant delivery.
    if before_reveal.visibleCharacters >= 0 and before_reveal.visibleCharacters < before_reveal.totalCharacters:
        physical(KEY_V, true)
        physical(KEY_V, false)
        check(w1_poll_signature(state()) == w1_poll_signature(before_reveal), "Incomplete delivery rejects Wait")
        if OS.get_environment("SF2_W1_REVEAL") != "natural":
            await key(KEY_ENTER)
            check(w1_poll_signature(state()) == w1_poll_signature(before_reveal) and state().token == before_reveal.token,
                "Reveal-only Confirm consumes no W1 poll")
    for frame in range(2000):
        if state().canWaitForText: break
        await process_frame
    var revealed := read_sample("w1-revealed")
    check(revealed.canWaitForText and revealed.tickDebt == 0, "Actual text delivery enables the poll without debt")
    check(w1_poll_signature(revealed) == w1_poll_signature(entry), "Full natural or reveal-all delivery preserves admitted gameplay state")
    var paused := state()
    await create_timer(0.12).timeout
    check(w1_poll_signature(state()) == w1_poll_signature(paused) and state().tickDebt == 0,
        "Display time adds no W1 polls or debt")
    var polls := int(OS.get_environment("SF2_W1_POLLS"))
    for index in range(polls):
        var before := state()
        physical(KEY_V, true)
        # Leave the final press held across Ack to exercise the existing rearm boundary.
        if index != polls - 1:
            physical(KEY_V, false)
            await process_frame
        var after := read_sample("w1-optional-poll-" + str(index))
        check(after.simulationTick == before.simulationTick + 1 and after.entities == before.entities,
            "One optional poll advances one suppressed-service opportunity")
    var before_ack := read_sample("w1-before-ack")
    await key(KEY_ENTER)
    var returned := read_sample("w1-returned")
    check(returned.canWaitAtInput and returned.simulationTick == before_ack.simulationTick + 1,
        "Actual Ack consumes the mandatory poll and returns ordinary control")
    check(not returned.waitingAtInput and returned.tickDebt == 0, "Departure disarms held input without debt")
    check(not returned.audio.receipts.any(func(r): return r.Sequence > before_ack.audio.sequence and r.Command == 67 and r.Operation == "started"),
        "W1 acceptance adds no validation effect")
    if polls > 0:
        physical(KEY_V, true)
        await create_timer(0.12).timeout
        check(w1_poll_signature(state()) == w1_poll_signature(returned), "Held Wait cannot propagate into returned field control")
        physical(KEY_V, false)
    finish_public()

func run_warp_transition() -> void:
    if not admit_field_paths(): return
    root.size = Vector2i(960, 640)
    Engine.max_fps = 120
    var period := int(OS.get_environment("SF2_WARP_PERIOD"))
    var package: Dictionary
    if private_route:
        package = JSON.parse_string(FileAccess.get_file_as_string("res://../reference/inputs/map3-opening-start.json"))
        var binding: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_WARP_BINDING")))
        package.start.display = binding.display
        package.start.mapPalettes = binding.mapPalettes
        package.controlledBoundary += " Explicit selected R1 logical display binding; source and retained-record join documented by the egress-transition owner."
    else:
        package = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/harbor-arrival.json"))
        var pair := {"color2":0,"color3":14}
        package.start.display = {"period":period,"base":pair,"current":pair,"visibility":"base-restored"}
        for map in package.world.maps:
            map.basePalette = pair
            map.battle = null
            map.onLoad = null
            map.events = []
        package.world.maps[0].entities[0].position = {"x":4,"y":4}
        package.world.maps[0].entities[0].actions = [{"op":"wait","ticks":200}]
        package.world.maps[0].events = [{"kind":"warp","x":2,"y":1,"map":"yard-map","position":{"x":2,"y":2},
            "facing":0,"marker":null,"requiredFlag":null,"requiredValue":true}]
        package.world.maps[1].onLoad = {"program":"warp-init","instruction":0}
        var instructions: Array = [{"op":"wait-ticks","ticks":2},{"op":"set-flag","flag":602,"value":true}]
        if "restored" in input_case:
            instructions.append({"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null,"fullBlack":{"period":6}})
        if "failure" in input_case: instructions.append({"op":"native-call","symbol":"unbound-effect","source":"authored"})
        instructions.append({"op":"end"})
        package.world.programs.append({"id":"warp-init","instructions":instructions})
    if not write_field_file("package", JSON.stringify(integer_numbers(package))): return
    if not write_settings(field_paths.settings): return
    field_main_started = true
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    root.grab_focus()
    await create_timer(0.3).timeout
    var initial := read_sample("warp-initial")
    if not initial.get("focused", false): field_unavailable.append("OS focus required for the actual transition observation")
    if initial.failure != null or not initial.get("canWaitAtInput", false) or not field_unavailable.is_empty():
        check(initial.get("canWaitAtInput", false), "Real entry has field control")
        finish_public()
        return
    view.connect("SessionResultObserved", record_warp_result)
    if "failure" not in input_case: view.connect("SessionResultObserved", observe_wait)
    if private_route:
        check(initial.mapViewport != null and initial.mapViewport.width > 0 and initial.presentation.error == null and initial.entities.any(func(e): return e.sprite != null),
            "Private R1 uses real map and sprite resources")
        await key(KEY_LEFT)
        for frame in range(120):
            await process_frame
            if state().canWaitAtInput: break
        check(state().canWaitAtInput, "Private first step finishes without waiting for ambient NPC motion")
        read_sample("private-before-first-warp")
    await key(KEY_LEFT if private_route else KEY_RIGHT)
    var paused_once := false
    var saw_out := false
    var saw_black := false
    var saw_destination_black := false
    var saw_in := false
    var returned := false
    for frame in range(1000):
        await process_frame
        var s := state()
        if s.failure != null:
            samples.append({"label":"warp-failure","state":s})
            check("failure" in input_case and s.failureVisible and s.presentation.paletteBrightness == 0 and not s.canWaitAtInput,
                "Reached failure remains readable over the black world with control held")
            returned = "failure" in input_case
            break
        if s.wait == "FullFadeWait":
            if s.fade.Kind == 1: # FadeOut enum
                saw_out = true
                check(s.map == initial.map, "Old scene stays mounted through FadeOut delivery")
                if s.presentation.paletteBrightness == 0: saw_black = true
            else:
                saw_in = true
                if s.presentation.paletteBrightness == 0: saw_destination_black = true
            check(not s.canWaitAtInput, "Finite fade retains control")
            if not paused_once and "pause" in input_case:
                paused_once = true
                view.hide()
                var held := state()
                await create_timer(0.1).timeout
                check(state().simulationTick == held.simulationTick and state().presentation.paletteBrightness == held.presentation.paletteBrightness,
                    "Hidden transition consumes no logical or actual time")
                view.show()
                paused = true
                held = state()
                await create_timer(0.1, true).timeout
                check(state().simulationTick == held.simulationTick, "Tree pause consumes no transition service")
                paused = false
                await process_frame
                check(state().tickDebt < 1.0 / 60.0, "Resume carries no paused time debt")
                var other := Window.new()
                other.hide()
                other.force_native = true
                other.transient = true
                other.title = "Warp transition focus observation"
                other.size = Vector2i(240, 100)
                root.add_child(other)
                other.show()
                other.grab_focus()
                await create_timer(0.1).timeout
                if not other.has_focus() or root.has_focus():
                    field_unavailable.append("Native transition focus did not transfer")
                    finish_public()
                    return
                held = state()
                await create_timer(0.1).timeout
                check(state().simulationTick == held.simulationTick and state().presentation.paletteBrightness == held.presentation.paletteBrightness,
                    "Real focus loss pauses logical and actual transition time")
                other.hide()
                root.grab_focus()
                await process_frame
                other.queue_free()
                if not root.has_focus():
                    field_unavailable.append("Native transition focus did not return")
                    finish_public()
                    return
                check(state().tickDebt < 1.0 / 60.0, "Focus return carries no elapsed debt")
                OS.delay_msec(120) # Delayed real callback, never fabricated completion.
        if s.wait == "WarpLoadWait":
            saw_black = saw_black or s.presentation.paletteBrightness == 0
            check(s.presentation.paletteBrightness == 0 and not s.canWaitAtInput, "Load gate is actually black")
        if s.canWaitAtInput:
            returned = true
            samples.append({"label":"warp-visible-return","state":s})
            check(s.presentation.paletteBrightness == 1 and s.display.Visibility == 1 and s.display.Base == s.display.Current,
                "Field control follows real and logical visible return")
            check(s.tickDebt == 0, "Visible return clears unused batch time")
            await assert_field_paused("warp-ready-no-debt")
            await tap_wait()
            read_sample("warp-next-input")
            break
        if frame % 4 == 0: samples.append({"label":"warp-progress","state":s})
    check(returned and saw_out and saw_black, "Bounded transition reaches its complete outcome through old-scene black")
    if "failure" not in input_case:
        check(saw_in and saw_destination_black, "Destination is mounted black and actually faded visible")
        check(state().presentation.paletteFades == 2, "Exactly one FadeOut and one FadeIn, including onLoad restoration")
    finish_public()
