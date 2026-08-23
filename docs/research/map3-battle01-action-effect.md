# Map 3 to Battle 01 Action/Effect — Static Contract

- Status: **Confirmed** H2 static contract; it is not an H3 observation, natural-play claim, or
  readiness claim.
- Fixture: `sf2-map3-battle01-action-effect-static-v1`
- Reproduction: `uv run sf2 h2 map3-battle01-action-effect`
- Evidence date: 2026-08-23
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Provenance

This R3b H2 rail begins at the retained R3a unentered per-target call
`0x9CD0 → 0xA3F4`. It closes only `battlesceneScript_ApplyActionEffect`
`0xA3F4..0xA45E` and the three statically visible Apply-to-Drop caller contexts. Each context
returns from the existing `battlesceneScript_DropEnemyItem` owner at, respectively,
`0x9CD8`, `0x9D3A`, and `0x9D98`; this rail stops at those return-resume PCs.

The parser inventories exactly eight source identities:
`battleactionsengine_1.asm`, `battleactionsengine_2.asm`, `attack.asm`, `castspell.asm`,
`useitem.asm`, `inflictdamage.asm`, `displaydeathmessage.asm`, and `dropenemyitem.asm`.
It independently checks their source order against 21 H1/ROM anchors: six caller Apply/Drop sites,
the complete Apply range, seven direct calls, return convergence, and six called-owner entries. The
ApplyActionEffect label is `battleactionsengine_2.asm` line 139 at H1/ROM `0xA3F4`; CastSpell is
`castspell.asm` line 13 at H1/ROM `0xB0A8`. The public fixture contains only source identities/hashes,
ROM addresses/hashes, selector values, powers,
topology, owner identities, and retained-fixture digests; it has no source text, ROM/H1 bytes, runtime
state, result, RNG, text, asset, capture, movie, save, or private payload.

## Confirmed Static Dispatcher and Caller Topology

`ApplyActionEffect` statically compares action selectors in this exact source order: Attack (0), Cast
Spell (1), Use Item (2), Burst Rock (4), Muddled (5), and Prism Laser (6). Attack calls the existing
physical-action owner; Cast Spell and Use Item call their existing spell/item owners. Burst Rock loads
power 18 and calls the existing damage owner; only its source-local `targetDies` false branch skips to
Done, while its true branch calls the existing death-message owner. Muddled has no call and goes to
Done. Prism Laser loads power 16, calls the same damage owner, and has the same source-local
`targetDies` false-to-Done/true-to-death-message topology. Any unmatched selector goes to Done.

The normal target loop, second-attack path, and counter-attack path each contain one direct
ApplyActionEffect call immediately followed by one direct DropEnemyItem call. This is a static
cross-owner sequence only. The action-construction, physical damage, spell, death, EXP, reward/drop,
and status owners retain their existing algorithms and goldens; this rail records neither their full
behavior nor a new reward calculation.

The fixture regenerates and digest-guards the accepted R3a turn/control fixture and the unchanged
`sf2-battle-actions-static-v1` engine projection both at construction and at the golden boundary. The
battle-actions index relation is rederived from the maintained source inventory: it changes only from
45 records/29 paths to 47 records/29 paths because this rail adds the ApplyActionEffect and CastSpell
entry objects.

## Runtime Unknown Register

The closed grouped H3 register is `naturalContinuity`, `initializedSnapshot`, `naturalFirstActor`,
`actorControlBranch`, `playerInputChronology`, `aiCommandSelected`, `movementPath`, `target`,
`action`, `preResolutionArrival`, `dispatchBranchReached`, `perTargetResult`, `statusOutcome`,
`targetDeath`, `expAward`, `goldAward`, `dropOutcome`, `followupOutcome`, `postEffectArrival`,
`afterTurn`, `multiRoundPlaythrough`, `victory`, and `playerReady`. All 23 are **Unknown**. This is
one deferred question queue, not authorization for an H3 fixture, BizHawk launch, or runtime branch
claim.

## Audit Impact

RA-06 gains only the static ApplyActionEffect-to-DropEnemyItem spine. Actual branch selection,
per-target results, status/death/EXP/gold/drop outcomes, follow-up behavior, post-effect arrival,
after-turn behavior, multi-round playthrough, and victory remain **Unknown**. RA-05, RA-07, RA-11,
and RA-12 are unchanged. The Map 3 to Battle 01 audit remains **OPEN** and **NOT READY**.
