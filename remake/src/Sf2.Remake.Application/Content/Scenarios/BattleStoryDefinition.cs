using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

// Battle programs share the interpreter and story state, without an exploration world.
public sealed class BattleStoryDefinition
{
    internal BattleStoryDefinition(IEnumerable<StoryProgram> programs, IReadOnlyDictionary<int, string> texts)
    {
        Programs = new ReadOnlyDictionary<string, StoryProgram>(programs.ToDictionary(p => p.Id));
        Texts = new ReadOnlyDictionary<int, string>(texts.ToDictionary(p => p.Key, p => p.Value));
    }
    public IReadOnlyDictionary<string, StoryProgram> Programs { get; }
    public IReadOnlyDictionary<int, string> Texts { get; }
    internal void RequireStart(BattleDefinition battle, BattleStartInput start)
    {
        var slots = battle.Initialization?.SourceCombatants;
        if (slots is null || slots.Count != battle.Deployments.Count || slots.Values.Distinct().Count() != slots.Count ||
            slots.Any(row => !battle.Deployments.Any(d => d.Actor == row.Value &&
                (d.Faction == BattleFaction.Ally ? row.Key is >= 0 and < 30 : row.Key is >= 128 and < 160))))
            throw new BattleRuleException("battle-story-slots", "battle.sourceCombatants");
        foreach (var operation in Programs.Values.SelectMany(p => p.Instructions).OfType<ConvertBattleActor>())
        {
            if (!slots.TryGetValue(operation.EnemySlot, out var enemy) || !slots.ContainsKey(operation.AllySlot))
                throw new BattleRuleException("battle-story-slot", "program.sourceSlot");
            if (!start.StoryFlags.Contains(operation.AllySlot)) continue;
            var retained = start.Actors.Single(a => a.Actor == enemy);
            if (start.NewBattle is not null || !start.RetainedSourceSlots.Contains(operation.EnemySlot) ||
                retained.PositionOverride is null && !retained.Absent)
                throw new BattleRuleException("retained-conversion-source-required", "start.retainedSourceSlots", true);
        }
    }
    public static MapPartyFlagLayout PartyFlags { get; } = new(30, 0, 32, 12);
}
