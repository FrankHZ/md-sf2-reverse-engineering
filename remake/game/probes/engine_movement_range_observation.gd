extends "res://probes/engine_world_viewport_observation.gd"

# Finite state/input/draw-operand witness; all changes after startup use mapped keys.
var previous_map: Node

func sample(label: String) -> Dictionary:
    var current := state()
    var facts := {}
    for key in ["sessionId","revision","observationSequence","stopReason","stage","actor","map","previewX","previewY","actors","movement","wait","canWaitAtInput","failure","viewport","story","scene"]:
        if current.has(key): facts[key] = current[key]
    var row := {"label":label,"state":facts}
    var board := view.get_node_or_null("MapViewport")
    if board != null and current.has("stage"):
        row.terrain = JSON.parse_string(board.call("ReadTerrainJson"))
    samples.append(row)
    return current

func range_read(label: String) -> Dictionary:
    sample(label)
    return samples[-1].terrain.movementRange

func hidden(label: String) -> bool:
    var r := range_read(label)
    return check(not r.visible and r.destinations == null, label + ": no movement range")

func stable(r: Dictionary, expected: Dictionary, label: String) -> bool:
    return check(r.visible and r.actor == expected.actor and r.origin == expected.origin and r.destinations == expected.destinations,
        label + ": range remains at committed origin")

func visible_geometry(label: String) -> bool:
    var s := sample(label)
    var t: Dictionary = samples[-1].terrain
    var r: Dictionary = t.movementRange
    if not check(s.viewport.width == 1920 and s.viewport.height == 1080 and t.cellSize == 96 and t.scale == 1 and t.clip,
        "Existing fixed 1080p/96px clipped board"): return false
    if not check(r.visible and r.z == 0 and r.draws > 0 and r.destinations == r.drawnDestinations,
        "Actual range draw uses exact eligible cells"): return false
    if not check(r.globalOrigin.x == t.board.x and r.globalOrigin.y == t.board.y and r.cellSize == 96,
        "Range inherits board camera transform"): return false
    if not check(r.fillAlpha > 0 and r.fillAlpha < 0.3 and r.edgeAlpha > r.fillAlpha,
        "Quiet translucent fill with legible edge"): return false
    if not check(t.previewZ.size() == 1 and t.previewZ[0] == 3 and t.actors.all(func(a): return a.z == 1), "Actor/path ordering"): return false
    var names: Array = t.children.map(func(c): return c.name)
    var index: int = names.find("MovementRange")
    if not check(index > 0, "Range is attached after terrain"): return false
    for layer in t.layers:
        if not check(names.find(layer.name) < index, "Range follows actual terrain nodes"): return false
    for actor_node in t.children.filter(func(n): return str(n.name).begins_with("Actor_")):
        if not check(names.find(actor_node.name) > index and actor_node.z == 1, "Range precedes actual actor nodes at Z1"): return false
    for actor in s.actors:
        if actor.id != r.actor and actor.hp > 0 and actor.x != null:
            if not check(not r.destinations.any(func(p): return p.X == actor.x and p.Y == actor.y), "Occupied actor excluded: " + actor.id): return false
    return true

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(6)
    if case_name == "private-region":
        await private_region()
        return
    if case_name == "automatic":
        await automatic_lifecycle()
        return
    if case_name == "outcome":
        await outcome_lifecycle()
        return
    if not await wait_for(func(s): return s.get("stage") == "Movement", "authored movement"): return
    await frames(3)
    if not visible_geometry("initial"): return
    var initial := range_read("initial-range")
    var expected := [{"X":2,"Y":3},{"X":3,"Y":3},{"X":5,"Y":3},{"X":6,"Y":3},{"X":7,"Y":3},{"X":8,"Y":3}]
    if not check(initial.destinations.map(func(p): return Vector2i(p.X,p.Y)) == expected.map(func(p): return Vector2i(p.X,p.Y)), "Corridor: enemy block, ally-through/not-stop, budget and barriers"): return
    if not await tap(KEY_RIGHT): return
    if not check(state().movement != null, "Actual preview animation pending"): return
    if not stable(range_read("ally-preview-animation"), initial, "ally animation"): return
    if not await wait_for(func(s): return s.get("movement") == null, "ally preview"): return
    if not stable(range_read("ally-preview"), initial, "ally preview"): return
    if not await tap(KEY_RIGHT): return
    if not await wait_for(func(s): return s.get("movement") == null, "through ally"): return
    if not stable(range_read("through-ally"), initial, "through ally"): return
    if not await tap(KEY_ENTER): return
    if not hidden("actions"): return
    if not await tap(KEY_H): return
    if not hidden("magic-list"): return
    if not await tap(KEY_ESCAPE): return
    if not await tap(KEY_I): return
    if not hidden("item-list"): return
    if not await tap(KEY_ESCAPE): return
    if not await tap(KEY_SPACE): return
    if not hidden("commit-ready"): return
    if not await tap(KEY_ESCAPE): return
    if not await tap(KEY_ESCAPE): return
    if not stable(range_read("cancel-return-animation"), initial, "return animation"): return
    if not await wait_for(func(s): return s.get("movement") == null, "cancel returned"): return
    if not stable(range_read("cancel-restored"), initial, "cancel restored"): return
    if not await tap(KEY_ENTER): return
    if not await tap(KEY_SPACE): return
    if not await tap(KEY_ENTER): return
    if not await wait_for(func(s): return s.get("stage") == "Movement" and s.get("actor") != initial.actor, "next actor"): return
    await frames(3)
    if not visible_geometry("next-actor"): return
    var next := range_read("next-range")
    if not check(next.actor == "guard-a" and next.origin.X == 4 and next.origin.Y == 3 and not next.destinations.has(initial.origin), "Fresh actor occupancy/origin"): return
    finish(true)

