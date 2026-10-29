# Map-Event Interaction State

- Status: **Confirmed** for the bounded source/H1/ROM state-flow relation.
- Evidence date: 2026-08-26.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-interaction-state-static-v1` joins only the two cross-owner fixed-RAM symbols left
outside the direct-state, dialogue-state, and request-state owners:
`ENTITY_FACING` at `$FFA912` and `EVENT_RELATIVE_POSITION` at `$FFB651`. It fresh-builds the retained
map-events, direct-state, direct-control, direct-handoff, predicate-results, map-setup, gameflow,
menus, battle-functions, and jump-interface owners before validating this projection.

## Confirmed Static Relation

The complete bounded source surface is 12 pinned source identities, seven function ranges, and seven
seam groups: 230 source operations / 792 H1-matched ROM bytes. The function ranges contribute
195 operations / 652 bytes: `GetActivatedEntity` `$2379A..$23844` (57/170),
`GetEntityEventIndex` `$22F4A..$22F76` (15/44), `GetPlayerEntityPosition`
`$22C60..$22C84` (9/36), `RunMapSetupItemEvent` `$47586..$4761A` (39/148),
`RunMapSetupEntityEvent` `$4761A..$476DC` (59/194), `GetRhodeFacing`
`$47832..$47856` (10/36), and `ProcessMapEventType6_ZoneEvent` `$25A7C..$25A94` (6/24).
The seven separately hashed seam groups contribute 35 operations / 140 bytes: the successful
`ProcessPlayerAction` entity path, field item invocation, four S07 jump stubs, the Map 9 Rhode caller,
five Map 6 compare/branch pairs, Map 9 Event 8's transform, and Map 28's wait/test/branch.

Exactly two byte writes target `EVENT_RELATIVE_POSITION`: the item-event store at `$47592` and the
entity-event store at `$4761E`, both from `d2`. In the static entity path, `GetActivatedEntity` returns
the player-facing source value in `d2` before the entity-event store. In the static item path,
`GetPlayerEntityPosition` returns the player Y tile in `d2` before the item-event store. These are
two source-distinct inputs to one byte; the contract deliberately rejects a universal meaning for that
location.

There are eight bounded byte reads: one `ENTITY_FACING` input in `GetPlayerEntityPosition`, one
`EVENT_RELATIVE_POSITION` input in `GetRhodeFacing`, five Map 6 immediate `#1` tests, and Map 9's
Event 8 transform into `d1`. The five Map 6 pairs preserve compare value, `bne` polarity, resolved
target, and lexical fallthrough. Map 28 separately performs its retained wait then tests
`ENTITY_FACING` before its `bne`; this bounded corpus has no `ENTITY_FACING` writer, so it does not
invent a writer, value, lifetime, or presentation effect. The four S07 stubs retain their instruction
targets and alias-aware effective targets, including both entity-event entries resolving to the same
effective entry.

The public fixture keeps only paths, symbols, numeric addresses/ranges, widths, registers, operands,
branch identities, counts, order, and hashes. It contains no source text, comments, ROM/H1 bytes,
captures, runtime services, or private paths. Its H1 listing evidence SHA-256 is the normalized UTF-8
listing value `F28FAF604DD8F37AE3EDAA819DD1C9A601863B0596F2C83602CA3D572BB8644D`; the private raw
input file-byte SHA remains separately immutable.

## Runtime Boundary

**Unknown:** `naturalEntityInteractionReachability`, `activatedEntityIdentity`, `entityFacingValue`,
`playerFacingValue`, `eventRelativePositionRuntimeValue`, `predicateBranchTaken`,
`itemInvocationReachability`, `itemPlayerYRuntimeValue`, `itemHandlerSelectionAndOutcome`,
`zoneInvocationReachability`, `zoneBranchAndScript`, and
`stateLifetimePersistenceTimingPresentationStoryMeaning`. This grouped H3 question queue makes no
runtime claim and does not authorize an emulator slice.

## Reproduction

```powershell
uv run sf2 h2 map-event-interaction-state
```
