using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal class Sf2BattleProgressionRule : IBattleProgressionRule
{
    public virtual string Identity => "sf2-progression";
    public virtual int Award(EngineBattleState battle, BattleActorState recipient, BattleActionKind kind,
        IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects)
    {
        int accumulated = 0;
        foreach (var reaction in reactions.Where(reaction => reaction.Actor == recipient.Actor && reaction.Kind != BattleReactionKind.CurseRecoil))
        {
            var target = battle.GetActor(reaction.Target);
            if (kind == BattleActionKind.SupportSpell ||
                kind is BattleActionKind.Healing or BattleActionKind.Item && reaction.Kind is BattleReactionKind.Status or BattleReactionKind.Ineffective)
            {
                int before = accumulated;
                if (reaction.Kind == BattleReactionKind.Status ||
                    reaction.Kind == BattleReactionKind.Resource && target.Faction != recipient.Faction)
                    accumulated = Math.Min(49, accumulated + 5);
                if (kind == BattleActionKind.Healing)
                    effects.Add(new("healing-exp-contribution", recipient.Actor, before, accumulated, Target: reaction.Target));
            }
            else if (kind is BattleActionKind.Physical or BattleActionKind.AttackSpell)
            {
                int before = accumulated;
                int kill = BattleRewards.KillExperience(recipient.Level, recipient.Definition.Physical!.Promoted, target.Level);
                if (reaction.Kind != BattleReactionKind.InstantDeath)
                    accumulated = Math.Min(49, accumulated + BattleRewards.DamageExperience(reaction.Amount, target.MaxHp, kill));
                if (reaction.HpAfter == 0) accumulated = Math.Min(49, accumulated + kill);
                if (kind == BattleActionKind.AttackSpell)
                    effects.Add(new("spell-exp-contribution", recipient.Actor, before, accumulated, Target: reaction.Target));
            }
            else if (recipient.IsAlly && target.MaxHp > 0 &&
                (recipient.Definition.Growth?.ClassId ?? Sf2ClassRules.SourceClass(recipient.Definition.ClassRule)) is 4 or 19 or 20)
            {
                int before = accumulated;
                accumulated = Math.Min(25, accumulated + HealingRules.Experience(reaction.Amount, target.MaxHp));
                if (kind == BattleActionKind.Healing)
                    effects.Add(new("healing-exp-contribution", recipient.Actor, before, accumulated, Target: reaction.Target));
            }
        }
        List<PhysicalRoll> rolls = [];
        bool halve = kind is BattleActionKind.Physical or BattleActionKind.AttackSpell ||
            reactions.Any(r => battle.GetActor(r.Target).Faction != recipient.Faction);
        int award = BattleRewards.Award(accumulated, halve && battle.Definition.Rewards?.HalvedExperience == true, ref seed, rolls);
        effects.AddRange(rolls.Select(roll => new BattleEffect("rng-" + roll.Purpose, recipient.Actor,
            roll.Before, roll.After, roll.Range, roll.Result)));
        return award;
    }
    public uint Gold(uint current, uint award) => BattleRewards.Gold(current, award);
    public ushort Kills(ushort current) => BattleRewards.Kills(current);
    public ushort Defeats(ushort current) => BattleRewards.Defeats(current);
    public BattleActorState Credit(BattleActorState actor, int amount, List<BattleEffect> effects) => BattleGrowthRules.Credit(actor, amount, effects);
    public BattleActorState Grow(BattleActorState actor, ref uint seed, List<BattleEffect> effects) => BattleGrowthRules.Grow(actor, ref seed, effects);
}
