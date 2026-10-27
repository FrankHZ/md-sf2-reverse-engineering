using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// A physical slot is presentation identity, never a combatant or a Godot node.
public sealed record BattleVisual(int Slot, ActorRef Appearance, MapPosition Position, byte Facing);
public sealed class BattleVisualState
{
    internal BattleVisualState(IReadOnlyDictionary<ActorRef, int> aliases, IEnumerable<BattleVisual> visuals)
    {
        Aliases = new ReadOnlyDictionary<ActorRef, int>(aliases.ToDictionary(p => p.Key, p => p.Value));
        Visuals = new ReadOnlyDictionary<int, BattleVisual>(visuals.ToDictionary(v => v.Slot));
    }
    public IReadOnlyDictionary<ActorRef, int> Aliases { get; }
    public IReadOnlyDictionary<int, BattleVisual> Visuals { get; }
    public BattleVisual Resolve(ActorRef actor) => Aliases.TryGetValue(actor, out int slot) &&
        slot is >= 0 and < 49 && Visuals.TryGetValue(slot, out var visual) ? visual :
        throw new BattleRuleException("battle-visual-alias", "battle.visuals", true);
    // Only initial placement and delivered movement update the physical slot.
    // Logical conversion coordinates must not move the ally's old physical slot.
    internal BattleVisualState Place(ActorRef actor, MapPosition position, byte? facing = null)
    {
        if (!Aliases.TryGetValue(actor, out int slot) || !Visuals.TryGetValue(slot, out var visual)) return this;
        var changed = visual with { Position = position, Facing = facing ?? visual.Facing };
        return changed == visual ? this : new(Aliases, Visuals.Values.Select(v => v.Slot == slot ? changed : v));
    }
    internal BattleVisualState ProjectStart(IEnumerable<BattleActorState> actors)
    {
        var placed = actors.Where(a => a.Position is not null && Aliases.TryGetValue(a.Actor, out int slot) && Visuals.ContainsKey(slot)).ToArray();
        if (placed.GroupBy(a => Aliases[a.Actor]).Any(group => group.Select(a => a.Position).Distinct().Count() > 1))
            throw new BattleRuleException("ambiguous-visual-position", "start.story.aliases", true);
        var result = this;
        foreach (var actor in placed) result = result.Place(actor.Actor, actor.Position!);
        return result;
    }
    internal BattleVisualState CopyAlias(ActorRef enemy, ActorRef ally)
    {
        var visual = Resolve(enemy);
        var aliases = Aliases.ToDictionary(p => p.Key, p => p.Value); aliases[ally] = visual.Slot;
        return new(aliases, Visuals.Values);
    }
    internal BattleVisualState Face(ActorRef actor, byte facing)
    {
        var selected = Resolve(actor);
        return new(Aliases, Visuals.Values.Select(v => v.Slot == selected.Slot ? v with { Facing = facing } : v));
    }
}
