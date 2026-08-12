"""One-launch caller/return observation for the four built service-menu entries.

This rail is intentionally narrower than the service state machines.  It runs
the outer saved-register frame of Shop, Church, Caravan, and Blacksmith, then
uses session-ROM shims to take the source cancel/return seam.  Text, window,
input, action, transaction, and persistence bodies do not execute.
"""

from __future__ import annotations

import re
import tempfile
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import mega_drive_checksum
from sf2tool.source_text import read_upstream_text

FIXTURE = repo_path("tests/fixtures/h3/service-menu-entry-return-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/service-menu-entry-return-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/service-menu-entry-return-observation.schema.json")
FAILURE_SCHEMA = repo_path("schemas/h3/service-menu-entry-return-callback-failure.schema.json")
OBSERVER = repo_path("tools/bizhawk/service_menu_entry_return_observer.lua")
H2_FIXTURE = repo_path("tests/fixtures/h2/common-menus-static-v1.json")

OWNER = "service-menu-entry-return"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OWNER)
UPSTREAM = repo_path("local/upstream/SF2DISASM")
DISASM = UPSTREAM / "disasm"
LISTING_RELATIVE = Path("build/sf2build-h1.lst")

SERVICES = ("shop", "church", "caravan", "blacksmith")
TRANSFER_KINDS = ("returning-call", "tail-transfer")
SERVICE_SYMBOLS = {
    "shop": "ShopMenu",
    "church": "ChurchMenu",
    "caravan": "CaravanMenu",
    "blacksmith": "BlacksmithMenu",
}
ALIASES = {service: f"j_{symbol}" for service, symbol in SERVICE_SYMBOLS.items()}
SOURCE_PATHS = {
    "shop": Path("code/common/menus/shop/shopactions.asm"),
    "church": Path("code/common/menus/church/churchactions_1.asm"),
    "caravan": Path("code/common/menus/caravan/caravanactions_1.asm"),
    "blacksmith": Path("code/common/menus/blacksmith/blacksmithactions.asm"),
}
JUMP_INTERFACE = Path("code/common/tech/jumpinterfaces/s05_jumpinterface.asm")

CASE_ORDER = (
    "context-menu-church",
    "context-menu-shop",
    "context-menu-blacksmith",
    "exploration-vint-church",
    "exploration-vint-caravan",
    "battle-test-church-main",
    "battle-test-shop",
    "battle-test-caravan",
    "battle-test-church-preserved-registers",
    "map-entity-church",
    "map-entity-shop",
    "map-entity-caravan",
    "map-entity-tail-church",
    "map-entity-tail-shop",
    "map-entity-tail-blacksmith",
)

FAMILY_ORDER = ("context-menu", "exploration-vint", "battle-test", "map-entity")

# This is a rule, not a list of manually selected program counters: every
# positive family × service × transfer-kind cell contributes its lowest-addressed direct transfer.
# BattleTest's second Church caller uniquely runs the source MOVEM save/restore
# pair around the call, so it is the one additional source-shaped stack case.
REPRESENTATIVE_SELECTION_RULE = {
    "positiveCell": "lowest-call-site-address",
    "additionalStackCase": "battle-test-church-movem-save-restore",
}

SERVICE_STUB_ADDRESS = 0xFF6D00
BLACKSMITH_RETURN_STUB_ADDRESS = 0xFF6D10
RESULT_STUB_ADDRESS = 0xFF6D20
OUTER_RETURN_TRAMPOLINE_ADDRESS = 0xFF6D30
HARNESS_BASE_ADDRESS = 0xFF6800
HARNESS_STRIDE = 32
HARNESS_RESULT_OFFSET = 24
STACK_TOP = 0xFFFF00
CALLER_FRAME_ADDRESS = 0xFF6A00
CURRENT_PORTRAIT_ADDRESS = 0xFFB6F6
CALLER_SENTINELS = {"d0": 0x11, "d1": 0x22, "d2": 0x33}

_DIRECT_ALIAS_TRANSFER = re.compile(
    r"^\s*(?P<opcode>jsr|bsr|jmp)(?:\.[bswl])?\s+(?P<target>j_(?:ShopMenu|ChurchMenu|CaravanMenu|BlacksmithMenu))\b",
    re.IGNORECASE,
)
_H1_ADDRESS = re.compile(r"^(?P<address>[0-9A-F]{8})\s+(?P<body>.*)$")
_LUA_DISPATCH_ROLE = re.compile(r'entry\.role=="(?P<role>[^"]+)"')

REGISTERED_CALLBACK_ROLES = frozenset(
    {
        "bootstrap-check-sram",
        "case-entry",
        "caller-call-site",
        "tail-transfer-site",
        "service-entry",
        "generated-service-cancel-stub",
        "generated-blacksmith-return-stub",
        "outer-caller-return",
        "outer-rts-harness-return",
        "caller-result",
    }
)


def _lua_register_roles(source: str) -> set[str]:
    """Parse every Lua ``register_exec`` role expression, including ``and/or`` branches."""
    roles: set[str] = set()
    cursor = 0
    while True:
        start = source.find("register_exec(", cursor)
        if start < 0:
            break
        position = start + len("register_exec(")
        depth = 1
        quote: str | None = None
        escaped = False
        while position < len(source) and depth:
            character = source[position]
            if quote is not None:
                if escaped:
                    escaped = False
                elif character == "\\":
                    escaped = True
                elif character == quote:
                    quote = None
            elif character in {"'", '"'}:
                quote = character
            elif character == "(":
                depth += 1
            elif character == ")":
                depth -= 1
            position += 1
        if depth:
            raise ValueError("service lifecycle Lua register_exec syntax audit drift")
        expression = source[start + len("register_exec(") : position - 1]
        fields: list[str] = []
        field_start = 0
        field_depth = 0
        quote = None
        escaped = False
        for index, character in enumerate(expression):
            if quote is not None:
                if escaped:
                    escaped = False
                elif character == "\\":
                    escaped = True
                elif character == quote:
                    quote = None
            elif character in {"'", '"'}:
                quote = character
            elif character == "(":
                field_depth += 1
            elif character == ")":
                field_depth -= 1
            elif character == "," and field_depth == 0:
                fields.append(expression[field_start:index])
                field_start = index + 1
        fields.append(expression[field_start:])
        if len(fields) >= 2:
            role_expression = fields[1]
            for match in re.finditer(r'"(?P<role>[^"]+)"', role_expression):
                if re.search(r"==\s*$", role_expression[: match.start()]):
                    continue
                roles.add(match["role"])
        cursor = position
    return roles


def lua_role_contract() -> dict[str, set[str]]:
    """Extract the observer's actual registration/dispatch role vocabulary."""
    source = OBSERVER.read_text(encoding="utf-8")
    registered = _lua_register_roles(source)
    dispatched = {match["role"] for match in _LUA_DISPATCH_ROLE.finditer(source)}
    observation = load_json(OBSERVATION_SCHEMA)["definitions"]["chronology"]["properties"]["role"][
        "enum"
    ]
    failure = load_json(FAILURE_SCHEMA)
    failure_roles = failure["properties"]["role"]["enum"]
    pending_roles = failure["properties"]["pendingCallback"]["properties"]["rolesAtPc"]["items"][
        "enum"
    ]
    return {
        "registered": registered,
        "dispatched": dispatched,
        "observation": set(observation),
        "failure": set(failure_roles),
        "pending": set(pending_roles),
    }


