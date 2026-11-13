extends "res://probes/engine_pre_voyage_guidance_observation.gd"

# Ordinary Continue from the F609 church slot; navigation and assertions read the running host.
var session_id: String
var battle: Node
var delivered := {}

func _initialize() -> void:
    call_deferred("run")

func focus_native() -> void:
    pass # Headless keyboard input uses the production bindings.

func state() -> Dictionary:
    var view := find("ExplorationSessionView")
    return observation(view, "ReadObservationJson")

func snap(label: String) -> void:
    var s := state()
    samples.append({"step":label, "map":s.get("map"), "gold":s.get("gold"), "revision":s.get("revision")})

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(12)
    for code in [KEY_ENTER, KEY_E, KEY_DOWN, KEY_ENTER]: await key(code)
    if not await wait_slots("continue-slots"): finish(); return
    await key(KEY_DOWN); await key(KEY_ENTER)
    for frame in 1800:
        await process_frame
        if state().get("canWaitAtInput", false): break
    var s := state()
    if s.get("map") != "map-3" or not 609.0 in s.flags:
        check(false, "Continue slot 2 at Granseal F609")
        finish()
        return
    session_id = s.sessionId
    await walk(Vector2i(7,20), "weapon counter")
    if issue != "": check(false, issue); finish(); return
    await key(KEY_UP); await frames(12); await key(KEY_ENTER)
    for frame in 240:
        if state().get("wait") == "ShopWait": break
        await process_frame
    if state().get("wait") != "ShopWait":
        check(false, "merchant opens"); finish(); return
    for code in [KEY_ENTER, KEY_DOWN, KEY_ENTER, KEY_DOWN, KEY_DOWN, KEY_ENTER]: await key(code)
    var quote := state()
    if quote.shop.wait.Phase != 4 or quote.shop.wait.Offer.Item != 72:
        check(false, "Short Spear quote"); finish(); return
    await key(KEY_ENTER)
    var paid := state()
    check(paid.shop.wait.Result == 0 and paid.gold == quote.gold - quote.shop.wait.Offer.Price, "buyer paid quoted price")
    check(paid.party[2].SourceLoadout.Items[2] == 72, "Chester received unequipped Short Spear")
    snap("purchase")
    for code in [KEY_ENTER, KEY_ESCAPE]: await key(code)
    await settle("leave shop")
    for code in [KEY_ESCAPE, KEY_DOWN, KEY_DOWN, KEY_ENTER, KEY_RIGHT, KEY_RIGHT, KEY_ENTER,
            KEY_DOWN, KEY_DOWN, KEY_ENTER, KEY_DOWN, KEY_DOWN, KEY_ENTER, KEY_ENTER]: await key(code)
    var items: Array = state().party[2].SourceLoadout.Items
    samples.append({"step":"equip", "items":items, "beforeGold":quote.gold, "afterGold":paid.gold})
    check(items == [56.0,0.0,200.0,127.0], "field Equip selects the purchased slot")
    for i in 6:
        if not state().fieldMenu.visible and not state().partyStatus.visible: break
        await key(KEY_ESCAPE)
    if not failures.is_empty(): finish(); return
    await walk(Vector2i(2,27), "Jaha joins at west gate")
    await walk(Vector2i(1,27), "west gate")
    if issue != "": check(false, issue); finish(); return
    await key(KEY_LEFT)
    for frame in 1200:
        battle = find("BattleSessionView")
        if battle != null and battle.is_visible_in_tree() and board().get("stage") == "Movement": break
        await process_frame
    if battle == null or board().get("map") != "map66" or board().sessionId != session_id:
        check(false, "same session enters Battle02"); finish(); return
    var gear: Dictionary = board().equipmentActors.filter(func(a): return a.actor == "ally-2")[0]
    check(gear.equippedSlot == 2 and gear.minimumRange == 1 and gear.maximumRange == 2, "spear range on entry")
    samples.append({"step":"battle entry", "gear":gear})
    for turn in 24:
        var current := board()
        if current.actor in ["ally-0","ally-2"]: await position_actor(current)
        await key(KEY_ENTER)
        var actions: Array = board().battleChoices.Actions
        if actions.any(func(a): return a.Action.Kind == 2 and a.Enabled):
            await key(KEY_F)
            var aimed := board()
            if aimed.stage != "CommitReady":
                check(false, "ordinary attack target"); break
        else:
            # Fresh action focus follows the live attack offer.
            for i in 4:
                if int(board().menu.command) == 3: break
                await key(KEY_DOWN)
            await key(KEY_ENTER)
        var before := board()
        var target = before.target if before.actor == "ally-2" else null
        var target_hp := hp(before, target) if target != null else -1
        var chester_hp := hp(before, "ally-2")
        var previous_defence = delivered.get("defence")
        await key(KEY_ENTER)
        if not await next_input(): break
        var returned := board()
        if target != null and delivered.get("attack") == target and hp(returned, target) < target_hp:
            delivered["attackDamage"] = true
            samples.append({"step":"attack settled", "target":target, "before":target_hp, "after":hp(returned,target)})
        if delivered.get("defence") != previous_defence and hp(returned, "ally-2") < chester_hp:
            delivered["defenceDamage"] = true
            samples.append({"step":"hit settled", "before":chester_hp, "after":hp(returned,"ally-2")})
        if delivered.has("attackDamage") and delivered.has("defenceDamage"):
            samples.append({"step":"player control", "actor":returned.actor, "round":returned.round, "stage":returned.stage})
            await key(KEY_ENTER)
            check(board().stage == "ActionChoice", "next player can open action choice")
            await key(KEY_ESCAPE)
            check(board().stage == "Movement", "next player can resume movement")
            break
    check(delivered.has("attackDamage") and delivered.has("defenceDamage"), "both equipped attack and received hit settled")
    finish()

