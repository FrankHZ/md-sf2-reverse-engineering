extends "res://probes/engine_pre_voyage_guidance_observation.gd"

# Bounded host consumer; use --headless so no player's window or input focus is touched.
# The route begins at an explicitly controlled pre-exorcism start, then uses real inputs.
func _initialize() -> void:
    create_timer(180).timeout.connect(func(): check(false, "180 second boundary"); finish())
    call_deferred("run")

func focus_native() -> void:
    pass

func persist() -> Dictionary:
    return observation(host, "ReadPersistenceObservationJson")

func witness(label: String) -> void:
    var s := state()
    print(label, " ", s.get("map"), " ", s.get("wait"))
    var p := persist()
    var summary := {"sessionId":p.get("sessionId"), "revision":p.get("revision"), "availability":p.get("availability")}
    if label in ["church-outside", "slot-published"]: summary.progress = p.get("progress")
    if label == "save-refused": summary.entityWork = p.get("entityWork")
    var entities: Array = []
    for e in s.get("entities", []):
        entities.append({"id":e.id, "visible":e.Visible, "slot":e.slot, "sprite":e.sprite,
            "x":e.x, "y":e.y, "facing":e.facing, "moving":e.moving, "follower":e.follower})
    samples.append({"label":label, "map":s.get("map"), "player":player(s), "flags":s.get("flags"),
        "entities":entities, "wait":s.get("wait"), "failure":s.get("failure"),
        "persistence":summary, "slots":slots()})

func write_report() -> void:
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(), "checks":checks, "failures":failures,
        "case":case_name, "samples":samples}, "\t"))
    file.close()

func finish() -> void:
    if ended: return
    ended = true
    write_report()
    quit(0 if failures.is_empty() else 1)

func stable_save() -> bool:
    for i in range(800):
        var p := persist()
        if p.get("availability", {}).get("Allowed", false): return true
        if p.get("availability", {}).get("Error") not in ["save-entity-unsettled", "save-story-motion"]: break
        await key(KEY_V)
    witness("save-refused")
    check(false, "settled save available")
    return false

func save_slot(second: bool, quit_after: bool) -> void:
    check(slots().get("state") == "save-slots", "real slot panel")
    if second: await key(KEY_DOWN)
    await key(KEY_ENTER)
    check(slots().get("state") == "confirm-save", "explicit slot confirmation before publication")
    witness("confirm-save")
    await key(KEY_DOWN)
    if quit_after: await key(KEY_DOWN)
    if quit_after:
        witness("save-and-quit-intent")
        write_report()
    await key(KEY_ENTER)
    if quit_after: return
    if await wait_slots("saved"):
        witness("slot-published")
        await key(KEY_ENTER)

func title_continue(second: bool = false) -> void:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if await wait_slots("continue-slots"):
        witness("continue-slots")
        if second: await key(KEY_DOWN)
        await key(KEY_ENTER)
    for i in range(3000):
        if find("ExplorationSessionView") != null: break
        await process_frame
    await settle("continue")
    witness("continued")

func church_record() -> void:
    # Face the priest across the counter; (32,12) is not a walkable destination.
    await walk(Vector2i(32,13), "church-priest-approach")
    witness("priest-approach")
    await key(KEY_UP); await settle("face-priest")
    witness("priest-facing")
    await key(KEY_ENTER)
    for i in range(3000):
        var s := state()
        if s.get("church", {}).get("visible", false): break
        if s.get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        else: await frames(3)
    check(state().get("church", {}).get("visible", false), "commission church caller opens services")
    witness("church-services")
    if not state().get("church", {}).get("visible", false): return
    for i in range(3): await key(KEY_DOWN)
    await key(KEY_ENTER)
    for i in range(3000):
        if slots().get("state") == "save-slots": break
        if state().get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        else: await frames(3)
    check(slots().get("state") == "save-slots", "Record returns through shared eligibility and real slots")
    witness("church-record-slots")
    await save_slot(true, false)

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1711-"):
        push_error("Use explicit output and issue1711-* namespace"); quit(2); return
    var namespace_root := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    slot_path = namespace_root.path_join("sf2-opening/slot1.json")
    node_added.connect(observe_node)
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(10)
    samples.append({"label":"background-owner", "headless":DisplayServer.get_name(), "focused":root.has_focus()})
    if case_name in ["continue", "legacy", "church"]:
        var before := FileAccess.get_file_as_string(slot_path)
        await title_continue()
        var payload: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(JSON.parse_string(before).payload))
        var expected: Dictionary = payload.data
        expected.flags = expected.flags.filter(func(f): return f < 256 or f > 383)
        samples.append({"label":"attach-progress", "persistence":restored})
        check(JSON.stringify(normalized(restored.get("progress"))) == JSON.stringify(expected),
            "fresh Continue preserves all progress except map-entry flags")
        if case_name != "legacy":
            check(state().map == "map-3" and int(player(state()).x) == 32*384 and int(player(state()).y) == 13*384,
                "church anchor restored")
            check(609 in state().flags and 610 in state().flags and not 3 in state().flags, "commission retained; Jaha not recruited early")
            check(state().entities.any(func(e): return e.id == "entity-3" and e.Visible), "F609 Jaha present")
            check(state().entities.filter(func(e): return e.Visible and e.get("follower") != null).size() == 2,
                "two ordinary followers reconstructed")
            check(state().entities.any(func(e): return e.id == "entity-142" and e.Visible and int(e.y) == 27*384),
                "F609 entity142 remains its NPC instead of default messenger retirement")
        if case_name == "church": await church_record()
        else:
            var old := player(state()).duplicate()
            await key(KEY_DOWN); await settle("continued-move")
            check(player(state()).x != old.x or player(state()).y != old.y, "Continue accepts actual movement")
            if case_name == "continue":
                await walk(Vector2i(3,23), "toward-west-zone")
                # (2,23) is the source's first-match RTS exception; y24 selects the join.
                await walk(Vector2i(2,24), "actual-jaha-event")
                check(3 in state().flags and 35 in state().flags, "ordinary next Jaha join commits once")
                witness("jaha-joined")
            check(FileAccess.get_file_as_string(slot_path) == before, "play leaves copied source slot unchanged")
        witness("continued-play")
        finish(); return
    var started: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(started == "started", "pre-session controlled admission: " + started)
    if started != "started":
        samples.append({"label":"start-failure", "entry":observation(find("SessionEntryView"), "ReadEntryObservationJson")})
        finish(); return
    await settle("start")
    witness("initial")
    if case_name == "route":
        await walk(Vector2i(27,52), "exorcism-and-commission", "map-19")
        if state().get("canWaitForChoice", false):
            witness("commission-choice")
            await key(KEY_LEFT); await key(KEY_ENTER); await settle("accept-commission")
        check(state().map == "map-19" and 609 in state().flags and 610 in state().flags,
            "real exorcism and commission complete")
        await walk(Vector2i(-1,31), "castle-exit", "map-3")
        await walk(Vector2i(32,13), "church-anchor")
    await frames(30)
    witness("church-outside")
    if case_name == "baseline":
        check(persist().availability.Error == "save-location-unavailable", "accepted baseline reproduces settled Map3 refusal")
        await key(KEY_F10)
        check((find("SystemSave") as Button).disabled, "baseline system Save agrees with policy")
        witness("baseline-menu-refusal"); finish(); return
    if not await stable_save(): finish(); return
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if await wait_slots("save-slots"): await save_slot(false, true)
    # Only the ordinary Save and Quit path may end a successful writer process.
    for i in range(6000): await process_frame
    check(false, "Save and Quit did not exit"); finish()
