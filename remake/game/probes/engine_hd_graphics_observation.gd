extends "res://probes/host_ui_observation.gd"

# Bounded production-host witness: native input, decoded pixels and ordinary preferences/save.
# The optional controlled input changes initial position only, before any session exists.
var preference_path: String
var slot_path: String

func applied() -> Dictionary: return observation(host, "ReadSettingsHostObservationJson").applied
func progress() -> Variant: return observation(host, "ReadPersistenceObservationJson").progress
func saved_preferences() -> String: return FileAccess.get_file_as_string(preference_path)

func field_settings() -> void:
    await key(KEY_F10)
    for i in range(5):
        if selected_menu() == "Settings": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(find("SettingsView") != null, "ordinary settled field Settings")

func return_field() -> void:
    if find("SettingsView") != null: await cancel_settings()
    if find("SystemMenuView") != null: await key(KEY_ESCAPE)

func set_mode(mode: String) -> void:
    await select_setting("graphics")
    if settings().draft.GraphicsMode != mode: await key(KEY_RIGHT)
    check(settings().draft.GraphicsMode == mode, "graphics draft " + mode)

func apply_mode(mode: String) -> void:
    var before: Variant = progress()
    await set_mode(mode); await select_setting("apply"); await key(KEY_ENTER)
    check(find("SettingsView") == null and applied().GraphicsMode == mode, "Apply publishes " + mode)
    check(progress() == before, "Apply preserves complete save projection")
    check(JSON.parse_string(saved_preferences()).graphicsMode == mode, "real preferences persist " + mode)
    await return_field()

func inventory(label: String, mode: String, expected: String) -> Array:
    await key(KEY_ESCAPE)
    for i in range(5):
        if state().fieldMenu.index == 2: break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(state().partyStatus.visible, label + ": ordinary Item opens member inventory")
    await key(KEY_ENTER); await key(KEY_ENTER); await frames(5)
    var action: Dictionary = state().partyStatus.preparation
    check(action.stage == "use-item", label + ": ordinary inventory Use chooser")
    var first: Array = JSON.parse_string(host.call("ReadIdentityIconPixelsObservationJson"))
    samples.append({"label":label + "-first", "preparation":action, "pixels":first})
    var herb_index := -1
    for i in range(action.page.rows.size()):
        if action.page.rows[i].icon.identity == "identity-icon.item.0": herb_index = i; break
    check(herb_index >= 0, label + ": actual carried herb")
    if herb_index >= 0:
        for i in range(herb_index): await key(KEY_DOWN)
    await frames(4)
    action = state().partyStatus.preparation
    var pixels: Array = JSON.parse_string(host.call("ReadIdentityIconPixelsObservationJson"))
    var uncovered := false
    for row in pixels:
        var icon: Dictionary = row.art
        if icon.identity == "identity-icon.item.0":
            check(icon.mode == mode and icon.variant == expected and icon.resource == icon.drawn, label + ": herb draws actual selected mode")
            check(icon.width == (64 if expected == "hd" else 16) and icon.height == (96 if expected == "hd" else 24), label + ": admitted decoded shape")
        elif icon.identity != null:
            uncovered = true
            check(icon.variant == "original", label + ": uncovered item uses Original")
    check(uncovered, label + ": another actual carried identity")
    samples.append({"label":label + "-herb-selected", "preparation":action, "pixels":pixels,
        "progress":progress(), "applied":applied()})
    if herb_index >= 0:
        var before: Variant = progress()
        var r: Dictionary = action.page.rows[herb_index].rect
        for pressed in [true, false]:
            var e := InputEventMouseButton.new(); e.button_index = MOUSE_BUTTON_LEFT
            e.position = Vector2(r.x + r.width / 2, r.y + r.height / 2); e.pressed = pressed
            Input.parse_input_event(e); await frames(4)
            check(state().partyStatus.preparation.stage == ("use-item" if pressed else "confirm"), label + ": unchanged row hit area and release-only selection")
        await key(KEY_ESCAPE)
        check(state().partyStatus.preparation.stage == "use-item" and state().partyStatus.preparation.index == herb_index and progress() == before,
            label + ": confirmation Back retains slot and all progress")
    for i in range(6):
        if state().fieldMenu.visible: break
        await key(KEY_ESCAPE)
    if state().fieldMenu.visible: await key(KEY_ESCAPE)
    return [first, pixels]

