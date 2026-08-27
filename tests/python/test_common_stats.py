import json
import shutil
from copy import deepcopy

import pytest

from sf2tool.h2 import stats as stats_module
from sf2tool.h2.map_event_random_battle_state import (
    normalize_map_event_random_battle_state_later_owner_index,
)
from sf2tool.h2.stats import (
    _combatant_clamp_contract,
    _combatant_distance_contract,
    _combatant_getter_contract,
    _combatant_mutation_contract,
    _verify_stats_fixture_owner,
    build_stats_inventory,
    verify_stats_inventory,
)
from sf2tool.jsonio import load_json, schema_composition_audit, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
FIXTURE = repo_path("tests/fixtures/h2/common-stats-static-v1.json")
OUTPUT_SCHEMA = repo_path("schemas/h2/common-stats-output.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2/common-stats-fixture.schema.json")
SOURCE_RECORD_SCHEMA = repo_path("schemas/h2/common-stats-source-record.schema.json")
GETTERS_SCHEMA = repo_path("schemas/h2/common-stats-getters.schema.json")
MUTATIONS_SCHEMA = repo_path("schemas/h2/common-stats-mutations.schema.json")
CLAMPS_SCHEMA = repo_path("schemas/h2/common-stats-clamps.schema.json")
DISTANCE_SCHEMA = repo_path("schemas/h2/common-stats-distance.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
MUTATION_ROUTINES = [
    "LoadAllyName",
    "SetClass",
    "SetLevel",
    "SetMaxHp",
    "SetCurrentHp",
    "SetMaxMp",
    "SetCurrentMp",
    "SetBaseAtt",
    "SetCurrentAtt",
    "SetBaseDef",
    "SetCurrentDef",
    "SetBaseAgi",
    "SetCurrentAgi",
    "SetBaseMov",
    "SetCurrentMov",
    "SetBaseResistance",
    "SetCurrentResistance",
    "SetBaseProwess",
    "SetCurrentProwess",
    "SetStatusEffects",
    "SetCombatantX",
    "SetCombatantY",
    "SetCurrentExp",
    "SetMovetypeAndAiCommandset",
    "SetMoveOrders",
    "SetTriggerRegions",
    "SetActivationBitfield",
    "SetEnemyIndex",
    "IncreaseLevel",
    "IncreaseMaxHp",
    "IncreaseCurrentHp",
    "IncreaseMaxMp",
    "IncreaseCurrentMp",
    "IncreaseBaseAtt",
    "IncreaseCurrentAtt",
    "IncreaseBaseDef",
    "IncreaseCurrentDef",
    "IncreaseBaseAgi",
    "IncreaseCurrentAgi",
    "IncreaseBaseMov",
    "IncreaseCurrentMov",
    "IncreaseExp",
    "IncreaseKills",
    "IncreaseDefeats",
    "DecreaseCurrentHp",
    "DecreaseCurrentMp",
    "DecreaseCurrentAtt",
    "DecreaseBaseDef",
    "DecreaseCurrentDef",
    "DecreaseBaseAgi",
    "DecreaseCurrentAgi",
    "DecreaseBaseMov",
    "DecreaseCurrentMov",
]

CLAMP_ROUTINES = [
    "IncreaseAndClampByte",
    "IncreaseAndClamp7Bits",
    "DecreaseAndClampByte",
    "IncreaseAndClampWord",
    "DecreaseAndClampWord",
    "IncreaseAndClampLong",
    "DecreaseAndClampLong",
]


def _canonical_fixture_owner_output() -> dict[str, object]:
    fixture = load_json(FIXTURE)
    representative_symbols = fixture["expected"]["representativeSymbols"]
    return {
        "upstream": {"commit": fixture["upstreamCommit"]},
        "files": [
            {
                "path": f"code/common/stats/{relative}",
                "globalLabels": [symbol],
            }
            for relative, symbol in representative_symbols.items()
        ],
        "statsFacts": fixture["expected"]["statsFacts"],
        "alternateSources": fixture["expected"]["alternateSources"],
    }


def _assert_fixture_mutation_rejected(broken: dict[str, object], *, owner: str) -> None:
    try:
        validate_json(broken, FIXTURE_SCHEMA, owner=owner)
    except ValueError:
        return
    with pytest.raises(ValueError, match="common stats"):
        _verify_stats_fixture_owner(
            broken,
            _canonical_fixture_owner_output(),
            rom_manifest=load_json(ROM_MANIFEST),
            research_index=load_json(RESEARCH_INDEX),
        )


def _copy_getter_sources(tmp_path):
    upstream = UPSTREAM
    disasm = tmp_path / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/stats/combatantstats_1.asm",
        "code/common/stats/combatantstats_2.asm",
        "code/common/stats/combatantstats_3.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(upstream / "disasm" / relative, destination)
    listing = tmp_path / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(upstream / "build/sf2build-h1.lst", listing)
    return disasm


def _copy_distance_sources(tmp_path):
    disasm = _copy_getter_sources(tmp_path)
    for relative in (
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "code/gameflow/battle/battleactions/initbattlesceneproperties.asm",
        "code/gameflow/battle/battleactions/isabletocounterattack.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(UPSTREAM / "disasm" / relative, destination)
    return disasm


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_getter_contract_matches_full_fixture_and_boundaries() -> None:
    output = build_stats_inventory(UPSTREAM)
    fixture = load_json(FIXTURE)
    actual = output["statsFacts"]["combatantGetterContract"]
    expected = fixture["expected"]["statsFacts"]["combatantGetterContract"]
    assert actual == expected
    assert actual["routineOrder"] == [
        "GetCombatantName",
        "GetClass",
        "GetLevel",
        "GetMaxHp",
        "GetCurrentHp",
        "GetMaxMp",
        "GetCurrentMp",
        "GetBaseAtt",
        "GetCurrentAtt",
        "GetBaseDef",
        "GetCurrentDef",
        "GetBaseAgi",
        "GetCurrentAgi",
        "GetBaseMov",
        "GetCurrentMov",
        "GetBaseResistance",
        "GetCurrentResistance",
        "GetBaseProwess",
        "GetCurrentProwess",
        "GetStatusEffects",
        "GetCombatantX",
        "GetCombatantY",
        "GetCurrentExp",
        "GetMovetype",
        "GetAiCommandset",
        "GetMoveOrders",
        "GetTriggerRegions",
        "GetActivationBitfield",
        "GetEnemy",
        "GetKills",
        "GetDefeats",
    ]
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_1.asm",
        "startAddress": 33488,
        "endAddressExclusive": 34074,
        "physicalSpanBytes": 586,
    }
    assert actual["entryAddressAbi"] == expected["entryAddressAbi"]
    assert {
        name: actual["getters"][name]
        for name in ("GetMovetype", "GetAiCommandset", "GetMoveOrders", "GetTriggerRegions")
    } == {
        name: expected["getters"][name]
        for name in ("GetMovetype", "GetAiCommandset", "GetMoveOrders", "GetTriggerRegions")
    }
    assert actual["getters"]["GetCombatantName"] == expected["getters"]["GetCombatantName"]
    assert actual["getters"]["GetEnemy"] == expected["getters"]["GetEnemy"]
    assert set(actual["internalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert set(actual["externalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert actual["staticBoundary"] == {
        "callerVisibleMeaning": "inferred",
        "callerAndRuntimeOutcome": "unknown",
        "setterAndClampFollowup": "unknown",
    }


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_common_stats_index_relation_adds_only_combatant_getters_without_function_drift() -> None:
    current_index = load_json(RESEARCH_INDEX)
    accepted_base_index = normalize_map_event_random_battle_state_later_owner_index(
        current_index
    )

    def indexed_records(index: dict[str, object]) -> dict[str, dict[str, object]]:
        return {
            record["id"]: record
            for record in index["records"]
            if record["sourcePath"].startswith("code/common/stats/")
        }

    current_records = indexed_records(current_index)
    accepted_base_records = indexed_records(accepted_base_index)
    assert set(current_records) - set(accepted_base_records) == {"stats.combatant-getters"}
    assert set(accepted_base_records) - set(current_records) == set()

    output = build_stats_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 22
    assert output["summary"]["indexedFileCount"] == 17
    assert output["indexedRecordIds"] == sorted(current_records)
    assert output["indexedSourcePaths"] == sorted(
        {record["sourcePath"] for record in current_records.values()}
    )
    assert {
        record["sourcePath"] for record in current_records.values()
    } == {record["sourcePath"] for record in accepted_base_records.values()}

    def function_evidence(index: dict[str, object]) -> set[tuple[str, str, str]]:
        return {
            (record["id"], binding["fixtureField"], binding["addressId"])
            for record in index["records"]
            for evidence in record.get("evidence", [])
            if evidence.get("fixtureId") == stats_module.ID
            for binding in evidence.get("bindings", [])
        }

    assert function_evidence(current_index) == function_evidence(accepted_base_index)
    assert output["statsFacts"] == load_json(FIXTURE)["expected"]["statsFacts"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_mutation_contract_matches_full_fixture_and_boundaries() -> None:
    output = build_stats_inventory(UPSTREAM)
    fixture = load_json(FIXTURE)
    actual = output["statsFacts"]["combatantMutationContract"]
    expected = fixture["expected"]["statsFacts"]["combatantMutationContract"]
    assert actual == expected
    assert actual["routineOrder"] == MUTATION_ROUTINES
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_2.asm",
        "startAddress": 34138,
        "endAddressExclusive": 35184,
        "physicalSpanBytes": 1046,
    }
    assert actual["wrappers"] == expected["wrappers"]
    assert actual["dependencyAbi"] == expected["dependencyAbi"]
    dependencies = actual["dependencyAbi"]["routines"]
    assert list(dependencies) == [
        "DecreaseAndClampByte",
        "DecreaseAndClampWord",
        "IncreaseAndClamp7Bits",
        "IncreaseAndClampByte",
        "IncreaseAndClampWord",
        "SetCombatantByte",
        "SetCombatantWord",
    ]
    assert {name: fact["accessMode"] for name, fact in dependencies.items()} == {
        "DecreaseAndClampByte": "readModifyWrite",
        "DecreaseAndClampWord": "readModifyWrite",
        "IncreaseAndClamp7Bits": "readModifyWrite",
        "IncreaseAndClampByte": "readModifyWrite",
        "IncreaseAndClampWord": "readModifyWrite",
        "SetCombatantByte": "writeOnly",
        "SetCombatantWord": "writeOnly",
    }
    assert dependencies["SetCombatantByte"]["result"] is None
    assert dependencies["SetCombatantWord"]["result"] is None
    assert actual["classificationCounts"] == {
        "loadAllyName": 1,
        "directSet": 27,
        "increase": 16,
        "decrease": 9,
    }
    assert actual["internalEffectiveDirectCallSiteCounts"] == {
        name: 0 for name in actual["routineOrder"]
    }
    assert sum(actual["externalEffectiveDirectCallSiteCounts"].values()) == 184
    assert len(actual["externalDirectCallerOccurrences"]) == 34
    assert actual["wrappers"]["LoadAllyName"] == expected["wrappers"]["LoadAllyName"]
    assert actual["wrappers"]["SetMoveOrders"] == expected["wrappers"]["SetMoveOrders"]
    assert actual["wrappers"]["SetTriggerRegions"] == expected["wrappers"]["SetTriggerRegions"]
    assert actual["staticBoundary"] == {
        "clampAlgorithm": "unknown",
        "callerAndRuntimeOutcome": "unknown",
    }
    assert set(actual["internalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert set(actual["externalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_clamp_contract_matches_full_fixture_and_boundaries() -> None:
    output = build_stats_inventory(UPSTREAM)
    fixture = load_json(FIXTURE)
    actual = output["statsFacts"]["combatantClampContract"]
    expected = fixture["expected"]["statsFacts"]["combatantClampContract"]

    assert actual == expected
    assert actual["routineOrder"] == CLAMP_ROUTINES
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_3.asm",
        "startAddress": 37650,
        "endAddressExclusive": 37918,
        "physicalSpanBytes": 268,
    }
    assert actual["routineAddresses"] == {
        "IncreaseAndClampByte": 37650,
        "IncreaseAndClamp7Bits": 37684,
        "DecreaseAndClampByte": 37740,
        "IncreaseAndClampWord": 37782,
        "DecreaseAndClampWord": 37812,
        "IncreaseAndClampLong": 37850,
        "DecreaseAndClampLong": 37880,
    }
    assert actual["constants"] == {
        "BYTE_MASK": 255,
        "TWO_TURN_THRESHOLD": 128,
        "TURN_AGILITY_MASK": 127,
    }
    assert actual["externalEffectiveDirectCallSiteCounts"] == {
        "IncreaseAndClampByte": 10,
        "IncreaseAndClamp7Bits": 2,
        "DecreaseAndClampByte": 8,
        "IncreaseAndClampWord": 4,
        "DecreaseAndClampWord": 1,
        "IncreaseAndClampLong": 0,
        "DecreaseAndClampLong": 0,
    }
    assert actual["callerBoundary"] == {
        "wrapperSourcePath": "code/common/stats/combatantstats_2.asm",
        "allExternalSitesFromWrapperSource": True,
        "externalSiteCount": 25,
        "wrapperCalledRoutines": [
            "IncreaseAndClampByte",
            "IncreaseAndClamp7Bits",
            "DecreaseAndClampByte",
            "IncreaseAndClampWord",
            "DecreaseAndClampWord",
        ],
        "zeroDirectCallerRoutines": ["IncreaseAndClampLong", "DecreaseAndClampLong"],
    }
    assert actual["h3BoundaryCrossCheck"] == {
        "fixtureId": "sf2-stat-clamp-boundaries-v1",
        "fixtureCaseId": "slade-thif-level39-stat-clamps",
        "helpersObserved": {
            "increaseByte": True,
            "increaseWord": True,
            "increase7Bits": True,
            "decreaseByte": True,
        },
        "coveredHelpers": [
            {
                "name": "IncreaseAndClampByte",
                "helpersObservedField": "increaseByte",
                "fixtureFunctionField": "increaseAndClampByteAddress",
                "fixtureFunctionAddress": 37650,
                "operationIds": [
                    "base-attack-cap",
                    "base-defense-cap",
                    "current-move-cap",
                    "current-attack-byte-carry",
                ],
                "operationKinds": ["increase-byte"],
                "operationCount": 4,
            },
            {
                "name": "IncreaseAndClampWord",
                "helpersObservedField": "increaseWord",
                "fixtureFunctionField": "increaseAndClampWordAddress",
                "fixtureFunctionAddress": 37782,
                "operationIds": ["max-hp-word-wrap"],
                "operationKinds": ["increase-word"],
                "operationCount": 1,
            },
            {
                "name": "IncreaseAndClamp7Bits",
                "helpersObservedField": "increase7Bits",
                "fixtureFunctionField": "increaseAndClamp7BitsAddress",
                "fixtureFunctionAddress": 37684,
                "operationIds": ["base-agility-cap-preserves-turn-flag"],
                "operationKinds": ["increase-7bits"],
                "operationCount": 1,
            },
            {
                "name": "DecreaseAndClampByte",
                "helpersObservedField": "decreaseByte",
                "fixtureFunctionField": "decreaseAndClampByteAddress",
                "fixtureFunctionAddress": 37740,
                "operationIds": [
                    "current-defense-underflow",
                    "current-move-underflow",
                    "current-agility-underflow",
                ],
                "operationKinds": ["decrease-byte"],
                "operationCount": 3,
            },
        ],
        "uncoveredHelpers": [
            "DecreaseAndClampWord",
            "IncreaseAndClampLong",
            "DecreaseAndClampLong",
        ],
    }
    assert actual["staticBoundary"] == {
        "runtimeBehaviorBeyondExistingFixture": "unknown",
        "nextStaticFrontier": "GetDistanceBetweenCombatants",
    }
    assert all(
        actual["algorithms"][name]["routineAddress"] == actual["routineAddresses"][name]
        for name in CLAMP_ROUTINES
    )
    assert all(
        actual["algorithms"][name]["entryAddressCall"]["instructionIndex"]
        < actual["algorithms"][name]["fieldWrite"]["instructionIndex"]
        < actual["algorithms"][name]["preserveRestore"]["terminal"]["instructionIndex"]
        for name in CLAMP_ROUTINES
    )
    assert actual["algorithms"]["IncreaseAndClamp7Bits"]["fieldMask"] == {
        "constant": "TURN_AGILITY_MASK",
        "value": 127,
        "instruction": {
            "instructionIndex": 5,
            "opcode": "andi.b",
            "operands": ["#TURN_AGILITY_MASK", "d2"],
        },
    }
    assert actual["algorithms"]["IncreaseAndClamp7Bits"]["preservedBitsMask"] == {
        "constant": "TWO_TURN_THRESHOLD",
        "value": 128,
        "instruction": {
            "instructionIndex": 4,
            "opcode": "andi.b",
            "operands": ["#TWO_TURN_THRESHOLD", "d3"],
        },
    }
    assert actual["boundedFunctionOrder"] == CLAMP_ROUTINES
    assert actual["algorithms"]["IncreaseAndClampLong"]["maximumComparison"] == {
        "instructionIndex": 3,
        "opcode": "cmp.l",
        "operands": ["d6", "d1"],
        "followingBranch": {
            "instructionIndex": 4,
            "opcode": "bcs.s",
            "sourceTarget": "loc_93EC",
            "parsedBranchTarget": "loc_93EC",
            "conditionCode": "cs",
        },
    }
    assert actual["algorithms"]["DecreaseAndClampWord"]["maximumComparison"][
        "followingBranch"
    ] == {
        "instructionIndex": 11,
        "opcode": "bls.s",
        "sourceTarget": "@Continue",
        "parsedBranchTarget": None,
        "conditionCode": "ls",
    }
    assert actual["algorithms"]["IncreaseAndClamp7Bits"]["controlFlowInstructionOrder"] == list(
        range(20)
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_distance_contract_matches_full_fixture_and_boundaries() -> None:
    actual = build_stats_inventory(UPSTREAM)["statsFacts"]["combatantDistanceContract"]
    expected = load_json(FIXTURE)["expected"]["statsFacts"]["combatantDistanceContract"]
    assert actual == expected
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_3.asm",
        "startAddress": 37918,
        "endAddressExclusive": 38018,
        "physicalSpanBytes": 100,
    }
    assert actual["instructionEncodedByteCount"] == 100
    assert actual["abi"] == {
        "inputSelectors": [
            {
                "register": "d0",
                "meaningfulWidthBits": 16,
                "widthUseSite": {
                    "instructionIndex": 14,
                    "opcode": "move.w",
                    "operands": ["d5", "d0"],
                },
            },
            {
                "register": "d1",
                "meaningfulWidthBits": 16,
                "widthUseSite": {
                    "instructionIndex": 1,
                    "opcode": "move.w",
                    "operands": ["d1", "d5"],
                },
            },
        ],
        "result": {
            "register": "d2",
            "meaningfulWidthBits": 16,
            "widthUseSites": [
                {"instructionIndex": 23, "opcode": "sub.w", "operands": ["d4", "d2"]},
                {"instructionIndex": 26, "opcode": "sub.w", "operands": ["d5", "d3"]},
                {"instructionIndex": 29, "opcode": "add.w", "operands": ["d3", "d2"]},
                {"instructionIndex": 31, "opcode": "move.w", "operands": ["#-1", "d2"]},
            ],
        },
        "preservedRegisters": "d0-d1/d3-d5",
        "coordinateValueWidthBits": 16,
        "coordinateWidthUseSites": [
            {"instructionIndex": 9, "opcode": "move.w", "operands": ["d1", "d2"]},
            {"instructionIndex": 13, "opcode": "move.w", "operands": ["d1", "d3"]},
            {"instructionIndex": 18, "opcode": "move.w", "operands": ["d1", "d4"]},
            {"instructionIndex": 22, "opcode": "move.w", "operands": ["d1", "d5"]},
        ],
        "deltaWorkingRegisters": ["d2", "d3"],
    }
    assert actual["callerCounts"] == {
        "directSiteCount": 2,
        "aliasSiteCount": 0,
        "effectiveSiteCount": 2,
    }
    assert actual["callerTargetCounts"] == {
        "internalDirect": {"GetDistanceBetweenCombatants": 0},
        "externalDirect": {"GetDistanceBetweenCombatants": 2},
        "internalAlias": {"GetDistanceBetweenCombatants": 0},
        "externalAlias": {"GetDistanceBetweenCombatants": 0},
    }
    assert actual["directCallerSites"] == [
        {
            "sourcePath": "code/gameflow/battle/battleactions/initbattlesceneproperties.asm",
            "lineNumber": 88,
            "opcode": "jsr",
            "instructionTarget": "GetDistanceBetweenCombatants",
            "effectiveTarget": "GetDistanceBetweenCombatants",
        },
        {
            "sourcePath": "code/gameflow/battle/battleactions/isabletocounterattack.asm",
            "lineNumber": 78,
            "opcode": "jsr",
            "instructionTarget": "GetDistanceBetweenCombatants",
            "effectiveTarget": "GetDistanceBetweenCombatants",
        },
    ]
    assert actual["jumpInterfaceAliases"] == {
        "j_GetDistanceBetweenCombatants": {
            "effectiveTarget": "GetDistanceBetweenCombatants",
            "sourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        }
    }
    assert actual["aliasCallerSites"] == []
    assert actual["callerBoundary"] == {
        "internalDirectSiteCount": 0,
        "externalDirectSiteCount": 2,
        "internalAliasSiteCount": 0,
        "externalAliasSiteCount": 0,
        "aliasDefinitionsAreNotCallSites": True,
    }
    assert actual["controlFlow"]["instructionOrder"] == list(range(34))
    assert actual["controlFlow"]["labelInstructionIndices"] == {
        "GetDistanceBetweenCombatants": 0,
        "@loc_1": 26,
        "@loc_2": 29,
        "@loc_3": 31,
        "@Done": 32,
    }
    assert actual["h3Boundary"]["fixtureIds"] == []


def test_combatant_distance_schema_rejects_deep_drift() -> None:
    fixture = load_json(FIXTURE)

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantDistanceContract"])
        _assert_fixture_mutation_rejected(broken, owner="statsFacts distance contract")

    invalid(lambda value: value["sourceRange"].pop("endAddressExclusive"))
    invalid(lambda value: value["routineOperations"].reverse())
    invalid(lambda value: value["routineOperations"][0].__setitem__("extra", True))
    invalid(lambda value: value["controlFlow"]["coordinateCalls"].pop("targetY"))
    invalid(
        lambda value: value["controlFlow"]["firstAxis"]["noBorrowBranch"].__setitem__(
            "sourceTarget", "@loc_2"
        )
    )
    invalid(lambda value: value["directCallerSites"][0].__setitem__("lineNumber", 87))
    invalid(
        lambda value: value["callerTargetCounts"]["internalDirect"].pop(
            "GetDistanceBetweenCombatants"
        )
    )
    invalid(lambda value: value["abi"]["inputSelectors"][0]["widthUseSite"].pop("opcode"))
    invalid(lambda value: value.__setitem__("unexpected", True))


def test_combatant_clamp_schema_rejects_complete_shape_drift() -> None:
    fixture = load_json(FIXTURE)
    schema = load_json(CLAMPS_SCHEMA)

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantClampContract"])
        _assert_fixture_mutation_rejected(broken, owner="statsFacts clamp contract")

    invalid(lambda value: value["routineOrder"].reverse())
    invalid(lambda value: value["routineAddresses"].__setitem__("IncreaseAndClampLong", 37851))
    invalid(lambda value: value["sourceRange"].pop("physicalSpanBytes"))
    invalid(lambda value: value["constants"].__setitem__("BYTE_MASK", 127))
    invalid(lambda value: value["algorithms"]["IncreaseAndClamp7Bits"].pop("fieldMask"))
    invalid(
        lambda value: value["algorithms"]["DecreaseAndClampWord"]["maximumComparison"][
            "followingBranch"
        ].__setitem__("sourceTarget", "@Other")
    )
    invalid(lambda value: value["algorithms"]["IncreaseAndClampByte"].pop("maximumAssignment"))
    invalid(
        lambda value: value["algorithms"]["IncreaseAndClampWord"].__setitem__(
            "maximumComparisonRenamed",
            value["algorithms"]["IncreaseAndClampWord"].pop("maximumComparison"),
        )
    )
    invalid(
        lambda value: value["algorithms"]["IncreaseAndClamp7Bits"]["preserveRestore"][
            "save"
        ].__setitem__("unexpected", True)
    )
    invalid(
        lambda value: value["h3BoundaryCrossCheck"]["coveredHelpers"][0]["operationIds"].reverse()
    )
    invalid(
        lambda value: value["callerBoundary"]["zeroDirectCallerRoutines"].__setitem__(
            0, "IncreaseAndClampByte"
        )
    )
    invalid(lambda value: value["routineOperations"]["IncreaseAndClampByte"][1].pop("opcode"))
    invalid(
        lambda value: value["externalDirectCallerOccurrences"][
            "code/common/stats/combatantstats_2.asm"
        ][0].__setitem__("effectiveTarget", "IncreaseAndClampLong")
    )
    invalid(lambda value: value["h3BoundaryCrossCheck"].pop("uncoveredHelpers"))
    invalid(lambda value: value["boundedFunctionOrder"].reverse())
    invalid(lambda value: value.__setitem__("unexpected", True))
    routine_operations = schema["properties"]["routineOperations"]
    assert all(
        item["allOf"][0]["items"]
        == {
            "$ref": (
                "https://sf2-research.example/schemas/h2/"
                "common-stats-source-record.schema.json"
            )
        }
        for item in routine_operations["properties"].values()
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("routine", "needle", "replacement"),
    (
        ("IncreaseAndClampByte", "bcs.s   @MakeMaxValue", "bcc.s   @MakeMaxValue"),
        ("IncreaseAndClampByte", "bcs.s   @Continue", "bcs.s   @Done"),
        ("IncreaseAndClampByte", "cmp.b   d6,d1", "cmp.b   d5,d1"),
        ("IncreaseAndClampByte", "move.b  d6,d1", "move.b  d5,d1"),
        ("IncreaseAndClampByte", "andi.w  #BYTE_MASK,d1", "andi.w  #TURN_AGILITY_MASK,d1"),
        ("IncreaseAndClampWord", "move.w  d1,(a0,d7.w)", "move.w  d1,(a0,d6.w)"),
        ("IncreaseAndClampWord", "move.w  d6,d1", "move.w  d6,d2"),
        ("DecreaseAndClampByte", "move.b  d5,d1", "move.b  d6,d1"),
        ("DecreaseAndClampByte", "move.w  d4,-(sp)", "move.w  d3,-(sp)"),
        ("DecreaseAndClampWord", "bhi.s   @CheckForMaxValue", "bls.s   @CheckForMaxValue"),
        ("DecreaseAndClampWord", "bhi.s   @CheckForMaxValue", "bhi.s   @MakeMinValue"),
        ("DecreaseAndClampWord", "cmp.w   d6,d1", "cmp.w   d5,d1"),
        ("IncreaseAndClampLong", "add.l   (a0,d7.w),d1", "sub.l   (a0,d7.w),d1"),
        ("IncreaseAndClampLong", "bmi.s   loc_93E8", "bmi.s   loc_93EC"),
        ("IncreaseAndClampLong", "rts", "nop"),
        ("DecreaseAndClampLong", "move.l  (sp)+,d4", "move.l  (sp)+,d3"),
        ("DecreaseAndClampLong", "move.l  d1,(a0,d7.w)", "move.l  d1,(a0,d6.w)"),
        ("DecreaseAndClampLong", "move.l  d6,d1", "move.l  d6,d2"),
        ("IncreaseAndClamp7Bits", "or.b    d3,d1", "or.b    d2,d1"),
        ("IncreaseAndClamp7Bits", "movem.w d2-d3,-(sp)", "movem.w d2-d4,-(sp)"),
        (
            "IncreaseAndClamp7Bits",
            "andi.b  #TURN_AGILITY_MASK,d2",
            "andi.b  #BYTE_MASK,d2",
        ),
        (
            "IncreaseAndClamp7Bits",
            "movem.w (sp)+,d2-d3",
            "movem.w (sp)+,d2-d4",
        ),
        ("IncreaseAndClamp7Bits", "move.b  d1,(a0,d7.w)", "move.b  d1,(a0,d6.w)"),
        (
            "IncreaseAndClamp7Bits",
            "andi.w  #BYTE_MASK,d1",
            "andi.w  #TURN_AGILITY_MASK,d1",
        ),
    ),
)
def test_combatant_clamp_scoped_contract_rejects_control_flow_and_data_mutations(
    tmp_path, routine, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_3.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index(f"{routine}:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    expected = load_json(FIXTURE)["expected"]["statsFacts"]["combatantClampContract"]
    try:
        actual = _combatant_clamp_contract(disasm)
    except ValueError as error:
        assert "combatant clamp" in str(error)
    else:
        assert actual != expected
    assert expected["routineOrder"] == CLAMP_ROUTINES


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        ("bsr.w   GetCombatantX", "bsr.w   GetCombatantY"),
        ("beq.w   @loc_3", "bne.w   @loc_3"),
        ("bcc.s   @loc_1", "bcc.s   @loc_2"),
        ("move.w  d1,d2", "move.b  d1,d2"),
        ("sub.w   d4,d2", "sub.w   d5,d2"),
        ("add.w   d3,d2", "sub.w   d3,d2"),
        ("move.w  #-1,d2", "move.w  #0,d2"),
        ("@loc_1:", "@loc_4:"),
        ("movem.l d0-d1/d3-d5,-(sp)", "movem.l d0-d1/d3-d4,-(sp)"),
        ("movem.l (sp)+,d0-d1/d3-d5", "movem.l (sp)+,d0-d1/d3-d4"),
        ("rts", "nop"),
    ),
)
def test_combatant_distance_function_scoped_guards_reject_mutations(
    tmp_path, needle, replacement
) -> None:
    disasm = _copy_distance_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_3.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index("GetDistanceBetweenCombatants:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant distance"):
        _combatant_distance_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_distance_inventory_scans_injected_owning_file_call(tmp_path) -> None:
    disasm = _copy_distance_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_3.asm"
    source.write_text(
        (
            source.read_text(encoding="utf-8")
            + "\n                jsr     GetDistanceBetweenCombatants\n"
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="direct caller inventory"):
        _combatant_distance_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        ("0000941E                            GetDistanceBetweenCombatants:",
         "00009420                            GetDistanceBetweenCombatants:"),
        ("0000941E 48E7 DC00", "0000941E 48E7 DC"),
        (
            "00009482                                ; End of function "
            "GetDistanceBetweenCombatants",
            "00009480                                ; End of function "
            "GetDistanceBetweenCombatants",
        ),
    ),
)
def test_combatant_distance_h1_boundary_and_encoding_guards_reject_mutations(
    tmp_path, needle, replacement
) -> None:
    disasm = _copy_distance_sources(tmp_path)
    listing = disasm.parent / "build/sf2build-h1.lst"
    text = listing.read_text(encoding="utf-8")
    assert needle in text
    listing.write_text(text.replace(needle, replacement, 1), encoding="utf-8")
    with pytest.raises(ValueError, match="combatant distance"):
        _combatant_distance_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_distance_caller_parser_ignores_non_calls_and_accepts_suffixes(tmp_path) -> None:
    expected = load_json(FIXTURE)["expected"]["statsFacts"]["combatantDistanceContract"]
    for directory_name, statement in (
        ("comment", "; jsr.l GetDistanceBetweenCombatants"),
        ("near-miss", "jsrx    GetDistanceBetweenCombatants"),
    ):
        disasm = _copy_distance_sources(tmp_path / directory_name)
        source = disasm / "code/gameflow/battle/battleactions/initbattlesceneproperties.asm"
        source.write_text(source.read_text(encoding="utf-8") + f"\n{statement}\n", encoding="utf-8")
        assert _combatant_distance_contract(disasm) == expected

    disasm = _copy_distance_sources(tmp_path / "legal-suffix")
    source = disasm / "code/gameflow/battle/battleactions/initbattlesceneproperties.asm"
    source.write_text(
        (
            source.read_text(encoding="utf-8")
            + "\n                jsr.l   GetDistanceBetweenCombatants\n"
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="direct caller inventory"):
        _combatant_distance_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_distance_caller_identity_change_fails_construction(tmp_path) -> None:
    disasm = _copy_distance_sources(tmp_path)
    source = disasm / "code/gameflow/battle/battleactions/isabletocounterattack.asm"
    source.write_text(
        source.read_text(encoding="utf-8").replace(
            "jsr     GetDistanceBetweenCombatants", "jsr     j_GetDistanceBetweenCombatants", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="direct caller inventory"):
        _combatant_distance_contract(disasm)


def test_combatant_mutation_schema_rejects_deep_drift() -> None:
    fixture = load_json(FIXTURE)
    schema = load_json(MUTATIONS_SCHEMA)

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantMutationContract"])
        _assert_fixture_mutation_rejected(broken, owner="statsFacts mutation contract")

    invalid(lambda value: value["wrappers"].pop("SetMoveOrders"))
    invalid(lambda value: value["routineOrder"].reverse())
    invalid(
        lambda value: value["wrappers"]["SetTriggerRegions"]["packedMerge"].__setitem__(
            "resultRegister", "d2"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["SetCombatantByte"].__setitem__(
            "storedWidthBits", 16
        )
    )
    invalid(lambda value: value["wrappers"]["LoadAllyName"]["copy"].pop("copiedBytes"))
    invalid(lambda value: value["wrappers"]["LoadAllyName"]["copy"].__setitem__("unexpected", True))
    invalid(
        lambda value: value["wrappers"].__setitem__(
            "SetMoveOrdersRenamed", value["wrappers"].pop("SetMoveOrders")
        )
    )
    invalid(lambda value: value["sourceRange"].__setitem__("physicalSpanBytes", 1047))
    invalid(
        lambda value: value["wrappers"]["LoadAllyName"]["copy"].__setitem__("copyIterations", 9)
    )
    invalid(
        lambda value: value["wrappers"]["IncreaseKills"]["guard"].__setitem__(
            "returnBranchTarget", "@Other"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["IncreaseAndClampByte"].pop(
            "fieldReadInstructionIndex"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["SetCombatantByte"].__setitem__(
            "accessMode", "readModifyWrite"
        )
    )
    invalid(lambda value: value.__setitem__("unexpected", True))
    assert schema["additionalProperties"] is False
    corpus = schema["properties"]["routineOperations"]
    assert all(
        item["items"]
        == {
            "$ref": (
                "https://sf2-research.example/schemas/h2/"
                "common-stats-source-record.schema.json"
            )
        }
        for item in corpus["properties"].values()
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("relative", "needle", "replacement"),
    (
        (
            "combatantstats_2.asm",
            "moveq   #COMBATANT_OFFSET_LEVEL,d7",
            "moveq   #COMBATANT_OFFSET_CLASS,d7",
        ),
        ("combatantstats_2.asm", "bsr.w   SetCombatantWord", "bsr.w   SetCombatantByte"),
        ("combatantstats_2.asm", "lsl.w   #BYTE_SHIFT_COUNT,d1", "lsl.w   #NIBBLE_SHIFT_COUNT,d1"),
        ("combatantstats_2.asm", "or.b    d2,d1", "or.b    d1,d2"),
        ("combatantstats_2.asm", "lea     (a0),a1", "lea     (a0),a0"),
        (
            "combatantstats_2.asm",
            "moveq   #ALLYNAME_CHARACTERS_COUNTER,d0",
            "moveq   #BYTE_SHIFT_COUNT,d0",
        ),
        ("combatantstats_2.asm", "dbf     d0,@Loop", "dbf     d0,@Return"),
        ("combatantstats_2.asm", "clr.w   d5", "moveq   #1,d5"),
        (
            "combatantstats_2.asm",
            "move.w  #CHAR_STATCAP_LEVEL,d6",
            "move.w  #CHAR_STATCAP_HP,d6",
        ),
        ("combatantstats_2.asm", "blt.s   @Return", "bge.s   @Return"),
        (
            "combatantstats_2.asm",
            "move.w  COMBATANT_OFFSET_HP_MAX(a0),d6",
            "move.b  COMBATANT_OFFSET_HP_MAX(a0),d6",
        ),
        (
            "combatantstats_2.asm",
            "rts\n\n    ; End of function SetClass",
            "nop\n\n    ; End of function SetClass",
        ),
        ("combatantstats_3.asm", "move.b  d1,(a0,d7.w)", "move.w  d1,(a0,d7.w)"),
        ("combatantstats_3.asm", "add.b   (a0,d7.w),d1", "add.w   (a0,d7.w),d1"),
    ),
)
def test_combatant_mutation_guards_reject_semantic_mutations(
    tmp_path, relative, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats" / relative
    source.write_text(
        source.read_text(encoding="utf-8").replace(needle, replacement, 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant mutation"):
        _combatant_mutation_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("routine", "needle", "replacement"),
    (
        ("IncreaseCurrentHp", "#COMBATANT_OFFSET_HP_CURRENT,d7", "#COMBATANT_OFFSET_HP_MAX,d7"),
        ("DecreaseCurrentMp", "#COMBATANT_OFFSET_MP_CURRENT,d7", "#COMBATANT_OFFSET_MP_MAX,d7"),
        ("SetMoveOrders", "#COMBATANT_OFFSET_MOVE_ORDERS,d7", "#COMBATANT_OFFSET_ALLY_KILLS,d7"),
        ("IncreaseCurrentDef", "movem.l d5-a0,-(sp)", "movem.l d6-a0,-(sp)"),
    ),
)
def test_combatant_mutation_scoped_guards_reject_nonfirst_source_drift(
    tmp_path, routine, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_2.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index(f"{routine}:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant mutation"):
        _combatant_mutation_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("relative", "needle", "replacement"),
    (
        ("combatantstats_1.asm", "bne.s   @Enemy", "beq.s   @Enemy"),
        (
            "combatantstats_1.asm",
            "moveq   #COMBATANT_OFFSET_LEVEL,d7",
            "moveq   #COMBATANT_OFFSET_CLASS,d7",
        ),
        (
            "combatantstats_1.asm",
            "moveq   #COMBATANT_OFFSET_HP_MAX,d7",
            "moveq   #COMBATANT_OFFSET_HP_CURRENT,d7",
        ),
        ("combatantstats_1.asm", "bsr.w   GetCombatantWord", "bsr.w   GetCombatantByte"),
        ("combatantstats_1.asm", "move.w  d1,d2", "move.w  d2,d1"),
        ("combatantstats_1.asm", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("combatantstats_1.asm", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2", "andi.w  #BYTE_MASK,d2"),
        ("combatantstats_1.asm", "move.w  #-1,d1", "move.w  #0,d1"),
        (
            "combatantstats_1.asm",
            "rts\n\n    ; End of function GetEnemy",
            "bra.s   @Continue\n\n    ; End of function GetEnemy",
        ),
        ("combatantstats_3.asm", "bcc.s   @Enemy", "bcs.s   @Enemy"),
        ("combatantstats_3.asm", "clr.w   d1", "nop"),
        ("combatantstats_3.asm", "move.w  (a0,d7.w),d1", "move.b  (a0,d7.w),d1"),
    ),
)
def test_combatant_getter_guards_reject_semantic_mutations(
    tmp_path, relative, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats" / relative
    source.write_text(
        source.read_text(encoding="utf-8").replace(needle, replacement, 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant|shop"):
        _combatant_getter_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("routine", "needle", "replacement"),
    (
        ("GetMovetype", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("GetMovetype", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1", "andi.w  #BYTE_MASK,d1"),
        ("GetAiCommandset", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1", "andi.w  #BYTE_MASK,d1"),
        ("GetMoveOrders", "move.w  d1,d2", "move.w  d2,d1"),
        ("GetMoveOrders", "lsr.w   #BYTE_SHIFT_COUNT,d1", "lsr.w   #NIBBLE_SHIFT_COUNT,d1"),
        ("GetMoveOrders", "andi.w  #BYTE_MASK,d1", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1"),
        ("GetMoveOrders", "andi.w  #BYTE_MASK,d2", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2"),
        ("GetTriggerRegions", "move.w  d1,d2", "move.w  d2,d1"),
        ("GetTriggerRegions", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("GetTriggerRegions", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2", "andi.w  #BYTE_MASK,d2"),
    ),
)
def test_composite_getter_guards_reject_copy_shift_and_mask_mutations(
    tmp_path, routine, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_1.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index(f"{routine}:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant"):
        _combatant_getter_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_getter_component_is_shared_closed_and_rejects_drift() -> None:
    fixture = load_json(FIXTURE)
    facts = load_json(GETTERS_SCHEMA)
    assert facts["additionalProperties"] is False
    assert facts["properties"]["entryAddressAbi"]["additionalProperties"] is False
    assert facts["properties"]["getters"]["additionalProperties"] is False

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantGetterContract"])
        _assert_fixture_mutation_rejected(broken, owner="statsFacts getter contract")

    invalid(lambda value: value.pop("routineOrder"))
    invalid(lambda value: value["getters"].pop("GetEnemy"))
    invalid(
        lambda value: value["getters"].__setitem__(
            "GetEnemyRenamed", value["getters"].pop("GetEnemy")
        )
    )
    invalid(lambda value: value["routineOrder"].reverse())
    invalid(lambda value: value.__setitem__("unexpected", True))
    invalid(lambda value: value["sourceRange"].pop("physicalSpanBytes"))
    invalid(
        lambda value: value["getters"]["GetMoveOrders"]["splitValue"]["copy"].__setitem__(
            "destinationRegister", "d1"
        )
    )
    invalid(
        lambda value: value["entryAddressAbi"]["routines"]["GetCombatantEntryAddress"][
            "validRoutes"
        ]["ally"].__setitem__("enemyThresholdBranchTaken", True)
    )


def test_combatant_stats_roots_reuse_the_same_structural_components() -> None:
    fixture = load_json(FIXTURE)
    assert set(fixture["expected"]["statsFacts"]) == {
        "flags",
        "party",
        "inventories",
        "combatantType",
        "spells",
        "newGame",
        "inventoryBoundary",
        "combatantGetterContract",
        "combatantMutationContract",
        "combatantClampContract",
        "combatantDistanceContract",
    }
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    assert "definitions" not in output_schema
    assert "definitions" not in fixture_schema
    component_refs = {
        "combatantGetterContract": (
            "https://sf2-research.example/schemas/h2/common-stats-getters.schema.json"
        ),
        "combatantMutationContract": (
            "https://sf2-research.example/schemas/h2/common-stats-mutations.schema.json"
        ),
        "combatantClampContract": (
            "https://sf2-research.example/schemas/h2/common-stats-clamps.schema.json"
        ),
        "combatantDistanceContract": (
            "https://sf2-research.example/schemas/h2/common-stats-distance.schema.json"
        ),
    }
    output_facts = output_schema["properties"]["statsFacts"]["properties"]
    fixture_facts = fixture_schema["properties"]["expected"]["properties"]["statsFacts"][
        "properties"
    ]
    for field, reference in component_refs.items():
        assert output_facts[field] == {"$ref": reference}
        assert fixture_facts[field] == {"$ref": reference}


def test_common_stats_function_address_golden_is_owned_outside_schema() -> None:
    fixture = load_json(FIXTURE)
    broken = deepcopy(fixture)
    broken["function"]["updateForceAddress"] += 2
    validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid function-address mutation")
    with pytest.raises(ValueError, match="common stats function binding drift"):
        _verify_stats_fixture_owner(
            broken,
            _canonical_fixture_owner_output(),
            rom_manifest=load_json(ROM_MANIFEST),
            research_index=load_json(RESEARCH_INDEX),
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_common_stats_verifier_rejects_schema_valid_function_drift_before_write(
    tmp_path, monkeypatch
) -> None:
    broken = deepcopy(load_json(FIXTURE))
    broken["function"]["updateForceAddress"] += 2
    fixture_path = tmp_path / "common-stats.json"
    fixture_path.write_text(json.dumps(broken, indent=2) + "\n", encoding="utf-8")
    validate_json(broken, FIXTURE_SCHEMA, owner="schema-valid function-address mutation")

    output_path = tmp_path / "output.json"
    monkeypatch.setattr(stats_module, "FIXTURE", fixture_path)
    with pytest.raises(ValueError, match="common stats function binding drift"):
        verify_stats_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


def test_common_stats_function_binding_rejects_index_address_and_owner_drift() -> None:
    fixture = load_json(FIXTURE)
    output = _canonical_fixture_owner_output()
    rom_manifest = load_json(ROM_MANIFEST)

    wrong_address = deepcopy(load_json(RESEARCH_INDEX))
    party = next(record for record in wrong_address["records"] if record["id"] == "stats.party")
    party["addresses"][0]["value"] += 2
    with pytest.raises(ValueError, match="common stats function binding drift"):
        _verify_stats_fixture_owner(
            fixture,
            output,
            rom_manifest=rom_manifest,
            research_index=wrong_address,
        )

    wrong_owner = deepcopy(load_json(RESEARCH_INDEX))
    party = next(record for record in wrong_owner["records"] if record["id"] == "stats.party")
    party["evidence"][0]["verifier"] = "src/sf2tool/h2/other.py"
    with pytest.raises(ValueError, match="common stats research-index evidence owner drift"):
        _verify_stats_fixture_owner(
            fixture,
            output,
            rom_manifest=rom_manifest,
            research_index=wrong_owner,
        )


def test_common_stats_schema_composition_audit_stays_local_and_golden_free() -> None:
    schema_paths = [
        SOURCE_RECORD_SCHEMA,
        GETTERS_SCHEMA,
        MUTATIONS_SCHEMA,
        CLAMPS_SCHEMA,
        DISTANCE_SCHEMA,
        OUTPUT_SCHEMA,
        FIXTURE_SCHEMA,
    ]
    report = schema_composition_audit(schema_paths)

    assert report["schemaCount"] == 7
    assert report["totalSizeBytes"] < 1_100_000
    assert report["constCount"] == 4
    assert report["largeConstCount"] == 0
    assert report["referencedResourceCount"] == 5
    assert report["unresolvedReferences"] == []
    assert report["duplicateBodyGroups"] == []
    components = report["files"][:5]
    assert all(component["constCount"] == 0 for component in components)
