extends SceneTree

# Direct native observation. It drives real input events and reads the running view; no images,
# session setters, synthetic receipts, unit-test harness, or runtime RNG/state reset.
var samples: Array = []
var failures: Array = []
var view: Node

func _initialize() -> void:
    call_deferred("_run")

func _read(label: String) -> Dictionary:
    var snapshot: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    samples.append({"label": label, "state": snapshot})
    return snapshot

func _check(condition: bool, description: String) -> void:
    if not condition:
        failures.append(description)
        push_error(description)

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
    var scene := load("res://Main.tscn") as PackedScene
    var host := scene.instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    # Apply after the view has selected its actual-window scaling policy, not during host bootstrap.
    await _resize(Vector2i(960, 540))
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        failures.append("Authored view did not start.")
        _finish()
        return
    var initial := _read("initial")
    _check(initial.viewport.width == 960 and initial.viewport.height == 540, "Representative initial viewport is 960x540")
    if initial.failure != null:
        failures.append("Startup: " + str(initial.failure))
        _finish()
        return
    var arguments := OS.get_cmdline_user_args()
    var case_index := arguments.find("--observation-case")
    if case_index >= 0 and case_index + 1 < arguments.size():
        if arguments[case_index + 1] == "target-cycle":
            await _target_cycle(initial)
            _finish()
            return
        if arguments[case_index + 1] == "layout":
            await _layout(initial, arguments.has("--long-path"))
            _finish()
            return
        if arguments[case_index + 1] == "physical":
            await _physical(initial, arguments.has("--reverse-kills"))
            _finish()
            return
        failures.append("Unknown observation case.")
        _finish()
        return
    _check(initial.round == 1 and initial.stage == "Movement", "Natural initial player control")
    var actor: Dictionary = initial.actors[0]
    _check(actor.visible and actor.nodeX == actor.x * 40 + 2 and actor.nodeY == actor.y * 40 + 4,
        "Actual actor node projects committed coordinates")
    await _press(KEY_D)
    var moved := _read("movement-preview")
    _check(moved.previewX == actor.x + 1 and moved.actors[0].x == actor.x and moved.mainSeed == initial.mainSeed,
        "Real movement input is provisional")
    await _press(KEY_ENTER)
    await _press(KEY_ESCAPE)
    var cancelled := _read("cancelled")
    _check(cancelled.previewX == actor.x and cancelled.stage == "Movement" and cancelled.mainSeed == initial.mainSeed,
        "Real cancellation returns to committed state")
    await _press(KEY_ENTER)
    await _press(KEY_H)
    var selected := _read("self-heal-selected")
    _check(selected.stage == "CommitReady" and selected.actors[0].hp == actor.hp, "HEAL selection has no effects")
    await _press(KEY_ENTER)
    var healed := _read("heal-committed")
    var expected_hp := 100 if initial.map == "yard-map" else 20
    var expected_exp := 10 if initial.map == "yard-map" else 15
    _check(healed.actors[0].hp == expected_hp and healed.actors[0].mp == actor.mp - 3
        and healed.actors[0].exp == expected_exp, "Actual HEAL HP/MP/EXP")
    _check(healed.mainSeed == 4283241012 and healed.thinkingSeed == initial.thinkingSeed, "HEAL carries exact RNG images")
    _check(healed.actor != initial.actor and healed.stopReason == "PlayerInput", "Application advances to next live actor")
    _check(healed.roster.contains("HP " + str(expected_hp)) and healed.actors[0].text.contains(str(expected_hp)),
        "Actual HUD and actor node project healed state")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_round := _read("automatic-next-round")
    _check(next_round.round == 2 and next_round.actor == initial.actor and next_round.mainSeed == 2656571956,
        "STAY naturally reaches round 2 with carried RNG")
    var saw_ai := false
    for state in [healed, next_round]:
        for observation in state.observations:
            saw_ai = saw_ai or observation.Kind == "ai-stay"
    _check(saw_ai, "Configured Stay AI executes without an adapter scheduling command")
    await _press(KEY_ENTER)
    await _press(KEY_H)
    await _press(KEY_TAB)
    var rejected := _read("range-rejected")
    _check(rejected.failure == "target-range" and rejected.failureKind == "IllegalCommand"
        and rejected.mainSeed == next_round.mainSeed, "Range rejection reaches the live view without RNG mutation")
    await _press(KEY_ESCAPE)
    await _press(KEY_ENTER)
    await _press(KEY_X)
    var unsupported := _read("physical-unsupported")
    _check(unsupported.failure == "physical-attack" and unsupported.failureKind == "UnsupportedCapability",
        "Unsupported capability stays distinct in the live view")
    _finish()

