extends "res://probes/engine_world_viewport_observation.gd"

# Bounded ordinary-host input and actual board projection. No screenshots or state setters.
func sample(label: String) -> Dictionary:
    var s := state()
    var row := {"label": label}
    for key in ["sessionId", "revision", "stage", "actor", "target", "candidate", "previewX", "previewY",
            "movement", "failure", "mainSeed", "thinkingSeed", "queueCursor", "previewNodes", "previewInsideMap"]:
        row[key] = s.get(key)
    row.actors = s.actors.map(func(a): return {"id":a.id,"x":a.x,"y":a.y,"hp":a.hp,"mp":a.mp,
        "nodeColor":a.nodeColor,"nodeX":a.nodeX,"nodeY":a.nodeY,"sprite":a.sprite})
    row.menu = {"page":s.menu.page,"candidate":s.menu.candidate}
    row.board = board()
    samples.append(row)
    return s

func board() -> Dictionary:
    return JSON.parse_string(view.get_node("MapViewport").call("ReadTerrainJson"))

func settled() -> bool:
    return await wait_for(func(s): return s.get("movement") == null and s.get("stage") != null, "selection settled")

func feedback(label: String, expect_range: bool) -> bool:
    await frames(2)
    var s := sample(label)
    var b: Dictionary = samples[-1].board
    var cue: Dictionary = b.selection
    if not check(b.cellSize == 96 and b.scale == 1 and b.clip, "Retained 96px clipped board"): return false
    if not check(cue.visible and cue.actor == s.actor and cue.draws > 0 and cue.z == 3,
            label + ": actual selected actor cue drawn above terrain"): return false
    if not check(is_equal_approx(cue.rect.width, 96) and is_equal_approx(cue.rect.height, 96) and cue.stroke == "ffffffff"
            and cue.outlineWidth > cue.strokeWidth and cue.inset > cue.outlineWidth / 2,
            label + ": steady inset white brackets with contrasting outline"): return false
    if not check(s.previewNodes == 1 and b.children.filter(func(n): return n.name == "Selection").size() == 1
            and b.children.filter(func(n): return n.z == 3).size() == 1 + b.targets.size(),
            label + ": one actor cue, no path breadcrumb nodes"): return false
    var actor: Dictionary = s.actors.filter(func(a): return a.id == s.actor)[0]
    if not check(is_equal_approx(cue.rect.x, actor.nodeX - 2) and is_equal_approx(cue.rect.y, actor.nodeY - 4),
            label + ": cue follows the actual actor projection"): return false
    for a in s.actors:
        if a.sprite != null and a.sprite.visible:
            if not check(a.sprite.modulate == "ffffffff" and a.sprite.texturePresent,
                    label + ": sprite palette unmodified: " + a.id): return false
    if not check(b.movementRange.visible == expect_range, label + ": range lifecycle"): return false
    if expect_range:
        if not check(b.movementRange.destinations == b.movementRange.drawnDestinations and b.movementRange.draws > 0,
                label + ": actual range draw consumes exact legal stops"): return false
    return true

func same_resources(a: Dictionary, b: Dictionary) -> bool:
    return a.mainSeed == b.mainSeed and a.thinkingSeed == b.thinkingSeed and a.queueCursor == b.queueCursor \
        and a.actors.map(func(v): return [v.id,v.x,v.y,v.hp,v.mp]) == b.actors.map(func(v): return [v.id,v.x,v.y,v.hp,v.mp])

