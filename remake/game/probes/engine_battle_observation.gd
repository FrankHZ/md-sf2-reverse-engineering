extends SceneTree

# Direct native observation. It drives real input events and reads the running view; no images,
# session setters, synthetic receipts, unit-test harness, or runtime RNG/state reset.
var samples: Array = []
var failures: Array = []
var view: Node

# This external observer owns diagnostics; game user arguments belong only to GameRoot.
func _diagnostic(name: String, fallback: String = "") -> String:
    var value := OS.get_environment("SF2_OBSERVATION_" + name)
    return fallback if value.is_empty() else value

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
        failures.append("Common battle view did not start.")
        _finish()
        return
    var initial := _read("initial")
    _check(initial.viewport.width == 960 and initial.viewport.height == 540, "Representative initial viewport is 960x540")
    _check(host.name == "GameRoot", "The actual ordinary entry scene owns every common startup")
    if _diagnostic("CASE") == "startup":
        var expected := _diagnostic("EXPECT_FAILURE")
        _check(initial.failure == null if expected.is_empty() else initial.failure == expected,
            "Actual startup has the selected expected success or diagnostic")
        if expected.is_empty():
            _check(initial.stopReason == "PlayerInput" and initial.stage == "Movement", "Common session computes initial player control")
            var origin := _diagnostic("EXPECT_ORIGIN", "public-authored-controlled-start")
            _check(initial.origin == origin, "Explicit content selection reaches the expected common source")
        else:
            _check(initial.actors == null and initial.mainSeed == null, "Invalid options cannot publish a session or RNG state")
        _finish()
        return
    if initial.failure != null:
        failures.append("Startup: " + str(initial.failure))
        _finish()
        return
    var observation_case := _diagnostic("CASE")
    if not observation_case.is_empty():
        if observation_case == "private-source-ai":
            await _private_source_ai(initial)
            _check(samples.size() == 13, "Private source AI completed all thirteen continuous-input checkpoints")
            _finish()
            return
        if observation_case == "private-initialized":
            await _private_initialized(initial)
            _check(samples.size() == 7, "Private entry completed all seven real-input checkpoints")
            _finish()
            return
        if observation_case == "extra-turn":
            await _extra_turn(initial)
            _check(samples.size() == 7, "Extra-turn observation completed all seven checkpoints")
            _finish()
            return
        if observation_case == "commandset-continuation":
            var continuation_shape := _diagnostic("CONTINUATION_SHAPE", "move-attack")
            await _commandset_continuation(initial, continuation_shape)
            var checkpoint_count := 6 if continuation_shape == "move-attack" else 3 if continuation_shape == "startup" else 4
            _check(samples.size() == checkpoint_count, "Commandset observation completed every native checkpoint")
            _finish()
            return
        if observation_case == "target-selection":
            var target_shape := _diagnostic("TARGET_SHAPE", "secondary")
            await _target_selection(initial, target_shape)
            _check(samples.size() == (4 if target_shape == "missing-class" else 5), "Target observation completed every native checkpoint")
            _finish()
            return
        if observation_case == "enemy-actions":
            await _enemy_actions(initial, _diagnostic("ENEMY_SHAPE", "counter"))
            _check(samples.size() == 4, "Enemy observation completed all four native checkpoints")
            _finish()
            return
        if observation_case == "target-cycle":
            await _target_cycle(initial)
            _finish()
            return
        if observation_case == "layout":
            await _layout(initial, _diagnostic("LONG_PATH") == "1")
            _finish()
            return
        if observation_case == "physical":
            await _physical(initial, _diagnostic("REVERSE_KILLS") == "1")
            _finish()
            return
        if observation_case == "followups":
            var shape := _diagnostic("FOLLOWUP_SHAPE")
            if shape.is_empty():
                failures.append("Missing follow-up observation shape.")
            else:
                await _followups(initial, shape)
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
    _check(unsupported.failure == "physical-definition" and unsupported.failureKind == "UnsupportedCapability",
        "Unsupported capability stays distinct in the live view")
    _finish()

