extends SceneTree
var host: Node
var view: Node
var samples: Array = []
var failures: Array = []
var done := false
var attach: Dictionary = {}
func _initialize() -> void: call_deferred("run")
func check(ok: bool, message: String) -> void:
    if not ok: failures.append(message); push_error(message)
func frames(count: int) -> void:
    for i in range(count): await process_frame
func key(code: int) -> void:
    var e := InputEventKey.new(); e.keycode=code; e.pressed=true; Input.parse_input_event(e)
    await frames(2)
    e=InputEventKey.new(); e.keycode=code; e.pressed=false; Input.parse_input_event(e)
    await frames(2)
func persistence() -> Dictionary: return JSON.parse_string(host.call("ReadPersistenceObservationJson"))
func state() -> Dictionary: return JSON.parse_string(view.call("ReadObservationJson"))
func panel() -> Dictionary:
    var p := host.find_child("SaveSlotView",true,false)
    return {} if p==null else JSON.parse_string(p.call("ReadSaveObservationJson"))
func sample(label: String) -> void:
    samples.append({"label":label,"milliseconds":Time.get_ticks_msec(),"persistence":persistence(),"state":state() if view!=null else {},"panel":panel(),"window":JSON.parse_string(host.call("ReadWindowSupportObservationJson")),"audio":JSON.parse_string(host.call("ReadAudioObservationJson"))})
func wait_panel(expected: String) -> bool:
    var deadline:=Time.get_ticks_msec()+10000
    while Time.get_ticks_msec()<deadline:
        if panel().get("state")==expected: return true
        OS.delay_msec(2)
        await process_frame
    sample("panel-timeout-"+expected); check(false,"panel "+expected); return false
