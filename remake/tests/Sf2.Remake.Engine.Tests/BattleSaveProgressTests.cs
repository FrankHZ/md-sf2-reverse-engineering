using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveProgressTests
{
    [Fact]
    public void PromotedEnemyOrderSurvivesRestoreAndLaterAiDecision()
    {
        var (definition, initial) = BattleSaveFixture.Create();
        var battle = initial.Battle.With(actors: initial.Battle.Actors.Select(a => a.Actor.Value == "enemy"
            ? a.With(activationWord: 0x2061, primaryOrder: 129, secondaryOrder: 0) : a));
        var current = BattleSaveFixture.Stay(BattleSaveFixture.At(battle, initial.Story));
        Assert.Equal((byte)0, current.Battle.GetActor(new("enemy")).PrimaryOrder);
        Assert.Equal((byte)255, current.Battle.GetActor(new("enemy")).SecondaryOrder);
        var saved = BattleSaveFixture.Capture(definition, current);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, RuleCompositions.Sf2())).Session.Current;
        var control = BattleSaveFixture.Stay(current); var next = BattleSaveFixture.Stay(restored);
        BattleSaveFixture.EqualBattle(control.Battle, next.Battle);
        Assert.Equal((byte)0, next.Battle.GetActor(new("enemy")).PrimaryOrder);
        Assert.Equal((byte)255, next.Battle.GetActor(new("enemy")).SecondaryOrder);
    }

    [Fact]
    public void EquipmentAndDiscardRemainCommittedAfterCancelAndRestore()
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var old = initial.Battle; var d = old.Definition; var hero = old.Actors[0];
        var deployment = hero.Deployment with { Definition = hero.Definition.WithEquipment(new(8,
            [new(35, "Sword", 1, 5, 1, 1)])) };
        var loadout = new BattleSourceLoadout([163, 61, 127, 127], [63, 63, 63, 63]);
        hero = new(deployment, hero.Hp, hero.Mp, hero.Exp, hero.Position, hero.Kills, hero.Defeats,
            progress: hero.Progress! with { SourceLoadout = loadout }, sourceLoadout: loadout, activationWord: 0);
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain,
            d.Deployments.Select(p => p.Actor == hero.Actor ? deployment : p), [], d.Rewards, d.Initialization, d.Outcome,
            discard: new([new(35, "Sword", false, false), new(61, "Gift", true, false)]));
        var definition = Replace(basis, encounter);
        var battle = new EngineBattleState(encounter, old.Actors.Select(a => a.Actor == hero.Actor ? hero : a), old.MainSeed,
            old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold, old.StartPolicy, old.Regions,
            itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var current = BattleSaveFixture.At(battle, initial.Story);
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, BattleSaveFixture.Capture(definition, current), RuleCompositions.Sf2())).Session;
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new BeginEquipment(), new SelectEquipment(4), new Confirm(), new Cancel() })
            EngineTestContent.Accept(session, command);
        Assert.True(session.QueryBattleSave().Allowed); Assert.Equal(14, session.Current.Battle.GetActor(hero.Actor).Attack);
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new BeginDiscard(), new SelectDiscardItem(1), new Confirm(), new Confirm() })
            EngineTestContent.Accept(session, command);
        Assert.False(session.QueryBattleSave().Allowed);
        var discard = session.Current.BattleDiscard!;
        EngineTestContent.Accept(session, new CompleteTextReveal(discard.Token)); EngineTestContent.Accept(session, new Acknowledge(discard.Token));
        EngineTestContent.Accept(session, new Cancel());
        var saved = session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(saved.Error);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved.Data!, session.Rules)).Session.Current;
        Assert.Equal(new ushort[] { 35, 127, 127, 127 }, restored.Battle.GetActor(hero.Actor).SourceLoadout!.Items);
        Assert.Equal((byte)1, restored.Battle.ItemCustody!.Deals[61]); Assert.Equal(14, restored.Battle.GetActor(hero.Actor).Attack);
        Assert.Equal(old.Queue, restored.Battle.Queue); Assert.Equal(old.Cursor, restored.Battle.Cursor);
        Assert.Equal(old.MainSeed, restored.Battle.MainSeed); Assert.Equal(old.ThinkingSeed, restored.Battle.ThinkingSeed);
    }

    [Fact]
    public void AnotherPlayerAndDeadQueueEntryUseCurrentOrderWithoutRegeneration()
    {
        var (basis, initial) = BattleSaveFixture.Create(0, uint.MaxValue);
        var b = initial.Battle; var d = b.Definition;
        var input = initial.Story.FieldPlayer!.RetainedAllies![0];
        var deployment = basis.Encounters["opening"].Deployments.Single(a => a.Actor == input.Actor);
        var actor = new BattleActorState(deployment, 12, 2, input.Exp, new(6, 5), input.Kills, input.Defeats,
            progress: input.Progress, sourceLoadout: input.SourceLoadout, activationWord: 0);
        var encounter = d.WithDeployments(d.Deployments.Append(deployment)); var definition = Replace(basis, encounter);
        var battle = new EngineBattleState(encounter, b.Actors.Append(actor).OrderBy(a => a.ProcessingOrder), b.MainSeed, b.ThinkingSeed, 19,
            [new(new("enemy"), 31), new(actor.Actor, 30), new(new("hidden"), 29), new(new("ally-0"), 20), new(null, 255)],
            1, b.Gold, b.StartPolicy, b.Regions, itemCustody: b.ItemCustody);
        var flags = initial.Story.Flags.Append(33).ToArray();
        var story = initial.Story.Copy(null, flags: flags, partyLists: MapPartyMembership.Rebuild(flags, definition.Exploration!.PartyFlags!),
            fieldPlayer: initial.Story.FieldPlayer with { RetainedAllies = [] });
        var current = BattleSaveFixture.At(battle, story); var saved = BattleSaveFixture.Capture(definition, current);
        Assert.Null(saved.Battle.DeathCredit); // Unknown remains distinct from known-no-ally.
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, RuleCompositions.Sf2())).Session.Current;
        Assert.Equal(actor.Actor, restored.Selection!.Actor); Assert.Equal(19, restored.Battle.Round);
        var control = BattleSaveFixture.Stay(current); var next = BattleSaveFixture.Stay(restored);
        BattleSaveFixture.EqualBattle(control.Battle, next.Battle);
        Assert.Equal(new ActorRef("ally-0"), next.Selection!.Actor); Assert.Equal(3, next.Battle.Cursor);
        Assert.Equal(0u, next.Battle.MainSeed); Assert.Equal(uint.MaxValue, next.Battle.ThinkingSeed);
    }

    [Fact]
    public void CommittedHealingAndItemPaymentAreNotReplayedOnRestore()
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var old = initial.Battle; var d = old.Definition; var hero = old.Actors[0];
        var spell = new SpellRef("heal", 1);
        var growth = hero.Definition.Growth! with { Spells = [new(1, 0)], SpellDefinitions = new Dictionary<byte, SpellRef> { [0] = spell } };
        var h = hero.Definition;
        var healer = new BattleActorDefinition(h.Actor, BattleClassRule.UnpromotedPriest, h.Level, h.MaxHp, h.MaxMp,
            h.Attack, h.Defense, h.Agility, h.ExtraRoundAction, h.Move, [spell], h.Physical,
            sourceLoadout: h.SourceLoadout, growth: growth);
        var deployment = hero.Deployment with { Definition = healer };
        var loadout = new BattleSourceLoadout([0, 127, 127, 127], [0, 63, 63, 63]);
        hero = new(deployment, 7, 8, 38, hero.Position, 4, 2, progress: hero.Progress! with
            { MaxHp = 40, Spells = [spell], SourceLoadout = loadout }, sourceLoadout: loadout, activationWord: 0);
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain,
            d.Deployments.Select(p => p.Actor == hero.Actor ? deployment : p), [new(spell, 3, 5, 0, 1)],
            d.Rewards, d.Initialization, d.Outcome, [new(0, "Herb", 5, 0, 1)]);
        var definition = Replace(basis, encounter);
        var battle = new EngineBattleState(encounter, old.Actors.Select(a => a.Actor == hero.Actor ? hero : a), old.MainSeed, old.ThinkingSeed,
            old.Round, [new(hero.Actor, 30), new(hero.Actor, 25), new(hero.Actor, 20), new(null, 255)], 0, old.Gold,
            old.StartPolicy, old.Regions, itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var current = BattleSaveFixture.At(battle, initial.Story); var rules = RuleCompositions.Sf2();
        foreach (var selection in new SessionCommand[] { new SelectSpell(spell), new SelectItem(0) })
        {
            foreach (var command in new SessionCommand[] { new Confirm(), selection, new SelectTarget(hero.Actor), new Confirm() })
            { var result = BattleCommandDispatcher.Submit(current, command, rules); Assert.Null(result.Failure); current = result.Snapshot; }
            Assert.False(BattleSave.Query(definition, current, rules).Allowed);
            while (current.BattleScene is { } scene)
            {
                SessionCommand command = scene.Healing is { LogicalComplete: false, AtTimedInput: false } ? new AdvanceSimulation(scene.Token) :
                    scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind);
                var result = BattleSceneContinuation.Submit(current, command, rules); Assert.Null(result.Failure); current = result.Snapshot;
            }
        }
        Assert.Equal(5, current.Battle.GetActor(hero.Actor).Mp);
        Assert.Equal(127, current.Battle.GetActor(hero.Actor).SourceLoadout!.Items[0]);
        Assert.True(current.Battle.GetActor(hero.Actor).Hp > hero.Hp);
        var saved = BattleSaveFixture.Capture(definition, current);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, rules));
        Assert.Empty(restored.Result.Observations); BattleSaveFixture.EqualBattle(current.Battle, restored.Session.Current.Battle);
        BattleSaveFixture.EqualBattle(BattleSaveFixture.Stay(current).Battle, BattleSaveFixture.Stay(restored.Session.Current).Battle);
    }

    [Fact]
    public void CompletedKillDropAndExperienceSurviveTwoIndependentRestoresWithoutReaward()
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var d = initial.Battle.Definition;
        var drops = new EnemyItemDropDefinition(1, new Dictionary<ActorRef, byte> { [new("enemy")] = 0 },
            [new(1, 0, 61, 24)], [new(61, "Gift", true)]);
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments, [],
            d.Rewards, d.Initialization, new(new("ally-0"), new("hidden")), itemDrops: drops);
        var definition = Replace(basis, encounter);
        var actors = initial.Battle.Actors.Select(a => a.Actor.Value switch
        {
            "enemy" => a.With(hp: 1, position: new(5, 5), sourceLoadout: new([61, 127, 127, 127], [63, 63, 63, 63])),
            "hidden" => a.With(hp: 10, initialized: true, position: new(9, 9), attack: 5), _ => a,
        });
        var b = initial.Battle;
        var battle = new EngineBattleState(encounter, actors, b.MainSeed, b.ThinkingSeed, b.Round,
            [new(new("ally-0"), 25), new(new("ally-0"), 20), new(null, 255)], 0, b.Gold, b.StartPolicy,
            b.Regions, itemCustody: b.ItemCustody, deathCredit: b.DeathCredit);
        var current = BattleSaveFixture.At(battle, initial.Story);
        var rules = RuleCompositions.Sf2();
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new ChooseAction(SessionAction.PhysicalAttack), new SelectTarget(new("enemy")), new Confirm() })
        {
            var action = BattleCommandDispatcher.Submit(current, command, rules); Assert.Null(action.Failure); current = action.Snapshot;
        }
        Assert.False(BattleSave.Query(definition, current, rules).Allowed);
        while (current.BattleScene is { } scene)
        {
            var result = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind), rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        Assert.Equal(0, current.Battle.GetActor(new("enemy")).Hp);
        Assert.Equal((ushort)5, current.Battle.GetActor(new("ally-0")).Kills);
        Assert.True(current.Battle.GetActor(new("ally-0")).Exp > 38);
        Assert.Equal(4333u, current.Battle.Gold);
        Assert.Contains((byte)24, current.Battle.ItemCustody!.OnceFlags);
        Assert.Equal(61, current.Battle.GetActor(new("ally-0")).SourceLoadout!.Items[0]);
        var saved = BattleSaveFixture.Capture(definition, current);
        foreach (int repeat in new[] { 1, 2 })
        {
            var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, rules));
            Assert.Empty(restored.Result.Observations); Assert.Null(restored.Session.Current.BattleScene);
            BattleSaveFixture.EqualBattle(current.Battle, restored.Session.Current.Battle);
            var after = BattleSaveFixture.Stay(restored.Session.Current);
            var control = BattleSaveFixture.Stay(current);
            BattleSaveFixture.EqualBattle(control.Battle, after.Battle);
            Assert.Equal(4333u, after.Battle.Gold); Assert.Equal((ushort)5, after.Battle.GetActor(new("ally-0")).Kills);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void NoSceneTurnPreservesDeathCreditForLaterPoisonAccounting(bool allyCredit)
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var d = initial.Battle.Definition;
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments, [],
            d.Rewards, d.Initialization, new(new("ally-0"), new("hidden")), poison: true);
        var definition = Replace(basis, encounter); var b = initial.Battle;
        var actors = b.Actors.Select(a => a.Actor.Value switch
        {
            "enemy" => a.With(hp: 1, status: 2),
            "hidden" => a.With(hp: 10, initialized: true, position: new(9, 9), attack: 5), _ => a,
        });
        var battle = new EngineBattleState(encounter, actors, b.MainSeed, b.ThinkingSeed, b.Round,
            [new(new("ally-0"), 30), new(new("ally-0"), 25), new(new("enemy"), 20), new(new("ally-0"), 18), new(null, 255)],
            0, b.Gold, b.StartPolicy, b.Regions, itemCustody: b.ItemCustody, deathCredit: new(allyCredit ? new ActorRef("ally-0") : null));
        var current = BattleSaveFixture.Stay(BattleSaveFixture.At(battle, initial.Story));
        Assert.Equal(battle.DeathCredit, current.Battle.DeathCredit); Assert.Equal(1, current.Battle.Cursor);
        var saved = BattleSaveFixture.Capture(definition, current);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, RuleCompositions.Sf2())).Session.Current;
        var control = BattleSaveFixture.Stay(current); var resumed = BattleSaveFixture.Stay(restored);
        BattleSaveFixture.EqualBattle(control.Battle, resumed.Battle);
        Assert.Equal(0, resumed.Battle.GetActor(new("enemy")).Hp);
        Assert.Equal((ushort)(allyCredit ? 5 : 4), resumed.Battle.GetActor(new("ally-0")).Kills);
    }

    [Fact]
    public void VisualAliasesRetainSharedAndOldPhysicalSlotsWithoutLogicalReprojection()
    {
        var (definition, current) = BattleSaveFixture.Create();
        var visuals = new BattleVisualState(new Dictionary<ActorRef, int> { [new("ally-0")] = 1, [new("enemy")] = 1 },
            [new(0, new("ally-0"), new(2, 2), 1), new(1, new("enemy"), new(7, 7), 2)]);
        current = BattleSaveFixture.At(current.Battle.With(visuals: visuals), current.Story.Copy(null, flags: current.Story.Flags.Append(900)));
        var saved = BattleSaveFixture.Capture(definition, current);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, RuleCompositions.Sf2())).Session.Current;
        Assert.Equal(visuals.Aliases, restored.Battle.Visuals!.Aliases); Assert.Equal(visuals.Visuals, restored.Battle.Visuals.Visuals);
        Assert.Equal(new MapPosition(7, 7), BattleStoryOperations.Visual(restored, new("ally-0")).Position);
        Assert.Equal(new MapPosition(2, 2), restored.Battle.Visuals.Visuals[0].Position);
        Assert.Contains(900, restored.Story.Flags);
        Assert.Equal(current.Battle.GetActor(new("ally-0")).Position, restored.Battle.GetActor(new("ally-0")).Position);
    }

    internal static ScenarioDefinition Replace(ScenarioDefinition basis, BattleDefinition encounter) =>
        new(basis.Package, basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
}
