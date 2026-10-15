extends "res://probes/engine_battle01_to_battle02_observation.gd"

# Controlled INITIAL entry only. All subsequent movement/combat/retry uses host keys.
var battle_initial: Dictionary = {}
var last_board: Dictionary = {}
var retry_entry: Dictionary = {}
var recording_retry := false
var outcome_events: Array = []
var outcome_seen := {}

func state() -> Dictionary:
    var s:=super.state()
    if recording_retry and retry_entry.is_empty() and s.has("stage"):
        retry_entry=s.duplicate(true)
    for row in s.get("observations",[]):
        if row.Kind in ["battle-outcome","after-battle-join","battle-unlock-cleared","battle-completed-set","defeat-recovered","battle-returned"] and not outcome_seen.has(row.Sequence):
            outcome_seen[row.Sequence]=true
            outcome_events.append(row)
    return s

func _initialize() -> void:
    call_deferred("run_outcome")

func run_outcome() -> void:
    route_scope = "controlled initial Battle02; actual outcome and field control; same-session defeat retry"
    root.size = Vector2i(960,640)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s := state()
    if s.get("failure") != null or s.get("sessionId") == null:
        failures.append("startup: " + str(s.get("failure")))
        samples.append({"label":"startup-failed","state":s})
        await _finish()
        return
    castle_identity = s.sessionId
    s = await bound_castle_settle("initial-battle02")
    battle_initial = s.duplicate(true)
    samples.append({"label":"initial-battle02","state":s})
    var lose := OS.get_environment("SF2_BATTLE_OUTCOME_CASE") == "defeat"
    if issue == "": s = await fight(s,lose)
    samples.append({"label":"outcome-return","state":s})
    if issue == "":
        _check(not s.has("stage") and s.get("canWaitAtInput",false),"usable field return")
        _check(numeric_contains(s.flags,502) != lose and numeric_contains(s.flags,402) == lose,"completion/unlock distinction")
        _check(not numeric_contains(s.flags,403),"no Battle03 request")
        _check(s.map == ("map-3" if lose else "map-66"),"source return map")
        _check(s.audio.error==null and s.cameraProjection.bound,"return audio/projection")
        var projected: Array=s.cameraProjection.actors.filter(func(a): return a.entity=="entity-0")
        _check(projected.size()==1 and projected[0].visible,"actual player projection")
        var returned_leader: Dictionary=s.party.filter(func(a): return a.Actor.Value=="ally-0")[0]
        _check(returned_leader.Hp>0,"living returned leader")
        if lose:
            _check(s.gold==floor(last_board.gold/2),"ordinary defeat halves current gold")
            var leader_before: Dictionary=last_board.actors.filter(func(a): return a.id=="ally-0")[0]
            _check(returned_leader.Hp==leader_before.maxHp and not outcome_events.any(func(e): return e.Kind=="battle-completed-set"),"leader recovery without completion")
            _check(selected_audio.any(func(e): return int(e.Command)==12),"authored defeat cue consumed")
        if not lose:
            _check(s.logicalView.Area.Layer==254 and projected[0].originX==s.logicalView.AX.Position/16 and projected[0].originY==s.logicalView.AY.Position/16,"actual layer254 foreground camera")
            _check(outcome_events.any(func(e): return e.Kind=="after-battle-join" and e.Detail=="0") and outcome_events.any(func(e): return e.Kind=="battle-completed-set" and e.Detail=="502"),"empty after-program join/flag tail")
            var leader: Dictionary=last_board.actors.filter(func(a): return a.id=="ally-0")[0]
            var expected := Vector2i(int(leader.x)+16,int(leader.y)+19)
            _check(Vector2i(int(player(s).x)/384,int(player(s).y)/384)==expected,"live battle coordinates plus area origin")
            _check(int(player(s).facing)==3,"scene reload facing reaches return")
        var before := player(s).duplicate()
        await key(KEY_DOWN if lose else KEY_RIGHT)
        s = await outcome_settle()
        _check(player(s).x != before.x or player(s).y != before.y,"actual field movement")
        samples.append({"label":"field-movement","state":s})
    if issue == "" and lose:
        var returned: Dictionary=s.duplicate(true)
        recording_retry=true
        s=await walk_field(s,Vector2i(32,15),"church-door")
        s=await walk_field(s,Vector2i(0,-1),"retry-west-exit","map-66")
        samples.append({"label":"retry-route-result","state":s})
        if issue!="" or not s.has("stage"):
            failures.append(issue if issue!="" else "retry not admitted")
            await _finish()
            return
        _check(s.sessionId==castle_identity and s.get("stage")=="Movement","same-session retry player control")
        _check(not numeric_contains(s.storyFlags,502) and numeric_contains(s.storyFlags,402),"retry remains incomplete/unlocked")
        _check(s.gold==returned.gold,"retry preserves gold")
        samples.append({"label":"retry-initialized","state":retry_entry})
        for enemy in s.actors.filter(func(a): return str(a.id).begins_with("enemy-")):
            var baseline: Dictionary=battle_initial.actors.filter(func(a): return a.id==enemy.id)[0]
            var fresh: Dictionary=retry_entry.actors.filter(func(a): return a.id==enemy.id)[0]
            # Automatic turns may run before the first observable player boundary.
            # Deployment anchors and explicit source accounting remain observable.
            _check(fresh.hp==baseline.maxHp and fresh.anchorX==baseline.anchorX and fresh.anchorY==baseline.anchorY and fresh.defeats==baseline.defeats and fresh.kills==baseline.kills,"fresh enemy "+enemy.id)
        for member in returned.party:
            var actual: Dictionary=s.actors.filter(func(a): return a.id==member.Actor.Value)[0]
            _check(actual.exp==member.Exp and actual.kills==member.Kills and actual.defeats==member.Defeats,"carried accounting "+actual.id)
            _check(actual.hp==0 if member.Hp==0 else actual.hp==actual.maxHp,"source living-member recovery "+actual.id)
        samples.append({"label":"retry-player-control","state":s})
        var retry := s.duplicate(true)
        await key(KEY_RIGHT)
        s=await outcome_settle()
        await key(KEY_ESCAPE)
        s=await outcome_settle()
        _check(s.previewX==retry.previewX and s.previewY==retry.previewY and s.mainSeed==retry.mainSeed and s.thinkingSeed==retry.thinkingSeed,"retry movement/cancel retains seeds")
        samples.append({"label":"retry-movement-cancel","state":s})
    if issue!="": failures.append(issue)
    events.append({"outcomeTransitions":outcome_events})
    await _finish()

