# Caravan formation and depot services

## Bounded agreement

This is the staged implementation agreement derived from [#1868](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1868),
under [campaign risk 4](remaining-campaign-risk-baseline.md#risks-in-blocking-order) and
[#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). Rigor remains **High**:
formation, inventory, story return and historical saves share state across chapters. The immediate
outcome is usable ordinary Caravan service at the already admitted Desktop/Floor World NPCs and a
way to deploy a newly recruited Creed companion when twelve members are already active. This is
a staged service: ordinary entry and Join/Purge are implemented below; Depot and Item remain unsupported.

The design was inspected at `726da800a843a9af0bc951d6b83fd5d5ae0ee8df`. Formation implementation
consumes accepted Battle23 main `84a08691384a88f69fa4e85f9f52e8655f6e03d3`. Subsequent service
slices serialize the shared owners below and update the [campaign feature trace](../capability-status.md#campaign-feature-trace).
A private remake package is not evidence for a new original-game claim.

Use the existing party flags, carried resources, immutable custody, service wait, content compiler,
save formats and field/shop design system. No service platform, second roster, alternate inventory,
capture system, new savepoint or private acquisition is needed. The original design investigation
used tracked owners and pinned source only. Implementation
acceptance additionally uses the engine and ordinary native observations below, with private inputs
kept local. No new original emulator evidence is claimed.

## Current formation implementation

**Confirmed:** [#1870](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1870) implements
ordinary Map22 entity128 and Map26 entity134 entry, Join/Purge, nested cancellation and callback return.
Application owns `CaravanWait` and token-checked commands; `MapPartyMembership` changes active flags
without deleting joined flags or actor resources. The same publication rebuilds `PartyLists` and
`ActiveAllies`. Godot projects the existing scrollable field choices/pack panel and retains release,
focus and pointer barriers. Depot and Item remain visible with a readable unsupported result.
Neither service entry requires the vehicle flags nor service exit reloads the map.

Private compilation lowers the actual `j_CaravanMenu` call, retaining call/tail return. Content binds
source text2 at entry and text10 at exit through the existing text/portrait player. The native
portrait is Rohde11 or Astral31 according to F70, independent of the NPC sprite. The bilingual catalog
selects these two source messages; formation prompts use the ordinary UI translation keys. Two engine behavior tests cover F70-clear/Rohde and F70-set/Astral through entry,
service and final return. Natural retained observations exercise F70-clear/Rohde. **Unknown:** native F70-set/Astral service traversal.

The nine focused `CaravanServiceTests` exercise different reserve/relief members, dead confirmation,
under-capacity join, full12 exchange/cancel/Bowie refusal, Purge/rejoin, empty reserve and unsupported
choices, stale/repeated/foreign commands and a failing callback after a committed exchange. Selection
failures leave the snapshot unchanged; a later callback failure retains the accepted formation.
They use actual engine state, not a reference-case predicate or a separate service model.

The registered private `handoffs/issue1870` bundle contains `Play-en.cmd` / `Play-zh-CN.cmd`.
Choose **Continue → Slot1**. The default is the ordinary Map26 service-return save at (11,36), Up,
with Peter7 in reserve and eleven active members. Confirm speaks to the NPC. Join can restore Peter;
Purge can select another non-leader. Save is available after Leave and the text10 return. The
`floor-*` and `desktop-*` seeds retain the independent original twelve-member histories unchanged.
No existing player namespace is overwritten; each review needs a fresh Sample.

From that registered bundle in PowerShell7:

```powershell
./Play.ps1 -Language en -Sample floor-review -Mode formation
./Play.ps1 -Language zh-CN -Sample floor-review -Mode formation
./Play.ps1 -Language en -Sample desktop-review -Mode formation -Seed ./slots/desktop-en.json
./Play.ps1 -Language zh-CN -Sample desktop-review -Mode formation -Seed ./slots/desktop-zh-CN.json
./Play.ps1 -Language en -Sample field-review -Mode cold -Seed ./slots/service-en.json
./Play.ps1 -Language zh-CN -Sample battle-review -Mode exercise -Seed ./slots/battle-zh-CN.json
```

`formation` uses ordinary keyboard input to inspect empty Join, Depot/Item refusal, Bowie refusal,
Purge Sarah1 and dead Kazin4, decline/accept dead rejoin, rejoin Sarah, cancel nested selection,
and Purge Peter7. Held Confirm is observed without a second command leaking into the result. Both
Map22 and Map26 run these operations in both languages. Complete party resources stay equal across
the service; only active flag39 is absent at final return. Names, story/transport flags and inventory
are retained. Map26 then walks through the real cave/confrontation into Battle23 with the selected
eleven-member formation; Peter remains retained with his HP/items and no battle position. Cold battle
Continue executes ordinary movement confirmation and Stay to the next actor without an adapter fault.

All complete decoded field and battle save data match before and after cold Continue → Save in both
languages. The old Battle19 record also round-trips unchanged. Actual field movement after cold load
is observed. These are remake persistence results, not original SRAM/runtime claims. The inherited
natural saves have only twelve joined members: native full-thirteen-member replacement and Creed's
new recruit → service → next-battle journey await the recruitment chapter. Full-capacity exchange is
engine-tested, not mislabelled as that natural journey.

The resource bound is one incrementally recompiled existing world and its required contained
encounter/audio dependencies, one reused runtime, and bounded receipts; initial added-output budget
350 MiB. No second full acquisition, capture system, service registry, new save schema or migration
is introduced. The old formats already persist flags, active members and all retained resources.
The registered `Rebuild.py --repository <checkout> --output local/<fresh-directory>` recipe uses
the maintained compiler and localization selector against the same pinned inputs after the owning
Debug adapter build; it does not re-extract world media. Run `uv run sf2 verify engine`, `uv run sf2 verify adapter`, direct native observations and the clean
committed `uv run sf2 verify plan --scope engine --base origin/main --head HEAD`. The shared Python
changes lower remake services/select display text only; original extraction/evidence is unchanged.
The bundle preserves early admission/localization/source-copy failures and native exit codes separately
from observer results. Existing #1716 ObjectDB shutdown warnings remain; successful observations do
not establish leak-free exit. The final handoff owns exact commands, candidate identity and CI.

## Evidence and the first consumers

**Confirmed:** original-source claims below are static observations of
[SF2DISASM master c834c652](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm),
full commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. Paths in this section are relative to
that commit's `disasm/`. The accepted evidence binds USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; no ROM read or new
runtime observation was necessary for this design. **Unknown:** natural original menu traversal,
transaction completion, presentation timing and save/reload results beyond the named existing seams.

| Consumer / source owner | Bounded finding and consequence |
| --- | --- |
| `data/maps/entries/map22/mapsetups/s2_entityevents.asm:Map22_EntityEvent2` and `data/maps/entries/map26/mapsetups/s2_entityevents.asm:Map26_EntityEvent5` | Entity128 on Desktop and entity134 on Floor World call `j_CaravanMenu` then return. The callback bodies impose no vehicle/F64/F65 test. Preserve the selected map interaction wrapper and its return; do not require driving or invent a transport unlock. The [Floor World owner](cotton-balloon-floor-world.md#source-and-implementation-boundary) records the reached unsupported consumer. |
| `code/common/menus/caravan/caravanactions_1.asm:CaravanMenu` (`0x21FD2..0x228A2`) and `caravanactions_2.asm` (`0x228A8..0x229CA`) | Top actions are Join / Depot / Item / Purge. Depot is Look / Deposit / Derive / Drop; Item is Use / Give / Equip / Drop. Top cancel exits; nested cancel returns to its parent; completed actions repeat their local selection. [Common menus](../../../docs/research/presentation/common-menus.md) and [service contract](../../../docs/design/contracts/party/service-interactions.md#caravan-source-boundary) own the complete static boundary. |
| `code/common/stats/battleparty.asm:JoinForce`, `JoinBattleParty`, `LeaveBattleParty`, `UpdateForce` | Joined and active flags are distinct. JoinForce sets joined, rebuilds lists, then sets active only below capacity12; it does not rebuild again after that final write. A thirteenth recruit remains joined/reserve. UpdateForce orders lists by member index; it is not a user-reorderable formation grid. LeaveBattleParty clears active and writes combatant X=-1, without erasing membership or items. |
| [Creed recruitment](../../../docs/research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md) | Eric14, Karna15, Randolf16 and Tyrin17 have source-owned offers. Accepted Yes joins the selected member before F787 and following; refusal and F22 alternatives remain chapter content. Joined, deployed and an entity's current follow relation are different facts. Do not turn full formation into rejected recruitment, choose an automatic relief, or infer deployment from following. |
| `code/common/stats/caravaninventory.asm:AddItemToCaravan`, `RemoveItemFromCaravan` (`0x9A3C..0x9A9A`) and `sf2enums.asm` | Capacity64, index mask `0x7F`, appended identity bytes, full-add no-op, removal by selected index with ordered compaction and `ITEM_NOTHING` tail. Member packs have four slots. Duplicate identities are separate entries. |

There is no contradiction between the low-level
[Caravan/deals contract](../../../docs/design/contracts/party/caravan-and-deals-state.md) leaving capacity
and mask **Unknown** within its six-fact fixture and the broader service owner confirming capacity64.
The service fixture and direct source supply the extra facts here; this document does not silently
enlarge that low-level contract. Likewise raw source item broken-bit15 is not the remake's logical
broken marker256. Normalize using the existing engine representation at its boundary; depot storage
retains identity only, so neither equipped nor broken state survives deposit.

**Confirmed:** the accepted common-menu H3 lifecycle evidence covers a controlled entry/cancel and
saved-frame return, not an ordinary complete Caravan transaction. Its status must not be promoted to
depot persistence proof. **Inferred:** the existing service architecture is sufficient for these
consumers with the bounded additions below. Original hardware persistence remains **Unknown**;
remake save continuity is an explicit acceptance requirement, not an original runtime claim.

## Player behavior and state transitions

### Join and Purge

Join lists reserve members, including dead members. No reserve produces a readable empty result.
A dead selection asks for confirmation; decline restarts Join without mutation. Below12 active,
acceptance sets that member active. At capacity, choose a current active relief; cancelling that
choice restarts Join without changing either member. Bowie/member0 cannot be relieved. A valid
exchange clears the relief's active flag then sets the selected reserve's active flag. Purge lists
active members, refuses Bowie, and moves a selected other member to reserve. It never invokes
LeaveForce. These are **Confirmed** source branches in `caravanMenu_Join` and `caravanMenu_Purge`.

Rebuild the displayed membership after each commit, preserve all HP/MP/status/growth/items/names,
and retain joined flags. Follow relations, Fairy/Oddler story flags and raft berth are untouched.
Do not remove an ally resource record or set its general `Absent` marker merely because it is in
reserve: the existing outcome owner already explains that raw combatant X is not field-position
authority. The next battle derives placement from active flags and current resources. Validate the
selected member against live membership and source capacity, not a fixed Creed roster or save name.

### Depot

All joined members, including reserves, can supply or receive items. Preserve numbered member slots
and depot indices; a byte identity alone cannot identify a duplicate selection.

| Action | Source behavior and proposed command boundary |
| --- | --- |
| Look | Read only. Empty storage returns an empty result. Show the selected identity and admitted item description/type/equip eligibility and sale value; source price is `floor(price*3/4)`, not an actual sale. The source has a special Chirrup Sandals description and ring eligibility branches; do not fabricate those from a weapon-only query. Missing wider appraisal remains explicitly unsupported. |
| Deposit | Check count below64 before selection/commit. Refuse an equipped cursed item. Source calls add-to-caravan before DropItemBySlot. Prepare the new compacted member pack and appended normalized identity together; publish once. At64 return no room, retaining the donor exactly. UnSellable prevents Drop, not Deposit. |
| Derive (retrieve) | Select stored index, then a joined recipient. A pack with space receives the item through AddItem, then storage removes/compacts it. With four held items, select a return slot; refuse an equipped cursed return item. Source order is member removal, storage removal, member add, storage add. The outgoing member identity is appended at the depot tail, not inserted at the vacated depot index. A64-entry depot can exchange because removal frees a place. |
| Drop | Unsellable items refuse. Require confirmation before removal. On acceptance compact storage and, for rare items, increment the existing Deals count with saturation15. Refusal/cancel leaves both stores intact. Do not expose purchasing Deals as an implication of this routing. |

Pure preparation followed by one session commit is the modern transaction boundary. Keep source
operation order within that preparation; do not claim the original was atomic. Reject stale token,
member, slot or expected word/index before publishing. A later dialogue or program failure retains
an already committed transaction; Back never rolls it back. No generic transaction log is needed.

### Item and cancellation

Reuse the field pack/equipment/result presentation, but use the Caravan caller's rules. Directly
forwarding existing Field commands is insufficient:

- **Confirmed:** `FieldInventory.Member` requires ordinary field input, while a service owns a wait.
  Refactor the small member/loadout mutation helper for a validated service context; keep the public
  ordinary-field gate. Do not temporarily clear the wait to make commands pass.
- **Confirmed:** `caravanItemSubmenu_Give` gives to self by removing the selected item and appending
  it with AddItem, even with a full pack. Current `FieldGive` also supports a distinct self-exchange
  flow. Reuse Add/Remove and expected-word checks, not that caller policy unchanged. For different
  members, retain the full-pack exchange and equipped-curse guards on both outgoing items.
- **Confirmed:** Caravan Use checks the field-usable table, calls UseItemOnField, then removes the
  selected donor slot after return. The [field effects owner](../../../docs/research/world/field-item-effects.md)
  lists IDs3,5,9–15; herbs/seeds are not field healing. Current `FieldRecovery` supports Antidote,
  ordinary no-effect handling and map-event/EGRESS policies; it does not implement the whole table.
  Reuse supported effects only with the Caravan policy. Never trigger the Cotton Balloon map-event
  route from the depot/Caravan Use submenu, or consume an unsupported stat item as a success.
- Equip reuses admitted equipment rules and selected-slot handoff; Drop reuses compacting removal,
  unsellable guard and rare-to-Deals routing. Equipped cursed gear, rings and broken gear remain
  outside existing ordinary equipment support. Read item definitions to distinguish a known refusal
  from an unavailable effect; reject before mutation rather than granting a wider equipment lifecycle.

Keep the four root actions and explicit unsupported choices visible during staged delivery. Back
from nested Depot/Item returns to root; source item/member cancellation returns to its owning action
loop or submenu as described above. Specifically, cancelling Join's reserve selection or Purge's
member selection returns to root; cancelling Join's relief returns to reserve selection. Cancelling
Depot's first selection returns to Depot; cancelling Derive's recipient restarts stored-item
selection. Cancelling Item Use/Give's target restarts its donor selection; cancelling the initial
member selection returns to Item. Top Leave completes the original callback once. It restores
ordinary control through ProgramRunner, without map reload, teleport or re-running entry rewards.
Use existing English/Chinese layouts, focus retention, pointer/keyboard/pad mapping and release
barriers from [field and shop](../godot/field-and-shop.md). Application owns phases, tokens, results
and commands; Godot owns focus and rendering only. Save is available after the source return on
existing settled ground; no mid-service save/resume protocol is proposed.

## Existing owners and minimum changes

Paths in this table are relative to `remake/` unless prefixed with `src/sf2tool/` (repository root).
Domain, Application and Content shorthand means `src/Sf2.Remake.Domain/`,
`src/Sf2.Remake.Application/` and `src/Sf2.Remake.Content/`; sibling bare filenames use the preceding
directory. They are implementation dependencies, not paths owned by this design task.

| Boundary | Existing owner and smallest change |
| --- | --- |
| Membership | `src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs` owns joined/active/reserve derivation. Add bounded deployment/relief rules there or an adjacent small Domain owner; retain JoinForce's source semantics. `src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs` already joins resources and reconciles `PartyLists`/`ActiveAllies` at program return. Service publication must keep views coherent during the wait too. `PartyDetailsQuery.cs` queries joined carried resources; `BattleEntry.cs` rebuilds active formation. `OutcomePartyOperations.cs` is victory-only and must not be invoked as an ordinary-service API. |
| Service entry and return | `src/sf2tool/remake_exploration_programs.py` and `remake_exploration_services.py` currently lower Shop/Church, leaving the Caravan native call unsupported. Add its typed lowering, preserving call versus tail return. `src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs`, Content `Scenarios/ExplorationContentReader.cs`, Application `Runtime/Exploration/ProgramRunner.cs` and `Runtime/GameSession.cs` admit/dispatch a dedicated OpenCaravan/wait/commands. Follow `ShopService.cs` and `FieldService.cs`; do not introduce a pluggable service registry. Extend `ExplorationDispatcher.cs` control exclusion and the actual host wait routes. |
| Content and presentation | Content `Scenarios/PrivateServicePrograms.cs` owns source service greeting wrappers; Caravan greeting/portrait must follow its own source, including F70's Astral/Rohde selection. Do not reuse Church text110 or Shop text158. Add a small `game/src/Exploration/CaravanServiceView.cs`, reusing existing choice/pack/party widgets and localization keys. Wire it through `ExplorationSessionView.cs`, `.Clock.cs`, `.Mouse.cs` and existing menu exclusion. Neither transport `CaravanRuntime.cs` nor `CaravanDefinition.cs` owns services. |
| Items | Domain `Battles/Rules/BattleInventory.cs` already supplies compacting Remove and Add that clears equipped while retaining the engine broken marker. Application `Runtime/Exploration/FieldInventory.cs` and `FieldRecovery.cs` own carried mutation/effects. Content `Scenarios/PrivateFieldPreparation.cs` admits a bounded subset. Add only the service-context checks and actual reached operations; reuse private item metadata for rare/unsellable/cursed classification. Unsupported effects must not prevent carrying an otherwise admitted bare identity. |
| Depot lifetime | Domain `Battles/State/BattleItemCustody.cs` currently contains only OnceFlags and Deals. Extend that same carried value with a distinct immutable ordered depot list, bounded at64; do not encode depot entries in Deals. It already crosses BattleStartInput → battle → outcome → field. Update every reconstruction (`Mark`, `AddDeal`, continuation capture/restore and wire decode) to retain depot, including a rare drop while depot is nonempty. A nullable custody still means unavailable knowledge, not empty inventory. |
| Persistence | Application `Runtime/Persistence/OpeningSaveData.cs`, `OpeningSave.cs`, `OpeningRestore.cs`; Domain `Battles/State/BattleContinuationData.cs`, `BattleContinuationRestore.cs`; Content `Persistence/BattleSaveWire.cs`, `OpeningSaveJson.cs`; Application `Runtime/Persistence/BattleSaveStory.cs`. Carry/validate depot at both field and battle boundaries. Wire admission currently requires properties except an explicit compatibility list; merely adding a C# default is not enough. |

The concrete new state prevents item loss at a battle/save boundary. A UI-owned array would be lost
there; Deals cannot express duplicate ordered slots or transfer semantics. Extending existing custody
is smaller than adding a second campaign store and propagating it through every battle/field copy.
No new cache, manifest, checksum or generic migration framework is justified.

For old accepted field-v1 and battle-v2 saves, propose an optional depot property whose absence means
empty **only for the previously supported remake formats**, where no depot operation existed.
New writes include the list. Keep explicit null/malformed/over64/out-of-range or unsupported identities
as admission failures; allow duplicates and preserve order. Keep unknown live custody distinct from
that version-specific compatibility default. Reject new incompatible saves in older binaries rather
than silently dropping depot. Reuse current envelope/slot recovery and compatibility owners; this
does not promise backward loading by an older binary. Verify known old slots before choosing any
additional revision change. There is no source SRAM import or original-save migration in scope.

Membership remains flag-authoritative. Persist the matching derived ActiveAllies plus every retained
ally's resources, including reserves, and initialize only newly admitted records through
`Gameplay/Sf2/Sf2HistoricalRoster.cs`. Never replace old party progress with recipe defaults.
The two RNG words, names, gold, OnceFlags/Deals, story flags, position/layer, return town and raft
berth must survive unchanged except for the expressly selected operation.

## Recommended implementation tickets

These are concrete proposed slices, not dispatched work or a request to create parallel writers.
They consume accepted main sequentially. Each claims its exact edits at dispatch; listed shared files
must be transferred after #1867/the current chapter writer stops. Existing files over the source-size
threshold require extracting the affected responsibility without net growth, per repository guidance.

1. **Ordinary Caravan entry, Join/Purge and Creed full-force handoff.** Own the membership, typed
   service-entry/return and host routes named above; new
   `src/Sf2.Remake.Application/Runtime/Exploration/CaravanService.cs`,
   `game/src/Exploration/CaravanServiceView.cs` and
   `tests/Sf2.Remake.Engine.Tests/CaravanServiceTests.cs` (all relative to `remake/`). Own affected
   `game/src/Localization/GameText.cs` and `GameText.Keys.cs`, this design and the capability row.
   Reuse existing party persistence, without adding depot yet. Depend on accepted current campaign
   content; full ordinary Creed acceptance additionally depends on its restoration/consultation/
   recruitment chapter. Before that chapter, admit only controlled engine membership evidence plus
   actual Map22/26 entry/return; leave the Creed ordinary handoff explicitly pending. Acceptance:
   dead-reserve Yes/No, below12 join, full12 relief/cancel/Bowie refusal, Purge, retained resources,
   old-save Continue and the changed party's next real battle. Do not claim the entire service complete.
2. **Depot transfers with field/battle persistence.** Depend on slice1. Own the custody and persistence
   files in the table, `src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs` whose custody equality boundary
   must include the new component, Content `Scenarios/EnemyItemDropReader.cs` for initial custody admission,
   a small new `src/Sf2.Remake.Domain/Battles/Rules/CaravanInventory.cs`, service/view/localization
   extensions, and `tests/Sf2.Remake.Engine.Tests/CaravanInventoryTests.cs`. Include Look's supported
   appraisal, Deposit/Derive/Drop, reserve packs and all failure returns; preserve explicit wider Look
   limits. Acceptance: duplicate-index selection, count63→64, no-room65th attempt, full member exchange
   at depot64, cancel, unsellable/curse refusal, rare Deals saturation and repeated save/battle/return
   with a nonempty depot. New Game starts empty; known old saves upgrade without rewriting history.
3. **Caravan Item connection and caller differences.** Depend on slices1–2. Own existing
   `FieldInventory.cs`, `FieldRecovery.cs`, `PrivateFieldPreparation.cs` at their table paths, the
   service/view/localization extensions and `tests/Sf2.Remake.Engine.Tests/CaravanItemTests.cs`.
   Add service-context admission and the source self-Give rule, supported Use/Equip/Drop, then prove
   ordinary field preparation still follows its own policy. Acceptance includes full/self/different
   member Give, exact slot compaction, supported Antidote consumption, refusal of nonusable items,
   unsupported effects with no consumption, return to Caravan and final callback return. Wider
   field stat items, cursed/ring/broken equipment lifecycles and exhaustive appraisal remain separate
   first-consumer extensions under #1369, not prerequisites disguised as a full inventory redesign.

For each implementation slice, update this document's proposal status and affected chapter/capability
owner together with demonstrated behavior. Changing `remake/content/private/pre-voyage-opening.json`
or package selection is not automatically required: existing Map22/26 callbacks already exist.
If Creed recruit definitions/callers are absent, the chapter owner supplies them; the service must
report missing bindings rather than manufacture allies. Shared source/compiler edits remain engine
wiring only when their actual semantic boundary excludes changes to research evidence.

## Acceptance and reproduction boundary

For design review, use the accepted base and pinned source above. Read the named routines with
`git -C local/upstream/SF2DISASM show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`;
verify that checkout's HEAD/status before filesystem reads. Compare tracked service evidence in
`tests/fixtures/h2/common-menus-static-v1.json`, `common-stats-static-v1.json` and
`field-item-effects-static-v1.json` with the linked owners. Their existing reproduction commands are
`uv run sf2 h2 common-menus`, `uv run sf2 h2 common-stats` and `uv run sf2 h2 field-item-effects`;
these are provenance routes, not newly claimed gate runs in this design. Run `git diff --check`,
resolve this document's relative links, inspect the one-file staged diff/private boundary, and obtain
`uv run sf2 verify plan --base origin/main --head HEAD` on the clean committed candidate. Do not
launch engine/H3 suites for this documentation change.

Implementation acceptance uses small actual Domain/Application/Content behavior tests via
`uv run sf2 verify engine`, `uv run sf2 verify adapter`, and the affected ordinary host observations.
Follow the [environment and scope owner](../development-and-verification.md#scope); no tests of
probes/planners or new comparison platform. Exact source/H2 comparison is needed only if its claim
changes. Use real state/input/projection and error observations, never screenshots.

The retained **future** ordinary starting inputs are the registered `issue1864` handoff's
`slots/floor-en.json` and `slots/floor-zh-CN.json`, and the earlier Desktop victory slots identified
by [its reproduction route](cotton-balloon-floor-world.md#confirmed-remake-observations).
They were not opened or modified here. A later implementation must copy them to disposable review
namespaces, use its compatible candidate bundle and matching localization, and visit the actual
NPC; do not launch the old package and mistake its unsupported native call for candidate behavior.
Creed's post-recruit ordinary slot must come from the chapter implementation when accepted; none is
invented here. At each relevant boundary verify:

- top/nested cancellation, held Confirm, stale command and failed transaction retain state/control;
- different valid party/item choices work, including a dead reserve and full packs, not just one
  named roster or fixture sequence;
- service exit permits movement and Save; cold Continue preserves the complete decoded progress;
- entering a supported next battle uses the chosen active members and their actual equipment;
  victory, defeat/EGRESS, battle Save/Continue and a later depot visit retain storage and reserves;
- bilingual names, prompts, failure reasons and actual input reach the same command outcomes.

**Unknown:** ordinary original transaction timing/persistence and the future remake
Creed-to-next-service natural route. The latter requires the chapter's source-owned
transport/follower/return path, not a new global Caravan hotkey. Wider item effects and equipment
lifecycles are known unsupported remake capabilities, not missing original static evidence.
Later recruitment variants, transport UI access, debug BattleTest entry, blacksmith, shop Deals/Repair,
church promotion and original SRAM compatibility are outside this bounded service delivery.
