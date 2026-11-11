using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateFieldEgress
{
    internal static (ExplorationDefinition World, FieldEgressDefinition? Capability) Bind(ExplorationDefinition world)
    {
        // mainactions @IsOnOverworldMap selects source maps 66..78, independently of battle number.
        var maps = Enumerable.Range(66, 13).Select(id => new MapId($"map-{id}"))
            .Where(world.Maps.ContainsKey).ToArray();
        if (maps.Length == 0) return (world, null);
        const string program = "sf2-field-egress";
        Require(!world.Programs.ContainsKey(program), "field-egress-program-conflict", "world.programs");
        // Reuse the single admitted finite command-77 PCM under the existing modern SFX policy.
        // Keep the asset's recorded timer; no retiming, new capture or substituted command.
        var sound = world.Visuals?.Audio.Where(row => row.Value.Command == 77 && row.Value.LoopBegin is null).ToArray();
        Require(sound is { Length: 1 }, "field-egress-audio", "world.presentation.audio");
        var sequence = new StoryProgram(program, [
            new PresentCue(PresentationCueKind.Sound, sound![0].Key),
            new PresentCue(PresentationCueKind.FlashWhite, "white-four-pulses", DurationTicks: 32),
            new ResetPartyBattleStats(LivingOnly: true), new EndProgram()]);
        var bound = new ExplorationDefinition(world.Maps.Values, world.Programs.Values.Append(sequence),
            world.Texts, world.Provenance, world.PartyFlags, world.Visuals, world.MemberNames,
            world.TextTokens, world.TextFont) { GameplayRevision = world.GameplayRevision, Guidance = world.Guidance };
        return (bound, new(System.Array.AsReadOnly(maps), new(program, 0)));
    }
}
