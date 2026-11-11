extends "res://probes/localization_observation.gd"

# Production F1, not DesignPreview. Use only an explicitly disposable save namespace.
var entry_view: Node
var output := OS.get_environment("SF2_ENTRY_UI_OUTPUT")
var checks := 0
var pages := 0
var story_pages := 0

func check(ok: bool, message: String) -> void:
    checks += 1
    super.check(ok, message)

func frames(count := 4) -> void:
    for i in range(count): await process_frame

func rect(control: Control) -> Dictionary:
    var r := control.get_global_rect()
    return {"x":r.position.x,"y":r.position.y,"width":r.size.x,"height":r.size.y}

func contained(inner: Rect2, outer: Rect2) -> bool:
    return outer.grow(1).encloses(inner)

func capture(label: String) -> void:
    var controls: Array = []
    for node in host.find_children("*", "Control", true, false):
        if node.is_visible_in_tree() and (node is Label or node is Button or node is LineEdit):
            controls.append({"name":str(node.name),"text":node.text,"rect":rect(node),"fontSize":node.get_theme_font_size("font_size")})
    samples.append({"label":label,"controls":controls,"entry":entry_state(),"slots":slot_state()})

func entry_state() -> Dictionary:
    return JSON.parse_string(entry_view.call("ReadEntryObservationJson")) if is_instance_valid(entry_view) else {}

func slot_state() -> Dictionary:
    var slots := host.find_child("SaveSlotView",true,false)
    return JSON.parse_string(slots.call("ReadSaveObservationJson")) if slots else {}

func wait_slots(expected: String) -> bool:
    var deadline := Time.get_ticks_msec()+15000
    while Time.get_ticks_msec() < deadline:
        if slot_state().get("state") == expected: await frames(); return true
        await process_frame
    check(false,"slot deadline: "+expected)
    capture("slot-deadline")
    return false

func square(button: Button) -> void:
    var style := button.get_theme_stylebox("normal") as StyleBoxFlat
    check(style != null and style.corner_radius_top_left == 0 and style.corner_radius_bottom_right == 0,"square production action: "+str(button.name))

func form_geometry(name: String) -> void:
    var panel := host.find_child(name,true,false) as Control
    var region := Rect2(1216,64,640,952)
    check(panel != null and contained(panel.get_global_rect(),region),"entry form stays in right region: "+name)
    if panel == null: return
    check(not panel.get_global_rect().intersects(Rect2(160,92,1024,896)),"form does not cover Witch: "+name)
    for button in panel.find_children("*","Button",true,false):
        if button.is_visible_in_tree():
            square(button)
            check(button.size.y >= 64 and button.get_theme_font_size("font_size") == 30,"readable action height and font")
            check(button.size.x <= 612,"wrapped action width inside column")

func joy(button: JoyButton) -> void:
    var e := InputEventJoypadButton.new()
    e.button_index=button; e.pressed=true; Input.parse_input_event(e)
    await frames(2)
    e=InputEventJoypadButton.new(); e.button_index=button; e.pressed=false; Input.parse_input_event(e)
    await frames()

func click(button: Control) -> void:
    # Input.parse_input_event receives containing-window coordinates; entry's canvas is scaled.
    var at := root.get_stretch_transform() * button.get_global_transform_with_canvas() * (button.size / 2)
    var motion := InputEventMouseMotion.new(); motion.position=at; motion.global_position=at; motion.relative=Vector2(3,3)
    Input.parse_input_event(motion); await frames(2)
    for down in [true,false]:
        var e := InputEventMouseButton.new(); e.position=at; e.global_position=at
        e.button_index=MOUSE_BUTTON_LEFT; e.pressed=down; Input.parse_input_event(e)
        await frames(2)

func type_name(text: String) -> void:
    var select := InputEventKey.new(); select.keycode=KEY_A; select.ctrl_pressed=true; select.pressed=true
    Input.parse_input_event(select); await frames(2)
    select=InputEventKey.new(); select.keycode=KEY_A; select.ctrl_pressed=true; select.pressed=false
    Input.parse_input_event(select); await press(KEY_BACKSPACE)
    for character in text:
        var e := InputEventKey.new(); e.unicode=character.unicode_at(0); e.pressed=true
        Input.parse_input_event(e); await frames(2)

