extends SceneTree
var host: Node
var view: Node
var failures: Array = []
var samples: Array = []
var case_name := OS.get_environment("SF2_LOCALIZATION_CASE")
var more_seen := false
var accepted: Array = []
var started := Time.get_ticks_msec()
func _initialize() -> void: call_deferred("_run")
func check(ok: bool, message: String) -> void:
    if not ok:
        failures.append(message)
        push_error(message)
func press(key: Key) -> void:
    var e := InputEventKey.new()
    e.keycode = key; e.pressed = true; Input.parse_input_event(e)
    await process_frame; await process_frame
    e = InputEventKey.new(); e.keycode = key; e.pressed = false; Input.parse_input_event(e)
    await process_frame
func state() -> Dictionary: return JSON.parse_string(view.call("ReadObservationJson"))
func sample(label: String) -> Dictionary:
    var s := state()
    var compact := {}
    for key in ["sessionId", "revision", "mode", "canWaitAtInput", "canWaitForText", "randomSeedCopy", "mainSeed", "thinkingSeed", "actors", "fieldText", "textWindow", "logicalText", "displayLocale", "displayFallback", "dialogue", "dialoguePanel", "visibleCharacters", "failure", "stage", "scene", "wait", "flags", "church", "search", "searchBlock", "party", "gold", "hint"]:
        if s.has(key): compact[key] = s[key]
    samples.append({"label": label, "state": compact})
    return s
func field_done(label: String) -> void:
    var last := -1
    var slow_waited := false
    for frame in range(1800):
        var s := state()
        if s.failure != null: samples.append({"label":"error-labels", "labels":view.find_children("*", "Label", true, false).map(func(n): return n.text)}); check(false, "field failure " + str(s.failure)); return
        if s.get("canWaitAtInput", false): sample(label + ":control"); return
        if s.get("dialoguePanel", {}).get("moreBelow", false) and not more_seen:
            more_seen = true; sample(label + ":read-more")
            var label_node := view.find_child("Dialogue", true, false) as Label
            var loaded_font := label_node.get_theme_font("font")
            var missing: Array = []
            for character in label_node.text.length():
                var code := label_node.text.unicode_at(character)
                if code > 32 and not loaded_font.has_char(code): missing.append(code)
            check(missing.is_empty() and loaded_font.get("allow_system_fallback") == false, "mixed text uses bundled glyphs without system fallback")
            check(s.dialoguePanel.panel.width <= 1920 and s.dialoguePanel.panel.height <= 1080 and s.dialoguePanel.lineHeight > 0, "measured long-text geometry remains operable at 1080p")
        if case_name == "field-slow" and not slow_waited:
            if int(s.get("visibleCharacters", 0)) < 6:
                await process_frame; continue
            slow_waited = true; sample(label + ":incremental-reveal")
        if case_name == "transition" and s.wait == "DialogueWait":
            sample("unmigrated-dialogue")
            check(s.displayLocale == "en" and s.displayFallback == "unmigrated-consumer", "leaving retained field projection resets English consumer metadata")
            await press(KEY_ENTER); continue
        if case_name == "transition" and s.wait == "TextCloseWait" and s.textWindow == "OpenTextWindow":
            check(s.displayLocale == "zh-CN", "retained field close projection keeps its Chinese metadata")
        var wait = s.get("fieldText")
        if wait != null and int(wait.Token.Value) != last:
            sample(label + ":span"); last = int(wait.Token.Value)
        if s.get("canWaitForText", false):
            sample(label + ":ready")
            await press(KEY_ENTER)
        elif frame % 8 == 0 and wait != null: await press(KEY_ENTER)
        else: await process_frame
        if Time.get_ticks_msec() - started > 35000: break
    check(false, "field deadline " + label)
func settings_state() -> Dictionary:
    var panel := host.get_node("SettingsView")
    return JSON.parse_string(panel.call("ReadSettingsObservationJson"))
