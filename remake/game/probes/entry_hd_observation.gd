extends "res://probes/engine_hd_graphics_observation.gd"

func entry() -> Dictionary:
    return observation(find("SessionEntryView"), "ReadEntryObservationJson")

func capture_entry(label: String, expected: String) -> void:
    var row := entry()
    check(row.assetError == null, label + ": entry assets admitted")
    check(row.variant == expected, label + ": coherent variant")
    check(row.artRect.width == 1024 and row.artRect.height == 896, label + ": logical geometry")
    var pixels := []
    for layer in row.drawnTextures:
        var texture := instance_from_id(int(layer.texture)) as Texture2D
        check(texture != null and layer.width == (1024 if expected == "hd" else 256) and
            layer.height == (896 if expected == "hd" else 224), label + ": texture density")
        var context := HashingContext.new()
        context.start(HashingContext.HASH_SHA256)
        context.update(texture.get_image().get_data())
        pixels.append({"name":layer.name, "sha256":context.finish().hex_encode()})
    samples.append({"label":label, "entry":row, "pixels":pixels})

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue2179-"):
        push_error("Use issue2179-* disposable namespace and SF2_HOST_UI_OUTPUT")
        quit(2); return
    preference_path = OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host)
    await frames(6)
    if case_name == "invalid":
        check(entry().assetError == "entry-content-unavailable", "bad declaration rejected at mount")
        samples.append({"entry":entry()}); finish(); return
    var fallback := case_name in ["legacy", "partial"]
    var expected := "original" if fallback or case_name == "switch" else "hd"
    capture_entry("title", expected)
    check((entry().numeralOffset == 0 and entry().numeralOpacity == 1) if applied().ReducedFlash else
        (entry().numeralOffset > 0 and entry().numeralOpacity < 1), "title numeral obeys transition/Reduced Flash")
    check(entry().artRect.x == 448 and entry().artRect.y == 92, "title anchor")
    await key(KEY_ENTER)
    var seen := {}
    for i in range(230):
        seen[entry().artFrame] = true
        await process_frame
    await key(KEY_ENTER)
    for i in range(220):
        seen[entry().artFrame] = true
        await process_frame
    samples.append({"transitionFrames":seen})
    if not applied().ReducedFlash:
        check(seen.has("witch-arrival") and seen.has("witch-blink"), "arrival and blink on existing clock")
    await key(KEY_E)
    capture_entry("witch", expected)
    check(entry().artRect.x == 160 and entry().artRect.y == 92, "Witch anchor")
    if case_name == "switch":
        for mode in ["hd", "original", "hd"]:
            await open_witch_settings()
            await apply_mode(mode)
            await frames()
            capture_entry("applied-" + mode, mode)
        await open_witch_settings()
        await select_setting("text"); await key(KEY_RIGHT)
        await select_setting("apply"); await key(KEY_ENTER)
        await key(KEY_ESCAPE)
        capture_entry("title-persisted-hd", "hd")
        await key(KEY_ENTER)
        for i in range(4):
            if entry().page != 0: break
            await key(KEY_ENTER)
        var speaking := false
        for i in range(220):
            speaking = speaking or entry().artFrame == "witch-mouth"
            await process_frame
        check(speaking, "adjustable dialogue draws HD mouth on existing clock")
        await key(KEY_E)
        await open_witch_settings()
        await select_setting("flash"); await key(KEY_RIGHT)
        await select_setting("apply"); await key(KEY_ENTER)
        check(applied().ReducedFlash, "Reduced Flash applied")
        for i in range(220):
            check(entry().artFrame == "witch", "Reduced Flash holds base Witch")
            await process_frame
    finish()
