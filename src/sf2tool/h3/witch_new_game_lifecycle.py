from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.witch_save_actions import _equates, _require_order, _section
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/witch-new-game-lifecycle-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-witch-new-game-lifecycle-fixture.schema.json")
OUTPUT_SCHEMA = repo_path("schemas/h3-witch-new-game-lifecycle-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/witch_new_game_lifecycle_observer.lua")

WITCH_SOURCE_PATH = Path("code/specialscreens/witch/witchstart.asm")
NEW_GAME_SOURCE_PATH = Path("code/common/stats/newgame.asm")
CONFIGURATION_SOURCE_PATH = Path("code/gameflow/special/configurationmode.asm")
JUMP_NEW_GAME_PATH = Path("code/common/tech/jumpinterfaces/s02_jumpinterface.asm")
JUMP_NAME_ALLY_PATH = Path("code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm")
JUMP_MENU_PATH = Path("code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm")
CONST_PATH = Path("sf2const.asm")
ENUM_PATH = Path("sf2enums.asm")


def _source_use_sites(
    witch_source: str,
    new_game_source: str,
    configuration_source: str,
    jump_new_game: str,
    jump_name_ally: str,
    jump_menu: str,
) -> dict[str, list[dict[str, Any]]]:
    new = _section(witch_source, "witchMenuAction_New")
    initialize = _section(new_game_source, "InitializeGameSettings")
    configuration = _section(configuration_source, "CheatModeConfiguration")
    return {
        "newAction": _require_order(
            new,
            (
                ("move.b", "(save_flags).l,d2"),
                ("andi.w", "#3,d2"),
                ("eori.w", "#3,d2"),
                ("lsl.w", "#1,d2"),
                ("btst", "#1,d2"),
                ("beq.s", "@loc_8"),
                ("moveq", "#1,d0"),
                ("bra.s", "@loc_9"),
                ("moveq", "#2,d0"),
                ("moveq", "#1,d1"),
                ("jsr", "j_executewitchmainmenu"),
                ("tst.w", "d0"),
                ("bmi.s", "byte_73c2"),
                ("subq.w", "#1,d0"),
                ("move.w", "d0,((current_save_slot-$1000000)).w"),
                ("jsr", "j_newgame"),
                ("clr.w", "d0"),
                ("jsr", "j_nameally"),
                ("bsr.w", "cheatmodeconfiguration"),
                ("clr.w", "d0"),
                ("moveq", "#3,d1"),
                ("moveq", "#%1111,d2"),
                ("jsr", "j_executewitchmainmenu"),
                ("tst.w", "d0"),
                ("bpl.s", "@loc_13"),
                ("clr.w", "d0"),
                ("btst", "#0,d0"),
                ("beq.s", "@loc_14"),
                ("setflg", "78"),
                ("btst", "#1,d0"),
                ("beq.s", "@loc_15"),
                ("setflg", "79"),
                ("addi.w", "#233,d0"),
                ("bsr.w", "displaytext"),
                ("move.w", "((current_save_slot-$1000000)).w,d0"),
                ("move.b", "#gamestart_map,((current_map-$1000000)).w"),
                ("move.b", "#gamestart_map,((egress_map-$1000000)).w"),
                ("bsr.w", "savegame"),
                ("move.b", "#gamestart_map,d0"),
                ("move.w", "#gamestart_savepoint_x,d1"),
                ("move.w", "#gamestart_savepoint_y,d2"),
                ("move.w", "#gamestart_facing,d3"),
                ("moveq", "#1,d4"),
                ("bra.w", "mainloop"),
            ),
            name="witchMenuAction_New",
        ),
        "newGameReset": _require_order(
            initialize,
            (
                ("moveq", "#0,d0"),
                ("move.b", "d0,((current_map-$1000000)).w"),
                ("move.b", "d0,((egress_map-$1000000)).w"),
            ),
            name="InitializeGameSettings",
        ),
        "cheatMode": _require_order(
            configuration,
            (
                ("btst", "#input_bit_start,((player_1_input-$1000000)).w"),
                ("beq.w", "@return"),
            ),
            name="CheatModeConfiguration",
        ),
        "newGameAlias": _require_order(
            _section(jump_new_game, "j_NewGame"),
            (("jmp", "newgame(pc)"),),
            name="j_NewGame",
        ),
        "nameAllyAlias": _require_order(
            _section(jump_name_ally, "j_NameAlly"),
            (("jmp", "nameally(pc)"),),
            name="j_NameAlly",
        ),
        "menuAlias": _require_order(
            _section(jump_menu, "j_ExecuteWitchMainMenu"),
            (("jmp", "executewitchmainmenu(pc)"),),
            name="j_ExecuteWitchMainMenu",
        ),
    }


