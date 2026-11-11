extends "res://probes/engine_field_menu_observation.gd"

# Production F5–F7: mapped input, finite state/geometry samples, no gameplay setters or screenshots.
func state(label: String = "") -> Dictionary:
    var s: Dictionary = JSON.parse_string(view.call("ReadObservationJson"))
    if not label.is_empty():
        samples.append({"label":label,"fieldMenu":s.fieldMenu,"partyStatus":s.partyStatus,"shop":s.shop,
            "revision":s.revision,"party":s.party,"gold":s.gold,"mainSeed":s.get("mainSeed"),"thinkingSeed":s.get("thinkingSeed"),"flags":s.flags,"tick":s.simulationTick,"wait":s.wait,"failure":s.failure,"canWaitAtInput":s.canWaitAtInput})
    return s

func key(code: Key, pressed: bool) -> void:
    if OS.get_environment("SF2_FIELD_UI_REMAP") == "1":
        code = {KEY_ENTER:KEY_F5,KEY_ESCAPE:KEY_F6}.get(code,code)
    var e := InputEventKey.new()
    e.keycode = code; e.pressed = pressed; Input.parse_input_event(e)
    await frames(2)

func inside(rect: Dictionary, label: String) -> void:
    check(rect.x >= 31 and rect.y >= 31 and rect.x + rect.width <= root.size.x - 31 and rect.y + rect.height <= root.size.y - 31, label + " safe bounds")

func choices(page: Dictionary, label: String) -> void:
    inside(page.rect, label)
    inside(page.backRect, label + " Back")
    check(page.hintRect.y + page.hintRect.height <= page.backRect.y + 1, label + " footer separate")
    for row in page.rows:
        check(row.minimumHeight <= row.rect.height + 1, label + " full row reading")
        check(row.textRect.x - row.rect.x == 88, label + " fixed marker/icon columns")

func commands(menu: Dictionary, label: String) -> void:
    inside(menu.rect, label)
    check(menu.rect.x >= root.size.x - 592, label + " right service region")
    var y: float = -1
    for row in menu.list.rows:
        check(row.rect.y > y and row.rect.height >= 80, label + " ordered tall rows")
        y = row.rect.y
        check(row.labelRect.x - row.rect.x == 108, label + " fixed caption column")
        if OS.get_environment("SF2_FIELD_UI_ART") == "1": check(row.art.mounted, label + " required original command art")
        if row.art.mounted: check(row.art.scale == 3 and row.art.width == 24 and row.art.height == 24, label + " original source art at3x")

func signature(s: Dictionary) -> Array:
    return [s.party, s.gold, s.get("mainSeed"), s.get("thinkingSeed"), s.flags]

func preparation() -> Dictionary:
    return state().partyStatus.preparation

func root_focus(index: int) -> void:
    for i in range(5):
        if state().fieldMenu.index == index: break
        await press(KEY_DOWN)

func action_focus(index: int) -> void:
    for i in range(8):
        if int(preparation().index) == index: break
        await press(KEY_DOWN)

