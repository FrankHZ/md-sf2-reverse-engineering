extends "res://probes/engine_action_choice_observation.gd"

# Bounded actual decision consumer. Same content, ordinary C# rebuild/restart selection,
# real player keys and presenter completion; expected variant exists only in this observer.
var stages: Array = []

func _result(json: String) -> void:
    super(json)
    var result: Dictionary = JSON.parse_string(json)
    if result.observations.any(func(event): return event.Kind in ["ai-target", "battle-movement-finished", "scene-prepared", "action-committed"]):
        stages.append(_state())

func run(owner: SceneTree, initial: Dictionary) -> void:
    _start(owner)
    var expected: String = observer._diagnostic("EXPECT_TARGET")
    observer._check(expected in ["swordsman", "lookout"], "An external selector names the expected authored algorithm target")
    observer._check(initial.actor == "swordsman" and initial.queueCursor == 0 and initial.mainSeed == 0x00C01234,
        "Five three-draw source round candidates hand the same admitted state to both algorithms")
    observer._check(initial.actors[0].hp == 400 and initial.actors[1].hp == 100 and initial.actors[2].hp == 500,
        "Same starting resources contain a first legal and a distinct lowest-HP candidate")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_SPACE)
    var ready: Dictionary = observer._read("decision-ready")
    observer._check(observer._same_battle(initial, ready), "Provisional player Stay does not invoke automatic decision or draws")
    await observer._press(KEY_ENTER)
    var begun: Dictionary = observer._read("automatic-decision-movement")
    observer._check(begun.failure == null and begun.mainSeed == initial.mainSeed and begun.thinkingSeed == initial.thinkingSeed,
        "Decision and live movement carry no future physical construction or repeated thinking")
    observer._check(begun.queueCursor == 1 and begun.aiMemory[0].lastTarget == expected,
        "Selected target memory is published once while automatic queue entry is retained")
    var completed: Dictionary = await _settle("decision-completed")
    var sword: bool = expected == "swordsman"
    observer._check(completed.failure == null and completed.stopReason == "PlayerInput" and completed.actor == "lookout"
        and completed.queueCursor == 2 and not completed.scene.visible, "Actual presentation finishes and releases next ordinary player input")
    observer._check(completed.actors[0].hp == (399 if sword else 400) and completed.actors[1].hp == (100 if sword else 99)
        and completed.actors[2].hp == (487 if sword else 499), "Selected physical applies first damage1 and the source counter13 or1 to actual live HP")
    observer._check(completed.actors[0 if sword else 1].exp == 1 and completed.gold == initial.gold
        and completed.actors[0].mp == initial.actors[0].mp and completed.actors[1].mp == initial.actors[1].mp,
        "The selected ally receives exactly the admitted counter award; unrelated resources stay unchanged")
    observer._check(completed.actors[2].x != initial.actors[2].x or completed.actors[2].y != initial.actors[2].y,
        "Both algorithms execute a real movement segment before physical construction")
    observer._check(completed.actors[2].visible and completed.actors[2].nodeX == completed.actors[2].x * 40 + 2
        and completed.actors[2].nodeY == completed.actors[2].y * 40 + 4, "Actual field node settles on the committed automatic destination")
    _rng(initial.mainSeed, 0x557E1234, 0x976E1234, 14, 48)
    observer._check(completed.thinkingSeed == initial.thinkingSeed and completed.aiMemory[0].lastTarget == expected,
        "Neither movement nor scene completion reruns the chosen decision or consumes thinking")
    var decisions := 0
    var automatic_commits := 0
    var player_commits := 0
    var moves := 0
    var finished_move := -1
    var first_construction := -1
    var hits: Array = []
    for index in range(facts.size()):
        var event: Dictionary = facts[index]
        if event.Kind == "ai-target":
            decisions += 1
            observer._check(event.Actor.Value == "raider" and event.Target.Value == expected, "One automatic result selects the externally expected target")
        if event.Kind == "battle-movement-segment-started":
            moves += 1
        if event.Kind == "battle-movement-finished":
            finished_move = index
        if event.Kind.begins_with("rng-") and first_construction == -1:
            first_construction = index
        if event.Kind.begins_with("physical-"):
            hits.append(event.Kind)
            var counter: bool = event.Kind == "physical-counter"
            observer._check(event.Actor.Value == (expected if counter else "raider") and event.Target.Value == ("raider" if counter else expected),
                "The same physical scene consumer retains selected actor/target and counter reversal")
        if event.Kind == "action-committed":
            if event.Actor.Value == "raider":
                automatic_commits += 1
            if event.Actor.Value == "swordsman":
                player_commits += 1
        observer._check(event.Kind != "thinking-rng", "Authored target priorities require no source thinking draw")
    observer._check(decisions == 1 and automatic_commits == 1 and player_commits == 1 and moves > 0
        and finished_move >= 0 and first_construction > finished_move and hits == ["physical-first", "physical-counter"],
        "One decision, actual finished movement, physical construction, staged hits and one queue consumption stay ordered")
    observer.samples.append({"label": "decision-boundary-states", "states": stages.duplicate(true)})
    var retained_during_movement := false
    for state in stages:
        if state.queueCursor == 1 and state.mainSeed == initial.mainSeed and state.actors[2].x == initial.actors[2].x and state.actors[2].hp == initial.actors[2].hp:
            retained_during_movement = true
    observer._check(retained_during_movement,
        "Actual result readback retains unchanged resources/placement/main RNG during movement delivery")
    await observer.process_frame
    var stable: Dictionary = observer._read("decision-stable-player-input")
    observer._check(stable.revision == completed.revision and stable.mainSeed == completed.mainSeed
        and stable.thinkingSeed == completed.thinkingSeed and stable.failure == null, "No stale wait or further automatic work remains at settled player input")