func board() -> Dictionary:
    return observation(battle, "ReadObservationJson")

func hp(s: Dictionary, actor: String) -> int:
    return int(s.actors.filter(func(a): return a.id == actor)[0].hp)

func position_actor(s: Dictionary) -> void:
    var origin := Vector2i(s.previewX, s.previewY)
    var paths := {origin:[]}
    var queue: Array[Vector2i] = [origin]
    var best := origin
    var keep_clear: bool = s.actor == "ally-0"
    var best_score := -1000
    var cells := {}
    var terrain := observation(battle.get_node("MapViewport"), "ReadTerrainJson")
    for p in terrain.movementRange.destinations: cells[Vector2i(p.X,p.Y)] = true
    var foes: Array = s.actors.filter(func(a): return str(a.id).begins_with("enemy-") and a.hp > 0)
    while not queue.is_empty():
        var at: Vector2i = queue.pop_front()
        var gap := 1000
        for foe in foes:
            gap = mini(gap, absi(at.x-int(foe.x)) + absi(at.y-int(foe.y)))
        var score := gap if keep_clear else -gap
        if score > best_score: best = at; best_score = score
        for d in [Vector2i.UP,Vector2i.LEFT,Vector2i.DOWN,Vector2i.RIGHT]:
            var n: Vector2i = at+d
            if cells.has(n) and not paths.has(n):
                paths[n] = paths[at]+[d]
                queue.append(n)
    for d in paths[best]:
        await key(KEY_UP if d == Vector2i.UP else KEY_LEFT if d == Vector2i.LEFT else KEY_DOWN if d == Vector2i.DOWN else KEY_RIGHT)
        for i in 100:
            if board().movement == null: break
            await process_frame

func next_input() -> bool:
    for frame in 2400:
        if not is_instance_valid(battle) or board().has("pendingFieldReturn"):
            check(false, "battle ended before next player control"); return false
        var s := observation(battle, "ReadSceneObservationJson")
        var c: Dictionary = s.scene
        if s.failure != null or c.error != null:
            check(false, "battle playback " + str([s.failure,c.error])); return false
        if c.visible and c.displayedAlly == "ally-2" and c.weaponVisible:
            var attack: bool = c.phase == "ActionAnimation" and c.actorCard.actor == "ally-2" and c.frameIndex >= 0
            var defence: bool = c.phase == "Reaction" and c.targetCard.actor == "ally-2" and c.reactionKind == "Damage"
            var role := "attack" if attack else "defence"
            if (attack or defence) and delivered.get(role + "Token") != c.waitToken:
                check(str(c.weaponResource).begins_with("weapon72/") and (not attack or c.animationIndex == 80), "actual spear frame " + role)
                samples.append({"step":role, "round":s.round, "actor":c.actorCard.actor, "target":c.targetCard.actor,
                    "phase":c.phase, "sequence":c.animationIndex, "frame":c.weaponResource, "reaction":c.reactionKind,
                    "amount":c.reactionAmount})
                delivered[role + "Token"] = c.waitToken
                delivered[role] = c.targetCard.actor if attack else c.waitToken
        if not c.visible and s.actor != null and s.movement == null and board().stage == "Movement": return true
        if c.visible and c.requiresAcknowledgement: await key(KEY_ENTER)
        else: await process_frame
    var stalled := board()
    samples.append({"step":"timeout","actor":stalled.get("actor"),"stage":stalled.get("stage"),"page":stalled.menu.page,"phase":stalled.scene.phase,"stop":stalled.stopReason})
    check(false, "battle input did not resume"); return false

func finish() -> void:
    if ended: return
    ended = true
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples}, "  "))
    file.close()
    host.queue_free()
    await frames(4)
    quit(0 if failures.is_empty() else 1)
