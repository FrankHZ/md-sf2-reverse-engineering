extends "res://probes/engine_map3_commission_save_observation.gd"

# Real host draws after ordinary Continue or a controlled pre-session start.
func pixel_digest(tex: Texture2D) -> String:
    var im := tex.get_image()
    im.convert(Image.FORMAT_RGBA8)
    var digest := HashingContext.new()
    digest.start(HashingContext.HASH_SHA256)
    digest.update(im.get_data())
    return digest.finish().hex_encode()

func _initialize() -> void:
    create_timer(240).timeout.connect(func(): check(false, "terrain observation deadline"); finish())
    call_deferred("run")

func battle_progress() -> Array:
    var battle := find("BattleSessionView") as Control
    if battle == null or not battle.is_visible_in_tree(): return []
    var data: Dictionary = battle.call("ReadCaptureState",false)
    return [data.revision,data.actor,data.stage,data.gold,data.storyFlags,data.mainSeed,data.thinkingSeed,data.get("inventories")]

func mode(value: String) -> void:
    await key(KEY_F10)
    await choose_system_action("Settings")
    if find("SettingsView") == null: check(false, "settings did not open"); return
    await select_setting("graphics")
    if settings().draft.GraphicsMode != value: await key(KEY_RIGHT)
    await select_setting("apply")
    var paused: Variant = persist().progress
    var battle_before := battle_progress()
    var start := Time.get_ticks_usec()
    await key(KEY_ENTER)
    check(find("SettingsView") == null and paused == persist().progress, "Apply preserves paused progress")
    check(battle_before == battle_progress(),"Apply preserves paused tactical state")
    samples.append({"apply":value, "ms":(Time.get_ticks_usec()-start)/1000.0})
    if find("SystemMenuView") != null: await key(KEY_ESCAPE)
    await frames(5)

func terrain_sample(label: String, region: Rect2i, width: int) -> Array:
    var view := find("ExplorationSessionView")
    var p := region.position
    var n := region.size
    var request: int = view.call("ObserveLayoutRegion", p.x,p.y,n.x,n.y)
    var data: Dictionary
    for tick in 60:
        await process_frame
        data = view.call("ReadLayoutRegion", p.x,p.y,n.x,n.y)
        if data.draw != null and data.draw.request == request: break
    view.call("StopLayoutObservation")
    var handles := {}
    var chosen := {}
    var passes := {}
    var first := {}
    for row in data.draw.uses if data.draw != null else []:
        var tex := instance_from_id(int(row.resourceIdentity)) as Texture2D
        check(tex != null and tex.get_width() == width and row.selector.pixels == width, "live field density")
        check(is_equal_approx(row.width,row.textureWidth * 4.0), "field footprint")
        handles[row.resourceIdentity] = true
        passes[row.layer] = true
        if row.selector.contextual: chosen[Vector2i(row.sourceX,row.sourceY)] = true
        if first.is_empty() or not first.selector.contextual and row.selector.contextual: first = row
    if not first.is_empty():
        first["rgba"] = pixel_digest(instance_from_id(int(first.resourceIdentity)) as Texture2D)
    check(not handles.is_empty() and (width == 24 or not chosen.is_empty()), label + " observed terrain")
    var s := state()
    check(s.failure == null and s.presentation.error == null, label + " field errors")
    samples.append({"label":label,"map":s.map,"player":player(s).get("id"),"textures":handles.size(),
        "contexts":chosen.size(),"passes":passes.keys(),"first":first,"f217":217 in s.flags,"f624":624 in s.flags})
    return handles.keys()

func load_slot(index: int) -> bool:
    await key(KEY_ENTER); await key(KEY_E); await key(KEY_DOWN); await key(KEY_ENTER)
    if not await wait_slots("continue-slots"): return false
    var rows: Array = slots().choices
    var target := rows.find_custom(func(label): return str(label).begins_with("Slot %d " % index))
    if target < 0: check(false,"missing named slot " + str(index)); return false
    for i in rows.size():
        if slots().selected == target: break
        await key(KEY_DOWN)
    await key(KEY_ENTER)
    for i in 300:
        if find("ExplorationSessionView") != null: break
        await process_frame
    if find("ExplorationSessionView") == null: check(false,"Continue failed"); return false
    await settle("Continue")
    return true

