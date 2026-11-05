# Local Services and Battle Overview

This is the design proposal for [#1543](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1543).
**Standard rigor:** reduce repeated service travel and battlefield orientation work while keeping
story access, transactions and hidden information under their existing owners. This page owns the
recommendations, product choices and minimum acceptance; it does not authorize implementation.
The pre-voyage main story remains the delivery priority. No implementation Issues are created here.

Recommend **near-town services after a first visit**, plus an **on-demand, read-only battle overview**.
Remote treatment and shopping deliberately reduce travel risk; they are not original-game fidelity
corrections. Product approval is **Unknown**. Retain current save eligibility rather than coupling
remote Church access to save-anywhere. The following boundaries describe the proposed first version,
not capabilities already available to players.

## What can actually be reused

The inspected accepted implementation is
[`27f7416bbb58d62c9cc7995b44186d9b0f695544`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/27f7416bbb58d62c9cc7995b44186d9b0f695544).
**Confirmed — source structure and linked retained observations:**

| Surface | Actual capability and access | Boundary that the shortcut must retain |
| --- | --- | --- |
| Galam Shop | Finite Buy/Sell through the real F663 Map16 entity135/144 callers, inventories1/16. The separate controlled private consumer has Talk/trade/return observations; the authored purchase→battle consumer demonstrates carried supplies. | Default Map16 has no shop callers; F530 is a separate unadmitted setup. Full ordinary Galam story access is not established by this service demonstration. Deals/Repair and rare/broken sales remain unsupported. |
| Opening Church | Actual Map3 default/F501 priest: ordered Raise, poison/stun Cure, Record adventure and Leave. Payment, decline, insufficient funds and real caller return are implemented. | No Promotion, curse removal or general full-party heal. Raise retains its cost and HP cap; Cure retains status order and fees. A service cannot save while its caller is waiting. |
| Party | Field Party Status/details and Prepare: Equip/Unarmed, Give/exchange, supported Use/Magic and confirmed Drop. | It is personal field preparation, not a town service or Caravan recruitment/depot. Member/content and action legality remain; field HEAL/Herb/Seed do not become free field HP recovery. |
| Save | Shared two-slot host, revision-checked capture, explicit destination/overwrite and existing recovery/Continue rules. Church asks to close first, then reaches that host. | QuerySave requires a supported profile, settled field and supported recipe. Map16, arbitrary remote locations, battle suspend and autosave are not enabled. Menu Save and Church Save grant the same permission. |

The [capability owner](../capability-status.md#finite-galam-shop-and-purchased-supply-closure),
[Galam admission](../content/profiles-and-trust.md#finite-galam-shop-admission),
[Church agreement](../application/opening-session-lifecycle.md#implemented-opening-church) and
[field preparation agreement](../application/opening-session-lifecycle.md#implemented-field-preparation)
own the detailed limits. The accepted campaign reaches Battle04 outcomes and the first prison input;
work on Battle05–06 under #1538 is not evidence of supported services or ordinary reach here.

Original-game evidence is separate. The
[service contract](../../../docs/design/contracts/party/service-interactions.md) names pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, its H2 fixture and bounded Church H3 seams.
The original static menu includes Buy/Sell/Repair/Deals and Raise/Cure/Promote/Save, a wider surface
than this remake admission. Its natural caller timing and broader lifecycle remain **Unknown**.
Neither this proposal nor a remake transaction proves original remote service access.

### Entry, writes and return were checked together

These are reproduction routes for source inspection at the accepted object, not new runtime passes:

| Chain | Owning source and result |
| --- | --- |
| Setup → actual service instruction | The Galam admission above traces `mapsetups.asm` → F663 entity-event table → caller body → `CURRENT_SHOP_INDEX` → `j_ShopMenu`. The [source compiler](../../../src/sf2tool/remake_exploration_programs.py) lowers that assignment/call; [ShopInstructionReader](../../src/Sf2.Remake.Content/Scenarios/ShopInstructionReader.cs) binds source stock/item terms rather than UI prices. A body filename shared with another setup does not grant access. |
| Talk → program → wait | [ExplorationInteractionQuery](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationInteractionQuery.cs) and [ExplorationDispatcher.Interact](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs) check actual entity, counter reach/facing and flag-selected event. [MapEventDispatcher](../../src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs) installs caller/event context; [ProgramRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) creates ShopWait/ChurchWait at OpenShop/OpenChurch. |
| Answer → payment/custody → continuation | [GameSession.Submit](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) checks session/revision; [ShopService](../../src/Sf2.Remake.Application/Runtime/Exploration/ShopService.cs) and [FieldService](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldService.cs) require the live wait/token/offer, recheck resources and replace the carried party atomically. Closing runs the retained caller; it is not simply hiding a window. |
| Party and Save consume that same state | [FieldInventory](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldInventory.cs) checks settled field, member and content; [PartyStatusView](../../game/src/Exploration/PartyStatusView.cs) uses QueryPartyDetails. [ExplorationSessionView](../../game/src/Exploration/ExplorationSessionView.cs) delays an accepted Church Save request until CanWaitAtInput. [OpeningPersistenceHost](../../game/src/Entry/OpeningPersistenceHost.cs) queries/captures through [OpeningSave](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs), then LocalSaveStore writes the common slots. |

Thus a host shortcut cannot submit a ShopCommand to ordinary field input, fabricate a ChurchWait,
move the player to a merchant, or run an absent NPC's whole event script. Those are missing entry
capabilities, not permissions inferred from a compiled service definition. No factual conflict in
this inspected chain needs product escalation; genuinely new access rules below do. Future evidence
conflicts follow the [operations workflow](../../../docs/operations/github-project-governance.md#resolve-evidence-conflicts-before-product-choices).

## Recommended town access

**Proposed, awaiting product choice:** Menu → Journey → Local services names one eligible town and
offers its currently admitted services. Access works within that town's ordinary safe areas and
within a small content-declared approach area outside its entrance. A player who has already visited
and unlocked the service can stop outside and buy or seek treatment without loading the town or
walking to the NPC. It is not a global list of every visited merchant.

| Condition at request time | Proposed result |
| --- | --- |
| First arrival / town never visited | No remote purchase or treatment. Enter town normally; allow the shortcut only after its service is encountered and admitted by the current story. Merely seeing the town on a map is insufficient. |
| Visited town, unlocked service, safe town/approach area | Offer only that town's current setup and service list. This is the intended repeat-visit shortcut. |
| Farther wilderness, dungeon, another town's territory | No remote service. Return to an eligible approach or use ordinary world access; do not choose the nearest shop by distance alone. |
| Battle, dialogue, cutscene, transition, pending encounter or another transaction | Unavailable with one contextual reason. Opening the host Menu can pause presentation but never makes a service legal. |
| Story lockdown, evacuated town, absent provider or changed setup | Re-evaluate current setup and availability; prior discovery does not override closure or expose future stock. |
| Two approach areas overlap / association unresolved | No automatic nearest-town fallback. Content must assign an unambiguous town or withhold access until it does. |
| Save requested from Local services | Close the service and query the real current field location. Unsupported destination stays unavailable; the town's Church does not lend its save recipe to the player. |

Application owns the availability decision and committing entry; Content owns town/approach
association, discoverable provider, setup/story predicates and stock bindings. The host only renders
the result and submits intent. Map IDs alone, imported inventory tables and UI visibility are never
authorization. Prefer existing story/visit facts where they prove discovery; when none does, a future
implementation needs a minimal owned discovery fact with explicit save/Continue semantics. No such
fact or town-service association is implemented now, and loading a save must not unlock all services.

The smallest implementation would add a bounded Application entry into the existing service
transaction, with a continuation back to the requesting settled field state. It must reuse the same
Shop/Church terms, payment and inventory/status owners. It must not replay off-map NPC movement,
dialogue or story flags. Validate availability at entry and at confirmation against current
session/revision and setup; preserve the existing stale-token/offer checks. A changed/closed service
refuses without charging and refreshes its reason. No second gold balance, inventory, service price
cache, delivery currency, simulated travel or new remote-stock pool is proposed. Existing stock
lists are not evidence of a finite quantity system; retain their actual behavior.

NPC conversation, discovery, quests and first-visit scenes stay in the world. A remote service is
labelled as a service, not Talk, and cannot mark a conversation heard. A provider whose service cannot
be separated safely from a required scene remains world-only until its owner establishes that seam.
Content admission and the ordinary campaign connection are prerequisites for a Galam shortcut;
the controlled F663 example must not make shops appear during the current prison handoff.

### Gameplay choice, including saving

| Option | Benefit and cost | Recommendation |
| --- | --- | --- |
| Inside-town shortcut only | Removes counter navigation; preserves approach/return travel and more town ambience. Does not satisfy the wish to transact without entering town. | Conservative fallback if remote risk reduction is unwanted. |
| Visited, unlocked near-town service | Removes town load/walk/Talk on repeat visits; retains a return trip from distant danger. Paid Raise/Cure before re-entering town and purchases at the gate can still make recovery and resupply easier. | **Recommend**, subject to product approval of that deliberate reduction in risk. |
| Any visited town from anywhere | Maximum convenience, but money can replace route planning; remote selling frees full packs and funds purchases, and repeated revival/cure changes attrition. Even unchanged prices do not preserve the original economy. | Defer; not the default remote interpretation. |
| Remote Church also enables Save anywhere | Reduces lost progress but changes retry exposure and permits saving after resupply at new locations; it needs new capture/restore boundaries and compatibility decisions. | Do not include. Retain current QuerySave and Continue destination. |

Near-town access preserves prices, four-slot capacity, supported stock and treatment charges, not
all strategic costs: fewer walks can avoid movement-triggered encounters and remove the cost of
returning to a priest with defeated members. It adds no free HP/MP refill and no treatment during a
battle. Fees, cooldowns or stock scarcity would be separate balance changes; do not invent them to
claim fidelity. Save remains an explicit player action: a paid trade is not durable until an eligible
save succeeds. Explain an unavailable save once at that entry, without a permanent warning banner.

## One menu and predictable returns

Keep the [unified Menu](presentation.md#system-menu-agreement-1518), 1080p baseline and 96px world
cells. Add contextual Journey rows within its existing theme, rather than another screen-edge
toolbar. Resume remains the initial default; field Party and Save shortcuts remain aliases to their
existing owners. Battle Overview is a battle-only row. Party is not dependent on a nearby town.

The sketch is proposed content/hierarchy, not a fixed-height pixel specification. Keep 64px action
rows, readable text, long Chinese wrapping and focus visibility; use a bounded scroll area if the
extra Journey rows do not fit. Do not shrink the world or stack multiple modal hints to fit.

```text
Menu                                 Local services · Galam
  > Resume game                        > Item shop       [Buy / Sell]
  Journey                                Weapon shop     [Buy / Sell]
    Local services · Galam…              Back
    Party…                             Selected row: current stock and funds
    Save…  [unavailable: this location]   Deals / Repair: unavailable
    Settings…                          (No Church inferred from this shop admission.)
  Session
    New Game…                        Opening Church · Granseal [separate example]
    Quit…                              > Raise           [existing ordered offers]
                                         Cure            [poison / stun]
Battle Journey row instead:               Promotion       [unavailable]
    Battle overview…                     Record adventure… (Save)
                                         Leave
```

| Interaction | Focus and control after it |
| --- | --- |
| Menu → Local services → shop/treatment | Transfer to the one service input owner; keep parent menus covered. Reuse stock/member/slot/offer and paid/declined result steps. Show cost and target before committing; never auto-buy or auto-treat on entry. |
| Back during a transaction | Retain that transaction's existing phase semantics. At its top-level Leave/Back, complete the service continuation, then restore the chosen provider row. Back from Local services restores its Menu row; Resume returns to the original field position/facing. Completed payments remain completed. |
| Failure or service disappears | Recoverable refusal stays with the responsible panel; after closure, refresh parent availability and retain focus if possible, otherwise Back. A failed continuation retains the real stopped-scene failure, not a fake successful return. |
| Menu → Party → Prepare | Use existing member and action selections. Close Prepare to Party, close Party to the originating Menu row; the existing direct field shortcut still closes to field. This menu-origin return is proposed host wiring, not current behavior. |
| Menu → Save | Existing confirmation Back → slots → Menu; display actual Continue destination and keep Cancel initially selected for overwrite. |
| Remote Church → Record adventure | Finish the remote service first, then call the same save host. Close success/cancel/error to Local services only after field has settled, with Church focused; reopening creates a fresh service wait. Ordinary NPC Church keeps its existing return to exploration. Never restore a consumed ChurchWait behind Save. |
| Pause, focus loss, mouse release or controller hold | Retain existing pause/recovery ownership, neutral-input/release guards and pointer cancellation. Returning cannot reuse the gesture to buy, overwrite, move or advance a turn. |

Match [#1544](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1544)'s already stated
minimal-hint direction: keep the relevant Confirm/Back action, selected-row information and a useful
disabled reason; full bindings stay in existing help. Prices, overwrite risk and illegal-action
reasons are necessary information, not clutter. Keyboard, pad and mouse reach the same actions;
focus and unavailable states use text/shape as well as color. This is not a new hints system.

For a repeat Galam visit, today's controlled example requires walking into counter reach, Talk,
Buy → stock → member → confirmation → result → Leave, then walking back. The proposal replaces
the travel/Talk segment with Menu → Local services → Item shop; the transaction segment is unchanged.
For the Map3 priest, replace the repeated priest approach with the same local-services entry while
retaining ordered treatment confirmations. From an eligible approach this also removes entering and
leaving town; a player already at the counter can still use Talk, which may be faster. These are
**Inferred** interaction savings, not measured seconds, exact button counts or a human usability pass.

## Read-only battle overview

Recommend an on-demand whole-board schematic through Menu → Battle overview, first at a settled
player movement/action/target selection. Disable it during entry, automatic enemy turns, movement,
attack/result/death presentation, region programs, appearances, retreat or outcome return. This
bounded opening rule avoids adding a parallel simulation/pause or reveal-history system. Existing
Menu may still open in those phases; its overview row explains that the player must finish the
current sequence. Closing restores the same Menu row and then the unchanged battle selection.

```text
Battle overview · current board                Legend / selected known unit
  +-----------------------------+              A  Ally        e  Known enemy
  | A A       ....^^^^          |              @  Active ally  [ ] Camera area
  |   @ [camera] ....     e     |              ^  Terrain     .  Open ground
  |         ....      e        |              Bowie · current public status
  +-----------------------------+              Objective: only if already disclosed
  Back                                        (No destination/attack confirmation)
```

The dots are a schematic, not Battle01 coordinates or a claim about its terrain. Use Battle01 at an
ordinary player turn as the first finite comparison: player identifies the active ally and known
enemies outside the 96px-cell viewport, closes the overview and continues the same selection.
Opening/closing a panel replaces repeated orientation searches; it does not promise fewer inputs
than an already useful automatic camera. Human readability/effort gains remain **Inferred**.

| Displayed information | Source and disclosure boundary |
| --- | --- |
| Board outline and terrain classes | Current BattleDefinition width/height and admitted semantic terrain, clipped to its SourceMapRegion. Do not render the full source world map, adjacent rooms, hidden passages or unadmitted overlays. Distinguish terrain from unit-specific traversability; no new pathfinding claim. If a later map hides terrain, withhold that information until an explicit disclosure rule exists. |
| Allies and known enemies | Same current-session logical actors and public positions used for the ordinary board, after its appearance/visibility restrictions. No raw deployment roster, future spawn slots, reserve enemies or total-enemy counter. An off-camera already displayed enemy remains known; screen clipping alone is not fog of war. |
| Active actor, target and preview | Same Selection and approved board projection. Distinguish committed position from an uncommitted movement preview; marking a preview never commits movement. Do not display an enemy AI target, planned path or future queue. |
| Camera rectangle | Current BattleMapViewport/WorldViewport geometry clipped to the battle region, accounting for its origin and scale. Read the camera; opening the overview does not retarget it. |
| Objective | Only already player-disclosed text/position. No marker inferred from victory predicates, future events, hidden region triggers or a boss deployment. There is no general disclosed-objective query in this inspected presentation; omit the row/marker until an owning content contract supplies it. |

**Confirmed — source structure:** [BattleSnapshotProjection](../../game/src/Battles/BattlePresentation.cs)
builds markers from actors with positions, while its roster iterates all actors. It is not a general
player-knowledge API. [BattleSpawnRules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleSpawnRules.cs)
can populate a batch before [BattleRoundContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRoundContinuation.cs)
finishes individual appearances. [BattleMapViewport.PresentAppearance](../../game/src/Battles/BattleMapViewport.cs)
hides later actors in that batch; reading positions alone during that sequence can reveal them early.
The first overview therefore requires the settled selection boundary and public-board filtering;
it must not copy the diagnostic roster. A future continuously visible minimap needs an explicit
shared disclosure decision first. The current camera follows selection/story subjects through
Frame/WorldViewport.Focus; a freely pannable overview camera is not already implemented.

First version is **read-only**, including no click-to-move, auto-path, target selection, AI threat
union or camera movement. If later approved, click-to-center and a keyboard/pad equivalent may change
only camera focus, with an explicit Return to active unit. That separate extension needs a temporary
inspection focus and return policy so Frame does not immediately override it; it must not submit a
movement/target command. Do not bundle that extension into the first overview's acceptance.

Use distinct shapes plus labels, an inspectable known-unit list and scalable panel content rather
than requiring a tiny colored dot or precise mouse hit. Do not reduce 96px gameplay cells to create a
permanent corner map. No new art assets or map capture pipeline are needed for this design.

## Minimum acceptance if selected for implementation

These checks define the proposed behavior; they have **not** been run or passed by this design task.

| Boundary | Smallest useful acceptance |
| --- | --- |
| Availability | One visited/unlocked safe approach works; first visit, distant/dungeon, wrong/late setup, closure, battle and story-busy cases refuse. Reload retains only justified discovery. No direct command bypasses the query or stale-state rejection. |
| Real service reuse | A bounded Galam F663 buy/sell and Map3 Raise/Cure use actual current terms and carried party. Verify exact payment/inventory or HP/status change, full pack, insufficient gold, decline and stale/duplicate confirmation. No remote-only prices, free revival, repair or promotion appears. |
| Return and Save | Each origin returns to its declared focus; pause/held input cannot double-submit. Ordinary NPC continuation still completes. Remote Church Save at an ineligible location refuses; an eligible existing recipe retains destination, overwrite Cancel, failure recovery and fresh-process Continue of completed resources, without reapplying a payment. |
| Overview | One ordinary Battle01 stable selection shows the current board, active ally, public enemies and correct camera extent. Open/close leaves position, selection, turn, resources, RNG and progress unchanged. A bounded existing appearance example establishes that pending actors/hidden objectives never leak and busy phases refuse. |
| Accessible, minimal presentation | At 1080p with the 96px world, EN/zh-CN content fits or scrolls without losing focus, prices or reasons. Keyboard/pad/mouse and existing remaps reach the same actions; color alone is insufficient. Human legibility remains separately reviewable. |

Future code acceptance uses small actual-engine behavior tests for new entry/disclosure rules and
direct existing adapter state/input/projection/error observation where needed. No screenshots,
measurement platform, comparator framework or tests of verification tools follow from this page.
Original natural service reach, all-device aesthetics and retained ObjectDB shutdown warnings remain
**Unknown**; this proposal neither resolves them nor reruns historical failures.

## Limited follow-up recommendation

The relative assessment below is design judgment (**Inferred**), not a cost benchmark. At most
three next steps are recommended, after the current main-story priority and explicit selection;
the remaining candidates are deferred rather than a new implementation backlog.

| Rank / candidate | Benefit | Rules / information risk | Complexity and accessibility | Existing owner / disposition |
| --- | --- | --- | --- | --- |
| 1 · Near-town services | Removes repetitive town entry and NPC travel | Deliberate resupply/revival risk reduction; retain story, stock, payment and Save gates | Medium: new availability/entry and return wiring; reuse accessible transaction controls | This proposal + Application service/lifecycle owners; product choice before a bounded implementation |
| 2 · Read-only battle overview | Improves spatial orientation beyond the viewport | No simulation change; hidden appearances/objectives must remain undisclosed | Medium: public projection and readable schematic/list; on-demand avoids crowding | This proposal + BattlePresentation/BattleMapViewport; choose first-version disclosure boundary |
| 3 · Minimal contextual hints | Less visual noise across existing controls | Low if fees, errors and dangerous confirmations stay visible | Low/medium, limited screen group; existing help/remaps preserve discoverability | Existing #1544, already backlog; review its design later, no duplicate ticket |
| Deferred · Camera locate from overview | Faster inspection of a known unit | Presentation only if focus is separated from gameplay selection | Medium: temporary focus and return; require pad/keyboard parity | Existing viewport owner; separate extension after read-only use is judged useful |
| Deferred · Enemy threat overlay | Faster tactical comparison | Can reveal activation, ranges or AI intent; geometric range is not predicted AI action | Medium/high disclosure and terrain semantics; crowded for color/vision needs | Gameplay rules and movement-range owners; no full AI-plan display |
| Deferred · Animation speed / broader skip | Shorter repetition | Completion/payment/story order must survive; cannot skip rules | Coupled lifecycle work; reuse Reduced Motion and existing supported skips first | Presentation/settings and session owners; no global speed system |
| Deferred · Bulk inventory organization | Fewer repeated slot operations | Changes custody/equipment, full-pack exchange and possibly sale economy | Medium/high transactional scope; preview/undo intent needs design | FieldInventory/Prepare owner; no batch sell or auto-equip |
| Deferred · Route guidance | Easier navigation | Risks quest/spawn spoilers and replacing exploration | Medium content disclosure and accessible wayfinding | Campaign/content owners; no new quest tracker or auto-path |

The product choices are concrete: accept near-town remote replenishment or choose inside-town-only;
accept an on-demand overview restricted to stable player selections or defer it. Recommended defaults
are near-town access and the restricted overview, with existing Save limits. Timing relative to the
main story remains a scheduling decision, not authorization to start all three recommendations.

## Design-task verification

This task changes only this owner and short navigation/status in presentation and opening UI gaps.
Acceptance is source-chain review, local Markdown target/anchor and scope checks, `git diff --check`,
the clean committed `uv run sf2 verify plan --base origin/main --head HEAD`, and actual scoped CI.
Load the owning ignored input configuration for the planner as required by the repository. No SDK,
Godot, emulator, runtime evidence acquisition or new fixtures are required. Exact head, planner
selection, CI conclusions and stopped-writer handoff belong in the Draft PR/Issue, not a rolling
history here. All report material remains below the 2MiB limit and excludes private content/paths.
