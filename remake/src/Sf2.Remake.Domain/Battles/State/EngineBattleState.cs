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
public enum BattleClassRule { UnpromotedPriest, Ordinary, UnpromotedSwordsman, UnpromotedWarrior, UnpromotedKnight }
public enum BattleControl { Player, Automatic }
public enum BattleAiStrategy { Stay, AttackThenApproach, SourceOrders }
public enum BattleFaction { Ally, Enemy }

public sealed class PhysicalCriticalRule
{
    private PhysicalCriticalRule(ushort chanceDenominator, int damageBonusShift)
    { ChanceDenominator = chanceDenominator; DamageBonusShift = damageBonusShift; }
    public static PhysicalCriticalRule OneIn32WithHalfBonus { get; } = new(32, 1);
    public static PhysicalCriticalRule OneIn16WithQuarterBonus { get; } = new(16, 2);
    public ushort ChanceDenominator { get; }
    public int DamageBonusDenominator => 1 << DamageBonusShift;
    internal int DamageBonusShift { get; }
}

public sealed record PhysicalActorDefinition(PhysicalCriticalRule Critical, bool Promoted, bool Leader, ushort Gold,
    byte MinimumRange = 1, byte MaximumRange = 1);
public sealed record BattleRewardDefinition(bool HalvedExperience);
public sealed record BattleOutcomeDefinition(ActorRef Leader, ActorRef DefeatedGateEnemy);
public enum BattleOutcomeKind { Victory, Defeat }

public sealed record HealingSpellDefinition(
    SpellRef Spell, byte MpCost, ushort Power, byte MinimumRange, byte MaximumRange);

public sealed class BattleActorDefinition
{
    internal BattleActorDefinition(ActorRef actor, BattleClassRule classRule,
        byte level, ushort maxHp, byte maxMp,
        byte attack, byte defense, byte agility, bool extraRoundAction, byte move, IEnumerable<SpellRef> spells,
        PhysicalActorDefinition? physical = null, BattleMover mover = BattleMover.Regular,
        BattleSourceLoadout? sourceLoadout = null, BattleGrowthDefinition? growth = null)
    {
        Actor = actor; ClassRule = classRule;
        Level = level; MaxHp = maxHp; MaxMp = maxMp; Attack = attack; Defense = defense;
        Agility = agility; ExtraRoundAction = extraRoundAction; Move = move; Spells = Array.AsReadOnly(spells.ToArray()); Physical = physical;
        Mover = mover; SourceLoadout = sourceLoadout; Growth = growth;
    }
    public ActorRef Actor { get; }
    public BattleClassRule ClassRule { get; }
    internal byte? SourceClassId => ClassRule switch
    {
        BattleClassRule.UnpromotedSwordsman => 0, BattleClassRule.UnpromotedWarrior => 2,
        BattleClassRule.UnpromotedPriest => 4, BattleClassRule.UnpromotedKnight => 1, _ => null,
    };
    public byte Level { get; }
    public ushort MaxHp { get; }
    public byte MaxMp { get; }
    public byte Attack { get; }
    public byte Defense { get; }
    public byte Agility { get; }
    public bool ExtraRoundAction { get; }
    public byte Move { get; }
    public IReadOnlyList<SpellRef> Spells { get; }
    public PhysicalActorDefinition? Physical { get; }
    public BattleMover Mover { get; }
    public BattleSourceLoadout? SourceLoadout { get; }
    public BattleGrowthDefinition? Growth { get; }
    internal BattleActorDefinition WithGrowth(BattleGrowthDefinition growth) => new(Actor, ClassRule,
        Level, MaxHp, MaxMp, Attack, Defense, Agility, ExtraRoundAction, Move, Spells, Physical, Mover, SourceLoadout, growth);
}

public sealed class BattleActorState
{
    internal BattleActorState(BattleDeploymentDefinition deployment, ushort hp, byte mp, byte? exp,
        MapPosition? position, ushort? kills, ushort? defeats, ActorRef? lastTarget = null,
        byte? attack = null, ushort status = 0, ushort? activationWord = null, byte aiMemory = 0,
        BattleActorProgress? progress = null)
    { Deployment = deployment; Hp = hp; Mp = mp; Exp = exp; Position = hp == 0 ? null : position;
        Kills = kills; Defeats = defeats; LastTarget = lastTarget;
        Progress = progress;
        Attack = attack ?? (progress is null ? deployment.Definition.Attack :
            (byte)Math.Min(200, progress.BaseAttack + (deployment.Definition.Growth?.AttackBonus ?? 0)));
        Status = status; ActivationWord = activationWord; AiMemory = aiMemory; }
    public BattleDeploymentDefinition Deployment { get; }
    public BattleActorDefinition Definition => Deployment.Definition;
    public BattleFaction Faction => Deployment.Faction;
    public BattleControl Control => Deployment.Control;
    public BattleAiStrategy? AiStrategy => Deployment.AiStrategy;
    public int ProcessingOrder => Deployment.ProcessingOrder;
    public bool IsAlly => Faction == BattleFaction.Ally;
    public ActorRef Actor => Definition.Actor;
    public BattleActorProgress? Progress { get; }
    public byte Level => Progress?.Level ?? Definition.Level;
    public ushort MaxHp => Progress?.MaxHp ?? Definition.MaxHp;
    public byte MaxMp => Progress?.MaxMp ?? Definition.MaxMp;
    public byte BaseAttack => Progress?.BaseAttack ?? (byte)(Definition.Attack - (Definition.Growth?.AttackBonus ?? 0));
    public byte Defense => Progress?.Defense ?? Definition.Defense;
    public byte Agility => Progress?.Agility ?? Definition.Agility;
    public IReadOnlyList<SpellRef> Spells => Progress?.Spells ?? Definition.Spells;
    public BattleSourceLoadout? SourceLoadout => Progress?.SourceLoadout ?? Definition.SourceLoadout;
    public ushort Hp { get; }
    public byte Mp { get; }
    public byte? Exp { get; }
    public MapPosition? Position { get; }
    public ushort? Kills { get; }
    public ushort? Defeats { get; }
    public ActorRef? LastTarget { get; }
    public byte Attack { get; }
    public ushort Status { get; }
    public ushort? ActivationWord { get; }
    public byte AiMemory { get; }
    internal BattleActorState With(ushort? hp = null, byte? mp = null, byte? exp = null,
        MapPosition? position = null, ushort? kills = null, ushort? defeats = null, ActorRef? lastTarget = null,
        byte? attack = null, ushort? status = null, ushort? activationWord = null, byte? aiMemory = null,
        BattleActorProgress? progress = null) =>
        new(Deployment, hp ?? Hp, mp ?? Mp, exp ?? Exp, position ?? Position, kills ?? Kills, defeats ?? Defeats,
            lastTarget ?? LastTarget, attack ?? Attack, status ?? Status, activationWord ?? ActivationWord, aiMemory ?? AiMemory,
            progress ?? Progress);
}

