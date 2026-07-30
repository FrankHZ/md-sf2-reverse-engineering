from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.entity_population_reload as entity_population_reload
from sf2tool.cli import build_parser
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _fixture() -> dict[str, object]:
    return load_json(entity_population_reload.FIXTURE)


def _observation(fixture: dict[str, object]) -> dict[str, object]:
    return entity_population_reload._expected_observation(fixture)  # type: ignore[arg-type]


def test_entity_population_reload_static_contract_is_complete_and_exact() -> None:
    fixture = _fixture()
    validate_json(
        fixture,
        entity_population_reload.FIXTURE_SCHEMA,
        owner="entity population fixture",
    )
    actual = entity_population_reload.build_entity_population_reload_contract(ROM, UPSTREAM)

    assert {key: fixture[key] for key in ("function", "ram", "constants", "sourceFacts")} == actual
    assert actual["function"] == {
        "runMapSetupInitFunctionAddress": 292092,
        "newEntityHandlerAddress": 290360,
        "loadMapEntitiesHandlerAddress": 288394,
        "reloadEntitiesHandlerAddress": 288456,
        "loadEntitiesFromMapSetupHandlerAddress": 288600,
        "initializeNewEntityAddress": 279920,
        "initializeMapEntitiesAddress": 278732,
        "getEntityAddressFromCharacterAddress": 290890,
        "getMapSetupEntityListAddress": 292752,
        "loadEntityMapspritesAddress": 24612,
    }
    assert actual["constants"] == {
        "entityRecordByteCount": 32,
        "mapTileSize": 384,
        "mapTacticalBase": 46,
        "identitySignedByteBranchInstruction": "bpl.s @Ally",
        "entityEnemyIndexDifference": 96,
        "combatantMaskAll": 255,
        "allocationScanCounter": 62,
        "allocationScanItemCount": 63,
        "allocationIncrementInstruction": "addq.w #1,d0",
        "coordinateScaleSymbol": "MAP_TILE_SIZE",
        "coordinateScaleValue": 384,
        "clearRecordCounter": 48,
        "clearRecordCount": 49,
        "emptyCoordinateWord": 28672,
        "entityFieldOffsets": {
            "xWord": 0,
            "yWord": 2,
            "xDestWord": 12,
            "facingByte": 16,
            "entityNumberByte": 18,
            "mapspriteByte": 19,
        },
    }
    source = actual["sourceFacts"]
    assert [row["macro"] for row in source["handlers"]] == [
        "newEntity",
        "loadMapEntities",
        "reloadEntities",
        "loadEntitiesFromMapSetup",
    ]
    assert [
        [callback["instructionTarget"] for callback in row["callbacks"]]
        for row in source["handlers"]
    ] == [
        ["InitializeNewEntity"],
        [
            "DisableDisplayAndInterrupts",
            "InitializeMapEntities",
            "LoadEntityMapsprites",
            "EnableDisplayAndInterrupts",
        ],
        ["GetEntityAddressFromCharacter", "InitializeMapEntities"],
        [
            "DisableDisplayAndInterrupts",
            "GetMapSetupEntityList",
            "j_InitializeMapEntities",
            "LoadEntityMapsprites",
            "EnableDisplayAndInterrupts",
        ],
    ]
    assert source["callerBreakdown"] == fixture["sourceFacts"]["callerBreakdown"]
    assert source["runtimeQuestions"] == [
        "entity-population-reload/allocation-capacity-beyond-observed-high-water",
        "entity-population-reload/normal-story-reachability-and-save-map-reload-persistence",
        "entity-population-reload/player-visible-rendering-animation-vdp-timing",
        "entity-population-reload/collision-pathfinding-consumer-effects",
    ]

    case_inputs = entity_population_reload._case_inputs(actual, fixture)  # type: ignore[arg-type]
    assert [row["id"] for row in case_inputs] == fixture["runtimeGolden"]["recordOrder"]
    assert [row["macro"] for row in case_inputs] == [
        "newEntity",
        "newEntity",
        "newEntity",
        "loadMapEntities",
        "reloadEntities",
        *["loadEntitiesFromMapSetup"] * 7,
    ]
    assert all("runtimeGolden" not in row for row in case_inputs)
    observation = _observation(fixture)
    validate_json(
        observation,
        entity_population_reload.OBSERVATION_SCHEMA,
        owner="entity population observation",
    )
    assert observation["records"] == fixture["runtimeGolden"]["records"]
    assert observation["recordOrder"] == fixture["runtimeGolden"]["recordOrder"]


