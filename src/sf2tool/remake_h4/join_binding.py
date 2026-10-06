"""Worktree-local JOIN selection and per-call partial-result lifetime."""

from sf2tool.paths import repo_path

from .join_consumer import join_consumer
from .join_witness import join_witness


def plain_join_binding(
    ref, actual, evidence_root, world_path, *, read, rows, require, _bounded_list
):
    """Bind only the accepted JOIN occurrence; indices locate evidence, not legality.

    The four selected files reuse the accepted segment seals. This is an offline
    consumer readback, not resumability validation or original music completion.
    Missing named evidence is unavailable; a present contradiction is false.
    """
    result = dict(original=None, plain=None, audio=None, caller=None, anchors={})
    plain_value = None

    def finalize():
        # Every exit preserves observed contradictions without closing partial evidence.
        values = [plain_value, result["audio"], result["caller"]]
        result["plain"] = False if False in values else None if None in values else True
        return result

    def set_plain(value):
        nonlocal plain_value
        plain_value = value

    if evidence_root is None or world_path is None:
        return finalize()
    evidence_root, world_path = (
        p.resolve() if p.is_absolute() else repo_path(p) for p in (evidence_root, world_path)
    )
    require(
        evidence_root.is_relative_to(repo_path("local")),
        "JOIN evidence must be selected beneath this worktree's local/",
    )
    paths = [
        evidence_root / name
        for name in (
            "candidate.json",
            "runtime/segment-pair.json",
            "runtime/checkpoints.jsonl",
            "runtime/actual-inputs.jsonl",
        )
    ]
    if not all(p.is_file() for p in paths):
        return finalize()

    if not join_witness(ref, paths, result, read, rows):
        return finalize()
    return join_consumer(actual, world_path, result, finalize, set_plain, read, _bounded_list)
