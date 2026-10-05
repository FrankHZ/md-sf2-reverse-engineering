"""Compose ordered audio consumer checks with caller-owned bounded report lists."""

from sf2tool.remake_h4.audio_events import select_events
from sf2tool.remake_h4.audio_finite import compare_finite_releases
from sf2tool.remake_h4.audio_identity import select_identity
from sf2tool.remake_h4.audio_playback import compare_playbacks
from sf2tool.remake_h4.audio_scene import compare_scene_releases
from sf2tool.remake_h4.audio_source import classify_slots
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def audio_consumer_binding(actual, context, source_root, bounded_list):
    """Compose accepted audio rules with complete selected playback/release edges.

    WaitToken describes current service context, not a voice. A cue with exactly
    one outstanding start can be associated uniquely; overlapping same-cue voices
    require retained instance identity and remain unavailable here.
    """
    result = dict(
        value=None,
        checks=bounded_list(),
        playbacks=bounded_list(),
        releases=bounded_list(),
        sourceRules=dict(
            upstream=UPSTREAM,
            driver="disasm/code/common/tech/sound/sounddriver.asm:Load_Music/Load_SFX/"
            "Fade_Out/UpdateSound/StopMusic/loc_DF2/loc_F88",
            bus="disasm/code/common/tech/interrupts/"
            "applyfadingeffectandz80busupdate.asm:ApplyZ80BusUpdates/@IsFadeOut",
            policy="remake/docs/presentation-and-assets.md#sound-fade-request-and-effect-lifetime",
            originalCompletion="Unknown",
        ),
    )

    def check(name, value, **identity):
        result["checks"].append(dict(name=name, value=value, **identity))

    def finish():
        values = [c["value"] for c in result["checks"]]
        result["value"] = False if False in values else None if None in values else True
        return result

    if not context or not actual.get("audioReceipts"):
        check("complete selected audio/context absent", None)
        return finish()
    required = (
        "audioReceiptGaps",
        "audioSequenceSeen",
        "audioTerminal",
        "warpRecords",
        "sceneObservations",
        "samples",
        "inputRecords",
    )
    if any(key not in actual for key in required) or any(
        key not in context for key in ("provenance", "audio", "programs", "events")
    ):
        check("selected dependency channels absent", None)
        return finish()
    check(
        "selected complete default keyboard audio scope",
        True if actual.get("h4Variant") == "A" else None,
    )
    check(
        "selected original identity",
        context.get("provenance", {}).get("commit") == UPSTREAM
        and context.get("provenance", {}).get("romSha256") == ROM,
    )
    classified = classify_slots(source_root, check)
    if classified is None:
        return finish()
    slots, types = classified
    selected = select_identity(actual, context, check)
    if selected is None:
        return finish()
    assets, wrappers, receipts, terminal, session = selected
    playback = compare_playbacks(
        receipts, assets, terminal, slots, types, result["playbacks"], check
    )
    if playback is None:
        return finish()
    instances, fades = playback
    events = select_events(actual, session, check)
    compare_scene_releases(
        actual, session, events, fades, instances, receipts, wrappers, result["releases"], check
    )
    compare_finite_releases(
        actual, context, session, events, assets, instances, receipts, result["releases"], check
    )
    return finish()
