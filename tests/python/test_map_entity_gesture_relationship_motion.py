from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_entity_gesture_relationship_motion as gesture
from sf2tool.h3.map_entity_gesture_relationship_motion import (
    build_map_entity_gesture_relationship_motion_contract,
    build_map_entity_gesture_relationship_motion_static_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def test_gesture_motion_static_contract_inventories_all_seven_sections() -> None:
    actual = build_map_entity_gesture_relationship_motion_static_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    assert [(row["macro"], row["handler"]) for row in actual["sourceFacts"]["handlers"]] == [
        ("shiver", "csc2A_entityShiver"),
        ("nod", "csc26_entityNodHead"),
        ("followEntity", "csc2C_followEntity"),
        ("faceEntity", "csc52_faceEntity"),
        ("moveNextToPlayer", "csc28_moveEntityNextToPlayer"),
        ("fly", "csc2F_fly"),
        ("moveEntityAboveAnother", "csc31_moveEntityAboveEntity"),
    ]
    assert actual["followerPositionSignedByteTable"] == [
        24,
        0,
        0,
        -24,
        -24,
        0,
        0,
        24,
        24,
        -24,
        -24,
        -24,
        -24,
        24,
        24,
        24,
    ]
    assert actual["sourceFacts"]["handlers"][2]["scriptCursorReadUseSites"] == [
        {
            "sourceRegister": "a6",
            "destinationOperand": "d0",
            "transferredByteCount": 1,
            "cursorAdvanceByteCount": 0,
            "instruction": "move.b (a6),d0",
        },
        {
            "sourceRegister": "a6",
            "destinationOperand": "d0",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 2,
            "instruction": "move.w (a6)+,d0",
        },
        {
            "sourceRegister": "a6",
            "destinationOperand": "d0",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 2,
            "instruction": "move.w (a6)+,d0",
        },
        {
            "sourceRegister": "a6",
            "destinationOperand": "d2",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 2,
            "instruction": "move.w (a6)+,d2",
        },
    ]


def test_gesture_motion_runtime_contract_matches_complete_static_fixture() -> None:
    fixture = load_json(gesture.FIXTURE)
    actual = build_map_entity_gesture_relationship_motion_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    for field in (
        "romSha256",
        "provenance",
        "function",
        "ram",
        "constants",
        "followerPositionSignedByteTable",
        "sourceFacts",
        "sourceUseSites",
        "runtimeQuestions",
    ):
        assert actual[field] == fixture[field]
    assert actual["constants"]["shiverLoopIterationCount"] == 3
    assert actual["constants"]["nodLoopIterationCount"] == 1
    assert actual["constants"]["followerTableSelectorByteStride"] == 2
    assert actual["constants"]["moveVelocityMagnitude"] == 48
    assert actual["constants"]["aboveFollowerHorizontalOffsetWord"] == 65512
    assert actual["runtimeQuestions"] == [
        {
            "group": "Map Test 0 controlled handler seams",
            "label": "Unknown",
            "questions": [
                "Which normal map-script paths reach each of the seven handlers?",
                "Do rendered animation, collision, path, or persistence effects follow "
                "these bounded RAM and callback seams?",
                "How do unseeded entity, combatant, and follower-table inputs affect "
                "these seams outside the fixed matrix?",
            ],
        }
    ]


def test_gesture_motion_fixture_derives_complete_case_matrix() -> None:
    fixture = load_json(gesture.FIXTURE)
    fixture_before = deepcopy(fixture)
    static = build_map_entity_gesture_relationship_motion_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    actual = gesture.derive_case_expectations(static, fixture)
    assert actual == [case["expected"] for case in fixture["cases"]]
    assert fixture == fixture_before
    assert [(case["id"], case["macro"]) for case in fixture["cases"]] == [
        ("shiver-state-restore", "shiver"),
        ("nod-loop-state-boundary", "nod"),
        ("follow-selector-zero-live", "followEntity"),
        ("follow-selector-seven-live", "followEntity"),
        ("follow-zero-current-hp", "followEntity"),
        ("face-horizontal-right", "faceEntity"),
        ("face-horizontal-left", "faceEntity"),
        ("face-vertical-down", "faceEntity"),
        ("face-vertical-up", "faceEntity"),
        ("face-distance-tie", "faceEntity"),
        ("move-next-right", "moveNextToPlayer"),
        ("move-next-up", "moveNextToPlayer"),
        ("move-next-left", "moveNextToPlayer"),
        ("move-next-down", "moveNextToPlayer"),
        ("fly-zero-operand", "fly"),
        ("fly-nonzero-operand", "fly"),
        ("above-add-follower-register-order", "moveEntityAboveAnother"),
    ]
    follow_probes = [
        case["expected"]["currentHpSeedProbe"]
        for case in fixture["cases"]
        if case["macro"] == "followEntity"
    ]
    assert follow_probes == [
        {
            "sourceHelperInvoked": True,
            "firstScriptWordByteOffset": 0,
            "firstScriptWordByteLane": "high",
            "characterByte": 0,
            "storageAddress": 16771086,
            "storageTransferByteCount": 2,
        }
    ] * 3
    assert all(
        case["expected"]["currentHpSeedProbe"] is None
        for case in fixture["cases"]
        if case["macro"] != "followEntity"
    )
    assert static["sourceUseSites"]["aliveStatusFirstScriptWordByteProbe"] == {
        "sourceUseSite": {"instruction": "move.b (a6),d0", "sourceLine": 1521},
        "scriptCursorByteOffset": 0,
        "transferByteCount": 1,
        "scriptWordByteLane": "high",
        "destinationRegister": "d0",
        "advancesScriptCursor": False,
    }
    assert [
        (case["id"], case["runtimeGolden"]["callbackTargetOrderObserved"])
        for case in fixture["cases"]
        if case["macro"] == "shiver"
    ] == [
        (
            "shiver-state-restore",
            [
                "GetEntityAddressFromCharacter",
                "UpdateEntitySprite_0",
                "Sleep",
                "UpdateEntitySprite_0",
                "Sleep",
                "UpdateEntitySprite_0",
                "Sleep",
                "UpdateEntitySprite_0",
                "Sleep",
                "UpdateEntitySprite_0",
                "Sleep",
                "UpdateEntitySprite_0",
                "Sleep",
            ],
        )
    ]


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["cases"][0]["entityStateSeeds"][0].pop("flagsBByte"),
        lambda value: value["cases"][0]["entityStateSeeds"][0].update(
            {"flagsB": value["cases"][0]["entityStateSeeds"][0].pop("flagsBByte")}
        ),
        lambda value: value["sourceUseSites"]["followerSelector"][
            "orderedTableByteLoads"
        ][0].update({"unexpected": 1}),
        lambda value: value["cases"].reverse(),
        lambda value: value["cases"][0]["entityStateSeeds"][0].__setitem__("flagsBByte", 256),
        lambda value: value["instrumentation"].__setitem__("scriptInputRamOffset", 5),
    ],
)
def test_gesture_motion_fixture_schema_rejects_recursive_mutations(mutate: object) -> None:
    fixture = deepcopy(load_json(gesture.FIXTURE))
    assert callable(mutate)
    mutate(fixture)
    with pytest.raises(ValueError, match="entity gesture fixture"):
        validate_json(fixture, gesture.FIXTURE_SCHEMA, owner="entity gesture fixture")