def test_entity_population_reload_h2_use_site_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    static = build_map_script_engine_contract(ROM, UPSTREAM)
    mutated = deepcopy(static)
    uses = mutated["entityPopulationCommandFacts"]["handlers"][2]["sectionGuard"][
        "sourceConstantUses"
    ]
    uses[0]["value"] = 385
    monkeypatch.setattr(
        entity_population_reload, "build_map_script_engine_contract", lambda *_: mutated
    )
    with pytest.raises(ValueError, match="reload source-use value drift"):
        entity_population_reload.build_entity_population_reload_contract(ROM, UPSTREAM)


def test_entity_population_reload_h2_input_and_map_init_joins_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """The selected source-program and map-init identities are construction guards."""
    static = build_map_script_engine_contract(ROM, UPSTREAM)
    mutated_static = deepcopy(static)
    site = next(
        command
        for source in mutated_static["entityPopulationCommandFacts"]["sourceSites"]
        if source["programId"] == "cs_5249E"
        for command in source["commands"]
        if command["macro"] == "loadEntitiesFromMapSetup"
    )
    site["sourceOrderKey"] = "cs_5249E:4:loadEntitiesFromMapSetup"
    monkeypatch.setattr(
        entity_population_reload,
        "build_map_script_engine_contract",
        lambda *_: mutated_static,
    )
    with pytest.raises(ValueError, match="source-site .*identity drift"):
        entity_population_reload.build_entity_population_reload_contract(ROM, UPSTREAM)

    monkeypatch.setattr(
        entity_population_reload,
        "build_map_script_engine_contract",
        build_map_script_engine_contract,
    )
    mutated_init = deepcopy(entity_population_reload.build_map_init_contract(ROM, UPSTREAM))
    owner = next(
        row
        for row in mutated_init["primarySourceBodies"]
        if row["sourceOwnerSymbol"] == "ms_map17_InitFunction"
    )
    owner["operations"][2]["scriptTargetSymbol"] = "cs_5249E_renamed"
    monkeypatch.setattr(
        entity_population_reload, "build_map_init_contract", lambda *_: mutated_init
    )
    with pytest.raises(ValueError, match="map-init script join drift"):
        entity_population_reload.build_entity_population_reload_contract(ROM, UPSTREAM)


