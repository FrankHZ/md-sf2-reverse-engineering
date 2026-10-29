# New Granseal Janet recruitment and departure

Evidence date: 2026-10-14. **Confirmed:** town zones `(19,29)` and `(20,29)`
run Janet's scene when F805 is set and F806 is clear. The scene allocates a
current Janet entity, stages the conversation, joins ally18 to the force,
requests three following actions and returns; only then does the caller set806.
Active-party capacity and actual actor mappings remain separate conditions.
No Cannon, Dynamite, F804, F70 or Elric-membership test admits this scene.
**Inferred:** Janet asks to accompany Elric and the party accepts her.
**Unknown:** natural reach, supplied state, completed services/presentation,
usable control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #973](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/973)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one local departure/recruitment outcome, separating admission, physical actors,
force/active flags, current following and fresh population. This document alone
is owned. Accepted dependencies are [town setup/Peter introduction](../eastern-parmecia/new-granseal-town-through-peter-introduction.md),
[castle return/Astral following](./new-granseal-return-and-astral-following.md),
[inventor/Dynamite](./new-granseal-inventor-and-dynamite-gift.md),
[common stats](../../party/common-stats.md), [common scripting](../../world/common-scripting.md),
[request state](../../world/map-event-request-state.md) and
[request consumption](../../world/map-event-request-consumption.md).
No unmerged work is evidence. Stop at local zone return and necessary immediate
fresh-entry consequences. North Cave travel/blast, later battle, full town NPC
catalogs, acquisition/runtime, engine/UI/translation and shared owners are excluded.

Paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Reused fixtures bind USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Unqualified town files mean `data/maps/entries/map06/mapsetups/`; map/entity
engines mean `code/common/scripting/map/` and `code/common/scripting/entity/`.
Constants are from `sf2enums.asm`, RAM symbols from `sf2const.asm`. Fnnn denotes
flag nnn; intervals are half-open; command indices are retained corpus records.

Minimum evidence is selected pinned source, existing public fixtures and the
retained 18,285,979-byte map-init/map-script-engine pair. A single linear
comparison is budgeted below one minute/512 MiB, with small local receipts and
prose. No regeneration, new framework, installation or cleanup is needed.
Replan material source contradictions or expanded machinery. Counts locate
records; complete ordered joins and actual consumers support these conclusions.

## Town population and zone admission

**Confirmed:** `mapsetups.asm`/GetCurrentMapSetup select the last applicable
Map6 setup: default, F701, then F702. All three pointer tables share the zone
table and `ms_map6_InitFunction`. Neither805 nor806 selects a town population.
The default includes a fixed Peter7 `(18,25)`, Up, eas_RightLeftLoop; the701/702
lists have no fixed Peter. An existing follower7 mapping suppresses that fixed
ally declaration. Reference139 is a guard, at `(11,12)` Left/eas_Init in default,
`(13,24)` Up/eas_InitSlow in701, and `(18,28)` Up/eas_InitSlow in702. It is not
follower B31 or the castle's Astral139. No selected resident list declares Janet.

Shared init has eight operations. F802 set runs cs_54E70: set132 `(11,18)`,
Down; end; return. Otherwise F706 set runs cs_54E78: set138 `(63,63)`, Down;
set139 `(15,23)`, Down; end; return. Both clear simply returns. These are current
lookup writes, not removal or changed resident declarations. No806, recruitment,
follower construction or egress mutation belongs to this init.

InitializeMapEntities clears/rebuilds mappings and allocates enabled followers
before residents, then physical0 at the admission tuple. Map6 uses the ordinary
`table_Followers`, without the overworld Caravan prefix. F67 supplies Peter;
F74/B/Astral, F68/B/Oddler and later F70/B/Astral can each allocate a physical
follower, with later rows overwriting B's lookup without removing earlier
physical entries. Other enabled rows affect allocation and order. F70 clear
alone does not establish B's absence; F805 does not construct or validate B.
The accepted town and return owners retain the complete declaration order.

`s3_zoneevents.asm` ($54CD0..$54D3C) has five explicit rows and a default.
Zero-based rows3/4 are `(19,29)`/`(20,29)` → Map6_ZoneEvent3. The complete
callback is seven operations at `$54D24..$54D3C` (source58–71):
`chkFlg805; beq.s Map6_DefaultZoneEvent; chkFlg806; bne.s Map6_DefaultZoneEvent;
script cs_55288; setFlg806; rts`. The default is exactly the shared final
`rts` at `$54D3A..$54D3C`, not another entry into recruitment.

