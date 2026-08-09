# Portrait Window and State Contract

- **Confirmed original structure:** six bounded portrait/menu service identities, the complete
  56-slot portrait pointer corpus, 52 unique private payloads and four aliases, counted eye/mouth
  headers, palette and Stack-load boundaries, and the source-static selection/window/update order
  described below.
- **Inferred original behavior:** none promoted here. Upstream labels and comments do not establish
  player-facing intent, death semantics, or visible animation behavior.
- **Unknown original behavior:** caller admission and natural reachability, invalid portrait-index
  effects, malformed-header behavior, VInt/RNG/DMA cadence, input-repeat meaning, visible blinking or
  mouth timing, window-motion duration, palette/VRAM completion, final composition, and exact
  presentation across dialogue, menus, battles, and story scenes.
- Remake status: implementation-neutral Phase 3 private-import and state-handoff contract; no
  renderer, window toolkit, portrait animation policy, accessibility behavior, localization flow, or
  licensed portrait pack has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static seam from an original portrait selector through private portrait
data loading and bounded portrait/name-window state. It owns:

1. the six research-record identities and H1-bound entry addresses listed in the evidence audit;
2. the raw `GetCombatantPortrait` sign branch and bounded `GetAllyPortrait` class-remap order;
3. the 56 ordered pointer slots, 52 unique portrait payloads, four aliases, counted eye/mouth header
   shape, palettes, and 2,048-byte decoded payload size;
4. source-static open/close portrait-window state and callback registration/removal order;
5. source-static eye/mouth update gates, counters, RNG-call operands/post-adds, original/alternate tile
   selection, and mirror-state handoff;
6. source-static open/close name-window order and its raw current-HP zero/nonzero font branch;
7. a public H4 surface based on identities, structure, counts, aliases, hashes, and synthetic traces
   rather than original portrait bytes.

It does not own portrait selection by entity map-sprite property, dialogue commands, service-menu
state machines, raw window layouts, window-engine behavior, global VInt/DMA semantics, rendered
frames, text content, audio, persistence, localization, accessibility, or distributable assets.

The selected executable owners are:

- `sf2-common-menus-static-v1` in
  [`tests/fixtures/h2/common-menus-static-v1.json`](../../../tests/fixtures/h2/common-menus-static-v1.json),
  implemented by [`src/sf2tool/h2/menus.py`](../../../src/sf2tool/h2/menus.py);
- `sf2-portrait-graphics-decode-v1` in
  [`tests/fixtures/h2/portrait-graphics-decode-v1.json`](../../../tests/fixtures/h2/portrait-graphics-decode-v1.json),
  implemented by [`src/sf2tool/h2/portraits.py`](../../../src/sf2tool/h2/portraits.py).

The owning research prose is
[Common Menu Engines and Services](../../research/common-menus.md) and
[Technical Graphics and Decompression Services](../../research/technical-graphics.md).

## Pre-Contract Evidence Audit

Fresh reproduction passed on the evidence date:

```text
Inventory sf2-common-menus-static-v1
SHA256 9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6
Files 42
LayoutIncludedFiles 41
IndexedRecords 42
Status PASS

Contract sf2-portrait-graphics-decode-v1
SHA256 D691E2058673D2837A391AE771E5DEBE6F3F2F896222F819F3F414F33D4EEEB6
PortraitPointers 56
UniquePayloads 52
DecodedBytes 106496
Status PASS
```

The audit found exactly six currently unassociated research records:

| Record ID | Original symbol | ROM address | Selected executable owner |
| --- | --- | ---: | --- |
| `menus.ally-portrait` | `GetAllyPortrait` | 87,862 | `sf2-common-menus-static-v1` |
| `menus.combatant-portrait` | `GetCombatantPortrait` | 75,322 | `sf2-common-menus-static-v1` |
| `menus.name-under-portrait` | `OpenNameUnderPortraitWindow` | 92,590 | `sf2-common-menus-static-v1` |
| `menus.portrait-functions` | `ClosePortraitEyes` | 87,286 | `sf2-common-menus-static-v1` |
| `menus.load-portrait` | `LoadPortrait` | 87,594 | `sf2-portrait-graphics-decode-v1` |
| `menus.portrait-window` | `OpenPortraitWindow` | 72,518 | `sf2-common-menus-static-v1` |

