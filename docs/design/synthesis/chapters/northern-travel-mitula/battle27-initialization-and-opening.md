# Battle27: initial formation and the restrictions confrontation

Three enemies challenge the force before Battle27's first activation step.
**Confirmed:** two Dark Soldiers ask where the visitors came from and whether
they know about the restrictions; a Lizardman dismisses ignorance as an excuse.
The questions have no player-choice command. Fresh entry constructs the actual
party and sixteen enemies before this opening, but neither the dialogue nor
their placement proves that they are activated, visible or ready to take a turn.

This guide continues [Pacalon departure and conditional Battle27 admission](pacalon-departure-to-battle27-admission.md).
Its fresh endpoint is `BattleLoop:@Start`, **before ActivateEnemies**. The
F88 continuation instead stops at `@IndividualTurns_Loop`, before individual-turn
execution. **Inferred:** the source name TO_TRISTAN and the exchange frame an
encounter about travel restrictions. **Unknown:** natural arrival, supplied
state, completed services, presentation, usable control and persistence.
No observed or remake frontier advances.

## What fresh entry does before the confrontation

**Confirmed:** the predecessor's ordered scan supplies Map74/Battle27 and crop
`(2,16,32,48)`. Battle27 precedes Battle32; a held completion flag 527 can clear
unlock flag 427 while still admitting 27 on that scan. Admission has not yet assigned
CURRENT_BATTLE. The following explanation requires the actual 74/27 handoff;
initialization does not rerun admission or repair changed crop bytes.

`BattleLoop` clears PLAYER_TYPE, sets F399 and checks F88. With F88 clear it:

1. Clears SECONDS_COUNTER, saves incoming map/battle **words**, assigns their
   low bytes to CURRENT_MAP/CURRENT_BATTLE and installs base VInts.
2. Calls the before wrapper. It checks 450+current battle, hence F477. Held F477
   skips it; clear F477 dispatches slot 27 of the 48-word before table to
   `ms_Empty`. Its only word is the terminator. Before does not set F477.
3. Restores incoming words and reassigns current map/battle bytes, then clears
   **all** region flags 90–105, not merely the four regions declared here.
4. Heals eligible allies, initializes actual party positions, initializes
   enemies and clears AI memory.
5. Clears d0.w and calls LoadBattle for combatant 0, then calls the start wrapper.
   Returning reaches the fresh pre-ActivateEnemies endpoint.

Empty before still enters ExecuteMapScript, clears its text-skip latch and
takes the shared terminator tail. A source call is conditional on its services
returning. Interruption preserves reached writes rather than rolling them back.
F477 held skips both hooks without skipping fresh reset, healing, formation
or load. Before and start sample independently; a changed flag between them
can alter the second decision. Later consumers independently reread
CURRENT_BATTLE. Restoring the arguments after before repairs a stale fresh
selector, not all later selector changes.

## Twelve places do not name twelve party members

**Confirmed:** healing scans all 30 allies independently of membership. Living
allies, and always Peter 7/Lemon 28, receive maximum HP/MP; other dead allies are
skipped. Status is first masked to STUN|POISON|CURSE (`$0007`), then derived
stats, status/equipment effects and curse are recomputed. That intermediate
mask is not a promise about final curse. Healing performs no recruitment,
promotion, inventory replacement or active-party selection. HP is a word;
MP and position fields are bytes.