func field_flow() -> void:
    var initial := state()
    await press(back_key)
    commands(state().fieldMenu.menu, "field root")
    var tick = state().simulationTick
    await frames(15)
    check(state().simulationTick == tick, "field command list pauses ordinary time")
    await root_focus(1); await press(confirm_key)
    var party: Dictionary = state("magic-member").partyStatus
    check(party.visible and party.selectorOpen and party.focusPane == 1, "persistent member column and magic pane")
    inside(party.card.rect, "member detail")
    inside(party.hintRect, "member footer")
    check(party.hintMinimumHeight <= party.hintRect.height + 1, "complete member hint")
    check(party.layout.items.entries.size() == 4, "four real slots in member sheet")
    await press(confirm_key)
    check(preparation().stage == "magic", "actual learned spell page")
    choices(preparation().page, "spells")
    await press(back_key); await press(back_key)
    check(state().fieldMenu.index == 1, "Magic returns initiating selection")
    await root_focus(2); await press(confirm_key); await press(confirm_key)
    check(preparation().stage == "item-actions", "owner before operations")
    commands(preparation().operations, "item commands")
    await press(confirm_key)
    var page: Dictionary = preparation().page
    check(preparation().stage == "use-item" and page.rows.size() == 4, "Use keeps numbered empty positions")
    choices(page, "item slots")
    var slots: Array = page.rows.map(func(r): return r.rect)
    var long_text := OS.get_environment("SF2_FIELD_UI_LONG") == "1"
    await pad(JOY_BUTTON_DPAD_DOWN)
    if not long_text: check(preparation().page.rows.map(func(r): return r.rect) == slots, "pad focus leaves every item hit area still")
    await action_focus(3); await press(confirm_key)
    check(preparation().stage == "result", "empty slot is inspectable refusal")
    check(signature(state()) == signature(initial), "empty slot cannot use previous valid item")
    await press(back_key)
    check(preparation().stage == "use-item" and preparation().index == 3, "refusal Back restores actual empty slot")
    await press(back_key)
    await action_focus(1); await press(confirm_key)
    await action_focus(1); await press(confirm_key)
    check(preparation().stage == "give-target", "duplicate item selected by slot")
    check(preparation().pack.rows.size() == 4, "recipient pack preview")
    await action_focus(1); await press(confirm_key)
    var full: bool = preparation().stage == "exchange"
    if OS.get_environment("SF2_FIELD_UI_FULL") == "1": check(full, "full pack enters actual exchange")
    if full:
        choices(preparation().page, "exchange return slots")
        await action_focus(2); await press(confirm_key)
    check(preparation().stage == "confirm", "explicit Give/exchange confirmation")
    if long_text:
        inside(preparation().pack.rect, "long recipient pack")
        var reading_before = preparation().page.scroll
        for i in range(12): await press(KEY_LEFT)
        check(preparation().page.scroll == 0, "mapped reading reaches whole quote start")
        for i in range(12): await press(KEY_RIGHT)
        check(preparation().page.scroll > 0, "mapped reading reaches whole quote end")
        check(preparation().pack.scroll > 0, "mapped reading reaches full long pack")
        state("long-quote-reading")
    var before := state()
    await press(back_key)
    check(preparation().stage == ("exchange" if full else "give-target") and signature(state()) == signature(before), "cancel retains resources and immediate parent")
    await press(confirm_key)
    choices(preparation().page, "transaction quote")
    await key(confirm_key, true); await frames(15)
    check(preparation().stage == "result" and state().revision == before.revision + 1, "held Give commits exactly once")
    var committed := state("give-committed")
    await frames(15)
    check(state().revision == committed.revision and signature(state()) == signature(committed), "held result input cannot repeat Give")
    await key(confirm_key, false); await press(confirm_key)
    await action_focus(2); await press(confirm_key)
    check(preparation().stage == "equipment", "equipment preview uses fixed source slots")
    var equipment_before := state(); await press(back_key)
    check(signature(state()) == signature(equipment_before),"equipment preview cancellation is free")
    await action_focus(3); await press(confirm_key); await action_focus(0); await press(confirm_key)
    check(preparation().stage == "confirm","Drop keeps explicit final confirmation")
    var drop_before := state(); await press(back_key)
    check(preparation().stage == "drop-item" and preparation().index == 0 and signature(state()) == signature(drop_before),"Drop Back retains exact slot and resources")
    await press(confirm_key); await press(confirm_key)
    check(preparation().stage == "result" and state().party != drop_before.party and state().gold == drop_before.gold,"Drop removes only after real confirmation")
    state("drop-committed"); await press(confirm_key)
    await press(back_key); await press(back_key)
    check(state().fieldMenu.visible and state().fieldMenu.index == 2, "committed action unwinds to Item")
    for size in [Vector2i(2560,1440), Vector2i(3840,2160), Vector2i(1920,1080)]:
        root.size = size; await frames(5); commands(state().fieldMenu.menu, "resize " + str(size))
    # Completed pointer gesture and right-click return; no click-through from held opening input.
    await click(state().fieldMenu.menu.commands[0].rect)
    check(state().partyStatus.visible and not preparation().visible, "pointer row opens only Member")
    await mouse(Vector2(80,80), true, MOUSE_BUTTON_RIGHT); await mouse(Vector2(80,80), false, MOUSE_BUTTON_RIGHT)
    check(state().fieldMenu.visible, "right click returns to same field list")
    await root_focus(3)
    if state().fieldMenu.menu.commands[3].enabled:
        await press(confirm_key)
        check(state().wait == "FieldSearchWait" and not state().fieldMenu.visible, "Search submits the real faced consumer")
        await press(back_key); await frames(8)
        check(not state().fieldMenu.visible and state().canWaitAtInput, "Search result Back returns usable field")
    else:
        var declined := state(); await press(confirm_key)
        check(state().fieldMenu.visible and state().revision == declined.revision, "unavailable Search submits nothing")
        await press(back_key)
    state("field-final")

