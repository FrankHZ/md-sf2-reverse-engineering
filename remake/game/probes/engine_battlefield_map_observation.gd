extends "res://probes/engine_battle02_outcome_observation.gd"

# Bounded ordinary-host observation. Reuse existing keyboard/navigation/decision
# helpers; retain only semantic map checkpoints, not frames or extracted artwork.
var map_case := OS.get_environment("SF2_BATTLEFIELD_CASE")
var map_samples: Array = []
var map_events: Array = []
var map_seen: Dictionary = {}
var scene_seen := false
var death_seen := false
var scene_return_seen := false
var target_seen := false
var movement_seen := false
var first_map: Node
var old_texture: WeakRef
var old_map_texture: WeakRef
var old_children: Array = []

func _initialize() -> void:
    call_deferred("run_map")

func state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null: view = host.get_node("BattleSessionView")
    var s: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    if s.has("stage"):
        if s.get("scene") is Dictionary:
            scene_seen = scene_seen or bool(s.scene.get("visible",false))
            death_seen = death_seen or s.scene.get("fieldDeath") != null
        if s.get("stage") == "TargetChoice": target_seen = true
        if s.get("stage") == "Movement" and scene_seen: scene_return_seen = true
        for row in s.get("observations",[]) if s.get("observations") != null else []:
            if row.Kind in ["movement-started","physical-first","battle-outcome","battle-returned"] and not map_seen.has(row.Sequence):
                map_seen[row.Sequence] = true
                if map_events.size() < 48: map_events.append(row)
    return s

func map_read(label: String, s: Dictionary) -> Dictionary:
    var map: Node = view.get_node("MapViewport")
    var terrain: Dictionary = JSON.parse_string(map.call("ReadTerrainJson"))
    map_samples.append({"label":label,"map":terrain,"actor":s.get("actor"),"stage":s.get("stage"),
        "previewX":s.get("previewX"),"previewY":s.get("previewY"),"actors":s.get("actors"),
        "sessionId":s.get("sessionId"),"revision":s.get("revision"),"scene":s.get("scene")})
    _check(terrain.clip and terrain.scale > 0 and terrain.visibleLocal.width > 0 and terrain.visibleLocal.height > 0,label+":usable clipped board")
    if map_case == "authored":
        _check(terrain.region == null and terrain.layers.is_empty(),"intentional authored terrain")
        return terrain
    var region: Dictionary = terrain.region
    var second: bool = map_case.begins_with("battle02") or map_case == "transition" and map_seen.has("second-map")
    var expected := Vector2i(16,19) if second else Vector2i.ZERO
    _check(region.Map.Value == ("map-57" if map_case in ["pilot","battle01","transition","target-switch"] and not second else "map-66"),label+":selected visual map")
    _check(Vector2i(region.Origin.X,region.Origin.Y) == expected,label+":source region origin")
    _check(terrain.layers.size() == 2 and terrain.textures > 0,label+":retained terrain passes")
    for layer in terrain.layers:
        _check(layer.drawSequence > 0 and layer.map == region.Map.Value and layer.visualMap == layer.map,label+":actual bound draw")
        _check(layer.z == (2 if layer.highPriority else 0),label+":tile priority ordering")
    _check(terrain.layers[0].draws > 0,label+":ground drawn")
    for actor in terrain.actors:
        if not actor.visible: continue
        _check(actor.validTexture and actor.z == 1,label+":valid actor texture")
        var rows: Array = s.actors.filter(func(a): return a.id == actor.id)
        if rows.is_empty() or rows[0].x == null: continue
        var x: float = s.previewX if s.actor == actor.id and s.previewX != null else rows[0].x
        var y: float = s.previewY if s.actor == actor.id and s.previewY != null else rows[0].y
        _check(is_equal_approx(actor.x,(x+0.5)*terrain.cellSize) and is_equal_approx(actor.y,(y+0.5)*terrain.cellSize),label+":actor/cell alignment "+actor.id)
    _check(terrain.previewZ.all(func(z): return z == 3),label+":preview above terrain and actors")
    if s.get("target") != null and s.stage in ["TargetChoice","CommitReady"]:
        var selected: Dictionary = s.actors.filter(func(a): return a.id == s.target)[0]
        var tx: float = s.previewX if s.target == s.actor else selected.x
        var ty: float = s.previewY if s.target == s.actor else selected.y
        _check(terrain.targets.size() == 4 and terrain.targets.all(func(n): return n.z == 3),label+":accepted target outline")
        _check(is_equal_approx(terrain.targets[0].rect.x,tx*terrain.cellSize+1) and is_equal_approx(terrain.targets[0].rect.y,ty*terrain.cellSize+1),label+":target/cell alignment")
    else:
        _check(terrain.targets.is_empty(),label+":no stale target outline")
    if s.get("previewRect") != null: _check(s.previewInsideMap,label+":preview framed")
    return terrain

