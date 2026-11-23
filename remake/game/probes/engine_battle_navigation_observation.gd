extends SceneTree

# Finite ordinary-host input/consumer observation; no gameplay state injection.
# Caller supplies an ignored report path and ordinary --authored-package/settings.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var events: Dictionary = {}
var submissions := 0
var receipts: Array[Dictionary] = []
var commits: Array = []
var route := OS.get_environment("SF2_BATTLE_NAVIGATION_ROUTE")
var steps: Array[Dictionary] = []
var shift_pressed := false

func _initialize() -> void:
    create_timer(35).timeout.connect(func(): push_error("Battle navigation observation timed out"); quit(2))
    call_deferred("run")

func check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)

func wait_ms(duration: int) -> void:
    var deadline := Time.get_ticks_msec() + duration
    while Time.get_ticks_msec() < deadline:
        await process_frame

func state(label: String = "") -> Dictionary:
    var s: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    s["submissions"] = submissions
    s["commits"] = commits.duplicate()
    if not label.is_empty():
        var row := {"label": label}
        for key in ["revision", "actor", "stage", "stopReason", "previewX", "previewY",
                "failure", "mainSeed", "thinkingSeed", "queueCursor", "movement", "previewRect",
                "equipmentSlot", "giveMenu", "discardMenu", "returnSlot", "submissions", "commits"]:
            row[key] = s.get(key)
        row["menu"] = {"page": s.menu.page, "command": s.menu.command, "row": s.menu.row,
            "candidate": s.menu.candidate, "visible": s.menu.visible,
            "detail": s.menu.layout.detail, "help": s.menu.layout.help, "navigation": s.menu.layout.navigation,
            "targetCardVisible": s.menu.layout.targetCard.visible}
        samples.append(row)
    return s

func send(action: String, pressed: bool) -> void:
    var e: InputEvent = events[action].duplicate()
    if e is InputEventJoypadMotion or e is InputEventJoypadButton:
        e.device = 0
    if e is InputEventJoypadMotion:
        e.axis_value = events[action].axis_value if pressed else 0.0
    else:
        e.pressed = pressed
    if e is InputEventKey:
        if e.keycode == KEY_SHIFT:
            shift_pressed = pressed
            e.location = KEY_LOCATION_LEFT
        e.shift_pressed = shift_pressed
    Input.parse_input_event(e)
    await process_frame
    await process_frame

func resample_hold(action: String) -> void:
    var e: InputEvent = events[action].duplicate()
    if e is InputEventKey:
        e.pressed = true
        e.echo = true
    elif e is InputEventJoypadButton:
        e.device = 0
        e.pressed = true
    else:
        e.device = 0
        e.axis_value *= 0.42
        Input.parse_input_event(e)
        await process_frame
        e = e.duplicate()
        e.axis_value = -events[action].axis_value
        Input.parse_input_event(e)
        await process_frame
        e = e.duplicate()
        e.axis_value = events[action].axis_value
    Input.parse_input_event(e)
    await process_frame

func pointer(rect: Dictionary, pressed: bool, button := MOUSE_BUTTON_LEFT) -> void:
    var e := InputEventMouseButton.new()
    e.button_index = button
    e.pressed = pressed
    e.position = root.get_stretch_transform() * Vector2(rect.x + rect.width / 2.0, rect.y + rect.height / 2.0)
    Input.parse_input_event(e)
    await process_frame
    await process_frame

func tap(action: String) -> void:
    await send(action, true)
    await send(action, false)

func settle() -> Dictionary:
    var deadline := Time.get_ticks_msec() + 6000
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.movement == null and s.stage != null and s.stopReason in ["PlayerInput", "Rejected"]:
            return s
        await process_frame
    check(false, "Real battle movement/automatic consumer did not settle")
    return state("settle-timeout")

