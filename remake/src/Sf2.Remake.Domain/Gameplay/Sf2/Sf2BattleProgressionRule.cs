using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal class Sf2BattleProgressionRule : IBattleProgressionRule
{
    public virtual string Identity => "sf2-progression";
    public virtual int Award(EngineBattleState battle, BattleActorState recipient, BattleActionKind kind,
        IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects)
    {
        int accumulated = 0;
        foreach (var reaction in reactions.Where(reaction => reaction.Actor == recipient.Actor))
        {
            var target = battle.GetActor(reaction.Target);
            if (kind == BattleActionKind.Physical)
            {
                int kill = BattleRewards.KillExperience(recipient.Level, recipient.Definition.Physical!.Promoted, target.Level);
                accumulated = Math.Min(49, accumulated + BattleRewards.DamageExperience(reaction.Amount, target.MaxHp, kill));
                if (reaction.HpAfter == 0) accumulated = Math.Min(49, accumulated + kill);
            }
            else if (kind == BattleActionKind.Healing || recipient.Definition.ClassRule == BattleClassRule.UnpromotedPriest)
                accumulated = HealingRules.Experience(reaction.Amount, target.MaxHp);
        }
        List<PhysicalRoll> rolls = [];
        int award = BattleRewards.Award(accumulated, kind == BattleActionKind.Physical && battle.Definition.Rewards!.HalvedExperience, ref seed, rolls);
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
