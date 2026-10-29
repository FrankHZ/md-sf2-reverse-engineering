extends SceneTree

# Small actual-host geometry/input witness. No session setters, screenshots or frame dumps.
var host: Node
var view: Node
var failures: Array = []
var unavailable: Array = []
var samples: Array = []
var began := Time.get_ticks_msec()
var ended := false
var recovered := false
var case_name := OS.get_environment("SF2_VIEWPORT_CASE")

func _initialize() -> void:
    create_timer(75).timeout.connect(func(): fail("75-second observer deadline"))
    call_deferred("run")

func fail(message: String) -> bool:
    if not ended:
        failures.append(message)
        finish()
    return false

func check(ok: bool, message: String) -> bool:
    return true if ok else fail(message)

func focus() -> bool:
    if ended: return false
    if not root.has_focus():
        if recovered:
            unavailable.append("Native focus lost after one recovery")
            finish(true)
            return false
        recovered = true
        root.grab_focus()
        var deadline := Time.get_ticks_msec() + 1000
        while not root.has_focus() and Time.get_ticks_msec() < deadline: await process_frame
        if not root.has_focus():
            unavailable.append("Native focus unavailable")
            finish(true)
            return false
    return true

func state() -> Dictionary:
    var field := host.find_child("ExplorationSessionView", true, false)
    view = field if field != null and not field.is_queued_for_deletion() else host.find_child("BattleSessionView", true, false)
    return JSON.parse_string(view.call("ReadObservationJson"))

func sample(label: String) -> Dictionary:
    var current := state()
    var row := {"label":label, "focused":root.has_focus(), "owner":str(view.name)}
    for key in ["sessionId", "revision", "mode", "map", "stage", "actor", "failure", "wait", "stop", "stopReason", "viewport", "mapViewport", "worldGeometry", "cameraProjection", "logicalView", "entities", "actors", "previewRect", "zoom", "menu", "help", "dialogue", "choiceProjection", "entrySkip", "battleHandoffPending", "retreat", "scene", "battleChoices", "textId", "skipRequiresRelease", "battleMounted"]:
        if current.has(key): row[key] = current[key]
    var board := host.find_child("MapViewport", true, false)
    if board != null and current.has("stage"):
        row.terrain = JSON.parse_string(board.call("ReadTerrainJson"))
    samples.append(row)
    return current

func wait_for(test: Callable, label: String, seconds := 8.0) -> bool:
    var deadline := Time.get_ticks_msec() + int(seconds * 1000)
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return false
        var current := state()
        if current.get("failure") != null:
            sample("failure")
            return fail("Adapter/session failure: " + str(current.failure))
        if test.call(current): return true
        await process_frame
    sample("deadline-" + label)
    return fail("Deadline: " + label)

func key_event(code: Key, pressed: bool) -> void:
    var event := InputEventKey.new()
    event.keycode = code
    event.pressed = pressed
    Input.parse_input_event(event)

func tap(code: Key) -> bool:
    if not await focus(): return false
    key_event(code, true)
    await process_frame
    await process_frame
    key_event(code, false)
    await process_frame
    return not ended

func frames(count: int) -> void:
    for index in range(count):
        if ended: return
        await process_frame

