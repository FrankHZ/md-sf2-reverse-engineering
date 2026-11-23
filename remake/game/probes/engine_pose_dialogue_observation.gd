extends "res://probes/engine_world_viewport_observation.gd"

# Expected actor, field sprite, logical portrait, drawn portrait and portrait flags.
const PRISON := {
    919: ["entity-5", 59, null, 5, 0],
    921: ["entity-5", 59, null, 5, 0],
    923: ["entity-5", 59, null, 5, 0],
    928: ["entity-5", 14, 5, 5, 0],
    931: ["", null, null, -1, 0],
    945: ["entity-5", 59, null, 5, 192],
    946: ["entity-1", 2, 1, 1, 0],
}
const SHIP := {
    2249: ["entity-3", 8, 3, 3, 0],
    2250: ["entity-3", 59, null, 3, 0],
    2251: ["entity-3", 59, null, 3, 0],
    2252: ["entity-5", 14, 5, 5, 0],
}

func _initialize() -> void:
    create_timer(180).timeout.connect(func():
        if is_instance_valid(view):
            var last: Dictionary = view.call("ReadCaptureWitness", false)
            samples.append({"textId": last.textId, "wait": last.wait, "failure": last.failure})
        fail("180-second simulation deadline"))
    call_deferred("run")

func run() -> void:
    if not check(case_name in ["prison", "ship"], "Select prison or ship case"): return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    var started: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    if not check(started == "started", "Admit controlled field start: " + started): return
    await frames(6)
    state()
    var expected: Dictionary = PRISON if case_name == "prison" else SHIP
    var seen := {}
    if case_name == "prison":
        if not await wait_for(func(s): return s.canWaitAtInput, "prison before door zone"): return
        key_event(KEY_UP, true)
        if not await wait_for(func(s): return s.textId == 901, "ordinary door-zone entry"): return
        key_event(KEY_UP, false)
    # This is the normal mapped acceleration input. Each dialogue confirmation is a fresh press.
    key_event(KEY_SHIFT, true)
    while not ended:
        var progress: Dictionary = view.call("ReadCaptureWitness", false)
        if not check(progress.failure == null, "Healthy field session: " + str(progress.failure)): return
        if seen.size() == expected.size() and (case_name == "ship" or progress.canWaitAtInput):
            key_event(KEY_SHIFT, false)
            if case_name == "prison":
                await frames(3)
                var final := state()
                var doors_open: bool = final.flags.any(func(flag): return int(flag) == 661)
                samples.append({"atFieldInput":final.canWaitAtInput, "doorsOpen":doors_open,
                    "panelVisible":final.dialoguePanel.visible, "texture":final.dialoguePanel.portraitTexture})
                if not check(final.canWaitAtInput and doors_open and not final.dialoguePanel.visible and
                    final.dialoguePanel.portraitTexture == null, "Door flag and fresh field control without a mounted portrait"): return
            finish(true)
            return
        if progress.canWaitForText:
            var id := int(progress.textId)
            if expected.has(id) and not seen.has(id):
                await frames(3)
                if not observe_portrait(id, expected[id]): return
                seen[id] = true
            if not await tap(KEY_ENTER): return
        else:
            await process_frame

func observe_portrait(id: int, expected: Array) -> bool:
    var s := state()
    var panel: Dictionary = s.dialoguePanel
    var drawn: Dictionary = s.get("portraitProjection") if s.get("portraitProjection") != null else {}
    var sprite = null
    for actor in s.entities:
        if actor.id == expected[0]:
            sprite = actor.sprite
            break
    samples.append({"textId":s.textId, "actor":expected[0], "sprite":sprite, "logicalPortrait":s.portraitId,
        "portraitWindow":s.portraitWindow, "drawnPortrait":drawn.get("id"), "flags":drawn.get("flags"),
        "mirrored":drawn.get("mirrored"), "portraitVisible":panel.portraitVisible, "texture":panel.portraitTexture,
        "speakerName":panel.speakerName, "failure":s.failure})
    var portrait_visible: bool = expected[3] >= 0
    return check(s.textId == id and s.failure == null and panel.visible and sprite == expected[1] and
        s.portraitId == expected[2] and drawn.get("id") == expected[3] and drawn.get("flags") == expected[4] and
        panel.portraitVisible == portrait_visible and (panel.portraitTexture != null) == portrait_visible and
        (not portrait_visible or drawn.get("mirrored") == ((expected[4] & 0x40) != 0)) and
        panel.speakerName == null, "Text %d uses the expected sprite and portrait projection" % id)
