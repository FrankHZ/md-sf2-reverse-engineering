extends "res://probes/engine_equipment_observation.gd"

var hd_review = preload("res://probes/HdBattleProjection.gd").new()
var mode := "original"
var fairy_seen := {}

func actor(s: Dictionary, id: String) -> Dictionary:
    return s.actors.filter(func(a): return a.id == id)[0]

func find(name_: String) -> Node: return host.find_child(name_, true, false)
func settings() -> Dictionary: return JSON.parse_string(find("SettingsView").call("ReadSettingsObservationJson"))

func actor_state(state: Dictionary) -> Array:
    return state.actors.map(func(a):
        var copy: Dictionary = a.duplicate(true)
        copy.erase("globalRect"); copy.erase("sprite")
        return copy)

func apply_mode(next_mode: String) -> void:
    var before := board()
    var before_scene: Dictionary = scene().scene
    await key(KEY_F10)
    for i in range(5):
        var button := find("SystemSettings") as Button
        if button != null and button.theme_type_variation == "EntrySelectedButton": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    var settings_view := find("SettingsView")
    check(settings_view != null, "ordinary battle Settings")
    if settings_view == null: return
    check(await preload("res://probes/settings_navigation.gd").setting(settings_view, "graphics"), "graphics reachable")
    if settings().draft.GraphicsMode != next_mode: await key(KEY_RIGHT)
    check(await preload("res://probes/settings_navigation.gd").setting(settings_view, "apply"), "Apply reachable")
    var apply_started := Time.get_ticks_msec()
    await key(KEY_ENTER)
    var apply_ms := Time.get_ticks_msec() - apply_started
    check(find("SettingsView") == null, "Apply succeeds")
    mode = next_mode
    if find("SystemMenuView") != null: await key(KEY_ESCAPE)
    var after := board()
    check(after.mainSeed == before.mainSeed and after.thinkingSeed == before.thinkingSeed and JSON.stringify(actor_state(after)) == JSON.stringify(actor_state(before)),
        "graphics transaction preserves actors and both random streams")
    check(scene().scene.waitToken == before_scene.waitToken, "graphics transaction preserves scene wait")
    var settings_host: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
    check(settings_host.applied.GraphicsMode == mode, "ordinary preference applied")
    samples.append({"mode":mode,"applyMs":apply_ms,"settings":settings_host,"scene":scene().scene,
        "before":{"actors":actor_state(before),"mainSeed":before.mainSeed,"thinkingSeed":before.thinkingSeed},
        "after":{"actors":actor_state(after),"mainSeed":after.mainSeed,"thinkingSeed":after.thinkingSeed}})

func scene() -> Dictionary:
    var current := super.scene()
    hd_review.observe(current.scene, view, check, mode == "hd")
    var closeup := view.find_child("BattleScene", true, false)
    if closeup == null: return current
    for row in current.scene.fairySprites:
        var binding: Dictionary = row.binding
        if fairy_seen.has(binding.resource): continue
        var sprite := closeup.get_node_or_null("SceneCanvas/" + row.name) as Sprite2D
        if sprite == null or sprite.texture == null:
            check(false, "healing sprite and texture mounted"); continue
        check(mode == "hd" and binding.hd and sprite.texture.get_size() == Vector2(binding.width,binding.height)*4,
            "actual healing body/wing/dust HD binding")
        check(sprite.texture.get_size()*sprite.scale == Vector2(binding.width,binding.height), "healing logical footprint")
        fairy_seen[binding.resource] = true
    return current

func settle() -> Dictionary:
    for frame in range(2400):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native scene failure: " + str([current.failure,s.error])); return {}
        if current.hasBattleControl and s.phase == null and current.movement == null:
            var state := board()
            if state.stage == "Movement" and state.stopReason == "PlayerInput": return state
        if s.requiresAcknowledgement and not s.suppressed:
            if s.healing == null or s.healing.AtTimedInput or s.healing.LogicalComplete:
                await key(KEY_ENTER)
    check(false, "finite heal/physical bound")
    return {}

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host); root.grab_focus(); await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HEAL_START"))
    check(admitted == "started", "controlled formation: " + str(admitted))
    view = find("BattleSessionView")
    if view == null:
        check(false,"battle mounted")
        samples.append({"startupLabels":host.find_children("*","Label",true,false).map(func(n): return n.text)})
        finish(); return
    view.connect("SessionResultObserved", result_observed)
    var initial := await settle()
    if initial.is_empty(): finish(); return
    await apply_mode("hd")
    # The adjacent opponent supplies a physical scene; Sarah heals its recipient.
    var healed := false
    for turn in range(8):
        var state := board()
        await key(KEY_ENTER)
        if state.actor == "ally-1" and not healed:
            var before := actor(state,"ally-1")
            var priest := actor(state,"ally-1")
            await key(KEY_H); await key(KEY_ENTER)
            for i in range(3):
                if board().target == "ally-1": break
                await key(KEY_TAB)
            check(board().target == "ally-1", "ordinary HEAL target")
            await key(KEY_ENTER)
            var next := await settle()
            if next.is_empty(): finish(); return
            check(actor(next,"ally-1").hp > before.hp and actor(next,"ally-1").mp == priest.mp-3, "one heal and MP charge")
            healed = true
            break
        else: await key(KEY_SPACE)
        if (await settle()).is_empty(): finish(); return
    check(healed, "actual healing completed")
    for resource in ["body0","body1","wing0","wing1","dust0","dust1","dust2","dust3","dust4"]:
        check(fairy_seen.has("healing/"+resource), "actual healing resource " + resource)
    await apply_mode("original")
    var original_start: int = hd_review.samples.size()
    hd_review.seen.clear()
    # Subsequent opponent playback must use Original, then HD can be selected again.
    for turn in range(3):
        await key(KEY_ENTER)
        await key(KEY_SPACE); await settle()
        if hd_review.samples.slice(original_start).any(func(row): return not row.binding.hd): break
    check(hd_review.samples.slice(original_start).any(func(row): return not row.binding.hd), "actual Original closeup after Apply")
    await apply_mode("hd")
    samples.append({"hdReadback":hd_review.samples,"healingResources":fairy_seen.keys()})
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null: push_error("observation output unavailable"); quit(1); return
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
        "scope":"prepared private HEAL, physical playback and ordinary graphics Settings; controlled formation, not natural reach",
        "samples":samples,"events":events}, "  "))
    output.close()
    if host != null: host.queue_free()
    quit(0 if failures.is_empty() else 1)
