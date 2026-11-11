extends "res://probes/engine_compact_commands_observation.gd"

# Production input and native geometry only. Initial positions belong to caller-owned starts.
var demo_case := OS.get_environment("SF2_DEMO_BATTLE_CASE")
var phases: Array = []

func state() -> Dictionary:
    var battle := host.find_child("BattleSessionView",true,false)
    var field := host.find_child("ExplorationSessionView",true,false)
    if battle != null and battle.is_visible_in_tree():
        var current: Dictionary = JSON.parse_string(battle.call("ReadObservationJson"))
        # A retained board may remain visible as the field-owned outcome's backdrop.
        if not current.has("pendingFieldReturn"):
            view = battle; return current
    view = field
    return JSON.parse_string(field.call("ReadObservationJson"))

func enter_battle() -> bool:
    var deadline := Time.get_ticks_msec()+12000
    var nudged := false
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if s.get("failure") != null:
            samples.append({"label":"entry-failure","state":s})
            return expect(false,"Entry failure: "+str(s.failure))
        if s.get("stage") == "Movement" and s.get("movement") == null: return true
        if s.get("canWaitAtInput",false) and not nudged:
            nudged = true; await act("down")
        elif s.get("wait") == "DialogueWait" or s.get("canWaitForText",false) or s.get("scene",{}).get("requiresAcknowledgement",false):
            await act("confirm")
        else: await process_frame
    samples.append({"label":"entry-timeout","state":state()})
    return expect(false,"Controlled approach did not reach ordinary player boundary")

func move_mouse(point: Vector2) -> void:
    var e := InputEventMouseMotion.new(); e.position = point; e.relative = Vector2(3,2)
    Input.parse_input_event(e); await frames(3)

func cell_point(x: int, y: int) -> Vector2:
    var b := board()
    return Vector2(b.board.x+(x+0.5)*96,b.board.y+(y+0.5)*96)

func within(r: Dictionary, parent: Dictionary) -> bool:
    return r.x >= parent.x-1 and r.y >= parent.y-1 and r.x+r.width <= parent.x+parent.width+1 and r.y+r.height <= parent.y+parent.height+1

func read_geometry(label: String) -> bool:
    await frames(4)
    var s := state()
    var m: Dictionary = s.menu.layout
    samples.append({"label":label,"menu":s.menu,"board":board(),"inspection":s.inspection})
    if not s.menu.visible: return true
    if not expect(inside(m.description) and inside(m.actor) and inside(m.targetCard.rect),label+": decision windows fit"): return false
    for r in m.descriptionLabels:
        if not expect(within(r.rect,m.description),label+": full detail/hints contained"): return false
    return true

