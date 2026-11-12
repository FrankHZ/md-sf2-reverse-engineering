extends "res://probes/host_ui_observation.gd"

# Current preference/layout and Yeel/house consumer; campaign traversal has its own observer.
# Finite actual host inputs. Checkpoint input is admitted before a session exists.
# Samples retain only the changed state/UI, never map payloads or per-frame dumps.
var preference_path: String
var slot_path: String
var guide_seen := false

func applied() -> Dictionary: return observation(host, "ReadSettingsHostObservationJson").applied
func progress() -> Variant: return observation(host, "ReadPersistenceObservationJson").progress
func guidance() -> Dictionary: return state().get("guidance", {})
func player(s: Dictionary) -> Dictionary:
    for e in s.get("entities", []):
        if e.id == "entity-0": return e
    return {}
func capture(label: String) -> void:
    var s := state()
    var menu: Dictionary = s.get("fieldMenu", {}).duplicate(true)
    for command in menu.get("menu", {}).get("commands", []): command.erase("art")
    for row in menu.get("menu", {}).get("list", {}).get("rows", []): row.erase("art")
    if not menu.get("visible", false):
        menu = {"visible":false, "index":menu.get("index")}
    var text: Variant = s.get("fieldText")
    if text is Dictionary:
        text = {"phase":text.Phase, "index":text.Index, "end":text.End, "projection":text.Projection, "token":text.Token}
    samples.append({"label":label, "map":s.get("map"), "flags":s.get("flags"), "player":player(s),
        "wait":s.get("wait"), "failure":s.get("failure"), "guidance":guidance(), "fieldMenu":menu, "worldGeometry":s.get("worldGeometry"),
        "text":text, "textId":s.get("textId"), "canWaitForText":s.get("canWaitForText"),
        "dialoguePanel":s.get("dialoguePanel"), "help":s.get("help"), "musicWait":s.get("musicWait"),
        "settings":settings(), "applied":applied()})

func select_setting(index: int) -> void:
    for i in range(15):
        if settings().selected == index: return
        await key(KEY_DOWN)
    check(false, "setting focus " + str(index))

func field_settings() -> void:
    await key(KEY_F10)
    check(not guidance().get("visible", false), "host Menu immediately hides world guidance")
    for i in range(5):
        if selected_menu() == "Settings": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(find("SettingsView") != null, "ordinary field Settings")

func return_field() -> void:
    if find("SettingsView") != null: await key(KEY_ESCAPE)
    if find("SystemMenuView") != null: await key(KEY_ESCAPE)
    await frames(4)

func set_guidance(on: bool, apply_: bool = true) -> void:
    await field_settings(); await select_setting(10)
    if settings().draft.ModernGuidance != on: await key(KEY_RIGHT)
    if apply_: await select_setting(7); await key(KEY_ENTER)
    else: await key(KEY_ESCAPE)
    await return_field()

func settle(label: String) -> Dictionary:
    var began := Time.get_ticks_msec()
    var text_checkpoint := -1
    for i in range(6500):
        await process_frame
        var s := state()
        if s.get("failure") != null: check(false, label + ": " + str(s.failure)); capture(label); return s
        if s.has("stage"): return s
        if s.get("textId") != null and int(s.textId) != text_checkpoint:
            # Each reached dialogue is a new live checkpoint, still under the host's55s total deadline.
            text_checkpoint = int(s.textId); began = Time.get_ticks_msec()
            samples.append({"label":label + ":text", "id":text_checkpoint, "wait":s.wait,
                "guidanceHidden":not guidance().get("visible", false)})
        if s.get("canWaitAtInput", false) and not player(s).get("moving", true): return s
        if s.get("canWaitForText", false):
            check(not guidance().get("visible", false), label + ": dialogue hides guidance")
            await key(KEY_ENTER)
        elif s.get("wait") in ["FieldTextWait", "W1TextWait", "DialogueWait"] and i % 2 == 0:
            # The production reader accepts fresh Reveal/Continue intent before its logical end.
            await key(KEY_ENTER)
        if Time.get_ticks_msec() - began > 30000: break
    check(false, label + ": 30s checkpoint bound"); capture(label); return state()

