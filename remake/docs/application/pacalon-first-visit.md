# Pacalon town and the first castle visit

Continue either [Battle26 victory slot](battle26-higins.md) on Map72. The east scroll
carries the live party to the drought overworld, Map71. The two town entrances at
(21,15) and (22,15) lead to Map02 (21,30) facing Up. Controlled copies entered with F526 clear and showed the Battle31 boundary with
F431 held, both with F531 clear and held. These copies exercise the entrance
selector. The two ordinary parties entered with F431 and F530 clear.
[Original transfers and admission](../../../docs/research/chapters/northern-travel-mitula/battle26-return-through-pacalon-entry.md)

## Town

**Confirmed** through ordinary input. The drought scene uses ten residents. Logical
slots 128–134 give messages 3583–3594, 135 and 136 open the weapon and item shops,
and 137 opens the church. The projected physical indices depend on the followers
already in scene. The three services use the existing greetings and menus.

| Service | Stock in source order |
| --- | --- |
| Weapon shop 10 | Buster Sword 2,600 · Great Sword 5,100 · Heavy Lance 1,600 · Javelin 3,400 · Large Axe 2,250 · Assault Shell 2,500 · Great Shot 5,000 · Guardian Staff 2,380 · Indra Staff 3,200 · Power Glove 1,800 |
| Item shop 25 | Medical Herb 10 · Healing Seed 200 · Healing Drop 300 · Antidote 20 · Fairy Powder 100 · Angel Wing 40 |

Great Shot adds the BRGN option for the carried roster. Its other source classes
are SNIP and BWNT. Great Sword's HERO, BRN, BDBT, NINJ and RDBN classes are
outside the carried roster. **Confirmed** in the English party. Rohde bought
Great Shot for 5,000, carried it back through the west scroll and equipped it in
Battle26. He dealt 27 damage to a Lesser Demon on his first attack with the weapon,
using his battle sprite. The source sets its regular weapon graphics to NONE.
The existing legal equipment selection owns admission.
[Town callback and stock tables](../../../docs/research/chapters/northern-travel-mitula/pacalon-interactions-and-castle-handoff.md)

| Search target | Position | Result |
| --- | --- | --- |
| Cheerful Bread | (33,6) | F172 |
| Medical Herb | (27,24) | F173 |
| Signs | (8,13), (15,10) | Introduction 425 and 4144–4145 |
| Statues | (17,14), (17,18), (25,18), (25,14) | Introduction 432 and 4146–4148, 4151 |
| Well | (6,5) | Introduction 428 and 4149 |

## Castle

**Confirmed** in the installed remake. Town north tiles (19,0) and (23,0) have
first-match priority and land at (20,30) and (22,30). Other warp tiles on row 0
retain the horizontal coordinate. All three arrivals face Up. Frayja is declared
as logical ally 22 and gives 3301–3302. The twelve other declarations begin
at logical NPC 128. The non-service dialogue spans 3281–3300 and 3303–3307.
The priest uses the common church. Frayja remains outside the party. The
source init hides mapped 22 when F22 is held, and ordinary population
deduplicates a follower already declared as an actor. Those changed membership
states lie outside the inherited parties.
[Castle population and callbacks](../../../docs/research/chapters/northern-travel-mitula/pacalon-castle-entry-and-first-visit.md)

When F810 is clear, Event4 opens 3286 and calls `cs_5DA7A`. The centaur
moves right two tiles and down one at speed 32, right two at speed 16 and
right one at speed 8. The headshake and three shivers occur between or after
those movements as scripted. F810 stays clear through the script and is
held during 3287. The settled centaur is at (9,23) and the camera returns
to Bowie. A repeat after explicit reentry gives 3287 from the centaur's
fresh position (4,22) with the scene skipped.

| Search target | Position | Result |
| --- | --- | --- |
| Fairy Powder | (22,4) | F174 |
| Pegasus Wing | (30,19) | F175 |
| Mithril chest | (56,4) | F176 and the open block |
| Bookshelves | (35,3), (36,3) | Introduction 427 and 4152–4153 |
| Locked door | (56,8) | Introduction 433 and 4154 |
| Statues | (19,15), (23,15), (26,8), (32,8), (26,11), (32,11) | Introduction 432 and 4155–4160 |

