using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;

[assembly: InternalsVisibleTo("Sf2.Remake.Application")]
[assembly: InternalsVisibleTo("Sf2.Remake.Content")]
[assembly: InternalsVisibleTo("Sf2.Remake.Engine.Tests")]

namespace Sf2.Remake.Domain.Battles;

public readonly record struct ActorRef(string Value);
public readonly record struct SpellRef(string Value, byte Level);
public enum BattleActionKind { Stay, Healing, Physical, Item, Retreat, PrismLaser, AttackSpell, SupportSpell }
public readonly record struct BattleActionRef(BattleActionKind Kind, SpellRef? Spell = null, int? ItemSlot = null);
public enum BattleClassRule { UnpromotedPriest, Ordinary, UnpromotedSwordsman, UnpromotedWarrior, UnpromotedKnight, UnpromotedMage, UnpromotedArcher, UnpromotedThief, UnpromotedTortoise, UnpromotedPhoenix, UnpromotedRanger, UnpromotedWolfman, UnpromotedBirdman }
public enum BattleControl { Player, Automatic }
public enum BattleFaction { Ally, Enemy }

public sealed class PhysicalCriticalRule
{
    private PhysicalCriticalRule(ushort chanceDenominator, int damageBonusShift, bool inflictsSleep = false,
        ushort doubleChanceDenominator = 32, ushort counterChanceDenominator = 32, bool inflictsSilence = false, bool inflictsPoison = false, bool inflictsStun = false)
    { InflictsStun = inflictsStun; InflictsPoison = inflictsPoison; InflictsSilence = inflictsSilence; CounterChanceDenominator = counterChanceDenominator; DoubleChanceDenominator = doubleChanceDenominator; ChanceDenominator = chanceDenominator; DamageBonusShift = damageBonusShift; InflictsSleep = inflictsSleep; }
    public static PhysicalCriticalRule OneIn32WithHalfBonus { get; } = new(32, 1);
    public static PhysicalCriticalRule OneIn16WithQuarterBonus { get; } = new(16, 2);
    public static PhysicalCriticalRule OneIn32WithFrequentDoubleAndCounter { get; } = new(32, 1, doubleChanceDenominator: 16, counterChanceDenominator: 16);
    public static PhysicalCriticalRule OneIn8WithHalfBonus { get; } = new(8, 1);
    public static PhysicalCriticalRule OneIn4WithFrequentDoubleAndCounter { get; } = new(4, 1, doubleChanceDenominator: 8, counterChanceDenominator: 8);
    public static PhysicalCriticalRule Taros { get; } = new(4, 1, doubleChanceDenominator: 16, counterChanceDenominator: 8);
    public static PhysicalCriticalRule Monk { get; } = new(4, 1, doubleChanceDenominator: 16, counterChanceDenominator: 16);
    public static PhysicalCriticalRule Thief { get; } = new(16, 2, doubleChanceDenominator: 16);
    public static PhysicalCriticalRule Cleric { get; } = new(32, 1, counterChanceDenominator: 16);
    public ushort CounterChanceDenominator { get; }
    public ushort DoubleChanceDenominator { get; }
    public static PhysicalCriticalRule Sleep { get; } = new(4, 0, true);
    public static PhysicalCriticalRule Silence { get; } = new(4, 0, doubleChanceDenominator: 16, counterChanceDenominator: 16, inflictsSilence: true);
    public static PhysicalCriticalRule Poison { get; } = new(4, 0, inflictsPoison: true);
    public static PhysicalCriticalRule Stun { get; } = new(4, 0, inflictsStun: true);
    public bool InflictsStun { get; }
    public bool InflictsPoison { get; }
    public bool InflictsSilence { get; }
    public bool InflictsSleep { get; }
    public ushort ChanceDenominator { get; }
    public int DamageBonusDenominator => DamageBonusShift == 0 ? 0 : 1 << DamageBonusShift;
    internal int DamageBonusShift { get; }
}

public sealed record PhysicalActorDefinition(PhysicalCriticalRule Critical, bool Promoted, bool Leader, ushort Gold,
    byte MinimumRange = 1, byte MaximumRange = 1, bool CannotCounter = false);
public sealed record BattleRewardDefinition(bool HalvedExperience);
public sealed record BattleOutcomeDefinition(ActorRef Leader, ActorRef DefeatedGateEnemy,
    IReadOnlyDictionary<int, MapPosition>? DeathPositions = null);
