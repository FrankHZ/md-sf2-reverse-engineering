"""Lower selected source story and entity operations into immutable runtime programs."""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool import remake_exploration_services as services
from sf2tool.h2.battle_ai import _equates
from sf2tool.h2.battle_global_data import _arguments, _integer, _statements, _tokens
from sf2tool.remake_exploration_actions import action_stream, inline_actions, walking_actions
from sf2tool.remake_exploration_joins import join_selector, join_service
from sf2tool.remake_exploration_scene import (
    portrait_operations,
    raft_placement,
    scene_operation,
    sound_operation,
)
from sf2tool.remake_exploration_view import counted_view_loop, view_operation


def _location(program: str, instruction: int = 0) -> dict[str, Any]:
    return {"program": program.lower().replace("_", "-"), "instruction": instruction}


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
                "disasm/code/common/menus/portraitfunctions.asm",
                "disasm/code/common/tech/interrupts/trap6_mapscript.asm",
                "disasm/code/common/tech/interrupts/trap9_contextualfunctions.asm",
                "disasm/code/common/tech/interrupts/vint.asm",
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
        match = re.search(rf"^{re.escape(symbol)}:[ \t]*", text, re.MULTILINE)
        if match is None:
            raise ValueError("selected source label is missing: " + symbol)
        result, inline = [], False
        for line in text[match.end() :].splitlines():
            if label := re.match(r"^([A-Za-z_]\w*):[ \t]*(.*)", line):
                if not inline:
                    break
                line = label[2]
            for statement in _statements(line):
                opcode, _, operands = statement.partition(" ")
                result.append(
                    {"index": len(result), "opcode": opcode, "operandText": operands.strip()}
                )
                if opcode in (
                    "entityActions",
                    "entityActionsWait",
                    "customActscript",
                    "customActscriptWait",
                ):
                    inline = True
                elif opcode in ("endActions", "ac_end"):
                    inline = False
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
        if len(rows) == 1 and rows[0]["opcode"] == "mainEntity":
            storage = "disasm/data/maps/mapsetupsstorage.asm"
            self.sources.add(storage)
            includes = re.findall(r'include\s+"([^"]+)"', (self.upstream / storage).read_text())
            paths = ["disasm/" + path.replace(chr(92), "/") for path in includes]
            tail = paths[paths.index(relative) + 1]
            first = re.search(r"^(\w+):", (self.upstream / tail).read_text(), re.MULTILINE)
            rows.extend(self.source_operations(tail, first[1]))
        if rows[0]["opcode"] != "mainEntity" or rows[-1]["opcode"] not in (
            "cscEntitiesEnd",
            "msEntitiesEnd",
        ):
            raise ValueError("source cutscene entity table boundary")
        x, y, facing = map(self.number, _tokens(rows[0]["operandText"]))
        entities = []
        non_ally = 128
        for row in rows[1:-1]:
            if row["opcode"] not in ("entity", "entityRandomWalk"):
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
                    "actions": (
                        walking_actions(self, *map(self.number, values[4:]))
                        if row["opcode"] == "entityRandomWalk"
                        else action_stream(self, values[4])
                    ),
                }
            )
        return {
            "op": "scene-entities",
            "position": {"x": x, "y": y},
            "facing": facing,
            "population": self.population(),
            "entities": entities,
        }

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
        rows, index = self.source_operations(relative, symbol), 0
        if loop := counted_view_loop(self, relative, symbol, rows):
            lowered, rows = loop
            result.extend(lowered)
        registers: dict[str, int] = {}
        speaker = None
        event_speaker = True
        narration = False
        saved_speaker = (speaker, event_speaker)
        self.programs[symbol]["entitiesRunning"] = not any(
            row["opcode"] in ("chkFlg", "txt", "script", "rts") for row in rows
        )
        while index < len(rows):
            row = rows[index]
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{relative}:{symbol}[{row['index']}]"
            index += 1
            if lowered_view := view_operation(self, op, args):
                result.append(lowered_view)
            elif op == "flashScreenWhite" and 24 <= self.number(args[0]) < 32:
                result.append(
                    {
                        "op": "present",
                        "kind": "FlashWhite",
                        "resource": "white-four-pulses",
                        "entity": None,
                        "position": None,
                        "durationTicks": self.number(args[0]),
                    }
                )
            elif op in ("csc_end", "rts") or (op == "dc.w" and args == ["$FFFF"]):
                result.append({"op": "end" if op == "rts" else "end-map-script"})
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
            elif (
                op == "cmpi.b"
                and len(args) == 2
                and args[0].startswith("#")
                and args[1] == "((EVENT_RELATIVE_POSITION-$1000000)).w"
                and index < len(rows)
                and rows[index]["opcode"] in ("beq.s", "bne.s")
            ):
                branch = rows[index]
                index += 1
                result.append(
                    {
                        "op": "branch-interaction-facing",
                        "facing": self.number(args[0][1:]),
                        "whenEqual": branch["opcode"] == "beq.s",
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
            elif (
                op == "lea"
                and len(args) == 2
                and args[1] == "a0"
                and (target := re.fullmatch(r"(\w+)\(pc\)", args[0]))
                and index < len(rows)
                and rows[index]["opcode"] == "jsr"
                and rows[index]["operandText"] == "ExecuteMapScript"
            ):
                result.append({"op": "call", "target": _location(self.compile(target[1]))})
                index += 1
            elif op == "script":
                result.append(
                    {
                        "op": "call",
                        "target": _location(self.compile(args[0])),
                        "activateEntities": True,
                    }
                )
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
                if args[0].startswith("((CURRENT_SPEECH_SFX"):
                    saved_speaker = (speaker, event_speaker)
            elif op == "move.w" and args == [
                "((SPEECH_SFX_COPY-$1000000)).w",
                "((CURRENT_SPEECH_SFX-$1000000)).w",
            ]:
                speaker, event_speaker = saved_speaker
                narration = False
            elif (
                op == "move.b"
                and len(args) == 2
                and args[0].startswith("#")
                and args[1] == "((CURRENT_SHOP_INDEX-$1000000)).w"
            ):
                registers["shop"] = self.number(args[0][1:])
            elif op in ("jsr", "jmp") and args == ["j_ShopMenu"] and "shop" in registers:
                result.append(services.shop_service(self, registers.pop("shop")))
                if op == "jmp":
                    result.append({"op": "end"})
            elif op in ("jsr", "jmp") and args == ["j_ChurchMenu"]:
                result.append(services.church_service(self))
                if op == "jmp":
                    result.append({"op": "end"})
            elif op == "jsr" and args == ["j_YesNoPrompt"]:
                # The direct prompt returns d0, unlike csc06 which writes F89.
                if rows[index]["opcode"] == "tst.w" and rows[index]["operandText"] == "d0":
                    index += 1
                if rows[index]["opcode"] in ("bne.s", "bne.w"):
                    result.append(
                        {
                            "op": "prompt-yes-no",
                            "noTarget": _location(self.compile(rows[index]["operandText"])),
                        }
                    )
                    index += 1
                    continue
                tail = rows[index : index + 3]
                if [r["opcode"] for r in tail] != ["clsTxt", "tst.w", "bne.s"] or tail[1][
                    "operandText"
                ] != "d0":
                    raise ValueError("direct Yes/No caller is not admitted")
                declined = symbol + "-declined"
                self.programs[declined] = {
                    "id": declined.lower().replace("_", "-"),
                    "source": source,
                    "entitiesRunning": False,
                    "instructions": [
                        {"op": "close-text"},
                        {"op": "jump", "target": _location(self.compile(tail[2]["operandText"]))},
                    ],
                }
                result.extend(
                    [{"op": "prompt-yes-no", "noTarget": _location(declined)}, {"op": "close-text"}]
                )
                index += 3
            elif op == "jsr" and args == ["j_NameAlly"]:
                self.sources.add("disasm/code/common/menus/nameallyscreen.asm")
                result.append({"op": "name-member", "member": registers["d0"]})
            elif op == "jsr" and args == ["GetEntityPortaitAndSpeechSfx"]:
                speaker = self.entity(str(registers["d0"]))
            elif op == "jsr" and args == ["DisplayCurrentPortrait"]:
                if narration and speaker is None:
                    speaker, event_speaker = saved_speaker
                narration = False
                result.extend(portrait_operations(self, speaker, event_speaker, source))
            elif op == "jsr" and args == ["j_ClosePortraitWindow"]:
                result.append({"op": "close-portrait"})
            elif (
                op == "clr.w"
                and args == ["((CURRENT_SPEECH_SFX-$1000000)).w"]
                and index >= 2
                and rows[index - 2]["operandText"]
                == "((CURRENT_SPEECH_SFX-$1000000)).w,((SPEECH_SFX_COPY-$1000000)).w"
            ):
                speaker = None
                event_speaker = False
                narration = True
                result.append({"op": "speaker", "entity": None})
            elif (
                op == "move.b"
                and len(args) == 2
                and args[1] == "((EGRESS_MAP-$1000000)).w"
                and args[0].startswith("#")
            ):
                result.append({"op": "egress-map", "map": "map-" + str(self.number(args[0][1:]))})
            elif op == "jsr" and args == ["CheckRandomBattle"]:
                self.sources.add("disasm/code/common/scripting/map/mapsetupsfunctions_1.asm")
                result.append(
                    {
                        "op": "source-random-battle",
                        "battle": registers["d0"],
                        "onRequest": _location("source-random-battle-presentation"),
                    }
                )
            elif op == "executeSubroutine" and args == ["j_csub_40F2"]:
                result.append({"op": "restore-cached-roof"})
            elif op == "reloadEntities" and re.fullmatch(r"ms_map\d+_Entities", args[0]):
                result.append(
                    {
                        "op": "reload-default-entities",
                        "map": "map-" + args[0].removeprefix("ms_map").removesuffix("_Entities"),
                    }
                )
            elif op == "joinForceAI":
                result.append(
                    {
                        "op": "join-ai-party" if self.number(args[1]) else "join-player-party",
                        "member": self.number(args[0]),
                    }
                )
            elif op == "reloadEntities" and (
                variant := re.fullmatch(r"ms_map(\d+)_flag(\d+)_Entities", args[0])
            ):
                result.append(
                    {
                        "op": "reload-setup-entities",
                        "map": f"map-{int(variant[1])}",
                        "setup": f"ms-map{int(variant[1])}-flag{int(variant[2])}",
                    }
                )
            elif op == "flashScreenWhite":
                ticks = ((self.number(args[0]) >> 3) + 1) * 8
                result.append(
                    {
                        "op": "present",
                        "kind": "FlashWhite",
                        "resource": "white-pulses",
                        "entity": None,
                        "position": None,
                        "durationTicks": ticks,
                    }
                )
            elif op == "executeSubroutine" and args == ["csub_458E"]:
                self.sources.update(
                    (
                        "disasm/code/common/scripting/map/bbcs_11_function.asm",
                        "disasm/code/gameflow/exploration/exploration.asm",
                    )
                )
                result.append({"op": "align-background-to-view-target"})
            elif op == "faceEntity":
                result.append(
                    {
                        "op": "face-entity",
                        "entity": self.entity(args[0]),
                        "target": self.entity(args[1]),
                    }
                )
            elif op == "move.l" and args == [
                "#MAP_EVENT_RELOADMAP",
                "((MAP_EVENT_TYPE-$1000000)).w",
            ]:
                result.append({"op": "request-current-map"})
            elif op == "executeSubroutine" and args[0] in self.raw:
                result.append({"op": "call", "target": _location(self.compile(args[0]))})
            elif op == "jsr" and args in (["j_GetMaxHp"], ["j_GetMaxMp"]):
                setter = "j_SetCurrentHp" if args == ["j_GetMaxHp"] else "j_SetCurrentMp"
                if (
                    index < len(rows)
                    and rows[index]["opcode"] == "jsr"
                    and rows[index]["operandText"] == setter
                ):
                    index += 1
                    result.append(
                        {
                            "op": "restore-member-resource",
                            "member": registers["d0"],
                            "hp": setter == "j_SetCurrentHp",
                        }
                    )
                else:
                    result.append(self.native(op, source))
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
                        "actions": walking_actions(
                            self, registers["d1"], registers["d2"], registers["d3"]
                        ),
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
                        services.raw_display_text(),
                    ]
                )
            elif (
                Path(relative).name.startswith("s4_descriptions")
                and (lowered := services.description_service(op, args, registers)) is not None
            ):
                result.extend(lowered)
                if op == "jmp":  # Tail DisplayText returns directly to the description dispatcher.
                    break
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
            elif op == "hideText":
                result.extend([{"op": "close-portrait"}, {"op": "close-text"}])
            elif op in ("startEntity", "stopEntity"):
                result.append(
                    {
                        "op": "entity-animation",
                        "entity": self.entity(args[0]),
                        "counter": 0 if op == "startEntity" else 255,
                    }
                )
            elif op == "move.b" and args[1:] == ["((RAFT_MAP-$1000000)).w"]:
                result.append(raft_placement(self, args, rows[index : index + 2]))
                index += 2
            elif op == "newEntity" and (
                128 <= self.number(args[0]) < 160
                or self.number(args[0]) < 30
                and args[4] == "NOTHING_BYTE"
            ):
                self.sources.add("disasm/code/common/scripting/entity/entityfunctions_1.asm")
                result.append(
                    {
                        "op": "new-entity",
                        "entity": self.entity(args[0]),
                        "position": {"x": self.number(args[1]), "y": self.number(args[2])},
                        "facing": self.number(args[3]),
                        "sprite": self.number(args[4]),
                        "actions": action_stream(self, "eas_Init"),
                    }
                )
            elif op == "moveNextToPlayer":
                entity, direction = self.entity(args[0]), self.number(args[1])
                result.extend(
                    [
                        {"op": "move-next-to-player", "entity": entity, "direction": direction},
                        {"op": "wait-motion", "entity": entity},
                        {
                            "op": "face",
                            "entity": entity,
                            "facing": (direction + 2) & 3,
                            "refreshSprite": True,
                        },
                        {"op": "wait-motion", "entity": "entity-0"},
                    ]
                )
            elif op == "setQuake" and 0 <= self.number(args[0]) <= 255:
                result.append({"op": "quake", "amount": self.number(args[0])})
            elif op == "loadEntitiesFromMapSetup":
                result.append(
                    {
                        "op": "setup-entities",
                        "position": {"x": self.number(args[0]), "y": self.number(args[1])},
                        "facing": self.number(args[2]),
                    }
                )
            elif op == "resetMap":
                result.extend([{"op": "reset-scene-map"}, {"op": "wait-ticks", "ticks": 1}])
            elif op == "reloadMap":
                result.extend(
                    [
                        {
                            "op": "reload-scene-map",
                            "camera": {"x": self.number(args[0]), "y": self.number(args[1])},
                        },
                        {"op": "wait-ticks", "ticks": 1},
                    ]
                )
            elif op == "playSound":
                result.append(sound_operation(args[0]))
            elif op == "warp" and (
                self.scene_maps is None or self.number(args[0]) in self.scene_maps
            ):
                result.append(
                    {
                        "op": "queue-warp",
                        "map": "map-" + str(self.number(args[0])),
                        "position": {"x": self.number(args[1]), "y": self.number(args[2])},
                        "facing": self.number(args[3]),
                    }
                )
            elif op == "entityFlashWhite":
                result.append(
                    {
                        "op": "present",
                        "kind": "EntityEffect",
                        "resource": "flash-white",
                        "entity": self.entity(args[0]),
                        "position": None,
                        "durationTicks": (self.number(args[1]) // 4 + 1) * 4,
                    }
                )
            elif op in ("entityShakeHead", "headshake"):
                result.append(
                    {
                        "op": "present",
                        "kind": "Gesture",
                        "resource": "shake-head",
                        "entity": self.entity(args[0]),
                        "position": None,
                        "durationTicks": 42,
                    }
                )
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
                result.extend(join_service(join_selector(args[0], self.number)))
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
                        "entity": None
                        if self.number(args[0]) in (-1, 0xFFFF)
                        else self.entity(args[0]),
                    }
                )
            elif op in ("loadMapFadeIn", "mapLoad") and (
                self.scene_maps is None or self.number(args[0]) in self.scene_maps
            ):
                # csc37 starts out-to-black and falls through csc48. The source's later fadeInB
                # is a separate operation; loading a scene does not execute map init/population.
                # csc48 detaches before its first enabled fade wait. With the finite blocking
                # helper, express that service boundary explicitly before entering the fade.
                result.append({"op": "camera-entity", "entity": None})
                if op == "loadMapFadeIn":
                    result.append(
                        {
                            "op": "present",
                            "kind": "FadeOut",
                            "resource": "black",
                            "entity": None,
                            "position": None,
                        }
                    )
                result.extend(
                    [
                        {
                            "op": "scene-map",
                            "map": "map-" + str(self.number(args[0])),
                            "camera": {"x": self.number(args[1]), "y": self.number(args[2])},
                        },
                        # csc48 reactivates installed services, waits once, then returns.
                        {"op": "wait-ticks", "ticks": 1},
                    ]
                )
            elif op == "loadMapEntities":
                table_path = (
                    "disasm/" + self.raw[args[0]]["path"] if args[0] in self.raw else relative
                )
                result.append(self.scene_entities(table_path, args[0]))
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
            elif op == "animEntityFX" and args[1] in (
                "MOSAIC_IN",
                "MOSAIC_OUT",
                "SLIDE_IN",
                "SLIDE_OUT",
            ):
                result.append(
                    {
                        "op": "present",
                        "kind": "EntityEffect",
                        "resource": {
                            "MOSAIC_IN": "mosaic-in",
                            "MOSAIC_OUT": "mosaic-out",
                            "SLIDE_IN": "slide-in",
                            "SLIDE_OUT": "slide-out",
                        }[args[1]],
                        "entity": self.entity(args[0]),
                        "position": None,
                        **({"durationTicks": 46} if args[1] in ("SLIDE_IN", "SLIDE_OUT") else {}),
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
            elif op == "removeShadow":
                result.append({"op": "remove-shadow", "entity": self.entity(args[0])})
            elif op == "closeTxt":
                result.extend([{"op": "close-portrait"}, {"op": "close-text"}])
            elif op == "clsTxt":
                result.append({"op": "close-text"})
            elif op == "yesNo":
                result.append({"op": "yes-no", "flag": 89})
            elif op == "cameraSpeed":
                result.append({"op": "camera-speed", "speed": self.number(args[0])})
            elif op == "waitIdle":
                result.append({"op": "wait-idle", "entity": self.entity(args[0])})
            elif op == "addNewFollower":
                result.append({"op": "add-follower", "entity": self.entity(args[0])})
            elif op == "csWait":
                result.append({"op": "wait-ticks", "ticks": self.number(args[0])})
            elif op == "setSprite":
                result.extend(scene_operation(self, op, args))
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
            elif op in ("setPos", "setPosFlash", "roofEvent", "stepEvent", "setDest", "setBlocks"):
                result.extend(scene_operation(self, op, args))
            elif op in (
                "entityActions",
                "entityActionsWait",
                "customActscript",
                "customActscriptWait",
            ):
                index, motion = inline_actions(self, rows, index, op, args, relative, symbol)
                result.append(motion)
            elif op in ("setActscript", "setActscriptWait"):
                actions = action_stream(self, args[1])
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
        if not result or result[-1]["op"] not in (
            "end",
            "end-map-script",
            "return",
            "jump",
            "native-call",
        ):
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
