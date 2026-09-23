using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattleScenePhase { Initialize, ActionMessage, ActionAnimation, Reaction, ResultMessage, DeathMessage, Reward, RewardMessage, GrowthMessage, GoldMessage, End }
public sealed record BattleSceneOffset(short X, short Y);
public enum BattleGrowthNoticeKind { Level, MaxHp, MaxMp, Attack, Defense, Agility, Spell }
public sealed record BattleGrowthNotice(BattleGrowthNoticeKind Kind, int Amount, SpellRef? Spell = null);

public sealed class BattleSceneState
{
    internal BattleSceneState(BattleActionResolution action, BattleScenePhase phase, int reactionIndex,
        WaitToken token, IReadOnlyList<BattleSceneOffset>? motion = null, IReadOnlyList<BattleGrowthNotice>? growth = null, int noticeIndex = 0)
    { Action = action; Phase = phase; ReactionIndex = reactionIndex; Token = token; Motion = motion ?? []; Growth = growth ?? []; NoticeIndex = noticeIndex; }
    internal BattleActionResolution Action { get; }
    internal int ReactionIndex { get; }
    internal BattleReaction Reaction => Action.Reactions[ReactionIndex];
    public BattleScenePhase Phase { get; }
    public WaitToken Token { get; }
    public ActorRef Actor => Reaction.Actor;
    public ActorRef Target => Reaction.Target;
    public string ActionKind => Reaction.Action;
    public string ReactionKind => Reaction.Kind.ToString();
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
        List<SessionObservation> observations, IReadOnlyList<BattleEffect>? construction = null)
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
        var scene = new BattleSceneState(action, BattleScenePhase.Initialize, 0, new(sequence));
        return Result(current, action.Prepared, scene, revision, sequence, observations);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
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
                next = BattleScenePhase.ActionAnimation; break;
            case BattleScenePhase.ActionAnimation:
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
                if (scene.TargetDefeated) { next = BattleScenePhase.DeathMessage; break; }
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
        return Result(current, battle, new(scene.Action, next, index, new(sequence), motion, growthNotices, noticeIndex), revision, sequence, observations);
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
