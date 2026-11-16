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
| <a id="s08"></a>S08 | `0x064000` | `0x0C8000` | 409,600 | 285 | 392,704 | [Map tiles, palettes, headers and map data](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/layout/sf2-08-0x064000-0x0C8000.asm) | [Map data inventory](world/map-data-inventory.md) |
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

| Stable ID | Start | End | Bytes | Purpose and evidence |
| --- | ---: | ---: | ---: | --- |
| <a id="s03-items"></a>S03-items | `0x016EA6` | `0x0176A6` | 2,048 | Item definitions, 128 records of 16 bytes. [ROM layout manifest](../../manifests/extractions/rom-static-layout.json) · [Source/ROM parity](party/static-core-data.md#rom-table-map) |
| <a id="s03-spells"></a>S03-spells | `0x0176A6` | `0x01796E` | 712 | Spell definitions, 89 records of 8 bytes. At the shared boundary the first spell owns the byte. [Source/ROM parity](party/static-core-data.md#rom-table-map) |
| <a id="s03-name-align"></a>S03-name-align | `0x017F3D` | `0x017F3E` | 1 | Assembly alignment between the item and class names. [Listing and adjacent table boundaries](party/static-core-data.md#rom-table-map) |
| <a id="s15-table"></a>S15-table | `0x1C8004` | `0x1C80E4` | 224 | `pt_Portraits`, 56 four-byte pointers. Slots 35 and 53–55 reuse other targets. [Pointer address and aliases](../../tests/fixtures/h2/portrait-graphics-decode-v1.json) |
| <a id="s15-payloads"></a>S15-payloads | `0x1C80E4` | `0x1D7E26` | 64,834 | 52 adjacent stored containers, portrait00 through portrait52 with alias slots excluded. Use the split lookup below for any member boundary. The 106,496 decoded tile bytes live outside ROM offset space. [Container format and parity](presentation/auxiliary-data-inventory.md#static-shape-and-copyright-boundary) |
| <a id="s17-ally-tail"></a>S17-ally-tail | `0x1EE884` | `0x1EE890` | 12 | **Unknown** gameplay purpose. Slots 30 and 31 are physically present at `start + 30 × 6` in the 32-record ally start table. Their status as reserved, reachable or padding remains open. [Canonical entities](party/static-core-data.md#canonical-entities) |
| <a id="s18-bank1"></a>S18-bank1 | `0x1F0000` | `0x1F8000` | 32,768 | First stored music bank. [Bank 1 parity and selection](presentation/sound-data-inventory.md#static-bank-parity) |
| <a id="s18-bank0"></a>S18-bank0 | `0x1F8000` | `0x200000` | 32,768 | Second stored music bank. Both banks use Z80 origin `0x8000`. Convert a bank-local address by adding its displacement from that origin to the selected physical start. [Separate address space](presentation/sound-data-inventory.md#separate-address-space) |

The eight listed subranges cover 133,367 bytes. Of these, 133,355 have an identified static purpose and 12 have an explicit semantic question. The remaining 1,963,785 bytes have a section assignment and await finer classification **in this ledger**. Existing subsystem owners may already explain them. Behavior coverage has no byte denominator. No runtime observation is attributed to these eight rows here. Source-file and runtime evidence keep their own denominators in [source coverage](source-coverage.md#what-covered-means).

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

For a boundary disagreement, compare the vanilla layout, active split directive and H1 listing or ROM-parity owner. An approximate free-space comment alone cannot classify a byte as padding. Keep conflicting claims and their source separate until the endpoints are resolved. Add a disjoint child row when its boundary and evidence owner can be named. Put pointers, aliases and decoded views in that row's explanation. Keep existing IDs linkable when subdividing. Start the next pass with the encounter and map data needed by Battle19 in S13 and S08. Track unresolved research by block ID in [the ROM inventory epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1819).
