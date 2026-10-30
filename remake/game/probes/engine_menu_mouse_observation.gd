extends "res://probes/engine_battle_status_observation.gd"

# Finite ordinary-host pointer/keyboard witness. No gameplay setters or screenshots.
func sample(label: String) -> Dictionary:
    var s := state()
    var compact := {}
    for key in ["revision","sessionId","stage","actor","mainSeed","thinkingSeed","inventories","menu","actors","previewX","previewY","failure","wait","token","choice","choicePointer","choiceRequiresRelease","canWaitForChoice","canWaitForText","canWaitAtInput","visibleCharacters","totalCharacters","flags","dialoguePanel","help","textId","simulationTick"]:
        if s.has(key): compact[key] = s[key]
    samples.append({"label":label,"focused":root.has_focus(),"state":compact})
    return s

func receive_input(event: InputEvent) -> void:
    # This recipe uses ordinary settings to unbind the interfering movement axes. Decide from
    # the actual installed actions; mapped pad input still invalidates the controlled witness.
    if event is InputEventJoypadMotion:
        var mapped := false
        for action in InputMap.get_actions():
            if str(action).begins_with("sf2_") and InputMap.event_is_action(event,action,true): mapped = true
        if not mapped:
            var axis_key := str(event.device)+":"+str(event.axis)
            if not neutral_axes.has(axis_key): neutral_axes[axis_key] = {"count":0,"lastMs":0,"unbound":true}
            neutral_axes[axis_key].count += 1
            neutral_axes[axis_key].lastMs = Time.get_ticks_msec()-began
            return
    if event is not InputEventMouse:
        super.receive_input(event)

func nav(id: String) -> Dictionary:
    return state().menu.layout.navigation.filter(func(n): return n.id == id)[0].rect

func resources(s: Dictionary) -> Array:
    return [s.mainSeed, s.thinkingSeed, s.inventories, s.actors.map(func(a): return [a.id,a.hp,a.mp])]

func center(rect: Dictionary) -> Vector2:
    return Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2)

func motion(position: Vector2) -> void:
    var event := InputEventMouseMotion.new()
    event.position = position
    event.global_position = position
    event.relative = position - Vector2(5, 5)
    Input.parse_input_event(event)
    await frames(3)

func button(position: Vector2, pressed: bool, which := MOUSE_BUTTON_LEFT, double_click := false) -> void:
    var event := InputEventMouseButton.new()
    event.position = position
    event.global_position = position
    event.button_index = which
    event.pressed = pressed
    event.double_click = double_click
    Input.parse_input_event(event)
    await frames(3)

func click(rect: Dictionary, which := MOUSE_BUTTON_LEFT) -> void:
    var position := center(rect)
    await motion(position)
    await button(position, true, which)
    await button(position, false, which)

