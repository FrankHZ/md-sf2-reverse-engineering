extends "res://probes/engine_battle01_outcome_observation.gd"

# Actual host input and read-only state. The pilot is an explicitly separate controlled
# entry; no checkpoint mutation, seed reset, screenshots or framebuffer reads.
var samples: Array = []
var events: Array = []
var failures: Array = []
var seen := 0
var phases: Dictionary = {}
var started := Time.get_ticks_msec()
var commission_refused := false
var selected_audio: Array = []
var seen_audio: Dictionary = {}
var music_loop: Array = []
var route_scope := "controlled Battle02 pilot; continuous story route not established"

func _initialize() -> void:
    call_deferred("_run" if OS.get_cmdline_user_args().has("--private-battle-start") else "_run_continuous")

func state() -> Dictionary:
    var current := super.state()
    observe_audio(current)
    return current

func observe_audio(current: Dictionary) -> void:
    for receipt in current.get("audio",{}).get("receipts",[]):
        if int(receipt.Command) not in [1,12,19,110,111] or seen_audio.has(receipt.Sequence): continue
        seen_audio[receipt.Sequence]=true
        if selected_audio.size()<256: selected_audio.append(receipt)
        else: issue="selected-audio-bound"

func capture_row(_channel: String, row: Dictionary, legacy: Array) -> void:
    var compact := row.duplicate()
    compact.erase("state")
    if legacy.size() < 256: legacy.append(compact)

func choose_command(s: Dictionary, lose: bool) -> Dictionary:
    print("SF2_PLAYER_TURN round=",s.round," actor=",s.actor)
    var decision := super.choose_command(s,lose)
    var actor: Dictionary = s.actors.filter(func(a): return a.id==s.actor)[0]
    capture_row("board", {"label":"live-turn","round":s.round,"actor":s.actor,"actors":s.actors},outcome_records)
    if lose: return decision
    var healer: bool = actor.learned.any(func(spell): return spell.Value=="heal")
    var inventory: Array = s.inventories.filter(func(row): return row.actor==s.actor)
    var herb: bool = not inventory.is_empty() and inventory[0].items.any(func(item): return (int(item)&127)==0)
    if not (healer and actor.mp>=3) and not herb: return decision
    var grid := flood(s,actor,actor,int(actor.move*2),true)
    var allies: Array = s.actors.filter(func(a): return not str(a.id).begins_with("enemy-") and a.hp>0)
    var enemies: Array = s.actors.filter(func(a): return str(a.id).begins_with("enemy-") and a.hp>0)
    var selected: Dictionary = {}
    var score := 1e12
    for ally in allies:
        if ally.maxHp-ally.hp<5: continue
        for cell in grid.costs:
            var p := {"x":int(cell)%48,"y":int(int(cell)/48),"cell":int(cell)}
            if allies.any(func(a): return a.id!=actor.id and distance(a,p)==0): continue
            if ally.id!=actor.id and distance(ally,p)>1: continue
            var value: float=ally.hp*1000+threat(p,enemies,actor)*20+grid.costs[cell]
            if value<score:
                score=value
                selected=p
                decision.target=ally.id
    if selected.is_empty(): return decision
    decision.action="heal" if healer and actor.mp>=3 else "herb"
    var path: Array=[selected]
    var cell: int=selected.cell
    while cell!=grid.start:
        cell=grid.previous[cell]
        path.push_front({"x":cell%48,"y":int(cell/48)})
    decision.path=path
    return decision

