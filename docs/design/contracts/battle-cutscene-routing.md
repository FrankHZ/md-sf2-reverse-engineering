# Battle Cutscene Routing Contract

- **Confirmed original structure:** four independent 48-slot battle-cutscene route tables, the
  59 built cutscene source programs, the ten routing-wrapper files, intro/completion/leader/region
  gate order, the shared after-battle join handoff, and the bounded cleanup and position-preparation
  seams described below.
- **Inferred original behavior:** caller-visible meaning of empty route targets and the story meaning
  of source names, flags, and program content.
- **Unknown original behavior:** rendered command timing, complete MAPSCRIPT effects, natural
  reachability and repeatability, persistence of played/completed state, and end-to-end story
  consequences.
- Remake status: implementation-neutral Phase 3 contract; no cutscene runtime, authoring language,
  timeline model, skip policy, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static selection and admission boundary between battle lifecycle hooks and
battle-authored map-script programs. It owns:

1. route-table identity, slot shape, and empty/non-empty counts;
2. the shared intro-flag gate used by before-battle and battle-start wrappers;
3. the completed-battle and leader-death gates used by after-battle and defeated wrappers;
4. region-record scan and flag/trigger admission order;
5. the source-program corpus boundary, including its built and excluded files;
6. the ordered dead-list, join-call, and position-preparation seams that are explicitly closed.

It does not own the outer `BattleLoop` schedule, MAPSCRIPT opcode semantics, dialogue or roster
mutations, presentation, campaign chronology, or product-level narrative design. Those remain with
adjacent contracts, research owners, or **Unknown**.

The executable evidence owners are:

- `sf2-battle-cutscene-data-static-v1` in
  [`tests/fixtures/h2/battle-cutscene-data-static-v1.json`](../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json);
- `sf2-battle-cutscenes-static-v1` in
  [`tests/fixtures/h2/battle-cutscenes-static-v1.json`](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json);
- `sf2-battle-routing-data-static-v1` in
  [`tests/fixtures/h2/battle-routing-data-static-v1.json`](../../../tests/fixtures/h2/battle-routing-data-static-v1.json).

The research owners are [battle cutscene data](../../research/battle-cutscene-data.md),
[battle cutscene routing](../../research/battle-cutscenes.md), and the cutscene-only portion of
[battle routing and terrain data](../../research/battle-routing-data.md).

## Pre-Contract Evidence Audit

This synthesis was checked against the three fixtures, their H2 verifiers, all ten pinned routing
source files, the six cutscene data tables, and the storage include graph. The audit preserves these
limits:

- `routeTargetsParsed` is false in the routing-data fixture; table occupancy is confirmed, but an
  exact public route-slot-to-program map is not;
- the 87 distinct command names are first tokens counted across source statements, not a closed
  MAPSCRIPT opcode registry or proof that all tokens execute through the same interpreter;
- the tracked program fixture retains addresses, aggregate counts, and hashes rather than dialogue,
  choreography, or other extracted content;
- the unassembled Battle 01 region source is an orphan, not a fourth built region program;
- outer lifecycle call order remains owned by the
  [battle-control contract](battle-control-lifecycle.md).

The audit also found that the accepted `leaderDeathPositions.movesAllSlotsOffscreen` and
`leaderDeathPositions.setsAllEnemyHpToZero` aggregates overstate the pinned source. The paired loop
sets X to `-1` for allies 0 through 29 and enemies 128 through 157, and calls `SetCurrentHp(0)` only
for those 30 enemy slots. It leaves `D1` at zero; the tail therefore sets X to `0` for enemy slots
158 and 159 and performs no HP write for either. That owner correction has been routed to the
research lane. This contract records the exact source-reviewed ranges but defines no fixture-bound
fidelity requirement from either disputed universal until the owner correction lands.

## Identity Domains

An implementation MUST keep these identities separate:

| Domain | Confirmed original boundary |
| --- | --- |
| route table | before battle, battle start, enemy defeated, and after battle are independent tables |
| route slot | each relative-pointer table contains 48 ordered slots |
| battle slot | the encounter/terrain backbone contains 45 slots and is not the route namespace |
| region route record | one 8-byte admission record, scanned until a word terminator `-1` |
| built cutscene program | one of 59 source files included by the layout-owned storage container |
| orphan source | Battle 01 `cs_regiontriggered_1.asm`, labeled but absent from the build graph |
| map-script command | interpreter-owned behavior referenced by a program, not defined by route admission |

The four route tables MAY share numeric slot indexes, but they MUST NOT be collapsed into one route
kind. The 48 route slots MUST NOT be positionally truncated to or described as the same namespace as
the 45 encounter slots. The [encounter-definition contract](battle-encounter-definition.md) owns that
distinction from the encounter side.

## Route-Table Shape

The accepted routing-data owner establishes:

