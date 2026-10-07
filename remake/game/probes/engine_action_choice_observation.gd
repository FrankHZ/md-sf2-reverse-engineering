extends RefCounted

# Bounded semantic choices consumer: actual ordinary input, session query and HUD readback.
var observer: SceneTree
var facts: Array = []

func _start(owner: SceneTree) -> void:
    observer = owner
    observer.view.connect("SessionResultObserved", Callable(self, "_result"))

func _result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if event.Kind not in ["scene-logical-step", "scene-delivery"]:
            facts.append(event)

func _state() -> Dictionary:
    return JSON.parse_string(observer.view.call("ReadObservationJson"))

# Real presenter completion and real player acknowledgement; never call completion APIs.
func _settle(label: String) -> Dictionary:
    var deadline := Time.get_ticks_msec() + 40000
    var phase := ""
    while Time.get_ticks_msec() < deadline:
        var current := _state()
        if current.failure != null:
            observer._check(false, "Actual consumer failed: " + str(current.failure))
            break
        if not current.scene.visible and current.stopReason == "PlayerInput":
            var result: Dictionary = observer._read(label)
            observer.samples.append({"label": label + ":events", "events": facts.duplicate(true)})
            return result
        if current.scene.visible:
            if phase != current.scene.phase:
                phase = current.scene.phase
                observer._read(label + ":" + phase)
            var healing = current.scene.healing
            if phase in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"] \
                and (healing == null or healing.AtTimedInput or healing.LogicalComplete):
                await observer._press(KEY_ENTER)
            else:
                await observer.process_frame
        else:
            await observer.process_frame
    observer._check(false, "Actual consumer reached its bounded completion deadline")
    return observer._read(label + ":incomplete")

func _draws(construction: bool) -> Array:
    var draws: Array = []
    for event in facts:
        if event.RandomRange != null and event.Kind.begins_with("rng-") \
            and event.Kind.begins_with("rng-reaction-") != construction:
            draws.append(event)
    return draws

func _after(seed: int, count: int) -> int:
    for _index in range(count):
        seed = ((((seed >> 16) * 13 + 7) & 65535) << 16) | (seed & 65535)
    return seed

func _rng(before: int, construction_seed: int, scene_seed: int, construction_count: int, reaction_count: int) -> void:
    var construction := _draws(true)
    var reaction := _draws(false)
    observer._check(construction.size() == construction_count and reaction.size() == reaction_count,
        "Construction and damaging-reaction draws retain their separate source schedules")
    if construction.is_empty():
        return
    observer._check(construction[-1].After == construction_seed and _after(construction_seed, reaction_count) == scene_seed,
        "Source construction seed and independently derived scene seed remain distinct")
    var seed := before
    for event in construction + reaction:
        observer._check(event.Before == seed and event.After == _after(seed, 1), "Actual main RNG facts form the ordered carried chain")
        var doubled: int = (int(event.RandomRange) * 2) & 65535
        var value: int = (((int(event.After) >> 16) * doubled) >> 16) >> 1
        observer._check(event.RandomValue == value, "Actual main RNG range/value matches the accepted word generator")
        seed = int(event.After)
    observer._check(seed == scene_seed, "Published final seed matches all actual action and scene draws")


func targets(owner: SceneTree, initial: Dictionary) -> void:
    _start(owner)
    var targets: Array = initial.battleChoices.Spells[0].Targets
    observer._check(targets.size() == 3 and targets[0].Enabled and not targets[1].Enabled and targets[2].Enabled,
        "Semantic query preserves the rejected middle candidate and later legal ally")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_H)
    var selected: Dictionary = observer._read("self-selected")
    await observer._press(KEY_TAB)
    var rejected: Dictionary = observer._read("first-tab-range-rejected")
    observer._check(rejected.failure == "target-range" and rejected.target == initial.actor
        and rejected.candidate == "guard-a" and rejected.revision == selected.revision
        and observer._same_battle(selected, rejected), "Rejected candidate preserves selected target, revision and battle/RNG")
    await observer._press(KEY_TAB)
    var later: Dictionary = observer._read("second-tab-later-legal-target")
    observer._check(later.failure == null and later.target == "guard-c" and later.stage == "CommitReady"
        and observer._same_battle(selected, later), "Tab reaches the legal ally after a rejected candidate")
    await observer._press(KEY_TAB)
    var wrapped: Dictionary = observer._read("third-tab-wraps-to-self")
    observer._check(wrapped.failure == null and wrapped.target == initial.actor, "Target cycle wraps normally")
    await observer._press(KEY_ESCAPE)
    var cancelled: Dictionary = observer._read("cancel-clears-candidate")
    observer._check(cancelled.candidate == null and cancelled.target == null and observer._same_battle(initial, cancelled),
        "Cancel clears only provisional selection and UI cursor")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_H)
    await observer._press(KEY_TAB)
    await observer._press(KEY_TAB)
    await observer._press(KEY_ENTER)
    var committed: Dictionary = await _settle("later-target-heal-committed")
    observer._check(committed.failure == null and committed.actors[0].hp == initial.actors[0].hp
        and committed.actors[0].mp == initial.actors[0].mp - 3, "Real input commits HEAL to the later ally, not self")
    var healed_later := false
    for observation in facts:
        if observation.Kind == "hp" and observation.Actor.Value == "guard-c":
            healed_later = observation.After > observation.Before
    observer._check(healed_later and committed.candidate == null, "Later ally receives healing and the next action has no stale UI cursor")

