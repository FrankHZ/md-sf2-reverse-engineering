extends "res://probes/engine_battle01_to_battle02_observation.gd"

# One controlled initial field entry. Every subsequent state is produced by real host keys.
var yeel_texts: Dictionary = {}
var request_samples: Array = []
var intro_sample: Dictionary = {}
var flash_samples: Array = []
var seen_flash: Dictionary = {}
var skipped := OS.get_environment("SF2_YEEL_CASE") == "skip"
var realtime_encounter := false

func observe_audio(current: Dictionary) -> void:
    super.observe_audio(current)
    for receipt in current.get("audio",{}).get("receipts",[]):
        if int(receipt.Command)!=109 or seen_audio.has(receipt.Sequence): continue
        seen_audio[receipt.Sequence]=true
        selected_audio.append(receipt)

func _initialize() -> void:
    call_deferred("run_yeel")

func bound_castle_settle(label: String, _yes: bool = true) -> Dictionary:
    for frame in range(16000):
        await process_frame
        if realtime_encounter: OS.delay_msec(16)
        var s := state()
        if s.get("failure") != null or s.get("sessionId") != castle_identity:
            issue=label+":session-failure"
            samples.append({"label":issue,"state":s})
            return s
        if s.has("stage"): return await _settle()
        var text_id: int = int(s.textId) if s.textId != null else -1
        if text_id in [784,785,786,787] and not yeel_texts.has(text_id):
            yeel_texts[text_id]={"id":text_id,"speaker":s.speaker,"portrait":s.portraitWindow,"entityEvent":s.entityEvent}
        if s.get("battleRequest",false) and request_samples.is_empty(): request_samples.append(s.duplicate(true))
        if s.get("egressMap")=="map-5" and intro_sample.is_empty(): intro_sample=s.duplicate(true)
        if s.get("presentationCue") is Dictionary and s.presentationCue.get("kind")=="FlashWhite":
            var p: Dictionary=s.presentationCue
            var phase: String=str([p.get("flashPulse"),p.whiteOpacity])
            if not seen_flash.has(phase):
                seen_flash[phase]=true
                flash_samples.append(p.duplicate(true))
        if s.get("canWaitForText",false): await key(KEY_ENTER)
        elif s.get("canWaitAtInput",false) and not player(s).get("moving",true): return s
    issue=label+":bounded-settle"
    return state()

func navigate(s: Dictionary, goal: Vector2i, label: String, expected_map: String="") -> Dictionary:
    var origin_map: String=s.map
    var rejected: Dictionary={}
    for movement in range(200):
        if issue!="" or s.has("stage"): return s
        var origin:=Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if origin==goal: return s
        var words: Array=[]
        for y in range(0,64,4): words.append_array(view.call("ReadLayoutRegion",0,y,64,4).words)
        var queue: Array[Vector2i]=[origin]
        var paths: Dictionary={origin:[]}
        var selected: Array=[]
        while not queue.is_empty():
            var at: Vector2i=queue.pop_front()
            for d in [Vector2i.UP,Vector2i.LEFT,Vector2i.DOWN,Vector2i.RIGHT]:
                var n: Vector2i=at+d
                if n.x<0 or n.y<0 or n.x>=64 or n.y>=64: continue
                var collision: int=int(words[at.y*64+at.x])&0xC000
                if d.x!=0 and collision in [0x4000,0x8000]:
                    var stair_y: int=at.y+(d.x if collision==0x4000 else -d.x)
                    if stair_y>=0 and stair_y<64 and (int(words[stair_y*64+n.x])&0xC000)==collision: n.y=stair_y
                if paths.has(n) or rejected.has(str([at,d])): continue
                var word: int=words[n.y*64+n.x]
                var marker: int=word&0x3C00
                var target: bool=n==goal
                if (word&0xC000)==0xC000 and marker!=0x400 and not target: continue
                if marker==0x1000 and not target: continue
                if origin_map=="map-66" and marker==0x1400 and n.y not in [9,10,28] and not target: continue
                if s.entities.any(func(e): return e.id!="entity-0" and e.Visible and (int(e.flagsA)&0x80)!=0 and absi(int(e.x)-n.x*384)<256 and absi(int(e.y)-n.y*384)<256): continue
                paths[n]=paths[at]+[d]
                if target:
                    selected=paths[n]
                    queue.clear()
                    break
                queue.append(n)
        if selected.is_empty():
            issue=label+":no-working-layout-path"
            return s
        var direction: Vector2i=selected[0]
        await key(_key(direction))
        s=await bound_castle_settle(label)
        if issue!="": return s
        if s.has("stage") or s.map!=origin_map:
            _check(s.has("stage") or s.map==expected_map,label+":expected-map")
            samples.append({"label":label,"state":s})
            return s
        var reached:=Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if reached==origin: rejected[str([origin,direction])]=true
    issue=label+":navigation-bound"
    return s

