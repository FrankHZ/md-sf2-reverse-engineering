"""Lower selected source story and entity operations into immutable runtime programs."""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _equates
from sf2tool.h2.battle_global_data import _arguments, _integer, _statements, _tokens
from sf2tool.remake_exploration_joins import join_service


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
        if symbol == "eas_Motionless":
            path = "disasm/data/scripting/entity/eas_battleneutralentities.asm"
            self.sources.add(path)
            text = (self.upstream / path).read_text(encoding="utf-8")
            if not re.search(
                r"eas_Motionless:\s*ac_motion OFF\s+word_44984:\s*ac_branch"
                r"\s+dc.w eas_Idle-word_44984",
                text,
            ):
                raise ValueError("motionless source action drift")
            result = self.action("ac_motion", ["OFF"], path + ":eas_Motionless")
            self.finish_in_idle(result)
            self.actions[symbol] = result
            return result
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
        if op == "ac_motion":
            return [{"op": "animation-counter", "counter": 1 if self.number(args[0]) else 255}]
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
        event_speaker = True
        narration = False
        self.programs[symbol]["entitiesRunning"] = not any(
            row["opcode"] in ("chkFlg", "txt", "script", "rts") for row in rows
        )
        while index < len(rows):
            row = rows[index]
            op, args = row["opcode"], _tokens(row["operandText"])
            source = f"{relative}:{symbol}[{row['index']}]"
            index += 1
            if op in ("csc_end", "rts") or (op == "dc.w" and args == ["$FFFF"]):
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
                pass
            elif op == "jsr" and args == ["GetEntityPortaitAndSpeechSfx"]:
                speaker = self.entity(str(registers["d0"]))
            elif op == "jsr" and args == ["DisplayCurrentPortrait"]:
                event_speaker = False
                narration = False
                result.append({"op": "speaker", "entity": speaker})
                result.append(
                    {"op": "open-portrait", "entity": speaker, "flags": 0}
                    if speaker is not None
                    else self.native(op, source)
                )
            elif op == "jsr" and args == ["j_ClosePortraitWindow"]:
                result.append({"op": "close-portrait"})
            elif (
                op == "clr.w"
                and args == ["((CURRENT_SPEECH_SFX-$1000000)).w"]
                and index >= 4
                and rows[index - 4]["opcode"] == "clsTxt"
                and rows[index - 3]["operandText"] == "j_ClosePortraitWindow"
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
            elif op == "executeSubroutine":
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
                            "useEventSpeaker": speaker is None and event_speaker,
                            **({"narration": True} if narration else {}),
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
            elif op == "newEntity" and 128 <= self.number(args[0]) < 160:
                self.sources.add("disasm/code/common/scripting/entity/entityfunctions_1.asm")
                result.append(
                    {
                        "op": "new-entity",
                        "entity": self.entity(args[0]),
                        "position": {"x": self.number(args[1]), "y": self.number(args[2])},
                        "facing": self.number(args[3]),
                        "sprite": self.number(args[4]),
                        "actions": self.action_stream("eas_Init"),
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
            elif op in ("resetMap", "reloadMap"):
                result.extend(
                    [
                        {
                            "op": "reload-scene-map",
                            "camera": {"x": self.number(args[0]), "y": self.number(args[1])}
                            if op == "reloadMap"
                            else {"x": 0, "y": 0},
                        },
                        {"op": "wait-ticks", "ticks": 1},
                    ]
                )
            elif op == "playSound":
                result.append(
                    {
                        "op": "present",
                        "kind": "SoundFade" if args[0] == "SOUND_COMMAND_FADE_OUT" else "Sound",
                        "resource": None if args[0] == "SOUND_COMMAND_FADE_OUT" else args[0],
                        "entity": None,
                        "position": None,
                    }
                )
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
                result.extend(join_service(self.number(args[0])))
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
            elif op == "loadMapFadeIn" and (
                self.scene_maps is None or self.number(args[0]) in self.scene_maps
            ):
                # csc37 starts out-to-black and falls through csc48. The source's later fadeInB
                # is a separate operation; loading a scene does not execute map init/population.
                # csc48 detaches before its first enabled fade wait. With the finite blocking
                # helper, express that service boundary explicitly before entering the fade.
                result.extend(
                    [
                        {"op": "camera-entity", "entity": None},
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
                        # csc48 reactivates installed services, waits once, then returns.
                        {"op": "wait-ticks", "ticks": 1},
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
            elif op == "animEntityFX" and args[1] in ("MOSAIC_IN", "MOSAIC_OUT", "SLIDE_IN"):
                result.append(
                    {
                        "op": "present",
                        "kind": "EntityEffect",
                        "resource": {
                            "MOSAIC_IN": "mosaic-in",
                            "MOSAIC_OUT": "mosaic-out",
                            "SLIDE_IN": "slide-in",
                        }[args[1]],
                        "entity": self.entity(args[0]),
                        "position": None,
                        **({"durationTicks": 46} if args[1] == "SLIDE_IN" else {}),
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
