"""Compose source construction and selected battle-scene consumers."""

from sf2tool.remake_h4.scene_checks import SceneChecks, merge
from sf2tool.remake_h4.scene_construction import construct_scenes
from sf2tool.remake_h4.scene_dependencies import admit_dependencies, collect_dependencies
from sf2tool.remake_h4.scene_evidence import compare_evidence
from sf2tool.remake_h4.scene_fielddeath import field_batches
from sf2tool.remake_h4.scene_phases import compare_phases
from sf2tool.remake_h4.scene_source import SceneMaterials
from sf2tool.remake_h4.scene_terminal import compare_terminal
from sf2tool.remake_h4_reference import UPSTREAM


def battle_scene_consumer_binding(actual, context, source_root, read_document):
    """Compose selected scene consumers with an explicit caller-owned document reader."""
    context = context or {}
    session = context.get("sessionId")
    checks = SceneChecks(session)
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/battle-scene-presentation.md",
            commands="ExecuteBattlesceneScript; InitializeBattlescene; "
            "bsc00/01/0A/0B/0D/10; EndBattlescene",
            construction="battlesceneScript_ApplyActionEffect/SwitchTargets/GiveExpAndGold/End",
            dependencies=[
                "PR606 resources",
                "PR629 physical",
                "PR625 HEAL",
                "PR630 reward/outcome",
                "PR631 AI transport",
                "PR619 audio",
                "PR560 field death",
                "PR588 return",
            ],
        ),
        unknown=[
            "original natural command/frame/VInt/hardware timing and rendered pixels",
            "unsampled intermediate reaction frames and weapon layer/Y projection",
            "terminal FieldSettle internal completed flag is Inferred; delay is Unknown",
            "historical A seed latch FAIL and HEAL timing FAIL are not corrected by this proof",
        ],
    )

    receipt, reward_context, physical_context, heal_context, materials, sequences, texts = (
        admit_dependencies(context, source_root, read_document, checks)
    )
    census = reward_context.get("scenes", [])
    warps, inputs, healing, events, owners, ordered = collect_dependencies(
        actual, context, read_document, checks
    )
    starts, pairs, bytoken, selected = compare_evidence(
        actual, receipt, census, healing, events, owners, ordered, checks
    )
    source_materials = SceneMaterials(materials, texts)
    scene_info, result["occurrences"] = construct_scenes(
        source_root,
        physical_context,
        heal_context,
        census,
        warps,
        events,
        owners,
        ordered,
        pairs,
        checks,
    )
    batches = field_batches(census, pairs, events, ordered, checks)
    terminal = compare_phases(
        pairs,
        bytoken,
        warps,
        inputs,
        owners,
        starts,
        batches,
        scene_info,
        reward_context,
        healing,
        materials,
        sequences,
        events,
        ordered,
        source_materials,
        checks,
    )
    compositions = compare_terminal(
        terminal,
        batches,
        warps,
        inputs,
        events,
        owners,
        ordered,
        physical_context,
        bytoken,
        session,
        checks,
    )
    if compositions:
        result["compositions"] = compositions
    result["checks"] = list(checks.rows.values())
    result["value"] = merge([c["value"] for c in result["checks"]])
    result["coverage"] = dict(
        scenes=len(census),
        phases=len(pairs),
        selected=len(selected),
        terminalCompositions=len(terminal),
    )
    return result
