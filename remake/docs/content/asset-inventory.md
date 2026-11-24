# Demo asset inventory and the Granseal art review

The detailed inventory is `manifests/pre-voyage-asset-inventory.csv` in the local product asset repository. Each row identifies a resource by owner, family and ID. Filter by `map-3` for the map, `shared actors` for walking frames, `shared dialogue` for portraits, or `battle-1` through `battle-7` for a particular encounter. `manifests/map3-cast.csv` joins map setup, entity or party member, sprite and portrait. These tables are produced from the selected demo content. They describe available assets; a conditional follower or unused block can be present without appearing in a playthrough.

| Owner in the selected demo | Identity and grain of a row | Where to look or what the variant covers |
| --- | --- | --- |
| 18 field maps | 5,332 map-local block IDs. Each has nine source tile references, layout-use count, sample coordinates and base/overlay area tags. | Original blocks draw at 24×24. Map3 alone supplies a complete bank of 96×96 HD blocks. Door, roof and story copies use the same block IDs. |
| Shared field actors | 462 poses: 77 sprite IDs × three direction strips × two walking phases. The lateral view is also mirrored. | Map3 selects the 96×96 treatment of each pose. A field cell, foot offset, phase and facing keep their existing meaning. |
| Shared dialogue and member sheet | 38 portrait atlases, including the hidden eye and mouth replacement columns. | Original 64×64 or 256×256 on Map3 in HD. Crop, mirror, animation mapping and panel dimensions are independent of texture density. |
| Field interface | 44 item/spell identity states plus 32 states across four command-menu families. | The herb retains its earlier two field variants. Other field icons keep their existing resources. |
| Battle presentation | 1,046 encounter-scoped raster identities. The ID distinguishes actor/frame, weapon, ground, backdrop, effect and menu art as applicable. | These are installed battle resources. Repetition of a character between battles is expected. |
| Title and witch | Six named entry layers, including the numeral, arrival, face and the two expressions. | Dimensions and used tile count refer to the entry pack. |
| Audio | 78 world cues and two entry cues with command, timer and sample rate. | Listed for navigation. Audio has no graphics variant. |

The CSV has 7,040 rows for this pack. Shared art appears once per logical owner and frame. This is the pre-voyage demo inventory; the rest of the retail ROM has a different scope. The [presentation owner](presentation-assets.md) also identifies the bundled UI font, theme and public-resource policy. Code-drawn windows and text are outside the raster table.

## Retail original preparation

**High rigor.** Prepare the original private resources once for multiple campaign consumers.
The fixed input is the registered 2 MiB USA ROM and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. The local asset Git publication owns the
reviewed bytes. The configured shared data root owns the working publication and assembled
packages. Runtime reads ordinary world/scene/entry inputs. Gameplay admission, translation beyond
the supplied name overlay, HD and public distribution have separate owners.

