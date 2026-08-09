# Party Membership State Contract

- Status: **Confirmed static party-membership state and bounded helper chronology**
- Evidence date: 2026-08-08
- Scope: implementation-neutral reconstruction of joined-force and active-party membership state,
  counted-prefix rebuilding, and the source-reviewed membership helpers, without importing map-script
  command behavior, persistence, UI, roster-choice policy, presentation, or balance meaning

## Judgment Boundary

This contract begins at the source-shaped party-state helpers in `battleparty.asm`. It does not define
how story scripts select members, how a player edits a roster, or when membership changes become
durable or visible.

- **Confirmed fixture-owned facts**: joined membership and active battle-party membership use separate
  flag ranges; `UpdateForce` builds joined-force, active-party, and reserve lists; `JoinForce`
  auto-activates below the source-defined force maximum; and `LeaveForce` moves the selected combatant
  off map.
- **Confirmed direct source review**: the six helper identities and their bounded instruction/call
  order; `UpdateForce` writes counted prefixes and their counts without an observed backing-tail clear;
  `JoinForce` sets joined membership, rebuilds, checks capacity, and conditionally calls
  `JoinBattleParty`; the three active-party helpers check, set, or clear the active flag; and the two
  leave helpers retain distinct source-shaped X writes.
- **Unknown**: numeric flag indexes, force capacity, ally cardinality, malformed-selector behavior,
  caller-visible condition-code meaning, automatic list synchronization after `JoinBattleParty`,
  `LeaveForce`, or `LeaveBattleParty`, normal-story reachability, death/status interaction, save/load
  persistence, map-script command outcomes, AI/follower behavior, UI and player roster-choice policy,
  presentation, and balance intent.

The [party and roster state contract](party-roster-state.md) owns the ten accepted map-script command
forms and their bounded handler-local runtime observations. The [global flag contract](global-flag-state.md)
owns generic flag-index addressing, and the [new-game initialization contract](new-game-state-initialization.md)
owns only its accepted initialization order edges. This contract owns the lower party-membership state
surface without borrowing those adjacent contracts' associations.

## Evidence Owner and Source Audit

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md).

The fixture binds `UpdateForce` at decimal address `39168` and owns exactly four
`expected.statsFacts.party` facts. Those four semantic facts are not silently expanded into a complete
runtime contract.

A separate read-only audit of pinned upstream commit
`c834c652b6862bc5679fd7f69a38a7093206efc6` reviewed
`code/common/stats/battleparty.asm` from `UpdateForce` through `LeaveBattleParty`. That direct source
review supplies the helper substructure and chronology below. It does not upgrade runtime
reachability, caller interpretation, persistence, or presentation to Confirmed evidence.

The active Issue #73 battle-functions aggregate and fixture `sf2-battle-functions-static-v1` are
deliberately excluded. Common scripting, map-script command records, new-game, generic flag,
combatant-state, save, menu, and gameflow records are also outside this contract's research-index
association boundary. The sole future association is `stats.party`.

## Membership Domains

**Confirmed static:** joined-force membership and active battle-party membership are separate flag
domains. A member can therefore be joined without being active. Reserve membership is not a third
persistent flag domain in the accepted source shape; `UpdateForce` derives the reserve counted prefix
from a joined member whose active flag is clear.

The contract uses symbolic flag ranges. It does not copy numeric flag bases, a member count, or a
capacity value into the implementation-neutral layer. Those values remain import data or explicitly
uncontracted evidence rather than hard-coded design meaning.

| Logical state | Accepted source role | Deliberate boundary |
| --- | --- | --- |
| joined membership | selects membership in the force/targets prefix | story availability and persistence are **Unknown** |
| active membership | separates active members from reserves | player choice, death rules, and capacity policy are **Unknown** |
| reserve classification | derived during `UpdateForce` when joined is set and active is clear | no independent reserve flag is claimed |

## Counted-Prefix Rebuild

**Confirmed static:** `UpdateForce` scans the source ally domain and rebuilds three counted prefixes:

1. every joined member is appended to the force/targets prefix;
2. a joined member with an active flag is appended to the active-party prefix;
3. a joined member without an active flag is appended to the reserve prefix;
4. the three resulting counts are written after the scan.

This establishes prefix contents and count writes for one completed static routine path. It does not
establish that unused bytes after any counted prefix are cleared. Consumers must treat the written
count as the boundary; stale backing-array tails must not be promoted into members.

The source scan order may determine prefix order, but this contract does not import numeric ally
cardinality or infer player-facing roster sorting. Concurrent mutation, interruption, invalid state,
and caller-visible partial rebuild behavior remain **Unknown**.

## `JoinForce` Chronology

The exact accepted source chronology is:

1. set the selected ally's joined flag;
2. call `UpdateForce`;
3. compare the rebuilt active-party count with the source-defined capacity constant;
4. when below that capacity, call `JoinBattleParty` to set the active flag.

The rebuild precedes the conditional active-flag set. Therefore this contract MUST NOT claim that the
active counted prefix or its written count already includes the newly activated member when
`JoinForce` returns. No second `UpdateForce` call appears in this helper. Whether a caller performs a
later rebuild, and when any list becomes synchronized with the new active flag, remain **Unknown**.

“Auto-activates below the force maximum” preserves the fixture-owned relationship and the source
branch order. It does not supply the numeric maximum, explain a full-party user experience, choose a
replacement member, or prove a caller-visible success result.

## Leave and Active-Party Helpers

Direct source review keeps these operations separate:

- `LeaveForce` clears joined membership and then writes the symbolic `MAP_NULLPOSITION` value through
  the combatant-X setter. It does not call `UpdateForce` in the reviewed helper.
- `IsInBattleParty` addresses the active flag and invokes the shared flag check. This contract retains
  the operation identity but does not create a new caller-visible Boolean or condition-code contract.
