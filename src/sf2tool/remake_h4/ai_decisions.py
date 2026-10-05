"""Compare matched AI callers, source effects and logical action delivery."""

import copy

from sf2tool.remake_h4.ai_checks import absent, event_clock, number, who
from sf2tool.remake_h4.ai_source import source_decision


def compare_decisions(rows, events, owners, census, source, checks):
    """Append causal decision/delivery checks; return occurrence summaries and Unknowns."""
    check, eq, before = checks.check, checks.eq, checks.before
    occurrences, unknown = [], []
    ordered = sorted(events.values(), key=lambda e: e.get("Sequence", -1))
    decisions = []
    for index, row in rows.items():
        envelope = row.get("result") or {}
        es = envelope.get("observations") or []
        if not any(e.get("Kind") == "regions-tested-cleared" for e in es):
            continue
        prior_state = rows.get(index - 1, {}).get("state") or {}
        actors = {x.get("id"): copy.deepcopy(x) for x in prior_state.get("actors") or []}
        seed = prior_state.get("thinkingSeed", absent)
        check("immediate caller actors", True if actors else None, index)
        check(
            "immediate caller thinking image", number(seed) if seed is not absent else None, index
        )
        check("immediate caller tested regions", "regionsTested" in prior_state or None, index)
        tested = prior_state.get("regionsTested", absent)
        missing_effects = [r for r in census if r[0] == index and r[2] not in events]
        previous_sequence = prior_state.get("observationSequence", -1)
        row_has_main_draw = any(
            str(e.get("Kind", "")).startswith(("rng-", "round-rng")) for e in es
        )
        for n, e in enumerate(es):
            actor = who(e)
            seq = e.get("Sequence")
            kind = e.get("Kind")
            unit = actors.get(actor, {})
            if not number(seq):
                continue
            if any(
                previous_sequence < r[2] < seq and r[3] == "thinking-rng" for r in missing_effects
            ):
                seed = absent
            if any(
                previous_sequence < r[2] < seq
                and r[3] in ("regions-tested", "regions-tested-cleared")
                for r in missing_effects
            ):
                tested = absent
            previous_sequence = seq
            if kind == "regions-tested":
                tested = e.get("After", absent)
            if kind == "regions-tested-cleared":
                eq("tested regions input", tested, e.get("Before", absent), seq)
                eq("tested regions cleared", 0, e.get("After", absent), seq)
                tested = 0
                following = []
                for f in es[n + 1 :]:
                    if f.get("Kind") == "regions-tested-cleared":
                        break
                    if str(f.get("Kind", "")).startswith(("ai-", "thinking-", "source-standby")):
                        following.append(f)
                for key in (
                    "aiMemory",
                    "lastTarget",
                    "activationWord",
                    "primaryOrder",
                    "secondaryOrder",
                    "anchorX",
                    "anchorY",
                    "move",
                ):
                    check("caller " + key, True if key in unit else None, seq)
                if source and actors and number(seed):
                    try:
                        expected = source_decision(source, actors, actor, int(seed))
                        eq("source ordered AI effects", expected["effects"], following, seq)
                        expected.update(
                            sequence=seq, revision=e.get("Revision"), actor=actor, index=index
                        )
                        decisions.append(expected)
                    except (KeyError, TypeError, StopIteration):
                        check("source decision operands", None, seq)
                    except (AssertionError, ValueError) as error:
                        check("caller in admitted source domain", False, seq)
                        unknown.append(str(error))
                else:
                    check("source decision operands", None, seq)
            if kind == "thinking-rng":
                eq("thinking draw live input", seed, e.get("Before", absent), seq)
                if number(e.get("Before")) and number(e.get("RandomRange")):
                    from sf2tool.h3.random_services import _signed_byte_step

                    bound = int(e["RandomRange"])
                    image = int(e["Before"])
                    word = image >> 16
                    check("thinking image domain", image <= 0xFFFFFFFF and 0 < bound < 128, seq)
                    if image <= 0xFFFFFFFF and 0 < bound < 128:
                        for _ in range(256):
                            word = _signed_byte_step(word)
                            value = word >> 8
                            if bound <= 1:
                                value = 0
                                break
                            if value < bound:
                                break
                        eq(
                            "source thinking draw and preserved other24 bits",
                            dict(After=(word << 16) | (image & 65535), RandomValue=value),
                            e,
                            seq,
                        )
                else:
                    check("thinking draw operands", None, seq)
                seed = e.get("After", absent)
            if actor in actors:
                if kind == "movement" and isinstance(e.get("To"), dict):
                    unit["x"] = e["To"].get("X")
                    unit["y"] = e["To"].get("Y")
                if kind == "ai-memory":
                    eq(
                        "memory live input",
                        unit.get("aiMemory", absent),
                        e.get("Before", absent),
                        seq,
                    )
                    unit["aiMemory"] = e.get("After", absent)
                if kind == "ai-target":
                    unit["lastTarget"] = who(e, "Target")
                if kind == "activation-word":
                    unit["activationWord"] = e.get("After", absent)
        if any(r[2] > previous_sequence and r[3] == "thinking-rng" for r in missing_effects):
            seed = absent
        for _, _, _, kind, actor, _ in missing_effects:
            if actor in actors and kind in ("ai-memory", "ai-target"):
                actors[actor]["aiMemory" if kind == "ai-memory" else "lastTarget"] = absent
        post = row.get("state") or {}
        eq("AI thinking image delivered", seed, post.get("thinkingSeed", absent), index)
        if not row_has_main_draw:
            eq(
                "AI preserves independent main image",
                prior_state.get("mainSeed", absent),
                post.get("mainSeed", absent),
                index,
            )
        current = {x.get("id"): x for x in post.get("actors") or []}
        memories = {x.get("actor"): x for x in post.get("aiMemory") or []}
        eq("unique memory actors", len(post.get("aiMemory") or []), len(memories), index)
        for actor, a in actors.items():
            if actor.startswith("enemy-"):
                eq(
                    "memory/target actor state delivered",
                    {k: a.get(k, absent) for k in ("aiMemory", "lastTarget")},
                    current.get(actor, absent),
                    index,
                )
                eq(
                    "independent memory projection",
                    dict(memory=a.get("aiMemory", absent), lastTarget=a.get("lastTarget", absent)),
                    memories.get(actor, absent),
                    index,
                )
    for decision in decisions:
        seq = decision["sequence"]
        actor = decision["actor"]
        commit_sequences = sorted(
            r[2] for r in census if r[2] > seq and r[3] == "action-committed" and r[4] == actor
        )
        end = commit_sequences[0] if commit_sequences else float("inf")
        commit = events.get(end)
        check("AI decision consumed by action commit", True if commit else None, seq)
        tail = [e for e in ordered if seq < e["Sequence"] <= end and who(e) == actor]
        next_ai = [e for e in tail if e.get("Kind") == "regions-tested-cleared"]
        check("commit before next same-actor decision", not next_ai, seq)
        path = decision["path"]
        pairs = list(zip(path, path[1:], strict=False))
        for kind in ("battle-movement-segment-started", "battle-movement-segment-arrived"):
            delivered = [e for e in tail if e.get("Kind") == kind]
            expected = [
                dict(
                    Kind=kind,
                    Actor={"Value": actor},
                    Detail="Automatic",
                    From=dict(X=a[0], Y=a[1]),
                    To=dict(X=b[0], Y=b[1]),
                )
                for a, b in pairs
            ]
            eq("source logical path " + kind, expected, delivered, seq)
        starts = [e for e in tail if e.get("Kind") == "battle-movement-segment-started"]
        arrivals = [e for e in tail if e.get("Kind") == "battle-movement-segment-arrived"]
        previous = event_clock(dict(Revision=decision["revision"], Sequence=seq))
        for start in starts:
            before("segment starts after previous boundary", previous, event_clock(start))
            arrive = next(
                (
                    e
                    for e in arrivals
                    if e.get("From") == start.get("From") and e.get("To") == start.get("To")
                ),
                None,
            )
            if arrive:
                before("segment arrives after start", event_clock(start), event_clock(arrive))
                previous = event_clock(arrive)
        finish = [e for e in tail if e.get("Kind") == "battle-movement-finished"]
        if pairs:
            eq(
                "logical movement completion",
                [
                    dict(
                        Actor={"Value": actor},
                        Detail="Automatic",
                        From=dict(X=path[0][0], Y=path[0][1]),
                        To=dict(X=path[-1][0], Y=path[-1][1]),
                    )
                ],
                finish,
                seq,
            )
            if finish:
                before("finish follows arrival", previous, event_clock(finish[0]))
        else:
            eq("Stay has no movement completion", [], finish, seq)
        prepared = [e for e in tail if e.get("Kind") == "scene-prepared"]
        eq(
            "chosen action prepared",
            [dict(Actor={"Value": actor})] if decision["kind"] == "attack" else [],
            prepared,
            seq,
        )
        strikes = [e for e in tail if e.get("Kind") == "physical-first"]
        eq(
            "chosen target consumed by physical strike",
            [dict(Actor={"Value": actor}, Target={"Value": decision["target"]})]
            if decision["kind"] == "attack"
            else [],
            strikes,
            seq,
        )
        if prepared and finish:
            before(
                "movement finishes before physical preparation",
                event_clock(finish[0]),
                event_clock(prepared[0]),
            )
        if commit:
            post = rows.get(owners[commit["Sequence"]], {}).get("state") or {}
            a = next((a for a in post.get("actors") or [] if a.get("id") == actor), absent)
            eq(
                "consumed position memory and target",
                dict(
                    x=path[-1][0],
                    y=path[-1][1],
                    aiMemory=decision["memory"],
                    lastTarget=decision["lastTarget"],
                ),
                a,
                seq,
            )
        occurrences.append(
            {
                k: decision[k]
                for k in (
                    "sequence",
                    "revision",
                    "actor",
                    "index",
                    "kind",
                    "path",
                    "target",
                    "memory",
                    "seed",
                    "lastTarget",
                )
            }
        )
    return occurrences, unknown
