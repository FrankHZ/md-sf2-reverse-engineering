"""Read-only H4 bindings for the accepted prepared-68..86 original winning chain.

Run as a module; outputs are private, fresh, and beneath this worktree's local/.
No native process, savestate load, acquisition reconciliation, or remake import.
"""

from __future__ import annotations

import argparse
import json
from collections import Counter
from hashlib import sha256
from pathlib import Path
from types import FunctionType

from sf2tool.h3.map3_messenger_acceptance import SEGMENT_IDENTITIES, _read_segment
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path

SOURCE = "9c3ea03ac5f5b467ee744f1ac624870da2408443"
UPSTREAM = "c834c652b6862bc5679fd7f69a38a7093206efc6"
ROM = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
OBSERVER = "8349604F919EB7071897CF9921ED51F951F5875A745B51289257B3F7CBB7D75B"
RUNNER = "97D424459D190B843598642F078BBF0CEDE2C7787F8FC0EB5492255E263D1339"
ACTIONS = {0: "Attack", 1: "CastSpell", 2: "UseItem", 3: "Stay"}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(path):
    return sha256(path.read_bytes()).hexdigest().upper()


def rows(path):
    with path.open(encoding="utf-8") as stream:
        for line, text in enumerate(stream, 1):
            yield line, json.loads(text)


def location(segment, line, row, filename="checkpoints.jsonl"):
    return {
        "record": f"prepared-{segment}/runtime/{filename}:{line}",
        "order": row["order"],
        "frame": row["frame"],
        "kind": row["kind"],
    }


def spell(value):
    return {
        "raw": value,
        "identity": value & 63,
        "rank": None if (value & 63) == 63 else (value >> 6) + 1,
        "empty": (value & 63) == 63,
    }


def combatant(row):
    result = dict(row)
    result["items"] = [
        {
            "slot": slot,
            "raw": value,
            "identity": value & 127,
            "empty": (value & 127) == 127,
            "equipped": bool(value & 128),
            "otherBits": value & ~255,
        }
        for slot, value in enumerate(row["items"])
    ]
    result["spells"] = [spell(value) for value in row["spells"]]
    return result


def accounting(facts):
    result = {
        k: facts[k]
        for k in (
            "party",
            "gold",
            "regionFlags",
            "turnOrder",
            "turnOffset",
            "rngBytes",
            "rngCopyByte",
            "rawTime",
        )
    }
    result.update(
        {k: [i for i, value in enumerate(facts[k]) if value] for k in ("joined", "active")}
    )
    result.update({k: [combatant(row) for row in facts[k]] for k in ("allies", "combatants")})
    return result


def state(row):
    return {
        k: row["state"][k]
        for k in (
            "map",
            "battle",
            "x",
            "y",
            "facing",
            "flags",
            "rngBytes",
            "rngCopyByte",
            "rawTime",
            "pendingReturns",
            "activeConsumers",
            "battleReturns",
            "mapEventWord",
            "typewriting",
            "portrait",
        )
    }


def binding(row, loc):
    f = row["facts"]
    result = {"source": loc, "state": state(row)}
    if row["kind"] == "stop:controllable-5b":
        f = f["accounting"]  # The terminal embeds battle_observation(), then accounting().
    a = f.get("accounting", f)
    if "allies" in a and "party" in a:
        result["accounting"] = accounting(a)
    if "records" in f:
        result["experience"] = {str(r["id"]): r["exp"] for r in f["records"]}
    return result


