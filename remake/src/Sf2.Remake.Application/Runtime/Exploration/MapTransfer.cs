using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class MapTransfer
{
    // Apply is a pure snapshot construction. Reuse its full admission before consuming a
    // field action; publish again after the old-world pass with its resulting party seed.
    internal static void Validate(ScenarioDefinition definition, SessionSnapshot current, ExplorationState candidate,
        ExplorationEvent warp)
    {
        var preview = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(MapEventDispatcher.Roof(candidate)), current.Story, current.StopReason);
        _ = Apply(definition, preview, warp.DestinationMap!, warp.Destination!, warp.Facing,
            warp.LoadMode, current.Story, []);
        if (SelectsBattle(current, definition.Exploration!.Maps[warp.DestinationMap!])) return;
        ValidateDisplay(current.Story.Display);
        var target = definition.Exploration.Maps[warp.DestinationMap!];
        if (warp.LoadMode == MapLoadMode.Rebuild && target.BasePalette is not { Valid: true })
            throw new BattleRuleException("warp-palette-binding", "map.basePalette", true);
        // Reject statically known unsupported display effects before consuming the move.
        var visited = new HashSet<string>();
        void Check(ProgramLocation? location)
        {
            if (location is not { } entry || !visited.Add(entry.Program)) return;
            foreach (var instruction in definition.Exploration.Programs[entry.Program].Instructions)
                switch (instruction)
                {
                    case PresentCue cue when IsPalette(cue): ValidateCue(cue); break;
                    case LoadSceneMap: throw new BattleRuleException("ordinary-warp-scene-load", "program.map", true);
                    case CallProgram call: Check(call.Target); break;
                    case JumpProgram jump: Check(jump.Target); break;
                    case BranchFlag branch: Check(branch.Target); break;
                    case BranchEntityCoordinates branch: Check(branch.Target); break;
                }
        }
        Check(target.OnLoad);
    }

    private static bool SelectsBattle(SessionSnapshot current, ExplorationMapDefinition target) =>
        target.Battle is { } battle &&
        (battle.UnlockedFlag is null || current.Story.Flags.Contains(battle.UnlockedFlag.Value)) &&
        (battle.CompletedFlag is null || !current.Story.Flags.Contains(battle.CompletedFlag.Value));

    internal static void ValidateDisplay(ExplorationDisplay? display)
    {
        if (display is not { Period: > 0, Base.Valid: true, Current.Valid: true } ||
            !Enum.IsDefined(display.Visibility) ||
            display.Visibility == FullFadeVisibility.Transitioning ||
            (display.Visibility == FullFadeVisibility.Black ? !display.Current.Black : display.Current != display.Base))
            throw new BattleRuleException("full-fade-state", "story.display", true);
    }

    internal static bool IsPalette(PresentCue cue) => cue.Kind is PresentationCueKind.FadeIn or
        PresentationCueKind.FadeOut or PresentationCueKind.FlashWhite or PresentationCueKind.RestorePalette;

    internal static void ValidateCue(PresentCue cue)
    {
        if (cue.FullBlack is null || cue.Resource != "black" ||
            cue.Kind is not (PresentationCueKind.FadeIn or PresentationCueKind.FadeOut) || cue.FullBlack.Period == 0)
            throw new BattleRuleException("full-black-fade-binding", "program.presentation", true);
    }

    internal static SessionSnapshot BeginWarp(ScenarioDefinition definition, SessionSnapshot current,
        ExplorationEvent warp, List<SessionObservation> observations)
    {
        if (SelectsBattle(current, definition.Exploration!.Maps[warp.DestinationMap!]))
            return Apply(definition, current, warp.DestinationMap!, warp.Destination!, warp.Facing,
                warp.LoadMode, current.Story, observations);
        var story = current.Story.Copy(null, warp: new(warp.DestinationMap!, warp.Destination!, warp.Facing, warp.LoadMode));
        var world = current.Exploration!;
        current = new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(world.PlayerEntity with { Actions = null, ActionCursor = 0, WaitingForMotion = false })),
            story, current.StopReason);
        return BeginFade(current, story, PresentationCueKind.FadeOut, FullFadePurpose.WarpOut, null, observations);
    }

    internal static SessionSnapshot BeginFade(SessionSnapshot current, StoryState story, PresentationCueKind kind,
        FullFadePurpose purpose, byte? temporaryPeriod, List<SessionObservation> observations)
    {
        ValidateDisplay(story.Display);
        if (temporaryPeriod == 0) throw new BattleRuleException("full-fade-period", "program.period", true);
        var display = story.Display!;
        byte period = temporaryPeriod ?? display.Period;
        var wait = new FullFadeWait(new(current.ObservationSequence + 1), kind, purpose, period, period,
            ExtraServices: purpose == FullFadePurpose.WarpOut ? 0 : 1,
            RestorePeriod: temporaryPeriod is null ? null : display.Period);
        return ProgramRunner.Commit(current, current.Active, story.Copy(story.Cursor, wait,
            display: display with { Period = period, Visibility = FullFadeVisibility.Transitioning }), observations, "full-fade-started", kind.ToString());
    }

    // One enabled VInt: fade first, then the physical entity slots, even on the terminator.
    // This is the helper's finite service rule, not a quota for CPU-time opportunities.
    internal static SessionSnapshot FadeTick(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var wait = (FullFadeWait)current.Story.Wait!;
        if (wait.LogicalDone) return current;
        var display = current.Story.Display!;
        if (wait.Entry == 8)
            wait = wait with { ExtraServices = wait.ExtraServices - 1, LogicalDone = true };
        else if (wait.Countdown > 1) wait = wait with { Countdown = (byte)(wait.Countdown - 1) };
        else
        {
            int entry = wait.Entry;
            if (entry < 7)
            {
                int offset = wait.Kind == PresentationCueKind.FadeIn ? entry - 6 : -entry - 1;
                ushort Color(ushort value) => (ushort)(Math.Clamp((value & 14) + 2 * offset, 0, 14) |
                    (Math.Clamp(((value >> 4) & 14) + 2 * offset, 0, 14) << 4) |
                    (Math.Clamp(((value >> 8) & 14) + 2 * offset, 0, 14) << 8));
                display = display with { Current = new(Color(display.Base.Color2), Color(display.Base.Color3)) };
            }
            wait = wait with { Entry = entry + 1, Countdown = wait.Period,
                LogicalDone = entry == 7 && wait.ExtraServices == 0 };
        }
        if (wait.LogicalDone)
            display = display with { Visibility = wait.Kind == PresentationCueKind.FadeOut ? FullFadeVisibility.Black : FullFadeVisibility.BaseRestored,
                Period = wait.RestorePeriod ?? display.Period };
        var story = current.Story.Copy(current.Story.Cursor, wait, display: display,
            simulationTick: checked(current.Story.SimulationTick + 1));
        // ExecuteFading honors the current program's enabled entity context; ordinary
        // ExplorationLoop has the field context installed for both fades.
        bool entities = wait.Purpose != FullFadePurpose.Script || story.Cursor is not { } cursor ||
            definition.Exploration!.Programs[cursor.Program].EntitiesRunning;
        var tick = entities ? EntityActionRunner.Tick(current.Exploration!, storyFlags: story.Flags) : null;
        current = ProgramRunner.Commit(current, tick is null ? current.Active : new ActiveExploration(MapEventDispatcher.Roof(tick.World)),
            story, observations, "fade-service", wait.Entry.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (tick?.Failure is { } failure) throw new FadeServiceFailure(current, failure);
        return FinishFade(current, observations);
    }

    internal sealed class FadeServiceFailure(SessionSnapshot snapshot, BattleRuleException failure) : Exception
    { internal SessionSnapshot Snapshot { get; } = snapshot; internal BattleRuleException Failure { get; } = failure; }

    internal static SessionSnapshot FinishFade(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.Wait is not FullFadeWait { LogicalDone: true, ActualDone: true } wait) return current;
        var story = current.Story;
        story = wait.Purpose switch
        {
            FullFadePurpose.WarpOut => story.Copy(null, new WarpLoadWait(new(current.ObservationSequence + 1))),
            FullFadePurpose.Script => story.Copy(story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null),
            _ => story.Copy(null, clearWarp: true),
        };
        return ProgramRunner.Commit(current, current.Active, story, observations, "full-fade-completed", wait.Kind.ToString());
    }

    internal static SessionSnapshot LoadTick(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var wait = (WarpLoadWait)current.Story.Wait!;
        var story = current.Story.Copy(null, wait with { Remaining = wait.Remaining - 1 },
            simulationTick: checked(current.Story.SimulationTick + 1));
        current = ProgramRunner.Commit(current, current.Active, story, observations, "map-load-service");
        if (wait.Remaining > 1) return current;
        var warp = story.Warp!;
        try { return Apply(definition, current, warp.Map, warp.Position, warp.Facing, warp.Mode, story.Copy(null), observations); }
        catch (BattleRuleException error) { throw new FadeServiceFailure(current, error); }
    }

    internal static SessionSnapshot ReturnVisible(SessionSnapshot current, List<SessionObservation> observations)
    {
        var display = current.Story.Display!;
        if (display.Base != display.Current)
            return BeginFade(current, current.Story.Copy(null), PresentationCueKind.FadeIn, FullFadePurpose.WarpIn, null, observations);
        if (display.Visibility != FullFadeVisibility.BaseRestored)
            throw new BattleRuleException("equal-palette-still-black", "story.display", true);
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(null, clearWarp: true), observations, "warp-visible");
    }

    internal static ExplorationState Build(ExplorationMapDefinition map, EntityRef player, MapPosition position,
        byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags, IReadOnlyList<int>? layoutFlags = null)
    {
        ValidateSetup(map, flags);
        var layout = map.Layout;
        foreach (var copy in map.LayoutEvents?.Flags ?? [])
            if ((layoutFlags ?? flags).Contains(copy.Flag)) layout = layout.ApplyBlockCopy(copy.Copy);
        if (!map.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(layout, position))
            throw new BattleRuleException("map-entry-position", "map.position");
        if (map.Population is { } population)
        {
            return MapEventDispatcher.RoofOnLoad(SceneEntities.Build(map, layout, player, position, facing,
                speed, party, flags, population, map.Entities));
        }
        var entities = map.Entities.Where(entity => entity.Entity != player).Select(entity =>
            new ExplorationEntity(entity.Entity, EntityMotionState.At(entity.Position, entity.Facing, entity.Speed) with
                { FlagsA = entity.Obstruction ? (byte)0x20 : (byte)0 }, entity.Visible, entity.Actions, Sprite: entity.Sprite)).ToList();
        entities.Insert(0, new(player, EntityMotionState.At(position, facing, speed), true));
        return MapEventDispatcher.RoofOnLoad(new(map, layout, player, entities, party));
    }

    internal static ExplorationState LoadScene(ExplorationDefinition definition, ExplorationState world, LoadSceneMap load)
    {
        if (!definition.Maps.TryGetValue(load.Map, out var map)) throw new BattleRuleException("missing-map", "scene.map");
        // csc48 changes the displayed layout/view; entity allocation, map init and battle selection
        // are separate calls. Keep the running program, party and physical entities intact here.
        return new(map, map.Layout, world.Player, world.AllEntities, world.Party, world.SpriteSize,
            world.Aliases, population: world.Population);
    }

    internal static SessionSnapshot Apply(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, byte facing, MapLoadMode mode, StoryState continuation, List<SessionObservation> observations)
    {
        var world = current.Exploration ?? throw new BattleRuleException("map-transfer-mode", "map", true);
        if (!definition.Exploration!.Maps.TryGetValue(map, out var target))
            throw new BattleRuleException("missing-map", "map");
        // MainLoop selects BattleLoop before ExplorationLoop. The before-battle script owns
        // its map load and entity replacement; retain the actual field scene until those calls.
        if (world.Party.NewBattle is not null && target.Battle is { } battle &&
            (battle.UnlockedFlag is null || current.Story.Flags.Contains(battle.UnlockedFlag.Value)) &&
            (battle.CompletedFlag is null || !current.Story.Flags.Contains(battle.CompletedFlag.Value)))
            return BattleEntry.Select(current, continuation.Copy(continuation.Cursor,
                returnAnchor: new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing)), battle, observations);
        ExplorationState next;
        var flags = current.Story.Flags;
        if (mode == MapLoadMode.Preserve)
        {
            ValidateSetup(target, current.Story.Flags);
            if (map != world.Map) throw new BattleRuleException("preserve-other-map", "map.loadMode", true);
            if (!target.Traversal.IsWithinActiveArea(position) || OriginalMapTraversal.IsBlocked(world.Layout, position))
                throw new BattleRuleException("map-entry-position", "map.position");
            next = world.WithEntity(world.PlayerEntity with
            { Motion = world.PlayerEntity.Motion with { X = (short)(position.X * 384), Y = (short)(position.Y * 384),
                XDestination = (short)(position.X * 384), YDestination = (short)(position.Y * 384), Facing = facing }, Actions = null, ActionCursor = 0 });
            next = MapEventDispatcher.RoofOnLoad(next);
        }
        else
        {
            var enteredFlags = flags.ToHashSet();
            foreach (var write in target.EntryFlags)
                if (write.Value) enteredFlags.Add(write.Flag); else enteredFlags.Remove(write.Flag);
            flags = enteredFlags.Order().ToArray();
            // Source chooses/populates the setup before clearing its temporary flags, then loads layout/init.
            next = Build(target, world.Player, position, facing, world.PlayerEntity.Motion.XSpeed, world.Party, current.Story.Flags, flags);
        }
        var story = continuation.Copy(continuation.Cursor,
            flags: flags,
            textWindow: new ClosedTextWindow(),
            continuation: continuation.EnteringBattle is null ? ProgramContinuation.MapLoaded : continuation.Continuation,
            returnAnchor: continuation.EnteringBattle is null ? new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing) : null);
        if (story.Display is not null && mode == MapLoadMode.Rebuild)
        {
            if (target.BasePalette is not { Valid: true })
                throw new BattleRuleException("warp-palette-binding", "map.basePalette", true);
            story = story.Copy(story.Cursor, display: story.Display with { Base = target.BasePalette });
        }
        if (target.OnLoad is { } onLoad)
            story = story.Copy(onLoad, callers: story.Cursor is { } caller ? story.Callers.Append(caller) : story.Callers);
        return ProgramRunner.Commit(current, new ActiveExploration(next), story, observations, "map-transferred", map.Value);
    }

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
        => BattleEntry.Continue(definition, current, observations);

    private static void ValidateSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        if (map.Setup is not { } route) return;
        var selected = MapSetupSelector.Select(route, route.DefaultSetup,
            flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture)));
        if (selected != route.DefaultSetup)
            throw new BattleRuleException("map-setup", selected.Value, true);
    }
}
