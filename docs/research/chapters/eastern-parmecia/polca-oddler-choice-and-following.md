# Polca Oddler choice and following

Evidence date: 2026-10-13. **Confirmed:** Polca's Y11 caller requires F716 set
and F717 clear; Y30 requires F716 set and F718 clear, with no F717 prerequisite.
Y11 refusal returns without common following. Y11 acceptance and both completed
Y30 answers reach a shared conclusion requesting Peter→Bowie, NPC140→Peter and
Luke→NPC140, then setting F68, clearing F75 and setting F718. Current following
and future population are different effects. **Inferred:** the attributed exchange
presents a reluctant group taking responsibility for Oddler. **Unknown:** natural
reach, actual input/actors/services, presentation and durable save/load outcomes.

## Agreement, provenance and bounded method

Standard rigor under [Issue #808](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/808)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
branch convergence, caller flags and two actor lifetimes need explicit boundaries.
This is the sole tracked owner; no shared-path change or stacked dependency.
Consumed accepted main is `74a74aa24a100548fbd79f480fbf3bf4fd3ad395`. Dependencies
are accepted [Battle13 outcomes and raft grant](./battle13-outcomes-through-polca-raft-grant.md),
[Polca zone ordering](./bedoe-return-through-battle13-admission.md),
[common scripting](../../world/common-scripting.md), [common menus](../../presentation/common-menus.md),
[prompt input/return](./volcanon-return-through-bedoe-king-choice.md#caller-prompt-result-and-response-texts),
and [membership/following](./bedoe-to-volcanon-audience.md).

Scope is the two callers, all records/inline actions/texts in five connected entry
regions, their immediate prompt, shadow and follower consumers, and each conditional
control return. Stop before actual Polca departure, boarding/river travel, Battle14,
later Oddler story or Luke recruitment. No natural-route proof, new parser/fixture,
capture, implementation or automatic acquisition. No inseparable expansion is needed.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only source checkout is clean at that pin. Retained map-init and
map-script-engine payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass. This is static
source evidence, not a new composed runtime observation. Fnnn means flag nnn;
fixture program indices are zero-based. Dialogue below is paraphrased.

The exact source is `data/maps/entries/map13/mapsetups/scripts.asm:514–727`,
214 lines through EOF. The corpus has **two physical programs**, `cs_58AE2`
(36 records) and `cs_58B7A` (125), with the other three labels inside the latter.
Those internal labels are branch/entry positions, not independently natural scenes.
The tables below use one-based record numbers local to each named entry region:

| Region / address | Source lines | Physical corpus record positions | Records / inline bodies / text sites |
| --- | --- | --- | --- |
| `cs_58AE2` / `0x58AE2` | 514–549 | First program 1–36 | 36 / 0 / 11 |
| `cs_58B7A` / `0x58B7A` | 550–582 | Second program 1–20 | 20 / 5 / 4 |
| `cs_58BEA` / `0x58BEA` | 583–674 | Second program 21–88 | 68 / 10 / 18 |
| `cs_58D4C` / `0x58D4C` | 675–713 | Second program 89–111 | 23 / 6 / 4 |
| `cs_58DD2` / `0x58DD2` | 714–727 | Second program 112–125 | 14 / 0 / 5 |

All **161 records** (146 commands, 13 sleeps, two terminators), 21 inline bodies
and 42 text sites are accounted for. Explicit cursor writes in physical order
are 1517, 1528, 1541, 1523, 1543, 1532, 1532, 1536. Text keys span 1517–1555
(`05ED..0613`), 39 distinct IDs. Prompt1523/1524 and thanks1532 have repeated
sites; warning/care/response texts1533–1535 and1544–1546 repeat wording at different
IDs. A linear text inventory does not imply executing every branch in that order.
The regions' `nextSingleText`/`nextText` counts are 7/4, 2/2, 11/7, 3/1 and 2/3,
respectively. Single-text handlers close the text/portrait and request Sleep10;
continued-text sites retain their distinct dialogue lifecycle and text controls.

Reuse the unchanged 18,285,979-byte retained pair in one process with linear
selected reads/comparisons. Estimates are below 512 MiB peak and one minute of
comparison time, with only small prose/receipts and no new exported corpus. The
existing comparison took 0.080 seconds; comparison plus direct record/text/caller
checks took 0.085 seconds. No new parser, comparator, manifest or capture is added.
Reassess before materially expanding the graph or resource envelope.

## Supplied population, request and caller selection

**Confirmed:** the accepted predecessor owns Battle13 return and mayor grant.
Fresh Map13 setup priority is F513 > F799 > F710 > default; all four share the
zone table. F513's thirteen regular rows128–140 include mayor137 (man at `(15,3)`,
Up) and Oddler140 (`(14,4)`, Right), both `eas_Init`. Peter7 and Luke10 are follower
declarations under F67/F75, not fixed rows in that population. With only those two
followers they occupy physical1/2 and mayor137/Oddler140 physical12/15. Other
supplied follower flags change those slots. A flag write without an actor rebuild
does not establish this population or repair stale mappings.

Shared init `s6_initfunction.asm:ms_map13_InitFunction` checks F711, F716, F718
in order: F711 clear stages130; F716 set faces137 Down; F718 set calls
`MoveEntityOutOfMap(140)`. The latter moves coordinates/destinations to internal
`$7000,$7000` without deleting its mapping. It does not transfer140 to lookup31.
The predecessor's raft grant is F716→F64→Map72/X43/Y48 after its own script returns;
this slice neither repeats that conversation nor validates/repairs that tuple.

`entityscriptengine_2.asm`'s movement event path masks the selected map block with
`$3C00`; `$1400` writes `MAP_EVENT_ZONE_EVENT=6` and X/Y words from destination plus
directional offsets, divided by `MAP_TILE_SIZE=384`. Being at a coordinate alone
does not prove this producer ran. `explorationfunctions_2.asm:ProcessMapEvent`
clears pending before dispatch. Type6 applies the init action to physical0, reads
X from parameter1 and Y from parameter3, and calls `j_RunMapSetupZoneEvent`.

`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` obtains the current setup and
scans four-byte rows in order. Byte255 is an axis wildcard; default uses `$FD`.
The common table is X24/any Y, any X/Y11, any X/Y30, then default. **X24 wins at
both `(24,11)` and `(24,30)`, even if its handler returns without a scene.** There
is no fallthrough to another row after the selected handler returns. X24's
F710/F799/F715 behavior remains owned by the accepted predecessor zone research.

| Selected handler | Exact source predicate and post-script write |
| --- | --- |
| `Map13_ZoneEvent1`, `0x5827C`, fixture program10 | Check716; clear returns. Check717; set returns. Otherwise run `cs_58AE2`; after return set717; return. No718 test. |
| `Map13_ZoneEvent2`, `0x58294`, program11 | Check716; clear returns. Check718; set returns. Otherwise run `cs_58BEA`; after return set718; return. No717 test. |
| `Map13_DefaultZoneEvent`, `0x582AA`, program12 | Return only. |

Each selected caller has seven operations. The default's return is also the final
operation of program11, not an extra write. Neither caller requires F513, F64,
F68, F75, membership, HP, natural previous contact or a particular population.
F717's source comment mentions Peter declining, but the actual caller sets it
after either completed Y11 answer. F718 likewise is not a receipt for a particular
answer or traversal. The predicates, not their comments, define suppression.

## Prompt and complete branch graph

**Confirmed:** `mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow` (`0x47490`)
saves A6, calls `j_YesNoPrompt`, restores A6 and tests D0 word. Zero sets F89;
nonzero clears F89; both then call Sleep10 before returning. The alias resolves
to `code/common/menus/yesnoprompt.asm:YesNoPrompt` (`0x15284`), the input-aware
entry. `csc0C_jumpIfFlagSet` (`0x47418`) takes the long target when CheckFlag is
nonzero; otherwise it skips that target. `csc0B_jump` replaces A6 unconditionally.

The accepted prompt starts selected Yes. A nonzero entry `PLAYER_1_INPUT`
snapshot requires release to zero; a zero snapshot bypasses that release wait.
The choice loop tests Left, Right, B, C, A in order. Left selects0, Right selects
`$FF`; neither confirms. B selects No and exits; C/A confirms the selected byte.
The closed prompt returns sign-extended0 for Yes or `$FFFF` for No/B cancel.
No input keeps waiting/animating: no timeout, automatic Yes or returned third
outcome. Direction has priority over buttons in the same sample. F89 is transient
choice state; no prior F89 supplies an answer without this prompt returning.

| Path, assuming services return | Ordered entry regions and flag boundary |
| --- | --- |
| Y11 Yes / F89 set | AE2 through record26 → B7A → D4C → DD2 → internal718 → caller717. |
| Y11 No or B / F89 clear | AE2 records27–36 → caller717. No shared following/68/75/718 write. |
| Y30 Yes / F89 set | BEA through record17 → D4C → DD2 → internal718 → caller718 again. |
| Y30 No or B / F89 clear | BEA records18–68, longer appeal → DD2 → internal718 → caller718 again. |

Y11 refusal leaves an eligible Y30 available if F716 remains set and F718 clear;
it is not a promise of a second natural prompt. Y30 refusal does **not** permanently
reject Oddler: its longer appeal converges on following without asking again.
Y30 can also be selected with F717 clear. Conversely F718 set/F717 clear suppresses
Y30 but still permits Y11. That unusual supplied state is not silently repaired.

### Y11 approach and acceptance bridge

**Confirmed:** AE2's cursor1517 requests IDs1517–1527. In this and subsequent
tables, "wait" names the chosen actor's idle-pointer service, not a global barrier.

| AE2 records | Ordered commands and attributed exchange |
| --- | --- |
| 1–7 | Cursor1517; camera `(16,8)`; set137 `(17,7)` Right and140 `(16,7)` Right; mayor interrupts1517; camera `(10,4)`; tentative request1518. |
| 8–17 | Mayor Left/sleep50/Right/sleep30; asks to take boy1519; sleep20; Luke accuses mayor of laziness and avoiding care1520–1521; mayor Left and hesitates1522. |
| 18–26 | Set Bowie `(22,12)` Right, Peter `(23,12)` Left, Luke `(23,11)` Down; camera `(16,6)`; sleep30; Peter asks leader1523–1524; prompt; F89-set jump to B7A. |
| 27–36 | Peter agrees refusal is prudent because the journey would burden a blind boy1525; sleep30; Bowie/Peter/Luke Up; Peter apologetically declines1526; camera `(15,2)`; mayor Right and protests1527; end. |

AE2 mayor texts use `$0,137`; Luke uses `$C0,ALLY_LUKE`. Peter's two prompt texts
and apology use `$80`, his journey/blindness text `$0`. These are requested text
operands, not evidence of an actual gesture or diagnosis.

| B7A records | Ordered commands and inline bodies |
| --- | --- |
| 1–6 | Cursor1528; Peter `$80` expresses surprise at leader's kindness1528; wait140 speed `(10,10)`/jump `eas_Idle`; start mayor Right3/faceLeft1; wait140 Right3; sleep20. |
| 7–13 | Start mayor Right3/faceLeft1 again; wait140 Right3; face137 and140 Down; Bowie/Peter/Luke Up. |
| 14–20 | Mayor thanks1529;137 Left; **FOLLOWER_B Right**; mayor tells Oddler that leader will take him and can be trusted1530–1531;140 Right; jump D4C. |

All three mayor texts here use `$0,137`. `FOLLOWER_B=31` is a distinct lookup
from NPC140, even though the nearby text addresses Oddler. No preceding B7A
command creates31 or aliases it to140. With F68 clear and no other declaration
of31, its default mapping may still be physical0; supplied F74/F70 can instead
map31 to Astral. The command must not be rewritten as `setFacing 140,RIGHT`.

### Y30 approach, refusal appeal and shared acceptance

**Confirmed:** BEA explicitly places the group rather than inheriting AE2's
coordinates. The complete region has 68 records, with two alternative continuations.

| BEA records | Ordered commands, inline bodies and attributed text |
| --- | --- |
| 1–6 | Cursor1541;140 calls for a wait1541; set140 `(14,21)` Down; camera `(10,18)`; wait140 Down3;140 asks to accompany group1542. |
| 7–17 | Wait Peter then Luke `eas_Init`; set Bowie `(14,30)` Up, Peter `(15,30)` Up, Luke `(13,30)` Up; camera `(10,22)`; cursor1523, Peter prompt1523–1524 (`$80`); yesNo; F89-set jump D4C. |
| 18–26 | Cursor1543; hideText;140 confused1543; wait140 speed `(10,10)`/idle;137 Down; wait140 Down3;140 Up; wait140 motion OFF/orientDown/updateSprite/idle; removeShadow140. |
| 27–37 | Sleep30; start Bowie Up2; start Peter Up2; wait Luke Up2; Peter `$80` warning1544 and caution1545; wait140 `eas_Init`;140 Down; headshake140; nod140;140 agrees1546. |
| 38–43 | Sleep50;140 Up; sleep50;140 Down;140 says devils will return while he stays1547 and suggests leader might resist them1548. |
| 44–53 | Wait Peter Up1; Peter Left/140 Right; Peter `$80` fears injury1549;140 Down/Bowie Left/Luke Right; Luke `$0` favors taking him1550, fears he could be killed1551 and appeals to leader1552. |
| 54–63 | Wait Peter Down1; Peter Left/Bowie Right; Peter `$80` says boy needs a guardian1553 and offers care1554; wait Peter Up1; Peter Left/140 Right/Bowie Up; Peter urges departure1555. |
| 64–68 | Cursor1532; nod140;140 thanks1532;140 Down; jump DD2, bypassing D4C. |

All140 speech uses `$0,140`. The 10 inline bodies are two custom bodies
(speed/idle and motion/orientation/sprite/idle), two Down3 moves for140, three
Up2 moves for the group, and Peter's Up1/Down1/Up1. The two warning text sites
use IDs1544–1545 here, not acceptance's1533–1534.

| D4C records | Ordered shared acceptance commands |
| --- | --- |
| 1–6 | Cursor1532; hideText; sleep20; nod140;140 thanks1532 (`$0`); wait140 speed `(10,10)`/idle. |
| 7–14 | Mayor137 Down; wait140 Down2;140 Up; wait140 motion OFF/orientDown/updateSprite/idle; removeShadow140; sleep30; start Bowie Up2; start Peter Up2. |
| 15–23 | Wait Luke Up2; sleep40; Peter `$80` warning1533 and caution1534; wait140 `eas_Init`;140 Down; headshake140; nod140;140 agrees1535 (`$0`); fall through DD2. |

D4C has six inline bodies: speed/idle, Down2, motion/orientation/sprite/idle,
and the three group Up2 actions. Y11 enters with its own earlier placements;
Y30 enters with its own. Neither D4C nor DD2 normalizes those different positions.

### Common conclusion

**Confirmed:** DD2's complete 14-record tail is:

| Records | Ordered commands and attribution |
| --- | --- |
| 1–6 | Cursor1536;140 (`$0`) identifies himself and says he was nearly dead on arrival1536, does not remember1537, does not know why devils pursue him1538, and fears causing trouble1539; narrator `$FF,ENTITY_NONE` announces leader's decision to take him1540. |
| 7–10 | Sleep30; follow Peter→Bowie selector2;140→Peter selector2; Luke→140 selector2. |
| 11–14 | Set68; clear75; set718; end. |

There is no `joinForce`, membership/active-party change, heal, HP write or force-AI
write in any region. Blindness, injury risk, near-death, amnesia and devil pursuit
remain attributed speech, not combat stats, medical state or proof of later plot.
The narrator's decision text is followed by services and separate writes; text
completion alone is not a following or persistence receipt.

## Action, sprite and service limits

**Confirmed:** `csc19_setEntityPosAndFacing` scales packed tile bytes by384 and
writes position/destination and facing; `csc23_setEntityFacing` changes facing and
updates the sprite. Their packed entity byte is the HP helper operand: ally0/7/10
can be skipped for that ally's zero HP; NPC137/140 and lookup31 bypass that ally
test. No entity-presence check follows mapping resolution. Their HP semantics
must not be substituted for the word-encoded follow command's high-byte behavior.

Action handlers `csc14`, `csc15` and `csc2D` install/replace pointers; waited forms
poll the chosen entity pointer for `eas_Idle`. B7A's mayor moves are nonwaited,
and its second installation may replace unfinished work. Waiting for140 does not
wait for mayor137. Similarly, waiting Luke Up2 is not a completion barrier for
Bowie/Peter Up2. Later Peter actions or final follower installation can replace
earlier action pointers. Actual achieved positions remain Unknown. Speed10 is an
internal per-axis parameter, not ten tiles per frame or a health change.

`ac_motion OFF` uses `esc14_setAnimationCounter` to set counter `-1`; it does not
prove immobility or injury. `ac_orientDown` writes orientation bits through
`esc1B_setEntityOrientation`, distinct from facing; `ac_updateSprite` invokes the
sprite service. `eas_Init` resets speed32, acceleration/deceleration and collision
settings, auto-facing, orientation Up, immersion and size24, then updates the
sprite and idles. It is not a heal. Headshake/nod handlers transform and refresh
sprite data with VInt/Sleep waits, bracketed by animation-counter changes. There
is **no `shiver` opcode in these 214 lines**; the mayor's shivering belongs to
the predecessor raft scene. Neither speech nor the posed stumble invents one.

The two `removeShadow 140` sites call `csc30_removeEntityShadow`: resolve current
mapping, `LoadMapsprite` into `FF6802_LOADING_SPACE`, `sub_45A8C`, `DmaMapsprite`,
then one VInt. The immediate `entityfunctions_4.asm` consumer rearranges a 576-byte
working copy in two passes, with helpers clearing color2 nibbles in selected
32-byte blocks and clearing words exactly `$2222` in another block. This is a
mapsprite-data transformation, not a persistent shadow flag, actor deletion or
HP effect. `DmaMapsprite` queues 288 words to the mapped entity's VRAM region and
enables processing. The following `eas_Init` and facing/gesture refreshes reload
ordinary sprite data; no permanent shadow removal across rebuild is established.
Exact displayed pixels and completion still require their runtime services.

Text/camera/gesture/window services and script termination have their own waits.
The physical regions contain requested standalone sleep totals160/20/130/90/30,
respectively, excluding prompt Sleep10 and other services. Branches skip portions;
the sum430 is an inventory, not a single played duration. No wall-clock estimate,
global actor barrier or proof of rendered/audio/input completion follows.

## Current chain versus future follower population

**Confirmed:** DD2's follow words are `$0007,$0000,$0002`,
`$008C,$0007,$0002`, `$000A,$008C,$0002`. `csc2C_followEntity` first reads the
**high byte of the first word**, zero in all three, before
`AdjustScriptPointerByCharacterAliveStatus`. Thus each tests **Bowie0's word HP**,
including NPC140's command. Zero skips six operand bytes; nonzero admits mapping
resolution regardless of Peter/Luke HP. It does not test Oddler's health.

`GetEntityAddressFromCharacter` resolves the follower and target through current
`ENTITY_INDEX_LIST`; these are lookup identities, not physical slots. There is
no validity/HP guard for the target. Default-zero missing mappings can resolve
physical0, while explicitly invalidated `$FF` is a different invalid state; neither
is repaired here. Supplied maps/populations/aliases must be stated before claiming
the intended chain actually forms. NPC140 and FOLLOWER_B31 remain distinct.

`entityfunctions_2.asm:AddFollower` copies/patches a 42-byte action, replaces the
current follower's pointer, advances the walking-action allocation and appends
the physical index only if absent from `EXPLORATION_ENTITIES`. Existing entries
are not reordered or removed. Selector2 supplies `(-24,0)` pixel offsets to
`entityscriptengine_2.asm:esc03_follow`; that consumer derives destinations and
velocities from target state and map collision. This is neither an immediate
teleport nor a wait for the chain to settle. An existing list Peter,Luke can become
Peter,Luke,140 even though the installed target chain is Peter→Bowie,
140→Peter,Luke→140. List order and target pointers are separate state.

After those calls, F68 set and F75 clear do not rebuild the current entities,
remove Luke, remap140 to31 or modify membership F7/F39/F10/F42. With zero Bowie HP
all follow calls can skip while the three flag writes and later caller write
still occur. Other already-active followers are not normalized by this tail.

On a later **fresh** entity initialization, `followersfunctions_1.asm` consumes
`data/scripting/entity/followers.asm` in table order, independently of force
membership. Polca is neither excluded nor an overworld caravan-prefix map.
F67 declares Peter; F68 declares Oddler's sprite under **FOLLOWER_B31**, after
the earlier Sarah/Peter/Kazin/Slade/Astral/Chester rows; F75 clear omits Luke's row.
With only F67/F68 set, Peter/31 are physical1/2 and F513 regular NPC140 is again
physical15, then F718 init moves140 off-map. Luke has no fixed F513 row to retain
him in that fresh population. This differs from DD2's current three-link chain.
Other follower flags alter allocation and can reuse31; in particular later F70
can overwrite that mapping with Astral. The code does not enforce a universal
two-follower population or repair conflicting flags. A no-rebuild transition is
not the same boundary, and no later map/save/load outcome is observed here.

## Partial state, repeat suppression and control return

**Confirmed:** all writes are sequential, not a transaction. Before prompt return
there is no new F89 result; after it but before later services return there may be
choice state without a completed conclusion. Each installed follow pointer can
precede the next, and all precede68→clear75→718. A stall before caller return can
leave internal718 set without caller717 (Y11) or without the redundant caller718
(Y30). No rollback or mismatch repair is supplied.

| Supplied/partially completed state | Source consequence |
| --- | --- |
| F716 clear | Both selected Y callers return without scenes, regardless of717/718/68/75. |
| F717 set, F718 clear, F716 set | Y11 suppressed; Y30 still admitted. No assertion that Y11 previously refused. |
| F718 set, F717 clear, F716 set | Y30 suppressed; Y11 still admitted, even against a fresh off-map140 or an existing31 follower. |
| F717/F718 both set | Both suppressed; neither validates the current chain or raft/actor/membership state. |
| F68 set/F718 clear or F68 clear/F718 set | No consistency gate: Y30 follows its718 predicate; future population and init separately follow68/718. |
| Completed Y11 refusal | Sets717 only at caller level; preserves supplied68/75/718 and current following. |
| Completed common conclusion | Internal68 set/75 clear/718 set; Y11 then sets717, Y30 then sets718 again; no new raft or warp request. |

The selected zone wrapper closes portrait/text, waits one VInt, then waits for
**physical0** position to equal destination. This is a separate service after
the handler's flag write, not a global completion wait for mayor/Peter/Luke/140.
`ProcessMapEventType6_ZoneEvent` returns to the exploration loop. `WaitForEvent`
retargets camera0 and invokes controlled-entity action setup only when pending
is zero; ordinary player type0 selects character control. The selected scripts
do not themselves queue a warp or other pending event. Reaching these services
is still conditional on earlier returns and supplied pending/player state.
**Stop at that caller/control boundary.** It is not an observed responsive-input
receipt, automatic Polca exit, river entry, battle admission or lasting save result.

## Reproduction and acceptance limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the retained read-only source pin/clean state. Run the existing
[payload and cutscene comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without rewriting its inputs. Select `cs_58AE2` and `cs_58B7A` from retained
`programCorpus.programs`; compare every record's source line/macro/arguments to
514–727, then review the 21 inline bodies and internal label targets separately.
Check all 42 text comments against keys `05ED..0613` in
`data/scripting/text/gamescript.txt`, respecting every cursor reset; do not export
a dialogue transcript. Compare the two seven-operation callers, first-priority
program 9 and shared return in 12 against the existing zone fixture.

| Claim boundary | Existing evidence and immediate source owners |
| --- | --- |
| Setup/callers/control | `tests/fixtures/h2/map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-events/zone-programs.json`, `map-event-request-state-static-v1.json`, `map-event-request-consumption-static-v1.json`; Map13 tables/init, movement request producer, type6 and zone wrapper. |
| Complete branch/text/action graph | Retained map-script-engine payload and `tests/fixtures/h2/map-script-engine-static-v1.json`; the two physical programs, five entry regions, inline bodies, text keys and `mapscriptengine_1/2.asm` handlers. |
| Prompt | `tests/fixtures/h2/common-menus-static-v1.json`; `yesnoprompt.asm`, jump-interface alias, csc11 result/flag/Sleep order and csc0C target polarity. |
| Current/future actors and sprites | Named mapping/HP helper, AddFollower/esc03, follower table/initializer, F513 rows, F718 init and csc30 with its immediate transformation/DMA consumers. No fixture expansion or new renderer claim. |

Aggregate fixture shape alone does not prove composed natural behavior. The
selected direct comparison establishes source-to-existing-record consistency;
manual consumer review supplies the bounded semantic connections. Retain
[PR805's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/805#issuecomment-6159053815),
including superseded CI, its scheduling-gap watcher exit and resolved navigation
failures, and its linked earlier failure history. None requires ritual rerunning.

**Unknown:** natural Y11/Y30 selection or choice history, actual HP/flags/current
mappings and membership/AI, concurrent positions and action completion, sampled
input/release, visible shadow/gesture/text/camera behavior, audio/services,
responsive control and save/load persistence. No runtime acquisition closes those
questions here; they do not block this declared static outcome or authorize expansion.

Record full owned diff, links/provenance/private boundary, direct comparison,
clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
final-head CI in the Issue/PR handoff. Documentation-only acceptance runs no local
blanket normal/full/engine/H3/H4/Godot suite. Leave a clean pushed Draft PR, Project
Review / Verify and frozen writer after the authorized main-gate callback succeeds.
Independent acceptance/merge remains main-gate's; no cleanup or next-Issue selection.
