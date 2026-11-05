extends SceneTree

var audio_ids: Array[int] = []

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    root.add_child(load("res://probes/SceneMusicContextObservation.cs").new())

func observe_audio(stage: String, off_area: bool, owner: Node) -> void:
    for child in owner.get_children():
        if child is AudioStreamPlayer and child.stream != null and child.playing:
            var stream_id: int = child.stream.get_instance_id()
            var playback_id: int = child.get_stream_playback().get_instance_id()
            for id in [stream_id, playback_id]:
                if not audio_ids.has(id): audio_ids.append(id)
            print("SCENE-AUDIO-ID ", JSON.stringify({"stage":stage, "offArea":off_area,
                "player":child.name, "stream":stream_id, "playback":playback_id}))

func report_audio_lifetime(stage: String) -> void:
    # Native validity queries do not recreate managed RefCounted wrappers.
    var alive_ids: Array[int] = []
    for id in audio_ids:
        if is_instance_id_valid(id): alive_ids.append(id)
    print("SCENE-AUDIO-LIFETIME ", JSON.stringify({"stage":stage,
        "trackedIds":audio_ids, "aliveIds":alive_ids}))

func audio_released() -> bool:
    for id in audio_ids:
        if is_instance_id_valid(id): return false
    return true
