extends "res://probes/engine_battle_inspection_observation.gd"

# Bounded production input/geometry witness. The caller supplies only prelaunch controlled inputs.
var compact_case := OS.get_environment("SF2_COMPACT_CASE")
var checks := 0

func _initialize() -> void:
    create_timer(50).timeout.connect(func(): fail("50-second compact command deadline"))
    call_deferred("run")

func expect(ok: bool, message: String) -> bool:
    checks += 1
    return check(ok, message)

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label,"revision":s.revision,"failure":s.failure,"gold":s.get("gold"),
        "party":s.get("party"),"church":s.get("church"),"stage":s.get("stage"),"menu":s.get("menu"),
        "target":s.get("target"),"selection":s.get("selection"),"search":s.get("search"),
        "client":[root.size.x,root.size.y],"viewport":s.get("viewport")})
    return s

func resources(s: Dictionary) -> Array:
    return [s.gold,s.mainSeed,s.thinkingSeed,s.queueCursor,
        s.actors.map(func(a):return [a.id,a.hp,a.mp,a.exp,a.kills,a.defeats,a.status,a.items,a.spells])]

func inside(r: Dictionary) -> bool:
    return r.x >= 0 and r.y >= 0 and r.x+r.width <= root.size.x+1 and r.y+r.height <= root.size.y+1

func list_geometry(list: Dictionary, mounted: bool) -> bool:
    if not expect(list.visible and list.rows.size() == 4, "Four labeled production command rows"): return false
    for i in range(4):
        var row: Dictionary = list.rows[i]
        if not expect(inside(row.rect) and row.rect.width == 528 and row.rect.height >= 80,
                "Full stable command hit area " + str(i)): return false
        if not expect(row.minimumHeight <= row.labelRect.height+1 and not row.label.is_empty(), "Full readable label " + str(i)): return false
        if i > 0 and not expect(row.rect.y >= list.rows[i-1].rect.y+list.rows[i-1].rect.height+8, "Ordered disjoint rows"): return false
        if not expect(row.art.scale == 3 and row.art.filter == "Nearest" and row.art.mounted == mounted
                and not row.art.labelVisible, "3x source icon or explicit text-only row"): return false
    return true

func mouse(position: Vector2, pressed: bool) -> void:
    var event := InputEventMouseButton.new()
    event.button_index = MOUSE_BUTTON_LEFT; event.position = position; event.pressed = pressed
    Input.parse_input_event(event); await frames(2)

func click(rect: Dictionary) -> void:
    var point := Vector2(rect.x+rect.width/2, rect.y+rect.height/2)
    var move := InputEventMouseMotion.new(); move.position = point; move.relative = Vector2(4,3)
    Input.parse_input_event(move); await frames(2)
    await mouse(point,true); await mouse(point,false); await frames(3)

func church_open() -> bool:
    if state().get("wait") == "ChurchWait": return true
    await act("confirm")
    for i in range(180):
        if state().get("wait") == "ChurchWait": return true
        if state().get("canWaitForText",false): await act("confirm")
        else: await process_frame
    return expect(false,"Real priest caller opens Church")

func church_ready() -> bool:
    for i in range(240):
        if state().get("canWaitAtInput",false): return true
        if state().get("canWaitForText",false): await act("confirm")
        else: await process_frame
    return expect(false,"Real priest caller returns to field")

