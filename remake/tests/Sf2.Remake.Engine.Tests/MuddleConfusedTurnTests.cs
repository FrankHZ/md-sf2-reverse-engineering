using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;
using EnemyParty = Sf2.Remake.Engine.Tests.EnemySpellDecisionTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MuddleConfusedTurnTests
{
    [Fact]
    public void AllyHitsOwnSideWithHalfExperienceAndKeepsTheSuccessfulFollowUpDraws()
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["encounters"]![0]!["rewards"]!["halvedExperience"] = true;
        });
        battle = Status(Stationary(battle, Mage), Mage, 0x18);
        var (end, events) = Play(battle, Mage, Priest, 2787);
        var strike = Assert.Single(events, e => e.Kind == "physical-first");
        Assert.Equal(Priest, strike.Target);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-double").RandomValue);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-counter").RandomValue);
        Assert.DoesNotContain(events, e => e.Kind is "physical-second" or "physical-counter");
        var hit = Assert.Single(events, e => e.Kind == "hp" && e.Actor == Priest);
        var plus = Assert.Single(events, e => e.Kind == "rng-exp-plus");
        var minus = Assert.Single(events, e => e.Kind == "rng-exp-minus");
        int raw = BattleRewards.DamageExperience((int)(hit.Before!.Value - hit.After!.Value), 30, 50);
        int expected = Math.Max(1, raw / 2 + (plus.RandomValue == 0 ? 1 : 0) - (minus.RandomValue == 0 ? 1 : 0));
        Assert.True(raw > expected);
        Assert.Equal((byte?)expected, end.GetActor(Mage).Exp);
        Assert.Equal(battle.Gold, end.Gold);
        Assert.Equal(battle.GetActor(Smoke).Hp, end.GetActor(Smoke).Hp);
        Assert.Equal(BattleControl.Player, end.GetActor(Mage).Control);
        Assert.Equal(0, end.GetActor(Mage).Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProtectedCenterGuardsOnEitherSideWithoutASelfDraw(bool enemy)
    {
        var battle = enemy ? EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = 8;
        }) : Battle();
        var actor = enemy ? EnemyParty.Cleric : Priest;
        var protectedTarget = enemy ? EnemyParty.Mage : Mage;
        var next = enemy ? EnemyParty.Scout : Mage;
        battle = Status(Stationary(battle, actor), actor, 0x18);
        var (end, events) = Play(battle, actor, next, 2521);
        Assert.Equal(protectedTarget, Assert.Single(events, e => e.Kind == "ai-target").Target);
        Assert.DoesNotContain(events, e => e.Kind is "rng-muddle-self-guard" or "movement" or "hp" or "mp");
        var line = Assert.Single(events, e => e.Kind == "rng-muddle-inaction-message");
        Assert.Equal((ushort?)16, line.RandomRange);
        Assert.Equal(2521L << 16, line.Before);
        var message = Assert.Single(events, e => e.Kind == "muddle-inaction");
        Assert.Equal((322 + (line.RandomValue <= 9 ? line.RandomValue!.Value : 0)).ToString(), message.Detail);
        Assert.Equal(battle.GetActor(actor).LastTarget, end.GetActor(actor).LastTarget);
        Assert.Equal(battle.GetActor(actor).Mp, end.GetActor(actor).Mp);
        Assert.Equal(enemy ? 0 : 1, events.Count(e => e.Kind == "exp"));
    }

    [Theory]
    [InlineData(194u, false)]
    [InlineData(2521u, true)]
    public void SelfPhysicalGuardIsDrawnOnceAndOnlyASuccessfulHitChangesTheActor(uint seed, bool guarded)
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
        });
        battle = Status(battle, Priest, 0x18);
        var before = battle.GetActor(Priest);
        var (end, events) = Play(battle, Priest, Mage, seed);
        var roll = Assert.Single(events, e => e.Kind == "rng-muddle-self-guard");
        Assert.Equal(guarded ? (ushort)1 : (ushort)0, roll.RandomValue);
        Assert.Equal(Priest, roll.Target);
        Assert.Equal(before.Position, end.GetActor(Priest).Position);
        Assert.Equal(guarded, events.Any(e => e.Kind == "muddle-inaction"));
        Assert.Equal(guarded ? 0 : 1, events.Count(e => e.Kind == "hp" && e.Actor == Priest));
        if (!guarded) Assert.True(end.GetActor(Priest).Hp < before.Hp);
    }

    [Theory]
    [InlineData(false, false, 1680u)]
    [InlineData(false, true, 1680u)]
    [InlineData(true, false, 2715u)]
    public void SelfAttackPreservesTheSameActorsPaymentWearGrowthAndDeath(bool item, bool lethal, uint seed)
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
        });
        var priest = battle.GetActor(Priest);
        var blaze = new SpellRef("blaze", 1);
        var physical = priest.Definition.Physical! with { Promoted = true };
        var equipment = new BattleEquipmentDefinition(10, [], rings:
            [new(24, "Black Ring", 1u << 17, RingStat.Attack, 10, DamageOnUse: true, Cursed: true)]);
        var curve = Enumerable.Repeat(new StatGrowthFraction(128, 128), 29).ToArray();
        var stats = new byte[] { 30, 20, 10, 5, 60 }.Select(v => new StatGrowth(v, (byte)(v + 10), curve)).ToArray();
        var growth = new BattleGrowthDefinition(17, 0, stats, [], new Dictionary<byte, SpellRef>());
        var loadout = new BattleSourceLoadout(item ? [152, 127, 127, 127] : [127, 127, 127, 127],
            item ? [63, 63, 63, 63] : [11, 63, 63, 63]);
        var definition = new BattleActorDefinition(Priest, BattleClassRule.Wizard, 5, 30, 20, 10, 5, 60, false, 2,
            item ? [] : [blaze], physical, BattleMover.Mage, loadout, growth, equipment);
        var owner = new BattleActorState(priest.Deployment with { Definition = definition }, lethal ? (ushort)1 : (ushort)30,
            20, 99, priest.Position, 0, 0, status: item ? (ushort)0x1C : (ushort)0x18, activationWord: 0);
        battle = Replace(battle, owner, attackItems: item
            ? [new(24, "Black Ring", new(new("blaze", 2), 0, 10, 1, 2, 1, AttackSpellElement.Fire), Breakable: true)] : []);

        var (end, events) = Play(battle, Priest, Mage, seed);
        var played = end.GetActor(Priest);
        Assert.Single(events, e => e.Kind == "hp" && e.Actor == Priest);
        Assert.Equal(Priest, Assert.Single(events, e => e.Kind == (item ? "ai-item-target" : "ai-spell-target")).Target);
        Assert.Equal(item ? 20 : 18, played.Mp);
        Assert.Equal(item ? 0 : 1, events.Count(e => e.Kind == "mp" && e.Actor == Priest));
        if (item)
        {
            Assert.Equal(408, played.SourceLoadout!.Items[0]);
            Assert.Single(events, e => e.Kind == "item-damaged");
        }
        if (lethal)
        {
            Assert.Equal(0, played.Hp);
            Assert.Null(played.Position);
            Assert.Equal(0, played.Status);
            Assert.Equal((ushort?)1, played.Defeats);
            Assert.Equal((byte?)99, played.Exp);
            Assert.DoesNotContain(events, e => e.Kind is "exp" or "rng-muddle-expiry");
        }
        else
        {
            Assert.InRange(played.Hp, 1, 29);
            Assert.Single(events, e => e.Kind == "level");
            Assert.Equal(6, played.Level);
            Assert.Equal(item ? CurseRules.Mask : 0, played.Status);
        }
        Assert.Equal((ushort?)0, played.Kills);
        Assert.Equal(battle.Gold, end.Gold);
    }

    [Fact]
    public void EnemyPhysicalKillKeepsCustodyAndDoesNotCreditThePlayer()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["actors"]![1]!["spells"] = new JsonArray();
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        var victim = battle.GetActor(EnemyParty.Wounded).With(hp: 1, status: 0x10,
            sourceLoadout: new([24, 127, 127, 127], [63, 63, 63, 63]));
        var drops = new EnemyItemDropDefinition(26, new Dictionary<ActorRef, byte> { [victim.Actor] = 130 },
            [new(26, 130, 24, 10)], [new(24, "Black Ring", true)]);
        battle = Replace(battle, victim, drops: drops);
        battle = Status(Stationary(battle, EnemyParty.Mage), EnemyParty.Mage, 0x18);
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 2787);
        Assert.Equal(victim.Actor, Assert.Single(events, e => e.Kind == "physical-first").Target);
        Assert.Equal(0, end.GetActor(victim.Actor).Hp);
        Assert.Equal(0, end.GetActor(victim.Actor).Status);
        Assert.Null(end.GetActor(victim.Actor).Position);
        Assert.Equal(24, end.GetActor(victim.Actor).SourceLoadout!.Items[0]);
        Assert.Equal(battle.Gold, end.Gold);
        Assert.Equal(battle.GetActor(EnemyParty.Scout).Kills, end.GetActor(EnemyParty.Scout).Kills);
        Assert.Empty(end.ItemCustody!.OnceFlags);
        Assert.Empty(end.ItemCustody.Deals);
        Assert.DoesNotContain(events, e => e.Kind is "exp" or "kills" or "gold" or "item-dropped" or "rng-item-drop");
        Assert.Null(end.DeathCredit?.Ally);
    }

    [Fact]
    public void AreaCanHitProtectedSlotAndEndTheBattleThroughTheDeathBatch()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["actors"]![1]!["spells"]![0]!["level"] = 2;
            d["spells"]![0]!["level"] = 2;
            d["spells"]![0]!["effect"]!["radius"] = 1;
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        battle = Stationary(battle, EnemyParty.Mage);
        var caster = battle.GetActor(EnemyParty.Mage).With(hp: 1, status: 0x18);
        battle = Replace(battle, caster, outcome: new(EnemyParty.Scout, EnemyParty.Wounded));
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == EnemyParty.Cleric ? a.With(hp: 0, clearPosition: true) :
            a.Actor == EnemyParty.Wounded ? a.With(hp: 1, status: 0x10) : a));
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 1680,
            scene =>
            {
                if (scene.Phase != BattleScenePhase.Reaction || !scene.HasReaction) return;
                Assert.Null(scene.DisplayedAlly);
                Assert.Equal(scene.ReactionTarget, scene.DisplayedEnemy);
            });
        Assert.Equal(EnemyParty.Wounded, Assert.Single(events, e => e.Kind == "ai-spell-target").Target);
        Assert.Equal(3, end.GetActor(caster.Actor).Mp);
        Assert.Equal(BattleOutcomeKind.Victory, BattleOutcomeRules.Check(end));
        foreach (var casualty in new[] { caster.Actor, EnemyParty.Wounded })
        {
            Assert.Equal(0, end.GetActor(casualty).Hp);
            Assert.Null(end.GetActor(casualty).Position);
            Assert.Equal(0, end.GetActor(casualty).Status);
            Assert.Single(events, e => e.Actor == casualty && e.Kind == "death-cleanup");
        }
        Assert.DoesNotContain(events, e => e.Kind is "muddle-inaction" or "gold" or "kills");
    }

    [Fact]
    public void ConnectedConfusedSpellSettlesBothDeathsAfterSceneRepositioningAndReturnsToField()
    {
        var world = Document("battle-drops");
        var battle = Document("enemy-spells");
        world["battle"] = battle;
        battle["maps"]![0]!["id"] = "arena";
        battle["spells"]![0]!["level"] = 2;
        battle["spells"]![0]!["effect"]!["radius"] = 1;
        battle["actors"]![0]!["physical"]!["leader"] = true;
        battle["actors"]![1]!["spells"]![0]!["level"] = 2;
        battle["actors"]![3]!["physical"]!["leader"] = true;
        battle["actors"]![3]!["items"] = new JsonArray(61);
        var encounter = battle["encounters"]![0]!;
        encounter["id"] = "arena-battle";
        encounter["map"] = "arena";
        encounter["placements"]![3]!["x"] = 5;
        encounter["placements"]![3]!["y"] = 2;
        encounter["leaderCleanup"] = JsonNode.Parse("""
            {"bowie":"scout","enemySlots":[{"actor":"mage","slot":128},{"actor":"cleric","slot":129},{"actor":"wounded","slot":130}]}
            """);
        encounter["itemDrops"] = JsonNode.Parse("""
            {"battle":18,"enemySlots":[{"actor":"wounded","slot":130}],"rows":[{"battle":18,"enemySlot":130,"item":61,"onceFlag":24}]}
            """);
        battle["dropItems"] = JsonNode.Parse("""[{"id":61,"name":"Achilles Sword","rare":true}]""");
        var input = battle["start"]!;
        input["encounter"] = "arena-battle";
        input["mainSeed"] = 48997u << 16; // After the nine first-round draws, the caster starts at 1680.
        input["actors"]![1]!["hp"] = 1;
        input["actors"]![2]!["hp"] = 0;
        input["actors"]![3]!["hp"] = 1;
        input["itemCustody"] = JsonNode.Parse("""{"onceFlags":[],"deals":[{"item":61,"count":3}]}""");
        var route = world["world"]!["maps"]![1]!["battle"]!;
        route["completedFlag"] = 501;
        route["outcome"]!["deathPositions"] = JsonNode.Parse("""
            [{"slot":0,"position":{"x":2,"y":2}},{"slot":128,"position":{"x":5,"y":2}}]
            """);

        var read = Reader(world).Read();
        Assert.True(read is ExplorationReadAccepted, read.ToString());
        var admitted = (ExplorationReadAccepted)read;
        var original = admitted.Definition;
        var defaults = original.EncounterStarts.Values.Single();
        // Control the enemy state at battle entry using the same admitted turn operands as the directed case.
        var controlled = defaults.WithResources(defaults.Actors.Select(actor => actor.Actor == EnemyParty.Mage
            ? actor with { Status = 0x18 } : actor.Actor == EnemyParty.Wounded ? actor with { Status = 0x10 } : actor), defaults.Gold);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values,
            exploration: original.Exploration, encounterStarts: [controlled]);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition, admitted.Start)).Session;
        Accept(session, new Interact(new("ferryman")));
        SettleOutcomeField(session);
        Assert.Equal(EnemyParty.Scout, session.Current.Selection!.Actor);

        var before = session.Current.Battle;
        var events = FinishMovement(session, Stay(session)).Observations.ToList();
        SessionCommand? finalDeathWait = null;
        for (int step = 0; step < 200 && session.Current.BattleScene is { } scene; step++)
        {
            Assert.False(session.Current.HasBattleControl);
            SessionCommand command = AttackSpellSessionTests.Next(session.Current);
            if (scene is { Phase: BattleScenePhase.FieldExit, FieldStep: 2 })
            {
                var casualty = session.Current.Battle.GetActor(EnemyParty.Wounded);
                Assert.Equal(0, casualty.Hp);
                Assert.Equal(0x10, casualty.Status);
                Assert.Null(casualty.Position);
            }
            if (scene.Phase == BattleScenePhase.FieldSettle)
            {
                finalDeathWait = command;
                Assert.Equal(3, session.Current.Battle.GetActor(EnemyParty.Mage).Mp);
                Assert.Null(session.Current.Battle.DeathCredit!.Ally);
                Assert.Equal(61, session.Current.Battle.GetActor(EnemyParty.Wounded).SourceLoadout!.Items[0]);
                Assert.All(scene.DeadActors, id =>
                {
                    var dead = session.Current.Battle.GetActor(id);
                    Assert.Equal(0, dead.Hp);
                    Assert.Equal(0, dead.Status);
                    Assert.Null(dead.Position);
                });
            }
            events.AddRange(Accept(session, command).Observations);
            if (scene is { Phase: BattleScenePhase.FieldExit, FieldStep: 2 })
            {
                var settled = session.Current;
                Assert.NotNull(Send(session, command).Failure);
                Assert.Same(settled, session.Current);
            }
        }
        Assert.True(finalDeathWait is not null, string.Join(", ", events.Select(e => e.Kind)));
        Assert.Equal(EnemyParty.Wounded, Assert.Single(events, e => e.Kind == "ai-spell-target").Target);
        ActorRef[] casualties = [EnemyParty.Mage, EnemyParty.Wounded];
        Assert.Equal(casualties, events.Where(e => e.Kind == "hp").Select(e => e.Actor!.Value));
        Assert.Equal(casualties, events.Where(e => e.Kind == "death-cleanup").Select(e => e.Actor!.Value));
        Assert.Single(events, e => e.Kind == "mp");
        Assert.Equal("Victory", Assert.Single(events, e => e.Kind == "battle-outcome").Detail);
        Assert.Single(events, e => e.Kind == "outcome-program-started");
        Assert.DoesNotContain(events, e => e.Kind is "exp" or "level" or "gold" or "kills" or "defeats" or
            "drop-once-set" or "item-received" or "item-to-deals" or "player-control" or "rng-muddle-self-guard");
        var pendingReturn = session.Current;
        Assert.NotNull(Send(session, finalDeathWait).Failure);
        Assert.Same(pendingReturn, session.Current);

        SettleOutcomeField(session, events);
        var returned = session.Current;
        Assert.Equal(SessionStopReason.PlayerInput, returned.StopReason);
        Assert.Single(events, e => e.Kind == "battle-completed-set" && e.Detail == "501");
        Assert.Single(events, e => e.Kind == "battle-unlock-cleared" && e.Detail == "401");
        Assert.Contains(501, returned.Story.Flags);
        Assert.DoesNotContain(401, returned.Story.Flags);
        var party = returned.Exploration!.Party;
        Assert.Equal(before.Gold, party.Gold);
        Assert.Same(before.ItemCustody, party.ItemCustody);
        var scout = Assert.Single(party.Actors);
        Assert.Equal(before.GetActor(EnemyParty.Scout).Exp, scout.Exp);
        Assert.Equal(before.GetActor(EnemyParty.Scout).Kills, scout.Kills);
        Assert.Equal(before.GetActor(EnemyParty.Scout).SourceLoadout!.Items, scout.SourceLoadout!.Items);
        var position = returned.Exploration.PlayerEntity.Position;
        Accept(session, new Move(ExplorationDirection.East));
        SettleOutcomeField(session);
        Assert.Equal(new MapPosition(position.X + 1, position.Y), session.Current.Exploration!.PlayerEntity.Position);
    }

    private static void SettleOutcomeField(GameSession session, List<SessionObservation>? observations = null)
    {
        for (int tick = 0; tick < 2000 && session.Current.Mode == SessionMode.Exploration && session.Current.Story.Wait is { } wait; tick++)
        {
            var result = Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            });
            observations?.AddRange(result.Observations);
        }
    }

    [Fact]
    public void PromotedMonsterConvertsAnAdmittedOwnSideStrikeAfterTheGuard()
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["actors"]![1]!["maxHp"] = 60;
            d["start"]!["actors"]![1]!["hp"] = 60;
        });
        var mage = battle.GetActor(Mage);
        var physical = mage.Definition.Physical! with { Promoted = true };
        var profile = new BattleActorDefinition(Mage, BattleClassRule.Monster, 25, 30, 20, 10, 5, 100,
            false, 0, [], physical, BattleMover.Hovering, mage.SourceLoadout);
        var kiwi = new BattleActorState(mage.Deployment with { Definition = profile }, mage.Hp, mage.Mp, mage.Exp,
            mage.Position, 0, 0, status: 0x18, activationWord: 0);
        var breath = new AttackSpellDefinition(new("kiwi", 1), 0, 28, 1, 1, 0, AttackSpellElement.Fire, 76);
        battle = Replace(battle, kiwi, extraSpells: [breath]);
        bool castSeen = false, reactionSeen = false;
        var (end, events) = Play(battle, Mage, Priest, 1, scene =>
        {
            if (!scene.IsMonsterBreath) return;
            if (scene.Phase == BattleScenePhase.ActionAnimation)
            {
                castSeen = true;
                Assert.Equal((byte?)76, scene.AttackAnimationSelector);
                Assert.Equal(Mage, scene.DisplayedAlly);
                Assert.Equal(Priest, scene.Target);
                Assert.Null(scene.DisplayedEnemy);
                Assert.Null(scene.MuddleActionMessage);
            }
            if (scene.Phase == BattleScenePhase.Reaction)
            {
                reactionSeen = true;
                Assert.Equal(Priest, scene.DisplayedAlly);
                Assert.Null(scene.DisplayedEnemy);
            }
        });
        Assert.True(castSeen && reactionSeen);
        Assert.Equal(Priest, Assert.Single(events, e => e.Kind == "ai-target").Target);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-kiwi-conversion").RandomValue);
        Assert.Single(events, e => e.Kind == "attack-spell" && e.Target == Priest);
        Assert.DoesNotContain(events, e => e.Kind is "mp" or "physical-first" or "muddle-inaction");
        Assert.Equal(20, end.GetActor(Mage).Mp);
        Assert.InRange(end.GetActor(Priest).Hp, 1, 59);
    }

    [Fact]
    public void HighPriorityEnemyTieRetainsCategoryDrawAndContinuesToMove()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![1]!["initialization"]!["commandset"] = 6;
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        battle = Stationary(battle, EnemyParty.Mage);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == EnemyParty.Mage
            ? a.With(status: 0x18, activationWord: 0x3001)
            : a.IsAlly ? a : a.With(hp: 1)));
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 1680);
        var category = Assert.Single(events, e => e.Kind == "rng-ai-attack-category");
        Assert.Equal((ushort?)2, category.RandomValue);
        Assert.Equal(2, events.Count(e => e.Kind == "ai-spell-candidate" && e.After == 17));
        Assert.Equal(EnemyParty.Mage, Assert.Single(events, e => e.Kind == "ai-move-target").Target);
        Assert.Equal(category.After, Assert.Single(events, e => e.Kind == "rng-muddle-expiry").Before);
        Assert.DoesNotContain(events, e => e.Kind is "hp" or "mp" or "muddle-inaction");
        Assert.Equal(battle.GetActor(EnemyParty.Mage).LastTarget, end.GetActor(EnemyParty.Mage).LastTarget);
    }

    [Fact]
    public void AnOwnTileMoveOrderCanFallThroughTwoAttackAttemptsInOneTurn()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![1]!["initialization"]!["commandset"] = 7;
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        var mage = battle.GetActor(EnemyParty.Mage).With(status: 0x18, activationWord: 0x3001, primaryOrder: 64);
        var init = battle.Definition.Initialization!;
        var ownPoint = new BattleInitializationDefinition(init.Regions, init.RegionProgram, init.AllyFormation,
            [mage.Position!], init.SourceCombatants, init.EnemyCount, init.RegionPrograms);
        battle = Replace(battle, mage, initialization: ownPoint);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == EnemyParty.Cleric || a.Actor == EnemyParty.Wounded
            ? a.With(hp: 1) : a));

        var (end, events) = Play(battle, mage.Actor, EnemyParty.Scout, 1680);
        var categories = events.Where(e => e.Kind == "rng-ai-attack-category").ToArray();
        Assert.Equal(2, categories.Length);
        var order = Assert.Single(events, e => e.Kind == "ai-command-move-order1");
        var attack = Assert.Single(events, e => e.Kind == "ai-command-attack1");
        Assert.True(categories[0].Sequence < order.Sequence && order.Sequence < categories[1].Sequence &&
            categories[1].Sequence < attack.Sequence);
        Assert.Equal(-1, order.After);
        Assert.Equal(-1, attack.After);
        Assert.Equal(categories[0].After, categories[1].Before);
        Assert.Equal(categories[1].After, Assert.Single(events, e => e.Kind == "rng-muddle-expiry").Before);
        Assert.DoesNotContain(events, e => e.Kind is "rng-muddle-self-guard" or "hp" or "mp");
        Assert.Equal(mage.Position, end.GetActor(mage.Actor).Position);
        Assert.Equal(mage.LastTarget, end.GetActor(mage.Actor).LastTarget);
    }

    [Fact]
    public void AbsenceOfReachableAttacksPursuesItsOwnFixedSlot()
    {
        var battle = Battle(d =>
        {
            d["actors"]![1]!["move"] = 1;
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![0]!["y"] = 3;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
            d["terrains"]![0]!["rows"]![3] = "pppppppppp";
        });
        var member = battle.GetActor(Priest);
        var physical = member.Definition.Physical! with { MinimumRange = 2, MaximumRange = 2 };
        var profile = new BattleActorDefinition(Priest, member.Definition.ClassRule, 5, 30, 20, 10, 5, 60,
            false, 1, member.Spells, physical, member.Definition.Mover);
        var owner = new BattleActorState(member.Deployment with { Definition = profile }, 30, 20, 0,
            member.Position, 0, 0, status: 0x18, activationWord: 0, sourceLoadout: member.SourceLoadout);
        battle = Replace(battle, owner);
        var (end, events) = Play(battle, Priest, Mage, 4500);
        Assert.Equal(Mage, Assert.Single(events, e => e.Kind == "ai-move-target").Target);
        Assert.True(end.GetActor(Priest).Position!.X < owner.Position!.X);
        Assert.DoesNotContain(events, e => e.Kind is "physical-first" or "rng-muddle-self-guard" or "muddle-inaction");
    }

    [Fact]
    public void ConfusedEnemyStaysAndTicksAfterTheFixedSlotIsKilledAndCleaned()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![1]!["x"] = 3;
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = 6;
            d["start"]!["actors"]![1]!["hp"] = 1;
        });
        var cleric = EnemyParty.Cleric;
        var scout = EnemyParty.Scout;
        battle = Stationary(battle, cleric).With(mainSeed: 4500u << 16,
            queue: [new(scout, 30), new(cleric, 25), new(scout, 20), new(null, 255)], cursor: 0);
        var fixedSlot = battle.GetActor(EnemyParty.Mage);
        Assert.True(fixedSlot.Initialized);
        Assert.NotNull(fixedSlot.Position);
        battle = Status(battle, cleric, 0x38);
        var current = BattleSaveFixture.At(battle, new StoryState());
        var rules = RuleCompositions.Sf2();
        List<SessionObservation> events = [];
        foreach (SessionCommand input in new SessionCommand[] { new Confirm(), new ChooseAction(SessionAction.PhysicalAttack),
            new SelectTarget(fixedSlot.Actor), new Confirm() })
        {
            var result = BattleCommandDispatcher.Submit(current, input, rules);
            Assert.Null(result.Failure);
            events.AddRange(result.Observations);
            current = result.Snapshot;
        }
        while (current.BattleScene is { } scene)
        {
            var result = BattleSceneContinuation.Submit(current, AttackSpellSessionTests.Next(current), rules);
            Assert.True(result.Failure is null, result.Failure?.ToString());
            events.AddRange(result.Observations);
            current = result.Snapshot;
        }

        var dead = current.Battle.GetActor(fixedSlot.Actor);
        Assert.Equal(0, dead.Hp);
        Assert.Equal(0, dead.Status);
        Assert.Null(dead.Position);
        Assert.True(dead.Initialized);
        Assert.True(current.Battle.GetActor(EnemyParty.Wounded).Hp > 0);
        Assert.Equal(scout, current.Selection!.Actor);
        Assert.Equal(2, current.Battle.Cursor);
        Assert.Contains(events, e => e.Kind == "ai-stay" && e.Actor == cleric);
        Assert.Single(events, e => e.Kind == "rng-muddle-expiry" && e.Actor == cleric);
        Assert.Equal(battle.GetActor(cleric).Position, current.Battle.GetActor(cleric).Position);
    }

    internal static EngineBattleState Stationary(EngineBattleState before, ActorRef actorRef,
        bool enhanced = false, BattleOutcomeDefinition? outcome = null)
    {
        var actor = before.GetActor(actorRef);
        var d = actor.Definition;
        var definition = new BattleActorDefinition(d.Actor, d.ClassRule, d.Level, d.MaxHp, d.MaxMp, d.Attack,
            d.Defense, d.Agility, d.ExtraRoundAction, 0, d.Spells, d.Physical, d.Mover, d.SourceLoadout,
            d.Growth, d.Equipment, d.Resistance, enhancedSpellPower: enhanced);
        var stationary = new BattleActorState(actor.Deployment with { Definition = definition },
            actor.Hp, actor.Mp, actor.Exp, actor.Position, actor.Kills, actor.Defeats, lastTarget: actor.LastTarget,
            status: actor.Status, activationWord: actor.ActivationWord, sourceLoadout: actor.SourceLoadout);
        return Replace(before, stationary, outcome: outcome, drops: before.Definition.ItemDrops);
    }

    private static EngineBattleState Replace(EngineBattleState before, BattleActorState actor,
        AttackItemDefinition[]? attackItems = null, BattleOutcomeDefinition? outcome = null,
        EnemyItemDropDefinition? drops = null, AttackSpellDefinition[]? extraSpells = null,
        BattleInitializationDefinition? initialization = null)
    {
        var d = before.Definition;
        var updated = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain,
            d.Deployments.Select(row => row.Actor == actor.Actor ? actor.Deployment : row),
            d.Spells.Values, d.Rewards, initialization ?? d.Initialization, outcome ?? d.Outcome,
            attackSpells: d.AttackSpells.Values.Concat(extraSpells ?? []),
            supportSpells: d.SupportSpells.Values, attackItems: attackItems, itemDrops: drops);
        return new(updated, before.Actors.Select(a => a.Actor == actor.Actor ? actor : a), before.MainSeed, before.ThinkingSeed,
            before.Round, before.Queue, before.Cursor, before.Gold, before.StartPolicy, before.Regions,
            itemCustody: drops is null ? before.ItemCustody : new([], new Dictionary<byte, byte>()));
    }

    internal static (EngineBattleState Battle, List<SessionObservation> Events) Play(
        EngineBattleState battle, ActorRef actor, ActorRef next, uint seed, Action<BattleSceneState>? observe = null)
    {
        battle = battle.With(mainSeed: seed << 16, queue: [new(actor, 30), new(next, 20), new(null, 255)], cursor: 0);
        var rules = RuleCompositions.Sf2();
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), new StoryState(),
            SessionStopReason.SimulationWait);
        var result = BattleAdvancer.Advance(snapshot, [], rules);
        List<SessionObservation> events = [];
        for (int i = 0; i < 200; i++)
        {
            Assert.True(result.Failure is null, result.Failure?.ToString());
            events.AddRange(result.Observations);
            var current = result.Snapshot;
            if (current.BattleMovement is { } move)
                result = BattleMovementContinuation.Submit(current, new CompletePresentation(move.Token, move.CompletionKind), rules);
            else if (current.BattleScene is { } scene)
            {
                observe?.Invoke(scene);
                result = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement
                    ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind), rules);
            }
            else
            {
                Assert.True(current.Selection?.Actor == next || BattleOutcomeRules.Check(current.Battle) is not null);
                return (current.Battle, events);
            }
        }
        throw new InvalidOperationException("Automatic turn did not settle.");
    }
}
