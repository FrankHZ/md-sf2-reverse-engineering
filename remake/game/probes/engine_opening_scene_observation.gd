extends SceneTree

# Ordinary controlled host, normal clock, no injected completions or screenshots.
var host: Node
var view: Node
var samples: Array = []
var failures: Array = []
var texts: Dictionary = {}
var palettes: Dictionary = {}
var began := Time.get_ticks_msec()
var ended := false
var initial_party := ""
var initial_gold: Variant
var pulses := 0
var recovered := false
var palette_proofs: Dictionary = {}
var effects: Dictionary = {}
var exterior_proofs := 0
var full_sequence := OS.get_environment("SF2_OPENING_SCENE") == "all"
var scene_order: Array = []
var hd_actors: Dictionary = {}
var hd_terrain := 0
var focus_pauses := 0

func _initialize() -> void:
    create_timer(900 if full_sequence else 300).timeout.connect(func(): finish("Bounded scene deadline"))
    call_deferred("run")

func compact(s: Dictionary) -> Dictionary:
    var result := {}
    for key in ["revision","simulationTick","map","wait","token","cursor","failure","failureField","stop","textId","displayLocale","displayFallback","fieldText","visibleCharacters","totalCharacters","logicalView","presentationCue","scenePaletteWait","entities","cameraProjection","portraitResourceProjection","display","audio","textWindow","portraitWindow"]:
        if s.has(key): result[key] = s[key]
    return result

func sample(label: String, s: Dictionary) -> void:
    if is_instance_valid(view):
        var delivery: Dictionary = view.call("ReadCaptureState",true,false)
        s["audio"] = delivery.get("audio")
    samples.append({"label":label,"state":compact(s)})
    print(label + " tick=" + str(s.get("simulationTick")))

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    for i in range(10): await process_frame
    if full_sequence:
        var load_deadline := Time.get_ticks_msec()+30000
        while Time.get_ticks_msec() < load_deadline:
            view = host.find_child("OpeningScene",true,false)
            if view != null: break
            var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
            if not opening.active: break
            await process_frame
    else:
        view = host.find_child("ExplorationSessionView",true,false)
    if view == null:
        var labels := []
        for label in host.find_children("*","Label",true,false):
            if label.is_visible_in_tree(): labels.append(label.text)
        samples.append({"label":"admission-failure","labels":labels})
        var entry = host.find_child("SessionEntryView",true,false)
        if entry != null: samples.append({"entry":JSON.parse_string(entry.call("ReadEntryObservationJson"))})
        if full_sequence: samples.append({"opening":JSON.parse_string(host.call("ReadOpeningObservationJson"))})
        finish("Exploration host unavailable")
        return
    view.connect("ResourceDrawObserved",func(): pass)
    var first: Dictionary = view.call("ReadCaptureState",false,false)
    var settings: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
    samples.append({"label":"settings-and-catalog","state":settings})
    if settings.localization.worldCatalog.messages != 81 or settings.localization.worldCatalog.error != null:
        finish("Full opening catalog was not admitted")
        return
    initial_party = JSON.stringify(first.party)
    initial_gold = first.gold
    sample("initial",first)
    while not ended:
        if full_sequence and not is_instance_valid(view):
            var opening: Dictionary = JSON.parse_string(host.call("ReadOpeningObservationJson"))
            var entry: Node = host.find_child("SessionEntryView",true,false)
            samples.append({"label":"title-handoff","opening":opening,
                "entry":JSON.parse_string(entry.call("ReadEntryObservationJson")) if entry != null else null})
            if opening.result != "completed" or opening.error != null or entry == null:
                failures.append("Natural title handoff failed")
            if texts.size() != 81: failures.append("Automatic message coverage: " + str(texts.size()))
            if scene_order != ["introcutscene1","introcutscene2","introcutscene3","introcutscene4"]:
                failures.append("Four scene order: " + str(scene_order))
            if failures.is_empty() and OS.get_environment("SF2_OPENING_NEW_GAME") == "1":
                await new_game()
            finish("")
            return
        if not root.has_focus():
            focus_pauses += 1
            if focus_pauses <= 3:
                recovered = true
                root.grab_focus()
            var until := Time.get_ticks_msec()+30000
            while not root.has_focus() and Time.get_ticks_msec() < until: await process_frame
            if not root.has_focus():
                finish("Native focus unavailable after bounded pause")
                return
        var s: Dictionary = view.call("ReadCaptureState",false,false)
        if s.get("cursor") != null:
            var program: String = str(s.cursor.get("Program",""))
            if program.begins_with("introcutscene") and not scene_order.has(program):
                scene_order.append(program)
                sample("scene-"+program,s)
        if s.get("failure") != null:
            sample("failure",s)
            var labels := []
            for label in host.find_children("*","Label",true,false):
                if label.is_visible_in_tree(): labels.append(label.text)
            samples.append({"label":"failure-labels","labels":labels})
            finish("Native scene failure: " + str(s.failure))
            return
        if JSON.stringify(s.party) != initial_party or s.gold != initial_gold:
            finish("Scene changed persistent party")
            return
        if s.get("textId") != null and s.get("fieldText") != null:
            var text_key := str(s.textId)
            if not texts.has(text_key) and (s.visibleCharacters < 0 or s.visibleCharacters >= s.totalCharacters):
                texts[text_key] = true
                sample("automatic-text-"+text_key,s)
                var locale := OS.get_environment("SF2_OPENING_LOCALE")
                if locale != "" and s.displayLocale != locale:
                    finish("Unexpected display locale for " + text_key)
                    return
        var palette: Variant = s.get("scenePaletteWait")
        var draw: Variant = s.get("cameraProjection")
        if draw != null:
            if draw.get("exteriorColor") != "000000ff":
                finish("Scene exterior draw is not opaque black")
                return
            exterior_proofs += 1
            for actor in draw.get("actors",[]):
                if actor.visible and actor.resourceSelector.get("hd",false):
                    hd_actors[str(actor.sprite)] = actor.resourceSelector.pixels
                if actor.visible and actor.sprite in [183,187] and not effects.has(str(actor.sprite)):
                    effects[str(actor.sprite)] = true
                    sample("effect-resource-"+str(actor.sprite),s)
            for layer in [draw.get("background"),draw.get("foreground")]:
                if layer == null: continue
                if layer.get("first") != null and layer.first.get("selector",{}).get("pixels",24) > 24:
                    hd_terrain += 1
        if draw != null and draw.get("cue") != null and draw.cue.get("terrainPalette",{}).get("active",false):
            var bank: Dictionary = draw.cue.terrainPalette
            if bank.blocks > 0:
                if bank.colors != draw.cue.scenePalette.projectedTerrain:
                    finish("Terrain texture bank differs from scene projection")
                    return
                for pixel in bank.slots:
                    if pixel.slot == 0: continue
                    var word := int(pixel.word)
                    if abs(pixel.r-float(word & 14)/14) > 0.004 or abs(pixel.g-float((word >> 4) & 14)/14) > 0.004 or abs(pixel.b-float((word >> 8) & 14)/14) > 0.004 or pixel.a != 1:
                        finish("Prepared terrain pixel differs from its palette slot")
                        return
                palette_proofs[str(draw.token)] = true
        if palette != null:
            var phase := "restore" if palette.elapsed >= 10 and palette.operation == "PulseScenePalette" else "begin"
            var key := str(palette.token) + phase
            if not palettes.has(key):
                palettes[key] = true
                sample("palette-"+key,s)
                if phase == "restore":
                    pulses += 1
                    if s.display.Scene.Terrain != palette.previousTerrain:
                        finish("Storm bank did not restore")
                        return
        if s.wait == "SceneEndWait" and not full_sequence:
            sample("terminal",s)
            var scene := OS.get_environment("SF2_OPENING_SCENE")
            var count := 16 if scene == "1" else 34 if scene == "4" else 13
            if texts.size() != count: failures.append("Automatic message coverage: " + str(texts.size()))
            if scene == "2" and pulses != 6: failures.append("Restored storm pulse count: " + str(pulses))
            if s.display.Scene != null: failures.append("Terminal retained temporary palette")
            if palette_proofs.is_empty(): failures.append("No actual palette texture proof")
            if exterior_proofs == 0: failures.append("No actual exterior draw proof")
            if not effects.has("183" if scene == "1" else "187"): failures.append("Required effect sprite was not drawn")
            finish("")
            return
        await process_frame

