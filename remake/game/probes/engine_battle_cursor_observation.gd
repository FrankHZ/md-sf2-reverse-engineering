extends "res://probes/engine_demo_battle_ui_observation.gd"

# Reuses production input/admission and the bounded observer; never mutates a live session.
var original_cursor := OS.get_environment("SF2_CURSOR_ORIGINAL") == "1"
var expected_sha := OS.get_environment("SF2_CURSOR_SHA256")

func cursor_art(art: Dictionary, cell: Dictionary, label: String) -> bool:
    if not expect(art.visible and art.draws > 0, label+": actual cursor draw"): return false
    if not expect(art.mode == ("original-single-cell" if original_cursor else "authored-fallback"),
            label+": selected original or explicit fallback"): return false
    if not original_cursor: return true
    if not expect(art.resource == "battle-cursor.single-cell" and art.sha256 == expected_sha
            and art.width == 32 and art.height == 32 and art.filter == "Nearest" and art.sourceScale == 4,
            label+": admitted source pixels and nearest 4x binding"): return false
    var center := cell_point(cell.X,cell.Y)
    var r: Dictionary = art.drawn
    return expect(is_equal_approx(r.width,128) and is_equal_approx(r.height,128)
        and abs(r.x+r.width/2-center.x) < 0.1 and abs(r.y+r.height/2-center.y) < 0.1,
        label+": 128px art centered on live 96px cell, 16px overhang")

func target_art(label: String) -> bool:
    await frames(4)
    var s := state()
    var b := board()
    samples.append({"label":label,"state":s,"board":b})
    if not expect(s.menu.page == "Target" and s.candidate != null, label+": actual eligible target"): return false
    var actor: Dictionary = s.actors.filter(func(a):return a.id == s.candidate)[0]
    if not expect(b.actionRange.visible and not b.actionRange.effect.is_empty(),label+": real reach/effect retained"): return false
    if original_cursor:
        if not expect(b.targets.is_empty(),label+": original replaces target border only"): return false
        return cursor_art(b.targetCursor,{"X":actor.x,"Y":actor.y},label)
    return expect(not b.targetCursor.visible and b.targets.size() == 4,
        label+": old/public package keeps its four-edge target fallback")

func cursor_inspection() -> bool:
    var baseline := signature(state())
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    for cell in [Vector2i(0,0),Vector2i(state().mapWidth-1,state().mapHeight-1)]:
        if not await cursor_to(cell.x,cell.y): return false
        await frames(4)
        if not await wait_for(func(_s):return board().settled,"Inspection edge camera settles"): return false
        var b := board()
        samples.append({"label":"inspection-edge","board":b,"inspection":state().inspection})
        if not cursor_art(b.inspectionCursor.art,state().inspection.cursor,"F8 map edge"): return false
        if not expect(b.clip and not b.movementRange.visible and not b.actionRange.visible
                and not b.targetCursor.visible,"Inspection clips art and retires decision overlays"): return false
    var unit: Dictionary = state().inspection.units[0]
    if not await cursor_to(unit.Position.X,unit.Position.Y): return false
    if not await wait_for(func(_s):return board().settled,"Unit hover camera settles"): return false
    await move_mouse(cell_point(unit.Position.X,unit.Position.Y))
    if not expect(state().inspection.cursor == unit.Position,"F8 unit hover retains live cell"): return false
    if not cursor_art(board().inspectionCursor.art,unit.Position,"F8 mouse hover"): return false
    await act("confirm")
    if not await inspect_page("Full"): return false
    var saved: Dictionary = state().inspection.cursor.duplicate()
    await move_mouse(Vector2(960,540))
    if not expect(state().inspection.cursor == saved,"Detail overlay consumes hover without moving board cursor"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    if not cursor_art(board().inspectionCursor.art,saved,"F8 detail return"): return false
    await act("menu")
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    await act("cancel")
    if not expect(state().inspection.page == "Cursor" and state().inspection.cursor == saved
            and signature(state()) == baseline,"Host cover/Back preserves inspection and gameplay"): return false
    await act("cancel")
    if not await settled(): return false
    return expect(not board().inspectionCursor.visible and board().movementRange.visible,
        "Inspection Back retires cursor and restores movement grid")

func settle_scene(previous: String) -> bool:
    await frames(3)
    var b := board()
    if not expect(not b.targetCursor.visible and b.targets.is_empty(),"Committed playback retires target cursor"): return false
    return await super.settle_scene(previous)

func cursor_decision() -> bool:
    var initial := state()
    await act("confirm"); await act("attack")
    if not await target_art("F4 physical"): return false
    var selected: Dictionary = state().actors.filter(func(a):return a.id == state().candidate)[0]
    await move_mouse(cell_point(selected.x,selected.y))
    if not await target_art("F4 mouse hover"): return false
    var baseline := resources(state())
    var wrong := cell_point(initial.previewX,initial.previewY)
    await move_mouse(wrong); await mouse(wrong,true); await mouse(wrong,false); await act("confirm")
    if not expect(state().candidate == null and not board().targetCursor.visible and board().targets.is_empty()
            and resources(state()) == baseline,"Invalid click clears cursor and cannot commit stale target"): return false
    await act("right")
    if not await target_art("F4 direction restores eligible target"): return false
    for size in [Vector2i(2560,1440),Vector2i(1920,1080)]:
        root.size = size; await frames(5)
        if not await target_art("F4 resize"): return false
    await act("cancel")
    if not expect(not board().targetCursor.visible and board().targets.is_empty(),"Target Back retires cursor"): return false
    await act("item"); await act("down"); await act("confirm")
    if not await target_art("F4 friendly item"): return false
    await act("cancel"); await act("cancel"); await act("cancel")
    if not await settled(): return false
    if not await cursor_inspection(): return false
    await act("confirm"); await act("attack")
    if not await target_art("F4 physical after F8"): return false
    await act("confirm")
    if not await settle_scene(initial.actor): return false
    for turn in range(5):
        if state().actor == "ally-1": break
        var previous: String = state().actor
        await act("confirm"); await act("stay"); await act("confirm")
        if not await settle_scene(previous): return false
    if not expect(state().actor == "ally-1","Real schedule reaches HEAL caster"): return false
    await act("confirm"); await act("spell"); await act("confirm")
    if not await target_art("F4 HEAL"): return false
    var heal_before := resources(state())
    await act("right")
    if not await target_art("F4 HEAL target cycle"): return false
    await act("cancel")
    if not expect(state().menu.page == "Magic" and not board().targetCursor.visible
            and resources(state()) == heal_before,"HEAL Back restores level without payment"): return false
    return true

func public_cursor() -> bool:
    await act("confirm"); await act("spell"); await act("confirm")
    if not await target_art("Public authored target fallback"): return false
    await act("cancel"); await act("cancel"); await act("cancel")
    if not await settled(): return false
    return await cursor_inspection()

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(5); bind_events()
    for action in ["spell","item","stay"]:
        events[action] = InputMap.action_get_events("sf2_"+action).filter(func(e):return e is InputEventKey)[0]
    var start := OS.get_environment("SF2_COMPACT_START")
    if not start.is_empty():
        var admitted = host.call("StartPreVoyageLocalizationObservation",start)
        if not expect(admitted == "started","Ordinary full-world admission: "+str(admitted)): return
    if not await enter_battle(): return
    if demo_case in ["area","aura"]:
        finish(await area_case()); return
    if not await wait_for(func(_s):return board().settled,"Initial camera settles"): return
    finish(await public_cursor() if demo_case == "public" else await cursor_decision())
