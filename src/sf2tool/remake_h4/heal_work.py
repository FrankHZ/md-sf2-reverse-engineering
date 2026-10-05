"""Source phase work obligations and final HEAL cleanup/completion."""

from sf2tool.remake_h4.heal_checks import match, merge, missing, ordered_kinds


def compare_work(work, owner, ordinal, events, cast, idle, checks):
    """Compare source phase/caller work and cleanup before scene return."""
    check = checks.check
    actor, target = owner.get("actor"), owner.get("expectedTarget")
    end_sequence = owner.get("sceneEndSequence")
    phase_steps, phase_messages, transitions = work["steps"], work["messages"], work["transitions"]
    complete_work, prior_scene = work["complete"], work["scene"]
    phases = (
        ["ActionMessage", "SpellCost", "ActionAnimation"]
        + (["TargetExit", "TargetEnter"] if actor != target else [])
        + ["Reaction", "ResultMessage", "MakeIdle", "SpellStop"]
        + (["ActorExit", "ActorEnter"] if actor != target else [])
        + ["Reward", "RewardMessage", "End"]
    )
    check(
        "source phase continuation",
        merge(
            [
                ordered_kinds([dict(Kind=p) for p in phases], [dict(Kind=p) for p in transitions]),
                True if complete_work else None,
            ]
        ),
        ordinal,
    )
    required_work = [p for p in phases if p not in ("Reward", "RewardMessage", "End")]
    for phase in dict.fromkeys(required_work + list(phase_steps)):
        steps = phase_steps.get(phase, [])
        expected = []

        def wait(count, caller, expected=expected):
            expected.extend(
                dict(caller=caller, remaining=n, timed=False) for n in range(count, 0, -1)
            )

        if phase in ("SpellCost", "Reaction"):
            wait(1, "OpenAllyMiniStatus:move")
            wait(1, "OpenAllyMiniStatus:movement-end")
        elif phase in ("ActionMessage", "ResultMessage"):
            wait(1, "ClearDialogueWindowLayout")
            wait(1, "dialogue-layout-dma")
            wait(1, "dialogue-window-move")
            text = phase_messages.get(phase)
            if text is None:
                check("message caller operands", None, ordinal)
                continue
            for glyph in text:
                if glyph not in "\n|}":
                    wait(1, "HandleBlinkingDialogueCursor")
                    wait(1, "HandleDialogueTypewriting")
            timed = sum(x["caller"] == "bsc10:input-before-vint" for x in steps)
            check("source timed-input bound", timed <= 65, ordinal)
            for n in range(timed):
                expected.append(
                    dict(caller="bsc10:input-before-vint", remaining=65 - n, timed=True)
                )
        elif phase == "ActionAnimation" and cast is not None:

            def setup():
                for _ in range(4):
                    wait(4, "cast-flash-on")
                    wait(3, "cast-flash-off")
                wait(1, "LoadSpellTileset:dma")

            if cast[1] == 0:
                setup()
            for frame in range(1, cast[0]):
                if cast[8 + frame * 8] != 15:
                    wait(1, "LoadAllyBattlespriteFrame:dma")
                if frame + 1 == cast[1]:
                    setup()
                wait(cast[9 + frame * 8], "bsc01:frame-sleep")
        elif phase == "MakeIdle" and idle is not None:
            wait(1, "bsc05:idle-frame-before-dma")
            wait(idle[int(owner["targetSprite"])], "bsc05:ally-idle-sleep")
            wait(1, "bsc05:idle-frame-after-dma")
        elif phase == "TargetExit":
            wait(30, "SwitchTargets:wait")
            wait(9, "SwitchAlly:exit")
        elif phase == "TargetEnter":
            wait(32, "SwitchAlly:enter")
        elif phase == "ActorExit":
            wait(30, "SwitchActor:wait")
            wait(16, "SwitchActor:exit")
        elif phase == "ActorEnter":
            wait(8, "SwitchActor:enter")
        elif phase == "SpellStop":
            expected.extend(
                dict(caller="bsc0D:toggle-drain", remaining=0, timed=False)
                for x in steps
                if x["caller"] == "bsc0D:toggle-drain"
            )
            wait(1, "ReinitializeSceneAfterSpell:wait")
            wait(1, "bsc0D:restore-wait")
        else:
            check("source phase work available", None, ordinal)
            continue
        check(
            "source logical opportunities " + str(phase),
            match(expected, steps) if complete_work else None,
            ordinal,
        )
    check(
        "fairy cleared before caller returns",
        match(
            dict(Control=0, ActiveCount=0, CleanupPending=False),
            (prior_scene.get("healing") or {}).get("Fairy", missing),
        ),
        ordinal,
    )
    check(
        "actual scene completion",
        match(
            [dict(Kind="scene-ended", Actor=dict(Value=actor), Sequence=end_sequence)],
            [e for _, e in events if e.get("Kind") == "scene-ended"],
        ),
        ordinal,
    )
