from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses

ID = "sf2-battle-cutscenes-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/cutscenes")
MANIFEST = repo_path("manifests/extractions/battle-cutscenes-static.json")
SCHEMA = repo_path("schemas/battle-cutscenes-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-cutscenes-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-cutscenes-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

_COMBATANT_ENUMS = Path("sf2enums.asm")
_LEADER_DEATH_SOURCE = Path("afterenemyleaderdeathpositions.asm")
_LEADER_DEATH_SYMBOL = "ApplyPositionsAfterEnemyLeaderDies"
_COMBATANT_STATS_2 = Path("code/common/stats/combatantstats_2.asm")
_COMBATANT_STATS_3 = Path("code/common/stats/combatantstats_3.asm")
_JUMP_INTERFACE = Path("code/common/tech/jumpinterfaces/s02_jumpinterface.asm")
_COMBATANT_CONSTANT_NAMES = (
    "COMBATANT_ALLIES_START",
    "COMBATANT_ALLIES_COUNTER",
    "COMBATANT_ENEMIES_START",
    "COMBATANT_ENEMIES_COUNTER",
    "COMBATANT_MASK_ENEMY_BIT",
    "COMBATANT_MASK_INDEX_AND_SORT_BIT",
)
_EQU_PATTERN = re.compile(r"^(?P<name>[A-Z0-9_]+):\s+equ\s+(?P<value>[^\s;]+)")
_INSTRUCTION_PATTERN = re.compile(
    r"^(?P<opcode>[A-Za-z]+(?:\.[A-Za-z]+)?)(?:\s+(?P<operands>.*))?$"
)

REPRESENTATIVE_SYMBOLS = {
    "afterbattlecutscenesend.asm": "EndAfterBattleCutscene",
    "afterbattlecutscenesstart.asm": "ExecuteAfterBattleCutscene",
    "afterenemyleaderdeathpositions.asm": "ApplyPositionsAfterEnemyLeaderDies",
    "battleendcutscenesend.asm": "loc_47C48",
    "battleendcutscenesstart.asm": "ExecuteBattleCutscene_Defeated",
    "battlestartcutscenesend.asm": "loc_47B8C",
    "battlestartcutscenesstart.asm": "ExecuteBattleStartCutscene",
    "beforebattlecutscenesend.asm": "loc_47AE8",
    "beforebattlecutscenesstart.asm": "ExecuteBeforeBattleCutscene",
    "regionactivatedcutscenes.asm": "ExecuteBattleRegionCutscene",
}

FUNCTION_SYMBOLS = {
    "afterBattleEndAddress": "EndAfterBattleCutscene",
    "afterBattleStartAddress": "ExecuteAfterBattleCutscene",
    "leaderDeathPositionsAddress": "ApplyPositionsAfterEnemyLeaderDies",
    "battleEndAddress": "loc_47C48",
    "battleEndStartAddress": "ExecuteBattleCutscene_Defeated",
    "battleStartEndAddress": "loc_47B8C",
    "battleStartAddress": "ExecuteBattleStartCutscene",
    "beforeBattleEndAddress": "loc_47AE8",
    "beforeBattleAddress": "ExecuteBeforeBattleCutscene",
    "regionCutsceneAddress": "ExecuteBattleRegionCutscene",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_equ_value(value: str) -> int:
    if value.startswith("$"):
        return int(value[1:], 16)
    return int(value, 10)


def _combatant_constants(disasm: Path) -> dict[str, int]:
    constants: dict[str, int] = {}
    for raw_line in (disasm / _COMBATANT_ENUMS).read_text(encoding="utf-8").splitlines():
        line = raw_line.split(";", maxsplit=1)[0].strip()
        match = _EQU_PATTERN.fullmatch(line)
        if match is not None and match["name"] in _COMBATANT_CONSTANT_NAMES:
            constants[match["name"]] = _parse_equ_value(match["value"])
    missing = set(_COMBATANT_CONSTANT_NAMES) - set(constants)
    if missing:
        raise ValueError(f"battle cutscene combatant constants missing: {sorted(missing)}")
    return constants


def _listener_tokens(source: str) -> list[tuple[str, str]]:
    tokens: list[tuple[str, str]] = []
    for raw_line in source.splitlines():
        line = raw_line.split(";", maxsplit=1)[0].strip()
        if not line:
            continue
        if line.endswith(":"):
            tokens.append(("label", line[:-1]))
            continue
        match = _INSTRUCTION_PATTERN.fullmatch(line)
        if match is None:
            raise ValueError(f"battle cutscene listener instruction parse drift: {line}")
        tokens.append((match["opcode"].lower(), (match["operands"] or "").strip()))
    return tokens


def _function_tokens(tokens: list[tuple[str, str]], symbol: str) -> list[tuple[str, str]]:
    try:
        start = tokens.index(("label", symbol))
    except ValueError as error:
        raise ValueError(f"battle cutscene listener missing {symbol}") from error
    for end in range(start + 1, len(tokens)):
        if tokens[end] == ("rts", ""):
            return tokens[start : end + 1]
    raise ValueError(f"battle cutscene listener {symbol} is missing RTS")


def _require_token_sequence(
    tokens: list[tuple[str, str]], expected: list[tuple[str, str]], *, name: str
) -> int:
    width = len(expected)
    for start in range(len(tokens) - width + 1):
        if tokens[start : start + width] == expected:
            return start
    raise ValueError(f"battle cutscene {name} chronology drift")


def _literal_operand(operands: str, *, opcode: str, register: str) -> int:
    prefix = "#"
    suffix = f",{register}"
    if not operands.startswith(prefix) or not operands.endswith(suffix):
        raise ValueError(f"battle cutscene {opcode} operand drift: {operands}")
    return _parse_equ_value(operands[len(prefix) : -len(suffix)])


def _routine_tokens(disasm: Path, path: Path, symbol: str) -> list[tuple[str, str]]:
    source = (disasm / path).read_text(encoding="utf-8")
    return _function_tokens(_listener_tokens(source), symbol)


def _require_leader_death_writer_dataflow(disasm: Path) -> None:
    jump_tokens = _listener_tokens((disasm / _JUMP_INTERFACE).read_text(encoding="utf-8"))
    for interface_symbol, target in (
        ("j_SetCombatantX", "SetCombatantX"),
        ("j_SetCurrentHp", "SetCurrentHp"),
    ):
        _require_token_sequence(
            jump_tokens,
            [("label", interface_symbol), ("jmp", f"{target}(pc)")],
            name=f"{interface_symbol} effective target",
        )

    for symbol, offset, leaf in (
        ("SetCombatantX", "COMBATANT_OFFSET_X", "SetCombatantByte"),
        ("SetCurrentHp", "COMBATANT_OFFSET_HP_CURRENT", "SetCombatantWord"),
    ):
        wrapper = _routine_tokens(disasm, _COMBATANT_STATS_2, symbol)
        expected = [
            ("label", symbol),
            ("movem.l", "d7-a0,-(sp)"),
            ("moveq", f"#{offset},d7"),
            ("bsr.w", leaf),
            ("movem.l", "(sp)+,d7-a0"),
            ("rts", ""),
        ]
        if wrapper != expected:
            raise ValueError(f"battle cutscene {symbol} d1-preservation drift")

    for symbol, width in (("SetCombatantByte", "b"), ("SetCombatantWord", "w")):
        writer = _routine_tokens(disasm, _COMBATANT_STATS_3, symbol)
        expected = [
            ("label", symbol),
            ("bsr.s", "GetCombatantEntryAddress"),
            (f"move.{width}", "d1,(a0,d7.w)"),
            ("rts", ""),
        ]
        if writer != expected:
            raise ValueError(f"battle cutscene {symbol} d1 writer drift")

    entry_address = _routine_tokens(
        disasm, _COMBATANT_STATS_3, "GetCombatantEntryAddress"
    )
    expected_entry_address = [
        ("label", "GetCombatantEntryAddress"),
        ("movem.w", "d0-d1,-(sp)"),
        ("cmpi.b", "#COMBATANT_ENEMIES_START,d0"),
        ("bcc.s", "@Enemy"),
        ("cmpi.b", "#COMBATANT_ALLIES_SPACE_END_MINUS_ONE,d0"),
        ("bhi.s", "@ErrorHandling"),
        ("bra.s", "@GetAddress"),
        ("label", "@Enemy"),
        ("cmpi.b", "#COMBATANT_ENEMIES_SPACE_END,d0"),
        ("bhi.s", "@ErrorHandling"),
        ("subi.b", "#COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END,d0"),
        ("label", "@GetAddress"),
        ("andi.w", "#BYTE_MASK,d0"),
        ("lsl.w", "#3,d0"),
        ("move.w", "d0,d1"),
        ("lsl.w", "#3,d0"),
        ("sub.w", "d1,d0"),
        ("lea", "((COMBATANT_DATA-$1000000)).w,a0"),
        ("adda.w", "d0,a0"),
        ("movem.w", "(sp)+,d0-d1"),
        ("rts", ""),
    ]
    if entry_address != expected_entry_address:
        raise ValueError("battle cutscene combatant-entry d0/d1 preservation drift")


def _leader_death_position_facts(disasm: Path) -> dict[str, Any]:
    constants = _combatant_constants(disasm)
    _require_leader_death_writer_dataflow(disasm)
    source = (disasm / SOURCE_ROOT / _LEADER_DEATH_SOURCE).read_text(encoding="utf-8")
    tokens = _function_tokens(_listener_tokens(source), _LEADER_DEATH_SYMBOL)

    _require_token_sequence(
        tokens,
        [
            ("label", _LEADER_DEATH_SYMBOL),
            ("movem.l", "d0-d1/d7-a0,-(sp)"),
            ("moveq", "#ALLY_BOWIE,d0"),
            ("jsr", "j_GetCurrentHp"),
            ("tst.w", "d1"),
            ("beq.w", "@Done"),
            ("move.w", "#COMBATANT_ENEMIES_START,d0"),
            ("jsr", "j_GetCurrentHp"),
            ("tst.w", "d1"),
            ("bne.w", "@Done"),
        ],
        name="leader-death life/death gate",
    )
    battle_scan_start = _require_token_sequence(
        tokens,
        [
            ("lea", "table_AfterBattlePositions(pc), a0"),
            ("clr.w", "d1"),
            ("move.b", "((CURRENT_BATTLE-$1000000)).w,d1"),
            ("label", "@FindBattle_Loop"),
            ("cmpi.w", "#-1,(a0)"),
            ("beq.w", "@Done"),
            ("cmp.w", "(a0),d1"),
            ("beq.w", "@Found"),
            ("adda.w", "#6,a0"),
            ("bra.s", "@FindBattle_Loop"),
            ("move.w", "#$80FF,(DEAD_COMBATANTS_LIST).l"),
            ("move.w", "#1,(DEAD_COMBATANTS_LIST_LENGTH).l"),
        ],
        name="leader-death battle table scan",
    )

    loop_start = _require_token_sequence(
        tokens,
        [
            ("label", "@Found"),
            ("moveq", "#COMBATANT_ALLIES_START,d0"),
            ("moveq", "#COMBATANT_ALLIES_COUNTER,d7"),
            ("label", "@MoveAllCombatantOffscreen_Loop"),
            ("move.w", "#-1,d1"),
            ("jsr", "j_SetCombatantX"),
            ("ori.b", "#COMBATANT_MASK_ENEMY_BIT,d0"),
            ("jsr", "j_SetCombatantX"),
            ("moveq", "#0,d1"),
            ("jsr", "j_SetCurrentHp"),
            ("andi.b", "#COMBATANT_MASK_INDEX_AND_SORT_BIT,d0"),
            ("addq.w", "#1,d0"),
            ("dbf", "d7,@MoveAllCombatantOffscreen_Loop"),
        ],
        name="leader-death offscreen/HP loop",
    )
    loop_end = loop_start + 13
    tail_start = loop_end
    try:
        position_pointer_index = tokens.index(("movea.l", "2(a0),a0"), tail_start)
    except ValueError as exc:
        raise ValueError(
            "battle cutscene leader-death position-only tail chronology drift"
        ) from exc
    tail_tokens = tokens[tail_start:position_pointer_index]
    if not tail_tokens or tail_tokens[0][0] != "move.w":
        raise ValueError("battle cutscene leader-death position-only tail chronology drift")
    if any(token == ("jsr", "j_SetCurrentHp") for token in tail_tokens):
        raise ValueError("battle cutscene position-only tail contains SetCurrentHp")
    tail_slot = _literal_operand(tail_tokens[0][1], opcode="tail start", register="d0")
    tail_slots: list[int] = []
    tail_index = 1
    while tail_index < len(tail_tokens):
        if tail_tokens[tail_index] != ("jsr", "j_SetCombatantX"):
            raise ValueError("battle cutscene leader-death position-only tail chronology drift")
        tail_slots.append(tail_slot)
        tail_index += 1
        if tail_index == len(tail_tokens):
            break
        if tail_tokens[tail_index][0] != "addq.w":
            raise ValueError("battle cutscene leader-death position-only tail chronology drift")
        tail_slot += _literal_operand(
            tail_tokens[tail_index][1], opcode="tail increment", register="d0"
        )
        tail_index += 1
    if not tail_slots:
        raise ValueError("battle cutscene leader-death position-only tail chronology drift")

    hp_calls = [
        index
        for index, token in enumerate(tokens)
        if token == ("jsr", "j_SetCurrentHp")
    ]
    hp_loop_index = loop_start + 9
    if hp_calls != [hp_loop_index]:
        raise ValueError("battle cutscene HP write must be only the DBF-loop instruction")

    ally_start = constants["COMBATANT_ALLIES_START"]
    ally_counter = constants["COMBATANT_ALLIES_COUNTER"]
    enemy_start = constants["COMBATANT_ENEMIES_START"]
    enemy_counter = constants["COMBATANT_ENEMIES_COUNTER"]
    enemy_bit = constants["COMBATANT_MASK_ENEMY_BIT"]
    index_sort_mask = constants["COMBATANT_MASK_INDEX_AND_SORT_BIT"]
    loop_count = ally_counter + 1
    ally_end = ally_start + loop_count - 1
    enemy_domain_count = enemy_counter + 1
    enemy_domain_end = enemy_start + enemy_counter
    hp_enemy_start = ally_start | enemy_bit
    hp_enemy_end = ally_end | enemy_bit
    domain_tail_count = enemy_domain_count - loop_count
    observed_tail_call_count = len(tail_slots)
    if domain_tail_count < 1 or observed_tail_call_count != domain_tail_count:
        raise ValueError(
            "battle cutscene position-only tail-count/domain relation drift: "
            f"source calls {observed_tail_call_count}, enum domain tail {domain_tail_count}"
        )

    if enemy_start != hp_enemy_start:
        raise ValueError("battle cutscene enemy-bit mapping/domain relation drift")
    if index_sort_mask & enemy_bit or ally_end & ~index_sort_mask:
        raise ValueError("battle cutscene index/sort mask reset relation drift")
    if tail_slots[0] != hp_enemy_end + 1 or tail_slots[-1] != enemy_domain_end:
        raise ValueError("battle cutscene position-only tail/domain relation drift")

    loop_x = _literal_operand(tokens[loop_start + 4][1], opcode="loop X", register="d1")
    hp_value = _literal_operand(tokens[loop_start + 8][1], opcode="loop HP", register="d1")
    battle_table_entry_bytes = _literal_operand(
        tokens[battle_scan_start + 8][1], opcode="battle table entry", register="a0"
    )
    position_loop_start = _require_token_sequence(
        tokens[position_pointer_index:],
        [
            ("movea.l", "2(a0),a0"),
            ("clr.w", "d1"),
            ("label", "@FindCombatant_Loop"),
            ("cmpi.w", "#-1,(a0)"),
            ("beq.w", "@Done"),
            ("move.b", "(a0),d0"),
            ("jsr", "j_GetCurrentHp"),
            ("tst.w", "d1"),
            ("bne.s", "@ApplyPositions"),
            ("cmpi.b", "#$80,d0"),
            ("bne.w", "@NextCombatant"),
            ("label", "@ApplyPositions"),
            ("move.b", "1(a0),d1"),
            ("jsr", "j_SetCombatantX"),
            ("move.b", "2(a0),d1"),
            ("jsr", "j_SetCombatantY"),
            ("label", "@NextCombatant"),
            ("adda.w", "#4,a0"),
            ("bra.s", "@FindCombatant_Loop"),
            ("label", "@Done"),
            ("movem.l", "(sp)+,d0-d1/d7-a0"),
            ("rts", ""),
        ],
        name="leader-death position application",
    ) + position_pointer_index
    position_entry_bytes = _literal_operand(
        tokens[position_loop_start + 17][1], opcode="position entry", register="a0"
    )
    if loop_x != -1 or hp_value != 0:
        raise ValueError("battle cutscene loop X/HP input value drift")
    if battle_table_entry_bytes != 6 or position_entry_bytes != 4:
        raise ValueError("battle cutscene position record stride drift")

    return {
        "requiresBowieAliveAndLeaderDead": True,
        "battleTableEntryBytes": battle_table_entry_bytes,
        "offscreenLoop": {
            "allySlotRange": [ally_start, ally_end],
            "allySlotCount": loop_count,
            "allyX": loop_x,
            "enemySlotRange": [hp_enemy_start, hp_enemy_end],
            "enemySlotCount": loop_count,
            "enemyX": loop_x,
        },
        "hpZeroLoop": {
            "enemySlotRange": [hp_enemy_start, hp_enemy_end],
            "enemySlotCount": loop_count,
            "hp": hp_value,
        },
        "positionOnlyTail": {
            "enemySlots": tail_slots,
            "enemySlotCount": observed_tail_call_count,
            "x": hp_value,
            "hasSetCurrentHp": False,
        },
        "positionEntryBytes": position_entry_bytes,
        "positionTerminator": -1,
        "unreachableDeadListWritePresent": True,
    }


def _cutscene_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "beforebattlecutscenesstart.asm",
        [
            "addi.w  #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   loc_47AE8",
            "move.w  rpt_BeforeBattleCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "battlestartcutscenesstart.asm",
        [
            "addi.w  #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   loc_47B8C",
            "jsr     j_SetFlag",
            "move.w  rpt_BattleStartCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "afterbattlecutscenesstart.asm",
        [
            "addi.w  #BATTLE_COMPLETED_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   EndAfterBattleCutscene",
            "move.w  rpt_AfterBattleCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "afterbattlecutscenesend.asm",
        [
            "move.b  table_AfterBattleJoins(pc,d0.w),d0",
            "jsr     j_JoinForce",
        ],
    )
    _require_ordered_fragments(
        root / "battleendcutscenesstart.asm",
        [
            "moveq   #ALLY_BOWIE,d0",
            "jsr     j_GetCurrentHp",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "jsr     j_GetCurrentHp",
            "addi.w  #BATTLE_COMPLETED_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "move.w  rpt_EnemyDefeatedCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "battleendcutscenesend.asm",
        [
            "move.b  table_EnemyLeaderPresentFlags(pc,d0.w),d0",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "jsr     j_GetCurrentHp",
            "move.b  d0,(a0,d1.w)",
            "move.b  #-1,1(a0,d1.w)",
            "addq.w  #1,(DEAD_COMBATANTS_LIST_LENGTH).l",
        ],
    )
    _require_ordered_fragments(
        root / "regionactivatedcutscenes.asm",
        [
            "lea     table_BattleRegionCutscenes-8(pc), a0",
            "addq.w  #8,a0",
            "cmpi.w  #TERMINATOR_WORD,(a0)",
            "cmp.b   (a0),d1",
            "move.w  2(a0),d1",
            "jsr     j_CheckFlag",
            "move.b  1(a0),d0",
            "jsr     j_CheckTriggerRegionFlag",
            "jsr     j_SetFlag",
            "movea.l 4(a0),a0",
            "trap    #MAPSCRIPT",
        ],
    )
    return {
        "intro": {
            "beforeBattleChecksSharedIntroFlag": True,
            "beforeBattleSetsFlag": False,
            "battleStartChecksSharedIntroFlag": True,
            "battleStartSetsFlagBeforeScript": True,
            "dispatchesByCurrentBattle": True,
        },
        "afterBattle": {
            "skipsScriptWhenBattleCompleted": True,
            "dispatchesByCurrentBattle": True,
            "joinTableRunsAtSharedEnd": True,
        },
        "enemyDefeated": {
            "requiresBowieAlive": True,
            "requiresLeaderSlotDead": True,
            "skipsScriptWhenBattleCompleted": True,
            "queuesLivingEnemiesWhenLeaderPresent": True,
            "deadListEntryTerminator": 255,
        },
        "leaderDeathPositions": _leader_death_position_facts(disasm),
        "region": {
            "tableEntryBytes": 8,
            "terminator": -1,
            "checksBattleThenPlayedFlagThenRegion": True,
            "setsPlayedFlagBeforeMapScript": True,
            "scriptDispatch": "MAPSCRIPT trap",
        },
    }


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    expected = toolchain["sf2disasm"]["commit"]
    if commit != expected:
        raise ValueError(f"battle cutscenes require SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    return disasm, commit, toolchain


def build_battle_cutscene_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle cutscene H1 listing is missing: {listing_path}")
    listing_addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle cutscene file set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).parent == SOURCE_ROOT
    ]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "function": {
            field: listing_addresses[symbol] for field, symbol in FUNCTION_SYMBOLS.items()
        },
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "cutsceneFacts": _cutscene_facts(disasm),
        "files": files,
    }


def verify_battle_cutscene_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_cutscene_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle cutscene static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle cutscene provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle cutscene summary drift")
    if output["function"] != fixture["function"]:
        raise ValueError("battle cutscene H1 address drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle cutscene symbol drift: {filename}::{symbol}")
    if output["cutsceneFacts"] != fixture["expected"]["cutsceneFacts"]:
        raise ValueError("battle cutscene model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle cutscene canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-cutscenes-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
