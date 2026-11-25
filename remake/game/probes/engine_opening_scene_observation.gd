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

func _initialize() -> void:
    create_timer(240).timeout.connect(func(): finish("240-second scene deadline"))
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
    view = host.find_child("ExplorationSessionView",true,false)
    if view == null:
        var labels := []
        for label in host.find_children("*","Label",true,false):
            if label.is_visible_in_tree(): labels.append(label.text)
        samples.append({"label":"admission-failure","labels":labels})
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
        if not root.has_focus():
            if recovered:
                finish("Native focus lost after one recovery")
                return
            recovered = true
            root.grab_focus()
            await process_frame
            if not root.has_focus():
                finish("Native focus unavailable")
                return
        var s: Dictionary = view.call("ReadCaptureState",false,false)
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
                if actor.visible and actor.sprite in [183,187] and not effects.has(str(actor.sprite)):
                    effects[str(actor.sprite)] = true
                    sample("effect-resource-"+str(actor.sprite),s)
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
        if s.wait == "SceneEndWait":
            sample("terminal",s)
            var scene := OS.get_environment("SF2_OPENING_SCENE")
            var count := 16 if scene == "1" else 13
            if texts.size() != count: failures.append("Automatic message coverage: " + str(texts.size()))
            if scene == "2" and pulses != 6: failures.append("Restored storm pulse count: " + str(pulses))
            if s.display.Scene != null: failures.append("Terminal retained temporary palette")
            if palette_proofs.is_empty(): failures.append("No actual palette texture proof")
            if exterior_proofs == 0: failures.append("No actual exterior draw proof")
            if not effects.has("183" if scene == "1" else "187"): failures.append("Required effect sprite was not drawn")
            finish("")
            return
        await process_frame

func finish(reason: String) -> void:
    if ended: return
    ended = true
    if reason != "": failures.append(reason)
    var output := FileAccess.open(OS.get_environment("SF2_OPENING_OUTPUT"),FileAccess.WRITE)
    if output != null:
        output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,
            "elapsedMs":Time.get_ticks_msec()-began,"texts":texts.keys(),"restoredPulses":pulses,"paletteTextureProofs":palette_proofs.size(),"effectSprites":effects.keys(),"exteriorProofs":exterior_proofs,
            "samples":samples},"  "))
        output.close()
    else: push_error("Missing observation output")
    if is_instance_valid(host):
        host.queue_free()
        await process_frame
        await process_frame
    quit(0 if failures.is_empty() else 1)
