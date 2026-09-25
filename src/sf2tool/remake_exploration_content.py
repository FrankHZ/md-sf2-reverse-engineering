"""Prepare selected private original exploration content; never read comparison fixtures.

The registered canonical export supplies actual layouts, entity records and ordered program
operations. This offline boundary verifies that export and the pinned source checkout. Runtime
receives normalized immutable definitions; controlled starts are a separate reference input.
Unimplemented native operations remain executable stop instructions at their source location.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import io
import json
import re
import subprocess
import wave
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.h2.ally_data import _growth_facts
from sf2tool.h2.battle_ai import _equates
from sf2tool.h2.battle_global_data import _arguments, _integer, _statements, _tokens
from sf2tool.h2.map_import import MANIFEST, _canonical_bytes
from sf2tool.h2.portraits import _read_animation_entries
from sf2tool.h2.variable_width_font import _parse_ascii_map, build_variable_width_font_contract
from sf2tool.jsonio import load_json
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
from sf2tool.remake_asset_build import (
    PLAYER_PALETTE_ADDRESS,
    PLAYER_POINTER_TABLE_ADDRESS,
    _combine_player_halves,
    _render_player_frame,
)
from sf2tool.texture_extract import decode_md_4bpp_tile, md_palette_color, palette_index_rgba


def _location(program: str, instruction: int = 0) -> dict[str, Any]:
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


def selected_map_palette_bindings(
    compiler: OriginalPrograms, maps: list[int]
) -> list[dict[str, Any]]:
    """Read only the two logical words used by ExplorationLoop's return comparison.

    The caller supplies the already verified pinned compiler source. This small
    metadata entry does not export assets, select a period, or read a checkpoint.
    Sources join the compiler's existing provenance collection.
    """
    if len(set(maps)) != len(maps) or any(not 0 <= map_id <= 255 for map_id in maps):
        raise ValueError("unique supported map ids required")
    result = []
    for map_id in maps:
        source = f"disasm/data/maps/entries/map{map_id:02d}/00-tilesets.asm"
        palette_ids = _arguments(
            (compiler.upstream / source).read_text(encoding="utf-8"), "mapPalette"
        )
        if len(palette_ids) != 1:
            raise ValueError("map palette selection must be singular")
        palette_id = compiler.number(palette_ids[0])
        palette = f"disasm/data/graphics/maps/mappalettes/mappalette{palette_id:02d}.bin"
        data = (compiler.upstream / palette).read_bytes()
        if len(data) != 32:
            raise ValueError("map palette must contain 16 words")
        pair = {
            "color2": int.from_bytes(data[4:6], "big"),
            "color3": int.from_bytes(data[6:8], "big"),
        }
        if any(value & ~0xEEE for value in pair.values()):
            raise ValueError("unsupported map CRAM word")
        compiler.sources.update((source, palette))
        result.append({"map": f"map-{map_id}", "base": pair})
    return result


def _selected_growth(
    compiler: OriginalPrograms, selections: list[dict[str, Any]]
) -> list[dict[str, Any]]:
    if not selections:
        return []
    curves_path = "disasm/data/stats/allies/growthcurves.asm"
    compiler.sources.update((curves_path, "disasm/code/common/stats/levelup.asm"))
    curves = _growth_facts((compiler.upstream / curves_path).read_text(encoding="utf-8"))["curves"]
    result = []
    for selected in selections:
        path = f"disasm/data/stats/allies/stats/allystats{selected['member']:02d}.asm"
        compiler.sources.add(path)
        text = (compiler.upstream / path).read_text(encoding="utf-8")
        blocks = re.split(
            r"^(?:[A-Za-z_]\w*:[ \t]*)?[ \t]*forClass[ \t]+(\w+)[ \t]*$", text, flags=re.MULTILINE
        )
        classes = dict(zip(blocks[1::2], blocks[2::2], strict=True))
        block = classes[selected["class"]]
        stats = []
        for name in ("hpGrowth", "mpGrowth", "attGrowth", "defGrowth", "agiGrowth"):
            initial, projected, curve = _tokens(_arguments(block, name)[0])
            index = (
                compiler.number("GROWTHCURVE_" + curve) & compiler.equates["GROWTHCURVE_MASK_INDEX"]
            )
            stats.append(
                {
                    "start": compiler.number(initial),
                    "projected": compiler.number(projected),
                    "curve": curves[index - 1] if index else [],
                }
            )
        spell_block = (
            blocks[2] if re.search(r"^\s*useFirstSpellList", block, re.MULTILINE) else block
        )
        expressions = _arguments(spell_block, "spellList")
        tokens = _tokens(expressions[0]) if expressions else []
        spells = []
        for level, expression in zip(tokens[::2], tokens[1::2], strict=True):
            name, *rank = expression.split("|")
            packed = compiler.number("SPELL_" + name) | (
                compiler.number("SPELL_" + rank[0]) if rank else 0
            )
            spells.append(
                {"level": compiler.number(level), "packed": packed, "spell": name.lower()}
            )
        result.append(
            {
                "actor": "ally-" + str(selected["member"]),
                "classId": compiler.number("CLASS_" + selected["class"]),
                "stats": stats,
                "spells": spells,
            }
        )
    return result


class OriginalPrograms:
    """Lower source operations, preserving unsupported operations and all branch targets."""

    def __init__(
        self,
        canonical: dict[str, Any],
        upstream: Path,
        entity_speed: int = 32,
        scene_maps: list[int] | None = None,
    ):
        self.upstream = upstream
        self.entity_speed = entity_speed
        self.scene_maps = scene_maps
        self.equates = _equates(upstream / "disasm")
        self.sources: set[str] = {"disasm/sf2enums.asm"}
        resources = canonical["resources"]
        self.raw = {
            row["id"]: row
            for key in ("standaloneScriptPrograms", "initSourcePrograms")
            for row in resources[key]
        }
        self.programs: dict[str, dict[str, Any]] = {}
        self.actions: dict[str, list[dict[str, Any]]] = {}
        self.sources.update(
            {
                "disasm/sf2cutscenemacros.asm",
                "disasm/code/common/scripting/map/mapscriptengine_1.asm",
                "disasm/code/common/scripting/map/mapscriptengine_2.asm",
                "disasm/code/common/menus/portraitwindow.asm",
                "disasm/code/common/tech/interrupts/trap5_textbox.asm",
                "disasm/code/common/scripting/entity/entityscriptengine_1.asm",
                "disasm/code/common/scripting/entity/entityscriptengine_2.asm",
                "disasm/code/common/scripting/entity/entityfunctions_1.asm",
                "disasm/code/common/scripting/entity/entityfunctions_2.asm",
                "disasm/code/common/scripting/map/mapfunctions.asm",
                "disasm/code/common/scripting/map/followersfunctions_1.asm",
                "disasm/code/common/scripting/map/mapsetupsfunctions_1.asm",
                "disasm/code/common/maps/mapload.asm",
                "disasm/code/common/stats/battleparty.asm",
                "disasm/code/common/tech/randomnumbergenerator.asm",
                "disasm/data/scripting/entity/eas_main.asm",
                "disasm/code/gameflow/exploration/explorationfunctions_2.asm",
                "disasm/code/gameflow/exploration/exploration.asm",
                "disasm/code/gameflow/mainloop.asm",
                "disasm/code/gameflow/battle/battleloop/heallivingandimmortalallies.asm",
                "disasm/code/gameflow/battle/battleloop/initializecombatants.asm",
            }
        )

    def register_file(self, relative: str) -> None:
        self.sources.add(relative)
        text = (self.upstream / relative).read_text(encoding="utf-8")
        for match in re.finditer(r"^([A-Za-z_]\w*):", text, re.MULTILINE):
            symbol = match.group(1)
            if symbol not in self.raw:
                self.raw[symbol] = {
                    "id": symbol,
                    "path": relative.removeprefix("disasm/"),
                    "operations": self.source_operations(relative, symbol),
                }

    def walking(self, x: int, y: int, radius: int) -> list[dict[str, Any]]:
        path = "disasm/data/scripting/entity/eas_main.asm"
        self.sources.add(path)
        text = (self.upstream / path).read_text(encoding="utf-8")
        block = re.search(
            r"^eas_Walking:(.*?ac_branch\s*\n\s*dc\.w[^\n]+)", text, re.MULTILINE | re.DOTALL
        )
        if block is None:
            raise ValueError("source walking template")
        result, labels = [], {}
        branch = False
        for index, line in enumerate(block.group(1).splitlines()):
            if match := re.match(r"^(\w+):\s*(.*)", line):
                labels[match[1]] = len(result)
                line = match[2]
            for statement in _statements(line):
                op, _, operands = statement.partition(" ")
                if op == "ac_randomWalk":
                    result.append({"op": "random-walk", "x": x, "y": y, "radius": radius})
                elif op == "ac_waitDest":
                    continue  # RandomWalkEntity owns the movement wait.
                elif op == "ac_branch":
                    branch = True
                elif op == "dc.w" and branch:
                    target = re.match(r"\((\w+)-\w+\)", operands.strip())
                    if target is None or target[1] not in labels:
                        raise ValueError("source walking branch")
                    result.append({"op": "jump", "instruction": labels[target[1]]})
                else:
                    result.extend(
                        self.action(op, _tokens(operands), f"{path}:eas_Walking[{index}]")
                    )
        return result

    def initial_ally_sprites(self) -> list[dict[str, Any]]:
        classes_path = "disasm/data/stats/allies/allystartdefs.asm"
        sprites_path = "disasm/data/stats/allies/allymapsprites.asm"
        self.sources.update(
            (classes_path, sprites_path, "disasm/code/common/scripting/entity/getallymapsprite.asm")
        )
        classes = _arguments(
            (self.upstream / classes_path).read_text(encoding="utf-8"), "startClass"
        )
        sprites = _arguments(
            (self.upstream / sprites_path).read_text(encoding="utf-8"), "mapsprite"
        )
        result = []
        for character in range(self.equates["COMBATANT_ALLIES_NUMBER"]):
            role = self.number("CLASS_" + classes[character])
            sprite = self.number("MAPSPRITE_" + sprites[character])
            if role == self.equates["CLASS_SDMN"]:
                sprite -= 1
            elif role != self.equates["CLASS_HERO"] and role < self.equates["CLASS_BDBT"]:
                if self.equates["CLASS_BDMN"] <= role <= self.equates["CLASS_TORT"]:
                    sprite -= 1
                elif role <= self.equates["CLASS_ACHR"]:
                    sprite -= 2
                elif role & 1:
                    sprite -= 1
            rohde = character == self.equates["ALLY_ROHDE"]
            result.append(
                {
                    "character": character,
                    "sprite": sprite,
                    "joinedFlag": 11 if rohde else None,
                    "unjoinedSprite": self.equates["MAPSPRITE_NPC_ROHDE"] if rohde else None,
                }
            )
        return result

    def number(self, value: str) -> int:
        return self.equates[value] if value in self.equates else _integer(value)

    def entity(self, value: str) -> str:
        return "entity-" + str(self.number(value))

    def source_operations(self, relative: str, symbol: str) -> list[dict[str, Any]]:
        self.sources.add(relative)
        text = (self.upstream / relative).read_text(encoding="utf-8")
        match = re.search(
            rf"^{re.escape(symbol)}:[ \t]*(.*?)(?=^[A-Za-z_]\w*:|\Z)",
            text,
            re.MULTILINE | re.DOTALL,
        )
        if match is None:
            raise ValueError("selected source label is missing: " + symbol)
        result = []
        for index, statement in enumerate(_statements(match.group(1))):
            opcode, _, operands = statement.partition(" ")
            result.append({"index": index, "opcode": opcode, "operandText": operands.strip()})
        return result

    def native(self, opcode: str, source: str) -> dict[str, Any]:
        return {"op": "native-call", "symbol": opcode, "source": source}

    def population(self) -> dict[str, Any]:
        followers = []
        for row in self.source_operations(
            "disasm/data/scripting/entity/followers.asm", "table_Followers"
        ):
            if row["opcode"] != "follower":
                continue
            flag, character, sprite, _ = map(self.number, _tokens(row["operandText"]))
            followers.append(
                {
                    "flag": flag,
                    "character": character,
                    "sprite": character if character < 30 else sprite,
                }
            )
        return {
            "allyCount": self.equates["COMBATANT_ALLIES_NUMBER"],
            "nonAllyStart": 128,
            "playerSprite": 0,
            "followers": followers,
            "allySprites": self.initial_ally_sprites(),
        }

    def scene_entities(self, relative: str, symbol: str) -> dict[str, Any]:
        rows = self.source_operations(relative, symbol)
        if rows[0]["opcode"] != "mainEntity" or rows[-1]["opcode"] != "cscEntitiesEnd":
            raise ValueError("source cutscene entity table boundary")
        x, y, facing = map(self.number, _tokens(rows[0]["operandText"]))
        entities = []
        non_ally = 128
        for row in rows[1:-1]:
            if row["opcode"] != "entity":
                raise ValueError("source cutscene entity record")
            values = _tokens(row["operandText"])
            ex, ey, ef, sprite = map(self.number, values[:4])
            if sprite >= self.equates["MAPSPRITES_SPECIALS_START"]:
                raise ValueError("special cutscene entity not admitted")
            identity = sprite if sprite < 30 else non_ally
            non_ally += int(sprite >= 30)
            entities.append(
                {
                    "id": f"entity-{identity}",
                    "position": {"x": ex & 63, "y": ey & 63},
                    "facing": ef,
                    "sprite": sprite,
                    "speed": self.entity_speed,
                    "visible": True,
                    "obstruction": False,
                    "actions": self.action_stream(values[4]),
                }
            )
        return {
            "op": "scene-entities",
            "position": {"x": x, "y": y},
            "facing": facing,
            "population": self.population(),
            "entities": entities,
        }

    @staticmethod
    def finish_in_idle(actions: list[dict[str, Any]]) -> None:
        # Source esc34 clears the timer before entering eas_Idle in the same service.
        actions.extend([{"op": "jump", "instruction": len(actions) + 1}, {"op": "idle"}])

    def action_stream(self, symbol: str) -> list[dict[str, Any]]:
        if symbol in self.actions:
            return self.actions[symbol]
        path = "disasm/data/scripting/entity/eas_actions.asm"
        rows = self.source_operations(path, symbol)
        result: list[dict[str, Any]] = []
        self.actions[symbol] = result
        for row in rows:
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{path}:{symbol}[{row['index']}]"
            if op == "ac_jump" and args == ["eas_Idle"]:
                self.finish_in_idle(result)
                break
            result.extend(self.action(op, args, source))
            if op in ("ac_end", "ac_jump"):
                break
        return result

    def action(self, op: str, args: list[str], source: str) -> list[dict[str, Any]]:
        directions = {
            "Right": (1, 0),
            "Up": (0, -1),
            "Left": (-1, 0),
            "Down": (0, 1),
            "UpRight": (1, -1),
            "UpLeft": (-1, -1),
            "DownLeft": (-1, 1),
            "DownRight": (1, 1),
        }
        if op.startswith("move") and op[4:] in directions:
            x, y = directions[op[4:]]
            return [{"op": "move", "x": x * self.number(args[0]), "y": y * self.number(args[0])}]
        if op.startswith("face") and op[4:] in directions:
            facing = [
                "Right",
                "Up",
                "Left",
                "Down",
                "UpRight",
                "UpLeft",
                "DownLeft",
                "DownRight",
            ].index(op[4:])
            return [{"op": "face", "facing": facing}, {"op": "wait", "ticks": self.number(args[0])}]
        if op == "eaWait" or op == "ac_wait":
            return [{"op": "wait", "ticks": self.number(args[0])}]
        if op == "ac_moveRel":
            return [
                {"op": "move", "x": self.number(args[0]), "y": self.number(args[1]), "wait": False}
            ]
        if op in ("ac_setSpeed", "ac_accelFactors"):
            return [
                {
                    "op": "speed" if op == "ac_setSpeed" else "acceleration",
                    "x": self.number(args[0]),
                    "y": self.number(args[1]),
                }
            ]
        flag_ops = {
            "ac_entityObstructable": ("a", 128),
            "ac_mapUncollidable": ("a", 64),
            "ac_entityUncollidable": ("a", 32),
            "ac_autoFacing": ("b", 64),
            "ac_immersed": ("b", 32),
        }
        if op in flag_ops:
            field, mask = flag_ops[op]
            return [
                {
                    "op": "flags",
                    "field": field,
                    "mask": mask,
                    "value": mask if args[0] == "ON" else 0,
                }
            ]
        if op in ("ac_acceleration", "ac_deceleration"):
            shift = 0 if op == "ac_acceleration" else 2
            return [
                {
                    "op": "flags",
                    "field": "a",
                    "mask": 3 << shift,
                    "value": (
                        (int(self.number(args[0]) != 0) | (int(self.number(args[1]) != 0) << 1))
                        << shift
                    ),
                }
            ]
        if op in ("ac_orientUp", "ac_orientLeft", "ac_orientDown", "ac_orientRight"):
            return [
                {
                    "op": "flags",
                    "field": "b",
                    "mask": 3,
                    "value": [
                        "ac_orientUp",
                        "ac_orientLeft",
                        "ac_orientDown",
                        "ac_orientRight",
                    ].index(op),
                }
            ]
        if op == "ac_setSize":
            return [{"op": "sprite-size", "size": self.number(args[0])}]
        if op == "ac_updateSprite":
            return [{"op": "refresh-sprite", "source": source}]
        if op == "ac_setFacing":
            return [{"op": "face", "facing": self.number(args[0])}]
        return [self.native(op, source)]

    def compile(self, symbol: str) -> str:
        if symbol in self.programs:
            return symbol
        if symbol not in self.raw:
            raise ValueError("source program target not imported: " + symbol)
        raw = self.raw[symbol]
        relative = "disasm/" + raw["path"]
        self.sources.add(relative)
        result: list[dict[str, Any]] = []
        self.programs[symbol] = {
            "id": symbol.lower().replace("_", "-"),
            "source": f"{relative}:{symbol}",
            "instructions": result,
        }
        rows, index = raw["operations"], 0
        registers: dict[str, int] = {}
        speaker = None
        self.programs[symbol]["entitiesRunning"] = not any(
            row["opcode"] in ("chkFlg", "txt", "script", "rts") for row in rows
        )
        while index < len(rows):
            row = rows[index]
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{relative}:{symbol}[{row['index']}]"
            index += 1
            if op in ("csc_end", "rts") or (op == "dc.w" and args == ["$FFFF"]):
                result.append({"op": "end"})
            elif (
                op == "chkFlg"
                and index < len(rows)
                and rows[index]["opcode"] in ("beq.s", "beq.w", "bne.s", "bne.w")
            ):
                branch = rows[index]
                index += 1
                result.append(
                    {
                        "op": "branch-flag",
                        "flag": self.number(args[0]),
                        "whenSet": branch["opcode"].startswith("bne"),
                        "target": _location(self.compile(branch["operandText"])),
                    }
                )
            elif op in ("bra.s", "bra.w"):
                result.append({"op": "jump", "target": _location(self.compile(args[0]))})
            elif (
                op == "cmpi.l"
                and len(args) == 2
                and args[0].startswith("#")
                and args[1] == "((ENTITY_DATA-$1000000)).w"
                and index < len(rows)
                and rows[index]["opcode"] in ("beq.s", "beq.w", "bne.s", "bne.w")
            ):
                packed = self.number(args[0][1:])
                x, y = (packed >> 16) & 65535, packed & 65535
                branch = rows[index]
                index += 1
                result.append(
                    {
                        "op": "branch-coordinates",
                        "entity": "entity-0",
                        "x": x if x < 32768 else x - 65536,
                        "y": y if y < 32768 else y - 65536,
                        "whenEqual": branch["opcode"].startswith("beq"),
                        "target": _location(self.compile(branch["operandText"])),
                    }
                )
            elif op == "script":
                result.append({"op": "call", "target": _location(self.compile(args[0]))})
            elif (
                op in ("moveq", "move.w")
                and len(args) == 2
                and args[0].startswith("#")
                and re.fullmatch("d[0-7]", args[1])
            ):
                registers[args[1]] = self.number(args[0][1:])
            elif op == "move.w" and args in (
                ["((CURRENT_SPEECH_SFX-$1000000)).w", "((SPEECH_SFX_COPY-$1000000)).w"],
                ["d1", "((CURRENT_PORTRAIT-$1000000)).w"],
                ["d2", "((CURRENT_SPEECH_SFX-$1000000)).w"],
            ):
                # These stores are represented by the following typed dialogue speaker binding.
                pass
            elif op == "jsr" and args == ["GetEntityPortaitAndSpeechSfx"]:
                speaker = self.entity(str(registers["d0"]))
            elif op == "jsr" and args == ["DisplayCurrentPortrait"]:
                result.append({"op": "speaker", "entity": speaker})
                result.append(
                    {"op": "open-portrait", "entity": speaker, "flags": 0}
                    if speaker is not None
                    else self.native(op, source)
                )
            elif op == "jsr" and args == ["j_ClosePortraitWindow"]:
                result.append({"op": "close-portrait"})
            elif op == "jsr" and args == ["(WaitForViewScrollEnd).l"]:
                result.append(
                    {
                        "op": "present",
                        "kind": "CameraWait",
                        "entity": None,
                        "position": None,
                        "resource": None,
                    }
                )
            elif op == "jsr" and args == ["MakeEntityWalk"]:
                result.append(
                    {
                        "op": "motion",
                        "entity": self.entity(str(registers["d0"])),
                        "wait": False,
                        "installation": "Preserve",
                        "actions": self.walking(registers["d1"], registers["d2"], registers["d3"]),
                    }
                )
            elif op == "jsr" and args == ["MoveEntityOutOfMap"]:
                result.append(
                    {"op": "retired-map3-entity-scratch"}
                    if relative.endswith("maps/entries/map03/mapsetups/s6_initfunction.asm")
                    and symbol == "byte_513A8"
                    and registers["d0"] == 142
                    else {
                        "op": "hide",
                        "entity": self.entity(str(registers["d0"])),
                        "removeAliases": False,
                    }
                )
            elif op == "txt":
                result.extend(
                    [
                        {"op": "text-cursor", "text": self.number(args[0])},
                        {
                            "op": "show-text",
                            "mode": "single",
                            "speaker": speaker,
                            "useEventSpeaker": speaker is None,
                            "explicitWindows": True,
                        },
                    ]
                )
            elif op in ("jump", "jumpIfFlagSet", "jumpIfFlagClear"):
                target = self.compile(args[-1])
                result.append(
                    {"op": "jump", "target": _location(target)}
                    if op == "jump"
                    else {
                        "op": "branch-flag",
                        "flag": self.number(args[0]),
                        "whenSet": op == "jumpIfFlagSet",
                        "target": _location(target),
                    }
                )
            elif op in ("setF", "clearF", "setFlg", "clrFlg", "setStoryFlag", "clearStoryFlag"):
                result.append(
                    {
                        "op": "set-flag",
                        "flag": self.number(args[0]) + (400 if "Story" in op else 0),
                        "value": op in ("setF", "setFlg", "setStoryFlag"),
                    }
                )
            elif op == "textCursor":
                result.append({"op": "text-cursor", "text": self.number(args[0])})
            elif op in ("nextText", "nextSingleText"):
                flags, selector = self.number(args[0]) & 255, self.number(args[1]) & 255
                # FFFF skips portrait lookup; it does not close an existing window.
                entity = None if (flags, selector) == (255, 255) else self.entity(args[1])
                result.append({"op": "open-portrait", "entity": entity, "flags": flags})
                result.append({"op": "wait-view"})
                result.append(
                    {
                        "op": "show-text",
                        "mode": "single" if op == "nextSingleText" else "continued",
                        "speaker": entity,
                        "speakerFlags": flags,
                        "explicitWindows": True,
                    }
                )
                if op == "nextSingleText":
                    result.extend(
                        [
                            {"op": "close-portrait"},
                            {"op": "close-text"},
                            {"op": "wait-ticks", "ticks": 10},
                        ]
                    )
            elif op == "showPortrait":
                result.append(
                    {
                        "op": "open-portrait",
                        "entity": self.entity(args[1]),
                        "flags": self.number(args[0]) & 255,
                    }
                )
            elif op == "hidePortrait":
                result.extend(
                    [
                        {
                            "op": "present",
                            "kind": "CameraWait",
                            "entity": None,
                            "position": None,
                            "resource": None,
                        },
                        {"op": "close-portrait"},
                    ]
                )
            elif op == "hide":
                result.append({"op": "hide", "entity": self.entity(args[0]), "removeAliases": True})
            elif op == "followEntity":
                position_path = "disasm/code/common/scripting/map/mapscriptengine_1.asm"
                positions = [
                    self.number(arg)
                    for arg in _arguments(
                        "\n".join(
                            row["opcode"] + " " + row["operandText"]
                            for row in self.source_operations(
                                position_path, "table_FollowerPositions"
                            )
                        ),
                        "dc.b",
                    )
                ]
                offset = self.number(args[2]) * 2
                result.append(
                    {
                        "op": "follow",
                        "entity": self.entity(args[0]),
                        "leader": self.entity(args[1]),
                        "x": positions[offset],
                        "y": positions[offset + 1],
                    }
                )
            elif op == "join":
                selector = self.number(args[0])
                members = [1, 2] if selector & 32767 == 128 else [selector & 32767]
                result.append(
                    {
                        "op": "present",
                        "kind": "CameraWait",
                        "entity": None,
                        "position": None,
                        "resource": None,
                    }
                )
                result.append(
                    {
                        "op": "present",
                        "kind": "Sound",
                        "resource": "MUSIC_SAD_JOIN" if selector & 32768 else "MUSIC_JOIN",
                        "entity": None,
                        "position": None,
                    }
                )
                result.extend({"op": "join-party", "member": member} for member in members)
                result.extend(
                    [
                        {"op": "text-cursor", "text": 447 if selector & 32767 == 128 else 446},
                        {
                            "op": "show-text",
                            "mode": "single",
                            "speaker": None,
                            "waitForAcknowledgement": False,
                            "explicitWindows": True,
                        },
                        {
                            "op": "present",
                            "kind": "SoundWait",
                            "resource": None,
                            "entity": None,
                            "position": None,
                        },
                        {
                            "op": "present",
                            "kind": "PreviousMusic",
                            "resource": None,
                            "entity": None,
                            "position": None,
                        },
                        {"op": "wait-text-input"},
                        {"op": "close-text"},
                        {"op": "wait-ticks", "ticks": 10},
                    ]
                )
            elif op in ("entityNodHead", "nod"):
                result.append(
                    {
                        "op": "present",
                        "kind": "Gesture",
                        "resource": "nod",
                        "entity": self.entity(args[0]),
                        "position": None,
                    }
                )
            elif op == "setCamDest":
                result.append(
                    {
                        "op": "camera-target",
                        "position": {"x": self.number(args[0]), "y": self.number(args[1])},
                    }
                )
                result.append(
                    {
                        "op": "present",
                        "kind": "CameraWait",
                        "resource": None,
                        "entity": None,
                        "position": None,
                    }
                )
            elif op == "setCameraEntity":
                result.append(
                    {
                        "op": "camera-entity",
                        "entity": None if self.number(args[0]) == -1 else self.entity(args[0]),
                    }
                )
            elif op == "loadMapFadeIn" and (
                self.scene_maps is None or self.number(args[0]) in self.scene_maps
            ):
                # csc37 starts out-to-black and falls through csc48. The source's later fadeInB
                # is a separate operation; loading a scene does not execute map init/population.
                result.extend(
                    [
                        {
                            "op": "present",
                            "kind": "FadeOut",
                            "resource": "black",
                            "entity": None,
                            "position": None,
                        },
                        {
                            "op": "scene-map",
                            "map": "map-" + str(self.number(args[0])),
                            "camera": {"x": self.number(args[1]), "y": self.number(args[2])},
                        },
                    ]
                )
            elif op == "loadMapEntities":
                result.append(self.scene_entities(relative, args[0]))
            elif op in ("mapFadeOutToWhite", "mapFadeInFromWhite"):
                result.append(
                    {
                        "op": "present",
                        "kind": "FadeOut" if op == "mapFadeOutToWhite" else "FadeIn",
                        "resource": "white",
                        "entity": None,
                        "position": None,
                    }
                )
            elif op == "resetForceBattleStats":
                self.sources.add("disasm/code/common/scripting/map/resetalliesstats.asm")
                result.append({"op": "reset-party-battle-stats"})
            elif op == "animEntityFX" and args[1] in ("MOSAIC_IN", "MOSAIC_OUT"):
                result.append(
                    {
                        "op": "present",
                        "kind": "EntityEffect",
                        "resource": "mosaic-in" if args[1] == "MOSAIC_IN" else "mosaic-out",
                        "entity": self.entity(args[0]),
                        "position": None,
                    }
                )
            elif op == "shiver":
                result.append(
                    {
                        "op": "present",
                        "kind": "Gesture",
                        "resource": "shiver",
                        "entity": self.entity(args[0]),
                        "position": None,
                    }
                )
            elif op == "closeTxt":
                result.extend([{"op": "close-portrait"}, {"op": "close-text"}])
            elif op == "clsTxt":
                result.append({"op": "close-text"})
            elif op == "yesNo":
                result.append({"op": "yes-no", "flag": 89})
            elif op == "csWait":
                result.append({"op": "wait-ticks", "ticks": self.number(args[0])})
            elif op == "setSprite" and self.number(args[1]) >= 30:
                result.append(
                    {"op": "sprite", "entity": self.entity(args[0]), "sprite": self.number(args[1])}
                )
            elif op == "setFacing":
                result.append(
                    {
                        "op": "face",
                        "entity": self.entity(args[0]),
                        "facing": self.number(args[1]),
                        "refreshSprite": True,
                    }
                )
            elif op == "setPriority":
                result.append(
                    {
                        "op": "priority",
                        "entity": self.entity(args[0]),
                        "value": self.number(args[1]) != 0,
                    }
                )
            elif op in ("fadeInB", "fadeOutB", "slowFadeInB", "slowFadeOutB"):
                result.append(
                    {
                        "op": "present",
                        "kind": "FadeIn" if "In" in op else "FadeOut",
                        "resource": "black",
                        "entity": None,
                        "position": None,
                        "fullBlack": {"period": 6 if op.startswith("slow") else None},
                    }
                )
            elif op == "setPos":
                result.append(
                    {
                        "op": "position",
                        "entity": self.entity(args[0]),
                        "position": {"x": self.number(args[1]), "y": self.number(args[2])},
                        "facing": self.number(args[3]),
                    }
                )
            elif op in (
                "entityActions",
                "entityActionsWait",
                "customActscript",
                "customActscriptWait",
            ):
                actions = []
                entered_idle = False
                while index < len(rows) and rows[index]["opcode"] not in ("endActions", "ac_end"):
                    action = rows[index]
                    if entered_idle:
                        # Consume inline bytes through the delimiter, without making code
                        # after an unconditional idle jump executable.
                        index += 1
                        continue
                    if action["opcode"] == "ac_jump" and _tokens(action["operandText"]) == [
                        "eas_Idle"
                    ]:
                        self.finish_in_idle(actions)
                        entered_idle = True
                        index += 1
                        continue
                    actions.extend(
                        self.action(
                            action["opcode"],
                            _tokens(action["operandText"]),
                            f"{relative}:{symbol}[{action['index']}]",
                        )
                    )
                    index += 1
                if index == len(rows):
                    raise ValueError("unterminated entity action: " + symbol)
                index += 1
                sequence = op.startswith("entityActions")
                if sequence and not entered_idle:
                    self.finish_in_idle(actions)
                result.append(
                    {
                        "op": "motion",
                        "entity": self.entity(args[0]),
                        "wait": op.endswith("Wait"),
                        "installation": "SlotTimerClearCollision" if sequence else "SlotTimer",
                        "actions": actions,
                    }
                )
            elif op in ("setActscript", "setActscriptWait"):
                actions = self.action_stream(args[1])
                result.append(
                    {
                        "op": "motion",
                        "entity": self.entity(args[0]),
                        "wait": op.endswith("Wait"),
                        "installation": "SlotTimer",
                        "actions": actions,
                    }
                )
            else:
                # Presentation/native services are not acknowledged by this compiler. The
                # native owner must implement the service before this instruction can finish.
                result.append(self.native(op, source))
        if not result or result[-1]["op"] not in ("end", "return", "jump", "native-call"):
            source_text = (self.upstream / relative).read_text(encoding="utf-8")
            labels = list(re.finditer(r"^([A-Za-z_]\w*):", source_text, re.MULTILINE))
            label_index = next(i for i, match in enumerate(labels) if match.group(1) == symbol)
            successor = labels[label_index + 1].group(1) if label_index + 1 < len(labels) else None
            if successor is None or successor not in self.raw:
                raise ValueError("source program fallthrough requires an explicit owner: " + symbol)
            result.append({"op": "jump", "target": _location(self.compile(successor))})
        return symbol

    def frontier(self, symbol: str, source: str) -> str:
        self.programs.setdefault(
            symbol,
            {
                "id": symbol.lower().replace("_", "-"),
                "source": source,
                "instructions": [self.native(symbol, source)],
            },
        )
        return symbol


def prepare_visuals(
    compiler: OriginalPrograms,
    canonical: dict[str, Any],
    maps: list[dict[str, Any]],
    rom_path: Path,
    asset_root: Path,
    manifest_sha256: str,
    atlas_bindings: dict[str, str],
) -> dict[str, Any]:
    rom = rom_path.read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != canonical["romSha256"]:
        raise ValueError("visual source ROM identity mismatch")
    manifest_bytes = (asset_root / "manifests/presentation-assets-v1.json").read_bytes()
    if hashlib.sha256(manifest_bytes).hexdigest().upper() != manifest_sha256:
        raise ValueError("admitted presentation manifest identity mismatch")
    manifest = json.loads(manifest_bytes)
    assets = {asset["assetId"]: asset for asset in manifest["assets"]}

    def raster(width, height, data, format="rgba8"):
        data = bytes(data)
        return {
            "width": width,
            "height": height,
            "format": format,
            "data": base64.b64encode(data).decode("ascii"),
            "sha256": hashlib.sha256(data).hexdigest().upper(),
        }

    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    map_definitions = {"map-" + str(row["id"]): row for row in canonical["maps"]}
    map_visuals = []
    sprites = {0}
    # PlayMapMusic's explicit original field-to-battle substitutions.
    music_replacements = {
        compiler.equates[name]: compiler.equates[target]
        for name, target in (
            ("MUSIC_NOTHING", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_TOWN", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_MITULA", "MUSIC_BATTLE_THEME_3"),
            ("MUSIC_MITULA_SHRINE", "MUSIC_BATTLE_THEME_1"),
            ("MUSIC_CASTLE", "MUSIC_BATTLE_THEME_1"),
        )
    }
    compiler.sources.add("disasm/code/gameflow/battle/battlemusic.asm")
    for map in maps:
        refs = map_definitions[map["id"]]["references"]
        asset = assets[atlas_bindings[map["id"]]]
        bucket = next(bucket for bucket in asset["buckets"] if bucket["scale"] == 2)
        path = (asset_root / bucket["runtimePath"]).resolve(strict=True)
        if not path.is_relative_to(asset_root.resolve()):
            raise ValueError("presentation asset path escapes selected root")
        payload = path.read_bytes()
        if (
            len(payload) != bucket["byteLength"]
            or hashlib.sha256(payload).hexdigest().upper() != bucket["sha256"]
        ):
            raise ValueError("admitted raster identity mismatch")
        map_visuals.append(
            {
                "map": map["id"],
                "music": [
                    {
                        "field": area["defaultMusic"],
                        "battle": music_replacements.get(
                            area["defaultMusic"], area["defaultMusic"]
                        ),
                    }
                    for area in resources["areaTables"][refs["areaTable"]]["records"]
                ],
                "atlas": raster(bucket["width"], bucket["height"], payload, "png"),
                "scale": bucket["scale"],
                "blocks": resources["blocksets"][refs["blockset"]]["blocks"],
            }
        )
        if "population" in map:
            sprites.update(
                entity["sprite"]
                for entity in map["entities"]
                if entity["sprite"] >= map["population"]["allyCount"]
            )
            sprites.update(row["sprite"] for row in map["population"]["allySprites"])
            sprites.update(
                row["unjoinedSprite"]
                for row in map["population"]["allySprites"]
                if row["unjoinedSprite"] is not None
            )
            sprites.update(
                row["sprite"]
                for row in map["population"]["followers"]
                if row["character"] >= map["population"]["allyCount"]
            )
    for program in compiler.programs.values():
        for instruction in program["instructions"]:
            if instruction["op"] == "sprite":
                sprites.add(instruction["sprite"])
            if instruction["op"] == "scene-entities":
                population = instruction["population"]
                sprites.update(
                    row["sprite"]
                    for row in instruction["entities"]
                    if row["sprite"] >= population["allyCount"]
                )
                sprites.update(row["sprite"] for row in population["allySprites"])
                sprites.update(
                    row["unjoinedSprite"]
                    for row in population["allySprites"]
                    if row["unjoinedSprite"] is not None
                )
                sprites.update(
                    row["sprite"]
                    for row in population["followers"]
                    if row["character"] >= population["allyCount"]
                )
    properties_path = "disasm/data/spritedialogproperties.asm"
    compiler.sources.update(
        (
            properties_path,
            "disasm/data/graphics/mapsprites/entries.asm",
            "disasm/data/graphics/portraits/entries.asm",
            "disasm/code/common/scripting/entity/getentityportaitandspeechsfx.asm",
            "disasm/code/common/scripting/entity/entityfunctions_4.asm",
            "disasm/code/common/menus/portraitfunctions.asm",
        )
    )
    properties = (compiler.upstream / properties_path).read_text(encoding="utf-8")
    columns = [
        [compiler.number(prefix + value) for value in _arguments(properties, macro)]
        for prefix, macro in (
            ("MAPSPRITE_", "mapsprite"),
            ("PORTRAIT_", "portrait"),
            ("SFX_", "speechSfx"),
        )
    ]
    dialogue = {
        sprite: (portrait if portrait < 128 else None, speech)
        for sprite, portrait, speech in zip(*columns, strict=True)
    }
    palette = [
        md_palette_color(int.from_bytes(rom[offset : offset + 2], "big"))
        for offset in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
    ]
    sprite_visuals = []
    for sprite in sorted(sprites):
        directions = []
        for direction in range(3):
            entry = PLAYER_POINTER_TABLE_ADDRESS + (sprite * 3 + direction) * 4
            address = int.from_bytes(rom[entry : entry + 4], "big")
            decoded = decode_basic_compressed(rom[address:], expected_output_bytes=576).output
            directions.append(
                raster(
                    48,
                    24,
                    _combine_player_halves(
                        _render_player_frame(decoded[:288], palette),
                        _render_player_frame(decoded[288:], palette),
                    ),
                )
            )
        portrait, speech = dialogue.get(sprite, (None, compiler.equates["SFX_DIALOG_BLEEP_6"]))
        sprite_visuals.append(
            {"sprite": sprite, "directions": directions, "portrait": portrait, "speech": speech}
        )
    portrait_visuals = []
    for portrait in sorted(
        {row["portrait"] for row in sprite_visuals if row["portrait"] is not None}
    ):
        # pt_Portraits: data/graphics/portraits/entries.asm, accepted pointer table at 0x1C8004.
        entry = 0x1C8004 + portrait * 4
        address = int.from_bytes(rom[entry : entry + 4], "big")
        data = rom[address:]
        _, offset = _read_animation_entries(data, 0, "eye")
        _, offset = _read_animation_entries(data, offset, "mouth")
        palette = [
            md_palette_color(int.from_bytes(data[index : index + 2], "big"))
            for index in range(offset, offset + 32, 2)
        ]
        decoded = decode_stack_compressed(data[offset + 32 :], expected_output_bytes=2048).output
        pixels = bytearray(64 * 64 * 4)
        for tile_id in range(64):
            tile = decode_md_4bpp_tile(decoded[tile_id * 32 : (tile_id + 1) * 32])
            for y in range(8):
                for x in range(8):
                    target = ((tile_id // 8 * 8 + y) * 64 + tile_id % 8 * 8 + x) * 4
                    pixels[target : target + 4] = bytes(
                        palette_index_rgba(palette, tile[y * 8 + x])
                    )
        portrait_visuals.append({"portrait": portrait, "raster": raster(64, 64, pixels)})
    audio = []
    for asset in manifest["assets"]:
        if asset["kind"] != "audio":
            continue
        runtime = asset["runtime"]
        path = (asset_root / runtime["runtimePath"]).resolve(strict=True)
        if not path.is_relative_to(asset_root.resolve()):
            raise ValueError("audio asset path escapes selected root")
        payload = path.read_bytes()
        if (
            len(payload) != runtime["byteLength"]
            or hashlib.sha256(payload).hexdigest().upper() != runtime["sha256"].upper()
        ):
            raise ValueError("admitted audio identity mismatch")
        with wave.open(io.BytesIO(payload), "rb") as sound:
            if (
                sound.getsampwidth() != 2
                or sound.getcomptype() != "NONE"
                or sound.getnchannels() != runtime["channels"]
                or sound.getframerate() != runtime["sampleRate"]
                or sound.getnframes() != runtime["sampleFrames"]
            ):
                raise ValueError("admitted audio format mismatch")
            pcm = sound.readframes(sound.getnframes())
            if len(pcm) != sound.getnframes() * sound.getnchannels() * 2:
                raise ValueError("admitted audio is truncated")
        audio.append(
            {
                "cue": asset["cue"],
                "command": asset["command"],
                "timerB": asset["timerB"],
                "sampleRate": runtime["sampleRate"],
                "channels": runtime["channels"],
                "sampleFrames": runtime["sampleFrames"],
                "loopBegin": runtime["loopBegin"],
                "loopEnd": runtime["loopEnd"],
                "pcm16": base64.b64encode(pcm).decode("ascii"),
                "sha256": hashlib.sha256(pcm).hexdigest().upper(),
            }
        )
    if not audio:
        raise ValueError("private original audio is missing from the selected asset pack")
    return {
        "maps": map_visuals,
        "sprites": sprite_visuals,
        "portraits": portrait_visuals,
        "audio": audio,
    }


def prepare(
    canonical_path: Path,
    upstream: Path,
    selection_path: Path,
    output: Path,
    rom_path: Path | None = None,
    presentation_root: Path | None = None,
) -> dict[str, int]:
    output = output.resolve()
    repository = Path(__file__).resolve().parents[2]
    if (
        not output.is_relative_to(repository / "local")
        or output in (canonical_path.resolve(), selection_path.resolve())
        or output.is_relative_to(upstream.resolve())
    ):
        raise ValueError("output must be ignored scratch distinct from read-only inputs")
    canonical = load_json(canonical_path)
    manifest = load_json(MANIFEST)
    if hashlib.sha256(_canonical_bytes(canonical)).hexdigest().upper() != manifest["outputSha256"]:
        raise ValueError("registered canonical input identity mismatch")
    expected_commit = canonical["upstream"]["commit"]
    completed = subprocess.run(
        ["git", "rev-parse", "HEAD"], cwd=upstream, capture_output=True, text=True, check=True
    )
    if completed.stdout.strip() != expected_commit:
        raise ValueError("pinned source commit mismatch")
    selection = load_json(selection_path)
    compiler = OriginalPrograms(
        canonical, upstream, selection["controlledEntitySpeed"], selection["maps"]
    )
    for selected in selection.get("eventMaps", []):
        folder = upstream / f"disasm/data/maps/entries/map{selected:02d}/mapsetups"
        for path in sorted(folder.glob("*.asm")):
            if path.name.startswith(
                ("s2_entityevents", "s3_zoneevents", "s6_initfunction", "scripts")
            ):
                compiler.register_file(path.relative_to(upstream).as_posix())
    for source in selection["additionalProgramSources"]:
        compiler.raw[source["symbol"]] = {
            "id": source["symbol"],
            "path": source["path"],
            "operations": compiler.source_operations("disasm/" + source["path"], source["symbol"]),
        }
    for symbol in selection["programs"]:
        compiler.compile(symbol)
    battle_routes = {}
    load_source = "disasm/code/gameflow/battle/battlefunctions/loadBattle.asm"
    compiler.sources.update((load_source, "disasm/code/gameflow/battle/battleloop_1.asm"))
    compiler.programs["source-battle-load"] = {
        "id": "source-battle-load",
        "source": load_source + ":LoadBattle",
        "entitiesRunning": False,
        "instructions": [
            {
                "op": "present",
                "kind": "FadeOut",
                "resource": "black",
                "entity": None,
                "position": None,
            },
            {
                "op": "present",
                "kind": "BattleLoad",
                "resource": None,
                "entity": None,
                "position": None,
            },
            {
                "op": "present",
                "kind": "FadeIn",
                "resource": "black",
                "entity": None,
                "position": None,
            },
            {"op": "end"},
        ],
    }
    coordinates_path = "disasm/data/battles/global/battlemapcoords.asm"
    compiler.sources.add(coordinates_path)
    coordinates = _arguments(
        (upstream / coordinates_path).read_text(encoding="utf-8"), "battleMapCoordinates"
    )
    for battle_id in selection["battles"]:
        map_id, _, _, _, _, trigger_x, trigger_y = map(
            compiler.number, _tokens(coordinates[battle_id])
        )
        if (trigger_x, trigger_y) != (255, 255):
            raise ValueError("coordinate-triggered battle selection is not admitted")
        hooks = []
        for hook in ("beforebattle", "battlestart"):
            path = "disasm/data/battles/cutscenes/" + hook + "cutscenes.asm"
            compiler.sources.add(path)
            expressions = _arguments((upstream / path).read_text(encoding="utf-8"), "dc.w")
            symbol = re.search(r"[A-Za-z_]\w*", expressions[battle_id]).group(0)
            hooks.append(_location(compiler.compile(symbol)))
        battle_routes[map_id] = {
            "encounter": "battle-" + str(battle_id),
            "unlockedFlag": 400 + battle_id,
            "completedFlag": 500 + battle_id,
            "introFlag": 450 + battle_id,
            "before": hooks[0],
            "start": hooks[1],
            "load": _location("source-battle-load"),
        }
        if "outcomeEgressMap" in selection:
            compiler.sources.update(
                (
                    "disasm/code/gameflow/battle/battleloop_2.asm",
                    "disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm",
                    "disasm/code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm",
                    "disasm/code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm",
                    "disasm/code/gameflow/battle/battleloop/getegresspositionforbattle.asm",
                    "disasm/code/common/maps/egressinit.asm",
                    "disasm/code/common/maps/mapinit_0.asm",
                    "disasm/code/common/windows/windowengine.asm",
                    "disasm/code/common/tech/interrupts/vintengine_2.asm",
                )
            )
            outcome_hooks = []
            for hook in ("battleend", "afterbattle"):
                path = "disasm/data/battles/cutscenes/" + hook + "cutscenes.asm"
                compiler.sources.add(path)
                expressions = _arguments((upstream / path).read_text(encoding="utf-8"), "dc.w")
                symbol = re.search(r"[A-Za-z_]\w*", expressions[battle_id]).group(0)
                outcome_hooks.append(_location(compiler.compile(symbol)))
            joins_path = "disasm/data/battles/cutscenes/afterbattlejoins.asm"
            compiler.sources.add(joins_path)
            joins = _arguments((upstream / joins_path).read_text(encoding="utf-8"), "dc.b")
            savepoints_path = "disasm/data/maps/global/savepointmapcoords.asm"
            compiler.sources.add(savepoints_path)
            savepoints = [
                list(map(compiler.number, _tokens(row)))
                for row in _arguments(
                    (upstream / savepoints_path).read_text(encoding="utf-8"),
                    "savePointMapCoordinates",
                )
            ]
            egress, ex, ey, facing = next(
                row for row in savepoints if row[0] == selection["outcomeEgressMap"]
            )
            battle_routes[map_id]["outcome"] = {
                "after": outcome_hooks[1],
                "defeated": outcome_hooks[0],
                "joinMember": compiler.number(joins[battle_id]),
                "victoryFacing": compiler.equates["DOWN"],
                "defeat": _location("source-ordinary-defeat"),
                "return": _location("source-outcome-return"),
                "egress": {
                    "map": "map-" + str(egress),
                    "position": {"x": ex, "y": ey},
                    "facing": facing,
                },
            }
    if "outcomeEgressMap" in selection:
        compiler.programs["source-ordinary-defeat"] = {
            "id": "source-ordinary-defeat",
            "source": "disasm/code/gameflow/battle/battleloop_2.asm:BattleLoop_Defeat",
            "entitiesRunning": False,
            "instructions": [
                {
                    "op": "present",
                    "kind": "Sound",
                    "resource": "MUSIC_SAD_THEME_2",
                    "entity": None,
                    "position": None,
                },
                {"op": "text-cursor", "text": 363},
                {"op": "show-text", "mode": "single", "speaker": None},
                {"op": "close-text"},
                {"op": "end"},
            ],
        }
        compiler.programs["source-outcome-return"] = {
            "id": "source-outcome-return",
            "source": "disasm/code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop",
            "entitiesRunning": False,
            "instructions": [
                {
                    "op": "present",
                    "kind": "FadeOut",
                    "resource": "black",
                    "entity": None,
                    "position": None,
                },
                {"op": "battle-return-map"},
                {
                    "op": "present",
                    "kind": "FadeIn",
                    "resource": "black",
                    "entity": None,
                    "position": None,
                },
                {"op": "end"},
            ],
        }
    resources = {
        key: {row["id"]: row for row in rows} for key, rows in canonical["resources"].items()
    }
    map_rows = {row["id"]: row for row in canonical["maps"]}
    maps = []
    for selected in selection["maps"]:
        row = map_rows[selected]
        references = row["references"]
        words = resources["layouts"][references["layout"]]["words"]
        areas = resources["areaTables"][references["areaTable"]]["records"]
        route_id = references["setupRoute"]
        route = resources["setupRoutes"].get(route_id)
        entities = []
        if route is not None:
            setup = resources["setupDefinitions"][route["defaultSetup"]]
            records = resources["entityLists"][setup["references"]["entities"]]["records"]
            npc = 128
            for entity in records:
                sprite = entity["mapSprite"]
                if sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"]:
                    raise ValueError("special entity index mapping is not admitted")
                identity = sprite if sprite < 30 else npc
                if sprite >= 30:
                    npc += 1
                entities.append(
                    {
                        "id": "entity-" + str(identity),
                        "position": {"x": entity["x"], "y": entity["y"]},
                        "facing": entity["facing"],
                        "speed": selection["controlledEntitySpeed"],
                        "visible": True,
                        "obstruction": False,
                    }
                )
        # Every source warp row is retained. Unadmitted destination/setup/scroll branches stop
        # before transfer; selected simple rows use the ordinary common transfer rule.
        events = []
        for index, warp in enumerate(
            resources["warpEventTables"][references["warpEventTable"]]["records"]
        ):
            destination = selected if warp["targetMap"] == 255 else warp["targetMap"]
            common = {
                "x": None if warp["trigger"]["x"] == 255 else warp["trigger"]["x"],
                "y": None if warp["trigger"]["y"] == 255 else warp["trigger"]["y"],
                "marker": 4096,
                "requiredFlag": None,
                "requiredValue": True,
            }
            if (
                destination in selection["maps"]
                and not warp["retainsCoordinates"]
                and warp["scrollMode"] == 0
            ):
                events.append(
                    {
                        "kind": "warp",
                        **common,
                        "map": "map-" + str(destination),
                        "position": warp["destination"],
                        "facing": warp["facing"],
                        "loadMode": "preserve" if warp["targetMap"] == 255 else "rebuild",
                    }
                )
            else:
                symbol = compiler.frontier(
                    f"map-{selected}-warp-{index}", f"{references['warpEventTable']}[{index}]"
                )
                events.append({"kind": "warp-frontier", **common, "program": _location(symbol)})
        population = (
            compiler.population() if route is None and "outcomeEgressMap" in selection else None
        )
        layout_events = None
        if selected in selection.get("eventMaps", []):
            folder = f"disasm/data/maps/entries/map{selected:02d}/mapsetups/"
            source_rows = compiler.source_operations(
                folder + "s1_entities.asm", setup["references"]["entities"]
            )
            source_rows = [source for source in source_rows if source["opcode"] != "msEntitiesEnd"]
            if len(source_rows) != len(records):
                raise ValueError("entity source/canonical cardinality mismatch")
            for entity, source_row, source_record in zip(
                entities, source_rows, records, strict=True
            ):
                args = _tokens(source_row["operandText"])
                entity["sprite"] = source_record["mapSprite"]
                entity["actions"] = (
                    compiler.walking(
                        **{
                            "x": source_record["walking"]["originX"],
                            "y": source_record["walking"]["originY"],
                            "radius": source_record["walking"]["range"],
                        }
                    )
                    if source_record["kind"] == "walking"
                    else compiler.action_stream(args[4])
                )
            population = compiler.population()
            for key, filename, macro, default in (
                ("entityEvents", "s2_entityevents.asm", "msEntityEvent", "msDefaultEntityEvent"),
                ("zoneEvents", "s3_zoneevents.asm", "msZoneEvent", "msDefaultZoneEvent"),
            ):
                table = compiler.source_operations(folder + filename, setup["references"][key])
                for source_row in table:
                    args = _tokens(source_row["operandText"])
                    if source_row["opcode"] not in (macro, default):
                        raise ValueError(
                            "unsupported event table row: "
                            f"{folder}{filename}[{source_row['index']}]"
                        )
                    symbol = args[-1].split("-")[0]
                    common = {
                        "program": _location(compiler.compile(symbol)),
                        "requiredFlag": None,
                        "requiredValue": True,
                    }
                    if key == "entityEvents":
                        events.append(
                            {
                                "kind": "interact",
                                **common,
                                "entity": compiler.entity(args[0])
                                if source_row["opcode"] == macro
                                else None,
                                "entityFlags": compiler.number(args[1])
                                if source_row["opcode"] == macro
                                else 0,
                            }
                        )
                    else:
                        events.append(
                            {
                                "kind": "step",
                                **common,
                                "marker": 0x1400,
                                "x": None
                                if source_row["opcode"] == default
                                or compiler.number(args[0]) == 255
                                else compiler.number(args[0]),
                                "y": None
                                if source_row["opcode"] == default
                                or compiler.number(args[1]) == 255
                                else compiler.number(args[1]),
                            }
                        )
                    if source_row["opcode"] == default:
                        # The default terminates the table; following instructions are not rows.
                        break

            def block_copy(record):
                return {
                    "source": record["source"] if record["source"]["y"] < 128 else {"x": 0, "y": 0},
                    "destination": record["destination"],
                    **record["size"],
                }

            layout_events = {
                "doors": [
                    {"trigger": record["trigger"], "copy": block_copy(record)}
                    for record in resources["stepEventTables"][references["stepEventTable"]][
                        "records"
                    ]
                ],
                "flags": [
                    {"flag": record["flag"], "copy": block_copy(record)}
                    for record in resources["flagEventTables"][references["flagEventTable"]][
                        "records"
                    ]
                ],
                "roofs": [
                    {
                        "trigger": record["trigger"],
                        "copy": block_copy(record),
                        "clear": record["source"]["y"] >= 128,
                    }
                    for record in resources["roofEventTables"][references["roofEventTable"]][
                        "records"
                    ]
                ],
            }
        on_load = compiler.frontier(
            f"map-{selected}-setup", f"{route_id}:ordered setup/init/population"
        )
        flag_table = references["flagEventTable"]
        flag_records = resources["flagEventTables"][flag_table]["records"]
        if flag_records and selected not in selection.get("eventMaps", []):
            guarded_load = compiler.frontier(f"map-{selected}-flag-layout", flag_table)
            instructions = []
            for index, record in enumerate(flag_records):
                instructions.extend(
                    [
                        {
                            "op": "branch-flag",
                            "flag": record["flag"],
                            "whenSet": False,
                            "target": _location(guarded_load, len(instructions) + 2),
                        },
                        compiler.native("flag-layout-copy", f"{flag_table}[{index}]"),
                    ]
                )
            instructions.append({"op": "jump", "target": _location(on_load)})
            compiler.programs[guarded_load]["instructions"] = instructions
            on_load = guarded_load
        battle = battle_routes.get(selected)
        input_program = None
        if entities or resources["stepEventTables"][references["stepEventTable"]]["records"]:
            input_program = _location(
                compiler.frontier(
                    f"map-{selected}-input", f"{route_id}:input/event/autonomous-entity services"
                )
            )
        if selected in selection.get("eventMaps", []):
            input_program = None
            on_load = compiler.compile(setup["references"]["initFunction"])
        maps.append(
            {
                "id": "map-" + str(selected),
                "layout": [words[n : n + 64] for n in range(0, 4096, 64)],
                "areas": [
                    {
                        "minX": a["mainLayerStart"]["x"],
                        "minY": a["mainLayerStart"]["y"],
                        "maxX": a["mainLayerEnd"]["x"],
                        "maxY": a["mainLayerEnd"]["y"],
                        "view": {
                            "foregroundX": a["secondLayerForegroundStart"]["x"],
                            "foregroundY": a["secondLayerForegroundStart"]["y"],
                            "backgroundX": a["secondLayerBackgroundStart"]["x"],
                            "backgroundY": a["secondLayerBackgroundStart"]["y"],
                            **{
                                f"parallax{plane}{axis.upper()}": a[key][axis]
                                for plane, key in (
                                    ("A", "mainLayerParallax"),
                                    ("B", "secondLayerParallax"),
                                )
                                for axis in ("x", "y")
                            },
                            **{
                                f"autoscroll{plane}{axis.upper()}": a[key][axis]
                                for plane, key in (
                                    ("A", "mainLayerAutoscroll"),
                                    ("B", "secondLayerAutoscroll"),
                                )
                                for axis in ("x", "y")
                            },
                            "layer": a["mainLayerType"],
                        },
                        "overlay": {
                            axis: a["secondLayerForegroundStart"][axis]
                            - a["secondLayerBackgroundStart"][axis]
                            for axis in ("x", "y")
                        },
                    }
                    for a in areas
                ],
                "entities": entities,
                **(
                    {
                        "population": population,
                        **({"layoutEvents": layout_events} if layout_events is not None else {}),
                        "entryFlags": [
                            {"flag": flag, "value": False}
                            for flag in range(
                                compiler.equates["MAPSETUP_TEMP_FLAGS_START"],
                                compiler.equates["MAPSETUP_TEMP_FLAGS_START"]
                                + compiler.equates["MAPSETUP_TEMP_FLAGS_COUNTER"]
                                + 1,
                            )
                        ]
                        + [{"flag": 80, "value": True}],
                    }
                    if population
                    else {}
                ),
                "events": events,
                "onLoad": _location(on_load),
                "battle": battle,
                "input": input_program,
                "setup": None
                if route is None
                else {
                    "default": route["defaultSetup"].lower().replace("_", "-"),
                    "variants": [
                        {
                            "flag": variant["flag"],
                            "setup": variant["setup"].lower().replace("_", "-"),
                        }
                        for variant in route["flagVariants"]
                    ],
                },
            }
        )
    # Map setup can be selected only at its implemented boundary. Source Map40's guarded
    # init function is compiled below; other setups remain explicit reached frontiers.
    for selected in selection["maps"]:
        row = map_rows[selected]
        route = resources["setupRoutes"].get(row["references"]["setupRoute"])
        if route is None:
            compiler.programs[f"map-{selected}-setup"]["instructions"] = [{"op": "end"}]
            continue
        setup = resources["setupDefinitions"][route["defaultSetup"]]
        init = resources["initFunctions"][setup["references"]["initFunction"]]
        operations = init["operations"]
        # Only the existing source guard/script/return shape is supported, not an ID special case.
        if (
            len(operations) == 4
            and [op["opcode"] for op in operations] == ["chkFlg", "beq.s", "script", "rts"]
            and operations[1]["localBranchTargetIndex"] == 3
        ):
            program_id = f"map-{selected}-setup"
            body = [
                {
                    "op": "branch-flag",
                    "flag": compiler.number(operations[0]["operandText"]),
                    "whenSet": False,
                    "target": _location(program_id, 2),
                },
                {"op": "call", "target": _location(compiler.compile(operations[2]["operandText"]))},
                {"op": "end"},
            ]
            compiler.programs[program_id]["instructions"] = body
    texts_path = "disasm/data/scripting/text/gamescript.txt"
    names_path = "disasm/data/stats/allies/allynames.asm"
    compiler.sources.add(names_path)
    member_names = re.findall(
        r'allyName\s+"([^"]+)"', (upstream / names_path).read_text(encoding="utf-8")
    )
    if len(member_names) != compiler.equates["COMBATANT_ALLIES_NUMBER"]:
        raise ValueError("source ally name count")
    compiler.sources.add(texts_path)
    # Private text retains source tags; the adapter does not claim original tag/audio timing.
    texts = [
        {"id": int(line[:4], 16), "text": line[5:]}
        for line in (upstream / texts_path).read_text(encoding="utf-8").splitlines()
        if re.match(r"^[0-9A-Fa-f]{4}=.+", line)
    ]
    visuals = None
    if (rom_path is None) != (presentation_root is None):
        raise ValueError("visual preparation requires both registered ROM and presentation root")
    if rom_path is not None:
        visuals = prepare_visuals(
            compiler,
            canonical,
            maps,
            rom_path,
            presentation_root,
            selection["presentationManifestSha256"],
            selection["presentationAtlases"],
        )
    growth = _selected_growth(compiler, selection.get("growthClasses", []))
    font_path = "disasm/data/graphics/tech/fonts/variablewidthfont.bin"
    ascii_path = "disasm/data/scripting/text/asciitotextsymbolmap.asm"
    compiler.sources.update(
        (
            ascii_path,
            "disasm/data/graphics/tech/fonts/variablewidthfont.txt",
            "disasm/code/common/tech/incbins/s06_incbins_graphics.asm",
            "disasm/code/common/tech/pointers/s06_pointers.asm",
            "disasm/code/common/maps/camerafunctions.asm",
            "disasm/code/common/maps/animations.asm",
            "disasm/code/common/scripting/text/textfunctions_1.asm",
            "disasm/code/common/scripting/text/textfunctions_2.asm",
        )
    )
    # The split binary is private, not an upstream Git blob. Reuse the existing
    # source/ROM reader (including pointer and ASCII parity); emit only advances.
    font = build_variable_width_font_contract(
        rom_path or private_input_path(ROM_INPUT_IDENTITY), upstream
    )
    text_font = {
        "asciiToSymbol": _parse_ascii_map((upstream / ascii_path).read_text(encoding="utf-8")),
        "advances": [row["advancePixels"] for row in font["glyphs"]],
    }
    sources = [{"path": font_path, "sha256": font["font"]["sha256"]}]
    for relative in sorted(compiler.sources):
        raw = (upstream / relative).read_bytes()
        committed = subprocess.run(
            ["git", "show", f"{expected_commit}:{relative}"],
            cwd=upstream,
            capture_output=True,
            check=True,
        ).stdout
        if raw.replace(b"\r\n", b"\n") != committed.replace(b"\r\n", b"\n"):
            raise ValueError("selected pinned source has local changes: " + relative)
        sources.append({"path": relative, "sha256": hashlib.sha256(raw).hexdigest().upper()})
    result = {
        "formatVersion": 1,
        "profile": "private-local-controlled-start",
        "package": selection["package"],
        "provenance": {
            **canonical["upstream"],
            "romSha256": canonical["romSha256"],
            "sources": sources,
            "controlledBoundary": selection["controlledBoundary"],
        },
        "world": {
            "maps": maps,
            "programs": list(compiler.programs.values()),
            "texts": texts,
            "memberNames": member_names,
            "textFont": text_font,
            **({"growth": growth} if growth else {}),
            **({"presentation": visuals} if visuals else {}),
            **(
                {
                    "partyFlags": {
                        "memberCount": compiler.equates["COMBATANT_ALLIES_NUMBER"],
                        "joinedStart": compiler.equates["FORCEMEMBER_JOINED_FLAGS_START"],
                        "activeStart": compiler.equates["FORCEMEMBER_ACTIVE_FLAGS_START"],
                        "capacity": compiler.equates["FORCE_MAX_SIZE"],
                    }
                }
                if selection.get("eventMaps")
                else {}
            ),
        },
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return {"maps": len(maps), "programs": len(compiler.programs), "sources": len(sources)}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--canonical", required=True, type=Path)
    parser.add_argument("--upstream", required=True, type=Path)
    parser.add_argument("--selection", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--rom-path", type=Path)
    parser.add_argument("--presentation-root", type=Path)
    args = parser.parse_args()
    destination = args.output.resolve()
    repo = Path(__file__).resolve().parents[2]
    if not destination.is_relative_to(repo / "local"):
        raise SystemExit(
            "private content output must stay in this worktree's ignored local directory"
        )
    try:
        print(
            json.dumps(
                prepare(
                    args.canonical,
                    args.upstream,
                    args.selection,
                    destination,
                    args.rom_path,
                    args.presentation_root,
                )
            )
        )
    except (ValueError, KeyError, OSError, subprocess.CalledProcessError) as error:
        # Do not expose local private input paths through an exception traceback.
        raise SystemExit(
            "private exploration preparation failed: " + type(error).__name__
        ) from None


if __name__ == "__main__":
    main()