func restart() -> void:
    if is_instance_valid(host):
        root.remove_child(host)
        host.queue_free()
        await process_frame
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("BattleSessionView")
    if view == null:
        push_error("Battle host unavailable: " + str(host.find_children("*", "Label", true, false).map(func(n): return n.text)))
        quit(2)
        await process_frame
        return
    await settle()
    submissions = 0
    receipts.clear()
    commits.clear()
    view.connect("SessionResultObserved", func(json: String):
        var result: Dictionary = JSON.parse_string(json)
        if result.boundary == "submit":
            submissions += 1
            receipts.append({"revision": result.revision, "failure": result.failure,
                "kinds": result.observations.map(func(event): return event.Kind)})
        for event in result.observations:
            if event.Kind == "action-committed" and event.Actor != null: commits.append(event.Actor.Value))
    events.clear()
    for action in ["up", "right", "down", "left", "confirm", "cancel", "menu", "accelerate", "equipment", "give", "discard", "spell", "attack", "item", "target", "stay"]:
        var mapped := InputMap.action_get_events("sf2_" + action)
        for e in mapped:
            if route == "pad" and e is InputEventJoypadButton or route == "axis" and e is InputEventJoypadMotion:
                events[action] = e
                break
        if not events.has(action):
            for e in mapped:
                if e is InputEventKey:
                    events[action] = e
                    break

func same_position(a: Dictionary, b: Dictionary) -> bool:
    return a.previewX == b.previewX and a.previewY == b.previewY

