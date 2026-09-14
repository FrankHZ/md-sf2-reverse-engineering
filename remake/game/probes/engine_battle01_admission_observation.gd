extends "res://probes/engine_castle_tower_observation.gd"
var admission_records: Array = []
var white_seen := false
var mosaic_draws := 0
var shiver_draws := 0
var load_seen := false
var before_seen := false
var battle_ready: Dictionary = {}

func state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null: view = host.get_node("BattleSessionView")
    return JSON.parse_string(view.call("ReadObservationJson"))

func admission_settle(label: String) -> Dictionary:
    for count in range(16000):
        await process_frame
        frames += 1
        var s := state()
        if s.sessionId != castle_identity or s.failure != null:
            issue = label + ":session-failure"
            return s
        if s.has("stage"):
            battle_ready = s
            return s
        var p: Dictionary = s.presentation
        white_seen = white_seen or p.whiteOpacity > 0.95
        mosaic_draws = maxi(mosaic_draws, int(p.mosaicDraws))
        shiver_draws = maxi(shiver_draws, int(p.shiverDraws))
        if s.cursor != null and s.cursor.Program == "bbcs-01": before_seen = true
        if s.mode == "Battle" and s.wait == "PresentationWait":
            load_seen = true
            if s.flags.has(451.0):
                issue = "intro-flag-before-load-service"
                return s
            var mounted = host.get_node_or_null("ExplorationSessionView/BattleSessionView")
            if mounted != null:
                var projected: Dictionary = JSON.parse_string(mounted.call("ReadObservationJson"))
                if projected.stage != null or projected.boardChildren <= 0:
                    issue = "load-service-projection-or-early-input"
                    return s
        if s.wait == "DialogueWait" and not seen_texts.has(s.token):
            seen_texts[s.token] = true
            texts.append({"id":s.textId,"speaker":s.speaker,"program":s.cursor,"tick":s.simulationTick})
            await key(KEY_ENTER)
        elif s.wait == "ChoiceWait":
            await key(KEY_ENTER)
        elif s.stop == "PlayerInput" and not player(s).get("moving", true): return s
    issue = label + ":settle-timeout"
    return state()

func finish(s: Dictionary) -> void:
    if issue == "":
        castle_started = true
        s = await castle_route(s)
    if issue == "":
        var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://../../tests/fixtures/h3/map3-battle01-player-ready-v1.json"))
        admission_records.append({"label":"continuous-map21","session":s.sessionId,"party":s.partyLists,"seed":s.mainSeed,"flags":s.flags})
        for step in fixture.static.inputPlan:
            var p := player(s)
            if s.map != "map-" + str(int(step.from.map)) or p.x != step.from.x * 384 or p.y != step.from.y * 384:
                issue = "admission-route-position"
                break
            await key(input_code(step.input))
            s = await admission_settle(step.waypoint)
            admission_records.append({"label":step.waypoint,"map":s.get("map"),"player":player(s),"wait":s.get("wait"),"cursor":s.get("cursor"),"failure":s.failure})
            if issue != "": break
    if issue == "":
        if not before_seen or not white_seen or mosaic_draws <= 0 or shiver_draws <= 0 or not load_seen:
            issue = "before-battle-presentation-services"
        elif s.stage != "Movement" or s.round != 1 or not s.storyFlags.has(451.0) or s.boardChildren <= 0 or not s.previewInsideMap:
            issue = "first-player-projection"
        else:
            await key(KEY_ENTER)
            await process_frame
            s = state()
            if s.stage != "ActionChoice": issue = "actual-first-confirm"
            await key(KEY_ESCAPE)
            await process_frame
            s = state()
            if s.stage != "Movement" or s.mainSeed != battle_ready.mainSeed or s.sessionId != castle_identity:
                issue = "actual-first-cancel"
    var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT") + ".admission.json", FileAccess.WRITE)
    file.store_string(JSON.stringify({"issue":issue,"records":admission_records,"castle":castle_records,"whiteSeen":white_seen,"mosaicDraws":mosaic_draws,"shiverDraws":shiver_draws,"loadSeen":load_seen,"ready":battle_ready}, "  "))
    file.close()
    await super.finish(s)
