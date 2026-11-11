extends "res://probes/engine_demo_battle_ui_observation.gd"

# Bounded correction witness: actual F4 headers and the actual F10 targeting help.
var vitals_case := OS.get_environment("SF2_VITALS_CASE")

func disjoint(a: Dictionary, b: Dictionary) -> bool:
    return a.x+a.width <= b.x+1 or b.x+b.width <= a.x+1 or a.y+a.height <= b.y+1 or b.y+b.height <= a.y+1

func header_geometry(label: String) -> bool:
    await frames(5)
    var s := state()
    var layout: Dictionary = s.menu.layout
    var card: Dictionary = layout.targetCard if layout.targetCard.visible else layout.actorCard
    samples.append({"label":label,"menu":s.menu,"stage":s.stage,"window":[root.size.x,root.size.y]})
    if not expect(card.visible and card.decisionHeader and inside(card.rect),label+": decision-only live header fits"): return false
    if not expect(within(card.rect,layout.decisionGroup) and inside(layout.decisionGroup),label+": header belongs to the single decision frame"): return false
    if not expect(card.nameVisibleLines == card.nameLines and card.nameLines <= 2,label+": actual full name fits measured lines"): return false
    if vitals_case == "wide" and not expect(card.name.length() >= 55 and card.hp.maximum == 30000,label+": real prelaunch long-name/wide-vital selection"): return false
    for vital in [card.hp,card.mp]:
        if not expect(vital.fontSize == 36 and vital.image.rect.width == 232 and vital.image.rect.height == 28,label+":36px readings and232x28 images"): return false
        if not expect(vital.valueRect.y+vital.valueRect.height <= vital.image.rect.y+1 and vital.valueMinimumWidth <= vital.valueRect.width+1,
                label+": full numeric value above its gauge"): return false
        if not expect(within(vital.valueRect,card.rect) and within(vital.image.rect,card.rect),label+": vital stays in header"): return false
        if not expect(card.nameRect.y+card.nameRect.height <= vital.valueRect.y+1,label+": name and readings do not overlap"): return false
        if vital.maximum == 0 and not expect(vital.text == "—" and vital.image.fraction == 0,label+": no-MP dash and empty track retained"): return false
    if not expect(card.hp.valueRect.y == card.mp.valueRect.y and card.hp.image.rect.y == card.mp.image.rect.y
            and card.hp.valueRect.x+card.hp.valueRect.width <= card.mp.valueRect.x,label+": parallel separate HP/MP columns"): return false
    if not expect(card.rect.x == layout.description.x and card.rect.width == layout.description.width and disjoint(card.rect,layout.description),
            label+": header and footer form one measured column"): return false
    if layout.commandList.visible:
        if not expect(card.rect.x == layout.commandPanel.x and card.rect.width == layout.commandPanel.width
                and card.rect.y+card.rect.height <= layout.commandPanel.y and within(layout.commandPanel,layout.decisionGroup),label+": layout A header/commands align in one frame without overlap"): return false
    if layout.listVisible and not expect(card.rect.x == layout.list.x and card.rect.width == layout.list.width
            and card.rect.y+card.rect.height <= layout.list.y and within(layout.list,layout.decisionGroup),label+": magic list replaces rows in the same frame"): return false
    for row in layout.descriptionLabels:
        if not expect(within(row.rect,layout.description),label+": full description/hints contained"): return false
    for row in layout.navigation:
        if row.visible and not expect(within(row.rect,layout.description),label+": target controls contained"): return false
    if not expect(inside(layout.description),label+": actual footer fits viewport"): return false
    var entry := host.find_child("SystemMenuEntry",true,false) as Control
    return expect(disjoint(card.rect,control_rect(entry)) and disjoint(layout.description,control_rect(entry)),label+": host entry remains separate")

func host_cover() -> bool:
    var before := resources(state())
    var page: String = state().menu.page
    await act("menu")
    if not expect(host.find_child("SystemMenuView",true,false) != null,"Actual host Menu covers current F4 page"): return false
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    await act("cancel")
    return expect(state().menu.page == page and state().scene.phase == null and resources(state()) == before,"Host return retains page and cannot commit")

