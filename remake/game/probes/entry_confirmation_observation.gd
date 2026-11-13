extends "res://probes/localization_observation.gd"
var last_frame := 0
var max_gap := 0
func gap() -> void:
    var now := Time.get_ticks_usec()
    if last_frame > 0: max_gap = maxi(max_gap, now-last_frame)
    last_frame = now
func entry() -> Dictionary:
    return JSON.parse_string(host.find_child("SessionEntryView",true,false).call("ReadEntryObservationJson"))
func snap(label: String) -> void:
    var e := host.find_child("SessionEntryView",true,false)
    var p := e.find_child("SessionMusic",true,false) if e else null
    var s := host.find_child("SaveSlotView",true,false)
    samples.append({"label":label,"driver":AudioServer.get_driver_name(),"entry":entry() if e else {},"slots":JSON.parse_string(s.call("ReadSaveObservationJson")) if s else {},"player":{"paused":p.stream_paused,"playing":p.playing,"position":p.get_playback_position(),"canProcess":p.can_process()} if p else {}})
func until_slots() -> void:
    for i in range(2000):
        var s := host.find_child("SaveSlotView",true,false)
        if s and not JSON.parse_string(s.call("ReadSaveObservationJson")).state.ends_with("loading"): return
        await process_frame
    check(false,"slot completion deadline")
func choose(label: String, key: Key) -> void:
    var audio_before: Dictionary = entry().audio
    max_gap=0; last_frame=Time.get_ticks_usec()
    var begin:=Time.get_ticks_usec()
    await press(key)
    if host.find_child("SaveSlotView",true,false)==null: await press(KEY_ENTER)
    var loading := host.find_child("SaveSlotView",true,false)
    if loading and label.begins_with("new"):
        check(JSON.parse_string(loading.call("ReadSaveObservationJson")).state=="entry-loading","immediate loading feedback")
    await until_slots()
    samples.append({"label":label,"elapsedMs":(Time.get_ticks_usec()-begin)/1000.0,"maxFrameGapMs":max_gap/1000.0})
    snap(label+"-ready")
    await create_timer(0.3).timeout
    snap(label+"-later")
    var audio_after: Dictionary = entry().audio
    check(not audio_after.musicPaused and audio_after.musicPlaying,"entry music remains active: "+label)
    check(audio_before.sequence==audio_after.sequence,"slot admission does not restart music: "+label)
    check(audio_after.musicPosition>audio_before.musicPosition,"entry music advances: "+label)
func _run() -> void:
    var space:=OS.get_environment("SF2_ENTRY_CONFIRMATION_NAMESPACE")
    if space=="" or space!=OS.get_environment("SF2_SAVE_NAMESPACE") or OS.get_environment("SF2_ENTRY_CONFIRMATION_OUTPUT")=="":
        push_error("Select an explicit disposable save namespace and report path before entry observation.")
        quit(2);return
    process_frame.connect(gap)
    root.size=Vector2i(1920,1080)
    host=load("res://Main.tscn").instantiate();root.add_child(host)
    await process_frame;await process_frame
    if OS.get_environment("SF2_ENTRY_CONFIRMATION_CASE").begins_with("active-quit"):
        await active_quit();return
    await press(KEY_ENTER);await press(KEY_E)
    if entry().visibleCharacters>=0 and entry().visibleCharacters<entry().totalCharacters: await press(KEY_ENTER)
    if OS.get_environment("SF2_ENTRY_CONFIRMATION_CASE")!="timing":
        await boundaries();return
    snap("menu")
    await choose("new-first",KEY_ENTER)
    await press(KEY_ESCAPE)
    await choose("new-again",KEY_ENTER)
    await press(KEY_ESCAPE);await press(KEY_DOWN)
    await choose("continue",KEY_ENTER)
    await press(KEY_ESCAPE)
    snap("returned")
    finish()

func slot_state() -> Dictionary:
    var s:=host.find_child("SaveSlotView",true,false)
    return JSON.parse_string(s.call("ReadSaveObservationJson")) if s else {}

