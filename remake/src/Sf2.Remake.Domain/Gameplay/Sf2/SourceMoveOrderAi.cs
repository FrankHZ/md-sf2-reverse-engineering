using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Source command ordering with per-decision temporary obstruction grids. Persistent terrain
// and MOV are never modified. All propagation and path reconstruction use existing owners.
internal sealed class SourceMoveOrderAi(EngineBattleState battle, ActorRef actorRef, IPhysicalActionRule physical)
{
    private EngineBattleState _battle = battle;
    private BattleActorState Actor => _battle.GetActor(actorRef);
    private readonly List<BattleEffect> _effects = [];
    private bool[] _blocked = new bool[2304];
    private byte Primary => Actor.PrimaryOrder ?? throw Missing("source-ai-orders");

    internal BattleAutomaticAction Resolve(int commandset)
    {
        int mode = commandset == 14 ? 2 : commandset == 9 ? 1 : 0;
        Restrict(mode);
        int first = commandset switch { 14 => 5, 9 => 3, _ => 1 };
        if (MoveOrder(first, mode, follow: first == 3) is { } move) return Complete(move);
        int attack = commandset == 14 ? 2 : commandset == 9 ? 3 : 1;
        if (EnemyPhysicalDecision.TryResolve(_battle, actorRef, physical, Grid()) is { } action)
        {
            _effects.Add(new("ai-command-attack" + attack, actorRef, After: 0));
            return Complete(action);
        }
        _effects.Add(new("ai-command-attack" + attack, actorRef, After: -1));
        _effects.Add(new("ai-command-heal" + (commandset == 14 ? 3 : commandset == 9 ? 2 : 1), actorRef, After: -1));
        _effects.Add(new("ai-command-support", actorRef, After: -1));
        if (commandset is 9 or 14 && MoveOrder(commandset == 9 ? 4 : 2, mode, follow: commandset == 14) is { } fallback)
            return Complete(fallback);
        Restrict(mode);
        var targets = _battle.Actors.Where(unit => unit.Hp > 0 && unit.Position is not null && unit.Faction != Actor.Faction)
            .OrderBy(unit => unit.ProcessingOrder).ToArray();
        if (targets.Length == 0)
        {
            _effects.Add(new("ai-command-move" + (mode + 1), actorRef, After: -1));
            _effects.Add(new("ai-stay", actorRef));
            return Complete(new(_battle, [], Actor.Position!, [Actor.Position!]));
        }
        // Source scores before clearing temporary flags, approaches on unrestricted terrain,
        // then reapplies the order restriction and opposing occupancy for its actual stop.
        var scores = WeightedMovement.Build(Costs(), Offset(Actor.Position!), 128);
        _blocked = new bool[2304];
        var approachCosts = Costs();
        Restrict(mode);
        var pursuit = AiMovementRules.PursueOrdered(approachCosts, scores, Grid(), Actor.Position!, targets,
            Actor.Definition.Mover is BattleMover.Hovering or BattleMover.Flying ? PhysicalPriorityTable.Flying : PhysicalPriorityTable.Regular,
            Occupied, _battle.Definition.Width, _battle.Definition.Height);
        _effects.Add(new("ai-move-target", actorRef, pursuit.TargetCosts[pursuit.TargetIndex], pursuit.Cost,
            Target: targets[pursuit.TargetIndex].Actor));
        _effects.Add(new("ai-command-move" + (mode + 1), actorRef, After: 0));
        return Complete(new(_battle, [], pursuit.Destination, BattleMovement.Route(Actor.Position!, pursuit.MoveString)));
    }

