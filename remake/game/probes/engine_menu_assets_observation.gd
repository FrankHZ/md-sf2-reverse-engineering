extends "res://probes/engine_battle_save_observation.gd"

# Ordinary title/New Game or Continue; only mapped input and read-only projections.
var mode := OS.get_environment("SF2_MENU_ASSETS_CASE")
var expect_art := OS.get_environment("SF2_MENU_ASSETS_EXPECT") == "original"
var back := KEY_ESCAPE

func record(label: String) -> void:
    var s := state()
    var row := {"label":label}
    for k in ["sessionId", "revision", "map", "stage", "wait", "failure", "displayLocale", "fieldMenu", "church", "partyStatus", "menu", "party", "gold", "actors", "equipmentVisible", "equipmentText", "hudPanel"]:
        if s.has(k): row[k] = s[k]
    if s.has("entities"): row["player"] = player(s)
    samples.append(row)

func diamond(menu: Dictionary, prefix: String, label: String) -> void:
    check(menu.commands.size() == 4, label + ": four cardinal commands")
    for k in ["selection", "hint"]:
        if menu.has(k + "Rect"):
            var rect: Dictionary = menu[k + "Rect"]
            check(rect.x >= 0 and rect.y >= 0 and rect.x+rect.width <= 1921 and rect.y+rect.height <= 1081, label + ": " + k + " fits viewport")
            check(menu[k + "MinimumHeight"] <= rect.height+1, label + ": " + k + " text fits")
    for c in menu.commands:
        var a: Dictionary = c.art
        check(a.mounted == expect_art, label + ": expected resource mode " + c.id)
        check(c.rect.width == 96 and c.rect.height == 96, label + ": integral geometry")
        if expect_art:
            check(a.resource == prefix + "." + c.id.to_lower() + (".selected" if c.selected else ".normal"), label + ": actual drawn resource " + c.id)
            check(a.width == 24 and a.height == 24 and a.scale == 4 and a.filter == "Nearest" and not a.extraFrame and not a.labelVisible, label + ": original raster projection")
        else:
            check(a.labelVisible and a.extraFrame, label + ": explicit text fallback")
    var c: Array = menu.commands
    check(c[0].rect.y < c[1].rect.y and c[1].rect.x < c[0].rect.x and c[2].rect.x > c[0].rect.x and c[3].rect.y > c[1].rect.y, label + ": source cardinal relation")
    record(label)

func field() -> void:
    await key(back)
    for direction in [KEY_UP, KEY_LEFT, KEY_RIGHT, KEY_DOWN]:
        await key(direction); await frames(3)
        diamond(state().fieldMenu, "field-menu", "field-" + str(direction))
    await key(KEY_UP); await key(KEY_ENTER)
    check(state().partyStatus.visible and not state().fieldMenu.visible, "Member opens above hidden root")
    record("member-text-consumer")
    await key(back)
    check(state().fieldMenu.visible, "Member returns to same root")
    await key(KEY_LEFT); await key(KEY_ENTER); await key(KEY_ENTER)
    record("field-magic-consumer")
    for i in 3:
        if state().fieldMenu.visible: break
        await key(back)
    check(state().fieldMenu.visible, "Magic unwinds to root")
    await key(KEY_RIGHT); await key(KEY_ENTER); await key(KEY_ENTER)
    record("field-item-operations")
    # Existing Item operations: Use, Give, Equip, Drop.
    await key(KEY_DOWN); await key(KEY_DOWN); await key(KEY_ENTER)
    record("field-equipment-consumer")
    for i in 4:
        if state().fieldMenu.visible: break
        await key(back)
    check(state().fieldMenu.visible, "Equipment unwinds to root")
    await key(back)
    check(not state().fieldMenu.visible and not state().partyStatus.visible, "field caller restored")
    record("field-return")

func battle() -> void:
    await key(KEY_ENTER)
    for direction in [KEY_UP, KEY_LEFT, KEY_RIGHT, KEY_DOWN]:
        await key(direction); await frames(3)
        diamond(state().menu.layout, "command-menu", "battle-" + str(direction))
    await key(KEY_LEFT); await key(KEY_ENTER)
    record("battle-magic-consumer")
    await key(back)
    if state().menu.page == "Main":
        await key(KEY_E)
        check(state().equipmentVisible and not state().menu.visible, "ordinary equipment opens above hidden command menu")
        record("battle-equipment-consumer"); await key(back)
    await key(back)
    record("battle-return")
    check(state().stage == "Movement", "battle menu returns to Movement")

func player(s: Dictionary) -> Dictionary:
    for e in s.get("entities", []):
        if e.id == "entity-0": return e
    return {}

func cell() -> Vector2i:
    var p := player(state())
    return Vector2i(int(p.x / 384), int(p.y / 384))

func settle_field() -> bool:
    for i in 1200:
        var s := state()
        if s.get("failure") != null: check(false, "field stays healthy"); return false
        if s.canWaitAtInput and not player(s).get("moving", true): return true
        if s.get("wait") in ["DialogueWait", "FieldTextWait", "ChoiceWait"]: await key(KEY_ENTER)
        else: await process_frame
    check(false, "field settles"); return false