def coverage():
    # These are field groups of the existing ten-layer contract, not a public schema.
    groups = [
        (
            1,
            "map/position/facing/flags/party/gold/stats/status/items/spells/seed/copy",
            "decoded",
            "admission",
            "map3-controlled-admission.md",
        ),
        (
            1,
            "live NPC positions/facing/continuation phase",
            "decoded",
            "inherited.entities + entityIndexBytes",
            "map-exploration.md",
        ),
        (
            1,
            "all 1024 flags, base stats/prowess/EXP beyond serialized R1 fields",
            "missing-original-detail",
            "selected R1 emits only named flags and accounting fields",
            "map3-controlled-admission.md",
        ),
        (
            1,
            "normalized clock and timing-to-RNG mapping",
            "missing-semantic-binding",
            "admission.rawTime + requests + rng",
            "randomness.md",
        ),
        (
            2,
            "successful physical requests and consumer-committed player actions/choices",
            "decoded",
            "requests/decisions",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            2,
            "field directional consumers/destination/warp",
            "decoded",
            "route/movements",
            "map-exploration.md",
        ),
        (
            2,
            "cancel/reselect",
            "missing-original-branch",
            "no invented cancellation",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            3,
            "reached program/operation entry-return and text/speaker/choice",
            "decoded",
            "story/choices",
            "dialogue-system.md",
        ),
        (
            3,
            "entity mutation details and source operands",
            "retained-not-decoded",
            "story provenance/facts",
            "standalone-map-script-program-data.md",
        ),
        (
            4,
            "battle/load/start/activation/regions/spawn/order/first actor/state",
            "decoded",
            "encounter/turns",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            4,
            "full first-control readiness predicates",
            "retained-not-decoded",
            "movement poll + prepared-72 continuation observer",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            5,
            "actor/action/destination/resource/slot/effect-target/HP/MP/status/items/EXP/gold",
            "decoded",
            "decisions/scenes/checkpoints",
            "battle-action-construction.md",
        ),
        (
            5,
            "turn scores/order and AI before/after",
            "decoded",
            "turns/checkpoints",
            "battle-ai-decision.md",
        ),
        (
            5,
            "AI internal choice/memory",
            "retained-not-decoded",
            "StartAiControl/ExecuteAiControl full combatant records; source decoding still needed",
            "randomness.md",
        ),
        (
            5,
            "thinking-helper draw stream",
            "missing-original-detail",
            "only main/debug-aware generator callbacks are recorded",
            "randomness.md",
        ),
        (
            5,
            "main RNG before/after/range/result and enclosing source consumer",
            "decoded",
            "rng",
            "randomness.md",
        ),
        (
            5,
            "every draw-to-specific-effect and timing dependence",
            "missing-semantic-binding",
            "rng consumer scope is not a proved individual effect",
            "randomness.md",
        ),
        (
            5,
            "follow-up attack kind",
            "decoded",
            "scenes.effects.attackType",
            "battle-action-construction.md",
        ),
        (
            6,
            "victory/after-program/tail/flags/loop/return/setup",
            "decoded",
            "victory/story",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            7,
            "neutral settled endpoint and party state",
            "decoded",
            "endpoint + terminal readback",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            7,
            "next nonneutral input and effect (RA-12)",
            "missing-original-branch",
            "not in accepted #504",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            8,
            "6A no save/restart + 7C reached resource bindings",
            "missing-remake-observation",
            "resource inventory/import/playback separate",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            9,
            "dialogue IDs and source command/ack seams",
            "decoded",
            "story; DisplayText shim remains explicit",
            "dialogue-system.md",
        ),
        (
            9,
            "unshimmed dialogue reveal/ack completion",
            "missing-original-detail",
            "DisplayText RTS cannot prove natural completion",
            "text-and-font-system.md",
        ),
        (
            9,
            "audio dispatch/mailbox and scene initialize/execute/end",
            "decoded",
            "audio/scenes",
            "battle-scene-presentation.md",
        ),
        (
            9,
            "scene command/resource/animation/consumer completion details",
            "missing-original-detail",
            "static structure exists; natural per-command trace absent",
            "battle-scene-presentation.md",
        ),
        (
            9,
            "audio wait/replacement/stop semantic rules",
            "static-supported",
            "not audible delivery",
            "music-wait-service.md",
        ),
        (
            9,
            "actual host resource use/completion/token/error correlation",
            "missing-remake-observation",
            "no host run here",
            "map3-battle01-continuous-scenario.md",
        ),
        (
            10,
            "1A/2A/4A/6A/9A/10A named results and accessibility variants",
            "missing-remake-observation",
            "accepted definitions; execution pending",
            "map3-battle01-continuous-scenario.md",
        ),
    ]
    return [
        {
            "layer": layer,
            "fields": fields,
            "referenceStatus": status,
            "binding": path,
            "owner": f"docs/design/contracts/{owner}",
            "comparisonResult": "Unavailable",
            "actual": "no remake observation supplied",
        }
        for layer, fields, status, path, owner in groups
    ]


