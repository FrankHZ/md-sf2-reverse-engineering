using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleMemberListView : Panel
{
    private readonly Label _title = new() { Name = "Title", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _empty = new() { Name = "Empty", MouseFilter = MouseFilterEnum.Ignore };
    internal MemberRosterView Roster { get; } = new() { Name = "Roster" };

    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_title); AddChild(_empty); AddChild(Roster);
    }

    internal void Present(BattleMemberSnapshot snapshot, BattleMapViewport map, bool hd)
    {
        _title.Text = GameText.Get("battle.member.title");
        _title.Position = new(24, 16); _title.Size = new(Size.X - 48, 48);
        _empty.Text = GameText.Get("battle.member.empty");
        _empty.Visible = snapshot.Members.Count == 0;
        Roster.Position = _empty.Position = new(24, 72);
        Roster.Size = Size - new Vector2(48, 96);
        Roster.Present(snapshot.Members.Select(m => new MemberRosterEntry(m.Actor,
            DisplayNames.InspectionName(true, m.NameIndex, m.Name, m.CustomName),
            !m.Available ? GameText.Get("party.value-unavailable") : m.Defeated ? GameText.Get("party.defeated") :
                !m.Deployed ? GameText.Get("party.absent") : "",
            m.Available, map.RosterPortrait(m.Actor, hd))).ToArray(), focused: true);
    }

    internal object Observe() => new { text = _title.Text, empty = _empty.Visible ? _empty.Text : null,
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), viewport = BattleMapViewport.Rectangle(Roster.GetGlobalRect()),
        scroll = Roster.ScrollVertical, rows = Roster.ObserveRows() };
}
