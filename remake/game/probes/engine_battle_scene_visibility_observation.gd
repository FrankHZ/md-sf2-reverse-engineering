extends "res://probes/engine_battle_status_observation.gd"

# Reuse ordinary scene inputs; only inspect native visibility at semantic samples.
func sample(label: String) -> Dictionary:
    var s := super.sample(label)
    if not s.has("scene"): return s
    var board := host.find_child("MapViewport", true, false)
    var scene: Dictionary = s.scene
    var closeup: bool = scene.visible and scene.fieldDeath == null and scene.phase not in ["SleepTurnMessage", "PoisonDamageMessage", "PoisonExhaustedMessage"]
    var visible_sprites := 0
    for node in board.find_children("MapSprite_*", "Sprite2D", true, false):
        if node.is_visible_in_tree(): visible_sprites += 1
    samples[-1].mapVisibility = {"closeup":closeup,"mapVisible":board.is_visible_in_tree(),"visibleSprites":visible_sprites}
    check(board.is_visible_in_tree() != closeup, "Map visibility follows full close-up ownership")
    if closeup: check(visible_sprites == 0, "No native map sprites draw through close-up")
    return s
