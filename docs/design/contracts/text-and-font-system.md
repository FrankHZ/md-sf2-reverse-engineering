# Text Storage, Decoding, and Font Contract

- **Confirmed original structure:** the 255-entry context-Huffman offset table, 86 reachable trees,
  17 compressed text banks, 4,267 length-prefixed records, complete deterministic symbol replay,
  80-record variable-width font, 256-entry ASCII map, and bounded consumer facts below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** control-code side effects, inserted name/item/spell behavior,
  rendered glyph overlap, palette and window composition, typewriter/input/wait timing, DMA-visible
  output, nonstandard symbol injection, and player-facing text semantics.
- Remake status: implementation-neutral Phase 3 contract; no dialogue markup, localization format,
  font technology, layout engine, renderer, accessibility policy, or licensed text content has been
  selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the import and identity boundary from an original text-line ID through its
compressed record and context decoder to a glyph-ID stream and font metadata. It owns:

1. the original global line-ID, bank, record, compressed payload, and pointer-table shape;
2. context-Huffman offset, tree-packing, code, reachability, and decoder-state facts;
3. deterministic aggregate replay of all original compressed records without publishing plaintext;
4. ASCII-byte-to-glyph mapping and variable-width glyph-record metadata;
5. a public H4 surface based on structure, counts, addresses, and hashes rather than copyrighted text
   or font pixels.

It does not own map-script dialogue command routing, portrait or speech-SFX selection, control-code
execution, name/item/spell substitution, windows, glyph raster composition, input waits, audio,
campaign ordering, translation, or final presentation. The adjacent
[dialogue-command contract](dialogue-system.md) owns source command layouts and handler-local cursor
seams; it deliberately does not decode or render this corpus.

The executable evidence owners are:

- `sf2-text-huffman-static-v1` in
  [`tests/fixtures/h2/text-huffman-static-v1.json`](../../../tests/fixtures/h2/text-huffman-static-v1.json);
- `sf2-text-banks-static-v1` in
  [`tests/fixtures/h2/text-banks-static-v1.json`](../../../tests/fixtures/h2/text-banks-static-v1.json);
- `sf2-variable-width-font-static-v1` in
  [`tests/fixtures/h2/variable-width-font-static-v1.json`](../../../tests/fixtures/h2/variable-width-font-static-v1.json).

The research owners are the text and font sections of
[common scripting](../../research/common-scripting.md),
[technical services](../../research/technical-services.md),
[auxiliary data](../../research/auxiliary-data-inventory.md), and
[technical graphics](../../research/technical-graphics.md).

## Pre-Contract Evidence Audit

This synthesis was checked against the three dedicated fixtures, their verifiers, the generated
static outputs, decoder and display source fragments, all bank payloads, the Huffman tree payload,
and the font/ASCII resources. Fresh reproduction passed for all three dedicated owners on the
evidence date. The audit preserves these limits:

- the tracked text-bank fixture contains no plaintext; decoded per-string symbol arrays stay under
  the ignored local root, and `gamescript.txt` contributes only its count, ID continuity, and hash;
- a compressed length prefix bounds stored bytes, while the decoder stops at symbol 254; the observed
  8-to-15 trailing stored bits are retained as data facts, not labeled as padding or content;
- source labels for symbols 238 through 253 do not establish control-code effects, operand widths,
  waits, substitutions, palette changes, or visible meaning;
- symbol 253 is absent from all 4,267 original records, which proves corpus absence rather than
  impossibility under direct or modified input;
- glyph IDs 70 and 71 are absent from both accepted normal input paths, not proven unreachable from
  every debug, direct, modified, or corrupt caller;
- source call counts distinguish regular and non-regular dialogue paths, but do not establish pixels,
  overlap, timing, or player-visible layout;
