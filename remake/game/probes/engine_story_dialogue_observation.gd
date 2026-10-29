extends "res://probes/engine_world_viewport_observation.gd"

# Semantic checkpoints only. Ordinary mapped events; no session setters/completions.
var receipts: Array = []
var last_sequence := -1
var placement := ""
var text_window := false

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", on_result)

func on_result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if int(event.Sequence) <= last_sequence: continue
        if event.Kind in ["text-w2-accepted", "text-revealed", "dialogue-acknowledged", "choice-selected", "choice-returned", "presentation-completed", "region-program-started"] or event.Kind.begins_with("choice-"):
            if receipts.size() < 100: receipts.append(event)
        last_sequence = int(event.Sequence)

func sample(label: String) -> Dictionary:
    var s := state()
    var compact := {}
    for name in ["sessionId","revision","mode","stage","actor","map","wait","token","stop","failure","failureVisible","canWaitAtInput","canWaitForText","canWaitForChoice","choiceRequiresRelease","gameplayHeld","visibleCharacters","totalCharacters","textId","dialogue","help","dialoguePanel","portraitId","portraitFlags","portraitWork","portraitProjection","portraitResourceProjection","textWindow","portraitWindow","choice","choiceProjection","flags","viewport","mapViewport","worldGeometry","story","cameraFocus","entrySkip","skipRequiresRelease","simulationTick"]:
        if s.has(name): compact[name] = s[name]
    samples.append({"label":label,"state":compact})
    return s

func rect(d: Dictionary) -> Rect2:
    return Rect2(d.x,d.y,d.width,d.height)

func layout(s: Dictionary) -> bool:
    var p: Dictionary = s.dialoguePanel if s.has("dialoguePanel") else s.story.panel
    if not check(p.fontSize == 30 and p.visible, "Visible shared 30px dialogue"): return false
    if not check(Rect2(32,32,1856,1016).encloses(rect(p.panel)), "Panel inside safe area"): return false
    if not check(rect(p.panel).encloses(rect(p.body)) and rect(p.panel).encloses(rect(p.hintRect)), "Body and hint inside panel"): return false
    if not check(not rect(p.body).intersects(rect(p.hintRect)), "Body clear of hint"): return false
    if p.choicesVisible:
        if not check(rect(p.panel).encloses(rect(p.choice)) and not rect(p.choice).intersects(rect(p.body)) and not rect(p.choice).intersects(rect(p.hintRect)), "Attached choices clear of text/hints"): return false
    if p.portraitVisible:
        if not check(p.portraitAboveFrame and p.portraitTexture != null and rect(p.panel).encloses(rect(p.portrait)), "Actual portrait child above frame and inside panel"): return false
        if not check(not rect(p.portrait).intersects(rect(p.body)) and not rect(p.portrait).intersects(rect(p.hintRect)), "Portrait clear of body/hint"): return false
    if s.has("worldGeometry"):
        if not check(s.viewport.width == 1920 and s.viewport.height == 1080 and s.worldGeometry.cellSize == 96 and s.mapViewport == s.viewport, "World stays at accepted geometry"): return false
    return true

func ready_text() -> bool:
    return await wait_for(func(s): return s.visibleCharacters < 0 or s.visibleCharacters >= s.totalCharacters, "actual reveal", 8.0)

func continue_text() -> bool:
    var s := state()
    if s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters:
        if not await tap(KEY_ENTER): return false
    return await tap(KEY_ENTER)

func run() -> void:
    root.size = Vector2i(1920,1080)
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(6)
    state()
    await frames(4)
    if not await focus(): return
    if case_name.begins_with("private"):
        await private_scene()
    elif case_name == "skip":
        await skip_entry()
    elif case_name == "battle":
        await battle_story()
    else:
        await authored()

