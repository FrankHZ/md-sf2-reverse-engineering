namespace Sf2.Remake.Domain.Maps;

public sealed record MapPartyFlagLayout(int MemberCount, int JoinedStart, int ActiveStart, int Capacity);
public sealed record MapPartyLists(IReadOnlyList<int> Joined, IReadOnlyList<int> Active, IReadOnlyList<int> Reserve);
public sealed record MapPartyJoinResult(IReadOnlyList<int> Flags, MapPartyLists Lists);

public static class MapPartyMembership
{
    // Deployment changes membership flags only. Reserve actors retain their resources and field identity.
    public static MapPartyJoinResult Deploy(IReadOnlyCollection<int> flags, MapPartyFlagLayout layout, int member, int? relief = null)
    {
        var lists = Rebuild(flags, layout);
        if (!lists.Reserve.Contains(member)) throw new ArgumentException("Member must be in reserve.", nameof(member));
        if (lists.Active.Count >= layout.Capacity)
        {
            if (relief is null || relief == 0 || !lists.Active.Contains(relief.Value))
                throw new ArgumentException("A non-leader active relief is required.", nameof(relief));
        }
        else if (relief is not null) throw new ArgumentException("No relief is needed.", nameof(relief));
        var changed = flags.Where(flag => relief is null || flag != layout.ActiveStart + relief.Value)
            .Append(layout.ActiveStart + member).Distinct().Order().ToArray();
        return new(Array.AsReadOnly(changed), Rebuild(changed, layout));
    }

    public static MapPartyJoinResult Purge(IReadOnlyCollection<int> flags, MapPartyFlagLayout layout, int member)
    {
        if (member == 0 || !Rebuild(flags, layout).Active.Contains(member))
            throw new ArgumentException("A non-leader active member is required.", nameof(member));
        var changed = flags.Where(flag => flag != layout.ActiveStart + member).ToArray();
        return new(Array.AsReadOnly(changed), Rebuild(changed, layout));
    }

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
