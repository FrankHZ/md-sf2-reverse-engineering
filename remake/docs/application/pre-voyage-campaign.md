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
| Opening to Yeel. Ordinary opening and selected later segments have separate witnesses. | Map3 → castle/tower → Battle01 return → commission and Jaha → Map66 Battle02. Victory hands the current field party back on Map66. Yeel's conversation sets F72 for Kazin following. A legal Map66 request consumes encounter state to enter Battle03. | Retain opening [search, dialogue and Church](../content/opening-branch-inventory.md), refusal/acceptance, ordinary Battle02 defeat and paid retreat/retry, Yeel conversation, repeat visit and the source-eligible encounter without visiting Yeel. |
| A · Battle03 return → Hawel → Battle04 → captivity. Pending ordinary play. | Battle03 victory clears F403, sets F503 and uses the first party member's final tile plus origin. Defeat restores Bowie, halves gold and uses egress. Paid escape takes its own egress return. Map66 (6,5) enters Map43 (6,11) Up. Default setup runs the house scene; F612 selects the revisit. Join Kazin (F4, active subject to capacity), follow, clear F72, set F404. Caller sets F612/F650, clears F72 and writes egress 43. | Check Map43 descriptions, scene, revisit and exit. Preserve growth, equipment and accounting through member activation. House init has no F503 or F72 gate. Check actual Kazin action and recipient, and a Galam Archer attacking from distance 2, with readable result and control. |
| A · Battle04 outcomes. Pending. | First victory stages Lemon's capture warp, executes the shared Bowie join tail, clears F404 and sets F504 before returning Map17 (0,0) Right. Completed victory skips capture and uses the saved ordinary victory return. Special defeat restores Bowie and halves gold, clears F404, sets F504 and overrides the egress map to 17; retain egress coordinates/facing. With egress 43 these are (7,5) Down. Legal EGRESS/Wing consumes its actual resource and returns to the egress map; first escape leaves F404 armed. | Check the distinct incoming return operands and usable handoff on real Map17 with carried resources. Stage B owns the prison interior, including F660-gated wake/reset and subsequent player interactions. |
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
Battle01 and title content. The [eight-map selection](../../content/private/pre-voyage-opening.json)
adds Yeel (Map5), its real Map66 encounter request, Battle02 outcome and the Battle03 definitions.
The producer binds each map's two base palette words from the verified ROM. Both ordinary readers
load the same world. The historical package ID identifies the same opening members and save recipes.

