extends "res://probes/engine_battle_cursor_observation.gd"

# Observe the existing readers through ordinary input on bounded battle and field starts.
var chinese := OS.get_environment("SF2_SPELL_DESCRIPTION_LANGUAGE") == "zh-CN"
var low_mp := OS.get_environment("SF2_SPELL_DESCRIPTION_LOW_MP") == "1"

func spell_battle() -> bool:
    bind_events()
    events.spell = InputMap.action_get_events("sf2_spell").filter(func(e): return e is InputEventKey)[0]
    if not await enter_battle(): return false
    await act("confirm")
    await act("spell")
    var first := state()
    var reader: Dictionary = first.menu.layout.reader
    samples.append({"label":"first-rank", "menu":first.menu, "actor":first.actors.filter(func(a): return a.id == first.actor)})
    var damage := "造成火属性伤害" if chinese else "Deals fire damage"
    var silence := "施放仍会消耗 MP" if chinese else "Casting still uses MP"
    if not expect(first.menu.page == "Magic" and first.menu.layout.rows[0].contains("2 MP") and
            damage in reader.text and silence in reader.text and "BLAZE" not in reader.text,
            "Rank one describes the effect and payable suppression without repeating the row"): return false
    await act("right")
    var keyboard: Dictionary = state().menu.layout.reader
    var key: InputEvent = events.left
    events.left = InputMap.action_get_events("sf2_left").filter(func(e): return e is InputEventJoypadButton)[0]
    await act("left")
    events.left = key
    var pad: Dictionary = state().menu.layout.reader
    await read_wheel(reader)
    var wheel: Dictionary = state().menu.layout.reader
    if not expect(keyboard.scroll > 0 and pad.scroll < keyboard.scroll and wheel.scroll > pad.scroll and
            pad.hint != keyboard.hint and signature(state()) == signature(first) and state().menu.row == 0,
            "Keyboard, mapped pad and wheel only move the reading position"): return false
    await act("down")
    var second := state()
    var area: Dictionary = second.menu.layout.reader
    if not expect(second.menu.row == 1 and second.menu.layout.rows[1].contains("6 MP") and damage in area.text and
            ("半径 1 格" if chinese else "within 1 tile") in area.text and area.scroll == 0 and
            area.rect == reader.rect and second.menu.layout.rowRects == first.menu.layout.rowRects and
            signature(second) == signature(first), "Rank two adds its area and resets reading in the same bounds"): return false
    if low_mp:
        if not expect(("MP不足" if chinese else "Not enough MP") in area.text and silence not in area.text,
                "Insufficient MP refuses this rank before Silence can suppress it"): return false
        await act("confirm")
        if not expect(state().menu.page == "Magic" and signature(state()) == signature(second),
                "Unavailable rank does not submit"): return false
    else:
        await act("confirm")
        var target := state()
        if not expect(target.menu.page == "Target" and target.menu.layout.reader.text == "BLAZE 2" and
                target.menu.layout.reader.hint == null and resources(target) == resources(second),
                "Target yields the reader to the exact level before payment"): return false
        await act("cancel")
        var returned := state()
        if not expect(returned.menu.page == "Magic" and returned.menu.row == 1 and returned.menu.layout.reader.text == area.text and
                resources(returned) == resources(second), "Back restores the exact Magic row and description"): return false
    await act("down")
    var unsupported := state()
    if not expect(unsupported.menu.row == 2 and
            ("无法使用此法术效果" if chinese else "spell effect is unavailable") in unsupported.menu.layout.reader.text,
            "Unadmitted healing effect has an explicit explanation"): return false
    await act("down")
    var egress := state()
    if not expect(egress.menu.row == 3 and egress.menu.layout.rows[3].contains("8 MP") and
            ("选择施法者本人" if chinese else "Select the caster") in egress.menu.layout.reader.text and
            silence not in egress.menu.layout.reader.text, "EGRESS names the self target and bypasses Silence"): return false
    if not low_mp:
        await act("confirm")
        if not expect(state().menu.page == "Target" and state().candidate == egress.actor,
                "Ordinary EGRESS Confirm focuses the caster"): return false
        await act("cancel")
        if not expect(state().menu.row == 3 and resources(state()) == resources(egress), "EGRESS returns without spending"): return false
    samples.append({"label":"battle-magic", "rankOne":reader, "rankTwo":area, "unsupported":unsupported.menu.layout.reader,
        "egress":egress.menu.layout.reader, "keyboard":keyboard.scroll, "pad":pad.scroll, "wheel":wheel.scroll})
    return expect(state().failure == null and state().scene.error == null, "Battle Magic has no session or presentation error")

func field_signature(s: Dictionary) -> Array:
    return [s.sessionId, s.revision, s.observationSequence, s.mainSeed, s.thinkingSeed, s.party, s.gold, s.flags, s.itemCustody]

func field_prepare() -> Dictionary:
    return state().partyStatus.preparation