func _extra_turn(initial: Dictionary) -> void:
    var player: String = initial.actors[0].id
    var other: String = initial.actors[1].id
    _check(initial.round == 1 and initial.queueCursor == 0 and initial.actor == player
        and initial.mainSeed == 0xFF4D1234, "Initial extra-turn round consumes exactly eleven draws")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    var selected := _read("first-stay-selected")
    _check(selected.stage == "CommitReady" and selected.queueCursor == 0
        and selected.mainSeed == initial.mainSeed, "Selection does not consume the first entry")
    await _press(KEY_ENTER)
    var first := _read("first-entry-consumed")
    _check(first.failure == null and first.actor == player and first.round == 1 and first.queueCursor == 1
        and first.stage == "Movement" and first.stopReason == "PlayerInput"
        and first.mainSeed == initial.mainSeed, "Same actor receives its second entry in the same round")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var second := _read("second-entry-consumed")
    _check(second.failure == null and second.actor == other and second.round == 1 and second.queueCursor == 3
        and second.mainSeed == initial.mainSeed, "Second entry consumes once before ordinary AI and next ally")
    var saw_ai := false
    for observation in second.observations:
        saw_ai = saw_ai or observation.Kind == "ai-stay"
    _check(saw_ai, "Configured enemy runs between the extra actor and ordinary ally")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_round := _read("next-round")
    _check(next_round.failure == null and next_round.round == 2 and next_round.queueCursor == 0
        and next_round.actor == player and next_round.mainSeed == 0x887A1234,
        "Natural next round carries the seed after twenty-two draws")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_first := _read("next-round-first-consumed")
    _check(next_first.actor == player and next_first.round == 2 and next_first.queueCursor == 1,
        "Extra eligibility persists on natural round generation")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_second := _read("next-round-second-consumed")
    _check(next_second.failure == null and next_second.actor == other and next_second.round == 2
        and next_second.queueCursor == 3 and next_second.mainSeed == next_round.mainSeed,
        "Both second-round entries are consumed through actual input")
    for state in [first, second, next_round, next_first, next_second]:
        _check(state.thinkingSeed == initial.thinkingSeed and state.gold == initial.gold
            and state.actors[0].hp == initial.actors[0].hp and state.actors[0].mp == initial.actors[0].mp
            and state.actors[0].visible and state.actors[0].x == initial.actors[0].x
            and state.actors[0].y == initial.actors[0].y, "STAY preserves resources and actual actor node")

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

func _followups(initial: Dictionary, shape: String) -> void:
    var expected: Dictionary = {
        "sticky": {"hits": ["first", "second", "counter"], "hp": 493, "target_hp": 453, "exp": 2, "seed": 0x3A1E1234, "draws": 20},
        "counter": {"hits": ["first", "counter"], "hp": 491, "target_hp": 474, "exp": 3, "seed": 0x557E1234, "draws": 14},
        "ally-death": {"hits": ["first", "counter"], "hp": 0, "target_hp": 478, "exp": 99, "seed": 0x4CCA1234, "draws": 10},
        "second-death": {"hits": ["first", "second"], "hp": 500, "target_hp": 0, "exp": 24, "seed": 0xD1F61234, "draws": 12}
    }.get(shape, {})
    if expected.is_empty():
        failures.append("Unknown follow-up shape.")
        return
    await _press(KEY_ENTER)
    await _press(KEY_X)
    await _press(KEY_TAB)
    var selected := _read("followup-selected")
    _check(selected.failure == null and selected.target == initial.actors[2].id
        and selected.mainSeed == initial.mainSeed, "Follow-up selection leaves the battle untouched")
    await _press(KEY_ENTER)
    var result := _read("followup-resolved-next-control")
    _check(result.failure == null and result.actor == initial.actors[1].id, "One action returns the next living player")
    _check(result.actors[0].hp == expected.hp and result.actors[2].hp == expected.target_hp
        and result.actors[0].exp == expected.exp, "Actual follow-up HP and aggregate EXP")
    _check(result.mainSeed == expected.seed and result.thinkingSeed == initial.thinkingSeed, "Exact naturally carried action seeds")
    var hits: Array = []
    var draws := 0
    var commits := 0
    for observation in result.observations:
        if observation.Kind.begins_with("physical-"):
            hits.append(observation.Kind.trim_prefix("physical-"))
            var counter: bool = observation.Kind == "physical-counter"
            _check(observation.Actor.Value == (initial.actors[2].id if counter else initial.actor)
                and observation.Target.Value == (initial.actor if counter else initial.actors[2].id),
                "Semantic attack observation preserves actor/target reversal")
        if observation.RandomRange != null:
            draws += 1
        if observation.Kind == "action-committed":
            commits += 1
    _check(hits == expected.hits and draws == expected.draws and commits == 1, "Source-ordered bounded chain and one atomic action")
    var enemy_dead: bool = expected.target_hp == 0
    _check(result.gold == initial.gold + (19 if enemy_dead else 0), "One configured kill reward only")
    if shape == "ally-death":
        _check(result.actors[0].x == null and not result.actors[0].visible
            and result.actors[0].defeats == 7 and result.roster.contains("DEFEATS 7"), "Counter death clears actor node and projects defeat accounting")
    if enemy_dead:
        _check(result.actors[2].x == null and not result.actors[2].visible and result.actors[0].kills == 1,
            "Second-hit death cancels counter and clears the enemy exactly once")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var next_round := _read("followup-next-round")
    _check(next_round.round == 2 and next_round.actor == (initial.actors[1].id if expected.hp == 0 else initial.actor),
        "Next round excludes any dead actor and returns actual living control")

