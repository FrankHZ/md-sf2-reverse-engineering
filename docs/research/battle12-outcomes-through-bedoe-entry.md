# Battle12 outcomes through Bedoe entry

Evidence date: 2026-10-13. **Confirmed:** Battle12 has a leader-present tail, a
three-text defeated script that sets F714, and a first-win after script requesting
Map52 `(23,8)`, Left. Map52 init separately requires F714 set and F712 clear for
Peter's two-text response. The selected mountain exit requests Bedoe, Map10
`(12,30)`, Up. Its default, F719-clear init stages the guards' challenge and Peter's
recognition, then places guards 150/151 at `(34,51)` / `(35,51)` before the caller
sets F719 and optionally moves 150–156 out of map. **Inferred:** this is a candidate
content continuation from the accepted Battle12 opening. **Unknown:** natural
travel, supplied state, completed services, visible presentation and playability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #780](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/780), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Battle outcome, map selectors, entity identity and asynchronous scene services cross
existing owners and need explicit admission and stopping boundaries. This document
is the sole tracked change; no shared-path needs. Consume accepted
[Battle12 admission](./polca-fairy-through-battle12-admission.md),
[battle lifecycle](./battle-loop.md), [cutscene dispatch](./battle-cutscenes.md),
[map request consumption](./map-event-request-consumption.md) and
[common scripting](./common-scripting.md). Starting accepted base is
`ecbf9fff9c9f6fed2320a35019a158931dc7eb12`; no stack.

Scope is row 12 outcomes, `edcs_battle12` / `abcs_battle12`, Map52 init / `cs_5C622`,
the selected Map52-to-Map10 request, setup/population discriminators and `cs_570B0`
through the Bedoe init caller and first conditional restored control. Stop before
`cs_573EC`. Combat/AI audit, king/Volcanon scenes, the F722 scene, Skreech event,
later yes/no contract, recruitment, shops, floor traversal, Battle13, layout/path
extraction, runtime acquisition and new infrastructure are excluded. Higher-story
flags below are selectors only. No inseparable scope expansion is needed.

Source paths are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their existing fixture comparisons and the cutscene-source comparison pass. This
uses retained provenance; no fresh ROM extraction or scene observation is claimed.
Fnnn means flag nnn. Table rows and init `sourceOrder` are zero-based; script record
numbers are one-based positions in `programCorpus.programs[].commands`.

Reuse the retained 18,285,979-byte pair in one process with linear selected reads
and comparisons, estimated below 512 MiB peak and one minute, plus small prose and
receipts. Comparisons completed in 0.090 seconds. Map10 `scripts.asm` is 26,440 bytes /
637 lines; the selected scene is only lines 4–224, **221 lines**, ending before
`cs_573EC` at line 225. Map52's selected segment is lines 82–87, six lines. Inline
action payloads are separate from script records. No new payload, source copy,
parser, comparator or per-scene fixture is required. Reassess before expanding these
bounds. Unknowns do not authorize acquisition. Direct source/fixture/document checks,
the clean committed planner and actual final-head CI form this docs-only profile.

## Battle12 outcome and return order

**Confirmed:** `sf2enums.asm` defines `BATTLE_MOUNT_VOLCANO=12` and
`MAP_MOUNT_VOLCANO=52`. The relevant row 12 sources are:

| Source under `data/battles/` | Exact selection |
| --- | --- |
| `global/enemyleaderpresence.asm` | `-1`: leader present, a nonzero byte. |
| `cutscenes/battleendcutscenes.asm` | `edcs_battle12`, at `0x4ACC8`. |
| `cutscenes/afterbattlecutscenes.asm` | `abcs_battle12`, at `0x4ACEC`. |
| `cutscenes/afterbattlejoins.asm` | `0`, Bowie. This is the included table in `layout/sf2-07-0x044000-0x064000.asm`; the duplicate `global/afterbattlejoins.asm` agrees. |
| `global/battlemapcoords.asm` | Map52, area `(0,0,32,32)`, trigger `(255,255)`; no other battle row uses Map52. |

After an individual action, `battleloop_1.asm` calls
`ExecuteBattleCutscene_Defeated`, then `ProcessKilledCombatants`, then
`CountRemainingCombatants`. The wrapper in
`code/gameflow/battle/cutscenes/battleendcutscenesstart.asm` tests Bowie's **word** HP
nonzero, enemy128's word HP zero, then F512. Either failed HP test returns without
the script or leader tail. F512 clear runs the selected script; F512 set skips it
but still reaches `battleendcutscenesend.asm:loc_47C48`.

