extends SceneTree

func _initialize():
    call_deferred("run")

func run():
    root.add_child(load("res://Main.tscn").instantiate())
    for edge in [[KEY_LEFT, true], [KEY_LEFT, false], [KEY_ENTER, true]]:
        var event = InputEventKey.new()
        event.keycode = edge[0]
        event.pressed = edge[1]
        Input.parse_input_event(event)
        # Allow normal input delivery before the next edge; C# owns the actual Quit.
        await process_frame
