extends "res://probes/engine_equipment_observation.gd"

const CASTER := "ally-15"
var rank_ := int(OS.get_environment("SF2_AURA_RANK"))
var recipients: Array = []
var fairy_seen := false

func unit(state: Dictionary, id: String) -> Dictionary:
    return state.actors.filter(func(row): return row.id == id)[0]

func settle() -> Dictionary:
    for _frame in range(10000):
        await process_frame
        var current := scene()
        if current.failure != null or current.scene.error != null:
            check(false, "native session or presentation error")
            samples.append(current)
            return {}
        var s: Dictionary = current.scene
        if not s.fairySprites.is_empty():
            fairy_seen = true
            for row in s.fairySprites:
                check(row.binding != null, "fairy resource binding")
        if s.phase == "ResultMessage" and not recipients.has(s.displayedAlly):
            recipients.append(s.displayedAlly)
        if s.requiresAcknowledgement == true:
            await key(KEY_ENTER)
        var b := board()
        if s.phase == null and current.movement == null and b.stage == "Movement" and b.stopReason == "PlayerInput":
            return b
    check(false, "bounded ordinary return")
    return {}

func run() -> void:
    root.size = Vector2i(1920, 1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await process_frame
    var admitted = "started"
    if OS.get_environment("SF2_AURA_CONTINUE") == "1":
        for _frame in range(20): await process_frame
        await key(KEY_ENTER)
        await key(KEY_E)
        await key(KEY_DOWN)
        await key(KEY_ENTER)
        for _frame in range(2000):
            OS.delay_msec(5)
            await process_frame
            var panel := host.find_child("SaveSlotView", true, false)
            if panel != null and JSON.parse_string(panel.call("ReadSaveObservationJson")).state == "continue-slots": break
        await key(KEY_ENTER)
        for _frame in range(2000):
            OS.delay_msec(5)
            await process_frame
            if host.find_child("BattleSessionView", true, false) != null: break
    else:
        admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_AURA_START"))
    view = host.find_child("BattleSessionView", true, false)
    check(admitted == "started" and view != null, "controlled private admission: " + str(admitted))
    if not failures.is_empty():
        samples.append({"labels":host.find_children("*", "Label", true, false).map(func(node): return node.text)})
        finish(); return
    view.connect("SessionResultObserved", result_observed)
    var before := await settle()
    if OS.get_environment("SF2_AURA_COLD_ONLY") == "1":
        samples.append({"continued":before})
        await save()
        finish(); return
    for _turn in range(40):
        if before.is_empty() or before.actor == CASTER: break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        before = await settle()
    check(not before.is_empty() and before.actor == CASTER, "Karna ordinary turn")
    if not failures.is_empty(): finish(); return
    await key(KEY_ENTER)
    await key(KEY_H)
    # Current book order is HEAL1-3, BLAST1-4, BOOST1-2, AURA1-4.
    for _row in range(8 + rank_): await key(KEY_DOWN)
    await key(KEY_ENTER)
    var selected := board()
    check(selected.spell != null and selected.spell.Value == "aura" and selected.spell.Level == rank_,
        "ordinary Magic rank selection")
    if not failures.is_empty():
        samples.append(selected)
        finish(); return
    if rank_ == 4: check(selected.target == CASTER, "whole-side self center")
    events.clear()
    await key(KEY_ENTER)
    var after := await settle()
    if after.is_empty(): finish(); return
    var payments := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == CASTER)
    var recoveries := events.filter(func(e): return e.Kind == "healing-recovery")
    var awards := events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == CASTER)
    check(payments.size() == 1 and unit(before, CASTER).mp - unit(after, CASTER).mp == [7,11,15,20][rank_-1],
        "single rank-specific payment")
    check(recoveries.size() >= 2 and recipients.size() == recoveries.size(), "all ordered recipients presented")
    check(awards.size() == 1 and fairy_seen, "single accumulated award and bound fairy presentation")
    check(after.sessionId == before.sessionId, "same session return")
    if OS.get_environment("SF2_AURA_CONTINUE") == "1": await save()
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "next ordinary action menu")
    samples.append({"rank":rank_, "before":before, "after":after, "recipients":recipients})
    finish()

func save() -> void:
    await key(KEY_F10)
    for _row in range(4):
        var button := host.find_child("SystemSave", true, false) as Button
        if button != null and button.theme_type_variation == "EntrySelectedButton": break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_slots("save-slots"): return
    await key(KEY_ENTER)
    if not await wait_slots("confirm-save"): return
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if await wait_slots("saved"): await key(KEY_ENTER)

func wait_slots(expected: String) -> bool:
    for _frame in range(2000):
        OS.delay_msec(5)
        await process_frame
        var panel := host.find_child("SaveSlotView", true, false)
        if panel == null: continue
        var state: Dictionary = JSON.parse_string(panel.call("ReadSaveObservationJson"))
        if state.state == expected: return true
        if state.state in ["error", "save-uncertain"]:
            samples.append(state)
            break
    check(false, "save panel " + expected)
    return false

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures,
        "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    quit(0 if failures.is_empty() else 1)