`edcs_battle12` has **nine records**, eight opcode commands and a terminator:
cursor 2490; camera speed `$20`; target 128; single text 2490; targetBowie; text 2491;
single text 2492; setF714; end. All three texts use speaker 128, even after camera
tracking switches to Bowie. `gamescript.txt` keys `09BA..09BC` express surprise at
defeat, recognition of the jewel and a claim that the devils were waiting for Bowie.
These are dialogue claims, not independent proof of the army's history or knowledge.
Text/camera services must return before F714 is written. The script neither sets
F512 nor directly declares victory. The terminator conditionally waits for scrolling
when a dialogue window remains, then clears scrolling speed; target selection itself
is not a wait for the target to appear on screen.

The nonzero leader row makes the shared tail scan all 32 enemy slots 128–159. It
appends every slot with nonzero word HP to the existing death list and writes a
new `-1` sentinel, without requiring placed/nonnegative X and without directly zeroing HP.
`ProcessKilledCombatants` then performs its services, credits deaths, writes each
listed combatant's X/Y to `-1`, clears status, updates stats and moves the mapped
entity to internal `$7000,$7000`. This is sufficient to remove the queued enemies
from the later count's placed-and-nonzero-HP predicate; do not call it a direct
all-enemy HP-zero write. Dead enemies already fail the count.

`CountRemainingCombatants` counts only nonnegative-X, nonzero-word-HP combatants;
Bowie's zero HP forces ally count zero. The loop tests allies first, so simultaneous
exhaustion loses. With the HP admission, completed death-list services and counted
allies remaining, enemy128's death therefore leads through the leader tail to zero
counted enemies and victory. The scene alone is not the victory boundary. The later
after-turn-effects path processes deaths and checks counts again, without an extra
defeated-wrapper call at that same point.

| Outcome | Ordered caller effects and limits |
| --- | --- |
| Victory | `battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies; seeds map from `CURRENT_MAP`; updates force, selects its first battle-party member and seeds X/Y from that combatant plus area origin and facing from its mapped entity. It zeroes parameter 1, calls the after wrapper, clears F412, sets F512, reads parameters 2–5 and returns D4=1. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, requests exhaustion text 363, restores Bowie's HP, halves gold by unsigned right shift and returns the egress tuple with D4=-1. The Battle4 exception does not apply. No new F512 completion. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` consumes MP or the item, runs text/flash services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame and returns the egress tuple with D4=0. No defeat gold penalty or victory completion. |

`ExecuteAfterBattleCutscene` skips the scene with F512 set. With F512 clear,
`abcs_battle12` has **three records**: cursor 2493; warp Map52 `(23,8)`, Left; end.
It displays no text. `csc07_warp` writes event type 1, parameter 1 zero, then the four
map/X/Y/facing bytes. This replaces the seeded return tuple before victory reads
it; it is not an immediate map load. F512-set victory retains the variable seeded
Map52 position/facing, not necessarily Bowie's position or `(23,8)`. This distinction
also applies to supplied states reaching victory without the defeated scene: the
after script does not test or repair F714.

Both after-wrapper branches reach `EndAfterBattleCutscene` and `j_JoinForce(0)`.
`code/common/stats/battleparty.asm` sets F0, updates force and adds Bowie through
F32 only if active-party count is below `FORCE_MAX_SIZE=12`. The return member was
selected before this tail. This does not join Peter or Luke, set their active-party
flags, or change their force-AI membership.

Living/immortal healing restores HP/MP to maxima and preserves STUN/POISON/CURSE;
Peter7 and Lemon28 are always processed, including when previously dead. Other dead
allies stay dead. Thus normal completed victory/exploration healing processes Peter,
but it does not establish F67, an entity mapping, party membership or a particular
supplied maximum HP. `UpdateBattleUnlockedFlag` clears F412 only if F512 was already
set; first-battle loss/egress retains the unlock. `GetEgressPositionForBattle` has no
Battle12 override: it consumes carried `EGRESS_MAP`, with the unmatched-map `(1,1)`,
Up fallback in `GetSavepointForMap`. F399 set on battle entry excludes its pre-first-
battle bedroom fallback. No fixed last-savepoint or Bedoe defeat destination follows.