def _observation_fixture() -> dict[str, object]:
    fixture = load_json(gesture.FIXTURE)
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["records"][0]["entityStateAfter"][0].pop("flagsBByte"),
        lambda value: value["records"][0]["directCallbackPlan"][0].update(
            {
                "instruction_target": value["records"][0]["directCallbackPlan"][0].pop(
                    "instructionTarget"
                )
            }
        ),
        lambda value: value["records"][2]["addFollowerRegisterWordsObserved"].update(
            {"unexpected": 1}
        ),
        lambda value: value["records"].reverse(),
        lambda value: value["records"][10]["entityStateAfter"][0].__setitem__(
            "xVelocity", 65536
        ),
    ],
)
def test_gesture_motion_observation_schema_rejects_recursive_mutations(mutate: object) -> None:
    observation = _observation_fixture()
    assert callable(mutate)
    mutate(observation)
    with pytest.raises(ValueError, match="entity gesture observation"):
        validate_json(observation, gesture.OBSERVATION_SCHEMA, owner="entity gesture observation")


@pytest.mark.parametrize("schema_path", [gesture.FIXTURE_SCHEMA, gesture.OBSERVATION_SCHEMA])
def test_gesture_motion_schemas_close_every_declared_object(schema_path: Path) -> None:
    schema = load_json(schema_path)

    def visit(value: object) -> None:
        if isinstance(value, list):
            for item in value:
                visit(item)
        elif isinstance(value, dict):
            if value.get("type") == "object":
                assert value.get("additionalProperties") is False
                assert isinstance(value.get("required"), list)
            for key, item in value.items():
                if key != "const":
                    visit(item)

    visit(schema)