func run() -> void:
    root.size = Vector2i(1920,1080)
    node_added.connect(observe_node)
    root.window_input.connect(receive_input)
    root.focus_entered.connect(func(): receive_focus(true))
    root.focus_exited.connect(func(): receive_focus(false))
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(8)
    if not await focus(): return
    if case_name.begins_with("authored"):
        await authored_choices()
        return
    if case_name.begins_with("private"):
        await private_choices()
        return
    if case_name.contains("icons"):
        var deadline := Time.get_ticks_msec()+10000
        while not state().has("stage"):
            var field := state()
            if not check(field.get("failure") == null, "Existing private entry remains healthy"): return
            if not check(Time.get_ticks_msec() < deadline, "Existing private entry reaches battle through bounded skip"): return
            if field.get("helpVisible",false) and "Skip battle intro" in str(field.get("help","")):
                await tap(KEY_ESCAPE)
            else: await process_frame
    if not await wait_for(func(s): return s.get("stage") == "Movement", "initial battle movement"): return
    var before := state()
    await tap(KEY_ENTER)
    var opened := sample("pilot-opened")
    if not check(opened.stage == "ActionChoice", "Keyboard opens existing action menu"): return
    if case_name.begins_with("emphasis"):
        await focus_emphasis(before)
        return
    if case_name == "focus-battle":
        await battle_focus()
        return
    if case_name == "battle":
        await battle_menus(before)
        return
    if case_name == "physical":
        await physical_menu(before)
        return
    if case_name == "heal":
        await heal_commit(before)
        return
    await motion(center(opened.menu.layout.commands[1].rect))
    sample("pilot-after-motion")
    if not check(state().menu.command == 1, "Actual pointer motion focuses Magic"): return
    await tap(KEY_DOWN)
    if not check(state().menu.command == 3, "Keyboard takes focus back from pointer"): return
    await frames(8)
    if not check(state().menu.command == 3, "Stationary pointer never steals keyboard focus on refresh"): return
    await click(state().menu.layout.commands[1].rect)
    var magic := sample("pilot-magic")
    if not check(magic.menu.page == "Magic" and magic.stage == "ActionChoice", "Deliberate click opens only Magic list"): return
    await click(magic.menu.layout.rowRects[0])
    var target := sample("pilot-target")
    if not check(target.menu.page == "Target" and target.stage == "CommitReady", "List click selects existing action and target without commit"): return
    if not check(target.mainSeed == before.mainSeed and target.actors == before.actors, "No resources or RNG spent before explicit commit"): return
    # Replayed release and a double-click press cannot commit the newly created target control.
    var confirm: Dictionary = target.menu.layout.navigation.filter(func(n): return n.id == "Confirm")[0].rect
    await button(center(confirm), false)
    await button(center(confirm), true, MOUSE_BUTTON_LEFT, true)
    await button(center(confirm), false)
    var rejected := sample("pilot-rejected-release")
    if not check(rejected.stage == "CommitReady" and rejected.revision == target.revision, "Release without press and double click cannot cross-stage commit"): return
    await click(confirm)
    var committed := sample("pilot-explicit-commit")
    if not check(committed.stage != "CommitReady" and committed.revision > target.revision, "Fresh explicit click commits exactly through the existing session"): return
    finish(true)

func emphasis_sample(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label,"ms":Time.get_ticks_msec()-began,"revision":s.revision,
        "command":s.menu.command,"page":s.menu.page,"commands":s.menu.layout.commands})
    return s

func emphasis_active(s: Dictionary, index: int, reduced: bool) -> bool:
    for i in range(4):
        var command: Dictionary = s.menu.layout.commands[i]
        var art: Dictionary = command.art
        if not check(command.selected == (i == index), "Only current command is selected"): return false
        if not check(art.filter == "Nearest" and art.scale == 4, "Nearest integer icon scale is preserved"): return false
        if not check(art.drawnOffsetY == int(art.drawnOffsetY), "Actual drawing stays on integer pixels"): return false
        if i != index or reduced:
            if not check(art.focusRemainingMs == 0 and art.drawnOffsetY == 0, "Unselected/reduced drawing stays stable"): return false
        elif not check(art.focusRemainingMs > 0 and art.drawnOffsetY < 0, "New focus advances actual selected drawing immediately"): return false
        if case_name.contains("icons"):
            if not check(art.mounted and art.width == 24 and art.height == 24 and not art.extraFrame and not art.labelVisible,
                "Admitted original icons retain their existing rendering"): return false
            if not check(art.resource.ends_with(".selected" if i == index else ".normal"), "Existing selected/normal resource is drawn"): return false
    return true

