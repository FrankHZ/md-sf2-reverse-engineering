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
    # Explicit keyboard focus owns details scrolling, never field movement.
    await tap(KEY_TAB)
    await tap(KEY_DOWN)
    await frames(4)
    s = record("keyboard-scroll")
    if not check(s.partyStatus.detailFocus and (s.partyStatus.detailHeight <= s.partyStatus.detailRect.height or s.partyStatus.detailScroll > 0), "Mapped details focus and scrolling"): return
    await pad(JOY_BUTTON_RIGHT_SHOULDER)
    if not check(not state().partyStatus.detailFocus, "Mapped pad focus transfer"): return
    # Native row click selects only on completed press/release.
    s = state()
    var target := 0 if int(s.partyStatus.index) != 0 else mini(1,party.size()-1)
    var rect: Dictionary = s.partyStatus.rows[target].rect
    # Release without its own press never selects a roster row.
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
    if not check(state().partyStatus.index == target, "Mouse roster selection"): return
    if case_name == "authored":
        s = state()
        if not check(party.size() == 14 and party[0].Inventory[0].Empty == false and party[0].Inventory[0].Name == party[0].Inventory[1].Name, "Full roster and duplicate slot identity"): return
        if not check(party[1].Inventory.all(func(slot): return slot.Empty == true) and party[2].hp == 0 and party[3].Inventory.all(func(slot): return slot.Empty == false), "Empty slots and dead retained member"): return
        for count in range(5): await wheel(s.partyStatus.rosterRect, MOUSE_BUTTON_WHEEL_DOWN)
        s = record("roster-wheel")
        if not check(s.partyStatus.rosterScroll > 0, "Mouse roster scroll"): return
        var last: Dictionary = s.partyStatus.rows[-1].rect
        # Bring the last row into the visible portion using ordinary mapped navigation.
        for count in range(14-int(s.partyStatus.index)-1): await tap(KEY_DOWN)
        await frames(4)
        s = state()
        if not check(s.partyStatus.index == 13 and s.partyStatus.rows[-1].rect.y >= s.partyStatus.rosterRect.y and s.partyStatus.rows[-1].rect.y < s.partyStatus.rosterRect.y+s.partyStatus.rosterRect.height, "Keyboard selection remains visible"): return
    s = state()
    await wheel(s.partyStatus.detailRect,MOUSE_BUTTON_WHEEL_DOWN)
    await frames(3)
    s = record("mouse-details-scroll")
    if not check((s.partyStatus.detailHeight <= s.partyStatus.detailRect.height or s.partyStatus.detailScroll > 0), "Mouse details scrolling"): return
    var scroll: Dictionary = s.partyStatus.detailRect
    if not check(scroll.x > s.partyStatus.rosterRect.x+s.partyStatus.rosterRect.width and scroll.x+scroll.width <=1920 and scroll.y+scroll.height <=972 and s.partyStatus.card.rect.y+s.partyStatus.card.rect.height <=scroll.y, "Stable nonoverlapping native panels"): return
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