func active_quit() -> void:
    # Reuse the existing controlled field bootstrap; subsequent menu/name/close inputs
    # are ordinary product actions. This does not claim preceding natural reach.
    var controlled:=OS.get_environment("SF2_ENTRY_CONFIRMATION_START")
    if controlled=="":check(false,"active close needs an existing controlled field start");finish();return
    check(host.call("StartPreVoyageLocalizationObservation",controlled)=="started","real controlled field session mounted")
    await process_frame;await process_frame;await process_frame
    await press(KEY_F10)
    check(host.find_child("SystemMenuView",true,false)!=null,"ordinary system menu opens")
    for i in range(3):await press(KEY_DOWN)
    await press(KEY_ENTER)
    check(entry().replacement,"active session opens replacement entry")
    await press(KEY_UP);await press(KEY_ENTER)
    if OS.get_environment("SF2_ENTRY_CONFIRMATION_CASE")=="active-quit-write":
        await until_slots();await press(KEY_ENTER);await press(KEY_ENTER)
        await press(KEY_DOWN);await press(KEY_ENTER)
        for i in range(1200):
            if slot_state().get("message","").begins_with("Saving"):break
            await process_frame
        check(slot_state().get("message","").begins_with("Saving"),"active replacement publication reached")
    var before: Dictionary=JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
    samples.append({"label":"active-close-request","slots":slot_state(),"sessionId":before.sessionId,"revision":before.revision})
    for i in range(600):
        if slot_state().get("state")=="confirm-quit":break
        await process_frame
    var after: Dictionary=JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    samples.append({"label":"active-close-completed","slots":slot_state(),"sessionId":after.sessionId,"revision":after.revision})
    check(slot_state().get("state")=="confirm-quit","deferred close reaches existing quit confirmation")
    check(slot_state().get("message","").contains("unsaved"),"original dirty-session warning retained")
    check(before.sessionId==after.sessionId and before.revision==after.revision,"replacement result preserves original session")
    if slot_state().get("state")!="confirm-quit":finish();return
    await press(KEY_ESCAPE)
    check(host.find_child("SaveSlotView",true,false)==null,"cancel quit releases confirmation")
    await press(KEY_ESCAPE)
    view=host.get_node_or_null("ExplorationSessionView")
    check(view!=null and state().canWaitAtInput,"cancel quit resumes original playable session")
    check(not JSON.parse_string(host.call("ReadAudioObservationJson")).musicPaused,"cancel quit resumes original audio")
    finish()
func wait_field(label: String) -> void:
    for i in range(1800):
        view=host.get_node_or_null("ExplorationSessionView")
        if view:
            samples.append({"label":label,"elapsedMs":(Time.get_ticks_usec()-last_begin)/1000.0,"maxFrameGapMs":max_gap/1000.0,"audio":JSON.parse_string(host.call("ReadAudioObservationJson")),"persistence":JSON.parse_string(host.call("ReadPersistenceObservationJson"))})
            return
        if slot_state().get("state","").contains("error"):
            snap("error");check(false,"field start failed");return
        await process_frame
    check(false,"field mount deadline")
