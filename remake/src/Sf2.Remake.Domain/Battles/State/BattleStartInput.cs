using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// External controlled/session input. Each start is validated against admitted definitions before use.
public sealed record BattleActorStartInput(ActorRef Actor, ushort Hp, byte Mp, byte? Exp,
    ushort? Kills, ushort? Defeats, ushort Status, MapPosition? PositionOverride, BattleActorProgress? Progress = null,
    BattleSourceLoadout? SourceLoadout = null, ushort? ActivationWord = null, bool? Initialized = null, bool Absent = false, bool RetainDeadPosition = false);

public sealed class BattleStartInput
{
    public BattleStartInput(string encounter, IEnumerable<BattleActorStartInput> actors,
        uint mainSeed, uint thinkingSeed, uint? gold, NewBattleStartPolicy? newBattle = null,
        IEnumerable<ActorRef>? activeAllies = null, BattleItemCustody? itemCustody = null,
        IEnumerable<int>? storyFlags = null, BattleVisualState? visuals = null, IEnumerable<int>? retainedSourceSlots = null)
    {
        ArgumentNullException.ThrowIfNull(actors);
        Visuals = visuals; RetainedSourceSlots = Array.AsReadOnly((retainedSourceSlots ?? []).ToArray());
        Encounter = encounter; Actors = Array.AsReadOnly(actors.ToArray());
        MainSeed = mainSeed; ThinkingSeed = thinkingSeed; Gold = gold; NewBattle = newBattle; ItemCustody = itemCustody;
        ActiveAllies = activeAllies is null ? null : Array.AsReadOnly(activeAllies.ToArray());
        StoryFlags = Array.AsReadOnly((storyFlags ?? []).Distinct().Order().ToArray());
    }
    public string Encounter { get; }
    public IReadOnlyList<BattleActorStartInput> Actors { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public uint? Gold { get; }
    public NewBattleStartPolicy? NewBattle { get; }
    public IReadOnlyList<ActorRef>? ActiveAllies { get; }
    public BattleItemCustody? ItemCustody { get; }
    public IReadOnlyList<int> StoryFlags { get; }
    public BattleVisualState? Visuals { get; }
    public IReadOnlyList<int> RetainedSourceSlots { get; }
}
