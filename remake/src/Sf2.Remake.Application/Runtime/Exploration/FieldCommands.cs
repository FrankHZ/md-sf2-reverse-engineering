using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public abstract record FieldCommand(ActorRef Member) : SessionCommand;
public sealed record FieldEquip(ActorRef Member, int Slot, ushort? ExpectedWord) : FieldCommand(Member);
public sealed record FieldGive(ActorRef Member, int Slot, ushort ExpectedWord, ActorRef Target,
    int? ReturnSlot = null, ushort? ExpectedReturnWord = null) : FieldCommand(Member);
public sealed record FieldDrop(ActorRef Member, int Slot, ushort ExpectedWord) : FieldCommand(Member);
public sealed record FieldUseItem(ActorRef Member, int Slot, ushort ExpectedWord, ActorRef? Target = null) : FieldCommand(Member);
public sealed record FieldCast(ActorRef Member, SpellRef Spell, ActorRef? Target = null) : FieldCommand(Member);

internal static class FieldCommands
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command, SessionRules? rules = null)
    {
        try
        {
            bool effective = false;
            var effect = FieldRecoveryEffect.NoEffect;
            bool recovery = command is FieldUseItem or FieldCast;
            var party = recovery ? FieldRecovery.Prepare(definition, current, command, out effect, out effective)
                : FieldInventory.Prepare(definition, current, command);
            if (effect == FieldRecoveryEffect.MapEvent && command is FieldUseItem use &&
                FieldItemEvents.Match(current, (byte)(use.ExpectedWord & 127)) is { } entry)
            {
                List<SessionObservation> observations = [];
                var story = current.Story.Copy(entry.Program, continuation: ProgramContinuation.FieldInput);
                current = ProgramRunner.Commit(current, current.Active, story, observations, "field-item-event", use.Member.Value);
                return ProgramRunner.Run(definition, current, observations, rules ?? Gameplay.RuleCompositions.Sf2());
            }
            if (effect == FieldRecoveryEffect.Egress)
                return FieldEgress.Begin(definition, current, command.Member, party, rules ?? Gameplay.RuleCompositions.Sf2());
            long revision = checked(current.Revision + 1), sequence = checked(current.ObservationSequence + 1);
            var snapshot = new SessionSnapshot(current.SessionId, revision, sequence,
                new ActiveExploration(current.Exploration!.WithParty(party)), current.Story, SessionStopReason.PlayerInput);
            return new(snapshot, [new(sequence, revision, recovery ? effective ? "field-status-recovered" : "field-no-effect" : command switch
            { FieldEquip => "field-equipped", FieldGive => "field-given", FieldDrop => "field-dropped", _ => "field-prepared" }, command.Member)], snapshot.StopReason);
        }
        catch (BattlePolicyFault error) { return ProgramRunner.Failure(current, [], error); }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }
}
