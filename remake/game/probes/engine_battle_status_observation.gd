extends "res://probes/engine_world_viewport_observation.gd"

# Bounded ordinary-host state/input/draw observation. No setters, screenshots or helper tests.
var changes: Array = []
var phases: Array = []
var reaction_targets: Array = []

func sample(label: String) -> Dictionary:
    var s := state()
    var row := {"label":label}
    for key in ["revision","stage","actor","target","failure","actors","menu","scene","movement","viewport","mapViewport","zoom","previewRect","observations","stopReason"]:
        if s.has(key): row[key] = s[key]
    samples.append(row)
    return s

func rect(d: Dictionary) -> Rect2:
    return Rect2(d.x,d.y,d.width,d.height)

func card(c: Dictionary, s: Dictionary, drawn := false) -> bool:
    if not c.visible: return true
    var rows: Array = s.actors.filter(func(a): return a.id == c.actor)
    if not check(rows.size() == 1, "Card retains stable diagnostic identity"): return false
    var a: Dictionary = rows[0]
    if not check(not c.name.is_empty() and c.name != c.actor and c.nameLines <= 2 and c.nameFontSize >= 22, "Readable name in reserved lines"): return false
    if drawn and not check(rect(s.viewport).encloses(rect(c.rect)) and rect(c.rect).encloses(rect(c.nameRect)), "Card and name fit actual viewport/frame"): return false
    for pair in [["hp","maxHp"],["mp","maxMp"]]:
        var g: Dictionary = c[pair[0]]
        if not check(g.current == a[pair[0]] and g.maximum == a[pair[1]], "Card follows current authoritative " + pair[0]): return false
        var fraction: float = 0.0 if g.maximum == 0 else float(g.current)/g.maximum
        if not check(is_equal_approx(g.image.fraction,fraction) and g.text == ("—" if g.maximum == 0 else str(int(g.current))+"/"+str(int(g.maximum))), "Current/max, dash and fill agree"): return false
        if not check(g.image.mounted and g.image.filter == "Nearest" and g.image.frame.ends_with("frame.png") and g.image.track.ends_with("track.png"), "Real image frame/track/fill mounted"): return false
        if drawn and not check(g.image.drawCount > 0 and is_equal_approx(g.image.drawnFraction,fraction) and is_equal_approx(g.image.drawnFill.width,(g.image.rect.width-12)*fraction), "Actual image draw uses ratio and empty/full width"): return false
    if a.hp == 0 and not check("Defeated" in c.role, "Zero HP has a non-color defeated cue"): return false
    if a.hp > 0 and a.hp * 4 <= a.maxHp and not check("Low HP" in c.role, "Low HP has a non-color warning"): return false
    return true

func menus(label: String) -> Dictionary:
    await frames(4)
    var s := sample(label)
    if not card(s.menu.layout.actorCard,s,true): return {}
    if not card(s.menu.layout.targetCard,s,true): return {}
    var layout: Dictionary = s.menu.layout
    if not check(s.mapViewport == s.viewport and s.zoom == 1, "UI keeps accepted full world and scale"): return {}
    var board := view.get_node("MapViewport")
    var terrain: Dictionary = JSON.parse_string(board.call("ReadTerrainJson"))
    samples[-1].range = terrain.movementRange
    if not check(terrain.cellSize == 96 and terrain.movementRange.visible == (s.stage == "Movement"), "Existing 96px cells and movement-range stage visibility"): return {}
    var panels: Array = [layout.actor,layout.context,layout.description]
    if layout.targetCard.visible: panels.append(layout.targetCard.rect)
    if layout.diamondVisible:
        panels.append(layout.diamond)
        if not check(layout.diamond.width == 288 and layout.diamond.height == 224, "Accepted diamond geometry"): return {}
    if layout.listVisible: panels.append(layout.list)
    for i in range(panels.size()):
        if not check(rect(s.viewport).encloses(rect(panels[i])), "Visible menu panel fits viewport"): return {}
        for j in range(i):
            if not check(not rect(panels[i]).intersects(rect(panels[j])), "Visible menu panels remain disjoint"): return {}
    var subject = s.get("target") if layout.targetCard.visible else s.actor
    var subjects: Array = s.actors.filter(func(a): return a.id == subject)
    if not subjects.is_empty():
        # During target selection the map camera uses the candidate, otherwise preview.
        var focus_rect = s.previewRect if not layout.targetCard.visible else subjects[0].globalRect
        if focus_rect != null:
            for p in panels:
                if not check(not rect(p).intersects(rect(focus_rect)), "Panels avoid retained subject"): return {}
    return s

func result_events(json: String) -> void:
    var r: Dictionary = JSON.parse_string(json)
    for e in r.observations:
        if e.Kind in ["mp","hp"]: changes.append(e)