func _run() -> void:
    var english := load("res://localization/en.po") as Translation
    var chinese := load("res://localization/zh_CN.po") as Translation
    var font := load("res://fonts/localization/NotoSansCJKsc-Regular.otf") as FontFile
    var english_apply := english.get_message("ui.settings.apply")
    var broken_font := FontFile.new()
    if case_name == "required-ui": english.erase_message("ui.settings.apply")
    if case_name == "required-font": broken_font.take_over_path("res://fonts/localization/NotoSansCJKsc-Regular.otf")
    if case_name == "ui-fallback":
        chinese.erase_message("ui.settings.flash")
        chinese.add_message("ui.settings.speed", "坏参数 {99999999999999999999999999999999}")
    if case_name in ["service-fallback", "search-invalid"]:
        chinese.erase_message("church.paid.raise")
        chinese.erase_message("church.promotion")
        english.add_message("church.promotion", "Promotion is unavailable here. Choose another service or Leave. Your current gold and party status are unchanged. Return to the service list or Leave to resume exploration.")
        chinese.add_message("search.full", "错误参数 {7}")
    host = (load("res://Main.tscn") as PackedScene).instantiate(); root.add_child(host)
    await process_frame; await process_frame
    view = host.get_node_or_null("ExplorationSessionView")
    if view == null and not case_name.begins_with("heal"):
        samples.append({"label":"startup-labels", "labels":host.find_children("*", "Label", true, false).map(func(n): return n.text)})
        check(false, "ordinary authored host starts field"); finish(); return
    if case_name.begins_with("heal"): await heal(); return
    if case_name.begins_with("service"):
        await services(); return
    if case_name.begins_with("search"):
        await searches(); return
    if view.has_signal("SessionResultObserved"): view.connect("SessionResultObserved", on_result)
    if case_name in ["field-slow", "field-instant", "restart", "transition"]:
        if case_name == "restart":
            var persisted: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
            check(persisted.applied.Language == "zh-CN", "independent process reloads Chinese preference")
        await field_done("chinese")
        var playback: Dictionary = JSON.parse_string(host.call("ReadAudioObservationJson"))
        samples.append({"label":"audio", "audio":playback, "accepted":accepted})
        var speech: Array = playback.receipts.filter(func(row): return row.Command == 66 and row.Operation == "started")
        check(speech.size() > 0 if case_name == "field-slow" else speech.is_empty(), "normal reveal speaks; instant and reveal-all stay silent")
        check(accepted == ["text-w1-accepted", "text-w2-accepted"], "ReadMore and visual wrap add no source acknowledgement")
        if case_name == "transition": check(samples.any(func(row): return row.label == "unmigrated-dialogue"), "ordinary program reached unlocalized dialogue")
        if OS.get_environment("SF2_LOCALIZATION_LONG") == "1": check(more_seen, "long Chinese mixed output reaches measured ReadMore")
        finish(); return
    await field_done("english")
    var initial := sample("before-settings")
    await press(KEY_ESCAPE)
    check(host.get_node_or_null("SettingsView") != null, "safe field settings opens")
    if host.get_node_or_null("SettingsView") == null: finish(); return
    samples.append({"label":"settings-en", "settings":settings_state()})
    if case_name in ["required-ui", "required-font"]:
        check(settings_state().labels[6] == "Apply" and settings_state().labels[7] == "Cancel", "compiled recovery has distinct English controls")
        for i in range(6): await press(KEY_DOWN)
        await press(KEY_ENTER)
        check(host.get_node_or_null("SettingsView") != null and "localization-resources" in settings_state().message, "required resource failure refuses Apply before publish")
        samples.append({"label":"required-refused", "settings":settings_state(), "host":JSON.parse_string(host.call("ReadSettingsHostObservationJson"))})
        english.add_message("ui.settings.apply", english_apply)
        font.take_over_path("res://fonts/localization/NotoSansCJKsc-Regular.otf")
        await press(KEY_ESCAPE)
        finish(); return
    if case_name == "read-only":
        check(settings_state().readOnly and settings_state().selected == 8, "launch override read-only skips editable rows")
        await press(KEY_LEFT); await press(KEY_ESCAPE)
        check(JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied.Language == "en", "read-only launch preserves locale")
        finish(); return
    if case_name == "write-failure":
        var path := OS.get_environment("SF2_LOCALIZATION_PREFS")
        DirAccess.make_dir_recursive_absolute(path.get_base_dir())
        var file := FileAccess.open(path, FileAccess.WRITE)
        file.store_string("external owner sentinel"); file.close()
        for i in range(4): await press(KEY_DOWN)
        await press(KEY_ENTER); await press(KEY_DOWN); await press(KEY_DOWN); await press(KEY_ENTER)
        samples.append({"label":"write-refused", "settings":settings_state(), "host":JSON.parse_string(host.call("ReadSettingsHostObservationJson"))})
        check(settings_state().applied.Language == "en" and "changed outside" in settings_state().message and "relaunch" in settings_state().message, "external write retains live locale and specific recovery instructions")
        check(FileAccess.get_file_as_string(path) == "external owner sentinel", "external preference bytes preserved")
        await press(KEY_ESCAPE); finish(); return
    for i in range(4): await press(KEY_DOWN)
    await press(KEY_ENTER)
    check(settings_state().draft.Language == "zh-CN" and settings_state().applied.Language == "en", "locale only edits draft")
    await press(KEY_ESCAPE)
    check(JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied.Language == "en", "Cancel retains English")
    await press(KEY_ESCAPE)
    for i in range(4): await press(KEY_DOWN)
    await press(KEY_ENTER)
    await press(KEY_DOWN); await press(KEY_DOWN); await press(KEY_ENTER)
    var applied: Dictionary = JSON.parse_string(host.call("ReadSettingsHostObservationJson"))
    samples.append({"label":"settings-applied", "host":applied})
    check(host.get_node_or_null("SettingsView") == null and applied.applied.Language == "zh-CN", "Apply publishes selected locale")
    var unchanged := sample("after-settings")
    check(initial.sessionId == unchanged.sessionId and initial.revision == unchanged.revision and initial.randomSeedCopy == unchanged.randomSeedCopy, "settings does not submit gameplay or change identity")
    await press(KEY_ESCAPE)
    samples.append({"label":"settings-zh", "settings":settings_state()})
    if case_name == "ui-fallback":
        check(settings_state().labels[1].begins_with("Text speed:") and settings_state().labels[3].begins_with("Reduced flash:"), "invalid and missing Chinese UI rows fall back wholly to English")
        check(applied.localization.rows["ui.settings.speed"].Reason == "invalid-translation" and applied.localization.rows["ui.settings.flash"].Reason == "missing-translation", "exact UI fallback reason and locale retained")
        check(applied.localization.rows["ui.settings.speed"].EffectiveLocale == "en", "bad UI parameter does not become Chinese success")
    check("语言" in str(settings_state().labels), "mounted settings labels use Chinese")
    var body := host.get_node("SettingsView").find_child("SettingsMessage", true, false) as Label
    var mounted_font := body.get_theme_font("font")
    check(mounted_font.has_char(0x4e2d) and mounted_font.has_char(0x6587), "bundled actual font has required Chinese glyphs")
    samples.append({"label":"font", "family":mounted_font.get_font_name(), "size":body.get_theme_font_size("font_size"), "glyph":mounted_font.has_char(0x4e2d), "rect":str(body.get_global_rect())})
    await press(KEY_ESCAPE)
    await press(KEY_ENTER)
    await field_done("chinese")
    check(samples.any(func(row): return row.label == "chinese:span" and row.state.displayLocale == "zh-CN" and "请稍候" in row.state.dialogue), "real FieldTextWait translated projection")
    finish()
