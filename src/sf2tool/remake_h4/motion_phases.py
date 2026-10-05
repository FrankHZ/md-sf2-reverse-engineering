"""Occurrence-bound logical phase interpretation and serialization tolerance."""


def phase_reader(effect, subject, token):
    def semantic_phases(state, effect=effect, subject=subject, token=token):
        if effect == "nod":
            age = (state.get("nod") or {}).get("Elapsed")
            return (
                ()
                if age is None
                else ("normal-before" if age < 10 else "lowered" if age < 30 else "normal-after",)
            )
        cue = state.get("presentationCue") or {}
        age = cue.get("elapsed")
        if age is None or cue.get("token") != token:
            return ()
        if effect == "shiver" and cue.get("gesture") == subject and cue.get("shivering") is True:
            return tuple(
                {1 if int(max(0, age + d) * 60 / 5) % 2 == 0 else -1 for d in (-1e-14, 1e-14)}
            )
        if effect in ("mosaic-in", "mosaic-out") and cue.get("mosaic") == subject:
            age = 0.5 - age if effect == "mosaic-out" else age
            return tuple(
                {
                    8 if a < 0.1 else 6 if a < 0.2 else 4 if a < 0.3 else 2 if a < 0.4 else 1
                    for a in (age - 1e-14, age + 1e-14)
                }
            )
        return ()

    return semantic_phases
