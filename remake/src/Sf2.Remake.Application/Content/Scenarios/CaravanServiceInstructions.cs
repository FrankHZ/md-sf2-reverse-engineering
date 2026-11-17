namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record OpenCaravan : StoryInstruction;
// Native Caravan messages use an explicit portrait, independent of the interacting NPC.
public sealed record OpenServicePortrait(int Portrait) : StoryInstruction;
