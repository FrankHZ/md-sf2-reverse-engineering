extends "res://probes/engine_battle_return_status_observation.gd"

# Finite actual-host input/projection witness. No session setters or screenshots.
func record(label: String) -> Dictionary:
    var s := state()
    var compact := {}
    for key in ["revision", "mode", "failure", "party", "gold", "mainSeed", "thinkingSeed", "flags", "entities", "partyStatus", "canWaitAtInput", "returnNotice"]:
        if s.has(key): compact[key] = s[key]
    samples.append({"label":label,"state":compact})
    return s

func run() -> void:
    if case_name == "victory":
        await super.run()
        return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(8)
    if not await focus(): return
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "settled field"): return
    await inspect_party()

func party_check() -> void:
    if not check(events.filter(func(e): return e.Kind == "outcome-program-started").size() == 1 and events.filter(func(e): return e.Kind == "battle-returned").size() == 1, "Real outcome/return completed once"): return
    samples.append({"label":"return-events","events":events})
    await inspect_party()

func resources(s: Dictionary) -> Array:
    return [s.revision,s.party,s.gold,s.get("mainSeed"),s.get("thinkingSeed"),s.flags]

func inspect_party() -> void:
    var before := record("before-open")
    if case_name == "victory":
        var leader: Dictionary = before.party.filter(func(a): return a.Actor.Value == "leader")[0]
        if not check(leader.Hp == 40 and leader.Exp == 48 and before.gold == 84, "Actual changed return resources: HP10 to40, EXP0 to48, gold73 to84"): return
    key_event(KEY_E,true)
    await frames(4)
    key_event(KEY_DOWN,true)
    await frames(3)
    if not check(state().partyStatus.index == 0 and resources(state()) == resources(before), "Held opening input cannot navigate or move"): return
    key_event(KEY_DOWN,false); key_event(KEY_E,false)
    await frames(5)
    var s := record("opened")
    if not check(s.partyStatus.visible and s.partyStatus.count > 0, "Real field party opened"): return
    var party: Array = s.partyStatus.projection
    for index in range(party.size()):
        var p: Dictionary = party[index]
        var carried: Dictionary = s.party.filter(func(a): return a.Actor.Value == p.actor)[0]
        if not check(p.hp == carried.Hp and p.mp == carried.Mp and p.exp == carried.Exp and p.Inventory.size() == 4, "Live scalar resources and four source slots"): return
        if case_name == "private":
            if not check(p.Inventory.all(func(slot): return slot.Empty != false or slot.Name != null), "All carried private item names admitted"): return
            if not check(p.Spells.all(func(spell): return spell.Name != null), "All learned private spell names admitted"): return
    # A held opening direction is modal, and an echo cannot move the field player.
    var field_entities = s.entities
    key_event(KEY_DOWN,true)
    await frames(12)
    key_event(KEY_DOWN,false)
    await frames(3)
    if not check(state().entities == field_entities, "Modal held direction never moves field player"): return
    var selected = state().partyStatus.index
    await pad(JOY_BUTTON_DPAD_DOWN)
    if not check(state().partyStatus.index == (int(selected)+1)%party.size(), "Mapped pad roster navigation"): return
    await tap(KEY_LEFT)
    for count in range(int(state().partyStatus.index)): await tap(KEY_LEFT)
    # Original member-screen grouping is the default; the roster is only a compact selector.
    s = record("source-layout")
    if not check(not s.partyStatus.selectorOpen, "No persistent roster replaces original summary"): return
    if not source_layout(s.partyStatus): return
    if case_name == "private" and not check_portrait(): return
    await tap(KEY_TAB)
    await tap(KEY_DOWN)
    s = record("magic-focus")
    if not check(s.partyStatus.focusPane == 1, "Mapped magic focus"): return
    await pad(JOY_BUTTON_RIGHT_SHOULDER)
    await tap(KEY_DOWN)
    if not check(state().partyStatus.focusPane == 2, "Mapped pad items focus"): return
    await pad(JOY_BUTTON_RIGHT_SHOULDER)
    # Mouse selector opens only when requested; complete gestures select and close it.
    await click(state().partyStatus.selectorRect)
    s = state()
    if not check(s.partyStatus.selectorOpen, "Compact member dropdown opens"): return
    var target := 0 if int(s.partyStatus.index) != 0 else mini(1,party.size()-1)
    var rect: Dictionary = s.partyStatus.rows[target].rect
    await mouse_button(center(rect), false)
    if not check(state().partyStatus.index == s.partyStatus.index, "Orphan mouse release rejected"): return
    await mouse_button(center(rect), true)
    var motion := InputEventMouseMotion.new()
    motion.position = Vector2(4,4); motion.relative = Vector2(20,20)
    Input.parse_input_event(motion)
    await frames(2)
    await mouse_button(center(rect), false)
    if not check(state().partyStatus.index == s.partyStatus.index, "Drag-out release rejected"): return
    await click(rect)
    if not check(state().partyStatus.index == target and not state().partyStatus.selectorOpen, "Mouse member selection closes dropdown"): return
    if case_name == "private" and not check_portrait(): return
    var selected_before = state().partyStatus.index
    await click(state().partyStatus.nextRect)
    if not check(state().partyStatus.index == (int(selected_before)+1)%party.size(), "Mouse next-member arrow"): return
    if case_name == "private" and not check_portrait(): return
    if case_name == "authored":
        if not check(party.size() == 14 and party[0].Inventory[0].Empty == false and party[0].Inventory[0].Name == party[0].Inventory[1].Name, "Full roster and duplicate slot identity"): return
        if not check(party[1].Inventory.all(func(slot): return slot.Empty == true) and party[2].hp == 0 and party[3].Inventory.all(func(slot): return slot.Empty == false), "Empty slots and dead retained member"): return
        await click(state().partyStatus.selectorRect)
        for count in range(5): await wheel(state().partyStatus.rosterRect, MOUSE_BUTTON_WHEEL_DOWN)
        s = record("selector-wheel")
        if not check(s.partyStatus.rosterScroll > 0, "Mouse selector list scroll"): return
        for count in range(14-int(s.partyStatus.index)-1): await tap(KEY_DOWN)
        await frames(4)
        s = state()
        if not check(s.partyStatus.index == 13 and s.partyStatus.rows[-1].rect.y >= s.partyStatus.rosterRect.y and s.partyStatus.rows[-1].rect.y < s.partyStatus.rosterRect.y+s.partyStatus.rosterRect.height, "Keyboard selected member remains visible"): return
        await click(s.partyStatus.rows[-1].rect)
        if not check(not state().partyStatus.selectorOpen, "Selected last member closes dropdown"): return
    # Paired list panes retain distinct scroll positions; wheel cannot move the other pane.
    s = state()
    var magic_scroll = s.partyStatus.layout.magic.scroll
    await wheel(s.partyStatus.layout.items.rect, MOUSE_BUTTON_WHEEL_DOWN)
    s = record("items-scroll")
    if not check(s.partyStatus.layout.magic.scroll == magic_scroll and (s.partyStatus.layout.items.height <= s.partyStatus.layout.items.rect.height or s.partyStatus.layout.items.scroll > 0), "Independent items scroll"): return
    if not source_layout(s.partyStatus): return
    # Hide/show is the existing visibility/focus suspension seam; a held press cannot resume it.
    view.hide()
    await frames(2)
    view.show()
    await frames(3)
    await click(state().partyStatus.closeRect)
    if not check(not state().partyStatus.visible, "Mouse close affordance"): return
    if not check(resources(state()) == resources(before), "Browse and close publish no command or gameplay changes"): return
    for repeat in range(2):
        await pad(JOY_BUTTON_START)
        await frames(3)
        if not check(state().partyStatus.visible and state().partyStatus.index == 0, "Reopen reads fresh party at first member"): return
        await pad(JOY_BUTTON_B)
        if not check(not state().partyStatus.visible and resources(state()) == resources(before), "Mapped close has no clickthrough"): return
    await tap(KEY_DOWN)
    if not await wait_for(func(current): return current.get("canWaitAtInput",false),"fresh field movement"): return
    var moved := record("fresh-movement")
    if not check(moved.entities != before.entities and not moved.partyStatus.visible, "Fresh field control after close"): return
    finish(true)

