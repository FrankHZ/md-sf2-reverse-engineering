# Map14 plank and first deck exchange

[Issue #1732](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1732) continues the
[Map58 save and departure](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control).
The [campaign](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1734) uses High rigor
for carried party state. This Standard content slice ends at the ordinary confirmation for message
2263. The [Battle08 continuation](battle08-new-granseal.md) starts at this confirmation.

## Play and review

In the prepared checkout, run `./local/issue1732/play.ps1` (Chinese) or
`./local/issue1732/play.ps1 en` (English) from PowerShell. Choose Continue and slot 1.
The launcher seeds a dedicated player namespace on first use from a compatible disposable Map58
copy. The review uses separate existing disposable copies. Graphics can be changed in Settings.
Map3 has the #1717 painted option and Map14 uses its installed original art.

1. Leave the tower, return through the castle and cross the quake town to the docks. Speak to the
   captain and accept departure. After the voyage the party stands on Map14 at (40,8), facing Left.
2. Walk east to (41,8), north to (41,5), then east to (45,5). Select No and confirm. Step left to
   (44,5), then right to ask again. Cancel with the bound cancel key (default X). Step left and
   right once more and confirm Yes.
3. On the deck, wait for the merchant to speak. Confirm the first line to reach the man's reply.
   **The direct next input for #1735 is Confirm on text 2263.** The ordinary field text wait belongs
   to `cs-58fa4` instruction 26, with return to `ms-map14-initfunction` instruction 2 pending.
   A fresh process can reproduce it from the Map58 slot. Map14 has no admitted save recipe.

**Confirmed, remake.** Two Godot 4.7.2 .NET processes ran these English and Chinese routes from
ordinary title Continue using key events and the production main scene. Both initial progress
captures match all 25 fields of the retained Map58 capture.

| Ordinary input | Observed state and projection |
| --- | --- |
| Arrive and step Down | (40,8) Left to (40,9). F507/F700 set, F74/F640 clear. |
| No, then Cancel on re-entry | Both return to field input at (45,5), F257 set and F89 clear. Movement to (44,5) succeeds after each. Both re-entries show question 2261. |
| Yes | The source zone requests explicit Map14 (13,11) Down. The default init calls the deck scene. F89 set and F257 clear. Player, merchant 128, young man 129 and man 130 have visible textured draws. |
| Read 2262 | Merchant 128 has a displayed portrait. Actor 129 (physical slot 8) has passed its right/down movements and reached (10,14) at action 5. |
| Confirm and read 2263 | Speaker 130. Actor 129 has passed its second wait and reached (10,7). Both dialogues display fully with the selected locale. The fallback, field, presentation and audio error fields are null. `MUSIC_TOWN` is playing. |

Across the plank sequence the seven party records (including equipment and HP), active/joined
lists, item custody, 2,806 gold and Map16 egress match the first Map14 control. F458/F703/F408/F508
are clear at the handoff. Ordinary time advances RNG state. The tested slot includes Kiwi.
Source order, the F6 alternative, later init writes and the second choice are described in the
[research](../../../docs/research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md)
and [design](../../../docs/design/synthesis/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md).
Original natural timing and human assessment of the presentation remain **Unknown**.

The ignored `local/issue1732/release-en` and `release-zh` receipts include
`fresh-tower-continue.json`, `map14-first-control.json`, `plank-{no,cancel,yes}.json`,
`{no,cancel}-{control,step}.json`, `merchant.json`, `reply.json`, stderr and native exit codes.
Inspect the state fields `dialogue`, `displayLocale`, `cameraProjection.actors`, `entities` and
`callers`, and the audio receipts. Both release runs exited 0. English reported two ObjectDB instances at
shutdown and Chinese stderr was empty. Earlier incremental-pack runs `run-en02` and `run-zh01` also
completed and exited 0 with a two-instance ObjectDB warning at shutdown. The initial English
observer exited 2 while Continue was still loading. Its fixed-FPS polling exhausted a frame
budget before background admission completed. The `run-en01` record retains that failure.
The successful runs allowed real time for the same title operation.

## Build and select the private content

From the checkout root, load `. ./local/private-inputs.ps1` and follow the
[ordinary world preparation](pre-voyage-campaign.md#preparing-the-ordinary-selection) with the
current tracked selection and clean SF2DISASM at `c834c652b6862bc5679fd7f69a38a7093206efc6`.
The producer compiles the Map14 event table, default init, cast and deck programs. The measured
fresh base world is 118,246,263 bytes (18 maps, 499 programs, 297 source paths). Use that
`$freshWorld` and the ordinary opening battle `$scene` for the
[complete menu and cursor publication](../content/presentation-assets.md#final-publication-and-ordinary-selection).
The exact media stages below use fresh directories under this checkout's ignored local/.
`$master` selects the [accepted Map3 material master](../content/asset-inventory.md#rebuild-and-review)
and `$entry` selects the ordinary title pack.

```powershell
uv run python -m sf2tool.remake_command_menu --rom $rom --upstream $upstream --listing $listing `
  --rom-assets --world $freshWorld --scene $scene --menu all --identity-icons --item-menu `
  --shop-battlefield --target-cursor --out-dir $menu
uv run python -m sf2tool.remake_hd_identity_icons --world "$menu/world.json" `
  --scene "$menu/scene.json" --out-dir $herb
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_map3_hd `
  --world "$herb/world.json" --scene "$herb/scene.json" --entry $entry --master $master --out $release
```

The selected `release/world.json` is 120,815,330 bytes, leaving 13,402,398 bytes below the
128 MiB world limit. World, opening scene and 32 relative dependencies total 155,481,378 bytes.
The publication with its 12 review files is 170,840,382 bytes. World presentation data, all
seven battle scenes after JSON parsing, and all five external WAV files match #1717.
The source compiler emits actor 129's asynchronous waits with operands 1200 and 600.
[#1740](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1740) owns the signed-low-byte
execution. No Map14-specific rule was added to the engine.

To reproduce the #1732 package at its accepted revision, derive its Chinese catalog against the finished world with the
[ordinary selector](../development-and-verification.md#pre-voyage-chinese-consumer-observation).
It includes 2261–2263 (809 world messages, 51 names, 111,915 bytes). The 1,042-byte entry catalog
keeps its six aliases. `--scope pre-voyage` reproduces the earlier 806-message catalog and
review outputs byte for byte. Both English and Chinese select the same gameplay package.

For the earlier sizing pilot, compiling `cs_58FA4` from Map14 `mapsetups/scripts_2.asm` in the
existing [compiler](../../../src/sf2tool/remake_exploration_programs.py) and updating its three
compiled programs in a fresh copy of #1717 produced a 120,810,482-byte world. Only instruction 10
differed among those three program bodies. The default full build above also refreshes the other
selected maps and programs and supplies the player and successor packages.

For a fresh launch, retain the ordinary entry and Battle01 data inputs. Select the published
world.json with `SF2_PRIVATE_EXPLORATION_CONTENT`, scene.json with `SF2_PRIVATE_BATTLE_SCENE_CONTENT`,
world.zh-CN.json with `SF2_DISPLAY_CATALOG`, and entry.zh-CN.json with `SF2_ENTRY_DISPLAY_CATALOG`.
Clear `SF2_PRIVATE_CONTROLLED_START`, use an external save namespace containing the compatible
Map58 slot and launch `& $env:GODOT_BIN --path remake/game`.
