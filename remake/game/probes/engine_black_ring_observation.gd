extends "res://probes/engine_battle_save_observation.gd"

var mode := OS.get_environment("SF2_RING_CASE")
var chinese := OS.get_environment("SF2_RING_LANGUAGE") == "zh-CN"
var ring_name := "黑色戒指" if chinese else "Black Ring"

func wizard(s: Dictionary) -> Dictionary:
    return s.actors.filter(func(row): return row.id == "ally-4")[0]

func pack(s: Dictionary) -> Array:
    return wizard(s).items.map(func(word): return int(word))

func field_wizard(s: Dictionary) -> Dictionary:
    return s.party.filter(func(row): return row.Actor.Value == "ally-4")[0]

func sample(label: String) -> void:
    var s := state() if view != null else {}
    var scene: Dictionary = s.get("scene", {})
    samples.append({"label":label, "stage":s.get("stage"),"actor":s.get("actor"),"map":s.get("map"),
        "failure":s.get("failure"),"feedback":s.get("feedback"),"gold":s.get("gold"), "church":s.get("church"), "shop":s.get("shop"),
        "wizard":wizard(s) if s.has("stage") and s.has("actors") else null,
        "equipmentSlot":s.get("equipmentSlot"),"equipmentText":s.get("equipmentText"),
        "menu":s.get("menu"),"scene":scene,"panel":panel(),"saveAvailability":persistence().get("availability"),
        "wait":s.get("wait"),
        "fieldWizard":field_wizard(s) if s.get("party") != null else null})

func choose_equipment(slot: int) -> bool:
    for cycle in range(7):
        if state().get("equipmentSlot") == slot:
            return true
        await key(KEY_DOWN)
    check(false, "equipment slot " + str(slot))
    return false

func battle_use() -> bool:
    var before := state()
    var holder := wizard(before)
    check(before.actor == "ally-4" and holder.mp == 0, "controlled WIZ with zero MP")
    await key(KEY_ENTER)
    if mode == "battle-damage":
        await key(KEY_E)
        if not await choose_equipment(1): return false
        sample("bare-ring-preview")
        check(ring_name in state().equipmentText, "localized ring preview")
        await key(KEY_ENTER)
        var worn := wizard(state())
        check(worn.items[1] == 152 and worn.attack == holder.attack + 10 and (int(worn.status) & 4) != 0,
            "confirmed slot contributes attack and curse")
        await key(KEY_E)
        if not await choose_equipment(5): return false
        await key(KEY_ENTER)
        check(state().failure == "equipped-curse" and wizard(state()).items == worn.items,
            "worn removal refused without mutation")
        check(("无法取下" if chinese else "cannot be removed") in state().feedback, "localized removal refusal")
        sample("locked-removal")
        await key(KEY_ESCAPE)

    await key(KEY_I)
    sample("item-selector")
    # Slot 1 holds the ring in the controlled fork.
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if state().get("stage") == "TargetChoice": await key(KEY_TAB)
    if state().get("stage") != "CommitReady":
        sample("target-missing")
        check(false, "ordinary BLAZE 2 target confirmed")
        return false
    await key(KEY_ENTER)
    var projected := {}
    for frame in range(3600):
        var result: Dictionary = JSON.parse_string(view.call("ReadSceneObservationJson"))
        var scene: Dictionary = result.scene
        if result.failure != null or scene.error != null:
            check(false, "battle consumption failure: " + str([result.failure,scene.error]))
            return false
        if scene.get("requiresAcknowledgement",false):
            if not projected.has(scene.phase):
                projected[scene.phase] = true
                if scene.phase in ["ActionMessage","EquipmentWearMessage"]:
                    check(ring_name in scene.message, "localized item name in " + scene.phase)
                sample("delivery-" + scene.phase)
            await key(KEY_ENTER)
        elif result.hasBattleControl and not projected.is_empty():
            break
        await process_frame
    var after := state()
    sample("after-use")
    check(after.get("stage") == "Movement" and projected.has("EquipmentWearMessage"), "use and wear return to Movement")
    check(not projected.has("SpellCost") and wizard(after).mp == 0, "item use spent no MP")
    if mode == "battle-damage":
        check(pack(after) == [213,408,17,0] and (int(wizard(after).status) & 4) != 0,
            "wear changes only worn ring and retains curse")
    else:
        check(pack(after) == [213,17,0,127] and (int(wizard(after).status) & 4) == 0,
            "broken reuse removes only the selected slot and clears curse")
    return failures.is_empty()

func battle_description() -> void:
    var before := state()
    var word := int(wizard(before).items[1])
    check(before.actor == "ally-4" and (word & 127) == 24, "controlled WIZ with Black Ring in slot 2")
    await key(KEY_ENTER)
    await key(KEY_I)
    await key(KEY_DOWN)
    sample("ring-description")
    var chosen := state()
    var layout: Dictionary = chosen.menu.layout
    check(chosen.failure == null and chosen.menu.page == "Items" and layout.descriptionVisible
        and ring_name in layout.rows[int(layout.selectedRow)], "Black Ring detail visible")
    var lines := String(layout.detail).split("\n")
    var warning := "损坏时在战斗中使用，道具会消失。" if chinese else "Damaged: battle use destroys it."
    check(lines.count(warning) == (1 if (word & 256) != 0 else 0), "destruction warning matches the damaged bit")
    check(("装备后：攻击 +10。" if chinese else "When equipped: Attack +10.") in lines,
        "description retains equipment contribution")
    check(("火属性伤害" if chinese else "fire damage") in layout.detail and
        ("半径 1 格" if chinese else "within 1 tile") in layout.detail, "description reflects BLAZE 2 damage and area")
    check(pack(chosen) == pack(before), "reading the description preserves the pack")

