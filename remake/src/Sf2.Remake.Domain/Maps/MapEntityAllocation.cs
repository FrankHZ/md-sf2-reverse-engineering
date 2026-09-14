using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record MapFollowerSpawn(int Character, int Sprite);
public sealed record MapEntitySlot(int Slot, int Character, int Sprite, int? SourceRecord, int? FollowerOrder);
public sealed record MapEntitySourceBinding(int SourceRecord, int Slot, bool ReusedFollower);

public sealed class MapEntityAllocation
{
    internal MapEntityAllocation(IEnumerable<MapEntitySlot> slots, IDictionary<int, int> aliases,
        IEnumerable<MapEntitySourceBinding> sources)
    {
        Slots = Array.AsReadOnly(slots.OrderBy(slot => slot.Slot).ToArray());
        Aliases = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>(aliases));
        Sources = Array.AsReadOnly(sources.ToArray());
    }
    public IReadOnlyList<MapEntitySlot> Slots { get; }
    public IReadOnlyDictionary<int, int> Aliases { get; }
    public IReadOnlyList<MapEntitySourceBinding> Sources { get; }
}

public static class MapEntityAllocator
{
    public static MapEntityAllocation Allocate(IReadOnlyList<int> sourceSprites,
        IReadOnlyList<MapFollowerSpawn> followers, int allyCount, int nonAllyStart, int playerSprite)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(allyCount);
        List<MapEntitySlot> slots = [new(0, 0, playerSprite, null, null)];
        Dictionary<int, int> aliases = [];
        List<MapEntitySourceBinding> sources = [];
        for (int index = 0; index < followers.Count; index++)
        {
            var follower = followers[index];
            int slot = slots.Count;
            slots.Add(new(slot, follower.Character, follower.Sprite, null, index));
            aliases[follower.Character] = slot;
        }
        int nonAlly = nonAllyStart;
        for (int index = 0; index < sourceSprites.Count; index++)
        {
            int sprite = sourceSprites[index];
            bool ally = sprite < allyCount;
            if (ally && aliases.TryGetValue(sprite, out int existing))
            {
                // The source consumes the record tail but retains the follower's position/script.
                sources.Add(new(index, existing, true));
                continue;
            }
            int character = ally ? sprite : nonAlly++;
            int slot = slots.Count;
            slots.Add(new(slot, character, sprite, index, null));
            aliases[character] = slot;
            sources.Add(new(index, slot, false));
        }
        aliases[0] = 0;
        return new(slots, aliases, sources);
    }
}
