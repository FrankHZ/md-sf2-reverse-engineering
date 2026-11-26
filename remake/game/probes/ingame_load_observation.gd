extends "res://probes/numbered_save_review.gd"

# Disposable production-host inputs only. No session setters, screenshots or player files.
func run() -> void:
    var space := OS.get_environment("SF2_SAVE_NAMESPACE")
    storage = OS.get_environment("SF2_SAVE_ROOT").path_join(space).path_join("sf2-opening")
    if output.is_empty() or not space.begins_with("issue2286-") or space.get_file() != space \
            or not storage.is_absolute_path() or not DirAccess.dir_exists_absolute(storage):
        push_error("Select a fresh issue2286-* disposable namespace and output")
        quit(2)
        return
    root.size = Vector2i(1920, 1080)
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(6)
    await key(KEY_E)
    if not require(host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START")) == "started", "controlled initial field admission"):
        finish(); return
    if case_name == "battle":
        await battle_load()
        finish(); return
    if not await settled(): finish(); return
    if case_name == "story":
        await key(KEY_LEFT)
        await frames(30)
        await key(KEY_ENTER)
        for i in range(600):
            if state().get("canWaitForText", false): break
            await process_frame
        if require(state().get("canWaitForText", false), "ordinary NPC Talk reaches text"):
            await key(KEY_F10)
            check((find("SystemLoad") as Button).disabled, "story text refuses Load")
            check(not (find("ReasonLoad") as Label).text.is_empty(), "busy Load explains refusal")
            var paused := gameplay()
            sample("story-load-refused")
            await edge(KEY_ESCAPE, true)
            await frames(20)
            check(gameplay() == paused, "held return does not acknowledge story")
            await edge(KEY_ESCAPE, false)
        finish(); return
    if not await open_save(): finish(); return
    if not await choose("2"): finish(); return
    if not await publish(): finish(); return
    var original := slot_bytes(2)
    var initial := state()
    await key(KEY_RIGHT)
    await key(KEY_F10)
    for i in range(300):
        if find("SystemMenuView") != null: break
        await process_frame
    await frames(3)
    var before := gameplay()
    check(before.sessionId == initial.sessionId, "ordinary movement retains session")
    await choose_system_action("Load")
    if not await wait_slots("confirm-load"): finish(); return
    check(slots().selected == 0, "unsaved progress confirmation defaults Cancel")
    await key(KEY_ENTER)
    check(selected_menu() == "Load" and gameplay() == before, "warning Cancel preserves exact old session and Load focus")
    if not await load_history(): finish(); return
    await key(KEY_ESCAPE)
    check(selected_menu() == "Load" and gameplay() == before, "history Cancel preserves exact old session and Load focus")
    for kind in ["future", "incompatible"]:
        var envelope: Dictionary = JSON.parse_string(original.get_string_from_utf8())
        var record: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
        if kind == "future": record.formatVersion = 99
        else: record.data.compatibility.rules = "incompatible-load-control"
        var raw := JSON.stringify(record)
        envelope.payload = Marshalls.utf8_to_base64(raw)
        envelope.sha256 = raw.sha256_text().to_upper()
        write_bytes("slot2.json", JSON.stringify(envelope).to_utf8_buffer())
        if not await load_history(): finish(); return
        await choose("2")
        if not await wait_slots("error"): finish(); return
        check(gameplay() == before, kind + " record preserves old session")
        sample(kind + "-record")
        await key(KEY_ESCAPE)
    write_bytes("slot2.json", original)
    # A file changed after listing must be reread, not restored from the row cache.
    if not await load_history(): finish(); return
    write_bytes("slot2.json", "{".to_utf8_buffer())
    await choose("2")
    if not await wait_slots("error"): finish(); return
    check(gameplay() == before, "changed record does not replace or mutate old session")
    sample("changed-record")
    await key(KEY_ESCAPE)
    if not await load_history(): finish(); return
    await choose("2")
    if not await wait_slots("error"): finish(); return
    check(gameplay() == before, "damaged record leaves old session usable")
    sample("damaged-record")
    await key(KEY_ESCAPE)
    write_bytes("slot2.bak", original)
    if not await load_history(): finish(); return
    await choose("2")
    if not await wait_slots("recover-backup"): finish(); return
    check(slots().selected == 0, "backup recovery defaults Cancel")
    await key(KEY_ENTER)
    check(slot_bytes(2) == "{".to_utf8_buffer() and gameplay() == before, "recovery Cancel preserves storage and old session")
    if not await load_history(): finish(); return
    await choose("2")
    if not await wait_slots("recover-backup"): finish(); return
    await key(KEY_DOWN)
    var old_field := find("ExplorationSessionView")
    var old_battle := find("BattleSessionView")
    restored = {}
    await edge(KEY_ENTER, true)
    if not await replacement(before.sessionId): finish(); return
    check(not is_instance_valid(old_field) and not is_instance_valid(old_battle), "old field and battle projections released")
    check(slot_bytes(2) == original, "explicit backup recovery publishes original bytes")
    check(find("SessionEntryView") == null and find("SystemMenuView") == null and find("SaveSlotView") == null, "loaded gameplay bypasses title and Witch")
    var held := gameplay()
    await frames(15)
    check(gameplay().progress == held.progress and state().get("wait") == null, "held confirmation cannot move or interact in new gameplay")
    await edge(KEY_ENTER, false)
    check(state().map == initial.map and state().gold == initial.gold, "restored original field map and gold")
    samples.append({"label": "restored", "state": state(), "persistence": observation(host, "ReadPersistenceObservationJson"), "audio": observation(host, "ReadAudioObservationJson")})
    # A valid primary takes the same path, with no recovery or write.
    await key(KEY_F10)
    var clean := state()
    await choose_system_action("Load")
    if slots().get("state") == "confirm-load":
        await key(KEY_DOWN)
        await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): finish(); return
    await choose("2")
    if not await replacement(clean.sessionId): finish(); return
    check(slot_bytes(2) == original, "valid primary read never rewrites save")
    await key(KEY_RIGHT)
    check(state().sessionId != before.sessionId, "new session accepts ordinary input after release")
    finish()

