# Desktop Kingdom: First Arrival and Conditional Control

**Confirmed:** a fresh exploration arrival on Map22 builds a new population, stages the party's
landing and discussion, then installs following before a conditional return to exploration.
**Inferred:** the scene portrays Goliath putting the small party on his desk; Peter's suggestion to
look around gives the next local objective. **Unknown:** natural arrival, the actual supplied state,
completed actions/services, visible or heard performance, useful control and persistence.

This English guide continues [the magic-door transfer](creed-magic-door-and-desktop-transfer.md)
from its admitted fresh Map22 exploration endpoint, with F422 clear and F522/F776 clear for the
baseline first arrival. Fnnn means flag nnn. It consumes only the
[accepted arrival research](../../research/desktop-kingdom-first-arrival.md), under
[Issue #903](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903) and the
[Standard roadmap agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It stops at conditional local first-arrival return/repeat, before local interactions, the separate
`cs_5996E` branch or Battle22 opening, roster and outcomes. No new playthrough, raw-source
reinvestigation, remake or localization work is included.

## A Fresh Desk Population

**Confirmed:** admission and initialization are separate. F422 clear avoids the predecessor's
Battle22 selection; F422 set can select that battle even at requested `(0,0)`. F522 set cannot
cancel a battle already selected. This guide begins only on the fresh exploration path.

That path clears the pending event type, adjusts the step counter, heals eligible allies, fades
out and disables camera following. Explicit map 22 sets CURRENT_MAP=22/CURRENT_BATTLE=255, loads
tilesets, waits for fading, clears VInts, selects and initializes entities and loads their sprites.
It clears temporary flags 256–383, sets F80, initializes sprite-frame state, targets physical 0,
loads the map, installs base VInts and calls the selected init callback. Healing does not promise
revival. The mansion's parked/shrunk physical actors do not survive as this new population.

Map22 has a default setup and an F522 override, with no further override. Their entity pointers
differ, but EntityEvents, ZoneEvents, AreaDescriptions, Section5 and InitFunction are shared.
Each accessor selects the current setup anew. F776 selects no population; F522's alternate roster
is a different supplied-state boundary, outside the baseline below.

The allocator clears entity records and lookup bytes, allocates selected followers starting at
physical 1, then processes the 30 setup rows in source order. It declares Bowie at physical 0
**after** those rows, at requested `(0,0)`, Down, with `eas_Idle`. Shared SPRITE_SIZE becomes 16;
follower termination and mode/raft handling follow. Bowie is therefore not the first record declared.

All baseline rows appear below. P is a conditional physical-slot example: only follower flags
F67/Peter and F68/B are selected, Eric/Karna have no existing lookup, and raft handling does not
overwrite a slot. `Init` means `eas_Init`; `Fixed` means `eas_InitFixedSprite`. Logical NPC names
are script identities, not physical slots or combatants.

| Row | Logical identity | P | Mapsprite / action | Position, facing |
| --- | --- | --- | --- | --- |
| 0 | Eric 14 | 3 | Ally lookup; walking origin `(15,14)`, range 1 | `(15,14)`, Down |
| 1 | Karna 15 | 4 | Ally lookup; Init | `(5,15)`, Down |
| 2 | NPC128 | 5 | CARAVAN=62; Init | `(3,6)`, Down |
| 3 | NPC129 | 6 | YOUNG_MAN=196; Init | `(29,21)`, Right |
| 4 | NPC130 | 7 | YOUNG_WOMAN=197; Init | `(30,21)`, Left |
| 5 | NPC131 | 8 | OLD_MAN=198; walking origin `(24,22)`, range 1 | `(24,22)`, Down |
| 6 | NPC132 | 9 | PRIEST=192; Init | `(5,6)`, Down |
| 7 | NPC133 | 10 | MAID=208; Init | `(14,3)`, Down |
| 8 | NPC134 | 11 | KING_PONPEI=188; Init | `(15,2)`, Down |
| 9 | NPC135 | 12 | GUARD=206; Init | `(8,7)`, Left |
| 10 | NPC136 | 13 | GUARD=206; Init | `(33,24)`, Left |
| 11 | NPC137 | 14 | KING=153; Init | `(24,5)`, Down |
| 12 | NPC138 | 15 | QUEEN=135; Init | `(26,5)`, Down |
| 13 | NPC139 | 16 | BISHOP=144; Init | `(22,5)`, Down |
| 14 | NPC140 | 17 | BISHOP=144; Init | `(28,5)`, Down |
| 15 | NPC141 | 18 | KNIGHT=78; Init | `(20,5)`, Down |
| 16 | NPC142 | 19 | KNIGHT=78; Init | `(30,5)`, Down |
| 17 | NPC143 | 20 | ROOK=125; Init | `(18,5)`, Down |
| 18 | NPC144 | 21 | ROOK=125; Init | `(32,5)`, Down |
| 19 | NPC145 | 22 | QUEEN=135; Init | `(24,6)`, Down |
| 20 | NPC146 | 23 | QUEEN=135; Init | `(26,6)`, Down |
| 21 | NPC147 | 24 | PAWN=77; Init | `(22,6)`, Down |
| 22 | NPC148 | 25 | PAWN=77; Init | `(28,6)`, Down |
| 23 | NPC149 | 26 | PAWN=77; Init | `(20,6)`, Down |
| 24 | NPC150 | 27 | PAWN=77; Init | `(30,6)`, Down |
| 25 | NPC151 | 28 | PAWN=77; Init | `(18,6)`, Down |
| 26 | NPC152 | 29 | PAWN=77; Init | `(32,6)`, Down |
| 27 | NPC153 | 30 | ORB=172; Fixed | `(63,63)`, Down |
| 28 | NPC154 | 31 | ORB=172; Fixed | `(63,63)`, Down |
| 29 | NPC155 | 32 | ORB=172; Fixed | `(63,63)`, Down |

Eric 14 and Karna 15 use ally lookup slots because their sprite operands are below 30. An existing
nonzero ally lookup skips that row entirely without allocating another physical slot; neither row
requires joined/active membership. The current follower table selects neither Eric nor Karna.
Their class and HP still affect sprite selection, including BLUE_FLAME for a dead ally outside battle.
Eric and NPC131 receive walking programs with their listed origin/range: initial wait 30,
speed/acceleration/collision setup, then random walk, destination wait and wait 20 repeatedly.
Their declarations do not fix their later random positions.

The other rows append NPC lookups from offset 32; logical 128–155 resolve by subtracting 96.
Eric/Karna consume no NPC cursor. In the two-follower example Peter 7 is physical 1 and B 31 physical 2,
initially at the request's position/facing with `eas_Follower1`/`eas_Follower2`. Map22 is not an
overworld map, so F65 adds no Caravan/A prefix. Other follower selectors allocate independently;
repeated B/A/D identities are not deduplicated. Later lookup writes replace the mapping while older
physical records remain. Missing Peter/B lookups can resolve to physical 0. Force membership alone
does not supply either follower, and the P column is not a universal roster.

NPC128 is the new map's **CARAVAN62**, not the mansion's WORKER. No selected arrival command changes
its sprite or position. Its lines are attributed to Goliath by the dialogue; that does not declare a
giant sprite, prove his natural physical presence or carry a mansion actor into the map.

No selected entry/scene sets PLAYER_TYPE or saved raft location. Walking mode 0 is a coherent
predecessor premise, not an effect of being on a desk. F64 with RAFT_MAP=22 can declare a raft at
physical 31 using saved coordinates masked by `$7F`, overwriting the example's ORB154 record while
its NPC lookup still names 31. Mode 2 instead makes main a RAFT sprite, parks followers and declares
slot31 at `(64,64)` without the saved-map test. Initial sprite loading includes nonempty physical32;
later sprite transformation skips ENTNUM=32. Allocation, lookup and sprite-update behavior are
separate facts.

## Landing and Getting Their Bearings

**Confirmed:** the shared init dispatcher first checks F776. Clear runs the complete `cs_59656`;
only its return permits the caller to set F776. Set bypasses the entire arrival, without proving
it ever completed. The scene contains 136 top-level commands, seven custom bodies and thirteen
inline bodies. Command indices below are zero-based; B is logical FOLLOWER_B=31. Triples mean
Bowie, Peter, B, in that order. Every inline body ends with `endActions`; every custom body with
`ac_end`. Coordinates, speeds and waits are literal source operands, not measured performance.

The opening stages six actors for a paired descent, removes the trio's shadows and fades in.
After the ORBs are hidden, the party's orientation and animation are prepared for the discussion.
This supports the intended landing image without proving carrying or simultaneous arrival.

| Commands | Complete ordered actions |
| --- | --- |
| 0–10 | Text cursor 1748; reloadMap `0,19`; waited Init for the triple; stopEntity for the triple; three waited custom bodies, each `ac_setSpeed 64,64; ac_jump eas_Idle`. |
| 11–20 | setPos Bowie `(4,8)`, Peter `(3,7)`, B `(5,7)`, ORB153 `(12,16)`, ORB154 `(11,15)`, ORB155 `(13,15)`, all Down; removeShadow for the triple; fadeInB. |
| 21–29 | Unwaited inline Down 16 for Bowie, Peter, B, then DownLeft 8 for ORB153 and ORB154; waited DownLeft 8 for ORB155; hide 153, 154, 155 in order. |
| 30–39 | Three waited custom bodies: `ac_motion OFF; ac_orientLeft` for Bowie / `ac_orientRight` for Peter and B, then `ac_updateSprite; ac_jump eas_Idle` each. Wait 100; startEntity triple; waited Init triple. |

`reloadMap 0,19` is camera/display work on **current Map22**. It packs the camera words, passes
negative d1 to LoadMap and reloads current blocks/layout, area, parallax, animation and display state.
The explicit camera input supplies `(0,7296)` before area offsets, equivalent to tile Y19 in
384-unit coordinates. It neither requests map0, places main at `(0,19)`, rebuilds entities nor calls
init again. CURRENT_BATTLE=255 selects full-map loading rather than the Battle22 crop.

SetPos writes both current and destination coordinates at 384 units per tile, plus facing and a
sprite-update request. Inline moves compile to relative motion and destination wait, followed by
idle. The installer `csc2D` first applies `andi.b #$9F` to FLAGS_A, clearing bits 5/6. Its esc04
consumer calls HasSameDestinationAsOtherEntity, whose initial bit-5 test then returns via `clr.w d4`
with Z set, before scanning competing destinations; esc04's `bne` conflict branch is not taken.
These **unmodified selected moves bypass the destination scan**, including coincident trio/ORB
targets. Ordinary collision is not an explanation for their paired destinations.

Given the stated placements and completed unmodified moves, the nominal pairs are Bowie/153 at
`(4,24)`, Peter/154 at `(3,23)`, B/155 at `(5,23)`. Only ORB155's inline action is waited before
hiding 153,154,155. Alias, action replacement or stalled services still prevent a guarantee that
all six arrived together. Hide writes current/destination `$7000,$7000`, changes all matching lookup
bytes to `$FF` and removes a matching follower entry. It leaves the action pointer; this is stronger
than the mansion's `(63,63)` parking. With the raft overwrite, hiding 154 can hide that raft and
invalidate its other lookup. No visible result has been observed here.

## Realizing Where They Are

**Confirmed:** B recalls the strange door and upward feeling. Peter says they are small, investigates,
and describes an enormous desk like a village. NPC128 answers; Peter recognizes Goliath and protests.
The ensuing exchange joins the landing to Goliath's warning without implementing a universal
magic-door or protection rule.

| Commands | Complete ordered actions |
| --- | --- |
| 40–51 | Headshake Bowie, Peter, B; face them Up, Right, Left; wait 20; B texts 1748–1750, Peter texts 1751–1752. |
| 52–54 | Waited B custom `ac_setSpeed 48,48; ac_jump eas_Idle`; waited B inline Left 1; B text 1753. |
| 55–66 | Wait 5; unawaited Peter RightLeft; wait 100; waited Peter inline Down 2 then Right 1; wait 5; unawaited Peter RightLeft; wait 130; face Peter Up, Bowie Down, B Down; Peter text 1754; NPC128 text 1755. |
| 67–76 | Unwaited Bowie inline Left 1; waited Peter inline Right 1; wait 5; unawaited Bowie RightLeft; wait 10; wait 5; unawaited Peter RightLeft; wait 5; unawaited B RightLeft; wait 130. |
| 77–89 | Face Peter, Bowie, B Down; waited Bowie Jump, unawaited Peter Jump; shiver Bowie then Peter; waited DeactivateAutoFacing for Bowie then Peter; unwaited Bowie inline Up 1, waited Peter inline Up 1; waited Init Bowie then Peter. |
| 90–96 | Peter continuing text 1756; two consecutive waited Peter Jumps; wait 10; Peter single text 1757; NPC128 continuing 1758 then single 1759. |

The complete text sequence is below. C means continuing `nextText`; S means `nextSingleText`, which
closes text/portrait and sleeps 10 after display. `$C0` selects right-side and mirrored portrait bits;
`$0` selects neither. There is no Yes/No prompt. B is **Inferred** to be intended as Oddler, subject
to the actual lookup. NPC128's lines are **Inferred** to be Goliath's voice, not a sprite substitution.

| ID | Form; modifier; speaker | Exact literal |
| --- | --- | --- |
| 1748 | C; $C0; B | `What happened?{W2}` |
| 1749 | C; $C0; B | `A strange feeling swept over{N}me when I entered that door.{W2}` |
| 1750 | S; $C0; B | `Then, it felt like...I was{N}lifted upward!{W1}` |
| 1751 | C; $0; Peter | `It's very difficult to explain{N}what's happened to us....{W2}` |
| 1752 | S; $0; Peter | `Anyway, we're very small now.{W1}` |
| 1753 | S; $C0; B | `Small?  How small?{W1}` |
| 1754 | S; $0; Peter | `Let's see...now we're on a{N}desk...and it's very large,{N}like a village.{W1}` |
| 1755 | S; $0; 128 | `Exactly.{W1}` |
| 1756 | C; $0; Peter | `Goliath!{W2}` |
| 1757 | S; $0; Peter | `Hey, what did you do to us?{W1}` |
| 1758 | C; $0; 128 | `I told you not to go in{N}there.  You didn't listen{N}to my warning.{W2}` |
| 1759 | S; $0; 128 | `You guys look so cute!{N}Ha, ha!{N}Bye, little kiddies!{W1}` |
| 1760 | C; $0; Peter | `{LEADER}, cheer up!{W2}` |
| 1761 | S; $0; Peter | `Let's look around this{N}Desktop Kingdom.{W1}` |
| 1762 | S; $0; B | `I agree!  We can meet Creed{N}later.{W1}` |

**Confirmed:** portrait/speech selection reads the **actual mapped sprite**, not the narrative name.
B mapped to ODDLER selects Oddler's portrait and bleep 4; Peter's base/promoted sprites select their
own portraits and bleep 4/5. The complete dialogue-property table has no CARAVAN entry: baseline 128
falls back to PORTRAIT_DEFAULT `$FFFF` (no new portrait window) and bleep 6. An already open portrait
is not reopened. HP/class, duplicate/missing mappings or another selected follower can change this
presentation; modifier 0 alone does not promise a portrait.

`{N}` advances a line. W1/W2 wait on directional/A/B/C input; W2 also uses the arrow and validation
sound path. `{LEADER}` runs UpdateForce and names the first battle-party member, rather than an
immutable Bowie string. Debug mode plus P2 Start can latch SKIP_CUTSCENE_TEXT, bypassing selected
text commands and csWait sleeps. The nominal sequence assumes that skip is clear; its source waits
establish neither supplied input nor elapsed display time.

## Farewell and the Invitation to Explore

**Confirmed:** after Goliath's teasing farewell, the script requests two groups of sounds and turns
the trio. Peter encourages the leader to look around Desktop Kingdom; B agrees to meet Creed later.
That is the local narrative objective. The sound calls do not prove heard giant footsteps or a
completed departure: NPC128 receives no movement command in this passage.

| Commands | Complete ordered actions |
| --- | --- |
| 97–109 | Five repetitions of wait 20 then playSound DIALOG_BLEEP_6; face Bowie, Peter, B Right. |
| 110–120 | Five repetitions of wait 20 then playSound DIALOG_BLEEP_8; wait 50. |
| 121–129 | Waited Peter inline Left 2; face Bowie Down, Peter Up, B Down; Peter continuing 1760 then single 1761; face Bowie Right, B Left; B single text 1762. |
| 130–135 | Waited Init Bowie, Peter, B; followEntity Peter/Bowie/2, B/Peter/2; csc_end. |

The five bleep 6 requests (constant 74) and five bleep 8 requests (76) are explicit playSound calls,
separate from per-character speech sounds. Each is preceded by wait 20; wait 50 follows the second
group. The handler passes the operand to the sound-command service without waiting for audible
playback to finish. Text/sound requests and a heard performance remain distinct.

## Temporary Actions and Their Completion Limits

**Confirmed:** action names describe several different mechanisms. Stop/start write the animation
counter -1/0; they do not freeze movement or globally disable input. Custom `ac_motion OFF` also
writes animation counter -1, without zeroing velocity or canceling a destination. Orientation
changes low FLAGS_B bits independently of facing; speed writes affect X/Y speed fields.

Init resets acceleration factors 0/0, speed 32/32, acceleration/deceleration off, obstructability on,
entity/map uncollidable off, autofacing on, orientation Up, immersion off, shared size 24, then updates
the sprite and jumps idle. It neither places/revives an actor nor turns animation on. InitFixedSprite
adds autofacing off and animation off. The opening custom speed 64 and B's later speed 48 are temporary;
final Init restores 32. Earlier startEntity and headshake restoration are separate animation writes.

RightLeft means four Right/Left/Right/Left steps, each with sprite update and wait 30, then idle.
Jump turns autofacing off, moves `(0,-1)`, waits 5, moves `(0,1)`, waits 5, moves `(0,0)`, restores
autofacing and goes idle. DeactivateAutoFacing only turns that property off and goes idle. Installing
a later action replaces the actor's pointer even when an earlier unwaited look or jump is unfinished.
Waited forms poll **only the resolved actor's pointer for eas_Idle**; installer timer seeding uses
the physical index. They provide no all-actor barrier or timeout. Summing waits proves no exact
duration, displacement or service completion.

RemoveShadow transforms both frames of the mapped sprite through a 576-byte temporary buffer,
DMA-writes them and waits one VInt; it creates no persistent shadow-status field. Later sprite
updates can reload the pixels. FadeInB requests FadeInFromBlack across all palettes and waits for
the fading service and another VInt. A returned service is not an observed image.

Headshake synchronously performs seven pixel-rearrangement iterations, with sprite loads/DMA,
two VInts, update and another VInt after each half, then restores animation counter 0. It does not
move stored position or facing. Shiver saves shared size/animation, writes size 21 and animation -1,
then three times enables resize/update/Sleep 5 and disables resize/update/Sleep 5. It restores saved
size and animation, not an earlier resize bit. Neither command establishes a permanent scale.

Size is one shared word, consumed during transformation when an actor's resize bit is set.
Orientation and selected effects are also applied there; special sprites and ENTNUM=32 skip this
transformation, while sprite-load capacity can defer action progress. Fresh size 16, Init 24,
shiver 21 and the predecessor's 18 are different shared writes. They establish no persistent miniature
party, changed geometry, statistics, inventory or pickup attachment.

## Following, Flags and Conditional Return

**Confirmed:** final Init is followed by Peter-following-Bowie and B-following-Peter, operand 2.
The handler resolves physical identities, copies the 42-byte follower program, patches target and
signed offset `(-24,0)`, installs it and adds the physical follower to EXPLORATION_ENTITIES if absent.
It grants neither membership nor a warp into completed formation. The installed program repeatedly
follows its target/destination; command return does not prove a following result.

The alive guards differ. Placement, facing and stop/start can skip a dead ally. Action installation,
headshake, shiver, removeShadow and hide do not share that guard. The follow handler reads the
**high byte before the word follower operand**: encoded `$0007`/`$001F` both supply zero, so both
selected follow calls test Bowie's HP. The stats lookup masks the combatant index to its low byte;
full follower operands are resolved afterward. Peter/B HP and missing/duplicate mappings therefore
cannot be summarized by one uniform alive/missing-actor policy.

The scene terminator conditionally waits for view scrolling when a dialogue window exists, clears
scrolling speed and restores registers; it does not join concurrent actors. The init wrapper merely
calls the selected callback and restores registers, without the zone wrapper's physical 0-coordinate
wait or text/portrait closure. Only after `cs_59656` returns does its caller write F776.

The **independent next check** is F522: clear returns; set checks F778, returns when set, otherwise
calls the excluded `cs_5996E` then sets F778. F776 already set bypasses first arrival but not this
second check. Both setup pointers use this dispatcher. This identifies later content without
explaining its body or treating F776 as a population/completion-history guarantee.

Within the arrival scene there is no story-flag/F422/F522/F776, party-membership, PLAYER_TYPE,
saved-raft, warp or battle-admission write. F80/temp-flag work belongs to the earlier caller.
After init, exploration compares palette words, conditionally requests music/fade, sets movement
sound and enters WaitForEvent. Only MAP_EVENT_TYPE zero permits camera target 0 and the current-mode
controller to be installed on physical 0. Mode 0 uses eas_ControlledCharacter; mode 1 uses source-named
eas_Raft (Caravan control); otherwise eas_Caravan (raft control), without validating arbitrary modes.
Nonzero pending work skips controller installation and takes priority over A/C input. Scene return
therefore promises no unconditional useful control or stable event.

Fresh explicit-map entry is distinct from the map-index-255 return branch that runs
UpdateMainEntityProperties and DeclareRaftEntity. Neither camera reload nor init return enters that
branch, rebuilds the population or restores main to `(0,0)`. Supplied mode and actual scene mutations
still matter. This guide stops at that conditional first-arrival/repeat boundary, before interaction,
postbattle scene or Chessboard admission/content.

## Evidence Review and Changed-State Limits

**Confirmed provenance:** accepted main `e120a9b6890527eb018e081f1c5b1907c21155bc` contains the
corrected research from [PR902](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902).
Its [research owner](../../research/desktop-kingdom-first-arrival.md#agreement-and-provenance) pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
It owns the complete 30-row population, 136-command scene, seven custom/thirteen inline bodies,
fifteen text/control/speaker joins and named immediate consumers. This guide reviews those accepted
joins, not raw source or a new runtime.

The bounded existing executable review covers
[map-init fixture sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), its
[schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py), plus
[map-script fixture sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json),
its [schema](../../../schemas/h2/map-script-engine-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_script_engine.py).
The existing owners are `h2 map-init` and `h2 map-script-engine`; the accepted
[retained-comparison recipe](../../research/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
uses map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts` equality without
regenerating payloads. Map-init supplies selected dispatcher/pointer order. Map-script aggregates
and action-bridge facts establish their named stream/handler boundaries, not every local action,
text, allocation or service claim. Complete accepted selected direct-source joins supplement them;
no aggregate proves natural reach, presentation or composed runtime success. No conflicting accepted
owner was found at this boundary.

Preserve [#900's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174161863)
(head `d076ad3b35c97f1ee3af71ffa20872f18c0329b7`, tree `288535602cb5b6a7a8e03123731f631cfef339c8`),
[correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174261324)
(head `6a37579b1bb5e97f119ca933459e63e12060c4ad`, tree `f8760697e748a395b4d041771be3e6a5ac6b7f11`),
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174272544).
Both used base `95d55721b7234c7c8e877174fd8f9ff65e2bbca8`. The unchanged retained pair totals
18,285,979 bytes, compared in 0.081s. Original [CI38605731097](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38605731097)
and corrected [CI38606392847](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38606392847)
remain separate. Preserve the [resolved P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174194697):
installer FLAGS_A5/6 clearing bypasses the destination scan for these moves; this guide retains that
correction, not the original unconditional conflict explanation. Original complete joins and
comparisons were not rerun for the paragraph correction.

Preserve [#899's predecessor handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/899#issuecomment-6174047117)
and [PR901 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/901#issuecomment-6174065903),
including their original/rebased #896 receipts, #895 results and earlier linked failures. #900's
bounded inspection/path/constant-decoding/CRLF failures and corrections, #899's private-path
false-positive receipt and corrected pass, and independent-review bounded-read/attachment failures
remain at those links. None is replaced by this guide or relabeled as a failed game suite.

| Changed supplied premise | Consequence retained from accepted static review |
| --- | --- |
| F422 set | Battle22 can be selected before this exploration premise. |
| F776 set, F522 clear | Baseline population loads, but the arrival/placement/follow work is skipped; prior completion is not proved. |
| F522 set, F778 clear/set | Alternate population, then independent postbattle call/return after the F776 check; not this baseline scope. |
| Other/missing/duplicate follower mappings, HP or class | Physical allocation, speakers, guards and replaced pointers differ; logical names do not guarantee distinct living actors. |
| Mode 2 or F64 with RAFT_MAP=22 | Main/follower/slot31 behavior differs; nominal ORB154 can actually name a raft. |
| Shared size changed, deferred/skipped update, replaced/uncompleted concurrent action | No persistent scale or simultaneous six-actor completion follows; installer destination-scan bypass still applies to the unmodified moves. |
| Stalled text/fade/action service | Earlier writes can exist without later dialogue, F776 or control; no rollback/all-actor barrier. |
| Pending request before WaitForEvent | Pending work wins over controller installation; no protected useful-control interval. |

These are static discriminators, not executed scenarios. **Unknown:** natural reach; supplied flags,
setup, party/HP/class/lookups, mode/raft/pending state; completed services and concurrent actions;
rendered ORBs/shadows/size/pose/NPC performance; heard sound; useful control and save persistence.
They create no automatic runtime queue under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). Later local interactions and
postbattle/Battle22 content remain separate; neither campaign nor
[epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514) is complete.