func authored() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "authored control"): return
    var initial := sample("field-context")
    if not check(not initial.dialoguePanel.visible and initial.help.contains(": Talk") and initial.help.contains(": Wait"), "Field hides dialogue and shows actual Talk/Wait"): return
    if not await tap(KEY_ENTER): return
    await frames(3)
    var started := sample("talk-reveal")
    if not layout(started): return
    if case_name == "top" and not check(started.dialoguePanel.placement == "top", "Bottom subject selects top once"): return
    if not check(not started.dialoguePanel.portraitVisible and started.dialoguePanel.speakerName == null, "No invented portrait/name"): return
    if case_name == "instant":
        if not check(started.visibleCharacters < 0, "Instant mode delivers immediately"): return
    else:
        if not check(started.visibleCharacters >= 0 and started.visibleCharacters < started.totalCharacters, "Incremental reveal reached"): return
        await tap(KEY_ESCAPE)
        if not check(state().token == started.token, "Cancel does not skip ordinary text"): return
        key_event(KEY_ENTER,true)
        await frames(3)
        var revealed := sample("reveal-all")
        if not check(revealed.token == started.token and revealed.visibleCharacters >= revealed.totalCharacters, "Reveal-all retains real input token"): return
        await frames(20)
        var echo := InputEventKey.new()
        echo.keycode = KEY_ENTER; echo.pressed = true; echo.echo = true
        Input.parse_input_event(echo)
        await frames(3)
        if not check(state().token == started.token, "Held/echo Confirm cannot acknowledge"): return
        key_event(KEY_ENTER,false)
        await frames(2)
        var stable_place: String = state().dialoguePanel.placement
        view.hide()
        await frames(10)
        if not check(state().token == started.token, "Hidden dialogue retains pending token"): return
        view.show()
        await frames(3)
        if not check(state().dialoguePanel.placement == stable_place, "Hidden return keeps placement"): return
    if case_name in ["overflow","repeat"]:
        var long_text := sample("measured-overflow")
        if not check(long_text.dialoguePanel.moreBelow and long_text.dialoguePanel.lines > 25, "Measured content needs reading pages"): return
        var pages := 0
        while state().dialoguePanel.moreBelow and pages < 20:
            if not await tap(KEY_ENTER): return
            pages += 1
            if not check(state().token == started.token, "Reading page never acknowledges"): return
        sample("last-reading-page")
        if not check(not state().dialoguePanel.moreBelow and pages > 0, "Full text can be read at 30px"): return
    if not await tap(KEY_ENTER): return
    if case_name == "failure":
        await frames(20)
        var failed := sample("stopped-scene")
        if not check(failed.failure != null and failed.failureVisible, "Controlled existing failure readable"): return
        var before: int = failed.revision
        await tap(KEY_ENTER); await tap(KEY_ESCAPE); await frames(20)
        if not check(state().revision == before and state().failure == failed.failure, "Failure retains state without fake retry"): return
        finish(true); return
    if case_name == "repeat":
        var repeated := sample("repeated-new-window")
        if not check(repeated.wait == "DialogueWait" and repeated.token != started.token and repeated.dialogue == started.dialogue and repeated.dialoguePanel.scroll == 0 and repeated.dialoguePanel.moreBelow, "Identical new window resets reading independently of text prefix"): return
        await tap(KEY_ENTER)
        var pages := 0
        while state().dialoguePanel.moreBelow and pages < 20:
            await tap(KEY_ENTER)
            pages += 1
        if not check(not state().dialoguePanel.moreBelow, "Repeated window readable from beginning"): return
        await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.wait == "ChoiceWait", "authored choice"): return
    var choice := sample("authored-question")
    if not layout(choice): return
    if not check(choice.dialogue == started.dialogue and choice.dialoguePanel.choicesVisible and choice.choice.Work == null, "Question retained with actual simplified choice semantics"): return
    if not await tap(KEY_ESCAPE): return
    if not await wait_for(func(s): return s.canWaitAtInput, "No returns control"): return
    var declined := sample("declined-closed")
    if not check(not declined.dialoguePanel.visible and not declined.flags.any(func(f): return int(f) == 10), "Cancel=No closes the window"): return
    var before_move: Array = declined.entities.map(func(e): return [e.id,e.x,e.y])
    await tap(KEY_UP if case_name == "top" else KEY_DOWN)
    if not await wait_for(func(s): return s.canWaitAtInput, "fresh movement"): return
    if not check(state().entities.map(func(e): return [e.id,e.x,e.y]) != before_move, "Fresh direction works after close"): return
    if case_name in ["overflow","repeat"]:
        finish(true); return
    # Return to the same actual interaction, then choose Yes.
    await tap(KEY_DOWN if case_name == "top" else KEY_UP)
    if not await wait_for(func(s): return s.canWaitAtInput, "return step"): return
    await tap(KEY_RIGHT)
    if not await wait_for(func(s): return s.canWaitAtInput, "face obstruction"): return
    await tap(KEY_ENTER)
    if not await ready_text(): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.wait == "ChoiceWait", "second choice"): return
    await tap(KEY_ENTER)
    await frames(3)
    var automatic := sample("accepted-automatic")
    if not check(automatic.flags.any(func(f): return int(f) == 10) and not automatic.help.contains("Continue") and not automatic.help.contains("▾"), "Yes retains automatic scene without false Continue"): return
    if not await wait_for(func(s): return s.canWaitAtInput and s.map == "yard-map", "automatic transfer return"): return
    var returned := sample("returned-control")
    if not check(not returned.dialoguePanel.visible and returned.sessionId == initial.sessionId, "Same-session automatic close/control"): return
    finish(true)