func _enemy_actions(initial: Dictionary, shape: String) -> void:
    var enemy: String = initial.actors[2].id
    var ally: String = initial.actors[0].id
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    var ready := _read("enemy-action-ready")
    _check(_same_battle(initial, ready), "Player selection does not execute automatic enemy work")
    await _press(KEY_ENTER)
    var result := _read("enemy-action-result")
    if shape == "unsupported":
        _check(result.failure == "level-up" and result.stopReason == "Unsupported", "Late enemy counter award stops as Unsupported")
        _check(result.mainSeed == initial.mainSeed and result.thinkingSeed == initial.thinkingSeed
            and result.gold == initial.gold and result.actors[0].hp == initial.actors[0].hp
            and result.actors[2].hp == initial.actors[2].hp and result.aiMemory[0].lastTarget == null,
            "Failed whole enemy action preserves HP, rewards, memory and both seeds")
        _check(result.queueCursor == initial.queueCursor + 1, "Only the preceding player action consumed its queue entry")
    else:
        var dead: bool = shape == "enemy-death"
        _check(result.failure == null and result.stopReason == "PlayerInput" and result.actor == initial.actors[1].id,
            "Real enemy action returns control to next living player")
        _check(result.actors[0].hp == 478 and result.actors[2].hp == (0 if dead else 493), "Enemy hit and reversed ally counter update actual HP")
        _check(result.actors[0].exp == (24 if dead else 1) and result.actors[2].exp == 0,
            "Counter EXP belongs only to the ally")
        _check(result.mainSeed == (0xB1BC1234 if dead else 0x557E1234) and result.thinkingSeed == 0x02EF0042,
            "Main and thinking streams carry through source draws")
        _check(result.aiMemory[0].lastTarget == ally, "Committed target memory follows actual selected ally")
        _check(result.gold == initial.gold + (19 if dead else 0), "Counter kill grants configured enemy gold")
        if dead:
            _check(result.actors[2].x == null and not result.actors[2].visible and result.actors[0].kills == 1,
                "Counter death removes enemy occupancy and visible node once")
        if shape == "movement":
            _check(result.actors[2].x == 4 and result.actors[2].y == 3, "Source ring chooses reachable attack destination")
        var hits: Array = []
        var commits := 0
        for observation in result.observations:
            if observation.Kind.begins_with("physical-"):
                hits.append(observation.Kind)
                var counter: bool = observation.Kind == "physical-counter"
                _check(observation.Actor.Value == (ally if counter else enemy)
                    and observation.Target.Value == (enemy if counter else ally), "Physical observations preserve enemy/counter roles")
            if observation.Kind == "action-committed":
                commits += 1
        _check(hits == ["physical-first", "physical-counter"] and commits == 2,
            "One player commit and one atomic enemy chain use the common publisher")
    await process_frame
    var stable := _read("enemy-action-stable")
    _check(stable.revision == result.revision and stable.mainSeed == result.mainSeed and stable.thinkingSeed == result.thinkingSeed,
        "Input or Unsupported boundary remains stable across actual frames")