func items(owner: SceneTree, initial: Dictionary) -> void:
    _start(owner)
    var choices: Array = initial.battleChoices.Items
    observer._check(choices.size() == 4 and choices[0].Label == "Recovery leaf" and choices[0].Enabled
        and choices[1].Label == "Small remedy" and choices[2].Empty and choices[3].Empty,
        "Semantic inventory exposes real slots, labels and empty states")
    observer._check(choices[0].Targets[0].Enabled and choices[0].Targets[1].Reason.Code == "target-range"
        and choices[1].Targets[1].Enabled, "Semantic item query carries per-item target legality")
    # Recipe selects J/Start as the item bindings. Every transition goes through ordinary host input.
    await observer._press(KEY_ENTER)
    var choosing: Dictionary = observer._read("item-action-choice")
    await observer._press(KEY_I)
    var unmapped: Dictionary = observer._read("old-binding-inert")
    observer._check(unmapped.revision == choosing.revision, "Overridden item key does not trigger a command")
    await observer._press(KEY_J)
    var selected: Dictionary = observer._read("held-item-selected")
    observer._check(selected.itemSlot == 0 and selected.target == initial.actor and selected.stage == "CommitReady", "Remapped item selects the held slot and self")
    observer._check(selected.itemChoices.contains("Recovery leaf · selected") and selected.help.contains("J / Pad Start"), "Actual HUD shows item identity, selected slot and remapped controls")
    await observer._press(KEY_TAB)
    var rejected: Dictionary = observer._read("item-outside-range")
    observer._check(rejected.failure == "target-range" and rejected.target == initial.actor and rejected.mainSeed == initial.mainSeed, "Distant target rejection retains accepted target and RNG")
    observer._check(rejected.inventories == initial.inventories, "Rejected target consumes no inventory")
    await observer._press(KEY_J)
    var reselected: Dictionary = observer._read("different-item-selected")
    observer._check(reselected.itemSlot == 1 and reselected.target == initial.actor, "Cycling selects the next real inventory slot")
    await observer._press(KEY_ESCAPE)
    var cancelled: Dictionary = observer._read("item-cancelled")
    observer._check(cancelled.itemSlot == null and cancelled.stage == "Movement" and cancelled.inventories == initial.inventories and cancelled.mainSeed == initial.mainSeed, "Cancel discards choices without resource changes")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_J)
    await observer._press(KEY_ENTER)
    var committed: Dictionary = await _settle("item-consumed")
    observer._check(committed.failure == null and committed.actors[0].hp == 70 and committed.actors[0].mp == initial.actors[0].mp, "Herb restores ten missing HP without MP payment")
    observer._check(committed.inventories[0].items == [6.0, 127.0, 127.0, 127.0] and committed.actor == "guard-a", "Consumption shifts remaining slot and advances to another holder")
    var remaining: Array = committed.battleChoices.Items
    observer._check(remaining[0].Label == "Recovery leaf" and remaining[0].Enabled,
        "Next actor receives its own current semantic inventory")
    observer._check(committed.thinkingSeed == initial.thinkingSeed and committed.mainSeed != initial.mainSeed, "Only the action's main RNG channel advances")
    await observer._press(KEY_ENTER)
    var event := InputEventJoypadButton.new()
    event.button_index = JOY_BUTTON_START
    event.pressed = true
    Input.parse_input_event(event)
    await observer.process_frame
    await observer.process_frame
    event = InputEventJoypadButton.new()
    event.button_index = JOY_BUTTON_START
    event.pressed = false
    Input.parse_input_event(event)
    await observer.process_frame
    var pad: Dictionary = observer._read("gamepad-item-selection")
    observer._check(pad.itemSlot == 0 and pad.target == "guard-a" and pad.failure == null, "Remapped gamepad selects another holder's own item")
    await observer._press(KEY_ENTER)
    var other: Dictionary = await _settle("other-holder-consumed")
    observer._check(other.failure == null and other.actors[1].hp == 12 and other.actors[1].exp == 1 and other.inventories[1].items == [127.0, 127.0, 127.0, 127.0], "Non-healer consumes once, clamps HP and earns minimum EXP")
    var current := other
    for _turn in range(6):
        if current.actor == "guard-a":
            break
        await observer._press(KEY_ENTER)
        await observer._press(KEY_SPACE)
        await observer._press(KEY_ENTER)
        current = JSON.parse_string(observer.view.call("ReadObservationJson"))
    observer._check(current.actor == "guard-a", "Ordinary turns return the empty holder")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_J)
    var empty: Dictionary = observer._read("empty-inventory")
    observer._check(empty.failure == "empty-item-slot" and empty.mainSeed == current.mainSeed and empty.inventories == current.inventories, "Empty inventory rejection is visible and atomic")
