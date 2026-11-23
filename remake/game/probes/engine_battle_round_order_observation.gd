extends "res://probes/engine_battle_cursor_observation.gd"

# A short authored battle through ordinary host input, reading the production panel.
func rect(r: Dictionary) -> Rect2:
    return Rect2(r.x, r.y, r.width, r.height)

func order_geometry(label: String, expected: Dictionary) -> bool:
    await frames(5)
    var s := state()
    var layout: Dictionary = s.menu.layout
    var order: Dictionary = layout.roundOrder
    var area := rect(order.rect)
    samples.append({"label":label, "client":[root.size.x, root.size.y], "order":order})
    if not expect(order.visible and within(order.rect,s.viewport) and s.failure == null and s.scene.error == null,
            label+": live readout fits without adapter errors"): return false
    for panel in [layout.actorCard, layout.targetCard, layout.commandList, layout.terrainPanel, board().cellCursor]:
        var bounds: Dictionary = panel.rect if panel.has("rect") else panel.drawn
        if panel.visible and not expect(not area.intersects(rect(bounds)),label+": clears "+str(bounds)): return false
    if not expect(not area.intersects(rect(layout.description)) and
            (not layout.listVisible or not area.intersects(rect(layout.list))) and
            (not layout.contextVisible or not area.intersects(rect(layout.context))),label+": clears decision panels"): return false
    var entry := host.find_child("SystemMenuEntry",true,false) as Control
    if not expect(not area.intersects(entry.get_global_rect()),label+": clears host entry"): return false
    var panel := view.find_child("RoundOrder",true,false) as Control
    if not expect(panel.mouse_filter == Control.MOUSE_FILTER_IGNORE and panel.focus_mode == Control.FOCUS_NONE,
            label+": passive panel"): return false
    for child in panel.get_children():
        var reading := child as Label
        if reading.is_visible_in_tree() and not expect(area.encloses(reading.get_global_rect()) and
                reading.mouse_filter == Control.MOUSE_FILTER_IGNORE and reading.get_theme_font_size("font_size") >= 24 and
                reading.get_line_count() == reading.get_visible_line_count() and reading.get_line_count() <= 2,
                label+": full label fits and ignores pointer: "+str(reading.name)): return false
    return expect(order.current == expected.current and order.next == expected.next and
        order.prompt == expected.prompt and order.end == expected.end,label+": live order agrees across pages")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(5)
    if host.find_child("BattleSessionView",true,false) == null:
        samples.append({"label":"startup-refusal","text":host.find_children("*","Label",true,false).map(func(n): return n.text)})
        fail("Controlled authored battle did not start")
        return
    bind_events()
    if not await enter_battle(): return
    if not await wait_for(func(_s): return board().settled,"initial camera"): return
    var start := state()
    var order: Dictionary = start.menu.layout.roundOrder
    samples.append({"label":"admitted-queue","order":order,"queue":start.turnOrder,"cursor":start.queueCursor})
    var chinese := TranslationServer.get_locale().begins_with("zh")
    var duplicate := "Long Controlled Combatant Name"
    if not expect(order.current.begins_with("当前" if chinese else "Now") and order.next.size() >= 2 and
            order.next.size() + order.overflow == 4 and
            order.end == (("本回合另有 %d 次行动" if chinese else "+%d this round") % order.overflow),
            "Localized current, bounded later slots and overflow"): return
    if not expect(order.next[0].begins_with("1. 我方" if chinese else "1. Ally") and "(1,4)" in order.next[0].replace("（","(").replace("）",")") and
            order.next[1].begins_with("2. 敌方" if chinese else "2. Enemy") and "(4,3)" in order.next[1].replace("（","(").replace("）",")") and
            duplicate in order.next[0] and duplicate in order.next[1],"Same long name with distinct side, cell and slot"): return
    if not await order_geometry("Movement",order): return
    await act("cancel")
    if not expect(state().inspection.active and not state().menu.layout.roundOrder.visible,"Inspection supersedes the decision HUD"): return
    await act("cancel")
    if not await order_geometry("Movement after inspection",order): return
    if not expect(state().round == start.round and signature(state()) == signature(start),
            "Inspection and refresh preserve gameplay state"): return
    await act("confirm")
    if not await order_geometry("Main",order): return
    var before := signature(state())
    await act("menu")
    if not expect(host.find_child("SystemMenuView",true,false) != null and not state().menu.layout.roundOrder.visible,
            "Host Menu hides order"): return
    root.size = Vector2i(1600,900)
    await frames(5)
    await act("cancel")
    if not await order_geometry("Main after cover at 1600x900",order): return
    if not expect(signature(state()) == before,"Host cover and resize preserve queue, RNG and resources"): return
    var area := rect(state().menu.layout.roundOrder.rect)
    await move_mouse(area.get_center())
    await mouse(area.get_center(),true)
    await mouse(area.get_center(),false)
    if not expect(signature(state()) == before and state().menu.page == "Main","Readout does not capture a Main action"): return
    for command in [1,2]:
        for i in range(posmod(command-state().menu.command,4)): await act("down")
        await act("confirm")
        var page := "Magic" if command == 1 else "Items"
        if not expect(state().menu.page == page,"Opened "+page): return
        if not await order_geometry(page,order): return
        await act("cancel")
    await act("attack")
    if not expect(state().menu.page == "Target" and state().candidate != null,"Real legal Target"): return
    if not await wait_for(func(_s): return board().settled,"target camera"): return
    if not await order_geometry("Target",order): return
    var target: Dictionary = state().actors.filter(func(a): return a.id == state().candidate)[0]
    var at := cell_point(target.x,target.y)
    await move_mouse(at)
    await mouse(at,true)
    await mouse(at,false)
    var committed := state()
    finish(expect(committed.scene.phase != null and not committed.menu.layout.roundOrder.visible and committed.failure == null,
        "Legal complete click reaches delivery and hides the readout"))
