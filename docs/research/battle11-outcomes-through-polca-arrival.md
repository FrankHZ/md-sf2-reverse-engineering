# Battle11 outcomes through Polca arrival

Evidence date: 2026-10-13. **Confirmed:** Battle11 has empty battle-end and after-battle
scene rows. Victory returns to Map48 at a battle-derived position; a separate selected
exit requests Map72 `(56,46)`, Down, and its Polca entrance requests Map13 `(15,30)`,
Up. The first X24 zone scene stages the injured Oddler's arrival, explicitly rebuilds
the F710 population, then returns to a caller that handles actor 130 and sets F710.
**Inferred:** these connections and scene form a candidate continuation from the
accepted cave opening. **Unknown:** natural cave crossing, supplied state, completed
services and visible presentation. Table connectivity is not a playable-route proof.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #772](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/772), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Battle returns, map admission, scene actors and population replacement cross existing
owners and need explicit boundaries. The sole tracked owner is this document; no
shared index, registry, schema, fixture, parser, design, engine, UI or translation edit.
Consume accepted [Battle11 admission](./east-shrine-return-through-battle11-admission.md),
[battle lifecycle](./battle-loop.md), [cutscene dispatch](./battle-cutscenes.md),
[map requests](./map-event-request-consumption.md) and
[the previous follower analysis](./battle10-outcomes-through-east-shrine-visit.md#current-following-versus-the-f77-rebuild).
Starting accepted base is `47f7ad43912990fbe440c9790f4149ee610ba772`; no stack.

Scope is row 11 outcomes, the selected Map48 `(3,12)` → Map72 `(56,46)` and Map72
`(43,46)` → Map13 `(15,30)` requests, ordinary Polca population/init and F710-clear
`Map13_ZoneEvent0` through its first conditional restored control. No mandatory shrine
visit is assumed. Combat/AI, cave layout/path extraction, other cave routes, Battle25
content, Battle12/13 content, full town inventory, `cs_58512` turtle interaction,
F710-set later scene, Y11/Y30 later-story branches, mayor/raft/Oddler recruitment,
Bedoe, runtime acquisition and new comparison machinery are excluded. Alternative
battle/setup rows are admission and actor-selection discriminators only.

Source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing fixture comparisons and cutscene-source comparison pass; no fresh ROM
extraction or scene observation is claimed. Fnnn means flag nnn. Battle/warp rows,
map-init `sourceOrder` and event `programOrder` are zero-based; script record numbers
are one-based positions in retained `programCorpus.programs[].commands`.

Reuse the retained 18,285,979-byte pair and existing comparisons: one process, linear
selected reads, estimated peak below 512 MiB and comparisons below one minute, plus
small prose/receipts. Actual comparisons completed in 0.088 seconds. Inventory finds
Map13 `scripts.asm` is 727 lines / 31,323 bytes, but the selected scene is only lines
4–131 (128 lines), immediately before `cs_58512`; init is 41 lines / 1,446 bytes.
Read that bounded segment and its existing consumers; do not publish the full script
or expand into the rest of town. Reassess if another producer or broader evidence is
needed. Unknowns do not authorize collection. Direct document/source/fixture checks,
the clean committed planner and actual final-head CI complete this docs-only profile.

## Outcomes and the separate cave return

**Confirmed:** `battleloop_1.asm` tests counted allies before enemies after actions
and after-turn effects. `battleloop/countremainingcombatants.asm` counts only
nonnegative-X, nonzero-word-HP combatants; Bowie's zero HP forces ally count zero.
Zero enemies with allies remaining wins; simultaneous exhaustion loses.

| Row11 source / consumer | Consequence |
| --- | --- |
| `data/battles/global/enemyleaderpresence.asm` | Zero. Enemy 128's death alone does not clear the other enemies or establish victory. |
| `data/battles/cutscenes/battleendcutscenes.asm` | `ms_Empty`. `battleendcutscenesstart.asm` dispatches only with Bowie alive, enemy128 dead and F511 clear. F511 set skips the scene; the shared leader tail still reads zero and does not append the remaining enemies to the death list. |
| `data/battles/cutscenes/afterbattlecutscenes.asm` | `ms_Empty`, dispatched only with F511 clear. It does not replace the seeded return tuple. |
| `data/battles/cutscenes/afterbattlejoins.asm` | Zero, Bowie. The table included by `layout/sf2-07-0x044000-0x064000.asm` agrees with the duplicate global-path row. `EndAfterBattleCutscene` calls `j_JoinForce` even when F511 skipped the after script. |

The join tail in `code/common/stats/battleparty.asm` sets F0, updates force and
conditionally adds Bowie to the active party through F32 when capacity permits.
It does not recruit Oddler, Gerhalt or Peter. The return member has already been
selected before this tail. An empty script still reaches its terminator's conditional
view-scroll wait and scrolling-speed reset; empty does not mean no interpreter work.

| Outcome | Ordered behavior |
| --- | --- |
| Victory | `battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies, seeds map from `CURRENT_MAP`, updates force and selects its first battle-party member, adds area X/Y to that member's combatant position, and reads its mapped facing. It zeroes parameter 1, runs the after wrapper, clears F411, sets F511 and returns parameters 2–5 with D4=1. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, presents exhaustion text 363, restores Bowie's HP, halves gold by unsigned right shift and returns the egress tuple with D4=-1. The Battle4 exception does not apply; F511 is not newly set. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` consumes spell MP or the item, performs text/flash services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame and returns the egress tuple with D4=0. It adds neither the defeat penalty nor victory completion. |

Row11 of `battlemapcoords.asm` is Map48, area `(0,0,27,32)`, trigger `(15,26)`.
Victory therefore normally seeds Map48 with the first member's battle-derived
position/facing, not the admission tuple, cave exit or necessarily Bowie's position.
Living/immortal healing does not resurrect dead non-immortal allies.
`UpdateBattleUnlockedFlag` clears F411 only if F511 was already set; first-battle
loss/egress retains the supplied unlock. `GetEgressPositionForBattle` has no Battle11
override: it passes carried `EGRESS_MAP` to `GetSavepointForMap`, including the
unmatched-map `(1,1)`, Up fallback. Battle entry's F399 excludes the pre-first-battle
bedroom fallback. No last savepoint or fixed defeat destination is established here.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls `SwitchMap` and then exploration,
without immediate `CheckBattle`. No flag-switched row replaces 48,72 or 13. Immediate
victory return therefore does not re-admit row 11 or 25, even with a supplied F425.
Ordinary supplied-map entry clears pending, adjusts the step counter, heals living/
immortal allies, sets current map/not-in-battle, rebuilds entities and sprites, clears
temporary flags 256–383, sets F80, loads the map and calls init. Map48's accepted
[no-setup sentinel](./east-shrine-return-through-battle11-admission.md#cave-admission-f411-and-the-no-battle-alternative)
supplies no fixed population or init, while persistent flag followers can be rebuilt.
Pending zero after the services permits control. None of this locates an accessible
path to the selected exit.

## Selected cave exit and Polca admission

**Confirmed:** `sf2enums.asm` defines `MAP_DARK_CAVES=48`,
`MAP_OVERWORLD_BEDOE_KINGDOM=72` and **`MAP_POLCA=13`**. `(15,30)` is Polca's arrival
tuple; Map15 is a different map. In `data/maps/entries/mapNN/6-warp-events.asm`:

| Source / row | Match and type | Requested destination |
| --- | --- | --- |
| Map48 row 3 | `(3,12)`, no scroll | Map72 `(56,46)`, Down |
| Map72 row 5 | `(43,46)`, no scroll | Map13 `(15,30)`, Up |

Map48's other exits are `(55,30)`, `(52,13)` and `(15,27)`; none substitutes for the
selected row. The last is the return-side exit near the prior admission. Map72 row 4
returns from `(56,45)` to Map48 `(3,11)`, not the Battle11 trigger `(15,26)` or Battle25
trigger `(55,29)`. These discriminate requests only, not walkable connections.

Ordinary movement in `entityscriptengine_2.asm` needs layout attribute `$1000` before
`WarpIfSetAtPoint` scans matching eight-byte rows and writes event type 1, parameters
and warp SFX. The selected fixed rows replace both coordinates. The no-scroll
`ProcessMapEventType1_Warp` idles main, performs the SFX service, unwinds exploration,
samples/replaces main position and returns map/X/Y/facing to `MainLoop`, which calls
`SwitchMap` then `CheckBattle`. Mutable pending parameters and service completion
remain the [request owner's](./map-event-request-consumption.md) boundary. Neither
row checks F511, shrine flags, Peter membership or Oddler flags.

The new Map72 request is subject to the accepted
[Map72 interruption limits](./east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request):
ordered wildcard battle rows 8/24/26 use F408/F424/F426; matching completion clears its
unlock without declining that same call. Absent admission, its empty fixed list/init
can still invoke `cs_4FF5A` with F716 set and F10 clear. Its `$1400` zones can select
Battle26 or the F523-dependent Battle8/24 helper, or the specific later-story zone.
The different `(56,46)` arrival does not prove those interruptions absent, nor bypass
the existing random-helper cooldown/completion conditions. No new Map72 scene audit.

For Map13, the sole battle row is 13, `BATTLE_POLCA_VILLAGE`, area `(0,0,32,32)`,
trigger `(255,255)`. `CheckBattle` therefore selects it on supplied F413, including
at `(15,30)`; if F513 is also set it clears F413 and still selects 13 on this call.
F413 clear permits ordinary Polca entry regardless of F513. The rectangle is an
output, not a containment test. F511 does not establish F413/F513, and ordinary town
arrival must not be claimed when this battle discriminator selects another branch.

## Ordinary Polca population and init

**Confirmed:** `data/maps/mapsetups.asm` lists default `ms_map13`, then F710, F799,
F513 overrides in that order. `GetCurrentMapSetup` scans all rows and the **last set
flag wins**, so priority is F513 > F799 > F710 > default. The four pointer tables
share `ms_map13_ZoneEvents` and `ms_map13_InitFunction`; F513 alone substitutes the
entity-event table as well as population. File inventory is essential:
`pointertable_709.asm` and `s1_entities_709.asm` actually contain the **flag710** symbols.
The filename suffix is not a F709 predicate.

For default or explicit F710 populations, `InitializeMapEntities` clears mappings,
builds flag followers, then consumes the fixed/walking rows. Gerhalt's first row uses
ally 9's lookup entry; it does **not** advance the ordinary NPC-event pointer. Regular
NPCs begin at 128 independently of their physical entity slots:

| Lookup | Default origin / identity relevant to this scene |
| --- | --- |
| Ally 9, Gerhalt | `(23,13)`, Down; separate ally lookup, not event 128. An existing ally mapping skips that row. |
| 128,129 | Male and female centaurs; establish the two regular rows before the kappa. |
| **130** | Kappa/turtle, `(1,11)`, Down, `eas_InitSlow`. It is not the female centaur. |
| 133 | Beastman at `(23,6)`, Right. The narrator/text requests identify him as the beastman checking the boy. |
| 137 | Man at `(15,3)`, Down; addressed as mayor in this scene's texts. |
| 140 | Oddler sprite initially at `(63,63)`, Down; scene explicitly places him at `(29,7)`. |

The default/F710 lists each have one ally and thirteen regular NPC rows. The F710
list changes only 133 to `(16,4)` and 140 to `(14,3)` among their declarations; its
other origins/actions remain the same. Actual slot numbers depend on preceding
followers. Polca is neither a no-follower map nor overworld; F65 does not select the
caravan prefix. F67 supplies Peter7 independently of F7/F39, word HP or force AI.
There is no fixed Peter row here. Dead allies outside battle receive the shared
blue-flame alternative, but neither sprite selection nor text requests establish
party membership or a living actor.

This mapping is not unconditional for arbitrary supplied flags. F799 selects a
shorter population with different NPC identities (130 is a minotaur; 133/137/140 lack
fixed rows), yet uses the same init/zone handlers. F513 has another population and
entity-event table. F710-clear alone does not rule either selector out. The lookup
consumer has no missing-entity guard: an untouched zero mapping resolves physical 0,
not a safe skip. The scene is not proved sensible or naturally reachable under
inconsistent supplied story states; no additional admission predicate is invented.

`ms_map13_InitFunction` at `0x582E0` (retained init body 18) performs these ordered,
independent tests:

1. F711 clear invokes `cs_58310`; set skips it.
2. F716 set calls `ChangeEntityFacing($89=137,Down)`; clear skips it.
3. F718 set calls `MoveEntityOutOfMap(140)`; clear skips it; then return.

`cs_58310` at `0x58310`, source lines 33–41, has four records: `setFacing 130,Down`,
`customActscriptWait 130`, `startEntity 130`, end. Its inline actions turn motion off,
write Right orientation, request a sprite update and jump to `eas_Idle`.
`esc1B_setEntityOrientation` writes the low direction bits of flagsB; this is distinct
from facing. `startEntity` zeroes the animation counter. These source effects support
an **Inferred** staged turtle pose, not a verified visible rotated sprite or a turtle
interaction. F711-set init alone does not hide 130. `MoveEntityOutOfMap` resolves its
argument and writes `$7000,$7000` via `SetEntityPosition`; it does not delete an actor.
The selected scene can later reposition 140 even if supplied F718 hid it on entry.

## First X24 event and scene records

**Confirmed:** `s3_zoneevents.asm` lists X24/wildcard Y first, then wildcard X/Y11,
then wildcard X/Y30, then default. Ordinary zone production requires `$1400`.
The first matching row wins: at `(24,11)` or `(24,30)`, the X24 handler wins, and a
handler return does not fall through. This is not automatic invocation on town entry.
`Map13_ZoneEvent0` (`0x5823A`, retained zone program 9) tests F710 alone for its first
branch. With F710 clear it runs `cs_58330`; supplied F711/F716/F718 do not skip that
call. F710-set instead reaches the F799-set/F715-clear later-scene discriminator,
whose body is excluded. No continuous walk from `(15,30)` to an eligible X24 tile is
established.

`cs_58330` has **83 records: 68 opcode commands, 14 sleeps and one terminator**.
Inline actions are separate payloads. The direct source and retained corpus agree
on every record's source line, macro and operands. Cursor 1452 produces eleven text
requests, ten `nextSingleText` and one `nextText`, IDs 1452–1462 (`05AC..05B6`) with
ordinary cursor behavior. `gamescript.txt` supplies the text; no transcript is needed.

| Records | Ordered content and requested actions |
| --- | --- |
| 1–14 | Door-rumble SFX; quake 6; white-flash 90; quake 0; waits 20/5; Bowie Right/Left oscillation; wait 5; 133 Up/Down oscillation; waits 50/5; Peter Right/Left oscillation; wait 100; cursor 1452. Oscillation installations do not wait. |
| 15–29 | Place 140 `(29,7)`, Left; set speed 8 and wait Left2; speed 4; face Bowie/Peter Right; wait 140 Left1; disable 140 auto-facing; speed 32; start Down1, sleep 2, replace with stop script; motion off / Left orientation / sprite update; face 133 Right; sleep 60. |
| 30–41 | 133 reacts (1452); wait Right3, face Down; narrator describes checking the boy (1453); 133 reports injury (1454); sleep 30, face Right, sleep 50; wait Left2; jump with wait, another jump without wait; call for mayor (1455). |
| 42–53 | Place 137 `(17,7)`, Right; camera destination `(13,3)`; place Bowie/Peter both `(25,9)`, Up; track 137; wait 137 Right5/Up1, face Right. Mayor asks what happened (1456), beastman reports injured mountain boy (1457), mayor reacts (1458); wait 137 Down1/Right3; face 133 Right. |
| 54–60 | Mayor's `nextText`1459 describes injured eyes, followed by single text 1460 attributing injury to eruption; wait 133 Right3/Down1, face Left; 133 requests treatment (1461); 137 orders bringing the boy inside (1462); nod 133. |
| 61–71 | Start 137 Right1 and 133 Left1; sleep 3; stop 133 then 137; face 137 Left; sleep 30; start 140 Up1, sleep 2, stop 140; sleep 30. These replacements do not certify a completed tile step. |
| 72–83 | Neutral camera; start 137 and 140 Left9, wait 133 Left9; reset 140 with `eas_Init`; face 137 Left, 133 Up, 140 Down; reset Bowie with `eas_Init`; attempt Peter→Bowie following, selector 2; explicitly reload F710 population; end. |

Text requests use 133 five times, 137 five times and narrator `ENTITY_NONE` once.
They present an injured boy and a request for treatment; they do not recruit Oddler,
join Gerhalt, offer a choice, write a warp or test battle completion. No story-flag
write occurs inside this program. Fourteen explicit sleeps total 392 requested frames,
not measured duration; action, scrolling, fading and text services add other waits.
Debug text/sleep skip behavior prevents treating these counts as unconditional output.

## Services, following and the population replacement

**Confirmed:** `mapscriptengine_1.asm` separates command requests from their consumers:

- `csc33_setQuakeAmount` sees neither high mode bit in 6 or 0 and directly writes
  `QUAKE_AMPLITUDE`. `csc41_flashScreenWhite` shifts 90 right by 3 to 11 and uses `dbf`,
  making twelve `LaunchFading` calls, then `DuplicatePalettes`. Each launch waits for
  VInt/fading-state completion. The argument 90 is not proof of exactly 90 visible
  frames; quake amplitude/flash visibility and successful service completion are Unknown.
- `csc14_setEntityActscriptManual` installs the inline pointer; `csc15_setEntityActscript`
  installs a named pointer; `csc2D_entityActionSequence` builds an action block in dynamic
  storage. Their wait forms poll for `eas_Idle`; none has the ally-HP guard used by
  `setPos`/`setFacing`. A short sleep followed by `eas_StopMoving` replaces the prior
  action with `ac_moveRel 0,0` then idle, rather than waiting out the original move.
  `eas_DeactivateAutoFacing` clears auto-facing; later `eas_Init` restores it and speed 32,
  collision/obstruction settings, orientation and sprite update. This is not a rendered
  pose or collision-free motion observation.
- `csc19_setEntityPosAndFacing` and `csc23_setEntityFacing` probe their actual byte
  character operand. For allies below 30 they skip only when **word** HP is zero;
  HP256 is nonzero. NPC 130/133/137/140 bypass this guard. Thus Bowie/Peter's `(25,9)`
  requests are conditional on their own HP, while action installations can still run.
  Missing mappings can resolve main even where HP allows a command.
- `setCamDest` clears entity targeting, sets the destination and waits for scrolling;
  `setCameraEntity` maps/writes a target without that wait. `CAMERA_NEUTRAL` disables
  tracking. Text handlers wait for scrolling and invoke portrait/speech/window services;
  single-text calls additionally close windows and sleep ten frames. The terminator
  conditionally waits for scrolling and clears speed, not every entity's action.

At record 81, `sf2cutscenemacros.asm:followEntity` encodes words `$0007,$0000,$0002`.
`csc2C_followEntity` first reads the **high byte** of the first word without advancing
A6. The shared alive-status helper therefore tests Bowie0's word HP, not Peter7's.
Zero skips all six operand bytes; nonzero proceeds independently of Peter's HP.
With valid mappings it installs Peter following Bowie at selector 2's `(-24,0)` offset,
through `entityfunctions_2.asm:AddFollower`: copy/patch the 42-byte follower action and
append its physical index if absent. Action replacement precedes duplicate detection.
This does not set F67 or join/activate Peter, and does not wait for visible following.
If Peter's mapping is absent, the resolver can select main for both operands. The
[existing relationship-handler evidence](./common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)
covers the high-byte guard at its controlled seam, not this Polca scene composition.

Record 82 `csc44_reloadEntities` immediately disables entity-update VInts, samples
main's current X/Y divided by 24 and facing, reads the **explicit**
`ms_map13_flag710_Entities` pointer and calls `InitializeMapEntities`, then reenables
those VInts. It does not query setup flags, call map init, heal/reset temporary flags,
or call the separate `LoadEntityMapsprites` service used by `loadMapEntities`.
`ClearEntities` clears entity/action state and mappings and resets walking allocation;
followers and the explicit list are rebuilt, and main is declared idle at the sampled
coordinates/facing. Therefore the current transient Peter-follow relation is **not
preserved** as an existing entity/action/list entry.

With only F67 among follower flags, the rebuild creates Peter in physical 1 using
`eas_Follower1`, whose `ac_follow 0,-24,0` has the same target/offset as the successful
selector 2 request in this bounded case. The relation is rebuilt from F67, not preserved
from record 81. Other enabled follower rows alter physical order/action
selection. With F67 clear no Peter fixed row recreates him, even if record 81 previously
acted on a supplied mapping. F68 can independently create an Oddler-sprite follower
under 31, separate from the fixed 140; this scene does not set F68 or F718.
The explicit list restores 133 `(16,4)`, 137 `(15,3)` and 140 `(14,3)` regardless of the
preceding Left9 movement's actual endpoint. Main's sampled position is only the
requested `(25,9)` if the HP guard and subsequent services/state permit it.

## Caller handoff and evidence boundary

**Confirmed:** the population reload occurs while the first-event caller's F710 is
still clear. After `cs_58330` terminates, `Map13_ZoneEvent0` tests F711:

- Clear: rerun `cs_58310` on the rebuilt 130 to restore the staged turtle action/pose.
- Set: call `MoveEntityOutOfMap(130)` on that rebuilt mapping.

Only after this branch completes does the caller set F710 and return. No F711, F716,
F718, F68, join or pending warp write is added. A later setup lookup considers F710
alongside higher-priority F799/F513; the flag write itself does not reload again.
A supplied higher-priority setup can thus disagree with the explicitly loaded current
population. Neither the F711 write history nor the later-story flag relations are
reconstructed here.

`RunMapSetupZoneEvent` closes portrait/text, waits one VInt and waits for physical 0 to
stop after this caller. `WaitForEvent`, only with pending zero, restores camera target 0
and calls `j_SetControlledEntityActScript`. This is the first conditional restored
control boundary. It does not certify all rebuilt walking/follower actions, map
presentation or physical input response. Stop here, before `cs_58512`, other zone
branches, mayor conversations, raft, recruitment or Bedoe.

| Relation | Existing evidence and direct reproduction | Limits to preserve |
| --- | --- | --- |
| Outcomes / return | `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`; exact row 11 targets/leader/join, shared loop/wrappers/main/egress consumers. | Defeat before empty-enemy victory; dead leader alone; F511-set skips; actual first member and carried egress map; no immediate admission. |
| Requests / setup / actors | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`; selected warp rows 3/5, battle rows 11/13/25 and inherited Map72 discriminators; ordered setup and list consumers. | Layout attributes, mutable request, F413/F513 same-call selection, last-set setup priority, ally versus regular event slots, missing actors. |
| Init / caller / records | `map-init-static-v1.json` body 18; `map-events/zone-programs.json` program 9; retained `cs_58310` and `cs_58330`; source nested actions/text hex keys. | F711/F716/F718 independence, X24 priority, F710-set exclusion, all 87 records' line/macro/operands, debug skips and waits. |
| Following / replacement | `map-script-engine-static-v1.json` and `h3/map-entity-gesture-relationship-motion-v1.json`; direct macro, HP/lookup, follower, clear/init and reload consumers. | Word-operand/high-byte guard, zero/missing Peter, F67 present/absent, explicit population before F710 write, rebuilt versus transient following. No new H3 or story observation. |

Static fixture names are under `tests/fixtures/h2/`; the explicit `h3/` path is under
`tests/fixtures/`. Load `. ./local/private-inputs.ps1` in the owning checkout, select
the retained read-only `$upstream`, and verify its pin/clean status. Run the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select the two `entryLabel` values in `programCorpus.programs`; compare every command's
`sourceLine`, macro and arguments against the named source segment, with inline
entity actions inspected separately. Read init body 18 and zone program 9, the exact
rows/lists/handlers named above, and text keys `05AC..05B6` from `gamescript.txt`.
This requires no new parser, comparator, fixture, source copy or runtime collection.

**Unknown:** natural cave/town traversal and collision, actual flags/actors/HP/party/
follower mappings, interruption-free travel, quake/flash appearance and timing,
movement/fading/text/pending completion, current sprite presentation and continuous
playability. Existing static and controlled-handler evidence does not prove this
composition. No inseparable scope expansion was needed to reach the declared caller.

Record direct checks and any failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in the
Issue/PR handoff. No local normal/full/engine/H3/H4/Godot suite solely for prose. Leave
a clean pushed Draft PR, Project Review / Verify, frozen writer and successful
completion callback; independent main-gate owns acceptance/merge. No cleanup or next
slice is authorized by this handoff.
