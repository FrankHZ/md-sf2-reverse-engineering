using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemySpellDecisionTests
{
    internal static readonly ActorRef Scout = new("scout"), Mage = new("mage"), Cleric = new("cleric"), Wounded = new("wounded");
    internal static EngineBattleState Battle(Action<JsonNode>? change = null)
    {
        var document = Document("enemy-spells"); change?.Invoke(document);
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        return BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
    }
    internal static BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, SessionRules? rules = null)
    {
        rules ??= RuleCompositions.Sf2();
        return BattleDecisionRules.Decide(new SourceEnemyAi(), battle, actor, rules.Physical, rules.Progression, rules.Actions);
    }

    [Fact]
    public void OrdinaryAttackerWithAttackOneAndSufficientMpChoosesPhysicalContact()
    {
        var document = Document("enemy-spells");
        var placement = document["encounters"]![0]!["placements"]![1]!;
        placement["x"] = 3;
        placement["initialization"]!["commandset"] = 6;
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var encounter = admitted.Definition.Encounters.Values.Single();
        var mage = encounter.Deployments.Single(row => row.Actor == Mage).Definition;
        var attacker = new BattleActorDefinition(Mage, mage.ClassRule, mage.Level, mage.MaxHp, 12,
            mage.Attack, mage.Defense, mage.Agility, mage.ExtraRoundAction, mage.Move,
            [new("attack", 1)], mage.Physical, mage.Mover, new([127, 127, 127, 127], [5, 63, 63, 63]));
        var battle = BattleTurnFlow.Start(encounter.WithDeployments(encounter.Deployments.Select(
            row => row.Actor == Mage ? row with { Definition = attacker } : row)), admitted.Start);
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Mage ? actor.With(mp: 12) : actor));

        var decision = Decide(battle, Mage);
        Assert.Equal(Scout, decision.Target);
        Assert.Null(decision.ChosenAction); // The ordinary physical target commits through the default action.
    }

    [Fact]
    public void AutomaticCasterWithBlazeThreeAndBoostTwoCastsTheAffordableBlazeRank()
    {
        var document = Document("enemy-spells");
        foreach (var (rank, cost, power) in new[] { (2, 5, 10), (3, 10, 15) })
        {
            var spell = document["spells"]![0]!.DeepClone();
            spell["level"] = rank;
            spell["mpCost"] = cost;
            spell["effect"]!["power"] = power;
            spell["effect"]!["radius"] = 1;
            document["spells"]!.AsArray().Add(spell);
        }
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var encounter = admitted.Definition.Encounters.Values.Single();
        var mage = encounter.Deployments.Single(row => row.Actor == Mage).Definition;
        var caster = new BattleActorDefinition(Mage, mage.ClassRule, mage.Level, mage.MaxHp, mage.MaxMp,
            mage.Attack, mage.Defense, mage.Agility, mage.ExtraRoundAction, mage.Move,
            [new("blaze", 1), new("blaze", 2), new("blaze", 3), new("boost", 1), new("boost", 2)],
            mage.Physical, mage.Mover, new([127, 127, 127, 127], [0x8B, 0x43, 63, 63]));
        var battle = BattleTurnFlow.Start(encounter.WithDeployments(encounter.Deployments.Select(
            row => row.Actor == Mage ? row with { Definition = caster } : row)), admitted.Start);
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Mage ? actor.With(mp: 2) : actor));

        var decision = Decide(battle, Mage);
        Assert.Equal(new BattleActionRef(BattleActionKind.AttackSpell, new("blaze", 1)), decision.ChosenAction);
        Assert.Equal(Scout, decision.Target);
    }

    [Fact]
    public void EnemySelectsKnownFreezeAndUsesIceResistance()
    {
        var battle = Battle(d =>
        {
            var spell = d["spells"]![0]!;
            spell["id"] = "freeze";
            spell["mpCost"] = 3;
            spell["effect"]!["element"] = "ice";
            spell["effect"]!["power"] = 10;
            d["actors"]![1]!["spells"]![0]!["id"] = "freeze";
            d["actors"]![0]!["resistance"] = 2 << 4;
        });
        var action = Decide(battle, Mage);
        var key = new SpellRef("freeze", 1);
        Assert.Equal(new BattleActionRef(BattleActionKind.AttackSpell, key), action.ChosenAction);
        Assert.Equal(5, Sf2AttackSpellAction.PotentialDamage(battle.GetActor(Mage), battle.GetActor(Scout), battle.Definition.AttackSpells[key]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnhancedEnemyScoresRawResistedPowerAndExecutesEnhancedResistedDamage(bool confused)
    {
        var battle = Battle(d =>
        {
            d["actors"]![confused ? 3 : 0]!["resistance"] = 1 << 6;
            var encounter = d["encounters"]![0]!;
            encounter["placements"]![confused ? 3 : 0]!["x"] = 4;
            encounter["placements"]![confused ? 3 : 0]!["y"] = 2;
        });
        var victim = confused ? Wounded : Scout;
        battle = MuddleConfusedTurnTests.Stationary(battle, Mage, enhanced: true, outcome: new(Scout, Mage));
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Mage
            ? actor.With(status: confused ? (ushort)0x18 : (ushort)0, activationWord: 0x3001)
            : actor.Actor == victim ? actor.With(hp: 6) : actor));
        var (end, events) = MuddleConfusedTurnTests.Play(battle, Mage, Scout, 1680);
        var candidate = Assert.Single(events, e => e.Kind == "ai-spell-candidate");
        Assert.Equal(victim, candidate.Target);
        Assert.Equal(2, candidate.After); // Raw 6 minus 1 resistance leaves 1 HP, below the kill threshold.
        var hp = Assert.Single(events, e => e.Kind == "hp" && e.Actor == victim);
        Assert.Equal(6, hp.Before);
        Assert.Equal(0, hp.After); // Enhanced 7 minus 1 resistance deals the lethal 6.
        Assert.Equal(3, end.GetActor(Mage).Mp);
    }

    [Theory]
    [InlineData(1, 0u, true, false)]
    [InlineData(2, 0u, false, false)]
    [InlineData(2, 0x10000000u, true, false)]
    [InlineData(2, 0u, true, true)]
    public void AquaRankSelectsTheActualPackedCategoryRule(byte level, uint seed, bool cast, bool spellOnly)
    {
        var battle = Battle(d =>
        {
            var spell = d["spells"]![0]!;
            spell["id"] = "aqua"; spell["level"] = level; spell["mpCost"] = 0;
            spell["maximumRange"] = spellOnly ? 3 : 1; spell["effect"]!["element"] = "ice";
            spell["effect"]!["power"] = 15;
            d["actors"]![1]!["spells"]![0]!["id"] = "aqua";
            d["actors"]![1]!["spells"]![0]!["level"] = level;
        }).With(mainSeed: seed);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Mage
            ? a.With(position: new(spellOnly ? 6 : 3, 2), mp: 0, status: SilenceRules.Mask) : a));
        var chosen = Decide(battle, Mage);
        Assert.Equal(cast, chosen.ChosenAction?.Kind == BattleActionKind.AttackSpell);
        Assert.Equal(level == 1 ? seed : BattleRandom.NextMain(seed, 6).After, chosen.Battle.MainSeed);
        Assert.Equal(level == 1 ? 0 : 1, chosen.Effects.Count(e => e.Kind == "rng-ai-attack-category"));
        if (cast)
        {
            Assert.Equal(new SpellRef("aqua", level), chosen.ChosenAction!.Value.Spell);
            var rules = RuleCompositions.Sf2();
            var prepared = BattleActionRules.Prepare(new Sf2AttackSpellAction(), chosen.Battle, Mage,
                chosen.Destination, chosen.ChosenAction.Value, Scout, rules.Progression);
            Assert.Equal(0, prepared.Prepared.GetActor(Mage).Mp);
            Assert.Contains(prepared.Reactions, hit => hit.Target == Scout && hit.HpAfter < hit.HpBefore);
        }
    }

    [Fact]
    public void EnemySwordUsesActualSlotAtZeroMpAndRetainsTheWeapon()
    {
        var before = Battle();
        var source = before.Definition;
        var bolt = new AttackSpellDefinition(new("bolt", 2), 15, 16, 1, 3, 2, AttackSpellElement.Lightning);
        var definition = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height, source.Terrain,
            source.Deployments, source.Spells.Values, source.Rewards, source.Initialization,
            attackSpells: source.AttackSpells.Values.Append(bolt));
        var origin = before.GetActor(Scout).Position!;
        var actors = before.Actors.Select(unit => unit.Actor == Mage ? unit.With(mp: 0, position: new(origin.X + 3, origin.Y),
            sourceLoadout: new([127, 238, 127, 127], [63, 63, 63, 63])) : unit);
        var battle = new EngineBattleState(definition, actors, before.MainSeed, before.ThinkingSeed, before.Round,
            before.Queue, before.Cursor, before.Gold, before.StartPolicy, before.Regions);
        var rules = RuleCompositions.Sf2();
        var sword = new BattleActionRef(BattleActionKind.AttackSpell, bolt.Spell, 1);
        var actor = battle.GetActor(Mage);
        var prepared = BattleActionRules.Prepare(new Sf2AttackSpellAction(), battle, Mage, actor.Position!,
            sword, Scout, rules.Progression);
        Assert.Equal(1, prepared.AttackItemSlot);
        Assert.Equal(0, prepared.Prepared.GetActor(Mage).Mp);
        Assert.Equal(238, prepared.Prepared.GetActor(Mage).SourceLoadout!.Items[1]);
        Assert.Contains(prepared.Reactions, hit => hit.Target == Scout && hit.HpAfter < hit.HpBefore);
        Assert.Throws<BattleRuleException>(() => rules.Action(sword).RequireAction(battle, Mage, sword with { ItemSlot = 0 }));
    }

    [Fact]
    public void SentryMovesToBlazeRangeAndOnlyStagesSourceDecisionRng()
    {
        var before = Battle(); var chosen = Decide(before, Mage);
        Assert.Equal(new BattleActionRef(BattleActionKind.AttackSpell, new("blaze", 1)), chosen.ChosenAction);
        Assert.Equal(Scout, chosen.Target); Assert.Equal(new MapPosition(4, 2), chosen.Destination);
        Assert.Equal(new[] { new MapPosition(6, 2), new MapPosition(5, 2), new MapPosition(4, 2) }, chosen.Path);
        Assert.Equal(Scout, chosen.Battle.GetActor(Mage).LastTarget);
        Assert.Equal(60, chosen.Battle.GetActor(Scout).Hp); Assert.Equal(5, chosen.Battle.GetActor(Mage).Mp);
        var thinking = BattleRandom.NextThinkingWord((ushort)(before.ThinkingSeed >> 16), 3);
        Assert.Equal(((uint)thinking.After << 16) | (before.ThinkingSeed & 65535), chosen.Battle.ThinkingSeed);
        Assert.Equal(BattleRandom.NextMain(before.MainSeed, 6).After, chosen.Battle.MainSeed);
        Assert.Single(chosen.Effects, e => e.Kind == "thinking-rng");
        Assert.Single(chosen.Effects, e => e.Kind == "rng-ai-attack-category");
        Assert.DoesNotContain(chosen.Effects, e => e.Kind.StartsWith("rng-spell-"));
        Assert.Equal(new MapPosition(6, 2), before.GetActor(Mage).Position);
    }

    [Theory]
    [InlineData(0u, false)] [InlineData(0x10000000u, true)] [InlineData(0x80000000u, false)] [InlineData(0xC0000000u, true)]
    public void ViablePhysicalAndSpellBothScoreBeforeMainCategoryChooses(uint main, bool spell)
    {
        var before = Battle().With(mainSeed: main);
        before = before.With(actors: before.Actors.Select(a => a.Actor == Mage ? a.With(position: new(3, 2)) : a));
        var chosen = Decide(before, Mage);
        Assert.Equal(spell, chosen.ChosenAction?.Kind == BattleActionKind.AttackSpell);
        Assert.Equal(Scout, chosen.Target); Assert.Equal(new MapPosition(3, 2), chosen.Destination);
        Assert.Equal(new[] { "ai-spell-candidate", "ai-candidate", "rng-ai-attack-category" },
            chosen.Effects.Where(e => e.Kind is "ai-spell-candidate" or "ai-candidate" or "rng-ai-attack-category").Select(e => e.Kind));
        Assert.Equal(2, chosen.Effects.Count(e => e.Kind == "thinking-rng"));
        Assert.Equal(BattleRandom.NextMain(main, 6).After, chosen.Battle.MainSeed);
    }

    [Theory]
    [InlineData("regular", true, "wounded")] [InlineData("regular", false, "scout")]
    [InlineData("healer", true, "wounded")] [InlineData("healer", false, "scout")]
    [InlineData("archer", true, "wounded")] [InlineData("archer", false, "scout")]
    [InlineData("centaur", true, "wounded")] [InlineData("centaur", false, "scout")]
    [InlineData("mage", true, "wounded")] [InlineData("mage", false, "wounded")]
    public void LethalSpellTiesAlwaysUseMagePreferenceWhilePhysicalTiesUseActualMover(string mover, bool cast, string target)
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["classRule"] = "unpromoted-knight";
            d["actors"]![3]!["classRule"] = "unpromoted-warrior";
            d["actors"]![1]!["physical"]!["movementType"] = mover;
            d["start"]!["actors"]![0]!["hp"] = 1;
            d["start"]!["actors"]![3]!["hp"] = 1;
            d["start"]!["actors"]![1]!["initialization"]!["activationWord"] = 0x3001;
            var placements = d["encounters"]![0]!["placements"]!;
            placements[0]!["x"] = 3; placements[0]!["y"] = 3;
            placements[1]!["x"] = 4; placements[1]!["y"] = 3;
            var reserve = placements[3]!;
            reserve["faction"] = "ally"; reserve["control"] = "player"; reserve["aiStrategy"] = null;
            reserve["processingOrder"] = 1; reserve["initialization"]!["baseAiWord"] = null;
            reserve["x"] = 5; reserve["y"] = 3;
            placements[2]!["initialization"]!["primaryOrder"] = 255;
            d["start"]!["actors"]![3]!["initialization"]!["activationWord"] = 0;
            d["encounters"]![0]!["initialization"]!["sourceCombatants"]![3]!["slot"] = 1;
        }).With(mainSeed: cast ? 0x10000000u : 0u);
        var chosen = Decide(before, Mage);
        Assert.Equal(cast, chosen.ChosenAction?.Kind == BattleActionKind.AttackSpell);
        Assert.Equal(new ActorRef(target), chosen.Target);
        Assert.Equal(before.GetActor(Mage).Position, chosen.Destination);
        Assert.Equal(2, chosen.Effects.Count(e => e.Kind == "ai-spell-candidate" && e.After == 17));
        Assert.Equal(2, chosen.Effects.Count(e => e.Kind == "ai-candidate" && e.After == 17));
        Assert.Equal(before.ThinkingSeed, chosen.Battle.ThinkingSeed);
        Assert.Single(chosen.Effects, e => e.Kind == "rng-ai-attack-category");
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    public void HealerOneWithHealTwoChecksBookCostBeforeCastingHealOne(int mp, bool canHeal)
    {
        var battle = Battle(d =>
        {
            var healTwo = d["spells"]![1]!.DeepClone();
            healTwo["level"] = 2; healTwo["mpCost"] = 5; healTwo["maximumRange"] = 2;
            d["spells"]!.AsArray().Add(healTwo);
            d["actors"]![2]!["spells"]!.AsArray().Add(JsonNode.Parse("""{"id":"heal","level":2}"""));
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = 0;
            d["start"]!["actors"]![2]!["mp"] = mp;
        });
        var decision = Decide(battle, Cleric);
        Assert.Equal(canHeal, decision.ChosenAction == new BattleActionRef(BattleActionKind.Healing, new("heal", 1)));
        if (canHeal)
        {
            Assert.Equal(Wounded, decision.Target);
            Assert.DoesNotContain(decision.Effects, e => e.Kind == "ai-command-attack1");
        }
    }

    [Fact]
    public void HighPriestReachesAWoundedAllyUsingTheSelectedHealThreeRange()
    {
        var battle = Battle(d =>
        {
            var cleric = d["actors"]![2]!;
            cleric["maxMp"] = 29;
            d["start"]!["actors"]![2]!["mp"] = 20;
            d["encounters"]![0]!["placements"]![3]!["x"] = 3;
            foreach (int level in new[] { 2, 3 })
            {
                var spell = d["spells"]![1]!.DeepClone();
                spell["level"] = level;
                spell["maximumRange"] = level;
                spell["mpCost"] = level == 3 ? 10 : 5;
                spell["effect"]!["adjustedPower"] = level == 3 ? 30 : 15;
                d["spells"]!.AsArray().Add(spell);
                cleric["spells"]!.AsArray().Add(new JsonObject { ["id"] = "heal", ["level"] = level });
            }
        });
        var chosen = Decide(battle, Cleric);
        var spellRef = new SpellRef("heal", 3);
        Assert.Equal(new BattleActionRef(BattleActionKind.Healing, spellRef), chosen.ChosenAction);
        Assert.Equal(Wounded, chosen.Target);
        Assert.Equal(new MapPosition(6, 5), chosen.Destination);
        var rule = new Sf2HealingRule();
        var selected = rule.RequireSpell(battle, Cleric, spellRef);
        Assert.Equal(10, selected.MpCost);
        Assert.Equal(Wounded, rule.RequireTarget(battle, Cleric, chosen.Destination!, selected, Wounded).Actor);
    }

    [Fact]
    public void HealCommandTwoChoosesHealOneBeforeOrderAttackOrPursuitWithoutDecisionRng()
    {
        var before = Battle(); var chosen = Decide(before, Cleric);
        Assert.Equal(new BattleActionRef(BattleActionKind.Healing, new("heal", 1)), chosen.ChosenAction);
        Assert.Equal(Wounded, chosen.Target); Assert.Equal(new MapPosition(5, 5), chosen.Destination);
        Assert.Equal(before.MainSeed, chosen.Battle.MainSeed); Assert.Equal(before.ThinkingSeed, chosen.Battle.ThinkingSeed);
        Assert.Contains(chosen.Effects, e => e.Kind == "ai-command-heal2" && e.After == 0);
        Assert.DoesNotContain(chosen.Effects, e => e.Kind.StartsWith("ai-command-move-order"));
        Assert.Equal(8, chosen.Battle.GetActor(Wounded).Hp); Assert.Equal(7, chosen.Battle.GetActor(Cleric).Mp);
    }

    [Theory]
    [InlineData(4, 17)] [InlineData(17, 2)] [InlineData(18, 1)]
    public void ClericSourceModifierUsesScriptFourLethalityAndInclusiveOneFifthWithoutRng(int hp, int priority)
    {
        var before = Battle(d =>
        {
            d["start"]!["actors"]![0]!["hp"] = hp;
            d["start"]!["actors"]![3]!["hp"] = 30;
            var reserve = d["encounters"]![0]!["placements"]![3]!;
            reserve["faction"] = "ally"; reserve["control"] = "player"; reserve["aiStrategy"] = null;
            reserve["processingOrder"] = 1; reserve["initialization"]!["baseAiWord"] = null;
            reserve["x"] = 0; reserve["y"] = 8;
            d["start"]!["actors"]![3]!["initialization"]!["activationWord"] = 0;
            d["encounters"]![0]!["initialization"]!["sourceCombatants"]![3]!["slot"] = 1;
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["primaryOrder"] = 255;
        });
        before = before.With(actors: before.Actors.Select(a => a.Actor == Scout ? a.With(position: new(6, 4)) : a));
        var chosen = Decide(before, Cleric);
        Assert.Equal(Scout, chosen.Target); Assert.Null(chosen.ChosenAction);
        Assert.Equal(priority, Assert.Single(chosen.Effects, e => e.Kind == "ai-candidate").After);
        Assert.Equal(before.ThinkingSeed, chosen.Battle.ThinkingSeed); Assert.Equal(before.MainSeed, chosen.Battle.MainSeed);
        Assert.DoesNotContain(chosen.Effects, e => e.RandomRange is not null);
    }

    [Fact]
    public void MultipleSpellTargetsUseReverseSourceScoringBeforePhysicalAndRemainIndependentOfActorCollectionOrder()
    {
        var before = Battle(d =>
        {
            var placement = d["encounters"]![0]!["placements"]![3]!;
            placement["faction"] = "ally"; placement["control"] = "player"; placement["aiStrategy"] = null;
            placement["processingOrder"] = 1; placement["initialization"]!["baseAiWord"] = null;
            placement["x"] = 3; placement["y"] = 2;
            d["start"]!["actors"]![3]!["initialization"]!["activationWord"] = 0;
            d["encounters"]![0]!["initialization"]!["sourceCombatants"]![3]!["slot"] = 1;
        }).With(mainSeed: 0x10000000);
        var chosen = Decide(before, Mage); var reversed = Decide(before.With(actors: before.Actors.Reverse()), Mage);
        Assert.Equal(BattleActionKind.AttackSpell, chosen.ChosenAction!.Value.Kind);
        Assert.Equal(new[] { Wounded, Scout, Wounded }, chosen.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.Target!.Value));
        Assert.Equal(chosen.Target, reversed.Target); Assert.Equal(chosen.Destination, reversed.Destination);
        Assert.Equal(chosen.Effects, reversed.Effects); Assert.Equal(chosen.Battle.ThinkingSeed, reversed.Battle.ThinkingSeed);
        Assert.Equal(0xD0070000u, chosen.Battle.MainSeed); Assert.Equal(chosen.Battle.MainSeed, reversed.Battle.MainSeed);
    }

    [Theory]
    [InlineData(2, true)] [InlineData(8, false)] [InlineData(6, false)]
    public void SourceCommandOrderChoosesHealingOrAttackFromSameChangedValidState(int commandset, bool heal)
    {
        var before = Battle(d => d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = commandset);
        before = before.With(actors: before.Actors.Select(a => a.Actor == Scout ? a.With(position: new(6, 4)) : a));
        var chosen = Decide(before, Cleric);
        Assert.Equal(heal ? Wounded : Scout, chosen.Target);
        Assert.Equal(heal, chosen.ChosenAction?.Kind == BattleActionKind.Healing);
    }

    [Theory]
    [InlineData(30, 7)] [InlineData(21, 7)] [InlineData(20, 2)]
    public void NoNeedOrInsufficientMpFallsThroughHealerCommands(int hp, int mp)
    {
        var before = Battle(d =>
        {
            d["start"]!["actors"]![3]!["hp"] = hp; d["start"]!["actors"]![2]!["mp"] = mp;
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["primaryOrder"] = 255;
        });
        var chosen = Decide(before, Cleric);
        Assert.Null(chosen.ChosenAction); Assert.Null(chosen.Target);
        Assert.Contains(chosen.Effects, e => e.Kind == "ai-command-heal2" && e.After == -1);
        Assert.Contains(chosen.Effects, e => e.Kind == "ai-command-move2");
        Assert.Equal(before.MainSeed, chosen.Battle.MainSeed); Assert.Equal(before.ThinkingSeed, chosen.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData(20, true)] [InlineData(21, false)]
    public void HealingThresholdUsesInclusiveTwoThirds(int hp, bool casts)
    {
        var before = Battle(d => d["start"]!["actors"]![3]!["hp"] = hp);
        Assert.Equal(casts, Decide(before, Cleric).ChosenAction?.Kind == BattleActionKind.Healing);
    }

    [Theory]
    [InlineData(3, 9, 14, false, null)]
    [InlineData(3, 10, 14, false, 1)]
    [InlineData(3, 11, 14, false, 2)]
    [InlineData(3, 11, 14, true, 1)]
    [InlineData(3, 3, 15, false, 1)]
    [InlineData(3, 19, 15, false, 2)]
    [InlineData(3, 20, 15, false, 3)]
    [InlineData(2, 29, 20, false, 1)]
    [InlineData(4, 14, 29, false, 4)]
    [InlineData(4, 13, 29, false, 3)]
    [InlineData(4, 3, 15, false, 3)]
    [InlineData(4, 19, 14, false, null)]
    [InlineData(4, 20, 14, false, 2)]
    [InlineData(4, 20, 14, true, 1)]
    public void EnemyHealUsesIncomingEntryForMissingHpAndAffordability(
        byte entryLevel, byte mp, int missingHp, bool self, int? expected)
        => Assert.Equal(expected, (int?)EnemySpellDecision.HealLevel(entryLevel, mp, missingHp, self));

    [Theory]
    [InlineData(19, 9, 3)]
    [InlineData(16, 14, 4)]
    public void EnemyHealCanSpendItsRemainingMpWhenThePackedLookupApprovesAHigherRank(int hp, int mp, byte rank)
    {
        var battle = Battle(d =>
        {
            var cleric = d["actors"]![2]!;
            cleric["maxHp"] = 47; cleric["maxMp"] = 39;
            d["start"]!["actors"]![2]!["hp"] = hp;
            d["start"]!["actors"]![2]!["mp"] = mp;
            for (int level = 2; level <= 4; level++)
            {
                var spell = d["spells"]![1]!.DeepClone();
                spell["level"] = level;
                spell["mpCost"] = level switch { 2 => 5, 3 => 10, _ => 20 };
                spell["effect"]!["adjustedPower"] = level == 2 ? 15 : 30;
                d["spells"]!.AsArray().Add(spell);
                cleric["spells"]!.AsArray().Add(new JsonObject { ["id"] = "heal", ["level"] = level });
            }
        });
        var rules = RuleCompositions.Sf2();
        var choice = Decide(battle, Cleric, rules);
        var action = new BattleActionRef(BattleActionKind.Healing, new SpellRef("heal", rank));
        Assert.Equal(action, choice.ChosenAction);
        var resolution = BattleActionRules.Prepare(rules.Action(action), battle, Cleric, choice.Destination,
            action, choice.Target, rules.Progression);
        Assert.Equal(0, resolution.ApplySpellCost(battle).GetActor(Cleric).Mp);
    }

    [Fact]
    public void TinyMissingHpDoesNotCastAndHealerCanHealItselfUsingItsCurrentPosition()
    {
        var tiny = Battle(d => { d["actors"]![3]!["maxHp"] = 6; d["start"]!["actors"]![3]!["hp"] = 4; });
        Assert.Null(EnemySpellDecision.Heal(tiny, Cleric, RuleCompositions.Sf2().Actions));
        var hurt = Battle(d => d["start"]!["actors"]![2]!["hp"] = 10);
        var chosen = Decide(hurt, Cleric);
        Assert.Equal(Cleric, chosen.Target); Assert.Equal(hurt.GetActor(Cleric).Position, chosen.Destination);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void HealingPriorityUsesTargetMoverAndStableSourceSlotsInsteadOfHpOrCollectionOrder(bool reverse)
    {
        var before = Battle(d =>
        {
            d["start"]!["actors"]![1]!["hp"] = 20;
            d["encounters"]![0]!["placements"]![1]!["x"] = 5;
            d["encounters"]![0]!["placements"]![1]!["y"] = 4;
        });
        if (reverse) before = before.With(actors: before.Actors.Reverse());
        Assert.Equal(Mage, Decide(before, Cleric).Target); // Mage priority11 exceeds regular2, despite more current HP.
        var tied = Battle(d =>
        {
            d["actors"]![1]!["physical"]!["movementType"] = "regular";
            d["start"]!["actors"]![1]!["hp"] = 20;
            d["encounters"]![0]!["placements"]![1]!["x"] = 5;
            d["encounters"]![0]!["placements"]![1]!["y"] = 4;
        });
        if (reverse) tied = tied.With(actors: tied.Actors.Reverse());
        Assert.Equal(Mage, Decide(tied, Cleric).Target); // Equal priority retains slot128 before130.
    }

    [Fact]
    public void UnreachableHighPriorityTargetIsSkippedAndOccupiedCastingStopsAreNeverUsed()
    {
        var before = Battle(d => d["start"]!["actors"]![1]!["hp"] = 10); // Higher priority but too far away.
        Assert.Equal(Wounded, Decide(before, Cleric).Target);
        var blocked = Battle(d =>
        {
            var rows = d["terrains"]![0]!["rows"]!.AsArray();
            rows[4] = "pppp#ppppp"; rows[5] = "ppp#p#pppp"; rows[6] = "pppp#ppppp";
        });
        Assert.Null(EnemySpellDecision.Heal(blocked, Cleric, RuleCompositions.Sf2().Actions));
        var occupied = before.With(actors: before.Actors.Select(a => a.Actor == Mage ? a.With(position: new(5, 5), hp: 30) : a));
        var chosen = Decide(occupied, Cleric);
        Assert.NotEqual(new MapPosition(5, 5), chosen.Destination);
        Assert.DoesNotContain(occupied.Actors, a => a.Actor != Cleric && a.Hp > 0 && a.Position == chosen.Destination);
    }

    [Fact]
    public void SentryWithoutCandidateStaysWhileAttackerPursuesAndMpFailureCanUsePhysical()
    {
        var before = Battle(d => d["start"]!["actors"]![1]!["mp"] = 1);
        var sentry = Decide(before, Mage); Assert.Null(sentry.Target); Assert.Equal(before.GetActor(Mage).Position, sentry.Destination);
        var attacker = Battle(d =>
        {
            d["start"]!["actors"]![1]!["mp"] = 1;
            d["encounters"]![0]!["placements"]![1]!["initialization"]!["commandset"] = 6;
        });
        Assert.NotEqual(attacker.GetActor(Mage).Position, Decide(attacker, Mage).Destination);
        var near = before.With(actors: before.Actors.Select(a => a.Actor == Mage ? a.With(position: new(3, 2)) : a));
        var physical = Decide(near, Mage); Assert.Equal(Scout, physical.Target); Assert.Null(physical.ChosenAction);
        Assert.DoesNotContain(physical.Effects, e => e.Kind == "rng-ai-attack-category");
    }

    [Fact]
    public void EnemyHealingUsesRealFactionAndSharedPaymentEffectWithoutExperience()
    {
        var before = Battle(); var actor = before.GetActor(Cleric);
        var rule = new Sf2HealingRule();
        Assert.DoesNotContain(Scout, rule.QueryTargets(before, Cleric));
        var chosen = Decide(before, Cleric);
        var prepared = BattleActionRules.Prepare(RuleCompositions.Sf2().Action(chosen.ChosenAction!.Value), before,
            Cleric, chosen.Destination, chosen.ChosenAction.Value, Wounded, new Sf2BattleProgressionRule());
        Assert.Null(prepared.Reward); Assert.Empty(prepared.ConstructionEffects);
        Assert.Null(prepared.FirstAlly);
        Assert.Equal(before.MainSeed, prepared.Prepared.MainSeed);
        Assert.Equal(23, Assert.Single(prepared.Reactions).HpAfter); Assert.Equal(8, before.GetActor(Wounded).Hp);
        Assert.Equal(4, prepared.ApplySpellCost(prepared.Prepared).GetActor(Cleric).Mp);
        Assert.Equal("invalid-heal-target", Assert.Throws<BattleRuleException>(() => rule.RequireTarget(before,
            Cleric, new(3, 2), before.Definition.Spells[new("heal", 1)], Scout)).Code);
        Assert.Equal(actor.Mp, before.GetActor(Cleric).Mp);
    }

    [Fact]
    public void SelectedHealingRuleRejectsBeforeAnyAiMovementResourcesOrRngPublish()
    {
        var rules = new SessionRules("refused-enemy-heal", new RefuseHealing());
        var before = Battle(); var seed = before.MainSeed;
        var error = Assert.Throws<BattleRuleException>(() => Decide(before, Cleric, rules));
        Assert.Equal("selected-healing-refusal", error.Code);
        Assert.Equal(seed, before.MainSeed); Assert.Equal(7, before.GetActor(Cleric).Mp); Assert.Equal(8, before.GetActor(Wounded).Hp);
    }

    private sealed class RefuseHealing : IHealingRule
    {
        private readonly Sf2HealingRule _source = new();
        public string Identity => "refuse-healing";
        public HealingSpellDefinition RequireSpell(EngineBattleState b, ActorRef a, SpellRef s) =>
            throw new BattleRuleException("selected-healing-refusal", "spell", true);
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState b, ActorRef a) => _source.QueryTargets(b, a);
        public BattleActorState RequireTarget(EngineBattleState b, ActorRef a, MapPosition p, HealingSpellDefinition s, ActorRef t) =>
            _source.RequireTarget(b, a, p, s, t);
        public BattleActionResolution Prepare(EngineBattleState b, ActorRef a, MapPosition p, SpellRef s, ActorRef t, IBattleProgressionRule r) =>
            throw new BattleRuleException("selected-healing-refusal", "spell", true);
    }
}