func navigate(goal: Vector2i, label: String, expected_map: String = "") -> Dictionary:
    var s := state(); var origin_map: String = s.map
    var rejected := {}
    for movement in range(150):
        if s.has("stage") or not failures.is_empty(): return s
        var origin := Vector2i(int(player(s).x) / 384, int(player(s).y) / 384)
        if origin == goal: return s
        var words: Array = []
        var view := find("ExplorationSessionView")
        for y in range(0, 64, 4): words.append_array(view.call("ReadLayoutRegion", 0, y, 64, 4).words)
        var queue: Array[Vector2i] = [origin]
        var paths: Dictionary = {origin:[]}; var selected: Array = []
        while not queue.is_empty():
            var at: Vector2i = queue.pop_front()
            for d in [Vector2i.UP, Vector2i.LEFT, Vector2i.DOWN, Vector2i.RIGHT]:
                var n: Vector2i = at + d
                if n.x < 0 or n.y < 0 or n.x >= 64 or n.y >= 64 or paths.has(n) or rejected.has(str([at,d])): continue
                var word: int = int(words[n.y * 64 + n.x]); var marker := word & 0x3C00
                if (word & 0xC000) == 0xC000 and marker != 0x400 and n != goal: continue
                if marker == 0x1000 and n != goal: continue
                if s.entities.any(func(e): return e.id != "entity-0" and e.Visible and (int(e.flagsA) & 0x80) != 0 and absi(int(e.x) - n.x * 384) < 256 and absi(int(e.y) - n.y * 384) < 256): continue
                paths[n] = paths[at] + [d]
                if n == goal: selected = paths[n]; queue.clear(); break
                queue.append(n)
        if selected.is_empty(): check(false, label + ": no existing layout path"); return s
        var d: Vector2i = selected[0]
        await key(KEY_UP if d == Vector2i.UP else KEY_DOWN if d == Vector2i.DOWN else KEY_LEFT if d == Vector2i.LEFT else KEY_RIGHT)
        s = await settle(label)
        if s.has("stage") or s.map != origin_map:
            check(s.has("stage") or s.map == expected_map, label + ": actual map handoff"); capture(label); return s
        if Vector2i(int(player(s).x) / 384, int(player(s).y) / 384) == origin: rejected[str([origin,d])] = true
    check(false, label + ": movement bound"); return s

func menu_check(expected: bool) -> void:
    var aid := guidance().duplicate(true)
    await key(KEY_ESCAPE)
    # Compare at the same paused menu boundary; free-field ambient ticks may consume RNG after Back.
    var before: Variant = progress()
    check(state().fieldMenu.visible and guidance().get("visible", false) == expected, "root menu retains the independent objective or Off/coverage hiding")
    check(guidance().panelId == aid.panelId and guidance().text == aid.text and guidance().objectiveKey == aid.objectiveKey,
        "opening root menu retains the same panel and complete current objective")
    check(not "Message unavailable" in guidance().text, "objective has valid localized operands")
    var frozen := state()
    capture("gameplay-menu")
    if expected:
        var layout: Dictionary = state().fieldMenu.menu
        check(layout.rect.x >= 32 and layout.rect.y >= 32 and layout.rect.x + layout.rect.width <= root.size.x - 32 and
            layout.rect.y + layout.rect.height <= root.size.y - 32, "complete command group stays in safe bounds")
        var r: Dictionary = guidance().rect
        check(not Rect2(r.x,r.y,r.width,r.height).intersects(Rect2(layout.rect.x,layout.rect.y,layout.rect.width,layout.rect.height)),
            "independent objective avoids actual command bounds")
        check_player_clear()
        check(not guidance().text in layout.reason and not guidance().markerVisible, "command help has no duplicate objective or world marker")
        check(guidance().fontSize == 30 and find("FieldGuidance").find_children("ObjectivePanel", "Panel", true, false).size() == 1,
            "exactly one passive objective panel keeps30px body")
        var initial_index: int = state().fieldMenu.index
        await key(KEY_RIGHT); await key(KEY_DOWN)
        check(state().fieldMenu.index == (initial_index + 1) % 5, "passive objective never takes command direction focus")
        await key(KEY_UP)
        if initial_index == 0:
            await key(KEY_K if applied().Bindings.has("confirm") else KEY_ENTER)
            check(state().partyStatus.visible and not state().fieldMenu.visible and not guidance().get("visible", false), "Member covers both aids")
            await key(KEY_ESCAPE)
            check(state().fieldMenu.visible and state().fieldMenu.index == initial_index and guidance().visible == expected,
                "Party Back retains command and restores the same objective")
    check(state().revision == frozen.revision and state().flags == frozen.flags and state().party == frozen.party and
        state().mainSeed == frozen.mainSeed and state().thinkingSeed == frozen.thinkingSeed, "reading/Party input preserves live revision, story, party and RNG")
    var after: Variant = progress()
    if after != before: samples.append({"label":"menu-progress-difference", "before":before, "after":after})
    check(after == before, "query/menu/Party/Back do not alter save progress")
    await key(KEY_ESCAPE)
    check(not state().fieldMenu.visible and guidance().visible == expected and guidance().panelId == aid.panelId and guidance().text == aid.text,
        "closing root menu retains the same independent objective")

