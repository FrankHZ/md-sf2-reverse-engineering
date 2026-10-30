using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PartyDetailsQueryTests
{
    [Fact]
    public void ReadsCarriedGrowthEquipmentAndDuplicateSlotsWithoutPublishingState()
    {
        var read = Admitted("battle-equipment");
        var champion = read.Start.Actors[0];
        var loadout = new BattleSourceLoadout([178, 50, 127, 99], []);
        var progress = new BattleActorProgress(7, 250, 19, 40, 33, 28, [new("changed-magic", 3)], loadout);
        var session = Field(read, [champion with { Hp = 0, Mp = 5, Exp = 87, Progress = progress, SourceLoadout = loadout, Absent = true }]);
        var before = session.Current;
        var party = session.QueryPartyDetails()!;
        var member = Assert.Single(party.Members);
        Assert.Equal(7, member.Level!.Value); Assert.Equal(250, member.MaxHp!.Value); Assert.Equal(19, member.MaxMp!.Value);
        Assert.Equal(33, member.Defense!.Value); Assert.Equal(28, member.Agility!.Value);
        var definition = read.Definition.Encounters[read.Start.Encounter].Deployments.Single(d => d.Actor == champion.Actor).Definition;
        Assert.Equal(Math.Min(200, 40 + BattleEquipment.Contribution(definition, loadout)), member.Attack!.Value);
        Assert.Equal("CHANGED MAGIC 3", Assert.Single(member.Spells!).Name);
        Assert.Equal(new[] { 0, 1, 2, 3 }, member.Inventory.Select(s => s.Slot));
        Assert.True(member.Inventory[0].Equipped); Assert.False(member.Inventory[1].Equipped);
        Assert.NotNull(member.Inventory[0].Name);
        Assert.Equal(member.Inventory[0].Name, member.Inventory[1].Name);
        Assert.True(member.Inventory[2].Empty); Assert.False(member.Inventory[3].Empty); Assert.Null(member.Inventory[3].Name);
        Assert.Equal(0, member.Carried.Hp); Assert.True(member.Carried.Absent);
        Assert.Same(before, session.Current);
        Assert.Equal(party.Gold, session.QueryPartyDetails()!.Gold);
        Assert.Same(before.Exploration!.Party, session.Current.Exploration!.Party);
    }

    [Fact]
    public void JoinedFlagsExcludeFutureSeededAlliesButKeepDeadAbsentAndReserveMembers()
    {
        var read = Admitted();
        var encounter = read.Definition.Encounters[read.Start.Encounter];
        var allies = encounter.Deployments.Where(d => d.Faction == BattleFaction.Ally).ToArray();
        var deployments = encounter.Deployments.Select(d => d.Faction == BattleFaction.Ally ? d with
        { Initialization = new(null, 0, 0, 0, 0, AllyPartyMember: Array.IndexOf(allies, d)) } : d).ToArray();
        var changed = new BattleDefinition(encounter.Encounter, encounter.Map, encounter.Width, encounter.Height, encounter.Terrain,
            deployments, encounter.Spells.Values);
        var actors = read.Start.Actors.Select(a => a with { Hp = 0, Absent = true, Initialized = false }).ToArray();
        var session = Field(read, actors, changed, new(30, 100, 200, 12), [100]);
        Assert.Equal(allies[0].Actor, Assert.Single(session.QueryPartyDetails()!.Members).Carried.Actor);
        // The active flag is deliberately absent: reserve membership is still inspectable.
        Assert.Empty(session.Current.Story.PartyLists?.Active ?? []);
    }

    [Fact]
    public void MissingLoadoutIsUnknownRatherThanEmptyAndBattleHasNoFieldQuery()
    {
        var read = Admitted();
        var original = read.Definition.Encounters[read.Start.Encounter];
        var deployments = original.Deployments.Select(d => d with { Definition = new BattleActorDefinition(d.Actor,
            d.Definition.ClassRule, d.Definition.Level, d.Definition.MaxHp, d.Definition.MaxMp,
            d.Definition.Attack, d.Definition.Defense, d.Definition.Agility, false, d.Definition.Move, d.Definition.Spells) });
        var withoutLoadout = new BattleDefinition(original.Encounter, original.Map, original.Width, original.Height,
            original.Terrain, deployments, original.Spells.Values);
        var session = Field(read, read.Start.Actors, withoutLoadout);
        Assert.All(session.QueryPartyDetails()!.Members, member =>
            Assert.All(member.Inventory, slot => Assert.Null(slot.Empty)));
        Assert.Null(Start().QueryPartyDetails());
        // Authored outcome worlds can have party flags without source member bindings.
        // Their carried allies remain inspectable rather than disappearing after return.
        var authored = Field(read, read.Start.Actors, layout: new(30, 100, 200, 12));
        Assert.Equal(2, authored.QueryPartyDetails()!.Members.Count);
    }

    [Fact]
    public void ActualEquipmentCommitAndGrowthHaveTheSameFieldAndBattleAttack()
    {
        var battle = Start("battle-equipment");
        Accept(battle, new Confirm()); Accept(battle, new BeginEquipment());
        Accept(battle, new SelectEquipment(1)); Accept(battle, new Confirm());
        var equipped = battle.Current.Battle.Actors.Single(a => a.IsAlly);
        var curve = Enumerable.Repeat(new StatGrowthFraction(128, 128), 29).ToArray();
        var growth = new BattleGrowthDefinition(0, 0,
            Enumerable.Repeat(new StatGrowth(10, 30, curve), 5).ToArray(), [], new Dictionary<byte, SpellRef>());
        var definition = equipped.Definition.WithGrowth(growth);
        var actor = new BattleActorState(equipped.Deployment with { Definition = definition }, equipped.Hp,
            equipped.Mp, 99, equipped.Position, 0, 0, sourceLoadout: equipped.SourceLoadout);
        uint seed = 0x1234;
        var grown = BattleGrowthRules.Award(actor, 24, ref seed, []);
        Assert.True(grown.Level > actor.Level);
        foreach (var live in new[] { equipped, grown })
        {
            var encounter = battle.Current.Battle.Definition;
            var changed = new BattleDefinition(encounter.Encounter, encounter.Map, encounter.Width, encounter.Height,
                encounter.Terrain, [live.Deployment], encounter.Spells.Values);
            var carried = new BattleActorStartInput(live.Actor, live.Hp, live.Mp, live.Exp, live.Kills, live.Defeats,
                live.Status, null, live.Progress, live.SourceLoadout);
            var field = Field(Admitted("battle-equipment"), [carried], changed);
            var details = Assert.Single(field.QueryPartyDetails()!.Members);
            Assert.Equal(live.Attack, details.Attack!.Value);
            Assert.Equal(live.Level, details.Level!.Value);
            Assert.Equal(live.MaxHp, details.MaxHp!.Value);
            Assert.True(details.Inventory[1].Equipped);
        }
    }

    private static GameSession Field(ScenarioReadAccepted read, IEnumerable<BattleActorStartInput> actors,
        BattleDefinition? encounter = null, MapPartyFlagLayout? layout = null, int[]? flags = null)
    {
        var admitted = Assert.IsType<ExplorationReadAccepted>(new AuthoredScenarioPackageReader(PathFor("harbor-arrival")).Read());
        var field = admitted.Definition.Exploration!;
        var world = new ExplorationDefinition(field.Maps.Values, field.Programs.Values, field.Texts, partyFlags: layout);
        var definition = new ScenarioDefinition("party-inspection", [encounter ?? read.Definition.Encounters[read.Start.Encounter]], exploration: world);
        var party = new BattleStartInput(read.Start.Encounter, actors, read.Start.MainSeed, read.Start.ThinkingSeed, read.Start.Gold);
        var start = admitted.Start;
        return Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(start.Map, start.Player, start.Position, start.Facing, start.Speed, flags ?? [], party))).Session;
    }
}