func decision_vitals() -> bool:
    await act("confirm")
    if not await header_geometry("main"): return false
    if not await host_cover(): return false
    var baseline := resources(state())
    for i in range(4):
        if state().menu.command == 1: break
        await act("down")
    await act("confirm")
    if not expect(state().menu.page == "Magic","Actual Magic list opens"): return false
    if not await header_geometry("magic-list"): return false
    if not await host_cover(): return false
    # Wide authored data has a real learned attack spell; private Battle01 uses actual Attack.
    if vitals_case == "wide": await act("confirm")
    else:
        await act("cancel"); await act("attack")
    if not expect(state().menu.page == "Target" and state().candidate != null,"Actual legal target page"): return false
    if not await wait_for(func(_s):return board().settled,"actual target camera"): return false
    if not await header_geometry("target"): return false
    if not await host_cover(): return false
    var chosen: String = state().candidate
    var actor: Dictionary = state().actors.filter(func(a):return a.id == chosen)[0]
    var point := cell_point(actor.x,actor.y)
    await move_mouse(point)
    if not expect(state().candidate == chosen and state().scene.phase == null and resources(state()) == baseline,"Hover only previews actual target"): return false
    await mouse(point,true)
    if not expect(state().scene.phase == null,"Press alone does not execute"): return false
    var invalid := cell_point(state().previewX,state().previewY)
    await move_mouse(invalid); await mouse(invalid,false)
    await act("confirm")
    if not expect(state().scene.phase == null and resources(state()) == baseline,"Invalid release/confirmation cannot submit earlier target"): return false
    await act("right")
    if not expect(state().candidate != null,"Mapped pad/direction restores a legal target"): return false
    await act("cancel")
    if not expect(state().menu.page == ("Magic" if vitals_case == "wide" else "Main") and resources(state()) == baseline,"Target Cancel returns freely"): return false
    if vitals_case == "wide": await act("confirm")
    else: await act("attack")
    for size_ in [Vector2i(2560,1440),Vector2i(3440,1440),Vector2i(1920,1080)]:
        root.size = size_; await frames(5)
        if not await header_geometry("target-resize-"+str(size_)): return false
    if not await wait_for(func(_s):return board().settled,"resized target camera"): return false
    actor = state().actors.filter(func(a):return a.id == state().candidate)[0]
    point = cell_point(actor.x,actor.y)
    await move_mouse(point); await mouse(point,true); await mouse(point,false)
    var committed := state()
    samples.append({"label":"click-committed","scene":committed.scene,"stage":committed.stage})
    if not expect(committed.scene.phase != null and not committed.menu.visible,"Complete legal-target click immediately executes without another Confirm"): return false
    if not expect(not committed.scene.actorCard.decisionHeader and not committed.scene.targetCard.decisionHeader,
            "Close-up retains its existing non-decision cards"): return false
    return expect(committed.scene.actorCard.hp.image.rect.y < committed.scene.actorCard.mp.image.rect.y,"Close-up readings retain prior stacked layout")

func help_vitals() -> bool:
    if not await help_case(): return false
    var row: Dictionary = samples.filter(func(s):return s.label == "actual-battle-help")[0].help
    var language: String = JSON.parse_string(host.call("ReadSettingsHostObservationJson")).localization.language
    return expect(("悬停合法目标仅预览" in row.body and "完整点击该目标会立即执行行动" in row.body and "不能提交先前目标" in row.body)
        if language == "zh-CN" else ("Hover a legal target to preview it" in row.body and "complete click" in row.body and "executes the action immediately" in row.body
            and "cannot submit the earlier target" in row.body),"Actual F10 help states preview versus immediate click execution and invalid-click safety")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(5)
    if vitals_case == "wide" and host.find_child("BattleSessionView",true,false) == null:
        samples.append({"label":"startup-refusal","text":host.find_children("*","Label",true,false).map(func(n):return n.text)})
        expect(false,"Controlled authored variant passes ordinary admission"); return
    bind_events()
    if vitals_case != "wide":
        var admitted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COMPACT_START"))
        if not expect(admitted == "started","Admitted current full-world consumer start: "+str(admitted)): return
    if vitals_case == "help": finish(await help_vitals()); return
    if not await enter_battle(): return
    if not await wait_for(func(_s):return board().settled,"initial camera"): return
    finish(await decision_vitals())