func check_player_clear() -> void:
    var s := state(); var p := player(s); var g: Dictionary = s.worldGeometry; var r: Dictionary = guidance().rect
    var actor := Rect2((Vector2(p.x,p.y) / 16 - Vector2(g.cameraX,g.cameraY)) * 4,Vector2(96,96)).grow(24)
    check(not Rect2(r.x,r.y,r.width,r.height).intersects(actor), "objective avoids actual projected player with24px clearance")

func town_flow() -> void:
    var knows_hawel: bool = 610.0 in state().flags
    check(guidance().get("objectiveKey") == ("guidance.find-hawel" if knows_hawel else null) and
        guidance().visible == knows_hawel, "before invitation retain only the disclosed Hawel task")
    check(not guidance().get("markerVisible", false) and not "Kazin" in str(guidance().get("text", "")) and
        not "WWWWWWW" in str(guidance().get("text", "")),
        "before guide disclosure no companion name or invitation entrance marker")
    await menu_check(knows_hawel)
    await key(KEY_ENTER)
    for i in range(400):
        if state().get("canWaitForText", false): break
        await process_frame
    var flags: Array = state().flags
    await key(KEY_ESCAPE)
    check(state().flags == flags and not 72.0 in state().flags and not guidance().visible, "Cancel cannot complete guide dialogue")
    await settle("kazin-invitation")
    check(72.0 in state().flags and guidance().visible and guidance().get("objectiveKey") == "guidance.visit-hawel",
        "real caller changes the disclosed goal to visiting Hawel with the guide")
    var declared: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_HOST_UI_START"))).start.get("names", {})
    if declared.has("ally-4"):
        await key(KEY_ESCAPE)
        check(declared["ally-4"] in guidance().text, "admitted seven-character custom companion name is preserved")
        await key(KEY_ESCAPE)
    capture("after-invitation")
    await menu_check(true)
    var bytes := FileAccess.get_file_as_string(preference_path)
    await set_guidance(false, false)
    check(applied().ModernGuidance and guidance().visible and FileAccess.get_file_as_string(preference_path) == bytes, "Cancel retains On and exact preferences")
    await set_guidance(false)
    check(not applied().ModernGuidance and not guidance().visible, "Apply Off hides destination immediately")
    await menu_check(false)
    check(not JSON.parse_string(FileAccess.get_file_as_string(preference_path)).modernGuidance, "Off persisted")
    var at := player(state())
    await key(KEY_DOWN); await settle("off-ordinary-move")
    check(player(state()).y != at.y, "Off retains ordinary movement")
    await set_guidance(true)
    check(guidance().visible, "On republishes live destination")
    await field_settings(); await select_setting(10); await key(KEY_RIGHT)
    var lock_path := preference_path.get_base_dir().path_join("input.lock")
    check(DirAccess.rename_absolute(lock_path, lock_path + ".retained") == OK, "retain prior owned lock file")
    check(DirAccess.make_dir_absolute(lock_path) == OK, "owned actual write refusal")
    var previous := FileAccess.get_file_as_string(preference_path)
    await select_setting(7); await key(KEY_ENTER)
    check(find("SettingsView") != null and applied().ModernGuidance and FileAccess.get_file_as_string(preference_path) == previous, "failed Apply retains both aids' applied preference and bytes")
    capture("write-refused"); check(DirAccess.remove_absolute(lock_path) == OK, "remove owned empty fault directory")
    check(DirAccess.rename_absolute(lock_path + ".retained", lock_path) == OK, "restore prior owned lock file")
    await return_field()
    for size_ in [Vector2i(1920,1080), Vector2i(2560,1440)]:
        root.size = size_; await frames(6)
        check(guidance().visible and guidance().mouseFilter == "Ignore", "supported layout has passive destination")
        var r: Dictionary = guidance().rect
        check(r.x >= 32 and r.y >= 32 and r.x + r.width <= size_.x - 32 and r.y + r.height <= size_.y - 32, "destination remains in safe edge")
        var before := player(state())
        for pressed in [true,false]:
            var e := InputEventMouseButton.new(); e.button_index = MOUSE_BUTTON_LEFT; e.pressed = pressed
            e.position = Vector2(r.x + 24, r.y + 24); Input.parse_input_event(e); await frames()
        check(player(state()).x == before.x and player(state()).y == before.y, "destination click never travels")
        capture("layout-" + str(size_))
    root.size = Vector2i(1280,720); await frames(5)
    check(window().recovery != null and not guidance().get("visible", false), "small-window recovery hides guidance")
    root.size = Vector2i(1920,1080); await frames(5)
    check(guidance().visible, "recovery requeries destination")
    await field_settings(); await select_setting(4); await key(KEY_RIGHT); await select_setting(7); await key(KEY_ENTER); await return_field()
    check(("村外" in guidance().text) == (applied().Language == "zh-CN"), "live locale switch refreshes destination width and copy")
    await menu_check(true)
    check(applied().GraphicsMode == "hd", "guidance and language preserve #1703 HD choice")
    await pad(JOY_BUTTON_B)
    check(state().fieldMenu.visible, "mapped controller Cancel opens gameplay menu")
    var focused: int = state().fieldMenu.index
    await pad(JOY_BUTTON_DPAD_DOWN)
    check(state().fieldMenu.index == (focused + 1) % 5 and guidance().visible, "controller directions belong only to current menu while objective remains")
    await pad(JOY_BUTTON_B)
    await navigate(Vector2i(7,1), "visible-north-approach")
    await frames(8)
    check(guidance().markerVisible, "actual visible north exit has a named hollow marker")
    capture("visible-town-exit")
    var s := await navigate(Vector2i(7,0), "yeel-north-exit", "map-66")
    check(s.map == "map-66" and guidance().visible and not guidance().markerVisible and ("Route not available" in guidance().text or "暂无可用路线" in guidance().text), "real transfer drops old town anchor; incomplete leg has no bearing")
    capture("overworld-incomplete")

