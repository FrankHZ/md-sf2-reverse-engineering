extends "res://probes/engine_demo_battle_ui_observation.gd"

# Serial production observation. Synthetic native events prove software routing, not physical hardware.
var hint_case := OS.get_environment("SF2_HINT_CASE")

func hints() -> Dictionary:
    return JSON.parse_string(host.call("ReadSettingsHostObservationJson")).hints

func entry_state() -> Dictionary:
    return JSON.parse_string(host.find_child("SessionEntryView",true,false).call("ReadEntryObservationJson"))

func noise_button(device := 0, pressed := true) -> void:
    var e := InputEventJoypadButton.new(); e.device = device; e.button_index = JOY_BUTTON_MISC1; e.pressed = pressed
    Input.parse_input_event(e); await frames(2)

func pad(device := 0) -> void:
    await noise_button(device,true); await noise_button(device,false)

func keyboard(echo := false) -> void:
    var e := InputEventKey.new(); e.keycode = KEY_F12; e.pressed = true; e.echo = echo
    Input.parse_input_event(e); await frames(2)
    e = InputEventKey.new(); e.keycode = KEY_F12; e.pressed = false
    Input.parse_input_event(e); await frames(2)

func motion(at: Vector2, relative: Vector2, device := 0) -> void:
    var e := InputEventMouseMotion.new(); e.position = at; e.relative = relative; e.device = device
    Input.parse_input_event(e); await frames(2)

func axis(value: float) -> void:
    var e := InputEventJoypadMotion.new(); e.device = 0; e.axis = JOY_AXIS_TRIGGER_LEFT; e.axis_value = value
    Input.parse_input_event(e); await frames(2)

func menu_axis(value: float) -> void:
    var e := InputEventJoypadMotion.new(); e.device = 0; e.axis = JOY_AXIS_RIGHT_X; e.axis_value = value
    Input.parse_input_event(e); await frames(2)

func use_device(kind: String) -> void:
    if kind == "pad": await pad()
    else: await keyboard()

func noise_case() -> bool:
    await pad()
    if not expect(hints().device == "pad" and hints().layout == "position", "Unregistered native pad uses positional labels"): return false
    if OS.get_environment("SF2_HINT_NO_MENU_PAD") == "1":
        if not expect("Keyboard" in hints().compact.Menu or "键盘" in hints().compact.Menu, "Absent implicit Menu pad binding explicitly falls back to real keyboard key"): return false
    if not expect(not "South" in str(hints()) and not "East" in str(hints()) and not "input." in str(hints()), "Player labels contain no internal face enums or missing catalog keys"): return false
    await keyboard(true)
    await motion(Vector2(100,100),Vector2.ZERO)
    await motion(Vector2(101,100),Vector2(1,0))
    await motion(Vector2(120,100),Vector2(20,0),-1)
    var echo := InputEventAction.new(); echo.action = "sf2_confirm"; echo.pressed = true
    Input.parse_input_event(echo); await frames(2); Input.action_release("sf2_confirm")
    if not expect(hints().device == "pad", "Key echo, mouse micro-motion/emulated echo and action echo cannot switch hints"): return false
    await keyboard()
    await axis(0.12); await axis(0.49)
    if not expect(hints().device == "keyboard-mouse", "Axis drift below action dead zone cannot switch hints"): return false
    await axis(0.7)
    if not expect(hints().device == "pad", "Actual deflection crosses dead zone"): return false
    await keyboard(); await axis(0.9); await axis(0.48); await axis(0.52)
    if not expect(hints().device == "keyboard-mouse", "Held axis and threshold chatter do not steal keyboard hints"): return false
    await axis(0); await axis(0.8); await axis(0)
    await noise_button(0,true); await keyboard(); await noise_button(0,true)
    if not expect(hints().device == "keyboard-mouse", "Repeated held pad event cannot steal hints"): return false
    await noise_button(0,false); await pad(1)
    if not expect(hints().pad == 1, "Another controller on the same input family replaces active device identity"): return false
    Input.joy_connection_changed.emit(1,false); await frames(2)
    if not expect(hints().device == "keyboard-mouse", "Active disconnect falls back to keyboard without selecting an idle pad"): return false
    await pad(0)
    root.gui_embed_subwindows = false
    var focus_owner := Window.new(); focus_owner.size = Vector2i(160,100); focus_owner.position = Vector2i(-18000,-18000)
    root.add_child(focus_owner); focus_owner.show(); focus_owner.grab_focus(); await frames(15)
    if not expect(not root.has_focus() and focus_owner.has_focus(), "Another owned native window receives focus: root="+str(root.has_focus())+" other="+str(focus_owner.has_focus())+" embedded="+str(focus_owner.is_embedded())): return false
    await keyboard()
    if not expect(hints().device == "pad", "Unfocused input cannot switch hint ownership"): return false
    focus_owner.hide(); focus_owner.queue_free(); root.grab_focus(); await frames(15)
    if not expect(root.has_focus() and hints().device == "pad", "Native focus restoration retains device without automatic switch"): return false
    await motion(Vector2(300,300),Vector2.ZERO)
    if not expect(hints().device == "pad", "Restored mouse position with no relative motion cannot select a device"): return false
    await motion(Vector2(320,300),Vector2(20,0))
    return expect(hints().device == "keyboard-mouse", "Subsequent obvious movement selects keyboard/mouse")