func church_case() -> bool:
    if not await church_open(): return false
    await frames(4)
    var opened := record("church-opened")
    var original: Dictionary = opened.church.commandList
    if not list_geometry(original,compact_case != "authored-church"): return false
    # Native drawing alternates the two admitted frames without moving any hit area.
    await act("down")
    var first_art: Dictionary = state().church.commandList.rows[1].art
    await create_timer(0.26).timeout
    var second_art: Dictionary = state().church.commandList.rows[1].art
    if compact_case != "authored-church" and not expect(first_art.resource == second_art.resource if first_art.reducedFlash else first_art.resource != second_art.resource,
            "Original selected frames honor motion preference"): return false
    await act("up")
    var before: Array = [opened.party,opened.gold,opened.revision]
    await act("down"); await act("down")
    var refused := record("promotion-focus")
    if not expect(refused.church.index == 2 and not refused.church.commandList.rows[2].enabled,
            "Axis navigation inspects unavailable Promotion"): return false
    if not expect([refused.party,refused.gold,refused.revision] == before,"Focus does not transact"): return false
    await act("confirm")
    refused = record("promotion-refused")
    if not expect(refused.church.error != null and [refused.party,refused.gold,refused.revision] == before,
            "Unavailable confirmation explains refusal without state change"): return false
    if not expect(original.rows.map(func(r):return r.rect) == refused.church.commandList.rows.map(func(r):return r.rect),
            "Refusal keeps service hit geometry fixed"): return false
    for window in [Vector2i(2560,1440),Vector2i(1920,1080)]:
        root.size = window; await frames(6)
        var s := record("church-resize-"+str(window))
        if not list_geometry(s.church.commandList,compact_case != "authored-church"): return false
        if not expect(inside(s.church.panelRect) and inside(s.church.hintRect),"Service reason and mapped hints fit window"): return false
    # Press in one row and release in another cannot select either service.
    var rows: Array = state().church.commandList.rows
    await mouse(Vector2(rows[0].rect.x+200,rows[0].rect.y+40),true)
    await mouse(Vector2(rows[1].rect.x+200,rows[1].rect.y+40),false)
    if not expect(state().revision == opened.revision,"Mismatched pointer release cannot transact"): return false
    await click(state().church.commandList.rows[0 if compact_case == "authored-church" else 1].rect)
    var result := record("cure-selected-by-label")
    if not expect(result.church.page == ("offer" if compact_case == "authored-church" else "result"),
            "Full labeled row opens real eligible offer or no-applicable result"): return false
    if result.church.page == "offer":
        var quote: Dictionary = result.church.wait.Offer
        if not expect(quote.Cost > 0 and result.gold == opened.gold,"Quote retains exact real cost and no payment"): return false
        await act("cancel")
        var declined := record("quote-declined")
        if not expect(declined.church.page == "result" and declined.gold == opened.gold,"Cancel declines real quote"): return false
    await act("confirm")
    # Cure may progress to the next actual source-ordered condition after declining.
    for i in range(8):
        if state().church.page == "services": break
        await act("cancel")
    if not expect(state().church.page == "services" and state().church.index == (0 if compact_case == "authored-church" else 1),"Service return restores parent focus"): return false
    if compact_case == "authored-church":
        # A separate fresh selection pays the actual quote once, even while Confirm is held.
        await act("confirm")
        var quoted: Dictionary = state().church.wait.Offer
        await send_action("confirm",true); await frames(8)
        var paid := record("raise-paid-once")
        if not expect(paid.church.page == "result" and paid.gold == opened.gold-quoted.Cost,
                "Typed quote pays exactly its real cost"): return false
        await frames(8)
        if not expect(state().revision == paid.revision and state().gold == paid.gold,"Held Confirm cannot repay"): return false
        await send_action("confirm",false); await act("confirm")
    for i in range(5):
        if state().church.index == 3: break
        await act("down")
    var save := record("record-help")
    if not expect(save.church.index == 3 and inside(save.church.panelRect),"Record and complete save help fit"): return false
    await act("confirm")
    if not await church_ready(): return false
    await frames(4)
    var persistence: Dictionary = JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    var deadline := Time.get_ticks_msec() + 5000
    while persistence.panel == "entry-loading" and Time.get_ticks_msec() < deadline:
        await process_frame
        persistence = JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    samples.append({"label":"church-record-host", "persistence":persistence})
    if not expect(persistence.panel in ["save-slots","error"] and state().wait != "ChurchWait",
            "Church completes caller before shared Save host"): return false
    await act("cancel"); await frames(4)
    if not expect(state().canWaitAtInput and not state().church.visible,"Save Back returns field without consumed Church wait"): return false
    return true