func compact_flow() -> void:
    for size_ in [Vector2i(1920,1080), Vector2i(2560,1440)]:
        root.size = size_; await frames(6)
        var aid := guidance()
        check(aid.visible and aid.mouseFilter == "Ignore" and aid.fontSize == 30, "supported compact objective is readable and passive")
        var label := find("FieldGuidance").find_child("Destination", true, false) as Label
        var natural_width := 0.0
        for line in label.text.split("\n"):
            natural_width = maxf(natural_width, label.get_theme_font("font").get_string_size(line,HORIZONTAL_ALIGNMENT_LEFT,-1,30).x)
        check(absf(aid.rect.width - (minf(448,natural_width) + 32)) < 2, "short copy shrinks to measured text; long copy caps at480px")
        check(aid.lines > label.text.split("\n").size() if natural_width > 449 else aid.lines == label.text.split("\n").size(),
            "long copy wraps naturally without reducing font or dropping objective/destination/action")
        var r: Dictionary = aid.rect
        check(Rect2(Vector2(32,32),Vector2(size_) - Vector2(64,64)).encloses(Rect2(r.x,r.y,r.width,r.height)), "full wrapped panel stays within safe edges")
        var text_rect: Dictionary = aid.textRect
        check(r.height >= text_rect.height + 24 and r.width >= text_rect.width + 32, "panel contains every wrapped line with padding")
        check_player_clear()
        await menu_check(true); await menu_check(true)
        capture("compact-" + str(size_))
    root.size = Vector2i(1280,720); await frames(5)
    check(window().recovery != null and not guidance().visible, "below-minimum recovery hides compact objective")
    root.size = Vector2i(1920,1080); await frames(5)
    await set_guidance(false); await menu_check(false)
    check(not guidance().visible, "Off hides independent objective with root closed")
    await set_guidance(true); await menu_check(true)

