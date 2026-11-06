using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleMemberListView : Panel
{
    private readonly Label _rows = new() { Name = "Members", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_rows); _rows.AddThemeFontSizeOverride("font_size", 28);
    }
    internal void Present(BattleMemberSnapshot snapshot, int selected)
    {
        _rows.Position = new(24, 20); _rows.Size = Size - new Vector2(48, 40);
        int count = Math.Max(1, (int)((Size.Y - 120) / 66));
        int first = Math.Clamp(selected - count / 2, 0, Math.Max(0, snapshot.Members.Count - count));
        _rows.Text = GameText.Get("battle.member.title") + "\n\n" + (snapshot.Members.Count == 0 ? GameText.Get("battle.member.empty") :
            string.Join("\n", snapshot.Members.Skip(first).Take(count).Select((m, i) =>
                (first + i == selected ? "> " : "  ") + DisplayNames.InspectionName(true, m.NameIndex, m.Name, m.CustomName) +
                (!m.Available ? " · " + GameText.Get("party.value-unavailable") : m.Defeated ? " · " + GameText.Get("party.defeated") :
                    !m.Deployed ? " · " + GameText.Get("party.absent") : ""))));
    }
    internal object Observe() => new { text = _rows.Text, rect = BattleMapViewport.Rectangle(GetGlobalRect()) };
}
