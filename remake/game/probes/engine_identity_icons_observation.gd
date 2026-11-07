extends "res://probes/engine_menu_assets_observation.gd"

func record(label: String) -> void:
    var s := state()
    var row := {"label":label}
    for k in ["revision", "stage", "failure", "displayLocale", "fieldMenu", "partyStatus", "menu", "party", "actors", "equipmentVisible", "equipmentText", "equipmentIcon", "hudPanel"]:
        if s.has(k): row[k] = s[k]
    samples.append(row)

func icons(rows: Array, label: String) -> void:
    var visible := 0
    for icon in rows:
        if not icon.visible: continue
        visible += 1
        check(icon.resource == icon.drawn and icon.texture != null, label + ": actual texture identity")
        check(icon.width == 16 and icon.height == 24 and icon.scale == 4 and icon.filter == "Nearest", label + ": original size/filter")
        check(icon.rect.width >= 64 and icon.rect.height >= 96, label + ": projection size")
    check(visible > 0 if expect_art else visible == 0, label + ": resource/fallback mode")
    record(label)
    samples.append({"label":label + "-explicit-pixels", "pixels":JSON.parse_string(host.call("ReadIdentityIconPixelsObservationJson"))})

func church() -> void: pass

func field() -> void:
    await key(back); await key(KEY_LEFT); await key(KEY_ENTER); await key(KEY_ENTER)
    await frames(4)
    var actions: Dictionary = state().partyStatus.preparation
    check(actions.stage == "magic", "ordinary exploration Magic")
    icons(actions.icons, "field-magic")
    var before: int = state().revision
    var held := InputEventKey.new(); held.keycode = KEY_ENTER; held.pressed = true; Input.parse_input_event(held)
    await frames(20)
    check(state().revision == before and state().partyStatus.preparation.stage == "confirm", "held opening Confirm never commits child")
    held = InputEventKey.new(); held.keycode = KEY_ENTER; held.pressed = false; Input.parse_input_event(held); await frames(3)
    await key(back)
    check(state().partyStatus.preparation.stage == "magic" and state().revision == before, "field confirmation Back preserves resources")
    for i in 4:
        if state().fieldMenu.visible: break
        await key(back)
    await key(KEY_RIGHT); await key(KEY_ENTER); await key(KEY_ENTER)
    await key(KEY_ENTER); await frames(4)
    actions = state().partyStatus.preparation
    check(actions.stage == "use-item", "ordinary exploration Use")
    icons(actions.icons, "field-use")
    await key(KEY_DOWN); await frames(4); icons(state().partyStatus.preparation.icons, "field-use-next")
    await key(back); await key(KEY_RIGHT); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.preparation.stage == "equipment", "ordinary exploration Equip")
    icons(state().partyStatus.preparation.icons, "field-equip")
    await key(KEY_DOWN); await frames(4); icons(state().partyStatus.preparation.icons, "field-equip-next")
    for i in 4:
        if state().fieldMenu.visible: break
        await key(back)
    check(state().fieldMenu.visible, "field returns to same diamond")
    await key(back); check(not state().partyStatus.visible, "field caller restored")
    record("field-return")

func battle() -> void:
    var controlled := OS.get_environment("SF2_IDENTITY_CONTROLLED") == "1"
    # Actual saved player turn. Iterate actual turns only when the current actor has no magic.
    for turn in 4:
        await key(KEY_ENTER); await key(KEY_LEFT); await key(KEY_ENTER); await frames(4)
        var layout: Dictionary = state().menu.layout
        record("magic-attempt-" + str(turn))
        if layout.rows.size() >= (3 if controlled else 1):
            icons(layout.icons, "battle-magic")
            if controlled:
                check(layout.icons.size() >= 3 and layout.icons[0].resource == "identity-icon.spell.0.selected" and layout.icons[1].resource == "identity-icon.spell.0.normal", "HEAL levels share source base ID")
            await key(KEY_DOWN); await frames(4); icons(state().menu.layout.icons, "battle-magic-next")
            await key(back); break
        if not expect_art and mode == "authored":
            icons(layout.icons, "authored-magic-empty"); await key(back); break
        await key(back); await key(back)
        if not await stay(): return
    await key(KEY_RIGHT); await key(KEY_ENTER); await frames(4)
    check(state().menu.page == "Items", "ordinary battle Item")
    icons(state().menu.layout.icons, "battle-item")
    await key(KEY_DOWN); await frames(4); icons(state().menu.layout.icons, "battle-item-next")
    if controlled:
        var item_icons: Array = state().menu.layout.icons
        check(item_icons[1].resource == "identity-icon.item.0.selected" and item_icons[2].resource == "identity-icon.item.0.normal", "duplicate herbs keep distinct slots/shared identity")
        await key(KEY_ENTER); await key(back); await frames(4)
        check(state().menu.page == "Items" and state().menu.row == 1, "target Back retains duplicate item slot")
        icons(state().menu.layout.icons, "duplicate-target-return")
        await key(back); await key(back)
        check(state().stage == "Movement", "controlled return")
        return
    if not expect_art and mode == "authored":
        await key(back); await key(back); record("authored-text-return"); return
    await key(back); await key(KEY_E); await frames(4)
    check(state().equipmentVisible and not state().menu.visible, "equipment covers command menu")
    icons([state().equipmentIcon], "battle-equip")
    await key(KEY_DOWN); await frames(4); icons([state().equipmentIcon], "battle-equip-next")
    await key(back); await key(back)
    check(state().stage == "Movement", "battle Back restores Movement")
    record("battle-return")



func run() -> void:
    if mode == "invalid-scene":
        node_added.connect(node_added_observe)
        root.size = Vector2i(1920,1080)
        host = (load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
        root.grab_focus(); await frames(5)
        await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
        if not await wait_panel("error"): finish(); return
        samples.append({"label":"invalid-resource-refused-before-continue", "panel":panel(), "persistence":persistence()})
        check(view == null, "invalid private resource never mounts a gameplay view")
        finish(); return
    if OS.get_environment("SF2_IDENTITY_CONTROLLED") != "1":
        await super.run()
        return
    create_timer(55).timeout.connect(func(): check(false, "controlled deadline"); finish())
    root.size = Vector2i(1920,1080)
    host = load("res://probes/IdentityIconsObservation.cs").new(); root.add_child(host)
    root.grab_focus()
    var admission: String = host.call("Begin", OS.get_environment("SF2_IDENTITY_PARTY"), OS.get_environment("SF2_IDENTITY_SETTINGS"))
    samples.append({"label":"independent-presenter-admission", "result":admission})
    if admission != "Movement": check(false, "controlled admission: " + admission); finish(); return
    view = host; await frames(5)
    record("controlled-presenter-entry")
    await battle(); finish()
