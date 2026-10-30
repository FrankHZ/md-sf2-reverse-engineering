# Opening Session Lifecycle and Feature Closure

## Agreement and decision

**High rigor:** initialization and durable recovery cross Content, the one Application session,
party resources and the host; an apparently successful load can silently lose progress. This owner
defines the proposed implementation contract for [#1418](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1418)
and design [#1419](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1419).
It is design, not implemented persistence or independent product acceptance.

The outcome is New Game → Map3 exploration → Battle01 → usable victory/defeat return → Save →
process exit → fresh process Continue. Required preparation, pickups and opening services share
that same party. Existing Battle02/Yeel consumers are shared-state regression dependencies, not
an extension of this endpoint. The [capability matrix](../capability-status.md) owns delivered
behavior; [UI gaps](../evidence/opening-ui-gaps.md) owns presentation acceptance.

Recommend **two manually selected local slots and savepoint restoration**, initially for the
supported Map3 phases and settled Battle01 return on Map57. Save is explicit from a field menu;
Continue chooses a slot. A save records progress now, but loading relocates to the declared safe
anchor and rebuilds ambient NPCs/doors/camera. Show that destination before saving. It neither
rolls the party back to arrival nor promises exact position, NPC phase or uninterrupted RNG results.
Castle/palace/tower interiors initially explain “Save after returning to a supported location.”
They remain playable. Adding a save location requires its own bounded reconstruction acceptance.

This is a remake choice, not a reconstruction of the Witch screen or original SRAM. The
[save-system contract](../../../docs/design/contracts/system/save-system.md) confirms two logical
slots and bounded in-process original helpers, but leaves physical restart/power-loss and normal
player save UX Unknown. No arbitrary story/battle snapshot, autosave, cloud save, SRAM import,
copy/delete UI, migration framework, name editor, promotion or Battle03 ending is promised here.
New Game never overwrites a slot; only an explicit confirmed Save does. Quit warns about unsaved
progress and offers cancel; it does not secretly autosave.

Current milestone: independently accept this design, then the first executable New Game slice
below. Main-gate owns Issue dispatch, exact production ownership and integration. Reassess High
rigor at the persistence review and final restart acceptance; ordinary inventory/UI slices can
use Standard once their state boundary is fixed. No unresolved aesthetic choice gates this design.

## Current feature map

The inspection baseline is accepted `f2a200da44cb597e50f50633559b5f21cb8f7f13`.
“Confirmed structure” below is a code/document claim, not new runtime or original-game evidence.
Implemented, standalone-only, missing integration, missing behavior and Unknown are distinct.

| Surface | Current state and owner | Closure disposition |
| --- | --- | --- |
| Entry/initialization | **Confirmed structure:** [GameRoot](../../game/src/GameRoot.cs) immediately starts an authored package or explicit private controlled start. [PrivateExplorationReader](../../src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs) requires separately supplied start and party. | **Missing behavior:** product New Game/default initialization, title and independent replacement-session lifetime. #1420. |
| Persistence/Continue | **Unsupported:** no save DTO, disk store, restore admission or player Continue. A controlled start is not a save. | **Missing behavior:** #1421; follow the boundary below. |
| Walk/Talk/party inspection | Implemented in the connected route: semantic movement/interaction and read-only roster/details. [PartyDetailsQuery](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs) projects carried state; absent loadout/accounting stays unknown. | Keep the same authority. A readable unknown label is acceptable for optional presentation metadata, not for gold/EXP needed to transact or save. |
| Equip/Give/Drop | [BattleEquipment](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs), [BattleGive](../../src/Sf2.Remake.Domain/Battles/Rules/BattleGive.cs), [BattleDiscard](../../src/Sf2.Remake.Domain/Battles/Rules/BattleDiscard.cs) and battle continuations exist, with standalone authored admission limits. | **Missing integration and field command behavior:** no field consumer. Reuse numerical/custody rules, not battle selection state or a fabricated battle. #1422. |
| Field Use/magic | Selected battle healing/item rules exist; the ordinary field menu does not consume them. | **Missing field policy/integration:** living targets, cost, no-effect/cancel, consumption and field EXP policy require the named contracts. Start with early recovery only. #1422. |
| Search/descriptions/chests/ground items | Original contracts exist; the current common field command surface has Move/Interact, not Search. Source map preparation imports entity/zone events and layout events; that is not an item pickup consumer. | **Missing behavior/content integration:** area descriptions, one-time receipt and full-pack handling; preserve receipt state across reload and save. #1423. |
| Receipts/Deals | [BattleItemCustody](../../src/Sf2.Remake.Domain/Battles/State/BattleItemCustody.cs) owns authored battle drop once-flags and Deals; carried through field party. Null means unobserved, not empty. | **Standalone-only:** private field pickups/services need explicit admitted data and their own receipt IDs. Do not use battle drop flags as chest IDs or silently replace null with empty. |
| Church/shop | Map3 entity event13 names ChurchMenu in accepted source. Generic native calls do not implement service transactions or a UI. | **Missing behavior/integration:** raise, supported cure and common Save entry first; buy/sell only at verified reachable opening shops. #1424. No shop is invented because inventory rules exist. |
| Battle01/outcomes | Connected before/load/start, combat, growth, victory Map57 return and ordinary defeat Map3 return are bounded implemented capabilities. | Preserve reward/join/flag order and once-only outcome commits. Continue must prove actual play through this existing boundary. |
| Later encounters | Selected Battle02/Yeel behavior exists independently. | Do not claim later campaign, private battle equipment or battle suspend from this design. Shared records must remain compatible or reject explicitly. |

### Reachable branch disposition

Use the [opening chapter](../../../docs/design/synthesis/chapters/grans-opening/opening-through-battle01.md)
and [Map3 optional interaction inventory](../../../docs/research/chapters/grans-opening/map3-optional-interactions.md)
as the source inventory, then check the current compiled program at each reached branch. Mainline
success is not all-branch closure. The bounded inspection is of these selected maps, not every game map.

| Location/phase | Established implementation versus remaining closure |
| --- | --- |
| Map3 house, Sarah, school stairs, Zone7, messenger and Zone8 | Connected programs and joins exist. Save anchors must distinguish pre-messenger and F603-set phases; no reload may replay JOIN or grant starting items. |
| Map3 optional NPCs/default events, descriptions and two item placements | Source table inventory is established; natural optionality/reach of every branch is **Unknown**. Lowered ordinary dialogue is reusable, ChurchMenu remains missing, descriptions/items lack the field consumer. #1423 records each actual offer/guard/result, including empty fallback, before declaring the opened area complete. |
| F600-clear direct castle approach / `cs_51454` | The old blanket `moveNextToPlayer` gap is stale: [compiler](../../../src/sf2tool/remake_exploration_programs.py) emits move/waits/facing; [ProgramRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) executes `MoveEntityNextToPlayer`. **Confirmed structure**, not proof this whole optional route now completes. Check its remaining native calls and actual return, without fabricating F600 or implementing the opcode twice. |
| Map19 castle and Map20 palace/Astral revisit | First/repeat visits, refusal/re-prompt and follower variants have bounded acceptance in [session programs](session-and-programs.md#castle-palace-astral-and-tower). Other NPC branches/alternate map setups are not promoted to accepted by that group. Interior save is deferred, not disabled traversal. |
| Map21 guard, tower and Battle01 | Accepted conditional guard and battle admission remain authoritative. A save attempt during guard motion, entry or combat rejects; it never completes the program or sets unlock flags. |
| Map57 victory / Map3 defeat | Usable returns are implemented. Map57 F506 layout-copy remains a guarded frontier; reject that unsupported restore phase. Map3 defeat retains unlock, halves gold and restores the leader under the existing outcome policy. Loading must not run either outcome again. |
| Open warps beyond this endpoint | Preparation retains warp rows and fails at unadmitted destinations/setups. #1423 must identify any reachable stop and provide a clearly bounded unavailable destination before committing transfer, or implement the necessary opening branch. Silently accepting Unsupported as a no-op is forbidden. Later campaign admission remains outside #1418. |

This is an accurate gap disposition, not a claim that all reachable NPCs were exercised. #1423's
bounded first pass must enumerate offered entity/default/zone/search/warp branches in Maps3/19/20/21
and returned Map57 for the admitted opening flags, with supported, unavailable or Unknown outcomes.
Only source uncertainty that prevents a required behavior warrants a focused evidence follow-up.

## New Game construction

Separate immutable content admission from start construction; reuse existing readers and validation
helpers rather than a second private parser. The current `PrivateExplorationReader` composition
through a battle controlled-start reader must be separated where it forces checkpoint input.
Specifically, `PrivateBattleScenarioReader.Read` passes `ControlledBattleStart` into
`PrivateBattleDefinitionReader.Read`, and `Assemble` builds ally definitions from that start's class,
level, maxima and loadout as well as its current resources. Replacing only the outer JSON would retain
this coupling. The minimum #1420 change is to extract reusable static/encounter admission, build
immutable ally capabilities from admitted data, and assemble fresh or restored progression separately.
The controlled-start adapter can still explicitly assemble its diagnostic overrides. Continue must
not make saved progress redefine trusted base content or reconstruct defaults from a checkpoint.
The same separation applies to controlled `NewBattleStartPolicy.BridgeBoundary` provenance: a genuine
new-game entry must not invent an H3 bridge label to satisfy a diagnostic start gate. Keep real source
difficulty/control/status admission with the selected rules and keep comparison provenance at its
existing diagnostic boundary.
Application receives a validated new-game start, selects the configured `SessionRules`, then uses
the existing exploration/program lifecycle. Neither runtime nor Content reads `remake/reference`,
test fixtures, prior slots, comparison checkpoints or a test's expected end state.

| Initial field | Required source/default and admission |
| --- | --- |
| Identity, name, class, starting level, loadout, spells and growth | Existing private static/growth content from the pinned ally/start/class/name/item tables, under [ally data](../../../docs/design/contracts/party/ally-definition-data.md) and [level-up](../../../docs/design/contracts/party/level-up.md). Store stable actor IDs; display default localized names from content. No player naming UI initially. Do not infer class/name from a sprite or reuse battle-ready stats as new-game stats. |
| Roster | Only member0 joined/active initially, source flags/membership mapping retained; Sarah/Chester join via ordinary programs. Construct the supported opening actors and future join baselines once, including Jaha if retained shared encounter content requires it. Unjoined actors are not displayed or deployed. Do not initialize all 30 merely to satisfy a full-game target. |
| HP/MP/base stats and learned spells | Apply the selected start-level initialization/growth and equipment refresh, with full current HP/MP. Use the existing algorithms where valid; any missing new-game initializer is real #1420 work. Initial growth consumes the declared main seed in source order; do not copy R1 numeric stats. |
| EXP, kills, defeats, status, gold | Explicit supported fresh defaults: EXP/kills/defeats/status = 0; gold = 0. Gold0 is also observed at the controlled R1 seam in [admitted start](../../../docs/research/chapters/grans-opening/map3-admitted-start.md); zero status is a remake fresh-start policy, not closure of R1's inherited POISON Unknown. No nullable transaction-critical accounting in a product start. |
| Items and custody | Decode complete runtime item words, including equipped/broken/auxiliary bits, from initial definitions; four projected item bytes are insufficient for a save. Explicit empty Deals, battle once-flags and field pickup ledger for a fresh game, with supported item identities admitted. These are new-game choices; null controlled evidence remains null. |
| Flags/options | Fresh story flags with only the initialization/membership bits required by admitted source setup; normal difficulty F78/F79 clear, no debug control flags. Initial mode/difficulty is fixed and recorded in the start contract, not inherited from the last session. |
| Location | Map3 `(56,3)`, Down, default setup, source player identity; run actual initial map setup/programs. This handoff is evidenced by the save-system and admitted-start owners. Do not seed messenger/castle/tower completion flags. |
| RNG/settings | Two explicit starting seeds supplied once by the host, then only selected rules advance them; tests can supply fixed seeds, including zero where the existing rules admit it. Default random selection adds no new seed-validity restriction. Preserve complete uint storage, not only a low word. Default text/input/reduced-flash settings use the existing settings owner. This choice is not original reset timing parity. |

The initial-resource data audit in #1420 is an acceptance boundary: names/classes, starting growth,
full item words and accounting must all be admitted before a playable New Game is claimed. Optional
portrait/class-label translations may remain visibly unavailable. Missing mechanical data rejects
startup with an actionable content diagnostic, never a plausible number or a practice-yard fallback.
The existing [R1 owner](../../../docs/research/chapters/grans-opening/map3-admitted-start.md) retains
its controlled menus/naming, omitted POISON and time/NPC-phase limits. New Game adds no original H3
claim. The product opens after the original title/Witch sequence; recreating that sequence is not required.

## State authority and save boundary

[GameSession](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) remains the sole publisher.
[ExplorationState and StoryState](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
and [BattleStartInput](../../src/Sf2.Remake.Domain/Battles/State/BattleStartInput.cs) already own the
carried field state. A save DTO is a detached value exported by Application from one admitted revision,
not a second mutable session and not `SessionSnapshot` serialization.

| State | Authority and persistence treatment |
| --- | --- |
| Ally resources/progression | Field `Party.Actors` / active battle actors; persist every supported retained ally's ID, current HP/MP, EXP, kills/defeats, status, initialization/absence, attained level/base growth/maxima and learned spells. Persist current class if mutable in admitted content; otherwise bind its immutable class identity. Never regenerate attained growth on load. |
| Equipment/inventory | Carried `SourceLoadout` and progression; persist all four complete item words and spellbook consistency. Recompute derived attack/movement from admitted rules; validate against class/equipment limits. No separate UI inventory copy. |
| Flags/membership/following | `Story.Flags` plus content party mapping; persist flags and any authoritative non-flag membership/order for authored packages. Rebuild `PartyLists`; validate rather than overwrite inconsistent `ActiveAllies`. Follower selection/order survives, physical allocation/offsets restart by map recipe. |
| Gold/Deals/receipts | Party gold and `ItemCustody` are authoritative. Persist current amounts, once-flags and Deals counts. Add field receipt keys to their owning state when #1423 implements acquisition; keep the key namespace separate. Pending transaction state is never saveable. |
| Map/position/geometry | Content owns base layout/population/setup and restoration recipe. Persist map, selected supported phase/recipe ID, safe landing anchor, and durable event flags/receipts. Do not persist whole layout, NPC script PCs or source RAM. Doors/roof copies and ambient positions reset as a map reload; persistent layout effects must be reproducible from saved flags/recipe. |
| RNG and world counters | Persist both live seeds and supported step counter/battle-request state (request must be false). Preserve EgressMap and any future gameplay counter actually read after restoration. Reject unknown required counters; do not confuse wall-clock time with simulation state. |
| Story execution | Cursor, callers, waits, event caller, warp, battle entry/outcome and skip must be absent at save. No tokens/continuations serialized. Persist text cursor or other scalar semantic registers if subsequent programs read them without assignment; clear only under a documented recipe invariant. |
| Logical display/audio | Close windows; reconstruct logical view, base/restored palette, player/follower sprite requests and required text settings from admitted map and saved semantic settings. Preserve current music cue and known previous-cue history where later music instructions consume it; start a fresh playback generation at step0. Reject unknown required history. No audio position or completion token is resumed. |
| Host/presentation | New session ID/revision/observation sequence, input release barrier, menu focus, camera smoothing, nodes/textures/PCM handles and caches. Never persist resources, services, private paths or completion receipts. Settings persistence remains separate from game progress. |

### Eligibility and restoration recipes

Application exposes a query returning allowed/typed reason and a capture operation that rechecks
the same session ID/revision. `CanWaitAtInput` alone is insufficient. Capture requires:

1. Active exploration, successful PlayerInput/FieldInput boundary, no stopped failure, active battle or selection.
2. No story cursor/callers/wait/event caller, pending ordinary/script warp, entering/outcome route,
   retained battle player binding or entry skip; no service/receipt/item transaction in flight.
3. Player exactly settled on a legal cell, no player/follower commanded motion or unfinished sprite
   readiness; closed/nonmoving text/portrait/choice, no scrolling logical view, fade/quake or pending
   roof/block-copy transaction. Background music must have no required completion outstanding.
4. The location/flags select one admitted restore recipe; its anchor is legal/unoccupied after rebuild,
   followers fit, and every durable map effect is accounted for. Unrecognized setup/phase or required
   unknown party/accounting state returns an explicit unavailable reason.

Ambient NPC walking need not stop forever to make saving possible. The recipe identifies which
default NPC scripts may be discarded/restarted; only those ambient scripts can be active. A story
installed move, hide or follower change that cannot be reconstructed is a rejection, even if player
input is available. Snapshot acquisition pauses command/tick dispatch for one revision; saving does
not drain waits, poll NPCs to “settle,” draw RNG, heal the party or run an event.

First recipe scope is deliberately finite. The map numbers, story predicates and reconstruction
facts below belong to validated Content or the explicit SF2 profile policy, never to hard-coded
opening checks in generic `GameSession`, save DTO validation or filesystem code. Generic capture
asks the selected profile for a recipe and validates its typed result using existing map invariants.

- **Map3 opening, F603 clear:** restore at `(56,3)`, Down with phase-correct population. This includes
  ordinary stable intermediate opening states and optional conversations, not just the first
  frame. Preserve their one-time flags and completed local effects; reconstruct any required
  phase-dependent hides/positions without replaying the dialogue or join transaction.
- **Map3 joined opening / defeat return:** after the messenger/Zone8 has finished (F603 set), normal
  supported setup and no active story; restore at `(32,13)`, Up, the existing admitted Map3 egress
  anchor. Preserve completed gate/palace/Astral/unlock flags, party and receipts. Rebuild the
  source-selected population, F603 entity142 retirement, follower allocation and flag layout effects;
  clear only the map-local entry bits that the normal reload owns. Do not heal or replay JOIN.
- **Map57 Battle01 victory return:** after full `battle-returned`, F501 set and F506 clear; retain a
  dedicated resume anchor from the actual outcome landing tuple, facing Down. It is not the player's
  later exact position and not `Story.ReturnAnchor`, which can describe the previous map. Add this
  small location-owned anchor at completed return; validate it against Map57 content on capture/load.
  Restore the void setup/player/followers and flags without `abcs_battle01`, stat reset or reward replay.

These are proposed product recipes, not existing save support. #1421 must implement and prove
ordinary legal variations within all three required boundaries: reordered optional conversations,
refusal/retry, changed party resources/equipment, living/dead members, earned progress and different
valid flags/landing tuples. Missing handling of such a state is an implementation defect or unfinished
acceptance, not permission to disable it as “uncovered.” A genuinely unsupported later Map3 setup
must still reject even when F603 remains set. Avoid enumerating walkthrough IDs: guards are
map/setup and semantic phase capabilities. No universal
snapshot engine or second scripting language is needed: extend the existing map builder with these
finite restoration inputs and reuse its population/layout helpers. #1423 must reassess recipes when
new persistent map mutations become reachable.

Restore builds a candidate off-session: admit content → validate saved state → select recipe →
rebuild world/projections → validate the first field boundary → mount required resources → publish
the new session. Do **not** call ordinary `GameSession.Start` with the saved flags and let its on-load
program replay. Refactor the shared pure construction where necessary; Continue must not trigger a
battle, service, heal, growth draw, one-time script or reward while reconstructing. A failed candidate
leaves the current session/slot usable; disposing candidate resources follows existing host ownership.

The seeds in the candidate equal the saved seeds. Restoration itself has no gameplay RNG draws;
ambient programs begin on the next deliberate service. Repeated loads with the same content/rules
and semantic input/poll sequence must agree in gameplay results. Comparison with uninterrupted play
is limited to persistent resources/progress: NPC phases, elapsed music and subsequent draw timing
can differ because this is a savepoint reload. This distinction must appear in player help and tests.

## Format, compatibility and local storage

Use a small explicit version1 JSON record: format version, slot, sequence number, save timestamp for
display only, content profile/package and gameplay revision, rule-composition identity/revision,
recipe revision, then the owned party/world values above. The host maps a selected package ID to an
already configured source; saved data cannot select a path, assembly, executable rule or trust root.
Content still enforces existing pinned private provenance. Save files containing private gameplay
state remain local and untracked, with no resource/export attachment to Issues or PRs.

Wrap the payload with an integrity digest over its exact stored UTF-8 bytes and verify it before
semantic admission. This checksum detects accidental in-range data damage that JSON/range checks
would miss; it is not change detection, authentication or an asset/content hash. Keep one documented
encoding and existing SHA-256 support, without a canonicalization library or signatures. Bound the
whole file before parsing; duplicate payload/checksum keys reject. A checksum-valid but semantically
invalid record still rejects at ordinary domain/content validation.

Compatibility is exact for format, gameplay-content revision, rule revision and recipe revision.
Use explicit revisions maintained with semantic changes, plus the existing private provenance identity;
do not add a hash of the entire asset export or Git commit as a universal compatibility gate. Same ID
alone is insufficient. Pure portrait/UI/audio-encoding changes can retain gameplay compatibility if
logical cues/resources remain valid. Changed stats/program meaning/receipt IDs require a new gameplay
revision. Unknown versions or identities reject with “This save needs the matching content/rules”; no
best-effort load, inferred migration or fallback to a new game. Future formats require a separately
reviewed migration only if a real retained save needs one.

Application owns DTO capture/validation/restore; Content owns bounded JSON admission/encoding and
binding to admitted definitions; the host owns a small filesystem adapter. Domain has no file/Godot
dependency. Add only the storage boundary needed to return typed results; avoid a plugin store,
save manifest database or generalized transaction framework. Parsing bounds: 1 MiB per slot,
bounded depth32, no duplicate/unknown fields, duplicate actor/receipt keys, invalid ranges, dangling
IDs or incompatible membership/loadouts. Limits are initial engineering bounds, not gameplay facts;
raise only with a measured legitimate record. A compact opening save should be tens of KiB, not the
roughly 128 MiB private world export. CPU/memory are O(saved actors + flags + receipts), not O(assets).

Use an explicitly configured absolute save root outside removable worktrees, with a per-checkout
namespace by default; no source-root/cwd inference, C-drive fallback, cache copying or shared mutable
test slots. Production installation chooses its persistent root once. Acceptance uses a separate
temporary H-local namespace, preserving previous evidence and user slots. Content profile namespaces
separate authored and private saves. Slot names are fixed by the adapter, never supplied paths.

For each slot retain `slotN.json` and one last valid `slotN.bak`; only one writer holds the slot lock.
Capture immutable bytes and freeze further gameplay commands while the short save operation runs.
Write a uniquely named sibling temporary file, flush file data, close, then re-read/validate it.
For an existing valid primary use the platform same-volume atomic replace with backup; for the first
save atomically rename the completed temporary file. Never delete/truncate the primary first and
never fall back to a cross-volume copy. If the configured filesystem cannot provide this operation,
report unsupported storage and preserve the old slot. Success is reported only after publication and
readback. Do not claim immunity to device failure or all power-loss cases from filesystem API success.

On startup ignore incomplete temporary files. Validate the primary independently; if corrupt, show
the error and offer the valid compatible backup with its timestamp (explicit recovery, no silent
rollback). An incompatible primary is not automatically replaced by an older backup. Missing content
is distinguishable from corruption, empty slot, permission failure and incompatible version. A read
never mutates or repairs files. After explicit backup recovery, preserve that valid backup until a new
validated primary is installed; never rotate the corrupt primary over it. Failed writes preserve the
last known valid record; an uncertain post-replace result reports an uncertain save and revalidates
before retry, rather than claiming that nothing was written. Unreferenced temporary cleanup is scoped
to that adapter's own attempt, not a directory sweep or worktree-cleanup authority.

## First executable slices and acceptance

The linked outcome Issues are Backlog boundaries, not concurrent writers or automatic tasks. Main-gate
fills exact dispatch ownership after this design is accepted. Paths below are likely direct owners;
new names are recommendations, not committed scaffolding. Shared `GameRoot`, `GameSession`, readers
and capability/navigation docs are serialized. No slice requires every later service first.

| Outcome | First coherent implementation and likely direct paths | Dependency and stopping acceptance |
| --- | --- | --- |
| [#1420 New Game](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1420) | `remake/game/src/GameRoot.cs`, new `game/src/Entry/SessionEntryView.cs`; Content `Scenarios/PrivateExplorationReader.cs`, `PrivateBattleScenarioReader.cs`, `PrivateBattleDefinitionReader.cs` and a focused `OpeningNewGameReader.cs`; Application `Runtime/GameSession.cs` plus `Runtime/Exploration/OpeningNewGame.cs` only for missing construction. Reuse `BattleStartInput`, initialization rules and accepted static/growth readers. | Accept #1419 first. Separate content definition admission from checkpoint construction at the concrete coupling above; normal title/New Game enters actual Map3 with complete supported state. Two starts are independent; valid varied seeds/data work; missing mechanical content rejects. Existing controlled CLI entries remain explicitly diagnostic. A small authored New Game witness is useful but does not complete private opening acceptance. |
| [#1421 Save/Continue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1421) | New Application `Runtime/Persistence/OpeningSave.cs` and `OpeningRestore.cs`, focused Content `Persistence/OpeningSaveJson.cs`, host `Persistence/LocalSaveStore.cs`; `GameSession.cs`, `ExplorationState.cs`, `MapTransfer.cs`, `BattleOutcome.cs`, entry/field-menu consumers. Content recipe data uses existing exploration definitions/readers. | #1420's admitted state and lifecycle first. First deliver capture/reconstruction of the three recipes with actual engine tests; then disk/entry integration and real restart, in one bounded PR if reviewable or two sequential PRs under this outcome. Capture alone is not completion. No need to wait for field inventory/services. |
| [#1422 Field preparation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1422) | First field Equip: existing `Domain/Battles/Rules/BattleEquipment.cs`, Application `Runtime/Exploration/ExplorationDispatcher.cs` and focused `FieldInventory.cs`, `Runtime/SessionContract.cs`, private item/class admission, `game/src/Exploration/PartyStatusView.cs` and a field action panel. | #1420/state contract; can proceed independently of disk implementation after shared ownership is scheduled. Preview/cancel preserve resources; confirm changes the carried party once; next battle consumes that loadout. Then Give/exchange, early Use/HEAL and necessary Drop as coherent slices. Full HP, dead/invalid target, insufficient MP, equipped/last-item and full-pack cases are explicit; no battle EXP policy borrowed silently for field use. |
| [#1423 Search/branches](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1423) | First inventory the bounded offered branches above; then one complete Map3 Search/receipt path. `src/sf2tool/remake_exploration_maps.py`/`remake_exploration_programs.py`, Content `Scenarios/ExplorationContentReader.cs`, Application exploration dispatch plus focused search reducer, existing inventory/custody rule and host field menu. | Shared #1422 item mutation semantics, not all its UI. Import source descriptions/placements, confirm once-only award, full inventory, repeat visit and reload/save survival. Other branches close one reached missing behavior at a time. Services remain #1424; source-lowering changes retain their affected research verification. |
| [#1424 Required services](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1424) | First Map3 ChurchMenu caller → raise/supported cure → close to same field; use a focused Application service transaction and Content cost/status admission, Domain existing party rules where valid, host service panel. Connect Save to #1421 only after the service caller has returned to an eligible field boundary. | #1420/accounting and required #1422 rule consumers; Save entry needs #1421. Verify actual church offer/return, decline, insufficient gold, no applicable target and successful payment/recovery. Then verify actual opening shop entry before buy/sell with capacity/payment/rare-Deals preservation. Promotion, mithril and Caravan storage deferred. |
| [#1425 Independent loop](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1425) | Existing engine/native observation entry and direct capability/UI/verification owners; no new comparator. | Accepted preceding required features or an explicit disposition that retains the opening promise. Main-gate independently checks the complete route below. Component passes and human aesthetic approval are separate. |

Each implementation adds small tests of actual engine state changes, not tests of JSON helpers,
planners or observation tools. Use direct malformed-file/storage controls for the adapter. Run the
affected `uv run sf2 verify engine` / `uv run sf2 verify adapter` and native state/input/error checks
under the existing locked environment only when implementing those changes. Source-lowering work
selects its own affected research checks. Preserve completed failures; do not restore blanket suites.

### Restart and failure acceptance

1. Use the normal New Game UI with the private opening content, no comparison-start argument or later
   state injection. Move/talk/join; save at eligible Map3. Record a compact semantic summary of party,
   inventory, flags, gold, receipt/Deals state, both seeds and selected resume anchor.
2. Terminate the process; start a genuinely new process using the same configured save root and
   choose Continue. Confirm a new session ID, released input, canonical landing and preserved
   persistent summary. Move/interact, traverse ordinary castle/tower programs, enter and finish
   Battle01; do not replace the journey with teleport or a terminal-state assignment.
3. Save the settled Map57 returned party, exit and load again. Verify attained growth/EXP/items,
   gold, membership/following, F401/F501 and receipt state, then actual movement/interaction or a
   legal return-map transition. Confirm no repeated JOIN, reward, healing or outcome script.
4. Separately exercise actual ordinary defeat and Map3 return/save/load/retry, including depleted
   resources and dead nonleader allies where supported. If a required retreat path is not admitted
   for Battle01, report that boundary; selected Battle02 retreat is not proof of it.
5. Try Save during dialogue/choice, movement, an ambient-versus-story NPC move, sprite/fade wait,
   map transfer, service transaction, battle and after-program. Reject with no mutation/write and
   recover normal control. Ambient reset is tested independently of forbidden pending story work.
6. Directly exercise empty, truncated, malformed, duplicate-key, out-of-range, unknown version,
   mismatched rule/content and unavailable-resource records; invalid input publishes no partial
   session. Locked/read-only destination, interrupted temporary write and replace failure protect
   a known valid slot. Corrupt primary plus valid backup, invalid backup and first-save interruption
   have distinct observable results. Never fault-inject into a user's retained slot.
7. Reload twice with identical semantic commands/polls to compare deterministic continuation; vary
   legal party/flag/content inputs rather than testing only the fixed walkthrough. Include one
   affected existing Battle02 continuity assertion when shared carried-state code changes.

Use existing bounded observations and semantic summaries, not screenshots, per-frame captures or
full content dumps. A save/recovery control needs at most primary+backup+one temporary record (under
3 MiB per slot); clean up only explicitly owned disposable controls after retention obligations.
Replan if a valid save exceeds 1 MiB, reconstruction needs an arbitrary story interpreter, or required
opening branches exceed the finite recipes: fix scope/ownership before building a generic framework.

## Design validation and remaining limits

This documentation slice uses bounded static inspection, relative-link/anchor and ownership checks,
`git diff --check`, a clean-head `uv run sf2 verify plan --base origin/main --head HEAD`, and actual CI
reporting. It launches no SDK, Godot, emulator, extraction or normal/full suite. Exact Git objects,
command results and CI belong to the Issue/PR handoff, not a milestone log here.

**Unknown:** full optional-route runtime reach, original R1 POISON/bootstrap chronology, original
durable SRAM behavior and exact presentation timing remain with their original owners. **Inferred:**
savepoint normalization is simpler and safer for this opening than arbitrary interpreter snapshots;
the implementation must demonstrate the finite recipes, not rely on that inference as a pass.
Design acceptance does not close #1418, establish all opened branches, or make persistence runnable.
