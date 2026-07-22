from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.h2.menus import (
    _caravan_range,
    _require_ordered_shop_section,
    _shop_direct_call_occurrences,
    _shop_instruction_records,
    _shop_jump_aliases,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-common-stats-static-v1"
SOURCE_ROOT = Path("code/common/stats")
ALTERNATE_SOURCES = {
    SOURCE_ROOT / "items/itemfunctions_s7_0.asm": SOURCE_ROOT / "iteminventory.asm",
    SOURCE_ROOT / "items/fielditemeffects.asm": Path(
        "code/common/menus/item/fielditemeffects.asm"
    ),
    SOURCE_ROOT / "items/itemactions_1.asm": Path(
        "code/common/menus/item/isitemusableonfield.asm"
    ),
}
MANIFEST = repo_path("manifests/extractions/common-stats-static.json")
SCHEMA = repo_path("schemas/common-stats-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/common-stats-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-stats-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battleparty.asm": "UpdateForce",
    "caravaninventory.asm": "AddItemToCaravan",
    "combatantstats_1.asm": "GetCombatantName",
    "combatantstats_2.asm": "LoadAllyName",
    "combatantstats_3.asm": "GetCombatantEntryAddress",
    "dealsinventory.asm": "GetDealsItemAmount",
    "findname.asm": "GetClassName",
    "gameflags.asm": "CheckFlag",
    "getcombatanttype.asm": "GetCombatantType",
    "gold.asm": "SetGold",
    "iteminventory.asm": "ReceiveMandatoryItem",
    "items/fielditemeffects.asm": "UseItemOnField",
    "items/itemactions_1.asm": "IsItemUsableOnField",
    "items/itemfunctions_s7_0.asm": "ReceiveMandatoryItem",
    "itemstats.asm": "GetItemName",
    "levelup.asm": "LevelUp",
    "newgame.asm": "NewGame",
    "spellstats.asm": "GetSpellName",
    "unusedsub_9482.asm": "nullsub_9482",
    "updatecombatantstats.asm": "UpdateCombatantStats",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _alternate_source_fact(
    disasm: Path, alternate_path: Path, canonical_path: Path, layout: str
) -> dict[str, Any]:
    canonical = disasm / canonical_path
    alternate = disasm / alternate_path
    canonical_bytes = canonical.read_bytes()
    alternate_bytes = alternate.read_bytes()
    range_pattern = re.compile(rb"; 0x([0-9A-F]+)\.\.0x([0-9A-F]+)")
    canonical_range = range_pattern.search(canonical_bytes)
    alternate_range = range_pattern.search(alternate_bytes)
    if not canonical_range or canonical_range.groups() != alternate_range.groups():
        raise ValueError(f"alternate source ROM range drift: {alternate_path}")
    canonical_labels = set(
        _parse_source_file(canonical, canonical_path.as_posix())["globalLabels"]
    )
    alternate_labels = set(
        _parse_source_file(alternate, alternate_path.as_posix())["globalLabels"]
    )
    canonical_include = f'include "{str(canonical_path).replace("/", chr(92))}"'
    alternate_include = f'include "{str(alternate_path).replace("/", chr(92))}"'
    if canonical_include not in layout or alternate_include in layout:
        raise ValueError(f"alternate source layout inclusion drift: {alternate_path}")
    return {
        "canonicalPath": canonical_path.as_posix(),
        "alternatePath": alternate_path.as_posix(),
        "sameAnnotatedRomRange": True,
        "sourceByteIdentical": canonical_bytes == alternate_bytes,
        "sharedGlobalSymbols": sorted(canonical_labels & alternate_labels),
        "canonicalIncludedByLayout": True,
        "alternateIncludedByLayout": False,
        "alternateExcludedFromStrictReach": True,
        "canonicalSha256": hashlib.sha256(canonical_bytes).hexdigest().upper(),
        "alternateSha256": hashlib.sha256(alternate_bytes).hexdigest().upper(),
    }


def _combatant_getter_contract(disasm: Path) -> dict[str, Any]:
    """Extract the complete source-shaped read surface of combatantstats_1.asm."""
    root = disasm / SOURCE_ROOT
    source_path = root / "combatantstats_1.asm"
    source = source_path.read_text(encoding="utf-8")
    entry_abi_source = (root / "combatantstats_3.asm").read_text(encoding="utf-8")
    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    listing = (disasm.parent / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    _require_ordered_shop_section(
        source_path,
        "GetCombatantName:",
        "GetClass:",
        (
            "btst    #COMBATANT_BIT_ENEMY,d0",
            "bne.s   @Enemy",
            "bsr.w   GetCombatantEntryAddress",
            "tst.b   (a0,d7.w)",
            "dbf     d0,@CountNameLength_Loop",
            "bsr.w   GetEnemy",
            "movea.l (p_table_EnemyNames).l,a0",
            "bsr.w   FindName",
        ),
    )
    _require_ordered_shop_section(
        root / "combatantstats_3.asm",
        "GetCombatantEntryAddress:",
        "SetCombatantByte:",
        (
            "cmpi.b  #COMBATANT_ENEMIES_START,d0",
            "cmpi.b  #COMBATANT_ALLIES_SPACE_END_MINUS_ONE,d0",
            "subi.b  #COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END,d0",
            "lsl.w   #3,d0",
            "move.w  d0,d1",
            "lsl.w   #3,d0",
            "sub.w   d1,d0",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "GetMovetype:",
        "GetAiCommandset:",
        (
            "moveq   #COMBATANT_OFFSET_MOVETYPE_AND_AI_COMMANDSET,d7",
            "bsr.w   GetCombatantByte",
            "lsr.w   #NIBBLE_SHIFT_COUNT,d1",
            "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "GetClass:",
        "GetLevel:",
        (
            "moveq   #COMBATANT_OFFSET_CLASS,d7",
            "bsr.w   GetCombatantByte",
            "movem.l (sp)+,d7-a0",
            "rts",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "GetMoveOrders:",
        "GetTriggerRegions:",
        (
            "moveq   #COMBATANT_OFFSET_MOVE_ORDERS,d7",
            "bsr.w   GetCombatantWord",
            "lsr.w   #BYTE_SHIFT_COUNT,d1",
            "andi.w  #BYTE_MASK,d1",
            "andi.w  #BYTE_MASK,d2",
        ),
    )
    _require_ordered_shop_section(
        source_path,
        "GetEnemy:",
        "GetKills:",
        (
            "btst    #COMBATANT_BIT_ENEMY,d0",
            "bne.s   @Continue",
            "move.w  #-1,d1",
            "rts",
            "moveq   #COMBATANT_OFFSET_ENEMY_INDEX,d7",
            "bsr.w   GetCombatantByte",
            "movem.l (sp)+,d7-a0",
            "rts",
        ),
    )
    constant_names = (
        "COMBATANT_BIT_ENEMY",
        "ALLYNAME_CHARACTERS_COUNTER",
        "NIBBLE_SHIFT_COUNT",
        "BYTE_SHIFT_COUNT",
        "BYTE_LOWER_NIBBLE_MASK",
        "BYTE_MASK",
        "COMBATANT_ALLIES_SPACE_END_MINUS_ONE",
        "COMBATANT_ENEMIES_START",
        "COMBATANT_ENEMIES_SPACE_END",
        "COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END",
        *(re.findall(r"#(COMBATANT_OFFSET_[A-Z0-9_]+)", source)),
    )
    constants: dict[str, int] = {}
    for name in dict.fromkeys(constant_names):
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"combatant getter constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)
    routine_names = re.findall(
        r"^\s*; End of function ([A-Za-z_][A-Za-z0-9_]*).*?$", source, re.MULTILINE
    )
    if (
        len(routine_names) != 31
        or routine_names[0] != "GetCombatantName"
        or routine_names[-1] != "GetDefeats"
    ):
        raise ValueError("combatant getter routine boundary drift")

    def section(name: str) -> str:
        start = source.find(f"{name}:")
        end = source.find("\n    ; End of function", start)
        if start < 0 or end < 0:
            raise ValueError(f"combatant getter source boundary drift: {name}")
        return source[start:end]

    routines = {name: _shop_instruction_records(section(name)) for name in routine_names}
    entry_routine_names = (
        "GetCombatantEntryAddress",
        "GetCombatantByte",
        "GetCombatantWord",
    )

    def entry_section(name: str) -> str:
        start = entry_abi_source.find(f"{name}:")
        end = entry_abi_source.find("\n    ; End of function", start)
        if start < 0 or end < 0:
            raise ValueError(f"combatant entry ABI source boundary drift: {name}")
        return entry_abi_source[start:end]

    entry_routines = {
        name: _shop_instruction_records(entry_section(name)) for name in entry_routine_names
    }

    def listing_address(name: str) -> int:
        match = re.search(rf"^([0-9A-F]{{8}})\s+{name}:$", listing, re.MULTILINE)
        if not match:
            raise ValueError(f"combatant getter H1 symbol drift: {name}")
        return int(match.group(1), 16)

    addresses = {name: listing_address(name) for name in routine_names}
    entry_addresses = {name: listing_address(name) for name in entry_routine_names}
    targets = set(routines)
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    internal = {
        "code/common/stats/combatantstats_1.asm": _shop_direct_call_occurrences(
            source_path, alias_targets, targets
        )
    }
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path != source_path
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }

    def totals(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                occurrence["siteCount"]
                for occurrences in callers.values()
                for occurrence in occurrences
                if occurrence["effectiveTarget"] == target
            )
            for target in routine_names
        }

    def find_index(name: str, predicate) -> int:
        try:
            return next(index for index, record in enumerate(routines[name]) if predicate(record))
        except StopIteration:
            raise ValueError(f"combatant getter use-site drift: {name}") from None

    def instruction_index(records: list[dict[str, Any]], description: str, predicate) -> int:
        matches = [index for index, record in enumerate(records) if predicate(record)]
        if len(matches) != 1:
            raise ValueError(f"combatant getter {description} drift")
        return matches[0]

    def terminal(name: str, restored_registers: str = "d7-a0") -> int:
        records = routines[name]
        index = len(records) - 1
        if records[index]["opcode"] != "rts":
            raise ValueError(f"combatant getter terminal drift: {name}")
        restore = records[index - 1]
        if (
            restore["opcode"] != "movem.l"
            or restore["operands"] != ["(sp)+", restored_registers]
            or restore["directTarget"] is not None
            or restore["branchTarget"] is not None
        ):
            raise ValueError(f"combatant getter restore-before-terminal drift: {name}")
        return index

    generic_spec = {
        "GetClass": ("COMBATANT_OFFSET_CLASS", "GetCombatantByte"),
        "GetLevel": ("COMBATANT_OFFSET_LEVEL", "GetCombatantByte"),
        "GetMaxHp": ("COMBATANT_OFFSET_HP_MAX", "GetCombatantWord"),
        "GetCurrentHp": ("COMBATANT_OFFSET_HP_CURRENT", "GetCombatantWord"),
        "GetMaxMp": ("COMBATANT_OFFSET_MP_MAX", "GetCombatantByte"),
        "GetCurrentMp": ("COMBATANT_OFFSET_MP_CURRENT", "GetCombatantByte"),
        "GetBaseAtt": ("COMBATANT_OFFSET_ATT_BASE", "GetCombatantByte"),
        "GetCurrentAtt": ("COMBATANT_OFFSET_ATT_CURRENT", "GetCombatantByte"),
        "GetBaseDef": ("COMBATANT_OFFSET_DEF_BASE", "GetCombatantByte"),
        "GetCurrentDef": ("COMBATANT_OFFSET_DEF_CURRENT", "GetCombatantByte"),
        "GetBaseAgi": ("COMBATANT_OFFSET_AGI_BASE", "GetCombatantByte"),
        "GetCurrentAgi": ("COMBATANT_OFFSET_AGI_CURRENT", "GetCombatantByte"),
        "GetBaseMov": ("COMBATANT_OFFSET_MOV_BASE", "GetCombatantByte"),
        "GetCurrentMov": ("COMBATANT_OFFSET_MOV_CURRENT", "GetCombatantByte"),
        "GetBaseResistance": ("COMBATANT_OFFSET_RESIST_BASE", "GetCombatantWord"),
        "GetCurrentResistance": ("COMBATANT_OFFSET_RESIST_CURRENT", "GetCombatantWord"),
        "GetBaseProwess": ("COMBATANT_OFFSET_PROWESS_BASE", "GetCombatantByte"),
        "GetCurrentProwess": ("COMBATANT_OFFSET_PROWESS_CURRENT", "GetCombatantByte"),
        "GetStatusEffects": ("COMBATANT_OFFSET_STATUSEFFECTS", "GetCombatantWord"),
        "GetCombatantX": ("COMBATANT_OFFSET_X", "GetCombatantByte"),
        "GetCombatantY": ("COMBATANT_OFFSET_Y", "GetCombatantByte"),
        "GetCurrentExp": ("COMBATANT_OFFSET_EXP", "GetCombatantByte"),
        "GetMovetype": ("COMBATANT_OFFSET_MOVETYPE_AND_AI_COMMANDSET", "GetCombatantByte"),
        "GetAiCommandset": ("COMBATANT_OFFSET_MOVETYPE_AND_AI_COMMANDSET", "GetCombatantByte"),
        "GetMoveOrders": ("COMBATANT_OFFSET_MOVE_ORDERS", "GetCombatantWord"),
        "GetTriggerRegions": ("COMBATANT_OFFSET_TRIGGER_REGIONS", "GetCombatantByte"),
        "GetActivationBitfield": ("COMBATANT_OFFSET_ACTIVATION_BITFIELD", "GetCombatantWord"),
        "GetKills": ("COMBATANT_OFFSET_ALLY_KILLS", "GetCombatantWord"),
        "GetDefeats": ("COMBATANT_OFFSET_ALLY_DEFEATS", "GetCombatantWord"),
    }
    if set(generic_spec) != set(routine_names) - {"GetCombatantName", "GetEnemy"}:
        raise ValueError("combatant getter generic specification coverage drift")

    def generic(name: str) -> dict[str, Any]:
        records = routines[name]
        field_index = find_index(name, lambda record: record["opcode"] == "moveq")
        helper_index = find_index(
            name, lambda record: record["directTarget"] in {"GetCombatantByte", "GetCombatantWord"}
        )
        field_name = records[field_index]["operands"][0].removeprefix("#")
        helper = records[helper_index]["directTarget"]
        if generic_spec[name] != (field_name, helper):
            raise ValueError(f"combatant getter field/helper specification drift: {name}")
        if records[field_index]["operands"][1:] != ["d7"]:
            raise ValueError(f"combatant getter field register drift: {name}")
        return {
            "routineAddress": addresses[name],
            "selector": {
                "register": "d0",
                "widthBits": 8,
                "entryAddressAbi": "GetCombatantEntryAddress",
            },
            "field": {
                "constant": field_name,
                "value": constants[field_name],
                "loadInstructionIndex": field_index,
                "helperCallInstructionIndex": helper_index,
                "helper": helper,
                "readWidthBits": 8 if helper == "GetCombatantByte" else 16,
            },
            "return": {
                "register": "d1",
                "widthBits": 16,
                "terminalInstructionIndex": terminal(name),
            },
        }

    getters = {name: generic(name) for name in generic_spec}
    for name in ("GetCombatantX", "GetCombatantY"):
        getters[name]["return"]["signExtendInstructionIndex"] = find_index(
            name, lambda record: record["opcode"] == "ext.w" and record["operands"] == ["d1"]
        )
    def exact_index(name: str, description: str, opcode: str, operands: list[str]) -> int:
        return instruction_index(
            routines[name],
            f"{name} {description}",
            lambda record: record["opcode"] == opcode and record["operands"] == operands,
        )

    def extracted_value(
        name: str, *, source_register: str, shift_constant: str | None, mask_constant: str
    ) -> dict[str, Any]:
        value = {
            "sourceRegister": source_register,
            "outputRegister": source_register,
            "mask": {
                "constant": mask_constant,
                "value": constants[mask_constant],
                "instructionIndex": exact_index(
                    name,
                    f"{source_register} mask",
                    "andi.w",
                    [f"#{mask_constant}", source_register],
                ),
            },
        }
        if shift_constant is not None:
            value["shift"] = {
                "constant": shift_constant,
                "value": constants[shift_constant],
                "instructionIndex": exact_index(
                    name,
                    f"{source_register} shift",
                    "lsr.w",
                    [f"#{shift_constant}", source_register],
                ),
            }
        return value

    movetype = generic("GetMovetype")
    movetype["return"] = {
        "outputRegister": "d1",
        "widthBits": 16,
        "terminalInstructionIndex": terminal("GetMovetype"),
    }
    movetype["highNibble"] = extracted_value(
        "GetMovetype",
        source_register="d1",
        shift_constant="NIBBLE_SHIFT_COUNT",
        mask_constant="BYTE_LOWER_NIBBLE_MASK",
    )
    getters["GetMovetype"] = movetype
    commandset = generic("GetAiCommandset")
    commandset["return"] = {
        "outputRegister": "d1",
        "widthBits": 16,
        "terminalInstructionIndex": terminal("GetAiCommandset"),
    }
    commandset["lowNibble"] = extracted_value(
        "GetAiCommandset",
        source_register="d1",
        shift_constant=None,
        mask_constant="BYTE_LOWER_NIBBLE_MASK",
    )
    getters["GetAiCommandset"] = commandset
    for name, high_shift, high_mask, high_name in (
        ("GetMoveOrders", "BYTE_SHIFT_COUNT", "BYTE_MASK", "highByte"),
        ("GetTriggerRegions", "NIBBLE_SHIFT_COUNT", "BYTE_LOWER_NIBBLE_MASK", "highNibble"),
    ):
        fact = generic(name)
        copy_index = exact_index(name, "d1-to-d2 copy", "move.w", ["d1", "d2"])
        high = extracted_value(
            name,
            source_register="d1",
            shift_constant=high_shift,
            mask_constant=high_mask,
        )
        low = extracted_value(
            name,
            source_register="d2",
            shift_constant=None,
            mask_constant=high_mask,
        )
        if not (
            copy_index
            < high["shift"]["instructionIndex"]
            < high["mask"]["instructionIndex"]
            < low["mask"]["instructionIndex"]
        ):
            raise ValueError(f"combatant getter split-value order drift: {name}")
        fact["return"] = {
            "primaryOutputRegister": "d1",
            "secondaryOutputRegister": "d2",
            "widthBits": 16,
            "terminalInstructionIndex": terminal(name),
        }
        fact["splitValue"] = {
            "copy": {
                "sourceRegister": "d1",
                "destinationRegister": "d2",
                "instructionIndex": copy_index,
            },
            high_name: high,
            "lowByte" if name == "GetMoveOrders" else "lowNibble": low,
        }
        getters[name] = fact
    name_records = routines["GetCombatantName"]
    name_enemy_test = instruction_index(
        name_records,
        "GetCombatantName enemy bit test",
        lambda record: record["opcode"] == "btst"
        and record["operands"] == ["#COMBATANT_BIT_ENEMY", "d0"],
    )
    name_enemy_branch = instruction_index(
        name_records,
        "GetCombatantName enemy branch",
        lambda record: record["opcode"] == "bne.s" and record["branchTarget"] == "@Enemy",
    )
    name_entry_call = instruction_index(
        name_records,
        "GetCombatantName entry call",
        lambda record: record["directTarget"] == "GetCombatantEntryAddress",
    )
    name_counter = exact_index(
        "GetCombatantName",
        "length counter initialization",
        "moveq",
        ["#ALLYNAME_CHARACTERS_COUNTER", "d0"],
    )
    name_length_zero = exact_index(
        "GetCombatantName", "length zero initialization", "clr.w", ["d7"]
    )
    name_byte_test = exact_index("GetCombatantName", "name byte test", "tst.b", ["(a0,d7.w)"])
    name_break_branch = exact_index("GetCombatantName", "name break branch", "beq.s", ["@Break"])
    name_increment = exact_index(
        "GetCombatantName", "name length increment", "addq.w", ["#1", "d7"]
    )
    name_loop = exact_index(
        "GetCombatantName", "name loop", "dbf", ["d0", "@CountNameLength_Loop"]
    )
    name_done_branch = exact_index("GetCombatantName", "name done branch", "bra.s", ["@Done"])
    name_enemy_call = instruction_index(
        name_records,
        "GetCombatantName enemy index call",
        lambda record: record["directTarget"] == "GetEnemy",
    )
    name_table_load = exact_index(
        "GetCombatantName", "enemy table load", "movea.l", ["(p_table_EnemyNames).l", "a0"]
    )
    name_find = instruction_index(
        name_records,
        "GetCombatantName name lookup call",
        lambda record: record["directTarget"] == "FindName",
    )
    name_restore = len(name_records) - 2
    if not (
        name_enemy_test < name_enemy_branch < name_entry_call < name_counter < name_length_zero
        < name_byte_test < name_break_branch < name_increment < name_loop < name_done_branch
        < name_enemy_call < name_table_load < name_find < name_restore
        < terminal("GetCombatantName", "d0-d1")
    ):
        raise ValueError("combatant getter GetCombatantName control-flow order drift")
    getters["GetCombatantName"] = {
        "routineAddress": addresses["GetCombatantName"],
        "selector": {
            "register": "d0",
            "widthBits": 8,
            "enemyBitTestInstructionIndex": name_enemy_test,
            "enemyBranchInstructionIndex": name_enemy_branch,
        },
        "allyNameSource": {
            "entryAddressCallInstructionIndex": name_entry_call,
            "lengthCounterConstant": "ALLYNAME_CHARACTERS_COUNTER",
            "lengthCounterValue": constants["ALLYNAME_CHARACTERS_COUNTER"],
            "lengthCounterInitializationInstructionIndex": name_counter,
            "lengthZeroInitializationInstructionIndex": name_length_zero,
            "byteTestInstructionIndex": name_byte_test,
            "breakBranchInstructionIndex": name_break_branch,
            "incrementInstructionIndex": name_increment,
            "lengthLoopInstructionIndex": name_loop,
            "doneBranchInstructionIndex": name_done_branch,
        },
        "enemyNameSource": {
            "enemyIndexCallInstructionIndex": name_enemy_call,
            "nameTableLoadInstructionIndex": name_table_load,
            "findNameCallInstructionIndex": name_find,
        },
        "return": {
            "addressRegister": "a0",
            "lengthRegister": "d7",
            "restoreInstructionIndex": name_restore,
            "terminalInstructionIndex": terminal("GetCombatantName", "d0-d1"),
        },
    }
    enemy_records = routines["GetEnemy"]
    enemy_bit_test = instruction_index(
        enemy_records,
        "GetEnemy enemy bit test",
        lambda record: record["opcode"] == "btst"
        and record["operands"] == ["#COMBATANT_BIT_ENEMY", "d0"],
    )
    enemy_branch = instruction_index(
        enemy_records,
        "GetEnemy enemy branch",
        lambda record: record["opcode"] == "bne.s" and record["branchTarget"] == "@Continue",
    )
    enemy_minus_one = exact_index("GetEnemy", "non-enemy result", "move.w", ["#-1", "d1"])
    enemy_terminals = [
        index for index, record in enumerate(enemy_records) if record["opcode"] == "rts"
    ]
    if len(enemy_terminals) != 2:
        raise ValueError("combatant getter GetEnemy terminal count drift")
    enemy_non_terminal = enemy_terminals[0]
    enemy_unreachable = exact_index("GetEnemy", "source-unreachable branch", "bra.s", ["GetKills"])
    enemy_field = exact_index(
        "GetEnemy", "enemy index field", "moveq", ["#COMBATANT_OFFSET_ENEMY_INDEX", "d7"]
    )
    enemy_helper = instruction_index(
        enemy_records,
        "GetEnemy byte helper",
        lambda record: record["directTarget"] == "GetCombatantByte",
    )
    enemy_restore = len(enemy_records) - 2
    if not (
        enemy_bit_test < enemy_branch < enemy_minus_one < enemy_non_terminal < enemy_unreachable
        < enemy_field < enemy_helper < enemy_restore < terminal("GetEnemy")
    ):
        raise ValueError("combatant getter GetEnemy control-flow order drift")
    getters["GetEnemy"] = {
        "routineAddress": addresses["GetEnemy"],
        "selector": {
            "register": "d0",
            "widthBits": 8,
            "enemyBitTestInstructionIndex": enemy_bit_test,
            "enemyBranchInstructionIndex": enemy_branch,
        },
        "nonEnemy": {
            "minusOneResultInstructionIndex": enemy_minus_one,
            "terminalInstructionIndex": enemy_non_terminal,
            "sourceUnreachableBranchInstructionIndex": enemy_unreachable,
        },
        "enemy": {
            "fieldConstant": "COMBATANT_OFFSET_ENEMY_INDEX",
            "fieldValue": constants["COMBATANT_OFFSET_ENEMY_INDEX"],
            "fieldLoadInstructionIndex": enemy_field,
            "helperCallInstructionIndex": enemy_helper,
            "helper": "GetCombatantByte",
            "readWidthBits": 8,
        },
        "return": {
            "register": "d1",
            "widthBits": 16,
            "restoreInstructionIndex": enemy_restore,
            "terminalInstructionIndex": terminal("GetEnemy"),
        },
    }
    entry_records = entry_routines["GetCombatantEntryAddress"]

    def entry_exact(name: str, description: str, opcode: str, operands: list[str]) -> int:
        return instruction_index(
            entry_routines[name],
            f"{name} {description}",
            lambda record: record["opcode"] == opcode and record["operands"] == operands,
        )

    entry_enemy_compare = entry_exact(
        "GetCombatantEntryAddress",
        "enemy threshold comparison",
        "cmpi.b",
        ["#COMBATANT_ENEMIES_START", "d0"],
    )
    entry_enemy_branch = entry_exact(
        "GetCombatantEntryAddress", "enemy threshold branch", "bcc.s", ["@Enemy"]
    )
    entry_ally_compare = entry_exact(
        "GetCombatantEntryAddress",
        "ally upper-bound comparison",
        "cmpi.b",
        ["#COMBATANT_ALLIES_SPACE_END_MINUS_ONE", "d0"],
    )
    entry_ally_error = entry_ally_compare + 1
    if (
        entry_records[entry_ally_error]["opcode"] != "bhi.s"
        or entry_records[entry_ally_error]["operands"] != ["@ErrorHandling"]
    ):
        raise ValueError("combatant entry ABI ally error branch drift")
    entry_ally_address = entry_exact(
        "GetCombatantEntryAddress", "ally address branch", "bra.s", ["@GetAddress"]
    )
    entry_enemy_upper_compare = entry_exact(
        "GetCombatantEntryAddress",
        "enemy upper-bound comparison",
        "cmpi.b",
        ["#COMBATANT_ENEMIES_SPACE_END", "d0"],
    )
    entry_enemy_error = entry_enemy_upper_compare + 1
    if (
        entry_records[entry_enemy_error]["opcode"] != "bhi.s"
        or entry_records[entry_enemy_error]["operands"] != ["@ErrorHandling"]
    ):
        raise ValueError("combatant entry ABI enemy error branch drift")
    entry_adjust = entry_exact(
        "GetCombatantEntryAddress",
        "enemy index adjustment",
        "subi.b",
        ["#COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END", "d0"],
    )
    entry_mask = entry_exact(
        "GetCombatantEntryAddress", "selector mask", "andi.w", ["#BYTE_MASK", "d0"]
    )
    entry_shifts = [
        index
        for index, record in enumerate(entry_records)
        if record["opcode"] == "lsl.w" and record["operands"] == ["#3", "d0"]
    ]
    entry_copy = entry_exact(
        "GetCombatantEntryAddress", "stride copy", "move.w", ["d0", "d1"]
    )
    entry_subtract = entry_exact(
        "GetCombatantEntryAddress", "stride subtraction", "sub.w", ["d1", "d0"]
    )
    entry_base = entry_exact(
        "GetCombatantEntryAddress",
        "entry base load",
        "lea",
        ["((COMBATANT_DATA-$1000000)).w", "a0"],
    )
    entry_add = entry_exact(
        "GetCombatantEntryAddress", "entry address addition", "adda.w", ["d0", "a0"]
    )
    entry_valid_terminal = entry_exact("GetCombatantEntryAddress", "valid terminal", "rts", [])
    entry_valid_restore = entry_valid_terminal - 1
    if (
        entry_records[entry_valid_restore]["opcode"] != "movem.w"
        or entry_records[entry_valid_restore]["operands"] != ["(sp)+", "d0-d1"]
    ):
        raise ValueError("combatant entry ABI valid restore drift")
    entry_error_restore = instruction_index(
        entry_records,
        "GetCombatantEntryAddress error restore",
        lambda record: record["labels"] == ["@ErrorHandling"]
        and record["opcode"] == "movem.w"
        and record["operands"] == ["(sp)+", "d0-d1"],
    )
    entry_error_code = entry_exact(
        "GetCombatantEntryAddress", "error code write", "move.l", ["#'CNUM'", "(ERRCODE_BYTE0).l"]
    )
    entry_error_return = entry_exact(
        "GetCombatantEntryAddress",
        "error return-address write",
        "move.l",
        ["(sp)", "(ERRCODE_BYTE4).l"],
    )
    entry_trap = entry_exact(
        "GetCombatantEntryAddress", "VINT trap", "trap", ["#VINT_FUNCTIONS"]
    )
    entry_vint_function = entry_exact(
        "GetCombatantEntryAddress", "VINT function argument", "dc.w", ["VINTS_DEACTIVATE"]
    )
    entry_vint_argument = entry_exact(
        "GetCombatantEntryAddress", "VINT argument", "dc.l", ["0"]
    )
    entry_loop = instruction_index(
        entry_records,
        "GetCombatantEntryAddress infinite loop",
        lambda record: record["labels"] == ["@InfiniteLoop"]
        and record["opcode"] == "bra.s"
        and record["branchTarget"] == "@InfiniteLoop",
    )
    entry_order = [
        entry_enemy_compare,
        entry_enemy_branch,
        entry_ally_compare,
        entry_ally_error,
        entry_ally_address,
        entry_enemy_upper_compare,
        entry_enemy_error,
        entry_adjust,
        entry_mask,
        entry_copy,
        entry_subtract,
        entry_base,
        entry_add,
        entry_valid_restore,
        entry_valid_terminal,
        entry_error_restore,
        entry_error_code,
        entry_error_return,
        entry_trap,
        entry_vint_function,
        entry_vint_argument,
        entry_loop,
    ]
    if entry_shifts != [entry_mask + 1, entry_copy + 1] or entry_order != sorted(entry_order):
        raise ValueError("combatant entry ABI control-flow or stride order drift")
    stride_shift_count = int(entry_records[entry_shifts[0]]["operands"][0].removeprefix("#"))
    entry_stride_bytes = (1 << (stride_shift_count * 2)) - (1 << stride_shift_count)
    byte_records = entry_routines["GetCombatantByte"]
    byte_entry_call = instruction_index(
        byte_records,
        "GetCombatantByte entry call",
        lambda record: record["directTarget"] == "GetCombatantEntryAddress",
    )
    byte_clear = entry_exact("GetCombatantByte", "d1 zero initialization", "clr.w", ["d1"])
    byte_read = entry_exact("GetCombatantByte", "byte read", "move.b", ["(a0,d7.w)", "d1"])
    byte_terminal = entry_exact("GetCombatantByte", "terminal", "rts", [])
    word_records = entry_routines["GetCombatantWord"]
    word_entry_call = instruction_index(
        word_records,
        "GetCombatantWord entry call",
        lambda record: record["directTarget"] == "GetCombatantEntryAddress",
    )
    word_read = entry_exact("GetCombatantWord", "word read", "move.w", ["(a0,d7.w)", "d1"])
    word_terminal = entry_exact("GetCombatantWord", "terminal", "rts", [])
    if not (
        byte_entry_call < byte_clear < byte_read < byte_terminal
        and word_entry_call < word_read < word_terminal
    ):
        raise ValueError("combatant entry ABI reader order drift")
    entry_abi = {
        "sourcePath": "code/common/stats/combatantstats_3.asm",
        "routines": {
            "GetCombatantEntryAddress": {
                "routineAddress": entry_addresses["GetCombatantEntryAddress"],
                "operations": entry_records,
                "validRoutes": {
                    "ally": {
                        "enemyThresholdComparisonInstructionIndex": entry_enemy_compare,
                        "enemyThresholdBranchInstructionIndex": entry_enemy_branch,
                        "enemyThresholdBranchTarget": "@Enemy",
                        "enemyThresholdBranchTaken": False,
                        "upperBoundComparisonInstructionIndex": entry_ally_compare,
                        "errorBranchInstructionIndex": entry_ally_error,
                        "errorBranchTarget": "@ErrorHandling",
                        "errorBranchTaken": False,
                        "addressBranchInstructionIndex": entry_ally_address,
                        "addressBranchTarget": "@GetAddress",
                    },
                    "enemy": {
                        "enemyThresholdComparisonInstructionIndex": entry_enemy_compare,
                        "enemyThresholdBranchInstructionIndex": entry_enemy_branch,
                        "enemyThresholdBranchTarget": "@Enemy",
                        "enemyThresholdBranchTaken": True,
                        "upperBoundComparisonInstructionIndex": entry_enemy_upper_compare,
                        "errorBranchInstructionIndex": entry_enemy_error,
                        "errorBranchTarget": "@ErrorHandling",
                        "errorBranchTaken": False,
                        "indexAdjustmentInstructionIndex": entry_adjust,
                    },
                },
                "errorRoutes": {
                    "allyAboveUpperBound": {
                        "comparisonInstructionIndex": entry_ally_compare,
                        "branchInstructionIndex": entry_ally_error,
                        "branchTarget": "@ErrorHandling",
                        "branchTaken": True,
                    },
                    "enemyAboveUpperBound": {
                        "comparisonInstructionIndex": entry_enemy_upper_compare,
                        "branchInstructionIndex": entry_enemy_error,
                        "branchTarget": "@ErrorHandling",
                        "branchTaken": True,
                    },
                },
                "addressCalculation": {
                    "selectorMaskConstant": "BYTE_MASK",
                    "selectorMaskValue": constants["BYTE_MASK"],
                    "selectorMaskInstructionIndex": entry_mask,
                    "leftShiftCount": stride_shift_count,
                    "leftShiftInstructionIndexes": entry_shifts,
                    "copyInstructionIndex": entry_copy,
                    "subtractInstructionIndex": entry_subtract,
                    "entryStrideBytes": entry_stride_bytes,
                    "baseLoadInstructionIndex": entry_base,
                    "addressAddInstructionIndex": entry_add,
                    "restoreInstructionIndex": entry_valid_restore,
                    "terminalInstructionIndex": entry_valid_terminal,
                },
                "errorHandling": {
                    "restoreInstructionIndex": entry_error_restore,
                    "errorCodeWriteInstructionIndex": entry_error_code,
                    "errorReturnAddressWriteInstructionIndex": entry_error_return,
                    "trapInstructionIndex": entry_trap,
                    "vintFunctionArgumentInstructionIndex": entry_vint_function,
                    "vintArgumentInstructionIndex": entry_vint_argument,
                    "infiniteLoopInstructionIndex": entry_loop,
                    "infiniteLoopTarget": "@InfiniteLoop",
                },
            },
            "GetCombatantByte": {
                "routineAddress": entry_addresses["GetCombatantByte"],
                "operations": byte_records,
                "entryAddressCallInstructionIndex": byte_entry_call,
                "read": {
                    "zeroInitializationInstructionIndex": byte_clear,
                    "readInstructionIndex": byte_read,
                    "readOpcode": "move.b",
                    "sourceOperand": "(a0,d7.w)",
                    "destinationRegister": "d1",
                },
                "terminalInstructionIndex": byte_terminal,
            },
            "GetCombatantWord": {
                "routineAddress": entry_addresses["GetCombatantWord"],
                "operations": word_records,
                "entryAddressCallInstructionIndex": word_entry_call,
                "read": {
                    "readInstructionIndex": word_read,
                    "readOpcode": "move.w",
                    "sourceOperand": "(a0,d7.w)",
                    "destinationRegister": "d1",
                },
                "terminalInstructionIndex": word_terminal,
            },
        },
    }
    return {
        "sourcePath": "code/common/stats/combatantstats_1.asm",
        "sourceRange": _caravan_range(source, "code/common/stats/combatantstats_1.asm"),
        "routineOrder": routine_names,
        "routineAddresses": addresses,
        "constants": constants,
        "routineOperations": routines,
        "entryAddressAbi": entry_abi,
        "getters": getters,
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": totals(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": totals(external),
        "staticBoundary": {
            "callerVisibleMeaning": "inferred",
            "callerAndRuntimeOutcome": "unknown",
            "setterAndClampFollowup": "unknown",
        },
    }


def _combatant_mutation_contract(disasm: Path) -> dict[str, Any]:
    """Extract the source-shaped mutation wrapper surface of combatantstats_2.asm."""
    root = disasm / SOURCE_ROOT
    source_path = root / "combatantstats_2.asm"
    source = source_path.read_text(encoding="utf-8")
    helpers_path = root / "combatantstats_3.asm"
    helpers_source = helpers_path.read_text(encoding="utf-8")
    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    listing = (disasm.parent / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    names = re.findall(r"^\s*; End of function ([A-Za-z_][A-Za-z0-9_]*).*?$", source, re.MULTILINE)
    if len(names) != 53 or names[0] != "LoadAllyName" or names[-1] != "DecreaseCurrentMov":
        raise ValueError("combatant mutation routine boundary drift")

    def section(text: str, name: str) -> str:
        start = text.find(f"{name}:")
        end = text.find("\n    ; End of function", start)
        if start < 0 or end < 0:
            raise ValueError(f"combatant mutation section drift: {name}")
        return text[start:end]

    routines = {name: _shop_instruction_records(section(source, name)) for name in names}

    def address(name: str) -> int:
        match = re.search(rf"^([0-9A-F]{{8}})\s+{name}:$", listing, re.MULTILINE)
        if not match:
            raise ValueError(f"combatant mutation H1 address drift: {name}")
        return int(match.group(1), 16)

    addresses = {name: address(name) for name in names}
    constant_names = tuple(
        dict.fromkeys(
            re.findall(
                r"#(COMBATANT_OFFSET_[A-Z0-9_]+|CHAR_STATCAP_[A-Z0-9_]+|ALLYNAME_CHARACTERS_COUNTER|BYTE_SHIFT_COUNT|NIBBLE_SHIFT_COUNT|BYTE_MASK|BYTE_LOWER_NIBBLE_MASK)",
                source,
            )
        )
    )
    constants: dict[str, int] = {}
    for name in constant_names:
        match = re.search(rf"^{name}:\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)", enums, re.MULTILINE)
        if not match:
            raise ValueError(f"combatant mutation constant drift: {name}")
        raw = match.group(1)
        constants[name] = int(raw[1:], 16) if raw.startswith("$") else int(raw)

    def exact(
        records: list[dict[str, Any]], description: str, opcode: str, operands: list[str]
    ) -> int:
        matches = [
            index
            for index, record in enumerate(records)
            if record["opcode"] == opcode and record["operands"] == operands
        ]
        if len(matches) != 1:
            raise ValueError(f"combatant mutation {description} drift")
        return matches[0]

    def call(records: list[dict[str, Any]], description: str, target: str) -> int:
        matches = [
            index for index, record in enumerate(records) if record["directTarget"] == target
        ]
        if len(matches) != 1:
            raise ValueError(f"combatant mutation {description} drift")
        return matches[0]

    def final_terminal(records: list[dict[str, Any]], name: str) -> int:
        index = len(records) - 1
        if records[index]["opcode"] != "rts":
            raise ValueError(f"combatant mutation terminal drift: {name}")
        return index

    direct_specs = {
        "SetClass": ("COMBATANT_OFFSET_CLASS", "SetCombatantByte"),
        "SetLevel": ("COMBATANT_OFFSET_LEVEL", "SetCombatantByte"),
        "SetMaxHp": ("COMBATANT_OFFSET_HP_MAX", "SetCombatantWord"),
        "SetCurrentHp": ("COMBATANT_OFFSET_HP_CURRENT", "SetCombatantWord"),
        "SetMaxMp": ("COMBATANT_OFFSET_MP_MAX", "SetCombatantByte"),
        "SetCurrentMp": ("COMBATANT_OFFSET_MP_CURRENT", "SetCombatantByte"),
        "SetBaseAtt": ("COMBATANT_OFFSET_ATT_BASE", "SetCombatantByte"),
        "SetCurrentAtt": ("COMBATANT_OFFSET_ATT_CURRENT", "SetCombatantByte"),
        "SetBaseDef": ("COMBATANT_OFFSET_DEF_BASE", "SetCombatantByte"),
        "SetCurrentDef": ("COMBATANT_OFFSET_DEF_CURRENT", "SetCombatantByte"),
        "SetBaseAgi": ("COMBATANT_OFFSET_AGI_BASE", "SetCombatantByte"),
        "SetCurrentAgi": ("COMBATANT_OFFSET_AGI_CURRENT", "SetCombatantByte"),
        "SetBaseMov": ("COMBATANT_OFFSET_MOV_BASE", "SetCombatantByte"),
        "SetCurrentMov": ("COMBATANT_OFFSET_MOV_CURRENT", "SetCombatantByte"),
        "SetBaseResistance": ("COMBATANT_OFFSET_RESIST_BASE", "SetCombatantWord"),
        "SetCurrentResistance": ("COMBATANT_OFFSET_RESIST_CURRENT", "SetCombatantWord"),
        "SetBaseProwess": ("COMBATANT_OFFSET_PROWESS_BASE", "SetCombatantByte"),
        "SetCurrentProwess": ("COMBATANT_OFFSET_PROWESS_CURRENT", "SetCombatantByte"),
        "SetStatusEffects": ("COMBATANT_OFFSET_STATUSEFFECTS", "SetCombatantWord"),
        "SetCombatantX": ("COMBATANT_OFFSET_X", "SetCombatantByte"),
        "SetCombatantY": ("COMBATANT_OFFSET_Y", "SetCombatantByte"),
        "SetCurrentExp": ("COMBATANT_OFFSET_EXP", "SetCombatantByte"),
        "SetMovetypeAndAiCommandset": (
            "COMBATANT_OFFSET_MOVETYPE_AND_AI_COMMANDSET",
            "SetCombatantByte",
        ),
        "SetActivationBitfield": ("COMBATANT_OFFSET_ACTIVATION_BITFIELD", "SetCombatantWord"),
        "SetEnemyIndex": ("COMBATANT_OFFSET_ENEMY_INDEX", "SetCombatantByte"),
    }
    clamp_specs = {
        "IncreaseLevel": (
            "increase",
            "COMBATANT_OFFSET_LEVEL",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_LEVEL",
        ),
        "IncreaseMaxHp": (
            "increase",
            "COMBATANT_OFFSET_HP_MAX",
            "IncreaseAndClampWord",
            "CHAR_STATCAP_HP",
        ),
        "IncreaseMaxMp": (
            "increase",
            "COMBATANT_OFFSET_MP_MAX",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_MP",
        ),
        "IncreaseBaseAtt": (
            "increase",
            "COMBATANT_OFFSET_ATT_BASE",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_ATT",
        ),
        "IncreaseCurrentAtt": (
            "increase",
            "COMBATANT_OFFSET_ATT_CURRENT",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_ATT",
        ),
        "IncreaseBaseDef": (
            "increase",
            "COMBATANT_OFFSET_DEF_BASE",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_DEF",
        ),
        "IncreaseCurrentDef": (
            "increase",
            "COMBATANT_OFFSET_DEF_CURRENT",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_DEF",
        ),
        "IncreaseBaseAgi": (
            "increase",
            "COMBATANT_OFFSET_AGI_BASE",
            "IncreaseAndClamp7Bits",
            "CHAR_STATCAP_AGI_BASE",
        ),
        "IncreaseCurrentAgi": (
            "increase",
            "COMBATANT_OFFSET_AGI_CURRENT",
            "IncreaseAndClamp7Bits",
            "CHAR_STATCAP_AGI_CURRENT",
        ),
        "IncreaseBaseMov": (
            "increase",
            "COMBATANT_OFFSET_MOV_BASE",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_MOV",
        ),
        "IncreaseCurrentMov": (
            "increase",
            "COMBATANT_OFFSET_MOV_CURRENT",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_MOV",
        ),
        "IncreaseExp": (
            "increase",
            "COMBATANT_OFFSET_EXP",
            "IncreaseAndClampByte",
            "CHAR_STATCAP_EXP",
        ),
        "IncreaseKills": ("increase", "COMBATANT_OFFSET_ALLY_KILLS", "IncreaseAndClampWord", None),
        "IncreaseDefeats": (
            "increase",
            "COMBATANT_OFFSET_ALLY_DEFEATS",
            "IncreaseAndClampWord",
            None,
        ),
        "DecreaseCurrentAtt": (
            "decrease",
            "COMBATANT_OFFSET_ATT_CURRENT",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_ATT",
        ),
        "DecreaseBaseDef": (
            "decrease",
            "COMBATANT_OFFSET_DEF_BASE",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_DEF",
        ),
        "DecreaseCurrentDef": (
            "decrease",
            "COMBATANT_OFFSET_DEF_CURRENT",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_DEF",
        ),
        "DecreaseBaseAgi": (
            "decrease",
            "COMBATANT_OFFSET_AGI_BASE",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_AGI_DECREASING",
        ),
        "DecreaseCurrentAgi": (
            "decrease",
            "COMBATANT_OFFSET_AGI_CURRENT",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_AGI_DECREASING",
        ),
        "DecreaseBaseMov": (
            "decrease",
            "COMBATANT_OFFSET_MOV_BASE",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_MOV",
        ),
        "DecreaseCurrentMov": (
            "decrease",
            "COMBATANT_OFFSET_MOV_CURRENT",
            "DecreaseAndClampByte",
            "CHAR_STATCAP_MOV",
        ),
    }
    special_current = {
        "IncreaseCurrentHp": (
            "increase",
            "COMBATANT_OFFSET_HP_CURRENT",
            "IncreaseAndClampWord",
            "COMBATANT_OFFSET_HP_MAX",
            "move.w",
        ),
        "IncreaseCurrentMp": (
            "increase",
            "COMBATANT_OFFSET_MP_CURRENT",
            "IncreaseAndClampByte",
            "COMBATANT_OFFSET_MP_MAX",
            "move.b",
        ),
        "DecreaseCurrentHp": (
            "decrease",
            "COMBATANT_OFFSET_HP_CURRENT",
            "DecreaseAndClampWord",
            "COMBATANT_OFFSET_HP_MAX",
            "move.w",
        ),
        "DecreaseCurrentMp": (
            "decrease",
            "COMBATANT_OFFSET_MP_CURRENT",
            "DecreaseAndClampByte",
            "COMBATANT_OFFSET_MP_MAX",
            "move.b",
        ),
    }
    expected = (
        {"LoadAllyName", "SetMoveOrders", "SetTriggerRegions"}
        | set(direct_specs)
        | set(clamp_specs)
        | set(special_current)
    )
    if expected != set(names):
        raise ValueError("combatant mutation wrapper coverage drift")

    def field(records: list[dict[str, Any]], name: str, helper: str) -> dict[str, Any]:
        load = next(
            (
                index
                for index, record in enumerate(records)
                if record["opcode"] == "moveq"
                and len(record["operands"]) == 2
                and record["operands"][1] == "d7"
            ),
            None,
        )
        if load is None:
            raise ValueError(f"combatant mutation field load drift: {name}")
        constant = records[load]["operands"][0].removeprefix("#")
        helper_index = call(records, f"{name} helper", helper)
        return {
            "constant": constant,
            "value": constants[constant],
            "loadInstructionIndex": load,
            "helperCallInstructionIndex": helper_index,
            "helper": helper,
        }

    def preserved(records: list[dict[str, Any]], name: str) -> dict[str, Any]:
        saves = [
            index
            for index, record in enumerate(records)
            if record["opcode"] == "movem.l" and record["operands"][-1:] == ["-(sp)"]
        ]
        if len(saves) != 1:
            raise ValueError(f"combatant mutation save drift: {name}")
        save = saves[0]
        registers = records[save]["operands"][0]
        restore = exact(records, f"{name} restore", "movem.l", ["(sp)+", registers])
        if not save < restore < final_terminal(records, name):
            raise ValueError(f"combatant mutation restore drift: {name}")
        return {
            "registers": registers,
            "saveInstructionIndex": save,
            "restoreInstructionIndex": restore,
            "terminalInstructionIndex": final_terminal(records, name),
        }

    wrappers: dict[str, Any] = {}
    for name, (constant, helper) in direct_specs.items():
        records = routines[name]
        if records[0]["opcode"] != "movem.l" or records[0]["operands"][0] != "d7-a0":
            raise ValueError(f"combatant mutation direct preserve drift: {name}")
        item = field(records, name, helper)
        if (item["constant"], item["helper"]) != (constant, helper):
            raise ValueError(f"combatant mutation direct specification drift: {name}")
        wrappers[name] = {
            "kind": "directSet",
            "routineAddress": addresses[name],
            "selector": {"register": "d0", "widthBits": 8},
            "value": {"register": "d1", "widthBits": 8 if helper.endswith("Byte") else 16},
            "field": item,
            "preserveRestore": preserved(records, name),
        }
    for name, (kind, constant, helper, maximum) in clamp_specs.items():
        records = routines[name]
        item = field(records, name, helper)
        if (item["constant"], item["helper"]) != (constant, helper):
            raise ValueError(f"combatant mutation clamp specification drift: {name}")
        min_index = exact(records, f"{name} minimum", "clr.w", ["d5"])
        if maximum is None:
            max_index = exact(records, f"{name} maximum", "move.w", ["#9999", "d6"])
            max_constant = None
            max_value = 9999
        else:
            opcode = (
                "moveq"
                if maximum in {"CHAR_STATCAP_AGI_BASE", "CHAR_STATCAP_AGI_CURRENT"}
                else "move.w"
            )
            max_index = exact(records, f"{name} maximum", opcode, [f"#{maximum}", "d6"])
            max_constant = maximum
            max_value = constants[maximum]
        guarded = name in {"IncreaseKills", "IncreaseDefeats"}
        branch = None
        if guarded:
            test = exact(records, f"{name} selector test", "tst.b", ["d0"])
            branch = exact(records, f"{name} return branch", "blt.s", ["@Return"])
            if not test < branch < min_index:
                raise ValueError(f"combatant mutation guarded order drift: {name}")
        wrappers[name] = {
            "kind": kind,
            "routineAddress": addresses[name],
            "selector": {"register": "d0", "widthBits": 8},
            "delta": {
                "register": "d1",
                "widthBits": 8 if helper.endswith(("Byte", "7Bits")) else 16,
            },
            "field": item,
            "clampArguments": {
                "minimumRegister": "d5",
                "minimumInstructionIndex": min_index,
                "maximumRegister": "d6",
                "maximumConstant": max_constant,
                "maximumValue": max_value,
                "maximumInstructionIndex": max_index,
            },
            "guard": {
                "present": guarded,
                "selectorRegister": "d0" if guarded else None,
                "testOpcode": "tst.b" if guarded else None,
                "selectorTestInstructionIndex": exact(
                    records, f"{name} selector test", "tst.b", ["d0"]
                )
                if guarded
                else None,
                "branchOpcode": "blt.s" if guarded else None,
                "returnBranchInstructionIndex": branch,
                "returnBranchTarget": "@Return" if guarded else None,
                "returnBranchTaken": True if guarded else None,
                "returnPathTerminalInstructionIndex": final_terminal(records, name),
            },
            "preserveRestore": preserved(records, name),
        }
    for name, (kind, _constant, helper, maximum_field, read_opcode) in special_current.items():
        records = routines[name]
        item = field(records, name, helper)
        if (item["constant"], item["helper"]) != (_constant, helper):
            raise ValueError(f"combatant mutation current-value specification drift: {name}")
        entry_call = call(records, f"{name} entry address", "GetCombatantEntryAddress")
        max_read = exact(
            records, f"{name} maximum read", read_opcode, [f"{maximum_field}(a0)", "d6"]
        )
        min_index = exact(records, f"{name} minimum", "clr.w", ["d5"])
        if (
            not min_index
            < entry_call
            < max_read
            < item["loadInstructionIndex"]
            < item["helperCallInstructionIndex"]
        ):
            raise ValueError(f"combatant mutation current-value order drift: {name}")
        wrappers[name] = {
            "kind": kind,
            "routineAddress": addresses[name],
            "selector": {"register": "d0", "widthBits": 8},
            "delta": {"register": "d1", "widthBits": 8 if read_opcode == "move.b" else 16},
            "field": item,
            "clampArguments": {
                "minimumRegister": "d5",
                "minimumInstructionIndex": min_index,
                "maximumRegister": "d6",
                "maximumFieldConstant": maximum_field,
                "maximumFieldValue": constants[maximum_field],
                "maximumReadOpcode": read_opcode,
                "maximumReadInstructionIndex": max_read,
            },
            "entryAddressCallInstructionIndex": entry_call,
            "preserveRestore": preserved(records, name),
        }
    for name, constant, helper, shift, mask, width in (
        (
            "SetMoveOrders",
            "COMBATANT_OFFSET_MOVE_ORDERS",
            "SetCombatantWord",
            "BYTE_SHIFT_COUNT",
            "BYTE_MASK",
            16,
        ),
        (
            "SetTriggerRegions",
            "COMBATANT_OFFSET_TRIGGER_REGIONS",
            "SetCombatantByte",
            "NIBBLE_SHIFT_COUNT",
            "BYTE_LOWER_NIBBLE_MASK",
            8,
        ),
    ):
        records = routines[name]
        item = field(records, name, helper)
        if (item["constant"], item["helper"]) != (constant, helper):
            raise ValueError(f"combatant mutation packed specification drift: {name}")
        shift_index = exact(
            records, f"{name} shift", f"lsl.{'w' if width == 16 else 'b'}", [f"#{shift}", "d1"]
        )
        mask_index = exact(
            records, f"{name} mask", f"andi.{'w' if width == 16 else 'b'}", [f"#{mask}", "d2"]
        )
        merge_index = exact(
            records, f"{name} merge", f"or.{'w' if width == 16 else 'b'}", ["d2", "d1"]
        )
        if (
            not shift_index
            < mask_index
            < merge_index
            < item["loadInstructionIndex"]
            < item["helperCallInstructionIndex"]
        ):
            raise ValueError(f"combatant mutation packed order drift: {name}")
        effective_input_width = constants[mask].bit_count()
        if constants[shift] != effective_input_width:
            raise ValueError(f"combatant mutation packed input width drift: {name}")
        wrappers[name] = {
            "kind": "directSet",
            "routineAddress": addresses[name],
            "selector": {"register": "d0", "widthBits": 8},
            "field": item,
            "packedMerge": {
                "highInput": {
                    "register": "d1",
                    "effectiveInputWidthBits": effective_input_width,
                    "shiftOpcode": f"lsl.{'w' if width == 16 else 'b'}",
                    "shiftOperationWidthBits": width,
                },
                "lowInput": {
                    "register": "d2",
                    "effectiveInputWidthBits": effective_input_width,
                    "maskOpcode": f"andi.{'w' if width == 16 else 'b'}",
                    "maskOperationWidthBits": width,
                },
                "shiftConstant": shift,
                "shiftValue": constants[shift],
                "shiftInstructionIndex": shift_index,
                "maskConstant": mask,
                "maskValue": constants[mask],
                "maskInstructionIndex": mask_index,
                "mergeInstructionIndex": merge_index,
                "resultRegister": "d1",
                "mergeOpcode": f"or.{'w' if width == 16 else 'b'}",
                "mergeOperationWidthBits": width,
                "storedWidthBits": width,
            },
            "preserveRestore": preserved(records, name),
        }
    load = routines["LoadAllyName"]
    load_test = exact(load, "LoadAllyName null test", "tst.b", ["(a0)"])
    load_branch = exact(load, "LoadAllyName null branch", "beq.s", ["@Return"])
    load_pointer_copy = exact(load, "LoadAllyName pointer copy", "lea", ["(a0)", "a1"])
    load_entry = call(load, "LoadAllyName entry address", "GetCombatantEntryAddress")
    load_counter = exact(
        load, "LoadAllyName counter", "moveq", ["#ALLYNAME_CHARACTERS_COUNTER", "d0"]
    )
    load_copy = exact(load, "LoadAllyName copy", "move.b", ["(a1)+", "(a0)+"])
    load_loop = exact(load, "LoadAllyName loop", "dbf", ["d0", "@Loop"])
    load_save = exact(load, "LoadAllyName save", "movem.l", ["d0/a0-a1", "-(sp)"])
    load_restore = exact(load, "LoadAllyName restore", "movem.l", ["(sp)+", "d0/a0-a1"])
    load_terminal = final_terminal(load, "LoadAllyName")
    load_copy_width = 8 if load[load_copy]["opcode"].endswith(".b") else 16
    load_copy_iterations = constants["ALLYNAME_CHARACTERS_COUNTER"] + 1
    if not (
        load_test
        < load_branch
        < load_save
        < load_pointer_copy
        < load_entry
        < load_counter
        < load_copy
        < load_loop
        < load_restore
        < load_terminal
    ):
        raise ValueError("combatant mutation LoadAllyName order drift")
    wrappers["LoadAllyName"] = {
        "kind": "loadAllyName",
        "routineAddress": addresses["LoadAllyName"],
        "input": {"sourceAddressRegister": "a0", "combatantSelectorRegister": "d0"},
        "nullPath": {
            "testInstructionIndex": load_test,
            "branchInstructionIndex": load_branch,
            "branchTarget": "@Return",
            "branchTaken": True,
            "terminalInstructionIndex": load_terminal,
        },
        "preserveRestore": {
            "registers": "d0/a0-a1",
            "saveInstructionIndex": load_save,
            "restoreInstructionIndex": load_restore,
            "terminalInstructionIndex": load_terminal,
        },
        "sourcePointerCopy": {
            "sourceRegister": "a0",
            "destinationRegister": "a1",
            "instructionIndex": load_pointer_copy,
        },
        "entryAddressCallInstructionIndex": load_entry,
        "copy": {
            "sourceAddressRegister": "a1",
            "destinationAddressRegister": "a0",
            "counterRegister": "d0",
            "counterConstant": "ALLYNAME_CHARACTERS_COUNTER",
            "counterValue": constants["ALLYNAME_CHARACTERS_COUNTER"],
            "copyIterations": load_copy_iterations,
            "copyWidthBits": load_copy_width,
            "copiedBytes": load_copy_iterations * (load_copy_width // 8),
            "counterInstructionIndex": load_counter,
            "copyInstructionIndex": load_copy,
            "loopInstructionIndex": load_loop,
        },
        "terminalInstructionIndex": final_terminal(load, "LoadAllyName"),
    }
    helper_names = sorted(
        {item["field"]["helper"] for item in wrappers.values() if "field" in item}
    )
    dependencies: dict[str, Any] = {}
    helper_widths = {
        "SetCombatantByte": 8,
        "SetCombatantWord": 16,
        "IncreaseAndClampByte": 8,
        "IncreaseAndClamp7Bits": 8,
        "IncreaseAndClampWord": 16,
        "DecreaseAndClampByte": 8,
        "DecreaseAndClampWord": 16,
    }
    for name in helper_names:
        records = _shop_instruction_records(section(helpers_source, name))
        entry_index = call(records, f"{name} dependency entry", "GetCombatantEntryAddress")
        writes = [
            index
            for index, record in enumerate(records)
            if record["opcode"].startswith("move.") and record["operands"][-1] == "(a0,d7.w)"
        ]
        if len(writes) != 1:
            raise ValueError(f"combatant mutation dependency write drift: {name}")
        write = writes[0]
        width = 8 if records[write]["opcode"].endswith(".b") else 16
        if helper_widths[name] != width:
            raise ValueError(f"combatant mutation dependency width drift: {name}")
        access_mode = "writeOnly" if name.startswith("Set") else "readModifyWrite"
        reads = [
            index
            for index, record in enumerate(records)
            if record["operands"][:1] == ["(a0,d7.w)"]
        ]
        if (access_mode == "writeOnly" and reads) or (
            access_mode == "readModifyWrite" and len(reads) != 1
        ):
            raise ValueError(f"combatant mutation dependency read drift: {name}")
        read = reads[0] if reads else None
        read_width = None if read is None else (8 if records[read]["opcode"].endswith(".b") else 16)
        if read_width is not None and read_width != width:
            raise ValueError(f"combatant mutation dependency read width drift: {name}")
        terminal_index = final_terminal(records, name)
        dependencies[name] = {
            "routineAddress": address(name),
            "operations": records,
            "entryAddressCallInstructionIndex": entry_index,
            "input": {
                "selectorRegister": "d0",
                "deltaOrValueRegister": "d1",
                "fieldOffsetRegister": "d7",
                "minimumRegister": None if access_mode == "writeOnly" else "d5",
                "maximumRegister": None if access_mode == "writeOnly" else "d6",
            },
            "accessMode": access_mode,
            "fieldReadInstructionIndex": read,
            "fieldReadOpcode": None if read is None else records[read]["opcode"],
            "fieldReadWidthBits": read_width,
            "fieldWriteInstructionIndex": write,
            "fieldWriteOpcode": records[write]["opcode"],
            "storedWidthBits": width,
            "result": None if access_mode == "writeOnly" else {"resultRegister": "d1"},
            "terminalInstructionIndex": terminal_index,
        }
    targets = set(names)
    aliases = _shop_jump_aliases(disasm, targets)
    alias_targets = {alias: fact["effectiveTarget"] for alias, fact in aliases.items()}
    internal = {
        "code/common/stats/combatantstats_2.asm": _shop_direct_call_occurrences(
            source_path, alias_targets, targets
        )
    }
    external = {
        path.relative_to(disasm).as_posix(): occurrences
        for path in sorted((disasm / "code").rglob("*.asm"), key=lambda value: value.as_posix())
        if path != source_path
        if (occurrences := _shop_direct_call_occurrences(path, alias_targets, targets))
    }

    def totals(callers: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
        return {
            target: sum(
                item["siteCount"]
                for occurrences in callers.values()
                for item in occurrences
                if item["effectiveTarget"] == target
            )
            for target in names
        }

    classification_counts = {
        kind: sum(wrapper["kind"] == kind for wrapper in wrappers.values())
        for kind in ("loadAllyName", "directSet", "increase", "decrease")
    }
    if classification_counts != {
        "loadAllyName": 1,
        "directSet": 27,
        "increase": 16,
        "decrease": 9,
    }:
        raise ValueError("combatant mutation classification count drift")

    return {
        "sourcePath": "code/common/stats/combatantstats_2.asm",
        "sourceRange": _caravan_range(source, "code/common/stats/combatantstats_2.asm"),
        "routineOrder": names,
        "routineAddresses": addresses,
        "constants": constants,
        "routineOperations": routines,
        "wrappers": wrappers,
        "classificationCounts": classification_counts,
        "dependencyAbi": {
            "sourcePath": "code/common/stats/combatantstats_3.asm",
            "routines": dependencies,
        },
        "jumpInterfaceAliases": aliases,
        "internalDirectCallerOccurrences": internal,
        "internalEffectiveDirectCallSiteCounts": totals(internal),
        "externalDirectCallerOccurrences": external,
        "externalEffectiveDirectCallSiteCounts": totals(external),
        "staticBoundary": {"clampAlgorithm": "unknown", "callerAndRuntimeOutcome": "unknown"},
    }


def _stats_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    combatant_getters = _combatant_getter_contract(disasm)
    combatant_mutations = _combatant_mutation_contract(disasm)
    _require_ordered_fragments(
        root / "gameflags.asm",
        [
            "andi.l  #FLAG_MASK,d1",
            "divu.w  #8,d1",
            "lea     ((GAME_FLAGS-$1000000)).w,a0",
            "adda.w  d1,a0",
            "swap    d1",
            "moveq   #$FFFFFF80,d0",
            "lsr.b   d1,d0",
        ],
    )
    _require_ordered_fragments(
        root / "battleparty.asm",
        [
            "lea     ((TARGETS_LIST-$1000000)).w,a2",
            "lea     ((BATTLE_PARTY_MEMBERS-$1000000)).w,a3",
            "lea     ((RESERVE_MEMBERS-$1000000)).w,a4",
            "addi.w  #FORCEMEMBER_JOINED_FLAGS_START,d1",
            "bsr.s   CheckFlag",
            "addi.w  #FORCEMEMBER_ACTIVE_FLAGS_START,d1",
            "bsr.s   CheckFlag",
            "move.w  d2,((TARGETS_LIST_LENGTH-$1000000)).w",
            "move.w  d3,((BATTLE_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "move.w  d4,((OTHER_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "cmpi.w  #FORCE_MAX_SIZE,((BATTLE_PARTY_MEMBERS_NUMBER-$1000000)).w",
            "bsr.w   JoinBattleParty",
        ],
    )
    _require_ordered_fragments(
        root / "caravaninventory.asm",
        [
            "moveq   #CARAVAN_MAX_ITEMS_NUMBER_MINUS_ONE,d0",
            "cmp.w   ((CARAVAN_ITEMS_NUMBER-$1000000)).w,d0",
            "andi.w  #ITEMENTRY_MASK_INDEX,d1",
            "move.b  d1,(a0,d0.w)",
            "addq.w  #1,((CARAVAN_ITEMS_NUMBER-$1000000)).w",
            "subq.w  #1,((CARAVAN_ITEMS_NUMBER-$1000000)).w",
            "move.b  #ITEM_NOTHING,(a0)",
        ],
    )
    _require_ordered_fragments(
        root / "dealsinventory.asm",
        [
            "cmpi.b  #DEALS_MAX_NUMBER_PER_ITEM,d2",
            "add.b   d0,(a0)",
            "tst.b   d2",
            "sub.b   d0,(a0)",
            "andi.l  #ITEMENTRY_MASK_INDEX,d1",
            "divu.w  #2,d1",
            "btst    #DEALS_BIT_REMAINDER,d1",
            "moveq   #DEALS_ADD_AMOUNT_EVEN,d0",
            "moveq   #DEALS_ADD_AMOUNT_ODD,d0",
        ],
    )
    _require_ordered_fragments(
        root / "getcombatanttype.asm",
        [
            "btst    #COMBATANT_BIT_ENEMY,d0",
            "bsr.w   GetClass",
            "move.b  table_ClassTypes(pc,d1.w),d1",
            "mulu.w  #COMBATANT_ALLIES_NUMBER,d1",
            "add.w   d0,d1",
            "bset    #15,d1",
            "bsr.s   GetEnemy",
        ],
    )
    _require_ordered_fragments(
        root / "spellstats.asm",
        [
            "andi.w  #SPELLENTRY_MASK_INDEX,d1",
            "movea.l (p_table_SpellNames).l,a0",
            "bsr.w   FindName",
            "movea.l (p_table_SpellDefinitions).l,a0",
            "movea.l (p_table_SpellDefinitions).l,a0",
            "move.w  #1,d2",
            "andi.w  #SPELLENTRY_MASK_INDEX,d4",
            "lsr.w   #SPELLENTRY_OFFSET_LV,d5",
            "move.b  d1,(a0)",
            "move.w  #2,d2",
            "move.b  d1,-(a0)",
            "clr.w   d2",
        ],
    )
    _require_ordered_fragments(
        root / "newgame.asm",
        [
            "bsr.w   InitializeGameSettings",
            "bsr.w   InitializeAllyCombatantEntry",
            "moveq   #GAMESTART_GOLD,d1",
            "bsr.w   SetGold",
            "moveq   #ALLY_BOWIE,d0",
            "bsr.w   JoinForce",
            "move.l  #LONGWORD_SPELLS_INITVALUE,COMBATANT_OFFSET_SPELLS(a1)",
            "bsr.w   LoadAllyClassData",
            "bsr.w   InitializeAllyStats",
            "bsr.w   UpdateCombatantStats",
            "lea     ((GAME_FLAGS-$1000000)).w,a0",
            "lea     ((DEALS_ITEMS-$1000000)).w,a0",
            "lea     ((CARAVAN_ITEMS-$1000000)).w,a0",
            "move.b  #2,((MESSAGE_SPEED-$1000000)).w",
        ],
    )
    return {
        "flags": {
            "flagIndexMasked": True,
            "bitsPerByte": 8,
            "maskStartsAtBit7": True,
            "checkSetAndClearShareGetFlag": True,
        },
        "party": {
            "joinedAndActiveUseSeparateFlagRanges": True,
            "updateBuildsForceActiveAndReserveLists": True,
            "joinForceAutoActivatesBelowForceMax": True,
            "leaveForceMovesCombatantOffMap": True,
        },
        "inventories": {
            "caravanMasksItemStatusBits": True,
            "caravanFullAddIsIgnored": True,
            "caravanRemovalCompactsAndWritesNothing": True,
            "dealsStoresTwoItemCountsPerByte": True,
            "dealsCountSaturates": True,
            "dealsRemoveAtZeroIsIgnored": True,
        },
        "combatantType": {
            "allySetsHighBit": True,
            "allyEncodesClassTypeTimesAllyCountPlusIndex": True,
            "enemyReturnsEnemyIndex": True,
            "upstreamMarksFeatureUnused": True,
        },
        "spells": {
            "definitionMissDefaultsToFirstEntry": True,
            "learnSuccess": 0,
            "sameOrHigherKnownFailure": 1,
            "noRoomFailure": 2,
            "higherLevelReplacesKnownEntry": True,
        },
        "newGame": {
            "settingsBeforeAllies": True,
            "allAlliesInitialized": True,
            "startingGoldThenBowieJoin": True,
            "allySpellSlotsInitializedToNothing": True,
            "classDataThenStatsThenDerivedStats": True,
            "clearsFlagsDealsAndCaravan": True,
            "defaultMessageSpeed": 2,
        },
        "inventoryBoundary": {
            "combatantGettersAndSettersInventoried": True,
            "itemDefinitionHelpersInventoried": True,
            "existingLevelGoldAndDerivedStatRailsRetained": True,
            "callerDependentUiAndItemEffectsRemainQueued": True,
        },
        "combatantGetterContract": combatant_getters,
        "combatantMutationContract": combatant_mutations,
    }


def build_stats_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {
        Path(row["path"]).relative_to(SOURCE_ROOT).as_posix(): row["globalLabels"][0]
        for row in files
    } != REPRESENTATIVE_SYMBOLS:
        raise ValueError("common stats file/symbol set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    ]
    layout = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((disasm / "layout").glob("*.asm"))
    )
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
        "excludedAlternateFileCount": len(ALTERNATE_SOURCES),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "statsFacts": _stats_facts(disasm),
        "alternateSources": [
            _alternate_source_fact(disasm, alternate, canonical, layout)
            for alternate, canonical in ALTERNATE_SOURCES.items()
        ],
        "files": files,
    }


def verify_stats_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_stats_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="common stats static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("common stats provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("common stats summary drift")
    by_relative = {
        Path(row["path"]).relative_to(SOURCE_ROOT).as_posix(): row for row in output["files"]
    }
    for relative, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_relative[relative]["globalLabels"]:
            raise ValueError(f"common stats symbol drift: {relative}::{symbol}")
    if output["statsFacts"] != fixture["expected"]["statsFacts"]:
        raise ValueError("common stats model drift")
    if output["alternateSources"] != fixture["expected"]["alternateSources"]:
        raise ValueError("common stats alternate-source drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("common stats canonical hash drift")
    destination = output_path or repo_path("local/derived/common-stats-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "ExcludedAlternates": output["summary"]["excludedAlternateFileCount"],
        "Status": "PASS",
    }
