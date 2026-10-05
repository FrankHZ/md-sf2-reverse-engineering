"""Follow the observed Wait edge owner through cancellation and repeated service."""


class WaitInput:
    """Press/release ownership belonging to exactly one admitted case."""

    def __init__(self):
        self.position = 0
        self.down = False
        self.armed = False
        self.owner = None
        self.services = 0
        self.evidence = False

    def consume(self, records, index, check):
        for input_index in range(self.position, index):
            observed_input = records[input_index]
            if observed_input.get("kind") != "input":
                continue
            self.evidence = True
            if "key" not in observed_input:
                check("input key available", None, input_index)
                self.armed = None
                continue
            pressed = observed_input.get("pressed")
            if not isinstance(pressed, bool):
                check(
                    "input edge available",
                    None if "pressed" not in observed_input else False,
                    input_index,
                )
                self.armed = None
            elif observed_input.get("key") == 86:
                if pressed and self.down is False:
                    self.owner, self.armed, self.services = observed_input, True, 0
                self.down = pressed
                if not pressed:
                    self.armed = False
            elif pressed:
                # A non-Wait action cancels the repeat; another V down while it is
                # already held cannot rearm it without the real release edge.
                self.armed = False
        self.position = index + 1

    def check_services(self, records, index, before, events, check):
        for event_kind, key, ready in (
            ("gameplay-wait", 86, "canWaitAtInput"),
            ("text-w1-accepted", 4194309, "canWaitForText"),
        ):
            if any(e["Kind"] == event_kind for e in events):
                if event_kind == "gameplay-wait":
                    check(
                        "ordinary input owns gameplay-wait",
                        self.owner["before"][ready] is True if self.owner else None,
                        index,
                    )
                    check(
                        "live Wait press/release ownership",
                        self.armed if self.evidence else None,
                        index,
                    )
                    if self.services:
                        check(
                            "held repeat ready state",
                            before["waitingAtInput"] is True
                            and before["canWaitAtInput"] is True
                            and before["focused"] is True,
                            index,
                        )
                    self.services += 1
                    continue
                inputs = [
                    r for r in records[:index] if r["kind"] == "input" and r.get("pressed") is True
                ]
                check(
                    "ordinary input owns " + event_kind,
                    inputs[-1]["key"] == key and inputs[-1]["before"][ready] is True
                    if inputs
                    else None,
                    index,
                )
