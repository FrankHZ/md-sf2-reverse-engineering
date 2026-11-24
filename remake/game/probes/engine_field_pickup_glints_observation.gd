extends "res://probes/engine_pre_voyage_guidance_observation.gd"

# Inspect actual draw commands and play through the normal host input routes.
func _initialize() -> void:
    create_timer(45).timeout.connect(func():
        if ended: return
        check(false, "45 second deadline")
        samples.append({"label":"deadline", "entry":observation(find("SessionEntryView"), "ReadEntryObservationJson"),
            "slots":slots(), "menu":selected_menu()})
        finish())
    call_deferred("run")

func marks(s: Dictionary) -> Array:
    var draws: Variant = s.get("pickupGlints")
    return draws if draws is Array else []

func mark_at(s: Dictionary, x: int, y: int) -> Dictionary:
    for mark in marks(s):
        if int(mark.x) == x and int(mark.y) == y: return mark
    return {}

func power_water_count(s: Dictionary) -> int:
    var count := 0
    for member in s.get("party", []):
        for item in member.get("SourceLoadout", {}).get("Items", []):
            if (int(item) & 127) == 9: count += 1
    return count

func witness(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label, "map":s.get("map"), "player":player(s), "marks":marks(s),
        "search":s.get("search"), "searchBlock":s.get("searchBlock"), "flags":s.get("flags"),
        "error":s.get("failure"), "fieldMenu":s.get("fieldMenu", {}).get("visible"),
        "camera":s.get("cameraProjection", {}).get("legalScreen") if s.get("cameraProjection") is Dictionary else null})
    check(not s.is_empty() and s.get("failure") == null, label + ": live field without error")
    return s

func check_search(kind: int, flag: int, received: bool) -> Dictionary:
    await key(KEY_ENTER)
    var s := witness("search")
    var receipt: Variant = s.get("search")
    check(s.get("wait") == "FieldSearchWait" and receipt is Dictionary and int(receipt.Kind) == kind,
        "actual Search result " + str(kind))
    check((flag in s.flags) == received and marks(s).is_empty(), "receipt flag and modal coverage")
    return s

func dismiss_search() -> void:
    for i in range(8):
        if state().get("wait") != "FieldSearchWait": break
        await key(KEY_ENTER)
    check(state().get("canWaitAtInput", false), "Search returns usable control")

func controlled_start() -> bool:
    var result: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(result == "started", "controlled private admission: " + result)
    if result != "started": return false
    await settle("field admission")
    return await enable_draw_observation()

func enable_draw_observation() -> bool:
    var field := find("ExplorationSessionView")
    check(field != null and state().get("canWaitAtInput", false), "mounted field control")
    if field == null: return false
    field.call("ReadObservationJson")
    await frames(4)
    return true

func continue_slot(index: int) -> bool:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): return false
    for i in range(index): await key(KEY_DOWN)
    await key(KEY_ENTER)
    var start := Time.get_ticks_msec()
    while find("ExplorationSessionView") == null and Time.get_ticks_msec() - start < 9000:
        if slots().get("state") == "error": break
        await process_frame
    if find("ExplorationSessionView") == null:
        var detail := find("SlotDetail") as Label
        check(false, "Continue mount: " + (detail.text if detail != null else str(slots())))
        return false
    await settle("ordinary title Continue")
    return await enable_draw_observation()

func wait_slots(expected: String) -> bool:
    for i in range(480):
        var state_name: String = slots().get("state", "")
        if state_name == expected: return true
        if state_name == "error": break
        await frames(3)
    var detail := find("SlotDetail") as Label
    check(false, "Expected slot state " + expected + ": " + (detail.text if detail != null else str(slots())))
    return false

