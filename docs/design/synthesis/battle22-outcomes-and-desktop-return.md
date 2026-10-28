# Chessboard Outcomes and the Return to Desktop Kingdom

**Confirmed:** defeating the King can end Battle22 through the leader-cleanup path;
ordinary enemy counting is a separate outcome check. Victory completes the battle and
returns to a rebuilt Desktop population. A separately gated scene announces a Cotton
Balloon, without granting or using it. **Inferred:** Ponpei presents this as the reward
for winning his chess challenge. **Unknown:** natural victory, the supplied state,
completed services, the visible scene and usable control.

This English guide follows [Chessboard initialization and initial roster](battle22-initialization-and-roster.md).
It consumes [accepted Battle22 outcome research](../../research/battle22-outcomes-and-desktop-return.md)
and the [battle lifecycle contract](../contracts/battle-control-lifecycle.md), under
[Issue #915](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/915) and the
[current Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It stops at conditional local exploration/control or a pending return, plus the changed
King/Maid conversations. [Cotton Balloon acquisition and Floor World departure](cotton-balloon-through-floor-world-entry.md)
continues with the separate chest/guard/item-use joins through conditional Map26 loader
admission, before destination population/init. Fnnn names flag nnn; row and command indices are zero-based. Coordinates
are source operands, not observed movement.

## How the Encounter Ends

**Confirmed:** the defeated hook first requires living Bowie and enemy 128 with HP 0.
In the fresh roster, enemy 128 is the chess King. Battle22's leader-presence entry is -1.
With F522 clear the hook executes `ms_Empty`; with F522 set it skips that script. Both
eligible paths reach the leader tail, which appends every nonzero-HP enemy in 128–159 to
the killed list. It checks neither placement X nor a new damage result, and does not
clear those enemies' HP itself.

ProcessKilledCombatants then performs its presentation/bookkeeping and writes listed
X/Y=-1, clears status, updates stats and moves mapped actors off-map. Remaining counts
require nonnegative X and nonzero HP, so this cleanup can remove living chess pieces
from counting without individually reducing them to HP 0. Fresh Ponpei/Maid neutral
slots 159/158 already have HP 0 and are not appended. Supplied state may differ.

The ordinary count scans thirty allies and thirty-two enemies. Bowie HP 0 forces the
ally count to zero. Both outcome checks test zero allies first: simultaneous elimination
is defeat. Bowie dead also bypasses the defeated hook. With coherent state and surviving
Bowie, King death through that hook can finish the encounter; zero counted enemies is
also sufficient at an ordinary count boundary. This is not an all-enemies-only objective.

The order matters: individual turn → defeated hook → killed processing → count. If both
factions remain, after-turn effects → killed processing → count follows. That second
check has no defeated hook: King HP becoming zero there does not itself invoke leader
cleanup. The debug kill-all caller is separate evidence. Battle22 has no region-cutscene
row or after-leader positioning override; do not import Battle21's scene or Battle05's
position changes.

## Victory Completes the Battle Before Desktop Init

**Confirmed:** first and repeat victory share this order:

1. Heal living allies, plus Peter 7 and Lemon 28 even when dead, to maximum HP/MP.
   Retain only existing STUN/POISON/CURSE bits and update stats; other dead allies stay dead.
2. Rebuild joined/active force lists in ascending ally ID order. The first active member
   supplies the return position and its mapped battle actor supplies facing.
3. Run the after wrapper. F522 clear dispatches `ms_Empty`; F522 set skips it.
   Both paths reach the common JoinForce(0) tail for Bowie.
4. Clear F422, set F522, read the return parameters and return d4=1.

The selected join table is the **cutscenes** table included by the layout, not its similarly
named global counterpart. Bowie joining is not a new Eric/Karna recruit. JoinForce sets
his joined flag and rebuilds the lists; it activates him only when the active count is
below twelve. It does not make room in a full party, and that activation does not immediately
rebuild the list/count again. The seed was already chosen, so this join cannot repair it.

An empty script still takes ExecuteMapScript's terminator tail, including a conditional
dialogue-scroll wait, clearing scrolling speed, register restoration and return. Empty
does not guarantee immediate completion. Every later write depends on earlier services
returning; an unfinished prefix has no automatic rollback.

### The Returned Position Is Not the Scene Placement

**Confirmed:** the victory seed is CURRENT_MAP plus the first active member's combatant
X/Y added to BATTLE_AREA_X/Y using byte arithmetic, and that member's mapped actor facing.
For the accepted Map22 area `(9,0)`, local `(x,y)` becomes full-map `(x+9,y)` modulo that
arithmetic. It is not the initial placement row or the earlier `(0,0,Right)` admission warp.
The first-member helper does not search for a living member or repair an empty list.
Changed membership, position, lookup or saved area state can change the tuple.

Battle22's empty after script does not replace this seed. BattleLoop reads PARAM2–5
directly, rather than waiting for a new warp event, and writes no new MAP_EVENT_TYPE here.
MainLoop resumes at its alternate entry, performs SwitchMap and enters exploration,
**bypassing CheckBattle**. The flag-switched-map table has no Map22 row.

F422 admission, F472 intro, F522 completion and F88 suspension remain independent. Victory
clears 422/sets 522 without writing 472/88. F88-set entry consumes current/saved state without
fresh roster/hooks; these outcome claims still require actual CURRENT_BATTLE=22. F472 is
not an outcome or Desktop-init gate. Suspension is not victory or a completion write.

## Desktop Is Rebuilt With Different Identities

**Confirmed:** a normal explicit Map22 exploration return clears MAP_EVENT_TYPE, heals
eligible allies again, fades out, disables view following and sets CURRENT_MAP=22 and
CURRENT_BATTLE=255. It replaces entities/lookups, loads mapsprites, clears temporary
flags 256–383, sets F80, loads the full map, installs VInts and runs init. Battle actors
and their actions/lookups are gone; the board is no longer cropped. A supplied map 255
uses the existing-population arm instead, without that fresh selection/reset/assignment.

F522 selects the alternate population, independently of F776/F778. Both setups share
events, zones, descriptions, Section5 and init. The alternate has **fourteen rows**:
Eric/Karna, Caravan, young man/woman, old man, priest, Maid/Ponpei, two guards and three
fixed OBJECT1 placeholders at `(63,63)`. The
[complete population table](../../research/battle22-outcomes-and-desktop-return.md#f522-population-and-layout)
owns all positions, facings, sprites and actions. Eric and the old man use walking actions;
the other visible rows use init actions. Their listed positions do not predict later motion.

Ally rows do not test joined/active membership, so seeing Eric/Karna does not recruit them.
An existing nonzero ally lookup can skip an entire row without consuming a physical slot;
the current follower table contains neither, so normal fresh selection does not supply
that skip. Ordinary NPC lookups are appended separately. With only F67/Peter and F68/B
followers, physical 1/2 are those followers; rows 0–13 occupy physical 3–16. Maid 133/Ponpei 134
are physical 10/11 in that example. NPC 128 is again Caravan, not the chess combatant King 128.
The battle neutrals' state 159/158 and B31/A30 script keys are also distinct identities.

The sixteen chess rows and old ORB153–155 rows are not retained. Shared event entries 137–152
still exist; placeholders 137–139 reuse some of those logical IDs. Their off-map placement
does not prove natural interaction, nor does it remove their event callbacks.

### Followers Are Supplied Separately

**Confirmed:** Map22 is not an overworld map, so F65 does not add Caravan/A here. F522
does not create Peter or Oddler; battle membership is not follower admission. The ordinary
F67/F68 example declares Peter/eas_Follower1 and B/eas_Follower2 at the return seed. There
is no fixed Peter fallback. Other follower flags can change allocation and overwrite
B/A/D lookup aliases while earlier physical actors remain.

Fresh ClearEntities zeros lookups. An absent living Peter or absent B can therefore resolve
to physical 0, rather than a safe no-op. Current HP/class selects ally sprites, including
BLUE_FLAME for HP 0 outside battle. Existing mode/raft handling also remains: F64 with
RAFT_MAP=22 declares physical 31 at saved coordinates; mode 2 instead changes main to RAFT,
parks followers and declares 31 at `(64,64)`. The ordinary two-follower population ends at 16,
but extra follower allocations can reach 31 and be overwritten while aliases remain.
No selected outcome or scene boards a raft.

### A Chest Layout Change Does Not Give an Item

**Confirmed:** the full load applies two F522 copies on the 64-wide working layout:
1×1 from `(39,0)` to `(25,10)`, then 1×1 from `(40,0)` to `(25,40)`. Source comments
identify chest-for-chessboard and blank-for-blank respectively. Chest overlays follow;
an already set chest flag can subsequently apply the opened-chest word `$D802`.
The chest row names `(25,10)`, F212 and COTTON_BALLOON. F522 does not set F212.
These records establish neither inventory custody nor a decoded collision/rendering route.
The [acquisition guide](cotton-balloon-through-floor-world-entry.md#finding-the-gift-is-separate-from-receiving-it)
explains the subsequent search, pre-delivery F212 write and full-party/repeat/interruption paths.

## Which Scene Runs on Return

**Confirmed:** Map22 init first tests F776. Clear runs the complete arrival scene, then
sets 776 only after return. It independently tests F522 and F778: completed battle plus
F778 clear runs `cs_5996E`, then sets 778 only after return.

| Supplied gates | Conditional result |
| --- | --- |
| F776 set, F522 clear | Neither scene, regardless of F778. |
| F776 clear, F522 clear | Arrival, then 776; no victory scene. |
| F776 set, F522 set, F778 clear | Victory scene, then 778. |
| F776 set, F522 set, F778 set | Neither scene; alternate population/layout still selected. |
| F776 clear, F522 set | Arrival first, then 776; victory scene follows only with F778 clear and completed arrival. |

F522-set/F776-clear arrival runs against the **alternate** population. It references old
ORB153–155, whose absent lookups can alias main. The baseline arrival staging/completion
example cannot be promised for this state. An unfinished arrival prevents both the 776
write and later victory-scene dispatch. Externally supplied flags do not prove played scenes.

### The Complete Reward Announcement

**Confirmed:** `cs_5996E` contains twelve commands, including the terminator:

| Index | Command | Consequence if reached |
| --- | --- | --- |
| 0 | textCursor 1794 | Select the first line below. |
| 1 | setCameraEntity CAMERA_NEUTRAL | Write camera target -1 directly, without actor lookup. |
| 2 | reloadMap 10,0 | Reuse the working layout and request camera/display state. |
| 3 | setPos Bowie,25,12,Up | Position the admitted Bowie actor. |
| 4 | setPos Peter,24,13,Up | Position the admitted Peter actor. |
| 5 | setPos B,25,13,Up | Position the actual B31 lookup. |
| 6 | fadeInB | Request fade-in and wait for its service. |
| 7 | nextSingleText $0,134 | Display 1794, close windows, sleep 10. |
| 8 | setCamDest 20,7 | Disable following, request camera destination, wait for scrolling. |
| 9 | nextText $0,134 | Display 1795, leaving continuation open. |
| 10 | nextSingleText $0,134 | Display 1796, close windows, sleep 10. |
| 11 | csc_end | Conditional scroll wait, clear speed, restore registers, return. |

Lines 1794–1796 use Ponpei 134: “Checkmate” and a gift introduction; the statement that
a Cotton Balloon is inside; and advice that it permits safe departure from Desktop Kingdom.
The [literal text/control table](../../research/battle22-outcomes-and-desktop-return.md#camera-actor-and-text-services)
retains keys 0702–0704 and W1/W2 controls. Normal NPC 134 uses KING_PONPEI188, no portrait
and speech bleep 6; he is neither KING_GRANSEAL nor the chess King. Actual mapped sprites
and already open portraits affect the service. Both text handlers wait for scrolling;
only nextSingleText performs the final window closure/sleep. Debug P2-Start can skip
the text without skipping the other commands, so F778 is not proof of every line being seen.

reloadMap uses negative d1 to reuse CURRENT_MAP=22's working blocks/layout; it does not
recopy flags, replace entities, rerun init or crop the board again. The fresh exploration
load already selected the full F522 layout. Its packed camera operand `(10,0)` becomes
`$1E00`, interpreted as `(3840,0)` before area/parallax handling. setCamDest multiplies
`(20,7)` by 384. These are camera-border requests, not player or return coordinates.

With coherent mappings, the three setPos requests are full-map Bowie `(25,12)`, Peter
`(24,13)` and B `(25,13)`, all Up, **without adding battle origin 9**. The handler skips
HP 0 true allies 0–29; B31 bypasses that test. An absent living Peter can write main;
absent B can then overwrite main at `(25,13)`. Other B aliases target another actor.
Victory healing does not supply missing follower flags. The writes change current/destination
coordinates and facing, but retain existing action scripts: followers may subsequently move.
They do not create a new return tuple, freeze actors or promise immutable final positions.

There is no warp, item grant, choice, flag command, movement sequence or follower-action
replacement in this program. Init writes 778 only after the terminator returns. Fade and
scroll waits depend on VInts, with no elapsed-time or visible-completion proof here; later
failure leaves earlier placement/camera/text effects intact. Exploration then conditionally
submits music/fade and reaches WaitForEvent. Pending type 0 targets physical 0 and installs
the current-mode action; a new pending event takes priority. BattleLoop supplied PLAYER_TYPE=0,
so the coherent path selects eas_ControlledCharacter. Completed init alone does not prove
later input/services returned or usable control occurred.

## Talking to Ponpei and the Maid Again

**Confirmed:** the shared event table remains. Actual activation and wrapper return belong to
the [interaction owner](../../research/desktop-kingdom-interactions-through-battle22-entry.md#actual-activation-event-selection-and-return);
apply the new population before reusing physical examples. These callbacks use F522,
independently of F776/F778:

| Interaction/state | Content and writes |
| --- | --- |
| Ponpei, F522 set | Line 1797 praises the party and offers an army if they stay; no force join or F777 write. |
| Ponpei, F522 clear | Existing F777 permission branch from the predecessor. |
| Maid, F522 set/F256 clear | Lines 1798–1799 explain the King's joke and special treasure; set 256 after both text calls return. |
| Maid, F522 set/F256 set | Lines 1800–1801 mention adventurers northeast in Floor World and ask for a visit; no further flag write. |
| Maid, F522 clear | Line 1777; no F256 check/write. |

F256 is temporary: every explicit fresh exploration load clears 256–383, even for the same
map. The scene's reloadMap does not reset it. Repeated Maid interaction within that load
advances to the second arm; later fresh reentry can restore the first arm even with F778 set.
An interruption before SetFlag leaves 256 clear; after it there is no rollback.
F522/776/777/778 are outside the reset range. None establishes an observed conversation.

The event action bitfields turn Maid toward the player without restoration, and turn Ponpei
then restore his saved facing on return; they are not approach restrictions. Maid selects
no portrait/bleep 3, Ponpei no portrait/bleep 6. The wrapper temporarily disables entity updates,
conditionally restores facing, closes windows and reactivates updates after the callback.
These services must return before local completion; no army recruitment, Balloon acquisition
or new map request occurs. F522 independently suppresses the chess-start zone. The
[guard and departure continuation](cotton-balloon-through-floor-world-entry.md#the-exit-guard-reads-chest-state)
explains F212/F258 clearance, wrapper-restored facing and the separate own-tile item match.

## Loss, Egress and Repeat Returns

**Confirmed:** defeat updates the unlock flag, delivers exhaustion music/text, restores
Bowie's HP, halves unsigned gold and returns Egress coordinates with d4=-1. Admitted
Egress/Angel Wing deducts spell MP or removes the selected item, performs its text/flash/closure,
updates unlock and returns Egress coordinates with d4=0. Admission does not prove menu
availability, MP or item custody. Neither path runs the victory after/join or sets 522/472.
Unlock update preserves 422 with 522 clear and clears 422 with 522 set.

Battle22 has no special Egress destination. The admitted zone wrote EGRESS_MAP=22; with
that retained and BattleLoop's F399 set, the savepoint is Desktop `(5,8)`, Up. Changed
EGRESS_MAP selects its own row or default `(1,1)`, Up; a direct F399-clear call uses
Map3 `(56,3)`, Down. F64 independently resets saved raft coordinates; Map22 takes the
default Map69 `(42,15)` reset, while F64 clear preserves them. These are not victory seeds
or post-scene positions.

Loss/escape also bypass CheckBattle on the immediate return. Retained F422 after an
incomplete first encounter does not immediately readmit Battle22; later normal admission
can use it. Explicit Map22 exploration loads its actual F522 population/init gates, rather
than choosing init from outcome sign. Supplied 522 set/778 clear can run the announcement
even after nonvictory. Repeat victory with 522/778 set still seeds/joins/writes flags but
skips the scene, so it has no automatic `(25,12)` placement. Suspension remains a separate
F88 save/continuation path and proves no cross-process persistence.

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `546ccbb21ab1e9da42f5899fe7eae86ead18d255`
contains [#912/PR914 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/914#issuecomment-6175364090)
and [#911/PR913 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/913#issuecomment-6175260041).
The research pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This guide adds no extraction, source reacquisition or runtime observation.

Pre-synthesis adversarial review checked the complete outcome owner, initial-roster guide,
linked arrival/interaction and request consumers, lifecycle contract, fixture payloads,
schemas and existing verifier selections:

| Fixture / narrow reproduction owner | Reviewed boundary |
| --- | --- |
| [sf2-battle-loop-static-v1](../../../tests/fixtures/h2/battle-loop-static-v1.json), `uv run sf2 h2 battle-loop` | Counting eligibility, healing and killed cleanup; kill-all HP writes differ from the selected leader tail. |
| [sf2-battle-control-static-v1](../../../tests/fixtures/h2/battle-control-static-v1.json), `uv run sf2 h2 battle-control` | Two ordered outcome checks, victory/loss returns and suspension; selected seed/gate operands come from accepted direct joins. |
| [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `uv run sf2 h2 battle-cutscenes` | Alive-Bowie/dead-leader checks and common join tail; exact slot22 targets/tables require the research's complete selection. |
| [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `uv run sf2 h2 map-init` | Complete eleven-operation Map22 init, arrival-first branches and post-return 776/778 writes. |
| [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `uv run sf2 h2 map-script-engine` | Selected cs_5996E reload/camera sites and handler-local placement/text/service boundaries; aggregates alone do not prove composed natural execution. |

Their schemas and verifiers are the existing
[loop](../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[control](../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[cutscenes](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[init](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py) and
[script](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py).
Exact fourteen rows, two copies, twelve commands, literal text and immediate full-load/follower/
callback consumers remain with the research's [reproduction boundary](../../research/battle22-outcomes-and-desktop-return.md#reproduction-and-retained-limits).
Routing aggregates with routeTargetsParsed=false do not prove selected slot targets. No accepted
semantic conflict was found. Older runtime-question queues remain deferred ambiguities under
ADR0014/0016, not acquisition instructions.

Preserve [#912's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/912#issuecomment-6175333338):
head `9cd21b5766caa4e939913aef2c818e9097681f98`, tree `6c76d0b0cd5d30d36f4201d57f815d01aa98aa41`,
base `f8cd67cd3818a1790b76354922015c13d8af083a`,
[CI38614495457](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38614495457).
Its unchanged 18,285,979-byte pair comparison passed in 0.084s; scope/research-public succeeded,
engine/adapter/H4 skipped by scope. Its oversized reads, guessed config/source paths/corpus key,
Windows 206 rejected write and routing `KeyError` for output.romSha256 remain preserved with their
corrections; no gameplay suite failed. This synthesis does not rerun or replace that receipt.
[#911's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/911#issuecomment-6175244286)
retains scope-only [CI38613823392](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38613823392)
and its corrected URL/drive-token diagnostic. The predecessor's
[evidence section](battle22-initialization-and-roster.md#evidence-review-and-remaining-limits)
retains distinct #908 original/rebased objects and CI, #907 scope-only CI, and linked
#904/#903/#900/#899 failures/corrections, including csc2D FLAGS_A5/6 bypass, working-layout
reload and RightLeft facing. Those results remain at their owning receipts.

Direct acceptance checks complete prose/diff, quantities and coordinate/identity contrasts,
provenance, links/fragments, English/private boundary and whitespace. Inspect the configured
clean committed dependency planner and actual final-head CI. No local normal/full/engine/
Godot/H3/H4 or translation suite is selected solely for prose.

**Unknown:** natural outcome/return/interaction; supplied flags, party, HP/class/items,
lookups/current-saved geometry, mode/raft; completed input/action/text/scroll/fade/services;
visible/heard performance, usable control and persistence. Counterexamples above are source
states, not executed scenarios or an automatic runtime queue under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). The accepted #916 continuation belongs to the
[Cotton Balloon guide](cotton-balloon-through-floor-world-entry.md). This completes neither the campaign nor
[epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
