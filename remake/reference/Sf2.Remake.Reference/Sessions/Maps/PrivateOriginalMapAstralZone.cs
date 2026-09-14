using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapAstralZoneReceipt
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapAstralZoneStage> _stages;

    internal PrivateOriginalMapAstralZoneReceipt(
        OriginalMapAstralZoneDefinition definition,
        PrivateOriginalMapSarahState sarahBefore,
        PrivateOriginalMapSarahState sarahAfter,
        PrivateOriginalMapZone601State zone601Before,
        PrivateOriginalMapZone601State zone601After,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        SarahBefore = sarahBefore ?? throw new ArgumentNullException(nameof(sarahBefore));
        SarahAfter = sarahAfter ?? throw new ArgumentNullException(nameof(sarahAfter));
        Zone601Before = zone601Before ??
            throw new ArgumentNullException(nameof(zone601Before));
        Zone601After = zone601After ?? throw new ArgumentNullException(nameof(zone601After));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        Trigger = definition.Trigger;
        PositionProgramIdentity = definition.PositionProgramIdentity;
        MessengerCompletionFlag603 = definition.MessengerCompletionFlag603;
        RequiredEntity142Flag602 = definition.RequiredEntity142Flag602;
        CompletionFlag260 = definition.CompletionFlag260;
        _textIds = Array.AsReadOnly(definition.TextIds.ToArray());
        _stages = Array.AsReadOnly(definition.Stages.ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapAstralZoneEventIdentity EventIdentity { get; }

    public MapPosition Trigger { get; }

    public string PositionProgramIdentity { get; }

    public int MessengerCompletionFlag603 { get; }

    public bool MessengerCompletionFlag603Set => false;

    public int RequiredEntity142Flag602 { get; }

    public bool RequiredEntity142Flag602Set => true;

    public int CompletionFlag260 { get; }

    public bool CompletionFlag260Set => true;

    public PrivateOriginalMapSarahState SarahBefore { get; }

    public PrivateOriginalMapSarahState SarahAfter { get; }

    public PrivateOriginalMapZone601State Zone601Before { get; }

    public PrivateOriginalMapZone601State Zone601After { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public IReadOnlyList<OriginalMapAstralZoneStage> Stages => _stages;

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapAstralZoneDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            Trigger == definition.Trigger &&
            string.Equals(
                PositionProgramIdentity,
                definition.PositionProgramIdentity,
                StringComparison.Ordinal) &&
            MessengerCompletionFlag603 == definition.MessengerCompletionFlag603 &&
            RequiredEntity142Flag602 == definition.RequiredEntity142Flag602 &&
            CompletionFlag260 == definition.CompletionFlag260 &&
            SarahBefore.Phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared &&
            SarahAfter.Phase ==
                PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned &&
            SarahAfter.ActorSourceRecord == definition.SarahSourceRecord &&
            SarahAfter.LogicalActorId == definition.SarahLogicalActorId &&
            SarahAfter.ActorPosition == definition.SarahDestination &&
            SarahAfter.ActorOpaqueFacing == definition.SarahOpaqueFacing &&
            Zone601Before.Phase ==
                PrivateOriginalMapZone601LifecyclePhase.AmbientWalkingHandoff &&
            Zone601After.Phase ==
                PrivateOriginalMapZone601LifecyclePhase.AstralZoneRepositioned &&
            Zone601After.ActorSourceRecord == definition.Zone601ActorSourceRecord &&
            Zone601After.LogicalActorId == definition.Zone601LogicalActorId &&
            Zone601After.ActorPosition == definition.Zone601ActorDestination &&
            Zone601After.ActorOpaqueFacing == definition.Zone601ActorOpaqueFacing &&
            TextIds.SequenceEqual(definition.TextIds) &&
            Stages.SequenceEqual(definition.Stages);
    }
}