func entry_case() -> bool:
    if not await noise_case(): return false
    await act("confirm"); await act("equipment")
    for i in range(50):
        if not entry_state().dialogue.moreBelow: break
        await act("confirm")
    if not expect(entry_state().stage == "menu", "Ordinary Witch reaches real journey menu"): return false
    var before := entry_state()
    await pad()
    var after := entry_state()
    samples.append({"label":"witch-pad","entry":after,"hints":hints()})
    if not expect(after.page == before.page and after.titleSelection == before.titleSelection and after.dialogue.scroll == before.dialogue.scroll, "Device refresh preserves Witch page, reading and selection"): return false
    var down := InputEventJoypadButton.new(); down.button_index = JOY_BUTTON_DPAD_DOWN; down.pressed = true
    Input.parse_input_event(down); await frames(2)
    Input.parse_input_event(down.duplicate()); await frames(2)
    if not expect(entry_state().titleSelection == 1, "One held pad edge moves focus exactly once"): return false
    down = down.duplicate(); down.pressed = false; Input.parse_input_event(down); await frames(2)
    route = "pad"; events.clear(); bind_events(); await act("up"); await act("confirm")
    var deadline := Time.get_ticks_msec()+15000
    while host.find_child("SaveSlotView",true,false) == null and Time.get_ticks_msec()<deadline: await process_frame
    var slots := host.find_child("SaveSlotView",true,false)
    if not expect(slots != null, "Real New Game admission opens disposable slots"): return false
    while "loading" in str(JSON.parse_string(slots.call("ReadSaveObservationJson"))).to_lower() and Time.get_ticks_msec()<deadline: await process_frame
    var slot_before: Dictionary = JSON.parse_string(slots.call("ReadSaveObservationJson"))
    await keyboard()
    if not expect(JSON.parse_string(slots.call("ReadSaveObservationJson")).selected == slot_before.selected, "Slots update without changing selected destination"): return false
    samples.append({"label":"slots-keyboard","hint":slots.find_child("SlotHint",true,false).text,"hints":hints()})
    route = "keyboard"; events.clear(); bind_events(); await act("confirm")
    var name := host.find_child("NameEntryView",true,false)
    if not expect(name != null, "Ordinary slot choice reaches physical-keyboard naming"): return false
    var original: Dictionary = JSON.parse_string(name.call("ReadNameObservationJson"))
    await pad()
    var current: Dictionary = JSON.parse_string(name.call("ReadNameObservationJson"))
    if not expect(current.inputMode == "keyboard" and current.keyboard and current.draft == original.draft, "Pad hint change cannot edit name or steal editor focus"): return false
    samples.append({"label":"physical-keyboard-name","name":current})
    await tap(KEY_ESCAPE)
    return expect(host.find_child("NameEntryView",true,false) == null, "Physical Escape returns freely without publishing a save")