func run_yeel() -> void:
    route_scope="controlled accepted Battle02 return; actual Yeel guide/reentry/zone request and Battle03 actions in one session"
    if skipped: route_scope="independent controlled INITIAL field zone approach, F72/F502 clear; no Kazin visit before actual Battle03 admission"
    root.size=Vector2i(960,640)
    host=(load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s:=state()
    if s.get("failure")!=null or s.get("sessionId")==null:
        failures.append("initial-content-admission:"+str(s.get("failure")))
        samples.append({"label":"startup-failure","state":s})
        await _finish()
        return
    castle_identity=s.sessionId
    s=await bound_castle_settle("initial-return")
    samples.append({"label":"initial-return","state":s})
    var initial:=s.duplicate(true)
    _check(s.map=="map-66" and s.party.size()==4,"four-member accepted field return")
    _check(not numeric_contains(s.flags,402) and numeric_contains(s.flags,502)!=skipped,"declared initial outcome flags")
    if not skipped:
        s=await navigate(s,Vector2i(19,23),"south-yeel-warp","map-5")
        if issue=="": s=await navigate(s,Vector2i(7,7),"walk-to-kazin")
        if issue=="":
            await key(KEY_UP)
            s=await bound_castle_settle("face-kazin")
            await key(KEY_ENTER)
            s=await bound_castle_settle("kazin-offer")
            samples.append({"label":"following","state":s})
            _check(numeric_contains(s.flags,72) and not numeric_contains(s.flags,4) and not numeric_contains(s.flags,403),"follower without recruitment/unlock")
            _check(s.party==initial.party and s.gold==initial.gold,"conversation preserves party resources/accounting/growth")
            _check(yeel_texts.size()==4 and yeel_texts[786].speaker==null and yeel_texts[787].speaker=="entity-4","fixed text IDs and narrative speaker")
        if issue=="": s=await navigate(s,Vector2i(7,0),"north-exit","map-66")
        if issue=="": s=await navigate(s,Vector2i(18,22),"north-reentry","map-5")
        if issue=="":
            var kazin: Array=s.entities.filter(func(e): return e.id=="entity-4" and e.Visible)
            _check(kazin.size()==1 and kazin[0].follower!=null,"one following Kazin after reentry")
            samples.append({"label":"one-kazin-reentry","state":s})
            s=await navigate(s,Vector2i(7,0),"return-to-field","map-66")
    if issue=="":
        s=await navigate(s,Vector2i(17,21),"approach-real-zone")
        if issue=="":
            # Give the real audio mixer time to deliver the finite cue's Finished
            # callback while the ordinary 32-tick flash advances; no callback injection.
            realtime_encounter=true
            s=await navigate(s,Vector2i(16,21),"actual-zone-request")
            realtime_encounter=false
    if issue=="":
        _check(s.has("stage") and s.map=="map66" and s.actors.filter(func(a): return str(a.id).begins_with("enemy-")).size()==6,"source Battle03 selected")
        _check(not request_samples.is_empty(),"pending request observed")
        _check(not intro_sample.is_empty() and intro_sample.battleRequest==false and intro_sample.egressMap=="map-5","request consumed and reached Egress writer")
        _check(flash_samples.any(func(p): return p.get("flashPulses",0)==4),"four short flash pulses projected")
        _check(intro_sample.presentationCue.get("whiteOpacity",-1)==0,"flash overlay restored")
        _check(flash_samples.all(func(p): return p.whiteOpacity==0) if initial.reducedFlash else flash_samples.any(func(p): return p.whiteOpacity==1),"reduced flash opacity follows settings")
        _check(intro_sample.display==initial.display,"encounter flash preserves logical palette and brightness")
        _check(selected_audio.any(func(a): return int(a.Command)==109 and a.Operation=="started") and selected_audio.any(func(a): return int(a.Command)==109 and a.Operation=="finished"),"actual encounter audio start/finish callbacks")
        samples.append({"label":"battle03-first-player","state":s})
        var before:=s.duplicate(true)
        var path:=_route(s)
        if path.is_empty():
            issue="no-legal-player-movement"
        else:
            await key(_key(path[0]))
            s=await outcome_settle()
            await key(KEY_ESCAPE)
            s=await outcome_settle()
            _check(s.previewX==before.previewX and s.previewY==before.previewY and s.mainSeed==before.mainSeed and s.thinkingSeed==before.thinkingSeed,"Battle03 movement/cancel")
            samples.append({"label":"battle03-cancelled","state":s})
            for action in range(12):
                var old_actors: Array=s.actors.duplicate(true)
                await key(KEY_ENTER)
                s=await outcome_settle()
                await key(KEY_SPACE)
                await key(KEY_ENTER)
                s=await outcome_settle()
                samples.append({"label":"committed-player-action-"+str(action),"state":s})
                var changed: bool=s.actors.any(func(a): return str(a.id).begins_with("enemy-") and old_actors.any(func(old): return old.id==a.id and (old.x!=a.x or old.y!=a.y or old.hp!=a.hp)))
                if changed:
                    _check(s.stopReason=="PlayerInput" and s.stage=="Movement","enemy action through next usable player boundary")
                    break
                if action==11: issue="no-observed-enemy-action"
    if issue!="": failures.append(issue)
    events.append({"texts":yeel_texts.values(),"requestSamples":request_samples,"intro":intro_sample,"flash":flash_samples})
    await _finish()
