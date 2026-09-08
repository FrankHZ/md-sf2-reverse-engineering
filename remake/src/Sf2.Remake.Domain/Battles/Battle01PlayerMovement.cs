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
    public string Id { get; }
    public byte ClassId { get; }
    public byte MovementType { get; }
    // Bounded classdefs rows4/1 + landEffectSettingsAndMoveCosts rows12/2, pinned c834c652.
    // Low nibble 15 becomes signed -1; the upper nibble is a separate land-effect setting.
    public IReadOnlyList<sbyte> Costs { get; }
    public IReadOnlyList<byte> LandEffects { get; } = Array.AsReadOnly<byte>(
        [0, 1, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
    internal static Battle01MovementProfile? ForClass(byte? classId) => classId switch
    {
        4 => Priest, 1 => Centaur, _ => null,
    };
}

public sealed class Battle01MovementGrid
{
    internal Battle01MovementGrid(byte[] totalCosts, byte[] movableGrid, int[] expansionOrder)
    {
        TotalCosts = Array.AsReadOnly(totalCosts); MovableGrid = Array.AsReadOnly(movableGrid);
        ExpansionOrder = Array.AsReadOnly(expansionOrder);
    }
    public IReadOnlyList<byte> TotalCosts { get; }
    public IReadOnlyList<byte> MovableGrid { get; }
    public IReadOnlyList<int> ExpansionOrder { get; }
    public int ReachableCount => MovableGrid.Count(value => value < 128);
    public int? CostAt(MapPosition position)
    {
        int offset = Battle01PlayerMovement.Offset(position);
        return CostAtOffset(offset);
    }
    internal int? CostAtOffset(int offset) => MovableGrid[offset] >= 128 ? null :
        (MovableGrid[offset] << 8) | TotalCosts[offset];
}

public sealed class Battle01MovementRange
{
    internal Battle01MovementRange(Battle01Combatant actor, IReadOnlyList<int> occupants, byte[] projectedTerrain,
        Battle01MovementGrid grid, Battle01MovementProfile profile)
    {
        ActorIndex = actor.Index; Origin = actor.Position; Budget = actor.Stats.Move * 2;
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

public enum Battle01PlayerMovementStage { Selection, ActionChoice }

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
        Battle01PlayerMovementStage stage)
    {
        Range = range; Preview = preview; Stage = stage;
    }
    public Battle01MovementRange Range { get; }
    public Battle01MovementPath Preview { get; }
    public MapPosition Cursor => Preview.Positions[^1];
    public Battle01PlayerMovementStage Stage { get; }
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
        occupancy[Offset(actor.Position)] = -1; occupancy[target] = actorIndex;
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
            if (opponent.Stats.HpCurrent == 0 || position.X is < 0 or >= 48 || position.Y is < 0 or >= 48) continue;
            terrain[Offset(position)] |= 0x80;
        }
        var grid = BuildWeightedGrid(terrain, profile.Costs, Offset(actor.Position), actor.Stats.Move * 2);
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
        if (terrain.Count != 2304 || moveCosts.Count != 16 || startOffset is < 0 or >= 2304 || budget is < 0 or > 510)
            throw new ArgumentException("Movement requires a complete storage grid, sixteen costs and a bounded MOV*2 budget.", "movement");
        var total = Enumerable.Repeat((byte)255, 2304).ToArray();
        var movable = Enumerable.Repeat((byte)255, 2304).ToArray();
        var heads = Enumerable.Repeat(-1, 32).ToArray();
        var links = Enumerable.Repeat(-1, 2304).ToArray();
        var admitted = new bool[2304]; var expansion = new List<int>();
        int remaining = budget, spent = 0, current = startOffset;
        while (true)
        {
            admitted[current] = true; StoreCost(current, spent); expansion.Add(current);
            foreach (int neighbor in new[] { current + 1, current - 1, current - 48, current + 48 })
            {
                // Deliberately bounds-check before reading, unlike the original unsafe probe chronology.
                if (neighbor is < 0 or >= 2304 || admitted[neighbor]) continue;
                byte entry = terrain[neighbor];
                if ((entry & 0x80) != 0) continue;
                int type = entry & 0x1F;
                if (type >= moveCosts.Count) throw new ArgumentException("Terrain has no admitted movement-cost entry.", "terrain");
                int cost = moveCosts[type];
                if (cost < 0 || cost > remaining) continue;
                admitted[neighbor] = true; // First admission; never relax an already admitted cell.
                if (cost == remaining) { StoreCost(neighbor, spent + cost); continue; }
                int bucket = (remaining - cost) & 31;
                links[neighbor] = heads[bucket]; heads[bucket] = neighbor;
            }
            while (true)
            {
                int bucket = remaining & 31, next = heads[bucket];
                if (next >= 0) { heads[bucket] = links[next]; current = next; break; }
                spent++; remaining--;
                if (remaining <= 0) break;
            }
            if (remaining <= 0) break;
        }
        return new(total, movable, expansion.ToArray());

        void StoreCost(int offset, int cost)
        {
            total[offset] = unchecked((byte)cost); movable[offset] = (byte)(cost >> 8);
        }
    }
}