func focus_emphasis(initial: Dictionary) -> void:
    var reduced := case_name.contains("reduced")
    await frames(15)
    var settled := state()
    var rectangles: Array = settled.menu.layout.commands.map(func(c): return c.rect)
    var revision: int = settled.revision
    await tap(KEY_LEFT)
    var first := emphasis_sample("keyboard-emphasis")
    if not emphasis_active(first,1,reduced): return
    await frames(4)
    var advancing := emphasis_sample("keyboard-advances")
    if not reduced and not check(advancing.menu.layout.commands[1].art.drawnOffsetY > first.menu.layout.commands[1].art.drawnOffsetY,
        "Short emphasis advances toward stable selected drawing"): return
    await tap(KEY_RIGHT)
    if not emphasis_active(emphasis_sample("quick-new-focus"),2,reduced): return
    var position := center(state().menu.layout.commands[3].rect)
    await motion(position)
    if not emphasis_active(emphasis_sample("pointer-emphasis"),3,reduced): return
    await frames(16)
    var idle := emphasis_sample("settled-idle")
    if not check(idle.menu.layout.commands.all(func(c): return c.art.focusRemainingMs == 0 and c.art.drawnOffsetY == 0), "Emphasis settles without idle animation"): return
    await motion(position) # Same coordinates are not a fresh focus change.
    if not check(state().menu.layout.commands[3].art.focusRemainingMs == 0, "Stationary pointer cannot restart emphasis"): return
    await tap(KEY_DOWN)
    if not check(state().menu.layout.commands[3].art.focusRemainingMs == 0, "Repeated current direction cannot restart emphasis"): return
    if not check(state().revision == revision and resources(state()) == resources(initial), "Drawing/focus never submits commands or spends resources/RNG"): return
    if not check(state().menu.layout.commands.map(func(c): return c.rect) == rectangles, "Drawing never moves hit rectangles"): return
    await tap(KEY_LEFT)
    view.hide()
    if not check(state().menu.layout.commands.all(func(c): return c.art.focusRemainingMs == 0), "Hiding immediately cancels active emphasis"): return
    await frames(2)
    view.show(); await frames(3)
    var shown := emphasis_sample("shown-stable")
    if not check(shown.menu.layout.commands.all(func(c): return c.art.drawnOffsetY == 0 and c.art.focusRemainingMs == 0), "Showing cannot resume stale emphasis"): return
    if not DisplayServer.get_name() == "headless":
        await tap(KEY_RIGHT)
        if not await transfer_focus(): return
        if not check(state().menu.layout.commands.all(func(c): return c.art.focusRemainingMs == 0 and c.art.drawnOffsetY == 0), "Native focus loss/return leaves stable selection"): return
    await tap(KEY_ESCAPE)
    if not check(state().stage == "Movement" and state().menu.layout.commands.all(func(c): return c.art.focusRemainingMs == 0), "Closing cancels emphasis"): return
    await tap(KEY_ENTER)
    if not emphasis_active(emphasis_sample("reopened-new-selection"),0,reduced): return
    await frames(15)
    if not check(state().menu.layout.commands.all(func(c): return c.art.focusRemainingMs == 0 and c.art.drawnOffsetY == 0), "Reopened selection settles without stale motion"): return
    await tap(KEY_RIGHT)
    if not emphasis_active(emphasis_sample("before-scene-retirement"),2,reduced): return
    host.queue_free()
    await frames(3)
    if not check(not is_instance_valid(host), "Active menu retires with its ordinary host scene"): return
    samples.append({"label":"scene-retired","hostFreed":true})
    finish(true)