func inspect_case() -> bool:
    var initial := state()
    var destinations: Array = board().movementRange.destinations
    for step in [["left",-1,0],["down",0,1],["up",0,-1],["right",1,0]]:
        if destinations.any(func(p):return p.X == initial.previewX+step[1] and p.Y == initial.previewY+step[2]):
            await act(step[0]); break
    if not await settled(): return false
    if not expect(state().previewX != initial.previewX or state().previewY != initial.previewY,"Real provisional movement reaches another legal cell"): return false
    await act("cancel")
    if not await settled(): return false
    if not await inspect_page("Cursor"): return false
    if not expect(state().previewX == initial.previewX and state().previewY == initial.previewY and resources(state()) == resources(initial),
            "Movement Cancel finishes real undo before inspection and preserves resources/RNG"): return false
    var baseline := signature(state())
    if not expect(state().inspection.hover.unit == initial.actor,"Cursor immediately projects actual unit"): return false
    await act("confirm")
    if not await inspect_page("Full"): return false
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    # Hover another disclosed unit; equal-width HP/MP and land effect occupy the same card.
    var unit: Dictionary = state().inspection.units.filter(func(u):return u.Actor.Value != initial.actor)[0]
    var p := cell_point(unit.Position.X,unit.Position.Y)
    await move_mouse(p)
    var h: Dictionary = state().inspection.hover
    if not expect(h.unit == unit.Actor.Value and h.land.ends_with("%"),"Pointer and directional focus disclose same unit/land"): return false
    if not expect(abs(h.hpGauge.rect.width-h.mpGauge.rect.width) < 1 and inside(h.rect),"Inspection vitals share equal readable space"): return false
    samples.append({"label":"unit-hover","inspection":state().inspection})
    await mouse(p,true); await mouse(p,false)
    if not await inspect_page("Full"): return false
    var original_cell: Dictionary = state().inspection.cursor.duplicate()
    await act("cancel")
    if not expect(state().inspection.cursor == original_cell,"Details Back restores exact inspected cell"): return false
    # Move to empty ground by ordinary direction input, then use pointer to enter options.
    await act("down")
    for i in range(6):
        if state().inspection.hover.unit == null: break
        await act("down")
    var empty: Dictionary = state().inspection.cursor
    p = cell_point(empty.X,empty.Y)
    await move_mouse(p)
    if not expect(state().inspection.hover.unit == null and not state().inspection.hover.land.is_empty(),"Empty ground hover has terrain and coordinates"): return false
    samples.append({"label":"ground-hover","inspection":state().inspection})
    await mouse(p,true); await mouse(p,false)
    if not await inspect_page("Options"): return false
    if not list_geometry(state().inspection.options.commandList,true): return false
    await act("confirm")
    if not await inspect_page("Members"): return false
    if not state().inspection.members.is_empty():
        await act("down")
        var member_index: int = state().inspection.memberIndex
        await act("confirm")
        if not await inspect_page("MemberFull"): return false
        await act("cancel")
        if not expect(state().inspection.page == "Members" and state().inspection.memberIndex == member_index,"Member detail Back restores exact roster row"): return false
        samples.append({"label":"member-return","inspection":state().inspection})
    else:
        if not expect(not state().inspection.roster.text.is_empty(),"Standalone start explicitly explains absent joined roster"): return false
    await act("cancel")
    await act("down"); await act("confirm")
    if not await inspect_page("Map"): return false
    var overview: Dictionary = state().inspection.overview
    if not expect(overview.terrainCellCount == initial.mapWidth*initial.mapHeight and overview.pointerActions,"Whole-board map draws all terrain and disclosed units"): return false
    samples.append({"label":"overview","inspection":state().inspection})
    var marker: Dictionary = overview.units[0]
    var map_rect: Dictionary = overview.map
    var panel: Rect2 = host.find_child("Overview",true,false).get_global_rect()
    var map_point := panel.position+Vector2(map_rect.x+(marker.Position.X+0.5)*map_rect.width/initial.mapWidth,
        map_rect.y+(marker.Position.Y+0.5)*map_rect.height/initial.mapHeight)
    await move_mouse(map_point); await mouse(map_point,true); await mouse(map_point,false)
    if not expect(state().inspection.page == "Cursor" and state().inspection.cursor == marker.Position,"Map pointer locates disclosed unit without acting"): return false
    await cursor_to(empty.X,empty.Y); await act("confirm"); await act("confirm")
    if not await inspect_page("Map"): return false
    await act("down"); await act("cancel")
    if not await inspect_page("Options"): return false
    await act("down"); await act("confirm")
    var settings := host.find_child("SettingsView",true,false)
    if not expect(settings != null and JSON.parse_string(settings.call("ReadSettingsObservationJson")).battleOnly,"Actual Speed caller opens bounded settings"): return false
    await act("cancel")
    if not expect(state().inspection.page == "Options" and state().inspection.optionIndex == 2,"Speed Back restores same option"): return false
    if not expect(signature(state()) == baseline,"Inspection/map/member/settings browsing is strictly read-only"): return false
    await act("down"); await act("confirm"); await frames(4)
    var save: Dictionary = JSON.parse_string(host.call("ReadPersistenceObservationJson"))
    if not expect(save.panel in ["save-slots","error"],"Actual Quit opens shared eligible-save or explicit refusal"): return false
    await act("cancel")
    if not expect(state().inspection.page == "Options" and state().inspection.optionIndex == 3,"Quit Back restores exact caller"): return false
    await act("cancel")
    # Edge dwell, motion, keyboard retirement and fresh pointer re-entry, on a scrollable axis.
    await move_mouse(Vector2(700,2))
    var before: float = board().cameraY
    await create_timer(0.12).timeout
    if not expect(board().cameraY == before,"Edge band honors dwell"): return false
    await create_timer(0.35).timeout
    if not expect(board().cameraY < before,"Exposed upper edge pans actual camera"): return false
    samples.append({"label":"edge-pan","before":before,"after":board(),"inspection":state().inspection})
    await act("left")
    if not expect(state().inspection.edge.speed.x == 0 and state().inspection.edge.speed.y == 0,"Keyboard retires pointer pan"): return false
    root.focus_exited.emit(); await frames(3); root.focus_entered.emit(); await frames(3)
    if not expect(state().inspection.edge.speed.y == 0,"Focus return requires fresh pointer gesture"): return false
    await move_mouse(Vector2(701,3)); root.mouse_exited.emit(); await frames(2)
    if not expect(state().inspection.edge.speed.y == 0,"Native pointer exit ends edge gesture"): return false
    if not expect(resources(state()) == resources(initial),"Quit's permitted music capture preserves actual battle and both RNG seeds"): return false
    await act("cancel")
    return expect(not state().inspection.active and state().stage == "Movement","Inspection returns usable player control")

