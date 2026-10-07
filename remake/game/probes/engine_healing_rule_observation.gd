extends RefCounted

# A bounded HEAL responsibility of the existing native observer. Reads only the
# actual view and uses real input; retains phase changes, not per-frame captures.
var observer: SceneTree
var facts: Array = []

func _result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if event.Kind not in ["scene-logical-step", "scene-delivery"]:
            facts.append(event)

func _state() -> Dictionary:
    return JSON.parse_string(observer.view.call("ReadObservationJson"))

func _settle_movement() -> void:
    var deadline := Time.get_ticks_msec() + 10000
    while _state().stopReason != "PlayerInput" and Time.get_ticks_msec() < deadline:
        await observer.process_frame
    observer._check(_state().stopReason == "PlayerInput", "Actual movement presenter completed")

func _settle_heal(label: String) -> Dictionary:
    var deadline := Time.get_ticks_msec() + 20000
    var phase := ""
    while Time.get_ticks_msec() < deadline:
        var current := _state()
        if current.failure != null:
            observer._check(false, "Consumer failed: " + str(current.failure))
            break
        if not current.scene.visible:
            return observer._read(label)
        if phase != current.scene.phase:
            phase = current.scene.phase
            observer._read(label + ":" + phase)
        var healing = current.scene.healing
        # Mandatory work and presentation completion come from the real consumer.
        # Supply only player acknowledgements at eligible settled message input.
        if phase in ["ActionMessage", "ResultMessage", "RewardMessage", "GrowthMessage"] \
            and (healing == null or healing.AtTimedInput or healing.LogicalComplete):
            await observer._press(KEY_ENTER)
        else:
            await observer.process_frame
    observer._check(false, "HEAL consumer reached its bounded completion deadline")
    return observer._read(label + ":incomplete")

func run(owner: SceneTree, initial: Dictionary) -> void:
    observer = owner
    observer.view.connect("SessionResultObserved", Callable(self, "_result"))
    var half: bool = initial.rules == "authored-healing-b"
    observer._check(initial.rules in ["authored-healing-a", "authored-healing-b"], "Compiled ordinary factory selects an authored HEAL algorithm")
    observer._check(initial.actor == "medic-a" and initial.actors[0].hp == 30 and initial.actors[0].mp == 8,
        "Real Content initializes the common session")
    var option: Dictionary = initial.healingChoices.Spells[0]
    observer._check(option.Enabled and option.Targets[0].Enabled and option.Targets[1].Enabled == not half,
        "Same-session target query exposes full-HP distinction")
    if half:
        observer._check(option.Targets[1].Reason.Code == "target-not-injured", "Query carries the selected rule's rejection reason")
    await observer._press(KEY_D)
    await _settle_movement()
    var moved: Dictionary = observer._read("healing-movement-preview")
    observer._check(moved.previewX == 4 and moved.actors[0].x == 3 and moved.mainSeed == initial.mainSeed,
        "Movement remains provisional without RNG publication")
    await observer._press(KEY_ESCAPE)
    await _settle_movement()
    var cancelled: Dictionary = observer._read("healing-cancel")
    observer._check(cancelled.previewX == 3 and cancelled.mainSeed == initial.mainSeed, "Actual input cancels the provisional choice")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_H)
    var selected: Dictionary = observer._read("healing-rule-selected")
    observer._check(selected.stage == "CommitReady" and selected.target == "medic-a" and selected.mainSeed == initial.mainSeed,
        "The same view selects a queried spell and self target without effects")
    await observer._press(KEY_TAB)
    var other: Dictionary = observer._read("healing-full-hp-candidate")
    observer._check(other.mainSeed == initial.mainSeed and other.actors[1].hp == 40,
        "Other-target query and selection publish no resources or RNG")
    observer._check(other.failure == "target-not-injured" and other.target == "medic-a" if half
        else other.failure == null and other.target == "guard-a", "Confirm legality matches the query's full-HP distinction")
    await observer._press(KEY_TAB)
    await observer._press(KEY_ENTER)
    var prepared: Dictionary = observer._read("healing-rule-prepared")
    observer._check(prepared.scene.visible and prepared.scene.actionKind == "heal" and prepared.scene.spell.Level == 1,
        "Actual scene consumer accepts the selected strategy's supported presentation")
    observer._check(prepared.actors[0].hp == 30 and prepared.actors[0].mp == 8,
        "Construction publishes neither cost nor recovery")
    observer._check(prepared.scene.reactionAmount == (5 if half else 10), "Presenter receives the strategy's legal recovery operand")
    var completed := await _settle_heal("healing-rule-complete")
    var expected_hp := 35 if half else 40
    observer._check(completed.failure == null and completed.scene.error == null and not completed.scene.visible
        and completed.actor != initial.actor, "Actual completion releases the turn exactly once without adapter errors")
    observer._check(completed.actors[0].hp == expected_hp and completed.actors[0].mp == 5 and completed.actors[0].exp == (0 if half else 10),
        "Selected algorithm reaches the actual HP/MP/EXP result")
    observer._check(completed.roster.contains("HP " + str(expected_hp)) and completed.actors[0].text.contains(str(expected_hp)),
        "HUD and battlefield actor nodes project the committed recovery")
    observer._check(completed.thinkingSeed == initial.thinkingSeed, "Scene retains the independent thinking image")
    var award_draws := 0
    var paid := 0
    var recovered := 0
    var ended := 0
    var seed: int = initial.mainSeed
    for event in facts:
        if event.Kind in ["rng-exp-plus", "rng-exp-minus"]:
            award_draws += 1
        if event.RandomRange != null:
            observer._check(event.Before == seed, "Each real emitted draw continues the current seed")
            seed = ((((seed >> 16) * 13 + 7) & 65535) << 16) | (seed & 65535)
            observer._check(event.After == seed, "Each real emitted draw uses the fixed RNG recurrence")
        if event.Kind == "mp": paid += 1
        if event.Kind == "hp": recovered += 1
        if event.Kind == "after-turn" and event.Actor.Value == initial.actor: ended += 1
    observer._check(award_draws == (0 if half else 2) and paid == 1 and recovered == 1 and ended == 1,
        "Real staged publications have the selected award draws and single resource/turn effects")
    observer._check(completed.mainSeed == seed, "Final seed includes the real scene's later fairy work")
    observer.samples.append({"label": "healing-rule-semantic-events", "events": facts})