func battle_sample(width: int) -> Array:
    var b := find("BattleSessionView")
    var terrain := observation(b.get_node("MapViewport"),"ReadTerrainJson")
    var board: Dictionary = b.call("ReadCaptureState", false)
    check(board.failure == null and board.scene.error == null and terrain.clip, "battle scene and adapter")
    var geometry := []
    var used := []
    var contexts := []
    for layer in terrain.layers:
        var draw: Variant = layer.first
        if draw == null: continue
        var tex := instance_from_id(int(draw.texture)) as Texture2D
        check(tex != null and tex.get_width() == width and draw.source.width == width / 3,
            "real tactical texture and source tile density")
        check(draw.rect.width == terrain.cellSize / 3 and layer.highPriority == ((int(draw.word) & 0x8000) != 0),
            "tactical priority and logical destination")
        geometry.append([layer.name,layer.draws,layer.offsetX,layer.offsetY,draw.sourceX,draw.sourceY,
            draw.tile,draw.word,draw.rect,layer.z])
        used.append(draw)
        if layer.firstContext != null:
            var sample: Dictionary = layer.firstContext.duplicate()
            var variant := instance_from_id(int(sample.texture)) as Texture2D
            sample["rgba"] = pixel_digest(variant)
            sample["overlay"] = layer.overlay
            sample["offset"] = [layer.offsetX,layer.offsetY]
            contexts.append(sample)
    check(width == 24 or not contexts.is_empty(),"tactical live neighborhood")
    check(not geometry.is_empty(), "actual battle terrain passes")
    samples.append({"battle":terrain.region,"width":width,"textures":terrain.textures,"draws":used,"contexts":contexts,"geometry":geometry})
    return geometry

func quake(cold: bool) -> void:
    var slot := 5 if cold else 4
    var path := OS.get_environment("SF2_SAVE_ROOT").path_join("issue2175/sf2-opening/slot%d.json" % slot)
    var original := FileAccess.get_file_as_bytes(path)
    if not await load_slot(slot): return
    if cold:
        check(state().map == "map-4" and 624 in state().flags,"cold F624 Continue")
        if applied().GraphicsMode != "hd": await mode("hd")
        await terrain_sample("cold",Rect2i(24,0,11,10),96)
        await mode("original")
        await terrain_sample("cold-original",Rect2i(24,0,11,10),24)
    else:
        check(state().map == "map-19" and 624 not in state().flags, "post-tower castle slot")
        if applied().GraphicsMode != "hd": await mode("hd")
        if not failures.is_empty(): return
        var old := await terrain_sample("castle",Rect2i(0,0,10,10),96)
        await edge(KEY_SHIFT,true)
        await walk(Vector2i(16,5),"castle-warning")
        await walk(Vector2i(25,13),"castle-safe-group")
        await walk(Vector2i(-1,31),"quake-entry","map-4")
        if issue != "": check(false,issue); return
        check(624 in state().flags and 217 not in state().flags, "first quake completed")
        check(old.all(func(id): return not is_instance_id_valid(int(id))), "castle terrain released")
        var initial := await terrain_sample("first quake",Rect2i(24,0,11,10),96)
        await key(KEY_DOWN); await settle("advance")
        await key(KEY_DOWN); await settle("blocked")
        check(Vector2i(int(player(state()).x)/384,int(player(state()).y)/384)==Vector2i(29,5),"copied terrain blocks the second step")
        await walk(Vector2i(50,24),"docks","map-44")
        if issue != "": check(false,issue); return
        check(initial.all(func(id): return not is_instance_id_valid(int(id))), "first quake terrain released")
        await walk(Vector2i(0,25),"revisit","map-4")
        if issue != "": check(false,issue); return
        await walk(Vector2i(29,4),"revisit quake region")
        if issue != "": check(false,issue); return
        await edge(KEY_SHIFT,false)
        await terrain_sample("quake revisit",Rect2i(24,0,11,10),96)
        check(624 in state().flags and 217 not in state().flags,"revisit flags")
    check(original == FileAccess.get_file_as_bytes(path),"slot preserved")