func shrine() -> void:
    if not await continue_slot(0): return
    var before := witness("full-unclaimed")
    var mark := mark_at(before, 19, 24)
    check(before.map == "map-42" and 133 not in before.flags and not mark.is_empty(),
        "full pack still shows the unclaimed Power Water chest")
    await frames(12)
    var next := mark_at(state(), 19, 24)
    check(not mark.is_empty() and not next.is_empty() and mark.opacity != next.opacity, "ordinary soft pulse")
    await key(KEY_ESCAPE)
    var menu := witness("field-menu")
    check(menu.fieldMenu.visible and marks(menu).is_empty(), "field commands cover the glint")
    await key(KEY_ESCAPE)
    await field_settings()
    await select_setting("guidance")
    check("glints on unclaimed items" in settings().message and marks(state()).is_empty(),
        "short settings description and host coverage")
    await key(KEY_RIGHT)
    await select_setting("apply"); await key(KEY_ENTER); await return_field()
    check(not applied().ModernGuidance and marks(witness("guidance-off")).is_empty(), "applied Off hides glints")
    var full := await check_search(4, 133, false)
    check(full.party == before.party and int(full.searchBlock) == 0xD801, "Full preserves packs and closed chest")
    await dismiss_search()
    await field_settings()
    await select_setting("guidance"); await key(KEY_RIGHT)
    await select_setting("flash"); await key(KEY_RIGHT)
    await select_setting("apply"); await key(KEY_ENTER); await return_field()
    var steady := mark_at(witness("reduced-flash-full-retry"), 19, 24)
    await frames(12)
    check(applied().ReducedFlash and not steady.is_empty() and steady == mark_at(state(), 19, 24),
        "reduced flash retains a steady mark after Full")
    await drop_herb()
    if not failures.is_empty(): return
    var granted := await check_search(2, 133, true)
    check(int(granted.search.ItemWord) == 9 and int(granted.searchBlock) == 0xD802 and
        power_water_count(granted) == power_water_count(before) + 1, "Search grants one Power Water and opens chest")
    await dismiss_search()
    check(marks(witness("collected")).is_empty(), "collected chest stays clear at ordinary input")
    var repeat := await check_search(0, 133, true)
    check(repeat.party == granted.party, "repeat Search grants nothing")
    await dismiss_search()
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("confirm-save"): return
    await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("saved"): return
    await key(KEY_ENTER); await frames(4)
    check(FileAccess.file_exists(slot_path.replace("slot1.json", "slot2.json")), "ordinary Save wrote disposable slot 2")

func drop_herb() -> void:
    await key(KEY_E); await key(KEY_ENTER)
    for i in range(4): await key(KEY_DOWN)
    await key(KEY_ENTER)
    var pane: Dictionary = state().partyStatus.preparation
    check(pane.stage == "drop-item", "ordinary Drop opens pack chooser")
    if pane.stage != "drop-item": return
    await key(KEY_DOWN); await key(KEY_ENTER); await key(KEY_ENTER)
    check(state().partyStatus.preparation.stage == "result", "ordinary Drop commits once")
    await key(KEY_ENTER)
    for i in range(4):
        if not state().partyStatus.visible and not state().fieldMenu.visible: break
        await key(KEY_ESCAPE)

func cleared_shrine() -> void:
    if not await continue_slot(1): return
    var restored_field := witness("fresh-continued-after-grant")
    check(restored_field.map == "map-42" and 133 in restored_field.flags and power_water_count(restored_field) == 1 and
        marks(restored_field).is_empty(), "fresh Continue keeps receipt and custody with no glint")
    var repeat := await check_search(0, 133, true)
    check(repeat.party == restored_field.party, "Continue repeat keeps inventory")
    await dismiss_search()
    await walk(Vector2i(19, 16), "real CURRENT stair", "map-42")
    var hall := witness("upper-hall")
    check(hall.map == "map-42" and int(player(hall).x) == 2 * 384 and int(player(hall).y) == 8 * 384,
        "real stair switches the visible room")
    var field := find("ExplorationSessionView")
    var locked: Dictionary = field.call("ReadLayoutRegion", 4, 3, 1, 1)
    check((int(locked.words[0]) & 0x3C00) != 0x1800 and 901 not in hall.flags and 198 not in hall.flags and marks(hall).is_empty(),
        "unreleased item in the visible hall stays unmarked")
    await walk(Vector2i(7, 15), "real hall exit", "map-41")
    check(witness("shrine-exterior").map == "map-41" and marks(state()).is_empty(), "changed map has no stale marks")

func foreground() -> void:
    if not await controlled_start(): return
    var s := witness("prison-hidden-item")
    var mark := mark_at(s, 56, 4)
    check(not mark.is_empty() and marks(s).size() == 1 and 205 not in s.flags, "hidden item in the current prison area")
    for direction in [KEY_UP, KEY_LEFT, KEY_LEFT]:
        await key(direction); await settle("face hidden item")
    var result := await check_search(2, 205, true)
    check(int(result.search.ItemWord) == 126, "hidden Search gives the actual item")
    await dismiss_search()
    check(marks(witness("hidden-item-collected")).is_empty(), "claimed hidden item stops drawing")