| Route table | Slots | Non-empty targets |
| --- | ---: | ---: |
| before battle | 48 | 27 |
| battle start | 48 | 1 |
| enemy defeated | 48 | 3 |
| after battle | 48 | 25 |

Empty versus non-empty status is a fidelity fact. Exact target identities, empty-target execution
behavior, out-of-domain indexes, and normal-story reachability are not closed by this fixture.

The region table contains four longword-bearing route records followed by a word terminator. The
program inventory contains three built region-triggered source files. Because target parsing is
explicitly absent, a remake MUST NOT infer a one-to-one route/program correspondence merely from
those aggregate counts.

## Built Program Corpus

The layout-owned storage container includes 59 labeled cutscene files across 34 battle indexes. The
confirmed type counts are:

| Source-program type | Built files |
| --- | ---: |
| before battle | 27 |
| after battle | 25 |
| battle end / enemy defeated | 3 |
| battle start | 1 |
| region triggered | 3 |

The parser counts 5,672 source statements, 87 distinct first-token names, and 59 `csc_end` tokens.
The source audit confirms one `csc_end` token in each of the 59 built files, while the public fixture
retains only the aggregate. Frequent tokens include text advance, waits, facing/position changes,
action scripts, and entity actions.

These are structural inventory facts. They do not establish:

- a closed executable opcode set;
- statement timing, concurrency, skip behavior, or presentation;
- the meaning of dialogue, flags, joins, deaths, map mutations, or entity actions;
- that every built program is naturally reachable;
- that the same interpreter owns embedded subroutines or entity-definition statements.

Complete generated command/file detail remains private under ignored `local/derived/` output. A
public remake repository MUST NOT reconstruct program content from aggregate counts or redistribute
the extracted source rows.

## Shared Intro Gate

Before-battle and battle-start wrappers compute the same per-battle intro flag by adding the current
battle index to `BATTLE_INTRO_CUTSCENE_FLAGS_START`.

**Confirmed static before-battle order:** the wrapper checks the shared flag and returns when it is
already set. Otherwise it selects the current-battle entry from the before-battle relative-pointer
table and calls `ExecuteMapScript`. It does not set the shared flag.

**Confirmed static battle-start order:** the wrapper checks the same flag and returns when it is
already set. Otherwise it sets the flag before selecting the current-battle entry and calling
`ExecuteMapScript`.

This proves flag polarity and call order. It does not prove why two lifecycle hooks share one flag,
whether an empty target returns immediately, when the flag persists to storage, or visible timing
between the two hooks. The outer new-battle order remains with
[battle control](battle-control-lifecycle.md).

## After-Battle Route and Join Tail

The after-battle wrapper computes the current battle's completed flag. When that flag is clear it
selects the after-battle route and calls `ExecuteMapScript`; when the flag is set it skips that script.
Both paths reach the same tail.

The shared tail:

1. reads the current battle index;
2. reads one byte from `table_AfterBattleJoins`;
3. calls `JoinForce` with that byte;
4. restores the wrapper state and returns.

The layout-owned table contains 52 bytes and every byte is zero. The source marks the feature
unused, but the control flow still reaches the read/call seam. The visible effect of repeatedly
passing zero, interaction with an already-present ally, persistence, and story meaning remain
**Unknown**. A fidelity adapter MUST preserve the observable handoff if it models this wrapper; it
MUST NOT invent 52 distinct join outcomes.

## Enemy-Defeated Admission and Cleanup Tail

The defeated wrapper first requires Bowie's current HP to be nonzero and enemy slot 128's current HP
to be zero. Failure of either gate returns before script dispatch and before the shared cleanup tail.

When both life/death gates pass, the completed-battle flag controls only script dispatch:

- a clear flag selects the current enemy-defeated route, calls `ExecuteMapScript`, then reaches the
  cleanup tail;
- a set flag skips the script and reaches the same cleanup tail directly.

At the tail, a per-battle enemy-leader flag decides whether to scan all 32 enemy slots. Each enemy
whose current HP is nonzero is appended to the existing dead-combatant list; the next byte is written
as `0xFF`, and the list length is incremented. The wrapper does not establish when that list is
processed or when HP is subsequently cleared. Those are owned by
[battle control](battle-control-lifecycle.md).

## Leader-Death Position Preparation

The position listener shares the Bowie-HP-nonzero/enemy-128-HP-zero gate. It scans six-byte battle
records until a word terminator `-1`, then selects a table of four-byte combatant position records.

The accepted, source-reviewed boundary is:

- allies 0 through 29 and enemies 128 through 157 receive X `-1`; the same enemy range receives
  current HP zero;
- enemy slots 158 and 159 then receive X `0`, because the tail does not reload `D1`, and receive no
  HP write in this function;
- the position table terminates with `-1`;
- the source retains an unreachable dead-list write after an unconditional loop branch;
- the exact X and HP ranges remain pending fixture-owner correction for final contract acceptance.

