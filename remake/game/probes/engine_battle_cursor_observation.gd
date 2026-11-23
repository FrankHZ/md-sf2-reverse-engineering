extends "res://probes/engine_demo_battle_ui_observation.gd"

# Reuses production input/admission and the bounded observer; never mutates a live session.
var original_cursor := OS.get_environment("SF2_CURSOR_ORIGINAL") == "1"
var expected_sha := OS.get_environment("SF2_CURSOR_SHA256")

func move_mouse(point: Vector2) -> void:
    var event := InputEventMouseMotion.new()
    event.position = root.get_stretch_transform() * point
    event.relative = Vector2(8,6)
    Input.parse_input_event(event)
    await frames(3)

func mouse(position: Vector2, pressed: bool) -> void:
    await super.mouse(root.get_stretch_transform() * position,pressed)

func cursor_art(art: Dictionary, cell: Dictionary, label: String) -> bool:
    if not expect(art.visible and art.draws > 0, label+": actual cursor draw"): return false
    if not expect(art.mode == ("original-single-cell" if original_cursor else "authored-fallback"),
            label+": selected original or explicit fallback"): return false
    if not original_cursor:
        var fallback: Dictionary = art.drawn
        var middle := cell_point(cell.X,cell.Y)
        return expect(fallback.width == 88 and fallback.height == 88 and
            Vector2(fallback.x+44,fallback.y+44).distance_to(middle) < 0.1,label+": authored pixel corners share the cell center")
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
    if not expect(b.cursorRole == "Target" and not b.movementRange.visible, label+": only target markers"): return false
    if not expect(s.menu.layout.targetCard.visible and not s.menu.layout.actorCard.visible, label+": only recipient card"): return false
    var at := {"X":s.previewX,"Y":s.previewY} if actor.id == s.actor else {"X":actor.x,"Y":actor.y}
    var card: Dictionary = s.menu.layout.targetCard
    var friendly: bool = card.level != null and card.roleVisible
    if not expect(friendly if label.begins_with("F4 HEAL") or label == "F4 friendly item"
        else card.level == null and not card.roleVisible, label+": friendly-only level"): return false
    return cursor_art(b.cellCursor,at,label)

func clear_marks(label: String) -> bool:
    var b := board()
    return expect(b.cursorRole == "None" and not b.cellCursor.visible and not b.movementRange.visible and
        not b.actionRange.visible,label+": no board decision marks")

func decision_hidden(label: String) -> bool:
    var s := state()
    return clear_marks(label) and expect(not s.menu.visible and not s.hudPanel.visible and
        s.inspection.hover == null,label+": no decision chrome")

func decision_geometry(label: String) -> bool:
    await frames(4)
    var s := state()
    var m: Dictionary = s.menu.layout
    var card: Dictionary = m.targetCard if m.targetCard.visible else m.actorCard
    var bounds: Dictionary = s.viewport
    var nav: Array = m.navigation.filter(func(n): return n.visible)
    var entry := host.find_child("SystemMenuEntry",true,false) as Control
    if entry != null:
        var menu_rect := entry.get_global_rect()
        var footer := Rect2(m.description.x,m.description.y,m.description.width,m.description.height)
        if not expect(not menu_rect.intersects(footer),label+": host chrome stays clear"): return false
    if not expect(s.failure == null and s.scene.error == null and s.menu.visible and m.descriptionVisible,
        label+": healthy visible decision"): return false
    if not expect(within(m.description,bounds) and within(m.reader.rect,m.description) and
        nav.all(func(n):return within(n.rect,m.description)),label+": clipped reader and fixed controls fit"): return false
    if card.visible and not expect(within(card.rect,bounds) and card.rect.x == m.description.x and
        card.rect.width == m.description.width and card.rect.y+card.rect.height <= m.description.y,
        label+": independent measured card and footer"): return false
    if card.visible:
        if demo_case == "public" and card.actor == s.actor and not expect(card.hp.maximum == 30000 and
            card.name.length() >= 55,label+": five-digit HP and long actor name"): return false
        if not expect(card.nameVisibleLines == card.nameLines and card.nameLines <= 2,label+": whole measured name"): return false
        for vital in [card.hp, card.mp]:
            if not expect(within(vital.valueRect,card.rect) and within(vital.image.rect,card.rect) and
                vital.valueMinimumWidth <= vital.valueRect.width+1, label+": full vital values inside card"): return false
    if m.commandList.visible:
        var commands: Array = m.commands
        var column: Dictionary = m.commandList.rect
        if not expect(commands.size() == 4 and commands.filter(func(row):return row.selected).size() == 1 and
                card.rect.y+card.rect.height <= column.y and column.y+column.height <= m.description.y,
                label+": four Main rows with one focus clear the card and reader"): return false
        var bottom: float = column.y
        for row in commands:
            if not expect(row.visible and not row.label.is_empty() and within(row.rect,bounds) and within(row.caption,row.rect) and
                    row.rect.x == column.x and row.rect.width == column.width and row.rect.y >= bottom,
                    label+": labeled row matches its full hit area"): return false
            bottom = row.rect.y+row.rect.height
    if m.listVisible and not expect(within(m.list,bounds) and m.list.y >= card.rect.y+card.rect.height and
        m.list.y+m.list.height <= m.description.y and not m.commandList.visible,label+": child list replaces Main in clear column"): return false
    if m.contextVisible and not expect(within(m.context,bounds),label+": round and gold fit"): return false
    samples.append({"label":label,"viewport":bounds,"menu":s.menu})
    return true

