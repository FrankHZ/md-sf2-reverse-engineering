extends "res://probes/engine_battle_cursor_observation.gd"

# Bounded correction witness: decision, inspection and playback cards in Main.tscn.
var vitals_case := OS.get_environment("SF2_VITALS_CASE")

func disjoint(a: Dictionary, b: Dictionary) -> bool:
    return a.x+a.width <= b.x+1 or b.x+b.width <= a.x+1 or a.y+a.height <= b.y+1 or b.y+b.height <= a.y+1

func inside(r: Dictionary) -> bool:
    return within(r,state().viewport)

func card_geometry(card: Dictionary, label: String) -> bool:
    if not expect(card.visible and card.frame and inside(card.rect),label+": framed live card fits"): return false
    if not expect(not card.name.is_empty() and card.nameFontSize == 30 and within(card.nameRect,card.rect)
            and card.nameVisibleLines == card.nameLines,label+": complete name fits at body size"): return false
    if not expect(card.roleVisible == (card.level != null),label+": role disclosure follows projected level"): return false
    var header_bottom: float = card.nameRect.y+card.nameRect.height
    if card.roleVisible:
        var level := ("等级 %d" if TranslationServer.get_locale().begins_with("zh") else "Level %d") % int(card.level)
        if not expect(card.roleFontSize == 30 and level in card.role and within(card.roleRect,card.rect)
                and card.roleRect.y >= header_bottom-1,label+": localized role and level fit below name at body size"): return false
        header_bottom = card.roleRect.y+card.roleRect.height
    for vital in [card.hp,card.mp]:
        if not expect(vital.labelFontSize == 30 and vital.fontSize == 30 and vital.image.rect.width >= 220
                and vital.image.rect.height == 28,label+": vital labels and values use body size"): return false
        if not expect(vital.image.rect.x+vital.image.rect.width <= vital.valueRect.x+1 and
                vital.labelRect.x+vital.labelRect.width <= vital.image.rect.x+1 and
                vital.valueRect.y <= vital.image.rect.y and vital.valueRect.y+vital.valueRect.height >= vital.image.rect.y+vital.image.rect.height and
                vital.valueMinimumWidth <= vital.valueRect.width+1,
                label+": full numeric value sits beside its gauge"): return false
        if not expect(within(vital.labelRect,card.rect) and within(vital.valueRect,card.rect)
                and within(vital.image.rect,card.rect),label+": gauge and readings fit the card"): return false
        if not expect(header_bottom <= vital.valueRect.y+1,label+": header and readings do not overlap"): return false
        var reading := "—" if vital.maximum == 0 else "%d/%d" % [vital.current,vital.maximum]
        var fraction: float = 0.0 if vital.maximum == 0 else float(vital.current)/vital.maximum
        if not expect(vital.text == reading and is_equal_approx(vital.image.fraction,fraction),label+": live readings and fill agree"): return false
    if not expect(card.hp.valueRect.y+card.hp.valueRect.height <= card.mp.valueRect.y and
            card.hp.valueRect.x == card.mp.valueRect.x and card.hp.image.rect.width == card.mp.image.rect.width,
            label+": HP/MP rows align in one column"): return false
    return true