func physical(owner: SceneTree, initial: Dictionary, reverse: bool) -> void:
    _start(owner)
    var physical_options: Array = initial.battleChoices.Actions.filter(func(option): return option.Action.Kind == 2)
    observer._check(physical_options.size() == 1 and physical_options[0].Targets.size() == 3
        and physical_options[0].Targets[0].Enabled and physical_options[0].Targets[1].Enabled
        and physical_options[0].Targets[2].Reason.Code == "target-range",
        "Actual physical query supplies ordered legal opponents and the rejected distant candidate")
    var player: String = initial.actor
    var first_index := 3 if reverse else 2
    var second_index := 2 if reverse else 3
    await observer._press(KEY_ENTER)
    await observer._press(KEY_F)
    await observer._press(KEY_TAB)
    await observer._press(KEY_TAB)
    await observer._press(KEY_TAB)
    var rejected: Dictionary = observer._read("physical-range-rejected")
    observer._check(rejected.failure == "target-range", "Distant opponent fails range validation")
    observer._check(rejected.target == initial.actors[3].id and rejected.candidate == initial.actors[4].id,
        "Rejected physical cursor retains authoritative target")
    await observer._press(KEY_TAB)
    if reverse:
        await observer._press(KEY_TAB)
    var selected: Dictionary = observer._read("first-physical-selected")
    observer._check(selected.failure == null and selected.target == initial.actors[first_index].id,
        "Physical candidate cycles past rejection")
    observer._check(selected.mainSeed == initial.mainSeed and selected.actors[first_index].hp == 15,
        "Target selection leaves resources and RNG unchanged")
    facts.clear()
    await observer._press(KEY_ENTER)
    observer._check(_state().mainSeed == 0x0A7F1234 and _state().actors[first_index].hp == 15,
        "Construction carries the retained source seed before persistent damage")
    var killed: Dictionary = await _settle("first-death-next-control")
    _rng(initial.mainSeed, 0x0A7F1234, 0x41571234, 6, 24)
    var first_exp := 23 if initial.map == "stone-court-map" else 48
    observer._check(killed.failure == null and killed.mainSeed == 0x41571234 and killed.actors[0].exp == first_exp,
        "Physical attack carries exact RNG and EXP")
    observer._check(killed.actors[first_index].hp == 0 and killed.actors[first_index].x == null
        and not killed.actors[first_index].visible, "Dead enemy leaves occupancy and visible battlefield")
    observer._check(killed.gold == initial.gold + 17 + first_index and killed.actors[0].kills == 1,
        "First death credits configured gold and killer")
    observer._check(killed.actor == initial.actors[1].id and killed.stopReason == "PlayerInput", "Automatic next living player")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_SPACE)
    await observer._press(KEY_ENTER)
    var next_round: Dictionary = observer._read("physical-next-round")
    observer._check(next_round.actor == player and next_round.round == 2 and next_round.mainSeed == 0x01231234,
        "Next round consumes only living turn entries")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_F)
    await observer._press(KEY_TAB)
    observer._check(observer._read("second-physical-selected").target == initial.actors[second_index].id,
        "Second legal kill uses the opposite opponent")
    facts.clear()
    await observer._press(KEY_ENTER)
    observer._check(_state().mainSeed == 0x7AE11234 and _state().actors[second_index].hp == 15,
        "Second construction uses the post-scene/post-round seed before damage")
    var second: Dictionary = await _settle("second-death-next-control")
    _rng(next_round.mainSeed, 0x7AE11234, 0xCE791234, 6, 24)
    var final_exp := 47 if initial.map == "stone-court-map" else 97
    observer._check(second.failure == null and second.actors[0].exp == final_exp and second.mainSeed == 0xCE791234,
        "Second physical action preserves expected EXP and RNG")
    observer._check(second.gold == initial.gold + 39 and second.actors[0].kills == 2,
        "Both kill orders settle the same gold and kill count")
    observer._check(second.actor == initial.actors[1].id and second.actors[second_index].x == null
        and not second.actors[second_index].visible, "Second death cleans up and returns actual control")
    observer._check(second.thinkingSeed == initial.thinkingSeed, "Stay AI does not consume thinking RNG")

