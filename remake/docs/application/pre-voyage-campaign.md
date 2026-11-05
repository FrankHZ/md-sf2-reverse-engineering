# Pre-voyage campaign

The [capability ledger](../capability-status.md) records playable support. This agreement tracks the
handoffs required by [the campaign issue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1535).
Cross-chapter acceptance is **High** because encounter selection, field reload, recruitment and
saved progress share party state. A local rule or content change uses **Standard** scope.

## Session and handoffs

One ordinary title New Game selects an immutable world and starts the party from source definitions.
Continue selects that world independently and restores progress at an admitted safe point. Each
battle consumes the current joined/active party, HP/MP, status, levels/EXP, spells, equipped slots,
gold, custody, accounting and both RNG streams. Apply each source mutation once. Capture the map,
setup, placement/facing, egress register, result and next accepted input at each handoff.

| Stage and current acceptance boundary | Field, encounter and party transfer | Reachable interactions and verification |
| --- | --- | --- |
| Opening to Yeel. Confirmed in one ordinary session through Battle03. | Map3 → castle/tower → Battle01 return → commission and Jaha → Map66 Battle02. Victory hands the current field party back on Map66. Yeel's conversation sets F72 for Kazin following. A legal Map66 request consumes encounter state to enter Battle03. | Retain opening [search, dialogue and Church](../content/opening-branch-inventory.md), refusal/acceptance, ordinary Battle02 defeat and paid retreat/retry, Yeel conversation, repeat visit and the source-eligible encounter without visiting Yeel. |
| A · Battle03 outcomes, Hawel recruitment and Battle04 observed. | Battle03 victory clears F403, sets F503 and uses the first party member's final tile plus origin. Defeat restores Bowie, halves gold and uses egress. Paid escape takes its own egress return. Map66 (6,5) enters Map43 (6,11) Up. Default setup runs the house scene; F612 selects the revisit. Join Kazin (F4, active subject to capacity), follow, clear F72, set F404. Caller sets F612/F650, clears F72 and writes egress 43. | Ordinary input played the house scene, inspected the diary, exited and revisited the F612 setup in the three-encounter world. The level-4 mage retained his generated stats, equipped rod and herb. House init has no F503 or F72 gate. Battle04 exercised Kazin BLAZE with the selected enemy recipient, and Galam Archer attacks from distance 2, with result messages and resumed turns. |
| A · Battle04 outcomes and prison entry observed. | First victory stages Lemon's capture warp, executes the shared Bowie join tail, clears F404 and sets F504 before returning Map17 (0,0) Right. Completed victory skips capture and uses the saved ordinary victory return. Special defeat restores Bowie and halves gold, clears F404, sets F504 and overrides the egress map to 17; retain egress coordinates/facing. With egress 43 these are (7,5) Down. Legal EGRESS/Wing consumes its actual resource and returns to the egress map; first escape leaves F404 armed. | First victory and special defeat reached their distinct Map17 entry operands, then the first text 880 input. Paid EGRESS returned to Map43 and allowed re-entry. Stage B owns the remaining F660-gated prison program and subsequent interactions. |
| B · Captivity through Battle06 admission. Pending. | Early Map17 wake (F660), doors (F661), warning (F662) and Slade following (F73) → Map16 rally (F663), guard request (F405/F664) → Battle05. Its first victory returns Map16 (16,18) Down, recruits Slade, clears F73 and unlocks Battle06 (F406). Carry the actual jewel and paid purchases into the Map66 board. | Speak to prisoners/companions, maid and Slade, use doors, stairs warning and passage; check guard first/repeat, compatible purchased equipment and paid supplies carried onto the board. |
| C · Battle06 through Battle07 tower return. Pending. | Battle06 victory clears F406, sets F506 and returns on Map66. Map3's changed setup offers Kiwi (joined F6, active F38 subject to capacity). Battle07 admission uses F407. First victory sets F74/F640 through its scene and clears F407/sets F507 through the caller. Preserve Slade, Kiwi and royal followers in their separate roles. | Exercise join and bypass for Kiwi, real changed equipment, enemy support and reached statuses, costs, deaths and resumed control. |
| D · Tower aftermath through departure. Pending. | The Map57 tower return carries F507 and F74/F640. The castle's Map3 request selects quake Map4 on F640. Map44 departure clears F74/F640 and requests Map73 (26,13) Right. Admit the first stable successor control. | Check ordinary interactions, refusal/acceptance where offered, and fresh-process Continue from an existing legal point. Stage ends before Battle08. |