def test_gesture_motion_width_and_word_wrap_guards() -> None:
    fixture = load_json(gesture.FIXTURE)
    static = build_map_entity_gesture_relationship_motion_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    invalid = deepcopy(fixture)
    invalid["cases"][0]["entityStateSeeds"][0]["flagsBByte"] = 256
    with pytest.raises(ValueError, match="state seed width"):
        gesture._derived_case_records(static, invalid)
    invalid_word = deepcopy(fixture)
    invalid_word["cases"][0]["entityStateSeeds"][0]["xWord"] = 65536
    with pytest.raises(ValueError, match="state seed width"):
        gesture._derived_case_records(static, invalid_word)
    assert gesture._signed_velocity(0x8000, 0, 48) == (0x8000, 65488)
    wrapped = deepcopy(fixture["cases"][5])
    wrapped["entityStateSeeds"][0]["xDest"] = 0x8000
    wrapped["entityStateSeeds"][0]["yDest"] = 0
    wrapped["entityStateSeeds"][1]["xDest"] = 0
    wrapped["entityStateSeeds"][1]["yDest"] = 1
    assert gesture._face_value(wrapped, static) == static["constants"]["directionValues"]["left"]


def test_gesture_motion_cursor_offset_is_derived_from_ordered_source_use_sites() -> None:
    fixture = load_json(gesture.FIXTURE)
    static = build_map_entity_gesture_relationship_motion_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    assert [
        record["scriptCursorRamOffsetAfter"]
        for record in gesture._derived_case_records(static, fixture)
    ] == [case["expected"]["scriptCursorRamOffsetAfter"] for case in fixture["cases"]]

    advanced = deepcopy(static)
    advanced["sourceFacts"]["handlers"][0]["scriptCursorReadUseSites"][0][
        "cursorAdvanceByteCount"
    ] = 1
    with pytest.raises(ValueError, match="cursor use-site advance drift"):
        gesture._derived_case_records(advanced, fixture)

    reordered = deepcopy(static)
    use_sites = reordered["sourceFacts"]["handlers"][2]["scriptCursorReadUseSites"]
    use_sites[2], use_sites[3] = use_sites[3], use_sites[2]
    with pytest.raises(ValueError, match="cursor use-site source order drift"):
        gesture._derived_case_records(reordered, fixture)


@pytest.mark.parametrize(
    ("relative_path", "symbol", "before", "after", "builder"),
    [
        (
            gesture.SOURCE_PATH,
            "csc2F_fly",
            "bne.s   loc_46EB8",
            "beq.s   loc_46EB8",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2A_entityShiver",
            "jsr     (Sleep).w",
            "jsr     (WaitForVInt).w",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2A_entityShiver",
            "moveq   #2,d7",
            "moveq   #3,d7",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2A_entityShiver",
            "move.w  (a6)+,d0",
            "move.b  (a6)+,d0",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2C_followEntity",
            "add.w   d2,d2",
            "add.w   d3,d2",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2F_fly",
            "move.b  #16,ENTITYDEF_OFFSET_LAYER(a5)",
            "move.b  #15,ENTITYDEF_OFFSET_LAYER(a5)",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc26_entityNodHead",
            "move.b  #0,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)",
            "move.b  #1,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc52_faceEntity",
            "move.b  #RIGHT,ENTITYDEF_OFFSET_FACING(a5)",
            "move.b  #LEFT,ENTITYDEF_OFFSET_FACING(a5)",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc28_moveEntityNextToPlayer",
            "addi.w  #MAP_TILE_SIZE,d1",
            "subi.w  #MAP_TILE_SIZE,d1",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc31_moveEntityAboveEntity",
            "jsr     AddFollower",
            "jsr     WaitForVInt",
            "static",
        ),
        (
            gesture.SOURCE_PATH,
            "csc31_moveEntityAboveEntity",
            "moveq   #$FFFFFFE8,d2",
            "moveq   #$FFFFFFE8,d3",
            "static",
        ),
        (
            gesture.COMBATANT_WORD_SOURCE_PATH,
            None,
            "move.w  (a0,d7.w),d1",
            "move.b  (a0,d7.w),d1",
            "runtime",
        ),
        (
            gesture.SOURCE_PATH,
            "csc2C_followEntity",
            "move.b  (a6),d0",
            "move.b  (a6)+,d0",
            "runtime",
        ),
    ],
)
def test_gesture_motion_source_mutations_fail_before_h3_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    relative_path: Path,
    symbol: str | None,
    before: str,
    after: str,
    builder: str,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    source_path = (upstream / relative_path).resolve()
    original = Path.read_text

    def altered(path: Path, *args: object, **kwargs: object) -> str:
        source = original(path, *args, **kwargs)
        if path.resolve() == source_path:
            if symbol is None:
                changed = source.replace(before, after, 1)
            else:
                start = source.index(f"{symbol}:")
                end = source.index(f"; End of function {symbol}", start)
                section = source[start:end]
                changed_section = section.replace(before, after, 1)
                changed = source[:start] + changed_section + source[end:]
            if changed == source:
                raise AssertionError(f"source mutation target drift: {before}")
            return changed
        return source

    monkeypatch.setattr(Path, "read_text", altered)
    with pytest.raises(ValueError, match="drift"):
        if builder == "static":
            build_map_entity_gesture_relationship_motion_static_contract(
                repo_path("local/roms/sf2-us.bin"), upstream
            )
        else:
            build_map_entity_gesture_relationship_motion_contract(
                repo_path("local/roms/sf2-us.bin"), upstream
            )