func node_added_observe(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and node.get_class()=="Control":
        if not node.is_connected("SessionResultObserved",on_result.bind(node)): node.connect("SessionResultObserved",on_result.bind(node))
func on_result(json: String, node: Node) -> void:
    var r:Dictionary=JSON.parse_string(json)
    if r.boundary=="attach":
        view=node
        if node.name in ["BattleSessionView","ExplorationSessionView"]: attach=persistence()
func wait_field() -> bool:
    var deadline:=Time.get_ticks_msec()+20000
    var i:=0
    while Time.get_ticks_msec()<deadline:
        var field:=host.get_node_or_null("ExplorationSessionView")
        if field!=null:
            var f:Dictionary=JSON.parse_string(field.call("ReadObservationJson"))
            if f.get("failure")!=null:view=field;sample("field-failure");check(false,"field return has no failure");return false
            if f.get("canWaitAtInput",false) and f.get("cursor")==null:
                view=field;return true
            if f.get("wait") in ["DialogueWait","FieldTextWait","W1TextWait","ChoiceWait","FieldSearchWait"]:await key(KEY_ENTER)
        elif i%30==29:await key(KEY_ENTER)
        i+=1
        OS.delay_msec(1)
        await process_frame
    sample("field-timeout");check(false,"usable field return");return false
func settle() -> bool:
    for i in range(6000):
        var s:=state()
        if s.get("failure")!=null: sample("settle-failure"); check(false,"no battle failure"); return false
        if s.get("stage")=="Movement" and s.get("stopReason")=="PlayerInput" and s.get("movement")==null: return true
        if s.get("scene",{}).get("requiresAcknowledgement",false) or s.get("discard",{}).get("revealed",false): await key(KEY_ENTER)
        elif i%30==29: await key(KEY_ENTER)
        await process_frame
    sample("settle-timeout"); check(false,"settled action");return false
func stay() -> bool:
    await key(KEY_ENTER); await key(KEY_SPACE); await key(KEY_ENTER)
    return await settle()
func suffix() -> void:
    for i in range(8):
        if not await stay(): return
    await key(KEY_F10)
    sample("suffix-final")
    await key(KEY_X)
func save_flow(quit_after: bool, expect_error: bool=false, close_during: bool=false) -> bool:
    await key(KEY_F10); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_panel("save-slots"):return false
    sample("save-slots")
    await key(KEY_ENTER)
    check(panel().get("state")=="confirm-save","slot confirms overwrite separately")
    sample("confirm-save")
    await key(KEY_DOWN)
    if quit_after: await key(KEY_DOWN)
    if quit_after: sample("save-and-quit-confirmed");write_report()
    await key(KEY_ENTER)
    if close_during:
        host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
        if not await wait_panel("save-uncertain"):return false
        sample("close-after-publication-result")
        return true
    if quit_after:
        for i in range(10000):
            if panel().get("state") in ["error","save-uncertain"]:
                sample("quit-write-error")
                if expect_error:return true
                check(false,"SaveQuit success");finish();return false
            OS.delay_msec(1); await process_frame
        check(false,"SaveQuit exits process");return false
    if not await wait_panel("saved"):return false
    sample("saved")
    await key(KEY_ENTER)
    sample("returned")
    return true
func continue_title(field: bool=false) -> bool:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_panel("continue-slots"):return false
    sample("continue-slots")
    await key(KEY_ENTER)
    if field:
        if not await wait_field():return false
    elif not await wait_battle():return false
    samples.append({"label":"before-first-input","persistence":attach})
    sample("continued")
    return true
func wait_battle() -> bool:
    var deadline:=Time.get_ticks_msec()+20000
    while Time.get_ticks_msec()<deadline:
        view=host.get_node_or_null("BattleSessionView")
        if view!=null and state().get("stage")=="Movement" and state().get("stopReason")=="PlayerInput": return true
        var field:=host.get_node_or_null("ExplorationSessionView")
        if field!=null:
            var f:Dictionary=JSON.parse_string(field.call("ReadObservationJson"))
            if f.get("wait") in ["DialogueWait","FieldTextWait","W1TextWait"]: await key(KEY_ENTER)
        OS.delay_msec(1)
        await process_frame
    sample("battle-timeout"); check(false,"battle Movement reached"); return false
func run() -> void:
    node_added.connect(node_added_observe)
    root.size=Vector2i(1920,1080)
    host=(load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
    # Headless input uses native dispatch without taking a visible foreground window.
    root.grab_focus(); await frames(3)
    var mode:=OS.get_environment("SF2_SAVE_CASE")
    if mode in ["unknown","backup-cancel","backup-recover"]:
        await key(KEY_ENTER);await key(KEY_E);await key(KEY_DOWN);await key(KEY_ENTER)
        if not await wait_panel("continue-slots"):finish();return
        sample("recovery-slots")
        await key(KEY_ENTER)
        if not await wait_panel("error" if mode=="unknown" else "recover-backup"):finish();return
        sample("recovery-decision")
        if mode=="backup-recover":
            await key(KEY_DOWN);await key(KEY_ENTER)
            if not await wait_battle():finish();return
            sample("backup-mounted")
        else:
            await key(KEY_X)
            check(host.find_child("SessionEntryView",true,false)!=null,"cancel retains entry")
            sample("recovery-cancelled")
        finish();return
    if mode in ["pilot","vertical","boundaries"]:
        var start=OS.get_environment("SF2_BATTLE_SAVE_START")
        samples.append({"label":"controlled-field-approach","result":host.call("StartPreVoyageLocalizationObservation",start)})
        if not await wait_battle(): finish(); return
        sample("battle-entry")
        if mode=="boundaries":
            var identity=state().sessionId
            var held:=InputEventKey.new();held.keycode=KEY_F10;held.pressed=true;Input.parse_input_event(held)
            await frames(2)
            await key(KEY_ENTER)
            check(host.find_child("SystemMenuView",true,false)!=null,"held menu requires release before confirm")
            held=InputEventKey.new();held.keycode=KEY_F10;held.pressed=false;Input.parse_input_event(held)
            await frames(2);await key(KEY_X)
            check(state().sessionId==identity,"held input preserves session")
            await key(KEY_UP)
            await frames(30)
            var before:=state()
            await key(KEY_F10)
            var reason:=host.find_child("ReasonSave",true,false)
            check(reason!=null and "Cancel movement" in reason.text,"preview refusal explains cancel")
            check(state().revision==before.revision,"menu does not cancel preview")
            sample("nonzero-preview-refused")
            await key(KEY_X);await key(KEY_X);await key(KEY_F10)
            var frozen=state().revision
            await frames(20)
            check(state().revision==frozen,"pending return callbacks freeze under menu")
            sample("undo-pending-refused")
            await key(KEY_X)
            if not await settle():finish();return
            sample("real-undo-completed")
            check(not FileAccess.file_exists(OS.get_environment("SF2_SAVE_ROOT")+"/"+OS.get_environment("SF2_SAVE_NAMESPACE")+"/sf2-opening/slot1.json"),"no queued save")
            finish();return
        if mode=="vertical":
            # Natural connected admission, followed by one consumed player entry.
            if not await stay():finish();return
            sample("partly-consumed-queue")
        if not await save_flow(false):finish();return
        if mode=="vertical": await suffix()
    elif mode in ["resume","save-quit","shape","resources","victory","defeat","retreat","field-continue","storage-fail","uncertain","uncertain-close","busy-scene","overwrite-cancel","native-history"]:
        if not await continue_title(mode=="field-continue"):finish();return
        if mode=="native-history":
            if not await stay():finish();return
            sample("enemy-scene-completed")
            if not await save_flow(false):finish();return
            finish();return
        if mode=="overwrite-cancel":
            var before=persistence()
            await key(KEY_F10);await key(KEY_DOWN);await key(KEY_ENTER)
            if not await wait_panel("save-slots"):finish();return
            await key(KEY_ENTER)
            sample("overwrite-consent")
            check("Overwrite" in panel().message,"occupied slot explicitly asks consent")
            await key(KEY_ENTER)
            if not await wait_panel("save-slots"):finish();return
            await key(KEY_X);await key(KEY_X)
            check(persistence().progress==before.progress,"overwrite refusal preserves gameplay")
            sample("overwrite-cancelled")
            finish();return
        if mode in ["storage-fail","uncertain","uncertain-close"]:
            if not await stay():finish();return
            var identity=state().sessionId
            if not await save_flow(true,true,mode=="uncertain-close"):finish();return
            check(panel().state==("save-uncertain" if mode in ["uncertain","uncertain-close"] else "error"),"failure remains in game")
            await key(KEY_X)
            if mode in ["uncertain","uncertain-close"]:
                await key(KEY_ENTER)
                check(panel().get("state")=="save-uncertain","uncertain cannot retry before inspection")
                await key(KEY_ENTER)
                if not await wait_panel("continue-slots"):finish();return
                sample("uncertain-inspected")
                await key(KEY_X)
            await key(KEY_X)
            check(state().sessionId==identity,"failure/cancel retains session")
            if not await stay():finish();return
            sample("failure-returned-to-play")
            finish();return
        if mode=="busy-scene":
            await key(KEY_ENTER);await key(KEY_H);await key(KEY_ENTER);await key(KEY_ENTER)
            var before:=state()
            check(before.scene.get("visible",false),"real committed scene pending")
            await key(KEY_F10)
            await frames(30)
            check(state().revision==before.revision,"scene callbacks frozen")
            sample("scene-save-refused")
            await key(KEY_X)
            if not await settle():finish();return
            sample("pending-scene-really-completed")
            finish();return
        if mode=="field-continue":sample("usable-field-continue");finish();return
        if mode in ["victory","defeat","retreat"]:
            await key(KEY_ENTER)
            if mode=="victory":await key(KEY_F)
            elif mode=="defeat":await key(KEY_SPACE)
            else:await key(KEY_H);await key(KEY_ENTER)
            sample("outcome-action")
            await key(KEY_ENTER)
            if not await wait_field():finish();return
            sample("usable-field-return")
            if not await save_flow(false):finish();return
            sample("returned-field-v1-saved")
            finish();return
        if mode=="shape":
            if not await save_flow(false):finish();return
            if not await stay():finish();return
            sample("shape-next-player")
            finish();return
        if mode=="resources":
            await key(KEY_ENTER);await key(KEY_H);await key(KEY_ENTER)
            for i in range(12):
                if state().get("candidate")=="ally-1":break
                await key(KEY_TAB)
            sample("heal-target")
            await key(KEY_ENTER)
            if not await settle():finish();return
            sample("paid-heal")
            await key(KEY_ENTER);await key(KEY_I)
            sample("item-list")
            await key(KEY_DOWN);await key(KEY_ENTER)
            sample("item-target")
            await key(KEY_ENTER)
            if not await settle():finish();return
            sample("paid-item")
            if not await save_flow(false):finish();return
            await suffix();finish();return
        if mode=="save-quit":
            # Persist report before the actual successful UI exit removes this observer.
            write_report()
            await save_flow(true)
            return
        await suffix()
    finish()
func write_report() -> void:
    var file:=FileAccess.open(OS.get_environment("SF2_SAVE_REPORT"),FileAccess.WRITE)
    file.store_string(JSON.stringify({"pid":OS.get_process_id(),"case":OS.get_environment("SF2_SAVE_CASE"),"samples":samples,"failures":failures},"\t")); file.close()
func finish() -> void:
    if done:return
    done=true
    write_report()
    # Bounded exit attribution only: release the observer's node references and the
    # actual host immediately. No GC, mixer delay or altered gameplay completion.
    if OS.get_environment("SF2_SAVE_RELEASE_OBSERVER")=="1":
        view=null
        host.free()
        host=null
    quit(0 if failures.is_empty() else 1)
