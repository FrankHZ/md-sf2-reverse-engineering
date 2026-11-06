extends "res://probes/engine_connected_equipment_observation.gd"

# Two ordinary connected turns: fill a live pack, then exchange equipped words.
# Reuse the finite legal-stop query and installed input map. No inventory/session setters.
var delivered: Array = []
var last_sequence := 0

func observed(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if int(event.Sequence) > last_sequence:
            delivered.append(event)
            last_sequence = int(event.Sequence)

func inventory(actor: String) -> Array:
    return state().inventories.filter(func(a): return a.actor == actor)[0].items

func gear(actor: String) -> Dictionary:
    return state().equipmentActors.filter(func(a): return a.actor == actor)[0]

func record(label: String) -> void:
    var s := state()
    samples.append({"label":label, "signature":signature(s), "stage":s.stage, "actor":s.actor,
        "inventories":s.inventories, "equipment":s.equipmentActors, "giveMenu":s.giveMenu,
        "give":s.give, "menu":s.menu, "choices":s.battleChoices.Give, "cursor":s.queueCursor})

func approach(recipient: String) -> bool:
    var s := state()
    var target: Dictionary = s.actors.filter(func(a): return a.id == recipient)[0]
    var origin := Vector2i(int(s.previewX),int(s.previewY))
    var allowed: Dictionary = {}
    for p in board().movementRange.destinations: allowed[Vector2i(int(p.X),int(p.Y))] = true
    var parents := {origin:origin}
    var pending := [origin]
    var stop := origin
    var found := false
    while not pending.is_empty():
        var p: Vector2i = pending.pop_front()
        if abs(p.x-int(target.x))+abs(p.y-int(target.y)) == 1:
            stop = p; found = true; break
        for delta in [Vector2i.UP,Vector2i.LEFT,Vector2i.RIGHT,Vector2i.DOWN]:
            var neighbor: Vector2i = p+delta
            if allowed.has(neighbor) and not parents.has(neighbor):
                parents[neighbor] = p; pending.append(neighbor)
    if not check(found, "Existing movement query contains an adjacent legal stop"): return false
    var path: Array = []
    while stop != origin:
        path.push_front(stop); stop = parents[stop]
    for point in path:
        var current := state()
        var delta: Vector2i = point-Vector2i(int(current.previewX),int(current.previewY))
        await act("up" if delta == Vector2i.UP else "left" if delta == Vector2i.LEFT else "right" if delta == Vector2i.RIGHT else "down")
        if not await settled(): return false
    return true

func select_recipient(recipient: String) -> bool:
    await act("confirm")
    for attempt in range(4):
        if state().target == recipient: break
        await act("down")
    if not check(state().target == recipient, "Adjacent recipient is offered through normal input"): return false
    await act("confirm")
    return true

func transfer(recipient: String, exchange: bool) -> bool:
    if not await approach(recipient): return false
    await act("confirm")
    var before := state()
    var donor: String = before.actor
    if not check(OS.get_keycode_string(events.give.keycode) in str(before.menu), "Ordinary menu advertises installed Give mapping"): return false
    await act("give")
    if not check(state().stage == "GiveItemChoice" and state().itemSlot == 0, "Give opens actual equipped slot"): return false
    if not check(("选择要交出" if chinese else "Choose an item to give") in state().giveMenu, "Localized Give consumer is projected"): return false
    record("exchange-item-choice" if exchange else "normal-item-choice")
    await act("cancel")
    if not check(state().stage == "ActionChoice" and state().inventories == before.inventories
            and free_state(state()) == free_state(before), "Item Cancel is free"): return false
    await act("give")
    if not await select_recipient(recipient): return false
    if exchange:
        if not check(state().stage == "GiveExchangeChoice" and state().returnSlot == 0, "Full pack requires exact equipped return slot"): return false
        await act("cancel")
        if not check(state().stage == "GiveTargetChoice" and state().inventories == before.inventories, "Exchange Cancel returns recipient without mutation"): return false
        await act("confirm"); await act("confirm")
    if not check(state().stage == "GiveCommitReady", "Explicit transaction confirmation"): return false
    record("exchange-confirmation" if exchange else "normal-confirmation")
    await act("cancel")
    if not check(state().inventories == before.inventories and free_state(state()) == free_state(before), "Final Cancel preserves both loadouts and preview"): return false
    await act("confirm")
    var given: int = int(inventory(donor)[0])
    var returned: int = int(inventory(recipient)[0])
    var expected_donor := inventory(donor).duplicate(); expected_donor.pop_front(); expected_donor.append(127.0)
    var expected_recipient := inventory(recipient).duplicate()
    if exchange:
        expected_recipient.pop_front(); expected_recipient.append(127.0)
        expected_donor[expected_donor.find(127.0)] = float(returned & 127)
    expected_recipient[expected_recipient.find(127.0)] = float(given & 127)
    var start_events := delivered.size()
    await act("confirm")
    var committed := state()
    if not check(committed.give.active and committed.give.visible, "Actual localized result precedes turn completion"): return false
    if chinese and not check("交给" in committed.give.text and not "Wooden" in committed.give.text
            and not "SARAH" in committed.give.text, "Selected Chinese item/member catalog reaches committed result"): return false
    if not check(inventory(donor) == expected_donor and inventory(recipient) == expected_recipient, "Source compaction and unequipped receipt order"): return false
    if not check(gear(donor).attack == gear(donor).baseAttack and (not exchange or gear(recipient).attack == gear(recipient).baseAttack), "Equipped removal refreshes live ATT on both sides"): return false
    if not check(committed.queueCursor == before.queueCursor and committed.mainSeed == before.mainSeed
            and committed.thinkingSeed == before.thinkingSeed and committed.itemCustody == before.itemCustody
            and committed.gold == before.gold, "Prefix preserves queue, RNG and custody/Deals"): return false
    record("exchange-result" if exchange else "normal-result")
    await act("confirm")
    if state().give.active: await act("confirm")
    if not await next_player(): return false
    var turn_events: Array = delivered.slice(start_events)
    for kind in ["give-committed", "action-committed", "after-turn"]:
        if not check(turn_events.filter(func(e): return e.Kind == kind and e.Actor.Value == donor).size() == 1,
                "Exactly one " + kind + " for giver"): return false
    record("exchange-next-player" if exchange else "normal-next-player")
    return true

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    events.give = InputMap.action_get_events("sf2_give").filter(func(e): return e is InputEventKey)[0]
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Registered Map57 controlled approach: " + str(admitted)): return
    await frames(2)
    if not await enter_battle(): return
    view.connect("SessionResultObserved", observed)
    var recipients: Array = state().inventories.filter(func(a): return a.actor != state().actor and a.actor.begins_with("ally-") and a.items.filter(func(w): return (int(w) & 127) != 127).size() == 3)
    if not check(recipients.size() == 1, "Real initial three-item recipient"): return
    var recipient: String = recipients[0].actor
    if not await transfer(recipient, false): return
    if not check(state().actor != recipient, "Next ordinary actor can perform exchange"): return
    if not await transfer(recipient, true): return
    finish(true)
