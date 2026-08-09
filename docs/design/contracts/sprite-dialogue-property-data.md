# Sprite-Dialogue Property Data Contract

- **Confirmed original structure:** the complete 119-record map-sprite dialogue-property table,
  independent two-byte terminator, H1-bound addresses, source/ROM parity, and bounded lookup consumer
  described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** natural fallback reachability, caller admission, portrait suppression
  and rendering, speech-SFX playback and timing, text/window/input synchronization, runtime and
  caller-specific provenance of the particular entity map-sprite byte supplied to this lookup, and
  player-facing meaning.
- Remake status: implementation-neutral Phase 3 data/lookup contract; no dialogue presentation,
  portrait renderer, voice or bleep policy, localization flow, accessibility behavior, or licensed
  content pack has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static identity and private-import boundary for the original mapping from
an entity's map-sprite byte to one portrait byte and one speech-SFX byte. It owns:

1. the ordered 119-record table, its four-byte record layout, address range, and hash;
2. the independent `0xFFFF` word terminator after those records;
3. map-sprite key uniqueness, portrait-value and speech-SFX aggregate facts, and reserved-byte facts;
4. the complete source-static lookup order, match results, miss termination, and fallback values;
5. a public H4 surface based on structure, identities, counts, histogram, address, and hashes rather
   than the original row payload.

It does not own dialogue-command decoding, text selection, window composition, portrait pixels,
speech playback, controller waits, caller reachability, map-sprite assignment, or final presentation.
The adjacent [dialogue-command contract](dialogue-system.md) owns command and caller-local seams. The
[audio-system contract](audio-system.md) owns its bounded music/SFX command domains and driver data;
neither adjacent contract turns a property-table speech-SFX identity into observed audio behavior.

The sole executable owner is `sf2-sprite-dialogue-static-v1` in
[`tests/fixtures/h2/sprite-dialogue-static-v1.json`](../../../tests/fixtures/h2/sprite-dialogue-static-v1.json),
implemented by
[`src/sf2tool/h2/sprite_dialogue.py`](../../../src/sf2tool/h2/sprite_dialogue.py). The research owners
are the sprite-dialogue sections of
[Auxiliary Data Inventory](../../research/auxiliary-data-inventory.md) and
[Common Scripting](../../research/common-scripting.md).

## Pre-Contract Evidence Audit

Fresh reproduction passed on the evidence date:

```text
Contract sf2-sprite-dialogue-static-v1
SHA256 B1D5979F71C298F2805D88D223CC89581D198396812943EA4FF1E93A7BA2B185
Rows 119
PortraitBearingRows 80
Status PASS
```

The audit checked the dedicated fixture, verifier, source-owner prose, private generated output,
H1-bound addresses, source/ROM range, and pinned consumer source. It also found an exact one-to-one
future association boundary of two currently unassociated research records:

- `scripting.entity.getentityportaitandspeechsfx` bound to
  `table.GetEntityPortaitAndSpeechSfx`;
- `auxiliary.data.table-mapspritedialogueproperties` bound to
  `table.table_MapspriteDialogueProperties`.

The source spelling `GetEntityPortaitAndSpeechSfx` is retained only as the original identifier;
prose uses “portrait.” The aggregate common-scripting and auxiliary-data fixtures are not evidence
dependencies of this contract. Registration is deferred until preliminary semantic acceptance.

The audit preserves these limits:

- 119 four-byte records occupy 476 bytes. The following `0xFFFF` word is a separate two-byte
  terminator, not a 120th record; together they form the 478-byte checked range.
- “51 distinct portrait byte values” is a cardinality fact. It does not define a contiguous `0..50`
  range or a closed portrait domain.
- the ten source-named speech-SFX identities and their counts do not establish audible waveform,
  pitch, speaker, mood, scene use, timing, or player-facing meaning;
- all map-sprite keys are unique in this accepted table, but the consumer's first-match scan remains
  part of the original algorithm;
- the tracked fixture contains no raw row array. Complete rows remain private/generated input.

Active Issue #81 owns a separate technical-graphics correction and is not an evidence or merge
dependency for this document.

## Physical Table Contract

**Confirmed static:** `table_MapspriteDialogueProperties` starts at H1-bound ROM address 284,282 and
its accepted range ends exclusively at 284,760. The complete 478-byte source/ROM range has SHA-256
`DA351FAB189D0AE07D1300152A02B5D214DE179CE89C2E802B27EA7D617CAACC`.

Each of the 119 records has this exact byte order:

```text
offset +0: map-sprite key
offset +1: portrait byte
offset +2: speech-SFX byte
offset +3: reserved byte
```

