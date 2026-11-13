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

    // Source-default entry for standalone program comparisons.
    internal static SessionResult Run(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations,
        IReadOnlyList<ProgramControlRead>? prefixReads = null) => Run(definition, current, observations, Gameplay.RuleCompositions.Sf2(), prefixReads);

    internal static SessionResult Run(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations,
        SessionRules rules, IReadOnlyList<ProgramControlRead>? prefixReads = null, WaitToken? fieldMoveToken = null)
    {
        List<ProgramControlRead> reads = prefixReads?.ToList() ?? [];
        SessionResult Finish(SessionResult result) => result with { ProgramControlReads = reads.AsReadOnly() };
        for (int budget = 0; budget < 256; budget++)
        {
            try
            {
                if (current.Story.Wait is ChoiceWait or NameWait) current = BattleEntrySkip.Disarm(current, observations, "choice");
                if (current.Story.Wait is not null) return Finish(Result(current, observations));
                if (current.Story.Cursor is not { } cursor)
                {
                    if (current.Story.Continuation == ProgramContinuation.DefeatedProgramFinished)
                        return Finish(BattleDefeatedProgram.Resume(definition, current, observations, rules));
                    if (current.BattleRound is { Program: true })
                        return Finish(BattleRoundContinuation.ResumeProgram(current, observations, rules));
                    if (current.Story.BattleRequest && current.Story.EventCaller is null)
                    {
                        current = SourceRandomBattleRunner.Consume(definition, current, observations, rules);
                        continue;
                    }
                    if (current.Story.ScriptWarp is { } pendingWarp && current.Story.Continuation is not
                        (ProgramContinuation.VictoryProgramFinished or ProgramContinuation.OutcomeMapLoaded))
                    {
                        var warpEvent = new ExplorationEvent(ExplorationEventKind.Warp, null, null, null, null,
                            pendingWarp.Map, pendingWarp.Position, pendingWarp.Facing);
                        MapTransfer.Validate(definition, current, current.Exploration!, warpEvent, rules, sourcePlacement: true);
                        current = current.WithStory(current.Story.Copy(null, clearScriptWarp: true, clearEventCaller: true));
                        current = MapTransfer.BeginWarp(definition, current, warpEvent, observations, rules, sourcePlacement: true);
                        continue;
                    }
                    if (current.Story.Warp is not null)
                    {
                        current = MapTransfer.ReturnVisible(current, observations);
                        continue;
                    }
                    if (current.Story.Continuation is ProgramContinuation.VictoryProgramFinished or ProgramContinuation.DefeatProgramFinished)
                    {
                        current = BattleOutcome.Continue(definition, current, observations, rules);
                        continue;
                    }
                    if (current.Story.Continuation == ProgramContinuation.OutcomeMapLoaded)
                    {
                        current = Commit(current, current.Active, current.Story.Copy(null,
                            continuation: ProgramContinuation.FieldInput, clearEnteringBattle: true, clearDefeatedScene: true,
                            saveAnchor: new(current.Exploration!.Map, current.Exploration.PlayerEntity.Position,
                                current.Exploration.PlayerEntity.Motion.Facing)), observations, "battle-returned");
                        // Destination init may have queued a second warp during the outcome return.
                        if (current.Story.ScriptWarp is not null) continue;
                    }
                    if (current.Story.Continuation is ProgramContinuation.MapLoaded or ProgramContinuation.BeforeBattleFinished or ProgramContinuation.BattleLoadFinished)
                    {
                        current = MapTransfer.Continue(definition, current, observations, rules);
                        continue;
                    }
                    if (current.Story.Continuation == ProgramContinuation.BattleStartFinished)
                    {
                        current = BattleEntrySkip.Disarm(current, observations, "entry-finished");
                        current = current.WithStory(current.Story.Copy(null, continuation: ProgramContinuation.FieldInput));
                        return Finish(BattleAdvancer.Advance(current, observations, rules));
                    }
                    current = MapEventDispatcher.Finish(current, observations, fieldMoveToken);
                    if (current.Story.Wait is not null) continue;
                    if (current.Story.Display is { } display &&
                        (display.Visibility != FullFadeVisibility.BaseRestored || display.Current != display.Base))
                        throw new BattleRuleException("field-display-not-visible", "story.display", true);
                    // WaitForEvent replaces the player's script on successful control return.
                    // Setup belongs to its next entity service; physical motion survives here.
                    if (current.Exploration is { } world && (world.Population is not null || world.Definition.Caravan is not null) &&
                        world.PlayerEntity is { } player &&
                        (player.Actions is not null || player.Follower is not null || player.WaitingForMotion))
                        current = new(current.SessionId, current.Revision, current.ObservationSequence,
                            new ActiveExploration(world.WithEntity(player with
                                { Actions = null, ActionCursor = 0, Follower = null, WaitingForMotion = false })),
                            current.Story, current.StopReason);
                    if (current.Story.LogicalView is { } view && current.Exploration is { } field)
                        current = current.WithStory(current.Story.Copy(null,
                            logicalView: view with { TargetSlot = field.PlayerEntity.Slot }));
                    if (current.Exploration is { } returned && definition.Exploration?.PartyFlags is { } membership && returned.Party.NewBattle is not null)
                    {
                        var lists = MapPartyMembership.Rebuild(current.Story.Flags, membership);
                        var members = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                            .Where(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember is not null)
                            .GroupBy(row => row.Initialization!.AllyPartyMember!.Value).ToDictionary(group => group.Key, group => group.First().Actor);
                        var party = returned.Party;
                        var activeMembers = lists.Active.Select(member => members.TryGetValue(member, out var actor) ? actor :
                            throw new BattleRuleException("party-member-definition", "party.active", true));
                        current = new(current.SessionId, current.Revision, current.ObservationSequence,
                            new ActiveExploration(returned.WithParty(new(party.Encounter, party.Actors, party.MainSeed,
                                party.ThinkingSeed, party.Gold, party.NewBattle, activeMembers, party.ItemCustody, party.StoryFlags, party.Visuals, party.RetainedSourceSlots, party.DeathCredit))),
                            // Gameplay consumes current flags; the source counted lists change
                            // only at their actual UpdateForce consumers (join, LEADER, entry).
                            current.Story.Copy(null), current.StopReason);
                    }
                    return Finish(Result(Stop(current, SessionStopReason.PlayerInput), observations));
                }
                if (!definition.Programs.TryGetValue(cursor.Program, out var program) ||
                    cursor.Instruction < 0 || cursor.Instruction >= program.Instructions.Count)
                    throw new BattleRuleException("program-cursor", "program.cursor");
                StoryInstruction instruction = program.Instructions[cursor.Instruction];
                if (current.Story.Continuation == ProgramContinuation.DefeatedProgramFinished)
                    BattleDefeatedProgram.RequireStagingInstruction(instruction);
                if (current.BattleRound is { Program: true } && instruction is not
                    (EndProgram { SourceMapScript: false } or ReturnProgram or JumpProgram or BranchFlag or CallProgram { ActivateEntities: false } or WriteFlag or
                    SetTextCursor or ShowText { ExplicitWindows: false, UseEventSpeaker: false } or CloseText or
                    WaitProgramTicks or ConvertBattleActor or JoinBattleForceAi or SetEntityFacing { Facing: < 4 } or SetCameraEntity or SetCameraTarget or
                    PresentCue { Kind: PresentationCueKind.CameraWait, FullBlack: null, Entity: null, Position: null, Resource: null, DurationTicks: null }))
                    throw new BattleRuleException("battle-program-instruction", "program." + instruction.GetType().Name, true);
                // Only synchronous, effect-free dispatch may return arrival to the producing Move.
                // A yielded wait resumes in a new Run, without the move token.
                if (instruction is not (BranchFlag or BranchEntityCoordinates or JumpProgram or
                    CallProgram { ActivateEntities: false } or ReturnProgram or EndProgram { SourceMapScript: false }))
                    fieldMoveToken = null;
                var callersBefore = current.Story.Callers;
                if (instruction is UnsupportedInstruction unsupported)
                    throw new BattleRuleException("program-opcode", unsupported.Source + ":" + unsupported.Opcode, true);
                var story = current.Story.Copy(Next(cursor));
                var active = current.Active;
                var token = new WaitToken(checked(current.ObservationSequence + 1));
                switch (instruction)
                {
                    case OpenShop shop when current.Exploration is not null:
                        story = story.Copy(story.Cursor, new ShopWait(token, shop.Definition));
                        break;
                    case OpenChurch church when current.Exploration is not null:
                        story = story.Copy(story.Cursor, new ChurchWait(token, church.Definition));
                        break;
                    case VehicleInstruction vehicle:
                        (active, story) = CaravanRuntime.Execute(definition, current, vehicle, story, token);
                        break;
                    case ConvertBattleActor convert:
                        current = BattleStoryOperations.Convert(current, convert, observations);
                        story = current.Story.Copy(Next(cursor)); active = current.Active; break;
                    case JoinBattleForceAi joinAi:
                        current = BattleStoryOperations.JoinAi(current, joinAi, observations);
                        story = current.Story.Copy(Next(cursor)); active = current.Active; break;
                    case SetCameraEntity camera when current.BattleRound is { Program: true }:
                        if (camera.Entity is not { } cameraActor) throw new BattleRuleException("battle-camera-entity", "program.camera", true);
                        var cameraVisual = BattleStoryOperations.Visual(current, cameraActor);
                        story = story.Copy(story.Cursor, cameraEntitySlot: cameraVisual.Slot, clearCameraTarget: true); break;
                    case SetEntityFacing facing when current.BattleRound is { Program: true }:
                        var facingActor = current.Battle.Actors.SingleOrDefault(a => a.Actor.Value == facing.Entity.Value)
                            ?? throw new BattleRuleException("battle-story-entity", "program.entity", true);
                        // The source facing opcode skips the lookup itself for HP0 allies.
                        if (facingActor.IsAlly && facingActor.Hp == 0) break;
                        _ = BattleStoryOperations.Visual(current, facing.Entity);
                        active = ((ActiveBattle)current.Active) with { Battle = current.Battle.With(
                            visuals: current.Battle.Visuals!.Face(facingActor.Actor, facing.Facing),
                            actors: current.Battle.Actors.Select(a => a.Actor == facingActor.Actor ? a.With(facing: facing.Facing) : a)) }; break;
                    case ParkRaft park:
                        story = story.Copy(story.Cursor, raft: park.Berth); break;
                    case SetEgressMap egress:
                        story = story.Copy(story.Cursor, egressMap: egress.Map); break;
                    case RequestCurrentMap:
                        story = story.Copy(story.Cursor, battleRequest: true); break;
                    case RestoreCachedRoof:
                        var roofWorld = current.Exploration!;
                        var roof = MapBlockCopyLifecycleReducer.Restore(roofWorld.Layout, roofWorld.RoofState, new(false, false));
                        active = new ActiveExploration(roofWorld.WithLayout(roof.Layout, roof.LifecycleState)); break;
                    case SourceRandomBattle random:
                        current = SourceRandomBattleRunner.Check(current, random, cursor, observations);
                        continue;
                    case ResetPartyBattleStats reset:
                        active = new ActiveExploration(current.Exploration!.WithParty(OutcomeReturnRules.Heal(rules.OutcomeReturn, definition, current.Exploration.Party, all: !reset.LivingOnly)));
                        break;
                    case ReturnBattleMap:
                        // alt_MainLoopEntry returns through SwitchMap without CheckBattle.
                        var eventWarp = current.Story.ScriptWarp;
                        story = story.Copy(story.Cursor, clearWarp: true, clearScriptWarp: true);
                        var returning = eventWarp is null ? current.Story.OutcomeReturn ?? throw new BattleRuleException("outcome-return", "story.outcomeReturn")
                            : new FieldReturnAnchor(eventWarp.Map, eventWarp.Position, eventWarp.Facing);
                        current = MapTransfer.Apply(definition, current, returning.Map, returning.Position, returning.Facing,
                            MapLoadMode.Rebuild, story, observations, rules,
                            sourcePlacement: eventWarp is not null || current.Story.EnteringBattle?.Outcome is { BattleOrigin: not null } board && returning.Map == board.BattleMap,
                            checkBattle: false);
                        current = ExplorationMusicRunner.SelectFieldMusic(definition.Exploration!, current, observations);
                        continue;
                    case SourceStoryInstruction source:
                        try { active = SourceStoryRules.Apply(rules.SourceStory, definition, current, source, cursor); }
                        catch (BattleRuleException error)
                        {
                            var failed = Failure(current, observations, error);
                            return Finish(failed with { Failure = failed.Failure! with
                                { Message = $"{rules.SourceStory.Identity}: {source.GetType().Name} at {cursor.Program}:{cursor.Instruction}; {failed.Failure.Message}" } });
                        }
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
                    case BranchInteractionFacing branch:
                        var interaction = current.Story.EntityEvent ?? throw new BattleRuleException("entity-event-context", "program.facing");
                        if ((interaction.InteractionFacing == branch.Facing) == branch.WhenEqual) story = story.Copy(branch.Target);
                        break;
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
                    case OutcomePartyInstruction outcomeParty:
                        (active, story) = OutcomePartyOperations.Apply(definition, current, outcomeParty, story);
                        break;
                    case JoinPartyMember join:
                        var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("party-flag-layout", "world.partyFlags", true);
                        var joined = MapPartyMembership.Join(current.Story.Flags, layout, join.Member);
                        active = JoinResources(definition, current, join);
                        story = story.Copy(story.Cursor, flags: joined.Flags, partyLists: joined.Lists); break;
                    case AddExplorationFollower added:
                        var followerWorld = current.Exploration!;
                        int lastSlot = followerWorld.CurrentEntities.LastOrDefault();
                        active = new ActiveExploration(followerWorld.WithFollower(FollowerMotion.Install(Entity(current, added.Entity), lastSlot, -24, 0)));
                        break;
                    case FollowEntity follow:
                        int leaderSlot = Entity(current, follow.Leader).Slot;
                        active = new ActiveExploration(current.Exploration!.WithFollower(FollowerMotion.Install(Entity(current, follow.Entity), leaderSlot, follow.OffsetX, follow.OffsetY))); break;
                    case SetTextCursor text: story = story.Copy(story.Cursor, textCursor: text.Text); break;
                    case SetDialogueSpeaker speaker: story = story.Copy(story.Cursor, speaker: speaker.Entity, clearSpeaker: speaker.Entity is null); break;
                    case SetPlaneAutoscroll scroll:
                        story = story.Copy(story.Cursor, logicalView: ExplorationViewRunner.Autoscroll(
                            story.LogicalView ?? throw new BattleRuleException("field-view-context", "program.view", true), scroll)); break;
                    case AlignBackgroundToViewTarget:
                        story = story.Copy(story.Cursor, logicalView: ExplorationViewRunner.AlignBackground(current.Exploration!,
                            story.LogicalView ?? throw new BattleRuleException("field-view-context", "program.view", true))); break;
                    case SetPlanePosition plane:
                        story = story.Copy(story.Cursor, logicalView: ExplorationViewRunner.Position(
                            story.LogicalView ?? throw new BattleRuleException("field-view-context", "program.view", true), plane)); break;
                    case SetCameraSpeed speed:
                        if (story.TextSettings is null) throw new BattleRuleException("field-text-settings", "program.camera", true);
                        story = story.Copy(story.Cursor, textSettings: story.TextSettings with { ViewSpeed = speed.Speed }); break;
                    case SetCameraTarget target:
                        if (story.Continuation == ProgramContinuation.DefeatedProgramFinished && story.EnteringBattle?.Outcome?.BattleOrigin is { } cameraOrigin)
                            target = target with { Position = new(target.Position.X + cameraOrigin.X, target.Position.Y + cameraOrigin.Y) };
                        if (story.TextSettings is not null)
                        {
                            ExplorationTextRunner.ValidateContext(current);
                            story = story.Copy(story.Cursor, logicalView: ExplorationViewRunner.SetDestination(story.LogicalView!, target.Position),
                                clearCameraEntity: true, clearCameraTarget: true);
                        }
                        else story = story.Copy(story.Cursor, cameraTarget: target.Position, clearCameraEntity: true);
                        break;
                    case SetCameraEntity target:
                        if (story.TextSettings is not null)
                        {
                            ExplorationTextRunner.ValidateContext(current);
                            var cameraEntity = target.Entity is { } cameraReference ? Entity(current, cameraReference) : null;
                            if (cameraEntity?.Slot == 63)
                                throw new BattleRuleException("field-view-target", "program.camera", true);
                            story = story.Copy(story.Cursor, logicalView: story.LogicalView! with { TargetSlot = cameraEntity?.Slot },
                                clearCameraEntity: true, clearCameraTarget: true);
                            break;
                        }
                        story = story.Copy(story.Cursor, cameraEntitySlot: target.Entity is { } tracked ? Entity(current, tracked).Slot : null,
                            clearCameraEntity: target.Entity is null); break;
                    case LoadSceneMap load:
                        var scene = MapTransfer.LoadScene(definition.Exploration!, current.Exploration!, load);
                        story = story.TextSettings is not null ? MapTransfer.BindScene(current, scene, load).Copy(story.Cursor) :
                            story.Copy(story.Cursor, cameraTarget: load.Camera, clearCameraEntity: true);
                        active = new ActiveExploration(scene); break;
                    case ReloadDefaultEntities reloadList:
                        var live = current.Exploration!;
                        var list = definition.Exploration!.Maps[reloadList.Map];
                        var main = live.PlayerEntity;
                        active = new ActiveExploration(SceneEntities.Reload(live,
                            new(list.Population!, main.Position, main.Motion.Facing, reloadList.Setup is null ? list.Entities : list.Setups[reloadList.Setup].Entities), story.Flags, token.Value));
                        story = current.Story.Copy(cursor, new EntitySetSpriteWait(token)); break;
                    case LoadSetupEntities setupEntities:
                        var sourceMap = definition.Exploration!.Maps[current.Exploration!.Map].SelectSetup(story.Flags);
                        var population = sourceMap.Population ?? throw new BattleRuleException("missing-map-population", "program.entities", true);
                        active = new ActiveExploration(SceneEntities.Reload(current.Exploration,
                            new(population, setupEntities.Position, setupEntities.Facing, sourceMap.Entities), story.Flags, token.Value));
                        story = current.Story.Copy(cursor, new EntitySetSpriteWait(token)); break;
                    case LoadSceneEntities load:
                        CaravanRuntime.RequireOrdinaryMapOperation(current.Exploration!);
                        active = new ActiveExploration(SceneEntities.Reload(current.Exploration!, load, story.Flags, token.Value));
                        story = current.Story.Copy(cursor, new EntitySetSpriteWait(token)); break;
                    case OpenPortrait portrait:
                        EntityRef? portraitActor = portrait.UseEventSpeaker
                            ? story.EntityEvent?.Entity ?? throw new BattleRuleException("portrait-event-speaker", "program.portrait", true)
                            : portrait.Entity;
                        if (story.TextSettings is not null && ExplorationPortraitRunner.HasCaller(story) && portraitActor is { } boundActor)
                        {
                            current = ExplorationPortraitRunner.Open(definition, current, boundActor, portrait.Flags, observations, false);
                            story = current.Story.Wait is null ? current.Story.Copy(Next(cursor)) : current.Story;
                            break;
                        }
                        // A skipped lookup or an existing window preserves the gate. Missing
                        // metadata cannot prove absence; an unknown incoming window stays unknown.
                        if (portraitActor is null || story.PortraitWindow is not ClosedPortraitWindow) break;
                        var portraitEntity = Entity(current, portraitActor.Value);
                        if (portraitEntity.Sprite is not { } portraitSprite || definition.Exploration!.Visuals is not { } visuals ||
                            !visuals.Sprites.TryGetValue(portraitSprite, out var portraitVisual))
                            story = story.Copy(story.Cursor, portraitWindow: new UnknownPortraitWindow());
                        else if (portraitVisual.Portrait is { } portraitId)
                            story = story.Copy(story.Cursor, portraitWindow: new OpenPortraitWindow(portraitId, portrait.Flags));
                        break;
                    case ClosePortrait:
                        if (story.TextSettings is not null && ExplorationPortraitRunner.HasCaller(story))
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
                        if (current.BattleRound is { Program: true } && text.Speaker is { } battleSpeaker)
                            _ = BattleStoryOperations.Visual(current, battleSpeaker);
                        int selectedText = text.TextId ?? current.Story.TextCursor;
                        int nextTextCursor = text.TextId is null ? checked(current.Story.TextCursor + 1) : current.Story.TextCursor;
                        if (!definition.Texts.ContainsKey(selectedText))
                            throw new BattleRuleException("missing-dialogue-text", "program.text", true);
                        if (text.ExplicitWindows && current.Story.TextSettings is not null)
                        {
                            story = ExplorationTextRunner.Begin(definition.Exploration!, current, text, token, definition);
                            break;
                        }
                        if (text.ExplicitWindows && !program.EntitiesRunning &&
                            current.Story is { EntityEvent: { } context, PortraitWindow: ClosedPortraitWindow,
                                Continuation: ProgramContinuation.FieldInput, EnteringBattle: null } &&
                            current.Exploration!.TryResolveEntity(context.Entity, out var eventActor) &&
                            eventActor.Sprite is { } eventSprite && definition.Exploration!.Visuals is { } eventVisuals &&
                            eventVisuals.Sprites.TryGetValue(eventSprite, out var eventVisual) && eventVisual.Portrait is null &&
                            definition.Exploration.TextTokens.TryGetValue(selectedText, out var tokens) &&
                            tokens.Any(part => part.Kind == ExplorationTextTokenKind.Wait1) &&
                            tokens.All(part => part.Kind is not (ExplorationTextTokenKind.Unsupported or ExplorationTextTokenKind.Wait2 or ExplorationTextTokenKind.Delay1 or ExplorationTextTokenKind.Delay3) &&
                                (part.Kind != ExplorationTextTokenKind.MemberName || part.Member < definition.Exploration.MemberNames.Count)))
                        {
                            story = current.Story.Copy(cursor, TextSpan(tokens, token, selectedText, 0),
                                textCursor: nextTextCursor,
                                textWindow: new OpenTextWindow(selectedText, text.Mode,
                                    text.UseEventSpeaker ? context.Entity : text.Speaker, text.SpeakerFlags));
                            break;
                        }
                        story = current.Story.Copy(text.WaitForAcknowledgement ? cursor : Next(cursor),
                            text.WaitForAcknowledgement ? new DialogueWait(token, selectedText, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags,
                                CloseOnAcknowledgement: !text.ExplicitWindows) : null,
                            textCursor: nextTextCursor,
                            portraitWindow: text.ExplicitWindows ? current.Story.PortraitWindow : new UnknownPortraitWindow(
                                text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags),
                            textWindow: new OpenTextWindow(selectedText, text.Mode, text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker, text.SpeakerFlags)); break;
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
                                ExplorationTextRunner.Close(logicalWindow));
                            break;
                        }
                        story = story.Copy(story.Cursor, textWindow: new ClosedTextWindow(),
                            portraitWindow: story.PortraitWindow is UnknownPortraitWindow ? new UnknownPortraitWindow() : story.PortraitWindow); break;
                    case NamePartyMember name:
                        var namedActor = CharacterNames.Binding(definition, name.Member);
                        if (!current.Exploration!.Party.Actors.Any(actor => actor.Actor == namedActor))
                            throw new BattleRuleException("name-member", "program.name", true);
                        story = current.Story.Copy(cursor, new NameWait(token, name.Member)); break;
                    case ChooseYesNo choice:
                        if (current.Story.TextSettings is not null) ExplorationTextRunner.ValidateContext(current);
                        story = current.Story.Copy(cursor, new ChoiceWait(token, choice.ResultFlag,
                            current.Story.TextSettings is null ? null : new ChoiceWork(), choice.NoTarget)); break;
                    case WaitProgramTicks ticks when ticks.Ticks > 0:
                        story = current.Story.Copy(cursor, new TickWait(token, ticks.Ticks)); break;
                    case WaitProgramTicks: break;
                    case PresentCue { Kind: PresentationCueKind.CameraWait } when current.Story.TextSettings is not null:
                        ExplorationTextRunner.ValidateContext(current);
                        story = current.Story.Copy(cursor, new ViewWait(token, !current.Story.LogicalView!.Scrolling));
                        break;
                    case PresentCue { Kind: PresentationCueKind.SoundWait } when story.TextSettings is not null:
                        story = current.Story.Copy(cursor, ExplorationMusicRunner.Begin(current, token));
                        break;
                    case PresentCue cue:
                        if (cue.Kind == PresentationCueKind.FlashWhite)
                        {
                            MapTransfer.ValidateFlash(cue);
                            if (current.Story.Display is not null) MapTransfer.ValidateDisplay(current.Story.Display);
                        }
                        if (cue.FullBlack is not null && current.Active is not ActiveExploration)
                            throw new BattleRuleException("full-fade-context", "program.presentation", true);
                        if (cue.FullBlack is not null || current.Active is ActiveExploration &&
                            current.Story.Display is not null && MapTransfer.IsPalette(cue))
                        {
                            MapTransfer.ValidateCue(cue);
                            current = MapTransfer.BeginFade(current, current.Story, cue.Kind,
                                FullFadePurpose.Script, cue.FullBlack?.Period, observations,
                                cue.Resource == "white" ? FullFadeColor.White : FullFadeColor.Black);
                            continue;
                        }
                        if (cue.Entity is { } reference) cue = cue with { Entity = Entity(current, reference).Entity };
                        bool boundNod = cue is { Kind: PresentationCueKind.Gesture, Resource: "nod", Entity: not null } &&
                            current.Story.TextSettings is not null;
                        if (boundNod) ExplorationTextRunner.ValidateContext(current);
                        GestureRestore? restore = null;
                        if (cue is { Kind: PresentationCueKind.Gesture, Resource: "nod" or "shake-head", Entity: { } nodding })
                            active = EditEntity(current, nodding, entity => entity with { Motion = entity.Motion with { AnimationCounter = 255 } });
                        if (cue is { Kind: PresentationCueKind.Gesture, Resource: "shiver", Entity: { } shivering })
                        {
                            var entity = Entity(current, shivering);
                            var world = current.Exploration!;
                            restore = new(entity.Motion.AnimationCounter, world.SpriteSize);
                            active = new ActiveExploration(world.WithEntities(world.AllEntities.Select(row => row.Slot == entity.Slot
                                ? row with { Motion = row.Motion with { AnimationCounter = 255 } } : row), spriteSize: 21));
                        }
                        var music = current.Story.Music;
                        if (current.Story.TextSettings is not null)
                        {
                            if (cue.Kind == PresentationCueKind.Sound)
                                music = ExplorationMusicRunner.Request(definition.Exploration!, current, cue.Resource, token);
                            else if (cue.Kind == PresentationCueKind.PreviousMusic)
                                music = ExplorationMusicRunner.Previous(definition.Exploration!, current, token);
                            else if (cue.Kind is (PresentationCueKind.SoundFade or PresentationCueKind.SoundCut) && music is not null)
                                music = music with { EndStep = null, ActualDone = false, PreviousEligible = false };
                        }
                        story = current.Story.Copy(cursor, boundNod ? new NodWait(token, cue.Entity!.Value) : new PresentationWait(token, cue, restore), music: music); break;
                    case RemoveEntityShadow remove:
                        var posed = Entity(current, remove.Entity);
                        long update = checked(posed.SpriteRequest + 1);
                        active = new ActiveExploration(current.Exploration!.WithEntity(posed with {
                            SpriteRequest = update, WaitingForSprite = true, ShadowRemovedAt = update }));
                        story = current.Story.Copy(cursor, new EntitySpriteWait(token, posed.Slot, update));
                        break;
                    case SetEntitySprite sprite:
                        var changedSprite = Entity(current, sprite.Entity);
                        int spriteId = sprite.Ally ? SceneEntities.ResolveAllySprite(current.Exploration!.Population
                            ?? throw new BattleRuleException("ally-sprite-population", "program.sprite", true), current.Story.Flags, sprite.Sprite, sprite.Sprite) : sprite.Sprite;
                        changedSprite = changedSprite with { Sprite = spriteId,
                            SpriteRequest = checked(changedSprite.SpriteRequest + 1), WaitingForSprite = true };
                        active = new ActiveExploration(current.Exploration!.WithEntity(changedSprite));
                        story = current.Story.Copy(cursor, new EntitySpriteWait(token, changedSprite.Slot, changedSprite.SpriteRequest));
                        break;
                    case FaceOtherEntity turn:
                        var subject = Entity(current, turn.Entity);
                        var toward = Entity(current, turn.Target).Motion;
                        int dx = toward.XDestination - subject.Motion.XDestination, dy = toward.YDestination - subject.Motion.YDestination;
                        byte direction = (byte)(Math.Abs(dy) >= Math.Abs(dx) ? dy < 0 ? 1 : 3 : dx < 0 ? 2 : 0);
                        subject = subject with { Motion = subject.Motion with { Facing = direction },
                            SpriteRequest = checked(subject.SpriteRequest + 1), WaitingForSprite = true };
                        active = new ActiveExploration(current.Exploration!.WithEntity(subject));
                        story = current.Story.Copy(cursor, new EntitySpriteWait(token, subject.Slot, subject.SpriteRequest)); break;
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
                    case SetMapEntityAnimation animation:
                        active = EditEntity(current, animation.Entity, entity => entity with
                            { Motion = entity.Motion with { AnimationCounter = animation.Counter } }); break;
                    case ReloadSceneMap reload:
                        var reloaded = MapTransfer.ReloadScene(current, reload);
                        active = new ActiveExploration(reloaded.World);
                        story = reloaded.Story.Copy(Next(cursor));
                        break;
                    case SetMapQuake quake:
                        active = new ActiveExploration(current.Exploration!.WithQuake(quake.Amount)); break;
                    case AddMapEntity add:
                        active = new ActiveExploration(SceneEntities.Add(current.Exploration!, add, token.Value, story.Flags));
                        break;
                    case RestoreMemberResource restoreResource:
                        var party = current.Exploration!.Party;
                        var member = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                            .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember == restoreResource.Member)
                            ?? throw new BattleRuleException("party-member-definition", "program.member", true);
                        if (!party.Actors.Any(row => row.Actor == member.Actor))
                            throw new BattleRuleException("missing-party-member", "program.member", true);
                        active = new ActiveExploration(current.Exploration.WithParty(new(party.Encounter,
                            party.Actors.Select(row => row.Actor != member.Actor ? row : restoreResource.Hp
                                ? row with { Hp = row.Progress?.MaxHp ?? member.Definition.MaxHp }
                                : row with { Mp = row.Progress?.MaxMp ?? member.Definition.MaxMp }),
                            party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies, party.ItemCustody, party.StoryFlags, party.Visuals, party.RetainedSourceSlots, party.DeathCredit)));
                        break;
                    case MoveEntityNextToPlayer beside:
                        var playerMotion = current.Exploration!.PlayerEntity.Motion;
                        int destinationX = playerMotion.XDestination + (beside.Direction == 0 ? 384 : beside.Direction == 2 ? -384 : 0);
                        int destinationY = playerMotion.YDestination + (beside.Direction == 3 ? 384 : beside.Direction == 1 ? -384 : 0);
                        active = EditEntity(current, beside.Entity, entity => entity with
                        {
                            Motion = entity.Motion with
                            {
                                XDestination = checked((short)destinationX), YDestination = checked((short)destinationY),
                                XTravel = checked((ushort)Math.Abs(destinationX - entity.Motion.X)),
                                YTravel = checked((ushort)Math.Abs(destinationY - entity.Motion.Y)),
                                XVelocity = (short)(destinationX < entity.Motion.X ? -48 : 48),
                                YVelocity = (short)(destinationY < entity.Motion.Y ? -48 : 48),
                            },
                        });
                        break;
                    case WaitForEntityMotion waitMotion:
                        if (Entity(current, waitMotion.Entity).Motion.IsMoving)
                            story = current.Story.Copy(cursor, new EntityWait(token, waitMotion.Entity, Completion: EntityWaitCompletion.MotionStopped));
                        break;
                    case SetEntityVisibility visible:
                        active = EditEntity(current, visible.Entity, entity => entity with { Visible = visible.Visible }); break;
                    case HideMapEntity hide:
                        active = new ActiveExploration(current.Exploration!.Hide(hide.Entity, hide.RemoveAliases)); break;
                    case ApplyMapLayoutEvent layoutEvent:
                        var field = current.Exploration!;
                        active = new ActiveExploration(layoutEvent.Roof ? MapEventDispatcher.ScriptedRoof(field, layoutEvent.Position)
                            : MapEventDispatcher.OpenDoor(field, layoutEvent.Position));
                        break;
                    case FlashEntityPosition flash:
                        var flashing = Entity(current, flash.Entity);
                        active = new ActiveExploration(current.Exploration!.WithEntity(flashing with { Visible = false }));
                        story = current.Story.Copy(cursor, new PositionFlashWait(token, flash.Entity, flashing.Visible));
                        break;
                    case CopyMapLayout copy:
                        active = new ActiveExploration(current.Exploration!.WithLayout(current.Exploration.Layout.ApplyBlockCopy(copy.Copy)));
                        break;
                    case SetEntityDestination destination:
                        active = EditEntity(current, destination.Entity, entity =>
                        {
                            short x = checked((short)(destination.Position.X * 384)), y = checked((short)(destination.Position.Y * 384));
                            return entity with { Motion = entity.Motion with {
                                XDestination = x, YDestination = y,
                                XTravel = (ushort)Math.Abs(x - entity.Motion.X), YTravel = (ushort)Math.Abs(y - entity.Motion.Y),
                                XVelocity = (short)(x < entity.Motion.X ? -32 : 32), YVelocity = (short)(y < entity.Motion.Y ? -32 : 32) } };
                        });
                        if (destination.Wait) story = current.Story.Copy(cursor, new EntityWait(token, destination.Entity, Completion: EntityWaitCompletion.MotionStopped));
                        break;
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
                            Completion: motion.Completion ?? (motion.Installation == EntityScriptInstallation.Preserve
                                ? EntityWaitCompletion.NotBusy : EntityWaitCompletion.ScriptIdle)));
                        break;
                    case WaitForEntity wait:
                        if (wait.ScriptIdle ? !Entity(current, wait.Entity).IsScriptIdle : Entity(current, wait.Entity).Busy)
                            story = current.Story.Copy(cursor, new EntityWait(token, wait.Entity, Completion: wait.ScriptIdle ? EntityWaitCompletion.ScriptIdle : EntityWaitCompletion.NotBusy));
                        break;
                    case TransferToMap transfer:
                        story = story.Copy(story.Cursor, clearWarp: true);
                        current = MapTransfer.Apply(definition, current, transfer.Map, transfer.Position, transfer.Facing,
                            transfer.Mode, story, observations, rules);
                        continue;
                    case QueueMapWarp queued:
                        story = story.Copy(story.Cursor, scriptWarp: new(queued.Map, queued.Position, queued.Facing, MapLoadMode.Rebuild));
                        break;
                    default: throw new BattleRuleException("program-instruction", "program", true);
                }
                current = Commit(current, active, story, observations, story.Wait is NodWait ? "nod-started" : "program-instruction", instruction.GetType().Name, cursor);
                if (instruction is CallProgram or ReturnProgram ||
                    instruction is EndProgram && story.Wait is not ViewWait { ScriptReturn: true })
                    reads.Add(ControlRead(current, cursor, instruction.GetType().Name, callersBefore));
            }
            catch (BattleStoryOperations.PrefixFailure error) { return Finish(Failure(error.Snapshot, observations, error.Error)); }
            catch (BattlePolicyFault error) { return Finish(Failure(current, observations, error)); }
            catch (BattleRuleException error) { return Finish(Failure(current, observations, error)); }
        }
        return Finish(Result(Stop(current, SessionStopReason.SimulationWait), observations));
    }

    internal static ProgramControlRead ControlRead(SessionSnapshot current, ProgramLocation source, string operation,
        IReadOnlyList<ProgramLocation> callersBefore) =>
        new(current.SessionId, current.ObservationSequence, current.Revision, source, operation,
            current.Story.Cursor, Array.AsReadOnly(callersBefore.ToArray()), Array.AsReadOnly(current.Story.Callers.ToArray()));

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

    private static ActiveSessionState JoinResources(ScenarioDefinition definition, SessionSnapshot current, JoinPartyMember join)
    {
        int member = join.Member;
        if (definition.EncounterStarts.Count == 0) return current.Active;
        var world = current.Exploration!;
        var actors = definition.Encounters.Values.SelectMany(battle => battle.Deployments)
            .Where(actor => actor.Faction == BattleFaction.Ally && actor.Initialization?.AllyPartyMember == member)
            .Select(actor => actor.Actor).Distinct().ToArray();
        if (actors.Length != 1) throw new BattleRuleException("join-member-binding", "program.member", true);
        var party = world.Party;
        var resources = party.Actors.ToList();
        int index = resources.FindIndex(actor => actor.Actor == actors[0]);
        if (index >= 0 && (join.AiControlled is null || resources[index].ActivationWord is { } activation &&
            ((activation & 4) != 0) == join.AiControlled))
            return current.Active;
        if (index < 0)
        {
            var initial = definition.EncounterStarts.Values.SelectMany(start => start.Actors)
                .FirstOrDefault(actor => actor.Actor == actors[0])
                ?? throw new BattleRuleException("join-member-start", "program.member", true);
            index = resources.Count;
            resources.Add(initial);
        }
        if (join.AiControlled is { } enable)
        {
            var actor = resources[index];
            ushort word = actor.ActivationWord ?? throw new BattleRuleException("missing-activation-word", "party.activation", true);
            resources[index] = actor with { ActivationWord = (ushort)(enable ? word | 4 : word & ~4) };
        }
        return new ActiveExploration(world.WithParty(party.WithResources(resources, party.Gold)));
    }

    internal static SessionSnapshot Commit(SessionSnapshot current, ActiveSessionState active, StoryState story,
        List<SessionObservation> observations, string kind, string? detail = null, ProgramLocation? program = null)
    {
        story = ExplorationMusicRunner.InvalidateContext(active, story);
        long revision = checked(current.Revision + 1), sequence = checked(current.ObservationSequence + 1);
        observations.Add(new(sequence, revision, kind, Detail: detail, Program: program));
        var reason = story.Wait switch
        {
            FullFadeWait { LogicalDone: true } or FieldTextWait { LogicalDone: true } or NodWait { LogicalDone: true } or MusicWait { LogicalDone: true } => SessionStopReason.PresentationWait,
            ChoiceWait { Work.Automatic: true } => SessionStopReason.SimulationWait,
            DialogueWait or W1TextWait or ChoiceWait or NameWait or ChurchWait or ShopWait or PresentationWait or EntitySpriteWait or EntitySetSpriteWait => SessionStopReason.PresentationWait,
            EntityWait or PhysicalEntityWait or TickWait => SessionStopReason.SimulationWait,
            _ => SessionStopReason.SimulationWait,
        };
        return new(current.SessionId, revision, sequence, active, story, reason);
    }
    internal static SessionSnapshot Stop(SessionSnapshot current, SessionStopReason reason) =>
        new(current.SessionId, current.Revision, current.ObservationSequence, current.Active, current.Story, reason);
    internal static SessionResult Result(SessionSnapshot current, List<SessionObservation> observations) =>
        new(current, observations.AsReadOnly(), current.StopReason);
    internal static SessionResult Failure(SessionSnapshot current, List<SessionObservation> observations, BattlePolicyFault error) =>
        new(current, observations.AsReadOnly(), SessionStopReason.Faulted,
            BattleChoices.Failure(error));
    internal static SessionResult Failure(SessionSnapshot current, List<SessionObservation> observations, BattleRuleException error)
    {
        var reason = error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Faulted;
        return new(Stop(current, reason), observations.AsReadOnly(), reason,
            new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
    }
}
