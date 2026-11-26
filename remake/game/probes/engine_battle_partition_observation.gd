extends "res://probes/engine_equipment_recovery_observation.gd"

# Historical/current partition: ordinary Continue, Save and recovery action.
# The launcher supplies disposable numbered slots; all state reads are existing APIs.
func progress() -> Variant:
    return JSON.parse_string(host.call("ReadPersistenceObservationJson")).progress

func panel() -> Dictionary:
    var node := host.find_child("SaveSlotView", true, false)
    return {} if node == null else JSON.parse_string(node.call("ReadSaveObservationJson"))

func wait_panel(expected: String) -> bool:
    for _frame in range(1000):
        if panel().get("state") == expected: return true
        OS.delay_msec(2)
        await process_frame
    samples.append({"panelFailure":panel()})
    check(false, "ordinary save panel " + expected)
    return false

func save_current(label: String) -> bool:
    var before: Variant = progress()
    check(before != null, "settled battle capture")
    samples.append({label:before})
    await key(KEY_F10)
    # Settings is the current system-menu default; Save is two rows above it.
    await key(KEY_UP)
    await key(KEY_UP)
    await key(KEY_ENTER)
    if not await wait_panel("save-slots"): return false
    # Opening Save records the actual audio history through ObserveSaveMusic.
    var synchronized: Variant = progress()
    var without_music: Dictionary = before.duplicate(true)
    without_music.Story.Music = synchronized.Story.Music
    check(without_music == synchronized, "Save preparation changes only observed audio history")
    samples.append({label + "Saved":synchronized})
    await key(KEY_ENTER)
    if not await wait_panel("confirm-save"): return false
    await key(KEY_DOWN)
    await key(KEY_ENTER)
    if not await wait_panel("saved"): return false
    await key(KEY_ENTER)
    check(progress() == synchronized, "Save preserves complete battle and retained party")
    return failures.is_empty()

func boot() -> bool:
    if not await super.boot(): return false
    var expected := OS.get_environment("SF2_PARTITION_EXPECTED")
    if not expected.is_empty():
        check(progress() == JSON.parse_string(FileAccess.get_file_as_string(expected)),
            "cold Continue preserves complete captured progress and both RNG words")
    return await save_current("restoredProgress")

func verify_return(after: Dictionary) -> void:
    await super.verify_return(after)
    if not failures.is_empty(): return
    await key(KEY_ESCAPE)
    await save_current("afterActionProgress")

func run() -> void:
    if OS.get_environment("SF2_PARTITION_SAVE_ONLY") != "1":
        await super.run()
        return
    root.size = Vector2i(1920,1080)
    host = load("res://Main.tscn").instantiate()
    root.add_child(host)
    await boot()
    finish()