- the currently failing aggregate `sf2-tech-interfaces-static-v1` rail is not an evidence dependency.
  Its jump-table record for `InitializeHuffmanDecoder` is excluded; the dedicated Huffman fixture owns
  the decoder address and source-shape facts used here. This document does not claim a fresh
  tech-interfaces PASS.

No contradiction was found in the three dedicated owners. One accepted upstream note says the
Huffman offset table has 256 entries; the actual 510-byte payload contains 255, and the executable
owner intentionally preserves that mismatch rather than repeating the note.

## Identity Domains

An implementation MUST keep these identities separate:

| Domain | Confirmed original boundary |
| --- | --- |
| global text-line ID | one integer in the contiguous source domain 0 through 4,266 |
| bank index | the high portion of the line ID, `lineId >> 8`, selecting one of 17 banks |
| within-bank index | `lineId & 255`; valid rows are determined by the selected bank's actual count |
| stored record | one length byte followed by that many compressed bytes |
| compressed bit position | state within a stored payload; termination may leave 8 through 15 stored bits |
| decoded symbol | one byte selected by the previous symbol's Huffman tree |
| context symbol | the previous decoded symbol; initial value is 254 |
| control symbol | a decoded symbol from 238 through 254; identity is closed, effect is not |
| glyph ID | one-based font record ID 1 through 80 |
| ASCII byte | an alternate direct-input byte mapped through a 256-entry table before glyph use |
| source text | copyrighted player-facing content kept outside tracked fixtures and public outputs |
| localized text | separately authored product content; never inferred from original symbol IDs |

The canonical import MUST retain the global line ID even if a remake stores text in named resources.
Bank position, compressed bytes, decoded symbols, glyph IDs, and localized strings are related but
must not be collapsed into one interchangeable identifier.

## Context-Huffman Tree Contract

**Confirmed static:** the offset resource is 510 bytes, giving 255 big-endian entries. Eighty-six
entries select a tree and 169 contain `0xFFFF`. Defined offsets are unique and strictly increasing.
The 1,952-byte tree resource packs all 86 records contiguously with no gap or overlap.

Each record stores its leaf symbols before its node-bit stream in reverse order. Across all records:

| Fact | Confirmed value |
| --- | ---: |
| leaf entries | 1,536 |
| non-leaf nodes | 1,450 |
| node bits | 2,986 |
| node-storage bytes | 416 |
| symbol-storage bytes | 1,536 |
| zero padding bits | 342 |
| code-length range | 0..14 bits |
| leaf-count range per tree | 1..66 |

Node bit `0` opens left and right children; bit `1` selects a leaf. Input bit `0` follows the left
branch and `1` follows the right. Context 54 is the sole one-leaf tree and emits symbol 58 without
consuming an input bit.

**Confirmed graph boundary:** the set of symbols emitted by all trees exactly equals the set of 86
defined context symbols. Starting from initial previous symbol 254, every defined context is
reachable, and no accepted decode path needs one of the 169 missing entries. This closes the static
context graph, not the semantic meaning of any emitted symbol.

An original-format decoder MUST preserve the previous-symbol context, the bit barrel across calls,
the branch order, reverse leaf storage, and zero-bit single-leaf case. A modern text pipeline MAY
transcode accepted private inputs to a different representation, provided the import adapter can
reproduce the canonical symbol hashes and boundaries.

## Text-Bank and Record Contract

**Confirmed static:** sixteen banks contain 256 records each and bank 16 contains 171, totaling 4,267
records. The 17 bank payloads occupy 79,013 bytes:

| Component | Confirmed bytes/count |
| --- | ---: |
| length prefixes | 4,267 bytes |
| compressed payloads | 74,746 bytes |
| bank payload total | 79,013 bytes |
| 17-entry pointer table | 68 bytes |
| alignment between final bank and pointer table | 1 byte |
| top-level pointer | 4 bytes |
| total source/ROM parity boundary | 79,086 bytes |

The source selection formula is `bank = lineId >> 8` and `withinBank = lineId & 255`. Within a bank,
the selected record is reached by advancing `1 + lengthByte` for each preceding record. This is an
import/layout rule, not permission to read beyond the actual 171 rows in the partial bank.