func settle_scene(previous: String) -> bool:
    var deadline := Time.get_ticks_msec()+16000
    var seen: Dictionary = {}
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if not expect(s.get("failure") == null,"Real action session has no failure"): return false
        if s.get("stage") == "Movement" and s.get("movement") == null and s.actor != previous: return true
        var sc: Dictionary = s.get("scene",{})
        if sc.get("phase") != null:
            if not expect(sc.error == null,"Actual playback has no presenter error"): return false
            if not seen.has(sc.waitToken):
                seen[sc.waitToken] = true; phases.append(sc.phase)
                samples.append({"label":"actual-scene","phase":sc.phase,"scene":sc,"actors":s.actors})
                var a: Dictionary = sc.actorCard; var t: Dictionary = sc.targetCard
                if a.visible and t.visible:
                    var ally: Dictionary = a if str(a.actor).begins_with("ally-") else t
                    var enemy: Dictionary = t if ally == a else a
                    if not expect(ally.rect.x > root.size.x/2 and ally.rect.y < 200 and enemy.rect.x < 100 and enemy.rect.y > ally.rect.y,
                            "Physical close-up uses upper-right ally and lower-left enemy"): return false
                    if sc.messagePanel.visible and not expect(enemy.rect.y+enemy.rect.height <= sc.messagePanel.panel.y,
                            "Lower enemy card clears real reader"): return false
            if sc.requiresAcknowledgement: await act("confirm")
            else: await process_frame
        elif s.get("canWaitForText",false): await act("confirm")
        else: await process_frame
    samples.append({"label":"action-timeout","state":state(),"board":board()})
    return expect(false,"Real action reaches subsequent player control")