func play_battle01(s: Dictionary) -> Dictionary:
    var lose := OS.get_environment("SF2_BATTLE_OUTCOME_CASE") == "defeat"
    var initial_gold: int = s.gold
    for action in range(200):
        if not s.has("stage"): break
        if s.sessionId != castle_identity or s.failure != null or s.stage != "Movement" or s.stopReason != "PlayerInput":
            issue = "native-action-readiness"
            break
        if not valid_projection(s, true):
            issue = "battle-viewport"
            break
        if capture != null:
            # The movement policy needs terrain; per-frame observations stay lightweight.
            capture_full_state = true
            s = state()
            capture_full_state = false
        var step := choose_command(s, lose)
        capture_row("outcomeRecords",{"label":"action-selected","decision":step,"state":s}, outcome_records)
        for i in range(1, step.path.size()):
            var p: Dictionary = step.path[i]
            var old: Dictionary = step.path[i - 1]
            await key(KEY_RIGHT if p.x > old.x else KEY_LEFT if p.x < old.x else KEY_DOWN if p.y > old.y else KEY_UP)
            s = await outcome_settle()
            if s.sessionId != castle_identity or s.actor != step.actor or s.stage != "Movement" or s.stopReason != "PlayerInput" or s.failure != null or s.previewX != p.x or s.previewY != p.y:
                issue = "native-movement-preview"
                break
        if issue != "": break
        await key(KEY_ENTER)
        s = await outcome_settle()
        if s.get("stage") != "ActionChoice" or s.actor != step.actor:
            issue = "native-action-choice"
            break
        await key(KEY_H if step.action == "heal" else KEY_F if step.action == "attack" else KEY_I if step.action == "herb" else KEY_SPACE)
        await process_frame
        s = state()
        if step.action == "herb":
            for slot in range(3):
                if s.failure == null and s.itemSlot != null: break
                await key(KEY_I)
                await process_frame
                s = state()
        if s.failure != null or s.sessionId != castle_identity or s.actor != step.actor or s.stage not in ["TargetChoice", "CommitReady"]:
            issue = "native-action-selection"
            break
        if step.target != null:
            for candidate in range(12):
                if s.target == step.target: break
                await key(KEY_TAB)
                await process_frame
                s = state()
            if s.target != step.target or s.failure != null:
                issue = "native-target-selection"
                break
        await key(KEY_ENTER)
        s = await outcome_settle()
        capture_row("outcomeRecords",{"actor":step.actor,"round":step.round,"action":step.action,"target":step.target,"map":s.get("map"),"failure":s.failure}, outcome_records)
        if issue != "": break
    if issue == "":
        if s.has("stage") or s.stop != "PlayerInput" or s.wait != null or s.battleMounted or not held_battle:
            issue = "outcome-return-control"
        elif numeric_contains(s.flags, 501) == lose or numeric_contains(s.flags, 401) != lose:
            issue = "outcome-flags-or-gold"
        elif lose and (s.map != "map-3" or after_sounds < 1 or after_programs.has("abcs-battle01")):
            issue = "ordinary-defeat-path"
        elif not lose and (s.map != "map-57" or after_mosaic <= 0 or (after_white if OS.get_environment("SF2_H4_VARIANT") in ["C", "D"] else not after_white) or not after_programs.has("abcs-battle01")):
            issue = "victory-after-program"
        else:
            var viewport := Rect2(s.viewport.x, s.viewport.y, s.viewport.width, s.viewport.height)
            var board := Rect2(s.mapViewport.x, s.mapViewport.y, s.mapViewport.width, s.mapViewport.height)
            if s.viewport.width != 960 or s.viewport.height != 640 or not board.has_area() or not viewport.encloses(board):
                issue = "return-viewport"
            var before := player(s)
            if not OS.get_environment("SF2_H4_VARIANT").is_empty():
                await settled_endpoint("first-return")
            await key(KEY_LEFT)
            s = await outcome_settle()
            if player(s).x == before.x and player(s).y == before.y: issue = "actual-return-movement"
            if not OS.get_environment("SF2_H4_VARIANT").is_empty() and issue == "":
                await settled_endpoint("before-down")
                var down_before := player(s)
                await key(KEY_DOWN)
                s = await outcome_settle()
                if player(s).x != down_before.x or player(s).y != down_before.y + 384: issue = "actual-return-down"
                await settled_endpoint("after-down")
    if capture == null:
        var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT") + ".outcome.json", FileAccess.WRITE)
        file.store_string(JSON.stringify({"issue":issue,"lose":lose,"initialGold":initial_gold,"records":outcome_records,"programs":after_programs.keys(),
            "heldBattle":held_battle,"mosaicOutDraws":after_mosaic,"whiteSeen":after_white,"soundStarts":after_sounds,"endpoints":endpoint_records,"final":s}, "  "))
        file.close()
    else:
        capture_row("outcomeSummary",{"issue":issue,"lose":lose,"initialGold":initial_gold,"programs":after_programs.keys(),
            "heldBattle":held_battle,"mosaicOutDraws":after_mosaic,"whiteSeen":after_white,"soundStarts":after_sounds,"final":s}, [])
    return s

