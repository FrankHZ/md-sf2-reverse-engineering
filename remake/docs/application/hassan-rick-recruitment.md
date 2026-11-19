# Rick's Hassan recruitment

After Taros, return to Hassan while F518 selects the new cast. Stand at (12,4) facing up and
Talk to Rick at (12,3). He joins as a level 19 Knight carrying an equipped Steel Lance.
If the active force has twelve members, he stays in reserve. When the Caravan overlaps
Bowie on the overworld, interact with it, choose Join, select Rick and choose an active
member to relieve. Leave the service to resume walking. A later battle can deploy Rick.
[Taros and Caravan](taros-caravan.md) · [Formation service](caravan-services-design.md)

## Source behavior and implementation

The F518 setup supplies Rick's actor. The source event checks F256, runs the recruit and local
follow program, then sets F256 on return. F12 records membership and F44 records active
formation. On fresh Hassan entry, F12 moves the actor off-map. The original persistent
follower table has no Rick row. Rick's start class, level, loadout and growth come from the
pinned USA definitions. [Original Hassan program](../../../docs/research/chapters/southern-travel-creed/hassan-rick-recruitment.md) ·
[Pinned starting records](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/allystartdefs.asm#L112-L118)

The private campaign selects KNTE growth for ally 12. The existing source admission creates the
party record and the common Join caller reads it. The private scene producer supplies the
sprite 27 field pose, Knight body with palette 1 and the selected weapon layers. The Chinese
catalog selects 1411–1419 for this admitted member and reuses the existing generic 446 and
Rick and Steel Lance names. [Campaign selection](../../content/private/pre-voyage-opening.json) ·
[Scene producer](../../../src/sf2tool/remake_battle_scene_content.py) ·
[Display catalog selection](../../../src/sf2tool/remake_localization.py)

## Confirmed in the running remake

| Player input | Result |
| --- | --- |
| Resume an eleven-active post-Taros Shrine save, leave the shrine and take the road east | An encounter intercepted the walk to Hassan. Paid EGRESS from Battle17 led to Hassan with F518. Ordinary Talk added Rick to both joined and active lists. |
| Resume a full force's Hassan return save | The English and Simplified Chinese Talk played 1411–1418, 446 and 1419, then returned input. Rick entered reserve and followed during that visit. Repeat Talk from the approach replayed no recruitment text and preserved the party. Fresh entry through Map69 (26,55) cleared F256 and parked Rick while retaining F12. |
| Use Caravan Join on the overworld | Cancelling Relief returned to reserve selection with the same party. Confirming replaced Slade with Rick and retained their records and equipment. The western road admitted Rick to Battle17 with map sprite 27 and Steel Lance word 203. |
| Select Attack during Rick's ordinary turn | From the same battle-ready save, Rick dealt 6 damage to the High Priest and gained 10 EXP in both languages. His acting card, Knight body and weapon layer were present. Player movement resumed and the resulting battle could be saved and continued. Paid EGRESS after the attack returned to Hassan with Rick still in the active party. |

## Save and Continue

A Hassan Save after Talk restores at the town anchor. Cold Continue retained Rick's record
and membership, cleared F256 and rebuilt Hassan with the local Rick actor parked. The
battle Save after the attack retained Rick's actual placement, EXP, equipment, the current
queue and both RNG words through cold Continue.

Roster revision 10 adds ally 12 to the private campaign. A compatible old field or battle
slot gets just the records missing from its historical revision. Starting growth uses a copy of
the saved main seed. Both RNG words carry through restoration. Rick's old-slot upgrade adds
neither F12 nor F44. It leaves the existing party, packs and skill progress intact.
English Pacalon town, Chinese Pacalon Castle and English Battle26 slots resumed from
the title and saved again. Town actors and the field RNG can advance while the running
field is idle. The first restored state retained the saved RNG words. The re-saved Battle26
retained its complete live battle. [Historical roster rules](battle-save-and-continue.md#continuing-across-historical-opening-rosters)

## Reproduction and limits

Use the registered private `handoffs/issue1939` bundle and its `HANDOFF.md`. It gives the
rebuild and fresh-sample launch commands for ordinary Talk, Relief, Attack and all cold
Continue checks. The unedited starts are an earlier Taros victory, a later full-force Hassan
return, and pre-upgrade Pacalon and Battle26 slots. The retained baseline shows ordinary Talk
stopping at `join-member-binding` on `program.member` after text 1418.

The slice uses **High** rigor because changed content crosses live party, formation, rendering
and existing field and battle saves. The retained world comparison changes only Rick's growth
and its source provenance. Generated assets, catalog text, slots and observations stay in the
private bundle. [Issue #1939](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1939)

**Unknown** includes the original game's natural runtime timing and audiovisual parity. Other
Rick weapon combinations and later encounters can use the admitted material, with ordinary
combat observed here in Battle17. Promotion retains its separate scope.