func decision_case() -> bool:
    var initial := state()
    var range: Dictionary = board().movementRange
    if not expect(range.visible and range.destinations.size() > 0,"Actual movement grid is visible"): return false
    await act("confirm"); await frames(3)
    if not await read_geometry("main-layout-A"): return false
    for window in [Vector2i(2560,1440),Vector2i(1920,1080)]:
        root.size = window; await frames(5)
        if not await read_geometry("main-layout-A-"+str(window)): return false
    var baseline := resources(state())
    await act("attack"); await frames(3)
    var target := state()
    if not expect(target.menu.page == "Target" and board().actionRange.reach.size() > 0,"Physical offer draws actual reach"): return false
    var foe: Dictionary = target.actors.filter(func(a):return a.id == target.candidate)[0]
    var p := cell_point(foe.x,foe.y)
    await move_mouse(p)
    if not expect(state().candidate == foe.id,"Hover chooses legal live target"): return false
    var wrong := cell_point(initial.previewX,initial.previewY)
    await move_mouse(wrong); await mouse(wrong,true); await mouse(wrong,false)
    var invalid := state()
    if not expect(invalid.candidate == null and invalid.scene.phase == null,"Wrong-side click retires earlier target without committing"): return false
    await act("confirm")
    if not expect(resources(state()) == baseline and state().scene.phase == null,"Confirm after invalid click cannot act on old target"): return false
    await act("right")
    if not expect(state().candidate == foe.id,"Pad/keyboard direction restores sole eligible target"): return false
    for invalid_cell in [Vector2i(initial.previewX,initial.previewY+1),Vector2i(initial.previewX+2,initial.previewY+1),Vector2i(7,18)]:
        var bad := cell_point(invalid_cell.x,invalid_cell.y)
        await move_mouse(bad); await mouse(bad,true); await mouse(bad,false); await act("confirm")
        if not expect(state().candidate == null and state().scene.phase == null and resources(state()) == baseline,
                "Empty/out-of-range/defeated-origin ground cannot confirm an old target "+str(invalid_cell)): return false
        await act("right")
    await move_mouse(Vector2(2,500)); await mouse(Vector2(2,500),true); await mouse(Vector2(2,500),false)
    if not expect(state().failure == null and state().scene.phase == null,"Centered-board margin is a safe non-cell gesture"): return false
    await act("cancel")
    if not expect(state().menu.page == "Main" and resources(state()) == baseline,"Target Back is free and restores action"): return false
    for action in ["equipment","give","discard"]:
        await act(action); await frames(3)
        var auxiliary := state()
        if not expect(auxiliary.stage != "ActionChoice" and auxiliary.hudPanel.visible and not auxiliary.menu.visible,"Only actual auxiliary page is visible: "+action): return false
        if not expect(inside(auxiliary.hudPanel.panel),"Auxiliary reading panel fits: "+action): return false
        await act("cancel")
        if not expect(state().stage == "ActionChoice" and resources(state()) == baseline,"Auxiliary Back preserves transaction/resources: "+action): return false
    await act("cancel")
    if not await settled(): return false
    if not await inspect_case(): return false
    await act("confirm"); await act("attack")
    if not await wait_for(func(_s):return board().settled,"target camera settles after inspection"): return false
    foe = state().actors.filter(func(a):return a.id == state().candidate)[0]
    p = cell_point(foe.x,foe.y)
    await move_mouse(p); await mouse(p,true); await mouse(p,false)
    if not await settle_scene(initial.actor): return false
    if not expect("ResultMessage" in phases and "DeathMessage" in phases and "RewardMessage" in phases,"Real attack/damage/death/reward delivered"): return false
    # Reach the actual learned HEAL actor using real Stay, if another player is next.
    for turn in range(5):
        if state().actor == "ally-1": break
        var previous: String = state().actor
        await act("confirm"); await act("stay"); await act("confirm")
        if not await settle_scene(previous): return false
    if not expect(state().actor == "ally-1","Real turn schedule reaches Sarah"): return false
    await act("confirm"); await act("spell"); await frames(3)
    if not await read_geometry("learned-magic-offer"): return false
    if not expect(board().actionRange.reach.size() > 0,"Highlighted actual HEAL offer draws reach before target"): return false
    await act("confirm")
    var heal_before := resources(state())
    await act("cancel")
    if not expect(state().menu.page == "Magic" and resources(state()) == heal_before,"HEAL target cancellation is free and retains level"): return false
    await act("confirm")
    for i in range(5):
        if state().candidate == "ally-0": break
        await act("right")
    if not expect(state().candidate == "ally-0","Actual friendly HEAL target admitted"): return false
    var patient: Dictionary = state().actors.filter(func(a):return a.id == "ally-0")[0]
    var caster: Dictionary = state().actors.filter(func(a):return a.id == "ally-1")[0]
    await act("confirm")
    if not await settle_scene("ally-1"): return false
    var after := state()
    var healed: Dictionary = after.actors.filter(func(a):return a.id == "ally-0")[0]
    var paid: Dictionary = after.actors.filter(func(a):return a.id == "ally-1")[0]
    if not expect(healed.hp > patient.hp and paid.mp == caster.mp-3,"Actual HEAL recovers HP and pays once"): return false
    var switched := samples.filter(func(s):return s.label == "actual-scene" and s.scene.actorCard.actor == "ally-0" and not s.scene.targetCard.visible and s.scene.actorCard.rect.x < 100)
    return expect(not switched.is_empty(),"Recovery shows actual recipient alone in upper-left window")

