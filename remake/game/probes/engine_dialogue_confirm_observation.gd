extends "res://probes/engine_exploration_control_observation.gd"

# Finite generated authored content, installed before ordinary Main startup. No live state edits.
var reading_case := OS.get_environment("SF2_READING_CASE")
var receipts: Array = []
var receipt_sequence := -1
var confirm_usec := 0
var interrupt_service := false
var interrupted_tick := -1

func raster(width: int, height: int) -> Dictionary:
    var bytes := PackedByteArray()
    bytes.resize(width * height * 4)
    var hash := HashingContext.new()
    hash.start(HashingContext.HASH_SHA256)
    hash.update(bytes)
    return {"width":width,"height":height,"format":"rgba8","data":Marshalls.raw_to_base64(bytes),"sha256":hash.finish().hex_encode()}

func music(cue: String, command: int, frames: int, end_step: int) -> Dictionary:
    var bytes := PackedByteArray()
    bytes.resize(frames * 2)
    var hash := HashingContext.new()
    hash.start(HashingContext.HASH_SHA256)
    hash.update(bytes)
    var clip := {"cue":cue,"command":command,"timerB":0,"sampleRate":8000,"channels":1,
        "sampleFrames":frames,"pcm16":Marshalls.raw_to_base64(bytes),"sha256":hash.finish().hex_encode(),
        "loopBegin":null if end_step else 0,"loopEnd":null if end_step else frames}
    if end_step: clip.modernEndStep = end_step
    return clip

func integral_numbers(value: Variant) -> Variant:
    if value is Dictionary:
        for name in value: value[name] = integral_numbers(value[name])
    elif value is Array:
        for index in range(value.size()): value[index] = integral_numbers(value[index])
    elif value is float and value == floor(value): return int(value)
    return value

