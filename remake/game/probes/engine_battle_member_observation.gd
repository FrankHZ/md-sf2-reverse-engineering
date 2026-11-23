extends "res://probes/engine_battle_inspection_observation.gd"

# Reuses ordinary mapped input and the existing post-undo signature; no gameplay setters.
var chinese := OS.get_environment("SF2_MEMBER_ZH") == "1"

func open_members() -> bool:
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var s := state()
    var empty := Vector2i(int(s.inspection.cursor.X), int(s.inspection.cursor.Y))
    while s.inspection.units.any(func(u): return u.Position.X == empty.x and u.Position.Y == empty.y):
        empty.x = (empty.x + 1) % int(s.mapWidth)
        if empty.x == 0: empty.y = (empty.y + 1) % int(s.mapHeight)
    if not await cursor_to(empty.x, empty.y): return false
    await act("confirm")
    if not await inspect_page("Options"): return false
    if not check(("地图" if chinese else "Map") in state().inspection.options.commandList.rows[1].label, "Map remains an offered option"): return false
    await act("up")
    if not check(state().inspection.optionIndex == 0, "Up chooses Member"): return false
    await send_action("confirm", true)
    await frames(10)
    if not await inspect_page("Members"): return false
    await send_action("confirm", false)
    return true

func select_member(actor: String) -> bool:
    var count: int = state().inspection.members.size()
    for n in range(count):
        var i: Dictionary = state().inspection
        if i.members[i.memberIndex].Actor.Value == actor: return true
        await act("down")
    return fail("Admitted member not reachable: " + actor)

func detail(actor: String, close: String) -> bool:
    if not await select_member(actor): return false
    var selected: int = state().inspection.memberIndex
    await act("attack")
    if not await inspect_page("MemberFull"): return false
    var s := sample("member-detail-" + actor + "-" + close)
    var unit: Dictionary = s.inspection.unit
    var live: Dictionary = s.actors.filter(func(a): return a.id == actor)[0]
    if not check(unit.Actor.Value == actor and unit.Hp == live.hp and unit.Mp == live.mp and unit.Attack == live.attack,
            "Shared detail reads selected live battle scalars"): return false
    if not check(s.inspection.full.stats.size() == 6 and s.inspection.full.items.entries.size() == 4
            and s.inspection.full.stats.any(func(row): return row.label == ("等级" if chinese else "LEVEL")),
            "Actual full-detail widgets show localized stats and loadout"): return false
    await act("target"); await act("down")
    await send_action(close, true); await frames(8)
    if not await inspect_page("Members"): return false
    if not check(state().inspection.memberIndex == selected, "Sheet returns to selected roster row, including active actor"): return false
    await send_action(close, false)
    return true

func roster_pointer(rect: Dictionary, button: MouseButton, pressed: bool) -> void:
    var event := InputEventMouseButton.new()
    event.position = Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2)
    event.global_position = event.position
    event.button_index = button
    event.pressed = pressed
    Input.parse_input_event(event)
    await frames(3)

func enter_roster() -> bool:
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var s := state()
    var empty := Vector2i(s.inspection.cursor.X, s.inspection.cursor.Y)
    while s.inspection.units.any(func(unit): return unit.Position.X == empty.x and unit.Position.Y == empty.y):
        empty.x = (empty.x + 1) % int(s.mapWidth)
        if empty.x == 0: empty.y = (empty.y + 1) % int(s.mapHeight)
    if not await cursor_to(empty.x, empty.y): return false
    await act("confirm")
    if not await inspect_page("Options"): return false
    await act("confirm")
    return await inspect_page("Members")

