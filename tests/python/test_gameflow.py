from __future__ import annotations

import json
from copy import deepcopy

import pytest

from sf2tool.h2 import gameflow
from sf2tool.h2.gameflow import build_gameflow_inventory
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/gameflow-core-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-gameflow-core-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/gameflow-core-static-v1.json")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_gameflow_source_scope_join_keeps_fourteen_records_over_thirteen_files() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = build_gameflow_inventory(UPSTREAM)

    assert output["indexedRecordIds"] == fixture["expected"]["indexedRecordIds"]
    assert output["indexedSourcePaths"] == fixture["expected"]["indexedSourcePaths"]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 14
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 13
    assert "map.block-mutation.copy-helper" in output["indexedRecordIds"]
    assert output["indexedSourcePaths"] == output["scopes"]


def test_gameflow_index_schemas_pin_the_exact_ordered_scope_and_records() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture = load_json(FIXTURE_PATH)

    expected = fixture["expected"]
    assert output_schema["properties"]["scopes"]["const"] == expected["indexedSourcePaths"]
    assert output_schema["properties"]["indexedRecordIds"]["const"] == expected[
        "indexedRecordIds"
    ]
    assert output_schema["properties"]["indexedSourcePaths"]["const"] == expected[
        "indexedSourcePaths"
    ]
    assert fixture_schema["properties"]["expected"]["properties"]["indexedRecordIds"][
        "const"
    ] == expected["indexedRecordIds"]
    assert fixture_schema["properties"]["expected"]["properties"]["indexedSourcePaths"][
        "const"
    ] == expected["indexedSourcePaths"]
    assert output_schema["properties"]["summary"]["additionalProperties"] is False


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_gameflow_index_schemas_reject_missing_or_reordered_copy_helper() -> None:
    fixture = load_json(FIXTURE_PATH)
    missing_field_fixture = deepcopy(fixture)
    del missing_field_fixture["expected"]["indexedSourcePaths"]
    with pytest.raises(ValueError, match="indexedSourcePaths"):
        validate_json(missing_field_fixture, FIXTURE_SCHEMA, owner="missing gameflow index field")

    extra_field_fixture = deepcopy(fixture)
    extra_field_fixture["expected"]["unexpectedIndexedRecord"] = True
    with pytest.raises(ValueError, match="unexpectedIndexedRecord"):
        validate_json(extra_field_fixture, FIXTURE_SCHEMA, owner="extra gameflow index field")

    malformed_fixture = deepcopy(fixture)
    malformed_fixture["expected"]["indexedRecordIds"].remove("map.block-mutation.copy-helper")
    with pytest.raises(ValueError, match="indexedRecordIds"):
        validate_json(malformed_fixture, FIXTURE_SCHEMA, owner="missing gameflow index fixture")

    malformed_output = build_gameflow_inventory(UPSTREAM)
    malformed_output["indexedRecordIds"].reverse()
    with pytest.raises(ValueError, match="indexedRecordIds"):
        validate_json(malformed_output, OUTPUT_SCHEMA, owner="reordered gameflow output")


def test_gameflow_source_scope_join_rejects_missing_layout_file_coverage(
    monkeypatch: pytest.MonkeyPatch, tmp_path
) -> None:
    index = load_json(repo_path("manifests/research-index.json"))
    z80_record = next(
        record for record in index["records"] if record["id"] == "gameflow.start.z80-init"
    )
    z80_record["sourcePath"] = "code/other/z80init.asm"
    mutated_index = tmp_path / "research-index-missing-gameflow-source.json"
    mutated_index.write_bytes((json.dumps(index, indent=2) + "\n").encode("utf-8"))
    monkeypatch.setattr(gameflow, "RESEARCH_INDEX", mutated_index)

    with pytest.raises(ValueError, match="research-index source coverage drift"):
        gameflow._index_records_for_gameflow_scope()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_warp_facing_wrong_parameter_fails_during_inventory_construction(monkeypatch) -> None:
    read_source = gameflow.read_upstream_text

    def wrong_parameter(path):
        source = read_source(path)
        if path.name == "explorationfunctions_2.asm":
            original = "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3"
            assert source.count(original) == 1
            source = source.replace(original, original.replace("PARAM_5", "PARAM_4"))
        return source

    monkeypatch.setattr(gameflow, "read_upstream_text", wrong_parameter)
    with pytest.raises(ValueError, match="warp facing"):
        build_gameflow_inventory(UPSTREAM)