The base inventory joins H2 contracts, pinned pointer tables, aliases, class/item references and
the reviewed world/entry and asset manifest. Its 3,611 rows include distinct source IDs and
consumer bindings. The separate 7,040-row demo table above describes a different selection.
`manifests/retail-v1-coverage.csv` in the private asset Git adds the preparation status and
selector alongside each base row. Use its `retail_status` column. The reviewed commit
`52435b04f49a84a6a9ed4444620055a070648001` records 3,375 prepared rows, 32 command-menu
bindings already present and 204 inapplicable slots. These are CSV classification counts.
The [audio exceptions](#base-audio-coverage-exceptions) qualify the entries inherited from the
base world. Each family index identifies the ROM and source revision. Aliased slots refer to
shared physical art.

| Family | Prepared material and reuse |
| --- | --- |
| Field maps | All 79 descriptors with native atlases, 115 decoded indexed tilesets, 16 CRAM palettes, 77 shared blocksets and 77 shared layouts. The 19,771 block definitions and 315,392 layout words retain tile/cell flags. Area, foreground/roof, flag and step-copy records and tile animation operands are joined by map ID. |
| Walking and portraits | 720 ordinary direction slots. 669 distinct physical strips carry both phases, with nine sentinel slots and the remaining aliases. 59 valid member/class bindings include ordinary and promoted field IDs, battle sprites and palettes. The six special/action streams retain native indexed tiles and the ten source pointers. The 56 portrait slots select 52 unique PNGs and retain eye/mouth tile mappings. Special-sprite dispatch, coordinate assembly and timed presentation still depend on the engine capability. |
| Battle | Native-size transparent frames for 85 bodies with 163 palettes, plus the separate PLDN selection below. Both ally/enemy animation tables retain headers and frame/weapon operands. The 103 enemy species map to body/palette. 23 four-frame weapon tile banks, 42 two-color palettes and 84 item selectors retain the caller's body palette. All 30 ground and 30 backdrop pointers resolve to their physical variants. 23 spell containers, four invocation containers, status/transition graphics and Bolt sprite operands are available. Indexed effect tiles and control operands require the effect's original composition rule. No encounter matrix is generated. |
| UI and entry | All 163 source icon slots have normal and highlighted art. The raw other-art slots keep their own identities. Only supported semantic keys are mounted. The nine UI tile groups and font have prepared images. Nine special-screen streams are decoded. Private title/witch resources include palette/layout/bubble operands and the six existing mounted entry rasters. |
| Audio | The inventory includes 37 non-silent music commands and 56 SFX commands. The other 27 music slots select silent targets. New production supplied 18 music and 25 SFX candidates. Every new SFX records the controlled music timer context. The base supplies additional timer variants and the known authored entries below. Arbitrary caller/timer combinations and hardware waveform equality remain **Unknown**. |

### Base audio coverage exceptions

The historical base coverage counted inherited authored audio as prepared. Command 12/C4 used the
two-second defeat chord through issue2106. The current local asset commit
206f42802ccd06ddb880bcd453069f0c6284c8a6 adds the original cue at music/12/candidate.json,
bringing newly prepared music candidates to 19. Coverage excludes the recognized defeat placeholder
from original-audio coverage and selects the reviewed candidate when present.
[Capture, current package and reconstruction](../godot/audio.md#invasion-map3-sad-theme)

Command 109/C2 remains the explicitly authored 240 ms encounter notification. Its inherited coverage
expresses the accepted modern cue. Original waveform coverage for that command remains **Unknown**.
[Encounter capability](../capability-status.md#yeel-guide-through-battle03-implementation-agreement)

### First consumers

| Consumer | Available selection | Remaining acceptance |
| --- | --- | --- |
| #1956 | Chester, Rick and Eric field sprites 6, 28 and 34 have three 48×24 two-phase strips each and the original portrait/speech operands. Their Battle26 PLDN sprite 13 palettes are 0, 1 and 3, with five body frames each and respectively seven, seven and six item bindings. Spear/javelin attacks select sequence 81. The base supplies Higins palette 2. Decimal music 25 (`19` hex) is a 433,106-sample-frame finite cue, Timer B `CE`. | The progression mount and Content reader admit Chester, Rick and Eric as PLDN. Eric's ordinary promotion and Continue were observed. Controlled rules cover Chester's and Rick's eligible rites. [PLDN admission and observations](../application/ordinary-promotion-classes.md#pldn-ordinary-reproduction) |
| #1956 | Kazin and Tyrin use field sprites 12 and 43 and battle sprite 21 with palettes 2 and 1. Mount selects six prepared walking strips and eight existing body frames. Each palette reuses the four mounted rod/stick bindings. | Current readers admit both WIZ growth and presentation rows. Kazin's promotion, rod attack, learned cast and Continue were observed. Tyrin has controlled coverage. [WIZ admission and observations](../application/ordinary-promotion-classes.md#wiz-ordinary-reproduction) |
| #1956 | Jaha and Randolf use field sprites 9 and 40 and battle body 18 palettes 1 and 2. Luke uses field sprite 25 and battle body 14 palette 0. Mount selects nine prepared walking strips and fifteen body frames and reuses the existing axe and sword layers. | Current readers admit the three GLDT/BDBT rows. Ordinary Jaha and Luke played Battle26 and cold-continued. Randolf has controlled coverage. [Melee admission and observations](../application/ordinary-promotion-classes.md#gladiator-and-bird-battler-ordinary-reproduction) |
| #1956 | Elric and Janet use field sprites 31/46 and battle body 31 palettes 0/1. May uses field sprite 21 and battle body 29 palette 0. Mount selects nine walking strips and fifteen body frames. Items 45–50 have no independent weapon frame. | All three SNIP/BWNT member growth and body rows are mounted, with six equipment bindings each. [Ranged admission and observations](../application/ordinary-promotion-classes.md#sniper-and-bow-knight-ordinary-reproduction) |
| #1956 | Sarah and Karna use field sprites 3/37 and battle body 25 palettes 1/2. Mount selects six walking strips, six body frames and the four existing rod/stick layers per member. | Both VICR growth and appearance rows are mounted. Ordinary Battle26 observed both healing poses and Karna's staff. [Vicar admission and observations](../application/ordinary-promotion-classes.md#vicar-ordinary-reproduction) |
| #1956 | Gerhalt and Peter use field sprites 23/19, portraits 49/48 and battle bodies 30/28 with palette 0. Mount selects six walking strips, ten body frames and two portraits. Both have unarmed rows. | Both progressions and class/member presentations are mounted. The PHNX overlay adds four 32×32 frames from prepared spell tileset 20. Its selector 61 played in Battle26. [Wolf Baron and Phoenix reproduction](../application/ordinary-promotion-classes.md#wolf-baron-and-phoenix-ordinary-reproduction) |
| #1956 | Slade uses field sprite 15, portrait 50 and battle body 26 palette 0. Mount selects three walking strips, six body frames, eight compatible mounted sword layers and both states of icons 27/28. | Labeled controlled states exercise Ninja growth and its unarmed, sword and spell branches. [Ninja reproduction](../application/ordinary-promotion-classes.md#ninja-controlled-reproduction-and-ordinary-pilot) |
| #1956 | Kiwi uses field sprite 17, portrait 51 and body 27 palette 0. Mount selects three walking strips, five body frames, the distinct special sequence 84 and two prepared flame particles. | Labeled controlled states exercise MNST growth, its unarmed strike and flame conversion. [Monster reproduction](../application/ordinary-promotion-classes.md#monster-controlled-reproduction-and-ordinary-pilot) |
| #1950 / #1962 | Decimal 87 JUMP has 15,142 sample frames under Castle Timer B `D2` from Bedoe Map10. Decimal 99 BLOAB has 8,440 under Town `CB`. The latter is a controlled field context. | Both WAVs are available. Their feature tickets own the missing `SessionAudio.SoundSlots` classifications, actual calls and natural trigger verification. |
| #1967 | Both states for spell IDs 7 MUDDLE, 8 DESOUL and 9 SLEEP were read from the new field and Battle26 bindings. The prepared zh-CN catalog includes all three names. | Actual battle use and rule behavior belong to the consumer. |

**Confirmed.** Offline assembly from the stable files yielded a 145-file, 294,283,435-byte
priority package. The production opening reader admitted the three new field sprites, resolved all
six icons to equal field/battle bytes, and admitted looping command 4 and finite 25, 87 and 99.
A bounded Godot WASAPI playback of those four new admitted WAVs wrapped 4 with playback active
and emitted exactly one Finished for each finite cue. The earlier Chester/Rick candidate reproduced
the member-admission failure. The asset-only `promotion-v1` package assembled all three appearances
directly from private Git and the closed base. Only the Battle26 scene differs from the Chester/Rick
candidate. Its six added Eric bindings resolve to five new body rasters and 24 reused weapon rasters.
The user produced Eric's files after the initial automatic approval rejection recorded in #2001.
Previously recorded original New Game and Continue observations and the four-object exit warning
remain archived with the base.

### Resource budget and observed pilot

Run one family and one emulator at a time. Indexed graphics scale with unique resources, frames and
actual palettes. Music uses 176,400 bytes/second for stereo PCM16 at 44.1 kHz. The visual pilot
produced 326 icon states in 0.47 s. Maps took 5.25 s, 85 battle bodies 5.35 s, and the two-member
PLDN scene preparation 4.83 s. The source listing is about 24 MB. A timed finite music 25 pilot
took 6.73 s including source and library setup. Its 4,121,348-byte capture produced a 1,732,468-byte
WAV. Worktree scratch measured 10,138,595 bytes after staging that pilot. Sampled Python/emulator
working sets together peaked at 860,676,096 bytes, and the emulator peak was 853,745,664 bytes.
Both the crop and source capture reproduced byte for byte. The first new loop 4 used
42,685,004 capture bytes and 8,290,156 candidate WAV bytes. The first 43 successful native runs took about 398 seconds
in total. The failed boundary and timed finite repeat took another 19 seconds. For the finite
pilot, scratch plus its stable review copy occupied about 16 MB. The current reviewed library
contains 2,239 files and 1,095,202,183 bytes including the retained source WAVs.

The 18 music candidates use 196,874,396 runtime WAV bytes plus 775,395,168 source WAV bytes.
The 25 sound candidates use 2,137,372 and 103,033,700 bytes respectively. Keep these separate from
the visual budget. Music 15 initially lacked a whole-channel recurrence in the bounded check.
A 30,000-frame capture with a wider observation window matched full channel records at frames
4,154 and 7,969. The failed capture remains available locally. Loop boundaries establish driver
state recurrence. Perceptual seam review and natural gameplay across every track remain **Unknown**.

Allow 1 GiB headroom for a single visual or PLDN operation. On a fresh complete rebuild allow
5 GiB for the working and stable publications before the additional private Git copy. Disk use
depends on retained captures and package copies. Stop on input drift or a source/renderer
disagreement. A failed family leaves the previous publication available.
[Preparation, assembly and recovery commands](presentation-assets.md#prepare-and-mount-retail-resources)

## Map3 coverage and treatment

The outdoor area is 51×32 cells. Two interior areas each measure 11×10. The exterior's roof/facade plane has a 32-row offset and can be copied, revealed or restored. The player package includes all 447 block definitions, including copy sources and the declaration absent from the initial layout. HD supplies separate textures to the existing layer renderer. Tile priority, camera clipping and actor occlusion continue to use the layout and its nine tile words. The source atlas alpha distinguishes transparent index zero from opaque black ink.

The earlier painted master is `masters/map3-materials/surfaces.png` in the local asset repository at `2e8524dcdd94b1568cf2e15e389d660a1f876a54`. The accompanying README records the text-only GPT Image prompt. It supplies sixteen original material studies. The local painter selects the applicable surfaces for grass, leaves, water, earth, stone, shingles, oak, timber and iron. It applies those painted shapes at display resolution using the original's color regions, guide ink and opacity. Neighboring blocks share a boundary treatment. Walking frames and portraits use local contour, shadow and cloth treatments of their source poses; their silhouettes and character designs are retained. No game pixels were supplied to the image service. The herb uses its earlier Scale2x recipe.

## Map3 density-ramp terrain (#1765)

The current private review uses the Map3 L1 experiment from the project's graphics checkout at
`172662709b31cb296b55f51894754ce810c30f6c`. The publisher runs its own `expand_ramp_dither` and
`edge_aware_2x` implementations directly from that pinned checkout. Only offline production needs
the graphics checkout. The experiment, painted master and derived game pixels remain private.
[Graphics experiment and provenance](hd-art-pilot.md#what-the-earlier-experiment-establishes)

The treatment scans aligned 2×2 checkerboards. Eligible pairs have an OKLCH lightness gap of at
least 0.02 and a hue distance of at most 0.7 radians when both colors have chroma above 0.03.
A 12-pixel radius measures the fraction of lighter pixels among the pair. The closest endpoint
or one of up to four intermediate levels replaces the checkerboard. Other textures and checkers
containing a transparent pixel pass through to edge-aware 2× scaling. The original atlas distinguishes
opaque black from transparent index zero. Lossless nearest doubling fits the 48×48 L1 result into
the existing 96×96 HD slot. Original uses the same admitted 24×24 blocks as before.

**Confirmed:** neighbors affect both local density and edge reconstruction. The producer filters
each area and its foreground separately, then cuts the results into blocks. A 3×3 row-major group
of block IDs contains all the pixels needed for one block. The optional `hdBlockContexts` field
binds groups to 96×96 rasters. The renderer looks up the live working layout on each plane, including
after a roof, door or flag copy. The area edge uses -1 for a missing neighbor. Known layout edits
are prepared from their source rectangles. An unpublished combination selects the center block's
own L1 image. Older worlds continue to select their installed HD bank or original art.

### Rebuild and review

Select a complete ordinary private world and its matching opening scene. The observed review
used the #1759 publication with town Search and shops. The publisher replaces just the Map3
terrain variant. It preserves every original raster, existing character/portrait variant and
gameplay field, and copies the existing scene and dependencies. The #1759 world is 127,169,379
bytes. The review world is 125,267,166 bytes and the complete package with plates is about 191 MB.
The retained 128 MiB world-document limit applies. Allow one output package and 1 GiB scratch on
the output volume. Filtering visits only Map3's 447 blocks, three areas, foreground and bounded
copy patches. Peak memory includes the parsed world and one plane's filter data. No cloud calls.

```powershell
. ./local/private-inputs.ps1
# $world and $scene select the ordinary package; $gfx selects the pinned graphics checkout.
# Choose a fresh ignored directory for $output.
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_map3_ramp --world $world --scene $scene --gfx-root $gfx --out $output
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
```

Keep the matching entry, display catalogs and battle inputs. Use the ordinary title **Continue →
slot 1** from a separate review namespace with a compatible Map3 save near (32,13). Start in
Original. Open **F10 → Settings → Graphics → HD → Apply**, then switch back at the same position.
The prepared local `local/issue1765/Play Map3.cmd` launches this review. The neighboring
`replay.ps1` runs the same Continue and settings sequence with its own fresh disposable slot.

| Static comparison at the same coordinates | Observation |
| --- | --- |
| Grass and trees at (10,15)–(15,20) | 103 of 82,944 L1 RGB pixels differ from the historical composite. The input already differs at 20 Original pixels. |
| Riverbank at (20,14)–(27,17) | Block-local filtering differed from the old L1 at 501 pixels, including 362 in the boundary band. The final assembled blocks match all 73,728 reference pixels. |
| Wall and roof at (29,8)–(33,14) | The old renderer converted black to transparency and treated palette zero differently. The current Original differs at 1,882 of 20,160 RGB pixels. The candidate differs at 8,148 of 80,640 L1 pixels. Its foreground uses the admitted alpha and remains independent of the ground. |

The complete static exterior reassembled from the actual published 96×96 payloads, at native
L1 density, equals the separately filtered and composited plane image byte for byte. The bank
contains **58 distinct opaque colors** across base and context variants, up from 15. The earlier
84-entry table includes unused levels. Inspect `review/grove.png`, `riverbank.png`, `roof.png` and
`continue.png` for the same crops in Original and HD. These are private asset plates.

**Confirmed:** the [running host witness](../../game/probes/engine_map_art_observation.gd) loaded
the ordinary slot on the integrated UI. In 180 visible ground cells, selected textures used
15 → 42 → 15 opaque colors. Eleven foreground cells used 8 → 12 → 8. Both directions kept the
same tile passes, screen rectangles, four display pixels per logical pixel, paused progress and
slot bytes. Returning to Original reused its texture handles. The existing portrait changed
texture density within the same 144×192 panel. The native exit can still report its known
two-ObjectDB-instance cleanup warning. **Unknown:** player acceptance of the terrain style.

## Earlier painted package

For the optional Map3 HD package, first prepare or reuse the
[complete ordinary menu/identity/Item/Shop world and matching opening battle scene](presentation-assets.md#final-publication-and-ordinary-selection).
Menu publication alone does not generate the painted Map3 bank. Select that world, its scene,
the existing entry pack and the material master above as explicit read-only inputs. Set
`$materialMaster` to that master file and `$output` to a fresh ignored private output directory;
the publisher refuses an existing output or a world already containing painted Map3 blocks.
If a matching HD pack is already prepared, reuse its two outputs and skip regeneration.
To rebuild, after loading the configuration below, set `SF2_PRIVATE_EXPLORATION_CONTENT` and
`SF2_PRIVATE_BATTLE_SCENE_CONTENT` to the complete menu pack without the painted bank.
Run from the repository root. This is an offline Pillow dependency only.

```powershell
. ./local/private-inputs.ps1
# Confirm/select the non-HD menu world and its matching scene before rebuilding.
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_map3_hd --world $env:SF2_PRIVATE_EXPLORATION_CONTENT --scene $env:SF2_PRIVATE_BATTLE_SCENE_CONTENT --entry $env:SF2_PRIVATE_ENTRY_CONTENT --master $materialMaster --out $output
# Select both outputs in this same launch process:
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
```

The final pack retains the original content IDs and gameplay revision. Keep the existing entry,
battle/static inputs, display catalogs, save root/namespace and settings selections. Use the
[ordinary launch](../../README.md#run-locally) in the same configured process; New Game or Continue
then reads the matching `world.json` and `scene.json` above. Existing compatible slots retain their
compatibility rules; do not copy or rewrite a player's save to select artwork.

In Settings, choose **Graphics: HD** and apply. The settings transaction prepares the installed HD
content before publishing the change. On Map3, the outdoor and two interior areas use 96×96 blocks
in place of 24×24; actors and portraits gain density while logical geometry and panel size stay the
same. Choose **Graphics: Original** and apply to switch back. Other maps keep their installed art;
a world without the optional bank retains Original even when HD is selected. This makes package
selection distinct from the graphics preference. Human art-direction approval remains separate.

The local `review/` folder contains all three map plates, two interior comparisons, a street detail, a character comparison and the two CSV files. The map plates compose source areas and the static roof copy; a moving actor or an opened roof must be reviewed in the game. These private images belong on the reviewer's machine.

The selected HD world is about 121 MB (decimal), below the unchanged 128 MiB input limit. One release adds roughly 6.5 MB to the herb world. Preparation is bounded by the admitted cells, actor poses and portraits, and makes one set of complete field plates; it does not collect gameplay frames. Derived assets and slots remain private.

For the actual host witness, give `engine_map_art_observation.gd` an isolated `issue1717-check-*` namespace containing a compatible Map3 slot near (32,13), the installed pack, writable settings initially in Original and an explicit report destination. It loads through Continue, changes Settings in both directions and reopens the member sheet. Compare the actual drawn block widths, pass geometry, portrait density and paused progress. Field time can legitimately advance the main RNG between menu visits. Human art-direction approval remains with the player.
