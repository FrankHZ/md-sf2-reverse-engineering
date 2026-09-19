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
        KEY_F:"attack", KEY_H:"spell", KEY_TAB:"target", KEY_SPACE:"stay"}[code]
    if use_pad:
        var buttons := {"up":JOY_BUTTON_DPAD_UP,"right":JOY_BUTTON_DPAD_RIGHT,
            "down":JOY_BUTTON_DPAD_DOWN,"left":JOY_BUTTON_DPAD_LEFT,
            "confirm":JOY_BUTTON_A,"cancel":JOY_BUTTON_B,"attack":JOY_BUTTON_X,
            "spell":JOY_BUTTON_Y,"target":JOY_BUTTON_RIGHT_SHOULDER,"stay":JOY_BUTTON_LEFT_SHOULDER}
        if remapped:
            buttons.merge({"confirm":JOY_BUTTON_Y,"cancel":JOY_BUTTON_X,"attack":JOY_BUTTON_A,
                "spell":JOY_BUTTON_B,"target":JOY_BUTTON_LEFT_SHOULDER,"stay":JOY_BUTTON_RIGHT_SHOULDER}, true)
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
                "confirm":KEY_E,"cancel":KEY_Q,"attack":KEY_R,"spell":KEY_T,"target":KEY_U,"stay":KEY_O}[action]
        event.pressed = pressed
        Input.parse_input_event(event)

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

func write_settings(path: String) -> void:
    var settings := {"formatVersion":1,"confirmCancel":"swapped" if remapped else "standard",
        "textMode":"adjustable" if remapped else "instant", "charactersPerSecond":20,
        "reducedFlash":remapped,"bindings":{}}
    if private_route:
        settings.textMode = "instant"
        settings.reducedFlash = false
    if OS.get_environment("SF2_INPUT_RATE") != "": settings.charactersPerSecond = int(OS.get_environment("SF2_INPUT_RATE"))
    if remapped:
        var keys := {"up":"I","right":"L","down":"K","left":"J","confirm":"Q","cancel":"E",
            "attack":"R","spell":"T","target":"U","stay":"O"}
        var buttons := {"up":"DpadUp","right":"DpadRight","down":"DpadDown","left":"DpadLeft",
            "confirm":"West","cancel":"North","attack":"South","spell":"East","target":"LeftShoulder","stay":"RightShoulder"}
        var axes := {"up":["RightY-"],"right":["RightX+"],"down":["RightY+"],"left":["RightX-"]}
        for action in keys:
            settings.bindings[action] = {"keys":[keys[action]],"buttons":[buttons[action]],"axes":axes.get(action, [])}
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify(settings))
    file.close()

func run() -> void:
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

func finish_public() -> void:
    var output := OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"case":input_case,"failures":failures,
        "maximumWhite":maximum_white,"completedWhite":completed_white,"samples":samples}, "  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