func shrine() -> void:
    await settle("shrine")
    if applied().GraphicsMode != "hd": await mode("hd")
    var view := find("ExplorationSessionView")
    var closed: int = view.call("ReadLayoutRegion",19,24,1,1).words[0]
    await terrain_sample("closed chest",Rect2i(18,23,3,3),96)
    await key(KEY_ENTER)
    var opened: int = view.call("ReadLayoutRegion",19,24,1,1).words[0]
    samples.append({"chestWords":[closed,opened],"search":state().get("search"),"wait":state().get("wait"),
        "position":player(state()),"claimed":133 in state().flags})
    check(133 in state().flags and opened != closed,"real Search replaces the chest")
    await terrain_sample("open chest",Rect2i(18,23,3,3),96)
    await key(KEY_ENTER); await settle("chest result")
    await walk(Vector2i(19,16),"shrine stair","map-42")
    await terrain_sample("shrine hall",Rect2i(0,4,11,10),96)
    await walk(Vector2i(7,15),"shrine exit","map-41")
    if issue != "": check(false,issue); return
    var at := Vector2i(int(player(state()).x)/384,int(player(state()).y)/384)
    await terrain_sample("shrine exterior",Rect2i(at-Vector2i(3,3),Vector2i(7,7)),96)

func door_roof() -> void:
    await settle("Hawel house")
    if applied().GraphicsMode != "hd": await mode("hd")
    var field := find("ExplorationSessionView")
    var roof: Array = field.call("ReadLayoutRegion",2,16,12,8).words
    var closed: int = field.call("ReadLayoutRegion",11,9,1,1).words[0]
    await terrain_sample("roof present",Rect2i(2,16,12,8),96)
    await key(KEY_UP); await settle("step into doorway")
    if field.call("ReadLayoutRegion",2,16,12,8).roof == null:
        await key(KEY_UP); await settle("enter house")
    var open: Dictionary = field.call("ReadLayoutRegion",2,16,12,8)
    var door: int = field.call("ReadLayoutRegion",11,9,1,1).words[0]
    check(door != closed and open.roof != null and open.words.all(func(word):return int(word) == 0),
        "door copy and active cleared roof")
    await terrain_sample("open doorway",Rect2i(8,7,7,5),96)
    samples.append({"doorWords":[closed,door],"roofCells":roof.size(),"roofCleared":open.roof != null})
    await key(KEY_DOWN); await settle("leave house")
    if field.call("ReadLayoutRegion",2,16,12,8).roof != null:
        await key(KEY_DOWN); await settle("leave doorway")
    var restored_roof: Dictionary = field.call("ReadLayoutRegion",2,16,12,8)
    check(restored_roof.roof == null and restored_roof.words == roof,"cached roof restored")
    await terrain_sample("restored roof",Rect2i(2,16,12,8),96)

func signed_planes() -> void:
    await settle("docks deck")
    if applied().GraphicsMode != "hd": await mode("hd")
    # The deck has a -32 relative overlay. Its A and B source cells project
    # into the same logical cell at (24,4), with two different painted blocks.
    var field := find("ExplorationSessionView")
    field.call("ReadObservationJson")
    await frames(4)
    var camera: Dictionary = field.call("ReadCaptureState",false,true).cameraProjection
    check(is_equal_approx(camera.foreground.y-camera.background.y,-32*24.0),"signed plane displacement")
    await terrain_sample("nonempty A",Rect2i(24,4,1,1),96)
    var a: Dictionary = samples.back().first
    await terrain_sample("packed B",Rect2i(24,36,1,1),96)
    var b: Dictionary = samples.back().first
    if a.is_empty() or b.is_empty(): return
    check(a.overlay and not b.overlay and a.block == 29 and b.block == 207 and
        a.selector.contextual and b.selector.contextual and a.rgba != b.rgba,
        "signed sources resolve distinct real contextual textures")