func hold_foreground() -> void:
    if not await controlled_start(): return
    var field := find("ExplorationSessionView")
    var request: int = field.call("ObserveLayoutRegion", 58, 22, 1, 1)
    await frames(4)
    var s := witness("docks-hold")
    var origin: Dictionary = s.cameraProjection.background
    var layout: Dictionary = field.call("ReadLayoutRegion", 58, 22, 1, 1)
    var draws: Array = layout.draw.uses
    # The upper stroke crosses opaque atlas ink at source pixel (12,12).
    var tip := (Vector2(58 * 24 + 12.5, 9 * 24 + 15.5) - Vector2(origin.x, origin.y)) * 4 + Vector2(1, -8.5)
    check(int(layout.draw.request) == request and draws.any(func(d):
        return (d.layer == "foreground" and d.highPriority == true and int(d.block) == 201 and
            Rect2(d.x, d.y, d.width, d.height).has_point(tip))),
        "live high-priority A-plane draw crosses the B-plane glint's upper stroke")
    check(140 not in s.flags and mark_at(s, 58, 9).is_empty(), "foreground suppresses the entire unclaimed hold glint")
    samples.append({"label":"hold-foreground", "word":layout.words[0], "draws":draws})
    field.call("StopLayoutObservation")

func tower() -> void:
    if not await controlled_start(): return
    var field := find("ExplorationSessionView")
    var request: int = field.call("ObserveLayoutRegion", 15, 6, 1, 1)
    await frames(4)
    var s := witness("unclaimed-tower-mithril")
    var layout: Dictionary = field.call("ReadLayoutRegion", 15, 6, 1, 1)
    var draws: Array = layout.draw.uses.filter(func(d): return d.layer == "foreground")
    var origin: Dictionary = s.cameraProjection.foreground
    var star := Rect2((Vector2(15 * 24 + 8, 6 * 24 + 11) - Vector2(origin.x, origin.y)) * 4, Vector2(36, 36))
    var clip: Dictionary = s.cameraProjection.legalScreen
    check(s.map == "map-40" and 132 not in s.flags and int(layout.words[0]) == 0xDC42 and
        Rect2(clip.x, clip.y, clip.width, clip.height).encloses(star), "unclaimed Mithril and full star footprint in the active clip")
    check(int(layout.draw.request) == request and draws.any(func(d):
        return (d.highPriority == true and int(d.tile) == 7 and int(d.words[0]) == 0x8140 and
            Rect2(d.x, d.y, d.width, d.height).has_point(star.position + Vector2(18, 22)))),
        "the later A-plane high-priority tile crosses a lower star stroke")
    check(mark_at(s, 15, 6).is_empty(), "terrain suppresses the entire Mithril glint")
    samples.append({"label":"tower-foreground", "starBounds":star, "word":layout.words[0], "draws":draws})
    field.call("StopLayoutObservation")
    var result := await check_search(2, 132, true)
    check(int(result.get("search", {}).get("ItemWord", -1)) == 123, "ordinary Search still grants Mithril")
    await dismiss_search()
    check(mark_at(witness("tower-mithril-collected"), 15, 6).is_empty(), "claimed Mithril stays clear")

func wall() -> void:
    if not await controlled_start(): return
    var s := witness("wall-between-player-and-chest")
    var field := find("ExplorationSessionView")
    var layout: Dictionary = field.call("ReadLayoutRegion", 33, 23, 3, 1)
    check((int(layout.words[0]) & 0xC000) == 0xC000 and (int(layout.words[1]) & 0x3C00) == 0x1800,
        "live wall separates a still-marked chest in this map area")
    check(mark_at(s, 34, 23).is_empty() and mark_at(s, 35, 23).is_empty(), "same-area collision conceals the treasure beyond the wall")

func docks() -> void:
    if not await controlled_start(): return
    var s := witness("docks-a-plane")
    check(s.map == "map-44" and not mark_at(s, 27, 6).is_empty(), "A-plane barrel draws in the active view")
    check(mark_at(s, 58, 9).is_empty(), "lower hold pickup stays hidden from the deck")
    var result := await check_search(2, 139, true)
    check(int(result.search.ItemWord) == 11, "docks barrel grants Quick Chicken")

func run() -> void:
    var save_namespace := OS.get_environment("SF2_SAVE_NAMESPACE")
    if output.is_empty() or (save_namespace != "issue2166" and not save_namespace.begins_with("issue2024-")):
        push_error("Use an explicit report and a disposable issue2024-* or issue2166 namespace"); quit(2); return
    slot_path = OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot1.json")
    node_added.connect(observe_node)
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(10)
    match case_name:
        "shrine": await shrine()
        "continue": await cleared_shrine()
        "foreground": await foreground()
        "hold-foreground": await hold_foreground()
        "tower": await tower()
        "wall": await wall()
        "docks": await docks()
        _: check(false, "unknown case")
    if issue != "": check(false, issue)
    finish()

func finish() -> void:
    if ended: return
    ended = true
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file != null:
        file.store_string(JSON.stringify({"passed":failures.is_empty(), "checks":checks, "failures":failures,
            "case":case_name, "samples":samples}))
        file.close()
    call_deferred("close_observer", 0 if failures.is_empty() and file != null else 1)

func close_observer(code: int) -> void:
    if is_instance_valid(host): host.queue_free()
    await frames(12)
    quit(code)