func _same_battle(before: Dictionary, after: Dictionary) -> bool:
    for field in ["mainSeed", "thinkingSeed", "round", "actor"]:
        if before[field] != after[field]:
            return false
    for index in range(before.actors.size()):
        for field in ["hp", "mp", "exp", "x", "y"]:
            if before.actors[index][field] != after.actors[index][field]:
                return false
    return true

func _target_cycle(initial: Dictionary) -> void:
    await _press(KEY_ENTER)
    await _press(KEY_H)
    var selected := _read("self-selected")
    await _press(KEY_TAB)
    var rejected := _read("first-tab-range-rejected")
    _check(rejected.failure == "target-range" and rejected.target == initial.actor
        and rejected.candidate == "guard-a" and rejected.revision == selected.revision
        and _same_battle(selected, rejected), "Rejected candidate preserves selected target, revision and battle/RNG")
    await _press(KEY_TAB)
    var later := _read("second-tab-later-legal-target")
    _check(later.failure == null and later.target == "guard-c" and later.stage == "CommitReady"
        and _same_battle(selected, later), "Tab reaches the legal ally after a rejected candidate")
    await _press(KEY_TAB)
    var wrapped := _read("third-tab-wraps-to-self")
    _check(wrapped.failure == null and wrapped.target == initial.actor, "Target cycle wraps normally")
    await _press(KEY_ESCAPE)
    var cancelled := _read("cancel-clears-candidate")
    _check(cancelled.candidate == null and cancelled.target == null and _same_battle(initial, cancelled),
        "Cancel clears only provisional selection and UI cursor")
    await _press(KEY_ENTER)
    await _press(KEY_H)
    await _press(KEY_TAB)
    await _press(KEY_TAB)
    await _press(KEY_ENTER)
    var committed := _read("later-target-heal-committed")
    _check(committed.failure == null and committed.actors[0].hp == initial.actors[0].hp
        and committed.actors[0].mp == initial.actors[0].mp - 3, "Real input commits HEAL to the later ally, not self")
    var healed_later := false
    for observation in committed.observations:
        if observation.Kind == "hp" and observation.Actor.Value == "guard-c":
            healed_later = observation.After > observation.Before
    _check(healed_later and committed.candidate == null, "Later ally receives healing and the next action has no stale UI cursor")

func _rectangle(value: Dictionary) -> Rect2:
    return Rect2(value.x, value.y, value.width, value.height)

func _visible_layout(state: Dictionary, label: String) -> void:
    var viewport := _rectangle(state.viewport)
    var map_viewport := _rectangle(state.mapViewport)
    var hud := _rectangle(state.hud)
    _check(viewport.encloses(map_viewport) and viewport.encloses(hud) and not map_viewport.intersects(hud),
        label + ": map/HUD regions fit without overlap")
    _check(state.hudClipsContents and state.previewInsideMap, label + ": HUD clips scrolling content and preview is visible")
    for text_node in state.hudLabels:
        _check(text_node.rect.height > 0 and text_node.rect.width <= hud.size.x,
            label + ": HUD text retains usable height and bounded width")
    for actor in state.actors:
        if actor.id == state.actor or actor.id == state.target:
            _check(actor.visible and actor.insideMap and viewport.encloses(_rectangle(actor.globalRect)),
                label + ": current actor/target is fully visible")

func _resize(size: Vector2i) -> void:
    root.size = size
    for frame in range(6):
        await process_frame

func _wheel_hud(state: Dictionary) -> void:
    for step in range(5):
        var event := InputEventMouseButton.new()
        event.position = _rectangle(state.hud).get_center()
        event.global_position = event.position
        event.button_index = MOUSE_BUTTON_WHEEL_DOWN
        event.factor = 1.0
        event.pressed = true
        Input.parse_input_event(event)
        await process_frame
    for frame in range(6):
        await process_frame