func geometry(current: Dictionary) -> bool:
    if not check(current.viewport.width == 1920 and current.viewport.height == 1080, "Actual 1920x1080 host"): return false
    if not check(current.mapViewport == current.viewport, "Common full world rectangle"): return false
    if current.has("worldGeometry"):
        if not check(current.worldGeometry.cellSize == 96 and current.worldGeometry.pixelScale == 4, "Field shared cell/sampling"): return false
        var projection = current.get("cameraProjection")
        if projection != null:
            if not check(projection.scale == 4 and not projection.actors.is_empty(), "Private actual actor/plane draw available"): return false
            for actor in projection.actors:
                if not check(actor.width == 96 and actor.height == 96, "Ordinary source sprite frame remains 96px"): return false
    else:
        var terrain: Dictionary = samples[-1].terrain
        if not check(terrain.cellSize == 96 and terrain.scale == 1 and terrain.pixelScale == 4, "Battle fixed scale"): return false
        for actor in terrain.actors:
            if not check(actor.outputWidth == 96 and actor.outputHeight == 96 and actor.scale == 4, "Private battle sprite matches field scale"): return false
    return true

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(5)
    state() # Subscribe to the existing camera draw readback before sampling.
    await frames(5)
    if not await focus(): return
    if case_name in ["entry", "region"]:
        await entry()
        return
    elif case_name == "far":
        await far_target()
        return
    elif case_name == "scene":
        await scene()
        return
    else:
        if not await wait_for(func(s): return s.get("canWaitAtInput", false), "field input"): return
        await frames(3)
        var initial := sample("field-initial")
        if not geometry(initial): return
        if case_name == "room":
            if not check(initial.worldGeometry.legalScreen == {"x":432.0,"y":60.0,"width":1056.0,"height":960.0}, "Small legal room is centered at fixed scale"): return
            view.call("ObserveLayoutRegion",48,0,8,2)
            await frames(3)
            var boundary: Dictionary = view.call("ReadLayoutRegion",48,0,8,2)
            samples.append({"label":"legal-boundary-draw", "draw":boundary.draw})
            if not check(boundary.draw != null and not boundary.draw.uses.is_empty(), "Boundary actual draw is available"): return
            for use in boundary.draw.uses:
                if not check(use.sourceX >= 51 and use.x >= 432 and use.x + use.width <= 1488, "No adjacent packed room draw beyond legal boundary"): return
            view.call("ObserveLayoutRegion",0,0,1,1)
        if case_name == "warp":
            for step in range(2):
                if not await tap(KEY_LEFT): return
                if not await wait_for(func(s): return s.get("canWaitAtInput", false), "room exit step"): return
            await frames(12)
            var warped := sample("same-map-area-transfer")
            if not geometry(warped): return
            if not check(warped.cameraProjection.actors.any(func(a): return a.entity == "entity-0" and a.visible), "Player is visible immediately after packed-room warp"): return
            if not check(warped.worldGeometry.bounds.x == 0 and warped.worldGeometry.legalScreen.width == 1920, "Warp changes legal area without exposing packed rooms"): return
            finish(true)
            return
        if case_name == "inspect":
            finish(true)
            return
        if not await tap(KEY_RIGHT): return
        if not await wait_for(func(s): return s.get("canWaitAtInput", false), "field step"): return
        await frames(24)
        var moved := sample("field-moved-turned")
        if not geometry(moved): return
        if not await tap(KEY_LEFT): return
        if not await wait_for(func(s): return s.get("canWaitAtInput", false), "return step"): return
        await frames(12)
        sample("field-returned")
    finish(true)

