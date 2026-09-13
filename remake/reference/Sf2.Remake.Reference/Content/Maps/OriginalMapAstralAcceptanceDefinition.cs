using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

/// <summary>
/// Controlled result compiled from the accepted map3-castle-battle-unlock H2 contract.
/// Canonical admission binds the actor/event; it does not supply or execute the script bodies.
/// </summary>
public sealed class OriginalMapAstralAcceptanceDefinition
{
    public OriginalMapAstralAcceptanceDefinition(OriginalMapEntityDefinition actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.Identity != new OriginalMapEntityRecordIdentity("ms_map19_Entities", 13) ||
            actor.Position != new MapPosition(16, 5) || actor.OpaqueFacing != 3 ||
            actor.Kind != OriginalMapEntityRecordKind.Fixed)
        {
            throw new ArgumentException("Astral requires the admitted Map 19 actor 140.", nameof(actor));
        }

        Actor = actor;
    }

    public OriginalMapEntityDefinition Actor { get; }
    public MapId Map { get; } = new("map19");
    public MapPosition InteractionPosition { get; } = new(16, 6);
    public byte InteractionOpaqueFacing => 1;
    public MapPosition AcceptedActorEndpoint { get; } = new(63, 63);
    public byte AcceptedActorOpaqueFacing => 2;
    public string HandlerIdentity => "Map19_EntityEvent12";
    public string PromptProgramIdentity => "cs_52F0C";
    public string AcceptanceProgramIdentity => "cs_52F40";
    public int HandlerCompletionFlag => 607;
    public int ProgramCompletionFlag => 608;
}