func spell_field() -> bool:
    for key in [KEY_ENTER, KEY_E, KEY_DOWN, KEY_ENTER]:
        await tap(key)
    var limit := create_timer(15)
    while host.find_child("ExplorationSessionView", true, false) == null and limit.time_left > 0:
        await process_frame
    if not expect(host.find_child("ExplorationSessionView", true, false) != null, "Published field seed continues"): return false
    if not await wait_for(func(s): return s.canWaitAtInput, "settled field input"): return false
    await tap(KEY_ESCAPE)
    await tap(KEY_DOWN)
    await tap(KEY_ENTER)
    await tap(KEY_RIGHT)
    await tap(KEY_ENTER)
    var before := state()
    var magic: Dictionary = before.partyStatus.preparation
    samples.append({"label":"first-field-choice", "preparation":magic})
    if not expect(magic.visible and magic.stage == "magic", "Direct field Magic opens the second member's choices"): return false
    var free: Dictionary = magic.page.description
    if not expect(("不消耗 MP" if chinese else "uses no MP") in free.text and magic.page.rows[0].text.contains("3 MP"),
            "Field HEAL has a free no-effect description"): return false
    await tap(KEY_ENTER)
    if not expect(field_prepare().stage == "confirm" and field_signature(state()) == field_signature(before),
            "Free branch uses ordinary confirmation without payment"): return false
    await tap(KEY_ESCAPE)
    var row := -1
    for i in range(1, magic.page.rows.size()):
        await tap(KEY_DOWN)
        var current := field_prepare()
        if ("解除" if chinese else "Cures") in current.page.description.text and current.page.rows[i].text.contains("6 MP"):
            row = i
            break
    var cure := field_prepare()
    var reader: Dictionary = cure.page.description
    if not expect(row >= 0 and cure.index == row and ("中毒" if chinese else "poison and paralysis") in reader.text and
            reader.rect == free.rect and cure.page.rows.map(func(r): return r.rect) == magic.page.rows.map(func(r): return r.rect) and
            field_signature(state()) == field_signature(before),
            "Recovery rank two describes the actual cure and maintains reading and row geometry"): return false
    await tap(KEY_RIGHT)
    var scrolled := field_prepare()
    if not expect(scrolled.page.description.scroll > 0 and scrolled.index == row and field_signature(state()) == field_signature(before),
            "Field reading changes scroll without gameplay effects"): return false
    await tap(KEY_ENTER)
    var target := field_prepare()
    if not expect(target.stage == "magic-target" and not target.page.description.visible and ("2" in target.title) and
            field_signature(state()) == field_signature(before), "Field Target shows the rank and keeps the reader out of target selection"): return false
    await tap(KEY_ENTER)
    if not expect(field_prepare().stage == "confirm" and field_signature(state()) == field_signature(before),
            "Choosing the recipient leads to ordinary confirmation without payment"): return false
    await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    var returned := field_prepare()
    samples.append({"label":"field-magic", "free":free, "cure":reader, "scrolled":scrolled.page.description,
        "target":target.title, "return":returned.page.description})
    if not expect(returned.stage == "magic" and returned.index == row and returned.page.description.text == reader.text and
        field_signature(state()) == field_signature(before) and state().failure == null,
        "Field Back restores the exact rank with no resource or RNG change"): return false
    await tap(KEY_ENTER)
    await tap(KEY_ENTER)
    await tap(KEY_ENTER)
    var paid := state()
    var caster_before: Dictionary = before.party.filter(func(a): return a.Actor.Value == "ally-1")[0]
    var caster_after: Dictionary = paid.party.filter(func(a): return a.Actor.Value == "ally-1")[0]
    var target_before: Dictionary = before.party.filter(func(a): return a.Actor.Value == "ally-0")[0]
    var target_after: Dictionary = paid.party.filter(func(a): return a.Actor.Value == "ally-0")[0]
    samples.append({"label":"field-payment", "mp":[caster_before.Mp, caster_after.Mp],
        "status":[target_before.Status, target_after.Status]})
    return expect(field_prepare().stage == "result" and paid.revision == before.revision + 1 and
        paid.failure == null and caster_after.Mp == caster_before.Mp - 6 and int(target_before.Status) & 2 != 0 and
        int(target_after.Status) & 2 == 0 and paid.mainSeed == before.mainSeed and paid.thinkingSeed == before.thinkingSeed,
        "Confirmed recovery clears poison, pays once and leaves both RNG streams untouched")

func run() -> void:
    root.size = Vector2i(1600, 900) if chinese else Vector2i(1920, 1080)
    var field := demo_case == "spell-field"
    var key := "spell-description.cure-payment" if field else "battle.range"
    var catalog: Translation = load("res://localization/zh_CN.po" if chinese else "res://localization/en.po")
    var extra := "请查看所选法术的说明。" if chinese else "Read the selected spell description. "
    catalog.add_message(key, str(catalog.get_message(key)) + "\n" + extra.repeat(12))
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(8)
    var passed := await spell_field() if field else await spell_battle()
    if passed:
        host.queue_free()
        await frames(5)
    finish(passed)
