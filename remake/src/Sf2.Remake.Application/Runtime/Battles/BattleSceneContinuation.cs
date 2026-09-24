using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattleScenePhase { Initialize, ActionMessage, SpellCost, ActionAnimation, TargetExit, TargetEnter, Reaction, ResultMessage, MakeIdle, SpellStop, DeathMessage, ActorExit, ActorEnter, Reward, RewardMessage, GrowthMessage, GoldMessage, End }
public sealed record BattleSceneOffset(short X, short Y);
public enum BattleGrowthNoticeKind { Level, MaxHp, MaxMp, Attack, Defense, Agility, Spell }
public sealed record BattleGrowthNotice(BattleGrowthNoticeKind Kind, int Amount, SpellRef? Spell = null);

internal enum HealingWorkKind { Wait, TimedInput, Enable, Stop, Drain, Cleanup }
internal sealed record HealingWork(HealingWorkKind Kind, int Count, string Caller, int Frame = -1);

// One continuation local to this scene. Counters are source work, not host frames.
// Presentation delivery can finish before or after them without adding work.
public sealed record HealingSceneCursor
{
    internal BattleSceneAnimation Cast { get; init; } = null!;
    internal int CasterIdleTicks { get; init; }
    internal int TargetIdleTicks { get; init; }
    internal IReadOnlyList<HealingWork> Work { get; init; } = [];
    internal int Index { get; init; }
    public int Remaining { get; internal init; }
    public bool Delivered { get; internal init; }
    public HealingFairyState? Fairy { get; internal init; }
    public bool CastRequested { get; internal init; }
    public string ActionText { get; internal init; } = "";
    public string RecoveryText { get; internal init; } = "";
    public bool LogicalComplete => Index >= Work.Count;
    public bool AtTimedInput => !LogicalComplete && Work[Index].Kind == HealingWorkKind.TimedInput;
    public string? Caller => LogicalComplete ? null : Work[Index].Caller;
    public int CastFrame => LogicalComplete ? -1 : Work[Index].Frame;
    public int Opportunity => LogicalComplete ? 0 : Work[Index].Count - Remaining;

    internal static HealingSceneCursor Create(BattleActionResolution action, BattleSceneDefinition? content)
    {
        var actor = action.Prepared.GetActor(action.Actor);
        var target = action.Prepared.GetActor(action.Reactions[0].Target);
        // Authored packages have an intentionally small authored gesture; private
        // scenes require their admitted original sequence rather than this fallback.
        var cast = new BattleSceneAnimation(0, 2, 255, false, null,
            [new(0, 1, 0, 0, null), new(0, 1, 0, 0, null)]);
        int casterIdle = 1, targetIdle = 1;
        string ActorName(BattleActorState a) => content is null ? a.Actor.Value : content.MemberNames[a.ProcessingOrder];
        string actionText = "{NAME} cast\n{SPELL} level {#}!", recoveryText = "{NAME} recovered\n{#} hit points.";
        if (content is not null)
        {
            if (content.Healing is null || !content.Allies.TryGetValue(actor.Definition.ClassRule, out var visual) ||
                !visual.Sequences.TryGetValue("cast", out cast!) || visual.IdleTicks <= 0 ||
                !content.Allies.TryGetValue(target.Definition.ClassRule, out var targetVisual) || targetVisual.IdleTicks <= 0)
                throw new BattleRuleException("healing-scene-content", "battleScenes.healing", true);
            casterIdle = visual.IdleTicks; targetIdle = targetVisual.IdleTicks;
            actionText = content.Texts[274]; recoveryText = content.Texts[298];
        }
        return new() { Cast = cast, CasterIdleTicks = casterIdle, TargetIdleTicks = targetIdle,
            ActionText = actionText.Replace("{NAME}", ActorName(actor)).Replace("{SPELL}", action.Spell!.Spell.Value.ToUpperInvariant())
                .Replace("{#}", action.Spell.Spell.Level.ToString()).Replace("{N}", "\n"),
            RecoveryText = recoveryText.Replace("{NAME}", ActorName(target)).Replace("{#}", action.Reactions[0].Amount.ToString()).Replace("{N}", "\n") };
    }

