using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class NorthCliffTests
{
    private static (ScenarioDefinition Definition, SessionSnapshot Current) LoadCliff()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var read = new OpeningRestoreReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), Input("SF2_PRIVATE_BATTLE_SCENE_CONTENT")).Read();
        var definition = Assert.IsType<ExplorationReadAccepted>(read).Definition;
        var record = OpeningSaveJson.ReadRecord(File.ReadAllBytes(Input("SF2_NORTH_CLIFF_SAVE")), 1);
        var original = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2())).Session.Current;
        return (definition, original);
    }

    [PrivateInputFact("SF2_NORTH_CLIFF_SAVE")]
    public void CliffCallerUsesJoinedCustodyBeforeStartingEitherScene()
    {
        var (definition, original) = LoadCliff();
        var world = original.Exploration!;
        var reserve = world.Party.Actors.Single(row => row.Actor.Value == "ally-2");
        Assert.DoesNotContain(reserve.Actor, world.Party.ActiveAllies!);
        Assert.DoesNotContain(11, original.Story.Flags);

        // All variants keep a Cannon in the depot and both items on an unjoined Rohde.
        // The dead reserve holds the only eligible pack. Duplicate whole words make
        // both the reverse search and the Dynamite slot shift observable.
        (ushort[] Pack, ushort[] Expected, bool Success)[] cases = [
            ([114, 116, 114, 116], [114, 116, 127, 127], true),
            ([114, 116, 1, 127], [1, 127, 127, 127], true),
            ([114, 1, 127, 127], [114, 1, 127, 127], false),
            ([116, 1, 127, 127], [116, 1, 127, 127], false)];
        foreach (var (pack, expected, success) in cases)
        {
            var actors = world.Party.Actors.Select(actor =>
            {
                ushort[] items = actor.Actor == reserve.Actor ? pack : actor.Actor.Value == "ally-11"
                    ? [114, 116, 127, 127] : [127, 127, 127, 127];
                var loadout = new BattleSourceLoadout(items, actor.SourceLoadout!.Spells);
                return actor with { Hp = actor.Actor == reserve.Actor ? (ushort)0 : actor.Hp,
                    SourceLoadout = loadout, Progress = actor.Progress! with { SourceLoadout = loadout } };
            });
            var party = FieldInventory.Replace(world.Party, actors, world.Party.ItemCustody!.WithDepot([114]));
            var start = new SessionSnapshot(original.SessionId, original.Revision, original.ObservationSequence,
                new ActiveExploration(world.WithParty(party)), original.Story.Copy(new("map72-zoneevent3", 0),
                    flags: original.Story.Flags.Except([524, 67])), original.StopReason);
            var result = ProgramRunner.Run(definition, start, []);
            Assert.Null(result.Failure);
            var after = result.Snapshot;
            Assert.Equal(success ? "cs-4ffda" : "cs-5023e", after.Story.Cursor?.Program);
            Assert.DoesNotContain(750, after.Story.Flags);
            Assert.DoesNotContain(256, after.Story.Flags);
            Assert.Equal(expected, after.Exploration!.Party.Actors.Single(a => a.Actor == reserve.Actor).SourceLoadout!.Items);
            Assert.Equal(new byte[] { 114 }, after.Exploration.Party.ItemCustody!.Depot);
            if (!success) Assert.Equal((byte)116, after.Story.DialogueItem);
        }
    }

    [PrivateInputFact("SF2_NORTH_CLIFF_SAVE")]
    public void ExplicitMapEntryClearsTheVisitLatchAndCopiesOnlyTheOpenedEntrance()
    {
        var (definition, original) = LoadCliff();
        var world = original.Exploration!;
        foreach (bool opened in new[] { false, true })
        {
            var flags = original.Story.Flags.Append(256);
            if (opened) flags = flags.Append(750).Append(11);
            var before = original.WithStory(original.Story.Copy(null, flags: flags));
            var entered = MapTransfer.Apply(definition, before, world.Map, world.PlayerEntity.Position, 1,
                MapLoadMode.Rebuild, before.Story, [], RuleCompositions.Sf2());
            Assert.DoesNotContain(256, entered.Story.Flags);
            var fresh = entered.Exploration!;
            for (int y = 0; y < 2; y++)
                Assert.Equal(opened ? world.Layout[0, y] : world.Layout[27, 33 + y], fresh.Layout[27, 33 + y]);
            Assert.DoesNotContain(fresh.AllEntities, e => e.Entity.Value is "entity-11" or "entity-128" or "entity-129");
        }
    }
}