public enum BattleOutcomeKind { Victory, Defeat, Retreat }

public sealed record HealingItemDefinition(byte ItemId, string Name, ushort Power, byte MinimumRange, byte MaximumRange,
    bool CuresPoison = false);

public sealed record RetreatSpellDefinition(SpellRef Spell, byte MpCost);

public enum HealingTargets { Single, AuraRadius, AuraAllAllies }
public sealed record HealingSpellDefinition(
    SpellRef Spell, byte MpCost, ushort Power, byte MinimumRange, byte MaximumRange,
    HealingTargets Targets = HealingTargets.Single, byte Radius = 0, bool CuresPoison = false)
{
    public bool IsAura => Targets is HealingTargets.AuraRadius or HealingTargets.AuraAllAllies;
}

public sealed class BattleActorDefinition
{
    internal BattleActorDefinition(ActorRef actor, BattleClassRule classRule,
        byte level, ushort maxHp, byte maxMp,
        byte attack, byte defense, byte agility, bool extraRoundAction, byte move, IEnumerable<SpellRef> spells,
        PhysicalActorDefinition? physical = null, BattleMover mover = BattleMover.Regular,
        BattleSourceLoadout? sourceLoadout = null, BattleGrowthDefinition? growth = null, BattleEquipmentDefinition? equipment = null, ushort resistance = 0)
    {
        Actor = actor; ClassRule = classRule; Resistance = resistance;
        Level = level; MaxHp = maxHp; MaxMp = maxMp; Attack = attack; Defense = defense;
        Agility = agility; ExtraRoundAction = extraRoundAction; Move = move; Spells = Array.AsReadOnly(spells.ToArray()); Physical = physical;
        Mover = mover; SourceLoadout = sourceLoadout; Growth = growth; Equipment = equipment;
    }
    public ActorRef Actor { get; }
    public BattleClassRule ClassRule { get; }
    public ushort Resistance { get; }
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
    public BattleEquipmentDefinition? Equipment { get; }
    internal BattleActorDefinition WithGrowth(BattleGrowthDefinition growth) => new(Actor, ClassRule,
        Level, MaxHp, MaxMp, Attack, Defense, Agility, ExtraRoundAction, Move, Spells, Physical, Mover, SourceLoadout, growth, Equipment, Resistance);
    internal BattleActorDefinition WithEquipment(BattleEquipmentDefinition equipment) => new(Actor, ClassRule,
        Level, MaxHp, MaxMp, Attack, Defense, Agility, ExtraRoundAction, Move, Spells, Physical, Mover, SourceLoadout, Growth, equipment, Resistance);
}