func shop_focus(index: int) -> void:
    for i in range(12):
        if int(state().shop.index) == index: break
        await press(KEY_DOWN)

func phase(value: int, label: String) -> void:
    check(int(state().shop.wait.Phase) == value, label)
    if value != 0: choices(state().shop.page, label)

func shop_flow() -> void:
    await press(confirm_key)
    for i in range(600):
        if state().wait == "ShopWait": break
        await frames(1)
    if not state().shop.visible:
        state("shop-start-failed"); check(false,"ordinary merchant caller opened"); return
    commands(state().shop.services, "shop services")
    var baseline := state()
    await shop_focus(2); await press(confirm_key)
    check(int(state().shop.wait.Phase) == 0 and signature(state()) == signature(baseline), "Repair preserves real unavailable boundary")
    commands(state().shop.services, "shop refusal")
    await click(state().shop.services.commands[0].rect); phase(1,"pointer Buy stock")
    await click(state().shop.page.rows[1].rect); phase(2,"pointer stock recipient")
    check(state().shop.pack.visible and state().shop.pack.rows.size() == 4, "focused recipient's four slots shown")
    var pack: Dictionary = state().shop.pack
    await shop_focus(1)
    check(state().shop.pack.title != pack.title, "recipient focus projects the correct pack")
    await press(confirm_key); phase(4,"Buy quote")
    check("→" in state().shop.title, "quote shows before/after gold")
    var quoted := state()
    await press(back_key); phase(2,"quote Back")
    check(state().shop.index == 1 and signature(state()) == signature(quoted), "recipient Back preserves selection and resources")
    await press(confirm_key)
    await key(confirm_key,true); await frames(15)
    var paid := state("bought")
    check(int(paid.shop.wait.Phase) == 5, "Buy produces actual result")
    var offer: Dictionary = quoted.shop.wait.Offer
    if int(paid.shop.wait.Result) == 0:
        check(paid.gold == quoted.gold-offer.Price and paid.party != quoted.party, "actual payment and inventory grant")
    else:
        check(signature(paid) == signature(quoted), "refused purchase retains every resource")
    await frames(15); check(state().revision == paid.revision, "held buy confirms only once")
    await key(confirm_key,false); await press(confirm_key); phase(0,"purchase acknowledgement")
    await shop_focus(1); await press(confirm_key); phase(2,"Sell owner")
    await shop_focus(0); await press(confirm_key); phase(3,"Sell slots")
    check(state().shop.page.rows.size() == 4, "Sell retains fixed empty and occupied slots")
    var slots: Array = state().shop.page.rows.map(func(r): return r.rect)
    await shop_focus(1)
    check(state().shop.page.rows.map(func(r): return r.rect) == slots,"Sell focus preserves slot hit areas")
    await shop_focus(0); await press(confirm_key); phase(4,"Sell quote")
    var sale := state(); var sale_offer: Dictionary = sale.shop.wait.Offer
    await press(back_key); phase(2,"sale cancellation returns source owner")
    check(signature(state()) == signature(sale),"sale cancellation pays/removes nothing")
    await press(confirm_key); phase(3,"return to retained sale slot"); await press(confirm_key); await press(confirm_key)
    var sold := state("sold")
    check(int(sold.shop.wait.Phase) == 5 and int(sold.shop.wait.Result) == 1 and sold.gold == sale.gold+sale_offer.Price and sold.party != sale.party,"real sale removes exact slot and pays exact price")
    await press(confirm_key); phase(0,"sale acknowledgement")
    # Buy repeatedly through ordinary confirmations until the recipient's real pack refuses.
    await shop_focus(0); await press(confirm_key); await shop_focus(0); await press(confirm_key); await shop_focus(1); await press(confirm_key)
    for i in range(5):
        var before := state(); await press(confirm_key)
        var result := state()
        if int(result.shop.wait.Result) == 0:
            check(result.gold == before.gold-before.shop.wait.Offer.Price and result.party != before.party,"refill pays and grants exactly once")
            state("refill-purchase")
        if int(result.shop.wait.Result) != 0:
            check(signature(result) == signature(before),"full/funds refusal is atomic")
            state("capacity-or-funds-refusal"); break
        await press(confirm_key); await shop_focus(0); await press(confirm_key); await shop_focus(0); await press(confirm_key); await shop_focus(1); await press(confirm_key)
    if int(state().shop.wait.Phase) == 5: await press(confirm_key)
    else:
        await press(back_key); await press(back_key); await press(back_key)
    await click(state().shop.services.backRect)
    for i in range(600):
        if state().canWaitAtInput: break
        await frames(1)
    check(state().canWaitAtInput and not state().shop.visible,"Leave completes actual source caller")
    var old = state().entities
    await press(KEY_LEFT); await frames(60)
    check(state().entities != old,"usable ordinary movement after shop return")
    state("shop-final")
    if OS.get_environment("SF2_FIELD_UI_ART") == "1":
        var field_before := state()
        await press(back_key); commands(state().fieldMenu.menu,"complete-world field commands")
        var paused := state("complete-world-menu-opened")
        await click(state().fieldMenu.menu.commands[2].rect)
        check(state().partyStatus.visible,"complete-world actual item owner")
        var party: Dictionary = state().partyStatus
        check(party.layout.items.entries.size() == 4,"complete-world fixed pack")
        for row in party.layout.items.entries:
            if row.icon.visible: check(row.icon.scale == 3 and row.icon.width == 16 and row.icon.height == 24,"complete-world identity art")
        await press(confirm_key)
        commands(preparation().operations,"complete-world item commands")
        await press(confirm_key); choices(preparation().page,"complete-world Use slots")
        await press(back_key); await press(back_key); await press(back_key)
        check(signature(state()) == signature(paused) and state().simulationTick == paused.simulationTick,"complete-world paused menus preserve resources/RNG/flags/time")
        await press(back_key)
        check(state().canWaitAtInput and not state().fieldMenu.visible and not state().partyStatus.visible,"complete-world free return")
        check(state().party == field_before.party and state().gold == field_before.gold,"complete-world preparation inspection spends no party resources")
        state("complete-world-field-return")

