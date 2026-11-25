using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

internal enum HealingWorkKind { Wait, TimedInput, Enable, Stop, Drain, Cleanup }
internal sealed record HealingWork(HealingWorkKind Kind, int Count, string Caller, int Frame = -1);

// One continuation local to this scene. Counters are source work, not host frames.
// Presentation delivery can finish before or after them without adding work.
public sealed record HealingDisplay(ActorRef Caster, ActorRef Target, SpellRef Spell, int RecoveredHp,
    string CasterName, string TargetName, int? ActionSourceText, int? RecoverySourceText);

public sealed record HealingSceneCursor
{
    internal bool EnemyFairy { get; init; }
    internal BattleSceneDefinition? AllyAuraContent { get; init; }
    internal BattleSceneAnimation Cast { get; init; } = null!;
    internal int CasterIdleTicks { get; init; }
    internal int TargetIdleTicks { get; init; }
    internal IReadOnlyList<HealingWork> Work { get; init; } = [];
    internal int Index { get; init; }
    public int Remaining { get; internal init; }
    public bool Delivered { get; internal init; }
    public HealingFairyState? Fairy { get; internal init; }
    public bool CastRequested { get; internal init; }
    public HealingDisplay Display { get; internal init; } = null!;
    public string ActionText { get; internal init; } = "";
    public string RecoveryText { get; internal init; } = "";
    public bool LogicalComplete => Index >= Work.Count;
    public bool AtTimedInput => !LogicalComplete && Work[Index].Kind == HealingWorkKind.TimedInput;
    public string? Caller => LogicalComplete ? null : Work[Index].Caller;
    public int CastFrame => LogicalComplete ? -1 : Work[Index].Frame;
    public int Opportunity => LogicalComplete ? 0 : Work[Index].Count - Remaining;

    internal static void RequireContent(EngineBattleState battle, ActorRef actorRef, ActorRef targetRef,
        BattleSceneDefinition? content, bool required, string sequence = "cast")
    {
        if (content is null)
        {
            if (required) throw new BattleRuleException("healing-scene-content", "battleScenes.healing", true);
            return;
        }
        if (content.Healing is null ||
            !TryVisual(battle.GetActor(actorRef), content, out var visual) ||
            !visual.Sequences.ContainsKey(sequence) || visual.IdleTicks <= 0 ||
            !TryVisual(battle.GetActor(targetRef), content, out var targetVisual) || targetVisual.IdleTicks <= 0)
            throw new BattleRuleException("healing-scene-content", "battleScenes.healing", true);
    }

    private static bool TryVisual(BattleActorState actor, BattleSceneDefinition content, out BattleSceneActorVisual visual) =>
        actor.IsAlly ? content.TryAllyVisual(actor, out visual!) : content.Enemies.TryGetValue(actor.Actor, out visual!);

    internal static HealingSceneCursor Create(BattleActionResolution action, BattleSceneDefinition? content, StoryState? story = null, int index = 0)
    {
        var spell = action.Spell ?? action.Item!.EquipmentEffect!;
        string sequence = action.Item is null ? "cast" : "item";
        RequireContent(action.Prepared, action.Actor, action.Reactions[index].Target, content, false, sequence);
        var actor = action.Prepared.GetActor(action.Actor);
        var target = action.Prepared.GetActor(action.Reactions[index].Target);
        // Authored packages have an intentionally small authored gesture; private
        // scenes require their admitted original sequence rather than this fallback.
        var cast = new BattleSceneAnimation(0, 2, 255, false, null,
            [new(0, 1, 0, 0, null), new(0, 1, 0, 0, null)]);
        int casterIdle = 1, targetIdle = 1;
        string ActorName(BattleActorState a) => !a.IsAlly ? story is null ? content?.EnemyNames?.GetValueOrDefault(a.Actor) ?? a.Actor.Value
            : CharacterNames.Battle(story, a, content) : story is null ? content is null ? a.Actor.Value : content.MemberNames[a.ProcessingOrder]
            : CharacterNames.Actor(story, a.Actor, content is null ? a.Actor.Value : content.MemberNames[a.Deployment.Initialization?.AllyPartyMember ?? a.ProcessingOrder]);
        string actionText = action.Item is null ? "{NAME} cast\n{SPELL} level {#}!" : "{NAME} used {ITEM}!", recoveryText = "{NAME} recovered\n{#} hit points.";
        if (content is not null)
        {
            _ = TryVisual(actor, content, out var visual);
            _ = TryVisual(target, content, out var targetVisual);
            cast = visual.Sequences[sequence];
            casterIdle = visual.IdleTicks; targetIdle = targetVisual.IdleTicks;
            actionText = content.Texts[action.Item is null ? 274 : 275]; recoveryText = content.Texts[298];
        }
        return new() { EnemyFairy = !actor.IsAlly, AllyAuraContent = spell.IsAura && actor.IsAlly ? content : null,
            Cast = cast, CasterIdleTicks = casterIdle, TargetIdleTicks = targetIdle,
            Display = new(action.Actor, target.Actor, spell.Spell, action.Reactions[index].Amount,
                story is null ? CharacterNames.ReadableIdentity(actor.Actor) : CharacterNames.Battle(story, actor, content),
                story is null ? CharacterNames.ReadableIdentity(target.Actor) : CharacterNames.Battle(story, target, content),
                content is null ? null : action.Item is null ? 274 : 275, content is null ? null : 298),
            ActionText = actionText.Replace("{ITEM}", action.Item?.Name ?? "").Replace("{NAME}", ActorName(actor)).Replace("{SPELL}", spell.Spell.Value.ToUpperInvariant())
                .Replace("{#}", spell.Spell.Level.ToString()).Replace("{N}", "\n"),
            RecoveryText = recoveryText.Replace("{NAME}", ActorName(target)).Replace("{#}", action.Reactions[index].Amount.ToString()).Replace("{N}", "\n") };
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
                var setup = HealingFairy.Begin(level, seed, actor, next.EnemyFairy); seed = setup.Seed; effects.AddRange(setup.Effects);
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