The upper rooms use the two separate pairs of CURRENT stairs. The west pair
connects (2,4) and (52,3), and the east pair connects (5,12) and (55,11). The
opened chest and the moved centaur persist across the stairs. Explicit return
and reentry rebuilds the declared actors and reapplies the held chest flag.
The south castle exit retains X and lands on Map02 row 1 facing Down. The town
south exit lands at Map71 (22,16), Down, before the Battle27 road.
[Castle stairs and search](../../../docs/research/chapters/northern-travel-mitula/pacalon-castle-entry-and-first-visit.md#nine-shared-descriptions-and-collection-records)
· [Next journey](../../../docs/research/chapters/northern-travel-mitula/pacalon-departure-to-battle27-admission.md)

Pickups in both maps use the same ordered placement in joined packs. Repeated
Search returns empty. The shared [field Search tests](../../tests/Sf2.Remake.Engine.Tests/FieldSearchTests.cs)
exercise refusal, dropping an item and retry for both chest and non-chest
markers. The new items can be carried. Pegasus promotion and Mithril forging
remain outside the implemented services.
[Field Search rules](campaign-field-search.md)

## Church and Continue

Church entry and cancelling Save leave the previous EGRESS map in place.
Successful publication selects the church's map and sets F399. The shared
[save publication tests](../../tests/Sf2.Remake.Engine.Tests/OpeningSaveTests.cs)
exercise write refusal and write exceptions before the anchor changes. The
town and castle return coordinates are Map02 (41,10) Up and Map36 (40,5) Up.
Separate ordinary Battle26 runs used the two anchors. EGRESS cost Bowie 8 MP on
each run, returning the English party to Map02 with 5,527 gold and the Simplified
Chinese party to Map36 with 8,747 gold.

**Confirmed** with the producing checkout's scratch unavailable. A fresh title
Continue and immediate Save preserved the complete decoded gameplay data for
both languages at town (sequence 2 to 3), castle (4 to 5), and controlled Map71
departure (5 to 6). The four earlier Battle26 battle and victory saves also
matched their immediate re-saves. Ordinary movement worked after Continue on
the field. Both parties retained reserve Higins. Frayja stayed outside the
party, and F810 and the collection flags survived the castle Continue.

## Fairy Powder Use

[#1942](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1942) covers Powder carried in the
existing campaign through Pacalon and Battle26. It uses the existing field, service, battle and
save transactions. The field route retains current target selection, localized feedback and input
return. The item can come from Search or a shop, and the command pays from the actual holder and slot.

At pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, the Field and Caravan callers
`mainactions.asm:@PickTarget` and `caravanactions_1.asm:caravanItemSubmenu_Use` check the target
selection before calling `UseItemOnField`. Each invokes `RemoveItemBySlot` on the holder after the
effect returns. In both callers item 5 reaches `CurePoisonAndParalysis`, which clears bits 1 and 0
and updates stats even along the no-effect message path. In the remake, valid confirmation consumes
one item and preserves HP, MP, unrelated status and custody. Cancel, missing target and stale slot
leave the party unchanged. [Field item evidence](../../../docs/research/world/field-item-effects.md#confirmed-static-contract) ·
[Caravan Use rules](caravan-services-design.md#current-item-implementation)

Battle has its own source behavior. `itemdefs.asm` selects `POWDER` rank 1, and `spelldefs.asm`
specifies 0 MP, teammate range 0–1 and radius 0. `castspell.asm:spellEffect_Detox` clears poison
at that rank, leaving paralysis intact. `breakuseditem.asm:battlesceneScript_BreakUsedItem` removes
the selected non-equipment slot even after ineffective resolution. The remake uses the existing
battle poison-cure reaction, ineffective draw, item consumption and EXP rules.
[Original item records](../../../docs/design/contracts/party/item-definition-data.md) ·
[Remake battle binding](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)

### Ordinary input and controlled status checks

Use the #1927 Pacalon handoff's matching castle or departure slot as a read-only seed. Confirm F174
and item 5 in the live loadout after title Continue. Keep an external sample save namespace per
journey. Mount the current Debug adapter and the stable runtime at
`handoffs/issue1966/content/package` under the registered shared input root. Use the display
catalogs at `handoffs/issue1941/content/catalog-final`. The runtime includes the patched world,
its relative encounter and audio inputs, and the SLOW and BOOST identity icons. The [#1966 preparation
recipe](../domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction) can rebuild
it from the registered base if the published package is unavailable.

| Ordinary caller | Reproduce from legitimate custody |
| --- | --- |
| Field, English | Continue `castle-en.json` on Map36. Open Field Item, choose Kazin, then Use and his second slot. Back from the target list and from final confirmation, checking both return paths. Confirm against Bowie. The no-effect result spends item 5 and compacts `[213,5,118,123]` to `[213,118,123,127]`. |
| Caravan, Chinese | Continue `departure-zh-CN.json` on Map71. Align with the visible Caravan and confirm the [overworld service entry](caravan-services-design.md#current-overworld-vehicle-entry). Choose Item, Use, Slade and his third slot. Cancel the target list, reopen the same slot and confirm Bowie. The Chinese result reports no poison or paralysis to cure and that 妖精粉 was consumed. The pack changes from `[231,0,5,118]` to `[231,0,118,127]`. |

Dismiss each result and exit to settled field input, then use the ordinary Save slot and Quit.
Start a fresh process and Continue from that same slot. Re-save and compare the entire decoded
`data` object, including pack words, F174 and party resources. Both comparisons match with the
record sequence advancing by one. All four runs exited 0. Some processes logged an ObjectDB
warning during shutdown. The input, state, projected feedback and save observations completed.
Both ordinary targets have status 0. The observed branches cover ownership,
cancellation, ineffective confirmation, control, save and cold restore. Natural acquisition of
the target ailments in these histories remains **Unknown**.

For Battle26, start with a fresh copy of `castle-en.json`. Kazin is fallen. Use ordinary Field Give
to exchange Powder for Sarah's Medical Herb in her third slot. Sarah receives Powder in her fourth
slot. Follow the [Battle26 road](#reproduction), and on Sarah's first turn use Item on herself.
Cancel at target selection once, then confirm. The ineffective result leaves HP 58, MP 59 and
status 0. The pack remains `[214,15,123,5]` on cancel and becomes `[214,15,123,127]` on confirmation.
The action gives 1 EXP in that run and returns to the next player's Movement. Save there and
cold-continue. The complete decoded battle `data` matches after the fresh process re-save, and the
record sequence advances by one. The Use process saved successfully and then exited with
`0xC0000005` in `GodotObject.Finalize` after confirming ordinary Quit. The cold process exited 0 with
an ObjectDB shutdown warning. [Shutdown issue #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

`FieldRecoveryTests` and `CaravanItemTests` separately use controlled poison-only, paralysis-only,
combined and unrelated statuses. They check no-effect payment, other resources and invalid-target
rejection. `PoisonCureTests` observes the battle status write and later stun-turn processing separately.
These tests and the ordinary remake observations leave original runtime timing and audiovisual
behavior **Unknown**.

### Continue after worktree cleanup

With the producing `local/` temporarily unavailable, restore only the registered worktree
configuration. Use the external English Powder-used slot at
`handoffs/issue1942/player-saves/en-field-final/powder-review/sf2-opening/slot1.json` as the read-only
seed. Give its copy a fresh external player namespace. Use a fresh ignored receipt directory
for logs, temporary files and the Godot profile. [Fresh worktree setup](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup)

The Pacalon observer's `continue` mode opens the title's slot picker, re-saves from settled input,
then sends a movement input. Run the following from the owning checkout. Set absolute paths for
a fresh `$sampleRoot` outside all worktrees and a fresh ignored `$receipt` directory.

```powershell
. ./local/private-inputs.ps1
$handoffs = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs'
$pacalon = Join-Path $handoffs 'issue1927/pacalon'
$package = Join-Path $handoffs 'issue1966/content/package'
$catalog = Join-Path $handoffs 'issue1941/content/catalog-final'
$config = Get-Content -LiteralPath "$pacalon/player-config.json" -Raw | ConvertFrom-Json -AsHashtable
foreach ($key in $config.inputs.Keys) {
    $value = $config.inputs[$key]
    if (![IO.Path]::IsPathFullyQualified($value)) { $value = Join-Path $pacalon $value }
    [Environment]::SetEnvironmentVariable($key, $value, 'Process')
}
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
$env:SF2_DISPLAY_CATALOG = Join-Path $catalog 'world.zh-CN.json'
$env:SF2_ENTRY_DISPLAY_CATALOG = Join-Path $catalog 'entry.zh-CN.json'
Get-ChildItem -LiteralPath "$handoffs/issue1955/drivers" -Directory |
    Copy-Item -Destination ./local -Recurse
$env:SF2_DESKTOP_MODE = 'continue'
$env:SF2_REVIEW_LANGUAGE = 'en'
$env:SF2_ROUTE_DIR = $receipt
$env:SF2_SAVE_ROOT = $sampleRoot
$env:SF2_SAVE_NAMESPACE = 'powder-review'
$env:SF2_REVIEW_SLOT_PATH = Join-Path $sampleRoot 'powder-review/sf2-opening/slot1.json'
$env:APPDATA = Join-Path $receipt 'appdata'
$env:LOCALAPPDATA = Join-Path $receipt 'localappdata'
$env:TEMP = Join-Path $receipt 'temp'
$env:TMP = $env:TEMP
foreach ($dir in @($receipt, $env:APPDATA, $env:LOCALAPPDATA, $env:TEMP,
        [IO.Path]::GetDirectoryName($env:SF2_REVIEW_SLOT_PATH))) {
    [void][IO.Directory]::CreateDirectory($dir)
}
$seed = Join-Path $handoffs 'issue1942/player-saves/en-field-final/powder-review/sf2-opening/slot1.json'
[IO.File]::Copy($seed, $env:SF2_REVIEW_SLOT_PATH, $false)
$game = (Resolve-Path -LiteralPath './remake/game').Path
Push-Location -LiteralPath $game
try {
    & $env:GODOT_BIN --headless --fixed-fps 60 --audio-driver Dummy --resolution 1920x1080 `
        --path $game --script "$pacalon/Pacalon.gd" --log-file "$receipt/native.log" `
        -- --input-settings "$pacalon/settings-en.json"
} finally { Pop-Location }
```

The clean-scratch run reached settled Map36 input and moved after dismissing the save result.
Its `immediate-resave-slot.json` had identical complete decoded `data`, including F174 and
Kazin's `[213,118,123,127]` pack. The record sequence advanced from 6 to 7. Godot exited 0 and
logged an ObjectDB warning during shutdown.

## Reproduction

The private handoff is `issue1927/pacalon`. Use fresh sample names to preserve
existing receipts. Use `zh-CN` for the independent Chinese party. The rebuild
uses the configured private inputs and the [campaign selection](../../content/private/pre-voyage-opening.json).
The package retains both unchanged Battle26 victory seeds.

Run from the private handoff directory.

```powershell
./Rebuild.ps1 -Repo <checkout> -Output <new-private-directory>
./Play.ps1 -Language en -Mode route -Sample visit
./Play.ps1 -Language en -Mode continue -Sample town-load -Seed ./slots/town-en.json
./Play.ps1 -Language en -Mode continue -Sample castle-load -Seed ./slots/castle-en.json
./Play.ps1 -Language en -Mode road -Sample road -Seed ./slots/town-en.json
./Play.ps1 -Language en -Mode shot -Sample shot -Seed ./receipts/en-road/output.json
./Play.ps1 -Language en -Mode egress -Sample return -Seed ./receipts/en-road/output.json
```

For castle EGRESS start the road from the castle slot. Use the same language
for the road and its return. Save receipts include the slot before and after
Continue. Compare the entire `data` object in the decoded UTF-8 JSON envelope
payload. The outer sequence advances by one. The handoff records the bounded
runs and earlier failed attempts.

Battle27, the restored-rain return, Frayja recruitment, later item services and
human audiovisual acceptance remain outside this visit. Native shutdown under
[#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) remains
open. The narrow F526 control produced usable Map02 input before two shutdown
failures with the same finalizer access-violation stack.
