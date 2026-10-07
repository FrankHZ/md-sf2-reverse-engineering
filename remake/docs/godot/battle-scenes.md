# Battle Presentation

Application owns staged gameplay publication and wait tokens. Godot consumes the current scene,
movement and death state, displays admitted resources and submits actual completion. Rule selection
and legality remain in the [engine](../domain/gameplay-rules.md); runnable observations belong to
[battle verification](../verification/battles.md).

Private rendering remains bounded to admitted encounter, actor and scene resources. `BattleSceneView`
matches source class/actor visuals and checks the equipped source item against the admitted weapon
visual; it also chooses source ground/animation presentation. This resource compatibility is not
general mod/schema support and does not grant Godot gameplay mutation or provenance authority.
Missing private resources fail visibly; they do not silently use authored gestures.

## Physical Battle Scenes

The common session separates physical action construction from scene execution. Domain prepares
ordered first/second/counter reactions and the action award. Application owns each continuation and
wait token: initialization, action text, animation, HP reaction, result/death text, EXP credit and
text, growth and its notices, gold text, then scene end. HP changes at reaction entry; EXP credit
precedes its text and growth follows acknowledgement. Kill/defeat accounting, after-turn, outcome
programs and the next actor wait for scene end. Wrong, stale or unrelated input cannot release a
wait. AI passes its prepared action to this same consumer without selecting or rolling twice.