func finish() -> void:
    var f := FileAccess.open(OS.get_environment("SF2_LOCALIZATION_OUTPUT"), FileAccess.WRITE)
    f.store_string(JSON.stringify({"passed":failures.is_empty(), "failures":failures, "samples":samples}, "  ")); f.close()
    quit(0 if failures.is_empty() else 1)

func heal() -> void:
    view = host.get_node_or_null("BattleSessionView")
    check(view != null, "ordinary battle host starts HEAL sample")
    if view == null: finish(); return
    var initial := sample("heal-initial")
    await press(KEY_ENTER); await press(KEY_H); sample("heal-selected"); await press(KEY_ENTER); sample("heal-selected-target"); await press(KEY_ENTER); sample("heal-committed")
    var seen := {}
    var reading_checked := false
    for frame in range(3600):
        var s := state()
        if s.failure != null: check(false, "HEAL session failure"); break
        var scene: Dictionary = s.get("scene", {})
        var phase = scene.get("phase")
        if phase != null and not seen.has(phase):
            seen[phase] = true; sample("heal:" + str(phase))
        if seen.has("ResultMessage") and phase == null:
            sample("heal-complete")
            check(s.actors[0].mp == initial.actors[0].mp - 3 and s.actors[0].hp > initial.actors[0].hp, "HEAL spends once and publishes actual recovery")
            check(samples.any(func(row): return row.label == "heal:ResultMessage" and ("Recovery for" if case_name == "heal-en" else "恢复了") in row.state.scene.message), "real dynamic HEAL uses selected language name and amount template")
            finish(); return
        var healing = scene.get("healing")
        if healing != null and healing.AtTimedInput and not reading_checked:
            var before_reading := state()
            for pause in range(20): await process_frame
            var after_reading := state()
            check(before_reading.mainSeed == after_reading.mainSeed and before_reading.scene.healing.Remaining == after_reading.scene.healing.Remaining, "reading at timed input adds no RNG or simulation debt")
            reading_checked = true
        if phase in ["ActionMessage", "ResultMessage", "RewardMessage", "GrowthMessage"] and (healing == null or healing.AtTimedInput or healing.LogicalComplete): await press(KEY_ENTER)
        else: await process_frame
        if Time.get_ticks_msec() - started > 35000: break
    check(false, "HEAL native deadline"); finish()