func enter_battle() -> bool:
    var deadline := Time.get_ticks_msec() + 25000
    var nudged := false
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("failure") != null: return fail("Entry failure: " + str(s.failure))
        if s.get("stage") == "Movement" and s.get("movement") == null: return true
        if s.get("canWaitAtInput",false) and not nudged:
            nudged = true
            await tap(KEY_DOWN)
        elif s.get("canWaitForText",false) or s.get("scene",{}).get("requiresAcknowledgement",false) \
                or "Reveal / Continue" in str(s.get("help","")):
            await tap(KEY_ENTER)
        else: await process_frame
    return fail("Controlled field approach did not reach actual battle input")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(4)
    if OS.get_environment("SF2_SELECTION_PRIVATE") == "1":
        var result = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_SELECTION_START"))
        if not check(result == "started", "Existing private controlled-start admission: " + str(result)): return
        await frames(2)
    if host.find_child("ExplorationSessionView",true,false) == null and host.find_child("BattleSessionView",true,false) == null:
        samples.append({"label":"startup-diagnostic","labels":host.find_children("*","Label",true,false).map(func(n): return n.text)})
        fail("Ordinary host did not mount a gameplay view; see startup diagnostic")
        return
    if not await enter_battle(): return
    if not feedback_ready(): return
    if not await feedback("initial", true): return
    var initial := state()
    var initial_range: Dictionary = board().movementRange
    var directions := [[KEY_RIGHT,1,0],[KEY_DOWN,0,1],[KEY_LEFT,-1,0],[KEY_UP,0,-1]]
    var direction: Array = []
    for d in directions:
        if initial_range.destinations.any(func(p): return p.X == initial.previewX+d[1] and p.Y == initial.previewY+d[2]):
            direction = d
            break
    if not check(not direction.is_empty(), "Available neighboring legal stop"): return
    await tap(direction[0])
    if not check(state().movement != null, "Actual movement delivery observed"): return
    if not await feedback("moving-preview", true): return
    if not await settled(): return
    if not await feedback("preview-settled", true): return
    if not check(board().movementRange.destinations == initial_range.destinations
            and same_resources(initial, state()), "Preview retains range, committed actor positions and resources"): return
    await tap(KEY_ENTER)
    if not await feedback("actions", false): return
    await tap(KEY_ESCAPE)
    if not await feedback("returning-preview", true): return
    if not await settled(): return
    if not await feedback("cancel-restored", true): return
    if not check(state().previewX == initial.previewX and state().previewY == initial.previewY
            and same_resources(initial,state()), "Cancel restores origin without resource or RNG effects"): return
    var stable_cue: Dictionary = board().selection
    var idle_revision = state().revision
    await create_timer(0.7).timeout
    if not check(board().selection.rect == stable_cue.rect and board().selection.visible
            and board().selection.stroke == stable_cue.stroke and state().revision == idle_revision,
            "Idle cue stays visible and steady without additional logical progress"): return
    # Try one direction until the ordinary range guard rejects; keep the last accepted endpoint.
    var rejected := false
    for i in range(48):
        var before := state()
        await tap(direction[0])
        var after := state()
        if after.failure != null:
            rejected = true
            if not check(after.previewX == before.previewX and after.previewY == before.previewY
                    and after.revision == before.revision, "Range rejection preserves accepted preview"): return
            if not await feedback("range-edge-rejected", true): return
            break
        if not await settled(): return
    if not check(rejected, "Reached actual range edge"): return
    await tap(KEY_ESCAPE)
    if not await settled(): return
    if not await feedback("edge-cancel-restored", true): return
    if not check(same_resources(initial,state()), "Range-edge traversal/cancel preserves resources"): return
    # Authored healer uses Magic; the connected first actor uses its ordinary Medical Herb.
    var private_case := OS.get_environment("SF2_SELECTION_PRIVATE") == "1"
    var parent_page := "Items" if private_case else "Magic"
    await tap(KEY_ENTER)
    await tap(KEY_I if private_case else KEY_H)
    sample("action-list")
    if not check(state().menu.page == parent_page, "Ordinary action list opened"): return
    if private_case: await tap(KEY_DOWN) # Slot 1 Herb, after equipped slot 0 Wooden Stick.
    await tap(KEY_ENTER)
    sample("target-entry")
    if not check(state().menu.page == "Target", "Ordinary target selection opened"): return
    if not await feedback("first-target", false): return
    var first := state()
    for i in range(8):
        await tap(KEY_RIGHT)
        if state().failure == null and state().target != first.target: break
    if not await feedback("switched-target", false): return
    var second := state()
    if not check(first.candidate != second.candidate and second.target == second.candidate,
            "Actual accepted target switches"): return
    var target_actor: Dictionary = second.actors.filter(func(a): return a.id == second.target)[0]
    var target_cell := Rect2(target_actor.nodeX-2, target_actor.nodeY-4, 96, 96)
    if not check(board().targets.size() == 4 and board().targets.all(func(t): return t.z == 3 \
            and target_cell.encloses(Rect2(t.rect.x,t.rect.y,t.rect.width,t.rect.height))),
            "Cyan target border follows only the selected target"): return
    await tap(KEY_ESCAPE)
    if not await feedback("target-back", false): return
    if not check(state().menu.page == parent_page and board().targets.is_empty(), "Target Back clears target border and returns to list"): return
    await tap(KEY_ESCAPE)
    await tap(KEY_ESCAPE)
    if not await settled(): return
    if not await feedback("final-movement", true): return
    if not check(same_resources(initial,state()), "Target browsing and Back do not commit gameplay"): return
    finish(true)

func feedback_ready() -> bool:
    var s := state()
    if OS.get_environment("SF2_SELECTION_PRIVATE") == "1":
        return check(s.actors.any(func(a): return a.sprite != null and a.sprite.visible), "Private mounted sprite consumer exists")
    return true

func finish(completed := false) -> void:
    # Release this observer's native node references before the owned process exits.
    view = null
    if is_instance_valid(host): host.free()
    host = null
    super.finish(completed)
