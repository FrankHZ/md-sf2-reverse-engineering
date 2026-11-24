using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RingEquipmentTests
{
    private static readonly ActorRef Hero = new("ally-0"), Enemy = new("enemy");

    internal static PrivateBattleDefinitions Source()
    {
        var basis = ConnectedBattleEquipmentTests.Source();
        SourceItemDefinition Ring(byte id, string code, string kind, string stat, int amount) =>
            new(id, code, code, "", "ALL", ["ALL"], 0, 0, 0, kind, id == 19 ? "ATTACK" : id == 20 ? "BOOST" : "NOTHING", id == 19 ? "ATTACK" : id == 20 ? "BOOST" : "NOTHING", 1,
                [new(stat, amount), new("NONE", 0), new("NONE", 0)]);
        return new(basis.Encounter, [], basis.Classes.Values,
            [.. basis.Items.Values,
             Ring(19, "POWER_RING", "RING|RARE|BREAKABLE", "INCREASE_ATT", 5),
             Ring(20, "PROTECT_RING", "RING|RARE|BREAKABLE", "INCREASE_DEF", 5),
             Ring(21, "QUICK_RING", "RING|RARE|BREAKABLE", "INCREASE_AGI", 5),
             Ring(22, "RUNNING_RING", "RING|RARE", "INCREASE_MOV", 2),
             Ring(126, "CHIRRUP_SANDALS", "RING|RARE", "NONE", 0)], [], [], []);
    }

    [Fact]
    public void MovingActorUsesTheirWornSoundRingWithoutChangingWeaponOrStats()
    {
        var (_, initial) = WithPack([178, 126, 127, 127]);
        var actor = initial.Battle.GetActor(Hero);
        var equipped = BattleEquipment.Equip(actor, 1);
        Assert.Equal(new ushort[] { 178, 254, 127, 127 }, equipped.SourceLoadout!.Items);
        Assert.Equal((actor.Attack, actor.Defense, actor.Agility, actor.Move, BattleEquipment.Physical(actor)),
            (equipped.Attack, equipped.Defense, equipped.Agility, equipped.Move, BattleEquipment.Physical(equipped)));

        int? StepSound(BattleActorState moving)
        {
            var battle = initial.Battle.With(actors: initial.Battle.Actors.Select(row => row.Actor == Hero ? moving : row));
            return BattleCommandDispatcher.Submit(BattleSaveFixture.At(battle, initial.Story),
                new Move(ExplorationDirection.East)).Snapshot.BattleMovement!.SoundOverride;
        }

        Assert.Null(StepSound(actor));
        Assert.Equal(99, StepSound(equipped));
        Assert.Null(StepSound(BattleEquipment.Equip(equipped, BattleEquipment.RemoveRingChoice)));
    }

    [Fact]
    public void OnlyLeaderEquipmentChangesOrdinaryFieldSteps()
    {
        var (_, initial) = WithPack([178, 126, 127, 127]);
        var battle = initial.Battle.Definition;
        var leader = initial.Battle.GetActor(Hero);
        var hero = battle.Deployments.Single(row => row.Actor == Hero);
        var companion = new ActorRef("friend");
        // The leader fixture defines party slot zero. A distinct allied actor carries the same ring.
        var friend = hero with
        {
            Definition = new(companion, hero.Definition.ClassRule, 1, 20, 5, 6, 4, 6, false, 6, [],
                sourceLoadout: new([254, 127, 127, 127], [63, 63, 63, 63]), equipment: hero.Definition.Equipment),
            Initialization = hero.Initialization! with { AllyPartyMember = 1 },
        };
        battle = battle.WithDeployments([hero, friend]);
        EntityRef keeper = new("keeper");
        var map = new ExplorationMapDefinition(new("sample-field"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(keeper, new(5, 1), 2, 32)],
            [new(ExplorationEventKind.Interact, null, null, keeper, new("depot", 0))]);
        var world = new ExplorationDefinition([map], [new("depot", [new OpenCaravan(), new EndProgram()])],
            partyFlags: new(20, 0, 32, 12));
        var definition = new ScenarioDefinition("field-footsteps", [battle], exploration: world)
            { FieldPreparation = PrivateFieldPreparation.Read(Source()) };
        var party = new BattleStartInput(battle.Encounter,
            [new(Hero, leader.Hp, leader.Mp, null, null, null, 0, null, leader.Progress, leader.SourceLoadout),
             new(companion, 10, 0, null, null, null, 0, null)], 0, 0, 0,
            itemCustody: new([], new Dictionary<byte, byte>()));
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(1, 1), 0, 32, [0, 1, 32, 33, 70], party))).Session;

        void Step(bool sounds)
        {
            var result = EngineTestContent.Accept(session, new Move(ExplorationDirection.East));
            var cues = result.Observations.Where(row => row.Kind == "entity-sound").Select(row => row.Detail).ToList();
            for (int i = 0; !session.Current.CanWaitAtInput; i++)
            {
                Assert.True(i < 32);
                result = EngineTestContent.Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
                cues.AddRange(result.Observations.Where(row => row.Kind == "entity-sound").Select(row => row.Detail));
            }
            Assert.Equal(sounds ? new[] { "99" } : [], cues);
        }

        Step(false);
        Assert.Null(session.QueryFieldActions(Hero).Items.Single(row => row.Word == 126).Use);
        EngineTestContent.Accept(session, new FieldEquip(Hero, 1, 126));
        Step(true);
        EngineTestContent.Accept(session, new FieldEquip(Hero, BattleEquipment.RemoveRingChoice, 254, ExpectedSourceSlot: 1));
        Step(false);
        WaitToken DepotWait() => Assert.IsType<CaravanWait>(session.Current.Story.Wait).Token;
        EngineTestContent.Accept(session, new Interact(keeper));
        EngineTestContent.Accept(session, new ChooseCaravan(DepotWait(), CaravanAction.Depot));
        EngineTestContent.Accept(session, new ChooseDepot(DepotWait(), DepotAction.Deposit));
        EngineTestContent.Accept(session, new SelectCaravanMember(DepotWait(), 0));
        EngineTestContent.Accept(session, new SelectDepotSlot(DepotWait(), 1, 126));
        Assert.Equal(new byte[] { 126 }, session.Current.Exploration!.Party.ItemCustody!.Depot);
        EngineTestContent.Accept(session, new ContinueCaravan(DepotWait()));
        EngineTestContent.Accept(session, new BackCaravan(DepotWait()));
        EngineTestContent.Accept(session, new ChooseDepot(DepotWait(), DepotAction.Look));
        var stored = session.Current.Exploration.Party;
        EngineTestContent.Accept(session, new SelectDepotItem(DepotWait(), 0, 126));
        var look = Assert.IsType<CaravanWait>(session.Current.Story.Wait);
        Assert.Equal(CaravanPhase.DepotLook, look.Phase);
        Assert.Same(stored, session.Current.Exploration.Party);
        Assert.Equal("depot.description.chirrup-sandals", CaravanDepotQuery.Look(definition, session.Current, look.Item!.Value).DescriptionKey);
        Assert.Null(CaravanDepotQuery.Look(definition, session.Current, 19).DescriptionKey);
        EngineTestContent.Accept(session, new ContinueCaravan(DepotWait()));
        EngineTestContent.Accept(session, new BackCaravan(DepotWait()));
        EngineTestContent.Accept(session, new ChooseDepot(DepotWait(), DepotAction.Derive));
        EngineTestContent.Accept(session, new SelectDepotItem(DepotWait(), 0, 126));
        EngineTestContent.Accept(session, new SelectCaravanMember(DepotWait(), 0));
        Assert.Empty(session.Current.Exploration.Party.ItemCustody!.Depot);
        Assert.Equal(new ushort[] { 178, 126, 127, 127 }, session.Current.Exploration.Party.Actors[0].SourceLoadout!.Items);
    }

    private static (ScenarioDefinition Definition, SessionSnapshot Current) WithPack(ushort[] words, ushort status = 0,
        ushort? applied = null, byte agility = 24)
    {
        var (basis, initial) = BattleSaveFixture.Create();
        var old = initial.Battle;
        var encounter = PrivateFieldPreparation.Equipment(old.Definition, Source(), allowBattleChanges: true);
        var definition = new ScenarioDefinition(basis.Package,
            basis.Encounters.Values.Select(row => row.Encounter == encounter.Encounter ? encounter : row),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var hero = old.GetActor(Hero);
        var loadout = new BattleSourceLoadout(words, hero.SourceLoadout!.Spells);
        var deployment = encounter.Deployments.Single(row => row.Actor == Hero);
        var actor = new BattleActorState(deployment, hero.Hp, hero.Mp, hero.Exp, hero.Position, hero.Kills, hero.Defeats,
            status: status, progress: hero.Progress! with { SourceLoadout = loadout, Defense = 40, Agility = agility },
            sourceLoadout: loadout, activationWord: 0, appliedStatBits: applied);
        var battle = new EngineBattleState(encounter, old.Actors.Select(a => a.Actor == Hero ? actor : a),
            old.MainSeed, old.ThinkingSeed, old.Round, old.Queue, old.Cursor, old.Gold, old.StartPolicy, old.Regions,
            itemCustody: old.ItemCustody, deathCredit: old.DeathCredit);
        var story = initial.Story.Copy(null, fieldPlayer: initial.Story.FieldPlayer! with
            { RetainedAllies = initial.Story.FieldPlayer.RetainedAllies!.Select(a => a with { Absent = false }).ToArray() });
        return (definition, BattleSaveFixture.At(battle, story));
    }

    [Fact]
    public void RingBeforeBladeAndRingOnlyResolveTheCorrectAttackAndRange()
    {
        var (_, current) = WithPack([147, 178, 20, 127]);
        var hero = current.Battle.GetActor(Hero);
        Assert.Equal(hero.BaseAttack + 10, hero.Attack);
        Assert.Equal((byte)50, BattleEquipment.EquippedWeaponItem(hero));
        Assert.Equal(2, BattleEquipment.Physical(hero)!.MaximumRange);
        var protect = BattleEquipment.Equip(hero, 2);
        Assert.Equal(new ushort[] { 19, 178, 148, 127 }, protect.SourceLoadout!.Items);
        Assert.Equal(hero.BaseAttack + 5, protect.Attack);
        Assert.Equal(45, protect.Defense);
        var unarmed = BattleEquipment.Equip(protect, BattleEquipment.UnarmedChoice);
        Assert.Null(BattleEquipment.EquippedWeaponItem(unarmed));
        Assert.Equal(1, BattleEquipment.Physical(unarmed)!.MaximumRange);
        Assert.Equal(45, unarmed.Defense);
        var removed = BattleEquipment.Equip(unarmed, BattleEquipment.RemoveRingChoice);
        Assert.Equal(new ushort[] { 19, 50, 20, 127 }, removed.SourceLoadout!.Items);
        Assert.Equal(40, removed.Defense);
        Assert.Equal(hero.BaseAttack, removed.Attack);
        var duplicate = new BattleSourceLoadout([147, 148, 127, 127], hero.SourceLoadout!.Spells);
        Assert.Equal("equipment-multiple", Assert.Throws<BattleRuleException>(() =>
            BattleEquipment.ValidateLoadout(hero.Definition, duplicate)).Code);
    }

    [Fact]
    public void RemovingOrStoringWornRingMovesOnlyItsWordAndClearsItsEffect()
    {
        var (_, current) = WithPack([147, 178, 21, 127]);
        var donor = current.Battle.GetActor(Hero);
        var withoutRing = BattleEquipment.RemoveItem(donor, 0);
        var damage = new Sf2PhysicalAction();
        var changed = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == Hero ? withoutRing : a));
        Assert.Equal(damage.EstimateDamage(changed, Hero, Enemy) + 5, damage.EstimateDamage(current.Battle, Hero, Enemy));
        Assert.Equal(new ushort[] { 178, 21, 127, 127 }, withoutRing.SourceLoadout!.Items);
        Assert.Equal(donor.Attack - 5, withoutRing.Attack);
        Assert.Equal(2, BattleEquipment.Physical(withoutRing)!.MaximumRange);

        var custody = new BattleItemCustody([], new Dictionary<byte, byte>(), [20, 20]);
        var deposited = CaravanInventory.Deposit(donor.SourceLoadout!, custody, 0, 147, cursed: false);
        Assert.Equal(withoutRing.SourceLoadout.Items, deposited.Pack.Items);
        Assert.Equal(new byte[] { 20, 20, 19 }, deposited.Custody.Depot);
        var received = CaravanInventory.Derive(deposited.Pack, deposited.Custody, 2, 19);
        Assert.Equal(new ushort[] { 178, 21, 19, 127 }, received.Pack.Items);
        Assert.Equal(new byte[] { 20, 20 }, received.Custody.Depot);
    }

    [Fact]
    public void QuickRingAffectsNextQueueWithoutChangingTheLiveQueue()
    {
        var (_, current) = WithPack([21, 127, 127, 127], status: 0x3000, agility: 100);
        var before = current.Battle;
        var equipped = BattleEquipment.Equip(before.GetActor(Hero), 0);
        var after = before.With(actors: before.Actors.Select(a => a.Actor == Hero ? equipped : a));
        Assert.Equal(127, equipped.Agility);
        Assert.Equal(before.Queue, after.Queue);
        BattleTurnFlow.GenerateQueue(after, out var next);
        Assert.Equal(127, next.Candidates.Single(a => a.Actor == Hero).Agility);
        // The increase retains the effective extra-entry bit, including a source-packed base.
        Assert.Equal((byte)205, BattleEquipment.PackedAgility(200, 0, equipped.Definition, equipped.SourceLoadout));
    }

    [Theory]
    [InlineData(20, 45, 55)]
    [InlineData(148, 50, 60)]
    public void ColdContinuePreservesBareOrWornRingAndAppliedCounter(ushort word, int before, int refreshed)
    {
        var (definition, live) = WithPack([word, 178, 127, 127], 0x3000, 0x1000);
        var saved = BattleSaveFixture.Capture(definition, live);
        var bytes = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved));
        var record = OpeningSaveJson.ReadRecord(bytes, 1);
        var cold = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2())).Session;
        BattleSaveFixture.EqualBattle(live.Battle, cold.Current.Battle);
        var actor = cold.Current.Battle.GetActor(Hero);
        Assert.Equal(before, actor.Defense);
        Assert.Equal(refreshed, actor.With(refreshStats: true).Defense);
        if (word == 148)
            Assert.Equal(55, BattleEquipment.Equip(actor, BattleEquipment.RemoveRingChoice).Defense);
    }

    [Theory]
    [InlineData(0, null, 2)]
    [InlineData(0xD000, (ushort)0x5000, 5)]
    public void DamagedRingAndAppliedAttackSurviveColdContinue(ushort status, ushort? applied, int version)
    {
        var (definition, live) = WithPack([403, 178, 127, 127], status, applied);
        var saved = BattleSaveFixture.Capture(definition, live);
        var written = new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, saved);
        Assert.Equal(version, written.FormatVersion);
        var record = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(written), 1);
        var cold = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2())).Session;
        BattleSaveFixture.EqualBattle(live.Battle, cold.Current.Battle);
        var worn = cold.Current.Battle.GetActor(Hero);
        Assert.Equal(applied, worn.LastRefreshBits);
        var bare = BattleEquipment.Equip(worn, BattleEquipment.RemoveRingChoice);
        Assert.Equal((ushort)275, bare.SourceLoadout!.Items[0]);
        Assert.Equal((ushort)403, BattleEquipment.Equip(bare, 0).SourceLoadout!.Items[0]);
        Assert.Equal((ushort)275, BattleInventory.Add(new([127, 127, 127, 127], worn.SourceLoadout!.Spells), 403).Items[0]);
    }

    [Fact]
    public void RemovingRingRevalidatesTheWornSlotEvenIfItsWordIsUnchanged()
    {
        var (_, current) = WithPack([147, 178, 19, 127]);
        current = Send(current, new Confirm()).Snapshot;
        current = Send(current, new BeginEquipment()).Snapshot;
        current = Send(current, new SelectEquipment(BattleEquipment.RemoveRingChoice)).Snapshot;
        var hero = current.Battle.GetActor(Hero);
        var shifted = hero.With(sourceLoadout: new([19, 178, 147, 127], hero.SourceLoadout!.Spells));
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == Hero ? shifted : a));
        var stale = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(battle, current.Selection), current.Story, current.StopReason);
        var result = BattleCommandDispatcher.Submit(stale, new Confirm());
        Assert.Equal("stale-equipment-selection", result.Failure?.Code);
        Assert.Same(stale, result.Snapshot);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void RemovingRunningRingKeepsConfirmedAttackAndBackUsesTheReducedBudget()
    {
        var (_, current) = WithPack([150, 178, 127, 127]);
        var destination = new MapPosition(12, 5);
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(a =>
            a.Actor == Enemy ? a.With(position: new(14, 5)) : a));
        var preview = BattleMovement.Preview(battle, Hero, destination);
        var start = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(battle, new(Hero, preview, BattleSelectionStage.Movement)), current.Story, current.StopReason);
        var after = start;
        foreach (var command in new SessionCommand[] { new Confirm(), new BeginEquipment(), new SelectEquipment(BattleEquipment.RemoveRingChoice), new Confirm() })
            after = Send(after, command).Snapshot;

        Assert.Equal(6, after.Battle.GetActor(Hero).Move);
        Assert.Equal(new MapPosition(4, 5), after.Battle.GetActor(Hero).Position);
        Assert.Equal(destination, after.Selection!.Preview.Destination);
        var option = BattleChoices.Query(after, RuleCompositions.Sf2(), null, false).Actions.Single(row => row.Action.Kind == BattleActionKind.Physical);
        Assert.True(option.Targets.Single(row => row.Actor == Enemy).Enabled);
        var selected = Send(after, new ChooseAction(SessionAction.PhysicalAttack)).Snapshot;
        var target = Send(selected, new SelectTarget(Enemy)).Snapshot;
        var committed = Send(target, new Confirm());
        Assert.Equal(destination, committed.Snapshot.Battle.GetActor(Hero).Position);
        Assert.Contains(committed.Observations, o => o.Kind == "movement" && o.From == new MapPosition(4, 5) && o.To == destination);

        var back = Send(after, new Cancel()).Snapshot;
        Assert.Equal(BattleMovementPurpose.Return, back.BattleMovement!.Purpose);
        Assert.Equal(preview.Path.Reverse(), back.BattleMovement.Path);
        Assert.Equal(BattleSelectionStage.Movement, back.Selection!.Stage);
        Assert.Equal("movement-pending",
            BattleMovementContinuation.Submit(back, new CommitBattleStay()).Failure!.Code);
        while (back.BattleMovement is { } movement)
        {
            var arrived = BattleMovementContinuation.Submit(back, new CompletePresentation(movement.Token, movement.CompletionKind));
            Assert.Null(arrived.Failure);
            back = arrived.Snapshot;
        }
        var range = BattleMovementQuery.Query(back)!;
        Assert.Equal(new MapPosition(4, 5), range.Origin);
        Assert.Equal(range.Origin, back.Selection!.Preview.Destination);
        Assert.Contains(new MapPosition(10, 5), range.Destinations);
        Assert.DoesNotContain(new MapPosition(11, 5), range.Destinations);
        Assert.DoesNotContain(destination, range.Destinations);
        Assert.Equal((ushort)22, back.Battle.GetActor(Hero).SourceLoadout!.Items[0]);

        var reselected = Send(back, new Move(ExplorationDirection.East)).Snapshot;
        Assert.Equal(range.Origin, reselected.BattleMovement!.From);
        Assert.Equal(new MapPosition(5, 5), reselected.BattleMovement.To);
    }

    private static SessionResult Send(SessionSnapshot current, SessionCommand command)
    {
        var result = BattleCommandDispatcher.Submit(current, command);
        Assert.Null(result.Failure);
        return result;
    }
}