The public fixture does not close natural caller reachability, the fourth position byte, complete
per-record eligibility, final visible entity state, or presentation timing. Those remain **Unknown**.

## Region Admission

The region wrapper scans 8-byte records in table order. For each record it performs these tests:

1. stop when the first word is `-1`;
2. compare the record's battle byte with the current battle;
3. skip when the record's played flag is already set;
4. skip when its trigger-region flag is clear;
5. set the played flag;
6. load the program pointer and invoke the `MAPSCRIPT` trap.

The wrapper contains no rescan edge after the admitted trap; it falls through to restoration and
return. This is static control flow, not runtime proof of natural trigger order, flag persistence,
script return behavior, repeatability, or rendered sequencing.

Region admission is distinct from enemy-region activation and spawn admission. Those remain with
the [battle-control contract](battle-control-lifecycle.md), not this cutscene route table.

## MAPSCRIPT and Cross-System Boundary

Route wrappers transfer control into the shared map-script system. This contract stops at the
selected pointer, call/trap seam, pre-dispatch flags, and explicitly confirmed post-dispatch tail.

Map loading, block/entity operations, camera and screen commands remain with
[map exploration](map-exploration.md). Dialogue handlers remain with the
[dialogue contract](dialogue-system.md), and force/active-party mutations remain with the
[party-roster contract](party-roster-state.md). Asset preparation and rendered battle-scene behavior
remain with [battle-scene presentation](battle-scene-presentation.md).

Program names and comments are not evidence that every referenced effect has been parsed or observed.
End-to-end story side effects remain **Unknown** until the corresponding command owners and natural
caller paths are joined explicitly.

## Fidelity and Modernization Boundary

An original-fidelity routing layer MUST preserve:

- four independent 48-slot route tables and their empty/non-empty status;
- the shared intro flag, including before-battle check-only and battle-start set-before-dispatch;
- after-battle completion gating plus the unconditional shared join tail;
- defeated-route life/death/completion gates and ordered dead-list append/terminator writes;
- region table scan order, played/trigger flag polarity, and set-before-trap order;
- the 59-file built corpus and exclusion of the storage container and Battle 01 orphan as programs;
- the separation between route admission and MAPSCRIPT execution.

A future remake MAY use typed route records, a timeline editor, deduplicated program assets,
skippable scenes, new trigger types, or authored narrative state. Those are product decisions. A new
or changed route MUST be represented as remake content or an explicit compatibility deviation, not
silently attributed to the original.

## H4 Acceptance Surface

A future H4 adapter should use synthetic route presence and state rather than extracted dialogue or
graphics. It SHOULD compare:

1. four 48-slot presence vectors and the four-record region table;
2. before/start decisions and flag-write order for clear and set intro flags;
3. after-battle dispatch/skip decisions plus the shared join-call argument;
4. defeated-route early exits, completion-dependent dispatch, and dead-list append order;
5. leader-position record selection plus only the X/HP ranges and tail values accepted after the
   owner correction;
6. region scan decisions and flag state immediately before the MAPSCRIPT trap;
7. built/excluded program identities, type counts, and aggregate command-shape metadata.

H4 MUST NOT require tracked extracted dialogue, choreography, map assets, or source programs. Natural
story reachability, presentation timing, persistence, and complete script effects remain outside the
adapter until separately evidenced or deliberately designed.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| built/excluded program corpus, type and command-shape counts | **Confirmed static** | `sf2-battle-cutscene-data-static-v1` ([`battle-cutscene-data-static-v1.json`](../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json)) | Program content, reachability, command semantics, timing, story effects |
| intro/after/defeated/region wrapper order and bounded mutation seams | **Confirmed static** | `sf2-battle-cutscenes-static-v1` ([`battle-cutscenes-static-v1.json`](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)) | Natural callers, persistence, MAPSCRIPT effects; disputed all-slot X and HP quantifiers excluded |
| four route-table shapes, region count, and zero-filled join table | **Confirmed static** | `sf2-battle-routing-data-static-v1` ([`battle-routing-data-static-v1.json`](../../../tests/fixtures/h2/battle-routing-data-static-v1.json)) | Exact route targets and empty-target behavior; terrain record excluded |
| outer battle lifecycle | **Separate owner** | [Battle-control lifecycle](battle-control-lifecycle.md) | Do not infer wrapper call timing from route tables |
| dialogue, entity/force/map effects, presentation, and story meaning | **Separate owner / Unknown** | Dedicated contracts and future evidence | No aggregate cutscene fixture closes end-to-end behavior |

## Reproduction

```powershell
uv run sf2 h2 battle-cutscene-data
uv run sf2 h2 battle-cutscenes
uv run sf2 h2 battle-routing-data
uv run sf2 design-contracts test
uv run sf2 research-index test
```
