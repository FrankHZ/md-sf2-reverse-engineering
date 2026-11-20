# Elven Village and the first Battle19 return

**Confirmed** in the private campaign with ordinary Godot input. The accepted post-Taros shrine and Southwest Parmecia landing slots both open in English and Simplified Chinese. The shrine route crosses the river and reaches the same walking fork as the landing slot. The landing witness has eleven joined and active members, three living, 9,174 gold, F64/F65, a Map6 return town and a raft at Map69 (28,55). F419/F469/F519 start clear. [Caravan controls and shrine route](caravan-travel.md)

## From the landing

From (56,27), walk Left three, Down three, Left four. Allow the Caravan to board and land as the party crosses the dry riverbed. At Map68 (49,30) choose a direction.

| Input | Observed result |
| --- | --- |
| Left | Enter Map23 at (22,29) facing Right. The default setup deploys six village residents alongside the field party and selects Elven Town music. Walk Down through the south exit to Map68 (48,31) facing Down. Walking, the raft berth and clear F419/F469/F519 survive the round trip. |
| Up, Left twice | The marked cell at (47,29) sets F419 and admits Battle19. First start sets F469. Content supplies twelve placement templates, fifteen enemy rows, three regions and one point. The inherited party produces eleven ally records with Bowie, Sarah and Peter on the board. Actual equipment, current HP/MP and the learned spell books feed the existing battle rules. |

The private selection imports the village's default setup, warps, music and the encounter. The High Priest's healing command tries wounded allies in priority order. Its local spell level can change between targets. The chosen HEAL level supplies the actual MP cost and casting range. BLAST and FREEZE use their affordable learned levels. A behavior test exercises HEAL3 at distance three, alongside checks for the source's self-target, wound and MP thresholds. [Original selection and initialization](../../../docs/research/chapters/southern-travel-creed/battle19-initialization-and-first-control.md) · [Engine behavior checks](../../tests/Sf2.Remake.Engine.Tests/EnemySpellDecisionTests.cs)

## Victory and the next slot

Defeat the leader Orc in source slot 128 while Bowie lives. The leader outcome clears the surviving opposition. In both runs Peter first killed the High Priest in slot 129, received its held Shining Ball (item 16) into an empty slot and kept it through Continue. The drop requires a qualifying lethal action, the matching held item and clear item-custody bit 6. It makes no rarity roll. A full pack routes this rare item to Deals. Leader cleanup alone gives no receipt. Full-pack and cleanup cases have engine coverage. Using the Ball as an attack item remains outside this chapter. [Drop and cleanup behavior](../../tests/Sf2.Remake.Engine.Tests/EnemyItemDropSessionTests.cs) · [Full-pack and custody rules](../../tests/Sf2.Remake.Engine.Tests/EnemyItemDropTests.cs)

The first active member's final position and facing determine the return. The field adds (5,19) before rejoining the force. F419 clears, F469 stays set and F519 is set. The English and Chinese input sequences produced different positions.

| Run | Final Bowie position | First walking Map68 position | Save after one Right step |
| --- | --- | --- | --- |
| English | (16,14) Down | (21,33) Down | (22,33) Right |
| Simplified Chinese | (19,17) Down | (24,36) Down | (25,36) Right |

Use F10 → Save on the settled tile. Both saves hold 13,264 gold. A new process loaded each through title-screen Continue and walked Right once. The next serialized save changed only X and the step counter (9,999 to 9,998). This comparison includes names, members, levels, HP/MP, packs, equipment, both RNG words, flags, item custody, gold, berth and return town. The slots are the playable handoff for the next chapter.

## Other outcomes and equipment

- **Defeat.** Bowie's defeat returned the party to Map6 (11,22) facing Up with 4,587 gold from 9,174.
- **EGRESS.** Bowie's cast spent eight MP (15 to 7 at payment) and preserved 9,174 gold. The return refreshed his MP to 15.
- **Retry.** Both routes reset the raft to Map69 (42,15), retained F419/F469 and left F519 clear. Walk through Maps73, 72 and 69. The scroll into Map68 retains the field. Enter Taros Shrine, then leave it. Fresh Map68 admission starts Battle19 before another marked step. Both runs reached player Movement again.
- **Equipment.** From the old landing, use the field party's Equip menu to select Unarmed and save. Continue that copy and enter Battle19. Bowie's third pack slot still holds the unequipped Achilles Sword. The ordinary system menu saved this state at the first Movement input. On a separate run the battle Equip menu restored the sword in that slot and the party won. Another run kept Bowie unarmed and lost.

[Original outcome and return selectors](../../../docs/research/chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md) · [Map23 departure](../../../docs/research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md)

## Reproduce

