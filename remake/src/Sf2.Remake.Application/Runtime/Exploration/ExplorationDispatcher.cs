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
        if (start.EntityPhases.Count > 0)
        {
            var phased = new Dictionary<int, ExplorationEntity>();
            foreach (var phase in start.EntityPhases)
            {
                if (!world.TryResolveEntity(phase.Entity, out var entity) || entity.Entity != phase.Entity ||
                    entity.Slot != phase.Slot || entity.Actions is not { } actions ||
                    phase.ActionCursor < 0 || phase.ActionCursor >= actions.Actions.Count ||
                    actions.Actions[phase.ActionCursor] is not WaitEntityTicks wait || wait.Ticks != phase.NextWaitTicks ||
                    (phase.WaitingForMotion && (phase.ActionCursor == 0 ||
                        actions.Actions[phase.ActionCursor - 1] is not RandomWalkEntity)) ||
                    !phased.TryAdd(entity.Slot, entity with { Motion = phase.Motion, ActionCursor = phase.ActionCursor,
                        WaitingForMotion = phase.WaitingForMotion }))
                    throw new BattleRuleException("entity-start-phase", "start.entityPhases");
            }
            world = world.WithEntities(world.AllEntities.Select(entity =>
                phased.TryGetValue(entity.Slot, out var replacement) ? replacement : entity));
        }
        if (start.EntryProgram is { } entry && (entry.Instruction != 0 || !definition.Exploration.Programs.ContainsKey(entry.Program)))
            throw new BattleRuleException("program-entry", "start.program");
        if (start.Display is not null) MapTransfer.ValidateDisplay(start.Display);
        var story = new StoryState(start.Flags, start.EntryProgram ?? map.OnLoad,
            continuation: start.EntryProgram is null ? ProgramContinuation.MapLoaded : ProgramContinuation.FieldInput,
            partyLists: definition.Exploration.PartyFlags is { } partyFlags ? MapPartyMembership.Rebuild(start.Flags, partyFlags) : null,
            display: start.Display);
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
            bool playerWait = command is WaitAtInput or WaitForText;
            if (command is WaitAtInput)
            {
                if (!current.CanWaitAtInput)
                    return Reject(current, "field-input-unavailable", "command");
                // A deliberate input owns exactly one opportunity; use the existing update path.
                command = new AdvanceSimulation();
            }
            else if (command is WaitForText textWait)
            {
                if (!current.CanWaitForText) return Reject(current, "text-input-unavailable", "command");
                if (textWait.Wait != current.Story.Wait!.Token) return Reject(current, "stale-or-wrong-wait", "wait");
                command = new AdvanceSimulation(textWait.Wait);
            }
            switch (command)
            {
                case Acknowledge ack:
                    if (current.Story.Wait is not DialogueWait || current.Story.Wait.Token != ack.Wait)
                        return Reject(current, "stale-or-wrong-wait", "wait");
                    current = ProgramRunner.Commit(current, current.Active, FinishWait(current.Story), observations, "presentation-acknowledged");
                    break;
                case CompletePresentation completion:
                    if (current.Story.Wait is FullFadeWait fade)
                    {
                        if (fade.Token != completion.Wait || fade.Kind != completion.Kind || fade.ActualDone)
                            return Reject(current, "stale-or-wrong-presentation", "presentation");
                        current = ProgramRunner.Commit(current, current.Active,
                            current.Story.Copy(current.Story.Cursor, fade with { ActualDone = true }), observations,
                            "presentation-completed", completion.Kind.ToString());
                        current = MapTransfer.FinishFade(current, observations);
                        break;
                    }
                    if (current.Story.Wait is not PresentationWait presenting || presenting.Token != completion.Wait || presenting.Cue.Kind != completion.Kind)
                        return Reject(current, "stale-or-wrong-presentation", "presentation");
                    var completedActive = current.Active;
                    if (presenting.Cue is { Kind: PresentationCueKind.Gesture, Resource: "nod", Entity: { } nodded })
                    {
                        var entity = ProgramRunner.Entity(current, nodded);
                        completedActive = new ActiveExploration(current.Exploration!.WithEntity(entity with { Motion = entity.Motion with { AnimationCounter = 0 } }));
                    }
                    if (presenting is { Cue: { Kind: PresentationCueKind.Gesture, Resource: "shiver", Entity: { } shivered }, Restore: { } restore })
                    {
                        var entity = ProgramRunner.Entity(current, shivered);
                        completedActive = new ActiveExploration(current.Exploration!.WithEntities(current.Exploration.AllEntities.Select(row => row.Slot == entity.Slot
                            ? row with { Motion = row.Motion with { AnimationCounter = restore.AnimationCounter, FlagsB = (byte)(row.Motion.FlagsB & ~8) } } : row), restore.SpriteSize));
                    }
                    current = ProgramRunner.Commit(current, completedActive, FinishWait(current.Story), observations, "presentation-completed", completion.Kind.ToString());
                    break;
                case EntitySpriteReady sprite:
                    var spriteWorld = current.Exploration;
                    var spriteEntity = spriteWorld?.AllEntities.FirstOrDefault(entity => entity.Slot == sprite.Slot);
                    if (spriteEntity is null || !spriteEntity.WaitingForSprite || spriteEntity.SpriteRequest != sprite.Request || spriteEntity.SpriteReady == sprite.Request)
                        return Reject(current, "stale-or-wrong-sprite", "sprite");
                    bool facingReady = current.Story.Wait is EntitySpriteWait facingWait && facingWait.Slot == sprite.Slot && facingWait.Request == sprite.Request;
                    bool setReady = current.Story.Wait is EntitySetSpriteWait;
                    var mountedWorld = spriteWorld!.WithEntity(spriteEntity with
                        { SpriteReady = sprite.Request, WaitingForSprite = !(facingReady || setReady) });
                    current = ProgramRunner.Commit(current, new ActiveExploration(mountedWorld),
                        facingReady || (setReady && !mountedWorld.AllEntities.Any(entity => entity.WaitingForSprite)) ? FinishWait(current.Story) : current.Story,
                        observations, "entity-sprite-ready", sprite.Slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
                    if (current.CanWaitForText && !playerWait)
                        return Reject(current, "explicit-text-wait-required", "command");
                    if (advance.Ticks is < 1 or > 600 || advance.Wait != current.Story.Wait?.Token)
                        return Reject(current, "stale-or-wrong-wait", "wait");
                    if (current.Story.Wait is FullFadeWait { LogicalDone: true })
                        return Reject(current, "fade-awaiting-presentation", "wait");
                    bool existingFieldInput = current.StopReason == SessionStopReason.PlayerInput &&
                        current.Story.Cursor is null && current.Story.Wait is null;
                    for (int tick = 0; tick < advance.Ticks; tick++)
                    {
                        if (current.Story.Wait is FullFadeWait or WarpLoadWait)
                        {
                            var token = current.Story.Wait.Token;
                            current = current.Story.Wait is FullFadeWait
                                ? MapTransfer.FadeTick(definition, current, observations)
                                : MapTransfer.LoadTick(definition, current, observations);
                            if (current.Story.Wait?.Token != token)
                                return ProgramRunner.Run(definition, current, observations);
                            if (current.Story.Wait is FullFadeWait { LogicalDone: true })
                                return ProgramRunner.Result(current, observations);
                            continue;
                        }
                        bool entityUpdates = current.Story.Wait is DialogueWait { InputFirstEntityService: { } enabled }
                            ? enabled : current.Story.Wait is EntityEventFacingWait || current.Story.Cursor is not { } location ||
                                definition.Exploration!.Programs[location.Program].EntitiesRunning;
                        var pendingMove = (current.Story.Wait as EntityWait)?.PendingMove;
                        EntityActionTickResult? tickResult = entityUpdates && current.Exploration is { } world
                            ? EntityActionRunner.Tick(world, pendingMove, current.Story.Flags,
                                fieldControl: existingFieldInput || pendingMove is not null) : null;
                        var active = tickResult is not null ? new ActiveExploration(MapEventDispatcher.Roof(tickResult.World)) : current.Active;
                        var story = current.Story;
                        if (tickResult?.Failure is { } failure)
                        {
                            story = story.Copy(story.Cursor, story.Wait, simulationTick: checked(story.SimulationTick + 1));
                            current = ProgramRunner.Commit(current, active, story, observations, "entity-action-stopped", failure.Field);
                            return ProgramRunner.Failure(current, observations, failure);
                        }
                        if (story.Wait is EntityWait { PendingMove: not null } fieldMove && tickResult?.FieldMove is { } field)
                        {
                            story = story.Copy(null, field.Moved && field.Event?.Kind != ExplorationEventKind.Warp
                                ? new EntityWait(fieldMove.Token, tickResult.World.Player, field.Event?.Program) : null,
                                simulationTick: checked(story.SimulationTick + 1));
                            current = ProgramRunner.Commit(current, active, story, observations, "simulation-tick");
                            if (field.DoorOpened)
                                current = ProgramRunner.Commit(current, active, story, observations, "door-opened");
                            if (field.Event is { Kind: ExplorationEventKind.Warp } warp)
                            {
                                if (warp.Program is { } frontier)
                                    current = ProgramRunner.Commit(current, active, story.Copy(frontier), observations, "warp-program");
                                else
                                {
                                    current = ProgramRunner.Commit(current, active, story, observations, "warp-started");
                                    current = MapTransfer.BeginWarp(definition, current, warp, observations);
                                }
                                return ProgramRunner.Run(definition, current, observations);
                            }
                            current = ProgramRunner.Commit(current, active, story, observations,
                                field.Moved ? "movement-started" : "movement-blocked");
                            if (!field.Moved) return ProgramRunner.Run(definition, current, observations);
                            continue;
                        }
                        if (story.Wait is EntityEventFacingWait)
                        { active = MapEventDispatcher.Face(current, active); story = story.Copy(story.Cursor); }
                        else if (story.Wait is TickWait timer)
                            story = timer.Remaining <= 1 ? FinishWait(story) : story.Copy(story.Cursor, timer with { Remaining = timer.Remaining - 1 });
                        else if (story.Wait is EntityWait entityWait && active is ActiveExploration explored &&
                            explored.World.TryResolveEntity(entityWait.Entity, out var awaitedEntity) && !awaitedEntity.Busy)
                            story = story.Cursor is null ? story.Copy(entityWait.AfterMotion) : FinishWait(story);
                        story = story.Copy(story.Cursor, story.Wait, simulationTick: checked(story.SimulationTick + 1));
                        current = ProgramRunner.Commit(current, active, story, observations, playerWait ? "gameplay-wait" : "simulation-tick");
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
        catch (MapTransfer.FadeServiceFailure error) { return ProgramRunner.Failure(error.Snapshot, observations, error.Failure); }
        catch (BattleRuleException error) { return ProgramRunner.Failure(current, observations, error); }
    }

    private static SessionResult Move(ScenarioDefinition definition, SessionSnapshot current, Move move, List<SessionObservation> observations)
    {
        if (!Enum.IsDefined(move.Direction)) return Reject(current, "invalid-direction", "direction");
        var world = current.Exploration!;
        var player = world.PlayerEntity;
        if (player.Busy) return Reject(current, "entity-busy", "player");
        // Admission sees the same control policy as the later player service, without
        // publishing setup or advancing motion/entities before that service.
        var controlled = world.Population is null ? world : world.WithEntity(player with
            { Motion = EntityActionRunner.ControlledMotion(player.Motion) });
        var preview = MapEventDispatcher.Move(controlled, move.Direction, current.Story.Flags);
        if (preview.Outcome.Event is { Kind: ExplorationEventKind.Warp } warp)
        {
            if (warp.Program is null) MapTransfer.Validate(definition, current, preview.World, warp);
        }
        else if (!preview.Outcome.Moved && !preview.Outcome.DoorOpened)
            return ProgramRunner.Result(ProgramRunner.Stop(ProgramRunner.Commit(current,
                new ActiveExploration(world.WithEntity(player with { Motion = player.Motion with
                    { Facing = preview.World.PlayerEntity.Motion.Facing } })), current.Story, observations, "movement-blocked"),
                SessionStopReason.PlayerInput), observations);
        var wait = new EntityWait(new(current.ObservationSequence + 1), world.Player, PendingMove: move.Direction);
        return ProgramRunner.Result(ProgramRunner.Commit(current, current.Active, current.Story.Copy(null, wait),
            observations, "movement-requested"), observations);
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

    private static ProgramLocation? NextCursor(StoryState story) => story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null;
    private static StoryState FinishWait(StoryState story)
    {
        bool legacyClose = story.Wait is DialogueWait { Mode: TextDisplayMode.Single, CloseOnAcknowledgement: true };
        return story.Copy(NextCursor(story), textWindow: legacyClose ? new ClosedTextWindow() : story.TextWindow,
            portraitWindow: legacyClose ? new UnknownPortraitWindow() : story.PortraitWindow);
    }
    private static SessionResult Reject(SessionSnapshot current, string code, string field) => BattleCommandDispatcher.Reject(current, code, field);

}