public sealed class BattleActorState
{
    internal BattleActorState(BattleDeploymentDefinition deployment, ushort hp, byte mp, byte? exp,
        MapPosition? position, ushort? kills, ushort? defeats, ActorRef? lastTarget = null,
        byte? attack = null, ushort status = 0, ushort? activationWord = null, byte aiMemory = 0,
        BattleActorProgress? progress = null, BattleSourceLoadout? sourceLoadout = null, bool retainDeadPosition = false,
        byte? primaryOrder = null, byte? secondaryOrder = null, byte facing = 3, bool initialized = true)
    { SourceLoadout = sourceLoadout ?? progress?.SourceLoadout ?? deployment.Definition.SourceLoadout;
        Initialized = initialized; Deployment = deployment; Hp = hp; Mp = mp; Exp = exp; Position = hp == 0 && !retainDeadPosition ? null : position;
        Kills = kills; Defeats = defeats; LastTarget = lastTarget;
        Progress = progress is null ? null : progress with { SourceLoadout = SourceLoadout };
        Attack = attack ?? BattleEquipment.Attack(Definition, progress, SourceLoadout);
        // PositionBattleEntities declares ordinary allies/enemies with facing3 (DOWN).
        if (facing > 3) throw new ArgumentOutOfRangeException(nameof(facing));
        Facing = facing;
        Status = status; ActivationWord = activationWord; AiMemory = aiMemory;
        PrimaryOrder = primaryOrder ?? deployment.Initialization?.PrimaryOrder;
        SecondaryOrder = secondaryOrder ?? deployment.Initialization?.SecondaryOrder; }
    public BattleDeploymentDefinition Deployment { get; }
    public BattleActorDefinition Definition => Deployment.Definition;
    public BattleFaction Faction => Deployment.Faction;
    public BattleControl Control => Deployment.SourceAllyControl
        ? (ActivationWord & 4) != 0 ? BattleControl.Automatic : BattleControl.Player : Deployment.Control;
    public BattleStrategyRef? AiStrategy => Deployment.AiStrategy;
    public int ProcessingOrder => Deployment.ProcessingOrder;
    public bool IsAlly => Faction == BattleFaction.Ally;
    public ActorRef Actor => Definition.Actor;
    public BattleActorProgress? Progress { get; }
    public byte Level => Progress?.Level ?? Definition.Level;
    public bool Initialized { get; }
    public ushort MaxHp => Initialized ? Progress?.MaxHp ?? Definition.MaxHp : (ushort)0;
    public byte MaxMp => Progress?.MaxMp ?? Definition.MaxMp;
    public byte BaseAttack => Progress?.BaseAttack ?? Definition.Equipment?.BaseAttack ?? (byte)(Definition.Attack - (Definition.Growth?.AttackBonus ?? 0));
    public byte Defense => Progress?.Defense ?? Definition.Defense;
    public byte Agility => Progress?.Agility ?? Definition.Agility;
    public IReadOnlyList<SpellRef> Spells => Progress?.Spells ?? Definition.Spells;
    public BattleSourceLoadout? SourceLoadout { get; }
    public ushort Hp { get; }
    public byte Mp { get; }
    public byte? Exp { get; }
    public MapPosition? Position { get; }
    public byte Facing { get; }
    public ushort? Kills { get; }
    public ushort? Defeats { get; }
    public ActorRef? LastTarget { get; }
    public byte Attack { get; }
    public ushort Status { get; }
    public ushort? ActivationWord { get; }
    public byte AiMemory { get; }
    public byte? PrimaryOrder { get; }
    public byte? SecondaryOrder { get; }
    internal BattleActorState With(ushort? hp = null, byte? mp = null, byte? exp = null,
        MapPosition? position = null, ushort? kills = null, ushort? defeats = null, ActorRef? lastTarget = null,
        byte? attack = null, ushort? status = null, ushort? activationWord = null, byte? aiMemory = null,
        BattleActorProgress? progress = null, BattleSourceLoadout? sourceLoadout = null, bool clearPosition = false,
        byte? primaryOrder = null, byte? secondaryOrder = null, byte? facing = null, bool? initialized = null) =>
        new(Deployment, hp ?? Hp, mp ?? Mp, exp ?? Exp, clearPosition ? null : position ?? Position, kills ?? Kills, defeats ?? Defeats,
            lastTarget ?? LastTarget, attack ?? Attack, status ?? Status, activationWord ?? ActivationWord, aiMemory ?? AiMemory,
            progress ?? Progress, sourceLoadout ?? progress?.SourceLoadout ?? SourceLoadout, retainDeadPosition: true,
            primaryOrder: primaryOrder ?? PrimaryOrder, secondaryOrder: secondaryOrder ?? SecondaryOrder, facing: facing ?? Facing, initialized: initialized ?? Initialized);
}

public sealed record BattleDeploymentDefinition(BattleActorDefinition Definition, BattleFaction Faction,
    int ProcessingOrder, BattleControl Control, BattleStrategyRef? AiStrategy, MapPosition Position,
    BattleDeploymentInitialization? Initialization = null, bool SourceAllyControl = false)
{
    public ActorRef Actor => Definition.Actor;
}

public sealed record BattleMapRegion(MapId Map, MapPosition Origin);

