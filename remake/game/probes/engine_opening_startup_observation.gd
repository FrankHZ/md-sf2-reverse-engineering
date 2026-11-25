extends SceneTree

# Bounded ordinary-input handoffs; no scene fast-forward or injected gameplay completion.
var host: Node
var helper: Node
var samples: Array = []
var failures: Array = []
var ended := false
var case_name := OS.get_environment("SF2_OPENING_CONTROL")
var began := Time.get_ticks_msec()
var observed_view: Node

func _initialize() -> void:
    call_deferred("run")

func _process(_delta: float) -> bool:
    if not ended and Time.get_ticks_msec()-began > 180000: finish("180-second control deadline")
    return false

func frames(count := 4) -> void:
    for i in range(count): await process_frame

func edge(key: Key, down: bool) -> void:
    var event := InputEventKey.new()
    event.keycode = key; event.physical_keycode = key; event.pressed = down
    Input.parse_input_event(event)

func press(key: Key) -> void:
    edge(key,true); await frames(2); edge(key,false); await frames(3)

func opening() -> Dictionary:
    return JSON.parse_string(host.call("ReadOpeningObservationJson"))

func entry() -> Dictionary:
    var node := host.find_child("SessionEntryView",true,false)
    return JSON.parse_string(node.call("ReadEntryObservationJson")) if node != null else {}

func check(ok: bool, reason: String) -> void:
    if not ok: failures.append(reason)

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    helper = load("res://probes/OpeningStartupControl.cs").new(); root.add_child(helper)
    if case_name == "seeded":
        var restore_deadline := Time.get_ticks_msec()+60000
        while Time.get_ticks_msec() < restore_deadline:
            var restored := host.find_child("ExplorationSessionView",true,false)
            if restored != null:
                check(not opening().active and opening().result == null,"Explicit Continue bypasses opening")
                samples.append({"label":"journey","state":restored.call("ReadCaptureState",true,false),
                    "persistence":JSON.parse_string(host.call("ReadPersistenceObservationJson"))})
                finish(""); return
            await process_frame
        finish("Explicit Continue did not restore"); return
    if case_name == "load":
        check(opening().loading,"Loading boundary reached")
        helper.call("RetainOwner",host)
        edge(KEY_E,true)
    elif case_name not in ["absent","bad","empty"]:
        var deadline := Time.get_ticks_msec()+60000
        while Time.get_ticks_msec() < deadline:
            var view := host.find_child("OpeningScene",true,false)
            if view != null:
                if observed_view != view:
                    observed_view = view
                    view.connect("ResourceDrawObserved",func(): pass)
                    await frames(2)
                var state: Dictionary = view.call("ReadCaptureState",false,false)
                var reached: bool = state.get("fieldText") != null if case_name in ["focus","newgame","continue"] else state.get("scenePaletteWait") != null
                if case_name == "delay":
                    reached = state.get("fieldText") != null and state.fieldText.get("AutomaticDelay",false) and state.fieldText.get("Remaining",0) > 0
                if case_name == "motion":
                    reached = state.get("wait") == "EntityWait" and state.get("entities",[]).any(func(actor): return actor.moving)
                if reached:
                    if case_name == "focus": await focus_control(view)
                    helper.call("RetainOwner",host)
                    samples.append({"label":"skip-boundary","state":state})
                    samples.append({"label":"settings","state":JSON.parse_string(host.call("ReadSettingsHostObservationJson"))})
                    if case_name == "pointer":
                        var button: Control = host.find_child("OpeningSkip",true,false)
                        var at := root.get_stretch_transform()*button.get_global_transform_with_canvas()*(button.size/2)
                        for down in [true,false]:
                            var event := InputEventMouseButton.new()
                            event.position=at; event.global_position=at; event.button_index=MOUSE_BUTTON_LEFT; event.pressed=down
                            Input.parse_input_event(event); await frames(2)
                    else:
                        edge(KEY_RIGHT,true); edge(KEY_ENTER,true); edge(KEY_E,true)
                    break
            if not opening().active: break
            await process_frame
    var title_deadline := Time.get_ticks_msec()+30000
    while Time.get_ticks_msec() < title_deadline:
        if not entry().is_empty(): break
        await process_frame
    await frames(8)
    var expected := "absent" if case_name == "absent" else "failed" if case_name in ["bad","empty"] else "skipped"
    check(opening().result == expected,"Expected handoff " + expected)
    check(entry().get("title",false),"Skip leaves independent title")
    check(opening().journey == null,"Opening allocated no journey")
    samples.append({"label":"title-held","opening":opening(),"entry":entry()})
    edge(KEY_E,false); edge(KEY_ENTER,false); edge(KEY_RIGHT,false); await frames(8)
    check(entry().get("title",false),"Release does not activate title")
    if case_name not in ["absent","bad","empty"]:
        helper.call("DeliverRetiredCompletion",host)
        var late_deadline := Time.get_ticks_msec()+30000
        while not helper.call("RetiredLoadCompleted") and Time.get_ticks_msec() < late_deadline: await process_frame
        check(helper.call("RetiredLoadCompleted"),"Retired detached load actually completed")
        await frames(8)
        check(opening().result == "skipped" and entry().get("title",false),"Late loading and duplicate completion cannot replace title")
    var voices := host.find_children("*","AudioStreamPlayer",true,false)
    check(voices.size() == 1,"Only title music owner remains")
    check(entry().get("audio",{}).get("musicCue") == "MUSIC_TITLE","Title owns command28")
    await press(KEY_ENTER); await frames(8)
    if case_name == "back":
        edge(KEY_E,true); await frames(2)
        check(entry().get("latched",false),"Held Start latches Witch menu before release")
        edge(KEY_E,false)
    else: await press(KEY_E)
    if not await wait_entry_neutral("menu","before-back"):
        finish(""); return
    samples.append({"label":"witch","entry":entry()})
    await press(KEY_ESCAPE)
    if not await wait_entry_neutral("title","after-back"):
        finish(""); return
    check(entry().get("title",false) and not opening().active,"Witch Back returns without replay")
    if case_name in ["newgame","continue"]:
        await press(KEY_ENTER); await press(KEY_E)
        if await wait_entry_neutral("menu","before-journey"): await journey()
    finish("")

