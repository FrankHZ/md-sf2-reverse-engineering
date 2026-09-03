using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateMap3CameraProjection
{
    internal const int CenterColumn = 6;
    internal const int CenterRow = 3;
    internal const int SourceUnitsPerLogicalPixel =
        PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile /
        PrivateOriginalMapBaseViewProjection.BlockPixelSize;

    private PrivateMap3CameraProjection(
        MapId map,
        int focusXUnits,
        int focusYUnits,
        int topLeftPixelX,
        int topLeftPixelY)
    {
        Map = map;
        FocusXUnits = focusXUnits;
        FocusYUnits = focusYUnits;
        TopLeftPixelX = topLeftPixelX;
        TopLeftPixelY = topLeftPixelY;
    }

    internal MapId Map { get; }

    internal int FocusXUnits { get; }

    internal int FocusYUnits { get; }

    internal int FocusPixelX => FocusXUnits / SourceUnitsPerLogicalPixel;

    internal int FocusPixelY => FocusYUnits / SourceUnitsPerLogicalPixel;

    internal int TopLeftPixelX { get; }

    internal int TopLeftPixelY { get; }

    internal int OriginX => TopLeftPixelX /
        PrivateOriginalMapBaseViewProjection.BlockPixelSize;

    internal int OriginY => TopLeftPixelY /
        PrivateOriginalMapBaseViewProjection.BlockPixelSize;

    internal int OriginPixelOffsetX => TopLeftPixelX %
        PrivateOriginalMapBaseViewProjection.BlockPixelSize;

    internal int OriginPixelOffsetY => TopLeftPixelY %
        PrivateOriginalMapBaseViewProjection.BlockPixelSize;

    internal int PlayerPixelX => FocusPixelX - TopLeftPixelX;

    internal int PlayerPixelY => FocusPixelY - TopLeftPixelY;

    internal bool RequiresTrailingColumn => OriginPixelOffsetX != 0;

    internal bool RequiresTrailingRow => OriginPixelOffsetY != 0;

    internal static PrivateMap3CameraProjection Create(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapPlayerLocomotionSnapshot? locomotion = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        MapPosition source = snapshot.PlayerPosition;
        int offsetXUnits = 0;
        int offsetYUnits = 0;
        if (locomotion is not null)
        {
            if (locomotion.DestinationPosition != snapshot.PlayerPosition)
            {
                throw new ArgumentException(
                    "The camera locomotion destination must match the authoritative private-map position.",
                    nameof(locomotion));
            }

            source = locomotion.SourcePosition;
            offsetXUnits = locomotion.OffsetXUnits;
            offsetYUnits = locomotion.OffsetYUnits;
        }

        ValidatePosition(source, nameof(locomotion));
        int focusXUnits = checked(
            (source.X * PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile) +
            offsetXUnits);
        int focusYUnits = checked(
            (source.Y * PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile) +
            offsetYUnits);
        int maximumFocusXUnits = checked(
            (WorkingMapLayout.ColumnCount - 1) *
            PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile);
        int maximumFocusYUnits = checked(
            (WorkingMapLayout.RowCount - 1) *
            PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile);
        if (focusXUnits < 0 || focusXUnits > maximumFocusXUnits ||
            focusYUnits < 0 || focusYUnits > maximumFocusYUnits ||
            focusXUnits % SourceUnitsPerLogicalPixel != 0 ||
            focusYUnits % SourceUnitsPerLogicalPixel != 0)
        {
            throw new ArgumentException(
                "The camera focus must remain on the admitted map at an exact logical-pixel boundary.",
                nameof(locomotion));
        }

        int focusPixelX = focusXUnits / SourceUnitsPerLogicalPixel;
        int focusPixelY = focusYUnits / SourceUnitsPerLogicalPixel;
        int maximumTopLeftPixelX = checked(
            (WorkingMapLayout.ColumnCount -
                PrivateOriginalMapBaseViewProjection.ColumnCount) *
            PrivateOriginalMapBaseViewProjection.BlockPixelSize);
        int maximumTopLeftPixelY = checked(
            (WorkingMapLayout.RowCount -
                PrivateOriginalMapBaseViewProjection.RowCount) *
            PrivateOriginalMapBaseViewProjection.BlockPixelSize);
        int topLeftPixelX = Math.Clamp(
            focusPixelX - checked(
                CenterColumn * PrivateOriginalMapBaseViewProjection.BlockPixelSize),
            0,
            maximumTopLeftPixelX);
        int topLeftPixelY = Math.Clamp(
            focusPixelY - checked(
                CenterRow * PrivateOriginalMapBaseViewProjection.BlockPixelSize),
            0,
            maximumTopLeftPixelY);
        return new PrivateMap3CameraProjection(
            snapshot.Map,
            focusXUnits,
            focusYUnits,
            topLeftPixelX,
            topLeftPixelY);
    }

    private static void ValidatePosition(MapPosition position, string parameterName)
    {
        if (position.X < 0 || position.X >= WorkingMapLayout.ColumnCount ||
            position.Y < 0 || position.Y >= WorkingMapLayout.RowCount)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "The camera source position must remain inside the admitted working layout.");
        }
    }
}
