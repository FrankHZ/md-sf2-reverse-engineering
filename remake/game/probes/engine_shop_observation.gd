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
        check(Rect2(Vector2.ZERO,Vector2(root.size)).encloses(node.get_global_rect()),"viewport "+str(node.name))
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
    view=host.find_child("ExplorationSessionView",true,false)
    if view==null:
        visible_text("startup-diagnostic");check(false,"ordinary exploration host");return false
    view.connect("SessionResultObserved",result_observed)
    return true
func buy(stock_index: int, member_index: int) -> void:
    await key(KEY_ENTER)
    for i in range(stock_index):await key(KEY_DOWN)
    picture("stock");visible_text("stock-text")
    await key(KEY_ENTER)
    for i in range(member_index):await key(KEY_DOWN)
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
func run() -> void:
    if not await start_host():finish();return
    var initial:=await settle_field()
    if initial.is_empty():finish();return
    picture("initial-private")
    check(initial.map=="map-16","actual Galam map")
    await key(KEY_ENTER);var menu:=await settle_field()
    if menu.is_empty():finish();return
    check(menu.wait=="ShopWait" and menu.shop.visible,"actual source caller opened shop")
    var item_case:=OS.get_environment("SF2_SHOP_CASE")!="weapon"
    check(menu.shop.wait.Definition.Stock==([0.0,1.0,3.0,4.0] if item_case else [57.0,72.0,73.0,33.0,85.0,86.0,103.0]),"real selected source inventory")
    picture("real-caller");visible_text("menu-text")
    await buy(1 if item_case else 0,0)
    if item_case:await buy(2,1)
    # Sell the actual newly purchased slot, while retaining the source equipped first slot.
    await key(KEY_DOWN);await key(KEY_ENTER);await key(KEY_ENTER);await key(KEY_DOWN);await key(KEY_ENTER)
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
