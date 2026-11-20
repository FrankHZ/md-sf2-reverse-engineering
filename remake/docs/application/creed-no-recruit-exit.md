# Can the party leave Creed without choosing a companion?

**Confirmed:** For the settled post-consultation F784 room with four living, unjoined candidates
in their initial positions, the original walking rules leave Bowie on the north side of the hall.
Goliath and the four candidates occupy every crossing. The Fairy can follow and the Oddler caller
has no F787 test. Ordinary remake inputs from the saved Fairy room
met each of the five actors. From Eric's recruited save, eight south inputs reached the Oddler
scene and eight more left the mansion. Both branches cold-restored with usable control.
**Unknown:** natural original timing or a bypass outside this settled population.

[Issue #1940](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1940) uses Standard
rigor for this one route, under the [campaign agreement](remaining-campaign-risk-baseline.md).
The comparison begins at ordinary Map33 return after consultation and covers the hall
crossings, the south exit where reachable, and the next cold Continue. The [chapter](creed-recruitment.md) owns the
accepted Fairy, offer, Oddler, departure and persistence behavior.

## Original room and walking rules

The original inputs are the registered USA retail ROM and SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`. The [return and offers](../../../docs/research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md),
[Oddler scene](../../../docs/research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md)
and [departure](../../../docs/research/chapters/southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md)
own the source behavior. The additional reachability premises are as follows.

- Map34's explicit exit enters Map33 at (9,5) Right and rebuilds actors and layout. With F783/F784
  set and F786/F22 clear, Map33 uses `s1_entities_784.asm` and `s6_initfunction.asm`. Goliath128
  starts at (17,14), Creed129 at (17,5), and resident Fairy130 at (17,10). The init puts Eric14 at
  (15,14), Randolf16 at (16,15), Tyrin17 at (18,15) and Karna15 at (19,14), subject to the
  individual joined and placement HP checks. NewGame initializes ally records, including current HP,
  before Bowie's initial JoinForce. Being unjoined does not itself skip placement.
- The five actors across the hall run `eas_Init`, which enables obstruction and jumps to
  `eas_Idle`'s wait/branch loop. Goliath's F784 talk displays 1905–1906. An initial No ends with
  refusal dialogue. Neither action moves the speaker. The walking Goliath with origin (22,4) and
  range one belongs to the later fresh F786 population. An initial Yes joins one candidate,
  sets F787, shrinks the other three and makes the chosen ally follow.
- Resident130's Fairy interaction copies the current word (17,13) to (9,5), adds following and
  writes F762 then F785. Here the copy replaces the shrine-warp word 0x1032 with plain floor
  0x009B. Stepping back to that tile therefore stays in the room. Map33 has only that warp row
  and the south row at Y31. Before the copy a shrine round trip would enter again from the
  north. Map34's two internal CURRENT255 rows reuse Map34. They supply no entrance south of the
  actors. F22 is the only flag-based layout copy in Map33. The field Egress spell and Angel Wing
  use the overworld map range 66–78 in `mainactions.asm`.
- In normal field control, `applyfadingeffectandz80busupdate.asm:@ProcessInputs` resolves
  perpendicular D-pad directions before writing CURRENT_PLAYER_INPUT. The controlled character
  waits for its destination between steps. `entityscriptengine_2.asm:esc02_controlCharacter`
  checks obstructable actors at both current and reserved coordinates before a door, warp or
  zone can fire. Both absolute coordinate differences must be below 256 fixed-point units for
  either position to block. One tile is 384. The destination is checked for collision after
  door copying. At `loc_52E8` an admitted move decrements a positive STEP_COUNTER once. Waiting
  leaves these five fixed actors in place. ExplorationLoop's entry subtraction of 20000 with
  signed clamp to zero does not release them.

**Confirmed:** A four-neighbor flood fill from (17,9) reaches 86 cells, with maximum Y14.
The two furthest pockets are (16,14) and (18,14). From the west pocket, Left, Down and Right
meet Eric, Randolf and Goliath respectively. From the east pocket, Down and Right meet Tyrin
and Karna. Along Y12–13 the five-column passage has walls at X14 and X20. The upper component
contains no stair, layer transition, zone or remaining warp. The door marker at (17,11) permits entry into the passage. The western (11,9) table row
has no door marker in this layout. The doors at (12,13), (8,14) and (15,20) lie beyond the
component. As a topology check, making any one of the five hall cells vacant in the calculation opens a route to (15,21). Vacating only Creed's
northern cell does not. These are calculations on coordinates, never inputs to a running game.

To reproduce, run `uv run sf2 rom verify` with the checkout's private selections. Use
`sf2tool.h2.map_layouts.decode_map_blocks` on the ROM slice [0xAC748, 0xAD04C), then
`decode_map_layout` on [0xAD04C, 0xAD410) with the decoded block-word count divided by nine.
These are the Map33 entries in the pin's `split/sf2splits.txt`. Index the 4096 words by 64*y+x.
Apply the Fairy copy. Within the first area (0,0)–(24,31), start on layer zero, exclude the five
hall actors and Creed, and admit each cardinal destination with high bits other than 0xC000.
For marker 0x0400 use the matching source word in `map33/4-step-events.asm` before that test.
(17,11) changes from 0xC46F to 0x0006. The frontier above contains no live layer/stair transition,
so there is no additional reachable layer to expand. The zone word at (15,21) is 0x1419.

At that zone, clear F774 selects the earlier Goliath scene. Otherwise F785 set and F786 clear
run `cs_5B466`. Its normal return sets F786/F69 and clears F68. The south warp leads to
Map67 (26,23) Up. These statements describe the caller and destination under their admission
conditions. Histories with a prior join, changed HP, F22 or a different loaded room require
their own actor and layout premises.

## Remake evidence and ordinary observation

**Confirmed:** All 4096 base words in the #1876 world and the world selected by #1938 match
the decoded ROM. Both #1876 `slots/fairy-{en,zh-CN}.json` saves have Bowie at (17,9) Down,
step counter zero, F67/F68/F762/F774/F783/F784/F785 set, and F14–17/F22/F69/F786/F787 clear.
Their current entity list is [0,1,2,5]. Physical Fairy5 follows Oddler2. The five hall actors
are visible, settled and obstructable. Applying the actual saved layout changes gives the same
86 cells in both languages. The retained `en-no-recruit02` result records texts 1919/1903,
then `unrecruited-departure:no-working-layout-path` and native exit 2. The earlier-mansion
`no-recruit04` departures are separate successful histories.

[MapEventDispatcher.Move](../../src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)
checks the candidate against [EntityMotion.FieldObstructed](../../src/Sf2.Remake.Domain/Maps/EntityMotion.cs)
before dispatching a door or zone. [Cold field-scene restore](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2FieldSceneRestore.cs)
restores the saved actors, following and changed words. A current-room flag change alone does
not rebuild the actors. [CreedRecruitmentTests](../../tests/Sf2.Remake.Engine.Tests/CreedRecruitmentTests.cs)
uses a zero-word layout and an interaction with entity129 to drive the Oddler surrogate. It
checks flag and persistence behavior. The ordinary inputs below exercise the corresponding hall and zone.

### Ordinary results

The #1938 runtime carries the accepted execution code at 34050ee4. It read the #1927 Pacalon
package and separate copies of the two English #1876 slots. Each run used the existing Godot
4.7.2 headless host, ordinary key press/release, and title Continue. The observer read settled
player and actor coordinates, input readiness, logical view, presentation camera, dialogue,
party lists and error fields. At both pockets the room camera reported (120,225) and tracked
player slot 0. The headless camera actor projection was null.

| Seed and ordinary input | Settled observation |
| --- | --- |
| Fairy at (17,9) Down. Down four, Left, Down. | Bowie walked through the (17,11) door into the west pocket at (16,14). |
| Left, Down, Right, released separately at (16,14). | Bowie faced Eric14 (physical6), Randolf16 (7) and Goliath128 (3) in turn. Player position and destination stayed (16,14) on all three inputs. |
| Up, Right twice, Down to (18,14). Then Down, Right. | Bowie faced Tyrin17 (8) and then Karna15 (9). His current and destination cells stayed (18,14). All five blocking actors were visible with bit 0x80 set, and their current and reserved cells matched the source positions. |
| Save in the east pocket, Quit, cold Continue. | F762/F785/F68 stayed set, F786/F69/F787 and joined14–17 stayed clear. The entire decoded save data matched on immediate resave. Up reached (18,13). |
| Recruited Eric at (15,13) Down. Down eight, acknowledge the text when offered. | Bowie crossed Eric's former cell and opened the Y20 door. The last input targeted (15,21). Texts 1932–1945 played in order. Control returned at (15,23) Up, with F786/F69/F14/F787 set and F68 clear. |
| Down eight, Save on Devil's Tail, Quit, cold Continue. | The Y31 warp delivered Map67 (26,23) Up. The entire decoded save data matched on immediate resave. Down reached (26,24). |

The four completed samples were `hall03`, `chosen01`, `cold-hall01` and `cold-chosen01`.
Each had an empty observer issue, product Quit, native exit 0 and clear gameplay, presentation,
audio and fallback errors. Save sequences advanced 93→94 in the hall and 94→95 outside.
Shutdown logged two ObjectDB instances in three runs and four in the Oddler run. All four
processes exited without an access violation. [Issue #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
tracks an intermittent exit access violation after Chinese Continue. Its relationship to the
ObjectDB warning remains **Unknown**. Two earlier attempts failed during setup or observation. An in-checkout save root
was rejected at title in `hall01` (exit 2). In `hall02` the observer misspelled the case of the
`Joined` field and logged script errors despite exit 0. The corrected hall run consumed the
actual joined list. These failures and the older #1876 navigation failure are distinct.

After [Rick's recruitment](hassan-rick-recruitment.md) merged at 0604b5d1, the two stable worlds'
Map33 definitions and full program arrays compared equal. The historical roster reader and save
recipe changed. Sample `current01` used the owning checkout's Debug adapter, the accepted #1939
stable content and a copy of the revision-9 hall save. Title Continue, Save, Up to (18,13) and
menu Quit passed with native exit 0. Comparing the resave added exactly the initialized unjoined
ally12 record and changed recipe revision 9 to 10. The other nineteen records, room graph, layout,
flags and all other decoded fields matched. This process also logged two ObjectDB instances at shutdown and exited without an access violation.

### Replay

From the root of a [configured checkout](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup),
launch the #1938 stable player with a copy of one #1876 seed. At the title, choose Continue →
Slot 1 and follow the [ordinary inputs](#ordinary-results) for that seed. Press F10 → Save →
Slot 1, confirm, return to the game, then use F10 → Quit. For cold Continue, run
`& $env:GODOT_BIN @launch` again in the same PowerShell session. For the second branch,
change `$seedName` and run the whole block to create a fresh namespace.

```powershell
. ./local/private-inputs.ps1
$creedSlots = Read-Host 'Absolute path to the registered issue1876/slots directory'
$seedName = 'fairy-en.json' # Second branch uses recruited-en.json.
$bundle = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1938'
$config = Get-Content -Raw -LiteralPath "$bundle/player-config.json" | ConvertFrom-Json -AsHashtable
foreach ($name in $config.inputs.Keys) {
    $value = $config.inputs[$name]
    if (![IO.Path]::IsPathRooted($value)) { $value = Join-Path $bundle $value }
    [Environment]::SetEnvironmentVariable($name, $value, 'Process')
}
$run = 'creed-' + [guid]::NewGuid().ToString('N')
$profileDir = Join-Path (Resolve-Path ./local).Path $run
$env:APPDATA = "$profileDir/appdata"
$env:LOCALAPPDATA = "$profileDir/localappdata"
$env:TEMP = "$profileDir/temp"
$env:TMP = $env:TEMP
$env:SF2_SAVE_ROOT = Join-Path (Split-Path -Parent $env:SF2_SHARED_INPUT_ROOT) 'save-validation/issue1940'
$env:SF2_SAVE_NAMESPACE = $run
$slots = Join-Path $env:SF2_SAVE_ROOT "$run/sf2-opening"
foreach ($dir in @($slots,$env:APPDATA,$env:LOCALAPPDATA,$env:TEMP)) {
    [void][IO.Directory]::CreateDirectory($dir)
}
[IO.File]::Copy((Join-Path $creedSlots $seedName), "$slots/slot1.json", $false)
$launch = @('--path', "$bundle/runtime/remake/game", '--log-file', "$profileDir/native.log",
    '--', '--input-settings', "$bundle/settings-en.json")
& $env:GODOT_BIN @launch
```

The four primary runs used about 374 KiB for receipts and disposable save files. The post-Rick
check added 89 KiB, within the total 5 MiB estimate. Both worlds were reused in place. The original
hardware/controller observation, other populations and timing-dependent bypasses remain
**Unknown**. This investigation changes documentation only.