func _run() -> void:
    var save_namespace := OS.get_environment("SF2_SAVE_NAMESPACE")
    if output == "" or not save_namespace.begins_with("issue1606-"):
        push_error("Select SF2_ENTRY_UI_OUTPUT and an issue1606-* disposable namespace.")
        quit(2); return
    root.size=Vector2i(2560,1440) if OS.get_environment("SF2_ENTRY_UI_LARGE")=="1" else Vector2i(1920,1080)
    if OS.get_environment("SF2_ENTRY_UI_LABELS") == "1":
        # A display-only input variant, before GameText admission; never edit the PO or save data.
        var catalog := load("res://localization/zh_CN.po") as Translation
        catalog.add_message("entry.new", "开始一段新的冒险旅程，长项仍然完整显示。".repeat(5))
        catalog.add_message("slot.record", "槽位 {0} · {1} · {2} · {3}\n"+"这是一段用于检查完整阅读与滚动边界的长槽位说明。".repeat(20))
        catalog.add_message("name.help", "仅支持键盘输入，最多七个字符。保留草稿，按回车确认，按返回键返回。".repeat(30))
    host=load("res://Main.tscn").instantiate(); root.add_child(host)
    await frames()
    if OS.get_environment("SF2_ENTRY_UI_CHOICE") != "":
        await story_choice()
        finish(); return
    entry_view=host.find_child("SessionEntryView",true,false)
    check(entry_view != null,"ordinary title entry mounted")
    if entry_view == null: finish(); return
    await press(KEY_ENTER); await press(KEY_E)
    for i in range(80):
        var more: bool = entry_state().get("dialogue",{}).get("moreBelow",false)
        if not more: break
        pages += 1
        check(not entry_view.find_child("NewGame",true,false).is_visible_in_tree(),"long greeting owns actions until read")
        await press(KEY_ENTER)
    await frames()
    capture("witch-menu")
    var entry := entry_state()
    var greeting: Dictionary = entry.dialogue.panel
    check(greeting.x >= 1216 and greeting.y+greeting.height <= 504,"greeting remains separate from Witch and actions")
    check(entry.artRect == {"x":160.0,"y":92.0,"width":1024.0,"height":896.0},"full original Witch rectangle retained")
    var start := entry_view.find_child("NewGame",true,false) as Button
    square(start)
    check(start.find_child("FocusMarker",true,false).visible,"one selected marker")
    var hints := entry_view.find_child("EntryHint",true,false) as Label
    check(contained(hints.get_global_rect(),Rect2(1216,64,640,952)) and hints.get_theme_font_size("font_size") == 24,"fixed entry hints fit without shrinking")
    await joy(JOY_BUTTON_DPAD_DOWN)
    check(entry_state().titleSelection == 1,"mapped pad moves entry focus")
    await joy(JOY_BUTTON_DPAD_UP)
    if OS.get_environment("SF2_ENTRY_UI_RECOVER") == "1":
        await recover()
        finish(); return
    await click(start)
    if OS.get_environment("SF2_ENTRY_UI_ERROR") == "1":
        if await wait_slots("new-game-error"):
            form_geometry("SlotWindow"); capture("storage-error")
            check(slot_state().message.contains("save-root"),"storage configuration refusal remains explicit")
            await press(KEY_ESCAPE)
            check(host.find_child("SaveSlotView",true,false) == null,"storage refusal retains safe Cancel")
        finish(); return
    if not await wait_slots("new-game-slots"): finish(); return
    form_geometry("SlotWindow"); capture("new-slots")
    if OS.get_environment("SF2_ENTRY_UI_LABELS") == "1":
        var reading := host.find_child("SlotReading",true,false) as ScrollContainer
        check(reading.get_v_scroll_bar().max_value > reading.size.y,"long occupied slot uses internal scrolling")
    await press(KEY_ENTER); await frames()
    form_geometry("NameWindow"); capture("name")
    if OS.get_environment("SF2_ENTRY_UI_LABELS") == "1":
        var reading := host.find_child("NameReading",true,false) as ScrollContainer
        var hint := host.find_child("NameHint",true,false) as Control
        check(reading.get_v_scroll_bar().max_value > reading.size.y and contained(hint.get_global_rect(),Rect2(1216,64,640,952)),"long name help scrolls above fixed actions and hints")
    await type_name("ABCDEFGH")
    var editor := host.find_child("NameEntryView",true,false)
    check(not JSON.parse_string(editor.call("ReadNameObservationJson")).valid,"eight characters rejected")
    await press(KEY_ENTER)
    check(host.find_child("NameEntryView",true,false) != null,"invalid Enter retains name draft")
    await type_name("鲍伊")
    check(not JSON.parse_string(editor.call("ReadNameObservationJson")).valid,"unsupported alphabet rejected")
    await type_name("Ada")
    check(JSON.parse_string(editor.call("ReadNameObservationJson")).valid,"admitted name accepted")
    await press(KEY_ESCAPE)
    if not await wait_slots("new-game-slots"): finish(); return
    await press(KEY_ENTER); await frames()
    editor=host.find_child("NameEntryView",true,false)
    check(JSON.parse_string(editor.call("ReadNameObservationJson")).draft == "Ada","Back retains name draft")
    await press(KEY_ENTER)
    if not await wait_slots("confirm-new-game"): finish(); return
    check(slot_state().selected == 0,"final Start retains safe Back default")
    form_geometry("SlotWindow"); capture("final-confirmation")
    var save_path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-opening/slot1.json")
    var prior_save := FileAccess.get_file_as_string(save_path) if FileAccess.file_exists(save_path) else ""
    await press(KEY_ENTER); await frames()
    check(host.find_child("NameEntryView",true,false) != null,"confirmation Back returns editor")
    check((FileAccess.get_file_as_string(save_path) if FileAccess.file_exists(save_path) else "") == prior_save,"confirmation Back leaves destination unchanged")
    await press(KEY_ENTER); await frames(); await press(KEY_DOWN); await press(KEY_ENTER)
    for i in range(2400):
        view=host.get_node_or_null("ExplorationSessionView")
        if view: break
        if slot_state().get("state") == "new-game-error": break
        await process_frame
    check(view != null,"real final Start publishes initial save and mounts field")
    if view == null: capture("start-failure"); finish(); return
    check(JSON.parse_string(host.call("ReadPersistenceObservationJson")).names != null,"named session mounted")
    sample("field")
    finish()