All 119 reserved bytes are zero. The consumer does not load offset `+3`; zero-filled storage and
consumer ignorance are separate confirmed facts. After the 476 record bytes, one big-endian
`0xFFFF` word terminates the scan domain.

A private importer MUST preserve record order, each raw byte, the standalone terminator, source
symbol, address range, and full-range hash. A public fixture or report MUST retain only structural
metadata, hashes, aggregate counts, identifiers, or synthetic examples.

## Key and Value Domains

**Confirmed static:** the 119 map-sprite key bytes are all distinct. There are no duplicate key
values in the accepted corpus. This proves that each present key selects one row, but it does not
prove that every possible byte value is present or naturally assigned to an entity.

The portrait column contains 51 distinct byte values. Thirty-nine rows contain source identity
`PORTRAIT_NONE`; the other 80 are portrait-bearing by the accepted owner's classification. These
counts neither establish a contiguous portrait-ID range nor guarantee a renderable asset for every
raw, modified, debug, or corrupt value.

The speech-SFX column contains ten source identities:

| Source identity | Confirmed row count |
| --- | ---: |
| `DIALOG_BLEEP_1` | 7 |
| `DIALOG_BLEEP_2` | 12 |
| `DIALOG_BLEEP_3` | 9 |
| `DIALOG_BLEEP_4` | 8 |
| `DIALOG_BLEEP_5` | 33 |
| `DIALOG_BLEEP_6` | 33 |
| `DIALOG_BLEEP_7` | 9 |
| `DIALOG_BLEEP_8` | 5 |
| `TAROS_DIALOG_BLEEP` | 1 |
| `DEMON_BREATH` | 2 |

The counts total 119. These names remain provenance-bearing enum identities, not presentation or
audio-semantics claims.

## Complete Consumer Lookup Order

**Confirmed static:** the original identifier `GetEntityPortaitAndSpeechSfx` begins at ROM address
284,216. Its complete lookup and result order is:

1. preserve `d0`, `a0`, and `a5` on the stack;
2. apply `andi.w #COMBATANT_MASK_ALL,d0`;
3. execute `clr.w d1`, then `clr.w d2`;
4. call `GetEntityAddressFromCharacter` and read the entity's map-sprite byte into `d0`;
5. load the property-table address and compare that byte with the current record's key;
6. on a match, move the portrait byte into `d1`, sign-extend it to a word, move the speech-SFX byte
   into the already-cleared `d2`, and take the done path;
7. on a miss, advance exactly four bytes, compare the next word with `0xFFFF`, and either repeat the
   key comparison or write the two fallback words;
8. restore `d0`, `a0`, and `a5`, then return.

The hit-path portrait result is signed because the byte move is followed by `ext.w d1`;
`PORTRAIT_NONE` byte value 255 therefore becomes word value `-1`. The hit-path speech-SFX result is
an unsigned byte value in a word because `clr.w d2` precedes the byte write into `d2`. An importer or
compatibility adapter MUST NOT omit that clear while claiming the accepted unsigned result.

On table exhaustion the source writes `PORTRAIT_DEFAULT` (`-1`) to `d1` and
`SFX_DIALOG_BLEEP_6` (`74`) to `d2`. This is the static fallback path. Which original entity/caller
states naturally reach it remains **Unknown**.

The accepted corpus's unique keys make “first match” deterministic. A compatibility implementation
should still preserve ordered first-match behavior for private original-format imports rather than
silently defining a new duplicate-key policy.

## Implementation-Neutral Import Model

The following is a logical contract, not an engine-class prescription:

```text
SpriteDialoguePropertyTable {
  tableId
  sourceSymbol: table_MapspriteDialogueProperties
  romStart: 284282
  romEndExclusive: 284760
  rangeHash

  records[119] {               // private import only
    rowIndex
    mapSpriteKeyByte
    portraitByte
    speechSfxByte
    reservedByte
  }

  terminatorWord: 0xFFFF       // independent of records[]
}

SpriteDialogueLookupInput {
  characterIndexWord
  resolvedEntityMapSpriteByte
}

SpriteDialogueLookupResult {
  portraitWord                 // sign-extended on hit; -1 on fallback
  speechSfxWord                // zero-extended on hit; 74 on fallback
  matchedRowIndex?
  usedFallback
}
```

The public form omits `records[]`. It retains the table identity, range, hash, record/terminator
shape, cardinalities, speech-SFX histogram, consumer addresses, and lookup rules. A modern runtime
may transcode a validated private import into a keyed resource, provided original row order and
first-match behavior remain reproducible for fidelity tests.

## Cross-System Separation

The handoff into this contract is an entity map-sprite byte. How an entity received that byte is
owned by entity, map-script, ally/enemy definition, or other state systems. This contract neither
validates the incoming assignment nor asserts that all 119 keys occur during normal play.

