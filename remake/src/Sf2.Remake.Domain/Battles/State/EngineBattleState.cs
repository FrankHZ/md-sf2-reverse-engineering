using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Sf2.Remake.Domain.Maps;

[assembly: InternalsVisibleTo("Sf2.Remake.Application")]
[assembly: InternalsVisibleTo("Sf2.Remake.Content")]
[assembly: InternalsVisibleTo("Sf2.Remake.Engine.Tests")]
[assembly: InternalsVisibleTo("Sf2.Remake.Reference")]

namespace Sf2.Remake.Domain.Battles;

public readonly record struct ActorRef(string Value);
public readonly record struct SpellRef(string Value, byte Level);
public enum BattleClassRule { UnpromotedPriest, Ordinary }
public enum BattleController { Player, Stay }

public sealed record PhysicalActorDefinition(byte Prowess, bool Promoted, bool Leader, ushort Gold, ushort InitialKills);
public sealed record BattleRewardDefinition(bool HalvedExperience, uint InitialGold);

public sealed record HealingSpellDefinition(
    SpellRef Spell, byte MpCost, ushort Power, byte MinimumRange, byte MaximumRange);

public sealed class BattleActorDefinition
{
    internal BattleActorDefinition(ActorRef actor, byte slot, BattleClassRule classRule,
        BattleController controller, byte level, ushort maxHp, byte maxMp,
        byte attack, byte defense, byte agility, byte move, IEnumerable<SpellRef> spells,
        PhysicalActorDefinition? physical = null)
    {
        Actor = actor; Slot = slot; ClassRule = classRule; Controller = controller;
        Level = level; MaxHp = maxHp; MaxMp = maxMp; Attack = attack; Defense = defense;
        Agility = agility; Move = move; Spells = Array.AsReadOnly(spells.ToArray()); Physical = physical;
    }
    public ActorRef Actor { get; }
    public byte Slot { get; }
    public BattleClassRule ClassRule { get; }
    public BattleController Controller { get; }
    public byte Level { get; }
    public ushort MaxHp { get; }
    public byte MaxMp { get; }
    public byte Attack { get; }
    public byte Defense { get; }
    public byte Agility { get; }
    public byte Move { get; }
    public IReadOnlyList<SpellRef> Spells { get; }
    public PhysicalActorDefinition? Physical { get; }
    public bool IsAlly => Slot < 128;
}

public sealed class BattleActorState
{
    internal BattleActorState(BattleActorDefinition definition, ushort hp, byte mp, byte exp,
        MapPosition? position, ushort? kills = null)
    { Definition = definition; Hp = hp; Mp = mp; Exp = exp; Position = hp == 0 ? null : position;
        Kills = kills ?? definition.Physical?.InitialKills ?? 0; }
    public BattleActorDefinition Definition { get; }
    public ActorRef Actor => Definition.Actor;
    public ushort Hp { get; }
    public byte Mp { get; }
    public byte Exp { get; }
    public MapPosition? Position { get; }
    public ushort Kills { get; }
    internal BattleActorState With(ushort? hp = null, byte? mp = null, byte? exp = null,
        MapPosition? position = null, ushort? kills = null, bool remove = false) =>
        new(Definition, hp ?? Hp, mp ?? Mp, exp ?? Exp, remove ? null : position ?? Position, kills ?? Kills);
}

public sealed class BattleDefinition
{
    internal BattleDefinition(string encounter, MapId map, int width, int height,
        IEnumerable<byte> terrain, IEnumerable<BattleActorState> actors,
        IEnumerable<HealingSpellDefinition> spells, BattleRewardDefinition? rewards = null)
    {
        Encounter = encounter; Map = map; Width = width; Height = height;
        Terrain = Array.AsReadOnly(terrain.ToArray());
        InitialActors = Array.AsReadOnly(actors.ToArray());
        Spells = new ReadOnlyDictionary<SpellRef, HealingSpellDefinition>(spells.ToDictionary(s => s.Spell));
        Rewards = rewards;
    }
    public string Encounter { get; }
    public MapId Map { get; }
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<byte> Terrain { get; }
    public IReadOnlyList<BattleActorState> InitialActors { get; }
    public IReadOnlyDictionary<SpellRef, HealingSpellDefinition> Spells { get; }
    public BattleRewardDefinition? Rewards { get; }
    public bool Contains(MapPosition position) => position.X < Width && position.Y < Height;
}

public sealed class EngineBattleState
{
    internal EngineBattleState(BattleDefinition definition, IEnumerable<BattleActorState> actors,
        uint mainSeed, uint thinkingSeed, int round, IEnumerable<TurnOrderEntry> queue, int cursor, uint? gold = null)
    {
        Definition = definition; Actors = Array.AsReadOnly(actors.ToArray());
        MainSeed = mainSeed; ThinkingSeed = thinkingSeed; Round = round;
        Queue = Array.AsReadOnly(queue.ToArray()); Cursor = cursor;
        Gold = gold ?? definition.Rewards?.InitialGold ?? 0;
    }
    public BattleDefinition Definition { get; }
    public IReadOnlyList<BattleActorState> Actors { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public int Round { get; }
    public int Cursor { get; }
    public uint Gold { get; }
    internal IReadOnlyList<TurnOrderEntry> Queue { get; }
    public BattleActorState GetActor(ActorRef actor) => Actors.Single(a => a.Actor == actor);
    internal EngineBattleState With(IEnumerable<BattleActorState>? actors = null, uint? mainSeed = null,
        int? round = null, IEnumerable<TurnOrderEntry>? queue = null, int? cursor = null, uint? gold = null) =>
        new(Definition, actors ?? Actors, mainSeed ?? MainSeed, ThinkingSeed,
            round ?? Round, queue ?? Queue, cursor ?? Cursor, gold ?? Gold);
}

internal sealed class BattleRuleException(string code, string field, bool unsupported = false)
    : Exception(code)
{
    internal string Code { get; } = code;
    internal string Field { get; } = field;
    internal bool Unsupported { get; } = unsupported;
}

internal sealed record BattleEffect(string Kind, ActorRef Actor, long Before, long After,
    ushort? RandomRange = null, ushort? RandomValue = null);