    internal HealingSceneCursor Enter(BattleScenePhase phase)
    {
        List<HealingWork> work = [];
        void Wait(int count, string caller, int frame = -1) { if (count > 0) work.Add(new(HealingWorkKind.Wait, count, caller, frame)); }
        switch (phase)
        {
            case BattleScenePhase.ActionMessage:
            case BattleScenePhase.ResultMessage:
                // Battle CreateDialogueWindow: clear-layout VInt, DMA drain, move VInt.
                Wait(1, "ClearDialogueWindowLayout"); Wait(1, "dialogue-layout-dma"); Wait(1, "dialogue-window-move");
                string text = phase == BattleScenePhase.ActionMessage ? ActionText : RecoveryText;
                foreach (char glyph in text.Where(c => c != '\n'))
                {
                    // The selected logical message setting is source speed2. Product
                    // instant/reveal speed never changes these per-glyph operations.
                    if (glyph is '\u007c' or '\u007d') continue;
                    Wait(1, "HandleBlinkingDialogueCursor"); Wait(1, "HandleDialogueTypewriting");
                }
                work.Add(new(HealingWorkKind.TimedInput, (1 << (8 - 2)) + 1, "bsc10:input-before-vint"));
                break;
            case BattleScenePhase.SpellCost:
            case BattleScenePhase.Reaction:
                Wait(1, "OpenAllyMiniStatus:move"); Wait(1, "OpenAllyMiniStatus:movement-end");
                break;
            case BattleScenePhase.ActionAnimation:
                void Setup(int frame)
                {
                    for (int flash = 0; flash < 4; flash++) { Wait(4, "cast-flash-on", frame); Wait(3, "cast-flash-off", frame); }
                    Wait(1, "LoadSpellTileset:dma", frame);
                    work.Add(new(HealingWorkKind.Enable, 0, "HealingFairy:enable", frame));
                }
                if (Cast.Trigger == 0) Setup(1);
                for (int frame = 1; frame < Cast.Frames.Count; frame++)
                {
                    if (Cast.Frames[frame].Frame != 15)
                    {
                        // The admitted ally-only HEAL path does not schedule an
                        // enemy-layout graphics request. The source's conditional
                        // WaitForBattlesceneGraphicsUpdate is not an unconditional tick.
                        Wait(1, "LoadAllyBattlespriteFrame:dma", frame);
                    }
                    if (frame + 1 == Cast.Trigger) Setup(frame);
                    Wait(Cast.Frames[frame].Ticks, "bsc01:frame-sleep", frame);
                }
                break;
            case BattleScenePhase.TargetExit: Wait(30, "SwitchTargets:wait"); Wait((272 - 136 + 15) / 16, "SwitchAlly:exit"); break;
            case BattleScenePhase.TargetEnter: Wait((136 + 376 + 15) / 16, "SwitchAlly:enter"); break;
            case BattleScenePhase.ActorExit: Wait(30, "SwitchActor:wait"); Wait((136 + 112 + 15) / 16, "SwitchActor:exit"); break;
            case BattleScenePhase.ActorEnter: Wait((136 - 8 + 15) / 16, "SwitchActor:enter"); break;
            case BattleScenePhase.MakeIdle:
                // bsc05 calls sub_1938C on both sides of Sleep. Each actual idle
                // frame load ends in WaitForDmaQueueProcessing; weapon DMA merely
                // enables the queue and sub_1942 itself does not wait.
                Wait(1, "bsc05:idle-frame-before-dma");
                Wait(TargetIdleTicks, "bsc05:ally-idle-sleep");
                Wait(1, "bsc05:idle-frame-after-dma");
                break;
            case BattleScenePhase.SpellStop:
                work.Add(new(HealingWorkKind.Stop, 0, "bsc0D:control2"));
                work.Add(new(HealingWorkKind.Drain, 0, "bsc0D:toggle-drain"));
                work.Add(new(HealingWorkKind.Cleanup, 1, "ReinitializeSceneAfterSpell:wait"));
                Wait(1, "bsc0D:restore-wait");
                break;
        }
        return this with { Work = work.AsReadOnly(), Index = 0, Remaining = work.Count == 0 ? 0 : work[0].Count, Delivered = false };
    }

