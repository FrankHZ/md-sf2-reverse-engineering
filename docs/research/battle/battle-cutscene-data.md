# Battle Cutscene Data Inventory

- Status: **Confirmed** for the complete 61-file directory, the 59 built cutscenes and H1 addresses,
  the include container, the orphan source, file-type counts, and command-shape inventory
- Status: **Inferred** for presentation and cross-system story effects
- Status: **Unknown** for three grouped runtime/research questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Build Boundary

`data/battles/entries` contains 61 ASM files and 5,982 lines. The original layout directly includes
the unlabeled, byte-free `battlecutscenesstorage.asm` container; that container transitively includes
59 labeled cutscene files. Each of those 59 has a representative H1 address.

Two files cannot honestly count toward strict symbol reach:

- the storage file owns the include graph but emits no label or bytes;
- `battle01/cs_regiontriggered_1.asm` defines `rbcs_battle01` but is referenced by neither the original
  layout nor the storage container, so it is hashed as an orphan rather than assigned a borrowed H1
  address.

This produces 61/61 H2 inventory and 59/61 strict indexed-file reach.

## Static Shape, Not Story Completion

The 59 built files cover 34 battle indexes: 27 before-battle, 25 after-battle, three battle-end, one
battle-start, and three region-triggered cutscenes. Their 5,672 executable macro statements use 87
distinct command names, and every built file reaches `csc_end`. The most frequent command families
are text advance/wait, facing/position changes, action scripts, and entity actions.

This is deliberately a structure inventory. It does not claim that dialogue, choreography, map
mutations, flags, joins, deaths, or story meaning have been reconstructed. The earlier
`battle-cutscenes` H2 rail remains the owner for admission/routing code; this batch supplies the data
side and exact source-to-ROM provenance needed before story contracts are written.

Generated command/file detail stays under ignored
`local/derived/battle-cutscene-data-static.json`. The tracked fixture contains only addresses,
aggregate command counts, type counts, and hashes.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. provenance and possible historical reachability of the orphan Battle 01 region cutscene;
2. cutscene command timing and presentation;
3. story side effects crossing map, entity, force, flag, and battle state.

## Reproduction

```powershell
uv run sf2 h2 battle-cutscene-data
uv run sf2 research-index test
```
