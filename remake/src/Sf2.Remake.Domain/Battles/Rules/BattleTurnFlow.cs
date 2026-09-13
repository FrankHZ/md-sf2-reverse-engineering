namespace Sf2.Remake.Domain.Battles;

internal static class BattleTurnFlow
{
    internal static EngineBattleState Start(BattleDefinition definition, uint main, uint thinking) =>
        new(definition, definition.InitialActors, main, thinking, 0, [], 0);

    internal static EngineBattleState GenerateRound(EngineBattleState battle)
    {
        var generated = TurnOrderRules.Generate(battle.Actors.Select(a =>
            new TurnOrderCandidate(a.Definition.Slot, true, a.Hp, a.Definition.Agility)),
            (ushort)(battle.MainSeed >> 16));
        return battle.With(mainSeed: ((uint)generated.NextSeed << 16) | (battle.MainSeed & 0xFFFF),
            round: checked(battle.Round + 1), queue: generated.Slots, cursor: 0);
    }

    // The original combatant-byte sentinel starts a round; negative AGI entries behind it do not run.
    internal static bool AtRoundEnd(EngineBattleState battle) =>
        battle.Cursor >= battle.Queue.Count || battle.Queue[battle.Cursor].ActorSlot == 255;

    internal static BattleActorState QueuedActor(EngineBattleState battle) =>
        battle.Actors.Single(a => a.Definition.Slot == battle.Queue[battle.Cursor].ActorSlot);

    internal static EngineBattleState ConsumeEntry(EngineBattleState battle) =>
        battle.With(cursor: checked(battle.Cursor + 1));
}