func equal_geometry(a: Array, b: Array) -> void:
    for i in range(min(a.size(), b.size())):
        for j in range(min(a[i].size(), b[i].size())):
            check(a[i][j].art.identity == b[i][j].art.identity and a[i][j].art.resource == b[i][j].art.resource and
                a[i][j].art.rect == b[i][j].art.rect and a[i][j].art.scale == b[i][j].art.scale,
                "mode retains identity, selection, anchor and draw rectangle")

func save_slot() -> void:
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("saved"): return
    check(FileAccess.file_exists(slot_path), "ordinary Save creates disposable slot")
    await key(KEY_ENTER)

func continue_slot() -> void:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): return
    await key(KEY_ENTER)
    for i in range(1800):
        if find("ExplorationSessionView") != null: break
        await process_frame
    check(find("ExplorationSessionView") != null, "fresh ordinary Continue mounts saved field")
    var envelope: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(slot_path))
    var payload: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
    samples.append({"label":"restore-projection", "atAttach":normalized(restored.get("progress")), "saved":payload.data, "now":normalized(progress())})
    check(normalized(restored.get("progress")) == payload.data, "Continue restores every saved progress field at attach")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1703-"):
        push_error("Use a disposable issue1703-* namespace and SF2_HOST_UI_OUTPUT"); quit(2); return
    var namespace_root := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    preference_path = namespace_root.path_join("sf2-preferences/input.json")
    slot_path = namespace_root.path_join("sf2-opening/slot1.json")
    node_added.connect(observe_node)
    root.size = Vector2i(1920,1080); root.position = Vector2i(-20000,-20000)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(10)
    check(root.has_focus(), "native window owns focus")
    samples.append({"label":"startup", "applied":applied()})
    if case_name == "continue":
        check(applied().GraphicsMode == "hd", "relaunch reads saved HD preference")
        var saved_slot := FileAccess.get_file_as_string(slot_path)
        await continue_slot()
        await inventory("continued-hd", "hd", "hd")
        check(FileAccess.get_file_as_string(slot_path) == saved_slot, "Continue and inventory retain exact slot bytes")
        finish(); return
    if case_name == "invalid-authored":
        var entry := observation(find("SessionEntryView"), "ReadEntryObservationJson")
        check(entry.failure.Code == OS.get_environment("SF2_HD_EXPECT_ERROR"), "malformed declared HD pair rejects: " + str(entry.failure))
        check(find("ExplorationSessionView") == null, "invalid declared pair never mounts")
        samples.append({"label":"admission-rejected", "entry":entry}); finish(); return
    if case_name == "public":
        await frames(8)
        check(applied().GraphicsMode == "hd", "public launch retains selected HD preference")
        await key(KEY_ESCAPE); await key(KEY_ENTER)
        check(state().partyStatus.visible, "public ordinary field inventory is visible")
        var entries: Array = state().partyStatus.layout.items.entries
        check(not entries.is_empty(), "public inventory has actual named rows")
        for row in entries: check(not row.text.is_empty() and row.icon.variant == "text" and not row.icon.visible,
            "public named inventory row uses explicit text fallback")
        var icons: Array = JSON.parse_string(host.call("ReadIdentityIconPixelsObservationJson"))
        check(icons.is_empty(), "public profile retains text without private namespace")
        samples.append({"label":"public-text", "state":state(), "pixels":icons})
        finish(); return
    if case_name == "prepare-failure":
        var before_bytes := saved_preferences()
        var before: Variant = progress()
        var revision: int = state().revision
        await field_settings(); await set_mode("hd"); await select_setting("apply"); await key(KEY_ENTER)
        check(find("SettingsView") != null and applied().GraphicsMode == "original" and saved_preferences() == before_bytes and progress() == before and state().revision == revision,
            "native decode refusal preserves prior graphics, preference bytes and progress")
        check("prepared" in settings().message or "准备" in settings().message, "Apply explains resource preparation failure")
        samples.append({"label":"prepare-failure", "settings":settings()}); finish(); return
    await open_witch_settings()
    geometry("SettingsWindow")
    if case_name != "override": await select_setting("graphics")
    if case_name != "override": check("installed artwork" in settings().message or "安装了高清素材" in settings().message, "readable HD coverage in settings")
    if case_name == "override":
        check(settings().readOnly and applied().GraphicsMode == "hd", "read-only launch override selects HD")
        check((find("Setting_apply") as Button).disabled, "override disables Apply")
        await key(KEY_ESCAPE); check(not FileAccess.file_exists(preference_path), "override creates no ordinary preference")
        finish(); return
    if case_name == "defaults":
        await select_setting("defaults"); await key(KEY_ENTER)
        check(settings().draft.GraphicsMode == "original" and applied().GraphicsMode == "hd", "Restore defaults stages Original only")
        await cancel_settings(); await open_witch_settings()
        check(applied().GraphicsMode == "hd", "Cancel defaults preserves HD")
        await select_setting("defaults"); await key(KEY_ENTER); await select_setting("apply"); await key(KEY_ENTER)
        check(applied().GraphicsMode == "original" and JSON.parse_string(saved_preferences()).graphicsMode == "original", "Apply defaults persists Original")
        finish(); return
    check(applied().GraphicsMode == "original", "old version-1 preferences default Original")
    var initial_bytes := saved_preferences()
    await set_mode("hd"); await cancel_settings()
    check(applied().GraphicsMode == "original" and saved_preferences() == initial_bytes, "Witch Cancel keeps graphics and exact old preferences")
    if case_name == "title":
        await open_witch_settings(); await apply_mode("hd")
        check(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START")) == "started",
            "Witch choice carries into a newly mounted session")
        await frames(8); await inventory("witch-choice-first-mount", "hd", "hd")
        finish(); return
    var result: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(result == "started", "ordinary selected world admits controlled initial position: " + result)
    if result != "started": finish(); return
    await frames(8)
    if case_name == "old-private":
        await field_settings(); await apply_mode("hd")
        await inventory("old-pack-hd-preference", "hd", "original")
        finish(); return
    if case_name == "failure":
        await inventory("before-write-failure", "original", "original")
        await field_settings(); var before: Variant = progress()
        var lock_path := namespace_root.path_join("sf2-preferences/input.lock")
        check(DirAccess.make_dir_absolute(lock_path) == OK, "disposable storage fault creates real lock-path refusal")
        await set_mode("hd"); await select_setting("apply"); await key(KEY_ENTER)
        check(find("SettingsView") != null and applied().GraphicsMode == "original" and progress() == before and saved_preferences() == initial_bytes,
            "real write failure preserves graphics, preferences and progress")
        samples.append({"label":"write-failure", "settings":settings()})
        check(DirAccess.remove_absolute(lock_path) == OK, "remove owned empty fault directory")
        await return_field(); await inventory("after-write-failure", "original", "original")
        finish(); return
    var original := await inventory("original", "original", "original")
    await field_settings(); await apply_mode("hd")
    var hd := await inventory("hd", "hd", "hd")
    equal_geometry(original, hd)
    var reopened := await inventory("hd-reopened", "hd", "hd")
    equal_geometry(hd, reopened)
    await field_settings(); await set_mode("original"); await cancel_settings(); await return_field()
    var cancelled := await inventory("hd-after-cancel", "hd", "hd")
    check(cancelled == hd, "Cancel retains mounted HD textures and exact pixels")
    await field_settings(); await apply_mode("original")
    var reverted := await inventory("reverted", "original", "original")
    equal_geometry(original, reverted)
    check(original == reverted, "Original return reuses exact textures and decoded pixels")
    await save_slot()
    var save_bytes := FileAccess.get_file_as_string(slot_path)
    await field_settings(); await set_mode("hd"); await cancel_settings(); await return_field()
    check(applied().GraphicsMode == "original", "field Cancel retains Original")
    await field_settings(); await apply_mode("hd")
    check(FileAccess.get_file_as_string(slot_path) == save_bytes, "graphics Apply never writes gameplay slot")
    samples.append({"label":"ready-to-relaunch", "applied":applied(), "progress":progress()})
    finish()
