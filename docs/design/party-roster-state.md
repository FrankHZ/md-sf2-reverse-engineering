# Party and Roster State Contract

- **Confirmed original structure:** six source-named map-script command forms, their physical stream
  layouts, named handler branch/mutation/call order, source-site corpus, direct/effective caller
  identity, and the provenance link to the common-stats roster source.
- **Inferred original behavior:** none in this contract.
- **Unknown original behavior:** normal-story reachability, roster/list capacity, save persistence,
  and player-visible roster/death outcomes.
- Remake status: implementation-neutral Phase 3 contract; no engine has been selected.

## Import Boundary

A remake importer MUST retain the source forms `join`, `jumpIfDefeatedByLastAttack`, `jumpIfDead`,
`allyDefeated`, `updateDefeatedAllies`, and `reviveAlly` as distinct ordered commands. It MUST preserve
each command's original byte width, operand width and stream offset, source program identity, command
index, and raw operand text. It MUST retain all 304 program-total rows, including zero rows and the
two zero-use forms; a zero source-use count does not establish runtime unreachability.

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

The imported direct-call graph MUST retain instruction target and effective target separately. A
jump-interface alias is not erased: `j_JoinForce` remains the instruction target while `JoinForce`
is the resolved target. The connection to `sf2-common-stats-static-v1` is source provenance for
`code/common/stats/battleparty.asm` and its `JoinForce`/`UpdateForce` labels; it is not permission to
copy sibling fixture data into this contract.

## Evidence and Runtime Boundary

Executable evidence is fixture ID `sf2-map-script-engine-static-v1` at
`tests/fixtures/h2/map-script-engine-static-v1.json`, field `forceStateCommandFacts`; its verifier is
`src/sf2tool/h2/map_script_engine.py`. It pins the upstream commit, US ROM hash, handler addresses,
full 304-program source-site/total corpus, section guards, caller maps, and common-stats provenance
identity.

`force-state/roster-death-persistence-visible-outcomes` remains the single grouped H3 question.
Until it is observed, a remake MUST define its own save, capacity, roster membership, death/revive,
and visible presentation policy explicitly rather than treating these static operations as a complete
gameplay lifecycle.
