extends "res://probes/engine_world_viewport_observation.gd"

var events: Array = []
var last_sequence := -1
var seen: Dictionary = {}

func observe_node(node: Node) -> void:
    if node.has_signal("SessionResultObserved") and not node.is_connected("SessionResultObserved", result_events):
        node.connect("SessionResultObserved", result_events)

func result_events(raw: String) -> void:
    var r: Dictionary = JSON.parse_string(raw)
    for e in r.get("observations", []):
        if int(e.Sequence) > last_sequence:
            last_sequence = int(e.Sequence)
            if e.Kind in ["outcome-program-started", "battle-returned", "gold-changed", "experience-changed", "action-committed"]: events.append(e)

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label,"state":s})
    return s

func run() -> void:
    root.size = Vector2i(1920,1080)
    node_added.connect(observe_node)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(5)
    if not await focus(): return
    var initial := record("initial")
    if case_name == "illegal":
        await illegal_move()
        return
    if case_name in ["failed", "startup-failed"]:
        if not check(initial.feedbackVisible and "stopped" in initial.feedback, "Failure is visible above battle"): return
        await tap(KEY_ENTER)
        if not check(state().revision == initial.revision, "Failed input stays stopped"): return
        finish(true); return
    var acted := false
    for frame in range(12000):
        if ended: return
        var s := state()
        if not check(s.get("failure") == null, "Session/presentation healthy"): return
        if s.get("canWaitAtInput", false) and acted:
            await party_check()
            return
        if s.has("stage"):
            var scene: Dictionary = s.scene
            if scene.get("phase") != null:
                var token := str(scene.waitToken)
                if not seen.has(token):
                    seen[token] = true
                    if scene.message != "":
                        await frames(2)
                        s = record("message-" + str(scene.phase))
                        if not check(s.scene.messagePanel.visible and s.scene.messagePanel.fontSize == 30, "Shared readable message panel"): return
                if "Continue" in str(scene.messagePanel.hint) or "Reveal" in str(scene.messagePanel.hint): await tap(KEY_ENTER)
                else: await process_frame
            elif s.stage == "Movement" and s.stopReason == "PlayerInput":
                await tap(KEY_ENTER)
                if not acted:
                    await tap(KEY_H)
                    record("unavailable-magic")
                    await tap(KEY_ESCAPE)
                await tap(KEY_SPACE if case_name == "defeat" else KEY_F)
                await tap(KEY_ENTER)
                acted = true
            else: await process_frame
        else:
            if not seen.has("return"):
                seen["return"] = true
                record("return-program-busy")
                var battle := host.find_child("BattleSessionView", true, false)
                if not check(not battle.get_node("CommandMenu").is_visible_in_tree() and not battle.get_node("BattleScene").is_visible_in_tree(), "Old battle windows retired"): return
                await tap(KEY_E)
                if not check(not state().partyStatus.visible, "Busy return cannot open status"): return
            if s.get("wait") == "DialogueWait" or s.get("canWaitForText", false): await tap(KEY_ENTER)
            else: await process_frame
    fail("Bounded return deadline")

func illegal_move() -> void:
    # Ordinary movement reaches the authored board boundary; no injected command/result.
    for step in range(12):
        await tap(KEY_UP)
        await frames(30)
        var s := state()
        if s.get("failure") == null: continue
        record("illegal-move")
        if not check(s.failureKind == "IllegalCommand" and s.menu.visible, "Recoverable illegal move retains menu"): return
        if not check("Unavailable:" in s.menu.layout.detail and s.menu.layout.helpVisible, "Menu preserves readable reason and controls"): return
        if not check(not s.feedbackVisible, "Global feedback does not cover menu error or controls"): return
        await tap(KEY_ESCAPE)
        await frames(30)
        await tap(KEY_ENTER)
        var recovered := record("recovered-actions")
        if not check(recovered.failure == null and recovered.stage == "ActionChoice" and recovered.menu.visible, "Fresh ordinary input recovers after illegal move"): return
        finish(true)
        return
    fail("No illegal boundary reached within twelve moves")

func party_check() -> void:
    var before := record("ready-return")
    if not check(not before.battleMounted and "Back in the field" in before.returnNotice, "Actual ready return notice"): return
    var count := 0
    await tap(KEY_E)
    await frames(4)
    count = int(state().partyStatus.count)
    for member in range(count):
        var s := record("party-" + str(member))
        var c: Dictionary = s.partyStatus.card
        var a: Dictionary = s.party.filter(func(row): return row.Actor.Value == c.actor)[0]
        if not check(s.partyStatus.visible and c.actor == a.Actor.Value and c.hp.current == a.Hp and c.mp.current == a.Mp, "Card reads authoritative party values"): return
        if not check(c.hp.image.drawCount > 0 and str(int(s.gold)) in s.partyStatus.header, "Actual gauge draw and carried gold"): return
        await tap(KEY_RIGHT)
        await frames(3)
    var revision = state().revision
    await tap(KEY_ENTER)
    if not check(not state().partyStatus.visible and state().revision == revision, "Close does not click through"): return
    await tap(KEY_E)
    await frames(3)
    view.hide()
    await frames(2)
    view.show()
    await frames(3)
    await tap(KEY_ESCAPE)
    var after := record("closed-status")
    if not check(after.party == before.party and after.gold == before.gold and after.revision == before.revision, "View, paging, suspension and close mutate no resources"): return
    await tap(KEY_RIGHT)
    await wait_for(func(s): return s.get("canWaitAtInput", false), "fresh field movement")
    var moved := record("fresh-movement")
    if not check(moved.entities != before.entities and not moved.partyStatus.visible, "Fresh movement is accepted after close"): return
    if not check(events.filter(func(e): return e.Kind == "outcome-program-started").size() == 1 and events.filter(func(e): return e.Kind == "battle-returned").size() == 1, "Exactly one outcome and return"): return
    samples.append({"label":"events","events":events})
    finish(true)
