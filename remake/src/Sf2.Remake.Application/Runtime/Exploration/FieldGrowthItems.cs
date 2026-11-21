using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum FieldGrowthStat { Hp, Mp, Attack, Defense, Agility, Move }

public sealed record FieldGrowthGain(FieldGrowthStat Stat, int Amount);
public sealed record FieldGrowthResult(ActorRef Target, IReadOnlyList<FieldGrowthGain> Gains,
    byte? Level = null, SpellRef? Learned = null)
{
    public bool NoEffect => Level is null && Gains.Count == 0;
}

internal static class FieldGrowthItems
{
    internal static bool Admitted(FieldRecoveryEffect effect) =>
        effect is >= FieldRecoveryEffect.IncreaseAttack and <= FieldRecoveryEffect.LevelUp;

    internal static (BattleActorStartInput Actor, FieldGrowthResult Result) Apply(
        PartyMemberDetails target, FieldRecoveryEffect effect, ref uint seed)
    {
        var carried = target.Carried;
        var progress = carried.Progress ??
            throw new BattleRuleException("field-growth-progress", "actor.progress", true);
        var loadout = FieldInventory.Loadout(target);
        var actor = new BattleActorState(target.Deployment!, carried.Hp, carried.Mp, carried.Exp, null,
            carried.Kills, carried.Defeats, status: carried.Status, progress: progress, sourceLoadout: loadout);
        if (effect == FieldRecoveryEffect.LevelUp)
        {
            List<BattleEffect> effects = [];
            var grown = BattleGrowthRules.LevelUp(actor.With(exp: 0), ref seed, effects, out var rawGains);
            if (grown.Level == actor.Level)
                return (carried with { Exp = 0 }, new(actor.Actor, []));
            var learned = effects.SingleOrDefault(effect => effect.Kind == "spell-learned");
            SpellRef? spell = learned is null ? null : actor.Definition.Growth!.SpellDefinitions[(byte)learned.After!.Value];
            var gains = Array.AsReadOnly(rawGains.Select((gain, stat) => new FieldGrowthGain((FieldGrowthStat)stat, gain))
                .Where(gain => gain.Amount != 0).ToArray());
            return (carried with { Exp = 0, Progress = grown.Progress, SourceLoadout = grown.SourceLoadout },
                new(actor.Actor, gains, grown.Level, spell));
        }

        if (effect == FieldRecoveryEffect.IncreaseMp && actor.MaxMp == 0)
            return (carried, new(actor.Actor, []));

        int amount;
        if (effect == FieldRecoveryEffect.IncreaseMove)
            amount = actor.BaseMove switch { 9 => 0, 8 => 1, _ => 2 };
        else
        {
            var draw = BattleRandom.NextMain(seed, 3);
            seed = draw.After;
            amount = draw.Value + 2;
        }

        var current = progress.CurrentStats ?? new(null, null, null);
        FieldGrowthStat stat;
        switch (effect)
        {
            case FieldRecoveryEffect.IncreaseHp:
                stat = FieldGrowthStat.Hp;
                progress = progress with { MaxHp = (ushort)Math.Min(200, actor.MaxHp + amount) };
                carried = carried with { Hp = (ushort)Math.Min(progress.MaxHp, actor.Hp + amount) };
                break;
            case FieldRecoveryEffect.IncreaseMp:
                stat = FieldGrowthStat.Mp;
                progress = progress with { MaxMp = Add(actor.MaxMp, amount) };
                carried = carried with { Mp = Add(actor.Mp, amount, progress.MaxMp) };
                break;
            case FieldRecoveryEffect.IncreaseAttack:
                stat = FieldGrowthStat.Attack;
                // With the admitted static ATT contributions, adding then clamping
                // current ATT equals the projection from the new base.
                progress = progress with { BaseAttack = Add(actor.BaseAttack, amount) };
                break;
            case FieldRecoveryEffect.IncreaseDefense:
                stat = FieldGrowthStat.Defense;
                progress = progress with { Defense = Add(actor.BaseDefense, amount) };
                current = current with { Defense = Changed(Add(actor.Defense, amount),
                    BattleEquipment.Defense(progress.Defense, actor.Status, actor.Definition, loadout)) };
                break;
            case FieldRecoveryEffect.IncreaseAgility:
                stat = FieldGrowthStat.Agility;
                progress = progress with { Agility = Add(actor.BaseAgility, amount, 100) };
                byte packed = (byte)((actor.PackedAgility & 128) | Add(actor.Agility, amount, 127));
                byte packedBase = (byte)(progress.Agility | (actor.Definition.ExtraRoundAction ? 128 : 0));
                current = current with { PackedAgility = Changed(packed,
                    BattleEquipment.PackedAgility(packedBase, actor.Status, actor.Definition, loadout)) };
                break;
            case FieldRecoveryEffect.IncreaseMove:
                stat = FieldGrowthStat.Move;
                byte basis = Add(actor.BaseMove, amount);
                progress = progress with { BaseMove = basis == actor.Definition.Move ? null : basis };
                current = current with { Move = Changed(Add(actor.Move, amount),
                    BattleEquipment.Move(basis, actor.Status, actor.Definition, loadout)) };
                break;
            default:
                throw new BattleRuleException("field-item-effect-unsupported", "item", true);
        }
        return (carried with { Progress = progress with
            { CurrentStats = current is { Defense: null, PackedAgility: null, Move: null } ? null : current } },
            new(actor.Actor, [new(stat, amount)]));
    }

    private static byte Add(byte value, int amount, int maximum = 200) => (byte)Math.Min(maximum, value + amount);
    private static byte? Changed(byte value, byte refreshed) => value == refreshed ? null : value;
}