Complete deterministic replay yields 152,679 decoded symbols, of which 4,267 are the single
terminator 254 for each record and 148,412 are non-terminators. All 86 defined contexts occur. The
stored compressed payload length ranges from 3 through 116 bytes, excluding its one-byte prefix;
decoded record length ranges from 2 through 243 symbols, and the terminator leaves 8 through 15
stored bits in every record.

The adjacent source script has exactly the contiguous IDs 0 through 4,266. Its plaintext and the
per-record decoded symbol sequences remain private/generated; tracked evidence retains only aggregate
facts and hashes. A remake importer can validate a user-provided private source pack without making
the pack a repository or distribution dependency.

## Symbol and Control Boundary

The decoder begins with previous symbol 254, selects a tree from the previous decoded symbol, emits
one new symbol, and makes that symbol the next context. Symbol 254 terminates the current record.
Symbols 238 through 252 occur 8,783 times before terminators in the original corpus. Symbol 253 occurs
zero times.

These are byte-domain facts only. Until a dedicated owner closes each consumer, a remake MUST retain
control symbols and any following bytes as source-faithful structured input rather than assigning
guessed operations. In particular, this contract does not define:

- parameter widths or cursor advancement for each control symbol;
- name, item, spell, number, or other substitutions;
- window open/close, line wrapping, waits, input admission, or skip behavior;
- font color, palette, portrait, speech SFX, or other presentation effects;
- whether symbol 253 can be reached through nonstandard input.

Modern markup and localization tokens are deliberate authoring choices. Their adapter must keep an
explicit mapping back to accepted original IDs when fidelity tests or original content imports need
that traceability.

## Variable-Width Font Contract

**Confirmed static:** the font payload is 2,560 bytes containing 80 fixed 32-byte glyph records. Each
record has a padded width header followed by fifteen two-byte rows with twelve usable pixel columns.
All header and row padding bits are zero. The accepted metadata contains one blank glyph, 1,633 set
pixels, 746 rows with pixels, stored widths 3 through 9, and five glyphs whose set pixels extend at or
beyond the computed advance.

The source loader uses one-based IDs: it subtracts one, shifts by five, and adds the result to the font
base. It masks the stored width to its low nibble, advances zero for a stored value of zero, and
otherwise advances `storedWidth + 1`. No tracked fixture exports glyph pixels. Whole-font, pointer,
and ASCII-map hashes plus aggregate shape counters provide the tracked public parity surface;
per-glyph hashes remain generated local metadata.

The 256-entry ASCII map accepts only glyph IDs 1 through 80. It reaches 78 unique IDs, maps 145 input
bytes to default glyph 1, has 79 non-default printable-ASCII entries and 32 non-default extended-byte
entries, and never emits glyph IDs 70 or 71. The Huffman corpus independently emits 69 IDs in the
glyph range; the union of both normal inputs still reaches 78 of 80 and omits the same two IDs.

**Confirmed source topology:** `GetNextTextSymbol` uses the ASCII table only when the source-named RAM
ASCII pointer is nonzero; the Huffman path bypasses it. `SymbolsToGraphics` calls the glyph loader once
for regular dialogue and twice on the source's non-regular path. Rendering overlap, palette, advance
composition, DMA, and timing remain **Unknown**.

## Caller and Adjacent-System Boundary

The [dialogue-command contract](dialogue-system.md) validates line-ID operands against the contiguous
0-through-4,266 domain and owns command/cursor sequencing. This contract owns how a valid line ID
selects stored text and how decoded symbols relate to font identities. Neither contract alone proves
end-to-end player-visible dialogue.

Window construction, portrait placement, speech sound, controller waits, map-script reachability,
story order, and save persistence remain with their respective owners. A caller may retain only a
line ID, while the text system may return an opaque symbol/control stream to later services. That seam
is preferable to embedding window or story behavior into the decoder.

