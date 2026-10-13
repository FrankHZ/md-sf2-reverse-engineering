# Battle 05 outcomes through Galam's exits to source Battle 06 admission

Evidence date: 2026-10-12. **Confirmed:** Battle05's first-completion after-program recruits Slade,
sets F384, clears F73, unlocks Battle06 with F406, and supplies Map16 `(16,18)`, Down to the victory
return. Map16's south exit connects through Map18 to Map66, where row 6 can admit Battle06. Defeat
and escape do not execute that after-program. **Inferred:** the connected dialogue and staging
express the intended escape from Galam toward Granseal. **Unknown:** natural victory, continuous
walking, caller entity state, reentry behavior and completed presentation. This is a conditional
source relation, not a playthrough or proof that the numerical next battle is unavoidable.

## Agreement and provenance

This is the Standard-rigor agreement for [Issue #708](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/708),
under [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Outcome,
party, map and admission consumers cross ownership boundaries and need explicit predicates. The
sole tracked write is this document. The accepted starting base is
`78c2c28d88dd9d84a3e027c44fae0745fbbf6df0`, including
[the Battle04-to-Battle05 owner](./battle04-to-battle05-content-route.md) from
[PR #706](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/706). Concurrent content synthesis
is independent; no unmerged conclusion is consumed and no shared-path write is required.

Scope is Battle05 outcome/defeated/after/join/return consumers, the directly connected Galam exit
content, and Battle06 admission through selected before/start dependencies. Stop before Battle06
resolution. Later campaign, balance/winnability, broad traversal, new tooling/fixtures, emulator
acquisition, engine/UI and Chinese translation are excluded. Acceptance is pinned source and
fixture/verifier review, conditional content/gap tables, prose/reference/link/private-boundary and
diff checks, a clean committed planner and actual final-head CI. Delivery is a pushed Draft PR for
independent main-gate review; this document does not accept a remake capability.

All original-source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only source checkout was verified clean at that exact pin. The predecessor's retained
`h2 map-init` and `h2 map-script-engine` payloads bind the USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` and that source revision.
Their selected tool/fixture dependencies are unchanged; both retained payload comparisons passed
again here. This is not a new extraction, ROM verification or runtime observation. Addresses below
are static source/H1 provenance.

`Fnnn` denotes a flag. `sf2enums.asm` defines `BATTLE_GALAM_CASTLE=5`,
`BATTLE_TO_GRANSEAL=6`, `MAP_GALAM_CASTLE=16`, `MAP_GALAM_INTERIORS=17`,
`MAP_GALAM_DRAWBRIDGE=18`, `MAP_OVERWORLD_GRANSEAL_KINGDOM=66` and `ALLY_SLADE=5`.
Short battle-code paths below are under `code/gameflow/battle/`; short script-handler names are in
`code/common/scripting/map/mapscriptengine_1.asm` or `mapscriptengine_2.asm`.
Map setup files are under `data/maps/entries/mapNN/mapsetups/`; map-content files are in `mapNN/`.

## Battle05's defeated consumer and outcomes

**Confirmed:** `battleloop_1.asm` checks factions after the individual turn's defeated-script/death
processing and again after turn-end effects/death processing. `CountRemainingCombatants`
(`battleloop/countremainingcombatants.asm`, `0x23C58`) counts nonnegative X and positive HP, and forces
zero allies when Bowie is dead. Zero allies takes precedence over zero enemies. Battle05's
`data/battles/global/enemyleaderpresence.asm` row is **zero**; the AI label `LEADER` on its first
spriteset enemy is not that table's boolean and does not itself establish victory.

The additional Battle05-specific consumer matters before interpreting those counts:

| Consumer / predicate | Ordered effect and limit |
| --- | --- |
| Battlescene return in `battlefunctions/executeindividualturn.asm` | After `j_EndBattlescene`, calls `j_ApplyPositionsAfterEnemyLeaderDies`, then `LoadBattle`, before returning to the top-level defeated-script check. The jump alias resolves through `code/common/tech/jumpinterfaces/s07_jumpinterface.asm`. This is a named battlescene-return caller, not an assertion that every HP-zero event follows it. |
| `ApplyPositionsAfterEnemyLeaderDies` (`cutscenes/afterenemyleaderdeathpositions.asm`, `0x47D9E`) | Requires Bowie HP nonzero and combatant 128 HP zero; scans `table_AfterBattlePositions` for current battle. The table's sole battle is 5. It does **not** test F505. Allies 0–29 and enemies 128–157 receive X=-1; enemy slots 128–157 also receive HP=0. Slots 158/159 receive X=0 only, with no HP write. It does not kill allies or indiscriminately clear all 32 enemy HP slots. |
| Selected `abp_Battle5` (`data/battles/global/afterbattlepositions.asm`, `0x47E6C` table) | Restores battle-relative positions Bowie `(16,4)`, Slade `(17,4)`, and slot 128 `(17,2)`. Allies require nonzero HP; slot 128 is explicitly allowed at zero HP. Fourth bytes are skipped by the four-byte stride. With area origin `(0,14)`, these correspond to map Y 18/18/16. Positioning Slade is not a force join and cannot prove he fought in Battle05. |
| `ExecuteBattleCutscene_Defeated` (`0x47B92`) | With Bowie alive, slot 128 dead and F505 clear, dispatches row 5 of `data/battles/cutscenes/battleendcutscenes.asm` to `edcs_battle5` (`0x49AB2`). F505 set skips the script. The shared tail's leader-presence lookup is zero for Battle05, so it adds no remaining living enemies to the dead list. This is separate from the earlier position consumer's HP writes. |

The ordinary spriteset's first enemy is a Galam Knight (`data/battles/spritesets/spriteset05.asm`).
The position consumer's effects explain a source route to zero remaining enemies; they do not prove
that the player can naturally produce its input state, or that every possible caller reaches the
same staging. In particular, leader death caused at the later turn-end check does not acquire this
battlescene-return call merely because its HP is now zero. Preserve that caller distinction.

| Outcome | State / script order | Return and boundary |
| --- | --- | --- |
| Victory with F505 clear | `BattleLoop_Victory` (`battleloop_2.asm`, `0x23CBA`) heals living/immortal allies, prepares map-event parameters from the current map and first battle-party member's final position plus area origin/facing, with parameter 1 zero. `ExecuteAfterBattleCutscene` calls `abcs_battle05`; its warp replaces parameters 1–5 and sets event type 1. Shared after-join row 5 then calls `JoinForce(0)`. Victory clears F405, sets F505 and reads the replaced fields. | d0–d3 = Map16 `(16,18)`, Down; d4=1. `MainLoop` at `alt_MainLoopEntry` (`0x75E0`) calls `SwitchMap` then exploration without a new `CheckBattle`. `ExplorationLoop` clears event type at entry. The pending script warp is consumed as the victory return, not dispatched again as a second map event. |
| Victory with F505 already set | After wrapper skips the entire after-program, but still calls `JoinForce(0)` from the shared table, then victory clears F405 and sets F505. | Prepared current-map/final-position fields survive. No new F384, Slade join, F73 clear, F406 write or fixed castle warp is supplied by this skipped script. This is a source-available alternative, not proof of natural Battle05 repetition. |
| Defeat | `UpdateBattleUnlockedFlag` clears F405 only if F505 was already set. Fixed text 363, Bowie HP restored to maximum, gold halved by unsigned shift, then `GetEgressPositionForBattle` (`0x23E50`), d4=-1. Battle04's special completion/upgrade/capture tail is not taken. | With predecessor-selected `EGRESS_MAP=16` and F399 set, Map16 `(27,7)`, Up from `data/maps/global/savepointmapcoords.asm`. No F505 write or after/join program. Subsequent exploration performs the ordinary living/immortal heal; this is not revival of every fallen ally. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` respectively charges spell MP or removes the chosen item slot, then flashes/closes text, updates the unlock flag and obtains egress position, d4=0. | Same conditional Map16 `(27,7)`, Up. No after-program, Slade join or F406 producer. F505 clear leaves F405 armed; F505 set disperses F405. No defeat gold penalty. |

`BattleLoop` sets F399. `GetEgressPositionForBattle` has no Battle05 exception and reads the selected
`EGRESS_MAP`; a different incoming egress selection gives a different return. Calling
`GetSavepointForMap` (`code/common/maps/egressinit.asm`, `0x75EC`) with F399 clear instead selects the
game-start tuple, and an unlisted map defaults to `(1,1)`, Up. Do not turn the conditional Galam
savepoint into a universal destination.

The accepted predecessor's `UpgradeBattle` limitation remains: its Battle04-specific defeat call
scans `list_RandomBattles` and sets a completion flag only for a listed battle. Battle04/05/06 are
not listed. `battle4DefeatUpgradesBattle=true` in the older `battle-control` summary denotes a call,
not an enemy upgrade, increment of current battle or unlock of the next one. Battle05's actual F406
producer is the after-program below.

## After-program: jewel, recruitment and return

**Confirmed:** `data/battles/entries/battle05/cs_afterbattle.asm`, `abcs_battle05`
(`0x49B48`), uses cursor 2332. Its ordered state-bearing commands are:

| Source-site index in retained map-script-engine corpus | Effect / actual consumer |
| --- | --- |
| 11: `setF 384` | Sets F384 during the jewel dialogue. Neither this program nor `edcs_battle5` awards an inventory slot or removes a jewel from Slade's inventory. The narration is not an `AddItem` transaction. |
| 18: `join ALLY_SLADE` | `csc08_joinForce` (`0x47398`) selects ordinary join music, calls `j_JoinForce(5)`, gets class and shows fixed ID 446 with name/class substitutions, then waits/closes text. This fixed message does not advance the program's text cursor. |
| `JoinForce` (`code/common/stats/battleparty.asm`, `0x9900..0x99DE` owner) | The `s02_jumpinterface.asm` alias resolves to the implementation. Sets joined flag F5 and calls `UpdateForce`; if the active-member count is below 12, `JoinBattleParty` sets active flag F37. Full-party input does not force Slade active. Join itself does not revive him or set HP. |
| 21/22: `clearF 73`; `setStoryFlag 6` | Clears the persistent Slade follower selector, then `csc13_setStoryFlag` adds 400 and sets F406. Does not change `CURRENT_BATTLE` or synchronously admit a new battle. |
| 23/24: `followEntity ALLY_SLADE,ALLY_BOWIE,2`; `warp ...` | The follow handler uses its character-alive gate and `AddFollower` for the current entities. This relationship is separate from F73 and force membership. Warp requests Map16 `(16,18)`, Down; `csc07_warp` writes the event fields for the victory consumer above. |

After-table slot 5 is `0x47CFE`; the consumed join-table slot is `0x47D6F` in
`data/battles/cutscenes/afterbattlejoins.asm`. Its zero byte reaches ordinary `JoinForce(0)` even when
the after-script was skipped; it is not a no-call sentinel or the Slade recruitment mechanism.
Do not confuse it with the excluded namesake under `data/battles/global/`.

The predecessor established Slade's scene/follower presence before Battle05, not joined membership.
This slice locates the explicit join after first-completion victory without retroactively placing
him in the pre-battle force. Rebuilt exploration entities depend on selected setup and follower
flags, so the script's temporary following relationship does not prove Slade remains visibly behind
Bowie after the map return.

## Direct exit content and conditional Battle06 selection

No flag-switched-map row remaps Map16, Map18 or Map66. `GetCurrentMapSetup` keeps the last matching
selector: Map16 default → F663 → F530; Map18 default → F530; Map66 has one setup. The early-story
route below assumes F530 clear. F663 was produced by the predecessor's army-departure zone when that
zone was invoked; Battle05 victory does not itself set it. Later selected setups are safe alternative
endpoints, outside this route.

| Stage / selected data | Predicate and actual consumer | Supported boundary |
| --- | --- | --- |
| Map16 victory return | Default/F663 share `ms_map16_InitFunction` (`s6_initfunction.asm`): F664 set hides entity 145. Their zone X=3 handler returns immediately on F505, before writing `EGRESS_MAP` or reissuing the guard scene. Zone X=17 still tests F663 independently. | Exploration at the supplied `(16,18)`, Down, with setup-specific actors. F505 prevents the already-completed guard battle trigger; it does not prove every route through the castle is open. |
| Map16 south exit (`6-warp-events.asm`) | Record `(255,31)`, no-scroll, destination Map18 `(9,10)`, Down. As with other content warps, the record must be selected by the movement/warp consumer; a record is not proof of arrival. | Direct drawbridge-map handoff. Other exits are not removed: Map16's north exit `(255,0)` instead supplies Map66 `(32,4)`, Down; its physical reach is unproved. |
| Map18 layout preparation (`3-flag-events.asm`) | F611 controls both copies: source `(19,0)`, size `(18,30)` to `(0,0)` and source `(38,30)`, size `(18,18)` to `(38,0)`. `LoadMapBlocksAndLayout` in `code/common/maps/mapload.asm` calls `CopyMapBlocks` only for set flags. The source labels call these drawbridge-down/new-roof copies. | The predecessor's Map17 init sets F611 unconditionally after its F660/F663 branches; it is not conditional on F663 despite the data comment. F611 clear is a different working layout. No layout was decoded here, so neither copy proves a traversable route. |
| Default Map18 setup | `s6_initfunction.asm` (`0x52B48`) and default zone target return directly; step-event content is empty. Four logical NPCs 128–131 are merchant, man and two guards. | No mandatory init/zone script supplies F406 here. Optional conversation content is below; those handlers do not gate or unlock Battle06. |
| Map18 south exit (`6-warp-events.asm`) | Record `(255,29)`, no-scroll, destination Map66 `(31,9)`, Down. North exit `(255,9)` returns Map16 `(26,30)`, Up; the two tower-stair records stay in Map18. | A conditional two-map exit chain exists. Legal walking from castle return to its south marker and across the drawbridge to this marker remains **Unknown**. |
| No-scroll return request | `WaitForEvent` → `ProcessMapEvent` clears the pending type; `ProcessMapEventType1_Warp` with parameter 1 zero unwinds exploration and supplies map/X/Y/facing to `MainLoop`. Nonzero parameter 1 instead stays in exploration's scrolling branch. | `SwitchMap` leaves Map66 unchanged, then `CheckBattle` runs before Map66 exploration setup/init/zone dispatch. No new Map66 zone event is required to arm F406. |
| `CheckBattle` (`code/common/maps/getbattle.asm`, `0x799C`) | Ascending row scan; matching map, set unlock flag and matching/wildcard trigger bytes. On Map66, rows 2/3/4 precede row 6, all with wildcard triggers. Thus F402/F403/F404 must be clear for row 6 to win; F406 must be set. F506 is not a rejection: if set, the routine clears F406 but still returns battle 6 for this selection. | Row 6 supplies area `(21,2,20,32)` and trigger `(255,255)`. The area rectangle is output, not an admission containment test. Both selected Map66 exit tuples can match row 6; their natural paths are distinct and unproved. |

A useful counterexample is F406 clear with F505 already set: repeating the victory wrapper skips the
only after-program producer in this slice, and merely reaching Map66 does not arm Battle06. Another
is F403 set alongside F406 with F402 clear: row 3 wins even if F503 is already set, since completion clears its
unlock only after selecting that encounter. Numeric chronology cannot substitute for flag state.

Optional return to Map17 also differs after victory: F505 selects `ms_map17_flag505` (unless F530
wins), whose zone table contains only the default return target. It reuses the existing init and
entity-event pointers. The prison/shortcut/tunnel zones from its default setup are therefore not
replayed merely because the player revisits. This does not close the predecessor's arbitrary
reentry, door/shortcut reconstruction or follower-state Unknowns.

### Dialogue and actors

**Confirmed:** the following IDs were checked against script operands and hexadecimal entries in
`data/scripting/text/gamescript.txt`. They describe the ordinary unskipped dialogue branch. Debug
skipping may suppress cursor increments; no rendered speech or off-screen story assertion is proved.
Only brief summaries are published.

| Caller | IDs / actor binding | Content and limit |
| --- | --- | --- |
| `edcs_battle5` | Cursor 2323: 2323–2331 (`0x913..0x91B`). 128, Slade and no-portrait narration. | The knight recalls jailing Slade; Slade searches his bag, finds the Jewel of Light, and asks about the other jewel. The knight says King Galam took it. No join, inventory award or onward unlock occurs in this script. |
| `abcs_battle05` | Cursor 2332: 2332–2341 (`0x91C..0x925`). Slade and no-portrait narration; fixed join ID 446 is separate. | Slade offers the jewel; narration says it fuses to Bowie's neck. Slade discusses a later church visit and accompanying Bowie toward Granseal to find the other jewel. F384 and the actual join/unlock consumers above are distinct from that dialogue. No mandatory church event is called. |
| `Map18_EntityEvent0` | Fixed 870–872 (`0x366..0x368`), NPC 128 / narration with portrait and speech-SFX handling. | Trader admires the jewel, tries and fails to remove it. No F384 condition is checked by this handler; it is source-available dialogue, not proof that arbitrary incoming state owns the jewel. No item transfer occurs. |
| `Map18_EntityEvent1` | Fixed 873–874 (`0x369..0x36A`), NPC 129. | Concern for a mother visiting Granseal and the unexpected war. |
| `Map18_EntityEvent2` | Fixed 875–876 (`0x36B..0x36C`), NPC 130. | Guard asks about strangers and states the king's order to stop departures. Dialogue alone does not close collision or exit reachability. |
| `Map18_EntityEvent3` | Fixed 877–878 (`0x36D..0x36E`), NPC 131. | Guard describes the unused rusty bridge and excludes civilians from the tower. No bridge flag mutation occurs here. |
| Battle06 `bbcs_06` | Cursor 2342: 2342–2356 (`0x926..0x934`). 129/130 guards, Slade and 128 officer; Bowie is staged but has no text operand. | Guards stop the party; Slade claims to carry something left by King Galam. The officer offers to deliver it, recognizes the escapees and identifies them as Granseal spies. The failed excuse leads into battle staging, not a successful passage to Granseal. |

## Battle06 before/start stopping boundary

**Confirmed:** before-table slot 6 (`0x47A94`) selects `bbcs_06`
(`data/battles/entries/battle06/cs_beforebattle.asm`, `0x49BCA`), guarded by F456 in
`ExecuteBeforeBattleCutscene`. The program's `loadMapFadeIn Map66,25,5` is map/camera staging, not
an exploration warp to player coordinates `(25,5)`. It loads `ce_49CBA`, staging Bowie at `(30,9)`,
Slade at `(29,8)` and three guard actors (128–130). These are scene entities, not the combat roster.
The before-script ends with `setStoryFlag 7` (retained command 45), setting **F407**. That write is
an entry-side effect, not Battle06 completion or proof of Battle07 admission; no later route is
investigated here.

For F88 clear, `BattleLoop` installs map/battle, runs the before wrapper, restores map/battle,
clears region flags 90–105, heals living/immortal allies, initializes allies/enemies, clears AI memory,
loads the battle and calls `ExecuteBattleStartCutscene`. Start-table row 6 (`0x47B38`) is `ms_Empty`.
The start wrapper sets F456 before dispatch when it was clear. If F456 was already set, both before
and start scripts are skipped, so the F407 write is not repeated. F88 set takes the suspended entry,
clears F88, restores the saved seconds counter, clears AI memory and reloads battle state without
these new battle initialization and intro steps.

`data/battles/spritesets/spriteset06.asm` (`0x1B365C`) supplies 6 ally placements, 9 enemies,
1 AI region and 0 AI points. `InitializeAllAlliesBattlePositions` consumes the actual joined/active
party and skips dead members; the placements do not recruit named characters or prove a particular
six-member force. The before-scene's Slade actor is likewise not proof of incoming membership.
Natural Battle06 entry, actual party/HP, script-service completion, presentation and winnability
remain **Unknown**. Battle06 outcomes and later content are excluded.

## Existing coverage and gap disposition

Fixture names below are under `tests/fixtures/h2/`; commands have prefix `uv run sf2`.
Coverage refers to each owner's admitted boundary, not a freshly executed entire rail. The existing
`battle_cutscenes._cutscene_facts` source guards were run directly against the pin and matched the
complete `cutsceneFacts` fixture, including the HP-write/position-only distinction.

| Claim / owner | Disposition and existing surface | Missing relation or discriminating control |
| --- | --- | --- |
| Battle05 outcome and defeated-position consumers | **Covered** at guarded source scope by `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json` and `battle-global-data-static-v1.json`; [battle loop](./battle-loop.md), [battle global data](./battle-global-data.md). Commands: `h2 battle-control`, `battle-loop`, `battle-cutscenes`, `battle-global-data`. | `battle_cutscenes.py` guards the leader-death loop, distinct position-only tail and record application; its fixture explicitly pins 128–157 HP-zero versus 158/159 X-only. Exact selected table/caller composition here is direct source review. Controls: Bowie dead suppresses it; F505 does not gate the position helper; leader table zero does not erase that helper. |
| After-program recruitment, state and warp | **Covered** by `map-script-engine-static-v1.json` force/story/transition source sites and handler guards, plus shared common-stats identity; [common scripting](./common-scripting.md). Command: `h2 map-script-engine`. Retained payload fixture check passed. | Anchors: `abcs_battle05` 11/18/21/22/23/24. Controls: full active party does not force F37; join does not heal; F73 and current follow relationship differ; F505-set wrapper skips all these commands. Shared table zero joins Bowie, not Slade. |
| Selected map/setup/init/entity/zone consumers | **Covered** as table/stream/target structure by `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json` and `map-events/entity-programs.json` / `zone-programs.json`; [map data](./map-data-inventory.md). Commands: `h2 map-setup`, `map-entities`, `map-init`, `map-events`. Retained init comparison passed. | F530 changes setup priority; F505 suppresses the Map16 guard producer and selects Map17's default-only zone table. Map18 NPC speech neither proves F384 nor changes F611/F406. Selected handlers were read directly; target inventories do not prove natural invocation. |
| Battle05 after → Map16/18 exit → Map66 row 6 | **Reuse/extend** the accepted conditional-admission candidate in the predecessor through map-script-engine, `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json` and [map-content](./map-content.md). Narrow owners: `h2 map-script-engine`, `map-event-request-consumption`, `common-maps`, `map-content`. | Missing executable composition is the F406 producer, victory-return consumption, chosen no-scroll request and ascending row selection. Minimum public-safe output would retain source/caller identities, input flags, request fields and selected row, not layouts/dialogue. Existing rails cover separate handlers/data, not this composed conditional claim. Controls: F402 clear/F403+F406 set selects 3; F506 still selects 6 once; nonzero parameter 1 stays in exploration; first return bypasses CheckBattle. No standalone per-battle fixture or new command is justified. |
| Castle/drawbridge walking and reentry | **Reuse/extend map-content** only if a later accepted claim needs static path analysis; otherwise **deferred runtime Unknown**. Command: `h2 map-content`. | Need specified working layout, markers, actor/follower state and start/target coordinates. Minimum result is conditional reachability for the two south-exit legs. Controls: F611 clear versus set, and north versus south exit. Existing warp/copy records do not establish collision/path reachability. Preserve the predecessor's prison/shortcut reentry gaps; no automatic emulator queue. |
| Battle06 before/start and data | **Covered** at selected source/table structure by map-script-engine, `battle-cutscene-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-spriteset-data-static-v1.json`; [cutscene data](./battle-cutscene-data.md), [routing data](./battle-routing-data.md), [spriteset data](./battle-spriteset-data.md). Commands: `h2 battle-cutscene-data`, `battle-routing-data`, `battle-spriteset-data`. | Routing's `routeTargetsParsed=false` prevents claiming full target semantics from that inventory. Direct source and retained `bbcs_06` command 45 establish this F407 write. Controls: F456/F88 skips; scene actors differ from active force. **Deferred runtime Unknown:** natural entry, service timing and completed presentation. Stop before resolution. |

No new bounded H2 fixture is implemented or required for this document. The useful static conditional
chain is closed at Battle06 selection; natural walking and arbitrary state closure remain explicit
safe endpoints rather than a campaign-wide investigation or a new platform.

## Reproduction and handoff boundary

Work is bounded source/fixture/verifier reading and small in-memory selections. The retained JSON
payloads total 18,285,979 bytes (approximately 18.3 MB) and were not rewritten. No map decoding,
extraction, capture, emulator/Godot launch, installation or new environment was performed.

Load `local/private-inputs.ps1` in the same PowerShell process as each tool. `$upstream` denotes the
verified read-only pinned source selection, not a machine path for publication. The accepted
predecessor owns the completed `uv run sf2 rom verify`, `uv run sf2 h2 map-init --upstream-path $upstream`
and `uv run sf2 h2 map-script-engine --upstream-path $upstream` receipts; those commands were not
rerun here. Set `$upstream` to that verified checkout and reproduce this slice's retained-payload
and source-guard comparisons from the owning checkout with:

```powershell
. ./local/private-inputs.ps1
@'
import json
import sys
from pathlib import Path
from sf2tool.h2.battle_cutscenes import _cutscene_facts
from sf2tool.h2.map_script_engine import _verify_map_script_engine_fixture
read = lambda p: json.loads(Path(p).read_text(encoding="utf-8"))
f = read("tests/fixtures/h2/map-init-static-v1.json")
o = read("local/derived/map-init-static.json")
assert f["upstreamCommit"] == o["upstream"]["commit"]
assert f["romSha256"] == o["romSha256"] and f["function"] == o["function"]
metadata = {"schemaVersion", "id", "upstream", "romSha256", "function"}
assert f["expected"] == {k: v for k, v in o.items() if k not in metadata}
_verify_map_script_engine_fixture(
    read("tests/fixtures/h2/map-script-engine-static-v1.json"),
    read("local/derived/map-script-engine-static.json"))
assert _cutscene_facts(Path(sys.argv[1]) / "disasm") == read(
    "tests/fixtures/h2/battle-cutscenes-static-v1.json")["expected"]["cutsceneFacts"]
print("retained map-init/map-script-engine and cutscene source comparisons: PASS")
'@ | uv run python -X utf8 - $upstream
```

Complete prose, source/fixture references, links and private-boundary review precede exact-path
staging. Run `git diff --check` and committed
`uv run sf2 verify plan --base origin/main --head HEAD`; record exact Git identities, planner, actual
CI, any completed failures and stopped-writer state on the Issue/PR handoff. Normal/full/engine/H3/H4
are not selected for this documentation-only agreement. Preserve ignored inputs/results and leave
independent acceptance and merge to main-gate.