func _target_selection(initial: Dictionary, shape: String) -> void:
    var primary: String = initial.actors[0].id
    var secondary: String = initial.actors[1].id
    var selected: String = secondary if shape == "secondary" else primary
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    var ready := _read("targets-ready")
    _check(_same_battle(initial, ready), "Selection does not run target scoring")
    await _press(KEY_ENTER)
    var result := _read("targets-first-action")
    if shape == "missing-class":
        _check(result.failure == "ai-target-class" and result.stopReason == "Unsupported", "Reached class cohort requires actual class definition")
        _check(result.mainSeed == initial.mainSeed and result.thinkingSeed == initial.thinkingSeed
            and result.actors[0].hp == 500 and result.actors[1].hp == 500 and result.actors[2].hp == 500
            and result.aiMemory[0].lastTarget == null and result.queueCursor == initial.queueCursor + 1,
            "Rejected target selection preserves the complete enemy action and prior player commit")
        await process_frame
        var stable := _read("targets-unsupported-stable")
        _check(stable.revision == result.revision, "Unsupported remains stopped")
        return
    _check(result.failure == null and result.actor == secondary, "Competing targets resolve then return real player control")
    var calls: Array = []
    var scores: Array = []
    for observation in result.observations:
        if observation.Kind == "thinking-rng":
            calls.append(observation.Target.Value)
        if observation.Kind == "ai-candidate":
            scores.append([observation.Before, observation.After])
        if observation.Kind == "ai-target":
            _check(observation.Target.Value == selected and observation.Before == (1 if shape == "movement" else 19)
                and observation.After == (1 if shape == "movement" else 15), "Raw maximum and returned cap preserve chosen source cohort")
    _check(calls == [secondary, primary], "Thinking calls follow reverse reachable slot order")
    _check(scores == ([[12.0, 1.0], [14.0, 1.0]] if shape == "movement" else [[0.0, 19.0], [0.0, 19.0]]),
        "Candidate costs and priorities follow actual configuration")
    _check(result.aiMemory[0].lastTarget == selected and result.mainSeed == 0x557E1234
        and result.thinkingSeed == 0x02EF0042, "Chosen target and both RNG channels publish with common action")
    var index := 1 if shape == "secondary" else 0
    _check(result.actors[index].hp == 478 and result.actors[index].exp == 1 and result.actors[2].hp == 493,
        "Chosen ally receives damage, counters and earns the shared reward")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var round_two := _read("targets-round-two")
    _check(round_two.round == 2 and round_two.actor == primary and round_two.mainSeed == 0x97231234,
        "Actual commands carry the first action's RNG into the next generated round")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var second := _read("targets-second-action")
    _check(second.failure == null and second.actor == secondary and second.aiMemory[0].lastTarget == primary,
        "Continued thinking selects primary from live competing candidates")
    _check(second.mainSeed == 0xE0E11234 and second.thinkingSeed == 0x01EF0042
        and second.actors[0].hp == result.actors[0].hp - 26 and second.actors[1].hp == result.actors[1].hp,
        "Second action consumes natural draws and changes only the actual target")