func battle_menus(initial: Dictionary) -> void:
    var baseline := resources(initial)
    var magic := center(state().menu.layout.commands[1].rect)
    # Drag out and return, focus transfer, keyboard handoff and replacement all invalidate a press.
    await motion(magic); await button(magic,true)
    await motion(Vector2(960,500)); await motion(magic); await button(magic,false)
    if not check(state().menu.page == "Main", "Drag out and return does not activate"): return
    await button(magic,true)
    view.hide(); await frames(2); view.show(); await frames(2)
    await button(magic,false)
    if not check(state().menu.page == "Main", "Hidden owner cannot reuse a held gesture"): return
    await button(magic,true)
    await tap(KEY_RIGHT)
    await button(magic,false)
    if not check(state().menu.page == "Main" and state().menu.command == 2, "Keyboard focus cancels prior pointer press"): return
    await click(state().menu.layout.commands[0].rect)
    if not check(state().menu.page == "Main" and state().menu.layout.detail.contains("Unavailable"), "Unavailable Attack remains inspectable"): return
    await click(state().menu.layout.commands[1].rect)
    var list := sample("magic-list")
    if not check(list.menu.layout.rows.size() == 4, "Existing long spell offers form a scrollable list"): return
    var list_center := center(list.menu.layout.list)
    for i in range(9):
        await button(list_center,true,MOUSE_BUTTON_WHEEL_DOWN)
    var scrolled := state()
    if not check(scrolled.menu.layout.scroll > 0 and scrolled.menu.row == 0, "Wheel scroll stays local without stealing focus"): return
    await click(scrolled.menu.layout.rowRects[3])
    var refused := sample("unavailable-spell")
    if not check(refused.menu.row == 3 and refused.menu.layout.detail.contains("Not enough MP") and resources(refused) == baseline, "Unavailable spell explains without mutation"): return
    await click(nav("Back"))
    await click(state().menu.layout.commands[1].rect)
    await click(state().menu.layout.rowRects[0])
    var target := sample("available-target")
    if not check(target.menu.page == "Target" and target.target == initial.actor, "Available spell enters target flow with self offer"): return
    await click(nav("NextTarget"))
    var unavailable_target := sample("unavailable-target")
    if not check(unavailable_target.menu.layout.detail.contains("Out of range") and unavailable_target.menu.candidate != target.target, "Target next includes unavailable candidate and diagnostic"): return
    await click(nav("Confirm"))
    if not check(state().target == target.target and resources(state()) == baseline and state().menu.page == "Target", "Disabled candidate cannot commit prior valid target"): return
    await click(nav("PreviousTarget"))
    if not check(state().menu.candidate == initial.actor, "Previous returns through existing ordered targets"): return
    await click(nav("Back"))
    if not check(state().menu.page == "Magic" and state().stage == "ActionChoice", "Target Back restores spell list"): return
    await click({"x":900,"y":400,"width":20,"height":20}, MOUSE_BUTTON_RIGHT)
    if not check(state().menu.page == "Main", "Right click unwinds exactly one list layer"): return
    await click(state().menu.layout.commands[2].rect)
    await click(state().menu.layout.rowRects[2])
    if not check(state().menu.layout.detail.contains("This slot is empty") and state().menu.page == "Items", "Empty item is inspectable and inactive"): return
    await click(state().menu.layout.rowRects[0])
    if not check(state().menu.page == "Target", "Available item enters target flow"): return
    await click(nav("Back")); await click(nav("Back"))
    await click(state().menu.layout.commands[3].rect)
    if not check(state().menu.page == "Commit" and resources(state()) == baseline, "Stay requires separate confirmation"): return
    await click(nav("Back")); await click(nav("Back"))
    if not check(state().stage == "Movement" and resources(state()) == baseline, "Layered Back preserves resources and RNG"): return
    # A held release cannot open a menu created by a different physical input.
    await button(magic,true); await tap(KEY_ENTER); await button(magic,false)
    if not check(state().menu.page == "Main", "Menu creation does not adopt an earlier press"): return
    await click(state().menu.layout.commands[3].rect)
    await click(nav("Confirm"))
    if not await wait_for(func(s): return s.get("stage") == "Movement" and s.actor != initial.actor, "Stay advances actor once"): return
    sample("stay-committed-next-actor")
    finish(true)

func physical_menu(initial: Dictionary) -> void:
    await click(state().menu.layout.commands[0].rect)
    if not check(state().menu.page == "Target", "Attack enters existing target confirmation"): return
    var target := sample("attack-selected")
    await click(nav("Confirm"))
    var accepted := sample("attack-committed")
    if not check(accepted.revision > target.revision and accepted.stage != "CommitReady", "Explicit Attack confirmation reaches session"): return
    finish(true)

func heal_commit(initial: Dictionary) -> void:
    # The reused scene witness reads its initial actor baseline from the first sample.
    samples[0].actors = initial.actors
    await click(state().menu.layout.commands[1].rect)
    await click(state().menu.layout.rowRects[0])
    var selected := state()
    if not check(resources(selected) == resources(initial), "Heal selection is provisional"): return
    var commit := nav("Confirm")
    resource_observation_started = true
    await click(commit)
    await button(center(commit),true,MOUSE_BUTTON_LEFT,true)
    await button(center(commit),false)
    if not await scene_until_control(true): return
    var done := sample("heal-resource-commit")
    var actor: Dictionary = done.actors.filter(func(a): return a.id == initial.actor)[0]
    var before: Dictionary = initial.actors.filter(func(a): return a.id == initial.actor)[0]
    if not check(actor.mp == before.mp-3 and actor.hp > before.hp, "One complete click spends one spell cost and delivers healing"): return
    if not check(changes.filter(func(e): return e.Kind == "mp").size() == 1, "Double-click tail cannot duplicate resource commit"): return
    samples.append({"label":"actual-resource-observations","changes":changes})
    finish(true)