func bound_castle_settle(label: String, yes: bool = true) -> Dictionary:
    for frame in range(30000):
        await process_frame
        var s := state()
        if s.failure != null or s.sessionId != castle_identity:
            issue = label + ":session-failure"
            return s
        if s.has("stage") or s.get("mode") == "Battle":
            return await second_admission(label) if numeric_contains(s.flags,402) else await admission_settle(label)
        observe_castle_frame(s)
        if s.map == "map-21" and s.wait == "EntitySpriteWait" and s.cursor.Program == "cs-53ef4": guard_sprite_wait = s
        if s.get("canWaitForText", false):
            if not seen_texts.has(s.token):
                seen_texts[s.token] = true
                texts.append({"id":s.textId,"program":s.cursor})
            await key(KEY_ENTER)
        elif s.wait == "ChoiceWait":
            if s.cursor.Program in ["cs-53482", "cs-53176"] and not commission_refused:
                commission_refused = true
                await key(KEY_X)
            else:
                await key(KEY_ENTER if yes else KEY_X)
        elif s.get("canWaitAtInput", false) and not player(s).get("moving", true): return s
    issue = label + ":bounded-field-wait"
    return state()

func _run_continuous() -> void:
    route_scope = "controlled initial tower; same-session Battle01 victory through castle/Jaha to Battle02 actions"
    root.size = Vector2i(960,640)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s := state()
    if s.get("failure") != null or s.get("sessionId") == null:
        samples.append({"label":"startup-failed","state":s})
        failures.append("continuous startup: " + str(s.get("failure")))
        _finish()
        return
    castle_identity = s.sessionId
    s = await bound_castle_settle("opening-start")
    if s.has("stage"):
        if OS.get_environment("SF2_ROUTE_STARTUP_ONLY")=="1":
            route_scope="tower startup and first Battle01 movement/cancel; clock selected by launcher"
            var initial := s.duplicate(true)
            await key(KEY_LEFT)
            s=await outcome_settle()
            _check(s.previewX!=initial.previewX or s.previewY!=initial.previewY,"startup actual movement preview")
            await key(KEY_ESCAPE)
            s=await outcome_settle()
            _check(s.previewX==initial.previewX and s.previewY==initial.previewY and s.mainSeed==initial.mainSeed,"startup cancel retains state")
        else:
            if issue == "": s = await play_battle01(s)
            if OS.get_environment("SF2_BATTLE_OUTCOME_CASE")=="defeat":
                route_scope="same-session ordinary Battle01 defeat and usable Map3 return; authored defeat audio"
            elif issue == "": s = await after_victory(s)
        samples.append({"label":"continuous-progress","state":s})
        if issue != "": failures.append(issue)
        _finish()
        return
    var plan: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../../tests/fixtures/h3/map3-battle01-natural-route-v1.json"))
    for step in plan.expectedObservation.records[0].logicalInputTrace:
        if issue != "" or numeric_contains(s.get("flags",[]),603): break
        var p := player(s)
        if p.get("x") != step.x * 384 or p.get("y") != step.y * 384:
            issue = "opening-route-position"
            break
        await key(input_code(step.input))
        s = await bound_castle_settle("opening")
    if issue == "": s = await castle_route(s, true)
    if issue == "": s = await castle_route(s, false, "map20-to-map19-royal-return")
    if issue == "":
        var entry: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../../tests/fixtures/h3/map3-battle01-player-ready-v1.json"))
        for step in entry.static.inputPlan:
            var p := player(s)
            if s.map != "map-"+str(int(step.from.map)) or p.x != step.from.x*384 or p.y != step.from.y*384:
                issue = "battle01-entry-position"
                break
            await key(input_code(step.input))
            s = await bound_castle_settle(step.waypoint)
            if issue != "": break
    if issue == "": s = await play_battle01(s)
    samples.append({"label":"continuous-progress","state":s})
    if issue != "": failures.append(issue)
    _finish()

