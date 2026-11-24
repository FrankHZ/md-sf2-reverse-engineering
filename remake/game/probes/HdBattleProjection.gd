extends RefCounted

# Bounded readback at changed resource/phase identities; never capture whole frames.
var samples: Array = []
var seen := {}
var death_seen := false

func observe(s: Dictionary, view: Node, check: Callable, hd: bool = true) -> void:
    var closeup := view.find_child("BattleScene", true, false)
    if closeup == null:
        check.call(false, "production BattleScene node present"); return
    for pair in [["ally", "Ally"], ["enemy", "Enemy"], ["weapon", "Weapon"],
        ["background", "Background"], ["ground", "Ground"]]:
        var binding: Dictionary = s[pair[0]]
        if not binding.visible or binding.resource == null: continue
        var key := str([hd, s.phase, pair[0], binding.resource])
        if seen.has(key): continue
        var sprite := closeup.get_node_or_null("SceneCanvas/" + pair[1]) as Sprite2D
        if sprite == null or sprite.texture == null:
            check.call(false, "mounted texture present " + key); continue
        check.call(binding.hd == hd, "published graphics binding " + key)
        check.call(not hd or sprite.texture.get_width() == int(binding.width * 4) and
            sprite.texture.get_height() == int(binding.height * 4), "four pixels per logical scene pixel " + key)
        check.call(sprite.texture.get_size() * sprite.scale == Vector2(binding.width, binding.height),
            "unchanged scene footprint " + key)
        var image := sprite.texture.get_image()
        if image == null:
            check.call(false, "texture pixels readable " + key); continue
        image.convert(Image.FORMAT_RGBA8)
        var digest := HashingContext.new()
        digest.start(HashingContext.HASH_SHA256)
        digest.update(image.get_data())
        seen[key] = true
        samples.append({"phase":s.phase,"node":pair[0],"binding":binding,
            "rgba":digest.finish().hex_encode(),"position":[sprite.position.x,sprite.position.y],
            "flipH":sprite.flip_h,"flipV":sprite.flip_v,"z":sprite.z_index})
    if hd and s.fieldDeath != null:
        for node in view.get_node("MapViewport").find_children("*", "Sprite2D", true, false):
            if node.get_meta("mapsprite", -1) != 63 or not node.is_visible_in_tree(): continue
            if node.texture == null:
                check.call(false, "field-exit texture present"); continue
            death_seen = true
            check.call(node.texture.get_width() == 96 and node.texture.get_size() * node.scale == Vector2(96,96),
                "actual field-exit HD half-strip and footprint")

func finish(check: Callable) -> void:
    check.call(not samples.is_empty(), "actual HD closeup resources observed")
    check.call(death_seen, "actual HD field-exit draw observed")
