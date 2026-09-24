# Map 3 Messenger Acceptance

- Status: **Confirmed** for the accepted R2a continuation, the bounded Issue #475 Map19 observation independently accepted in [PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476), and [Issue #485's independently reviewed first Battle01 player-ready acquisition](#accepted-first-battle01-player-ready-acquisition).
- Fixture: sf2-map3-messenger-acceptance-runtime-v1
- Case: natural-map3-messenger-accept-to-follower-ready-wait
- ROM: USA retail SHA-256 9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9
- Source baseline: ShiningForceCentral/SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6

## Battle01 actions and victory continuation (Issue #496)

[Issue #496](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/496) extends the existing
segmented acquisition with the explicit `VICTORY_CONTINUATION = "natural-battle01-victory-5b"`
selection. After independent [PR #499 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/499),
native collection at accepted `a7383cd9133a00de456c9f4575af1482070f6aff` confirmed recoverable battle
save/load, player/AI actions, effects, RNG, scene consumption/reloads and an original **defeat**.
The final response exceeded the existing bridge limit and the host result remains **FAIL**.
After [PR #500 source acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/500),
subsequent native collection confirmed the corrected terminal reply and original Heal 1 input/effects.
After [PR #501 source acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/501),
the compatible chain below confirms original Medical Herb selection, consumption and healing.
After [PR #502 source acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/502),
prepared-45..67 confirm original victory and after-program return, followed by a post-victory
observer **FAIL** described below. After [PR #503 source acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/503),
prepared-68..86 reach [two-frame stable field readiness](#native-victory-and-stable-field-readiness).
The [Issue515 native result](#native-post-victory-ordinary-input-result-issue-515) records the next
ordinary Down input and settled displacement. Independent result acceptance remains required;
remaining 8D and H4 are not established.

The existing `NATURAL_CONTINUATION` first-player-ready mode retains its original terminal predicate.
Issue #485's accepted prepared-37 final pair remains nonresumable. New execution sources require a fresh
compatible chain; neither a changed mode nor a changed observer may reuse an incompatible parent.
Historical accounting begins at 35 starts, 6562.418724000221 charged seconds, 105007 delivered resource
frames and 2063 advancing batches; the retained Issue496 total through prepared-86 is **115 starts /
14197.020479699888 charged seconds / 365304 resource frames / 13351 advancing batches**.
These are retained observations, not stopping ceilings. Failed21's
missing final callback/restoration payload remains **Unknown**. Other retained failures are unchanged.
Prepared-41, explicit abort 61 and callback failure 67 independently retain final
callback/restoration **Unknown**.

### Native save/load, actions and failed terminal

The table below retains the first source chain. Its terminal total was 46 / 7479.841504100186 /
140632 / 3306; it must not be resumed by either later observer identity.

All candidates below are in the owning worktree's ignored `local/issue496/`. Only prepared-06
bootstraps R1; every child loads and verifies original RAM/register/frame and observer state before
input. The battle parents retain the three original return descriptors, checked against saved RAM.

| Candidate / actual start | Saved frame / checkpoint | Delivered frames / batches / active seconds |
| --- | --- | --- |
| 06 / 36 | 8605, messenger rank 4 | 8605 / 134 / 178.91040549997706 |
| 07 / 37 | 10386, Map19 rank 5 | 1781 / 33 / 43.00154950004071 |
| 08 / 38 | 15606, royal rank 6 | 5220 / 77 / 100.50722060003318 |
| 09 / 39 | 17740, guard rank 7 | 2134 / 46 / 54.40373250003904 |
| 10 / 40 | 22368, turn 1 / rank 9 | 4628 / 181 / 103.56984469998861 |
| 11 / 41 | 22479, turn 2 / rank 10 | 111 / 44 / 8.227438599977177 |
| 12 / 42 | 23211, round 2 / turn 10 / rank 18 | 732 / 66 / 22.08622260001721 |
| 13 / 43 | 24277, round 3 / turn 19 / rank 27 | 1066 / 241 / 87.14023969997652 |
| 14 / 44 | 27203, round 4 / turn 28 / rank 36 | 2926 / 269 / 138.76676889997907 |
| 15 / 45 | 32727, round 5 / turn 39 / rank 47 | 5524 / 115 / 122.41380649997154 |
| 16 / 46 | 35625, original `observed-defeat`; no new pair | 2898 / 37 / 58.39555099996505 |

Prepared-11 demonstrates actual load of the first battle parent, movement and STAY, original
player/turn returns, RNG and a forward save. Later segments record actual attack script/effect,
scene consumption, reload and death-processing seams. These bounded observations do not cover
every R3 action branch. At defeat Bowie is 0/12 HP, Chester 0/11, Sarah 9/11 with all 10 MP unused;
two enemies have died, gold is 180 and four enemies remain alive. The first isolated Chester
advance leaves him at 5 HP before his first attack; later consecutive three-point hits kill
Chester and Bowie. This is a failed tactic, not an original-game defect or a winning trace.

The exact final logged reply encodes to **78571 bytes**: completed state 41557 and terminal callback
36814 bytes plus envelope. The unchanged bridge rejects it above **65536 bytes** with
`bridge payload length out of range`. Its native receipt exits 0 without timeout or forced
termination; the observation records `loaded-segment-entry` restoration and callbacks cleared,
the host confirms session-ROM deletion/canonical identity, and an independent process check finds
no survivor. This completed failure is fully charged; it is not an interrupted run.

The narrow correction projects only victory-mode `terminalCallback` to stop reason, callback
PC/frame/order/boundary and `observationFile="observer.observed.json"`. The full terminal facts
remain unchanged in that file and checkpoints; completed state remains in the reply. Direct
execution of the actual old/current Lua reply blocks on this retained defeat produces
78571 / **41926** bytes. Substituting success, unsupported-input and segment-save reasons exercises
the same projection without claiming those native outcomes. Legacy mode is unchanged. The runner
still reads full local observation/cleanup evidence and distinguishes incomplete outcomes from 5B.

Reproduce retained pair/ordering/core/stack checks with `uv run python -X utf8
local/issue496/audit-native-pair.py <prepared-name> <fresh-audit-name.json>` when repeating;
the retained results are `<prepared-name>-pair-audit.json`. `inspect-blocker.py`,
`check-terminal-wire.py <fresh-output>`, `review-defeat.py` and `review-targets.py` reconstruct the
failure, wire sizes and scene HP changes from `runtime/{actual-inputs.jsonl,checkpoints.jsonl,
observer.observed.json,host-status.json,bridge/receipt.json}`. `native-blocker-16.json`,
`defeat-scene-results.jsonl` and `defeat-targets.jsonl` retain the compact results. The earlier local
audit assertion about saved versus final parent order and the wire-driver function-serialization
failure are preserved separately; neither changed original evidence or launched native.

Prepared-15 is the last complete parent of the failed source chain. The corrected execution bytes
make **every old parent incompatible**. After independent source acceptance, prepare a fresh R1
chain carrying the full 46-start totals above; do not resume or relabel old pairs.

### Native Heal 1 chain and remaining item-input gap

Prepared-17..32 use accepted `43b92d14de774f153e263ba00b511aaa8fa22955`, tree
`97ea2c7b206ed3e5ad3fa8f985fe214bbded233d`. Prepared-17 bootstraps a fresh R1, carrying all
46 prior starts; 18..21 reach the same first battle checkpoint at frame 22368. Prepared-22..26
save rounds 2..6 at frames 23256, 24286, 25755, 27627 and 31872. Every complete pair passed
original core/frame/register/RAM, observer order, original return-stack and forward-checkpoint
readbacks before reuse. These saves and failed children remain retained under their own identities.

| Candidate / actual start | Parent | Observed result |
| --- | --- | --- |
| 27 / 57 | 26 | Heal 1 restores Chester 4→11, Sarah MP 10→7. Enemy 133's one turn/scene has FIRST then SECOND attacks, Bowie 8→4→0. Defeat frame 34945. |
| 29 / 58 | 26 | Heal restores Bowie 8→12. Enemy 133 again hits twice, 12→4; the attempted short retreat does not prevent enemy 129's later 4→0 hit. Defeat frame 36272. |
| 30 / 59 | 26 | Heal plus retreat using all original reachable destinations survives to round 7 / turn 54 / rank 62, frame 36182. Bowie 4 HP, Sarah 5 HP / 7 MP, Chester 1 HP. |
| 31 / 60 | 30 | Bowie healed 4→12; Chester dies. Save round 8 / turn 65 / rank 73, frame 41170: Bowie 6 HP, Sarah 1 HP / 4 MP. |
| 32 / 61 | 31 | Bowie kills enemy 133. Sarah self-heals 1→11 / MP 4→1, then enemy 128 lowers her to 9. Enemy 130 lowers Bowie 6→3; next-round enemy 128 lowers him 3→0. Defeat frame 44706 / turn 69. |

Each of 27, 29 and 32 is a completed **INCOMPLETE-OBSERVATION**, not a transport failure or an
interrupted run. All 15 native invocations exit 0 without timeout/forced termination, clear
callbacks, restore the entry state, delete the session ROM and retain the canonical identity;
post-run process inspection found no owned survivor. Failed-child accounting includes every
attempt. Preparation 28 failed in the local caller's relative-parent reconciliation before creating
a candidate or launching; resolving the parent path fixed that caller, without changing source.

Reproduce the compact retained projections with `herb-native-summary.py`, `root-scene-review.py
<prepared-name>`, `review-fatal-scene27.py`, `review-round29.py` and `reconcile-32.py` under
`local/issue496/`, using `uv run python -X utf8`. `herb-native-summary.json`,
`fatal-scene27.jsonl`, `round29-scenes.jsonl`, `failed-{27,29,32}-accounting.json` and the native
receipts preserve results. Scene HP changes compare before `WriteBattlesceneScript` with after
`EndBattlescene`; script construction temporarily computes and restores HP, so its return alone
does not prove consumed damage/healing.

The last complete parent's original inventories are Bowie `[199,0,127,127]`, Sarah
`[213,0,0,127]`, Chester `[184,0,127,127]`. Pinned `ITEM_MEDICAL_HERB=0` and index mask `0x7F`
identify unused herbs; equipped weapon bits are already present. This concrete missing manual
input justifies the bounded extension below. No further native start uses the constrained old
source. Its parents remain incompatible with the changed observer; the next accepted fresh R1
must inherit all **61 / 8837.811812300177 / 192811 / 5222** costs. Victory and 5B remain **Unknown**.

### Native herb observation and completed-branch recovery

Prepared-33..44 use accepted `5e25c5e0b71a4be9f1924de89ce07c848696c225`, tree
`24ae083b6cd8b06dfabfcd7f45f7753ff80461b6`. Fresh 33 inherits all 61 earlier starts;
33..37 reach the original first player-ready frame 22368. Complete 38..40 save rounds 2..4
at frames 23256, 24286 and 25755. Each complete pair passes core/RAM/register/frame,
original return-stack and forward-checkpoint readback before reuse.

| Candidate / actual start | Parent | Observed result and charged frames / batches / seconds |
| --- | --- | --- |
| 41 / 70 | 40 | Local operator stalls at the herb icon menu; forced containment, no pair. 2083 / 608 / 488.6106827999465. |
| 42 / 71 | 40 | Original herb heals Chester 6→11; save round 5 / turn 37 / rank 45, frame 28381. 2626 / 139 / 77.18620880000526. |
| 43 / 72 | 42 | Enemy 132 dies; save round 6 / turn 46 / rank 54, frame 33640. Bowie 4 HP, Sarah 7 HP / 10 MP, Chester 11 HP. 5259 / 127 / 123.45435800001724. |
| 44 / 73 | 43 | Chester attacks 133 (5→3); enemy 128 then kills Bowie (4→0), defeat frame 34972. 1332 / 43 / 31.629579000000376. |

**Confirmed (bounded native observation):** 42's turn 33 follows Item→Use=0, Left from
weapon choice 0 to herb choice 1, then C. Original action 2 / item 0 / slot 1 enters
`battlesceneScript_UseItem`, uses HEALIN 16 and reaches `RemoveItemBySlot`. Bowie's inventory
changes `[199,0,127,127]`→`[199,127,127,127]`; consumed `EndBattlescene` shows Chester 6→11
and Bowie MP unchanged at 8. Herb confirmation and effect are observed; native cancellation is
still **Unknown**. Complete 42/43 and defeat 44 exit 0, restore entry, clear callbacks, delete
their session ROM and preserve canonical identity. No owned native process survives.

41 is a distinct **FAIL**, not a defeat or clean abort. A snapshot evaluates readiness for C;
at a selected weapon it correctly reports `unsupported-battle-selection`, while a directional
step remains individually admissible. The old local operator mistook this for a navigation
prohibition and repeatedly sent neutral. Ctrl-C ended its outer shell without stopping the
native chain. Stopping only the verified upstream operator then lost the pipe reader; the bridge
records `[Errno 22] Invalid argument`, exit 1 and forced termination. Host ROM cleanup/identity
are confirmed, but the missing final observation leaves restoration/callback removal **Unknown**.
All 2083 completed frames through 27838 and 608 batches remain charged. The corrected local
operator permits only directions/B for that sole readiness reason and supplies a per-segment
`abort.request` hook through its existing stdin, preserving the response reader.

43 exposes a tactical recovery limit: Bowie at `(8,8)` is surrounded by four enemies and has
no herb; Chester acts before enemies 128/129/130, then Sarah/Bowie. The actual grid supplies
no unoccupied position from which Chester can heal Bowie or attack 1-HP enemy 128. Pinned
`itemdefs.asm` entry 56 / `sf2enums.asm` identify his equipped item 184 as Wooden Stick,
range 1..1, not Short Spear (72). ROM record `0x17226`, source-defined range offsets 4/5,
confirms both range bytes are 1. This rules out the proposed two-tile attack; it does not
prove every possible original outcome from 43. The revised local policy considers enemies still
due to act and avoids advancing Bowie beyond immediate healing range into multiple estimated
threats. Rescue positioning considers every original reachable destination before cohesion.
`leader-guard-readback-review.json` records changed prospective choices against 43's retained
turn-42 states; it does not prove a replayed outcome or exact AI reach.

The runner now permits choosing an earlier compatible complete parent after **every claimed
attempt in its source lineage has completed**. It follows existing parent links to the root and
existing immutable claims through all branches, including siblings/cousins. Each edge verifies
parent-pair identity, source identities, ordinal and forward checkpoint. A unique traversal rejects
duplicate children, cycles, missing ancestry claims and descendants of an unsealed/terminal pair.
Raw receipt/input increments are reconciled in frozen prior-cost order, not traversal order;
zero-cost pre-process failures precede a launch with identical prior totals. Saved cumulative
costs must agree with those increments. Missing/incomplete receipts, unfinished processes,
unreconciled cleanup and accounting gaps reject both preparation and the launch-time recheck.
Terminal pairs may be verified and charged as completed descendants but remain forbidden resume
parents. The selected parent's original state/order is restored; all branches' resource costs
are inherited. No pair, claim, failed observation or original state is overwritten. This retains
the single-writer execution boundary; it does not admit concurrent branch launches.

**Confirmed at the PR502 offline boundary:** actual 33..44 records reconcile
to the same 73-start totals when selecting either 42 or 43. Direct sibling/cousin, zero-start,
terminal and rejection cases exercise the actual accounting function. The runner identity changed,
so all PR501 pairs remain incompatible with new execution: independent source acceptance must
precede one fresh compatible R1 carrying the complete totals. Future branches within that new
identity can recover an earlier save without repeating R1. No hash whitelist or identity exception
is introduced. The following native result exercises that recovery; stable 5B, full 8D and H4
remain **Unknown**.

Reproduce with `uv run python -X utf8 local/issue496/victory-native-summary.py 44 <fresh-output>`
and the existing pair/scene readers. Retained projections include `victory-native-through44.json`,
`prepared-42-root-herb-review.json`, `rescue43-readback.json`, `weapon56-pinned-review.json`,
`prepared-41-root-failure-review.json` and the raw native receipts. Readers that publish results
exclusively require fresh output names when rerun. `check-branch-accounting.py <fresh-directory>`
executes the production function with explicit synthetic pair-read seams; `branch-real-boundaries.json`
and `branch-accounting-real42.json` retain full real-pair validation. The ordinary verification,
host check, committed plan and actual CI belong to the source PR handoff. Inspection/driver errors
are retained separately in `branch-offline-failures.json`; they neither change native evidence nor
add launches.

### Native victory, after-program return and post-victory observer failure

**Confirmed (bounded native observation):** prepared-45..67 use accepted PR502 main
`193802d3b5dca0ce43f9f4f8e8f54d88037179f3`, tree
`53f4661d3cfcb772eea04494a1c4f0b73e614bd0`. Runner identity is
`43EDFA090AADE098EF31972AECAFB7DE2F20AEB4C16310016D8B8CF94827393F`, observer
`A85C12B920D79161AC5082D5B5AC596F83FD3A111E60428253251C09D83367C7`.
Fresh 45 inherits all 73 prior starts. Original snapshots at 45..49 match the corresponding
33..37 prefix, reaching first battle control at frame 22368. Complete battle pairs pass original
RAM/register/frame, order/epoch, return-stack and forward-checkpoint checks before reuse.

The selected successful lineage is 45→46→47→48→49→50→51→52→53→54→55→57→58→59→62→63→65→66→67.
The intervening attempts remain retained and charged; no state, claim or pair was repaired.
In particular, 62 loads earlier successful parent 59 after completed 60 and failed 61, proving
actual whole-lineage branch recovery. Saved game state rolls back; cumulative resource costs do not.

| Candidate / actual start | Parent | Bounded result |
| --- | --- | --- |
| 56 / 85 | 55 | Original defeat at frame 40851. Chester kills 133, but enemy attacks lower Bowie 9→6→3→0. |
| 57 / 86 | 55 | Chester instead uses his herb on Bowie 9→12; retreat survives to round 8, frame 40815. |
| 58..59 / 87..88 | 57..58 | Enemy 133 dies; Sarah's Heal restores Chester, MP 4→1; round 10 save at 46287. |
| 60 / 89 | 59 | Complete round 11 save at 51300 with Chester dead. Sarah, not Chester, kills enemy 129. This pair was saved, not aborted. |
| 61 / 90 | 60 | Explicit stdin abort: zero completed frames/batches, 3.5335696999682114 seconds, native exit 1. |
| 62 / 91 | 59 | Earlier-parent branch; Chester then Sarah kill 131, no ally HP damage; round 11 save at 48287. |
| 63 / 92 | 62 | Sarah uses herb on Bowie 3→12 and reaches level 2; round 12 save at 51066. |
| 64 / 93 | 63 | Sarah's last herb heals Chester 4→11. Enemy 129 lowers Bowie 12→6; Bowie kills 129, then 128/130 lower Bowie 6→2→0. Defeat at 54922. |
| 65 / 94 | 63 | Revised leader protection survives the exhausted healing inventory; Bowie retreats with 6 HP. Round 13 save at 53013. |
| 66 / 95 | 65 | Sarah kills 129; Bowie lowers 128 to 1 HP. Round 14 save at 56476, all three allies alive. |
| 67 / 96 | 66 | Chester turn 102 kills 128; Sarah turn 103 kills 130. Victory and after-program return are reached, then setup callback fails at 60983. |

The local operator retains its versions and actual decisions. Corrections permit self-healing
retreat, original herb holders as healers, threatened-leader healing before later enemy turns,
safe rendezvous, supported attacks, and leader survival even after the final herb is consumed.
Distance, threat and damage estimates remain tactics, never original legality or damage guarantees.
A prospective finish-before-retreat variant was inspected but never executed. The driver now stops
after one audited round with `continue-rounds.py <parent> <child-number> once`, preventing the
previous automatic-next-child race. Every resumed attempt still uses the accepted source protocol.

**Confirmed:** 67 reaches victory at frame 58657. `ExecuteAfterBattleCutscene` enters at 58660,
order 151133, and `abcs_battle01` (`0x496DC`) has 67 matched actual operation entry/return pairs.
The fixture's 80 source records also include embedded entity actions and trailing entity data;
they are not 80 dispatcher iterations. At frame 60945, program return is order 157461;
shared-tail entry 157463; enclosing after-routine return 157464; F401-clear return 157467;
F501-set return 157469; BattleLoop return with D4=1 157470; SwitchMap return 157472;
ExplorationLoop entry 157473. The failure frame 60983 reads map 57, battle 255, F401 clear and
F501 set. This proves that bounded spine, not two settled field-control frames or full 8D.

67 ends **FAIL**, `unexpected selected map setup` at `0x47504`, order 157550. Native exit is 1,
without forced termination or timeout; session-ROM deletion and canonical identity are confirmed.
The missing final observation leaves restoration/callback clearing **Unknown**. No terminal pair
was produced. Its 4507 frames / 83 batches / 98.33460810000543 seconds remain fully charged.
61 likewise retains missing final-observer cleanup **Unknown** despite host containment and ROM
cleanup; its reader originally awaited a normal step reply after abort, then reported EOF. That
local caller now drains abort completion separately. These failures do not invalidate earlier
complete pairs. Ordinary defeats 56/64 and other successful segments have complete cleanup.

**Confirmed source correction (PR503 offline review boundary):** pinned `MapSetups` has no map57
row. `GetCurrentMapSetup` returns `ms_Void` (`0x477E8`), whose word is `$FFFF`; the existing
pre-battle setup whitelist omitted this actual post-victory selection. Preparation reuses the
map-setup parser/encoder, checks the entire routing table against ROM and binds the named fallback
load and sentinel. Only map57 after the observed victory/flag/BattleLoop/exploration sequence,
with actual battle255, accepts that exact pointer. Other map/pointer checks remain strict.

A second false consumer would prevent stable 5B even after that correction.
`EndAfterBattleCutscene` is the BRA-entered tail of `ExecuteAfterBattleCutscene`, not a new call.
The native tail A7=`0xFFFFF0` points at saved D0/D1, while the owning entry A7=`0xFFFFF8`
contains return PC `0x23D0E`. The old generic callback mistakes saved D0=0 for a return PC and
leaves one pending consumer after the enclosing call returns. Source/H1/ROM bind the prologue,
branch and `movem.l (sp)+,d0-d1; rts` tail. The corrected observer records tail entry only under
its live owning call and requires that entry at the existing enclosing return; it neither clears
pending state nor creates another return consumer. Existing flag and stable-field predicates remain.

Reproduce with `victory-native-summary.py 67 <fresh-output>`, `audit-native-pair.py`, the existing
scene readers and `check-post-victory.py <fresh-output-directory>` under `local/issue496/`, via
`uv run python -X utf8`. `victory-native-through67.json` and `post-victory67-readback.json` retain
costs and chronology. The actual-Lua driver covers the real saved-register tail shape, the full
return/two-frame sequence, invalid setup phase/flags/pointer/map and unchanged old setup rejection;
`check-old-post-victory.py` reproduces both old defects. `post-victory-offline-failures.json`
separates local inspection/driver mistakes from native failures. All private inputs/outputs remain
ignored; no new fixture, schema, CLI or shared bridge is introduced. Corrected source identities
invalidate every old pair. Independent source/preparation acceptance preceded fresh prepared-68,
actual start 97, carrying all 96-start totals. The following result records that completed chain.

### Native victory and stable field readiness

**Confirmed (bounded native observation):** after independent PR503 source/preparation acceptance,
prepared-68..86 execute accepted main `9c3ea03ac5f5b467ee744f1ac624870da2408443`, tree
`7edb5a5169f1847100598c62f147b247579df433`. Fresh 68 starts from R1, carrying all 96 earlier
starts. Its configuration identity is
`221F0B722B802656ECEE6B4806075B05252B2FED26E49F8EAD42BC3A22E51B16`; runner identity is
`97D424459D190B843598642F078BBF0CEDE2C7787F8FC0EB5492255E263D1339`, and observer identity is
`8349604F919EB7071897CF9921ED51F951F5875A745B51289257B3F7CBB7D75B`.
No incompatible save is loaded. The local operator reuses only successful original controller
requests from 45→46→47→48→49→50→51→52→53→54→55→57→58→59→62→63→65→66→67,
checking actual returned frame, positions, flags, RNG/time and battle accounting at each request.
The failed request from 67 is excluded; after its last successful response at 60928, only neutral
continuation is delivered. Accepted source readiness and host guards remain enforced.

The 18 complete intermediate pairs (68..85) match the selected old checkpoints' entire `original`
and `core` objects, including RAM/register/frame state. New 72 reaches first battle control at
22368; new 85 saves round 14 at 56476. Segment 86 reaches `controllable-5b`, rank 112,
round 14 / turn 103, at frame **61010**. Its pair is terminal and **nonresumable**: the production
reader accepts explicit terminal inspection and rejects its use as a resume parent.

| Segment 86 event | Observer frame / order |
| --- | --- |
| Natural victory | 58657 / 151125 |
| `abcs_battle01` return | 60945 / 157461 |
| Shared after-tail / enclosing `ExecuteAfterBattleCutscene` return | 60945 / 157462, 157463 |
| F401 clear / F501 set returns | 60945 / 157466, 157468 |
| BattleLoop D4=1 / SwitchMap / ExplorationLoop | 60945 / 157469, 157471, 157472 |
| Map57 selects exact `ms_Void` at `0x477E8` | 60983 / 157549 |
| `controllable-5b` terminal | 61010 / 157607 |

All **67** reached after-program operation entry/return pairs match. The static fixture's 80
source operations include embedded action records and trailing entity data; they are not 80
runtime iterations. Independent native review also verifies 106 audio dispatch/mailbox pairs in
this final segment; that bounded evidence does not establish full 8D presentation coverage.

Two consecutive completed neutral frames, **61009 and 61010**, retain Map57, player `(5,12)`,
raw `(1920,4608)`, facing 3 (DOWN), battle sentinel 255, F401 false and F501 true. Original player
control and field-action input polls are observed; the final player poll is at `0x4FF8`, frame
61010, value 0. Movement and camera are settled; map-event word, typewriting, pending returns and
all active-consumer counts are zero. No map program, battle return, transfer or modal remains.
The generic window-state byte is 1; actual dialogue/portrait predicates, rather than that byte
alone, establish readiness. Party, joined and active roster are `[0,1,2]`; gold is 420.

| Ally | Level | HP / MP | Items | Spells |
| --- | --- | --- | --- | --- |
| Bowie | 1 | 12/12, 8/8 | `[199,127,127,127]` | `[10,63,63,63]` |
| Sarah | 2 | 12/12, 12/12 | `[213,127,127,127]` | `[0,63,63,63]` |
| Chester | 2 | 12/12, 0/0 | `[184,127,127,127]` | `[63,63,63,63]` |

All three status-effect fields are zero; no herbs remain and equipped weapons are retained.
The player field entity determines the endpoint position: ally battle-record positions are not
additional field entities. RNG bytes are `[188,203,0,0]`, copy 188; raw time fields are frame 170,
seconds 674 and secondsFrames 40. Full original records remain in the private terminal evidence.

All 19 native invocations exit 0 without timeout or forced termination, restore their recorded
entry state, clear callbacks, delete session ROMs and preserve canonical ROM identity. Final 86
reports `loaded-segment-entry` restoration and host `OBSERVATION-COMPLETE-UNREVIEWED`; that status
does not itself grant independent acceptance. Native/operator process inspection finds no owned
survivor. Segment 86 costs 4534 frames / 83 batches / 108.69000169995707 seconds. The new chain adds
19 starts / 1791.716448599822 seconds / 61010 frames / 2798 batches, yielding complete historical
**115 / 14197.020479699888 / 365304 / 13351** totals. Terminal-pair accounting, production
lineage reconciliation and the complete native summary agree. Previous failures and cleanup
Unknowns, including 21/41/61/67, remain unchanged.

Reproduce the retained result without launching native, after loading `local/private-inputs.ps1`:

```powershell
uv run python -X utf8 local/issue496/audit-victory-terminal.py prepared-86 <fresh-output.json>
uv run python -X utf8 local/issue496/compare-winning-checkpoints.py <fresh-output.json>
uv run python -X utf8 local/issue496/victory-native-summary.py 86 <fresh-output.json>
```

`stable86-audit-01.json`, `stable86-checkpoints.json` and `victory-native-through86.json` retain
passing executor results. Main-gate's independent `root-final-chain-review.json`,
`prepared-86-root-final-structure.json`, `prepared-86-root-final-semantic.json` and
`root-final-scenes-batch.jsonl` verify the full chain, terminal structure/state/consumers and scenes.
These local readers and reports remain ignored inspection aids; the tracked runner/observer at
the exact accepted Git object, original source and private native records own the observations.
PR503's completed source checks (normal verification, actual Lua/host and ROM bindings) remain
recorded in its handoff. This four-document result requires direct document/scope checks and its
committed verification plan, not another native or aggregate suite.

**Unknown at the PR504 boundary:** that terminal collector executes no post-endpoint nonneutral ordinary input. RA-12's
requirement that the next ordinary logical input be accepted and its state effect observed remains
unchanged and unproved. Bounded two-frame stable 5B readiness is Confirmed; full RA-12, remaining
8D, frozen replay, H4 and completion of #437 are not claimed. This result is savestate-linked
original acquisition, not uninterrupted wall-time execution. No additional native launch is part
of that result handoff. The later Issue515 result below observes the additional input without
loading or modifying prepared-86.

### Retained HEAL consumer evidence

This is a readback of the accepted PR504 acquisition above, not another native run. Its original
source remains `c834c652b6862bc5679fd7f69a38a7093206efc6`; its collector is the tracked runner and
`tools/bizhawk/map3_messenger_acceptance_observer.lua` at
`9c3ea03ac5f5b467ee744f1ac624870da2408443`. The canonical ROM identity is at this document's top.
The private records remain in the research worktree's `local/issue496/`. Below, **CP** identifies
a one-based line in the named segment's `runtime/checkpoints.jsonl`, followed by its global
`order`. Frames are **observer frames**; callback-time `emulatorFrame` and completed-frame input
receipts remain distinct. These minimum facts do not publish the private trace or save state.

**Confirmed (original observations):** both selected actions reach HEAL 1 through ordinary magic,
level and target input, construct a single-target effect, and consume the resulting scene.

| Record | Construction and resolved target | Consumed result |
| --- | --- | --- |
| `prepared-78`, Sarah self-heal | CP74 / 82511 / frame32112: actor1, action1, spell0. CP76 / 82515 / frame32113, `battlesceneScript_ApplyActionEffect:before`: target count1, `[1]`. | CP277 / 83950 / frame32725, `EndBattlescene:after`: Sarah HP5→11, MP10→7. |
| `prepared-81`, Sarah heals Chester | CP354 / 114889 / frame44669: actor1, action1, spell0. CP356 / 114893 / frame44670 resolves count1, `[2]`. | CP561 / 116436 / frame45334: Chester HP5→11; Sarah HP7 unchanged and MP4→1. |

The `WriteBattlesceneScript:before` target arrays `[0,1]` and `[2,1]` still describe field selection
storage. They are not two consumed spell targets. The later effect caller owns that distinction.
Construction also temporarily changes resources; only the replay/scene endpoint proves persistent
recovery. The source `createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` writes the
caster's MP reaction before the casting animation; `castspell.asm:spellEffect_Heal` writes the
target's recovery reaction before message298. The retained outer callbacks do not sample the exact
runtime MP/HP command instants.

**Confirmed (saved settings and reached text branch):** `prepared-77/runtime/continuation.json`
contains original RAM bytes `02 00` at offsets `F717/F718`, respectively `MESSAGE_SPEED` and
`NO_BATTLE_MESSAGES_TOGGLE` in pinned `sf2const.asm`. The later `prepared-78` save has the same bytes.
This is actual saved RAM, not a substitution of `InitializeGameSettings` defaults. During self-heal,
CP118/130 (orders82685/82795, frames32177/32226) enter/return from `DisplayText`274; CP148/180
(83099/83249, frames32367/32426) do so for text298. Both return to `0x19200` in
`bsc10_displayMessage`. Other-target text298 is CP455/497 (115640/115810, frames44992/45055), with
the same return PC. Neither complete scene contains a `DisplayText`362 entry.

Pinned `battlescenes/battlesceneengine_0.asm:bsc10_displayMessage` skips the normal text if the
no-message toggle is set; after displaying it, only speed0 emits text362 (`{DICT}{W2}`). These
observations therefore bind the normal display and non-W2 trailing-wait branch. Exact speed2
throughout the action remains **Inferred**: command-time setting bytes were not sampled. Explicit
setting writers are `code/common/stats/newgame.asm`, `code/common/menus/battlefieldsettingswindow.asm`
and, for speed only, `data/maps/entries/map25/mapsetups/s2_entityevents.asm`. The selected Tower
movement/magic/target/scene route does not enter those setters; this is not an exhaustive proof
against every indirect RAM write.

**Confirmed (source-bound RNG calls):** the collector's `returned()` entry records `returnPc` and
its `rng:draw` record executes at that return PC, with ordered before/after shared seeds. The
following are reached calls in the self-heal, not an inferred fixed animation budget.

| `prepared-78` CP / order / frame | Return PC and pinned source owner | Observed call |
| --- | --- | --- |
| 138 / 83081, 141 / 83084, 144 / 83087; frame32363 | `0x1A8A6`, `0x1A8BC`, `0x1A8CE`; `battlescenes/animation/healingfairy.asm:spellanimationSetup_HealingFairy` | Ordered ranges32/30/12, results21/23/0. |
| 157 / 83148 / 32387; 210 / 83437 / 32504 | `0x1C59E`; `battlescenes/animation/update/healingfairy.asm` | Reentry range16. |
| 161 / 83154 / 32388 through 246 / 83675 / 32604, at the intervening named draw records | `0x1C72E`, same update owner | Periodic dust range12. |
| 204 / 83405 and 207 / 83408; frame32491. 249 / 83682 and 252 / 83685; frame32606 | `0x1C6F8`, `0x1C70C`, same update owner | X-boundary ranges28/32 in that order. |

The other-target setup has the same three caller PCs at CP418/421/424,
orders115459/115462/115465, frame44920. The two award RNG operations occur during construction;
their nested `GenerateRandomOrDebugNumber` and `GenerateRandomNumber` records must not be counted
as four independent seed steps. A draw gap gives neither the number of no-draw updates nor their
gate state.

#### Static VInt installation and HEAL ordering

The following audit is **Confirmed for the pinned source flow**, including the transitive window
and text calls reached by these ordinary HEAL scenes. It is not a sampled slot inventory at every
original interrupt. Paths in this table are relative to `disasm/code/`.

| Boundary and source | Installed services or ordering |
| --- | --- |
| `gameflow/battle/battlescenes/initializebattlescene.asm`; `common/tech/interrupts/trap9_contextualfunctions.asm` | After fade-out, `VINTS_CLEAR` clears all eight pointers and the enabled bitfield. Trap9's add selects the first empty slot and sets its bit. Later initialization adds `VInt_UpdateBattlesceneGraphics` first, then `VInt_UpdateWindows`: slots0/1 and bitfield3 at that completed installation boundary. The old field callbacks do not survive the clear. |
| Transitive `common/menus/ministatuswindow.asm:CreateBattlesceneMiniStatusWindows`, its open/close helpers and `BuildMiniStatusWindow`; `common/windows/windowengine.asm` | Creation calls `InitializeWindowProperties` and `CreateWindow` twice between the two installs. These reset window records/indices and use layout/stat/font, move and DMA helpers; they do not install or remove a contextual function. Open/close and `WaitForWindowMovementEnd` consume the installed window callback. `sub_19B0`/`sub_1942` in `common/tech/graphics/graphics_2.asm` manage sprite state/links, not the slot table. |
| `common/scripting/text/textfunctions_1.asm` and `textfunctions_2.asm`; `common/windows/windowengine.asm:VInt_UpdateWindows` | Display/create/typewrite/close use window records and VInt waits, without a further VInt registration. Pinned `gamescript.txt` entries **0112/012A (hex)** are decimal274/298 and have no W1/W2 or portrait operation. The contextual portrait/member/timer installers are in their separate menu owners, not this call chain. `InitializeWindowProperties` clears portrait/dialogue/timer indices; `CloseDialogueWindow` is not `ClosePortraitWindow`. |
| `common/tech/interrupts/vdpcontrol.asm`, `vintengine_1.asm`, `vint.asm` | Enable/disable helpers control interrupts/`VINT_ENABLED`, not slot allocation. `WaitForVInt` arms `ENABLE_VINT` and waits; `Sleep(n)` requests n such waits. `VInt` checks the enable bit, processes DMA/fade/Z80 work, then invokes enabled slots ascending. It clears `WAITING_NEXT_VINT` afterward and rearms from `VINT_ENABLED`. A busy-wait caller therefore must not be modelled as zero opportunities solely because it has no explicit `Sleep`. |
| `gameflow/battle/battlescenes/battlesceneengine_4.asm:VInt_UpdateBattlesceneGraphics` | Enemy idle, ally idle, status animation and `sub_1F282` precede `UpdateSpellanimation`; enemy/ally position and sprite linking follow. The installed window service follows this callback. This gives a source partial order, not a universal game-wide service list. |
| `gameflow/battle/battlescenes/updatespellanimation.asm:ReinitializeSceneAfterSpell`; `battlesceneengine_0.asm:bsc0D_endAnimation` and `EndBattlescene` | Spell cleanup clears properties/lifetime/control/toggle and waits for VInt; it does not remove either contextual service. `bsc0D` requests control2, waits for toggle0, restores palette/background state and waits again. `EndBattlescene` has its own timed input wait and closes mini-status windows; it still does not restore field callbacks. |
| `gameflow/battle/battlefunctions/executeindividualturn.asm`, `loadBattle.asm`; `gameflow/battle/battlevints.asm:SetBaseVIntFunctions` | After scene return, `LoadBattle` clears callbacks; after map/entity loading, `SetBaseVIntFunctions` clears again and installs map planes, entities, view, scrolling, sprites, windows and map animations. The special timer addition is guarded by battle44, not Battle01. |

The audit excludes unrelated full-screen/map-script transitions and the invalid-combatant fatal
branch in `combatantstats_3.asm`, which deactivates callbacks and never returns. Valid actor/target
records and completed scene returns bind the selected ordinary path. Static slot allocation is
therefore no longer an unconstrained service-inventory question. These ordinary traces do not record
its live enable state or exact interrupt opportunities; the separately admitted diagnostic below
records them for one neutral self-HEAL1 scene.

The ordinary HEAL script and dispatcher impose this further **Confirmed source order**:

1. MP resource reaction precedes PRST's casting action. `GetSpellanimation` supplies
   `HEALING_FAIRY`, unlike the Medical Herb's `NONE`. Fairy setup sends SFX77, executes the
   four `Sleep4`/`Sleep3` flash pairs, clears/loads spell graphics, makes its setup draws and finally
   sets lifetime`FFFF`, current animation and control1. The scene-data clear initially covers the
   control/toggle fields; this fairy is not already enabled during its own pre-setup flash.
2. `bsc01_animateAllyAction` calls setup at the selected sequence trigger before that frame's
   `Sleep`. Its header termination check is separate. The accepted
   [animation fixture](../../tests/fixtures/h2/battle-sprite-animation-static-v1.json) binds zero
   terminate bytes for all retail sequences; completion of the casting frames does not stop HEAL.
3. `battleactions/animateaction.asm:battlesceneScript_SwitchTargets` writes its wait/switch only
   when the displayed combatant changes. Recovery mode2 and text298 follow. In `DisplayText`,
   `HandleDialogueTypewriting` has its own per-character waits/input shortcut. The subsequent
   `bsc10` nonzero-speed loop at `loc_1921A` tests input **before** `WaitForVInt`/`DBF`; for a stable
   speed2 and no input it requests65 waits (64 initial counter, inclusive DBF). This conditional
   source count is neither a whole-scene duration nor proof of natural fairy update counts.
   W2's separate range256/copy/VInt/input preamble is absent from this reached trailing-wait branch.
4. `WriteBattlesceneScript` next writes the ordinary make-idle command. The regular
   `bsc05_makeAllyIdle` path itself sleeps using `BATTLESCENE_ALLYBATTLESPRITE_ANIMATION_SPEED`;
   it does not end the spell. Only afterward does the script's `endAnimation` request `bsc0D`.
   `UpdateSpellanimation` gates on toggle/control, clears lifetime for control2 and decrements a
   nonzero lifetime before dispatch. `FFFF` is a decrementing word, not an immutable sentinel.
   Fairy phase3 with lifetime0 retires an instance; the last active instance invokes cleanup.
   X-boundary range28/32 and dust range12 may occur in the same update. `bsc0C` control3 is the
   distinct forced-cleanup path, not the ordinary HEAL terminator.
5. `battleactions/battleactionsengine_2.asm:battlesceneScript_End` places stop before switching
   back to the actor and awarding EXP. Its **construction** callback in self-heal is CP88..103,
   frame32113; it cannot be cited as execution of the stop. The last observed fairy draw is
   frame32606, while reward text263 starts at CP255/order83698/frame32611. The stop drain completes
   before that reward, but the last draw does not identify either the stop request or toggle-clear
   instant. No fixed fairy lifetime follows from this external bound.

The retained ordinary observer lacks individual bsc instants, no-draw fairy updates/properties,
live enable gates, stop/cleanup timing and the actual `loc_1921A` input read. Its stale W1/W2
`lastConsumerPoll` is not that read. The [accepted diagnostic](#accepted-neutral-self-heal1-diagnostic)
supplies these fields for its one neutral self-HEAL1 scene. **Unknown:** other settings/targets and
an alternative explicit Wait/input stream at the timed consumer. These limits concern opportunity
admission and state transitions, not missing setup/RNG callers or an unbound choice between W2 and
timed battle text.

#### Rechecking the retained HEAL facts

Set `$researchWorktree` to the existing evidence-owning worktree and `$pinnedUpstream` to the local
pinned checkout root. These commands read records/source only; they do not load a save or launch
an emulator. Read the source files named above at the fixed Git object, for example:

```powershell
git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlescenes/initializebattlescene.asm'
git show '9c3ea03ac5f5b467ee744f1ac624870da2408443:tools/bizhawk/map3_messenger_acceptance_observer.lua'
$records = Join-Path $researchWorktree 'local/issue496'
. ./local/private-inputs.ps1
@'
import json, sys
from pathlib import Path
root = Path(sys.argv[1])
for name in ("prepared-77", "prepared-78"):
    saved = json.loads((root / name / "runtime/continuation.json").read_text())
    ram = bytes.fromhex(saved["original"]["ramHex"])
    assert ram[0xF717:0xF719] == bytes((2, 0))
    print(name, "saved settings", list(ram[0xF717:0xF719]))
for name, first, effect, last, expected in (
    ("prepared-78", 74, 76, 277, (1, 5, 11, 10, 7)),
    ("prepared-81", 354, 356, 561, (2, 5, 11, 4, 1)),
):
    rows = {}
    with (root / name / "runtime/checkpoints.jsonl").open(encoding="utf-8") as stream:
        for line, raw in enumerate(stream, 1):
            if first <= line <= last:
                rows[line] = json.loads(raw)
            if line >= last:
                break
    before, resolved, after = (rows[n]["facts"] for n in (first, effect, last))
    assert before["actor"] == 1 and before["action"] == 1 and before["itemOrSpell"] == 0
    target, hp0, hp1, mp0, mp1 = expected
    assert resolved["targetCount"] == 1 and resolved["targets"] == [target]
    party0 = {x["id"]: x for x in before["accounting"]["combatants"]}
    party1 = {x["id"]: x for x in after["accounting"]["combatants"]}
    assert (party0[target]["hpCurrent"], party1[target]["hpCurrent"],
            party0[1]["mpCurrent"], party1[1]["mpCurrent"]) == (hp0, hp1, mp0, mp1)
    texts = [x["facts"]["target"] for x in rows.values() if x["kind"] == "DisplayText:entry"]
    assert 274 in texts and 298 in texts and 362 not in texts
    print(name, "target/resources", expected, "text entries", texts)
    for line, row in rows.items():
        if row["kind"] == "rng:draw" and row["facts"]["source"] == "GenerateRandomNumber":
            f = row["facts"]
            print(line, row["order"], row["frame"], hex(row["pc"]),
                  f["before"]["d6"] & 65535, f["d7"] & 65535)
'@ | uv run python -X utf8 - $records
```

For ordinary-input reproduction inspect `prepared-78/runtime/bridge/receipt.json` requests1..17
and join `actual-inputs.jsonl` by request `id` and global `order`. Parent `prepared-77` is the
nonterminal, resumable movement save at frame32013/rank54; child78 CP1/order82206 verifies its
historical load before input. The next selected HEAL starts99 observer frames later. Other-target
parent80 is likewise nonterminal at44272/rank78, with the selected child81 action397 frames later.
These are retained save genealogies, not permission to restore them with a changed observer:
`SEGMENT_IDENTITIES` requires matching runner/observer/execution sources at preparation and launch.
Additional instrumentation would need an independently accepted compatibility boundary; terminal86
remains forbidden as a resume parent. No new fixture, collector change or original acquisition is
required merely to reproduce the facts documented here.

### Instrumented HEAL diagnostic method (Issue #546)

This capability extends the existing Map3 acquisition rail for the remaining dynamic fields in
[retained HEAL consumer evidence](#retained-heal-consumer-evidence). It grants no native launch or
new evidence acceptance. The first supported diagnostic observes one Sarah self-targeted HEAL1
with neutral scene input. An input-shortening comparison remains unimplemented and **Unknown**.
Static scene slot installation/order, existing RNG callers and historical outcomes retain their
original owners; they are not collected again to manufacture additional coverage.

The explicit `diagnostic_kind="heal1-consumer-diagnostic"` prepare/run selection requires the
accepted Issue515 `prepared-11` nonterminal parent: ordinal9, neutral completed frame32013,
battle-player-movement rank54. Its own child12 records a successful historical load before input.
That parent's exposed 68K RAM/registers/emulatorFrame/core match Issue496 parent77, but this does
not compare Z80/VDP/audio or all serialized core state. Admission uses Issue515's own lineage and
native evidence. Old parent77's raw-settings migration is outside this method; terminal86 and
Issue515 terminal20 remain forbidden parents.

Ordinary preparation, launch and ordinary lineage edges retain all `SEGMENT_IDENTITIES` equality
checks. The diagnostic records its immutable parent identities, sealed ancestry and settings identity
separately from its current observer/runner and additive source/PC/ROM bindings. All old source
bindings, helpers, fixtures, ROM, H1, tools and continuation remain exact. It requires the existing
`bizhawk-local-path-roles-v1` configuration mechanism, which replaces only verified local path roles
while preserving all other settings and the raw config digest. It does not convert a legacy parent,
whitelist changed hashes or claim that code identity proves native state compatibility. Preparation
freezes current source/configuration/input material; any later code edit invalidates that candidate.

New passive callbacks retain named bsc entry/return with A6 and the preceding stream word, actual
timed-text reads/end, command-time message settings, VInt gates/slots/service context, setup enable,
update entry/return including zero draws and fairy properties, runtime stop/control and cleanup clear.
Existing RNG and global order logs are reused. Measurement returns have their own dispatch storage;
they do not change ordinary pending/consumer state or saved continuation keys. The resolved action
must be Sarah's HEAL1 targeting herself. Only neutral input is admitted after construction starts;
this does not pretend that the old W1/W2 poll admits a timed-text shortening input.

The diagnostic's `vintCount` is the monotonic observed interrupt count; `vint` is the current
activation ID (zero outside VInt), with `vintParent` and `vintDepth` identifying nesting. A VInt
entry records the interrupted service separately as `interruptedService` and begins with
`service=false`; its return restores the interrupted activation and service. Service returns restore
their prior service, including LIFO tail calls sharing one return PC/stack. This is necessary because
pinned `VInt`'s `andi #$F800,sr` clears the interrupt mask despite its misleading source comment.
Graphics can call `UpdateSpellanimation`, tail-dispatch HealingFairy, branch to
`ReinitializeSceneAfterSpell`, and tail-call `WaitForVInt` while the outer graphics service remains
active. The source owners are `code/common/tech/interrupts/vint.asm` and
`code/gameflow/battle/battlescenes/{updatespellanimation.asm,animation/update/healingfairy.asm}`
at pinned revision `c834c652b6862bc5679fd7f69a38a7093206efc6`.

**Confirmed (offline observer behavior only):** direct current Lua execution with mocked host APIs
reproduces outer graphics/update/fairy/cleanup, nested VInt graphics/windows, then outer
cleanup/fairy/update/graphics returns. Every record retains its activation/parent/depth/service;
the inner entry does not inherit outer graphics, and the outer return retains its original ID while
the count remains increased. Shared-slot service tail returns also restore in LIFO order. Ordinary
observer state is unchanged except checkpoint/order output; measurement returns and VInt contexts
finish empty. Reproduce in a fresh ignored output with same-process private configuration and
`uv run --locked python -X utf8 local/issue546/check-context.py <fresh-output-name>`.
The retained pre-correction run records the completed outer-cleanup attribution failure; neither
that failure nor the corrected mock execution is native HEAL reach evidence.

The terminal is the matched `EndBattlescene` return followed by completion of its current emulator
frame. No subsequent host step is admitted. Callback-time facts and completed-frame facts remain
separate: this is not an instruction-exact CPU stop and does not prove an absence of same-frame
downstream effects. The result is `HEAL-DIAGNOSTIC-COMPLETE-UNREVIEWED` only with the required
observations, empty measurement-return/VInt contexts, no active service, neutral scene frames and
clean callback/entry-state/process/ROM cleanup. Save commands,
segment-pair publication and diagnostic descendants are prohibited. Missing coverage or failed
cleanup remains failure/Unknown, with raw logs retained.

An authorized native invocation appends a typed diagnostic leaf through the existing exclusive
`resumed-by*.json` claims. Sealed parents, metadata and prior claims remain unchanged. Full lineage
reconciliation charges successful and failed diagnostic receipts alongside ordinary attempts; missing
or incomplete process/input/cleanup receipts block continuation. Historical totals through Issue515
remain 136 starts / 15540.144989400113 seconds / 428408 frames / 16188 batches, carrying the earlier
115→117 history. Source-admission H3 gates and later tooling costs remain separately recorded.

The prepared per-attempt containment is **2400 delivered frames, 256 advancing batches and 180
active seconds**, measured above inherited totals. These are finite attempt bounds, not a permanent
launch quota, renewed cumulative budget or restoration of retired stabilization ceilings. Existing
60-second startup/exchange, 120-second idle/disconnected and 3+3-second teardown containment remains.
The natural bridge's 7200 setting is not a whole-run hard wall timer. Lua checks diagnostic active
time at command/frame boundaries and bounds socket idle waits; bridge exchange containment covers
an unresponsive emulator. Cleanup time is still charged in the completed receipt.

Offline preparation uses the existing API with `interactive=True`, `continuation=VICTORY_CONTINUATION`,
`segment=10`, the explicit parent directory, `diagnostic_kind=HEAL_DIAGNOSTIC` and
`proposed_timeout_seconds=7200`; it takes no prior-cost overrides. Use a fresh worktree-local ignored
output and freeze source first. Preparation creates no runtime claim and starts no emulator.
Source/configuration guards and direct Lua execution with mocked host APIs establish only offline
behavior. Each native use requires separate admission, exclusive registered-installation ownership
and independent result review. The accepted instance below establishes only its named restore and
observation boundary. Other restores and broader native compatibility remain **Unknown**; the old
frozen replay remains disabled.

#### Accepted neutral self-HEAL1 diagnostic

**Confirmed (bounded native observation, independently reviewed):** private
`local/issue546/prepared-04` ran once from its own accepted Issue515 `prepared-11` parent under
source commit `3e69c2d49c8105c0e485dcbea70c688b57a86902`. It used the USA retail ROM
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, the pinned
SF2DISASM revision named above, BizHawk2.11.1/Genplus-gx and the registered installation. Its frozen
`candidate.json` binds all ten ancestry pairs, seven execution helpers, tool identities, settings
and the additive eight-source/35-PC observations. Raw execution identities are:

| Material | SHA256 |
| --- | --- |
| Observer | `688C67CAD7FDEA3B8B27FFF3EC184FEE3ED4934E21B534D791BD0D59875DE483` |
| Runner | `8BDB8303CCE6FDDA516D3B5193DA1DC5BC4503D452A3F889C0F878EDE53EE9C4` |
| Configuration | `F2DABDE77E1CBC9CFA92D34496125CDF7C129545724990325F47D46C39F7A360` |
| Input | `05562B5546BEBE81C0EA462E2285268CA6CB943091F7B5A01536CD6D94C4AA95` |

Prepared01/02/03 remain invalidated and unexecuted. The update onto777dc709 changed raw observer/runner
bytes on checkout despite identical Git content, and the preserved pre-native identity check rejected
prepared03 before any claim/process; fresh04 bound the executed bytes. Later documentation commits
or rebased heads are not new execution identities. Neither raw evidence nor the parent was migrated.

The production host records `HEAL-DIAGNOSTIC-COMPLETE-UNREVIEWED`; independent review accepted its
named boundary without rewriting that original status. Under `prepared-04/runtime/`, the raw owners
are `checkpoints.jsonl`, `actual-inputs.jsonl`, `observer.observed.json`, `observer.status.txt`,
`host-status.json` and `bridge/{receipt.json,receipt.jsonl,lua-status.json}`. CP below means a one-based
line in this checkpoint file, not a line in any older trace. All18 own-child12 prefix states (hello
plus17 replies) matched frame/original-state/accounting readbacks before the diagnostic scene.

| Seam | CP | Frame / global order | Observed boundary |
| --- | --- | --- | --- |
| loaded before input | 1 | 32013 / 82206 | parent9, child10; no input before load/readback |
| construction / resolved target | 76 / 83 | 32112 / 82513; 32113 / 82522 | actor1/action1/spell0; sole target1 |
| playback | 452 | 32177 / 83019 | bsc runtime begins, distinct from construction |
| setup enabled | 2042 | 32363 / 84985 | toggle1/control1/lifetime65535; cumulative draws5 |
| first zero-draw / draw-bearing fairy return | 2048 / 2297 | 32364 / 84993; 32387 / 85288 | graphics service; drawCount0 / 1 |
| stop request / control write | 3729 / 3730 | 32517 / 86982; 32517 / 86983 | control1→2; toggle1/lifetime65381 |
| cleanup entry / cleared | 4664 / 4665 | 32607 / 88099; 32607 / 88100 | activation487/graphics; control2→0, toggle/lifetime0 |
| nested VInt | 4666 | 32608 / 88103 | current488/parent487/depth2; service false, interrupted graphics |
| outer cleanup / fairy return | 4674 / 4675 | 32608 / 88111; 32608 / 88112 | count488/current487/depth1/graphics restored |
| stop handler return | 4689 | 32609 / 88128 | outside VInt/service, toggle/control/lifetime0 |
| matched `EndBattlescene` return | 5650 | 32725 / 89323 | scene seam; no subsequent host step |

All5373 HEAL records reconcile activation/parent/depth/service context. Entries/returns balance for
605 VInts (one nested activation),576 graphics/controller updates,601 windows services and244 fairy
updates. Fairy draw counts are0 for217 calls,1 for25 and2 for2:27 draw-bearing updates,29 draws inside
those updates. The existing scoped `GenerateRandomNumber` records total34, distinct from the fairy
count; observed cumulative draws are2 at setup entry,5 at setup enable and34 after cleanup. These
are observed counts and deltas, never fixed gameplay durations, callback quotas or production rules.

The live graphics slot is0 with vector `[126508,18824,0,0,0,0,0,0]`. Its observed tuples
`(VINT_PARAMETERS,VINT_ENABLED,enabledSlots)` are `(0,128,3)` and `(8,128,3)`. Windows observes that
scene vector at slot1 and the field vector at slot5; enabledSlots3/127 remains tied to each recorded
phase. This confirms live opportunities for this run, not a second discovery of static installation.
`properties`, `fairy`, `toggle`, `control` and `lifetime` accompany the named update records;28 property
byte offsets vary across fairy entry/return snapshots, as do fairy offsets3/5/7/9/11. CP4654 is the
last draw-bearing fairy return (frame32606/order88087, drawCount2); CP4675 is a later zero-draw
return spanning the nested cleanup. The full byte snapshots remain private, with raw locators here.

All observed message settings are speed2/noMessages0. All130 actual timed-input reads observe0:
first CP859/frame32226/order83524, last CP3441/frame32490/order86640. Timed-input ends are
CP1444/frame32291/order84241 and CP3458/frame32491/order86659. The12 named bsc entry/return pairs
retain A6/preceding opcode alongside these live fields. Neutral input is confirmed from construction
through the final frame; no shortening input was tested.

The final delivered frame is32725, input order89326, emulator beforeFrame32724/afterFrame32725.
CP5652/order89325 retains a same-frame `after-stop:callback` at PC153104, role
`candidate:battle-lifecycle`. Thus the accepted endpoint is the scene return plus completion of its
current frame, not an instruction-exact stop or absence of downstream same-frame callbacks.

Typed parent claim `resumed-by-0002.json` charges1 start/28.409386800020002 active seconds/712
frames/23 batches. The reconciled acquisition total is137/15568.554376200133/429120/16211 in the
same units. Operator wall44.7686604000628 seconds is auxiliary; the separately passed source-admission
H3 gates cost2 starts/187.82799699995667 command wall seconds and are not acquisition active time.
All prior costs/failures and21/41/61/67 cleanup Unknowns remain retained.

Native exit0, no timeout/forced termination, receipt/journal equality, callbacks cleared and
`sessionStateRestored=true` establish the **loaded-segment-entry** restoration mode. The legacy
individual-bootstrap restoration flags remain false in this mode; it does not claim the ordinary
H3 fifteen-field profile. The session ROM was deleted, canonical identity unchanged and no owned
process survived. Measurement returns/VInt contexts are0, service false; no save/pair/descendant
exists. Shared file metadata, settings/user bytes and450 release members remained unchanged.

The first offline post-run audit failed by applying the strict launch-time validator to the expanded
exit config; its log/script remain `local/issue546/diagnostic-04-audit-failure-01.txt` and
`audit-diagnostic-04-before-postrun-fix.py`. BizHawk exit serialization normalized separators and
expanded20 path roles to284 with unused-platform/recent-ROM defaults. Read-only examination verifies
all20 selected GEN/Global resolved roles still equal their exact local launch destinations; the
shared installation is unchanged. The launch receipt's settings digest is not the rewritten exit
file's digest. No launch guard, raw artifact or source was changed to pass, and no native retry ran.

**Unknown:** other message settings/targets/spells, input-shortening behavior, broader natural-route
or HEAL coverage, and hidden-state equivalence to other parents. This one accepted restore/scene
does not establish general hardware equivalence, remaining8D/H4 or #437 completion, and grants no
new execution. Static premises and historical outcomes retain their existing owners.

#### Recovery and make-idle opportunity attribution

This readback uses the accepted `prepared-04` records and the pinned source above; it adds no
execution or broader gameplay admission. All CP numbers are one-based checkpoint lines. Frames
below are observer `frame`, not `emulatorFrame`. A VInt activation is an interrupt, not proof of a
`WaitForVInt` call. The [remake comparison](../../remake/docs/presentation-and-assets.md#separate-comparison-boundaries)
retains the separate continuous-cursor failure.

**Confirmed (recovery):** CP2059–2091 spans `bsc0B_executeAllyReaction`, frames32364–32367,
orders85004–85042. A6 progresses from `00FF0022` at entry to `00FF002A` at return. Its three VInts
245–247 are each parent0/depth1, each with one graphics service, window service and fairy update.
Lifetime changes `FFFE→FFFB`; seed remains `00920000`, control/toggle1. VInt entries CP2060/2070/2080
have A6=`FFFFDE80`/`00FF0028`/`00FF0028`; the matching returns preserve those register values.
The preceding `CloseDialogueWindow` returns before bsc0B without an intervening VInt.

**Confirmed (source):** paths in this table are relative to pinned `disasm/`.

| Owner / symbols | Relevant source behavior |
| --- | --- |
| `code/gameflow/battle/battlescenes/battlesceneengine_0.asm`: `bsc0B_executeAllyReaction` | The `byte_FFB588` busy spin precedes argument consumption, while A6 is still the entry script cursor. HP/MP/status consume three words before the mini-window call; mode consumes one afterward. Healing mode2 skips the damage-specific sleeps. |
| `code/common/menus/ministatuswindow.asm`: `OpenAllyBattlesceneMiniStatusWindow`, `BuildMiniStatusWindow`, `WriteStatValue` | Numeric window construction precedes `MoveWindow` with length1, explicit `WaitForVInt`, bar-tile DMA enqueue, then `WaitForWindowMovementEnd`. Bar-tile enqueue does not wait. |
| `code/common/menus/menuenginecommon.asm`: `WriteTilesFromNumber`; `code/common/scripting/text/asciinumber.asm`: `WriteAsciiNumber`; `sf2const.asm`: `LOADED_NUMBER` | Number rendering calls ASCII conversion, which saves A6, uses `LOADED_NUMBER=$FFDE80` as A6, then restores it. Conversion has no subroutine or wait calls. |
| `code/common/windows/windowengine.asm`: `MoveWindow`, `WaitForWindowMovementEnd`, `VInt_UpdateWindows` | Movement-end waits at least once, then repeats while `MOVING_WINDOWS_BITFIELD` is nonzero. The service clears/recomputes that field before advancing animation counters: a length1 move can be marked moving on its destination-reaching service, then cleared on the next. |

**Inferred:** VInt245 interrupts numeric conversion during window construction, before the two
explicit window waits; VInts246/247 serve those waits after script A6 restoration. This fits the
temporary number-buffer A6 and the complete source path. The busy spin's entry A6 does not explain
the observed `FFFFDE80` activation. A shorter busy interval is not excluded, and the movement-end
helper is conditional, not a universal fixed-one-opportunity operation. No missing unconditional
third wait is identified. Exact interrupted PCs and helper boundaries are not recorded.

**Confirmed (make-idle):** CP3460–3727 spans `bsc05_makeAllyIdle`, frames32491–32517,
orders86661–86980. It contains26 top-level VInts372–397 and26 fairy updates, all parent0/depth1.
Lifetime changes `FF7F→FF65`; seed changes `D8800000→ECE20000`. Register groups are:

| VInts | Entry CPs | A6 | D0 signature | Source attribution (**Inferred**) |
| --- | --- | --- | --- | --- |
| 372–373 | 3461,3471 | `FFFFFF78` | `3200`, `0` | first stack decompression |
| 374 | 3481 | `00FF0038` | `0900` | first DMA wait |
| 375–394 | 3491…3687 | `00FF0038` | low word19…0 | `Sleep(20)` |
| 395–396 | 3697,3707 | `FFFFFF78` | `0080`, `0004` | second stack decompression |
| 397 | 3717 | `00FF0038` | `0900` | second DMA wait |

**Confirmed (source):** `battlesceneengine_0.asm` calls `sub_1938C` on each side of the idle
`Sleep`; for a real frame it calls `sub_1942C`, which conditionally waits for pending graphics and
calls `battlesceneengine_1.asm:LoadAllyBattlespriteFrameAndWaitForDma`. That loader sets d0=`$900`
and calls `code/common/tech/interrupts/vintengine_3.asm:ApplyVIntVramDmaOnCompressedTiles` →
`DecompressTilesForVramDma` → `code/common/tech/graphics/decompression.asm:LoadStackCompressedData`.
The decoder links/restores A6 and performs local work without subroutine/wait calls. The DMA wrapper
restores d0, queues DMA, and the loader tail-calls `WaitForDmaQueueProcessing`.
`code/common/tech/interrupts/enabledmaqueueprocessing.asm` distinguishes queue enable (request bit
only) from queue wait (request then `WaitForVInt`). `vintengine_1.asm:Sleep` subtracts one and loops
`WaitForVInt` via DBF;20 iterations match the observed low-word19…0 sequence. Weapon queue enable
is not another wait; `WaitForBattlesceneGraphicsUpdate` is conditional, not an unconditional tick.

**Inferred:** four make-idle opportunities interrupt its two decompressions. Recovery and make-idle
thus share the broad distinction between interruptible CPU work and explicit waits, with different
source operations. Neither the observed3 nor26 is a general duration or a production correction
budget. These intervals have no nested VInts; the later nested cleanup remains a separate seam.

**Unknown:** the saved interrupt PC/SR/call stack, exact numeric field/instruction, live busy/window/
graphics-gate values, and opportunity counts for changed entry timing or content. Observer
`interruptedService` is a logical service label, not the saved CPU return PC. The register patterns
support the attribution but do not establish exact instructions or a universal CPU-work tick cost.
If stronger attribution is needed, the missing evidence is saved interrupt context and the disputed
helper/gate boundaries at these named activations; this statement authorizes no new acquisition.
The gameplay/RNG consequences and feasible semantic handling remain a separate unresolved question
under [ADR0010 Option A/8D](../decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a).
The counts alone neither require hardware simulation nor waive result differences.

To reproduce this bounded readback without execution, set `$researchWorktree` to the evidence owner
and `$pinnedUpstream` to its existing pinned source checkout. The output contains only the named
register/count facts, not private state payloads:

```powershell
$records = Join-Path $researchWorktree 'local/issue546/prepared-04/runtime/checkpoints.jsonl'
. ./local/private-inputs.ps1
@'
import json, sys
from collections import Counter
from pathlib import Path
rows = [json.loads(s) for s in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines()]
for start, end in ((2059, 2091), (3460, 3727)):
    part = rows[start - 1:end]
    print(start, end, dict(Counter(r["kind"] for r in part)))
    for cp, r in enumerate(part, start):
        if r["kind"] not in ("heal:bsc:before", "heal:bsc:after", "heal:vint", "heal:vint-return"):
            continue
        f = r["facts"]
        print(cp, r["kind"], r["frame"], r["order"],
              f'{f["a6"]:08X}', f'{f["d0"]:08X}',
              f["vint"], f["vintParent"], f["vintDepth"],
              f'{f["lifetime"]:04X}', bytes(r["state"]["rngBytes"]).hex().upper())
'@ | uv run python -X utf8 - $records
git -C $pinnedUpstream rev-parse HEAD
git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/scripting/text/asciinumber.asm'
git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/tech/interrupts/vintengine_3.asm'
```

Inspect the other named symbols from the same exact Git object; later source or remake changes
cannot substitute for that pinned original evidence.

#### Read back the accepted diagnostic without execution

Run from the evidence-owning worktree with `$researchWorktree` naming it. The production readers
below have the executed Python/Lua Git content; inspect commit3e69c2d4 if those owners later change.
The recipe reads raw evidence only: it never prepares, restores, advances or launches. The final
current-ledger print can include later append-only attempts; the asserted137 total is reconstructed
from this candidate's frozen prior costs and its own completed receipt.

```powershell
$records = Join-Path $researchWorktree 'local/issue546/prepared-04'
. ./local/private-inputs.ps1
@'
import json, sys
from collections import Counter
from pathlib import Path
from sf2tool.bizhawk_debug_bridge import read_receipt
from sf2tool.h3 import map3_messenger_acceptance as m
p = Path(sys.argv[1]); r = p / "runtime"
read = lambda path: json.loads(path.read_text(encoding="utf-8"))
prepared = read(p / "candidate.json")
for key in ("ObserverSha256", "RunnerSha256", "ConfigurationSha256", "InputSha256"):
    print(key, prepared[key])
receipt = read_receipt(r / "bridge")
assert receipt == read(r / "bridge/receipt.json")
m._assert_heal_diagnostic_output(r, prepared)
parent = Path(prepared["Segment"]["parentDirectory"])
pair, metadata = m._read_segment(parent)
m._assert_heal_diagnostic_parent(prepared, parent, pair, metadata)
rows = [json.loads(s) for s in (r / "checkpoints.jsonl").read_text().splitlines()]
heal = [x for x in rows if x["kind"].startswith("heal:")]
assert len(heal) == 5373
count = 0; context = (0, 0, False); interrupts = []; services = {}
for row in heal:
    kind, f = row["kind"][5:], row["facts"]
    if kind == "vint":
        assert f["interruptedService"] == context[2]
        interrupts.append(context); count += 1
        context = (count, context[0], False)
    if kind in ("graphics:before", "windows:before"):
        services.setdefault(context[0], []).append(context[2])
        context = (*context[:2], kind.split(":")[0])
    assert (f["vint"], f["vintParent"], f["service"]) == context
    assert (f["vintCount"], f["vintDepth"], f["inVint"]) == (count, len(interrupts), bool(interrupts))
    if kind in ("graphics:after", "windows:after"):
        assert context[2] == kind.split(":")[0]
        context = (*context[:2], services[context[0]].pop())
    if kind == "vint-return":
        assert context[2] is False
        context = interrupts.pop()
assert count == 605 and context == (0, 0, False) and not interrupts
assert not any(services.values())
fairy = [x["facts"]["drawCount"] for x in heal if x["kind"] == "heal:fairy:after"]
assert Counter(fairy) == {0: 217, 1: 25, 2: 2}
timed = [x for x in heal if x["kind"] == "heal:timed-input-read"]
assert len(timed) == 130 and all(x["facts"]["input"] == 0 for x in timed)
for line in (1, 76, 83, 2042, 3729, 3730, 4664, 4665, 4666, 4674, 4675, 4689, 5650, 5652):
    x = rows[line - 1]
    print(line, x["kind"], x["frame"], x["order"])
frames = [json.loads(s) for s in (r / "actual-inputs.jsonl").read_text().splitlines()]
frames = [x for x in frames if x["kind"] == "frame"]
assert len(frames) == 712 and len({x["id"] for x in frames}) == 23
assert (frames[-1]["frame"], frames[-1]["order"]) == (32725, 89326)
assert (prepared["HistoricalControlledStarts"] + 1,
        prepared["Segment"]["priorActiveSeconds"] + receipt["elapsedSeconds"],
        prepared["Segment"]["priorFrames"] + len(frames),
        prepared["Segment"]["priorBatches"] + 23) == (137, 15568.554376200133, 429120, 16211)
assert not (r / "segment.State").exists() and not (r / "segment-pair.json").exists()
assert not list(r.glob("resumed-by*.json"))
print("current reconciled ledger", m._resume_accounting(parent)[0])
print("PASS: bounded artifact readback; no native execution")
'@ | uv run --locked python -X utf8 - $records
```

### Post-victory ordinary-input preparation (Issue #515)

**Confirmed (source checkpoint):** accepted PR519 extended the existing victory continuation to
observe RA-12's then-missing nonneutral input after stable field readiness. This preparation alone
was not original-game evidence; the subsequent native result is recorded below.
Prepared-86 remains terminal/nonresumable and unchanged. Prepared-85 has incompatible execution
sources; PR511's new early pairs also have a different continuation selection. None is a legal
resume parent for this extension. The shortest compatible method is a fresh R1 chain reusing the
retained successful ordinary input requests, comparing each original response/checkpoint, then
continuing beyond the former terminal. This does not reopen frozen original-reference replay.

The pre-input boundary retains the existing two consecutive neutral settled frames and the entire
natural victory/after-program/flag/BattleLoop/SwitchMap lineage. It records map/player, flags,
party/combatants/items/spells/gold, RNG/time, camera, actual polls and zero blocking consumers, then
pauses the current batch without terminating. The operator sees `postVictoryInputReady`. Exactly
one one-frame direction request is allowed after that boundary; field menu buttons, multi-frame
directions and a second direction request are rejected without advancing. Only neutral continuation
is needed after delivery. No field checkpoint is newly made resumable.

The existing source/H1/ROM-bound `esc02_controlCharacter` (`0x4FF8`) and `loc_52E8` (`0x52E8`)
callbacks retain the original input selection and movement acceptance. The selected direction bit
must equal the observed byte in the delivery frame; the movement callback must occur in that same
frame. Its D4/D5 are the original destination increments, not host-computed movement. Terminal
`controllable-5b` now also requires changed coordinates on the same map and two further neutral
settled frames with the original readiness guards. The host checks input identity/order, both PCs,
zero consumers, unchanged no-battle/flag requirements and final raw coordinates against the observed
D4/D5 additions modulo 16 bits. Full before/input/after facts stay in the private observation and
checkpoint log; bridge replies retain the bounded terminal reference.

**Confirmed (offline only, 2026-09-22):** direct execution of the current Lua and Python terminal
validation with a mocked emulator boundary rejects the old battle sentinel, premature completion,
blocked/menu/repeated/multi-frame input, acceptance without displacement and a neutral input byte.
It preserves an accepted direction/read/movement/settled-result sequence. This validates collector
logic, not natural reach. Reproduce after loading `local/private-inputs.ps1` with
`uv run python -X utf8 local/issue515/check-field.py <fresh-output-name>`; the ignored driver reuses
the retained Lua host glue and executes current production blocks. Preparation uses
`local/issue515/prepare.py`; ordinary production-role, ROM, affected messenger and normal verification
checks plus the committed planner/CI belong in the exact-head handoff. No new verification-tool
tests, schemas, fixtures or registry entries are introduced.

Historical costs through PR504 remain **115 starts / 14197.020479699888 seconds / 365304 frames /
13351 batches**. Production lineage reconciliation of PR511's separate early parent/child adds
**2 / 53.36055679997662 / 2080 / 37**, so the new preparation carries segmented totals
**117 / 14250.381036499864 / 367384 / 13388** before any Issue515 native start.
Separate later tooling diagnostics also remain charged in `local/issue515/later-diagnostics.json`:
the PR507/509/511 short bridge comparisons have four starts, each 1030 frames and 228 advancing
requests; PR507 additionally has its separate 1030-frame continuous comparison. PR511's disconnect
and idle diagnostics preserve both starts, actual elapsed times and forced-disconnect containment.
These are separate diagnostic observations, not descendants or additional victory evidence; their
receipts and the prior ordinary H3 gate results remain with their tooling owners. Old 21/41/61/67
cleanup Unknowns and the disk-exhausted interrupted full verification remain unchanged.

The concrete preparation and source must pass independent main-gate admission before collection.
The intended first post-boundary request is one frame of Down, followed by neutral settling; actual
input/readiness/result remain original observations. If no acceptance or settled displacement occurs,
the finite operator preserves the failed attempt and stops rather than inventing success. Use the
registered installation, fresh worktree-local paths and strict versioned continuation-settings
identity. Old raw-settings pairs cannot be converted, resealed or normalized. Reuse successful
forward saves, preserve every new cost/failure, and return native evidence for independent acceptance
in this same Issue. Audio, remaining 8D, H4 and #437 completion are outside this source checkpoint.

### Native post-victory ordinary-input result (Issue #515)

**Confirmed (bounded native observation, 2026-09-22):** following independent PR519 source and
concrete-preparation admission, `local/issue515/prepared-02..20` completed a fresh compatible chain
under the same victory continuation. The accepted execution source is
`58c5a94a4c5349fd221a130a0970222962715528`; the worktree advanced to accepted
`7c90c147e6746567732a812c410fcf04b1ed0cc4` before segment 17 without changing the runner, observer
or any frozen execution dependency. Runner identity is
`0E41A02DD87532D0D178A5E574EEA3219CCB12977702A1A3824265AB7C8A8319`; observer identity is
`85520F65CE34292430F8779B8AA2EB4E5B739F63DEBFF0A284D9398598BFF061`. The unmodified initial
prepared-02 configuration is `D40091D71FAF2F48AAA585413E95CC5121D6A3F75F193DC4C06E86A1DB13210B`;
final prepared-20 configuration is `EA571EC14A60B29DDFBF6F6C3E43552DB0E14EB9B69E379CB157CBA99CD6911D`.
The pinned USA ROM, upstream, H1 and fixture identities remain those bound by this owner and each
private preparation. No old terminal save, source-incompatible pair or raw-settings identity was
converted or loaded.

All 18 resumable checkpoints match the corresponding old successful 68..85 `original` and `core`
objects in full, including saved RAM, registers and emulator frame. Every retained input response
also matches its original frame, position, flags, RNG/time and battle accounting. Each child loads
and validates its new compatible parent; no original state is injected after the declared R1 start.
All 19 pairs retain the same `bizhawk-local-path-roles-v1` settings identity
`16D3F8DCFF20863937B4F417A8AD2E7505164F54D63CCC2733120B1E9A1F9C33`, while preserving their own
raw settings identities and local writable paths.

The final segment preserves natural victory at **58657**, after-program/shared-tail/enclosing
return at **60945**, F401 clear before F501 set, BattleLoop D4=1, SwitchMap/exploration return,
and exact Map57 void setup at **60983**. All **67** after-program operation pairs and **106**
audio dispatch/mailbox pairs in that segment close; this remains bounded consumer evidence, not
complete presentation or audible-output acceptance. The pre-input facts at frames **61009/61010**
match the old prepared-86 terminal's position, flags, RNG/time, camera, player and complete battle
accounting. That old terminal remains unchanged and nonresumable.

| Boundary | Original observation |
| --- | --- |
| Pre-input settled frames 61009/61010 | Map57, `(5,12)`, raw `(1920,4608)`, facing 3 (DOWN); battle255, F401 false/F501 true; settled camera/movement and no pending battle, program, transfer or modal consumers |
| One Down frame 61011 | Request 84; `esc02_controlCharacter` at `0x4FF8` observes value 2 from `PLAYER_1_INPUT` (`0xFFDE97`), with D7=48 selecting that original input source |
| Movement acceptance, same frame 61011 | `loc_52E8` at `0x52E8`: D2=0, D3=32, D4=0, D5=384; the original destination increment is retained rather than supplied by the host |
| Final settled frames 61023/61024 | Map57, `(5,13)`, raw `(1920,4992)`, facing DOWN; position equals destination, battle255, F401 false/F501 true, actual neutral player/field polls and all blocking-consumer counts zero |

Exactly one nonneutral frame follows pre-input readiness, followed by **13 neutral frames**.
The final raw displacement equals the original D4/D5 additions modulo 16 bits. Party, joined and
active roster remain `[0,1,2]`, gold remains 420, and all ally records match the pre-input records,
including stats, zero status effects, items and spells described above. RNG remains
`[188,203,0,0]`, copy 188. Raw time advances from frame/seconds/secondsFrames `170/674/40` to
`184/674/54`. Direct reads of the saved original RAM agree with these serialized fields.
The final evidence pair is terminal and the production reader rejects it as a resume parent.

All **19** native invocations exit 0, without timeout or forced termination; callbacks clear,
entry core state is restored, session ROMs are removed and the canonical ROM remains unchanged.
For resumed segments the restoration kind is `loaded-segment-entry`; the original R1-specific
restoration flags are not substituted for that entry-core check. Shared installation file size/mtime
inventories remain unchanged after each run. No acquisition process survives. The final host status
is `OBSERVATION-COMPLETE-UNREVIEWED`: independent main-gate result acceptance remains required.
The observed direction/effect supplies RA-12's missing evidence for review; it does not itself
claim accepted RA-12 closure, remaining 8D, H4 or completion of #437.

New chain costs are **19 starts / 1289.7639529002481 seconds / 61024 frames / 2800 batches**.
The final segment contributes **4548 frames / 85 batches / 90.85220870003104 seconds**.
Complete segmented totals, including the prior costs above, are **136 starts /
15540.144989400113 seconds / 428408 frames / 16188 batches**. Frame-log counting, native receipts,
sealed pairs and production lineage reconciliation agree. The two source-admission H3 gates are
separate controlled observations: two starts and 183.27032080001663 seconds of wrapper elapsed
including preparation, not an invented emulator-active total. Later tooling diagnostics and old
21/41/61/67 cleanup Unknowns remain in their existing records.

One local operator guard failed before invoking the production runner for prepared-17: an unrelated
accepted `remake_h4_reference.py` addition made its broad `src/tools` comparison fail. No runtime
folder, process start or frame delivery occurred. The closed-pipe abort then raised OSError 22;
both diagnostics remain preserved. After advancing to accepted main and rechecking identical
frozen sources, the unchanged preparation ran successfully with separate retry logs. The local
wrapper now checks accepted ancestry plus the unchanged production identity checks; it does not
waive compatibility. The later account-limit interruption did not interrupt native execution:
prepared-19 and 20 completed normally and were audited on recovery, never rerun or relabeled FAIL.

Reproduce the completed evidence read-only after loading `local/private-inputs.ps1`:

```powershell
uv run python -X utf8 local/issue515/audit-terminal.py prepared-20 <fresh-output.json>
uv run python -X utf8 local/issue515/final-ram.py prepared-20 <fresh-output.json>
uv run python -X utf8 local/issue515/summary-native.py <fresh-output.json>
```

The retained results are `terminal-audit.json`, `prepared-20-final-ram.json`, `native-summary.json`
and each segment's audit/receipt/status files. These ignored readers and reports aid reproduction;
the accepted production source, pinned original inputs and native records own the evidence. PR519's
normal verification, affected checks, two actual H3 gates and public CI remain valid because this
result changes only documentation. Direct result, scope/link/private-boundary checks, the committed
result plan and actual CI are recorded in the handoff; no full suite or native-chain rerun is needed.

### Source binding and observation fields

Preparation reads the five accepted R3a/R3b/R3c/R3d/R4a fixtures and validates their named source
identities and canonical ROM anchor bytes. It reuses the existing H1 listing for named function and
input-read bindings; it does not rebuild H1 or create a fixture. The pinned US ROM and upstream
`c834c652b6862bc5679fd7f69a38a7093206efc6` identities remain the baseline. The observation uses:

| Original seam / owner | Recorded facts and boundary |
| --- | --- |
| `GenerateBattleTurnOrder`, `ExecuteIndividualTurn`, `ProcessBattleEntityControlPlayerInput`, `StartAiControl`, `ExecuteAiControl`, `ExecuteAiCommand` (R3a) | Ordered round/turn/actor, actual player/AI branch, entry and return registers, turn order/offset and complete combatant state. Repeated movement polls are not new turns. |
| `ControlBattleEntity` input read `0x22E70`, `ExecuteDiamondMenu`'s actual directional input read, `loc_23186` after `WaitForVInt`, target consumer entry/return | Actual movement entity and destination, button read, menu choice, target cursor/count/list and command result. Callback-time polls and frame-end snapshots are separate. |
| `WriteBattlesceneScript`, `battlesceneScript_ApplyActionEffect`, `battlesceneScript_DropEnemyItem`, `battlesceneScript_End` (R3a–c) | Before/after action selector, item/spell/slot, actor/target coordinates, attack type (including reached second/counter attacks), scene EXP/gold and all ally/enemy record bytes, including removed/dead slots. Existing parsed stats/items/spells and raw EXP remain available; no outcome is generated by the observer. |
| `GenerateRandomNumber`, `GenerateRandomOrDebugNumber` | Actual caller/return stack, incoming range registers, returned value registers and seed bytes before/after each reached call after admission. No RNG normalization or forcing. |
| `InitializeBattlescene`, `ExecuteBattlesceneScript`, `EndBattlescene`, `LoadBattle`, `ProcessAfterTurnEffects`, `ProcessKilledCombatants`, `CountRemainingCombatants` (R3d) | Scene request/consumer return, reload, after-turn changes, death processing and outcome counts. Existing text/entity-operation/audio request, mailbox-write and acknowledgement observations continue through the after-program. No pixel, waveform or hardware-equivalence claim. |
| `BattleLoop_Victory`, `ExecuteAfterBattleCutscene`, `abcs_battle01`, `EndAfterBattleCutscene`, `ClearFlag`, `SetFlag`, BattleLoop return, `SwitchMap`, `ExplorationLoop` (R4a) | Actual selected program, operation entry/completion and program return, after-routine/join return, F401 clear before F501 set, D4=1 and subsequent map/field return. Victory entry alone cannot finish acquisition. |

The ordinary `state`/step response adds `battlefield`: current original movement grid and its
source-defined dimensions, target list, action fields, registers, accounting and complete combatant
records with live mapped entity position/destination/facing. `MAP_ARRAY_BYTESIZE` is derived from
the original width×height expression and checked against its storage interval. These private facts
let a finite request/response operator choose movement and targets without reproducing game rules.

### Input and coherent battle checkpoints

All advancing actions remain original controller input. In the continuation after admission,
nonneutral steps contain exactly one frame. Input requires a fresh, active source consumer and a
released previous input. Movement, diamond-menu and target polls are distinct. Consumed confirmation
or cancellation invalidates the old poll; menu/target changes wait for a new actual poll. Neutral
steps may advance up to the existing 120-frame maximum and stop early at a new consumer boundary.
Dialogue acknowledgements still require an active text/wait consumer, preventing an automatic C
from becoming a battle/menu action. No script chooses a button inside the observer.

Movement, physical attack, STAY and ordinary diamond/target cancellation are available.
Heal 1 support reuses `ExecuteBattlefieldMagicMenu` and its nested `SelectSpellLevel`, then the same
original target consumer. Pinned `magicmenu.asm` and H1/ROM input reads bind `loc_10AD8` and
`loc_10CF4`; the selected entry must be exactly `SPELL_HEAL` (level 1). Its source definition and
ROM bytes at the H1 `table_SpellDefinitions` symbol bind spell 0 / MP cost 3. The reply exposes the
four original displayed spell words; D1 indexes the actual target list. Up/Left decrement and
Down/Right increment that original index. Both nested magic consumers must close before saving;
only the enclosing magic consumer is additionally discounted at the level input poll. Other
pending callers still block input. C/B consumption invalidates the old poll, including cancellation
back to the icon menu. The existing action-1/effect/RNG observations consume the original result;
no HP, MP, target, action or RNG is written by the observer.

The Medical Herb extension pins `itemmenu.asm`, `itemstats.asm`, `itemdefs.asm` and
`breakuseditem.asm` in addition to the retained player-dispatch/action sources. H1/ROM bind the
actual item input read at `loc_10616`, the complete first item definition at `table_ItemDefinitions`,
and its HEALIN-1 definition: consumable item 0, spell 16, range 0–1, radius 0, nominal power 10.
Actual restored HP, target validity and consumption remain original results, not observer writes.

The first diamond's source `D2` distinguishes battle STAY/SEARCH contexts from `MENU_ITEM`.
The second diamond is exposed as `battle-item-action`; only Use=0 or B cancellation is admitted.
Equip/Give/Drop confirmation is rejected before advancing input, with a defensive unsupported stop
at the original poll/return seams. `battle-item` then requires the preceding Use result and the
original `CreatePulsatingItemRangeGrid` callback. The four displayed item words and inventory
remain original; confirmation permits only Medical Herb after masking flags. Return D0 is the
item, D1 the actual inventory slot, checked against that actor's full item word. Item B returns to
the item-action diamond; target B returns to the top battle diamond. Each closed consumer clears
its poll; pending item/target/effect calls block saving and unrelated pending calls block input.
The same target consumer supplies the actual allowed allies. Before/after observations of
`battlesceneScript_UseItem`, `battlesceneScript_BreakUsedItem` and `RemoveItemBySlot` retain the
original action, item, slot, full inventories and HP/MP, alongside the existing effect/scene chain.
No herb effect or successful native item cancellation is claimed by the offline checks.

Entering `BattlefieldMenu`, selecting a manual spell other than Heal 1, any other manual item,
Egress or Angel Wing produces an explicit
`unsupported-battle-input` observation, not a fabricated continuation. AI spell/item/effect paths
remain observable. Defeat produces `observed-defeat`. Neither outcome is accepted as victory/5B.
The finite operator can use current grid/positions and the actual available targets; no fixed
actor, round count, winning route or reward is required. Existing equipped weapons need no Equip
operation; herbs can target an allowed ally without Give; Drop and field menus are unnecessary
for this route. Bowie's only recorded spell is Egress, which leaves the requested battle; Sarah's
recorded Heal is supported. No other presently necessary ordinary input was identified from this
inventory/spell/caller review. This is a bounded capability assessment, not full menu coverage.
A later extension of unsupported input
consumers needs source-bound readiness in this same owner.

Segments 1–3 retain the existing early/royal/guard checkpoints. In victory mode, segment 4 and later
may save at a completed neutral player-movement poll, with a settled actual actor and camera,
matching turn/moving/view actor, no event, text, dialogue/portrait, pending short consumer, map
program or audio dispatch. Palette modes 0/5 retain their source meanings. `CURRENT_BATTLEACTION`
is recorded but need not be zero: the original leaves STAY and cancellation values across later
player-control entries. A checkpoint rank advances with an actual new individual-turn dispatch;
another poll or another position in the same turn cannot reseal the parent checkpoint.

The original core still contains the live BattleLoop, individual-turn and player-control stack.
The observer saves their three exact `{kind,target,stack,pc}` return descriptors, checks the return
addresses against original RAM and re-registers the named callback behavior after native load.
Only those declared long calls may remain; ordinary `returned` closures must be closed. The load
still verifies the complete original RAM/register/frame snapshot before input and rechecks the
same checkpoint predicate. No live closure is discarded, no pending count is cleared to permit a
save, and no register/RAM/ROM/flag/actor/RNG repair is introduced.

The `controllable-5b` readiness predicate requires observed after-program and after-routine returns,
F401-clear→F501-set, actual BattleLoop return with D4=1, SwitchMap return and ExplorationLoop entry,
then two consecutive neutral completed frames at the same map/location/facing. Both require actual
`CURRENT_BATTLE=NOT_CURRENTLY_IN_BATTLE` (255), player entity control and recent field-action polls,
settled movement/camera, no program, modal,
transfer or live battle return. The final observation retains exact reached flags, map/player,
party/roster, inventory/stats/spells/EXP/gold and consumer facts. The Issue515 extension above retains
this pre-input boundary and terminates only after the ordinary input and settled displacement.
Its saved pair is terminal and
nonresumable. A runtime receipt remains unreviewed evidence until independent acceptance; it is
neither deterministic replay nor H4.

The original `ExplorationLoop` entry precedes its conditional write of the no-battle sentinel.
Preparation binds that write's sentinel and RAM operands against source/H1/ROM; both completed
frames must read it, and the runner rejects a contradictory terminal battle value with FAIL.
Main-gate's retained PR499 counterexample showed that return bookkeeping alone could accept battle1.
That completed failure remains in the local review results; the corrected direct sequence rejects
battle1 and other active indices, performs the original no-battle transition and only then accepts.

### Preparation, operator and verification

Load `local/private-inputs.ps1` in the launching PowerShell. Use the existing prepare API with
`interactive=True, continuation=VICTORY_CONTINUATION, segment=1`, a fresh ignored destination,
`proposed_timeout_seconds=7200` and the four reviewed accounting values above. Children specify
the next ordinal and `resume_directory` only; successful parents and failed-child costs are checked
by the existing pair protocol. Victory mode requires explicit segmented accounting. Mode identity
is included in parent compatibility. Native save/load are still outside callbacks.

The completed post-victory correction chain began with prepared-68 in a fresh ignored destination,
segment 1 without a parent, 96 historical starts and actual start 97, carrying the four totals
through prepared-67 under the existing 7200-second proposal. It ended at terminal prepared-86;
old preparations and source-incompatible pairs must not be reused.
Preparation reports `CANDIDATE-PREPARED-NOT-ADMITTED`. It does not authorize execution or count
as another actual start; independent source/preparation acceptance must precede native collection.

The local prepared operator uses the same finite stdin/stdout request/response mechanism as #485.
After main-gate admits the concrete preparation, `local/issue496/operator-prefix.py
<prepared-name> <initial|gate|royal|guard|battle>` reuses the existing finite prefix operator to drive
a fresh compatible chain to its first battle save. The retained local entry point
`uv run python -X utf8 local/issue496/operator-victory.py <prepared-child-name>` drives a child
from that movement checkpoint to the next coherent checkpoint or a terminal observation. It uses
current original grid/positions, menu choices and targets, and only acknowledges active dialogue
consumers. Its `invoke-reviewed.py` wrapper checks clean accepted execution sources and parent
accounting before invoking `run_map3_observation_candidate` with the same explicit selection.
Prepared-06..32 retain their executed operator decisions and prior operator versions. Current
offline tactics consider healing resources before retreat, allow each herb holder to heal an
original allowed target, and distinguish the item-action and item-icon menus. Threatened injured
allies precede already sheltered allies; a threatened Bowie's identity breaks comparable choices,
rather than overriding every more urgent wound. The operator evaluates the original grid,
permits transit through allies while rejecting occupied final positions as the original caller
does, prefers weakened targets and limits unnecessary separation. Enemy move-plus-one distance,
cohesion and injury thresholds are tactical estimates, not exact enemy reach or gameplay legality.
`operator-victory-before-herb.py` retains the pre-extension policy. Readback proposals are not
replayed outcomes; herb use, a winning battle route and bounded stable field readiness are observed
above. The Issue515 result additionally records post-endpoint ordinary input. The sequential
`continue-rounds.py` performs preparation, one finite round operator, complete pair audit and then
the next preparation; an incomplete result stops the chain. No script acts inside a callback.

Direct reproduction uses `local/issue496/bind.py`, `check-herb.py <fresh-output>`,
`check-host.py <fresh-output>` and `prepare-herb.py <fresh-output>` with `uv run python -X utf8`.
The Lua driver executes the actual observer blocks with synthetic memory/core APIs; its save/load
results prove observer continuation logic only, never native savestate compatibility. It covers
residual actions, repeated turns/polls, menu/target isolation, unclosed consumers, corrupted stack,
saved callback reconstruction/forward save, RNG, defeat, incomplete after-program/flag order and
the complete two-frame endpoint. The item cases additionally exercise two diamond contexts,
selection/slot mismatch, cancellation returns, consumed polls, pending/save rejection, effect and
inventory-removal callback closure, and unsupported subcommands/items. Existing #485 first-ready,
stabilization and host/pair direct
checks also exercise the unchanged mode. Ruff/Lua syntax and normal `uv run sf2 verify` are selected;
the committed planner and actual CI results belong in the PR handoff. No new helper tests, broad
legacy H3 queue, full suite or native launch is selected by this bounded offline change.

`herb-offline-failures.json` retains corrected local Ruff, source-discovery and result-projection
failures separately from game outcomes. `herb-operator-readback-review.json` contains the prospective
decisions against retained native states plus the newly bound static herb metadata; it is not
evidence that those proposed movements or item actions executed.

## Bounded interactive acquisition

The current new-method boundary is [Issue #485's segmented acquisition](#savestate-linked-segments-issue-485).
The exhausted #475 allowance and historical restrictions below remain scoped to their original attempts.

Issue [473](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/473) implements an explicit
interactive acquisition mode for the existing candidate. That implementation slice was **offline
preparation only, zero emulator launches**. After PR #474 was independently accepted and merged,
[Issue #475](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/475) separately admitted
and consumed exactly one acquisition. Its [result](#single-admitted-interactive-acquisition-result)
reached the Map19 terminal and was independently accepted by main-gate in
[PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476); no execution allowance remains. [ADR 0015](../decisions/0015-original-reference-replay-and-h4-boundary.md#distinguish-interactive-acquisition-frozen-replay-and-remake-h4)
separates this operation from frozen replay and remake H4. The
[bridge protocol](../operations/bizhawk-debug-bridge.md#map3-interactive-composition-offline-only)
owns communication, serial requests and the single process. This observer retains source-bound R1
admission, checkpoints, typed failures and final restoration. No new CLI, debugger service, fixture,
schema, planner, route policy or verification-helper test is introduced.

**Confirmed (source/implementation boundary):** preparation checks the pinned SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, H1 listing/ROM callback bytes, retained R1/R2/RA
contracts and installed tool identities. The explicit mode uses an empty input declaration; it does
not load, convert or execute #471's frozen house-neutral input. The old default API retains its
frozen frame-table semantics. New mode preparation binds the shared bridge Python/Lua/JSON sources
in addition to existing candidate execution identities. Preparation grants no execution permission.

The readback and terminal stay as described below: controlled NewGame/SaveGame/default Map3 state,
allies (including inherited POISON), full live entity/index state, RNG/raw time and service/scratch
restoration at the first R1 `WaitForEvent`. Nothing normalizes those inherited values after that seam.
The first original Map19 movement acceptance must follow the real gate program, original F604 trap,
north warp, Map19 init/program returns, original wait/controller installation and closed pending
consumers. The terminal callback is retained once; remaining batch frames are skipped. The current
frame may finish before final restoration, and the reply distinguishes those two observation times.

### Consumer observations for explicit operator decisions

Existing `DisplayText`, `CloseDialogueWindow`, script and prompt entry/return events remain source
observations. Interactive frame-end snapshots additionally report pending returns, active consumer
counts, and the most recent source consumer poll with its observer/emulator frame and PC:

| Poll | Pinned source owner and meaning |
| --- | --- |
| `text-wait1` | `code/common/scripting/text/textfunctions_1.asm`, `loc_65B4`: current-input read after `WaitForVInt` in `symbol_wait1` |
| `text-wait2-loop` | Same source, `loc_6472`: wait2 loop before animation/VInt/input processing; not the later acknowledgement read itself |
| `WaitForEvent-action` | `code/gameflow/exploration/explorationfunctions_2.asm`, `loc_2593C`: field loop A/C dispatch; another C may invoke an entity/area action or FieldMenu |
| `YesNoPrompt-release` | `code/common/menus/yesnoprompt.asm`, `loc_1530C`: wait for release of player-one input |
| `YesNoPrompt-choice` | Same source, `loc_15314`: choice loop; B selects No, A/C accepts the current choice |

These named symbols are bound to H1 and canonical ROM bytes during preparation. A last poll can be
stale; its timestamp and active consumers must be read together. `CURRENTLY_TYPEWRITING=0`, a script
return or a past poll alone is **not** readiness proof. Lua never converts these facts into a button
decision. FieldMenu remains a typed stop, not a condition to repair with injected state. The #475
result below records the reached native boundary. Other availability, timing and downstream reach
remain **Unknown**.

### Preparation and operator protocol

The following describes capability use, not remaining launch permission. The sole reviewed #475
material was retained `local/issue473/prepared-02`; `prepared-01` is stale and forbidden for execution.
Do not reprepare or invoke either after the consumed attempt.

Load the current worktree's `local/private-inputs.ps1` in the launching shell. Use installed `uv`
dependencies and shared Lua. The existing API prepares into a fresh ignored directory without a
runtime copy or process:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import UPSTREAM, prepare_map3_observation_candidate
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path

report = prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    output_directory=Path("local/issue473/prepared-01"),
    proposed_timeout_seconds=1800, interactive=True,
)
```

Outputs are `candidate.json`, `config.json`, syntax-checked `config.lua` and `input.json`. The last
file identifies the explicit mode/clock and contains zero preselected frames; its digest is **not an
actual input recording**. The report retains `CANDIDATE-PREPARED-NOT-ADMITTED`, zero launches, the
1800-second wall bound, 28634-frame total, 2048 batches and 120-frame maximum batch. It records
historical controlled starts 2 and future ordinal 3 without claiming that ordinal was executed.

After independent merge and separate main-gate admission, #475 invoked this same runner with the
explicit matching mode. This records the consumed invocation; **do not repeat it**:

```powershell
uv run python -c "from pathlib import Path; from sf2tool.h3.map3_messenger_acceptance import run_map3_observation_candidate; from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path; run_map3_observation_candidate(private_input_path(ROM_INPUT_IDENTITY), Path('local/issue473/prepared-02'), interactive=True)"
```

After the admitted R1 frame finishes, hello prints its paused state. Enter one JSON array per line:
`["state"]`, `["step", 1, "C"]`, `["step", 1, "neutral"]`, or `["abort"]`. Each button is held for
the requested 1–120 frames; neutral requires its own explicit step to advance a release frame. Paused
queries consume no original frames. The operator chooses from original observations; no fixed pulse
schedule or automatic B replacement is supplied. Inspect the private callback records as needed.

### Actual input, clocks, order, and failure boundary

Each `runtime/actual-inputs.jsonl` object has a shared monotonic `order`, observer `frame`,
`emulatorFrame`, `r1Epoch` and `r1EmulatorEpoch` (false before admission). Checkpoints share this order;
their `boundary` distinguishes `callback-time` from `host-loop` diagnostics. Input log kinds are:

| Kind | Meaning |
| --- | --- |
| `command` | Validly framed command ID and fields received; not proof of application |
| `applying` | Button submitted, `beforeFrame`, next `inputFrame`, command ID (0 for bootstrap); a crash here does not prove completion |
| `frame` | `emu.frameadvance` returned; actual button, `beforeFrame`, `afterFrame`, R1-relative `inputFrame`, command ID and bootstrap marker |
| `result` | Command outcome, actually advanced count, source snapshot labelled `frame-end`, terminal callback separately and error if any |

Record every completed frame, including neutral and the declared automatic Start/neutral bootstrap.
The R1 admission callback may occur inside a bootstrap frame; retain both its callback emulator clock
and that frame's completion instead of relabelling it an operator frame. JSONL gaps in `order` are
expected because `checkpoints.jsonl` shares the sequence. No requested batch substitutes for completed
`frame` records. Physical input-consumption evidence remains the original controller/consumer callbacks.

The first error or terminal ends remaining advances. Unsupported buttons, wrong arity, malformed
counts, budget overflow, disconnect and abort fail closed. At 28634 total frames without terminal the
session fails; at most 2048 accepted input batches are allowed. The 1800-second wall limit starts at
the owned process launch and includes startup and all paused time. Host containment, short exchange
timeouts, callback exceptions, restoration and final session-ROM deletion keep their distinct statuses.
EOF graceful cleanup remains **Unknown** at the native boundary; a killed process is not proof of Lua
restoration. The #475 result below includes independent owned-process and canonical-identity checks. An acquisition completion remains `OBSERVATION-COMPLETE-UNREVIEWED`, not replay/H4 PASS.

### Issue #473 offline verification and retained limitations

Use Python AST/compile, targeted Ruff, the shared Lua syntax compiler and bounded direct protocol,
input-log/budget/error-path checks. API stubs can check delivery ordering and skipped batch remainders;
they cannot establish actual emulator input, core behavior, natural reach or native EOF cleanup.
Record the clean committed planner and actual public CI, but shared-path fanout does not authorize
local normal/full/H1/H2/H3 runtime queues for this slice. No bridge smoke or warmup is permitted.

Preserve #460's raw-MAP_CURRENT failure, #465's FieldMenu failure, #469's source/trace audit, #471's
pre-API CRLF assertion failure/correction and its unexecuted fourteen-neutral replacement candidate.
At #473 preparation the controlled historical count was **2**. The separately admitted #475
acquisition below consumed the one additional start, making the total **3**. There is no remaining
call, reset, extra smoke/replay/retry or fourth start. Disabled original replay ordinals 1/2 and
their restrictions remain separate. Arrival timing, NPC/RNG effects, prompt/gate/natural continuity,
complete 8D and H4 stay **Unknown** until their own required evidence and independent acceptance.

## Natural Battle01 continuation capability (offline only)

[Issue #483](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/483) adds the explicit
`continuation="natural-battle01-player-ready"` selection to the existing prepare/run APIs, with
`interactive=True`. The [accepted route and admission proposal](map3-battle01-audit.md#map19-continuation-admission-proposal-issue-479-not-authorized)
owns its scope and proposed limits. **Confirmed (offline implementation checks only):** this
selection serializes an empty interactive declaration, keeps first Map19 control as an intermediate
checkpoint and observes the selected natural castle/tower callers. Preparation supplies no saved
Map19 core, generated inputs, R2d injection, actor/order assertion or launch permission. Omitting the
selection retains the previous frozen/Map19 behavior and limits; the R2d bridge remains separate.

The candidate reuses R2d's `_static_contract` read-only RAM/admission fields and Lua
`extension_combatant`, without installing `config.extension`. Additional source files are compared
with pinned Git objects; callback instructions, selected warp operands and sound-dispatch write sites
are bound to the existing H1 listing and canonical ROM. Setup selections are observed on every init
call. Stack-matched returns distinguish repeated init/program/consumer invocations. Caller records
join F605, F607/F608 and F401/F256 to their real program returns. Astral's decline stops at the reached
F89 branch; it never inherits the messenger's fixed default prompt-result assertion.

`loc_47156` records the reached operation PC/opcode/operands and enclosing program; `loc_47140`
records its returned blocking work. Reached awaited entity operations additionally read back
`eas_Idle`. Entity lookup records retain selector/index bytes and actual returned A5, including alias135;
non-waited actions remain live. Text/prompt consumers retain the existing acknowledgement and return
observations. Sound trap requests and original 68K mailbox writes are separate observations, including
raw music/control replacement commands. Neither a script return nor a mailbox write establishes all
presentation completion, audible output, fade/resume behavior or 7C asset provenance.

Accounting checkpoints read all ally records, joined/active flags, full item words/spells, 32-bit gold,
actual party/placed combatants, RNG bytes/copy byte/raw time and actual turn order/offset. They occur at
R1, first Map19 control, actual next-tile displacement, royal return, Astral/guard caller return,
before/after load and generation, and ready. The selected Map21 `(5,15)`/Down checkpoint requires a
neutral completed frame, settled raw position/destination, no pending event/consumer and a reached
field-action poll. The operator must acquire it before taking the tower exit; Lua supplies no route
or input policy.

The source-progress clock resets on reached operation/consumer returns, accepted movement/text
acknowledgements and actual position, relevant entity/camera/fade or prompt-choice changes. Repeated
unchanged polling, held input without an accepted read and `state`/`ping` do not renew it.

The first `0x22E70` sample requires returned before/start/load/generation work, Map57/Battle1/area,
F401/F501/F451, matching natural first ally/turn/moving actor and **mapped entity** view target,
neutral input, zero full event word, no action/targeting/modal or pending blocking consumer, and
settled camera/fade state. Region flags and carried state are observations, never natural goldens.
The callback sample and completed frame are stored separately. After a stop, remaining callbacks in
that same frame still record PC/role/state; semantic collection ends at the first stop and the rest
of the requested batch is skipped. This cannot suspend an instruction or prevent original code from
executing during the remainder of that frame.

`player-ready` yields `OBSERVATION-COMPLETE-UNREVIEWED`. First non-player dispatch or action entry
produces `OUT-OF-SCOPE-BEFORE-PLAYER-READY`, a coverage stop. Decline, premature outcome and budget
stops produce `INCOMPLETE-OBSERVATION`; malformed input, abort, transport and callback/readback errors
retain their failure role/reason and cleanup record. Cleanup failure defeats any successful endpoint.
No retry or forced ally follows an AI-first result. These statuses establish neither replay nor H4.

### Offline preparation and verification boundary

Load the current worktree's ignored `local/private-inputs.ps1` in the launching PowerShell process.
The narrow ROM check is `uv run sf2 rom verify`. Direct syntax/lint checks use
`uv run ruff check src/sf2tool/h3/map3_messenger_acceptance.py src/sf2tool/bizhawk_debug_bridge.py`,
`ast.parse(Path(name).read_text(encoding="utf-8"))` for both Python files, and
`validate_lua_syntax(Path("tools/bizhawk/map3_messenger_acceptance_observer.lua"), bizhawk_contract()[1])`
from `sf2tool.h3.bizhawk`, executed with `uv run python -X utf8`. The last command only loads the
installed Lua library to compile the chunk; it does not start BizHawk.

Prepare only, choosing a **fresh** ignored destination each time:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import (
    NATURAL_CONTINUATION, UPSTREAM, prepare_map3_observation_candidate,
)
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
report = prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    output_directory=Path("local/issue483/your-new-preparation"),
    proposed_timeout_seconds=7200, interactive=True, continuation=NATURAL_CONTINUATION,
)
```

The candidate report carries exact ROM/source/H1/tool, observer/runner/helper, configuration and
input-declaration identities. It declares historical starts **3**, proposed future ordinal **4** and
**zero** authorized additional starts. A future independently admitted invocation must explicitly
select the same continuation in `run_map3_observation_candidate`; no run API was called in #483.

Direct offline checks exercise the actual Lua operation/return/dispatch and frame-delivery blocks
with synthetic memory/clocks, plus host framing, idle containment and selection rejection. They cover
full-word event rejection, pending consumers/modal/input/actor/view mismatches, waited entity return,
actual lookup-target readback, same-frame AI coverage stop, prompt decline, repeated stack returns,
source progress and independent wall/stage/idle/frame/batch limits. These are implementation checks,
not original observations or new helper-test suites. Initial lint/source-binding and offline driver
environment failures are retained under the Issue's ignored outputs; corrected direct checks use new
output directories. Earlier offline source projections are stale, not prepared or resumable candidates.
The final Issue/PR handoff identifies the frozen prepared material and actual public CI/planner result.

**Unknown:** native compatibility and real route feasibility, natural actor/order, failure-path Lua
restoration under abrupt EOF, natural battle/victory/5B, remaining 8D, 7C audio and H4. The two controlled
FAILs, accepted bounded #475 observation, old unused candidates and disabled replay's missing receipts,
timeout and cleanup failure remain intact. No original/native start, smoke/replay/H1/H2/H3 or broad
suite was run for this capability. Independent capability review and concrete main-gate lineage
admission remain necessary before any fourth controlled start. Issue #485 below supplies the user's
newer savestate-method authorization.

## Savestate-linked segments (Issue #485)

The user's [stabilization instruction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752405430)
supersedes the former four-start envelope, per-invocation approval stops, exhausted-parent policy and
cumulative resource stop ceilings for this work. Continue correction, independent integration and
actual acquisition until stable. Historical failures and consumption remain evidence; preparation
does not erase them. Main-gate retains independent review and serialized integration. The current
acceptance is an early safe native save, load with original-core and observer checks before input,
then forward movement to a later safe save. That bounded chain is now
[Confirmed in native execution](#accepted-early-native-save-and-resume); later route collection continues.

The prepare/run APIs use `interactive=True, continuation=NATURAL_CONTINUATION, segment=1..4`.
A fresh segment 1 requires explicit reviewed starts, active seconds, delivered frames and advancing
batches. Initial failed-attempt totals were **6 / 2080.6977567999857 / 13561 / 225**. Seconds must be
finite and nonnegative; counts must be nonnegative integers, excluding booleans. Resumes take
`resume_directory` and inherit accounting from the complete parent plus completed failed child
attempts. Overrides are rejected. Logical observer/emulator frames and R1 epochs resume from the
parent; cumulative resource frames include failures without inventing game continuity.

| Safe save boundary | Original closure and forward checkpoint |
| --- | --- |
| Segment 1, house | Existing `map3-house-exit-zone` waypoint `(4,4)`, F601, closed field in `candidate-route`. |
| Segment 1, Sarah | Existing `(42,9)` waypoint after F256 and closed field. |
| Segment 1, Astral approach | Existing entity142 waypoint `(55,17)`, F256 and before F602; before interaction. |
| Segment 1, messenger | Existing `(43,10)` waypoint, F603 and observed R2a return in `candidate-gate-route`. |
| Segment 1, Map19 | Original gate/F604/warp/init closure and first accepted controller movement, then observed `(26,29)` displacement. |
| Segment 2, royal return | Map20 `(23,39)`/Down after `cs_53996` and init return/F605, before Astral. |
| Segment 3, guard return | Map21 `(5,15)`/Down, F401/F256, closed guard script/caller and four natural warps. |
| Segment 4, player-ready | Existing `0x22E70` callback and its neutral completed frame; final evidence only, never resumable. |

Same-ordinal segment-1 continuation is allowed only from an earlier named checkpoint. Saving again
requires a strictly greater checkpoint rank and a later actual observer frame. Parent loading checks
the parent's exact checkpoint predicate without pretending it has advanced. No duplicate parent
save, relabelled prefix, replay or manual game-state reconstruction counts as a forward segment.

For resumable saves, the observer requires a paused neutral completed frame, zero pending returns,
closed consumers, an empty program stack, no audio dispatch, no event/text/window/fade, a settled
player and a fresh field-action poll. `saveReadiness` exposes the same predicate to state, save and
load, including raw camera current/destination values and original effective scrolling bits. A normal
not-ready `save` returns `ok=true`, `terminal=false`, `advanced=0`, `save.status="not-ready"` and
reasons; it writes nothing, performs no restoration and renews no idle interval. Identity, core,
callback, malformed-request and I/O failures retain fault containment.

**Confirmed (pinned source/H1/ROM):** `WaitForViewScrollEnd` (`0x4708`) uses
`IsMapScrollingToViewTarget` (`0x4728`), which masks `VIEW_SCROLLING_PLANES_BITFIELD` according to
packed layer autoscroll X/Y words. `LoadMap` writes loaded current A/B coordinates without writing
the corresponding destinations (`mapload.asm`, current writes at `0x2CB4` onward). The same fixed-point
units apply to current/destination; pixel projection shifts by four. Equality is therefore not a
necessary stationary-view predicate: destinations may belong to the old map origin. Save and natural
player-ready use the original effective scrolling predicate with the other closure checks. This is
not a new runtime proof of Map19 readiness; the earlier raw camera discrepancy remains unmeasured.

**Confirmed (implementation/source boundary):** entity C consumes the existing route waypoint's
map/tile/facing/target plus live entity mapping, target position/destination/facing and fresh closed
field state before accepting the step. Later Astral/guard interactions consume the castle route owner.
Wrong facing, missing/moved target or stale field poll returns typed `input.status="not-ready"` with
zero frames and no batch charge. Dialogue C requires an active DisplayText consumer with a fresh
wait poll; prompt C requires an active choice poll after release. Post-join `WaitForPlayerInput` is
tracked as one active return consumer across its polling loop. A stale poll alone never admits C.

FieldMenu recovery follows the original source path: entry → top-level ExecuteDiamondMenu returns
`-1` after B → FieldMenu returns → a fresh WaitForEvent action poll. `mainactions.asm`,
`diamondmenu.asm` and `explorationvints.asm` own cancellation and restoration of field updates.
Only neutral/B may advance during recovery; save and ordinary actions remain unavailable. Entry or
return during a multi-frame command truncates its remaining frames and replies with actual advancement.
A noncancel choice/submenu remains explicitly unsupported and cannot falsely mark field restoration.
No RAM/register repair is performed. The generic bridge library/protocol is unchanged.

Native `savestate.save/load` execute outside callbacks. The observer verifies actual nonempty save
output and version/system/core identity. On load it compares all exposed registers, complete 68K RAM
and emulator frame before input, restores route phase, gates, completed facts, consumer/program state,
progress, epochs and record order, and registers needed future messenger callbacks for early parents.
Closed return stacks allow dynamic return closures to be rebuilt rather than serialized. Bootstrap
patches must be restored; cleanup restores the initial bootstrap scope or the loaded entry snapshot.

The immutable pair binds `segment.State`, `continuation.json`, input/checkpoint/observation/status and
host/bridge receipts to prepared material. Publication follows successful exit and callback cleanup,
session deletion, canonical identity and frame/batch/order reconciliation. An exclusive `resumed-by.json`
claim records the first child; additional numbered claims retain subsequent attempts without replacing
earlier branches. The [completed-branch recovery](#native-herb-observation-and-completed-branch-recovery)
now reconciles the entire source lineage before reuse of any compatible complete parent, including
successful descendants and sibling/cousin costs. Missing/incomplete attempt receipts block resume.
An explicitly recorded, cleanup-verified pre-process failure may retry without adding a native start,
frame or batch only when both owners prove no process/input; missing logs alone never prove zero.
Any measured failed launch-attempt duration remains charged in cumulative seconds, even without a
native start. Failures before the launch timer retain their unmeasured duration in the original receipt.
No claim, failed branch, parent or evidence file is overwritten. Observer/runner/source/tool identities
must still match the parent; a source correction can make an old parent incompatible and does not
authorize bypassing that check.

The former 7200-second, 36000-frame, 600-batch, stage and source-progress ceilings are observations
for stabilization, not termination conditions. Actual cumulative costs and stalled source progress
remain visible. Keep 1–120 frames per explicit step, 60-second startup/exchange, 120-second disconnected
or idle-operator containment and 3+3-second teardown. Inspection and zero-frame rejected steps do not
renew idle time. Offline gaps advance no game frames. Ordinary standalone bridge limits are unchanged.

Prepare after source freeze in a fresh ignored destination; initial arguments currently include
`reviewed_prior_starts=6`, `reviewed_prior_active_seconds=2080.6977567999857`,
`reviewed_prior_delivered_frames=13561`, `reviewed_prior_advancing_batches=225`,
`proposed_timeout_seconds=7200` (the retained observational configuration value). Those initial values
belong to the completed first segment; continue from the latest complete parent and inherit its actual
totals below. Native outcomes
remain `SEGMENT-SAVED-UNREVIEWED` or `OBSERVATION-COMPLETE-UNREVIEWED` until independent acceptance.

Direct source/ROM preparation, actual-Lua branch checks and host/pair checks cover this implementation.
No helper tests or unrelated long runtime queue is added. **Confirmed:** the bounded early native
save/load chain through the resumed messenger and Map19 checkpoints below, and the
[final compatible chain's first Battle01 player-ready endpoint](#accepted-first-battle01-player-ready-acquisition)
with actual actor 2. **Unknown:** native FieldMenu cancellation, unobserved route timing/actors,
downstream victory/5B and remaining 8D/7C/H4. Idle-failure cleanup remains attempt-specific; attempt
21's final restoration and callback removal are **Unknown**.
These are savestate-linked original observations, not uninterrupted wall-time execution, replay or H4.

### Accepted early native save and resume

**Confirmed:** [independent main-gate acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752645799)
validated an actual house save → native load before input → Sarah forward save on accepted
`22728bbf563b2b251cf870073675b5a33bb504e0`, tree `a34247fde3a61871efd30f3efc950d18c6b281a9`
([PR #489](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/489)). The ROM/source baseline above,
BizHawk 2.11.1 Genplus-gx, runner `10D6483121BABEE8D4460722504D10A1FB17BEF077B8468196E5C04945C84C67`
and observer `AF154C1C272FD8418A2996099B7B8A4616D1AB30FEF73ABE49DC722DFDB01090` were unchanged across both processes.

| Native segment | Closed checkpoint | Actual result |
| --- | --- | --- |
| `prepared-09`, historical start 7 / PID 1624 | House `(4,4)`, F601, rank 1, observer/emulator 1030 | 1030 frames including initial bootstrap; 11 advancing batches; 140.23876470001414 seconds; native save and complete pair. |
| `prepared-10`, historical start 8 / PID 15492 | Sarah `(42,9)`/Up, F601/F256, rank 2, observer/emulator 2080 | Loaded parent at 1030 before input, then 1050 new frames and 26 advancing batches; 199.03445660002762 seconds; later native save and complete pair. |

Both exited 0 without timeout/forced termination; callback removal, original-state restoration,
session deletion and canonical identity passed. The child restores the loaded-entry core snapshot;
individual bootstrap restoration booleans are inapplicable on this resume. The live Sarah entity C
passed waypoint/facing/target/field readiness, followed by active text-consumer acknowledgements.

The child's first event is `segment:loaded-before-input`, frame 1030/order 2333, after full exposed
register, complete 68K RAM and emulator-frame equality checks. It inherits R1 epoch 355 without
repeating bootstrap. Parent/child records have contiguous unique orders 1–2332 / 2333–4812 and
logical frames 1–1030 / 1031–2080. The parent claim, material/runtime-setting identities and strictly
forward checkpoint match; save readiness was inspected before each save, not after terminal cleanup.

Parent pair/state SHA-256:
`5BBB918BA76B45C2D3F1327BE156DA9B8D55D5E7ABCE1F4D2FC1AAC4086B12CF` /
`B7009BEEF34006ACCF5B54EB91C1E8AF39BC7D2538FE91B0632CF95CD6E2E2BD`.
Sarah pair/state SHA-256:
`5B41893058ABAFDE38F54070F2EFD751FF33B105A0E258DA2CEA6E13F526A03D` /
`AC52CD193B71DAA6578BE44B3DA13418EFEC67A297F2B70BA5118ABBA724BFE7`.

Reproduce validation without a native rerun by loading private-input configuration and using the
tracked `_read_segment(Path(...).resolve())` for each retained candidate directory. Reconcile their
bound completed-frame and shared-order logs as above, compare the child's declared parent pair,
and inspect its load-before-input event and pre-save bridge snapshot. The original acquisition used
`prepare_map3_observation_candidate` / `run_map3_observation_candidate` with the natural interactive
selection, ordinal 1 for both and `resume_directory` naming the house parent for the child. Actual
controller commands are retained in the bound input receipts; this is not a reusable frozen replay.
The [execution handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752640589)
locates the private receipts and direct reconciliation command; no state or game-content artifact is public.

At the Sarah checkpoint, cumulative accounting was **8 actual starts / 2419.9709781000274 charged
seconds / 15641 resource frames / 262 advancing batches**, including all earlier failures. Its successful
chain had 2080 logical frames; the forward continuation below supersedes Sarah as the latest parent.

### Resumed messenger and Map19 checkpoints

**Confirmed:** the same frozen implementation subsequently saved the Astral approach and resumed
through the messenger. Independent reviews accepted the [Astral checkpoint](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752705908)
and [messenger checkpoint](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752796810).
The [Map19 child](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752816999)
was independently accepted after native save, pair validation and raw receipt reconciliation.
Runner/observer identities remain those above. Each child loads complete original RAM/register/frame
and observer state before input, preserves epoch 355, and advances to a higher checkpoint rank.

| Candidate / actual start | Completed boundary | New charged consumption / successful record orders |
| --- | --- | --- |
| `prepared-11` / 9, PID 38348 | Parent Sarah frame 2080 → rank 3, Map3 `(55,17)`/Left, before F602, frame 2656 | 576 frames / 14 batches / 121.39703140000347 seconds; orders 4813–6141. |
| `prepared-13` / 11, PID 40308 | Parent 11 frame 2656 → rank 4, Map3 `(43,10)`/Down, F602/F603/F600/F66/F89, frame 8409 | 5753 frames / 86 batches / 535.700546099979 seconds; orders 6142–19759. |
| `prepared-14` / 12, PID 17196 | Parent 13 frame 8409 → rank 5, Map19 `(26,29)`/Up after F604, frame 10030 | 1621 frames / 33 batches / 128.33305830002064 seconds; orders 19760–23675. |

At frame 2651 the live readiness result identified Down as the wrong player facing for entity 142.
No C was sent then: original Left1 + neutral4 established the required Left facing and ready state at
2656. This is pre-input detection and correction, not an actual zero-frame C rejection. The resumed
messenger uses actual text/prompt polls and the original default-zero Yes answer. After join text 447,
`PlayMusicAfterCurrentOne` returns before the original `WaitForPlayerInput` poll permits C. Its return,
follow commands, F603 and closed field control precede the rank 4 save. No FieldMenu occurred.
The next child traverses the original six-text gate program, F604, north warp, Map19 initialization
and first accepted movement before rank 5. F604 changes at frame 9781/orders 23078–23079; the warp
handler at 9902/order 23355 targets Map19. First movement at 10014/order 23627 still observes `(26,30)`;
the next-tile marker at 10015/order 23634 observes `(26,29)`. Save is later at 10030. These callback
and completed-frame boundaries remain distinct. Pre-save snapshots show empty consumers/programs/returns,
neutral input, fresh field polls and `saveReady=true`. All three saved processes exited 0 without
forced termination or timeout; loaded-entry restoration, callback removal, session deletion and
canonical identity passed. Map19 camera readiness uses the source effective-scroll predicate; raw
coordinate equality is not required.

The failed sibling `prepared-12` is preserved separately: actual start 10 / PID 41588 loaded parent 11,
reached F602 without FieldMenu, and stopped at frame 3585, Map3 `(58,13)`, text-wait1. Context recovery
exceeded the existing operator-idle deadline. Exit 1 / timeout true / forced termination false charged
929 frames / 16 batches / 219.33223120000912 seconds, orders 6142–8422. Independent
[failure-accounting review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752766124)
confirmed loaded-entry restoration, cleared callbacks, removed partial observation, deleted session ROM,
unchanged canonical input and unchanged parent. No child state/pair exists. This is an operator-context
failure, not a route or facing defect. Its overlapping branch orders are not successful continuation;
all actual costs remain charged in the retry. Compact live readbacks and saving at supported checkpoints
before context recovery improve operation without changing watchdogs or generating keepalive input.

Pair / native-state SHA-256:

- Astral: `A552026FE5C59998820EF1758E9552CD3BBBB7FB92C204A61B5391BE6C5EEE1B` /
  `B1896926C9B6382E2977A726648155DADC2492BB1006C92AB15919E7866BB0E8`.
- Messenger: `958C672CEE6F1B3BC02BECDF6B5D8F4329DC1459B2CF604E2646F9C72292D66A` /
  `8AF7B04CDEE26FF81E7909B14CB0C3EE026BDE60C13C1C054FA4236E0FC35BAA`.
- Map19: `E4EC12E4D687D9FFDFB85D7AF9AD596B6D5F7751145E71C5065A644C65207FD9` /
  `B00B8AD2A41D055DBD9282719E84C26F9E86A7F42C2635A5AEF810D08A6D673B`.

Use the same `_read_segment` validation and bound-receipt reconciliation described above for candidates
11, 13 and 14. For failed 12 inspect completed host/bridge/input/checkpoint receipts; `_resume_accounting`
on absolute parent 11 reconciled its failure before retry preparation. Each successful child used natural
interactive ordinal 1 and its immediately preceding complete parent. No private artifacts are public.
At Map19, cumulative accounting was **12 actual starts / 3424.7338451000396 charged seconds / 24520
resource frames / 411 advancing batches**, with 10030 logical frames on the successful branch. The
royal/guard continuation and final-segment failure below supersede this continuation point.

### Royal and guard saves; BattleLoop return-stack failure

**Confirmed:** independent reviews accepted the [royal segment](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752906075)
and [guard segment](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752930085)
on the unchanged PR #489 execution sources. The subsequent source correction makes these historical
pairs incompatible with the corrected observer; they remain preserved evidence.

| Candidate / actual start | Native load and saved boundary | New charged consumption / record orders |
| --- | --- | --- |
| `prepared-15` / 13, PID 40052, ordinal 2 | Load Map19 parent 14 at 10030 → rank 6 Map20 `(23,39)`/Down at 14970, F605 | 4940 frames / 79 batches / 445.02149070001906 seconds; orders 23676–35469. |
| `prepared-16` / 14, PID 23532, ordinal 3 | Load royal parent 15 at 14970 → rank 7 Map21 `(5,15)`/Down at 17004, F401/F256/F607/F608 | 2034 frames / 47 batches / 213.08364640001673 seconds; orders 35470–40531. |

The royal original init return is observed at frame 14853; the completed snapshot at 14912 has F605
and closed consumers at `(23,39)` but faces Left, so save readiness correctly remains false. Ordinary
Down/Up/neutral movement subsequently establishes the declared Down-facing checkpoint at 14970.
The guard caller is closed in snapshot 16943 at `(4,16)`/Down; ordinary Right/Up/Down/neutral movement
then reaches `(5,15)`/Down at 17004. Neither original return is the later save. Both processes exit 0
without timeout/forced kill; pair/source/core identity, loaded-entry restoration, callback removal,
canonical input and session-ROM cleanup pass. No repeated bootstrap or FieldMenu occurs. The reviewer
initially confused royal Down value 3 with Up value 1; its failed checker is retained and corrected
against actual state/input without a native rerun.

Pair / state SHA-256:

- Royal: `5895CA0C4CE0520252E7C3A2B58AC42816AB079551D9BD3D1506A2629E7BD256` /
  `1C14E95F23FC9ED405763EC493CBCE928960F5F8CF586B63BEEF37752879E382`.
- Guard: `D0F362C791136D8ECDDAFB1639DBC10C3A5FA38EB8E8A96D7D7E629D6A68C6B3` /
  `26205BCD2E6956CF2FF05498546A8A560F9CF41C47835C92469BA48C8E198344`.

**Confirmed (failed observation):** `prepared-17`, ordinal 4 / actual start 15 / PID 38404, loads guard
parent 16 before input at 17004 and reaches the Map40 wildcard warp at 17653. Its incoming/effective
destination is Map57 while raw `CURRENT_MAP` is still Map40; the convenience snapshot map field must
not conflate these. `CheckBattle` at `0x799C` records stack `0xFFFFFC`, return PC `0x75D0`, then
`BattleLoop` at `0x23A84` fails with `premature battle loop`: one `battle:check` return remains pending
and no admitted checkpoint exists. Failure checkpoint is order 42061; the full receipt ends at 42063.
The last 48-frame Up request delivers only 37 frames; remaining queued input is not delivered. New
consumption is 649 frames / 18 batches / 60.111493600008544 seconds. Exit 1 has no timeout/forced kill;
completed failure receipts retain loaded-entry restoration, cleared callbacks, removed partial
observation, session deletion, canonical identity and unchanged parent 16. No final pair is published.

**Confirmed (source and direct Lua):** pinned `code/common/maps/getbattle.asm` ends `CheckBattle`
with balanced `movem` and `rts`; `code/gameflow/mainloop.asm` calls it, tests D7 at `0x75D0`, moves D7
to D1 and calls `BattleLoop`. This is an ordinary return, not a tail transfer. The existing observer
`returned()` helper masks actual A7 to 24 bits but formerly compared it with unmasked `stack + 4`.
For stack `0xFFFFFC`, expected `0x1000000` cannot equal masked live value 0. The one-line correction
applies `0xFFFFFF` to the expected address too, retaining return-PC, stack, single-use closure and
pending-consumer checks. It changes no original state.

Direct execution of the extracted actual Lua helper and battle-admission callbacks reproduces the
same failure before correction; ordinary return and missing-admission rejection controls pass.
After correction all six cases pass: wrapped CheckBattle, bus-zero return, ordinary return,
wrong-stack exclusion, nested same-return-PC ordering/single-use and absent-admission rejection.
The existing 17 actual-Lua stabilization checks also pass. Reproduce with the actual helper/callback
bodies, recorded stack/PC, D7=1 and both full `0x1000000` and masked-zero returned A7. The retained
`check-return-stack.py return-stack-before before` result is a historical execution of the observer
from accepted `06b9dad9f0cfa94866efb1b555d731b14704fe29`, before the edit. The driver reads the current
observer; its `before` argument does not select an old version. On corrected HEAD, run
`uv run python -X utf8 local/issue485/check-return-stack.py <fresh-ignored-output-name> after`.
To reproduce the old failure, extract the named old observer with `git show` into ignored scratch
and point an isolated diagnostic driver at it; never overwrite the frozen current execution source.
The retained `stabilization-lua-checks.py return-stack-regression` execution checks the existing cases.
Load current
private-input configuration to select pinned Lua. No tracked helper tests are added.
**Inferred at this correction:** the asymmetric mask caused the native missed return. The
[subsequent compatible chain](#corrected-chain-and-player-entry-window-count-failure) confirms the
native return and lifecycle; first player-ready acceptance was still **Unknown** at that correction.

Accounting before the compatible restart was **15 actual starts / 4142.950475800084 charged seconds / 32143 resource frames /
555 advancing batches**, including all failures. Observer SHA changes invalidate the old parents under
mandatory identity checks. After independent source acceptance, create a fresh compatible chain with
those totals as reviewed prior consumption. Preserve old pairs and failure receipts; do not rewrite
or waive their identities, or treat the old controller log as an assumed-ready replay. Live readiness
still governs each interaction. At that handoff, first player-ready and its evidence-only final frame
were **Unknown**; the [accepted final chain](#accepted-first-battle01-player-ready-acquisition) resolves
that bounded endpoint. Battle01 actions/victory/5B, native menu cancellation and H4 remain **Unknown**.

### Corrected chain and player-entry window-count failure

**Confirmed:** after [PR #492](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/492), the fresh
chain uses accepted `6b9120a43788c61be68f024cfd5e6d1c376963be`, observer
`B19D23C93F90AF48C021746ACD1A81A6EAE0E1DE7F6BC21387D58437134F6177` and unchanged runner
`10D6483121BABEE8D4460722504D10A1FB17BEF077B8468196E5C04945C84C67`. The ROM, pinned upstream
and BizHawk 2.11.1/Genplus-gx identities are unchanged. It inherits all 15 prior starts and costs.
Only 18 bootstraps R1 (observer/emulator epochs 355/354); every later child loads its complete parent
before input. Independent main-gate reviews accept each successful save through guard 26.

| Prepared attempt / actual start | Parent; logical frames | Result | New resource frames / batches / seconds |
| --- | --- | --- | --- |
| 18 / 16 | fresh; 0→1030 | House rank 1 | 1030 / 11 / 78.33681439998327 |
| 19 / 17 | 18; 1030→2080 | Sarah rank 2 | 1050 / 26 / 104.844541700033 |
| 20 / 18 | 19; 2080→2656 | Astral approach rank 3 | 576 / 14 / 71.5148315000115 |
| 21 / 19 | 20; 2656→7041 | Operator idle timeout; no pair | 4385 / 69 / 407.4769251999678 |
| 22 / 20 | 20; 2656→8585 | Messenger rank 4 | 5929 / 82 / 501.6548577999929 |
| 23 / 21 | 22; 8585→10366 | Map19 rank 5 | 1781 / 33 / 42.77628290001303 |
| 24 / 22 | 23; 10366→15586 | Royal rank 6 | 5220 / 77 / 100.00866600003792 |
| 25 / 23 | 24; 15586→16981 | Local operator abort; no pair | 1395 / 27 / 33.145016900030896 |
| 26 / 24 | 24; 15586→17720 | Guard rank 7 | 2134 / 46 / 53.04010589997051 |
| 27 / 25 | 26; 17720→22348 | Player-entry predicate failure; no pair | 4628 / 181 / 93.92492390004918 |

At 20, the operator sees Down-facing not-ready at 2651 and turns Left before any entity142 C.
No wrong-facing C or FieldMenu is delivered. At 24, royal closure at 15528 faces Left; ordinary
movement reaches the required Down-facing save at 15586. At 26, guard caller closure at 17659
`(4,16)` is distinct from the `(5,15)`/Down save at 17720. Successful pairs retain source identity,
contiguous delivered frames, unique local record order, actual ready-before-C, exit 0 and confirmed
restoration/callback/ROM cleanup. They do not reset resource costs to their logical clocks.

Attempt 21 (PID 9772) times out during task context recovery at the actual YesNoPrompt choice 0.
It exits 1 without forced termination; host termination, session deletion and canonical identity are
confirmed. **Unknown:** final loaded-entry restoration and callback removal, because its status file
only retains `observer-started` and the bridge's last `ready` is not final cleanup evidence. This
differs from the earlier attempt 12; its cleanup wording cannot be copied to 21.

The ignored finite operator for 22 advances the main route with actual position/consumer/readiness
checks, then rejects the legitimate `WaitForPlayerInput` consumer locally. The same live process is
immediately checked and finished manually with C1, neutral60 and safe save. Thus 22 is assisted,
not wholly automatic. The exact executed script is retained as `operator22-executed.js`. Attempts
23 and 24 complete through a direct stdin/stdout operator that waits for each full JSON response.
Attempt 25 (PID 28792) exposes a separate operator bug: while awaiting another acknowledgement,
neutral frames legitimately close Astral's caller with F607/F608 and no active consumers/returns.
The operator fails to recheck completion and requests abort 28, delivering zero additional frames.
Its explicit observer abort failure retains loaded-entry restoration, callback removal, output
removal and ROM cleanup; it is not an original-game mismatch. The corrected local operator checks
stage completion after each neutral wait, including royal, Astral and guard closure, and waits for
gate/guard closure only while an actual consumer/return owns that wait. Attempt 26 completes
automatically. These ignored operators change no observer/runner/bridge or game-state semantics,
extend no watchdog, and send no artificial keepalive.

**Confirmed (failed observation):** [main-gate's attempt 27 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5753234168)
records PID 36576, orders 41946–52958, 13 ready-checked C acknowledgements and exit 1 without timeout
or forced termination. The last neutral120 delivers only 6 frames. At 18369, CheckBattle entry
43706 uses stack `0xFFFFFC`; return 43707 reaches `0x75D0`, result 43708 reports D7=1, admission
43709 precedes BattleLoop 43710 with D1=1/F88 clear. Incoming map 57 remains distinct from raw
source map 40. This is native confirmation of the wrapped-return correction.

The original before-program returns at 22246; load/start return at 22314; activation/region/spawn
return at 22316; generation and first dispatch occur at 22319 with actual actor **2**. Player control
then reaches `0x22E70` at observer frame 22348 / emulator frame 22347. Failure order 52956 reports
`player-ready input/modal/transfer mismatch`: window count 2, no active consumers or pending returns,
typewriting/input/event word zero. This proves entry at the named PC, **not passed player-ready**.
The compound assertion short-circuits; unrecorded later guard values are not established by this
failure. Loaded-entry restoration, callback removal, partial-output removal, session deletion and
canonical identity pass. No final evidence pair is published, and no battle action follows.

**Confirmed (pinned source/H1/ROM):** `WINDOW_IS_PRESENT` at `0xFFB13F` is a count. In
`code/common/windows/windowengine.asm`, `InitializeWindowProperties` clears it at `0x47E4`, tests
MAP_CURRENT at `0x47E8` and increments the non-current-map baseline at `0x47F0`.
`code/gameflow/battle/battlefunctions/executeindividualturn.asm` calls the mini status window at
`0x23F72` before calling `ProcessBattleEntityControlPlayerInput` at `0x23FE6`→`0x24662`.
`code/common/menus/ministatuswindow.asm` increments the count at `0x11572` and returns after its
window movement; this nonblocking battlefield display remains during movement input at `0x22E70`.
The jump at `0x10014`→`0x11572` and the relative input-control call are resolved against named H1
symbols and canonical ROM operands; other cited instruction spans match directly.

The observer correction removes only the two final-battle `windowState == 2` modal exclusions.
It preserves lifecycle/actor/area checks and actual script, return, typewriting, dialogue/portrait,
input, targeting, action, event-word, fade and effective-scroll checks. A `battle:player-ready-check`
record now retains those already-read guard fields before the compound assertion. Field/save/menu
predicates, the runner and RAM configuration are unchanged. Corrected observer SHA is
`F5859C05A4D43A837973BCA95F6B87E974C928EB5FC0E88FE40CF5821A89B218`.

Direct actual-Lua callback checks reproduce rejection of count 2 on the old body and acceptance
after correction, with 15 distinct invalid-state rejection controls. Retained reproduction uses
`check-player-ready-window.py window-count-before-corrected-driver before local/issue485/window-count-old-observer.lua`
for the observer extracted from accepted `6b9120a4`, and
`check-player-ready-window.py <fresh-ignored-output> after` for current source, each under
`uv run python -X utf8` after loading private configuration. The initial local driver's Lua `assert`
returned two values and incorrectly enlarged the area table; its 15/16 result is retained separately,
not a native failure or a passed guard check. Corrected before/after drivers require the named
failure reason and pass all 16 cases. `check-window-source.py` verifies the source/H1/ROM relationships;
the six return-stack checks and 17 stabilization checks pass. Normal `uv run sf2 verify` passes
148 tests plus document/index/ROM/toolchain checks. These are direct verification, not added helper tests.
Fresh `prepared-28` passes offline source/H1/ROM and Lua preparation on the corrected observer with
the reviewed cumulative costs below; it has not launched at this source handoff.

Consumption before the window-count-compatible chain was **25 actual starts / 5629.673442000174 charged seconds / 60271 resource frames /
1121 advancing batches**, preserving every failed branch. Read-only reconciliation is retained in
`summarize-corrected.py` and `corrected-summary-through27.json`; individual candidate, input,
checkpoint, process and pair receipts remain authoritative. New source identity requires another
fresh compatible chain after independent acceptance; do not waive or rewrite parents 18–26.
At that source handoff, native acceptance of all corrected final guards and the final evidence-only
frame remained **Unknown**. The [accepted final chain](#accepted-first-battle01-player-ready-acquisition)
resolves that endpoint; native menu recovery, battle actions/victory/5B and H4 remain **Unknown**.

### Movement-grid palette failure and correction

**Confirmed:** [PR #493](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/493) is accepted at
`d5e9a6cf7aa7a4c8d0e0aff399aa752972b8fbd6` (tree `e8344883fb426bb504f6769c82f98daf16c01249`).
Its observer `F5859C05A4D43A837973BCA95F6B87E974C928EB5FC0E88FE40CF5821A89B218` and unchanged
runner form a fresh compatible chain with all prior consumption inherited. CI run 35543107750 passes
scope/research-public; engine/adapter are skipped. The finite operator completes the entire fresh
R1→house→Sarah→Astral→messenger prefix, including actual `WaitForPlayerInput`, without manual fallback.
Source identity stays frozen through all five attempts; only the ignored summary label changes from
`frames` to `requestedFrames`. Actual costs always come from delivered input records.

| Prepared attempt / actual start | Parent; logical frames | Result | New resource frames / batches / seconds |
| --- | --- | --- | --- |
| 28 / 26 | fresh; 0→8605 | Messenger rank 4; 32 ready-checked C | 8605 / 134 / 174.67164269997738 |
| 29 / 27 | 28; 8605→10386 | Map19 rank 5 | 1781 / 33 / 42.078283500042744 |
| 30 / 28 | 29; 10386→15606 | Royal rank 6 | 5220 / 77 / 99.46084710001014 |
| 31 / 29 | 30; 15606→17740 | Guard rank 7 | 2134 / 46 / 53.35756550001679 |
| 32 / 30 | 31; 17740→22368 | Final palette predicate failure; no pair | 4628 / 181 / 95.60115339996992 |

Main-gate independently accepts parents 28–31 and reconciles failure 32. Fresh R1 epochs remain
355/354; later children load before input without another bootstrap. Royal closure at 15548/Left
is distinct from its Down-facing save at 15606; guard caller closure at 17679 `(4,16)` is distinct
from its `(5,15)`/Down save at 17740. Successful native pairs, clocks, orders and cleanup pass.

**Confirmed (failed observation):** attempt 32, PID 41396, reaches the actual first player-input PC
`0x22E70` at observer frame 22368 / emulator frame 22367, after the original lifecycle and first
dispatch of actor 2. Diagnostic order 52983 records window count 2, `fading=5`, and
`cutsceneOrMenuModal=false`; input, targeting, action, event word, typewriting, dialogue/portrait
indices and effective scrolling are all explicitly zero. The corrected window-count handling now
passes at the native boundary. The remaining `fading == 0` condition rejects this actual state,
producing failure 52984; total record order is 41973–52986. The final neutral120 delivers only 6
frames, with no later battle input. Exit 1 has no timeout/forced termination; loaded-entry restoration,
callback removal, partial-output removal, session deletion and canonical ROM identity pass. This is
a completed failed observation, not a passed ready point or a mere local readback-format error.

**Confirmed (pinned source/H1/ROM):** `FADING_SETTING` at `0xFFDEF0` selects palette effects, including
nonblocking movement-grid pulsation. `sf2enums.asm` defines `PULSATING_1=5`.
`CreatePulsatingBlocksForGrid` in `code/gameflow/battle/battlefunctions/battlefunctions_0.asm` writes
5 at `0x22CD4` and returns through `CheckMapLayerType`. `ExecuteIndividualTurn` calls it at
`0x23F88`→`0x22C84` before `ProcessBattleEntityControlPlayerInput`. The fifth mode starts at
`table_FadingData+32` (`0xB3E`) in `data/tech/fadingdata.asm`; its `0x88` command at offset 10 sends
the palette pointer back eight places. `ApplyFadingEffect` in
`code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm` handles that loop instead of the
`0x80` end marker. Thus mode 5 remains active during player movement; waiting for zero at this seam
cannot establish readiness. This is not an unfinished screen transition.

The final-only correction admits mode 0 or this source-selected mode 5 and records
`paletteModeAllowed` with the raw setting. Other modes remain rejected at this bounded seam; this
does not classify every other palette mode as universally blocking. All other actual guards and the
field/save `fading == 0` checks remain unchanged. Runner, RAM configuration, original state, input
logic and parent identity rules are unchanged. Corrected observer SHA is
`42DA091C807A6BC0262317E168D9519F4AFF5C0ACC1E97EBE4928DF4185FC3BB`.

Direct actual-callback verification under window count 2/mode 5 reproduces the old rejection and
passes the corrected 31-case local check (two allowed states, 15 retained invalid-state controls and
14 other palette modes). Main-gate independently passes 271 cases covering both allowed byte modes,
all 254 other byte values and 15 existing guard negatives. Retained commands are
`uv run python -X utf8 local/issue485/check-player-ready-pulse.py <fresh-output> after` and
`uv run python -X utf8 local/issue485/check-grid-pulse-source.py`, after loading private configuration.
For the old body, supply the observer extracted from `d5e9a6cf` as the driver's fourth argument and
use `before`; that mode does not itself select historical source. The source check binds setter,
call and input instructions to H1/ROM. H1 truncates the long table row, so only its displayed eight-byte
prefix is compared as H1 evidence; the full row and loop byte are compared directly from pinned source
to canonical ROM. The initial incomplete-listing-span diagnostic is retained, not interpreted as a
ROM mismatch. Six return-stack and 17 stabilization checks pass; normal `uv run sf2 verify` passes
148 tests and document/index/ROM/toolchain checks. No verification-helper tests are added.

Consumption before the palette-compatible restart was **30 actual starts / 6094.842934200191 charged seconds /
82639 resource frames / 1592 advancing batches**. Read-only reconciliation is retained in
`corrected-summary-through32.json`; authoritative receipts and all prior failures, including attempt
21's final cleanup Unknown, remain intact. Fresh `prepared-33` passes offline source/H1/ROM and Lua
preparation without a launch for another compatible chain after source acceptance; no old source
parent may be reused. Native acceptance of the corrected final palette guard and evidence-only final
frame was **Unknown** at that source handoff; the following result resolves that bounded endpoint.

### Accepted first Battle01 player-ready acquisition

**Confirmed:** the fresh compatible chain 33→34→35→36→37 reaches the first Battle01 player-ready
callback and seals its evidence-only native state/continuation pair. [Main-gate independently accepts](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5753580250)
all five segments, including the final guards, lifecycle, parent/source identities, clocks, delivered
inputs, mandatory pair hashes and cleanup. This is the bounded Issue #485 result; it does not complete
Issue #437, Battle01 actions/victory/5B, frozen replay or H4.

Source acceptance is [PR #494](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/494), commit
`1b84a878fbe48b6bae1ed3e3274269efbcf90317`, tree `fc1a3d3e9ce1f7a1f443700d3d9b87ab374a44be`.
CI run 35544390278 passes scope/research-public and skips engine/adapter. All five segments freeze
observer SHA `42DA091C807A6BC0262317E168D9519F4AFF5C0ACC1E97EBE4928DF4185FC3BB` and unchanged
runner `10D6483121BABEE8D4460722504D10A1FB17BEF077B8468196E5C04945C84C67`, with the USA ROM,
pinned SF2DISASM baseline and BizHawk 2.11.1/Genplus-gx identities above. Only 33 bootstraps R1,
at observer/emulator epochs 355/354; each later child verifies its complete parent load before input.
The finite direct stdin/stdout operator completes all five segments without manual fallback or a
changed input policy. Every C below follows actual consumer/entity readiness.

| Prepared attempt / actual start / PID | Parent; logical frames | Saved endpoint | Actual frames / batches / C / charged seconds |
| --- | --- | --- | --- |
| 33 / 31 / 21728 | fresh; 0→8605 | Messenger rank 4, Map3 `(43,10)`/Down | 8605 / 134 / 32 / 176.29173870000523 |
| 34 / 32 / 28440 | 33; 8605→10386 | Map19 rank 5, `(26,29)`/Up | 1781 / 33 / 6 / 42.333924799982924 |
| 35 / 33 / 40308 | 34; 10386→15606 | Royal rank 6, Map20 `(23,39)`/Down | 5220 / 77 / 22 / 99.83197920001112 |
| 36 / 34 / 22484 | 35; 15606→17740 | Guard rank 7, Map21 `(5,15)`/Down | 2134 / 46 / 8 / 53.93224510003347 |
| 37 / 35 / 39136 | 36; 17740→22368 | Player-ready rank 8, Map57/Battle1 | 4628 / 181 / 13 / 95.18590199999744 |

The first row includes all 355 bootstrap frames. Royal caller closure at 15548/Left remains distinct
from the Down-facing save at 15606; guard caller closure at 17679 `(4,16)` remains distinct from its
save at 17740. Shared record orders are respectively 1–20106, 20107–24342, 24343–36692,
36693–41972 and 41973–52988. Actual frames are contiguous across successful parents; offline gaps
do not advance the original game or reset costs.

The final child observes Map21→40 at 17945 and the original initialization return at 17997, then
the Map57 warp at 18389. CheckBattle entry/return/result orders 43733/43734/43735 establish D7=1;
admission 43736 precedes BattleLoop 43737 with D1=1 and F88 clear. At frame 22266, before-program
`script:return` is order 52738, owning function `battle:before:return` is 52739 and the
`natural:before` completion checkpoint is 52740. Load/start completion checkpoints are at
22334/orders52883/52889; activation/region/spawn completion checkpoints are at
22336/orders52897/52901/52905; generation/first dispatch occur at 22339/orders52915/52916.
Actual first actor is **2**, not the seeded R2d actor 1. F401/F451 are set and F501 is clear.

At `ControlBattleEntity` `0x22E70`, after `WaitForVInt` and before input read, guard order 52983
records window count **2**, palette setting **5**, `paletteModeAllowed=true` and
`cutsceneOrMenuModal=false`. Input, targeting, current battle action, map event word, typewriting,
dialogue/portrait window indices and effective scrolling are all zero. There are zero pending blocking
consumers; actor/moving actor/view entity are all 2 and area is `[0,0,16,20]`. Natural-ready order
52984 precedes stop order **52985**, observer frame **22368**, emulator frame **22367**.
The neutral completed-frame save is separately order **52987**, PC `0xF00`, observer/emulator frame
**22368/22368**. Command 181 requests neutral120 but delivers only **6** frames, skipping the other
114. The operator's `requestedFrames=4742` is not actual delivery: the receipt records **4628**.
No battle action or further gameplay input follows readiness.

The terminal checkpoint is rank 8/ordinal 4, `player-ready`, **resumable=false**. Its native state is
167514 bytes, SHA `63EE0E6D14660E8825362CF75A8DC586427C0BED533BFAA1225F9F38D4B36B40`; pair SHA
is `BC5F0D377C42772B839D8DA519755EA5679964BCA6FE1EE4986A2B5B4DC349A3`. These identify local
private evidence, not distributable artifacts or a gameplay continuation parent. All five processes
complete with exit 0, no timeout or forced termination, confirmed entry-state restoration and callback
removal, session-ROM deletion and unchanged canonical ROM. Initial 33 restores its bootstrap entry;
children restore their loaded entry. Attempt 21's separate final-cleanup **Unknown** remains intact.

Current cumulative consumption is **35 actual starts / 6562.418724000221 charged seconds /
105007 resource frames / 2063 advancing batches**, including every earlier failed branch. The final
successful lineage contributes 22368 resource frames, 471 batches, 81 actual-ready C acknowledgements
and 467.5757898000302 active seconds. Retained read-only commands
are `uv run python -X utf8 local/issue485/check-final37.py` and
`uv run python -X utf8 local/issue485/summarize-corrected.py corrected-summary-through37.json`.
They pass; `final37-acceptance.json` and `corrected-summary-through37.json` retain their results.
Main-gate's separate `root494-segment-review.py 33` (also 34, 35 and 36) and
`root494-final-review.py` pass, with corresponding `root494-segmentNN-review.json` and
`root494-final-review.json` reports. Independent `root494-chain-review.json` reconciles the complete
five-process lineage, and OS inspection finds all five recorded PIDs absent. The candidate/input/checkpoint/process/pair receipts remain
authoritative. A resumable-parent reader intentionally rejects the final pair and is not its acceptance
command. These commands inspect retained local evidence; no native rerun is needed for this result.

**Unknown:** Battle01 actions, victory and 5B, native menu recovery and H4. This result is savestate-linked
original acquisition from controlled R1, not natural visible New/load, uninterrupted wall-time
execution or frozen replay. Final-result documentation changes only the four existing owners and uses
direct document/scope checks, a committed verification plan and public CI; source, normal/full suites
and native execution are not rerun. Main-gate owns integration and Issue #485 closure; #437 remains open.

### Segment 1 wrong-facing failure

**Confirmed:** accepted base `ca175155998e13c62d6daacd67ed60bd0203af99`, immutable `prepared-06`,
historical start 6, PID 34132, ran 2026-09-20 20:03:46.486608–20:12:30.704421 UTC.
The [retained failure receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752384854)
records 524.2179030999541 active seconds, 2821 completed frames and 51 advancing batches, bringing
cumulative consumption to the totals above. At completed frame 2818 the player was Map3 `(55,17)`
facing Down. The operator sent C2 without consuming the existing entity142 waypoint's Left facing
toward `(54,17)`. FieldMenu entered at frame 2821 / `0x2127E`, causing the then-required callback
failure. The final neutral120 advanced only one frame; 119 requested frames were not delivered.
F601/F256 and basement entry were reached; F602, messenger, gate and Map19 were not. The process
exited 1 without timeout/forced kill; restoration passed, callbacks were removed, session ROM deleted
and canonical ROM unchanged. No savestate, continuation or pair was produced. This failure is retained;
the new guard/recovery policy does not relabel it successful or refund its costs.

### Segment 1 camera readiness failure

**Confirmed, independently reconciled by main-gate:** the sole admitted `prepared-03` invocation
on accepted `069ffe9008792dd73f824397674ee4dc445cd962` was historical start **5**, PID45316,
2026-09-20 18:44:59.131595–19:10:51.321569 UTC. It consumed **1552.1900652000331** active seconds,
**10740** contiguous completed frames (355 bootstrap, 10385 operator), and **174** advancing batches.
Combined with the earlier registration failure, cumulative consumption is **1556.4798537000315**
seconds / **10740** frames / **174** batches. Full raw identities and reproduction are retained in the
[failure handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752014834).
The clock registration succeeded; the completed earlier actual-library proof is retained, not rerun.

R1, house/Sarah/Astral, messenger acceptance and follower flags, gate F604, north warp and Map19
init/control were observed. Gate F604 completed at observer frame 10451; Map19 init entered/returned
at 10674; first field wait was 10700; first Up acceptance was 10724. The displacement marker at 10725
records tile-floor coordinates, not a settled camera. After sixteen neutral frames, completed
frame 10740 showed Map19 `(26,29)`, raw player X9984/Y11136, zero input/event word/pending returns/
consumers and a fresh field poll. Command 175 (`save`) failed before native save with
`segment camera unsettled`. Preceding closure/modal/player-destination assertions passed. The old
snapshot omitted camera raw values: the exact discrepancy and additional settling time are **Unknown**.
No camera values, save state or completed acquisition are inferred from the displacement marker.

Exit 1 was completed, without timeout/forced termination. Scope/session restoration and callback
cleanup were true, callback count zero, session ROM deleted and canonical identity unchanged. No
`segment.State`, `continuation.json`, `segment-pair.json` or final observation was produced. Preserve
all failed runtime/preparation material; it cannot serve as a parent. This is a completed failed run,
not an interrupted queue to restart.

**Confirmed static source/ROM boundary:** pinned `SF2DISASM`
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `camerafunctions.asm` / `animations.asm`, contain
`WaitForViewScrollEnd` at `0x4708` and `IsMapScrollingToViewTarget` at `0x4728`.
The latter reads `VIEW_SCROLLING_PLANES_BITFIELD` (`0xFFA82A`); nonzero packed X/Y autoscroll words
at `0xFFA846` / `0xFFA848` respectively mask it with `3` / `12`. Clear effective bits are the
instantaneous no-scroll predicate. `WaitForViewScrollEnd` additionally waits/checks across VInts;
readiness does not claim to execute or prove that full temporal routine. `VInt_UpdateViewData`
selects plane B for layer type 0 and A otherwise. Map19 area bytes at `0xA45E0` select type 0,
zero autoscroll for both planes and 256/256 parallax. Preparation binds both complete function spans
(after resolving H1's five forward-branch placeholders from named labels) and all 32 area bytes to
the canonical ROM. The initial direct H1 comparison failure is retained; unresolved listing operands
were not a ROM or runtime defect. Readiness preserves A-coordinate equality and additionally requires
the original masked scrolling bits clear. The failure alone does not justify removing either guard.

**Confirmed offline implementation boundary:** direct actual-Lua checks cover readiness, typed
nonterminal protocol, unchanged paused/idle/progress budgets, subsequent explicit steps, fatal failures,
source masks and resumed resource offsets. Direct runner/bridge checks cover reviewed values, sealed
parent inheritance, reconciliation and unchanged host deadlines. Drivers and results remain ignored
under `local/issue485/`; no helper tests or native process were added. These checks do not prove actual
native save/load or the time needed to settle the camera.

**Confirmed adopted policy:** [main-gate decision 5752243620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752243620)
sets only the first Map19 absolute cumulative deadline to **3600 seconds**. Remaining total resources
are **5643.5201462999685 seconds, 25260 frames and 426 batches**; reviewed prior consumption is unchanged.
The former 2400 deadline left only 843.5201462999685 seconds, below the last reacquisition cost.
The adopted deadline leaves **2043.5201462999685 seconds**, **491.3300810999354** above that observed
cost. This is an allowance, not a prediction or guarantee of reaching a saveable boundary. The runner,
host limit validation and generated observer configuration consume this same stage value. Total 7200,
guard 5700, frame/batch/progress/idle limits remain unchanged. `prepared-05` and prior material remain
immutable; new prepare-only material records the adopted limit without granting execution. Main-gate
must independently accept/merge the final material and issue concrete admission before any process.
No retry, replay, smoke,
backup-state search or broader verification queue is authorized; Issue #485 remains open.

### Segment 1 clock registration failure

**Confirmed, independently checked by main-gate:** the first invocation of frozen
`local/issue485/prepared-01` on accepted `1cce8b997a4f244e5e1461cf4b4bb65a26ea0264` started PID18440
at 2026-09-20 18:08:49 UTC, then exited 1 after **4.289788499998394 active seconds**. Its sole
checkpoint is `candidate:registration` failure at observer/emulator frame 0, before controlled R1,
with `stopwatch` nil at the `StartNew()` call. Host hello failed with WinError 10054. There were zero
commands, input batches or delivered frames and no native state, continuation metadata or pair.
This is actual historical controlled start **4** despite not completing segment 1. Full identities
and bounded diagnostics are in the [failure handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5751654860).

Process termination, zero retained callbacks, session-ROM deletion and unchanged canonical identity
were confirmed; there was no timeout or forced termination. `scopeArmed=false` and
`sessionStateRestored=false` remain a failed/unarmed restoration result, not successful acquisition
cleanup. Retain the original runtime directory and all prior failures unchanged. The consumed time
above must enter new first-segment material as reviewed prior active seconds; preparing a new
candidate does not erase it, renew the 7200-second allowance, or admit a retry/second segment.
No resumable parent exists. Retained `local/issue485/prepared-02` is stale and forbidden for execution:
it incorrectly carries zero prior active seconds. Preserve it unchanged; replacement prepare-only
material and its identities are recorded in the Issue handoff.

**Confirmed (actual-library offline boundary):** the installed release's NLua 1.4.1.0 and Lua 5.4,
hosted on CLR 4.0.30319.42000 with the release's .NET Framework 4.8 binding configuration, reproduce
`luanet.import_type("System.Diagnostics.Stopwatch") == nil` in a fresh NLua interpreter. Loading
`System` first resolves the type and permits increasing `Stopwatch.Elapsed.TotalSeconds` readings;
`System.DateTimeOffset` and its Unix milliseconds conversion also resolve. The observer now uses
the existing `luanet.load_assembly` convention and explicit type assertions before clock use.
Its elapsed clock remains monotonic; UTC is used only for the existing one-time launch offset.

To reproduce without starting EmuHawk or loading a ROM/core, instantiate the installed
`NLua.Lua(true)` in a small .NET Framework console host, use the pinned `dll` directory for managed
dependency/native Lua resolution and copy the release's `EmuHawk.exe.config` binding redirects as
the host config. Execute the observer's actual `if natural` clock-initialization block through
`Lua.DoString`, with `SF2_BRIDGE_LAUNCH_EPOCH` supplied before host startup. In a fresh interpreter,
first record the unresolved import; then run the corrected block and verify a second elapsed sample
exceeds the first, without replacing `luanet`, the imported types or clock with stubs. Check the loaded
assembly/module inventory excludes BizHawk/game/core components. This checks the real Lua/.NET type
and call boundary only; it does not establish the complete EmuHawk registration/transport, native
state roundtrip or original route.

The earlier offline mechanical checks supplied successful `import_type` stubs and therefore could
not detect the missing assembly initialization. Their PASS was not native compatibility evidence.
The failed original run, initial offline-host constructor/dependency/compiler setup errors, and the
corrected actual-library observations remain under ignored `local/issue485/`. This correction adds
no helper suite or game launch. New review material must use a fresh destination, explicit current
prior-start accounting and NOT-ADMITTED status; the failed `prepared-01` must not be overwritten.

## Single admitted interactive acquisition result

**Confirmed bounded original observation, independently accepted by main-gate in
[PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476):** Issue [475](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/475)
consumed its sole API call/native start on 2026-09-20 UTC (2026-09-19 America/Chicago launch date).
The unchanged accepted base was `9cb14a55c2777b515a18450e98fc37ca90350292`, tree
`63659d8c0d16f662279727dc1301f641ef0a3d4c`. This separately admitted interactive controlled-R1
acquisition superseded preparation's NOT-ADMITTED disposition only for that attempt. The return was
`OBSERVATION-COMPLETE-UNREVIEWED`. This immutable acquisition-runner label is preserved after
main-gate acceptance of the bounded original observation; it is not a frozen-replay PASS, public
golden, complete 8D or H4 PASS.
Historical controlled starts are now **3**; #460/#465 completed FAIL, #471's pre-API CRLF assertion
and unexecuted house-neutral preparation, stale #473 prepared-01, and disabled replay ordinal 1/2
failures/restrictions remain preserved. No retry, fourth start, follow-up replay or cleanup is authorized.

### Frozen material and actual transport

Read-only Git/ownership/process checks found a clean transferred Research worktree, no open PR and
no EmuHawk. The retained `local/issue473/prepared-02/runtime` did not exist. Current private-input
configuration and narrow ROM verification passed; all report source/H1/fixture/helper/tool identities
matched without preparation or material changes. The pre-call note and full identity check are retained
under ignored `local/issue475/`. Exact SHA-256 identities were:

| Material | SHA-256 |
| --- | --- |
| `candidate.json` | `73AE17BEA9290DF305A313063F139429E329114BDEE3DBD578B3695DEB591F84` |
| `config.json` | `B0C0D8BB16F3440BB64F9851257DAEFD81288C7E1C8782C322F0EE15B8E38A80` |
| `config.lua` | `C58438B4B3A830C00A145774EBFEFD2C102269F71A38EDF039D0F69B76CBC2DB` |
| Empty mode declaration `input.json` | `05562B5546BEBE81C0EA462E2285268CA6CB943091F7B5A01536CD6D94C4AA95` |
| Observer | `33766BE3A243EE782AF0F156DD6B48EE15BF8CEF0D20D285AC8CB66A68C063A3` |
| Runner | `37FAF15B03745C984316456051C6C7CA242A54E59078CC1B281F8D7A4927CC27` |

The existing `run_map3_observation_candidate(..., interactive=True)` ran once through a persistent
PTY with `uv run python -X utf8 -c`; stdin remained available for one JSON array plus carriage return
per command. No transport drycheck, warmup, smoke, replay or second API call occurred. All 200 commands
were explicit `step` requests; there were no B/A inputs, state/ping probes, abort or disconnect.
Button/release choices used current coordinates, source route topology, active/pending consumers and
fresh polls. The post-join choice also used text 447's actual return plus the existing source chain
`csc08_joinForce` → `FadeOut_WaitForP1Input` → `WaitForPlayerInput`. That wait has no dedicated poll
in this observer: its exact live entry was **Inferred**, not established by the stale text poll.
After C at frame 8495 the original window-close callback ran, then the messenger returned.

### Reached source boundaries

Frames below are observer frames; callback `emulatorFrame` is one lower in this run. They are
provenance for this acquisition, not gameplay timing requirements or a reusable input schedule.
All PCs/symbols refer to the pinned source/H1/ROM identified above.

| Boundary | Actual observation |
| --- | --- |
| Controlled R1 service/scratch restoration | Frame 355 / emulator 354, `WaitForEvent` `0x2591C`; patches/scratch restored before operator input. All 30 retained ally status words read 0, hence no inherited POISON in this attempt. Live entity/index and raw RNG/time readbacks remain private; raw time was not normalized. |
| House, Sarah and Astral | House `cs_5145C` entry/return 501/1015; Sarah `cs_513D6` 2025/2052; Astral exit `cs_5148C` 3541/3543. Original house and school stair handlers reached; F601/F602 became true. No FieldMenu callback occurred. |
| Messenger and prompt | `cs_5149A` entry 3881, YesNoPrompt entry 7267, fresh choice poll 7291, C 7292, original return D0=0 at 7297 (`0x47498`). Accepted-path text IDs 517–531, 535–536 and join text 447 reached their real DisplayText consumers. |
| Follower-ready return | Messenger return 8514; follower-ready wait 8515 at `(43,10)`, Down, F600/F66/F603 true, F604 false. All tracked script/text/prompt/close consumers returned. |
| Gate `Map3_ZoneEvent4` / `cs_51652` | Entry 8903 at the source-selected gate edge. Six real text IDs 537–542 completed. Script return at 10032 has order 20952, followed by F604 false before trap at `0x50E3E` (20953) and true at `0x50E42` (20954). Guard readbacks are retained; no unobserved entity motion is inferred from script return alone. |
| North warp | Frame 10197, original handler `0x25978`, source `(28,2)` to target `(28,1)`, operands `[0,19,26,30,1]`; effective destination Map19. |
| Map19 setup and consumers | `ms_map19_InitFunction` `0x530EA` entry, `cs_53104` entry/return, init return all at 10259, orders 21446–21449. First wait `0x2591C` at 10285 precedes original controller installation. |
| Sole terminal | Frame 10329 / emulator 10328, original movement acceptance `0x52E8`, Up input 1, Map19 `(26,30)`, F604 true, event word 0, pending returns 0 and every tracked consumer count 0. The completed frame remains `(26,30)`; acceptance of movement does not prove next-tile arrival. |

### Complete logs, process and cleanup

**Confirmed:** PID 39748 ran BizHawk 2.11.1 / Genplus-gx, startup 9.433 seconds and total owned
elapsed 1778.575 seconds including paused operator time, within 1800. Exactly 10329 applying/completed
frame pairs cover emulator frames 0→10329 without gaps: 355 bootstrap frames (214 Start, 141 neutral)
and 9974 operator frames (8351 neutral, 302 Left, 597 Right, 325 Down, 361 Up, 38 C). The 200 batches
all contain 1–120 completed frames, below 2048 batches and 28634 total frames. No partial batch or
skipped remainder was exercised because the last request was one Up frame. The 582 checkpoints and
input records share all 21640 unique ordered records; each command/result agrees with the host receipt.

The terminal callback precedes final frame completion. Its checkpoint reports emulator 10328 and
its snapshot has raw frame-counter 206; the preserved frame-end snapshot reports emulator 10329 and counter 207. Final
restoration then reloads the declared bootstrap core state: the enclosing last `result` log metadata
reports emulator 214 while its nested acquired snapshot remains 10329. This is a cleanup boundary,
not a missing input-frame interval or permission to replace the acquired terminal with restored state.

Host/API exit was 0, `timedOut=false`, `forcedTermination=false`, process terminated and bridge
Lua status `closed`. Candidate status ends with `callbacks-cleared:0` / `observer-finished`, without
failure lines. Every declared restoration field is true; `outputRemoved=false` preserves the result.
The runner deleted the session ROM and rechecked canonical identity unchanged. Independent OS
inspection after completion found no PID/runtime-copy survivors and zero EmuHawk; no containment
or extra cleanup was needed. Host stdout/stderr slots are null in this composition; actual native
output remains in the bridge process log, while `local/issue475/api-console.txt` retains API stdout.

Private runtime evidence stays in `local/issue473/prepared-02/runtime/`:

| Artifact | SHA-256 |
| --- | --- |
| `actual-inputs.jsonl` | `21A9422DA146189FE8C46CAAAE141E7694EF24F67C6B135AB077F896C514E021` |
| `checkpoints.jsonl` | `E7D50E264F73DECB6129240067F5E36FCBB177954903F8634E829224054748C2` |
| `host-status.json` | `9716B8BC9FCAC809E6AC688B2D9B188556C7C198846748EAE50D6584E89A0E46` |
| `observer.observed.json` | `EA6EE99767C49F4B9E16CDD48C91AAC9C4634B1BC03E307FCBCBBAAE69EAB8D4` |
| `bridge/receipt.json` | `1C0B1158A20F9D24CA1D68B6807DF7078D08387696B856D0370C4763AEDF5F15` |

### Verification and remaining boundary

Read-only result reproduction loads those existing JSON/JSONL/status files: pair `applying`/`frame`
by ID/button/beforeFrame, require consecutive completed frames, merge checkpoint/input `order`, join
all 200 commands/results against `bridge/receipt.json`, and inspect the named callback sequence,
terminal/restoration/host status and independent OS result. The retained direct command is
`uv run python -X utf8 local/issue475/check_result.py` (with current private configuration loaded);
it does not invoke or import the acquisition runner. Its exact assertions and summary remain local.

The core retained-record integrity check can also be reproduced without that local analysis script:

```python
import json
from pathlib import Path
root = Path("local/issue473/prepared-02/runtime")
inputs = [json.loads(line) for line in (root / "actual-inputs.jsonl").read_text().splitlines()]
checkpoints = [json.loads(line) for line in (root / "checkpoints.jsonl").read_text().splitlines()]
frames = [row for row in inputs if row["kind"] == "frame"]
assert [row["afterFrame"] for row in frames] == list(range(1, 10330))
assert sorted(row["order"] for row in inputs + checkpoints) == list(range(1, 21641))
terminal = [row for row in checkpoints if row["kind"] == "map19:first-original-movement-acceptance"]
assert len(terminal) == 1 and terminal[0]["frame"] == 10329
assert terminal[0]["state"]["pendingReturns"] == 0
```

Preserved local analysis failures: identity checker 01 compared raw CRLF bytes to normalized source
text hashes; checker 02 hashed the returned projection wrapper instead of its `projectionSha256`.
The corrected checker 03 matched existing preparation semantics. Result checker 01 incorrectly
required a gap between final frame order and result order; corrected checker 02 accepts adjacent
records and passed on the same completed run. No material, source, input or native result was repaired,
and no native invocation followed any analysis correction. One tool-output formatter failed before
sending a controller command; it was corrected without an emulator restart or extra input.

Direct result/material checks and 64 document links/anchors passed. Acceptance also records the
clean committed planner and actual Public CI on the Draft PR. No normal/full/H1/H2/H3 queues or new verification-helper tests are authorized.
**Unknown:** unobserved prompt branches, native EOF/abort recovery, dedicated long-idle thresholds,
partial-batch termination, repeatability/frozen replay, downstream tower/battle/5B continuity and
complete 8D/H4. Paused snapshots and ordinary controller delivery are observed only for this run;
no general backend or presentation compatibility follows. Main-gate independently accepted these
bounded original observations in [PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476).
Downstream contract adoption and broader acceptance remain separate; integration belongs to main-gate.

## Boundary

This is a continuation of the accepted R1 admitted-start and R2 natural-opening fixtures. It begins
at the original ExecuteMapScript callback at ROM 0x4712C, with A0 = cs_5149A (0x5149A), before
the first script word is interpreted. It ends only after Map3_ZoneEvent8 sets flag F603 at 0x50EE4,
returns at 0x50EE8, and the original WaitForEvent at 0x2591C is stable.

The public fixture retains the accepted R1 and R2 fixture bytes and projections as prelaunch and
golden-boundary guards. It does not promote either prefix again. At R2 entry, F603=false and the
field menu remains **NotReached**.

## Confirmed source and runtime facts

**Confirmed:** scripts_1.asm:cs_5149A is ROM 0x5149A..0x51651 (440 bytes, SHA-256
01C2ACC81830937BDD6510F88F9FA4E4BF67D6E8F1E49A6693BAEF19B88068AA). The accepted
original-default path contains 116 parsed operations. Its prompt path is csc11_promptYesNoForStoryFlow
at 0x47490 → YesNoPrompt at 0x15284 → SetFlag F89 → csc0C_jumpIfFlagSet at 0x47418 →
cs_51614; the original default-zero return is the accepted choice. Decline/re-prompt is
source/H1/ROM-only, not a runtime claim.

**Confirmed:** source/H1/ROM guards fix the accepted order F600, F66, csc08_joinForce selector 128,
Sarah then Chester JoinForce at 0x9956, the two followentity commands, guards 138/139, csc_end at
0x51650, and the Zone Event 8 commit. The observer requires service and return callbacks; it never
infers completion from callee entry.

**Confirmed:** the one runtime case records 17 reached map-script text commands with IDs 517–531 and
535–536, plus csc08 join text 447. Public data contains IDs, raw source-compatible speaker operands,
and a control-shape hash only. A speaker operand is the source macro modifier/entity word: the Sarah
portrait form is 0xC001, not a normalized character ID. Dialogue prose, captures, assets, and audio
remain private or unobserved.

**Confirmed:** runtime callbacks observe the prompt acceptance, F89 branch, joins of Sarah (1) and
Chester (2), UpdateForce/JoinBattleParty completion, Sarah→Bowie and Chester→Sarah follower links
(distance 2), endpoint Map 3 / 43,10 / Down, Zone Event 8 F603, and the stable wait. The endpoint and
active-party result are fixture/model-owned observed facts, not source-only goldens. Character aliases
for guard selectors 138/139 are resolved through GetEntityAddressFromCharacter and the entity-index
list before physical entity readback.

## RA effect and exclusions

- **RA-03 Confirmed:** extends through this accepted messenger body and follower-ready wait only.
- **RA-04 Confirmed:** only the post-messenger launch state above; Castle, Maps 19/20/21/40/57,
  CheckBattle, Battle 01, and all before/start cutscenes remain **Inferred** or **Unknown**.
- **RA-08 Confirmed:** field-menu **NotReached** remains limited to this extended prefix.
- **RA-09 Confirmed:** reached command/control/text-ID/speaker/prompt/join chronology only.
  Rendered prose, speaker/window presentation, and timing are **Unknown**.
- **RA-11 Confirmed capability inventory:** reached dialogue windows, Yes/No UI, entity/camera
  animation, join music/audio, and VInt/DMA/CRAM/VDP surfaces are private immutable inputs or H4
  questions. No tolerances, audio/pixel claim, or complete 8C closure is made.

Excluded: persistence; optional Map 3/menu content; decline/re-prompt runtime; R3/R4; Phase 4,
Godot, MCP, remake or product changes; and redistribution of private payloads.

## Reproduction

    uv run sf2 h3 map3-messenger-acceptance --timeout-seconds 300

The verifier validates all three closed schemas, source/H1/ROM derivations, retained R1/R2 projection
digests before launch and at the golden boundary, then requires a typed-clean callback status. A failure
removes output, restores the declared scope, clears all callbacks, and returns nonzero. The disposable
session ROM is deleted; canonical ROM bytes are rechecked unchanged.

## Unshimmed Map3 candidate (preparation only)

**Confirmed capability; bounded diagnostic failed:** Issue #456 adds `prepare_map3_observation_candidate` and
`run_map3_observation_candidate` to the existing Python owner, with an opt-in `candidate` mode in
the same observer. The old command, default R2a cases, schemas and observed fixture remain unchanged.
Neither Python function is wired into the CLI. Preparation performs no original emulator launch;
execution requires an independent method and lineage/budget disposition. The two separately
admitted diagnostics below failed and exhausted their respective permissions. A successful API return
would be
`OBSERVATION-COMPLETE-UNREVIEWED`, never a public golden or H4 verdict.

The controlled prefix still uses the accepted CheckSram return redirect, checkpoint/menu thunk,
NewGame→SaveGame→MainLoop→default Map3 setup/init path. At the first `WaitForEvent`, before
original player control installation, the candidate checks the observed R1 player/ally/item-byte/
spell/party/difficulty fields, gold, idle event word and source-proved initial route guards. It reads
all 30 **16-bit** status words at `COMBATANT_DATA + id * COMBATANT_DATA_ENTRY_SIZE +
COMBATANT_OFFSET_STATUSEFFECTS` and reports POISON using the source mask, without asserting its
omitted value. Entity records and index mappings are private raw readbacks for active-NPC comparison.
RNG, its copied byte and raw VInt time are captured; R1's post-boundary normalized time is not imposed
on this continuation. The [admitted-start owner](map3-admitted-start.md) supplies those boundaries.

Before starting its input clock, it restores **all three** session patches (menu alias, name alias,
DisplayText) and verifies both cartridge and bus bytes. It also restores generated checkpoint/thunk
RAM. The original CheckSram return redirect has already been consumed; the original main-loop calls
own the live stack, so the pre-bootstrap stack is not written over it. Retained setup mutations are
the original controlled NewGame/SaveGame/default-map state, inherited status/NPC/RNG/raw time and
the in-process bootstrap lineage. This is not natural New/load or passive reset replay.

After that boundary, the candidate does not call the adaptive route driver, write automation markers,
inject MAP_EVENT, bridge flags/positions/RNG, force entity completion, or advance PC. Only original
execution changes game state until terminal/failure cleanup restores the saved core/scope. Ordinary
joypad input comes from the already existing `set_messenger_input` boundary and a prebound frame
array. The epoch is selected once at the admitted wait; subsequent selection depends only on elapsed
frames. Callbacks can record, fail or stop, but cannot reschedule input. The generic scenario facade
is a data-only descriptor check and cannot supply a movie/start; the disabled replay materializer's
fixed 33-row recording is unsuitable. Neither is revived or silently substituted.

### Source binding and observation boundary

The canonical ROM and pinned source revision at the top of this document remain required. Material
preparation validates the retained R1/R2/R2a projections, the actual H1 listing, original source
sections, final ROM operands, and Lua syntax. Newly consumed source files must match their exact
pinned Git objects. H1's unresolved PC-relative listing words are **not** final ROM operands:
`byte_50E32` binds the `chkFlg 604` branch and original LEA/script macro; ROM displacement decoding
binds `cs_51652`. The original F604 trap is `0x50E3E`, followed by the original RTS at `0x50E42`.

The observer retains messenger text/prompt/join/follower callbacks and checks R2's messenger entry
and R2a's follower-ready state. It records actual script/text/close-window entry and stack-matched
return, text wait1 and acknowledgement input reads, controller reads and accepted movement. A
single physical-PC dispatcher handles shared return sites. Callback failures use the shared failure
prefix/nonzero exit with private `kind=map3-candidate-callback-failure` JSON; JSONL preserves actual
checkpoints and the last checkpoint on failure. Candidate phase/role names are not claimed to satisfy
the closed legacy R2a failure schema. That schema and the default rail remain unchanged.

Gate admission requires R2a, original `Map3_ZoneEvent4`, `cs_51652` return, original F604 trap/readback,
then the north warp. `csc14_setEntityActscriptManual` (`0x46950`), `loc_46966` and `loc_46970` bind
action installation and the original idle wait. Entity 138 is **not awaited**; its sampled state is
not declared complete because entity 139 or the program returned. Entity 139 must actually have
`eas_Idle` at its awaited command return. The first Map19 wait requires original warp/init and
`cs_53104` returns with no tracked program/text/close return pending. The terminal is the first
original player movement acceptance at `loc_52E8` after that wait, with Map19, no pending event,
no typewriting and F604 set. No royal/tower, battle or 5B continuation is included.

Unexpected programs, warps or FieldMenu entry, ordering/state drift, exhausted fixed input, frame
budget, host timeout and cleanup failure remain failures. A fresh `runtime` directory is mandatory;
the execution composition reserves a fresh attempt before identity checks and refuses to overwrite it.
It copies the canonical ROM to a disposable session, preserves checkpoint/status/host diagnostics,
and independently attempts session deletion and the canonical-identity check even after failure.
The accepted [shared-tool mechanism](../operations/local-private-inputs.md#bizhawk-runtime-copies)
resolves the reviewed EXE/Lua identities from the shared installation and copies verified release
files into `runtime/observer/bizhawk-*` before any separately admitted launch. Its executable,
configuration, cwd and TEMP/TMP stay in that local copy; observer outputs remain beside the
`runtime/observer` stem. The candidate opts into `run_observer`'s diagnostic callbacks, which use
`run_native_bizhawk_process` and `NativeProcessResult`. Callers without the optional result callback
retain their existing process/timeout/status behavior; the native helper's existing consumers also
retain their default behavior.
Preparation itself creates only `input.json`, `config.json`, `config.lua` and `candidate.json` in an
explicit fresh worktree-local ignored directory. Missing input is `FileNotFoundError`/unavailable;
bad input/source binding is rejected before materialization, never `PRELAUNCH-PASS`.

### Private process diagnostics

Issue #460 retains one `runtime/host-status.json` per attempted composition, including identity or
preparation failure before process creation. `process.started=false` distinguishes that boundary;
PID is written immediately after creation, before waiting. Exit code, stdout/stderr, timeout,
termination and tree-kill result come from the native helper, never exception-message matching.
Timeout is persisted before termination is attempted, so a failed kill cannot erase that fact.
Unavailable results remain JSON null: a PID alone does not prove termination, and an interrupted
helper with no final result leaves exit/output/termination unknown. `timeout_tree_killed` retains
the helper's existing meaning (including its on-started-error cleanup); it is not an independent
residual-process scan. Original callback compatibility and residual process state still need an
admitted observation.

The private report retains the reviewed candidate identities, actual runtime-copy path/command,
settings path, cwd/TEMP/TMP, and exact runtime EXE/Lua/settings/generated observer-config identities.
Preparation also binds the process helper, bootstrap descriptor/library and tool resolver. Execution
rejects helper/bootstrap drift and copied EXE/Lua drift before creation. The generated config is
retained beside the observer stem; it derives from the frozen JSON with only the existing bootstrap
and attempt-local output/status/checkpoint paths supplied by the reviewed helpers. These private
paths and payloads must not be pasted into public PR/Issue output.

Lua status, checkpoints and observation files remain intact when the process, callback or terminal
checks fail. Host cleanup records session deletion and canonical ROM comparison independently;
a primary exception and cleanup errors both survive in the diagnostic. This is an opt-in host
report, not a new replay ledger, fixture or launch allowance. A failed attempt is preserved and
requires independent disposition; a fresh directory is not permission to retry.

### Reproduce preparation without execution

After loading the current ignored private-input configuration, this API prepares a supplied trace:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import UPSTREAM, prepare_map3_observation_candidate
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path

prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    input_path=Path("local/issue460/diagnostic-input-proposal.json"),
    output_directory=Path("local/issue460/review-candidate"),  # must not exist
    proposed_timeout_seconds=600,  # proposal for review, not a launch allowance
)
```

Run through `uv run python -X utf8` in the owning worktree. The closed input object has
`clock="first-r1-wait-next-frame"`, `provenance="diagnostic-parameters"` and `frames`: 1–36,000
explicit single-button strings (`Up`, `Down`, `Left`, `Right`, `A`, `B`, `C`, or empty for neutral).
No conditions, coordinates, executable expressions or unknown timing values are accepted as input.
The report binds ROM/source/listing/observer/configuration/input identities and retains Unknowns.

The private review trace's reproducible **diagnostic** recipe uses R2
`expectedObservation.records[0].logicalInputTrace` in its complete order and R2b
`static.routeGraph.segments[0].inputs` / `[2].inputs`; these provide logical directions, not observed
timing. Start with 120 neutral frames; give each logical input 2 pressed + 22 neutral frames. After
the last input of each contiguous waypoint group, append 120 neutral, except:

- For `map3-house-exit-zone`, `map3-sarah-classroom`, `map3-astral-zone-introduction` and
  `map3-astral-zone`, append 12 repetitions of 2 C + 118 neutral instead.
- Append no block after the first `map3-entity142` navigation group or `map3-entity142-face`.
  Consume the trace's Left facing input and then its actual C interaction first. After that final
  `map3-entity142` C, append 120 neutral and 24 repetitions of 2 C + 118 neutral. This relocates
  the earlier proposal's premature 12-confirm block and facing wait after interaction while
  preserving all its pulse counts and durations.

Then append 60 C/neutral repetitions for the messenger, segment 0 inputs (2 pressed + 22 neutral
each), 24 C/neutral repetitions for the gate, segment 2 inputs at the same duration, 240 neutral,
2 Up, and 120 neutral. This separate proposal still has 23,234 frames, a proposed 600-second host
timeout and a 28,634-frame watchdog (input plus retained bootstrap/controlled-prefix margins).
Neither limit is an allowance or a reset of historical consumption. All pulse counts, waits and
reach remain **Unknown**; moving the blocks after interaction does not establish that they finish
before control resumes or avoid FieldMenu. No block may be rescheduled from live state.

**Confirmed input-construction defect, not an original-game observation:** the retained Issue #456
candidate-06 expands each contiguous waypoint group, placing 12 C pulses at frame ordinals
6769–8208 before the trace's entity142 Left-facing step and actual C interaction at `(55,17)`.
The complete trace contains a navigation group, a separate facing row and the final interaction
under the repeated `map3-entity142` name. The new proposal only reorders frames 6769–9816 to
preserve this logical order; old candidate/input material remains unchanged. Source
`code/gameflow/exploration/explorationfunctions_0.asm:GetActivatedEntity` uses player facing for
the target block. `explorationvints.asm:ProcessPlayerAction` sends C through entity/area selection,
with FieldMenu as a fallback. Thus premature C is not a harmless generic wait; the actual selected
action and timing are still **Unknown**.

**Confirmed source correction:** the full pinned
`data/maps/entries/map03/mapsetups/scripts_1.asm:cs_51652` through `csc_end` contains six
`nextSingleText` commands between the two guard-action groups. The H2 `programs/cs_51652/operations`
array is a selected control-effect projection, not the complete script. The initial Issue #460
analysis incorrectly inferred no gate dialogue from that subset; that finding is withdrawn.
The gate's 24-confirm block remains unchanged. Source confirmation uses the pinned commit named
above and blob `5e9b260b9e07dd22387a0ab1ab3b6666d1ab05e1`; it does not prove the proposed timing.

Direct acceptance consists of source/H1/ROM binding, materialization and rejection checks,
Python lint/compile, Lua compilation and the clean committed planner. The Issue #463 correction
uses the narrower zero-launch checks below; it does not rerun normal/full/H1 or runtime gates.
Issue #460 directly exercises the opted-in process handoff and error paths with synthetic process
doubles only, including pre-process failure, PID persistence, timeout/failed termination, callback
failure, missing output and independent cleanup. These checks launch no native process and are not
original evidence. Ordinary planner-selected runtime gates remain **NOT RUN**; the separately admitted candidate
diagnostic and its failed result are recorded below. The local
environment needed an explicitly authorized pinned checkout, verified tool copies and one real
bit-perfect H1 build; its kept listing/log precede preparation and are not runtime evidence. Consuming
the accepted shared-tool mechanism requires direct preparation/copy checks, not another H1 build or
normal suite solely because the base changed. Local-copy EXE/Lua bytes must equal the candidate's
reviewed identities. These checks do not establish native startup or original-game compatibility.

Old R2b/replay ordinal-2 timeout **FAIL**, cleanup failure, missing genuine receipts/ledger,
retry/reset prohibition and completed #431/#434 failures remain preserved by the
[audit dossier](map3-battle01-audit.md#first-necessary-original-observation-dossier).
This candidate grants no launch allowance and supplies no natural continuity, full 8D or H4 PASS.

## Single admitted controlled-start diagnostic result

**Confirmed failure, 2026-09-19 project date (2026-09-20 02:07 UTC):** the
[one-run main-gate disposition](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/460#issuecomment-5746916798)
was executed once from accepted commit `4386c0148362e44d8d9256cf183671832f7c10e2`, tree
`3f0aeda35c7715f6e7002baf3f3021b4e78fe7c3`. This was a controlled-R1-start diagnostic, not
scenario acceptance or passive reset/New/load replay. The disposition invoked ADR 0015's accepted
further-work condition after independent acceptance of the source/admission, observer-restoration
and diagnostic-capability corrections. It did not reset historical consumption, grant a two-plus-one
sequence or satisfy the disabled replay's ordinal-3 prerequisite. Its single process allowance is
now exhausted. No retry, repair launch, longer limit or follow-on H3 is authorized.

The existing `run_map3_observation_candidate` API consumed the retained `candidate-proposal-03`
once after an invocation note and read-only identity/process checks. Frozen input SHA-256 was
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`, runner
`C6EDA0CCBD4BBEFE1732883F31B51B0736EF4DA7BC5D7B39CE4C969432509F14`, configuration
`8495B3D379970403AD4672E321E1D87F688903700DF470B07F2B1D027FCDCD07`, and observer
`335148A5D50383CA4152E14C4E18498AE1CAE2FC615E830421DCB29D108DB028`.
The reviewed report's helper/bootstrap, EXE/Lua, pinned source, retained fixtures, H1 listing and ROM
identities matched before invocation. Limits stayed 23,234 frozen input frames, 28,634 watchdog
frames and 600 seconds. No preceding native warmup or old H3 command ran.

**Confirmed observed boundary:** 13 private checkpoints record the first R1 wait and reported
service/scratch restoration at observer frame 354, followed by the first two accepted Left inputs.
At observer frame 499 / input frame 145, `ProcessMapEventType1_Warp` entry `0x25978` reached
`candidate:warp`. Current map was 3; the raw destination-map operand was `255` (`MAP_CURRENT`).
The candidate asserted `warp beyond bounded Map3/19 route` in phase `candidate-route`. The failure
record's expected and actual PC both equal `0x25978`; this was not an unexpected callback address.
It stopped at handler entry, before observing completion of that warp. Messenger, entity142, gate,
F604, north warp and first Map19 control were not reached by this diagnostic. The proposed input
reordering was therefore not exercised at its corrected interaction.

**Confirmed observer/source mismatch:** at that accepted Git object,
`tools/bizhawk/map3_messenger_acceptance_observer.lua:2425` permits only raw map 3 for a prefix
warp, besides its separately guarded raw map 19 branch. Pinned `sf2enums.asm:MAP_CURRENT` defines
255 as reloading the current map. `data/maps/entries/map03/6-warp-events.asm` gives the house stair
warp at `(54,3)` that sentinel and destination `(3,3)`. In
`code/gameflow/exploration/explorationfunctions_2.asm`, `ProcessMapEventType1_Warp` reads
`MAP_EVENT_PARAM_2`, while `ExplorationLoop` branches on byte `-1` to `@MapIndexNotProvided`
instead of replacing `CURRENT_MAP`. The observer rejects this valid source representation at the
first opening warp. This explains the reported assertion; it is not an original route failure or
proof that the warp completed. The executed Python/Lua identities and attempted material remain preserved. The separate
zero-launch correction below changes the opt-in observer, without reclassifying that failure.

**Confirmed process and retained cleanup:** native PID 42464 started once, returned exit code 1,
reported `timed_out=false` and `process_terminated=true`; timeout tree-kill was not used. The host
result is `FAIL` / `started-failure`. Full stdout/stderr remain private. Lua emitted the typed failure,
reported callback count zero, scope/session restoration and output removal. No terminal observation
file remains. The host independently recorded session ROM deletion and unchanged canonical ROM.
A separate post-exit operating-system check found neither that PID nor a process executing from this
attempt's runtime directory alive; no additional process cleanup was needed. Runtime copies, status,
checkpoints and all prior results are retained.

For read-only reproduction of the finding, inspect the retained `candidate-proposal-03/runtime/`
`host-status.json`, `observer.status.txt` and `checkpoints.jsonl`, together with the Issue #460
invocation note and post-exit process check. Compare the exact executed observer and pinned source
sections named above; do not invoke the candidate again or rematerialize it around collision refusal.
The failure reaches the host exit contract and bounds the native cleanup result for this attempt.
It establishes no general callback compatibility, successful gate transition, natural continuity,
full 8D or H4. The intended caller-dependent gate question and later timing/reach remain **Unknown**.
Historical R2b/replay failures and missing genuine receipts remain intact; documentation of this
failed diagnostic does not replenish any allowance.

## MAP_CURRENT correction and bounded warp admission

**Confirmed source/code correction, 2026-09-19 project date:** Issue #463 corrects only the opt-in
candidate warp admission. `MAP_CURRENT` is a raw operand, not map 255. On the no-scroll path
(`MAP_EVENT_PARAM_1=0`), `ProcessMapEventType1_Warp` passes the raw map byte in D0 back to MainLoop;
`ExplorationLoop` compares it with byte -1 and branches to `@MapIndexNotProvided`, retaining
`CURRENT_MAP`. `UpdatePlayerPosFromMapEvent` reads coordinates/facing; it does **not** resolve the
map sentinel. The R2 source builder's comment records this same no-scroll distinction; its
executable behavior is unchanged. A nonzero scroll-mode path writes the map byte directly and
must not receive the same sentinel interpretation.

Preparation binds `sf2enums.asm`, `sf2mapmacros.asm`, the complete Map3 `6-warp-events.asm`, and
`explorationfunctions_2.asm` to their pinned Git contents. Existing map-content encoding/decoding
checks the complete nine-row table at H1 symbol `Map03s6_WarpEvents` (`0x978F0`) against the canonical
ROM, including its terminator. Source guards retain no-scroll dispatch and current-map branch order;
H1/ROM binds the `cmpi.b` at `0x257E4` to the parsed equate, the final ROM BEQ at `0x257E8` to
`0x25828`, and the BNE.W at `0x2597C` to `loc_259CC`. No H1 rebuild is performed.

The candidate reuses R2 `static.route.runtimeOpening.warps` and `navigation.inputPlan`, joining
source position, attempted movement target and raw warp destination separately. The source-bound
prefix is:

| Warp | Player source | Movement target | Raw map | Effective map | Warp destination / facing |
| --- | --- | --- | --- | --- | --- |
| House stairs down | `(55,3)` | `(54,3)` | 255 | 3 | `(3,3)` / Right |
| School stairs down | `(45,7)` | `(46,7)` | 255 | 3 | `(59,12)` / Left |
| School stairs up | `(58,13)` | `(59,12)` | 255 | 3 | `(46,7)` / Down |
| North castle entrance | `(28,2)` | `(28,1)` | 19 | 19 | `(26,30)` / Up |

These are static admission constraints, not newly observed transitions. School stairs up uses R2's
source-derived diagonal target. North uses the R2b navigation/warp segment and retained source row
whose trigger X is wildcard 255; the bounded route still requires actual target `(28,1)`. That trigger
wildcard is distinct from the map operand sentinel. The north callback retains original gate-return
and F604 requirements, checks no-scroll/raw map/destination/facing, and rejects subsequent warps.
The later original Map19 init/program/wait/input checks remain unchanged.

The Lua checkpoint preserves all five raw operands, observed current map, player source and movement
target, plus `effectiveDestinationMap`. Only no-scroll `MAP_CURRENT` uses current map for this
read-only calculation. Admission also requires the exact source row: literal map 3 cannot substitute
for raw 255 in the three prefix rows; raw 255 cannot substitute for north's explicit map 19. Wrong
map, scroll mode, coordinates, destination, facing, gate state, and later/out-of-scope warps fail.
The callback never assigns game state, simulates a warp, or selects input from live state.

### Zero-launch verification and stopping condition

Load `local/private-inputs.ps1` in the owning worktree and run `uv run sf2 rom verify`, then the
preparation API shown above with the unchanged `local/issue460/candidate-proposal-03/input.json`
and a fresh ignored output directory. Input SHA-256 remains
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`; all 23,234 frames,
600-second proposed limit and 28,634-frame watchdog are unchanged. The preparation report binds
exact source/listing/runner/observer/configuration identities. It remains
`CANDIDATE-PREPARED-NOT-ADMITTED` with no `runtime` directory.

Direct checks retained under `local/issue463/` use `uv run python -X utf8 local/issue463/direct.py`
(with a fresh output name on reproduction). They compile Lua without running the observer, then
exercise only the extracted admission callback in an in-process Lua library using immutable sample
reads: the three current-map rows and explicit north destination pass; malformed/out-of-scope
variants fail. In-memory source branch, ROM operand/branch/table, and retained-map mutations must
reject construction. These are checks of admission constraints, not original runtime observations;
no tracked validation-tool tests or new parser are added. Python lint/compile, document links/scope,
`git diff --check`, clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
public CI complete the preparation review. Exact identities and check outcomes belong in the frozen
Draft PR/Issue handoff; private payloads stay local.

**Unknown:** actual handler completion, later fixed-input reach/timing, caller-dependent gate/F604
transition, first Map19 control and general callback compatibility remain unobserved by this
correction. Selected runtime gates are **NOT RUN**. The frame499/input145/PID42464 diagnostic remains
**FAIL**, with its cleanup record intact. #460's permission is exhausted; no native/no-ROM startup,
candidate execution, old H3 or replay preflight is authorized. Normal/full/H1 results and prior
failures remain preserved without rerun. Independent main-gate review of this frozen correction
precedes any decision on further work; code/materialization PASS never triggers a retry or closes #437.

## Corrected candidate single-run result

**Confirmed failure, 2026-09-19 project date (2026-09-20 02:42 UTC):**
[Issue #465's exact main-gate disposition](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/465)
admitted only the retained corrected candidate once, from accepted commit
`07dce82ac1f157e626521549aa30b62c0a064165`, tree `dff51195c746619ba4953c73a2dc71a255367399`.
The accepted MAP_CURRENT correction above was the sole capability change from the failed #460
attempt. Under ADR 0015's further-work condition this was the **second actual native process start
of the controlled-observation method**. It consumed #465's entire permission; no third controlled
start or frozen acceptance is reserved. The disabled replay's separate ordinal 1/2 consumption,
ordinal-2 timeout/cleanup **FAIL**, missing genuine receipts/ledger, ordinal-3 prerequisites and
no nominal reset/fourth-attempt rule remain unchanged. PID 42464 / frame 499 / input 145 / exit 1
from #460 remains **FAIL**, with all original artifacts and cleanup records preserved.

The retained `candidate-02` preparation used the same 23,234 input frames as #460, a 28,634-frame
watchdog and a 600-second hard timeout. Before the one `run_map3_observation_candidate` call,
read-only checks verified the accepted Git tree, no competing writer/emulator, absent runtime,
canonical ROM, complete retained source/helper/bootstrap/tool/fixture/listing identities and limits.
The ignored invocation note was written first. No material was regenerated, warmed up or repaired;
no old H3/replay command or alternative input was run. Frozen SHA-256 identities were:

| Material | SHA-256 |
| --- | --- |
| Input | `37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145` |
| Python runner | `BC0A62798DDDC4128C719E35A3C504F08FD9D634B64F9AE682614980872BAEB5` |
| Lua observer | `264C75201CCE8C4A1DAB1F2D63057395E0359EE51C7A4CAEB1C5C8327E23679D` |
| Configuration | `30E80814C4A6D240484E515253BFD98835112E2ED2976422C29AC478374B5682` |

**Confirmed bounded observations:** the 72 retained checkpoints include the original first warp
handler at `0x25978`, frame 499 / input 145: no-scroll raw map 255, observed current/effective map 3,
source `(55,3)`, movement target `(54,3)`, destination `(3,3)` / Right. Unlike the old failed attempt,
execution continued to the original controller read at frame 643 / input 289 with Map 3 `(3,3)`,
Right and no pending map event. This establishes post-warp control for this house transition;
the effective-map calculation alone would not establish it. The same frame records movement
acceptance and zone dispatch toward `(4,4)`, followed by `cs_5145C` (`0x5145C`) entry and its
stack-matched return at frame 1167 / input 813. These are reached callbacks, distinct from the
static school/north warp expectations above; neither school warp was reached.

**Confirmed failure boundary:** the original `FieldMenu` entry at `0x2127E` triggered
`candidate:unexpected-field-menu` in phase `candidate-route`, frame **1749 / input 1395**.
The observer's expected and actual PC agree. The last checkpoint before the failure record is
`field-menu:reached`; readback is Map 3 `(4,4)`, F601 set, F603/F604 clear, map-event word 0,
typewriting 0 and zero tracked pending returns. The frozen house-exit acknowledgement block had
supplied C at input frames 1393–1394; the original controller read at 1393 records that C, and
1395 is already neutral. Thus this fixed trace actually reaches the forbidden menu after the
opening script returns. It does not establish that the original route is impossible or that
any particular alternate timing would succeed. No menu choice/return/effect, corrected entity142
interaction, messenger/R2a boundary, gate/F604, north warp or Map19 terminal was observed.

**Confirmed process and cleanup:** native PID **26448** started once and exited **1**, with
`timed_out=false`, `process_terminated=true` and `timeout_tree_killed=false`. Host status is
`FAIL` / `started-failure`. Lua reports callback count zero, output removal and every recorded
scope/session restoration flag true. The host records session ROM deletion and unchanged canonical
ROM. An independent post-exit OS scan found neither the owned PID nor a process attributable to
this runtime copy; no extra process/tree termination was needed. Runtime copies, generated config,
checkpoints, typed status and full stdout/stderr remain private and retained. No terminal observation
exists. The API interval 02:42:04.211612–02:42:40.001038 UTC includes preparation/cleanup and is not
an exact native-runtime measurement.

Read-only reproduction uses `local/issue465/invocation-note.json`, `result-review.py`,
`result-summary.json` and `process-survival.json`, with the retained
`local/issue463/candidate-02/runtime/host-status.json`, `observer.status.txt` and `checkpoints.jsonl`.
Load the current ignored private-input configuration, then run
`uv run python -P -X utf8 local/issue465/result-review.py` with a fresh summary output name.
Inspect the named accepted Git object and unchanged input for the source
expectations and callback checks; do not invoke the candidate again. Direct result, material,
document/scope checks and the committed planner validate this report, not successful gameplay.

**Unknown:** later fixed-input timing/reach, the original gate/F604/north transition, first Map19
control, general callback compatibility and the continuous milestone remain open. No public golden,
full 8D or H4 result is promoted. Normal/full/H1/H2/H3 queues and new tests are excluded from this
result-only scope. The diagnostic stops here regardless of further hypotheses; independent
main-gate review of this result does not authorize another launch or complete #437.

## Fixed acknowledgement schedule: source and retained-trace review

**Confirmed, zero-launch review (Issue #469):** the frozen house block treats C as a generic
acknowledgement for longer than the opening dialogue lasts. C is also an exploration action.
The completed #465 failure therefore contradicts that scheduling assumption, not the original
route's legality. This section consumes the result above and the complete pinned source; it changes
no input, observer, fixture, golden or launch permission.

### Opening dialogue, return and subsequent actions

**Confirmed source:** `scripts_1.asm:cs_5145C` first waits for entity 128's initialization and
movement, then executes `textCursor 510`, two `nextText` commands (510, 511), `textCursor 483`,
one `nextSingleText` (483), a final `setActscriptWait 128,eas_Init`, and `csc_end`.
The pinned `gamescript.txt` gives 510/511 one `{W2}` each and 483 one `{W1}`. These are three
text commands, not twelve acknowledgement requirements. The complete script has no Yes/No prompt.

`sf2cutscenemacros.asm` maps `nextText` to command 2 and `nextSingleText` to command 0.
`mapscriptengine_2.asm:csc02_displayTextbox` calls `DisplayText` and increments the cutscene
cursor; `csc00_displaySingleTextbox` additionally closes portrait/dialogue windows and sleeps
10 ticks before returning. `textfunctions_1.asm` implements `{W2}` at
`ParseSpecialTextSymbol:@wait2/loc_6472` and `{W1}` at `symbol_wait1/loc_659C/loc_65B4`.
Both wait for ordinary directional/A/B/C input through `CURRENT_PLAYER_INPUT`; both temporarily
clear `CURRENTLY_TYPEWRITING` while waiting. Thus typewriting 0 alone does not mean input-ready.
`HandleDialogueTypewriting` in `textfunctions_2.asm` can also shorten character delay when
`PLAYER_1_INPUT` is nonzero; a supplied pulse is not necessarily a completed acknowledgement.

**Confirmed retained observations:** all ordinals below are the existing candidate's one-based
input clock, with observer frame = input + 354. Entry/return pairs are the observer's original-PC,
stack-matched callbacks, not reconstructed timings.

| Input ordinal | Observed boundary |
| --- | --- |
| 289 | `ExecuteMapScript` entry for `cs_5145C`; zone target `(4,4)` |
| 339–553; 557–673 | `DisplayText` entry/return for 510, then 511; returns coincide with C pulses |
| 677; 790; 793 | Text 483 entry; `symbol_wait1`; actual `loc_65B4` acknowledgement read `0x20` (C), then text return |
| 793–802; 813 | Single-text dialogue close; script return at `0x58C`; the zone wrapper's extra close also enters/returns at 813 |
| 913; 916–1033; 1042 | Controller reads C; a new trap-based text 483 enters/returns at `0x6260`/`0x574`; its dialogue close returns |
| 1153; 1156–1273; 1282 | Controller reads C; another trap-based text 483 and dialogue close complete |
| 1393; 1395 | Controller reads C; `FieldMenu` entry `0x2127E` fails the candidate, although the input table is neutral by 1395 |

The observer instruments `{W1}`'s read, not `{W2}`'s loop. The first two text returns and concurrent
C pulses are **Confirmed**; their exact internal `{W2}` read times are **Unknown**. The later
`text:wait1` records carry cutscene cursor 484, but the paired `DisplayText` target is 483:
trap-based `txt` does not set that cutscene cursor. Those records do not establish text 484.

**Confirmed source:** after `cs_5145C`, `s3_zoneevents.asm:Map3_ZoneEvent6` calls
`MakeEntityWalk` for selector 128 with `(5,6,1)`, sets F601 and returns. `MakeEntityWalk` and
`entityfunctions_2.asm:SetWalkingActscript` install walking work without waiting for its completion.
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` closes windows, waits a VInt and waits for the
player to stop before returning to exploration. Script return is therefore not the whole caller's
return or a proof of completed NPC movement. The observed F601 readback changes after script
return; the first retained subsequent nonzero controller read is 913. The exact first ready frame
between those boundaries is **Unknown**, since neutral controller reads are not logged.

`explorationfunctions_2.asm:WaitForEvent/loc_2593C` tests A/C after pending map events;
`ExplorationLoop/loc_2587E` dispatches `ProcessPlayerAction`. In `explorationvints.asm`, that
function saves `PLAYER_1_INPUT` in D7 before waiting for player/view movement. A goes to
`loc_25BCC`; ordinary C goes through `GetActivatedEntity`, entity-event dispatch, then
`CheckArea` if no entity was selected. If neither yields an action, control falls through to `j_FieldMenu`
(`s05_jumpinterface.asm` resolves it to `FieldMenu`). Releasing C after dispatch need not cancel
the saved action. `esc02_controlCharacter` at `0x4FF8` separately chooses `PLAYER_1_INPUT` when
D7 is nonzero, matching the observed D7=48/value=32 reads; it is not itself a menu-entry callback.

**Inferred:** the two post-opening displays are re-interactions with entity 128: the F602-clear
`s2_entityevents.asm:Map3_EntityEvent2` displays 483, matching the two trap-based calls, player
position/facing and the source's departing NPC. The final C's ordinary entity/area fallback
explains the later menu entry. **Unknown:** the exact selected entity/negative lookup result,
NPC position and area-check result at each action were not recorded by this candidate. Do not
promote the plausible explanation that the NPC moved out of interaction range to an observed fact.
The confirmed repeated text and menu callbacks already establish that these extra pulses were
not harmless opening-dialogue acknowledgements.

### Later blocks: complete source, no later runtime claim

**Confirmed construction/source; Unknown reach and timing:** direct expansion of the documented
recipe equals all 23,234 frozen input frames. Each listed block repeats 2 C + 118 neutral; ranges
include its final neutral tail. Later rows were never reached by #465. They share the same risk
whenever C outlasts the intended consumer; source command counts cannot prove which frame crosses
that boundary or justify replacing the pulse count with a text count.

| Block / input range / pulse count | Complete owning flow and acknowledgement boundary |
| --- | --- |
| House / 313–1752 / 12 | `cs_5145C`: 510, 511, 483 as above. Pulses start at 313 + 120k, k=0..11; seven start after the observed script return. |
| Sarah classroom / 3433–4872 / 12 | Intended first `Map3_EntityEvent0`, F602/F603/F256 clear: `txt` 512 (`W2`), 480 (no explicit W token), 481 (`W1`), then complete movement-only `cs_513D6` and F256. Later interactions take the flag-dependent branch; the entity-event wrapper closes windows. |
| Astral introduction / 5161–6600 / 12 | `Map3_ZoneEvent7`, F602/F603 clear: portrait and `txt` 513 (`W1`), then return and wrapper close. No twelve-acknowledgement loop. |
| Entity 142 / 6937–9816 / 24 | After the frozen Left-facing step and actual C interaction: `Map3_EntityEvent15` conditionally displays 500 (`W2`) and sets F261, then 501 (`W1`) and F602. Repeating C can select later interactions; F602-related re-init `cs_513A0` only positions Sarah, with no text. |
| Astral zone / 9985–11424 / 12 | `Map3_ZoneEvent7`, F602 set and F603/F260 clear: 514 (`W2`), 515 (`W2`), 516 (`W1`), then complete positioning-only `cs_5148C`, F260 and wrapper close. |
| Messenger / 11881–19080 / 60 | Complete `cs_5149A` through its branches: accepted Yes path uses 517–531, 535–536, a real Yes/No prompt/F89 branch, plus `csc08_joinForce` text 447 and `FadeOut_WaitForP1Input`. The decline path uses 532–533 instead of the accepted join ending. Entity movement/waits separate text commands; the caller sets F603 after script return. |
| Gate / 19825–22704 / 24 | Complete `cs_51652`: six `nextSingleText` commands, 537–542 (each `W1`), between two guard-action groups; entity 139 is awaited, 138 is not. `Map3_ZoneEvent4` commits F604 after script return, then the zone wrapper returns. |

The messenger block cannot be treated as text acknowledgements alone: `YesNoPrompt` initializes
choice zero, may wait for release of entry input, handles left/right selection and C/A confirmation,
and treats B as No. Join text 447 has no explicit W token, but its caller separately waits for
player input. Replacing every C with B or assigning one pulse per displayed command is therefore
not a source-supported correction for the entire route. The gate's six real text commands remain
required; the withdrawn no-dialogue assertion is not revived. None of these static branches proves
the proposed later arrival state, completion frame, selected prompt result or absence of extra C
actions in the unshimmed candidate.

### Bounded correction proposal and reproduction

**Inferred correction proposal, not executed:** preserve the house prefix through the final
observed acknowledgement at 793–794 and replace only the seven remaining house C pairs
(913–914, 1033–1034, 1153–1154, 1273–1274, 1393–1394, 1513–1514, 1633–1634) with neutral frames.
Keep the block length and subsequent ordinals unchanged. This removes the demonstrated extra
actions at their cause; neutralizing only 1393 would retain the two unintended conversations and
later surplus pulses. It adds no adaptive input or observer behavior. This is a concrete candidate
correction for independent review, not an established successful replacement: altered interaction
and RNG/NPC evolution can change later state, and the downstream blocks have no measured safe
cutoff. No complete fixed-frame route replacement can be established from the retained failure.

Reproduce the static review against `ShiningForceCentral/SF2DISASM` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`, with paths relative to `disasm/`:

- `data/maps/entries/map03/mapsetups/{scripts_1,s2_entityevents,s3_zoneevents,s6_initfunction}.asm`
  supplies the complete programs/callers above; `scripts_1.asm` blob is
  `5e9b260b9e07dd22387a0ab1ab3b6666d1ab05e1`.
- `data/scripting/text/gamescript.txt` blob `d1f5c1fa20ff2a2d442408d71d2dcfbffc2cb7bd`
  supplies hex-indexed text control tokens. Retain IDs/tokens only, not dialogue prose.
- `code/common/scripting/map/{mapscriptengine_2,mapsetupsfunctions_1}.asm`,
  `code/common/scripting/text/textfunctions_{1,2}.asm`,
  `code/common/scripting/entity/{entityscriptengine_2,entityfunctions_2}.asm`, and
  `code/gameflow/exploration/{explorationfunctions_0,explorationfunctions_2,explorationvints}.asm`
  supply the named consumers, return order, action dispatch and input masks.
- `sf2cutscenemacros.asm`, `code/common/tech/interrupts/trap5_textbox.asm`,
  `code/common/menus/yesnoprompt.asm`, and
  `code/gameflow/battle/battlefunctions/battlefunctions_0.asm:FadeOut_WaitForP1Input`
  resolve macro/trap/prompt/join behavior; the latter calls `code/common/tech/input.asm:WaitForPlayerInput`.

Use `git -C local/upstream/SF2DISASM show <pinned-commit>:disasm/<path>` for these source sections.
Read the unchanged `local/issue463/candidate-02/input.json` (identity in the preceding result) and
its `runtime/checkpoints.jsonl`, `host-status.json`, `observer.status.txt`; filter checkpoint kinds
`script:*`, `DisplayText:*`, `CloseDialogueWindow:*`, `text:*`, `input:original-controller-read`
and `field-menu:reached`. The read-only local `local/issue469/audit.py` compares the reviewed source
files to pinned Git objects, expands the existing recipe in memory and reports only IDs/control
facts from the retained 72 checkpoints. Run with
`.venv/Scripts/python.exe -P -X utf8 local/issue469/audit.py`; it creates no candidate or runtime.
The [observer](../../tools/bizhawk/map3_messenger_acceptance_observer.lua)'s `install_candidate`
owns the recorded fields and callback limitations; H2 projections are not substitutes for these
complete scripts.

Acceptance is direct source/trace, document/link/scope and `git diff --check` review, then the
clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual public CI.
The planner's generic `public-core` selection does not authorize local normal/full/H1/H2/H3,
warmups, materialization or tests of verification programs for this documentation-only slice.
Both controlled starts remain consumed, both completed failures and the separate disabled replay
ordinal-1/2 restrictions remain preserved. This proposal grants no third start, runtime permission,
natural-continuity claim, full 8D/H4 acceptance or completion of #437.

## Prepared house-neutral correction (not executed)

**Confirmed preparation only, 2026-09-19 project date:**
[Issue #471](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/471) materializes the
[bounded proposal above](#bounded-correction-proposal-and-reproduction) from accepted commit
`ddd17b2442455ac4ab9864418ed2f30f4837fda6`, tree `d7e8d6b06f7f9537e6d72e55388dcccc328c771c`.
The original `local/issue463/candidate-02/input.json` remains unchanged at SHA-256
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`.
Only one-based ordinals **913–914, 1033–1034, 1153–1154, 1273–1274, 1393–1394,
1513–1514 and 1633–1634** change from C to neutral (empty string) in the new private copy.
Direct comparison confirms exactly 14 changed entries, identical remaining frames and metadata,
23,234 total frames and no insertion, deletion or ordinal shift. The prefix through 793–794,
the remaining house tail and every later block retain their original positions and contents.

The fresh input is `local/issue471/house-neutral-input.json`; the prepared output is
`local/issue471/candidate-01/`. The existing `prepare_map3_observation_candidate` completed its
first and only call for this Issue with `CANDIDATE-PREPARED-NOT-ADMITTED`, `EmulatorLaunches=0`,
23,234 input frames, proposed timeout 600 seconds and watchdog 28,634 frames. Its source/H1/ROM
binding and Lua syntax checks passed without building H1 or starting an emulator. The ROM's narrow
identity verifier also passed. The candidate has no `runtime/` directory; an OS process check found
zero EmuHawk processes. No launch API, runtime materialization or no-ROM warmup was invoked.

| Prepared material | SHA-256 |
| --- | --- |
| New input (standalone and candidate copy) | `4BA1E4738877A72866BCC0B6A644F75479FB5A9E21CC761E7D99FEC24403A76A` |
| `candidate.json` | `B7D8D6116490DD5A1DB122F23275603273695A66B32D2C0CA30C7211A63D28BF` |
| `config.json` | `47D7A3C287B3DEB0ABDF429DFA0C782BB8873826A4CBAC5310F5B3014B89A405` |
| `config.lua` | `19BFFE2BB3AC0B1D90698436E283A70714A97CFAD86077ADF477C1670A775A3A` |
| Current `src/sf2tool/h3/bizhawk.py` | `832966617D63CA97B341C09F684F0E7021B16CAF4578A4D27B77F82AD8779B64` |
| Shared BizHawk executable | `F8CDB93551A544F680BF3876D9D8D72643859E7A44A23B04E1A25B92E48F80CD` |
| Shared Lua library | `4786E0DF4CAF120E3BEDF0B6DDA260525DF2187C66DED220A21A53ACE76B0501` |

The report binds USA ROM `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`,
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, listing
`FA21225556FC916ED287A42E8D2451A15326B4FBF02816DDE7D82C6D4354E982`, every selected source,
retained fixture and execution helper. Runner and observer identities equal those in the
[completed #465 result](#corrected-candidate-single-run-result). The current helper includes the
accepted default mute change; no existing runtime configuration was changed. A parsed comparison
with the retained candidate-02 configuration finds only the 14 frames, their input identity and
fresh output/checkpoint/status paths changed. All other observation configuration is identical;
no adaptive input, callback semantics or downstream schedule was added.

Reproduce the input transformation below only into a fresh ignored destination. The original has
CRLF line endings; the new copy uses the same observed serialization. Neither parsing nor
comparison rewrites the original. The accepted source rationale remains the fixed-schedule review
above; these assertions verify the transformation, not successful gameplay.

```python
import json
from copy import deepcopy
from hashlib import sha256
from pathlib import Path

original = Path("local/issue463/candidate-02/input.json").read_bytes()
assert sha256(original).hexdigest().upper() == (
    "37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145"
)
old = json.loads(original)
new = deepcopy(old)
changed = [n for start in (913, 1033, 1153, 1273, 1393, 1513, 1633)
           for n in (start, start + 1)]
for n in changed:
    assert old["frames"][n - 1] == "C"
    new["frames"][n - 1] = ""
assert len(old["frames"]) == len(new["frames"]) == 23234
assert {k: v for k, v in old.items() if k != "frames"} == {
    k: v for k, v in new.items() if k != "frames"
}
assert [i + 1 for i, (a, b) in enumerate(zip(old["frames"], new["frames"], strict=True))
        if a != b] == changed
new_bytes = (json.dumps(new, indent=2) + "\n").replace("\n", "\r\n").encode("utf-8")
assert sha256(new_bytes).hexdigest().upper() == (
    "4BA1E4738877A72866BCC0B6A644F75479FB5A9E21CC761E7D99FEC24403A76A"
)
with Path("local/<fresh-review>/house-neutral-input.json").open("xb") as stream:
    stream.write(new_bytes)
```

Load `. ./local/private-inputs.ps1` in the launching PowerShell process, run `uv run sf2 rom verify`,
then use `uv run python -P -X utf8` and the existing
[preparation API example](#reproduce-preparation-without-execution), substituting that new input,
an absent `local/<fresh-review>/candidate-01` output and `proposed_timeout_seconds=600`.
The report/config identities above name the retained Issue #471 destination; another destination
changes generated path-bearing configuration and its identity. Read the retained candidate for
review instead of overwriting or regenerating it.

The actual invocation and PASS output are retained as
`local/issue471/preparation-command-02.txt` and `preparation-output-02.txt`, with
`preparation-summary.json`, `rom-verify.txt` and `process-check-02.json`. The initial
`preparation-command.txt` / `preparation-output.txt` and `failure-state.json` preserve a completed
local inspection **FAIL**: an extra assertion required original bytes to equal LF-only JSON
serialization despite the original's CRLF. It failed before any new input or preparation API call.
Main-gate independently checked that boundary and authorized removal of this extra assertion in
the same task; the original was never normalized or written back. This was not a preparation API,
ROM, toolchain or game failure, and no native attempt was retried.

**Unknown:** downstream arrival, timing, NPC/RNG evolution, prompt choices, later C consumers,
gate/F604/north/Map19 reach, callback/cleanup compatibility and natural continuity remain unobserved.
The transformation removes demonstrated surplus actions from the input; it proves no successful
route or original-game outcome. Full 8D/H4 and #437 remain incomplete.

Acceptance is direct input/source/preparation and document/link/scope/whitespace checks, followed
by the clean committed planner and actual public CI. Local normal/full/H1/H2/H3 queues and tests
of verification programs are **NOT RUN**, outside this preparation-only scope. The completed
#460 and #465 **FAIL** results and original artifacts remain preserved. Controlled-method actual
starts remain **2 with no remaining permission**; disabled replay ordinal-1/2 consumption,
timeout/cleanup failure, missing genuine receipts, ordinal-3 prerequisites and retry/reset/fourth-
launch prohibitions remain separate and unchanged. This Issue, new identity, preparation PASS or
independent PR review grants no third controlled start or frozen acceptance. Stop at the Draft PR
for independent main-gate review; there is no launch, merge or cleanup authorization.
