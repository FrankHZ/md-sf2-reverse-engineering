# Battle Spell Presentation

**Standard scope.** [#2104] tracks every USA spell-table family and rank through gameplay
admission, scene delivery and verification. Battles01–07 through first Map14 control set the
pre-voyage priority. Later private campaigns and controlled authored packages establish their own
boundaries. This inventory uses the existing rules, scene readers, presenters and private resource
publisher. Each implementation slice owns its own acceptance.

## AQUA family playback #2315

**Standard scope.** Prepared BUBBLE_BREATH replaces primitive bubbles for Kraken Head in
Battle16 and Bubbling Ooze in Battle23. Willard names the encounter; enemy90 Willard has
no spell. Preserve AI, payment, targets, damage/death and returned control. No new species,
boss rule, campaign admission, save format, #1716 or broader renderer work is included.

Owned code: `game/src/Battles/AquaEffect.cs`, `AttackSpellView.cs`, `BattleSceneView.cs`,
`BattleSceneView.Rasters.cs`, `game/src/Audio/SessionAudio.cs` and Content's
`PrivateAttackSpellBindings.cs`. Producer: `src/sf2tool/remake_aqua_mount.py` plus existing
attack-mount/private-package/retail CLI/engine-path registrations. Observer:
`game/probes/engine_aqua_family_observation.gd`. Serial document owners are this file,
battle-scenes, kraken-battle, willard-burrow-battle23, presentation-assets, asset-inventory
and capability-status. Accepted #2314 is the complete base; independent opening/Witch/catalogs
remain selected. Production-reader closure and bounded ordinary enemy input own acceptance.

### AQUA source and delivery

**Confirmed source structure:** USA ROM SHA256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. `spelldefs.asm` maps AQUA1/2 to22/54,
zero MP, power13/15, range3/1 and radius0. Spell ID40 is not its animation selector.
`SetupSpellanimation` shifts variant bits and adds1: setup receives1/2 after masking
mirroring. Setup `0x1B488..0x1B4F6` loads tileset0 and sets the228-update emission hold.
Rank2 replaces palette3 slots9/13/14 with raw `$CBF/$B9A/$B46`; the VDP's `$EEE` mask gives
`$CAE/$A8A/$A46`. Rank1 retains `$ACA/$8A6/$602`. Only these source operands are masked;
the shared strict decoder is unchanged.

Update `0x1E5D0..0x1E7B2` uses16 slots. `graphic_Bubble` starts at VDP(212,248), logical
(84,120); its table grows through8/8/8/8/16/24/32-square frames at zero-based tiles
0/1/2/3/4/8/17. The33-tile prepared stream is1,056 bytes. States10/20/30/40/50/60 change
frame; state80 retires a bubble. State10 spawns its successor while the hold remains.
Initial random fixed-point velocities are x320..831 and y-512..-257, with y+28 per update;
at40 vertical velocity resets to-64..-33 and later receives y-32 plus horizontal sway.
Horizontal flip follows state bit2. Each spawn requests104; first retirement also releases
the source damage wait. Neither228 nor307 is asserted as the original whole action length.

Pinned `enemydefs.asm` assigns AQUA1 to Kraken Head87 and AQUA2 to Bubbling Ooze16;
Willard90 has four NOTHING slots. Battle16 uses head body44/palette0/sequence44;
Battle23's six Ooze placements share body0/palette2/sequence0. Existing scene readers
select live actor/target terrain and equipment. No generic caster is introduced.

[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/bubblebreath.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/bubblebreath.asm) ·
[Variant decoder](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_2.asm) ·
[Enemy definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/enemies/enemydefs.asm).

**Modern delivery:** deterministic cosmetic velocities and nine-tick emissions during the
228-tick hold drain within307/60 seconds on the existing action clock. The unchanged action
continuation delivers damage after this finite animation. Rendering draws no gameplay RNG.
The actor trigger starts playback; breath adds no generic cast77. Prepared104/CB audio is
mounted; `sounddriver.asm:sfx_28` has one active PSG tone3 pointer (ninth of ten slots), now
registered in `SessionAudio`'s existing whole-clip replacement policy. Reduced Flash suppresses
rapid flipping while retaining growth/motion and outcomes. Held1.5x uses shared pacing.
Seven-frame rank groups choose HD atomically; incomplete HD falls back entirely, partial
Original fails at presentation, and whole absence retains the existing modern fallback.
**Unknown:** natural reach, original cadence, palette/plane/GPU fidelity, physical listening
and human aesthetic acceptance.

### AQUA verification

Select complete `derived/issue2315/aqua-v1`, independent #2252 opening, #2255 Witch and
#2266 catalogs. After the [locked Debug build](../development-and-verification.md#locked-net-workflow):

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2315/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case aqua1 `
  -Sample aqua1-review -SaveRoot $freshExternalSaveRoot
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case aqua2 `
  -Sample aqua2-hd-review -Graphics hd -Reduced -Fast -SaveRoot $freshExternalSaveRoot
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case aqua2 `
  -Sample aqua2-fallback-review -Graphics hd -Variant incomplete -SaveRoot $freshExternalSaveRoot
```

**Confirmed in the remake:** production reader accepts26 complete scenes and seven current
rank bindings: Battle16 rank1 and Battles17/19/23 ranks1/2. Availability is not caster admission.
Both controlled Continue records restore. These are controlled prerequisites, not natural
campaign continuations: the accepted later-party Bowie record is placed beside each actual
caster; Kraken limbs are defeated to clear its swarm guard, Battle23 retains its living leader,
and the Ooze activation bit is set. Main seeds are`0x1234`/`0x10000000`; thinking seed is
`0x12340000`. Enemy books/species and decision rules remain unchanged.
Continue → Confirm → Stay dispatches real AI. Rank1 deals16 and rank2 deals15 to Bowie,
with zero MP delta, one HP application, no enemy reward and usable next ActionChoice.
Existing engine assertions retain death, category/payment and chapter-return behavior;
these bounded scenes do not independently re-prove natural victory or death.

Published-input Original, HD/Reduced Flash/held-speed and incomplete-HD runs pass with the
producer package path absent. Draw/state observations cover all seven frames, actual body/
sequence, scenery, texture density,104, early Confirm refusal and returned control. Reader
peak stays below896MB; final native peaks are below1.31GB and each takes under4.4 seconds.
Engine verification passes3,383 with50 optional-fixture skips; Release/Debug builds pass.
No helper tests, gameplay screenshots, bulk frames or new audio acquisition were added.

Retained failures: missing optional Pillow, wrong Battle16 filename, raw palette-bit rejection,
nullable build error, wrong save writer overload and nonformation deployment. First Kraken
correctly waited for its swarm condition. First Battle23 marked its leader dead and returned
to exploration; its observer then read a battle-only projection. The next Ooze correctly took
standby before activation was supplied. First fast run delivered AQUA but lost Shift across
ordinary input; the observer gesture was corrected. Setup also retained a relative-output
rejection, ctypes pointer-width error and text-encoding error. Complete logs remain in the
private review. Successful exits retain6–10 ObjectDB warnings under#1716; no repair is claimed.

## ATLAS family playback #2314

**Standard scope:** bind admitted SORC ATLAS1–2 through the accepted summon lifecycle,
invocation decoder and scene publisher. Presentation crosses content, producer and adapter
boundaries but changes no gameplay rule. Preserve one payment, fixed actual-recipient
division, ordered result/death/reward and returned control. No enemy summon admission,
promotion/recruitment, AI, Battle27+, save schema, #1716 or broader renderer work.

Owned paths: `game/src/Battles/AtlasEffectView.cs`, `BattleSceneView.Summon.cs`, the existing
`BattleSceneView.cs` observation field, Content's `PrivateAttackSpellBindings.cs`,
`src/sf2tool/remake_atlas_mount.py`, existing attack-mount/private-package/retail CLI/engine-path
registrations, and `game/probes/engine_atlas_family_observation.gd`. Serial document owners
are this file, battle-scenes, church-promotion-design, presentation-assets, asset-inventory
and capability-status. Accepted DAO/APOLLO/NEPTUN and their complete package are dependencies.
Acceptance covers a one-scene pilot, inherited-value equality, production-reader scene/rank
closure, short ordinary single/divided casts, and one HD/settings/fallback boundary.

### ATLAS source and delivery choices

**Confirmed source structure:** USA ROM SHA256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. `spelldefs.asm` selects20/52 for ATLAS1–2;
setup/update have no rank branch. Enemy HEAL rank probing can read an ATLAS operand;
that is not a cast or enemy summon admission. Setup `0x1B37C..0x1B406` loads invocation1,
preloads its second pose and initializes two16-sprite streams with delays6/1, horizontal
velocities +9/-9 and vertical velocity10. The damage hold releases after80 updates.
The shared loader contributes cast77, four `$CCE` flashes and8 entrance ticks.

`LoadInvocationSpell` starts with bank-bit6 clear, loads frame0 into the alternate bank,
then toggles bit6. Setup therefore loads frame1 into the base bank. Update
`0x1E134..0x1E2D4` starts with base-layout frame1 and changes to layout+36/frame0 on the
11th motion update. It then plays HIT_1/2 (83/84), requests a four-update vertical
background displacement `[-3,3,2,-2]`, and waits2 updates before the next spawn. Each stream
restarts every14 updates. Source random choices are x0..23, y0..47, later side and hit cue;
initial VDP x16/304 and y16 include the128-coordinate bias. The header at ROM`0x12CD26`
is `(1,1,1)`, initially offscreen; reproduce by following invocation table entry1 at
`1221368+4` in the verified ROM. Normal/mirrored layouts share the same palette:
`enums/vdp.asm` defines `VDPTILE_PALETTE1=0`. Both128×128 poses use the accepted palette
word0 skip and split-stream/VRAM-gap decoder; no separate impact tileset is loaded.

[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/atlas.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/atlas.asm) ·
[Shared loader](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_3.asm) ·
[Impact displacement](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/updatespellanimation.asm).

**Modern delivery:** a116/60-second minimum covers setup plus the initial80-update hold.
Both streams continue through ordered recipients until the shared Reward/End reset restores
the caster, weapon and ground. Fixed phase offsets and alternating side/cue replace cosmetic
random calls without consuming gameplay RNG. Logical time, plane displacement, palette flashes
and whole-clip audio are approximations. Reduced Flash suppresses flashes/shake while retaining
both poses and motion; held1.5× uses the shared clock. Existing package recordings and
`SessionAudio` slots cover77 (YM4),83/84 (YM6); no new audio capture or slot change is needed.
The two-pose group chooses HD atomically, falls back entirely when HD is incomplete, and
rejects partial Original. An absent group retains the existing modern fallback.
**Unknown:** natural higher-rank training/reach, original hardware cadence and plane/GPU
fidelity, physical listening and human aesthetic acceptance.

### ATLAS verification

Use complete `derived/issue2314/atlas-v1`, independent #2252 opening, #2255 Witch and
#2266 catalogs. After the [locked Debug build](../development-and-verification.md#locked-net-workflow):

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2314/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case atlas1-single `
  -Sample atlas-original-review -SaveRoot $freshExternalSaveRoot
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case atlas2-multi `
  -Sample atlas-hd-review -Graphics hd -Reduced -Fast -SaveRoot $freshExternalSaveRoot
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case atlas1-single `
  -Sample atlas-fallback-review -Graphics hd -Variant incomplete -SaveRoot $freshExternalSaveRoot
```

**Confirmed in the remake:** the production reader accepts52 rank bindings across26 scenes
and both controlled Continue records. Kazin retains the accepted Battle26/DAO-derived layout,
Wooden Rod and seed`0x0DA20000`. Rank1 uses controlled SORC21/book`[93,94,31,32]`;
rank2 uses SORC32/book`[93,94,95,96]`. These legal destination books and levels are controlled
prerequisites, not natural training. Continue → Magic → target → Confirm reaches20/52,
rank1 damage43 and rank2 ordered45/35 across two actual recipients, one14/23-MP payment,
death cleanup, one reward and usable next ActionChoice. Existing summon assertions own division.

Final node/draw observations cover both128×128 resources, both directions and streams,
base-bank frame1 before impact-frame0, caster replacement/restoration and cues77/83/84.
Original, HD/Reduced Flash/held-speed and incomplete-HD fallback pass from the published
package with the producer package absent. Reader peak is887MB; native witnesses stay below
1.4GB and5 seconds. No gameplay screenshots, bulk frames or helper tests were added.

The completed engine run retained3,392 passes,6 skips and35 failures: it inherited the native
launcher's private selections, which omit `SF2_PRIVATE_CONTROLLED_START` and do not match
older private tests' scene/encounter boundaries. No product rule was changed for those tests.
A fresh-environment rerun of exactly the35 failed nodes plus `SummonSpellTests` gives5 passes
and35 optional-fixture skips; those skips are not private-test passes. Production-reader and
native acceptance above independently cover this package. Do not repeat the full suite to
replace the retained failure. Release/Debug adapter builds pass. Initial pose-order witnesses
are superseded by the bank-traced correction and the three focused final reruns.
Setup-only diagnostics include the unquoted PowerShell Git tree expression, missing TEMP
warnings before directory creation, an unavailable upstream binary URL and a summary read
using the wrong text encoding. Original-mode exit retains8 ObjectDB warnings under#1716;
no cleanup repair or user visual acceptance is claimed.

## NEPTUN family playback #2313

**Standard scope:** bind currently admitted SORC NEPTUN1–2 through the accepted summon
lifecycle, shared invocation decoder and private scene publisher. The finite presentation
change preserves payment, fixed actual-recipient division, ordered damage/death/reward and
returned control. No new admission, AI, recruitment, promotion, Battle27+, save schema,
#1716 repair or AQUA breath is included.

Owned code: `NeptunEffectView.cs`, `BattleSceneView.Summon.cs`, the existing scene observation
field, `SessionAudio.cs` (two admitted sound-slot entries), `PrivateAttackSpellBindings.cs`, `remake_neptun_mount.py`, existing attack-mount,
private-package, retail CLI and engine-path registrations; `engine_neptun_family_observation.gd`.
Serial documentation owners are this file, battle-scenes, church-promotion-design,
presentation-assets, asset-inventory and capability-status. Accepted DAO/APOLLO are dependencies.
Acceptance requires a representative pilot, inherited-value comparison, production-reader
scene/rank closure, short ordinary-input single/divided casts and an HD/settings/fallback
boundary. Existing engine division/accounting and shared lifetime evidence are reused.

### NEPTUN source and delivery choices

**Confirmed source structure:** USA ROM SHA256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. `spelldefs.asm` selects19/51 for NEPTUN1–2;
AQUA uses BUBBLE_BREATH instead. Setup/update contain no rank branch. Setup at
`0x1B21A..0x1B37C` loads invocation2 and tileset14. The shared loader supplies cast77,
four `$CCE` flashes and8 entrance ticks. NEPTUN then plays108 and blinks frame1 at
logical `(128,48)` with21 alternating on/off durations `[1,8,1,7,...,1,1,1]` totaling49.
After a25-tick water hold, it switches to body frame0 and plays110; the water sprite
rises5 pixels for17 updates, alternating its second/third frames every2 updates.
The normal spell load then overwrites the body bank and hides those16 sprites. After20
more updates, rain begins. Original sprite hiding is preserved rather than leaving the
body superimposed on the rain.

Update at `0x1DFD4..0x1E134` releases the damage hold at40. Every3 updates it regenerates
19 rain streaks at fixed source x offsets and cosmetic-random y24..135, each with a
16×32 top and16×16 bottom. Three sizzles start at offsets2/5/8, use three16×16 frames
with5/5/12-tick durations and final blinking at y128, then wait a random4..11 updates.
Sizzle placement uses random x0..255 and cue115. The background table is `[1,0,-1,0]`.
Tileset14's42 tiles provide rising frames0/6/12, rain18/26 and sizzle30/34/38 (zero-based).
Palette3 slots9/13/14 receive `$EA6/$6AC/$E0E`; invocation palette-word0 and VRAM gap use
the accepted decoder unchanged.

[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/neptun.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/neptun.asm).

**Modern delivery:** a187/60-second minimum covers the setup and initial rain hold;
water continues across ordered recipients until the existing Reward/End reset restores
caster/weapon/ground. Deterministic rain positions and sizzle phases replace cosmetic RNG;
no gameplay RNG is drawn. Plane shake, palette flashes, clock and audio scheduling are
approximations. Reduced Flash suppresses flashes, shake and body/sizzle blinking, keeping
art, cues and outcomes. Held1.5× uses the shared clock. Existing77/110 audio is retained and already prepared108/115 recordings are mounted.
Pinned `sounddriver.asm` headers `sfx_2C`/`sfx_33` and USA ROM readback at driver
base`0x1EC000` give108=YM4/5 (the third pointer is inactive) and115=PSG tone3/noise. Those two entries extend the
existing whole-clip replacement policy; this is not a new audio lifetime mechanism.
The ten-frame group selects HD atomically or falls back entirely to Original; partial
Original fails explicitly. An absent group retains the existing modern fallback.
**Unknown:** natural higher-rank training/reach, hardware cadence/palette/GPU fidelity and
human aesthetic acceptance. Controlled admission and node observations do not close them.

### NEPTUN verification

Select complete `derived/issue2313/neptun-v1`, independent #2252 opening, #2255 Witch
and #2266 catalogs. Use the [locked Debug build](../development-and-verification.md#locked-net-workflow).

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2313/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case neptun2-multi `
  -Sample neptun-hd-review -Graphics hd -Reduced -Fast -SaveRoot $freshExternalSaveRoot
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case neptun1-single `
  -Sample neptun-fallback-review -Graphics hd -Variant incomplete -SaveRoot $freshExternalSaveRoot
# Original: omit -Graphics and -Variant, with another fresh sample.
```

**Confirmed in the remake:** the production reader accepts52 bindings in26 scenes and
both controlled Continue records. Rank1 retains the accepted Battle26/DAO-derived Kazin
SORC20 book `[93,94,31,63]`; rank2 uses controlled SORC26 and its legal destination book
`[93,94,95,32]`. Both retain Wooden Rod and seed`0x0DA20000`. The higher level/book are
controlled prerequisites, not natural training. Ordinary Continue → Magic → target → Confirm
observes selectors19/51, rank1 damage37 and rank2 ordered damage38/30 across two actual
recipients, one12/19-MP payment, death cleanup, one action reward and usable next ActionChoice.
NEPTUN's existing critical denominator16 accounts for the first-recipient critical values;
no damage, division or RNG rule changed. Existing summon unit assertions own accounting.

Node/draw observations see all ten resource identities and dimensions, caster replacement,
water rising from y64 to-21 (held1.5× samples reach-16),19 paired rain parts separated by32
pixels, body absence during rain, and started cues77/108/110/115. Published-input production
reader, HD/Reduced Flash/held-speed and incomplete-HD fallback pass with producer paths
unavailable. Reader peak is976MB; native witnesses peak1.34GB and take under5 seconds.
Engine verification passes3,383 with50 optional private-input skips; final Release and Debug
adapter builds pass. No helper tests, gameplay screenshots or frame collection are added.

Retained completed failures: the first local witness reached playback but stopped with
`audio-command-unavailable` because prepared108/115 were not mounted; the second stopped
with `sfx-slots-unavailable` before the two source-backed slot entries were added. Both
reported `bounded return to control`; their complete receipts remain preserved. The ROM
readback also corrected an initial draft108 mask: its third channel pointer is inactive.
Setup-only failures include an incorrect source-directory assumption and missing local
TEMP directory warnings before creation. Successful native exits retain8 ObjectDB warnings
under#1716; no cleanup repair or human/GPU/hardware acceptance is claimed.

## APOLLO family playback #2312

**Standard scope.** Bind admitted SORC APOLLO1–2 to invocation3 and tileset19 through
existing content, summon lifetime and action authority. Payment, actual-recipient division,
ordered results/death/rewards and usable return remain unchanged. No new admission,
promotion, recruitment, enemy AI, Battle27+, save schema or #1716 repair is included.

Ownership: `ApolloEffectView.cs`, `BattleSceneView.cs` and its `Summon` partial;
`PrivateAttackSpellBindings.cs`; `remake_apollo_mount.py`, the shared invocation decoder
extracted from `remake_dao_mount.py`, existing attack-mount/private-package/retail hooks
and engine-path registration; `engine_apollo_family_observation.gd`. This file,
battle-scenes, church-promotion-design, presentation-assets, asset-inventory and
capability-status own the serial documentation edits. The accepted DAO implementation
is the dependency; no unmerged code, new lifecycle or gameplay state authority is used.

### APOLLO source and delivery choices

**Confirmed source structure:** USA ROM SHA256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. `spelldefs.asm` selects APOLLO18 and
APOLLO|VARIATION2 (50); `PrivateAttackSpellBindings` now carries both actual selectors.
The setup/update routines contain no rank branch. This is distinct from BLAZE.

`animation/apollo.asm` (`0x1B0D0..0x1B21A`) calls the common invocation loader with3,
plays lightning118 and lowers all16 body sprites by8 pixels for22 updates. Invocation3's
header at ROM `0x12E988` is `(272,1,1)`, giving logical origin `(144,-127)` before lowering.
Frame1 is loaded into the hidden bank while frame0 remains visible for30 updates; then
the bank switches to frame1 and frame2 is preloaded. The mouth graphic at `(146,75)`
blinks for20 updates with sound116. The common loader's cast77, four `$CCE` flashes,
8-update entrance, palette-word0 skip and 16-tile VRAM gap match DAO's existing decoder.

`animation/update/apollo.asm` (`0x1DE24..0x1DFD4`) alternates body banks and mouth
positions every6 updates. Its first counter clears the damage hold at40. Up to20
particles use two sizes, four frames at3-update intervals, source cosmetic RNG positions
x0..127/y16..175 and flips; large flames drift left1 pixel/update and request116.
`LoadSpellTilesetForInvocation` maps tiles0..23 to VRAM1920. Setup copies tiles24/40/56/72
to VRAM384/528/1440/1776. Mouth frames use tiles0/4; small flames use20/16/12/8;
large flames use72/56/40/24. Palette3 slots9/13/14 receive `$EEE/$26E/$02A`.
The backdrop offset table is `[1,2,1,0,-1,-2,-1,0]`; the red pulse table is `$00F,8,1`.

Paths are relative to `disasm/code/gameflow/battle/battlescenes/`; the loader is in
`battlesceneengine_1/3.asm`, layout in `battlesceneengine_4.asm`, and the spell table in
`disasm/data/stats/spells/spelldefs.asm`.
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/apollo.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/apollo.asm).

**Modern delivery:** the shared cast trigger starts a148/60-second minimum:28 flash,
8 entrance,22 lowering,30 held-body,20 mouth and40 flame ticks. Body, mouth and flames
continue across ordered recipients; Reward/End restores the caster/weapon/ground
immediately using the existing lifecycle rather than reproducing the hardware palette
fade. A fixed20-slot,2-tick spawn schedule with40-tick reuse replaces cosmetic RNG;
visual scheduling consumes no gameplay RNG. Backdrop displacement and red flashes
approximate the original plane/palette operations. Reduced Flash suppresses those
flashes and displacement while preserving sprites/audio/outcomes. Held1.5× uses the
shared clock. Existing sound77/116/118 recordings and explicit audio default selection
are reused, with deterministic flame cues; no new audio capture or BLAZE assets are used.

The13-frame Original/HD group is atomic. Partial HD selects all Original; partial
Original reports `apollo-original-group-incomplete`. Packages with no APOLLO group
retain explicit modern fallback. [NEPTUN](#neptun-family-playback-2313) uses the same boundary; [ATLAS](#atlas-family-playback-2314) now shares that lifecycle.

### APOLLO reproduction and verification

Select complete `derived/issue2312/apollo-v1`, independent #2252 opening, #2255 Witch
and #2266 catalogs. Build the Debug adapter with the [locked workflow](../development-and-verification.md#locked-net-workflow).

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2312/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case apollo1-single `
  -Sample apollo-review -SaveRoot $freshExternalSaveRoot
# Distinct rank/divided recipients: apollo2-multi -Graphics hd -Reduced -Fast.
# Atomic fallback: apollo1-single -Graphics hd -Variant incomplete.
```

**Confirmed in the remake:** the production reader accepts52 rank bindings across26
scenes and both controlled saves. They reuse the accepted Battle26 input and DAO
controlled history, with Kazin SORC20, legal destination book `[93,94,31,63]`, the
Wooden Rod and seed`0x0DA20000`. These are controlled prerequisites, not natural training.
Ordinary Continue → Magic → target → Confirm observes APOLLO1 damage21 and APOLLO2
ordered damage24/25 across two recipients, one10/17-MP payment, death cleanup,
one action reward and usable next ActionChoice. The probe reads actual13 resource
identities/dimensions, draw calls, caster replacement, selectors18/50 and sound receipts.
Both ranks' art is the same. Existing summon unit assertions own division/accounting.

With producer scratch unavailable, published-input reader, HD/Reduced Flash/held-speed
and incomplete-HD atomic-fallback witnesses pass on the final Debug build. Engine
verification passes3,383 with50 optional private-input skips. No helper tests or
screenshots are added. An audit corrected the initially displayed preloaded pose before
final playback checks. Retained setup failures are incorrect initial source-directory
assumptions, a UTF-8 report read with the system encoding, and a formatter line-length
failure; none is a runtime acceptance result. Some native exits retain8 ObjectDB warnings
under#1716. **Unknown:** natural reach/training, hardware cadence/palette fidelity and
human visual acceptance. Exact Git objects, selected gates and actual CI belong to the PR.

## DAO family playback #2310

**Standard scope.** Admitted SORC DAO1–2 uses invocation0 and tileset3 on the existing
scene, raster reader and action authority. No gameplay admission, save format, Battle27+,
other summon family or #1716 repair is included. Application still pays once, fixes the
actual recipient list before dividing power, and delivers ordered damage, deaths and rewards.

Ownership is `src/sf2tool/remake_dao_mount.py`, its existing attack-mount/private-package/
retail CLI hooks and engine-path registration; `PrivateAttackSpellBindings.cs`;
`DaoEffectView.cs`, `BattleSceneView.cs`, `BattleSceneView.Summon.cs`; and
`engine_dao_family_observation.gd`. This file, battle-scenes, church-promotion-design,
presentation-assets, asset-inventory and capability-status own documentation. These are
serial shared-file edits. No unmerged dependency or parallel renderer is introduced.

### DAO source and deliberate delivery choices

**Confirmed source structure**, pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`: `spelldefs.asm` selects DAO17 and
DAO|VARIATION2 (49). The existing unsilenced learned caller reads that selector.
`animation/dao.asm` (`0x1B012..0x1B0D0`) calls `LoadInvocationSpell` with0, then
loads frames1–7, holding frame5 for20 updates with lightning118. Its common loader
plays cast77, flashes `$CCE` four times, saves/replaces the ally, loads frame0 and waits8.
The invocation header is `(256,176,1)`: a128×128 arrangement at logical `(128,48)`
after the VDP bias. `LoadInvocationSpriteFrameToVram` skips palette word0 and loads
colors1–15. `layout_Invocation` places the second128-tile stream after a16-tile VRAM gap.

`animation/update/dao.asm` (`0x1DCE8..0x1DE24`) has no rank branch. It emits up to16
small8×8, medium16×24 and large24×32 rocks from tiles0,1 and7. Source cosmetic RNG
chooses velocity `-1792..-1025` in8.8 units, x0..255, y128..191 and flips; acceleration
is6/256 per update and each spawn plays rumble93. The setup admits damage after90
updates, while scene script termination controls the remaining effect. Background
modification4 has amplitude1; target modification3 alternates accumulation over20
updates. `$248` palette pulses use random1–2 on/30–59 off delays. Common
`ReinitializeSceneAfterSpell` clears graphics, palette/effect state and summon sprites;
`bsc05_makeAllyIdle` ends the effect, fades the invocation out and restores the saved ally.

These paths are relative to `disasm/code/gameflow/battle/battlescenes/`; the loader and
layout are in `battlesceneengine_1/3/4.asm`, and reinitialization is in
`updatespellanimation.asm`. The table is `disasm/data/stats/spells/spelldefs.asm`.
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/dao.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/dao.asm)