def test_warp_statements_are_function_scoped_comment_free_and_width_preserving() -> None:
    source = """Other:
        move.l d0,d3
; End of function Other
Probe: ; move.b wrong,d3
        ; rts
        move.b value , d3 ; move.w decoy,d3
        bne.s @Finish
@Finish: ; another comment
        rts
; End of function Probe
Later:
        move.w d1,d3
; End of function Later
"""
    assert gameflow._warp_function_statements(source, "Probe") == [
        "move.b value,d3",
        "bne @Finish",
        "@Finish:",
        "rts",
    ]
    assert gameflow._warp_function_statements(source.replace("bne.s", "bne.w"), "Probe") == (
        gameflow._warp_function_statements(source, "Probe")
    )


@pytest.mark.parametrize(
    "source",
    (
        "ProbeExtra:\n rts\n; End of function ProbeExtra",
        "; Probe:\n rts\n; End of function Probe",
        "Probe:\n rts\n; End of function ProbeExtra",
        "Probe:\n rts\n; End of function Probe\nProbe:\n rts\n; End of function Probe",
        "Probe:\n@Same:\n@Same:\n rts\n; End of function Probe",
        "Probe:\n rts\n; End of function Probe\n; End of function Probe",
    ),
    ids=(
        "near-entry",
        "comment-entry",
        "near-end",
        "duplicate-function",
        "duplicate-label",
        "duplicate-end",
    ),
)
def test_warp_statements_reject_ambiguous_or_near_miss_boundaries(source: str) -> None:
    with pytest.raises(ValueError, match="warp facing"):
        gameflow._warp_function_statements(source, "Probe")


@pytest.fixture
def warp_sources() -> dict[str, str]:
    if not UPSTREAM.is_dir():
        pytest.skip("pinned upstream checkout is unavailable")
    return {
        str(path).replace("\\", "/"): gameflow.read_upstream_text(UPSTREAM / "disasm" / path)
        for path in gameflow.SOURCE_PATHS
        if path.name in ("exploration.asm", "explorationfunctions_2.asm", "mainloop.asm")
    }


def _change_warp_function(sources, name, old, new):
    changed = dict(sources)
    matches = [key for key, source in changed.items() if f"\n{name}:" in source]
    assert len(matches) == 1
    key = matches[0]
    prefix, entry, rest = changed[key].partition(f"{name}:")
    body, end, tail = rest.partition(f"; End of function {name}")
    assert old in body
    changed[key] = prefix + entry + body.replace(old, new, 1) + end + tail
    return changed


def test_warp_source_spine_accepts_original_and_legal_branch_suffixes(warp_sources) -> None:
    assert gameflow._guard_warp_facing_handoff(warp_sources) is None
    changed = _change_warp_function(
        warp_sources, "ProcessMapEventType1_Warp", "bne.w   loc_259CC", "bne.s   loc_259CC"
    )
    changed = _change_warp_function(changed, "MainLoop", "bsr.w   SwitchMap", "bsr.s   SwitchMap")
    changed = _change_warp_function(
        changed,
        "ProcessMapEventType1_Warp",
        "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
        "move.b\t((MAP_EVENT_PARAM_5-$1000000)).w , d3 ; not PARAM_4",
    )
    assert gameflow._guard_warp_facing_handoff(changed) is None