Thus805 clear returns without testing806;805 set/806 set returns without
repairing Janet membership or following;805 set/806 clear admits the scene.
No802,804,70,761, item, Elric13, Janet18, HP, party-capacity or actual-Astral
predicate is present here. Earlier town zones remain separate callbacks.

A matching table coordinate is not natural reach. Walking's
`entityscriptengine_2.asm:loc_5294` must submit event6 through layout attribute
$1400 after movement admission. The request is a single mutable slot.
ProcessMapEvent clears its type before ProcessMapEventType6_ZoneEvent installs
eas_Init on physical0, loads the coordinate words and invokes RunMapSetupZoneEvent.
The wrapper reselects the setup and its zone table but does not rebuild actors.
Carried flags, player mode, current actors, HP and request survival are premises.

## Complete scene and encoded actions

**Confirmed:** `scripts.asm:299–352`, cs_55288, contains45 records at
`$55288..$5537E`:228 bytes of command records plus18 inline-action bytes,
246 bytes total. There are no scene branches. B=FOLLOWER_B31, P=Peter7,
J=Janet18 and Bowie=ally0; these selectors resolve through current mappings.
All 45 records, including the three inline sequences, are accounted for below.

| Indices | Exact ordered operations |
| --- | --- |
| 0–4 | textCursor1126; newEntity J `(16,22)`, Down, sprite operand18; setActscriptWait B/eas_Init; setActscriptWait P/eas_Init; nextSingleText `$00,J` (1126). |
| 5–9 | setActscript Bowie/eas_Jump; setActscript B/eas_Jump; setActscriptWait P/eas_Jump; setFacing139 Up; setCamDest `(14,17)`. |
| 10–15 | setDest Bowie `(19,28)`; setDest B `(20,28)`; setDest P `(20,27)`; setFacing Bowie Up; B Up; P Up. |
| 16–20 | entityActionsWait J: Down2, Right3, Down1, endActions; setCamDest `(14,22)`; setFacing B Left; nextSingleText `$C0,B` (1127); setFacing B Up. |
| 21–25 | nextSingleText `$00,J` (1128); nextSingleText `$C0,P` (1129); headshake J; nextText `$00,J` (1130); nextSingleText `$00,J` (1131). |
| 26–30 | setFacing P Down; csWait50; entityActionsWait P: Right1, Down1, endActions; setFacing P Up; nextSingleText `$C0,B` (1132). |
| 31–35 | entityActionsWait J: Down1, endActions; nextSingleText `$00,J` (1133); setFacing Bowie Right; setFacing B Left; nextSingleText `$C0,B` (1134). |
| 36–40 | nod Bowie; setFacing Bowie Up; setFacing B Up; nextSingleText `$00,J` (1135); join J. |
| 41–44 | followEntity P,Bowie,2; followEntity B,P,2; followEntity J,B,2; csc_end. |

`sf2cutscenemacros.asm` encodes word opcodes; the selected operand layouts are:

| Macro / opcode | Operands after opcode; complete record size |
| --- | --- |
| textCursor / $04 | text-index word;4 bytes |
| newEntity / $2B | selector word, X/Y/facing/sprite bytes;8 |
| setActscript[Wait] / $15 | actor byte, wait byte0/$FF, script-address long;8 |
| nextSingleText / $00; nextText / $02 | modifier byte then actor byte;4 |
| setFacing / $23 | actor byte, facing byte;4 |
| setCamDest / $32 | X word, Y word;6 |
| setDest / $29 | actor, X, Y words;8 |
| entityActionsWait / $2D | actor byte, wait byte$FF;4 before inline payload |
| headshake / $27; nod / $26 | actor word;4 |
| join / $08 | ally/modifier word$0012;4 |
| followEntity / $2C | follower, leader, direction words;8 |
| csWait50; csc_end | $8032; $FFFF respectively;2 each |

Inline direction/count byte pairs are `$03,2; $00,3; $03,1; $8080`,
`$00,1; $03,1; $8080`, and `$03,1; $8080`:8/6/4 bytes. They are consumed by
csc2D, not extra top-level records. That handler generates relative move/wait
operations and an eas_Idle tail in action RAM, then waits for the addressed
entity's action pointer to become eas_Idle. The moves use tile-sized displacements
(384 internal units); actual movement/collision/update completion is a premise.