The ally initializer first sets every ally's X to -1 and calls UpdateForce.
Membership flags rebuild joined/active/reserve lists in ascending ally-ID order;
a supplied stale list is not authoritative. The header declares **12 ally
placement slots, 16 enemy rows, 4 regions and 1 point**. The
[complete twelve-position table](../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#actual-party-and-all-twelve-placement-slots)
places living active members around local X14–17/Y44–46. Each row supplies
X/Y to the next eligible member, not an ally identity. Its HEALER1, NOTHING,
NONE/region 15, filler0 and STARTING fields do not overwrite the ally's AI,
inventory or other state.

A dead active member consumes a processed-member slot but leaves the placement
pointer unchanged, so the next living member uses the unused earlier position.
Twelve bounds processed members, not twelve guaranteed actors. Surplus active
members pass through LeaveBattleParty, clearing their active bit and setting
X=-1 without immediately rebuilding the list/count. Fewer eligible members
produce fewer actors. The zero-member count is decremented before the first
member read, without an empty-list guard; arbitrary inconsistent empty state
is not established safe. Bowie is not guaranteed by the formation count, and
Higins 19 participates only when actual membership and HP permit it.

## Six enemy types, sixteen initialized rows

**Confirmed:** BattleSpriteset27 is 390 bytes: header 4, ally rows 144, enemy
rows 192, regions 48 and point 2. CURRENT_BATTLE selects its pointer; subsection
offsets come from that selected header. Each combatant row is 12 bytes, including
type/ordinal, X/Y, commandset, item word, two order/region pairs and the
filler/spawn pair read as a word. The
[complete sixteen-row table and encoded results](../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#complete-enemy-declarations-and-initialization)
remain the row authority. This compact reading groups their identities:

| Type / stored ID | Rows (combatant 128 + row) | Role of the declarations here |
| --- | --- | --- |
|Lizardman (19)|0,8,14|Row 0 is LEADER with MOVE_TO point 0/region 3; other rows carry ATTACKER1 orders.|
|Dark Soldier (18)|3,4,5|Three separate combatants; rows 4/5 are the two Soldier speakers.|
|Wyvern (46)|1,6|Two flying combatants with different stored orders/regions.|
|Dark Sniper (62)|7,9,11,15|Four separate follow-order declarations.|
|Wizard (72)|10|One ATTACKER3 declaration following enemy row 4.|
|Dark Bishop (81)|2,12,13|HEALER5/HEALER4 declarations, with their own primary/secondary references.|

These commandset names, counts and references do not prove tactics or actions.
All rows have item NOTHING and filler 0/STARTING 0, making every initialization
word `$0000`. All spawn nibbles are below HIDDEN2, so rows 128–143 call
InitializeEnemyStats. Battle32's Jaro exception does not skip Battle27 row 15.
Fresh initialization calls no starting-position occupancy check;
ResetSpawningEnemyStats belongs to a separate later consumer.

The six [definition, weapon, spell and stat tables](../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#definitions-weapons-spells-and-stats)
explain the initializer's actual inputs. Each 56-byte definition is copied as
14 longwords, with current HP/MP set from maxima, position/type written and
movetype/commandset packed into separate nibbles. All have ENHANCED spell power
and no initial status. Soldiers/Lizardmen retain equipped Large Axes, Snipers
Hyper Cannons, Wizards/Bishops Flails; Wyverns have no weapon. Row NOTHING
returns before AddItem/equipping and does not erase these copied weapons or
add a Black Ring. Soldier ATTACK, Wyvern FLAME, Wizard BLAZE LV3 and Bishop
AURA/BLAST LV3 are declarations, not evidence that a spell is used.

Difficulty adjustment has consecutive `beq` instructions after the SUPER
comparison. Every difficulty applies floor(base ATT*5/4), and SUPER applies
it twice. Equipped effects then add 25/40/19 ATT for Axe/Cannon/Flail. For
example, Soldier base/current ATT is 38/63 normally and 47/72 on SUPER; the
owning table gives all six results. DEF/AGI/MOV remain the declared values
under these no-initial-status definitions. This establishes conditional
initializer stats, not damage, hit rates or intended difficulty.

Activation retains definition `$F000` and ORs the rotated initialization word
masked `$0FFF`: Bishop rows 2/12/13 start at `$3000`, all others `$2000`.
Definition BIT4/BIT5 are discarded; primary/secondary-active, neutral and
spawn-mode bits are clear. Trigger bytes and move-order words are packed
storage, not executed activation. Enemy slots 144–159 receive activation 0,
max/current HP0 and X=-1, but are not fully cleared: their unchanged subsection
pointer reads row 0's trigger pair, producing `$33`; Y/MP/items and other fields
are not universally reset. ClearAiMemory separately fills 48 last-target bytes
with `$FF` and 48 memory bytes with 0; it generates no turn order.

Battle27 is absent from the complete random-upgrade list, so upgrade selection
rejects it before completion/level/RNG checks. Held F527 does not upgrade these
definitions. It is also absent from the neutral table: clear F527 finds no group,
held F527 skips scanning. Neither path creates a neutral Higins actor. Battle26's
upgrade/neutral facts cannot be transferred here.

The [four regions and point 0](../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#four-regions-and-one-point)
are battle-local declarations. Regions0–3 correspond to F90–93; type 4's
consumer uses triangles 1/2/4 and 3/2/4 from the four vertex words. Second/trailing
bytes are not geometry operands. Region15 is the no-trigger sentinel, not
another polygon. Point0=`(15,23)` is row 0's MOVE_TO reference. FOLLOW_ENEMY
operands refer to combatants 128/137/136/133/132/138/142. No region evaluation,
target choice, path or movement executes within this guide's endpoint.

## A rebuilt battlefield, not the retained overworld cast

**Confirmed:** LoadBattle preserves its d0 combatant argument, selects actual
CURRENT_MAP, requests fade-out, sets view target -1, loads tilesets, waits for
fade, clears VInts, waits a VInt and calls PositionBattleEntities.
ClearEntities resets 49 physical 32-byte records, all 64 lookup bytes and walking
allocation state. Earlier exploration actors, aliases and scripts are discarded.

The physical scan covers 30 allies and 32 enemies. Nonnegative signed-byte X/Y
permits declaration; neutral-marked enemies are skipped. **There is no HP
test in this scan.** Fresh initialization normally supplies the HP/position
relationship; arbitrary supplied state need not. Positions become
`(X & $3F)*384,(Y & $3F)*384`, facing Down3 with eas_Standing. Declaration writes
actor fields, clears travel/velocity/acceleration, sets flags `$7040` and
initializes animation/wait counters. Installing an action proves no completed pose.

Actual ally class selects its sprite, including the Rohde membership exception;
battle selection does not use exploration's dead-ally BLUE_FLAME branch.
Enemy stored types select Soldier 82, Lizardman 83, Wyvern 110, Sniper 126,
Wizard 136 and Bishop 145. They all use sequential allocation; Wyvern rows 1/6
have aerial layer 1, the others layer 0. With N declared allies, combatant 128+r
maps to physical N+r, hence enemies 12–27 for N=12. Ordinary fresh population
is at most 28 and has no neutral group. That bound is not a capacity guard:
DeclareNewEntity has none, and arbitrary supplied positions can exceed it.

Skipped slots receive lookup-1; ally-extra offsets 30/31 retain ClearEntities's zero.
Enemy IDs128–159 use lookup offsets 32–63 by subtracting 96. Logical combatant,
physical actor and sprite are distinct identities. LoadBattle restores d0=0,
uses Bowie's lookup for VIEW_TARGET_ENTITY and samples chosen tile X/Y. A
negative lookup falls back to physical 0 for those reads, without creating Bowie
or repairing membership.

LoadMap receives **d0=-1** for target-based view placement but **d1=74** for a
full map load, unlike its negative-d1 CURRENT reload. Map74 selects palette 0,
tilesets 101/102/109/110/105 and one descriptor:0..63 bounds, zero layer origins/
autoscroll, parallax 256/256, layer type 254 and MAIN_THEME1. Descriptor selection
compares target coordinates with bounds*384 without adding crop; the sole
descriptor is also the fallback. Blocks/layout, flag copies and chest overlays
load before the supplied battle rectangle is copied to working origin 0/0.

For the accepted crop,32*48=1536 words/3072 bytes are copied using **64-wide
storage rows**. Battle area ends at local `(31*384,47*384)`; other layer/music
metadata still comes from the descriptor. Changed BATTLE_AREA bytes are not
repaired. Formation, enemy, region and point positions are already local:
source-layout correspondence adds `(2,16)`. Placement0 `(14,44)` corresponds
to `(16,60)`, enemy row 0 `(15,19)` to `(17,35)`, point 0 to `(17,39)`.
These arithmetic joins do not decode a traversable route.

The suffix waits a VInt, loads mapsprites, installs base VInts, loads the
CURRENT_BATTLE-selected BattleTerrain27 into BATTLE_TERRAIN_ARRAY and requests
map music/fade-in. MAIN_THEME1 is not substituted, so music 1 is submitted;
Battle44's timer branch does not apply. Terrain selection does not publish a
terrain corpus or establish movement legality. Source order proves neither
completed rendering/audio nor usable control.

## The complete opening: two Soldiers, then the Lizardman

**Confirmed:** the start wrapper checks 450+current battle, sets F477 **before
dispatch**, then selects the current battle's slot in its separate 48-word
table. Slot27 selects `bscs_battle27`. The flag and later table selector reread
CURRENT_BATTLE independently; changing it can separate flag identity from
program identity. This scene requires 27 at selection. Held F477 skips dispatch.

The complete 34-byte program has nine commands, with no local actor creation,
movement, damage, recruitment, inventory, outcome, story-flag or map request:

| Command index | Request |
| --- | --- |
|0|Set text cursor 2658.|
|1|Set camera speed word `$30` (48).|
|2|Set camera target to logical combatant 133.|
|3|Display next single text, portrait-control byte `$00`, speaker 133.|
|4|Set camera target to 132.|
|5|Display next single text, `$00`, speaker 132.|
|6|Set camera target to 128.|
|7|Display next single text, `$00`, speaker 128.|
|8|End (`$FFFF`).|

The accepted gamescript literals include the spelling **to bad**. `{N}` is
newline and `{W1}` the text wait; there are no variable substitutions:

| Text | Fresh logical speaker | Literal |
| --- | --- | --- |
|2658|133, row 5 Dark Soldier `(17,38)`|`Where did you come from?{N}What are you doing here?{W1}`|
|2659|132, row 4 Dark Soldier `(8,34)`|`Don't you know about the{N}restrictions?{W1}`|
|2660|128, row 0 Lizardman `(15,19)`|`Even if you didn't know,{N}that's just to bad.{W1}`|

The literal meaning is a challenge about the visitors' origin/purpose, a
reminder of restrictions, then dismissal of ignorance. **Inferred:** this
frames the force as encountering enforcement on the way toward source-named
Tristan. The script names no issuing authority or geographic extent for the
restrictions. No yes/no prompt, answer branch or player decision follows.

Logical 133/132/128 use lookup 37/36/32 and physical N+5/N+4/N; for twelve
declared allies these are 17/16/12, with sprites 82/82/83. Neither Soldier nor
Lizardman has a row in the complete sprite-dialogue property table, so fresh
resolution falls back to portrait `$FFFF` (none) and speech bleep 6=74.
It resolves the **current physical sprite**, not a text-supplied NPC name;
changed/aliased sprites can resolve differently. The helpers do not check HP.

Missing speakers are not safely skipped. Camera selection writes missing
lookup `$FF`, a negative target that the camera updater avoids dereferencing.
Dialogue's GetEntityAddressFromCharacter instead shifts unsigned `$00FF`
by 5 without a sentinel guard, yielding ENTITY_DATA offset `$1FE0` for these
operands. It does not establish a valid actor, portrait or speech read. This
unguarded lookup is separate from the camera's negative-target handling and
LoadBattle's physical 0 fallback.

Camera speed 48 is a stored word, not 48 frames or pixels. VInt_UpdateViewData
reads a nonnegative physical target, applies thresholds/bounds and requests a
destination. The nonzero override and descriptor parallax/autoscroll feed
scroll-speed words. WaitForViewScrollEnd checks across VInts. These consumers
explain the requests without proving a particular scroll or view completed.

Normally each nextSingleText first invokes portrait handling, resolves speech
again, writes CURRENT_SPEECH_SFX, waits for scroll and calls DisplayText.
It then increments the cursor, closes portrait/text and calls Sleep 10. Speaker
words `$0085/$0084/$0080` are not the `$FFFF` no-speaker sentinel; their portrait
control bits 15/14 are 0. Portrait opening also requires an empty portrait window
and a resolved portrait other than -1. Existing portrait state can bypass opening
even while speech resolves anew. Only normal completion of all lines leaves
cursor 2661. Sleep 10 is separate from `{W1}`; neither is measured rendered timing.

The real `{W1}` consumer saves/clears typewriting state and loops through
RNG-seed-copy work and WaitForVInt. It accepts **current** UP/DOWN/LEFT/RIGHT/
B/C/A input, not necessarily a new press; Start is not in that mask. Held input
and supplied state therefore matter. The line is not a guaranteed fresh
confirmation request.

ExecuteMapScript clears text skip at entry, but debug mode plus P2 Start can
latch it at the command loop. A skipped single-text consumes its operand
without display, cursor increment, closing or Sleep 10. Camera commands still
run; midscene skip can leave a prefix cursor. The terminator waits for scroll
conditionally when a dialogue window is open, clears camera speed and restores
interpreter registers. It does not restore Bowie's view target: after all three
camera commands the target remains row 0's mapping, subject to actual state.

F477 is a wrapper write, **not proof that the opening finished**. Interruption
after the write can suppress a later fresh replay of an incomplete scene.
Only returned services and wrapper reach the fresh pre-ActivateEnemies stop;
that stop proves no activated cast, player turn or battle result.

## F88 continuation uses supplied battle state

**Confirmed:** held F88 still clears PLAYER_TYPE and sets 399, restores saved
seconds, clears 88 and AI memory, clears VIEW_TARGET_ENTITY and calls LoadBattle
without explicitly clearing d0. It branches directly to
`@IndividualTurns_Loop`; the following branch to `@Start` is unreachable.
Incoming 74/27 are not assigned to current selectors. There is no region reset,
healing, membership/position/enemy initialization or hook replay, even with 477
clear. Current map/battle/crop, HP, items, positions, activation and turn state
are supplied inputs. Load still rebuilds physical actors and view, so this is
not byte-for-byte preservation or fresh-state repair.

At this **specific 74/27 admission entry**, flag traps/helpers and ClearAiMemory
preserve d0. Forcing F88 therefore carries incoming low byte 74 to
GetEntityIndexForCombatant and reads **lookup offset 74 outside the 64-byte list**,
rather than selecting Bowie 0. A changed incoming d0 changes this read;
LoadBattle's negative-result fallback does not validate the index first.
This conditional register result does not assert that natural suspend/resume
uses this entry or these registers.

The stop is before individual-turn execution. The next instructions use supplied
CURRENT_BATTLE_TURN as a byte offset into BATTLE_TURN_ORDER. A selected `$FF`
sentinel branches to `@Start`; otherwise ExecuteIndividualTurn follows. Merely
reaching the label guarantees neither a valid actor nor an immediate turn.
LoadBattle itself performs no round reset or turn-order resampling.

Continue with [Battle27 activation and the first turn handoff](battle27-activation-and-first-turn-dispatch.md)
only after the fresh opening wrapper returns. That guide distinguishes checked
regions, persistent flags, enemy activation, spawning and turn admission through
the first ExecuteIndividualTurn call, before its body; it also retains F88's
initial supplied-order bypass and sentinel reentry.

## Changed states and the supported boundary

| Changed premise | Consequence that must remain conditional |
| --- | --- |
|Incoming map/battle, stale selectors or later CURRENT_BATTLE change|Fresh restores arguments; resume uses current selectors; each later reader selects independently.|
|F477 clear/held or interruption after its write|Hooks can skip independently; fresh construction still runs; flag is not scene completion.|
|F527 clear/held|Neither creates a Battle27 upgrade or neutral group; completion is not the intro flag.|
|Dead/fewer/surplus/empty members; changed HP/class/status/items|Actual flags/list, processed-member count, placement pointer, stats and sprite readers govern; no guaranteed named twelve.|
|Difficulty, enemy type/position/neutral bits, crop or F88 changes|Conditional definitions/load/population differ; physical scan has no HP or capacity guard.|
|Missing Bowie, dead/missing/aliased speaker|View fallback, negative camera target and unguarded dialogue pointer are separate mechanisms.|
|Existing portrait, held accepted input or debug skip|Portrait opening, waits, cursor and visible text can differ while camera commands continue.|
|Interrupted reset/heal/load/text/camera/wrapper|Only the reached prefix is supported; no completed presentation or usable control follows.|
|Supplied turn index/order on F88|A sentinel can branch toward activation; no turn/round result is established here.|

Activation, region cutscenes, spawning, turn generation/execution, AI, actions,
combat, outcomes and later exploration/story remain unsupported by this guide.
Positive initial HP, row names and counts do not establish visibility, natural
participation, reachability or tactics. Optional compressed-layout absence
remains a no-decoded-route limit, not a ROM blocker or acquisition authority.
Natural supplied flags/state, timing/interruption, completed presentation,
usable control and save/load persistence remain **Unknown**.

## Evidence review and retained results

The complete accepted [Battle27 research](../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md)
is the **Confirmed static evidence owner**, accepted as
[#1033 / PR1036](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1036)
in `85016e1c27af3fd91cd82d0468b37c896c69f3b4`. Its pinned source is SF2DISASM
`master` at `c834c652b6862bc5679fd7f69a38a7093206efc6`; its provenance binds
the USA retail ROM identity in the root README. Source paths, addresses,
complete tables, literals and private comparisons remain owned there.
Fnnn denotes flag nnn; row/lookup indices are zero-based.

The complete accepted predecessor supplies admission, not proof of fresh
initialization. The [encounter contract](../../../contracts/battle/battle-encounter-definition.md),
[battle-control contract](../../../contracts/battle/battle-control-lifecycle.md) and
[dialogue contract](../../../contracts/interface/dialogue-system.md) retain their structural
and runtime limits. Their broad Unknown queues do not override the deeper
accepted Battle27 predicates or establish natural reach. No blocking evidence
conflict was found; a future conflict returns to research without a Layer B repair.

The pre-synthesis review checked the complete Layer A prose and every
changed-state row, then selected actual public payloads, schema shapes and
verifier consumers. It did **not** rerun private extraction:

| Fixture / actual consumer | Reviewed join and limit |
| --- | --- |
|`sf2-battle-spriteset-data-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_spriteset_data.py)|BattleSpriteset27 address/next-address extent and 45-slot structure; `rowContentTracked=false` does not prove its 28 combatant rows, regions or point. Those use the accepted deeper review.|
|`sf2-battle-global-data-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-global-data-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_global_data.py)|45 seven-field map rows and summary shapes; public neutral/random counts omit their actual battle lists. No 27 membership conclusion comes from these counts.|
|`sf2-battle-loop-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-loop-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_loop.py)|Actual healing/status-mask/30-and 32-slot/48-byte-memory/terrain facts and ordered source-fragment guards; not a full party, physical or turn simulation.|
|`sf2-battle-control-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-control-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_control.py)|F88, region reset 90–105, twelve-byte entries and turn sentinel 255; compact order facts do not prove this admission's d0, safe lookups or execution.|
|`sf2-battle-cutscenes-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_cutscenes.py)|Before checks without setting; start sets before script; actual current selector and ordered guards. Does not contain the nine-command body or text literals.|
|`sf2-battle-routing-data-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_routing_data.py)|Separate48-slot hook tables/45 terrain slots, terrain 32 alias 27; `routeTargetsParsed=false`. Aggregate one-nonempty-start count alone does not identify this program.|

Owning commands are `uv run sf2 h2 battle-spriteset-data`, `battle-global-data`,
`battle-loop`, `battle-control`, `battle-cutscenes` and `battle-routing-data`.
Source and the retained 18,285,979-byte map-init/script pair are unavailable in
this design checkout. The accepted 0.083s comparison is provenance only, with
no source copying, acquisition, regeneration or fresh private comparison here.

[#1033 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1033#issuecomment-6183638591)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1036#issuecomment-6183661030)
retain final direct 0.202s/independent 0.195s/CI38675636173 separately from
original 0.207s/CI38675382781, earlier 0.215s and schema-path/local-$ref guard
failures. The independent nonexistent-region-filename probe was corrected to
the existing owner; it changed no data. The rebased research was byte-identical.
[#1034 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1034#issuecomment-6183549178)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1035#issuecomment-6183600028)
retain author 1.414s/independent 1.433s/CI38674976874 and its corrected inspection/
checker diagnostics. The predecessor preserves #1029's distinct original/final
results, failures and linked earlier Unknowns; no completed suite is reset.

Standard rigor for [#1038](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1038)
keeps fresh/resume state, membership, physical identity and intro completion
separate. The [current agreement](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
owns the five Markdown paths and acceptance boundary. The handoff records
complete prose/diff/changed-state review, actual public-schema/consumer joins,
navigation/links/anchors/UTF-8/private checks, configured clean committed
planner and actual exact-head scoped CI. Documentation scope skips blanket
normal/full/Python/.NET/Godot/H3/H4/translation suites. Activation
research and UI692 remain unconsumed; there is no new runtime queue.
