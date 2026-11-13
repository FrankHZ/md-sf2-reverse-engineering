extends RefCounted

# Navigate actual visible controls by their stable meaning using the installed keys.
static func tap(tree: SceneTree, action: String) -> void:
    var key: InputEventKey = InputMap.action_get_events("sf2_" + action).filter(func(e): return e is InputEventKey)[0]
    for pressed in [true, false]:
        var e := InputEventKey.new(); e.keycode = key.keycode; e.pressed = pressed
        Input.parse_input_event(e)
        await tree.process_frame; await tree.process_frame

static func row(view: Node, id: String) -> bool:
    if view == null: return false
    var obs: Dictionary = JSON.parse_string(view.call("ReadSettingsObservationJson"))
    if not id in obs.rows: return false
    for step in obs.rows.size():
        if JSON.parse_string(view.call("ReadSettingsObservationJson")).focus == id: return true
        await tap(view.get_tree(), "down")
    return false

static func setting(view: Node, id: String) -> bool:
    if view == null: return false
    var tree := view.get_tree()
    var obs: Dictionary = JSON.parse_string(view.call("ReadSettingsObservationJson"))
    var sections := {"language":"display","graphics":"display","text":"display","speed":"display",
        "guidance":"assist","flash":"assist","defaults":"assist","convention":"input","help":"input",
        "edit":"input","reset":"input","diagnostics":"debug","victory":"debug"}
    if not id in obs.rows:
        if obs.page != "Root": await tap(tree, "cancel")
        if sections.has(id):
            if not await row(view, sections[id]): return false
            await tap(tree, "confirm")
    return await row(view, id)
