using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum Member { Bowie, Sarah, Chester }
    private bool _showMembers;
    private Member _member;
    private UnitSummary MemberUnit(Member member) => member switch
    {
        Member.Bowie => Bowie, Member.Sarah => Sarah, _ => new(T("切斯特", "Chester"), 22, 26, 0, 0)
    };

    private void ToggleMembers()
    {
        _giveStep = GiveStep.Closed;
        if (!_showMembers)
            _member = _scene == ShopScene ? (_shopStep == ShopStep.Member ? (Member)_focus : _shopMember)
                : _scene == 3 && !(_page == Page.Target && _target == 0) ? Member.Sarah : Member.Bowie;
        _showMembers = !_showMembers;
        Render();
    }

    private void SelectMember(Member member)
    {
        if (_member == member) return;
        _member = member;
        Render();
    }

    private bool MemberInput(InputEvent input)
    {
        if (input is InputEventKey { Keycode: Key.F5, Pressed: true, Echo: false } ||
            input is InputEventJoypadButton { ButtonIndex: JoyButton.X, Pressed: true })
        { ToggleMembers(); return true; }
        if (!_showMembers) return false;
        if (input.IsActionPressed("ui_cancel") ||
            input is InputEventJoypadButton { ButtonIndex: JoyButton.B, Pressed: true } ||
            input is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true })
        { if (Preparing) BackGive(); else ToggleMembers(); return true; }
        if (input.IsActionPressed("ui_accept") ||
            input is InputEventJoypadButton { ButtonIndex: JoyButton.A, Pressed: true })
        { if (Preparing) Activate(); else OpenGive(); return true; }
        if (input.IsActionPressed("ui_down") || input.IsActionPressed("ui_up"))
        {
            if (Preparing)
            {
                int delta = input.IsActionPressed("ui_down") ? 1 : -1;
                _giveFocus = (_giveFocus + delta + _choices.Count) % _choices.Count;
                Refresh();
            }
            else SelectMember((Member)(((int)_member + (input.IsActionPressed("ui_down") ? 1 : 2)) % 3));
            return true;
        }
        return false;
    }

    private void Members()
    {
        Put(_surface, new ColorRect { Color = new Color(0, 0, 0, 0.5f), MouseFilter = MouseFilterEnum.Ignore },
            new(0, 72, 1920, 1008));
        var roster = Panel(new(104, 186, 304, 376));
        Text(roster, new(20, 12, 264, 58), T("队伍", "Party"), 32);
        foreach (Member member in Enum.GetValues<Member>())
        {
            bool selected = member == _member;
            var row = Utility(new(124, 270 + (int)member * 94, 264, 82), MemberUnit(member).Name,
                () => SelectMember(member), selected);
            row.AddThemeFontSizeOverride("font_size", 30);
            if (selected) row.AddThemeColorOverride("font_color", new Color("ffe19a"));
        }

        DrawUnitSheet(new(428, 186), _member);
        Footer(T("↑ ↓ 切换队员     Enter / A 整备     Esc / B 返回", "↑ ↓ Switch member     Enter / A Prepare     Esc / B Back"));
        Utility(new(1418, 1003, 204, 48), GameText.Get("party.prepare"), OpenGive);
        Utility(new(1642, 1003, 204, 48), T("返回", "Back"), ToggleMembers);
    }

    // A null roster member is the staged enemy. Its inventory and attributes are independent of the party.
    private void DrawUnitSheet(Vector2 position, Member? member)
    {
        var sheet = Panel(new(position, new Vector2(1388, 704)));
        sheet.Name = "MemberSheet";
        bool healer = member == Member.Sarah;
        var unit = member is { } ally ? MemberUnit(ally) : Gizmo;
        var pack = member is { } owner ? _packs[owner] : null;
        var portrait = member is { } actor ? _stage.MemberPortrait((int)actor) :
            _stage.SceneArt?.Enemies.Values.FirstOrDefault() is { } enemy ? _stage.BattleTexture(enemy.Frames[0]) : null;
        if (portrait is not null)
            Put(sheet, new TextureRect { Texture = portrait, TextureFilter = TextureFilterEnum.Nearest,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore }, new(32, 28, 144, 192));
        int identityX = portrait is null ? 32 : 208;
        Text(sheet, new(identityX, 22, 470, 68), unit.Name, 40);
        Text(sheet, new(identityX, 96, 470, 52), member switch {
            null => T("敌军", "Enemy"),
            Member.Sarah => T("僧侣   ·   Lv. 4", "Priest   ·   Lv. 4"),
            Member.Chester => T("骑士   ·   Lv. 4", "Knight   ·   Lv. 4"),
            _ => T("剑士   ·   Lv. 5", "Swordsman   ·   Lv. 5") }, 30);
        if (member.HasValue)
            Text(sheet, new(identityX, 155, 470, 48), healer ? "EXP    24" : "EXP    36", 24, "cad6e5");
        Vitals(sheet, new(808, 64), unit);

        void Section(float x, float width, string title)
        {
            Text(sheet, new(x, 252, width, 52), title, 30);
            Put(sheet, new ColorRect { Color = new Color("536c87"), MouseFilter = MouseFilterEnum.Ignore },
                new(x, 312, width, 2));
        }
        Section(32, 302, T("能力", "Attributes"));
        Section(382, 404, GameText.Get("party.magic"));
        Section(834, 522, GameText.Get("party.items"));
        string[] stats = ["attack", "defense", "agility", "move"];
        int[] values = member switch { null => [7, 5, 5, 5], Member.Sarah => [11, 9, 12, 5],
            Member.Chester => [15, 13, 11, 7], _ => [16, 12, 10, 6] };
        // The two starting weapons each contribute 3 attack until sold.
        if (pack is not null && member != Member.Chester && !pack.Any(IsWeapon)) values[0] -= 3;
        for (int i = 0; i < stats.Length; i++)
        {
            Text(sheet, new(32, 332 + i * 76, 224, 64), GameText.Get("party.stat." + stats[i]), 30);
            Text(sheet, new(264, 332 + i * 76, 70, 64), values[i].ToString(), 30).HorizontalAlignment = HorizontalAlignment.Right;
        }

        void DetailRow(float x, float y, float width, string? key, string label, string note = "")
        {
            var texture = key is null ? null : _stage.Icon(key, false);
            const float offset = 64;
            if (texture is not null)
                Put(sheet, new TextureRect { Texture = texture, TextureFilter = TextureFilterEnum.Nearest,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    MouseFilter = MouseFilterEnum.Ignore }, new(x, y, 48, 72));
            float info = note.Length == 0 ? 0 : 128;
            Text(sheet, new(x + offset, y, width - offset - info, 72), label, 28);
            if (info > 0) Text(sheet, new(x + width - info, y, info, 72), note, 24, "cad6e5")
                .HorizontalAlignment = HorizontalAlignment.Right;
        }
        if (healer)
        {
            DetailRow(382, 328, 404, "identity-icon.spell.0", T("回复  Lv. 1", "HEAL  Lv. 1"), "3 MP");
            DetailRow(382, 410, 404, "identity-icon.spell.0", T("回复  Lv. 2", "HEAL  Lv. 2"), "5 MP");
        }
        else DetailRow(382, 328, 404, null, GameText.Get("party.none-learned"));
        for (int slot = 0; slot < PackCapacity; slot++)
        {
            var id = pack is null ? -1 : SlotId(pack, slot);
            DetailRow(834, 328 + slot * 82, 522, ItemArt(id), SlotLabel(id, slot),
                IsWeapon(id) ? GameText.Get("party.equipped") : "");
        }
    }
}
