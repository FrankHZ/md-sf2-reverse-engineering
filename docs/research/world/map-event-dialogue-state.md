# Map-Event Dialogue State

## Static Contract

**Confirmed:** `sf2-map-event-dialogue-state-static-v1` reconstructs the source/H1/ROM control-flow
and dialogue-state surface of the 24 positive programs selected from the complete 914-program
map-event corpus. It does not repeat the general map-event, direct-state, direct-control,
direct-handoff, sprite-dialogue, or text-service algorithms; those remain retained-owner joins.

The zero-inclusive source denominator is 24 positive and 890 zero programs over 3,579 operations.
The 24 selected contexts contain 414 contextual / 374 physical operations and 72 contextual / 64
physical labels. Their static shape is 278 ordinary operations, 40 conditional branches, 20
unconditional branches, 49 direct calls, one direct jump, and 26 returns. All 374 target-program
operation PCs, plus the five entity-dispatch/helper anchors, are source/H1/ROM checked (379 anchors).

Five source-defined fixed-state symbols are retained with identity, width, direction, and operand
form: `CURRENT_PORTRAIT`, `CURRENT_SPEECH_SFX`, `SPEECH_SFX_COPY`, `DIALOGUE_NAME_INDEX_1`, and
`MESSAGE_SPEED`. The selected programs have 100 contextual state-access edges at 72 physical
instruction PCs. Physical accounting is by unique instruction PC, while same-PC symbol-edge
identities remain checked so that deduplication cannot drop or conflate an access.

Twenty-three selected programs emit text and one is state-only. The text relation has 89 contextual
sites at 76 physical PCs: 69 numeric line references and 20 `clsTxt` close sentinels. The combined
state/text relation is 189 contextual sites at 148 unique physical PCs. Every text/close site and
every return carries may/must reaching-definition identities; an empty must set is valid at a merge,
while each may set remains nonempty.

The 17 table-owner joins are exact source/index identities. The source provenance set has 22 files:
the 17 selected event tables plus `sf2const.asm`, `sf2enums.asm`, `sf2macros.asm`, the map setup
dispatcher, and `GetEntityPortaitAndSpeechSfx`. Entity setup dispatch records the two source-ordered
portrait/SFX prefill writes and the helper lookup entry/call as static entry seeds. Map 6's
`Map6_DefaultEntityEvent` remains a standalone RTS entry; its shared physical suffix is analyzed only
under the true `Map6_EntityEvent13` entry. The fixture guards the suffix identity and labels and does
not invent a Default-to-tail edge.

Provenance: USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`,
`ShiningForceCentral/SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, H1 listing
`build/sf2build-h1.lst` SHA-256
`F28FAF604DD8F37AE3EDAA819DD1C9A601863B0596F2C83602CA3D572BB8644D`, and
`uv run sf2 h2 map-event-dialogue-state`. The public fixture contains structural identities,
PCs, numeric text IDs/sentinels, reaching-definition IDs, and hashes only; it contains no decoded
dialogue or private ROM/source bytes.

## Runtime Boundary

**Unknown:** no H3 claim is made for normal story program reachability, selected control-flow path,
entity lookup input identity, entity portrait lookup result, entity speech-SFX lookup result, zone/item
inherited entry state, actual text-service execution, actual displayed line order, dialogue-name
substitution value, portrait window visibility or placement, speech-SFX playback or timing, message
speed cadence, controller advance timing, post-return state lifetime or persistence, or story meaning.
These are one grouped runtime-question queue, not inferred presentation or execution facts.

## Reproduction

```powershell
uv run sf2 h2 map-event-dialogue-state
```