func track_steps(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.observations:
        if event.Detail == "Player" and event.Kind in ["battle-movement-segment-started", "battle-movement-finished"]:
            steps.append({"kind": event.Kind, "frame": Engine.get_process_frames(), "from": event.From, "to": event.To})

func await_steps(count: int) -> void:
    for i in 90:
        if steps.size() >= count: return
        await process_frame
    check(false, "Expected %d movement boundaries, received %d" % [count, steps.size()])

func check_handoff(index: int) -> void:
    if steps.size() <= index + 1:
        check(false, "Missing landing/start pair")
        return
    var landing := steps[index]
    var start := steps[index + 1]
    check(landing.kind == "battle-movement-finished" and start.kind == "battle-movement-segment-started"
        and landing.to == start.from and landing.frame == start.frame, "Next preview starts on the landing frame")

func movement_checks(accelerated := false) -> void:
    await restart()
    var initial := state("movement-initial")
    if events.right is InputEventKey and events.right.keycode != KEY_RIGHT:
        var removed := InputEventKey.new()
        removed.keycode = KEY_RIGHT
        removed.pressed = true
        Input.parse_input_event(removed)
        await wait_ms(180)
        removed = InputEventKey.new()
        removed.keycode = KEY_RIGHT
        removed.pressed = false
        Input.parse_input_event(removed)
        check(same_position(initial, state()), "Removed default direction cannot bypass installed remapping")
    steps.clear()
    view.connect("SessionResultObserved", track_steps)
    if accelerated: await send("accelerate", true)
    check(state().pacing.input.rate == (1.5 if accelerated else 1.0), "Selected movement rate is active")
    await send("right", true)
    await await_steps(5)
    check_handoff(1)
    check_handoff(3)
    var next = state().movement
    check(next != null and not steps.is_empty() and next.from == steps.back().from and next.to == steps.back().to
        and next.progress == 0, "Map projects the new segment at its own starting cell")
    await send("right", false)
    var moved := await settle()
    if accelerated: await send("accelerate", false)
    samples.append({"accelerated": accelerated, "rightHold": steps.duplicate()})
    state("movement-repeated-released")
    check(steps.size() == 6 and submissions == 6 and moved.previewX == initial.previewX + 3
        and moved.previewY == initial.previewY and moved.stage == "Movement" and moved.actor == initial.actor,
        "One hold and mid-step release produce exactly three player steps and three completions")
    check(moved.mainSeed == initial.mainSeed and moved.thinkingSeed == initial.thinkingSeed,
        "Provisional held movement preserves both RNG seeds")
    for i in 20: await process_frame
    check(submissions == 6, "Release stops future movement")
    if accelerated: return
    await restart()
    steps.clear()
    view.connect("SessionResultObserved", track_steps)
    await send("left", true)
    var crossing = state().movement
    var deadline := Time.get_ticks_msec() + 1000
    while crossing != null and crossing.progress <= 0.5 and steps.size() == 1 and Time.get_ticks_msec() < deadline:
        await process_frame
        crossing = state().movement
    check(crossing != null and crossing.progress > 0.5 and steps.size() == 1, "Turn late in the current step")
    await send("down", true)
    await await_steps(3)
    check_handoff(1)
    await send("down", false)
    var turned := await settle()
    state("turned-newest-direction-released")
    for i in 20: await process_frame
    check(same_position(turned, state()), "Releasing newest direction does not resume an older held direction")
    check(turned.previewX == initial.previewX - 1 and turned.previewY == initial.previewY + 1
        and submissions == 4, "Turn during the first delivery replaces the next direction exactly once")
    check(turned.failure == null, "Turn/release isolation is observed before any range rejection")
    await send("left", false)
    await restart()
    await send("right", true)
    await settle()
    var edge := state("range-rejected")
    check(edge.failure == "movement-range" and edge.previewX > initial.previewX and edge.stage == "Movement",
        "Range edge preserves the final legal preview and reports the actual rejection")
    for i in 20: await process_frame
    check(submissions == edge.submissions, "Rejected hold stops rather than repeatedly submitting failures")
    await send("right", false)
    await tap("left")
    var recovered := await settle()
    check(recovered.failure == null and recovered.previewX == edge.previewX - 1
        and recovered.previewY == edge.previewY, "Fresh Left recovers one cell after range rejection")

func movement_menu() -> void:
    await restart()
    var origin := state()
    await send("right", true)
    await send("confirm", true)
    var arrived := await settle()
    check(arrived.stage == "Movement" and arrived.previewX == origin.previewX + 1 and submissions == 2,
        "In-flight Confirm disarms the direction and cannot open or commit on landing")
    await send("right", false)
    await send("confirm", false)
    await tap("confirm")
    check(state().stage == "ActionChoice", "Fresh Confirm opens actions at the delivered cell")
    await tap("cancel")
    await settle()
    check(same_position(origin, state()) and state().stage == "Movement", "Action-menu Cancel returns to origin")
    await send("right", true)
    await tap("menu")
    var paused: Dictionary = JSON.parse_string(host.call("ReadWindowSupportObservationJson"))
    check(paused.battlePaused == "Disabled", "Host menu owns input during the step")
    var before := submissions
    for i in 20: await process_frame
    check(submissions == before, "No movement or commit behind the host menu")
    await tap("cancel")
    check(view.process_mode == Node.PROCESS_MODE_DISABLED, "Held gameplay input keeps the menu release barrier")
    await send("right", false)
    await tap("cancel")
    var resumed := await settle()
    for i in 20: await process_frame
    check(not host.find_child("SystemMenuView", true, false) and submissions == before + 1
        and resumed.stage == "Movement" and resumed.previewX == origin.previewX + 1 and resumed.actor == origin.actor,
        "Dismissing the menu only completes the interrupted step")
    await tap("left")
    await settle()
    var returned := state("fresh-left-after-menu")
    check(same_position(origin, returned) and returned.failure == null and returned.actor == origin.actor
        and returned.stage == "Movement", "Fresh Left moves the same actor back to origin after resume")

func menu_checks() -> void:
    await restart()
    await send("confirm", true)
    var duplicate: InputEvent = events.confirm.duplicate()
    if duplicate is InputEventKey:
        duplicate.pressed = true
        duplicate.echo = true
    elif duplicate is InputEventJoypadButton:
        duplicate.device = 0
        duplicate.pressed = true
    Input.parse_input_event(duplicate)
    await wait_ms(330)
    check(state().stage == "ActionChoice" and state().menu.page == "Main", "Held Confirm only opens actions")
    await send("confirm", false)
    await tap("left")
    await tap("confirm")
    var list := state("magic-list-entered")
    await send("down", true)
    var first := state()
    await wait_ms(160)
    var repeated := state("magic-list-held")
    check(first.menu.row != repeated.menu.row, "Held direction repeats list row navigation")
    await send("down", false)
    await wait_ms(200)
    check(state().menu.row == repeated.menu.row, "Release stops list navigation")
    check(state().mainSeed == list.mainSeed and state().thinkingSeed == list.thinkingSeed,
        "Menu navigation preserves RNG")
    await tap("confirm")
    check(state().menu.page == "Target", "Fresh Confirm enters target selection")
    await send("right", true)
    var target := state()
    await wait_ms(260)
    var next := state("target-held")
    check(target.menu.candidate == next.menu.candidate and target.revision == next.revision,
        "Held direction leaves the selected target stable")
    check(next.scene.visible == false, "Target navigation cannot commit an action")
    await send("cancel", true)
    await wait_ms(260)
    check(state().menu.page == "Magic", "Held Cancel returns once to the owning list")
    var returned := state("target-back-held-direction")
    await wait_ms(200)
    check(state().menu.row == returned.menu.row, "Old target direction cannot spill into its parent list")
    await send("right", false)
    await send("cancel", false)
    await tap("cancel")
    await tap("cancel")
    await settle()
    check(state().stage == "Movement", "Fresh cancellations return through Main to Movement")

func lifecycle_checks() -> void:
    await restart()
    await send("right", true)
    await settle()
    root.focus_exited.emit()
    var suspended := await settle()
    await wait_ms(240)
    root.focus_entered.emit()
    await wait_ms(240)
    check(same_position(suspended, state()), "Focus handler clears held input and prevents resumed movement")
    state("focus-return-held")
    await send("right", false)
    await tap("down")
    await settle()
    check(state().previewY > suspended.previewY, "Fresh direction resumes after focus return")
    await restart()
    await send("right", true)
    await settle()
    await tap("menu")
    var paused: Dictionary = JSON.parse_string(host.call("ReadWindowSupportObservationJson"))
    check(paused.battlePaused == "Disabled", "Ordinary system menu actually pauses its battle consumer")
    var covered := state("host-menu-paused")
    await wait_ms(250)
    check(state().revision == covered.revision, "Host pause prevents movement delivery and repeat")
    await tap("cancel")
    await send("right", false)
    await tap("cancel")
    var resumed := await settle()
    await wait_ms(250)
    check(same_position(resumed, state()), "Closing host menu cannot restart the previous direction")
    await restart()
    await send("right", true)
    await settle()
    view.hide()
    await wait_ms(160)
    view.show()
    var shown := await settle()
    await wait_ms(240)
    check(same_position(shown, state()), "Hide/show does not resume held navigation")
    await send("right", false)
    await direct_stay("confirm")

func same_target(expected: Dictionary, message: String) -> void:
    var now := state()
    check(now.revision == expected.revision and now.submissions == expected.submissions and
        now.target == expected.target and now.menu.candidate == expected.menu.candidate, message)

func board_cell(x: int, y: int) -> Dictionary:
    var terrain: Dictionary = JSON.parse_string(view.get_node("MapViewport").call("ReadTerrainJson"))
    var size: float = terrain.cellSize * terrain.scale
    return {"x": terrain.board.x + x * size, "y": terrain.board.y + y * size, "width": size, "height": size}

func inert_buttons(rect: Dictionary) -> void:
    for button in [MOUSE_BUTTON_WHEEL_UP, MOUSE_BUTTON_WHEEL_DOWN, MOUSE_BUTTON_MIDDLE]:
        await pointer(rect, true, button)
        await pointer(rect, false, button)

func inspection_buttons(origin: Dictionary) -> void:
    await tap("cancel")
    var before := state()
    check(before.inspection.active and before.inspection.page == "Cursor", "Cancel opens read-only inspection")
    await inert_buttons(board_cell(origin.previewX, origin.previewY - 1))
    var after := state()
    check(after.inspection.cursor == before.inspection.cursor and after.revision == before.revision
        and after.submissions == before.submissions, "Wheel and middle preserve inspection focus")
    await tap("cancel")

func target_pointer_admission() -> void:
    var ready := state("legal-target-before-inert-buttons")
    var other: Dictionary = ready.actors.filter(func(unit): return unit.id == "reserve-2")[0]
    var legal := board_cell(other.x, other.y)
    var illegal := board_cell(ready.previewX, ready.previewY)
    await inert_buttons(legal)
    same_target(ready, "Wheel and middle on a different legal target preserve the command and candidate")
    await inert_buttons(illegal)
    same_target(ready, "Wheel and middle on an illegal cell preserve the command and candidate")
    await pointer(illegal, true)
    await pointer(illegal, false)
    var declined := state("illegal-candidate")
    var confirmation: Dictionary = declined.menu.layout.navigation[2]
    check(declined.menu.page == "Target" and declined.menu.candidate == null and declined.target == ready.target
        and declined.revision == ready.revision and declined.submissions == ready.submissions
        and not declined.menu.layout.targetCard.visible and confirmation.visible and not confirmation.enabled
        and "Choose a legal target." in declined.menu.layout.detail and not "commit" in declined.menu.layout.help,
        "Invalid board cell clears the visible candidate and confirmation while retaining the session selection")
    await tap("confirm")
    await pointer(confirmation.rect, true)
    await pointer(confirmation.rect, false)
    same_target(declined, "Mapped and pointer Confirm both refuse the invalid candidate")
    await tap("right")
    var restored := state("legal-target-restored")
    check(restored.revision == declined.revision + 1 and restored.submissions == declined.submissions + 1
        and restored.menu.candidate == restored.target and restored.menu.layout.targetCard.visible
        and restored.menu.layout.navigation[2].enabled and "commit" in restored.menu.layout.help,
        "Fresh direction restores a legal visible and confirmable target in one submission")


func fresh_targets(physical_count: int) -> void:
    await restart()
    var origin := state("target-case-origin")
    if physical_count == 2 and route == "keyboard":
        await inspection_buttons(origin)
    await tap("confirm")
    var actions := state()
    await tap("attack")
    if physical_count == 0:
        var unavailable := state("attack-no-target")
        check(unavailable.stage == "ActionChoice" and unavailable.menu.page == "Main"
            and unavailable.revision == actions.revision and not unavailable.menu.layout.commands[0].enabled
            and "No available target" in unavailable.menu.layout.detail
            and not "Choose a legal target." in unavailable.menu.layout.detail,
            "Attack with zero legal targets stays in Main and displays its distinct reason")
        await tap("confirm")
        same_target(unavailable, "Confirm does not submit the unavailable Attack offer")
    else:
        if physical_count == 2 and route == "keyboard":
            await target_pointer_admission()
        await target_edges("Attack", physical_count, "Main")

    await tap("spell")
    await send("down", true)
    var list := state()
    await wait_ms(145)
    check(state().menu.row != list.menu.row, "Magic keeps ordinary list repeat")
    var selected_row := state("magic-row-before-target")
    await tap("confirm")
    var entered := state("target-entry-with-held-list-direction")
    var entry_receipts := receipts.slice(selected_row.submissions)
    check(entered.menu.page == "Target" and entered.menu.candidate == origin.actor
        and entered.menu.layout.targetCard.visible and entered.menu.layout.navigation[2].enabled
        and not entered.menu.layout.listVisible and entered.revision == selected_row.revision + 2
        and entry_receipts.size() == 2 and entry_receipts[0].kinds == ["spell-selected"]
        and entry_receipts[1].kinds == ["target-selected"]
        and entry_receipts[0].revision == selected_row.revision + 1 and entry_receipts[1].revision == entered.revision,
        "Magic publishes the action and default target receipts in order and shows the legal decision")

    await resample_hold("down")
    await wait_ms(260)
    same_target(entered, "An inherited direction cannot cross into Target")
    await send("down", false)
    await target_edges("Magic", 2, "Magic")
    check(state().menu.row == selected_row.menu.row, "Target Back restores the exact Magic level")
    if physical_count == 2:
        await tap("confirm")
        await send("right", true)
        await tap("menu")
        var covered := state("target-host-covered")
        check(view.process_mode == Node.PROCESS_MODE_DISABLED, "System menu covers the target phase")
        await wait_ms(245)
        await send("right", false)
        await send("right", true)
        await tap("cancel")
        await resample_hold("right")
        await wait_ms(160)
        same_target(covered, "Host close with a held mapped direction keeps the live Target undisturbed")
        check(state().menu.page == "Target" and state().menu.visible, "Host return shows the same target phase")
        await send("right", false)

        if route == "keyboard":
            var rect: Dictionary = state().menu.layout.navigation[2].rect
            var before_resize := state()
            await pointer(rect, true)
            root.size = Vector2i(2560, 1440)
            await wait_ms(85)
            check(state().menu.layout.navigation[2].rect != rect, "Client resize moves the actual confirmation control")
            root.size = Vector2i(1920, 1080)
            await wait_ms(85)
            check(state().menu.layout.navigation[2].rect == rect, "Confirmation returns to the original bounds")
            await pointer(rect, false)
            same_target(before_resize, "Resize invalidates a captured Confirm even when its bounds return")
            await pointer(rect, true)
            await tap("cancel")
            await tap("confirm")
            var reopened := state("target-reopened-with-held-click")
            await pointer(rect, false)
            same_target(reopened, "A held click release cannot confirm a different target phase")
        var before_edge := state()
        await tap("right")
        check(state().revision == before_edge.revision + 1,
            "Fresh direction after cover and reentry submits one selection")
        await tap("cancel")
    await tap("cancel")

    await tap("item")
    await tap("down")
    await tap("confirm")
    await target_edges("Item", 2, "Items")
    var returned := state("target-item-parent")
    check(returned.menu.row == 1 and returned.inventories == origin.inventories and
        returned.mainSeed == origin.mainSeed and returned.thinkingSeed == origin.thinkingSeed and commits.is_empty(),
        "All target returns preserve the item row, inventory and RNG without spending an action")
    if route == "keyboard" and physical_count == 2:
        await tap("confirm")
        var board := view.get_node("MapViewport")
        var terrain: Dictionary = JSON.parse_string(board.call("ReadTerrainJson"))
        var rect: Dictionary = terrain.cellCursor.rect
        await pointer(rect, true)
        await tap("cancel")
        var parent := state()
        await pointer(rect, false)
        same_target(parent, "A board release after Back has no target or commit")
        await tap("confirm")
        terrain = JSON.parse_string(board.call("ReadTerrainJson"))
        rect = terrain.cellCursor.rect
        await pointer(rect, true)
        await pointer(rect, false)
        var clicked := state("fresh-board-click-after-back")
        check(clicked.menu.page != "Target" and clicked.scene.visible, "A fresh complete board click commits after reentry")

func target_edges(label: String, count: int, parent: String) -> void:
    var start := state(label + "-entered")
    check(start.menu.page == "Target" and start.menu.candidate != null and start.menu.candidate == start.target,
        label + " enters a legal Target phase")
    await send("right", true)
    var moved := state(label + "-first-edge")
    check(moved.revision == start.revision + 1 and (moved.menu.candidate != start.menu.candidate) == (count > 1),
        label + " selects once per fresh direction")
    await resample_hold("right")
    await wait_ms(260)
    same_target(moved, label + " ignores repeated samples and held direction")
    await tap("down")
    check(state().revision == moved.revision + 1, label + " accepts a different direction while Right is held")
    await tap("up")
    var independent := state()
    check(independent.menu.candidate == moved.menu.candidate and independent.revision == moved.revision + 2,
        label + " preserves previous/next candidate order")
    await send("right", false)
    await tap("right")
    await tap("right")
    var tapped := state(label + "-rapid-taps")
    check(tapped.revision == independent.revision + 2 and tapped.menu.candidate == independent.menu.candidate,
        label + " accepts each rapid fresh press in the existing order")
    await tap("target")
    check(state().revision == tapped.revision + 1, label + " Target shortcut selects once")
    await send("right", true)
    var before_back := state()
    await tap("cancel")
    var backed := state(label + "-back")
    var back_receipts := receipts.slice(before_back.submissions)
    check(backed.menu.page == parent and backed.stage == "ActionChoice" and backed.target == null
        and backed.menu.candidate == null and not backed.menu.layout.targetCard.visible
        and not backed.menu.layout.navigation[2].visible and backed.menu.layout.listVisible == (parent != "Main")
        and backed.revision == before_back.revision + 1 and back_receipts.size() == 1
        and back_receipts[0].kinds == ["action-choice"] and back_receipts[0].revision == backed.revision,
        label + " Back submits once and restores the live parent controls")

    await wait_ms(245)
    same_target(backed, label + " carried direction cannot repeat in the parent")
    await send("right", false)

func direct_stay(gesture: String, notice := false) -> void:
    await restart()
    var initial := state("stay-origin")
    if gesture != "confirm":
        await tap("right")
        await settle()
        check(state().previewX == initial.previewX + 1, "Stay uses a moved legal preview")
    await tap("confirm")
    for i in range(posmod(2-state().menu.command,4)): await tap("down")
    await send("down", true)
    var decision := state("stay-decision")
    check(decision.stage == "ActionChoice" and decision.menu.command == 3, "Main owns the Stay decision")
    if gesture == "click":
        var rect: Dictionary = decision.menu.layout.commands[3].rect
        await pointer(rect, true)
        check(state().revision == decision.revision and commits.is_empty(), "Mouse-down alone does not spend Stay")
        await pointer(rect, false)
    else:
        await send(gesture, true)
    if notice:
        for i in range(180):
            if state().scene.phase == "PoisonDamageMessage": break
            await process_frame
        var shown := state("stay-poison-notice")
        check(shown.scene.phase == "PoisonDamageMessage", "Stay reaches the real after-turn notice")
        await resample_hold(gesture)
        await wait_ms(260)
        check(state().revision == shown.revision and state().scene.phase == shown.scene.phase,
            "Held Confirm cannot acknowledge the new notice")
        await send(gesture, false)
        await tap("confirm")
    var next := await settle()
    state("stay-next-actor")
    check(next.actor != initial.actor and next.stage == "Movement" and commits.count(initial.actor) == 1,
        "One Stay gesture advances the acting turn once directly to continuation")
    var stayed: Dictionary = next.actors.filter(func(a): return a.id == initial.actor)[0]
    check(stayed.x == decision.previewX and stayed.y == decision.previewY, "Stay commits the displayed legal stop")
    if gesture != "click" and not notice:
        await resample_hold(gesture)
        await wait_ms(280)
        check(state().revision == next.revision and state().stage == "Movement",
            "Held input cannot operate the next actor")
        await send(gesture, false)
    await send("down", false)

func transaction_confirmation(kind: String) -> void:
    await restart()
    await tap("confirm")
    var decision := state("transaction-decision")
    await tap(kind)
    await tap("confirm")
    if kind == "give":
        await tap("target")
        await tap("confirm")
    var expected := "GiveCommitReady" if kind == "give" else "DiscardCommitReady"
    var pending := state("explicit-" + kind + "-confirmation")
    check(pending.stage == expected and pending.inventories == decision.inventories and commits.is_empty(),
        "Transaction waits for explicit confirmation before changing custody")
    await tap("cancel")
    var parent := state("transaction-back")
    check(parent.stage == ("GiveTargetChoice" if kind == "give" else "ActionChoice") and
        parent.inventories == decision.inventories and same_position(parent, decision),
        "Transaction Back preserves inventory and the legal stop")
    if kind == "discard":
        await tap(kind)
    await tap("confirm")
    check(state().stage == expected, "Reentering still requires confirmation")
    await tap("confirm")
    for i in range(60):
        if state()[kind].revealed == true: break
        await process_frame
    var result := state("transaction-awaiting-acknowledgement")
    check(result[kind].visible and result[kind].revealed and result.inventories != decision.inventories and
        commits.is_empty(), "Committed custody waits for real result acknowledgement")
    await tap("confirm")
    var returned := await settle()
    state("transaction-return")
    if kind == "give":
        check(returned.actor != decision.actor and commits.count(decision.actor) == 1,
            "Give ends the acting turn after acknowledgement")
    else:
        check(returned.actor == decision.actor and returned.stage == "ActionChoice" and
            same_position(returned, decision) and commits.is_empty(), "Drop returns free to the same decision")

func auxiliary_checks(kind: String) -> void:
    await restart()
    await tap("confirm")
    await tap(kind)
    var initial := state("auxiliary-opened")
    check(initial.stage in ["EquipmentChoice", "GiveItemChoice", "DiscardItemChoice"], "Real auxiliary selection is admitted")
    await send("down", true)
    var first := state()
    await wait_ms(160)
    var repeated := state("auxiliary-held")
    check(repeated.revision > first.revision, "Held direction reaches the actual auxiliary selector again")
    check(repeated.equipmentSlot != first.equipmentSlot or repeated.giveMenu != first.giveMenu or
        repeated.discardMenu != first.discardMenu, "Repeat changes the projected auxiliary candidate")
    await send("down", false)
    var released := state()
    await wait_ms(200)
    check(state().revision == released.revision, "Release stops auxiliary navigation")
    await tap("cancel")
    check(state().stage == "ActionChoice" and state().inventories == initial.inventories and state().mainSeed == initial.mainSeed and
        state().thinkingSeed == initial.thinkingSeed, "Auxiliary Back preserves resources/RNG and returns to actions")

func range_projection(label: String) -> Dictionary:
    var board: Node = view.get_node("MapViewport")
    var terrain: Dictionary = JSON.parse_string(board.call("ReadTerrainJson"))
    samples.append({"label": label, "movementRange": terrain.movementRange, "actionRange": terrain.actionRange})
    return terrain

func traversal_range_checks() -> void:
    await restart()
    var initial := state("corridor-origin")
    var shown := range_projection("corridor-range")
    var movement: Dictionary = shown.movementRange
    var expected: Array = []
    for x in range(2, 9): expected.append({"X": float(x), "Y": 3.0})
    check(initial.stage == "Movement" and initial.previewX == 3 and initial.previewY == 3
        and movement.visible and movement.draws > 0 and movement.drawnReachableCells == expected
        and movement.reachableCells == expected, "Draw includes ally and tiles beyond it, excluding enemy, barriers and over-budget cells")
    check(movement.destinations == expected.filter(func(p): return p.X != 4.0)
        and not shown.actionRange.visible, "Legal stops omit the ally and action range stays hidden in Movement")

    await tap("right")
    await settle()
    var ally := state("preview-on-ally")
    for i in 20: await process_frame
    check(ally.previewX == 4 and ally.previewY == 3 and same_position(ally, state())
        and submissions == ally.submissions, "Releasing Right on the ally stops further preview steps")
    await tap("confirm")
    var refusal := state("occupied-confirm")
    check(refusal.failure == "occupied-destination" and refusal.stage == "Movement"
        and refusal.actor == initial.actor and same_position(ally, refusal) and refusal.revision == ally.revision,
        "Confirm on the ally refuses without committing")
    check(range_projection("range-after-refusal").movementRange.drawnReachableCells == expected,
        "The drawn range stays anchored through preview and refusal")

    await tap("right")
    await settle()
    var beyond := state("preview-beyond-ally")
    check(beyond.previewX == 5 and beyond.previewY == 3 and beyond.failure == null,
        "Fresh Right can continue through the ally")
    await tap("confirm")
    check(state("valid-confirm").stage == "ActionChoice" and not range_projection("range-in-actions").movementRange.visible,
        "Confirm on the empty cell opens actions and hides movement range")
    await tap("spell")
    var spell := range_projection("spell-reach")
    check(state("spell-list").menu.page == "Magic" and not spell.movementRange.visible and spell.actionRange.visible
        and spell.actionRange.reach == expected.slice(2, 5), "Magic shows its own one-tile reach from the provisional stop")
    await tap("cancel")
    await tap("cancel")
    await settle()
    var restored := range_projection("range-after-back")
    check(same_position(initial, state("back-at-origin")) and state().stage == "Movement"
        and restored.movementRange.visible and restored.movementRange.drawnReachableCells == expected
        and not restored.actionRange.visible, "Back restores the original movement drawing after return")

func run() -> void:
    root.size = Vector2i(1920, 1080)
    var auxiliary := OS.get_environment("SF2_BATTLE_NAVIGATION_AUX")
    if auxiliary == "handoff":
        await movement_checks()
        await movement_checks(true)
        await movement_menu()
    elif auxiliary == "traversal-range":
        await traversal_range_checks()
    elif auxiliary.begins_with("fresh-targets-"):
        await fresh_targets(int(auxiliary.get_slice("-", 2)))
    elif auxiliary.begins_with("confirm-"):
        await transaction_confirmation(auxiliary.get_slice("-", 1))
    elif auxiliary.begins_with("direct-stay-"):
        var gesture := auxiliary.get_slice("-", 2)
        await direct_stay({"notice": "confirm", "shortcut": "stay"}.get(gesture, gesture), gesture == "notice")
    elif auxiliary.is_empty():
        await movement_checks()
        await menu_checks()
        await lifecycle_checks()
    else:
        await auxiliary_checks(auxiliary)
    root.remove_child(host)
    host.queue_free()
    await create_timer(0.15).timeout
    var file := FileAccess.open(OS.get_environment("SF2_BATTLE_NAVIGATION_OUTPUT"), FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed": failures.is_empty(), "failures": failures,
        "route": route, "samples": samples, "receipts": receipts, "focusBoundary": "window signals; physical OS focus remains unclaimed"}, "  "))
    file.close()
    print("Battle navigation observation passed: ", failures.is_empty())
    quit(0 if failures.is_empty() else 1)
