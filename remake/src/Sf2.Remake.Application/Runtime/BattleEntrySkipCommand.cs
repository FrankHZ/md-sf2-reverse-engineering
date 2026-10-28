using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.Application.Runtime;

public abstract record BattleEntrySkipCommand : SessionCommand;
public sealed record SkipBattleEntry : BattleEntrySkipCommand;
public sealed record AdvanceBattleEntrySkip : BattleEntrySkipCommand;
public sealed record CancelBattleEntrySkip : BattleEntrySkipCommand;
// The adapter validates/mounts the cue before suppressing its voluntary delivery latency.
public sealed record CompleteSkippedEntryPresentation(WaitToken Wait, PresentationCueKind Kind) : BattleEntrySkipCommand;
public sealed record CompleteSkippedEntryMusic(long Generation, string Cue) : BattleEntrySkipCommand;
