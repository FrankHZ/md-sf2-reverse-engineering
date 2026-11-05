extends "res://probes/engine_dialogue_confirm_observation.gd"

# Actual shared panel and ordinary host; generated content is installed before startup.
var locale := OS.get_environment("SF2_REFLOW_LOCALE")
var baseline := OS.get_environment("SF2_REFLOW_BASELINE") == "1"
var entry: Node

func compact(s: Dictionary) -> Dictionary:
    var result: Dictionary = {}
    for name in ["token","wait","simulationTick","displayLocale","displayFallback","visibleCharacters","totalCharacters","dialoguePanel","canWaitForText","canWaitAtInput","portraitWindow","failure"]:
        result[name] = s.get(name)
    result.hardBreaks = s.get("dialogue","").count("\n")
    return result

func record(label: String) -> Dictionary:
    var s := state()
    samples.append({"label":label,"state":compact(s)})
    require(s.failure == null and s.sessionId == identity,"healthy same-session " + label)
    return s

func _finish() -> void:
    if is_instance_valid(view): samples.append({"label":"terminal","state":compact(state())})
    view = null # Parent writes no duplicated full text-work/unit dump.
    super._finish()

func prepare_package() -> void:
    super.prepare_package()
    var path := OS.get_environment("SF2_READING_PACKAGE")
    var doc: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(path))
    var english := "A short opening{N}continues across the available reading width, with enough words to wrap naturally.\n\nAn explicit paragraph stays separate.{N}{N}A source blank line stays separate.{W1} Appended words remain readable.{W1}"
    var chinese := "开场的简短文字{N}按当前阅读区域的宽度自然排列，让句子完整呈现。\n\n明确分段保持独立。{N}{N}原文空行保持独立。{W1}追加内容仍然完整可读。{W1}"
    if reading_case.contains("long"):
        english = ("A source layout line{N}continues with enough words to fill the modern reading area. ").repeat(30) + "{W1} Appended ending.{W1}"
        chinese = ("原版布局的短行{N}在现代阅读区域中按宽度排列，较长文字通过分页完整阅读。").repeat(30) + "{W1}追加的结尾。{W1}"
    if reading_case.contains("name"):
        english = "{NAME;0}{N}joins{N}{NAME;0}{W1} Appended words.{W1}"
        chinese = "{NAME;0}{N}joins{N}{NAME;0}{W1}追加内容。{W1}"
    if reading_case.contains("choice"):
        english = "First{N}line."
        chinese = "第一{N}行。"
        doc.world.programs[0].instructions = [{"op":"text-cursor","text":100},
            {"op":"show-text","mode":"single","speaker":null},{"op":"yes-no","flag":10},{"op":"end"}]
    doc.world.texts[0].text = english
    var output := FileAccess.open(path,FileAccess.WRITE)
    output.store_string(JSON.stringify(integral_numbers(doc))); output.close()
    var overlay := {"package":doc.package,"locale":"zh-CN","texts":[{"namespace":"world.text","id":100,"text":chinese}]}
    var overlay_path := path.get_base_dir().path_join("display.json")
    output = FileAccess.open(overlay_path,FileAccess.WRITE)
    output.store_string(JSON.stringify(integral_numbers(overlay))); output.close()
    OS.set_environment("SF2_DISPLAY_CATALOG",overlay_path)

func geometry(panel: Dictionary, bounds: Vector2) -> bool:
    var p: Dictionary = panel.panel
    var b: Dictionary = panel.body
    var h: Dictionary = panel.hintRect
    return require(p.x >= 0 and p.y >= 0 and p.x+p.width <= bounds.x+1 and p.y+p.height <= bounds.y+1 and
        b.x >= p.x and b.x+b.width <= p.x+p.width+1 and b.y+b.height <= h.y+1 and panel.fontSize == 30,
        "panel, body and footer fit without shrinking text")

func entry_state() -> Dictionary:
    return JSON.parse_string(entry.call("ReadEntryObservationJson"))

