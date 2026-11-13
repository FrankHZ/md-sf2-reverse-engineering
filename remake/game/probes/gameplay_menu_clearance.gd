extends "res://probes/engine_demo_battle_ui_observation.gd"

# Read mounted controls and send ordinary input after a controlled field or battle start.
var menu_button: Button
var geometry_errors: Array[String] = []

func measure(page: String, controls: Array, labels: Array = []) -> void:
    var button := menu_button.get_global_rect()
    var viewport := root.get_visible_rect()
    if not menu_button.is_visible_in_tree() or menu_button.disabled or button.end.x != viewport.end.x - 32 or button.position.y != 32 or button.size != Vector2(160,56):
        geometry_errors.append(page + ": Menu unavailable or outside 32px anchor")
    var row := {"page":page, "window":str(viewport.size), "menu":str(button), "controls":{}, "labels":[]}
    for control: Control in controls:
        var bounds := control.get_global_rect()
        row.controls[str(control.name)] = str(bounds)
        if not control.is_visible_in_tree() or not viewport.encloses(bounds) or bounds.intersects(button):
            geometry_errors.append(page + ": " + str(control.name) + " hidden, clipped or covered by Menu")
    for label: Label in labels:
        var glyphs := Rect2()
        for i in label.text.length():
            var character := label.get_character_bounds(i)
            if character.has_area():
                character = label.get_global_transform() * character
                glyphs = glyphs.merge(character) if glyphs.has_area() else character
        row.labels.append({"text":label.text, "glyphs":str(glyphs)})
        if not label.is_visible_in_tree() or not glyphs.has_area() or not label.get_global_rect().grow(1).encloses(glyphs) or button.intersects(glyphs):
            geometry_errors.append(page + ": clipped or covered label " + label.text)
    samples.append(row)

func cover(by_click: bool) -> bool:
    var caller := view
    var before := state()
    var stable := resources(before) if before.has("stage") else field_position(before)
    if by_click:
        var point := menu_button.get_global_rect().end - Vector2(3,3)
        await move_mouse(point); await mouse(point,true)
        if not expect(host.find_child("SystemMenuView",true,false) == null,"Press alone leaves caller active"): return false
        await mouse(point,false)
    else: await act("menu")
    var resume := host.find_child("SystemResume",true,false) as Button
    if not expect(resume != null and resume.theme_type_variation == "EntrySelectedButton" and not caller.can_process(),
            "Menu owns input at Resume and pauses its caller"): return false
    await act("cancel")
    var after := state()
    return expect(caller.can_process() and menu_button.is_visible_in_tree() and
        stable == (resources(after) if before.has("stage") else field_position(after)) and
        before.get("selection") == after.get("selection") and before.get("candidate") == after.get("candidate") and
        before.get("menu",{}).get("command") == after.get("menu",{}).get("command") and
        before.get("inspection",{}).get("cursor") == after.get("inspection",{}).get("cursor") and
        before.get("inspection",{}).get("page") == after.get("inspection",{}).get("page"),
        "Back restores the running caller, selection and cursor")

func field_position(s: Dictionary) -> Array:
    return [s.sessionId, s.map, s.entities[0].x, s.entities[0].y, s.gold]

func field() -> bool:
    if not await wait_for(func(s):return s.get("canWaitAtInput",false),"settled field"): return false
    var footer := view.find_child("ContextHint",true,false)
    var adventure := footer.find_child("FieldMenuEntry",true,false)
    measure("field", [footer,adventure], [adventure,footer.find_child("Help",true,false)])
    for key in ["normal","hover","pressed","disabled","focus"]:
        var style := menu_button.get_theme_stylebox(key) as StyleBoxFlat
        if not expect(style != null and [style.corner_radius_top_left,style.corner_radius_top_right,
                style.corner_radius_bottom_left,style.corner_radius_bottom_right] == [0,0,0,0],"Menu square in "+key): return false
    if not await cover(true): return false
    await click(control_rect(adventure))
    if not expect(state().fieldMenu.visible,"Adventure entry clickable after host Back"): return false
    await act("cancel")
    return expect(state().canWaitAtInput and not state().fieldMenu.visible,"Back restores exploration")