func authored_choices() -> void:
    if not await wait_for(func(s): return s.get("wait") == "DialogueWait", "authored unread question"): return
    var unread := sample("authored-unread")
    if not check(unread.visibleCharacters >= 0 and unread.visibleCharacters < unread.totalCharacters, "Unread question reached"): return
    await click(unread.dialoguePanel.panel)
    if not check(state().token == unread.token and not 10.0 in state().flags, "Click cannot bypass unread question"): return
    await tap(KEY_ENTER)
    # Begin while there is no choice control, then attach it with a different input.
    await button(center(state().dialoguePanel.choice),true)
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.get("wait") == "ChoiceWait", "authored choices"): return
    var token: int = state().token
    await button(center(state().choicePointer.yesRect),false)
    if not check(state().token == token, "Newly attached choice rejects an earlier press/release"): return
    await frames(3)
    var question := sample("authored-question")
    if case_name == "authored-focus":
        await choice_focus(question)
        return
    if case_name == "authored-clicks":
        await click(question.choicePointer.yesRect)
        if not await wait_for(func(s): return s.get("wait") == "ChoiceWait" and s.token != question.token, "clicked Yes then second choice"): return
        var next := sample("authored-clicked-yes")
        if not check(10.0 in next.flags, "Deliberate Yes click answers Yes"): return
        await click(next.choicePointer.noRect,MOUSE_BUTTON_RIGHT)
        if not await wait_for(func(s): return s.get("canWaitAtInput",false), "right-click No return"): return
        if not check(not 11.0 in sample("authored-right-click-no").flags, "Right click retains explicit No semantics"): return
        finish(true)
        return
    var no_rect: Dictionary = question.choicePointer.noRect
    await motion(center(no_rect))
    if not check(state().choicePointer.no.begins_with("▸"), "Hover No visibly owns authored focus"): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.get("wait") == "ChoiceWait" and s.token != question.token, "second authored question"): return
    var second := sample("authored-hover-no-confirm")
    if not check(not 10.0 in second.flags, "Confirm answers the currently highlighted No"): return
    # The same physical release/double click cannot answer the next attached row.
    await button(center(second.choicePointer.yesRect),false)
    await button(center(second.choicePointer.yesRect),true,MOUSE_BUTTON_LEFT,true)
    await button(center(second.choicePointer.yesRect),false)
    if not check(state().token == second.token, "New question rejects unowned release and double click"): return
    await motion(center(second.choicePointer.noRect))
    await tap(KEY_LEFT)
    if not check(state().choicePointer.yes.begins_with("▸"), "Direction key takes focus back to Yes"): return
    await frames(6)
    if not check(state().choicePointer.yes.begins_with("▸"), "Stationary pointer cannot reclaim authored focus"): return
    await tap(KEY_ENTER)
    if not await wait_for(func(s): return s.get("canWaitAtInput",false), "authored return"): return
    var done := sample("authored-keyboard-yes-confirm")
    if not check(11.0 in done.flags and not 10.0 in done.flags, "Keyboard Confirm answers displayed Yes only"): return
    finish(true)

func private_choices() -> void:
    if not await wait_for(func(s): return s.get("canWaitAtInput",false), "private zone input"): return
    await tap(KEY_DOWN)
    if not await wait_for(func(s): return s.get("canWaitAtInput",false), "private zone approach"): return
    await tap(KEY_RIGHT)
    var deadline := Time.get_ticks_msec()+45000
    while Time.get_ticks_msec() < deadline:
        if not await focus(): return
        var s := state()
        if not check(s.failure == null, "Healthy private choice sequence"): return
        if s.wait == "ChoiceWait" and s.get("canWaitForChoice",false) and not s.choiceRequiresRelease:
            await source_answer()
            return
        if s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters: await tap(KEY_ENTER)
        elif s.canWaitForText or s.get("w1") != null and s.w1.AtInput:
            key_event(KEY_ENTER,true)
            await frames(3)
            if state().wait == "ChoiceWait":
                sample("source-entry-held-opening")
                if not await wait_for(func(x): return x.choice.Work.Phase == 2, "source held release barrier"): return
                var release := sample("source-release-barrier")
                await click(release.choicePointer.yesRect)
                if not check(state().choice.Work.Phase == 2, "Click cannot bypass source release gate"): return
            key_event(KEY_ENTER,false)
            await frames(3)
        else: await process_frame
    fail("Private question deadline")