func return_case() -> bool:
    if demo_case == "retreat":
        if not await inspect_case(): return false
        for turn in range(6):
            if state().actor == "ally-0": break
            var previous: String = state().actor
            await act("confirm"); await act("stay"); await act("confirm")
            if not await settle_scene(previous): return false
        if not expect(state().actor == "ally-0","Real schedule reaches learned EGRESS caster"): return false
    await act("confirm")
    if demo_case == "retreat":
        await act("spell"); await act("confirm")
        samples.append({"label":"retreat-target","state":state()})
        var before := resources(state())
        await act("cancel")
        samples.append({"label":"retreat-canceled","state":state()})
        if not expect(resources(state()) == before and state().menu.page == "Magic","Retreat cancellation does not pay"): return false
        await act("confirm"); await act("confirm")
    elif demo_case == "defeat":
        await act("stay"); await act("confirm")
    else:
        await act("attack"); await act("confirm")
    var deadline := Time.get_ticks_msec()+20000
    var shown := false
    var last := ""
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if not expect(s.get("failure") == null,"Actual result/return has no failure"): return false
        if not s.has("stage") and s.get("canWaitAtInput",false):
            samples.append({"label":"usable-field-return","state":s})
            var battle := host.find_child("BattleSessionView",true,false)
            if not expect(shown and (battle == null or not battle.is_visible_in_tree()),"Real result retires battle HUD before usable field control"): return false
            var before_entities: Array = s.entities
            await act("down"); await frames(8)
            return expect(state().entities != before_entities,"Returned field accepts real directional movement")
        var sc: Dictionary = s.get("scene",{})
        var retreat: Dictionary = s.get("retreat",{})
        var mark: String = str(sc.get("phase"))+str(retreat.get("phase"))+str(s.get("textId"))
        if mark != last:
            last = mark; samples.append({"label":"return-phase","scene":sc,"retreat":retreat,"state":s})
        if sc.get("phase") != null or retreat.get("active",false): shown = true
        if sc.get("requiresAcknowledgement",false) or retreat.get("phase") == "Text" or s.get("canWaitForText",false) or s.get("wait") == "DialogueWait" or s.get("story",{}).get("textVisible",false): await act("confirm")
        else: await process_frame
    samples.append({"label":"return-timeout","state":state()})
    return expect(false,"Actual result reaches usable field return")

func area_case() -> bool:
    var initial := state()
    var aura := demo_case == "aura"
    await act("confirm"); await act("spell")
    await act("down")
    if aura: await act("down"); await act("down")
    if not await read_geometry("actual-area-offer"): return false
    await act("confirm")
    var selected := state()
    var effect: Dictionary = board().actionRange
    if not expect(effect.visible and effect.effect.size() > 1,"Actual admitted area spell draws multi-cell effect cursor"): return false
    samples.append({"label":"area-before-cancel","range":effect,"state":selected})
    var before := resources(selected)
    await act("cancel")
    if not expect(resources(state()) == before and state().menu.page == "Magic","Area spell Cancel preserves resources/RNG and level"): return false
    await act("confirm"); await act("confirm")
    var deadline := Time.get_ticks_msec()+25000
    var targets: Array = []
    var tokens: Dictionary = {}
    while Time.get_ticks_msec() < deadline:
        var s := state()
        if not expect(s.failure == null and s.scene.error == null,"Multi-target action has no session/presenter error"): return false
        if s.get("stage") == "Movement" and s.get("movement") == null and s.actor != initial.actor:
            samples.append({"label":"area-completed","state":s,"targets":targets})
            var caster: Dictionary = s.actors.filter(func(a):return a.id == initial.actor)[0]
            return expect(targets.size() >= (5 if aura else 2) and caster.mp == (60 if aura else 14),"Actual ordered multi-target work completes with one cost and next control")
        var sc: Dictionary = s.scene
        if sc.get("phase") != null and not tokens.has(sc.waitToken):
            tokens[sc.waitToken] = true
            if sc.phase == "ResultMessage":
                var effect_state: Dictionary = sc.aura if aura else sc.attackSpell
                var target: String = effect_state.target
                targets.append(target)
                samples.append({"label":"area-target-result","scene":sc})
                if aura and not expect(sc.actorCard.actor == target and not sc.targetCard.visible,"AURA card switches to current actual recipient"): return false
        if sc.get("requiresAcknowledgement",false): await act("confirm")
        else: await process_frame
    return expect(false,"Multi-target action reaches next control")