func _layout(initial: Dictionary, long_path: bool) -> void:
    _visible_layout(initial, "initial")
    _check(initial.mapWidth == 48 and initial.actors[0].x == 47, "Admitted full-width map begins at the right edge")
    await _press(KEY_A)
    var moved := _read("edge-preview")
    _check(moved.previewX == 46 and moved.actors[0].x == 47 and _same_battle(initial, moved),
        "Edge input previews movement without committing battle state")
    _visible_layout(moved, "edge-preview")
    if long_path:
        for step in range(44):
            await _press(KEY_A)
        for step in range(42):
            await _press(KEY_S)
        var distant := _read("long-path-frame")
        _check(distant.failure == null and distant.previewX == 2 and distant.previewY == 45
            and distant.zoom < 1 and _same_battle(initial, distant), "Long valid preview zooms to keep origin and destination visible")
        _visible_layout(distant, "long-path-frame")
    await _resize(Vector2i(640, 480))
    var narrow := _read("narrow-window")
    _check(narrow.viewport.width == 640 and narrow.viewport.height == 480, "UI follows the actual resized viewport")
    _visible_layout(narrow, "narrow-window")
    await _wheel_hud(narrow)
    var scrolled := _read("hud-wheel-scroll")
    _check(scrolled.hudScroll > narrow.hudScroll and _same_battle(narrow, scrolled),
        "Actual wheel input exposes overflowing HUD without gameplay changes")
    await _resize(Vector2i(1280, 720))
    var wide := _read("wide-window")
    _check(wide.viewport.width == 1280 and wide.viewport.height == 720, "Wide viewport reflows the two regions")
    _visible_layout(wide, "wide-window")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var committed := _read("movement-committed-next-control")
    _check(committed.failure == null and committed.actors[0].x == wide.previewX
        and committed.actors[0].y == wide.previewY, "Visible preview can be committed through actual input")
    _visible_layout(committed, "next-control")

func _physical(initial: Dictionary, reverse: bool) -> void:
    var player: String = initial.actor
    var first_index := 3 if reverse else 2
    var second_index := 2 if reverse else 3
    await _press(KEY_ENTER)
    await _press(KEY_X)
    await _press(KEY_TAB)
    await _press(KEY_TAB)
    await _press(KEY_TAB)
    var rejected := _read("physical-range-rejected")
    _check(rejected.failure == "target-range", "Distant opponent fails range validation")
    _check(rejected.target == initial.actors[3].id and rejected.candidate == initial.actors[4].id,
        "Rejected physical cursor retains authoritative target")
    await _press(KEY_TAB)
    if reverse:
        await _press(KEY_TAB)
    var selected := _read("first-physical-selected")
    _check(selected.failure == null and selected.target == initial.actors[first_index].id,
        "Physical candidate cycles past rejection")
    _check(selected.mainSeed == initial.mainSeed and selected.actors[first_index].hp == 15,
        "Target selection leaves resources and RNG unchanged")
    await _press(KEY_ENTER)
    var killed := _read("first-death-next-control")
    var first_exp := 23 if initial.map == "stone-court-map" else 48
    _check(killed.failure == null and killed.mainSeed == 176099892 and killed.actors[0].exp == first_exp,
        "Physical attack carries exact RNG and EXP")
    _check(killed.actors[first_index].hp == 0 and killed.actors[first_index].x == null
        and not killed.actors[first_index].visible, "Dead enemy leaves occupancy and visible battlefield")
    _check(killed.gold == initial.gold + 17 + first_index and killed.actors[0].kills == 1,
        "First death credits configured gold and killer")
    _check(killed.actor == initial.actors[1].id and killed.stopReason == "PlayerInput", "Automatic next living player")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_round := _read("physical-next-round")
    _check(next_round.actor == player and next_round.round == 2 and next_round.mainSeed == 3184202292,
        "Next round consumes only living turn entries")
    await _press(KEY_ENTER)
    await _press(KEY_X)
    await _press(KEY_TAB)
    _check(_read("second-physical-selected").target == initial.actors[second_index].id,
        "Second legal kill uses the opposite opponent")
    await _press(KEY_ENTER)
    var second := _read("second-death-next-control")
    var final_exp := 48 if initial.map == "stone-court-map" else 98
    _check(second.failure == null and second.actors[0].exp == final_exp and second.mainSeed == 3226014260,
        "Second physical action preserves expected EXP and RNG")
    _check(second.gold == initial.gold + 39 and second.actors[0].kills == 2,
        "Both kill orders settle the same gold and kill count")
    _check(second.actor == initial.actors[1].id and second.actors[second_index].x == null
        and not second.actors[second_index].visible, "Second death cleans up and returns actual control")
    _check(second.thinkingSeed == initial.thinkingSeed, "Stay AI does not consume thinking RNG")

func _finish() -> void:
    var report := {"samples": samples, "failures": failures, "passed": failures.is_empty()}
    var args := OS.get_cmdline_user_args()
    var output_index := args.find("--observation-output")
    if output_index >= 0 and output_index + 1 < args.size():
        var output := FileAccess.open(args[output_index + 1], FileAccess.WRITE)
        if output != null:
            output.store_string(JSON.stringify(report, "\t"))
            output.close()
        else:
            failures.append("Could not write observation output.")
    print("SF2_ENGINE_OBSERVATION " + JSON.stringify({"passed": failures.is_empty(), "samples": samples.size(), "failures": failures}))
    quit(0 if failures.is_empty() else 1)