The registered private handoff `handoffs/issue1834` includes the runtime package, bilingual catalogs, both old starting profiles, both victory saves, the unarmed field and battle slots, input drivers and captured state. Follow its `HANDOFF.md` from a prepared isolated checkout. Use a fresh sample name and disposable save namespace for each seeded run. Modes `village`, `win`, `escape`, `loss` and `restore` send input to the game and read its live observations. Compare serialized slots after a cold Continue with the included comparison. The two additional private audio cues use bounded original-driver captures. Their selection checks do not establish original hardware timing.

A [post-rescue repeat with Elric](battle20-elric.md#return-to-battle19-with-elric) reaches Movement and cold Continue with F519 set. Other return towns retain their individual admission boundaries. The [Harpy Pond chapter](harpy-pond-approach.md) continues from these saves. [Campaign scope and following stages](remaining-campaign-risk-baseline.md#chapter-handoffs)

## Return after Creed

**Confirmed** in the remake from the English post-Creed mine-approach save on Map68. F762 comes from the mansion Fairy interaction. On fresh village entry four residents occupy the later setup. The travelling Fairy is a separate follower. In the default setup, a Chinese after-Battle19 party with F13 clear met six residents and read messages 1697–1698. F13 belongs to that default conversation. [Creed producer](creed-recruitment.md#persistence-and-continuation) · [Original dispatch](../../../docs/research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md#supplied-f762-table)

| Ordinary input | Observed result |
| --- | --- |
| Talk downstairs, then take the stairs | The later old man delivered 1703–1704 in English. The party arrived upstairs at (57,3) Left with four residents. |
| At the priest choose Save, cancel, then choose Save and confirm | Cancel kept Map9 as the return town. Publication recorded Map23 and F399 with the player at (61,4) Up. |
| Cold Continue the same slot in Simplified Chinese | The upstairs party resumed with the later residents. The worker delivered 1702 in Chinese. The return stair landed at (21,8) Right. The south exit led to Map68 (48,31) Down, where another step advanced to (48,32). |
| Enter Battle19 from the marked zone and cast EGRESS | This party held F64, which excludes field EGRESS. Bowie's battle cast took his MP from 15 to 7 at payment. The party returned to the Map23 priest at (61,4) Up with four residents and 13,132 gold. Down reached (61,5). |

The private selection uses the existing setup, service and warp compilers. The USA source table `savepointmapcoords.asm` supplies the church anchor in both populations. Village saves keep the selected setup, actual actors and working layout through the existing settled-scene format. Engine coverage includes both populations and refusal of a scene whose setup disagrees with the saved flags. The later man's message 1701 has compiled coverage. His wandering prevented the optional native Talk attempt from reaching him. **Unknown** for the original includes natural reach, completed church outcomes and timing. Fairy Woods and Battle44 retain their separate admission boundary.

### Reproduce the post-Creed visit

Use the registered private input configuration, the pinned disassembly checkout, the private localization corpus, the #1941 stable package, the #1966 spell-icon preparation and the #1876 mine-approach slot. The private #1949 runner stages the navigation helpers and compiles only the village variant. It shares existing media where hard links are available. The new world is about 102 MB. The #1966 preparation is reused on subsequent runs. Run on the owning Godot checkout with its Debug adapter and imported project. Use a new candidate and new sample names for a repeat. [Private setup](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup) · [Locked adapter build](../development-and-verification.md#locked-net-workflow)

```powershell
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. ./local/private-inputs.ps1
$handoffs = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs'
$runner = Join-Path $handoffs 'issue1949/runner'
$upstream = Read-Host 'Pinned SF2DISASM checkout'
$corpus = Read-Host 'Private localization corpus'
$creedSlots = Read-Host 'Registered issue1876/slots directory'
$tag = 'village-' + [guid]::NewGuid().ToString('N').Substring(0,8)
& pwsh -NoProfile -File "$runner/Rebuild.ps1" -Repo (Get-Location).Path -Upstream $upstream -Corpus $corpus -Candidate $tag
$play = './local/issue1949/Play.ps1'
& pwsh -NoProfile -File $play -CandidateName $tag -Task default -Language zh-CN -Sample "$tag-default" -Seed "$handoffs/issue1834/slots/after-battle19-zh-CN.json"
& pwsh -NoProfile -File $play -CandidateName $tag -Task later -Language en -Sample "$tag-later" -Seed "$creedSlots/mine-approach-en.json"
$slots = Join-Path (Split-Path -Parent $env:SF2_SHARED_INPUT_ROOT) 'save-validation/issue1949'
& pwsh -NoProfile -File $play -CandidateName $tag -Task cold -Language zh-CN -Sample "$tag-cold" -Seed "$slots/$tag-later/search/sf2-opening/slot1.json"
```

Read `entry.json`, the text observations, `church-cancel.json`, `church-saved.json`, `paid-egress.json` and `egress-return.json` under the corresponding worktree-local sample. Check the displayed locale during each text wait, the anchor before and after publication, and the player projection after EGRESS. The completed cold return exited 0. A separate run recorded the following step and then exited with 0xC0000005 during Godot finalization. The native teardown issue remains open under [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).