func source_layout(p: Dictionary) -> bool:
    var l: Dictionary = p.layout
    var stats: Array = l.stats
    var member: Dictionary = p.projection[int(p.index)]
    var values: Array = [member.Level,member.Attack,member.Defense,member.Agility,member.exp]
    for index in range(values.size()):
        var expected := "Unavailable" if values[index] == null else str(int(values[index]))
        if not check(stats[index].value == expected, "Aligned stat value reads live projection"): return false
    if not check(p.kills == ("Unavailable" if member.Kills == null else str(int(member.Kills))) and p.defeats == ("Unavailable" if member.Defeats == null else str(int(member.Defeats))), "Left summary reads carried kills/defeats"): return false
    if not check(stats.map(func(row): return row.label) == ["LEVEL","ATTACK","DEFENSE","AGILITY","EXP","MOVE"], "Source-aligned stat labels"): return false
    if not check(stats[0].labelRect.y == stats[1].labelRect.y and l.hpRect.y == stats[2].labelRect.y and l.mpRect.y == stats[3].labelRect.y and stats[4].labelRect.y == stats[5].labelRect.y, "Four original stat pairs align"): return false
    if not check(p.portraitRect.x < l.rect.x and p.portraitRect.y+p.portraitRect.height <= p.recordRect.y and p.recordRect.y+p.recordRect.height <= p.goldRect.y, "Portrait records and gold remain left"): return false
    if not check(l.magic.rect.y == l.items.rect.y and l.magic.rect.x+l.magic.rect.width < l.items.rect.x and l.items.rect.x+l.items.rect.width <= l.rect.x+l.rect.width, "Magic and items are parallel columns"): return false
    if not check(l.rect.y+l.rect.height <= 960 and l.magic.rect.y > stats[4].labelRect.y and l.items.rect.y+l.items.rect.height <=960, "Readable groups remain inside1080p frame"): return false
    for value in p.recordValues:
        if not check(value.minimumWidth <= value.rect.width and value.rect.x+value.rect.width <= p.recordRect.x+p.recordRect.width-24, "Measured accounting value fits summary border"): return false
    for row in stats:
        if not check(row.labelRect.x+row.labelRect.width <= row.valueRect.x, "Stat labels/values never overlap"): return false
    return true