# Navigation reads the live layout in bounded regions; only semantic endpoints are saved.
# Search chooses keyboard inputs. The actual controller owns collision, events and warps.
func walk_field(s: Dictionary, goal: Vector2i, label: String, expected_map: String = "") -> Dictionary:
    var origin_map: String = s.map
    var words: Array = []
    for y in range(0,64,4):
        var region: Dictionary = view.call("ReadLayoutRegion",0,y,64,4)
        words.append_array(region.words)
    var rejected := {}
    for move in range(256):
        if issue != "": return s
        var origin := Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if origin==goal: return s
        var queue: Array[Vector2i] = [origin]
        var paths := {origin:[]}
        var selected: Array = []
        while not queue.is_empty():
            var at: Vector2i = queue.pop_front()
            for d in [Vector2i.DOWN,Vector2i.RIGHT,Vector2i.UP,Vector2i.LEFT]:
                var n: Vector2i = at+d
                if n.x<0 or n.y<0 or n.x>=64 or n.y>=64: continue
                var collision: int = int(words[at.y*64+at.x])&0xC000
                if d.x!=0 and collision in [0x4000,0x8000]:
                    var stair_y: int = at.y + (d.x if collision==0x4000 else -d.x)
                    if stair_y>=0 and stair_y<64 and (int(words[stair_y*64+n.x])&0xC000)==collision: n.y=stair_y
                if paths.has(n) or rejected.has(str([at,d])): continue
                var word: int = words[n.y*64+n.x]
                var target: bool = (goal.x<0 or n.x==goal.x) and (goal.y<0 or n.y==goal.y)
                if target and (goal.x<0 or goal.y<0): target=(word&0x3C00)==0x1000
                if (word&0xC000)==0xC000 and not target: continue
                if (word&0x3C00)==0x1000 and not target: continue
                if s.entities.any(func(e): return e.id!="entity-0" and e.Visible and (int(e.flagsA)&0x20)!=0 and absi(int(e.x)-n.x*384)<384 and absi(int(e.y)-n.y*384)<384): continue
                paths[n]=paths[at]+[d]
                if target:
                    selected=paths[n]
                    queue.clear()
                    break
                queue.append(n)
        if selected.is_empty():
            issue=label+":no-live-layout-path"
            return s
        var first: Vector2i=selected[0]
        await key(KEY_RIGHT if first.x>0 else KEY_LEFT if first.x<0 else KEY_DOWN if first.y>0 else KEY_UP)
        s=await bound_castle_settle(label)
        if issue!="": return s
        if s.has("stage") or s.map!=origin_map or (expected_map!="" and selected.size()==1):
            if s.get("map")!=expected_map and not s.has("stage"): issue=label+":unexpected-map"
            samples.append({"label":label,"state":s})
            return s
        var reached := Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if reached==origin: rejected[str([origin,first])]=true
        if reached==goal:
            samples.append({"label":label,"state":s})
            return s
    issue=label+":navigation-bound"
    return s

func after_victory(s: Dictionary) -> Dictionary:
    samples.append({"label":"actual-battle01-victory-return","state":s})
    for step in [
        [Vector2i(-1,19),"tower-exit","map-40"],
        [Vector2i(-1,31),"passage-exit","map-21"],
        [Vector2i(3,16),"castle-entrance","map-20"],
        [Vector2i(6,37),"castle-lower","map-19"],
        [Vector2i(23,3),"royal-approach","map-20"],
        [Vector2i(25,35),"royal-local-warp","map-20"],
        [Vector2i(27,52),"exorcism-and-commission","map-19"],
    ]:
        if issue!="": return s
        s=await walk_field(s,step[0],step[1],step[2])
    if issue=="" and (not commission_refused or not numeric_contains(s.flags,609) or not numeric_contains(s.flags,610)):
        issue="commission-return-flags"
    if issue=="": s=await walk_field(s,Vector2i(-1,31),"return-to-granseal","map-3")
    if issue=="": s=await walk_field(s,Vector2i(2,27),"jaha-join")
    if issue=="" and (not numeric_contains(s.flags,3) or not numeric_contains(s.flags,402) or numeric_contains(s.flags,66)):
        issue="jaha-caller-flags"
    if issue=="": s=await walk_field(s,Vector2i(0,-1),"west-exit","map-66")
    if issue=="":
        await exercise_battle02()
        if failures.is_empty() and OS.get_environment("SF2_ROUTE_MUSIC_LOOP")=="1":
            await observe_music_loop()
        s=state()
    return s

func observe_music_loop() -> void:
    var deadline := Time.get_ticks_msec()+110000
    var next_sample := 0
    var previous := -1.0
    while Time.get_ticks_msec()<deadline:
        await process_frame
        if Time.get_ticks_msec()<next_sample: continue
        next_sample=Time.get_ticks_msec()+500
        var current := _board()
        var audio: Dictionary=JSON.parse_string(host.call("ReadAudioObservationJson"))
        music_loop.append({"ms":Time.get_ticks_msec(),"cue":audio.musicCue,"playing":audio.musicPlaying,"position":audio.musicPosition,"error":audio.error})
        if audio.error!=null or audio.musicCue!="MUSIC_MAIN_THEME" or not audio.musicPlaying:
            _check(false,"main theme continued playback"); return
        var position: float=audio.musicPosition
        if previous-position>1.0: return
        previous=position
    _check(false,"main theme actual loop wrap within bounded observation")

