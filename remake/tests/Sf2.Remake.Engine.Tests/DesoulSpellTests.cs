using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class DesoulSpellTests
{
    private static readonly SpellRef RankOne = new("desoul", 1), RankTwo = new("desoul", 2);
    private static readonly ActorRef Leader = new("leader"), Enemy = new("enemy"), Survivor = new("survivor");

    [Theory]
    [InlineData(0, 2600, 4, false)]
    [InlineData(0, 3200, 5, true)]
    [InlineData(1, 3200, 5, false)]
    [InlineData(1, 3800, 6, true)]
    [InlineData(2, 3800, 6, false)]
    [InlineData(2, 4500, 7, true)]
    [InlineData(3, 4500, 7, false)]
    public void RankOneUsesOneStatusRollAndAwardsOnlyKillExperience(int setting, uint word, int roll, bool hit)
    {
        var battle = Battle(d =>
        {
            Learn(d, RankOne);
            d["terrains"]![0]!["rows"]![2] = "pppppppppp";
            d["actors"]![0]!["level"] = 11;
            d["battlePoison"] = "fixed-two-after-turn";
            d["start"]!["firstAlly"] = "mage";
            d["encounters"]![0]!["outcome"] = new JsonObject { ["leader"] = "priest", ["defeatedGateEnemy"] = "smoke" };
            d["encounters"]![0]!["rewards"]!["halvedExperience"] = true;
            d["actors"]![2]!["resistance"] = (setting << 14) | 0xC0; // FIRE's setting cannot affect STATUS.
        });
        battle = Status(battle, Smoke, 0x300).With(mainSeed: word << 16);
        var action = Cast(battle, RankOne, Smoke);
        var reaction = Assert.Single(action.Reactions);
        var target = battle.GetActor(Smoke);
        Assert.Equal(hit ? BattleReactionKind.InstantDeath : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal(0, reaction.Amount);
        Assert.False(reaction.Critical);
        Assert.Equal(target.Status, reaction.StatusWrite);
        Assert.Equal(new[] { "rng-desoul", "spell-exp-contribution", "rng-exp-plus", "rng-exp-minus" },
            action.ConstructionEffects.Where(e => e.Kind != "gold").Select(e => e.Kind));
        Assert.Equal((ushort?)roll, action.ConstructionEffects[0].RandomValue);
        int kill = BattleRewards.KillExperience(11, false, target.Level);
        Assert.InRange(kill, 10, 40);
        Assert.Equal(hit ? kill : 0, action.ConstructionEffects[1].After);
        var plus = BattleRandom.NextMain(BattleRandom.NextMain(battle.MainSeed, 8).After, 16);
        var minus = BattleRandom.NextMain(plus.After, 16);
        Assert.Equal(minus.After, action.Prepared.MainSeed);
        Assert.Equal(Math.Max(1, (hit ? kill / 2 : 0) + (plus.Value == 0 ? 1 : 0) - (minus.Value == 0 ? 1 : 0)),
            action.Reward!.Amount);

        var paid = action.ApplySpellCost(action.Prepared);
        Assert.Equal(12, paid.GetActor(Mage).Mp);
        Assert.Equal(target.Hp, paid.GetActor(Smoke).Hp);
        Assert.Equal(hit ? 0 : target.Hp, action.ApplyReaction(paid, reaction).GetActor(Smoke).Hp);
        Assert.Equal(battle.Gold + (hit ? target.Definition.Physical!.Gold : 0u), paid.Gold);
    }

    [Fact]
    public void RankTwoSilencePaysWithoutEffectAndInvalidSelectionsPublishNothing()
    {
        var battle = Battle(d =>
        {
            Learn(d, RankTwo);
            d["terrains"]![0]!["rows"]![2] = "pppppppppp";
        });
        Assert.Equal("attack-spell-target", Assert.Throws<BattleRuleException>(() => Cast(battle, RankTwo, Priest)).Code);
        Assert.Equal("spell-not-known", Assert.Throws<BattleRuleException>(() => Cast(battle, RankOne, Smoke)).Code);
        var dry = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Mage ? actor.With(mp: 12) : actor));
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => Cast(dry, RankTwo, Smoke)).Code);
        var cast = Cast(Status(battle, Mage, 0x300), RankTwo, Smoke);
        Assert.NotNull(cast.Silenced);
        Assert.Empty(cast.Reactions);
        Assert.Empty(cast.ConstructionEffects);
        Assert.Null(cast.Reward);
        Assert.Equal(battle.MainSeed, cast.Prepared.MainSeed);
        Assert.Equal(7, cast.ApplySpellCost(cast.Prepared).GetActor(Mage).Mp);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void RankTwoSettlesOrderedHitAndMissThenReturnsWithSharedVictoryAndCustody(bool heldOnceFlag, bool full)
    {
        var session = Start(full ? "battle-drops-full" : "battle-drops", d =>
        {
            var b = d["battle"]!;
            Learn(b, RankTwo);
            b["actors"]![0]!["level"] = 5;
            b["actors"]![0]!["maxMp"] = 20;
            b["start"]!["actors"]![0]!["mp"] = 20;
            b["start"]!["actors"]![2]!["hp"] = 20;
            b["start"]!["mainSeed"] = 64157u << 16; // Nine initiative draws leave word 50024: STATUS 7, drop 0, STATUS 0.
            if (heldOnceFlag) b["start"]!["itemCustody"]!["onceFlags"]!.AsArray().Add(24);
            var other = b["encounters"]![0]!["placements"]![3]!;
            other["x"] = 3;
            other["processingOrder"] = 10;
        });
        Accept(session, new Interact(new("ferryman")));
        SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(RankTwo));
        Accept(session, new SelectTarget(Enemy));
        var committed = Accept(session, new Confirm());

        var rolls = ConstructionRolls(committed).ToArray();
        Assert.Equal(new[] { "rng-desoul", "rng-item-drop", "rng-desoul", "rng-exp-plus", "rng-exp-minus" },
            rolls.Select(e => e.Kind));
        Assert.Equal(Enemy, rolls[0].Target);
        Assert.Equal(Survivor, rolls[2].Target);
        Assert.Equal(new long?[] { 30, 30 }, committed.Observations.Where(e => e.Kind == "spell-exp-contribution").Select(e => e.After));
        var done = FinishBattleScenes(session, committed);
        var events = done.Observations;
        Assert.Single(events, e => e.Kind == "hp" && e.Actor == Enemy && e.Before == 20 && e.After == 0);
        Assert.Single(events, e => e.Kind == "ineffective" && e.Actor == Survivor);
        Assert.DoesNotContain(events, e => e.Kind is "critical" or "rng-reaction-knockback");
        Assert.Single(events, e => e.Kind == "scene-step-started" && e.Detail == "DeathMessage");
        Assert.Equal(heldOnceFlag || full ? 0 : 1, events.Count(e => e.Kind == "item-received"));
        Assert.Equal(full ? 1 : 0, events.Count(e => e.Kind == "item-to-deals"));
        Assert.Equal(heldOnceFlag ? 0 : 1, events.Count(e => e.Kind == "drop-once-set"));
        Assert.Single(events, e => e.Kind == "battle-outcome" && e.Detail == "Victory");

        SettleField(session);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        var party = session.Current.Exploration!.Party;
        var caster = party.Actors.Single(a => a.Actor == Leader);
        Assert.Single(events, e => e.Kind == "mp" && e.Actor == Leader && e.Before == 20 && e.After == 7);
        Assert.Equal((ushort?)2, caster.Kills); // Direct kill plus forced cleanup. Only the direct kill pays.
        Assert.Equal((uint)84, party.Gold);
        Assert.Equal(new byte[] { 24 }, party.ItemCustody!.OnceFlags);
        Assert.Equal(full ? 15 : 14, party.ItemCustody.Deals[110]);
        Assert.Equal(full ? 0 : heldOnceFlag ? 127 : 110, caster.SourceLoadout!.Items[0] & 127);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RankTwoUsesEnemyCountAndCompletesAnySurvivingCasterAfterTurn(bool defeat)
    {
        var session = Start("silence-watch", d =>
        {
            Learn(d, RankTwo);
            d["battlePoison"] = "fixed-two-after-turn";
            d["start"]!["firstAlly"] = "mage";
            d["start"]!["actors"]![0]!["hp"] = 2;
            d["start"]!["actors"]![0]!["status"] = "poison";
            // After the normal initiative draws, the effect inputs are 3800 (6,6) or 4500 (7,4,7).
            d["start"]!["mainSeed"] = (defeat ? 30577u : 64170u) << 16;
            d["actors"]![0]!["level"] = 11;
            for (int y = 1; y <= 3; y++) d["terrains"]![0]!["rows"]![y] = "pppppppppp";
            var encounter = d["encounters"]![0]!;
            encounter["outcome"] = new JsonObject { ["leader"] = "mage", ["defeatedGateEnemy"] = "smoke" };
            var initialization = encounter["initialization"]!;
            initialization["enemyCount"] = defeat ? 3 : 2;
            AddNeighbor("resistant", 129, 1, defeat ? 3 : 0);
            if (defeat) AddNeighbor("susceptible", 130, 3, 0);

            void AddNeighbor(string id, int slot, int y, int resistance)
            {
                var actor = d["actors"]![2]!.DeepClone();
                actor["id"] = id; actor["extraRoundAction"] = false; actor["resistance"] = resistance << 14;
                d["actors"]!.AsArray().Add(actor);
                var placement = encounter["placements"]![2]!.DeepClone();
                placement["actor"] = id; placement["processingOrder"] = slot; placement["y"] = y;
                encounter["placements"]!.AsArray().Add(placement);
                var vitals = d["start"]!["actors"]![2]!.DeepClone();
                vitals["actor"] = id;
                d["start"]!["actors"]!.AsArray().Add(vitals);
                initialization["sourceCombatants"]!.AsArray().Add(new JsonObject { ["slot"] = slot, ["actor"] = id });
            }
        });
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(RankTwo));
        Accept(session, new SelectTarget(Smoke));
        var done = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal(defeat ? BattleOutcomeKind.Defeat : BattleOutcomeKind.Victory, session.Current.BattleResult);
        Assert.Equal((ushort?)2, session.Current.Battle.GetActor(Mage).Kills);
        Assert.Equal((ushort?)(defeat ? 1 : 0), session.Current.Battle.GetActor(Mage).Defeats);
        Assert.Equal((uint?)114, session.Current.Battle.Gold);
        Assert.Equal(2, done.Observations.Count(e => e.Kind == "scene-step-started" && e.Detail == "DeathMessage"));
        Assert.Equal(defeat ? 1 : 0, done.Observations.Count(e => e.Kind == "poison-hp"));
        Assert.Equal(defeat ? new long?[] { 10, 10, 20 } : [10L, 20L],
            done.Observations.Where(e => e.Kind == "spell-exp-contribution").Select(e => e.After));
        Assert.Equal(defeat ? 1 : 0, done.Observations.Count(e => e.Kind == "ineffective" && e.Actor == new ActorRef("resistant")));
        Assert.Null(session.Current.Battle.GetActor(Smoke).Position);
        Assert.Equal(!defeat, session.Current.Battle.GetActor(new("resistant")).Hp == 0);
    }

    private static void Learn(JsonNode battle, SpellRef spell)
    {
        battle["spells"]!.AsArray().Add(new JsonObject { ["id"] = spell.Value, ["level"] = spell.Level,
            ["mpCost"] = spell.Level == 1 ? 8 : 13, ["minimumRange"] = 1, ["maximumRange"] = 2,
            ["effect"] = new JsonObject { ["kind"] = "instant-death", ["radius"] = spell.Level - 1 } });
        battle["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = spell.Value, ["level"] = spell.Level });
    }

    private static BattleActionResolution Cast(EngineBattleState battle, SpellRef rank, ActorRef target)
    {
        var action = new BattleActionRef(BattleActionKind.AttackSpell, rank);
        var rules = RuleCompositions.Sf2();
        return BattleActionRules.Prepare(rules.Action(action), battle, Mage, battle.GetActor(Mage).Position!,
            action, target, rules.Progression);
    }

    private static void SettleField(GameSession session)
    {
        for (int i = 0; i < 2000 && session.Current.Mode == SessionMode.Exploration && session.Current.Story.Wait is { } wait; i++)
            Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(fade.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            });
    }
}