## Mountain return and Peter's response

**Confirmed:** `code/gameflow/mainloop.asm:alt_MainLoopEntry` calls `SwitchMap` and
then exploration, **without immediate `CheckBattle`**. The return tuple is used
before an ordinary future warp restarts the main admission path. Neither Map52 nor
Map10 appears in `data/maps/global/flagswitchedmaps.asm`.

For a supplied map, `ExplorationLoop` clears pending, adjusts the step counter,
heals living/immortal allies, sets current map/not-in-battle, rebuilds entities and
sprites, clears temporary flags 256–383, sets F80, loads the map and calls init.
The after-script pending warp is therefore consumed as the battle return tuple,
not replayed as an additional exploration warp. After init, the palette comparison
can select map music/fade; `WaitForEvent` restores camera target 0 and calls
`j_SetControlledEntityActScript` only if pending is zero. Service return and actual
input responsiveness remain separate conditions.

Map52 has default `ms_map52` and F512 override `ms_map52_flag512`. Both share init;
the override has an empty fixed list, whereas default declares four exploration
NPCs 128–131. Victory sets F512 before this setup selection. Empty fixed population
does not suppress persistent followers. Neither map is overworld or one of the two
no-follower maps, so F65 does not select the caravan prefix. `followers.asm` and
`InitializeFollowerEntities` supply Peter7 from F67 independently of F7/F39, HP or
force AI. There is no fixed Peter row in either Map52 population. Missing zero
mapping resolves physical 0 in `GetEntityAddressFromCharacter`, not a safe no-op.

`ms_map52_InitFunction` at `0x5C4D6` tests F714. Clear branches directly to return;
set **falls through** to separately delimited `sub_5C4DC`. That helper tests F712:
set returns, clear invokes `cs_5C622`, then sets F712. Retained init body 65 contains
only the first two instructions; direct inspection of the following source section
is necessary to establish this continuation. F512 selects population, F714 admits
the helper, and F712 suppresses the response: none is interchangeable.

`cs_5C622` at `0x5C622` has **six records**: set Peter `(22,8)`, Right; cursor 2493;
fadeInB; nextText2493; nextSingleText2494; end. Keys `09BD..09BE` have Peter recall
the enemy's claim and ask why the devils want Bowie. It does not move Bowie, join
Peter, install a following relation, change force AI or write a warp. Peter's
`setPos` probes his own word HP and skips only at zero; HP256 is nonzero. The text
requests and fade have no matching Peter-presence/HP admission. A successful HP
probe with missing Peter mapping can instead reposition main. F67-clear is not
repaired by this scene. Sprite selection's dead-ally blue flame is likewise not an
actor-presence proof.

The caller writes F712 only after the script returns. Later F714-set/F712-set entry
skips it; F714-clear/F712-clear entry does not set F712. Supplied F512-set/F714-clear
entry is not repaired, and F512-clear/F714-set/F712-clear entry can run the response
with the default NPC list. The source does not enforce a history joining these
flags. The first conditional restored control is the exploration boundary above;
it neither proves a walk from `(23,8)` to the exit nor completion of all follower
movement.

## Selected Bedoe request and setup discriminators

**Confirmed:** `data/maps/entries/map52/6-warp-events.asm` row 0 matches `(28,6)`,
no scroll, and requests `MAP_BEDOE=10`, `(12,30)`, Up. Row1 instead matches
`(0,255)` and requests Polca `(30,6)`, Left; it does not overlap the selected row.
Neither row checks F512/F714/F712, Peter membership or Bedoe's story flags.

Ordinary movement needs layout attribute `$1000` before `WarpIfSetAtPoint` scans
eight-byte rows and writes event type 1, parameters and warp SFX. The no-scroll
`ProcessMapEventType1_Warp` idles main, runs the SFX service, unwinds exploration,
samples/replaces position and returns the tuple. This row replaces both coordinates.
`MainLoop` then runs `SwitchMap` and `CheckBattle`. Map10 has **no battle row**, so
no Battle13 admission or F413/F513 predicate is implied by this request. Ordinary
Map52 entry, if separately requested, still has the predecessor's wildcard row 12:
F412 admits it, and F512 clears the unlock without declining that same call. That
is distinct from the immediate postbattle return, which bypasses `CheckBattle`.
No path/layout extraction establishes that the selected exit is naturally reachable.