def assert_lua_role_contract() -> None:
    """Close role registration, deterministic dispatch, schemas, and chronologies together."""
    actual = lua_role_contract()
    if actual["registered"] != REGISTERED_CALLBACK_ROLES:
        raise ValueError("service lifecycle Lua registered-role audit drift")
    if actual["dispatched"] != REGISTERED_CALLBACK_ROLES:
        raise ValueError("service lifecycle Lua deterministic-dispatch role audit drift")
    expected_observation = {
        "case-entry",
        "caller-call-site",
        "tail-transfer-site",
        "service-entry",
        "generated-service-cancel-stub",
        "generated-blacksmith-return-stub",
        "outer-caller-return",
        "outer-rts-harness-return",
        "caller-result",
    }
    if actual["observation"] != expected_observation:
        raise ValueError("service lifecycle observation-role audit drift")
    if actual["failure"] < REGISTERED_CALLBACK_ROLES:
        raise ValueError("service lifecycle callback-failure role audit drift")
    if actual["pending"] != REGISTERED_CALLBACK_ROLES:
        raise ValueError("service lifecycle pending-callback role audit drift")


def _normal(text: str) -> str:
    return re.sub(r"\s+", " ", text.split(";", 1)[0].strip()).lower()


def _literal_instruction_rows(source: str, symbol: str) -> list[str]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"service lifecycle source function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"service lifecycle source function end is missing: {symbol}")
    return [
        _normal(line)
        for line in source[start.start() : end].splitlines()
        if _normal(line) and not _normal(line).endswith(":")
    ]


def _require_order(source: str, symbol: str, fragments: tuple[str, ...]) -> None:
    rows = _literal_instruction_rows(source, symbol)
    cursor = 0
    for fragment in fragments:
        expected = _normal(fragment)
        try:
            cursor = rows.index(expected, cursor) + 1
        except ValueError as error:
            raise ValueError(
                f"service lifecycle source guard drift in {symbol}: {fragment}"
            ) from error


def _family(path: str) -> str:
    if path == "code/common/scripting/map/mapscriptengine_2.asm":
        return "context-menu"
    if path == "code/gameflow/exploration/explorationvints.asm":
        return "exploration-vint"
    if path == "code/gameflow/special/battletest.asm":
        return "battle-test"
    if path.startswith("data/maps/entries/"):
        return "map-entity"
    raise ValueError(f"service lifecycle caller is outside the bounded source graph: {path}")


def _service_for_alias(alias: str) -> str:
    for service, known_alias in ALIASES.items():
        if alias == known_alias:
            return service
    raise ValueError(f"unknown service-menu jump alias: {alias}")


def _listing_blocks(listing: str) -> dict[str, list[str]]:
    blocks: dict[str, list[str]] = {}
    current: str | None = None
    for line in listing.splitlines():
        match = re.match(r"^(?:[0-9A-F]{8}\s+)?; ASM FILE (.+?) :\s*$", line)
        if match:
            current = match.group(1).replace("\\", "/")
            blocks.setdefault(current, [])
        elif current is not None:
            blocks[current].append(line)
    return blocks


def _h1_records(lines: list[str]) -> list[dict[str, Any]]:
    records: list[dict[str, Any]] = []
    for line in lines:
        match = _H1_ADDRESS.match(line)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"]).strip()
        if not body or body.endswith(":"):
            continue
        records.append({"address": int(match["address"], 16), "instruction": body})
    return records


