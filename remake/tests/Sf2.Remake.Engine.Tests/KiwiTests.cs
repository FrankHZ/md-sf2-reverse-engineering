using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class KiwiTests
{
    [Theory]
    [InlineData(BattleMover.Regular, 30, 0)]
    [InlineData(BattleMover.Flying, 18, 0)]
    [InlineData(BattleMover.Archer, 30, 0)]
    [InlineData(BattleMover.Mage, 2, 1)]
    public void TortoiseParticipatesInSourcePriorityTies(BattleMover mover, int rank, int selected)
    {
        var table = PhysicalTargetRules.Table(mover);
        byte? tortoise = Sf2ClassRules.SourceClass(BattleClassRule.UnpromotedTortoise);
        Assert.Equal(rank, PhysicalTargetRules.ClassRank(tortoise, table));
        // A tied lethal cohort ranks class before movement. Mage targets TORT
        // ahead of PRST; the other selected tables retain the opposite order.
        var result = PhysicalTargetRules.Select([new(1, 16, 4), new(2, 16, tortoise)], table);
        Assert.Equal(selected, result!.Value.Index);
        Assert.Equal(15, result.Value.CappedPriority);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DirectNamingConsentDoesNotWriteSceneChoiceFlagOrJoinBeforeNaming(bool accept)
    {
        var (definition, current) = NamingStart(new ChooseYesNo(null, new("declined", 0)), new NamePartyMember(1),
            new JoinPartyMember(1), new EndProgram());
        var choice = Assert.IsType<ChoiceWait>(current.Story.Wait);
        // Unbound authored choice isolates the returned answer from the window animation.
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, choice with { Work = null }));
        var result = ExplorationDispatcher.Submit(definition, current, new ChooseDialogue(choice.Token, accept));
        Assert.Null(result.Failure);
        Assert.DoesNotContain(89, result.Snapshot.Story.Flags);
        Assert.DoesNotContain(1, result.Snapshot.Story.Flags);
        if (!accept) { Assert.Null(result.Snapshot.Story.Wait); Assert.Empty(result.Snapshot.Story.Names); return; }
        var name = Assert.IsType<NameWait>(result.Snapshot.Story.Wait);
        result = ExplorationDispatcher.Submit(definition, result.Snapshot, new NameMember(name.Token, "Shelly"));
        Assert.Null(result.Failure);
        Assert.Equal("Shelly", result.Snapshot.Story.Names["ally-1"]);
        Assert.Contains(1, result.Snapshot.Story.Flags);
        Assert.Contains(33, result.Snapshot.Story.Flags);
        Assert.DoesNotContain(89, result.Snapshot.Story.Flags);
        Assert.Equal(current.Exploration!.Party.MainSeed, result.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(current.Exploration.Party.Gold, result.Snapshot.Exploration.Party.Gold);
    }

    [Theory]
    [InlineData(0, "Nova")] [InlineData(1, "Shelly")] [InlineData(1, "")]
    public void NameIsTokenBoundValidatedAndRoundTripsAnExistingSafePoint(int member, string name)
    {
        var (definition, current) = NamingStart(new NamePartyMember(member), new EndProgram());
        var wait = Assert.IsType<NameWait>(current.Story.Wait);
        foreach (var command in new SessionCommand[] { new NameMember(new(wait.Token.Value + 1), "Valid"),
            new NameMember(wait.Token, "12345678"), new NameMember(wait.Token, "bad\n"), new Acknowledge(wait.Token) })
        {
            var invalid = ExplorationDispatcher.Submit(definition, current, command);
            Assert.NotNull(invalid.Failure); Assert.Same(current, invalid.Snapshot);
        }
        var result = ExplorationDispatcher.Submit(definition, current, new NameMember(wait.Token, name));
        Assert.Null(result.Failure);
        Assert.Equal(current.Story.Flags, result.Snapshot.Story.Flags);
        Assert.Equal(current.Exploration!.Party.Actors, result.Snapshot.Exploration!.Party.Actors);
        Assert.Equal(current.Exploration.Party.Gold, result.Snapshot.Exploration.Party.Gold);
        Assert.Equal(current.Exploration.Party.MainSeed, result.Snapshot.Exploration.Party.MainSeed);
        Assert.NotNull(ExplorationDispatcher.Submit(definition, result.Snapshot, new NameMember(wait.Token, "Again")).Failure);
        var capture = OpeningSave.Capture(definition, result.Snapshot, RuleCompositions.Sf2(), result.Snapshot.SessionId, result.Snapshot.Revision);
        Assert.Null(capture.Error);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, capture.Data!, RuleCompositions.Sf2()));
        Assert.Equal(result.Snapshot.Story.Names, restored.Session.Current.Story.Names);
    }

    [Fact]
    public void TortoiseUsesItsSourceClassWithoutInventingAWeapon()
    {
        var session = Start(change: document => document["actors"]![0]!["classRule"] = "unpromoted-tortoise");
        var actor = session.Current.Battle.Actors.Single(row => row.Actor.Value == "medic-a");
        Assert.Equal((byte)11, Sf2ClassRules.SourceClass(actor.Definition.ClassRule));
        Assert.Equal(Start().Current.Battle.GetActor(actor.Actor).Attack, actor.Attack);
    }

    [Theory]
    [InlineData(1)] [InlineData(3)]
    public void TortoiseInitializesItsGrowthWithoutAddingWeaponsOrSpells(byte level)
    {
        var curve = Array.AsReadOnly(Enumerable.Repeat(new StatGrowthFraction(0, 0), 29).ToArray());
        var growth = new BattleGrowthDefinition(11, 0,
            [new(5, 5, curve), new(0, 0, []), new(8, 8, curve), new(15, 15, curve), new(7, 7, curve)],
            [], new Dictionary<byte, SpellRef>());
        var definition = new BattleActorDefinition(new("tortoise"), BattleClassRule.UnpromotedTortoise,
            1, 5, 0, 8, 15, 7, false, 5, [],
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]), growth: growth);
        uint seed = 0x1234abcdu;
        var input = OpeningPartyInitialization.Initialize(
            new(definition, BattleFaction.Ally, 6, BattleControl.Player, null, new(1, 1)), level, ref seed);
        Assert.Equal(level, input.Progress!.Level);
        Assert.Equal(input.Progress.MaxHp, input.Hp); Assert.Equal(0, input.Mp);
        Assert.Empty(input.Progress.Spells);
        Assert.All(input.SourceLoadout!.Items, item => Assert.Equal(127, item));
        Assert.Equal(input.Progress.BaseAttack, BattleEquipment.Attack(definition, input.Progress, input.SourceLoadout));
        uint expected = 0x1234abcdu;
        for (int i = 0; i < (level - 1) * 8; i++) expected = BattleRandom.NextMain(expected, 128).After;
        Assert.Equal(expected, seed);
    }

    private static (ScenarioDefinition Definition, SessionSnapshot Current) NamingStart(params StoryInstruction[] instructions)
    {
        var (original, data) = OpeningRestoreTests.Fixture();
        var world = original.Exploration!;
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values,
            exploration: new(world.Maps.Values, [new("naming", instructions), new("declined", [new EndProgram()])],
                provenance: world.Provenance, partyFlags: world.PartyFlags, visuals: world.Visuals)) { Persistence = original.Persistence };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var current = session.Current.WithStory(session.Current.Story.Copy(new("naming", 0)));
        var result = ProgramRunner.Run(definition, current, []);
        Assert.Null(result.Failure);
        return (definition, result.Snapshot);
    }
}
