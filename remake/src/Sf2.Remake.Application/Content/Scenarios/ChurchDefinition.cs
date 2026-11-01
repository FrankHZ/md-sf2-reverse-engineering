namespace Sf2.Remake.Application.Content.Scenarios;

// Immutable source operands; caller identity and map/story guards stay in Content programs.
public sealed record ChurchDefinition(ushort RaisePerLevel, ushort PromotedExtra, ushort RaisedHpCap,
    ushort PoisonCost, ushort StunCost);
public sealed record OpenChurch(ChurchDefinition Definition) : StoryInstruction;
