extends "res://probes/engine_map3_opening_observation.gd"
var castle_started := false
var castle_records: Array = []
var castle_identity := ""
var palette_min := 1.0
var priority_seen := false
var guard_sprite_wait: Dictionary = {}
var guard_release: Dictionary = {}
func guard(s: Dictionary) -> Dictionary:
    for entity in s.get("entities", []):
        if entity.id == "entity-128": return entity
    return {}
func remember(s: Dictionary, label: String) -> void:
    castle_records.append({"label":label,"map":s.get("map"),"player":player(s),"guard":guard(s),"flags":s.get("flags"),"cursor":s.get("cursor"),"wait":s.get("wait"),"failure":s.get("failure"),"presentation":s.get("presentation")})
func settle(label: String, yes: bool = true) -> Dictionary:
    for count in range(5000):
        await process_frame
        frames += 1
        var s := state()
        if s.map == "map-21" and s.wait == "EntitySpriteWait" and s.cursor.Program == "cs-53ef4":
            guard_sprite_wait = s
        if s.map == "map-21" and s.flags.has(401.0) and guard_release.is_empty():
            guard_release = s
        palette_min = minf(palette_min, s.presentation.paletteBrightness)
        for entity in s.get("entities", []):
            if entity.id == "entity-131" and entity.priority: priority_seen = true
        if s.sessionId != castle_identity or s.failure != null:
            issue = label + ":session-failure"
            remember(s, label)
            return s
        if s.wait == "DialogueWait":
            if not seen_texts.has(s.token):
                seen_texts[s.token] = true
                texts.append({"id":s.textId,"speaker":s.speaker,"flags":s.speakerFlags,"program":s.cursor,"tick":s.simulationTick})
                await key(KEY_ENTER)
        elif s.wait == "ChoiceWait":
            await key(KEY_ENTER if yes else KEY_N)
            choices += 1
        elif s.stop == "PlayerInput" and not player(s).get("moving", true):
            return s
    issue = label + ":settle-timeout"
    var stopped := state()
    remember(stopped, label)
    return stopped
func input_code(name: String) -> int:
    return {"Left":KEY_LEFT,"Right":KEY_RIGHT,"Up":KEY_UP,"Down":KEY_DOWN,"C":KEY_C}[name]
func castle_route(s: Dictionary) -> Dictionary:
    castle_identity = s.sessionId
    remember(s, "live-opening-completed")
    if decline:
        await key(KEY_RIGHT)
        s = await settle("rejoined-opening-return")
        if issue != "": return s
    var graph: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"))
    for segment in graph.static.routeGraph.segments:
        if segment.kind == "navigation":
            for step in range(segment.inputs.size()):
                var p := player(s)
                if s.map != "map-" + str(int(segment.map)) or p.x != segment.points[step][0] * 384 or p.y != segment.points[step][1] * 384:
                    issue = segment.id + ":position-" + str(step)
                    remember(s, issue)
                    return s
                await key(input_code(segment.inputs[step]))
                s = await settle(segment.id)
                if issue != "": return s
        elif segment.kind == "entity-interaction":
            await key(KEY_UP)
            s = await settle(segment.id)
            if issue != "": return s
            await key(KEY_C)
            s = await settle(segment.id, false)
            if issue != "": return s
            remember(s, "astral-declined")
            if not s.flags.has(607.0) or s.flags.has(608.0):
                issue = "astral-decline-flags"
                return s
            await key(KEY_C)
            s = await settle("astral-reprompt-accept")
            if issue != "": return s
        elif segment.kind == "entity-terminal":
            for input in [KEY_RIGHT, KEY_RIGHT, KEY_C]:
                await key(input)
                s = await settle(segment.id)
                if issue != "": return s
            remember(s, "guard-released")
            if not s.flags.has(401.0) or not s.flags.has(256.0) or guard(s).x != 6 * 384 or player(s).facing != 3:
                issue = "guard-release-state"
                return s
            if guard_sprite_wait.is_empty() or guard_sprite_wait.flags.has(401.0) or guard_sprite_wait.flags.has(256.0) or player(guard_sprite_wait).facing != 3:
                issue = "guard-sprite-service-order"
                return s
            if player(s).spriteReady != player(s).spriteRequest or player(s).waitingForSprite:
                issue = "guard-sprite-service-incomplete"
                return s
            var before_texts := texts.size()
            for input in [KEY_RIGHT, KEY_RIGHT, KEY_C]:
                await key(input)
                s = await settle("guard-repeat-interaction")
                if issue != "": return s
            remember(s, "guard-repeat-interaction")
            if texts.size() != before_texts + 1 or texts[-1].id != 579 or guard(s).x != 6 * 384:
                issue = "guard-repeat-state"
                return s
            for input in [KEY_UP, KEY_DOWN, KEY_UP]:
                await key(input)
                s = await settle("released-path-field-input")
                if issue != "": return s
            if player(s).x != 5 * 384 or player(s).y != 15 * 384:
                issue = "released-path-position"
                return s
            for frame in range(120): await process_frame
            s = state()
            if s.stop != "PlayerInput" or s.wait != null or s.failure != null:
                issue = "released-path-stable-input"
                return s
        if segment.id == "map20-palace-init-and-return":
            if palette_min != 0 or not priority_seen or s.presentation.paletteBrightness != 1:
                issue = "palace-presentation"
                return s
        if segment.id == "map20-to-map19-royal-return":
            for input in [KEY_LEFT, KEY_RIGHT]:
                await key(input)
                s = await settle("palace-repeat-entry")
                if issue != "": return s
            remember(s, "palace-repeat-entry")
            if s.map != "map-20" or player(s).y != 37 * 384 or s.presentation.paletteFades != 1:
                issue = "palace-repeat-state"
                return s
            for input in [KEY_DOWN, KEY_UP]:
                await key(input)
                s = await settle("palace-repeat-return")
                if issue != "": return s
        remember(s, segment.id)
    return s
func finish(s: Dictionary) -> void:
    if issue == "" and not castle_started:
        castle_started = true
        s = await castle_route(s)
    var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT") + ".castle.json", FileAccess.WRITE)
    file.store_string(JSON.stringify({"issue":issue,"records":castle_records,"paletteMinimum":palette_min,"prioritySeen":priority_seen,"guardSpriteWait":guard_sprite_wait,"guardRelease":guard_release}, "  "))
    file.close()
    await super.finish(s)