`data/maps/mapsetups.asm` has default `ms_map10` then a single F722 override. The
selector's last-set rule thus chooses the override exactly when F722 is set. Its
pointer table replaces entities, entity events, zone events **and init**; descriptions
and section5 are shared. F719 and F722 are independent supplied discriminators:
F719 clear does not guarantee that the default guard caller is selected.

| Selected setup | Bounded actor/init consequences |
| --- | --- |
| Default, F722 clear | One ally row, Luke10 `(38,51)`, Up, and 29 regular NPC rows. Init at `0x57030`, retained body 14: F719 clear runs `cs_570B0`, then sets F719; either F719 branch subsequently tests F720 and, if set, moves 150–156 out of map in order. |
| F722 set | Luke10 `(38,51)`, Down, and 26 regular rows. Init at `0x5708E`, body 15: F799 set moves Luke out of map; independently F724 set moves 152 out; return. No guard script, F719 write or F720 test. |

`InitializeMapEntities` clears mappings, declares flag followers, then consumes the
selected fixed/walking list. Luke's ally row uses lookup 10 and does **not** increment
the regular NPC lookup stream starting at 128; an existing Luke mapping, e.g. from
F75, skips this fixed declaration. Actual physical slots vary with followers. The
default guards are regular lookups 150 and 151, birdman sprites initially at
`(34,51)` and `(35,51)`, Up, `eas_Init`; they are not Luke or physical slots 150/151.
Default 152–156 are the five subsequent birdmen. With F722 set, 150/151 instead
start at `(35,53)` / `(37,53)` and 152 is a birdwoman at `(6,10)`, Right. The alternate
caller's 152 is therefore not the default guard 152. No alternate scene or town audit
is needed to establish these discriminators.

Peter again has no fixed row: F67 supplies his mapping. Luke's declaration does not
join him; this scene has no Luke command. Story followers, actual battle party,
word HP and force AI are separate. Default initialization supplies guards 150/151,
but a missing mapping under an altered/supplied state is not guarded by the scene.

## Guard challenge and exchange

**Confirmed:** `cs_570B0` has **122 records: 113 opcode commands, eight sleeps and
one terminator**. All records' source lines, macros and operands agree with the
retained corpus; inline entity actions were read separately. Cursor 1616 yields
25 text requests, keys `0650..0668` / IDs 1616–1640: 19 `nextSingleText` and six
`nextText`. Speakers are guard 150 eight times, guard 151 seven times and Peter ten
times. There is no choice, join, flag write or warp inside the program.

| Records | Ordered content and requests |
| --- | --- |
| 1–12 | Cursor 1616; reload current view at `(6,19)`; place Bowie `(11,25)` Up, Peter `(11,26)` Up,150 `(10,21)` Right and 151 `(13,21)` Left. Wait for Peter/Bowie `eas_Init`; set both guards' speed 48 through waited inline actions; fadeInB; sleep 30. |
| 13–28 | Wait 150 Right1/faceDown, then his challenge 1616; wait 151 Left1/faceDown, then restriction 1617. Peter moves Right1/Up1 and objects 1618. Two paired jumps start 150 without waiting and wait 151; sleep 30. Each guard moves Down1, then recognition/apology/welcome texts 1619–1621. |
| 29–40 | Peter Up1; sleeps 30/5; start right/left oscillation, sleep 120. Move Right2 and express concern 1622; Left4 and ask about guarding 1623; Right2/faceUp10 and ask what happened 1624. Guard151 reports a recent devil invasion 1625. |
| 41–55 | Peter speed 48 and Up1, reaction 1626;150 Down1/faceRight, Peter Left. Guard150's texts 1627–1629 describe the army, the broken barrier and Volcanon's claimed victory. Reset Peter with `eas_Init`, Down2/faceLeft; Bowie Right. Peter connects their battle to the army 1630;150 faces Down,151 Down1, and 150 asks about other devils 1631. |
| 56–72 | Guards face each other, nod and face Down. Both speed 8, auto-facing off; start 150 Up1 and wait 151 Up1. Both speed 48; start 150 Left1/Down5 and 151 Right1/Down5 **without waiting**; sleep 30. |
| 73–82 | Bowie/Peter Down; two waited Peter jumps. Peter calls them to stop 1632; guards face inward; Peter says those devils were defeated 1633; Bowie nods. Camera destination `(6,22)` waits for scrolling. |
| 83–100 | Both guards speed 8; start 150 Down1, wait 151 Down1; reset both with `eas_Init`. Wait 150 Right1/faceUp and text 1634; wait 151 Left1/faceUp and text 1635. Face inward,150's caution 1636,151 nods. Both Up;151's texts 1637–1638 announce a Polca patrol and ask Peter to see King Bedoe. |
| 101–114 | Both Down, speed 8, auto-facing off; start 150 Up1, wait 151 Up1; sleep 50. Both speed 48; start 150 Down5, wait 151 Down5; sleep 40. |
| 115–122 | Peter Left, Bowie Right; Peter's invitation/introduction texts 1639–1640. Attempt Peter→Bowie following, selector 2. Explicitly place150 `(34,51)` Up and 151 `(35,51)` Up; end. |

