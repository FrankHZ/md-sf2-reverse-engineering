extends "res://probes/engine_battle_inspection_observation.gd"

# Finite authored command/result witness, through installed native input bindings.
var delivered: Array = []
var last_sequence := 0
var chinese := OS.get_environment("SF2_SEARCH_ZH") == "1"
var full := OS.get_environment("SF2_SEARCH_FULL") == "1"

func observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if int(event.Sequence) > last_sequence:
            delivered.append(event)
            last_sequence = int(event.Sequence)

func inventory(s: Dictionary) -> Array:
    return s.inventories.filter(func(a): return a.actor == "champion")[0].items

func turn(s: Dictionary) -> Array:
    return [s.actor,s.queueCursor,s.round,s.mainSeed,s.thinkingSeed,s.turnOrder,s.previewX,s.previewY]

func record(label: String) -> void:
    var s := state()
    samples.append({"label":label,"turn":turn(s),"inventories":s.inventories,"flags":s.storyFlags,
        "search":s.search,"chests":s.searchChests,"menu":s.menu,"actors":s.actors})

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    if not await enter_battle(): return
    view.connect("SessionResultObserved", observed)
    await act("right")
    if not await settled(): return
    await act("confirm")
    if not check(state().battleChoices.Search == null and state().menu.layout.commands[3].label == ("待机" if chinese else "Stay"), "Unfaced cell retains Stay"): return
    await act("cancel")
    if not await settled(): return
    await act("right")
    if not await settled(): return
    await act("down")
    if not await settled(): return
    await act("confirm")
    var before := state()
    if not check(before.battleChoices.Search.Flag == 701 and before.menu.layout.commands[3].label == ("调查" if chinese else "Search"), "Preview/facing selects actual chest and Down label"): return
    record("faced-preview")
    await act("down"); await act("confirm")
    if not check(state().stage == "SearchCommitReady", "Explicit Search confirmation"): return
    await act("cancel")
    if not check(state().stage == "ActionChoice" and inventory(state()) == inventory(before) and turn(state()) == turn(before), "Cancel retains resources and preview"): return
    await act("down"); await act("confirm")
    var event_start := delivered.size()
    await act("confirm")
    var result := state()
    record("result")
    if not check(result.search.active and result.search.visible and result.search.kind == ("Full" if full else "Item"), "Actual Search result owns input"): return
    if not check(turn(result) == turn(before), "No premature Stay suffix"): return
    if chinese and not check("宝箱" in result.search.text, "Chinese result template is rendered"): return
    # Holding a result input cannot replay the command or fabricate another receipt.
    await send_action("confirm", true); await frames(5); await send_action("confirm", false)
    for n in range(3):
        if not state().search.active: break
        await act("confirm")
    if full:
        if not check(state().stage == "ActionChoice" and state().menu.page == "Main" and turn(state()) == turn(before), "Full returns free to same actual menu and preview"): return
        if not check(inventory(state()) == inventory(before) and not 701.0 in state().storyFlags, "Full does not distribute to other ally or claim chest"): return
        if not check(delivered.slice(event_start).all(func(e): return not e.Kind in ["battle-search-receipt","action-committed","after-turn"]), "Full has no receipt or turn suffix"): return
        await act("cancel")
        if not await settled(): return
        if not check(state().previewX == 1 and state().previewY == 2, "Outer Cancel really undoes preview"): return
    else:
        if not await wait_for(func(s): return s.get("stage") == "Movement" and s.get("movement") == null, "Search turn suffix"): return
        var changes: Array = delivered.slice(event_start)
        if not check(changes.filter(func(e): return e.Kind == "battle-search-receipt").size() == 1 and changes.filter(func(e): return e.Kind == "action-committed" and e.Actor.Value == "champion").size() == 1, "One receipt and one true acting turn commit"): return
        if not check(inventory(state()) == [0.0,189.0,113.0,0.0] and 701.0 in state().storyFlags, "Live inventory and opened flag agree"): return
        var marker := view.get_node("MapViewport/Board/Chest_701") as Label
        if not check(marker.is_visible_in_tree() and marker.text == ("已开" if chinese else "Open"), "Opened chest projects from durable receipt"): return
    record("return")
    samples.append({"events":delivered,"mappedConfirm":OS.get_keycode_string(events.confirm.keycode),"mappedCancel":OS.get_keycode_string(events.cancel.keycode)})
    root.remove_child(host); host.queue_free(); await frames(2)
    finish(true)