func entry() -> void:
    var skipped := false
    var skip_attempts := 0
    var deadline := Time.get_ticks_msec() + 15000
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return
        var current := state()
        if current.get("failure") != null:
            sample("entry-failure")
            fail("Entry adapter failure")
            return
        if current.has("stage"): break
        if not skipped and skip_attempts < 2 and not current.get("skipRequiresRelease", true) and "Skip battle intro" in str(current.get("help", "")):
            sample("field-before-entry")
            skip_attempts += 1
            await tap(KEY_ESCAPE)
            var after_skip := sample("after-skip-attempt-" + str(skip_attempts))
            skipped = after_skip.get("entrySkip") != null or after_skip.get("battleHandoffPending", false) or after_skip.has("stage")
            if not skipped and skip_attempts == 2:
                fail("Two eligible mapped Cancel attempts did not start entry skip")
                return
        elif current.get("entrySkip") != null or current.get("battleHandoffPending", false):
            await process_frame
        elif current.get("canWaitForText", false) or current.get("wait") == "ChoiceWait" or "Reveal / Continue" in str(current.get("help", "")):
            await tap(KEY_ENTER)
        else: await process_frame
    if not await wait_for(func(s): return s.get("stage") == "Movement", "first battle control"): return
    sample("battle-handoff-first-frame")
    await frames(60)
    var initial := sample("battle-initial")
    if not geometry(initial): return
    if not await tap(KEY_ENTER): return
    await frames(4)
    var menu := sample("battle-menu-overlay")
    if not check(menu.mapViewport == initial.mapViewport and menu.zoom == initial.zoom and menu.previewRect == initial.previewRect, "Opening menu preserves world geometry"): return
    if not await tap(KEY_ESCAPE): return
    if not await tap(KEY_UP): return
    await frames(45)
    var moved := sample("battle-movement-preview")
    if not geometry(moved): return
    if not await tap(KEY_ESCAPE): return # restore the provisional tile
    # Stay only until the already learned Egress user receives ordinary control.
    for turn in range(4):
        var current := state()
        if current.actor == "ally-0": break
        if not await tap(KEY_ENTER): return
        if not await tap(KEY_SPACE): return
        if not await tap(KEY_ENTER): return
        if not await wait_for(func(s): return s.get("stage") == "Movement" and s.get("stopReason") == "PlayerInput" and s.get("movement") == null, "next ordinary ally", 12.0): return
    if not check(state().actor == "ally-0", "Egress user receives normal control"): return
    if not await tap(KEY_ENTER): return
    if not await tap(KEY_H): return
    await frames(3)
    var magic := sample("battle-magic-list")
    if not check(magic.menu.layout.listVisible and not magic.menu.layout.rows.is_empty(), "Actual spell list overlays fixed world"): return
    if case_name == "entry":
        if not check(magic.menu.layout.detail.contains("Unavailable"), "Unsupported historical Egress remains explicit"): return
        if not await tap(KEY_ESCAPE): return
        if not await tap(KEY_I): return
        if not await tap(KEY_DOWN): return
        if not await tap(KEY_ENTER): return
        if not await wait_for(func(s): return s.get("stage") in ["TargetChoice", "CommitReady"], "actual item targeting"): return
        var first_target := sample("item-target-first")
        if not await tap(KEY_TAB): return
        await frames(30)
        var second_target := sample("item-target-next")
        if not check(second_target.zoom == first_target.zoom, "Changing target only pans"): return
        finish(true)
        return
    if not await tap(KEY_ENTER): return
    await frames(3)
    var targeted := sample("battle-target")
    if not check(targeted.stage == "CommitReady", "Existing Egress self target"): return
    if not await tap(KEY_ENTER): return
    if not await wait_for(func(s): return s.get("retreat", {}).get("revealed", false), "actual Egress text"): return
    sample("egress-text")
    if not await tap(KEY_ENTER): return
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "usable field return", 15.0): return
    await frames(12)
    var returned := sample("field-return")
    if not geometry(returned): return
    if not check(returned.sessionId == initial.sessionId, "Same session through entry and return"): return
    if not await tap(KEY_DOWN): return
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "usable returned movement"): return
    sample("field-return-moved")
    finish(true)

func clear_panels(current: Dictionary) -> bool:
    var layout: Dictionary = current.menu.layout
    var panels: Array = [layout.actor, layout.context, layout.description]
    if layout.listVisible: panels.append(layout.list)
    if layout.diamondVisible: panels.append(layout.diamond)
    for i in range(panels.size()):
        var a: Dictionary = panels[i]
        var rect := Rect2(a.x,a.y,a.width,a.height)
        if not check(Rect2(0,0,1920,1080).encloses(rect), "Visible panel stays within viewport"): return false
        for j in range(i):
            var b: Dictionary = panels[j]
            if not check(not rect.intersects(Rect2(b.x,b.y,b.width,b.height)), "Visible panels never overlap"): return false
    return true