public sealed class BattleDefinition
{
    internal BattleDefinition(string encounter, MapId map, int width, int height,
        IEnumerable<BattleTerrain> terrain, IEnumerable<BattleDeploymentDefinition> deployments,
        IEnumerable<HealingSpellDefinition> spells, BattleRewardDefinition? rewards = null,
        BattleInitializationDefinition? initialization = null, BattleOutcomeDefinition? outcome = null,
        IEnumerable<HealingItemDefinition>? healingItems = null, BattleMapRegion? sourceMapRegion = null,
        IEnumerable<RetreatSpellDefinition>? retreatSpells = null, IEnumerable<RetreatItemDefinition>? retreatItems = null,
        TarosEffectiveness? tarosEffectiveness = null, LeaderCleanupDefinition? leaderCleanup = null,
        EnemyItemDropDefinition? itemDrops = null, PrismLaserDefinition? prismLaser = null, BattleGiveDefinition? give = null,
        BattleDiscardDefinition? discard = null, bool poison = false, IEnumerable<AttackSpellDefinition>? attackSpells = null, IEnumerable<SupportSpellDefinition>? supportSpells = null, BattleSearchDefinition? search = null)
    {
        SupportSpells = new ReadOnlyDictionary<SpellRef, SupportSpellDefinition>((supportSpells ?? []).ToDictionary(s => s.Spell));
        AttackSpells = new ReadOnlyDictionary<SpellRef, AttackSpellDefinition>((attackSpells ?? []).ToDictionary(s => s.Spell));
        Search = search; Poison = poison; Discard = discard; Give = give; PrismLaser = prismLaser; TarosEffectiveness = tarosEffectiveness; LeaderCleanup = leaderCleanup; ItemDrops = itemDrops;
        Encounter = encounter; Map = map; Width = width; Height = height;
        Terrain = Array.AsReadOnly(terrain.ToArray());
        Deployments = Array.AsReadOnly(deployments.OrderBy(deployment => deployment.ProcessingOrder).ToArray());
        Spells = new ReadOnlyDictionary<SpellRef, HealingSpellDefinition>(spells.ToDictionary(s => s.Spell));
        Rewards = rewards; Initialization = initialization; Outcome = outcome; SourceMapRegion = sourceMapRegion;
        RetreatSpells = new ReadOnlyDictionary<SpellRef, RetreatSpellDefinition>((retreatSpells ?? []).ToDictionary(s => s.Spell));
        RetreatItems = new ReadOnlyDictionary<byte, RetreatItemDefinition>((retreatItems ?? []).ToDictionary(item => item.ItemId));
        HealingItems = new ReadOnlyDictionary<byte, HealingItemDefinition>((healingItems ?? []).ToDictionary(item => item.ItemId));
    }
    private BattleDefinition(BattleDefinition source, IEnumerable<BattleDeploymentDefinition>? deployments = null,
        BattleOutcomeDefinition? outcome = null, IEnumerable<RetreatSpellDefinition>? retreatSpells = null,
        IEnumerable<RetreatItemDefinition>? retreatItems = null, BattleGiveDefinition? give = null,
        BattleDiscardDefinition? discard = null, BattleSearchDefinition? search = null) : this(source.Encounter, source.Map, source.Width, source.Height,
        source.Terrain, deployments ?? source.Deployments, source.Spells.Values, source.Rewards, source.Initialization,
        outcome ?? source.Outcome, source.HealingItems.Values, source.SourceMapRegion, retreatSpells ?? source.RetreatSpells.Values,
        retreatItems ?? source.RetreatItems.Values, source.TarosEffectiveness, source.LeaderCleanup, source.ItemDrops, source.PrismLaser,
        give ?? source.Give, discard ?? source.Discard, source.Poison, source.AttackSpells.Values, source.SupportSpells.Values, search ?? source.Search) { }
    internal BattleDefinition WithOutcome(BattleOutcomeDefinition outcome, IEnumerable<RetreatSpellDefinition>? retreatSpells = null,
        IEnumerable<RetreatItemDefinition>? retreatItems = null) => new(this, outcome: outcome, retreatSpells: retreatSpells, retreatItems: retreatItems);
    internal BattleDefinition WithDeployments(IEnumerable<BattleDeploymentDefinition> deployments) => new(this, deployments);
    internal BattleDefinition WithGive(BattleGiveDefinition give) => new(this, give: give);
    internal BattleDefinition WithDiscard(BattleDiscardDefinition discard) => new(this, discard: discard);

