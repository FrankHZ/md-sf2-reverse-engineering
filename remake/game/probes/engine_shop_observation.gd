extends SceneTree

# Ordinary device inputs and actual running consumer state. No gameplay setters or synthetic delivery.
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var events: Array = []
var started := Time.get_ticks_msec()

func _initialize() -> void: call_deferred("run")
func check(ok: bool, message: String) -> void:
    if not ok: failures.append(message)
func field() -> Dictionary: return JSON.parse_string(view.call("ReadObservationJson"))
func key(code: Key) -> void:
    if OS.get_environment("SF2_MENU_REMAPPED") == "1":
        code = {KEY_UP:KEY_U,KEY_LEFT:KEY_K,KEY_RIGHT:KEY_L,KEY_DOWN:KEY_J,KEY_ENTER:KEY_Q,KEY_ESCAPE:KEY_B}.get(code,code)
    var event := InputEventKey.new();event.keycode=code;event.pressed=true;Input.parse_input_event(event)
    await process_frame;await process_frame
    event=InputEventKey.new();event.keycode=code;event.pressed=false;Input.parse_input_event(event)
    await process_frame;await process_frame
func result_observed(json: String) -> void:
    var event: Dictionary=JSON.parse_string(json)
    for row in event.observations:
        if str(row.Kind).begins_with("shop-") or row.Kind in ["status","hp","mp","item-consumed","action-committed"]:events.append(row)
func visible_text(label: String) -> void:
    var rows: Array=[]
    for node in host.find_children("*","Control",true,false):
        if not node.is_visible_in_tree() or not (node is Label or node is Button):continue
        var font: Font=node.get_theme_font("font")
        for i in range(node.text.length()):
            if node.text.unicode_at(i)>32:check(font.has_char(node.text.unicode_at(i)),"glyph "+str(node.text.unicode_at(i)))
        check(node.get_minimum_size().y<=node.size.y+1,"minimum height "+str(node.name))
        var parent := node.get_parent()
        var clipped := false
        while parent != null:
            if parent is ScrollContainer: clipped=true;break
            parent=parent.get_parent()
        if not clipped: check(Rect2(Vector2.ZERO,Vector2(root.size)).encloses(node.get_global_rect()),"viewport "+label+" "+str(node.name)+" "+str(node.get_global_rect()))
        rows.append({"node":str(node.name),"text":node.text})
    samples.append({"label":label,"labels":rows})
func picture(label: String) -> void:
    var s:=field()
    samples.append({"label":label,"sessionId":s.sessionId,"revision":s.revision,"map":s.map,"stop":s.stop,
        "wait":s.wait,"gold":s.gold,"party":s.party,"shop":s.shop,"player":s.entities.filter(func(a):return a.id in ["entity-0","traveler"])})
func settle_field() -> Dictionary:
    for frame in range(1200):
        await process_frame
        var s:=field()
        if s.get("failure")!=null or s.get("presentationFailure")!=null:
            visible_text("field-diagnostic");check(false,"field failure "+str(s.get("failure"))+str(s.get("presentationFailure")));return {}
        if s.wait=="ShopWait" or s.stop=="PlayerInput":return s
    check(false,"bounded field settle");return {}
func start_host() -> bool:
    root.size=Vector2i(1920,1080);host=load("res://Main.tscn").instantiate();root.add_child(host)
    await process_frame;await process_frame
    if OS.get_environment("SF2_INSPECTION_START") != "":
        var admission: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_INSPECTION_START"))
        check(admission == "started", "controlled product-world admission: " + admission)
        await process_frame; await process_frame
    root.grab_focus()
    view=host.find_child("ExplorationSessionView",true,false)
    if view==null:
        visible_text("startup-diagnostic");check(false,"ordinary exploration host");return false
    view.connect("SessionResultObserved",result_observed)
    return true
func focus_shop(index: int) -> void:
    for i in range(12):
        if int(field().shop.index) == index: return
        await key(KEY_DOWN)
    check(false, "shop focus " + str(index))

func buy(stock_index: int, member_index: int) -> void:
    await focus_shop(0);await key(KEY_ENTER)
    await focus_shop(stock_index)
    picture("stock");visible_text("stock-text")
    await key(KEY_ENTER)
    await focus_shop(member_index)
    await key(KEY_ENTER)
    var before:=field();var offer: Dictionary=before.shop.wait.Offer
    check(int(before.shop.wait.Phase)==4,"transaction confirmation")
    picture("offer");visible_text("offer-text")
    await key(KEY_ENTER)
    var paid:=field()
    check(int(paid.shop.wait.Result)==0,"purchase succeeded")
    check(paid.gold==before.gold-offer.Price,"exact payment and grant")
    picture("purchased");visible_text("result-text")
    await key(KEY_ENTER)
