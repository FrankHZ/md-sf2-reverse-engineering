extends SceneTree

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    root.add_child(load("res://probes/AudioLifetimeObservation.cs").new())

func stream_id(player: AudioStreamPlayer) -> int:
    return player.stream.get_instance_id()

func playback_id(player: AudioStreamPlayer) -> int:
    return player.get_stream_playback().get_instance_id()

func alive(id: int) -> bool:
    # C# GodotObject.IsInstanceIdValid calls InstanceFromId, recreating a managed
    # RefCounted wrapper while backend deletion is pending. Do not retain our subjects.
    return is_instance_id_valid(id)