- `JoinBattleParty` sets the active flag. It does not rebuild any counted prefix in the reviewed
  helper.
- `LeaveBattleParty` clears the active flag and then writes the source literal `-1` through the
  combatant-X setter. It does not rebuild any counted prefix in the reviewed helper.

`MAP_NULLPOSITION` and the `LeaveBattleParty` literal `-1` remain distinct source identities. This
contract does not assume numeric equivalence, shared intent, or identical caller-visible behavior.
Likewise, the fixture phrase “moves combatant off map” is retained only for `LeaveForce`; the visible
or collision meaning of either X write belongs to map/combatant owners and future runtime evidence.

List and count synchronization after `JoinBattleParty`, `LeaveForce`, and `LeaveBattleParty` is
**Unknown**. A modern implementation may maintain stronger internal invariants, but original-fidelity
compatibility must expose the accepted chronology and must not pretend that the original helpers
performed an unobserved rebuild or backing-tail clear.

## Cross-System Separation

Party membership state is not the complete roster system:

- map-script roster/death commands retain their own stream layouts, handler timing, and H3 boundary;
- new-game initialization owns when its starting-member join operation is requested, not the final
  counted-list result;
- global flags own generic storage addressing rather than party lifecycle meaning;
- combatant state owns the low-level X setter surface rather than map visibility or collision;
- story recruitment, roster menus, party-capacity UX, save/load, AI/followers, death/revival, and
  battle admission require their own owners;
- presentation, localization, accessibility, and balance are deliberate design layers, not facts
  inferred from these static helpers.

## Implementation-Neutral State Model

```text
PartyMembershipState
  joinedFlags: symbolic joined-membership domain
  activeFlags: symbolic active-membership domain
  countedPrefixes:
    joinedMembers: { entries, count }
    activeMembers: { entries, count }
    reserveMembers: { entries, count }

rebuildMembershipPrefixes()
  scan source ally domain
  append joined members to joinedMembers
  partition joined members by active flag
  write all three counts
  backingTailClear: notContracted

joinForce(member)
  set joinedFlags[member]
  rebuildMembershipPrefixes()
  if activeMembers.count < sourceCapacity:
    joinBattleParty(member)
  postJoinActivePrefixSynchronization: unknown

leaveForce(member)
  clear joinedFlags[member]
  setCombatantX(member, symbolic MAP_NULLPOSITION)
  prefixSynchronization: unknown

isInBattleParty(member)
  perform activeFlags membership check
  callerVisibleInterpretation: unknown

joinBattleParty(member)
  set activeFlags[member]
  prefixSynchronization: unknown

leaveBattleParty(member)
  clear activeFlags[member]
  setCombatantX(member, source literal -1)
  prefixSynchronization: unknown
```

This is a logical parity model, not a required engine memory layout. A remake may use sets, stable
vectors, derived queries, transactions, or stronger synchronization internally. Its fidelity adapter
must still reproduce the two membership domains, counted-prefix rebuild boundary, exact `JoinForce`
chronology, and distinct leave-operation identities.

## Original Fidelity and Modernization

Original-fidelity mode preserves the four fixture-owned facts, the directly reviewed helper order,
and the representative `UpdateForce` identity/address. It reports caller, runtime, synchronization,
and persistence questions instead of treating comments or helper names as a complete player-facing
roster specification.

A remake may choose immediate list synchronization, dynamic capacity, explicit result types,
transactional roster edits, or a different roster UI. Those are explicit product decisions. If an
original-fidelity adapter keeps a stronger internal invariant, it must still emulate the accepted
observable ordering where downstream compatibility tests depend on the pre-activation rebuild.

Public parity fixtures need structural metadata, symbolic identities, and synthetic member indexes;
they do not require copyrighted names, dialogue, portraits, or other original assets.

## H4 Acceptance Gates

A future remake party-membership adapter passes this contract only when:

1. joined and active membership remain separate logical domains, while reserve membership is derived
   during the accepted rebuild rather than invented as a required third flag range;
2. `UpdateForce` reconstructs the three counted prefixes and writes their counts without requiring or
   claiming an unobserved backing-tail clear;
3. `JoinForce` preserves set-joined → rebuild → capacity-check → conditional-set-active chronology and
   does not assert that the rebuilt active prefix already contains the newly active member;
4. `LeaveForce` retains symbolic `MAP_NULLPOSITION`, while `LeaveBattleParty` retains its distinct
   literal `-1`, without assuming equivalence;
5. post-helper list synchronization, caller-visible results, malformed inputs, story reachability,
   death/status interaction, persistence, UI, presentation, and balance remain separately tested or
   explicitly **Unknown**;
6. adjacent map-script, flag, new-game, combatant, save, and gameflow contracts remain independently
   testable rather than being collapsed into this state layer;
7. public fixtures use structural metadata and synthetic values rather than copyrighted content.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| separate joined/active flags; three rebuilt lists; conditional auto-activation; `LeaveForce` off-map handoff | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Numeric constants, runtime results, caller meaning |
| six helper identities and bounded instruction/call order | **Confirmed static source review** | pinned `battleparty.asm` at upstream commit `c834c652b6862bc5679fd7f69a38a7093206efc6` | Runtime reachability and caller-visible semantics |
| counted-prefix contents and count writes, without backing-tail clear | **Confirmed static source review** | `UpdateForce` source body | Concurrent/invalid state and visible ordering |
| post-activation/post-leave synchronization | **Unknown** | Future grouped runtime/caller evidence | No implicit rebuild may be invented |
| story, save/load, roster UI, AI/followers, death/revival, presentation, balance | **Separate owner / Unknown** | Adjacent contracts and future synthesis/runtime work | Do not infer the complete roster experience |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