func second_admission(label: String) -> Dictionary:
    for tick in range(16000):
        await process_frame
        var s := state()
        if s.failure!=null or s.sessionId!=castle_identity:
            issue=label+":battle02-admission-failure"
            return s
        if s.has("stage"): return await _settle()
        if s.get("canWaitForText",false): await key(KEY_ENTER)
    issue=label+":battle02-admission-timeout"
    return state()

func _check(value: bool, message: String) -> void:
    if not value:
        failures.append(message)
        push_error(message)

func _press(key: Key) -> void:
    var event := InputEventKey.new()
    event.keycode = key
    event.pressed = true
    Input.parse_input_event(event)
    await process_frame
    event = InputEventKey.new()
    event.keycode = key
    event.pressed = false
    Input.parse_input_event(event)
    await process_frame

func _result(value: String) -> void:
    var result: Dictionary = JSON.parse_string(value)
    for event in result.observations:
        if event.Sequence > seen:
            seen = int(event.Sequence)
            events.append(event)

func _board() -> Dictionary:
    return JSON.parse_string(view.call("ReadObservationJson"))

func _sample(label: String) -> Dictionary:
    var state := _board()
    samples.append({"label":label,"state":state})
    return state

func _settle() -> Dictionary:
    for tick in range(6000):
        var state: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
        if state.failure != null:
            _check(false, "host failure: " + str(state.failure))
            return _sample("failed")
        if state.movement != null:
            await process_frame
        elif state.scene.visible or state.scene.fieldDeath != null:
            var key := str([state.scene.waitToken,state.scene.phase])
            if not phases.has(key): phases[key] = state
            if state.scene.phase in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
                await _press(KEY_ENTER)
            else:
                await process_frame
        elif state.actor != null:
            return _board()
        else:
            await process_frame
    _check(false,"bounded wait for real player control")
    return _sample("timeout")

func _route(board: Dictionary) -> Array:
    var enemies: Array = board.actors.filter(func(a): return str(a.id).begins_with("enemy-") and a.hp > 0 and a.x != null)
    var occupied := {}
    for actor in board.actors:
        if actor.hp > 0 and actor.x != null and actor.id != board.actor:
            occupied[Vector2i(int(actor.x),int(actor.y))] = true
    var origin := Vector2i(int(board.previewX),int(board.previewY))
    var queue: Array[Vector2i] = [origin]
    var paths := {origin:[]}
    while not queue.is_empty():
        var cell: Vector2i = queue.pop_front()
        if not occupied.has(cell) and enemies.any(func(a): return abs(cell.x-int(a.x))+abs(cell.y-int(a.y))==1): return paths[cell]
        for direction in [Vector2i.UP,Vector2i.LEFT,Vector2i.DOWN,Vector2i.RIGHT]:
            var next: Vector2i = cell+direction
            if next.x<0 or next.y<0 or next.x>=int(board.mapWidth) or next.y>=int(board.mapHeight): continue
            if paths.has(next) or enemies.any(func(a): return int(a.x)==next.x and int(a.y)==next.y) or board.terrain[next.y*48+next.x] in ["Barrier","Impassable"]: continue
            paths[next] = paths[cell]+[direction]
            queue.append(next)
    return []

func _key(direction: Vector2i) -> Key:
    return KEY_W if direction==Vector2i.UP else KEY_S if direction==Vector2i.DOWN else KEY_A if direction==Vector2i.LEFT else KEY_D

func _run() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        _check(false,"ordinary battle presenter exists")
        _finish()
        return
    await exercise_battle02()
    _finish()