var last_begin:=0
func boundaries() -> void:
    var which:=OS.get_environment("SF2_ENTRY_CONFIRMATION_CASE")
    # The preceding Confirm reveals the menu; it must not select a slot.
    if which in ["new","quit-write"]:
        await press(KEY_ENTER);await until_slots();snap("slots")
        await press(KEY_ENTER)
        check(host.find_child("NameEntryView",true,false)!=null,"ordinary name opens")
        await press(KEY_ENTER)
        check(slot_state().get("state")=="confirm-new-game","ordinary final confirmation")
        await press(KEY_DOWN)
        max_gap=0;last_begin=Time.get_ticks_usec()
        await press(KEY_ENTER)
        check(slot_state().get("state")=="entry-loading","prepare feedback")
        if which=="quit-write":
            for i in range(1200):
                if slot_state().get("message","").begins_with("Saving"): break
                await process_frame
            check(slot_state().get("message","").begins_with("Saving"),"write pending reached")
            host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
            var report:=FileAccess.open(OS.get_environment("SF2_ENTRY_CONFIRMATION_OUTPUT"),FileAccess.WRITE)
            report.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"slots":slot_state()},"  "));report.close()
            return
        await press(KEY_ENTER);await press(KEY_ENTER)
        await wait_field("new-mounted")
        check(view!=null,"new session mounted after durable write")
        await create_timer(0.2).timeout
        finish();return
    if which=="restore":
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots();snap("continue-slots")
        max_gap=0;last_begin=Time.get_ticks_usec()
        await press(KEY_ENTER);await wait_field("continue-mounted")
        check(view!=null,"ordinary Continue mounted")
        finish();return
    if which=="bad-slot":
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots();snap("bad-primary-list")
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots();snap("empty-slot")
        check(slot_state().get("state")=="error","empty slot fails visibly")
        await press(KEY_ESCAPE)
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots()
        await press(KEY_ENTER);await until_slots();snap("recovery-offer")
        check(slot_state().get("state")=="recover-backup","corrupt primary requires explicit recovery")
        await press(KEY_ESCAPE)
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots()
        await press(KEY_ENTER);await until_slots()
        await press(KEY_DOWN)
        max_gap=0;last_begin=Time.get_ticks_usec()
        await press(KEY_ENTER);await wait_field("recovered-mounted")
        check(view!=null,"backup recovery restores ordinary session")
        finish();return
    if which=="window":
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots()
        root.size=Vector2i(1280,720)
        await press(KEY_ENTER)
        await create_timer(0.2).timeout
        snap("covered-continue")
        check(host.find_child("WindowRecoveryView",true,false)!=null,"invalid window opens recovery before restore")
        check(entry().audio.musicPaused,"window recovery pauses music")
        root.size=Vector2i(1920,1080)
        await process_frame;await process_frame;await process_frame
        snap("recovery-closed")
        check(slot_state().get("state")=="continue-slots","recovered window retains original slot choice")
        check(not entry().audio.musicPaused,"window recovery resumes music")
        finish();return
    if which=="cancel":
        await press(KEY_ENTER)
        check(slot_state().get("state")=="entry-loading","cancel begins during load")
        await press(KEY_ENTER);await press(KEY_DOWN);await press(KEY_ENTER)
        check(slot_state().get("state")=="entry-loading","repeated confirmation rejected")
        await press(KEY_ESCAPE)
        samples.append({"label":"return-request","slots":slot_state()})
        for i in range(1200):
            if host.find_child("SaveSlotView",true,false)==null:break
            await process_frame
        check(host.find_child("SaveSlotView",true,false)==null,"cancel returns after in-flight read")
        check(host.get_node_or_null("ExplorationSessionView")==null,"cancel publishes no late session")
        await choose("new-after-cancel",KEY_ENTER)
        check(slot_state().get("state")=="new-game-slots","retry after canceled load")
        finish();return
    if which=="bad-source":
        var original:=OS.get_environment("SF2_PRIVATE_EXPLORATION_CONTENT")
        OS.set_environment("SF2_PRIVATE_EXPLORATION_CONTENT",OS.get_environment("SF2_ENTRY_CONFIRMATION_OUTPUT")+".missing")
        await press(KEY_ENTER);await until_slots();snap("bad-source")
        check(slot_state().get("state")=="new-game-error","bad source rejected")
        OS.set_environment("SF2_PRIVATE_EXPLORATION_CONTENT",original)
        await press(KEY_DOWN);await press(KEY_ENTER);await until_slots();snap("source-retry")
        check(slot_state().get("state")=="new-game-slots","corrected source re-admitted")
        finish();return
    if which=="detach":
        await press(KEY_ENTER)
        var old:=host
        root.remove_child(old)
        host=load("res://Main.tscn").instantiate();root.add_child(host)
        old.queue_free()
        await create_timer(1.5).timeout
        check(host.find_child("SaveSlotView",true,false)==null,"late result does not attach to replacement host")
        check(entry().stage=="title","replacement host retains title")
        snap("replacement-host");finish();return
    if which=="focus":
        await press(KEY_ENTER)
        root.emit_signal("focus_exited")
        await press(KEY_ENTER);await until_slots();snap("load-after-focus-loss")
        check(slot_state().get("state")=="new-game-slots","focus loss does not duplicate action")
        root.emit_signal("focus_entered")
        await press(KEY_ESCAPE)
        # Back from the slots selects New Game in the Witch menu.
        await press(KEY_UP)
        if entry().visibleCharacters >= 0 and entry().visibleCharacters < entry().totalCharacters:
            await press(KEY_ENTER)
        await press(KEY_ENTER)
        snap("settings-paused")
        check(host.find_child("SettingsView",true,false)!=null,"Witch Settings opens")
        check(entry().audio.musicPaused,"Settings pauses entry music")
        await press(KEY_ESCAPE);snap("settings-closed")
        check(not entry().audio.musicPaused,"Settings close resumes entry music")
        finish();return
    if which=="quit":
        await press(KEY_ENTER)
        host.notification(Node.NOTIFICATION_WM_CLOSE_REQUEST)
        samples.append({"label":"quit-during-load","slots":slot_state()})
        var f:=FileAccess.open(OS.get_environment("SF2_ENTRY_CONFIRMATION_OUTPUT"),FileAccess.WRITE)
        f.store_string(JSON.stringify({"passed":failures.is_empty(),"samples":samples},"  "));f.close()
        return
    check(false,"unknown case");finish()



func finish() -> void:
    var output:=FileAccess.open(OS.get_environment("SF2_ENTRY_CONFIRMATION_OUTPUT"),FileAccess.WRITE)
    if output==null:
        push_error("Entry observation output could not be written.");quit(2);return
    output.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"samples":samples},"  "))
    output.close();quit(0 if failures.is_empty() else 1)
