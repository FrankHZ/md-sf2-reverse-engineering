extends "res://probes/entry_ui_observation.gd"

# Production input only. Private cases select a controlled starting point before play;
# give/drop use their existing authored encounters. No save publication or screen capture.
var mode := OS.get_environment("SF2_COPY_CASE")
var chinese := OS.get_environment("SF2_COPY_LANG") == "zh-CN"

func control(id: String) -> Control:
    return host.find_child(id, true, false) as Control

func observe(label: String, value) -> void:
    samples.append({"label":label,"value":value})

func _run() -> void:
    output = OS.get_environment("SF2_COPY_OUTPUT")
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1789-"):
        push_error("Select a fresh copy report and a disposable issue1789-* slot namespace.")
        quit(2); return
    root.size = Vector2i(1920,1080)
    if mode == "entry-long":
        var catalog := load("res://localization/zh_CN.po") as Translation
        catalog.add_message("new.slots", "确认前不会更改存档。".repeat(140))
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(8)
    if mode.begins_with("entry"):
        entry_view = control("SessionEntryView")
        await entry_copy()
    elif mode in ["story","story-debug"]:
        view = control("BattleSessionView")
        if view: await battle_story_copy()
        else: check(false,"Authored battle did not mount")
    elif mode == "window":
        var mounted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COPY_START"))
        if mounted != "started": check(false,"Expected field session for window recovery")
        view = control("ExplorationSessionView")
        await frames(8)
        root.size=Vector2i(1280,720); await frames()
        var label := control("RecoveryMessage") as Label
        var action := control("Recovery0") as Button
        check(label != null and label.is_visible_in_tree() and "1280" in label.text and "720" in label.text,"Recovery explains current client size")
        check(action != null and action.text == ("恢复窗口大小" if chinese else "Restore window size"),"Recovery action is independent of the supported minimum")
        if label: observe("recovery",label.text)
        root.size=Vector2i(1920,1080); await frames()
        check(control("WindowRecoveryView")==null and state().canWaitAtInput,"Supported client restores field control")
    elif mode in ["give","exchange","discard"]:
        view = control("BattleSessionView")
        if view: await transaction_copy()
        else: check(false,"Authored battle did not mount")
    elif mode in ["field","field-fallback","field-debug","shop","church"]:
        var admitted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COPY_START"))
        check(admitted == "started","Controlled production field admission: "+str(admitted))
        view = control("ExplorationSessionView")
        if view:
            if await ready_field():
                if mode.begins_with("field"): await field_copy()
                else: await service_copy()
    else: check(false,"Unknown copy case")
    var file := FileAccess.open(output,FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"checks":checks,"failures":failures,"samples":samples},"  ")); file.close()
    host.queue_free(); await frames(6)
    quit(0 if failures.is_empty() else 1)

