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
    if not check(("地图" if chinese else "Map") in state().inspection.hint, "Map remains an offered option"): return false
    await act("down")
    if not check(state().inspection.optionIndex == 1, "Directions choose Member"): return false
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

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
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
        if not check(OS.get_keycode_string(events[action].keycode) in roster.inspection.hint, "Hints use installed binding: " + action): return
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
    if not check(state().inspection.optionIndex == 1, "Member caller retains option selection"): return
    await act("up"); await act("confirm")
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