func authored_roster() -> bool:
    var baseline := signature(state())
    if not await enter_roster(): return false
    var inspection: Dictionary = sample("roster-open").inspection
    var roster: Dictionary = inspection.roster
    var rows: Array = roster.rows
    var large := case_name == "member-large"
    if not check(rows.size() == (14 if large else 2) and roster.scroll == 0 and rows[0].focused
            and rows.map(func(row): return row.actor) == inspection.members.map(func(member): return member.Actor.Value)
            and rows.all(func(row): return not row.portrait and row.available),
            "Shared rows preserve battle order, focus and optional portrait space"): return false
    if large:
        if not check(rows[0].lines > 1 and rows[0].name.length() >= 60
                and rows[1].condition == "Defeated" and rows[2].condition == "Absent",
                "Long identity wraps and retained conditions differ"): return false
        await act("confirm")
        if not await inspect_page("MemberFull"): return false
        var details := view.find_child("Details", true, false)
        var name_label := details.get_node("MemberName") as Label
        var class_label := details.get_node("Class") as Label
        if not check(name_label.text == rows[0].name and name_label.get_visible_line_count() == name_label.get_line_count()
                and name_label.get_global_rect().end.y <= class_label.get_global_rect().position.y,
                "The long name remains readable on the full sheet"): return false
        await act("cancel")
        if not await inspect_page("Members"): return false
        for index in [1, 2]:
            var rect: Dictionary = state().inspection.roster.rows[index].rect
            await roster_pointer(rect, MOUSE_BUTTON_LEFT, false)
            if not check(state().inspection.page == "Members", "Orphan release leaves roster open"): return false
            await roster_pointer(rect, MOUSE_BUTTON_LEFT, true)
            await roster_pointer(rect, MOUSE_BUTTON_LEFT, false)
            if not await inspect_page("MemberFull"): return false
            var selected: Dictionary = sample("retained-detail-" + str(index)).inspection
            if not check(selected.unit.Actor.Value == rows[index].actor and selected.unit.Position == null
                    and (selected.unit.Hp == 0) == (index == 1) and selected.full.items.entries.size() == 4
                    and not selected.full.has("preparation"),
                    "Defeated and absent members open ordinary read-only details"): return false
            await act("cancel")
            if not await inspect_page("Members"): return false
            if not check(state().inspection.roster.rows[index].focused, "Pointer selection survives Back"): return false
        await roster_pointer(roster.viewport, MOUSE_BUTTON_WHEEL_DOWN, true)
        await roster_pointer(roster.viewport, MOUSE_BUTTON_WHEEL_DOWN, false)
        if not check(state().inspection.roster.scroll > 0 and state().inspection.roster.rows[2].focused,
                "Wheel moves the viewport while preserving focus"): return false
        for index in range(3, rows.size()): await act("down")
    else:
        await act("up")
    inspection = sample("roster-last-focused").inspection
    roster = inspection.roster
    var last: Dictionary = roster.rows[-1]
    if not check(last.focused and last.rect.y >= roster.viewport.y and
            last.rect.y + last.rect.height <= roster.viewport.y + roster.viewport.height and
            (roster.scroll > 0) == large, "Navigation reveals the last row"): return false
    await act("confirm")
    if not await inspect_page("MemberFull"): return false
    if not check(state().inspection.unit.Actor.Value == last.actor, "Keyboard opens the focused member"): return false
    var back: Dictionary = state().inspection.footer.backRect
    await footer_mouse(back,MOUSE_BUTTON_LEFT,true)
    await act("cancel")
    if not await inspect_page("Members"): return false
    await roster_pointer(last.rect,MOUSE_BUTTON_LEFT,false)
    if not await inspect_page("Members"): return false
    var restored: Dictionary = sample("roster-detail-back").inspection.roster
    if not check(restored.scroll == roster.scroll and restored.rows[-1].actor == last.actor and restored.rows[-1].focused,
            "Back restores exact scroll and actor focus"): return false
    if not check(not board().movementRange.visible, "Member pages clear inspected movement"): return false
    await act("cancel"); await act("cancel")
    if not await inspect_page("Cursor"): return false
    await act("cancel")
    return check(not state().inspection.active and state().failure == null and state().scene.error == null
        and board().cursorRole == "Movement", "Back returns healthy ordinary Movement") and purity(baseline, "authored members")

