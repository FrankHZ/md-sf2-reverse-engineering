import copy
import json
import shutil
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import battle_actions
from sf2tool.h2.battle_actions import (
    BATTLE_SCENE_ENGINE_SOURCE_ROOT,
    BATTLE_SCENE_MACROS_PATH,
    ENUMS_PATH,
    FIXTURE,
    FIXTURE_SCHEMA,
    H1_LIST_PATH,
    ITEM_BREAK_MESSAGES_PATH,
    MACROS_PATH,
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


def _replace_after(value: bytes, anchor: bytes, old: bytes, new: bytes) -> bytes:
    anchor_index = value.index(anchor)
    target_index = value.index(old, anchor_index + len(anchor))
    return value[:target_index] + new + value[target_index + len(old) :]


def _copy_message_sources(tmp_path: Path) -> tuple[Path, Path]:
    disasm = tmp_path / "disasm"
    for relative in (
        BATTLE_SCENE_MACROS_PATH,
        ENUMS_PATH,
        TEXT_LINES_PATH,
        ITEM_BREAK_MESSAGES_PATH,
        MACROS_PATH,
    ):
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
        "dynamicResolutionCount": 11,
        "unresolvedDynamicSiteCount": 0,
        "dynamicCandidateCount": 56,
        "distinctDynamicCandidateMessageIdCount": 56,
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
    dynamic_sites = [
        site
        for site in contract["messageSites"]
        if site["messageOperand"]["kind"] == "dynamic-expression"
    ]
    assert [
        (
            site["sourcePath"],
            site["sourceLine"],
            site["messageOperand"]["staticResolution"]["resolver"],
            len(site["messageOperand"]["staticResolution"]["candidateMessages"]),
        )
        for site in dynamic_sites
    ] == [
        (
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
            34,
            "attack-type-chain",
            3,
        ),
        (
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
            83,
            "spell-selector-chain",
            9,
        ),
        (
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
            116,
            "muddled-message-offset",
            10,
        ),
        (
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
            134,
            "prism-enemy-branch",
            2,
        ),
        ("code/gameflow/battle/battleactions/inflictdamage.asm", 153, "damage-branch-chain", 5),
        ("code/gameflow/battle/battleactions/displaydeathmessage.asm", 20, "death-side-branch", 2),
        (
            "code/gameflow/battle/battleactions/castspell.asm",
            422,
            "muddle-spell-shared-assignment",
            1,
        ),
        ("code/gameflow/battle/battleactions/castspell.asm", 482, "desoul-side-branch", 2),
        ("code/gameflow/battle/battleactions/castspell.asm", 572, "absorb-side-branch", 2),
        ("code/gameflow/battle/battleactions/breakuseditem.asm", 32, "item-break-message", 10),
        ("code/gameflow/battle/battleactions/breakuseditem.asm", 44, "item-break-message", 10),
    ]
    assert [
        candidate["lineId"]
        for candidate in dynamic_sites[2]["messageOperand"]["staticResolution"]["candidateMessages"]
    ] == list(range(322, 332))
    assert dynamic_sites[1]["messageOperand"]["staticResolution"]["controlFacts"] == [
        {
            "kind": "selector-register",
            "symbol": "BATTLESCENE_SPELL_INDEX",
            "value": None,
            "branchMnemonic": None,
            "branchTarget": None,
            "messageSymbol": None,
            "messageLineId": None,
        },
        *[
            {
                "kind": "selector-branch", "symbol": symbol, "value": value,
                "branchMnemonic": "beq", "branchTarget": "@Message_CastSpell",
                "messageSymbol": message_symbol, "messageLineId": message_line_id,
            }
            for symbol, value, message_symbol, message_line_id in (
                ("SPELL_SPOIT", 15, "MESSAGE_SPELLCAST_PUT_ON_A_DEMON_SMILE", 310),
                ("SPELL_FLAME", 17, "MESSAGE_SPELLCAST_BELCHED_OUT_FLAMES", 278),
                ("SPELL_KIWI", 41, "MESSAGE_SPELLCAST_BELCHED_OUT_FLAMES", 278),
                ("SPELL_SNOW", 18, "MESSAGE_SPELLCAST_BLEW_OUT_A_SNOWSTORM", 279),
                ("SPELL_DEMON", 19, "MESSAGE_SPELLCAST_CAST_DEMON_BREATH", 276),
                ("SPELL_ODDEYE", 43, "MESSAGE_SPELLCAST_ODD_EYE_BEAM", 320),
                ("SPELL_DAO", 29, "MESSAGE_SPELLCAST_SUMMONED", 283),
                ("SPELL_APOLLO", 30, "MESSAGE_SPELLCAST_SUMMONED", 283),
                ("SPELL_NEPTUN", 31, "MESSAGE_SPELLCAST_SUMMONED", 283),
                ("SPELL_ATLAS", 32, "MESSAGE_SPELLCAST_SUMMONED", 283),
            )
        ],
        {
            "kind": "selector-register",
            "symbol": "BATTLEACTION_OFFSET_ITEM_OR_SPELL",
            "value": None,
            "branchMnemonic": None,
            "branchTarget": None,
            "messageSymbol": None,
            "messageLineId": None,
        },
        {
            "kind": "selector-branch", "symbol": "SPELL_AQUA", "value": 40,
            "branchMnemonic": "beq", "branchTarget": "@Message_CastSpell",
            "messageSymbol": "MESSAGE_SPELLCAST_BLEW_OUT_AQUA_BREATH", "messageLineId": 281,
        },
        {
            "kind": "selector-branch", "symbol": "SPELL_AQUA|SPELL_LV2", "value": 104,
            "branchMnemonic": "beq", "branchTarget": "@Message_CastSpell",
            "messageSymbol": "MESSAGE_SPELLCAST_BLEW_OUT_BUBBLE_BREATH", "messageLineId": 282,
        },
        {
            "kind": "final-default-assignment", "symbol": None, "value": None,
            "branchMnemonic": None, "branchTarget": None,
            "messageSymbol": "MESSAGE_SPELLCAST_DEFAULT", "messageLineId": 274,
        },
    ]
    assert contract["itemBreakMessageResolver"]["h1ByteCount"] == 52
    assert contract["itemBreakMessageResolver"]["h1Sha256"] == (
        "037C7F69095E47FA84808F9E596403EDA681D10766205D7B3EF86E3C36BA12B5"
    )
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
def test_battle_message_dynamic_resolver_source_grammar_guards_fail_before_fixture(
    tmp_path: Path,
) -> None:
    disasm, upstream = _copy_message_sources(tmp_path)
    baseline = _build_battle_message_contract(disasm, upstream)
    assert baseline["summary"]["unresolvedDynamicSiteCount"] == 0

    create = disasm / SOURCE_ROOT / "createbattlescenemessage.asm"
    original_create = create.read_bytes()
    create.write_bytes(
        original_create.replace(
            b"move.w  #MESSAGE_BATTLE_ATTACK,d1",
            b"move.w  #MESSAGE_BATTLE_DODGE,d1",
            1,
        )
    )
    with pytest.raises(ValueError, match="attack message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(b"bls.s   @Message_Muddled", b"bhi.s   @Message_Muddled", 1)
    )
    with pytest.raises(ValueError, match="muddled message offset source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(
            b"beq.w   @Message_Attack ", b"beq.w   @Message_CastSpell ", 1
        )
    )
    with pytest.raises(ValueError, match="attack message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        _swap_first_occurrences(
            original_create,
            b"move.w  #MESSAGE_BATTLE_SECOND_ATTACK,d1",
            b"move.w  #MESSAGE_BATTLE_COUNTER_ATTACK,d1",
        )
    )
    with pytest.raises(ValueError, match="attack message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(
            b"move.w  BATTLEACTION_OFFSET_ITEM_OR_SPELL(a3),d2",
            b"move.w  ((BATTLESCENE_SPELL_INDEX-$1000000)).w,d2",
            1,
        )
    )
    with pytest.raises(ValueError, match="cast message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(b"cmpi.w  #9,d0", b"cmpi.w  #8,d0", 1)
    )
    with pytest.raises(ValueError, match="muddled message offset source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(b"add.w   d0,d1", b"add.w   d2,d1", 1)
    )
    with pytest.raises(ValueError, match="muddled message offset source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(
        original_create.replace(
            b"@Message_Attack:\r\n",
            b"move.w  #MESSAGE_BATTLE_DODGE,d1\r\n@Message_Attack:\r\n",
            1,
        )
    )
    with pytest.raises(ValueError, match="attack message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    create.write_bytes(original_create)

    damage = disasm / SOURCE_ROOT / "inflictdamage.asm"
    original_damage = damage.read_bytes()
    damage.write_bytes(
        original_damage.replace(b"bra.s   @Goto_CutoffMessage", b"bra.s   @CutoffMessage", 1)
    )
    with pytest.raises(ValueError, match="damage message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    damage.write_bytes(
        original_damage.replace(b"bra.s   @CutoffMessage", b"bra.s   @Goto_CutoffMessage", 1)
    )
    with pytest.raises(ValueError, match="damage message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    damage.write_bytes(original_damage)

    death = disasm / SOURCE_ROOT / "displaydeathmessage.asm"
    original_death = death.read_bytes()
    death.write_bytes(
        original_death.replace(
            b"bra.s   @WriteBattleMessageCommand", b"bra.s   @Enemy", 1
        )
    )
    with pytest.raises(ValueError, match="death message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    death.write_bytes(original_death)

    cast = disasm / SOURCE_ROOT / "castspell.asm"
    original_cast = cast.read_bytes()
    cast.write_bytes(
        _replace_after(
            original_cast,
            b"spellEffect_Muddle:",
            b"@BattleMessage:\r\n",
            b"move.w  #MESSAGE_BATTLE_FELL_ASLEEP,d2\r\n@BattleMessage:\r\n",
        )
    )
    with pytest.raises(ValueError, match="muddle spell message source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(
        _replace_after(
            original_cast,
            b"@DetermineBattleMessage:",
            b"bne.s   @EnemyMessage",
            b"bne.s   byte_B53C",
        )
    )
    with pytest.raises(ValueError, match="desoul message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(
        original_cast.replace(b"bra.s   byte_B562", b"bra.s   @EnemyMessage", 1)
    )
    with pytest.raises(ValueError, match="desoul message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(
        _replace_after(
            original_cast,
            b"@DetermineMessage:",
            b"bne.s   @EnemyMessage",
            b"bne.s   byte_B642",
        )
    )
    with pytest.raises(ValueError, match="absorb message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(
        original_cast.replace(b"bra.s   byte_B66C", b"bra.s   @EnemyMessage", 1)
    )
    with pytest.raises(ValueError, match="absorb message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(
        _replace_after(
            original_cast,
            b"@DetermineMessage:",
            b"move.b  (a5),d0",
            b"move.b  (a4),d0",
        )
    )
    with pytest.raises(ValueError, match="absorb message selector source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    cast.write_bytes(original_cast)

    break_source = disasm / SOURCE_ROOT / "breakuseditem.asm"
    original_break_source = break_source.read_bytes()
    break_source.write_bytes(
        original_break_source.replace(b"moveq   #0,d0", b"moveq   #2,d0", 1)
    )
    with pytest.raises(ValueError, match="item-break caller break mode source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(
        original_break_source.replace(b"tst.b   dodge(a2)", b"tst.b   cutoff(a2)", 1)
    )
    with pytest.raises(ValueError, match="item-break helper source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(
        original_break_source.replace(b"bra.s   @Goto_FindItem", b"bra.s   @FindItem", 1)
    )
    with pytest.raises(ValueError, match="item-break helper source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(
        original_break_source.replace(b"bra.s   @FindItem_Loop", b"bra.s   @Done", 1)
    )
    with pytest.raises(ValueError, match="item-break helper source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(
        original_break_source.replace(
            b"andi.w  #ITEMENTRY_MASK_INDEX,d0", b"andi.w  #$7E,d0", 1
        )
    )
    with pytest.raises(ValueError, match="item-break helper source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(
        original_break_source.replace(b"add.w   d0,d3", b"add.w   d1,d3", 1)
    )
    with pytest.raises(ValueError, match="item-break helper source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    break_source.write_bytes(original_break_source)

    table = disasm / ITEM_BREAK_MESSAGES_PATH
    original_table = table.read_bytes()
    table.write_bytes(
        original_table.replace(
            b"itemBreakMessage POWER_RING, PIECES",
            b"itemBreakMessage POWER_RING, FLAMES",
            1,
        )
    )
    with pytest.raises(ValueError, match="item-break table H1 byte parity drift"):
        _build_battle_message_contract(disasm, upstream)
    table.write_bytes(original_table.replace(b"tableEnd.w", b"tableEnd.b", 1))
    with pytest.raises(ValueError, match="item-break table source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    table.write_bytes(
        original_table
        + b"\r\n; itemBreakMessage POWER_RING, FLAMES\r\n"
    )
    assert _build_battle_message_contract(disasm, upstream) == baseline
    table.write_bytes(
        original_table.replace(b"tableEnd.w", b"dc.b    $00\r\n                tableEnd.w", 1)
    )
    with pytest.raises(ValueError, match="item-break table source grammar drift"):
        _build_battle_message_contract(disasm, upstream)
    table.write_bytes(original_table)

    listing = upstream / H1_LIST_PATH
    original_listing = listing.read_bytes()
    listing.write_bytes(
        original_listing.replace(
            b"0000BCF0 13                       M",
            b"0000BCF0 12                       M",
            1,
        )
    )
    with pytest.raises(ValueError, match="item-break table H1 byte parity drift"):
        _build_battle_message_contract(disasm, upstream)
    listing.write_bytes(original_listing)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_message_item_auxiliary_owner_join_is_verifier_only_and_exact(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    output = build_battle_actions_inventory(UPSTREAM)
    owner_source = battle_actions.ITEM_AUXILIARY_FIXTURE
    monkeypatch.setattr(
        battle_actions,
        "ITEM_AUXILIARY_FIXTURE",
        tmp_path / "missing-item-auxiliary-fixture.json",
    )
    assert build_battle_actions_inventory(UPSTREAM) == output

    def assert_owner_rejected(
        suffix: str,
        mutate: Any,
        expected_error: str,
    ) -> None:
        owner_path = tmp_path / f"item-auxiliary-{suffix}.json"
        owner = load_json(owner_source)
        mutate(owner)
        owner_path.write_text(json.dumps(owner, indent=2) + "\n", encoding="utf-8")
        validate_json(
            owner,
            battle_actions.ITEM_AUXILIARY_FIXTURE_SCHEMA,
            owner=f"temporary owner fixture {suffix}",
        )
        monkeypatch.setattr(battle_actions, "ITEM_AUXILIARY_FIXTURE", owner_path)
        output_path = tmp_path / f"battle-actions-{suffix}.json"
        with pytest.raises(ValueError, match=expected_error):
            battle_actions.verify_battle_actions_inventory(
                UPSTREAM,
                output_path=output_path,
            )
        assert not output_path.exists()

    assert_owner_rejected(
        "commit",
        lambda owner: owner.__setitem__("upstreamCommit", "0" * 40),
        "item-break owner provenance drift",
    )
    assert_owner_rejected(
        "rom-sha256",
        lambda owner: owner.__setitem__("romSha256", "0" * 64),
        "item-break owner ROM provenance drift",
    )
    assert_owner_rejected(
        "address",
        lambda owner: owner["table"].__setitem__("table_ItemBreakMessages", 48369),
        "item-break owner table address drift",
    )
    assert_owner_rejected(
        "count",
        lambda owner: owner["summary"].__setitem__("breakMessageCount", 24),
        "item-break owner row count drift",
    )
    assert_owner_rejected(
        "rule",
        lambda owner: owner["consumerRules"].__setitem__(
            "breakMessages", "matched item byte replaces the selected base message"
        ),
        "item-break owner consumer rule drift",
    )

    canonical_rom_sha256 = load_json(battle_actions.ROM_MANIFEST)["hashes"]["sha256"]
    owner = load_json(owner_source)
    owner["romSha256"] = "0" * 64
    owner_path = tmp_path / "item-auxiliary-owner-matches-fixture-only.json"
    owner_path.write_text(json.dumps(owner, indent=2) + "\n", encoding="utf-8")
    monkeypatch.setattr(battle_actions, "ITEM_AUXILIARY_FIXTURE", owner_path)
    with pytest.raises(ValueError, match="item-break owner ROM provenance drift"):
        battle_actions._verify_item_break_auxiliary_owner(
            output,
            fixture_rom_sha256="0" * 64,
            canonical_rom_sha256=canonical_rom_sha256,
        )

    monkeypatch.setattr(battle_actions, "ITEM_AUXILIARY_FIXTURE", owner_source)
    with pytest.raises(ValueError, match="item-break owner ROM provenance drift"):
        battle_actions._verify_item_break_auxiliary_owner(
            output,
            fixture_rom_sha256="0" * 64,
            canonical_rom_sha256=canonical_rom_sha256,
        )


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

    def missing_dynamic_resolution(value: dict[str, Any], *, fixture_value: bool) -> None:
        dynamic_operand = contract(value, fixture_value=fixture_value)["messageSites"][3][
            "messageOperand"
        ]
        dynamic_operand.pop("staticResolution")

    def extra_dynamic_control_field(value: dict[str, Any], *, fixture_value: bool) -> None:
        control = contract(value, fixture_value=fixture_value)["messageSites"][3][
            "messageOperand"
        ]["staticResolution"]["controlFacts"][0]
        control["unexpected"] = True

    def wrong_dynamic_candidate(value: dict[str, Any], *, fixture_value: bool) -> None:
        candidates = contract(value, fixture_value=fixture_value)["messageSites"][3][
            "messageOperand"
        ]["staticResolution"]["candidateMessages"]
        candidates[0]["lineId"] = 274

    def reorder_dynamic_candidates(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["messageSites"][3]["messageOperand"][
            "staticResolution"
        ]["candidateMessages"].reverse()

    def wrong_item_break_offset(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["itemBreakMessageResolver"]["tableRows"][
            0
        ]["messageOffset"] = 0

    def renamed_item_break_field(value: dict[str, Any], *, fixture_value: bool) -> None:
        resolver = contract(value, fixture_value=fixture_value)["itemBreakMessageResolver"]
        resolver["renamedTableRows"] = resolver.pop("tableRows")

    def wrong_item_break_h1_hash(value: dict[str, Any], *, fixture_value: bool) -> None:
        resolver = contract(value, fixture_value=fixture_value)["itemBreakMessageResolver"]
        resolver["h1Sha256"] = "0" * 64

    def wrong_dynamic_summary(value: dict[str, Any], *, fixture_value: bool) -> None:
        contract(value, fixture_value=fixture_value)["summary"]["unresolvedDynamicSiteCount"] = 1

    def wrong_dynamic_selector(value: dict[str, Any], *, fixture_value: bool) -> None:
        controls = contract(value, fixture_value=fixture_value)["messageSites"][4][
            "messageOperand"
        ]["staticResolution"]["controlFacts"]
        controls[1]["symbol"] = "COMBATANT_BIT_ALLY"

    def wrong_dynamic_branch(value: dict[str, Any], *, fixture_value: bool) -> None:
        controls = contract(value, fixture_value=fixture_value)["messageSites"][3][
            "messageOperand"
        ]["staticResolution"]["controlFacts"]
        controls[2]["branchTarget"] = "@Unexpected"

    def wrong_muddled_bound(value: dict[str, Any], *, fixture_value: bool) -> None:
        controls = contract(value, fixture_value=fixture_value)["messageSites"][7][
            "messageOperand"
        ]["staticResolution"]["controlFacts"]
        controls[1]["value"] = 8

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
        missing_dynamic_resolution,
        extra_dynamic_control_field,
        wrong_dynamic_candidate,
        reorder_dynamic_candidates,
        wrong_item_break_offset,
        renamed_item_break_field,
        wrong_item_break_h1_hash,
        wrong_dynamic_summary,
        wrong_dynamic_selector,
        wrong_dynamic_branch,
        wrong_muddled_bound,
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
