extends "res://probes/engine_world_viewport_observation.gd"

# Finite actual input/state/projection witness. No gameplay setters or screenshots.
var events: Dictionary = {}
var route := OS.get_environment("SF2_INSPECTION_ROUTE")
var private_case := OS.get_environment("SF2_INSPECTION_PRIVATE") == "1"
var hidden_case := OS.get_environment("SF2_INSPECTION_HIDDEN") == "1"

func state() -> Dictionary:
    var battle := host.find_child("BattleSessionView", true, false)
    view = battle if battle != null and battle.is_visible_in_tree() else host.find_child("ExplorationSessionView", true, false)
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String) -> Dictionary:
    var s := state()
    var row := {"label":label}
    for key in ["sessionId", "revision", "observationSequence", "stage", "actor", "previewX", "previewY", "selection",
            "movement", "inspectionEntryPending", "inspectionRelease", "mainSeed", "thinkingSeed", "queueCursor",
            "turnOrder", "failure", "inspection", "mapWidth", "mapHeight", "appearance"]:
        row[key] = s.get(key)
    if s.has("inspection"): row.board = board()
    samples.append(row)
    return s

func board() -> Dictionary:
    return JSON.parse_string(view.get_node("MapViewport").call("ReadTerrainJson"))

func send_action(action: String, pressed: bool) -> void:
    var e: InputEvent = events[action].duplicate()
    if e is InputEventJoypadMotion or e is InputEventJoypadButton: e.device = 0
    if e is InputEventJoypadMotion: e.axis_value = events[action].axis_value if pressed else 0.0
    else: e.pressed = pressed
    Input.parse_input_event(e)
    await frames(2)

func act(action: String) -> void:
    await send_action(action, true)
    await send_action(action, false)

func bind_events() -> void:
    for action in ["up", "right", "down", "left", "attack", "confirm", "cancel", "menu", "target"]:
        var mapped := InputMap.action_get_events("sf2_" + action)
        for e in mapped:
            if route == "pad" and e is InputEventJoypadButton or route == "axis" and e is InputEventJoypadMotion:
                events[action] = e
                break
        if not events.has(action):
            events[action] = mapped.filter(func(e): return e is InputEventKey)[0]

func settled() -> bool:
    return await wait_for(func(s): return s.get("movement") == null and s.get("stage") == "Movement", "Movement delivery")

func inspect_page(page: String) -> bool:
    await frames(2)
    var i: Dictionary = state().inspection
    return check(i.active and i.page == page, "Inspection page: " + page + " actual=" + str(i))

func signature(s: Dictionary) -> Array:
    return [s.sessionId, s.revision, s.observationSequence, s.mainSeed, s.thinkingSeed, s.queueCursor,
        s.turnOrder, s.selection, s.storyFlags, s.gold, s.statuses, s.inventories, s.itemCustody, s.deathCredit,
        s.actors.map(func(a): return [a.id,a.x,a.y,a.hp,a.mp,a.exp,a.kills,a.defeats,a.status,a.attack,a.defense,a.items,a.spells])]

func cursor_to(x: int, y: int) -> bool:
    var safety := 0
    while state().inspection.cursor.X != x or state().inspection.cursor.Y != y:
        if ended: return false
        var p: Dictionary = state().inspection.cursor
        await act("right" if p.X < x else "left" if p.X > x else "down" if p.Y < y else "up")
        safety += 1
        if safety > 110: return fail("Cursor did not reach admitted cell")
    return true

func enter_battle() -> bool:
    var deadline := Time.get_ticks_msec() + 25000
    var nudged := false
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("failure") != null: return fail("Entry failure: " + str(s.failure))
        if s.get("stage") == "Movement" and s.get("movement") == null: return true
        if s.get("canWaitAtInput",false) and not nudged:
            nudged = true
            await act("down")
        elif s.get("canWaitForText",false) or s.get("scene",{}).get("requiresAcknowledgement",false) or "Reveal / Continue" in str(s.get("help","")):
            await act("confirm")
        else: await process_frame
    return fail("Controlled approach did not reach ordinary player boundary")

func purity(baseline: Array, label: String) -> bool:
    return check(signature(state()) == baseline, "Post-undo purity: " + label)