func _run_entry() -> void:
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    entry = host.find_child("SessionEntryView",true,false)
    if not require(entry != null,"ordinary title mounted"): return
    if not require(entry_state().assetError == null,"existing entry input admitted"): return
    if not await tap(KEY_ENTER): return
    for page in range(5):
        var s := entry_state()
        if not require(s.page == page,"Witch page stays on expected segment"): return
        samples.append({"label":"entry-page", "page":page,"locale":s.displayLocale,"characters":s.totalCharacters,
            "hardBreaks":s.message.count("\n"),"dialogue":s.dialogue})
        if not geometry(s.dialogue,Vector2(1920,1080)): return
        if not require(s.dialogue.panel.x >= 1216 and s.dialogue.panel.y+s.dialogue.panel.height <= 504,"entry safe region"): return
        if not baseline and not require(not s.message.contains("\n"),"confirmed entry source lines reflow"): return
        if not await tap(KEY_ENTER): return # Existing entry reveal-first policy.
        while entry_state().dialogue.moreBelow:
            if not await tap(KEY_ENTER): return
        if not await tap(KEY_ENTER): return
    if not require(entry_state().stage == "menu","entry reaches ordinary menu without save access"): return
    if not require(entry_state().displayLocale == locale,"entry locale admitted"): return
    if reading_case.contains("switch"):
        # Real menu navigation and Apply in an isolated external preferences namespace.
        var menu := entry_state()
        for i in range(3):
            if not await tap(KEY_J): return
        if not await tap(KEY_ENTER): return
        if not require(host.find_child("SettingsView",true,false) != null,"safe entry Settings opened"): return
        for i in range(4):
            if not await tap(KEY_J): return
        if not await tap(KEY_ENTER): return
        if not await tap(KEY_J): return
        if not await tap(KEY_J): return
        if not await tap(KEY_ENTER): return
        if not require(host.find_child("SettingsView",true,false) == null,"locale Apply closes settings"): return
        var changed := entry_state()
        if not require(changed.displayLocale == "zh-CN" and changed.stage == "menu" and changed.textureIds == menu.textureIds,
            "locale Apply refreshes the same entry without resource or page reset"): return
        samples.append({"label":"locale-applied","locale":changed.displayLocale,"dialogue":changed.dialogue})
    _finish()

func _run() -> void:
    root.size = Vector2i(2560,1440) if reading_case.contains("wide") else Vector2i(1920,1080)
    if reading_case.begins_with("entry"):
        await _run_entry()
        return
    prepare_package()
    host = (load("res://Main.tscn") as PackedScene).instantiate()
    root.add_child(host)
    await process_frame
    await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if not require(view != null,"ordinary field host mounted"): return
    identity = state().sessionId
    if not await wait_until(func(): return state().canWaitAtInput,"field control"): return
    if not await tap(KEY_ENTER): return
    if not await wait_until(func(): return state().wait in ["FieldTextWait","W1TextWait","DialogueWait"],"source text mounted"): return
    var first := record("source-layout")
    if not geometry(first.dialoguePanel,Vector2(root.size)): return
    if not require(first.dialoguePanel.portraitVisible == reading_case.contains("portrait"),"expected portrait reservation"): return
    if not baseline:
        if not require(not first.dialogue.contains("opening\n") and not first.dialogue.contains("文字\n"),"soft source break reflowed"): return
        if not reading_case.contains("long") and not reading_case.contains("name") and not reading_case.contains("choice") and not require(first.dialogue.count("\n\n") == 2,"literal and source blank paragraphs preserved"): return
    if reading_case.contains("name"):
        if not require(first.dialogue == "Leader joins Leader","resolved Latin names retain word spacing in Chinese display"): return
    if not require(first.displayLocale == locale,"requested display locale admitted without fallback"): return
    if reading_case.contains("long"):
        if not await tap(KEY_ENTER): return
        var pages := 0
        while state().dialoguePanel.moreBelow and pages < 50:
            if not await tap(KEY_ENTER): return
            pages += 1
            if not require(state().token == first.token,"reading page cannot acknowledge"): return
        if not require(pages > 0 and not state().dialoguePanel.moreBelow,"all long text readable"): return
        record("last-page")
    if not await tap(KEY_ENTER): return
    if not await wait_until(func(): return state().token != first.token,"one Confirm advances source span",20.0): return
    var next := record("append")
    if not require(next.dialogue.begins_with(first.dialogue),"append retains display prefix"): return
    if reading_case.contains("choice"):
        if not require(next.wait == "ChoiceWait" and next.displayLocale == first.displayLocale,"choice retains resolved question"): return
        if not await text_ready(): return
        if not await tap(KEY_X): return
        if not await wait_until(func(): return state().canWaitAtInput,"fresh decline completes choice"): return
        _finish()
        return
    if reading_case.contains("long"):
        while state().dialoguePanel.moreBelow:
            if not await tap(KEY_ENTER): return
    if not await tap(KEY_ENTER): return
    if not await wait_until(func(): return state().canWaitAtInput,"genuine close returns field control",20.0): return
    record("return")
    _finish()
