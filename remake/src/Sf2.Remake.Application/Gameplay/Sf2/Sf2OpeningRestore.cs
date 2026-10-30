using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Finite opening savepoint recipes. No ordinary on-load program is executed here.
internal sealed class Sf2OpeningRestore : IOpeningRestorePolicy
{
    public void ValidateRegisters(OpeningSaveData data)
    {
        // Fresh system initialization uses map0; admitted ordinary defeat uses Map3.
        if (data.EgressMap is not ("map-0" or "map-3"))
            throw new BattleRuleException("save-egress-map", "save.egressMap");
    }

    public bool CanDiscardEntityProgram(ExplorationState world, ExplorationEntity entity)
    {
        var actions = entity.Actions!.Actions;
        if (world.Definition.Entities.Any(row => row.Actions is { } ambient && actions.SequenceEqual(ambient.Actions)))
            return true;
        // The completed Map3 messenger installs wandering with a new origin. Only its
        // already-entered closed random-walk/wait/back-jump cycle can reset. Pending
        // setup and scripted movement cannot be discarded at a savepoint.
        if (world.Map.Value != "map-3") return false;
        for (int index = 0; index + 2 < actions.Count; index++)
            if (actions[index] is RandomWalkEntity && actions[index + 1] is WaitEntityTicks &&
                actions[index + 2] is JumpEntityAction jump && jump.Instruction == index &&
                entity.ActionCursor >= index && entity.ActionCursor <= index + 2)
                return true;
        return false;
    }

    public SaveLocation Select(ScenarioDefinition definition, MapId map, IReadOnlyList<int> flags, FieldReturnAnchor? anchor)
    {
        if (!definition.Exploration!.Maps.TryGetValue(map, out var content)) Fail();
        if (map.Value == "map-3")
        {
            if (content!.Setup is { } setup && MapSetupSelector.Select(setup, setup.DefaultSetup,
                    flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture))) != setup.DefaultSetup)
                Fail();
            return flags.Contains(603)
                ? new("map3-joined", new(map, new(32, 13), 1), "Granseal, outside the church")
                : new("map3-opening", new(map, new(56, 3), 3), "Granseal, Bowie's home");
        }
        if (map.Value == "map-57" && flags.Contains(501) && !flags.Contains(506) &&
            anchor is { Map.Value: "map-57", Facing: 3 })
            return new("battle01-return", anchor, "Ancient Tower, completed battle return");
        Fail(); return null!;
    }

    public ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, out IReadOnlyList<int> enteredFlags)
    {
        var anchor = location.Anchor;
        var map = definition.Exploration!.Maps[anchor.Map];
        var entered = flags.ToHashSet();
        foreach (var write in map.EntryFlags)
            if (write.Value) entered.Add(write.Flag); else entered.Remove(write.Flag);
        enteredFlags = Array.AsReadOnly(entered.Order().ToArray());
        var world = MapTransfer.Build(map, new("entity-0"), anchor.Position, anchor.Facing, 32, party, flags, enteredFlags);
        if (anchor.Map.Value == "map-3")
        {
            // ms_map3 init: joined Sarah retires entity142; otherwise F602 moves Sarah.
            if (flags.Contains(1)) Hide("entity-142", true);
            else if (flags.Contains(602) && world.TryResolveEntity(new("entity-1"), out var sarah))
                world = world.WithEntity(sarah with { Motion = sarah.Motion with
                    { X = 41 * 384, Y = 10 * 384, XDestination = 41 * 384, YDestination = 10 * 384,
                        Facing = 1, XSpeed = 32, YSpeed = 32 } });
            if (flags.Contains(603)) Hide("entity-142", false);
        }
        // Allocation starts followers at the leader. This is the same legal overlap as a map reload;
        // their ordinary following service establishes spacing on the next deliberate input.
        if (world.AllEntities.Any(entity => entity.Visible && entity.Slot != world.PlayerEntity.Slot &&
                entity.Follower is null && entity.Position == anchor.Position))
            throw new BattleRuleException("save-anchor-occupied", "save.anchor");
        return world;

        void Hide(string id, bool aliases)
        {
            if (world.TryResolveEntity(new(id), out var entity) && entity.Entity.Value == id)
                world = world.Hide(entity, aliases);
        }
    }
    private static void Fail() => throw new BattleRuleException("save-location-unavailable", "save.recipe", true);
}
