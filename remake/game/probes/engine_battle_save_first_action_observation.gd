extends "res://probes/engine_battle_save_observation.gd"

var observed: Array = []

func on_result(json: String, node: Node) -> void:
    super.on_result(json, node)
    var result: Dictionary = JSON.parse_string(json)
    check(result.failure == null, "session result has no failure")
    observed.append_array(result.observations)

func actor(board: Dictionary, id: String) -> Dictionary:
    for row in board.actors:
        if row.id == id:
            return row
    check(false, "actor available: " + id)
    return {}

func settle() -> bool:
    var deadline := Time.get_ticks_msec() + 12000
    while Time.get_ticks_msec() < deadline:
        var board := state()
        if board.get("failure") != null or board.scene.error != null:
            break
        if board.stage == "Movement" and board.stopReason == "PlayerInput" and board.movement == null:
            return true
        if board.scene.requiresAcknowledgement and not board.scene.suppressed and not board.scene.completed:
            await key(KEY_ENTER)
        await process_frame
    sample("settle-timeout")
    check(false, "action reaches settled Movement")
    return false

func save_over_slot_one() -> bool:
    await key(KEY_F10)
    await key(KEY_UP)
    await key(KEY_ENTER)
    if not await wait_panel("save-slots"):
        return false
    await key(KEY_ENTER)
    if panel().get("state") != "confirm-save":
        check(false, "slot 1 overwrite confirmation")
        return false
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_panel("saved"):
        return false
    await key(KEY_ENTER)
    sample("save-return")
    return true

func cast() -> bool:
    var before := state()
    check(before.actor == "ally-4" and before.queueCursor == 1, "Kazin at the controlled entry")
    await key(KEY_ENTER)
    await key(KEY_H)
    var labels: Array = state().menu.layout.rows
    var index := -1
    for i in range(labels.size()):
        if "混乱 2" in labels[i]:
            index = i
            break
    if index < 0:
        sample("magic-rank-unavailable")
        check(false, "MUDDLE 2 present in ordinary Magic")
        return false
    for i in range(index):
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    for i in range(8):
        if state().get("target") == "enemy-1":
            break
        await key(KEY_TAB)
    if state().get("target") != "enemy-1":
        sample("cast-target-unavailable")
        check(false, "Dark Bishop selected through ordinary targeting")
        return false
    await key(KEY_ENTER)
    if not await settle():
        return false
    var after := state()
    sample("cast-settled")
    var kazin := actor(after, "ally-4")
    check(kazin.mp == 3 and kazin.exp == 72, "Kazin pays and earns once")
    check(actor(after, "enemy-1").status == 56, "Dark Bishop carries rank 2")
    return failures.is_empty()

func first_stay(check_busy: bool) -> void:
    var before := state()
    check(before.actor == "ally-9" and before.round == 25 and before.queueCursor == 3,
        "Gerhalt resumes at the partly consumed queue")
    check(actor(before, "enemy-1").status == 56, "rank 2 active before first action")
    observed.clear()
    await key(KEY_ENTER)
    await key(KEY_SPACE)
    var deadline := Time.get_ticks_msec() + 12000
    while Time.get_ticks_msec() < deadline:
        var board := state()
        if board.get("failure") != null:
            break
        if board.scene.phase == "PhysicalInactionMessage":
            sample("guarded-inaction")
            if check_busy:
                var revision: int = board.revision
                await key(KEY_F10)
                var save: Button = host.find_child("SystemSave", true, false)
                check(save != null and save.disabled and panel().is_empty(), "Save is disabled while the scene is pending")
                check(persistence().availability.Error == "save-battle-busy", "live battle refuses the in-flight scene")
                await frames(15)
                check(state().revision == revision, "system menu freezes pending scene")
                sample("busy-save-refused")
                await key(KEY_X)
            if not await settle():
                return
            var after := state()
            var bishop := actor(after, "enemy-1")
            var casualty := actor(after, "enemy-0")
            check(after.actor == "ally-0" and after.stage == "Movement" and after.queueCursor == 5,
                "Bowie reaches the next stable decision")
            check(bishop.hp == 39 and bishop.status == 40, "guard leaves HP intact and ticks rank 2")
            check(casualty.initialized and casualty.hp == 0 and casualty.x == null and casualty.y == null and not casualty.visible,
                "cleaned slot stays off the projected board")
            check(actor(after, "ally-4").mp == 3 and actor(after, "ally-4").exp == 72,
                "no duplicated payment or reward")
            samples.append({"label":"first-action-effects","effects":observed.duplicate()})
            sample("first-action-settled")
            return
        await process_frame
    sample("guard-not-reached")
    check(false, "real Stay reaches the guarded action")

func run() -> void:
    node_added.connect(node_added_observe)
    root.size = Vector2i(1920, 1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(3)
    var cold := OS.get_environment("SF2_SAVE_CASE") == "cold"
    if await continue_title():
        if cold or await cast():
            if await save_over_slot_one():
                await first_stay(cold)
    finish()