## Fidelity, Localization, and Copyright Boundary

Original-format compatibility requires preserving:

- global IDs, bank and within-bank selection, record boundaries, pointers, and source/ROM hashes;
- the exact context-tree graph, decoder state, bit order, leaf order, and terminator behavior;
- decoded symbol/control identities without guessed effects;
- glyph IDs, ASCII mapping, record widths, padding validity, and loader address/advance formulas.

A modern remake may deliberately choose UTF-8 text, shaped fonts, proportional layout, scalable UI,
right-to-left support, accessibility settings, new control markup, and independent localization.
Those choices do not alter original evidence. A mapping layer should report where modern content or
behavior intentionally diverges from the original ID/symbol contract.

Original plaintext, compressed bank bytes, decoded symbol sequences, font pixels, screenshots, and
captured rendering remain private/generated copyrighted inputs. Do not commit or redistribute them.
Only separately licensed or newly authored text and fonts may ship with a public remake.

## H4 Acceptance Surface

A remake-side importer or compatibility adapter can claim this contract only when automated tests
prove:

1. exactly 4,267 original line IDs map to the accepted 17-bank, 16-full/one-partial record shape;
2. pointer order, record lengths, bank-payload hashes, aggregate decoded hashes, terminator
   count, and source/ROM parity metadata match the accepted private input;
3. all 255 offset entries, 86 trees, 1,536 leaves, packing spans, code lengths, and context-graph
   reachability match the canonical owner;
4. decode begins at context 254, preserves cross-call bit/context state, handles the zero-bit leaf,
   and emits one terminator per record;
5. control symbols remain lossless opaque identities until a separately accepted behavior owner
   defines their effects;
6. all 80 glyph records and 256 ASCII mappings retain IDs, widths, padding validity, missing IDs,
   loader formulas, and the accepted whole-font/pointer/ASCII-map hashes;
7. tracked/public test artifacts contain only metadata, hashes, and synthetic examples, never
   original plaintext, compressed payloads, decoded sequences, or font pixels;
8. localized or modernized text/rendering behavior is tested and reported separately from original
   storage/decoder/font parity.

H4 does not require the original Huffman format or bitmap font as the remake's authoring/runtime
format. It requires deterministic provenance-preserving import when original-compatible private data
is used.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| 255 offsets, 86 trees, packing, codes, decoder state, and context reachability | **Confirmed static** | `sf2-text-huffman-static-v1` ([`text-huffman-static-v1.json`](../../../tests/fixtures/h2/text-huffman-static-v1.json)) | Symbol meaning, control effects, nonstandard injection |
| 17 banks, 4,267 records, pointer shape, full deterministic replay, aggregate hashes | **Confirmed static** | `sf2-text-banks-static-v1` ([`text-banks-static-v1.json`](../../../tests/fixtures/h2/text-banks-static-v1.json)) | Plaintext semantics, substitutions, presentation, caller reachability |
| 80 glyph records, 256 ASCII mappings, font consumer/address/advance facts | **Confirmed static** | `sf2-variable-width-font-static-v1` ([`variable-width-font-static-v1.json`](../../../tests/fixtures/h2/variable-width-font-static-v1.json)) | Pixels in public artifacts, overlap, palette, DMA, visible timing |
| map-script dialogue command and cursor routing | **Separate owner** | [dialogue-command contract](dialogue-system.md) | End-to-end decoded/rendered dialogue remains unclosed |
| control-code execution and complete presentation | **Unknown** | Future bounded consumer/runtime owners | Do not infer effects from source labels or corpus occurrence |
| localization, modern markup/font, accessibility, distributable content | **Deliberate design** | Future product/content decisions | Requires provenance, licensing, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 text-huffman
uv run sf2 h2 text-banks
uv run sf2 h2 variable-width-font
uv run sf2 design-contracts test
uv run sf2 verify
```
