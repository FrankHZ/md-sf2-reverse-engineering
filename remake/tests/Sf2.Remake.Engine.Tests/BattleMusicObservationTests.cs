using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMusicObservationTests
{
    [Fact]
    public void NativeBattleHistoryIsDetachedAndCannotRewriteKnownMusic()
    {
        var (definition, current) = BattleSaveFixture.Create();
        var old = current.Story.Music!;
        current = current.WithStory(current.Story.Copy(null, music: old with { HistoryBound = false }));
        string[] history = ["previous", "field"];
        var result = BattleMusicObservation.Submit(definition, current, RuleCompositions.Sf2(),
            new(old.Map, old.Area, new(old.Cue, history)));
        Assert.Null(result.Failure);
        Assert.Same(current.Active, result.Snapshot.Active);
        Assert.Equal(current.Revision + 1, result.Snapshot.Revision);
        Assert.True(result.Snapshot.Story.Music!.HistoryBound);
        history[0] = "bad";
        Assert.Equal("previous", result.Snapshot.Story.Music.Previous[0]);
        var unchanged = BattleMusicObservation.Submit(definition, result.Snapshot, RuleCompositions.Sf2(),
            new(old.Map, old.Area, new(old.Cue, ["previous", "field"])));
        Assert.Same(result.Snapshot, unchanged.Snapshot);
        var later = BattleMusicObservation.Submit(definition, result.Snapshot, RuleCompositions.Sf2(),
            new(old.Map, old.Area, new(old.Cue, ["field", "previous", "field"])));
        Assert.Null(later.Failure);
        Assert.Equal(result.Snapshot.Story.Music.Generation, later.Snapshot.Story.Music!.Generation);
        Assert.Same(result.Snapshot.Active, later.Snapshot.Active);
        Assert.Equal(new[] { "field", "previous", "field" }, later.Snapshot.Story.Music.Previous);
        var wrong = BattleMusicObservation.Submit(definition, result.Snapshot, RuleCompositions.Sf2(),
            new(old.Map, old.Area, new("previous", [old.Cue])));
        Assert.Equal("save-battle-music-disagreement", wrong.Failure?.Code);
        Assert.Same(result.Snapshot, wrong.Snapshot);
    }

    [Theory]
    [InlineData("map")]
    [InlineData("area")]
    [InlineData("resource")]
    [InlineData("finite")]
    [InlineData("completed")]
    [InlineData("busy")]
    public void InvalidOrBusyObservationDoesNotChangeGameplay(string variant)
    {
        var (definition, current) = BattleSaveFixture.Create();
        var old = current.Story.Music!;
        if (variant == "finite") current = current.WithStory(current.Story.Copy(null, music: old with { EndStep = 20 }));
        if (variant == "completed") current = current.WithStory(current.Story.Copy(null, music: old with { ActualDone = true }));
        if (variant == "busy") current = new(current.SessionId, current.Revision, current.ObservationSequence,
            current.Active, current.Story, SessionStopReason.SimulationWait);
        var result = BattleMusicObservation.Submit(definition, current, RuleCompositions.Sf2(),
            new(variant == "map" ? new("wrong") : old.Map, variant == "area" ? 999 : old.Area,
                new(variant == "resource" ? "missing" : old.Cue, old.Previous)));
        Assert.NotNull(result.Failure);
        Assert.Same(current, result.Snapshot);
        Assert.Empty(result.Observations);
    }
}