def _source_transfers(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    result: dict[str, list[dict[str, Any]]] = {}
    for path in sorted(disasm.rglob("*.asm"), key=lambda item: item.as_posix()):
        relative = path.relative_to(disasm).as_posix()
        transfers: list[dict[str, Any]] = []
        for line_number, raw in enumerate(read_upstream_text(path).splitlines(), start=1):
            match = _DIRECT_ALIAS_TRANSFER.match(raw.split(";", 1)[0])
            if match is not None:
                transfers.append(
                    {
                        "lineNumber": line_number,
                        "instructionTarget": match["target"],
                        "opcode": match["opcode"].lower(),
                    }
                )
        if transfers:
            result[relative] = transfers
    return result


def _caller_inventory(disasm: Path, listing: str) -> list[dict[str, Any]]:
    blocks = _listing_blocks(listing)
    sites: list[dict[str, Any]] = []
    for path, source_transfers in _source_transfers(disasm).items():
        h1_transfers = []
        for index, record in enumerate(_h1_records(blocks.get(path, []))):
            match = _DIRECT_ALIAS_TRANSFER.match(record["instruction"])
            if match is not None:
                h1_transfers.append((index, record, match))
        if len(source_transfers) != len(h1_transfers):
            raise ValueError(f"service lifecycle source/H1 direct-transfer count drift: {path}")
        records = _h1_records(blocks[path])
        for source_transfer, (index, h1, match) in zip(source_transfers, h1_transfers, strict=True):
            if (
                source_transfer["instructionTarget"] != match["target"]
                or source_transfer["opcode"] != match["opcode"].lower()
                or index + 1 >= len(records)
            ):
                raise ValueError(f"service lifecycle direct-transfer H1 identity drift: {path}")
            following = records[index + 1]
            instruction_width = following["address"] - h1["address"]
            if instruction_width != 6:
                raise ValueError(f"service lifecycle direct-transfer width drift: {path}")
            transfer_kind = (
                "tail-transfer" if source_transfer["opcode"] == "jmp" else "returning-call"
            )
            service = _service_for_alias(source_transfer["instructionTarget"])
            sites.append(
                {
                    "siteId": f"{path}:{source_transfer['lineNumber']}:{service}",
                    "family": _family(path),
                    "sourcePath": path,
                    "sourceLineNumber": source_transfer["lineNumber"],
                    "opcode": source_transfer["opcode"],
                    "transferKind": transfer_kind,
                    "instructionWidthBytes": instruction_width,
                    "instructionTarget": source_transfer["instructionTarget"],
                    "effectiveTarget": SERVICE_SYMBOLS[service],
                    "callSiteAddress": h1["address"],
                    "returnAddress": following["address"]
                    if transfer_kind == "returning-call"
                    else None,
                    "returnInstruction": (
                        _normal(following["instruction"])
                        if transfer_kind == "returning-call"
                        else None
                    ),
                    "returnKind": (
                        "rts"
                        if _normal(following["instruction"]).startswith("rts")
                        else "continued-instruction"
                    )
                    if transfer_kind == "returning-call"
                    else "outer-rts-harness",
                }
            )
    return sorted(sites, key=lambda item: item["callSiteAddress"])


def _bind_rom_instruction_bytes(
    inventory: list[dict[str, Any]], rom: bytes, aliases: dict[str, dict[str, Any]]
) -> None:
    """Bind each source/H1 direct transfer to its six ROM opcode-and-alias bytes."""
    opcodes = {"jsr": b"\x4e\xb9", "jmp": b"\x4e\xf9"}
    for site in inventory:
        opcode = site["opcode"]
        expected = opcodes[opcode] + aliases[_service_for_alias(site["instructionTarget"])][
            "instructionTargetAddress"
        ].to_bytes(4, "big")
        actual = rom[
            site["callSiteAddress"] : site["callSiteAddress"] + site["instructionWidthBytes"]
        ]
        if actual != expected:
            raise ValueError(
                f"service lifecycle direct-transfer ROM opcode/target drift: {site['siteId']}"
            )
        site["instructionHex"] = actual.hex().upper()


def _entry_function_records(listing: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"service lifecycle H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"service lifecycle H1 function end is missing: {symbol}")
    return _h1_records(listing[start.start() : end].splitlines())


def _find_record(records: list[dict[str, Any]], fragment: str) -> int:
    expected = _normal(fragment)
    matches = [
        index for index, record in enumerate(records) if _normal(record["instruction"]) == expected
    ]
    if len(matches) != 1:
        raise ValueError(f"service lifecycle H1 instruction identity drift: {fragment}")
    return matches[0]


def _first_after(records: list[dict[str, Any]], index: int, fragment: str) -> int:
    expected = _normal(fragment)
    for cursor in range(index + 1, len(records)):
        if _normal(records[cursor]["instruction"]) == expected:
            return cursor
    raise ValueError(f"service lifecycle H1 post-label identity drift: {fragment}")


def _entry_seams(disasm: Path, listing: str) -> dict[str, Any]:
    control = {
        "shop": (
            "ShopMenu",
            (
                "movem.l d0-a5,-(sp)",
                "link a6,#-22",
                "jsr j_ExecuteDiamondMenu",
                "cmpi.w #-1,d0",
                "beq.s @ExitShop",
                "unlk a6",
                "movem.l (sp)+,d0-a5",
                "rts",
            ),
            "link a6,#-22",
            "jsr j_ExecuteDiamondMenu",
            "unlk a6",
        ),
        "church": (
            "ChurchMenu",
            (
                "movem.l d0-a5,-(sp)",
                "link a6,#-36",
                "jsr j_ExecuteDiamondMenu",
                "cmpi.w #-1,d0",
                "beq.s @ExitMenu",
                "unlk a6",
                "movem.l (sp)+,d0-a5",
                "rts",
            ),
            "link a6,#-36",
            "jsr j_ExecuteDiamondMenu",
            "unlk a6",
        ),
        "caravan": (
            "CaravanMenu",
            (
                "movem.l d0-a5,-(sp)",
                "link a6,#-12",
                "jsr j_ExecuteDiamondMenu",
                "cmpi.w #-1,d0",
                "beq.w @ExitCaravan",
                "unlk a6",
                "movem.l (sp)+,d0-a5",
                "rts",
            ),
            "link a6,#-12",
            "jsr j_ExecuteDiamondMenu",
            "unlk a6",
        ),
        "blacksmith": (
            "BlacksmithMenu",
            (
                "movem.l d0-a5,-(sp)",
                "link a6,#-24",
                "bsr.w ProcessBlacksmithOrders",
                "unlk a6",
                "movem.l (sp)+,d0-a5",
                "rts",
            ),
            "link a6,#-24",
            "bsr.w ProcessBlacksmithOrders",
            "unlk a6",
        ),
    }
    result: dict[str, Any] = {}
    for service, (symbol, guard, link, controlled_call, unlk) in control.items():
        source = read_upstream_text(disasm / SOURCE_PATHS[service])
        _require_order(source, symbol, guard)
        records = _entry_function_records(listing, symbol)
        link_index = _find_record(records, link)
        controlled_index = _find_record(records, controlled_call)
        unlk_index = _find_record(records, unlk)
        if link_index + 1 >= len(records):
            raise ValueError(f"service lifecycle prelude target drift: {service}")
        seam = {
            "entryAddress": records[0]["address"],
            "preludeRedirectAddress": records[link_index + 1]["address"],
            "controlledCallAddress": records[controlled_index]["address"],
            "controlledReturnAddress": records[controlled_index + 1]["address"],
            "epilogueAddress": records[unlk_index]["address"],
        }
        if service == "blacksmith":
            process_records = _entry_function_records(listing, "ProcessBlacksmithOrders")
            update_index = _find_record(process_records, "jsr j_UpdateForce")
            seam["controlledTargetAddress"] = process_records[update_index]["address"]
            seam["preludeTargetAddress"] = seam["controlledCallAddress"]
            seam["controlledResultD0Word"] = None
        else:
            compare_index = _first_after(records, controlled_index, "cmpi.w #-1,d0")
            if records[compare_index]["address"] != seam["controlledReturnAddress"]:
                raise ValueError(f"service lifecycle cancel-result compare seam drift: {service}")
            compare = _normal(records[compare_index]["instruction"])
            match = re.fullmatch(r"cmpi\.w #(?P<value>-?\d+),d0", compare)
            if match is None:
                raise ValueError(f"service lifecycle cancel-result operand ABI drift: {service}")
            cancel_word = int(match["value"]) & 0xFFFF
            seam["controlledTargetAddress"] = SERVICE_STUB_ADDRESS
            seam["preludeTargetAddress"] = records[controlled_index - 4]["address"]
            seam["controlledResultD0Word"] = cancel_word
        result[service] = seam
    return result


def _jump_hex(target: int, *, pad_to: int = 6) -> str:
    value = b"\x4e\xf9" + target.to_bytes(4, "big")
    if pad_to < len(value) or pad_to % 2:
        raise ValueError("service lifecycle invalid JMP patch width")
    return (value + b"\x4e\x71" * ((pad_to - len(value)) // 2)).hex().upper()


def _patches(static: dict[str, Any]) -> list[dict[str, Any]]:
    patches: list[dict[str, Any]] = []
    stubs = {stub["role"]: stub for stub in static["generatedStubs"]}
    for service in SERVICES:
        seam = static["entrySeams"][service]
        patches.append(
            {
                "role": f"{service}-entry-prelude-redirect",
                "address": seam["preludeRedirectAddress"],
                "widthBytes": 8,
                "patchedHex": _jump_hex(seam["preludeTargetAddress"], pad_to=8),
            }
        )
        if service == "blacksmith":
            patches.append(
                {
                    "role": "blacksmith-process-controlled-return-redirect",
                    "address": seam["controlledTargetAddress"],
                    "widthBytes": 6,
                    "patchedHex": _jump_hex(stubs["generated-blacksmith-return-stub"]["address"]),
                }
            )
            patches.append(
                {
                    "role": "blacksmith-caller-continuation-redirect",
                    "address": seam["controlledReturnAddress"],
                    "widthBytes": 8,
                    "patchedHex": _jump_hex(seam["epilogueAddress"], pad_to=8),
                }
            )
        else:
            patches.append(
                {
                    "role": f"{service}-diamond-cancel-service-shim",
                    "address": seam["controlledCallAddress"],
                    "widthBytes": 6,
                    "patchedHex": (
                        b"\x4e\xb9"
                        + stubs["generated-service-cancel-stub"]["address"].to_bytes(4, "big")
                    )
                    .hex()
                    .upper(),
                }
            )
            patches.append(
                {
                    "role": f"{service}-cancel-epilogue-redirect",
                    "address": seam["controlledReturnAddress"] + 4,
                    "widthBytes": 6,
                    "patchedHex": _jump_hex(seam["epilogueAddress"]),
                }
            )
    for case in static["cases"]:
        if (
            case["transferKind"] == "returning-call"
            and case["returnKind"] == "continued-instruction"
            and case["family"] != "context-menu"
        ):
            patches.append(
                {
                    "role": f"caller-continuation-{case['caseId']}",
                    "address": case["continuationRedirectAddress"],
                    "widthBytes": 6,
                    "patchedHex": _jump_hex(RESULT_STUB_ADDRESS),
                }
            )
    ordered = sorted(patches, key=lambda item: item["address"])
    for left, right in zip(ordered, ordered[1:], strict=False):
        if left["address"] + left["widthBytes"] > right["address"]:
            raise ValueError("service lifecycle session-ROM patch overlap")
    return ordered


def _generated_stubs(entry_seams: dict[str, Any]) -> list[dict[str, Any]]:
    """Generate the controlled source-shaped service returns from guarded seam ABI facts."""
    cancel_words = {
        entry_seams[service]["controlledResultD0Word"] for service in ("shop", "church", "caravan")
    }
    if cancel_words != {0xFFFF}:
        raise ValueError("service lifecycle controlled cancel-result ABI drift")
    cancel_word = next(iter(cancel_words))
    cancel_bytes = bytes((0x70, cancel_word & 0xFF, 0x4E, 0x75))
    return [
        {
            "role": "generated-service-cancel-stub",
            "purpose": "controlled-diamond-cancel-return",
            "address": SERVICE_STUB_ADDRESS,
            "widthBytes": len(cancel_bytes),
            "instructionHex": cancel_bytes.hex().upper(),
            "resultD0Word": cancel_word,
        },
        {
            "role": "generated-blacksmith-return-stub",
            "purpose": "controlled-process-blacksmith-orders-return",
            "address": BLACKSMITH_RETURN_STUB_ADDRESS,
            "widthBytes": 2,
            "instructionHex": "4E75",
            "resultD0Word": None,
        },
    ]


def _h2_service_entries() -> list[str]:
    """Consume the accepted common-menus H2 owner without rewriting it."""
    h2 = load_json(H2_FIXTURE)
    if h2["id"] != "sf2-common-menus-static-v1":
        raise ValueError("service lifecycle common-menus H2 fixture identity drift")
    entries = h2["expected"]["menuFacts"]["serviceEntries"]
    if entries != ["BlacksmithMenu", "CaravanMenu", "ChurchMenu", "FieldMenu", "ShopMenu"]:
        raise ValueError("service lifecycle common-menus H2 service-entry boundary drift")
    return entries


def _family_service_counts(inventory: list[dict[str, Any]]) -> dict[str, dict[str, int]]:
    """Return the complete zero-inclusive service × caller-family relation."""
    return {
        family: {
            service: sum(
                site["family"] == family and site["effectiveTarget"] == SERVICE_SYMBOLS[service]
                for site in inventory
            )
            for service in SERVICES
        }
        for family in FAMILY_ORDER
    }


def _family_service_transfer_counts(
    inventory: list[dict[str, Any]],
) -> dict[str, dict[str, dict[str, int]]]:
    """Keep returning calls and tail transfers distinct in every zero-inclusive cell."""
    return {
        family: {
            service: {
                transfer_kind: sum(
                    site["family"] == family
                    and site["effectiveTarget"] == SERVICE_SYMBOLS[service]
                    and site["transferKind"] == transfer_kind
                    for site in inventory
                )
                for transfer_kind in TRANSFER_KINDS
            }
            for service in SERVICES
        }
        for family in FAMILY_ORDER
    }


def _selected_cases(inventory: list[dict[str, Any]], rom: bytes) -> list[dict[str, Any]]:
    """Derive the compact representative matrix from the complete inventory."""
    selected: list[dict[str, Any]] = []
    for family in FAMILY_ORDER:
        family_selected = []
        for service in SERVICES:
            candidates = [
                site
                for site in inventory
                if site["family"] == family
                and site["effectiveTarget"] == SERVICE_SYMBOLS[service]
                and site["transferKind"] == "returning-call"
            ]
            if not candidates:
                continue
            site = min(candidates, key=lambda candidate: candidate["callSiteAddress"])
            case_id = f"{family}-{service}"
            if family == "battle-test" and service == "church":
                case_id = "battle-test-church-main"
            family_selected.append(
                {
                    "caseId": case_id,
                    **site,
                    "service": service,
                    "callerEntryAddress": site["callSiteAddress"],
                    "continuationRedirectAddress": site["returnAddress"],
                }
            )
        selected.extend(sorted(family_selected, key=lambda case: case["callSiteAddress"]))

    stack_candidates = [
        site
        for site in inventory
        if site["family"] == "battle-test"
        and site["effectiveTarget"] == SERVICE_SYMBOLS["church"]
        and site["transferKind"] == "returning-call"
        and rom[site["callSiteAddress"] - 4 : site["callSiteAddress"]] == bytes.fromhex("48E7FFFE")
        and rom[site["returnAddress"] : site["returnAddress"] + 4] == bytes.fromhex("4CDF7FFF")
    ]
    if len(stack_candidates) != 1:
        raise ValueError("service lifecycle saved-register caller rule drift")
    stack_case = stack_candidates[0]
    stack_case_record = {
        "caseId": "battle-test-church-preserved-registers",
        **stack_case,
        "service": "church",
        "callerEntryAddress": stack_case["callSiteAddress"] - 4,
        "continuationRedirectAddress": stack_case["returnAddress"] + 4,
    }
    battle_end = next(
        index + 1
        for index, case in enumerate(selected)
        if case["family"] == "battle-test"
        and case["callSiteAddress"]
        == max(
            candidate["callSiteAddress"]
            for candidate in selected
            if candidate["family"] == "battle-test"
        )
    )
    selected.insert(battle_end, stack_case_record)

    tail_selected: list[dict[str, Any]] = []
    for family in FAMILY_ORDER:
        for service in SERVICES:
            candidates = [
                site
                for site in inventory
                if site["family"] == family
                and site["effectiveTarget"] == SERVICE_SYMBOLS[service]
                and site["transferKind"] == "tail-transfer"
            ]
            if not candidates:
                continue
            site = min(candidates, key=lambda candidate: candidate["callSiteAddress"])
            tail_selected.append(
                {
                    "caseId": f"{family}-tail-{service}",
                    **site,
                    "service": service,
                    "callerEntryAddress": site["callSiteAddress"],
                    "continuationRedirectAddress": None,
                }
            )
    selected.extend(sorted(tail_selected, key=lambda case: case["callSiteAddress"]))
    selected_by_id = {case["caseId"]: case for case in selected}
    actual_order = tuple(case["caseId"] for case in selected)
    if actual_order != CASE_ORDER or len(selected_by_id) != len(CASE_ORDER):
        raise ValueError("service lifecycle representative-selection order drift")
    return selected


def _movem_register_count(instruction: str, *, push: bool) -> int:
    """Parse the source-shaped contiguous MOVEM register range without a magic byte count."""
    register_range = (
        r"(?P<first_kind>[da])(?P<first_index>\d)-"
        r"(?P<last_kind>[da])(?P<last_index>\d)"
    )
    match = re.fullmatch(
        r"movem\.l " + (register_range + r",-\(sp\)" if push else r"\(sp\)\+," + register_range),
        _normal(instruction),
    )
    if match is None:
        raise ValueError("service lifecycle saved-register range identity drift")
    first = (0 if match["first_kind"] == "d" else 8) + int(match["first_index"])
    last = (0 if match["last_kind"] == "d" else 8) + int(match["last_index"])
    if not 0 <= first <= last <= 15:
        raise ValueError("service lifecycle saved-register range bounds drift")
    return last - first + 1


def _outer_return_stack_contract(
    cases: list[dict[str, Any]],
    disasm: Path,
    listing: str,
    symbols: dict[str, int],
    rom: bytes,
) -> None:
    """Derive the caller-owned return word and post-RTS stack seams per selected case."""

    context_records = _entry_function_records(listing, "csc12_executeContextMenu")
    context_push = _find_record(context_records, "move.l a6,-(sp)")
    context_entry = symbols["csc12_executeContextMenu"]
    if (
        context_push != 1
        or _normal(context_records[0]["instruction"]) != "move.w (a6)+,d0"
        or context_records[context_push]["address"] != context_entry + 2
    ):
        raise ValueError("service lifecycle context caller stack-push identity drift")
    size_suffix = _normal(context_records[context_push]["instruction"]).split(".", 1)[1][0]
    operand_bytes = {"b": 1, "w": 2, "l": 4}.get(size_suffix)
    if operand_bytes is None:
        raise ValueError("service lifecycle context caller stack-push width drift")

    for case in cases:
        frame_bytes = 0
        if case["family"] == "context-menu":
            if case["callerEntryAddress"] != context_entry:
                raise ValueError("service lifecycle context caller-entry stack seam drift")
            frame_bytes = operand_bytes
        elif case["caseId"] == "battle-test-church-preserved-registers":
            battle_source = read_upstream_text(disasm / "code/gameflow/special/battletest.asm")
            _require_order(
                battle_source,
                "DebugModeBattleTest",
                ("movem.l d0-a6,-(sp)", "jsr j_ChurchMenu", "movem.l (sp)+,d0-a6"),
            )
            battle_records = _entry_function_records(listing, "DebugModeBattleTest")
            save_index = _find_record(battle_records, "movem.l d0-a6,-(sp)")
            restore_index = _first_after(battle_records, save_index, "movem.l (sp)+,d0-a6")
            if (
                battle_records[save_index]["address"] != case["callSiteAddress"] - 4
                or battle_records[restore_index]["address"] != case["returnAddress"]
            ):
                raise ValueError("service lifecycle saved-register H1 seam drift")
            save = rom[case["callSiteAddress"] - 4 : case["callSiteAddress"]]
            restore = rom[case["returnAddress"] : case["returnAddress"] + 4]
            if save[:2] != bytes.fromhex("48E7") or restore[:2] != bytes.fromhex("4CDF"):
                raise ValueError("service lifecycle saved-register stack opcode drift")
            save_registers = int.from_bytes(save[2:], "big").bit_count()
            restore_registers = int.from_bytes(restore[2:], "big").bit_count()
            source_rows = _literal_instruction_rows(battle_source, "DebugModeBattleTest")
            source_save = next(row for row in source_rows if row == "movem.l d0-a6,-(sp)")
            source_restore = next(row for row in source_rows if row == "movem.l (sp)+,d0-a6")
            register_range_count = _movem_register_count(source_save, push=True)
            if register_range_count != _movem_register_count(source_restore, push=False):
                raise ValueError("service lifecycle saved-register source range drift")
            if register_range_count != _movem_register_count(
                battle_records[save_index]["instruction"], push=True
            ) or register_range_count != _movem_register_count(
                battle_records[restore_index]["instruction"], push=False
            ):
                raise ValueError("service lifecycle saved-register H1 range drift")
            if (
                save_registers != restore_registers
                or save_registers != register_range_count
                or save_registers == 0
            ):
                raise ValueError("service lifecycle saved-register stack mask drift")
            frame_bytes = save_registers * operand_bytes

        returning = case["transferKind"] == "returning-call"
        return_word_bytes = operand_bytes if returning else 0
        target = case["returnAddress"] if returning else RESULT_STUB_ADDRESS
        role = "outer-caller-return" if returning else "outer-rts-harness-return"
        if target is None:
            raise ValueError("service lifecycle outer-return target drift")
        case.update(
            {
                "serviceEntryStackAddress": STACK_TOP - frame_bytes - return_word_bytes,
                "postServiceRtsStackAddress": (
                    STACK_TOP - frame_bytes + (0 if returning else operand_bytes)
                ),
                "outerReturnTargetAddress": target,
                "outerReturnRole": role,
            }
        )


def build_static_contract(rom_path: Path, upstream_path: Path = UPSTREAM) -> dict[str, Any]:
    """Derive the complete source/H1/ROM caller and entry-return contract."""
    disasm = upstream_path / "disasm"
    h2_entries = _h2_service_entries()
    listing = (upstream_path / LISTING_RELATIVE).read_text(encoding="utf-8")
    symbols = listing_symbol_addresses(listing)
    aliases: dict[str, dict[str, Any]] = {}
    interface = read_upstream_text(disasm / JUMP_INTERFACE)
    for service, alias in ALIASES.items():
        target = SERVICE_SYMBOLS[service]
        if not re.search(rf"^{alias}:\s*\n\s*jmp\s+{target}\(pc\)", interface, re.MULTILINE):
            raise ValueError(f"service lifecycle jump-interface alias drift: {alias}")
        aliases[service] = {
            "instructionTarget": alias,
            "instructionTargetAddress": symbols[alias],
            "effectiveTarget": target,
            "effectiveTargetAddress": symbols[target],
        }
    rom = rom_path.read_bytes()
    inventory = _caller_inventory(disasm, listing)
    _bind_rom_instruction_bytes(inventory, rom, aliases)
    entry_seams = _entry_seams(disasm, listing)
    if len(inventory) != 69:
        raise ValueError("service lifecycle caller denominator drift")
    counts = {
        service: sum(1 for site in inventory if site["effectiveTarget"] == SERVICE_SYMBOLS[service])
        for service in SERVICES
    }
    if counts != {"shop": 33, "church": 29, "caravan": 5, "blacksmith": 2}:
        raise ValueError("service lifecycle caller service partition drift")
    family_counts = dict(sorted(Counter(site["family"] for site in inventory).items()))
    if family_counts != {
        "battle-test": 4,
        "context-menu": 3,
        "exploration-vint": 2,
        "map-entity": 60,
    }:
        raise ValueError("service lifecycle caller family partition drift")
    transfer_counts = dict(sorted(Counter(site["transferKind"] for site in inventory).items()))
    if transfer_counts != {"returning-call": 62, "tail-transfer": 7}:
        raise ValueError("service lifecycle caller transfer-kind partition drift")
    family_service_counts = _family_service_counts(inventory)
    expected_family_service_counts = {
        "context-menu": {"shop": 1, "church": 1, "caravan": 0, "blacksmith": 1},
        "exploration-vint": {"shop": 0, "church": 1, "caravan": 1, "blacksmith": 0},
        "battle-test": {"shop": 1, "church": 2, "caravan": 1, "blacksmith": 0},
        "map-entity": {"shop": 31, "church": 25, "caravan": 3, "blacksmith": 1},
    }
    if family_service_counts != expected_family_service_counts:
        raise ValueError("service lifecycle zero-inclusive family/service partition drift")
    family_service_transfer_counts = _family_service_transfer_counts(inventory)
    expected_family_service_transfer_counts = {
        "context-menu": {
            "shop": {"returning-call": 1, "tail-transfer": 0},
            "church": {"returning-call": 1, "tail-transfer": 0},
            "caravan": {"returning-call": 0, "tail-transfer": 0},
            "blacksmith": {"returning-call": 1, "tail-transfer": 0},
        },
        "exploration-vint": {
            "shop": {"returning-call": 0, "tail-transfer": 0},
            "church": {"returning-call": 1, "tail-transfer": 0},
            "caravan": {"returning-call": 1, "tail-transfer": 0},
            "blacksmith": {"returning-call": 0, "tail-transfer": 0},
        },
        "battle-test": {
            "shop": {"returning-call": 1, "tail-transfer": 0},
            "church": {"returning-call": 2, "tail-transfer": 0},
            "caravan": {"returning-call": 1, "tail-transfer": 0},
            "blacksmith": {"returning-call": 0, "tail-transfer": 0},
        },
        "map-entity": {
            "shop": {"returning-call": 28, "tail-transfer": 3},
            "church": {"returning-call": 22, "tail-transfer": 3},
            "caravan": {"returning-call": 3, "tail-transfer": 0},
            "blacksmith": {"returning-call": 0, "tail-transfer": 1},
        },
    }
    if family_service_transfer_counts != expected_family_service_transfer_counts:
        raise ValueError("service lifecycle zero-inclusive family/service/transfer partition drift")
    cases = _selected_cases(inventory, rom)
    for case in cases:
        if case["family"] == "context-menu":
            case["callerEntryAddress"] = symbols["csc12_executeContextMenu"]
    _outer_return_stack_contract(cases, disasm, listing, symbols, rom)
    static = {
        "aliases": aliases,
        "callerInventory": inventory,
        "callerDenominator": len(inventory),
        "callerServiceCounts": counts,
        "callerFamilyCounts": family_counts,
        "callerTransferCounts": transfer_counts,
        "callerFamilyServiceCounts": family_service_counts,
        "callerFamilyServiceTransferCounts": family_service_transfer_counts,
        "representativeSelectionRule": REPRESENTATIVE_SELECTION_RULE,
        "h2ServiceEntries": h2_entries,
        "entrySeams": entry_seams,
        "generatedStubs": _generated_stubs(entry_seams),
        "outerReturnTrampoline": {
            "address": OUTER_RETURN_TRAMPOLINE_ADDRESS,
            "widthBytes": 6,
            "instructionPrefixHex": "4EF9",
            "purpose": "generated-source-return-transfer",
        },
        "cases": cases,
        "registerSentinels": CALLER_SENTINELS,
        "currentPortraitAddress": CURRENT_PORTRAIT_ADDRESS,
        "stackTop": STACK_TOP,
        "harness": {
            "baseAddress": HARNESS_BASE_ADDRESS,
            "strideBytes": HARNESS_STRIDE,
            "resultOffsetBytes": HARNESS_RESULT_OFFSET,
            "resultStubAddress": RESULT_STUB_ADDRESS,
            "callerFrameAddress": CALLER_FRAME_ADDRESS,
            "checkSramAddress": symbols["CheckSram"],
            "contextMenuHandlerAddress": symbols["csc12_executeContextMenu"],
            "caseFrameBudget": 180,
        },
    }
    patches = _patches(static)
    data = rom_path.read_bytes()
    for patch in patches:
        start = patch["address"]
        original = data[start : start + patch["widthBytes"]]
        if len(original) != patch["widthBytes"]:
            raise ValueError("service lifecycle ROM patch source range drift")
        patch["originalHex"] = original.hex().upper()
    static["sessionPatches"] = patches
    return static


def _canonical_static(static: dict[str, Any]) -> dict[str, Any]:
    return {
        key: static[key]
        for key in (
            "aliases",
            "callerInventory",
            "callerDenominator",
            "callerServiceCounts",
            "callerFamilyCounts",
            "callerTransferCounts",
            "callerFamilyServiceCounts",
            "callerFamilyServiceTransferCounts",
            "representativeSelectionRule",
            "h2ServiceEntries",
            "entrySeams",
            "generatedStubs",
            "outerReturnTrampoline",
            "cases",
            "registerSentinels",
            "currentPortraitAddress",
            "stackTop",
            "harness",
            "sessionPatches",
        )
    }


def _source_context(static: dict[str, Any]) -> dict[str, int]:
    return {
        f"{service}EntryAddress": static["aliases"][service]["effectiveTargetAddress"]
        for service in SERVICES
    }


def _assert_fixture(fixture: dict[str, Any], static: dict[str, Any]) -> None:
    if tuple(fixture["caseOrder"]) != CASE_ORDER:
        raise ValueError("service lifecycle exact case-order guard drift")
    if fixture["static"] != _canonical_static(static):
        raise ValueError("service lifecycle static golden guard drift")
    expected_cases = static["cases"]
    if fixture["cases"] != expected_cases:
        raise ValueError("service lifecycle selected case matrix guard drift")
    if fixture["sourceContext"] != _source_context(static):
        raise ValueError("service lifecycle source-context entry guard drift")
    if fixture["acceptedObservation"] != expected_observation(fixture, static):
        raise ValueError("service lifecycle accepted observation guard drift")
    _validate_case_matrix(static["cases"], static["harness"]["contextMenuHandlerAddress"])


def _validate_case_matrix(cases: list[dict[str, Any]], context_handler: int) -> None:
    """Bind the compact runtime cohort to the derived selection rule."""
    expected = (
        (
            "context-menu-church",
            "context-menu",
            "church",
            "returning-call",
            "continued-instruction",
        ),
        ("context-menu-shop", "context-menu", "shop", "returning-call", "continued-instruction"),
        (
            "context-menu-blacksmith",
            "context-menu",
            "blacksmith",
            "returning-call",
            "continued-instruction",
        ),
        ("exploration-vint-church", "exploration-vint", "church", "returning-call", "rts"),
        (
            "exploration-vint-caravan",
            "exploration-vint",
            "caravan",
            "returning-call",
            "continued-instruction",
        ),
        (
            "battle-test-church-main",
            "battle-test",
            "church",
            "returning-call",
            "continued-instruction",
        ),
        ("battle-test-shop", "battle-test", "shop", "returning-call", "continued-instruction"),
        (
            "battle-test-caravan",
            "battle-test",
            "caravan",
            "returning-call",
            "continued-instruction",
        ),
        (
            "battle-test-church-preserved-registers",
            "battle-test",
            "church",
            "returning-call",
            "continued-instruction",
        ),
        ("map-entity-church", "map-entity", "church", "returning-call", "rts"),
        ("map-entity-shop", "map-entity", "shop", "returning-call", "rts"),
        ("map-entity-caravan", "map-entity", "caravan", "returning-call", "rts"),
        ("map-entity-tail-church", "map-entity", "church", "tail-transfer", "outer-rts-harness"),
        ("map-entity-tail-shop", "map-entity", "shop", "tail-transfer", "outer-rts-harness"),
        (
            "map-entity-tail-blacksmith",
            "map-entity",
            "blacksmith",
            "tail-transfer",
            "outer-rts-harness",
        ),
    )
    actual = tuple(
        (
            case["caseId"],
            case["family"],
            case["service"],
            case["transferKind"],
            case["returnKind"],
        )
        for case in cases
    )
    if actual != expected:
        raise ValueError("service lifecycle exact representative case matrix drift")
    for case in cases:
        if case["transferKind"] == "tail-transfer":
            if (
                case["callerEntryAddress"] != case["callSiteAddress"]
                or case["continuationRedirectAddress"] is not None
                or case["returnAddress"] is not None
                or case["returnKind"] != "outer-rts-harness"
            ):
                raise ValueError("service lifecycle tail-transfer harness continuation guard drift")
        elif case["family"] == "context-menu":
            if case["callerEntryAddress"] != context_handler:
                raise ValueError("service lifecycle context caller-entry guard drift")
        elif case["caseId"] == "battle-test-church-preserved-registers":
            if (
                case["callerEntryAddress"] != case["callSiteAddress"] - 4
                or case["continuationRedirectAddress"] != case["returnAddress"] + 4
            ):
                raise ValueError("service lifecycle saved-register continuation guard drift")
        elif (
            case["callerEntryAddress"] != case["callSiteAddress"]
            or case["continuationRedirectAddress"] != case["returnAddress"]
        ):
            raise ValueError("service lifecycle direct caller continuation guard drift")


def _inner_stub(static: dict[str, Any], case: dict[str, Any]) -> dict[str, Any]:
    role = (
        "generated-blacksmith-return-stub"
        if case["service"] == "blacksmith"
        else "generated-service-cancel-stub"
    )
    return next(stub for stub in static["generatedStubs"] if stub["role"] == role)


def _expected_case_chronology(
    static: dict[str, Any], case: dict[str, Any], index: int
) -> list[dict[str, Any]]:
    """Return the complete internal callback order, including generated harness entry."""
    result_address = HARNESS_BASE_ADDRESS + (index - 1) * HARNESS_STRIDE + HARNESS_RESULT_OFFSET
    inner = _inner_stub(static, case)
    return [
        {
            "role": "case-entry",
            "pc": HARNESS_BASE_ADDRESS + (index - 1) * HARNESS_STRIDE,
        },
        {
            "role": (
                "tail-transfer-site"
                if case["transferKind"] == "tail-transfer"
                else "caller-call-site"
            ),
            "pc": case["callSiteAddress"],
        },
        {
            "role": "service-entry",
            "pc": static["aliases"][case["service"]]["effectiveTargetAddress"],
        },
        {"role": inner["role"], "pc": inner["address"]},
        {"role": case["outerReturnRole"], "pc": static["outerReturnTrampoline"]["address"]},
        {"role": "caller-result", "pc": result_address},
    ]


def expected_observation(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    records = []
    trampoline_readback = []
    for index, case in enumerate(static["cases"], start=1):
        stack_after = (
            STACK_TOP + 4
            if (
                case["family"] == "context-menu"
                or case["returnKind"] == "rts"
                or case["transferKind"] == "tail-transfer"
            )
            else STACK_TOP
        )
        a6_after = (
            CALLER_FRAME_ADDRESS + 2 if case["family"] == "context-menu" else CALLER_FRAME_ADDRESS
        )
        records.append(
            {
                "id": case["caseId"],
                "family": case["family"],
                "service": case["service"],
                "transferKind": case["transferKind"],
                "callSiteAddress": case["callSiteAddress"],
                "entryAddress": static["aliases"][case["service"]]["effectiveTargetAddress"],
                "returnAddress": case["returnAddress"],
                "resultAddress": (
                    HARNESS_BASE_ADDRESS + (index - 1) * HARNESS_STRIDE + HARNESS_RESULT_OFFSET
                ),
                "returnKind": case["returnKind"],
                "registersAfter": {
                    "d0": index - 1 if case["family"] == "context-menu" else CALLER_SENTINELS["d0"],
                    "d1": CALLER_SENTINELS["d1"],
                    "d2": CALLER_SENTINELS["d2"],
                    "a6": a6_after,
                    "a7": stack_after,
                },
                "serviceBodyBypassed": True,
                "callbackChronology": _expected_case_chronology(static, case, index)[1:],
            }
        )
        trampoline_readback.append(
            {
                "id": case["caseId"],
                "role": case["outerReturnRole"],
                "address": static["outerReturnTrampoline"]["address"],
                "targetAddress": case["outerReturnTargetAddress"],
                "widthBytes": static["outerReturnTrampoline"]["widthBytes"],
                "hex": _jump_hex(case["outerReturnTargetAddress"]),
                "serviceEntryStackAddress": case["serviceEntryStackAddress"],
                "sourceReturnAddress": case["outerReturnTargetAddress"],
                "postServiceRtsStackAddress": case["postServiceRtsStackAddress"],
            }
        )
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "caseOrder": list(CASE_ORDER),
        "records": records,
        "sessionPatchReadback": [
            {"role": patch["role"], "address": patch["address"], "hex": patch["patchedHex"]}
            for patch in static["sessionPatches"]
        ],
        "generatedStubReadback": [
            {
                "role": stub["role"],
                "address": stub["address"],
                "widthBytes": stub["widthBytes"],
                "hex": stub["instructionHex"],
            }
            for stub in static["generatedStubs"]
        ],
        "outerReturnTrampolineReadback": trampoline_readback,
        "restoration": {
            "currentPortraitRestored": True,
            "callerFrameRestored": True,
            "callbacksCleared": True,
        },
        "callbacksCleared": 0,
    }


def _instrument_session_rom(rom_path: Path, static: dict[str, Any], destination: Path) -> None:
    original = rom_path.read_bytes()
    ordered = sorted(static["sessionPatches"], key=lambda patch: patch["address"])
    for left, right in zip(ordered, ordered[1:], strict=False):
        if left["address"] + left["widthBytes"] > right["address"]:
            raise ValueError("service lifecycle session-ROM patch overlap")
    data = bytearray(original)
    for patch in static["sessionPatches"]:
        address = patch["address"]
        original_bytes = bytes.fromhex(patch["originalHex"])
        patched_bytes = bytes.fromhex(patch["patchedHex"])
        if len(original_bytes) != patch["widthBytes"] or len(patched_bytes) != patch["widthBytes"]:
            raise ValueError("service lifecycle session patch width drift")
        if data[address : address + len(original_bytes)] != original_bytes:
            raise ValueError(
                f"service lifecycle session patch original-byte drift: {patch['role']}"
            )
        data[address : address + len(patched_bytes)] = patched_bytes
        if data[address : address + len(patched_bytes)] != patched_bytes:
            raise ValueError(f"service lifecycle session patch readback drift: {patch['role']}")
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if rom_path.read_bytes() != original:
        raise ValueError("service lifecycle instrumentation altered canonical ROM")
    destination.write_bytes(data)
    if destination.read_bytes() != data:
        raise ValueError("service lifecycle session ROM write readback drift")


def preflight_service_menu_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM
) -> dict[str, Any]:
    """Run the complete source/H1/ROM and disposable-session prelaunch gate."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="service-menu lifecycle fixture")
    assert_lua_role_contract()
    static = build_static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    session_rom: Path | None = None
    try:
        with tempfile.TemporaryDirectory(
            prefix="sf2-service-menu-lifecycle-preflight-"
        ) as directory:
            session_rom = Path(directory) / "service-menu-entry-return.session.bin"
            _instrument_session_rom(rom_path, static, session_rom)
            for patch in static["sessionPatches"]:
                actual = session_rom.read_bytes()[
                    patch["address"] : patch["address"] + patch["widthBytes"]
                ]
                if actual != bytes.fromhex(patch["patchedHex"]):
                    raise ValueError(
                        f"service lifecycle preflight patch readback drift: {patch['role']}"
                    )
    except Exception as error:
        if session_rom is not None and session_rom.exists():
            raise ValueError(
                "service lifecycle disposable session ROM residue after preflight failure"
            ) from error
        raise
    if session_rom is None or session_rom.exists():
        raise ValueError("service lifecycle disposable session ROM residue")
    return {
        "Fixture": fixture["id"],
        "Cases": len(static["cases"]),
        "CallerDenominator": static["callerDenominator"],
        "SessionPatches": len(static["sessionPatches"]),
        "Status": "PRELAUNCH-PASS",
    }


def _observer_config(fixture: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "caseOrder": fixture["caseOrder"],
        "cases": static["cases"],
        "static": _canonical_static(static),
        "sourceContext": _source_context(static),
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _failure_diagnostic(
    status_path: Path, static: dict[str, Any] | None = None
) -> dict[str, Any] | None:
    payload = callback_failure_status(status_path, owner=OWNER, schema_path=FAILURE_SCHEMA)
    if payload is None:
        return None
    lines = status_path.read_text(encoding="utf-8").splitlines()
    failures = [index for index, line in enumerate(lines) if line.startswith(STATUS_PREFIX)]
    if len(failures) != 1 or failures[0] != len(lines) - 1:
        raise ValueError("service lifecycle callback failure must be terminal and unique")
    if not any(line.startswith("milestone:") for line in lines[: failures[0]]):
        raise ValueError("service lifecycle callback failure lacks a preceding milestone")
    pending = payload["pendingCallback"]
    observed = pending["observedChronology"]
    expected = pending["expectedChronology"]
    if pending["observedChronologyCount"] != len(observed):
        raise ValueError("service lifecycle callback failure observed chronology-count drift")
    if pending["expectedChronologyCount"] != len(expected):
        raise ValueError("service lifecycle callback failure expected chronology-count drift")
    if observed != expected[: len(observed)]:
        raise ValueError(
            "service lifecycle callback failure observed chronology is not an expected prefix"
        )
    if static is not None and pending["expectedCaseId"] is not None:
        case_index = pending["caseIndex"]
        if not 1 <= case_index <= len(static["cases"]):
            raise ValueError("service lifecycle callback failure case-index drift")
        case = static["cases"][case_index - 1]
        expected_chronology = _expected_case_chronology(static, case, case_index)
        if pending["expectedCaseId"] != case["caseId"] or expected != expected_chronology:
            raise ValueError("service lifecycle callback failure expected chronology drift")
        stack = payload["stackReadback"]
        if payload["role"] == "service-entry":
            if (
                stack["expectedA7"] != case["serviceEntryStackAddress"]
                or stack["expectedTopLongword"] != case["outerReturnTargetAddress"]
                or stack["actualA7"] is None
                or stack["actualTopLongword"] is None
            ):
                raise ValueError(
                    "service lifecycle callback failure service-entry stack diagnostic drift"
                )
        elif payload["role"] in {"outer-caller-return", "outer-rts-harness-return"} and (
            stack["expectedA7"] != case["postServiceRtsStackAddress"]
            or stack["actualA7"] is None
            or stack["actualTopLongword"] is None
        ):
            raise ValueError("service lifecycle callback failure trampoline stack diagnostic drift")
    return payload


def verify_service_menu_lifecycle(
    rom_path: Path, upstream_path: Path = UPSTREAM, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="service-menu lifecycle fixture")
    assert_lua_role_contract()
    verify_runtime_contract(fixture, rom_path)
    static = build_static_contract(rom_path, upstream_path)
    _assert_fixture(fixture, static)
    session_rom: Path | None = None
    try:
        with tempfile.TemporaryDirectory(prefix="sf2-service-menu-lifecycle-") as directory:
            session_rom = Path(directory) / "service-menu-entry-return.session.bin"
            _instrument_session_rom(rom_path, static, session_rom)
            status_path = repo_path("local/derived/h3/service-menu-entry-return.status.txt")
            try:
                observed = _with_instrumented_rom_database(
                    session_rom,
                    "SF2 H3 service menu entry-return instrumentation",
                    lambda: run_observer(
                        rom_path=session_rom,
                        observer_path=OBSERVER,
                        config=_observer_config(fixture, static),
                        output_name=OWNER,
                        timeout_seconds=timeout_seconds,
                    ),
                )
            except RuntimeError as error:
                diagnostic = _failure_diagnostic(status_path, static)
                if diagnostic is not None:
                    raise RuntimeError(
                        f"{OWNER} observer callback failure: {diagnostic}"
                    ) from error
                raise
            assert_observer_status(
                status_path,
                owner=OWNER,
                schema_path=FAILURE_SCHEMA,
                required_milestones=(
                    "milestone:direct-function-probe",
                    "milestone:service-menu-cases-entered",
                ),
            )
            validate_json(observed, OBSERVATION_SCHEMA, owner="service-menu lifecycle observation")
            expected = expected_observation(fixture, static)
            if observed != expected:
                raise ValueError("service lifecycle observation golden drift")
    except Exception as error:
        if session_rom is not None and session_rom.exists():
            raise ValueError(
                "service lifecycle disposable session ROM residue after runtime failure"
            ) from error
        raise
    if session_rom is None or session_rom.exists():
        raise ValueError("service lifecycle disposable session ROM residue after runtime")
    return {
        "Fixture": fixture["id"],
        "Cases": len(static["cases"]),
        "CallerDenominator": static["callerDenominator"],
        "BizHawkLaunches": 1,
        "CallbacksCleared": observed["callbacksCleared"],
        "Restoration": observed["restoration"],
        "SessionRomDeleted": True,
        "Status": "PASS",
    }
