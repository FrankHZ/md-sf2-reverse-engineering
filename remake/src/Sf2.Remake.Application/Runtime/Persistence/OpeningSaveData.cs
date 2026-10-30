using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

// Progress only. Definitions, interpreter continuations, resources and paths never cross this boundary.
public sealed record SaveCompatibility(string Profile, string Package, int GameplayRevision,
    string Rules, int RuleRevision, int RecipeRevision, string Repository, string Commit, string Rom);
public sealed record SavedAlly(string Actor, byte Class, ushort Hp, byte Mp, byte Exp, ushort Kills,
    ushort Defeats, ushort Status, ushort Activation, bool Initialized, bool Absent,
    byte Level, ushort MaxHp, byte MaxMp, byte BaseAttack, byte Defense, byte Agility,
    IReadOnlyList<ushort> Items, IReadOnlyList<byte> Spells);
public sealed record SavedDeal(byte Item, byte Count);
public sealed record SavedMusic(string Cue, IReadOnlyList<string> Previous);
// Native playback knowledge, like a presentation completion; not a request to change the music.
public sealed record ObserveFieldMusic(MapId Map, int Area, SavedMusic Music) : SessionCommand;
public sealed record OpeningSaveData(SaveCompatibility Compatibility, string Recipe,
    string Map, int X, int Y, byte Facing, string Encounter,
    IReadOnlyList<SavedAlly> Allies, IReadOnlyList<string> ActiveAllies, uint MainSeed, uint ThinkingSeed,
    uint Gold, IReadOnlyList<byte> OnceFlags, IReadOnlyList<SavedDeal> Deals, IReadOnlyList<int> Flags,
    short StepCounter, string EgressMap, int TextCursor, ExplorationTextSettings TextSettings,
    byte FadePeriod, SavedMusic Music, byte Difficulty, bool AllyAutoBattle, bool OpponentControl);
public sealed record SaveLocation(string Recipe, FieldReturnAnchor Anchor, string Description);
public sealed record SaveAvailability(Guid SessionId, long Revision, SaveLocation? Location, string? Error)
{
    public bool Allowed => Error is null && Location is not null;
}
public sealed record SaveCapture(OpeningSaveData? Data, string? Error);

internal interface IOpeningRestorePolicy
{
    void ValidateRegisters(OpeningSaveData data);
    bool CanDiscardEntityProgram(ExplorationState world, ExplorationEntity entity);
    SaveLocation Select(ScenarioDefinition definition, MapId map, IReadOnlyList<int> flags, FieldReturnAnchor? anchor);
    ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, out IReadOnlyList<int> enteredFlags);
}