func map_settle() -> Dictionary:
    for tick in range(24000):
        await process_frame
        var s := state()
        if tick == 1000:
            print("SF2_MAP_SETTLE ",JSON.stringify({"tick":tick,"map":s.get("map"),"mode":s.get("mode"),"wait":s.get("wait"),"stage":s.get("stage"),"stop":s.get("stop"),"failure":s.get("failure"),"cursor":s.get("cursor")}))
        if s.get("failure") != null: return s
        if s.has("stage"):
            if s.stage != null and s.stopReason == "PlayerInput": return s
            if s.scene.phase in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
                if s.scene.healing == null or s.scene.healing.LogicalComplete or s.scene.healing.AtTimedInput: await key(KEY_ENTER)
        elif s.get("wait") == "ChoiceWait":
            if s.cursor.Program in ["cs-53482","cs-53176"] and not commission_refused:
                commission_refused = true
                await key(KEY_X)
            else: await key(KEY_ENTER)
        elif s.get("canWaitForText",false): await key(KEY_ENTER)
        elif s.get("wait") == "DialogueWait" and s.get("fieldText") == null:
            if not seen_texts.has(s.token):
                seen_texts[s.token] = true
                await key(KEY_ENTER)
        elif s.get("canWaitAtInput",false) and not player(s).get("moving",true): return s
    issue = "bounded-map-settle"
    return state()

func outcome_settle() -> Dictionary:
    return await map_settle()

func bound_castle_settle(_label: String, _yes: bool = true) -> Dictionary:
    return await map_settle()

func valid_projection(s: Dictionary, _preview: bool) -> bool:
    return s.zoom > 0 and s.mapViewport.width > 0 and s.mapViewport.height > 0

func flood(s: Dictionary, actor: Dictionary, origin: Dictionary, budget: int, occupied: bool) -> Dictionary:
    var start_cell: int = int(origin.y)*48+int(origin.x)
    var costs: Dictionary = {start_cell:0}
    var previous: Dictionary = {}
    var pending: Array = [start_cell]
    while not pending.is_empty():
        var index := 0
        for i in range(pending.size()):
            if costs[pending[i]] < costs[pending[index]]: index = i
        var cell: int = pending.pop_at(index)
        for delta in [1,-1,-48,48]:
            var next: int = cell+delta
            if next < 0 or next >= 2304 or next%48 >= s.mapWidth or int(next/48) >= s.mapHeight: continue
            if absi(delta) == 1 and int(cell/48) != int(next/48): continue
            if occupied and s.actors.any(func(a): return a.hp > 0 and str(a.id).begins_with("enemy-") and a.x != null and int(a.y)*48+int(a.x) == next): continue
            var surface: String = s.terrain[next]
            var cost := 2
            if surface == "Barrier": continue
            if actor.mover != "Flying":
                if surface == "HighSky": continue
                if actor.mover != "Hovering":
                    if surface == "Impassable": continue
                    if surface == "Brush": cost = 2 if actor.mover == "Stealth" else 3
                    if surface == "Rough": cost = 2 if actor.mover == "Stealth" else 5 if actor.mover == "Centaur" else 3
                    if surface == "Deep": cost = 2 if actor.mover == "Stealth" else 5 if actor.mover == "Centaur" else 4
                    if surface == "Desert": cost = 4 if actor.mover == "Stealth" else 5 if actor.mover == "Centaur" else 3
            var total: int = costs[cell]+cost
            if total <= budget and total < costs.get(next,10000):
                costs[next] = total
                previous[next] = cell
                if not pending.has(next): pending.append(next)
    return {"costs":costs,"previous":previous,"start":start_cell}

func observe_field(label: String, s: Dictionary) -> void:
    var camera: Dictionary = s.get("cameraProjection",{})
    _check(camera.get("bound",false) and s.get("failure") == null,label+":exploration atlas remains bound")
    map_samples.append({"label":label,"fieldMap":s.map,"camera":camera,"player":player(s),"sessionId":s.sessionId})

func movement_cancel(s: Dictionary) -> Dictionary:
    var before := s.duplicate(true)
    for code in [KEY_RIGHT,KEY_UP,KEY_LEFT,KEY_DOWN]:
        await key(code)
        s = await map_settle()
        if s.previewX != before.previewX or s.previewY != before.previewY: break
    movement_seen = s.previewX != before.previewX or s.previewY != before.previewY
    _check(movement_seen,"ordinary movement changes preview")
    await process_frame
    map_read("movement-preview",s)
    await key(KEY_ESCAPE)
    s = await map_settle()
    _check(s.previewX == before.previewX and s.previewY == before.previewY and s.mainSeed == before.mainSeed and s.thinkingSeed == before.thinkingSeed,"cancel restores position and RNG")
    await process_frame
    map_read("cancel-restored",s)
    return s

