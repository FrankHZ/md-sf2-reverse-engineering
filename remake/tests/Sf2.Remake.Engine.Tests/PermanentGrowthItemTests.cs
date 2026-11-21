using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class PermanentGrowthItemTests
{
    private static readonly ActorRef Holder = new("ally-0"), Target = new("ally-1");
    private const uint Seed = 0x00001234;

    [Theory]
    [InlineData(9, FieldRecoveryEffect.IncreaseAttack, FieldGrowthStat.Attack)]
    [InlineData(10, FieldRecoveryEffect.IncreaseDefense, FieldGrowthStat.Defense)]
    [InlineData(11, FieldRecoveryEffect.IncreaseAgility, FieldGrowthStat.Agility)]
    [InlineData(13, FieldRecoveryEffect.IncreaseHp, FieldGrowthStat.Hp)]
    [InlineData(14, FieldRecoveryEffect.IncreaseMp, FieldGrowthStat.Mp)]
    public void StatUsePaysTheSelectedDuplicateAndClampsTheBaseAndLiveStat(
        ushort item, FieldRecoveryEffect effect, FieldGrowthStat stat)
    {
        var session = Start(item, effect, a => a with { Hp = 198, MaxHp = 199, Mp = 198, MaxMp = 199,
            BaseAttack = 199, Defense = 199, Agility = 99 });
        var before = session.Current;
        var command = new FieldUseItem(Holder, 2, item, Target);
        Assert.NotNull(Send(session, command with { Target = new("missing") }).Failure);
        Assert.NotNull(Send(session, command with { ExpectedWord = 0 }).Failure);
        Assert.Same(before, session.Current);
        var gained = Assert.Single(Use(session, command).Gains);
        Assert.Equal(new FieldGrowthGain(stat, 2), gained);
        var member = Details(session, Target);
        var progress = member.Carried.Progress!;
        Assert.Equal(stat == FieldGrowthStat.Attack ? 200 : 199, progress.BaseAttack);
        Assert.Equal(stat == FieldGrowthStat.Defense ? 200 : 199, member.Defense!.Value);
        Assert.Equal(stat == FieldGrowthStat.Agility ? 100 : 99, progress.Agility);
        Assert.Equal(stat == FieldGrowthStat.Agility ? 101 : 99, member.Agility!.Value);
        Assert.Equal(stat == FieldGrowthStat.Hp ? 200 : 198, member.Carried.Hp);
        Assert.Equal(stat == FieldGrowthStat.Mp ? 200 : 198, member.Carried.Mp);
        Assert.Equal(new ushort[] { item, 0, 127, 127 }, Details(session, Holder).Carried.SourceLoadout!.Items);
        Assert.Equal(0x00071234u, Party(session).MainSeed);
        Assert.Equal(before.Exploration!.Party.ThinkingSeed, Party(session).ThinkingSeed);
        var committed = session.Current;
        Assert.Equal("stale-input", session.Submit(new(before.SessionId, before.Revision, null, command)).Failure!.Code);
        Assert.Same(committed, session.Current);
    }

    [Theory]
    [InlineData(6, 8)]
    [InlineData(8, 9)]
    [InlineData(9, 9)]
    [InlineData(10, 12)]
    public void PimentoUsesStoredBaseAndKeepsItThroughEquipmentRefreshAndContinue(byte before, byte after)
    {
        var session = Start(12, FieldRecoveryEffect.IncreaseMove,
            a => a with { BaseMove = before, Items = [22, 127, 127, 127] });
        var effect = Use(session, new(Holder, 2, 12, Target));
        Assert.Equal(new(FieldGrowthStat.Move, after - before), Assert.Single(effect.Gains));
        Assert.Equal(after, Details(session, Target).Move);
        Assert.Equal(Seed, Party(session).MainSeed);
        Accept(session, new FieldEquip(Target, 0, 22));
        Assert.Equal(after + 2, Details(session, Target).Move!.Value);
        session = Cold(session);
        Assert.Equal(after + 2, Details(session, Target).Move!.Value);
        Accept(session, new FieldEquip(Target, BattleEquipment.RemoveRingChoice, 150, ExpectedSourceSlot: 0));
        Assert.Equal(after, Details(session, Target).Move);
        Assert.Equal(after, Details(session, Target).Carried.Progress!.BaseMove);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void ChickenWritesLiveAgilityBeforeRefreshingOnlyTheHolder(bool selfUse, byte expected)
    {
        var session = Start(11, FieldRecoveryEffect.IncreaseAgility,
            a => a with { Agility = 4, Status = 1, Items = [11, 22, 127, 127] });
        Use(session, selfUse ? new(Target, 0, 11, Target) : new(Holder, 2, 11, Target));
        session = Cold(session);
        var target = Details(session, Target);
        Assert.Equal(6, target.Carried.Progress!.Agility);
        Assert.Equal(expected, target.Agility);
        int ring = selfUse ? 0 : 1;
        Accept(session, new FieldEquip(Target, ring, 22));
        Assert.Equal(1, Details(session, Target).Agility!.Value);
        Assert.Null(Details(session, Target).Carried.Progress!.CurrentStats);
    }

    [Fact]
    public void HoneyWithoutMaxMpStillPaysAndBreadCanRestoreAFallenTarget()
    {
        var honey = Start(14, FieldRecoveryEffect.IncreaseMp, a => a with { MaxMp = 0, Mp = 0 });
        Assert.True(Use(honey, new(Holder, 2, 14, Target)).NoEffect);
        Assert.Equal(Seed, Party(honey).MainSeed);
        Assert.Equal(new ushort[] { 14, 0, 127, 127 }, Details(honey, Holder).Carried.SourceLoadout!.Items);

        var bread = Start(13, FieldRecoveryEffect.IncreaseHp, a => a with { Hp = 0 });
        Use(bread, new(Holder, 2, 13, Target));
        Assert.Equal(2, Details(bread, Target).Carried.Hp);
        Assert.Equal(27, Details(bread, Target).MaxHp!.Value);
    }

    [Fact]
    public void ChickenSaturatesTheCurrentLowBitsWithoutClearingTheExtraTurnFlag()
    {
        var session = Start(11, FieldRecoveryEffect.IncreaseAgility,
            a => a with { Agility = 99, CurrentStats = new(null, 0xFE, null) });
        Use(session, new(Holder, 2, 11, Target));
        var member = Details(Cold(session), Target);
        Assert.Equal(100, member.Carried.Progress!.Agility);
        Assert.Equal(0xFF, member.Carried.Progress.CurrentStats!.PackedAgility!.Value);
        Assert.Equal(127, member.Agility!.Value);
    }

    [Fact]
    public void MilkAddsToTheAlreadyBoostedAndEquippedValueUntilTheNextRefresh()
    {
        var session = Start(10, FieldRecoveryEffect.IncreaseDefense,
            a => a with { Defense = 31, Status = 0x1000, Items = [148, 127, 127, 127] });
        Assert.Equal(39, Details(session, Target).Defense!.Value);
        Use(session, new(Holder, 2, 10, Target));
        session = Cold(session);
        var target = Details(session, Target);
        Assert.Equal(33, target.Carried.Progress!.Defense);
        Assert.Equal(41, target.Defense!.Value);
        Accept(session, new FieldEquip(Target, BattleEquipment.RemoveRingChoice, 148, ExpectedSourceSlot: 0));
        Assert.Equal(37, Details(session, Target).Defense!.Value);
    }

    [Theory]
    [InlineData(3, 4, 0x00621234u)]
    [InlineData(40, 40, Seed)]
    public void SelfAppleClearsStoredExpAndKeepsTheBookAfterPayment(byte old, byte expected, uint seed)
    {
        var session = Start(15, FieldRecoveryEffect.LevelUp, a => a with { Level = old, Exp = 89, Hp = 0, Mp = 0,
            BaseMove = 8, Items = [15, 0, 127, 127] });
        var reported = Use(session, new(Target, 0, 15, Target));
        var cold = Cold(session);
        var target = Details(cold, Target);
        Assert.Equal(expected, target.Level);
        Assert.Equal(0, target.Carried.Exp!.Value);
        Assert.Equal(0, target.Carried.Hp);
        Assert.Equal(0, target.Carried.Mp);
        Assert.Equal(8, target.Move!.Value);
        Assert.Equal(seed, Party(cold).MainSeed);
        Assert.Equal(new ushort[] { 0, 127, 127, 127 }, target.Carried.SourceLoadout!.Items);
        Assert.Equal(old == 40 ? null : new SpellRef("muddle", 1), reported.Learned);
        Assert.Equal(old == 40 ? 63 : 7, target.Carried.SourceLoadout.Spells[0]);
        Assert.Equal(old == 40, reported.NoEffect);
        if (old != 40) Assert.Equal(new(FieldGrowthStat.Hp, 1), Assert.Single(reported.Gains));
    }

    [Fact]
    public void CaravanCancelAndStaleTokenDoNotPayThenAConfirmedUseGrowsTheReserve()
    {
        var session = Start(12, FieldRecoveryEffect.IncreaseMove, a => a with { BaseMove = 8 }, caravan: true);
        var original = Party(session);
        WaitToken Token() => Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        Accept(session, new ChooseCaravan(Token(), CaravanAction.Item));
        Accept(session, new ChooseCaravanItem(Token(), CaravanItemAction.Use));
        Accept(session, new SelectCaravanMember(Token(), 0));
        Accept(session, new SelectCaravanItemSlot(Token(), 2, 12));
        var canceled = new SelectCaravanMember(Token(), 1);
        Accept(session, new BackCaravan(Token()));
        Assert.Same(original, Party(session));
        Assert.NotNull(Send(session, canceled).Failure);
        Assert.Same(original, Party(session));
        Accept(session, new SelectCaravanMember(Token(), 0));
        Accept(session, new SelectCaravanItemSlot(Token(), 2, 12));
        Accept(session, new SelectCaravanMember(Token(), 1));
        var result = Assert.IsType<CaravanWait>(session.Current.Story.Wait).FieldGrowth!;
        Assert.Equal(new FieldGrowthGain(FieldGrowthStat.Move, 1), Assert.Single(result.Gains));
        Assert.Equal(9, Details(session, Target).Move!.Value);
        Assert.DoesNotContain(Target, Party(session).ActiveAllies!);
        Assert.Equal(new ushort[] { 12, 0, 127, 127 }, Details(session, Holder).Carried.SourceLoadout!.Items);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BattleContinueKeepsMoveAndDirectValuesForDeployedAndRetainedMembers(bool timed)
    {
        var (definition, current) = BattleSaveFixture.Create();
        var saved = BattleSaveFixture.Capture(definition, current);
        var hero = saved.Battle.Actors[0];
        var retained = Assert.Single(saved.Story.Player.RetainedAllies);
        var updated = hero with { Progress = hero.Progress! with { BaseMove = 8, CurrentStats = new(19, null, null) } };
        var reserve = retained with { Progress = retained.Progress! with { BaseMove = 9, CurrentStats = new(null, 115, null) } };
        var actors = saved.Battle.Actors.Select(row => row == hero ? updated : timed && row.Actor == "enemy"
            ? row with { Status = 0x0C00, LastRefreshCounters = 0x0400 } : row).ToArray();
        saved = saved with { Battle = saved.Battle with { Actors = actors },
            Story = saved.Story with { Player = saved.Story.Player with { RetainedAllies = [reserve] } } };
        var wire = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved)), 1);
        Assert.Equal(timed ? 5 : 2, wire.FormatVersion);
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, wire, RuleCompositions.Sf2())).Session;
        var actor = restored.Current.Battle.GetActor(new(hero.Actor));
        Assert.Equal(8, actor.Move);
        Assert.Equal(19, actor.Defense);
        var destination = new MapPosition(12, 5);
        Assert.DoesNotContain(destination, BattleMovement.Destinations(current.Battle, actor.Actor));
        Assert.Equal(destination, BattleMovement.Preview(restored.Current.Battle, actor.Actor, destination).Destination);
        Assert.Equal(9, Assert.Single(restored.Current.Story.FieldPlayer!.RetainedAllies!).Progress!.BaseMove!.Value);
        Assert.Equal(115, Assert.Single(restored.Current.Story.FieldPlayer.RetainedAllies!).Progress!.CurrentStats!.PackedAgility!.Value);
    }

    private static GameSession Start(ushort item, FieldRecoveryEffect effect, Func<SavedAlly, SavedAlly> change,
        bool caravan = false)
    {
        var (basis, data) = OpeningRestoreTests.Fixture(joined: true);
        var preparation = new FieldPreparationDefinition(
            [new((byte)item, "Growth item", false, false), new(0, "Herb", false, false),
             new(20, "Protect Ring", false, false), new(22, "Running Ring", false, false)],
            [new((byte)item, effect)]);
        var encounter = basis.Encounters[data.Encounter];
        var growth = new BattleGrowthDefinition(0, 0,
            [new(1, 30, Enumerable.Repeat(new StatGrowthFraction(8, 8), 29).ToArray()), ..Enumerable.Repeat(new StatGrowth(0, 0, []), 4)],
            [new(4, 7)], new Dictionary<byte, SpellRef> { [7] = new("muddle", 1) });
        var equipment = new BattleEquipmentDefinition(0, [], rings:
            [new(20, "Protect Ring", uint.MaxValue, RingStat.Defense, 5),
             new(22, "Running Ring", uint.MaxValue, RingStat.Move, 2)]);
        var battle = encounter.WithDeployments(encounter.Deployments.Select(d => d with
            { Definition = d.Definition.WithGrowth(growth).WithEquipment(equipment) }));
        var definition = new ScenarioDefinition(basis.Package, [battle], exploration: basis.Exploration)
            { Persistence = basis.Persistence, FieldPreparation = preparation };
        var holder = data.Allies[0] with { Items = [item, 0, item, 127] };
        var target = change(data.Allies[1]);
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data with
            { Allies = [holder, target], MainSeed = Seed }, RuleCompositions.Sf2())).Session;
        if (!caravan) return session;

        EntityRef keeper = new("keeper");
        var map = new ExplorationMapDefinition(new("village"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(keeper, new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, keeper, new("caravan", 0))]);
        var world = new ExplorationDefinition([map], [new("caravan", [new OpenCaravan(), new EndProgram()])], [],
            partyFlags: new(30, 0, 32, 12));
        var source = Party(session);
        var party = new BattleStartInput(source.Encounter, source.Actors, source.MainSeed, source.ThinkingSeed, source.Gold,
            activeAllies: [Holder], itemCustody: source.ItemCustody);
        session = Assert.IsType<SessionStarted>(GameSession.Start(new(basis.Package, [battle], exploration: world)
            { FieldPreparation = preparation }, new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32,
                [0, 1, 32, 70, 787], party), RuleCompositions.Sf2())).Session;
        Accept(session, new Interact(keeper));
        return session;
    }

    private static GameSession Cold(GameSession session)
    {
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var slot = new OpeningSaveRecord(1, 1, 1, DateTimeOffset.UnixEpoch, captured.Data!);
        var wire = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(slot), 1);
        return Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(session.Definition, wire, session.Rules)).Session;
    }

    private static PartyMemberDetails Details(GameSession session, ActorRef actor) =>
        session.QueryPartyDetails()!.Members.Single(member => member.Carried.Actor == actor);
    private static BattleStartInput Party(GameSession session) => session.Current.Exploration!.Party;
    private static SessionResult Send(GameSession session, SessionCommand command) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
    private static SessionResult Accept(GameSession session, SessionCommand command)
    {
        var result = Send(session, command);
        Assert.Null(result.Failure);
        return result;
    }
    private static FieldGrowthResult Use(GameSession session, FieldUseItem item) =>
        Assert.Single(Accept(session, item).Observations).FieldGrowth!;
}