func portrait_texture(image: TextureRect, label: String) -> Texture2D:
    if not check(is_instance_valid(image) and image.is_visible_in_tree()
            and is_instance_valid(image.texture) and image.texture.get_size() == Vector2(48, 64),
            label + ": visible, valid 48x64 texture"): return null
    samples.append({"label":label, "control_size":[image.size.x, image.size.y],
        "texture_size":[image.texture.get_width(), image.texture.get_height()]})
    return image.texture

func roster_portrait(index: int, label: String) -> Texture2D:
    var row := view.find_child("Roster", true, false).get_node("Members/Member" + str(index))
    var image: TextureRect
    for child in row.get_children():
        if child is TextureRect: image = child
    if not check(is_instance_valid(image) and image.size.x == 48 and image.size.y >= 64, label + ": portrait row geometry"):
        return null
    return portrait_texture(image, label)

func field_portrait(index: int, label: String) -> int:
    var party: Dictionary = state().partyStatus
    if not check(party.visible and party.index == index and party.rows[index].focused, label + ": selected field row"):
        return 0
    var row_texture := roster_portrait(index, label + "-row")
    if row_texture == null: return 0
    var summary := view.find_child("MemberPortrait", true, false) as TextureRect
    if not check(is_instance_valid(summary) and summary.size == Vector2(144, 192), label + ": summary portrait geometry"):
        return 0
    var detail_texture := portrait_texture(summary, label + "-summary")
    if not check(detail_texture == row_texture, label + ": summary borrows the selected row texture"):
        return 0
    return row_texture.get_instance_id()

func field_portraits() -> bool:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "settled field for Party"): return false
    await tap(KEY_E)
    var first := field_portrait(0, "field-first")
    if first == 0: return false
    await act("down")
    var second := field_portrait(1, "field-second")
    if not check(second != 0 and second != first, "Different member selects different art"): return false
    await act("confirm")
    if not check(state().partyStatus.preparation.visible, "Prepare opened on the selected member"): return false
    await act("cancel")
    if not check(field_portrait(1, "field-prepare-back") == second, "Prepare Back restores the live selected texture"): return false
    await act("cancel")
    if not check(not state().partyStatus.visible, "Field Party closes"): return false
    await tap(KEY_E)
    if not check(field_portrait(0, "field-reopen") == first, "Reopened field Party uses live first-row art"): return false
    await act("cancel")
    return check(not state().partyStatus.visible and state().failure == null, "Field Party closes cleanly again")