func on_result(json: String) -> void:
    var result: Dictionary = JSON.parse_string(json)
    for event in result.get("observations", []):
        if event.Kind in ["text-w1-accepted", "text-w2-accepted"]: accepted.append(event.Kind)

func click_row(index: int, require_unchanged := true) -> void:
    var before := state()
    var row: Dictionary = before.church.rows[index].rect
    var point := Vector2(row.x + row.width / 2, row.y + row.height / 2)
    var button := InputEventMouseButton.new()
    button.button_index = MOUSE_BUTTON_LEFT; button.position = point; button.pressed = true
    Input.parse_input_event(button); await process_frame; await process_frame
    if require_unchanged: check(state().revision == before.revision, "pointer press alone never submits")
    button = InputEventMouseButton.new()
    button.button_index = MOUSE_BUTTON_LEFT; button.position = point; button.pressed = false
    Input.parse_input_event(button); await process_frame; await process_frame

func church_phase(phase: int, label: String) -> Dictionary:
    var s := sample(label)
    check(s.wait == "ChurchWait" and s.church.wait.Phase == phase, label + " typed phase")
    check(s.failure == null, label + " has no session failure")
    for key in ["title", "hint"]:
        var rect: Dictionary = s.church[key + "Rect"]
        check(rect.x >= 0 and rect.y >= 0 and rect.x + rect.width <= 1920 and rect.y + rect.height <= 1080, key + " stays within 1080p")
        check(s.church[key + "MinimumHeight"] <= rect.height, key + " fits allocated text height")
        var panel: Dictionary = s.church.panelRect
        check(rect.y + rect.height <= panel.y + panel.height - 20, key + " remains inside panel bottom")
    check(s.church.panelRect.y + s.church.panelRect.height <= 1080, "Church panel stays within 1080p")
    for row in s.church.rows:
        check(row.rect.y + row.rect.height <= s.church.hintRect.y, "hint does not overlap Church choices")
    return s