func inspect_map() -> bool:
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var inspection := view.find_child("Inspection",true,false)
    var hover := inspection.get_node("HoverCard") as Control
    var hint := inspection.get_node("InspectionHint") as Label
    measure("inspection-unit", [hover,hint], hover.find_children("*","Label",true,false) + [hint])
    if not expect(state().inspection.hover.unit == state().actor,"Acting unit in hover card"): return false
    var cell: Dictionary = state().inspection.cursor
    var subject := Rect2(cell_point(cell.X,cell.Y)-Vector2(48,48),Vector2(96,96))
    if not expect(not hover.get_global_rect().intersects(subject),"Card clears inspected unit"): return false
    if not await cover(true): return false
    await act("down")
    if not expect(state().inspection.hover.unit == null,"Reach adjacent empty ground"): return false
    measure("inspection-ground",[hover,hint],[hint])
    await act("confirm")
    if not await inspect_page("Options"): return false
    var options: Dictionary = state().inspection.options
    await click(options.commandList.rows[1].rect)
    if not await inspect_page("Map"): return false
    var panel := inspection.get_node("Overview") as Control
    for window in [Vector2i(2560,1440),Vector2i(1920,1080)]:
        root.size = window; await frames(6)
        measure("overview",[panel,hint],[panel.get_node("Legend"),hint])
        if not await cover(false): return false
    var s := state()
    var overview: Dictionary = s.inspection.overview
    var unit: Dictionary = overview.units[0]
    var map: Dictionary = overview.map
    var point := panel.global_position + Vector2(map.x+(unit.Position.X+0.5)*map.width/s.mapWidth,
        map.y+(unit.Position.Y+0.5)*map.height/s.mapHeight)
    await move_mouse(point); await mouse(point,true); await mouse(point,false)
    if not expect(state().inspection.page == "Cursor" and state().inspection.cursor == unit.Position,
            "Click on relocated map returns to the chosen unit"): return false
    await act("cancel")
    return await settled()

func battle() -> bool:
    if not await enter_battle(): return false
    if not await wait_for(func(_s):return board().settled,"camera settles"): return false
    if not await inspect_map(): return false
    await act("confirm"); await act("down")
    var layout := view.find_child("CommandMenu",true,false).get_node("Layout")
    await frames(8)
    var footer := layout.get_node("DescriptionPanel")
    var back := footer.find_child("Back",true,false)
    for window in [Vector2i(1920,1080),Vector2i(2560,1440),Vector2i(1920,1080)]:
        root.size = window; await frames(6)
        if not await wait_for(func(_s):return board().settled,"decision camera after resize"): return false
        measure("decision",[layout.get_node("ActorCard"),layout.get_node("DecisionGroup"),footer,back],
            [footer.find_child("Detail",true,false),footer.find_child("Help",true,false),back])
        var actor_cell := Rect2(cell_point(state().previewX,state().previewY) - Vector2(48,48),Vector2(96,96))
        samples[-1].actorCell = str(actor_cell); samples[-1].cameraSettled = board().settled
        if not expect(not footer.get_global_rect().intersects(actor_cell),"Reason clears the acting unit"): return false
        if not expect(state().menu.layout.commandList.rows[1].selected,"Magic row remains selected"): return false
        if window.y == 1440 and not expect(footer.size.x == layout.get_node("ActorCard").size.x,
                "Long reason uses normal column when height permits"): return false
    await click(control_rect(back))
    if not expect(state().stage == "Movement","Clicking footer Back restores movement"): return false
    await act("confirm"); await act("down")
    var main: Dictionary = state().menu.layout
    if not await cover(false): return false
    await click(main.commandList.rows[0].rect)
    if not expect(state().menu.page == "Target","Click on Attack row opens targets"): return false
    measure("target",[layout.get_node("TargetCard"),layout.get_node("DescriptionPanel"),layout.find_child("Navigation",true,false)])
    if not await cover(true): return false
    if not expect(state().menu.page == "Target","Host Back restores targeting"): return false
    var target: Dictionary = state().actors.filter(func(a):return a.id == state().candidate)[0]
    var point := cell_point(target.x,target.y)
    await move_mouse(point); await mouse(point,true); await mouse(point,false)
    var scene := view.find_child("SceneUi",true,false)
    if not await wait_for(func(s):return s.scene.phase != null and scene.is_visible_in_tree(),"ordinary attack enters close-up"): return false
    measure("close-up",[scene.get_node("ActorCard"),scene.get_node("TargetCard")])
    return true

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(6)
    var result = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COMPACT_START"))
    if not expect(result == "started","Registered world and controlled start admitted: "+str(result)): return
    bind_events(); await frames(6)
    menu_button = host.find_child("SystemMenuEntry",true,false) as Button
    var ok := await field() if not state().has("stage") else await battle()
    if ok: ok = expect(geometry_errors.is_empty(),str(geometry_errors))
    finish(ok)
