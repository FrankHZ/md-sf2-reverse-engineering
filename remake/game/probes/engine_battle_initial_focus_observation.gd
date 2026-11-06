extends "res://probes/engine_world_viewport_observation.gd"

# Bounded actual-host input/offer/focus witness. No session or gameplay setters.
var events: Dictionary = {}

func edge(action: String, pressed: bool) -> void:
    var event: InputEvent = events[action].duplicate()
    event.pressed = pressed
    Input.parse_input_event(event)
    await frames(2)

func press(action: String) -> void:
    await edge(action, true)
    await edge(action, false)

func resources(s: Dictionary) -> Array:
    return [s.mainSeed, s.thinkingSeed, s.queueCursor, s.inventories,
        s.actors.map(func(a): return [a.id, a.x, a.y, a.hp, a.mp])]

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label, "sessionId":s.sessionId, "revision":s.revision,
        "stage":s.stage, "preview":[s.previewX,s.previewY], "actor":s.actor,
        "menu":s.menu, "choices":s.battleChoices, "resources":resources(s), "failure":s.failure})
    return s

func settled() -> bool:
    return await wait_for(func(s): return s.get("movement") == null and s.get("stage") != null, "settled movement")

func main_focus(label: String, expected: int) -> bool:
    var s := record(label)
    if not check(s.battleChoices.Revision == s.revision and s.battleChoices.SessionId == s.sessionId
            and s.battleChoices.Actor.Value == s.actor, label + ": offer belongs to the current revision/actor"): return false
    var available: bool = s.battleChoices.Failure == null and s.battleChoices.Actions.any(func(a):
        return a.Action.Kind == 2 and a.Enabled and a.Targets.any(func(t): return t.Enabled))
    var command := 0 if available else 3
    if not check(s.stage == "ActionChoice" and s.menu.page == "Main" and s.menu.command == command,
            label + ": initial focus follows current enabled physical targets"): return false
    if expected >= 0 and not check(command == expected, label + ": representative target boundary"): return false
    return check(s.menu.layout.diamondVisible and s.menu.layout.commands[command].selected,
        label + ": actual diamond projects the selected command")

func children() -> bool:
    var before := state()
    await press("right")
    await edge("confirm", true)
    await frames(20)
    await edge("confirm", false)
    if not check(state().menu.page == "Items", "Manual Item opens its existing list"): return false
    await press("cancel")
    if not check(state().menu.page == "Main" and state().menu.command == 2, "Item Back keeps manual focus"): return false
    await press("down")
    await press("confirm")
    if not check(state().menu.page == "Commit" and resources(state()) == resources(before),
            "Stay requires a separate confirmation and preserves resources"): return false
    await press("cancel")
    if not check(state().menu.page == "Main" and state().menu.command == 3, "Stay Back keeps chosen focus"): return false
    if before.menu.command == 0:
        await press("up")
        await press("confirm")
        if not check(state().menu.page == "Target", "Attack opens target choice without committing"): return false
        await press("cancel")
        if not check(state().menu.page == "Main" and state().menu.command == 0, "Target Back keeps Attack focus"): return false
    record("children-back")
    return check(resources(state()) == resources(before), "Child selection/Back preserves gameplay and both seeds")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(4)
    if OS.get_environment("SF2_FOCUS_PRIVATE") == "1":
        if not check(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_FOCUS_START")) == "started",
                "Existing connected controlled-start admission"): return
        await frames(4)
    if host.find_child("BattleSessionView", true, false) == null:
        samples.append({"label":"startup-diagnostic", "labels":host.find_children("*", "Label", true, false).map(func(n): return n.text)})
        fail("Ordinary host did not mount a battle; inspect startup labels")
        return
    if not await settled(): return
    for action in ["up","down","left","right","confirm","cancel"]:
        for event in InputMap.action_get_events("sf2_" + action):
            if event is InputEventKey:
                events[action] = event
                break
    var original := record("initial-movement")
    var expected := OS.get_environment("SF2_FOCUS_EXPECTED").split(",")
    await press("confirm")
    if not main_focus("origin-main", int(expected[0])): return
    if not await children(): return
    await press("cancel")
    if not await settled(): return
    if not check(state().stage == "Movement" and resources(state()) == resources(original), "Main Cancel restores origin"): return
    var steps := OS.get_environment("SF2_FOCUS_STEPS").split(",", false)
    for i in steps.size():
        var path := steps[i].split("+", false)
        for j in path.size()-1:
            await press(path[j])
            if not await settled(): return
        await press(path[-1])
        if not await settled(): return
        record("preview-" + str(i))
        await press("confirm")
        if not main_focus("preview-main-" + str(i), int(expected[i+1])): return
        var chosen: int = state().menu.command
        await frames(20)
        if not check(state().menu.command == chosen, "Refresh preserves chosen initial focus"): return
        if not await children(): return
        await press("cancel")
        if not await settled(): return
        if not check(state().stage == "Movement" and resources(state()) == resources(original), "Preview cancellation preserves gameplay/RNG"): return
    await press("confirm")
    if not main_focus("origin-reentry", int(expected[0])): return
    if not check(resources(state()) == resources(original), "Entry/re-entry never commits action or spends RNG"): return
    finish(true)

func finish(completed := false) -> void:
    view = null
    if is_instance_valid(host): host.free()
    host = null
    super.finish(completed)