func wait_entry_neutral(stage: String, label: String) -> bool:
    var started := Time.get_ticks_msec()
    var before := entry_boundary()
    var current := before
    while Time.get_ticks_msec()-started < (30000 if stage == "menu" else 2000):
        if current.entry.get("stage") == stage and not current.entry.get("latched",true) and not current.entry.get("activated",true) and not current.input.held and not current.input.releaseBarrier and current.focused:
            samples.append({"label":label,"before":before,"ready":current,"elapsedMs":Time.get_ticks_msec()-started})
            return true
        await process_frame
        current = entry_boundary()
    samples.append({"label":label,"before":before,"last":current,"elapsedMs":Time.get_ticks_msec()-started})
    check(false,"Entry did not reach released " + stage + " at " + label)
    return false

func entry_boundary() -> Dictionary:
    var input_state: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
    return {"entry":entry(),"opening":opening(),"focused":root.has_focus(),
        "input":{"held":input_state.held,"releaseBarrier":input_state.releaseBarrier,
        "enter":Input.is_physical_key_pressed(KEY_ENTER),"start":Input.is_physical_key_pressed(KEY_E),
        "back":Input.is_physical_key_pressed(KEY_ESCAPE)}}

func focus_control(view: Node) -> void:
    root.gui_embed_subwindows = false
    var focus_window := Window.new()
    focus_window.title="SF2 bounded focus recovery check"
    focus_window.size=Vector2i(360,160)
    root.add_child(focus_window); focus_window.popup_centered(); focus_window.grab_focus()
    await frames(8)
    var before: Dictionary = view.call("ReadCaptureState",false,false)
    check(not root.has_focus(),"Actual native window lost focus")
    await frames(45)
    var paused: Dictionary = view.call("ReadCaptureState",false,false)
    check(before.simulationTick == paused.simulationTick,"Focus loss pauses scene time")
    check(opening().audio.musicPaused,"Focus loss pauses opening music")
    samples.append({"label":"focus-paused","tick":paused.simulationTick,"opening":opening()})
    focus_window.queue_free(); await frames(3); root.grab_focus(); await frames(45)
    var resumed: Dictionary = view.call("ReadCaptureState",false,false)
    check(root.has_focus() and resumed.simulationTick > paused.simulationTick,"Same owner resumes after native focus returns")
    check(not opening().audio.musicPaused,"Focus return resumes opening music")

func slot_state() -> Dictionary:
    var node := host.find_child("SaveSlotView",true,false)
    return JSON.parse_string(node.call("ReadSaveObservationJson")) if node != null else {}

func wait_slot(expected: String) -> bool:
    var deadline := Time.get_ticks_msec()+60000
    while Time.get_ticks_msec() < deadline:
        if slot_state().get("state") == expected: return true
        await process_frame
    samples.append({"label":"slot-failure","slot":slot_state()})
    check(false,"Slot boundary " + expected); return false

func journey() -> void:
    var selected := "Continue" if case_name == "continue" else "NewGame"
    if entry().get("selectedAction") != selected: await press(KEY_DOWN if case_name == "continue" else KEY_UP)
    await press(KEY_ENTER)
    if entry().get("continueText") == 225: await press(KEY_ENTER)
    if not await wait_slot("continue-slots" if case_name == "continue" else "new-game-slots"): return
    await press(KEY_ENTER)
    if case_name == "newgame":
        await frames(8)
        helper.call("SetNewGameSeeds",host)
        await press(KEY_ENTER)
        if not await wait_slot("confirm-new-game"): return
        await press(KEY_DOWN); await press(KEY_ENTER)
    var deadline := Time.get_ticks_msec()+60000
    while Time.get_ticks_msec() < deadline:
        if entry().get("continueText") in [226, 224]:
            await press(KEY_ENTER)
        var view := host.find_child("ExplorationSessionView",true,false)
        if view != null:
            samples.append({"label":"journey","state":view.call("ReadCaptureState",true,false),
                "persistence":JSON.parse_string(host.call("ReadPersistenceObservationJson"))})
            return
        await process_frame
    check(false,"Ordinary journey did not mount")

func finish(reason: String) -> void:
    if ended: return
    ended=true
    if reason != "": failures.append(reason)
    var file := FileAccess.open(OS.get_environment("SF2_OPENING_OUTPUT"),FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"case":case_name,"failures":failures,
        "elapsedMs":Time.get_ticks_msec()-began,"samples":samples},"  ")); file.close()
    if is_instance_valid(helper): helper.queue_free()
    if is_instance_valid(host): host.queue_free()
    observed_view = null; helper = null; host = null
    await process_frame; await process_frame
    quit(0 if failures.is_empty() else 1)
