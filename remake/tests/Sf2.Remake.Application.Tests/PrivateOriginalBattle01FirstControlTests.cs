using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01FirstControlTests
{
    [Fact]
    public void EntryConsumesTheExactCurrentCandidateAndAtomicallyInstallsItsRangeAndSupplement()
    {
        var session = RoundSession(); var before = session.PrivateOriginalBattle01!;
        int actor = before.Battle.FirstRound!.FirstCandidate!.Value.CombatantIndex;
        var next = Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(
            session.EnterPrivateOriginalBattle01FirstControl(before, actor)).Snapshot;
        Assert.Same(next, session.PrivateOriginalBattle01); Assert.NotSame(before, next);
        Assert.Equal(Battle01Phase.PlayerMovementSelection, next.Battle.Phase);
        Assert.Equal(actor, next.Battle.FirstControl!.ActorIndex); Assert.True(next.Battle.FirstControl.CandidateWordSupplied);
        Assert.Null(before.Battle.Roster[1].AiBitfield); Assert.Equal((ushort?)0, next.Battle.Roster[1].AiBitfield);
        Assert.Null(next.Battle.Roster[0].AiBitfield); Assert.Null(next.Battle.Roster[2].AiBitfield);
        Assert.Equal(10, next.Battle.FirstControl.Movement.Range.Budget);
        Assert.True(next.Battle.FirstControl.Movement.Range.CanStopAt(new(1, 2)));
        Assert.Same(before.Preparation, next.Preparation); Assert.Same(before.SourceLocomotion, next.SourceLocomotion);
        Assert.Same(before.SourceSnapshot, next.SourceSnapshot); Assert.Same(before.SourceBridge, next.SourceBridge);
        Assert.Same(before.Battle.FirstRound, next.Battle.FirstRound); Assert.Equal(0xA4991234u, next.Battle.RandomSeedImage);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01FirstControlRejected>(
            session.EnterPrivateOriginalBattle01FirstControl(next, actor)).Diagnostic.Field);
        Assert.Same(next, session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("copy")]
    public void MissingForeignAndCopiedSnapshotsCannotEnterControl(string scenario)
    {
        var session = RoundSession(); var before = session.PrivateOriginalBattle01!;
        var expected = scenario switch { "missing" => null, "foreign" => RoundSession().PrivateOriginalBattle01,
            _ => new(before.Preparation, before.Battle, before.SourceLocomotion, before.SourceBridge) };
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01FirstControlRejected>(
            session.EnterPrivateOriginalBattle01FirstControl(expected, 1)).Diagnostic.Field);
        Assert.Same(before, session.PrivateOriginalBattle01); Assert.Null(before.Battle.Roster[1].AiBitfield);
        Assert.Equal("actor", Assert.IsType<PrivateOriginalBattle01FirstControlRejected>(
            session.EnterPrivateOriginalBattle01FirstControl(before, 0)).Diagnostic.Field);
        Assert.Same(before, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void KnownAiWordRemainsUnavailableAndCannotOpenMovementOrLoseTheCandidate()
    {
        var session = RoundSession(); var before = session.PrivateOriginalBattle01!; var roster = before.Battle.Roster.ToArray();
        var actor = roster[1]; roster[1] = Internal<Battle01Combatant>(actor.Deployment, actor.Stats, actor.ClassId,
            actor.EnemySource, (ushort?)4, actor.Position);
        var input = ReplaceBattle(session, CopyRound(before.Battle, roster));
        var result = Assert.IsType<PrivateOriginalBattle01FirstControlUnavailable>(
            session.EnterPrivateOriginalBattle01FirstControl(input, 1));
        Assert.Equal(1, result.Decision.ActorIndex); Assert.Equal(Battle01FirstControlAvailability.AiControlled, result.Decision.Availability);
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal((ushort?)4, input.Battle.Roster[1].AiBitfield);
        Assert.Equal(0, input.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal(0xA4991234u, input.Battle.RandomSeedImage);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(
            session.SelectPrivateOriginalBattle01PlayerDestination(input, 1, new(1, 2))).Diagnostic.Field);
        Assert.Same(input, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void FailedMovementProjectionCannotCommitTheMissingWordOrAnyOtherControlState()
    {
        var session = RoundSession(); var before = session.PrivateOriginalBattle01!;
        var terrain = before.Battle.Terrain.ToArray(); terrain[1 * 48 + 2] = 16;
        var input = ReplaceBattle(session, CopyRound(before.Battle, terrain: terrain));
        Assert.Equal("terrain", Assert.IsType<PrivateOriginalBattle01FirstControlRejected>(
            session.EnterPrivateOriginalBattle01FirstControl(input, 1)).Diagnostic.Field);
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Null(input.Battle.Roster[1].AiBitfield);
        Assert.Null(input.Battle.FirstControl); Assert.Equal(before.Battle.RandomSeedImage, input.Battle.RandomSeedImage);
        Assert.Same(before.Battle.FirstRound, input.Battle.FirstRound);
    }

    internal static GameSession RoundSession()
    {
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        for (int y = 0; y < 20; y++) for (int x = 0; x < 16; x++) terrain[y * 48 + x] = 1;
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        var prepared = PrivateOriginalBattle01InitializationTests.Prepare(session,
            PrivateOriginalBattle01StartupTests.Definition(terrain: terrain));
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01FirstRound(initialized));
        return session;
    }
    internal static T Internal<T>(params object?[] arguments) => (T)Activator.CreateInstance(typeof(T),
        BindingFlags.Instance | BindingFlags.NonPublic, null, arguments, null)!;
    private static Battle01InitializedState CopyRound(Battle01InitializedState source,
        Battle01Combatant[]? roster = null, byte[]? terrain = null)
    {
        var initial = Internal<Battle01InitializedState>(roster ?? source.Roster.ToArray(), source.Regions.ToArray(),
            terrain ?? source.Terrain.ToArray(), source.Occupancy.ToArray(), 0x1234u);
        return Internal<Battle01InitializedState>(initial, initial.Roster.ToArray(), source.RegionFlags90Through105.ToArray(),
            source.NewlyTestedRegionMask, source.RandomSeedImage, source.FirstRound);
    }
    private static PrivateOriginalBattle01SessionSnapshot ReplaceBattle(GameSession session, Battle01InitializedState state)
    {
        var source = session.PrivateOriginalBattle01!;
        var input = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, state, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, input);
        return input;
    }
}
