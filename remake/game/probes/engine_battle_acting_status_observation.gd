extends "res://probes/engine_world_viewport_observation.gd"

# Bounded actual-host input/card/delivery observation. No state setters or frame dumps.
var events: Dictionary = {}
var chinese := OS.get_environment("SF2_ACTING_ZH") == "1"
var private_case := OS.get_environment("SF2_ACTING_PRIVATE") == "1"

func edge(action: String, pressed: bool) -> void:
    var event: InputEvent = events[action].duplicate()
    event.pressed = pressed
    Input.parse_input_event(event)
    await frames(2)

func press(action: String) -> void:
    await edge(action, true)
    await edge(action, false)

func signature(s: Dictionary) -> Array:
    return [s.sessionId, s.revision, s.observationSequence, s.mainSeed, s.thinkingSeed,
        s.round, s.queueCursor, s.selection, s.statuses, s.inventories, s.gold,
        s.actors.map(func(a): return [a.id,a.hp,a.mp,a.exp,a.kills,a.defeats])]

func resources(s: Dictionary) -> Array:
    return [s.mainSeed,s.thinkingSeed,s.round,s.queueCursor,s.inventories,s.gold,
        s.actors.map(func(a): return [a.id,a.hp,a.mp,a.exp,a.kills,a.defeats])]

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label,"actor":s.actor,"stage":s.stage,"movement":s.movement,
        "card":s.menu.layout.actorCard,"inspection":s.inspection.active,
        "signature":signature(s),"help":s.menu.layout.help,"failure":s.failure})
    return s

func settled() -> bool:
    return await wait_for(func(s): return s.get("movement") == null and s.get("stage") == "Movement", "actual movement delivery")

func card_at(label: String, x: int, y: int, percent: int) -> bool:
    await frames(3)
    var s := record(label)
    var card: Dictionary = s.menu.layout.actorCard
    if not check(card.visible and card.actor == s.actor and not card.name.is_empty(), label + ": current actor card"): return false
    if not check(("等级" if chinese else "Level") in card.role, label + ": localized live class/level header"): return false
    if private_case and not check(not ("unavailable" in card.role) and not ("未知" in card.role), label + ": admitted class identity"): return false
    var prefix := "地形效果" if chinese else "Land effect"
    if not check(prefix in card.land and ("(%d, %d)" % [x,y]) in card.land, label + ": actor-qualified shown tile"): return false
    if s.stage == "Movement" and not check(OS.get_keycode_string(events.confirm.keycode) in s.menu.layout.help and
            OS.get_keycode_string(events.right.keycode) in s.menu.layout.help, label + ": hints follow actual mapped controls"): return false
    if percent >= 0 and not check(("%d%%" % percent) in card.land, label + ": mover/protection setting"): return false
    var actor: Dictionary = s.actors.filter(func(a): return a.id == s.actor)[0]
    for vital in ["hp","mp"]:
        var gauge: Dictionary = card[vital]
        if not check(gauge.current == actor[vital] and gauge.image.mounted and gauge.image.drawCount > 0,
                label + ": live mounted " + vital + " gauge"): return false
        if not check(gauge.valueRect.y >= card.landRect.y + card.landRect.height and
                gauge.valueRect.y + gauge.valueRect.height <= card.rect.y + card.rect.height,
                label + ": land and vitals fit inside card"): return false
    var terrain: Dictionary = JSON.parse_string(view.get_node("MapViewport").call("ReadTerrainJson"))
    return check(terrain.cellSize == 96 and s.zoom == 1 and s.viewport == s.mapViewport, label + ": fixed world geometry")

func step(action: String, x: int, y: int, from_percent: int, to_percent: int) -> bool:
    await edge(action,true)
    var s := state()
    if not check(s.movement != null, "Real movement segment exists before completion"): return false
    if not await card_at("travelling", x,y,from_percent): return false
    await edge(action,false)
    if not await settled(): return false
    return await card_at("delivered", x+(1 if action == "right" else 0),y+(-1 if action == "up" else 0),to_percent)

