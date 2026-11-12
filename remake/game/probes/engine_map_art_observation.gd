extends "res://probes/host_ui_observation.gd"

# Use a disposable copy of a compatible ordinary field slot and installed review pack.
func inspect_blocks(view: Node, density: int) -> Dictionary:
    var bounds := Rect2i(26, 9, 12, 8)
    var request: int = view.call("ObserveLayoutRegion", bounds.position.x, bounds.position.y, bounds.size.x, bounds.size.y)
    var info: Dictionary
    for i in range(120):
        await process_frame
        info = view.call("ReadLayoutRegion", bounds.position.x, bounds.position.y, bounds.size.x, bounds.size.y)
        if info.draw != null and info.draw.request == request: break
    var rows: Array = [] if info.get("draw") == null else info.draw.uses
    check(not rows.is_empty(), "actual visible block draws")
    var ids := {}
    var geometry := []
    for row in rows:
        check(row.selector.pixels == density, "actual raster width for logical cell")
        ids[str(row.block)] = row.resourceIdentity
        geometry.append([row.sourceX, row.sourceY, row.tile, row.pass, row.x, row.y, row.width, row.height])
    samples.append({"pixels":density, "draws":rows.size(), "blocks":ids.size(), "map":info.map})
    view.call("StopLayoutObservation")
    return {"ids":ids, "geometry":geometry}

func inspect_party_portrait(expected: int) -> void:
    await key(KEY_ESCAPE)
    await key(KEY_ENTER)
    var face := find("MemberPortrait") as TextureRect
    check(face != null and face.is_visible_in_tree() and face.texture != null, "member sheet displays portrait")
    if face != null and face.texture != null:
        check(face.texture.get_width() == expected and face.size == Vector2(144, 192),
            "member portrait selected at constant display size")
        samples.append({"memberPortrait":face.texture.get_width(), "display":str(face.size)})
    for i in range(4):
        if not state().partyStatus.visible and not state().fieldMenu.visible: break
        await key(KEY_ESCAPE)

func change_mode(mode: String) -> void:
    await key(KEY_F10)
    for i in range(6):
        if selected_menu() == "Settings": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    var settings_view := find("SettingsView")
    check(settings_view != null, "ordinary exploration settings")
    if settings_view == null: return
    for i in range(20):
        if settings().selected == 5: break
        await key(KEY_DOWN)
    if settings().draft.GraphicsMode != mode: await key(KEY_RIGHT)
    for i in range(20):
        if settings().selected == 7: break
        await key(KEY_DOWN)
    var paused: Variant = observation(host, "ReadPersistenceObservationJson").progress
    await key(KEY_ENTER)
    check(find("SettingsView") == null, "Apply accepted")
    check(paused == observation(host, "ReadPersistenceObservationJson").progress, "Apply preserves paused progress")
    if find("SystemMenuView") != null: await key(KEY_ESCAPE)
    await frames(4)

func run() -> void:
    if not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1717-check-"):
        push_error("select a disposable review slot"); quit(2); return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(12)
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): finish(); return
    await key(KEY_ENTER)
    for i in range(1800):
        if find("ExplorationSessionView") != null: break
        await process_frame
    var view := find("ExplorationSessionView")
    check(view != null, "ordinary Continue mounted Map3")
    if view == null: finish(); return
    await frames(8)
    var before: Dictionary = await inspect_blocks(view, 24)
    await inspect_party_portrait(48)
    var progress: Variant = observation(host, "ReadPersistenceObservationJson").progress
    await change_mode("hd")
    var painted: Dictionary = await inspect_blocks(view, 96)
    await inspect_party_portrait(192)
    await change_mode("original")
    var after: Dictionary = await inspect_blocks(view, 24)
    await inspect_party_portrait(48)
    check(before.ids == after.ids and before.ids != painted.ids, "distinct HD and restored original textures")
    check(before.geometry == painted.geometry and after.geometry == before.geometry, "same visible tile passes and screen geometry")
    var now: Variant = observation(host, "ReadPersistenceObservationJson").progress
    var changed := []
    for k in progress:
        if now.get(k) != progress.get(k): changed.append(k)
    check(changed.all(func(k): return k == "MainSeed"), "only ordinary field time advances across menu visits")
    samples.append({"fieldTimeChanges":changed})
    finish()