func services() -> void:
    var zh := case_name != "service-en"
    var initial := sample("service-initial")
    await press(KEY_ENTER)
    var menu := church_phase(0, "church-menu")
    check(("教堂" if zh else "Church") in menu.church.title, "selected locale Church menu")
    check(("鼠标" if zh else "Mouse") in menu.church.hint, "translated control hint")
    # The unavailable row remains selectable for its explanation, without a command.
    await click_row(2)
    var promote := church_phase(0, "promote-unavailable")
    check(promote.revision == menu.revision and ("暂不支持转职" if zh and case_name != "service-fallback" else "Promotion is unavailable") in promote.church.error, "Promote unavailable reason retains gameplay")
    if case_name == "service-low":
        await click_row(1)
        var offer := church_phase(1, "poison-offer")
        check(offer.church.wait.Offer.Effect == 1, "poison before stun")
        await press(KEY_ENTER)
        var insufficient := church_phase(2, "poison-insufficient")
        check(insufficient.gold == initial.gold and insufficient.church.wait.Result.Kind == 2, "insufficient gold does not pay")
        await press(KEY_ENTER)
        var stun := church_phase(1, "stun-offer-after-family-skip")
        check(stun.church.wait.Offer.Effect == 2, "insufficient poison skips same family")
        await press(KEY_ENTER); await press(KEY_ENTER)
        check(state().church.wait.Phase == 0 and state().gold == initial.gold, "insufficient cure returns to menu")
        await click_row(4)
        check(state().canWaitAtInput, "leave recovers ordinary field input")
        sample("service-low-control"); finish(); return
    await click_row(0)
    church_phase(1, "raise-offer")
    await click_row(0)
    var paid := church_phase(2, "raise-paid")
    check(paid.gold == initial.gold - 10 and paid.party[0].Hp == 100, "Raise pays once before result and applies cap")
    check(("复活" if zh and case_name != "service-fallback" else "Raise") in paid.church.title, "localized Raise result or whole English fallback")
    if case_name in ["service-fallback", "search-invalid"]: check("invalid" not in paid.church.title and "missing-translation" in paid.church.hint, "missing template is explicit English fallback")
    var stable := state()
    for frame in range(8): await process_frame
    check(state().revision == stable.revision and state().gold == stable.gold, "redraw does not repay")
    await press(KEY_ENTER); await click_row(0)
    var none := church_phase(2, "raise-no-applicable")
    check(none.church.wait.Result.Kind == 0, "no-applicable result retained")
    await press(KEY_ENTER); await click_row(1)
    church_phase(1, "poison-offer")
    await press(KEY_ENTER)
    var poison := church_phase(2, "poison-paid")
    check(poison.gold == initial.gold - 20 and poison.party[0].Status == 5, "poison payment and remaining stun/curse")
    await press(KEY_ENTER); await press(KEY_ESCAPE)
    var declined := church_phase(2, "stun-declined")
    check(declined.gold == poison.gold and declined.church.wait.Result.Kind == 1 and declined.church.wait.CurseUnavailable, "decline retains money and separate curse notice")
    await press(KEY_ENTER); await click_row(1); await press(KEY_ENTER)
    var cured := church_phase(2, "stun-paid")
    check(cured.gold == initial.gold - 40 and cured.party[0].Status == 4, "stun payment retains curse")
    check(("诅咒" if zh else "Curse") in cured.church.title, "curse has independent localized unavailable notice")
    await press(KEY_ENTER); await click_row(1)
    var curse := church_phase(2, "curse-only-no-applicable")
    check(curse.church.wait.Result.Kind == 0 and curse.church.wait.CurseUnavailable, "curse alone is not curable")
    await press(KEY_ENTER)
    await click_row(3)
    var closed := sample("save-closes-church")
    check(closed.wait != "ChurchWait" and closed.canWaitAtInput, "Save closes Church before existing persistence flow")
    check(initial.mainSeed == closed.mainSeed and initial.thinkingSeed == closed.thinkingSeed, "Church does not change RNG")
    finish()