func header_geometry(label: String) -> bool:
    await frames(5)
    var s := state()
    var layout: Dictionary = s.menu.layout
    var card: Dictionary = layout.targetCard if layout.targetCard.visible else layout.actorCard
    samples.append({"label":label,"menu":s.menu,"stage":s.stage,"window":[root.size.x,root.size.y]})
    if not card_geometry(card,label) or not expect(card.decisionHeader,label+": actual decision card"): return false
    if vitals_case == "wide" and not expect(card.name.length() >= 55 and card.nameLines == 2 and card.hp.maximum == 30000,
            label+": long English name wraps beside five-digit vitals"): return false
    if vitals_case == "zh-long" and card.actor == "ally-1" and not expect(card.name.begins_with("北境守望")
            and card.nameLines == 2 and "祭司" in card.role,label+": admitted Chinese name wraps above its class"): return false
    if not expect(card.rect.x == layout.description.x and card.rect.width == layout.description.width and disjoint(card.rect,layout.description),
            label+": header and footer form one measured column"): return false
    if not expect(layout.contextText == (("回合 %d" if TranslationServer.get_locale().begins_with("zh") else "Round %d") % s.round),label+": round has no currency"): return false
    if not expect(layout.terrainPanel.visible == layout.actorCard.visible,label+": terrain belongs to the acting phase"): return false
    if layout.terrainPanel.visible and not expect(disjoint(layout.terrainPanel.rect,card.rect) and inside(layout.terrainPanel.rect),
            label+": terrain has its own visible space"): return false
    if layout.contextVisible and layout.terrainPanel.visible and not expect(layout.context.width == layout.terrainPanel.rect.width and
            layout.context.height == layout.terrainPanel.rect.height,label+": round and land effect form equal rows"): return false
    if layout.commandList.visible:
        if not expect(card.rect.x == layout.commandList.rect.x and card.rect.width == layout.commandList.rect.width
                and disjoint(card.rect,layout.commandList.rect) and disjoint(layout.commandList.rect,layout.description),
                label+": card and Main rows clear the reader"): return false
    if layout.listVisible and not expect(card.rect.x == layout.list.x and card.rect.width == layout.list.width
            and disjoint(card.rect,layout.list),label+": child list replaces Main below the card"): return false
    if not expect(within(layout.reader.rect,layout.description),label+": text scrolls inside the reader"): return false
    for row in layout.navigation:
        if row.visible and not expect(within(row.rect,layout.description),label+": target controls contained"): return false
    if not expect(inside(layout.description),label+": actual footer fits viewport"): return false
    var hint := view.find_child("DescriptionPanel",true,false).find_child("Help",true,false) as Label
    if not expect(hint.is_visible_in_tree() and hint.get_theme_font_size("font_size") == 24,
            label+": operating help uses the hint size"): return false
    var entry := host.find_child("SystemMenuEntry",true,false) as Control
    return expect(disjoint(card.rect,control_rect(entry)) and disjoint(layout.description,control_rect(entry)),label+": host entry remains separate")

