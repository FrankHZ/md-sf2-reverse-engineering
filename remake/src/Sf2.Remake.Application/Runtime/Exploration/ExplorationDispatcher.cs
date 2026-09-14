using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ExplorationDispatcher
{
    internal static SessionResult Start(ScenarioDefinition definition, ExplorationStartInput start)
    {
        if (definition.Exploration is null || !definition.Exploration.Maps.TryGetValue(start.Map, out var map))
            throw new BattleRuleException("missing-map", "start.map");
        if (start.Facing > 3 || start.Speed is 0 or > 384 || start.Flags.Any(flag => flag is < 0 or > 65535))
            throw new BattleRuleException("exploration-start", "start");
        var world = MapTransfer.Build(map, start.Player, start.Position, start.Facing, start.Speed, start.Party, start.Flags);
        if (start.EntryProgram is { } entry && (entry.Instruction != 0 || !definition.Exploration.Programs.ContainsKey(entry.Program)))
            throw new BattleRuleException("program-entry", "start.program");
        var story = new StoryState(start.Flags, start.EntryProgram ?? map.OnLoad,
            continuation: start.EntryProgram is null ? ProgramContinuation.MapLoaded : ProgramContinuation.FieldInput,
            partyLists: definition.Exploration.PartyFlags is { } partyFlags ? MapPartyMembership.Rebuild(start.Flags, partyFlags) : null);
        return ProgramRunner.Run(definition, new(Guid.NewGuid(), 0, 0, new ActiveExploration(world), story,
            SessionStopReason.SimulationWait), []);
    }

    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, SessionCommand command)
    {
        if (current.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted)
            return Reject(current, "session-stopped", "command");
        List<SessionObservation> observations = [];
        try
        {
            switch (command)
            {
                case Acknowledge ack:
                    if (current.Story.Wait is not DialogueWait || current.Story.Wait.Token != ack.Wait)
                        return Reject(current, "stale-or-wrong-wait", "wait");
                    current = ProgramRunner.Commit(current, current.Active, FinishWait(current.Story), observations, "presentation-acknowledged");
                    break;
                case CompletePresentation completion:
                    if (current.Story.Wait is not PresentationWait presenting || presenting.Token != completion.Wait || presenting.Cue.Kind != completion.Kind)
                        return Reject(current, "stale-or-wrong-presentation", "presentation");
                    var completedActive = current.Active;
                    if (presenting.Cue is { Kind: PresentationCueKind.Gesture, Resource: "nod", Entity: { } nodded })
                    {
                        var entity = ProgramRunner.Entity(current, nodded);
                        completedActive = new ActiveExploration(current.Exploration!.WithEntity(entity with { Motion = entity.Motion with { AnimationCounter = 0 } }));
                    }
                    current = ProgramRunner.Commit(current, completedActive, FinishWait(current.Story), observations, "presentation-completed", completion.Kind.ToString());
                    break;
                case EntitySpriteReady sprite:
                    var spriteWorld = current.Exploration;
                    var spriteEntity = spriteWorld?.AllEntities.FirstOrDefault(entity => entity.Slot == sprite.Slot);
                    if (spriteEntity is null || !spriteEntity.WaitingForSprite || spriteEntity.SpriteRequest != sprite.Request || spriteEntity.SpriteReady == sprite.Request)
                        return Reject(current, "stale-or-wrong-sprite", "sprite");
                    current = ProgramRunner.Commit(current, new ActiveExploration(spriteWorld!.WithEntity(spriteEntity with { SpriteReady = sprite.Request })),
                        current.Story, observations, "entity-sprite-ready", sprite.Slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case ChooseDialogue choice:
                    if (current.Story.Wait is not ChoiceWait waiting || waiting.Token != choice.Wait)
                        return Reject(current, "stale-or-wrong-wait", "wait");
                    current = ProgramRunner.Commit(current, current.Active,
                        FinishWait(current.Story).Copy(NextCursor(current.Story),
                            flags: ProgramRunner.Flags(current.Story, waiting.ResultFlag, choice.Yes)), observations,
                        "dialogue-chosen", choice.Yes ? "yes" : "no");
                    break;
                case AdvanceSimulation advance:
                    if (advance.Ticks is < 1 or > 600 || advance.Wait != current.Story.Wait?.Token)
                        return Reject(current, "stale-or-wrong-wait", "wait");
                    bool existingFieldInput = current.StopReason == SessionStopReason.PlayerInput &&
                        current.Story.Cursor is null && current.Story.Wait is null;
                    for (int tick = 0; tick < advance.Ticks; tick++)
                    {
                        bool entityUpdates = current.Story.Wait is EntityEventFacingWait || current.Story.Cursor is not { } location ||
                            definition.Exploration!.Programs[location.Program].EntitiesRunning;
                        EntityActionTickResult? tickResult = entityUpdates && current.Exploration is { } world ? EntityActionRunner.Tick(world) : null;
                        var active = tickResult is not null ? new ActiveExploration(MapEventDispatcher.Roof(tickResult.World)) : current.Active;
                        var story = current.Story;
                        if (tickResult?.Failure is { } failure)
                        {
                            story = story.Copy(story.Cursor, story.Wait, simulationTick: checked(story.SimulationTick + 1));
                            current = ProgramRunner.Commit(current, active, story, observations, "entity-action-stopped", failure.Field);
                            return ProgramRunner.Failure(current, observations, failure);
                        }
                        if (story.Wait is EntityEventFacingWait)
                        { active = MapEventDispatcher.Face(current, active); story = story.Copy(story.Cursor); }
                        else if (story.Wait is TickWait timer)
                            story = timer.Remaining <= 1 ? FinishWait(story) : story.Copy(story.Cursor, timer with { Remaining = timer.Remaining - 1 });
                        else if (story.Wait is EntityWait entityWait && active is ActiveExploration explored &&
                            !explored.World.Entities[entityWait.Entity].Busy)
                            story = story.Cursor is null ? story.Copy(entityWait.AfterMotion) : FinishWait(story);
                        story = story.Copy(story.Cursor, story.Wait, simulationTick: checked(story.SimulationTick + 1));
                        current = ProgramRunner.Commit(current, active, story, observations, "simulation-tick");
                        // Existing field input does not interrupt background ticks. Completing a
                        // wait or resuming a program still yields at its next control boundary.
                        if (story.Wait is null && !existingFieldInput)
                            return ProgramRunner.Run(definition, current, observations);
                    }
                    return existingFieldInput ? ProgramRunner.Run(definition, current, observations)
                        : ProgramRunner.Result(current, observations);
                default:
                    if (current.Story.Wait is not null || current.Story.Cursor is not null || current.Exploration is null)
                        return Reject(current, "program-owns-control", "command");
                    if ((command is Sf2.Remake.Application.Runtime.Move or Sf2.Remake.Application.Runtime.Interact) && current.Exploration.Definition.InputProgram is { } inputProgram)
                    {
                        current = ProgramRunner.Commit(current, current.Active, current.Story.Copy(inputProgram), observations, "map-input-program");
                        return ProgramRunner.Run(definition, current, observations);
                    }
                    if (command is Move move) return Move(definition, current, move, observations);
                    if (command is Interact interact) return Interact(definition, current, interact, observations);
                    return Reject(current, "exploration-command", "command");
            }
            return ProgramRunner.Run(definition, current, observations);
        }
        catch (BattleRuleException error) { return ProgramRunner.Failure(current, observations, error); }
    }

    private static SessionResult Move(ScenarioDefinition definition, SessionSnapshot current, Move move, List<SessionObservation> observations)
    {
        if (!Enum.IsDefined(move.Direction)) return Reject(current, "invalid-direction", "direction");
        var world = current.Exploration!;
        var player = world.PlayerEntity;
        if (player.Busy) return Reject(current, "entity-busy", "player");
        byte facing = Facing(move.Direction);
        var faced = player with { Motion = player.Motion with { Facing = facing } };
        var candidate = world.Definition.Traversal.ResolveCandidateTarget(world.Layout, player.Position, move.Direction);
        if (candidate is not null && world.Definition.Traversal.IsWithinActiveArea(candidate))
        {
            var opened = MapEventDispatcher.OpenDoor(world, candidate);
            if (!ReferenceEquals(opened, world))
            {
                world = opened;
                current = ProgramRunner.Commit(current, new ActiveExploration(world), current.Story, observations, "door-opened");
            }
            var warp = world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Warp &&
                Matches(entry, candidate, world.Layout[candidate.X, candidate.Y], current.Story));
            if (warp is not null)
            {
                if (warp.Program is { } frontier)
                {
                    current = ProgramRunner.Commit(current, current.Active, current.Story.Copy(frontier), observations, "warp-program");
                    return ProgramRunner.Run(definition, current, observations);
                }
                var transferred = MapTransfer.Apply(definition, current, warp.DestinationMap!, warp.Destination!, warp.Facing,
                    warp.LoadMode, current.Story, observations);
                return ProgramRunner.Run(definition, transferred, observations);
            }
        }
        var traversal = world.Definition.Traversal.TryMove(world.Layout, player.Position, move.Direction);
        var others = world.AllEntities.Where(entity => entity.Slot != player.Slot && entity.Visible);
        bool occupied = world.Definition.Population is not null
            ? EntityMotion.FieldObstructed(traversal.Position.X * 384, traversal.Position.Y * 384, others.Select(entity => entity.Motion))
            : others.Any(entity => entity.Motion.XDestination / 384 == traversal.Position.X && entity.Motion.YDestination / 384 == traversal.Position.Y);
        if (traversal.Outcome != OriginalMapTraversalOutcome.Moved || occupied)
            return ProgramRunner.Result(ProgramRunner.Stop(ProgramRunner.Commit(current,
                new ActiveExploration(world.WithEntity(faced)), current.Story, observations, "movement-blocked"),
                SessionStopReason.PlayerInput), observations);
        var actions = new EntityActionProgram([new MoveEntityAbsolute(traversal.Position, world.Definition.Population is not null), new StopEntityActions()]);
        var moved = world.WithEntity(faced with { Actions = actions, ActionCursor = 0 });
        var step = world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Step &&
            Matches(entry, traversal.Position, world.Layout[traversal.Position.X, traversal.Position.Y], current.Story));
        var wait = new EntityWait(new(current.ObservationSequence + 1), world.Player, step?.Program);
        current = ProgramRunner.Commit(current, new ActiveExploration(moved), current.Story.Copy(null, wait), observations, "movement-started");
        return ProgramRunner.Result(current, observations);
    }

    private static SessionResult Interact(ScenarioDefinition definition, SessionSnapshot current, Interact command,
        List<SessionObservation> observations)
    {
        var world = current.Exploration!;
        if (!world.Entities.TryGetValue(command.Entity, out var entity) || !entity.Visible)
            return Reject(current, "missing-interaction-entity", "entity");
        var player = world.PlayerEntity;
        int dx = entity.Position.X - player.Position.X, dy = entity.Position.Y - player.Position.Y;
        if (Math.Abs(dx) + Math.Abs(dy) != 1 || player.Motion.Facing !=
            (dx > 0 ? 0 : dx < 0 ? 2 : dy < 0 ? 1 : 3))
            return Reject(current, "interaction-range-or-facing", "entity");
        var entry = world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Interact &&
            (entry.Entity is null || entry.Entity == command.Entity) && (entry.RequiredFlag is null ||
                current.Story.Flags.Contains(entry.RequiredFlag.Value) == entry.RequiredFlagValue));
        if (entry?.Program is not { } program) return Reject(current, "no-interaction", "entity");
        current = MapEventDispatcher.Interact(current, entry, command.Entity, observations);
        return ProgramRunner.Run(definition, current, observations);
    }

    private static bool Matches(ExplorationEvent entry, MapPosition position, ushort word, StoryState story) =>
        (entry.X is null || entry.X == position.X) && (entry.Y is null || entry.Y == position.Y) &&
        (entry.RequiredMarker is null || (word & 0x3C00) == entry.RequiredMarker) &&
        (entry.RequiredFlag is null || story.Flags.Contains(entry.RequiredFlag.Value) == entry.RequiredFlagValue);
    private static ProgramLocation? NextCursor(StoryState story) => story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null;
    private static StoryState FinishWait(StoryState story) => story.Copy(NextCursor(story),
        textWindow: story.Wait is DialogueWait { Mode: TextDisplayMode.Single } ? new ClosedTextWindow() : story.TextWindow);
    private static byte Facing(ExplorationDirection direction) => direction switch
    { ExplorationDirection.East => 0, ExplorationDirection.North => 1, ExplorationDirection.West => 2, _ => 3 };
    private static SessionResult Reject(SessionSnapshot current, string code, string field) => BattleCommandDispatcher.Reject(current, code, field);

}
