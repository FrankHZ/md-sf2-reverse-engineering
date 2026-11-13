# From the Polca berth to the Kraken entrance

Continue the [Polca raft chapter](polca-first-raft.md) from its ordinary pre-boarding save. This chapter ends when the player's next water move selects the Map49 entrance and its Battle16 encounter. The current package displays a dismissible destination notice there. The party can turn back and sail on. The Kraken fight, its return to the river and the first visit to Hassan belong to the [parent chapter](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1796). High rigor applies because this handoff carries live party and vehicle state across the change of map and control.

## Play from the berth

In the prepared checkout, run `./local/issue1797/play.ps1 en` or `./local/issue1797/play.ps1 zh-CN`. Choose **Continue**, then **slot 1**. This is a review copy of the previous chapter's field save. Bowie starts on Map72 at (43,47), facing west. Tap one direction at a time and let each move finish. The table uses arrow keys, with the number of taps after the direction.

| From | Input | Arrival |
| --- | --- | --- |
| Polca bank (43,47) | ↓ to board, then ↓ | Map72 (43,49) |
| First water step (43,49) | ← ↓ ← ↓3 ← ↓9 | Map69 (41,16), facing down |
| South Parmecia (41,16) | ↓ ← ↓2 ← ↓ ← ↓5 →2 ↓2 → ↓17 | Map69 (41,44) |
| Lower river | ←2 ↓4 ←4 ↓2 ←4 ↓ ←3 | The last ← from (29,51) requests the Kraken entrance at (28,51). |
| Destination notice | Confirm, then → | Back at river input, Map69 (30,51) |

For a short return trip, use ↓ ↑ ↓ from the original bank. The first Down boards, Up parks the separate raft at (43,48) and lands Bowie on (43,47), then Down boards the same raft again. Both trips start with the same ordinary save.

## Observed in the remake

**Confirmed** — English and Chinese ordinary Continue both retained the eleven ally records and 9,753 gold throughout the river trip. F64, Luke's membership and F416 remained set. After boarding, Bowie had sprite 61 and mode 2. From (42,49), pressing Left toward the cliff at (41,49) left the raft stationary at player input. The next Down advanced normally.

**Confirmed** — The connected water route hit Map72 row 6 at (40,62). Map69 fresh entry placed slot 0 at (41,16) facing down. Slots 1 and 2 stood off-map and the new physical raft occupied slot 31 at (64,64) facing left. The recorded berth stayed on Map72 (43,48). Fresh exploration entry applied living/immortal ally recovery and the 20,000-step adjustment. This party's vitals were already at their recovered values. The seven fallen allies stayed dead. The counter changed from 26,807 to 6,807 and stayed there during water movement.

**Confirmed** — With the actual F416 set and F516 clear, the ordinary left input selected Map69 row 0, a no-scroll warp to Map49 (15,16), Up. The destination's source battle row selected `battle-16`. The current package marks this encounter unavailable before committing movement, entry effects or battle flags. Both languages displayed the destination notice at (29,51), and closing it returned control. To bind the fight, move 16 from `unavailableBattles` into the campaign's `battles` selection, provide the [Battle16 content](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1798) and its real script and outcome bindings, then regenerate the world. The existing warp remains its owner.

## Alternate entries and independent comparisons

- Map72 row 9 is a scrolling edge to Map69, with preserved X and destination Y=1. A four-direction search of the current working layout found no water-only approach from the berth without an earlier matched warp. A controlled engine case checks that a scrolling transfer keeps the current facing, physical actors, raft mode, berth, party, flags and step counter while bypassing destination battle admission. A supplied available encounter clears raft control before its before-battle program and retains the parked berth and party. The separate collision case changes the blocking actor's current and reserved positions, the active flag and the matching warp rows. [Raft travel tests](../../tests/Sf2.Remake.Engine.Tests/RaftTravelTests.cs)
- No-scroll entry uses ordered battle rows. On Map69, Battle9 has priority over Battle17 when both unlocks apply. The observed party had F409 and F417 clear. Battle17 on Map69 and Battle14 on Map70 retain their source predicates as unavailable encounters. The separate Battle15/Map50 branch lies outside this package. The river trip requires neither encounter. The later F65 Caravan follower population and vehicle door/copy markers remain outside this route.
- With F416 clear, Map49 has no eligible battle row. It has no fixed setup or init script. F516 also controls a working-layout copy on Map69 at (28,51), so a battle selection with supplied flags and a natural request from the river are distinct checks. The connected run covers the pre-victory layout. Original natural reach, hardware transition timing, and the completed-battle return remain **Unknown** in this run. [Original entry and flag contract](../../../docs/design/synthesis/chapters/southern-travel-creed/south-parmecia-through-kraken-battle16-entry.md)

## Reproduce

In the prepared checkout, run `./local/issue1797/run.ps1 -Lang en` or `./local/issue1797/run.ps1 -Lang zh-CN`. Add `-Shore` for the independent park/reboard trip. Each run reads the existing validation slot, submits ordinary keyboard input and writes observations into a fresh local directory. The private handoff in `local/issue1797/HANDOFF.md` names the preparation inputs and the retained failed runs. The engine comparisons run with the filter `FullyQualifiedName~RaftTravelTests`.

The import uses the registered USA ROM and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. Ordered warp rows and working-layout markers come from the map import. Encounter selection comes from `table_BattleMapCoordinates`. The fresh/scroll distinction and physical actor identities come from the [raft controller and southern exits](../../../docs/research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md). Parking and restoration follow [numeric type 5](../../../docs/research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#numeric-type5-tuple-save-disembark-and-restoration).