func entry_copy() -> void:
    check(entry_state().stage == "title","Original title opens")
    await press(KEY_ENTER); await frames()
    var hint := control("EntryHint") as Label
    var greeting := entry_state()
    check(greeting.stage == "dialogue" and not greeting.message.is_empty(),"Witch greeting is readable")
    observe("greeting footer",hint.text)
    if mode == "entry-debug":
        check(greeting.displayFallback != null and hint.is_visible_in_tree() and str(greeting.displayFallback) in hint.text,"Diagnostics retain actual fallback reason"); return
    check(not hint.is_visible_in_tree() and not greeting.dialogue.hint.is_empty(),"Only the reading controls accompany the greeting")
    if mode == "entry-fallback":
        check(greeting.displayLocale == "en" and greeting.displayFallback != null,"English story remains readable with recorded reason"); return
    await press(KEY_E); await frames()
    check(hint.text == ("普通难度" if chinese else "Normal difficulty") and hint.is_visible_in_tree(),"New Game shows its difficulty")
    await press(KEY_DOWN); await frames()
    check(not hint.is_visible_in_tree(),"Continue does not advertise New Game difficulty")
    await press(KEY_UP); await press(KEY_ENTER)
    if not await wait_slots("new-game-slots"): return
    var slot_hint := control("SlotHint") as Label
    observe("new slots",slot_state().message)
    check(("确认前" if chinese else "until you confirm") in slot_state().message,"Slot choice preserves no-write boundary")
    if mode == "entry-long":
        var reading := control("SlotReading") as ScrollContainer
        check(reading.get_v_scroll_bar().visible and "Left" in slot_hint.text,"Overflow advertises mapped reading")
        await press(KEY_RIGHT)
        check(reading.scroll_vertical > 0,"Advertised reading actually scrolls")
        await press(KEY_ESCAPE)
        check(control("NewGame").is_visible_in_tree(),"Long slot page cancels to Witch")
        return
    check(not control("SlotReading").get_v_scroll_bar().visible and not ("Left" in slot_hint.text),"Short slot page has no scroll instruction")
    await press(KEY_ENTER); await frames()
    var edit := control("NameInput") as LineEdit
    check(edit.placeholder_text.is_empty() and not control("NameError").text.is_empty(),"Empty draft shows the actual default name once")
    await type_name("ABCDEFGH"); await press(KEY_ENTER)
    check(control("NameEntryView") != null and "7" in control("NameError").text,"Invalid name retains editor and seven-character limit")
    await type_name("Ada"); await press(KEY_ENTER)
    if not await wait_slots("confirm-new-game"): return
    var confirmation := slot_state()
    observe("final confirmation",confirmation.message)
    check(confirmation.selected == 0 and "Ada" in confirmation.message and "1" in confirmation.message and
        ("备份" if chinese else "backup") in confirmation.message and ("未保存" if chinese else "Unsaved") in confirmation.message,"Initial save names slot, backup and lost progress; Back is selected")
    await press(KEY_ENTER); await frames()
    check(control("NameInput").text == "Ada","Back retains the name without writing")

func ready_field() -> bool:
    for i in range(480):
        var s := state()
        if s.get("canWaitAtInput",false): return true
        if s.failure != null: break
        if s.get("canWaitForText",false) or s.get("wait") in ["FieldTextWait","DialogueWait","W1TextWait"]: await press(KEY_ENTER)
        else: await frames(1)
    check(false,"Expected field control"); return false

func field_copy() -> void:
    await press(KEY_ESCAPE); await frames()
    var opened := state()
    check(opened.fieldMenu.visible,"Ordinary Cancel opens Adventure")
    for i in 4:
        var s := state()
        var menu: Dictionary = s.fieldMenu.menu
        observe("field "+str(i),[menu.reason,menu.hint])
        var expected := ["队员","施法","道具","面前"] if chinese else ["stats","cast","item","in front"]
        check(s.fieldMenu.index == i and expected[i] in menu.reason and not "\n" in menu.hint,
            "Focused row has a short purpose/refusal and one mapped control line")
        if i == 0:
            await press(KEY_ENTER)
            check(state().partyStatus.visible,"Party opens actual member page")
            await press(KEY_ESCAPE)
        if i < 3: await press(KEY_DOWN)
    await press(KEY_ENTER); await frames()
    check(state().wait == "FieldSearchWait" and not state().dialogue.is_empty(),"Search reaches its actual result")
    if mode in ["field-fallback","field-debug"]:
        var result := state()
        observe("search fallback",[result.displayFallback,result.hint])
        check(result.displayFallback != null and (str(result.displayFallback) in result.hint) == (mode=="field-debug"),"Search preserves English body and separates the diagnostic from reading")
    await press(KEY_ESCAPE)
    if not await ready_field(): return
    await press(KEY_F10); await frames()
    var menu_hint := control("SystemMenuHint") as Label
    check(menu_hint != null and not ("Left" in menu_hint.text),"Short system menu has no scroll instruction")
    await press(KEY_ESCAPE); await frames()
    check(control("SystemMenuView") == null and state().canWaitAtInput,"Menu Back returns control")

