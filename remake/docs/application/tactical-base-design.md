# New Granseal Tactical Base visit

[Issue #1898](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1898) owns this design
under the [campaign agreement](remaining-campaign-risk-baseline.md). Rigor is **Standard** for the
design because map entry, the live roster, two shared item receipts and Continue meet at the same
boundary. The proposed player outcome is an optional visit from the late New Granseal castle,
conversations with the current force, ordinary field preparation and Search, and a return to a
usable castle save. The current publication still stops at the entrance. The campaign remains
at the Pacalon/Battle26 boundary.

## Design owners and evidence

Use the L2 [map interaction](../../../docs/design/synthesis/themes/map-design-principles.md#action-and-interaction),
[return and persistence](../../../docs/design/synthesis/themes/map-design-principles.md#return-and-persistence)
and [service exchanges](../../../docs/design/synthesis/themes/progression-and-economy.md#source-static-service-exchanges)
from the [design map](../../../docs/design/design-map.md). L3 decisions come from the
[map exploration contract](../../../docs/design/contracts/world/map-exploration.md),
[Caravan service contract](../../../docs/design/contracts/party/service-interactions.md#caravan-source-boundary),
[campaign Search](campaign-field-search.md) and
[save and restore policy](opening-session-lifecycle.md#eligibility-and-restoration-recipes).

**Confirmed** below means static for original behavior unless the observation explicitly says
otherwise. The registered USA retail ROM is identified by the
[existing quote evidence](../../../docs/research/world/map-event-tactical-base-quote-state.md#provenance-and-reproduction).
The source column refers to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Read each named file with `git -C <registered-source> show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<file>`.
The quote evidence pins caller and callee shapes to H1 and ROM. This design adds a direct source
inspection and an accepted-remake code inspection.

| Original boundary | Confirmed source behavior |
| --- | --- |
| Entrance and exit | `data/maps/entries/map07/6-warp-events.asm` row 0 requests a no-scroll transfer from (19,2) to (19,22) Up. `MAP_TACTICAL_BASE` is the enum alias for Map46. There is no story flag on the warp row. Map46's sole warp is (19,23) to `MAP_NEW_GRANSEAL_CASTLE`, Map7 (19,3) Down. Neither target is replaced by `table_FlagSwitchedMaps`. The separate Map7 south edge still exits to town. The symbolic/numeric warp scan found only this source entrance to Map46. |
| Initial load | `data/maps/mapsetups.asm` selects Map46's single `ms_map46` pointer table. `ExplorationLoop` in `code/gameflow/exploration/explorationfunctions_2.asm` allocates entities, loads their sprites, clears setup temporary flags, loads the map and calls init before ordinary control. `InitializeFollowerEntities` returns early for Map46. `map46/3-flag-events.asm` applies the initial block copy when F0 is set. Both maps share palette and tileset identifiers, while Map46 owns its own layout, working block copy and area. Return is a fresh Map7 entry using current flags, including the F805 setup and independent Astral follower selection. |
| Population and appearance | Map46 has 29 ally records in member index order and one fixed Astral resident (event 128), in addition to the player allocated by the loader. `ms_map46_InitFunction` tail-jumps to `InitializeTacticalBaseForceMembers` in `code/common/scripting/map/headquartersfunctions.asm`. Unjoined members are moved offscreen and idled. Joined reserves keep their authored placement and ambient behavior. Joined active allies other than the player take successive active positions in ascending ally order, face Down and idle. Death does not change the placement branch. The ordinary loader and active adjustment use `GetAllyMapsprite` with current class and HP; in field a dead ally uses sprite 190. |
| Talk | Normal `ProcessPlayerAction` reaches `GetActivatedEntity` and `RunMapSetupEntityEvent` for the faced resident. The common wrapper selects portrait and speech from the actor, subject to their available binding. Map46 events 0–28 tail-call `DisplayTacticalBaseQuote` with the ally ID. It opens the ally name window, chooses ID 1 at HP zero, otherwise `0xDC3 + ally` when active or `0xDE1 + ally` in reserve, displays the line and closes the name window. The name-window helper styles dead names and looks up the actual combatant name. Astral event 29 displays ID 11 and returns. The dynamic name window is a separate operation from the common portrait wrapper. [Quote evidence](../../../docs/research/world/map-event-tactical-base-quote-state.md) |
| Formation, Depot and Item | `map46/mapsetups/s2_entityevents.asm` and the default zone/item handlers contain no `j_CaravanMenu` caller. The shared C-button Caravan path in `code/gameflow/exploration/explorationvints.asm` requires the second entity to be a co-located Caravan. Map46 suppresses that follower along with the rest. An ordinary A-button uses `FieldMenu` and its normal Member/Magic/Item/Search behavior. Astral does not open Formation, Depot, Church, Shop, or an Item service. Existing Caravan callers outside the base still own Join/Purge, Depot and Caravan Item. [Caravan ownership](caravan-services-design.md#player-behavior-and-state-transitions) |
| Search and custody | Map46's two loose item rows in `8-other-items.asm` share receipts with Map7. Cheerful Bread (13) uses F142 in both maps, and Warrior Pride (119) uses F143 despite different placements. Source aliases in the two tables use the same flags and item operands. F142 is a pickup receipt, irrespective of the source comment. A successful claim in either scene sets the receipt read by both. Map46 has no chest or scripted item reward and the default description returns. [Original Search](../../../docs/research/world/field-search-control.md) |
| Music and saving | `map46/2-areas.asm` binds `MUSIC_HEADQUARTERS` (39), while Map7's area uses `MUSIC_CASTLE` (38). The exploration loop owns the palette-sensitive default music/fade path. `data/maps/global/savepointmapcoords.asm` has neither a Map46 savepoint nor a Map7 savepoint. The ordinary source church anchor for New Granseal is in town, Map6. The Map46 exit row does not write an EGRESS anchor. |
| Shared entry behavior | The no-scroll warp returns to MainLoop. Normal exploration entry then calls `HealLivingAndImmortalAllies`, which refreshes current HP/MP for living members and for Peter/Lemon, retaining Stun/Poison/Curse. This is a shared original entry rule. The accepted remake's ordinary walking rebuild carries party resources through. A local visit would use that existing remake policy. A decision to change shared recovery belongs at the map-transfer owner. [Current MapTransfer](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) |

**Unknown:** original natural reach and path timing for this castle visit, ordinary allocation and
quote presentation, both item claims in live play, original save/reload outcomes across the visit,
and human audiovisual quality. Existing controlled evidence establishes the documented helper
boundaries. A play run of the remake would establish only its own behavior.

## Current consumer gaps

- **Confirmed in accepted remake code.** The
  [private selection](../../content/private/pre-voyage-opening.json) has Map7 and omits Map46.
  [Warp lowering](../../../src/sf2tool/remake_exploration_maps.py) retains out-of-package targets as
  a frontier, which explains the present doorway. Selecting Map46 alone would also enable the
  source row in earlier Map7 setups. The proposal below keeps the first publication scoped to the
  late return.
- **Confirmed in accepted remake code.** `MapTransfer.Build` selects setup and flag layout before
  [SceneEntities.Build](../../src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)
  allocates party and residents, then normal transfer runs the map's admitted init. Existing
  population code resolves aliases and class sprites, but does not filter or place a headquarters
  cast, and does not apply the dead-sprite rule to this ordinary allocation. The generic allocator
  keeps source member ordering. Hiding an unjoined ally must also exclude collision, Talk and
  ambient work. The type needs current party and membership, so a fixed cast baked from one save
  would yield incorrect placements for other legal parties.
- **Confirmed in accepted remake code.** The
  [program compiler](../../../src/sf2tool/remake_exploration_programs.py) preserves unsupported
  operations as stops. The dedicated native init and quote functions need bounded bindings.
  Existing portrait, speaker, text, pause and localization behavior should consume a current ally
  identity and a selected numeric line. Audit name display and focus through the ordinary adapter.
  Check class sprites, flame, actor-specific speech and portrait bindings, and both catalogs for
  every admitted current member's potentially reached quote IDs. Astral's own line is independent.
- **Confirmed in accepted remake code.** The private selection already binds the shared
  Headquarters loop at command 39, timer B 195. The destination still needs an area-to-cue mapping,
  scene resources and the corresponding layout/foreground references. Use the existing
  [audio runner](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationMusicRunner.cs)
  and host cue selection. Check command 38 on return, including the case where the maps share
  palette identifiers. Audit mounted media before budgeting additional rasters or sound.
- **Confirmed in accepted remake code.** [Field Save](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs)
  checks settled input, windows, movement and restorability.
  [Sf2OpeningRestore](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs)
  already selects the F802 `new-granseal-return` recipe for Map7, including a finite scene record
  for its live or fresh cast. It has no Map46 recipe, and ordinary restore does not execute an
  entry program. Publishing in-base Save would therefore require a deliberate recipe and
  reconstruction decision. The Map7 return can use the existing scene and compatibility owners.

## Implementation proposal

**Inferred** from the source and consumer checks. Use one optional visit slice after this design is
accepted. Admit the Map7 doorway for the F802 return context, including both the live and fresh
castle populations. F802 is a first-publication boundary in the remake. The source row itself is
unconditional. Retain explicit frontier behavior for earlier Map7 visits, and keep unrelated
destinations outside the publication. No F805, gift, recruitment or battle-completion requirement
is needed for the doorway.

- Use the existing no-scroll rebuild and ordinary input routes both ways. Add the smallest typed
  population setup and quote operations that express the actual inputs. Keep party state
  authoritative, and apply seat order only to joined active members other than the player.
  Preserve original slot/alias identity where used by the caller. Bind the supported current
  ensemble and its legal class variations through the existing shared appearance owners.
  Data from later members may remain dormant. A legal joined member requires a real binding and
  explicit admission, including reserve and defeated variants.
- Support ally Talk, Astral Talk and both shared-receipt Search rows. Reuse the ordinary field Item
  operations and their current effects and failures. A player who wants to swap the deployed and
  reserve members, deposit or withdraw, or use Caravan Item leaves the base and chooses an existing
  Caravan caller. Re-entry derives the new placements and quotes from the changed active flags.
  Persist shared item custody through existing services and saves. The promotion effect of
  Warrior Pride stays with its own church/class owner.
- Return to the castle through the south step, settle control and Save there. The first slice leaves
  in-base Save unavailable. If local Save inside the headquarters is
  desired, add it as a dependent design/implementation slice. Give it a finite settled recipe,
  use the same placement rules to reconstruct the cast and reuse existing scene persistence if
  positional state cannot be safely recomputed. Preserve the carried EGRESS anchor. Validate
  recipe, map, walkable landing, appearance, active membership and music. No mid-dialogue save,
  original SRAM import or separate save format is warranted by the first visit.

A door-only slice would admit the wrong population and reached callbacks. Opening a Caravan menu
at Astral would invent a service and add a live formation-refresh problem. A separate generic
party-service system would duplicate existing formation and custody rules. The single visit slice
covers the connected route, and can depend on separately accepted appearance bindings if the
current legal cast exposes one. Coordinate any shared content/engine/campaign-index edits with
main-gate before claiming them. [Engine boundary](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#typed-content-and-capability-admission)

## Acceptance for the implementation slice

1. For each language, prefer an untouched copy of the settled #1896 fresh F805 castle slot where
   retained. Validate its map, F802 and recipe through ordinary Continue. If that language has only
   the retained #1896 Map73 exit slot, walk back through Map6 to Map7 from that compatible seed. Use the audience/live variant in a separate copy. No Pacalon backtrack,
   flag edit or synthetic roster is needed for ordinary acceptance. A pre-F802 castle seed from
   the [year-later route](new-granseal-east-shrine.md) covers the scoped frontier and normal exit.
2. From a walked approach, check the effective warp marker, traversal, landing, facing, active
   area, settled control, ordered roster, sprite identity, absence of followers and area cue.
   Validate layout/marker and collision for the doorway, the inward approach, the two loose items
   and the walk back onto the exit. Observe a deployed and a reserve ally and Astral using normal
   input, with their expected numeric lines, language, focus and close/return. Cover defeated
   non-immortal and unjoined cases with actual engine behavior tests if the ordinary seed lacks
   them. Check a second legal formation or party size so seats and quotes cannot be fixed to a
   single roster. Verify no root Caravan wait opens here.
3. Claim at least one still-available receipt at either location and check its empty counterpart
   after transfer. Use a sibling copy for opposite claim order, the other receipt or full packs as
   needed. Check receipt and custody publication together. Use an admitted Field Item action or
   cancel back to walking. For a formation witness, change one member through the existing
   external Caravan, revisit and check placement and the new quote branch. Depot regressions
   check state continuity with an existing nonempty depot when available.
4. Walk the actual exit to Map7 (19,3) Down. Compare the fresh castle setup, F70 follower relation,
   cue, current party resources and custody, durable flag changes, egress town and berth. Account
   for normal step counter, ambient motion and RNG changes across play. Save after control settles.
   Cold Continue in a separate ordinary launch and compare decoded saved progress, scene identity,
   restored audio cue and control. Exercise the old castle/exit seeds and one unchanged current
   Pacalon seed through their own Continue entry. The original seed bytes remain intact.
5. Use small engine behavior checks and the actual adapter build only for the implementation.
   Observe ordinary Godot input, state, projection and errors, including successful save
   publication before exit. Record any completed shutdown failure separately under
   [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716). No screenshots or
   new capture platform are needed.

Use the stable [#1896 handoff](new-granseal-return.md#reproduction-gates-and-retained-limits)
and the current [Pacalon inputs](pacalon-first-visit.md#reproduction) read-only.
The design needs only fixed source reads, existing evidence and document checks. For implementation,
plan one destination and its referenced resources, two language copies and bounded state/receipt
observations. One 64×64 16-bit working layout is 8 KiB raw. A full RGBA layer at 24 px per block
would be 9 MiB before compression. Budget up to 32 MiB incremental private content and 4 MiB for
copied slots and compact receipts initially, assuming the existing cue and most actor assets
are mounted. Measure the actual referenced additions and available disk headroom before a candidate
package build. Reuse the registered base bundle, existing producers and the reviewed audio source.
Keep candidate outputs in the owned ignored location. Local source-derived content and receipts
follow the [private input policy](../../../docs/operations/local-private-inputs.md). Any public
implementation requires separately authorized assets and presentation.
