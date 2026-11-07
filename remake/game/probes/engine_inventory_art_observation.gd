extends "res://probes/engine_identity_icons_observation.gd"

# Ordinary New Game/Continue consumers. Bounded state/draw observations, no active setters.
func record(label: String) -> void:
    var s := state()
    var row := {"label":label}
    for k in ["sessionId", "revision", "stage", "actor", "failure", "displayLocale", "fieldMenu", "partyStatus", "party", "inspection", "transactionIcon", "returnIcon", "hudPanel", "inventories", "itemSlot", "returnSlot", "giveMenu", "discardMenu"]:
        if s.has(k): row[k] = s[k]
    samples.append(row)

func sheet(details: Dictionary, label: String) -> void:
    var art: Array = []
    for list in [details.magic, details.items]:
        for row in list.entries:
            art.append(row.icon)
            if row.icon.visible:
                check(row.rect.x >= row.icon.rect.x + row.icon.rect.width, label + ": text clears image")
                check(row.rect.width > 100, label + ": text has readable width")
                check(row.minimumHeight <= row.rect.height and row.rowRect.height >= row.rect.height,
                    label + ": wrapped name fits its list row")
    icons(art, label)
    check(details.items.entries.size() == 4, label + ": four live slots")

func field() -> void:
    if OS.get_environment("SF2_INVENTORY_FAULTS") == "1":
        await field_errors()
        return
    await key(back); await key(KEY_UP); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.visible, "ordinary Member entry")
    sheet(state().partyStatus.layout, "field-member")
    await key(KEY_RIGHT); await frames(4)
    sheet(state().partyStatus.layout, "field-member-next")
    await key(KEY_TAB); await key(KEY_TAB); await key(KEY_DOWN); await frames(4)
    check(state().partyStatus.layout.items.scroll > 0 if expect_art else true, "inventory list scrolls")
    record("field-member-scrolled")
    await key(back); await key(KEY_RIGHT); await key(KEY_ENTER); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.preparation.stage == "item-actions", "ordinary Item operation caller")
    for direction in [KEY_UP, KEY_LEFT, KEY_RIGHT, KEY_DOWN]:
        await key(direction); await frames(4)
        diamond(state().partyStatus.preparation.operations, "item-menu", "item-operation-" + str(direction))
        var page: Dictionary = state().partyStatus
        var operations: Dictionary = page.preparation.operations
        check(not page.memberLayerVisible and not page.preparation.panelVisible and not page.preparation.veilVisible,
            "Item diamond has no common Member/menu panel or veil")
        check(operations.detailRect.x - operations.diamondRect.x - operations.diamondRect.width == 32,
            "separate explanation leaves 32px visible world after diamond")
        check(operations.hintMinimumHeight <= operations.hintRect.height and operations.detailRect.y >= 0
            and operations.detailRect.y + operations.detailRect.height <= 1049, "operation explanation fits viewport")
    var operation_rect: Dictionary = state().partyStatus.preparation.operations.commands[0].rect
    await click(Vector2(operation_rect.x + operation_rect.width / 2, operation_rect.y + operation_rect.height / 2))
    check(state().partyStatus.preparation.stage == "use-item", "pointer selects the actual Use operation")
    await key(back); await frames(3)
    var back_rect: Dictionary = state().partyStatus.preparation.operations.backRect
    await click(Vector2(back_rect.x + back_rect.width / 2, back_rect.y + back_rect.height / 2))
    check(not state().partyStatus.preparation.visible and state().partyStatus.memberLayerVisible,
        "operation pointer Back restores Member layer")
    await key(KEY_ENTER)
    await key(KEY_LEFT); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.preparation.stage == "give-item", "Give actual list")
    icons(state().partyStatus.preparation.icons, "field-give")
    for row in state().partyStatus.preparation.rows:
        check(row.minimumWidth <= row.rect.width and row.minimumHeight <= row.rect.height,
            "Give row retains wrapped name without clipping")
    await key(KEY_DOWN); await frames(4)
    icons(state().partyStatus.preparation.icons, "field-give-slot")
    var before: int = state().revision
    await key(KEY_ENTER); await key(back); await frames(4)
    check(state().partyStatus.preparation.stage == "give-item" and state().partyStatus.preparation.index == 1 and state().revision == before, "recipient Back restores exact slot without transaction")
    await key(back); await key(KEY_DOWN); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.preparation.stage == "drop-item", "Drop actual list")
    icons(state().partyStatus.preparation.icons, "field-drop")
    await key(KEY_ENTER); await key(back); await frames(4)
    check(state().revision == before and state().partyStatus.preparation.stage == "drop-item", "Drop confirmation Back retains resources")
    await key(back); await key(KEY_UP); await key(KEY_ENTER); await frames(4)
    icons(state().partyStatus.preparation.icons, "field-use-preserved")
    record("field-use-unavailable-reason")
    await key(back); await key(back); await key(back)
    check(state().fieldMenu.visible, "Item returns through member to root")
    if expect_art and mode == "field-controlled":
        await key(KEY_RIGHT); await key(KEY_ENTER); await key(KEY_ENTER)
        await key(KEY_LEFT); await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER)
        await key(KEY_DOWN); await key(KEY_ENTER); await key(KEY_ENTER)
        record("field-fill-result")
        await key(KEY_ENTER)
        check(state().partyStatus.preparation.stage == "item-actions", "actual Give fills recipient and restores actions")
        await key(KEY_LEFT); await key(KEY_ENTER); await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER); await frames(4)
        check(state().partyStatus.preparation.stage == "exchange", "ordinary full pack opens return slots")
        var exchange: Dictionary = state().partyStatus.preparation
        icons(exchange.icons, "field-exchange")
        check(exchange.icons[1].identity == exchange.icons[2].identity and exchange.icons[1].texture == exchange.icons[2].texture,
            "duplicate return slots borrow one texture")
        await key(KEY_DOWN); await key(KEY_DOWN); await key(KEY_ENTER); await key(back); await frames(4)
        check(state().partyStatus.preparation.stage == "exchange" and state().partyStatus.preparation.index == 2,
            "exchange confirmation Back retains exact duplicate return slot")
        icons(state().partyStatus.preparation.icons, "field-exchange-return")
        await key(KEY_ENTER); await key(KEY_ENTER); await key(KEY_ENTER)
        check(state().partyStatus.preparation.stage == "item-actions", "actual exchange completes and restores Item actions")
        record("field-exchange-committed")
        await key(back); await key(back)
    await key(back); record("field-return")

