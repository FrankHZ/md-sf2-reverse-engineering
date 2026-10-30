extends SceneTree

var refs: Array = []
var finished_count := 0

func _initialize() -> void:
    call_deferred("run")

func voice(loop: bool) -> AudioStreamPlayer:
    var player := AudioStreamPlayer.new()
    var wav := AudioStreamWAV.new()
    wav.format = AudioStreamWAV.FORMAT_16_BITS
    wav.mix_rate = 8000
    var pcm := PackedByteArray()
    pcm.resize(1600) # Project-authored 0.1 second silence; no original content.
    wav.data = pcm
    if loop:
        wav.loop_mode = AudioStreamWAV.LOOP_FORWARD
        wav.loop_end = 800
    player.stream = wav
    root.add_child(player)
    player.finished.connect(func(): finished_count += 1)
    player.play()
    refs.append([weakref(wav), weakref(player.get_stream_playback())])
    return player

func retained() -> int:
    var count := 0
    for pair in refs:
        for ref in pair:
            if ref.get_ref() != null: count += 1
    return count

func release(player: AudioStreamPlayer) -> void:
    player.stop()
    player.stream = null
    player.queue_free()

func drain() -> void:
    var deadline := Time.get_ticks_msec() + 2000
    while retained() > 0 and Time.get_ticks_msec() < deadline:
        await process_frame

func run() -> void:
    var mode := OS.get_environment("SF2_AUDIO_CASE")
    print("LIFETIME ", JSON.stringify({"mode":mode,"driver":AudioServer.get_driver_name()}))
    if mode == "active" or mode == "stop-quit":
        var player := voice(true)
        await process_frame
        if mode == "stop-quit": release(player)
        print("LIFETIME terminal retained=", retained())
        quit()
        return
    for i in range(4):
        var player := voice(false)
        var deadline := Time.get_ticks_msec() + 2000
        while finished_count < i + 1 and Time.get_ticks_msec() < deadline:
            await process_frame
        release(player)
        await drain()
        print("LIFETIME natural ", i, " retained=", retained(), " finished=", finished_count)
        player = voice(true)
        await process_frame
        release(player)
        print("LIFETIME interrupted-before-drain ", i, " retained=", retained())
        await drain()
        print("LIFETIME interrupted-after-drain ", i, " retained=", retained())
    quit(0 if retained() == 0 and finished_count == 4 else 1)