func control_rect(control: Control) -> Dictionary:
    var r := control.get_global_rect()
    return {"x":r.position.x,"y":r.position.y,"width":r.size.x,"height":r.size.y}

func occlusion_case() -> bool:
    await act("cancel")
    if not await inspect_page("Cursor"): return false
    var initial := signature(state())
    var cursor: Dictionary = state().inspection.cursor.duplicate()
    var h: Dictionary = state().inspection.hover
    var covered := Vector2(h.rect.x+32,h.rect.y+32)
    await move_mouse(covered); await mouse(covered,true); await mouse(covered,false)
    if not expect(state().inspection.page == "Cursor" and state().inspection.cursor == cursor,"Hover-card motion/press/release never select underlying ground"): return false
    var exposed := cell_point(cursor.X,cursor.Y+1)
    await move_mouse(exposed); await mouse(exposed,true)
    await move_mouse(covered); await mouse(covered,false)
    if not expect(state().inspection.page == "Cursor","Board press and covered release cancel the gesture"): return false
    await mouse(covered,true); await move_mouse(exposed); await mouse(exposed,false)
    if not expect(state().inspection.page == "Cursor","Covered press and exposed release cannot open a ground menu"): return false
    await move_mouse(Vector2(700,2)); await create_timer(0.25).timeout
    if not expect(state().inspection.edge.speed.y < 0,"Exposed edge arms scrolling before occlusion"): return false
    h = state().inspection.hover
    covered = Vector2(h.rect.x+32,h.rect.y+32)
    await move_mouse(covered)
    var camera: float = board().cameraY
    await create_timer(0.22).timeout
    if not expect(state().inspection.edge.speed.y == 0 and board().cameraY == camera,"Hover card retires edge scrolling without moving its underlying cell"): return false
    # Root's real host entry owns its hit region, including motion that never reaches battle input.
    await move_mouse(Vector2(1800,2)); await create_timer(0.22).timeout
    var menu_entry := host.find_child("SystemMenuEntry",true,false) as Control
    await move_mouse(menu_entry.get_global_rect().get_center())
    if not expect(state().inspection.edge.speed.y == 0,"Host entry occlusion retires edge scrolling"): return false
    await click(control_rect(menu_entry)); await frames(3)
    if not expect(host.find_child("SystemMenuView",true,false) != null,"Actual host menu covers battle"): return false
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    if not expect(signature(state()) == initial and state().inspection.page == "Cursor","Host cover consumes press/release without battle mutation"): return false
    var settings_row := host.find_child("SystemSettings",true,false) as Button
    if not expect(settings_row.disabled,"Battle keeps the existing host Settings restriction"): return false
    await click(control_rect(settings_row))
    if not expect(host.find_child("SettingsView",true,false) == null and signature(state()) == initial,"Unavailable host control cannot act on the covered board"): return false
    await act("cancel")
    if not expect(state().inspection.page == "Cursor" and signature(state()) == initial,"Host Back restores unchanged inspection"): return false
    # A gesture started on the board cannot complete after a host modal takes ownership.
    cursor = state().inspection.cursor
    exposed = cell_point(cursor.X,cursor.Y)
    await move_mouse(exposed); await mouse(exposed,true); await act("menu")
    await mouse(exposed,false); await act("cancel")
    if not expect(state().inspection.page == "Cursor" and signature(state()) == initial,"Host takeover cancels the pending board gesture"): return false
    samples.append({"label":"occlusion-completed","inspection":state().inspection})
    return true

