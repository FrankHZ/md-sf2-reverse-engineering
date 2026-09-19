extends "res://probes/engine_battle01_admission_observation.gd"

var outcome_records: Array = []
var after_programs: Dictionary = {}
var held_battle := false
var after_mosaic := 0
var after_white := false
var after_sounds := 0

# External observation driver: choose normal keyboard commands from the live board.
# The game never reads this policy or a prescribed opponent/kill sequence.
func distance(a: Dictionary, b: Dictionary) -> int:
    return absi(int(a.x - b.x)) + absi(int(a.y - b.y))

func flood(s: Dictionary, actor: Dictionary, origin: Dictionary, budget: int, occupied: bool) -> Dictionary:
    var start: int = origin.y * 48 + origin.x
    var costs: Dictionary = {start: 0}
    var previous: Dictionary = {}
    var pending: Array = [start]
    var blocked: Dictionary = {}
    if occupied:
        for other in s.actors:
            if other.hp > 0 and other.id.begins_with("enemy-"): blocked[int(other.y * 48 + other.x)] = true
    while not pending.is_empty():
        var index := 0
        for i in range(pending.size()):
            if costs[pending[i]] < costs[pending[index]]: index = i
        var cell: int = pending.pop_at(index)
        for delta in [1, -1, -48, 48]:
            var next: int = cell + delta
            if next < 0 or next >= 2304 or next % 48 >= s.mapWidth or next / 48 >= s.mapHeight: continue
            if absi(delta) == 1 and cell / 48 != next / 48: continue
            if blocked.has(next): continue
            var surface: String = s.terrain[next]
            var cost := 2
            if surface == "Barrier" or surface == "Impassable": continue
            if surface == "Brush": cost = 3
            if surface == "Rough": cost = 5 if actor.mover == "Centaur" else 3
            if surface == "Deep": cost = 5 if actor.mover == "Centaur" else 4
            var total: int = costs[cell] + cost
            if total <= budget and total < costs.get(next, 10000):
                costs[next] = total
                previous[next] = cell
                if not pending.has(next): pending.append(next)
    return {"costs":costs,"previous":previous,"start":start}

func threat(p: Dictionary, enemies: Array, actor: Dictionary, attacked: Dictionary = {}) -> int:
    var result := 0
    for foe in enemies:
        if foe == attacked and foe.hp <= maxi(1, int((actor.attack - foe.defense) * 3 / 4)): continue
        if distance(p, foe) <= foe.move + 1: result += 1
    return result

func choose_command(s: Dictionary, lose: bool) -> Dictionary:
    var actor: Dictionary = {}
    var enemies: Array = []
    var allies: Array = []
    var support: Dictionary = {}
    for row in s.actors:
        if row.id == s.actor: actor = row
        if row.hp <= 0: continue
        if row.id.begins_with("enemy-"): enemies.append(row)
        else:
            allies.append(row)
            for learned in row.learned:
                if learned.Value == "heal": support = row
    var grid := flood(s, actor, actor, int(actor.move * 2), true)
    var cells: Array = grid.costs.keys()
    cells.sort()
    var positions: Array = []
    for cell in cells:
        var p := {"x":cell % 48,"y":int(cell / 48),"cell":cell}
        var free := true
        for row in allies:
            if row.id != actor.id and distance(p, row) == 0: free = false
        if free: positions.append(p)
    var destination := {"x":actor.x,"y":actor.y,"cell":int(actor.y * 48 + actor.x)}
    var target: Variant = null
    var action := "stay"
    var score := 1000000
    if not lose and actor == support and actor.mp >= 3:
        for ally in allies:
            if ally.maxHp - ally.hp < 6: continue
            for p in positions:
                if ally != actor and distance(p, ally) > 1: continue
                var candidate: int = ally.hp * 1000 + grid.costs[p.cell]
                if candidate < score:
                    score = candidate
                    destination = p
                    target = ally.id
                    action = "heal"
    if not lose and target == null:
        for foe in enemies:
            for p in positions:
                if distance(p, foe) != 1: continue
                var candidate: int = foe.hp * 10000 + threat(p, enemies, actor, foe) * 100 + grid.costs[p.cell]
                if candidate < score:
                    score = candidate
                    destination = p
                    target = foe.id
                    action = "attack"
    if target == null and (not lose or actor.leader):
        score = 1000000
        var goals: Array = []
        for foe in enemies: goals.append(flood(s, actor, foe, 255, false).costs)
        for p in positions:
            if not lose and grid.costs[p.cell] > 4: continue
            var nearest := 10000
            for goal in goals: nearest = mini(nearest, goal.get(p.cell, 10000))
            var candidate: int = nearest * 100 + grid.costs[p.cell]
            if candidate < score:
                score = candidate
                destination = p
        if not lose and not support.is_empty():
            if actor != support and distance(actor, support) > 3:
                var to_support: Dictionary = flood(s, actor, support, 255, false).costs
                score = 1000000
                for p in positions:
                    var candidate: int = to_support.get(p.cell, 10000) * 100 + grid.costs[p.cell]
                    if candidate < score:
                        score = candidate
                        destination = p
            elif actor == support:
                for ally in allies:
                    if distance(actor, ally) > 3: destination = {"x":actor.x,"y":actor.y,"cell":grid.start}
    var attacked: Dictionary = {}
    for foe in enemies:
        if foe.id == target: attacked = foe
    if not lose and actor.leader and actor.hp <= 6 and threat(destination, enemies, actor, attacked) * 3 >= actor.hp:
        score = 1000000
        for p in positions:
            var nearest := 1000
            for foe in enemies: nearest = mini(nearest, distance(p, foe))
            var candidate: int = -nearest * 100 + grid.costs[p.cell]
            if candidate < score:
                score = candidate
                destination = p
        target = null
        action = "stay"
    var path: Array = [destination]
    var cell: int = destination.cell
    while cell != grid.start:
        cell = grid.previous[cell]
        path.push_front({"x":cell % 48,"y":int(cell / 48)})
    return {"actor":actor.id,"round":s.round,"seed":s.mainSeed,"path":path,"target":target,"action":action}