def test_direct_call_parser_ignores_comment_and_operand_near_misses() -> None:
    assert gesture._direct_calls(
        [
            {"instruction": "bsr.w GetEntityAddressFromCharacter", "sourceLine": 1},
            {"instruction": "jsr (WaitForVInt).w", "sourceLine": 2},
            {"instruction": "bsr.l AddFollower", "sourceLine": 3},
            {"instruction": "bsr.s LoadMapsprite", "sourceLine": 4},
            {"instruction": "move.w GetEntityAddressFromCharacter,d0", "sourceLine": 5},
            {"instruction": "nearbsr.w AddFollower", "sourceLine": 6},
        ]
    ) == [
        {
            "instruction": "bsr.w GetEntityAddressFromCharacter",
            "opcode": "bsr",
            "instructionTarget": "GetEntityAddressFromCharacter",
            "sourceLine": 1,
        },
        {
            "instruction": "jsr (WaitForVInt).w",
            "opcode": "jsr",
            "instructionTarget": "WaitForVInt",
            "sourceLine": 2,
        },
        {
            "instruction": "bsr.l AddFollower",
            "opcode": "bsr",
            "instructionTarget": "AddFollower",
            "sourceLine": 3,
        },
        {
            "instruction": "bsr.s LoadMapsprite",
            "opcode": "bsr",
            "instructionTarget": "LoadMapsprite",
            "sourceLine": 4,
        },
    ]


def test_source_section_strips_comment_before_call_instruction_parsing() -> None:
    source = "\n".join(
        (
            "demo:",
            "    jsr (WaitForVInt).w ; direct-call comment",
            "    rts",
            "; End of function demo",
        )
    )
    assert gesture._direct_calls(gesture._source_section(source, "demo")) == [
        {
            "instruction": "jsr (WaitForVInt).w",
            "opcode": "jsr",
            "instructionTarget": "WaitForVInt",
            "sourceLine": 2,
        }
    ]


def test_gesture_motion_observer_closes_config_and_emits_json_nulls() -> None:
    observer = gesture.OBSERVER.read_text(encoding="utf-8")

    assert "assert_closed_keys" in observer
    assert "forbidden key" in observer
    assert "value_or_json_null" in observer
    assert "value==nil or value==json_null" in observer
    assert "effective callback order drift" in observer
    assert "record.sourceLocal=source_local" in observer
    assert "copy(derived.sourceLocal)" not in observer
    assert "same_value(source_local[key],expected_source_value)" in observer
    assert "current-HP byte-probe character drift" in observer
    assert "shiverTemporarySizeAfterWriteAddress" in observer
    assert "nodFinalAnimCounterAfterWriteAddress" in observer
    assert "faceUpdateCallSiteAddress" in observer
    assert "flyZeroLayerAfterWriteAddress" in observer
    assert 'config["function"]' in observer
    assert "config.function" not in observer