func field_case() -> bool:
    var initial := state()
    await act("menu")
    await menu_axis(0.7) # Same mapped Menu action closes it.
    await menu_axis(0.49); await menu_axis(0.52)
    if not expect(host.find_child("SystemMenuView",true,false) == null, "One deflection with dead-zone chatter cannot execute Menu twice"): return false
    await menu_axis(0); await menu_axis(0.7); await menu_axis(0)
    var menu := host.find_child("SystemMenuView",true,false)
    if not expect(menu != null, "Clear neutral rearms mapped Menu exactly once"): return false
    await pad()
    var pad_hint: String = menu.find_child("SystemMenuHint",true,false).text
    await keyboard()
    var key_hint: String = menu.find_child("SystemMenuHint",true,false).text
    if not expect(pad_hint != key_hint and hints().compact.Confirm in key_hint, "Mounted system footer immediately uses active installed binding"): return false
    # Real Settings and Help, then real swap Apply. No binding capture or product API is added.
    await act("down"); await act("down"); await act("confirm")
    var settings := host.find_child("SettingsView",true,false)
    if not expect(settings != null, "Legal field Settings opens"): return false
    for i in range(8): await act("down")
    await act("confirm")
    var help := host.find_child("ControlsHelpView",true,false)
    if not expect(help != null, "Existing complete on-demand Help remains accessible"): return false
    await act("down")
    var help_before: Dictionary = JSON.parse_string(help.call("ReadControlsHelpObservationJson"))
    await pad()
    var help_after: Dictionary = JSON.parse_string(help.call("ReadControlsHelpObservationJson"))
    if not expect(help_after.page == help_before.page and help_after.scroll == help_before.scroll and help_after.hint != help_before.hint, "Help changes navigation cue without resetting reading page or scroll"): return false
    if not expect(" / " in help_after.body, "Full Help retains alternative bindings for both devices"): return false
    samples.append({"label":"help-active-pad","help":help_after})
    await act("cancel")
    for i in range(6): await act("up")
    await act("confirm") # Confirm/Cancel swap draft
    for i in range(4): await act("down")
    var old_confirm: InputEvent = events.confirm.duplicate()
    old_confirm.pressed = true; Input.parse_input_event(old_confirm); await frames(2)
    if not expect(JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied.ConfirmCancel == "swapped", "Ordinary Settings Apply publishes actual swapped mappings"): return false
    old_confirm.pressed = false; Input.parse_input_event(old_confirm); await frames(4)
    events.clear(); bind_events()
    if not expect(host.find_child("SettingsView",true,false) == null and menu.is_visible_in_tree(), "Apply's old confirmation cannot also activate the returned menu"): return false
    await pad(); await keyboard()
    if not expect(hints().compact.Confirm == OS.get_keycode_string(events.confirm.keycode), "Hint follows actual remapped/swapped InputMap after Apply"): return false
    samples.append({"label":"system-applied","hint":menu.find_child("SystemMenuHint",true,false).text,"hints":hints()})
    await act("cancel")
    if not expect(state().map == initial.map and state().party == initial.party and state().gold == initial.gold and state().flags == initial.flags and state().entities[0].x == initial.entities[0].x and state().entities[0].y == initial.entities[0].y, "Modal/Help/remapping round trip leaves gameplay unchanged"): return false
    if hint_case == "menu": return true
    for action in ["down","right"]:
        if not await wait_for(func(s):return s.canWaitAtInput,"legal source approach"): return false
        await act(action)
    var deadline := Time.get_ticks_msec()+45000
    while Time.get_ticks_msec()<deadline:
        var s := state()
        if s.wait == "ChoiceWait" and s.canWaitForChoice and not s.choiceRequiresRelease: break
        if s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters or s.dialoguePanel.moreBelow or s.canWaitForText or s.get("w1") != null and s.w1.AtInput:
            var before := s
            await pad()
            if not expect(state().token == before.token and state().dialoguePanel.scroll == before.dialoguePanel.scroll, "Story device switch cannot acknowledge or reset reading"): return false
            samples.append({"label":"story-pad","panel":state().dialoguePanel,"hints":hints()})
            await act("confirm")
        else: await process_frame
    var question := state()
    samples.append({"label":"source-question-boundary","state":question})
    if not expect(question.wait == "ChoiceWait" and question.canWaitForChoice, "Real source question reaches legal choice"): return false
    await pad(); await keyboard()
    if not expect(state().choice == question.choice and state().dialoguePanel.scroll == question.dialoguePanel.scroll, "Choice and reading position survive pure device switch"): return false
    if not expect(hints().compact.Cancel in state().dialoguePanel.hint and ("No" in state().dialoguePanel.hint or "否" in state().dialoguePanel.hint), "Choice explicitly names effective Cancel=No"): return false
    samples.append({"label":"choice","panel":state().dialoguePanel,"hints":hints()})
    await act("cancel")
    return expect(not state().flags.has(question.choice.ResultFlag), "Mapped Cancel still answers No without a second confirmation")

