# USA ROM block ledger

Find a zero-based file offset in the section table, then follow its layout and research links. For a named binary asset, the [offset lookup](#look-up-a-split-file) returns every matching upstream path. The [documented subranges](#documented-subranges) have stable IDs for citations from research and issues. All ends are exclusive. The last byte belongs to S18 at `0x1FFFFF`. [ROM identity](../../manifests/roms/sf2-us.json) · [Byte-identical reconstruction](reproducible-original.md#result)

## Physical sections

**Confirmed** boundaries and build contents come from the 18 vanilla includes in the [pinned ROM layout](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-00-romlayout.asm). Each layout link below names its ordered inputs. The description identifies principal contents and the research link gives a starting point for their interpretation. A section can contain code, data and alignment in the same interval.

| ID | Start | End | Bytes | Split entries | Distinct split bytes | Layout and contents | Research |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- |
| <a id="s01"></a>S01 | `0x000000` | `0x008000` | 32,768 | 2 | 1,184 | [System startup, interrupts, map movement and script dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-01-0x000000-0x008000.asm) | [Map engine](world/common-maps.md) |
| <a id="s02"></a>S02 | `0x008000` | `0x010000` | 32,768 | 0 | 0 | [Stats, battle actions, item effects and enemy decisions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-02-0x008000-0x010000.asm) | [Battle actions](battle/battle-actions.md) |
| <a id="s03"></a>S03 | `0x010000` | `0x018000` | 32,768 | 6 | 1,082 | [Menus and prompts, item and spell tables](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-03-0x010000-0x018000.asm) | [Core table map](party/static-core-data.md#rom-table-map) |
| <a id="s04"></a>S04 | `0x018000` | `0x020000` | 32,768 | 1 | 32 | [Battle scene playback and effects](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-04-0x018000-0x020000.asm) | [Battle scenes](battle/battle-scene-engine.md) |
| <a id="s05"></a>S05 | `0x020000` | `0x028000` | 32,768 | 7 | 6,998 | [Field services, combat and exploration routines, special sprites](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-05-0x020000-0x028000.asm) | [Service and menu callers](presentation/common-menus.md) |
| <a id="s06"></a>S06 | `0x028000` | `0x044000` | 114,688 | 37 | 104,077 | [Text decoding and banks, fonts, interface tiles, credits and special screens](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-06-0x028000-0x044000.asm) | [Special screens](presentation/special-screens.md) |
| <a id="s07"></a>S07 | `0x044000` | `0x064000` | 131,072 | 1 | 120 | [Actor and scene scripts, battle scenes and map setups](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-07-0x044000-0x064000.asm) | [Map setup inventory](world/map-data-inventory.md) |
| <a id="s08"></a>S08 | `0x064000` | `0x0C8000` | 409,600 | 285 | 392,704 | [Map tiles, palettes, headers and map data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-08-0x064000-0x0C8000.asm) | [Map data inventory](world/map-data-inventory.md) · [Block and layout ranges by map](world/map-payload-ranges.md) |
| <a id="s09"></a>S09 | `0x0C8000` | `0x100000` | 229,376 | 670 | 225,544 | [Map sprite frames and their lookup data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-09-0x0C8000-0x100000.asm) | [Map sprite corpus](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s10"></a>S10 | `0x100000` | `0x130000` | 196,608 | 35 | 194,456 | [Title screen, battle backdrops and invocation graphics](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-10-0x100000-0x130000.asm) | [Title and special screens](presentation/special-screens.md) |
| <a id="s11"></a>S11 | `0x130000` | `0x180000` | 327,680 | 54 | 327,028 | [Enemy battle sprite graphics](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-11-0x130000-0x180000.asm) | [Battle sprite corpus](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s12"></a>S12 | `0x180000` | `0x1AC000` | 180,224 | 243 | 177,918 | [Ally battle sprites, animations, status and transition tiles](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-12-0x180000-0x1AC000.asm) | [Sprite and animation corpus](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s13"></a>S13 | `0x1AC000` | `0x1B8000` | 49,152 | 45 | 20,242 | [Terrain, formations, enemy definitions and battle setup code](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-13-0x1AC000-0x1B8000.asm) | [Battle formations](battle/battle-spriteset-data.md) |
| <a id="s14"></a>S14 | `0x1B8000` | `0x1C8000` | 65,536 | 131 | 65,006 | [Battle grounds, weapons, spell graphics and witch screens](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-14-0x1B8000-0x1C8000.asm) | [Grounds, weapons and spells](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s15"></a>S15 | `0x1C8000` | `0x1D8000` | 65,536 | 52 | 64,834 | [Portrait pointer chain, compressed portraits and alignment](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-15-0x1C8000-0x1D8000.asm) | [Portraits](../design/contracts/graphics/portrait-graphics-data.md) |
| <a id="s16"></a>S16 | `0x1D8000` | `0x1E0000` | 32,768 | 167 | 31,296 | [Icon lookup and storage](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-16-0x1D8000-0x1E0000.asm) | [Icon storage](presentation/auxiliary-data-inventory.md) |
| <a id="s17"></a>S17 | `0x1E0000` | `0x1F0000` | 65,536 | 10 | 54,530 | [PCM, instruments, sound driver, ally stats and special-screen graphics](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-17-0x1E0000-0x1F0000.asm) | [Ally and class records](party/static-core-data.md#rom-table-map) |
| <a id="s18"></a>S18 | `0x1F0000` | `0x200000` | 65,536 | 2 | 65,536 | [Z80 music bank 1 followed by bank 0](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-18-0x1F0000-0x200000.asm) | [Bank addressing](presentation/sound-data-inventory.md#separate-address-space) |

**Reading the totals.** The section intervals cover 2,097,152/2,097,152 physical bytes. The [split inventory](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/split/sf2splits.txt) contains 1,748 active `#split` entries whose union contains 1,732,587 bytes. The other 364,565 bytes include assembled code, tables and alignment. For example, S02 has zero split entries and a documented battle engine. In S16, the item-127 and other-000 icon paths name the same bytes. Split filenames count separately in the entry column, while overlapping bytes count once within their section.

## Documented subranges

These entries record byte-level interpretations that can be followed back to a project owner. Their offsets nest inside the physical sections above. Boundary and purpose are **Confirmed** unless the purpose column says **Unknown**.

The [S08 map block and layout index](world/map-payload-ranges.md) lists 154 physical subranges in 77 pairs. Together they cover 193,678 stored bytes across 79 map IDs. Map24 shares the Map23 pair and Map46 shares the Map07 pair. The table below lists the other ten subranges.

| Stable ID | Start | End | Bytes | Purpose and evidence |
| --- | ---: | ---: | ---: | --- |
| <a id="s03-items"></a>S03-items | `0x016EA6` | `0x0176A6` | 2,048 | Item definitions, 128 records of 16 bytes. [ROM layout manifest](../../manifests/extractions/rom-static-layout.json) · [Source/ROM parity](party/static-core-data.md#rom-table-map) |
| <a id="s03-spells"></a>S03-spells | `0x0176A6` | `0x01796E` | 712 | Spell definitions, 89 records of 8 bytes. At the shared boundary the first spell owns the byte. [Source/ROM parity](party/static-core-data.md#rom-table-map) |
| <a id="s03-name-align"></a>S03-name-align | `0x017F3D` | `0x017F3E` | 1 | Assembly alignment between the item and class names. [Listing and adjacent table boundaries](party/static-core-data.md#rom-table-map) |
| <a id="s13-battle19-terrain"></a>S13-battle19-terrain | `0x1AEB5A` | `0x1AED3C` | 482 | `BattleTerrain19`, Stack-compressed battle terrain. The loader expands it to 48×48 one-byte cells. [Decode parity](battle/battle-routing-data.md#static-shape) · [Battle19 map and terrain selection](chapters/southern-travel-creed/battle19-initialization-and-first-control.md#map68-selection-and-fresh-versus-saved-lifetimes) |
| <a id="s13-battle19-spriteset"></a>S13-battle19-spriteset | `0x1B46BE` | `0x1B482C` | 366 | `BattleSpriteset19`, four count bytes followed by 12 ally templates and 15 enemy records (12 bytes each), three AI regions (12 bytes each) and one two-byte AI point. [Formation and placement](chapters/southern-travel-creed/battle19-initialization-and-first-control.md#twelve-templates-and-the-actual-party) · [Victory rule](chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md#end-checks-and-firstrepeated-outcomes) |
| <a id="s15-table"></a>S15-table | `0x1C8004` | `0x1C80E4` | 224 | `pt_Portraits`, 56 four-byte pointers. Slots 35 and 53–55 reuse other targets. [Pointer address and aliases](../../tests/fixtures/h2/portrait-graphics-decode-v1.json) |
| <a id="s15-payloads"></a>S15-payloads | `0x1C80E4` | `0x1D7E26` | 64,834 | 52 adjacent stored containers, portrait00 through portrait52 with alias slots excluded. Use the split lookup below for any member boundary. The 106,496 decoded tile bytes live outside ROM offset space. [Container format and parity](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s17-ally-tail"></a>S17-ally-tail | `0x1EE884` | `0x1EE890` | 12 | **Unknown** gameplay purpose. Slots 30 and 31 are physically present at `start + 30 × 6` in the 32-record ally start table. Their status as reserved, reachable or padding remains open. [Canonical entities](party/static-core-data.md#canonical-entities) |
| <a id="s18-bank1"></a>S18-bank1 | `0x1F0000` | `0x1F8000` | 32,768 | First stored music bank. [Bank 1 parity and selection](presentation/sound-data-inventory.md#static-bank-parity) |
| <a id="s18-bank0"></a>S18-bank0 | `0x1F8000` | `0x200000` | 32,768 | Second stored music bank. Both banks use Z80 origin `0x8000`. Convert a bank-local address by adding its displacement from that origin to the selected physical start. [Separate address space](presentation/sound-data-inventory.md#separate-address-space) |

Across both tables, 164 disjoint subranges cover 327,893 bytes. Of these, 327,881 have an identified static purpose and 12 have an explicit semantic question. The remaining 1,769,259 bytes have a section assignment and await finer classification **in this ledger**. Existing subsystem owners may already explain them. Behavior coverage has no byte denominator. No runtime observation is attributed to these rows here. Source-file and runtime evidence keep their own denominators in [source coverage](source-coverage.md#what-covered-means).

## Map68 and Battle19

<a id="s08-map68-layout"></a>[S08-map68-layout](world/map-payload-ranges.md#s08-map68-layout) is the stored `Map68s1_Layout` interval in the S08 index. Its decoded grid supplies the walking-zone and village-warp markers below.

**Confirmed** for the pinned USA image. The map, terrain and formation pointer tables store four-byte big-endian ROM addresses. Map slot 68 (`pt_MapData + 4 × 68`, file offset `0x094C9A`) selects the Map68 header at `0x0BDA0A`. Its layout pointer begins at header offset **10**, after six palette/tileset bytes and the block pointer. The unused `MAPDATA_OFFSET_LAYOUT` enum gives 8. The loader and retail header agree on 10. Among the 79 maps, only Map68 selects this layout. [Pinned map entries](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries.asm#L1458-L1478) · [Header and loader parity](world/map-content.md#complete-static-boundary)

| First and last stored byte | At the exclusive end | Boundary evidence |
| --- | --- | --- |
| `0x0BE05E`, `0x0BE60D` → S08-map68-layout | `0x0BE60E` starts `Map68s9_Animations`. | The [split lookup](#look-up-a-split-file) returns the Map68 `1-layout.bin` path for both bytes. The preceding byte at `0x0BE05D` belongs to `0-blocks.bin`. H1 labels the following animation table. |
| `0x1AEB5A`, `0x1AED3B` → S13-battle19-terrain | `0x1AED3C` starts Battle20 terrain. | Slot 19 at `0x1AD150` in [pt_BattleTerrainData](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/terrainentries.asm) agrees with the `battle19/terrain.bin` split. Slots 4 and 32 reuse 3 and 27 respectively. Only 19 selects this target. |
| `0x1B46BE`, `0x1B482B` → S13-battle19-spriteset | `0x1B482C` starts the Battle20 header. | Slot 19 at `0x1B313A` in [pt_BattleSpritesets](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/spritesets/entries.asm) is the sole pointer to this assembled formation. H1 labels both headers. The [unused binary aggregate](battle/battle-routing-data.md#build-boundary) repeats the symbol names and contributes zero additional bytes. |

To repeat the pointer and marker lookup, load the [local input configuration](../operations/local-private-inputs.md), set `$rom` to the verified USA file and run this from the repository root. For each target the query prints its file offset and every slot sharing it.

```powershell
@'
from pathlib import Path
from struct import unpack_from
import sys
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout

rom = Path(sys.argv[1]).read_bytes()
def ptr(offset):
    return unpack_from(">I", rom, offset)[0]

headers = [ptr(0x094B8A + 4*i) for i in range(79)]
layouts = [ptr(h + 10) for h in headers]
terrain = [ptr(0x1AD104 + 4*i) for i in range(45)]
formations = [ptr(0x1B30EE + 4*i) for i in range(45)]
for targets, slot in [(layouts, 68), (terrain, 19), (formations, 19)]:
    print(hex(targets[slot]), [i for i, target in enumerate(targets) if target == targets[slot]])

h = headers[68]
blocks, *_ = decode_map_blocks(rom[ptr(h + 6):ptr(h + 10)])
words, *_ = decode_map_layout(rom[ptr(h + 10):ptr(h + 42)], len(blocks) // 9)
for x, y in [(47, 29), (48, 30)]:
    print(x, y, hex(words[64*y + x] & 0x3C00))
'@ | uv run python -X utf8 - $rom
```

At `(47,29)` the decoded marker is `0x1400`. Walking opcode 2 can request event 6 here. Map68's default zone callback selects battle 19. With F519 clear it sets F419 and requests a reload. On reload, `CheckBattle` admits the F419-enabled Map68 row with wildcard trigger coordinates. The row's `(5,19)` origin and 48×32 size describe the battle crop. Driving opcode 7 has no zone request. At `(48,30)` the marker is `0x1000`. The pointer at header offset 30 selects `Map68s6_WarpEvents` at `0x0BDA5E`. Its zero-based row 2 supplies Map23 `(22,29)`, facing Right, independently of F519. [Walking and reload](chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md#walking-zone-callback-and-battle19-admission) · [Village warp](chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md#independent-map68-entrance-to-map23)

The formation gives enemy slot 128 an Orc base and the LEADER AI command set. The defeated hook reads a separate leader-presence byte at `0x047CA1` (slot 19, `0xFF`). With Bowie alive and enemy 128 dead, it queues surviving enemies for removal from the placed count. The loop tests zero allies before zero enemies, so Bowie's death takes precedence. **Unknown** here are natural traversal of either fork and an actual Battle19 victory in the original. [End checks and outcomes](chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md#end-checks-and-firstrepeated-outcomes)

## Look up a split file

In PowerShell, set `$upstream` to the local SF2DISASM checkout and `$offset` to a ROM file offset. Read the pinned Git object so local changes cannot alter the result. At `0x1C80E4` this prints the first portrait payload. At `0x0176A6` no split matches: use S03-spells above. A 68k listing address can be used directly only for a symbol mapped in this ROM image. RAM addresses, banked Z80 addresses and offsets in decoded assets require their own mapping.

```powershell
$offset = 0x1C80E4
$pin = 'c834c652b6862bc5679fd7f69a38a7093206efc6'
git -C $upstream show ($pin + ':split/sf2splits.txt') | ForEach-Object {
    if ($_ -match '^#split\s+(0x[0-9a-f]+),(0x[0-9a-f]+),(.+)$') {
        if ([int]$Matches[1] -le $offset -and $offset -lt [int]$Matches[2]) { $_ }
    }
}
```

For a boundary disagreement, compare the vanilla layout, active split directive and H1 listing or ROM-parity owner. An approximate free-space comment alone cannot classify a byte as padding. Keep conflicting claims and their source separate until the endpoints are resolved. Add a disjoint child row when its boundary and evidence owner can be named. Put pointers, aliases and decoded views in that row's explanation. Keep existing IDs linkable when subdividing. Track unresolved research by block ID in [the ROM inventory epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1819).
