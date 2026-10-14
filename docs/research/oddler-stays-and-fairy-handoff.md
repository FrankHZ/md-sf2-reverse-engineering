# Oddler stays and the Fairy follower handoff

Evidence date: 2026-10-14. **Confirmed:** an admitted Map33 zone0 call with
F774 set, F785 set and F786 clear runs the complete Oddler departure scene.
It hides four recruit actors, stages B's farewell and hide, then requests
Peter and resident130 to follow. Only after script return does the caller set
F786, set F69 and clear F68. **Inferred:** this stages Oddler staying with Creed
while Fairy accompanies the party. **Unknown:** natural reach, actual supplied
actor/state premises, completed presentation, usable control and save persistence.
The current resident130 relation and the next population's Fairy158 are distinct.

## Agreement and provenance

Standard rigor under [Issue #957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/957)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one coherent scene through its immediate zone return, including changed-state
and fresh/reused-population consequences. Accepted starting base is
`17718cd3ece7dc087737f3a6bea4eb463fd4553f`. This document alone is owned.
Consume accepted [Creed return and recruitment](./creed-return-fairy-and-recruitment-choices.md),
[consultation](./evil-spirit-consultation-and-zone-return.md),
[shrine entry](./evil-spirit-shrine-entry-and-scene-admission.md),
[restoration](./creed-restoration-and-basement-departure.md), and
[first Goliath scene](./creed-mansion-entry-and-first-goliath-scene.md).
No shared index, fixture, schema, renderer, comparator, engine/UI, translation,
runtime, acquisition or environment work. Concurrent synthesis consumes accepted
predecessors only. Stop before a later walked mansion exit, overworld travel or
dwarf healing; instructions to go north are dialogue, not that travel.

All original-source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only source checkout is clean at that pin. Retained evidence
binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Setup filenames mean `data/maps/entries/map33/mapsetups/`; map/entity engine
filenames mean `code/common/scripting/map/` and `code/common/scripting/entity/`.
Constants come from `sf2enums.asm`, RAM symbols from `sf2const.asm`. Fnnn denotes
flag nnn. Address ranges are half-open; source lines and command indices are
static locators, not runtime observations.

Minimum evidence is pinned static source and existing selected fixtures plus
the retained 18,285,979-byte map-init/map-script-engine pair. Comparison is
linear in that pair, budgeted under one minute/512 MiB, with only small selected
rows, prose and receipts. The predecessor's 0.081s comparison is retained; no
regeneration or new framework is needed. Reassess material contradictions or
consumer expansion before increasing this scope. Exact joins, rather than
counts alone, support the claims below.

## Request, gate and actual actors

**Confirmed:** `entityscriptengine_2.asm:loc_5294` recognizes working-layout
type `$1400`, writes MAP_EVENT_ZONE_EVENT=6 and derives word X/Y from main's
destination plus movement offsets divided by MAP_TILE_SIZE=384. Movement,
collision, actual layout and reaching that producer remain premises.
`ProcessMapEvent` clears the pending event type before dispatch; type6 installs
main's init action and passes parameter1/3 words to `RunMapSetupZoneEvent`.
The wrapper reselects the current setup and compares coordinate low bytes,
accepting the first matching row (255 would be a wildcard). It does not prove
that `(15,21)` was reached by ordinary walking.

The complete `s3_zoneevents.asm` table at `$5AA8A..$5AA96` is:

| Row PC / coordinate | Target / immediate boundary |
| --- | --- |
| `$5AA8A` / `(15,21)` | Map33_ZoneEvent0, `$5AA98..$5AACA` |
| `$5AA8E` / `(12,14)` | Map33_ZoneEvent1, `$5AACA..$5AAD8`: if F783 clear, call the separate cs_5AF36; otherwise return |
| `$5AA92` / default `$FD,0` | Map33_DefaultZoneEvent, `$5AA96..$5AA98`: rts |

All four setup pointer tables and all six Map33 selector routes share this
zone table. Zone0's complete control flow is:

| PC / source line | Operation and branch |
| --- | --- |
| `$5AA98` / 25 | chkFlg774 |
| `$5AA9C` / 26 | bne.s byte_5AAAA: set F774 selects the later tests |
| `$5AA9E` / 27 | Otherwise script cs_5AC58, the earlier Goliath scene |
| `$5AAA4` / 28 | After that script returns, setFlg774 |
| `$5AAA8` / 29 | bra.s return_5AAC8; no fallthrough to Oddler's scene |
| `$5AAAA` / 32 | chkFlg785 |
| `$5AAAE` / 33 | beq.s return_5AAC8: clear F785 skips |
| `$5AAB0` / 34 | chkFlg786 |
| `$5AAB4` / 35 | bne.s return_5AAC8: set F786 skips |
| `$5AAB6` / 36 | script cs_5B466 |
| `$5AABC` / 37 | After script return, setFlg786 |
| `$5AAC0` / 38 | setFlg69 |
| `$5AAC4` / 39 | clrFlg68 |
| `$5AAC8` / 42 | rts |

There is no F787, joined-recruit, F784, F783, F762, F67/F68, HP, membership,
capacity or successful-follow prerequisite in this gate. F785 is set after the
earlier Fairy callback even if its Bowie-HP-guarded following was skipped.
A direct scene invocation omits this gate and these caller writes entirely.
F774-clear priority still holds if F785/F786 are already set.

The last-set-wins selector in `data/maps/mapsetups.asm:106–112` is default,
F523, F783, F784, F786, then F22. Default/F783/F22 select baseline; F523 selects
the restoration population; F784 selects the post-consultation population;
F786 selects the stay-behind population unless F22 overrides it. Setup selection
at callback time does not rebuild an already loaded population.

For the ordinary F784 population with F786/F22 clear, `s1_entities_784.asm`
(`$5A712..$5A74C`) supplies the following seven resident rows. P=Peter7,
B=FOLLOWER_B31, C=resident129/Creed, F=resident130/Fairy in the scene tables.

| Logical actor / lookup offset | Declared sprite, tile, facing and action | Normal F784 init |
| --- | --- | --- |
| 128 / 32 | WORKER202, `(17,14)` Up, eas_Init | No additional placement; Goliath is unused by cs_5B466 |
| C129 / 33 | CREED212, `(17,5)` Down, eas_Init | No additional placement |
| F130 / 34 | FAIRY219, `(17,10)` Up, eas_InitSlow | No additional placement |
| Eric14 / 14 | Ally sprite, `(63,63)` Down, eas_Init | If F14 clear: `(15,14)` Down |
| Randolf16 / 16 | Ally sprite, `(63,63)` Up, eas_Init | If F16 clear: `(16,15)` Down |
| Tyrin17 / 17 | Ally sprite, `(63,63)` Down, eas_Init | If F17 clear: `(18,15)` Down |
| Karna15 / 15 | Ally sprite, `(63,63)` Up, eas_Init | If F15 clear: `(19,14)` Down |

These placement commands also skip the named ally if HP0. F787 does not suppress
init placement. Ally mapsprites depend on class and HP; a dead ally outside
battle can use blue flame. Exploration healing does not revive every dead ally.
The full init's independent F523-set/F783-clear restoration branch and Hassan
EGRESS_MAP write remain possible under artificial supplied flags; F784 alone
is not proof of chronology. The accepted restoration owner retains that branch.

`mapfunctions.asm:InitializeMapEntities` allocates flag-selected followers before
residents, masks resident coordinates by `$3F`, scales by384, and skips an ally
resident whose lookup is already nonzero, without testing joined/active status.
With only F67/F68 ordinary followers, Bowie is physical0, P/B physical1/2,
128/C/F physical3/4/5, and Eric/Randolf/Tyrin/Karna physical6/7/8/9. This example
assumes normal mode and no extra followers; it is not an intrinsic slot assignment.
The ordinary follower table has no four-recruit rows. A recruit's earlier current
following or parked shrink state can nevertheless be inherited in this room.

`data/scripting/entity/followers.asm` selects P for F67, B/Oddler for F68 and
FOLLOWER_C158/Fairy for F69. Fairy158 uses lookup62, not resident130's34.
Other rows F74/F70 also allocate B with Astral; repeated logical identities can
overwrite lookup31 while leaving earlier physical actors. Map33 does not select
the overworld Caravan follower table. Missing ordinary lookups initially zero
can alias physical0; hide changes matching lookups to -1. The selected command
consumers do not validate, recreate or repair those mappings. Actual sprite,
class, mode, population, list order and alias premises must accompany any result.

## Complete scene and embedded actions

**Confirmed:** `scripts.asm:536–627`, cs_5B466 at `$5B466..$5B5B6`, contains 67 retained
map-script records indexed0–66, spanning336 encoded bytes including inline bodies.
Nine inline action bodies add their own movement,
facing and endActions records; they are not additional top-level commands.
All records and complete bodies are covered below. D/U/L/R mean relative tile
movement; `faceDown1` is a facing/update/wait operation, never D1. Every action
body here is waited for its selected actor only. Text IDs refer to the next table.

| Source lines / command indices | Complete ordered sequence |
| --- | --- |
| 536–544 / 0–8 | Cursor1932; camera destination `(12,15)`; sleep20; P Up; Bowie Up; B shiver; sleep20; P1932 single; C1933 single. |
| 545–555 / 9–19 | B Up; F Up; C at `(17,15)` Down; camera destination `(12,7)`; install/wait eas_Init on P, then B, then F; Bowie `(15,23)` Up; P `(15,22)` Up; B `(16,22)` Up; F `(16,23)` Up. |
| 556–565 / 20–25 | Hide Eric, Randolf, Tyrin, Karna in that order; camera targets C; C action D2,L2,D4,endActions. |
| 566–573 / 26–30 | Camera destination `(10,18)`; P1934 single; C1935 text; C R1,faceDown1,endActions; C1936 single. |
| 574–578 / 31–35 | P Right; B Left; P1937 single; B1938 text; B1939 single. |
| 579–586 / 36–40 | C L1,faceDown1,endActions; P Up; B Up; C1940 text; C1941 single. |
| 587–594 / 41–46 | B U1,endActions; B Left; C Right; sleep60; C nod; sleep30. |
| 595–606 / 47–52 | C L1,faceDown1,endActions; B L1,faceDown1,endActions; B1942 text; B1943 single; B nod; sleep30. |
| 607–618 / 53–60 | B U1,endActions; sleep70; B Down; sleep50; B Up; sleep30; B U3,endActions; hide B. |
| 619–627 / 61–66 | C R1,faceDown1,endActions; C1944 text; C1945 single; followEntity P,Bowie,2; followEntity F,P,2; csc_end. |

Under successful distinct mappings, permitted placements and completed relative
actions, C's path is `(17,15)`→`(17,17)`→`(15,17)`→`(15,21)`→`(16,21)`→
`(15,21)`→`(14,21)`→`(15,21)`. B's explicit placement and actions give
`(16,22)`→`(16,21)`→`(15,21)`→`(15,20)`→`(15,17)` before hide. The five
faceDown1 operands do not add Y steps. C ends with a requested Down facing;
B turns Down then Up during the pauses and is hidden after its last U3.
These are conditional coordinate derivations, not observed collision-free motion.
B's predecessor follow pointer is replaced by Init before these moves. Bowie's
and P's named-ally HP guards can skip their placements/facings. F and C are NPC
selectors and B31 bypasses that named-ally guard.

The scene has no map reload, layout copy, warp, join/leave-force command,
yes/no menu, story-flag command, EGRESS_MAP/PLAYER_TYPE write or explicit music
change. Four recruit hides apply regardless of joined/active status, selected
choice, HP or prior shrink. The scene does not undo a recruitment choice or
select a replacement recruit.

## Dialogue, action consumers and limits

**Confirmed:** `data/scripting/text/gamescript.txt` keys078C–0799 join the
fourteen literal source comments to IDs1932–1945 in order. S means
nextSingleText; T means nextText. Speakers refer to the actual resolved sprite,
not a universal assertion about whoever occupies the selector.

| ID / source line / form | Speaker and content | Exact controls |
| --- | --- | --- |
| 1932 / 543 / S | P asks about Oddler | `{W1}` |
| 1933 / 544 / S | C says he understands | `{W1}` |
| 1934 / 567 / S | P asks what he understands | `{W1}` |
| 1935 / 568 / T | C says Oddler will remain | `{W2}` |
| 1936 / 573 / S | C asks Oddler to agree | `{W1}` |
| 1937 / 576 / S | P asks if it is true | `{W1}` |
| 1938 / 577 / T | B says he wants to stay without knowing why | `{N}{W2}` |
| 1939 / 578 / S | B insists he must stay | `{N}{W1}` |
| 1940 / 585 / T | C calls him weak | `{W2}` |
| 1941 / 586 / S | C recommends staying and saying farewell | `{N}{W1}` |
| 1942 / 603 / T | B thanks the leader for travelling together | `{N}{LEADER}{W2}` |
| 1943 / 604 / S | B thanks the party and asks them to be careful | `{N}{W1}` |
| 1944 / 623 / T | C says the leader will learn the reason later | `{LEADER}{N}{N}{W2}` |
| 1945 / 624 / S | C tells them not to worry and to go north | `{N}{W1}` |

P and C use modifier0; all four B texts use `$80,FOLLOWER_B`, encoded `$801F`.
`csc1D_showPortrait` tests bit15/bit14 for portrait options; B supplies bit15,
not bit14. It does not replace an already-open portrait. The lookup for portrait
and speech masks the modifier and uses the actual mapsprite through
`getentityportaitandspeechsfx.asm:GetEntityPortaitAndSpeechSfx` (`$45638..$4567A`).
`data/spritedialogproperties.asm` gives base Peter his base portrait/bleep4,
promoted Peter his promoted portrait/bleep5, Creed his portrait/bleep8 and
Oddler his portrait/bleep4. Other actual sprites, HP/class or aliases can change
these selections. F130 and Goliath128 have no speech in this scene.

`mapscriptengine_2.asm:csc04` sets the cursor; csc00/02 display then increment
it, nominally ending1946. csc00 also closes portrait/text and sleeps10; csc02
does not. Both wait for view scrolling before display. `textfunctions_1.asm`
consumes N as newline and W1/W2 as input waits; W2 includes continuation-arrow
and validation-sound behavior. LEADER calls UpdateForce and reads the first
active member's name, not necessarily Bowie under changed membership. The
scene's HP guard can still read Bowie independently. No yesNo/F89 branch or
choice mutation follows these wishes and questions. They do not heal weakness,
explain its cause mechanically or perform a northward warp.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry; debug/P2 Start can latch it.
Skipped csc00/02 consume their operand without displaying or advancing the cursor.
Thus nominal text IDs, input waits and sleeps are not a guarantee of elapsed
service time; non-text commands still execute. The source transcript establishes
what the scene supplies, not visible text, portrait orientation or audible output.

The selected consumers preserve these distinctions:

- `mapscriptengine_1.asm:csc19` places current and destination X/Y together from
  unsigned bytes scaled by384 and sets facing/update. csc23 sets facing/update.
  Their byte-actor HP guard skips allies below30 with HP0; C/F and B bypass it.
  No joined/active predicate is added. Init/action/gesture/hide commands below
  have no equivalent named-actor HP skip.
- csc15 installs each eas_Init pointer, seeds the entity wait timer with its
  physical index and waits for that actor's pointer to become eas_Idle.
  `data/scripting/entity/eas_actions.asm:eas_Init` sets acceleration factors0/0,
  speed32/32, acceleration/deceleration off, obstructability on, entity/map
  uncollidable off, autofacing on, orientation Up, immersion off, size24,
  sprite update and idle. It does not reset position or remove an existing
  EXPLORATION_ENTITIES entry. F's earlier InitSlow speed is replaced here.
- csc2D compiles the nine inline programs into action RAM, clears actor flags
  A bits5/6, appends an eas_Idle jump and waits only for the selected pointer.
  The 16 inline action operands and nine endActions are all consumed. Move
  macros encode direction and signed amount. `esc04_moveToRelativeDest` scales
  deltas by384, adds current position and checks destination conflict.
  `faceDown 1` encodes `$0B,1`; csc2D_B emits setFacing3, updateSprite and wait1.
  It contributes no movement. esc00 consumes the entity's byte wait timer.
  Neither waits nor speed32 are measured presentation durations/rates.
- csc2A shiver saves shared size/animation state, selects size21, stops animation
  and repeats resize/update, Sleep5, clear-resize/update, Sleep5 three times;
  it restores saved size/animation afterward. It does not restore an originally
  set resize bit. csc26 nod stops animation, sleeps10, transforms/DMAs the nod,
  sleeps20, restores by sprite update, sleeps10 and sets animation counter0.
  These services and the explicit sleeps20,20,60,30,30,70,50,30 do not join all
  other actors' actions or establish visible gestures.
- csc32 disables view following, sends the requested tile destination through
  SetCameraDestination/SetViewDestination and waits for scrolling. The three
  destination operands are `(12,15)`, `(12,7)`, `(10,18)`. csc24 between them
  resolves C through lookup33 and sets VIEW_TARGET_ENTITY; the final csc32
  disables that following again. There is no final camera-target-Bowie command.
  View clamping/service, aliases and completion matter; these are not player
  coordinates or evidence of a displayed camera position.

Each explicit action wait is per actor, with no timeout or global settlement
barrier. Installing a pointer can replace an earlier follow/shrink/action.
Even apparently serial script commands coexist with entity service. A missing
or aliased actor can redirect mutations or waits; hidden mappings have no local
validity repair. Static sequence completion is conditional on those services
returning, not proof that an arbitrary supplied state can finish the scene.

## Five hides, two current follows and ordered return

**Confirmed:** csc2E decodes a word selector, resolves it and calls
`entityfunctions_2.asm:HideEntity`. The four recruit calls occur before C's
walk; B's call follows its waited U3. Each writes current/destination coordinates
`$7000,$7000`, invalidates every matching ENTITY_INDEX_LIST byte to -1 and
removes the physical actor from EXPLORATION_ENTITIES if present. It does not
change joined/active flags, remove force membership, clear an action pointer,
erase sprite/flags or deallocate the entity record. It does not await a prior
shrink or prove that all four recruits had finished shrinking.

`entityscriptengine_2.asm:VInt_UpdateEntities/loc_4EF4` skips current X >=$7000
before entity update/action dispatch. A hidden slot is therefore dormant during
ordinary service while that X remains, even though its old pointer survives.
That is different from the accepted shrink tail's `(63,63)` parking, which
retains the lookup and list membership. Hiding the previously chosen recruit
removes its current following/list entry but leaves its recruitment intact.
No source here guarantees rendered absence or later reactivation behavior.
With aliases, the first hide can invalidate several logical lookups; later
hides/actions need not refer to independent actors. No local repair occurs.

The final followEntity words are `$0007,$0000,2` and `$0082,$0007,2`.
`csc2C` reads the follower word's **high byte** for
AdjustScriptPointerByCharacterAliveStatus before decoding either actor. Both
high bytes are0: both relations test Bowie's HP, not Peter's/Fairy's. HP0 skips
six argument bytes and the relation; it does not undo the hides, earlier scene
work or the later caller writes if the script returns. A living Bowie with a
dead Peter can therefore admit both follows even though Peter's own placement
and facing commands were skipped.

Otherwise csc2C resolves current mappings, indexes direction2 in
`table_FollowerPositions` to `(-24,0)` and calls AddFollower. Each call copies
42 bytes of eas_Follower1 into action RAM, patches its physical target and
offsets, advances the allocation pointer and replaces the follower's action.
It appends the physical actor to EXPLORATION_ENTITIES only if absent. An already
listed actor still receives a new action allocation. The calls request P→Bowie,
then resident130→P; they do not teleport, wait for settling, create Fairy158,
join force or set F69. The follower template in `data/scripting/entity/eas_main.asm`
sets its speed/movement properties and follows through destination waits when
serviced. Missing/self/aliased targets can replace relations without validation.
The new Fairy→P relation is installed only if this second call is admitted.
Init has already replaced F's earlier Fairy→B action, so skipping the final
follow does not preserve that earlier relation.

`mapscriptengine_2.asm:loc_47234` handles csc_end: when DIALOGUE_WINDOW_INDEX is
nonzero it waits for view scrolling, then clears VIEW_SCROLLING_SPEED, restores
caller registers and returns. It does not poll every actor. The MAPSCRIPT6 trap
activates entity updates and calls the j_ExecuteMapScript alias; its register
restoration does not restore mutated global state. Only this returned path
reaches caller set786, then set69, then clear68, then rts at the PCs above.

`RunMapSetupZoneEvent` closes portrait/text after the selected callback, waits
one VInt and calls WaitForEntityToStopMoving with physical index0. That helper
polls current/destination X/Y equality, not eas_Idle, every follower or service.
The physical0 wait also occurs for the default/skipped callback; no-valid-setup
bypasses callback/window closure but still reaches the wait. An invoked callback
and an admitted scene are not synonymous. No new usable-controller observation
or automatic map exit follows from either return.

The sequence is not atomic. Interruption before script end can preserve hides,
positions/actions and partial text while all three caller flags retain their
old values. Interruption after set786 can leave69/68 unchanged; after set69 it
can leave68 set as well. If786 was set by that partial prefix, a later zone0
call takes the skip branch rather than completing the missing writes. Already
set69 or clear68 are simply reasserted on a fully admitted return. No rollback,
rewind, all-actor settlement or save persistence is established by these flags.

## Current population, fresh entry and changed premises

**Confirmed:** the returned scene has not reloaded Map33. With successful
ordinary mappings,128 remains the existing Goliath; C remains the staged Creed
near `(15,21)`; F remains the existing Fairy130, with a requested P-follow
relation if admitted; B and all four recruit actors are hidden/dormant. The
last explicit Bowie/P/F placements were `(15,23)`, `(15,22)`, `(16,23)`, Up,
but subsequent following/service means these are not promised settled positions.
No resident Oddler130 has been created at this point. F67, F762, F774, F783–785,
F787 and all recruit membership/active flags are unchanged by this scene/caller.
F786/69/68 have only the ordered mutations described above.

A later explicit fresh LoadMap runs entity reconstruction and selected init;
CURRENT255/reuse retains current actors/layout and does not perform that rebuild.
Fresh entry clears temporary flags256–383 and sets80, preserving the selected
non-temporary flags here. That is load-time behavior, not observed save storage.
Each setup accessor reselects current flags; changing flags between accesses can
produce combinations rather than a coherent narrative snapshot.

| Supplied state / boundary | Consequence and limit |
| --- | --- |
| Fresh F786 set, F22 clear | `s1_entities_786.asm` (`$5A74C..$5A766`) declares WORKER128 `(22,4)` Up walking with origin `(22,4)`/range1; CREED129 `(17,5)` Down eas_Init; ODDLER130 `(17,7)` Up eas_Init. No recruit rows. `s6_initfunction_786.asm:ms_map33_flag312_InitFunction` is only rts at `$5AC56`; the label does not make F312 its selector. |
| Fresh after full caller return, F22 clear, ordinary F67/69 set and F68 clear | Ordinary followers allocate P and Fairy158, separately from the three F786 residents. With no extras they occupy physical1/2; resident128/C/Oddler130 occupy3/4/5. B/Oddler is no longer selected by F68. Other B-producing flags still matter. Resident Oddler130 is not the hidden old B record, nor Fairy158 the old resident130 mapping. |
| Fresh F786 set but69 clear or68 still set | F786 still selects Oddler130 residents; follower selection independently omits Fairy158 and/or includes B/Oddler. Two Oddler sprites are possible from resident130 plus B. The zone skip does not reconcile these flags. |
| F22 set, including together with F786 | Last selector wins: six baseline resident rows from `s1_entities.asm` (`$5A6A6..$5A6D8`): WORKER128 Up, CREED129 Left, Eric Down, Randolf Up, Tyrin Down, Karna Up, all initially `(63,63)`. No resident130. F69 still independently selects Fairy158. Normal init places128 `(14,14)` Up. |
| F22 and F784 both set | The F22 Goliath branch does not bypass the later independent F784 recruit tail: living unjoined recruits are placed at the four coordinates above. Joined recruits normally remain parked; F787 does not govern placement. F523-set/F783-clear can still run restoration first; do not silently assume it already occurred. |
| F22 fresh layout | Map33's complete `3-flag-events.asm` has the F22 copy `(28,0)`, size25×32, to `(0,0)`. It has no786/69/68 replay of this scene or earlier Fairy's one-cell copy. This slice does not infer collision reach from that copy. |
| Retained population after set786 or changed22 | Existing Fairy130 remains that physical actor; callbacks may now select Oddler's1948–1949 words for it. Setting786 does not relocate Goliath or Creed, replace130, unhide recruits or instantiate158. F22 can select different tables without making absent130 present. Explicit init/rebuild invocation is a separate premise. |
| F774 set and either785 clear or786 set | Zone0 returns without this script and without all three writes, even if69/68 disagree with the intended narrative. Wrapper closure/physical0 wait still applies. |
| F774 clear | Earlier Goliath scene and set774 only on its return; no same-call Oddler fallthrough. F785/786 do not override priority. |
| F787 clear, no recruited ally, or784 clear | Does not reject this scene. Actual selected population/mappings can differ; admission is not a certificate that resident130 is Fairy. |
| HP0 Bowie | Bowie placement/facing and both final follows can skip; action/gesture/hides and caller flags can still execute if all services return. No natural-completion claim. |
| HP0 Peter or recruits | Peter placement/facing can skip while Init/action/text and Bowie-guarded follow still run. Recruit hides have no HP guard and do not delete membership. |
| Missing, hidden or aliased selectors | Zero can alias main; -1 has no validity guard. Hides can invalidate multiple aliases, and pointer replacements can redirect waits. No independent-actor result, repair or normal return is guaranteed. |
| Direct repeated scene or cleared786 | No local membership/69/68 guard inside the script prevents repetition. It may now encounter hidden recruits/B or a different130. A normal786-set callback instead skips. Neither proves safe replay. |

**Inferred:** the speech plus B's departure and the later population change
express Oddler remaining with Creed and Fairy continuing with the party.
**Confirmed:** those claims decompose into present-actor operations, ordered
caller flags and conditional future population selectors. No force-leave, new
recruitment, healing or automatic travel transaction is hidden in the narrative.

**Unknown:** natural zone reach and exact request/layout; supplied flags, class,
HP, joined/active/list state, mode, population and mappings; actual collision,
action/view/portrait/text/audio completion; controller usability and save/load
persistence. The original reason for Oddler's wishes and any later northward
route or dwarf-healing outcome are outside this slice. These limits do not
create an emulator or acquisition queue.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same PowerShell process as every
controlled command. Reuse the exact
[retained pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`.
The comparison here completed in0.085s with all18,285,979 bytes and mtimes
unchanged. No regeneration. Documentation-only acceptance uses direct checks
and actual selected CI; no blanket local normal/full/engine/Godot/H3/H4 run.

The complete selected joins and semantic review are reproducible as follows:

1. In retained `local/derived/map-script-engine-static.json`, select
   `programCorpus.programs[id=cs_5B466]`. Join all67 records to exact source
   lines536–627, macros/argument order and `$5B466` entry. Read all nine inline
   bodies through endActions:16 action pairs plus9 terminators, including five
   faceDown1 operands. Resolve encodings through `sf2cutscenemacros.asm` and
   the csc2D/esc consumers, rather than treating inline bytes as top-level calls.
2. In `tests/fixtures/h2/map-events/routing-setup.json`, select
   `expected.categories.zoneEvents.tables[symbol=ms_map33_ZoneEvents]`: all
   three rows/PCs/relative targets and all six Map33 zone route joins. In
   `zone-programs.json`, join all14 Map33_ZoneEvent0 operations with source
   opcode/operands/line/PC, plus default rts and the adjacent zone1 guard/call/
   return boundary. Preserve F774 priority and caller786/69/68 order.
3. Join all14 nextText/nextSingleText sites to `gamescript.txt`1932–1945,
   exact source literals, speaker/modifier/form and ordered controls above.
   Read cursor/skip/portrait/LEADER and acknowledgement consumers. No transcript
   or all-game text publication is needed.
4. Reuse the accepted Map33 setup/init joins; cross-check source selector
   order and complete baseline/F784/F786 populations, selected init and all
   ordinary follower rows. Confirm FOLLOWER_B31 and FOLLOWER_C158/lookup62,
   actual mapsprite properties and byte-actor versus word-high-byte guards.
   Directly inspect placement, Init, all selected action/gesture/camera paths,
   HideEntity plus dormant service, AddFollower, csc_end, trap and wrapper wait.
5. Review the entire document and cached diff, local links/anchors, UTF-8,
   sole-path ownership and private/generated boundary; run `git diff --check`.
   On a clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`, record selected
   partitions and actual exact-head CI in the Issue/PR handoff, then freeze a
   clean pushed Draft PR for independent main-gate acceptance.

Preserve [#953's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178749558),
[rebase receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178813915)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/956#issuecomment-6178823238).
They retain original/rebased exact Git objects, both actual CI runs and0.081s
pair; FOLLOWER_C158 and complete shrink-tail corrections; source-check suffix,
URL and direct-txt whitespace inspection diagnostics; linked #949/#945/#941/#937
results, completed failures and Unknowns. The consultation synthesis's
[callback correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)
also remains: a callback can return while skipping its scene and caller writes.
No completed suite or failure record was reset or replaced by this work.
