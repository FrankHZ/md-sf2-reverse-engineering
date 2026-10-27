using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

// Source-ordered writes for the named two-actor Jaro operation. No faction mutation.
internal static class BattleStoryOperations
{
    internal static BattleActorState Actor(SessionSnapshot current, int slot, bool ally)
    {
        if (current.Mode != SessionMode.Battle || current.Battle.Definition.Initialization is not { } init ||
            !init.SourceCombatants.TryGetValue(slot, out var actor) || current.Battle.GetActor(actor).IsAlly != ally ||
            (ally ? slot is < 0 or >= 30 : slot is < 128 or >= 160))
            throw new BattleRuleException("battle-story-slot", "program.sourceSlot", true);
        return current.Battle.GetActor(actor);
    }

    internal static SessionSnapshot Convert(SessionSnapshot current, ConvertBattleActor operation, List<SessionObservation> observations)
    {
        try
        {
        var enemy = Actor(current, operation.EnemySlot, false);
        var ally = Actor(current, operation.AllySlot, true);
        var position = enemy.Position; // retained live slot, including absent/dead; never the definition start
        current = Write(current, current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == enemy.Actor
            ? a.With(hp: 0, clearPosition: true) : a)), current.Story, observations, "conversion-enemy-removed");
        current = Write(current, current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == ally.Actor
            ? a.With(position: position, clearPosition: position is null) : a)), current.Story, observations, "conversion-ally-positioned");
        current = Join(current, operation.AllySlot, observations);
        current = Write(current, current.Battle, current.Story.Copy(current.Story.Cursor,
            flags: ProgramRunner.Flags(current.Story, 32 + operation.AllySlot, true)), observations, "conversion-explicit-active");
        // Invalid retained lookup fails HERE, preserving conversion, flags, lists and PC.
        var visuals = current.Battle.Visuals ?? throw new BattleRuleException("battle-visual-alias", "battle.visuals", true);
        return Write(current, current.Battle.With(visuals: visuals.CopyAlias(enemy.Actor, ally.Actor)), current.Story,
            observations, "conversion-alias-copied");
        }
        catch (BattleRuleException error) { throw new PrefixFailure(current, error); }
    }

    internal static SessionSnapshot JoinAi(SessionSnapshot current, JoinBattleForceAi operation, List<SessionObservation> observations)
    {
        var ally = Actor(current, operation.AllySlot, true);
        ushort word = ally.ActivationWord ?? throw new BattleRuleException("missing-activation-word", "actor.activation", true);
        word = operation.Value != 0 ? (ushort)(word | 4) : (ushort)(word & ~4);
        if (operation.Value != 0) current = Join(current, operation.AllySlot, observations);
        return Write(current, current.Battle.With(actors: current.Battle.Actors.Select(a =>
            a.Actor == ally.Actor ? a.With(activationWord: word) : a)), current.Story, observations, "join-force-ai-word");
    }

    private static SessionSnapshot Join(SessionSnapshot current, int member, List<SessionObservation> observations)
    {
        var layout = BattleStoryDefinition.PartyFlags;
        current = Write(current, current.Battle, current.Story.Copy(current.Story.Cursor,
            flags: ProgramRunner.Flags(current.Story, layout.JoinedStart + member, true)), observations, "join-force-joined");
        var lists = MapPartyMembership.Rebuild(current.Story.Flags, layout);
        current = Write(current, current.Battle, current.Story.Copy(current.Story.Cursor, partyLists: lists), observations, "join-force-list-rebuilt");
        if (lists.Active.Count < layout.Capacity)
            current = Write(current, current.Battle, current.Story.Copy(current.Story.Cursor,
                flags: ProgramRunner.Flags(current.Story, layout.ActiveStart + member, true)), observations, "join-force-conditional-active");
        return current;
    }

    internal static BattleVisual Visual(SessionSnapshot current, EntityRef entity)
    {
        var actor = new ActorRef(entity.Value);
        if (!current.Battle.Actors.Any(a => a.Actor == actor))
            throw new BattleRuleException("battle-story-entity", "program.entity", true);
        return (current.Battle.Visuals ?? throw new BattleRuleException("battle-visual-alias", "battle.visuals", true)).Resolve(actor);
    }

    private static SessionSnapshot Write(SessionSnapshot current, EngineBattleState battle, StoryState story,
        List<SessionObservation> observations, string kind) => ProgramRunner.Commit(current,
            ((ActiveBattle)current.Active) with { Battle = battle }, story, observations, kind);

    // Return the last accepted prefix when an operation fails after committing writes.
    internal sealed class PrefixFailure(SessionSnapshot snapshot, BattleRuleException error) : Exception
    { internal SessionSnapshot Snapshot { get; } = snapshot; internal BattleRuleException Error { get; } = error; }
}