def _require_scratch_isolation(*sources: str) -> None:
    if any("ff6802_loading_space" in source.lower() for source in sources):
        raise ValueError("witch New lifecycle work-RAM scratch overlaps parsed original source")


def _immediate_from_use_site(
    use_site: dict[str, Any], *, destination: str, description: str
) -> int:
    """Read one immediate from the verified source use site, never a duplicate literal."""
    try:
        immediate, observed_destination = use_site["operand"].split(",", 1)
    except ValueError as error:
        raise ValueError(
            f"witch New lifecycle {description} has no immediate/destination operand"
        ) from error
    if observed_destination != destination or not immediate.startswith("#"):
        raise ValueError(
            f"witch New lifecycle {description} has unexpected operand "
            f"{use_site['operand']!r}"
        )
    raw = immediate[1:]
    try:
        if raw.startswith("$"):
            return int(raw[1:], 16)
        if raw.startswith("%"):
            return int(raw[1:], 2)
        return int(raw)
    except ValueError as error:
        raise ValueError(
            f"witch New lifecycle {description} immediate is not numeric: {immediate!r}"
        ) from error


def _symbol_immediate_from_use_site(
    use_site: dict[str, Any], *, destination: str, constants: dict[str, int], description: str
) -> int:
    """Resolve a source operand through the one parsed enum/constants map."""
    try:
        immediate, observed_destination = use_site["operand"].split(",", 1)
    except ValueError as error:
        raise ValueError(
            f"witch New lifecycle {description} has no symbolic immediate/destination operand"
        ) from error
    if observed_destination != destination or not immediate.startswith("#"):
        raise ValueError(
            f"witch New lifecycle {description} has unexpected operand "
            f"{use_site['operand']!r}"
        )
    symbol = immediate[1:].upper()
    if symbol not in constants:
        raise ValueError(
            f"witch New lifecycle {description} refers to unknown parsed constant {symbol!r}"
        )
    return constants[symbol]


