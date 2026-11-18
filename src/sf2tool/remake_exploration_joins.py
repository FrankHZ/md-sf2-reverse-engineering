"""Lower the selected csc08 join service without consuming the cutscene text cursor."""


def music_input_return() -> list[dict]:
    """FadeOut_WaitForP1Input waits for music and input; it does not fade the palette."""
    return [
        {"op": "present", "kind": "SoundWait", "resource": None, "entity": None, "position": None},
        {"op": "present", "kind": "PreviousMusic", "resource": None,
         "entity": None, "position": None},
        {"op": "wait-text-input"},
    ]


def join_selector(expression, number):
    selector = 0
    for operand in expression.split("|"):
        selector |= number(operand.strip())
    return selector


def join_service(selector: int) -> list[dict]:
    result = []
    members = [1, 2] if selector & 32767 == 128 else [selector & 32767]
    result.append(
        {
            "op": "present",
            "kind": "CameraWait",
            "entity": None,
            "position": None,
            "resource": None,
        }
    )
    result.append(
        {
            "op": "present",
            "kind": "Sound",
            "resource": "MUSIC_SAD_JOIN" if selector & 32768 else "MUSIC_JOIN",
            "entity": None,
            "position": None,
        }
    )
    result.extend({"op": "join-party", "member": member} for member in members)
    result.extend(
        [
            {
                "op": "show-text",
                "text": 447 if selector & 32767 == 128 else 446,
                "mode": "single",
                "speaker": None,
                "waitForAcknowledgement": False,
                "explicitWindows": True,
                **({"nameMember": selector & 32767} if selector & 32767 != 128 else {}),
            },
            *music_input_return(),
            {"op": "close-text"},
            {"op": "wait-ticks", "ticks": 10},
        ]
    )
    return result
