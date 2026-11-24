extends "res://probes/engine_equipment_observation.gd"

const CASTER := "ally-1"
const TARGET := "ally-0"
const COSTS := [3, 6, 10]
var rank_ := int(OS.get_environment("SF2_DETOX_RANK"))
var original := OS.get_environment("SF2_DETOX_HD") != "1"
var effective := OS.get_environment("SF2_DETOX_EFFECTIVE") == "1"
var suppressed_cast := OS.get_environment("SF2_DETOX_SILENCED") == "1"
var frames_seen := {}
var layers_seen := {}
var result_seen := false
var close_seen := false
var first_mote := false
var animation_token = null
var observed_flashes := false
var left_arc_seen := false

func unit(state: Dictionary, id: String) -> Dictionary:
    return state.actors.filter(func(row): return row.id == id)[0]

func status(state: Dictionary, id: String) -> int:
    return int(state.statuses.filter(func(row): return row.actor == id)[0].status)

func observe_art(s: Dictionary) -> void:
    var art: Dictionary = s.detox
    if art.flash and not observed_flashes:
        observed_flashes = true
        var ally := view.find_child("Ally", true, false) as Sprite2D
        check((ally.modulate == Color.WHITE) == s.reducedFlash, "Reduced Flash controls only the burst tint")
    close_seen = close_seen or (s.phase == "SpellStop" and art.closing and not art.sprites.is_empty())
    var effect := view.find_child("DetoxEffect", true, false)
    var canvas := effect.get_parent()
    for row in art.sprites:
        var sprite := effect.get_node(NodePath(row.name)) as Sprite2D
        if row.name == "Mote" and not first_mote:
            first_mote = true
            check(sprite.position == Vector2(166, 79) and sprite.z_index == 4 and not sprite.flip_h,
                "source right arc entry and foreground mote")
        if row.name == "Mote" and not left_arc_seen and row.x < 120:
            left_arc_seen = true
            check(row.x >= 32 and row.y >= 48 and row.y <= 80, "source left arc bounds")
            samples.append({"leftArc":row})
        var witnessed := "%s:%s" % [row.resource, row.z]
        if frames_seen.has(witnessed): continue
        frames_seen[witnessed] = row
        var frame := int(row.resource.trim_prefix("detox/frame-"))
        var size := 16 if frame in [2, 3] else (24 if frame in [4, 5] else 8)
        var expected := Vector2(size, size)
        check(sprite.visible and sprite.texture != null, "mounted DETOX Sprite2D: " + row.resource)
        if sprite.texture != null:
            check(row.hd == (not original) and sprite.texture.get_size() == expected * (1 if original else 4)
                and sprite.texture.get_size() * sprite.scale == expected, "complete bank and logical size")
        check(sprite.position == Vector2(row.x, row.y) and sprite.z_index == row.z,
            "readback matches real node")
        if row.name != "Mote":
            layers_seen[int(row.z)] = true
            check(canvas.get_node("Enemy").get_index() > effect.get_index(),
                "back trail sorts behind the close-up actors")

func settle() -> Dictionary:
    for _frame in range(2400):
        await process_frame
        var current := scene()
        var s: Dictionary = current.scene
        if current.failure != null or s.error != null:
            check(false, "native battle or scene error: " + str([current.failure, s.error]))
            return {}
        if s.phase == null and current.hasBattleControl and current.movement == null:
            var b := board()
            if b.stage == "Movement" and b.stopReason == "PlayerInput": return b
        if s.detox != null:
            check(not suppressed_cast and s.spell != null and s.spell.Value == "detox", "DETOX Magic owns playback")
            check(s.spellAnimationSelector == 6 + 32 * (rank_ - 1) and s.fairySprites.is_empty(),
                "rank selector with exclusive DETOX artwork")
            observe_art(s)
            if s.phase == "ActionAnimation" and animation_token == null and not s.detox.sprites.is_empty():
                animation_token = s.waitToken
                check(s.displayedAlly == CASTER and s.allyVisible and s.weaponVisible and s.backgroundVisible,
                    "cast pose and mounted scene remain visible")
                await key(KEY_ENTER)
                check(scene().scene.waitToken == animation_token, "early Confirm cannot complete the cast")
            if s.phase == "ResultMessage" and not result_seen:
                result_seen = true
                check(s.displayedAlly == TARGET and s.message.length() > 0 and not s.detox.sprites.is_empty(),
                    "effect overlaps the selected recipient's result")
                check((s.reactionKind != "Ineffective") == effective, "effective or ineffective result")
                samples.append({"phase":s.phase, "result":s.message, "kind":s.reactionKind,
                    "target":s.displayedAlly, "status":status(board(), TARGET)})
        if s.phase == "SilencedCastMessage" and not result_seen:
            result_seen = true
            check(s.silencedSpell.Value == "detox" and s.detox == null and not s.message.is_empty(), "silence message")
            samples.append({"phase":s.phase, "result":s.message})
        if s.requiresAcknowledgement == true and not s.suppressed:
            await key(KEY_ENTER)
    check(false, "bounded return to ordinary movement")
    samples.append({"deadline":scene(), "board":board()})
    return {}

