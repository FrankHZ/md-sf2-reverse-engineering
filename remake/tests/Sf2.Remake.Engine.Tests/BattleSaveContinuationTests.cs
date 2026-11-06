using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveContinuationTests
{
    [Theory]
    [InlineData(0x1234ABCDu, 0x80ABCDEFu)]
    [InlineData(0u, 0u)]
    [InlineData(0xFFFF0001u, 0xFF000001u)]
    public void PartlyConsumedBattle01QueueContinuesThroughAiAndNextRound(uint main, uint thinking)
    {
        var (definition, initial) = BattleSaveFixture.Create(main, thinking);
        // First commit a real action and automatic standby/movement before the captured boundary.
        var live = BattleSaveFixture.Stay(initial);
        Assert.NotEqual(initial.Battle.Cursor, live.Battle.Cursor);
        var data = BattleSaveFixture.Capture(definition, live);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, data, RuleCompositions.Sf2()));
        Assert.Empty(restored.Result.Observations);
        Assert.NotEqual(live.SessionId, restored.Session.Current.SessionId);
        Assert.Equal(0, restored.Session.Current.Revision);
        BattleSaveFixture.EqualBattle(live.Battle, restored.Session.Current.Battle);
        var controlNext = BattleSaveFixture.Stay(live);
        var resumedNext = BattleSaveFixture.Stay(restored.Session.Current);
        BattleSaveFixture.EqualBattle(controlNext.Battle, resumedNext.Battle);
        Assert.True(controlNext.Battle.Round > live.Battle.Round);
        Assert.NotEqual(live.Battle.MainSeed, controlNext.Battle.MainSeed);
        Assert.Equal(live.Story.Flags, resumedNext.Story.Flags);
        Assert.Equal(live.Story.FieldPlayer!.RetainedAllies![0].Hp, resumedNext.Story.FieldPlayer!.RetainedAllies![0].Hp);
    }
}
