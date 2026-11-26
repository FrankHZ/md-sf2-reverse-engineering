extends SceneTree

# Bounded actual adapter observation; no screenshots, save writes or injected clocks.
var host: Node
var failures: Array = []
var proofs: Dictionary = {}
var samples: Array = []
var done := false
var began := Time.get_ticks_msec()
var camera_samples: Array = []
var scene_order: Array = []
var full_sequence := OS.get_environment("SF2_OPENING_HD_FULL") == "1"
var pixel_banks: Array = []
var pixel_proofs := 0

func _initialize() -> void:
    create_timer(900 if full_sequence else 300).timeout.connect(func(): finish("Opening HD observation deadline"))
    call_deferred("run")

func run() -> void:
    root.size = Vector2i(1600,900) if OS.get_environment("SF2_OPENING_HD_SIZE") == "small" else Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    for i in range(15): await process_frame
    var view := host.find_child("ExplorationSessionView",true,false)
    if view == null:
        var labels := []
        for label in host.find_children("*","Label",true,false): labels.append(label.text)
        samples.append({"admission":labels})
        finish("Controlled opening view missing")
        return
    view.connect("ResourceDrawObserved",func(): pass)
    var mode := OS.get_environment("SF2_OPENING_HD_MODE")
    var expected_path := OS.get_environment("SF2_OPENING_HD_PIXELS")
    if expected_path != "": pixel_banks = JSON.parse_string(FileAccess.get_file_as_string(expected_path))
    var expected_hd := mode != "original"
    var initial: Dictionary = view.call("ReadCaptureState",false,false)
    var party := JSON.stringify(initial.party)
    var gold: Variant = initial.gold
    while not done:
        var state: Dictionary = view.call("ReadCaptureState",false,false)
        if state.get("cursor") != null:
            var program: String = state.cursor.get("Program","")
            if program.begins_with("introcutscene") and not scene_order.has(program): scene_order.append(program)
        if state.get("failure") != null:
            samples.append(state)
            finish("Scene failure")
            return
        if JSON.stringify(state.party) != party or state.gold != gold:
            finish("Persistent party changed")
            return
        var draw: Variant = state.get("cameraProjection")
        if draw != null:
            var logical: Variant = draw.get("logicalView")
            if logical != null:
                camera_samples.append({"tick":draw.simulationTick,"map":draw.map,"logical":logical,"quake":draw.quake,
                    "background":plane_origin(draw.get("background")),
                    "foreground":plane_origin(draw.get("foreground")),
                    "targetSlot":draw.get("targetSlot"),"scale":draw.scale,
                    "width":draw.width,"height":draw.height})
                for pair in [["background","BX","BY"],["foreground","AX","AY"]]:
                    var plane: Variant = draw.get(pair[0])
                    if plane == null: continue
                    var expected_x: float = logical[pair[1]].Position/16.0+120.0-draw.width/(2.0*draw.scale)
                    var expected_y: float = logical[pair[2]].Position/16.0+132.0-draw.height/(2.0*draw.scale)+draw.quake
                    if abs(plane.x-expected_x)>0.001 or abs(plane.y-expected_y)>0.001:
                        finish("Camera draw diverged from logical plane during follow switch")
                        return
                for actor in draw.get("actors",[]):
                    var ax: String = "BX" if logical.Area.Layer == 0 else "AX"
                    var ay: String = "BY" if logical.Area.Layer == 0 else "AY"
                    var expected_x: float = logical[ax].Position/16.0+120.0-draw.width/(2.0*draw.scale)
                    var expected_y: float = logical[ay].Position/16.0+132.0-draw.height/(2.0*draw.scale)+draw.quake
                    if abs(actor.originX-expected_x)>0.001 or abs(actor.originY-expected_y)>0.001:
                        finish("Actor and terrain camera origins diverged")
                        return
            if draw.get("exteriorColor") != "000000ff":
                finish("Exterior is not opaque black")
                return
            var bank: Dictionary = draw.get("cue",{}).get("terrainPalette",{})
            var palette: Variant = state.get("scenePaletteWait")
            var pulse: bool = palette != null and palette.operation == "PulseScenePalette"
            var restoring: bool = pulse and palette.elapsed >= 10
            var alternate := bool(bank.get("hdBank",false))
            if alternate and not pixel_banks.is_empty():
                var expected: Array = []
                for row in pixel_banks:
                    if same_words(row.colors, bank.colors): expected = row.hashes
                for actual in bank.get("hdSamples",[]):
                    if not expected.has(actual.rgbaSha256):
                        samples.append({"pixelMismatch":bank})
                        finish("Actual uploaded HD pixels differ from published palette bank")
                        return
                    pixel_proofs += 1
            for layer in [draw.get("background"),draw.get("foreground")]:
                if layer == null or layer.get("first") == null: continue
                var pixels: int = layer.first.get("selector",{}).get("pixels",0)
                var wanted := 96 if expected_hd else 24
                if mode == "missing" and pulse and not restoring: wanted = 24
                if pixels != wanted:
                    samples.append({"state":state,"wanted":wanted,"actual":pixels})
                    finish("Unexpected mounted terrain dimensions")
                    return
                var key := str(state.map)+("-pulse" if pulse and not restoring else "-restore" if restoring else "-base")
                if not proofs.has(key):
                    proofs[key] = {"pixels":pixels,"bank":bank,"palette":palette}
                    samples.append({"key":key,"draw":draw})
                if pulse and not restoring and mode == "hd" and not alternate:
                    finish("Storm did not mount its HD palette bank")
                    return
                if restoring and state.display.Scene.Terrain != palette.previousTerrain:
                    finish("Storm failed to restore previous colors")
                    return
            for actor in draw.get("actors",[]):
                if actor.visible and actor.sprite in [183,187]:
                    if actor.resourceSelector.get("hd",false) != expected_hd:
                        finish("Effect did not select requested graphics mode")
                        return
                    proofs["effect-"+str(actor.sprite)] = actor.resourceSelector
        if state.wait == "SceneEndWait":
            if full_sequence and scene_order != ["introcutscene1","introcutscene2","introcutscene3","introcutscene4"]:
                failures.append("Four source scenes were not observed")
            if state.display.Scene != null: failures.append("Palette retained at terminal")
            if not proofs.has("effect-183") or not proofs.has("effect-187"):
                failures.append("Both effects must actually draw")
            if not proofs.has("map-65-pulse") or not proofs.has("map-65-restore"):
                failures.append("Missing pulse/restoration draw")
            if not pixel_banks.is_empty() and expected_hd and mode != "missing" and pixel_proofs == 0:
                failures.append("No uploaded palette pixels were compared")
            finish("")
            return
        await process_frame

func plane_origin(plane: Variant) -> Variant:
    return null if plane == null else {"x":plane.x,"y":plane.y}

func same_words(left: Array, right: Array) -> bool:
    if left.size() != right.size(): return false
    for i in range(left.size()):
        if int(left[i]) != int(right[i]): return false
    return true

func finish(reason: String) -> void:
    if done: return
    done = true
    if reason != "": failures.append(reason)
    var output := FileAccess.open(OS.get_environment("SF2_OPENING_OUTPUT"),FileAccess.WRITE)
    if output != null:
        output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
            "elapsedMs":Time.get_ticks_msec()-began,"proofs":proofs,"samples":samples,
            "camera":camera_samples,"sceneOrder":scene_order,"pixelProofs":pixel_proofs},"  "))
        output.close()
    if is_instance_valid(host):
        host.queue_free()
        await process_frame
        await process_frame
    quit(0 if failures.is_empty() else 1)