Every candidate has at least one selected executable owner. Registration is deferred until
preliminary semantic acceptance.

The common-menus fixture is an aggregate inventory. This contract consumes only the five selected
function identities/addresses and its explicit portrait timing queue boundary. It does not consume
service state machines, prompt return rules, field-item dispatch, shop/church/Caravan/blacksmith
facts, icon/UI graphics, minimap/member/ending inventories, or the unbuilt alternate.

The detailed helper sequences below are **Confirmed static source review** at the pinned commit. They
are not presented as bounded runtime observations.

## Portrait Selector Boundary

### Combatant selector

**Confirmed static source review:** `GetCombatantPortrait` tests the low byte of incoming `d0` and
branches on its sign:

1. a negative byte calls the entity portrait/speech-SFX lookup, then copies returned `d1` into `d0`;
2. a nonnegative byte calls `GetAllyPortrait`;
3. both routes return through the same exit.

This is a raw sign-branch contract. It does not prove which combatant kinds or caller states supply
each route. The enemy/entity lookup is separately owned by
[Sprite-Dialogue Property Data](sprite-dialogue-property-data.md); this contract does not consume its
fixture or duplicate its table/lookup H4 surface.

### Ally/class remap

**Confirmed static source review:** `GetAllyPortrait` preserves `d1`, compares incoming `d0.b` with
symbol `COMBATANT_ALLIES_NUMBER`, and takes `bhi` directly to return when the unsigned byte is above
that source constant. Otherwise it calls `GetClass` and checks these five class identities in order:

| Class identity | Replacement portrait identity |
| --- | --- |
| `CLASS_HERO` | `PORTRAIT_BOWIE_PROMO` |
| `CLASS_PHNX` | `PORTRAIT_PETER_PROMO` |
| `CLASS_WFBR` | `PORTRAIT_GERHALT_PROMO` |
| `CLASS_NINJ` | `PORTRAIT_SLADE_PROMO` |
| `CLASS_MNST` | `PORTRAIT_KIWI_PROMO` |

If none matches, the input value remains in `d0`. The contract preserves the raw byte comparison,
branch condition, symbolic identities, and order. It does not infer a broader numeric portrait or
ally domain from upstream comments or names.

## Private Portrait Corpus

**Confirmed static:** the top-level pointer table starts at ROM address 1,867,780 and contains 56
ordered slots. They resolve to 52 unique payloads and four aliases:

- portrait slot 35 reuses payload owner 33;
- slots 53, 54, and 55 reuse payload owner 52.

The complete private corpus has these accepted metadata facts:

| Field | Confirmed value |
| --- | ---: |
| pointer slots | 56 |
| unique payloads | 52 |
| aliases | 4 |
| total payload bytes | 64,834 |
| header bytes | 3,788 |
| palette bytes within the headers | 1,664 |
| compressed bytes | 61,046 |
| decoded bytes per unique payload | 2,048 |
| total decoded bytes | 106,496 |
| eye entries | 261 |
| mouth entries | 218 |
| minimum/maximum header bytes | 36 / 100 |

Each unique payload stores a word-counted sequence of four-byte eye entries, then a word-counted
sequence of four-byte mouth entries, then one 32-byte palette, followed by one Stack-compressed tile
stream. The palette bytes are part of the accepted header-byte boundary, not an additional amount to
add to the total payload size.

All 56 pointer values and all 52 payloads match source, H1, and ROM. The pointer order and alias
identity must remain first-class during private import. A public fixture or report retains only
counts, addresses, aliases, sizes, codec statistics, and hashes rather than original headers,
palettes, compressed streams, or decoded tiles.

The original loader performs no explicit selector-range check before indexing the pointer table.
The accepted corpus closes valid slots `0..55`; behavior for invalid, injected, modified, or corrupt
indices remains **Unknown** rather than being normalized into a new fallback.

## `LoadPortrait` Source Order

