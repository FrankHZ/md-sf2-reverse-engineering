extends "res://probes/engine_shop_observation.gd"

var battle_view: Node
var seen: Dictionary={}
func board() -> Dictionary:return JSON.parse_string(battle_view.call("ReadObservationJson"))
func scene() -> Dictionary:return JSON.parse_string(battle_view.call("ReadSceneObservationJson"))
func actor(id: String) -> Dictionary:return board().actors.filter(func(a):return a.id==id)[0]
func settle_battle() -> Dictionary:
    for frame in range(2000):
        await process_frame
        var current:=scene();var s: Dictionary=current.scene
        if current.failure!=null or s.error!=null:check(false,"battle failure "+str(current));return {}
        var b:=board()
        if b.stage=="Movement" and b.stopReason=="PlayerInput" and current.movement==null:return b
        if s.waitToken!=null and not seen.has(s.waitToken):
            seen[s.waitToken]=true
            if s.phase in ["ActionMessage","ResultMessage"]:
                samples.append({"label":"supply-scene","kind":s.actionKind,"spell":s.spell,"item":s.item,"message":s.message,
                    "reaction":s.reactionKind,"actors":b.actors,"statuses":b.statuses})
                visible_text("supply-scene-text")
        if s.phase in ["ActionMessage","ResultMessage","RewardMessage","GrowthMessage","PoisonDamageMessage","PoisonExhaustedMessage"]:await key(KEY_ENTER)
    samples.append({"label":"battle-timeout","board":board(),"scene":scene()});check(false,"bounded battle settle");return {}
func stay() -> Dictionary:
    await key(KEY_ENTER);await key(KEY_SPACE);await key(KEY_ENTER);return await settle_battle()
func use_item(index: int) -> Dictionary:
    await key(KEY_ENTER);await key(KEY_I)
    for i in range(index):await key(KEY_DOWN)
    await key(KEY_ENTER);await key(KEY_ENTER);return await settle_battle()
func run() -> void:
    if not await start_host():finish();return
    var initial:=await settle_field()
    if initial.is_empty():finish();return
    await key(KEY_ENTER);await settle_field();check(field().wait=="ShopWait","ordinary merchant input")
    await buy(1,0);await buy(2,1)
    await key(KEY_ESCAPE);await settle_field();picture("purchases-carried")
    await key(KEY_DOWN);await settle_field()
    for i in range(3):await key(KEY_RIGHT);await settle_field()
    await key(KEY_UP)
    for frame in range(1500):
        await process_frame
        battle_view=host.find_child("BattleSessionView",true,false)
        if battle_view!=null and battle_view.is_visible_in_tree():break
        var s:=field()
        if s.wait in ["DialogueWait","FieldTextWait","W1TextWait"]:await key(KEY_ENTER)
    if battle_view==null or not battle_view.is_visible_in_tree():
        samples.append({"label":"field-handoff-timeout","field":field()});visible_text("handoff-diagnostic");check(false,"ordinary connected battle handoff");finish();return
    battle_view.connect("SessionResultObserved",result_observed)
    var b:=await settle_battle()
    if b.is_empty():finish();return
    check(b.sessionId==initial.sessionId,"purchase and usage stay in same session")
    check(b.actor=="medic-a","initial medic control")
    var hp=actor("medic-a").hp
    b=await use_item(0)
    if b.is_empty():finish();return
    check(actor("medic-a").hp==hp+20,"purchased Healing Seed restores20 HP")
    check(b.actor=="guard-a","next member control")
    b=await use_item(0)
    if b.is_empty():finish();return
    check(int(b.statuses.filter(func(a):return a.actor=="guard-a")[0].status)==0,"purchased Antidote cures poison")
    b=await stay()
    if b.is_empty():finish();return
    check(b.actor=="medic-a","next ordinary round")
    var mp=actor("medic-a").mp
    await key(KEY_ENTER);await key(KEY_H);await key(KEY_DOWN);await key(KEY_ENTER)
    # Target list is admitted living same-faction order. Scout is the third member.
    await key(KEY_DOWN);await key(KEY_DOWN);await key(KEY_ENTER)
    b=await settle_battle()
    if b.is_empty():finish();return
    check(actor("medic-a").mp==mp-3,"DETOX1 pays3 MP")
    check(int(b.statuses.filter(func(a):return a.actor=="scout-a")[0].status)==0,"DETOX1 cures second poisoned member")
    var x=b.previewX;await key(KEY_LEFT);await process_frame
    check(board().previewX==x-1,"further ordinary movement")
    samples.append({"label":"usable-supply-closure","board":board()});visible_text("further-input")
    finish()
