extends SceneTree

# Direct native observation. It drives real input events and reads the running view; no images,
# setters, synthetic receipts, unit-test harness, or runtime RNG/state reset.
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
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        failures.append("Authored view did not start.")
        _finish()
        return
    var initial := _read("initial")
    if initial.failure != null:
        failures.append("Startup: " + str(initial.failure))
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