func _commandset_continuation(initial: Dictionary, shape: String) -> void:
    var enemy: String = initial.actors[2].id
    if shape == "startup":
        _check(initial.actors[2].x == 5 and initial.actor == initial.actors[0].id
            and initial.queueCursor == 1 and initial.mainSeed == 0xDC7F1234 and initial.thinkingSeed == 0xBEEF0042,
            "Enemy-first startup completes MOVE1 and yields actual player control")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    if shape != "startup":
        var ready := _read("continuation-ready")
        _check(_same_battle(initial, ready), "Player selection leaves AI continuation provisional")
    await _press(KEY_ENTER)
    var result := _read("continuation-result")
    if shape == "unreachable":
        _check(result.failure == "ai-move-target-domain" and result.stopReason == "Unsupported"
            and result.queueCursor == initial.queueCursor + 1 and result.actors[2].x == 7,
            "Incomplete target costs stop the whole enemy action after the player commit")
    else:
        _check(result.failure == null and result.actor == initial.actors[1].id, "Continuation yields next living player control")
        var expected_x := 7 if shape == "occupied" else 6 if shape == "weighted" else 5
        _check(result.actors[2].x == expected_x and result.actors[2].y == initial.actors[2].y,
            "Source preliminary movement and legal stopping correction reach the configured destination")
        if shape != "startup":
            var commands: Array = []
            var commits := 0
            for observation in result.observations:
                if observation.Kind.begins_with("ai-command-"):
                    commands.append([observation.Kind, observation.After])
                if observation.Kind == "action-committed" and observation.Actor.Value == enemy:
                    commits += 1
                if observation.Kind == "ai-move-target":
                    _check(observation.Target.Value == initial.actors[1 if shape == "secondary" else 0].id
                        and observation.Before == (6 if shape == "weighted" else 12), "MOVE1 selects the current unsigned-cost target")
                _check(not observation.Kind.begins_with("rng-") and observation.Kind != "thinking-rng"
                    and observation.Kind != "physical-first", "MOVE1 and unavailable commands draw no RNG or same-turn attack")
            _check(commands == [["ai-command-attack1", -1.0], ["ai-command-heal1", -1.0],
                ["ai-command-support", -1.0], ["ai-command-move1", 0.0]] and commits == 1,
                "Three failed commands followed by successful MOVE1 publish one enemy action, including origin Stay")
    _check(result.mainSeed == initial.mainSeed and result.thinkingSeed == initial.thinkingSeed
        and result.gold == initial.gold and result.aiMemory[0].lastTarget == null,
        "Continuation preserves both random streams, rewards and last-target memory")
    for index in range(initial.actors.size()):
        for field in ["hp", "mp", "exp", "kills", "defeats"]:
            _check(result.actors[index][field] == initial.actors[index][field], "Movement preserves actual actor resources")
    if shape == "move-attack":
        await _press(KEY_ENTER)
        await _press(KEY_SPACE)
        await _press(KEY_ENTER)
        var next_round := _read("continuation-round-two")
        _check(next_round.round == 2 and next_round.actor == initial.actor and next_round.mainSeed == 0xEE281234,
            "Actual input naturally generates the next round from unchanged movement history")
        await _press(KEY_ENTER)
        await _press(KEY_SPACE)
        await _press(KEY_ENTER)
        result = _read("continuation-next-turn-attack")
        _check(result.failure == null and result.actor == initial.actors[1].id and result.actors[2].x == 2
            and result.actors[0].hp == 478 and result.actors[2].hp == 493 and result.actors[0].exp == 1,
            "Next actual turn reaches ATTACK1, common physical action and ally counter")
        _check(result.mainSeed == 0xDAA61234 and result.thinkingSeed == 0x02EF0042
            and result.aiMemory[0].lastTarget == initial.actor and result.roster.contains("HP 478"),
            "Exact continued RNG and target memory agree with live HUD")
    await process_frame
    var stable := _read("continuation-stable")
    _check(stable.revision == result.revision and stable.mainSeed == result.mainSeed and stable.thinkingSeed == result.thinkingSeed,
        "Player or Unsupported boundary remains stable across native frames")

func _finish() -> void:
    var report := {"samples": samples, "failures": failures, "passed": failures.is_empty()}
    var output_path := _diagnostic("OUTPUT")
    if not output_path.is_empty():
        var output := FileAccess.open(output_path, FileAccess.WRITE)
        if output != null:
            output.store_string(JSON.stringify(report, "\t"))
            output.close()
        else:
            failures.append("Could not write observation output.")
    print("SF2_ENGINE_OBSERVATION " + JSON.stringify({"passed": failures.is_empty(), "samples": samples.size(), "failures": failures}))
    quit(0 if failures.is_empty() else 1)

