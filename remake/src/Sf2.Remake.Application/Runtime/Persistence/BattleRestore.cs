using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
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
        Require(data.BattleRecipeRevision == 1, "recipe-version");
        Require(definition.Encounters.TryGetValue(data.Battle.Encounter, out var encounter), "encounter");
        var battle = BattleContinuationRestore.Build(encounter!, data.Battle);
        if (revision < expected.RecipeRevision)
            data = data with { Story = definition.Persistence!.UpgradeBattleRoster(definition, data, battle, expected) };
        var story = BattleSaveStory.Build(definition, data, battle);
        BattleSaveStory.RequireMembership(definition, story, battle);
        _ = rules.Bind([encounter!]);
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