func battle_case() -> bool:
    if not await enter_battle(): return false
    var authored := compact_case == "authored-battle"
    # The private approach reaches a learned-spell actor by ordinary Stay turns.
    for turn in range(0 if authored else 6):
        if state().actor == "ally-1": break
        var previous: String = state().actor
        await act("confirm")
        var stay: InputEvent = InputMap.action_get_events("sf2_stay").filter(func(e):return e is InputEventKey)[0]
        for pressed in [true,false]:
            var e: InputEvent = stay.duplicate(); e.pressed = pressed; Input.parse_input_event(e); await frames(2)
        for frame in range(600):
            var current := state()
            if current.get("stage") == "Movement" and current.get("movement") == null and current.actor != previous: break
            if current.get("scene",{}).get("requiresAcknowledgement",false): await act("confirm")
            else: await process_frame
    if not expect(state().actor == ("champion" if authored else "ally-1"), "Learned-spell actor reaches ordinary control"): return false
    await act("confirm"); await frames(4)
    var opened := record("battle-actions")
    if route == "remapped" and not expect("U/" in opened.menu.layout.help and "/J" in opened.menu.layout.help and "Q" in opened.menu.layout.help,
            "Battle hints use installed custom bindings"): return false
    if not expect(opened.stage == "ActionChoice","Real action boundary"): return false
    for window in [Vector2i(1600,900),Vector2i(1920,1080)]:
        root.size = window; await frames(6)
        var s := record("battle-resize-"+str(window))
        var layout: Dictionary = s.menu.layout
        var rows: Array = layout.commands
        var frame: Dictionary = layout.commandList.rect
        var content: Dictionary = layout.commandList.content
        var stretch := root.get_stretch_transform()
        var fits := func(rect: Dictionary) -> bool:
            var top_left: Vector2 = stretch * Vector2(rect.x,rect.y)
            var bottom_right: Vector2 = stretch * Vector2(rect.x+rect.width,rect.y+rect.height)
            return top_left.x >= 0 and top_left.y >= 0 and bottom_right.x <= window.x+1 and bottom_right.y <= window.y+1
        if not expect(root.size == window,"Actual client reached the requested size"): return false
        if not expect(layout.commandList.visible and rows.size() == 4,"Four visible Main rows"): return false
        var terrain: Dictionary = layout.terrainPanel.rect
        if not expect(fits.call(frame) and frame.x == layout.actor.x and frame.width == layout.actor.width and
                frame.x == layout.description.x and frame.width == layout.description.width and
                layout.terrainPanel.visible and terrain.y == layout.actor.y + layout.actor.height + 12 and
                frame.y >= terrain.y + terrain.height + 12 and frame.y + frame.height + 12 <= layout.description.y,
                "Main frame aligns with the card and reader and clears terrain and reader"): return false
        if not expect(content.x == frame.x + 16 and content.y == frame.y + 16 and
                content.width == frame.width - 32 and content.height == frame.height - 32,
                "Native panel pads the command list on every side"): return false
        var labels: Array = rows.map(func(row): return row.label)
        if not expect(labels in [["Attack","Magic","Item","Stay"],["攻击","魔法","道具","待机"]],
                "Localized command captions are always visible in source command order"): return false
        var row_top: float = content.y
        for row in rows:
            if not expect(row.visible and fits.call(row.rect) and fits.call(row.caption) and
                    row.rect.x == content.x and row.rect.width == content.width and row.rect.y >= row_top and
                    row.caption.y >= row.rect.y and row.caption.y+row.caption.height <= row.rect.y+row.rect.height,
                    "Full hit row and caption fit without intersecting the card or another row"): return false
            if not expect(row.art.mounted != authored and row.art.filter == "Nearest" and
                    (authored or (row.art.width == 24 and row.art.height == 24 and row.art.scale == 3)),
                    "Main reuses admitted command art or its authored fallback"): return false
            row_top = row.rect.y+row.rect.height
        if not expect(fits.call(layout.actor) and fits.call(layout.description) and
                row_top == content.y + content.height and layout.reader.rect.height > 0,
                "All rows clear the measured acting card and the scrollable footer"): return false
        if not expect(layout.navigation.all(func(control): return control.id != "Back"),
                "Main lists actions and leaves cancellation to input"): return false
    var initial_focus: int = opened.menu.command
    if not expect(initial_focus == (0 if opened.menu.layout.commands[0].enabled else 3),
            "Initial focus follows the live Attack offer"): return false
    for direction in ["down","up"]:
        var step := 1 if direction == "down" else -1
        for i in range(1,5):
            await act(direction)
            var focused := record("battle-main-"+direction+"-"+str(i))
            var expected := posmod(initial_focus+i*step,4)
            if not expect(focused.menu.page == "Main" and focused.menu.command == expected and
                    focused.menu.layout.commands[expected].selected and
                    focused.menu.layout.commands.filter(func(row): return row.selected).size() == 1 and
                    focused.menu.layout.description == opened.menu.layout.description and focused.revision == opened.revision,
                    "Up/Down wraps one row at a time without moving the footer or submitting"): return false
    await act("left"); await act("right")
    if not expect(state().menu.command == initial_focus,"Horizontal directions leave vertical selection in place"): return false
    var baseline := resources(opened)
    var guide := InputEventJoypadButton.new(); guide.device = 3; guide.button_index = JOY_BUTTON_GUIDE
    for pressed in [true,false]:
        guide.pressed = pressed; Input.parse_input_event(guide); await frames(3)
    var pad_state := state()
    if not expect((pad_state.menu.layout.help != opened.menu.layout.help or route == "pad") and
            pad_state.menu.command == opened.menu.command and resources(pad_state) == baseline and
            pad_state.actor == opened.actor,"Inert controller preserves selection and gameplay"): return false
    var motion := InputEventMouseMotion.new(); motion.position = Vector2(80,80); motion.relative = Vector2(15,0)
    Input.parse_input_event(motion); await frames(3)
    if not expect((state().menu.layout.help == opened.menu.layout.help or route == "pad") and resources(state()) == baseline,
            "Mouse restores keyboard/mouse cues without changing gameplay"): return false
    var first: Dictionary = state().menu.layout.commands[0].rect
    var second: Dictionary = state().menu.layout.commands[1].rect
    var frame: Dictionary = state().menu.layout.commandList.rect
    var border := Vector2(frame.x+3, frame.y+3)
    var gap := Vector2(first.x+48, (first.y+first.height+second.y)/2)
    var inside_row := Vector2(second.x+48,second.y+48)
    await mouse(border,true); await mouse(border,false)
    await mouse(gap,true); await mouse(inside_row,false)
    await mouse(inside_row,true); await mouse(gap,false)
    if not expect(state().menu.page == "Main" and state().menu.command == initial_focus and state().revision == opened.revision,
            "Frame border and row gaps never activate or retain a command press"): return false
    await mouse(Vector2(first.x+48,first.y+48),true)
    root.size = Vector2i(2560,1440); await frames(4)
    root.size = Vector2i(1920,1080); await frames(4)
    await mouse(Vector2(first.x+48,first.y+48),false)
    if not expect(state().menu.page == "Main" and state().revision == opened.revision,
            "Relayout cancels incomplete command click"): return false
    if opened.menu.layout.commands[0].enabled:
        await click(state().menu.layout.commands[0].caption)
        if not expect(state().menu.page == "Target" and not state().menu.layout.commandList.visible,
                "Attack caption opens the legal targets and hides the Main frame"): return false
        await act("cancel")
        if not expect(state().menu.page == "Main" and state().menu.command == 0,"Attack Target Back keeps the parent row"): return false
    await click(state().menu.layout.commands[1].rect)
    var magic := record("magic-child")
    if not expect(magic.menu.page == "Magic" and not magic.menu.layout.commandList.visible,"Child replaces Main rows"): return false
    if not expect(abs(magic.menu.layout.list.x-opened.menu.layout.commandList.rect.x) < 1
            and abs(magic.menu.layout.list.y-opened.menu.layout.commandList.rect.y) < 1,"Child list keeps parent origin"): return false
    if not expect(magic.menu.layout.rows.size() > 0,"Learned spells replace rows"): return false
    if magic.menu.layout.rows.size() > 0:
        if authored: await act("down")
        var action: int = state().menu.row
        var spell: String = state().menu.layout.rows[action]
        await act("confirm"); await frames(3)
        var target := record("magic-target-or-refusal")
        if not expect(target.menu.page == "Target", "Enabled actual healing offer reaches target page"): return false
        if target.menu.page == "Target":
            if not expect(target.menu.layout.targetCard.visible and not target.menu.layout.actorCard.visible
                    and target.menu.layout.targetCard.actor == target.target,"Recipient card replaces acting card during targeting"): return false
            await act("right"); await act("left")
            await act("cancel"); await frames(3)
            if not expect(state().menu.page == "Magic" and state().menu.row == action and
                    state().menu.layout.rows[action] == spell,"Target Back restores the same spell and level"): return false
    await act("cancel")
    if not expect(state().menu.page == "Main" and state().menu.command == 1,"Child Back restores Magic command"): return false
    var item: Dictionary = state().menu.layout.commands[2]
    var hit := Vector2(item.rect.x+item.rect.width-4,item.rect.y+item.rect.height/2)
    var item_motion := InputEventMouseMotion.new(); item_motion.position = hit; item_motion.relative = Vector2(4,3)
    Input.parse_input_event(item_motion); await frames(3)
    if not expect(state().menu.page == "Main" and state().menu.command == 2 and state().menu.layout.commands[2].surfaceColor == "204c76",
            "Hover focuses and highlights the entire Item row"): return false
    await mouse(hit,true)
    if not expect(state().menu.page == "Main" and state().menu.layout.commands[2].surfaceColor == "354c64",
            "Press highlights the captured row without opening a child"): return false
    await mouse(Vector2(item.caption.x+8,hit.y),false)
    if not expect(state().menu.page == "Items" and not state().menu.layout.commandList.visible,
            "Full click from row edge to caption opens Item and hides the Main frame"): return false
    if authored:
        await act("down")
        var item_row: int = state().menu.row
        var item_label: String = state().menu.layout.rows[item_row]
        await act("confirm")
        if not expect(state().menu.page == "Target","An enabled Item row opens Target"): return false
        await act("cancel")
        if not expect(state().menu.page == "Items" and state().menu.row == item_row and
                state().menu.layout.rows[item_row] == item_label,"Target Back restores the same inventory slot"): return false
    await act("cancel")
    if not expect(state().menu.page == "Main" and state().menu.command == 2,"Item Back retains parent"): return false
    await act("down")
    var focused := record("stay-focused")
    if not expect(focused.menu.page == "Main" and focused.menu.command == 3,"Stay remains focused before choice"): return false
    if not expect(resources(state()) == baseline,"All provisional pages preserve resources and both RNG seeds"): return false
    await act("cancel")
    if not await settled(): return false
    if not expect(not state().inspection.active,"Root Back returns only to Movement"): return false
    record("battle-returned-movement")
    await act("confirm")
    await click(state().menu.layout.commands[3].rect)
    if not expect(state().stage != "ActionChoice" and state().revision > opened.revision,
            "One complete click on Stay advances the turn"): return false
    record("battle-stay-clicked")
    return true