The dialogue describes threats and intentions; it does not independently establish
an invasion, Volcanon's success or a patrol reaching Polca. In particular, the final
placements return the two guard lookups to their default coordinates. These are
explicit position/destination writes, not a hidden warp or proof that they permanently
leave the map. Later action execution and the caller's F720 branch remain relevant.
The eight sleeps total 335 requested frames, not scene duration; text, scrolling,
fading and action services have additional waits, and debug text/sleep skips exist.

## Service, coordinate and following boundaries

**Confirmed:** `mapscriptengine_1.asm:csc46_reloadMap` sets view target to `-1`,
disables entity-update VInts, passes D1=`-1` and packed `(6,19)` multiplied by 3 to
`LoadMap`, then enables display/interrupts and entity-update VInts and waits one
VInt. `code/common/maps/mapload.asm:LoadMap` takes its current-map reload branch.
For these operands the packed value is `$1239`; its components 18/57 shift left 7
into 2304/7296 internal units, equal to 6/19 tiles, before map-area/parallax handling.
This reload refreshes map/view services; it does not call `InitializeMapEntities`,
rerun setup init, clear temporary flags or write a pending map change. In particular,
it does not undo the caller-selected population or recursively replay the scene.
Successful return is not proof of a rendered viewport or uninterrupted service.

`csc19_setEntityPosAndFacing` multiplies each coordinate by `MAP_TILE_SIZE=384` and
writes current position, destination and facing. These are internal 1/16-pixel
units: `NIBBLE_SHIFT_COUNT=4` is used by `entityscriptengine_1.asm:VInt_UpdateSprites`
before sprite coordinates. One map tile is **384 internal units /24 pixels**, not
24 internal units. `setPos` and `setFacing` read their actual byte character operand:
allies below 30 skip only at zero **word** HP; NPC150/151 bypass that HP test. Thus
Bowie/Peter staging is conditional on their own HP; the NPC writes are not.

`csc14_setEntityActscriptManual`, `csc15_setEntityActscript` and
`csc2D_entityActionSequence` install actions; wait forms poll the selected entity's
`eas_Idle`. Their installations have no general ally-HP or missing-mapping guard.
Inline actions here set speed 8/48 then jump idle. `eas_Init` restores speed 32,
auto-facing, collision/obstruction properties, orientation and sprite update;
`eas_DeactivateAutoFacing` clears auto-facing. `eas_Jump` has its own motion/wait
sequence and restores auto-facing. Waiting for 151 never constitutes an explicit
wait for 150. A later installation can replace a still-running action; sleeps and
matched-looking paths do not prove simultaneous movement completion. Final `setPos`
does not itself replace an actor's action pointer.

`setCamDest` clears tracking and waits for scroll completion; `setCameraEntity`
resolves/writes a target without that wait. Text handlers wait for view scrolling
and invoke portrait/speech/window services; single-text calls also close windows
and sleep ten frames. `fadeInB` calls the fade service. The interpreter terminator's
conditional view wait and speed reset are not a barrier for all entity actions.
Existing controlled-handler evidence in [common scripting](./common-scripting.md)
proves only its named seams, not this composed story or its visible timing.

