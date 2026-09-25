using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ProgramRunner
{
    internal static ProgramLocation Next(ProgramLocation location) => location with { Instruction = checked(location.Instruction + 1) };

    internal static W1TextWait TextSpan(IReadOnlyList<ExplorationTextToken> tokens, WaitToken token, int text, int start)
    {
        int end = start;
        while (end < tokens.Count && tokens[end].Kind != ExplorationTextTokenKind.Wait1) end++;
        return new(token, text, end, end < tokens.Count);
    }

    internal static SessionResult Run(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        for (int budget = 0; budget < 256; budget++)
        {
            try
            {
                if (current.Story.Wait is not null) return Result(current, observations);
                if (current.Story.Cursor is not { } cursor)
                {
                    if (current.Story.Warp is not null)
                    {
                        current = MapTransfer.ReturnVisible(current, observations);
                        continue;
                    }
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
                    if (current.Story.Wait is not null) continue;
                    if (current.Story.Display is { } display &&
                        (display.Visibility != FullFadeVisibility.BaseRestored || display.Current != display.Base))
                        throw new BattleRuleException("field-display-not-visible", "story.display", true);
                    // WaitForEvent replaces the player's script on successful control return.
                    // Setup belongs to its next entity service; physical motion survives here.
                    if (current.Exploration is { Population: not null } world &&
                        world.PlayerEntity is { } player &&
                        (player.Actions is not null || player.Follower is not null || player.WaitingForMotion))
                        current = new(current.SessionId, current.Revision, current.ObservationSequence,
                            new ActiveExploration(world.WithEntity(player with
                                { Actions = null, ActionCursor = 0, Follower = null, WaitingForMotion = false })),
                            current.Story, current.StopReason);
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
                        story = story.Copy(story.Cursor, clearWarp: true);
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
                    case EndProgram { SourceMapScript: true } when current.Story.TextSettings is not null:
                        if (current.Story.TextWindow is OpenTextWindow)
                        {
                            ExplorationTextRunner.ValidateContext(current);
                            story = current.Story.Copy(cursor, new ViewWait(token, !current.Story.LogicalView!.Scrolling, ScriptReturn: true));
                        }
                        else story = ReturnFromScript(current.Story);
                        break;
                    case EndProgram or ReturnProgram:
                        story = Return(current.Story);
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
                        story = story.Copy(call.Target, callers: current.Story.Callers.Append(Next(cursor)),
                            entityServices: call.ActivateEntities && story.TextSettings is not null ? true : null); break;
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
                    case SetCameraTarget target:
                        if (story.TextSettings is not null) throw new BattleRuleException("field-view-camera-command", "program.camera", true);
                        story = story.Copy(story.Cursor, cameraTarget: target.Position, clearCameraEntity: true); break;
                    case SetCameraEntity target:
                        if (story.TextSettings is not null) throw new BattleRuleException("field-view-camera-command", "program.camera", true);
                        story = story.Copy(story.Cursor, cameraEntitySlot: target.Entity is { } tracked ? Entity(current, tracked).Slot : null,
                            clearCameraEntity: target.Entity is null); break;
                    case LoadSceneMap load:
                        if (story.TextSettings is not null) throw new BattleRuleException("field-view-scene-map", "program.map", true);
                        if (story.Warp is not null)
                            throw new BattleRuleException("ordinary-warp-scene-load", "program.map", true);
                        active = new ActiveExploration(MapTransfer.LoadScene(definition.Exploration!, current.Exploration!, load));
                        story = story.Copy(story.Cursor, cameraTarget: load.Camera, clearCameraEntity: true); break;
                    case LoadSceneEntities load:
                        active = new ActiveExploration(SceneEntities.Reload(current.Exploration!, load, story.Flags, token.Value));
                        story = current.Story.Copy(cursor, new EntitySetSpriteWait(token)); break;
                    case OpenPortrait portrait:
                        if (story.TextSettings is not null && story.EntityEvent is not null && portrait.Entity is { } boundActor)
                        {
                            current = ExplorationPortraitRunner.Open(definition, current, boundActor, portrait.Flags, observations, false);
                            story = current.Story.Wait is null ? current.Story.Copy(Next(cursor)) : current.Story;
                            break;
                        }
                        // A skipped lookup or an existing window preserves the gate. Missing
                        // metadata cannot prove absence; an unknown incoming window stays unknown.
                        if (portrait.Entity is null || story.PortraitWindow is not ClosedPortraitWindow) break;
                        var portraitEntity = Entity(current, portrait.Entity.Value);
                        if (portraitEntity.Sprite is not { } portraitSprite || definition.Exploration!.Visuals is not { } visuals ||
                            !visuals.Sprites.TryGetValue(portraitSprite, out var portraitVisual))
                            story = story.Copy(story.Cursor, portraitWindow: new UnknownPortraitWindow());
                        else if (portraitVisual.Portrait is { } portraitId)
                            story = story.Copy(story.Cursor, portraitWindow: new OpenPortraitWindow(portraitId, portrait.Flags));
                        break;
                    case ClosePortrait:
                        if (story.TextSettings is not null && story.EntityEvent is not null)
                        {
                            current = ExplorationPortraitRunner.Close(current, observations, false);
                            story = current.Story.Wait is null ? current.Story.Copy(Next(cursor)) : current.Story;
                            break;
                        }
                        story = story.Copy(story.Cursor, portraitWindow: new ClosedPortraitWindow()); break;
                    case WaitForView:
                        if (current.Story.TextSettings is null)
                            story = current.Story.Copy(cursor, new PresentationWait(token, new(PresentationCueKind.CameraWait)));
                        else
                        {
                            ExplorationTextRunner.ValidateContext(current);
                            story = current.Story.Copy(cursor, new ViewWait(token, !current.Story.LogicalView!.Scrolling));
                        }
                        break;
                    case ShowText text:
                        if (!definition.Exploration!.Texts.ContainsKey(current.Story.TextCursor))
                            throw new BattleRuleException("missing-dialogue-text", "program.text", true);
                        if (text.ExplicitWindows && text.WaitForAcknowledgement && current.Story.TextSettings is not null)
                        {
                            story = ExplorationTextRunner.Begin(definition.Exploration, current, text, token);
                            break;
                        }
                        if (text.ExplicitWindows && !program.EntitiesRunning &&
                            current.Story is { EntityEvent: { } context, PortraitWindow: ClosedPortraitWindow,
                                Continuation: ProgramContinuation.FieldInput, EnteringBattle: null } &&
                            current.Exploration!.TryResolveEntity(context.Entity, out var eventActor) &&
                            eventActor.Sprite is { } eventSprite && definition.Exploration.Visuals is { } eventVisuals &&
                            eventVisuals.Sprites.TryGetValue(eventSprite, out var eventVisual) && eventVisual.Portrait is null &&
                            definition.Exploration.TextTokens.TryGetValue(current.Story.TextCursor, out var tokens) &&
                            tokens.Any(part => part.Kind == ExplorationTextTokenKind.Wait1) &&
                            tokens.All(part => part.Kind is not (ExplorationTextTokenKind.Unsupported or ExplorationTextTokenKind.Wait2) &&
                                (part.Kind != ExplorationTextTokenKind.MemberName || part.Member < definition.Exploration.MemberNames.Count)))
                        {
                            story = current.Story.Copy(cursor, TextSpan(tokens, token, current.Story.TextCursor, 0),
                                textCursor: checked(current.Story.TextCursor + 1),
                                textWindow: new OpenTextWindow(current.Story.TextCursor, text.Mode,
                                    text.UseEventSpeaker ? context.Entity : text.Speaker, text.SpeakerFlags));
                            break;
                        }
                        story = current.Story.Copy(text.WaitForAcknowledgement ? cursor : Next(cursor),
                            text.WaitForAcknowledgement ? new DialogueWait(token, current.Story.TextCursor, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags,
                                CloseOnAcknowledgement: !text.ExplicitWindows) : null,
                            textCursor: checked(current.Story.TextCursor + 1),
                            portraitWindow: text.ExplicitWindows ? current.Story.PortraitWindow : new UnknownPortraitWindow(
                                text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags),
                            textWindow: new OpenTextWindow(current.Story.TextCursor, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags)); break;
                    case WaitForTextInput:
                        if (current.Story.TextWindow is not OpenTextWindow open)
                            throw new BattleRuleException("text-input-without-window", "program.text");
                        story = current.Story.Copy(cursor, new DialogueWait(token, open.Text, open.Mode, open.Speaker, open.SpeakerFlags,
                            CloseOnAcknowledgement: false, InputFirstEntityService: program.EntitiesRunning)); break;
                    case CloseText:
                        if (current.Story.LogicalText is { Open: true } logicalWindow)
                        {
                            ExplorationTextRunner.ValidateContext(current);
                            story = current.Story.Copy(cursor, new TextCloseWait(token), logicalText:
                                logicalWindow with { AnimationCounter = 0, AnimationLength = 8, Moving = true });
                            break;
                        }
                        story = story.Copy(story.Cursor, textWindow: new ClosedTextWindow(),
                            portraitWindow: story.PortraitWindow is UnknownPortraitWindow ? new UnknownPortraitWindow() : story.PortraitWindow); break;
                    case ChooseYesNo choice:
                        story = current.Story.Copy(cursor, new ChoiceWait(token, choice.ResultFlag)); break;
                    case WaitProgramTicks ticks when ticks.Ticks > 0:
                        story = current.Story.Copy(cursor, new TickWait(token, ticks.Ticks)); break;
                    case WaitProgramTicks: break;
                    case PresentCue cue:
                        if (cue.FullBlack is not null || current.Story.Display is not null && MapTransfer.IsPalette(cue))
                        {
                            MapTransfer.ValidateCue(cue);
                            current = MapTransfer.BeginFade(current, current.Story, cue.Kind,
                                FullFadePurpose.Script, cue.FullBlack!.Period, observations);
                            continue;
                        }
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
                            var configured = motion.Installation == EntityScriptInstallation.Preserve ? entity.Motion :
                                entity.Motion with { WaitTimer = checked((byte)entity.Slot),
                                    FlagsA = motion.Installation == EntityScriptInstallation.SlotTimerClearCollision
                                        ? (byte)(entity.Motion.FlagsA & 0x9F) : entity.Motion.FlagsA };
                            return entity with { Motion = configured, Actions = motion.Actions, ActionCursor = 0,
                                WaitingForMotion = false, Follower = null };
                        });
                        if (motion.Wait) story = current.Story.Copy(cursor, new EntityWait(token, motion.Entity,
                            Completion: motion.Installation == EntityScriptInstallation.Preserve
                                ? EntityWaitCompletion.NotBusy : EntityWaitCompletion.ScriptIdle));
                        break;
                    case WaitForEntity wait:
                        if (Entity(current, wait.Entity).Busy) story = current.Story.Copy(cursor, new EntityWait(token, wait.Entity));
                        break;
                    case TransferToMap transfer:
                        story = story.Copy(story.Cursor, clearWarp: true);
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

    private static StoryState Return(StoryState story) => story.Callers.Count == 0 ? story.Copy(null) :
        story.Copy(story.Callers[^1], callers: story.Callers.SkipLast(1));

    internal static StoryState ReturnFromScript(StoryState story) =>
        Return(story).Copy(story.Callers.Count == 0 ? null : story.Callers[^1],
            textSettings: story.TextSettings! with { ViewSpeed = 0 });

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
            FullFadeWait { LogicalDone: true } or FieldTextWait { LogicalDone: true } => SessionStopReason.PresentationWait,
            DialogueWait or W1TextWait or ChoiceWait or PresentationWait or EntitySpriteWait or EntitySetSpriteWait => SessionStopReason.PresentationWait,
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