**Confirmed static source review and corpus boundary:** for a valid accepted slot, `LoadPortrait`
performs this order:

1. preserve `d0..a3`;
2. resolve the selected pointer-table entry;
3. read the eye-entry count, store it, and copy that many four-byte entries when nonzero;
4. read the mouth-entry count, store it, and copy that many four-byte entries when nonzero;
5. copy exactly eight palette longwords into current, base, and backup palette state in parallel;
6. set the Stack destination to the portrait loading space and call `LoadStackCompressedData`, whose
   accepted payload output is 2,048 bytes;
7. increment `INPUT_REPEAT_DELAYER` by source byte value 6;
8. move the post-decode `a1` value into `a0`, load immediate VRAM destination `$F800` into `a1`, load
   `$0400` words into `d0`, and load source value 2 into `d1`;
9. call `ApplyVIntVramDma`;
10. call `ApplyVIntCramDma`;
11. restore `d0..a3` and return.

Steps 6 through 10 are exact source call/operand order. The immediate `$F800`/`$0400`/`2` operands
belong to the VRAM call seam. The selected portrait fixture does not define a portrait-specific CRAM
transfer size or prove CRAM completion; `ApplyVIntCramDma` uses global palette/queue state owned
outside this contract. Neither call establishes VInt cadence, queue processing order, wall-clock
duration, visible completion, or hardware timing.

The increment by 6 is a raw source mutation. Its upstream comment does not prove player-facing hold
input behavior, and this contract does not assign it a timing interpretation.

## Portrait Window State

### Open

**Confirmed static source review:** `OpenPortraitWindow` returns immediately when the portrait-window
index is nonzero. Otherwise it performs this bounded order:

1. increment the global window-present byte and preserve registers/input selector;
2. store mirror and right-side input bytes;
3. initialize the portrait VDP-tile word, blink counter to 20, and secondary portrait counter to 6;
4. create the portrait window and store its one-based window index;
5. select the normal or mirrored portrait-layout identity from the mirror toggle and copy 160 bytes;
6. restore the selector, call `GetAllyPortrait`, then call `LoadPortrait`;
7. set the window destination using the stored side toggle, move the window with source speed value
   4, and call `WaitForWindowMovementEnd`;
8. register `VInt_PerformPortraitBlinking` through the VInt-function trap;
9. set the blink-control byte to `-1`, restore registers, and return.

The normal/mirrored 8x10 layout data and exact raw 160-byte content remain owned by
[UI Layout Data](ui-layout-data.md). This contract preserves only source layout identity selection
and copy size; it does not consume the UI-layout fixture or add layout associations.

### Close

**Confirmed static source review:** `ClosePortraitWindow` returns immediately when its index is zero.
Otherwise it removes `VInt_PerformPortraitBlinking` before calculating the offscreen destination,
moves with source speed 4, waits for movement end, deletes the window, clears the stored index,
restores registers, decrements the window-present byte, and returns.

The open/close order establishes state balance and callback lifetime in source. It does not prove
visible motion duration, callback cadence, DMA completion, presentation correctness, or behavior
when global state is externally inconsistent.

## Eye and Mouth Update State

### Immediate close/update helper

**Confirmed static source review:** `ClosePortraitEyes` clears the blink-control byte, calls
`WaitForVInt`, preserves `d0`, and uses raw bit 0 to choose original/alternate eye tiles and raw bit 1
to choose original/alternate mouth tiles. Each choice calls `UpdatePortrait` with the corresponding
count/data state. The bit meanings here are call-shape facts, not rendered semantic labels.

### Registered VInt helper

**Confirmed static source review:** `VInt_PerformPortraitBlinking` returns when the portrait-window
index is zero or the blink-control byte is zero. Its accepted source operations include:

- decrement the blink counter; choose alternate eye tiles at counter 3 and original eye tiles at
  counter 0;
- at blink counter 0, call `GenerateRandomNumber` with `d6=120`, add 30 to returned `d7`, and store
  the result as the next counter;
- gate mouth-counter processing with the typewriting byte and existing counter state;
- choose alternate mouth tiles when the counter reaches 5 and original mouth tiles when it reaches
  zero or the source reset path;