func followups(owner: SceneTree, initial: Dictionary, shape: String) -> void:
    _start(owner)
    var expected: Dictionary = {
        "sticky": {"hits": ["first", "second", "counter"], "hp": 493, "target_hp": 453, "exp": 2, "seed": 0x3A1E1234, "draws": 20},
        "counter": {"hits": ["first", "counter"], "hp": 491, "target_hp": 474, "exp": 3, "seed": 0x557E1234, "draws": 14},
        "ally-death": {"hits": ["first", "counter"], "hp": 0, "target_hp": 478, "exp": 99, "seed": 0x4CCA1234, "draws": 10},
        "second-death": {"hits": ["first", "second"], "hp": 500, "target_hp": 0, "exp": 24, "seed": 0xD1F61234, "draws": 12}
    }.get(shape, {})
    if expected.is_empty():
        observer.failures.append("Unknown follow-up shape.")
        return
    await observer._press(KEY_ENTER)
    await observer._press(KEY_F)
    await observer._press(KEY_TAB)
    var selected: Dictionary = observer._read("followup-selected")
    observer._check(selected.failure == null and selected.target == initial.actors[2].id
        and selected.mainSeed == initial.mainSeed, "Follow-up selection leaves the battle untouched")
    facts.clear()
    await observer._press(KEY_ENTER)
    observer._check(_state().mainSeed == expected.seed and _state().actors[0].hp == initial.actors[0].hp
        and _state().actors[2].hp == initial.actors[2].hp, "Follow-up construction preserves the retained seed and saved HP")
    var result: Dictionary = await _settle("followup-resolved-next-control")
    var scene_seed: int = _after(expected.seed, expected.hits.size() * 24)
    _rng(initial.mainSeed, expected.seed, scene_seed, expected.draws, expected.hits.size() * 24)
    observer._check(result.failure == null and result.actor == initial.actors[1].id, "One action returns the next living player")
    observer._check(result.actors[0].hp == expected.hp and result.actors[2].hp == expected.target_hp
        and result.actors[0].exp == expected.exp, "Actual follow-up HP and aggregate EXP")
    observer._check(result.mainSeed == scene_seed and result.thinkingSeed == initial.thinkingSeed, "Exact naturally carried action seeds")
    var hits: Array = []
    var draws := 0
    var commits := 0
    for observation in facts:
        if observation.Kind.begins_with("physical-"):
            hits.append(observation.Kind.trim_prefix("physical-"))
            var counter: bool = observation.Kind == "physical-counter"
            observer._check(observation.Actor.Value == (initial.actors[2].id if counter else initial.actor)
                and observation.Target.Value == (initial.actor if counter else initial.actors[2].id),
                "Semantic attack observation preserves actor/target reversal")
        if observation.RandomRange != null and not observation.Kind.begins_with("rng-reaction-"):
            draws += 1
        if observation.Kind == "action-committed":
            commits += 1
    observer._check(hits == expected.hits and draws == expected.draws and commits == 1, "Source-ordered bounded chain and one atomic action")
    var enemy_dead: bool = expected.target_hp == 0
    observer._check(result.gold == initial.gold + (19 if enemy_dead else 0), "One configured kill reward only")
    if shape == "ally-death":
        observer._check(result.actors[0].x == null and not result.actors[0].visible
            and result.actors[0].defeats == 7 and result.roster.contains("DEFEATS 7"), "Counter death clears actor node and projects defeat accounting")
    if enemy_dead:
        observer._check(result.actors[2].x == null and not result.actors[2].visible and result.actors[0].kills == 1,
            "Second-hit death cancels counter and clears the enemy exactly once")
    await observer._press(KEY_ENTER)
    await observer._press(KEY_SPACE)
    await observer._press(KEY_ENTER)
    var next_round: Dictionary = observer._read("followup-next-round")
    observer._check(next_round.round == 2 and next_round.actor == (initial.actors[1].id if expected.hp == 0 else initial.actor),
        "Next round excludes any dead actor and returns actual living control")
