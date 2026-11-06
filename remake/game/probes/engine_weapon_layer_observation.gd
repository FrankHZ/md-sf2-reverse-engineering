extends "res://probes/engine_connected_equipment_observation.gd"

# One ordinary attack or adjacent sword item gesture in the selected opening. Retain only unique
# semantic frame combinations, not per-frame recordings or screenshots.
var combinations: Dictionary = {}
var attacks: Dictionary = {}
var front_variation := OS.get_environment("SF2_WEAPON_FRONT_VARIATION") == "1"
var chosen_actor := "ally-0" if OS.get_environment("SF2_WEAPON_SWORD") == "1" else "ally-2"

func monitor() -> void:
    if ended or not is_instance_valid(host): return
    var battle := host.find_child("BattleSessionView", true, false)
    if battle == null or not battle.is_visible_in_tree(): return
    var board_state: Dictionary = JSON.parse_string(battle.call("ReadObservationJson"))
    if board_state.has("pendingFieldReturn"): return
    var s: Dictionary = JSON.parse_string(battle.call("ReadSceneObservationJson"))
    var c: Dictionary = s.scene
    if not c.visible or not c.weaponVisible: return
    var key := str([c.displayedAlly,c.phase,c.animationIndex,c.frameIndex])
    if c.phase not in ["ActionMessage","ActionAnimation","ResultMessage"] or combinations.has(key): return
    combinations[key] = true
    var row := {"label":"weapon-combination", "actor":c.actorCard.actor}
    for field in ["phase","actionKind","displayedAlly","animationIndex","frameIndex","allyFrame","allyResource","weaponResource",
            "weaponFlipH","weaponFlipV","weaponX","weaponY","allyX","allyY","weaponZ","allyZ",
            "weaponRect","message","actorCard","targetCard","error"]:
        row[field] = c[field]
    samples.append(row)
    var canvas := battle.find_child("SceneCanvas",true,false)
    var overlay := battle.find_child("SceneUi",true,false)
    if not check(c.error == null and canvas != null and overlay != null and overlay.is_visible_in_tree(),
            "Actual scene and separate UI overlay remain available"): return
    var ui_canvas := overlay.get_parent().get_parent()
    if not check(ui_canvas is CanvasLayer and ui_canvas.layer == 1 and c.actorCard.visible and c.targetCard.visible,
            "Cards/messages remain on battle UI canvas above scene-local Z"): return
    row.uiCanvasLayer = ui_canvas.layer
    var front: bool = front_variation and c.phase == "ActionAnimation" and c.animationIndex == 0 and c.frameIndex == 2
    if not check(c.weaponZ == (3 if front else 1) and c.allyZ == 2,
            "Selected weapon layer projects on the declared side of the body"): return
    if not check(c.weaponRect.x == 0 and c.weaponRect.y == 0 and c.weaponRect.width == 128 and c.weaponRect.height == 128,
            "Flip keeps the selected weapon raster footprint"): return
    if c.allyResource.begins_with("ally2/"):
        var idle: bool = c.phase != "ActionAnimation" or c.actorCard.actor != "ally-2" or c.frameIndex < 0
        if not check(not c.weaponFlipH and not c.weaponFlipV and c.weaponResource == "weapon56/frame0"
                and c.weaponX-c.allyX == (12 if idle else -7)
                and c.weaponY-c.allyY == (-1 if idle else -6), "KNTE frame, flip and body-relative offset"): return
    elif c.allyResource.begins_with("ally0/"):
        # Selected sequence0 operands, independently read from the pinned ROM.
        var pose := [0,1,true,false,-16,10]
        if c.phase == "ActionAnimation" and c.actorCard.actor == "ally-0" and c.frameIndex >= 0:
            pose = [[3,1,false,false,62,-18],[3,0,false,false,62,-22],[4,2,true,true,-18,53]][int(c.frameIndex)]
        if not check(c.allyFrame == pose[0] and c.weaponResource == "weapon71/frame"+str(pose[1])
                and c.weaponFlipH == pose[2] and c.weaponFlipV == pose[3]
                and c.weaponX-c.allyX == pose[4] and c.weaponY-c.allyY == pose[5],
                "SDMN selected body/hold frame, weapon flip and relative offset"): return

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    events.item = InputMap.action_get_events("sf2_item").filter(func(e): return e is InputEventKey)[0]
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Existing controlled field entry: " + str(admitted)): return
    await frames(2)
    if not await enter_battle(): return
    process_frame.connect(monitor)
    for turn in range(24):
        var s := state()
        if s.actor == chosen_actor and chosen_actor == "ally-2":
            if not await march(): return
        await act("confirm")
        if ended: return
        var action: Dictionary = state().battleChoices.Actions.filter(func(a): return a.Action.Kind == 2)[0]
        if s.actor == "ally-0" and chosen_actor == "ally-0":
            await act("item")
            await act("down"); await act("confirm")
            for attempt in range(3):
                if state().target == chosen_actor: break
                await act("target")
            if not check(state().stage == "CommitReady" and state().target == chosen_actor,
                    "Actual carried Herb selects its own holder"): return
            var inventory = state().inventories
            await act("confirm")
            if ended or not await next_player(): return
            if not check(state().inventories != inventory, "Real item consumed and next player control released"): return
            if not check(samples.any(func(r): return r.get("weaponFlipH",false) and r.get("weaponFlipV",false)),
                    "Adjacent sword item gesture reaches both-flip weapon frame"): return
            if front_variation and not check(samples.any(func(r): return r.get("weaponZ") == 3)
                    and samples.any(func(r): return r.get("weaponZ") == 1), "Valid layer variation switches behind/front/behind within one delivered scene"): return
            samples.append({"label":"item-completed", "after":free_state(state())})
            finish(true)
            return
        if s.actor == chosen_actor and action.Enabled:
            await act("attack")
            if not check(state().stage == "CommitReady", "Ordinary attack selects a legal target"): return
            var target: String = state().target
            var hp: int = state().actors.filter(func(a): return a.id == target)[0].hp
            await act("confirm")
            if ended or not await next_player(): return
            if not check(state().actors.filter(func(a): return a.id == target)[0].hp < hp,
                    "Actual attack applies damage and returns player control"): return
            attacks[s.actor] = true
            samples.append({"label":"attack-completed", "actor":s.actor, "target":target,
                "beforeHp":hp, "after":free_state(state())})
        else:
            await act("down"); await act("confirm"); await act("confirm")
            if ended or not await next_player(): return
        if attacks.size() == 1:
            if not check(samples.any(func(r): return r.get("animationIndex") == (2 if chosen_actor == "ally-2" else 0) and r.get("frameIndex") == 0),
                    "Selected actual attack pose reached"): return
            if chosen_actor == "ally-0" and not check(samples.any(func(r): return r.get("weaponFlipH",false)), "Adjacent sword horizontal flip reached"): return
            finish(true)
            return
    fail("Bounded ordinary equipped attack was not reached")

func finish(completed := false) -> void:
    if process_frame.is_connected(monitor): process_frame.disconnect(monitor)
    super.finish(completed)

func next_player() -> bool:
    var deadline := Time.get_ticks_msec()+20000
    while not ended and Time.get_ticks_msec() < deadline:
        var s := state()
        if s.has("pendingFieldReturn"): return fail("Battle outcome reached before the selected attack returned player control")
        if s.get("failure") != null: return fail("Ordinary turn failure: " + str(s.failure))
        if s.get("stage") == "Movement" and s.get("movement") == null: return true
        if s.get("scene",{}).get("requiresAcknowledgement",false): await act("confirm")
        else: await process_frame
    return false if ended else fail("Ordinary turn did not settle")
