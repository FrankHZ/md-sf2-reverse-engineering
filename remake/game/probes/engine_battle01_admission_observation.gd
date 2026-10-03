extends "res://probes/engine_castle_tower_observation.gd"
var capture: Node
var capture_full_state := false
var capture_failed := false
const RETAINED_CONTROL_ROWS := 256

func capture_row(channel: String, row: Dictionary, legacy: Array) -> void:
    if capture == null:
        legacy.append(row)
        return
    if capture_failed or not capture.call("IsAccepting"): return
    if channel == "resourceUses":
        row = row.duplicate()
        row.erase("identity")
        get("current_draw_uses").append(row)
        return
    if not capture.call("Record",channel,row):
        capture_failed = true
        issue = "capture-failed:" + str(capture.call("Failure"))
        return
    # Only the existing route's small control assertions need retained rows. Evidence
    # lives in the stream. No camera/audio/draw history stays in probe arrays.
    if channel == "samples" and (row.label.begins_with("opening-ready-") or row.label == "portrait-route-initial"):
        legacy.append({"label":row.label,"state":{"textId":row.state.get("textId")}})
    elif channel == "warpRecords" and row.result.observations.any(func(o): return o.Kind in ["choice-returned","nod-returned","zone-entered"]):
        legacy.append({"result":{"observations":row.result.observations}})
    elif channel == "inputRecords":
        legacy.clear()
        legacy.append({"delivery":row.delivery})
    elif channel == "rawTextDraws" and row.visible and not row.text.is_empty() and (row.characters < 0 or row.characters >= row.total):
        legacy.clear()
        legacy.append(row)
    elif channel == "waitReceipts":
        legacy.append(row)
    elif channel == "choiceDraws":
        var phase: int = row.choice.Work.Phase
        if not legacy.any(func(previous):return previous.choice.Work.Phase == phase): legacy.append(row)
    if legacy.size() > RETAINED_CONTROL_ROWS or not capture.call("ValidateControl",legacy,"retained-control:" + channel):
        capture.call("Cancel","active-control-row-limit")
        capture_failed = true
        issue = "capture-active-control-row-limit"

func capture_count(channel: String, legacy: Array) -> int:
    return int(capture.call("Count",channel)) if capture != null else legacy.size()

const OBSERVATION_SIZE := Vector2i(960, 640)
var admission_records: Array = []
var white_seen := false
var mosaic_draws := 0
var shiver_draws := 0
var load_seen := false
var before_seen := false
var battle_loaded: Dictionary = {}
var battle_ready: Dictionary = {}

func run() -> void:
    # Headless --resolution can leave the actual root viewport at 64x64.
    root.size = OBSERVATION_SIZE
    await process_frame
    await super.run()

func valid_projection(s: Dictionary, with_preview: bool) -> bool:
    var viewport: Dictionary = s.viewport
    var map: Dictionary = s.mapViewport
    if viewport.width != OBSERVATION_SIZE.x or viewport.height != OBSERVATION_SIZE.y:
        return false
    var screen := Rect2(viewport.x, viewport.y, viewport.width, viewport.height)
    var board := Rect2(map.x, map.y, map.width, map.height)
    if not board.has_area() or not screen.encloses(board) or s.zoom < 1.0:
        return false
    if with_preview:
        if s.previewRect == null or not s.previewInsideMap: return false
        var preview: Dictionary = s.previewRect
        var cursor := Rect2(preview.x, preview.y, preview.width, preview.height)
        if not cursor.has_area() or not board.encloses(cursor): return false
    return true

func state() -> Dictionary:
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null: view = host.get_node("BattleSessionView")
    if capture != null:
        if view.name == "ExplorationSessionView": return view.call("ReadCaptureState",capture_full_state,bool(get("reading_resource_draw")))
        return view.call("ReadCaptureState",capture_full_state)
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
            capture_full_state = true
            s = state()
            capture_full_state = false
            battle_ready = s
            return s
        var p: Dictionary = s.presentation
        white_seen = white_seen or p.whiteOpacity > 0.95
        mosaic_draws = maxi(mosaic_draws, int(p.mosaicDraws))
        shiver_draws = maxi(shiver_draws, int(p.shiverDraws))
        if s.cursor != null and s.cursor.Program == "bbcs-01": before_seen = true
        if s.mode == "Battle" and s.wait == "PresentationWait":
            load_seen = true
            if numeric_contains(s.flags, 451):
                issue = "intro-flag-before-load-service"
                return s
            var mounted = host.get_node_or_null("ExplorationSessionView/BattleSessionView")
            if mounted != null:
                var projected: Dictionary = JSON.parse_string(mounted.call("ReadObservationJson"))
                battle_loaded = projected
                if projected.stage != null or projected.boardChildren <= 0 or not valid_projection(projected, false):
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
        capture_row("admissionRecords",{"label":"continuous-map21","session":s.sessionId,"party":s.partyLists,"seed":s.mainSeed,"flags":s.flags}, admission_records)
        for step in fixture.static.inputPlan:
            var p := player(s)
            if s.map != "map-" + str(int(step.from.map)) or p.x != step.from.x * 384 or p.y != step.from.y * 384:
                issue = "admission-route-position"
                break
            await key(input_code(step.input))
            s = await admission_settle(step.waypoint)
            capture_row("admissionRecords",{"label":step.waypoint,"map":s.get("map"),"player":player(s),"wait":s.get("wait"),"cursor":s.get("cursor"),"failure":s.failure}, admission_records)
            if issue != "": break
    if issue == "":
        if not before_seen or not white_seen or mosaic_draws <= 0 or shiver_draws <= 0 or not load_seen:
            issue = "before-battle-presentation-services"
        elif battle_loaded.is_empty() or s.stage != "Movement" or s.round != 1 or not numeric_contains(s.storyFlags, 451) or s.boardChildren <= 0 or not valid_projection(s, true):
            issue = "first-player-projection"
        else:
            await key(KEY_ENTER)
            await process_frame
            s = state()
            if s.stage != "ActionChoice": issue = "actual-first-confirm"
            await key(KEY_ESCAPE)
            await process_frame
            s = state()
            if s.stage != "Movement" or s.mainSeed != battle_ready.mainSeed or s.sessionId != castle_identity or not valid_projection(s, true):
                issue = "actual-first-cancel"
    if issue == "": s = await after_admission(s)
    if capture == null:
        var file = FileAccess.open(OS.get_environment("SF2_EXPLORATION_OBSERVATION_OUTPUT") + ".admission.json", FileAccess.WRITE)
        file.store_string(JSON.stringify({"issue":issue,"expectedViewport":{"width":OBSERVATION_SIZE.x,"height":OBSERVATION_SIZE.y},"records":admission_records,"castle":castle_records,"whiteSeen":white_seen,"mosaicDraws":mosaic_draws,"shiverDraws":shiver_draws,"loadSeen":load_seen,"loaded":battle_loaded,"ready":battle_ready}, "  "))
        file.close()
    else:
        capture_row("admissionSummary",{"issue":issue,"expectedViewport":{"width":OBSERVATION_SIZE.x,"height":OBSERVATION_SIZE.y},
            "whiteSeen":white_seen,"mosaicDraws":mosaic_draws,"shiverDraws":shiver_draws,"loadSeen":load_seen,"loaded":battle_loaded,"ready":battle_ready}, [])
    await super.finish(s)

func after_admission(s: Dictionary) -> Dictionary:
    return s