Record 119 `sf2cutscenemacros.asm:followEntity` encodes words `$0007,$0000,$0002`.
`csc2C_followEntity` first probes the **high byte** of the first word without
advancing A6, so the alive helper tests **Bowie0's word HP**, not Peter7's. Zero
skips all six operand bytes; nonzero proceeds regardless of Peter's HP. With valid
mappings it installs Peter following Bowie at selector 2's `(-24,0)` offset.
`entityfunctions_2.asm:AddFollower` copies/patches a 42-byte follower action and
appends its physical index if absent; action replacement precedes duplicate
detection. Missing Peter can resolve main for both operands. No wait for following,
F67 write, force join, party admission or force-AI update follows. Unlike the earlier
Polca reload-entities scene, no subsequent population rebuild occurs in this script;
this is a current relation, not a promise of persistence across later entry.

## Caller, restored control and reproduction

**Confirmed:** after the default script terminates, `ms_map10_InitFunction` sets
F719, then independently tests F720. Clear returns with the scene's final explicit
guard placements; set calls `MoveEntityOutOfMap` on 150,151,152,153,154,155,156 in that
order. This helper resolves the lookup and writes internal `$7000,$7000` through
`SetEntityPosition`; it does not delete the actor or recruit anyone. Thus supplied
F720 set does not skip an F719-clear scene; it moves the guards out only afterward.
With F719 set, the scene is skipped but the same F720 handling still runs. F722-set
selection instead uses the alternate caller and does not write F719 at all.

`RunMapSetupInitFunction` returns directly after its selected call; the zone-event
wrapper's close-window/main-idle tail is not inserted here. Exploration's palette/
music/fade branch follows, then `WaitForEvent`, only with pending zero, retargets
physical 0 and calls `j_SetControlledEntityActScript`. The latter selects its action
from `PLAYER_TYPE`; ordinary zero selects `eas_ControlledCharacter`. This is the
first conditional restored-control boundary. It does not establish physical input
response, a completed follower path, all NPC actions idle or future floor access.
Stop here before the next scene and later Bedoe content.

| Relation | Existing evidence and direct review | Discriminators retained |
| --- | --- | --- |
| Row12 outcomes | `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`; exact table rows, wrappers, death-list/count, victory/defeat/egress/main consumers. | Leader `-1`; HP gates before F512; F512-set still reaches leader/join tails; position-based removal; defeat priority; seeded versus explicit return; carried egress. |
| Short scripts / mountain init | Retained `edcs_battle12`, `abcs_battle12`, `cs_5C622`; `map-init-static-v1.json` body 65 plus direct `sub_5C4DC`; text keys `09BA..09BE`. | All 18 records; F714/F712/F512 independence; caller write after script; healing versus mapping/party/AI; no immediate battle check. |
| Selected request / Bedoe selection | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`; exact warp row 0 and competing row 1, no Map10 battle/replacement rows, pointer/list consumers. | Attribute/pending/service gates; F722 versus F719; Luke ally lookup versus regular 150/151/152; F67 and missing mappings. |
| Guard scene / init return | Retained `cs_570B0`, map-init bodies 14/15, direct source lines 4–224 and text keys `0650..0668`; map/view/entity/follow/control consumers. | All 122 records and inline payloads; word-HP/high-byte guard; waited versus nonwaited actions; real coordinate units; final placements before F719/F720; no natural-control claim. |

Static fixture names above are under `tests/fixtures/h2/`. Load
`. ./local/private-inputs.ps1` in the owning checkout, select the retained read-only
`$upstream`, and verify its pin and clean status. Use the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select the four `entryLabel` values above in `programCorpus.programs`; compare every
command's `sourceLine`, macro and operands against the named segment, and inspect
inline actions separately. Select init `primarySourceBodies` 14/15/65 and the direct
fall-through section, then the named table rows/lists/handlers. Summary fixtures do
not independently parse every selected row: direct pinned source supplies those
exact values. Text IDs map to four-digit hex keys in `gamescript.txt`. This needs no
new comparator or runtime collection.

**Unknown:** natural mountain/Bedoe walking and collision, supplied flags and
maximum/current HP, actual party/follower/AI mappings, interruption-free service
and pending completion, camera/fade/motion/text presentation, and continuous
playability. Dialogue claims and static connectivity do not close these questions.
No inseparable ownership expansion was needed. Record completed direct checks and
failures, clean committed `uv run sf2 verify plan --base origin/main --head HEAD`
and actual final-head CI in the Issue/PR handoff. Do not launch local normal/full/
engine/H3/H4/Godot suites solely for this prose. Leave a clean pushed Draft PR,
Project Review / Verify, frozen writer and successful completion callback;
independent main-gate owns acceptance/merge. No cleanup or next slice is authorized.
