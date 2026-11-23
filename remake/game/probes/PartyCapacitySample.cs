using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Probes;

// Admit the authored definition, then start the actual host with one member's unknown records.
public sealed partial class PartyCapacitySample : Node
{
    public override void _Ready()
    {
        var host = GD.Load<PackedScene>("res://Main.tscn").Instantiate<GameRoot>();
        var settings = new InputSettings { FormatVersion = 1, Language = OS.GetEnvironment("SF2_PARTY_LANGUAGE") };
        var output = ProjectSettings.GlobalizePath($"user://party-capacity/{Guid.NewGuid():N}/slot.json");
        host.ConfigurePreview(new Source(OS.GetEnvironment("SF2_PARTY_PACKAGE")), settings, output, () => GetTree().Quit());
        AddChild(host);
    }

    private sealed class Source(string package) : IScenarioSource
    {
        public ScenarioReadResult Read()
        {
            var content = new AuthoredScenarioPackageReader(package).Read();
            if (content is not ExplorationReadAccepted field) return content;
            var start = field.Start;
            var party = start.Party;
            var changed = new BattleStartInput(party.Encounter, party.Actors.Select(actor =>
                actor.Actor.Value == "guard-a" ? actor with { Kills = null, Defeats = null } : actor),
                party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies,
                party.ItemCustody, party.StoryFlags, party.Visuals, party.RetainedSourceSlots, party.DeathCredit);
            var names = new Dictionary<string, string>(start.Names) { ["medic-a"] = "WWWWWWW" };
            return field with { Start = new(start.Map, start.Player, start.Position, start.Facing, start.Speed,
                start.Flags, changed, start.EntryProgram, start.EntityPhases, start.Display, start.TextSettings,
                start.StepCounter, start.EgressMap) { Names = names } };
        }
    }
}
