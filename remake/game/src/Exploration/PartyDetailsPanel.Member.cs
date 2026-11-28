using Godot;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class PartyDetailsPanel
{
    private void FitMemberSlotGaps()
    {
        int gap = 12;
        if (_memberSlotHeight == 80 && _slotCount > 1 && _items.Size.Y > 0)
        {
            // Keep the normal gaps unless wrapped item names need that space.
            float rowHeight = _itemRows.Take(_slotCount).Sum(slot => slot.Row.GetCombinedMinimumSize().Y);
            gap = Mathf.Clamp(Mathf.FloorToInt((_items.Size.Y - rowHeight) / (_slotCount - 1)), 0, gap);
        }
        if (_inventory.GetThemeConstant("separation") != gap)
            _inventory.AddThemeConstantOverride("separation", gap);
    }

    private void ConfigureMemberReaders()
    {
        _magicTitle.FocusMode = _itemsTitle.FocusMode = FocusModeEnum.All;
        _magicTitle.FocusEntered += () => FocusPane = 1;
        _itemsTitle.FocusEntered += () => FocusPane = 2;
        foreach (var (title, pane) in new[] { (_magicTitle, 1), (_itemsTitle, 2) })
        {
            var button = ButtonFor!(null, () => ReadingControl(pane).GrabFocus());
            title.AddChild(button);
            button.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        }
        var reader = MemberPageButton.Stack(_spells);
        reader.SizeFlagsVertical = SizeFlags.ExpandFill;
        reader.AddChild(ButtonFor!(_magic, () => _magicTitle.GrabFocus()));
        _magic.AddChild(reader);
        _description.EnableGuiScroll();
    }

    internal Control ReadingControl(int pane, int? slot = null) => pane == 1 ? _magicTitle :
        _slotCount > 0 ? _itemRows[slot ?? ItemSlot].Row : _itemsTitle;

    internal void MoveSlotFocus(int step)
    {
        if (_slotCount > 0)
            ReadingControl(2).FindValidFocusNeighbor(step < 0 ? Side.Top : Side.Bottom)?.GrabFocus();
    }

    private void BuildMemberSheet(CenterContainer portrait)
    {
        var inset = new MarginContainer();
        foreach (var edge in new[] { "left", "top", "right", "bottom" })
            inset.AddThemeConstantOverride("margin_" + edge, 24);
        AddChild(inset);
        inset.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var columns = new HBoxContainer();
        inset.AddChild(columns);
        var attributes = new VBoxContainer { CustomMinimumSize = new(280, 0) };
        attributes.AddThemeConstantOverride("separation", 52);
        columns.AddChild(attributes);
        portrait.CustomMinimumSize = new(160, 192);
        portrait.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        attributes.AddChild(portrait);
        var stats = new GridContainer { Columns = 2 };
        stats.AddThemeConstantOverride("h_separation", 8);
        stats.AddThemeConstantOverride("v_separation", 4);
        attributes.AddChild(stats);
        foreach (var (title, value) in _stats)
        {
            title.CustomMinimumSize = new(0, 48);
            title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            stats.AddChild(title);
            stats.AddChild(value);
        }

        var reading = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        reading.AddThemeConstantOverride("separation", 12);
        columns.AddChild(reading);
        var header = new HBoxContainer { CustomMinimumSize = new(0, 144) };
        reading.AddChild(header);
        var identity = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        identity.AddThemeConstantOverride("separation", 12);
        header.AddChild(identity);
        identity.AddChild(_name);
        identity.AddChild(_class);
        var vitals = new VBoxContainer { CustomMinimumSize = new(480, 0), Alignment = BoxContainer.AlignmentMode.Center };
        vitals.AddThemeConstantOverride("separation", 16);
        header.AddChild(vitals);
        foreach (var control in new Control[] { _hp, _unknownHp, _mp, _unknownMp })
        {
            control.CustomMinimumSize = new(0, 56);
            vitals.AddChild(control);
        }

        var lists = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        reading.AddChild(lists);
        var spells = ReadingColumn(lists, _magicTitle, _magic, 0.43f);
        spells.AddChild(_description);
        ReadingColumn(lists, _itemsTitle, _items, 0.57f);
    }

    private static VBoxContainer ReadingColumn(HBoxContainer parent, Label title, ScrollContainer scroll, float share)
    {
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = share };
        column.AddThemeConstantOverride("separation", 12);
        parent.AddChild(column);
        column.AddChild(title);
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        column.AddChild(scroll);
        return column;
    }
}