csc15 installs its action pointer and initializes its read timer from the
resolved physical index. Wait variants busy-wait for eas_Idle; the other two
jump installations return immediately. eas_Init sets speed32, zero acceleration
factors, disables acceleration/deceleration, enables obstruction, disables
entity/map uncollidability, enables auto-facing, orients Up, clears immersion,
sets size24, updates the sprite and jumps Idle. eas_Jump disables auto-facing,
requests relative `(0,-1)`, wait5, `(0,1)`, wait5, `(0,0)`, then enables
auto-facing and jumps Idle. Waiting for P alone is not a join of all jump actions.

csc29 has no HP guard. It writes scaled destinations and signed velocity32/
absolute travel for both axes, then waits for that physical actor's position
to equal destination because these selector words have bit15 clear. csc32
sets VIEW_TARGET_ENTITY=-1, requests its tile destination and waits scrolling.
There is no explicit camera-track command at scene end.

For distinct valid mappings and completing actions, the staged destinations
are Bowie `(19,28)`, B `(20,28)`, P `(20,27)`. J's first sequence moves from
`(16,22)` to `(19,25)`; later J reaches `(19,26)`, and P reaches `(21,28)`.
These are conditional instruction-derived positions before following, not
observed endpoints or a guarantee about actors already moving or aliased.
The scene requests final Bowie/B/P Up; relative movement determines J's facing.

## Actual allocation, HP and lookup boundaries

**Confirmed:** csc2B calls InitializeNewEntity with eas_Init and no HP,
existing-map, force-membership or capacity guard. For positive ally18,
GetAllyMapsprite replaces the supplied sprite byte18 with the ally's actual
class-dependent sprite: Janet base/promo/special45/46/47, or Blue Flame190
when current HP is0 outside battle. Sprite operand18 is not a promise to use
mapsprite18 (Peter). The getter has no join requirement and does not heal.

InitializeNewEntity scans63 lookup bytes, takes the maximum positive signed
physical index, adds1, overwrites Janet's lookup and calls DeclareNewEntity.
It does not reuse an existing Janet or search an empty physical slot. The
32-byte entity declaration writes coordinates/destination, zeroes velocity,
travel and acceleration, writes facing/layer/sprite/action/flags and initializes
counters; UpdateEntityProperties then updates the new physical entity. There
is no capacity rejection or old-entity removal. An already allocated Janet
can remain as an older physical actor or follow-list entry while later script
references address the new one. Repeated admission is not safe deduplication.

GetEntityAddressFromCharacter masks selectors with$FF, maps negative references
through the enemy/NPC offset$60, reads ENTITY_INDEX_LIST and scales the physical
index by32. A cleared ordinary lookup0 aliases physical0. A hidden -1 mapping
has no safe-absence check. B/P missing, hidden or aliased is not rejected by
the scene; the same physical actor can receive several sequential operations,
with later destinations/actions overwriting earlier ones.

The HP policies differ by opcode; do not turn them into one scene-wide rule:

- csc23 reads the actual actor byte. It skips zero-HP named allies below30;
  B31 bypasses that check and139 has the enemy/NPC bit set. Thus dead Bowie/P
  facing changes can skip while B can still act on a mapping that aliases them.
- csc15 action installation, csc29 destination, csc2D inline action, csc26 nod,
  csc27 headshake and csc2B allocation do not use that HP guard. Dead Janet
  still allocates/moves/shakes/joins; this scene has no revival or HP write.
- csc2C reads `(a6)` as a **byte before reading the follower word**. All three
  supplied words ($0007/$001F/$0012) begin with0, so the shared guard tests
  Bowie's HP, not Peter's, B's or Janet's. Bowie HP0 skips each six-byte operand
  group and returns to dispatch; living Bowie admits all three regardless of
  the followers' own HP. This is a source-specific encoding/consumer fact.

Nod disables animation with counter-1, sleeps10, transforms/DMA-updates the
sprite, sleeps20, restores its sprite, sleeps10 and writes counter0. Headshake
performs seven iterations of its two transforms/sprite restores with six VInt
waits per iteration, then counter0. Neither validates identity or joins all
movement. These are requested services, not measured rendered gestures.