func help_case() -> bool:
    var initial := state()
    await act("menu")
    var settings_row := host.find_child("SystemSettings",true,false) as Button
    if not expect(settings_row != null and not settings_row.disabled,"Settled field permits actual F10 Settings"): return false
    for i in range(6):
        if settings_row.theme_type_variation == "EntrySelectedButton": break
        await act("down")
    await act("confirm")
    var settings := host.find_child("SettingsView",true,false)
    if not expect(settings != null,"Actual host Settings opens at its supported caller"): return false
    for i in range(10):
        if JSON.parse_string(settings.call("ReadSettingsObservationJson")).selected == 8: break
        await act("down")
    await act("confirm")
    var help := host.find_child("ControlsHelpView",true,false)
    if not expect(help != null,"Actual F10 controls help opens"): return false
    var found := false
    var locale: String = JSON.parse_string(settings.call("ReadSettingsObservationJson")).locale
    var read: Dictionary = JSON.parse_string(help.call("ReadControlsHelpObservationJson"))
    for i in range(read.count):
        read = JSON.parse_string(help.call("ReadControlsHelpObservationJson"))
        if ("只循环合法目标" in read.body if locale == "zh-CN" else "only legal targets" in read.body):
            found = true; samples.append({"label":"actual-battle-help","help":read})
        if not expect(not "disabled targets can be inspected" in read.body and not "可查看不可选目标" in read.body,"Help agrees with legal-only cycling"): return false
        await act("right")
    if not expect(found,"Selected language F10 help includes current battle target contract"): return false
    await mouse(Vector2(100,300),true); await mouse(Vector2(100,300),false)
    await act("cancel"); await act("cancel"); await act("cancel")
    var after := state()
    samples.append({"label":"help-return","before":initial,"after":after})
    return expect(after.canWaitAtInput and after.map == initial.map and after.party == initial.party and after.gold == initial.gold,
        "Help/settings/menu Back restores unchanged field caller")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); root.grab_focus()
    await frames(5)
    if InputMap.action_get_events("sf2_confirm").is_empty():
        samples.append({"label":"startup-refusal","text":host.find_children("*","Label",true,false).map(func(n):return n.text)})
        expect(false,"Selected package must pass ordinary startup/input admission"); return
    var catalog: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson")).localization
    for key in ["battle.inspect.ground-effect","battle.inspect.ground-impassable","battle.inspect.land-percent","battle.inspect.cursor-help","host.help-text-29"]:
        var translation: Dictionary = catalog.rows[key]
        if not expect(translation.EffectiveLocale == catalog.language and translation.Reason == null,"Battle catalog row uses selected language: "+key): return
    bind_events()
    for action in ["spell","item","stay","equipment","give","discard"]:
        events[action] = InputMap.action_get_events("sf2_"+action).filter(func(e):return e is InputEventKey)[0]
    if demo_case not in ["area","aura","victory","defeat"]:
        var admitted = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_COMPACT_START"))
        if not expect(admitted == "started","Actual admitted full-world controlled start: "+str(admitted)): return
    if demo_case == "help": finish(await help_case()); return
    if demo_case in ["victory","defeat"]:
        samples.append({"label":"field-before-interact","state":state()})
        await act("confirm")
        samples.append({"label":"field-after-interact","state":state()})
    if not await enter_battle():
        return
    await wait_for(func(_s):return board().settled,"initial camera settles")
    samples.append({"label":"initial","state":state(),"board":board()})
    finish(await occlusion_case() if demo_case == "occlusion" else await area_case() if demo_case in ["area","aura"] else await return_case() if demo_case in ["retreat","victory","defeat"] else await decision_case())