    internal HealingSceneCursor Advance(uint initialSeed, ActorRef actor, int level, List<BattleEffect> effects,
        out uint seed, bool acknowledge = false)
    {
        seed = initialSeed;
        var next = this;
        if (acknowledge)
        {
            if (AtTimedInput) next = Next(); // bsc10 tests input before its WaitForVInt.
            return next with { Delivered = true };
        }
        if (next.LogicalComplete) return next;
        var work = next.Work[next.Index];
        if (work.Caller == "cast-flash-on") next = next with { CastRequested = true };
        if (work.Kind == HealingWorkKind.Cleanup)
            next = next with { Fairy = HealingFairy.FinishCleanup(next.Fairy!) };
        else if (next.Fairy is { } fairy)
        {
            var update = HealingFairy.Advance(fairy, seed, actor);
            next = next with { Fairy = update.State }; seed = update.Seed; effects.AddRange(update.Effects);
        }
        if (work.Kind == HealingWorkKind.Drain)
        {
            if (next.Fairy!.ActiveCount == 0) next = next.Next();
        }
        else next = next.Remaining <= 1 ? next.Next() : next with { Remaining = next.Remaining - 1 };
        return next.Normalize(seed, actor, level, effects, out seed);
    }

    internal HealingSceneCursor Normalize(uint initialSeed, ActorRef actor, int level, List<BattleEffect> effects, out uint seed)
    {
        seed = initialSeed; var next = this;
        while (!next.LogicalComplete)
        {
            var work = next.Work[next.Index];
            if (work.Caller == "cast-flash-on") next = next with { CastRequested = true };
            if (work.Kind == HealingWorkKind.Enable)
            {
                var setup = HealingFairy.Begin(level, seed, actor); seed = setup.Seed; effects.AddRange(setup.Effects);
                next = next with { Fairy = setup.State };
            }
            else if (work.Kind == HealingWorkKind.Stop) next = next with { Fairy = HealingFairy.RequestStop(next.Fairy!) };
            else break;
            next = next.Next();
        }
        return next;
    }
    private HealingSceneCursor Next() => this with { Index = Index + 1,
        Remaining = Index + 1 < Work.Count ? Work[Index + 1].Count : 0 };
}

public sealed class BattleSceneState
{
    internal BattleSceneState(BattleActionResolution action, BattleScenePhase phase, int reactionIndex,
        WaitToken token, IReadOnlyList<BattleSceneOffset>? motion = null, IReadOnlyList<BattleGrowthNotice>? growth = null, int noticeIndex = 0,
        HealingSceneCursor? healing = null)
    { Action = action; Phase = phase; ReactionIndex = reactionIndex; Token = token; Motion = motion ?? []; Growth = growth ?? []; NoticeIndex = noticeIndex; Healing = healing; }
    internal BattleActionResolution Action { get; }
    internal int ReactionIndex { get; }
    internal BattleReaction Reaction => Action.Reactions[ReactionIndex];
    public BattleScenePhase Phase { get; }
    public WaitToken Token { get; }
    public ActorRef Actor => Reaction.Actor;
    public ActorRef Target => Reaction.Target;
    public string ActionKind => Reaction.Action;
    public string ReactionKind => Reaction.Kind.ToString();
    public HealingItemDefinition? Item => Action.Item;
    public HealingSpellDefinition? Spell => Action.Spell;
    public HealingSceneCursor? Healing { get; }
    public bool SwitchesAlly => (Item is not null || Spell is not null) && Actor != Target;
    // InitializeActors / SwitchTargets / Script_End: only one ally occupies this side.
    public ActorRef? DisplayedAlly => Item is not null || Spell is not null
        ? Phase is BattleScenePhase.TargetEnter or BattleScenePhase.Reaction or BattleScenePhase.ResultMessage or BattleScenePhase.MakeIdle or BattleScenePhase.SpellStop or BattleScenePhase.ActorExit ? Target : Actor
        : Action.Prepared.GetActor(Actor).IsAlly ? Actor : Target;
    public ActorRef? DisplayedEnemy => Item is not null || Spell is not null ? null
        : Action.Prepared.GetActor(Actor).IsAlly ? Target : Actor;
    public bool Critical => Reaction.Critical;
    public int HpChange => Reaction.HpAfter - Reaction.HpBefore;
    public int MpChange => Reaction.MpAfter - Reaction.MpBefore;
    public int Amount => Reaction.Amount;
    public bool TargetDefeated => Reaction.HpAfter == 0;
    public ActorRef? RewardActor => Action.Reward?.Actor;
    public int RewardAmount => Action.Reward?.Amount ?? 0;
    internal IReadOnlyList<BattleGrowthNotice> Growth { get; }
    internal int NoticeIndex { get; }
    public BattleGrowthNotice? GrowthNotice => Phase == BattleScenePhase.GrowthMessage ? Growth[NoticeIndex] : null;
    public int GoldAmount => (int)(Action.ConstructionEffects.Where(effect => effect.Kind == "gold")
        .Sum(effect => effect.After!.Value - effect.Before!.Value));
    public bool IsHealingMessage => Healing is not null && Phase is BattleScenePhase.ActionMessage or BattleScenePhase.ResultMessage;
    public bool RequiresAcknowledgement => Phase is BattleScenePhase.ActionMessage or BattleScenePhase.ResultMessage or BattleScenePhase.DeathMessage
        or BattleScenePhase.RewardMessage or BattleScenePhase.GrowthMessage or BattleScenePhase.GoldMessage;
    public PresentationCueKind CompletionKind => Phase switch
    {
        BattleScenePhase.Initialize => PresentationCueKind.BattleLoad,
        BattleScenePhase.End => PresentationCueKind.SoundFade,
        _ => PresentationCueKind.Gesture,
    };
    public IReadOnlyList<BattleSceneOffset> Motion { get; }
}

