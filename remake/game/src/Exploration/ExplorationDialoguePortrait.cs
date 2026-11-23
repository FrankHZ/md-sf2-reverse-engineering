using System.Globalization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal static class ExplorationDialoguePortrait
{
    // The admitted missing-portrait pose is shared by allies and NPCs.
    private const int Pose1 = 59;

    internal static (int Portrait, byte Flags)? ForSpecialPose(
        ExplorationVisuals visuals, ExplorationState? world, StoryState story)
    {
        if (story.PortraitWindow is not ClosedPortraitWindow ||
            story.TextWindow is not OpenTextWindow { Speaker: { } speaker, SpeakerFlags: not 255 } text ||
            world?.Population?.AllySprites is not { } appearances ||
            !world.TryResolveEntity(speaker, out var actor) || actor.Entity != speaker || actor.Sprite != Pose1 ||
            !visuals.Sprites.TryGetValue(Pose1, out var pose) || pose.Portrait is not null ||
            !speaker.Value.StartsWith("entity-", StringComparison.Ordinal) ||
            !int.TryParse(speaker.Value.AsSpan("entity-".Length), NumberStyles.None, CultureInfo.InvariantCulture, out int character))
            return null;

        var appearance = appearances.FirstOrDefault(row => row.Character == character);
        if (appearance is null) return null;
        var progress = world.Party.Actors.FirstOrDefault(row => row.Actor.Value == $"ally-{character}")?.Progress;
        int? sprite = appearance.JoinedFlag is { } joined && !story.Flags.Contains(joined)
            ? appearance.UnjoinedSprite : appearance.ForClass(progress?.ClassId);
        return sprite is { } id && visuals.Sprites.TryGetValue(id, out var normal) && normal.Portrait is { } portrait
            ? (portrait, text.SpeakerFlags) : null;
    }
}
