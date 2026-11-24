extends "res://probes/engine_pre_voyage_terrain_observation.gd"

func battle_sample(width: int) -> Array:
    var geometry := super.battle_sample(width)
    var battle := find("BattleSessionView")
    var terrain := observation(battle.get_node("MapViewport"),"ReadTerrainJson")
    var capture: Dictionary = battle.call("ReadCaptureState",false)
    var actors := []
    for actor in terrain.actors:
        if not actor.visible: continue
        var texture := instance_from_id(int(actor.texture)) as Texture2D
        check(texture != null and texture.get_width() == width, "mounted tactical actor density")
        check(actor.outputWidth == 96 and actor.outputHeight == 96, "tactical actor logical size")
        var identity: Dictionary = capture.actors.filter(func(a): return a.id == actor.id)[0].sprite
        actors.append({"id":actor.id,"sprite":identity.resource,"selector":identity.resourceSelector,
            "width":texture.get_width(),"rgba":pixel_digest(texture),"geometry":[actor.x,actor.y,actor.outputWidth,actor.outputHeight]})
    check(not actors.is_empty(), "tactical actors drawn")
    samples.append({"actorMode":width,"actors":actors})
    return geometry

func field_sample(label: String, width: int) -> void:
    var s: Dictionary = JSON.parse_string(find("ExplorationSessionView").call("ReadObservationJson"))
    if s.get("cameraProjection") == null:
        check(false, "field draw observation mounted")
        return
    var actors := []
    for actor in s.cameraProjection.actors:
        if not actor.visible: continue
        var texture := instance_from_id(int(actor.texture)) as Texture2D
        check(texture != null and texture.get_width() == width, "mounted field actor density")
        check(actor.width == 96 and actor.height == 96, "field actor logical size")
        actors.append({"id":actor.entity,"selector":actor.resourceSelector,"rgba":pixel_digest(texture),
            "texture":actor.texture,"width":texture.get_width(),"geometry":[actor.x,actor.y,actor.width,actor.height]})
    check(not actors.is_empty() and s.failure == null, "healthy field actor projection")
    samples.append({"fieldMode":label,"actors":actors})

func run() -> void:
    if case_name != "field":
        await super.run()
        return
    if OS.get_environment("SF2_SAVE_NAMESPACE") != "issue2175":
        push_error("use the terrain observer's disposable issue2175 namespace under an isolated root")
        quit(2)
        return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(8)
    var mounted: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(mounted == "started", "controlled field mounted")
    if mounted != "started": finish(); return
    find("ExplorationSessionView").call("ReadObservationJson")
    await frames(12)
    if applied().GraphicsMode != "original": await mode("original")
    field_sample("original",24)
    await mode("hd")
    field_sample("hd",96)
    await mode("original")
    field_sample("restored",24)
    finish()
