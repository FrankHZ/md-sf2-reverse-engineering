using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public enum OriginalMapMiddleTowerGuardPreset
{
    ControlledPostAstralAndLocal256Clear,
}

/// <summary>
/// Compiled controlled result from the accepted castle-unlock H2 contract.
/// Canonical input binds the actor/event table, without supplying the handler/program bodies.
/// </summary>
public sealed class OriginalMapMiddleTowerGuardDefinition
{
    public OriginalMapMiddleTowerGuardDefinition(OriginalMapEntityDefinition actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.Identity != new OriginalMapEntityRecordIdentity("ms_map21_Entities", 1) ||
            actor.RawX != 5 || actor.RawY != 16 || actor.OpaqueFacing != 3 || actor.MapSprite != 206 ||
            actor.Kind != OriginalMapEntityRecordKind.Fixed || !actor.OpaqueTail.SequenceEqual(new byte[] { 0, 4, 0x60, 0xCE }))
        {
            throw new ArgumentException("The middle-tower guard requires the exact Map 21 actor 128.", nameof(actor));
        }
        Actor = actor;
    }

    public OriginalMapEntityDefinition Actor { get; }
    public MapId Map { get; } = new("map21");
    public OriginalMapMiddleTowerGuardPreset Preset => OriginalMapMiddleTowerGuardPreset.ControlledPostAstralAndLocal256Clear;
    public MapPosition InteractionPosition { get; } = new(4, 16);
    public byte InteractionOpaqueFacing => 0;
    public MapPosition AcceptedActorEndpoint { get; } = new(6, 16);
    public string HandlerIdentity => "Map21_EntityEvent0";
    public int HandlerAddress => 343726;
    public int TextId => 579;
    public string ProgramIdentity => "cs_53EF4";
    public int ProgramAddress => 343796;
    public string ProgramControlEffectSha256 => "08697EBB15C35F4AF661D38A32899E52EF13D09D53A6C52736DAD751BF6D3706";
    public int HandlerCompletionFlag => 256;
    public int ProgramStoryFlag => 1;
    public int ProgramCompletionFlag => 401;
    // setFacing 135,DOWN is retained as an Unknown effect, not assigned to the player or guard.
}
