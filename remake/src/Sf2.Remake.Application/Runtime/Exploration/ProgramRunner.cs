using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ProgramRunner
{
    internal static ProgramLocation Next(ProgramLocation location) => location with { Instruction = checked(location.Instruction + 1) };

    internal static SessionResult Run(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        for (int budget = 0; budget < 256; budget++)
        {
            try
            {
                if (current.Story.Wait is not null) return Result(current, observations);
                if (current.Story.Cursor is not { } cursor)
                {
                    if (current.Story.Continuation is ProgramContinuation.VictoryProgramFinished or ProgramContinuation.DefeatProgramFinished)
                    {
                        current = BattleOutcome.Continue(definition, current, observations);
                        continue;
                    }
                    if (current.Story.Continuation == ProgramContinuation.OutcomeMapLoaded)
                        current = Commit(current, current.Active, current.Story.Copy(null,
                            continuation: ProgramContinuation.FieldInput, clearEnteringBattle: true), observations, "battle-returned");
                    if (current.Story.Continuation is ProgramContinuation.MapLoaded or ProgramContinuation.BeforeBattleFinished or ProgramContinuation.BattleLoadFinished)
                    {
                        current = MapTransfer.Continue(definition, current, observations);
                        continue;
                    }
                    if (current.Story.Continuation == ProgramContinuation.BattleStartFinished)
                    {
                        var result = BattleAdvancer.Advance(current, observations);
                        return result with { Snapshot = result.Snapshot.WithStory(current.Story.Copy(null,
                            continuation: ProgramContinuation.FieldInput)) };
                    }
                    current = MapEventDispatcher.Finish(current, observations);
                    return Result(Stop(current, SessionStopReason.PlayerInput), observations);
                }
                if (!definition.Exploration!.Programs.TryGetValue(cursor.Program, out var program) ||
                    cursor.Instruction < 0 || cursor.Instruction >= program.Instructions.Count)
                    throw new BattleRuleException("program-cursor", "program.cursor");
                StoryInstruction instruction = program.Instructions[cursor.Instruction];
                if (instruction is UnsupportedInstruction unsupported)
                    throw new BattleRuleException("program-opcode", unsupported.Source + ":" + unsupported.Opcode, true);
                var story = current.Story.Copy(Next(cursor));
                var active = current.Active;
                var token = new WaitToken(checked(current.ObservationSequence + 1));
                switch (instruction)
                {
                    case ResetPartyBattleStats:
                        active = new ActiveExploration(current.Exploration!.WithParty(BattleOutcome.Heal(definition, current.Exploration.Party, all: true)));
                        break;
                    case ReturnBattleMap:
                        var returning = current.Story.OutcomeReturn ?? throw new BattleRuleException("outcome-return", "story.outcomeReturn");
                        current = MapTransfer.Apply(definition, current, returning.Map, returning.Position, returning.Facing,
                            MapLoadMode.Rebuild, story, observations);
                        continue;
                    case RetiredMap3EntityScratch:
                        var reloaded = current.Exploration!;
                        if (cursor != new ProgramLocation("byte-513a8", 1) || reloaded.Map.Value != "map-3" || current.Story.Continuation is not (ProgramContinuation.MapLoaded or ProgramContinuation.OutcomeMapLoaded) ||
                            current.Story.TextWindow is not ClosedTextWindow || !current.Story.Flags.Contains(603))
                            throw new BattleRuleException("inactive-window-scratch-context", "program.retiredMap3Entity", true);
                        if (reloaded.TryResolveEntity(new("entity-142"), out var existing))
                        {
                            active = new ActiveExploration(reloaded.Hide(existing, removeAliases: false));
                            break;
                        }
                        if (!current.Story.Flags.Contains(1) || !reloaded.AllEntities.Any(entity =>
                                entity.Entity.Value == "entity-142" && !entity.Visible && entity.Motion.X == 0x7000 && entity.Motion.Y == 0x7000))
                            throw new BattleRuleException("inactive-window-scratch-context", "program.retiredMap3Entity", true);
                        // Accepted #425: normal reload drains the old presentation work and clears
                        // windows; the first later window is rebuilt before publication. The real
                        // preceding hide/FF tombstone stays; these four stores affect inactive scratch.
                        break;
                    case EndProgram or ReturnProgram:
                        story = current.Story.Callers.Count == 0 ? current.Story.Copy(null) :
                            current.Story.Copy(current.Story.Callers[^1], callers: current.Story.Callers.SkipLast(1));
                        break;
                    case JumpProgram jump: story = story.Copy(jump.Target); break;
                    case BranchFlag branch when current.Story.Flags.Contains(branch.Flag) == branch.WhenSet:
                        story = story.Copy(branch.Target); break;
                    case BranchFlag: break;
                    case BranchEntityCoordinates branch:
                        var coordinates = Entity(current, branch.Entity).Motion;
                        if ((coordinates.X == branch.X && coordinates.Y == branch.Y) == branch.WhenEqual)
                            story = story.Copy(branch.Target);
                        break;
                    case CallProgram call:
                        story = story.Copy(call.Target, callers: current.Story.Callers.Append(Next(cursor))); break;
                    case WriteFlag flag:
                        story = story.Copy(story.Cursor, flags: Flags(current.Story, flag.Flag, flag.Value)); break;
                    case JoinPartyMember join:
                        var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("party-flag-layout", "world.partyFlags", true);
                        var joined = MapPartyMembership.Join(current.Story.Flags, layout, join.Member);
                        story = story.Copy(story.Cursor, flags: joined.Flags, partyLists: joined.Lists); break;
                    case FollowEntity follow:
                        int leaderSlot = Entity(current, follow.Leader).Slot;
                        active = EditEntity(current, follow.Entity, entity => FollowerMotion.Install(entity, leaderSlot, follow.OffsetX, follow.OffsetY)); break;
                    case SetTextCursor text: story = story.Copy(story.Cursor, textCursor: text.Text); break;
                    case SetDialogueSpeaker speaker: story = story.Copy(story.Cursor, speaker: speaker.Entity, clearSpeaker: speaker.Entity is null); break;
                    case SetCameraTarget target: story = story.Copy(story.Cursor, cameraTarget: target.Position, clearCameraEntity: true); break;
                    case SetCameraEntity target:
                        story = story.Copy(story.Cursor, cameraEntitySlot: target.Entity is { } tracked ? Entity(current, tracked).Slot : null,
                            clearCameraEntity: target.Entity is null); break;
                    case LoadSceneMap load:
                        active = new ActiveExploration(MapTransfer.LoadScene(definition.Exploration!, current.Exploration!, load));
                        story = story.Copy(story.Cursor, cameraTarget: load.Camera, clearCameraEntity: true); break;
                    case LoadSceneEntities load:
                        active = new ActiveExploration(SceneEntities.Reload(current.Exploration!, load, story.Flags, token.Value));
                        story = current.Story.Copy(cursor, new EntitySetSpriteWait(token)); break;
                    case ShowText text:
                        if (!definition.Exploration!.Texts.ContainsKey(current.Story.TextCursor))
                            throw new BattleRuleException("missing-dialogue-text", "program.text", true);
                        story = current.Story.Copy(cursor, new DialogueWait(token, current.Story.TextCursor, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags),
                            textCursor: checked(current.Story.TextCursor + 1),
                            textWindow: new OpenTextWindow(current.Story.TextCursor, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags)); break;
                    case CloseText: story = story.Copy(story.Cursor, textWindow: new ClosedTextWindow()); break;
                    case ChooseYesNo choice:
                        story = current.Story.Copy(cursor, new ChoiceWait(token, choice.ResultFlag)); break;
                    case WaitProgramTicks ticks when ticks.Ticks > 0:
                        story = current.Story.Copy(cursor, new TickWait(token, ticks.Ticks)); break;
                    case WaitProgramTicks: break;
                    case PresentCue cue:
                        if (cue.Entity is { } reference) cue = cue with { Entity = Entity(current, reference).Entity };
                        GestureRestore? restore = null;
                        if (cue is { Kind: PresentationCueKind.Gesture, Resource: "nod", Entity: { } nodding })
                            active = EditEntity(current, nodding, entity => entity with { Motion = entity.Motion with { AnimationCounter = 255 } });
                        if (cue is { Kind: PresentationCueKind.Gesture, Resource: "shiver", Entity: { } shivering })
                        {
                            var entity = Entity(current, shivering);
                            var world = current.Exploration!;
                            restore = new(entity.Motion.AnimationCounter, world.SpriteSize);
                            active = new ActiveExploration(world.WithEntities(world.AllEntities.Select(row => row.Slot == entity.Slot
                                ? row with { Motion = row.Motion with { AnimationCounter = 255 } } : row), spriteSize: 21));
                        }
                        story = current.Story.Copy(cursor, new PresentationWait(token, cue, restore)); break;
                    case SetEntitySprite sprite:
                        var changedSprite = Entity(current, sprite.Entity);
                        changedSprite = changedSprite with { Sprite = sprite.Sprite,
                            SpriteRequest = checked(changedSprite.SpriteRequest + 1), WaitingForSprite = true };
                        active = new ActiveExploration(current.Exploration!.WithEntity(changedSprite));
                        story = current.Story.Copy(cursor, new EntitySpriteWait(token, changedSprite.Slot, changedSprite.SpriteRequest));
                        break;
                    case SetEntityFacing facing:
                        var faced = Entity(current, facing.Entity);
                        faced = faced with { Motion = faced.Motion with { Facing = facing.Facing } };
                        if (facing.RefreshSprite)
                        {
                            faced = faced with { SpriteRequest = checked(faced.SpriteRequest + 1), WaitingForSprite = true };
                            story = current.Story.Copy(cursor, new EntitySpriteWait(token, faced.Slot, faced.SpriteRequest));
                        }
                        active = new ActiveExploration(current.Exploration!.WithEntity(faced)); break;
                    case SetEntityPriority priority:
                        active = EditEntity(current, priority.Entity, entity => entity with { Priority = priority.Value }); break;
                    case SetEntityVisibility visible:
                        active = EditEntity(current, visible.Entity, entity => entity with { Visible = visible.Visible }); break;
                    case HideMapEntity hide:
                        active = new ActiveExploration(current.Exploration!.Hide(Entity(current, hide.Entity), hide.RemoveAliases)); break;
                    case SetEntityPosition position:
                        active = EditEntity(current, position.Entity, entity => entity with
                        {
                            Motion = entity.Motion with
                            {
                                X = (short)(position.Position.X * 384), XDestination = (short)(position.Position.X * 384),
                                Y = (short)(position.Position.Y * 384), YDestination = (short)(position.Position.Y * 384),
                                Facing = position.Facing,
                            },
                        }); break;
                    case StartEntityMotion motion:
                        active = EditEntity(current, motion.Entity, entity =>
                        {
                            return entity with { Actions = motion.Actions, ActionCursor = 0, WaitingForMotion = false, Follower = null };
                        });
                        if (motion.Wait) story = current.Story.Copy(cursor, new EntityWait(token, motion.Entity));
                        break;
                    case WaitForEntity wait:
                        if (Entity(current, wait.Entity).Busy) story = current.Story.Copy(cursor, new EntityWait(token, wait.Entity));
                        break;
                    case TransferToMap transfer:
                        current = MapTransfer.Apply(definition, current, transfer.Map, transfer.Position, transfer.Facing,
                            transfer.Mode, story, observations);
                        continue;
                    default: throw new BattleRuleException("program-instruction", "program", true);
                }
                current = Commit(current, active, story, observations, "program-instruction", instruction.GetType().Name, cursor);
            }
            catch (BattleRuleException error) { return Failure(current, observations, error); }
        }
        return Result(Stop(current, SessionStopReason.SimulationWait), observations);
    }

    internal static IEnumerable<int> Flags(StoryState story, int flag, bool value) =>
        value ? story.Flags.Append(flag) : story.Flags.Where(existing => existing != flag);

    internal static ExplorationEntity Entity(SessionSnapshot current, EntityRef entity)
    {
        if (current.Exploration is not { } world || !world.TryResolveEntity(entity, out var result))
            throw new BattleRuleException("program-entity", "program.entity", true);
        return result;
    }
    private static ActiveExploration EditEntity(SessionSnapshot current, EntityRef entity, Func<ExplorationEntity, ExplorationEntity> edit) =>
        new(current.Exploration!.WithEntity(edit(Entity(current, entity))));

    internal static SessionSnapshot Commit(SessionSnapshot current, ActiveSessionState active, StoryState story,
        List<SessionObservation> observations, string kind, string? detail = null, ProgramLocation? program = null)
    {
        long revision = checked(current.Revision + 1), sequence = checked(current.ObservationSequence + 1);
        observations.Add(new(sequence, revision, kind, Detail: detail, Program: program));
        var reason = story.Wait switch
        {
            DialogueWait or ChoiceWait or PresentationWait or EntitySpriteWait or EntitySetSpriteWait => SessionStopReason.PresentationWait,
            EntityWait or TickWait => SessionStopReason.SimulationWait,
            _ => SessionStopReason.SimulationWait,
        };
        return new(current.SessionId, revision, sequence, active, story, reason);
    }
    internal static SessionSnapshot Stop(SessionSnapshot current, SessionStopReason reason) =>
        new(current.SessionId, current.Revision, current.ObservationSequence, current.Active, current.Story, reason);
    internal static SessionResult Result(SessionSnapshot current, List<SessionObservation> observations) =>
        new(current, observations.AsReadOnly(), current.StopReason);
    internal static SessionResult Failure(SessionSnapshot current, List<SessionObservation> observations, BattleRuleException error)
    {
        var reason = error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Faulted;
        return new(Stop(current, reason), observations.AsReadOnly(), reason,
            new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
    }
}
