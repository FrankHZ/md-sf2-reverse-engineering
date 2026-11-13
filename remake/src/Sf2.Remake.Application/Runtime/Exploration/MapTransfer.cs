using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class MapTransfer
{
    // Apply is a pure snapshot construction. Reuse its full admission before consuming a
    // field action; publish again after the old-world pass with its resulting party seed.
    internal static void Validate(ScenarioDefinition definition, SessionSnapshot current, ExplorationState candidate,
        ExplorationEvent warp, SessionRules rules, bool sourcePlacement = false)
    {
        var preview = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(MapEventDispatcher.Roof(candidate)), current.Story, current.StopReason);
        if (warp.LoadMode is MapLoadMode.ScrollHorizontal or MapLoadMode.ScrollVertical)
        {
            _ = Scroll(definition, preview, warp, []);
            return;
        }
        var admitted = Apply(definition, preview, warp.DestinationMap!, warp.Destination!, warp.Facing,
            warp.LoadMode, current.Story, [], rules, sourcePlacement);
        if (BattleEntry.First(definition, current.Story, definition.Exploration!.Maps[warp.DestinationMap!], rules, warp.Destination) is not null) return;
        ValidateDisplay(current.Story.Display);
        var target = definition.Exploration.Maps[warp.DestinationMap!].SelectSetup(current.Story.Flags);
        if (warp.LoadMode == MapLoadMode.Rebuild && target.BasePalette is not { Valid: true })
            throw new BattleRuleException("warp-palette-binding", "map.basePalette", true);
        // Reject statically known unsupported display effects before consuming the move.
        var visited = new HashSet<ProgramLocation>();
        void Check(ProgramLocation? location)
        {
            if (location is not { } entry || !visited.Add(entry)) return;
            foreach (var instruction in definition.Exploration.Programs[entry.Program].Instructions.Skip(entry.Instruction))
                switch (instruction)
                {
                    case PresentCue { Kind: PresentationCueKind.FlashWhite } cue: ValidateFlash(cue); break;
                    case PresentCue cue when IsPalette(cue): ValidateCue(cue); break;
                    case LoadSceneMap scene:
                        if (!definition.Exploration.Maps.TryGetValue(scene.Map, out var sceneMap) || sceneMap.BasePalette is not { Valid: true })
                            throw new BattleRuleException("scene-load-palette-binding", "map.basePalette", true);
                        break;
                    case CallProgram call: Check(call.Target); break;
                    case JumpProgram jump: Check(jump.Target); break;
                    case BranchFlag branch: Check(branch.Target); break;
                    case BranchInteractionFacing branch: Check(branch.Target); break;
                    case BranchEntityCoordinates branch: Check(branch.Target); break;
                }
        }
        // Resolve only the pure entry dispatch, before any operation can mutate flags.
        // After that boundary retain the conservative scan of every possible branch.
        var start = target.OnLoad;
        var dispatch = new HashSet<ProgramLocation>();
        while (start is { } at && dispatch.Add(at))
        {
            var instruction = definition.Exploration.Programs[at.Program].Instructions[at.Instruction];
            if (instruction is BranchFlag branch)
                start = admitted.Story.Flags.Contains(branch.Flag) == branch.WhenSet
                    ? branch.Target : new(at.Program, at.Instruction + 1);
            else if (instruction is JumpProgram jump) start = jump.Target;
            else break;
        }
        Check(start);
    }

    internal static void ValidateDisplay(ExplorationDisplay? display)
    {
        if (display is not { Period: > 0, Base.Valid: true, Current.Valid: true } ||
            !Enum.IsDefined(display.Visibility) ||
            display.Visibility == FullFadeVisibility.Transitioning ||
            (display.Visibility switch
            {
                FullFadeVisibility.Black => !display.Current.Black,
                FullFadeVisibility.White => !display.Current.White,
                _ => display.Current != display.Base,
            }))
            throw new BattleRuleException("full-fade-state", "story.display", true);
    }

    internal static bool IsPalette(PresentCue cue) => cue.Kind is PresentationCueKind.FadeIn or
        PresentationCueKind.FadeOut or PresentationCueKind.RestorePalette;

    internal static void ValidateFlash(PresentCue cue)
    {
        if (cue.Resource is not ("white-four-pulses" or "white-pulses") || cue.DurationTicks is not (> 0 and <= 240) ||
            cue.Resource == "white-pulses" && cue.DurationTicks % 8 != 0 ||
            cue.Entity is not null || cue.Position is not null || cue.FullBlack is not null)
            throw new BattleRuleException("flash-pulse-binding", "program.presentation", true);
    }

    internal static void ValidateCue(PresentCue cue)
    {
        if (cue.Resource is not ("black" or "white") ||
            cue.Resource == "white" && cue.FullBlack is not null ||
            cue.Kind is not (PresentationCueKind.FadeIn or PresentationCueKind.FadeOut) || cue.FullBlack?.Period == 0)
            throw new BattleRuleException("full-black-fade-binding", "program.presentation", true);
    }

    internal static SessionSnapshot BeginWarp(ScenarioDefinition definition, SessionSnapshot current,
        ExplorationEvent warp, List<SessionObservation> observations, SessionRules rules, bool sourcePlacement = false)
    {
        if (warp.LoadMode is MapLoadMode.ScrollHorizontal or MapLoadMode.ScrollVertical)
            return Scroll(definition, current, warp, observations);
        if (BattleEntry.First(definition, current.Story, definition.Exploration!.Maps[warp.DestinationMap!], rules, warp.Destination) is not null)
            return Apply(definition, current, warp.DestinationMap!, warp.Destination!, warp.Facing,
                warp.LoadMode, current.Story, observations, rules, sourcePlacement);
        var story = current.Story.Copy(null, warp: new(warp.DestinationMap!, warp.Destination!, warp.Facing, warp.LoadMode, sourcePlacement));
        var world = current.Exploration!;
        current = new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(world.PlayerEntity with { Actions = null, ActionCursor = 0, WaitingForMotion = false })),
            story, current.StopReason);
        return BeginFade(current, story, PresentationCueKind.FadeOut, FullFadePurpose.WarpOut, null, observations);
    }

    private static SessionSnapshot Scroll(ScenarioDefinition definition, SessionSnapshot current, ExplorationEvent warp,
        List<SessionObservation> observations)
    {
        var world = current.Exploration!;
        if (world.TravelMode != FieldTravelMode.Raft) CaravanRuntime.RequireOrdinaryMapOperation(world);
        var target = definition.Exploration!.Maps[warp.DestinationMap!].SelectSetup(current.Story.Flags);
        var player = world.PlayerEntity;
        var horizontal = warp.LoadMode == MapLoadMode.ScrollHorizontal;
        var position = new MapPosition(horizontal ? warp.Destination!.X : player.Position.X,
            horizontal ? player.Position.Y : warp.Destination!.Y);
        if (!target.Traversal.IsWithinActiveArea(position))
            throw new BattleRuleException("scroll-position", "event.position");
        // ProcessMapEventType1_Warp retains the population. Walking followers arrive
        // with the leader and DeclareRaftEntity projects the berth on the new map.
        var layout = LoadLayout(target, current.Story.Flags);
        var next = new ExplorationState(target, layout, world.Player, world.AllEntities, world.Party, world.SpriteSize,
            world.Aliases, population: world.Population, quake: world.Quake, travelMode: world.TravelMode, currentEntities: world.CurrentEntities);
        next = FollowerMotion.Arrive(next, position, player.Motion.Facing);
        if (world.TravelMode == FieldTravelMode.Walking && current.Story.Flags.Contains(64))
            next = RaftRuntime.OnScroll(next, current.Story.Flags, current.Story.Raft);
        var story = current.Story.Copy(null, logicalView: ExplorationViewRunner.Initialize(next));
        return ProgramRunner.Commit(current, new ActiveExploration(next), story, observations, "map-scrolled", target.Map.Value);
    }

    private static WorkingMapLayout LoadLayout(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        var layout = map.Layout;
        foreach (var copy in map.LayoutEvents?.Flags ?? [])
            if (flags.Contains(copy.Flag)) layout = layout.ApplyBlockCopy(copy.Copy);
        return FieldSearch.RestoreChests(layout, map.Search, flags);
    }

    internal static SessionSnapshot BeginFade(SessionSnapshot current, StoryState story, PresentationCueKind kind,
        FullFadePurpose purpose, byte? temporaryPeriod, List<SessionObservation> observations, FullFadeColor color = FullFadeColor.Black)
    {
        ValidateDisplay(story.Display);
        if (temporaryPeriod == 0) throw new BattleRuleException("full-fade-period", "program.period", true);
        var display = story.Display!;
        if (!Enum.IsDefined(color) || color == FullFadeColor.White && (purpose != FullFadePurpose.Script || temporaryPeriod is not null))
            throw new BattleRuleException("full-fade-color", "program.presentation", true);
        if (color == FullFadeColor.White) temporaryPeriod = 1;
        byte period = temporaryPeriod ?? display.Period;
        var wait = new FullFadeWait(new(current.ObservationSequence + 1), kind, purpose, period, period,
            ExtraServices: purpose == FullFadePurpose.WarpOut ? 0 : 1,
            RestorePeriod: temporaryPeriod is null ? null : display.Period, Color: color);
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
                // Source saturation at nibble15 has effective CRAM value14; retain even words.
                int offset = wait.Color == FullFadeColor.White
                    ? wait.Kind == PresentationCueKind.FadeIn ? 6 - entry : entry + 1
                    : wait.Kind == PresentationCueKind.FadeIn ? entry - 6 : -entry - 1;
                ushort Color(ushort value) => (ushort)(Math.Clamp((value & 14) + 2 * offset, 0, 14) |
                    (Math.Clamp(((value >> 4) & 14) + 2 * offset, 0, 14) << 4) |
                    (Math.Clamp(((value >> 8) & 14) + 2 * offset, 0, 14) << 8));
                display = display with { Current = new(Color(display.Base.Color2), Color(display.Base.Color3)) };
            }
            wait = wait with { Entry = entry + 1, Countdown = wait.Period,
                LogicalDone = entry == 7 && wait.ExtraServices == 0 };
        }
        if (wait.LogicalDone)
            display = display with { Visibility = wait.Kind == PresentationCueKind.FadeOut
                ? wait.Color == FullFadeColor.White ? FullFadeVisibility.White : FullFadeVisibility.Black
                : FullFadeVisibility.BaseRestored,
                Period = wait.RestorePeriod ?? display.Period };
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, wait, display: display));
        // Dispatcher already admitted music. Warp fades retain the installed field entity
        // context; script fades use the current explicit override/program enablement.
        current = ExplorationDispatcher.Service(definition, current, observations, "fade-service",
            musicServiced: true, entityEnabled: wait.Purpose == FullFadePurpose.Script ? null : true,
            detail: wait.Entry.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return FinishFade(current, observations);
    }

    internal sealed class FadeServiceFailure(SessionSnapshot snapshot, BattleRuleException failure) : Exception
    { internal SessionSnapshot Snapshot { get; } = snapshot; internal BattleRuleException Failure { get; } = failure; }

    internal static SessionSnapshot FinishFade(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.Wait is not FullFadeWait { LogicalDone: true } wait || !(wait.ActualDone || wait.DeliverySkipped)) return current;
        var story = current.Story;
        story = wait.Purpose switch
        {
            FullFadePurpose.WarpOut => story.Copy(null, new WarpLoadWait(new(current.ObservationSequence + 1))),
            FullFadePurpose.Script => story.Copy(story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null),
            _ => story.Copy(null, clearWarp: true),
        };
        return ProgramRunner.Commit(current, current.Active, story, observations, "full-fade-completed", wait.Kind.ToString());
    }

    internal static SessionSnapshot LoadTick(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations, SessionRules rules)
    {
        var wait = (WarpLoadWait)current.Story.Wait!;
        var story = current.Story.Copy(null, wait with { Remaining = wait.Remaining - 1 },
            simulationTick: checked(current.Story.SimulationTick + 1));
        current = ProgramRunner.Commit(current, current.Active, story, observations, "map-load-service");
        if (wait.Remaining > 1) return current;
        var warp = story.Warp!;
        try { return Apply(definition, current, warp.Map, warp.Position, warp.Facing, warp.Mode, story.Copy(null), observations, rules, warp.SourcePlacement); }
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
        byte facing, ushort speed, BattleStartInput party, IReadOnlyList<int> flags, IReadOnlyList<int>? layoutFlags = null, bool sourcePlacement = false)
    {
        map = map.SelectSetup(flags);
        var layout = LoadLayout(map, layoutFlags ?? flags);
        if (!map.Traversal.IsWithinActiveArea(position) || (!sourcePlacement && OriginalMapTraversal.IsBlocked(layout, position)))
            throw new BattleRuleException("map-entry-position", "map.position");
        if (map.Population is { } population)
        {
            return MapEventDispatcher.RoofOnLoad(SceneEntities.Build(map, layout, player, position, facing,
                speed, party, flags, population, map.Entities));
        }
        var entities = map.Entities.Where(entity => entity.Entity != player).Select(entity =>
            new ExplorationEntity(entity.Entity, EntityMotionState.At(entity.Position, entity.Facing, entity.Speed) with
                { FlagsA = entity.Obstruction ? (map.Caravan is null ? (byte)0x20 : (byte)0xE0) : (byte)0 }, entity.Visible, entity.Actions, Sprite: entity.Sprite)).ToList();
        entities.Insert(0, new(player, EntityMotionState.At(position, facing, speed), true, Sprite: map.Caravan is { } caravan ?
            party.Actors.Single(actor => actor.Actor == caravan.PlayerAppearance.Actor).Hp == 0
                ? caravan.PlayerAppearance.DefeatedSprite : caravan.PlayerAppearance.Sprite : null));
        return MapEventDispatcher.RoofOnLoad(new(map, layout, player, entities, party));
    }

    internal static ExplorationState LoadScene(ExplorationDefinition definition, ExplorationState world, LoadSceneMap load)
    {
        CaravanRuntime.RequireOrdinaryMapOperation(world);
        if (!definition.Maps.TryGetValue(load.Map, out var map)) throw new BattleRuleException("missing-map", "scene.map");
        // csc48 changes the displayed layout/view; entity allocation, map init and battle selection
        // are separate calls. Keep the running program, party, physical entities and append order intact here.
        return new(map, map.Layout, world.Player, world.AllEntities, world.Party, world.SpriteSize,
            world.Aliases, population: world.Population, quake: world.Quake, currentEntities: world.CurrentEntities);
    }

    internal static (ExplorationState World, StoryState Story) ReloadScene(SessionSnapshot current, ReloadSceneMap reload)
    {
        CaravanRuntime.RequireOrdinaryMapOperation(current.Exploration!);
        ExplorationTextRunner.ValidateContext(current);
        var story = current.Story;
        var world = current.Exploration!;
        ValidateDisplay(story.Display);
        if ((story.Warp is { } warp && warp.Map != world.Definition.Map) || story.TextWindow is not ClosedTextWindow ||
            story.PortraitWindow is not ClosedPortraitWindow || story.LogicalText is not { Open: false })
            throw new BattleRuleException("scene-reload-context", "story.windows", true);
        if (world.Definition.BasePalette != story.Display!.Base)
            throw new BattleRuleException("scene-reload-palette", "story.display", true);
        // Negative LoadMap retains working blocks, layout and roof state. Only csc36
        // first clears the layout and rewrites block zero. Only csc46 detaches the camera.
        var next = reload.ResetLayout ? world.WithLayout(WorkingMapLayout.ResetScene()) : world;
        var view = ExplorationViewRunner.LoadScene(next, reload.Camera, story.LogicalView!);
        if (reload.ResetLayout) view = view with { TargetSlot = story.LogicalView!.TargetSlot };
        var target = view.TargetSlot is { } slot
            ? next.AllEntities.FirstOrDefault(entity => entity.Slot == slot) ?? throw new BattleRuleException("field-view-target", "story.view", true)
            : next.PlayerEntity;
        next = MapEventDispatcher.RoofOnLoad(next, target, new(view.Area.ForegroundX, view.Area.ForegroundY));
        return (next, story.Copy(story.Cursor, logicalView: view, logicalText: new(),
            clearCameraEntity: !reload.ResetLayout, clearCameraTarget: !reload.ResetLayout));
    }

    internal static StoryState BindScene(SessionSnapshot current, ExplorationState next, LoadSceneMap load)
    {
        CaravanRuntime.RequireOrdinaryMapOperation(current.Exploration!);
        ExplorationTextRunner.ValidateContext(current);
        var story = current.Story;
        if (story.PortraitWindow is not ClosedPortraitWindow || story.Wait is not null || story.Typewriting ||
            (story.TextWindow is OpenTextWindow) != (story.LogicalText is { Open: true }))
            throw new BattleRuleException("scene-load-windows", "story.windows", true);
        ValidateDisplay(story.Display);
        // The source replaces PALETTE_1_BASE without copying it into current CRAM.
        // A settled black load is representable; visible base/current divergence is
        // outside this admission. Validate everything before publishing the scene.
        if (story.Display!.Visibility != FullFadeVisibility.Black)
            throw new BattleRuleException("scene-load-black", "story.display", true);
        if (next.Definition.BasePalette is not { Valid: true } palette)
            throw new BattleRuleException("scene-load-palette-binding", "map.basePalette", true);
        return story.Copy(story.Cursor, logicalView: ExplorationViewRunner.LoadScene(next, load.Camera, story.LogicalView!),
            logicalText: new(), textWindow: new ClosedTextWindow(),
            display: story.Display with { Base = palette }, clearCameraEntity: true, clearCameraTarget: true);
    }

    internal static SessionSnapshot Apply(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, byte facing, MapLoadMode mode, StoryState continuation, List<SessionObservation> observations, SessionRules rules,
        bool sourcePlacement = false, bool checkBattle = true)
    {
        var world = current.Exploration ?? throw new BattleRuleException("map-transfer-mode", "map", true);
        bool riding = world.TravelMode == FieldTravelMode.Raft && mode == MapLoadMode.Rebuild;
        if (!riding) CaravanRuntime.RequireOrdinaryMapOperation(world);
        if (!definition.Exploration!.Maps.TryGetValue(map, out var target))
            throw new BattleRuleException("missing-map", "map");
        target = target.SelectSetup(current.Story.Flags);
        // MainLoop selects BattleLoop before ExplorationLoop. The before-battle script owns
        // its map load and entity replacement; retain the actual field scene until those calls.
        if (checkBattle && world.Party.NewBattle is not null && BattleEntry.First(definition, current.Story, target, rules, position) is { } battle)
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
            next = FollowerMotion.Arrive(world, position, facing);
            next = MapEventDispatcher.RoofOnLoad(next);
        }
        else
        {
            var enteredFlags = flags.ToHashSet();
            foreach (var write in target.EntryFlags)
                if (write.Value) enteredFlags.Add(write.Flag); else enteredFlags.Remove(write.Flag);
            flags = enteredFlags.Order().ToArray();
            // Source chooses/populates the setup before clearing its temporary flags, then loads layout/init.
            var party = riding ? OutcomeReturnRules.Heal(rules.OutcomeReturn, definition, world.Party, all: false) : world.Party;
            next = Build(target, world.Player, position, facing, world.PlayerEntity.Motion.XSpeed, party, current.Story.Flags, flags,
                sourcePlacement || riding);
            if (riding)
                next = next.WithTravelMode(FieldTravelMode.Raft);
            next = RaftRuntime.Declare(next, flags, continuation.Raft);
        }
        // Entering ExplorationLoop subtracts once. A scroll never enters this path.
        if (continuation.StepCounter is { } steps)
            continuation = continuation.Copy(continuation.Cursor, stepCounter: Math.Max((short)0, unchecked((short)(steps - 20000))));
        var story = continuation.Copy(continuation.Cursor,
            flags: flags,
            textWindow: new ClosedTextWindow(),
            continuation: continuation.EnteringBattle is null ? ProgramContinuation.MapLoaded : continuation.Continuation,
            returnAnchor: continuation.EnteringBattle is null ? new(world.Map, world.PlayerEntity.Position, world.PlayerEntity.Motion.Facing) : null);
        story = ExplorationTextRunner.Initialize(next, story);
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

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations, SessionRules rules)
        => BattleEntry.Continue(definition, current, observations, rules);

    private static void ValidateSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        if (map.Setup is not { } route) return;
        var selected = MapSetupSelector.Select(route, route.DefaultSetup,
            flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture)));
        if (selected != route.DefaultSetup)
            throw new BattleRuleException("map-setup", selected.Value, true);
    }
}
