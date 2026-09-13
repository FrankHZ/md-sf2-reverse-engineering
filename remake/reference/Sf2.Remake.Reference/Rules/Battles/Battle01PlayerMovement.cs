using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed class Battle01MovementProfile
{
    private Battle01MovementProfile(string id, byte classId, byte movementType, sbyte[] costs)
    {
        Id = id; ClassId = classId; MovementType = movementType; Costs = Array.AsReadOnly(costs);
    }
    public static Battle01MovementProfile Priest { get; } = new("battle01-class4-healer12-source-profile-v1", 4, 12,
        [-1, 2, 2, 3, 4, 3, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1]);
    public static Battle01MovementProfile Centaur { get; } = new("battle01-class1-centaur2-source-profile-v1", 1, 2,
        [-1, 2, 2, 3, 5, 5, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1]);
    public static Battle01MovementProfile Regular { get; } = new("battle01-class0-regular1-source-profile-v1", 0, 1,
        [-1, 2, 2, 3, 4, 3, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1]);
    public string Id { get; }
    public byte ClassId { get; }
    public byte MovementType { get; }
    // Bounded classdefs rows4/1/0 + landEffectSettingsAndMoveCosts rows12/2/1, pinned c834c652.
    // Low nibble 15 becomes signed -1; the upper nibble is a separate land-effect setting.
    public IReadOnlyList<sbyte> Costs { get; }
    public IReadOnlyList<byte> LandEffects { get; } = Array.AsReadOnly<byte>(
        [0, 1, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
    internal static Battle01MovementProfile? ForClass(byte? classId) => classId switch
    {
        4 => Priest, 1 => Centaur, 0 => Regular, _ => null,
    };
}

public sealed class Battle01MovementGrid
{
    internal Battle01MovementGrid(byte[] totalCosts, byte[] movableGrid, int[] expansionOrder)
    { Core = new(totalCosts, movableGrid, expansionOrder); }
    internal WeightedMovementGrid Core { get; }
    public IReadOnlyList<byte> TotalCosts => Core.TotalCosts;
    public IReadOnlyList<byte> MovableGrid => Core.MovableGrid;
    public IReadOnlyList<int> ExpansionOrder => Core.ExpansionOrder;
    public int ReachableCount => MovableGrid.Count(value => value < 128);
    public int? CostAt(MapPosition position) => Core.CostAt(position);
    internal int? CostAtOffset(int offset) => Core.CostAtOffset(offset);
}

public sealed class Battle01MovementRange
{
    internal Battle01MovementRange(Battle01Combatant actor, IReadOnlyList<int> occupants, byte[] projectedTerrain,
        Battle01MovementGrid grid, Battle01MovementProfile profile)
    {
        ActorIndex = actor.Index; Origin = actor.RequirePosition(); Budget = actor.Stats.Move * 2;
        EffectiveStatsAtEntry = actor.Stats;
        OriginOccupancy = occupants; ProjectedTerrain = Array.AsReadOnly(projectedTerrain); Grid = grid; Profile = profile;
        LegalDestinations = Array.AsReadOnly(Enumerable.Range(0, 2304).Select(offset => new MapPosition(offset % 48, offset / 48))
            .Where(CanStopAt).ToArray());
    }
    public int ActorIndex { get; }
    internal Battle01Stats EffectiveStatsAtEntry { get; }
    public MapPosition Origin { get; }
    public int Budget { get; }
    public Battle01MovementProfile Profile { get; }
    public Battle01MovementGrid Grid { get; }
    public IReadOnlyList<int> OriginOccupancy { get; }
    public IReadOnlyList<byte> ProjectedTerrain { get; }
    public IReadOnlyList<MapPosition> LegalDestinations { get; }
    public bool CanStopAt(MapPosition position) => Battle01Initialization.WithinArea(position) &&
        Grid.CostAt(position) is not null && OriginOccupancy[Battle01PlayerMovement.Offset(position)] is var occupant &&
        (occupant == -1 || occupant == ActorIndex);
}

public enum Battle01PlayerMovementStage { Selection, ActionChoice, TargetSelection, HealingSpellSelection, HealingTargetSelection }

public sealed class Battle01MovementPath
{
    internal Battle01MovementPath(MapPosition[] positions, byte[] directions, byte[] returnDirections, int cost)
    {
        Positions = Array.AsReadOnly(positions); Directions = Array.AsReadOnly(directions);
        ReturnDirections = Array.AsReadOnly(returnDirections); Cost = cost;
    }
    public string PolicyId => "battle01-controlled-lowest-cost-preview-v1";
    public IReadOnlyList<MapPosition> Positions { get; }
    public IReadOnlyList<byte> Directions { get; }
    public IReadOnlyList<byte> ReturnDirections { get; }
    public int Cost { get; }
}

public sealed class Battle01PlayerMovementSelection
{
    internal Battle01PlayerMovementSelection(Battle01MovementRange range, Battle01MovementPath preview,
        Battle01PlayerMovementStage stage, Battle01PlayerAttackSelection? attack = null, Battle01PlayerHealingSelection? healing = null)
    {
        Range = range; Preview = preview; Stage = stage; Attack = attack; Healing = healing;
    }
    public Battle01MovementRange Range { get; }
    public Battle01MovementPath Preview { get; }
    public MapPosition Cursor => Preview.Positions[^1];
    public Battle01PlayerMovementStage Stage { get; }
    public Battle01PlayerAttackSelection? Attack { get; }
    public Battle01PlayerHealingSelection? Healing { get; }
    public int GridCost => Range.Grid.CostAt(Cursor)!.Value;
    public bool CanConfirm => Range.CanStopAt(Cursor);
}

public static class Battle01PlayerMovement
{
    // Pinned c834c652 landEffectSettingsAndMoveCosts row6: enemy Hovering, not Flying5.
    // Standby uses raw terrain propagation, separately from the player range/preview policy.
    internal static IReadOnlyList<sbyte> HoveringCosts { get; } = Array.AsReadOnly<sbyte>(
        [2, 2, 2, 2, 2, 2, 2, -1, 2, -1, -1, -1, -1, -1, -1, -1]);
    public static Battle01InitializedState SelectDestination(Battle01InitializedState current, int actorIndex, MapPosition destination)
    {
        var control = RequireControl(current, actorIndex, allowActionChoice: false);
        if (destination == control.Movement.Cursor)
            throw new ArgumentException("The cursor already selects that destination.", "destination");
        var preview = CreatePreview(control.Movement.Range, destination);
        var movement = new Battle01PlayerMovementSelection(control.Movement.Range, preview, Battle01PlayerMovementStage.Selection);
        return new(current, current.Roster.ToArray(), current.Occupancy, control.WithMovement(movement));
    }

    public static Battle01InitializedState Confirm(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, allowActionChoice: false);
        if (!control.Movement.CanConfirm)
            throw new ArgumentException("Another combatant occupies the selected destination.", "destination");
        var movement = new Battle01PlayerMovementSelection(control.Movement.Range, control.Movement.Preview,
            Battle01PlayerMovementStage.ActionChoice);
        return Relocate(current, actorIndex, movement.Cursor, control.WithMovement(movement));
    }

    public static Battle01InitializedState Cancel(Battle01InitializedState current, int actorIndex)
    {
        var control = RequireControl(current, actorIndex, allowActionChoice: true);
        var origin = control.Movement.Range.Origin;
        if (control.Movement.Stage == Battle01PlayerMovementStage.Selection && control.Movement.Cursor == origin)
            throw new ArgumentException("No selection or provisional relocation remains to cancel.", "cancel");
        var movement = new Battle01PlayerMovementSelection(control.Movement.Range,
            CreatePreview(control.Movement.Range, origin), Battle01PlayerMovementStage.Selection);
        // Controlled API restoration only. This does not execute the original return animation or menu.
        return Relocate(current, actorIndex, origin, control.WithMovement(movement));
    }

    private static Battle01FirstControlState RequireControl(Battle01InitializedState current, int actorIndex, bool allowActionChoice)
    {
        ArgumentNullException.ThrowIfNull(current);
        var control = current.FirstControl;
        if (control is null || (current.Phase != Battle01Phase.PlayerMovementSelection &&
                !(allowActionChoice && current.Phase == Battle01Phase.PlayerActionChoice)))
            throw new ArgumentException("This operation requires the active player selection phase.", "phase");
        if (control.ActorIndex != actorIndex)
            throw new ArgumentException("The request must name the current controlled actor.", "actor");
        return control;
    }

    private static Battle01InitializedState Relocate(Battle01InitializedState current, int actorIndex,
        MapPosition destination, Battle01FirstControlState control)
    {
        var roster = current.Roster.ToArray(); int index = Array.FindIndex(roster, unit => unit.Index == actorIndex);
        var actor = roster[index]; var occupancy = current.Occupancy.ToArray(); int target = Offset(destination);
        if (occupancy[target] != -1 && occupancy[target] != actorIndex)
            throw new ArgumentException("The relocation destination is occupied.", "destination");
        occupancy[Offset(actor.RequirePosition())] = -1; occupancy[target] = actorIndex;
        roster[index] = actor.WithPosition(destination);
        return new(current, roster, Array.AsReadOnly(occupancy), control);
    }

    internal static Battle01MovementPath CreatePreview(Battle01MovementRange range, MapPosition destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!Battle01Initialization.WithinArea(destination) || range.Grid.CostAt(destination) is not { } destinationCost)
            throw new ArgumentException("The selected tile is outside this scene or its movement range.", "destination");
        int current = Offset(destination), origin = Offset(range.Origin), previousDirection = -1;
        var backPositions = new List<MapPosition> { destination }; var backDirections = new List<byte>();
        while (current != origin)
        {
            int currentCost = range.Grid.CostAtOffset(current)!.Value;
            var eligible = new List<(int Offset, byte Direction, int Cost)>();
            // Probe order is right,left,up,down. The controlled policy keeps only the lowest-cost set.
            foreach (var (delta, direction) in new (int, byte)[] { (1, 0), (-1, 2), (-48, 1), (48, 3) })
            {
                int neighbor = current + delta;
                if (neighbor is < 0 or >= 2304) continue;
                var position = new MapPosition(neighbor % 48, neighbor / 48);
                // Grid propagation preserves flat wrap; this scene-bounded preview never replays a wrap as a tile step.
                if (!Battle01Initialization.WithinArea(position) ||
                    Math.Abs(position.X - current % 48) + Math.Abs(position.Y - current / 48) != 1) continue;
                if (range.Grid.CostAtOffset(neighbor) is { } cost && cost < currentCost)
                    eligible.Add((neighbor, direction, cost));
            }
            if (eligible.Count == 0) throw new ArgumentException("No complete bounded preview reaches the origin.", "path");
            int minimum = eligible.Min(candidate => candidate.Cost);
            var lowest = eligible.Where(candidate => candidate.Cost == minimum).OrderBy(candidate => candidate.Direction).ToArray();
            var selected = lowest.FirstOrDefault(candidate => candidate.Direction != previousDirection, lowest[0]);
            current = selected.Offset; previousDirection = selected.Direction;
            backDirections.Add(selected.Direction); backPositions.Add(new(current % 48, current / 48));
        }
        var positions = backPositions.AsEnumerable().Reverse().ToArray();
        int actualCost = positions.Skip(1).Sum(position => (int)range.Profile.Costs[range.ProjectedTerrain[Offset(position)] & 0x1F]);
        if (actualCost > destinationCost || actualCost > range.Budget)
            throw new ArgumentException("The complete preview exceeds its reported grid cost or movement budget.", "path");
        return new(positions, backDirections.AsEnumerable().Reverse().Select(direction => (byte)(direction ^ 2)).Append((byte)255).ToArray(),
            backDirections.Append((byte)255).ToArray(), actualCost);
    }

    internal static Battle01MovementRange CreateRange(Battle01InitializedState battle, Battle01Combatant actor)
    {
        var profile = Battle01MovementProfile.ForClass(actor.ClassId) ??
            throw new ArgumentException("Only the admitted PRST/HEALER and KNTE/CENTAUR movement profiles are implemented.", "movementProfile");
        var terrain = battle.Terrain.ToArray();
        foreach (var opponent in battle.Roster.Where(unit => (unit.Index < 128) != (actor.Index < 128)))
        {
            var position = opponent.Position;
            if (opponent.Stats.HpCurrent == 0 || position is null || position.X is < 0 or >= 48 || position.Y is < 0 or >= 48) continue;
            terrain[Offset(position)] |= 0x80;
        }
        var grid = BuildWeightedGrid(terrain, profile.Costs, Offset(actor.RequirePosition()), actor.Stats.Move * 2);
        // Player destination policy stays inside this scene's 16x20 area; propagation retains stride48.
        return new(actor, battle.Occupancy, terrain, grid, profile);
    }

    internal static int Offset(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        if (position.X is < 0 or >= 48 || position.Y is < 0 or >= 48)
            throw new ArgumentException("Movement coordinates must address the 48-by-48 storage grid.", nameof(position));
        return position.Y * 48 + position.X;
    }

    // Consumes the movement-only terrain projection; current battle terrain/occupancy stay distinct.
    // Matches the accepted project-owned weighted model, including flat neighbors and bucket wrap.
    internal static Battle01MovementGrid BuildWeightedGrid(IReadOnlyList<byte> terrain,
        IReadOnlyList<sbyte> moveCosts, int startOffset, int budget)
    {
        var grid = WeightedMovement.Build(terrain, moveCosts, startOffset, budget, preserveFlatRowNeighbors: true);
        return new(grid.TotalCosts.ToArray(), grid.MovableGrid.ToArray(), grid.ExpansionOrder.ToArray());
    }
}