func scene_until_control(healing: bool) -> bool:
    var last_phase := ""
    var seen := false
    var before: Dictionary = samples[-1]
    var initial: Array = before.actors
    var deadline := Time.get_ticks_msec()+25000
    while Time.get_ticks_msec() < deadline:
        if not root.has_focus(): sample("scene-focus-lost")
        if not await focus(): return false
        var s := state()
        if not check(s.failure == null and s.scene.error == null, "Scene/session remain available"): return false
        if s.scene.visible:
            if not seen:
                await frames(3) # Native containers and draw get a real delivery opportunity.
                s = sample("scene-first-draw")
                if not card(s.scene.actorCard,s,true) or not card(s.scene.targetCard,s,true): return false
            seen = true
            if not card(s.scene.actorCard,s) or not card(s.scene.targetCard,s): return false
            if not check(not s.menu.visible, "Menu cards detach during action scene"): return false
            if case_name == "laser" and s.scene.phase in ["TargetEnter","Reaction","ResultMessage","DeathMessage"]:
                if not check(s.scene.targetCard.actor == s.scene.laser.target, "Laser card follows actual ReactionTarget at every reached target phase"): return false
            if s.scene.phase != last_phase:
                last_phase = s.scene.phase
                phases.append(last_phase)
                if last_phase == "ResultMessage":
                    await frames(2)
                    s = state()
                    if not card(s.scene.actorCard,s,true) or not card(s.scene.targetCard,s,true): return false
                sample("scene-"+last_phase)
                if case_name in ["laser","aura"] and last_phase == "Reaction": reaction_targets.append(s.scene.targetCard.actor)
                if last_phase in ["Initialize","ActionMessage","ActionAnimation"]:
                    for a in s.actors:
                        var b: Dictionary = initial.filter(func(row): return row.id == a.id)[0]
                        if not check(a.hp == b.hp, "No preemptive damage/healing before reaction"): return false
                        if last_phase in ["Initialize","ActionMessage"] and not check(a.mp == b.mp, "No preemptive MP cost"): return false
            if s.scene.phase in ["ActionMessage","ResultMessage","DeathMessage","RewardMessage","GrowthMessage","GoldMessage"]:
                var h = s.scene.healing
                if h == null or h.AtTimedInput or h.LogicalComplete:
                    await tap(KEY_ENTER)
                    continue
        elif seen and s.get("stage") == "Movement" and s.stopReason == "PlayerInput":
            if not check("Reaction" in phases, "Reached real damage/recovery reaction"): return false
            if healing and not check("SpellCost" in phases and changes.any(func(e): return e.Kind == "mp"), "Observed authoritative MP cost phase"): return false
            if case_name == "private":
                var sarah: Dictionary = s.actors.filter(func(a): return a.id == "ally-1")[0]
                if not check(sarah.hp == 11 and sarah.mp == 7 and not changes.any(func(e): return e.Kind == "hp"), "Full-HP private HEAL spends MP without fabricating an HP change"): return false
            elif not check(changes.any(func(e): return e.Kind == "hp"), "Observed authoritative HP commit"): return false
            await menus("next-actor")
            return not ended
        await process_frame
    sample("scene-deadline")
    return fail("Scene did not release ordinary control")

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    root.grab_focus()
    await frames(8)
    if case_name in ["private","names","overflow"]:
        var deadline := Time.get_ticks_msec()+15000
        var skipped := false
        while Time.get_ticks_msec() < deadline and state().get("stage") != "Movement":
            if not await focus(): return
            var s := state()
            if not skipped and not s.get("skipRequiresRelease",true) and "Skip battle intro" in str(s.get("help","")):
                skipped = true
                await tap(KEY_ESCAPE)
            elif s.get("entrySkip") != null: await process_frame
            elif s.get("canWaitForText",false) or s.get("wait") == "ChoiceWait" or "Reveal / Continue" in str(s.get("help","")): await tap(KEY_ENTER)
            else: await process_frame
    if not await wait_for(func(s): return s.get("stage") == "Movement", "battle control"): return
    view.connect("SessionResultObserved",result_events)
    if case_name in ["names","overflow"]:
        await name_boundary()
        return
    var initial := await menus("initial-movement")
    if initial.is_empty(): return
    if case_name == "private":
        await private_menu()
        return
    if case_name == "laser":
        await laser_cards()
        return
    if case_name == "heal":
        if not check(initial.menu.layout.actorCard.name.contains("Northern Watch") and initial.menu.layout.actorCard.nameLines == 2, "Long public content name wraps readably"): return
    await tap(KEY_ENTER)
    if (await menus("main-actions")).is_empty(): return
    if case_name in ["heal","long","aura"]:
        await tap(KEY_H)
        if case_name == "aura":
            for level in range(3): await tap(KEY_DOWN)
        if (await menus("magic-list")).is_empty(): return
        await tap(KEY_ENTER)
        if (await menus("self-target")).is_empty(): return
        await tap(KEY_TAB)
        if (await menus("other-target")).is_empty(): return
        await tap(KEY_ESCAPE)
        var cancelled := await menus("target-cancel")
        if cancelled.is_empty() or not check(not cancelled.menu.layout.targetCard.visible and cancelled.menu.page == "Magic", "Cancel clears target card and restores list"): return
        await tap(KEY_ESCAPE)
        await tap(KEY_ESCAPE)
        if not await wait_for(func(s): return s.get("stage") == "Movement", "cancel to movement"): return
        var restored := await menus("movement-restored")
        if restored.is_empty() or not check(restored.actors[0].hp == initial.actors[0].hp and restored.actors[0].mp == initial.actors[0].mp, "Selection and Cancel never spend resources"): return
        await tap(KEY_ENTER)
        if not await wait_for(func(s): return s.get("stage") == "ActionChoice" and s.menu.page == "Main", "fresh actions after Cancel"): return
        sample("fresh-actions")
        await tap(KEY_H)
        if not await wait_for(func(s): return s.menu.page == "Magic", "fresh magic list"): return
        sample("fresh-magic")
        if case_name == "aura":
            for level in range(3): await tap(KEY_DOWN)
        await tap(KEY_ENTER)
        if not await wait_for(func(s): return s.get("stage") == "CommitReady" and s.menu.page == "Target", "fresh self target"): return
    else:
        await tap(KEY_F)
        if (await menus("enemy-target")).is_empty(): return
    sample("before-commit")
    await tap(KEY_ENTER)
    if not await scene_until_control(case_name in ["heal","long","aura"]): return
    if case_name == "aura" and not check(reaction_targets == ["a-edge","b-center","n-neutral","m-full","z-priest","f-far"], "Shared card follows every AURA recipient including full HP"): return
    samples.append({"label":"resource-events","events":changes,"phases":phases})
    finish(true)

