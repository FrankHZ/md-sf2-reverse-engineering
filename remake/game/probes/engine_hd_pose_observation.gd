extends "res://probes/engine_pose_dialogue_observation.gd"

# Normal dialogue still owns sprite/portrait selection. Only read mounted textures.
func observe_portrait(id: int, expected: Array) -> bool:
    if not super.observe_portrait(id, expected): return false
    var s := state()
    var panel: Dictionary = s.dialoguePanel
    var drawn: Dictionary = s.portraitProjection
    if expected[3] >= 0:
        var texture := instance_from_id(int(panel.portraitTexture)) as Texture2D
        if not check(texture != null and texture.get_width() == 192 and texture.get_height() == 256,
            "actual HD portrait density"): return false
        var image := texture.get_image()
        image.convert(Image.FORMAT_RGBA8)
        var digest := HashingContext.new()
        digest.start(HashingContext.HASH_SHA256)
        digest.update(image.get_data())
        samples.append({"hdText":id, "portrait":drawn.id, "tiles":drawn.tiles,
            "mirror":drawn.mirrored, "rgba":digest.finish().hex_encode(),
            "destination":[drawn.x,drawn.y,drawn.width,drawn.height]})
    for actor in s.cameraProjection.actors:
        if not actor.visible: continue
        var texture := instance_from_id(int(actor.texture)) as Texture2D
        if not check(texture != null and texture.get_width() == 96, "actual field HD actor density"): return false
    return true