    private BattleAutomaticAction? MoveOrder(int command, int mode, bool follow)
    {
        BattleAutomaticAction? result = null;
        var target = Target();
        if (target is not null && (!follow || _blocked[Offset(Actor.Position!)]))
        {
            // The point command first checks the current/adjacent tile in the allied-occupancy grid.
            var nearby = follow ? null : AiMovementRules.AttackPosition(Grid(allies: true), Actor.Position!, 0, Occupied) ??
                AiMovementRules.AttackPosition(Grid(allies: true), Actor.Position!, 1, Occupied);
            // The source tests returned X with tst.b, not the -1 failure sentinel.
            // X=0 skips this command; exhausted search (-1) still attempts it.
            bool position = follow || nearby?.X != 0;
            if (position)
            {
                Quadrant(target);
                result = EnemyPhysicalDecision.TryResolve(_battle, actorRef, physical, Grid());
                if (result is null)
                {
                    _blocked = new bool[2304];
                    var reverse = WeightedMovement.Build(Costs(), Offset(target), 128);
                    if (reverse.CostAt(Actor.Position!) is { } distance && distance > 0)
                    {
                        var preliminary = BattleMovement.Walk(reverse, Actor.Position!, Math.Max(0, distance - Actor.Definition.Move * 2),
                            _battle.Definition.Width, _battle.Definition.Height);
                        var legal = Grid(allies: true);
                        MapPosition? destination = null;
                        for (int radius = 0; radius <= 3 && destination is null; radius++)
                            destination = AiMovementRules.AttackPosition(legal, preliminary.Destination, radius, Occupied);
                        if (destination is not null && destination != Actor.Position)
                            result = new(_battle, [], destination, BattleMovement.Route(Actor.Position!,
                                BattleMovement.MoveString(legal, Actor.Position!, destination, _battle.Definition.Width, _battle.Definition.Height)));
                    }
                }
            }
        }
        Restrict(mode);
        _effects.Add(new("ai-command-move-order" + command, actorRef, After: result is null ? -1 : 0));
        return result;
    }

    private void Restrict(int mode)
    {
        var target = Target();
        if (target is null || mode == 0) return;
        if (mode == 1)
        {
            var reachable = WeightedMovement.Build(Costs(), Offset(target), Actor.Definition.Move * 2);
            for (int i = 0; i < _blocked.Length; i++) _blocked[i] |= reachable.CostAtOffset(i) is null;
        }
        else
        {
            // The source last-target branch still obtains the center from the move order.
            int radius = Actor.LastTarget is null ? 2 : 4;
            for (int i = 0; i < _blocked.Length; i++)
                _blocked[i] = Math.Abs(i % 48 - target.X) + Math.Abs(i / 48 - target.Y) > radius;
        }
    }

    private void Quadrant(MapPosition target)
    {
        var origin = Actor.Position!;
        bool left = target.X < origin.X, below = target.Y >= origin.Y;
        int x = Math.Clamp(origin.X + (left ? 4 : -4), 0, 47);
        int y = Math.Clamp(origin.Y + (below ? -4 : 4), 0, 47);
        for (int i = 0; i < _blocked.Length; i++)
            _blocked[i] = (left ? i % 48 >= x : i % 48 <= x) || (below ? i / 48 <= y : i / 48 >= y);
    }

    private MapPosition? Target()
    {
        if (Primary == 255) return null;
        if ((Primary & 64) == 0) { var followed = Follow(Primary); return followed.Hp == 0 ? null : followed.Position; }
        var points = _battle.Definition.Initialization!.AiPoints;
        int index = Primary & 15;
        if (index >= points.Count) throw Missing("source-ai-point");
        return points[index];
    }
    private BattleActorState Follow(byte index)
    {
        if (!_battle.Definition.Initialization!.SourceCombatants.TryGetValue(index, out var id)) throw Missing("source-follow-binding");
        return _battle.GetActor(id);
    }
    private sbyte[] Costs() => _battle.Definition.Terrain.Select((tile, index) => _blocked[index] ? (sbyte)-1 :
        BattleTerrainRules.MovementCost(tile, Actor.Definition.Mover)).ToArray();
    private WeightedMovementGrid Grid(bool allies = false)
    {
        var costs = Costs();
        foreach (var unit in _battle.Actors.Where(unit => unit.Hp > 0 && unit.Position is not null &&
            (allies ? unit.Faction == Actor.Faction : unit.Faction != Actor.Faction))) costs[Offset(unit.Position!)] = -1;
        return WeightedMovement.Build(costs, Offset(Actor.Position!), Actor.Definition.Move * 2);
    }
    private bool Occupied(MapPosition position) => _battle.Actors.Any(unit => unit.Hp > 0 && unit.Position == position);
    private BattleAutomaticAction Complete(BattleAutomaticAction result) => result with { Effects = Array.AsReadOnly<BattleEffect>([.. _effects, .. result.Effects]) };
    private static int Offset(MapPosition position) => position.Y * 48 + position.X;
    private static BattleRuleException Missing(string code) => new(code, "placements.orders", true);
}