For the later handoffs consult the [Battle05 tail](../../../docs/design/synthesis/chapters/grans-opening/battle05-outcomes-through-battle06.md), [Battle06 return](../../../docs/design/synthesis/chapters/grans-opening/battle06-outcomes-through-battle07.md), [Battle07 aftermath](../../../docs/design/synthesis/chapters/grans-opening/battle07-outcomes-and-aftermath.md) and [harbor choice](../../../docs/design/synthesis/chapters/grans-opening/earthquake-town-through-docks-departure.md).

The [source chapter guides](../../../docs/design/synthesis/chapters/grans-opening/battle03-outcome-through-battle04.md)
and [Battle04 outcome table](../../../docs/design/synthesis/chapters/grans-opening/battle04-outcomes-through-battle05.md)
own the conditional original rules. Natural original traversal, caller state and timing remain
**Unknown**. Keep ordinary-play observations and controlled variations separately identified.

## Acceptance and persistence

Use Normal, 1080p and 96px cells. Read actual running input, board/field and actor/target projections,
resources, next accepted input and errors. The [chapter presentation matrix](../evidence/opening-ui-gaps.md#pre-voyage-implementation-order)
identifies required bindings. Reuse the sound, physical, ranged, healing and fire consumers.
Keep private art and audio in the selected local package. Retain one small record per handoff.

Existing save eligibility covers the admitted default Map3 recipes and settled Battle01 Map57 return.
Save/Quit and Continue must round-trip a legal point with this selection. Restore verifies package,
member identities, membership flags, gear, resources and the egress register. New egress values
require explicit admission independent of location eligibility. Battle suspend, arbitrary field
saves, Map16/17 save recipes and high-level promotion are outside this delivery.

## Preparing the ordinary selection

Use the [opening inputs](../content/profiles-and-trust.md#opening-new-game-admission) for root
Battle01 and title content. The [ten-map selection](../../content/private/pre-voyage-opening.json)
admits Yeel (Map5), Hawel's house (Map43), Map17, the Map66 encounter requests and the Battle02/03/04 boards and outcomes.
The producer binds each map's two base palette words from the verified ROM. Both ordinary readers
load the same world. The package keeps its historical ID and uses gameplay revision 2 for the five generated members.
To Continue an existing four-member V1 slot, select its previously prepared world via
`SF2_PRIVATE_EXPLORATION_CONTENT`. To rebuild it with the current producer, obtain the revision-1
selection using `git show 158ee4528a86d2ffbab091023f7d1e40673c84ad:remake/content/private/pre-voyage-opening.json`
and supply the same four-member source/audio inputs in a separate ignored directory. Choose a
separate slot for a five-member New Game. The established [compatibility rule](opening-session-lifecycle.md#format-compatibility-and-local-storage)
checks the selected revision before restoring progress.

Reuse the [Battle02 input recipe](../verification/field-programs.md#prepare-the-selected-inputs)
and [Battle03 materials](../verification/field-programs.md#yeel-guide-through-battle03). Under one
fresh ignored output directory, place the three small encounter exports and their field-death scenes
in separate subdirectories. Place one copy of the shared trusted static, enemy and gold inputs there.
In the encounter list, supply one row per additional battle, ordered 02, 03, 04. Each row has
`placement`, `scene`, `terrain`, `static`, `enemies`, `gold` and `battleScenes` paths relative to the
world's parent. Product rows omit the diagnostic party checkpoint. Keep the root Battle01 inputs
and the preceding immutable exports read-only.

From the repository root with the configured private inputs loaded, use the accepted presentation
manifest, the nine reviewed additional command/timer candidates from the opening and Yeel recipes
(59/BD, 6E/CC, 6F/CC, 41/CC, 43/CC, 4A/CC, 41/C2, 43/C2, 01/C2 in hex), and the accepted authored
defeat and encounter cues. Include the three [Hawel candidates](#hawel-media-prerequisite) and
the [four timer-specific field effects](#yeel-warp-and-hawel-speech-variants). Pass each candidate once.

```powershell
$audioArgs = foreach ($candidate in $reviewedCandidates) { '--audio-candidate'; $candidate }
uv run python -m sf2tool.remake_exploration_content --canonical $canonical --upstream $upstream `
  --rom-path $rom --presentation-root $assets --selection remake/content/private/pre-voyage-opening.json `
  --encounter-inputs $encounterList --output $freshWorld @audioArgs
$env:SF2_PRIVATE_EXPLORATION_CONTENT = $freshWorld
```

Launch the ordinary title without diagnostic start arguments. Use an external disposable save
namespace as in the [save recipe](../verification/host.md#opening-save-and-continue).
The measured world is 108,998,786 bytes. Keep the unchanged 01/C2 main-theme WAV and its candidate
in `audio-01-c2` beneath the world's parent, alongside the three Hawel directories. Supply its
candidate once. These four referenced WAVs total 26,376,328 bytes. The selection also requires
the seven additional inline field cues below. Missing selected cues reject preparation.

## Ordinary New Game through the Battle03 return

**Confirmed in the remake.** At Normal, 1920×1080/96px, start at the title, join Sarah and Chester,
play Battle01, walk through the castle commission and recruit Jaha. Win Battle02 and enter Yeel
from Map66 (19,23). Talk to Kazin near (7,7), leave by the north exit and take the Map66 zone
at (16,21). The same session mounts Battle03 with the four carried members and real enemy board.
F72 selects Kazin as a field follower. The house scene owns his eventual recruitment.

| Actual ordinary inputs | Observable state and next control |
| --- | --- |
| March out after the Battle02 win, visit Yeel, talk to Kazin and trigger Battle03 | F502 and F72 set. Battle03's fresh introduction selects Yeel for egress. Spell and item retreat bind on the selected outcome route. The retained controlled two-encounter Yeel recipe without a Battle03 return keeps its earlier boundary. |
| Select Bowie's learned EGRESS and confirm its target | 8 MP → 0 at the paid text. Effect/close then eligible field recovery return to Map5 (11,6) Up. Gold 250, F403 set, F503 clear. Leave the church through its door and the north village exit for fresh Battle03 player control. |
| Move Bowie into danger on the retry and fight to ordinary defeat | Party returns to the live Yeel savepoint. Gold 250 → 125, Bowie restored and his defeat count increments. F403 stays set and F503 clear. Subsequent attempts included real Church Raise, earned EXP/gold, legal escapes and walking re-entry. |
| Defeat the remaining enemy | The first active member lands at the final tile plus the Battle03 area origin. Observed Map66 (18,21), F403 clear, F503 set, F72 set, four members, 100 gold. Ordinary movement reaches (6,6) outside Hawel. In the retained four-member selection, Up toward (6,5) gives the destination-unavailable panel. The nine-map selection enters the house. |
| Field Save at the earlier legal Map57 Battle01 return, then fresh-process Continue | The Map57 anchor (8,7), four stored member records, flags, 360 gold and both RNG streams matched; the resumed field accepted movement. At the later Map66 return the save offer gives `save-location-unavailable`. The Hawel house adds no safe save location. |

Numbers describe one run. The victory rule counts factions and Bowie's survival. F503 supplies
neither a house-init predicate nor a fixed coordinate. A living ally's supported sleep clears on
return recovery; a sleeping casualty retains its status until recovery that includes the dead.
The source chapter and [recovery contract](../../../docs/design/contracts/battle/battle-control-lifecycle.md)
retain the original-game boundary. Other unsupported status refreshes remain explicit.
With F399 set, the pinned `getegresspositionforbattle.asm` and `egressinit.asm` look up the
current EGRESS_MAP. The engine uses the live map's registered anchor, including a declared
map zero. The admitted opening has no zero anchor, so early defeat at zero uses the starting
map's declared savepoint (Granseal). This opening fallback applies only to defeat. Controlled
starts without a register use their route default. An unsupported live value gives `egress-map`.

On the ordinary route F453 starts clear. Unsuspended entry executes `bbcs_03` →
`csub_49816` (the pinned `battle03/cs_beforebattle.asm`), which writes Yeel before the start
wrapper sets F453. This write has no follower predicate. Re-entry with F453 set skips before and
inherits the existing register. Before the first house visit the only reachable egress writer in this selection is the Battle03 before
program. A first ordinary Battle03 fight with the reset register was not observed. A supplied
intro-seen state is a separate case. New Game and V1 Continue bind the same immutable opening fallback.

## Ordinary Hawel house and Kazin recruitment

**Confirmed in the remake.** Select the nine-map private world and use the ordinary title New Game
on Normal at 1920×1080 with 96px cells. The initial party data includes Kazin's source MAGE class,
level 4, equipped Wooden Rod (word 213), Medical Herb (0), and BLAZE 1 (11). His source growth
uses the main RNG during New Game. The party flags control when he joins and can deploy.

Save at the settled Battle01 return. In one run fresh-process Continue restored Map57 (6,16),
all five records, 360 gold, main RNG 2428264540 and thinking RNG 2423344417. The restored party
commissioned Jaha, fought Battle02, talked to Kazin in Yeel, recovered after defeat, won Battle03,
and walked to Map66 (6,6). Up entered Map43. The same session established these boundaries:

| Action | Result |
| --- | --- |
| Enter with F503 and F72, through (6,5) | The default setup played texts 811–834. At the fixed join text 446, F4 was set and F72 still represented the follower. On return F36/F404/F612/F650 were set, F72 was clear and egress was 43. The existing five records, equipment and progress survived the membership change. |
| Observe the house scripts | Both doors opened. Hawel alternated visibility at his original coordinates before moving off-map. Command 254 stopped current music immediately. Enemy music, impact 100 and sad theme 16 played. The finite sad join 20 ended before previous music resumed. |
| Search the bookshelf from (3,3) facing north | The source diary description appeared and dismissed back to field input. Exiting south returned Map66 (6,6). Re-entry selected F612, kept the single enrollment and returned control without replaying the scene. A second exit retained 149 gold and the joined party. The later Map66 save offer returned `save-location-unavailable`. |

The three-encounter selection ends here. With F404 armed the source caller selects Battle04 on
Map66. These exit/revisit observations establish the field content in the current selection.
The Battle04 continuation is recorded below. The earlier four-member world still restored its
original Map57 slot in a fresh process. Selecting revision 2 for that slot showed the matching-content message and preserved
the slot bytes.

**Confirmed source structure.** In the pinned `mapscriptengine_1.asm`, csc43 searches by roof
coordinate and csc47 calls OpenDoor. Field-marker predicates belong to ordinary movement. At
(4,9) a door marker and a roof table record coincide. A focused entry using the same cs_540C0
observed both door markers change from 0x0400 to 0x0800 and active roof record 1. The diagnostic
changed only the route to the house and the follower setup. csc2D_D copies the 34-byte eas_Jump
body before continuing the entity's action sequence. csc17 separates each of 31 two-tick hidden
intervals with visible delays, then branches to csc19 to place the actor. Original natural
traversal, exact visual timing and audio hardware phase remain **Unknown**.

A separate ordinary Battle02 win exposed a return refusal. Bowie finished at board (16,13),
terrain 1. Adding origin (16,19) selects Map66 (32,32), whose walking word is 0xC16B. The
[Battle02 return owner](../../../docs/design/synthesis/chapters/grans-opening/battle02-outcome-through-battle03.md#the-return-position-comes-from-the-battles-final-state)
records the source operands. The engine places the party at the sampled combatant coordinate
and retains the walking collision flags. The four adjoining tiles in this sample are blocked.
The targeted engine regression `OverworldVictoryKeepsTheCombatantTileEvenWhenFieldWalkingCannotEnterIt`
separately covers a passable-neighbor variation and the real center/four-neighbor collision words.
The latter returns successfully but all four movement inputs remain blocked and preserve the party
and flags. Successful placement does not establish usable exploration. The investigation and proposed
recovery boundary follow below; natural original recovery remains **Unknown**.

### Enclosed Battle02 return and field EGRESS

The [enclosed-return issue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1552)
uses **Standard** scope: distinguish a source/import/coordinate error from a missing field action
before changing gameplay. The independent diagnosis is complete at the engine/state boundary.
Recovery implementation and native input acceptance remain pending main-gate's scope decision.
Stage B does not resolve this earlier playable risk.

**Confirmed in the selected inputs.** Against the pinned source above and the verified US ROM,
Map66's decoded 4096-word layout equals the ordinary ten-map world's layout word for word.
`data/maps/entries/map66/0-blocks.bin` and `1-layout.bin` equal ROM ranges
`0xBBDD8..0xBCB9A` and `0xBCB9A..0xBD1F0` (end exclusive).
Battle02's compressed terrain equals both its imported input and ROM `0x1AD460..0x1AD5AC`;
the existing stack decoder gives terrain 1 at `13 * 48 + 16`. The admitted origin is `(16,19)`.
No layout replacement, field/battle coordinate disagreement or selected terrain import mismatch
explains this location.

| Field position | Walking word |
| --- | --- |
| Center `(32,32)` | `0xC16B` |
| West `(31,32)` | `0xC16A` |
| East `(33,32)` | `0xC16C` |
| North `(32,31)` | `0xC164` |
| South `(32,33)` | `0xC177` |

All four diagonal neighbors also have collision bits `0xC000`. None of these nine cells has a
door, warp or zone marker. The Granseal warp at `(30,32)` is two cells west, behind the blocked
neighbor. Map66's single area spans `(0,0)..(63,63)` and its init directly returns.

**Confirmed source structure.** `code/gameflow/battle/battleloop_2.asm:BattleLoop_Victory`
samples the final first active member and adds the area origin. The regular field controller is
`code/common/scripting/entity/entityscriptengine_2.asm:esc02_controlCharacter`:
`loc_50BC` gates map collision with entity flag bit 6; `loc_52C0` compares the destination against
`0xC000`. Its bit-15 stair candidate can also inspect a diagonal from this center; those
candidates are blocked too. `DeclareNewEntity` in `entityfunctions_1.asm` sets the collision
flag. The source debug+B bypass is not ordinary player input. Battle terrain and field collision
therefore have distinct responsibilities; admitting the combat projection must not clear collision.

**Confirmed in the current engine.** A focused controlled outcome used the unchanged ordinary
world, existing production assemblies, the actual `(16,13)` combatant coordinate and source route.
The complete outcome program returned Map66 `(32,32)`, `PlayerInput`, F402 clear/F502 set and
egress Map3. Each separate `Move` through `ExplorationDispatcher.Submit` emitted
`movement-blocked`; `WaitAtInput` stayed in exploration at the same position. The live interaction
query returned none, and field search was unavailable. Bowie knows `SpellRef("egress", 1)`,
but `FieldInventory.Query` offered no spells and `FieldCommands.Submit(FieldCast(...))` returned
`field-spell-unsupported`. Gold 102 and the selected main/thinking seeds 1535008860/24202529
survived return. This is a controlled boundary, not reconstruction of the entire recorded party
or a new ordinary-route/native observation. The retained real failure remains independent evidence.

Controls using the same real maps admit east/south movement from Map66 `(19,24)` and Map3
`(32,13)`. They establish that the closed return is local, not a global movement rejection; they
do not establish travel from the enclosed tile. The existing retreat and outcome tests retain
their live-anchor and resource contracts. Run the focused behavior tests under the locked SDK:

```powershell
# In remake/, after loading the owning worktree's private-input/tool configuration.
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN test tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --no-restore --disable-build-servers `
  --filter 'FullyQualifiedName~Battle02OutcomeTests|FullyQualifiedName~BattleRetreatTests|FullyQualifiedName~ExplorationSessionTests'
```

For the private input comparison, use the existing `sf2tool.h2.map_layouts.decode_map_blocks`
and `decode_map_layout`: decode `0-blocks.bin`, pass `len(block_words) // 9` when decoding
`1-layout.bin`, then compare all words with the flattened selected `world.maps[id=map-66].layout`.
Use `sf2tool.compression.decode_stack_compressed(..., expected_output_bytes=2304)` for the
named terrain input. Compare the compressed inputs directly with the named ROM slices before
interpreting either result. No full world diagnostic copy or whole-game acquisition is needed.

**Confirmed source recovery mechanism.** `code/common/menus/main/mainactions.asm`,
`@IsOnOverworldMap` / `@Egress`, admits EGRESS on source maps 66 through 78, obtains its MP
cost, calls `DecreaseCurrentMp`, flashes, calls `GetSavepointForMap(EGRESS_MAP)` and queues
map event 1. It does not halve gold or undo the victory flags. There is no sufficient-MP check
in this caller; `combatantstats_2.asm:DecreaseCurrentMp` uses `DecreaseAndClampByte` with
zero as its lower bound. `code/common/maps/egressinit.asm:GetSavepointForMap` and
`data/maps/global/savepointmapcoords.asm` select Map3 `(32,13)` Up when F399 is set and
the live egress map is 3. Its pre-F399 and raft branches are separate source conditions.
Original natural execution from the sampled enclosed tile, exact visual timing and onward travel
are **Unknown**; static availability is not an observed escape.

**Proposed minimum implementation, not accepted capability.** Admit field EGRESS for the
selected source overworld maps through `PrivateFieldPreparation`, expose it through the existing
Magic confirmation, and extend field command execution with the source MP payment and ordinary
flash/warp continuation. Resolve the live egress register through existing admitted savepoint
operands; do not choose a nearby tile, change the map, replay the battle outcome, or branch on
Battle02's number. Preserve gold, inventory, EXP, accounting, RNG and victory flags. Validate the
destination before payment; distinguish paid departure from subsequent ordinary entry recovery.
The bounded target is early walking mode with F399 set and supported live egress destinations.
Pre-F399, raft/Caravan and unbound destinations must remain explicit until admitted.

This affects Content field capability binding, Application field recovery/command and map-transfer
continuation, and the existing Godot field-action presentation. It needs behavior cases for low/zero
MP, cancellation/stale input, live destination changes, payment once, a normal accessible departure
and rejection before payment when unsupported. Non-overworld source EGRESS has no effect; retain
that distinction. Angel Wing shares the original destination path but also removes an inventory
slot and falls through the shared spell-entry code; its exact payment operands need their own check
and are not necessary to unblock a living Bowie with EGRESS.

Acceptance remains ordinary menu/input at this exact position, return with retained progress,
then re-entry and travel toward Yeel in the unchanged world, alongside a usable victory position
and legal paid-retreat control. Use a bounded native observation on the existing runtime after
implementation; do not replay the whole campaign. Until that passes, this issue is unresolved and
the enclosed return is not a playable-success claim. Retain the earlier ObjectDB exit **Unknown**.

## Battle04 and the Galam prison handoff

**Confirmed in the remake.** Continue the existing five-member Map57 save, complete the ordinary
Battle02/03 and Hawel route, then leave Map43 with F404 armed. This selects the source Battle04
board with five carried members, eight enemies, three AI regions and one AI point. The observations
use Normal, 1920×1080 and 96px cells. Original natural reach and hardware timing remain **Unknown**.

| Actual input and outcome | Observed handoff |
| --- | --- |
| Cast EGRESS, acknowledge its paid action and leave Hawel again | Bowie pays 8 MP. Map43 (7,5) Down retains 149 gold, F404 armed and F504 clear. Eligible field recovery restores his MP. The next exit mounts fresh Battle04. |
| Fight with the carried party and defeat every enemy | Kazin casts BLAZE1 at Archer slot 5 from distance 2, MP 13→11 and enemy HP 15→9. Archer slot 5 attacks Jaha from distance 2 for 1 damage. The acting/recipient HP/MP cards, modern fire effect and result messages update through the action. Later turns and rewards continue. Kazin finishes the leader with BLAZE and gains level 5/BLAZE2 through ordinary growth. |
| First victory | The real after-battle capture program runs, its caller joins Bowie, clears F404 and sets F504. Incoming Map17 is (0,0) Right with 509 gold and the carried items, EXP, levels, accounting and RNG. |
| Fight to Bowie's defeat on another real Continue route | Gold 149→74, Bowie HP 12 and his defeat count 6→7. F404 clears and F504 sets. Incoming Map17 is (7,5) Down from live egress 43. Sarah remains dead at this boundary. Earlier Huge Bat attacks rendered Kazin as the recipient with mage resource `ally4/frame0`, HP13→7→1 and resumed turns. |
| Both Map17 arrivals | Default initialization selects the actual 12-record population, resets party battle stats, loads its scene and presents text 880 at Bowie (59,4) Up. `FieldTextWait` accepts text input, with no session failure. Sarah's later recovery belongs to that source reset. |

The handoff stops at this first prison dialogue. Stage B owns the remaining program, F660, doors,
Slade and subsequent field control. The later `setDest` instruction remains explicitly Unsupported.
Engine variations also verify completed-victory capture skipping, distinct live egress coordinates
and caller-tail ordering before target initialization. These variations use authored states.

Prepare Battle04 with the pinned source and verified registered ROM under one new ignored directory.
The root Battle01 materials and existing two encounter inputs remain the shared immutable inputs.

```powershell
uv run python -m sf2tool.remake_exploration_battles --upstream $upstream --rom $rom `
  --battle 4 --output (Join-Path $bundle 'inputs/battle04')
uv run python -m sf2tool.remake_battle_scene_content --upstream $upstream --rom $rom `
  --battle 4 --output (Join-Path $bundle 'scene04-base')
uv run python -m sf2tool.remake_battle_scene_content --upstream $upstream --rom $rom `
  --field-death-base (Join-Path $bundle 'scene04-base/battle-scenes.json') `
  --output (Join-Path $bundle 'inputs/scene04')
```

Append the Battle04 encounter row using the shared static/enemy/gold paths and the new placement,
scene, terrain and field-death scene paths, then run the ordinary world recipe above. Content pins
placement `774A4F283268C61B1731D2E544C5ABF1C9382487D3EFA9F37E92CF44FFDB44DA`,
scene `C0F6BE4E0EA9324355DD9207FBF2CE927A65064EAB0C58E904B3423A79686E06` and the
existing BattleTerrain03 identity. The base scene export measured 1,209,226 bytes and its field-death
scene 287,759 bytes. The additional actors use the existing physical, ranged and fire consumers.

Keep one uncompressed final world and reuse the already reviewed WAVs. Budget about 109 MB for the
world, 2 MB for the additional small exports and at most 15 MiB for observations, retaining at least 1 GiB
free through writes and builds. Observed game peak working set was 1,555,095,552 bytes. An initial
compressed-file write consumed more allocation than its sample predicted and crossed the floor.
The run stopped and removed only its newly generated world, then measured a plain-file sample and
wrote the final once with a live free-space guard. Prior inputs, reports and saves were retained.

The first native battle exposed a gradient-route validation failure. Pinned
`code/gameflow/battle/battlefield/buildmovestringfunctions.asm` (`alt_BuildMoveString`, `loc_DE14`)
retains direction bits while lowering its threshold. The chosen path may cost more than its propagated
destination value. The validator admits that exact source walk when its destination is in budget,
while rejecting an extra detour. A small authored terrain variation covers this behavior.

Run `Battle02OutcomeTests`, `SourceEnemyAiTests` and the affected outcome/movement/map/scene/save
behavior tests under the locked SDK environment, plus `uv run sf2 verify adapter`. Existing
`PrivateBattleOutcomeProgramTests` use the controlled two-encounter world and now select the opening
encounter explicitly and enter through the ordinary battle initializer. Their original two `Single()`
failures, the mismatched metadata/product-world attempts and their focused passing reruns are retained.
The ordinary four-encounter reader and native route above supply the product acceptance.

## Hawel media prerequisite

**Confirmed:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
Map43 `2-areas.asm` and `cs_540C0` select field music 16, enemy music 5, impact 100,
sad music 16 and the finite sad join 20. The first Map43 visual preparation with only the
opening candidates rejected 16/20/100. The ordinary selection now pins these three reviewed
WAVs, their timer contexts and loop/finite profiles. The three Hawel external files total 10,827,808 bytes. The main theme also uses a contained WAV. External music has its own disk
and resident-memory cost. The 128 MiB world and 4 MiB scene limits apply to the actual JSON files.

### Private preparation

Use the registered US ROM (`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`)
and the [existing isolated audio export](../verification/field-programs.md#audio-selection-and-evidence-boundary).
The retained inputs are audio-only BizHawk 2.11.1 Genplus-gx wave dumps. Command 20 was the pilot.
The two finite observations use the documented 180-frame setup, music 5, stop 254 and quiet pre-roll,
then dispatch at frame 243. All channels became inactive at 896 for music 20 and 246 for SFX 100,
followed by 60 quiet frames. The latter's only active stream is type 1 PSG tone 3,
Z80 `1A33..1A3F` (end exclusive), first cursor `1A3B`. Its inherited timer comes from
`music05.asm`. The music headers in `music16.asm` and `music20.asm` select their own timers.
For 16, stop at 180, advance 60, dispatch at 241. Channel 0's cursor returned to `E520` at
2842 and 5441. The observer restored its saved core state after each bounded observation.
Dump lengths for 16/20/100 were 6400/1400/700 frames respectively, with `--dump-type=wave
--dump-close --audiosync=false`. Each launch verified the ROM and registered installation.

| Cue | Command / Timer B (decimal) | Capture SHA-256 | Capture interval [start, end) | Runtime loop [start, end) |
| --- | --- | --- | --- | --- |
| MUSIC_SAD_THEME_1 | 16 / 209 | `6BB7799A51784D2F61C8D0855859B0C73304DC0050F9B3EC42891FBB426F48E5` | 178888, 2397330 | 305879, 2218442 |
| MUSIC_SAD_JOIN | 20 / 205 | `C1E7D03E7B5A24E0E4B5A76DEB47134A75246CC56581CC105C27FAEB0B6E4385` | 179448, 661506 | Finite |
| SFX_BLO | 100 / 204 | `37231BC684B47834436178E0D634FE7051662AED2D92DB09A50EE3AFEF2634EF` | 179532, 185951 | Finite |

All offsets count stereo sample frames at 44,100 Hz, PCM16. The finite cuts retain 441 samples
on each side of the first/last above-one-LSB sample in the isolated interval. **Inferred:**
the modern loop for 16 uses the median of five local waveform recurrence lags (1,912,561–1,912,565
samples) near the observed cursor period and a low-amplitude seam. The full introduction and
one repeating passage remain in the runtime WAV. For 20 the selection chooses modern end step
710 from channel 0's 709 note/wait units plus its terminal step, under the
[existing two-gate policy](../../../docs/design/contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy).
Original interrupt phase and hardware timing remain **Unknown**.

To reproduce from each retained capture, set the values below from its row. Supply
`--loop-begin 305879 --loop-end 2218442` only for 16. The new candidate directory must
be a direct child of the selected world's directory. Keep its WAV there when moving the bundle.

```powershell
$dir = 'audio-{0:x2}-{1:x2}' -f $command, $timer
$loopArgs = if ($command -eq 16) { '--loop-begin', '305879', '--loop-end', '2218442' }
uv run python -m sf2tool.remake_audio --capture $capture --expected-sha256 $captureSha256 `
  --command $command --timer-b $timer --cue $cue --start-sample $start --end-sample $end `
  --output (Join-Path (Split-Path $freshWorld) $dir) @loopArgs
```

Pass each generated `candidate.json` to the ordinary producer above. It checks the source
command, reviewed timer, WAV digest, PCM shape and loop interval. A private world row marked
`runtimeWav: true` selects only `audio-cc-tt/command-cc-timer-tt.wav` beside that world
(two lowercase hex digits each). Content checks both path components for links, the actual file
length and canonical 44-byte PCM header before allocating samples, then checks the PCM digest.
Legacy inline worlds and the authored and title readers retain their existing inputs.

### Consumer observations

**Confirmed:** the ordinary package and a separate three-cue diagnostic used identical selected
audio rows. For the diagnostic, compile Map43 alone from the pinned Map43 setup files
(`compile_maps(..., only_maps=[43])`), binding the source compound join selector to the bitwise
OR of its equates in diagnostic isolation. `prepare_visuals` with the three candidates and
`audio_cues=("MUSIC_ENEMY_ATTACK", "MUSIC_SAD_THEME_1", "MUSIC_SAD_JOIN", "SFX_BLO")`
produced the 128×320 native atlas, field music 16 and all four rows. Supply the ordinary
`runtimeWavCues` as `wav_profiles` and the absolute world directory. In an ignored copy of the
eight-map world, include this map and its sprites/palette. Route the Map3 (54,3) warp to Map43
(6,11) Up and use an authored on-load sequence in place of the house events. After ordinary
New Game step left twice. The sequence waits 30 ticks before each of the following Sound requests:
5, 100, 16, 20. Then run SoundWait, PreviousMusic and End.

At the running Godot host, `ReadAudioObservationJson()` reported area music 16, existing
enemy music 5 and SFX 100 under requested/recorded timer 204. SFX 100 and music 20 each
finished once. Early input during the completed logical wait left the simulation tick unchanged.
After the actual finite finish, 16 restarted once, left moved the player from (6,11) to (5,11),
and playback wrapped from 50.25 to 6.97 seconds without another start/finish receipt.
Separate title New Game with one changed PCM byte in a copied 20 WAV returned `audio-identity`
before mounting a session. The ordinary selection now uses the reviewed
command 1/C2 candidate with its own timer, digest and loop interval.
Its WAV and 16's WAV total 24,422,332 bytes. The actual producer and New Game admitted both.
Peak game working set in that variation was 1,392,295,936 bytes. The known ObjectDB exit warning
remained visible in the successful samples.

### Yeel warp and Hawel speech variants

**Confirmed:** the incoming Yeel warp requests command 89 while the overworld main theme owns
timer 194. Without the exact variant the running host reported `audio-command-ambiguous` at the
warp fade. The current variant played through the transfer. Map43 soldiers and Hawel (sprites 66
and 189) select speech 74; Kazin's ally sprite 11 selects 73. These three additional speech variants
are selected in the house package. The ordinary scene completed its dialogue and finite join music.

Use the finite audio-only recipe above with the pinned SFX command's actual stream pointers.
For timer 194 start music 1, for 209 start music 16, and for 204 start music 5. Stop with command
254, retain the quiet pre-roll and dispatch the selected SFX at frame 243. Each 700-frame dump
observed channel completion and at least 60 frames of tail. Keep 441 samples of unmodified PCM
on either side of the non-quiet interval. Use the table's capture digest and sample interval in
`sf2tool.remake_audio` without loop arguments. Pass each resulting candidate to the world producer.
74/204 reuses the accepted opening asset.

| Cue | Command / timer | Capture SHA-256 | Cut [start, end) | All inactive (frame) |
| --- | --- | --- | --- | --- |
| SFX_WARP_TIMER_C2 | 89 / 194 | `59D67B55BCA5DA258EFDFF403DF8718F0314E0592CDCF1D771294D9FC513A22E` | 179377, 206878 | 288 |
| SFX_DIALOG_BLEEP_5_TIMER_D1 | 73 / 209 | `E5EB86FEB1A3CE0B17377A3D3C8F38E202A9684A9E9BFD89393DEDD04597C84F` | 179496, 187096 | 248 |
| SFX_DIALOG_BLEEP_6_TIMER_D1 | 74 / 209 | `1E99C7A475AEC7A7CD6B74AEB3CA3B0501BF7FDC74018404470CCCEF10FC56F9` | 179495, 187076 | 248 |
| SFX_DIALOG_BLEEP_5_TIMER_CC | 73 / 204 | `F76B0BCFD98954FAC9D7A46AA1ED6975E77665598883B42061C93FA915712F37` | 179508, 187362 | 248 |
| SFX_MENU_SWITCH_TIMER_D1 | 65 / 209 | `7BC37685700EF1FC057C5F742A29D8671500B7E31A0CC7B878372DCB97FC8AE1` | 179602, 191946 | 257 |
| SFX_VALIDATION_TIMER_D1 | 67 / 209 | `151A14F883EFF52CCA6CDBA27775583C53AF9101437C2FD43B51BF6FE4998CDE` | 179508, 202961 | 271 |
| SFX_WARP_TIMER_D1 | 89 / 209 | `1B3AE55D708CE17FD06A5603754441C1143FC349C0B991820B195547E8509DA8` | 179656, 201405 | 277 |

Pinned `terrainentries.asm` binds Battle04 to BattleTerrain03. The producer follows that pointer.
Map17's default population falls through from five records into seven shared records under the
following label. The producer joins them by canonical record address before checking cardinality.
