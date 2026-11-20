using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Domain.Battles.BattleContinuationRestore;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class BattleRestore
{
    internal static SessionResult Build(ScenarioDefinition definition, BattleSaveData data, SessionRules rules)
    {
        var expected = OpeningSave.FieldCompatibility(definition, rules);
        int revision = data.Compatibility.RecipeRevision;
        Require(revision >= 1 && revision <= expected.RecipeRevision &&
            data.Compatibility == (expected with { RecipeRevision = revision }), "incompatible");
        Require(data.BattleRecipeRevision == (data.EnemyDefinitions is null ? 1 : 2), "recipe-version");
        Require(definition.Encounters.TryGetValue(data.Battle.Encounter, out var encounter), "encounter");
        if (data.EnemyDefinitions is { } ids)
        {
            var route = BattleSaveStory.Route(definition, data);
            Require(route.RepeatUpgradeLevel.HasValue && route.CompletedFlag is { } flag && data.Story.Flags.Contains(flag) &&
                encounter!.Deployments.Where(row => row.Faction == BattleFaction.Enemy).All(row => row.Replacements is not null), "repeat-enemy-encounter");
            encounter = RepeatEnemySelection.Bind(encounter!, ids);
        }
        if (definition.PrivateDefinitions is not null)
        {
            // Historical encounters stored part of the party in the field-player binding.
            // Restore the saved partition. The story validator checks the full union and active membership.
            var actors = data.Battle.Actors.Select(actor => actor.Actor).ToHashSet(StringComparer.Ordinal);
            encounter = encounter!.WithDeployments(encounter.Deployments.Where(row =>
                row.Faction == BattleFaction.Enemy || actors.Contains(row.Actor.Value)));
        }
        var roster = BattleSaveStory.AllyDefinitions(definition);
        BattleActorProgress? RestoreClass(ActorRef actor, BattleActorProgress? progress)
        {
            if (!roster.TryGetValue(actor, out var row) || row.Definition.Growth is not { } growth) return progress;
            Require((definition.PrivateDefinitions is null || progress is not null) && (!data.HasClassTerms || progress?.ClassId is not null), "class");
            if (progress is null || progress.ClassId.HasValue) return progress;
            return progress with { ClassId = growth.ClassId };
        }
        var player = data.Story.Player;
        data = data with { Battle = data.Battle with { Actors = data.Battle.Actors.Select(a =>
                a with { Progress = RestoreClass(new(a.Actor), a.Progress) }).ToArray() },
            Story = data.Story with { Player = player with { RetainedAllies = player.RetainedAllies.Select(a =>
                a with { Progress = RestoreClass(a.Actor, a.Progress) }).ToArray() } } };
        var battle = BattleContinuationRestore.Build(encounter!, data.Battle);
        if (revision < expected.RecipeRevision)
            data = data with { Story = definition.Persistence!.UpgradeBattleRoster(definition, data, battle, expected) };
        var story = BattleSaveStory.Build(definition, data, battle);
        BattleSaveStory.RequireMembership(definition, story, battle);
        _ = rules.Bind([battle.Definition]);
        var actor = BattleTurnFlow.QueuedActor(battle);
        Require(actor.Hp > 0 && actor.Position is not null && actor.Control == BattleControl.Player, "actor-unavailable");
        var selection = new BattleSelection(actor.Actor, BattleMovement.Preview(battle, actor.Actor, actor.Position!), BattleSelectionStage.Movement);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, selection), story, SessionStopReason.PlayerInput);
        BattleSave.RequireStable(snapshot, rules);
        Require(BattleContinuationValues.Same(data.Battle, BattleContinuationValues.Capture(battle)) &&
            BattleSaveStory.Same(data.Story, BattleSaveStory.Capture(story)), "reconstruction-mismatch");
        return new(snapshot, [], SessionStopReason.PlayerInput);
    }
}
