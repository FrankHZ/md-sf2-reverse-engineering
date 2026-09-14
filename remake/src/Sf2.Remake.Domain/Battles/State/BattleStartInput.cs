using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// External controlled/session input. Each start is validated against admitted definitions before use.
public sealed record BattleActorStartInput(ActorRef Actor, ushort Hp, byte Mp, byte? Exp,
    ushort? Kills, ushort? Defeats, ushort Status, MapPosition? PositionOverride);

public sealed class BattleStartInput
{
    public BattleStartInput(string encounter, IEnumerable<BattleActorStartInput> actors,
        uint mainSeed, uint thinkingSeed, uint? gold, NewBattleStartPolicy? newBattle = null,
        IEnumerable<ActorRef>? activeAllies = null)
    {
        ArgumentNullException.ThrowIfNull(actors);
        Encounter = encounter; Actors = Array.AsReadOnly(actors.ToArray());
        MainSeed = mainSeed; ThinkingSeed = thinkingSeed; Gold = gold; NewBattle = newBattle;
        ActiveAllies = activeAllies is null ? null : Array.AsReadOnly(activeAllies.ToArray());
    }
    public string Encounter { get; }
    public IReadOnlyList<BattleActorStartInput> Actors { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public uint? Gold { get; }
    public NewBattleStartPolicy? NewBattle { get; }
    public IReadOnlyList<ActorRef>? ActiveAllies { get; }
}