func movement_focus(label: String) -> bool:
    await frames(4)
    var s := state()
    var b := board()
    samples.append({"label":label,"cursor":b.cellCursor,"range":b.movementRange})
    var card: Dictionary = s.menu.layout.actorCard
    var expected: int = 5 if demo_case == "public" else 1
    if not expect(card.visible and card.roleVisible and card.level == expected and within(card.roleRect,card.rect),
        label+": current ally level fits Movement card"): return false
    return expect(b.cursorRole == "Movement" and b.movementRange.visible and not b.actionRange.visible,
        label+": sole movement cursor and field") and cursor_art(b.cellCursor,{"X":s.previewX,"Y":s.previewY},label)

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
        if not cursor_art(b.cellCursor,state().inspection.cursor,"F8 map edge"): return false
        if not expect(b.clip and not b.movementRange.visible and not b.actionRange.visible
                and b.cursorRole == "Inspection","Inspection clips art and retires decision overlays"): return false
    var unit: Dictionary = state().inspection.units[0]
    if not await cursor_to(unit.Position.X,unit.Position.Y): return false
    if not await wait_for(func(_s):return board().settled,"Unit hover camera settles"): return false
    await move_mouse(cell_point(unit.Position.X,unit.Position.Y))
    if not expect(state().inspection.cursor == unit.Position,"F8 unit hover retains live cell"): return false
    if not cursor_art(board().cellCursor,unit.Position,"F8 mouse hover"): return false
    await act("confirm")
    if not await inspect_page("Full"): return false
    if not clear_marks("Details"): return false
    var saved: Dictionary = state().inspection.cursor.duplicate()
    await move_mouse(Vector2(960,540))
    if not expect(state().inspection.cursor == saved,"Detail overlay consumes hover without moving board cursor"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    if not cursor_art(board().cellCursor,saved,"F8 detail return"): return false
    await act("menu")
    if not decision_hidden("Inspection host cover"): return false
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    await act("cancel")
    if not expect(state().inspection.page == "Cursor" and state().inspection.cursor == saved
            and signature(state()) == baseline,"Host cover/Back preserves inspection and gameplay"): return false
    await act("down")
    if state().inspection.hover.unit != null: await act("down")
    if not expect(state().inspection.hover.unit == null,"Inspection finds empty ground"): return false
    await act("confirm")
    if not await inspect_page("Options") or not clear_marks("Options"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    await act("cancel")
    if not await settled(): return false
    return await movement_focus("Inspection Back")

func settle_scene(previous: String) -> bool:
    await frames(3)
    if not decision_hidden("Actual playback"): return false
    return await super.settle_scene(previous)

func paused_resize() -> bool:
    if not await wait_for(func(s):return s.scene.requiresAcknowledgement and s.scene.messagePanel.visible,
        "Actual close-up message awaits input"): return false
    var cards: Dictionary = state().scene
    if not expect(cards.actorCard.visible and cards.actorCard.roleVisible and cards.actorCard.level == 1 and
        within(cards.actorCard.roleRect,cards.actorCard.rect) and cards.targetCard.visible and
        cards.targetCard.level == null and not cards.targetCard.roleVisible, "Mounted scene shows only the ally level"): return false
    await act("menu")
    if not expect(host.find_child("SystemMenuView",true,false) != null,"Host covers actual close-up"): return false
    var before: Dictionary = state().scene
    var resume_size := root.size
    root.size = Vector2i(2560,1440)
    await frames(8)
    var after: Dictionary = state().scene
    for field in ["waitToken","displayedAlly","displayedEnemy","phase","visibleCharacters","elapsed","message"]:
        if not expect(before[field] == after[field],"Paused resize preserves "+field): return false
    if not decision_hidden("Paused close-up"): return false
    if not expect(before.canvas != after.canvas and before.messagePanel.panel != after.messagePanel.panel and
        before.actorCard.rect != after.actorCard.rect, "Paused close-up uses refreshed canvas, card and reader bounds"): return false
    samples.append({"label":"covered-resize","before":before,"after":after})
    root.size = resume_size
    await frames(5)
    await act("cancel")
    return expect(state().scene.waitToken == before.waitToken and state().scene.phase == before.phase,
        "Host Back resumes the same pending close-up")


func cursor_decision() -> bool:
    var initial := state()
    if not await movement_focus("Initial movement"): return false
    var range: Dictionary = board().movementRange
    var ally: Dictionary = initial.actors.filter(func(a):return a.id.begins_with("ally-") and a.id != initial.actor and a.hp > 0)[0]
    var crossing := {"X":ally.x,"Y":ally.y}
    if not expect(range.reachableCells.any(func(p):return p.X == crossing.X and p.Y == crossing.Y) and
        not range.destinations.any(func(p):return p.X == crossing.X and p.Y == crossing.Y),
        "Occupied ally is traversable but has no stop fill"): return false
    await send_action("left",true)
    if not await wait_for(func(s):return s.movement != null,"Real player preview starts"): return false
    if not expect(board().cursorRole == "Movement" and board().cellCursor.visible,"Cursor stays with the delivered step"): return false
    await send_action("left",false)
    if not await settled() or not await movement_focus("After preview step"): return false
    if not expect(state().previewX == initial.previewX-1,"Preview delivered to a legal stop"): return false
    await act("confirm")
    await act("cancel")
    if not await settled() or not await movement_focus("After Main Back undo"): return false
    if not expect(state().previewX == initial.previewX,"Undo returns to the original cell"): return false
    await act("confirm")
    if not clear_marks("Main") or not await decision_geometry("Main at 1080p"): return false
    for i in range(posmod(1-state().menu.command,4)): await act("down")
    if not expect(state().menu.command == 1,"Row navigation focuses Magic without choosing it"): return false
    var magic_caption: Dictionary = state().menu.layout.commands[1].caption
    await click(magic_caption)
    if not expect(state().menu.page == "Magic","Complete caption click chooses visible row"): return false
    if not clear_marks("Magic reader") or not await decision_geometry("Magic reader"): return false
    await act("confirm")
    if not await target_art("F4 HEAL"): return false
    var heal_before := resources(state())
    await act("right")
    if not await target_art("F4 HEAL target cycle"): return false
    await act("cancel")
    if not expect(state().menu.page == "Magic" and not board().cellCursor.visible and resources(state()) == heal_before,
        "HEAL Back restores level without payment"): return false
    await act("cancel")
    await act("attack")
    if not await target_art("F4 physical"): return false
    var selected: Dictionary = state().actors.filter(func(a):return a.id == state().candidate)[0]
    await move_mouse(cell_point(selected.x,selected.y))
    if not await target_art("F4 mouse hover"): return false
    var baseline := resources(state())
    var wrong := cell_point(initial.previewX,initial.previewY)
    await move_mouse(wrong); await mouse(wrong,true); await mouse(wrong,false); await act("confirm")
    if not expect(state().candidate == null and not board().cellCursor.visible and board().actionRange.effect.is_empty()
            and board().actionRange.visible and not state().menu.layout.targetCard.visible and not state().menu.layout.actorCard.visible
            and not state().menu.layout.navigation.filter(func(v):return v.id=="Confirm")[0].enabled
            and resources(state()) == baseline,"Invalid click clears cursor and cannot commit stale target"): return false
    await act("right")
    if not await target_art("F4 direction restores eligible target"): return false
    for size in [Vector2i(3440,1440),Vector2i(1920,1080)]:
        root.size = size; await frames(5)
        if not await target_art("F4 resize") or not await decision_geometry("Target at "+str(size)): return false
    await act("cancel")
    if not clear_marks("Physical Target Back"): return false
    await act("item"); await act("down"); await act("confirm")
    if not await target_art("F4 friendly item"): return false
    await act("cancel"); await act("cancel"); await act("cancel")
    if not await settled(): return false
    if not await cursor_inspection(): return false
    await act("confirm"); await act("attack")
    if not await target_art("F4 physical after F8"): return false
    if not await wait_for(func(_s):return board().settled,"Camera settles before target click"): return false
    var foe: Dictionary = state().actors.filter(func(a):return a.id == state().candidate)[0]
    var point := cell_point(foe.x,foe.y)
    await move_mouse(point); await mouse(point,true)
    if not expect(state().scene.phase == null,"Target press only previews"): return false
    await mouse(point,false)
    if not await paused_resize() or not await settle_scene(initial.actor): return false
    return true

func read_wheel(reader: Dictionary) -> void:
    var point := Vector2(reader.rect.x+reader.rect.width/2,reader.rect.y+reader.rect.height/2)
    var wheel := InputEventMouseButton.new()
    wheel.button_index = MOUSE_BUTTON_WHEEL_DOWN; wheel.position = root.get_stretch_transform() * point; wheel.pressed = true
    Input.parse_input_event(wheel); await frames(4)

func public_cursor() -> bool:
    if not await movement_focus("Public authored movement fallback"): return false
    await act("confirm")
    if not await decision_geometry("Public Main"): return false
    while state().menu.command != 2: await act("down")
    var main := state()
    await act("right")
    if not expect(main.menu.layout.reader.hint != null and state().menu.layout.reader.scroll > 0 and
        state().menu.command == main.menu.command and signature(state()) == signature(main),
        "Mapped Right reads overflowing Main without changing command or state"): return false

    await act("spell")
    if not clear_marks("Public Magic") or not await decision_geometry("Public Magic"): return false
    var list := state()
    var reader: Dictionary = list.menu.layout.reader
    if not expect(reader.maximum > 0 and reader.hint != null and within(reader.hintRect,list.menu.layout.description),
        "Wrapped Magic shows the active reading hint below the body"): return false
    await act("right")
    var keyboard: Dictionary = state().menu.layout.reader
    if not expect(keyboard.scroll > 0,"Keyboard reads Magic"): return false
    var key: InputEvent = events.left
    events.left = InputMap.action_get_events("sf2_left").filter(func(e): return e is InputEventJoypadButton)[0]
    await act("left"); events.left = key
    var pad: Dictionary = state().menu.layout.reader
    if not expect(pad.scroll < keyboard.scroll and pad.hint != keyboard.hint, "Mapped D-pad reads Magic and updates its hint"): return false
    await read_wheel(reader)
    var wheel: Dictionary = state().menu.layout.reader
    if not expect(wheel.scroll > pad.scroll and state().menu.row == list.menu.row and signature(state()) == signature(list),
        "Wheel and mapped reads preserve row, revision, submissions, resources and RNG"): return false
    await send_action("right",true)
    await act("menu")
    var held_at_cover: int = state().menu.layout.reader.scroll
    await send_action("right",false)
    await frames(3)
    await send_action("right",true)
    await act("cancel")
    await create_timer(0.25).timeout
    if not expect(state().menu.visible and state().menu.row == list.menu.row and state().menu.layout.reader.scroll == held_at_cover and
        signature(state()) == signature(list), "Host return preserves reading and focus while held direction stays inert"): return false
    await send_action("right",false)
    await act("down")
    var changed := state()
    samples.append({"label":"mapped-and-wheel-reading","keyboard":keyboard,"pad":pad,"wheel":wheel,
        "resumedScroll":held_at_cover,"initialRow":list.menu.row,"changedRow":changed.menu.row,
        "afterRow":changed.menu.layout.reader})
    if not expect(changed.menu.row != list.menu.row and changed.menu.layout.reader.scroll == 0 and
        changed.menu.layout.reader.rect == reader.rect, "Changing row resets scroll within the same reader geometry"): return false
    await act("confirm")
    if not await target_art("Public authored multi-target fallback") or not await decision_geometry("Public Target"): return false
    var m: Dictionary = state().menu.layout
    if not expect(m.targetCard.rect.x == 32 and m.description.x == 32 and board().cellCursor.rect.x > 1270,
        "Right-edge recipient moves the card and footer to the clear left column"): return false
    if not expect(board().actionRange.effect.size() > 1 and board().actionRange.steady and m.reader.maximum == 0 and
        m.reader.hint == null, "Target keeps the quiet range and concise non-overflowing footer"): return false
    var target := state()
    await read_wheel(m.reader)
    if not expect(state().candidate == target.candidate and signature(state()) == signature(target),
        "Wheel over Target footer is inert"): return false
    await act("cancel"); await act("cancel"); await act("cancel")
    if not await settled(): return false
    return await cursor_inspection()

func run() -> void:
    root.size = Vector2i(1600,900) if demo_case == "public" else Vector2i(1920,1080)
    if demo_case == "public":
        var chinese := OS.get_environment("SF2_CURSOR_LANGUAGE") != "en"
        var catalog: Translation = load("res://localization/zh_CN.po" if chinese else "res://localization/en.po")
        var long_text := "在确认前请查看所选行动的说明。" if chinese else "Read the selected action description before confirming. "
        for key in ["battle.range", "battle.no-items"]:
            catalog.add_message(key, str(catalog.get_message(key)) + "\n" + long_text.repeat(24))
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(5); bind_events()
    for action in ["spell","item","stay"]:
        events[action] = InputMap.action_get_events("sf2_"+action).filter(func(e):return e is InputEventKey)[0]
    var start := OS.get_environment("SF2_COMPACT_START")
    if not start.is_empty():
        var admitted = host.call("StartPreVoyageLocalizationObservation",start)
        if not expect(admitted == "started","Ordinary full-world admission: "+str(admitted)): return
    var entry := host.find_children("*","",true,false).filter(func(node):return node.has_method("ReadEntryObservationJson"))
    if start.is_empty() and not entry.is_empty():
        var refusal: Dictionary = JSON.parse_string(entry[0].call("ReadEntryObservationJson"))
        fail("Authored admission: "+str(refusal.get("failure"))+" "+str(refusal.get("error"))); return
    if not await enter_battle(): return
    if demo_case in ["area","aura"]:
        finish(await area_case()); return
    if not await wait_for(func(_s):return board().settled,"Initial camera settles"): return
    if OS.get_environment("SF2_CURSOR_CLOSEUP_ONLY") == "1":
        var previous: String = state().actor
        await act("confirm")
        await act("spell")
        await act("confirm")
        if not await target_art("F4 HEAL") or not await decision_geometry("Friendly Target"): return
        var card: Dictionary = state().menu.layout.targetCard
        if not expect(card.actor == previous and card.level == 1 and within(card.roleRect,card.rect),
            "Friendly Target displays the live self level inside the card"): return
        await act("cancel")
        await act("cancel")
        await act("attack")
        await act("confirm")
        finish(await paused_resize() and await settle_scene(previous))
        return
    finish(await public_cursor() if demo_case == "public" else await cursor_decision())