**Confirmed, static source and engine boundary:** the pinned US baseline and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` support construction/replay separation in
`disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`,
`battleactionsengine_2.asm`, `attack.asm` and `battlescenes/battlesceneengine_0.asm` beneath that battle
source directory. The admitted physical damaging reaction consumes twelve pairs of shared-main RNG
draws (range5 for the ally and range7 for the enemy). Source loops `loc_18E6E`/`loc_19004` follow the
reaction-mode and `d1 == 0x8000` guards; this direct-draw rule is not every hit or scene's total RNG.
Growth consumes the carried main image after
those reactions; an old scalar comparison that omitted scene draws is not final-session parity.
`BattleSceneTests` checks phase boundaries, ordered followups, continuous reaction seeds, growth,
deferred accounting and input release; migrated action tests retain their construction assertions.

The private source content candidate contains the selected SDMN/PRST/KNTE and GIZMO frames,
Wooden Sword/Rod and Wooden Stick, Tower Interior background9 and ground9. The generator composes
source hardware tile chunks and background layout, preserves source frame timing/weapon flags,
and writes the existing pack-v1 master/2x/4x structure plus `battle-scenes.json`. Raw palette
provenance remains intact; displayed base CRAM words use mask `0x0EEE`, including the retail low
bits in words `0x0DB0` and `0x0E50`. Canonical inputs remain read-only and generation only accepts a
new ignored worktree-local output. This is a review candidate, not asset-library promotion.

After loading the [private input/tool selections](../../../docs/operations/local-private-inputs.md),
use absolute selected ROM/upstream paths and a fresh ignored destination:

```powershell
uv run python -m sf2tool.remake_battle_scene_content --rom $selectedRom --upstream $pinnedUpstream --output $candidateOutput
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $candidateOutput 'battle-scenes.json'
$env:SF2_BATTLE_SCENE_OBSERVATION_OUTPUT = Join-Path $observationOutput 'observation.json'
& $env:GODOT_BIN --path remake/game --audio-driver WASAPI --script res://probes/engine_battle_scene_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
```

Build the actual adapter first using the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
The isolated scene consumer/generator uses `uv run sf2 verify plan --scope engine --base origin/main --head HEAD`
on a clean committed head. Its exact generator path selects engine behavior, adapter compilation and
the existing direct public checks; research inputs still reject that explicit scope. This classification
does not extend to other generators or change research verification requirements.
The normal host also consumes this content through `--private-exploration-start`; source private
physical scenes fail explicitly when content is missing. The external observer reads actual node
resources, frame/position state, session effects and audio receipts, and submits ordinary keys.
It accepts `SF2_BATTLE_SCENE_WORLD=1` for the controlled Map40 navigation/warp route with an admitted
source world. `SF2_BATTLE_SCENE_REWARD=1` additionally exercises the adjacent player attack and
requires an explicitly prepared party with Bowie HP40, attack30, defense20 and EXP99. These are
controlled consumer observations, not original natural-entry fixtures. World, party and output
paths remain private. No screenshots or runtime state setters are used.

**Confirmed, bounded host observation:** source actor/weapon/environment textures, the 22-iteration
entrance projection, message waits, enemy and player hits, death/EXP/growth/gold, music2/5, SFX81/83/102,
253 requests and resumed battlefield music execute through the running Godot/WASAPI consumers.
Normal/reduced-flash runs preserve the same reaction draws, final seeds, resources and cursor while
the setting suppresses recoil/flashing. Battle text clears speech bleeps, matching source bsc10/11.
The host uses the existing modern half-second fade and instant/adjustable text settings. Hardware
timing, exact final pixels, continuous opening/outcome acceptance and original audio mixing remain
**Unknown**; these observations grant no H4 or deviation waiver.

### Medical Herb scenes

The admitted item family uses the same typed scene continuation. Inventory compaction and its two
range16 award draws occur during construction; HP/MP remain unchanged until the recovery command.
Recovery applies HP once without spending MP. The actor earns EXP after recovery text and restoration
of the actor's displayed side, then growth follows the EXP acknowledgement. Missing growth rejects
the whole preparation before inventory, movement or RNG can publish. Self-use, another living ally,
full HP and PRST/non-PRST awards share this path. Application supplies the displayed ally and absent
enemy, including outgoing/incoming target and actor phases; Godot never represents an ally as an enemy.

**Confirmed, static caller selection:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the following chain beneath `disasm/` distinguishes a
Medical Herb from casting the HEAL spell. This consumes the existing
[battle-scene](../../../docs/design/contracts/battle-scene-presentation.md) and
[action-construction](../../../docs/design/contracts/battle-action-construction.md) contracts; it does
not extend their natural timing claims.

| Source / symbol | Selected behavior |
| --- | --- |
| `data/stats/items/itemdefs.asm`; `code/common/stats/itemstats.asm:GetEquipmentType` | Medical Herb is non-equipment; equipment type is zero. |
| `code/gameflow/battle/battleactions/getspellanimation.asm:battlesceneScript_GetSpellanimation` | Non-equipment skips the use-spell animation field, retaining `SPELLANIMATION_NONE=0`. HEALIN's effect calculation does not imply its fairy animation. |
| `battleactions/createbattlesceneanimation.asm`; `battlescenes/getallyanimation.asm`; `battlescenes/battlesceneengine_0.asm:sub_184B0` (under `code/gameflow/battle/`) | USE_ITEM type2 selects the ordinary class sequence and preserves the caller's selector, rather than substituting its header default. Candidate headers remain raw; item selector zero belongs to the action. |
| `battlescenes/battlesceneengine_2.asm:SetupSpellanimation`; `battlescenes/animation/nothing.asm` | Selector zero dispatches to Nothing, which returns without fairy setup/update. |
| `battleactions/battleactionsengine_1.asm:InitializeActors`; `animateaction.asm:SwitchTargets`; `battleactionsengine_2.asm:battlesceneScript_End` | Begin with the actor alone; switch to another ally before recovery and back to the actor before EXP. Self-use needs neither switch. |
| `battleactions/castspell.asm:spellEffect_Heal`; `battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction` | Mode2 applies recovery then sends SFX113 and returns; no damaging-reaction recoil loop. Message298 follows recovery; item message275 names the consumed item. |

Recheck any named source directly using the selected read-only upstream root, for example:

```powershell
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battleactions/getspellanimation.asm'
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlescenes/animation/nothing.asm'
```

The source starting Chester loadout is `WOODEN_STICK|EQUIPPED` in `allystartdefs.asm`:
word184 masked by127 gives item56. `getweaponspriteandpalette.asm:table_WeaponGraphics` at ROM0x1F9E2
selects sprite10/palette19 for item56. The class KNTE sequence is index2, including item type2;
weapon sprite10 does not select the spear-throw specialization. The candidate admits this actual
starting binding. Runtime checks the live equipped item against the candidate and rejects missing
weapon support; it does not assign a class-wide weapon or silently replace equipment. An older
Short Spear candidate does not establish starting Chester presentation. Preserved physical observations
only prove the actor/loadout combinations they actually reached.

**Confirmed, bounded host observations:** ordinary keys consume source item frames/text, one-sided
status/resources, target/actor switches, SFX113, EXP/growth and return to battlefield control. The
Map40 source-world startup heals the party before battle, so that observation covers full-HP use;
a separate ordinary-input battle run first obtains actual enemy damage, then covers wounded
self/other-ally recovery without claiming world audio.
Normal/reduced-flash source-world runs agree on gameplay, inventory and bounded RNG records. A narrow
Chester physical observation consumes the corrected live Wooden Stick resource and sequence2.

The switch projection uses `bscWait $1E`, `bsc07_switchAllies` coordinates and
`SwitchAllyBattlesprite`'s capped16 steps through the existing presentation timing mechanism.
DMA/VInt/input/audio alignment remains **Unknown**. Neither those durations nor recovery sound
playback add invented shared-RNG opportunities. No fairy or24 recoil draws means only those two
branches are absent, not that the whole scene has zero RNG. HEAL continuous-opportunity parity and the pending waiting-policy
decision remain separate; no original timing/pixel/H4 acceptance or asset-library promotion follows.

For the affected engine behavior, select `MedicalHerbTests|BattleSceneTests` with the locked SDK's
`dotnet test --filter` (fully qualified names), then build the adapter. The same committed engine-scope
planner applies. The existing native observer accepts `SF2_BATTLE_SCENE_HERB=1`; it uses Chester
self-use and Sarah-to-Bowie only as controlled observation inputs, not gameplay legality rules.
Select the normal source battle inputs and the fresh candidate as above. For the source-world/audio
pair, copy the tracked opening party to a fresh ignored JSON, retain
all inventory words and set Sarah EXP99. Set `SF2_PRIVATE_CONTROLLED_START` to that absolute path and
`SF2_PRIVATE_EXPLORATION_CONTENT` to the existing world without copying it, and set
`SF2_BATTLE_SCENE_WORLD=1`. Use `--private-exploration-start $explorationStart` with the controlled
Map40 boundary below, plus a fresh output path for each run. The world program's healing is retained.

```json
{"formatVersion":1,"controlledBoundary":"Controlled Map40 scene-consumer observation; not natural entry","start":{"map":"map-40","player":"entity-0","position":{"x":4,"y":30},"facing":1,"speed":32,"flags":[0,1,2,32,33,34,401,451]}}
```

Pass `--input-settings $settingsPath` containing `{"formatVersion":1,"reducedFlash":true}` for the
paired accessibility run. `SF2_BATTLE_SCENE_CHESTER=1` instead selects the narrow physical binding
observation with ordinary Tower entrance movement; its controlled party retains the starting
inventory and gives Chester HP/maxHP40 and defense20. For positive recovery, set `SF2_BATTLE_SCENE_HERB=1` and
`SF2_BATTLE_SCENE_WOUNDED=1` with the same controlled Chester HP/maxHP40/defense20 party,
Sarah EXP0 and `--private-battle-start $partyPath`. The observer walks Chester into actual enemy
attacks, brings Sarah alongside using ordinary movement, then uses Sarah's herbs on Chester and herself.
Each action must begin below live maxHP and increase HP. Supplying a low starting HP alone does not
establish injury: the observed battle initialization restores HP. Chained enemy scenes are identified
by their new initialization token and the preceding scene-ended event, without requiring a hidden
frame between scenes. These modes use the same fresh-output/error
contract and no screenshots, original emulator, runtime state setters or fixed comparison seeds.

## HEAL spell scenes

The bounded HEAL1–3 consumer prepares two award draws, then retains scene/turn ownership through
MP cost, casting, optional ally switch, HP recovery, recovery text, ordinary idle, fairy retirement,
cleanup, actor restoration, EXP/growth and input release. Full HP still casts and awards the healing
minimum; self-healing preserves the already charged caster MP. Growth consumes the seed carried
through the scene, not the construction seed. Private scenes require admitted PRST cast content,
idle parameters and healing rasters; authored packages use an explicitly authored small gesture.
Other spell effects and HEAL4 remain unsupported. The historical #523/#437 records do not establish full scene or hardware parity; the
[current composed milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
has its separately bounded acceptance.

`HealingFairy` implements source integer word arithmetic, signed motion residuals, wing/dust state,
level-dependent one/one/two instances, setup ranges32/30/12 and conditional update draws. Lifetime
FFFF decrements before update. X-boundary ranges28/32 and periodic dust12 can co-occur; phase3
retirement under control2, clear-before-cleanup-wait and later restoration remain separate boundaries.
The existing private scene extractor supplies source body/wing/dust rasters and PRST cast sequence
without promoting assets. Godot projects that state and plays source SFX77 at cast setup and113 at
recovery. Reduced flash and text reveal alter delivery only.

The scene-local `HealingSceneCursor` separates mandatory operations, explicit neutral timed-input
reads and host delivery. Its logical message setting is speed2/no-messages0, independent of modern
text settings. `bsc10` tests acknowledgement before the next VInt opportunity; a neutral read advances
one opportunity. Neither reveal nor acknowledgement injects W2/range256 draws. Ordinary field
`WaitAtInput` and plain-dialogue `WaitForText` retain their own eligibility guards. Scene commands
require the current session, revision and scene token, with exactly one logical step per command.
Completion delivery may arrive early or late, but cannot add steps after logical completion.
For HEAL action/recovery messages, `CompletePresentation` reports text readiness independently of
`Acknowledge`. The source speed2 neutral timeout completes without an additional confirmation once
text is ready; if reveal finishes later, its delivery alone releases the expired message, with no
new logical opportunity or RNG. The host reports readiness at the timed-input boundary or after
timeout, and idle/reveal callbacks do not supply neutral Wait. Early acknowledgement still returns
before the next input-loop VInt. Reward/growth messages retain their existing acknowledgement policy.

Source owners are pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlescenes/animation/healingfairy.asm`, `animation/update/healingfairy.asm`, `battlesceneengine_0.asm`
(`bsc01`, `bsc05`, `bsc0B`, `bsc0D`, `bsc10`), `battlesceneengine_1.asm`, `common/tech/graphics/graphics_1.asm`
(`sub_179C/table_1840`) and the window/text helpers identified in the
[retained HEAL evidence](../../../docs/research/map3-messenger-acceptance.md#retained-heal-consumer-evidence).
Paths start below `disasm/code/gameflow/battle/` except the common helpers. PR550's accepted
prepared-04 instrumentation uses execution `3e69c2d49c8105c0e485dcbea70c688b57a86902` and its explicitly
rebound Issue515 prepared-11 parent. It does not reinterpret the incompatible older parent.

### Separate comparison boundaries

**Confirmed — function comparison:** `local/issue523/heal-scene/comparison/Compare.csproj` calls the
production kernel with HEAL1 and the externally observed post-award seed5C0D0000. Setup produces
seed00920000 and the observed properties. It then supplies exactly one update per recorded
`heal:fairy:after` and requests control2 from the trace's stop/control events. All244 returns match
property words, lifetime/control/toggle, dust age/frame/clock and carried seed, including29 conditional
update draws. Counts are comparison outcomes, not production budgets. This proves the update function
for that externally supplied opportunity/control schedule; it does not prove that the application
admits the original schedule, sprite coordinates/pixels, or every possible entry state.

**Confirmed — completed continuous-cursor comparison failure:**
`local/issue523/heal-scene/cursor-comparison/Compare.csproj` runs the production cursor from the same
external post-award seed, admitted PRST cast/idle content, recovery text `Sarah recovered\n6 hit points.`
and neutral timed reads. Subsequent opportunities and control2 come from the cursor, not trace events.
The first named divergence is recovery `bsc0B`, prepared-04 CP2059–2091 (one-based checkpoint lines),
observer frames32364–32367. Entry has actor/target1, HEAL1; the source action supplies HP+6, MP+0,
status0, reaction mode2. At CP2059, a6=FF0022, seed00920000, control1, lifetimeFFFE, toggle1,
fairy age2/phase7/delay23/dustClock1. Source `OpenAllyBattlesceneMiniStatusWindow` contains an explicit
move/VInt and a movement-end wait. The current cursor ends recovery after two opportunities with
lifetimeFFFC, age4, delay21. The observed caller returns after three, with lifetimeFFFB, age5,
delay20 (CP2084/2091). Neither has drawn yet, so both seeds are00920000 at this first divergence.

At recovery text return, the cursor has seed43EE0000/lifetimeFF80; original CP3458/3459 has
D8800000/FF7F. This is an actual intermediate RNG divergence, not a waived display difference.
At make-idle exit the cursor has seedECE20000/lifetimeFF6A, while CP3727 has ECE20000/FF65;
the equal seed does not restore the missing state/opportunity correspondence. At final stop return,
both reach seed8E2D0000/control0; this particular capped recovery and construction-time award remain
HP5→11, MP10→7, EXP+13. That endpoint coincidence does not establish general result equality:
other seeds, content or stop phases may alter conditional draws and later gameplay/growth.

The PRST idle header is20. `bsc05` performs a real frame load on each side of Sleep; each ends in
`WaitForDmaQueueProcessing`, whose helper unconditionally branches to `WaitForVInt`. The cursor
therefore retains those two operations around the content-provided sleep. Weapon queue enable and
`sub_1942` do not themselves wait. The observed26-frame bsc05 interval is not a source sleep value.
An earlier hand-calculated153-versus154 estimate incorrectly used that observed interval; it is
superseded by direct cursor execution:149 active opportunities before stop versus154 observed.
No149/153/154/26 budget or seed padding is used in production.

**Confirmed — bounded opportunity readback:** the
[research attribution](../../../docs/research/map3-messenger-acceptance.md#recovery-and-make-idle-opportunity-attribution)
records three top-level recovery VInts: the first has A6=`FFFFDE80` (the source number buffer), the
next two have restored script A6=`00FF0028`. Source numeric conversion has no waits; window movement
has an explicit VInt followed by a wait that repeats conditionally until movement ends. Make-idle
has26 top-level VInts: four with stack A6 in two pairs, two with restored script A6/d0=`$900`, and20
with the Sleep counter19…0. The source performs two decompressions and DMA waits around Sleep(20).

**Inferred:** recovery's extra opportunity interrupts numeric window construction; make-idle's four
extra opportunities interrupt the two decompressions. These are CPU-work interruptions outside the
explicit waits, not evidence for fixed +1/+4 waits. The pinned source/register mapping and exact
readback command belong to the research owner. The audited `HealingSceneCursor` opportunity rules
are unchanged between the retained comparison at8e6881ea and accepted PR557 main
`bfcb819fe61cf6b7f2a3a45822f6de51e411e559`; its neutral timeout correction changes completion handling,
not these opportunity rules. The completed continuous-cursor **FAIL** remains.

**Unknown — gameplay-opportunity admission:** exact interrupted PCs, live busy/window/graphics gates,
cross-state/content timing, and a general semantic treatment remain unproven. Conditional window
completion alone does not account for interruptions during CPU work. Wider content/gate coverage
and full continuous scene conformance remain Unknown at the [retained comparison boundary](../evidence/retained-comparisons.md).
[ADR0010 Option A/8D](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)
still controls shared RNG and selected gameplay results. This attribution does not waive the
intermediate RNG difference or establish that hardware simulation is necessary. A next proposal
must identify concrete gameplay effects and feasible semantic handling under that policy; the
observed counts alone supply neither a new product decision nor permission for new capture.

**Confirmed — separate native settings comparison:** actual Godot inputs in the existing scene
observer exercise full-HP HEAL3, wounded other-target HEAL1 and wounded self-target HEAL2, alongside
physical scenes. Each completes real fairy retirement/cleanup and releases input, with actual77/113
audio and live projected nodes. Normal/instant versus reduced-flash/adjustable text uses the same
semantic inputs; final `native-timeout-normal-01` / `native-timeout-reduced-01` records each finish
six scenes and match all2311 ordered semantic events, RNG and final resource/turn state (delivery
metadata excluded). Both action and recovery messages also exhaust65 neutral inputs with zero
Confirms after natural text readiness; their final-input receipts contain no extra RNG or delivery.
Later HEAL cases retain early acknowledgements. This proves bounded settings conformance, not original neutral-input conformance.
The reproduction and completed failure records belong to the
[verification owner](../verification/battles.md#heal-scene-verification).

## Battlefield death batches

The common action continuation retains newly defeated combatants on their battlefield cells until
its field exit completes. The close-up panel hides after `scene-ended`; the same action still blocks
input, automatic advancement and outcome programs. The defeated hook runs before the batch, then
all members turn twelve times, one sound116 request starts the three effect63 directions, and only
then are positions/status and kills/defeats cleaned. A final settle releases the action. Empty batches
have no presentation or sound. Old corpses are never rediscovered by an HP-zero roster scan.

The private board binds original map sprites through source ally/enemy assignments, using the
selected world's `ExplorationVisuals.Sprites`. The selected scene document supplies only the missing
63 direction sheets and field selectors. These are mapsprites, distinct from close-up battlesprites.
Settled units use a static field pose in the diagnostic board; full battlefield composition, terrain
art and ordinary idle animation remain outside this consumer. The field phase freezes each dead
sprite's first walking frame, while whole-batch direction changes select up/side/down sheets and
right-facing mirroring. Existing whole-PCM116/BD playback may outlast modern visual delivery; its
sample count does not control gameplay or RNG. Reduced animation may shorten display duration.

**Confirmed static source boundary:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/battleloop/processkilledcombatants.asm` (`0x24518..0x24642`, sound caller
`0x24574`) supplies the list traversal, facing sequence, effect63, and cleanup ordering.
`sf2enums.asm` defines twelve turns/Sleep3, effect direction1/2/3 with Sleep8 and final Sleep10.
`entityscriptengine_1.asm:VInt_UpdateSprites` uses signed ANIMCOUNTER=-1 to freeze frame1;
`entityscriptengine_2.asm:ChangeEntityMapsprite` supplies direction-sheet loading. The host explicitly
models those presentation stages, not an asserted universal seventy-VInt timeline.

The bounded RNG argument follows `executeindividualturn.asm` -> `LoadBattle` ->
`PositionBattleEntities` -> `eas_Standing/eas_Idle`: ordinary combatants are stationary and execute
speed/flags setup followed by wait/branch without random operations. Battle01's neutral Mist Demon
and Astral also select `eas_Standing` in `data/battles/global/battleneutralentities.asm`.
`SetBaseVIntFunctions` restores battlefield entity/map/view/scroll/sprite/window/map-animation
services, not the close-up reaction/fairy RNG callbacks. Death animation itself changes entity
facing/graphics with zero travel and frozen animation counters; it does not start random-walk or
random-branch scripts. No gameplay RNG call is added at a field delivery completion.

**Unknown:** exact interrupt opportunities, initial sprite-load queue occupancy, asynchronous service
interleavings and hardware timing. The source can add waits when queued sprite loads reach seven;
those are not an unconditional frame allowance. Source timing, natural multi-death, special entity32,
random scripted battlefield entities and after-turn status deaths are not established by this
bounded consumer. Current physical actions produce at most one casualty; multi-member domain cleanup
is separately exercised without pretending those producers exist. Option A, 8D/H4 and the retained
HEAL recovery opportunity mismatch remain open; no padding, trace-count fitting or reseeding is used.

Actual enemy-death and ally-counter-death normal/reduced consumers exercise sprite63,116BD and
cleanup/control release with equal ordered semantic events and final gameplay state within each
pair. The counter uses the finite selection policy above: music2 requests timerC6 and the selected
pack's unique81/CC recording plays unchanged. The original missing81/C6 failure remains retained;
modern PCM reuse resolves host delivery, not hardware-exact counter audio. Direct adapter observations
cover exact preference, unique reuse, named playback, missing/ambiguous rejection and actual finite
completion without gameplay changes. The world/audio pack and #517's116 recording remain unchanged.

## Battlefield movement and walking audio

Application owns a finite route and one completion token per adjacent segment. Player steps change
the provisional destination; committed cells and resources remain unchanged until the action.
Friendly cells may be traversed but cannot be selected as a final stop. Cancel walks the source
decreasing-cost return route to the committed origin, rather than reversing the input history.
AI retains its chosen route, thinking stream and memory from one decision. A pure, discarded action
preflight preserves atomic Unsupported admission, including random-dependent reward capability;
actual action construction publishes main RNG only after arrival. Damage, rewards and turn advance
retain their existing action continuation. No future battle snapshot is retained alongside live state.

Godot interpolates only the current legal segment, selects direction and both halves of the admitted
map-sprite sheet, and delivers that segment's token once. Display durations are modern 0.20 seconds,
or 0.10 with reduced animation; delta, redraw, audio completion and arrival add no gameplay RNG.
Busy gameplay input cannot acknowledge movement. Each new segment requests79 once. Blocked input,
origin Cancel, origin Stay and repeated projection have no walking cue. The private consumer reuses
the selected world's finite `SFX_WALKING_TIMER_BD`, command79/timer189, 44,100 Hz, 30,006 frames,
SHA256 `181C8EDDCD877D9AB031366B88F926A44F97684CDAB44697307FEFAAE8014F9B`.
No new extraction, resource copy or promotion is needed. Whole-PCM replacement remains the accepted
audio policy; modern segment duration need not equal PCM duration.

**Confirmed static source boundary:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, beneath `disasm/`:

- `code/gameflow/battle/battlefunctions/battlefunctions_0.asm` supplies
  `MoveBattleEntityByMoveString`, whose sound-command call site `0x23078` starts each segment's sound
  before travel, along with destination waiting and
  control/temporary land-effect coordinate updates; `battlefield/buildmovestringfunctions.asm`
  supplies `BuildCancelMoveString`, and `battlefunctions/executeindividualturn.asm` selects the
  control boundary. `battlefunctions/setmovesfx.asm:SetMoveSfx` (`0x25790..0x257C0`) selects79 in battle, except an
  equipped Chirrup Sandals ring (`0x7E`) selects99. Existing private ATT-only weapon admission
  excludes rings; arbitrary loadouts and99 are outside this consumer.
- `code/gameflow/battle/battleloop_1.asm` clears `PLAYER_TYPE`; `SetControlledEntityActScript` in
  `code/common/scripting/entity/entityfunctions_2.asm` therefore selects the ordinary controlled
  character script. `data/scripting/entity/eas_main.asm` runs `ac_checkMapBlockCopy`,
  `esc02_controlCharacter`, destination wait and branch. Neither a vehicle nor random walk is
  selected. The AI movement-string path also waits on ordinary entity displacement.
- `CreatePulsatingBlocksForGrid` in `battlefunctions_0.asm` sets nonzero
  `FADING_SETTING=PULSATING_1` before movement. In
  `code/common/scripting/entity/entityscriptengine_2.asm`, `esc40_checkMapBlockCopy` checks that
  setting first and skips copying while the battle grid pulsates. This contextual guard matters:
  roof copying can mutate layout in other contexts. Map57's roof/step tables contain only `endWord`.
- `esc02` vehicle branches and `OpenDoor` have battle guards. Warp/zone branches may still write
  `MAP_EVENT_TYPE/PARAM` scratch; battle control consumes button/destination state and `BattleLoop`
  has no map-event dispatcher. Victory writes its return parameters from combatants. This is not
  a claim that every interrupt has no side effects.
- `LoadBattle` restores `SetBaseVIntFunctions`: map planes, entity scripts, view, scroll, sprites,
  windows and map animations. The admitted movement update and controlled/standing/idle scripts
  do not draw RNG. Battle01's other combatants and neutral entities use those stationary scripts;
  close-up reaction/fairy and random entity scripts are not active in this boundary.

Reproduce the audit with `git -C $pinnedUpstream show '<commit>:disasm/<path>'` for the named
symbols and `rg -n 'Random|RANDOM|SEED'` over the reached bodies, checking each caller's guards.
Absence of a random call alone does not prove natural reach or interrupt parity.
**Unknown:** exact VInt opportunities, sprite-load queue stalls, acceleration, held-input cadence,
broader scratch effects and hardware audio. Normal/reduced native equality establishes only the
bounded modern consumer. Option A, the retained HEAL CP2059–2091 recovery mismatch and 8D/H4 remain
open. No logical padding, reseeding or fitting to retained79 receipt counts is part of movement.
