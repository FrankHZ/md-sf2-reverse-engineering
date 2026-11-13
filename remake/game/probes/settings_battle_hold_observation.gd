extends "res://probes/settings_remap_observation.gd"

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1774-"): quit(2); return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(12)
    if find("BattleSessionView") == null: check(false,"mount authored stone court"); finish(); return
    for frame in 200:
        if state().get("stage") == "Movement": break
        await frames(1)
    check(mounted().hints.compact.Accelerate == "Q" and mounted().hints.compact.Confirm == "F5","battle uses editor-installed map")
    await edge(KEY_UP,true)
    await edge(KEY_Q,true)
    check(state().pacing.input.rate == 1.5, "Q accelerates provisional battle movement")
    await edge(KEY_UP,false); await edge(KEY_Q,false)
    for frame in 200:
        if state().get("movement") == null: break
        await frames(1)
    await Nav.tap(self,"down")
    for frame in 200:
        if state().get("movement") == null: break
        await frames(1)
    await Nav.tap(self,"confirm"); await Nav.tap(self,"attack")
    for n in 4:
        if state().scene.visible: break
        await Nav.tap(self,"confirm")
    for frame in 300:
        if state().scene.requiresAcknowledgement: break
        await frames(1)
    var scene: Dictionary = state().scene
    check(scene.requiresAcknowledgement, "authored attack reached real reading boundary")
    var token: Variant = scene.waitToken
    await edge(KEY_Q,true); await frames(12)
    check(state().pacing.input.rate == 1 and state().scene.waitToken == token, "Q neither accelerates reading nor acknowledges it")
    await edge(KEY_Q,false); await Nav.tap(self,"confirm"); await frames(8)
    check(state().scene.waitToken != token or not state().scene.requiresAcknowledgement,"fresh mapped Confirm acknowledges reading")
    finish()
