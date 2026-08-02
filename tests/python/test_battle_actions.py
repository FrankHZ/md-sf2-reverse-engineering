import copy
import shutil
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2.battle_actions import (
    BATTLE_SCENE_ENGINE_SOURCE_ROOT,
    BATTLE_SCENE_MACROS_PATH,
    ENUMS_PATH,
    FIXTURE,
    FIXTURE_SCHEMA,
    H1_LIST_PATH,
    MESSAGE_MACRO_NAMES,
    SCHEMA,
    SOURCE_ROOT,
    TEXT_LINES_PATH,
    _build_battle_message_contract,
    _parse_h1_message_uses,
    _parse_message_macro_definitions,
    _parse_source_message_uses,
    _parse_write_bsc_param_definition,
    _reconcile_battle_message_contract,
    build_battle_actions_inventory,
)
from sf2tool.jsonio import load_json, validate_json

UPSTREAM = Path("local/upstream/SF2DISASM")


def _swap_first_occurrences(value: bytes, first: bytes, second: bytes) -> bytes:
    first_index = value.index(first)
    second_index = value.index(second, first_index + len(first))
    return (
        value[:first_index]
        + second
        + value[first_index + len(first) : second_index]
        + first
        + value[second_index + len(second) :]
    )


def _copy_message_sources(tmp_path: Path) -> tuple[Path, Path]:
    disasm = tmp_path / "disasm"
    for relative in (BATTLE_SCENE_MACROS_PATH, ENUMS_PATH, TEXT_LINES_PATH):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(UPSTREAM / "disasm" / relative, destination)
    source_root = UPSTREAM / "disasm" / SOURCE_ROOT
    for source in source_root.glob("*.asm"):
        destination = disasm / SOURCE_ROOT / source.name
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
    scene_source_root = UPSTREAM / "disasm" / BATTLE_SCENE_ENGINE_SOURCE_ROOT
    for source in scene_source_root.glob("*.asm"):
        destination = disasm / BATTLE_SCENE_ENGINE_SOURCE_ROOT / source.name
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
    listing = tmp_path / H1_LIST_PATH
    listing.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(UPSTREAM / H1_LIST_PATH, listing)
    return disasm, tmp_path


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_contract_matches_complete_fixture_and_corpus() -> None:
    output = build_battle_actions_inventory(UPSTREAM)
    expected = load_json(FIXTURE)["expected"]["battleMessageContract"]

    assert output["battleMessageContract"] == expected
    contract = output["battleMessageContract"]
    assert contract["summary"] == {
        "macroDefinitionCount": 2,
        "writeBscParamDefinitionCount": 1,
        "completeSourceFileCount": 29,
        "positiveSourceFileCount": 11,
        "zeroSourceFileCount": 18,
        "siteCount": 54,
        "modeCounts": {"displayMessage": 49, "displayMessageWithNoWait": 5},
        "messageOperandKindCounts": {
            "immediate-message-enum": 43,
            "dynamic-expression": 11,
        },
        "distinctImmediateMessageSymbolCount": 41,
        "distinctImmediateMessageIdCount": 41,
        "callerTotalCount": 35,
        "h1BoundSiteCount": 54,
    }
    assert [row["siteCount"] for row in contract["fileTotals"]].count(0) == 18
    assert [row["h1ExpansionAddress"] for row in contract["messageSites"]] == sorted(
        row["h1ExpansionAddress"] for row in contract["messageSites"]
    )
    assert contract["messageSites"][0]["operandExpressions"] == [
        "#MESSAGE_BATTLE_IS_CURSED_AND_STUNNED",
        "(a4)",
        "#0",
        "#0",
    ]
    assert contract["messageSites"][-1]["messageOperand"]["lineId"] == 395
    assert contract["immediateMessages"][-1] == {
        "messageSymbol": "MESSAGE_BATTLE_RECEIVED_ITEM",
        "lineId": 395,
        "sourceUseCount": 1,
    }
    assert contract["battleSceneDispatcher"] == {
        "contractId": "sf2-battle-scene-engine-static-v1",
        "upstreamCommit": output["upstream"]["commit"],
        "sourcePath": "code/gameflow/battle/battlescenes/battlesceneengine_0.asm",
        "macroDispatches": [
            {
                "sourceMacro": "displayMessage",
                "commandWord": 0x10,
                "handler": "bsc10_displayMessage",
            },
            {
                "sourceMacro": "displayMessageWithNoWait",
                "commandWord": 0x11,
                "handler": "bsc11_displayMessageWithNoWait",
            },
        ],
    }

    reordered = copy.deepcopy(contract)
    reordered["messageSites"].reverse()
    with pytest.raises(ValueError, match="physical site order drift"):
        _reconcile_battle_message_contract(reordered)

    stale_file_total = copy.deepcopy(contract)
    stale_file_total["fileTotals"][0]["siteCount"] += 1
    with pytest.raises(ValueError, match="file totals do not reconcile"):
        _reconcile_battle_message_contract(stale_file_total)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_macro_parser_guards_definition_and_write_condition(
    tmp_path: Path,
) -> None:
    disasm = tmp_path / "disasm"
    destination = disasm / BATTLE_SCENE_MACROS_PATH
    destination.parent.mkdir(parents=True, exist_ok=True)
    original = (UPSTREAM / "disasm" / BATTLE_SCENE_MACROS_PATH).read_bytes()
    destination.write_bytes(original)

    definitions = _parse_message_macro_definitions(disasm)
    assert [row["sourceMacro"] for row in definitions] == list(MESSAGE_MACRO_NAMES)
    assert [row["commandWord"] for row in definitions] == [0x10, 0x11]
    assert definitions[0]["runtimeOutputWordCount"] == 6
    assert definitions[0]["runtimeOutputByteCount"] == 12
    assert _parse_write_bsc_param_definition(disasm)["runtimeOutputByteCount"] == 2

    destination.write_bytes(original.replace(b"move.w  #$10,(a6)+", b"move.b  #$10,(a6)+", 1))
    with pytest.raises(ValueError, match="message macro command emission drift"):
        _parse_message_macro_definitions(disasm)

    destination.write_bytes(
        original.replace(
            b"displayMessage: macro\r\n"
            b"    move.w  #$10,(a6)+\r\n"
            b"    writeBscParam \\1\r\n"
            b"    writeBscParam \\2\r\n"
            b"    writeBscParam \\3",
            b"displayMessage: macro\r\n"
            b"    move.w  #$10,(a6)+\r\n"
            b"    writeBscParam \\1\r\n"
            b"    writeBscParam \\3\r\n"
            b"    writeBscParam \\2",
            1,
        )
    )
    with pytest.raises(ValueError, match="message macro emission drift"):
        _parse_message_macro_definitions(disasm)

    destination.write_bytes(
        original.replace(b"displayMessageWithNoWait: macro", b"displayMessageNoWait: macro", 1)
    )
    with pytest.raises(ValueError, match="macro definition is missing"):
        _parse_message_macro_definitions(disasm)

    destination.write_bytes(original.replace(b"instr('\\1','(a')=0", b"instr('\\1','(d')=0", 1))
    with pytest.raises(ValueError, match="writeBscParam conditional emission drift"):
        _parse_write_bsc_param_definition(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_source_parser_ignores_comments_near_misses_and_rejects_arity(
    tmp_path: Path,
) -> None:
    disasm, _ = _copy_message_sources(tmp_path)
    baseline = _parse_source_message_uses(disasm)
    path = disasm / SOURCE_ROOT / "battleactionsengine_1.asm"
    original = path.read_bytes()
    path.write_bytes(
        original
        + b"\r\n; displayMessage #MESSAGE_BATTLE_DODGE,d0,#0,#0\r\n"
        + b"displayMessageNear #MESSAGE_BATTLE_DODGE,d0,#0,#0\r\n"
    )
    assert _parse_source_message_uses(disasm) == baseline

    path.write_bytes(
        original.replace(
            b"displayMessage #MESSAGE_BATTLE_IS_CURSED_AND_STUNNED,(a4),#0,#0",
            b"displayMessage #MESSAGE_BATTLE_IS_CURSED_AND_STUNNED,(a4),#0",
            1,
        )
    )
    with pytest.raises(ValueError, match="requires four operands"):
        _parse_source_message_uses(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_h1_and_immediate_enum_guards_fail_before_fixture(
    tmp_path: Path,
) -> None:
    disasm, upstream = _copy_message_sources(tmp_path)
    baseline = _build_battle_message_contract(disasm, upstream)
    assert baseline["summary"]["h1BoundSiteCount"] == 54
    command_words = {
        row["sourceMacro"]: row["commandWord"]
        for row in _parse_message_macro_definitions(disasm)
    }

    listing = upstream / H1_LIST_PATH
    original_listing = listing.read_bytes()
    listing.write_bytes(
        original_listing
        + b"\r\n; 00000000 displayMessage #MESSAGE_BATTLE_DODGE,d0,#0,#0\r\n"
        + b"00000000 displayMessageNear #MESSAGE_BATTLE_DODGE,d0,#0,#0\r\n"
    )
    assert _parse_h1_message_uses(upstream, command_words) == _parse_h1_message_uses(
        UPSTREAM, command_words
    )
    listing.write_bytes(original_listing)
    listing.write_bytes(
        original_listing.replace(b"3CFC 0010                M", b"3CFC 0012                M", 1)
    )
    with pytest.raises(ValueError, match="H1 command word drift"):
        _parse_h1_message_uses(upstream, command_words)
    listing.write_bytes(original_listing)

    address_register_instruction = b"M \tmove.b\t#0,(a6)+"
    assert address_register_instruction in original_listing
    listing.write_bytes(
        original_listing.replace(address_register_instruction, b"M \tmove.w\t#0,(a6)+", 1)
    )
    with pytest.raises(ValueError, match="writeBscParam branch emission drift"):
        _parse_h1_message_uses(upstream, command_words)
    listing.write_bytes(original_listing)

    address_register_parameter = b"M \tmove.b\t(a4),(a6)+\r\n"
    assert address_register_parameter in original_listing
    listing.write_bytes(original_listing.replace(address_register_parameter, b"", 1))
    with pytest.raises(ValueError, match="H1 instruction count drift"):
        _parse_h1_message_uses(upstream, command_words)
    listing.write_bytes(original_listing)

    macros = disasm / BATTLE_SCENE_MACROS_PATH
    original_macros = macros.read_bytes()
    macros.write_bytes(original_macros.replace(b"move.w  #$10,(a6)+", b"move.w  #$12,(a6)+", 1))
    with pytest.raises(ValueError, match="command word does not match dispatcher slot"):
        _build_battle_message_contract(disasm, upstream)
    macros.write_bytes(original_macros)

    dispatcher = disasm / BATTLE_SCENE_ENGINE_SOURCE_ROOT / "battlesceneengine_0.asm"
    original_dispatcher = dispatcher.read_bytes()
    dispatcher.write_bytes(
        _swap_first_occurrences(
            original_dispatcher,
            b"dc.w bsc10_displayMessage-rjt_BattlesceneScriptCommands\r\n",
            b"dc.w bsc11_displayMessageWithNoWait-rjt_BattlesceneScriptCommands\r\n",
        )
    )
    with pytest.raises(ValueError, match="command word does not match dispatcher slot"):
        _build_battle_message_contract(disasm, upstream)
    dispatcher.write_bytes(original_dispatcher)

    dispatcher.write_bytes(
        original_dispatcher.replace(
            b"bsc10_displayMessage-rjt_BattlesceneScriptCommands",
            b"bsc10_displayMessageBroken-rjt_BattlesceneScriptCommands",
            1,
        )
    )
    with pytest.raises(ValueError, match="dispatcher handler lookup drift"):
        _build_battle_message_contract(disasm, upstream)
    dispatcher.write_bytes(original_dispatcher)

    source = disasm / SOURCE_ROOT / "battleactionsengine_1.asm"
    original_source = source.read_bytes()
    source.write_bytes(
        original_source.replace(
            b"#MESSAGE_BATTLE_IS_CURSED_AND_STUNNED",
            b"#MESSAGE_BATTLE_NOT_A_REAL_SYMBOL",
            1,
        )
    )
    listing.write_bytes(
        original_listing.replace(
            b"#MESSAGE_BATTLE_IS_CURSED_AND_STUNNED",
            b"#MESSAGE_BATTLE_NOT_A_REAL_SYMBOL",
            1,
        ).replace(
            b"#message_battle_is_cursed_and_stunned",
            b"#message_battle_not_a_real_symbol",
        )
    )
    with pytest.raises(ValueError, match="immediate enum is unresolved"):
        _build_battle_message_contract(disasm, upstream)
    source.write_bytes(original_source)
    listing.write_bytes(original_listing)

    enums = disasm / ENUMS_PATH
    original_enums = enums.read_bytes()
    enums.write_bytes(
        original_enums.replace(
            b"MESSAGE_BATTLE_IS_CURSED_AND_STUNNED: equ 359",
            b"MESSAGE_BATTLE_IS_CURSED_AND_STUNNED: equ 9999",
            1,
        )
    )
    with pytest.raises(ValueError, match="outside text line domain"):
        _build_battle_message_contract(disasm, upstream)

    enums.write_bytes(original_enums)
    source.write_bytes(
        _swap_first_occurrences(
            original_source,
            b"displayMessage #MESSAGE_BATTLE_IS_CURSED_AND_STUNNED,(a4),#0,#0",
            b"displayMessage #MESSAGE_BATTLE_IS_STUNNED_AND_CANNOT_MOVE,(a4),#0,#0",
        )
    )
    with pytest.raises(ValueError, match="source/H1 macro identity drift"):
        _build_battle_message_contract(disasm, upstream)

    source.write_bytes(
        original_source.replace(
            b"displayMessage #MESSAGE_BATTLE_IS_CURSED_AND_STUNNED",
            b"displayMessageWithNoWait #MESSAGE_BATTLE_IS_CURSED_AND_STUNNED",
            1,
        )
    )
    with pytest.raises(ValueError, match="source/H1 macro identity drift"):
        _build_battle_message_contract(disasm, upstream)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_schemas_recursively_close_compact_contract_mutations() -> None:
    fixture = load_json(FIXTURE)
    output = build_battle_actions_inventory(UPSTREAM)
    validate_json(fixture, FIXTURE_SCHEMA, owner="battle actions fixture")
    validate_json(output, SCHEMA, owner="battle actions output")

    def contract(value: dict[str, Any], *, fixture_value: bool) -> dict[str, Any]:
        if fixture_value:
            return value["expected"]["battleMessageContract"]
        return value["battleMessageContract"]

    def missing(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][0].pop("sourceMacro")

    def renamed(value: dict[str, Any], *, fixture_value: bool) -> None:
        site = contract(value, fixture_value=fixture_value)["messageSites"][0]
        site["renamedSourceMacro"] = site.pop("sourceMacro")

    def extra(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][0]["unexpected"] = True

    def order(value: dict[str, Any], *, fixture_value: bool) -> None:
        sites = contract(value, fixture_value=fixture_value)["messageSites"]
        sites[0], sites[1] = sites[1], sites[0]

    def boundary(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][0]["messageOperand"][
            "lineId"
        ] = 4267

    def wrong_site_macro(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][0][
            "sourceMacro"
        ] = "displayMessageWithNoWait"

    def wrong_operand(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][0][
            "operandExpressions"
        ][0] = "#MESSAGE_BATTLE_DODGE"

    def wrong_message_id(value: dict[str, Any], *, fixture_value: bool) -> None:
        message_sites = contract(value, fixture_value=fixture_value)["messageSites"]
        message_sites[0]["messageOperand"] = copy.deepcopy(message_sites[1]["messageOperand"])

    def wrong_h1_address(value: dict[str, Any], *, fixture_value: bool) -> None:
        message_sites = contract(value, fixture_value=fixture_value)["messageSites"]
        message_sites[0]["h1ExpansionAddress"] = message_sites[1]["h1ExpansionAddress"]

    def wrong_macro_command(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["macroDefinitions"][0]["commandWord"] = 17

    def wrong_macro_emission(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["macroDefinitions"][0][
            "emissionStatementTemplates"
        ][0] = "move.w #$11,(a6)+"

    def reorder_immediate_messages(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["immediateMessages"].reverse()

    def reorder_file_totals(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["fileTotals"].reverse()

    def reorder_caller_totals(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["callerTotals"].reverse()

    mutations = (
        missing,
        renamed,
        extra,
        order,
        boundary,
        wrong_site_macro,
        wrong_operand,
        wrong_message_id,
        wrong_h1_address,
        wrong_macro_command,
        wrong_macro_emission,
        reorder_immediate_messages,
        reorder_file_totals,
        reorder_caller_totals,
    )
    for base, schema, owner, fixture_value in (
        (output, SCHEMA, "battle actions output", False),
        (fixture, FIXTURE_SCHEMA, "battle actions fixture", True),
    ):
        for mutation in mutations:
            mutated = copy.deepcopy(base)
            mutation(mutated, fixture_value=fixture_value)
            with pytest.raises(ValueError, match=f"{owner} failed schema validation"):
                validate_json(mutated, schema, owner=owner)