**Modern delivery.** The existing cast trigger starts a152/60-second presentation
minimum:28 flash ticks,8 entrance ticks,26 manifestation ticks and90 rock ticks.
The summoned body replaces caster/weapon/ground. Rocks, backdrop displacement and
target texture deformation continue across ordered recipients, then reset at Reward
or End. This handoff restores the caster immediately instead of reproducing the source
invocation palette fade. Deterministic rock positions/velocities/flips, an8-tick spawn cue schedule,
2/45-tick brown pulses and a target-texture shear approximate the source cosmetic
motion without consuming gameplay RNG or emulating VDP planes. Flash colors affect
the backdrop rather than selected hardware palette indices. Reduced Flash suppresses
flashes, shaking and deformation, preserving sprites, timing, audio and outcomes.
Held1.5× uses the shared clock. Existing explicit audio defaults select rumble93 at
Timer189 and lightning118 at Timer194 when the scene requests Timer198.

The11-resource group selects HD atomically. An incomplete optional HD group uses all
Original frames. A partial Original group reports `dao-original-group-incomplete`;
older packages with no DAO group retain their explicit modern fallback. Another spell
family is never substituted. Common summon responsibilities are confined to caster
replacement and scene restoration; [APOLLO](#apollo-family-playback-2312) uses that boundary.
[NEPTUN](#neptun-family-playback-2313) uses the same boundary; [ATLAS](#atlas-family-playback-2314) now shares that lifecycle.

### DAO reproduction and verification

Select complete `derived/issue2310/dao-v2`, independent #2252 opening, #2255 Witch and
#2266 catalogs. Build the Debug adapter using the
[locked workflow](../development-and-verification.md#locked-net-workflow). From a checkout:

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2310/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case dao1-single `
  -Sample dao-review -SaveRoot $freshExternalSaveRoot
# Cases: dao1-single, dao1-multi, dao2-single, dao2-multi.
# Settings: dao2-multi -Graphics hd -Reduced -Fast.
# Atomic fallback: dao1-single -Graphics hd -Variant incomplete.
```

**Confirmed in the remake.** The production reader checks52 rank bindings across26
scenes and restores four controlled records. They derive from the published #2307
Battle26 input, set Kazin to legal SORC12 with destination book `[93,30,31,63]`, retain
the equipped Wooden Rod, use seed`0x0DA20000`, and select one or two nearby recipients.
These are controlled prerequisites, not natural training or acquisition evidence.
Ordinary Continue → Magic → target → Confirm observes DAO1 damage22 versus11/9 and
DAO2 damage50 versus25/19, one8/15-MP payment, first-target death in the two-recipient
cases, ordered HP/death/reward events and usable next ActionChoice. Both ranks retain
the same art. HD/Reduced Flash/held speed and incomplete-HD atomic fallback pass.
The observer reads actual texture dimensions, resource identity, draw calls, phase,
caster visibility, target material and sound receipts; it takes no screenshots.

With producer scratch moved out of its working path, the published reader and fresh
HD/Reduced Flash/held-speed and incomplete-HD ordinary-input runs passed again.
`uv run sf2 verify engine` passed 3,383 with 50 optional private-input skips; Release
and Debug adapter builds passed. No gameplay rule changed and no helper tests were added.

Retained failures include missing Pillow on the first pilot, an unnecessary request
for already inherited audio during the first publication, the previously unset DAO
selector, an over-max controlled target HP, and a one-frame caster visibility reset
at phase changes. These are corrected at their owners. Rejected `dao-v1` had the wrong
invocation palette offset; only `dao-v2` is the review candidate. Native processes can
retain8 ObjectDB exit warnings under#1716; that issue is not repaired here.

**Unknown:** natural reach/training, original hardware cadence and palette fidelity,
actual GPU deformation appearance, and human visual acceptance. The Draft PR owns
the selected plan, actual CI and exact frozen Git objects.

### ABSORB implementation boundary #2309

**Standard**, because this is a bounded presentation binding with accepted equipment
and result authorities. Bind pinned ABSORB selector10 for admitted Misty Knuckles30,
Indra Staff92, Supply Staff96 and Demon Rod101. Preserve Item gestures, target MP loss
then capped caster gain, zero cast payment, selected-slot wear, reward and next control.
No learned/enemy admission, Battle27+, save-format change or #1716 work is included.

Owned implementation paths are `remake/game/src/Battles/AbsorbEffectView.cs`,
`remake/game/src/Battles/BattleSceneView.cs` and
`remake/game/probes/engine_absorb_family_observation.gd`. Documentation ownership is this
file, `battle-scenes.md`, `battle-interaction-baseline.md`, `../capability-status.md`,
`../domain/equipment-use-repair-design.md`, `../content/asset-inventory.md` and
`../content/presentation-assets.md`. Existing audio publication needs no new producer,
schema, planner registration or raster group: source setup flashes and update returns.

Acceptance uses an inherited-audio pilot, production-reader closure for all four equipment
bindings, short ordinary-input drain/gain and full-caster/empty-target observations,
representative Original/HD/settings coverage, affected engine/adapter checks and published
input reproduction with producer scratch unavailable. The accepted complete package already
contains the required cue and appearances, so no duplicate successor is needed. Any successor
would remain below500MiB; scratch/peak RAM stays below2GiB and native samples run serially.
Natural reach, hardware timing/palette fidelity and human aesthetics remain **Unknown**.

## ABSORB family playback #2309

**Confirmed source structure.** At pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `spelldefs.asm` selects ABSORB10 for
SPOIT1. `itemdefs.asm` has four SPOIT equipment rows: Misty Knuckles30 (MMNK),
Indra Staff92 (WIZ/SORC/VICR), Supply Staff96 (WIZ/SORC) and Demon Rod101
(WIZ/SORC/VICR). Whole-equipment admission already includes all four; the older
Indra-only witness below its equipment owner is historical evidence, not today's limit.

`animation/absorb.asm` at `0x1ACD8..0x1ACE4` plays `SFX_WARP`89 and calls
`ExecuteSpellcastFlashEffect` with `$000F`. The setup loads no tiles; the ABSORB
entry in `updatespellanimation.asm` is `RTS`. The shared flash writes palette1
color0 and palette3 color1 for four4-tick pulses separated by3-tick black periods.
Equipment's Use selector follows `battleactions/getspellanimation.asm` and retains
the body's Item animation. These paths are relative to
`disasm/code/gameflow/battle/battlescenes/` except the named battleactions and
`disasm/data/stats/{spells,items}` tables.
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/absorb.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/updatespellanimation.asm)

**Modern delivery.** `AbsorbEffectView` pauses the existing Item pose across28/60
seconds and projects source red onto the existing black backdrop. This follows the
accepted BUFF approximation, not hardware palette-index emulation. Reduced Flash
suppresses red while preserving timing, sound and results. Held1.5× uses the shared
clock. The effect resets when ActionAnimation ends; it consumes no gameplay RNG and
does not change MP, wear, EXP or next control. Existing content without selectable89
keeps its prior Item/result presentation. Declared corrupt audio still fails admission.

There is no ABSORB raster/HD group. Actor and weapon groups keep their existing atomic
Original/HD selection. `derived/issue2307/desoul-v1` already supplies the complete
content and warp cue. Battle music requests Timer198 and the registered explicit
default selects the prepared Timer203 recording; no hardware-exact sound claim is made.
No new media extraction, producer, schema or asset successor is required.

### ABSORB reproduction and verification

Retain the complete #2307 package plus independent #2252 opening, #2255 Witch and
#2266 catalogs. The immutable `derived/issue2309/review-v1` supplies the consumer,
controlled records and ordinary-input launchers. Build the Debug adapter using the
[locked workflow](../development-and-verification.md#locked-net-workflow), then run
from the repository root with a fresh sample and an absolute disposable save root:

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2309/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case indra `
  -Sample absorb-review -SaveRoot $freshExternalSaveRoot
# Other cases: demon-full and supply-empty.
# Settings: demon-full -Graphics hd -Reduced -Fast; indra -Graphics hd -Variant incomplete.
```

**Confirmed in the remake.** Production admission checks104 SPOIT bindings and702
equipment appearance selections across26 scenes, including Item gestures and complete
body/weapon resources. It restores all three records. They derive from the published
#2307 WIZ15 Kazin Battle26 record, keep his equipped wooden rod, replace held slot1
with92/101/96, set Silence, use seed`0x0DA20000`, and set caster/target MP to0/37,
60/37 and0/0 respectively. They are legal controlled prerequisites, not natural custody.

The [native observer](../../game/probes/engine_absorb_family_observation.gd) drives
Continue → Item → target → Confirm → next ActionChoice. Indra drains37→32 and gains0→5;
Demon Rod drains37→32 while the full caster stays60; Supply Staff's empty target and
caster stay0. Both reactions remain ordered even at zero, the selected bare copy is
damaged once, one EXP award follows, and further input works. The WIZ Item sequence21,
four flashes, one warp89, zero MP payment, preserved HP and unrelated slots all pass.
HD/Reduced Flash/held1.5× and incomplete-HD whole-body fallback pass. With producer
scratch moved out of its working path, the published reader and HD/full-caster entry
passed again using only registered inputs. Misty Knuckles
has reader/appearance closure; this slice does not rerun every unchanged equipment caller.

Require `passed: true`, process exit0 and no script/adapter errors. The first empty-target
observer incorrectly required two MP-change events at zero; its failed receipt is retained.
The corrected observer requires both reactions but emits no expectation of an MP-change
event unless the value changed. Existing engine tests retain capped recovery, zero target,
wear/destruction and rewards; no tests of verification programs were added.

Engine verification passed3383 with50 optional private-input skips. Debug/Release adapter
builds pass. The retained complete package is178 files /374,714,972 bytes; the pilot decoded
24,093 stereo frames (96,372 bytes) from its existing warp default. Initial native peak was
1,328,558,080 bytes and producer scratch was395,500,164 bytes, within2GiB. Some successful runs retain four ObjectDB exit warnings
under#1716. The frozen PR owns exact Git objects, selected gates and actual CI.

**Unknown.** Natural acquisition/reach, later campaign consumers, original hardware cadence,
palette-index behavior, actual GPU appearance and human visual acceptance. Original composition,
modern timing/audio and headless state/input/resource observations are separate boundaries.

## Source and status

**Confirmed** for table membership and selector assignments. The [USA ROM manifest](../../../manifests/roms/sf2-us.json)
and the [byte-identical vanilla rebuild](../../../docs/research/reproducible-original.md) bind
the source to SF2DISASM revision `c834c652b6862bc5679fd7f69a38a7093206efc6` with
`VANILLA_BUILD=1`. Read `table_SpellDefinitions` at `0x176A6–0x1796E`.
It has 89 records in 44 families, including item, innate and special effect records.
`SPELL_NOTHING` is a sentinel. The animation setup/update tables also contain standalone
selectors such as arrows and spear projectiles. Their own consumers govern those effects.
[Spell definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) ·
[Selector equates](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm)

For a static comparison, run these reads with `$upstream` set to the pinned checkout and
`$pin` to the commit above. Pair each `entry` with its next `animation`.
Read equipment and other action overrides before claiming a caller uses the table value.

```powershell
git -C $upstream show "$($pin):disasm/data/stats/spells/spelldefs.asm" |
    Select-String '^\s+(entry|animation)\s+'
git -C $upstream show "$($pin):disasm/code/gameflow/battle/battleactions/getspellanimation.asm"
git -C $upstream show "$($pin):disasm/data/stats/items/itemdefs.asm" |
    Select-String '^\s*(;\s*\d+:|useSpell\s+)'
```

In the selector column, v1–v4 means the logical variation numbers, in rank order unless stated.
The encoded variation bits are `00/20/40/60` hex in `sf2enums.asm`, and the enemy
origin flag is `80` hex. Use the equates and `sf2macros.asm:animation` for encoding.
The [original scene dispatcher](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm)
reads the rank selector for an unsilenced learned cast or equipment Use. Non-equipment Use
keeps selector NONE. The battle action can override the table, including Prism Laser. Compare
actual caster, target faction and action kind at the caller boundary.

- **Original assets bound** records actual private playback within the named package. **Partial**
  identifies which assets, action pose or feedback the player sees and which effect remains open.
- **Modern presentation** records a deliberate delivery choice with its acceptance boundary.
  **Placeholder** records an effect or scene retained pending presentation work. An authored
  witness can prove real input, resource and result delivery while its effect remains a placeholder.
- **Unavailable** means the named production gameplay or content path is missing. **Unknown**
  marks evidence still needed, including natural reach when only a controlled state was exercised.
  The [prepared retail library](../content/presentation-assets.md#prepare-and-mount-retail-resources)
  has the raw spell graphics. Actual scene admission, composition and playback determine coverage.
  Human visual acceptance remains with [#1456] and the wider presentation review [#1616].

The private [attack bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs),
[support bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs),
[healing and item bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)
and [encounter reader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
establish which source operands enter gameplay. Current book, faction, target, MP, AI command and
encounter admission still control the available action. The
[scene continuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)
owns ordered gameplay delivery. [BattleSceneView](../../game/src/Battles/BattleSceneView.cs) and
[AttackSpellView](../../game/src/Battles/AttackSpellView.cs) establish the adapter paths. Ordinary
placeholder attack Magic hides the scene actors and scenery and draws element-specific shapes.
Prepared DISPEL uses the mounted scene and cloud group. Other ordinary support Magic and suppressed casts use cards and messages on black. Item gestures, number
of recipients and actual reaction results follow the committed action.

## Graphics variants

Prepared BLAZE1–4, FREEZE1–4, BOLT1–4, BLAST1–4, DESOUL1–2, breath and wave rasters can select a
complete optional HD frame group from the battle scene. The light fairy uses a single group for body, wings and
dust. Partial groups use the complete Original art. Effect positions, mirroring, opacity
and Reduced Flash read the existing playback state. Higher density keeps the Original
draw extents and does not change cast or result delivery. Placeholder effects remain as
listed by rank below. [Binding contract](../content/asset-inventory.md#battle-raster-variant-binding-2177)

## Coverage by family and rank

### Recovery, support and retreat

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| HEAL 1–4 | HEALING_FAIRY v1–v4 | **Original assets bound** for private allied self/teammate casts. The private cast/idle, light fairy, live target and 77/113 cues are described in [HEAL spell scenes](#heal-spell-scenes). Native private HEAL1–3 settings cases and Sarah's Battle26 HEAL4 cover allied delivery. Enemy HEAL1 is observed with authored Mage/Cleric content, and a controlled private Battle19 Master Monk uses HEAL3 on itself. **Original assets bound** for the controlled pre-voyage enemy HEAL1 through the shared faction selector and evil-fairy group. | [Enemy healing acceptance](#enemy-healing-family-2227) covers a legal Dark Cleric recovery. Other higher-rank enemy playback and exact timing remain **Unknown** beyond named cases. [Enemy witness](../development-and-verification.md#enemy-spell-observation) · [Battle19 comparison](../application/battle19-repeat.md#controlled-comparisons) |
| AURA 1 | HEALING_FAIRY v1 | **Original assets bound** for admitted private allied content through the HEAL light-fairy path. Karna's allied ordered recovery has engine evidence and ordinary native campaign casting is **Unknown**. The private Battle19 Dark Bishop shares the faction selector and evil-fairy group; actual enemy AURA delivery remains **Unknown**. Authored AURA uses its own pulse, recipient cards and one EXP delivery. | [#2104] recovery follow-up after pre-voyage priorities. Reuse the same enemy fairy decision and verify actual area recipients when needed. [Current spells](../application/church-promotion-design.md#current-battle-spells) · [Battle19 private binding](../application/battle19-repeat.md) · [Authored AURA](battle-scenes.md#authored-aura-recovery) |
| AURA 2–4 | HEALING_FAIRY v2–v4 | **Original assets bound** for learned private allied Magic through the existing fairy path. Controlled Battle26 input covers ranks2/3 and VICR rank4, one payment and ordered return. | [Recovery and reproduction](../domain/gameplay-rules.md#private-learned-aura-ranks). Equipment Use rules and consumers are accepted under [#2268](../domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268); White Ring AURA2 now has [equipment recovery playback](#equipment-recovery-playback-2291). |
| DETOX 1–3 | DETOX v1–v3 | **Original assets bound** for admitted private Magic in the [DETOX successor](../content/presentation-assets.md#detox-successor). The same mote/trail composition plays for effective and paid no-effect casts and is suppressed by silence. Field Magic and Antidote/Fairy Powder have distinct delivery. Rune Axe Use binds DETOX2 through the [equipment recovery consumer](#equipment-recovery-playback-2291). | [#2232] delivers the [controlled native presentation](#prepared-detox-presentation). [#1965] owns rank-2/3 gameplay and equipped curse. Natural ordinary acquisition and casts remain **Unknown**. |
| DETOX 4 | DETOX v4 | **Original assets bound** for private learned Magic with one cast and one end wait around the ordered area results. | [Area rules and controlled Battle26 playback](../domain/detox-ranks-design.md#detox-4-area-and-field-admission) |
| BOOST 1 | BUFF1 v1 | **Original mechanism bound.** Learned Karna and Protect Ring use the original no-tile flash/PSH family, actual Magic/Item gesture, ordered AGI/DEF and return. | [BUFF playback](#buff-family-playback-2282); existing rule/expiry/save evidence remains with the support owner. |
| BOOST 2 | BUFF1 v2 | **Original mechanism bound.** Selector40 uses the same setup/update as8; learned Karna delivers mixed effective/ineffective area results with one5 MP payment. Enemy SUPPORT remains outside admission. | [BUFF playback](#buff-family-playback-2282). |
| SLOW 1–2 | DEBUFF1 v1–v2 | **Original assets bound**. Both Sarah ranks use the shared gray cloud group with actual AGI/DEF and ordered effective/ineffective results. Variation2 retains selector41 but does not select the separate DEBUFF2 palette. | [DEBUFF family playback](#debuff-family-playback-2280). [Support evidence](../domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction) |
| ATTACK 1 | BUFF1 v1 | **Original mechanism bound** for admitted equipment Use. Power Ring, Protect Ring, Valkyrie and Wish Staff have controlled native witnesses. Private learned ATTACK and ordinary enemy AI admission remain outside this binding. | [BUFF playback](#buff-family-playback-2282); [equipment authority](../domain/equipment-use-repair-design.md). |
| DISPEL 1 | DEBUFF1 v1 | **Original assets bound** for learned Kazin and existing Dark Smoke AI in the [DISPEL successor](../content/presentation-assets.md#dispel-successor). Controlled mixed results, paid silence and Original/HD fallback use ordinary input and completion. | [DISPEL playback](#dispel-playback-implementation-2238) records the source, responsibilities and reproduction. Natural learning/reach and hardware timing remain **Unknown**. Grand Cannon Use has [controlled Item-gesture playback](#debuff-family-playback-2280). |
| MUDDLE 1–2 | DEBUFF1 v1 for both | **Original assets bound**. Both learned Kazin ranks share DEBUFF1; controlled Battle26 mixed-result casts and Giant Knuckles Use mount the same clouds. Item retains its own gesture and wear. Controlled Battle25 rank-2 automatic actions and cold Continue retain their gameplay witnesses. | [DEBUFF family playback](#debuff-family-playback-2280). [Accepted MUDDLE evidence](../domain/hostile-spells-design.md#controlled-muddle-2-and-cold-continue-in-battle25) |
| DESOUL 1–2 | DESOUL v1–v2 | **Original assets bound** for learned Magic and Dark Sword Use in all 26 admitted scenes with the DESOUL package. Rank 2 adds two souls. Only an instant-death result strikes. Dark Sword uses variation 1 with its Item gesture. | [Playback and evidence](#desoul-family-playback-2307) · [Rule and settled-save evidence](../domain/hostile-spells-design.md#desoul-12-implementation-and-evidence) |
| SLEEP 1 | DEBUFF1 v1 | **Original assets bound**. A controlled Tyrin 25 Battle26 slot exercises private Magic, both original icons, localized result/no-effect feedback and cold Continue with an active sleep count. The shared DEBUFF cloud scene now carries ordered effective/ineffective results. Natural Tyrin progression to a cast remains **Unknown**. | [DEBUFF family playback](#debuff-family-playback-2280). [SLEEP boundary](../domain/hostile-spells-design.md#sleep-1-implementation-and-evidence) |
| EGRESS 1 | NONE | **Modern presentation**. Eligible retreat uses the same text, pulse, tone and fade for spell and Angel Wing, with different payment. Battle02 native witnesses cover cancellation, return and walking retry. Later encounters supply their own retreat destinations. | Preserve the accepted [retreat delivery](battle-scenes.md#authored-egress-delivery). Encounter admission belongs to [#1818]. Original visual/audio parity is **Unknown**. |

**Confirmed in the pinned setup/update source.** Both sides load healing tileset 6 and its
palette replacements. The enemy branch selects different tile offsets and initial phase 3,
compared with allied phase 7. The phase operand is not a palette selector. The shared source
update controls subsequent movement, mirroring and dust. [Enemy family evidence](#enemy-healing-family-2227)

### Attack Magic

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| BLAZE 1 | BLAZE v1 | **Original assets bound** in all26 admitted flame-successor scenes, with current caster, target, weapon and backdrop. Existing controlled Kazin Battle04 and Galam Mage Battle05 witnesses cover both sides. Packages without the mount retain the placeholder. | [Prepared BLAZE1](#prepared-blaze1-presentation) and [flame successor](#flame-family-playback-2293). Broader natural reach remains encounter-owned. |
| BLAZE 2 | BLAZE v2 | **Original assets bound** across the 26 admitted scene mounts in the flame successor, including Black Ring/Heat Axe/Mage Staff consumers. Controlled learned Battle04 and Item Battle26 witnesses use ten small meteors. | [Flame family playback](#flame-family-playback-2293); natural reach remains encounter-owned. |
| BLAZE 3 | BLAZE v3 | **Original assets bound** across admitted scenes: large flame and fourteen large meteors. Controlled Battle26 WIZ Kazin, Wizard enemy and Atlas Axe Use cover both factions and Item. | [Flame family playback](#flame-family-playback-2293). No new acquisition or encounter admission. |
| BLAZE 4 | BLAZE v4 | **Original assets bound**: large flame, four large meteors and eight-part flame dragon. Controlled Battle26 WIZ casts hit one opponent. | [Flame family playback](#flame-family-playback-2293); modern clock and floating-point trajectory are distinct from original timing. |
| FREEZE 1–2 | FREEZE v1–v2 | **Original assets bound** in all26 freeze-successor scenes. Controlled WIZ Tyrin casts cover both ranks; Battle19 Dark Madam covers enemy FREEZE2, with source-side origins and opposite rotation. | [FREEZE family](#freeze-family-playback-2298), including modern clock and natural-reach limits. |
| FREEZE 3–4 | FREEZE v3–v4 | **Original assets bound** through admitted learned casts; Freeze Staff Item uses v3. Controlled WIZ Tyrin covers area v3 and single-target v4 with its extra glint. Blizzard consumable remains NONE despite its table row. | [FREEZE family](#freeze-family-playback-2298). No new equipment acquisition or encounter admission. |
| BOLT 1–2 | BOLT v1–v2 | **Original assets bound** across all26 lightning-successor scenes, including Halberd1, Evil Ring2 and enemy Taros Sword2. Controlled native input retains Magic versus Item, zero-MP equipment and enemy no-wear. | [Lightning family](#lightning-family-playback-2301); natural acquisition/reach remain encounter-owned. |
| BOLT 3–4 | BOLT v3–v4 | **Original assets bound** through admitted learned casts. v3 has three concurrent strike streams/two orbs; v4 has four/four. Radius2 versus single-target gameplay is unchanged. | [Lightning family](#lightning-family-playback-2301). THUNDR table membership does not admit Holy Thunder or establish its consumable animation. |
| BLAST 1 | BLAST v1 | **Original assets bound** in all26 wind-successor scenes. Controlled Sarah retains the fifteen fragment/slash rasters, actors, backdrop and cues. | [Wind family](#wind-family-playback-2306); older BLAST1-only mounts remain supported. |
| BLAST 2–4 | BLAST v2–v4 | **Original assets bound** in all admitted wind-successor scenes. Even ranks grow larger fragments; v3/v4 add cyclone/dust. Controlled Sarah covers all ranks and existing Battle26 Dark Bishop covers hostile BLAST3. | [Wind family](#wind-family-playback-2306). No new equipment consumer, gameplay or encounter admission. |
| KATON 1–3 | BLAZE v2–v4 | **Original assets bound** through the actual selector, with neutral gameplay rules unchanged. Native NINJ rank1 uses v2; rank3 uses v4. Rank2 has production binding coverage, without a new native cast. | [Flame family playback](#flame-family-playback-2293). Natural learning/acquisition remain **Unknown**. |
| RAIJIN 1–3 | BOLT v2–v4 | **Original assets bound**, preserving distinct neutral gameplay. Controlled NINJ ranks1/3 use the shared selectors; rank2 has production-reader binding coverage. | [Lightning family](#lightning-family-playback-2301); natural learning/acquisition remain **Unknown**. |
| DAO 1–2 | DAO v1–v2 | **Original assets bound** for admitted SORC casts: invocation0, rising rocks, scene deformation and sound. Both ranks preserve actual-recipient division. | [DAO playback and limits](#dao-family-playback-2310). Natural training, hardware timing and human visual acceptance remain **Unknown**. |
| APOLLO 1–2 | APOLLO v1–v2 | **Original assets bound** for admitted SORC casts: invocation3, mouth and two flame sizes, selectors18/50. | [APOLLO playback](#apollo-family-playback-2312) covers controlled single/divided recipients and atomic Original/HD; natural training, hardware timing and human visual acceptance remain **Unknown**. |
| NEPTUN 1–2 | NEPTUN v1–v2 | **Original assets bound** for admitted SORC casts: invocation2, rising water, rain and sizzle, selectors19/51. | [NEPTUN playback](#neptun-family-playback-2313) covers controlled single/divided recipients and atomic Original/HD; natural training, hardware timing and human visual acceptance remain **Unknown**. |
| ATLAS 1–2 | ATLAS v1–v2 | **Original assets bound** for admitted SORC casts: invocation1, opposing two-pose body streams, selectors20/52. Enemy HEAL rank probing does not admit enemy summons. | [ATLAS playback](#atlas-family-playback-2314) covers controlled single/divided recipients and atomic Original/HD; natural training, hardware and human acceptance remain **Unknown**. |

### Item, innate and special rows

| Retail rows | Original selector | Gameplay, package and scene | Remaining owner |
| --- | --- | --- | --- |
| SPOIT 1 | ABSORB v1 | **Original mechanism bound** for Misty Knuckles30, Indra Staff92, Supply Staff96 and Demon Rod101 Item Use. Source warp89 and four red flashes accompany the existing Item gesture, ordered target MP loss and capped caster gain. | [ABSORB playback](#absorb-family-playback-2309) reuses the complete DESOUL package. No learned Magic or enemy admission. Equipment rules remain with [#2268](../domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268); natural late custody remains **Unknown**. |
| HEALIN 1–3 | HEALING_FAIRY v1–v3 in table, NONE for consumable Use | **Original assets bound** for the ordinary private Herb scene's item gesture and recovery. Seed and Drop share the admitted HP item consumer, with a bounded Seed supply witness. Holy Lance equipment Use reads the actual selector. | Preserve [item recovery](battle-scenes.md#medical-herb-scenes). Holy Lance Use is admitted under [#2268](../domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268); [#2291](#equipment-recovery-playback-2291) binds its Item gesture and fairy presentation. |
| FLAME 1–3 | FLAME_BREATH v1–v3 | **Unavailable** for these enemy spell rows. Kiwi's private converted Attack has separate assets and rules. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| SNOW 1–3 | SNOW_BREATH v2–v4 | **Unavailable** for these enemy spell rows. FREEZE already has a separate Magic path. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| DEMON 1–2 | DEMON_BREATH v1–v2 | **Unavailable** for these enemy spell rows. | [#1818] first enemy caller and [#1369] rule admission, then [#2104] breath playback. |
| POWER 1, GUARD 1, SPEED 1, IDATEN 1, HEALTH 1, HANNY 1, BRAVE 1 | NONE for each | **Modern delivery** in Field/Caravan, with item result and actual stat/level/growth changes; outside the battle close-up. Ordinary Water, Milk and Chicken Use remain **Unknown**, with rule tests. | Preserve the [permanent growth item scope](../domain/permanent-growth-items-design.md). Any additional caller needs its own evidence under [#1818]. |
| B_ROCK 1 | BURST_ROCK_EXPLOSION v1, also chosen by the action override | **Unavailable** in the connected campaign. The authored laser path excludes special Burst Rock targets and their action. | [#1369] and [#1818] own special action and encounter admission. [#2104] effect at first admitted caller. [Special-mechanics design](../domain/special-mechanics-design.md) |
| LASER 1 | NONE in table, PRISM_LASER by action override | **Modern presentation**, finite authored ray with charge, fire, reactions/deaths and continued control. Private Battle36 and original visual/audio effects are outside that acceptance. | [#1370] and [#1818] private special/encounter admission, then [#2104] review desired private presentation. [Authored laser](battle-scenes.md#authored-prism-laser) |
| POWDER 1 | NONE | **Partial** for private battle item cure with ordinary gesture and notice. Field/Caravan Use has its own status and feedback. | Keep this item separate from DETOX graphics. [Fairy Powder and higher cure boundaries](../domain/detox-ranks-design.md) |
| G_TEAR 1–2 | NONE | Rank 2 **partial** through admitted Fairy Tear item gesture and MP recovery notice, including ordinary Battle26 input. Rank 1 has a table row and no known retail item binding in `itemdefs.asm`. | Rank 1 live caller remains **Unknown**. Reuse the existing item route only if a caller warrants it. [Fairy Tear evidence](../domain/recovery-items-design.md#ordinary-input-observations) |
| FBALL 1 | BLAZE v3 in table, NONE for consumable Use | **Original item gesture bound** for Shining Ball with ordinary private damage, custody and return. | Preserve the distinct item route with [#2104] and the [offensive-item owner](../domain/offensive-items-design.md#ordinary-remake-observations). |
| BREZAD 1 | FREEZE v3 in table, NONE for consumable Use | **Original item gesture bound** for Blizzard with ordinary private damage, custody and return. | Preserve the distinct item route with [#2104] and the [offensive-item owner](../domain/offensive-items-design.md#ordinary-remake-observations). |
| THUNDR 1 | BOLT v3 in table, NONE for consumable Use | **Unavailable** as Holy Thunder private Item Use. | [#1818] item rule/admission. Reuse the ordinary item scene when admitted. |
| AQUA 1–2 | BUBBLE_BREATH v1–v2 | **Original assets bound**: seven frames per rank, distinct palettes, actual Kraken Head and Battle23 Bubbling Ooze bodies/terrain. Willard itself has no spell. | [AQUA playback](#aqua-family-playback-2315): reader closure, ordinary enemy actions and atomic Original/HD/settings; natural/hardware/human acceptance remains Unknown. |
| KIWI 1–4 | FLAME_BREATH v3 for all | **Partial** original assets. The optional MNST mount supplies the dedicated cast sequence and two prepared Flame Breath tiles. A controlled private Attack conversion projects the modern particle stream. All four ranks differ by level/power and share the visual path. | [#2104] later breath group can reuse the mounted assets when appropriate. Natural MNST promotion/breath in the connected pilot and exact particle motion remain **Unknown**. [Controlled Monster pilot](../application/ordinary-promotion-classes.md#monster-controlled-reproduction-and-ordinary-pilot) |
| SHINE 1 | NONE | **Unavailable**. Right of Hope from the Fairy Woods trial is outside the current campaign. | [#1818] trial/item caller, with the accepted field/item delivery boundary. [Flag and receipt](../content/campaign-flags.md) |
| ODDEYE 1 | ODD_EYE_BEAM v1 | **Unavailable** in the connected campaign. | [#1818] first encounter, [#1369] action rules and [#2104] beam effect. |

## Pre-voyage dispatch

1. **P2 [#2105], BLAZE2.** See the [prepared mount and observations](#prepared-blaze2-presentation).
2. **P2 [#2106], BLAST1.** After [#2105] acceptance, use learned Sarah from a legal pre-voyage
   state (PRST learns BLAST1 at level 10 in `allystats01.asm`), 2 MP and one adjacent opponent.
   Its source radius is zero. Prepare its own tileset-15 wind/dust composition, including the
   v1 side selection in `spellanimationSetup_Blast` and `spellanimationUpdate_Blast`.
   Later enemy BLAST ranks have their own encounter and package checks. Reuse the playback and
   publication interfaces from the accepted slices. The two issues have independent gameplay rules.
   [Blast setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/blast.asm) ·
   [Blast update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/blast.asm)

For each, use the [prepared BLAZE1](#prepared-blaze1-presentation) lifecycle and an isolated fresh
private package. Read the live projection, token and errors through one short ordinary-input
controlled cast on legal state. Check the mounted art and current scene identities, one payment,
each actual reaction, release, and the existing Reduced Flash, skip and held-rate behavior at their
applicable waits. Use modern timing within the accepted playback contract. Retain exact source
composition and the results of any failed observations. Natural pre-voyage learning/casting and
foreground visual acceptance remain **Unknown** where no named witness covers them.

## Follow-up order

- **P2 after the two child issues.** [#2104] retains first-use review of pre-voyage recovery and
  support. The [enemy healing family](#enemy-healing-family-2227) covers its bounded HEAL1
  recovery. Enemy AURA delivery remains **Unknown**. The [DETOX successor](#prepared-detox-presentation)
  covers learned ranks 1–3. [DISPEL playback](#dispel-playback-implementation-2238) supplies
  the first DEBUFF consumer. The [DEBUFF family](#debuff-family-playback-2280) extends it to
  admitted SLOW/MUDDLE/SLEEP and two equipment callers. The [BUFF family](#buff-family-playback-2282) has its own no-tile mechanism and consumer acceptance. [#1456] governs foreground acceptance and [#1616] the wider design review.
- **P3 for current later content.** [#2104] groups the remaining elemental variants and shared
  wind effects, then any additional support callers as admitted. The [lightning family](#lightning-family-playback-2301) covers admitted BOLT/RAIJIN and equipment. The [flame family](#flame-family-playback-2293) covers admitted KATON; the [FREEZE family](#freeze-family-playback-2298) covers admitted ice ranks and Freeze Staff. Private allied AURA2–4
  has [controlled ordinary coverage](../domain/gameplay-rules.md#private-learned-aura-ranks).
  [#2016], [#1967] and [#1926] own missing gameplay and item admission. Keep item gestures where
  the original selector path calls for them. Schedule equipment effect reuse at its first legal
  consumer.
- **Campaign-dependent.** [#1818], [#1369] and [#1370] own first-use chapter or action admission
  for future breaths, summons and special attacks. [#2104] retains effect coverage until the
  selected caller exists and the accepted original assets or modern decision can be named.

[#2232]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2232
[#1716]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716
[#1456]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456
[#1616]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616
[#1369]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369
[#1370]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370
[#1818]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818
[#1926]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926
[#1957]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957
[#1965]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965
[#1967]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967
[#2016]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2016
[#2104]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104
[#2105]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2105
[#2106]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2106

## Prepared BLAZE1 presentation

**Standard scope (#2068).** Battles04–07 can mount BLAZE1's small flame from the prepared
private effects library. The production scene shows the caster's cast pose, the current target,
the equipped weapon where applicable, and the selected background and ground. Ally and enemy
casts place the flame on the recipient's side. The cast pose cues the flame and cast sound.
Completion waits for both the pose sequence and the 48-tick flame. Reduced Flash holds one flame
frame with a smooth fade. The existing hit/coin message setting can suppress the coin notice
while the animation and spell damage notice retain their own waits. Session rules still deliver
cost, ordered reactions, death, rewards and next control.

**Confirmed source identity.** USA retail ROM `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` bind `spelldefs.asm`
BLAZE1 to animation selector 1. `spellanimationSetup_Blaze` loads tileset 2, whose table entry
is `SpellGraphics02` at `0x1BF3E6`. `LoadSpellTileset` replaces indices 9, 13 and 14 of
`palette_BattlesceneBase` (`0x198A8`) with `$022E`, `$028E` and `$08EE`. The registered 7,104-byte
tiles and base palette compare byte-for-byte to the decoded ROM stream and ROM palette.
`table_1A5E4` and `table_1BE2A` select column-major 2×4 sprites at zero-based tiles 33 and 41.
Each variation-1 entry duplicates the other at the same position. The adapter draws a single
16×32 flame at (72,112) or (176,128) on the 256×224 local canvas. The source cast sound is 77.
Fade, frame cadence, reaction motion and delivery on the Godot clock are modern presentation choices.

`createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` selects `BATTLEANIMATION_ATTACK`
for ordinary spellcasters, including Kazin and Galam Mage. `GetAllyAnimation` resolves Kazin's
class sequence 4 and `GetEnemyAnimation` resolves Galam Mage's species sequence 36. Battle04's
Kazin row exposes sequence 4 through `idle`, identical to its `attack` sequence. The registered
Battle05 Galam Mage row exposes sequence 36 through `attack`. Battles06–07 provide a `cast` alias
with exactly the same operands as `attack`, including trigger 2 and the frame 2→3 pose. The adapter
prefers a published `cast` row and otherwise reads the existing ordinary pose. The pose's trigger
cues the modern flame interval.

**Publication.** Select the registered fixed-backdrops package as `$base`, its prepared `retail-v1`
asset library as `$library`, and a fresh ignored absolute output `$candidate`. Load worktree private
selections in the same shell. The mount reads two existing tile frames and reuses the closed-package
publisher. An omitted `blaze-1/` namespace retains the legacy effect. A partial pair, extra frame
or wrong dimensions fails scene admission.

```powershell
uv run sf2 rom verify
uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blaze
```

To reproduce the source comparison, follow `p_pt_SpellGraphics` at `0x1B800C` through table slot 2.
Decode its stream at header offset +8 with `decode_stack_compressed`, using
the first header word as expanded length. Compare with `effects/spell-2.4bpp`, then compare the 16
palette words at `0x198A8` with `effects/base-palette.json`. Apply the three replacement words from
the header and use the named tile operands above to compare the two mounted rasters. The selected
package has 142 byte-identical dependencies and four scenes equal to their base values after
removing the two new rasters from each. The logical size increases by 4,496 bytes, and the largest
changed scene is 879,858 bytes against the 4 MiB reader limit. The publisher hardlinks unchanged
inputs on the same volume and copies changed scenes before writing. A cross-volume copy needs about
315 MB. Complexity is linear in the selected package's file closure, with worktree-local scratch.

**Native reproduction.** Select the candidate `world.json`, `main-scenes.json`, `entry/entry.json`
and six `opening-inputs` files in their ordinary `SF2_PRIVATE_*` environment selections. Use a fresh
worktree-local report/runtime and a disposable task-specific external save namespace. Provide
`SF2_BLAZE_START` pointing to an ignored controlled-start JSON with these values:

```json
{"start":{"battle":"battle-4","activeAllies":["ally-0","ally-4"],"actors":[{"actor":"ally-4","position":{"x":4,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}}]}}
```

The existing controlled startup mounts Battle04 with Kazin's normal learned BLAZE1 and stages one
legal target in range. The 1 HP target exercises the actual kill and rewards within one cast.
For each run set `SF2_OBSERVATION_OUTPUT` to a fresh file and select a version-1 input settings file
with `suppressBattleHitCoinMessages` and `reducedFlash` false, then only the former true, then only
the latter true. Set `SF2_BLAZE_SKIP_MESSAGES` and `SF2_BLAZE_REDUCED_FLASH` to matching `0`/`1` values
for the observer. Build the locked Debug adapter before launching the installed Godot console.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility --fixed-fps 60 `
    --resolution 1920x1080 --path remake/game --script res://probes/engine_prepared_blaze_observation.gd -- `
    --input-settings $settings
```

**Confirmed in the remake.** All three runs select and confirm BLAZE1 with ordinary input. Normal
and coin-skip play both flames, Reduced Flash plays the first. The read-only projection sees
Kazin's cast frame, Galam Soldier, rod, background, ground and the source flame. Cast cue 77
starts once on the animation token. Early Confirm
retains the animation token. The spell deals 6 damage, MP changes 13→11, Kazin gains 49 EXP and
one kill, and gold changes 60→110. The next actor, Bowie, opens a new action menu in the same
session. The coin notice is visible in normal and Reduced Flash and passes without input in skip.
The spell result remains readable in all three. The recordings cover actual resource binding and
timed playback. Headless exit reported ten ObjectDB instances after control returned.

**Enemy-side reproduction.** With the same private package and observer, set `SF2_BLAZE_ENEMY=1`,
`SF2_BLAZE_SKIP_MESSAGES=0`, `SF2_BLAZE_REDUCED_FLASH=0`, and use normal input settings. Point
`SF2_BLAZE_START` at a fresh ignored controlled-start JSON containing these values:

```json
{"start":{"battle":"battle-5","activeAllies":["ally-0"],"actors":[{"actor":"enemy-2","position":{"x":9,"y":6}}]}}
```

**Confirmed in the remake.** Battle05's SENTRY Galam Mage moves (9,6)→(5,5) and casts BLAZE1
at Bowie (3,5) through the ordinary enemy decision. Both frames draw at (176,128) with
`enemy36/frame3`, `ally0-palette0/frame0`, the equipped sword, `background8` and `ground8` visible.
The session records one cast, one MP payment (5→3), one HP change (12→6) and one enemy action
commit. Early Confirm retains the cast token. Bowie opens a new action menu in the same session.
The observer reports no session, scene or audio error and exits 0. Eight ObjectDB instances are
reported at exit. Original timing and foreground visual acceptance (#1456) remain **Unknown**.

## Prepared BLAZE2 presentation

**Standard scope ([#2105]).** Private Battles04–07 mount BLAZE2 for learned Magic. The same
cast pose, weapon, backdrop, cast cue 77 and completion token used by BLAZE1 now carry a
medium ground flame and ten small falling meteors with impacts. Subsequent reactions show the
actual recipient in order. A missing rank-2 mount uses the existing primitive effect.
Packages with a rank-2 namespace require all six correctly sized rasters.

**Confirmed source composition.** The BLAZE2 row in `spelldefs.asm` uses
`BLAZE|VARIATION2` (selector `0x21`), 6 MP, range 1–2, radius 1 and power 10.
`table_1A594` and `table_1A5BC` select ten small meteors and the following side operands.
The flame is drawn as a 24×48 raster made from a 3×4 top and 3×2 bottom at their original
relative positions. The table's tile indices are zero-based below.

| Branch | Flame origin | Bottom origin | Meteor entry X window start | Meteor direction and art |
| --- | --- | --- | --- | --- |
| Ally casting at enemy | (68,96) | (68,128) | 80 | Leftward, horizontal flip |
| Enemy casting at ally | (172,112) | (172,144) | 48 | Rightward, original orientation |

`table_1BE2A` uses top/bottom tiles 49/61 and 67/79. `table_1BE42` uses tiles 149 and
153 (16×16). `table_1BE52` and `table_1BE5A` use tiles 157 (24×24) and 166 (24×16)
for the small impacts. The second impact frame sits 8 pixels lower. The original update chooses
meteor X within a 96-pixel window and starts impact at or below Y 120 in local coordinates. In the remake, the
projector deterministically spreads ten meteors over 1.35 seconds from the existing cast-pose
trigger. Reduced Flash holds the first flame, meteor and impact frame and retains all ten
trajectories. The adapter uses its own motion, fade and scheduling with no palette flash.
The ally-side rank-2 branch was compared statically with the original side table.
[Blaze setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/blaze.asm) ·
[Blaze update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/blaze.asm)

**Publication.** Load the private worktree selections and select the prepared #2001 retail library
as `$library` and the accepted #2068 BLAZE1 package as `$base`. Choose a fresh absolute
`$candidate` under worktree `local/` for validation or the registered shared `derived/`
root for handoff. Reuse the package's current `world.json` and scene selections.

```powershell
uv run sf2 rom verify
uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blaze-2
```

Compare tiles and palette to the ROM via the [tileset-2 procedure](#prepared-blaze1-presentation).
Compare each mounted raster with the column-major tile operands above. The 146-member package
has 142 byte-identical dependencies and four scenes equal to their base values after removing the
six rank-2 rasters from each. The logical size grows by 14,240 bytes. Allow about 3.3 MB plus small
raster scratch for a same-volume mount and about 315 MB for a cross-volume copy. The largest
changed scene is 883,418 bytes against the 4 MiB reader limit.

**Native reproduction.** Use the [BLAZE1 launch and private selectors](#prepared-blaze1-presentation)
with the mounted rank-2 candidate. Use a fresh runtime and disposable save namespace per run.
Set `SF2_BLAZE_RANK=2` and `SF2_BLAZE_ENEMY=0`. Put this start document in a fresh ignored
directory and point `SF2_BLAZE_START` at it.

```json
{"start":{"battle":"battle-4","activeAllies":["ally-0","ally-4"],"partyInput":"kazin.json","actors":[{"actor":"ally-4","position":{"x":4,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}},{"actor":"enemy-2","position":{"x":6,"y":8}}]}}
```

Create the sibling `kazin.json` from the [controlled party template](../../reference/inputs/battle01-player-ready.json).
Set `id` to `blaze-2-controlled-party`, `evidenceOwner` to `issue-2105`, and
`bridgeBoundary` to `Pre-voyage Kazin level and book only`. Replace `allies` with the following array.
The reader decodes the standard private party shape against the opening static inputs and checks
class, equipment and the learned book against the Battle04 actor's admitted growth. MAGE
learns BLAZE2 at level 5. [Kazin's growth table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm)

Only level and book enter the Battle04 controlled start. The stat fields satisfy the controlled party shape.

```json
[{"id":4,"classId":3,"level":5,"maxHp":12,"hp":12,"maxMp":13,"mp":13,"attack":9,"defense":6,"agility":7,"move":5,"status":0,"items":[213,0,127,127],"spells":[75,63,63,63],"exp":0,"kills":0,"defeats":0}]
```

Run the observer with these settings.

| Case | `SF2_BLAZE_REDUCED_FLASH` | `SF2_BLAZE_SKIP_MESSAGES` | `SF2_BLAZE_HELD_RATE` |
| --- | ---: | ---: | ---: |
| Normal | 0 | 0 | 0 |
| Reduced Flash | 1 | 0 | 0 |
| Held rate and coin skip | 0 | 1 | 1 |

Match the version-1 settings file's `reducedFlash` and `suppressBattleHitCoinMessages` to
the observer selections. In the last case the observer holds Shift during the animation, reads
1.5× playback, and releases it before subsequent input.

**Confirmed in the remake.** Ordinary Magic selects rank 2 around the Galam Soldier at (6,7).
The Huge Bat at (6,8) receives the second reaction. Kazin's cast frame and Wooden Rod,
the live recipients, background and ground remain visible. All ten meteors fall and impact.
Early Confirm preserves the cast token, and cue 77 starts once. The three settings cases retain
the same settlement. Kazin pays 6 MP once (13→7), applies two ordered 9-point hits (HP 1→0
and 11→2), gains 49 EXP and one kill, and gold changes from 60 to 110. Skip passes the coin
notice while retaining damage messages. Bowie opens the next action menu in the same session.
Separate rank-1 ally and Galam Mage regressions preserve the accepted cast and usable control.
Godot reported ObjectDB exit warnings after control returned, with no session, scene or audio error.

**Unknown.** Natural pre-voyage learning and casting, a live mirrored BLAZE2 caller, original
timing, and foreground visual acceptance [#1456]. Equipment Use rules are accepted under [#2268](../domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268); its original effect playback remains a separate [#2104] boundary.

## HEAL spell scenes

The bounded HEAL1–4 consumer prepares two award draws, then retains scene/turn ownership through
MP cost, casting, optional ally switch, HP recovery, recovery text, ordinary idle, fairy retirement,
cleanup, actor restoration, EXP/growth and input release. Full HP still casts and awards the healing
minimum; self-healing preserves the already charged caster MP. Growth consumes the seed carried
through the scene, not the construction seed. Private scenes require admitted PRST cast content,
idle parameters and healing rasters; authored packages use an explicitly authored small gesture.
Private player AURA 1 uses the fairy path and checks the actual recovery recipients' scene bindings.
Its engine test checks ordered recovery and turn release. Ordinary player AURA 1 in private content
remains **Unknown**. Public authored AURA and the [finite attack, support and cure spells](battle-scenes.md#which-actions-can-be-reused)
use their own delivery paths. [Battle26 input and save observation](../application/church-promotion-design.md#current-battle-spells)
The historical #523/#437 records do not establish full scene or hardware parity; the
[current composed milestone](../../../docs/design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone)
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
[retained HEAL evidence](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#retained-heal-consumer-evidence).
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
[research attribution](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#recovery-and-make-idle-opportunity-attribution)
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

## Prepared BLAST1 presentation

**Standard scope (#2106).** This bounded adapter/content slice mounts the registered tileset-15
rank-1 fragments and slashes in Battles04–07. It shares the prepared cast clock and completion
token with BLAZE. Sarah's PRST level-10 book supplies the legal adjacent, single-target cast.
Acceptance requires live actors, weapon, backdrop, effect, one payment and settlement, completed
animation, and usable next control under normal, Reduced Flash and held-rate/coin-skip settings.
The implementation owns AttackSpellView, BattleSceneView, BlastOneEffect, the scene reader,
PrivateAttackSpellBindings, the existing retail publisher and its extracted spell-mount helper,
the narrow observation, SessionAudio cue 114 mapping, its producer registration in verification_engine_paths.py,
and these presentation/capability documents.

Publication is linear in the existing package closure. Budget 120 MB of new same-volume package
storage and raster scratch, 400 MB if hardlinks require copying, and 2 GiB publisher memory for
parsing the approximately 102 MB world JSON and its decoded objects.
Four scene JSON files and the world audio binding change. The publisher copies those files before writing and hardlinks
unchanged dependencies. Reassess if the closure or any scene's 4 MiB admission limit is exceeded.


**Confirmed source and identity.** Pinned SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6 and
USA ROM 9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9 select animation 5,
2 MP, power 6, range 1–1 and radius 0. PRST learns BLAST at level 10.
Follow the [existing ROM comparison](#prepared-blaze1-presentation), using slot 15 at
p_pt_SpellGraphics (0x1B800C). Its header is at 0x1C2C56. The decoded 6,816 bytes equal the
registered effects/spell-15.4bpp. Header replacement words are 0EC8, 026C, 0C82. The registered
base palette equals the 16 ROM words at 0x198A8.

Setup initializes eight fragments and one short-slash slot. Rank 1 leaves the even-rank flag clear
and bypasses the rank-3 cyclone and attached dust. The unmirrored/mirrored VDP origins
(208,224)/(312,240) become (80,96)/(184,112) on the local canvas. The six fragment modifiers
and eight slash modifiers stay relative to that origin. Mirroring changes the origin and
preserves those local modifiers.

The publisher uses column-major sprite tiles and the following zero-based operands.

| Frames | Starting tiles | Dimensions in pixels |
| --- | --- | --- |
| Diagonal fragments 0–2 | 0, 1, 5 | 8×8, 16×16, 16×24 |
| Horizontal fragments 3–5 | 23, 25, 29 | 8×16, 16×16, 24×16 |
| Vertical slash 6–8 | 151, 155, 163 | 16×16, 16×32, 16×24 |
| Horizontal slash 9–11 | 169, 173, 181 | 16×16, 32×16, 24×16 |
| Angled slash 12–14 | 187, 191, 207 | 16×16, 32×32, 16×24 |

table_1CBDE adds offsets to VDPTILE_SPELLTILE1, equal to SPELLTILE_START. table_1CC44 supplies
the slash dimensions and tile offsets. The adapter schedules eight fragments and eight short
slashes over 1.4 seconds with deterministic converging motion and fades. Reduced Flash fixes
each family to its middle frame. Timing, sampling and fades are modern approximations.
Cast cue 77 starts at the pose trigger and wind cue 114 starts 0.1 seconds later. The original
requests 114 repeatedly when particles spawn. The adapter uses one whole recording through
the accepted finite mixing policy. sounddriver.asm:sfx_32, driver offset 0x1D08, is type 1
with active channel indices 8/9 pointing at 0x1D1D/0x1D2A, PSG tone 3 and noise.
SessionAudio now admits that combination.

**Publication.** Load the worktree private configuration. Select the registered #2001 library as
$library, accepted #2105 package as $base, and a fresh absolute output as $candidate.
Run the following from the checkout root.

    uv run python -m sf2tool.remake_retail_assets --root $library --base $base --mount $candidate --family blast-1

The stable handoff selection is registered derived/issue2106/pre-voyage-spells-v2. The earlier
v1 retains the visual-only observation. Version 2 adds the registered cue 114 WAV with the existing
audio publisher. Four scenes grow by 20,536 bytes total and the largest is 888,552 bytes.
The final 147-file closure keeps 141 base files byte-identical. The five rewritten files total
105,448,603 bytes and the new WAV is 51,192 bytes. After removing the 15 BLAST rasters from each
scene and cue 114 from world audio, all five parsed documents equal their base documents.

**Native reproduction.** Select the candidate world.json, main-scenes.json, entry/entry.json and
six opening-inputs through the ordinary SF2_PRIVATE_* variables. Use a fresh worktree-local
report/runtime and a task-specific external save namespace. Copy the controlled party template
remake/reference/inputs/battle01-player-ready.json as sarah.json in the report directory.
Set id to blast-1-controlled-party, evidenceOwner to issue-2106 and bridgeBoundary to
Pre-voyage Sarah level and book only. Replace allies with the following array.

    [{"id":1,"classId":4,"level":10,"maxHp":11,"hp":11,"maxMp":10,"mp":10,"attack":9,"defense":5,"agility":5,"move":5,"status":0,"items":[213,0,0,127],"spells":[64,2,14,63],"exp":0,"kills":0,"defeats":0}]

Items and max MP match the opening Sarah baseline. The controlled reader imports only the validated
level/book. HEAL2, DETOX1 and BLAST1 are legal learned entries at level 10. Point SF2_BLAST_START
at an absolute sibling start JSON containing this document.

    {"start":{"battle":"battle-4","activeAllies":["ally-0","ally-1"],"partyInput":"sarah.json","actors":[{"actor":"ally-1","position":{"x":5,"y":7}},{"actor":"enemy-1","hp":1,"position":{"x":6,"y":7}}]}}

Build the Debug adapter using the locked SDK workflow. Set SF2_OBSERVATION_OUTPUT to a fresh report.
Launch the installed Godot with the following arguments and an absolute version-1 settings file.

    & $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility --fixed-fps 60 --resolution 1920x1080 --path remake/game --script res://probes/engine_private_wind_cast_observation.gd -- --input-settings $settings

Normal uses reducedFlash=false, suppressBattleHitCoinMessages=false and observer switches
SF2_BLAST_REDUCED_FLASH, SF2_BLAST_SKIP_MESSAGES, SF2_BLAST_HELD_RATE all set to 0.
Reduced Flash changes its settings field and observer switch to true/1.
The third case sets suppressBattleHitCoinMessages=true, SF2_BLAST_SKIP_MESSAGES=1 and
SF2_BLAST_HELD_RATE=1 with Reduced Flash off.
The ordinary Magic observer advances three rows because HEAL1 and HEAL2 both appear.

**Confirmed in the remake.** All three cases bind caster, target, weapon, background and ground,
read selector 5 and mounted frames, retain early-Confirm ownership, and cover the full effect.
Normal reads 15 frames, Reduced Flash reads 5, and held Shift reads 1.5× pacing.
Cues 77 and 114 each start once. Sarah pays 2 MP (10→8), delivers one 6-point hit to the 1-HP
enemy, gains one EXP and one kill, and gold changes 60→110. Bowie opens the next action menu.
BLAZE1 ally/enemy and BLAZE2 ally regressions pass on the final package.

**Completed failures.** The initial placeholder probe selected DETOX1 because it omitted HEAL2's
list row. The first prepared observation queried a reset effect target during Reaction; the
corrected assertion reads the displayed enemy. Adding cue 114 exposed sfx-slots-unavailable and
three completed deadline failures. The source-backed slot mapping corrected the failure and all
three affected cases passed. Reports remain under local/issue2106-baseline in the owning worktree.
Some runs report 8–10 ObjectDB instances at exit, covered by paused #1716.

**Unknown.** Natural pre-voyage learning/casting, live enemy BLAST1, exact hardware timing and
human foreground visual acceptance. Mirrored placement has static source comparison only.
Higher ranks remain with their existing owners.

## Enemy healing family #2227

Standard rigor applies to this bounded content/presentation correction. Acceptance covers the
shared HEAL/AURA faction selector, existing Original/HD contract, controlled pre-voyage enemy
HEAL1, an allied counterexample and a closed successor package. Natural enemy reach, actual
enemy AURA, private unbound ranks and hardware-exact timing remain **Unknown**.

**Confirmed** from pinned SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6,
`animation/healingfairy.asm` and `animation/update/healingfairy.asm`. Light body offsets are
0/16, wings 32/40 and dust 52 down through 48. Evil body offsets are 53/69, wings 85/93
and dust 105 down through 101. Both use tileset 6, palette 3 and the same three palette
replacements. Light starts at source sprite X384/phase7 without mirror; evil starts at
X96/phase3 mirrored. Y is 128 plus the existing range32 draw. Ranks 1/2/3/4 allocate
1/1/2/3 fairies with these same banks. Dust spawns at the current fairy position plus (12,12).
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/healingfairy.asm)
· [Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/healingfairy.asm)

The existing HealingSceneCursor selects the initial side from the caster for every healing
caller. Domain retains the existing update, lifetime and draw schedule. BattleSceneView uses
source coordinates minus the VDP origin and selects `healing.enemy` for enemy casts. Older
packages without that optional group retain their light artwork. Each selected body/wing/dust
group requires complete HD coverage; one missing alternate falls back to its whole Original
group. Graphics preparation includes both groups before publishing the preference.

**Confirmed** by the native `engine_enemy_healing_art_observation.gd` controlled Battle06
start. Dark Cleric enemy-0 chooses HEAL1 on Galam Soldier enemy-8 through the production AI.
One payment changes MP7→4 and recovery caps HP1→11. All nine evil body/wing/dust resources
are actual Sprite2D textures, with source-coordinate placement and stable logical footprints.
Separate HD, Original and missing-HD-dust4 runs return to ordinary player control with no enemy
EXP. The existing `engine_hd_heal_observation.gd` Battle04 self-HEAL counterexample covers
all nine light resources, one MP payment, HP recovery and ordinary Settings HD/Original changes.
No screenshots or campaign replay are part of these observations.

Reproduce with the complete package below and the existing locked Debug/Godot launch setup.
Set `SF2_HEAL_START` to a worktree-local JSON with `start.battle="battle-6"`,
`activeAllies=["ally-0","ally-1"]`, enemy-1 through enemy-7 HP0, enemy-8 HP1,
ally-0 position (7,25) and ally-1 position (6,25). Each override is an object in `start.actors`
with `actor`, optional `hp` and optional `position:{x,y}`. Retain the source cleric position,
loadouts and spellbook. Run the observer with a fresh disposable namespace and
`SF2_OBSERVATION_OUTPUT`; use `SF2_HEAL_EXPECT_HD=1` with HD settings, or `0` for Original
and a disposable package missing just `healing-enemy/dust4` from the Battle06 HD bank.
The allied recipe is in [HD rollout](../content/hd-rollout.md#private-battle-artwork-acceptance).

The behavior tests cover both source entry sides, rank reuse, motion, dust, retirement and
unchanged random draw setup. The engine suite completed with 3263 passes and 50 skips.
Adapter Release and Debug builds passed. Retained failures include an initial C# local-name
collision, a producer assumption about a nonexistent `scene.json` (the actual opening owner is
`main-scenes.json`), a missing Pillow invocation, a 64×64 observation viewport and a probe field
lookup. Final native runs have no adapter/script errors; some retain a four-ObjectDB exit warning.
Peak native memory and subjective appearance remain **Unknown**.

Verification on the committed slice uses `uv run sf2 verify plan --base origin/main --head HEAD
--scope engine`. The exact new producer is registered in `REMAKE_PRODUCER_PATHS`; research
classification rules retain their existing meaning. The plan and the PR's actual scope job select
`engine-unit`, `adapter-build`, `private-admission-build`, `live-inspection-build`,
`h4-comparison-build` and `research-public`. All six selected local checks passed, including
locked auxiliary builds and the direct Ruff/contract/index checks. CI conclusions are recorded
in the PR and independent acceptance handoff. Native evidence belongs to the earlier behavior
checks above; these build jobs do not imply new gameplay observations.

## Prepared DETOX presentation

[#2232] uses Standard rigor for one battle-scene effect across the admitted Magic ranks. The
acceptance boundary is a closed private successor, legal rank-1 cure and no-effect casts, the
rank-2 range/status and rank-3 cost differences, and cancellation, silence and graphics fallback.

**Confirmed** from the pinned USA source. Learned DETOX selects 6/38/70/102 for ranks 1–4.
The dispatcher also sets bit 7 for enemy casts. The DETOX setup and update read neither the
rank variation nor the mirror flag. Both use tileset 7, the same six mote frames and five
trail frames, and the same two arcs. The mote's VDP centers are (304,192) and (208,192),
with X/Y amplitudes 48/16 and eight angle units per update. The trail starts at (+8,+8)
relative to the current mote, drifts down one pixel per update and changes tile at six-update
intervals. Sprite priority group 2 links before group 1. The mote stays in group 2 and trail
particles take group 1 or 2 from the spawning angle. The pixel-space origin in close-up is
the source VDP coordinate minus 128. [Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/detox.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/detox.asm) ·
[Coordinate calculation](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/graphics/graphics_1.asm) ·
[Sprite links](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/graphics/graphics_2.asm)

The original script debits MP and starts the cast animation before switching to the recipient
and delivering its result. An effective clear has the status reaction and healing cue 113.
The no-effect path reports the failed cure after the same cast. Silence still pays MP and
skips the animation. The scene ends the spell after its notices, waiting for the shrink and
remaining trail before it returns to the caster. [Cast and silence dispatcher](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm) ·
[Selector](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm) ·
[Status effect](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm) ·
[Spell end](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_0.asm)

The adapter gives the existing cast trigger and SpellStop wait to DetoxEffectView. It
keeps the same live effect through caster/recipient switches and result text, then acknowledges
SpellStop after cleanup. Original or HD supplies all eleven frames at the same logical size.
Any missing HD frame selects all Original frames. A package with no DETOX group keeps the
earlier cast pose and notice route. An optional Original group must declare all eleven frames
at their source dimensions. The modern clock delivers the source cadence and orbit with a
local trail-spacing stream and a caster tint for the cast burst. Reduced Flash omits the tint.
The session retains its established mana, recipient, result, EXP and RNG authority. Field
Magic, Antidote and Fairy Powder keep their own delivery. [Package selection](../content/presentation-assets.md#detox-successor) ·
[Rank rules](../domain/detox-ranks-design.md)

**Confirmed in the remake** using the real private package and ordinary Magic input in
Godot 4.7.2 .NET. The probe reads actual Sprite2D textures, dimensions, both arcs, front/back
trail layers over the close-up, the live recipient, result and end wait. Cancel is free and
early Confirm retains the animation token. Each cast ends on Bowie's usable action menu in the
same session.

| Controlled entry | Mounted bank and observed result |
| --- | --- |
| Battle04, Sarah DETOX1 on clean Bowie | All eleven Original frames. MP 10→7, no effect and 1 EXP. Cast cue 77 once. |
| Battle09, Sarah DETOX1 on poisoned Bowie | All eleven HD frames. Poison 2→0 with stable HP, MP 10→7 and 5 EXP. Cues 77/113 once each. |
| Battle04, Sarah DETOX2 on Bowie at distance two | HD selected with one DETOX alternate missing. Whole Original group, Reduced Flash, STUN 1→0, MP 10→4 and 5 EXP. Cues 77/113 once each. |
| Battle04, Sarah DETOX3 on clean Bowie | Shared composition, selector 70, MP 10→0, no effect and 1 EXP. The accepted rank-3 equipment cure has its own Battle26 witness. |
| Battle04, silenced Sarah asks to cast DETOX1 | MP 10→7, silence notice, no DETOX effect, status write, cast/recovery cue or EXP. |

### Reproduce DETOX in Godot

Use a fresh worktree-local run directory, authorized disposable save root and unique
namespace. Load the locked SDK and private-input configuration, build Debug and join
the eleven-frame package from [the package owner](../content/presentation-assets.md#detox-successor).
Select its world, main scene, entry and six opening inputs together through their usual
SF2_PRIVATE_* selections. Use independent run and preference directories for each launch.

Copy remake/reference/inputs/battle01-player-ready.json into the run directory as sarah.json.
Set id to detox-art-controlled-party, evidenceOwner to issue-2232 and bridgeBoundary to
Controlled Sarah level and book only. Replace allies with one row as below. Choose the level
and packed DETOX entry from the table.

    [{"id":1,"classId":4,"level":4,"maxHp":11,"hp":11,"maxMp":10,"mp":10,"attack":9,"defense":5,"agility":5,"move":5,"status":0,"items":[213,0,0,127],"spells":[0,2,63,63],"exp":0,"kills":0,"defeats":0}]

Write start.json beside sarah.json and select it via SF2_DETOX_START. The start hook admits
the normal world, then imports Sarah's validated book and the controlled status before
mounting the session. The clean Original case uses this document.

    {"start":{"battle":"battle-4","activeAllies":["ally-0","ally-1"],"partyInput":"sarah.json","actors":[{"actor":"ally-0","position":{"x":4,"y":7},"status":0},{"actor":"ally-1","position":{"x":5,"y":7},"status":0}]}}

| Case | Start edits from the clean Original case |
| --- | --- |
| Active poison and complete HD | battle-9, Bowie (6,2) status 2 and Sarah (6,1). |
| Rank 2 and missing HD frame | Sarah level 13, packed spell 66 and position (5,8). Set Bowie status 1. Use a disposable successor package without detox/frame-10 in the Battle04 hdRasters bank and set reducedFlash to true. |
| Rank 3 | Sarah level 33 and packed spell 130. |
| Silence | Sarah status 768. |

Set SF2_DETOX_RANK to 1, 2 or 3, SF2_DETOX_EFFECTIVE to 1 for the two cures, SF2_DETOX_HD
to 1 for the complete HD case, and SF2_DETOX_SILENCED to 1 for silence. Set other switches to 0.
Set SF2_OBSERVATION_OUTPUT to a fresh worktree-local JSON. Use version-1 input settings
with language en and textMode instant. Select graphicsMode hd for complete HD and missing
HD, and original for other cases.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
    --fixed-fps 60 --resolution 1920x1080 --path remake/game `
    --script res://probes/engine_detox_art_observation.gd -- --input-settings $settings
```

**Completed failures.** An overlapping Release build locked the shared Domain intermediate,
then a serial build found an unnamed tuple in projection observation. Serial Release and
Debug passed after correction. The first probe needed an explicit Array type for the cue
readback. Battle04 rejects injected poison, so the poison witness moved to Battle09. That
start exposed a dropped death-credit context in the controlled hook. It now preserves the
admitted value in both start calls. One cancel/reopen rehearsal advanced from the retained
Magic row to HEAL. The probe now closes the reopened list before making the fresh selection.
Receipts and logs are retained in worktree-local issue2232. Some completed processes
reported ObjectDB exit warnings, which retain their separate [#1716] owner.

**Unknown.** Natural poison/stun acquisition for these casts, live enemy DETOX,
hardware-exact flash/timing, subjective foreground appearance and conditions for natural
pre-voyage learning. The controlled Battle04 no-effect cast establishes pre-voyage presentation.
The [DETOX 4 owner](../domain/detox-ranks-design.md#detox-4-area-and-field-admission) covers
the separate controlled area cast.

The engine gate passed with 3,263 tests and 50 optional private skips. The existing tests cover
rank masks, status refresh, equipped curse and separate item/Field handling. Release and Debug
adapter builds and the three standalone tool builds passed. Public Ruff, design-contract and
research-index checks passed. The committed engine-scoped plan and actual CI conclusions are in
the Draft PR handoff.


## DISPEL playback implementation #2238

**Standard, implemented for independent acceptance.** The bounded outcome is learned Kazin DISPEL1 and the already
admitted enemy DISPEL consumer using prepared DEBUFF1, with ordered effective/ineffective
results and paid silence suppression. Domain owns rolls, cost, status and rewards.
Application owns reaction order and completion tokens. Godot owns cast pose, scene actors,
clouds and presentation time. The [family extension](#debuff-family-playback-2280) covers SLOW, MUDDLE and SLEEP.

**Confirmed source structure.** At SF2DISASM c834c652, animation/debuff.asm selects
DEBUFF tileset 9, overrides palette slots 9/13/14 with 0888/0BBB/0DDD, and initializes
four staggered clouds. animation/update/debuff.asm uses 32×32 column-major sprites and
frames 0,1,2,1,0 at five updates per frame, followed by a randomized hidden interval.
Coordinates start at (64,32) for the enemy recipient and (168,48) for the ally recipient,
with separate 0–31 offsets. The adapter uses a local presentation PRNG. Hardware timing
and natural reach remain **Unknown**. The existing scene clock and completion tokens
provide the modern delivery boundary.

The three-frame pilot is 5,614 serialized bytes for Original plus 4× HD. The complete
174-file base is 361,493,189 bytes and its largest scene is 3,188,370 bytes. Publication
adds this group to declared scenes through the existing closed-package publisher.
Budget is 400 MB for the successor and another 400 MB for its atomic staging, under
1 GiB process memory, serial execution and no capture stream. Work is linear in package
bytes plus three fixed rasters. Replan if a scene exceeds the existing 4 MiB limit or
publication exceeds the budget. The pilot uses the accepted graphics filter and checks
alpha masks. Initial pilot failures were missing optional Pillow and unmasked source
palette words. The corrected producer uses the existing Pillow invocation and VDP
0x0EEE color mask. The published successor is 361,637,333 bytes across 174 files, with a 3,193,914-byte maximum scene. A disposable complete copy with one missing HD frame supplies the fallback observation and uses the same 400 MB scratch budget.


The cloud component owns a deterministic cosmetic sequence and the cast-trigger pause.
A minimum 30 cloud-update ticks after the 28-tick flash keeps the three frames deliverable
with instant text. This is a modern duration choice. The existing presenter supplies caster
pose, current reaction target, scenery, and one cast sound. Clouds continue through result
reading and retire before rewards/end. Support TargetEnter retains both opposing actors
in place. It does not use the recovery-only ally slide. Payment, rolls, status and rewards
remain in the existing Domain/Application path. Missing Original groups retain legacy cards.

**Confirmed bounded native observations.** The production input probe
`remake/game/probes/engine_dispel_art_observation.gd` checks mounted Sprite2D textures,
three source frames, 32×32 logical size, target side, foreground layer, cast completion,
ordered messages, single 5 MP payment and usable subsequent action input.

| Controlled case | Observed result |
| --- | --- |
| Battle04 Kazin, Original | Soldier then Bat resist; all three clouds and return |
| Same cast, complete HD and Reduced Flash | 4× group at original extent; cast tint suppressed |
| Same cast, one missing HD frame | Entire cloud group falls back to Original |
| Silenced Kazin | 5 MP paid; no effect RNG, status writes or cloud playback |
| Battle07 Kazin, seed 123456789 | Mage resists, Cleric status 128→896; ordered mixed results |
| Battle07 Dark Smoke AI | SUPPORT selects Sarah then Kazin, both resist; 20→15 MP, ally-side clouds and return |

### DISPEL reproduction

Prepare the successor using its linked package owner. Load worktree private inputs and build
Debug with the locked SDK. Select the package world, main scene, entry and six opening inputs
together. Set a disposable save root/namespace and a worktree-local output path. Use version-1
input settings with instant English text and the named graphics/Reduced Flash values.

Copy `remake/reference/inputs/battle01-player-ready.json` to local `party.json`. Replace
`allies` with the following controlled Kazin row. His MAGE book is legal at level 13 according
to pinned `data/stats/allies/stats/allystats04.asm`. The hook validates class, equipment and
learned entries before mounting.

```json
[{"id":4,"classId":3,"level":13,"maxHp":10,"hp":10,"maxMp":7,"mp":7,"attack":9,"defense":4,"agility":6,"move":5,"status":0,"items":[213,0,127,127],"spells":[75,7,6,63],"exp":0,"kills":0,"defeats":0}]
```

Create a local start document with `start.battle`, `activeAllies`, `partyInput` set to the
sibling party filename, and `actors` rows containing `actor`, `position` and optional
`status`/`hp`. The optional `mainSeed` selects initial state through GameSession.Start.

- Original/HD/fallback/silence use battle-4, active ally-0 and ally-4, at (4,7) and (5,7).
  Put enemy-1 at (5,8) and enemy-2 at (6,8). Silence sets ally-4 status 768.
- Mixed success uses battle-7 and mainSeed 123456789, active ally-0 at (12,34) and ally-4
  at (8,24). Put enemy-2 at (9,24) and enemy-3 at (9,25), both initially status 192.
  Set the other eight enemies' HP to zero in this controlled start.
- Enemy SUPPORT uses battle-7, active ally-0 at (12,34), ally-4 at (8,24), ally-1 at
  (9,24), and enemy-0 at (17,24). Set enemy-1 through enemy-9 HP to zero. This satisfies
  the existing activation and SUPPORT scoring boundary after ATTACK1 cannot reach.

Set SF2_DISPEL_START to the absolute start filename, SF2_DISPEL_CASTER to ally-4 or enemy-0,
SF2_DISPEL_SILENCED to 1 only for silence, SF2_DISPEL_SUCCESS to 1 only for mixed success,
and SF2_DISPEL_EXPECT_HD to 1 only for the complete HD group. Set other switches to 0.
Set SF2_OBSERVATION_OUTPUT to a fresh local JSON. The missing-HD case uses a disposable
package copy with only `debuff-1/frame-2` removed from Battle04's hdRasters.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
  --fixed-fps 60 --resolution 1920x1080 --path remake/game `
  --script res://probes/engine_dispel_art_observation.gd -- --input-settings $settings
```

**Completed failures.** The first native run observed only one cloud frame before immediate
result acknowledgements, corrected by the finite minimum duration. Enemy setups first failed
blocked placement, then left Smoke outside its activation region or allowed ATTACK1 first.
Their receipts remain in local issue2238. Some exited with the existing ObjectDB warning.
The mixed success recipe deliberately sets initial seed and sleeping targets. These are
controlled states, with natural acquisition and natural encounter reach **Unknown**.
The family extension below covers the admitted SLOW/MUDDLE/SLEEP and equipment consumers.
Hardware timing and human visual acceptance remain open under their existing owners.


## DEBUFF family playback #2280

**Standard; implemented for independent acceptance.** This bounded extension reuses
`DebuffEffectView`, its cosmetic clock/PRNG, completion tokens and complete-group fallback.
`BattleSceneView` selects SLOW1/2, MUDDLE1/2, SLEEP1 and DISPEL1 from the actual support
action. Domain/Application retain payment, rolls, effect success, ordered reactions, wear,
expiry and next control. No gameplay rule, new enemy admission, resource bytes or package
binding changes. BUFF, DESOUL, other equipment effect families and Battle27+ are out of scope.

**Confirmed source structure.** At SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to `disasm/`:

- `data/stats/spells/spelldefs.asm` selects9 for SLOW1, both MUDDLE ranks, SLEEP1 and DISPEL1;
  SLOW2 selects41 (`DEBUFF1|VARIATION2`).
- `code/gameflow/battle/battlescenes/battlesceneengine_2.asm:SetupSpellanimation` splits the
  variant from the low-five-bit setup index. Both9 and41 dispatch `spellanimationSetup_Debuff1`.
  `animation/debuff.asm` and `animation/update/debuff.asm` do not branch on variation;
  both use the gray palette and four-cloud update described above. The separately indexed
  `Debuff2`/`Debuff3` setup palettes are not SLOW2's variant.
- `code/gameflow/battle/battleactions/getspellanimation.asm` reads an equipment Use spell
  after `GetEquipmentType` returns weapon/ring. `createbattlesceneanimation.asm` then selects
  `BATTLEANIMATION_USE_ITEM`, and `animateaction.asm` forwards both operands. Thus Giant
  Knuckles31 (MUDDLE1) and Grand Cannon54 (DISPEL1) select9 with their Item gesture, not the
  Magic pose. `battlescenes/battlesceneengine_0.asm:bsc01_animateAllyAction` passes the
  selected operand through `sub_184B0`, which preserves nonzero selector9; the body animation
  header does not replace it. Ordinary consumables do not acquire an equipment overlay merely by name.

The existing source-bound SupportItem/SupportSpell definitions supply these effect identities.
The adapter checks the complete Original group before mounting it and retains its prior fallback
when absent. All three HD frames must exist to use HD. SLOW2 exposes selector41 even though
its art is identical. Source selector, mounted assets and observed caller are separate facts.

**Confirmed bounded remake observations.** The probe
`remake/game/probes/engine_debuff_family_observation.gd` uses ordinary Continue/Magic/Item
and real Sprite2D/state readback. Every final named case reaches the next ordinary action menu.

| Controlled input | Observed delivery |
| --- | --- |
| Battle26 Sarah SLOW1/2 |3/6 MP once; Wizard then Dark Bishop; both succeed from clear words. With Wizard already slowed, ordered ineffective/effective results preserve its slowed stats. Selector9/41, VICR cast25, three cloud frames. |
| Battle26 MAGE25 Kazin MUDDLE1/2 |6/11 MP once; seed123456789 produces ineffective Wizard then Bishop status48/56; cast4 and three frames. Seed0 retains two ineffective results. |
| Battle26 MAGE25 Tyrin SLEEP1 |4 MP once; the same mixed seed gives Bishop status192 after an ineffective Wizard; cast4 and later control. |
| Battle26 MMNK Sarah, bare Giant Knuckles |Zero MP with Silence; Item19, two ineffective results, exactly one wear draw, selected31→287. |
| Battle26 BRGN Rohde, bare Grand Cannon |Zero MP with Silence; Item11, two ineffective results, exactly one wear draw, selected54→310. |
| DISPEL regression |Allied Kazin cast4 and existing Battle07 Dark Smoke SUPPORT both mount clouds. Enemy recipients Sarah then Kazin use origin(168,48); one5 MP debit and return. No new AI family is admitted. |
| SLOW2 settings representatives |Original, complete4× HD with Reduced Flash/held Shift, and HD request missing frame2 with hit/coin suppression all finish. Missing HD falls back for the whole cloud group. Gameplay event order, HP/MP, status, stats, inventory, EXP and both RNG seeds agree across the three runs. |

Original clouds retain32×32 logical extent and foreground layer4. Both actors/scenery stay
mounted, early Confirm cannot complete the cast token, the cast sound occurs once, result
messages name the actual recipient and clouds retire before return. Suppressed hit/coin text
does not suppress these status-result acknowledgements. Modern timing and cosmetic RNG remain
the accepted DISPEL policy; natural acquisition/reach, exact retail timing and human visual
acceptance remain **Unknown**. Existing rule/expiry/cold-save evidence is reused, not recertified
by this presentation-only slice.

### Family reproduction and retained failures

Use immutable `derived/issue2274/equipment-v2` (174 files,368,469,461 bytes), independent
#2252 opening, #2255 Witch/entry catalog and #2266 world catalog under `SF2_SHARED_INPUT_ROOT`.
No new asset successor is published. `derived/issue2280/review-v1` contains the controlled
records, launchers and compact receipts; it needs no producer checkout. Build the Debug adapter
with the [locked SDK environment](../development-and-verification.md#locked-net-workflow).
From a configured isolated checkout, select a fresh sample and disposable external save root:

```powershell
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2280/review-v1'
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case slow2 `
  -Seed slow2-recast.json -Sample slow2-review -SaveRoot $disposableSaveRoot
# Other exact inputs: slow1-recast.json, muddle1-mixed.json, muddle2-mixed.json,
# sleep-mixed.json, cannon.json, knuckles.json, dispel.json (matching -Case).
# -Enemy with -Case dispel exercises the existing Battle07 SUPPORT route.
# -Graphics hd -Reduced -Fast and -Graphics hd -Fallback -Skip use slow2.json.
```

Records derive from the accepted #2273 Grand Cannon seed. Each moves the named holder to
Kazin's known square, puts Wizard/Bishop at(5,24)/(4,24), and swaps that holder to the first
queue slot. Sarah retains the accepted learned book; Kazin uses legal MAGE25 entries
[139,71,6,8]. Tyrin replaces Karna's deployment/queue/flags and adds learned SLEEP at25.
The MMNK witness replaces Sarah's equipment with a bare Giant Knuckles and no weapon layer.
SLOW recast uses version5 with explicit class terms and Wizard status0x0C00. These are legal
controlled states, not unchanged campaign histories. The launcher stages records locally and
never modifies registered inputs. Fallback makes one complete local copy and removes only
Battle26 `hdRasters["debuff-1/frame-2"]`.

Work is linear in one selected package read plus bounded scene input. The fallback copy is
under500MiB; no full trajectory or frame captures exist. The2GiB scratch budget includes the
single disposable package and local build/import/cache state; native cases are serial. No new
asset acquisition/API cost, schema, planner, framework or engine test is introduced. Selected
adapter/build and direct document checks own this diff; exact head/CI are in the frozen PR.

**Completed failures retained:** the first Continue probe outran asynchronous loading; adding
bounded wall time fixed the probe. Initial Grand Cannon attempts selected an empty slot, then
used typed Array.find against JSON floats; integer slot matching corrected the observer. The
first Tyrin record omitted the joined/deployed flag transfer and its process ended in the
existing Godot finalizer access violation (0xC0000005); the corrected record admits normally,
but causation of that native crash remains **Unknown**. The old enemy probe started gameplay
under the independent opening overlay and could not acknowledge; the family probe now skips
opening through ordinary input first. One intermediate observer quit before awaiting its
verification coroutine and emitted script errors despite a success JSON; that run is failed.
Final runs have no session/presenter/script error, but several retain6–8 ObjectDB exit warnings.
These warnings and the finalizer failure remain with #1716; this slice claims no lifecycle fix.

## BUFF family playback #2282

**Standard; bounded presentation scope.** BOOST1/2 and admitted ATTACK1 equipment Use
consume the source mechanism through `BuffEffectView` and the existing support scene.
Domain/Application retain MP, status construction/refresh, RNG, wear, ordered results and
next actor. No new gameplay rule, learned ATTACK or enemy AI admission is introduced.
The existing actor/raster loader, completion tokens, held-rate clock and audio voices remain
owners; no new shader, asset producer, schema or animation framework is needed.

**Confirmed source structure.** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
paths relative to `disasm/`:

- `data/stats/spells/spelldefs.asm` selects8 for BOOST1/ATTACK1 and40 for BOOST2.
  `battlescenes/battlesceneengine_2.asm:SetupSpellanimation` strips the variation bits;
  both dispatch `spellanimationSetup_Buff1`. The separately indexed Buff2 is not BOOST2.
- `code/gameflow/battle/battlescenes/animation/buff.asm` (1AB9E–1ABF4) loads no tiles.
  It plays77, invokes the cast flash with `$2C2`, and initializes interval30, delta−2,
  countdown1. `animation/update/buff.asm` (1CFFC–1D038) emits PSH97 at intervals28,26,…,2
  and ends at zero:14 cues. It never branches on variation. Spell tileset8 is unrelated.
- `battlesceneengine_3.asm:ExecuteSpellcastFlashEffect` writes palette1 color0 and
  palette3 color1 for four4-tick flashes separated by3-tick black periods. `tintscreen.asm`
  assigns BUFF `tint_None`; no cloud, particle or full-scene tint is implied.
- `battleactions/getspellanimation.asm` reads an equipment item's Use spell;
  `createbattlesceneanimation.asm` retains `BATTLEANIMATION_USE_ITEM` and `animateaction.asm`
  forwards its effect operand. `battlesceneengine_0.asm:bsc01_animateAllyAction` and
  `sub_184B0` preserve the nonzero effect. Power Ring19, Protect Ring20, Valkyrie80 and
  Wish Staff94 select ATTACK/BOOST through this path. Ordinary consumables are unchanged.
- `code/common/tech/sound/sounddriver.asm:sfx_21` is type2 with only its second pointer
  active (`byte_19CB`; other pointers immediately terminate at `byte_19D5`). `Load_SFX`
  maps that slot to YM5. SessionAudio admits97 in its existing whole-clip YM5 group.

The adapter uses the source green on the existing black backdrop, preserves actor/body/weapon
rasters and lets the source pose pause across the flash. It waits for the bounded effect before
releasing the action token. Reduced Flash suppresses the backdrop flashes, retaining pose,
PSH cadence and results. This is the accepted modern clock/composition approximation, not
per-index hardware palette emulation. No gameplay RNG is consumed. Original/HD selection and
whole-body fallback remain in the existing raster binder. The complete successor adds only
prepared PSH audio; [publication and reproducible command](../content/presentation-assets.md#buff-successor).

**Confirmed bounded remake observations.** The direct probe
`remake/game/probes/engine_buff_family_observation.gd` drives Continue, Magic/Item, target,
Confirm and the next ActionChoice through ordinary input. It inspects the actual Sprite2D
texture, backdrop, scene state, displayed results and audio receipts. Karna PRST28 casts
BOOST1 with2 MP; PRST30 casts BOOST2 with5 MP. Controlled BOOST2 has Kazin already boosted:
Bowie succeeds, Kazin is ineffective, then Peter/Gerhalt/Rohde/Elric/Karna/Janet succeed in
order. The ineffective target retains its prior stats. Karna's source pose is1.
Power Ring and Wish Staff use Sarah VICR Item25 on Peter with zero MP under Silence;
Protect Ring uses Item25 with ordered Sarah/Kazin/Peter/Rohde results. Each equipment
consumer makes one authoritative wear draw and damages the selected held copy exactly once.
Controlled PLDN Chester uses bare Valkyrie Item13 on Peter under Silence with zero MP. Sarah's existing Holy Staff/Life Ring end-turn
recovery remains distinct from BUFF. Every completed accepted sample reaches the next
ordinary action menu; early Confirm cannot release the active cast token. Each emits one77
and fourteen97 requests, then restores the black backdrop.

Protect Ring Original, complete HD with Reduced Flash/held Shift, and HD with one body frame
removed plus hit/coin suppression have identical gameplay events, HP/MP, stats, inventory,
EXP, status and both RNG seeds. Actual mounted Sarah textures confirm whole-body fallback;
no imaginary BUFF raster group is required. Existing rule, expiry and cold-save evidence is
reused. Natural learning/acquisition/reach, exact hardware timing and human aesthetic acceptance
remain **Unknown**. PSH uses the accepted sole-variant audio fallback (prepared Timer CB in
battle Timer C6), not an exact new timing capture.

### BUFF reproduction and retained failures

Use `derived/issue2282/buff-v1` with independent #2252 opening, #2255 Witch/catalog and
#2266 world catalog under `SF2_SHARED_INPUT_ROOT`. `derived/issue2282/review-v1` contains
immutable controlled records, Boot/Run launchers and receipts; no producer scratch is required.
Build the Debug adapter with the locked SDK environment, then from an isolated checkout:

```powershell
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2282/review-v1'
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case boost2 `
  -Seed boost2-recast.json -Sample buff-review -SaveRoot $disposableSaveRoot
# Other cases: boost1, power, protect, wish (same-named JSON).
# Valkyrie: -Case valkyrie -Seed valkyrie-ready.json.
# Settings representatives: -Case protect -Graphics hd -Reduced -Fast
# or -Case protect -Graphics hd -Fallback -Skip; use a fresh Sample each time.
```

Records derive from #2280's legal Battle26 support seed. Karna swaps position/first queue slot
with Sarah; rank2 uses her source PRST30 learned book. Recast uses battle v5 with explicit
class terms and Kazin status0x3000. Equipment records retain Sarah's VICR class and replace
one held slot, with zero MP and Silence. Valkyrie replaces deployed Sarah with already joined
Chester, PLDN1, bare item80 and the corresponding unarmed ATT5; flags/queue transfer with him. These are controlled prerequisites, not natural
campaign histories. The observer's inherited `SF2_DEBUFF_*` environment names are local
launcher inputs, not a production interface. Fallback removes only Sarah's
`hdRasters["equipment/ally25-palette1/frame1"]` from one disposable copy.

Resource work is linear in the prepared audio and one package closure. The small audio pilot
validates5865 stereo samples; publication plus atomic staging each stay below500MiB. The
single fallback copy, build/import/cache and receipts fit the initial2GiB scratch/RAM budget.
Native cases run serially. No new capture, remote API cost, producer, planner registration or
engine test is introduced. Adapter build, direct probe, document checks and clean-head selected
gates own acceptance; the Draft PR records exact head and actual CI.

**Completed failures retained:** two first runs paused after missing97 audio; the next found
missing97 in the audio slot dispatch. These were real failed consumers, corrected by the full
package and YM5 binding. Initial observer assertions used ActionMenu instead of ActionChoice,
off-by-one ring IDs, and compared HP across existing end-turn recovery; their receipts remain
failed. Initial Valkyrie records retained ATT8 after removing its old equipped spear, so
Continue did not admit them; correcting the controlled ATT to5 admits the same consumer.
One failed-record run also ended in the existing Godot finalizer access violation0xC0000005;
its cause remains Unknown and belongs with #1716. Final accepted observations have no session/presenter/script/audio error. ObjectDB
exit warnings remain with #1716 and are not a lifecycle pass. No failed record is relabeled.


### BUFF optional-group compatibility

BUFF's original presentation is optional: the scene must have both77 and97 selectable
through the existing exact-timer/default/sole-finite rules in its upcoming battle-music
context (allied2 or enemy5). `SessionAudio.CanSelectSceneEffects` performs that read-only
selection without caching or decoding media. Missing or ambiguous selections keep the prior
support Magic message path or equipment Item presentation. Thus equipment-v2 remains usable;
buff-v1 enables the original group. This availability check does not catch content/media
admission errors: a declared corrupt WAV/digest remains an error, never an optional fallback.

**Confirmed:** `compat-old-boost1` runs ordinary learned BOOST1 against unchanged equipment-v2:
no BUFF node activity or77/97 requests, prior message presentation,2 MP once, four ordered
successful recipients and next ActionChoice. `compat-new-boost1` against buff-v1 retains
mounted rasters/flash, one77/fourteen97 and return. Actor HP/MP/status/stats/items/EXP, both RNG
seeds and ordered results match across the two observations. Both exit0; the old-package
sample retains4 ObjectDB exit warnings. The earlier missing-audio/slot failures remain failed.
No wider matrix or retired suite was repeated for this correction.

`derived/issue2282/review-v2` adds the two receipts and updated launchers. Use
`-Case boost1 -Legacy` for equipment-v2 and omit `-Legacy` for buff-v1, with a fresh Sample
and disposable SaveRoot as above. The complete buff-v1 media package is unchanged.


## Equipment recovery playback #2291

**Standard; bounded presentation scope.** Holy Lance81/HEALIN1, White Ring23/AURA2
and Rune Axe43/DETOX2 reuse the existing recovery scene and prepared media. Domain
remains the sole HP/status, wear and reward authority. Application owns the ordered
recipients, logical fairy work and completion tokens; Godot projects Item gestures,
actual sprites and messages. `HealingSceneCursor.cs` and `BattleSceneView.Healing.cs`
extract the affected responsibilities without creating another runtime or clock.
No save format, class rule, asset producer, schema or audio algorithm changes.

**Confirmed source structure**, SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`;
paths below are relative to `disasm/`:

- `data/stats/items/itemdefs.asm` binds Holy Lance to HEALIN1, White Ring to AURA2 and
  Rune Axe to DETOX2. `data/stats/spells/spelldefs.asm` selects4,36,38 respectively.
- `code/gameflow/battle/battleactions/getspellanimation.asm:battlesceneScript_GetSpellanimation`
  checks equipment type in the USE_ITEM branch, reads the item's Use spell and then its
  animation. The separate CAST_SPELL silence branch does not suppress this item effect.
  Non-equipment retains NONE even when its calculation shares HEALIN/AURA/DETOX.
- `createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` keeps
  `BATTLEANIMATION_USE_ITEM`. `animateaction.asm` forwards the effect operand to
  `battlescenes/battlesceneengine_0.asm:bsc01_animateAllyAction`; `sub_184B0` returns
  for Item type2 without replacing that operand with a cast/header default.
- `battleactions/castspell.asm:spellEffect_Heal` and its cure receiver supply the
  authoritative reaction/result, not the view. `battleactions/battleactionsengine_1.asm`
  orders target switches and `battlesceneScript_MakeActorIdle`;
  `battleactionsengine_2.asm:battlesceneScript_End` emits `endAnimation` before switching
  back and reward. The existing fairy stop/drain and DETOX shrink/trail consume that end.

`BattleSceneState.RecoveryEffect` selects the visual definition from Magic or equipment
Item. It does not convert an Item action into Magic: no SpellCost phase, MP debit or
silence refusal is introduced. The fairy cursor uses the actual `item` sequence and
message275, keeps one effect across ordered AURA targets, then drains once. Rune Axe
uses the existing DETOX trigger and SpellStop delivery. Both poison and stun notices
now use the item semantic message receiver; formerly its second notice was empty and
ordinary input stalled at CureStunMessage. Ordinary consumables still have no recovery
overlay. Existing authored AURA without private content retains its authored presentation.

### Controlled native acceptance

**Confirmed bounded remake observations.**
`remake/game/probes/engine_equipment_recovery_observation.gd` drives Continue, ordinary
Item/Magic selection, target confirmation and the next ActionChoice. It reads actual
Sprite2D textures, scene phases, HP/status, messages and audio receipts. No screenshots,
synthetic presentation completions or emulator acquisition are used.

| Case / retained successful receipt | Actual result |
| --- | --- |
| Holy Lance self / `holy-b` | PLDN Chester, Item13, selector4, mounted light fairy; HP1→11, MP0 under Silence, one selected-slot wear81→337. |
| Holy Lance full HP / `holy-full-b` | Same Item/fairy at full HP with zero recovery and wear once. HD actor is mounted; Battle26 has no fairy HD entries, so the entire fairy group remains Original. |
| White Ring area / `aura-d` | VICR Sarah, Item25, selector36, one live fairy group across allies0,1,4,7,9,11,15 in source order. Sarah10→25; already-full recipients retain HP. MP0/Silence, one wear23→279, one close/return. Reduced Flash and held acceleration are enabled. Her separate Holy Staff/Life Ring end-turn recovery remains outside this action. |
| Rune Axe cure / `rune-c` | GLDT Jaha, Item18, selector38, actual complete HD mote/trails. Kazin's poison+stun3→0 with stable HP, both notices, zero MP under Silence and one wear43→299. |
| Rune Axe clean / `rune-clean-a` | Same original group and Item gesture; ineffective result, no status write and one wear. |
| Healing Seed / `seed-a` | Item25, selector0, HP10→30, one consumed slot; no fairy or DETOX overlay. |
| Learned regression / `heal-a`, `detox-a` | VICR Sarah HEAL1 pays20→17, heals10→28; DETOX2 pays20→14, cures Kazin3→0 with both notices. Existing Magic delivery remains live. |
| Optional media / `rune-fallback`, `rune-missing` | Removing one DETOX HD declaration selects the whole Original group. Removing all eleven optional Original/HD declarations retains Item pose/notices with no DETOX overlay and still reaches the next action menu. Declared damaged media is not caught or hidden. |

Every successful receipt ends in usable ordinary ActionChoice. A pending early cast token
is checked before natural completion where the source gesture leaves a sufficient window.
These observations use the accepted modern logical/presentation clocks and existing audio
variant selection. Natural acquisition/reach, exact hardware cadence and human visual/audio
acceptance remain **Unknown**. This is not a whole-game equipment or timing acceptance.

### Reproduce and retained failures

Reuse the immutable complete `derived/issue2282/buff-v1` (175 files,368493196 bytes),
independent #2252 opening, #2255 Witch/entry catalog and #2266 world catalog. No media
successor was needed. `derived/issue2291/review-v1` holds the controlled records, Boot/Run,
optional-group preparation and successful/failed native receipts. Boot stages writable
work only beneath the caller's checkout and requires a fresh external disposable save
root/namespace. The sole local fallback copy is below500MiB; reports are about25MiB.
Work is linear in the existing package and serial. No large acquisition was authorized;
the initial2GiB scratch/RAM budget was not expanded (peak RAM was not measured).

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2291/review-v1'
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case holy `
    -Sample holy-review -SaveRoot $freshExternalSaveRoot
# Other distinct families: -Case aura -Reduced -Fast; -Case rune -Graphics hd.
# Negative/regression cases: holy-full, rune-clean, herb (Healing Seed), heal, detox.
# Rune optional groups: -Case rune-clean -Variant incomplete -Graphics hd; -Variant missing.
```

Build Debug with the locked SDK workflow before native use. Release `verify adapter` alone
does not refresh Debug. Each process is bounded to60 seconds; inspect `native.json`,
`console.log` and `exit.txt`. The private review records identify the exact inputs; they
are never public fixtures. Holy records derive from #2282 `valkyrie-ready.json`, changing
slot2 to bare81 in both loadout copies, MP0/Silence and HP1 or max. Other records derive
from #2273 `seeds/aura.json`; slot2 is23/43/1 or learned Magic retains its book with MP20
and silence cleared. Rune uses Jaha's legal GLDT15 class, bare pack, base attack, Sarah's
position, queue head and active flag35 instead of33; target Kazin has status3 or0.
AURA/Rune inputs include current unjoined allies20–29 off-board from #2274's
`receipts/old-battle-zh/cold-slot.json` retained records, using current recipe11 and an
empty retained partition. Initial stats, books and equipped bonuses come from those
records and the pinned static item definitions, not invented fields. Both full-roster
inputs pass direct `SaveRecordRestore.Restore` with45 deployments and45 source slots.

Completed failures remain in the review: first adapter compilation caught an accidental
string-return insertion in the command method (corrected before Debug playback);
`holy-a` checked a token too late at the natural action boundary; `aura-c` counted later
end-turn equipment HP and non-action notices; `holy-full-a` incorrectly expected fairy HD
in Battle26. `rune-b` exposed the real missing second cure message, now fixed. `rune-a`
and `aura-b` retained invalid controlled-input attempts (old recipe10 with a current
roster, and the early Rune active-member setup); the direct consumer diagnosed the
recipe/retained-roster mismatch before the corrected recipe11 native attempt. `aura-a` is the separate
supported historical partition defect below. Successful exits are0; some retain2–6
ObjectDB shutdown warnings under #1716, which this slice does not repair.

### Supported historical AURA partition defect

**Confirmed historical compatibility defect; corrected by accepted #2295.** The unmodified
#2273 `review-v1/seeds/aura.json` is an accepted recipe10 Battle26 record with20 allies and15
enemies and no retained allies. On the pre-fix base, complete buff-v1 restored ordinary VICR
Sarah control, but disabled White Ring's item choice with `aura-source-slots`, field
`initialization.sourceCombatants`. The retained `aura-a` receipt preserves this refusal using
a disposable copy; the HP/status/slot values relevant to AURA match that source record.

`BattleRestore.Build` restored the saved35-actor deployment partition using `WithDeployments`,
whose copy formerly retained the package's45-entry Initialization source mapping.
`HealingTargetRules` required equal counts. The other10 allies were correctly retained by the
existing recipe upgrade, but their source-slot entries remained in the battle definition.
This was a supported old-record/new-package combination, not an invalid old save. The retained
before/selection projections keep all actor data and both RNG seeds unchanged; the only event
is entry to ActionChoice, with no Item commit, wear, HP/status write or result scene.
Complete-roster inputs supplied #2291's presentation acceptance without a compatibility edit.

The shared partition correction now preserves only mappings for actors in the restored battle,
without renumbering source slots or weakening the consumer validator. Current behavior,
old/current-record AURA acceptance, resave/cold Continue evidence and reproduction are owned by
[battle save and Continue](../application/battle-save-and-continue.md#historical-battle-source-slot-partition).
The original failed observation remains historical evidence. Reproduce that refusal only on the
named pre-fix base with `-Case aura -Seed old-aura.json`; it is no longer a current defect or
queued follow-up. No compatibility implementation is added by this presentation slice.

## Flame family playback #2293

**Standard scope.** Close presentation for admitted BLAZE1–4, KATON1–3 and equipment
BLAZE2/3 without new gameplay, acquisition, encounters or save semantics. Selection uses the
actual action animation, not the spell name or gameplay rank. Consumable Shining Ball remains
NONE. Application retains one action/result continuation; Item keeps its gesture and wear.

The owned implementation paths are `remake/game/src/Battles/{BattleSceneView.cs,
BattleSceneView.Rasters.cs,AttackSpellView.cs,BlazeMeteorEffect.cs}` (replacing
`BlazeTwoEffect.cs`), `remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs`,
`remake/src/Sf2.Remake.Content/Scenarios/{PrivateAttackSpellBindings.cs,BattleSceneContentReader.cs}`,
`src/sf2tool/{remake_attack_spell_mount.py,remake_private_package.py,remake_retail_assets.py,
remake_hd_battle_rasters.py}`, `remake/tests/Sf2.Remake.Engine.Tests/{AttackItemSessionTests.cs,
EquipmentItemTests.cs}`, and `remake/game/probes/{BattleRasterObservation.cs,
engine_flame_family_observation.gd}`. Documentation changes are confined to this family in
this owner, battle-scenes, equipment-use-repair-design, asset-inventory, presentation-assets
and capability-status. No schema, planner, language aggregate or save authority change.

**Plan and budget.** Reuse prepared #2001 tileset2 and complete #2282 buff-v1. Generate a
single representative scene first; extend all admitted scene bindings only after checking its
size and retained resources. The input closure is about369MB; allow one <500MiB immutable
successor plus atomic staging, <=2GiB scratch and peak RAM. Processing is linear in package
bytes plus small raster groups; serial native samples use ordinary input, token/event boundaries
and distinct mounted frame descriptors, without screenshots or bulk per-frame records. No new
acquisition, model API or emulator run. Replan before expansion if these bounds are exceeded.
Production-reader checks cover every mounted scene; native witnesses cover new variants, alias,
Item and enemy side plus representative HD/fallback/reduced-flash/held-rate paths. Engine and
adapter gates and the committed dependency planner select acceptance. Independent reproduction
must use published inputs after producer scratch is moved aside.

### Source composition and delivery

**Confirmed source.** The USA identity and pinned commit at the top of this owner govern
`getspellanimation.asm`, `createbattlesceneanimation.asm`, `animation/blaze.asm` and
`animation/update/blaze.asm`. The action selector reads equipment Use from its spell row,
while non-equipment consumables retain NONE. BLAZE selectors are1/33/65/97; KATON selectors
are33/65/97. BLAZE remains Fire and KATON remains Neutral in the bound gameplay definitions.
Equipment consumers retain Item gesture, zero MP and the existing selected-copy wear authority.

The same prepared tileset2 supplies all variations. `table_1BE2A` selects v3/v4 stacked
32×64 flames at tiles85+101 and117+133 (zero based). The +32-byte entries following
`table_1BE42` select24×24 meteors172/181 and32×32 impacts190/206. `graphic_BlazeDragon`
uses head17 (32×32), five body sprites8 (24×24), and tail4/0 (16×16).
`table_1A594`/`table_1A5BC` place the large flame at(64,80)/(168,96), choose14 meteors
for v3 and4 for v4, with meteor X-window starts80/48 and opposite travel/flip.
`table_1BE82` supplies body history gaps3/3/3/3/3/2/2 ticks and offsets4/8.
The dragon uses the source speed operands128,256,768 and16, angle steps8/256,
six-tick straight intervals, side bounds X32–160 or128–256, Y32–112 and wall-heading table.

**Modern presentation.** The adapter retains v2's deterministic meteor scheduling and uses
1.67s for v3 and1.8s for v4. Each selected meteor slot flies and impacts once. Dragon motion
uses floating-point trigonometry and a local deterministic visual RNG; it cannot consume
session RNG. This is source composition/steering on the modern clock, not a reproduction of
original fixed-point rounding, random streams, looping until scene-script stop, palette flashes,
per-frame flicker or hardware timing. Reduced Flash holds the first flame/meteor/impact raster
and uses smooth fading. Cast cue77 remains the existing audio boundary; original per-meteor
hover/impact cues and exact audio timing are **Unknown**. None of these choices changes ordered
reactions, death, rewards, payment, wear or continuation tokens.

`AttackAnimationSelector` projects the committed caller's selector; `PrivateAttackSpellBindings`
validates all four BLAZE rows. `BlazeMeteorEffect` extends the former v2 responsibility rather
than creating another scene or result owner. Missing Original groups retain existing fallback;
incomplete HD groups select the complete Original effect, including all ten resources for v4.

### Reproduction and acceptance

Select the complete immutable `derived/issue2293/flame-v1` plus independent #2252 opening,
#2255 Witch/entry catalog and #2266 world catalog. The successor retains every predecessor
file and existing Original/HD raster, actor, weapon, map, audio and prior spell family.
The publication and resource comparison are owned by [presentation assets](../content/presentation-assets.md#flame-successor-2293).
`derived/issue2293/review-v2` contains the controlled records, launchers, production reader and
all completed receipts. Its seeds derive from #2274's recipe11 full-roster record, not the
historical recipe10 AURA partition. They deliberately select legal classes/books, packs,
HP, positions, active flags, queue and RNG. They do not establish natural reach.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2293/review-v2'
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case blaze3 `
  -Sample blaze3-review -SaveRoot $freshExternalSaveRoot
# blaze4 -Graphics hd; katon1; ring -Graphics hd; axe; enemy; none; silenced.
# katon3 -Graphics hd -Reduced -Fast; blaze4 -Graphics hd -Variant incomplete.
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
```

**Confirmed in the remake.** Ordinary Magic, Item or Stay-to-enemy-AI input preserves the
actual WIZ/NINJ/GLDT/Wizard actors, weapons and scenery. v3 covers14 large meteor slots;
v4 covers4 and the eight-part dragon; KATON1 and Black Ring cover10 small slots.
Early Confirm retains the animation token. Ordered two-target v3/alias/Item reactions include
one lethal target and reward completion; v4 remains single target. Magic pays once; Item
ignores silence, spends no MP and draws wear once; Shining Ball consumes one copy with no
flame. Silenced Magic pays once with no flame or damage. Each completed sample opens the
next ordinary action menu. HD, partial-group fallback, Reduced Flash and actual1.5× held
acceleration have separate representative receipts, not a full permutation matrix.
Production admission covers26 scenes,182 BLAZE/KATON bindings and156 equipment flame bindings.
This is distinct from native playback in every scene. The public engine gate passed3378 tests
with50 private-input skips; the skipped checks are not claimed as passed.

**Retained failures.** The first adapter build found the old effect type name in
`BattleRasterObservation`; the corrected adapter build passed. Private consumer setup first
used an incorrect relative project reference, then an incorrect restore namespace; both were
corrected directly. Initial controlled records were rejected for learned/progress spell,
equipment-class, attack and current-membership mismatches; packed IDs, expanded rank lists,
loadout and active flags were corrected before successful restoration. An early boot observer
then dereferenced an absent slot view and emitted a misleading empty-success JSON; its native
exit was1, so it is retained as failure. Subsequent rejected-entry receipts also remain.
The initial Black Ring assertion used an unpromoted animation index; current WIZ Item is21.
The initial enemy seed chose legal physical attack (category1), so it proved no flame;
the explicit controlled main seed2000<<16 selects the spell branch. The first held-rate
sample pressed before the animation and saw no acceleration; the corrected sample presses
at the animation and observes1.5×. Existing ObjectDB exit warnings remain under #1716,
separate from successful gameplay return; no cleanup or exit-framework change is included.

**Unknown.** Natural original/player reach, native KATON2 and every equipment/encounter
combination, exact original motion/audio/timing, and human visual acceptance. The owning
Issue stops at a frozen Draft PR for independent integration.

The independent publication check first attempted to parse the closure report as a save;
that failed with `save-shape` after scene admission. Retained `review-v1` records this
incomplete verification entry. `review-v2` selects only the explicitly named case records;
all nine restore successfully. With producer scratch moved aside, its production reader
and a fresh HD BLAZE4 ordinary-input cast both pass against the published flame package.
An initial Ruff check reported only local import ordering; the corrected check passed.

The committed planner uses `--scope engine` for the reviewed remake-only producer changes.
It selects engine-unit, adapter-build, private-admission-build, live-inspection-build and the
three direct research-public checks (Ruff, design-contracts and research-index), with no
unclassified paths. The engine behavior result above remains valid across the settings-only
accepted-main advance; the rebased adapter and both selected tool builds pass, as do the
three public checks. No legacy aggregate, H3 or full research suite is selected or claimed.

## FREEZE family playback #2298

**Standard scope.** Extend admitted FREEZE1–4 learned/enemy casts and Freeze Staff
FREEZE3 through the existing action selector, AttackSpellView and continuation. Blizzard
consumable Use retains NONE. No gameplay, campaign, save or lifecycle expansion.

Owned paths: `remake/game/src/Battles/{AttackSpellView.cs,BattleSceneView.cs,
BattleSceneView.Rasters.cs,FreezeEffect.cs}`;
`remake/src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs`;
`src/sf2tool/{remake_attack_spell_mount.py,remake_private_package.py,
remake_retail_assets.py,remake_hd_battle_rasters.py}`;
`remake/game/probes/engine_freeze_family_observation.gd`;
`remake/game/src/Audio/SessionAudio.cs` for command120's existing voice-slot mapping; this owner and the bounded
FREEZE entries in battle-scenes, equipment-use-repair-design, presentation-assets and
asset-inventory. Existing engine assertions already cover Ice equipment wear/payment;
only actual engine behavior changes warrant additional unit assertions. Capability-status
contains only the released FREEZE capability entry. No second scene, registry or result authority.

**Plan and budget.** Start with prepared #2001 tileset11 and complete #2293 flame-v1
(175 files,369,705,232 bytes), preserving old resources. Pilot Battle26 Original/HD
composition before one complete immutable successor below500MiB. Allow staging plus output
within2GiB scratch/peak memory. Linear serial processing; no new ROM acquisition, API/model
processing or emulator capture. Production readers admit every scene; bounded native input
covers distinct variants, Item, enemy side, NONE and representative HD/group fallback,
Reduced Flash and1.5x. Preserve failures; stop/replan if measured budgets exceed limits.
Published-input reproduction must work after producer scratch is moved aside. Exact hardware
motion/audio timing, natural reach and human aesthetics are separate Unknown boundaries.

The representative Battle26 pilot adds14 Original and14 HD rasters, preserves every old
field/resource and measures6,460,629 bytes/0.21s, below the existing8MiB scene reader limit.

### FREEZE source composition

**Confirmed source.** Pinned `c834c652b6862bc5679fd7f69a38a7093206efc6`
[`getspellanimation.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm)
selects FREEZE values2/34/66/98 for learned ranks1–4 and66 for Freeze Staff98 Use.
The adapter's selector excludes the separately projected enemy-side bit128. Consumable
Blizzard17 keeps NONE, not its BREZAD table's FREEZE-v3 value. Item keeps its gesture,
zero MP, silence immunity and one selected-copy wear/consumption authority.

[`animation/freeze.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/freeze.asm)
(`0x1A654..0x1A73A`, `table_1A6DA`) loads tileset11 with cast77 and flash color0xE22.
The four initial radii are0x1000/1600/1C00/2200 and growth increments0xA0/C0/E0/100.
Unmirrored entry is VDP X368 plus0..7, Y140 plus0..39, heading158, turn-1 then-6,
orbit X200. Mirrored entry uses X128, Y156, heading226, turn+1 then+6, orbit X312.
Subtract128 for local canvas coordinates. Both sides keep the same unflipped crystal art.
[`graphics_1.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/graphics/graphics_1.asm)
`sub_179C`/`sub_1812` and
[`data/tech/spellanimations.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/tech/spellanimations.asm)
resolve the `(cos,-sin)` axes and quarter-height orbit.

[`update/freeze.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/freeze.asm)
(`0x1BE9E..0x1C248`) uses `table_1C18A` initial tiles0/1/2/6 and
`table_1C19A` transitions at ages8/13/18/23/28/34/40. Zero-based tiles0/1/2/6/15/24
have sizes8/8/16/24/24/32 square, with the second brightness bank at+40. v4 adds
8x24 glints at tiles80/83 in a48-square window at local(56,72)/(160,88).
Particles chain at age4, enter the expanding ascending orbit at28 and retire at46.
The source loops until the scene-script stop, flickers sprites, changes palette entries,
and repeats sound120 after randomized10..23-tick delays.
[`sounddriver.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/sound/sounddriver.asm)
`sfx_38` has one active type1 pointer, slot8 (`byte_1EBE`), hence PSG tone3.

**Modern presentation.** FreezeEffect plays16 deterministically spaced crystals over96/60s,
with source growth tables, side origins, turn/radius parameters and v4 glints. Floating-point
trigonometry and fixed local spreads replace original integer rounding and random choices.
It does not consume session RNG. Smooth fades replace alternating sprite hiding and global
palette flashes. Reduced Flash keeps crystal growth but uses one brightness bank and reduced
glint intensity. The prepared120 clip uses a0.3s replacement cadence after cast77; these are
source media and voice ownership, not hardware-exact audio timing or palette behavior.
The existing scene owns actor/weapon/background composition and waits for pose plus effect
completion, then ordered damage, deaths, rewards and next control. No second lifecycle exists.

### FREEZE reproduction and limits

Select `derived/issue2298/freeze-v1` for world/main scene/six opening inputs. Keep #2252
opening, #2255 Witch/entry catalog and #2266 world catalog independent. The published
`derived/issue2298/review-v1` contains controlled records, the production reader, launchers,
closure comparison and receipts. Seeds reuse #2293 recipe11; WIZ Tyrin has an explicit
source-compatible four-family book and controlled level40. Freeze Staff uses silenced WIZ
Kazin at0 MP. The enemy record uses production Battle19 deployments and the original
Dark Madam FREEZE2 book, with controlled active flag, positions, orders, queue and seeds.
These are legal restored inputs, not proof of natural training, acquisition or arrival.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2298/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case freeze4 `
  -Sample freeze4-review -SaveRoot $freshExternalSaveRoot -Graphics hd
# Cases: freeze1, freeze2, freeze3, freeze4, staff, none, enemy.
# freeze4 -Graphics hd -Reduced -Fast; freeze4 -Graphics hd -Variant incomplete.
```

Build the locked Debug adapter first, using configured absolute SDK/editor and protected
CLI home. Each case uses its own disposable namespace and worktree-local runtime profile.
Require receipt `passed:true`, exit0 and no script/adapter errors together. Production restore
precedes native launch. No screenshots, frame dumps, grinding or chapter replay are involved.

**Confirmed in the remake.** The production reader admits all26 scenes,104 FREEZE spell
bindings and26 Freeze Staff bindings, plus all seven controlled restores. Native witnesses
cover the four distinct variants, enemy-side2 and Item3, with actual mounted textures,
16 crystal slots, v4 glint, retained scene actors/weapons/scenery and early-Confirm token
protection. Two-target cases include lethal and surviving recipients. Single payment/wear,
ordered results and usable next action menu remain intact. Blizzard consumes once and draws
no FREEZE despite the table alias. HD/partial-group fallback/Reduced Flash/1.5x are mechanism
representatives, not every scene/rank/character permutation. Producer scratch is unavailable
during final published-input observations and reader reproduction.

**Retained failures.** Initial documentation append used the host's non-UTF8 default and
failed before writing; corrected commands use explicit UTF8. The first Ruff check found one
long line. Consumer setup initially lacked its copied source directory. Tyrin restore rejected
missing joined/active flags. Enemy preparation rejected missing custody, initialization,
active formation and mismatched deployment positions; all were corrected before native use.
Two enemy pilots stayed in source standby until the controlled activation bit was set; they
proved no cast. The first package comparison treated the audio list as a dictionary and failed;
the corrected comparison checks retained rows. Initial native playback reached Reaction then
stopped at `sfx-slots-unavailable`: command120 lacked a sound-slot mapping. An interim hypothesis
and one-cue attempt incorrectly blamed replay count; the source-backed PSG tone3 mapping fixes
the actual defect. Final cadence uses the existing replacement policy. The first trajectory
used swapped trig axes; final source-table review corrected it before the published runs.
These setup/native receipts remain failures, never relabeled as interrupted or successful.
Some completed processes retain ObjectDB exit warnings under #1716.
Initial memory receipts captured the console/conhost launcher; only the final named-game
child measurement establishes the approximately1.30GB native peak. The reader peaked at0.96GB.

**Unknown.** Natural original/player reach, all encounter/character/equipment permutations,
exact fixed-point motion, palette/priority flicker, random streams, hardware audio cadence
and human aesthetics. This slice makes no Battle27+, lifecycle or public-media distribution claim.

The public engine gate passes3378 tests with50 optional private-input skips; the adapter
Release build passes with zero warnings/errors. The dedicated published reader and native
receipts, rather than those skips, establish the private observations above.

## Lightning family playback #2301

**Standard scope.** Bind admitted BOLT1–4, neutral RAIJIN1–3
and equipment BOLT1/2, including enemy Taros Sword, to original prepared art through the
existing scene and result continuation. Gameplay, saves, acquisition, Battle27+ and Holy
Thunder admission are unchanged. Original composition and modern deterministic delivery
have separate acceptance; natural reach and exact hardware timing remain **Unknown**.

Owned implementation paths: `remake/game/src/Battles/{AttackSpellView.cs,BattleSceneView.cs,
BattleSceneView.Rasters.cs,BoltEffect.cs}`, `remake/game/src/Audio/SessionAudio.cs`,
`remake/src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs`,
`src/sf2tool/{remake_attack_spell_mount.py,remake_private_package.py,
remake_retail_assets.py,remake_hd_battle_rasters.py}`, and
`remake/game/probes/engine_bolt_family_observation.gd`. Documentation ownership is this
section plus narrow battle-scenes, equipment-use-repair-design, presentation-assets and
asset-inventory entries, plus the released lightning capability entry. Input/host
documentation remains with its separate owner. No planner, schema or aggregate changes are planned.

Use complete #2298 freeze-v1 (176 files /370,341,646 bytes) and prepared #2001 retail-v1
read-only. Pilot one scene before one complete immutable successor below500MiB. Preserve
all previous resources and independent #2252 opening, #2255 Witch and #2266 catalog.
Serial O(package bytes + fixed sprite operands) processing; <=2GiB scratch/peak RAM.
No acquisition, capture, API/model calls or new asset platform. Measure the pilot before
expansion and stop/replan if these limits fail. Production readers must cover all26 mounts,
then legal controlled restores precede bounded ordinary-input observations of distinct
variants, alias, Item/enemy, HD/group fallback, Reduced Flash and acceleration. Reproduce
from publication after producer scratch becomes unavailable. Adapter build and selected
engine gate supplement these direct observers; no tests of verification tools are added.

### Lightning source and modern delivery

**Confirmed source.** The pinned USA `c834c652b6862bc5679fd7f69a38a7093206efc6`
spell table selects7/39/71/103 for BOLT1–4 and39/71/103 for RAIJIN1–3.
`getspellanimation.asm` selects the equipment spell for Halberd83 (BOLT1), Evil Ring25
and Taros Sword110 (BOLT2). Non-equipment Use keeps NONE; Holy Thunder18/THUNDR's
v3 table row does not override that caller or admit the item. The source enemy bit128
is represented separately by the adapter's target-side projection. RAIJIN's neutral
resistance and damage remain separate from Lightning.

[`animation/bolt.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/bolt.asm)
(`0x1AA90..0x1AB9E`) loads tileset4, cast77 and flash0xECA.
`table_1AB4E` selects1/2/3/4 concurrent strikes and0/1/2/4 orbs.
[`update/bolt.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/bolt.asm)
(`0x1CE50..0x1CFFC`) selects either eleven-frame A/B operand table, advances the first
six frames every two ticks, pauses and advances the remaining five. Each frame has five
sprite records. Strike X is16..111 on the enemy-target side or144..239 on the ally-target
side; the latter adds8 to Y. Initial delays and subsequent positions/waits are randomized,
and source playback repeats until scene-script stop. `table_1AB5E` orbs use local
(72,64),(24,88),(120,72),(176,56); mirrored side rotates this order by two. Their eight
phases select tiles148/164, horizontal and vertical flips.

[`boltanimdata.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/graphics/battles/spells/animations/boltanimdata.asm)
(`0x1AA8CA..0x1AAC3A`) and `battlesceneengine_2.asm:ConstructComplexGraphic`
provide the X/Y, tile and size/property operands. The producer consumes the two prepared
440-byte tables, resolves one-based tile1 to VDP0x520, preserves H/V flips and earlier-sprite
overlap priority, and composes each frame on a64x96 canvas with local origin(-24,48).
The30-resource group comprises22 strike composites and eight32-square orb orientations.
All30 select Original or complete4x HD atomically, with identical logical draw extents.

**Modern presentation.** BoltEffect uses three finite36-tick cycles, four-tick stream
stagger and deterministic local positions/table choices on a120/60-second clock. It does
not consume gameplay RNG. Orbs advance every three ticks and fade at the endpoints;
Reduced Flash freezes their phase and lowers effect opacity. Global source palette flashes
are omitted. Source77/119 clips use the existing audio replacement policy;119 has six
modern cue requests rather than every source concurrent/random strike event.
`SoundSlots(119)` is YM5: `sounddriver.asm:sfx_37` is type2 with only its second pointer
(`byte_1D43`) active; `Load_SFX_Type_2` maps those pointers to YM4/5/6.
These are original media/composition with modern cadence, not hardware-exact timing/audio.

The existing scene retains background, actors and equipped weapon, waits for pose plus
effect completion, then delivers ordered damage/death/reward and next control. Equipment
retains its body-specific Item sequence (WIZ21, PLDN13); Taros's existing item-origin
consumer has no ordinary AttackItem record and uses the source body45 sequence. It still
shows the sword-use message, pays zero MP and never draws allied wear. No new lifecycle,
rule, save format or input authority is introduced.

### Lightning reproduction and acceptance

Select complete `derived/issue2301/bolt-v1` world/main scene/six opening inputs together,
retaining independent #2252 opening, #2255 Witch/entry catalog and #2266 world catalog.
The registered `derived/issue2301/review-v2` retains controlled records, reader, launchers,
closure comparison and native receipts. WIZ Tyrin's accepted four-family book supplies
BOLT1–4; NINJ Slade uses source-compatible KATON/RAIJIN books with controlled rank3/level40.
Halberd uses a controlled PLDN Chester with legal held custody. Evil Ring retains silenced,
zero-MP WIZ custody. Taros uses initialized production Battle18 enemies and its original
sword/loadout, activation/order, with two controlled allies at range3/4. These restored
records establish legal consumers, not natural learning, acquisition or chapter traversal.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2301/review-v2'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case bolt4 `
  -Sample bolt4-review -SaveRoot $freshExternalSaveRoot -Graphics hd
# Cases: bolt1, bolt2, bolt3, bolt4, raijin1, raijin3, halberd, ring, taros.
# bolt4 -Graphics hd -Reduced -Fast; bolt4 -Graphics hd -Variant incomplete.
```

Build the locked Debug adapter first with the protected configured environment. Each run
uses its own disposable save namespace and worktree-local profile. Require passed:true,
exit0 and no script/adapter errors together. Read production admission before native input.
No screenshots, frame dumps, grinding or campaign replay are involved. Producer scratch
is unavailable for final published-input reproduction.

**Confirmed in the remake.** All26 scene mounts admit30 Original/HD resources,182
BOLT/RAIJIN spell bindings and78 equipment bindings, and all nine controlled records restore.
Native witnesses cover all four BOLT variants, RAIJIN1/3, Halberd1, Evil Ring2 and Taros
Sword2; they show actual mounted textures, source strike/orb counts and scene composition.
Area casts include lethal and surviving targets; one payment/wear, ordered application,
death cleanup, reward and usable next action remain intact. Taros damages two allies with
zero MP and no wear. HD, partial-group fallback, Reduced Flash and held1.5x use mechanism
representatives. Rank2 RAIJIN and every encounter/member permutation have reader coverage,
not new native witnesses.

**Retained failures.** Initial pilot omitted the documented `--with pillow` environment
and failed before output. The first mount had an incorrectly extended audio tuple and
failed before publication; the corrected tuple selects119. Initial Ruff found long
lines. Halberd restore rejected its missing active-membership flag before native launch.
An inherited observer assumed every Item body uses WIZ21; PLDN's source Item13 required
its own expectation. The first Taros setup allowed a physical action at range2, so it
proved no cast. The range3 run cast successfully but the observer's ordinary AttackItem
filter missed the existing special Taros consumer; selection now uses its actual spell
and enemy origin. Review-v1 receipt publication stopped on a host GBK decoding error; its incomplete
copy remains retained. Complete review-v2 uses explicit UTF8 and staging.
These completed failures remain failures. Some completed processes
retain ObjectDB exit warnings under #1716; that issue is not expanded here.

**Unknown.** Natural original/player reach, unobserved combinations, exact random/fixed
hardware cadence, palette/priority flicker and human aesthetic acceptance. Holy Thunder,
Battle27+ and broader lifecycle work remain outside this slice.

The committed verification selection is `uv run sf2 verify plan --scope engine --base
origin/main --head HEAD`: engine-unit, adapter-build, private-admission-build,
live-inspection-build and research-public (Ruff, design-contracts and research-index).
The source edits are remake-only asset/selector/presentation consumers; no research
execution semantics or planner is changed. The default plan is retained alongside this
explicit scope decision. Engine passes3383 with50 optional private-input skips; both tool
builds and adapter Release pass with zero warnings/errors. Production reader/native
receipts establish this slice's private acceptance independently of those skips.
Published-input rebased reproduction passes; maximum measured native working set is
1,330,892,800 bytes and owned scratch logical size is817,677,889 bytes at publication.
The Draft PR's actual CI checks and frozen head identify remote acceptance separately.


## Wind family playback #2306

**Standard scope.** Extend BLAST1 to admitted BLAST2–4 and existing enemy consumers,
using the same scene/result continuation. No gameplay, equipment admission, save-format,
Battle27+ or #1716 changes. Natural reach, hardware timing and human visual acceptance
remain **Unknown**. Implementation owns `BlastEffect.cs` (replacing `BlastOneEffect.cs`),
`AttackSpellView.cs`, `BattleSceneView.cs`, `BattleSceneView.Rasters.cs`,
`PrivateAttackSpellBindings.cs`, `remake_attack_spell_mount.py`,
`remake_private_package.py`, `remake_retail_assets.py`, `remake_hd_battle_rasters.py`,
`BattleRasterObservation.cs` constructor migration and `engine_blast_family_observation.gd`.
Shared documentation is this section and narrow battle-scenes, presentation-assets,
asset-inventory, equipment and capability entries. No schema/planner/aggregate changes.

Consume complete #2301 bolt-v1 (177 files /373,560,638 bytes) and prepared #2001 retail-v1
read-only. Preserve independent #2252 opening, #2255 Witch and #2266 catalog. Serial
O(package bytes + fixed sprite operands) publication makes one immutable complete successor
below500MiB; scratch and peak memory stay below2GiB. The representative Battle26 pilot
preserves all previous values and produces23 Original/HD resources in0.29 seconds with
76,963,840-byte peak memory. Replan if those budgets fail. All26 production scene readers,
legal controlled restores, four allied ranks and enemy-side input cover the mechanisms;
HD/partial fallback, Reduced Flash and held acceleration use representatives. Final
reproduction must consume published inputs after producer scratch is unavailable.

### Wind source and delivery

**Confirmed source.** USA SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`
`spelldefs.asm` selects5/37/69/101, MP2/5/8/8, power6/8/12/30 and radius0/1/1/0.
`getspellanimation.asm` selects learned unsilenced Magic and marks enemy origin separately.
Production readers admit104 BLAST definitions across26 encounters and no BLAST AttackItem.
Existing Sarah PRST and Dark Bishop books supply the observed consumers.

[`animation/blast.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/blast.asm)
(`0x1A928..0x1AA40`) selects tileset15, cast77 and eight fragment slots plus one slash.
Even variations set the larger-fragment bank. Variations3/4 add cyclone and dust slots.
[`update/blast.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/blast.asm)
(`0x1C7F6..0x1CC56`) retains six fragment directions and eight slash entries.
The existing15 raster keys remain byte-identical. Even fragments start at zero-based
tiles1/25, then5/29 and11/35; the last pair adds24x32 and32x16 rasters. Cyclone banks
43/75/107 each stack two4x4 sprites, independently column-major, into32x64 composites.
Dust uses139/143/147 as16x16. `VALUE2` is the property high nibble, not a flip.
Earlier sprite slots retain overlap priority over cyclone/dust.

The fragment/slash origin is(80,96) for enemy targets or(184,112) for ally targets.
Cyclone base is(64,80)/(168,96), moving0.75 pixels per update, first reversing at30 ticks
then every60. Three cyclone frames advance every4 ticks; dust cycles three visible frames
and one hidden phase, attached at(+8,+52) during updates. Source randomized particle,
slash and repeat-until-stop scheduling remain separate from remake playback.

**Modern presentation.** The accepted1.4-second deterministic fragment/slash schedule,
converging travel/fades and one wind114 recording remain unchanged. The cyclone shares
that finite clock with deterministic triangle motion and source frame banks. Reduced Flash
holds middle frames; held1.5x changes the existing clock. No gameplay RNG is drawn and no
source global palette flash is added. Original/4x HD selection is atomic for15 resources
at rank1,17 at rank2 and23 at ranks3/4; missing HD members select the whole required
Original group. Old rank1-only packages work. The existing scene owns actors, scenery,
equipped weapon, completion, MP and ordered results.

### Wind reproduction and acceptance

Select complete `derived/issue2306/blast-v1` world/main scene/six opening inputs together,
retaining independent #2252 opening, #2255 Witch/entry catalog and #2266 world catalog.
`derived/issue2306/review-v1` retains legal controlled records, reader, launchers and receipts.
Sarah uses her existing PRST40 book and inventory; controlled records change her position,
current membership and queue. Battle26 Dark Bishop retains its species, loadout, orders
and spells. Full-health enemies and MAIN upper word2000 produce the existing category2
Magic decision; no production AI policy changes. These states do not prove natural training,
recruitment, acquisition or chapter traversal.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2306/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case blast4 `
  -Sample blast4-review -SaveRoot $freshExternalSaveRoot -Graphics hd
# Cases: blast1, blast2, blast3, blast4, bishop.
# blast4 -Graphics hd -Reduced -Fast; blast4 -Graphics hd -Variant incomplete.
```

Build the locked Debug adapter first. Each run uses a fresh disposable external save
namespace and worktree-local profile. Require passed:true, exit0 and no script/adapter
errors. Production readers admit all26 scenes and five records before native input.
Observe actual textures, actors/scenery/weapon, source variation and target side, one MP
payment before ordered HP delivery, lethal cleanup/reward and usable next action.
Area/single-target behavior remains owned by unchanged rules. No screenshots, frame
captures, grinding, combination matrix or tests of verification tools are added.

**Retained failures.** Initial Ruff reported an import/line-length issue. A scratch
inspection expected decoded save fields at the envelope root and stopped with KeyError.
The first Sarah record lacked matching current-membership flags and restore rejected it.
The first Dark Bishop case healed a wounded Lesser Demon; full-health and high-defense
attempts selected physical category1 and proved no BLAST. Reading the existing MAIN
category rule supplied the controlled seed; the final record restores original defense.
These completed failures remain failures. Successful processes can retain6–8 ObjectDB
exit warnings under paused #1716.

**Unknown.** Natural original/player reach, unobserved combinations, hardware-random
cadence, palette/priority flicker and human aesthetic acceptance. No Battle27+, new caller,
equipment, recruitment, save format or broader lifecycle work is included.

**Confirmed remake acceptance.** All four Sarah ranks and Dark Bishop BLAST3 pass ordinary
input, actual texture/side/composition, ordered result and next-menu observations. HD,
partial-group fallback and ReducedFlash/held1.5x pass. After the final overlap-order change,
HD4 and enemy3 pass again; with producer scratch unavailable, the published reader restores
all five inputs and published rank2, HD4 ReducedFlash/fast and incomplete-HD4 pass.
Maximum measured native working set is1,348,579,328 bytes; producer scratch logical size
at review publication is400,884,507 bytes. These host measurements fit the2GiB allowance.

The selected committed `--scope engine` plan contains engine-unit, adapter-build,
private-admission-build, live-inspection-build and research-public; no unclassified paths.
The default plan additionally lists the legacy always-run public-core. Both plans are
retained; the engine-only scope follows the current verification policy because these
Python changes are remake asset consumers, not research execution semantics.
Engine passes3383 with50 optional private-input skips. Locked adapter Debug/Release and
both tool Release builds pass with zero warnings/errors. Ruff, design-contracts and
research-index direct checks pass. No rules changed and no verification-tool tests were
added. Native evidence is independent of optional unit skips. The Draft PR records actual
remote CI and the frozen head/tree/base for independent main-gate acceptance.

## DESOUL family playback #2307

**Standard scope.** Learned DESOUL 1/2 and Dark Sword (70) Use share the reaper effect and
the existing battle scene continuation through Battle26. The effect follows the typed
result. The existing action owns MP, silence, selected-copy wear, target order,
death, custody, rewards and the outcome or next turn.

### DESOUL source and delivery

**Confirmed source.** At USA SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
the two learned ranks select animation 3 and 35. The Item caller resolves the item's
spell operand while keeping the Item gesture. Dark Sword selects DESOUL 1.
Learned silence selects NONE.
[Spell definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) ·
[Caller selection](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm)

Setup (`0x1A73A..0x1A848`) loads tileset 13, plays cast cue 77 and assigns a 16-entry
horizontal backdrop offset table. After VDP bias, the two side origins are (64,24)
and (144,40). Update (`0x1C248..0x1C53E`) composes the 64×48 reaper from four
column-major sprites and plays cue 110 on orientation changes. Variation 2 adds two
16×16 souls at vertical offsets −16 and +32. Their frames alternate every six ticks
and they reverse every 60 ticks after staggered starts. The death branch
(`loc_18F26` / `loc_19076`) signals the strike, with 20 ticks of preparation,
strike tiles and cue 106, then 32 ticks of recovery. Ineffective results retain hover.
[Setup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/desoul.asm) ·
[Update](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/animation/update/desoul.asm) ·
[Result caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_0.asm)

**Modern presentation.** The actor animation trigger starts a one-second cast window.
After a 28/60-second flash interval, the reaper hovers on a deterministic schedule while
the backdrop shader uses the source offsets over its upper 96 logical rows. Each
`InstantDeath` reaction holds the scene for at least 52/60 second, starting its strike
and cue at 20/60 second. Ineffective reactions keep the hover without a strike or cue 106.
The view resets at reward, scene end or field death. Reduced Flash removes the caster
flash and ripple, flattens the hover bob and holds the soul frame. Held 1.5× uses the
shared scene clock. Playback draws no gameplay RNG. Cast 77 and hover 110 use inherited
audio. The package adds the prepared strike 106, using the session's YM5 replacement slot.
[Presenter](../../game/src/Battles/DesoulEffectView.cs) ·
[Scene and completion](../../game/src/Battles/BattleSceneView.cs) ·
[Audio routing](../../game/src/Audio/SessionAudio.cs)

All four rasters form one atomic group for both variations. Complete 4× HD retains
Original logical extents. An incomplete HD group uses all four Original bindings.
Without the complete Original group, learned DESOUL and Dark Sword Use both present
their battle messages on black. Silenced Magic keeps paid
suppression feedback. [Asset inventory](../content/asset-inventory.md#desoul-raster-family-2307)

### DESOUL reproduction and verification

Select `derived/issue2307/desoul-v1` together with independent #2252 opening, #2255
Witch and #2266 catalog inputs. The registered review package supplies the selectors,
five controlled records, consumer and ordinary-input launchers. Use the
[locked Debug adapter workflow](../development-and-verification.md#locked-net-workflow)
before native input. Run the launchers from the repository root with a fresh sample
name and an absolute disposable external save root.

```powershell
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2307/review-v1'
& (Join-Path $review 'Verify.ps1') -Repo (Get-Location).Path
& (Join-Path $review 'Boot.ps1') -Repo (Get-Location).Path -Case desoul2 `
  -Sample desoul2-review -SaveRoot $freshExternalSaveRoot -Graphics hd
# Other cases: desoul1, miss, silence, dark-sword.
# For desoul2, also use -Graphics hd -Reduced -Fast and -Graphics hd -Variant incomplete.
```

**Confirmed in the remake.** The production reader admitted 26 scenes, 52 learned
rank bindings and 26 Dark Sword bindings, and restored all five controlled records.
The [native observer](../../game/probes/engine_desoul_family_observation.gd) checks
ordinary input, actor/scenery and texture projection, one payment for Magic, ordered
HP application and death cleanup, one action reward and usable next input. Require
`passed: true`, process exit 0 and no script or adapter errors. Battle26 covered
Kazin rank 1 death, rank 2 ordered death then ineffective, all-ineffective rank 1,
paid silence without an effect, and Dark Sword Item at zero MP while silenced, with
one selected-copy wear draw. HD, Reduced Flash with held 1.5×, and incomplete-HD
fallback passed. With producer scratch unavailable, the published reader and HD
settings entry passed. Independent review repeated the locked Debug build, reader,
DESOUL 2 HD and Dark Sword Original from the published inputs. The two native runs
exited 0 and accepted further input. The HD run retained eight ObjectDB exit
warnings under [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).

The implementation gates reported 3,383 engine passes with 50 optional private-input
skips, passing locked Debug/Release adapter builds, both tool Release builds, H4
compile and direct Ruff, design-contract and research-index checks. The committed
engine-scope plan includes the DESOUL mount with no unclassified paths. The
[Draft PR](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2318) owns the
CI results. `derived/issue2307/verification-v1` retains the receipts, including
preparation interpreter/format/audio-selection failures, the initial missing audio
slot, the first Item observer's incorrect expectations and the unregistered-mount
planner refusal. Some successful native runs retained six to eight ObjectDB exit
warnings.

**Unknown.** Natural learning, acquisition and reach, retail hardware cadence and
palette behavior, actual GPU ripple appearance and human visual acceptance. These
headless witnesses establish state, input and resource projection through nonterminal
return. Terminal, immunity, reward and custody behavior retain their
[engine evidence](../domain/hostile-spells-design.md#desoul-12-implementation-and-evidence).