The handoff out is a portrait word and speech-SFX word. The dialogue system may use those identities,
but this contract does not define:

- dialogue-command admission, text-line choice, name substitution, or story order;
- portrait asset lookup, suppression, placement, mirroring, animation, palette, or rendering;
- speech-SFX command submission, audio-driver behavior, waveform, duration, or synchronization;
- window allocation, text drawing, controller waits, skip behavior, or visible timing;
- localization, accessibility, alternate presentation, voice acting, or replacement assets;
- importer admission, diagnostics, or recovery for duplicate-key tables, truncated records, or
  missing terminators, and behavior for injected out-of-domain state.

Duplicate-key tables are outside the accepted valid corpus. If a source-shaped private table with
duplicate keys is scanned instead of rejected, the confirmed ordered algorithm selects the first
matching row; that deterministic scan result is not an acceptance guarantee for malformed input.

Those boundaries remain separate-owner, **Unknown**, or deliberate product design.

## Fidelity, Modernization, and Copyright Boundary

Original-format compatibility requires preserving table identity, H1-bound addresses, address range,
row order, all four bytes per row, the separate terminator, source/ROM hash, key/value identities,
complete consumer order, signed portrait result, zero-extended speech-SFX result, and fallback words
when importing a private original corpus.

A remake may choose named dialogue profiles, authored speaker metadata, voice playback, synthesized
bleeps, scalable portraits, subtitle accessibility, or locale-specific presentation. Those choices
must remain explicit modern content/architecture decisions rather than evidence about the original
table.

The original 119-row payload, portrait art, sound data, dialogue text, screenshots, and captured
output are private/generated copyrighted inputs. Do not commit or redistribute them. Public builds
require newly authored or properly licensed content.

## H4 Acceptance Surface

A remake-side importer or compatibility adapter can claim this contract only when automated tests
prove:

1. exactly 119 ordered four-byte records occupy 476 bytes and are followed by one independent
   two-byte `0xFFFF` terminator, producing the accepted 478-byte range and hash;
2. all 119 map-sprite keys remain distinct, all 119 reserved bytes remain zero, and record order and
   complete private bytes round-trip without publishing the row payload;
3. portrait metadata retains exactly 51 distinct byte values, 39 `PORTRAIT_NONE` rows, and 80
   portrait-bearing rows without inventing a contiguous or closed portrait domain;
4. the ten speech-SFX source identities and their exact 119-row histogram remain stable without
   assigning unverified audible or player-facing meaning;
5. the consumer preserves mask, `clr.w d1`, `clr.w d2`, entity resolution, map-sprite read, ordered
   comparison, match loads, portrait sign extension, miss stride, terminator check, fallback writes,
   register restore, and return order;
6. hit results preserve signed portrait behavior and zero-extended speech-SFX behavior, while miss
   results preserve portrait `-1` and speech-SFX value `74`;
7. public fixtures and reports contain only metadata, identities, counts, histogram, ranges, hashes,
   and synthetic examples, never original row bytes or audiovisual content;
8. caller reachability, fallback use, portrait/audio rendering, timing, localization, accessibility,
   and intentional presentation changes are tested and reported separately.

H4 does not require the original linear table as the remake's runtime storage. It requires
deterministic provenance-preserving import and an explicit compatibility path for the accepted
ordered lookup behavior.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| 119 four-byte records, independent terminator, 478-byte range/hash, unique keys, portrait/SFX/reserved aggregates | **Confirmed static** | `sf2-sprite-dialogue-static-v1` ([`sprite-dialogue-static-v1.json`](../../../tests/fixtures/h2/sprite-dialogue-static-v1.json)) | Raw rows stay private; natural key reachability remains unclosed |
| complete lookup order, signed portrait hit result, zero-extended SFX hit result, termination, and fallback | **Confirmed static** | `sf2-sprite-dialogue-static-v1` ([`sprite-dialogue-static-v1.json`](../../../tests/fixtures/h2/sprite-dialogue-static-v1.json)) | Natural fallback reachability and caller-visible use remain **Unknown** |
| command/caller-local dialogue seam | **Separate owner** | [dialogue-command contract](dialogue-system.md) | End-to-end text/portrait/audio presentation remains unclosed |
| speech-SFX driver/domain data | **Separate owner** | [audio-system contract](audio-system.md) | Playback admission, waveform, timing, and synchronization remain unclosed |
| localization, accessibility, voice policy, replacement assets, distributable content | **Deliberate design** | Future product/content decisions | Requires provenance, licensing, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 sprite-dialogue
uv run sf2 design-contracts test
uv run sf2 verify
```

The generated detailed output remains under ignored `local/derived/sprite-dialogue-static.json`.