public sealed record BattleDeploymentDefinition(BattleActorDefinition Definition, BattleFaction Faction,
    int ProcessingOrder, BattleControl Control, BattleAiStrategy? AiStrategy, MapPosition Position,
    BattleDeploymentInitialization? Initialization = null)
{
    public ActorRef Actor => Definition.Actor;
}

public sealed class BattleDefinition
{
    internal BattleDefinition(string encounter, MapId map, int width, int height,
        IEnumerable<BattleTerrain> terrain, IEnumerable<BattleDeploymentDefinition> deployments,
        IEnumerable<HealingSpellDefinition> spells, BattleRewardDefinition? rewards = null,
        BattleInitializationDefinition? initialization = null, BattleOutcomeDefinition? outcome = null)
    {
        Encounter = encounter; Map = map; Width = width; Height = height;
        Terrain = Array.AsReadOnly(terrain.ToArray());
        Deployments = Array.AsReadOnly(deployments.OrderBy(deployment => deployment.ProcessingOrder).ToArray());
        Spells = new ReadOnlyDictionary<SpellRef, HealingSpellDefinition>(spells.ToDictionary(s => s.Spell));
        Rewards = rewards; Initialization = initialization; Outcome = outcome;
    }
    public string Encounter { get; }
    public MapId Map { get; }
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<BattleTerrain> Terrain { get; }
    public IReadOnlyList<BattleDeploymentDefinition> Deployments { get; }
    public IReadOnlyDictionary<SpellRef, HealingSpellDefinition> Spells { get; }
    public BattleRewardDefinition? Rewards { get; }
    public BattleInitializationDefinition? Initialization { get; }
    public BattleOutcomeDefinition? Outcome { get; }
    public bool Contains(MapPosition position) => position.X < Width && position.Y < Height;
}

public sealed class EngineBattleState
{
    internal EngineBattleState(BattleDefinition definition, IEnumerable<BattleActorState> actors,
        uint mainSeed, uint thinkingSeed, int round, IEnumerable<TurnOrderEntry<ActorRef>> queue, int cursor, uint? gold,
        NewBattleStartPolicy? startPolicy = null, BattleRegionState? regions = null)
    {
        Definition = definition; Actors = Array.AsReadOnly(actors.ToArray());
        MainSeed = mainSeed; ThinkingSeed = thinkingSeed; Round = round;
        Queue = Array.AsReadOnly(queue.ToArray()); Cursor = cursor;
        Gold = gold; StartPolicy = startPolicy; Regions = regions;
    }
    public BattleDefinition Definition { get; }
    public IReadOnlyList<BattleActorState> Actors { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public int Round { get; }
    public int Cursor { get; }
    public uint? Gold { get; }
    public NewBattleStartPolicy? StartPolicy { get; }
    public BattleRegionState? Regions { get; }
    public IEnumerable<(ActorRef? Actor, byte AlteredAgility)> TurnOrder => Queue.Select(entry => (entry.Actor, entry.AlteredAgility));
    internal IReadOnlyList<TurnOrderEntry<ActorRef>> Queue { get; }
    public BattleActorState GetActor(ActorRef actor) => Actors.Single(a => a.Actor == actor);
    internal EngineBattleState With(IEnumerable<BattleActorState>? actors = null, uint? mainSeed = null,
        int? round = null, IEnumerable<TurnOrderEntry<ActorRef>>? queue = null, int? cursor = null, uint? gold = null,
        uint? thinkingSeed = null, BattleRegionState? regions = null) =>
        new(Definition, actors ?? Actors, mainSeed ?? MainSeed, thinkingSeed ?? ThinkingSeed,
            round ?? Round, queue ?? Queue, cursor ?? Cursor, gold ?? Gold, StartPolicy, regions ?? Regions);
}

internal sealed class BattleRuleException(string code, string field, bool unsupported = false)
    : Exception(code)
{
    internal string Code { get; } = code;
    internal string Field { get; } = field;
    internal bool Unsupported { get; } = unsupported;
}

internal sealed record BattleEffect(string Kind, ActorRef Actor, long? Before = null, long? After = null,
    ushort? RandomRange = null, ushort? RandomValue = null, ActorRef? Target = null);
