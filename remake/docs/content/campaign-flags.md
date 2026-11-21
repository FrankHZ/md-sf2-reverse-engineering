# Campaign flags from New Game through first Pacalon

**Standard rigor.** This is a developer lookup for the connected campaign and its alternative
callers. It joins several state lifecycles across chapters, so the index records the scope,
producer, consumer and evidence boundary together. The [private campaign selection](../../content/private/pre-voyage-opening.json)
defines the map defaults, admitted setup variants, event and Search selections and Battles01–26.
The [feature trace](../capability-status.md#campaign-feature-trace) and [campaign agreement](../application/remaining-campaign-risk-baseline.md)
define playable acceptance. The endpoint is drought-state Pacalon town and castle, then settled
Map71 on departure before Battle27. The index includes optional and refusal branches, retained
unavailable callers and later guards read at that boundary. Later chapters, debug routes and the
full game-wide flag space are outside the inventory. Older chapter observations retain their
individual start and end conditions.

## Look up a chapter or number

All IDs are decimal. F followed by a number means an original global flag. A range covers only
the stated family or the IDs named in its linked table. For a shared flag with several consumers,
follow the family first, then the chapter. In each chapter, source columns describe the original
US baseline. The remake column distinguishes compiled or controlled coverage from observed play.
**Confirmed** identifies what the linked evidence establishes at its own boundary. **Inferred**
identifies a source-supported interpretation without independent reproduction at that boundary.
**Unknown** identifies an unresolved claim.

| Decimal ID or range | Lookup |
| --- | --- |
| F0–29, F32–61 | [Joined and active families](#joined-active-and-following). F22 also selects [late Creed](campaign-flags-creed.md#creed-choices-and-departure) and affects [Pacalon Castle](campaign-flags-north.md#ketto-and-pacalon). |
| F64–77, F82–84 | [Transport and followers](#joined-active-and-following). See chapter rows for the writers. |
| F78–80, F88–105, F399 | [System and battle-local controls](#system-and-battle-local-controls). |
| F128–132, F134–168, F170–176, F188, F196–197, F200–202, F205–217, F220–221 | [Placement receipts](#pickup-receipts-and-custody). The table lists individual covered IDs and identifies later or unavailable placements. |
| F256–383 | [Visit latches](#visit-latches-and-map-entry), with chapter-local uses on each page. |
| F384–385, F600–605, F607–612, F620–624, F640, F650, F660–664, F700, F940 | [Grans opening, Galam and the voyage](campaign-flags-grans.md). |
| F401–426, F451–476, F501–526 | [Battle admission, introduction and completion](#battle-admission-introduction-and-completion). Find unlock producers in [Grans 01–07](campaign-flags-grans.md), [Parmecia 08–18](campaign-flags-parmecia.md), [Creed route 19–23](campaign-flags-creed.md) and [north 24–26](campaign-flags-north.md). F470 has an additional field writer. |
| F427–431, F438–439, F527, F530–531, F537–539, F543, F982, F999 | [Guards for later routes](#later-guards-at-the-boundary). F811 also occurs at the [Pacalon frontier](campaign-flags-north.md#ketto-and-pacalon). |
| F701–703, F705–709, F710–724, F725–732, F798–799 | [New Granseal, eastern Parmecia, the raft and Taros](campaign-flags-parmecia.md). F701/F702 have later consumers on the [north page](campaign-flags-north.md#new-granseal-and-the-north-road). |
| F751, F762, F774, F776–781, F783–787 | [Caravan, Elric and Creed](campaign-flags-creed.md). F762 and F785 also have [mine consumers](campaign-flags-north.md#mine-and-cannon). |
| F750, F759–761, F800, F802–806, F810–811 | [Mine return, North Cliff, Ketto and Pacalon](campaign-flags-north.md). F802 is first written at [Creed](campaign-flags-creed.md#creed-choices-and-departure). |

The [story-content index](../../../docs/research/story-content-coverage.md) routes original chapter
findings. The [global flag contract](../../../docs/design/contracts/world/global-flag-state.md)
owns addressing and operation identity. The [direct-access fixture](../../../tests/fixtures/h2/map-events/direct-flags.json)
indexes a 151-ID corpus of direct map-event operands, with source path, program and branch
anchors. [Same-program lifecycle](../../../docs/research/world/map-event-flag-lifecycle-state.md),
[cross-program relations](../../../docs/research/world/map-event-cross-program-flag-state.md) and
[route selection](../../../docs/research/world/map-event-flag-route-selection.md) establish static
relations. This index also follows map initializers, cutscenes, setup and layout selectors, battle
tables, follower and save services, and placement receipts. A static edge alone leaves actual
caller state, execution order across programs and natural reach **Unknown**.

<a id="visit-latches-and-map-entry"></a>

## Visit latches and map entry

**Original, confirmed static.** F256–383 are visit latches. Explicit fresh exploration entry
chooses its target and setup, allocates the population with the incoming flags, clears the visit
range, and sets F80 before the map/init continuation. If several setup selectors match, the last
matching source row wins. A CURRENT255 map reload keeps the loaded population and visit flags,
and can rerun init. An overworld scroll retains them without running ordinary destination init.
Internal stairs that request CURRENT can therefore keep a resident moved on the current visit.
A later explicit entry can choose a different setup or rebuild the original placements. The
number of a visit flag alone identifies its bit, while the loaded setup and callback identify its
meaning. [Source entry sequence](../../../docs/research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md) ·
[Source CURRENT and scroll](../../../docs/research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md) ·
[Setup selectors](../../../docs/design/contracts/world/map-setup-data.md#ordered-route-serialization)

**Remake.** The [map compiler](../../../src/sf2tool/remake_exploration_maps.py) emits fresh entry
writes. [MapTransfer](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) resolves
setup and eligible battle admission from incoming flags, applies the entry writes on rebuild
and loads the layout/init. Preserve retains working layout and actors and runs the bound init.
Scroll has its own path. Script scene reloads can have narrower view/layout effects. Check the
caller before treating a visual refresh, stair or postbattle return as a fresh map. The [cliff
retry](campaign-flags-north.md#north-cliff-and-north-cave) and [Ketto/Pacalon](campaign-flags-north.md#ketto-and-pacalon)
observations exercise different reset and replay outcomes.

## Save and Continue interpretation

**Original, confirmed static and controlled.** The save service copies 4,016 logical bytes from
and to COMBATANT_DATA at $FFE800. GAME_FLAGS spans 128 bytes from $FFF686 to $FFF705, at payload
offsets 3718–3845 inclusive. SRAM slot-occupied bits are separately stored in SAVE_FLAGS. The
existing in-process story-state probe observes an original flag mutation, SaveGame, inverse RAM
poison, LoadGame and the resulting branch for selected bits, including F89. The field Load route
goes through savepoint lookup and fresh map entry, where visit clears and other writes can change
the restored values. F88 redirects Load to suspended battle. Original durable media behavior and
natural Save → Load for individual chapters remain **Unknown** unless a chapter owner records
them. [Save contract](../../../docs/design/contracts/system/save-system.md#confirmed-static-contract) ·
[Witch payload anchors](../../../tests/fixtures/h3/witch-save-actions-v1.json) ·
[Opening flag block](../../../docs/research/chapters/grans-opening/map3-admitted-start.md) ·
[Controlled flag persistence](../../../docs/research/world/common-scripting.md#confirmed-map-script-story-state-branchprompt-command-family)

**Remake design and binding.** For supported settled field recipes, [OpeningSave](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs)
captures story flags, party, custody and position. Continue uses recipe validation and applicable
scene reconstruction/restoration. It can retain visit latches and live actor/layout deltas
through a cold launch. A fresh gameplay reentry is a separate event. A field recipe can refuse an
unsettled or unsupported boundary. Current battle saves carry the story flags, battle region
state, receipt/depot state and the admitted continuation separately and resume at the saved phase.
They neither set F88 for admission nor replay the before/start/outcome programs. A decoded save
match establishes that boundary's survival, while writer order and branch reach still need their
own evidence. [Field recipes](../application/opening-session-lifecycle.md#eligibility-and-restoration-recipes) ·
[Battle restore](../application/battle-save-and-continue.md#implemented-domainapplication-boundary-a) ·
[Cross-version roster](../application/battle-save-and-continue.md#continuing-across-historical-opening-rosters)

On chapter pages, **persistent** means outside the visit clear and carried where the selected
save recipe admits the state, subject to the row's explicit writers. **Visit** means the fresh
entry rule above. A cold-save witness applies to the named scene, path and supported version only.

## Joined, active and following

| ID | Original meaning and ordering | Remake binding and evidence |
| --- | --- | --- |
| F0–29 joined, F32–61 active | Source ally index *i* uses F*i* for joined and F(32 + *i*) for active, for the 30 source records. New Game starts with F0/F32 for Bowie. Join sets membership, rebuilds the counted prefixes, then conditionally sets the active bit. The prefix can still classify the new recruit as reserve until the next rebuild. The acknowledgement and any follow tail have their own order. [Original membership chronology](../../../docs/design/contracts/party/party-membership-state.md#joinforce-chronology). Current active capacity and battle placement can change deployment. Death and AI control are separate. Formation Relief changes active bits while preserving membership. F22 also gates later Creed offers and the mapped Frayja placement in Pacalon Castle. Slots without an admitted join are family references only. | [MapPartyMembership](../../src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs), [battle join operations](../../src/Sf2.Remake.Application/Runtime/Battles/BattleStoryOperations.cs) and [historical roster validation](../application/battle-save-and-continue.md#continuing-across-historical-opening-rosters). Examples include [initial allies](campaign-flags-grans.md#opening-granseal-and-the-tower), [Creed capacity/Relief](campaign-flags-creed.md#creed-choices-and-departure) and [Higins reserve then deployment](campaign-flags-north.md#ketto-and-pacalon). |
| F64 raft, F65 Caravan | Polca's mayor sets F64 with the first berth. The Taros victory scene grants F65. F64 admits raft declaration at matching berth, influences certain retreat resets and guards field Wing. F65 controls overworld Caravan and related placement/narrative consumers. Ownership, vehicle location, walking/riding mode and physical actor/control are separate. Both ownership flags survive ordinary fresh entry. | [Raft](../application/polca-first-raft.md), [Caravan](../application/caravan-travel.md) and [Item/Depot/Relief](../application/caravan-services-design.md) bind separate state. Save/Continue preserves both ownership and admitted berth/depot data. An older battle payload can explicitly lack a berth while retaining F64. |
| F66 Sarah/Chester, F67 Peter, F68 Oddler, F69 Fairy, F70 later Astral | These select follower rows on fresh population. F66 covers two rows and is cleared by Jaha's zone caller. F67 starts after Peter's audience. Polca sets F68. Creed's departure sets F69, then clears F68. Mine recovery clears F69. The New Granseal return sets F805, then F70. B can be rebound by multiple selected rows. Physical follow instructions can precede the flag writes or survive changes to them on a current visit. | [Source follower iteration](../../../docs/research/chapters/southern-travel-creed/desktop-kingdom-first-arrival.md), [Creed scene restore](../application/creed-recruitment.md#persistence-and-continuation) and [Astral current/fresh return](../application/new-granseal-return.md#current-fresh-and-cold-state). |
| F72 Kazin, F73 Slade, F74 Astral/King/Minister, F75 Luke, F77 old man, F83 King/Minister, F84 Rohde | The earlier temporary role can exist with membership clear. House clears F72 after joining Kazin. Battle05 after clears F73 after joining Slade. Battle07 intro clears F83. First after installs the royal train with F74. Captain clears F74. Polca's departure clears F75 before Luke's separate field join. Shrine after clears F77. Hassan's choice sets F84. Taros clears it. North Cliff later joins Rohde. Several roles share logical aliases. | [Grans writers](campaign-flags-grans.md), [Parmecia writers](campaign-flags-parmecia.md) and [cliff join](campaign-flags-north.md#north-cliff-and-north-cave). Actual actors and relationships use the loaded scene. The save owner validates which scene can be reconstructed from flags and which requires its recorded state. |
| F71 Lemon, F76 Zynk, F82 Frayja | Shared population table has rows for these later followers. F82 can affect Pacalon Castle under supplied flags. F22/F54 are Frayja's membership and active flags. Normal first Pacalon has no F82-producing scene. | Original [follower table](../../../docs/research/chapters/southern-travel-creed/desktop-kingdom-first-arrival.md) and [later Pacalon return](../../../docs/research/chapters/northern-travel-mitula/pacalon-return-frayja-and-battle31-admission.md). Later natural producers and journeys remain outside the connected remake boundary. |

## Battle admission, introduction and completion

The source bases 400, 450 and 500 are established by the
[battle routing and cutscene owners](../../../docs/research/chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md)
and [common loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control). For battle
*n* from 01 to 26, read the route as follows. The remake [content compiler](../../../src/sf2tool/remake_exploration_battles.py)
binds the three IDs, [BattleEntry](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)
selects a matching route, and [BattleOutcome](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)
and [SF2 return policy](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs)
own the result.

| Decimal family | Write → read and return |
| --- | --- |
| F401–426 (400 + *n*) | The chapter producer arms admission. The ordered battle table and applicable position/zone still determine entry. An already armed route can intercept a later map entry before its field init. The original scan admits the first matching armed route even with completion held, clearing that unlock as it selects. Remake admission applies the selected replay policy, which can consume the completed unlock and proceed to field control. On ordinary first EGRESS/Wing or defeat, the incomplete unlock survives and permits another attempt when the caller/position allows. Victory clears it. The Battle04 defeat exception clears F404 and sets F504 without the normal victory scene. See the [Grans exception](campaign-flags-grans.md#yeel-hawel-and-galam). |
| F451–476 (450 + *n*) | The source before/start wrappers read the introduction latch. On fresh intro the start wrapper sets it before the selected start program. It survives retreat, so retry after the write can skip intro even with completion clear. F470 has an earlier field writer at Harpy Pond. First admission and first confrontation are separate. Some intro bodies are empty, including Battle23. A finite presentation skip in the remake still runs the required state effects. The original F88 path resumes supplied state. [Source Battle21 order](../../../docs/research/chapters/southern-travel-creed/battle21-opening-and-initial-roster.md#admission-fresh-entry-and-scene-gates) · [Remake skip](../application/session-and-programs.md#battle-entry-skip-agreement). |
| F501–526 (500 + *n*) | First victory seeds the return from the first active ally’s terminal position plus the battle origin and uses the sampled facing. It runs the eligible after-program and shared join tail, clears unlock, then sets completion before returning. Check the specific chapter for a fixed scene return or another destination. Earlier script writes and post-return init can be distinct milestones. Completed victory skips the first after-program but can retain the separate shared join tail. A completed region check can rearm admission subject to the counter, RNG and route. Upgraded enemy selection also needs the source difficulty/level conditions and admitted definitions. A retry with completion clear is still a first-completion attempt. [Battle19 first/repeat and outcome](../../../docs/research/chapters/southern-travel-creed/battle19-outcomes-through-elven-village-entry.md) · [Observed Battle19 upgrade envelope](../application/battle19-repeat.md). |

## Pickup receipts and custody

**Original.** A valid working tile routes Search to a placement, and the receipt selects its empty
or previously opened path. In the source chest path, the open flag/layout can be written before
delivery. The ordinary full-hands failure closes the chest and withdraws the receipt so the player
can retry. The Balloons and battle chest show why program order matters. **Remake design.** Field
Search publishes grant/receipt/open block together on success, or preflights a full pack and leaves
the placement available. An empty chest can still acquire its open flag. Fresh layout reconstruction
uses the receipt. Current inventory, consumed items, gold, transport deposits and Deals remain
separate. [Original field Search](../../../docs/research/world/field-search-control.md) ·
[Desktop full hands](../../../docs/research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md) ·
[FieldSearch](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldSearch.cs)

The inventory follows the chest and loose-item pointers for current Search maps, including
shared tables. It also lists source placements on campaign maps outside the Search selection.
The working tile and reachable approach determine whether an admitted placement can be used.
[Source map entries](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries.asm) ·
[Remake Search selection](../../content/private/pre-voyage-opening.json)

| IDs | Placement and evidence |
| --- | --- |
| F128–132, F134–145, F148–149, F188, F196–197, F200, F205–211, F213, F217, F220 | Map3 F128/F220 (the latter is the empty shop chest), Map19 F129–131, Map40 F132, Yeel F134/F196–197, Hawel F135–136, Galam F137–138/F200/F207–208, prison F205–206, docks/ship F139–140, New Granseal F141/F209–211 and castle F142–143, Ribble F144–145, Polca F148, Bedoe F149, Map70 F188, Map66 F213 and quake Granseal F217. The [placement/marker inventory](../application/campaign-field-search.md#map-and-setup-inventory) distinguishes compiled rows from reached rewards. Shared F139/F140 records do not create valid Map14 markers. Opening F220 is obstructed in the admitted setup. |
| F146–147, F151–152, F168 | F146 Silver Tank in the western cave. F168 Fairy Tear in northern Map48. F147 Sword and F152 Wooden Panel are different Map50 field chests. The Map50 table assigns F151 to the Power Water chest. The remake's Battle15 Search uses F151 to open its tile and activate Golem 143. The Golem’s Power Water drop has a separate custody bit. Repeat initialization can reset the hidden Golem while F151 stays held. [North Cave](../application/battle25-north-cave.md#search-and-exits) · [Sword](../application/ribble-sword-rohde.md) · [Battle chest and Panel](../application/east-shrine-panel.md#optional-battle-chest) · [Original Battle15](../../../docs/research/chapters/southern-travel-creed/battle15-shrine-approach-and-outcomes.md). |
| F150, F153–155, F201 | Original Map62 Steel Sword chest F150, Hassan loose Running Pimento F153 and Mithril F154, separate Hassan Mithril chest F201, and Taros Shrine Healing Water chest F155. Ordinary Search granted each reward. Repeat and the named carry/return routes preserve the receipts. [Search routes](../application/campaign-field-search.md#hassan-the-shrines-elven-village-and-harpy-pond) · [Source tables and chapter boundary](campaign-flags-parmecia.md#hassan-the-shrines-and-taros). |
| F156 | Map30 Dry Stone. Ordinary pickup commits F156 and awards the held item, and repeat Search after reentry gives no second grant. Later use is outside this package. [Mine evidence and future use](campaign-flags-north.md#mine-and-cannon). |
| F157–159, F202, F214, F221 | Source Map23 Fairy Powder, Secret Book, Mithril and Vigor Ball are F157, F158, F202 and F221. Map51 Healing Drop F159 and Map67 Mithril F214 are known rows. Ordinary Search obtained all five Map23/51 receipts. Map67 is selected and its ordinary F214 grant remains **Unknown** here. [Search routes](../application/campaign-field-search.md#hassan-the-shrines-elven-village-and-harpy-pond) [Village research](../../../docs/research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md) · [Current selection](../../content/private/pre-voyage-opening.json). |
| F160–167, F212 | Fairy Cave F160–163 grants in source order are Angel Wing, Brave Apple, Medical Herb and Mithril. Creed F164–167 are Healing Rain, Antidote, Medical Herb and Running Ring. Ordinary first-visit Search set F164. It survived Kazin's Battle22 Use and cold Continue. [Rain receipt and consumption](../domain/recovery-items-design.md#ordinary-input-observations). Desktop F212 is the Cotton Balloon receipt and exit guard input. Own-tile Use requires actual custody, and Floor World transfer removes an item. F212 persists. [Cave observation](../application/fairy-cave.md) · [Creed inventory](../application/creed-restoration.md#reachable-interactions-and-remaining-boundary) · [Balloon](../application/cotton-balloon-floor-world.md). |
| F170–176 | Ketto F170 Bright Honey/F171 Healing Water. Pacalon town F172 Cheerful Bread/F173 Medical Herb. Castle F174 Fairy Powder/F175 Pegasus Wing/F176 Mithril chest. F175 and the held Wing survived the ordinary HERO rite in both parties. [Class and custody](../application/church-promotion-design.md#playable-hero-and-reproduction). Ordinary collection and cold receipts are observed at the named saves. [Ketto](../application/ketto-town.md#search-and-departure) · [Pacalon](../application/pacalon-first-visit.md#church-and-continue). |
| F215–216 | F215 is the Right of Hope chest receipt and F216 the Life Ring chest receipt at the Fairy Woods trial map. Both remain outside the connected remake. Use the [trial owner](../../../docs/research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md) for their specific consumers. |
| Separate battle drop once bits | The four-byte source drop bank is separate from GAME_FLAGS. For example Battle26's Black Ring bit 10 and Luke's joined F10 coexist independently. Leader cleanup without the carrier's ordinary death gives no ring. An eligible full-pack drop can go to Deals. The once bit suppresses another receipt. Battle save and return carry once bits and Deals independently. [Original Black Ring](../../../docs/research/chapters/northern-travel-mitula/battle26-outcomes-and-higins-recruitment.md#the-independent-black-ring-drop) · [Remake custody](../application/session-and-programs.md#battle-item-custody-and-receipt-playback) · [Battle26 observation](../application/battle26-higins.md#retreat-defeat-and-victory). |

## System and battle-local controls

| ID | Producer, reader and lifetime |
| --- | --- |
| F78/F79 | New-game difficulty writes the two weighted bits (1 and 2). Battle rules read the resulting 0–3 setting. They are outside visit clears. The supported remake opening fixes both clear. Original [New action cases](../../../docs/design/contracts/system/save-system.md) and [loop interpretation](../../../docs/research/battle/battle-loop.md#top-level-battle-control) · [remake construction](../application/opening-session-lifecycle.md#new-game-construction). |
| F80 | Fresh exploration sets it with the visit reset, outside the cleared range. Shared initialization/control bit. Presence in a save does not identify a chapter. Its complete consumer inventory is **Unknown** here. [Original Map3 entry](../../../docs/research/chapters/grans-opening/map3-admitted-start.md) · [map compiler](../../../src/sf2tool/remake_exploration_maps.py). |
| F88 | Original Suspend records elapsed time, sets F88 and calls Save. Load branches to BattleLoop, which restores elapsed seconds and clears F88 and AI memory before resuming turns. Ordinary remake BattleEntry rejects this flag. Typed battle saves resume by their captured phase. [Original load routing](../../../docs/design/contracts/system/save-system.md#confirmed-in-process-runtime-matrix) · [Original suspend caller and remake battle save](../application/battle-save-and-continue.md). |
| F89 | Original script story-flow Yes/No writes F89 after the answer (set for zero/Yes, clear for No/cancel there). Later script branches can read it. Direct return-value prompts do not all write F89, including the captain. The remake binds both shapes and rejects save during pending input. A prior F89 in a settled save is not the next answer. [Source commission](../../../docs/research/chapters/grans-opening/battle01-to-battle02-content-route.md) · [Choice binding](../application/session-and-programs.md#source-bound-yesno-lifecycle). |
| F90–105 | Source battle region latches. Fresh battle clears them after Before, before roster and region evaluation. Suspended source entry uses supplied region state and has its own AI-memory clear. Remake fresh entry clears the global range on initialized battle transition and uses typed region Flags/Tested and separate story once flags. Admitted battle restore retains those rather than resampling. [Loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control) · [Battle save](../application/battle-save-and-continue.md). |
| F399 | Battle entry or successful field save sets it for normal return through the selected EGRESS_MAP. In the remake, a supported church save changes the anchor at successful publication. The clear source branch uses initial Map3 placement. Changing the bit does not identify a saved town. Cancel/refused service need not update the register. [Original save caller](../../../docs/research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md) · [Remake Ketto return](../application/ketto-town.md#residents-and-the-counters) · [Pacalon churches](../application/pacalon-first-visit.md#church-and-continue). |

## Later guards at the boundary

| ID | Scope |
| --- | --- |
| F438/F439, F537–539 | Map66’s Y9/10 callback uses F537 set and F538 clear to arm F438. Its Y28 callback uses F538 set and F539 clear to arm F439. These are original alternate branches, excluded from the connected package. [Map66 conditions and consequences](../../../docs/research/chapters/grans-opening/battle02-to-battle03-content-route.md). |
| F530, F431/F531, F427–430/F527, F811 | F530 selects the rainy Pacalon variants and Map71→78. F431 arms the Battle31 boundary and F531 is its independent completion flag. The Map74 Battle27 request and F811’s later story branch are source references beyond first Pacalon. [Pacalon boundary details](campaign-flags-north.md#ketto-and-pacalon). |
| F543, F982, F999 | Later selector inputs on the early maps. F543 can override Grans setup rows and F982 can reset the castle route to baseline. F999 can substitute Map59 for Map58 and gates later layout copies. The campaign evidence assumes the applicable late inputs clear. It supplies no accepted early producer. [Castle selectors](../../../docs/research/chapters/grans-opening/battle01-to-battle02-content-route.md) · [Map58 return](../../../docs/research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md). |
