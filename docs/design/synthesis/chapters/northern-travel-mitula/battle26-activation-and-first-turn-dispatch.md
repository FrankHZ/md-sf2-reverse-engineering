# Battle26: activation and the first turn handoff

**Confirmed:** after unchanged fresh initialization, Battle26's fifteen living
enemies can enter the turn order even though the party triggers none of its three
regions. Their STARTING mode also produces an empty spawn list. Region activation,
spawning and turn eligibility are separate rules. The resulting first actor depends
on the actual party, current agility and sequential RNG; this guide ends at the first
call to ExecuteIndividualTurn, before any turn is performed.

**Inferred:** the formation stages contact before the party enters the configured
regions. **Unknown:** natural arrival, actual state and first actor, completed
services, visible presentation, usable control, save persistence and later combat.

## Scope and continuity

This guide continues [Battle26 initial formation and neutral Higins](battle26-initialization-and-roster.md)
at `BattleLoop:@Start`, after fresh LoadBattle and the start wrapper return. It
consumes the complete accepted
[activation and dispatch research](../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md),
including its changed-state analysis. The
[admission and confrontation guide](battle26-admission-and-geshp-confrontation.md)
owns the earlier story scene. Neither scene actors nor initialized commandset names
establish who controls a turn.

Standard rigor separates region geometry, checked flags, activation words, spawn
modes, scheduling records and a called action. This is the English Layer B outcome
for [#1014](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1014)
under [#514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514),
with [the roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
owning its current agreement. It introduces no original finding, engine policy or
runtime observation. Performed turns, tactics, movement, damage, outcomes,
recruitment and subsequent exploration are outside this guide's endpoint.
[Battle26 outcomes and Higins recruitment](battle26-outcomes-and-higins-recruitment.md)
continues from accepted outcome evidence through the returned MainLoop handoff;
it supplies no intervening combat transcript or later town admission. Independent
UI work supplies no evidence here.

All **Confirmed** original claims below are conditional deductions from accepted
Layer A at pinned SF2DISASM
[`c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and the locked USA retail baseline, rather than a new Battle26 playthrough. The
research owner retains instruction addresses, source paths, aliases and reproduction
commands. Fnnn means flag nnn; the activation table labels its hexadecimal words.

## What happens before the first call

**Confirmed:** the fresh round prefix is ordered as follows. A later step requires
the earlier called services to return; static order does not establish visible timing.

| Step | Selected operation | What it establishes |
| --- | --- | --- |
| 1 | ActivateEnemies | Scan enemy activation candidates, including the extra slot 160 fall-through. |
| 2 | ExecuteBattleRegionCutscene | Scan the complete region-hook table; current Battle26 matches no row. |
| 3 | Test DEBUG_MODE_TOGGLE.b | Zero skips diagnostics; nonzero calls PrintAllActivatedDefCons before spawning. |
| 4 | PopulateTargetsListWithSpawningEnemies | Write a word list length; only a nonempty list calls SpawnEnemyWithCamera for its byte IDs in stored order. |
| 5 | GenerateBattleTurnOrder | Fill, append and sort the two-byte turn records, then clear the current-turn byte. |
| 6 | Read one combatant byte | CURRENT_BATTLE_TURN is a byte offset into the buffer. ID 255 returns to step1; any other ID reaches ExecuteIndividualTurn. |

The dispatch read does not read the score or recheck HP, position, membership,
activation, neutral state or control type. No end-of-battle test occurs before this
call. It proves a handoff to the function, conditional on reaching it, rather than a
returned action, a player-ready menu or a completed AI turn.

The debug branch checks sixteen flags F90–105. For each held flag it writes the
region index into DIALOGUE_NUMBER, requests text463, restores/increments the index,
and finally closes text. These are diagnostic text-service calls, not activation
writes or evidence that diagnostic text appeared. Their completion is an additional
premise when the debug byte is nonzero.

## Regions and activation are different from turn eligibility

**Confirmed:** ActivateEnemies begins at enemy 128 and performs 32 ordinary helper
calls through 159. Each helper rejects negative signed X, then zero current HP,
then a nonnegative combatant-ID byte. It does not test Y, membership, neutral state,
move orders or commandset. Nonzero HP with its sign bit set passes this HP test.

The consumers have different admission rules and bounds:

| Consumer | Slots considered | Actual position/HP gate | Other important boundary |
| --- | --- | --- | --- |
| Ordinary enemy activation | 128–159, then an extra160 fall-through | Signed X nonnegative; HP word nonzero; ID byte negative | No Y, membership or neutral-bit exclusion. |
| Allies that can trigger geometry | All 30 slots0–29 | HP word positive signed; signed X and Y nonnegative | Uses neither active-party membership nor enemy/neutral slots. |
| Spawn-list construction | All 32 slots128–159 | Mode-dependent gates described below | Initialization bits are distinct from active bits. |
| Turn-list construction | Allies0–29, then enemies 128–157 | Signed X nonnegative; HP word nonzero | No Y, membership, neutral, active-region or commandset test. |

Thus a changed living placed ally slot can trigger geometry even if it is not a
current active member. A negative signed HP cannot trigger geometry but can pass
the nonzero-HP activation/turn checks. The32 activation candidates and 30 enemy
turn candidates must not be treated as one roster bound.

For the accepted fresh state, enemies 128–142 have positive HP and placed X.
Absent 143–158 fail activation's X gate. With completion526 clear, neutral 159 has
X18/Y14 and HP 0, so it fails HP rather than a neutral-bit test. With 526 held it
instead has X=-1/HP 0. The predecessor owns these conditional neutral writes.

### The complete fresh geometry

**Confirmed:** Battle26 configures three twelve-byte region records:

| Region / flag | Type | Vertices1,2,3,4 |
| --- | --- | --- |
| 0 / F90 | 4 | `(21,0),(12,31),(35,31),(35,0)` |
| 1 / F91 | 3 | `(4,0),(34,31),(34,0),(0,0)` |
| 2 / F92 | 4 | `(0,0),(0,21),(35,21),(35,0)` |

Type 3 uses triangle1/2/3. Other types try triangle1/2/4, then triangle3/2/4 only
when the first contains no eligible ally. The actual edge consumer sign-extends
byte coordinates and projects using signed multiplication/division with truncation
toward zero. It chooses the larger absolute X/Y delta, horizontal on equality,
uses bounded collinearity checks, accepts an edge result1 immediately, and otherwise
compares the ally's side with the opposite vertex with separate bit 0 exceptions.
This is the accepted integer geometry, not an arbitrary floating-point polygon test.

The twelve placement ordinals are `(7,25),(6,25),(5,25),(4,25),(6,26),(5,26),
(4,26),(6,27),(5,27),(4,27),(7,26),(7,27)`. All lie outside all three regions:
region 0 is to their right, region 1's diagonal excludes the lower-left formation,
and region 2 ends at Y21. Fewer living members use a subset; unused X=-1 slots are
rejected. This relies on completed fresh initialization with no intervening
position/HP/flag mutation, rather than a fixed twelve-character party.

A zero-region subsection would return before both region scanning and activation
writes. Battle26 instead selects its count of three. Each region first consults the
separate checked-region word
NEWLY_TRIGGERED_BATTLE_REGIONS. A held bit skips its geometry. An unchecked region
sets F90+index only if geometry matches, but sets the checked bit after testing
even on a miss. The first passing enemy consequently sets the low three checked
bits; supplied other bits persist. Exact word 7 requires those other bits clear.
The flags F90–92 remain clear with unchanged fresh positions regardless of incoming
checked bits, since fresh initialization cleared the persistent flags.

This prefix clears neither the checked word nor prior region flags/active bits.
A later AI body contains a checked-word clear, but is outside this endpoint and cannot justify
assuming that the incoming word was zero. The shared index consumer returns false
for255 and uses an inclusive count comparison; only0–2 are passed by this normal
Battle26 scan. Trigger nibble15 is a sentinel, not a geometry index to evaluate.

### All fifteen activation rows

**Confirmed:** the high trigger nibble is primary and the low nibble secondary.
When both nibbles equal15 there is no activation write. Otherwise a held primary
flag sets active bit 0 and returns before secondary selection. Only if primary is15 or clear can a held
non-15 secondary set bits 0 and 1. A miss leaves the word unchanged. Primary writes
preserve a previous secondary bit; both paths preserve other activation bits.

| Row / ID | Type | Trigger primary/secondary | Fresh word | If the relevant flag is held |
| --- | --- | --- | --- | --- |
| 0 /128 | Lizardman | 15/15 | `2000` | no activation write |
| 1 /129 | Wizard | 15/15 | `2000` | no activation write |
| 2 /130 | Dark Bishop | 15/15 | `3000` | no activation write |
| 3 /131 | Lesser Demon | 1/1 | `2000` | F91 -> `2001`, primary wins |
| 4 /132 | Dark Soldier | 1/15 | `2000` | F91 -> `2001` |
| 5 /133 | Dark Sniper | 1/15 | `2000` | F91 -> `2001` |
| 6 /134 | Dark Sniper | 2/2 | `2000` | F92 -> `2001`, primary wins |
| 7 /135 | Dark Soldier | 2/15 | `2000` | F92 -> `2001` |
| 8 /136 | Lesser Demon | 2/15 | `2000` | F92 -> `2001` |
| 9 /137 | Dark Soldier | 0/0 | `2000` | F90 -> `2001`, primary wins |
| 10 /138 | Dark Soldier | 0/15 | `2000` | F90 -> `2001` |
| 11 /139 | Lesser Demon | 0/15 | `2000` | F90 -> `2001` |
| 12 /140 | Lesser Demon | 15/15 | `2000` | no activation write |
| 13 /141 | Dark Bishop | 15/15 | `3000` | no activation write |
| 14 /142 | Dark Sniper | 15/15 | `2000` | no activation write |

The displayed words are hexadecimal for accepted unupgraded definitions. With
fresh flags and unchanged positions every word remains at its fresh value. None
of these selected rows has a secondary-only trigger: secondary is15 or equals
primary. Changed trigger bytes/flags can take the other branch, and prior bit 1
can survive a primary write. Completion526-held upgrades can change priority and
stats while initialized row triggers/STARTING mode remain as accepted, conditional
on upgrade-service return. Priority, NONE and HEALER/ATTACKER/LEADER labels prove
neither a tactic nor player/AI ownership.

## Why an extra activation pass can touch deals storage

**Confirmed:** after the32-call loop, d0.w is160 and d7.w is65535. No RTS,
branch or emitted data separates the loop from the immediately following helper
label. Execution falls into that helper once more; its RTS returns to BattleLoop.
The selected layout includes this whole file before the turn-order functions.
A function-end comment emits no return instruction.

The actual combatant-address helper rejects byte IDs above 160 with `bhi`, so160
passes its inclusive bound. Subtracting 96 yields storage index 64; `64*56` added
to the combatant base gives `0xFFF600`, CURRENT_GOLD, immediately before
DEALS_ITEMS at `0xFFF604`. This is an overlapping address rather than an allocated
enemy. Its selected pseudo-fields are:

| Pseudo-field | Actual packed-deals interpretation |
| --- | --- |
| X, byte at offset 46 | Counts for items 84/85; bit 7 makes pseudo-X negative and rejects the helper. |
| Current HP, word at offset 14 | Counts for Protect Ring20, Quick Ring21, Running Ring22 and White Ring23; a zero word rejects it. |
| Triggers, byte at offset 54 | Counts for items 100/101 become primary/secondary region nibbles. |
| Activation, word at offset 52 | Counts for items 96–99; the low bits belong to Goddess Staff99. |

The deals decoder selects `DEALS_ITEMS+floor(item/2)`, even items in the high
nibble and odd items in the low nibble. If pseudo-X passes, the ring-count word
is nonzero, and a non-15 pseudo-trigger references a held F90–104 flag, the normal
activation setter ORs bit 0 into the Goddess Staff count, or bits 0/1 for the
secondary path. Other quantity bits persist; an already-set bit need not change
the quantity. This is a conditional bit write, not unconditional staff addition,
a saturating increment, a shop transaction or proof of observed shopping.

With unchanged fresh Battle26, F90–105 were cleared and no geometry sets them.
Even otherwise eligible deals bytes find no held trigger flag, so this extra call
performs no deals write. Changed positions/flags/deals can alter that result;
earlier activation writes in the same prefix are already visible. The
[research alias join](../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains exact addresses/getters/setter and instruction provenance; the source BUG
comment alone is not the proof.

## The region hook returns, and fresh spawning is skipped

**Confirmed:** the complete region-hook table has four eight-byte rows and a
word 65535 terminator:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
| TO_MOUN32 | 0 | 386 | rbcs_battle32 |
| TO_MOUN32 | 1 | 386 | rbcs_battle32 |
| VERSUS_ODD_EYE40 | 0 | 387 | rbcs_battle40_1 |
| VERSUS_ODD_EYE40 | 1 | 388 | rbcs_battle40_2 |

The wrapper preserves d0/d1/a0, scans by eight bytes and compares current battle
before played/region flags. Battle26 skips every row and returns without checking
those flags, setting one or dispatching MAPSCRIPT. Complete table inspection proves
this disposition; absence of a named Battle26 file would not. With changed current
battle32/40, a selected row checks played then region, writes the played flag before
script dispatch and returns after that script. Interruption can preserve that flag
without completed script effects.

### STARTING is not hidden or respawn

**Confirmed:** spawn construction starts count 0 and scans32 slots128–159 in order.
It masks each activation word with `0x0F00`. Mode 0 skips immediately, before any
HP, region, position, item or reset call. All fifteen rows use STARTING0; definition
priority occupies `0xF000`, and activation changes only bits 0/1. Upgrading definitions
or triggering regions therefore does not change these rows' spawn mode.

Absent 143–158 have 0;159 has 0 or neutral 8, also masked0. All 32 candidates skip,
TARGETS_LIST_LENGTH.w becomes 0, and BattleLoop calls no spawn service. Old list
bytes are not cleared, but length 0 leaves them unconsumed. This branch performs no
item reset, respawn counter, camera, appearance text, sound or animation.

Changed state must follow the actual mode branches instead:

| Masked mode | Admission before reset |
| --- | --- |
| Hidden `0x0200` | Current HP 0 plus a held non-15 primary/secondary trigger; successful activation helper returns 0 rather than -1. Max HP must also be 0. |
| Respawn `0x0100` | Current HP 0; no region requirement. |
| Hidden+respawn `0x0300` | Same dead/trigger helper success, without the extra max-HP test. |
| Other nonzero mode | Matches no selected arm and appends nothing. |

Reset requires a real row: Battle26's count 15 gives exclusive combatant bound 143.
It tests the row's start against X/Y of all 30 allies and 32 enemies, without HP,
neutral or self-exclusion filters. A dead slot still at its own start can block
itself. Invalid row or occupied start writes max/current HP 0 and X=-1, returns carry
set and is not appended. Success reuses InitializeEnemyStats, including conditional
upgrades/current stats/items, restores the saved full activation word and returns
carry clear. Only then does the caller append the ID byte and increment its word
length, in ascending scan order.

The later mode comparisons use returned d1, the full restored word, rather than a
new masked mode. A successful hidden reset retains `0x0200` mode and matches neither
later respawn nor combined arm; successful respawn retains `0x0100` and does not
match the combined arm. Other active/priority bits do not change that distinction.
Each selected successful slot appends once. Source fall-through does not establish
a duplicate append or a monotonically incremented spawn counter.

The initializer retains its item boundary: row 2's successful Black Ring insertion
strips encoded equip/AI-use/drop bits through AddItem, while ordinary NOTHING rows
leave copied equipment. This is initializer reuse, not a separate spawn award;
fresh mode 0 prevents these reset writes altogether.

For a changed nonempty list, SpawnEnemyWithCamera uses the zero camera option,
preserves the outer traversal registers, calls entity spawn, clears cursor radius,
selects view target, requests cursor destination and waits before hiding the cursor.
Twelve iterations request orientation `(d7+3)&3`, d2=d3=-1 and Sleep3, followed by
spawn sound, the combatant name index, text397 and close-text. These are service
and loop-counter boundaries, not a simulated camera/animation, observed timing or
completed appearance. No broader battle's spawning is inferred.

## Building the first turn records

**Confirmed:** the buffer contains 64 two-byte `(combatant, score)` records,128 bytes.
Every record is initially `0xFFFF`: ID 255 and score 255, interpreted as signed -1.
After restoring the append pointer, the constructor scans allies 0–29 and then
enemies 128–157. Only nonnegative signed X and nonzero current HP are required.
Y, active membership, neutral flags, activated regions and commandsets do not gate
append. Thus fresh 128–142 qualify even with all selected regions/active bits clear;
143–157 fail X. Slots 158/159 are outside this loop. Neutral159 supplies no record;
a separately joined/placed Higins19 would be an ordinary ally candidate, not this
neutral actor.

### Current agility and sequential RNG

**Confirmed:** current AGI is read as an unsigned byte. Let `A=rawAGI & 0x7F`,
and `R(n)` be the next GenerateRandomNumber result for range n:

- First score: `A + R(A>>3) - R(A>>3) + R(3) - 1`.
- If rawAGI>=128, append a second record immediately for the same ID. Its base is
  `B=floor(A*5/6)` and score is `B + R(B>>3) - R(B>>3)`, without the final R(3)-1.
- Word arithmetic stores only the low result byte. Rejected candidates consume no
  draws; single-record candidates consume 3, double-record candidates 5, in slot order.

Every call updates the word seed to `(seed*13+7)&0xFFFF`, including range 0. For
these small ranges the result is `floor(seed*n/65536)` using that updated seed.
The direct helper preserves d6.w and bypasses the debug-direction override. The
three draws are sequential state changes, not independently chosen values.

All fifteen fresh unupgraded current-stat rows are:

| ID | Type | Current HP | Current AGI | A>>3 | Records |
| --- | --- | --- | --- | --- | --- |
| 128 | Lizardman | 40 | 38 | 4 | 1 |
| 129 | Wizard | 37 | 44 | 5 | 1 |
| 130 | Dark Bishop | 39 | 27 | 3 | 1 |
| 131 | Lesser Demon | 40 | 32 | 4 | 1 |
| 132 | Dark Soldier | 35 | 34 | 4 | 1 |
| 133 | Dark Sniper | 38 | 35 | 4 | 1 |
| 134 | Dark Sniper | 38 | 35 | 4 | 1 |
| 135 | Dark Soldier | 35 | 34 | 4 | 1 |
| 136 | Lesser Demon | 40 | 32 | 4 | 1 |
| 137 | Dark Soldier | 35 | 34 | 4 | 1 |
| 138 | Dark Soldier | 35 | 34 | 4 | 1 |
| 139 | Lesser Demon | 40 | 32 | 4 | 1 |
| 140 | Lesser Demon | 40 | 32 | 4 | 1 |
| 141 | Dark Bishop | 39 | 27 | 3 | 1 |
| 142 | Dark Sniper | 38 | 35 | 4 | 1 |

The predecessor's definitions, clear initial status and equipment effects support
this join. Difficulty changes ATT, not these AGIs; upgraded or supplied stats need
their own evaluation. All fifteen enemy first scores remain positive and below 128
for allowed draws. Their 45 draws follow the actual ally draws. With N eligible
initialized allies and T whose rawAGI>=128, the count is `15+N+T`, at most 39 for the
accepted twelve-position formation. Changed ally stats can yield extra records or
negative wrapped scores; there is no fixed named party or fixed first actor.

### Signed sorting, padding and capacity

**Confirmed:** sorting performs 62 outer passes of63 adjacent comparisons over the
whole64-record buffer. It reads whole records, compares their low score bytes as
signed values and swaps only when the right score is strictly greater. Equal scores
retain append order: earlier ally slots precede later ones, and ally entries precede
enemy entries on equal scores. A double actor's second record was appended directly
after its first. Unsigned135 sorts as -121; padding255 sorts as -1. Padding can
therefore precede changed-state records with scores below -1.

There is no capacity guard. A64-record buffer cannot safely hold arbitrary60
candidates with two records each, and 62 passes do not prove complete sorting for
every possible64-entry arrangement. The selected fresh maximum 39 fits; its positive
enemy scores ensure a non-sentinel first record after these passes. Larger supplied
parties, upgraded definitions and wrapped scores require checking actual count and
score premises rather than inheriting the fresh bound. No overflow experiment or
new scheduling platform is claimed.

The accepted [Battle01 turn fixture](../../../../../tests/fixtures/h3/battle01-turn-order-v1.json)
and [boundary fixture](../../../../../tests/fixtures/h3/turn-order-boundaries-v1.json)
observe their named controlled seams. The latter records AGI 128's scores0/255 and
AGI 127's score 135. They support byte wrapping, signed comparison and duplicate-turn
boundaries; they do not supply Battle26's party, seed or actor. The existing observer
reads all 64 records and collects non-255 IDs, so its reported entries do not prove
that every preceding buffer record is dispatchable; the padding/sentinel rules above
still apply.

## Dispatch, supplied continuation and interruption

**Confirmed:** generation clears CURRENT_BATTLE_TURN.b. BattleLoop zeroes d0.w,
reads that byte as an offset, points a0 into the buffer and replaces d0.b with the
combatant ID. The initial offset is0; the result is zero-extended within d0.w,
without clearing d0's upper longword, and a0 still addresses the buffer at the call.
ID 255 re-enters `@Start`; any other byte reaches the first ExecuteIndividualTurn
call. No score, control-ownership or legality recheck intervenes.

A completely empty eligible set leaves ID 255 at the selected offset and repeats
preparation, rather than declaring victory/defeat. Changed negative-score/padding
arrangements can also select the sentinel. Fresh positive enemies permit a first
call if the preceding services return, while its actual actor remains dependent
on party/current AGI/seed. Nothing here promises player-first or enemy-first control.

The initial F88-supplied branch differs. It restores saved seconds, clears 88/AI
memory, clears the view target, calls LoadBattle and branches directly to the
individual-turn read. Its following branch to `@Start` is unreachable. It initially
bypasses activation, region/debug/spawn work and turn generation, using supplied
current-turn/buffer bytes without alignment or range validation. A supplied non-255
ID reaches the call without rebuilt eligibility; a supplied 255 re-enters preparation
using actual CURRENT_BATTLE and current state. Incoming 72/26 arguments do not
replace supplied CURRENT_BATTLE here, and LoadBattle's neutral writes prevent a
byte-for-byte preservation claim.

| Changed premise or interruption | Consequence retained |
| --- | --- |
| Party/HP/positions/membership changed | Formation and geometry gates differ; no nominal-party guarantee. |
| Checked word or region/active flags supplied | Checked bits can suppress geometry; held flags and prior activation bits persist through this prefix. |
| Completion526 held or stats/items altered | Initialized row triggers/mode remain conditional; actual priority/HP/AGI/items and capacity can differ. |
| Absent/neutral slot changed | Concrete slot bounds, mode, position and HP govern each consumer; no universal neutral exclusion. |
| Deals/trigger fields changed | Slot160 can write Goddess Staff quantity bits under the exact gates. |
| Debug enabled or spawn mode changed | Additional services/reset/occupancy gates can intervene or write state before dispatch. |
| Seed/current AGI changed | Draw count, second records, signed scores and selected actor change. |
| F88/current-turn/buffer supplied | Initial dispatch can bypass preparation; sentinel re-entry is a separate path. |
| Interrupted before first call | Completed flag/deals/reset/list/seed/buffer writes are not rolled back; preparation is not atomic. |

**Unknown:** actual natural reach, party/class/items/HP/positions, flags/checked
word/deals, current/saved battle and turn state, incoming seed, first actor, shared
service completion, asynchronous rendering/audio, usable control and persistence.
No performed turn, selected tactic, combat result, Higins recruitment or later map
follows from this endpoint.

## Evidence review and retained results

The complete accepted #1009 prose, tables, instruction/address joins, spawn-return
correction and changed-state matrix were reviewed against the initialization guide
and selected shared owners. The
[lifecycle contract](../../../contracts/battle/battle-control-lifecycle.md) and
[cutscene routing contract](../../../contracts/battle/battle-cutscene-routing.md) retain wider
Unknowns. Their generic phrases such as placed/living, spawn animation and turn
index do not replace the exact HP tests, slot ranges, called-service or byte-offset
boundaries established by the accepted Battle26 research. Shared spawn-reset
uncertainty remains outside the selected row/occupancy branches closed here. The
[combatant-state contract](../../../contracts/party/combatant-state-access.md) admits selector160
under its static bound while leaving natural/runtime use Unknown; the selected
research closes the instruction/address alias without a runtime claim. The
[Deals contract](../../../contracts/party/caravan-and-deals-state.md) describes helper saturation,
which is distinct from this caller's activation-bit write.

| Reviewed surface / exact fixture ID | Proof and limit |
| --- | --- |
| `sf2-battle-loop-static-v1` and `sf2-battle-control-static-v1` | Shared lifecycle/mode/order facts and maintained `_build_lifecycle_facts` / `_build_control_facts`; summaries do not encode every selected branch, extra pass or dispatch operand. |
| `sf2-battle-spriteset-data-static-v1` | Public structure/source identity, with rowContentTracked=false. Complete fifteen-row/three-region joins come from accepted research, not the public header. |
| `sf2-battle-cutscenes-static-v1` | Eight-byte region records and battle/played/region/write order via `_cutscene_facts`; the complete four-row Battle26 no-match join belongs to research. |
| `sf2-battle01-region-activation-v1` and `sf2-battle01-secondary-activation-v1` | Named baseline/controlled geometry and primary/secondary bit observations; not a natural Battle26 route or later-round clear. |
| `sf2-battle01-turn-order-v1` and `sf2-turn-order-boundaries-v1` | Named current-AGI/RNG/duplicate/signed-tie/filter seams, schemas and actual fixture consumers; not arbitrary buffer capacity or first-actor proof. |

Maintained static consumers are
[battle loop](../../../../../src/sf2tool/h2/battle_loop.py),
[battle control](../../../../../src/sf2tool/h2/battle_control.py),
[spritesets](../../../../../src/sf2tool/h2/battle_spriteset_data.py) and
[cutscenes](../../../../../src/sf2tool/h2/battle_cutscenes.py).
Their owning commands are `uv run sf2 h2 battle-loop`, `battle-control`,
`battle-spriteset-data` and `battle-cutscenes`. The existing H3 fixture scripts and
schemas own the Battle01 observations; reviewing them here launches no emulator.
No in-scope contradiction was found. A source disagreement returns to research;
Layer B does not repair or broaden original evidence.

[#1009 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1009#issuecomment-6182172704)
and [PR1012 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1012#issuecomment-6182188584)
retain head `d1853cc9`, tree `9e6dcb38`, base `63142601`, final CI38664933850
(scope15s/research32s), original CI38664816111, selected joins0.259s/main-gate0.256s
and reused pair0.083s. Neutral tuple/source syntax, full-word spawn return and
source-endpoint integer-projection corrections retain their original diagnostics.
Linked predecessor completed results, failures and Unknowns remain with their owners.

[#1010 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1010#issuecomment-6182128648)
and [PR1011 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1011#issuecomment-6182140705)
retain head `2f0aa60e`, tree `374c2861`, CI38664654273 (scope15s), eight
schemas/563 links/accepted-table joins and completed inspection/tool diagnostics.
Private source and retained pair are unavailable in this design environment under
the accepted synthesis boundary. No acquisition, regeneration, new private
comparison or runtime claim follows; neither task resets completed suites.

This Issue's handoff records full prose/diff/navigation/links/UTF-8/private/exact
five-path checks, git diff --check, configured clean committed planner and actual
exact-head CI. Direct document/schema checks provide documentation acceptance;
blanket normal/full/Python/.NET/Godot/H3/H4/translation suites are skipped. The
candidate stops as a clean pushed Draft PR for independent main-gate acceptance,
with no merge, cleanup, archive, new environment or next Issue.
