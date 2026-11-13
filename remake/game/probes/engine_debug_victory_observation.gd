extends "res://probes/host_ui_observation.gd"

# Connected Battle02. All inputs after the documented initial approach use the host.
var receipts: Array = []
var sequences := {}
var board: Dictionary = {}

func watch(node: Node) -> void:
    if node.has_signal("SessionResultObserved"):
        node.connect("SessionResultObserved", func(raw: String):
            var r: Dictionary = JSON.parse_string(raw)
            if node.name == "BattleSessionView" and board.is_empty() and r.stopReason == "PlayerInput":
                var b := observation(node, "ReadObservationJson")
                if b.get("stage") == "Movement":
                    board = {"gold": b.gold, "allies": {}}
                    for actor in b.actors:
                        if str(actor.id).begins_with("ally-"):
                            board.allies[actor.id] = [actor.exp, actor.kills, actor.defeats]
            for event in r.observations:
                if event.Kind in ["debug-battle-victory", "outcome-program-started", "battle-completed-set", "battle-returned", "kills", "gold"] and not sequences.has(event.Sequence):
                    sequences[event.Sequence] = true
                    receipts.append([event.Kind, event.Detail]))

func select_setting(index: int) -> void:
    for i in range(settings().labels.size()):
        if settings().selected == index: return
        await key(KEY_DOWN)
    check(false, "setting reachable " + str(index))

func run() -> void:
    if not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1720-") or output.is_empty():
        push_error("Use fresh issue1720-* preferences and an explicit output"); quit(2); return
    root.size = Vector2i(1920, 1080)
    node_added.connect(watch)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(5)
    await open_witch_settings()
    if find("SettingsView") == null: finish(); return
    check("installed artwork" in settings().message or "安装了高清素材" in settings().message, "coverage describes installed variants")
    var on := case_name == "on"
    check(not settings().applied.DebugAutoVictory, "old preference defaults Off")
    await select_setting(12)
    check("自动获胜" in settings().labels[12] or "auto-win" in settings().labels[12], "bilingual label")
    await key(KEY_ENTER)
    check(settings().draft.DebugAutoVictory and not settings().applied.DebugAutoVictory, "draft is isolated")
    await key(KEY_ESCAPE)
    if on:
        await open_witch_settings()
        check(not settings().draft.DebugAutoVictory, "cancel discards toggle")
        await select_setting(12); await key(KEY_RIGHT); await select_setting(7); await key(KEY_ENTER)
    var applied: Dictionary = observation(host, "ReadSettingsHostObservationJson").applied
    check(applied.DebugAutoVictory == on and not applied.ShowDebugDiagnostics, "independent applied setting")
    var admitted: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    if admitted != "started": check(false, "controlled field admission: " + admitted); finish(); return
    for i in range(700):
        await frames(4)
        var b := host.get_node_or_null("BattleSessionView")
        var f := host.get_node_or_null("ExplorationSessionView")
        var s := observation(f, "ReadObservationJson")
        if s.get("failure") != null: check(false, "field failure " + str(s.failure)); break
        if not on and not board.is_empty() and b != null:
            await frames(12)
            var playing := observation(b, "ReadObservationJson")
            check(playing.get("stage") == "Movement" and not playing.actors.is_empty() and receipts.is_empty(), "Off keeps playable turn")
            samples.append(board); finish(); return
        if on and not board.is_empty() and s.get("canWaitAtInput",false) and s.get("flags", []).any(func(value): return int(value) == 502):
            check(s.map == "map-66" and not s.flags.any(func(value): return int(value) == 402), "normal Battle02 return clears unlock and sets completion")
            check(s.gold == board.gold, "no synthetic gold")
            for ally in s.party:
                if board.allies.has(ally.Actor.Value):
                    check(board.allies[ally.Actor.Value] == [ally.Exp, ally.Kills, ally.Defeats], "earned accounting " + ally.Actor.Value)
            check(receipts.count(["debug-battle-victory",null]) == 1 and ["outcome-program-started","Victory"] in receipts, "one victory through ordinary return")
            check(s.audio.error == null, "return audio healthy")
            samples.append({"board":board, "map":s.map, "gold":s.gold, "receipts":receipts})
            finish(); return
        if s.get("canWaitForText", false): await key(KEY_ENTER)
    samples.append({"board":board, "receipts":receipts, "wait":state().get("wait"), "map":state().get("map"), "flags":state().get("flags"), "idle":state().get("canWaitAtInput")})
    check(false, "expected settled campaign boundary"); finish()
