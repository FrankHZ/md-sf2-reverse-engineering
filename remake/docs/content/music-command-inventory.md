# Music command inventory

[Asset inventory](asset-inventory.md) · [Sound source inventory](../../../docs/research/presentation/sound-data-inventory.md) · [Audio delivery](../godot/audio.md)

**Confirmed static.** The pinned USA sound fixture and retail coverage CSV identify 37
non-silent music commands and 27 silent slots. The identifiers below are decimal. `Music_n`
is the Z80 pointer target, while `MUSIC_*` is the source command enum. The
[sound inventory](../../../docs/research/presentation/sound-data-inventory.md#music-command-and-bank-selection)
owns the fixed revision, ROM parity and bank/alias interpretation.

## How to read the table

- **Base** is the reviewed inherited selection in `derived/issue2001/issue1966-original`
  world/entry. **Retail** is the original-driver candidate at
  `runtime/retail-v1/music/<decimal ID>/candidate.json` in private asset Git at
  `206f42802ccd06ddb880bcd453069f0c6284c8a6`. The preparation CSV points to one or the other.
  **Original PCM** describes the material's provenance. Playback, timing and human listening
  acceptance have their own boundaries.
- **Pre-voyage** and **PGNT** name the two [registered package selections](asset-inventory.md#registered-selections).
  Package cells describe the selected world and entry documents. All common world music rows
  in these two packages share the same PCM identities except command 12. Both entry documents
  carry 10 and 28 with the same identities. **Prepared only** means the candidate exists in the
  retail library and is absent from both selections. The table covers these named packages.
- **Loop** and **finite** describe the selected clip, or the prepared clip when unmounted.
  The hex value is its recorded Timer B. A finite clip's end is a resource boundary. Check
  the caller's [wait/history policy](../application/session-and-programs.md#modern-finite-music)
  before equating it with story progression.
- **Bound** identifies a configured default, compiled request or adapter binding.
  **Observed** refers to the named input or controlled playback witness. Unless the cited
  owner establishes more, natural use of each caller, loop/end behavior in that caller and
  human listening remain **Unknown**. [SessionAudio][adapter] owns map/battle selection
  and actual playback. [SessionEntryView][entry-code] owns entry requests.

## Non-silent commands

| ID | Pinned command and role | Prepared material | Selected resource | Consumer and checked boundary |
| --- | --- | --- | --- | --- |
| 1 | `MUSIC_MAIN_THEME` · main theme | Base original PCM | World · loop C2 | Area and story selections bound. [Field audio][audio] |
| 2 | `MUSIC_ATTACK` · ally attack | Base original PCM | World · loop C6 | Ordinary physical scene selects 2 for allies. Actual scene audio and a controlled loop witness. [Physical scenes][physical] · [Audio][audio] |
| 3 | `MUSIC_PROMOTED_ATTACK` · promoted attack | Retail original PCM | Prepared only · loop C9 | Original promoted attack variant. The current physical scene chooses 2/5 by side. [Physical scenes][physical] · [#1818][campaign] |
| 4 | `MUSIC_PROMOTED_ATTACK_LOOP` · promoted attack loop | Retail original PCM | World · loop C9 | Bounded native playback in the retail priority package. The ordinary physical scene's current side-based choice uses 2/5. [Retail first consumers](asset-inventory.md#first-consumers) · [Physical scenes][physical] |
| 5 | `MUSIC_ENEMY_ATTACK` · enemy attack | Base original PCM | World · loop CC | Ordinary physical scene selects 5 for enemies. Actual scene audio and a controlled loop witness. [Physical scenes][physical] · [Audio][audio] |
| 6 | `MUSIC_BOSS_ATTACK` · boss attack | Base original PCM | World · loop CC | Area/story selection bound, including the Battle07 outcome program. Source capture and bounded delivery under the Stage C owner. [Stage C audio][stage-c] |
| 7 | `MUSIC_ZEON_ATTACK` · Zeon attack | Retail original PCM | Prepared only · loop CA | Later scene admission and actual attack cue selection. [#1818][campaign] |
| 8 | `MUSIC_TOWN` · town | Base original PCM | World · loop CB | Field defaults including Map3. Observed native loop and ordinary JOIN replacement and restart. [Audio][audio] |
| 9 | `MUSIC_INTRO` · intro theme | Retail original PCM | World · loop C4 | Selected as area music for the southern shrine route. [Shrine selection and scope][south-shrine] |
| 10 | `MUSIC_WITCH` · witch theme | Base original PCM | World + entry · loop C1 | Observed title-to-Witch playback. Creed reuses the same PCM as area music. [Entry witness][entry] · [Creed][creed] |
| 11 | `MUSIC_SUSPEND` · suspend | Retail original PCM | Prepared only · loop C2 | Original suspend and remake Quit/Continue have their own service policies. [Battle Quit][quit] |
| 12 | `MUSIC_SAD_THEME_2` · sad theme 2 | Retail original PCM. Base has the defeat placeholder. | Pre-voyage world original · loop C4. PGNT world **placeholder** · loop C4. | Flag-506 Map3 init and ordinary defeat requests are compiled. Native playback, loop/previous and controlled invaded Map3 movement were observed with original PCM. House stairs continuity has a separate issue. [Sad theme](../godot/audio.md#invasion-map3-sad-theme) · [#2165][map-music] |
| 13 | `MUSIC_ELVEN_TOWN` · elven town | Base original PCM | World · loop CF | Area music bound. [Elven Village caller](../application/battle19-village.md) · [Audio][audio] |
| 14 | `MUSIC_MITULA` · Mitula theme | Retail original PCM | Prepared only · loop CF | Check the first admitted source caller under campaign admission. [#1818][campaign] |
| 15 | `MUSIC_SAD_THEME_3` · sad theme 3 | Retail original PCM | Prepared only · loop C0 | The compiled Map72 `return-4ff40` request follows the terminator. All three init branches enter at the terminator, matching the pinned source `rts` before the unused suspend scene. Bind and verify the clip when a later active caller needs it. [Roft source caller](../../../docs/research/chapters/moun-nazca-grans/roft-town-interactions-and-zynk-recruitment.md) · [#1818][campaign] |
| 16 | `MUSIC_SAD_THEME_1` · sad theme 1 | Base original PCM | World · loop D1 | Actual Hawel house/Kazin delivery observed. [Hawel][hawel] |
| 17 | `MUSIC_PIANO_THEME` · piano | Retail original PCM | Prepared only · loop C9 | Check the actual interaction on admission. [#1818][campaign] |
| 18 | `MUSIC_CORRUPTED_SAVE` · damaged save | Retail original PCM | Prepared only · finite CC | Original cue available for a future accepted save-recovery caller. Current entry recovery has its own bounded behavior. [Entry owner](../application/opening-session-lifecycle.md#entry-confirmation-responsiveness) |
| 19 | `MUSIC_JOIN` · recruitment | Base original PCM | World · finite C0 | Actual ordinary Map3 join, finite wait, previous start and acknowledgement observed. Logical JOIN endpoint and original driver phase have separate evidence. [Audio][audio] · [Driver endpoint](../../../docs/research/presentation/sound-data-inventory.md#join-first-channel-logical-end) |
| 20 | `MUSIC_SAD_JOIN` · sad recruitment | Base original PCM | World · finite CD | Hawel house/Kazin witness observed finite end before previous music. [Hawel][hawel] |
| 21 | `MUSIC_SAVE` · save | Retail original PCM | Prepared only · finite D1 | Audio for a source Save caller has separate acceptance from working save delivery. [Church and Save][save] |
| 22 | `MUSIC_CURE` · cure service | Retail original PCM | Prepared only · finite D4 | Check playback and return when the Cure caller admits source music. [Church and Save][save] |
| 23 | `MUSIC_UNUSED_REVIVE` · unused revive variant | Retail original PCM | Prepared only · finite C8 | The source enum labels it unused. A supported normal caller is **Unknown**. [Sound source][sound] |
| 24 | `MUSIC_REVIVE` · revive service | Retail original PCM | Prepared only · finite C8 | Check a church Raise caller's resource and return if source audio is admitted. [Church and Save][save] |
| 25 | `MUSIC_PROMOTION` · promotion | Retail original PCM | World · finite CE | `church-promoted` binds the finite cue. Actual promotion start, Finished and castle music restart observed. [Promotion][promotion] |
| 26 | `MUSIC_CURSED_ITEM` · cursed item | Retail original PCM | Prepared only · finite C3 | Cue admission belongs to the equipment caller and its accepted feedback. [Curse behavior](../domain/equipment-use-repair-design.md) |
| 27 | `MUSIC_ITEM` · item acquisition | Base original PCM | World · finite C4 | Cataloged finite cut and item-program request. Cannon/New Granseal owners retain the caller observations and input waits. [Cannon][cannon] · [New Granseal return](../application/new-granseal-return.md) |
| 28 | `MUSIC_TITLE` · title | Base original PCM | Entry · finite C2 | Ordinary title cue plays in the entry restore witness. [Entry witness][entry] |
| 33 | `MUSIC_BATTLE_THEME_3` · battlefield theme 3 | Base original PCM | World · loop C0 | Battle defaults bound, including Map3. [Scene music selection](../godot/audio.md#scene-music-context-observation) |
| 34 | `MUSIC_BATTLE_THEME_1` · battlefield theme 1 | Base original PCM | World · loop BD | Battle defaults and controlled native loop witness. [Audio][audio] |
| 35 | `MUSIC_SHRINE` · shrine | Base original PCM | World · loop BD | Shrine and story selections bound. [East Shrine](../application/new-granseal-east-shrine.md#content-and-verification) |
| 36 | `MUSIC_FINAL_BATTLE` · final battle | Retail original PCM | Prepared only · loop CD | Admit with the final encounter and verify its battlefield selection. [#1818][campaign] |
| 37 | `MUSIC_MITHRIL_DIGGERS` · mine | Base original PCM | World · loop C6 | Dwarven mine area binding and original-driver recurrence. [Cannon][cannon] |
| 38 | `MUSIC_CASTLE` · castle | Base original PCM | World · loop D2 | Field default, controlled loop and actual restart after native promotion. [Audio][audio] · [Promotion][promotion] |
| 39 | `MUSIC_HEADQUARTERS` · headquarters | Base original PCM | World · loop C3 | Map46 area and Caravan support request bound. [Tactical base](../application/tactical-base-design.md) · [Caravan program](../../src/Sf2.Remake.Application/Runtime/Exploration/CaravanPrograms.cs) |
| 40 | `MUSIC_MITULA_SHRINE` · Mitula Shrine | Retail original PCM | Prepared only · loop C5 | Admit with its location and check visit-specific selection. [#1818][campaign] |
| 41 | `MUSIC_ENDING` · ending | Retail original PCM | Prepared only · finite C6 | Admit with the ending and check its actual final transition. [#1818][campaign] |

## Silent slots and the modern encounter cue

| Decimal IDs | Pinned source meaning | Preparation and consumer |
| --- | --- | --- |
| 0 | `MUSIC_NOTHING` · ignored command outside the 64 pointer slots | No clip. `SessionAudio.Play(int)` skips 0. [Sound source][sound] · [Adapter][adapter] |
| 29, 30, 31, 32 | Four slots select silent bank-0 target `Music_32`. Slot 32 is named `MUSIC_STOP`. | Inapplicable in retail preparation. Both selected packages omit clips. The adapter's explicit SoundCut and SoundFade operations have separate delivery rules. [Sound source][sound] · [Fade and cut](../godot/audio.md#sound-fade-request-and-effect-lifetime) |
| 42–64 inclusive | All 23 slots select silent bank-1 target `Music_64` | Inapplicable in retail preparation. Both selected packages omit clips. [Sound source][sound] |
| 109 | `SFX_BOOST` · SFX command outside the music bank | Both worlds carry the accepted **modern** 240 ms encounter notification, finite C2. The base CSV's prepared row refers to the authored clip. Native entry reports actual start/Finished and pulse completion, including ReducedFlash. Original waveform coverage remains **Unknown**. [Encounter notification][encounter] |

Timer variants matter to **SFX** selection. Use the [sound source's 56-command inventory](../../../docs/research/presentation/sound-data-inventory.md#embedded-sfx-command-and-header-domain),
the retail and package manifests for actual variants and the
[finite selection policy](../godot/audio.md#explicit-finite-defaults) for admission and fallback.
Music commands in each admitted collection have one selected resource.

## Direct review

Use the source [fixture](../../../tests/fixtures/h2/sound-data-static-v1.json) for the two alias groups
and the pinned enums for names. In the private asset checkout, read the coverage at the pinned
asset revision. Match candidate rows by command and recorded timer, and check `asset.runtime`
for playback shape. Inspect registered packages read-only after loading the owning checkout's
[private inputs](../../../docs/operations/local-private-inputs.md#selections).

```powershell
# $assetRoot is the configured local asset Git checkout.
$assetRevision = '206f42802ccd06ddb880bcd453069f0c6284c8a6'
git -C $assetRoot show "${assetRevision}:manifests/retail-v1-coverage.csv" |
    ConvertFrom-Csv | Where-Object { $_.family -eq 'music' -or ($_.family -eq 'sfx' -and $_.id -eq '109') } |
    Format-Table id, source_symbol, retail_status, retail_selection
```

In each package compare `world.presentation.audio` in `world.json` with
`presentation.audio` in `entry/entry.json`. Read the `cue`, `command`, `timerB`,
`sampleFrames`, `loopBegin` and `loopEnd` fields. Check the world-relative runtime WAV
where the row selects external media. For 12/C4, Pre-voyage carries 2,747,385 stereo frames
at 44.1 kHz with loop [1,374,107, 2,747,385). PGNT carries 44,100 mono frames at 22.05 kHz
with a full-buffer loop. For 109/C2 both carry 5,292 mono frames at 22.05 kHz, with no loop.
Use the owning playback receipts for what the player actually heard or what native callbacks did.

[adapter]: ../../game/src/Audio/SessionAudio.cs
[entry-code]: ../../game/src/Entry/SessionEntryView.cs
[audio]: ../godot/audio.md#audio-boundary
[sound]: ../../../docs/research/presentation/sound-data-inventory.md
[physical]: ../godot/battle-scenes.md#physical-battle-scenes
[stage-c]: ../application/pre-voyage-campaign.md#stage-c-audio-reproduction
[south-shrine]: ../application/pre-voyage-optional-locations.md#rebuild-and-select-the-private-inputs
[entry]: ../verification/host.md#title-and-witch-entry-observation
[creed]: ../application/creed-restoration.md#reachable-interactions-and-remaining-boundary
[quit]: ../godot/presentation.md#battle-options-quit-1651
[save]: ../godot/presentation.md#church-and-shared-save-agreement-1519
[hawel]: ../application/pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment
[promotion]: ../application/ordinary-promotion-classes.md#pldn-ordinary-reproduction
[cannon]: ../application/dwarven-mine-cannon.md#acceptance-and-resources
[campaign]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818
[map-music]: https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2165
[encounter]: ../capability-status.md#yeel-guide-through-battle03-implementation-agreement
