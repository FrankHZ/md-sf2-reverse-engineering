# Caravan formation and depot services

## Bounded agreement

This is the staged implementation agreement derived from [#1868](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1868),
under [campaign risk 4](remaining-campaign-risk-baseline.md#risks-in-blocking-order) and
[#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). Rigor remains **High**:
formation, inventory, story return and historical saves share state across chapters. The immediate
outcome is usable ordinary Caravan service at the already admitted Desktop/Floor World NPCs and a
way to deploy a newly recruited Creed companion when twelve members are already active. This is
a bounded service: ordinary entry, Join/Purge, Depot and the admitted Item operations are implemented below.

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
focus and pointer barriers. Depot and Item open their respective submenus; unsupported item effects remain readable refusals.
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
is observed. These are remake persistence results, not original SRAM/runtime claims. The [Creed chapter](creed-recruitment.md) supplies the later ordinary thirteenth-member reserve,
actual vehicle entry and full12 relief. English Eric replaces Sarah1; Chinese Karna replaces Peter7,
with all retained resources intact. Eric enters, acts and wins ordinary Battle19 on one branch.
The older NPC service witnesses retain their own unchanged twelve-member histories.

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

## Current overworld vehicle entry

The [Creed recruitment candidate](creed-recruitment.md) binds the source field-action service when
walking settled over physical1's visible Caravan sprite at the same destination. It reuses the
formation/depot/Item service and source2/10 Rohde/Astral caller. Shrink/park, HEADQUARTERS39,
menu return, grow, following, camera and previous music complete in ordinary bilingual observations.
No chapter/recruit flag controls admission. Five engine tests vary sprite, visibility, target and
position and consume the return with party/transport preserved. The source owner is
`explorationvints.asm:loc_25B40..return_25BF2` at the pinned revision. This extends the existing NPC
callers; it does not add appraisal/item effects, human acceptance or lifecycle claims.

## Current depot implementation

**Confirmed:** [#1871](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1871) adds
Look / Deposit / Retrieve (source Derive) / Drop to the same Map22/26 service. The single
`BattleItemCustody.Depot` owns immutable ordered identities, including duplicates, at capacity64.
`CaravanInventory` prepares both stores before `CaravanDepotService` publishes a transaction.
Member selection includes every initialized joined member, including reserves. Slot/index plus
expected word/identity and the existing session/wait tokens guard stale input. The numbered list
uses the existing scrollable choice/pack widgets and item icons, with English and Chinese prompts.

Deposit compacts the donor pack and appends identity only, clearing equipped/broken bits. Retrieve
removes the selected depot index; a full-pack exchange compacts the pack, adds the requested item,
and appends the returned identity to the depot tail. A full64 depot can exchange. Equipped cursed
outgoing items refuse. Unsellable prevents Drop, not Deposit. Drop requires confirmation and rare
items update existing Deals with saturation15. Cancellation and rejected commands preserve both
stores; later caller failure retains a committed transfer. Unknown custody remains unknown and
refuses operations while still allowing the player to leave the submenu.

Look reuses admitted names, item type, `floor(price*3/4)` valuation and class eligibility for
admitted weapons and stat rings. The Wooden Sword was appraised in both languages. The observed
Power Ring shows 2,250 gold and the eligible joined members. An absent price is shown as unknown.
Chirrup Sandals' special description has a separate owner. [Ring custody and appraisal](../domain/ring-equipment-design.md#implemented-behavior-and-ordinary-input)

Every custody reconstruction preserves Depot: `Mark`, `AddDeal`, battle start/action validation,
continuation capture/equality/restore, field capture/restore and battle wire decode. New field-v1
and battle-v2 writes include `depot`. Only the compatible old formats' missing field means empty;
explicit null, malformed values, sentinel/out-of-range identities, unknown private identities and
more than64 entries reject admission. The existing slot reader validates before publishing and
does not rewrite a rejected source slot. Older readers reject the unfamiliar property rather than
silently discard it. New Game remains empty; unavailable live custody is never upgraded to empty.

Small engine behavior tests cover63→64/65th refusal, duplicate index order, full pack/depot exchange,
normalization, curse/unsellable guards, reserve mutation, stale/cancelled commands, callback failure,
rare saturation, source appraisal, old-format absence and invalid saves. Controlled real engine
victory/defeat/retreat tests carry nonempty storage through battle save and field return/restore.
These capacity/rare/curse and controlled outcome cases are not claimed as natural acquisition journeys.

The registered private `handoffs/issue1871` bundle uses the accepted issue1870 world and bilingual
catalogs read-only through explicit configuration; only the adapter runtime and bounded receipts are
new. Its `Play-en.cmd` / `Play-zh-CN.cmd` open the ordinary title. Continue → Slot1 starts at the
Map26 NPC. Review uses a fresh Sample and disposable namespace. No user slot, original input,
predecessor package or removable checkout is a writable dependency. The added-output budget is
400 MiB, with no world/media extraction. From that bundle in PowerShell7:

```powershell
./Play.ps1 -Language en -Sample depot-review -Mode journey
./Play.ps1 -Language zh-CN -Sample depot-review -Mode journey
./Play.ps1 -Language en -Sample field-review -Mode cold -Seed ./slots/depot-en.json
./Play.ps1 -Language zh-CN -Sample battle-review -Mode exercise -Seed ./slots/depot-battle-zh-CN.json
./Play.ps1 -Language en -Sample victory-review -Mode return -Seed ./slots/depot-victory-en.json
./Play.ps1 -Language zh-CN -Sample egress-review -Mode return -Seed ./slots/depot-egress-zh-CN.json
./Play.ps1 -Language en -Sample defeat-review -Mode return -Seed ./slots/depot-defeat-en.json
```

Both Map22 and Map26 have ordinary bilingual Depot observations. The Map22 route first purges
Peter to make him a reserve, then restores him after the transfers. The Map26 journey deposits Bowie's naturally held Wooden Sword and Sarah's herb,
retrieves into Sarah's pack, cancels then commits a full-pack exchange, retrieves into reserve Peter,
and deposits from Peter. It inspects the weapon, cancels a Drop and confirms a common-herb Drop.
The remaining weapon stays in storage through service return, ordinary field Save, Join Peter,
paid Church Raise and actual cave/Battle23 admission. These are ordinary input and naturally held
items; no save was edited to manufacture acquisition or storage. English ordinary Battle23 victory, English ordinary defeat and Chinese paid EGRESS retain the
nonempty depot through their own returns. Each is cold-loaded and walked back to the actual NPC
for Look. Both languages' field and battle cold Continue → Save preserve the entire decoded data;
battle cold Continue also commits Stay and reaches the next actor. Accepted old field-v1 and
Battle19 battle-v2 inputs retain all prior data and gain only an empty depot. A separately labelled
controlled null-depot save is refused in the real Continue list, with its original slot bytes intact.
The outcome/cold-read receipts and exact commands are retained in the stable handoff. Native finalizer/ObjectDB failures remain
separate from observer success under #1716; no leak-free-exit claim is made.

Verification uses the engine aggregate once, failed owning classes after corrections, the affected
Caravan behavior selection, actual adapter build and native observations, then the committed
planner/CI. Preserve the completed aggregate and failed nodes rather than repeat a whole suite to
replace its history. The stable `Rebuild.py` mounts a current Debug adapter into a fresh explicitly
selected output using retained inputs; import changed PO resources before launching. No new original
ROM/emulator evidence or original SRAM compatibility is claimed.

## Current Item implementation

**Confirmed:** [#1872](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1872) connects
Use / Give / Equip / Drop at the same ordinary Map22/26 NPCs. `CaravanItemService` owns the typed
selection phases, expected slot words, wait tokens, cancellation and publication. It calls the actual
`FieldInventory` / `FieldRecovery` mutations with an internal Caravan caller policy. The live service
wait is retained throughout. Ordinary `FieldCommand` and `QueryFieldActions` still require settled
field input and cannot bypass a service wait; no alternate inventory or save authority is introduced.

Self-Give removes the selected slot then appends its word, even when the member's pack is full.
Add clears equipped and preserves the existing broken marker. Different-member Give checks both
outgoing slots, compacts both packs and clears equipped on moved words. All initialized joined
members, including reserves, are selectable. Cancelling target selection restarts the donor loop;
cancelling exchange returns to target selection. Equip uses the class mask and separate weapon
and stat ring choices. Drop requires confirmation, compacts the selected slot and retains the existing rare Deals
saturation and unsellable rules. Known equipped cursed outgoing items refuse before broader
unsupported-loadout checks, including both sides of exchange and unequipping. Selection/transaction
failure preserves the snapshot; a later callback failure does not undo a committed item action.

Caravan Use checks the source field-usable identity table separately from map events and ordinary
Field NoEffect policies. Antidote clears poison, and Fairy Powder clears poison and paralysis.
Each consumes the selected holder's slot even if the target has no relevant status, with distinct
bilingual feedback. A missing or invalid target consumes nothing. Herbs, seeds and map-event items
refuse without payment. Stat and level items remain unsupported. Shared Field validation distinguishes pack
shape/known identities, the selected slot's operation admission, and actually equipped words.
A carried Power Ring can be transferred or equipped. A valid worn ring leaves other admitted
pack operations available, and a transferred or retrieved ring is bare. Unknown identities,
stale words, invalid packs and unsupported worn effects still reject. Equipment choices retain
source slot indices and omit unsupported or broken choices. Ring Use, durability, Repair and
curses have separate owners. [Static ring rules](../domain/ring-equipment-design.md#custody-promotion-and-continue)
The [Pacalon Use observation](pacalon-first-visit.md#fairy-powder-use) exercises an overworld Caravan
caller with a legally held Powder and a status-free target, then saves and cold-continues.

All item changes reuse `FieldInventory.Replace` and `ExplorationState.WithParty`, retaining depot,
OnceFlags/Deals, all other actor resources, RNG, active membership and story/transport continuity.
No persistence format change is needed. Small actual-engine tests also cover ring-bearing donor/recipient packs, full exchanges, Drop and
Antidote and Fairy Powder recovery under both callers, plus ring effect refusal and ordinary Field self-exchange.
They distinguish the full self-Give
rule from ordinary Field self-exchange, both exchange guards, supported and ineffective Antidote,
unsupported/unsellable refusal, stale/cancelled commands, equipment limits and failing callback return.

The registered private `handoffs/issue1872` bundle reuses the accepted issue1870 world/catalogs and
issue1871 slots read-only. Its `correction/Play-en.cmd` and `correction/Play-zh-CN.cmd` open the ordinary title;
Continue → Slot1 reaches Map26's NPC. Each review uses a fresh Sample and disposable save namespace.
From its current `correction/` directory in PowerShell7:

```powershell
./Play.ps1 -Language en -Sample items-review -Mode items-journey -Seed ./slots/depot-en.json
./Play.ps1 -Language zh-CN -Sample desktop-review -Mode items-desktop -Seed ./slots/desktop-zh-CN.json
./Play.ps1 -Language en -Sample field-review -Mode items-field -Seed ./slots/service-en.json
./Play.ps1 -Language zh-CN -Sample cold-review -Mode cold -Seed ./slots/items-zh-CN.json
./Play.ps1 -Language en -Sample battle-review -Mode exercise -Seed ./slots/items-battle-en.json
python -X utf8 ../Compare.py
```

Both languages exercise ordinary Map22/26 entry, naturally held full-pack self-Give, cross-member
exchange/cancellation, re-equipping the admitted weapon, unsupported Brave Apple and nonusable item
feedback, target cancellation, Drop cancellation/commit, source text10 and field Save. Map26's
nonempty depot persists through paid Church recovery, ordinary cave entry and real Battle23 admission.
Cold Continue → Save preserves the complete decoded field and battle records in both languages;
ordinary Stay reaches the next actor. Known old field-v1 and battle-v2 slots retain all prior data
and only gain empty depot. Separate bilingual ordinary Field input still shows full self-exchange,
keeps a no-effect recovery item, equips, returns, saves and moves. Native Antidote/curse acquisition
is not claimed: those cases use actual-engine controlled tests. The preceding depot slice's natural
victory/defeat/EGRESS evidence is retained, not relabelled as a new Item-slice outcome run.

The same original retained seed with Jaha's `[161,19,127,127]` pack also completes a bounded bilingual
ordinary route in the registered bundle's `correction/` reader: Caravan gives the supported axe to
Peter, receives Healing Seed from Bowie, refuses ring Use without consumption, and returns. Ordinary
Field then queries Jaha, performs no-effect Seed Use and gives the Seed back. Power Ring19 remains
unchanged, all other actor resources remain intact, and the complete cold field-save data agree.
This uses the original ring-bearing history, not a replacement pack. Reproduce from `correction/`:

```powershell
./Play.ps1 -Language en -Sample ring-review -Mode ring -Seed ./slots/service-en.json
./Play.ps1 -Language zh-CN -Sample ring-review -Mode ring -Seed ./slots/service-zh-CN.json
./Play.ps1 -Language en -Sample ring-cold-review -Mode cold -Seed ./receipts/en-ring-review/ring-slot.json
python -X utf8 ../CompareCorrection.py
```

`player/` retains the earlier receipts unchanged; `correction/` mounts the current runtime and its
`Rebuild.py` reconstructs that candidate with the same input configuration. Earlier completed local
aggregates/native observations are retained; the correction runs affected actual-engine tests,
adapter/selected dependent builds, the bounded ring route and final CI. NPC greeting/return ticks can
advance RNG, so native seed comparisons separate service selections from ordinary Field actions;
complete save comparisons retain the actual resulting seeds.

High rigor remains appropriate for the shared inventory/save boundary. New output is bounded to one
small candidate runtime, reused immutable inputs and selected state/input receipts, budget250 MiB;
no world/media extraction or new tool platform is needed. Rebuild the owning Debug adapter, then run
`correction/Rebuild.py --repository <checkout> --output <fresh-output>` and import changed PO resources
with the configured Godot before launching. The engine aggregate, affected behavior selection, actual
adapter, direct native observations, clean committed planner and selected CI own separate checks.
The stable handoff retains exact candidate, commands and failures, including native exit status
separately from game/observer state. #1716 finalizer/ObjectDB clean-exit behavior remains **Unknown**;
no new original-game runtime, hardware or SRAM claim is made.

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
  lists IDs3,5,9–15. `FieldRecovery` admits Antidote and Fairy Powder for Caravan Use. The Field
  caller also owns ordinary no-effect handling and map-event/EGRESS policies.
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
| Depot lifetime | Domain `Battles/State/BattleItemCustody.cs` contains OnceFlags, Deals and the immutable ordered depot list, bounded at64; do not encode depot entries in Deals. It already crosses BattleStartInput → battle → outcome → field. Update every reconstruction (`Mark`, `AddDeal`, continuation capture/restore and wire decode) to retain depot, including a rare drop while depot is nonempty. A nullable custody still means unavailable knowledge, not empty inventory. |
| Persistence | Application `Runtime/Persistence/OpeningSaveData.cs`, `OpeningSave.cs`, `OpeningRestore.cs`; Domain `Battles/State/BattleContinuationData.cs`, `BattleContinuationRestore.cs`; Content `Persistence/BattleSaveWire.cs`, `OpeningSaveJson.cs`; Application `Runtime/Persistence/BattleSaveStory.cs`. Carry/validate depot at both field and battle boundaries. Wire admission currently requires properties except an explicit compatibility list; merely adding a C# default is not enough. |

The concrete new state prevents item loss at a battle/save boundary. A UI-owned array would be lost
there; Deals cannot express duplicate ordered slots or transfer semantics. Extending existing custody
is smaller than adding a second campaign store and propagating it through every battle/field copy.
No new cache, manifest, checksum or generic migration framework is justified.

For old accepted field-v1 and battle-v2 saves, the implemented optional depot property has absence meaning
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

These are the serialized slice boundaries implemented above, not a request to create parallel writers.
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
   member Give, exact slot compaction, supported Antidote and Fairy Powder consumption, refusal of nonusable items,
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

For original-source review, use the accepted base and pinned source above. Read the named routines with
`git -C local/upstream/SF2DISASM show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`;
verify that checkout's HEAD/status before filesystem reads. Compare tracked service evidence in
`tests/fixtures/h2/common-menus-static-v1.json`, `common-stats-static-v1.json` and
`field-item-effects-static-v1.json` with the linked owners. Their existing reproduction commands are
`uv run sf2 h2 common-menus`, `uv run sf2 h2 common-stats` and `uv run sf2 h2 field-item-effects`;
these are provenance routes, not newly claimed gate runs in this design. Run `git diff --check`,
resolve affected relative links, inspect the owned staged diff/private boundary, and obtain
`uv run sf2 verify plan --base origin/main --head HEAD` on the clean committed candidate. Do not
launch engine/H3 suites solely for a documentation change.

Implementation acceptance uses small actual Domain/Application/Content behavior tests via
`uv run sf2 verify engine`, `uv run sf2 verify adapter`, and the affected ordinary host observations.
Follow the [environment and scope owner](../development-and-verification.md#scope); no tests of
probes/planners or new comparison platform. Exact source/H2 comparison is needed only if its claim
changes. Use real state/input/projection and error observations, never screenshots.

The retained ordinary starting-input ancestry includes the registered `issue1864` handoff's
`slots/floor-en.json` and `slots/floor-zh-CN.json`, and the earlier Desktop victory slots identified
by [its reproduction route](cotton-balloon-floor-world.md#confirmed-remake-observations).
The implementation bundles above retain their descendants. Copy selected seeds to disposable review
namespaces, use the compatible candidate bundle and matching localization, and visit the actual
NPC; do not mistake a predecessor binary's capability boundary for candidate behavior.
The accepted [Creed chapter](creed-recruitment.md) supplies the post-recruit ordinary slot and actual
overworld vehicle caller into this existing service; its current route is documented under
[overworld vehicle entry](#current-overworld-vehicle-entry). At each relevant boundary verify:

- top/nested cancellation, held Confirm, stale command and failed transaction retain state/control;
- different valid party/item choices work, including a dead reserve and full packs, not just one
  named roster or fixture sequence;
- service exit permits movement and Save; cold Continue preserves the complete decoded progress;
- entering a supported next battle uses the chosen active members and their actual equipment;
  victory, defeat/EGRESS, battle Save/Continue and a later depot visit retain storage and reserves;
- bilingual names, prompts, failure reasons and actual input reach the same command outcomes.

**Unknown:** ordinary original transaction timing/persistence. Wider item effects and equipment
lifecycles are known unsupported remake capabilities, not missing original static evidence. A global
Caravan hotkey, debug BattleTest entry, blacksmith, shop Deals/Repair, church promotion and original
SRAM compatibility are outside this bounded service delivery. Later recruitment variants remain
with their source and optional chapter owners.