def project(evidence: Path):
    evidence = evidence.resolve(strict=True)
    require(
        evidence.parent.name == "local" and evidence.name == "issue496",
        "select the retained local/issue496 evidence root",
    )

    # Reuse the exact pair validator with a private globals copy. Its only repo_path
    # access is the input containment check. Never mutate the acquisition module or
    # redirect its other helpers (especially resume accounting / sealing).
    def evidence_path(relative):
        require(relative == "local", "unexpected pair-reader path request")
        return evidence.parent

    reader = FunctionType(
        _read_segment.__code__, {**_read_segment.__globals__, "repo_path": evidence_path}
    )
    reader.__kwdefaults__ = _read_segment.__kwdefaults__
    out = {
        k: []
        for k in (
            "lineage",
            "requests",
            "decisions",
            "choices",
            "menus",
            "movements",
            "route",
            "story",
            "encounter",
            "turns",
            "checkpoints",
            "scenes",
            "rng",
            "audio",
            "victory",
        )
    }
    out.update(
        sourceCommit=SOURCE,
        upstream=UPSTREAM,
        romSha256=ROM,
        coverage=coverage(),
        comparisonResult="Unavailable",
        reason="reference projection only; no actual H4 run",
    )
    previous = None
    previous_report = None
    previous_pair = None
    previous_meta = None
    event_order = 0
    scene = None
    scopes = []
    rng_calls = []
    last_request = None
    counts = Counter()
    for number in range(68, 87):
        directory = evidence / f"prepared-{number}"
        pair, meta = reader(directory, require_resumable=number != 86)
        require(pair["resumable"] == (number != 86), "terminal/resumable role mismatch")
        report = load_json(directory / "candidate.json")
        require(
            (
                report["RomSha256"],
                report["SourceCommit"],
                report["ObserverSha256"],
                report["RunnerSha256"],
            )
            == (ROM, UPSTREAM, OBSERVER, RUNNER),
            "not the accepted #504 acquisition source",
        )
        for filename, key in (
            ("config.json", "ConfigurationSha256"),
            ("input.json", "InputSha256"),
        ):
            require(digest(directory / filename) == report[key], f"{filename} identity mismatch")
        config = load_json(directory / "config.json")
        selection = report["Segment"]
        if previous is None:
            require(selection["parentDirectory"] is None, "first segment must be fresh R1")
        else:
            require(
                Path(selection["parentDirectory"]).resolve() == previous,
                "parent is not the immediately preceding winning segment",
            )
            require(
                selection["parentPairSha256"] == digest(previous / "runtime/segment-pair.json"),
                "parent pair identity mismatch",
            )
            require(
                all(report[k] == previous_report[k] for k in SEGMENT_IDENTITIES),
                "source/ROM/observer identity changed within winning chain",
            )
            require(
                selection["priorFrames"] == previous_pair["deliveredFrames"]
                and selection["priorActiveSeconds"] == previous_pair["activeSeconds"]
                and selection["priorBatches"] == previous_meta["batches"],
                "parent cumulative accounting mismatch",
            )
            require(
                pair["ordinal"] == previous_pair["ordinal"] + 1
                or pair["ordinal"] == previous_pair["ordinal"] == 1,
                "segment ordinal mismatch",
            )
            require(
                pair["historicalStarts"] == previous_pair["historicalStarts"] + 1,
                "unexpected intervening invocation",
            )
        out["lineage"].append(
            {
                "segment": number,
                "resumable": pair["resumable"],
                "pairSha256": digest(directory / "runtime/segment-pair.json"),
                "lastOrder": pair["lastOrder"],
                "frame": meta["observer"]["frame"],
            }
        )
        requests = {}
        for line, r in rows(directory / "runtime/actual-inputs.jsonl"):
            if r["kind"] == "command":
                require(r["id"] not in requests, "duplicate request id")
                requests[r["id"]] = {
                    "source": location(number, line, r, "actual-inputs.jsonl"),
                    "physicalFields": r["fields"],
                    "appliedFrames": 0,
                }
            elif r["kind"] == "frame" and not r["bootstrap"]:
                requests[r["id"]]["appliedFrames"] += 1
            elif r["kind"] == "result":
                require(r["ok"] and not r["error"], "failed request in selected winning chain")
                q = requests.pop(r["id"])
                require(q["appliedFrames"] == r["result"]["advanced"], "request delivery mismatch")
                require(q["physicalFields"][1] in ("step", "save"), "unsupported request kind")
                q["role"] = (
                    "controller-schedule"
                    if q["physicalFields"][1] == "step"
                    else "acquisition-save"
                )
                q.update(
                    resultSource=location(number, line, r, "actual-inputs.jsonl"),
                    resultState=r["result"]["state"]["state"],
                    ordinal=len(out["requests"]) + 1,
                )
                out["requests"].append(q)
        require(not requests, "uncompleted original request")
        segment_requests = [
            q for q in out["requests"] if q["source"]["record"].startswith(f"prepared-{number}/")
        ]
        request_index = 0
        for line, r in rows(directory / "runtime/checkpoints.jsonl"):
            require(r["order"] > event_order, "checkpoint order is not strictly monotonic")
            event_order = r["order"]
            k, f = r["kind"], r["facts"]
            counts[k] += 1
            loc = location(number, line, r)
            while (
                request_index < len(segment_requests)
                and segment_requests[request_index]["source"]["order"] < r["order"]
            ):
                last_request = segment_requests[request_index]
                request_index += 1
            loc["inputOrdinal"] = last_request["ordinal"] if last_request else None
            b = None
            if (
                k.startswith("natural:")
                or k
                in (
                    "battle:turn-dispatch",
                    "battle:player-return",
                    "battle:WriteBattlesceneScript:before",
                    "battle:EndBattlescene:after",
                    "stop:controllable-5b",
                )
                or (k.startswith("battle:") and k.endswith(":after"))
            ):
                b = binding(r, loc)
            if k == "segment:loaded-before-input":
                require(
                    previous_meta is not None
                    and r["frame"] == previous_meta["observer"]["frame"]
                    and f["parent"] == previous_pair["ordinal"],
                    "loaded parent frame/ordinal mismatch",
                )
            if k == "natural:r1":
                out["admission"] = b
            if k == "r1:inherited-status-and-live-entities":
                entities = []
                for entity in f["entities"]:
                    raw = bytes(entity["bytes"])
                    decoded = {"physical": entity["physical"]}
                    for name, offset, width in (
                        ("x", "X", 2),
                        ("y", "Y", 2),
                        ("destinationX", "XDEST", 2),
                        ("destinationY", "YDEST", 2),
                        ("facing", "FACING", 1),
                        ("layer", "LAYER", 1),
                        ("actionScript", "ACTSCRIPTADDR", 4),
                        ("waitTimer", "ACTSCRIPTWAITTIMER", 1),
                    ):
                        start = config["ram"]["ENTITYDEF_OFFSET_" + offset]
                        decoded[name] = int.from_bytes(raw[start : start + width], "big")
                    entities.append(decoded)
                out["inherited"] = {
                    "source": loc,
                    "entities": entities,
                    "entityIndexBytes": f["entityIndexBytes"],
                    "statusWords": f["allies"],
                }
            if k.startswith("natural:"):
                out["encounter" if r["state"]["battle"] != 255 else "route"].append(b)
            if k == "input:original-movement-acceptance":
                out["movements"].append(
                    {
                        "source": loc,
                        "state": state(r),
                        "delta": f,
                        "direction": {1: "Up", 2: "Down", 4: "Left", 8: "Right"}.get(
                            r["state"]["input"] & 15, "Unavailable: non-cardinal poll"
                        ),
                        "boundary": "accepted displacement; not tile arrival",
                    }
                )
            if k.startswith("battle-") and k.endswith(":return"):
                out["menus"].append(
                    {
                        "source": loc,
                        "returnValue": f["d0"] & 65535,
                        "cancelled": (f["d0"] & 65535) == 65535,
                    }
                )
            if k == "prompt:return":
                out["choices"].append(
                    {
                        "source": loc,
                        "choice": f["d0"] & 65535,
                        "meaning": "Yes" if f["d0"] & 65535 == 0 else "No",
                    }
                )
            if k.startswith(("script:", "operation:", "text:", "warp:", "setup:", "init:")):
                out["story"].append({"source": loc, "state": state(r), "facts": f})
            if k == "battle:turn-dispatch":
                out["turns"].append(
                    {**b, "actor": f["actor"], "round": f["round"], "turn": f["turn"]}
                )
            if k == "battle:player-return":
                require(f["action"] in ACTIONS, "unsupported committed player action")
                cancelled = (f["registers"]["d0"] & 65535) == 65535
                decision = {
                    "source": loc,
                    "actor": f["actor"],
                    "round": f["round"],
                    "turn": f["turn"],
                    "action": ACTIONS[f["action"]],
                    "cancelled": cancelled,
                    "destination": {"x": f["chosenX"], "y": f["chosenY"]},
                    "targetCandidates": f["targets"],
                    "effects": [],
                    "effectTargetStatus": "not applicable" if f["action"] == 3 else "pending scene",
                }
                if f["action"] == 1:
                    decision["spell"] = spell(f["itemOrSpell"])
                if f["action"] == 2:
                    actor = next(a for a in f["accounting"]["allies"] if a["id"] == f["actor"])
                    slot = f["itemSlot"]
                    require(0 <= slot < 4, "invalid committed item slot")
                    decision["item"] = combatant(actor)["items"][slot]
                    require(
                        decision["item"]["identity"] == (f["itemOrSpell"] & 127),
                        "item slot/action identity mismatch",
                    )
                out["decisions"].append(decision)
            if k == "battle:WriteBattlesceneScript:before":
                require(scene is None, "overlapping scene construction")
                scene = {
                    "occurrence": len(out["scenes"]) + 1,
                    "before": b,
                    "effects": [],
                    "actor": f["actor"],
                    "turn": f["turn"],
                    "boundaries": [],
                }
            if k in (
                "battle:battlesceneScript_ApplyActionEffect:before",
                "battle:InitializeBattlescene:before",
                "battle:InitializeBattlescene:after",
                "battle:ExecuteBattlesceneScript:before",
                "battle:ExecuteBattlesceneScript:after",
            ):
                require(scene is not None, "scene consumer without construction")
                scene["boundaries"].append(loc)
                if k.endswith("ApplyActionEffect:before"):
                    # Construction has now resolved TARGETS_LIST. The pre-construction list
                    # on player-return is a candidate list, not the chosen effect targets.
                    require(len(f["targets"]) == 1, "multi-target effect decoding unavailable")
                    effect = {
                        "source": loc,
                        "actor": f["sceneActor"],
                        "targets": f["targets"],
                        "action": ACTIONS.get(f["action"], "unsupported"),
                        "attackType": f["attackType"],
                    }
                    scene["effects"].append(effect)
            if k == "battle:EndBattlescene:after":
                require(scene is not None, "scene end without construction")
                scene["after"] = b
                out["scenes"].append(scene)
                decisions = [d for d in out["decisions"] if d["turn"] == scene["turn"]]
                if decisions:
                    require(len(decisions) == 1, "ambiguous player scene join")
                    decisions[0]["effects"] = scene["effects"]
                    decisions[0]["effectTargetStatus"] = "decoded at action-effect entry"
                scene = None
            if k.startswith("battle:") and k.endswith(":after"):
                out["checkpoints"].append(b)
            # Track actual nested original calls, including wrappers. Scope is causal
            # provenance; it is deliberately not a guessed per-draw gameplay effect.
            if k.startswith(("battle:", "rng:")) and k.endswith(":entry") and "returnPc" in f:
                scopes.append({"name": k[:-6], "source": loc, "returnPc": f["returnPc"]})
            if k.startswith("rng:") and k.endswith(":return"):
                rng_calls.append({"name": k[4:-7], "returnPc": f["returnPc"], "source": loc})
            if (
                k.startswith(("battle:", "rng:"))
                and k.endswith(":return")
                and "returnPc" in f
                and scopes
                and scopes[-1]["name"] == k[:-7]
            ):
                scopes.pop()
            if k == "rng:draw":
                require(
                    rng_calls and rng_calls[-1]["name"] == f["source"], "RNG return/draw mismatch"
                )
                call = rng_calls.pop()
                draw = {
                    "source": loc,
                    "generator": f["source"],
                    "before": f["before"]["seed"],
                    "after": f["seed"],
                    "range": f["before"][
                        "d0" if f["source"] == "GenerateRandomOrDebugNumber" else "d6"
                    ]
                    & 65535,
                    "value": f["d0" if f["source"] == "GenerateRandomOrDebugNumber" else "d7"]
                    & 65535,
                    "caller": call,
                    "consumerScopes": list(scopes),
                    "specificEffect": "Unavailable",
                }
                if f["source"] == "GenerateRandomNumber":
                    before_seed = int.from_bytes(bytes(draw["before"][:2]), "big")
                    after_seed = int.from_bytes(bytes(draw["after"][:2]), "big")
                    require(
                        after_seed == (before_seed * 13 + 7) & 65535,
                        "observed main RNG transition violates accepted source",
                    )
                    require(
                        draw["value"] == (after_seed * draw["range"]) >> 16,
                        "observed main RNG range/value violates accepted source",
                    )
                    draw["role"] = "base-generator-advance"
                else:
                    nested = out["rng"][-1]
                    require(
                        nested["generator"] == "GenerateRandomNumber"
                        and nested["before"] == draw["before"]
                        and nested["after"] == draw["after"]
                        and nested["value"] == draw["value"]
                        and nested["range"] == draw["range"],
                        "unmapped debug override/wrapper draw",
                    )
                    draw["role"] = "wrapper-result-not-additional-draw"
                    draw["baseDrawSource"] = nested["source"]
                out["rng"].append(draw)
            if k in ("audio:consumer-dispatch", "audio:mailbox-written"):
                out["audio"].append({"source": loc, "facts": f})
            if k.startswith("victory:") or k in (
                "battle:natural-victory",
                "battle:after-tail",
                "battle:loop-return",
                "battle:ExecuteAfterBattleCutscene:after",
            ):
                out["victory"].append({"source": loc, "state": state(r), "facts": f})
            if k == "stop:controllable-5b":
                out["endpoint"] = {**b, "readiness": f}
        previous, previous_pair, previous_report = directory, pair, report
        previous_meta = meta
    require(scene is None and not rng_calls, "unfinished scene/RNG return")
    require(counts["segment:loaded-before-input"] == 18, "missing parent load witnesses")
    require(counts["stop:controllable-5b"] == 1, "missing/duplicate terminal")
    require(
        out["endpoint"]["state"]["map"] == 57 and out["endpoint"]["state"]["battle"] == 255,
        "not the accepted terminal boundary",
    )
    require(
        all(d["effectTargetStatus"] != "pending scene" for d in out["decisions"]),
        "unbound committed action effect",
    )
    require(
        all(not menu["cancelled"] for menu in out["menus"]),
        "new cancellation branch requires source-specific decoding",
    )
    for ally in out["inherited"]["statusWords"]:
        require(
            ally["statusWord"]
            == out["admission"]["accounting"]["allies"][ally["id"]]["statusEffects"],
            "R1 inherited status/accounting mismatch",
        )
    terminal_raw = bytes.fromhex(meta["original"]["ramHex"])
    ram = config["ram"]

    def read(address, width=1):
        start = address & 65535
        return int.from_bytes(terminal_raw[start : start + width], "big")

    fields = {
        "level": ("LEVEL", 1),
        "hpCurrent": ("HP_CURRENT", 2),
        "hpMax": ("HP_MAX", 2),
        "mpCurrent": ("MP_CURRENT", 1),
        "mpMax": ("MP_MAX", 1),
        "statusEffects": ("STATUSEFFECTS", 2),
    }
    for ally in out["endpoint"]["accounting"]["allies"]:
        base = ram["COMBATANT_DATA"] + ally["id"] * ram["COMBATANT_DATA_ENTRY_SIZE"]
        for name, (offset, width) in fields.items():
            require(
                ally[name] == read(base + ram["COMBATANT_OFFSET_" + offset], width),
                "terminal saved RAM/accounting mismatch",
            )
        require(
            [item["raw"] for item in ally["items"]]
            == [read(base + ram["COMBATANT_OFFSET_ITEM_0"] + 2 * slot, 2) for slot in range(4)],
            "terminal saved RAM/item slots mismatch",
        )
    require(
        read(ram["CURRENT_MAP"]) == out["endpoint"]["state"]["map"]
        and read(ram["CURRENT_BATTLE"]) == out["endpoint"]["state"]["battle"],
        "terminal saved RAM/map/battle mismatch",
    )
    require(
        [read(ram["GAME_FLAGS"] + i) for i in range(128)] == out["endpoint"]["readiness"]["flags"],
        "terminal saved RAM/full flags mismatch",
    )
    pending_operations = []
    operation_pairs = []
    for entry in out["story"]:
        kind = entry["source"]["kind"]
        if kind == "operation:entry":
            pending_operations.append(entry)
        elif kind == "operation:return":
            require(bool(pending_operations), "operation return without entry")
            start = pending_operations.pop()
            require(
                start["facts"]["program"] == entry["facts"]["program"]
                and start["facts"]["operation"] == entry["facts"]["operation"],
                "operation entry/return mismatch",
            )
            operation_pairs.append(
                {
                    "entry": start["source"],
                    "return": entry["source"],
                    "operation": entry["facts"]["operation"],
                    "program": entry["facts"]["program"],
                }
            )
    require(not pending_operations, "unreturned operation")
    out["operationPairs"] = operation_pairs
    audio_pairs = []
    pending_audio = None
    for entry in out["audio"]:
        if entry["source"]["kind"] == "audio:consumer-dispatch":
            require(pending_audio is None, "overlapping audio dispatch")
            pending_audio = entry
        else:
            require(
                pending_audio is not None
                and pending_audio["facts"]["command"] == entry["facts"]["command"]
                and pending_audio["facts"]["sourcePc"] == entry["facts"]["pc"],
                "audio dispatch/mailbox mismatch",
            )
            audio_pairs.append(
                {
                    "dispatch": pending_audio["source"],
                    "mailbox": entry["source"],
                    "command": entry["facts"]["command"],
                }
            )
            pending_audio = None
    require(pending_audio is None, "uncompleted audio mailbox write")
    out["audioPairs"] = audio_pairs
    out["logicalInputs"] = sorted(
        [
            {"kind": kind, "index": i, "source": row["source"]}
            for kind in ("movements", "choices", "menus", "decisions")
            for i, row in enumerate(out[kind])
        ],
        key=lambda row: row["source"]["order"],
    )
    out["counts"] = dict(counts)
    out["summary"] = {
        "segments": len(out["lineage"]),
        "successfulRequests": len(out["requests"]),
        "playerDecisions": len(out["decisions"]),
        "scenes": len(out["scenes"]),
        "rngRecords": len(out["rng"]),
        "baseDraws": counts["rng:GenerateRandomNumber:return"],
        "wrapperResults": counts["rng:GenerateRandomOrDebugNumber:return"],
        "controllerRequests": sum(q["role"] == "controller-schedule" for q in out["requests"]),
        "comparisonResult": "Unavailable",
    }
    return out


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--evidence-root", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()
    require(
        output.is_relative_to(repo_path("local")) and not output.exists(),
        "output must be fresh beneath this worktree's ignored local/",
    )
    require(
        not output.is_relative_to(args.evidence_root.resolve()), "output cannot overwrite evidence"
    )
    result = project(args.evidence_root)
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("x", encoding="utf-8") as stream:
        json.dump(result, stream, ensure_ascii=False, separators=(",", ":"))
        stream.write("\n")
    print(json.dumps(result["summary"]))


if __name__ == "__main__":
    main()