func searches() -> void:
    if case_name == "search-ordinary":
        await field_done("ordinary-pilot-dialogue")
        for key in [KEY_UP, KEY_RIGHT, KEY_RIGHT, KEY_DOWN]:
            await press(key)
            for frame in range(12): await process_frame
        sample("ordinary-chest-approach")
    var initial := sample("search-initial")
    await press(KEY_ENTER)
    var found := sample("search-result")
    check(found.wait == "FieldSearchWait" and found.failure == null, "ordinary Search consumer")
    var kind: int = found.search.Kind
    var zh := case_name not in ["search-en", "search-switch"]
    if case_name in ["search-full", "search-invalid"]:
        check(kind == 4 and not found.flags.any(func(f): return int(f) == 150) and found.searchBlock == 0xD800, "full pack leaves flag/chest untouched")
        check(found.party == initial.party and found.gold == initial.gold, "full pack leaves all resources untouched")
        if case_name == "search-invalid": check("hands are full" in found.dialogue and found.displayFallback == "invalid-translation" and "英文回退" in found.hint, "invalid result row falls back wholly to English")
        else: check("背包已满" in found.dialogue, "Chinese full-pack template")
    elif case_name == "search-gold":
        check(kind == 3 and found.gold == initial.gold + 25 and found.flags.any(func(f): return int(f) == 150), "gold commits with receipt")
    elif case_name == "search-description":
        check(kind == 1 and found.search.Texts.map(func(n): return int(n)) == [425, 900] and "Traveler" in found.dialogue and "给" in found.dialogue, "description IDs and source name binding")
    else:
        check(kind == 2 and found.flags.any(func(f): return int(f) == 150) and found.searchBlock == 0xD801, "item flag/chest commit together")
        check(found.revision == initial.revision + 1 and found.party[0].SourceLoadout.Items[0] == (113 if case_name == "search-field-only" else 0), "single authoritative item revision")
        check(("获得了" if zh else "received") in found.dialogue and ("Practice Token" if case_name == "search-field-only" else "Practice Leaf") in found.dialogue, "localized semantic item template retains untranslated name")
        if case_name == "search-fallback": check(found.displayFallback == "missing-translation" and "英文回退" in found.hint, "missing source row remains visibly English fallback")
    var body := view.find_child("Dialogue", true, false) as Label
    var font := body.get_theme_font("font")
    check(font.get("allow_system_fallback") == false, "Search uses bundled font")
    for character in body.text.length():
        var code := body.text.unicode_at(character)
        if code > 32: check(font.has_char(code), "Search glyph " + str(code))
    var hint_node := view.find_child("DialogueHint", true, false) as Label
    var hint_rect := hint_node.get_global_rect()
    var panel: Dictionary = found.dialoguePanel.panel
    samples.append({"label":"search-hint-layout", "minimumHeight":hint_node.get_minimum_size().y, "height":hint_rect.size.y, "bottom":hint_rect.end.y, "panelBottom":panel.y + panel.height})
    check(hint_node.is_visible_in_tree() and hint_node.get_minimum_size().y <= hint_rect.size.y and hint_rect.end.y <= panel.y + panel.height - 20, "Search measured hint fits panel")
    check(found.dialoguePanel.body.y + found.dialoguePanel.body.height <= hint_rect.position.y, "Search body and hint do not overlap")
    var stable := state()
    for frame in range(8): await process_frame
    check(state().revision == stable.revision and state().party == stable.party, "Search redraw cannot grant")
    if case_name in ["search-zh", "search-full"]:
        var point := Vector2(found.dialoguePanel.panel.x + 100, found.dialoguePanel.panel.y + 60)
        var button := InputEventMouseButton.new()
        button.button_index = MOUSE_BUTTON_RIGHT if case_name == "search-full" else MOUSE_BUTTON_LEFT
        button.position = point; button.pressed = true
        Input.parse_input_event(button); await process_frame; await process_frame
        check(state().revision == stable.revision and state().wait == "FieldSearchWait", "Search pointer press alone retains result")
        var released := InputEventMouseButton.new()
        released.button_index = button.button_index; released.position = point; released.pressed = false
        Input.parse_input_event(released); await process_frame; await process_frame
        check(state().canWaitAtInput, "Search matching pointer release dismisses result")
        sample("search-pointer-release")
    # Reveal, then ordinary continuation and a second interaction.
    for attempt in range(6):
        if state().canWaitAtInput: break
        await press(KEY_ENTER)
    check(state().canWaitAtInput, "Search dismiss returns field control")
    if case_name == "search-field-only":
        await press(KEY_E)
        var inventory := state()
        check(inventory.partyStatus.visible and inventory.partyStatus.projection[0].Inventory[0].Name == "Practice Token", "field-only source name reaches actual read-only Party projection")
        sample("field-only-inventory")
        await press(KEY_ESCAPE)
    if case_name == "search-switch":
        var before_settings := state()
        await press(KEY_ESCAPE)
        for i in range(4): await press(KEY_DOWN)
        await press(KEY_ENTER); await press(KEY_DOWN); await press(KEY_DOWN); await press(KEY_ENTER)
        var after_settings := sample("search-locale-applied")
        check(before_settings.revision == after_settings.revision and before_settings.party == after_settings.party and before_settings.gold == after_settings.gold, "real locale Apply preserves acquired reward revision")
        check(JSON.parse_string(host.call("ReadSettingsHostObservationJson")).applied.Language == "zh-CN", "real Apply selects Chinese before repeat")
    await press(KEY_ENTER)
    var repeat := sample("search-repeat")
    check(repeat.search.Kind == (4 if case_name in ["search-full", "search-invalid"] else 1 if case_name == "search-description" else 0), "repeat keeps appropriate typed result")
    check(repeat.party == found.party and repeat.gold == found.gold and repeat.mainSeed == initial.mainSeed and repeat.thinkingSeed == initial.thinkingSeed, "repeat cannot grant or consume RNG")
    if case_name not in ["search-full", "search-invalid", "search-description"]: check(repeat.searchBlock == 0xD801, "repeat retains opened chest")
    finish()
