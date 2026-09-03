# Map 3 Original Player Locomotion and Animation

## Scope

This owner closes the bounded original-runtime facts needed by a Map 3 player-animation consumer.
It replays one controlled original Witch/New admission through the first original `WaitForEvent`,
saves that admitted state outside emulator callbacks, and replays four directional attempts from the
same state in one BizHawk launch. The tracked fixture is
`sf2-map3-original-player-locomotion-animation-runtime-v1`.

This is not a natural route to Map 3. The admission chain is retained from
`sf2-map3-admitted-start-runtime-v1`; the movement arithmetic is retained from
`sf2-entity-movement-runtime-v1`; and facing-to-source-slot/mirror identity is retained from
`sf2-map3-original-player-reference-frame-static-v1` and `sf2-map-sprite-decode-v1`. This owner does
not redefine those contracts.

## Controlled admission seed

**Confirmed:** at the first original `WaitForEvent` entry, the controlled entity is settled at
`x=21504`, `y=1152`, facing `DOWN` (`3`), with position equal to destination, zero travel and zero
velocity. Its stored animation counter is `26`. The immediately preceding original
`VInt_UpdateSprites` execution selected half `1` from counter `25` and then advanced the stored
counter to `26`.

That observed half is specific to this controlled admission boundary. It replaces neither a general
idle rule nor a universal standing-frame claim.

## Facing and source selection

The runtime records observe all four accepted facings. Their graphics identity is a static join to the
accepted reference-frame owner:

| Direction | Facing | Source slot | Horizontal mirror |
| --- | ---: | ---: | --- |
| `UP` | 1 | 0 | no |
| `LEFT` | 2 | 1 | no |
| `RIGHT` | 0 | 1 | yes |
| `DOWN` | 3 | 2 | no |

`sourceSlot` and `horizontalMirror` are not emulator-derived pixel observations. The verifier loads
the accepted static fixture and adds that exact join only after the Lua observer has returned its
runtime facts; accepted output is not passed into the observer configuration.

## Input, movement, and sprite order

**Confirmed:** from the common admitted seed, `UP` and `RIGHT` are blocked, while `LEFT` and `DOWN`
complete one 384-unit map tile. The observed order within an enabled VInt is:

1. `UpdateEntityData` applies any already-installed movement and its velocity-derived counter delta.
2. The controlled action script may consume input, change facing, and either install the next
   destination/travel/velocity or leave motion state unchanged when blocked.
3. `VInt_UpdateSprites` selects half `0` for counters below `15`, otherwise half `1`.
4. The sprite updater adds one to the counter and stores zero when the result exceeds `30`.

The first tick of every case begins with counter `26`. A blocked attempt changes facing before sprite
selection, does not change position, destination, travel, velocity, or the counter in the movement/
input phases, selects half `1` at counter `26`, and settles after the sprite updater stores `27`.
Blocked input therefore does not create a movement-derived counter delta, but the original enabled
VInt sprite cadence still advances once.

A successful first tick changes facing as needed and installs signed velocity `32`, travel `384`, and
a destination one tile away without changing position. It likewise selects half `1` at counter `26`
and stores `27`. The next twelve movement ticks advance position by 32 units. On each of those ticks,
`UpdateEntityData` adds one because `abs(xVelocity)+abs(yVelocity) >> 5` equals one, then
`VInt_UpdateSprites` adds one more. Arrival occurs on tick 13 with position equal to destination and
travel cleared; the signed velocity remains `-32` for `LEFT` and `32` for `DOWN`.

For both successful directions, the exact counters at sprite selection are
`26, 28, 30, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19`; the stored post-sprite values are
`27, 29, 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20`. The selected halves are therefore
`1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1`. Both successful moves settle after selecting half `1` and
storing counter `20`.

These facts prevent a consumer from deriving animation phase from an application simulation step or
from a last-traversal flag. The original counter has two distinct increments while moving and only the
sprite increment when stationary or blocked, and its visible-half selection precedes the sprite
increment.

## Evidence boundary

**Confirmed:** the source guard pins the original movement-before-input-before-sprite order and the
two counter updates. H1/ROM guards uniquely bind `UpdateEntityData` entry/return and the
`VInt_UpdateSprites` half-0, half-1, and post-counter seams. The observer records every complete
enabled VInt from the first movement-ready tick through settlement, including before-movement state,
after-movement state, an optional input attempt, and the exact sprite branch/counter result.

The observer uses one deterministic callback per physical PC. Saves and replay loads occur outside
callbacks. Success and failure both restore the bootstrap state, controlled entity, map/battle span,
generated RAM, and session-only cartridge patches, clear all callbacks, and propagate a typed failure
through the status/exit contract. The disposable session ROM is deleted by the Python verifier.

**Project policy:** the earlier static owner's `initial-reference-frame` half `0` remains a
private-import policy. It is not reclassified as an observed idle frame. A remake may translate the
confirmed counter/half state machine into engine-native timing, but must preserve the observable
ordering and selected-half sequence for this contract if it claims original compatibility.

**Unknown / out of scope:** rendered pixels, palette/color parity, screenshot or capture comparison,
DMA completion timing, camera behavior, NPC and follower animation, battle sprites, a general-purpose
animation engine, H4/8C presentation evidence, and a universal idle/standing half remain outside this
owner.

## Reproduction

With the accepted private inputs installed in the owning worktree:

```powershell
uv run sf2 h3 map3-original-player-locomotion-animation
```

The command performs one BizHawk launch and compares the public-safe observation with
`tests/fixtures/h3/map3-original-player-locomotion-animation-runtime-v1.json`.
