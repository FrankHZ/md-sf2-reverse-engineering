using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private bool _showMembers, _sarahSelected;

    private void ToggleMembers()
    {
        if (!_showMembers)
            _sarahSelected = _scene == 3 && !(_page == Page.Target && _target == 0);
        _showMembers = !_showMembers;
        Render();
    }

    private void SelectMember(bool sarah)
    {
        if (_sarahSelected == sarah) return;
        _sarahSelected = sarah;
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
        { ToggleMembers(); return true; }
        if (input.IsActionPressed("ui_down") || input.IsActionPressed("ui_up"))
        { SelectMember(!_sarahSelected); return true; }
        return false;
    }

    private void Members()
    {
        Put(_surface, new ColorRect { Color = new Color(0, 0, 0, 0.5f), MouseFilter = MouseFilterEnum.Ignore },
            new(0, 72, 1920, 1008));
        var roster = Panel(new(104, 186, 304, 302));
        Text(roster, new(20, 12, 264, 58), T("队伍", "Party"), 32);
        void MemberRow(int index, UnitSummary unit, bool sarah)
        {
            bool selected = sarah == _sarahSelected;
            var row = Utility(new(124, 270 + index * 94, 264, 82), (selected ? "›   " : "") + unit.Name,
                () => SelectMember(sarah), selected);
            row.AddThemeFontSizeOverride("font_size", 30);
            if (selected) row.AddThemeColorOverride("font_color", new Color("ffe19a"));
        }
        MemberRow(0, Bowie, false);
        MemberRow(1, Sarah, true);

        var sheet = Panel(new(428, 186, 1388, 704));
        sheet.Name = "MemberSheet";
        bool healer = _sarahSelected;
        var unit = healer ? Sarah : Bowie;
        var portrait = _stage.MemberPortrait(healer);
        if (portrait is not null)
            Put(sheet, new TextureRect { Texture = portrait, TextureFilter = TextureFilterEnum.Nearest,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore }, new(32, 28, 144, 192));
        int identityX = portrait is null ? 32 : 208;
        Text(sheet, new(identityX, 22, 470, 68), unit.Name, 40);
        Text(sheet, new(identityX, 96, 470, 52), healer ? T("僧侣   ·   Lv. 4", "Priest   ·   Lv. 4")
            : T("剑士   ·   Lv. 5", "Swordsman   ·   Lv. 5"), 30);
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
        int[] values = healer ? [11, 9, 12, 5] : [16, 12, 10, 6];
        for (int i = 0; i < stats.Length; i++)
        {
            Text(sheet, new(32, 332 + i * 76, 224, 64), GameText.Get("party.stat." + stats[i]), 30);
            Text(sheet, new(264, 332 + i * 76, 70, 64), values[i].ToString(), 30).HorizontalAlignment = HorizontalAlignment.Right;
        }

        void DetailRow(float x, float y, float width, string? key, string label, string note = "")
        {
            var texture = key is null ? null : _stage.Icon(key, false);
            float offset = texture is null ? 0 : 64;
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
        DetailRow(834, 328, 522, healer ? "identity-icon.item.85" : "identity-icon.item.71",
            healer ? T("1. 木杖", "1. Wooden Rod") : T("1. 木剑", "1. Wooden Sword"), GameText.Get("party.equipped"));
        DetailRow(834, 410, 522, "identity-icon.item.0", T("2. 药草", "2. Medical Herb"));
        for (int slot = 3; slot <= 4; slot++)
            DetailRow(834, 328 + (slot - 1) * 82, 522, null, $"{slot}. {GameText.Get("party.empty")}");

        Footer(T("↑ ↓ 切换队员     Esc / B 返回", "↑ ↓ Switch member     Esc / B Back"));
        Utility(new(1642, 1003, 204, 48), T("返回", "Back"), ToggleMembers);
    }
}
