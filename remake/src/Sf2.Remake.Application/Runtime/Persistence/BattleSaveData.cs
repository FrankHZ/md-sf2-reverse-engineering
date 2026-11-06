using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

public sealed record SavedBattleRoute(string Map, int Ordinal);
public sealed record SavedBattlePlayer(string Entity, ushort Speed, IReadOnlyList<BattleActorStartInput> RetainedAllies);
public sealed record SavedBattleMusic(string Map, int Area, string Cue, IReadOnlyList<string> Previous);
public sealed record SavedBattleStory(IReadOnlyList<int> Flags, IReadOnlyDictionary<string, string> Names,
    short StepCounter, string EgressMap, int TextCursor, ExplorationTextSettings TextSettings,
    ExplorationDisplay Display, LogicalTextWindow LogicalText, LogicalView LogicalView,
    EntityRef? Speaker, bool? EntityServices, bool WindowFixPending, FieldReturnAnchor? SaveAnchor,
    SavedBattleMusic Music, SavedBattlePlayer Player);
public sealed record BattleSaveData(SaveCompatibility Compatibility, int BattleRecipeRevision,
    SavedBattleRoute Route, BattleContinuationData Battle, SavedBattleStory Story);
public sealed record BattleSaveLocation(string Encounter, int Round, string Actor);
public sealed record BattleSaveAvailability(Guid SessionId, long Revision, BattleSaveLocation? Location, string? Error)
{
    public bool Allowed => Error is null && Location is not null;
}
public sealed record BattleSaveCapture(BattleSaveData? Data, string? Error);
