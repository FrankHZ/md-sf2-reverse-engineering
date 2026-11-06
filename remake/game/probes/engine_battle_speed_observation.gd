extends "res://probes/engine_battle_inspection_observation.gd"

var mode := OS.get_environment("SF2_SPEED_MODE")
var phases: Array = []
var delivered: Array = []
var last_token = null

func settings() -> Node:
    return host.find_child("SettingsView",true,false)

func preferences() -> Dictionary:
    return JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied

func settings_state() -> Dictionary:
    return JSON.parse_string(settings().call("ReadSettingsObservationJson"))

func open_speed() -> bool:
    if not state().inspection.active:
        await act("cancel")
        if not await inspect_page("Cursor"): return false
        if not await cursor_to(3,2): return false
        await act("confirm")
    if not await inspect_page("Options"): return false
    for n in range(3):
        if state().inspection.optionIndex == 2: break
        await act("down")
    await send_action("confirm",true); await frames(4)
    if not check(settings() != null and settings_state().battleOnly, "Speed opens the two-setting host draft"): return false
    await send_action("confirm",false)
    return true

func close_speed(action: String) -> bool:
    await send_action(action,true); await frames(5)
    if not check(settings() == null and state().inspection.page == "Options", "Speed returns to exact Options caller"): return false
    if not check(state().inspection.optionIndex == 2, "Caller retains Speed selection"): return false
    await send_action(action,false)
    return true

func preference_bytes() -> String:
    var path := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    return FileAccess.get_file_as_string(path)

func transaction() -> bool:
    var initial := preferences().duplicate(true)
    var original_file := preference_bytes()
    var baseline := signature(state())
    if not await open_speed(): return false
    samples.append({"label":"initial-custom", "settings":settings_state()})
    if not check("47.5" in settings_state().labels[0], "Custom reveal preference is explicit"): return false
    samples.append({"label":"custom", "settings":settings_state()})
    await act("right"); await act("down"); await act("right")
    if not await close_speed("cancel"): return false
    if not check(preferences() == initial and signature(state()) == baseline and preference_bytes() == original_file, "B discards both drafts without gameplay mutation"): return false
    if not await open_speed(): return false
    await act("down"); await act("right")
    if not await close_speed("attack"): return false
    var saved := preferences()
    var disk: Dictionary = JSON.parse_string(preference_bytes().trim_prefix("\ufeff"))
    if not check(disk.suppressBattleHitCoinMessages and disk.charactersPerSecond == 47.5, "A writes the actual ordinary preference file"): return false
    if not check(saved.TextMode == initial.TextMode and saved.CharactersPerSecond == 47.5 and saved.SuppressBattleHitCoinMessages, "A saves suppression while preserving custom reveal"): return false
    if not check(saved.Bindings == initial.Bindings and saved.Language == initial.Language and saved.ReducedFlash == initial.ReducedFlash, "Unrelated preferences and custom bindings survive"): return false
    if not await open_speed(): return false
    for preset in range(4):
        await act("right")
        var d: Dictionary = settings_state().draft
        if not check(d.TextMode == ("instant" if preset == 3 else "adjustable"), "Preset mode " + str(preset)): return false
        if preset < 3 and not check(d.CharactersPerSecond == [15.0,30.0,60.0][preset], "Preset cps " + str(preset)): return false
    # Actual host focus notifications clear held input while the draft/session stay paused.
    var before := signature(state())
    await send_action("right",true)
    root.focus_exited.emit(); await frames(4)
    root.focus_entered.emit(); await send_action("right",false)
    if not check(signature(state()) == before and not JSON.parse_string(host.call("ReadSettingsHostObservationJson")).held, "Focus loss releases held navigation and keeps gameplay paused"): return false
    # The held Right selected0; return left to3 for the comparison.
    await act("left")
    if not await close_speed("confirm"): return false
    if not check(preferences().TextMode == "instant" and preferences().SuppressBattleHitCoinMessages, "C publishes both options"): return false
    if not check(signature(state()) == baseline, "All settings edits retain gameplay and both RNG seeds"): return false
    samples.append({"label":"accepted", "preferences":preferences(),"signature":baseline})
    return true

func record_scene() -> void:
    var sc: Dictionary = state().scene
    if sc.get("waitToken") == null or sc.waitToken == last_token: return
    last_token = sc.waitToken
    phases.append({"phase":sc.phase,"token":sc.waitToken,"message":sc.message,"suppressed":sc.suppressed,"suppressible":sc.suppressible})

func fight() -> bool:
    process_frame.connect(record_scene)
    if state().inspection.active:
        await act("cancel"); await act("cancel")
    await act("confirm"); await act("attack"); await act("confirm")
    # TargetChoice -> commit confirmation is a separate real input.
    for n in range(4):
        if state().scene.get("phase") != null: break
        await act("confirm")
    var deadline := Time.get_ticks_msec()+20000
    var saw_scene := false
    var pause_checked := false
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("failure") != null: return fail("Battle failed: " + str(s.failure))
        if s.scene.get("phase") != null:
            saw_scene = true
            record_scene()
            if s.scene.phase == "ActionMessage" and not pause_checked:
                pause_checked = true
                var before := signature(s)
                await act("menu")
                var generic := host.find_child("SystemSettings",true,false)
                if not check(generic != null and generic.disabled, "General Settings remains disabled during battle"): return false
                await frames(8)
                if not check(signature(state()) == before, "Host Menu pauses pending scene without RNG or token advance"): return false
                await act("cancel")
            if s.scene.get("requiresAcknowledgement",false) and not s.scene.suppressed:
                await act("confirm")
            else: await process_frame
        elif saw_scene and s.stage == "Movement" and s.movement == null: break
        else: await process_frame
    samples.append({"label":"observed-phases","phases":phases})
    if not check(saw_scene and state().stage == "Movement" and state().movement == null, "Real battle completion returns to next Movement"): return false
    if not check(phases.any(func(p): return p.phase == "RewardMessage" and not p.suppressed and not p.message.is_empty()), "EXP message remains visible and acknowledged"): return false
    if not check(phases.any(func(p): return p.phase == "GoldMessage"), "Coin message seam reached"): return false
    for p in phases:
        if not check(p.suppressed == (preferences().SuppressBattleHitCoinMessages and p.suppressible), "Finite suppression policy for " + p.phase): return false
        if p.suppressed and not check(p.message.is_empty(), "Suppressed text is absent"): return false
    samples.append({"label":"battle-complete","phases":phases,"gameplay":signature(state()).slice(3),"preferences":preferences()})
    return true

func run() -> void:
    root.size=Vector2i(1920,1080)
    host=load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(4); bind_events()
    if not await enter_battle(): return
    if mode == "transaction":
        if not await transaction(): return
    elif mode == "restore":
        if not check(preferences().TextMode == "instant" and preferences().SuppressBattleHitCoinMessages, "Fresh process loads actual persisted preferences"): return
        if not await open_speed(): return
        if not check(settings_state().draft.SuppressBattleHitCoinMessages, "Reopened draft restores saved suppression"): return
        if not await close_speed("cancel"): return
    if not await fight(): return
    process_frame.disconnect(record_scene)
    root.remove_child(host); host.queue_free(); await frames(2)
    finish(true)