def build_witch_new_game_lifecycle_source_contract(upstream_path: Path) -> dict[str, Any]:
    disasm = upstream_path.resolve(strict=True) / "disasm"
    witch_source = (disasm / WITCH_SOURCE_PATH).read_text(encoding="utf-8")
    new_game_source = (disasm / NEW_GAME_SOURCE_PATH).read_text(encoding="utf-8")
    configuration_source = (disasm / CONFIGURATION_SOURCE_PATH).read_text(encoding="utf-8")
    jump_new_game = (disasm / JUMP_NEW_GAME_PATH).read_text(encoding="utf-8")
    jump_name_ally = (disasm / JUMP_NAME_ALLY_PATH).read_text(encoding="utf-8")
    jump_menu = (disasm / JUMP_MENU_PATH).read_text(encoding="utf-8")
    _require_scratch_isolation(witch_source, new_game_source)
    constants = _equates(
        (disasm / CONST_PATH).read_text(encoding="utf-8"),
        (
            "SAVE_FLAGS",
            "SAVE1_DATA",
            "SAVE2_DATA",
            "SAVE1_CHECKSUM",
            "SAVE2_CHECKSUM",
            "SRAM_START",
            "CURRENT_SAVE_SLOT",
            "GAME_FLAGS",
            "CURRENT_MAP",
            "EGRESS_MAP",
            "COMBATANT_DATA",
            "PLAYER_1_INPUT",
            "FF6802_LOADING_SPACE",
        ),
    )
    enum_values = _equates(
        (disasm / ENUM_PATH).read_text(encoding="utf-8"),
        (
            "GAMESTART_MAP",
            "GAMESTART_SAVEPOINT_X",
            "GAMESTART_SAVEPOINT_Y",
            "GAMESTART_FACING",
            "SAVE_SLOT_REAL_SIZE",
            "SAVE_SLOT_SIZE",
        ),
    )
    if enum_values["SAVE_SLOT_SIZE"] % enum_values["SAVE_SLOT_REAL_SIZE"]:
        raise ValueError("witch New lifecycle slot interval is not an integral address step")
    listing = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    addresses = listing_symbol_addresses(listing)
    use_sites = _source_use_sites(
        witch_source,
        new_game_source,
        configuration_source,
        jump_new_game,
        jump_name_ally,
        jump_menu,
    )
    new_action = use_sites["newAction"]
    save_flags_mask = _immediate_from_use_site(
        new_action[1], destination="d2", description="save-flags mask"
    )
    availability_xor_mask = _immediate_from_use_site(
        new_action[2], destination="d2", description="availability xor mask"
    )
    if availability_xor_mask != save_flags_mask:
        raise ValueError("witch New lifecycle availability xor no longer matches save-flags mask")
    selector_shift = _immediate_from_use_site(
        new_action[3], destination="d2", description="selector left shift"
    )
    if selector_shift < 0:
        raise ValueError("witch New lifecycle selector left shift cannot be negative")
    initial_menu_page = _immediate_from_use_site(
        new_action[9], destination="d1", description="initial menu page"
    )
    difficulty_menu_page = _immediate_from_use_site(
        new_action[20], destination="d1", description="difficulty menu page"
    )
    game_start_map = _symbol_immediate_from_use_site(
        new_action[38], destination="d0", constants=enum_values, description="MainLoop map"
    )
    if game_start_map != _symbol_immediate_from_use_site(
        new_action[35],
        destination="((current_map-$1000000)).w",
        constants=enum_values,
        description="saved current-map",
    ):
        raise ValueError("witch New lifecycle saved current-map diverges from MainLoop map")
    if game_start_map != _symbol_immediate_from_use_site(
        new_action[36],
        destination="((egress_map-$1000000)).w",
        constants=enum_values,
        description="saved egress-map",
    ):
        raise ValueError("witch New lifecycle saved egress-map diverges from MainLoop map")
    return {
        "function": {
            "checkSramAddress": addresses["CheckSram"],
            "newActionAddress": addresses["witchMenuAction_New"],
            "menuInstructionTargetAddress": addresses["j_ExecuteWitchMainMenu"],
            "menuEffectiveTargetAddress": addresses["ExecuteWitchMainMenu"],
            "newGameInstructionTargetAddress": addresses["j_NewGame"],
            "newGameEffectiveTargetAddress": addresses["NewGame"],
            "nameAllyInstructionTargetAddress": addresses["j_NameAlly"],
            "nameAllyEffectiveTargetAddress": addresses["NameAlly"],
            "cheatModeConfigurationAddress": addresses["CheatModeConfiguration"],
            "displayTextAddress": addresses["DisplayText"],
            "saveGameAddress": addresses["SaveGame"],
            "mainLoopAddress": addresses["MainLoop"],
        },
        "ram": {
            "currentSaveSlotAddress": constants["CURRENT_SAVE_SLOT"],
            "gameFlagsAddress": constants["GAME_FLAGS"],
            "currentMapAddress": constants["CURRENT_MAP"],
            "egressMapAddress": constants["EGRESS_MAP"],
            "combatantDataAddress": constants["COMBATANT_DATA"],
            "player1InputAddress": constants["PLAYER_1_INPUT"],
            "workRamScratchAddress": constants["FF6802_LOADING_SPACE"],
        },
        "storage": {
            "saveFlagsAddress": constants["SAVE_FLAGS"],
            "logicalPayloadByteCountPerSlot": enum_values["SAVE_SLOT_REAL_SIZE"],
            "storedPhysicalByteCountPerSlot": enum_values["SAVE_SLOT_REAL_SIZE"],
            "physicalAddressIntervalPerSlot": enum_values["SAVE_SLOT_SIZE"],
            "physicalAddressStepPerLogicalByte": (
                enum_values["SAVE_SLOT_SIZE"] // enum_values["SAVE_SLOT_REAL_SIZE"]
            ),
            "physicalWindowBaseAddress": constants["SRAM_START"] & ~1,
            "firstStoredPhysicalByteAddress": constants["SRAM_START"],
            "slot1DataAddress": constants["SAVE1_DATA"],
            "slot2DataAddress": constants["SAVE2_DATA"],
            "slot1ChecksumAddress": constants["SAVE1_CHECKSUM"],
            "slot2ChecksumAddress": constants["SAVE2_CHECKSUM"],
        },
        "newAction": {
            "saveFlagsMask": save_flags_mask,
            "availabilityXorMask": availability_xor_mask,
            "selectorScale": 1 << selector_shift,
            "menuReturnSubtract": _immediate_from_use_site(
                new_action[13], destination="d0", description="menu return subtraction"
            ),
            "initialMenuPage": initial_menu_page,
            "difficultyMenuPage": difficulty_menu_page,
            "difficultyAvailabilityMask": _immediate_from_use_site(
                new_action[21], destination="d2", description="difficulty availability mask"
            ),
            "sourceFlag78": int(new_action[28]["operand"]),
            "sourceFlag79": int(new_action[31]["operand"]),
            "gameStartMap": game_start_map,
            "gameStartSavepointX": _symbol_immediate_from_use_site(
                new_action[39],
                destination="d1",
                constants=enum_values,
                description="MainLoop savepoint x",
            ),
            "gameStartSavepointY": _symbol_immediate_from_use_site(
                new_action[40],
                destination="d2",
                constants=enum_values,
                description="MainLoop savepoint y",
            ),
            "gameStartFacing": _symbol_immediate_from_use_site(
                new_action[41],
                destination="d3",
                constants=enum_values,
                description="MainLoop facing",
            ),
            "mainLoopD4": _immediate_from_use_site(
                new_action[42], destination="d4", description="MainLoop d4"
            ),
        },
        "sourceUseSites": use_sites,
    }


def verify_witch_new_game_lifecycle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="witch New-game lifecycle runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    source_contract = build_witch_new_game_lifecycle_source_contract(upstream_path)
    for field in ("function", "ram", "storage", "newAction"):
        if fixture[field] != source_contract[field]:
            raise ValueError(
                f"witch New-game lifecycle golden disagrees with parsed source {field} contract"
            )
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "core": fixture["emulator"]["core"],
            "function": source_contract["function"],
            "ram": source_contract["ram"],
            "storage": source_contract["storage"],
            "newAction": source_contract["newAction"],
            "harness": fixture["harness"],
            "sampleOffsets": fixture["cases"]["sampleOffsets"],
            "cases": fixture["cases"]["matrix"],
        },
        output_name="witch-new-game-lifecycle",
        timeout_seconds=timeout_seconds,
    )
    validate_json(observed, OUTPUT_SCHEMA, owner="witch New-game lifecycle runtime observation")
    if observed != fixture["expectedObservation"]:
        raise ValueError(
            "witch New-game lifecycle runtime matrix mismatch\n"
            f"expected={fixture['expectedObservation']!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]["matrix"]),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