func battle_actions(s: Dictionary, finish: bool, lose: bool) -> Dictionary:
    for action in range(200):
        if issue != "" or s.get("failure") != null or not s.has("stage"): return s
        var step := choose_command(s,lose)
        for i in range(1,step.path.size()):
            var p: Dictionary = step.path[i]
            var old: Dictionary = step.path[i-1]
            await key(KEY_RIGHT if p.x > old.x else KEY_LEFT if p.x < old.x else KEY_DOWN if p.y > old.y else KEY_UP)
            s = await map_settle()
            if s.previewX != p.x or s.previewY != p.y:
                issue = "ordinary-battle-movement"
                return s
        await key(KEY_ENTER)
        s = await map_settle()
        await key(KEY_H if step.action == "heal" else KEY_F if step.action == "attack" else KEY_I if step.action == "herb" else KEY_SPACE)
        await process_frame
        s = state()
        if step.action == "herb":
            for slot in range(3):
                if s.failure == null and s.itemSlot != null: break
                await key(KEY_I)
                await process_frame
                s = state()
        if step.target != null:
            for candidate in range(12):
                if s.target == step.target: break
                await key(KEY_TAB)
                await process_frame
                s = state()
            _check(s.target == step.target,"ordinary selected target")
            if not map_seen.has("target-read"):
                map_read("target-selected",s)
                map_seen["target-read"] = true
            target_seen = true
            if not map_seen.has("target-lifetime"):
                map_seen["target-lifetime"] = true
                for attempt in range(12):
                    var accepted: Variant = s.target
                    await key(KEY_TAB)
                    await process_frame
                    s = state()
                    if s.failure != null:
                        _check(s.target == accepted,"rejected candidate retains actual selected target")
                        map_read("target-rejected-candidate",s)
                        map_seen["rejected-target"] = true
                        break
                    map_read("target-switched",s)
                await key(KEY_ESCAPE)
                s = await map_settle()
                map_read("target-cancelled",s)
                continue
        await key(KEY_ENTER)
        s = await map_settle()
        if s.has("stage") and (action < 2 or scene_return_seen and not map_seen.has("scene-return")):
            await process_frame
            map_read("next-player-"+str(action),s)
            if scene_return_seen: map_seen["scene-return"] = true
        if not finish and scene_return_seen and target_seen: return s
    issue = "bounded-battle-actions"
    return s