## Dialogue, consent and force membership

**Confirmed:** cursor1126 joins ten consecutive exact `gamescript.txt` keys
`0466..046F` to the scene comments/literals. The table paraphrases the content;
controls are ordered and exact. Join uses explicit-ID446 (`01BE`).

| ID / speaker | Content | Ordered controls |
| --- | --- | --- |
| 1126 / J `$00` | Found the leader; asks identity | `{N}{LEADER}{W1}` |
| 1127 / B `$C0` | Asks her identity | `{W1}` |
| 1128 / J `$00` | Names her love Elric and asks whether he is with them | `{N}{NAME;13}{N}{W1}` |
| 1129 / P `$C0` | Says he joined willingly | `{W1}` |
| 1130 / J `$00`, nextText | Wants to accompany him, not stop him | `{N}{N}{W2}` |
| 1131 / J `$00` | Pleads to be taken along | `{N}{W1}` |
| 1132 / B `$C0` | Warns about the journey | `{N}{N}{W1}` |
| 1133 / J `$00` | Insists despite the warning | `{N}{W1}` |
| 1134 / B `$C0` | Accepts her as tough enough | `{N}{W1}` |
| 1135 / J `$00` | Thanks the party | `{N}{W1}` |
| 446 / join context | NAME, CLASS and joined-force announcement | `{NAME}{CLASS}{N}`; no W1/W2 |

csc00/02 use actual mapped sprite properties for speech/portrait, not the
selector's narrative identity. Janet45/46/47 gives portrait18/bleep4; Peter18/19
portrait7/48 and bleep4/5; Astral209 portrait31/bleep6. Blue Flame has no matching
row and gets default portrait-1/bleep6. Missing/aliased B borrows its actual
mapped sprite's context. Modifier$C0 requests right-side/mirrored portrait;
$00 requests neither. csc1D does not replace an already-open portrait.

Ordinary csc00/02 wait scrolling, display and increment the cursor. csc00
then closes portrait/dialogue and sleeps10; csc02 leaves the windows for
continuation. Hence1130's W2 continues into1131 with the same Janet context.
N is newline; W1 waits for directions/A/B/C after its VInt wait, while W2 uses
the continuation/validation-sound path. LEADER rebuilds force lists and names
the first active member, not necessarily Bowie or the map's physical0; an
empty active list has no guarded fallback here. NAME;13 uses the explicit
ally13/Elric name, without checking membership. Renamed ally records affect
names. The script's claim that Elric joined is dialogue, not an admission test.
There is no yes/no opcode or player consent branch.

ExecuteMapScript clears the skip latch at entry; debug P2 Start can set it.
Skipped csc00/02 consume operands without displaying or incrementing the cursor,
and csWait50 is skipped. Allocation/actions, gestures, join, following and
caller806 are not gated by that latch. The join announcement is direct `txt`,
not csc00/02, and still has its own music/input path.

csc08 first writes speech0 and waits scrolling, reads$0012, clears/tests bit15
and requests MUSIC_JOIN19 (not the sad variant). It calls j_JoinForce for18,
gets Janet's current class, writes DIALOGUE_NAME_INDEX_1=18 and INDEX_2=class,
and displays446. DisplayText resets its substitution pointer; NAME then CLASS
consume these two words via the corresponding name services. The special128
Sarah/Chester path is not selected. Join does not initialize stats, class,
items, AI or HP, and the class in the announcement is supplied state.

`code/common/stats/battleparty.asm:JoinForce` sets joined flag18, calls
UpdateForce, then compares the rebuilt active count unsigned against
FORCE_MAX_SIZE12. Count below12 calls JoinBattleParty and sets active flag50;
count12 or above skips that call. UpdateForce scans joined IDs0–29, adding
all to TARGETS_LIST and dividing them by active flags32–61 into active and
reserve lists, irrespective of HP. JoinBattleParty only sets the active bit;
it does not rebuild the lists again. The active flag may therefore be newer
than the cached active/reserve lists until the next UpdateForce.

Capacity does not reject joining: with12 active members and Janet inactive,
she joins in reserve. Already joined Janet is not rejected and still receives
the announcement; if inactive with room, she becomes active. Already active
Janet remains active even at/above capacity, and no existing active bit is
cleared. An unjoined Janet with a pre-set active bit enters the rebuilt active
count immediately; this routine does not repair such inconsistent inputs.
Neither Janet's active membership nor her HP controls the later map-follow commands.