func art(label: String) -> void:
    await process_frame; await process_frame
    var shop: Dictionary = field().shop
    var original := OS.get_environment("SF2_MENU_ASSETS_EXPECT") == "original"
    if int(shop.wait.Phase) == 0:
        var menu: Dictionary = shop.services
        check(menu.rect.x == root.size.x-592 and menu.rect.y >=32, "right service list beside merchant")
        for i in 4:
            var command: Dictionary = menu.commands[i]
            check(command.id == ["Buy","Sell","Repair","Deals"][i] and command.enabled == (i<2), "source service slot/availability")
            check(command.art.mounted == original, "explicit shop resource mode")
            check(command.rect.width==528 and command.rect.height>=80,"shared command row bounds")
            if original: check(command.art.width==24 and command.art.height==24 and command.art.scale==3,"source command3x")
    else:
        for row in shop.page.rows:
            var icon: Dictionary = row.icon
            if not icon.visible: continue
            check(icon.resource == icon.drawn and icon.texture != null, "actual shop item draw")
            check(icon.width == 16 and icon.height == 24 and icon.scale == 3, "source item dimensions")
    picture(label); visible_text(label+"-text")
func pointer(rect: Dictionary) -> void:
    var point := Vector2(rect.x+rect.width/2,rect.y+rect.height/2)
    var event := InputEventMouseButton.new(); event.position=point;event.button_index=MOUSE_BUTTON_LEFT;event.pressed=true;Input.parse_input_event(event)
    await process_frame;await process_frame
    event=InputEventMouseButton.new();event.position=point;event.button_index=MOUSE_BUTTON_LEFT;event.pressed=false;Input.parse_input_event(event)
    await process_frame;await process_frame
func art_flow() -> void:
    var baseline:=field()
    if OS.get_environment("SF2_MENU_FAULT") == "1":
        check(baseline.shop.error != null,"bad declared shop image is explicit")
        await key(KEY_ENTER)
        check(int(field().shop.wait.Phase)==0 and field().party==baseline.party,"bad image cannot dispatch Buy")
        await key(KEY_ESCAPE);await settle_field()
        check(not field().shop.visible,"bad image retains independent Leave")
        picture("bad-shop-return");return
    for index in range(4):
        await focus_shop(index); await art("service-"+str(index))
    for index in [2,3]:
        await focus_shop(index); await key(KEY_ENTER)
        check(field().gold==baseline.gold and field().party==baseline.party and int(field().shop.wait.Phase)==0, "unavailable service preserves authority")
        await art("unavailable-"+str(index))
    var rect: Dictionary=field().shop.services.commands[0].rect
    var point:=Vector2(rect.x+rect.width/2,rect.y+rect.height/2)
    var mouse:=InputEventMouseButton.new();mouse.position=point;mouse.button_index=MOUSE_BUTTON_LEFT;mouse.pressed=true;Input.parse_input_event(mouse);await process_frame;await process_frame
    await key(KEY_F10);await key(KEY_ESCAPE)
    mouse=InputEventMouseButton.new();mouse.position=point;mouse.button_index=MOUSE_BUTTON_LEFT;mouse.pressed=false;Input.parse_input_event(mouse);await process_frame;await process_frame
    check(int(field().shop.wait.Phase)==0,"covered shop gesture cannot select")
    await focus_shop(0)
    var held:=InputEventKey.new();held.keycode=KEY_Q if OS.get_environment("SF2_MENU_REMAPPED")=="1" else KEY_ENTER;held.pressed=true;Input.parse_input_event(held)
    for frame in 20:await process_frame
    check(int(field().shop.wait.Phase)==1,"held service Confirm never chooses a stock item")
    held=InputEventKey.new();held.keycode=KEY_Q if OS.get_environment("SF2_MENU_REMAPPED")=="1" else KEY_ENTER;held.pressed=false;Input.parse_input_event(held);await process_frame;await process_frame
    await art("stock")
    if OS.get_environment("SF2_SHOP_CASE") == "weapon":
        await focus_shop(6)
        check(field().shop.page.scroll>0,"weapon stock scrolls beside independent Back")
        await art("stock-scrolled")
        await focus_shop(0)
    await key(KEY_DOWN);await art("stock-next")
    await key(KEY_ENTER);await key(KEY_ENTER);await art("buy-offer")
    check(int(field().shop.wait.Phase)==4, "real Buy confirmation")
    await key(KEY_ESCAPE);check(int(field().shop.wait.Phase)==2, "confirmation returns to real recipient")
    await key(KEY_ESCAPE);check(int(field().shop.wait.Phase)==1, "recipient returns to stock")
    await key(KEY_ESCAPE);check(int(field().shop.wait.Phase)==0, "stock returns to services")
    # Sell the real source equipped slot, then buy the first actual stock item with those proceeds.
    await focus_shop(1);await key(KEY_ENTER);await focus_shop(0);await key(KEY_ENTER);await focus_shop(0);await art("sale-slots")
    await key(KEY_ENTER);await art("sale-offer")
    var before:=field();var offer: Dictionary=before.shop.wait.Offer
    check(int(before.shop.wait.Phase)==4 and offer.Slot==0, "actual sale slot0")
    await key(KEY_ENTER)
    check(int(field().shop.wait.Result)==1 and field().gold==before.gold+offer.Price, "real sale receipt")
    await key(KEY_ENTER)
    await focus_shop(0);await key(KEY_ENTER);await focus_shop(0);await key(KEY_ENTER);await focus_shop(0);await key(KEY_ENTER)
    before=field();offer=before.shop.wait.Offer;await art("purchase-offer")
    await key(KEY_ENTER)
    if before.gold >= offer.Price:
        check(int(field().shop.wait.Result)==0 and field().gold==before.gold-offer.Price, "real purchase receipt")
    else:
        check(int(field().shop.wait.Result)==3 and field().gold==before.gold and field().party==before.party, "real insufficient-gold result preserves resources")
    await key(KEY_ENTER);await process_frame;await process_frame
    await pointer(field().shop.services.commands[0].rect)
    check(int(field().shop.wait.Phase)==1, "pointer Buy enters actual stock")
    await pointer(field().shop.page.rows[0].icon.rect)
    check(int(field().shop.wait.Phase)==2,"pointer on item image selects its actual stock row")
    await key(KEY_ESCAPE);await key(KEY_ESCAPE)
    await pointer(field().shop.services.backRect)
    await settle_field()
    check(field().wait==null and not field().shop.visible, "independent Leave restores source caller")
    picture("art-field-return")
