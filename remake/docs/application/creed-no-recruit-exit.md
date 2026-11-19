# Can the party leave Creed without choosing a companion?

**Confirmed:** For the settled post-consultation F784 room with four living, unjoined candidates
in their initial positions, the original walking rules leave Bowie on the north side of the hall.
Goliath and the four candidates occupy every crossing. The Fairy can follow and the Oddler caller
has no F787 test. The player still needs a way to reach its tile. **Inferred:** The existing remake
implements this physical restriction for the retained Fairy saves. **Unknown:** independent
ordinary observations on the current remake head and natural timing on original hardware.

[Issue #1940](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1940) uses Standard
rigor for this one route, under the [campaign agreement](remaining-campaign-risk-baseline.md).
The comparison begins at ordinary Map33 return after consultation and covers the hall
crossings, the south exit where reachable, and the next cold Continue. The [chapter](creed-recruitment.md) owns the already
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
their own actor/layout premises.

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
checks flag and persistence behavior. The real evidence gap is the player's approach.

Use the #1876 handoff's ordinary English slots as read-only seeds in separate disposable
namespaces. Use the current accepted adapter and one matching stable world, with ordinary
headless input and settled state/projection/error reads. The following observations are pending.

1. Continue a copy of `fairy-en.json`. Tap Down four times, Left, Down, settling after each move.
   At (16,14), tap Left, Down, Right separately. Record the player's origin/destination and each
   occupant's physical slot, obstruction bit and current/destination. Then tap Up, Right twice,
   Down to (18,14). Tap Down and Right separately. This sequence targets every crossing with ten
   predicted moves and five predicted collisions. On the static prediction both pockets retain control, the five
   attempts keep their origins, and the initial flag/membership/following state survives.
   Save there, use product Quit and cold Continue. A changed outcome calls for tracing the first
   differing input, tile or actor before changing production code.
2. For a positive control, Continue a separate copy of `recruited-en.json`. Eric is already
   joined and following, and Bowie is at (15,13) Down. Eight settled Down inputs along X15
   target the zone, including the door at Y20. Release direction when the zone starts, advance
   its dialogue and read F786/F69 set, F68 clear and F14/F787 retained. Read the returned player
   position (the scene requests (15,23) Up), then walk Down to Y31. Save on Map67 (26,23) Up,
   Quit, cold Continue and check ordinary control. If a no-choice probe actually crosses the
   barrier, continue that branch through the same caller, exit and cold check with F14–17/F787
   still clear. Keep the old failed search receipt.

Each run reuses the existing roughly 100 MB world. Prior #1876 no-choice, chosen and cold receipt
directories used about 170, 518 and 91 KiB respectively. Allow 5 MiB for the two branches'
compact receipts and slot copies, separate from local host build/import output. No new world
or media acquisition is needed. Stage one selects no production change. The chapter, capability
row and campaign character/location/Creed-flag entries await the ordinary result and serialized
ownership with #1939. **Unknown:** native behavior on the new head, other original histories,
and any timing-dependent original bypass outside the settled walking premises.
