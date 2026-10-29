# Map-Event Tactical-Base Quote State

Evidence date: 2026-08-27.

## Confirmed static caller corpus

**Confirmed:** the complete retained `sf2-map-events-static-v1` mother corpus has 914 program
contexts. Exactly 54 are positive callers of `DisplayTacticalBaseQuote`, leaving 860 zero contexts.
They occupy 54 ordered physical caller sites in two entity-event tables and comprise 108 source/H1
instruction rows / 432 bytes: 25 Map 37 callers and 29 Map 46 callers. Every caller consists exactly
of `moveq #ALLY_*,d0` followed by a tail `jmp DisplayTacticalBaseQuote`; no returning-call
continuation is inferred from that tail transfer. The public fixture keeps each positive context and
each physical site as a keyed ordered record rather than treating either table as a proxy site.

Map 37's positive set is `Map37_EntityEvent0` through `Map37_EntityEvent23`, then
`Map37_EntityEvent25`—not `Map37_EntityEvent24`. Map 46's set is `Map46_EntityEvent0` through
`Map46_EntityEvent28`. The two table entries are H1/ROM addresses `$5F86C` and `$5C0F8`; their
source table symbols are `ms_map37_EntityEvents` and `ms_map46_EntityEvents`. The 29 parsed
`ALLY_*` source enum selectors have the closed value domain 1 through 29.

## Confirmed callee and service shape

**Confirmed:** `DisplayTacticalBaseQuote` is the 16-statement, 16-H1-row, 58-byte function at
`$4790E..$47948` in `code/common/scripting/map/headquartersfunctions.asm`. It calls
`j_OpenNameUnderPortraitWindow`, `j_GetCurrentHp`, tests `d1` with `tst.w`, and uses a `bne.s` to
the living-member source block. Its fall-through sets `d0` to line ID 1. The living block copies
`d0` to `d1`, adds `FORCEMEMBER_ACTIVE_FLAGS_START` (32), then calls `j_CheckFlag`. The `beq.s`
fall-through reserve block adds `$DE1`; the other block adds `$DC3`. Both reach the ordered
`DisplayText`, `j_CloseNameUnderPortraitWindow`, and `rts` suffix. These are instruction, operand,
and branch-polarity facts only.

**Confirmed:** the public quote-line domain is exactly dead ID 1, active `$DC4..$DE0`, and reserve
`$DE2..$DFE`, for 59 unique line IDs. The fixture deliberately contains no decoded text.

**Confirmed:** the source/H1/ROM contract retains 124 owned source/H1 rows / 490 bytes plus nine
alias/effective service anchors, 133 anchors total. The fixture enumerates every anchor with only
structural instruction fields and hashes; it contains no raw source-statement prose or ROM bytes.
All 54 caller tail targets, the five callee service/text edges, and the four PC-relative jump-interface
targets resolve and fail closed before golden comparison. Alias-aware service pairs are
`j_OpenNameUnderPortraitWindow` `$100AC` → `OpenNameUnderPortraitWindow` `$169AE`,
`j_CloseNameUnderPortraitWindow` `$100B0` → `CloseNameUnderPortraitWindow` `$16A30`,
`j_GetCurrentHp` `$8048` → `GetCurrentHp` `$8336`, and `j_CheckFlag` `$8264` → `CheckFlag` `$98B4`;
the direct `DisplayText` entry is `$6260`. The contract guards raw-byte source identities,
structural source/H1 instruction shapes, ROM hashes, resolved branch/call/jump targets, and alias
targets before any golden comparison.

## Unknown runtime meaning

**Unknown:** this static rail does not establish natural program reachability, the selected map/caller,
the runtime ally selector or current HP, which branch is taken, current party-flag value, selected line
ID, or service completion. It also does not establish tail return/control order, state lifetime or
save/load persistence, input, dialogue/audio presentation timing, or story meaning. The grouped H3
queue is exactly the twelve `unknowns` keys in fixture
`sf2-map-event-tactical-base-quote-state-static-v1`; no runtime rail is authorized by this document.

## Provenance and reproduction

Pinned source: `ShiningForceCentral/SF2DISASM` `master`
`c834c652b6862bc5679fd7f69a38a7093206efc6`; local US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; H1 listing
`build/sf2build-h1.lst`. The complete ten-file source surface and source identities are fixture-pinned
in `tests/fixtures/h2/map-event-tactical-base-quote-state-static-v1.json`. Reproduce with:

```powershell
uv run sf2 h2 map-event-tactical-base-quote-state
```
