extends "res://probes/engine_connected_give_observation.gd"

# Reuse the accepted ordinary connected Give caller; only add bounded resource observations.
func art(label: String) -> void:
    var s := state()
    for icon in [s.transactionIcon, s.returnIcon]:
        if not icon.visible: continue
        check(icon.error == null and icon.drawn == icon.resource and icon.texture != null,
            label + ": actual semantic texture")
        check(icon.width == 16 and icon.height == 24 and icon.scale == 4 and icon.filter == "Nearest",
            label + ": source size/filter")
        check(icon.rect.x + icon.rect.width <= s.hudPanel.body.x,
            label + ": image stays outside readable text")
    if s.stage in ["GiveItemChoice", "GiveTargetChoice", "GiveExchangeChoice", "GiveCommitReady"]:
        var given: int = int(s.inventories.filter(func(a): return a.actor == s.actor)[0].items[int(s.itemSlot)]) & 127
        check(s.transactionIcon.identity == "identity-icon.item." + str(given), label + ": live giver slot")
        if s.returnSlot != null:
            var returned: int = int(s.inventories.filter(func(a): return a.actor == s.target)[0].items[int(s.returnSlot)]) & 127
            check(s.returnIcon.identity == "identity-icon.item." + str(returned), label + ": live return slot")
    samples.append({"label":label + "-art", "stage":s.stage, "itemSlot":s.itemSlot, "returnSlot":s.returnSlot,
        "given":s.transactionIcon, "returned":s.returnIcon, "hudPanel":s.hudPanel})

func record(label: String) -> void:
    super.record(label)
    var s := state()
    samples.append({"label":label + "-projection", "stage":s.stage, "given":s.transactionIcon, "returned":s.returnIcon})

func select_recipient(recipient: String) -> bool:
    var result: bool = await super.select_recipient(recipient)
    await frames(3)
    if result: art("selected-recipient")
    return result