func recovery_case() -> bool:
    var before := state()
    if OS.get_environment("SF2_HINT_NO_MENU_PAD") == "1":
        await pad(); await keyboard()
        var entry := host.find_child("SystemMenuEntry",true,false)
        if not expect("\n" in entry.tooltip_text and hints().compact.Menu in entry.tooltip_text, "Host entry retains pad-unavailable explanation after active-device refresh"): return false
    root.size = Vector2i(1280,720); await frames(8)
    var recovery := host.find_child("WindowRecoveryView",true,false)
    if not expect(recovery != null, "Actual undersized client mounts existing recovery"): return false
    await pad()
    var pad_text: String = recovery.find_child("RecoveryHint",true,false).text
    await keyboard()
    if not expect(recovery.find_child("RecoveryHint",true,false).text != pad_text, "Mounted recovery footer refreshes without selecting recovery actions"): return false
    await act("cancel")
    var quit_panel := host.find_child("WindowQuitView",true,false)
    if not expect(quit_panel != null, "Recovery Quit opens its actual confirmation"): return false
    var original: Dictionary = JSON.parse_string(quit_panel.call("ReadSaveObservationJson"))
    await pad()
    pad_text = quit_panel.find_child("SlotHint",true,false).text
    await keyboard()
    var current: Dictionary = JSON.parse_string(quit_panel.call("ReadSaveObservationJson"))
    if not expect(current.selected == 0 and current.message == original.message and quit_panel.find_child("SlotHint",true,false).text != pad_text,
            "Recovery Quit cache refreshes while retaining safe Cancel and unsaved warning"): return false
    samples.append({"label":"recovery-quit","confirmation":current,"hints":hints()})
    await act("cancel")
    root.size = Vector2i(1920,1080); await frames(8)
    return expect(host.find_child("WindowRecoveryView",true,false) == null and state().map == before.map and state().party == before.party,
        "Recovery Cancel and usable-size return preserve the field caller")

func battle_case() -> bool:
    if not await enter_battle(): return false
    await wait_for(func(_s):return board().settled,"initial board")
    await act("confirm")
    var before := state()
    var baseline := resources(before)
    await pad()
    var menu: Dictionary = state().menu
    if not expect(menu.page == before.menu.page and menu.command == before.menu.command and resources(state()) == baseline, "Pad switch preserves live command, resources and turn"): return false
    if not expect(hints().compact.Confirm in menu.layout.help and not "South" in menu.layout.help, "Production battle command cache uses active pad labels"): return false
    for row in menu.layout.descriptionLabels:
        if not expect(inside(row.rect), "Actual pad main footer remains inside viewport"): return false
    route = "pad"; events.clear(); bind_events(); await act("attack")
    if not expect(state().menu.page == "Target", "Real pad chooses Attack once"): return false
    await wait_for(func(_s):return board().settled,"target board")
    var chosen: String = state().candidate
    var actor: Dictionary = state().actors.filter(func(a):return a.id == chosen)[0]
    var point := cell_point(actor.x,actor.y)
    await motion(point,Vector2(8,0))
    if not expect(hints().device == "keyboard-mouse" and state().scene.phase == null and resources(state()) == baseline, "Obvious mouse aim changes prompt and only previews target"): return false
    if not expect(hints().compact.Confirm in state().menu.layout.help, "Battle target footer refreshes with installed keyboard Confirm"): return false
    samples.append({"label":"target-mouse","menu":state().menu,"hints":hints()})
    for row in state().menu.layout.descriptionLabels:
        if not expect(inside(row.rect),"Active target description/hint fits viewport"): return false
    await mouse(point,true)
    if not expect(state().scene.phase == null, "Pointer press alone cannot execute"): return false
    await mouse(point,false)
    return expect(state().scene.phase != null and not state().menu.visible, "One complete legal target click executes exactly once through original path")

func run() -> void:
    root.size = Vector2i(1920,1080); root.position = Vector2i(-20000,-20000)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(8)
    if not expect(root.has_focus(),"Owned native window has actual focus"): return
    route = "keyboard"; bind_events()
    events.equipment = InputMap.action_get_events("sf2_equipment").filter(func(e):return e is InputEventKey)[0]
    samples.append({"label":"native-device-boundary","connected":Input.get_connected_joypads(),"hints":hints()})
    if hint_case == "entry": finish(await entry_case()); return
    if not expect(host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COMPACT_START")) == "started","Registered full-world prelaunch input admits"): return
    if hint_case == "recovery": finish(await recovery_case()); return
    finish(await field_case() if hint_case in ["field","menu"] else await battle_case())