func run() -> void:
    root.size = Vector2i(1920,1080)
    if OS.get_environment("SF2_FIELD_UI_LONG") == "1":
        # Legal UI translation variation only; keep the product's exact operand arity.
        var text := "Read the complete consequence before confirming. ".repeat(24)
        for path in ["res://localization/en.po","res://localization/zh_CN.po"]:
            var translation: Translation = load(path)
            translation.add_message("prepare.confirm-exchange", text + " {0} / {1}")
            translation.add_message("shop.confirm-buy", text + " {0} / {1} / {2}")
            translation.add_message("shop.unavailable", text)
            translation.add_message("field-menu.search-help", text)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(8)
    if OS.get_environment("SF2_INSPECTION_START") != "":
        var admission = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
        check(admission == "started", "controlled production-world admission: " + str(admission)); await frames(8)
    root.grab_focus()
    view = host.find_child("ExplorationSessionView",true,false)
    if view == null:
        samples.append({"labels":host.find_children("*","Label",true,false).map(func(n):return n.text)})
        check(false,"ordinary exploration mounted"); await finish(); return
    for i in range(900):
        if state().canWaitAtInput: break
        await frames(1)
    check(state().failure == null and state().canWaitAtInput,"initial usable field")
    if mode == "shop": await shop_flow()
    else: await field_flow()
    await finish()
