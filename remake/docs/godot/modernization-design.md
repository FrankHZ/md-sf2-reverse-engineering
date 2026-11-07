# Modernization Design

This page owns separate proposals: local services/battle overview under #1543 below,
[optional objective and destination guidance](#optional-objective-and-destination-guidance-1578)
under #1578, and [held 1.5× exploration and battle acceleration](#held-15-acceleration-1619) under #1619.
Their implementation and acceptance decisions remain separate. The
[UI language](ui-design-language.md) and [scene review](ui-scene-review.md) own #1616's visual design.

## Local services and battle overview (#1543)

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
| Separate · Held acceleration / broader skip | Shorter repetition | Completion/payment/story order must survive; cannot skip rules | [#1619's bounded clock/input design](#held-15-acceleration-1619) covers exploration and battle; broader skip remains deferred | Existing presentation/session owners; do not start from this #1543 recommendation |
| Deferred · Bulk inventory organization | Fewer repeated slot operations | Changes custody/equipment, full-pack exchange and possibly sale economy | Medium/high transactional scope; preview/undo intent needs design | FieldInventory/Prepare owner; no batch sell or auto-equip |
| Separate · Route guidance | Easier navigation | Risks quest/spawn spoilers and replacing exploration | [#1578's bounded proposal](#optional-objective-and-destination-guidance-1578) below; independent implementation acceptance | Campaign/content owners; no new quest tracker or auto-path |

The product choices are concrete: accept near-town remote replenishment or choose inside-town-only;
accept an on-demand overview restricted to stable player selections or defer it. Recommended defaults
are near-town access and the restricted overview, with existing Save limits. Timing relative to the
main story remains a scheduling decision, not authorization to start all three recommendations.

## Optional objective and destination guidance (#1578)

First-stage design for [#1578](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578).
**Standard rigor:** one optional aid spans content disclosure, state queries, preferences and field
presentation. The outcome is a reviewable recommendation and bounded implementation contract;
no guidance is implemented or runtime-accepted by this document. Recommend **Modern guidance: On**
by default, **one short objective in the exploration gameplay menu**, and **a named next exit or
local destination with a quiet marker when its location is justified**. This helps a returning
player remember an already disclosed task without adding a permanent quest paragraph. Default,
marker usefulness and human readability remain product recommendations awaiting review.

This is intentional modernization, not an original quest-system claim. Reuse the
[shared visual language](ui-design-language.md) and [field composition](ui-scene-review.md#3-exploration-world-speech-choices-and-search):
original freestanding diamond, adjacent blue reading frame, gold only for focus, visible world,
1080p baseline and 96px cells. #1543's host Journey/services remain a separate proposal. The objective
belongs to the existing **Member / Magic / Item / Search gameplay diamond**, not the host
Settings/Save/Quit menu. #1544's accepted minimal-hint direction applies; its deferred implementation,
device-label cleanup and wider hint migration are not prerequisites or work authorized here.

### A normal chapter with an observable before and after

Select **Kazin's offer in Yeel → travel toward Hawel's house → first house scene and revisit**.
It has town, overworld, encounter interruption and completion boundaries already supported in the
[pre-voyage campaign](../application/pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment).
Do not start from the damaged ship or admit the next campaign chapter just to demonstrate guidance.

**Confirmed — accepted remake observations:** the linked campaign records ordinary New Game,
Battle01 save/Continue, Battle02, Kazin's Yeel conversation, defeat recovery, Battle03 victory,
Map66(6,6) → Map43 entry, recruitment and F612 reentry without duplicate enrollment. It also records
`save-location-unavailable` on Map66. These retained results establish an available consumer, not
a new native pass or permission to save at the new guidance checkpoints.
The [ordinary opening/return observation](../application/pre-voyage-campaign.md#ordinary-new-game-through-the-battle03-return)
also records paid EGRESS to Yeel, defeat recovery and a walk through the north village exit for
Battle03 retry. That is a concrete existing approach to reuse for the town hint, not an assumed
straight line from a battle's arbitrary victory tile.

**Confirmed — original static structure:** the
[Yeel/overworld contract](../../../docs/design/synthesis/chapters/grans-opening/battle02-outcome-through-battle03.md)
owns pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, map/event symbols and source links.
Yeel is Map5; its north exit returns to Map66(18,21), south exit to Map66(19,24). Kazin's fixed
texts784–787 offer a guide to Hawel outside the village; the caller sets F72 after `cs_51B14` returns.
F72 establishes following, not recruitment F4 or encounter unlock F403. A separate villager's
texts767–769 give northwest information, but there is no demonstrated durable “heard that line”
flag. Never infer that conversation from being in Yeel, or parse its rendered text at runtime.
Original natural traversal, timing and save persistence are **Unknown** in that static contract;
the remake observations above do not resolve those original-game Unknowns.

The finite coverage below is a **proposed modern content rule**, not inferred original mission logic.
Bind it to the admitted pre-voyage profile and compatible selected setups, not raw map numbers alone.

| Supported state / checkpoint | Objective and permitted destination | Required withholding or refresh |
| --- | --- | --- |
| Yeel before completed guide caller; F72 clear, F4/F612 clear | No pilot objective. “No guidance available here” is optional menu-only fallback. | Do not invent “Talk to Kazin,” a quest giver marker or knowledge of Hawel from the full dialogue catalog. This is uncovered guidance, not a claim that the story has no next task. |
| Settled Yeel after the caller; F72 set, F4/F612 clear | “Visit Hawel with Kazin.” Destination: “Hawel's house · outside Yeel.” Candidate next step: “North exit.” | The north exit is a recommended modern route, not a mandatory story gate. Publish that local hint only after checking the current setup, exit binding and walkable approach. Do not attribute northwest knowledge to the unrelated villager. |
| Same guide state on Map66, before Battle03 completion F503 | Same objective; identify the known house only at the precision justified by the guide disclosure. | Do not expose Battle03, soldiers, enemy positions, scene triggers or house-event consequences. If no justified traversable leg is bound, show “Route not available” with no bearing/marker; a geometric arrow to the house would falsely promise access across the encounter. |
| Battle, dialogue, automatic entry/return or stopped scene | No field objective panel or destination marker. | Preserve the actual owner and failure. Requery after usable field control, including defeat/Egress return; never use a cached victory or reset F72 to fit a planned route. |
| Map66 after F503, still following and before F4/F612 | Same objective; final approach is the real house entry at Map66(6,5), from the recorded (6,6) approach. | Recommend an entrance marker only once that ordinary public entrance is visible in the current field view and its approach is valid. Off-camera retain the known house/area label without an exact bearing. Coordinates stay internal; no newly revealed terrain or event zone is rendered. |
| Map43 entry scene running, then F4/F612 set and F72 clear | Hide during the scene; after it settles remove this objective and every pilot destination. On revisit keep it removed. | Do not chain to Battle04 or reveal Hawel's fate/recruitment before the real scene. No “quest complete” flag, reward or toast is added. |
| Other map/profile/setup, missing binding or inconsistent F72/F4/F612 combination | No pilot guidance; optional natural menu fallback. | Treat coverage as Unsupported. Never fall back to the previous map's coordinate or choose an arbitrary rule by order. |

**Inferred benefit:** today the player must remember the offer, find an exit and rediscover the house
approach; the proposal makes the known task and next supported leg inspectable without another
conversation. Compare identical reached checkpoints with guidance Off/On: can the player name the
goal and identify a justified next exit/entrance, and then make a legal move? Fewer inputs or faster
travel are not promised. The before case retains ordinary Talk, map travel and encounters; the after
case changes only information. If an exit/precision is unproved, the truthful missing-route state is
part of acceptance, not a reason to add pathfinding or a knowledge-history subsystem.

### One state query and one preference

**Confirmed — inspected source structure at accepted
[`9e72a85eb97aa85147a0c80bce43b8234d4b9c00`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/9e72a85eb97aa85147a0c80bce43b8234d4b9c00):**
[GameSession](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) owns the current session and
existing read-only queries; [ExplorationState](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
holds the current definition/layout/player/party, and session Story owns progress flags.
[MapTransfer](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) selects setups from
those flags and validates actual entry. The existing
[field-menu integration](../../game/src/Exploration/ExplorationSessionView.Menu.cs) checks
`CanWaitAtInput`, hides on invalidation and returns Party to its original menu.
[InputSettings](../../game/src/Input/InputSettings.cs),
[SettingsStore](../../game/src/Input/SettingsStore.cs) and
[SettingsHost](../../game/src/Entry/SettingsHost.cs) provide draft/Apply, atomic revision-checked
preference persistence, failure retention and read-only launch override. No general objective,
disclosure or destination query exists in this inspected baseline.

Add a small read-only Application query beside the existing field queries, consuming the current
session revision, phase, Story flags, party/follower state and admitted content. Return either no
coverage or one objective text key, known destination label and optional **current-map** next-step
anchor/precision plus an availability reason. Final destination and immediate leg are distinct
fields; a remote map coordinate is never a local marker. A finite profile-owned rule table supplies
modern text keys, predicates and disclosure/anchor bindings. Keep it with the existing profile's
content ownership; do not mine disassembly events or translate dialogue into tasks at runtime.

Prefer one bounded rule/record in the existing content model over a generic quest framework or new
registry. Validate ambiguous predicates and missing map/exit/entity bindings at content admission.
Runtime refusals remain possible as setup, visibility or phase changes. The query submits no command,
advances no clock, consumes no RNG and writes no flags, party, resources, encounter state or saves.
There is no completed-task list, visited-map ledger, saved objective ID or second story authority.

Recompute on opening the gameplay menu and after committed progress/map/setup changes, usable return
and Continue. Clear old markers **before** a transfer/scene/locale switch; publish only a result for
the current session/revision/map. Do not hold guidance through the transfer fade. Use existing view
refresh/invalidation; this tiny rule set needs no cache, hashes, background poller or event platform.

Use one `Modern guidance / 现代引导` On/Off row in existing general Settings. It controls both objective
and destination information; no new toolbar toggle or fifth diamond command. Recommend On for new
preferences and an old format1 file missing the member, preserving every existing setting/binding.
Explicit Off persists across New Game, Continue and app restart; Continue never restores an older
save's preference. Default reset follows the selected On default. Draft/Back cancels; Apply publishes
only after the existing successful write. Storage conflict/failure retains the previous applied
choice. Read-only launch overrides keep their current refusal. Extend existing Draft/Publish and
validation rather than accepting arbitrary unknown JSON or rewriting the save format. Settings
remains subject to its current host entry restrictions; guidance cannot open it during battle.

### Display precision, layout and return

The menu adds a labelled **Current objective / 当前目标** section above the selected command's
existing name/reason in its adjacent reading frame. Preserve both sections; a goal never replaces
Search's unavailable reason, a cost or a required action hint. Initially focus the same diamond
command as today. Objective text is passive and supplies no new Confirm action. On overflow use
the existing reading/scroll treatment with explicit focus transfer and Back returning to the same
diamond selection; directional diamond input cannot simultaneously scroll text. Do not add a
separate task screen or leave the paragraph behind when the menu closes.

At 1920×1080, retain the 32px safe edge and 32px gap from the 288px-wide diamond. Propose a reading
frame up to 960×424 rather than squeezing the existing 640×224 command detail. Try bottom-aligned
placement; if it covers the player, move the whole diamond/detail composition to the clear upper
safe edge once on open, as the field-reading policy already recommends. Keep Menu, person, full
command names, reasons and footer disjoint. This is proposed geometry, not measured acceptance.
Use 30px body/24px secondary text, wrap full names and grow/scroll within safe bounds before any
font reduction. Larger supported windows retain capped reading width and 96px world; below-minimum
windows use existing recovery and hide guidance. No world zoom or new responsive layout system.

| Surface | Minimum useful precision and interaction |
| --- | --- |
| Overworld, menu closed | One short quiet destination/next-step label near a safe edge; at most one hollow labelled marker for a justified visible current-map anchor. For an off-camera anchor use “Next: North exit” or another authored direction label; no screen-edge compass arrow through walls or undisclosed terrain. No constant objective paragraph, minimap, route line, distance countdown or camera retargeting. |
| Town, menu closed | Prefer the relevant exit or named building before an exact doorway. A known, currently visible person may use the live entity anchor, never a stored NPC coordinate; absent/hidden/script-owned people get no marker. The pilot uses the exit, not a new Kazin quest marker. No secret room, unopened interior or undisclosed item/reward marker. |
| Local blockage, closed door or unsupported leg | Distinguish known destination from “Route not available” or a justified “Entrance unavailable.” Remove a claimed next-step marker when its binding/approach fails. Existing collision/warp legality remains authoritative; an anchor is orientation, not a shortest-path or guaranteed-reach claim. No full search is added to prove access. |
| Unknown location / uncovered objective | “Location not known” only when that knowledge distinction is actually supplied; otherwise omit destination and use the coverage fallback. Never expose internal map IDs, flag names or “Unknown” as unexplained player jargon. Off hides fallback text as well. |
| Gameplay menu, Party, Search result, dialogue, service, host modal, battle | Hide world labels/markers while another reading/input owner is open. Only the root gameplay menu displays its objective section. Close Party to the same diamond; Search retains its actual result/return; host Back/Resume retains its original caller. Requery on field return; release/neutral guards prevent movement or confirmation from the closing gesture. |

Markers use a hollow shape plus a localized label, not color alone, and never look like gold
selection, a quest reward or a clickable movement destination. They ignore pointer input and do not
reveal terrain, pan the camera or move the actor. All goal/direction information must also be
readable in the menu; a marker is supplementary. This takes no dependency on #1543's battle overview
or a new town/world map. Ordinary control with guidance Off must still complete the same chapter.

Use existing en/zh-CN catalogs and localization/name projection. Proposed copy includes
`Current objective / 当前目标`, `Visit Hawel with Kazin. / 和 Kazin 一起拜访 Hawel。`,
`Hawel's house · outside Yeel / Hawel 的家 · Yeel 村外`,
`Next: North exit / 下一步：北侧出口`, and `Route not available / 暂无可用路线`.
These are new modern UI strings, not translated ROM dialogue or newly approved Chinese proper names.
Keep canonical/custom names under the existing naming policy; do not derive a name from a sprite.
Switch locale without retaining the old label or marker width. Exercise a long existing/custom name
and expanded English/Chinese copy, keyboard/pad remaps and mouse; use one currently legal action
hint and existing Help. Do not expose South/East button names or take over #1544's device work.

### Bounded implementation and human review

Each row is a later separately owned slice after design review, not permission to start code here.
Recheck accepted main and serialize shared paths with #1616, #1619 and settings/localization work.
Main-gate freezes exact paths at dispatch; the candidates below are navigation, not a parallel queue.

| Stage / dependencies | Candidate ownership | Acceptance and stopping boundary |
| --- | --- | --- |
| A · Query, preference and menu; accepted guidance/layout choices first | Application `Runtime/GameSession.cs` and a new adjacent field query; `Content/Scenarios/ExplorationDefinition.cs` and the existing private-profile content owner; adapter `Input/InputSettings.cs`, `Input/SettingsStore.cs`, `Entry/SettingsHost.cs`, `UI/SettingsView.cs`, `Exploration/FieldMenuView.cs`, `Exploration/ExplorationSessionView.Menu.cs`; existing locale catalogs | Implement only the table's goal/disclosure and settings transaction. Check before/after offer, completion/revisit, conflicting/uncovered state, On/Off/Apply/Back/failure/old preferences and menu→Party→Back. Stop with no world markers; this is partial #1578 delivery. |
| B · Town/overworld destination; same query after A | Existing field view/presentation and world projection owners, finite profile anchor bindings, existing catalogs; reuse A's query rather than another navigation authority | Yeel exit, Map66 incomplete leg, justified post-Battle03 house approach, blocked/absent anchor and transfer/return invalidation. No pointer travel, new maps or route solver. If exact precision cannot be justified, deliver truthful named-area/unavailable output and retain that limitation for review. |
| C · Integrated persistence and human review; after B | Existing campaign/save/field acceptance owners; product changes only for demonstrated defects within A/B ownership | Compare Off/On at the same finite checkpoints; actual Save/Continue only at an already eligible recipe, then walk to the sample. Confirm locale/window/controls and failure recovery. Human accepts whether the aid is useful and unobtrusive. Keep #1578 open until its remaining acceptance is met. |

Add small actual engine-behavior tests for the new query's progress/disclosure, ambiguity and read-only
invariants. Run affected engine behavior, `uv run sf2 verify engine` where those semantics change,
`uv run sf2 verify adapter`, committed planner and actual selected CI. Preference/UI verification
uses direct existing consumers, not tests of observers, planners or reports. Later native acceptance
reads actual input/state/projection/geometry/errors through the existing owned installation/project/
debug instance, without screenshots or a new replay/comparison framework. No runtime run is needed
for this design PR.

Bound later observation to the table's finite checkpoints and one already eligible save/Continue
pair, serially; reuse retained progress inputs rather than replaying the full campaign. Budget at
most 12 short cases, 30s per live checkpoint, 100KiB/case and 2MiB total report material, with no asset
copies or per-frame world dumps. Query work is O(number of finite admitted rules + local anchor
checks), output one record; no all-map search or acquisition. Replan if reaching a checkpoint needs
new campaign admission, a new persistent knowledge ledger or an unverified route platform. Retain
existing failure records and instances; no cleanup is implied.

Human review under [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456) must decide
the default On, whether the goal/command distinction is immediately clear, whether a named exit
is enough or a marker earns its visual cost, and whether the player can recognize unavailable
routing without being spoiled. Compare the goal before/after guide conversation and after house
completion, return/Continue, English/Chinese long names and supported window geometry. Measured
layout, natural-language quality, route/anchor precision and user benefit remain **Unknown** until
those checks; the design's source and document checks cannot pass them.

## Held 1.5× acceleration (#1619)

**Proposed, not implemented.** [#1619](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1619)
requires hold Shift for temporary 1.5× in both exploration and battle; release restores 1×.
This is an intentional modern pacing feature. It is independent of original message Speed,
text suppression, Reduced Flash and the existing battle-entry skip. No persistent toggle, rules
change, universal skip or global appearance decision is included.

**High rigor for this timing boundary:** inspection found mandatory HEAL work that advances RNG,
frame-driven battle story service, independently processed camera/text, and waits requiring real
audio completion. Retiming only movement or a parent delta would leave these consumers inconsistent.
The staged contract below controls that concrete coupling; it needs no new timing framework or
research acquisition. The earlier #1543 scope remains Standard. Independent design acceptance
precedes implementation; future slices must declare serial ownership of shared input/host files.

### Inspected baseline and ownership

**Confirmed — accepted source structure at
[`a480804eb16fe34249a2c8543da62734a9c4f271`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/a480804eb16fe34249a2c8543da62734a9c4f271):**
use `git show <commit>:<repository-relative path>` to reproduce this inspection. Linked source paths
below are navigation to the same owners, not a claim that any future head has already passed.

| Existing owner | Actual behavior and consequence |
| --- | --- |
| [GameInput](../../game/src/Input/GameInput.cs), [InputSettings](../../game/src/Input/InputSettings.cs) | No acceleration action. `Prepare` rejects Shift/Ctrl/Alt/Meta as keys, rejects duplicate bindings and limits each action to 32 events. `Resolve` uses exact event matching, fresh non-echo edges and pad/axis guards; `GameplayHeld` currently excludes only Wait. Adding Shift to a settings file alone fails and does not address Shift+direction matching or choice/entry-skip held-state consumers. |
| [Exploration clock](../../game/src/Exploration/ExplorationSessionView.Clock.cs), [view](../../game/src/Exploration/ExplorationSessionView.cs), [movement hold](../../game/src/Input/ExplorationMovementHold.cs) | Ordinary 60 Hz service, max 4 steps/callback, fractional debt only; `WaitAtInput` at readiness or `AdvanceSimulation(token,1)` during motion. Successful held steps chain at completion, refused attempts back off 120 ms. Nonordinary automatic work uses the same view accumulator, currently with a 600-tick cap; text/choice Wait has its separate explicit input cadence. |
| [ExplorationPresentation](../../game/src/Exploration/ExplorationPresentation.cs) | Uses elapsed delta for modern cues and camera; bound camera, motion, fades and nod readiness can instead be logical service. Real resource readiness and finite music are separate completion conditions. Ordinary camera projection must still occur after the last service, including a menu/scene boundary. |
| [BattleSessionView](../../game/src/Battles/BattleSessionView.cs), [navigation](../../game/src/Battles/BattleSessionView.Navigation.cs) | Dispatches one active consumer per callback. Movement/appearance/retreat/scene receive delta; program story consumes without delta; automatic `AdvanceSimulation` is callback driven. Navigation uses real 120 ms deadlines and actual movement completion; actions/Confirm remain edge-only. |
| [BattleSceneView](../../game/src/Battles/BattleSceneView.cs), [BattleStoryView](../../game/src/Battles/BattleStoryView.cs), [BattleMapViewport](../../game/src/Battles/BattleMapViewport.cs) | HEAL logical work can advance once per Consume; story TickWait once per callback. Story reveal and camera have their own `_Process` callbacks. Scaling only the parent's delta misses these. [HealingSceneCursor.Advance](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) advances fairy work and shared seed. [GameSession](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) rejects multi-tick ordinary battle commands: keep one existing legal command per opportunity. |
| [SessionAudio](../../game/src/Audio/SessionAudio.cs), [BattleRetreatView](../../game/src/Battles/BattleRetreatView.cs) | Audio has actual playback/history/generation and pause ownership. Egress Effect requires both its elapsed minimum and the real cue ending. `ExplorationPresentation` checks finite music identity/completion; elapsed time alone cannot satisfy it. |
| [GameRoot](../../game/src/GameRoot.cs), [WindowSupportHost](../../game/src/Entry/WindowSupportHost.cs) | Host modals disable gameplay views and pause session audio; `DisarmInput` and local focus/hide hooks protect resume. Acceleration must join those paths, including views disabled before another `_Process` can run. |

Keep [ordinary exploration time/NPC/RNG](../application/session-and-programs.md#ordinary-exploration-clock-and-save-boundary),
[successful-step continuity](../verification/field-programs.md#successful-step-continuity-1585) and
[battle hold/mode isolation](presentation.md#battle-directional-hold-and-mode-isolation-1615)
as the 1× behavioral baseline. These are remake contracts. Original source provenance and its timing
Unknowns remain in those owners and the [battle source trace](battle-interaction-baseline.md#evidence-and-reproduction-boundary).
No new original-game fidelity claim follows from acceleration.

### One rate, explicit domains

Recommend one transient held-rate decision in `GameInput`, sampled by the active view, with each
view retaining its existing elapsed/debt responsibility. The inactive/prepared/retiring view gets
no budget. Do not use `Engine.TimeScale`, multiply actor speeds, alter movement costs, change RNG,
or inject key/Confirm/Wait events. No save field, new persistent setting or second simulation clock.

| Domain | Proposed budget and boundary |
| --- | --- |
| Ordinary exploration | Feed `wallDelta × rate` into the existing 60 Hz clock. Service player motion, ordinary NPC actions/random walking, followers, standing/walking animation and bound view/scroll in their existing order. NPC time also accelerates while the player stands still with Shift held. The player alone never outruns the world clock. Preserve source per-tick motion speeds and animation increments. |
| Autonomous field/story work | Scale existing TickWait/entity/nod/fade/view logical service and modern timed cues together. Keep program PC, callers, flag/payment/effect order, exact wait tokens and real asset readiness. Check the current owner after every result. This includes admitted automatic battle-entry/return work, but does not request entry skip or omit instructions. |
| Battle | Scale player/automatic board movement, appearances, attack/spell/HEAL/death/status presentation, required automatic waits, round programs and automatic action continuation. Turn legality, AI decisions, turn order, ranges and costs stay in Application/Domain. A ready player selection never gains automatic actions. |
| Reading and decisions | Keep text reveal at the chosen real characters/s, reading/scrolling, explicit dialogue/choice polls, Confirm/Cancel/shortcuts and menu/list/target navigation at 1×. Shift supplies no acknowledgement, choice, timed-input acceptance or text-suppression request. Required autonomous text work still executes; delivery and input gates retain their separate conditions. |
| Camera | Gameplay travel/cue camera follows the same budget as its subject; bound camera readiness still comes from engine service. Inspection cursor, menu focus effects and pointer/UI animation use real time. Route independent camera `_Process` advancement through the owning view's budget; never advance the same WorldViewport twice. |
| Audio | Keep sample rate, pitch and music playback rate unchanged. Cues still start once at their existing semantic events; their wall-time spacing can shorten. Existing fade envelopes may follow the accelerated visual fade, retaining cue/history order. A finite music/cue completion remains real; no synthetic Finished, generation replacement or skip disposition. |

Battle needs a small scheduling correction before the feature can cover all these domains.
Reuse the exploration policy locally: 60 Hz opportunities, cap 4 per callback, drop excess whole
debt, no background timer. One scaled budget supplies both timed visual progress and mandatory
logical service, once. Split reading from automatic consumption where a consumer currently mixes
them. HEAL mandatory work, story TickWait and automatic continuation receive single legal service
opportunities, not `Ticks > 1` or repeated player input. Stop a callback at a new wait/phase/input
owner and discard its unused budget; re-read current state before further commands. Actual mount/
draw acknowledgements still require their real frame. Do not loop through newly mounted phases
using the old delta, or apply 1.5 again inside a child already given scaled time.

This makes battle logical service independent of rendering frequency. At 60 FPS, 1× retains the
existing one-opportunity cadence; at 30/120 FPS it replaces callback-dependent logical timing with
the same 60 opportunities/s. That deliberate correction must preserve matched-work endpoints and
1× control behavior before enabling 1.5×. It does not claim unchanged historical wall-time traces.
Ordinary field and automatic script scheduling retain separate eligibility in the same accumulator;
use the same bounded cap for the touched automatic path rather than carrying its 600-tick burst.
Mandatory work is delayed, never discarded: only unused host time is dropped.

Only committed battle Movement repetition shares the accelerated travel cadence (including its
120 ms navigation delay measured in gameplay time); list/target/inspection repeats stay real 120 ms.
Exploration successful steps still chain on completion without a new deadline; refused attempts
keep real 120 ms backoff. Releasing a direction, late turning, rejection and mode changes retain
their established guards. Holding Shift alone cannot submit a movement command.

### Shift admission and lifetime

Add a semantic `Accelerate` hold to `GameInput`, default logical Shift (both physical Shift keys).
Allow Shift only for this action; retain the ban on modifier chords and Ctrl/Alt/Meta bindings.
For ordinary gameplay key matching, ignore the acceleration Shift bit on a copy of the event;
preserve other modifier checks and the real key/pressed/echo identity. Pressing Shift after a
direction or a direction after Shift must both work; releasing Shift cannot clear the direction.
Shift+Confirm remains one ordinary fresh Confirm. Acceleration edges update held state without
returning a navigation/action command, so Shift press/release cannot clear an already held direction.
Never mutate the received event or relax all bindings to nonexact matching as a shortcut. Check actual Godot matching in the adapter witness.

Extend existing binding preparation/help rather than adding a parallel polling key table. Old
format 1 preferences with no Accelerate entry get Shift without changing their custom controls.
The new action is keyboard-accessible by default; explicitly permit its pad arrays to be empty,
since current defaults already occupy every named pad button. Optional user-provided free pad/
signed-axis bindings use the same uniqueness, 32-event limit and release/neutral guards. No default
button is stolen and no Controls editor expansion is required. Exclude Accelerate from
`GameplayHeld`, choice-entry raw-held input and entry-skip admission, while observing its own
aggregate held/release state. Multiple bindings retain 1.5× until all are released.

| Transition | Rate, input and debt disposition |
| --- | --- |
| Fresh Shift press in eligible focused gameplay | Arm 1.5×; preserve the in-flight move/phase's progress and token. Change only future elapsed budget; clear fractional host debt at the edge. Echo/repeated pad packets cannot re-arm or submit actions. |
| Last acceleration binding released | Restore 1× immediately for subsequent service, discard debt; finish current motion/phase naturally. Another still-held Shift/pad binding retains1.5×. Direction can remain held and continue at 1×. |
| Automatic wait/phase change within the same sequence | Keep a valid held request; discard old-consumer time. Preserve mandatory completion order, and do not credit the new consumer with old elapsed time. |
| Dialogue/choice/result acknowledgement, service, Party, field Menu or battle inspection | Disarm acceleration and discard debt when the actual input boundary is reached. Mandatory autonomous work before that boundary still uses the active rate even if text is already visible; reveal stays 1×. After leaving require release/neutral and a new Shift edge. A battle command menu is an ordinary player selection, not a paused host modal: Shift may be armed there, but cannot speed its navigation or commit an action. |
| Host Menu/Settings/Speed/Save/Quit, menu-entry settling | Disarm before pausing or settling; settling uses ordinary 1× and its existing 120-step budget. Cancel/return preserves reached gameplay state, clears debt and requires fresh activation. Do not restart Save settlement at 1.5×. |
| Focus loss, hide, window recovery, pause/unpause, device removal or remap Apply | Disarm synchronously through existing host/local hooks; clear debt and held activation. Observe aggregate release/neutral before a new non-echo edge; regain-focus polling must not re-arm a still-held key or count elapsed background time. |
| Map transfer, field↔battle ownership handoff, New Game/Continue, stopped failure or detach | Disarm old and new owner; no carried budget or held action. Ordinary actor progress and real completion tokens survive where their own contract permits. Failures stay stopped with the reached committed prefix. |

Use an explicit acceleration release latch distinct from the general action release barrier:
otherwise excluding Shift from `GameplayHeld` would allow stale holds to resume after a modal.
Continue always starts at 1×; rate/host fractions are disposable and never alter save eligibility.
The existing entry-skip request takes precedence while active and disarms held acceleration; do
not multiply its bounded fast-forward work or change its audio delivery dispositions.

### Message Speed 1–4 is a different control

The existing [Speed transaction](presentation.md#battle-speed-and-selective-messages-1650) already
implements source selector 0–3 with modern reveal presets 15/30/60 cps and instant. The four levels
sometimes called “Speed 1–4” are currently labelled 0–3 by `SettingsView` and both locale catalogs;
this feature does not rename them. Custom cps, selective ordinary hit/coin suppression and Apply/Back
persistence remain owned there. Those presets are not simulation multipliers and their mapping is not original VInt
equivalence. Held 1.5× neither edits nor multiplies them: the first level (label 0) at 15 cps still
reveals at 15 cps; the fourth (label 3) remains instant and still waits for any required fresh
acknowledgement. Source textSettings and mandatory script/HEAL work are not rewritten to a different messageSpeed to accelerate them.
General Settings remains unavailable during battle; the accepted inspection → Options → Speed
caller pauses and disarms acceleration like its current host modal. Use existing Controls help
for the Shift hold and release/reset rule; no new permanent toolbar, visual language or settings row.

### Determinism, rhythm and staged implementation

For the same initial content/state/seeds and legal actions at the same logical service boundaries,
compare positions/collision, NPC/follower state, program PCs/waits/flags, action order/targets,
HP/MP/status/inventory/gold/EXP and main/thinking seeds. Keep every mandatory service and its RNG
draw in order, including HEAL fairy work and accepting-input-before-wait semantics. Acceleration
adds no draw per action and changes no damage/resource rule. Equal real hold durations intentionally
travel farther and give NPCs more simulation opportunities; they can therefore produce different
seeds, collisions and later outcomes. Actions alone without their intervening time are insufficient
for a determinism claim. Do not freeze NPCs, reseed, or split their RNG to hide this distinction.

**Inferred design target:** eligible uninterrupted time takes approximately two-thirds of its 1×
wall duration. For example, a modern 0.20 s battle step targets 0.133 s; an authored 60-tick autonomous
wait targets 0.667 s. Whole encounters include reading, input, mount frames and real audio, so need
not meet that ratio. At 30/60/120 FPS, compare equal logical endpoints and allow one callback plus
one 60 Hz step of quantization per retained consumer boundary. A stall deliberately slows progress
after the cap; it never yields a compensating burst. Original timing and human comfort stay Unknown.

Each future row is a separately reviewed slice, not authorization to edit all listed files now.
Path prefixes in backticks are repository-relative and exact; proposed new files are marked new.
Application/Domain/Content remain read-only dependencies unless a concrete semantic failure is
demonstrated and separately scoped. Implementation must update its existing acceptance owners.

| Slice and dependency | Exact code paths | Acceptance and stopping boundary |
| --- | --- | --- |
| A · Shared input and exploration, after design acceptance | `remake/game/src/Input/GameInput.cs`, `remake/game/src/Input/InputSettings.cs`, `remake/game/src/Input/ExplorationMovementHold.cs`, `remake/game/src/Exploration/ExplorationSessionView.cs`, `remake/game/src/Exploration/ExplorationSessionView.Clock.cs`, `remake/game/src/Exploration/ExplorationPresentation.cs`, `remake/game/src/GameRoot.cs`, `remake/game/src/Entry/WindowSupportHost.cs`, `remake/game/src/Entry/SettingsHost.cs`; help `remake/game/src/UI/ControlsHelpView.cs`, `remake/game/src/Localization/GameText.cs`, `remake/game/localization/en.po`, `remake/game/localization/zh_CN.po` | Actual field movement/standing NPC/shared seed, autonomous wait/camera/fade, Shift event order, release and modal/transfer reset. Old binding files and custom pad controls still load. Keep battle acceleration disabled until B; A alone does not satisfy #1619. |
| B · Complete battle clock, after A and its 1× review | `remake/game/src/Battles/BattleSessionView.cs`, `remake/game/src/Battles/BattleSessionView.Navigation.cs`, new `remake/game/src/Battles/BattleSessionView.Clock.cs`, `remake/game/src/Battles/BattleSceneView.cs`, `remake/game/src/Battles/BattleStoryView.cs`, `remake/game/src/Battles/BattleMapViewport.cs`, `remake/game/src/Battles/BattleRetreatView.cs`, `remake/game/src/Battles/BattleAppearanceView.cs`, `remake/game/src/Battles/BattleGiveView.cs`, `remake/game/src/Battles/BattleDiscardView.cs`, `remake/game/src/Battles/BattleSearchView.cs` | Route mixed and independent callbacks through one active battle budget; keep transaction/result reveal unscaled. First prove 1× results/control and 30/60/120 FPS logical scheduling, then 1.5× movement/automatic attack/HEAL/story/death/return and no double delivery. New partial holds battle scheduling, not a generic scheduler. Stop with both domains delivered; no further campaign admission. |
| C · Integrated finite acceptance, after B | Existing consumers above; `remake/game/src/Audio/SessionAudio.cs`, `remake/game/src/Audio/SessionAudio.EntrySkip.cs` and `remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationMusicRunner.cs` are inspection dependencies, not planned rewrites. Behavior assertions, only if engine semantics change: `remake/tests/Sf2.Remake.Engine.Tests/OrdinaryFieldTimeTests.cs`, `remake/tests/Sf2.Remake.Engine.Tests/ExplorationSessionTests.OrdinaryTime.cs` | Connected field→Battle01→field reset; Speed 1–4/custom and suppression unchanged; finite music/Egress cue ends naturally at normal pitch, pause/resume and entry skip remain separate. Read actual state/input/projection/errors. No all-route replay or new audio engine. |

The help-file names and every existing candidate must be checked again against accepted main at
dispatch; serialize these paths with ongoing input, Speed and #1616 composition work. Keep new/
extracted source below 1,000 lines; extract only the touched timing responsibility if needed.

Use the existing [ordinary-clock native recipe](../development-and-verification.md#native-reproduction),
the [field continuity witness](../verification/field-programs.md#successful-step-continuity-1585),
and actual authored `remake/content/authored/stone-court.json` battle plus an already admitted HEAL/
story/finite-audio consumer. Add only the missing short input/transition observations to their
existing observers. If a selected example lacks a required wait, use a small ignored authored
variant using admitted instructions; do not expand original-game acquisition or import a route.
Compare matched tick/action endpoints, not snapshots at equal real seconds. Include blocked moves,
late turns, both Shift keys, Shift-before/after-direction, held Confirm, stale completion, phase
handoff, low-FPS cap, physical focus return and Save/Continue starting at 1×. Read pitch/playback and
cue completion directly; a shorter visual duration is not proof that audio finished.

Future code checks use `uv run sf2 verify adapter`, affected existing engine behavior assertions
and `uv run sf2 verify engine` when engine behavior changes, plus committed planner/selected CI.
Do not add unit tests of adapter observers, schedulers-as-test-tools or reports. A design PR uses
only direct checks below. Reuse the owned registered installation/project/debug instance for any
later actual observation; no screenshot or restart is implied by this design.

Bound the future pilot to one field segment and a finite set of battle phases, serially, at most
30 s per case. Record semantic transitions and coarse endpoint timing, not per-frame world JSON.
Budget at most 20 cases, 100 KiB/case and 2 MiB total new reports, no duplicated world/assets and no
network/model acquisition. Input/world memory stays at existing consumer scale; additional clock
state is O(1), service work is O(due opportunities × existing step cost), capped 4 per callback.
Stop/replan if a case needs more time/records, any output exceeds its cap, or a required seam cannot
be reached with admitted content; missing observations remain explicit. The pilot does not authorize
cleanup of retained evidence or another instance. Subjective comfort, exact original natural timing,
all-device behavior and existing unrelated shutdown warnings remain **Unknown**. No observation or
implementation acceptance in this section has yet passed.

## Design-task verification

Design tasks #1578 and #1619 change only this owner; source, runtime and other design owners are read-only.
Acceptance is source-chain review, local Markdown target/anchor and scope checks, `git diff --check`,
the clean committed `uv run sf2 verify plan --base origin/main --head HEAD`, and actual scoped CI.
Load the owning ignored input configuration for the planner as required by the repository. No SDK,
Godot, emulator, runtime evidence acquisition or new fixtures are required. Exact head, planner
selection, CI conclusions and stopped-writer handoff belong in the Draft PR/Issue, not a rolling
history here. All report material remains below the 2MiB limit and excludes private content/paths.
