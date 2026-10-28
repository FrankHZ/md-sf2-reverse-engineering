extends "res://probes/engine_battlefield_map_observation.gd"

# Explicit initial conditions only; subsequent actions use ordinary host keyboard input.
var retreat_events: Array = []
var retreat_seen: Dictionary = {}
var battle_view: Node
var egress_case := OS.get_environment("SF2_EGRESS_CASE")

func _initialize() -> void:
    call_deferred("run_egress")

func state() -> Dictionary:
    var s := super.state()
    if recording_retry and retry_entry.is_empty() and s.has("stage"):
        retry_entry = s.duplicate(true)
    return s

func collect_retreat(facts: Dictionary) -> void:
    for row in facts.get("observations", []):
        if (str(row.Kind).begins_with("retreat-") or row.Kind in ["battle-initialized", "battle-unlock-cleared", "battle-completed-set", "defeat-recovered", "after-turn"]) and not retreat_seen.has(row.Sequence):
            retreat_seen[row.Sequence] = true
            if retreat_events.size() < 100: retreat_events.append(row)
            else: failures.append("retreat-event-bound")

func phase_wait(phase: String) -> Dictionary:
    for frame in range(2000):
        await process_frame
        var s := state()
        if not s.has("stage") or s.get("failure") != null: return s
        if s.get("retreat", {}).get("phase") == phase: return s
    failures.append("retreat-phase-timeout:"+phase)
    return state()