func search_case() -> bool:
    if not await enter_battle(): return false
    await act("right")
    if not await settled(): return false
    await act("confirm")
    if not expect(state().battleChoices.Search == null,"Unfaced cell retains real Stay offer"): return false
    await act("cancel")
    if not await settled(): return false
    await act("right")
    if not await settled(): return false
    await act("down")
    if not await settled(): return false
    await act("confirm"); await frames(3)
    var before := record("faced-chest-search")
    var row: Dictionary = before.menu.layout.commands[3]
    if not expect(before.battleChoices.Search.Flag == 701 and row.enabled and row.label in ["Search","调查"]
            and row.art.resource == null,"Real faced chest substitutes labeled text Search without borrowing Stay art"): return false
    for i in range(4):
        if state().menu.command == 3: break
        await act("down")
    await act("confirm")
    if not expect(state().stage == "SearchCommitReady","Search keeps explicit confirmation"): return false
    await act("cancel")
    if not expect(state().stage == "ActionChoice" and state().menu.command == 3
            and resources(state()) == resources(before),"Search Back preserves preview and resources"): return false
    await act("confirm"); await act("confirm")
    var result := record("actual-search-result")
    return expect(result.search.active and result.search.visible and result.search.kind == "Item",
            "Normal Search delivers its actual result through production selection")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(5); bind_events()
    if not compact_case.begins_with("authored-"):
        var admitted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COMPACT_START"))
        if not expect(admitted == "started","Full-world controlled admission before ordinary input: "+str(admitted)): return
        await frames(6)
    var completed := await search_case() if compact_case == "authored-search" else await church_case() if compact_case.ends_with("church") else await battle_case()
    finish(completed)

func finish(completed := false) -> void:
    if ended: return
    samples.append({"label":"check-count","checks":checks})
    if is_instance_valid(host): host.free()
    view = null; host = null
    super.finish(completed)
