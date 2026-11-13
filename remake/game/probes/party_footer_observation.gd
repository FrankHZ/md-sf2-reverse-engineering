extends "res://probes/engine_field_service_ui_observation.gd"

# Ordinary Member, Magic and Item entry under the existing private field admission.
func field_flow() -> void:
    await press(back_key)
    var paused := state()
    for pane in range(3):
        await root_focus(pane)
        if pane == 1: await pad(JOY_BUTTON_A)
        else: await press(confirm_key)
        var p: Dictionary = state().partyStatus
        if not p.visible or p.focusPane != pane:
            check(false, "field command opens its member pane " + str(pane))
            return
        var footer := view.find_child("PartyFooter", true, false) as PanelContainer
        if footer == null:
            check(false, "mounted member footer")
            return
        var hint := footer.find_child("PartyHint", true, false) as Label
        var prepare := footer.find_child("Prepare", true, false) as Label
        var close := footer.find_child("Close", true, false) as Label
        if hint == null or prepare == null or close == null:
            check(false, "footer owns all three mounted controls")
            return
        var r := footer.get_global_rect()
        var inner := r.grow_individual(-14, -12, -14, -12)
        check(footer.is_visible_in_tree() and root.get_visible_rect().grow(-31).encloses(r), "full footer in safe area")
        for child in [hint, prepare, close]:
            check(inner.encloses(child.get_global_rect()) and child.is_visible_in_tree()
                and child.get_visible_line_count() == child.get_line_count(), "fully readable inset " + str(child.name))
        var h := hint.get_global_rect()
        var a := prepare.get_global_rect()
        var b := close.get_global_rect()
        check(h.end.x + 24 <= a.position.x and a.end.x + 24 <= b.position.x and b.end.x == inner.end.x
            and a.size.y >= 64 and b.size.y >= 64, "separate right-side targets and final inset")
        var gold := view.find_child("GoldFrame", true, false) as PanelContainer
        var gold_text := gold.get_node("Gold") as Label
        check(gold.is_visible_in_tree() and gold.get_global_rect().grow(-4).encloses(gold_text.get_global_rect())
            and gold_text.get_visible_line_count() == gold_text.get_line_count()
            and int(state().gold) == 60 and "60" in gold_text.text, "complete live gold in frame")
        for rect in [p.goldRect, p.recordRect]:
            check(p.layout.rect.y + p.layout.rect.height < rect.y
                and rect.y + rect.height < r.position.y, "records between full sheet and footer")
        var carried: Dictionary = p.projection[int(p.index)]
        for i in range(2):
            var value: Dictionary = p.recordValues[i]
            check(value.minimumWidth <= value.rect.width and value.rect.x + value.rect.width < p.recordRect.x + p.recordRect.width
                and value.text == str(int([carried.Kills, carried.Defeats][i])), "complete live record value")
        var name_label := view.find_child("MemberName", true, false) as Label
        check(name_label.text == "WWWWWWW" and p.rows[0].text.contains(name_label.text)
            and name_label.get_visible_line_count() == name_label.get_line_count(), "maximum-width legal member name")
        for region in [p.selectorRect, p.rosterRect, p.layout.rect, p.recordRect, p.goldRect]:
            check(root.get_visible_rect().grow(-31).encloses(Rect2(region.x, region.y, region.width, region.height)),
                "whole member sheet in safe area")
        samples.append({"pane":pane,"frame":str(r),"hint":hint.text,"hintRect":str(h),"prepare":prepare.text,
            "prepareRect":str(a),"close":close.text,"closeRect":str(b),"gold":gold_text.text,
            "goldRect":p.goldRect,"recordRect":p.recordRect,"recordValues":p.recordValues,"name":name_label.text})
        # The gap, an orphan release, and a release on the other target cannot activate either action.
        var gap := Vector2(a.end.x + 8, a.get_center().y)
        await mouse(gap,true); await mouse(gap,false)
        await mouse(b.get_center(),false)
        await mouse(a.get_center(),true); await mouse(b.get_center(),false)
        check(state().partyStatus.memberLayerVisible, "complete same-target gesture required")
        if pane == 0: await click(p.prepareRect)
        elif pane == 1: await pad(JOY_BUTTON_A)
        else: await press(confirm_key)
        var action: Dictionary = state().partyStatus.preparation
        check(action.visible and action.stage == ["actions","magic","item-actions"][pane]
            and not footer.is_visible_in_tree(), "preparation matches entry; footer relinquishes input")
        if pane == 0:
            await press(confirm_key)
            check(preparation().stage == "equipment", "member preparation opens equipment choices")
            await pad(JOY_BUTTON_B)
        await press(back_key)
        check(state().partyStatus.memberLayerVisible and state().partyStatus.focusPane == pane, "restore member and pane")
        if pane == 0: await pad(JOY_BUTTON_B)
        elif pane == 1: await press(back_key)
        else: await click(p.closeRect)
        var returned := state()
        check(returned.fieldMenu.visible and returned.fieldMenu.index == pane and not returned.partyStatus.visible,
            "close restores initiating selection")
        check(signature(returned) == signature(paused) and returned.revision == paused.revision
            and returned.simulationTick == paused.simulationTick and returned.failure == null, "browsing preserves session")
    await press(back_key)
    await pad(JOY_BUTTON_START)
    check(state().partyStatus.memberLayerVisible, "direct member shortcut")
    await click(state().partyStatus.closeRect)
    check(state().canWaitAtInput and not state().fieldMenu.visible and not state().partyStatus.visible,
        "direct close returns to field")