func press(key: Key) -> void:
    for down in [true,false]:
        var event := InputEventKey.new()
        event.keycode=key; event.physical_keycode=key; event.pressed=down
        Input.parse_input_event(event)
        for i in range(4): await process_frame

func wait_slot(expected: String) -> bool:
    var deadline := Time.get_ticks_msec()+60000
    while Time.get_ticks_msec() < deadline:
        var slots := host.find_child("SaveSlotView",true,false)
        if slots != null and JSON.parse_string(slots.call("ReadSaveObservationJson")).get("state") == expected: return true
        await process_frame
    failures.append("New Game slot boundary: "+expected)
    return false

func new_game() -> void:
    await press(KEY_ENTER); await press(KEY_E); await press(KEY_ENTER)
    if not await wait_slot("new-game-slots"): return
    await press(KEY_ENTER)
    var helper: Node = load("res://probes/OpeningStartupControl.cs").new()
    root.add_child(helper); helper.call("SetNewGameSeeds",host)
    await press(KEY_ENTER)
    if not await wait_slot("confirm-new-game"): return
    await press(KEY_DOWN); await press(KEY_ENTER)
    var deadline := Time.get_ticks_msec()+60000
    while Time.get_ticks_msec() < deadline:
        var field := host.find_child("ExplorationSessionView",true,false)
        if field != null:
            samples.append({"label":"new-game","state":field.call("ReadCaptureState",true,false),
                "persistence":JSON.parse_string(host.call("ReadPersistenceObservationJson"))})
            return
        await process_frame
    failures.append("New Game did not mount")

func finish(reason: String) -> void:
    if ended: return
    ended = true
    if reason != "": failures.append(reason)
    var output := FileAccess.open(OS.get_environment("SF2_OPENING_OUTPUT"),FileAccess.WRITE)
    if output != null:
        output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
            "elapsedMs":Time.get_ticks_msec()-began,"texts":texts.keys(),"restoredPulses":pulses,"paletteTextureProofs":palette_proofs.size(),"effectSprites":effects.keys(),"exteriorProofs":exterior_proofs,
            "sceneOrder":scene_order,"hdActors":hd_actors,"hdTerrainProofs":hd_terrain,"focusPauses":focus_pauses,
            "samples":samples},"  "))
        output.close()
    else: push_error("Missing observation output")
    if is_instance_valid(host):
        host.queue_free()
        await process_frame
        await process_frame
    quit(0 if failures.is_empty() else 1)