func after_admission(s: Dictionary) -> Dictionary:
    var lose := OS.get_environment("SF2_BATTLE_OUTCOME_CASE") == "defeat"
    var initial_gold: int = s.gold
    for action in range(200):
        if not s.has("stage"): break
        if not valid_projection(s, true):
            issue = "battle-viewport"
            break
        var step := choose_command(s, lose)
        for i in range(1, step.path.size()):
            var p: Dictionary = step.path[i]
            var old: Dictionary = step.path[i - 1]
            await key(KEY_RIGHT if p.x > old.x else KEY_LEFT if p.x < old.x else KEY_DOWN if p.y > old.y else KEY_UP)
        await key(KEY_ENTER)
        await key(KEY_H if step.action == "heal" else KEY_F if step.action == "attack" else KEY_SPACE)
        await process_frame
        s = state()
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
        outcome_records.append({"actor":step.actor,"round":step.round,"action":step.action,"target":step.target,"map":s.get("map"),"failure":s.failure})
        if issue != "": break
    if issue == "":
        if s.has("stage") or s.stop != "PlayerInput" or s.wait != null or s.battleMounted or not held_battle:
            issue = "outcome-return-control"
        elif s.flags.has(501.0) == lose or s.flags.has(401.0) != lose:
            issue = "outcome-flags-or-gold"
        elif lose and (s.map != "map-3" or after_sounds < 1 or after_programs.has("abcs-battle01")):
            issue = "ordinary-defeat-path"
        elif not lose and (s.map != "map-57" or after_mosaic <= 0 or not after_white or not after_programs.has("abcs-battle01")):
            issue = "victory-after-program"
        else:
            var viewport := Rect2(s.viewport.x, s.viewport.y, s.viewport.width, s.viewport.height)
            var board := Rect2(s.mapViewport.x, s.mapViewport.y, s.mapViewport.width, s.mapViewport.height)
            if s.viewport.width != 960 or s.viewport.height != 640 or not board.has_area() or not viewport.encloses(board):
                issue = "return-viewport"
            var before := player(s)
            await key(KEY_LEFT)
            s = await outcome_settle()
            if player(s).x == before.x and player(s).y == before.y: issue = "actual-return-movement"
    var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT") + ".outcome.json", FileAccess.WRITE)
    file.store_string(JSON.stringify({"issue":issue,"lose":lose,"initialGold":initial_gold,"records":outcome_records,"programs":after_programs.keys(),
        "heldBattle":held_battle,"mosaicOutDraws":after_mosaic,"whiteSeen":after_white,"soundStarts":after_sounds,"final":s}, "  "))
    file.close()
    return s

func outcome_settle() -> Dictionary:
    for frame in range(20000):
        await process_frame
        frames += 1
        var s := state()
        if s.sessionId != castle_identity or s.failure != null:
            issue = "outcome-session-or-presentation-failure"
            return s
        if s.has("stage"):
            if s.stage != null: return s
            continue
        held_battle = held_battle or s.battleMounted
        after_mosaic = maxi(after_mosaic, int(s.presentation.mosaicOutDraws))
        after_sounds = maxi(after_sounds, int(s.presentation.soundStarts))
        after_white = after_white or s.presentation.whiteOpacity > 0.95
        if s.cursor != null: after_programs[s.cursor.Program] = true
        if s.wait == "DialogueWait":
            if not seen_texts.has(s.token):
                seen_texts[s.token] = true
                texts.append({"id":s.textId,"speaker":s.speaker,"program":s.cursor,"tick":s.simulationTick})
                await key(KEY_ENTER)
        elif s.wait == "ChoiceWait": await key(KEY_ENTER)
        elif s.stop == "PlayerInput" and not player(s).get("moving", true): return s
    issue = "outcome-settle-timeout"
    return state()