func name_boundary() -> void:
    await frames(4)
    var s := sample("name-boundary")
    var c: Dictionary = s.menu.layout.actorCard
    if not check(c.visible and c.nameVisibleLines <= 2 and c.rect.width <= 800 and rect(s.viewport).encloses(rect(c.rect)), "Name metadata reaches a bounded native frame"): return
    if case_name == "names":
        if not check(c.name == "Northern Watch Keeper Three" and c.nameLines <= 2, "Display-only whitespace normalization preserves the complete readable name"): return
        if not card(c,s,true): return
    else:
        if not check(c.name.length() == 1202 and c.nameFontSize == 22 and c.rect.width == 800 and c.nameVisibleLines == 2, "Exceptional metadata retains its text with finite two-line ellipsis fallback"): return
    finish(true)

func laser_cards() -> void:
    # Existing prism-laser authored observation route: two off-ray Stays, then
    # one ordinary Up + Stay per ally. The automatic shot hits three real targets.
    for turn in range(2):
        await tap(KEY_ENTER)
        await tap(KEY_SPACE)
        await tap(KEY_ENTER)
        if not await wait_for(func(s): return s.get("stage") == "Movement", "off-ray turn"): return
    for turn in range(2):
        await tap(KEY_UP)
        if not await wait_for(func(s): return s.get("movement") == null, "laser positioning"): return
        await tap(KEY_ENTER)
        await tap(KEY_SPACE)
        sample("laser-before-commit")
        await tap(KEY_ENTER)
        if turn == 0 and not await wait_for(func(s): return s.get("stage") == "Movement", "second ally"): return
    if not await scene_until_control(false): return
    if not check(reaction_targets == ["companion","leader","sentry"], "Laser card switches through every ordered ally/enemy reaction"): return
    samples.append({"label":"laser-targets","targets":reaction_targets,"events":changes})
    finish(true)

func private_menu() -> void:
    var s := await menus("private-initial")
    if s.is_empty(): return
    if not check(s.menu.layout.actorCard.name.to_upper() in ["BOWIE","SARAH","CHESTER"], "Admitted party name replaces ally ID"): return
    # Stay through ordinary turn dispatch until the admitted HEAL user receives control.
    for turn in range(4):
        if state().actor == "ally-1": break
        await tap(KEY_ENTER)
        await tap(KEY_SPACE)
        await tap(KEY_ENTER)
        if not await wait_for(func(r): return r.get("stage") == "Movement" and r.get("movement") == null, "private next actor", 12.0): return
    if not check(state().actor == "ally-1", "Ordinary Sarah control"): return
    await tap(KEY_ENTER)
    await tap(KEY_H)
    if (await menus("private-spell-list")).is_empty(): return
    await tap(KEY_ENTER)
    if (await menus("private-self-target")).is_empty(): return
    await tap(KEY_TAB)
    var targeted := await menus("private-other-target")
    if targeted.is_empty(): return
    if not check(targeted.menu.layout.targetCard.visible and targeted.menu.layout.targetCard.actor == "ally-2" and targeted.menu.layout.targetCard.name == s.menu.layout.actorCard.name, "Same admitted Chester name as acting and unavailable target"): return
    await tap(KEY_ESCAPE)
    var cancelled := await menus("private-cancel")
    if cancelled.is_empty() or not check(not cancelled.menu.layout.targetCard.visible, "Private Cancel clears target card"): return
    await tap(KEY_ENTER)
    sample("private-before-commit")
    await tap(KEY_ENTER)
    if not await scene_until_control(true): return
    samples.append({"label":"private-resource-events","events":changes,"phases":phases})
    finish(true)