func inspection_vitals() -> bool:
    var baseline := signature(state())
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var inspection: Dictionary = state().inspection
    if not card_geometry(inspection.hover,"inspected ally"): return false
    var hint := view.find_child("InspectionFooter",true,false).find_child("Instructions",true,false) as Label
    if not expect(hint.is_visible_in_tree() and hint.get_theme_font_size("font_size") == 24,
            "Inspection instructions use the hint size"): return false
    samples.append({"label":"inspected-ally","inspection":inspection})
    await act("confirm")
    if not await inspect_page("Full"): return false
    var details := view.find_child("Details",true,false)
    var hp_label := details.get_node("HP/Label") as Label
    var hp_value := details.get_node("HP/Value") as Label
    if not expect(hp_label.is_visible_in_tree() and hp_value.get_theme_font_size("font_size") == 30
            and hp_label.get_theme_font_size("font_size") == 30
            and details.get_global_rect().encloses(hp_value.get_global_rect()),
            "Mounted details reuse the readable vital gauge"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var enemy: Dictionary = inspection.units.filter(func(unit):return not unit.Ally)[0]
    if not await cursor_to(enemy.Position.X,enemy.Position.Y): return false
    await frames(5)
    inspection = state().inspection
    var hover: Dictionary = inspection.hover
    samples.append({"label":"inspected-enemy","inspection":inspection})
    if not card_geometry(hover,"inspected enemy") or not expect(not hover.roleVisible and hover.role.is_empty(),
            "Enemy card has no role or level"): return false
    if not expect(disjoint(hover.rect,inspection.terrainPanel.rect) and disjoint(hover.rect,inspection.footer.rect),
            "Inspection card leaves terrain and operations visible"): return false
    await click(hover.rect)
    if not await inspect_page("Cursor"): return false
    if not expect(state().inspection.cursor == enemy.Position,"Card consumes clicks over the board"): return false
    await act("cancel")
    return await settled() and expect(signature(state()) == baseline,"Free inspection preserves battle state")

func host_cover() -> bool:
    var before := resources(state())
    var page: String = state().menu.page
    await act("menu")
    if not expect(host.find_child("SystemMenuView",true,false) != null,"Actual host Menu covers current F4 page"): return false
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    await act("cancel")
    return expect(state().menu.page == page and state().scene.phase == null and resources(state()) == before,"Host return retains page and cannot commit")

func defeated_vitals() -> bool:
    var deadline := Time.get_ticks_msec()+12000
    while Time.get_ticks_msec() < deadline and not ended:
        var scene: Dictionary = state().scene
        if scene.targetCard.visible and scene.targetCard.hp.current == 0:
            samples.append({"label":"defeated-card","scene":scene})
            if not card_geometry(scene.targetCard,"defeated enemy"): return false
            return expect(scene.error == null and (not scene.messagePanel.visible
                or disjoint(scene.targetCard.rect,scene.messagePanel.panel)),"Zero HP card clears the playback reader")
        if scene.requiresAcknowledgement: await act("confirm")
        else: await process_frame
    return expect(false,"The actual attack did not show its defeated enemy")

func decision_vitals() -> bool:
    if not await header_geometry("movement"): return false
    if not await inspection_vitals(): return false
    await act("confirm")
    if not await header_geometry("main"): return false
    if not await host_cover(): return false
    var baseline := resources(state())
    for i in range(posmod(1-state().menu.command,4)): await act("down")
    await act("confirm")
    if not expect(state().menu.page == "Magic","Actual Magic list opens"): return false
    if not await header_geometry("magic-list"): return false
    if not await host_cover(): return false
    var uses_magic := vitals_case in ["wide","zh-long"]
    if uses_magic: await act("confirm")
    else:
        await act("cancel"); await act("attack")
    if not expect(state().menu.page == "Target" and state().candidate != null,"Actual legal target page"): return false
    if not await wait_for(func(_s):return board().settled,"actual target camera"): return false
    if not await header_geometry("target"): return false
    if vitals_case == "zh-long" and not expect(state().menu.layout.targetCard.roleVisible,
            "Friendly target displays its class and level"): return false
    if not await host_cover(): return false
    var chosen: String = state().candidate
    var actor: Dictionary = state().actors.filter(func(a):return a.id == chosen)[0]
    var point := cell_point(actor.x,actor.y)
    await move_mouse(point)
    if not expect(state().candidate == chosen and state().scene.phase == null and resources(state()) == baseline,"Hover only previews actual target"): return false
    await mouse(point,true)
    if not expect(state().scene.phase == null,"Press alone does not execute"): return false
    var invalid := cell_point(state().previewX,state().previewY)
    if vitals_case == "zh-long":
        var enemy: Dictionary = state().actors.filter(func(a):return a.id == "enemy-0")[0]
        invalid = cell_point(enemy.x,enemy.y)
    await move_mouse(invalid); await mouse(invalid,false)
    await act("confirm")
    if not expect(state().scene.phase == null and resources(state()) == baseline,"Invalid release/confirmation cannot submit earlier target"): return false
    await act("right")
    if not expect(state().candidate != null,"Mapped pad/direction restores a legal target"): return false
    await act("cancel")
    if not expect(state().menu.page == ("Magic" if uses_magic else "Main") and resources(state()) == baseline,"Target Cancel returns freely"): return false
    if uses_magic: await act("confirm")
    else: await act("attack")
    for size_ in [Vector2i(1600,900),Vector2i(2560,1440)]:
        root.size = size_; await frames(5)
        if not await header_geometry("target-resize-"+str(size_)): return false
    var before_recovery := resources(state())
    root.size = Vector2i(1280,720); await frames(5)
    var recovery: Dictionary = JSON.parse_string(host.call("ReadWindowSupportObservationJson"))
    samples.append({"label":"below-window-floor","window":recovery})
    if not expect(not recovery.allowed and recovery.recovery != null and not state().menu.visible
            and resources(state()) == before_recovery,"Below the window floor, recovery covers battle input"): return false
    root.size = Vector2i(1920,1080); await frames(5)
    if not await header_geometry("target-after-recovery"): return false
    if not expect(resources(state()) == before_recovery,"Supported size restores target without acting"): return false
    if not await wait_for(func(_s):return board().settled,"resized target camera"): return false
    actor = state().actors.filter(func(a):return a.id == state().candidate)[0]
    point = cell_point(actor.x,actor.y)
    await move_mouse(point); await mouse(point,true); await mouse(point,false)
    var committed := state()
    samples.append({"label":"click-committed","scene":committed.scene,"stage":committed.stage})
    if not expect(committed.scene.phase != null and not committed.menu.visible,"Complete legal-target click immediately executes without another Confirm"): return false
    if not expect(not committed.scene.actorCard.decisionHeader and not committed.scene.targetCard.decisionHeader,
            "Close-up retains its existing non-decision cards"): return false
    await frames(5)
    var scene: Dictionary = state().scene
    samples.append({"label":"playback-cards","scene":scene})
    if not expect(scene.error == null,"Close-up has no adapter error") or not card_geometry(scene.actorCard,"playback primary"): return false
    if vitals_case == "zh-long": return expect(not scene.targetCard.visible,"Friendly healing uses one shared card")
    if not card_geometry(scene.targetCard,"playback enemy") or not expect(not scene.targetCard.roleVisible
            and disjoint(scene.actorCard.rect,scene.targetCard.rect),"Opposing cards fit with enemy disclosure"): return false
    return true if vitals_case == "wide" else await defeated_vitals()

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