func _private_initialized(initial: Dictionary) -> void:
    _check(initial.origin == "private-local-controlled-start" and initial.title.begins_with("PRIVATE CONTROLLED BATTLE"), "Common host discloses private controlled origin")
    _check(initial.actor == "ally-1" and initial.round == 1 and initial.queueCursor == 0, "Computed first candidate enters player movement")
    _check(initial.mainSeed == 0xA4991234 and initial.thinkingSeed == 0x12340000, "Initialized entry carries both independent RNG images")
    _check(initial.regionsTested == 7 and not initial.regionFlags.has(true), "Original initial placements test three inactive regions")
    _check(initial.turnOrder.size() == 64 and initial.turnOrder[0].actor == "ally-1" and initial.turnOrder[1].actor == "ally-2", "Actual generated queue is observed")
    _check(initial.gold == null and initial.roster.contains("EXP Unknown"), "Unknown accounting remains explicit in state and HUD")
    for actor in initial.actors:
        _check(actor.exp == null and actor.kills == null and actor.defeats == null, "No unknown actor accounting is invented")
        _check(actor.visible and actor.nodeX == actor.x * 40 + 2 and actor.nodeY == actor.y * 40 + 4, "Actual actor marker follows initialized position")
        if actor.id.begins_with("enemy-"):
            _check(actor.sourceAttack == 7 and actor.attack == 8 and actor.mover == "Hovering", "Source enemy baseline and one effective adjustment remain distinct")
        else:
            _check(actor.attack == actor.sourceAttack, "Equipped allies are not refreshed twice")
    await _press(KEY_W)
    var moved := _read("private-movement-preview")
    _check(moved.previewX == 9 and moved.previewY == 17 and moved.actors[1].y == 18 and moved.mainSeed == initial.mainSeed, "Common input moves only the preview")
    await _press(KEY_ENTER)
    await _press(KEY_ESCAPE)
    var cancelled := _read("private-cancelled")
    _check(cancelled.previewY == 18 and cancelled.stage == "Movement" and cancelled.mainSeed == initial.mainSeed, "Cancel restores committed initialized position")
    await _press(KEY_ENTER)
    await _press(KEY_H)
    await _press(KEY_ENTER)
    var unknown := _read("private-accounting-unsupported")
    _check(unknown.failure == "unspecified-exp" and unknown.mainSeed == initial.mainSeed and unknown.actors[1].mp == 10, "Reached HEAL cannot invent EXP or partially consume MP/RNG")
    await _press(KEY_ESCAPE)
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var centaur := _read("private-next-centaur")
    _check(centaur.actor == "ally-2" and centaur.queueCursor == 1 and centaur.actors[2].mover == "Centaur", "Next generated player uses the required Centaur mover")
    await _press(KEY_W)
    await _press(KEY_ESCAPE)
    var centaur_cancelled := _read("private-centaur-cancelled")
    _check(centaur_cancelled.previewX == 7 and centaur_cancelled.previewY == 18 and centaur_cancelled.mainSeed == initial.mainSeed, "Centaur uses the same movement/cancel commands")
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)
    var enemy := _read("private-standby-relay")
    _check(enemy.failure == null and enemy.stopReason == "PlayerInput" and enemy.actor == "ally-0" and enemy.queueCursor == 8, "Six actual inactive enemies complete standby before Bowie control")
    _check(enemy.mainSeed == initial.mainSeed and enemy.thinkingSeed == 0x01340000, "Standby advances only the independent thinking channel")
    var positions := [[6, 3], [10, 4], [6, 5], [8, 4], [9, 6], [6, 6]]
    var memories := [0x14, 0x34, 0x24, 0x24, 0x24, 0x24]
    for index in range(6):
        var actor: Dictionary = enemy.actors[index + 3]
        _check(actor.x == positions[index][0] and actor.y == positions[index][1] and actor.aiMemory == memories[index], "Standby uses evolving occupancy and each enemy's own memory")
        _check(actor.anchorX == initial.actors[index + 3].x and actor.anchorY == initial.actors[index + 3].y and actor.primaryOrder == 255 and actor.secondaryOrder == 255, "Immutable source anchor and decoded NONE orders survive relocation")
    for observation in enemy.observations:
        _check(observation.Kind != "ai-stay", "Source orders are never replaced by authored Stay")

