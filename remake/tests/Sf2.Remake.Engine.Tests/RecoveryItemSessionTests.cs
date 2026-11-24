using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.AuraHealingTests;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class RecoveryItemSessionTests
{
    private static readonly HealingItemDefinition Water = new(6, "Healing Water", 255, 0, 1, Effect: HealingItemEffect.FullHp);
    private static readonly HealingItemDefinition Tear = new(7, "Fairy Tear", 20, 0, 1, Effect: HealingItemEffect.RestoreMp);
    private static readonly HealingItemDefinition Rain = new(8, "Healing Rain", 255, 0, 0, Effect: HealingItemEffect.FullPartyHp);

    [Fact]
    public void WaterUsesTheMovedStopAndOriginalSlotEvenWithSilenceAndNoBook()
    {
        var session = Prepare(Water, patientHp: 10, patientMaxHp: 600);

        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        Accept(session, new Confirm());
        Accept(session, new SelectItem(2));
        var atStop = session.Current.Battle;
        Assert.NotNull(Send(session, new SelectTarget(Center)).Failure); // diagonal after moving north
        foreach (string id in new[] { "enemy", "n-neutral", "u-unplaced", "d-dead" })
            Assert.Equal("invalid-heal-target", Send(session, new SelectTarget(new(id))).Failure!.Code);
        Accept(session, new ReturnToBattleActions());
        Assert.Same(atStop, session.Current.Battle);
        Accept(session, new Cancel());
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        FinishMovement(session, Accept(session, new Confirm()));
        var commit = Use(session, Center);
        Assert.Equal(10, commit.Snapshot.Battle.GetActor(Center).Hp);
        Assert.Equal(new ushort[] { 6, 0, 127, 127 }, commit.Snapshot.Battle.GetActor(Caster).SourceLoadout!.Items);
        var finished = FinishBattleScenes(session, commit);
        Assert.Equal(600, finished.Snapshot.Battle.GetActor(Center).Hp);
        Assert.Equal(0, finished.Snapshot.Battle.GetActor(Caster).Mp);
        Assert.Single(finished.Observations, e => e.Kind == "item-consumed" && e.Before == 6 && e.After == 2);
        Assert.Equal(2, ConstructionRolls(commit).Count());
    }

    [Theory]
    [InlineData(4, 60, 5, 20, 16)]
    [InlineData(19, 60, 55, 5, 10)]
    [InlineData(20, 0, 0, 0, 10)]
    [InlineData(12, 60, 60, 0, 1)]
    public void TearUsesRecoveredMpOverMaximumHpForHealerExperience(
        byte classId, byte maxMp, byte beforeMp, byte recovery, int award)
    {
        var session = Prepare(Tear, classId, patientHp: 22, patientMaxHp: 30, patientMp: beforeMp, patientMaxMp: maxMp);
        Accept(session, new Confirm());
        var commit = Use(session, Center);
        Assert.Equal(award, commit.Snapshot.BattleScene!.RewardAmount);
        Assert.DoesNotContain(commit.Observations, e => e.Kind == "mp" || e.Kind == "hp");
        var result = FinishBattleScenes(session, commit);
        var patient = result.Snapshot.Battle.GetActor(Center);
        Assert.Equal(beforeMp + recovery, patient.Mp);
        Assert.Equal(22, patient.Hp);
        Assert.Equal((ushort)0x200, patient.Status);
        Assert.Equal(recovery == 0 ? 0 : 1, result.Observations.Count(e => e.Kind == "mp"));
        Assert.Equal(2, ConstructionRolls(commit).Count());
        Assert.Single(result.Observations, e => e.Kind == "item-consumed");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RainReplaysLivingPlacedSourceAlliesOnceFromSelfCenter(bool full)
    {
        var session = Prepare(Rain, 4, patientHp: 90, allFull: full);
        Accept(session, new Confirm());
        Accept(session, new SelectItem(2));
        Assert.Equal("invalid-aura-center", Send(session, new SelectTarget(Center)).Failure!.Code);
        Accept(session, new SelectTarget(Caster));
        var commit = Accept(session, new Confirm());
        var ordered = new[] { "a-edge", "b-center", "n-neutral", "m-full", "z-priest", "f-far" };
        Assert.Equal(ordered, commit.Snapshot.BattleScene!.HealingTargets.Select(target => target.Value));
        Assert.Equal(25, commit.Snapshot.BattleScene.RewardAmount);
        var phases = new List<BattleScenePhase>();
        var shown = new List<string>();
        var observations = commit.Observations.ToList();
        while (session.Current.BattleScene is { } scene)
        {
            phases.Add(scene.Phase);
            if (scene.Phase == BattleScenePhase.ResultMessage)
            {
                Assert.Equal(scene.Target, scene.DisplayedAlly);
                shown.Add(scene.Target!.Value.Value);
            }
            observations.AddRange(Accept(session, scene.RequiresAcknowledgement
                ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind)).Observations);
        }
        Assert.Equal(ordered, shown);
        Assert.Single(phases, phase => phase == BattleScenePhase.ActionMessage);
        Assert.Single(phases, phase => phase == BattleScenePhase.RewardMessage);
        Assert.DoesNotContain(BattleScenePhase.SpellCost, phases);
        Assert.Single(observations, e => e.Kind == "item-consumed");
        Assert.Equal(ordered, observations.Where(e => e.Kind == "healing-recovery").Select(e => e.Target!.Value.Value));
        Assert.Equal(0, session.Current.Battle.GetActor(new("d-dead")).Hp);
        Assert.Equal(20, session.Current.Battle.GetActor(new("u-unplaced")).Hp);
        Assert.Equal(600, session.Current.Battle.GetActor(new("f-far")).Hp);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void AntidoteClearsPoisonAndSkipsHalvingForTheFriendlyTarget()
    {
        var antidote = new HealingItemDefinition(3, "Antidote", 0, 0, 1, CuresPoison: true);
        var session = Prepare(antidote);
        Accept(session, new Confirm());
        var commit = Use(session, Center);
        Assert.Equal(5, commit.Snapshot.BattleScene!.RewardAmount);
        var settled = FinishBattleScenes(session, commit);
        Assert.Equal((ushort)0x200, settled.Snapshot.Battle.GetActor(Center).Status);
        Assert.Single(settled.Observations, observation => observation.Kind == "item-consumed");
    }

    [Theory]
    [InlineData(6)] [InlineData(7)] [InlineData(8)]
    public void FieldNoEffectAndCaravanRefusalKeepRecoverySupplies(byte item)
    {
        var preparation = new FieldPreparationDefinition([new(item, "Recovery supply", item == 8, false)],
            [new(item, FieldRecoveryEffect.NoEffect)]);
        BattleSourceLoadout Pack(int _) => new([item, 127, 127, 127], []);
        var session = CaravanServiceTests.Session(pack: Pack, preparation: preparation);
        var token = Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        Accept(session, new ChooseCaravan(token, CaravanAction.Item));
        token = Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        Accept(session, new ChooseCaravanItem(token, CaravanItemAction.Use));
        token = Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        Accept(session, new SelectCaravanMember(token, 0));
        token = Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        var before = session.Current;
        Assert.Equal("caravan-item-not-usable", Send(session, new SelectCaravanItemSlot(token, 0, item)).Failure!.Code);
        Assert.Same(before, session.Current);

        var field = CaravanServiceTests.Session(pack: Pack, preparation: preparation);
        Accept(field, new BackCaravan(Assert.IsType<CaravanWait>(field.Current.Story.Wait).Token));
        var party = field.Current.Exploration!.Party;
        var result = Accept(field, new FieldUseItem(new("member-0"), 0, item));
        Assert.Single(result.Observations, observation => observation.Kind == "field-no-effect");
        Assert.Same(party, field.Current.Exploration!.Party);
    }

    private static SessionResult Use(GameSession session, ActorRef target)
    {
        Accept(session, new SelectItem(2));
        Accept(session, new SelectTarget(target));
        return Accept(session, new Confirm());
    }

    private static GameSession Prepare(HealingItemDefinition item, byte classId = 12,
        ushort patientHp = 90, ushort patientMaxHp = 100, byte patientMp = 0, byte patientMaxMp = 0,
        bool allFull = false)
    {
        var admitted = Admitted("aura-healing");
        var basis = admitted.Definition.Encounters.Values.Single();
        var pack = new BattleSourceLoadout([item.ItemId, 0, item.ItemId, 127], [63, 63, 63, 63]);
        var growth = new BattleGrowthDefinition(classId, 0,
            Enumerable.Repeat(new StatGrowth(1, 1, []), 5).ToArray(), [], new Dictionary<byte, SpellRef>());
        var deployments = basis.Deployments.Select(row => row.Actor == Caster
            ? row with { Definition = new(Caster, BattleClassRule.Ordinary, 5, 100, 80, 10, 5, 100,
                false, 5, [], sourceLoadout: pack, growth: growth) }
            : row.Actor == Center
            ? row with { Definition = new(Center, BattleClassRule.Ordinary, 5, patientMaxHp, patientMaxMp, 10, 5, 19,
                false, 5, []) } : row.Actor.Value == "f-far" && item == Rain
                ? row with { SourceAllyControl = true, AiStrategy = new("source-ally-physical") } : row).ToArray();
        var encounter = new BattleDefinition(basis.Encounter, basis.Map, basis.Width, basis.Height,
            basis.Terrain, deployments, [], new(HalvedExperience: true), initialization: basis.Initialization,
            healingItems: [item], poison: item.CuresPoison);
        var inputs = admitted.Start.Actors.Select(row =>
        {
            var deployment = deployments.Single(d => d.Actor == row.Actor);
            var definition = deployment.Definition;
            if (deployment.SourceAllyControl) row = row with { ActivationWord = 4 };
            if (allFull && row.Hp > 0 && !row.Absent) row = row with { Hp = definition.MaxHp };
            return row.Actor == Caster ? row with { Mp = 0, Status = 0x200 } :
                row.Actor == Center ? row with { Hp = allFull ? patientMaxHp : patientHp, Mp = patientMp, Status = item.CuresPoison ? (ushort)0x202 : (ushort)0x200 } : row;
        });
        var result = GameSession.Start(new ScenarioDefinition("recovery-items", [encounter]),
            new BattleStartInput(basis.Encounter, inputs, 62u << 16, admitted.Start.ThinkingSeed, 73,
                deathCredit: item.CuresPoison ? new(null) : null));
        Assert.False(result is SessionStartFailed, (result as SessionStartFailed)?.Failure.ToString());
        return Assert.IsType<SessionStarted>(result).Session;
    }
}