func run() -> void:
    if not await start_host():finish();return
    var initial:=await settle_field()
    if initial.is_empty():finish();return
    picture("initial-private")
    check(initial.map=="map-16","actual Galam map")
    await key(KEY_ENTER);var menu:=await settle_field()
    if menu.is_empty():finish();return
    check(menu.wait=="ShopWait" and menu.shop.visible,"actual source caller opened shop")
    if menu.wait != "ShopWait": picture("caller-failure");finish();return
    var item_case:=OS.get_environment("SF2_SHOP_CASE")!="weapon"
    check(menu.shop.wait.Definition.Stock==([0.0,1.0,3.0,4.0] if item_case else [57.0,72.0,73.0,33.0,85.0,86.0,103.0]),"real selected source inventory")
    # Allow the newly shown native label/container layout to settle before measuring it.
    await process_frame;await process_frame
    picture("real-caller");visible_text("menu-text")
    if OS.get_environment("SF2_SHOP_ART") == "1":
        await art_flow();finish();return
    await buy(1 if item_case else 0,0)
    if item_case:await buy(2,1)
    # Sell the actual newly purchased slot, while retaining the source equipped first slot.
    await focus_shop(1);await key(KEY_ENTER);await focus_shop(0);await key(KEY_ENTER);await focus_shop(1);await key(KEY_ENTER)
    var offered:=field();var offer: Dictionary=offered.shop.wait.Offer
    check(int(offered.shop.wait.Phase)==4 and offer.Slot==1,"actual held sale slot")
    picture("sale-offer");await key(KEY_ENTER)
    check(field().gold==offered.gold+offer.Price and int(field().shop.wait.Result)==1,"sale receipt")
    picture("sold");await key(KEY_ENTER);await key(KEY_ESCAPE)
    var returned:=await settle_field()
    check(returned.stop=="PlayerInput" and returned.wait==null,"caller returned usable field control")
    check(returned.sessionId==initial.sessionId,"same authoritative session")
    var before_x=returned.entities.filter(func(a):return a.id=="entity-0")[0].x
    await key(KEY_LEFT);var moved:=await settle_field()
    check(moved.entities.filter(func(a):return a.id=="entity-0")[0].x<before_x,"subsequent ordinary field movement")
    picture("subsequent-input");visible_text("field-text")
    finish()
func finish() -> void:
    var file:=FileAccess.open(OS.get_environment("SF2_OBSERVATION_OUTPUT"),FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(),"failures":failures,"elapsedMs":Time.get_ticks_msec()-started,"samples":samples,"events":events},"  "))
    file.close()
    if host!=null:host.queue_free()
    quit(0 if failures.is_empty() else 1)