func run_map() -> void:
    root.size = Vector2i(960,640)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s := state()
    if s.get("sessionId") == null:
        _check(false,"startup:"+str(s.get("failure"))+":"+str(s.get("status")))
        await finish_map()
        return
    castle_identity = s.sessionId
    s = await map_settle()
    if map_case == "missing":
        _check(s.get("failure") == "presentation-unavailable" and s.get("failureVisible",false) and s.presentation.error == "battlefield-map-resource-unavailable","missing map fails visibly from hidden handoff")
        var frozen: int = s.revision
        var wait_token: Variant = s.token
        var cursor_before: Variant = s.cursor
        _check(s.wait == "PresentationWait" and s.presentation.activeCue == "BattleLoad" and s.presentation.battleLoads == 0,"failed BattleLoad is uncompleted")
        for frame in range(5): await process_frame
        await key(KEY_ENTER)
        await key(KEY_RIGHT)
        s = state()
        _check(s.revision == frozen and s.token == wait_token and s.cursor == cursor_before and s.presentation.battleLoads == 0,"failed adapter retains wait and cannot continue gameplay")
        map_samples.append({"label":"visible-missing-resource","failure":s.failure,"visible":s.failureVisible,"presentation":s.presentation,"token":s.token,"revision":s.revision,"cursor":s.cursor})
        await finish_map()
        return
    if s.get("failure") != null:
        _check(false,"startup:"+str(s.failure))
        await finish_map()
        return
    if not s.has("stage"):
        observe_field("initial-field",s)
        if map_case == "battle03":
            await key(KEY_LEFT)
            for frame in range(180):
                OS.delay_msec(16)
                await process_frame
            s = await map_settle()
    _check(s.has("stage"),"real exploration admission reaches battle")
    if not s.has("stage"):
        await finish_map()
        return
    await process_frame
    first_map = view.get_node("MapViewport")
    var initial_map := map_read("desktop-initial",s)
    if map_case == "authored":
        await finish_map()
        return
    var board: Node = first_map.get_node("Board")
    old_texture = weakref(board.get_node("MapSprite_"+str(s.actor)).texture)
    old_map_texture = weakref(instance_from_id(int(initial_map.layers[0].first.texture)))
    old_children = board.get_children().map(func(n): return n.get_instance_id())
    root.size = Vector2i(640,480)
    for frame in range(3): await process_frame
    s = state()
    map_read("small-window",s)
    s = await movement_cancel(s)
    root.size = Vector2i(960,640)
    for frame in range(3): await process_frame
    if map_case == "target-switch":
        for turn in range(6):
            var actor: Dictionary = s.actors.filter(func(a): return a.id == s.actor)[0]
            if actor.mp >= 3 and actor.learned.any(func(p): return p.Value == "heal"): break
            await key(KEY_ENTER)
            await map_settle()
            await key(KEY_SPACE)
            await key(KEY_ENTER)
            s = await map_settle()
        await key(KEY_ENTER)
        await map_settle()
        await key(KEY_H)
        await process_frame
        s = state()
        var initial_target: Variant = s.target
        map_read("self-target",s)
        var switched := false
        for attempt in range(6):
            await key(KEY_TAB)
            await process_frame
            s = state()
            if s.failure == null and s.target != initial_target:
                map_read("accepted-target-switched",s)
                switched = true
                break
        _check(switched,"ordinary accepted target switch moves outline")
        await key(KEY_ESCAPE)
        s = await map_settle()
        map_read("switched-target-cancelled",s)
        await finish_map()
        return
    if map_case != "pilot":
        s = await battle_actions(s,map_case != "battle03",map_case == "battle02-retry")
        _check(scene_seen and scene_return_seen,"actual action scene returns to next actor")
        if s.has("stage"):
            await process_frame
            map_read("action-scene-return",s)
        elif issue == "":
            for frame in range(3): await process_frame
            s = state()
            observe_field("battle-return-field",s)
            _check(old_texture.get_ref() != null and old_map_texture.get_ref() != null,"textures survive transient reparent")
            if map_case == "battle02-retry":
                s = await walk_field(s,Vector2i(32,15),"church-door")
                s = await walk_field(s,Vector2i(0,-1),"west-exit","map-66")
                await process_frame
                _check(s.has("stage"),"ordinary defeat retry reenters battle")
                if s.has("stage"):
                    var rebuilt := map_read("same-session-reentry",s)
                    _check(view.get_node("MapViewport") == first_map and rebuilt.builds == 1,"same encounter retains owner and resources")
                    _check(old_texture.get_ref() != null and old_map_texture.get_ref() != null,"retained textures valid after reentry")
                    s = await movement_cancel(s)
            elif map_case == "transition":
                s = await after_victory(s)
            else:
                await key(KEY_LEFT)
                s = await map_settle()
                observe_field("returned-field-movement",s)
    if issue != "": _check(false,issue)
    await finish_map()

func exercise_battle02() -> void:
    var s := await map_settle()
    for frame in range(3): await process_frame
    _check(old_texture.get_ref() == null and old_map_texture.get_ref() == null,"different-map rebuild releases previous textures")
    _check(old_children.all(func(id): return not is_instance_id_valid(id)),"different-map rebuild releases previous nodes")
    map_seen["second-map"] = true
    var terrain := map_read("different-map-rebuild",s)
    _check(view.get_node("MapViewport") == first_map and terrain.builds == 2,"same owner rebuilt for next encounter")
    var board: Node = first_map.get_node("Board")
    old_texture = weakref(board.get_node("MapSprite_"+str(s.actor)).texture)
    old_map_texture = weakref(instance_from_id(int(terrain.layers[0].first.texture)))
    await movement_cancel(s)

func finish_map() -> void:
    host.queue_free()
    # Give the native audio mixer its ordinary wall-clock opportunity to release
    # stopped voices after final host destruction, independently of fixed FPS.
    for frame in range(10):
        OS.delay_msec(10)
        await process_frame
    if old_texture != null:
        _check(old_texture.get_ref() == null and old_map_texture.get_ref() == null,"final owner destruction releases cached textures")
    var report := {"case":map_case,"passed":failures.is_empty(),"failures":failures,"samples":map_samples,
        "events":map_events,"scene":scene_seen,"sceneReturn":scene_return_seen,"death":death_seen,
        "target":target_seen,"rejectedTarget":map_seen.has("rejected-target"),"movement":movement_seen,"elapsedMs":Time.get_ticks_msec()-started}
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    if output != null: output.store_string(JSON.stringify(report))
    print("SF2_BATTLEFIELD_MAP ",map_case," passed=",failures.is_empty()," failures=",failures)
    quit(0 if failures.is_empty() else 1)