func source_answer() -> void:
    var question := sample("source-default-yes")
    if not check(question.choice.Work.Yes, "Source default Yes retained"): return
    await motion(center(question.choicePointer.noRect))
    if not check(not state().choice.Work.Yes, "Pointer hover changes authoritative source selection"): return
    await tap(KEY_LEFT)
    if not check(state().choice.Work.Yes, "Keyboard takes source selection back"): return
    await frames(5)
    if not check(state().choice.Work.Yes, "Stationary pointer cannot steal source selection"): return
    var answer: Dictionary = state().choicePointer.yesRect if case_name == "private-yes" else state().choicePointer.noRect
    await click(answer)
    var accepted := sample("source-deliberate-answer")
    if not check(accepted.wait == "ChoiceWait" and accepted.choice.Work.Phase == 4, "Explicit clicked answer retains source Closing"): return
    await button(center(answer),false)
    if not await wait_for(func(s): return s.get("wait") != "ChoiceWait", "source closing and return"): return
    var returned := sample("source-returned")
    if not check(returned.choice == null and not returned.dialoguePanel.choicesVisible, "Source deletes choice before next program input"): return
    if not check(returned.flags.has(question.choice.ResultFlag) == (case_name == "private-yes"), "Source publishes only the explicit clicked result at its existing boundary"): return
    finish(true)

func transfer_focus() -> bool:
    var other := Window.new()
    other.hide(); other.force_native = true; other.transient = true
    other.title = "Menu pointer focus witness"; other.size = Vector2i(240,100)
    root.add_child(other)
    other.show(); other.grab_focus()
    var deadline := Time.get_ticks_msec()+1000
    while not (other.has_focus() and not root.has_focus()) and Time.get_ticks_msec() < deadline: await process_frame
    if not (other.has_focus() and not root.has_focus()):
        other.queue_free()
        unavailable.append("Native focus transfer unavailable")
        finish(true)
        return false
    sample("native-focus-exited-with-press")
    other.hide(); root.grab_focus()
    deadline = Time.get_ticks_msec()+1000
    while not root.has_focus() and Time.get_ticks_msec() < deadline: await process_frame
    other.queue_free()
    if not root.has_focus():
        unavailable.append("Native root focus return unavailable")
        finish(true)
        return false
    await frames(3)
    sample("native-focus-returned-with-press")
    return true

func battle_focus() -> void:
    var initial := state()
    var magic := center(initial.menu.layout.commands[1].rect)
    await motion(magic); await button(magic,true)
    if not await transfer_focus(): return
    await button(magic,false)
    if not check(state().menu.page == "Main" and resources(state()) == resources(initial), "Real focus loss invalidates battle press"): return
    await motion(magic); await button(magic,true)
    await motion(Vector2(960,500)); await motion(magic); await button(magic,false)
    if not check(state().menu.page == "Main", "Windowed drag out and re-entry cannot activate"): return
    await click(state().menu.layout.commands[1].rect)
    if not check(state().menu.page == "Magic", "Fresh windowed click works after focus return"): return
    var row := center(state().menu.layout.rowRects[0])
    await button(row,true); await tap(KEY_ESCAPE); await button(row,false)
    if not check(state().menu.page == "Main" and resources(state()) == resources(initial), "Layer replacement invalidates old release"): return
    sample("windowed-layer-release-isolated")
    finish(true)

func choice_focus(question: Dictionary) -> void:
    var no := center(question.choicePointer.noRect)
    await motion(no); await button(no,true)
    if not await transfer_focus(): return
    await button(no,false)
    if not check(state().token == question.token and not 10.0 in state().flags, "Real focus loss invalidates choice press"): return
    await motion(no); await button(no,true)
    await motion(Vector2(960,500)); await motion(no); await button(no,false)
    if not check(state().token == question.token, "Windowed choice drag out and re-entry cannot answer"): return
    await click(state().choicePointer.noRect)
    if not await wait_for(func(s): return s.get("wait") == "ChoiceWait" and s.token != question.token, "fresh No click after focus return"): return
    var second := sample("windowed-choice-fresh-no")
    await button(center(second.choicePointer.yesRect),false)
    if not check(state().token == second.token, "New question cannot reuse release"): return
    await click(second.choicePointer.noRect,MOUSE_BUTTON_RIGHT)
    if not await wait_for(func(s): return s.get("canWaitAtInput",false), "windowed right-click No return"): return
    if not check(not 10.0 in state().flags and not 11.0 in state().flags, "No branch answers remain No"): return
    sample("windowed-choice-return")
    finish(true)
