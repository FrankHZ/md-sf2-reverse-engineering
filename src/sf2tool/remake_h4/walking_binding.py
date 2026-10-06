"""Sealed R1 admission, selected content and per-call contribution aggregation."""

from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import ACCEPTED_UPSTREAM_REPOSITORY
from sf2tool.remake_h4_reference import ROM, UPSTREAM

from .walking_slots import walking_slots


def walking_admission_binding(
    ref, actual, evidence_root, world_path, original_binding, *, read, rows, _bounded_list
):
    """Translate the pinned R1 walking continuation, then observe its consumption.

    The 50-byte eas_Walking layout includes the branch's external displacement.
    ClearEntities/SetWalkingActscript own these buffers; addresses locate this
    witness only. UpdateEntityData/esc01 define movement and destination waiting.
    No original velocity magnitude or frame duration is a modern expectation.
    """
    parts = {slot: [original_binding] for slot in (5, 6, 8)}
    motion_parts = [original_binding]
    anchors = dict(
        source=ref["inherited"]["source"],
        upstream=UPSTREAM,
        template="eas_Walking:50bytes; wait30@0/randomWalk@32/waitDest@40/wait20@42/branch@46",
        hiddenMotionGate="Inferred",
        slots={},
    )

    def combined(values):
        return False if False in values else None if None in values else True

    def finish():
        return dict(
            slots={slot: combined(values) for slot, values in parts.items()},
            motion=combined(motion_parts),
            anchors=anchors,
        )

    if original_binding is not True:
        return finish()
    if world_path is None or evidence_root is None:
        for values in parts.values():
            values.append(None)
        motion_parts.append(None)
        return finish()
    evidence_root, world_path = (
        p.resolve() if p.is_absolute() else repo_path(p) for p in (evidence_root, world_path)
    )
    # Plain JOIN has already verified the accepted pair/material/raw-file seals.
    checkpoints = [row for _, row in rows(evidence_root / "runtime/checkpoints.jsonl")]
    raw_record = checkpoints[1]
    source_ok = raw_record["kind"] == "r1:inherited-status-and-live-entities" and (
        raw_record["order"] == ref["inherited"]["source"]["order"]
        and ref["inherited"]["source"]["record"] == "prepared-68/runtime/checkpoints.jsonl:2"
    )
    for values in parts.values():
        values.extend((source_ok, None))
    motion_parts.extend((source_ok, None))
    if not world_path.is_file():
        return finish()
    try:
        selected = read(world_path)
        identity = selected["provenance"]
        source_ok = combined(
            [source_ok]
            + [
                None if identity.get(key) is None else identity[key] == expected
                for key, expected in (
                    ("commit", UPSTREAM),
                    ("romSha256", ROM),
                    ("repository", ACCEPTED_UPSTREAM_REPOSITORY),
                )
            ]
        )
        for values in parts.values():
            values.append(source_ok)
        motion_parts.append(source_ok)
        map3 = next(
            (m for m in selected.get("world", {}).get("maps", []) if m["id"] == "map-3"), {}
        )
        states = _bounded_list(s["state"] for s in actual["samples"])
        initial = states[0]
        admitted = actual.get("admissionSnapshot", {}).get("state", {})
        expected_actions = [
            dict(op="wait", ticks=30),
            dict(op="speed", x=0, y=0),
            dict(op="acceleration", x=1, y=1),
        ] + [dict(op="flags", field="a", mask=mask, value=mask) for mask in (3, 12, 128, 64, 32)]
        next_wait = 20  # Source wait20 follows waitDest, not a measured host duration.
        candidates = _bounded_list(
            (
                (i, s)
                for i, s in enumerate(states)
                if initial.get("simulationTick") is not None
                and s.get("simulationTick") is not None
                and 0 < s["simulationTick"] - initial["simulationTick"] < next_wait
            )
        )
        later = candidates[0] if candidates else None
        if later:
            sample_index, consumed = later
            reset_parts = [True if "warpRecords" in actual else None]
            for record in actual.get("warpRecords", []):
                try:
                    if record["result"]["revision"] > consumed["revision"]:
                        break
                    reset_parts.extend(
                        None if o.get("Kind") is None else o["Kind"] != "program-instruction"
                        for o in record["result"]["observations"]
                    )
                except KeyError:
                    reset_parts.append(None)
            # Missing reset evidence cannot hide an observed reinstall contradiction.
            no_reset = combined(reset_parts)
            delta = consumed["simulationTick"] - initial["simulationTick"]
            anchors["consumption"] = dict(
                sample=sample_index, logicalServices=delta, noProgramInstallation=no_reset
            )

        walking_slots(
            raw_record,
            ref,
            source_ok,
            map3,
            expected_actions,
            next_wait,
            initial,
            admitted,
            later,
            consumed if later else None,
            no_reset if later else None,
            delta if later else None,
            parts,
            motion_parts,
            anchors,
            combined,
        )
        motion_parts[2] = source_ok
    except (KeyError, IndexError, StopIteration):
        for values in parts.values():
            values.append(None)
        motion_parts.append(None)
    return finish()
