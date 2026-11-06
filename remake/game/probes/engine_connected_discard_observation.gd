extends "res://probes/engine_connected_equipment_observation.gd"

# Ordinary Map57 -> Battle01 input and actual state/projection. No inventory setters.
var delivered: Array = []
var last_sequence := 0

func observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if int(event.Sequence) > last_sequence:
            delivered.append(event)
            last_sequence = int(event.Sequence)

func inventory() -> Array:
    var s := state()
    return s.inventories.filter(func(a): return a.actor == s.actor)[0].items

func record(label: String) -> void:
    var s := state()
    samples.append({"label":label,"signature":signature(s),"freeState":free_state(s),
        "equipment":s.equipmentActors,"discardMenu":s.discardMenu,"discard":s.discard,
        "menu":s.menu,"choices":s.battleChoices.Discard})

func drop(slot: int, equipped: bool) -> bool:
    var before := state()
    var words := inventory().duplicate()
    if not check(OS.get_keycode_string(events.discard.keycode) in str(before.menu), "Action menu advertises installed Drop binding"): return false
    await act("discard")
    if not check(state().stage == "DiscardItemChoice", "Connected real slot selector"): return false
    for attempt in range(4):
        if state().itemSlot == slot: break
        await act("down")
    if not check(state().itemSlot == slot, "Exact held slot selected"): return false
    if not check(("丢弃" if chinese else "Choose an item to discard") in state().discardMenu, "Localized selection consumer"): return false
    record("equipped-selection" if equipped else "ordinary-selection")
    await act("cancel")
    if not check(state().stage == "ActionChoice" and state().inventories == before.inventories
            and free_state(state()) == free_state(before), "Selection Cancel is free"): return false
    await act("discard")
    for attempt in range(4):
        if state().itemSlot == slot: break
        await act("down")
    await act("confirm")
    if not check(state().stage == "DiscardCommitReady", "Explicit removal confirmation"): return false
    await act("cancel")
    if not check(state().inventories == before.inventories and free_state(state()) == free_state(before), "Confirmation Cancel preserves inventory and movement preview"): return false
    await act("discard")
    for attempt in range(4):
        if state().itemSlot == slot: break
        await act("down")
    await act("confirm")
    var start_events := delivered.size()
    await act("confirm")
    var committed := state()
    record("equipped-result" if equipped else "ordinary-result")
    if not check(committed.discard.active and committed.discard.visible, "Actual result delivery owns control"): return false
    if chinese and not check("丢弃" in committed.discard.text and not "Wooden" in committed.discard.text
            and not "CHESTER" in committed.discard.text, "Selected source names and result template use Chinese catalog"): return false
    words.remove_at(slot); words.append(127.0)
    if not check(inventory() == words, "Removal compacts actual four slots once"): return false
    if equipped and not check(equipment_actor().attack == equipment_actor().baseAttack
            and equipment_actor().equippedSlot == 4, "Equipped removal refreshes actual ATT and equipment"): return false
    if not check(free_state(committed) == free_state(before), "Drop preserves actor, preview, queue, resources, RNG and ordinary custody"): return false
    for attempt in range(3):
        if not state().discard.active: break
        await act("confirm")
    if not check(state().stage == "ActionChoice" and not state().discard.active
            and free_state(state()) == free_state(before), "Reveal/ack returns same free ActionChoice and preview"): return false
    var changes: Array = delivered.slice(start_events)
    if not check(changes.filter(func(e): return e.Kind == "discard-committed").size() == 1
            and changes.filter(func(e): return e.Kind in ["action-committed","after-turn"]).is_empty(), "Exactly one removal receipt and no action suffix"): return false
    record("equipped-free-return" if equipped else "ordinary-free-return")
    return true

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    events.discard = InputMap.action_get_events("sf2_discard").filter(func(e): return e is InputEventKey)[0]
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Registered Map57 approach: " + str(admitted)): return
    await frames(2)
    if not await enter_battle(): return
    view.connect("SessionResultObserved", observed)
    var origin := state()
    var allowed: Array = board().movementRange.destinations
    var step: Dictionary = {}
    for p in allowed:
        if abs(int(p.X)-int(origin.previewX))+abs(int(p.Y)-int(origin.previewY)) == 1:
            step = p; break
    if not check(not step.is_empty(), "Existing movement query offers a nonzero preview"): return
    await act("left" if step.X < origin.previewX else "right" if step.X > origin.previewX else "up" if step.Y < origin.previewY else "down")
    if not await settled(): return
    await act("confirm")
    if not check(state().stage == "ActionChoice" and equipment_actor().equippedSlot == 0, "Actual opening actor with equipped item"): return
    if not await drop(0, true): return
    if not await drop(0, false): return
    var committed_inventory := inventory().duplicate()
    var committed_custody = state().itemCustody
    await act("cancel")
    if not await settled(): return
    if not check(state().actor == origin.actor and state().previewX == origin.previewX and state().previewY == origin.previewY
            and inventory() == committed_inventory and state().itemCustody == committed_custody
            and state().queueCursor == origin.queueCursor, "Outer Cancel undoes movement but preserves committed removals and turn"): return
    record("movement-undone-items-still-removed")
    finish(true)
