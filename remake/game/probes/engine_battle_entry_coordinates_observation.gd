extends "res://probes/engine_map3_commission_save_observation.gd"

# The isolated recovery slot uses ordinary title Continue and movement. The
# other case starts before the local-coordinate Battle21 scene.
var scene_samples := {}
var story_events := {}
var texts_seen: Array[int] = []

func _initialize() -> void:
    create_timer(150).timeout.connect(func(): check(false, "Battle entry observation deadline"); finish())
    call_deferred("run")

func tile(entity: Dictionary) -> Vector2i:
    return Vector2i(roundi(float(entity.x) / 384), roundi(float(entity.y) / 384))

func scene_sample(label: String, s: Dictionary) -> void:
    var projection: Variant = find("ExplorationSessionView").call("ReadCaptureState", false, true).cameraProjection
    if projection != null: projection.erase("occlusionDraws")
    var actors := {}
    for actor in s.entities:
        if actor.id in ["entity-0", "entity-5", "entity-128"]:
            actors[actor.id] = {"x":actor.x, "y":actor.y, "moving":actor.moving}
    var row := {"label":label, "map":s.map, "view":s.logicalView, "projection":projection,
        "actors":actors, "cursor":s.cursor, "text":s.textId, "continuation":s.continuation}
    scene_samples[label] = row
    samples.append(row)

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.has_meta("entry_coordinates_observed"):
        node.set_meta("entry_coordinates_observed", true)
        node.connect("SessionResultObserved", func(payload: String):
            var result: Dictionary = JSON.parse_string(payload)
            for event in result.observations:
                if event.Kind in ["battle-selected", "battle-initialized", "battle-loaded"] or (
                    event.Kind == "program-instruction" and event.Detail == "WriteFlag" and
                    event.Program != null and event.Program.Program == "bbcs-06"):
                    story_events[event.Sequence] = event)

func settle_entry(battle21: bool) -> Dictionary:
    var field := find("ExplorationSessionView")
    field.call("ReadObservationJson") # Enable the existing draw observer.
    var program := "bbcs-21" if battle21 else "bbcs-06"
    var latest := {}
    for tick in range(5000):
        await process_frame
        var s := state()
        if tick % 120 == 0:
            latest = {"map":s.get("map"), "mode":s.get("mode"), "cursor":s.get("cursor"),
                "text":s.get("textId"), "wait":s.get("wait"), "continuation":s.get("continuation"),
                "stop":s.get("stop"), "presentation":s.get("presentation")}
        if s.get("failure") != null or s.get("presentation", {}).get("error") != null:
            samples.append({"label":"field-fault", "map":s.get("map"), "cursor":s.get("cursor"),
                "failure":s.get("failure"), "stop":s.get("stop"), "presentation":s.get("presentation")})
            check(false, "Field entry failure: " + str(s.get("failure")))
            return {}
        var cursor: Variant = s.get("cursor")
        if cursor != null and cursor.Program == program:
            if not scene_samples.has("scene-load") and s.map == ("map-67" if battle21 else "map-66") and int(cursor.Instruction) >= 4:
                scene_sample("scene-load", s)
            if s.get("canWaitForText", false):
                var text: int = s.textId
                if battle21:
                    scene_sample("local-coordinate-dialogue", s)
                    return s
                if texts_seen.is_empty() or texts_seen.back() != text:
                    texts_seen.append(text)
                    if text in [2342, 2343, 2347, 2353, 2356]:
                        scene_sample("text-" + str(text), s)
                await key(KEY_ENTER)
            elif s.get("wait") == "EntityWait" and s.logicalView.TargetSlot != null and not scene_samples.has("officer-follow"):
                scene_sample("officer-follow", s)
        var battle := find("BattleSessionView") as Control
        if battle == null or not battle.is_visible_in_tree(): continue
        var board: Dictionary = battle.call("ReadCaptureState", false)
        var scene: Dictionary = board.get("scene", {})
        latest["battle"] = {"stage":board.get("stage"), "stop":board.get("stopReason"),
            "phase":scene.get("phase"), "story":board.get("story")}
        if board.get("failure") != null or board.get("scene", {}).get("error") != null:
            samples.append({"label":"battle-fault", "failure":board.get("failure"), "scene":board.get("scene")})
            check(false, "Battle entry failure")
            return {}
        if board.get("stage") == "Movement" and board.get("stopReason") == "PlayerInput": return board
        # The recovered RNG can give an enemy the first turn. Only acknowledge its messages.
        if scene.get("phase") in ["ActionMessage", "ResultMessage", "DeathMessage", "RewardMessage", "GrowthMessage", "GoldMessage"]:
            var healing: Variant = scene.get("healing")
            if healing == null or healing.LogicalComplete or healing.AtTimedInput: await key(KEY_ENTER)
    samples.append({"label":"entry-deadline", "state":latest})
    check(false, "No first battle control in the observation window")
    return {}

