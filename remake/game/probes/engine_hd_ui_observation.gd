extends "res://probes/engine_hd_graphics_observation.gd"

# Observe production views through their mapped input and actual draw projections.
var graphics := "original"
var first: Dictionary = {}

func texture_pixels(texture_id: Variant) -> String:
    if texture_id == null: return ""
    var texture := instance_from_id(int(texture_id)) as Texture2D
    if texture == null:
        check(false, "drawn texture handle resolves")
        return ""
    var context := HashingContext.new()
    context.start(HashingContext.HASH_SHA256)
    context.update(texture.get_image().get_data())
    return context.finish().hex_encode()

func commands(label: String, rows: Array, prefix: String, fallback := false, count := 4) -> void:
    check(rows.size() == count, label + ": expected command count")
    var projection := []
    var textures := []
    var expected := "original" if fallback else graphics
    for row in rows:
        var art: Dictionary = row.art
        var suffix := ".selected" if row.selected else ".normal"
        check(str(art.resource).begins_with(prefix) and str(art.resource).ends_with(suffix),
            label + ": drawn resource follows selection")
        check(art.variant == expected and art.texture != null and art.filter == "Nearest" and
            art.width == (96 if expected == "hd" else 24) and art.height == art.width,
            label + ": selected texture density")
        projection.append([art.resource, art.scale, row.rect, row.enabled, row.selected])
        textures.append([art.texture, texture_pixels(art.texture)])
    if not fallback: compare(label, projection, textures)
    samples.append({"label":label, "graphics":graphics, "rows":rows, "textures":textures})

func identities(label: String, kind: String, partial := "") -> void:
    var rows: Array = JSON.parse_string(host.call("ReadIdentityIconPixelsObservationJson"))
    var projection := []
    var textures := []
    var selected := false
    var normal := false
    for row in rows:
        var art: Dictionary = row.art
        if not art.identity.begins_with("identity-icon." + kind + "."): continue
        var expected := "original" if art.identity == partial else graphics
        check(art.resource == art.drawn and art.mode == graphics and art.variant == expected and
            art.width == (64 if expected == "hd" else 16) and art.height == (96 if expected == "hd" else 24),
            label + ": identity texture and density")
        selected = selected or art.drawn.ends_with(".selected")
        normal = normal or art.drawn.ends_with(".normal")
        projection.append([art.drawn, art.rect, art.scale])
        textures.append([art.texture, row.pixelsSha256])
    check(selected and normal, label + ": actual selected and normal states")
    compare(label, projection, textures)
    samples.append({"label":label, "graphics":graphics, "pixels":rows})

func compare(label: String, projection: Array, textures: Array) -> void:
    if not first.has(label):
        first[label] = {"projection":projection, "textures":textures}
        return
    check(first[label].projection == projection, label + ": same resource state and input/draw geometry")
    check((first[label].textures == textures) == (graphics == "original"),
        label + ": mode change selects new handles, round trip reuses Original")

func cursor(label: String, role: String) -> void:
    var map := observation(find("BattleSessionView").get_node("MapViewport"), "ReadTerrainJson")
    var art: Dictionary = map.cellCursor
    check(map.cursorRole == role and art.visible and art.draws > 0 and
        art.mode == graphics + "-single-cell" and art.resource == "battle-cursor.single-cell" and
        art.width == (128 if graphics == "hd" else 32) and art.height == art.width,
        label + ": actual cursor texture follows the active role")
    check(art.rect.width == 96 and art.drawn.width == 128 and art.drawn.height == 128 and
        art.drawn.x == art.rect.x - 16 and art.drawn.y == art.rect.y - 16, label + ": unchanged cell and anchor")
    compare(label, [art.rect.width, art.rect.height, art.drawn.width, art.drawn.height, art.z], [art.texture, art.sha256])
    samples.append({"label":label, "graphics":graphics, "cursor":art})

func field_return() -> bool:
    for i in range(480):
        var s := state()
        if s.get("failure") != null: break
        if s.get("canWaitAtInput", false): return true
        if s.get("canWaitForText", false): await key(KEY_ENTER)
        else: await process_frame
    check(false, "returns to settled field control")
    samples.append({"fieldFailure":state()})
    return false

func field_sample(partial := false) -> void:
    await key(KEY_ESCAPE)
    for i in range(4):
        if state().fieldMenu.index == 2: break
        await key(KEY_DOWN)
    commands("field", state().fieldMenu.menu.commands, "field-menu.", partial)
    await key(KEY_ENTER); await key(KEY_ENTER)
    var step: Dictionary = state().partyStatus.preparation
    check(step.stage == "item-actions" and step.operations.visible, "Item opens real operations")
    commands("item", step.operations.commands, "item-menu.")
    await key(KEY_ENTER)
    check(state().partyStatus.preparation.stage == "use-item", "focused Use opens item rows")
    identities("field-items", "item", "identity-icon.item.71" if partial else "")
    for i in range(6):
        if state().get("canWaitAtInput", false) and not state().fieldMenu.visible and not state().partyStatus.visible: return
        await key(KEY_ESCAPE)
    check(false, "Item unwinds to the field")

func prepare_sample() -> void:
    await key(KEY_ESCAPE)
    for i in range(4):
        if state().fieldMenu.index == 0: break
        await key(KEY_UP)
    await key(KEY_ENTER)
    check(state().partyStatus.visible, "Member opens its roster")
    await key(KEY_ENTER)
    var page: Dictionary = state().partyStatus.preparation
    check(page.stage == "actions" and page.commands.visible, "Member Prepare opens the five-command list")
    var rows: Array = page.commands.commands
    check(rows.map(func(row):return str(row.art.resource).rsplit(".",true,1)[0]) ==
        ["item-menu.equip","item-menu.give","item-menu.use","field-menu.magic","item-menu.drop"],
        "Prepare retains the correct original source keys")
    commands("prepare", rows, "", false, 5)
    await key(KEY_ESCAPE)
    check(state().partyStatus.visible and not state().partyStatus.preparation.visible, "Prepare Back restores the roster")
    await key(KEY_ESCAPE); await key(KEY_ESCAPE)

