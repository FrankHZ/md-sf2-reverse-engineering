extends "res://probes/engine_objective_guidance_observation.gd"

# Three representative ordinary consumers. Existing source-route navigation is observation only.
var issue := ""
var saw_hidden_scene := false

func focus_native() -> void:
    if not root.has_focus():
        root.grab_focus(); await frames(6)

func key(code: int) -> void:
    await focus_native()
    await super.key(code)

func _initialize() -> void:
    create_timer(210 if case_name in ["voyage", "prison"] else 60).timeout.connect(func():
        check(false, "bounded representative observation deadline"); finish())
    call_deferred("run")

func state() -> Dictionary:
    var field := find("ExplorationSessionView")
    if field == null: return {}
    return field.call("ReadCaptureState", false, false)

func snap(label: String) -> void:
    var s := state()
    samples.append({"label":label, "map":s.get("map"), "flags":s.get("flags"), "wait":s.get("wait"),
        "player":[player(s).get("x"),player(s).get("y")], "guidance":guidance(), "failure":s.get("failure"),
        "rootMenuVisible":s.get("fieldMenu", {}).get("visible"), "locale":applied().Language, "window":window(),
        "focused":s.get("focused"), "fieldVisible":(find("ExplorationSessionView") as Control).is_visible_in_tree(), "rootSize":str(root.size)})
    print(label, " ", s.get("map"), " ", guidance().get("objectiveKey"))

func expect(goal: String, label: String) -> void:
    await focus_native()
    await frames(3)
    check(state().get("failure") == null, label + ": no adapter/session failure")
    check(guidance().get("visible", false) and guidance().get("objectiveKey") == "guidance." + goal, label + ": live goal " + goal)
    check(not "Message unavailable" in str(guidance().get("text")), label + ": localized destination has valid operands")
    snap(label)

func read_goal(expected: bool) -> void:
    await key(KEY_ESCAPE)
    var s := state()
    check(s.fieldMenu.visible and guidance().get("visible", false) == expected, "root menu retains the independent goal")
    check(not "Message unavailable" in guidance().text, "ordinary objective has correct optional-name arity")
    var revision: int = s.revision
    await frames(6)
    check(state().revision == revision and state().flags == s.flags and state().party == s.party,
        "reading submits no gameplay or story change")
    snap("menu-goal")
    await key(KEY_ESCAPE)

func settle(label: String) -> Dictionary:
    var checkpoint := Time.get_ticks_msec()
    var last_text := -1
    for i in range(14000):
        await focus_native()
        await frames(3)
        var s := state()
        if s.is_empty(): continue
        if s.get("failure") != null: issue = label + ":" + str(s.failure); check(false, issue); return s
        if s.get("canWaitAtInput", false) and not player(s).get("moving", true): return s
        if s.get("textId") != null and int(s.textId) != last_text:
            last_text = int(s.textId); checkpoint = Time.get_ticks_msec()
            check(not guidance().get("visible", false), label + ": scene hides guidance")
            saw_hidden_scene = true
        if s.get("canWaitForChoice", false): return s
        if s.get("canWaitForText", false) or s.get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]:
            await key(KEY_ENTER)
        if Time.get_ticks_msec() - checkpoint > 30000: break
    issue = label + ": checkpoint exceeded30s"; check(false, issue); snap(label); return state()

func ordinary_new_game() -> void:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_ENTER)
    if await wait_slots("new-game-slots"):
        await key(KEY_ENTER); await frames(4); await key(KEY_ENTER)
        if await wait_slots("confirm-new-game"): await key(KEY_DOWN); await key(KEY_ENTER)
    for i in range(1800):
        if find("ExplorationSessionView") != null: break
        await process_frame
    await settle("new-game")

func opening() -> void:
    await ordinary_new_game()
    await expect("start-day", "ordinary-new-game-home")
    await read_goal(true)
    await set_guidance(false)
    check(not guidance().visible, "existing Off controls expanded field aid")
    await read_goal(false)
    await set_guidance(true)
    await walk(Vector2i(54,3), "home-exit", "map-3")
    await walk(Vector2i(4,4), "morning-conversation")
    await expect("attend-school", "mother-discloses-school")
    await read_goal(true)
    check(saw_hidden_scene, "real opening dialogue owned presentation")

