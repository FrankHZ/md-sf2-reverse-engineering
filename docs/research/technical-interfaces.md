# Technical Jump Interfaces and Pointer Tables

- Status: **Confirmed** for all 25 layout-owned files, representative H1 addresses, 331 jump-stub
  mappings, and 60 longword-pointer mappings
- Status: **Unknown** only for the behavior of mapped targets not covered by their owning subsystem;
  the interface structure itself has no runtime ambiguity
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Routing Boundary

The ten `code/common/tech/jumpinterfaces` files and fifteen `code/common/tech/pointers` files are all
directly included by the pinned layout. Together they contain 3,575 lines and 391 global labels. The
large line count is mostly one-entry interface blocks and comments: the executable/data surface is
exactly 331 PC-relative jump stubs plus 60 longword pointers.

The jump interfaces span ROM sections 2, 3, 4, 5, 6, 7, and 13. Of the 331 stubs, 326 use the `j_`
prefix and five retain older `sub_` names. Every global entry in these files is verified as one
PC-relative `jmp` with a named target. The complete source/target mapping is included in canonical
generated output and protected by its hash.

The research-index join owns source-root membership, not a `tech.interfaces` ID or subsystem
prefix. It therefore retains all 25 historical interface records and the `tech.services`
`thinking-rng-alias` record: the latter shares `s13_jumpinterface.asm` with the section-13 routing
record. The complete association is 26 ordered records over the unchanged 25 ordered source paths;
the fixture owns those IDs, paths, and their per-source relation.

The pointer files span sections 2, 3, 6, 8, 10, 11, 12, 13, 14, 15, 16, and 17. Every global entry is
one `dc.l` target. These tables connect names, definitions, text banks, menus, maps, backgrounds,
battle sprites, ending resources, portraits, icons, growth data, and other section-owned content.
The inventory proves routing identity, not the semantic correctness of every destination asset. The
section-6 text-bank pointer now also has a deeper owner that verifies all 17 targets, the 68-byte
pointer table, and complete 4,267-record static decode.

## Verification Boundary

No emulator questions are required for this slice. Cross-section routing is fully determined by the
pinned source, H1 listing, canonical mapping hash, and per-file entry bindings. Target behavior remains
owned by the corresponding gameplay, data, or presentation rail.

## Reproduction

```powershell
uv run sf2 h2 tech-interfaces
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-interfaces-static.json`.