func service_sample(service: String) -> void:
    await key(KEY_ENTER)
    for i in range(480):
        if state().get("wait") == ("ShopWait" if service == "shop" else "ChurchWait"): break
        if state().get("canWaitForText", false): await key(KEY_ENTER)
        else: await process_frame
    var s := state()
    if s.get("wait") != ("ShopWait" if service == "shop" else "ChurchWait"):
        check(false, "interacting opens " + service)
        samples.append({"serviceFailure":s})
        return
    await frames(5)
    if service == "shop" and s.shop.visible:
        commands("shop", s.shop.services.commands, "shop-menu.")
        await key(KEY_ENTER)
        check(state().shop.wait.Phase == 1, "focused Buy opens stock")
        identities("shop-stock", "item")
        await key(KEY_ESCAPE)
    elif service == "church" and s.church.visible:
        commands("church", s.church.commands, "church-menu.")
        await key(KEY_DOWN)
        commands("church-cure", state().church.commands, "church-menu.")
        await key(KEY_UP)
    await key(KEY_ESCAPE)
    await field_return()

func battle_sample() -> void:
    await key(KEY_ENTER)
    check(state().menu.page == "Main", "Confirm opens the real battle command list")
    commands("battle", state().menu.layout.commands, "command-menu.")
    await key(KEY_H)
    if state().menu.page != "Magic":
        check(false, "mapped Magic opens spells")
        samples.append({"battleFailure":state()})
        return
    identities("battle-spells", "spell")
    for i in range(4):
        var icons: Array = state().menu.layout.icons
        if icons.any(func(art): return art.drawn == "identity-icon.spell.14.selected"): break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(state().menu.page == "Target" and state().candidate == "enemy-1", "selected BLAST has its prepared enemy target")
    await frames(12)
    cursor("target", "Target")
    for i in range(6):
        if state().get("stage") == "Movement": break
        await key(KEY_ESCAPE)
    await key(KEY_ESCAPE)
    check(state().inspection.page == "Cursor", "Back from movement opens inspection")
    cursor("inspection", "Inspection")
    for i in range(4):
        var inspect: Dictionary = state().inspection
        if not inspect.units.any(func(unit): return unit.Position == inspect.cursor): break
        await key(KEY_RIGHT)
    await key(KEY_ENTER)
    check(state().inspection.page == "Options", "Confirm over empty ground opens inspection options")
    commands("battlefield", state().inspection.options.commandList.rows, "battlefield-menu.")
    await key(KEY_ESCAPE); await key(KEY_ESCAPE)
    check(state().get("stage") == "Movement" and not state().inspection.active, "inspection returns to movement")

func wait_movement() -> bool:
    for i in range(480):
        var s := state()
        if s.get("failure") != null: break
        if s.get("stage") == "Movement" and s.get("stopReason") == "PlayerInput": return true
        if s.get("scene", {}).get("requiresAcknowledgement", false): await key(KEY_ENTER)
        else: await frames(3)
    check(false, "settled battle movement")
    samples.append({"battleFailure":state()})
    return false

func run() -> void:
    var space := OS.get_environment("SF2_SAVE_NAMESPACE")
    if output.is_empty() or not space.begins_with("issue2178-"):
        push_error("Use a disposable issue2178-* namespace and SF2_HOST_UI_OUTPUT"); quit(2); return
    preference_path = OS.get_environment("SF2_SAVE_ROOT").path_join(space).path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(6)
    var started: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    if case_name == "invalid":
        check(started == "ui-hd-raster-shape", "bad declared HD raster rejects at Content admission")
        check(find("ExplorationSessionView") == null, "invalid variant never mounts")
        finish(); return
    check(started == "started", "ordinary admission: " + started)
    if started != "started": finish(); return
    await frames(10)
    if case_name == "battle":
        if not await wait_movement(): finish(); return
        for i in range(2):
            if state().get("actor") == "ally-1": break
            await key(KEY_ENTER); await key(KEY_SPACE)
            if not await wait_movement(): finish(); return
        if state().get("actor") != "ally-1":
            check(false, "Sarah's prepared movement turn")
            finish(); return
    elif not await field_return(): finish(); return
    if case_name == "reopen":
        check(applied().GraphicsMode == "hd", "fresh process reads persisted HD preference")
        graphics = "hd"
        await field_sample()
        finish(); return
    check(applied().GraphicsMode == "original", "fresh launch defaults to Original")
    if case_name == "partial":
        await field_settings(); await apply_mode("hd")
        graphics = "hd"; await field_sample(true)
        finish(); return
    for selected_mode in ["original", "hd", "original"]:
        if selected_mode != graphics:
            await field_settings()
            if find("SettingsView") == null: finish(); return
            var start_ms := Time.get_ticks_msec()
            await apply_mode(selected_mode)
            samples.append({"graphicsApplied":selected_mode, "navigationAndApplyMs":Time.get_ticks_msec()-start_ms})
            if applied().GraphicsMode != selected_mode: finish(); return
            graphics = selected_mode
        match case_name:
            "field":
                await field_sample()
                await prepare_sample()
            "shop", "church": await service_sample(case_name)
            "battle": await battle_sample()
            _: check(false, "select field, shop, church or battle"); finish(); return
        check(state().get("failure") == null, "no production presentation failure")
        if not failures.is_empty(): finish(); return
    if case_name == "field":
        await field_settings()
        await apply_mode("hd")
    finish()