func load_history() -> bool:
    await choose_system_action("Load")
    if not await wait_slots("confirm-load"): return false
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    return await wait_slots("continue-slots")

func replacement(old_id: String) -> bool:
    for i in range(2400):
        if state().get("sessionId", old_id) != old_id and find("SaveSlotView") == null: return true
        await process_frame
        OS.delay_msec(1)
    sample("replacement-timeout")
    return require(false, "replacement completed")

func write_bytes(name_: String, bytes: PackedByteArray) -> void:
    var file := FileAccess.open(storage.path_join(name_), FileAccess.WRITE)
    file.store_buffer(bytes)
    file.close()

func gameplay() -> Dictionary:
    var persistence := observation(host, "ReadPersistenceObservationJson")
    return {"sessionId": persistence.sessionId, "revision": persistence.revision, "progress": persistence.progress}

func battle_load() -> void:
    var story_checked := false
    for i in range(2400):
        if state().get("stage") == "Movement" and state().get("movement") == null: break
        if state().get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]:
            if not story_checked:
                await key(KEY_F10)
                check((find("SystemLoad") as Button).disabled, "story dialogue refuses Load")
                check(not (find("ReasonLoad") as Label).text.is_empty(), "busy Load explains refusal")
                sample("story-load-refused")
                await key(KEY_ESCAPE)
                story_checked = true
            await key(KEY_ENTER)
        await process_frame
        OS.delay_msec(1)
    if not require(state().get("stage") == "Movement", "ordinary connected battle reaches player control"): return
    await key(KEY_ENTER)
    await key(KEY_F10)
    check(state().stage == "ActionChoice" and (find("SystemLoad") as Button).disabled, "battle action selection refuses Load")
    sample("battle-busy")
    await key(KEY_ESCAPE)
    await key(KEY_ESCAPE)
    if not require(state().get("stage") == "Movement", "cancel selection returns to player Movement"): return
    await key(KEY_F10)
    check(not (find("SystemLoad") as Button).disabled, "settled battle offers Load")
    await choose_system_action("Save")
    if not await wait_slots("save-slots"): return
    await choose("2")
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_slots("saved"): return
    await key(KEY_ENTER)
    var saved := gameplay()
    var old_view := find("BattleSessionView")
    var bytes := slot_bytes(2)
    await key(KEY_F10)
    await choose_system_action("Load")
    if not await wait_slots("continue-slots"): return
    await key(KEY_ESCAPE)
    check(selected_menu() == "Load" and gameplay() == saved, "battle history Cancel preserves player turn")
    await choose_system_action("Load")
    if not await wait_slots("continue-slots"): return
    await key(KEY_DOWN)
    await edge(KEY_ENTER, true)
    if not await replacement(saved.sessionId): return
    check(not is_instance_valid(old_view), "old battle view released after prepared replacement")
    check(gameplay().progress == saved.progress, "exact battle save payload restored before input")
    var held := gameplay()
    await frames(20)
    check(gameplay() == held and state().stage == "Movement", "held Confirm does not choose new battle action")
    check(find("SessionEntryView") == null and slot_bytes(2) == bytes, "battle load bypasses Witch without writing")
    sample("battle-loaded")
    await edge(KEY_ENTER, false)
    await key(KEY_ENTER)
    check(state().stage == "ActionChoice", "fresh Confirm works after held-key release")
