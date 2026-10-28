extends "res://probes/engine_yeel_to_battle03_observation.gd"

# Independent controlled INITIAL HP200 variation. The bat, RNG, status, queue and
# later resources are never changed by this external keyboard policy.
var sleep_seen := false
var skip_seen := false
var wake_seen := false
var resumed := false
var slept: Dictionary = {}
var observed: Dictionary = {}

func _initialize() -> void:
    call_deferred("run_sleep")

func record_sleep(s: Dictionary) -> void:
    for row in s.get("observations",[]):
        if observed.has(row.Sequence): continue
        observed[row.Sequence]=true
        if row.Kind in ["sleep-inflicted","sleeping-action-skipped","sleep-turn-message","sleep-status","rng-sleep-wake"]:
            events.append({"observation":row,"round":s.round,"queueCursor":s.queueCursor,"mainSeed":s.mainSeed,"actors":s.actors,"scene":s.scene})
            if row.Kind=="sleep-inflicted": sleep_seen=true
            if row.Kind=="sleeping-action-skipped": skip_seen=true
            if row.Kind=="sleep-status" and int(row.After)==0: wake_seen=true
    for actor in s.get("actors",[]):
        if (int(actor.status)&0xC0)!=0: slept[actor.id]=true
    if s.get("stage")=="Movement" and s.stopReason=="PlayerInput" and slept.has(s.actor):
        var active: Dictionary=s.actors.filter(func(a): return a.id==s.actor)[0]
        if int(active.status)==0:
            resumed=true
            samples.append({"label":"awakened-usable-control","state":s})

func sleep_settle() -> Dictionary:
    for frame in range(20000):
        await process_frame
        var s:=state()
        if s.get("sessionId")!=castle_identity or s.get("failure")!=null:
            issue="sleep-session-failure"
            samples.append({"label":issue,"state":s})
            return s
        if not s.has("stage"):
            issue="sleep-unexpected-field-return"
            return s
        record_sleep(s)
        if s.stage!=null and s.stopReason=="PlayerInput": return s
        if s.scene.phase in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
            await key(KEY_ENTER)
    issue="sleep-settle-bound"
    return state()

func flood(s: Dictionary, actor: Dictionary, origin: Dictionary, budget: int, occupied: bool) -> Dictionary:
    var start: int=int(origin.y)*48+int(origin.x)
    var costs: Dictionary={start:0}
    var previous: Dictionary={}
    var pending: Array=[start]
    while not pending.is_empty():
        pending.sort_custom(func(a,b): return costs[a]<costs[b])
        var at: int=pending.pop_front()
        for delta in [1,-1,-48,48]:
            var next: int=at+delta
            if next<0 or next>=2304 or next%48>=int(s.mapWidth) or int(next/48)>=int(s.mapHeight): continue
            if absi(at%48-next%48)+absi(int(at/48)-int(next/48))!=1: continue
            if occupied and s.actors.any(func(a): return a.hp>0 and str(a.id).begins_with("enemy-") and int(a.y)*48+int(a.x)==next): continue
            var surface: String=s.terrain[next]
            if surface in ["Barrier","Impassable","HighSky"]: continue
            var cost: int=2 if surface=="Open" else 5 if actor.mover=="Centaur" and surface in ["Rough","Deep","Desert"] else 4 if surface=="Deep" else 3
            var total: int=costs[at]+cost
            if total<=budget and total<costs.get(next,10000):
                costs[next]=total
                previous[next]=at
                if not pending.has(next): pending.append(next)
    return {"costs":costs,"previous":previous,"start":start}

func approach_bat(s: Dictionary) -> Array:
    var actor: Dictionary=s.actors.filter(func(a): return a.id==s.actor)[0]
    var bat: Dictionary=s.actors.filter(func(a): return a.id=="enemy-2")[0]
    var grid:=flood(s,actor,actor,int(actor.move)*2,true)
    var best: int=grid.start
    var score: int=distance(actor,bat)*100
    for cell in grid.costs:
        var point: Dictionary={"x":int(cell)%48,"y":int(int(cell)/48)}
        if s.actors.any(func(a): return a.id!=s.actor and a.hp>0 and a.x!=null and distance(a,point)==0): continue
        var value: int=distance(point,bat)*100+grid.costs[cell]
        if value<score: score=value;best=cell
    var route: Array=[]
    var at: int=best
    while at!=grid.start:
        var previous: int=grid.previous[at]
        route.push_front(Vector2i(at%48-previous%48,int(at/48)-int(previous/48)))
        at=previous
    return route

func run_sleep() -> void:
    route_scope="independent INITIAL HP200 field variation; actual source Huge Bat sleep critical, skipped entry, after-turn notice and wake/control"
    host=(load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s:=state()
    if s.get("failure")!=null or s.get("sessionId")==null:
        failures.append("sleep-initial-admission:"+str(s.get("failure")))
        samples.append({"label":"startup-failure","state":s})
        await _finish()
        return
    castle_identity=s.sessionId
    s=await bound_castle_settle("sleep-initial-field")
    s=await navigate(s,Vector2i(16,21),"sleep-real-zone")
    if issue=="": s=await sleep_settle()
    samples.append({"label":"sleep-initial-battle","state":s})
    for action in range(160):
        if issue!="" or resumed: break
        if s.stage!="Movement" or s.stopReason!="PlayerInput": issue="sleep-player-readiness";break
        for direction in approach_bat(s):
            await key(_key(direction))
            s=await sleep_settle()
            if issue!="": break
        if issue!="": break
        await key(KEY_ENTER)
        s=await sleep_settle()
        await key(KEY_SPACE)
        await key(KEY_ENTER)
        s=await sleep_settle()
        if action%16==0: samples.append({"label":"sleep-progress-"+str(action),"state":s})
    _check(sleep_seen,"natural source bat sleep infliction")
    _check(skip_seen,"sleeping queued actor skips its action")
    _check(wake_seen,"after-turn wake commits status after message completion")
    _check(resumed,"awakened actor receives usable player control")
    if issue!="": failures.append(issue)
    await _finish()
