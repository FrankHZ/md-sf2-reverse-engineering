# Campaign field Search

This Standard content correction covers optional searches in the ordinary campaign through the first raft step.

At ordinary field control, open the field menu and choose **Search** while facing the tile. Confirm also searches when there is no one to talk to. Close the result to move again. A full party can leave the item, free a slot and search again. During departure dialogue, choices, warps and battle entry, the current scene owns the input.

## Docks and damaged ship

**Confirmed (static).** The USA retail baseline and SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` supply two identical other-item records on Maps44 and 14. Both chest tables are empty. Their selected setups share their respective direct-return area-description handler. `CheckArea` uses the faced working-layout word masked with `0x3C00` before consulting an item table. The same flag is a single story receipt wherever a valid placement is found. [Field Search control](../../../docs/research/world/field-search-control.md) · [Map44 table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map44/8-other-items.asm) · [Map14 table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map14/8-other-items.asm).

| Target | Docks (44) | Damaged ship (14) |
| --- | --- | --- |
| (27,6), F139, Quick Chicken (11) | Barrel on deck. Face Up from (27,7). | Unmarked solid tile. From the post-Battle08 retreat deck, face Up from (27,7) for the ordinary empty response. |
| (58,9), F140, Medical Herb (0) | Barrel below deck. Before departure, walk to the stairs at (13,12), step Up, approach (58,8) and face Down. At F507 the passengers occupy the three stair entrances. | Outside all four source areas. The working word is zero. The accessible barrel at (50,9) below deck has no placement. |
| Ordinary unmarked tile | Explicit Search gives investigation and empty text 423/412. | Same result at first restored control after the voyage. |

**Confirmed (remake, ordinary input).** The F609 church Continue route exits Map3 east to Map44. The deck barrel grants Quick Chicken and sets F139. Repeating the action, then leaving through Map3 and returning, gives barrel-empty 405/410 without a second grant. The below-deck herb grants one carried item and sets F140. A second search gives barrel-empty 405/410. With all joined inventories filled on a separate controlled copy of that save, the same barrel gives Full, preserves the party and leaves F140 clear. Dropping an item through the ordinary menu permits a successful retry. The full-pack condition was supplied to this copy; the walking, menu and inventory actions used real inputs.

From a carried Map58 save, take the earthquake route. The F507 docks again allow the deck pickup at (27,6). Accept the captain and board with F139 set. The captain's choice has no field menu or exploration control. The first ship input is below deck. The accessible barrel at (50,9) gives 405/410 with no receipt. The plank crew occupies (44–46,6). During deck line 2263 the directional and field-menu keys leave the player in place, the line active and the field menu closed. Bring up the plank, advance the deck dialogue, enter Battle08 and cast Egress on Bowie's turn. At F458 take the stairs above deck and face (27,6). F139 survives departure and the paid retreat. The response is 423/412 with no receipt. The walking deck and below-deck observations cover different coordinates; the zero-word F140 coordinate is outside the active areas. [Departure route and accepted inputs](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) · [Deck and plank](map14-plank-deck.md#play-and-review) · [Battle08 retreat](battle08-new-granseal.md).

**Unknown.** The player's precise ship tile in the initial report, original natural paths to every placement and hardware timing. The [reported feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456#issuecomment-6843398513) was captured after returning to town. The [tower correction](opening-session-lifecycle.md#tower-exterior-search-admission) admitted Map40, verified area0 empty and Mithril pickup/repeat through private Content → GameSession, and used an authored Godot scene for menu and shortcut mapping. The [independent review of #1631](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1631#issuecomment-6679695453) retained those boundaries. The later docks route explicitly tested the town exit and repeat entry, captain No/cancel/Yes, first ship control and one step. Its [acceptance table](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) and the [later plank acceptance](map14-plank-deck.md#play-and-review) contain no dock or ship Search action.

## Map and setup inventory

**Confirmed (static).** The 31-map publication used for the report supplied Search on seven defaults and their compiled variants. All defaults and selected variants below have a null input program. A null input program alone does not grant control during a script. Each row lists every **compiled** flag variant alongside its default. Placements are source receipt flags, including optional and later-area entries. Counts in parentheses are text-only descriptions per setup; a dash denotes no placement. A compiled row alone gives no route to the item. [Selection](../../content/private/pre-voyage-opening.json) · [Original field menu Search entry](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm).

| Map | Additional setup flags | Source placements (descriptions) | Campaign Search / entry evidence |
| --- | --- | --- | --- |
| 3 | 609, 506 | 220 empty chest, 128 (17) | Existing. New Game and F609 church return. |
| 19 | 501, 609, 506, 507 | 129–131 (3) | Existing. Castle entry and carried earthquake return. |
| 20 | 501, 609, 506 | — (5) | Existing. Castle stairs. |
| 21 | 501, 609, 506 | — (0) | Existing. Upper castle/tower route. |
| 40 | 506 | 132 (0) | Existing. Tower exterior; #1612 private controlled Search. |
| 57 | — | — (0) | Existing. Tower antechamber. |
| 66 | — | 213 (0) | Added. Overworld field between opening encounters. |
| 5 | — | 196,197,134 (4) | Added. Yeel. The later F530 setup is outside the package. |
| 43 | 612 | 135,136 (9) | Existing. Hawel's house. |
| 17 | 505 | 205,206 (0) | Added. Galam prison, ladder and kitchen. |
| 16 | 663 | 137,138,207,208,200 (8) | Added. Galam rally, guard and battle return. |
| 18 | — | — (1) | Added. Drawbridge/camp exit from Galam. |
| 60 | — | — (0) | Added. Tower stairs. |
| 58 | — | — (0) | Added. Tower return. The two pointers alias the same empty chest table. |
| 4 | — | 217 (2) | Added. Quake entry and return crossing. |
| 44 | 609, 506, 507 | 139,140 (0) | Added. Docks, stairs and captain. F609/F507 input observed; default/F506 tables checked. |
| 73 | — | — (0) | Added. The voyage init owns control; ordinary overworld travel occurs in the later chapter. |
| 14 | 458 | 139,140 (0) | Added. Default below-deck and F458 above-deck input observed; both item coordinates lack a marker. |
| 72 | — | — (0) | Added. Eastern crossing and approach to the Polca raft. |
| 7 | 701 | 142,143 (2) | Added. Year-later castle and audience. |
| 6 | 701, 702 | 210,211,141,209 (callbacks) | Withheld. New Granseal is reached; its selected handlers need conditional code. |
| 69 | — | — (0) | Added. Battle09 return and Ribble approach. |
| 8 | 708 | 144,145 (callbacks) | Withheld. Ribble is reached; signs, objects and the tree have conditional handlers. |
| 70 | — | 188 (0) | Added. Battle10 field return and cave route. |
| 62 | 77 | 150 (0) | Deferred. The shrine entrance on the other bank was tested from supplied coordinates. Natural access before the raft remains Unknown. |
| 71 | — | — (0) | Added. Overworld passage between 70 and 72. |
| 48 | — | 146,168 (0) | Added. Dark Caves entrance and Battle11 return. |
| 13 | 710, 799, 513 | 148 (4) | Added. Polca and mayor. F513 ordinary sign at (5,9) shows 425/4101 in Chinese. Enter at (9,19), face Left from (36,6) and collect Protect Milk with F148. |
| 52 | 512 | — (0) | Added. Mountain battle return and Bedoe entrance. |
| 10 | 722 | 149 (9) | Added. Bedoe stairs and audience. |
| 11 | — | — (1) | Added. Volcanon Shrine. |

Route evidence outside the named Search observations is for entering the chapter or map. Separate rooms, endgame sections, unopened doors and every individual item are not implied by the entrance. See the [Grans stages](pre-voyage-campaign.md), [eastern travel and conditional shrine](new-granseal-east-shrine.md), [caves through Bedoe](dark-caves-bedoe.md) and [Polca departure](polca-first-raft.md). The remaining callback work is bounded at [#1759](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1759). Map6's description dispatch includes the F702 sign and yes/no interaction; Map8 has multiple F708 and scripted description rows. Flattening either table to text would change gameplay.

## Reproduce

In the prepared review checkout, start `./local/issue1757/play.ps1 Docks` and choose Continue, slot 1. Walk east from the F609 church, leave Granseal at (50,24) and use the two docks approaches above. `Voyage` starts at the tower return in slot 1. `Shops` starts at the F609 church with spending money in slot 2 for the [two shop counters](../content/profiles-and-trust.md#ordinary-campaign-shops). Each start seeds its own namespace once. All three use the same Chinese v2 publication.

Build the selected opening world using the [campaign producer](pre-voyage-campaign.md#preparing-the-ordinary-selection) and publish its finite inventory art and [current Chinese catalog](../development-and-verification.md#pre-voyage-chinese-consumer-observation). The catalog includes the actual compiled shop stock, nonempty Search rewards and text pairs across defaults and variants. Field receipt identity enables carry and Give. Equipment, item Use, promotion and other services retain their own capability checks.

For the ledger, join each selected map's canonical `chestItemTable`, `otherItemTable`, layout and `setupRoute` to its default and selected `areaDescriptions`. Inspect the compiled world's `search`, `input`, `setup` and `setups`; verify each placement against the faced marker and active area. Run the engine's `FieldSearchTests` and, with the prepared private world selected as `SF2_PRIVATE_TOWER_SEARCH_WORLD`, `TowerFieldSearchTests`. No per-frame trace or original runtime run is required for the static inventory.