func service_copy() -> void:
    await press(KEY_ENTER)
    for i in range(480):
        var s := state()
        if s[mode].visible: break
        if s.get("canWaitForText",false) or s.get("wait") in ["FieldTextWait","DialogueWait","W1TextWait"]: await press(KEY_ENTER)
        else: await frames(1)
    var s := state()
    if not s[mode].visible: check(false, "Expected "+mode+"; reached "+str(s.wait)); return
    var before := [s.revision,s.gold,s.party]
    await press(KEY_DOWN); await press(KEY_DOWN)
    s = state()[mode]
    var row: Dictionary = s.commandList.rows[2] if mode=="church" else s.services.list.rows[2]
    var focus: String = s.hint if mode=="church" else s.services.reason
    observe("focused service",[row.label,focus])
    check(not row.enabled and ("不可用" in row.label or "未开放" in row.label if chinese else "unavailable" in row.label.to_lower()),"Focused service already states its limit")
    await press(KEY_ENTER); s=state()
    var why = s[mode].error
    observe("after Confirm",why)
    check(why != null and not str(why) in focus and [s.revision,s.gold,s.party] == before,"Confirm explains refusal without payment or progression")
    if mode=="church":
        await press(KEY_DOWN)
        var cue: String = state().church.hint
        observe("Record",cue)
        check(("回到地图" if chinese else "returns you to the field") in cue,"Record names its return")
        await press(KEY_ENTER)
        for i in range(1000):
            if control("SaveSlotView") and slot_state().get("state") != "entry-loading": break
            if state().get("canWaitForText",false): await press(KEY_ENTER)
            else: await frames(1)
        check(slot_state().get("state") in ["save-slots","error"],"Record reached real save host")
    await press(KEY_ESCAPE)
    check(await ready_field(),"Back restores field control")

func battle_ready() -> bool:
    for i in range(500):
        var s := state()
        if s.stage == "Movement" and s.stopReason == "PlayerInput" and s.movement == null: return true
        await frames(1)
    check(false,"Expected battle Movement"); return false

func transaction_copy() -> void:
    if not await battle_ready(): return
    await press(KEY_DOWN)
    if not await battle_ready(): return
    if mode != "discard":
        await press(KEY_RIGHT)
        if not await battle_ready(): return
    await press(KEY_ENTER)
    var original := state()
    await press(KEY_R if mode=="discard" else KEY_G); await press(KEY_ENTER)
    if mode!="discard": await press(KEY_TAB); await press(KEY_ENTER)
    if mode=="exchange":
        check(state().stage=="GiveExchangeChoice","Full pack requires a return item")
        await press(KEY_ENTER)
    var s := state()
    var body: String = s.discardMenu if mode=="discard" else s.giveMenu
    observe(mode+" confirmation",body)
    check(("本次行动结束" if chinese else "ends this action") in body if mode!="discard" else
        ("仍可行动" if chinese else "still move or choose another action") in body,"Confirmation keeps the actual turn consequence")
    if mode=="exchange": check(("交换" if chinese else "Trade") in body,"Quote identifies an exchange")
    if mode=="discard":
        await press(KEY_ENTER)
        var after := state()
        check(after.discard.active and not after.discard.text.is_empty() and after.inventories != original.inventories,"Discard produces the real item result")
        observe("result",after.discard.text)
        return
    await press(KEY_ESCAPE)
    check(state().inventories == original.inventories,"Back does not transfer or discard")

func battle_story_copy() -> void:
    if not await battle_ready(): return
    for i in 2:
        await press(KEY_UP)
        if not await battle_ready(): return
    await press(KEY_ENTER); await press(KEY_SPACE); await press(KEY_ENTER)
    var tokens := []
    for i in 1600:
        var s := state()
        if s.story.kind == "DialogueWait" and s.story.presented:
            if s.story.token not in tokens:
                tokens.append(s.story.token)
                observe("battle story",s.story.panel.hint)
                check(not s.story.text.is_empty() and ("unmigrated-consumer" in s.story.panel.hint) == (mode=="story-debug"),"English story keeps its reading cue; debug alone shows reason")
            await press(KEY_ENTER)
        elif tokens.size() == 2 and s.stage == "Movement": return
        else: await frames(1)
    check(false,"Expected two story acknowledgements and return to movement")