    public string Encounter { get; }
    public MapId Map { get; }
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<BattleTerrain> Terrain { get; }
    public IReadOnlyList<BattleDeploymentDefinition> Deployments { get; }
    public IReadOnlyDictionary<SpellRef, HealingSpellDefinition> Spells { get; }
    public IReadOnlyDictionary<SpellRef, AttackSpellDefinition> AttackSpells { get; }
    public IReadOnlyDictionary<SpellRef, SupportSpellDefinition> SupportSpells { get; }
    public IReadOnlyDictionary<byte, HealingItemDefinition> HealingItems { get; }
    public IReadOnlyDictionary<SpellRef, RetreatSpellDefinition> RetreatSpells { get; }
    public IReadOnlyDictionary<byte, RetreatItemDefinition> RetreatItems { get; }
    public BattleRewardDefinition? Rewards { get; }
    public BattleInitializationDefinition? Initialization { get; }
    public BattleOutcomeDefinition? Outcome { get; }
    public BattleMapRegion? SourceMapRegion { get; }
    public TarosEffectiveness? TarosEffectiveness { get; }
    public LeaderCleanupDefinition? LeaderCleanup { get; }
    public EnemyItemDropDefinition? ItemDrops { get; }
    public PrismLaserDefinition? PrismLaser { get; }
    public BattleGiveDefinition? Give { get; }
    public BattleDiscardDefinition? Discard { get; }
    internal BattleDefinition WithSearch(BattleSearchDefinition search) => new(this, search: search);
    public BattleSearchDefinition? Search { get; }
    public bool Poison { get; }
    public bool Contains(MapPosition position) => position.X < Width && position.Y < Height;
}

// Null context means unknown; a present context with no Ally is the source FF sentinel.
public sealed record BattleDeathCredit(ActorRef? Ally);

public sealed class EngineBattleState
{
    internal EngineBattleState(BattleDefinition definition, IEnumerable<BattleActorState> actors,
        uint mainSeed, uint thinkingSeed, int round, IEnumerable<TurnOrderEntry<ActorRef>> queue, int cursor, uint? gold,
        NewBattleStartPolicy? startPolicy = null, BattleRegionState? regions = null, ActorRef? firstAlly = null,
        BattleItemCustody? itemCustody = null, BattleDeathCredit? deathCredit = null, BattleVisualState? visuals = null)
    {
        Visuals = visuals; Definition = definition; Actors = Array.AsReadOnly(actors.ToArray());
        MainSeed = mainSeed; ThinkingSeed = thinkingSeed; Round = round;
        Queue = Array.AsReadOnly(queue.ToArray()); Cursor = cursor;
        Gold = gold; StartPolicy = startPolicy; Regions = regions; DeathCredit = deathCredit ?? (firstAlly is { } ally ? new(ally) : null); ItemCustody = itemCustody;
    }
    public BattleDefinition Definition { get; }
    public IReadOnlyList<BattleActorState> Actors { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public int Round { get; }
    public int Cursor { get; }
    public uint? Gold { get; }
    public BattleItemCustody? ItemCustody { get; }
    public NewBattleStartPolicy? StartPolicy { get; }
    public BattleRegionState? Regions { get; }
    // Source BATTLESCENE_FIRST_ALLY survives turns without a scene. No start default.
    public ActorRef? FirstAlly => DeathCredit?.Ally;
    public BattleDeathCredit? DeathCredit { get; }
    public BattleVisualState? Visuals { get; }
    public IEnumerable<(ActorRef? Actor, byte AlteredAgility)> TurnOrder => Queue.Select(entry => (entry.Actor, entry.AlteredAgility));
    internal IReadOnlyList<TurnOrderEntry<ActorRef>> Queue { get; }
    public BattleActorState GetActor(ActorRef actor) => Actors.Single(a => a.Actor == actor);
    internal EngineBattleState With(IEnumerable<BattleActorState>? actors = null, uint? mainSeed = null,
        int? round = null, IEnumerable<TurnOrderEntry<ActorRef>>? queue = null, int? cursor = null, uint? gold = null,
        uint? thinkingSeed = null, BattleRegionState? regions = null, ActorRef? firstAlly = null,
        BattleItemCustody? itemCustody = null, BattleDeathCredit? deathCredit = null, BattleVisualState? visuals = null) =>
        new(Definition, actors ?? Actors, mainSeed ?? MainSeed, thinkingSeed ?? ThinkingSeed,
            round ?? Round, queue ?? Queue, cursor ?? Cursor, gold ?? Gold, StartPolicy, regions ?? Regions, firstAlly ?? FirstAlly,
            itemCustody ?? ItemCustody, deathCredit ?? (firstAlly is { } ally ? new(ally) : DeathCredit), visuals ?? Visuals);
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