func battle_portraits() -> bool:
    if not await enter_battle(): return false
    var baseline := signature(state())
    if not await enter_roster(): return false
    var battle_first := roster_portrait(0, "battle-first")
    if battle_first == null: return false
    await act("down")
    var battle_second := roster_portrait(1, "battle-second")
    if not check(battle_second != null and battle_second != battle_first, "Battle selection has distinct admitted portraits"): return false
    var selected: Dictionary = state().inspection
    await act("confirm")
    if not await inspect_page("MemberFull"): return false
    if not check(state().inspection.unit.Actor.Value == selected.roster.rows[1].actor, "Battle detail selected the portrait row"): return false
    await act("cancel")
    if not await inspect_page("Members"): return false
    if not check(state().inspection.roster.rows[1].focused and roster_portrait(1, "battle-detail-back") == battle_second,
            "Battle detail Back restores focus and live art"): return false
    await act("cancel")
    if not await inspect_page("Options"): return false
    await act("confirm")
    if not await inspect_page("Members"): return false
    return check(roster_portrait(0, "battle-reopen") == battle_first and state().scene.error == null,
        "Reopened battle Member uses live first-row art") and purity(baseline, "portrait route")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    if case_name in ["member-field-portraits", "member-battle-portraits"]:
        if not await focus(): return
        var art_admission = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
        if not check(art_admission == "started", "Controlled art-present admission: " + str(art_admission)): return
        if case_name == "member-field-portraits":
            if await field_portraits(): finish(true)
        elif await battle_portraits(): finish(true)
        return
    if case_name in ["member-small", "member-large"]:
        if not await focus(): return
        var entry := host.find_child("SessionEntryView", true, false)
        if entry != null:
            fail("Authored roster admission: " + str(JSON.parse_string(entry.call("ReadEntryObservationJson")).failure))
            return
        if not await enter_battle(): return
        if await authored_roster(): finish(true)
        return
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Registered controlled Map57 approach: " + str(admitted)): return
    await frames(2)
    if not await enter_battle(): return
    var initial := state()
    var baseline := signature(initial)
    samples.append({"label":"initial-live-state", "signature":baseline})
    if not await open_members(): return
    var roster := sample("connected-member-roster")
    if not check(roster.inspection.members.size() >= 2 and roster.inspection.members.all(func(m): return m.Available),
            "Actual connected force list has admitted initialized members"): return
    if not check(roster.inspection.members.all(func(m): return roster.inspection.units.any(func(u): return u.Actor.Value == m.Actor.Value and u.Ally)),
            "Member contains allies only"): return
    if not check(("战斗成员" if chinese else "Battle members") in roster.inspection.roster.text,
            "Localized roster is actually projected"): return
    for action in ["up", "down", "attack", "confirm", "cancel"]:
        if not check(OS.get_keycode_string(events[action].keycode) in roster.inspection.hint + roster.inspection.footer.back, "Hints use installed binding: " + action): return
    await act("up")
    if not check(state().inspection.memberIndex == roster.inspection.members.size()-1, "Roster wraps upward"): return
    await act("down")
    if not check(state().inspection.memberIndex == 0, "Roster wraps downward"): return
    for close in ["cancel", "confirm", "attack"]:
        if not await detail(initial.actor, close): return
    var other: String = roster.inspection.members.filter(func(m): return m.Actor.Value != initial.actor)[0].Actor.Value
    if not await detail(other, "cancel"): return
    if not purity(baseline, "current/other sheets, selection, scrolling, held close"): return
    await act("menu"); await act("cancel")
    if not await inspect_page("Members"): return
    if not purity(baseline, "host pause and return to roster"): return
    await act("cancel")
    if not await inspect_page("Options"): return
    if not check(state().inspection.optionIndex == 0, "Member caller retains option selection"): return
    await act("left"); await act("confirm")
    if not await inspect_page("Map"): return
    await act("cancel")
    if not await inspect_page("Options"): return
    await act("cancel")
    if not await inspect_page("Cursor"): return
    await send_action("cancel", true); await frames(10)
    if not check(not state().inspection.active and state().stage == "Movement" and state().actor == initial.actor,
            "Final Back restores same actor Movement and held input does not reenter"): return
    await send_action("cancel", false)
    if not purity(baseline, "entire Options/Member/Map/cursor return"): return
    sample("same-actor-Movement")
    samples.append({"label":"final-live-state", "signature":signature(state())})
    # An actual replacement host must not inherit MemberFull or its selected actor.
    if not await open_members(): return
    await act("confirm")
    if not await inspect_page("MemberFull"): return
    root.remove_child(host); host.free(); await frames(2)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(4)
    admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    if not check(admitted == "started", "Replacement controlled admission"): return
    await frames(2)
    if not await enter_battle(): return
    if not check(state().sessionId != initial.sessionId and not state().inspection.active
            and state().inspection.members == null and state().inspection.full == null,
            "New session retires Member context and projection"): return
    sample("replacement-retired-member")
    finish(true)

func finish(completed := false) -> void:
    view = null
    if is_instance_valid(host): host.free()
    host = null
    super.finish(completed)
