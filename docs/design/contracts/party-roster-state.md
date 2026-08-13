# Party and Roster State Contract

- **Confirmed original structure and bounded behavior:** ten source-named map-script command forms, their physical stream
  layouts, named handler branch/mutation/call order, source-site corpus, direct/effective caller
  identity, and provenance joins to the common-stats and follower-owner sources.
- **Inferred original behavior:** none in this contract.
- **Unknown original behavior:** normal-story reachability, roster/list capacity, player-visible
  roster/death outcomes, and physical/cross-process SRAM durability.
- Remake status: implementation-neutral Phase 3 contract; no engine has been selected.

## Import Boundary

A remake importer MUST retain the source forms `join`, `jumpIfDefeatedByLastAttack`, `jumpIfDead`,
`allyDefeated`, `updateDefeatedAllies`, `reviveAlly`, `joinBatParty`, `joinForceAI`,
`resetForceBattleStats`, and `addNewFollower` as distinct ordered commands. It MUST preserve
each command's original byte width, operand width and stream offset, source program identity, command
index, and raw operand text. It MUST retain both complete 304-row program-total corpora, including
zero rows and the two zero-use roster/death forms; a zero source-use count does not establish runtime
unreachability.

The importer MUST preserve macro and handler labels separately. In particular,
`jumpIfDefeatedByLastAttack` is the source macro label for handler
`csc0E_jumpIfForceMemberInList`; it MUST NOT be silently normalized into a new semantic command name.
`jumpIfDead` and `csc0F_jumpIfCharacterDead` are likewise separate source identities. Any engine-facing
name may be added only as an explicitly versioned remake decision, not substituted for these source
labels.

## Handler Boundary

The imported representation MUST preserve the following source-confirmed instruction structure:

- `join` has one word operand. Its section retains the bit-clear/`bne` music split, the selector test
  against the parsed `COMBATANT_ENEMIES_START` value, the two named special calls, and the ordinary
  `JoinForce`/`GetClass`/dialogue-index call and mutation order.
- `jumpIfDefeatedByLastAttack` and `jumpIfDead` each have a word followed by a long. Their conditional
  A6 transfer and four-byte skip are explicit stream outcomes, not a generic Boolean result.
- `allyDefeated` has one word and stores its byte before incrementing the source-named list length.
- `updateDefeatedAllies` has no operand. Its `cmpi.w #-1,d1; beq` skip means the source list write is
  on the non-equality fall-through path. A remake MUST retain that static branch fact but MUST NOT
  infer a user-visible definition of death from the adjacent source comment.
- `reviveAlly` has one word. Its equality path decrements the source-named length, while the
  non-equality path copies a byte and advances both pointers; no capacity or persistence policy is
  supplied by this rule.
- `joinBatParty` has one word. It retains the initial source `-1` write to
  `DIALOGUE_NAME_INDEX_1` before the membership test, the `BATTLE_PARTY_MEMBERS_NUMBER` read, the
  source `subq.w #2,d7` instruction, the current-HP zero branch, and the later replacement write
  before `LeaveBattleParty` then `JoinBattleParty`. These state-write/call order facts do not define a
  capacity or active/dead lifecycle.
- `joinForceAI` has two words. Its second-word `bne` polarity, clear/set uses of
  `AIBITFIELD_AI_CONTROLLED`, set-path-only `JoinForce` call, and common
  `SetActivationBitfield` tail are separate ordered facts. A remake MUST NOT replace the source macro
  label with an asserted “on/off” interpretation from its macro comment.
- `resetForceBattleStats` has no operands and retains the exact `ResetAlliesBattleStats` service call.
- `addNewFollower` has one word. It retains the `-1` scan sentinel, the last observed byte in `d1`,
  the fixed `$FFE8`/`0` `d2`/`d3` source arguments, and the final `AddFollower` call order; none of
  these register facts defines a follower lifecycle or visible effect.

The imported direct-call graph MUST retain instruction target and effective target separately. A
jump-interface alias is not erased: `j_JoinForce` remains the instruction target while `JoinForce`
is the resolved target. The connection to `sf2-common-stats-static-v1` is source provenance for
`code/common/stats/battleparty.asm` and its `JoinForce`/`UpdateForce` labels; it is not permission to
copy sibling fixture data into this contract. The active-party group additionally retains the
`GetActivationBitfield`/`SetActivationBitfield` owner paths, the `AddFollower` owner path, and the
`ResetAlliesBattleStats` owner path as provenance identities only.

## Evidence and Runtime Boundary

Evidence date: 2026-08-12.

Executable evidence is fixture ID `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field `forceStateCommandFacts`; its verifier is
`src/sf2tool/h2/map_script_engine.py`. It pins the upstream commit, US ROM hash, handler addresses,
full 304-program source-site/total corpus, section guards, caller maps, and common-stats provenance
identity. The nested `forceStateCommandFacts.activePartyCommandFacts` field pins the four additional
forms, their 29 sites, source-owner identities, and their own 304-row total corpus.

The roster/death matrix is fixture ID `sf2-force-state-roster-death-runtime-v1` at
`tests/fixtures/h3/force-state-roster-death-v1.json`; its verifier is
`src/sf2tool/h3/force_state_roster_death.py`. It confirms the fixed 14-case handler matrix: joined
membership absent/already-present; defeated-list empty/hit/miss; HP dead/live; defeated append;
offscreen/onscreen defeated updates; and revive empty/hit-first/hit-middle/miss. It further confirms
that joined membership and HP lie inside the original 4,016-byte `COMBATANT_DATA` SaveGame/LoadGame
domain, while `DEAD_COMBATANTS_LIST` and its length are outside it. Only the mutating absent-join case
performs original SaveGame, narrow joined-byte inverse poison, original LoadGame, and original
`CheckFlag`, and records the independently derived selected physical SRAM byte, checksum byte, and
`SAVE_FLAGS` occupied bit. The HP cases are saved-domain branch observations, not synthetic
Save/poison/Load evidence; list cases are handler-local scoped-restoration facts, not persistence claims. The
active-party matrix is fixture ID `sf2-force-state-active-party-runtime-v1` at
`tests/fixtures/h3/force-state-active-party-v1.json`; its verifier is
`src/sf2tool/h3/force_state_active_party.py`. It confirms bounded handler-local flag/list timing,
activation/join state, reset service order, and follower allocation/list effects; a remake MUST still
define normal-story reachability, capacity lifecycle, and player-visible presentation
explicitly through
`force-state/active-party-ai-follower/normal-story-reachability`,
`force-state/active-party-ai-follower/save-load-capacity-lifecycle`, and
`force-state/active-party-ai-follower/player-visible-presentation`.

For the bounded `updateDefeatedAllies` probe, the source/H1 loop performs 32 `GetCombatantX` checks.
The fixture snapshots the one seeded list byte plus its 32 possible write positions (33 bytes) solely
for restoration. This is not a derived list-capacity rule.

For fidelity within this bounded command surface, preserve the observable ordering rather than repairing
the list immediately: `UpdateForce` may leave a handler-local pre-replacement party snapshot while
membership flags have already changed. Preserve zero-selector no-join versus nonzero `JoinForce` behavior,
apply the reset status mask before the subsequent stat update, and preserve duplicate-follower allocation
and dynamic walking-parameter writes even when the follower list itself does not change. These are not
rules about save/load capacity or player-visible presentation.