func far_target() -> void:
    if not await wait_for(func(s): return s.get("stage") == "Movement", "authored large board"): return
    await frames(30)
    var initial := sample("authored-near")
    if not geometry(initial): return
    if not await tap(KEY_RIGHT): return
    await frames(30)
    var preview := sample("authored-preview")
    if not check(preview.previewX != initial.previewX and preview.zoom == initial.zoom, "Provisional movement retains scale"): return
    if not await tap(KEY_ESCAPE): return
    await frames(30)
    if not await tap(KEY_ENTER): return
    if not await tap(KEY_H): return
    await frames(5)
    var list_open := sample("authored-list-at-near-subject")
    if not clear_panels(list_open): return
    if not await tap(KEY_ENTER): return
    await frames(30)
    var near := sample("authored-self-target")
    if not clear_panels(near): return
    if not await tap(KEY_TAB): return
    await frames(240)
    var far := sample("authored-distant-target")
    if not geometry(far) or not clear_panels(far): return
    if not check(far.target == "guard-a" and far.zoom == near.zoom, "Far target never shrinks world"): return
    var target: Dictionary = far.actors.filter(func(a): return a.id == "guard-a")[0]
    if not check(target.insideMap, "Distant subject pans inside legal view"): return
    var description: Dictionary = far.menu.layout.description
    var target_rect := Rect2(target.globalRect.x,target.globalRect.y,target.globalRect.width,target.globalRect.height)
    if not check(not target_rect.intersects(Rect2(description.x,description.y,description.width,description.height)), "Corner description avoids the edge-clamped subject"): return
    var camera: Dictionary = samples[-1].terrain
    if not await tap(KEY_ESCAPE): return
    if not await tap(KEY_ESCAPE): return
    await frames(240)
    sample("authored-cancelled-to-actor")
    if not check(samples[-1].terrain.cameraX != camera.cameraX, "Cancel restores focus by panning"): return
    finish(true)

func scene() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "source zone input"): return
    sample("before-source-zone")
    if not await tap(KEY_DOWN): return
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "zone approach"): return
    if not await tap(KEY_RIGHT): return
    var last_text := -1
    var deadline := Time.get_ticks_msec() + 45000
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return
        var current := state()
        if current.get("failure") != null:
            sample("scene-failure")
            fail("Scene input failed")
            return
        var text := int(current.textId) if current.get("textId") != null else -1
        if text != last_text and not str(current.get("dialogue", "")).is_empty():
            await frames(3)
            sample("source-text-" + str(text))
            last_text = text
        if text == 525 and current.get("canWaitForText", false):
            await frames(3)
            var wall := sample("teacher-wall-525")
            if not geometry(wall): return
            var occlusions: Array = wall.cameraProjection.occlusionDraws
            if not check(occlusions.any(func(o): return o.subject == "entity-142" and o.overlaps.size() > 0), "Actual source teacher ink receives wall fragments"): return
            finish(true)
            return
        if current.get("canWaitForText", false) or "Reveal / Continue" in str(current.get("help", "")):
            await tap(KEY_ENTER)
        else: await process_frame
    sample("scene-deadline")
    fail("Bounded source scene did not reach wall boundary")

func finish(completed := false) -> void:
    if ended: return
    ended = true
    var output := OS.get_environment("SF2_VIEWPORT_OUTPUT")
    var file := FileAccess.open(output, FileAccess.WRITE)
    if file == null:
        push_error("Missing viewport output destination")
        quit(2)
        return
    file.store_string(JSON.stringify({"passed":completed and failures.is_empty() and unavailable.is_empty(), "failures":failures, "unavailable":unavailable, "elapsedMs":Time.get_ticks_msec()-began, "samples":samples}, "  "))
    file.close()
    quit(0 if completed and failures.is_empty() and unavailable.is_empty() else 1)
