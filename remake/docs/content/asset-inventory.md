# Remake asset inventory

This is the entry point for tracking a resource from its USA source identity through preparation,
selection, delivery and verification. The [presentation assets guide](presentation-assets.md)
owns preparation, package assembly and local provenance. The
[music command inventory](music-command-inventory.md) tracks each BGM command.

**Standard scope.** [#2163](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2163)
covers every presentation resource family, the two named private campaign selections and their
owners. Review at the resource or binding level with direct document, manifest and package
checks. Use the existing private manifests for per-ID graphics and SFX details. Capture, new
consumers, release and hardware-equivalence decisions belong to their feature owners.

## Read a resource row

| Stage | Evidence to look for |
| --- | --- |
| Source identity | The pinned USA enumeration, table, pointer or selector. Keep aliases, intended source variants and actual caller overrides visible. |
| Prepared material | The exact version of the private library, authored resource or derived art, with its ID and required palette, frame or timer binding. |
| Selected package | The named world/scene/entry set and whether its resource is present, original, derived, modern, placeholder or missing. Independent catalogs have separate selections. |
| Implemented consumer | The specific reader and the normal caller or presenter that can consume the selection. A compiled request records a static binding. Mark an unavailable reader, effect or use case **Unsupported**. |
| Verification | The bounded comparison or observation, including its package and caller. Use **Unknown** for unobserved reach, playback or visual acceptance. |

## Registered selections

All identifiers here are relative to the configured shared data root. Keep each package's
`world.json`, `main-scenes.json`, `entry/entry.json` and six `opening-inputs` selections
together. The active package is the set selected in the launch process.
[Ordinary package selection](presentation-assets.md#reuse-a-closed-private-package)

| Selection | Purpose and resource boundary |
| --- | --- |
| **Four-scene ordinary startup** · `derived/issue2252/pre-voyage-opening-v1` | Closed #2242 campaign successor with an independently bounded `opening/` selection, four source-ordered scenes,81 English/Chinese rows, compatible installed HD actors/portraits/Map42 terrain and existing six-layer entry HD. Map65 terrain and effects183/187 remain Original; terrain slot replacement falls back only for that effect. [Preparation, selection and observation boundary](presentation-assets.md#four-scene-startup-package-2252) |
| **Controlled opening scenes** · `derived/issue2250/opening-scenes-v1` | Complete Map42 shrine and bounded Map65 storm prefix, optional slot-index terrain resources,183/187 effects, selected modern audio and81 accepted Chinese rows. Independent of ordinary startup and the campaign selection. [Preparation and native reproduction](presentation-assets.md#controlled-opening-scene-package-2250) |
| **Pre-voyage** · `derived/issue2156/pre-voyage-map3-l1-v1` | Complete inline pre-voyage selection and source for the companion candidate. Provides [Map3 Original and L1 terrain](#complete-private-package-and-rebuild-2156), original cue 12 and the mounted pre-voyage spell rasters. Gameplay reach belongs to the [pre-voyage campaign](../application/pre-voyage-campaign.md). |
| **Pre-voyage companion candidate** · `derived/issue2174/pre-voyage-map3-companion-v1` | Complete selection for the HD rollout. Carries the same Map3 L1 centers and neighborhoods in one world-relative file. [Rebuild, admission and native comparison](#map3-terrain-companion-2174) |
| **Pre-voyage static terrain** · `derived/issue2175/pre-voyage-terrain-v1` | Complete field and tactical selection with twenty L1 banks and the original Map3 companion. [Production and live checks](#pre-voyage-static-terrain-2175) |
| **Pre-voyage actors and portraits** · `derived/issue2176/pre-voyage-actors-v1` | Complete terrain selection plus switchable shared actor/alternate and portrait banks. [Production and live checks](#pre-voyage-actors-and-portraits-2176) |
| **Pre-voyage battle artwork** · `derived/issue2177/pre-voyage-battle-v1` | Complete actor/terrain selection with eight close-up banks and shared allied body/weapon donors. Source package for the UI variants. [Production](#derive-selected-battle-rasters) · [Actual consumers](hd-rollout.md#private-battle-artwork-acceptance) |
| **Pre-voyage UI rasters** · `derived/issue2178/pre-voyage-ui-v1` | Complete battle artwork selection plus world and eight scene UI banks. Source package for the current entry selection. [Production](#pre-voyage-ui-rasters-2178) · [Actual consumers](hd-rollout.md#ui-raster-consumers-2178) |
| **Pre-voyage entry layers** · `derived/issue2179/pre-voyage-entry-v1` | Complete pre-voyage selection with six switchable title/Witch layers, carrying forward the #2178 UI, battle, actor, portrait and terrain banks. [Production](#pre-voyage-entry-layers-2179) · [Actual entry consumer](hd-rollout.md#production-entry-reproduction) |
| **Pre-voyage animated terrain** · `derived/issue2180/pre-voyage-tiles-v1` | Complete animated-terrain base. Eight map visuals extend the #2179 world with phase-specific Original and HD terrain. [Production and native replay](#pre-voyage-animated-terrain-2180) |
| **Enemy healing successor** · `derived/issue2227/pre-voyage-healing-v1` | Complete #2180 successor with Original/HD evil-fairy artwork and shared faction selection. [Resource and native boundary](#enemy-healing-family-2227) |
| **DETOX successor** · `derived/issue2232/pre-voyage-detox-v1` | Complete #2227 successor with 11 Original/HD DETOX frames across its declared close-ups. [Resource and closure](#detox-family-2232) |
| **PGNT** · `derived/issue1957/pgnt-v3` | Separate registered later progression/Battle26 witness. Its older Map3 HD bank has no L1 contexts and its cue 12 is the defeat placeholder. Preserve that evidence selection. Check resource lineage when proposing a successor that combines later progression with pre-voyage corrections. [PGNT reproduction](../development-and-verification.md#high-rank-elemental-magic-observation) |
| **Display catalogs** · `derived/issue1957/catalogs-v1`, `derived/issue2001/catalogs-v1` | Independent prepared Chinese name overlays for the owning reproductions. Match the catalog and entry locale to the named reproduction. A catalog supplies display names and does not select world, scene or media. [PGNT catalog selection](../domain/support-spell-status-design.md#battle26-ordinary-input-and-reproduction) · [Retail catalog selection](presentation-assets.md#sleep-display-name) |
| **Retail base and library** · `derived/issue2001/issue1966-original`, private asset Git `runtime/retail-v1` | Base world/entry provenance and preparation outputs used to assemble packages. Some prepared candidates have no current package binding. [Retail preparation](#retail-original-preparation) |

## Resource families and owners

The private asset Git entries below use `manifests/retail-v1-coverage.csv` for preparation
status and selector, and `manifests/pre-voyage-asset-inventory.csv` for the inventoried demo
snapshot. The current package and consumer owner establish what a particular game can show.

| Resource and identity | Source and prepared details | Package, consumer and acceptance owner |
| --- | --- | --- |
| Map tiles, tilesets, palettes, blocks and layouts | Retail `maps/index.json` and `map-*` CSV rows join map, set and palette references. A block contains nine source tile words. [Map data](../../../docs/research/world/map-data-inventory.md#map-content-closure) · [Graphics](../../../docs/research/presentation/technical-graphics.md#renderer-mapping-private-extraction-tooling) | World map and `presentation.maps` select live layout and raster data. Field and battle projection consume admitted areas. [Field projection](../godot/presentation.md#field-projection) · [Battlefield maps](../godot/battle-scenes.md#battlefield-maps) |
| Foreground, roof, door and animation variants | The map index retains plane, area, copy and animation operands. Preserve source tile flags and destination identity. [Map source](../../../docs/research/world/map-data-inventory.md) | The current map's live layout and renderer determine what copies and draws, including the [static banks](#pre-voyage-static-terrain-2175) and [tile substitutions](#pre-voyage-animated-terrain-2180). [Working layout](../godot/presentation.md#bounded-working-layout-and-draw-operands) · [#2167 map handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2167) |
| Walking, class appearances and special poses | Retail `walking/index.json` joins strip/phase and member/class aliases. Special streams retain their indexed tiles and dispatch references. `manifests/map3-cast.csv` connects entity and party IDs to their Map3 sprite and portrait. [Special sprites](../../../docs/research/presentation/technical-graphics.md#special-sprites-and-view-routing) | World `presentation.sprites` mounts the chosen strips. Class and special-pose callers own their selected appearance and remaining dispatch gaps. [Promotion](../application/ordinary-promotion-classes.md) · [Special-pose dialogue](../godot/presentation.md#special-pose-dialogue-portraits-2070) |
| Portraits and expressions | Retail `portraits/index.json` records slot aliases and replacement tiles. The atlas and eye/mouth mappings travel together. [Graphics](../../../docs/research/presentation/technical-graphics.md) | World `presentation.portraits` supplies dialogue and Member consumers, with optional Map3 density variants. [Story dialogue](../godot/presentation.md#shared-story-dialogue-agreement) · [Party details](../godot/presentation.md#party-details-agreement) |
| Battle body, weapon, ground and background | Retail `bodies`, `weapons` and `terrain` indexes retain body/palette/frame, item selector and encounter environment identity. [Source scene engine](../../../docs/research/battle/battle-scene-engine.md) | Main and encounter-specific scene inputs select composition and placements. Review the class/weapon scene binding at the encounter caller. [Scene roster and limits](../godot/battle-scenes.md#which-combatants-need-a-scene) · [Weapon composition](../godot/battle-scenes.md#weapon-frame-composition-1620) · [Backdrops](../godot/battle-scenes.md#fixed-backdrops-for-the-private-campaign) |
| Battle effect, spell, invocation and status/transition art | Retail `effects/index.json` retains indexed containers and effect controls. The spell table joins family and rank to selector and caller overrides. [Spell source](../godot/spell-presentation.md#source-and-status) | Check the scene mount and actual cast or Use separately. FREEZE 3/4 has admitted learned Magic and a bounded Battle26 witness with placeholder ice. Original effect composition and playback remain open. The four summon families have learned private rules; DAO1 has a bounded SORC native witness using the existing modern fallback. Original summon effects remain unavailable under #2104. [Spell coverage and follow-ups](../godot/spell-presentation.md#attack-magic) |
| UI art, icons, cursors and fonts | Retail `icons` and `ui` indexes distinguish storage slots, semantic IDs and normal/highlighted states. Theme, glyphs and licensed fonts have separate provenance. [Menus](../../../docs/research/presentation/common-menus.md) · [Fonts and theme](presentation-assets.md#fonts-theme-and-input-glyphs) | World and scene namespaces mount supported menus and identity icons. Check each enabled caller's acceptance and legacy text fallback. [Menu inventory](presentation-assets.md#bounded-source-to-consumer-inventory) · [Identity art](../godot/presentation.md#identity-art-acceptance-1667) · [Target cursor](../godot/battle-scenes.md#original-target-and-inspection-cursor-publication) |
| Entry and other special screens | Retail `entry/index.json` covers special streams and title/witch layout, palette and bubble inputs. [Original screens](../../../docs/research/presentation/special-screens.md) | The independent entry document mounts title/witch images and two music resources. Other screen streams are admitted per caller. [Entry consumer](../godot/presentation.md#production-entry-and-field-dialogue-1606) |
| BGM and SFX | [Sound inventory](../../../docs/research/presentation/sound-data-inventory.md) owns source IDs, bank aliases and timer/channel context. The private coverage CSV, per-command candidate and inherited base own prepared variants. | World and entry audio rows own selection, including the [command 12 and authored 109 distinctions](music-command-inventory.md). [Session audio](../godot/audio.md) owns cue, finite default, lifetime and playback acceptance. |
| Original/HD, authored UI and approved modern replacements | Match the original ID to its specific derivative or authored replacement. The graphics experiment and local derivative identities have their own provenance. [HD pilot](hd-art-pilot.md) · [Public authored resources](presentation-assets.md#product-design-and-distribution-limit) | The twenty pre-voyage terrain banks select Original or L1 via Settings. Actor and portrait variants have separate mounts. Other consumer surfaces keep their selected assets. [Map3 review](#map3-density-ramp-terrain-1765) · [#1699 graphics](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1699) · [#1456 human acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456) |

For **map configuration**, start with the source
[setup/selection records](../../../docs/research/world/map-data-inventory.md#setup-selection-and-rom-parity)
and the remake [location](campaign-locations.md) and [flag](campaign-flags.md) owners. They identify
the setup, guard, program and arrival to trace into the named world's map/program records and
the actual session caller. The asset row tracks the tile, palette, block and layer data selected
by that configuration. Configuration compilation, package selection and ordinary execution
each have their own evidence.

Source-derived media and its HD derivatives are private. Project-authored public assets
and licensed fonts follow their own provenance and distribution terms. Keep original, modern
and placeholder choices explicit where they share a semantic ID.
[Distribution owner](presentation-assets.md#product-design-and-distribution-limit) ·
[#1825 release](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1825)

## Open resource and consumer checks

- Keep original cue 12 and the L1 Map3 bank together in the next complete package that
  needs both pre-voyage fidelity and later progression. Use the registered PGNT package
  for its existing witness. [#1957 progression](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957)
- Bind prepared cue 15 with the first admitted later caller that requests it, such as
  Roft, and observe cue delivery and return. The compiled Map72 request follows the
  init terminator. [Music row](music-command-inventory.md#non-silent-commands) ·
  [#1818 campaign](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818)
- Check original cue 12 continuous playback across ordinary invaded-house stairs through
  audio handoff ownership. [#2165 audio initialization](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2165)
- Take placeholder attack effects and missing original summon presentation through their specific
  rank and encounter owners. Keep asset selection, gameplay admission and native
  presentation evidence distinct. [#2104 spell presentation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104) ·
  [Spell family table](../godot/spell-presentation.md#attack-magic)

## Demo inventory snapshot

The detailed inventory is `manifests/pre-voyage-asset-inventory.csv` in the local product asset repository. Each row identifies a resource by owner, family and ID. Filter by `map-3` for the map, `shared actors` for walking frames, `shared dialogue` for portraits, or `battle-1` through `battle-7` for a particular encounter. `manifests/map3-cast.csv` joins map setup, entity or party member, sprite and portrait. These tables describe the inventoried demo snapshot. A conditional follower or unused block can be present without appearing in a playthrough.

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
The [audio coverage interpretation](#audio-coverage-interpretation) qualifies the entries inherited from the
base world. Each family index identifies the ROM and source revision. Aliased slots refer to
shared physical art.

| Family | Prepared material and reuse |
| --- | --- |
| Field maps | All 79 descriptors with native atlases, 115 decoded indexed tilesets, 16 CRAM palettes, 77 shared blocksets and 77 shared layouts. The 19,771 block definitions and 315,392 layout words retain tile/cell flags. Area, foreground/roof, flag and step-copy records and tile animation operands are joined by map ID. |
| Walking and portraits | 720 ordinary direction slots. 669 distinct physical strips carry both phases, with nine sentinel slots and the remaining aliases. 59 valid member/class bindings include ordinary and promoted field IDs, battle sprites and palettes. The six special/action streams retain native indexed tiles and the ten source pointers. The 56 portrait slots select 52 unique PNGs and retain eye/mouth tile mappings. Special-sprite dispatch, coordinate assembly and timed presentation still depend on the engine capability. |
| Battle | Native-size transparent frames for 85 bodies with 163 palettes, plus the separate PLDN selection below. Both ally/enemy animation tables retain headers and frame/weapon operands. The 103 enemy species map to body/palette. 23 four-frame weapon tile banks, 42 two-color palettes and 84 item selectors retain the caller's body palette. All 30 ground and 30 backdrop pointers resolve to their physical variants. 23 spell containers, four invocation containers, status/transition graphics and Bolt sprite operands are available. Indexed effect tiles and control operands require the effect's original composition rule. No encounter matrix is generated. |
| UI and entry | All 163 source icon slots have normal and highlighted art. The raw other-art slots keep their own identities. Only supported semantic keys are mounted. The nine UI tile groups and font have prepared images. Nine special-screen streams are decoded. Private title/witch resources include palette/layout/bubble operands and the six existing mounted entry rasters. |
| Audio | The inventory includes 37 non-silent music commands and 56 SFX commands. The other 27 music slots select silent targets. The reviewed library carries 19 original-driver music and 25 SFX candidates. Each SFX candidate records its controlled music timer context. The base supplies additional timer variants and one retained authored cue. Arbitrary caller/timer combinations and hardware waveform equality remain **Unknown**. |

### Audio coverage interpretation

The [music command inventory](music-command-inventory.md) joins prepared clips to the two
registered selections and the consumer evidence for each command. Use it to interpret the
inherited preparation counts. Command 12 is original in the current pre-voyage package
and remains a placeholder in the older PGNT selection. Command 109 retains its accepted
modern encounter notification. [Command 12 provenance](../godot/audio.md#invasion-map3-sad-theme)

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
| #1950 / #1962 | Decimal 87 JUMP has 15,142 sample frames under Castle Timer B `D2` from Bedoe Map10. Decimal 99 BLOAB has 8,440 under Town `CB`. The latter is a controlled capture context. | The Bedoe and Chirrup Sandals owners track their actual callers separately from these prepared WAVs. [Bedoe branch](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1950) · [Chirrup playback](special-equipment-reach.md#remake-controls-and-reproduction) |
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

The current private terrain uses the Map3 L1 experiment from the project's graphics checkout at
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

### Complete private package and rebuild (#2156)

**Standard scope.** Restore the selected L1 treatment in Map3's terrain bank and live
neighborhoods. Retain the latest complete package's original art, existing HD actors and
portraits, gameplay, entry and audio. Graphics settings, saves and the independent Chinese
display catalog use their existing selections.

**Confirmed.** The #1790 launcher selected the separate #1765 review world. That release world is
absent from its retained handoff. The entire 447-raster bank in #2158 matches the retained #1717
painted bank byte for byte. The #1759, #1857, #1966 and #2106 world selections all carry that
bank and zero contexts. #1759 was also the input to the original L1 publisher, whose output
contained only the world, scene and dependencies declared by that world. The complete-package
copy takes the terrain of its selected source world. Carry the L1 world forward by starting from
the complete package below. **Unknown:** which intervening launcher or operator selection first
returned to the painted lineage. No complete L1 package was found in the registered inventory or
in private asset Git.

The registered selection is `derived/issue2156/pre-voyage-map3-l1-v1`. It derives from
`derived/issue2158/pre-voyage-sad-theme-v1` and includes the original command-12 cue from that
package. It has 148 files and 324,058,227 bytes. Of those files, 147 are byte-identical to #2158.
In the parsed world, only Map3's `hdBlocks` and `hdBlockContexts` differ. This complete
selection supplies the original inline bank for [transport](#map3-terrain-companion-2174). The optional plates go to checkout-local scratch.

Allow one full output package and 1 GiB disk headroom. This run started with about 35 GB free,
and produced a 100,937,361-byte world under the 128 MiB limit. I/O scales with the package bytes.
Filtering is bounded to 447 blocks, three areas, the exterior foreground and source copy patches.
Peak memory includes the parsed world and one plane's filter data.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
$base = Join-Path $derived 'issue2158/pre-voyage-sad-theme-v1'
# $gfx selects the pinned graphics checkout. $output selects a fresh private destination.
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_map3_ramp --base $base --gfx-root $gfx --out $output
```

Add `--review $review` with a fresh checkout-local scratch path to retain private plates for
the grass and trees, riverbank, roof and Continue view. The earlier `--world` and `--scene` pair
still produces a terrain review with its own scene and world dependencies.

Select the published `world.json`, `main-scenes.json`, `entry/entry.json` and six
`opening-inputs` together via the ordinary `SF2_PRIVATE_*` variables. The
[complete-package command](presentation-assets.md#reuse-a-closed-private-package) can check the
published closure using those inputs. For the
[native map-art probe](../../game/probes/engine_map_art_observation.gd), copy the registered
disposable `issue2049-stable-map3-f609` slot at (32,13) into a fresh external
`issue1765-check-*` namespace. Start with default Original and writable preferences. Use the
Debug adapter, a 1920×1080 headless window and a private `SF2_HOST_UI_OUTPUT`. The probe drives
ordinary title Continue, then switches through F10 Settings in both directions.

**Confirmed static.** All 447 blocks match the fixed L1 filter and nearest doubling. The bank
and its 767 context rasters cover 1,389 neighborhood keys and contain 58 opaque colors, including
opaque black. Transparent holes remain distinct. Reconstructing the three areas and exterior
foreground from the published textures at 48 px matches independent whole-plane filtering.
This includes grass and trees, the dirt and water boundary, walls and roof. Independent
whole-plane checks also match the six door edits, the eight distinct roof edits and the visible
flag-506 copy at (28,22). The second flag-506 copy at (57,23) lies outside the admitted planes.
The [earlier comparison with the graphics experiment](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/614aa3979d8a54f46609b4177eaf77484f716277/remake/docs/content/asset-inventory.md#rebuild-and-review)
records source-pixel and transparency differences in that historical selection.

**Confirmed native.** The published package continued with the producer plates unavailable.
Ground switched 24 → 96 → 24, with 115 of 180 visible ground cells and all 11 foreground cells
selecting neighborhood variants in HD. Ground and foreground kept their tile passes, draw
rectangles and 96 px world cells. The portrait switched 48 → 192 → 48 inside the same
144×192 panel. Both Apply operations preserved progress and the copied slot bytes.
In a separate Continue, ordinary Down
input changed the church door at (32,15) from block 143 to copied block 14, both with live HD
neighborhood selection. At (32,16) the roof restored its saved words and resumed contextual
96 px foreground draws in pass 3. Both observations exited 0 with the known two-ObjectDB
cleanup warning. Player review of this rebuilt package remains **Unknown**.

## Map3 terrain companion (#2174)

**Confirmed** against the complete #2156 package. The candidate has 149 files and 324,032,107
bytes. Its 99,392,754-byte world selects `hd/terrain/map-3.json` (1,518,487 bytes, 18.1% of
the 8 MiB bound). The 447 centers and 767 context rasters retain 973,959 encoded payload bytes
and 1,389 neighborhood keys. All 147 other files match byte for byte. Reinserting both bank
fields into the world restores the #2156 world bytes. No new art payload entered the prepared
library. The staged conversion and closure discovery took 1.96 s and peaked at 261.31 MiB on the
calibration host. Budget one 310 MiB output plus one relocated copy and 103 MiB for the authored
controls. Keep at least 1 GiB of free disk beyond these allocations.

From the repository root, set `$output` to a fresh absolute destination under registered
`derived/` or checkout-local `local/`. The producer publishes only after staging. To reproduce
the comparison, use the following with the same-process private configuration:

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2156/pre-voyage-map3-l1-v1'
function PackageInputs($pack) {
    '--world', "$pack/world.json", '--scene', "$pack/main-scenes.json",
    '--entry', "$pack/entry/entry.json", '--opening-inputs', "$pack/opening-inputs"
}
$inputs = PackageInputs $base
uv run --locked python -m sf2tool.remake_private_package @inputs --externalize-hd-terrain map-3 --destination $output
if ($LASTEXITCODE) { throw 'Publication failed' }
$inputs = PackageInputs $output
$copy = Join-Path $PWD 'local/hd-admission-copy'
uv run --locked python -m sf2tool.remake_private_package @inputs --destination $copy
if ($LASTEXITCODE) { throw 'Relocation failed' }
uv run --locked python -m sf2tool.remake_private_package @inputs --destination $copy --check
if ($LASTEXITCODE) { throw 'Copy differs' }
```

For direct comparison, run the following Python body with `uv run --locked python -X utf8 - $base $output`
on stdin. It checks the full file set, all unchanged siblings and the original serialization.

```python
import filecmp, json, sys
from pathlib import Path
from sf2tool.remake_private_package import read_json

base, candidate = map(Path, sys.argv[1:])
def names(p):
    return {f.relative_to(p) for f in p.rglob("*") if f.is_file()}
world = read_json(candidate / "world.json")
visual = next(v for v in world["world"]["presentation"]["maps"] if v["map"] == "map-3")
name = Path(visual.pop("hdTerrainFile"))
bank = read_json(candidate / name)
assert bank.pop("map") == visual["map"]
visual.update(bank)
assert names(candidate) == names(base) | {name}
assert all(filecmp.cmp(base / n, candidate / n, shallow=False) for n in names(base) - {Path("world.json")})
assert json.dumps(world, ensure_ascii=False).encode() == (base / "world.json").read_bytes()
```

Use the [locked .NET environment and PrivateAdmission build](../development-and-verification.md#direct-reproduction).
From `remake/`, pass `--repository .. --hd-terrain local/hd-admission-copy` to the Release tool. Paths after `--hd-terrain` resolve from
the checkout root. The tool adds two fresh control files to that disposable copy; choose a fresh copy for each run. It admits an
uncovered map and checks the actual centers and neighborhood bytes of an authored bank through
`OpeningRestoreReader` in both forms. It also exercises center-only, mixed declarations, map
identity, duplicate fields and context keys, raster identity, syntax, missing file, escape and
oversize outcomes. Each invalid declaration must return its expected ContentError.

**Confirmed native.** Use slot 1, v6 sequence 2 from the registered Map3 pre-recovery backup
(`map3-house-fault-20261124-031746/saved-before-recovery`, at 32,13). Copy only
`sf2-opening/slot1.json` to a fresh external `issue2174/sf2-opening/` under the disposable save
root. Start in Original with writable preferences. Build the Debug adapter and use the
[ordinary package environment](presentation-assets.md#reuse-a-closed-private-package) for each
of the two closed selections, first #2156 then #2174. Run from the checkout with an absolute
private output for each observation, the shared SDK on process PATH and worktree-local writable
runtime directories:

```powershell
$env:SF2_SAVE_ROOT = $disposableSaveRoot
$env:SF2_SAVE_NAMESPACE = 'issue2174'
$env:SF2_HOST_UI_OUTPUT = Join-Path $runRoot 'map-art.json'
& $env:GODOT_BIN --headless --path remake/game --resolution 1920x1080 --audio-driver Dummy --max-fps 60 `
  --log-file (Join-Path $runRoot 'map-art.log') --script res://probes/engine_map_art_observation.gd
```

Ordinary Continue mounted the candidate with producer staging gone. The probe holds the field
simulation delta at zero after attach, keeping actor ink at the same phase while servicing
normal keyboard input, Settings and draw callbacks. Compare the six `selectedDrawsSha256` rows
between receipts. They include actual RGBA texture readback, neighborhood selection, tile words,
source/destination rectangles, subject and ordered pass. All six matched. Each ground sample
had 1,669 draws over 180 cells, including actor occlusion. Foreground had 99 draws over 11
cells. HD reached 114 ground and all 11 foreground neighborhoods. Density went 24 → 96 → 24,
portrait width 48 → 192 → 48, with equal paused progress and identical slot bytes.

| Closed selection | Slot confirmation through mounted view | Apply HD / return to Original |
| --- | ---: | ---: |
| #2156 inline | 311 ms | 333 / 226 ms |
| #2174 companion | 310 ms | 363 / 232 ms |

Apply includes input press through release. Both selections restored the original texture IDs
and retained all 119 sampled HD handles in the visit cache after switching back. Both processes
exited 0 with the existing two-ObjectDB cleanup warning. **Unknown** GPU memory, residency over
a multi-map route and first Apply with additional maps. The [twenty-bank measurement](#pre-voyage-static-terrain-2175) covers the expanded selection and active-map lifetime.

## Pre-voyage static terrain (#2175)

**Confirmed** for the twenty map IDs in the [rollout selection](hd-rollout.md#coverage-from-the-selected-package).
Retail `runtime/retail-v1/maps/index.json` identifies original atlases, palettes, blocks and
layouts. The producer consumes their admitted counterparts in the complete #2174 package and the
Map3 L1 algorithms at graphics commit `172662709b31cb296b55f51894754ce810c30f6c`.
Stop at first stable Map14 control. Packed additional rooms are an art capacity bound.
Campaign admission still belongs to the [pre-voyage location agreement](../application/pre-voyage-optional-locations.md).
Water and other animated tile phases belong to [#2180](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2180).

The [working-layout selections](../../../src/sf2tool/remake_terrain_layouts.py) take copy operands
from the mounted programs and event tables. Copies read the evolving layout in source order.
Flag groups are applied together. Nearby door/chest changes contribute joint neighborhoods.
One active roof can replace or clear its destination and restoration returns to the underlying
layout. Tactical planes use the extent of their own source crop. Independent scene reads can
sample elsewhere in the 64×64 layout. The [producer](../../../src/sf2tool/remake_pre_voyage_terrain.py)
filters raster patches with one block of sampling halo and publishes only neighborhoods whose
pixels differ from the center. The filter operates on exact source RGBA including transparent
holes and opaque black. Geometry, flip and priority still come from the original words.

From the checkout root select the configured immutable input and a fresh output. Set `$gfx` to the
pinned graphics checkout. Scratch-only pilots compare crops to independently filtered whole planes.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2174/pre-voyage-map3-companion-v1'
$output = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2175/pre-voyage-terrain-v1'
$producer = @('run','--locked','--with','pillow==11.3.0','python','-B','-m',
    'sf2tool.remake_pre_voyage_terrain','--base',$base,'--gfx-root',$gfx)
foreach ($map in 4,66) {
    & uv @producer --pilot $map --out "local/pilot-$map"
    if ($LASTEXITCODE) { throw "Terrain calibration failed for map $map" }
}
& uv @producer --out $output
if ($LASTEXITCODE) { throw 'Terrain publication failed' }
```

| Pilot | Centers | Additional rasters | Distinct neighborhoods checked or selected | Bank bytes | Filter and publication | Process peak |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Map4 | 352 | 474 | 974 | 1,106,056 | 7.25 s | 261.01 MiB |
| Map66 | 500 | 1,713 | 2,936 | 3,606,612 | 21.03 s | 284.11 MiB |

Both pilots included package parsing, whole-plane comparison and bank serialization. Map4 compared
6,984 cell crops across ten planes/states. Map66 compared 23,681 crops across nineteen.
The timings exclude initial JSON parsing. Peak working sets include it. Map4 exercised the initial
layout, every intermediate quake copy and F624 reconstruction. Map66 included its full 64×64
plane, encounter footprints and the ship copy. Both stayed inside the planned 8 MiB per bank and
1 GiB offline process peak. The serial full run peaked at 385.84 MiB. The closed output contains
168 files and 342,904,510 bytes. World JSON is 99,393,569 bytes against the 128 MiB admission
bound. The twenty banks total 20,390,075 bytes for 5,568 centers, 10,232 additional rasters and
18,404 published context keys. All 148 previous files other than the world compare byte-for-byte.
Removing the nineteen new `hdTerrainFile` fields gives exact parsed world equality. The source
package, including Map3, remains a separate read-only reproduction. To repeat closure verification,
enumerate both selections with `sf2tool.remake_private_package.package_files`, compare common
non-world files directly and remove only the nineteen new references before comparing parsed worlds.
Both final pilot files also compare byte-for-byte with the full publication.

### Static terrain native replay

Use the [ordinary closed package environment](presentation-assets.md#reuse-a-closed-private-package)
with `$output`, writable Original settings, the locked Debug adapter and Godot 4.7.2 headless.
Use checkout-local TEMP, APPDATA, LOCALAPPDATA and an external disposable save root. Under its
`issue2175/sf2-opening/` copy the registered [post-tower slot 4 and the reconstructed Map4 slot 5](../application/pre-voyage-campaign.md#castle-to-quake-handoff-from-a-player-save-2167).
Keep their original slot names. Place the [Map3 review slot](#map3-terrain-companion-2174) in a
sibling disposable `issue2174/sf2-opening/` for the existing fixed-clock Map3 comparison.

Run `engine_pre_voyage_terrain_observation.gd` for each row below. For controlled field inputs,
write a JSON object with a `start` field to the selected worktree-local start file. Its fields are
`map`, `player` (`entity-0`), `position` (`x`, `y`), `facing`, `speed` (32) and `flags`.
For `shrine`, `signed` and `door`, decode the copied slot 4's base64 `payload` as UTF-8 JSON and
take `data.registers.flags` excluding F133 and F901. These are controlled initial states.
`hold` uses flags [32]. Each battle start uses only `battle` (`battle-5` or `battle-6`) and
`activeAllies` ([`ally-0`]). Keep the save namespace separate from manual play.

```powershell
$env:SF2_SAVE_NAMESPACE = 'issue2175'
$env:SF2_HOST_UI_CASE = $case
$env:SF2_HOST_UI_START = Join-Path $runRoot "$case-start.json"
$env:SF2_HOST_UI_OUTPUT = Join-Path $runRoot "$case.json"
& $env:GODOT_BIN --headless --path remake/game --resolution 1920x1080 --audio-driver Dummy --max-fps 60 `
    --log-file (Join-Path $runRoot "$case.log") --script res://probes/engine_pre_voyage_terrain_observation.gd
```

| Case | Initial condition and observed use |
| --- | --- |
| `quake` | Ordinary Continue slot 4. Castle warning and south exit select Map4. The first finished quake and F624 return each draw 99 contextual cells with 100 textures in the 11×10 region. The second south step remains blocked. F217 stays clear. All sampled castle handles expire on entering Map4 and first-visit Map4 handles expire at the docks. The source slot is byte-identical. |
| `cold` | Fresh Continue slot 5. The reconstructed quake region gives 99 contextual cells in HD and 85 distinct native textures in Original. Apply preserves the paused progress and slot bytes. |
| `battle5`, `battle6` | Controlled battles use Maps16/66 at origins (0,14)/(21,2). Original → HD → Original samples match on destination rectangles, block/tile words, priority, draw count and board geometry. The actual source tile is 8×8 or 32×32 at the same 32×32 destination. Compare each HD receipt's `contexts[].rgba` with the RGBA pixels of the bank PNG keyed by its row-major 3×3 neighborhood, clipped to its source footprint. Map16 yielded blocks 36 and 242, Map66 block 50. |
| `door` | Map43 (11,10), Up. The door changes from 0xC42A to 0x0806. The active roof clears all 96 cells and leaving restores the cached rectangle. The field draws HD at each step. |
| `shrine` | Map42 (19,23), Down. Ordinary Search changes 0xD801 to 0xD802 and sets F133. The lower room, upper hall and outside on Map41 all draw contextual HD. |
| `hold`, `signed` | Map44 hold (59,9), Left, and deck (27,7), Up. Both hold layers draw. On deck, A(24,4) and B(24,36) project at the signed displacement of −32 cells. Blocks 29 and 207 select different 96px neighborhood textures. |
| `voyage` | Continue slot 5, walk to the captain and accept. Map66 draws the shore, Map73 draws B at (0,10) and after the independent shift at (12,42). A short A-plane draw reached the nonempty source (11,0) with block 3, despite the blank 11×10 room at (0,0). The witness reads a single actual draw's source, texture and plane position together. First stable Map14 control draws HD with F700 set. |

**Confirmed** with the same full candidate for both lifetime comparisons. At `6160df05`,
the existing first Apply on Map3 walked every admitted field HD bank and took 1,055 ms.
With active-bank preparation the fixed-clock Map3 probe measured 231 ms for Apply HD and 124 ms
for Original, including press and release. Continue confirmation through mounted view took
316 ms. All six actual readback/geometry digests matched across the two implementations. All 119
sampled HD handles stayed valid for the toggle round trip on the same map. A map change retires
the prior field bank's uploaded textures and pickup alpha masks. A battle retains its own active
board textures until release. Encoded bytes remain in the admitted content model. **Unknown**
driver memory and timings on other hardware. Static terrain observations establish neither
tile-phase animation nor player approval of every map.

The first probe selected the wrong slot by list position in a sparse directory. Selecting the
visible slot label corrected that observation. The initial controlled shrine party lacked the
join flags for the chest grant. The captured flags above restore that prerequisite. An early
arrival check tried to read a second frame after seeing a nonempty A draw and missed the draw.
The observation now samples the texture used by that draw directly. Another battle probe included
ordinary playback between menu visits in its state comparison. Compare tactical state while
Settings is paused and compare terrain geometry across all three draws. These were probe/input
failures. The production layout and slot bytes retain their accepted identities.

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

## Pre-voyage actors and portraits (#2176)

`derived/issue2176/pre-voyage-actors-v1` extends the closed #2175 package. The original admitted
rasters are the immutable source; their source IDs, palette bindings and private provenance remain
with the walking/portrait indexes above. `sf2tool.remake_hd_actors` uses the same pinned graphics
filter as terrain, with the [transparent-family profile](hd-rollout.md#actor-and-portrait-implementation-2176).
It changes only optional `hdDirections`, `hdGraphics` and `hdRaster`. All Original fields and all
167 other package files compare directly equal. This includes the twenty terrain banks, opening
inputs, close-up scenes, audio and entry. Use the whole selection; do not mix its world with a
previous package's sibling paths.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2175/pre-voyage-terrain-v1'
$gfx = '<configured independent graphics repository at 172662709b31cb296b55f51894754ce810c30f6c>'
$candidate = Join-Path (Get-Location) 'local/actors-candidate'
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_hd_actors `
    --base $base --gfx-root $gfx --out $candidate
```

`--pilot` limits conversion to sprites 0/59/252 (including the three alternate groups) and
portrait5 while retaining the complete package. Choose fresh output directories. Decode the
source/candidate images to compare nearest-expanded alpha and opaque-black masks exactly; compare
source frames separately and preserve portrait tile boundaries. The complete run produces 498
direction bindings from 453 unique strips, 46 unique portrait atlases and a 97,658,510-byte world,
under the existing 128MiB limit. The closed 168-file package is 341,169,451 bytes. The pilot and
full producer plus direct pilot check took about four seconds on the review machine; this is not
a hardware-independent latency guarantee or a peak-memory measurement. Preparation only retains
the admitted world and small shared bank; it does not collect playthrough frames.

Publish with the existing copier after the focused observations:

```powershell
$output = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2176/pre-voyage-actors-v1'
uv run python -B -m sf2tool.remake_private_package `
    --world "$candidate/world.json" --scene "$candidate/main-scenes.json" `
    --entry "$candidate/entry/entry.json" --opening-inputs "$candidate/opening-inputs" `
    --destination $output
```

For an existing publication add `--check` to compare every file directly. Make the producer
scratch unavailable and repeat a normal consumer through the published package. The
[actor observations](hd-rollout.md#focused-observations) name the entry points and bounded claims.
Reproduction requires no historical ignored reports. Other HD families remain tracked by #2171.

## Battle raster variant binding (#2177)

The optional `hdRasters` scene object binds independently prepared PNGs to exact keys in the
scene's `rasters` object. The existing actor, palette, weapon and terrain selectors continue to
select those keys. The same bank can contain the effect keys already selected by the spell,
recovery or field-exit presenters. The package keeps the scene files and all existing dependencies
at their registered relative paths. [Battle graphics consumer](../godot/battle-scenes.md#graphics-variants)
· [Prepared spell groups](../godot/spell-presentation.md#graphics-variants)

Pass a directory of authored variant PNGs for each scene to extend. A key such as `body/front`
uses `body/front.png` relative to that directory. Each variant has equal-axis 2× or 4× original
raster dimensions and preserves the exact nearest-expanded alpha and opaque-black masks.
The producer checks the original raster identity before using it for this comparison. The
existing 4 MiB bound applies to each full scene JSON after embedding the variants.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_hd_battle_rasters `
    --base $base --out $candidate --scene main-scenes.json $authoredPngs
```

Use an absolute fresh candidate destination under the worktree's `local/`. Repeat `--scene`
for an admitted encounter sibling. The opening-input scene definition supplies battle setup;
the raster owner is `main-scenes.json`. The command prints per-scene variant
counts, input and output sizes, and the total closed-package size. An invalid declaration aborts
staging before publication. Use the existing package copier to publish a verified candidate.
[Closed package publication](#pre-voyage-actors-and-portraits-2176)

**Confirmed** on original synthetic art and an authored scenario. The packager retained a main
scene, an encounter scene, and the full declared closure, which was reopened after removing the
input scratch. Wrong density and changed masks aborted staging. The [native observation](hd-rollout.md#battle-raster-observation)
checks the runtime bindings independently. The registered production selection is
`derived/issue2177/pre-voyage-battle-v1`; the private observations below cover its real consumers.

### Derive selected battle rasters

The same producer accepts `--derive --gfx-root` with one scene path per `--scene`. It reads
the mounted Original rasters, generates temporary PNGs and passes them through the existing
variant binder and closed-package staging. The pinned graphics checkout supplies the exact-RGBA
edge-aware filter used by the actor producer.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2176/pre-voyage-actors-v1'
# $gfxRoot selects the pinned graphics checkout; $candidate is fresh ignored scratch.
$scenes = @('main-scenes.json',
    'inputs/scene02-field-01/shrine-mounted-scenes.json',
    'inputs/scene03-field-03/shrine-mounted-scenes.json',
    'inputs/scene04/shrine-mounted-scenes.json', 'inputs/scene05/shrine-mounted-scenes.json',
    'inputs/scene06-cast/shrine-mounted-scenes.json', 'inputs/scene07-cast/shrine-mounted-scenes.json',
    'inputs/scene08/shrine-mounted-scenes.json')
$sceneArgs = foreach ($scene in $scenes) { '--scene'; $scene }
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_hd_battle_rasters `
    --base $base --out $candidate --derive --gfx-root $gfxRoot --shared-allies @sceneArgs
```

Use only encounter paths declared by the selected world's `encounters[].battleScenes`.
Omit `--pilot` for full conversion of the selected scenes. Pilot selects the first complete
body group, weapon group, background, ground, healing group, field-death group and each
installed BLAZE1/BLAZE2/BLAST1 group. Overlapping groups close together. Full selection includes
all actor frames and weapon frames, default and terrain-selected scenery, healing body/wing/dust,
field exits and those three installed formal spell families. UI keys remain outside this closure.

Use `--shared-allies` for a campaign package. `CampaignScenes` can supply an earlier battle's
member-specific appearance from a later declared scene. The flag additionally derives only
ally body and weapon groups in every remaining declared render scene. This deliberately
includes the installed shared ally class/equipment superset, so revisits and member appearances
follow the same existing campaign admission. Later enemy, backdrop and spell groups receive
no additional coverage from the flag. Receipts label each processed file `selected-scene` or
`shared-allies`. Pilot limits each donor to its first complete allied body and weapon groups;
full coverage requires omitting `--pilot`.

The producer also derives an existing raster matching a shared ally key when that scene's local
actor rows omit the key. The runtime retains that existing Original when importing a donor
appearance. Its HD therefore follows its own dimensions and pixels. The shared key set is
collected sequentially from the same declared scene documents during preparation.

The reader shares a variant only when it inserts that variant's Original from the same donor.
The existing destination-first, then first-donor Original selection remains authoritative.
A same-key Original already in the destination retains that destination's own variant or its
Original fallback, even when another scene declares an HD value for the key. Runtime sharing
uses the same immutable raster references. Source fields and the 4 MiB-per-render-scene bound
remain part of package admission; the opening definition keeps its separate admission bound.

Target dimensions follow the existing displayed footprint at four pixels per logical pixel.
Native 96×96 ally frames, native spell effects and field-exit strips receive source×4. Other
body frames, weapons, scenery and the installed doubled fairy art receive source×2. For a
doubled source, every pixel must match a nearest-downsample/upsample round trip before logical
pixels are recovered. Each reported key names its chosen method as `native-exact-rgba`,
`recovered-nearest-2x` or `conservative-source-grid-2x`. The conservative method filters at
the supplied pixel grid and takes the 2× result, preserving detail that prevents exact recovery.
All methods verify alpha and opaque-black masks against the original. Field-exit halves filter
independently.

Scenery uses the conservative exact-RGBA family treatment. These independently composed horizon,
ground and scrolling layers keep their existing palette and opaque-black contrast; they do not
share the terrain producer's block neighborhood or density-ramp contract. Horizontal repeats
provide the background filter's sampling neighbors before the center is cropped. This avoids
introducing a second palette direction while preserving the accepted contour treatment. Direct
production comparison confirms that output colors remain in each Original palette and masks
remain exact. This slice does not claim L1 palette expansion for battle artwork.

Processing is sequential per scene and per raster. Its work is linear in selected source and
output pixels plus copied package bytes. Memory includes one parsed scene, its encoded variants
and the current filtering grids and images. Each scene retains the 4 MiB JSON admission limit
and each raster the 2048-pixel edge bound. Input/output size receipts support the caller's
storage and admission planning. Real selected counts, peak memory and scene headroom must be
measured in the private pilot before scaling.

**Confirmed** with generated color patterns and the pinned filter. A native ally, exact doubled
frame, non-reversible doubled frame, doubled fairy/scenery/weapon, two-half death strip and all
three formal spell-key groups exercise the full selector. Pilot generated 34 variants in a
33,419-byte nine-file package; full generated 35 in 33,980 bytes. The sample completed in
approximately two seconds, including two CLI subprocesses. Every output mask matched, the
Original raster objects remained equal, and the UI key received no variant.

**Confirmed** with a synthetic donor scene. The shared-allies pass adds exactly its two allied
body frames and one weapon, while its enemy, scenery, healing, exit and spell resources retain
their Original fields without new variants. The two-scene full sample contains ten files and
50,785 bytes. The focused `BattleSceneAllyAppearanceTests` exercise a missing destination
Original, a different existing Original without HD and a different existing Original with its
own HD. All three keep the selected appearance and paired Original/HD identity.
An additional synthetic CLI case retains a 96×96 destination Original and a different 24×24
donor Original under one shared body key; their produced HD widths remain 384 and 48 respectively.

### Published battle package

**Confirmed:** the Battle04 pilot produced 46 variants in 1.83 seconds; its scene was 954,082
bytes. The complete conversion with shared allied donors took 47.84 seconds on the review
machine. It adds 3,488 bindings across eight selected scene banks and eighteen shared-ally banks:
1,644 body, 1,580 weapon, 38 background, 38 ground, 72 healing, 24 field-exit, eight BLAZE1,
24 BLAZE2 and 60 BLAST1 bindings. Counts include repeated per-scene bindings. All 3,307 doubled
frames recovered exactly; 205 native frame operations include the separately processed exit
halves. No conservative source-grid fallback was needed in this selection.

The closed 168-file package is 357,304,188 bytes, an increase of 16,134,737 bytes. Its largest
scene is 3,224,271 bytes, leaving 970,033 bytes under the unchanged 4 MiB bound. The 142 other
files, including world, entry, audio and opening inputs, are byte-identical to the source package.
Removing `hdRasters` from each changed scene gives the identical original document. Every variant
was decoded and compared for dimensions, alpha, opaque black and palette containment. The world
remains 97,658,510 bytes. Total package and temporary work fit the existing storage allowance;
full producer peak working set and cross-hardware timing remain **Unknown**.
A repeated single-scene pilot measured 271,347,712 bytes (258.78 MiB) peak Windows working set
including package discovery, JSON parsing and filtering; it completed in 1.12 seconds. This
supports the 1 GiB offline planning allowance, without asserting a measured full-run peak.

Publish with the existing closed-package copier, using the command above's `$candidate` and
`$output = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2177/pre-voyage-battle-v1'`.
Use the same four copier input arguments shown for [actors](#pre-voyage-actors-and-portraits-2176),
then `--check` for an existing publication. Both copies contain the same relative dependencies.
After making all producer package directories unavailable, the published selection independently
passed BLAST1/death and HEAL/Settings observations. No producer scratch is a runtime dependency.
The [native reproduction](hd-rollout.md#private-battle-artwork-acceptance) names the exact short inputs.


## Pre-voyage UI rasters (#2178)

The `runtime/retail-v1` prepared `icons` and `ui` indexes resolve the semantic IDs, palette and
normal/selected source slots. [Original mounting](presentation-assets.md#bounded-source-to-consumer-inventory)
owns the exact joins. The UI producer uses the already admitted Original payloads from
`derived/issue2177/pre-voyage-battle-v1`, including each scene's own selected raster. That
closed package supplies every input raster. All new banks use the same Original resource names.

The world admits 204 identity states (including the mounted Unarmed pair) and four eight-state
command families. The eight pre-voyage render documents each admit 211 UI raster bindings. The
other 18 render scenes remain outside this production selection. The bounded producer uses the
pinned graphics filter at the native grid, preserves source alpha and opaque-black masks and
nearest doubles the result to four pixels per source pixel. The single-cell cursor uses nearest
4× to retain the tiny marks and overhang. Each family uses one treatment for both states. The
existing two world herb variants are carried byte for byte from the accepted base.

From the owning worktree, set `$gfx` to the clean graphics checkout at
`172662709b31cb296b55f51894754ce810c30f6c` and run:

```powershell
. ./local/private-inputs.ps1
uv sync --locked
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2177/pre-voyage-battle-v1'
$candidate = Join-Path $PWD 'local/issue2178/pre-voyage-ui-v1'
$scenes = @('main-scenes.json',
    'inputs/scene02-field-01/shrine-mounted-scenes.json',
    'inputs/scene03-field-03/shrine-mounted-scenes.json',
    'inputs/scene04/shrine-mounted-scenes.json', 'inputs/scene05/shrine-mounted-scenes.json',
    'inputs/scene06-cast/shrine-mounted-scenes.json',
    'inputs/scene07-cast/shrine-mounted-scenes.json', 'inputs/scene08/shrine-mounted-scenes.json')
$sceneArgs = foreach ($scene in $scenes) { '--scene'; $scene }
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_hd_identity_icons `
    --base $base --gfx-root $gfx @sceneArgs --out-dir $candidate
```

Production is serial: one parsed world and scene at a time, with shared source envelopes reused
across the selected banks. Work scales with unique pixels processed and bytes copied. Use the
existing 512 MiB staged-output allowance, a fresh worktree-local directory and 1 GiB of disk
headroom after staging. Existing limits are 120 MiB planned world output (128 MiB admission) and
4 MiB per scene. Decode residency for each complete bank is bounded by its dimensions, about
5.91 MiB for the world and 5.17 MiB for one tactical bank before driver allocation. Separate
views own their textures; these figures are per bank.

**Confirmed:** the main-scene pilot added 440,772 bytes. Full production took approximately
2.55 seconds and emitted a 359,189,445-byte closure, including a 97,892,927-byte world. The largest
selected scene was 1,891,742 bytes and the largest scene in the full closure remained 3,224,271
bytes. The original selects and previously installed variants are equal after removing the
1,922 new declarations. All 159 other files are byte-identical to the base. Direct decoding
confirmed dimensions, exact alpha and opaque-black masks, and source-palette containment for
each new declaration. Peak offline working set and GPU residency remain **Unknown**.

Publish through the [closed-package copier](presentation-assets.md#reuse-a-closed-private-package),
selecting `$candidate` as the source of all four inputs and
`derived/issue2178/pre-voyage-ui-v1` as the fresh shared destination. Verify with `--check`.
Use the published world, scene, entry and six opening inputs together for the
[consumer observation](hd-rollout.md#ui-raster-consumers-2178).

## Pre-voyage entry layers (#2179)

The six prepared `runtime/retail-v1` entry layers are consumed through the accepted #2178
package. `sf2tool.remake_hd_entry` uses the same pinned graphics filter and actor frame
conversion, preserving exact source alpha, opaque black and palette treatment. Each Original
256×224 image has a 1024×896 sibling named `<name>.hd.png`. The title-only `--pilot` produces
a complete package with a partial alternate group for fallback observation.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2178/pre-voyage-ui-v1'
$candidate = Join-Path $PWD 'local/issue2179/candidate'
# $gfx selects the clean graphics checkout at 172662709b31cb296b55f51894754ce810c30f6c.
uv run --locked --with pillow==11.3.0 python -B -m sf2tool.remake_hd_entry `
    --base $base --out $candidate --gfx-root $gfx
```

Choose fresh output paths for pilot and full conversion. The pilot contains 169 files and
359,212,273 bytes. The full package contains 174 files and 359,340,404 bytes, an increase of
150,959 bytes over #2178. Publish with the existing `sf2tool.remake_private_package` command,
selecting candidate `world.json`, `main-scenes.json`, `entry/entry.json` and `opening-inputs`.
The registered destination is `derived/issue2179/pre-voyage-entry-v1`; its `--check` comparison
passed. The publisher includes every declared `hdResources[].file` sibling in the closure.
[Actual entry acceptance and limits](hd-rollout.md#entry-layers-2179).

## Pre-voyage animated terrain (#2180)

**Confirmed** for the published `derived/issue2180/pre-voyage-tiles-v1` selection. The
private asset Git at `206f42802ccd06ddb880bcd453069f0c6284c8a6` supplies
`runtime/retail-v1/maps/index.json`, palettes and five indexed tilesets. The pinned source
owner is SF2DISASM `disasm/code/common/maps/animations.asm` and
`disasm/data/maps/entries.asm` with the selected `9-animations.asm` includes. Map4 aliases
Map3's table. Each selected entry transfers the entire listed range. First and second cache
offsets are zero and the tile count respectively. Logical counters are 20 and 20.

| Map | Prepared tileset | First target / tile count | Changed original centers in entry order | Additional distinct HD edge rasters in entry order |
| --- | ---: | ---: | ---: | ---: |
| 3 | 74 | 768 / 32 | 27, 37 | 59, 77 |
| 4 | 74 | 768 / 32 | 0, 24 | 0, 47 |
| 14 | 13 | 480 / 16 | 0, 24 | 0, 31 |
| 16 | 78 | 768 / 32 | 5, 20 | 5, 31 |
| 41 | 87 | 768 / 32 | 0, 9 | 0, 19 |
| 44 | 13 | 480 / 16 | 0, 16 | 0, 31 |
| 66 | 111 | 848 / 48 | 160, 0 | 375, 0 |
| 73 | 111 | 848 / 48 | 55, 0 | 122, 0 |

The other selected banks retain their static treatment. A zero above means the actual
replacement already matches the mounted source. Selected footprints include the southern
Map41 room and the detached Map73 plane. Block-bank coverage includes capacity outside
the player's path. The static atlas sampled at native density and prepared block words
matched the prepared library on all eight maps.

### Rebuild and select

Set `$maps` to the prepared retail maps directory and `$gfx` to the clean graphics
checkout at `172662709b31cb296b55f51894754ce810c30f6c`. From the owning worktree:

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2179/pre-voyage-entry-v1'
$producer = @('run','--locked','--with','pillow==11.3.0','python','-B','-m',
    'sf2tool.remake_pre_voyage_terrain','--base',$base,'--gfx-root',$gfx,'--animation-library',$maps)
& uv @producer --pilot 66 --out local/animation-pilot
if ($LASTEXITCODE) { throw 'Map66 pilot failed' }
& uv @producer --out local/animation-candidate
if ($LASTEXITCODE) { throw 'Terrain production failed' }
```

The pilot is a complete #2179 package with Map66 substitutions only. Select the candidate
using the [closed-package selection and copy commands](presentation-assets.md#reuse-a-closed-private-package).
After production, compare the #2179 and candidate file sets from `package_files`. Compare
all 173 non-world files directly. Remove only `tileAnimation` from the eight published
world rows and compare the parsed worlds. Both checks passed. The final world is
99,928,221 bytes, below the 134,217,728-byte limit. The complete set has 174 files and
361,375,698 bytes, an increase of 2,035,294 bytes. The measured single-process rebuild
took 6.88 seconds, with 392.32 MiB peak working set and 378.38 MiB peak committed.
Its output compared byte for byte with the registered publication. The Map66 pilot
table also equaled its final table. Runtime upload and driver residency remain **Unknown**.

### Native field and tactical replay

Use the [closed-package environment](presentation-assets.md#reuse-a-closed-private-package)
with the published package. Build the locked Debug adapter, set worktree-local APPDATA,
LOCALAPPDATA and TEMP, and select the authorized external disposable save root.
Set `SF2_SAVE_NAMESPACE=issue2180-<fresh-run>`, `SF2_HOST_UI_CASE` to the case below,
`SF2_HOST_UI_START` to an absolute worktree-local JSON and `SF2_HOST_UI_OUTPUT` to
a fresh ignored JSON. Each start wraps the following fields in a `start` object.

- `water` uses the #2035 `egress-writer.json` flags and names, `map-66`, player
  `entity-0`, position (33,47), facing 1 and speed 32. The probe samples (33,49)
  and (33,50) at 20-tick intervals, pauses for 43 frames, applies HD through Settings,
  enters the southern shrine and returns to the shore.
- `tactical-water` uses `{"start":{"battle":"battle-6","activeAllies":["ally-0"]}}`.
  The source crop starts at (21,2). The animated child draws the original tile regions
  at the board's normal scale and priority. Its parent retains the static draw commands.
- `invalid-tiles` uses the same battle start and an isolated copy of the package.
  Set the first Map66 phase's first replacement block ID to zero. Admission returns
  `map-animation-block`.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --rendering-method gl_compatibility `
    --fixed-fps 60 --resolution 1920x1080 --path remake/game --log-file $nativeLog `
    --script res://probes/engine_pre_voyage_terrain_observation.gd
```

**Confirmed** after moving every producer output aside. The field and tactical runs passed
43 and 17 checks. The existing Battle06 terrain case also passed under #2179 and #2180.
Independent comparison reconstructed the target tiles in the prepared Map66 atlas from
the indexed stream and map palette, then applied the block words and flips. For HD,
filter the current 3×3 window clipped to the source plane, crop its center and nearest-expand
48×48 to 96×96. All 24 native RGBA readbacks matched. The SHA-256 prefixes below identify
the two entry readbacks in table order.

| Draw | Original phase 0 / 1 | HD phase 0 / 1 |
| --- | --- | --- |
| Shore (33,49), block 30 | `a405ff8d7427cf08` / same | `e28faeb3149cab64` / `2151c01c3aa5af14` |
| Water (33,50), block 385 | `4e0e97e86075ac78` / `97ef1c6524da0fa3` | `8e12d883529e4158` / `0649becb2781913e` |
| Battle06 (21,2), block 50 | `7312c6c9f0e382d3` / `92b7977b8d76f07c` | `15124efea2d7209c` / `eaf435ba08f49c09` |

The retained failures were an absent optional `animations` key on static prepared rows,
a probe using a reserved GDScript name, an unadmitted field start and a comparison helper
using numeric JSON coordinates as array indices without integer conversion. The corrected
runs used the existing post-Battle06 party flags. Final positive observations reported
no adapter errors. The final field and tactical processes emitted ObjectDB shutdown warnings (two and ten objects).
Natural visit-by-visit visibility, GPU memory and hardware scanline phase remain **Unknown**.

## Enemy healing family #2227

The complete private successor is `derived/issue2227/pre-voyage-healing-v1`, based on
`derived/issue2180/pre-voyage-tiles-v1`. Its 26 declared healing scenes add the same nine
Original and nine HD rasters. Existing entry, animated terrain, actor, UI, audio and gameplay
data compare unchanged. Scene JSON is compacted during publication. The package is 361,243,589
bytes, compared with the 361,375,698-byte base. The unique new encoded raster banks total about
12.5 KB. Publication is O(package bytes), with one scene decoded at a time. The pre-run budget
was 3 GiB peak memory and 1 GiB scratch; peak memory was not measured. A complete nine-frame
pilot passed the existing alpha/opaque-black mask checks before publication.

Prepared retail effects tileset 6 supplies 3,392 tile bytes with the registered ROM/upstream
identity and palette replacements `08880aaa0ace`. Source tile offsets, rank reuse and native
acceptance are in [the spell owner](../godot/spell-presentation.md#enemy-healing-family-2227).
The existing doubled Original format and family `derive_frame` HD recipe are reused. The
publisher closes its temporary directory before the real native consumer cold-loads the result.
Only private derived storage contains these rasters and their HD variants.

## DETOX family #2232

The complete successor is `derived/issue2232/pre-voyage-detox-v1`. It consumes the
`derived/issue2227/pre-voyage-healing-v1` closure and the registered retail tileset 7,
with 1,056 indexed bytes and palette replacements `08880aaa0ccc`. Zero-based mote
tile offsets and square tile widths are (27,1), (26,1), (22,2), (18,2), (9,3), and (0,3).
The five 1×1 trail frames start at tile 28. The producer uses the existing VDP column
layout, alpha and opaque-black mask checks, and the pinned graphics filter at
`172662709b31cb296b55f51894754ce810c30f6c`. Original frames are 8–24 pixels on a side.
The HD bank is four times denser.

The one-family pilot encoded 9,605 bytes for both banks. With 26 declared scenes the
expected growth was about 250 KB and the complete package about 362 MB. Plan for one
candidate and one publication, up to 1 GiB of disk headroom and 3 GiB of peak memory
on the prior producer estimate. Copying and comparison are linear in package bytes.
Only the scene receiving the group is rewritten at a time after dependency discovery.
Peak memory was not measured.

**Confirmed** by direct comparison of the actual private publication. Its 174-file
closure includes 148 byte-identical files and 26 scene JSONs. Removing the two DETOX
banks from each changed scene restores structural equality with the base. The final
package is 361,493,189 bytes and the largest scene is 3,188,370 bytes of the existing
4 MiB limit. The aggregate gain is 249,600 bytes. The [package recipe](presentation-assets.md#detox-successor)
and [native acceptance](../godot/spell-presentation.md#prepared-detox-presentation)
own reproduction and scope.


## DEBUFF1 family #2238

**Confirmed prepared and consumed.** USA identity and SF2DISASM c834c652 bind tileset 9's
1,536-byte stream to three 32×32 clouds, tile offsets 0/16/32. Setup overrides its default
palette slots 9/13/14 with source words 0888/0BBB/0DDD masked by VDP 0x0EEE. Update uses
frame order 0/1/2/1/0. The producer checks the prepared index identity, stream length and
Original/HD alpha masks, then uses the existing closed-package publisher. The accepted filter
pin is 172662709b31cb296b55f51894754ce810c30f6c.

The successor has 174 files and 361,637,333 bytes, including 26 mounted scenes. Its largest
scene is 3,193,914 bytes. The three-frame pilot serialized group is 5,614 bytes. Complete HD
and missing-frame fallback were observed through actual native sprites. The executable consumer
includes learned SLOW1/2, MUDDLE1/2, SLEEP1, DISPEL1, admitted enemy DISPEL, and
Giant Knuckles/Grand Cannon Use in the [family extension](../godot/spell-presentation.md#debuff-family-playback-2280).
SLOW2 variation2 uses the same gray group; no additional palette or asset package is required.
The current complete equipment-v2 successor retains these bytes and all equipment/HD/map/audio content.
[Publication](presentation-assets.md#dispel-successor) and
[scope, source, budgets and reproduction](../godot/spell-presentation.md#dispel-playback-implementation-2238)
own further details. Private media remains outside Git.

## Baron promotion #2240

The prepared Jaha/Randolf rows use walking sprites 10 and 41 and battle body 22 with
palettes 1 and 2. They reuse the actors’ existing portraits. The mount selects six walking
strips and ten body frames. The axe and sword layers reuse the mounted selectors shared
with GLDT and HERO, with four prepared Large Axe frames at 128×128 for the additional
selector. BRN attack/item select 22 and dodge selects 62. Both unarmed rows have no
weapon layer. Palette slots 1 and 2 match `$0EEE` and `$0000` for BRN, GLDT and HERO in
the registered ROM’s `pt_AllyBattlesprites` (0x18001C). [Battle sprite evidence](../../../docs/design/contracts/graphics/battle-sprite-graphics-data.md)

The single-member pilot added nine battle rasters and about 42 KB of compact scene
data. The complete successor includes 30 BRN rows and restores the ten Eric PGNT rows,
with 22 new battle rasters in total. The combined logical additions were 23,368 bytes
to a compact world encoding and 101,763 bytes to Battle26. The closure has 174 files
and 360,915,362 bytes. Only the world and Battle26 files differ from the DISPEL base.
The production reader admitted both palettes and every supported weapon binding in
all 26 scenes. Eric’s PGNT growth, actor rows and referenced rasters match the accepted
PGNT package. [Mount and catalog](presentation-assets.md#baron-promotion-successor) ·
[Ordinary observations](../application/church-promotion-design.md#baron-promotion)

### Brass Gunner promotion (#2242)

The prepared retail-v1 walking/class table binds Elric to map sprite32, battle11/palette0,
and Janet to map sprite47, battle11/palette2. The BRGN family reuses the accepted archer
and Rohde weapon rows for items45–50. Each member has six bindings and the shared unarmed
fallback. The source-based growth merge preserves SNIP alongside BRGN and preserves the
predecessor's PGNT, BRN and DISPEL selections.

The single-scene pilot took 0.05 seconds and added 12 actor records, eight unique rasters
and 35,424 compact scene bytes. Preparation is linear in package bytes plus selected
records. The pre-run serial budget was 1.5 GiB temporary disk, 2 GiB peak memory and one
roughly 345 MiB published package, with no downloads or model/API cost. The complete
successor contains 360,965,435 bytes, 50,073 above the 360,915,362-byte predecessor.
No unchanged duplicate package was generated. Scene closure directly compares every
predecessor actor/raster and retains all of them. Native production loading, both rites,
Elric's three physical range variants and cold Continue passed. Original animation timing
and Janet's BRGN battle action remain **Unknown**.
[Package and rebuild](presentation-assets.md#brass-gunner-promotion-successor) ·
[Observed controls and failures](../application/church-promotion-design.md#brass-gunner-promotion)

### Master Monk promotion (#2244)

Pinned class/growth/appearance sources and the prepared retail library select Sarah map4,
battle19/palette1 and Karna map38, battle19/palette2. Each member has unarmed and item26–28
rows, with no independent weapon layer, plus ordinary cast and special animation83. The
existing preparation owner mounts these onto the accepted BRGN package without changing
previous class rows, rasters or growth. The production reader checks both members in every
scene. The mounted appearance set excludes Iron Knuckles29 and rare fists30–32.
The equipment filter does admit29; its missing appearance is tracked in the
[whole-game equipment plan](#whole-game-equipment-resource-plan-2268).

The serial pre-run budget was 1.5 GiB temporary disk, 2 GiB peak memory and one approximately
345 MiB successor; processing is O(package bytes + selected records), without downloads or
API/model cost. The single-member pilot took 0.028 seconds and added five rasters and
21,560 compact scene bytes, including an item29 row subsequently excluded by production
admission. The final complete successor has 174 files and 361,020,000 bytes, a 54,565-byte
increase over BRGN. Only world and Battle26 scene differ. It adds eight actor rows and ten
rasters; direct comparison preserves every predecessor actor/raster/growth row. The
[published mount](presentation-assets.md#master-monk-promotion-successor) supports both
rites and published-input cold Continue. [Observed actions, failed exits and Unknowns](../application/church-promotion-design.md#master-monk-promotion)
remain explicit; this does not establish original timing or distribution rights.

## Sorcerer promotion (#2266)

The registered complete successor `derived/issue2266/pre-voyage-sorc-v3` contains 174
files and 361,155,769 bytes, a 135,769-byte increase over the MMNK package. This includes
Kazin/Tyrin SORC walking/body variants, source body 15 palettes 0/1, cast poses and source
spell 29–32 normal/selected icons. Current mounted appearance choices are explicitly unarmed and
rods 85/86/87/88/89/90/92 for both SORC/WIZ actors and both VICR actors in all 26
loaded scenes. The production reader checks body, palette and separate weapon frames.
Guardian Staff (91) remains outside the admitted equipment rules because of its DEF effect.
The five accepted translated names live in `derived/issue2266/catalogs-v1/world.zh-CN.json`.

Preparation used one serial bounded pass; 1 GiB RAM and 1 GiB scratch/publication allowances
were sufficient for the initial pass. The corrective run retains v1 and rejected v2 alongside
v3; allow 2 GiB for these complete packages and local staging, with prepared immutable
dependencies reused and only changed JSON copied. Compact pilot receipts are 3,537,931 bytes before the final reader receipt.
No new ROM-derived animation capture was needed. The complete output was copied and
checked through dependency closure, with no producer-scratch dependency. After moving
producer scratch aside, the initial field/battle witnesses and corrective v3 battle Continue matched complete decoded
data. The production reader checks both actors across 26 encounter scenes and rejects
illegal saved books. Tyrin native playback and higher-rank spells remain Unknown.

Original DAO/APOLLO/NEPTUN/ATLAS effect composition and playback are **Unavailable** in
this successor. The native DAO1 witness uses the existing modern fallback, not an original
summon animation. [#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104)
retains the four-family gap. [Source, rule and native boundary](../application/church-promotion-design.md#sorcerer-promotion)
· [Producer and selections](presentation-assets.md#sorcerer-promotion-successor).

## Whole-game equipment resource plan (#2268)

The [equipment agreement](../domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268)
and [94-row reconciliation](campaign-items.md#whole-game-equipment-reconciliation) own the scope.
The resource plan uses the registered #2001 prepared retail library. The current complete
equipment successor is #2274 below, retaining the accepted SORC v3 selections, #2266 catalog
and separately selected #2252 opening/#2255 Witch. The equipment consumer/resource scope is
independently accepted.
Do not fall back to rejected v2 or require old producer scratch.

**Confirmed (prepared metadata and selected JSON).** The prepared weapon index contains84
item selectors (26–109),42 palettes and23 packed weapon files totaling188,416 bytes. Selector255
means no independent weapon raster, including fists/arrows; it does not imply a missing asset.
Taros110 is outside that item-selector index and retains its enemy-specific path; Iron Ball102
has no eligible player class. No acquisition or capture is needed merely to recover these
existing files. The pre-A selected Battle26 scene had42 distinct allied item IDs; raw row presence
is only a diagnostic, not full legal combination coverage. Guardian91 shares prepared selector16/
palette31 with Indra92 and needs its own admitted item key. Thieve's Dagger106 reuses
selector22/palette41. Iron Knuckles29 and Great Axe39 need explicit appearance coverage despite
already passing the equipment filter.

**Confirmed (source enumeration, not runtime admission).** The full source's30 ally starts plus
legal regular/special class destinations produce59 member/class pairs. Their weapon masks yield
557 legal member/class/weapon tuples, plus59 explicit unarmed tuples:616 appearance keys.
Including rings yields912 member/class/equipment tuples. Independently, the94 definitions yield
439 class/item pairs when ALL expands to32 and NONE to zero. These are joins over source
`allies`, `classes`, `items` and the accepted promotion mapping, not assumptions about which
members have joined the current campaign. The existing source party and class readers must
admit the necessary legal consumers; missing late class mappings cannot be hidden by limiting
this join to the20 pre-E mounted member identities. RBT/GLM/RDBN and late member records
are limited here to equipment eligibility, source-defined combat operands and controlled
consumer/appearance/save admission. Recruitment, chapter maps and full growth journeys remain
outside this work.

### Static-equipment successor (#2270)

`derived/issue2270/equipment-v1` is the complete slice-A successor to SORC v3:174 files,
365,717,916 bytes. It retains predecessor world, map/audio dependencies and class bindings;
independent #2252 opening/#2255 Witch and the #2266 catalog remain selected by the launcher.
The `static-equipment` family in the existing producer adds Guardian91, Dagger106 and Demon Rod101
using prepared selectors16/31,22/41 and19/38. Six rod member/class bodies and the THIF body reuse
existing original sequences; all26 production-reader scenes resolve the added keys. Guardian's
native attack explicitly projects the SORC body and weapon91 separately. Demon Rod art is reader-
validated; its native witness covers Equip/stats/save, not a completed attack scene.

The first three-key mount took1.10s and produced361,186,722 bytes. Production admission then
required complete equipment identity icons; the corrected package adds the existing normal/selected
icons to field and scene surfaces. The final size remains within the existing1GiB publication budget.
No new body capture, source acquisition, audio generation or per-item world copies are needed.
Peak memory was not measured; the existing2GiB planning allowance remains an estimate. The final
package is closed by `sf2tool.remake_private_package` and was consumed from its registered location.
The E slice still owns exhaustive legal class/item/body closure; this successor does not assert it.

```powershell
. ./local/private-inputs.ps1
& ./tools/Prepare-PrivateAssets.ps1 -Repo (Get-Location).Path -Operation Mount `
    -Family static-equipment -Package (Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2266/pre-voyage-sorc-v3') `
    -Destination $freshIgnoredPackage
```

The [slice A owner](../domain/equipment-use-repair-design.md#slice-a-reproduction-and-retained-failures)
records reader/native commands, retained failures and private receipt registration.

### Prowess successor (#2271)

`derived/issue2271/equipment-v1` is the complete successor to A:174 files,365,733,902 bytes,
a15,986-byte increase. Only Battle26 scene JSON differs; every predecessor actor and raster remains
identical and all other closure files compare byte-identical. It retains the same catalog, independent
opening/Witch, map/audio, growth and class selections. The existing `static-equipment` family adds
Counter Sword68 using selector6/palette12 and Ninja Katana108/Gisarme109 using selector8/palette17.
The five rows reuse HERO0, BRN3/16 and NINJ5 bodies; Counter Sword is not mounted onto NINJ.
The production reader verifies these five keys across all26 scenes through existing shared admission.

The single-scene pilot took0.164s, grew3,669,928→3,685,914 bytes and added5 actor rows/12 rasters.
The serial complete mount took under2s. The2GiB peak-memory/1GiB publication budgets were retained;
peak memory was not measured. Processing remains O(package bytes + selected records), with no
capture/download/API cost. Twelve encoded images include the two item-specific copies of the shared
Ninja selector; this bounded addition requires no new cache or manifest. No old package was removed.
Native Slade Equip/attack projects body26/palette0 and weapon109. Original special CUTOFF animation
is not mounted by this weapon-only addition. Full legal equipment/body closure remains E.

Reproduce with the preceding `Prepare-PrivateAssets.ps1 -Family static-equipment` command using
`derived/issue2270/equipment-v1` as `-Package` and a fresh ignored destination. The
[B owner](../domain/equipment-use-repair-design.md#b-reproduction-and-retained-failures) records
reader/native commands and retained failures. No real player save is an input or output.

### Recovery successor (#2272)

`derived/issue2272/equipment-v1` is the complete B successor:174 files,365,753,566 bytes,
a19,664-byte increase. Only `inputs/battle26/battle-scenes.json` changes; all other closure files
compare byte-identical. The existing `static-equipment` producer mounts Holy97 selector17/palette33
on VICR bodies and Mystery100 selector17/palette36 on WIZ/SORC/VICR bodies, reusing the registered
#2001 prepared weapon files and existing six body variants. Life124 has no independent weapon layer.
Eight added member/class/item rows resolve through the production reader in all26 scenes; this
retains A/B rows without claiming the exhaustive E closure. The unchanged #2266 catalog, independent
#2252 opening/#2255 Witch, maps and audio remain selected. No captures or downloads are needed.

The serial single-scene pilot took0.176s and added8 rows. Its pretty-printed scratch JSON grew
3,685,914→3,802,492 bytes; the final package serializer emits3,705,578 bytes for that scene.
Processing is O(package bytes + selected records), with one approximately349MiB successor and
small local probe/receipt files. The pre-run2GiB memory/2GiB scratch/1GiB publication allowances
remain sufficient by output size; peak memory was not measured. No per-item full package copies
or new caching/manifests were added. Native Stay and independent cold Continue consume the
registered successor directly, independent of producer scratch.

Reproduce with the preceding `Prepare-PrivateAssets.ps1 -Family static-equipment` command,
using `derived/issue2271/equipment-v1` as `-Package` and a fresh ignored destination. The
[C owner](../domain/equipment-use-repair-design.md#c-reproduction-and-retained-failures) records
reader/native commands, controlled custody, results and preserved failures. Full class/resource
closure remains E and future acquisition remains with the campaign owner.

### Use consumer reuse (#2273)

D adds no asset package, capture or downloaded source. It reuses the complete174-file,
365,753,566-byte `derived/issue2272/equipment-v1` package and independent #2266 catalog/#2252
opening/#2255 Witch selections. Its three ordinary-input witnesses use already mounted VICR,
BRGN and SORC bodies with existing equipped weapon layers; the newly used late item is bare.
White Ring AURA2, Grand Cannon DISPEL1 and Evil Ring BOLT2 use the existing Item gesture and
functional messages. This does not claim their original effect choreography (#2104) or create
late weapon/body keys. E still owns that complete class/resource closure. The production reader
checks the25 nonempty source Use bindings in all26 encounters, without adding learned spells.

Processing remains O(package bytes + source records); one shared read-only package and small
checkout-local scripts/seeds/receipts suffice. No full package copy is produced. The existing2GiB
peak/scratch and1GiB publication budgets remain planning limits, not measured peak memory.
The [D owner](../domain/equipment-use-repair-design.md#d-reproduction-and-retained-boundaries)
records exact reproduction, controlled custody, Repair/cold results and retained failures.

### Budget and pilot before expansion

The accepted package is about345MiB. Preparation should be O(input package bytes + selected
body/weapon records), with serial processing and one reusable final successor. No per-frame
runtime capture, downloaded executable, model/API generation, or per-item world copy is planned.
The23 packed weapon files are already present; raster expansion, JSON duplication and extra
body palettes dominate the uncertain increment. The59/616 source joins bound enumeration;
checking all26 scenes entails 16,016 expected appearance-key lookups, not16,016 native
runs. Reuse the established shared-scene admission mechanism instead of duplicating all rasters
per member or scene.

Provisional planning allowances:2GiB peak memory,2GiB worktree-local scratch/build outputs and
one final publication under1GiB, retaining the345MiB accepted input read-only. Expect less than
100MiB of new compact media/scene data if identical body/weapon rasters remain shared, but measure
before committing to that estimate. Raw188KiB weapon storage alone is not a final-package estimate.
Pilot CPU/runtime allowance is2minutes; the full serial mount/closure estimate is10minutes,
excluding a cold SDK restore and subsequent native observation. There is no network/model/API
cost for asset reuse. Implementation/review time is budgeted separately in the equipment agreement.

Use one representative scene for a mixed-stat rod91, a no-layer fist29, a late sword109 and one
missing late class body. Record input/output bytes, added actor/raster records, elapsed time,
peak process memory and actual production-reader result. A missing prepared body/sequence or
reader rejection must preserve its exact diagnostic and lead to a bounded dependency correction;
it is not grounds to omit that legal class. Do not generate all combinations until the pilot
proves the selected keys, layer identity and complete dependency closure. Replan through the
existing owner if peak memory exceeds2GiB, the estimated successor exceeds1GiB, media duplication
invalidates the increment estimate or a genuinely new capture/asset family is needed.

Each earlier mechanism slice may mount its few representative keys using existing producers and
retain a small changed-file overlay. The final slice produces the one complete reusable successor
via `sf2tool.remake_private_package`. Keep every predecessor map/audio/class/portrait/frame and
compare exact source-derived selections; do not rebuild unrelated assets. Validate the final world,
main scene, all26 encounter consumers, six opening inputs, catalog, independent opening and Witch
without any producer-local paths. Retain the closed package and receipts for independent review.

### Equipment closure pilot (#2274)

The four-key pilot reuses the registered #2001 library and C successor. Actual
`PrivateBattleContentReader` admission resolves Guardian91 for Kazin/SORC, Gisarme109 for
Slade/NINJ, Iron Knuckles29 for Karna/MMNK, and bare Zynk/RBT. Their body/palette/layer counts
are respectively15/0/4,26/0/4,19/2/0 and20/0/0. Compact scene size grows3,705,578 to3,756,563
bytes; four candidate rows replace two existing rows and add29 referenced rasters. The corrected
memory instrumentation measured57,577,472 bytes peak working set and0.037s. The first memory
sample returned zero because its native handle argument was not pointer-sized; it is not a valid
peak measurement. Pilot reader/build passed; receipts remain under the slice's ignored outputs.

Full expansion remains serial with one scene's shared complete appearance table, not one package
per key or duplicated rasters in26 scenes. Budget remains2GiB peak/scratch, publication below1GiB
and expected increment below100MiB. Prepared input is already local; no capture/download/API work
is needed. Measure the full single-scene output before the one closure publication, then measure
actual production admission and all26 shared consumers. Explicit source-class identity is needed
because Lemon's RDBN uses the BRN body; explicit bare records supersede legacy first-row fallback.

**Confirmed (corrected source enumeration).** The pinned starting-class and promotion
join and prepared walking index currently reproduce59 member/class pairs,557 weapon pairs and912
member/class/equipment pairs;94 definitions and439 class/item pairs agree with the earlier audit.
At pinned `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/data/stats/allies/allystartdefs.asm` has32 rows, while `disasm/sf2enums.asm` defines
`COMBATANT_ALLIES_NUMBER=30`. The final rows30/31 both start RDBN but are outside the30 playable
allies. Each contributes24 legal weapons,30 equipment combinations and one bare appearance.
Excluding precisely those rows explains61→59 member/class pairs,605→557 weapon tuples,
972→912 equipment tuples and666→616 appearance keys. No source-legal playable key is omitted
or invented to match the old estimate.

### Final reader and native acceptance

Use `OpeningNewGameReader`/`BattleSceneContentReader` and the existing consumer pattern to enumerate
actual class/item masks and member/class/item appearance keys. A raw file scan is insufficient:
member restrictions, class mapping and fallback selection can change which body is used. Require
explicit unarmed rows, legal weapon/body separation, source selector palettes, complete referenced
frames/sequences and correct ranged projectile families. Ring changes must not change weapon art;
fists and source no-layer classes must not gain a fabricated weapon overlay. Include legal variants
absent from current seeds and verify ineligible combinations reject.

Short native state/input/projection observations cover the genuinely new effect families and
representative body/layer changes, removal/destruction and fresh-process Continue. Ordinary current
shop acquisition and controlled late custody are reported separately. All functional checks remain
screenshot-free and use disposable save namespaces. Full original spell animation remains under
[#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104); an equipment Use result
must identify its actual functional/visual boundary rather than call a modern effect original.

### Complete equipment successor (#2274)

`derived/issue2274/equipment-v1` is the retained initial candidate (superseded by v2 below):174 files,368,276,281 bytes,
2,522,715 bytes above C. Only world.json and Battle26 scene JSON differ; all172 other predecessor
files compare byte-identical. Independent #2252 opening, #2255 Witch and #2266 Chinese catalog
remain selected. The existing private-package closure publishes this once; the production reader
and all native observations consume the registered package independently of producer scratch.

The full serial mount took5.915s and peaked at412,954,624 bytes working set. The initial full
memory query returned zero and is retained as failed instrumentation; the corrected Win32 call
specifies handle/pointer types and checks success. The completed scene, including retained HD aliases, is 6,197,105 bytes, below 8 MiB. The reader's former4MiB bound
was too small for the complete table and is now8MiB. There is still one shared actor/raster table,
not26 copies or616 packages. Scratch, including preserved pilot/failure candidates, remains below
the 2 GiB budget (1,125,716,387 bytes in the slice scratch at handoff); no source or previous publication is removed.

`equipment` in `remake_retail_assets` mounts prepared bodies, weapon palette selectors and exact
sequences; `remake_equipment_assets` joins original starts/masks and adds necessary source party
records. Existing field/HD/media selections are preserved. All260 distinct mounted weapon frames
compare pixel-for-pixel with their prepared4bpp selector/palette; newly mounted body PNGs compare
byte-for-byte. Production admission preserves27,144 predecessor HD body references. Its full table
includes all616 explicit keys, source animation indices/headers/frame/weapon operands and projectile
selectors in all26 scenes. The [E reproduction owner](../domain/equipment-use-repair-design.md#complete-equipment-consumers-2274)
records the commands, native boundaries, saved failures and independent review handoff.

The full production reader check peaked at 1,381,109,760 bytes (below 2 GiB), including
loading the predecessor for the HD preservation comparison.

### Equipment HD correction (v2)

**Confirmed.** Independent review found that v1 retained body HD but dropped selected weapon HD
after renaming its frame keys. The expanded production consumer fails on immutable v1; both v1
bundles and all completed failures remain available. The current complete package is
`derived/issue2274/equipment-v2`, with reproduction/consumer/native receipts in `review-v2`.
The producer inherits HD by accepted item/frame, checks duplicate donors agree and never uses a
different item's shared selector or palette. The116 unique accepted item/frame donors include
aliases of older resource keys. Their original raster visible pixels match after128→64 scaling;
only transparent RGB differs. Actual production selection preserves the dimensions, format and
bytes of27,144 body and17,160 weapon HD references across26 scenes. All260 original weapon
frames still compare pixel-for-pixel with prepared source data.

V2 has174 files /368,469,461 bytes:193,180 bytes above v1 and2,715,895 above C. Only world and
Battle26 scene JSON differ from C; the other172 files remain byte-identical. The mount took6.569s
with412,745,728-byte peak working set; complete published reader comparison peaked at1,387,896,832
bytes. These remain within the2GiB memory and<1GiB full-package limits. The correction uses one
isolated local candidate and one immutable publication; previous review scratch is untouched.
The `redbaron-hd` native receipt confirms selected Great Sword64 frames0–2 use256px HD textures
through ordinary input, then settle/save and exit0 (four retained #1716 warnings). Original late
rule/native-family evidence remains valid; no full campaign or per-key native rerun was made.

## BUFF no-tile mechanism #2282

BUFF1 loads no dedicated tiles. The [source/consumer owner](../godot/spell-presentation.md#buff-family-playback-2282) records selectors8/40, green cast flash and14 accelerating PSH cues. The complete `derived/issue2282/buff-v1` successor retains equipment-v2's rasters and adds prepared sound97 only:175 files,368,493,196 bytes. [Reproduction](presentation-assets.md#buff-successor).


### Flame family #2293

The complete [flame successor](presentation-assets.md#flame-successor-2293) uses the already
registered #2001 tileset2 (7,104 bytes) and base palette, with its existing022e/028e/08ee
replacements. Pinned source operands and zero-based tile/size mappings are in the
[flame mechanism owner](../godot/spell-presentation.md#source-composition-and-delivery).
The resource groups are `blaze-1` (two frames), `blaze-2` (six), `blaze-large` (six shared by
v3/v4) and `blaze-dragon` (four resources composing eight sprites). These are18 unique rasters,
with4× HD where missing; accepted predecessor variants are preserved. v4 selects the union
of large and dragon groups atomically, so one missing HD part falls back to Original for all10.

All26 current scene bindings pass the production reader. The complete175-file closure is
369,705,232 bytes, maximum scene6,439,791 bytes. Existing maps, body/weapon variants, previous
spell families, audio and opening inputs compare unchanged. Native bounded witnesses verify
actual textures, draw extents, actor frames and return separately from these static checks.
Natural acquisition, every encounter's playback and exact original timing remain Unknown.

### FREEZE family #2298

The [complete successor](presentation-assets.md#freeze-successor-2298) consumes prepared
spell tileset11 (2,752 bytes) with palette replacements0e88/0eca/0eee. `freeze/frame-0..13`
contains six growth shapes, their second brightness bank and two v4 glints. All14 form one
atomic Original/HD group, with4x HD at unchanged logical extents. Source tables and actual
side/rank composition are in the [FREEZE owner](../godot/spell-presentation.md#freeze-source-composition).
Prepared120 is23,586 stereo samples at44,100Hz,94,388 bytes; the existing PSG tone3 replacement
policy plays it with the modern cadence. All26 scene mounts retain previous groups, actors,
weapons, maps and audio. Resource admission, native texture/input witnesses and natural reach
are distinct claims; exact original timing and human aesthetics remain Unknown.

### Lightning family #2301

The [complete lightning successor](presentation-assets.md#lightning-successor-2301) reuses
prepared tileset4 (5,728 bytes), palette0ec8/0ca0/0e40 and the two440-byte BoltAnimData
operand tables. `bolt/frame-0..21` are64x96 source composites; `bolt/frame-22..29` are
32x32 orb orientations. All30 form one atomic Original/4x HD group at unchanged logical
extents. Existing scenery, actors, weapons, earlier spells, audio and independent entry
inputs remain intact. [Source and delivery](../godot/spell-presentation.md#lightning-source-and-modern-delivery)
separate original composition from modern cadence and Unknown natural reach/hardware timing.
