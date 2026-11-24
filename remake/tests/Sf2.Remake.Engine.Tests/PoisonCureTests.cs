using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PoisonCureTests
{
    private static readonly ActorRef Medic = new("medic-a"), Friend = new("guard-a");
    private static readonly SpellRef Detox = new("detox", 1), Detox4 = new("detox", 4);

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EnemyHealAliasCannotAdmitCureOrPublishAutomaticMovementPaymentStatusOrRng(bool poison)
    {
        var session=Start("enemy-spells",d=>
        {
            d["spells"]!.AsArray().Single(row=>row!["id"]!.GetValue<string>()=="heal")!["effect"]=
                new JsonObject { ["kind"]="cure-poison" };
            foreach(var row in d["start"]!["actors"]!.AsArray())
            {
                if(row!["actor"]!.GetValue<string>()=="mage")row["hp"]=0;
                if(row["actor"]!.GetValue<string>()=="wounded")row["status"]=poison?"poison":"none";
            }
            d["battlePoison"]="fixed-two-after-turn";d["start"]!["firstAlly"]="scout";
            d["encounters"]![0]!["outcome"]=new JsonObject { ["leader"]="scout",["defeatedGateEnemy"]="cleric" };
        });
        var before=session.Current.Battle;var cleric=EnemySpellDecisionTests.Cleric;
        var error=Assert.Throws<BattleRuleException>(()=>EnemySpellDecisionTests.Decide(before,cleric));
        Assert.True(error.Unsupported);Assert.Equal("enemy-poison-cure",error.Code);
        Assert.Equal("enemy-poison-cure",Assert.Throws<BattleRuleException>(()=>new Sf2HealingRule()
            .RequireSpell(before,cleric,new("heal",1))).Code);
        Accept(session,new Confirm());Accept(session,new ChooseAction(SessionAction.Stay));
        var stopped=Send(session,new Confirm());
        Assert.Equal(SessionStopReason.Unsupported,stopped.StopReason);Assert.Equal("enemy-poison-cure",stopped.Failure!.Code);
        Assert.Null(session.Current.BattleScene);Assert.Null(session.Current.BattleMovement);
        Assert.Equal(before.MainSeed,session.Current.Battle.MainSeed);Assert.Equal(before.ThinkingSeed,session.Current.Battle.ThinkingSeed);
        Assert.Equal(before.Gold,session.Current.Battle.Gold);
        foreach(var actor in before.Actors)
        {
            var after=session.Current.Battle.GetActor(actor.Actor);
            Assert.Equal(actor.Position,after.Position);Assert.Equal(actor.Hp,after.Hp);Assert.Equal(actor.Mp,after.Mp);
            Assert.Equal(actor.Status,after.Status);Assert.Equal(actor.LastTarget,after.LastTarget);
        }
        Assert.DoesNotContain(stopped.Observations,row=>row.Actor==cleric && row.Kind is "mp" or "status" or "movement" or "action-committed");
    }

    [Fact]
    public void PurchasedSuppliesAndDeathCreditSurviveFieldTicksTextAndConnectedBattleEntry()
    {
        var session=Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("shop-supplies")))).Session;
        Accept(session,new Interact(new("merchant")));
        ShopServiceTests.Buy(session,1,Medic);ShopServiceTests.Pay(session);ShopServiceTests.Continue(session);
        ShopServiceTests.Buy(session,3,Friend);ShopServiceTests.Pay(session);ShopServiceTests.Continue(session);
        Accept(session,new BackShop(ShopServiceTests.Wait(session).Token));
        var context=session.Current.Exploration!.Party;
        foreach(var direction in new[]{ExplorationDirection.South,ExplorationDirection.East,ExplorationDirection.East,ExplorationDirection.East,ExplorationDirection.North})
        {
            Accept(session,new Move(direction));
            for(int tick=0;tick<400 && session.Current.Mode==SessionMode.Exploration && !session.Current.CanWaitAtInput;tick++)
                Accept(session,session.Current.Story.Wait is DialogueWait dialogue ? new Acknowledge(dialogue.Token) :
                    new AdvanceSimulation(session.Current.Story.Wait?.Token));
        }
        Assert.Equal(SessionMode.Battle,session.Current.Mode);
        Assert.Equal(context.DeathCredit,session.Current.Battle.DeathCredit);
        Assert.Equal(context.Gold,session.Current.Battle.Gold);
        Assert.Equal((ushort)1,session.Current.Battle.GetActor(Medic).SourceLoadout!.Items[0]);
        Assert.Equal((ushort)3,session.Current.Battle.GetActor(Friend).SourceLoadout!.Items[0]);
        var finished=FinishBattleScenes(session,Accept(session,new Confirm()));
        Accept(session,new SelectItem(0));Accept(session,new SelectTarget(Medic));
        finished=FinishBattleScenes(session,Accept(session,new Confirm()));
        Assert.Equal(35,session.Current.Battle.GetActor(Medic).Hp);
        Assert.Equal(Friend,session.Current.Selection!.Actor);
        Accept(session,new Confirm());Accept(session,new SelectItem(0));Accept(session,new SelectTarget(Friend));
        FinishBattleScenes(session,Accept(session,new Confirm()));
        Assert.Equal(0,session.Current.Battle.GetActor(Friend).Status);
        Assert.Equal((ushort)127,session.Current.Battle.GetActor(Friend).SourceLoadout!.Items[0]);
    }
    private static GameSession Battle(bool cureItem = false, bool clean = false)
    {
        var document = Document("shop-supplies")["battle"]!.DeepClone();
        document["actors"]![0]!["items"] = new JsonArray(3, 1, 0, 127);
        document["actors"]![1]!["items"] = new JsonArray(3, 127, 127, 127);
        if (clean) document["start"]!["actors"]![1]!["status"] = "none";
        var read = Reader(document).Read();
        Assert.False(read is ScenarioReadRejected, read.ToString());
        var started = GameSession.Start(Reader(document));
        Assert.False(started is SessionStartFailed, started.ToString());
        return Assert.IsType<SessionStarted>(started).Session;
    }

    private static SessionSnapshot HigherBattle(ushort targetStatus, byte mp = 20, bool self = false)
    {
        var initial = Battle().Current.Battle;
        SpellRef rank2 = new("detox", 2), rank3 = new("detox", 3);
        var caster = initial.GetActor(Medic);
        var casterDefinition = caster.Deployment with
        {
            Definition = new(Medic, BattleClassRule.UnpromotedPriest, caster.Level, caster.MaxHp, caster.MaxMp,
                caster.BaseAttack, caster.BaseDefense, caster.Definition.Agility, false, caster.Move,
                [Detox, rank2, rank3], caster.Definition.Physical, sourceLoadout: caster.SourceLoadout)
        };
        var original = initial.Definition;
        bool withRing = !self && (targetStatus & 4) != 0;
        var friend = initial.GetActor(Friend);
        var recipient = withRing ? friend.Deployment with
        {
            Definition = new(Friend, BattleClassRule.Wizard, friend.Level, friend.MaxHp, friend.MaxMp,
                friend.BaseAttack, friend.BaseDefense, friend.Definition.Agility, false, friend.Move, [],
                friend.Definition.Physical, equipment: new(friend.BaseAttack,
                    [new(92, "Indra Staff", 1u << 17, 25, 1, 1)],
                    rings: [new(24, "Black Ring", 1u << 17, RingStat.Attack, 10, DamageOnUse: true, Cursed: true)])),
        } : friend.Deployment;
        var definition = new BattleDefinition(original.Encounter, original.Map, original.Width, original.Height, original.Terrain,
            original.Deployments.Select(row => row.Actor == Medic ? casterDefinition : row.Actor == Friend ? recipient : row),
            [original.Spells[Detox], new(rank2, 6, 0, 0, 2, CuresPoison: true), new(rank3, 10, 0, 0, 2, CuresPoison: true)],
            new(true), original.Initialization, original.Outcome, healingItems: original.HealingItems.Values, poison: true);
        if (withRing)
            friend = new BattleActorState(recipient, friend.Hp, friend.Mp, friend.Exp, friend.Position, friend.Kills, friend.Defeats,
                status: targetStatus, sourceLoadout: new([220, 408, 24, 127], [63, 63, 63, 63]));
        var actors = initial.Actors.Select(actor => actor.Actor == Medic
            ? new BattleActorState(casterDefinition, caster.Hp, mp, 0, caster.Position, 0, 0,
                status: self ? targetStatus : (ushort)0, sourceLoadout: caster.SourceLoadout)
            : actor.Actor == Friend ? friend.With(status: (ushort)(0x5000 | (self ? 0 : targetStatus)), position: new(3, 2), refreshStats: true)
                .With(status: (ushort)(0xF000 | (self ? 0 : targetStatus)))
            : actor);
        var battle = new EngineBattleState(definition, actors, 0x1234, initial.ThinkingSeed, initial.Round,
            [new(Medic, 20), new(Friend, 10)], 0, initial.Gold, initial.StartPolicy, initial.Regions,
            deathCredit: initial.DeathCredit);
        return BattleSaveFixture.At(battle, new StoryState([]));
    }

    private static SessionSnapshot AreaBattle(byte mp = 16, bool silenced = false)
    {
        var basis = AuraHealingTests.Battle();
        var caster = basis.GetActor(AuraHealingTests.Caster);
        var center = basis.GetActor(AuraHealingTests.Center);
        var medic = caster.Deployment with
        {
            Definition = new(caster.Actor, BattleClassRule.Vicar, 25, caster.MaxHp, caster.MaxMp,
                caster.BaseAttack, caster.BaseDefense, caster.BaseAgility, false, caster.Move, [Detox4])
        };
        var wizard = center.Deployment with
        {
            Definition = new(center.Actor, BattleClassRule.Wizard, 10, center.MaxHp, center.MaxMp,
                10, center.BaseDefense, center.BaseAgility, false, center.Move, [],
                equipment: new(10, [new(92, "Staff", 1u << 17, 25, 1, 1)],
                    rings: [new(24, "Black Ring", 1u << 17, RingStat.Attack, 10, Cursed: true)]))
        };
        var original = basis.Definition;
        var definition = new BattleDefinition(original.Encounter, original.Map, original.Width, original.Height, original.Terrain,
            original.Deployments.Select(row => row.Actor == caster.Actor ? medic : row.Actor == center.Actor ? wizard : row),
            [new(Detox4, 16, 0, 0, 3, HealingTargets.CureRadius, 2, CuresPoison: true)],
            new(true), original.Initialization, original.Outcome, poison: true);
        var pack = new BattleSourceLoadout([220, 408, 24, 127], [63, 63, 63, 63]);
        var actors = basis.Actors.Select(actor => actor.Actor.Value switch
        {
            "z-priest" => new(medic, caster.Hp, mp, 0, caster.Position, 0, 0, status: silenced ? (ushort)0x100 : (ushort)0),
            "b-center" => new(wizard, center.Hp, 0, 0, new(5, 3), 0, 0, status: 0x107, sourceLoadout: pack),
            "a-edge" => actor.With(position: new(7, 3), status: 2),
            "m-full" => actor.With(position: new(6, 4)),
            "n-neutral" => actor.With(position: new(5, 4), status: 2),
            "d-dead" => actor.With(position: new(5, 2), status: 2),
            "f-far" => actor.With(position: new(8, 3), status: 2),
            "enemy" => actor.With(position: new(6, 3), status: 2),
            _ => actor,
        });
        return BattleSaveFixture.At(new(definition, actors, 0x1234, basis.ThinkingSeed, basis.Round,
            [new(caster.Actor, 20), new(new("m-full"), 10)], 0, basis.Gold, basis.StartPolicy, basis.Regions), new StoryState([]));
    }

    [Fact]
    public void RankFourUsesTheSelectedCenterAndReplaysOrderedCuresAfterOnePayment()
    {
        var start = AreaBattle();
        var caster = AuraHealingTests.Caster;
        var center = AuraHealingTests.Center;
        var moved = Submit(start, new Move(ExplorationDirection.West));
        var segment = moved.BattleMovement!;
        var delivered = BattleMovementContinuation.Submit(moved, new CompletePresentation(segment.Token, segment.CompletionKind));
        Assert.Null(delivered.Failure);
        var actions = Submit(delivered.Snapshot, new Confirm());
        var choices = BattleChoices.Query(actions, Sf2.Remake.Application.Gameplay.RuleCompositions.Sf2(), null, false);
        var offer = Assert.Single(choices.Spells);
        Assert.Equal<byte?>(3, offer.MaximumRange);
        Assert.Equal(2, offer.Description!.Radius);
        Assert.Equal(SpellEffect.CurePoisonParalysisAndCurse, offer.Description.Effect);
        Assert.Contains(offer.Targets, target => target.Enabled && target.Actor == center);
        var targets = Submit(actions, new SelectSpell(Detox4));
        foreach (string invalid in new[] { "a-edge", "f-far", "n-neutral", "d-dead", "u-unplaced", "enemy" })
        {
            var refused = BattleCommandDispatcher.Submit(targets, new SelectTarget(new(invalid)));
            Assert.NotNull(refused.Failure);
            Assert.Same(targets, refused.Snapshot);
        }

        var selected = Submit(targets, new SelectTarget(center));
        Assert.Equal(center, selected.Selection!.Target);
        Assert.Equal(new MapPosition(3, 3), selected.Battle.GetActor(caster).Position);
        Assert.Same(selected.Battle, Submit(selected, new ReturnToBattleActions()).Battle);
        var committed = BattleCommandDispatcher.Submit(selected, new Confirm());
        Assert.Null(committed.Failure);
        var live = committed.Snapshot;
        Assert.Equal(new[] { "a-edge", "b-center", "m-full" }, live.BattleScene!.HealingTargets.Select(row => row.Value));
        Assert.False(live.BattleScene.IsAura);
        Assert.Equal(new MapPosition(2, 3), live.Battle.GetActor(caster).Position);
        Assert.Equal(16, live.Battle.GetActor(caster).Mp);
        Assert.Equal(0x107, live.Battle.GetActor(center).Status);

        var draws = committed.Observations.Where(row => row.RandomRange is not null).ToArray();
        Assert.Equal(new ushort?[] { 8, 16, 16 }, draws.Select(row => row.RandomRange));
        Assert.Equal(new long?[] { 5, 10, 10 }, committed.Observations
            .Where(row => row.Kind == "healing-exp-contribution").Select(row => row.After));
        var notices = new List<(string Actor, string Notice)>();
        var events = new List<SessionObservation>();
        for (int step = 0; live.BattleScene is { } scene; step++)
        {
            Assert.True(step < 40);
            if (scene.Phase is BattleScenePhase.ResultMessage or BattleScenePhase.CureStunMessage or BattleScenePhase.CureCurseMessage)
            {
                Assert.Equal(scene.Target, scene.DisplayedAlly);
                notices.Add((scene.Target!.Value.Value, scene.ShowsStunCure ? "stun" : scene.ShowsCurseCure ? "curse" :
                    scene.ReactionKind == "Ineffective" ? "ineffective" : "poison"));
            }
            var next = BattleSceneContinuation.Submit(live, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(next.Failure);
            events.AddRange(next.Observations);
            live = next.Snapshot;
        }
        Assert.Equal(new[] { ("a-edge", "poison"), ("b-center", "poison"), ("b-center", "stun"),
            ("b-center", "curse"), ("m-full", "ineffective") }, notices);
        Assert.Single(events, row => row.Kind == "mp");
        Assert.Single(events, row => row.Kind == "exp");
        Assert.Equal(0, live.Battle.GetActor(caster).Mp);
        int award = 10 + (draws[1].RandomValue == 0 ? 1 : 0) - (draws[2].RandomValue == 0 ? 1 : 0);
        Assert.Equal<byte?>((byte)award, live.Battle.GetActor(caster).Exp);
        var cured = live.Battle.GetActor(center);
        Assert.Equal(0x100, cured.Status);
        Assert.Equal(35, cured.Attack);
        Assert.Equal(new ushort[] { 220, 280, 24, 127 }, cured.SourceLoadout!.Items);
        Assert.All(start.Battle.Actors, actor => Assert.Equal(actor.Hp, live.Battle.GetActor(actor.Actor).Hp));
        Assert.Equal(0, live.Battle.GetActor(new("a-edge")).Status);
        Assert.All(new[] { "n-neutral", "d-dead", "f-far", "enemy" },
            id => Assert.Equal(2, live.Battle.GetActor(new(id)).Status));
        Assert.Equal(start.Battle.ThinkingSeed, live.Battle.ThinkingSeed);
        Assert.Equal(new ActorRef("m-full"), live.Selection!.Actor);
    }

    [Fact]
    public void RankFourCureExperienceCanAccumulatePastTheHpRecoveryLimit()
    {
        var start = AreaBattle();
        var battle = start.Battle.With(actors: start.Battle.Actors.Select(actor => actor.Actor.Value switch
        {
            "z-priest" or "m-full" => actor.With(status: 2),
            "f-far" => actor.With(position: new(5, 1)),
            "d-dead" => actor.With(hp: 10),
            _ => actor,
        }));
        var actions = Submit(BattleSaveFixture.At(battle, start.Story), new Confirm());
        var selected = Submit(Submit(actions, new SelectSpell(Detox4)), new SelectTarget(AuraHealingTests.Center));
        var cast = BattleCommandDispatcher.Submit(selected, new Confirm());
        Assert.Null(cast.Failure);
        Assert.Equal(new long?[] { 5, 10, 15, 20, 25, 30 }, cast.Observations
            .Where(row => row.Kind == "healing-exp-contribution").Select(row => row.After));
        Assert.InRange(cast.Snapshot.BattleScene!.Action.Reward!.Amount, 29, 31);
    }

    [Fact]
    public void RankFourRefusesLowMpAndSilencePaysWithoutAnyTargetWork()
    {
        var poor = Submit(AreaBattle(mp: 15), new Confirm());
        var refused = BattleCommandDispatcher.Submit(poor, new SelectSpell(Detox4));
        Assert.Equal("insufficient-mp", refused.Failure!.Code);
        Assert.Same(poor, refused.Snapshot);
        var state = Submit(AreaBattle(silenced: true), new Confirm());
        var selected = Submit(Submit(state, new SelectSpell(Detox4)), new SelectTarget(AuraHealingTests.Center));
        var cast = BattleCommandDispatcher.Submit(selected, new Confirm());
        Assert.Null(cast.Failure);
        var live = cast.Snapshot;
        var events = cast.Observations.ToList();
        int suppressions = 0;
        for (int step = 0; live.BattleScene is { } scene; step++)
        {
            Assert.True(step < 20);
            Assert.False(scene.HasReaction);
            if (scene.Phase == BattleScenePhase.SilencedCastMessage) suppressions++;
            var next = BattleSceneContinuation.Submit(live, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(next.Failure);
            events.AddRange(next.Observations);
            live = next.Snapshot;
        }
        Assert.Equal(1, suppressions);
        Assert.Single(events, row => row.Kind == "mp");
        Assert.DoesNotContain(events, row => row.Kind == "exp");
        var draw = Assert.Single(events, row => row.RandomRange is not null);
        Assert.Equal("rng-silence-expiry", draw.Kind);
        Assert.Equal((long)state.Battle.MainSeed, draw.Before);
        Assert.Equal(draw.After, (long)live.Battle.MainSeed);
        Assert.Equal(state.Battle.ThinkingSeed, live.Battle.ThinkingSeed);
        Assert.Equal(0, live.Battle.GetActor(AuraHealingTests.Caster).Mp);
        var center = AuraHealingTests.Center;
        Assert.Equal(0x107, live.Battle.GetActor(center).Status);
        Assert.Equal(state.Battle.GetActor(center).SourceLoadout, live.Battle.GetActor(center).SourceLoadout);
    }

    private static SessionSnapshot Submit(SessionSnapshot snapshot, SessionCommand command)
    {
        var result = BattleCommandDispatcher.Submit(snapshot, command);
        Assert.Null(result.Failure);
        return result.Snapshot;
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 1)]
    [InlineData(3, 0)]
    [InlineData(2, 2)]
    [InlineData(2, 4)]
    [InlineData(2, 7)]
    [InlineData(3, 4)]
    [InlineData(3, 7)]
    public void HigherBattleDetoxKeepsTheSameSideAwardAndRefreshesOnlyOnARealCure(byte rank, ushort status)
    {
        var start = HigherBattle(status);
        var selected = start;
        var spell = new SpellRef("detox", rank);
        foreach (SessionCommand command in new SessionCommand[] { new Confirm(), new SelectSpell(spell), new SelectTarget(Friend) })
        {
            var selection = BattleCommandDispatcher.Submit(selected, command);
            Assert.Null(selection.Failure);
            selected = selection.Snapshot;
        }
        Assert.Same(start.Battle, BattleCommandDispatcher.Submit(selected, new ReturnToBattleActions()).Snapshot.Battle);
        var committed = BattleCommandDispatcher.Submit(selected, new Confirm());
        Assert.Null(committed.Failure);
        var rng = committed.Observations.Where(row => row.RandomRange.HasValue).ToArray();
        int cleared = status & (rank == 2 ? 3 : 7);
        Assert.Equal(cleared == 0 ? new ushort?[] { 8, 16, 16 } : [16, 16], rng.Select(row => row.RandomRange));
        var live = committed.Snapshot;
        Assert.Equal(20, live.Battle.GetActor(Medic).Mp);
        Assert.Equal((ushort?)(0x5000 | status & 1), live.Battle.GetActor(Friend).LastRefreshBits);
        Assert.Equal(start.Battle.GetActor(Friend).SourceLoadout, live.Battle.GetActor(Friend).SourceLoadout);
        var notices = new List<string>();
        for (int steps = 0; live.BattleScene is { } scene; steps++)
        {
            Assert.True(steps < 30);
            if (scene.Phase is BattleScenePhase.ResultMessage or BattleScenePhase.CureStunMessage or BattleScenePhase.CureCurseMessage)
            {
                Assert.Equal(rank == 2 ? 14 : 10, live.Battle.GetActor(Medic).Mp);
                notices.Add(scene.ShowsStunCure ? "stun" : scene.ShowsCurseCure ? "curse" : cleared == 0 ? "ineffective" : "poison");
            }
            var advanced = BattleSceneContinuation.Submit(live, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(advanced.Failure);
            live = advanced.Snapshot;
        }
        var target = live.Battle.GetActor(Friend);
        Assert.Equal(0xF000 | (status & ~cleared), target.Status);
        Assert.Equal(start.Battle.GetActor(Friend).Hp, target.Hp);
        Assert.Equal(cleared == 0 ? (ushort?)0x5000 : null, target.LastRefreshBits);
        int gear = (status & 4) == 0 ? 0 : (cleared & 4) == 0 ? 35 : 25;
        Assert.Equal((cleared == 0 ? 12 : 15) + gear, target.Attack);
        Assert.Equal(cleared == 0 ? 45 : 55, target.Agility);
        if ((status & 4) != 0)
            Assert.Equal(new ushort[] { 220, (cleared & 4) == 0 ? (ushort)408 : (ushort)280, 24, 127 }, target.SourceLoadout!.Items);
        string[] expectedNotices = cleared == 0 ? ["ineffective"] :
            new[] { (2, "poison"), (1, "stun"), (4, "curse") }.Where(row => (cleared & row.Item1) != 0).Select(row => row.Item2).ToArray();
        Assert.Equal(expectedNotices, notices);
        Assert.Equal(start.Battle.ItemCustody, live.Battle.ItemCustody);
        int exp = cleared == 0 ? 1 : 5 + (rng[^2].RandomValue == 0 ? 1 : 0) - (rng[^1].RandomValue == 0 ? 1 : 0);
        Assert.Equal((byte?)exp, live.Battle.GetActor(Medic).Exp);
        Assert.Equal(Friend, live.Selection!.Actor);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(3, false)]
    public void BattleSelfDetoxKeepsItsPaymentAndSilenceSuppressesOnlyTheEffect(byte rank, bool silenced)
    {
        var live = HigherBattle(silenced ? (ushort)0x102 : rank == 3 ? (ushort)4 : (ushort)2, self: true);
        foreach (SessionCommand command in new SessionCommand[]
            { new Confirm(), new SelectSpell(new("detox", rank)), new SelectTarget(Medic), new Confirm() })
        {
            var result = BattleCommandDispatcher.Submit(live, command);
            Assert.Null(result.Failure);
            live = result.Snapshot;
        }
        var terminal = silenced ? BattleScenePhase.SilencedCastMessage : BattleScenePhase.ResultMessage;
        for (int step = 0; live.BattleScene is { } scene && scene.Phase != terminal; step++)
        {
            Assert.True(step < 20);
            var advanced = BattleSceneContinuation.Submit(live, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token)
                : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(advanced.Failure);
            live = advanced.Snapshot;
        }
        Assert.Equal(terminal, live.BattleScene!.Phase);
        Assert.Equal(rank == 3 ? 10 : 14, live.Battle.GetActor(Medic).Mp);
        Assert.Equal(silenced ? 0x102 : 0, live.Battle.GetActor(Medic).Status);
    }

    [Fact]
    public void HigherBattleDetoxRejectsInsufficientMpAndTargetsOutsideItsRange()
    {
        var rule = new Sf2HealingRule();
        SpellRef rank3 = new("detox", 3);
        var poor = HigherBattle(0, mp: 9).Battle;
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => rule.RequireSpell(poor, Medic, rank3)).Code);
        var good = HigherBattle(0).Battle;
        Assert.Equal("target-range", Assert.Throws<BattleRuleException>(() => rule.RequireTarget(good,
            Medic, new MapPosition(1, 3), rule.RequireSpell(good, Medic, rank3), Friend)).Code);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(false, true)] [InlineData(true, true)]
    public void SpellAndItemUseTheSameFiniteCureAndOnlyPublishStatusAtReaction(bool item, bool clean)
    {
        var session = Battle(clean: clean); var before = session.Current.Battle;
        var action = item ? new BattleActionRef(BattleActionKind.Item, ItemSlot: 0) : new BattleActionRef(BattleActionKind.Healing, Detox);
        var rule = item ? (IBattleActionRule)new Sf2ItemAction() : session.Rules.Action(action);
        var resolution = BattleActionRules.Prepare(rule, before, Medic, before.GetActor(Medic).Position!, action, Friend, session.Rules.Progression);
        Assert.Equal(before.GetActor(Friend).Status, resolution.Prepared.GetActor(Friend).Status);
        Assert.Equal(before.GetActor(Medic).Mp, resolution.Prepared.GetActor(Medic).Mp);
        Assert.Equal(item ? (ushort)1 : (ushort)3, resolution.Prepared.GetActor(Medic).SourceLoadout!.Items[0]);
        var paid = item ? resolution.Prepared : resolution.ApplySpellCost(resolution.Prepared);
        Assert.Equal(item ? 20 : 17, paid.GetActor(Medic).Mp);
        var reaction = Assert.Single(resolution.Reactions);
        Assert.Equal(clean ? BattleReactionKind.Ineffective : BattleReactionKind.Status, reaction.Kind);
        Assert.Equal(0, reaction.Amount); Assert.Equal(before.GetActor(Friend).Hp, reaction.HpAfter);
        Assert.Equal(0, resolution.ApplyReaction(paid, reaction).GetActor(Friend).Status);
        Assert.Equal(clean ? 1 : 0, resolution.ConstructionEffects.Count(row => row.Kind == "rng-cure-ineffective"));
        Assert.InRange(resolution.Reward!.Amount, 1, clean ? 2 : 6);
        Assert.Equal(before.ThinkingSeed, resolution.Prepared.ThinkingSeed);
    }

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(0x302)] [InlineData(0x3c2)] [InlineData(0)] [InlineData(1)] [InlineData(0x300)]
    public void CureClearsOnlyPoisonAndPreservesOtherPackedFields(ushort status)
    {
        var target = Battle().Current.Battle.GetActor(Friend).With(status: status);
        var reaction = PoisonCureRules.Reaction(Medic, target, "heal");
        Assert.Equal((status & 2) != 0 ? BattleReactionKind.Status : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal((status & 2) != 0 ? (ushort?)(status & ~2) : null, reaction.StatusWrite);
        Assert.Equal(target.Hp, reaction.HpAfter); Assert.Equal(target.Mp, reaction.MpAfter);
    }

    [Theory]
    [InlineData((ushort)3, true)]
    [InlineData((ushort)1, false)]
    public void RetailBattlePowderPaysForOnlyThePoisonEffect(ushort initialStatus, bool effective)
    {
        var document = Document("shop-supplies")["battle"]!.DeepClone();
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var existing = Assert.Single(admitted.Definition.Encounters.Values);
        var source = new PrivateBattleDefinitions(null!, [], [],
            [new(5, "FAIRY_POWDER", "Fairy Powder", "", "NONE", [], 0, 1, 100,
                "CONSUMABLE", "POWDER", "POWDER", 1, [new("NONE", 0)])],
            [new(73, 33, "POWDER", "Powder", "POWDER", 1, 0, "NONE", "TYPE_HEAL|TARGET_TEAMMATES",
                0, 1, 0, 0)], [], []);
        var battle = new BattleDefinition(existing.Encounter, existing.Map, existing.Width, existing.Height, existing.Terrain,
            existing.Deployments, existing.Spells.Values, existing.Rewards, outcome: existing.Outcome,
            healingItems: PrivateBattleActionBindings.HealingItems(source), poison: existing.Poison);
        var input = admitted.Start.WithResources(admitted.Start.Actors.Select(actor => actor.Actor == Friend
            ? actor with { Status = initialStatus }
            : actor.Actor == Medic ? actor with { SourceLoadout = new([3, 5, 0, 127], [63, 63, 63, 63]) } : actor),
            admitted.Start.Gold);
        var definition = new ScenarioDefinition(admitted.Definition.Package, [battle], battleStory: admitted.Definition.BattleStory);
        var started = GameSession.Start(definition, input);
        Assert.True(started is SessionStarted, started.ToString());
        var session = ((SessionStarted)started).Session;
        var before = session.Current.Battle;
        Assert.Equal(Medic, session.Current.Selection!.Actor);

        Accept(session, new Confirm());
        Accept(session, new SelectItem(1));
        Assert.NotNull(Send(session, new SelectTarget(new("dummy-a"))).Failure);
        Accept(session, new Cancel());
        Accept(session, new Cancel());
        Assert.Same(before, session.Current.Battle);

        Accept(session, new Confirm());
        Accept(session, new SelectItem(1));
        Accept(session, new SelectTarget(Friend));
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        var after = session.Current.Battle;
        var target = after.GetActor(Friend);
        var holder = after.GetActor(Medic);
        Assert.Equal(before.GetActor(Friend).Hp, target.Hp);
        Assert.Equal(before.GetActor(Friend).Mp, target.Mp);
        Assert.Equal(before.GetActor(Medic).Mp, holder.Mp);
        Assert.Equal(new ushort[] { 3, 0, 127, 127 }, holder.SourceLoadout!.Items);
        var cures = result.Observations.Where(row => row.Kind == "status" && row.Actor == Friend);
        if (effective) Assert.Equal(1, Assert.Single(cures).After);
        else Assert.Empty(cures);
        var stunTurn = Assert.Single(result.Observations, row => row.Kind == "stun-status" && row.Actor == Friend);
        Assert.Equal(1, stunTurn.Before);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void SeedUsesTwentyHpAndConsumesSelectedSlotWithoutMpCost()
    {
        var session = Battle(); var battle = session.Current.Battle; var actor = battle.GetActor(Medic);
        var action = new BattleActionRef(BattleActionKind.Item, ItemSlot: 1);
        var resolution = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Medic, actor.Position!, action, Medic, session.Rules.Progression);
        var reaction = Assert.Single(resolution.Reactions); Assert.Equal(20, reaction.Amount);
        Assert.Equal(actor.Hp, resolution.Prepared.GetActor(Medic).Hp);
        var applied = resolution.ApplyReaction(resolution.Prepared, reaction).GetActor(Medic);
        Assert.Equal(actor.Hp + 20, applied.Hp); Assert.Equal(actor.Mp, applied.Mp);
        Assert.Equal(new ushort[] {3,0,127,127}, applied.SourceLoadout!.Items);
    }

    [Fact]
    public void CancellationInvalidTargetAndInsufficientMpCommitNoMovementPaymentOrRng()
    {
        var session = Battle(); var before = session.Current.Battle;
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(Detox)); Accept(session, new Cancel()); Accept(session, new Cancel());
        Assert.Same(before, session.Current.Battle);
        var action = new BattleActionRef(BattleActionKind.Healing, Detox);
        Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(session.Rules.Action(action), before, Medic,
            before.GetActor(Medic).Position!, action, new("dummy-a"), session.Rules.Progression));
        var poor = before.With(actors: before.Actors.Select(a => a.Actor == Medic ? a.With(mp: 2) : a));
        Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(session.Rules.Action(action), poor, Medic,
            poor.GetActor(Medic).Position!, action, Friend, session.Rules.Progression));
        Assert.Same(before, session.Current.Battle);
    }

    [Fact]
    public void SceneCureHasNoHealingFairyAndPaysBeforeTheResultThenReturnsControl()
    {
        var session = Battle(); var mp = session.Current.Battle.GetActor(Medic).Mp;
        Accept(session,new Confirm()); Accept(session,new SelectSpell(Detox));
        Accept(session,new SelectTarget(Friend)); var result = Accept(session,new Confirm());
        Assert.NotNull(session.Current.BattleScene); Assert.Null(session.Current.BattleScene!.Healing);
        var finished=FinishBattleScenes(session,result);
        Assert.Equal(mp-3,session.Current.Battle.GetActor(Medic).Mp); Assert.Equal(0,session.Current.Battle.GetActor(Friend).Status);
        Assert.Single(finished.Observations,row=>row.Kind=="status" && row.Actor==Friend);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void ImmutableBattleCopiesKeepBothSupplyEffects()
    {
        var before=Battle().Current.Battle.Definition;
        var copy=before.WithDeployments(before.Deployments);
        Assert.True(copy.Spells[Detox].CuresPoison); Assert.True(copy.HealingItems[3].CuresPoison);
        Assert.Equal(20,copy.HealingItems[1].Power); Assert.False(copy.HealingItems[0].CuresPoison);
    }

    [Fact]
    public void AuthoredConnectedShopPackageAdmitsItsActualStockAndBattleConsumers()
    {
        var read=Reader(Document("shop-supplies")).Read(); Assert.False(read is ScenarioReadRejected,read.ToString());
        var admitted=Assert.IsType<ExplorationReadAccepted>(read);
        Assert.True(admitted.Definition.Encounters.Values.Single().Spells[Detox].CuresPoison);
        Assert.True(admitted.Definition.Encounters.Values.Single().Poison);
        Assert.Contains(admitted.Definition.Exploration!.Programs["shop-caller"].Instructions,
            op=>op is OpenShop {Definition.Stock.Count:3});
    }
}