func check_scene(label: String, area_max: Vector2i, camera: Vector2i) -> bool:
    var row: Dictionary = scene_samples.get(label, {})
    if row.is_empty():
        check(false, label + " was observed")
        return false
    var view: Dictionary = row.view
    check(Vector2i(view.Area.MaxX, view.Area.MaxY) == area_max, label + " view area")
    check(Vector2i(view.AX.Position, view.AY.Position) == camera * 384, label + " authored camera")
    return true

func observe_battle(before: Dictionary, board: Dictionary) -> void:
    var battle := find("BattleSessionView")
    await frames(4)
    var terrain := observation(battle.get_node("MapViewport"), "ReadTerrainJson")
    var active: Array = board.actors.filter(func(a): return a.id.begins_with("ally-") and a.x != null and a.hp > 0)
    samples.append({"label":"first-battle-control", "map":board.map, "size":[board.mapWidth,board.mapHeight],
        "actor":board.actor, "stage":board.stage, "active":active.map(func(a):return a.id),
        "gold":board.gold, "flags":board.storyFlags, "terrain":terrain, "events":story_events.values()})
    check(board.sessionId == before.sessionId and board.gold == before.gold, "Same session and carried gold")
    check(active.size() == 6 and 407 in board.storyFlags and 456 in board.storyFlags and 506 not in board.storyFlags,
        "Before/start flags and six active allies")
    check(board.mapWidth == 20 and board.mapHeight == 32 and terrain.region.Map.Value == "map-66" and
        Vector2i(terrain.region.Origin.X,terrain.region.Origin.Y) == Vector2i(21,2), "BattleLoad binds the true source crop")
    check(terrain.clip and terrain.layers.any(func(layer):return layer.draws > 0), "Battle-local terrain draws")
    for kind in ["battle-selected", "program-instruction", "battle-initialized", "battle-loaded"]:
        check(story_events.values().filter(func(event):return event.Kind == kind).size() == 1,
            "Exactly one " + kind)
    await key(KEY_ENTER)
    check(battle.call("ReadCaptureState", false).stage == "ActionChoice", "First player can confirm movement")
    await key(KEY_X)
    check(battle.call("ReadCaptureState", false).stage == "Movement", "First player can return to Movement")

func run() -> void:
    root.size = Vector2i(1920, 1080)
    node_added.connect(observe_node)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await frames(4)
    if case_name == "battle21":
        var started: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
        check(started == "started", "Admit the controlled Battle21 approach: " + started)
        if started == "started" and not (await settle_entry(true)).is_empty():
            check_scene("scene-load", Vector2i(63,63), Vector2i(5,18))
            var local: Dictionary = scene_samples["local-coordinate-dialogue"]
            check(local.text == 2576 and tile(local.actors["entity-128"]) == Vector2i(8,22),
                "Battle21 first dialogue uses the staged local speaker coordinates")
            var field := find("ExplorationSessionView")
            var copied: Array = field.call("ReadLayoutRegion", 12, 18, 5, 1).words
            var source: Array = field.call("ReadLayoutRegion", 33, 18, 5, 1).words
            check(copied.size() == 5 and copied == source, "Battle21 uses the copy from source (21,0)")
        host.queue_free()
        await frames(12)
        finish()
        return

    # Prepare the isolated slot before the process starts. Never write a game slot.
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): finish(); return
    await key(KEY_ENTER)
    var before := await settle("recovered-slot-1")
    check(before.get("map") == "map-16" and before.get("canWaitAtInput", false), "Slot 1 reaches castle field input")
    if not failures.is_empty(): finish(); return
    check(tile(player(before)) == Vector2i(27,7) and before.gold == 1072 and
        406 in before.flags and 505 in before.flags and 456 not in before.flags, "Recovered pre-entry state")
    await walk(Vector2i(26,31), "castle-south-exit", "map-18")
    if issue == "": await walk(Vector2i(9,28), "south-gate-approach")
    if issue != "":
        check(false, issue); finish(); return
    await key(KEY_DOWN)
    var board := await settle_entry(false)
    if not board.is_empty():
        check_scene("scene-load", Vector2i(63,63), Vector2i(25,5))
        if check_scene("text-2342", Vector2i(63,63), Vector2i(25,5)):
            var first: Dictionary = scene_samples["text-2342"]
            check(tile(first.actors["entity-0"]) == Vector2i(30,9) and
                tile(first.actors["entity-5"]) == Vector2i(29,8), "Bowie and Slade use source positions")
            check(first.projection != null and first.projection.actors.any(
                func(a):return a.entity == "entity-0" and a.visible), "Draw projects Bowie on the full map")
        check_scene("text-2343", Vector2i(63,63), Vector2i(31,9))
        check(scene_samples.has("officer-follow") and scene_samples.has("text-2353") and
            tile(scene_samples["text-2353"].actors["entity-128"]) == Vector2i(30,11), "Officer moves in source coordinates")
        check(texts_seen == range(2342,2357), "Before scene delivers its full text sequence once")
        await observe_battle(before, board)
    host.queue_free()
    await frames(12)
    finish()