func battle() -> void:
    var initial := state()
    await key(back); await frames(4)
    check(state().inspection.page == "Cursor", "ordinary Movement B inspection")
    # Reach one empty tile through the actual inspection input, then Options -> Member.
    for step in 10:
        var inspect: Dictionary = state().inspection
        if not inspect.units.any(func(u): return u.Position.X == inspect.cursor.X and u.Position.Y == inspect.cursor.Y): break
        await key(KEY_RIGHT)
    await key(KEY_ENTER); await key(KEY_DOWN); await key(KEY_ENTER); await key(KEY_ENTER); await frames(4)
    check(state().inspection.page == "MemberFull", "ordinary battle Member sheet")
    sheet(state().inspection.full, "battle-member")
    await key(KEY_TAB); await key(KEY_DOWN); await frames(4); record("battle-member-scrolled")
    await key(back)
    check(state().inspection.page == "Members", "sheet returns to same member list")
    await key(KEY_DOWN); await key(KEY_ENTER); await frames(4)
    sheet(state().inspection.full, "battle-member-next")
    for step in 4: await key(back)
    check(not state().inspection.active and state().stage == "Movement" and state().actor == initial.actor, "Member caller returns to same Movement")
    await key(KEY_ENTER); await key(KEY_G); await frames(4)
    if state().stage == "GiveItemChoice":
        icons([state().transactionIcon], "battle-give")
        await key(KEY_DOWN); await frames(4)
        icons([state().transactionIcon], "battle-give-slot")
        await key(back)
    else:
        check(not expect_art, "private Give is offered")
    await key(KEY_R); await frames(4)
    if state().stage == "DiscardItemChoice":
        icons([state().transactionIcon], "battle-drop")
        await key(KEY_ENTER); await frames(4)
        icons([state().transactionIcon], "battle-drop-confirm")
        await key(back); await key(back)
    else:
        check(not expect_art, "private Drop is offered")
    await key(back)
    check(state().inventories == initial.inventories and state().stage == "Movement", "uncommitted Give/Drop returns preserve inventory")
    record("battle-return")

func church() -> void: pass

func click(position: Vector2) -> void:
    var motion := InputEventMouseMotion.new(); motion.position = position; motion.relative = Vector2(8, 0)
    Input.parse_input_event(motion); await frames(2)
    for pressed in [true, false]:
        var button := InputEventMouseButton.new(); button.button_index = MOUSE_BUTTON_LEFT
        button.position = position; button.pressed = pressed; Input.parse_input_event(button); await frames(2)

func field_errors() -> void:
    var before: Array = state().party
    for attempt in 2:
        await key(back); await key(KEY_UP); await key(KEY_ENTER); await frames(4)
        var party: Dictionary = state().partyStatus
        check(party.visible and party.failure != null and not party.preparation.visible,
            "Member decode failure remains in read-only caller")
        record("member-decode-failure-" + str(attempt))
        await key(back); await frames(5)
        check(state().fieldMenu.visible and not state().partyStatus.visible, "Member error Back exits without reopening")
        await key(back)
    await key(back); await key(KEY_RIGHT); await key(KEY_ENTER); await key(KEY_ENTER); await frames(4)
    check(state().partyStatus.preparation.stage == "result" and state().partyStatus.preparation.failure != null,
        "operation decode failure is explicit at operation entry")
    record("operation-decode-failure")
    await key(back); await frames(8)
    check(state().partyStatus.visible and not state().partyStatus.preparation.visible, "operation error Back keeps member caller")
    await key(back); await key(back); await frames(8)
    check(state().party == before and not state().partyStatus.visible and not state().fieldMenu.visible,
        "resource errors preserve gameplay and final Back")
    record("resource-error-return")

func run() -> void:
    if mode != "field-controlled":
        await super.run()
        return
    create_timer(40).timeout.connect(func(): check(false, "controlled field deadline"); finish())
    node_added.connect(node_added_observe)
    root.size = Vector2i(1920,1080)
    host = (load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
    root.grab_focus(); await frames(5)
    var admitted: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
    check(admitted == "started", "existing controlled field admission: " + admitted)
    if not await wait_field(): finish(); return
    await field(); finish()
