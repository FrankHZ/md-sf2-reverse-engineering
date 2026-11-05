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
| A · Battle03 outcomes and approach to Hawel observed. Map43 and Battle04 pending. | Battle03 victory clears F403, sets F503 and uses the first party member's final tile plus origin. Defeat restores Bowie, halves gold and uses egress. Paid escape takes its own egress return. Map66 (6,5) enters Map43 (6,11) Up. Default setup runs the house scene; F612 selects the revisit. Join Kazin (F4, active subject to capacity), follow, clear F72, set F404. Caller sets F612/F650, clears F72 and writes egress 43. | Check Map43 descriptions, scene, revisit and exit. Preserve growth, equipment and accounting through member activation. House init has no F503 or F72 gate. Check actual Kazin action and recipient, and a Galam Archer attacking from distance 2, with readable result and control. |
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
admits Yeel (Map5), its Map66 encounter request, and the Battle02/03 boards and outcomes.
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
The measured world is 107,670,091 bytes. Keep the unchanged 01/C2 main-theme WAV and its candidate
in `audio-01-c2` beneath the world's parent, alongside the three Hawel directories. Supply its
candidate once. These four referenced WAVs total 26,376,328 bytes. The selection also requires
the four additional inline field cues below. Missing selected cues reject preparation.

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
| Defeat the remaining enemy | The first active member lands at the final tile plus the Battle03 area origin. Observed Map66 (18,21), F403 clear, F503 set, F72 set, four members, 100 gold. Ordinary movement reaches (6,6) outside Hawel. Up toward (6,5) gives the destination-unavailable panel. Dismiss it and move down to (6,7). |
| Field Save at the earlier legal Map57 Battle01 return, then fresh-process Continue | The Map57 anchor (8,7), four stored member records, flags, 360 gold and both RNG streams matched; the resumed field accepted movement. At the later Map66 return the save offer gives `save-location-unavailable`. Campaign saving after recruitment remains open. |

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
inherits the existing register. In the eight-map selection the only egress writer is this before
program; no normal route to Battle03 combat with the untouched reset register was found. A supplied
intro-seen state is a separate case. New Game and V1 Continue bind the same immutable opening fallback.

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
were prepared and admitted. Their delivery during the full house scene remains pending.

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

Chapter-specific compiler work also remains. Direct preparation rejects the compound sad-join
operand. Decoding that operand in isolation exposes the Map17 default setup's five local entities
followed by seven shared records under the next source label. The canonical list contains all
twelve; the current source reader stops at five and rejects their cardinality. Preserve that actual
fall-through and the Map43 roof, step and Hawel disappearance operations. The Battle04 board,
real actor frames, distinct returns and post-Kazin save impact still require implementation and
ordinary-play acceptance in A.