func church() -> void:
    # Bounded same-map walk to the accepted Map3 priest counter, using actual current layout.
    # New Game starts in the upstairs inset; cross the existing (54,3) stairs by input.
    if cell() == Vector2i(56,3):
        await key(KEY_LEFT); await settle_field()
        await key(KEY_LEFT); await settle_field()
        record("ordinary-stairs")
    var goal := Vector2i(32,13)
    var rejected := {}
    for step in 100:
        if cell() == goal: break
        if state().map != "map-3": check(false, "same Map3 approach"); return
        var grid := {}
        for y in range(0,32,16):
            for x in range(0,64,16):
                var region: Dictionary = view.call("ReadLayoutRegion",x,y,16,16)
                for j in 16:
                    for i in 16: grid[Vector2i(x+i,y+j)] = int(region.words[j*16+i])
        var entities: Array = state().entities
        var origin := cell()
        var pending: Array[Vector2i] = [origin]
        var previous := {origin:origin}
        var direction := {}
        var offsets := [Vector2i.UP,Vector2i.LEFT,Vector2i.RIGHT,Vector2i.DOWN]
        for here in pending:
            if here == goal: break
            for d in 4:
                var there: Vector2i = here + offsets[d]
                var slope: int = int(grid.get(here,0)) & 0xc000
                if d in [1,2] and slope in [0x4000,0x8000]:
                    var diagonal := Vector2i(there.x, here.y + (-1 if (slope == 0x8000) == (d == 2) else 1))
                    if grid.has(diagonal) and (int(grid[diagonal]) & 0xc000) == slope: there = diagonal
                if not grid.has(there) or previous.has(there) or rejected.has(str(here)+str(d)): continue
                var word: int = grid[there]
                if (word & 0x3c00) == 0x1000 or ((word & 0xc000) == 0xc000 and (word & 0x3c00) != 0x400): continue
                var occupied := false
                for e in entities:
                    if e.id != "entity-0" and e.Visible and e.get("follower") == null and abs(e.x-there.x*384)<128 and abs(e.y-there.y*384)<128: occupied = true
                if occupied: continue
                previous[there] = here; direction[there] = d; pending.append(there)
        if not previous.has(goal):
            samples.append({"label":"route-diagnostic", "origin":str(origin), "goalWord":grid.get(goal), "reachable":previous.keys().map(func(v):return str(v)), "neighbors":offsets.map(func(d):return {"cell":str(origin+d),"word":grid.get(origin+d)}), "rejected":rejected})
            check(false, "reachable priest counter"); record("church-route-blocked"); return
        var next := goal
        while previous[next] != origin: next = previous[next]
        await key([KEY_UP,KEY_LEFT,KEY_RIGHT,KEY_DOWN][direction[next]])
        if not await settle_field(): return
        if cell() == origin: rejected[str(origin)+str(direction[next])] = true
    check(cell() == goal, "ordinary walk reaches priest counter")
    if cell() != goal: return
    await key(KEY_UP); await settle_field(); await key(KEY_ENTER)
    for i in 300:
        if state().wait == "ChurchWait": break
        await process_frame
    check(state().wait == "ChurchWait", "actual priest opens church")
    if state().wait != "ChurchWait": record("church-entry-failure"); return
    check(not state().fieldMenu.visible and not state().partyStatus.visible, "church excludes field pages")
    for d in [KEY_UP,KEY_LEFT,KEY_RIGHT,KEY_DOWN]:
        await key(d); await frames(3)
        diamond(state().church, "church-menu", "church-"+str(d))
    await key(KEY_RIGHT)
    var before := state()
    await key(KEY_ENTER)
    check(not state().church.commands[2].enabled and state().revision == before.revision and state().party == before.party and state().gold == before.gold, "unavailable Promotion preserves state")
    record("church-promotion-refused")
    await key(KEY_LEFT); await key(KEY_ENTER)
    check(state().church.page == "result", "healthy Cure result")
    await key(KEY_ENTER); await key(back)
    await settle_field()
    check(not state().church.visible and state().canWaitAtInput, "church caller returns")
    record("church-return")

func finish() -> void:
    if done: return
    done = true
    var f := FileAccess.open(OS.get_environment("SF2_MENU_ASSETS_OUTPUT"), FileAccess.WRITE)
    f.store_string(JSON.stringify({"case":mode, "expectedArt":expect_art, "pid":OS.get_process_id(), "failures":failures, "samples":samples}, "\t")); f.close()
    if host: host.queue_free()
    await frames(4)
    quit(0 if failures.is_empty() else 1)

func run() -> void:
    create_timer(55).timeout.connect(func(): check(false, "55-second deadline"); finish())
    node_added.connect(node_added_observe)
    root.size = Vector2i(1920,1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
    root.grab_focus(); await frames(5)
    if mode == "field-new":
        await key(KEY_ENTER); await key(KEY_E); await key(KEY_ENTER)
        if not await wait_panel("new-game-slots"): finish(); return
        await key(KEY_ENTER); await key(KEY_ENTER)
        check(panel().get("state") == "confirm-new-game", "New Game final confirmation")
        await key(KEY_DOWN); await key(KEY_ENTER)
        if not await wait_field(): finish(); return
    elif mode == "invalid-scene":
        await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
        if not await wait_panel("continue-slots"): finish(); return
        await key(KEY_ENTER)
        if not await wait_panel("error"): finish(); return
        samples.append({"label":"partial-menu-refused", "panel":panel(), "persistence":persistence()})
        check(view == null, "partial private menu never mounts gameplay")
        finish(); return
    elif mode == "battle-continue":
        if not await continue_title(): finish(); return
    elif mode == "field-continue":
        if not await continue_title(true): finish(); return
    elif mode == "authored":
        if not await wait_battle(): finish(); return
    else:
        check(false, "known mode"); finish(); return
    record("ordinary-entry")
    if mode in ["field-new", "field-continue"]:
        await field()
        if failures.is_empty(): await church()
    else: await battle()
    finish()