internal static class BattleSceneContinuation
{
    internal static SessionResult Begin(SessionSnapshot current, BattleActionResolution action,
        List<SessionObservation> observations, IReadOnlyList<BattleEffect>? construction = null,
        BattleSceneDefinition? sceneContent = null)
    {
        long sequence = current.ObservationSequence, revision = checked(current.Revision + 1);
        var origin = current.Battle.GetActor(action.Actor).Position;
        if (origin != action.Destination)
            observations.Add(new(++sequence, revision, "movement", action.Actor, From: origin, To: action.Destination));
        // Computation facts can be observed now; persistent HP/reward facts are emitted only
        // when their commands execute. The typed action, not these observations, owns replay.
        foreach (var effect in construction ?? action.ConstructionEffects)
            if (effect.Kind is not ("hp" or "mp" or "dodge" or "critical" or "physical-first" or "physical-second" or "physical-counter"))
                observations.Add(Observe(effect, ++sequence, revision));
        observations.Add(new(++sequence, revision, "scene-prepared", action.Actor));
        var scene = new BattleSceneState(action, BattleScenePhase.Initialize, 0, new(sequence),
            healing: action.Spell is null ? null : HealingSceneCursor.Create(action, sceneContent));
        return Result(current, action.Prepared, scene, revision, sequence, observations);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        var scene = current.BattleScene!;
        if (scene.Healing is not { } healing) return Continue(current, command);
        bool advancing = command is AdvanceSimulation { Ticks: 1 } advance && advance.Wait == scene.Token && !healing.LogicalComplete;
        bool acknowledge = command is Acknowledge ack && ack.Wait == scene.Token && scene.RequiresAcknowledgement &&
            (healing.AtTimedInput || healing.LogicalComplete);
        bool delivery = command is CompletePresentation completion && completion.Wait == scene.Token &&
            (!scene.RequiresAcknowledgement || scene.IsHealingMessage) &&
            completion.Kind == scene.CompletionKind && !healing.Delivered;
        if (!advancing && !acknowledge && !delivery)
            return BattleCommandDispatcher.Reject(current, "battle-scene-wait", "command");
        var battle = current.Battle;
        List<BattleEffect> effects = [];
        uint seed = battle.MainSeed;
        if (advancing || acknowledge)
            healing = healing.Advance(seed, scene.Actor, scene.Spell!.Spell.Level, effects, out seed, acknowledge);
        if (delivery) healing = healing with { Delivered = true };
        battle = battle.With(mainSeed: seed);
        long sequence = current.ObservationSequence, revision = checked(current.Revision + 1);
        List<SessionObservation> observations = effects.Select(effect => Observe(effect, ++sequence, revision)).ToList();
        observations.Add(new(++sequence, revision, advancing ? "scene-logical-step" : "scene-delivery", scene.Actor,
            Detail: scene.Healing.Caller ?? scene.Phase.ToString()));
        var updated = new BattleSceneState(scene.Action, scene.Phase, scene.ReactionIndex, scene.Token,
            scene.Motion, scene.Growth, scene.NoticeIndex, healing);
        if (!healing.LogicalComplete || !healing.Delivered)
            return Result(current, battle, updated, revision, sequence, observations);
        var ready = new SessionSnapshot(current.SessionId, current.Revision, sequence,
            new ActiveBattle(battle, null, updated), current.Story, current.StopReason);
        var result = Continue(ready, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token)
            : new CompletePresentation(scene.Token, scene.CompletionKind));
        return result with { Observations = observations.Concat(result.Observations).ToArray() };
    }

    private static SessionResult Continue(SessionSnapshot current, SessionCommand command)
    {
        var scene = current.BattleScene!;
        bool accepted = command switch
        {
            Acknowledge ack => scene.RequiresAcknowledgement && ack.Wait == scene.Token,
            CompletePresentation completion => !scene.RequiresAcknowledgement && completion.Wait == scene.Token && completion.Kind == scene.CompletionKind,
            _ => false,
        };
        if (!accepted) return BattleCommandDispatcher.Reject(current, "battle-scene-wait", "command");
        var battle = current.Battle;
        long sequence = current.ObservationSequence, revision = checked(current.Revision + 1);
        List<SessionObservation> observations = [new(++sequence, revision, "scene-step-completed", scene.Actor, Detail: scene.Phase.ToString())];
        int index = scene.ReactionIndex;
        BattleScenePhase next;
        IReadOnlyList<BattleSceneOffset> motion = [];
        IReadOnlyList<BattleGrowthNotice> growthNotices = scene.Growth;
        int noticeIndex = scene.NoticeIndex;
        switch (scene.Phase)
        {
            case BattleScenePhase.Initialize: next = BattleScenePhase.ActionMessage; break;
            case BattleScenePhase.ActionMessage:
                observations.Add(new(++sequence, revision, scene.ActionKind, scene.Actor, Target: scene.Target));
                if (scene.Spell is not null)
                {
                    var caster = battle.GetActor(scene.Actor);
                    battle = scene.Action.ApplySpellCost(battle);
                    observations.Add(new(++sequence, revision, "mp", scene.Actor, caster.Mp, battle.GetActor(scene.Actor).Mp));
                    next = BattleScenePhase.SpellCost; break;
                }
                next = BattleScenePhase.ActionAnimation; break;
            case BattleScenePhase.SpellCost: next = BattleScenePhase.ActionAnimation; break;
            case BattleScenePhase.ActionAnimation:
                if (scene.SwitchesAlly) { next = BattleScenePhase.TargetExit; break; }
                goto case BattleScenePhase.TargetEnter;
            case BattleScenePhase.TargetExit: next = BattleScenePhase.TargetEnter; break;
            case BattleScenePhase.TargetEnter:
                battle = scene.Action.ApplyReaction(battle, scene.Reaction);
                if (scene.Critical) observations.Add(new(++sequence, revision, "critical", scene.Actor, Target: scene.Target));
                if (scene.Reaction.Kind == BattleReactionKind.Dodge)
                    observations.Add(new(++sequence, revision, "dodge", scene.Target));
                else
                {
                    if (scene.HpChange != 0) observations.Add(new(++sequence, revision, "hp", scene.Target, scene.Reaction.HpBefore, scene.Reaction.HpAfter));
                    if (scene.MpChange != 0) observations.Add(new(++sequence, revision, "mp", scene.Target, scene.Reaction.MpBefore, scene.Reaction.MpAfter));
                }
                var playback = BattleSceneRules.Reaction(battle, scene.Reaction);
                battle = playback.Battle;
                foreach (var effect in playback.Effects) observations.Add(Observe(effect, ++sequence, revision));
                motion = Array.AsReadOnly(playback.Motion.Select(offset => new BattleSceneOffset(offset.X, offset.Y)).ToArray());
                next = BattleScenePhase.Reaction;
                break;
            case BattleScenePhase.Reaction: next = BattleScenePhase.ResultMessage; break;
            case BattleScenePhase.ResultMessage:
                if (scene.Spell is not null) { next = BattleScenePhase.MakeIdle; break; }
                if (scene.TargetDefeated) { next = BattleScenePhase.DeathMessage; break; }
                if (scene.SwitchesAlly) { next = BattleScenePhase.ActorExit; break; }
                goto case BattleScenePhase.DeathMessage;
            case BattleScenePhase.MakeIdle: next = BattleScenePhase.SpellStop; break;
            case BattleScenePhase.SpellStop:
                if (scene.SwitchesAlly) { next = BattleScenePhase.ActorExit; break; }
                goto case BattleScenePhase.DeathMessage;
            case BattleScenePhase.ActorExit: next = BattleScenePhase.ActorEnter; break;
            case BattleScenePhase.ActorEnter:
                goto case BattleScenePhase.DeathMessage;
            case BattleScenePhase.DeathMessage:
                if (index + 1 < scene.Action.Reactions.Count) { index++; next = BattleScenePhase.ActionMessage; }
                else if (scene.Action.Reward is not null)
                {
                    var reward = scene.Action.CreditReward(battle);
                    battle = reward.Battle;
                    foreach (var effect in reward.Effects) observations.Add(Observe(effect, ++sequence, revision));
                    next = BattleScenePhase.Reward;
                }
                else next = scene.GoldAmount > 0 ? BattleScenePhase.GoldMessage : BattleScenePhase.End;
                break;
            case BattleScenePhase.Reward: next = BattleScenePhase.RewardMessage; break;
            case BattleScenePhase.RewardMessage:
                var before = battle.GetActor(scene.RewardActor!.Value);
                var grown = scene.Action.ApplyGrowth(battle);
                battle = grown.Battle;
                foreach (var effect in grown.Effects) observations.Add(Observe(effect, ++sequence, revision));
                growthNotices = GrowthNotices(before, battle.GetActor(before.Actor));
                next = growthNotices.Count > 0 ? BattleScenePhase.GrowthMessage : scene.GoldAmount > 0 ? BattleScenePhase.GoldMessage : BattleScenePhase.End;
                break;
            case BattleScenePhase.GrowthMessage:
                if (noticeIndex + 1 < growthNotices.Count) { noticeIndex++; next = BattleScenePhase.GrowthMessage; }
                else next = scene.GoldAmount > 0 ? BattleScenePhase.GoldMessage : BattleScenePhase.End;
                break;
            case BattleScenePhase.GoldMessage: next = BattleScenePhase.End; break;
            case BattleScenePhase.End:
                battle = scene.Action.Complete(battle);
                foreach (var effect in scene.Action.CompletionEffects) observations.Add(Observe(effect, ++sequence, revision));
                observations.Add(new(++sequence, revision, "scene-ended", scene.Action.Actor));
                var ended = new SessionSnapshot(current.SessionId, revision, sequence,
                    new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
                // Turn consumption/outcome occur only after the actual scene consumer has
                // finished; lethal HP on an earlier reaction is not permission to leave.
                var committed = BattleActionCommitter.Publish(ended, battle, scene.Action.Actor,
                    scene.Action.Destination, [], observations).WithStory(current.Story);
                return BattleAdvancer.Advance(committed, observations);
            default: throw new InvalidOperationException("Unknown battle scene phase.");
        }
        observations.Add(new(++sequence, revision, "scene-step-started", scene.Action.Reactions[index].Actor, Detail: next.ToString()));
        var healing = scene.Healing?.Enter(next);
        if (healing is not null)
        {
            List<BattleEffect> effects = [];
            healing = healing.Normalize(battle.MainSeed, scene.Actor, scene.Spell!.Spell.Level, effects, out uint seed);
            battle = battle.With(mainSeed: seed);
            foreach (var effect in effects) observations.Add(Observe(effect, ++sequence, revision));
        }
        return Result(current, battle, new(scene.Action, next, index, new(sequence), motion, growthNotices, noticeIndex, healing), revision, sequence, observations);
    }

    private static IReadOnlyList<BattleGrowthNotice> GrowthNotices(BattleActorState before, BattleActorState after)
    {
        if (before.Level == after.Level) return [];
        List<BattleGrowthNotice> notices = [new(BattleGrowthNoticeKind.Level, after.Level)];
        foreach (var (kind, gain) in new[] { (BattleGrowthNoticeKind.MaxHp, after.MaxHp-before.MaxHp),
            (BattleGrowthNoticeKind.MaxMp, after.MaxMp-before.MaxMp), (BattleGrowthNoticeKind.Attack, after.BaseAttack-before.BaseAttack),
            (BattleGrowthNoticeKind.Defense, after.Defense-before.Defense), (BattleGrowthNoticeKind.Agility, after.Agility-before.Agility) })
            if (gain > 0) notices.Add(new(kind, gain));
        foreach (var spell in after.Spells.Except(before.Spells)) notices.Add(new(BattleGrowthNoticeKind.Spell, spell.Level, spell));
        return notices.AsReadOnly();
    }

    private static SessionObservation Observe(BattleEffect effect, long sequence, long revision) =>
        new(sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After,
            RandomRange: effect.RandomRange, RandomValue: effect.RandomValue, Target: effect.Target);

    private static SessionResult Result(SessionSnapshot current, EngineBattleState battle, BattleSceneState scene,
        long revision, long sequence, List<SessionObservation> observations) =>
        new(new(current.SessionId, revision, sequence, new ActiveBattle(battle, null, scene), current.Story,
            SessionStopReason.PresentationWait), observations.AsReadOnly(), SessionStopReason.PresentationWait);
}