func exercise_battle02() -> void:
    view.connect("SessionResultObserved",_result)
    var initial := await _settle()
    if not failures.is_empty():
        return
    _sample("initial-control")
    _check(initial.mapWidth==22 and initial.mapHeight==18,"source Battle02 geometry")
    _check(initial.actors.size()==10,"four allies and six enemies")
    _check(initial.map=="map66","actual Battle02 map")
    _check(initial.boardChildren==22*18+20+initial.previewNodes,"fresh terrain and ten actor/sprite node pairs")
    _check(initial.actors.all(func(a): return a.sprite!=null and a.sprite.texturePresent),"all actual battlefield sprites")
    if route_scope.begins_with("controlled initial tower"):
        _check(initial.sessionId==castle_identity,"same session through second encounter")
        for flag in [3,35,402,451,452,501,609,610]:
            _check(numeric_contains(initial.storyFlags,flag),"continuous source flag "+str(flag))
        _check(not numeric_contains(initial.storyFlags,66),"Jaha caller clears66")
    var path := _route(initial)
    _check(not path.is_empty(),"reachable ordinary movement")
    # An ally may be crossed but cannot be the committed destination.
    for key in [KEY_D,KEY_S,KEY_A,KEY_W]:
        await _press(key)
        var candidate := _board()
        if not candidate.actors.any(func(a): return a.id!=candidate.actor and a.hp>0 and a.x==candidate.previewX and a.y==candidate.previewY): break
        await _press(KEY_ESCAPE)
    var preview := _sample("preview")
    _check(preview.previewX!=initial.previewX or preview.previewY!=initial.previewY,"movement preview changes")
    await _press(KEY_ENTER)
    await _settle()
    await _press(KEY_ESCAPE)
    await _settle()
    var cancelled := _sample("cancelled")
    _check(cancelled.previewX==initial.previewX and cancelled.previewY==initial.previewY and cancelled.mainSeed==initial.mainSeed,
        "cancel restores committed location without RNG")
    for turn in range(20):
        if not failures.is_empty(): break
        var board := _board()
        path = _route(board)
        var walked: Array = []
        for direction in path:
            var previous := _board()
            await _press(_key(direction))
            # Movement preview has its own presentation delivery; the next key is ordinary input.
            var scene: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
            while scene.movement != null:
                await process_frame
                scene = JSON.parse_string(view.call("ReadSceneObservationJson"))
            var next := _board()
            if next.previewX==previous.previewX and next.previewY==previous.previewY: break
            walked.append(direction)
        board = _board()
        while board.actors.any(func(a): return a.id!=board.actor and a.hp>0 and a.x==board.previewX and a.y==board.previewY) and not walked.is_empty():
            await _press(_key(-walked.pop_back()))
            await _settle()
            board = _board()
        await _press(KEY_ENTER)
        await _settle()
        board = _board()
        var target := ""
        for actor in board.actors:
            if str(actor.id).begins_with("enemy-") and actor.hp>0 and actor.x!=null and abs(actor.x-board.previewX)+abs(actor.y-board.previewY)==1:
                target = actor.id
                break
        if target.is_empty():
            await _press(KEY_SPACE)
        else:
            await _press(KEY_F)
            for candidate in range(10):
                if _board().target==target: break
                await _press(KEY_TAB)
        await _press(KEY_ENTER)
        await _settle()
        _sample("turn-"+str(turn))
        var ally_hit := events.any(func(e): return e.Kind=="physical-first" and str(e.Actor.Value).begins_with("ally-"))
        var enemy_hit := events.any(func(e): return e.Kind=="physical-first" and str(e.Actor.Value).begins_with("enemy-"))
        if ally_hit and enemy_hit: break
    _check(events.any(func(e): return e.Kind=="physical-first" and str(e.Actor.Value).begins_with("ally-")),"real player physical action")
    _check(events.any(func(e): return e.Kind=="physical-first" and str(e.Actor.Value).begins_with("enemy-")),"real enemy physical action returns to player")

func _finish() -> void:
    var report := {"scope":route_scope, "passed":failures.is_empty(),
        "failures":failures,"elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events,"phases":phases,
        "commissionRefused":commission_refused,"selectedAudio":selected_audio,"musicLoop":music_loop}
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    if output != null:
        output.store_string(JSON.stringify(report))
        output.close()
    print("SF2_ROUTE_OBSERVATION "+JSON.stringify({"passed":failures.is_empty(),"failures":failures}))
    if host != null: host.queue_free()
    await process_frame
    await process_frame
    # AudioServer releases stopped playback on its real audio thread. Accelerated
    # simulation frames alone do not give that thread a mixing opportunity.
    var cleanup_deadline := Time.get_ticks_msec()+250
    while Time.get_ticks_msec()<cleanup_deadline: await process_frame
    quit(0 if failures.is_empty() else 1)