Reuse the [Battle02 input recipe](../verification/field-programs.md#prepare-the-selected-inputs)
and [Battle03 materials](../verification/field-programs.md#yeel-guide-through-battle03). Under one
fresh ignored output directory, place the two small encounter exports and their field-death scenes
in separate subdirectories. Place one copy of the shared trusted static, enemy and gold inputs there.
In the encounter list, supply one row per additional battle, ordered 02 then 03. Each row has
`placement`, `scene`, `terrain`, `static`, `enemies`, `gold` and `battleScenes` paths relative to the
world's parent. Product rows omit the diagnostic party checkpoint. Keep the root Battle01 inputs
and the preceding immutable exports read-only.

From the repository root with the configured private inputs loaded, use the accepted presentation
manifest, the nine reviewed additional command/timer candidates from the opening and Yeel recipes
(59/BD, 6E/CC, 6F/CC, 41/CC, 43/CC, 4A/CC, 41/C2, 43/C2, 01/C2 in hex), and the accepted authored
defeat and encounter cues. Pass each candidate exactly once.

```powershell
$audioArgs = foreach ($candidate in $reviewedCandidates) { '--audio-candidate'; $candidate }
uv run python -m sf2tool.remake_exploration_content --canonical $canonical --upstream $upstream `
  --rom-path $rom --presentation-root $assets --selection remake/content/private/pre-voyage-opening.json `
  --encounter-inputs $encounterList --output $freshWorld @audioArgs
$env:SF2_PRIVATE_EXPLORATION_CONTENT = $freshWorld
```

Launch the ordinary title without diagnostic start arguments. Use an external disposable save
namespace as in the [save recipe](../verification/host.md#opening-save-and-continue).
**Confirmed:** the product reader admitted eight maps and Battle01/02/03 with this preparation
(128,128,156 bytes for the measured relative paths). At 1920×1080 the ordinary New Game entered
Map3 (56,3), accepted a left step, saved at the existing Map3 recipe and quit via the system menu.
Fresh-process Continue restored the named party, both RNG streams, flags and gold, and accepted the
same step from the recipe anchor. Peak game working set was 1,472,802,816 bytes at New Game. This verifies early entry, persistence and input. Walking and
fighting from this title to Battle03, the post-Battle03 field returns, recruitment and Battle04
remain pending.

## Hawel media prerequisite

**Confirmed:** the pinned Map43 area selects music command 16. Its default `cs_540C0` requests
command 100 during the soldiers' departure and 16 during Hawel's conversation. The sad join selects
command 20 and waits for finite music completion. The retained title uses 28 and 10. The prepared
world's 44 cues have 44 distinct PCM identities and include none of 16, 20 or 100. Preparing
Map43's actual visual with the pinned canonical import and ROM yields a 128×320 native atlas and
music 16. With the selected manifest and the reviewed candidates above, asking
`prepare_visuals` for `MUSIC_ENEMY_ATTACK`, `MUSIC_SAD_THEME_1`, `MUSIC_SAD_JOIN` and `SFX_BLO`
rejects the latter three as unavailable. To reproduce the asset check through the CLI, make a local
copy of the selection above, set its `audioCues` to those four names, and repeat the producer command
with a fresh destination. The CLI returns exit 1 with a path-safe ValueError summary; the underlying
producer identifies the missing cue names. The [audio owner](../godot/audio.md#audio-boundary) requires reviewed
local sound inputs. Godot's `SessionAudio.Play` rejects unbound resources.

**Inferred capacity, before acquiring audio:** minified, the eight-map world needs 126,163,791
bytes (8,053,937 bytes free within 128 MiB). Its audio rows alone need 122,599,488 bytes. For a
scale estimate, sum the explicit and repeated note/wait lengths of channel 0 in pinned
`disasm/data/sound/musicbank0/music{16,19,20}.asm`. One pass gives 3072, 504 and 709 units
respectively. Calibrate against the existing finite command 19 with 429,803 stereo frames at
44,100 Hz and Timer B 192. At the same export format the estimated base64 bytes are
`429803 × (units / 504) × ((256 − timerB) / 64) × 4 × 4/3`. Commands 16 (209) and 20 (205)
would add about 10.26 MB and 2.57 MB for estimated playback times of 44 s and 11 s.
Even the current audio rows plus these estimates exceed the document cap, before map data or SFX.
Exact capture lengths, loop point, timer-specific speech/impact bindings and final bytes are
**Unknown**. The estimate includes neither hardware timing acceptance nor new capture.

The minimum independent prerequisite is reviewed 16/20/100 audio and bounded product admission
for it. First measure one real loop/finite cut using the [existing materializer](../godot/audio.md#audio-boundary)
and check the speech commands at the reached timer. Prefer a contained reference to the existing
immutable runtime WAVs when embedding would exceed the cap. Keep the 128 MiB world and 4 MiB scene
limits, validate actual PCM lengths/loops before allocation, and test the same selection in both
New Game and Continue. Acceptance needs map music, sad join completion, previous music and resumed
input. Target one producer or game process at a time, under 2 GiB resident, at least 1 GiB disk
free, and at most 15 MiB of reports. Main-gate owns sequencing before resuming A.

Chapter-specific compiler work also remains. Direct preparation rejects the compound sad-join
operand. Decoding that operand in isolation exposes the Map17 default setup's five local entities
followed by seven shared records under the next source label. The canonical list contains all
twelve; the current source reader stops at five and rejects their cardinality. Preserve that actual
fall-through and the Map43 roof, step and Hawel disappearance operations. The Battle04 board,
real actor frames, distinct returns and post-Kazin save impact still require implementation and
ordinary-play acceptance in A.