func recover() -> void:
    await press(KEY_DOWN); await press(KEY_ENTER)
    if not await wait_slots("continue-slots"): return
    capture("continue-damaged-primary"); form_geometry("SlotWindow")
    await press(KEY_ENTER)
    if not await wait_slots("recover-backup"): return
    capture("recover-backup"); form_geometry("SlotWindow")
    check(slot_state().selected == 0,"backup confirmation defaults to Cancel")
    await press(KEY_ESCAPE)
    check(host.find_child("SaveSlotView",true,false) == null,"backup Cancel restores entry")
    await press(KEY_DOWN); await press(KEY_ENTER)
    if not await wait_slots("continue-slots"): return
    await press(KEY_ENTER)
    if not await wait_slots("recover-backup"): return
    await press(KEY_DOWN); await press(KEY_ENTER)
    for i in range(2400):
        view=host.get_node_or_null("ExplorationSessionView")
        if view: break
        await process_frame
    check(view != null,"explicit backup recovery restores real saved field")
    if view: sample("recovered-field")

func story_choice() -> void:
    check(host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_ENTRY_UI_START")) == "started","existing controlled presenter admits source approach")
    root.position=Vector2i(-20000,-20000)
    root.grab_focus(); await frames(8)
    check(root.has_focus(),"native source observer owns focus")
    for i in range(2400):
        view=host.get_node_or_null("ExplorationSessionView")
        if view: break
        await process_frame
    check(view != null,"actual controlled source field mounted")
    if view == null:
        capture("source-start-failure"); return
    for key in [KEY_DOWN,KEY_RIGHT]:
        for i in range(1200):
            if state().canWaitAtInput: break
            await process_frame
        await press(key)
    var deadline := Time.get_ticks_msec()+45000
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.failure != null: check(false,"healthy source choice approach"); return
        if s.wait == "ChoiceWait" and s.get("canWaitForChoice",false) and not s.choiceRequiresRelease: break
        if s.dialoguePanel.moreBelow: story_pages += 1
        if s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters or s.dialoguePanel.moreBelow or s.canWaitForText or s.get("w1") != null and s.w1.AtInput:
            await press(KEY_ENTER)
        else: await process_frame
    var question := sample("source-question")
    check(question.displayLocale == ("zh-CN" if OS.get_environment("SF2_ENTRY_UI_ZH") == "1" else "en"),"requested locale reaches actual question without silent English fallback")
    check(question.wait == "ChoiceWait" and question.canWaitForChoice,"actual source question is selectable")
    if question.wait != "ChoiceWait": return
    var p: Dictionary = question.dialoguePanel
    check(p.choice.height >= 64 and p.choice.y+p.choice.height <= p.hintRect.y,"attached choice and hint are disjoint")
    check(p.body.y+p.body.height <= p.choice.y,"reading area leaves actual choice space")
    check(p.panel.x >= 0 and p.panel.y >= 0 and p.panel.x+p.panel.width <= root.size.x and p.panel.y+p.panel.height <= root.size.y,"source dialogue fits viewport")
    check(p.fontSize == 30,"source dialogue retains body size")
    var row := view.find_child("Choice",true,false)
    var no := row.find_child("No",true,false) as Label
    var yes := row.find_child("Yes",true,false) as Label
    check(yes.get_theme_stylebox("normal") is StyleBoxFlat,"real choice opts into square action surface")
    var no_rect := no.get_global_rect()
    await press(KEY_RIGHT)
    check(not state().choice.Work.Yes,"Right selects actual No")
    check(no_rect == no.get_global_rect(),"choice bounds remain fixed when focus changes")
    await press(KEY_LEFT)
    check(state().choice.Work.Yes,"Left restores actual Yes")
    var choose_yes := OS.get_environment("SF2_ENTRY_UI_CHOICE") == "yes"
    if choose_yes: await click(yes)
    else: await press(KEY_ESCAPE)
    for i in range(1200):
        if state().wait != "ChoiceWait": break
        await process_frame
    var done := sample("source-answer-return")
    check(done.wait != "ChoiceWait" and not done.dialoguePanel.choicesVisible,"actual choice closes before continuation")
    check(done.flags.has(question.choice.ResultFlag) == choose_yes,"only the chosen answer publishes its result flag")

func finish() -> void:
    var file := FileAccess.open(output,FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"checks":checks,"nativeSize":[root.size.x,root.size.y],"entryPages":pages,"storyPages":story_pages,"failures":failures,"samples":samples},"  "))
    file.close()
    quit(0 if failures.is_empty() else 1)