func auxiliary() -> void:
    var initial := record("aux-origin")
    await press("confirm")
    await press("equipment")
    var equipment := record("equipment-selection")
    if not check(equipment.stage == "EquipmentChoice" and equipment.equipmentVisible, "Actual equipment submenu owns its existing shell"): return
    var c: Dictionary = equipment.menu.layout.actorCard
    if not check(c.actor == initial.actor and c.hp.current == initial.menu.layout.actorCard.hp.current and
            c.role == initial.menu.layout.actorCard.role, "Equipment selection retains live acting projection behind its existing shell"): return
    await press("cancel")
    if not await card_at("equipment-back",int(initial.previewX),int(initial.previewY),-1): return
    await press("give")
    var give := record("give-inventory-selection")
    c = give.menu.layout.actorCard
    if not check(give.stage == "GiveItemChoice" and c.actor == initial.actor and
            c.hp.current == initial.menu.layout.actorCard.hp.current and c.role == initial.menu.layout.actorCard.role,
            "Actual Give inventory selection retains current actor/resources/header"): return
    await press("cancel")
    await press("right")
    await press("confirm")
    var items := record("item-inventory-submenu")
    if not check(items.menu.page == "Items" and items.menu.layout.actorCard.visible and
            items.menu.layout.actorCard.actor == initial.actor, "Visible inventory submenu retains acting card"): return
    await press("cancel")
    if not check(resources(state()) == resources(initial), "Equipment/Give/Item browsing and Back preserve resources and RNG"): return
    finish(true)

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(4)
    if private_case:
        if not check(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_ACTING_START")) == "started", "Connected controlled-start admission"): return
        await frames(4)
    if host.find_child("BattleSessionView",true,false) == null:
        fail("Ordinary battle host not mounted")
        return
    for action in ["up","down","left","right","confirm","cancel","attack","equipment","give"]:
        events[action] = InputMap.action_get_events("sf2_"+action).filter(func(e): return e is InputEventKey)[0]
    if not await settled(): return
    if OS.get_environment("SF2_ACTING_AUX") == "1":
        await auxiliary()
        return
    var first := state()
    var x := int(first.previewX)
    var y := int(first.previewY)
    if not await card_at("initial",x,y,-1 if private_case else 15): return
    if not await step("up" if private_case else "right",x,y,-1 if private_case else 15,-1 if private_case else 30): return
    if not private_case:
        if not await step("right",x+1,y,30,30): return
        if not await step("right",x+2,y,30,0): return
    # Action-menu Back performs undo without opening inspection, exposing its full return delivery.
    await press("confirm")
    var main := record("confirmed-stop")
    if not check(main.stage == "ActionChoice", "Stop confirms to actual action menu: " + str(main.failure)): return
    await edge("cancel",true)
    var returning := state()
    if not check(returning.movement != null and returning.movement.purpose == "Return", "Actual undo path owns return"): return
    var last_token = null
    var deadline := Time.get_ticks_msec()+4000
    while state().movement != null and Time.get_ticks_msec() < deadline:
        var s := state()
        if s.movement.token != last_token:
            last_token = s.movement.token
            var tile: Dictionary = s.movement.from
            var value := -1 if private_case else 0 if tile.X == x+3 else 30
            if not await card_at("undo-segment",int(tile.X),int(tile.Y),value): return
        await process_frame
    await edge("cancel",false)
    if not await settled(): return
    if not await card_at("undo-origin",x,y,-1 if private_case else 15): return
    if not check(resources(state()) == resources(first), "Movement/undo preserves resources, RNG, round and queue"): return
    var baseline := signature(state())
    await press("cancel")
    if not check(state().inspection.active and not state().menu.layout.actorCard.visible, "Inspection retires ordinary card"): return
    await press("attack")
    if not check(state().inspection.page == "Full", "Current actor full details"): return
    await press("cancel")
    await frames(3)
    if not check(not state().inspection.active and signature(state()) == baseline, "Inspection return is pure and same actor"): return
    if not await card_at("inspection-return",x,y,-1 if private_case else 15): return
    await press("confirm")
    # Stay is down from the diamond's deterministic Attack/Stay initial focus.
    await press("down")
    await press("confirm")
    if not check(state().menu.page == "Commit", "Stay still requires separate confirmation"): return
    await press("confirm")
    if not await wait_for(func(s): return s.get("actor") != null and s.actor != first.actor and s.get("stage") == "Movement", "next player actor"): return
    var next := state()
    if not await card_at("actor-handoff",int(next.previewX),int(next.previewY),-1): return
    if not check(next.menu.layout.actorCard.name != first.menu.layout.actorCard.name, "Handoff retires previous name and class header"): return
    finish(true)

func finish(completed := false) -> void:
    view = null
    if is_instance_valid(host): host.free()
    host = null
    super.finish(completed)