@pytest.mark.parametrize(
    ("name", "old", "new"),
    (
        (
            "WarpIfSetAtPoint",
            "move.w  MAPDATA_EVENT_WARP_OFFSET_FACING",
            "move.b  MAPDATA_EVENT_WARP_OFFSET_FACING",
        ),
        (
            "WarpIfSetAtPoint",
            "MAPDATA_EVENT_WARP_OFFSET_FACING(a2)",
            "MAPDATA_EVENT_WARP_OFFSET_TYPE(a2)",
        ),
        ("WarpIfSetAtPoint", "blt.s   @SetWarpElements", "bne.s   @SetWarpElements"),
        ("WarpIfSetAtPoint", "@SetWarpElements:", "@SetWarpElementsWrong:"),
        (
            "ProcessMapEvent",
            "beq.w   ProcessMapEventType1_Warp",
            "bsr.w   ProcessMapEventType1_Warp",
        ),
        ("ProcessMapEventType1_Warp", "tst.b   ((MAP_EVENT_PARAM_1", "tst.w   ((MAP_EVENT_PARAM_1"),
        ("ProcessMapEventType1_Warp", "bne.w   loc_259CC", "beq.w   loc_259CC"),
        ("ProcessMapEventType1_Warp", "bne.w   loc_259CC", "bne.w   loc_259C2"),
        ("ProcessMapEventType1_Warp", "movem.l (sp)+,d0", "movem.w (sp)+,d0"),
        ("ProcessMapEventType1_Warp", "movem.l (sp)+,d0", "; movem.l (sp)+,d0"),
        (
            "ProcessMapEventType1_Warp",
            "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
            "move.w  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
        ),
        (
            "ProcessMapEventType1_Warp",
            "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
            "move.b  ((MAP_EVENT_PARAM_5_EXTRA-$1000000)).w,d3",
        ),
        (
            "ProcessMapEventType1_Warp",
            "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
            "; move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
        ),
        (
            "ProcessMapEventType1_Warp",
            "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
            "rts\n move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
        ),
        (
            "ProcessMapEventType1_Warp",
            "loc_259CC:",
            "loc_259CC:\n move.b ((MAP_EVENT_PARAM_5-$1000000)).w,d3",
        ),
        ("ProcessMapEventType1_Warp", "loc_259CC:", "loc_259CC:\n dc.w $4E75"),
        ("ProcessMapEventType1_Warp", "loc_259CC:", "loc_259CC:\n movem.l d0-d4,(a0)"),
        ("ProcessMapEventType1_Warp", "loc_259CC:", "loc_259CC:\n move.w #0,d3"),
        ("ProcessMapEventType1_Warp", "jsr     j_DeclareRaftEntity", "jmp     j_DeclareRaftEntity"),
        (
            "UpdatePlayerPosFromMapEvent",
            "move.b  ENTITYDEF_OFFSET_FACING(a0),d3",
            "move.b  ENTITYDEF_OFFSET_Y(a0),d3",
        ),
        ("UpdatePlayerPosFromMapEvent", "movea.l (sp)+,a0", "movea.w (sp)+,a0"),
        ("ExplorationLoop", "bsr.w   ProcessMapEvent", "jmp     ProcessMapEvent"),
        ("ExplorationLoop", "bsr.w   ProcessMapEvent", "move.l d3,-(sp)\n bsr.w ProcessMapEvent"),
        ("MainLoop", "jsr     j_ExplorationLoop", "jmp     j_ExplorationLoop"),
        ("MainLoop", "bra.s   @Start", "rts"),
    ),
    ids=(
        "producer-width",
        "producer-field",
        "producer-polarity",
        "producer-label",
        "dispatch-stack",
        "selector-width",
        "selector-polarity",
        "selector-target",
        "pop-width",
        "comment-pop",
        "facing-width",
        "near-field",
        "comment-field",
        "early-return",
        "wrong-branch-write",
        "inline-machine-code",
        "register-range",
        "d3-clobber",
        "callee-tail",
        "helper-field",
        "helper-stack-width",
        "caller-tail",
        "caller-push",
        "outer-caller-tail",
        "outer-return",
    ),
)
def test_warp_source_mutations_fail_before_fixture_comparison(warp_sources, name, old, new) -> None:
    changed = _change_warp_function(warp_sources, name, old, new)
    with pytest.raises(ValueError, match="warp facing"):
        gameflow._guard_warp_facing_handoff(changed)


def test_warp_facing_cannot_be_relocated_into_nonzero_branch(warp_sources) -> None:
    read = "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3"
    changed = _change_warp_function(warp_sources, "ProcessMapEventType1_Warp", read, "")
    changed = _change_warp_function(
        changed, "ProcessMapEventType1_Warp", "loc_259CC:", f"loc_259CC:\n {read}"
    )
    with pytest.raises(ValueError, match="warp facing"):
        gameflow._guard_warp_facing_handoff(changed)


@pytest.mark.parametrize("instruction", ("movem.l (sp)+,d0", "rts"), ids=("pop", "return"))
def test_warp_stack_and_return_order_cannot_be_preserved_only_as_fragments(
    warp_sources, instruction
) -> None:
    changed = _change_warp_function(
        warp_sources, "ProcessMapEventType1_Warp", instruction, ""
    )
    read = "move.b  ((MAP_EVENT_PARAM_5-$1000000)).w,d3"
    changed = _change_warp_function(
        changed, "ProcessMapEventType1_Warp", read, f"{instruction}\n {read}"
    )
    with pytest.raises(ValueError, match="warp facing"):
        gameflow._guard_warp_facing_handoff(changed)