func select_rank() -> Dictionary:
    await key(KEY_H)
    for _row in range(rank_): await key(KEY_DOWN)
    await key(KEY_ENTER)
    for _target in range(2):
        if board().target == TARGET: break
        await key(KEY_TAB)
    var b := board()
    check(b.stage == "CommitReady" and b.target == TARGET and b.spell != null
        and b.spell.Value == "detox" and b.spell.Level == rank_, "ordinary rank and recipient selection")
    if not failures.is_empty(): samples.append({"selection":b})
    return b

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await process_frame
    var admitted = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_DETOX_START"))
    view = host.find_child("BattleSessionView", true, false)
    check(admitted == "started" and view != null, "controlled private battle mount: " + str(admitted))
    if not failures.is_empty():
        samples.append({"startupLabels":host.find_children("*", "Label", true, false).map(func(node): return node.text)})
        finish(); return
    view.connect("SessionResultObserved", result_observed)
    var before := await settle()
    for _turn in range(4):
        if before.is_empty() or before.actor == CASTER: break
        await key(KEY_ENTER)
        await key(KEY_SPACE)
        before = await settle()
    check(not before.is_empty() and before.actor == CASTER, "ordinary first Sarah turn")
    if not failures.is_empty(): finish(); return

    await key(KEY_ENTER)
    var selected := await select_rank()
    if not failures.is_empty(): finish(); return
    if rank_ == 1 and effective:
        await key(KEY_ESCAPE)
        var canceled := board()
        check(unit(canceled, CASTER).mp == unit(before, CASTER).mp and status(canceled, TARGET) == status(before, TARGET)
            and canceled.mainSeed == before.mainSeed and scene().scene.detox == null, "cancel is free and never mounts playback")
        if canceled.menu.page == "Magic": await key(KEY_ESCAPE)
        selected = await select_rank()
        if not failures.is_empty(): finish(); return
    check(unit(selected, CASTER).mp == unit(before, CASTER).mp, "target selection is free")
    var caster := unit(before, CASTER)
    var target := unit(before, TARGET)
    var distance := int(abs(caster.x-target.x) + abs(caster.y-target.y))
    check(distance == (2 if rank_ == 2 else 1), "observed source range")
    events.clear()
    await key(KEY_ENTER)
    var after := await settle()
    if after.is_empty(): finish(); return
    var charge := events.filter(func(e): return e.Kind == "mp" and e.Actor.Value == CASTER)
    var award := events.filter(func(e): return e.Kind == "exp" and e.Actor.Value == CASTER)
    var cured := events.filter(func(e): return e.Kind == "status" and e.Actor.Value == TARGET)
    var audio: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
    var cue: Array = audio.receipts.filter(func(e): return e.Command == 77 and e.Operation == "started")
    var recovery: Array = audio.receipts.filter(func(e): return e.Command == 113 and e.Operation == "started")
    var gain: int = int(unit(after, CASTER).exp) - int(unit(before, CASTER).exp)
    check(charge.size() == 1 and charge[0].Before-charge[0].After == COSTS[rank_-1]
        and unit(after, CASTER).mp == unit(before, CASTER).mp-COSTS[rank_-1], "one rank-specific MP debit")
    check(cured.size() == (1 if effective else 0) and status(after, TARGET) == (0 if effective else status(before, TARGET))
        and unit(after, TARGET).hp == unit(before, TARGET).hp, "only the appropriate status write, with stable HP")
    check(gain == (0 if suppressed_cast else (5 if effective else 1)) and award.size() == (0 if suppressed_cast else 1),
        "source-scoped EXP and one publication")
    check(result_seen and cue.size() == (0 if suppressed_cast else 1) and recovery.size() == (1 if effective else 0)
        and audio.error == null, "cast cue for unsilenced spells and recovery cue only for a cure")
    check((frames_seen.is_empty() and not close_seen) if suppressed_cast else
        (first_mote and left_arc_seen and close_seen and observed_flashes and layers_seen.has(0) and layers_seen.has(4)),
        "mote, both trail layers and closing playback, or suppressed cast")
    if rank_ == 1 and not suppressed_cast:
        var keys := {}
        for entry in frames_seen.values(): keys[entry.resource] = true
        check(keys.size() == 11, "all source frames rendered")
    check(before.sessionId == after.sessionId and scene().scene.detox == null, "effect retired on return")
    await key(KEY_ENTER)
    check(board().stage == "ActionChoice", "next ordinary action menu opens")
    samples.append({"rank":rank_,"startStatus":status(before,TARGET),"endStatus":status(after,TARGET),
        "mpBefore":unit(before,CASTER).mp,"mpAfter":unit(after,CASTER).mp,"exp":gain,"nextActor":after.actor,
        "observedFrames":frames_seen,"castCueCount":cue.size(),"recoveryCueCount":recovery.size(),"closed":close_seen})
    finish()

func finish() -> void:
    var output := FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"), FileAccess.WRITE)
    if output == null:
        push_error("observation output unavailable")
        quit(1)
        return
    output.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures, "samples":samples, "events":events}, "  "))
    output.close()
    host.queue_free()
    await process_frame
    await process_frame
    quit(0 if failures.is_empty() else 1)