func run_egress() -> void:
    route_scope = "Battle02 EGRESS: initial controlled learned spell; paid text/effect/close; live field return and walking retry"
    root.size = Vector2i(960, 640)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    var s := state()
    if s.get("failure") != null or s.get("sessionId") == null:
        failures.append("startup:"+str(s.get("failure")))
        samples.append({"label":"startup-failed", "state":s})
        await _finish()
        return
    castle_identity = s.sessionId
    s = await map_settle()
    for node in host.find_children("*", "", true, false):
        if node.has_method("RepeatRetreatDeliveryObservation"): battle_view = node
    if battle_view == null:
        failures.append("battle-view-unavailable")
        await _finish()
        return
    battle_view.connect("SessionFactsObserved", collect_retreat)
    battle_initial = s.duplicate(true)
    # Other initially queued allies use real STAY; no actor/queue override.
    for action in range(16):
        if s.get("actor") == "ally-0": break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        await key(KEY_ENTER)
        s = await map_settle()
    _check(s.get("actor") == "ally-0", "live Bowie control")
    if not failures.is_empty():
        samples.append({"label":"no-caster", "state":s})
        await _finish()
        return
    var before := s.duplicate(true)
    samples.append({"label":"before-selection", "state":before})
    await key(KEY_ENTER)
    await key(KEY_H)
    s = state()
    if egress_case == "mp7":
        _check(s.failure == "insufficient-mp", "MP7 rejected")
        _check(s.actors.filter(func(a): return a.id == "ally-0")[0].mp == 7, "rejection does not pay")
        _check(not s.retreat.active and s.mainSeed == before.mainSeed and s.thinkingSeed == before.thinkingSeed, "rejection has no retreat or RNG")
        samples.append({"label":"mp7-rejected", "state":s})
        await _finish()
        return
    _check(s.failure == null and s.stage == "CommitReady" and s.target == "ally-0", "actual learned EGRESS/self selection")
    await key(KEY_ESCAPE)
    s = await map_settle()
    _check(s.actors.filter(func(a): return a.id == "ally-0")[0].mp == 8 and s.mainSeed == before.mainSeed and s.thinkingSeed == before.thinkingSeed, "cancel cannot pay or draw")
    await key(KEY_ENTER)
    await key(KEY_H)
    await key(KEY_ENTER)
    s = await phase_wait("Text")
    for frame in range(30):
        if s.retreat.revealed: break
        await process_frame
        s = state()
    _check(s.actors.filter(func(a): return a.id == "ally-0")[0].mp == 0, "8 MP paid before feedback")
    _check(s.retreat.revealed and s.retreat.textVisible and s.retreat.text.contains("EGRESS"), "actual text delivered")
    _check(s.mainSeed == before.mainSeed and s.thinkingSeed == before.thinkingSeed and s.queueCursor == before.queueCursor, "payment bypasses ordinary suffix")
    samples.append({"label":"paid-text", "state":s})
    battle_view.call("RepeatRetreatDeliveryObservation")
    var duplicate := state()
    _check(duplicate.failure == "stale-or-wrong-retreat-wait" and duplicate.revision == s.revision, "duplicate text delivery rejected without payment")
    # Focus signal and process suspension retain the pending text and paid resource.
    root.emit_signal("focus_exited")
    await key(KEY_ENTER)
    for frame in range(20): await process_frame
    var suspended := state()
    _check(suspended.revision == s.revision and suspended.retreat.suspended, "focus loss holds retreat input")
    samples.append({"label":"focus-held", "state":suspended})
    root.emit_signal("focus_entered")
    await key(KEY_ENTER)
    s = await phase_wait("Effect")
    samples.append({"label":"actual-effect", "state":s})
    var elapsed: float = s.retreat.elapsed
    battle_view.process_mode = Node.PROCESS_MODE_DISABLED
    for frame in range(20): await process_frame
    var paused := state()
    _check(paused.revision == s.revision and paused.retreat.elapsed == elapsed, "suspension cannot complete effect")
    battle_view.process_mode = Node.PROCESS_MODE_INHERIT
    s = await phase_wait("Close")
    samples.append({"label":"actual-close", "state":s})
    battle_view.call("RepeatRetreatDeliveryObservation")
    duplicate = state()
    _check(duplicate.failure == "stale-or-wrong-retreat-wait" and duplicate.revision == s.revision, "duplicate effect cannot close")
    # The real view consumes close next; no observer submits a successful completion.
    for frame in range(60):
        await process_frame
        s = state()
        if not s.has("stage"): break
    s = await map_settle()
    samples.append({"label":"usable-return", "state":s})
    if s.has("stage") or s.get("failure") != null:
        failures.append("return-failed:"+str(s.get("failure")))
        events.append({"retreat":retreat_events})
        await _finish()
        return
    _check(s.sessionId == castle_identity and s.canWaitAtInput and s.map == "map-3", "same session usable admitted savepoint")
    _check(not numeric_contains(s.flags, 502) and numeric_contains(s.flags, 402) and s.gold == before.gold, "no completion or defeat penalty")
    _check(s.party.filter(func(a): return a.Actor.Value == "ally-0")[0].Mp == 8, "later exploration recovery is separate from payment")
    var returned := s.duplicate(true)
    var position := player(s).duplicate()
    await key(KEY_DOWN)
    s = await map_settle()
    _check(player(s).y != position.y, "real usable field movement")
    recording_retry = true
    s = await walk_field(s, Vector2i(32, 15), "church-door")
    s = await walk_field(s, Vector2i(0, -1), "west-exit", "map-66")
    samples.append({"label":"same-session-reentry", "state":s, "firstEntry":retry_entry})
    _check(issue == "" and s.has("stage") and s.get("stage") == "Movement" and s.sessionId == castle_identity, "walking Battle02 retry with usable control")
    if s.has("stage"):
        for enemy in s.actors.filter(func(a): return str(a.id).begins_with("enemy-")):
            var fresh: Dictionary = retry_entry.actors.filter(func(a): return a.id == enemy.id)[0]
            var baseline: Dictionary = battle_initial.actors.filter(func(a): return a.id == enemy.id)[0]
            _check(fresh.hp == baseline.maxHp and fresh.anchorX == baseline.anchorX and fresh.anchorY == baseline.anchorY, "fresh enemy "+enemy.id)
        for member in returned.party:
            var actual: Dictionary = s.actors.filter(func(a): return a.id == member.Actor.Value)[0]
            _check(actual.exp == member.Exp and actual.kills == member.Kills and actual.defeats == member.Defeats and actual.items == member.SourceLoadout.Items and actual.spells == member.SourceLoadout.Spells, "carried party "+actual.id)
            _check(actual.hp == 0 if member.Hp == 0 else actual.hp == actual.maxHp, "eligible retry recovery "+actual.id)
            if member.Progress != null:
                _check(actual.level == member.Progress.Level and actual.maxHp == member.Progress.MaxHp and actual.maxMp == member.Progress.MaxMp, "carried growth "+actual.id)
        _check(s.round == 1 and s.gold == returned.gold, "fresh round and carried gold")
    var payments: Array = retreat_events.filter(func(e): return e.Kind == "retreat-mp-paid")
    var returns: Array = retreat_events.filter(func(e): return e.Kind == "retreat-exploration-entry")
    _check(payments.size() == 1 and returns.size() == 1, "exactly one payment and return")
    if payments.size() == 1 and returns.size() == 1:
        _check(not retreat_events.any(func(e): return e.Kind == "after-turn" and e.Sequence > payments[0].Sequence and e.Sequence < returns[0].Sequence), "retreat skips after-turn")
    _check(not retreat_events.any(func(e): return e.Kind in ["defeat-recovered", "battle-completed-set"]), "no ordinary outcome suffix")
    events.append({"retreat":retreat_events})
    if issue != "": failures.append(issue)
    await _finish()
