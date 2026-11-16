# S08 map block and layout ranges

Find a map number in the first column to read both of its stored ROM ranges. For a file offset, find the interval whose start is at or below the offset and whose end is above it. Rows follow physical address order. The first map in a row owns the split files, and every map in that row selects both files. For offsets between pairs, continue in the [S08 ledger](../rom-block-ledger.md#s08).

**Confirmed.** The blocks column identifies the compressed definitions of 3×3-tile blocks. The layout column identifies the compressed 64×64 grid of words that select those blocks and carry map flags. Lengths count stored bytes. Each payload has a stable ID of the form `S08-mapNN-blocks` or `S08-mapNN-layout`, using the two-digit owner number and a lowercase link fragment. For example, Map46 selects [Map07's block definitions](#s08-map07-blocks) and [Map07's layout](#s08-map07-layout). [Map header and loader](map-content.md#complete-static-boundary) · [Decode and ROM parity](map-content.md#private-binary-payloads)

| Selecting map IDs | Blocks [start, end) | Bytes | Layout [start, end) | Bytes |
| --- | --- | ---: | --- | ---: |
| Map00 | <a id="s08-map00-blocks"></a>`[0x094D4C, 0x0959B0)` | 3,172 | <a id="s08-map00-layout"></a>`[0x0959B0, 0x095E86)` | 1,238 |
| Map01 | <a id="s08-map01-blocks"></a>`[0x095F92, 0x0965E6)` | 1,620 | <a id="s08-map01-layout"></a>`[0x0965E6, 0x0968C2)` | 732 |
| Map02 | <a id="s08-map02-blocks"></a>`[0x096A8A, 0x097458)` | 2,510 | <a id="s08-map02-layout"></a>`[0x097458, 0x0977D0)` | 888 |
| Map03 | <a id="s08-map03-blocks"></a>`[0x097946, 0x0984E8)` | 2,978 | <a id="s08-map03-layout"></a>`[0x0984E8, 0x09898A)` | 1,186 |
| Map04 | <a id="s08-map04-blocks"></a>`[0x098A46, 0x0993E0)` | 2,458 | <a id="s08-map04-layout"></a>`[0x0993E0, 0x09965A)` | 634 |
| Map05 | <a id="s08-map05-blocks"></a>`[0x099758, 0x099F90)` | 2,104 | <a id="s08-map05-layout"></a>`[0x099F90, 0x09A300)` | 880 |
| Map06 | <a id="s08-map06-blocks"></a>`[0x09A422, 0x09AB8A)` | 1,896 | <a id="s08-map06-layout"></a>`[0x09AB8A, 0x09AF6C)` | 994 |
| Map07, Map46 | <a id="s08-map07-blocks"></a>`[0x09B014, 0x09B7F8)` | 2,020 | <a id="s08-map07-layout"></a>`[0x09B7F8, 0x09BAC2)` | 714 |
| Map08 | <a id="s08-map08-blocks"></a>`[0x09BBEA, 0x09C454)` | 2,154 | <a id="s08-map08-layout"></a>`[0x09C454, 0x09C742)` | 750 |
| Map09 | <a id="s08-map09-blocks"></a>`[0x09C874, 0x09D1F0)` | 2,428 | <a id="s08-map09-layout"></a>`[0x09D1F0, 0x09D660)` | 1,136 |
| Map10 | <a id="s08-map10-blocks"></a>`[0x09D858, 0x09E0B0)` | 2,136 | <a id="s08-map10-layout"></a>`[0x09E0B0, 0x09E45A)` | 938 |
| Map11 | <a id="s08-map11-blocks"></a>`[0x09E4EA, 0x09EB9E)` | 1,716 | <a id="s08-map11-layout"></a>`[0x09EB9E, 0x09ED80)` | 482 |
| Map12 | <a id="s08-map12-blocks"></a>`[0x09EDE6, 0x09EFF4)` | 526 | <a id="s08-map12-layout"></a>`[0x09EFF4, 0x09F1AA)` | 438 |
| Map13 | <a id="s08-map13-blocks"></a>`[0x09F27E, 0x09F882)` | 1,540 | <a id="s08-map13-layout"></a>`[0x09F882, 0x09FB38)` | 694 |
| Map14 | <a id="s08-map14-blocks"></a>`[0x09FC5C, 0x0A02B4)` | 1,624 | <a id="s08-map14-layout"></a>`[0x0A02B4, 0x0A057E)` | 714 |
| Map15 | <a id="s08-map15-blocks"></a>`[0x0A0656, 0x0A11DA)` | 2,948 | <a id="s08-map15-layout"></a>`[0x0A11DA, 0x0A1516)` | 828 |
| Map16 | <a id="s08-map16-blocks"></a>`[0x0A1682, 0x0A280E)` | 4,492 | <a id="s08-map16-layout"></a>`[0x0A280E, 0x0A2DD0)` | 1,474 |
| Map17 | <a id="s08-map17-blocks"></a>`[0x0A2F38, 0x0A36FC)` | 1,988 | <a id="s08-map17-layout"></a>`[0x0A36FC, 0x0A3952)` | 598 |
| Map18 | <a id="s08-map18-blocks"></a>`[0x0A3A30, 0x0A41AA)` | 1,914 | <a id="s08-map18-layout"></a>`[0x0A41AA, 0x0A45B2)` | 1,032 |
| Map19 | <a id="s08-map19-blocks"></a>`[0x0A4698, 0x0A5076)` | 2,526 | <a id="s08-map19-layout"></a>`[0x0A5076, 0x0A534A)` | 724 |
| Map20 | <a id="s08-map20-blocks"></a>`[0x0A5438, 0x0A5E9C)` | 2,660 | <a id="s08-map20-layout"></a>`[0x0A5E9C, 0x0A61A4)` | 776 |
| Map21 | <a id="s08-map21-blocks"></a>`[0x0A620E, 0x0A63C4)` | 438 | <a id="s08-map21-layout"></a>`[0x0A63C4, 0x0A6438)` | 116 |
| Map22 | <a id="s08-map22-blocks"></a>`[0x0A64A6, 0x0A6B92)` | 1,772 | <a id="s08-map22-layout"></a>`[0x0A6B92, 0x0A6E1A)` | 648 |
| Map23, Map24 | <a id="s08-map23-blocks"></a>`[0x0A6F12, 0x0A7B26)` | 3,092 | <a id="s08-map23-layout"></a>`[0x0A7B26, 0x0A81D4)` | 1,710 |
| Map25 | <a id="s08-map25-blocks"></a>`[0x0A831E, 0x0A893C)` | 1,566 | <a id="s08-map25-layout"></a>`[0x0A893C, 0x0A8C58)` | 796 |
| Map26 | <a id="s08-map26-blocks"></a>`[0x0A8CF0, 0x0A915E)` | 1,134 | <a id="s08-map26-layout"></a>`[0x0A915E, 0x0A9384)` | 550 |
| Map27 | <a id="s08-map27-blocks"></a>`[0x0A943C, 0x0A995A)` | 1,310 | <a id="s08-map27-layout"></a>`[0x0A995A, 0x0A9C00)` | 678 |
| Map28 | <a id="s08-map28-blocks"></a>`[0x0A9C82, 0x0AA3A2)` | 1,824 | <a id="s08-map28-layout"></a>`[0x0AA3A2, 0x0AA898)` | 1,270 |
| Map29 | <a id="s08-map29-blocks"></a>`[0x0AA9D2, 0x0AAF78)` | 1,446 | <a id="s08-map29-layout"></a>`[0x0AAF78, 0x0AB332)` | 954 |
| Map30 | <a id="s08-map30-blocks"></a>`[0x0AB3AE, 0x0AB564)` | 438 | <a id="s08-map30-layout"></a>`[0x0AB564, 0x0AB610)` | 172 |
| Map31 | <a id="s08-map31-blocks"></a>`[0x0AB6F2, 0x0ABF98)` | 2,214 | <a id="s08-map31-layout"></a>`[0x0ABF98, 0x0AC32C)` | 916 |
| Map32 | <a id="s08-map32-blocks"></a>`[0x0AC3A6, 0x0AC554)` | 430 | <a id="s08-map32-layout"></a>`[0x0AC554, 0x0AC642)` | 238 |
| Map33 | <a id="s08-map33-blocks"></a>`[0x0AC748, 0x0AD04C)` | 2,308 | <a id="s08-map33-layout"></a>`[0x0AD04C, 0x0AD410)` | 964 |
| Map34 | <a id="s08-map34-blocks"></a>`[0x0AD4B6, 0x0AD8D6)` | 1,056 | <a id="s08-map34-layout"></a>`[0x0AD8D6, 0x0ADA40)` | 362 |
| Map35 | <a id="s08-map35-blocks"></a>`[0x0ADB2C, 0x0ADDC6)` | 666 | <a id="s08-map35-layout"></a>`[0x0ADDC6, 0x0ADF6E)` | 424 |
| Map36 | <a id="s08-map36-blocks"></a>`[0x0AE08A, 0x0AEA3E)` | 2,484 | <a id="s08-map36-layout"></a>`[0x0AEA3E, 0x0AEE9E)` | 1,120 |
| Map37 | <a id="s08-map37-blocks"></a>`[0x0AEF04, 0x0AF894)` | 2,448 | <a id="s08-map37-layout"></a>`[0x0AF894, 0x0AFBE8)` | 852 |
| Map38 | <a id="s08-map38-blocks"></a>`[0x0AFD2C, 0x0B0446)` | 1,818 | <a id="s08-map38-layout"></a>`[0x0B0446, 0x0B0736)` | 752 |
| Map39 | <a id="s08-map39-blocks"></a>`[0x0B07B0, 0x0B097C)` | 460 | <a id="s08-map39-layout"></a>`[0x0B097C, 0x0B0A68)` | 236 |
| Map40 | <a id="s08-map40-blocks"></a>`[0x0B0AD6, 0x0B1216)` | 1,856 | <a id="s08-map40-layout"></a>`[0x0B1216, 0x0B1490)` | 634 |
| Map41 | <a id="s08-map41-blocks"></a>`[0x0B1552, 0x0B1914)` | 962 | <a id="s08-map41-layout"></a>`[0x0B1914, 0x0B1A62)` | 334 |
| Map42 | <a id="s08-map42-blocks"></a>`[0x0B1B5E, 0x0B1DCC)` | 622 | <a id="s08-map42-layout"></a>`[0x0B1DCC, 0x0B1F74)` | 424 |
| Map43 | <a id="s08-map43-blocks"></a>`[0x0B2006, 0x0B22BA)` | 692 | <a id="s08-map43-layout"></a>`[0x0B22BA, 0x0B23A0)` | 230 |
| Map44 | <a id="s08-map44-blocks"></a>`[0x0B246E, 0x0B29EA)` | 1,404 | <a id="s08-map44-layout"></a>`[0x0B29EA, 0x0B2BFE)` | 532 |
| Map45 | <a id="s08-map45-blocks"></a>`[0x0B2CE0, 0x0B339C)` | 1,724 | <a id="s08-map45-layout"></a>`[0x0B339C, 0x0B37B2)` | 1,046 |
| Map47 | <a id="s08-map47-blocks"></a>`[0x0B38B2, 0x0B47EC)` | 3,898 | <a id="s08-map47-layout"></a>`[0x0B47EC, 0x0B4AA4)` | 696 |
| Map48 | <a id="s08-map48-blocks"></a>`[0x0B4B5A, 0x0B4F08)` | 942 | <a id="s08-map48-layout"></a>`[0x0B4F08, 0x0B5186)` | 638 |
| Map49 | <a id="s08-map49-blocks"></a>`[0x0B5326, 0x0B53EE)` | 200 | <a id="s08-map49-layout"></a>`[0x0B53EE, 0x0B55AE)` | 448 |
| Map50 | <a id="s08-map50-blocks"></a>`[0x0B5668, 0x0B5952)` | 746 | <a id="s08-map50-layout"></a>`[0x0B5952, 0x0B5BEC)` | 666 |
| Map51 | <a id="s08-map51-blocks"></a>`[0x0B5C52, 0x0B5F4A)` | 760 | <a id="s08-map51-layout"></a>`[0x0B5F4A, 0x0B607E)` | 308 |
| Map52 | <a id="s08-map52-blocks"></a>`[0x0B60E8, 0x0B6984)` | 2,204 | <a id="s08-map52-layout"></a>`[0x0B6984, 0x0B6C2E)` | 682 |
| Map53 | <a id="s08-map53-blocks"></a>`[0x0B6C9C, 0x0B6F84)` | 744 | <a id="s08-map53-layout"></a>`[0x0B6F84, 0x0B718A)` | 518 |
| Map54 | <a id="s08-map54-blocks"></a>`[0x0B7204, 0x0B75BA)` | 950 | <a id="s08-map54-layout"></a>`[0x0B75BA, 0x0B78A2)` | 744 |
| Map55 | <a id="s08-map55-blocks"></a>`[0x0B7932, 0x0B7AC0)` | 398 | <a id="s08-map55-layout"></a>`[0x0B7AC0, 0x0B7CA0)` | 480 |
| Map56 | <a id="s08-map56-blocks"></a>`[0x0B7DA6, 0x0B827C)` | 1,238 | <a id="s08-map56-layout"></a>`[0x0B827C, 0x0B85B6)` | 826 |
| Map57 | <a id="s08-map57-blocks"></a>`[0x0B8628, 0x0B8922)` | 762 | <a id="s08-map57-layout"></a>`[0x0B8922, 0x0B89D2)` | 176 |
| Map58 | <a id="s08-map58-blocks"></a>`[0x0B8A32, 0x0B9156)` | 1,828 | <a id="s08-map58-layout"></a>`[0x0B9156, 0x0B9350)` | 506 |
| Map59 | <a id="s08-map59-blocks"></a>`[0x0B93BA, 0x0B9B68)` | 1,966 | <a id="s08-map59-layout"></a>`[0x0B9B68, 0x0B9E38)` | 720 |
| Map60 | <a id="s08-map60-blocks"></a>`[0x0BA00E, 0x0BA80E)` | 2,048 | <a id="s08-map60-layout"></a>`[0x0BA80E, 0x0BABA2)` | 916 |
| Map61 | <a id="s08-map61-blocks"></a>`[0x0BAC14, 0x0BAD4E)` | 314 | <a id="s08-map61-layout"></a>`[0x0BAD4E, 0x0BADAC)` | 94 |
| Map62 | <a id="s08-map62-blocks"></a>`[0x0BAE12, 0x0BAF28)` | 278 | <a id="s08-map62-layout"></a>`[0x0BAF28, 0x0BAFB0)` | 136 |
| Map63 | <a id="s08-map63-blocks"></a>`[0x0BB066, 0x0BB36C)` | 774 | <a id="s08-map63-layout"></a>`[0x0BB36C, 0x0BB6B2)` | 838 |
| Map64 | <a id="s08-map64-blocks"></a>`[0x0BB70E, 0x0BB79E)` | 144 | <a id="s08-map64-layout"></a>`[0x0BB79E, 0x0BB8D8)` | 314 |
| Map65 | <a id="s08-map65-blocks"></a>`[0x0BB932, 0x0BBC5A)` | 808 | <a id="s08-map65-layout"></a>`[0x0BBC5A, 0x0BBD1A)` | 192 |
| Map66 | <a id="s08-map66-blocks"></a>`[0x0BBDD8, 0x0BCB9A)` | 3,522 | <a id="s08-map66-layout"></a>`[0x0BCB9A, 0x0BD1F0)` | 1,622 |
| Map67 | <a id="s08-map67-blocks"></a>`[0x0BD294, 0x0BD746)` | 1,202 | <a id="s08-map67-layout"></a>`[0x0BD746, 0x0BD9F4)` | 686 |
| Map68 | <a id="s08-map68-blocks"></a>`[0x0BDA94, 0x0BE05E)` | 1,482 | <a id="s08-map68-layout"></a>`[0x0BE05E, 0x0BE60E)` | 1,456 |
| Map69 | <a id="s08-map69-blocks"></a>`[0x0BE6C6, 0x0BEF8E)` | 2,248 | <a id="s08-map69-layout"></a>`[0x0BEF8E, 0x0BF75C)` | 1,998 |
| Map70 | <a id="s08-map70-blocks"></a>`[0x0BF7E8, 0x0C004A)` | 2,146 | <a id="s08-map70-layout"></a>`[0x0C004A, 0x0C062A)` | 1,504 |
| Map71 | <a id="s08-map71-blocks"></a>`[0x0C06C2, 0x0C1360)` | 3,230 | <a id="s08-map71-layout"></a>`[0x0C1360, 0x0C1B8C)` | 2,092 |
| Map72 | <a id="s08-map72-blocks"></a>`[0x0C1C54, 0x0C2730)` | 2,780 | <a id="s08-map72-layout"></a>`[0x0C2730, 0x0C2EE2)` | 1,970 |
| Map73 | <a id="s08-map73-blocks"></a>`[0x0C2F88, 0x0C32F0)` | 872 | <a id="s08-map73-layout"></a>`[0x0C32F0, 0x0C3618)` | 808 |
| Map74 | <a id="s08-map74-blocks"></a>`[0x0C36CC, 0x0C4028)` | 2,396 | <a id="s08-map74-layout"></a>`[0x0C4028, 0x0C486A)` | 2,114 |
| Map75 | <a id="s08-map75-blocks"></a>`[0x0C4922, 0x0C4D8C)` | 1,130 | <a id="s08-map75-layout"></a>`[0x0C4D8C, 0x0C4F2E)` | 418 |
| Map76 | <a id="s08-map76-blocks"></a>`[0x0C4FB6, 0x0C57E8)` | 2,098 | <a id="s08-map76-layout"></a>`[0x0C57E8, 0x0C5D06)` | 1,310 |
| Map77 | <a id="s08-map77-blocks"></a>`[0x0C5DA6, 0x0C6460)` | 1,722 | <a id="s08-map77-layout"></a>`[0x0C6460, 0x0C697A)` | 1,306 |
| Map78 | <a id="s08-map78-blocks"></a>`[0x0C6A12, 0x0C768C)` | 3,194 | <a id="s08-map78-layout"></a>`[0x0C768C, 0x0C7EB6)` | 2,090 |

## Reproduce a lookup

The pinned split paths are `data/maps/entries/mapNN/0-blocks.bin` and `1-layout.bin` in the same directory. Their starts match the big-endian pointers at offsets 6 and 10 of each selecting map's header in `pt_MapData`. H2 compares the header and payload bytes to the USA ROM and checks each decoded layout against its blockset. [Pinned split directives](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/split/sf2splits.txt) · [Pinned map entries](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries.asm) · [H2 decode manifest](../../../manifests/extractions/map-layout-decode.json)

With the [local inputs](../../operations/local-private-inputs.md) and H1 listing prepared, run the following from the project root. Both queries use the verified header targets. The first returns both payloads for Map46. The second returns the Map23 layout and its two selectors, Map23 and Map24. Use the ledger's [split lookup](../rom-block-ledger.md#look-up-a-split-file) at the same offset to check the stored file independently.

```powershell
. ./local/private-inputs.ps1
uv run sf2 h2 map-content
uv run sf2 h2 map-layouts
$report = Get-Content -Raw -LiteralPath local/derived/map-content-static.json | ConvertFrom-Json
$ranges = foreach ($p in $report.binaryPayloads) {
    $kind = $p.kind
    $selectors = $report.mapEntries | Where-Object { $_.pointers.$kind.address -eq $p.address }
    [pscustomobject]@{
        Maps = @($selectors.map)
        Path = $p.path
        Start = $p.address
        End = $p.address + $p.byteCount
    }
}
$ranges | Where-Object { 46 -in $_.Maps }
$offset = 0x0A7B26
$ranges | Where-Object { $_.Start -le $offset -and $offset -lt $_.End }
```

The first block starts at `0x094D4C`. At `0x0B097C`, Map39 switches from blocks to layout. The last layout includes `0x0C7EB5` and ends at `0x0C7EB6`. The [Map68 pointer and fork tiles](../rom-block-ledger.md#map68-and-battle19) give a worked example of interpreting a decoded layout.
