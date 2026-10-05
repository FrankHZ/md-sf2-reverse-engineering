"""Selected original seed admission and the accepted executed copy/entry mechanism.

This independent dependency is shared by AI, field-service, modern and scoped CLI
consumers. Its historical latch result and missing/contradiction rules are distinct.
"""

import re
import subprocess
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import ROM, UPSTREAM, require

OWNER = "docs/design/contracts/map3-battle01-continuous-scenario.md"


def admission_seed_binding(actual, context, source_root):
    """Selected non-resume source path plus accepted PR621 executed mechanism.

    The historical capture supplies text delivery, never a corrected active image.
    Missing edges do not mask an independently available contradiction.
    """
    import xml.etree.ElementTree as ET

    context = context or {}
    result = dict(
        value=None,
        checks=[],
        historical=dict(result="FAIL", reason="disconnected historical A latch"),
        unknown=["original low24 image", "later first AI value"],
    )
    absent = object()

    def merge(values):
        return False if False in values else None if None in values else True

    def match(want, got=absent):
        if want is absent or got is absent or got is None and want is not None:
            return None
        if isinstance(want, dict):
            return (
                merge([match(v, got.get(k, absent)) for k, v in want.items()])
                if isinstance(got, dict)
                else False
            )
        return type(got) is bool and got == want if isinstance(want, bool) else got == want

    def check(name, value):
        result["checks"].append(dict(name=name, value=value))

    def one(name, rows):
        check(name + " unique", None if not rows else len(rows) == 1)
        return rows[0] if rows else {}

    def progression(name, values, *, strict=False):
        # Match W2's progression rules, but evaluate known operands even when
        # another operand is absent. Missing evidence cannot hide an inversion.
        known = [v for v in values if v is not None]
        check(
            name,
            merge(
                [
                    True if len(known) == len(values) else None,
                    all(v >= 0 for v in known),
                    all(
                        a < b if strict else a <= b for a, b in zip(known, known[1:], strict=False)
                    ),
                ]
            ),
        )

    check(
        "explicit composed scope",
        match("retained-keyboard-A-admission-seed", context.get("scope", absent)),
    )
    session = context.get("sessionId")
    check("historical session identity", match("99755635-cad5-4fff-bc88-b70fcc5f017b", session))
    original = context.get("original") or {}
    check(
        "selected original lineage",
        match(
            dict(
                SourceCommit=UPSTREAM,
                RomSha256=ROM,
                ConfigurationSha256="C881181FB83770EA71E815DC8A2EF16F4476F52256A57EEDF5BA6FE4DE72178D",
                ObserverSha256="8349604F919EB7071897CF9921ED51F951F5875A745B51289257B3F7CBB7D75B",
                RunnerSha256="97D424459D190B843598642F078BBF0CEDE2C7787F8FC0EB5492255E263D1339",
            ),
            original.get("candidate", absent),
        ),
    )
    selected = {}
    for name, kind, facts in (
        ("loop", "battle:loop", dict(d1=1, f88=False)),
        ("program", "script:entry", dict(target=0x494BC)),
        ("writeCall", "rng:GenerateRandomNumber:entry", dict(returnPc=0x647E, target=0x1600)),
        ("writeDraw", "rng:draw", dict(source="GenerateRandomNumber", before=dict(d6=256))),
        ("return", "script:return", dict(target=0x494BC)),
        ("turn", "battle:turn-dispatch", {}),
    ):
        row = original.get(name) or {}
        selected[name] = row
        check("original " + name, match(dict(kind=kind, facts=facts), row))
        check(
            "original non-resume flag at " + name,
            match(False, (row.get("state") or {}).get("flags", {}).get("88", absent)),
        )
    progression(
        "original write precedes turn readers",
        [row.get("order") for row in selected.values()],
        strict=True,
    )
    progression(
        "original frames follow source call order", [row.get("frame") for row in selected.values()]
    )
    check(
        "original W2 draw belongs to selected call",
        None
        if selected["writeCall"].get("order") is None or selected["writeDraw"].get("order") is None
        else selected["writeDraw"]["order"] == selected["writeCall"]["order"] + 2,
    )
    # A returned selected bbcs_01 cannot bypass its first W2. The only live copy
    # readers in this pinned source are battle AI; the unsigned wrapper has no
    # caller. This premise applies to the observed flag88-clear branch only.
    try:
        root = Path(source_root) if source_root else None
        if root is None:
            raise FileNotFoundError("original source not selected")
        root = root.resolve() if root.is_absolute() else repo_path(root)
        pin = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        clean = subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        check("reviewed original source pin and clean code", pin == UPSTREAM and clean == 0)
    except (OSError, subprocess.CalledProcessError):
        check("reviewed original source pin and clean code", None)
    result["sourceRule"] = dict(
        commit=UPSTREAM,
        owner=OWNER,
        section="admission-seed-copy-composition",
        branch="BattleLoop flag88 clear / Initialize",
        writer="bbcs_01 text2292 -> ParseSpecialTextSymbol loc_6472 -> byte write at647E",
        readers="battle AI -> GenerateRandomNumberUnderD6 -> GenerateRandomValueSigned",
        unused="GenerateRandomValueUnsigned only called by uncalled WaitForRandomValueToMatch",
    )

    # Reuse the accepted occurrence's raw delivery channels, without rerunning
    # the full W2 predicate or treating its old text latch as active state.
    inp = one(
        "historical beforebattle input",
        [r for r in actual.get("inputRecords", []) if r.get("_index", r.get("index")) == 812],
    )
    submit = one(
        "historical beforebattle result",
        [r for r in actual.get("warpRecords", []) if r.get("_index", r.get("index")) == 13797],
    )
    before, after = inp.get("before") or {}, inp.get("after") or {}
    envelope, state = submit.get("result") or {}, submit.get("state") or {}
    check(
        "selected W2 caller input",
        match(
            dict(
                ordinal=407,
                pressed=True,
                action="confirm",
                before=dict(
                    sessionId=session,
                    token=23128,
                    wait="FieldTextWait",
                    cursor=dict(Program="bbcs-01", Instruction=17),
                ),
            ),
            inp,
        ),
    )
    check(
        "selected W2 result session",
        match(dict(sessionId=session, revision=23291, failure=None), envelope),
    )
    check(
        "selected W2 result state",
        match(dict(sessionId=session, revision=23291), submit.get("state", absent)),
    )
    check(
        "whole Submit owns input ordinal and boundary",
        match(
            dict(inputOrdinal=inp.get("ordinal", absent), result=dict(boundary="submit")), submit
        ),
    )
    # Same span/snapshot joins as the W2 consumer. The retained indices locate
    # evidence; the physical input must actually own that result and after-state.
    start, end = inp.get("resultStart"), inp.get("resultEnd")
    index = submit.get("_index", submit.get("index"))
    check(
        "selected result at or after input span start",
        None if start is None or index is None else start <= index,
    )
    check(
        "selected result before input span end",
        None if end is None or index is None else index < end,
    )
    if start is not None and end is not None:
        span = [
            r
            for r in actual.get("warpRecords", [])
            if r.get("_index", r.get("index")) is not None
            and start <= r.get("_index", r.get("index")) < end
        ]
        one("one result in physical input span", span)
    identity_keys = ("sessionId", "revision", "observationSequence", "mode")
    # Pairwise comparison retains a known mismatch if the third channel is missing.
    for left_name, left, right_name, right in (
        ("result", envelope, "state", state),
        ("result", envelope, "input after", after),
        ("state", state, "input after", after),
    ):
        check(
            left_name + " joins " + right_name,
            match({k: left.get(k, absent) for k in identity_keys}, right),
        )
    snapshot_keys = ("simulationTick", "mainSeed", "token", "cursor", "wait", "map")
    check(
        "input after joins whole Submit snapshot",
        match({k: after.get(k, absent) for k in snapshot_keys}, state),
    )
    events = envelope.get("observations") or []
    kinds = ["rng-text-w2", "text-seed-copy", "text-w2-wait", "text-w2-input", "text-w2-accepted"]
    found, positions = {}, []
    for kind in kinds:
        rows = [(i, e) for i, e in enumerate(events) if e.get("Kind") == kind]
        found[kind] = one(kind, [e for _, e in rows])
        positions.extend(i for i, _ in rows)
    check("actual draw copy wait read order", positions == sorted(positions))
    check("write before service", None if len(positions) < 3 else positions[:3] == [0, 1, 2])
    draw, copy = found["rng-text-w2"], found["text-seed-copy"]
    check(
        "range and input main seed",
        match(dict(RandomRange=256, Before=before.get("mainSeed", absent)), draw),
    )
    if draw.get("Before") is not None:
        word, value = _rng_step(int(draw["Before"]) >> 16, 512)
        check(
            "independent main draw arithmetic",
            match(
                dict(After=(word << 16) | (int(draw["Before"]) & 65535), RandomValue=value >> 1),
                draw,
            ),
        )
    else:
        check("independent main draw arithmetic", None)
    check(
        "delivered copy byte",
        None
        if draw.get("RandomValue") is None
        else match(draw["RandomValue"], copy.get("After", absent)),
    )
    check("accepting source read", match("accept", found["text-w2-input"].get("Detail", absent)))
    for event_axis, snapshot_axis, strict in (
        ("Revision", "revision", False),
        ("Sequence", "observationSequence", True),
    ):
        values = [e.get(event_axis) for e in events]
        progression("event " + event_axis + " progression", values, strict=strict)
        lower = before.get(snapshot_axis)
        # Bound against every available ending channel. A missing envelope must
        # not suppress a contradiction already present in state or input after.
        for name, snapshot in (("result", envelope), ("state", state), ("input after", after)):
            upper = snapshot.get(snapshot_axis)
            progression(
                "input before to " + name + " " + snapshot_axis, [lower, upper], strict=True
            )
            for value in values:
                check(
                    "event " + event_axis + " follows input before",
                    None if value is None or lower is None else lower < value,
                )
                check(
                    "event " + event_axis + " inside " + name,
                    None if value is None or upper is None else value <= upper,
                )
        check(
            "last event closes Submit " + snapshot_axis,
            None if not values else match(envelope.get(snapshot_axis, absent), values[-1]),
        )
    progression(
        "copy commit follows draw commit", [draw.get("Revision"), copy.get("Revision")], strict=True
    )

    accepted = "bbf98c8ddbd04500d57165a958f78f86a9ff209b"
    check("accepted correction identity", match(accepted, context.get("correctionCommit", absent)))
    # Direct comparison of owned proof dependencies, not a new hash manifest.
    for path in (
        "remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs",
        "remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/SourceEnemyAi.cs",
        "remake/tests/Sf2.Remake.Engine.Tests/SourceEnemyAiTests.cs",
    ):
        try:
            old = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
                text=True,
                encoding="utf-8",
            )
            # Changed proof dependencies require renewed review, not a claim that
            # any source change is necessarily a behavior failure.
            check(
                "accepted mechanism dependency " + path,
                True if repo_path(path).read_text(encoding="utf-8") == old else None,
            )
        except (OSError, subprocess.CalledProcessError):
            check("accepted mechanism dependency " + path, None)
    path = "remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs"
    try:
        old = subprocess.check_output(
            ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
            text=True,
            encoding="utf-8",
        )
        pattern = r"public byte CurrentRandomSeedCopy\s*=>.*?;"
        previous = re.findall(pattern, old, re.S)
        check(
            "current active-byte projection",
            True
            if len(previous) == 1
            and previous == re.findall(pattern, repo_path(path).read_text(encoding="utf-8"), re.S)
            else None,
        )
    except (OSError, subprocess.CalledProcessError):
        check("current active-byte projection", None)
    path = "remake/game/src/Exploration/ExplorationSessionView.cs"
    try:
        old = subprocess.check_output(
            ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
            text=True,
            encoding="utf-8",
        )
        pattern = (
            r"randomSeedCopy = current\?\.CurrentRandomSeedCopy, "
            r"lastTextSeedCopy = current\?\.Story\.RandomSeedCopy,"
        )
        check(
            "host projects active byte separately from text latch",
            True
            if re.search(pattern, old)
            and re.search(pattern, repo_path(path).read_text(encoding="utf-8"))
            else None,
        )
        receipt = Path(context["correctionAdapterExit"])
        receipt = receipt if receipt.is_absolute() else repo_path(receipt)
        check(
            "accepted adapter compile",
            receipt.stat().st_size < 32 and receipt.read_text(encoding="utf-8-sig").strip() == "0",
        )
    except (KeyError, OSError, subprocess.CalledProcessError):
        check("accepted host mechanism evidence", None)
    try:
        trx_path = Path(context["correctionTrx"])
        trx_path = trx_path if trx_path.is_absolute() else repo_path(trx_path)
        require(trx_path.stat().st_size <= 64 * 1024, "seed correction TRX exceeds 64KiB")
        trx = ET.fromstring(trx_path.read_bytes())
        check(
            "accepted behavior execution identity",
            match("b4adba34-a764-4ebb-abcf-a3149653ffae", trx.get("id", absent)),
        )
        ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
        method = (
            "Sf2.Remake.Engine.Tests.SourceEnemyAiTests."
            "FieldTextCopyReplacesInitialAiByteBeforeEntryAndAiKeepsItsUpdatedByte"
        )
        expected = {
            f'{method}(textToken: "{{{token}}}", preserved: {preserved})'
            for token, preserved in (("W1", 3408025), ("W2", 3408025), ("W2", 11259375))
        }
        results = trx.findall("t:Results/t:UnitTestResult", ns)
        names = [r.get("testName") for r in results]
        check(
            "accepted executed behavior cases",
            False
            if set(names) - expected or len(set(names)) != len(names)
            else True
            if set(names) == expected
            else None,
        )
        check(
            "accepted behavior outcomes",
            merge([match("Passed", r.get("outcome", absent)) for r in results]),
        )
    except (KeyError, OSError, ValueError, ET.ParseError):
        check("accepted executed behavior evidence", None)
    result["mechanism"] = dict(
        commit=accepted,
        kind="accepted executed assertions plus unchanged source",
        actualEdge="ordinary text copy -> active Party image -> BattleEntry -> first AI read",
        notObserved="no corrected historical-A or whole-route host trace",
    )
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