func prepare_package() -> void:
    var source := ProjectSettings.globalize_path("res://../content/authored/harbor-arrival.json")
    var doc: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(source))
    var field := reading_case.begins_with("field")
    var w1 := reading_case == "w1"
    var portrait := reading_case.contains("portrait")
    var words := "First passage. Confirm proceeds to the next passage."
    if reading_case == "long": words = "A complete readable line of this long passage.\n".repeat(55)
    if field or w1: words += "{W1}Second passage.{W1}"
    if reading_case == "field-pause": words = "A moment{D1}.{D1}.{D1}.{D1} All set.{W1}Second passage.{W1}"
    if reading_case == "field-clear": words = "A{CLEAR}New cleared passage.{W1}Second passage.{W1}"
    doc.world.texts[0].text = words
    doc.world.texts[1].text = "Final passage."
    doc.world.memberNames = ["Leader","Companion"]
    var instructions: Array = [{"op":"text-cursor","text":100},
        {"op":"show-text","mode":"single","speaker":"ferryman" if field else null,"useEventSpeaker":w1,"explicitWindows":field or w1}]
    if not field and not w1:
        instructions.append({"op":"show-text","mode":"single","speaker":null})
    instructions.append({"op":"end"})
    doc.world.programs[0].instructions = instructions
    doc.world.programs[0].entitiesRunning = false
    if field or w1:
        doc.world.maps[0].entities[0].sprite = 30
        doc.world.maps[0].events[0].entityFlags = 3
        var maps: Array = []
        for map in doc.world.maps:
            map.layout = []
            for row in range(31):
                var cells: Array = []
                cells.resize(31); cells.fill(0)
                map.layout.append(cells)
            map.areas = [{"minX":0,"minY":0,"maxX":30,"maxY":30,"view":{"foregroundX":0,"foregroundY":32,"backgroundX":0,"backgroundY":0,"parallaxAX":256,"parallaxAY":256,"parallaxBX":256,"parallaxBY":256,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":0}}]
            var blocks: Array = []
            for block in range(1024): blocks.append([0,0,0,0,0,0,0,0,0])
            maps.append({"map":map.id,"atlas":raster(128,320),"scale":1,"blocks":blocks})
        doc.world.presentation = {"maps":maps,"sprites":[{"sprite":30,"directions":[raster(48,24),raster(48,24),raster(48,24)],"portrait":7 if portrait else null,"speech":0}],
            "portraits":[{"portrait":7,"raster":raster(64,64),"eyes":[[1,1,6,7]],"mouth":[[1,3,7,7]]}]}
        doc.world.programs.append({"id":"setup","instructions":[{"op":"sprite","entity":"traveler","sprite":30},{"op":"close-portrait"},{"op":"end"}]})
        doc.start.program = {"program":"setup","instruction":0}
        if field:
            var symbols: Array = []; symbols.resize(256); symbols.fill(1)
            var advances: Array = []; advances.resize(80); advances.fill(6)
            doc.world.textFont = {"asciiToSymbol":symbols,"advances":advances}
            doc.start.textSettings = {"messageSpeed":0,"mouthControl":0,"viewSpeed":0}

    if reading_case == "field-join":
        doc.world.texts[0].text = "Companion joined."
        doc.world.partyFlags = {"memberCount":2,"joinedStart":10,"activeStart":20,"capacity":2}
        doc.start.flags = [10,20]
        for visual in doc.world.presentation.maps:
            visual.music = [{"field":8,"battle":8}]
        doc.world.presentation.audio = [music("town",8,800,0),music("join",19,64000,505)]
        doc.world.programs[0].instructions = [
            {"op":"camera-entity","entity":"ferryman"},{"op":"wait-view"},
            {"op":"text-cursor","text":100},
            {"op":"present","kind":"Sound","resource":"join","entity":null,"position":null},
            {"op":"join-party","member":1},
            {"op":"try-add-actor-item","actor":"medic-a","item":5},
            {"op":"show-text","mode":"single","speaker":null,"waitForAcknowledgement":false,"explicitWindows":true},
            {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},
            {"op":"present","kind":"PreviousMusic","resource":null,"entity":null,"position":null},
            {"op":"wait-text-input"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},
            {"op":"face","entity":"ferryman","facing":3},{"op":"camera-entity","entity":"traveler"},
            {"op":"set-flag","flag":906,"value":true},{"op":"end"}]

    var file := FileAccess.open(OS.get_environment("SF2_READING_PACKAGE"),FileAccess.WRITE)
    file.store_string(JSON.stringify(integral_numbers(doc))); file.close()

func on_reading_result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations",[]):
        if int(event.Sequence) <= receipt_sequence: continue
        receipt_sequence = int(event.Sequence)
        if interrupt_service and event.Kind == "text-mandatory-service":
            interrupt_service = false
            interrupted_tick = state().simulationTick
            view.hide() # Real host invalidation from the first actual service callback.
        if event.Kind in ["text-revealed","text-w1-accepted","text-w2-accepted","presentation-acknowledged",
                "text-closed","entity-event-returned","text-delay-interrupted","music-delivery-skipped",
                "music-wait-returned","music-actual-completed"]:
            receipts.append({"kind":event.Kind,"tick":state().simulationTick,"usec":Time.get_ticks_usec(),"frame":Engine.get_process_frames()})

func observe_join() -> void:
    if not await wait_until(func(): return state().wait == "MusicWait","join playback and raw text"): return
    var playing := record("join-music-playing")
    if not require(playing.audio.musicPlaying and playing.audio.musicCue == "join" and
            not playing.musicWait.LogicalDone and playing.dialogue == "Companion joined." and playing.logicalView.TargetSlot != 0,
            "actual finite audio and recruitment text before Confirm"): return
    if not await key(KEY_ENTER,true): return
    if not await wait_until(func(): return state().wait == "DialogueWait","fresh edge restores music and reaches plain input",2.0): return
    var pending: Dictionary = integral_numbers(record("join-acknowledgement-pending"))
    if not require(pending.audio.musicPlaying and pending.audio.musicCue == "town" and pending.music.Cue == "town" and
            pending.partyLists.Joined == [0,1] and not 906 in pending.flags,"previous cue, membership and unfinished caller"): return
    if not require(receipts.filter(func(row): return row.kind == "music-delivery-skipped").size() == 1 and
            receipts.filter(func(row): return row.kind == "music-actual-completed").is_empty() and
            receipts.filter(func(row): return row.kind == "music-wait-returned").size() == 1,
            "one accelerated helper return with skipped playback receipt"): return
    var playback: Array = pending.audio.receipts
    if not require(playback.filter(func(row): return row.Operation == "stopped" and row.Cue == "join").size() == 1 and
            playback.filter(func(row): return row.Operation == "started" and row.Cue == "town").size() == 2,
            "playback actually stopped and previous cue restarted once"): return
    if not await key(KEY_ENTER,true,true): return
    if not await rest(0.2): return
    if not require(state().token == pending.token,"held and echo Confirm cannot acknowledge restored join text"): return
    if not await key(KEY_ENTER,false): return
    if not await tap(KEY_ENTER): return
    if not await wait_until(func(): return state().canWaitAtInput,"separate acknowledgement and caller cleanup",2.0): return
    var returned: Dictionary = integral_numbers(record("field-control-after-join"))
    var items: Array = returned.party[0].SourceLoadout.Items
    if not require(returned.textWindow == "ClosedTextWindow" and returned.portraitWindow == "ClosedPortraitWindow" and
            returned.partyLists.Joined == [0,1] and 906 in returned.flags and items.count(5) == 1 and
            returned.logicalView.TargetSlot == 0 and returned.audio.musicCue == "town" and returned.audio.musicPlaying,
            "field control after exactly one reward, flag, camera restore and window close"): return
    samples.append({"label":"join-receipts","receipts":receipts})
    _finish()

func visible_text_pause() -> bool:
    var current := state()
    return current.fieldText != null and current.fieldText.Phase == 9 and current.visibleCharacters >= current.totalCharacters

func observe_galam() -> void:
    if not await wait_until(visible_text_pause,"fully displayed Galam text at live D1",12.0): return
    var before := record("galam-text-899")
    if not require(before.textId == 899 and not before.dialoguePanel.moreBelow,"repeat maid dialogue is visible"): return
    if not await key(KEY_ENTER,true): return
    if not await wait_until(func(): return state().canWaitAtInput,"repeat dialogue returns to Galam",2.0): return
    if not await key(KEY_ENTER,true,true): return
    if not await rest(0.15): return
    var after: Dictionary = integral_numbers(record("galam-field-control"))
    if not require(after.canWaitAtInput and after.textWindow == "ClosedTextWindow" and
            after.map == "map-17" and 256 in after.flags,"held Confirm leaves settled control and visit latch"): return
    if not require(receipts.filter(func(row): return row.kind == "text-delay-interrupted").size() == 4 and
            receipts.filter(func(row): return row.kind == "text-w1-accepted").size() == 1,
            "one fresh Confirm ends all four D1 pauses and acknowledges once"): return
    if not await key(KEY_ENTER,false): return
    samples.append({"label":"galam-receipts","receipts":receipts})
    _finish()

func _run() -> void:
    if reading_case != "private-galam-pause": prepare_package()
    root.size = Vector2i(1920,1080)
    node_added.connect(func(node):
        if node.has_signal("SessionResultObserved"): node.connect("SessionResultObserved",on_reading_result))
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if reading_case == "private-galam-pause":
        var deadline := Time.get_ticks_msec() + 20000
        while view == null and Time.get_ticks_msec() < deadline:
            await process_frame
            view = host.get_node_or_null("ExplorationSessionView")
    if view == null: samples.append({"label":"startup","labels":host.find_children("*","Label",true,false).map(func(node): return node.text)})
    if not require(view != null,"ordinary host mounted"): return
    identity = state().sessionId
    if not await wait_until(func(): return state().canWaitAtInput,"initial control"): return
    if not await tap(KEY_ENTER): return
    if reading_case == "field-join":
        await observe_join()
        return
    if reading_case == "private-galam-pause":
        await observe_galam()
        return
    if not await wait_until(func(): return state().wait in ["DialogueWait","W1TextWait","FieldTextWait"],"ordinary text"): return
    var first := record("first-revealing")
    if not require(first.visibleCharacters < first.totalCharacters,"gradual display before Confirm"): return
    if reading_case.contains("portrait"):
        if not require(first.portraitWindow == "OpenPortraitWindow" and first.dialoguePanel.portraitVisible,"actual admitted portrait mounted"): return
    else:
        if not require(not first.dialoguePanel.portraitVisible,"no portrait control"): return
    if not await tap(KEY_ESCAPE): return
    if not require(state().token == first.token,"Cancel cannot dismiss ordinary text"): return
    if reading_case == "long":
        if not await tap(KEY_ENTER): return
        if not require(state().token == first.token and state().dialoguePanel.moreBelow,"first Confirm exposes current long page"): return
        var pages := 0
        while state().dialoguePanel.moreBelow and pages < 20:
            if not await tap(KEY_ENTER): return
            pages += 1
            if not require(state().token == first.token,"each reading-page edge retains token"): return
        record("last-page")
        if not require(pages > 0 and not state().dialoguePanel.moreBelow,"complete long passage readable"): return
    if reading_case == "field-pause":
        if not await wait_until(visible_text_pause,"visible text at live D1"): return
        record("visible-text-at-d1")
    interrupt_service = reading_case == "field-interrupt"
    confirm_usec = Time.get_ticks_usec()
    if not await key(KEY_ENTER,true): return
    if reading_case == "field-interrupt":
        if not require(interrupted_tick >= 0 and state().simulationTick == interrupted_tick,"host invalidation stops same batch after one real service"): return
        record("interrupted-during-service")
        if not await rest(0.1): return
        if not require(state().simulationTick == interrupted_tick,"hidden host remains frozen"): return
        if not await key(KEY_ENTER,false): return
        view.show()
        if not await wait_until(func(): return state().canWaitForText,"natural text work after restoration",6.0): return
        if not require(state().token == first.token,"restoration cannot replay cancelled reading intent"): return
        record("restored-pending-input")
        if not await key(KEY_ENTER,true): return
    if reading_case == "field-clear":
        if not await wait_until(func(): return state().fieldText != null and state().fieldText.DisplayStart > 0,"CLEAR projection"): return
        var cleared := record("clear-requires-new-reading")
        if not await rest(0.12): return
        if not require(state().token == first.token and state().fieldText.DisplayStart == cleared.fieldText.DisplayStart,"CLEAR cancels previous reading intent"): return
        if not await key(KEY_ENTER,false): return
        if not await key(KEY_ENTER,true): return
    if not await wait_until(func(): return state().token != first.token,"one Confirm reaches next segment",2.0): return
    var next := record("next-segment")
    if reading_case == "field-pause":
        var pauses := receipts.filter(func(row): return row.kind == "text-delay-interrupted")
        if not require(pauses.size() == 4 and pauses[0].frame == pauses[3].frame,
                "one fresh Confirm ends the four pauses in the displayed span"): return
    if not await key(KEY_ENTER,true,true): return
    if not await rest(0.15): return
    if not require(state().token == next.token,"held and echo Confirm cannot cross next segment"): return
    if not await key(KEY_ENTER,false): return
    if not await text_ready(): return
    record("fully-visible-final")
    # A pad press uses the same semantic Confirm; repeated down events cannot close another token.
    var button := InputEventJoypadButton.new()
    button.button_index = JOY_BUTTON_A; button.pressed = true
    Input.parse_input_event(button)
    await process_frame
    await process_frame
    button = InputEventJoypadButton.new()
    button.button_index = JOY_BUTTON_A; button.pressed = false
    Input.parse_input_event(button)
    if not await wait_until(func(): return state().canWaitAtInput,"final acknowledgement and genuine caller close",2.0): return
    record("usable-field-return")
    samples.append({"label":"delivery-and-acknowledgement","receipts":receipts,"confirmUsec":confirm_usec})
    _finish()