func real_returns() -> bool:
    var origin := sample("initial-player-boundary")
    # Action Cancel remains Movement. It must take a later B to enter inspection.
    await act("confirm")
    if not check(state().stage == "ActionChoice", "Ordinary action menu opened"): return false
    await act("cancel")
    if not await settled(): return false
    if not check(not state().inspection.active, "Action-menu Cancel returns only to Movement"): return false
    # No-move entry is read-only, not a fabricated Cancel/revision.
    var before := signature(state())
    await send_action("cancel", true)
    if not await inspect_page("Cursor"): return false
    await frames(10)
    if not purity(before, "held no-move B enters once"): return false
    await send_action("cancel", false)
    await act("confirm")
    if not check(not state().inspection.active and state().stage == "Movement", "Current actor C resumes Movement directly"): return false
    if not purity(before,"current actor C"): return false
    await act("cancel")
    await act("attack")
    if not await inspect_page("Full"): return false
    if not check(state().inspection.full != null, "Actual reused scalar detail widgets mounted"): return false
    await send_action("confirm", true)
    if not check(not state().inspection.active and state().stage == "Movement", "Current actor A sheet closes to Movement"): return false
    await frames(10)
    if not purity(before, "held sheet close cannot confirm gameplay"): return false
    await send_action("confirm", false)
    # Real provisional Movement B must undo/deliver before entry.
    var range: Array = board().movementRange.destinations
    var direction := ""
    for row in [["right",1,0],["down",0,1],["left",-1,0],["up",0,-1]]:
        if range.any(func(p): return p.X == origin.previewX + row[1] and p.Y == origin.previewY + row[2]):
            direction = row[0]
            break
    if not check(not direction.is_empty(), "Existing content supplies a legal neighbor"): return false
    await act(direction)
    if not await settled(): return false
    if not check(state().previewX != origin.previewX or state().previewY != origin.previewY, "Actual provisional move reached another tile"): return false
    await send_action("cancel", true)
    var undo := sample("actual-undo-pending")
    if not check(undo.movement != null and undo.inspectionEntryPending and not undo.inspection.active,
            "Movement B awaits actual return delivery before inspection"): return false
    # Controls during actual delivery cannot open inspection or submit gameplay.
    await send_action("attack", true)
    await send_action("attack", false)
    if not await wait_for(func(s): return s.inspection.active, "post-undo inspection"): return false
    var base := sample("post-undo-baseline")
    if not check(base.previewX == origin.previewX and base.previewY == origin.previewY and base.movement == null
            and base.inspection.cursor.X == origin.previewX and base.inspection.cursor.Y == origin.previewY,
            "Real undo restored origin then inspection cursor starts there"): return false
    await frames(8)
    if not purity(signature(base), "held entry remains read-only"): return false
    await send_action("cancel", false)
    var baseline := signature(state())
    var units: Array = state().inspection.units
    for ally in [true,false]:
        var candidates := units.filter(func(u): return u.Ally == ally and u.Actor.Value != base.actor)
        if candidates.is_empty(): continue
        var u: Dictionary = candidates[0]
        if not await cursor_to(u.Position.X,u.Position.Y): return false
        await act("attack")
        if not await inspect_page("Full"): return false
        var full: Dictionary = state().inspection
        if not check(full.unit.Actor.Value == u.Actor.Value and full.unit.Ally == ally,
                "Live full details bind selected disclosed unit"): return false
        # Wheel over an offered full-sheet list remains local reading input.
        var items_rect: Dictionary = full.full.items.rect
        var wheel := InputEventMouseButton.new()
        wheel.position=Vector2(items_rect.x+10,items_rect.y+10); wheel.button_index=MOUSE_BUTTON_WHEEL_DOWN; wheel.pressed=true
        Input.parse_input_event(wheel); await frames(2)
        if not purity(baseline,"full-sheet wheel reading"): return false
        await act("cancel")
        if not await inspect_page("Cursor"): return false
        await act("confirm")
        if not await inspect_page("Short"): return false
        if not check(state().inspection.shortStatus.visible and state().inspection.land.Actor.Value == u.Actor.Value,
                "Actual short card and actor-qualified land effect"): return false
        await act("confirm")
        if not await inspect_page("Cursor"): return false
    if not purity(baseline,"other-unit detail caller returns"): return false
    # Opposing legal edges, off-camera disclosed unit and on-demand Map.
    for corner in [[0,0],[base.mapWidth-1,base.mapHeight-1]]:
        if not await cursor_to(corner[0],corner[1]): return false
        if not await wait_for(func(_s): return board().settled,"inspection camera settle"): return false
        sample("camera-edge-"+str(corner))
        var b := board()
        if base.mapWidth*96 <= 1920 and not check(is_equal_approx(b.board.x,(1920-base.mapWidth*96)/2.0),"Small board stays centered horizontally"): return false
        if base.mapHeight*96 <= 1080 and not check(is_equal_approx(b.board.y,(1080-base.mapHeight*96)/2.0),"Small board stays centered vertically"): return false
        if not check(b.cellSize == 96 and b.scale == 1 and b.clip and b.inspectionCursor.visible,
                "Cursor uses retained clipped 96px board at edge"): return false
        if not check(b.visibleLocal.x >= 0 and b.visibleLocal.y >= 0
                and b.visibleLocal.x+b.visibleLocal.width <= base.mapWidth*96+0.01
                and b.visibleLocal.y+b.visibleLocal.height <= base.mapHeight*96+0.01,"Camera clipped to admitted area"): return false
    var off := units.filter(func(u): return u.Actor.Value != base.actor and (abs(u.Position.X-base.previewX)>10 or abs(u.Position.Y-base.previewY)>6))
    if private_case and not check(not off.is_empty(),"Existing private battle supplies disclosed off-camera unit"): return false
    if not off.is_empty():
        if not await cursor_to(off[0].Position.X,off[0].Position.Y): return false
        await act("confirm")
        if not await inspect_page("Short"): return false
        sample("off-camera-live-short")
        await act("cancel")
    var empty := Vector2i(0,0)
    while units.any(func(u): return u.Position.X == empty.x and u.Position.Y == empty.y): empty.x += 1
    if not await cursor_to(empty.x,empty.y): return false
    if not await wait_for(func(_s): return board().settled,"camera before Map"): return false
    await act("confirm")
    if not await inspect_page("Options"): return false
    var camera_before := [board().cameraX,board().cameraY]
    await act("confirm")
    if not await inspect_page("Map"): return false
    var map := sample("on-demand-map")
    if not check(map.inspection.overview.units == units and not map.inspection.overview.pointerActions
            and map.inspection.overview.camera.width>0,"Map consumes only disclosed area/units and camera rectangle"): return false
    await send_action("down",true)
    await create_timer(0.3).timeout
    await send_action("down",false)
    if not check(state().inspection.overview.selected != 0 and camera_before == [board().cameraX,board().cameraY],
            "Known-unit list navigates without moving camera or actors"): return false
    await act("cancel")
    if not await inspect_page("Options"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    if not purity(baseline,"Map/options/cursor"): return false
    # Reuse same directional hold and lifecycle owners.
    await cursor_to(0,0)
    await send_action("right",true)
    await create_timer(0.3).timeout
    if not check(state().inspection.cursor.X > 1,"Existing repeat owner repeats inspection cursor"): return false
    root.focus_exited.emit()
    var held: Dictionary = state().inspection.cursor
    await create_timer(0.2).timeout
    root.focus_entered.emit()
    await create_timer(0.2).timeout
    if not check(state().inspection.cursor == held,"Focus handlers clear repeat across return"): return false
    await send_action("right",false)
    await tap_pause(baseline)
    if ended: return false
    await send_action("right",true)
    await frames(2)
    await send_action("cancel",true)
    if not check(not state().inspection.active,"B closes cursor"): return false
    await create_timer(0.25).timeout
    if not purity(baseline,"held direction plus B exit cannot move actor"): return false
    await send_action("cancel",false)
    await send_action("right",false)
    if not purity(baseline,"all browse/close returns"): return false
    sample("same-origin-Movement-return")
    return true

func tap_pause(baseline: Array) -> void:
    sample("before-host-menu")
    await act("menu")
    sample("host-menu-paused")
    var paused: Dictionary = JSON.parse_string(host.call("ReadWindowSupportObservationJson"))
    if not check(paused.battlePaused == "Disabled","Host Menu pauses actual battle consumer"): return
    await create_timer(0.2).timeout
    await act("cancel")
    sample("after-host-menu-close")
    if not check(state().inspection.active,"Host modal close returns to existing inspection"): return
    purity(baseline,"host pause and return")

func hidden_appearance() -> bool:
    await act("cancel")
    var initial := sample("hidden-initial-disclosure")
    if not check(not initial.inspection.units.any(func(u): return u.Actor.Value in ["hidden","returner"]),
            "Unplaced hidden/reserve units absent from public cursor/Map roster"): return false
    await act("cancel")
    await act("up")
    if not await settled(): return false
    await act("confirm")
    # Use ordinary Stay shortcut and confirmation; appearance is real engine progression.
    var stay_event: InputEvent = InputMap.action_get_events("sf2_stay").filter(func(e): return e is InputEventKey)[0].duplicate()
    stay_event.pressed=true
    Input.parse_input_event(stay_event)
    await frames(2)
    stay_event=stay_event.duplicate(); stay_event.pressed=false; Input.parse_input_event(stay_event)
    await frames(2)
    await act("confirm")
    if not await wait_for(func(s): return s.appearance.active,"real pending appearance"): return false
    sample("pending-appearance")
    await act("cancel")
    if not check(not state().inspection.active,"Movement B unavailable during appearance delivery"): return false
    if not await wait_for(func(s): return s.stage=="Movement" and not s.appearance.active,"appearance delivery complete"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var appeared := sample("delivered-appearance-disclosure")
    if not check(appeared.inspection.units.any(func(u): return u.Actor.Value=="hidden")
            and appeared.inspection.units.any(func(u): return u.Actor.Value=="returner"),"Only delivered appearances join live disclosed roster"): return false
    await act("cancel")
    return true

func terrain_projection() -> bool:
    var s := state()
    var o: Dictionary = s.inspection.overview
    if not check(o.terrainCellCount == s.mapWidth*s.mapHeight and s.inspection.terrain.size()==o.terrainCellCount,
            "Map draws exactly admitted Width×Height terrain cells"): return false
    if not check(o.lastTerrainCell.x==s.mapWidth-1 and o.lastTerrainCell.y==s.mapHeight-1,
            "Map terrain excludes packed source/storage cells"): return false
    if not check(o.terrainClasses.size()>=2,"Existing admitted board has multiple schematic terrain classes"): return false
    var colors: Array = []
    for terrain in o.terrainClasses:
        if not check(terrain.cells==s.inspection.terrain.filter(func(t): return t==terrain.value).size(),
                "Actual drawn class count matches admitted semantic terrain: "+terrain.surface): return false
        if not check(not colors.has(terrain.color),"Terrain classes use distinct schematic colors"): return false
        colors.append(terrain.color)
    return true

func ui_projection() -> bool:
    var initial := signature(state())
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    if not check("查看" in state().inspection.hint and "Movement" not in state().inspection.hint,"Chinese cursor copy is player-facing"): return false
    await act("attack")
    if not await inspect_page("Full"): return false
    var full: Dictionary = state().inspection.full
    if not check(full.stats.any(func(s): return s.label=="等级") and "无异常状态" in full.classText,
            "Shared full-detail labels/status use Chinese locale"): return false
    var up := OS.get_keycode_string(events.up.keycode)
    var down := OS.get_keycode_string(events.down.keycode)
    if not check(up in state().inspection.hint and down in state().inspection.hint and "Up/Down" not in state().inspection.hint,
            "Detail scrolling hints use installed remapped directions"): return false
    sample("zh-remapped-full-details")
    await act("cancel")
    await act("cancel")
    var others: Array = state().inspection.units.filter(func(u): return u.Actor.Value != state().actor)
    var other: Dictionary = others[0]
    if not await cursor_to(other.Position.X,other.Position.Y): return false
    await act("confirm")
    if not await inspect_page("Short"): return false
    var short: Dictionary = state().inspection
    if not check("地形效果" in short.hint and str(int(short.land.Percent))+"%" in short.hint
            and "Regular" not in short.hint and "Centaur" not in short.hint and "Movement" not in short.hint,
            "Localized land effect names inspected actor and location without raw mover enums"): return false
    if not check("无异常状态" in short.shortStatus.role,"Shared short card status uses Chinese locale"): return false
    sample("zh-remapped-short-status")
    await act("cancel")
    var units: Array = state().inspection.units
    var empty := Vector2i(0,0)
    while units.any(func(u): return u.Position.X==empty.x and u.Position.Y==empty.y): empty.x+=1
    if not await cursor_to(empty.x,empty.y): return false
    if not await wait_for(func(_s): return board().settled,"UI Map camera baseline"): return false
    await act("confirm")
    if not check("战场选项" in state().inspection.hint,"Options localized"): return false
    await act("confirm")
    if not await inspect_page("Map"): return false
    if not await terrain_projection(): return false
    var map: Dictionary = state().inspection
    if not check("地图" in map.overview.legend and "已知单位" in map.overview.knownText and "地形" in map.overview.terrainTitle,
            "Map legend, terrain and known-unit labels localized"): return false
    if not check(up in map.hint and down in map.hint,"Map list hints use mapped directions"): return false
    var camera := [board().cameraX,board().cameraY]
    await act("down")
    if not check(camera==[board().cameraX,board().cameraY],"Localized terrain Map reads camera without moving it"): return false
    sample("zh-remapped-terrain-map")
    var settings: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
    for key in settings.localization.rows:
        if str(key).begins_with("battle.inspect."):
            var row: Dictionary = settings.localization.rows[key]
            if not check(row.EffectiveLocale=="zh-CN" and row.Reason==null,"Inspection locale row admitted: "+key): return false
    await act("cancel"); await act("cancel"); await act("cancel")
    if not purity(initial,"localized details/terrain Map closes"): return false
    # Same owned process, existing region-only controlled input; no new route replay.
    root.remove_child(host); host.queue_free(); await frames(2)
    host=load("res://Main.tscn").instantiate(); root.add_child(host); await frames(4)
    if not check(host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_INSPECTION_REGION_START"))=="started","Region UI admission"): return false
    if not await enter_battle(): return false
    var region_baseline := signature(state())
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var region: Dictionary = board().region
    if not check(region.Origin.X==16 and region.Origin.Y==19,"Terrain Map uses existing nonzero source region"): return false
    units=state().inspection.units
    empty=Vector2i(state().mapWidth-1,state().mapHeight-1)
    while units.any(func(u): return u.Position.X==empty.x and u.Position.Y==empty.y): empty.x-=1
    if not await cursor_to(empty.x,empty.y): return false
    if not await wait_for(func(_s): return board().settled,"nonzero-region UI camera"): return false
    await act("confirm"); await act("confirm")
    if not await inspect_page("Map"): return false
    if not await terrain_projection(): return false
    sample("zh-nonzero-region-terrain-map")
    await act("cancel"); await act("cancel"); await act("cancel")
    return purity(region_baseline,"nonzero-region terrain Map closes")

func run() -> void:
    root.size=Vector2i(1920,1080)
    host=load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4)
    bind_events()
    if private_case:
        var admitted=host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_INSPECTION_START"))
        if not check(admitted=="started","Registered connected controlled admission: "+str(admitted)): return
        await frames(2)
    if not await enter_battle(): return
    if OS.get_environment("SF2_INSPECTION_UI_ONLY")=="1":
        if not await ui_projection(): return
        root.remove_child(host); host.queue_free(); await frames(2)
        finish(true)
        return
    if not await real_returns(): return
    if hidden_case and not await hidden_appearance(): return
    # Actual host replacement removes the old view/context, never restores its focus/actor.
    await act("cancel")
    var old_session=state().sessionId
    root.remove_child(host); host.queue_free(); await frames(2)
    host=load("res://Main.tscn").instantiate(); root.add_child(host); await frames(4)
    if private_case:
        var admitted=host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_INSPECTION_REGION_START"))
        if not check(admitted=="started","Replacement admission"): return
        await frames(2)
    if not await enter_battle(): return
    if not check(state().sessionId != old_session and not state().inspection.active,"Session replacement retires disposable inspection"): return
    sample("replacement-retired")
    if private_case:
        # Supplement only geometry on an existing admitted nonzero-origin encounter.
        await act("cancel")
        if not await inspect_page("Cursor"): return
        var region: Dictionary = board().region
        if not check(region != null and region.Origin.X == 16 and region.Origin.Y == 19,
                "Existing Battle02 region exercises admitted nonzero origin"): return
        for edge in [[0,0],[state().mapWidth-1,state().mapHeight-1]]:
            if not await cursor_to(edge[0],edge[1]): return
            if not await wait_for(func(_s): return board().settled,"nonzero region camera"): return
            var b := board()
            if not check(is_equal_approx(b.board.x,(region.Origin.X*24-b.cameraX)*4)
                    and is_equal_approx(b.board.y,(region.Origin.Y*24-b.cameraY)*4)
                    and b.cellSize==96 and b.scale==1,"Camera applies source-region offset once at fixed scale"): return
            if not check(b.cameraX>=region.Origin.X*24 and b.cameraY>=region.Origin.Y*24
                    and b.cameraX+480<=(region.Origin.X+state().mapWidth)*24
                    and b.cameraY+270<=(region.Origin.Y+state().mapHeight)*24,"Camera never exposes neighboring packed area"): return
            sample("nonzero-region-edge-"+str(edge))
        await act("cancel")
    root.remove_child(host); host.queue_free(); await frames(2)
    finish(true)