- call `GenerateRandomNumber` with `d6=5`, add `$000A` to returned `d7`, and store the result.

The RNG operands and post-adds are source-static call facts. They are not observed probability
distributions, VInt frequencies, wall-clock delays, or visible animation timing.

### Tile update helper

**Confirmed static source review:** `UpdatePortrait` returns when the tile-entry count is zero.
Otherwise it reads four-byte entry records, selects original or alternate tile coordinates, applies
the mirror toggle to the window-tile write, and finishes with a window-destination handoff. Exact VDP
tile appearance, palette result, update cadence, and final frame remain **Unknown**.

## Name-Under-Portrait Window State

**Confirmed static source review:** `OpenNameUnderPortraitWindow` returns when its stored index is
nonzero. Otherwise it creates and writes the name window, reads current HP and the combatant name,
uses source name length for horizontal placement, then applies this raw branch:

- current HP word equals zero: call the orange-font writer;
- current HP word is nonzero: call the regular-font writer.

It then moves the window with source speed 4 and waits for movement end. The upstream comment labels
the zero branch as dead-character presentation, but this contract preserves only the raw zero/nonzero
predicate and selected writer identity.

`CloseNameUnderPortraitWindow` returns when its index is zero. Otherwise it moves the window
offscreen, waits, deletes it, clears the index, restores registers, and returns. Visible font colors,
name content, motion duration, status semantics, and caller admission remain separate or **Unknown**.

## Implementation-Neutral Import and State Model

The following is a logical compatibility model, not an engine class hierarchy:

```text
PortraitCatalog {
  pointerTableAddress
  pointerSlots[56] {
    slotId
    payloadRef
  }
  uniquePayloads[52] {          // private import only
    payloadId
    eyeEntries[]
    mouthEntries[]
    paletteBytes[32]
    compressedStream
    decodedHash
  }
}

PortraitSelectorResult {
  rawInputByte
  route: GET_ALLY_PORTRAIT | ENTITY_PROPERTY_LOOKUP
  portraitIdentity
}

PortraitWindowState {
  windowIndex
  windowPresentState
  mirroredToggle
  rightSideToggle
  blinkControl
  blinkCounter
  secondaryAnimationCounter
  callbackRegistered
}

NameWindowState {
  windowIndex
  selectedFontWriter: ORANGE | REGULAR
}
```

The public form omits private payload arrays and original palette/tile bytes. It retains pointer
order, alias relations, counts, sizes, addresses, hashes, symbolic selector identities, and synthetic
state-transition traces.

## Cross-System Separation

This contract receives a raw portrait/combatant selector and hands state to window, palette, VRAM,
and presentation services. It does not decide:

- entity map-sprite assignment or map-sprite-to-portrait/SFX property lookup;
- dialogue command admission, text selection, story order, or portrait-side modifier meaning;
- service-menu callers, Caravan messages, battle-scene selection, or member-screen composition;
- UI-layout data ownership, window allocator/movement implementation, or VInt callback scheduler;
- global CRAM/VRAM DMA queue semantics, completion, cadence, hardware timing, or frame composition;
- portrait art licensing, localization, accessibility, blinking safety, voice policy, or replacement
  content;
- invalid selector recovery, malformed private data diagnostics, persistence, or save/load behavior.

Those boundaries remain separate-owner, **Unknown**, private, or deliberate product design.

## Fidelity, Modernization, and Copyright Boundary

Original-format compatibility requires preserving pointer-slot identity and order, aliases, counted
eye/mouth records, palette/header/stream boundaries, decoded size/hash, selector branch/order,
open/close state order, exact LoadPortrait tail operands/calls, static animation operands/post-adds,
and the name-window zero/nonzero branch.

A remake may transcode private portrait streams during import, use a modern texture atlas, replace
random blinking with an accessibility-safe policy, choose different window motion, or use authored
locale-specific name presentation. Those decisions must be reported as deliberate modernization
rather than evidence about original visible behavior.

Original portrait headers, palettes, compressed streams, decoded tiles, screenshots, captures, and
other game assets are private/generated copyrighted inputs. Do not commit or redistribute them.
Public builds require newly authored or properly licensed content.

