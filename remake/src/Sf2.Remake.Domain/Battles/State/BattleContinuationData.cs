using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Finite continuation values, not a new-battle input. Nullable values mean known absence
// (or unknown accounting); disk codecs must encode their presence explicitly.
public sealed record SavedBattleActor(string Actor, MapPosition? Position, MapPosition DeploymentPosition,
    byte Facing, ushort Hp, byte Mp, byte? Exp, ushort? Kills, ushort? Defeats, ushort Status,
    bool Initialized, byte Attack, ushort? ActivationWord, byte AiMemory, byte? PrimaryOrder,
    byte? SecondaryOrder, string? LastTarget, BattleActorProgress? Progress, BattleSourceLoadout? Loadout);
public sealed record SavedBattleTurn(string? Actor, byte AlteredAgility);
public sealed record SavedBattleOptions(byte Difficulty, bool AllyAutoBattle, bool OpponentControl,
    ushort? MissingCandidateAllyWord);
public sealed record SavedBattleRegions(IReadOnlyList<bool> Flags, ushort Tested);
public sealed record SavedBattleVisuals(IReadOnlyDictionary<string, int> Aliases, IReadOnlyList<BattleVisual> Records);
public sealed record BattleContinuationData(string Encounter, IReadOnlyList<SavedBattleActor> Actors,
    uint MainSeed, uint ThinkingSeed, int Round, IReadOnlyList<SavedBattleTurn> Queue, int Cursor,
    uint Gold, SavedBattleOptions Options, SavedBattleRegions? Regions,
    BattleDeathCredit? DeathCredit, BattleItemCustody Custody, SavedBattleVisuals? Visuals);

internal static class BattleContinuationValues
{
    internal static bool SameLoadout(BattleSourceLoadout? left, BattleSourceLoadout? right) =>
        left is null || right is null ? left == right : left.Items.SequenceEqual(right.Items) && left.Spells.SequenceEqual(right.Spells);
    internal static bool SameProgress(BattleActorProgress? left, BattleActorProgress? right) =>
        left is null || right is null ? left == right : left with { Spells = right.Spells, SourceLoadout = right.SourceLoadout } == right &&
            left.Spells.SequenceEqual(right.Spells) && SameLoadout(left.SourceLoadout, right.SourceLoadout);
    internal static bool SameCarried(BattleActorStartInput left, BattleActorStartInput right) =>
        left with { Progress = right.Progress, SourceLoadout = right.SourceLoadout } == right &&
            SameProgress(left.Progress, right.Progress) && SameLoadout(left.SourceLoadout, right.SourceLoadout);
    // Compare only this finite contract. This is admission against accidental reconstruction
    // normalization, never speculative play, JSON comparison, or a general snapshot framework.
    internal static bool Same(BattleContinuationData left, BattleContinuationData right) =>
        left with { Actors = right.Actors, Queue = right.Queue, Regions = right.Regions, Custody = right.Custody, Visuals = right.Visuals } == right &&
        left.Queue.SequenceEqual(right.Queue) && left.Actors.Count == right.Actors.Count &&
        left.Actors.Zip(right.Actors).All(p => p.First with { Progress = p.Second.Progress, Loadout = p.Second.Loadout } == p.Second &&
            SameProgress(p.First.Progress, p.Second.Progress) && SameLoadout(p.First.Loadout, p.Second.Loadout)) &&
        (left.Regions is null || right.Regions is null ? left.Regions == right.Regions :
            left.Regions.Tested == right.Regions.Tested && left.Regions.Flags.SequenceEqual(right.Regions.Flags)) &&
        left.Custody.OnceFlags.SequenceEqual(right.Custody.OnceFlags) && left.Custody.Deals.OrderBy(p => p.Key).SequenceEqual(right.Custody.Deals.OrderBy(p => p.Key)) &&
        (left.Visuals is null || right.Visuals is null ? left.Visuals == right.Visuals :
            left.Visuals.Records.OrderBy(v => v.Slot).SequenceEqual(right.Visuals.Records.OrderBy(v => v.Slot)) &&
            left.Visuals.Aliases.OrderBy(p => p.Key).SequenceEqual(right.Visuals.Aliases.OrderBy(p => p.Key)));

    internal static BattleSourceLoadout? Loadout(BattleSourceLoadout? value) => value is null ? null : new(value.Items, value.Spells);
    internal static BattleActorProgress? Progress(BattleActorProgress? value) => value is null ? null : value with
    { Spells = Array.AsReadOnly(value.Spells.ToArray()), SourceLoadout = Loadout(value.SourceLoadout) };
    internal static BattleActorStartInput Carried(BattleActorStartInput value) => value with
    { Progress = Progress(value.Progress), SourceLoadout = Loadout(value.SourceLoadout) };
    internal static BattleContinuationData Capture(EngineBattleState battle)
    {
        if (battle.Gold is null || battle.ItemCustody is null || battle.StartPolicy is null)
            throw new BattleRuleException("save-battle-progress-unknown", "save.battle");
        var policy = battle.StartPolicy;
        return new(battle.Definition.Encounter, Array.AsReadOnly(battle.Actors.Select(actor => new SavedBattleActor(
            actor.Actor.Value, actor.Position, actor.Deployment.Position, actor.Facing, actor.Hp, actor.Mp,
            actor.Exp, actor.Kills, actor.Defeats, actor.Status, actor.Initialized, actor.Attack,
            actor.ActivationWord, actor.AiMemory, actor.PrimaryOrder, actor.SecondaryOrder, actor.LastTarget?.Value,
            Progress(actor.Progress), Loadout(actor.SourceLoadout))).ToArray()), battle.MainSeed, battle.ThinkingSeed,
            battle.Round, Array.AsReadOnly(battle.Queue.Select(t => new SavedBattleTurn(t.Actor?.Value, t.AlteredAgility)).ToArray()),
            battle.Cursor, battle.Gold.Value, new(policy.Difficulty, policy.AllyAutoBattle, policy.OpponentControl, policy.MissingCandidateAllyWord),
            battle.Regions is null ? null : new(Array.AsReadOnly(battle.Regions.Flags.ToArray()), battle.Regions.Tested),
            battle.DeathCredit is null ? null : battle.DeathCredit with { },
            new(battle.ItemCustody.OnceFlags, battle.ItemCustody.Deals), battle.Visuals is null ? null :
            new(new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(battle.Visuals.Aliases.ToDictionary(p => p.Key.Value, p => p.Value)),
                Array.AsReadOnly(battle.Visuals.Visuals.Values.Select(v => v with { }).ToArray())));
    }
}