After446, j_FadeOut_WaitForP1Input resolves through s05_jumpinterface to
battlefunctions_0.asm: preserve d0.w; request previous music$FB through
PlayMusicAfterCurrentOne; call WaitForPlayerInput; restore d0.w; return.
The music service submits WAIT_MUSIC_END plus the requested command and polls
WAIT_FOR_MUSIC_END with Sleep3. No palette fade is performed by this helper.
Input accepts a currently held direction/A/B/C/Start, otherwise waits VInt;
no new press/release is required. csc08 then closes dialogue, sleeps10 and
returns. It has one such input-service call, not the inventor helper's two.
Membership changes precede announcement/music/input completion.

## Current following, local return and fresh entry

**Confirmed:** with Bowie's HP guard admitted, csc2C independently resolves
both selector words on each call. Direction2 indexes `table_FollowerPositions`
and supplies signed pixel offsets `(-24,0)`. AddFollower receives physical
indices, installs the current ENTITY_WALKING_PARAMETERS as the action pointer,
copies42 bytes of eas_Follower1 there, advances the buffer, and patches the leader and two
offsets at$1E/$20/$22. It then scans EXPLORATION_ENTITIES to its -1 terminator:
already-listed physical followers are not appended or reordered; otherwise
it appends the index and a new terminator. Allocation/installation happens
before deduplication. No list-capacity, self-follow or cycle guard is present.

For distinct valid mappings this requests P→Bowie, B→P, J→B in that order.
It does not use the actual last list entry as leader, and does not imply that
P,B,J are adjacent in the list. Existing extra followers remain. If B maps
to physical0, the second call installs Bowie following P after P was made to
follow Bowie; if P and B alias, the second can overwrite the first action with
a self-follow action. Janet's new mapping does not remove an older Janet from
the list. These are installed dependencies, not a guaranteed completed chain.

The copied action uses speed32, zero acceleration, disabled obstruction and
enabled map/entity uncollidability, then loops ac_follow/ac_waitDest. The actual
`entityscriptengine_2.asm:esc03_follow` reads its stored physical leader each
iteration. At maximum axis distance≤$1E0 (30 pixels) it advances without
changing destination; above that it uses the leader's destination/facing,
rotates the signed offsets through sub_5FAC and checks layout<$C000. Its
fallback retries without relative Y, then uses the leader's destination.
Here relative Y is already0, so the first two candidates coincide. It writes
follower destination/velocity/travel, then waits destination before looping.
It neither snaps actors into a line nor re-resolves character mappings later.
No final follower coordinates, layout passability or completion are asserted.

csc_end conditionally waits scrolling if dialogue remains open, clears
VIEW_SCROLLING_SPEED and restores registers. Only after it returns does the
caller set806 and reach the shared rts. RunMapSetupZoneEvent closes portrait,
closes dialogue, waits one VInt and waits for **physical0** position to equal
destination, then restores registers and returns. It does not wait all followers.
These zone wrappers do not deactivate/reactivate VInt_UpdateEntities as the
inventor's entity-event wrapper does; action completion requires the existing
update service. No speech/name-state restoration or save transaction is added.

After local return, ExplorationLoop calls WaitForEvent again. Only with no
pending event does it restore camera target0 and install the mode-selected
controlled action on physical0. This can overwrite a physical0 follow action
created by an alias; it does not repair the list or prove the other actions
completed. A pending request bypasses that reset. The scene does not normalize
PLAYER_TYPE. Usable control is therefore a later conditional boundary.

A fresh ordinary town entry rebuilds followers/residents, reapplies the same
701/702 setup and802/706 init, and performs the accepted exploration healing
with its limits. There is **no Janet row** in table_Followers or any of the
three town resident lists. Joined18/active50 and806 do not by themselves
recreate her town entity or this three-call chain. F67/70 and the other follower
flags reconstruct their ordinary order;805/806 do not repair missing67/70.
Those are local fresh-Map6 consequences, not a claim about every other map.