func private_region() -> void:
    var deadline := Time.get_ticks_msec() + 15000
    var skipped := false
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("stage") == "Movement": break
        if s.get("failure") != null:
            sample("private-failure")
            fail("Private startup/entry failure")
            return
        if not skipped and not s.get("skipRequiresRelease", true) and "Skip battle intro" in str(s.get("help", "")):
            skipped = true
            await tap(KEY_ESCAPE)
        elif s.get("canWaitForText", false) or s.get("wait") == "ChoiceWait" or "Reveal / Continue" in str(s.get("help", "")):
            await tap(KEY_ENTER)
        else: await process_frame
    if not await wait_for(func(s): return s.get("stage") == "Movement", "private battle"): return
    await frames(6)
    if not visible_geometry("private-region"): return
    var t: Dictionary = samples[-1].terrain
    if not check(t.region.Origin.X == 16 and t.region.Origin.Y == 19 and t.layers.size() == 2 and t.layers.all(func(l): return l.drawSequence > 0), "Retained nonzero private region actual ground/foreground"): return
    if not check(t.layers.all(func(l): return l.z == (2 if l.highPriority else 0)), "Preserved source foreground ordering"): return
    previous_map = view.get_node("MapViewport")
    range_read("private-range")
    for turn in range(4):
        if state().actor == "ally-0": break
        await tap(KEY_ENTER)
        if not hidden("private-action-" + str(turn)): return
        await tap(KEY_SPACE)
        await tap(KEY_ENTER)
        if not await wait_for(func(s): return s.get("stage") == "Movement" and s.get("movement") == null, "private next actor"): return
    if not check(state().actor == "ally-0", "Ordinary Egress user control"): return
    await tap(KEY_ENTER)
    await tap(KEY_H)
    if not hidden("private-magic"): return
    await tap(KEY_ENTER)
    if not hidden("private-target"): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.get("retreat", {}).get("revealed", false), "retreat text"): return
    if not hidden("retreat"): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "field return", 15.0): return
    sample("field-return")
    if not check(not is_instance_valid(previous_map) or not previous_map.is_visible_in_tree(), "Old battle map/range detached at return"): return
    if not check(state().sessionId == samples[0].state.sessionId, "Same session private return"): return
    finish(true)

func automatic_lifecycle() -> void:
    if not await wait_for(func(s): return s.get("stage") == "Movement", "automatic case player"): return
    await frames(3)
    if not visible_geometry("before-automatic"): return
    for turn in range(5):
        await tap(KEY_ENTER)
        await tap(KEY_SPACE)
        await tap(KEY_ENTER)
        if not await wait_for(func(s): return s.get("stage") == "Movement" or s.get("movement") != null, "next turn or automatic movement"): return
        if state().get("movement") != null:
            if not check(state().movement.purpose == "Automatic", "Ordinary automatic actor movement"): return
            if not hidden("automatic-moving"): return
            finish(true)
            return
    fail("Expected automatic moving actor not observed")

func outcome_lifecycle() -> void:
    await tap(KEY_ENTER) # Interact with adjacent authored entry.
    var deadline := Time.get_ticks_msec() + 12000
    while Time.get_ticks_msec() < deadline and state().get("stage") != "Movement":
        var s := state()
        if s.get("canWaitForText", false) or s.get("wait") in ["DialogueWait","ChoiceWait"]:
            await tap(KEY_ENTER)
        else: await process_frame
    if not await wait_for(func(s): return s.get("stage") == "Movement", "outcome player"): return
    await frames(3)
    if not visible_geometry("before-outcome"): return
    var initial := state()
    previous_map = view.get_node("MapViewport")
    await tap(KEY_ENTER)
    await tap(KEY_F)
    if not hidden("physical-target"): return
    await tap(KEY_ENTER)
    var observed_scene := false
    var observed_story := false
    deadline = Time.get_ticks_msec() + 15000
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("canWaitAtInput", false): break
        if s.has("stage"):
            if s.get("scene", {}).get("visible", false) and not observed_scene:
                observed_scene = true
                if not hidden("committed-scene"): return
            if s.get("story", {}).get("active", false) and not observed_story:
                observed_story = true
                if not hidden("outcome-story"): return
            if s.get("story", {}).get("visible", false) or s.get("scene", {}).get("visible", false):
                await tap(KEY_ENTER)
            else: await process_frame
        else:
            if s.get("wait") != null and not observed_story:
                observed_story = true
                sample("outcome-field-story")
                if is_instance_valid(previous_map):
                    var retained: Dictionary = JSON.parse_string(previous_map.call("ReadTerrainJson"))
                    samples[-1].retainedRange = retained.movementRange
                    if not check(not retained.movementRange.visible and retained.movementRange.destinations == null, "Range cleared on retained outcome backdrop"): return
            if s.get("canWaitForText", false) or s.get("wait") in ["DialogueWait","ChoiceWait"]:
                await tap(KEY_ENTER)
            else: await process_frame
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "outcome usable field"): return
    sample("outcome-return")
    if not check(observed_scene and observed_story, "Actual committed scene and outcome program observed"): return
    if is_instance_valid(previous_map):
        var retained: Dictionary = JSON.parse_string(previous_map.call("ReadTerrainJson"))
        samples[-1].retainedRange = retained.movementRange
        if not check(not retained.movementRange.visible and retained.movementRange.destinations == null, "Range remains cleared after outcome return"): return
    if not check(state().sessionId == initial.sessionId, "Same session outcome return"): return
    finish(true)
