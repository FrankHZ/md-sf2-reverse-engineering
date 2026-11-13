extends "res://probes/engine_pre_voyage_guidance_observation.gd"

# Ordinary slot 2 Continue, mapped key edges, actual field projection and menu transactions.
var locale: String
var reading: String
var initial_gold: int
var initial_party: Array
var translated := {}

# Compare the rendered words with the selected private catalog/source span. The panel
# reflows source newlines for each language and never displays the control tokens.
func body_matches(s: Dictionary, id: int, span: int) -> bool:
    var expected: String = s.fieldText.Projection
    if locale == "zh-CN":
        var spans: PackedStringArray = str(translated[id]).split("{W2}")
        expected = "".join(spans.slice(0, span + 1)).replace("{N}", "").replace("{D3}", "")
    var actual := str(s.dialogue).replace(" ", "").replace("\n", "")
    return not actual.is_empty() and actual == expected.replace(" ", "").replace("\n", "")

func _initialize() -> void:
    call_deferred("run")

func focus_native() -> void:
    pass # The headless game window already owns input.

func state() -> Dictionary:
    return observation(find("ExplorationSessionView"), "ReadObservationJson")

func snap(label: String) -> void:
    var s := state()
    samples.append({"step":label, "map":s.get("map"), "wait":s.get("wait"), "text":s.get("textId"),
        "displayLocale":s.get("displayLocale"), "visibleCharacters":s.get("visibleCharacters"),
        "token":s.get("token"), "tick":s.get("simulationTick"), "gold":s.get("gold")})

func until(test: Callable, label: String, limit := 900) -> Dictionary:
    for i in limit:
        var s := state()
        if s.get("failure") != null:
            check(false, label + ": " + str(s.failure))
            return {}
        if test.call(s): return s
        await frames(1)
    snap("timeout-" + label)
    check(false, label + " did not arrive")
    return {}

func greeting(id: int) -> Dictionary:
    var s := await until(func(row): return row.get("textId") == id and row.get("wait") == "FieldTextWait", "greeting")
    if s.is_empty(): return s
    check(s.dialoguePanel.visible and s.dialoguePanel.portraitVisible and not s.church.visible and not s.shop.visible,
        "portrait and reading panel before services")
    check(s.displayLocale == locale and s.displayFallback == null, "selected language directly resolved")
    if reading == "instant": check(s.visibleCharacters == -1, "instant projection")
    else: check(s.visibleCharacters >= 0 and s.visibleCharacters < s.totalCharacters, "adjustable reveal starts")
    snap("greeting-" + str(id))
    return s

func church() -> void:
    for visit in 2:
        await edge(KEY_ENTER, true)
        var first := await greeting(110)
        if first.is_empty(): return
        var ready := await until(func(s): return s.get("canWaitForText", false), "first W2")
        if ready.is_empty(): return
        check(ready.fieldText.Wait2 and body_matches(ready, 110, 0), "first delivered W2")
        check(ready.token == first.token, "held interaction cannot acknowledge the first span")
        await edge(KEY_ENTER, false)
        await key(KEY_ENTER)
        ready = await until(func(s): return s.get("canWaitForText", false) and s.token != first.token, "second W2")
        if ready.is_empty(): return
        check(ready.fieldText.Wait2 and body_matches(ready, 110, 1),
            "second delivered W2")
        snap("second-W2")
        await edge(KEY_ENTER, true)
        var menu := await until(func(s): return s.get("church", {}).get("visible", false), "church menu")
        if menu.is_empty(): return
        await frames(12)
        check(state().church.page == "services" and state().church.requiresRelease, "held acknowledgement stops on Church")
        await edge(KEY_ENTER, false)
        check(state().church.page == "services", "release cannot select Raise")
        await key(KEY_ENTER)
        check(state().church.page == "result", "fresh Raise shows the healthy-party result")
        await key(KEY_ENTER)
        check(state().church.page == "services" and state().textId == null, "result returns directly to menu")
        snap("church-result-return")
        await key(KEY_ESCAPE)
        await settle("leave church")
        if issue != "": return

func shop() -> void:
    for visit in 2:
        await key(KEY_ENTER)
        var s := await greeting(158)
        if s.is_empty(): return
        var pause := await until(func(row): return row.get("fieldText") != null and row.fieldText.InterruptibleDelay, "D3")
        if pause.is_empty(): return
        check(pause.fieldText.Remaining >= 117 and pause.fieldText.Remaining <= 120, "long delay begins")
        check(body_matches(pause, 158, 0), "complete merchant body in the selected language")
        check(pause.visibleCharacters < 0 or pause.visibleCharacters >= pause.totalCharacters, "merchant body readable at delay")
        snap("shop-delay")
        if visit == 1: await edge(KEY_ENTER, true)
        var menu := await until(func(row): return row.get("shop", {}).get("visible", false), "shop menu")
        if menu.is_empty(): return
        var elapsed: int = int(menu.simulationTick - pause.simulationTick)
        samples.append({"step":"D3-menu", "interrupted":visit == 1, "ticks":elapsed})
        check(elapsed < 40 if visit == 1 else elapsed >= pause.fieldText.Remaining, "skip or neutral timeout")
        if visit == 1:
            await frames(12)
            check(state().shop.requiresRelease and state().shop.wait.Phase == 0, "held interrupt stops on Shop")
            await edge(KEY_ENTER, false)
        check(state().shop.wait.Phase == 0, "fresh input required to shop")
        await key(KEY_ENTER)
        check(state().shop.wait.Phase == 1, "fresh Buy opens actual stock")
        await key(KEY_ESCAPE)
        check(state().shop.wait.Phase == 0 and state().textId == null, "stock returns directly to services")
        snap("shop-stock-return")
        await key(KEY_ESCAPE)
        await settle("leave shop")
        if issue != "": return

func run() -> void:
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await frames(12)
    for code in [KEY_ENTER, KEY_E, KEY_DOWN, KEY_ENTER]: await key(code)
    if not await wait_slots("continue-slots"): finish(); return
    await key(KEY_DOWN); await key(KEY_ENTER)
    var s := await until(func(row): return row.get("canWaitAtInput", false), "Continue slot 2", 1800)
    if s.is_empty(): finish(); return
    locale = applied().Language
    if locale == "zh-CN":
        var catalog: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(OS.get_environment("SF2_DISPLAY_CATALOG")))
        for row in catalog.texts:
            if int(row.id) in [110, 158]: translated[int(row.id)] = row.text
        if translated.size() != 2:
            check(false, "both service rows in the private catalog"); finish(); return
    reading = s.textMode
    check(case_name == locale + "-" + reading and s.map == "map-3" and 609.0 in s.flags, "selected F609 language and reading mode")
    initial_gold = s.gold
    initial_party = s.party
    for counter in ["church", "shop"]:
        await walk(Vector2i(32,13) if counter == "church" else Vector2i(7,20), counter)
        if issue != "": check(false, issue); break
        await key(KEY_UP); await settle("face counter")
        if counter == "church": await church()
        else: await shop()
        if not failures.is_empty(): break
        check(state().gold == initial_gold and state().party == initial_party, "reading/subpage/exit preserve gold and party")
    finish()

func finish() -> void:
    if ended: return
    ended = true
    var file := FileAccess.open(output, FileAccess.WRITE)
    file.store_string(JSON.stringify({"passed":failures.is_empty(), "checks":checks, "failures":failures,
        "case":case_name, "samples":samples}))
    file.close()
    host.queue_free()
    await frames(8)
    quit(0 if failures.is_empty() else 1)
