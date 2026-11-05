extends "res://probes/engine_exploration_observation.gd"

# Bounded controlled source entry; actual input and draw operands, no images or live state edits.
var seen: Dictionary = {}
var case_name := OS.get_environment("SF2_VISIBILITY_CASE")
var checked := OS.get_environment("SF2_VISIBILITY_CHECK") == "1"
var region := [49, 0, 4, 56]
var began := Time.get_ticks_msec()

func state() -> Dictionary:
    return view.call("ReadCaptureState", false, false)

func raster(width: int, height: int) -> Dictionary:
    var bytes := PackedByteArray()
    bytes.resize(width * height * 4)
    bytes.fill(255)
    var hash := HashingContext.new()
    hash.start(HashingContext.HASH_SHA256)
    hash.update(bytes)
    return {"width":width,"height":height,"format":"rgba8","data":Marshalls.raw_to_base64(bytes),"sha256":hash.finish().hex_encode()}

func prepare_authored() -> void:
    var doc: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../content/authored/harbor-arrival.json"))
    var maps: Array = []
    for map in doc.world.maps:
        map.layout = []
        for y in range(64):
            var row: Array = []
            row.resize(64); row.fill(1)
            map.layout.append(row)
        map.areas = [{"minX":0,"minY":0,"maxX":30,"maxY":12,"view":{"foregroundX":0,"foregroundY":0,"backgroundX":0,"backgroundY":32,
            "parallaxAX":256,"parallaxAY":256,"parallaxBX":128,"parallaxBY":128,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":255}}]
        var blocks: Array = []
        for block in range(1024): blocks.append([0,0,0,0,0,0,0,0,0])
        maps.append({"map":map.id,"atlas":raster(128,320),"scale":1,"blocks":blocks})
        for entity in map.entities: entity.sprite = 30
    doc.world.presentation = {"maps":maps,"sprites":[{"sprite":30,"directions":[raster(48,24),raster(48,24),raster(48,24)],"portrait":null,"speech":0}],"portraits":[]}
    doc.world.programs.append({"id":"visibility","instructions":[{"op":"sprite","entity":"traveler","sprite":30},
        {"op":"camera-target","position":{"x":25,"y":3}},{"op":"wait-view"},{"op":"quake","amount":2},{"op":"wait-ticks","ticks":8},
        {"op":"camera-entity","entity":"traveler"},{"op":"wait-ticks","ticks":8},{"op":"quake","amount":0},{"op":"end"}]})
    doc.start.position = {"x":26,"y":8}
    doc.start.program = {"program":"visibility","instruction":0}
    doc.start.textSettings = {"messageSpeed":2,"mouthControl":0,"viewSpeed":0}
    var file := FileAccess.open(OS.get_environment("SF2_VISIBILITY_PACKAGE"),FileAccess.WRITE)
    file.store_string(JSON.stringify(integral_numbers(doc))); file.close()

func integral_numbers(value: Variant) -> Variant:
    if value is Dictionary:
        for name in value: value[name] = integral_numbers(value[name])
    elif value is Array:
        for index in range(value.size()): value[index] = integral_numbers(value[index])
    elif value is float and value == floor(value): return int(value)
    return value

func record(label: String, s: Dictionary) -> void:
    var compact: Dictionary = {}
    for field in ["map", "sessionId", "revision", "simulationTick", "cursor", "stop", "wait", "failure", "logicalView", "cameraProjection", "entities", "canWaitAtInput", "dialogue"]:
        compact[field] = s.get(field)
    compact = compact.duplicate(true)
    for entity in compact.entities: entity.erase("actionProgram")
    if compact.cameraProjection != null: compact.cameraProjection.erase("occlusionDraws")
    var layout: Dictionary = view.call("ReadLayoutRegion", region[0], region[1], region[2], region[3])
    if layout.draw != null:
        for use in layout.draw.uses:
            for redundant in ["words", "selector", "resourceIdentity"]: use.erase(redundant)
    samples.append({"label":label, "state":compact, "layout":layout})
    if not checked or s.cameraProjection == null: return
    var camera: Dictionary = s.cameraProjection
    var logical = s.logicalView
    var origin := Vector2(camera.actors[0].originX, camera.actors[0].originY)
    if logical != null and logical.TargetSlot == null:
        var main: Dictionary = camera.background if logical.Area.Layer == 0 else camera.foreground
        origin = Vector2(main.x, main.y)
    var bounds: Dictionary = camera.legalBounds
    var expected := Rect2((Vector2(bounds.x,bounds.y) - origin) * 4, Vector2(bounds.width,bounds.height) * 4).intersection(Rect2(Vector2.ZERO,Vector2(root.size)))
    var clip: Dictionary = camera.legalScreen
    _check(expected.is_equal_approx(Rect2(clip.x,clip.y,clip.width,clip.height)), "Actual main-plane area projection: " + label)
    if s.dialogue.begins_with("Sorry, kids."):
        _check(camera.foreground.draws + camera.foregroundHigh.draws > 0, "Legitimate foreground remains drawn")
        for id in ["entity-0", "entity-1", "entity-2", "entity-142", "entity-143"]:
            _check(camera.actors.any(func(actor): return actor.entity == id and actor.visible), "Required school actor remains visible: " + id)
    _check(layout.draw != null and not layout.draw.overflow, "Bounded actual draw available: " + label)
    if layout.draw != null:
        for use in layout.draw.uses:
            _check(expected.encloses(Rect2(use.x,use.y,use.width,use.height)), "Draw clips inside scene: " + label)
            _check(use.textureX >= 0 and use.textureY >= 0 and use.textureX + use.textureWidth <= 24 and use.textureY + use.textureHeight <= 24,
                "Source texture clip retained: " + label)
            if s.map == "map-3" and logical != null and logical.Area.MinX == 0:
                _check(use.sourceX <= 50, "No packed-room source cells: " + label)

func settled() -> bool:
    for frame in range(300):
        await process_frame
        var s := state()
        if s.failure != null: return false
        if s.canWaitAtInput:
            await process_frame
            return true
    return false

func transition() -> void:
    _check(await settled(), "Initial legal field control")
    record("outside-before", state())
    await _press(KEY_RIGHT)
    _check(await settled(), "Same-map room warp completes")
    var room := state()
    _check(room.logicalView.Area.MinX == 51 and room.logicalView.Area.MinY == 10, "Destination area selected")
    record("inside-room", room)
    await _press(KEY_LEFT)
    _check(await settled(), "Movement inside room")
    await _press(KEY_RIGHT)
    _check(await settled(), "Return warp completes")
    var outside := state()
    _check(outside.logicalView.Area.MinX == 0, "Outdoor area restored")
    record("outside-after", outside)
    _finish()

func _run() -> void:
    if case_name == "authored":
        region = [28, 0, 4, 56]
        prepare_authored()
    root.size = Vector2i(1920, 1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null:
        samples.append({"window": JSON.parse_string(host.call("ReadWindowSupportObservationJson"))})
        for node in host.find_children("*", "Label", true, false):
            samples.append({"label":str(node.name), "text":node.text})
        _check(false, "Controlled source entry mounts field")
        _finish()
        return
    view.call("ReadObservationJson") # Enable existing actual draw observation once.
    view.call("ObserveLayoutRegion", region[0], region[1], region[2], region[3])
    if case_name == "transition":
        await transition()
        return
    if case_name == "school-trigger": await _press(KEY_UP)
    for frame in range(9000):
        await process_frame
        var s := state()
        if s.failure != null:
            record("failure", s)
            _check(false, "Healthy source program")
            break
        if s.cameraProjection == null: continue
        var v = s.logicalView
        var label := ""
        if frame == 2: label = "initial"
        if case_name == "authored" and v != null:
            var key := str(v.TargetSlot) + "-" + str(s.simulationTick % 2) + "-" + str(s.wait)
            if not seen.has(key):
                seen[key] = true
                label = key
        if s.wait in ["FieldTextWait", "W1TextWait", "DialogueWait"]:
            var text_id: String = str(s.token)
            if not seen.has(text_id):
                seen[text_id] = true
                label = "dialogue-" + text_id
            if label != "" and (s.dialogue.begins_with("What?!") or s.dialogue.begins_with("Sorry, kids.") or "did you hear" in s.dialogue):
                record(label, s)
            await _press(KEY_ENTER)
            continue
        if v != null and v.TargetSlot == null:
            var key := "moving-" + str(v.BY.Destination)
            if not seen.has(key) and v.BY.Active:
                seen[key] = true
                label = key
        if label != "": record(label, s)
        if s.wait == "ChoiceWait":
            await _press(KEY_Z)
        if s.canWaitAtInput and s.cursor == null and seen.size() > 0:
            record("returned-control", s)
            if case_name == "school-trigger":
                _check(seen.has("moving-2688") and seen.has("moving-2304"), "Both original camera movements observed")
                await _press(KEY_DOWN)
                _check(await settled(), "Fresh movement after caller returns")
                var moved := state()
                _check(moved.entities.any(func(entity): return entity.id == "entity-0" and entity.x == 43 * 384 and entity.y == 11 * 384), "Returned field input moves player")
                record("fresh-movement", moved)
            _finish()
            return
    _check(false, "Finite scene returns field control")
    _finish()

func _finish() -> void:
    samples.append({"elapsedMs":Time.get_ticks_msec()-began})
    super._finish()
