# Battle 01 return to source Battle 02 admission

Evidence date: 2026-10-12. **Confirmed:** the pinned source contains a conditional connection
from the Battle 01 return, through the castle's exorcism and commission programs and Granseal's
Jaha event, to the unlock consumed by source Battle 02. **Inferred:** these edges form the intended
next story segment. **Unknown:** a continuously legal, naturally executed journey between the
accepted return endpoint and Battle 02 admission. Map and battle numbers alone establish no order.

## Scope and evidence boundary

This is the Standard-rigor research agreement for [Issue #691](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/691):
a bounded, cross-owner content handoff and fixture assessment, with no new extractor, fixture,
campaign database, runtime acquisition, or remake implementation. The only owned tracked path is
this document. Acceptance is a source/fixture-backed transition table, explicit missing joins and
candidate counterexamples, direct document checks, the committed verification plan, and actual CI.
Independent main-gate review remains required. Battle 02 resolution and onward travel are excluded.

The inspected baseline is accepted main `56eef43667f84fd8afb21c5bc08a5c7425bd74d3`.
All source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The upstream checkout was clean at that exact commit. The registered USA ROM passed identity
verification: SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Addresses below identify the pinned source/H1 owners; this note adds no new ROM execution evidence.

The [victory-return H2 owner](./map3-battle01-victory-return.md) confirms the static
`BattleLoop_Victory` → after-program → F401 clear/F501 set → `MainLoop` return spine.
Its historical runtime Unknowns are not the current project's entire original frontier. The
[accepted original acquisition](./map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
and [current readiness owner](../../../design/synthesis/milestones/map3-battle01-readiness.md#accepted-original-frontier)
also establish bounded original victory and settled Map57 `(5,12)`, plus a separately accepted
ordinary Down displacement to `(5,13)`. Those are terminal, nonresumable observations, not observations
of the extension investigated here. The accepted remake milestone is a separate composed boundary;
neither it nor unmerged Issue #690 material is used to prove this route.

Source names in `sf2enums.asm` identify Map57 as `MAP_ANCIENT_TOWER_ENTRANCE`, Map40 as
`MAP_ANCIENT_TOWER_EXTERIOR`, Maps21/20/19 as the source-named castle 3F/1F/2F, Map3 as
`MAP_GRANSEAL`, and Map66 as `MAP_OVERWORLD_GRANSEAL_KINGDOM`. These labels are identifiers,
not a reason to infer floor adjacency. `BATTLE_TO_YEEL = 2` identifies the zero-based source table
slot, not an independently verified player-facing battle number.

## Conditional transition and content table

**Confirmed** in this table means the stated static edge/operand, not a traversable walking path or
an executed presentation. A next endpoint is safe for further inspection; it is not a claim that
the original player reached it in this investigation. `Fnnn` denotes a game flag, not a text ID.

| Stage | Source-selected state, content and next endpoint | Evidence and limit |
| --- | --- | --- |
| Battle 01 return | `abcs_battle01` loads Map57 and `ce_497F6`; its source actors include the mist-demon entity 128 and Astral entity 129, with Bowie/Sarah/Chester commands. The program shows the spirit leaving and directs the party back toward the King. Victory subsequently clears F401 and sets F501. | **Confirmed:** `data/battles/entries/battle01/cs_afterbattle.asm`, `abcs_battle01` at `0x496DC`; `sf2-map3-battle01-victory-return-static-v1`. The source main-entity placement in `ce_497F6` is not substituted for the observed `(5,12)` field endpoint. Map57 has no `msMap` row and selects the void setup. |
| Tower exit to castle | Map57 wildcard `y=19` → Map40 `(15,13)`, Down; Map40 wildcard `y=31` → Map21 `(9,2)`, Down; Map21 `(3,16)` → Map20 `(3,36)`, Right. | **Confirmed table topology:** the three maps' `6-warp-events.asm`. Map40 selects default `ms_map40` under the early-state assumptions below. Map21 selects `ms_map21_flag501`, whose init `cs_53F2A` places entity 128 at `(6,16)`. **Unknown:** legal movement between these seams after victory, including marker/collision/occupancy effects. |
| Reach the royal-room event | One table-connected chain is Map20 `(6,37)` → Map19 `(6,2)`, Left; Map19 `(23,3)` → Map20 `(23,37)`, Down; Map20 `(25,35)` → current Map20 `(25,50)`, Right. Map19 and Map20 use their F501 setups. | **Confirmed table topology:** Map19/20 `6-warp-events.asm`; `pointertable_501.asm`. Map19 init is the internal direct-return entry `ms_map19_flag_501_InitFunction`, not the default init prefix. Map20's internal `ms_map20_flag501_InitFunction` still tests F507. This is a candidate connection, not the only possible path or a proven route through guards. |
| Exorcism, zone entry | `ms_map20_flag501_ZoneEvents` selects `(27,52)` → `Map20_1F5_ZoneEvent0`. That program gets/sets maximum HP and MP for allies 1 and 2, then calls `cs_53B66`. The script starts with cursor 2197, proceeds through internal label `cs_53C42`, then sets F609 and issues `warp MAP_GRANSEAL_CASTLE_2F,29,8,UP`. | **Confirmed:** Map20 `s3_zoneevents_501.asm` at `0x53766`, selected program `0x5376E`; `scripts_1.asm`, `cs_53B66` at `0x53B66`, shared suffix `0x53C42`. The zone is selected only when the controller dispatches a zone event at those coordinates. A record alone does not prove that dispatch. |
| Exorcism, alternate entity entry | F501 setup entity 130, facing condition Down, selects `Map20_1F5_EntityEvent2`, which directly calls `cs_53C42`. This enters the shared suffix with cursor 2200; it omits the zone caller's HP/MP preparation and the script prefix. | **Confirmed:** Map20 `s2_entityevents_501.asm`; `sf2-map-events-static-v1:entity-programs` and `:script-invocation`. It is a separate static caller, not proof of a naturally reachable shortcut or equivalent initial actor state. Do not make the `(27,52)` zone the exclusive source route. |
| Castle commission | F609 selects `ms_map19_flag609`. Its init tests F610; clear runs `cs_53176`, then sets F610 only after the script returns. Set skips the script. The script stages a departing soldier group, then the King/Minister's request to fetch Hawel near Yeel. `cs_53482` repeats the prompt on the clear-F89 branch; set F89 enters `cs_534AC`, ending with Sarah following Bowie and Chester following Sarah. | **Confirmed:** Map19 `s6_initfunction_609.asm` at `0x5310C`; `scripts.asm`, `cs_53176` at `0x53176`, `cs_53482`/`cs_534AC`. The F89 branch is source control flow; no claim is made that this investigation displayed or answered the prompt. F610 is not read by the later Jaha event. |
| Castle to Granseal | Map19 wildcard `y=31` → Map3 `(29,2)`, Down. With F609 selected, `ms_map3_flag609` supplies the Jaha entry at `(1,29)` and the two west-side zone records. The init hides `ALLY_JAHA` if F3 is already set. | **Confirmed:** Map19 `6-warp-events.asm`; Map3 `pointertable_609.asm`, `s1_entities_609.asm`, `s6_initfunction_609.asm`. Map3's F640 map-switch override must be clear; later setup overrides are not silently assumed absent. |
| Jaha join and unlock | The first zone record `(2,23)` selects `Map3_261_ZoneEvent0` (RTS). The later `(2,255)` record selects `Map3_261_ZoneEvent1`. If F3 is set it branches to that same RTS. If clear, it calls `cs_516A8`, then sets F402 and clears F66. The script uses cursor 662, addresses `ALLY_JAHA`, joins him and installs following behind Chester. | **Confirmed:** Map3 `s3_zoneevents_609.asm` at `0x50EEA`, program `0x50EF6`; `scripts_1.asm`, `cs_516A8`. First-match precedence means `(2,23)` does **not** run the wildcard join case. The event's caller after script return owns F402/F66; the `join` command is not itself the battle-unlock write. |
| West exit | Map3 wildcard `x=0` → Map66 `(29,32)`, Down. This is the target map consumed by Battle02's row. | **Confirmed:** Map3 `6-warp-events.asm`; battle-global row 2. **Unknown:** a collision/marker-aware walk from the join event to this warp. An arbitrary coordinate match does not invoke a warp without its controller event marker. |
| Admission boundary | `MainLoop` applies `SwitchMap`, calls `CheckBattle`, and passes returned `d7` to `BattleLoop` when it is not `-1`. For Map66 with F402 set, row 2 is the first eligible row: Maps in rows 0/1 differ, row 2's triggers are both wildcards. It writes battle area `(16,19,22,18)` and returns index 2. | **Confirmed source-derived selection:** `code/gameflow/mainloop.asm` (`0x75C4`), `code/common/maps/getbattle.asm` (`0x799C`), `table_BattleMapCoordinates` (`0x7A36`) row 2 at `0x7A44`. The primary route stops at this admission call. The encounter dependencies below identify what the new-battle branch would consume, not a Battle02 player-ready observation. |

The selected early-state interpretation requires F501 set, F609 initially clear, F3 initially clear,
and F402/F502/F452/F88 initially clear. Treat these as explicit scenario assumptions beyond what
the named observed endpoint actually exports; this note does not reconstruct a complete endpoint
flag bitset. Later selector flags must not override the named setups: Map3 has 506/543 after 609;
Map19 has 609/506/507/543/982 after 501; Maps20/21 have 609/506/543 after 501; Map40 has 506/507
after its default. The selector is last-set-flag-wins, not first-set-flag-wins. After F609 is set,
the intended rows on Maps3/19/20/21 change accordingly. F640 switches Map3 to Map4 before its
setup is considered. Map66 has its default setup without a flag alternative.

These constraints describe a conditional source path. They are not assertions that all flags retain
these values through natural play. In particular, F610 set can skip the commission; F3 set with
F402 clear can skip the unlock writer. Neither input combination is repaired by the inspected event.

## Dialogue and actor evidence

The public handoff keeps cursors, identities, short original summaries and branch effects. It does
not reproduce dialogue. The source comments support the narrative summaries; the executable proof
is the cursor/command/caller relation. A sprite name alone cannot identify a speaker.

| Program | Text and actor relation | Boundary |
| --- | --- | --- |
| `abcs_battle01` | Cursor 2305; successive dialogue commands select entity 129. `ce_497F6` explicitly loads that actor with `MAPSPRITE_ASTRAL`; the other cutscene actor is the mist demon. | **Confirmed** authored binding and cursor progression. Use the original acquisition for which operations actually occurred; do not equate the complete static command corpus to the reached subset. |
| `cs_53B66` / `cs_53C42` | Cursor 2197 at full entry, reset to 2200 at the suffix. Text operands address entity 128 and 130; Map20 F501 entity data binds these to King Granseal and Astral sprites. The final narrative command uses the full no-entity sentinel. | **Confirmed** source binding, distinct caller prefixes and text state. The zone caller's roster restoration is not inherited by the entity caller. Rendered portrait, speech and player timing remain **Unknown** here. |
| `cs_53176` / choice labels | Cursor 721 at entry; refusal explicitly resets to 757; acceptance explicitly resets to 758. Entity 138/140 text commands join the selected Map19 F609 Minister/King sprite entries. `newEntity 144` supplies the speaking soldier. Sarah and Chester use explicit ally operands. | **Confirmed** authored identities and F89 loop. The story-flow handler `csc11_promptYesNoForStoryFlow` sets F89 for a zero `YesNoPrompt` result, clears it otherwise; source labels/comments interpret the accepting branch. This is not a captured prompt response. |
| `cs_516A8` | Cursor 662 and three `nextSingleText` commands address `ALLY_JAHA`; the `join` command lies between the second and third. The join handler has its own message path and does not turn that service text into another cursor step. | **Confirmed** command order. `csc08_joinForce` → `JoinForce` sets the ally's joined flag (F3 for Jaha), updates force membership and conditionally joins the battle party if capacity permits. Four authored battle placement rows alone do not prove four live admitted allies for every caller. |

Map19 F501 entity-event text IDs 583–586 and Map20 F501 IDs 588–596/2198 provide optional
local reactions around the return; they are not additional unlock prerequisites. Their event tables
retain facing conditions and temporary flags 256/259–262. The directly relevant exception is the
entity-130 script call identified above. No per-NPC fixture or complete dialogue catalog is proposed.

The existing [dialogue-state rail](../../world/map-event-dialogue-state.md) covers its selected 24-program
state-access denominator, not every story speaker or every map-script program. For these scripts,
use the [common scripting](../../world/common-scripting.md) dialogue family, program references and
sprite-dialogue lookup boundary, together with the selected setup entity list. A successful
handler-local H3 case does not establish natural entry into these callers.

## Battle 02 data and admission semantics

**Confirmed:** the reproduced [battle-global rail](../../battle/battle-global-data.md) emits row 2 as
`[66,16,19,22,18,255,255]`. `CheckBattle` tests the unlock flag before trigger coordinates.
The area origin/size are output fields, not a geometric admission test. F402 is the gate, not
proximity to an enemy, a required region, F610, or the fact that Battle01 ended. If F502 is set,
the selected branch clears F402 but still returns index 2 on that call; it does not skip immediately
to a later row. A later call sees the changed unlock state. This distinction matters for a future
admission contract even though the intended first-entry assumptions have F502 clear.

**Confirmed source/data owners:**

- `data/battles/spritesets/entries.asm` slot 2 selects `BattleSpriteset02` at `0x1B3376`, owned by
  `spritesets/spriteset02.asm`. Its header is 4 ally placements / 6 enemies / 0 AI regions / 1 AI
  point. The ally placement IDs are 0–3; the enemy source types comprise two `HUGE_RAT` and four
  `OOZE`. All ten `combatantBehavior` records use `STARTING`. These are source declarations,
  not a claim that every actor is live, spawned, activated, or targetable under every entry state.
  The [spriteset inventory](../../battle/battle-spriteset-data.md) validates slot/include/header/macro shape;
  its public fixture explicitly has `rowContentTracked: false`.
- `data/battles/terrainentries.asm` slot 2 selects `BattleTerrain02`; it is not an alias slot.
  The [terrain/routing owner](../../battle/battle-routing-data.md) owns complete deterministic terrain decode
  and pointer/payload parity. No terrain grid needs to be copied into a new public fixture.
- `rpt_BeforeBattleCutscenes` row 2 and `rpt_BattleStartCutscenes` row 2 both target `ms_Empty`.
  The former slot is at `0x47A8C`, the latter at `0x47B30`. The missing `battle02` script directory
  is therefore not evidence of missing extraction. The generic wrappers use intro flag F452;
  before-battle only tests it, battle-start sets it before dispatch when it was clear. Empty script
  selection does not mean that the wrapper or its flag effect is absent.
- The clear-F88 branch of `code/gameflow/battle/battleloop_1.asm` installs current Map/Battle,
  runs the before wrapper, clears F90–F105, heals/initializes combatants, clears AI memory,
  calls `LoadBattle`, then the start wrapper. The saved-battle F88 branch is separate. Existing
  [Battle01 admission](./map3-battle01-admission.md) guards this shared sequence but binds its
  selected data to Battle01, not Battle02. `initializecombatants.asm`, `activateenemies.asm`,
  `triggerregions.asm` and `populateenemyspawns.asm` under `code/gameflow/battle/battleloop/`
  own placement/activation consumers. Battle02's actual initialized roster and first turn remain
  **Unknown** in this note; no first-turn or battle-resolution evidence is claimed.
- Map66's ordinary `ms_map66_Entities` is empty and its init is RTS. Neither fact describes its
  battle roster. Admission is tested by `MainLoop` before the ordinary exploration branch.

## Existing evidence and candidate dispositions

Fixture paths below are under `tests/fixtures/h2/`; IDs use the `sf2-` prefix shown in each file.
This inventory assessment used the fixture payloads and corresponding `src/sf2tool/h2/` verifier
code, not just the research summaries. Existing structured program rows remain the source of truth;
the table above is a readable selection, not a replacement parser.

| Apparent gap | Disposition and exact owner | Missing claim or meaningful counterexample |
| --- | --- | --- |
| Victory return, setup priority, physical warp records, entity lists | **Already covered by accepted fixtures:** `map3-battle01-victory-return-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-content-static-v1.json`. Reuse `h2 map3-battle01-victory-return`, `map-setup`, `map-entities`, `map-content`. | Do not add duplicate records for these maps. Their data/selector coverage does not assert marker-aware connectivity between the selected record endpoints. |
| Caller and script ownership | **Already covered:** `map-events/zone-programs.json` retains `Map3_261_ZoneEvent1:331510` and `Map20_1F5_ZoneEvent0:341870`; `entity-programs.json` and `script-invocation.json` retain the alternate `cs_53C42` call. `map-init-static-v1.json` retains the F609 init route/script call. `map-script-engine-static-v1.json` retains program/site/handler joins. Commands: `h2 map-events`, `map-init`, `map-script-engine`. | Do not flatten the internal script label into full-entry execution or drop the exact `(2,23)` record ahead of `(2,255)`. These differences are already representable in the existing corpus. |
| Flag relations and dialogue identity | **Reuse existing rails**, not new per-character fixtures: `map-event-flag-route-selection-static-v1.json` joins participating event programs to their setup topology; `map-script-engine` owns script flag/control/dialogue/force commands. | The flag-route rail's event-program denominator is not a cross-interpreter proof that `cs_53B66` sets F609 and later `cs_516A8` executes. Its topology is not temporal order. Likewise the 24-program `map-event-dialogue-state` rail is not universal speaker evidence. |
| Legal walking between listed seams | **Existing rail can be reused/extended**, only if a later task needs a static route claim. Use the decoded `map-content`/canonical-map-import and existing bounded route mechanisms used by `h2 map3-battle01-admission`; grant new ownership separately. | Missing: reachable event-marker terminals with selected setup occupancy and block-copy state. Minimal counterexample: a wildcard warp row matches an ordinary walkable cell with no warp marker, or a guard blocks the connection. Do not publish an arbitrary input sequence as gameplay legality. A path need not be added merely to make this document useful. |
| Battle02 roster/activation details | **Existing rail can be reused/extended:** `battle-spriteset-data`, existing battle-position/AI consumers and `battle-terrain`; no second spriteset/terrain importer. | The spriteset inventory counts headers/macros and locks source identity; it does not parse complete per-combatant behavior operands into its public contract. If live admission becomes required, join one selected record to its consumer rather than assuming `STARTING` or four rows proves live membership. Counterexample: Jaha absent from active party, or one behavior's activation operand changed while header counts remain unchanged. This scope stops short of activation/turn acceptance. |
| Selected Battle02 admission and empty dispatch | **Existing rails can be extended**, as detailed below. `common-maps-static-v1.json` guards shared `CheckBattle` fragments; `battle-global-data` parses all map rows; `battle-routing-data` explicitly records `routeTargetsParsed: false`; the deeper admission rail hard-codes row 1. | Missing executable join: row 2 + F402/F502 branches + main-loop dispatch + before/start slot targets/F452. Neither aggregate dimensions nor a row-1 golden proves this selected relationship. This is not sufficient reason to create a separate fixture for every battle. |
| Continuous original chronology, selected callers, presentation and persistence | **Deferred runtime Unknown.** No new H3 proposed or run. | The terminal original return does not supply a resumed trace to these events. Caller states, actual selected paths, visible speakers/portrait/audio, control handoff and persistence remain unobserved here. Any later H3 must independently satisfy [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)'s caller-dependence, material-contract-impact and existing-rail-reuse gate. |

### Candidate: extend admission selection and cutscene routing owners

This candidate supports a future content handoff that must distinguish “source slot exists” from
“these inputs select it.” It is not implemented by this Issue. **No new standalone H2 fixture is
justified here:** the missing deterministic relationships have existing owners that can carry them.
In particular, another battle-specific importer or a full-campaign route framework would duplicate
those owners without closing the caller-state and walking questions.

The bounded source surface is `getbattle.asm:CheckBattle`, `mainloop.asm:MainLoop`, authoritative
constants in `sf2enums.asm`, battle-global row 2, and the before/start wrappers and table slots
named above. Retain existing table parsers and H1/ROM identities. The owning narrow commands would
remain `uv run sf2 h2 common-maps` for the selector and `uv run sf2 h2 battle-routing-data` with
`uv run sf2 h2 battle-cutscenes` for target/consumer joins. Their current contracts are not claimed
to contain this extension. A minimal public-safe addition contains selected row/index/area, gate
flags, ordered branch outcomes, call/return targets and empty-program identities, using Battle02
as the bounded witness. It need not encode a full walkthrough, initial combatant roster, assets,
text, timing or turn order. The source identity for the shared `MainLoop` call order can be retained
from the existing admission owner without copying its Battle01 route or broadening that fixture's
historical claim.

Minimum discriminating cases/guards:

1. Map66, F402 set/F502 clear: index 2 and area `(16,19,22,18)`; vary coordinates, including a
   point outside that area, to distinguish wildcard triggers from an invented bounds predicate.
2. F402 clear with all other Map66 battle unlocks clear: no selection. If another Map66 unlock is
   enabled, preserve the scan's later-row result instead of claiming “no battle on Map66.”
3. F402/F502 both set: F402 clear **and index 2 returned on the same call**. A model that rejects
   completed battles before returning must fail this case.
4. Clear/set F452: before/start wrapper skip versus dispatch, and start's set-before-empty-dispatch
   order. A changed slot target, inverted branch, flag offset, table stride, or call order must fail
   the use-site construction, not merely change an aggregate count.

The source-backed F402 producer remains the existing `Map3_261_ZoneEvent1` record; a future consumer
may link that identity without duplicating its parser. Inputs `(2,23)` versus `(2,24)`, F3 clear/set,
and full-script versus suffix entry are useful adversarial controls for any later route composition.
They are not additional new-fixture proposals. Closing the candidate would still not close the
unproved walking joins or natural chronology.

## Reproduction and completion boundary

No large acquisition was required: selected source text, tracked fixture/verifier inspection, and
three existing small static inventory outputs suffice. Source, ROM, H1 listing and generated JSON
remain private; no scripts, decoded text, layouts or roster tables are published here. No shared
registry, index, schema, fixture, parser or navigation file changes with this note.

In the owning checkout, load `local/private-inputs.ps1` in the launching process. `$upstream`
below denotes the explicitly selected, verified read-only pinned checkout; its machine path is not
part of the public recipe. `--upstream-path` is necessary when it is outside the worktree default.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
uv run sf2 rom verify
uv run sf2 h2 battle-global-data --upstream-path $upstream
uv run sf2 h2 battle-routing-data --upstream-path $upstream
uv run sf2 h2 battle-spriteset-data --upstream-path $upstream
```

**Confirmed reproduced:** ROM identity and all three H2 commands passed on this evidence date.
The generated battle-global row was inspected at `facts.battleMaps.rows[2]`; the routing output
retains `facts.routeTargetsParsed = false`, and the spriteset output retains
`facts.rowContentTracked = false`. Individual cutscene targets and behavior operands above are
direct pinned-source findings, not falsely attributed to those aggregate fixture fields.

Other listed owner commands are reproduction routes, not claims of fresh runs in this slice.
No emulator, Godot, H4 comparison, normal/full research suite or new test was run. Direct links,
source symbols, candidate scope, private boundary and `git diff --check` are checked before freeze;
the committed `uv run sf2 verify plan --base origin/main --head HEAD` result and actual CI belong in
the Issue/PR handoff. The deliverable stops at a clean pushed Draft PR for independent acceptance.
Unknowns above remain explicit; they do not start an automatic fixture or runtime queue.