func prison() -> void:
    var started: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(started == "started", "admit compatible pre-session prison state: " + started)
    root.grab_focus(); await frames(10)
    check(root.has_focus(), "native controlled consumer owns focus after admission")
    await settle("prison-start")
    await expect("leave-cells", "prison-before-doors")
    await read_goal(true)
    await edge(KEY_SHIFT, true)
    await walk(Vector2i(53,5), "actual-prison-doors")
    if issue != "": return
    check(661 in state().flags, "real door caller commits661")
    await expect("escape-galam", "doors-open-stairs-next")
    check(guidance().routeKey == "guidance.approach-stairs", "no shortcut before actual warning")
    await walk(Vector2i(43,9), "actual-stairs-warning", "map-17")
    if issue != "": return
    check(662 in state().flags and guidance().routeKey == "guidance.use-shortcut", "real warning discloses shortcut")
    snap("shortcut-disclosed")
    await walk(Vector2i(57,5), "actual-shortcut", "map-17")
    if issue != "": return
    check(73 in state().flags and guidance().routeKey == "guidance.tunnel-kitchen", "actual tunnel stage has kitchen advice")
    await walk(Vector2i(2,24), "kitchen-warp", "map-17")
    await edge(KEY_SHIFT, false)
    check(guidance().routeKey == "guidance.leave-kitchen", "same-map kitchen arrival replaces tunnel advice")
    await read_goal(true)
    snap("kitchen-control")

func wait_choice() -> void:
    for i in range(1500):
        await focus_native()
        var s := state()
        if s.get("failure") != null: check(false, "departure prompt failure"); return
        if s.get("canWaitForChoice", false):
            check(not guidance().visible, "departure prompt hides destination"); return
        if s.get("canWaitForText", false) or s.get("wait") in ["DialogueWait", "FieldTextWait", "W1TextWait"]: await key(KEY_ENTER)
        else: await frames(3)
    check(false, "no actual departure choice")

func voyage() -> void:
    var save_bytes := FileAccess.get_file_as_string(slot_path)
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if await wait_slots("continue-slots"): await key(KEY_ENTER)
    for i in range(1800):
        if find("ExplorationSessionView") != null: break
        await process_frame
    await settle("actual-map58-continue")
    var envelope: Dictionary = JSON.parse_string(save_bytes)
    var data: Dictionary = JSON.parse_string(Marshalls.base64_to_utf8(envelope.payload))
    check(normalized(restored.get("progress")) == data.data, "real Map58 Continue retains every captured progress field")
    await expect("return-from-tower", "map58-continue-goal")
    await read_goal(true)
    await edge(KEY_SHIFT, true)
    for step in [[-1,37,"map-60"],[21,34,"map-60"],[38,2,"map-60"],[1,30,"map-60"],[13,1,"map-60"],
        [53,1,"map-57"],[-1,19,"map-40"],[-1,31,"map-21"],[3,16,"map-20"],[6,37,"map-19"]]:
        await walk(Vector2i(step[0],step[1]), "reverse-tower-" + str(step), step[2])
        if issue != "": return
        await expect("return-from-tower", "tower-return-control")
    await walk(Vector2i(16,5), "castle-warning")
    if issue != "": return
    await expect("escape-earthquake", "escape-disclosed")
    await walk(Vector2i(15,5), "warning-back"); await walk(Vector2i(16,5), "warning-revisit")
    await walk(Vector2i(25,13), "castle-safe-group")
    await walk(Vector2i(-1,31), "quake-town-entry", "map-4")
    if issue != "": return
    await expect("reach-docks", "earthquake-discloses-east-docks")
    await read_goal(true)
    await walk(Vector2i(50,24), "docks-entry", "map-44")
    await expect("board-ship", "docks-goal")
    await walk(Vector2i(0,25), "quake-revisit", "map-4")
    await expect("reach-docks", "quake-revisit-no-stale")
    await walk(Vector2i(50,24), "docks-reentry", "map-44")
    await walk(Vector2i(11,14), "dock-man-approach")
    await key(KEY_LEFT); await settle("face-dock-man"); await edge(KEY_SHIFT, false)
    var flags: Array = state().flags.duplicate()
    await key(KEY_ENTER); await wait_choice(); await key(KEY_RIGHT); await key(KEY_ENTER); await settle("departure-no")
    await expect("board-ship", "No-keeps-disclosed-goal")
    check(state().flags == flags, "No consumes no departure progress")
    await key(KEY_ENTER); await wait_choice(); await key(KEY_X); await settle("departure-cancel")
    await expect("board-ship", "Cancel-keeps-disclosed-goal")
    check(state().flags == flags, "Cancel consumes no departure progress")
    await key(KEY_ENTER); await wait_choice(); await key(KEY_LEFT); await edge(KEY_SHIFT, true); await key(KEY_ENTER)
    await settle("boarding-and-voyage"); await edge(KEY_SHIFT, false)
    var final := state()
    check(final.get("map") == "map-14" and 700 in final.flags and not 74 in final.flags and not 640 in final.flags,
        "actual boarding consumes departure and reaches damaged ship")
    check(not guidance().get("visible", false) and guidance().get("objectiveKey") == null, "boarding removes all previous guidance")
    await read_goal(false)
    await key(KEY_DOWN); await settle("ship-control")
    check(not guidance().visible and state().get("failure") == null, "successor remains playable without old goal")
    check(FileAccess.get_file_as_string(slot_path) == save_bytes, "ordinary route never overwrites copied Map58 slot")
    snap("boarded-and-cleared")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1578-"):
        push_error("Use explicit output and a disposable issue1578-* namespace"); quit(2); return
    var namespace_root := OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE"))
    preference_path = namespace_root.path_join("sf2-preferences/input.json")
    slot_path = namespace_root.path_join("sf2-opening/slot1.json")
    node_added.connect(observe_node)
    root.size = Vector2i(1920,1080); root.position = Vector2i(-20000,-20000)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus(); await frames(10)
    check(root.has_focus(), "actual native input owner")
    if case_name == "opening": await opening()
    elif case_name == "prison": await prison()
    elif case_name == "voyage": await voyage()
    else: check(false, "unknown representative case")
    if issue != "": check(false, issue)
    finish()