| Changed state / interrupted prefix | Consequence at this boundary |
| --- | --- |
|805 clear, regardless of items,70 or Janet membership | Return with no scene or new806. |
|805 set/806 set, Janet absent or unjoined | Return; no recruitment, repair or follow installation. |
|805 set/806 clear,804 clear or no Cannon/Dynamite/Elric | Still admitted. No inventory is searched or consumed. |
|70 clear or later setup without67 | Missing B/P can alias0; earlier B declarations or default fixed Peter can instead supply them. Admission is unchanged. |
|Janet already allocated/joined/dead | Allocation is still attempted, mappings may be replaced, speech follows actual sprite; joined-bit write and announcement repeat without revival. |
|Bowie dead, other actors alive | Named Bowie facing skips and all three follow calls skip; unguarded allocation/actions/join can still execute if their services return. |
|Bowie alive, P/J dead | Following remains admitted; byte-facing checks still differ. Full active party can leave Janet in reserve while she follows locally. |
|Interrupt after newEntity but before join | A Janet map entity can exist without new joined18 or806. A retry can allocate another. |
|Interrupt after joined18 but before active50 or announcement return | Membership, active state/caches and acknowledgement may disagree;806 is still clear. |
|Interrupt after join return or between follow calls | Recruitment can be complete while current following is absent/partial and806 clear. |
|After all follow calls but before caller806 | Installed following does not suppress a later eligible replay. |
|After806 before wrapper/control return | The flag suppresses another scene although windows/movement/control may remain incomplete. |
|Fresh town entry with806 set | No scene replay; ordinary reconstruction does not retain Janet's temporary map entity/following. Force flags remain distinct. |

There is no explicit item/HP/revival, warp, battle, egress or follower-unlock
flag command in this scene; the deliberate force writes are18 and conditional50,
followed by caller806. Ordinary service RAM, sprites, text, camera and movement
still change. Neither dialogue, joined membership nor806 is an atomic persisted
completion claim. The next town-exit boundary is the separate south warp row
`(any,31)` → Map73 `(29,53)`, Down, no scroll; this scene does not issue it.
No North Cave journey or use/blast is analyzed.

**Unknown:** natural zone reach, supplied flags/player mode/force/HP/class,
physical allocation/mapping and capacity, follower-list validity/order,
movement/collision/DMA/text/portrait/music/input completion, visible recruitment,
usable control and save/load persistence. None is an automatic runtime queue.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same process as controlled commands.
Reuse the [retained pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture`, and `_cutscene_facts`.
Keep both payloads and all completed receipts read-only; no extraction rerun.

1. Join all three Map6 setup pointers, shared eight-operation init, complete
   cs_54E70/cs_54E78 records, selected Peter/139 declarations and follower rows
   against retained facts/source. Verify no Janet declaration at fresh entry.
2. Join both Janet zone rows and the default, complete seven-operation
   Map6_ZoneEvent3 and one-operation default in
   `tests/fixtures/h2/map-events/zone-programs.json`, preserving source lines,
   addresses, operands and branch polarity. Do not count shared return twice.
3. Join all45 cs_55288 records to pinned source and retained programCorpus;
   inspect every macro layout plus all three inline sequences and246-byte
   extent. Join all ten exact literals/comments/control sequences by hex ID,
   plus explicit446 and its substitutions; publish paraphrases only.
4. Read actual allocation/sprite/lookup, byte/word HP guard, action/movement,
   force/capacity, follower insertion/consumer, text/portrait/music/input and
   zone/control return bodies named above. Check changed alias/HP/membership/
   capacity/admission states and interrupted prefixes against instructions.
   Follow jump aliases; caller fixtures alone do not assert callee algorithms.
5. Review complete prose/diff, local links/anchors, UTF-8, sole-path/private
   boundary and `git diff --check`. On a clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record actual
   exact-head CI in the Issue handoff. Documentation-only policy uses direct
   checks, not blanket normal/full/engine/Godot/H3/H4 suites.

Preserve [#969 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/969#issuecomment-6179831287)
and [PR971 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/971#issuecomment-6179847971):
author0.081s and independent0.086s unchanged-pair receipts and CI38647099720.
Their linked #965/PR968 records retain the distinct prior receipts, initial/final
CI, Battle19/32-byte-stride corrections and #954 callback/#962 Fairy corrections;
linked earlier failures, results and Unknowns remain intact. No completed
failure is reset or reclassified. Freeze a clean pushed Draft PR for independent
review; no merge, cleanup or next Issue.