func _private_stay() -> void:
    await _press(KEY_ENTER)
    await _press(KEY_SPACE)
    await _press(KEY_ENTER)

func _private_source_ai(initial: Dictionary) -> void:
    await _private_initialized(initial)
    await _press(KEY_W)
    await _private_stay()
    var round_two := _read("private-first-round-complete")
    _check(round_two.round == 2 and round_two.actors[0].x == 8 and round_two.actors[0].y == 17, "Bowie's committed move completes the real first round")
    var current: Dictionary = round_two
    for _turn in range(6):
        if current.round >= 3 or current.stopReason != "PlayerInput":
            break
        if current.actor == "ally-0":
            for _step in range(3):
                await _press(KEY_D)
            for _step in range(2):
                await _press(KEY_W)
        await _private_stay()
        current = JSON.parse_string(view.call("ReadObservationJson"))
    var activated := _read("private-region-activated")
    _check(activated.round == 3 and activated.actor == "ally-2" and activated.mainSeed == 0x9BD71234, "Natural session commands generate the next round's actual order")
    _check(activated.actors[0].x == 11 and activated.actors[0].y == 15 and activated.regionFlags[1], "Player movement activates the reached source region")
    var words := [0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070]
    for index in range(6):
        _check(activated.actors[index + 3].activationWord == words[index], "Only source-linked enemies gain the activation bit")
    await _private_stay()
    var first_active := _read("private-active-prefix-to-player")
    _check(first_active.actor == "ally-0" and first_active.failure == null and first_active.regionsTested == 0 and first_active.regionFlags[1], "Inactive prefix and first active pursuit return to actual Bowie control")
    await _private_stay()
    await _private_stay()
    var set_seven := _read("private-set-seven-to-next-player")
    _check(set_seven.round == 4 and set_seven.actor == "ally-1" and set_seven.mainSeed == 0x51DC1234 and set_seven.thinkingSeed == 0x02340000, "Source set7 completes and generated order returns to the next player with both RNG channels")
    _check(set_seven.actors[7].x == 11 and set_seven.actors[7].y == 6 and set_seven.actors[7].activationWord == 0x2071, "Actual set7 enemy pursues from its own evolving position")
    var saw_move_order := false
    for observation in set_seven.observations:
        if observation.Kind == "ai-command-move-order1" and observation.Actor.Value == "enemy-4" and observation.After == -1:
            saw_move_order = true
    _check(saw_move_order, "Set7 retains its failed MOVE_ORDER1 before the common pursuit")
    current = set_seven
    for _turn in range(12):
        if current.stopReason != "PlayerInput":
            break
        await _private_stay()
        current = JSON.parse_string(view.call("ReadObservationJson"))
    var stopped := _read("private-reached-action-boundary")
    _check(stopped.failure == "source-attack-operands" and stopped.stopReason == "Unsupported" and stopped.round == 6 and stopped.queueCursor == 5, "Reached physical action stops at the explicit step3 capability boundary")
    _check(stopped.mainSeed == 0x07821234 and stopped.thinkingSeed == 0x00340000 and stopped.actors[7].x == 11 and stopped.actors[7].y == 10, "Unsupported action retains the last successful state without a partial pursuit or reseed")
    for index in range(initial.actors.size()):
        var actor: Dictionary = stopped.actors[index]
        var before: Dictionary = initial.actors[index]
        _check(actor.hp == before.hp and actor.mp == before.mp and actor.attack == before.attack and actor.items == before.items and actor.spells == before.spells, "Continuous no-action flow preserves resources and the actual source loadout")
        _check(actor.anchorX == before.anchorX and actor.anchorY == before.anchorY and actor.exp == null and actor.kills == null and actor.defeats == null and actor.lastTarget == null, "Source anchor, Unknown accounting and last target remain intact")
    await process_frame
    var stable := _read("private-action-boundary-stable")
    _check(stable.revision == stopped.revision and stable.mainSeed == stopped.mainSeed and stable.thinkingSeed == stopped.thinkingSeed, "Native frames do not continue an Unsupported action")