func walk(goal: Vector2i, label: String, expected_map: String = "") -> Dictionary:
    var s := state()
    var view := find("ExplorationSessionView")
    var origin_map: String = s.map
    var words: Array = []
    for y in range(0,64,4):
        var region: Dictionary = view.call("ReadLayoutRegion",0,y,64,4)
        words.append_array(region.words)
    var rejected := {}
    for move in range(256):
        if issue != "": return s
        var origin := Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if origin==goal: return s
        var queue: Array[Vector2i] = [origin]
        var paths := {origin:[]}
        var selected: Array = []
        while not queue.is_empty():
            var at: Vector2i = queue.pop_front()
            for d in [Vector2i.DOWN,Vector2i.RIGHT,Vector2i.UP,Vector2i.LEFT]:
                var n: Vector2i = at+d
                if n.x<0 or n.y<0 or n.x>=64 or n.y>=64: continue
                var collision: int = int(words[at.y*64+at.x])&0xC000
                if d.x!=0 and collision in [0x4000,0x8000]:
                    var stair_y: int = at.y + (d.x if collision==0x4000 else -d.x)
                    if stair_y>=0 and stair_y<64 and (int(words[stair_y*64+n.x])&0xC000)==collision: n.y=stair_y
                if paths.has(n) or rejected.has(str([at,d])): continue
                var word: int = words[n.y*64+n.x]
                var target: bool = (goal.x<0 or n.x==goal.x) and (goal.y<0 or n.y==goal.y)
                if target and (goal.x<0 or goal.y<0): target=(word&0x3C00)==0x1000
                if (word&0xC000)==0xC000 and (word&0x3C00)!=0x400 and not target: continue
                if (word&0x3C00)==0x1000 and not target: continue
                if s.entities.any(func(e): return e.id!="entity-0" and e.Visible and (int(e.flagsA)&0x80)!=0 and absi(int(e.x)-n.x*384)<256 and absi(int(e.y)-n.y*384)<256): continue
                paths[n]=paths[at]+[d]
                if target:
                    selected=paths[n]
                    queue.clear()
                    break
                queue.append(n)
        if selected.is_empty():
            issue=label+":no-live-layout-path"
            return s
        var first: Vector2i=selected[0]
        await key(KEY_RIGHT if first.x>0 else KEY_LEFT if first.x<0 else KEY_DOWN if first.y>0 else KEY_UP)
        s=await settle(label)
        if issue!="": return s
        if s.has("stage") or s.map!=origin_map or (expected_map!="" and selected.size()==1):
            if s.get("map")!=expected_map and not s.has("stage"): issue=label+":unexpected-map"
            snap(label)
            return s
        var reached := Vector2i(int(player(s).x)/384,int(player(s).y)/384)
        if reached==origin: rejected[str([origin,first])]=true
        if reached==goal:
            snap(label)
            return s
    issue=label+":navigation-bound"
    return s
