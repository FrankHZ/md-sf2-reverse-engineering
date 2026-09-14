namespace Sf2.Remake.Domain.Maps;

public sealed record MapPartyFlagLayout(int MemberCount, int JoinedStart, int ActiveStart, int Capacity);
public sealed record MapPartyLists(IReadOnlyList<int> Joined, IReadOnlyList<int> Active, IReadOnlyList<int> Reserve);
public sealed record MapPartyJoinResult(IReadOnlyList<int> Flags, MapPartyLists Lists);

public static class MapPartyMembership
{
    public static MapPartyLists Rebuild(IReadOnlyCollection<int> flags, MapPartyFlagLayout layout)
    {
        var joined = Enumerable.Range(0, layout.MemberCount).Where(member => flags.Contains(layout.JoinedStart + member)).ToArray();
        return new(Array.AsReadOnly(joined),
            Array.AsReadOnly(joined.Where(member => flags.Contains(layout.ActiveStart + member)).ToArray()),
            Array.AsReadOnly(joined.Where(member => !flags.Contains(layout.ActiveStart + member)).ToArray()));
    }

    public static MapPartyJoinResult Join(IReadOnlyCollection<int> flags, MapPartyFlagLayout layout, int member)
    {
        if (member < 0 || member >= layout.MemberCount) throw new ArgumentOutOfRangeException(nameof(member));
        var changed = flags.Append(layout.JoinedStart + member).Distinct().Order().ToList();
        var lists = Rebuild(changed, layout);
        if (lists.Active.Count < layout.Capacity && !changed.Contains(layout.ActiveStart + member))
            changed.Add(layout.ActiveStart + member);
        // JoinForce does not rebuild the counted prefixes after its conditional active-flag write.
        return new(Array.AsReadOnly(changed.Order().ToArray()), lists);
    }
}