func service_page(kind: String, name: String) -> bool:
    for cycle in range(500):
        var s := state()
        if s.get("failure") != null: break
        var service: Dictionary = s[kind]
        if service.get("visible", false) and service.get("page") == name:
            return true
        if s.get("canWaitForText", false): await key(KEY_ENTER)
        await process_frame
    sample("service-missing-" + name)
    check(false, "service page " + name)
    return false

func confirm_save() -> bool:
    if not await wait_panel("save-slots"): return false
    await key(KEY_ENTER)
    if not await wait_panel("confirm-save"): return false
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_panel("saved"): return false
    sample("saved")
    return true

func church() -> bool:
    await key(KEY_ENTER)
    if not await service_page("church", "services"): return false
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await service_page("church", "offer"): return false
    var quote: Dictionary = state().church
    check("1250" in quote.title and ("诅咒" if chinese else "curse") in quote.title, "live Curse quote")
    sample("curse-quote")
    await key(KEY_ENTER)
    if not await service_page("church", "result"): return false
    var paid: Dictionary = state().church
    check("1250" in paid.title and ("诅咒" if chinese else "curse") in paid.title, "localized treatment result")
    var treated := field_wizard(state())
    check(state().gold == 7497 and treated.SourceLoadout.Items[1] == 280 and (int(treated.Status) & 4) == 0,
        "payment clears only the equipped bit and the derived curse")
    sample("curse-paid")
    await key(KEY_ENTER)
    if not await service_page("church", "services"): return false
    await key(KEY_ESCAPE)
    if not await wait_field(): return false
    for cycle in range(480):
        if persistence().availability.get("Allowed", false): break
        await frames(6)
    if not persistence().availability.get("Allowed", false):
        sample("settlement-failed")
        check(false, "treated field reaches a savable player input")
        return false
    await key(KEY_F10)
    await key(KEY_UP)
    await key(KEY_ENTER)
    return await confirm_save()

func repair() -> bool:
    # Castle south to the Pacalon weapon merchant. A separate fork starts one tile from the warp.
    for segment in [[KEY_DOWN,1],[KEY_LEFT,1],[KEY_DOWN,1],[KEY_LEFT,7],
                    [KEY_DOWN,12],[KEY_LEFT,3],[KEY_UP,5]]:
        for step in range(segment[1]):
            await key(segment[0])
            await frames(3)
            if not await wait_field(): return false
    var here := state()
    var player: Dictionary = here.entities.filter(func(row): return row.id == "entity-0")[0]
    check(here.map == "map-2" and player.x == 9 * 384 and player.y == 10 * 384 and player.facing == 1,
        "ordinary route reached the merchant")
    await key(KEY_ENTER)
    for cycle in range(150):
        if state().shop.visible: break
        if state().get("canWaitForText", false): await key(KEY_ENTER)
        await frames(4)
    if not state().shop.visible:
        sample("shop-missing")
        check(false, "merchant opens")
        return false
    await key(KEY_DOWN)
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    var member_name := "卡津" if chinese else "KAZIN"
    var member_found := false
    for member in range(32):
        var shop: Dictionary = state().shop
        if not shop.page.visible: break
        if shop.page.rows[shop.index].text == member_name:
            member_found = true
            break
        await key(KEY_DOWN)
    if not member_found:
        sample("repair-member-missing")
        check(false, "repair owner is the WIZ")
        return false
    await key(KEY_ENTER)
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    var quote: Dictionary = state().shop
    if quote.wait.get("Offer") == null:
        sample("repair-quote-missing")
        check(false, "repair offer opens")
        return false
    check(quote.wait.Offer.Price == 1250 and ring_name in quote.title, "broken ring Repair quote")
    sample("repair-quote")
    await key(KEY_ENTER)
    var repaired := field_wizard(state())
    if mode == "field-repair-worn":
        sample("repair-refused")
        check(("无法修理" if chinese else "cannot be repaired") in state().shop.title
            and state().gold == 8747 and repaired.SourceLoadout.Items[1] == 408
            and (int(repaired.Status) & 4) != 0, "Repair refuses the worn word without charging")
        return failures.is_empty()
    sample("repair-paid")
    check(("物品已修好" if chinese else "item has been repaired") in state().shop.title
        and state().gold == 7497 and repaired.SourceLoadout.Items[1] == 24
        and (int(repaired.Status) & 4) == 0, "paid Repair clears only damage")
    return failures.is_empty()

func run() -> void:
    node_added.connect(node_added_observe)
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    root.size = Vector2i(1920,1080)
    root.grab_focus()
    await frames(15)
    var field := mode.begins_with("field")
    if not await continue_title(field):
        finish()
        return
    if mode == "field-church":
        check((await church()) == true, "church treatment and save complete")
    elif mode in ["field-repair","field-repair-worn"]:
        check((await repair()) == true, "merchant repair complete")
    elif mode in ["battle-damage","battle-destroy"]:
        if await battle_use():
            await key(KEY_F10)
            await key(KEY_UP)
            await key(KEY_ENTER)
            check((await confirm_save()) == true, "battle save complete")
    elif mode == "battle-description":
        await battle_description()
    elif mode == "battle-cold":
        check(pack(state()) == [213,408,17,0] and (int(wizard(state()).status) & 4) != 0,
            "cold battle restored worn damage and curse")
    elif mode == "field-cold":
        var restored := field_wizard(state())
        check(state().get("canWaitAtInput",false) and state().gold == 7497 and restored.SourceLoadout.Items[1] == 280
            and (int(restored.Status) & 4) == 0, "cold field restores treatment, damage, and payment")
    else:
        check(false, "unknown observation case")
    finish()
