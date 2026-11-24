extends "res://probes/engine_battle_cursor_observation.gd"

# Ordinary Main with the public attack-spell yard, or the production Battle tab in DesignPreview.
func phase_geometry(label: String, cursor: String, decision: bool) -> bool:
    await frames(4)
    var s := state()
    var layout: Dictionary = s.menu.layout
    var b := board()
    var mounted := view.find_child("RoundOrder",true,false) != null
    samples.append({"label":label,"client":[root.size.x,root.size.y],"board":b.board,"cursor":b.cursorRole,
        "orderMounted":mounted,"viewport":s.viewport,"menu":s.menu,"failure":s.failure,"error":s.scene.error})
    if not expect(not mounted and not layout.has("roundOrder"),label+": no order node or layout entry"): return false
    if not expect(s.failure == null and s.scene.error == null and b.cursorRole == cursor and s.menu.visible == decision,
            label+": expected focus and decision visibility without errors"): return false
    if not decision: return true
    for panel in [layout.actorCard, layout.targetCard, layout.terrainPanel, layout.commandList]:
        if panel.visible and not expect(within(panel.rect,s.viewport),label+": remaining panel fits"): return false
    return expect(layout.descriptionVisible and within(layout.description,s.viewport),
        label+": normal decision footer fits")

func click_at(point: Vector2) -> void:
    await move_mouse(point)
    await mouse(point,true)
    await mouse(point,false)

func run() -> void:
    root.size = Vector2i(1920,1080)
    var preview := "--battle" in OS.get_cmdline_user_args()
    var scene := "res://probes/previews/DesignPreview.tscn" if preview else "res://Main.tscn"
    host = load(scene).instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(5)
    if host.find_child("BattleSessionView",true,false) == null:
        samples.append({"label":"startup-refusal","text":host.find_children("*","Label",true,false).map(func(n): return n.text)})
        fail("Public authored battle did not start")
        return
    bind_events()
    if not await enter_battle(): return
    if not await wait_for(func(_s): return board().settled,"initial camera"): return
    if not await phase_geometry("Movement","Movement",true): return
    if not expect(state().menu.layout.actorCard.visible,"Movement retains the acting unit card"): return
    if preview:
        finish(true)
        return

    var initial := state()
    await act("cancel")
    if not await inspect_page("Cursor") or not await phase_geometry("Free inspection","Inspection",false): return
    await move_mouse(cell_point(initial.previewX+1,initial.previewY-1))
    var inspected: Dictionary = state().inspection.cursor
    if not expect(inspected.X == initial.previewX+1 and inspected.Y == initial.previewY-1,
            "Mouse reaches the inspection cell"): return
    await act("cancel")
    if not expect(signature(state()) == signature(initial),"Inspection leaves authoritative battle state intact"): return

    await act("right")
    if not await settled(): return
    if not expect(state().previewX == initial.previewX+1,"Mapped movement reaches the adjacent legal cell"): return
    root.size = Vector2i(1600,900)
    await frames(5)
    await act("confirm")
    if not await phase_geometry("Main commands","None",true): return
    var main := state()
    var attack: Dictionary = main.menu.layout.commands[0]
    if not expect(main.menu.page == "Main" and attack.visible and attack.enabled,"Main has a live Attack row"): return
    await click_at(Vector2(attack.rect.x+attack.rect.width/2,attack.rect.y+attack.rect.height/2))
    if not await wait_for(func(_s): return board().settled,"target camera"): return
    if not await phase_geometry("Attack target","Target",true): return
    var targeting := state()
    if not expect(targeting.menu.page == "Target" and targeting.candidate != null and targeting.menu.layout.targetCard.visible,
            "Main row click enters Target with the recipient card"): return
    var target: Dictionary = targeting.actors.filter(func(a): return a.id == targeting.candidate)[0]
    await click_at(cell_point(target.x,target.y))
    if not await wait_for(func(s): return s.scene.phase != null,"target click reaches real delivery"): return
    finish(await phase_geometry("Battle delivery","None",false))