## H4 Acceptance Surface

A remake-side portrait importer/adapter can claim this contract only when automated tests prove:

1. exactly 56 ordered pointer slots resolve to 52 unique payloads with aliases `35→33` and
   `53/54/55→52`, while all pointer/payload identities match the accepted provenance;
2. private payloads preserve counted four-byte eye and mouth entries, one 32-byte palette, one Stack
   stream, the accepted aggregate counts/sizes, and exactly 2,048 decoded bytes per unique payload;
3. public fixtures/reports retain only metadata, identities, counts, aliases, sizes, addresses,
   codec statistics, hashes, and synthetic examples, never original portrait bytes;
4. the combatant selector preserves the raw byte-sign branch and return handoff, and the ally helper
   preserves its exact unsigned compare/`bhi` gate plus five symbolic class-remap checks in order;
5. LoadPortrait preserves counted eye/mouth copying, eight-longword palette copies, Stack call,
   repeat-delay `+6`, immediate VRAM operand setup `$F800`/`$0400`/`2`, VRAM call, then CRAM call,
   without claiming portrait-specific CRAM bounds, completion, or cadence;
6. portrait-window open/close preserves index guards, toggle/counter state, normal/mirrored layout
   identity selection, selector/load order, move/wait seams, callback add/remove lifetime, index
   clearing, and window-present balance;
7. source-shaped eye/mouth tests preserve gates, counter comparisons, RNG operands `120` and `5`,
   post-adds `30` and `$000A`, original/alternate selection, and mirror-state handoff without treating
   them as observed distributions or visible timing;
8. the name window preserves its index guards, create, initial `WriteWindowTiles`, current-HP read,
   combatant-name read, selected font-writer order, raw HP zero→orange and nonzero→regular writer
   selection, move/wait/delete order, and index clear;
9. invalid indices, malformed data, callers, VInt/RNG/DMA cadence, window motion, visible frames,
   palette results, localization, accessibility, and licensed content remain separate acceptance
   surfaces.

H4 does not require original assembly instructions, a Mega Drive VDP emulator, original payloads in
public builds, or frame-cycle parity unless a later explicit hardware-fidelity decision adds them.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| six function identities and H1-bound addresses | **Confirmed static inventory** | `sf2-common-menus-static-v1` and `sf2-portrait-graphics-decode-v1` (fixtures linked above) | Aggregate service/menu facts outside the selected records are excluded |
| 56 pointers, 52 payloads, aliases, counted headers, palettes, Stack streams, sizes, and source/H1/ROM parity | **Confirmed static** | `sf2-portrait-graphics-decode-v1` ([`portrait-graphics-decode-v1.json`](../../../tests/fixtures/h2/portrait-graphics-decode-v1.json)) | Raw payloads remain private; invalid-index behavior remains **Unknown** |
| selector, open/close, LoadPortrait, update, callback, and name-window instruction/call order | **Confirmed static source review** | Pinned source paths at commit `c834c652b6862bc5679fd7f69a38a7093206efc6` | Natural callers, externally inconsistent state, and runtime results remain **Unknown** |
| portrait property lookup and dialogue command/caller seams | **Separate owners** | [Sprite-Dialogue Property Data](sprite-dialogue-property-data.md) and [Dialogue System](dialogue-system.md) | End-to-end dialogue presentation remains unclosed |
| normal/mirrored portrait layout payloads | **Separate owner** | [UI Layout Data](ui-layout-data.md) | This contract preserves selection identity/copy size but consumes no layout fixture |
| VInt callback execution, RNG distribution, DMA queue processing/completion, visible timing, and final frames | **Unknown / separate owner** | Future bounded runtime/presentation evidence | Static call order is not runtime cadence |
| renderer, accessibility, localization, replacement portraits, and licensed content | **Deliberate design** | Future product/content decisions | Requires separate provenance and acceptance |

## Reproduction

```powershell
uv run sf2 h2 common-menus
uv run sf2 h2 portraits
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated detailed outputs remain under ignored `local/derived/common-menus-static.json` and
`local/derived/portrait-graphics-decode.json`.