func voyage_layers() -> void:
    if not await load_slot(5): return
    if applied().GraphicsMode != "hd": await mode("hd")
    await edge(KEY_SHIFT,true)
    await walk(Vector2i(50,24),"docks-entry","map-44")
    await walk(Vector2i(11,14),"dock-man")
    if issue != "": check(false,issue); return
    await key(KEY_LEFT); await settle("face-captain")
    await edge(KEY_SHIFT,false)
    await key(KEY_ENTER); await wait_choice(); await key(KEY_LEFT)
    if not failures.is_empty(): return
    await edge(KEY_SHIFT,true); await key(KEY_ENTER)
    var field := find("ExplorationSessionView")
    field.call("ReadObservationJson")
    var seen := {}
    var previous := ""
    for tick in 5000:
        await frames(3)
        var s := state()
        if s.get("failure") != null: check(false,"voyage failure"); return
        var map: String = s.get("map","")
        if map != previous:
            previous = map
            samples.append({"voyageMap":map,"cursor":s.get("cursor"),"wait":s.get("wait")})
            print("voyage ", map)
        if map == "map-44" and s.get("canWaitAtInput",false):
            check(false,"captain returned to docks control"); return
        if map == "map-14" and s.get("canWaitAtInput",false):
            await terrain_sample("arrival control",Rect2i(35,3,14,10),96)
            check(700 in s.flags and 640 not in s.flags,"arrival flags")
            break
        if map == "map-66" and not seen.has("map-66-background") or map == "map-73" and not (
                seen.has("map-73-background-shifted") and seen.has("map-73-foreground")):
            var camera: Variant = field.call("ReadCaptureState",false,true).cameraProjection
            if camera != null and camera.map == map:
                for plane in ["background","foreground"]:
                    var pass_: Variant = camera.get(plane)
                    if pass_ == null: continue
                    var tag: String = map + "-" + plane
                    if pass_.first == null:
                        var high: Variant = camera.get(plane + "High")
                        if high != null and high.first != null: pass_ = high
                        elif map == "map-73" and plane == "foreground" and not seen.has("map-73-blank-A") and camera.legalBounds.width == 11*24:
                            var a: Dictionary = field.call("ReadLayoutRegion",0,0,11,10)
                            if a.words.all(func(word): return (int(word) & 1023) == 0):
                                seen["map-73-blank-A"] = true
                                samples.append({"label":"map-73-blank-A","cells":a.words.size(),
                                    "AX":pass_.x,"AY":pass_.y,"BX":camera.background.x,"BY":camera.background.y,
                                    "lowDraws":pass_.draws,"highDraws":high.draws})
                    if pass_.first == null: continue
                    var first: Dictionary = pass_.first
                    if map == "map-73" and plane == "background" and first.sourceY >= 36:
                        tag += "-shifted"
                    if seen.has(tag): continue
                    var tex := instance_from_id(int(first.texture)) as Texture2D
                    check(tex != null and tex.get_width() == 96 and first.selector.pixels == 96,tag + " drawn texture")
                    seen[tag] = true
                    samples.append({"label":tag,"planeX":pass_.x,"planeY":pass_.y,"first":first,"draws":pass_.draws,
                        "bounds":camera.legalBounds,"rgba":pixel_digest(tex)})
        if s.get("canWaitForText",false) or s.get("wait") in ["DialogueWait","FieldTextWait","W1TextWait"]:
            await key(KEY_ENTER)
    await edge(KEY_SHIFT,false)
    check(state().map == "map-14" and seen.has("map-66-background") and
        seen.has("map-73-background-shifted") and (seen.has("map-73-foreground") or seen.has("map-73-blank-A")),
        "detached scene layers through landing " + str(seen))

func run() -> void:
    if OS.get_environment("SF2_SAVE_NAMESPACE") != "issue2175":
        push_error("select disposable issue2175 namespace"); quit(2); return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(8)
    if case_name == "voyage": await voyage_layers()
    elif case_name in ["quake","cold"]:
        await quake(case_name == "cold")
    else:
        var mounted: String = host.call("StartPreVoyageLocalizationObservation",OS.get_environment("SF2_HOST_UI_START"))
        check(mounted == "started","controlled mount " + mounted)
        if mounted != "started": finish(); return
        await frames(12)
        if case_name.begins_with("battle"):
            if applied().GraphicsMode != "original": await mode("original")
            var original := battle_sample(24)
            await mode("hd")
            check(original == battle_sample(96),"tactical HD geometry")
            await mode("original")
            check(original == battle_sample(24),"tactical original restored")
        elif case_name == "shrine": await shrine()
        elif case_name == "signed": await signed_planes()
        elif case_name == "door": await door_roof()
        elif case_name == "hold":
            await settle("interior")
            if applied().GraphicsMode != "hd": await mode("hd")
            await terrain_sample("hold",Rect2i(47,0,15,12),96)
            await terrain_sample("hold foreground",Rect2i(54,17,9,9),96)
        else: check(false,"unknown terrain observation case")
    finish()
