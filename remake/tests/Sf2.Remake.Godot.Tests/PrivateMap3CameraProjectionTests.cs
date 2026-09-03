using System.Reflection;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateMap3CameraProjectionTests
{
    [Fact]
    public void ControlledStartUsesTheExistingCenterAndClampsAtTheTopEdge()
    {
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(new MapPosition(56, 3));

        PrivateMap3CameraProjection camera = PrivateMap3CameraProjection.Create(snapshot);

        Assert.Equal(snapshot.Map, camera.Map);
        Assert.Equal(56 * 384, camera.FocusXUnits);
        Assert.Equal(3 * 384, camera.FocusYUnits);
        Assert.Equal(50 * 24, camera.TopLeftPixelX);
        Assert.Equal(0, camera.TopLeftPixelY);
        Assert.Equal(50, camera.OriginX);
        Assert.Equal(0, camera.OriginY);
        Assert.Equal(144, camera.PlayerPixelX);
        Assert.Equal(72, camera.PlayerPixelY);
        Assert.False(camera.RequiresTrailingColumn);
        Assert.False(camera.RequiresTrailingRow);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(63, 63, 52 * 24, 57 * 24, 11 * 24, 6 * 24)]
    public void FollowClampsToTheAdmittedWorkingLayout(
        int playerX,
        int playerY,
        int expectedLeft,
        int expectedTop,
        int expectedPlayerX,
        int expectedPlayerY)
    {
        PrivateMap3CameraProjection camera = PrivateMap3CameraProjection.Create(
            Snapshot(new MapPosition(playerX, playerY)));

        Assert.Equal(expectedLeft, camera.TopLeftPixelX);
        Assert.Equal(expectedTop, camera.TopLeftPixelY);
        Assert.Equal(expectedPlayerX, camera.PlayerPixelX);
        Assert.Equal(expectedPlayerY, camera.PlayerPixelY);
    }

    [Fact]
    public void AcceptedLocomotionMovesTheCameraTwoLogicalPixelsPerTransition()
    {
        MapPosition source = new(56, 3);
        MapPosition destination = new(57, 3);
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(destination);
        PrivateOriginalMapPlayerLocomotionSnapshot animation = BeginLocomotion(
            source,
            destination,
            ExplorationDirection.East,
            OriginalMapTraversalOutcome.Moved);
        PrivateMap3CameraProjection camera =
            PrivateMap3CameraProjection.Create(snapshot, animation);

        Assert.Equal(50 * 24, camera.TopLeftPixelX);
        Assert.Equal(144, camera.PlayerPixelX);
        for (int tick = 2;
            tick <= PrivateOriginalMapPlayerLocomotionSnapshot.SuccessfulMovementTickCount;
            tick++)
        {
            PrivateMap3CameraProjection previous = camera;
            animation = Advance(animation);
            camera = PrivateMap3CameraProjection.Create(snapshot, animation);

            Assert.Equal(previous.TopLeftPixelX + 2, camera.TopLeftPixelX);
            Assert.Equal(previous.TopLeftPixelY, camera.TopLeftPixelY);
            Assert.Equal(144, camera.PlayerPixelX);
            Assert.Equal(72, camera.PlayerPixelY);
            Assert.Equal(tick, animation.Tick);
        }

        Assert.Equal(51 * 24, camera.TopLeftPixelX);
        Assert.Equal(51, camera.OriginX);
        Assert.Equal(0, camera.OriginPixelOffsetX);
    }

    [Fact]
    public void BlockedAttemptRetainsTheCameraOrigin()
    {
        MapPosition source = new(56, 3);
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(source);
        PrivateOriginalMapPlayerLocomotionSnapshot animation = BeginLocomotion(
            source,
            source,
            ExplorationDirection.North,
            OriginalMapTraversalOutcome.BlockedByCollision);

        Assert.False(animation.IsMoving);
        Assert.Equal(
            PrivateMap3CameraProjection.Create(snapshot),
            PrivateMap3CameraProjection.Create(snapshot, animation));
    }

    [Fact]
    public void LocomotionForAnotherAuthoritativeDestinationIsRejected()
    {
        MapPosition source = new(56, 3);
        PrivateOriginalMapPlayerLocomotionSnapshot animation = BeginLocomotion(
            source,
            new MapPosition(57, 3),
            ExplorationDirection.East,
            OriginalMapTraversalOutcome.Moved);

        Assert.Throws<ArgumentException>(() => PrivateMap3CameraProjection.Create(
            Snapshot(source),
            animation));
    }

    private static PrivateOriginalMapSessionSnapshot Snapshot(MapPosition position) =>
        PrivateOriginalMapBaseViewportTests.Snapshot(
            [new ushort[9]],
            new ushort[WorkingMapLayout.WordCount],
            playerPosition: position);

    internal static PrivateOriginalMapPlayerLocomotionSnapshot BeginLocomotion(
        MapPosition source,
        MapPosition destination,
        ExplorationDirection direction,
        OriginalMapTraversalOutcome outcome)
    {
        Type type = typeof(PrivateOriginalMapPlayerLocomotionSnapshot);
        MethodInfo controlledAdmission = type.GetMethod(
            "ControlledAdmission",
            BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Controlled locomotion admission seam is missing.");
        MethodInfo begin = type.GetMethod(
            "Begin",
            BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Locomotion begin seam is missing.");
        PrivateOriginalMapPlayerLocomotionSnapshot current =
            (PrivateOriginalMapPlayerLocomotionSnapshot)controlledAdmission.Invoke(
                null,
                [source])!;
        OriginalMapTraversalResult traversal = new(
            source,
            destination,
            direction,
            outcome,
            sourceWord: 0,
            destinationWord: outcome == OriginalMapTraversalOutcome.Moved ? (ushort)0 : null);
        return (PrivateOriginalMapPlayerLocomotionSnapshot)begin.Invoke(
            null,
            [current, direction, source, traversal])!;
    }

    internal static PrivateOriginalMapPlayerLocomotionSnapshot Advance(
        PrivateOriginalMapPlayerLocomotionSnapshot animation)
    {
        MethodInfo advance = typeof(PrivateOriginalMapPlayerLocomotionSnapshot).GetMethod(
            "Advance",
            BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Locomotion advance seam is missing.");
        return (PrivateOriginalMapPlayerLocomotionSnapshot)advance.Invoke(animation, null)!;
    }
}