func fight(s: Dictionary, lose: bool) -> Dictionary:
    for action in range(240):
        if not s.has("stage"): return s
        if s.sessionId!=castle_identity or s.failure!=null or s.stage!="Movement" or s.stopReason!="PlayerInput":
            issue="battle02-action-readiness"
            return s
        last_board=s.duplicate(true)
        var step:=choose_command(s,lose)
        events.append({"round":s.round,"actor":s.actor,"actors":s.actors,"decision":step,"gold":s.gold,"mainSeed":s.mainSeed,"thinkingSeed":s.thinkingSeed})
        for i in range(1,step.path.size()):
            var p: Dictionary=step.path[i]
            var old: Dictionary=step.path[i-1]
            await key(KEY_RIGHT if p.x>old.x else KEY_LEFT if p.x<old.x else KEY_DOWN if p.y>old.y else KEY_UP)
            s=await outcome_settle()
            if issue!="": return s
            if s.previewX!=p.x or s.previewY!=p.y:
                issue="battle02-movement-preview"
                return s
        # The terminal actor may be the leader; retain its actual delivered preview.
        last_board=s.duplicate(true)
        for row in last_board.actors:
            if row.id==s.actor: row.x=s.previewX; row.y=s.previewY
        await key(KEY_ENTER)
        s=await outcome_settle()
        await key(KEY_H if step.action=="heal" else KEY_F if step.action=="attack" else KEY_I if step.action=="herb" else KEY_SPACE)
        await process_frame
        s=state()
        if step.action=="herb":
            for slot in range(3):
                if s.failure==null and s.itemSlot!=null: break
                await key(KEY_I)
                await process_frame
                s=state()
        if step.target!=null:
            for candidate in range(12):
                if s.target==step.target: break
                await key(KEY_TAB)
                await process_frame
                s=state()
        if s.failure!=null or (step.target!=null and s.target!=step.target):
            issue="battle02-action-target"
            return s
        await key(KEY_ENTER)
        s=await outcome_settle()
        if issue!="": return s
    issue="battle02-action-bound"
    return s

func choose_command(s: Dictionary, lose: bool) -> Dictionary:
    var result:=super.choose_command(s,lose)
    if lose or result.action in ["heal","herb"]: return result
    var actor: Dictionary=s.actors.filter(func(a): return a.id==s.actor)[0]
    if actor.leader and actor.hp<=6: return result
    var allies: Array=s.actors.filter(func(a): return not str(a.id).begins_with("enemy-") and a.hp>0)
    var enemies: Array=s.actors.filter(func(a): return str(a.id).begins_with("enemy-") and a.hp>0)
    if enemies.size()<=2: return result
    var support: Dictionary=allies.filter(func(a): return a.id=="ally-1")[0] if allies.any(func(a): return a.id=="ally-1") else actor
    var grid:=flood(s,actor,actor,int(actor.move*2),true)
    var best:=1e12
    var destination: Dictionary={"x":actor.x,"y":actor.y,"cell":grid.start}
    result.action="stay"
    result.target=null
    if int(s.round)==1:
        var formation: Dictionary={"ally-0":Vector2i(15,13),"ally-1":Vector2i(16,15),"ally-2":Vector2i(14,14),"ally-3":Vector2i(13,13)}
        var goal: Vector2i=formation[s.actor]
        for cell in grid.costs:
            var p: Dictionary={"x":int(cell)%48,"y":int(int(cell)/48),"cell":int(cell)}
            if allies.any(func(a): return a.id!=actor.id and distance(a,p)==0): continue
            var score: float=(absi(p.x-goal.x)+absi(p.y-goal.y))*100+grid.costs[cell]
            if score<best: best=score; destination=p
        var path: Array=[destination]
        var cell: int=destination.cell
        while cell!=grid.start:
            cell=grid.previous[cell]
            path.push_front({"x":cell%48,"y":int(cell/48)})
        result.path=path
        return result
    for foe in enemies:
        for cell in grid.costs:
            var p: Dictionary={"x":int(cell)%48,"y":int(int(cell)/48),"cell":int(cell)}
            if allies.any(func(a): return a.id!=actor.id and distance(a,p)==0): continue
            if distance(p,foe)!=1 or distance(p,support)>3: continue
            var score: float=threat(p,enemies,actor,foe)*1000+foe.hp*20+grid.costs[cell]
            if score<best:
                best=score
                destination=p
                result.action="attack"
                result.target=foe.id
    if result.target==null:
        # Keep the force together while the source AI approaches; advance only
        # when the nearest enemy is outside its approach pocket.
        var nearest:=100
        for foe in enemies: nearest=mini(nearest,distance(actor,foe))
        if nearest>8: return super.choose_command(s,false)
    var path: Array=[destination]
    var cell: int=destination.cell
    while cell!=grid.start:
        cell=grid.previous[cell]
        path.push_front({"x":cell%48,"y":int(cell/48)})
    result.path=path
    return result