@pytest.mark.parametrize(
    ("path", "old", "new", "message"),
    [
        (
            "disasm/code/common/scripting/entity/entityfunctions_1.asm",
            "moveq   #62,d7",
            "moveq   #61,d7",
            "source use-site drift",
        ),
        (
            "disasm/code/common/scripting/entity/entityfunctions_1.asm",
            "mulu.w  #MAP_TILE_SIZE,d2",
            "mulu.w  #383,d2",
            "source use-site drift",
        ),
        (
            "disasm/code/common/scripting/entity/entityfunctions_1.asm",
            "subi.w  #ENTITY_ENEMY_INDEX_DIFFERENCE,d7",
            "subi.w  #95,d7",
            "source use-site drift",
        ),
        (
            "disasm/code/common/scripting/entity/entityfunctions_1.asm",
            "move.w  #48,d7",
            "move.w  #47,d7",
            "source use-site drift",
        ),
        (
            "build/sf2build-h1.lst",
            "00046E50 4EB9 0004 4570                             jsr     InitializeNewEntity",
            "00046E50 4E90                                       jsr     (a0)",
            "H1 instruction identity drift",
        ),
    ],
)
def test_entity_population_reload_source_and_h1_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    path: str,
    old: str,
    new: str,
    message: str,
) -> None:
    target = UPSTREAM / path
    original_read_text = Path.read_text

    def patched_read_text(self: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(self, *args, **kwargs)
        if self == target:
            changed = text.replace(old, new)
            assert changed != text
            return changed
        return text

    monkeypatch.setattr(Path, "read_text", patched_read_text)
    with pytest.raises(ValueError, match=message):
        entity_population_reload.build_entity_population_reload_contract(ROM, UPSTREAM)


def test_entity_population_reload_parsers_reject_comments_labels_and_near_misses() -> None:
    source = """\
InitializeNewEntity:
    moveq #62,d7 ; real counter
    dbf d7,loc_44586
    ; addq.w #1,d0
    addq.w #1,d0
    mulu.w #MAP_TILE_SIZE,d1
    mulu.w #MAP_TILE_SIZE,d2
    bsr.w DeclareNewEntity
; End of function InitializeNewEntity
outside:
    moveq #62,d7
"""
    assert entity_population_reload._ordered_source_use(
        source,
        "InitializeNewEntity",
        [
            "moveq #62,d7",
            "dbf d7,loc_44586",
            "addq.w #1,d0",
            "mulu.w #MAP_TILE_SIZE,d1",
            "mulu.w #MAP_TILE_SIZE,d2",
            "bsr.w DeclareNewEntity",
        ],
    )[0] == "moveq #62,d7"
    listing = """\
00000000 testHandler:
00000000 4EB8 0000 jsr.w (InitializeNewEntity).w ; real call
00000004              ; jsr.w (InitializeNewEntity).w
00000004 303C 0000 move.w #InitializeNewEntity,d0
00000008 4EB8 0000 jsr.w (InitializeNewEntityExtra).w
0000000C 4E75 rts
; End of function testHandler
"""
    assert entity_population_reload._h1_function_lines(listing, "testHandler") == [
        (0, "jsr.w(InitializeNewEntity).w"),
        (4, "move.w#InitializeNewEntity,d0"),
        (8, "jsr.w(InitializeNewEntityExtra).w"),
        (12, "rts"),
    ]


@pytest.mark.parametrize(
    ("target", "mutate", "match"),
    [
        (
            "fixture",
            lambda value: value["runtimeGolden"]["records"][0].pop("callbackEvents"),
            "required property",
        ),
        (
            "fixture",
            lambda value: value["cases"][0].__setitem__(
                "renamedMacro", value["cases"][0].pop("macro")
            ),
            "macro",
        ),
        (
            "fixture",
            lambda value: value["cases"][0].__setitem__("extraNestedField", True),
            "Additional properties",
        ),
        (
            "fixture",
            lambda value: value["cases"].reverse(),
            "was expected",
        ),
        (
            "fixture",
            lambda value: value["cases"][1]["indexListSeedRecords"][0].__setitem__(
                "offset", 63
            ),
            "greater than the maximum of 62",
        ),
        (
            "observation",
            lambda value: value["records"][0].pop("callbackEvents"),
            "required property",
        ),
        (
            "observation",
            lambda value: value["records"][0].__setitem__(
                "renamedHandler", value["records"][0].pop("handlerAddressObserved")
            ),
            "handlerAddressObserved",
        ),
        (
            "observation",
            lambda value: value["records"][0].__setitem__("extraNestedField", True),
            "Additional properties",
        ),
        (
            "observation",
            lambda value: value["records"][3]["callbackEvents"].reverse(),
            "was expected",
        ),
        (
            "observation",
            lambda value: value["records"][0]["entitySlotReadRecords"][0]["fields"].__setitem__(
                "xWord", 65536
            ),
            "greater than the maximum of 65535",
        ),
    ],
)
def test_entity_population_reload_schemas_reject_malformed_content(
    target: str, mutate: object, match: str
) -> None:
    fixture = _fixture()
    value = deepcopy(fixture if target == "fixture" else _observation(fixture))
    assert callable(mutate)
    mutate(value)
    schema = (
        entity_population_reload.FIXTURE_SCHEMA
        if target == "fixture"
        else entity_population_reload.OBSERVATION_SCHEMA
    )
    with pytest.raises(ValueError, match=match):
        validate_json(value, schema, owner="entity population mutation")


def test_entity_population_reload_instrumentation_is_slice_owned_and_span_guarded(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    original = ROM.read_bytes()
    monkeypatch.setattr(entity_population_reload, "DERIVED_ROOT", tmp_path)
    output = entity_population_reload._instrument_rom(ROM, fixture["instrumentation"])
    assert output == tmp_path / "entity-population-reload.instrumented.bin"
    assert ROM.read_bytes() == original
    assert output.read_bytes()[292114:292120] == bytes.fromhex("4EB90000FF88")

    oversized = deepcopy(fixture["instrumentation"])
    oversized["stubOriginalHex"] = "FFFFFFFFFFFFFFFF"
    with pytest.raises(ValueError, match="exceeds verified padding"):
        entity_population_reload._instrument_rom(ROM, oversized)


def test_entity_population_reload_cli_and_lua_boundary_are_one_launch() -> None:
    args = build_parser().parse_args(["h3", "entity-population-reload"])
    assert args.h3_command == "entity-population-reload"
    assert args.timeout_seconds == 180
    source = entity_population_reload.OBSERVER.read_text(encoding="utf-8")
    assert source.count("memorysavestate.savecorestate") == 1
    assert "runtimeGolden" not in source
    assert "json.write(config.outputPath" in source
    assert "for _,handler in ipairs(config.handlers)" in source
    assert "for _,callback in ipairs(registered_handler.callbacks)" in source
    assert "callbackOrdersByMacro" in source
    assert "timeout:frame-budget-exhausted" in source
    assert 'emu.getregister("M68K PC")' in source
    assert "observed_handler=handler.handlerAddress" not in source
    assert "callSiteAddressObserved=callback.callSiteAddress" not in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(entity_population_reload.OBSERVER, executable)
