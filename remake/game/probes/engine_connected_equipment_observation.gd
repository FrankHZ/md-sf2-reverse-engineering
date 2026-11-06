extends "res://probes/engine_battle_inspection_observation.gd"

# Reuse mapped native input, controlled Map57 admission, live query and actual projection.
# No gameplay setters, source edits, screenshots or original natural-route claim.
var chinese := OS.get_environment("SF2_EQUIPMENT_ZH") == "1"

func equipment_actor() -> Dictionary:
    var s := state()
    return s.equipmentActors.filter(func(a): return a.actor == s.actor)[0]

func free_state(s: Dictionary) -> Array:
    return [s.actor, s.previewX, s.previewY, s.mainSeed, s.thinkingSeed, s.queueCursor, s.round,
        s.turnOrder, s.gold, s.itemCustody, s.deathCredit,
        s.actors.map(func(a): return [a.id,a.x,a.y,a.hp,a.mp,a.exp,a.kills,a.defeats,a.status])]

func equipment_sample(label: String) -> void:
    var s := state()
    samples.append({"label":label, "signature":signature(s), "freeState":free_state(s),
        "equipment":s.equipmentActors, "equipmentText":s.equipmentText,
        "visible":s.equipmentVisible, "menu":s.menu, "choices":s.battleChoices})

func choose_slot(slot: int) -> bool:
    for n in range(5):
        if state().equipmentSlot == slot: return true
        await act("down")
    return fail("Admitted equipment slot not reachable: " + str(slot))

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    events.equipment = InputMap.action_get_events("sf2_equipment").filter(func(e): return e is InputEventKey)[0]
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Registered controlled Map57 approach: " + str(admitted)): return
    await frames(2)
    if not await enter_battle(): return
    await act("confirm")
    var before := state()
    if not check(before.stage == "ActionChoice", "Real connected action menu"): return
    var initial_gear := equipment_actor()
    equipment_sample("connected-actions-before")
    if not check(OS.get_keycode_string(events.equipment.keycode) in str(before.menu), "Command menu advertises installed equipment binding"): return
    await act("equipment")
    if not check(state().stage == "EquipmentChoice" and state().equipmentVisible, "Existing entry mounts real equipment choices"): return
    if not await choose_slot(4): return
    if not check(("空手" if chinese else "Unarmed") in state().equipmentText, "Localized Unarmed is actually projected"): return
    if not check(equipment_actor() == initial_gear and state().inventories == before.inventories, "Preview preserves live gear"): return
    equipment_sample("unarmed-preview")
    await act("cancel")
    if not check(state().stage == "ActionChoice" and state().inventories == before.inventories
            and free_state(state()) == free_state(before), "Cancel returns same action preview without payment"): return
    await act("equipment")
    if not await choose_slot(4): return
    await act("confirm")
    var unarmed := equipment_actor()
    if not check(state().stage == "ActionChoice" and unarmed.attack == unarmed.baseAttack
            and unarmed.minimumRange == 1 and unarmed.maximumRange == 1
            and free_state(state()) == free_state(before), "Free Unarmed confirmation refreshes live ATT/range"): return
    equipment_sample("unarmed-confirmed")
    await act("equipment")
    if not await choose_slot(int(initial_gear.equippedSlot)): return
    await act("confirm")
    if not check(equipment_actor() == initial_gear and state().inventories == before.inventories
            and free_state(state()) == free_state(before), "Reequip uses same carried slots and current base once"): return
    equipment_sample("reequipped-confirmed")
    await act("cancel")
    if not await settled(): return
    if OS.get_environment("SF2_EQUIPMENT_APPROACH_ONLY") == "1":
        equipment_sample("same-actor-Movement-after-actual-field-entry")
        finish(true)
        return
    # March the already-equipped actor through ordinary turns in this same battle.
    # Other players Stay; all movement uses the existing finite legal-stop query.
    for turn in range(15):
        var boundary := state()
        samples.append({"label":"ordinary-turn", "actor":boundary.actor, "previewX":boundary.previewX,
            "previewY":boundary.previewY, "round":boundary.round, "cursor":boundary.queueCursor,
            "stage":boundary.stage, "menu":boundary.menu.page})
        if state().actor == before.actor:
            if not check(equipment_actor() == initial_gear, "Changed equipment survives subsequent ordinary turns"): return
            if not await march(): return
        await act("confirm")
        var attack: Dictionary = state().battleChoices.Actions.filter(func(a): return a.Action.Kind == 2)[0]
        if state().actor == before.actor and attack.Enabled:
            await act("attack")
            if not check(state().stage == "CommitReady", "Reequipped actor selects an actual legal target"): return
            var target: String = state().target
            var hp: int = state().actors.filter(func(a): return a.id == target)[0].hp
            await act("confirm")
            if not await next_player(): return
            if not check(state().actors.filter(func(a): return a.id == target)[0].hp < hp, "Subsequent ordinary physical attack delivers damage"): return
            equipment_sample("subsequent-legal-attack-and-player-control")
            finish(true)
            return
        await act("down"); await act("confirm"); await act("confirm")
        if not await next_player(): return
    fail("Bounded ordinary turns did not reach a legal equipped attack")

func march() -> bool:
    var s := state()
    var enemies: Array = s.actors.filter(func(a): return a.control == "Automatic" and a.hp > 0 and a.x != null)
    var origin := Vector2i(int(s.previewX), int(s.previewY))
    var allowed: Dictionary = {}
    for p in board().movementRange.destinations: allowed[Vector2i(int(p.X),int(p.Y))] = true
    var parents := {origin:origin}
    var pending := [origin]
    var best := origin
    # Search only the existing query's finite legal stops; every step is still real input.
    while not pending.is_empty():
        var p: Vector2i = pending.pop_front()
        if nearest(p,enemies) < nearest(best,enemies): best = p
        for delta in [Vector2i.UP,Vector2i.LEFT,Vector2i.RIGHT,Vector2i.DOWN]:
            var neighbor: Vector2i = p+delta
            if allowed.has(neighbor) and not parents.has(neighbor):
                parents[neighbor] = p
                pending.append(neighbor)
    var path: Array = []
    var cursor := best
    while cursor != origin:
        path.push_front(cursor)
        cursor = parents[cursor]
    for point in path:
        var current := state()
        var delta: Vector2i = point-Vector2i(int(current.previewX),int(current.previewY))
        await act("up" if delta == Vector2i.UP else "left" if delta == Vector2i.LEFT else "right" if delta == Vector2i.RIGHT else "down")
        if not await settled(): return false
    return true

func nearest(point: Vector2i, enemies: Array) -> int:
    var gap := 999
    for enemy in enemies: gap = mini(gap,abs(point.x-int(enemy.x))+abs(point.y-int(enemy.y)))
    return gap

func next_player() -> bool:
    var deadline := Time.get_ticks_msec()+20000
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("failure") != null: return fail("Ordinary turn failure: " + str(s.failure))
        if s.get("stage") == "Movement" and s.get("movement") == null: return true
        if s.get("scene",{}).get("phase") in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
            await act("confirm")
        else: await process_frame
    equipment_sample("unsettled-ordinary-turn")
    return fail("Ordinary turn did not settle")

func finish(completed := false) -> void:
    view = null
    if is_instance_valid(host): host.free()
    host = null
    super.finish(completed)
