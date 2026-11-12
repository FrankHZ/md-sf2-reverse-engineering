extends "res://probes/host_ui_observation.gd"

# Ordinary mapped Settings input and projections. No focus requests or gameplay setters.
var preference_path: String
func applied() -> Dictionary: return observation(host, "ReadSettingsHostObservationJson").applied
func saved() -> String: return FileAccess.get_file_as_string(preference_path)

func select_setting(index: int) -> void:
    for i in range(13):
        if settings().selected == index: return
        await key(KEY_DOWN)
    check(false, "setting focus " + str(index))

func open_settings(field: bool = false) -> void:
    if field:
        await key(KEY_F10)
        for i in range(5):
            if selected_menu() == "Settings": break
            await key(KEY_DOWN)
        await key(KEY_ENTER)
    else: await key(KEY_ESCAPE)
    check(find("SettingsView") != null, "ordinary Settings caller")

func visibility(expected: bool) -> void:
    var s := settings()
    check(s.debugVisible == expected, "visibility follows applied preference")
    check(not s.coverage.is_empty() and s.debugText == s.coverage, "observation survives either visibility")
    check(not s.message.contains(s.coverage.split("\n")[0]), "ordinary message excludes loading statistics")
    check(not "Message unavailable" in str(s.labels), "localized settings operands")
    check(("诊断" in s.labels[11]) == (s.locale == "zh-CN"), "bilingual discoverable Debug row")
    check("installed artwork" in s.message or "安装了高清素材" in s.message, "graphics capability remains visible")
    geometry("SettingsWindow")

func capture(label: String) -> void:
    samples.append({"label":label,"applied":applied(),"settings":settings()})

func accept() -> void:
    await select_setting(7); await key(KEY_ENTER)
    check(find("SettingsView") == null, "Apply writes before closing")

func run() -> void:
    if output.is_empty() or not OS.get_environment("SF2_SAVE_NAMESPACE").begins_with("issue1709-"):
        push_error("Use explicit output and disposable issue1709-* preferences"); quit(2); return
    preference_path = OS.get_environment("SF2_SAVE_ROOT").path_join(OS.get_environment("SF2_SAVE_NAMESPACE")).path_join("sf2-preferences/input.json")
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate(); root.add_child(host); await frames(10)
    await open_settings()
    var initial := applied()
    var original := saved()
    if case_name == "override":
        check(settings().readOnly and initial.ShowDebugDiagnostics, "read-only override retains On")
        check((find("Setting11") as Button).disabled, "override disables new preference")
        visibility(true); capture("override"); await key(KEY_ESCAPE)
        check(saved() == original, "override never writes ordinary preferences")
        finish(); return
    if case_name == "off":
        check(not initial.ShowDebugDiagnostics, "fresh process restores saved Off")
        visibility(false); capture("restored-off"); finish(); return
    if case_name == "restore":
        check(initial.ShowDebugDiagnostics, "fresh process restores saved On")
        visibility(true); capture("restored-on")
        await select_setting(6); await key(KEY_ENTER)
        check(not settings().draft.ShowDebugDiagnostics and applied() == initial, "Defaults stages Off")
        visibility(true); await key(KEY_ESCAPE); await open_settings()
        check(applied() == initial and saved() == original, "Cancel defaults retains live settings and bytes")
        await select_setting(11); await key(KEY_LEFT); await accept()
        check(not applied().ShowDebugDiagnostics and JSON.parse_string(saved()).showDebugDiagnostics == false, "Apply persists Off")
        check(applied().GraphicsMode == initial.GraphicsMode and applied().ModernGuidance == initial.ModernGuidance, "Off preserves graphics and guidance")
        await open_settings(); visibility(false); capture("applied-off"); finish(); return
    check(not initial.ShowDebugDiagnostics and not JSON.parse_string(original).has("showDebugDiagnostics"), "old version-1 preferences default Off")
    visibility(false); capture("old-preferences")
    await select_setting(11); await key(KEY_RIGHT)
    check(settings().draft.ShowDebugDiagnostics and not applied().ShowDebugDiagnostics, "toggle is draft only")
    visibility(false); await key(KEY_ESCAPE)
    check(applied() == initial and saved() == original, "Cancel preserves live settings and bytes")
    var result: String = host.call("StartPreVoyageLocalizationObservation", OS.get_environment("SF2_HOST_UI_START"))
    check(result == "started", "ordinary controlled field admission: " + result)
    if result != "started":
        samples.append({"label":"mount-failure", "entry":observation(find("SessionEntryView"), "ReadEntryObservationJson")})
        finish(); return
    await frames(10)
    for i in range(600):
        if state().get("canWaitAtInput", false): break
        await process_frame
    var field := state()
    samples.append({"label":"field-start", "map":field.get("map"), "stage":field.get("stage"), "wait":field.get("wait"),
        "failure":field.get("failure"), "canWaitAtInput":field.get("canWaitAtInput"), "fieldText":field.get("fieldText")})
    await open_settings(true)
    if find("SettingsView") == null: finish(); return
    var baseline := state()
    var before: Variant = observation(host, "ReadPersistenceObservationJson").progress
    visibility(false)
    var catalog: Dictionary = observation(host, "ReadSettingsHostObservationJson").localization
    check(catalog.worldCatalog.messages > 0 and catalog.worldCatalog.names > 0 and catalog.entryCatalog != null, "real catalogs remain observed with Debug Off")
    var lock_path := preference_path.get_base_dir().path_join("input.lock")
    check(DirAccess.make_dir_absolute(lock_path) == OK, "owned storage fault")
    await select_setting(11); await key(KEY_RIGHT); await select_setting(7); await key(KEY_ENTER)
    check(find("SettingsView") != null and applied() == initial and saved() == original, "write refusal preserves Off and exact bytes")
    visibility(false)
    check("storage" in settings().message or "存储" in settings().message, "actionable failure remains visible with Debug Off")
    capture("write-refusal")
    check(DirAccess.remove_absolute(lock_path) == OK, "remove owned empty fault")
    await accept()
    check(applied().ShowDebugDiagnostics and JSON.parse_string(saved()).showDebugDiagnostics, "Apply persists On")
    check(applied().GraphicsMode == initial.GraphicsMode and applied().ModernGuidance == initial.ModernGuidance, "On preserves graphics and guidance")
    check(observation(host, "ReadPersistenceObservationJson").progress == before and state().revision == baseline.revision and state().mainSeed == baseline.mainSeed and state().thinkingSeed == baseline.thinkingSeed, "settings retain progress, revision and RNG")
    await key(KEY_ESCAPE)
    await open_settings(true); visibility(true); capture("applied-on")
    await select_setting(9); await key(KEY_ENTER)
    check(settings().help, "existing control help remains available")
    await key(KEY_ESCAPE)
    check(not settings().help and applied().ShowDebugDiagnostics, "help Back retains settings")
    finish()