func check_portrait() -> bool:
    var p: Dictionary = state().partyStatus
    var portrait := view.find_child("MemberPortrait",true,false) as TextureRect
    return check(p.portraitVisible and portrait != null and portrait.texture != null and portrait.texture.get_size() == Vector2(48,64) and p.portraitRect.width ==192 and p.portraitRect.height ==256, "Reliably bound complete portrait at4x")

func center(rect: Dictionary) -> Vector2:
    return Vector2(rect.x+rect.width/2,rect.y+rect.height/2)

func mouse_button(position: Vector2, pressed: bool) -> void:
    var event := InputEventMouseButton.new()
    event.position = position; event.global_position = position
    event.button_index = MOUSE_BUTTON_LEFT; event.pressed = pressed
    Input.parse_input_event(event)
    await frames(3)

func click(rect: Dictionary) -> void:
    var position := center(rect)
    for pressed in [true,false]:
        var event := InputEventMouseButton.new()
        event.position = position; event.global_position = position
        event.button_index = MOUSE_BUTTON_LEFT; event.pressed = pressed
        Input.parse_input_event(event)
        await frames(3)

func wheel(rect: Dictionary, which: MouseButton) -> void:
    for pressed in [true,false]:
        var event := InputEventMouseButton.new()
        event.position = center(rect); event.global_position = event.position
        event.button_index = which; event.pressed = pressed
        Input.parse_input_event(event)
        await frames(2)

func pad(which: JoyButton) -> void:
    for pressed in [true,false]:
        var event := InputEventJoypadButton.new()
        event.device = 15; event.button_index = which; event.pressed = pressed
        Input.parse_input_event(event)
        await frames(3)