func private_scene() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "Zone8 approach input"): return
    sample("private-before-zone")
    await tap(KEY_DOWN)
    if not await wait_for(func(s): return s.canWaitAtInput, "Zone8 approach"): return
    await tap(KEY_RIGHT)
    var last_text := -1
    var reached_portrait := false
    var deadline := Time.get_ticks_msec()+60000
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return
        var s := state()
        if not check(s.failure == null, "Healthy private scene"): return
        var p: Dictionary = s.dialoguePanel
        if p.visible:
            if not text_window: placement = p.placement
            if not check(p.placement == placement, "Placement stable for window lifetime"): return
            text_window = true
        else: text_window = false
        if p.portraitVisible and not reached_portrait:
            await frames(2)
            var portrait := sample("private-portrait-child-draw")
            if not layout(portrait): return
            if not check(portrait.portraitProjection != null and portrait.portraitProjection.id >= 0, "Existing real portrait selector and draw"): return
            reached_portrait = true
        var text := int(s.textId) if s.textId != null else -1
        if text != last_text and not s.dialogue.is_empty():
            await frames(2)
            var line := sample("private-text-"+str(text))
            if not layout(line): return
            last_text = text
        if text == 535 and s.canWaitForText and case_name == "private-yes":
            var accepted := sample("source-yes-next-input")
            if not check(accepted.choice == null and not accepted.dialoguePanel.choicesVisible, "Source Yes returns to actual next text with choice deleted"): return
            finish(true); return
        if text == 525 and s.canWaitForText and case_name == "private":
            sample("private-525-input")
            if not check(reached_portrait, "Real source portrait reached"): return
            finish(true); return
        if s.wait == "ChoiceWait" and s.canWaitForChoice and not s.choiceRequiresRelease:
            var question := sample("source-choice-default")
            if not layout(question): return
            if not check(question.choice.Work.Yes, "Source default answer retained"): return
            await tap(KEY_RIGHT)
            sample("source-choice-no-focus")
            await tap(KEY_LEFT)
            var answer_key := KEY_ENTER if case_name == "private-yes" else KEY_ESCAPE
            key_event(answer_key,true)
            await frames(5)
            var held := sample("source-choice-held-release")
            if not check(held.entrySkip == null and held.choice.Work.Yes == (case_name == "private-yes"), "Actual answer and skip exclusion retained under held key"): return
            key_event(answer_key,false)
            await frames(3)
        elif s.canWaitAtInput and last_text > 525:
            var returned := sample("source-choice-return")
            if not check(not returned.dialoguePanel.visible, "Source decline closes window"): return
            await tap(KEY_DOWN)
            if not await wait_for(func(x): return x.canWaitAtInput, "source fresh movement"): return
            sample("source-fresh-control")
            finish(true); return
        elif s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters:
            await tap(KEY_ENTER)
        elif s.canWaitForText or (s.get("w1") != null and s.w1.AtInput):
            await tap(KEY_ENTER)
        else: await process_frame
    fail("Private bounded seam deadline")

func battle_story() -> void:
    if not await wait_for(func(s): return s.get("stage") == "Movement", "battle start"): return
    for step in range(2):
        await tap(KEY_UP)
        if not await wait_for(func(s): return s.get("stage") == "Movement" and JSON.parse_string(view.call("ReadSceneObservationJson")).movement == null, "settled battle preview"): return
    await tap(KEY_ENTER); await tap(KEY_SPACE); await tap(KEY_ENTER)
    var delivered := 0
    var previous := -1
    var deadline := Time.get_ticks_msec()+45000
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return
        var s := state()
        if not check(s.failure == null, "Healthy battle story"): return
        if s.story.active:
            if s.story.kind == "DialogueWait" and s.story.presented:
                if int(s.story.token) != previous:
                    previous = int(s.story.token); delivered += 1
                    var line := sample("battle-story-"+str(delivered))
                    if not layout(line): return
                    if not check(line.story.cameraSlot == 1 and line.cameraFocus.size() == 1, "Story camera retains actual subject"): return
                await tap(KEY_ENTER)
            else: await process_frame
        elif delivered >= 2 and s.get("stage") == "Movement":
            sample("battle-story-closed")
            if not check(not s.story.panel.visible, "Battle story closes after real acknowledgements"): return
            var preview: float = s.previewY
            await tap(KEY_DOWN)
            if not await wait_for(func(x): return JSON.parse_string(view.call("ReadSceneObservationJson")).movement == null, "fresh battle preview"): return
            if not check(state().previewY != preview, "Fresh battle movement after story"): return
            sample("battle-fresh-movement")
            finish(true); return
        else: await process_frame
    fail("Bounded battle-story deadline")


func skip_entry() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput", false), "skip authored start"): return
    await tap(KEY_ENTER)
    await tap(KEY_ENTER)
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.wait == "ChoiceWait", "choice before eligible intro"): return
    if not check(not state().help.contains("Skip battle intro"), "Choice never offers skip"): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return str(s.get("help", "")).contains("Skip battle intro"), "actual eligible entry hint"): return
    sample("eligible-entry-hint")
    key_event(KEY_ESCAPE,true)
    if not await wait_for(func(s): return s.get("battleHandoffPending",false), "held skip handoff"): return
    var held := sample("skip-held-handoff")
    if not check(held.skipRequiresRelease and held.battleHandoffPending, "Held skip cannot become combat input"): return
    key_event(KEY_ESCAPE,false)
    if not await wait_for(func(s): return s.get("stage") == "Movement", "skip release to battle"): return
    sample("skip-released-combat")
    await tap(KEY_ENTER)
    if not check(state().stage == "ActionChoice", "Fresh Confirm opens actual menu"): return
    await tap(KEY_ESCAPE)
    if not check(state().stage == "Movement", "Fresh Cancel closes menu normally"): return
    sample("skip-fresh-controls")
    finish(true)