func house_flow() -> void:
    check(72.0 in state().flags and 503.0 in state().flags and guidance().visible, "retained post-battle checkpoint has current objective")
    capture("post-battle-approach"); await menu_check(true)
    check(guidance().markerVisible, "visible validated house entrance has hollow named marker")
    await edge(KEY_SHIFT, true)
    var s := await navigate(Vector2i(6,5), "house-entry", "map-43")
    await edge(KEY_SHIFT, false)
    check(s.map == "map-43" and 4.0 in s.flags and 612.0 in s.flags and not 72.0 in s.flags and
        guidance().visible and guidance().get("objectiveKey") == "guidance.ancient-shrine-goal" and
        guidance().get("routeKey") == "guidance.leave-house" and not guidance().markerVisible,
        "actual house scene replaces the guide/entrance with the disclosed Ancient Shrine goal")
    capture("house-completed"); await menu_check(true)

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1705-"):
        push_error("Use disposable issue1705-* preferences and explicit output"); quit(2); return
    var namespace_root := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    preference_path = namespace_root.path_join("sf2-preferences/input.json")
    slot_path = namespace_root.path_join("sf2-opening/slot1.json")
    node_added.connect(observe_node)
    root.size = Vector2i(1920,1080); root.position = Vector2i(-20000,-20000)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(10)
    check(root.has_focus(), "native window input owner")
    if case_name == "continue":
        check(not applied().ModernGuidance, "restart reads explicit Off")
        await key(KEY_ESCAPE); await select_setting(6); await key(KEY_ENTER)
        check(settings().draft.ModernGuidance and not applied().ModernGuidance, "Restore defaults stages On only")
        await key(KEY_ESCAPE)
        check(not applied().ModernGuidance, "Cancel defaults preserves explicit Off")
        await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
        if await wait_slots("continue-slots"): await key(KEY_ENTER)
        for i in range(1800):
            if find("ExplorationSessionView") != null: break
            await process_frame
        await settle("ordinary-continue")
        check(not applied().ModernGuidance and not guidance().visible, "Continue never restores old preference")
        capture("continued-off")
        var previous_session: String = state().sessionId
        var original_slot := FileAccess.get_file_as_string(slot_path)
        var preferences := FileAccess.get_file_as_string(preference_path)
        check(not FileAccess.file_exists(slot_path.replace("slot1.json", "slot2.json")), "New Game uses an empty owned second slot")
        await key(KEY_F10)
        for i in range(5):
            if selected_menu() == "NewGame": break
            await key(KEY_DOWN)
        await key(KEY_ENTER); await key(KEY_UP); await key(KEY_ENTER)
        if await wait_slots("new-game-slots"):
            await key(KEY_DOWN); await key(KEY_ENTER); await frames(4); await key(KEY_ENTER)
            if await wait_slots("confirm-new-game"): await key(KEY_DOWN); await key(KEY_ENTER)
        for i in range(1800):
            if state().get("sessionId") != previous_session: break
            await process_frame
        check(state().get("sessionId") != previous_session and not applied().ModernGuidance and not guidance().visible,
            "real New Game keeps explicit Off without old session guidance")
        check(FileAccess.get_file_as_string(slot_path) == original_slot and FileAccess.get_file_as_string(preference_path) == preferences,
            "New Game retains old slot and preference bytes")
        capture("new-game-off"); finish(); return
    if case_name == "override":
        await key(KEY_ESCAPE)
        check(settings().readOnly and not applied().ModernGuidance and (find("Setting10") as Button).disabled, "read-only override disables guidance editing")
        await key(KEY_ESCAPE)
    var result: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(result == "started", "ordinary selected content admits initial checkpoint: " + result)
    if result != "started": finish(); return
    await settle("initial")
    if case_name == "compact": await compact_flow()
    elif case_name == "town": await town_flow()
    elif case_name == "house": await house_flow()
    elif case_name == "override":
        var bytes := FileAccess.get_file_as_string(preference_path)
        await key(KEY_ENTER); await settle("override-invitation")
        check(72.0 in state().flags and not guidance().visible, "real invitation retains read-only Off")
        await menu_check(false)
        check(FileAccess.get_file_as_string(preference_path) == bytes, "read-only guidance override never publishes preferences")
    elif case_name == "revisit":
        check(state().map == "map-43" and 4.0 in state().flags and 612.0 in state().flags and not 72.0 in state().flags, "admitted completed F612 house revisit")
        check(guidance().visible and guidance().get("objectiveKey") == "guidance.ancient-shrine-goal" and
            guidance().get("routeKey") == "guidance.leave-house" and not guidance().markerVisible,
            "F612 revisit retains the Ancient Shrine goal without the old guide/entrance")
        await menu_check(true)
        var flags: Array = state().flags
        await key(KEY_UP); await settle("revisit-move")
        check(state().flags == flags and guidance().visible and guidance().get("objectiveKey") == "guidance.ancient-shrine-goal" and
            not guidance().markerVisible, "revisit movement retains current goal without replaying the scene or restoring the old marker")
        capture("revisit")
    elif case_name == "save":
        await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
        if await wait_slots("save-slots"):
            await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
            if await wait_slots("saved"): await key(KEY_ENTER)
        check(FileAccess.file_exists(slot_path), "already eligible ordinary save succeeds")
        var saved := FileAccess.get_file_as_string(slot_path)
        await set_guidance(false)
        check(FileAccess.get_file_as_string(slot_path) == saved, "guidance never writes gameplay slot")
        capture("saved-then-off")
    finish()

func finish() -> void:
    if ended: return
    if host != null and find("ExplorationSessionView") != null: capture("terminal")
    ended = true
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file != null:
        file.store_string(JSON.stringify({"passed":failures.is_empty(), "checks":checks, "failures":failures,
            "case":case_name, "samples":samples})); file.close()
    quit(0 if failures.is_empty() else 1)
