# Optional pre-voyage objective and destination

This is the current implementation and coverage owner for
[#1578](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578).
The merged [#1705 pilot](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1706) supplied the
shared preference/query and two surfaces. The user authorized extending them, in story order,
from ordinary New Game through boarding. The original pilot’s narrower acceptance remains in its
Git object and retained observations. Human acceptance stays in #1578/#1456.

**Standard rigor:** finite content disclosure across existing Application, preferences and field UI.
Deliver a useful current objective and known destination/action during every supported main-story
free-control segment; change precision with the evidence. Include ordinary detours, defeats, EGRESS,
revisits, eligible Continue and the dock decision. Do not make optional recruitment mandatory.
Stop after boarding clears the aid; no later chapter guidance, new admission, quest history,
automatic travel, path solver, new save location or additional story/knowledge record.

The [visual language](ui-design-language.md), F7 field controls and existing Modern guidance setting
remain in force. The [design proposal](modernization-design.md#optional-objective-and-destination-guidance-1578)
records the original choices; the coverage below supersedes its pilot-only withholding boundaries
where the expanded story has already disclosed a task.

## Ownership and disclosure

`GameSession.QueryFieldGuidance` reads current session/revision/map, ordinary field phase, selected
setup, existing flags and any content-specified physical area/follower. It returns one result or
no coverage; ambiguity never selects the first rule. It changes no clock, RNG, resources, encounters,
party, flags or saves. Ordinary motion retains the aid; dialogue, automatic scenes, battle, services,
stopped scenes and host/window owners hide it. Return/Continue recompute from current state.

`PrivateFieldGuidance` owns the finite story rules, copy keys and optional current-map anchors.
The generic query contains no campaign map/flag constants. Variant setup identity is selected with
the existing admitted `MapSetupSelector`; source maps without setup selectors are supported.
Optional content areas distinguish packed home/school rooms and prison/tunnel/kitchen on the same
map. A rule may name a companion, but neither a name placeholder nor a live follower is mandatory.
F72 itself proves the completed Kazin guide disclosure after a return/load. Explicit follower
requirements remain supported for content that actually needs them.

The definition is immutable after admission and read-only to the UI. No objective ID is persisted.
Later source completion dominates earlier tasks, including on detours and EGRESS returns. F6/Kiwi
and current follower presence do not gate the castle/tower goal. Astral’s F620, royal disclosure
F621 and reached tower intro F457 independently establish tower knowledge; none becomes a new
mandatory companion or conversation gate. F74 clearing at boarding removes
the aftermath rules immediately; F700 prevents a later save/revisit reviving any earlier rule.

## Actual coverage and precision

The original baseline is pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` plus the registered
USA ROM. **Confirmed source structure / accepted remake content:** the
[opening chapter](../../../docs/design/synthesis/chapters/grans-opening/opening-through-battle01.md),
[commission](../../../docs/design/synthesis/chapters/grans-opening/battle01-return-through-battle02.md),
[Yeel](../../../docs/design/synthesis/chapters/grans-opening/battle02-outcome-through-battle03.md),
[house](../../../docs/design/synthesis/chapters/grans-opening/battle03-outcome-through-battle04.md),
[captivity](../../../docs/design/synthesis/chapters/grans-opening/battle04-outcomes-through-battle05.md),
[escape](../../../docs/design/synthesis/chapters/grans-opening/battle05-outcomes-through-battle06.md),
[return](../../../docs/design/synthesis/chapters/grans-opening/battle06-outcomes-through-battle07.md),
[tower aftermath](../../../docs/design/synthesis/chapters/grans-opening/post-tower-return-through-earthquake-entry.md)
and [docks](../../../docs/design/synthesis/chapters/grans-opening/earthquake-town-through-docks-departure.md)
own source symbols, exact callers and provenance. Their original natural reach/timing Unknowns
remain. The [ordinary campaign owner](../application/pre-voyage-campaign.md) separately owns the
admitted remake world, outcomes and actual connected routes; guidance admits none of those itself.

The following is modern guidance content, not a claim that the original had objectives. Regions and
actions are deliberately useful at coarse precision; absence of an exact anchor does not erase them.
`Fnnn` and coordinates below are internal content facts, never player-facing UI.

| Reached free-control boundary | Current goal and next destination/action | Precision / withholding |
| --- | --- | --- |
| Initial home, before F601/F602/F603 | Start the day; leave home. Outside, speak with mother near home. | Current room / known home; no King, messenger or mission spoiler. |
| Mother’s completed caller F601, before F602/F603 | Go to school; join classmates, or speak with the teacher when already downstairs. | School/room advice. Source483 discloses school; no future visitor. |
| F602, before F603 | Return to the classroom and join the lesson. | Class area/action, not the undisclosed messenger. |
| F603, before royal F605 | Visit Granseal Castle; north town exit / royal quarters. | Known castle and room role, no inferred doorway marker. |
| F605, before F608 | Join Astral in the castle hall. | Return from the royal room to the hall. Refusal does not advance it. |
| F608, before F401 | Follow Astral toward the tower; western stairs and real guard. | Castle passage advice; no enemy/encounter disclosure. |
| F401, before F501 | Enter Ancient Tower through the admitted castle/tower passage. | Same on defeat/EGRESS; no fabricated victory. |
| F501, before F609/F610 | Return to the King; tower return → royal room. | Actual changed setup, not the earlier guard invitation. |
| F609, before F610 | Speak with the royal party in the hall. | Hawel withheld until the commission returns. |
| F610, before F502 | Find Hawel near Yeel; leave town west, then travel toward Yeel. | The commission already disclosed Hawel; Jaha’s on-route scene stays ordinary. No Battle02 spoiler. |
| F502/F610, before F72/F612 | Find Hawel near Yeel; enter the village and ask villagers. | No new Kazin quest-giver marker or inferred villager conversation history. |
| F72, before F4/F612 | Visit Hawel with Kazin. Yeel: recommended north exit. Overworld before F503: known house, Route not available. | Pilot exit binding retained; no bearing through the encounter. |
| F72/F503, before F4/F612 | Same goal; known house entrance when visible. | Actual Map66 `(6,5)` from `(6,6)` to Map43 `(6,11)`. |
| F4/F612, before F504 | Go to the Ancient Shrine; leave Hawel’s house / continue south outside. | Source last line834 names **Ancient Shrine**, not Galam. No capture/ambush spoiler; coarse onward leg. |
| F504, settled prison before F661 | Find a way out of the cells; approach other prisoners. | Do not disclose the hidden tunnel or future Slade recruitment. Victory and special defeat converge only after real wake-up. |
| F661, before F662/F73 | Seek a way out; approach the stairs outside the cells. | The warning, not the guide, reveals a shortcut. |
| F662, before F73 | Find the known shortcut near the cells. | Revealed passage/action; no guessed navigation marker. |
| F73 in prison interiors | Follow tunnel to kitchen; then leave kitchen / find a door to the castle grounds. | Actual physical area changes the local action on this same map. Revisit does not invent restored layout copies. |
| Castle grounds before F505, including Battle05 defeat/EGRESS | Leave Galam; head for the western castle exit. | Live current setup. Do not send an escaped party back through the original closed cells. |
| F505, before F506, including Battle06 retreat | Return to Granseal; southern Galam gate → drawbridge → countryside. | No mandatory Church event from Slade’s advice and no tower goal from F407 alone. |
| F506, before F620/F621/F457 | Find out what happened at Granseal Castle; town north exit / castle people. | Optional Kiwi accepted, declined or absent gives the same main goal. No early Elis destination from the full catalog. |
| F620, F621 or F457, before F507, including Battle07 EGRESS | Go to the tower; castle passage and connected tower portals. With the royal scene not yet heard, the hall advice can recommend speaking with that group. | Each is actual disclosure. A reached intro/retreat keeps the tower goal even without royal following or F506; do not restore old home/castle questions. |
| F507/F74/F640, including Map58 Continue | Leave the tower with the royal party; retrace passages toward the castle. | Restore the existing aftermath; no new aftermath save recipe. |
| Castle F507 before/after warning F622 | Cross castle; after the warning, leave through the south exit to get everyone safe. | No harbor instruction before the actual warning/town scene. F623 does not prematurely finish the escape. |
| Quake town after F624 | Reach Granseal docks through the safer east exit. | Source717–720 disclose ship/east route; visible `(50,24)` entrance may be marked. |
| Docks F507/F74/F640/F624 | Depart by ship; speak with the man beside the ship. | Actual actor138/old-man handler, not an assumed NPC name or stored NPC-coordinate marker. |
| Docks No/cancel/revisit | Same disclosed goal; usable field requery. | No departure consumed, no new choice ledger or auto-acceptance. |
| Boarding / F74 clear / successor Map14 | No objective or destination. | Hide through the scene and remain clear after usable successor input. No Battle08 goal. |

On other admitted pre-voyage travel maps the already disclosed goal remains, with the appropriate
return-to-castle/countryside/exit action. Partial/unrelated profiles, unadmitted setups, genuine
missing disclosure and inconsistent ambiguous rules remain uncovered. This does not assert that a
missing main-story task exists nowhere. It does not silently admit the later Yeel F650 setup or
other absent content to manufacture revisit coverage.

The exact pilot north exit is Map5 `(7,0)`, approached from `(7,1)`, returning Map66 `(18,21)`.
The new quake east exit is Map4 `(50,24)`, approached from `(49,24)`, entering Map44 `(1,25)`.
Anchors are checked against the real public warp/marker, active area, current approach collision and
live obstructions. They appear only in the current visible field view. Invalid/off-camera anchors
retain their named destination/action at lower precision; there is no all-map reach or shortest-path
promise. Coarse school, royal room, prison and portal advice has no invented exact marker.

## UI, language and persistence

Modern guidance / 现代引导 remains one default-On general preference, including old format1 files
missing it. Existing Draft/Apply/Back, atomic write/conflict/failure retention, defaults and read-only
overrides control both surfaces. Explicit Off survives New Game, Continue and restart. Gameplay saves
contain no guidance. #1703 GraphicsMode and all existing settings/input behavior are unchanged.

The root Member/Magic/Item/Search menu shows Current objective with known destination/action in its
30px bounded reading region. Names, initial command focus, Search reasons, Back and overflow reading
ownership remain. Party/Prepare hides the root and returns to its command. The passive 24px field
label and at most one hollow named visible entrance ignore pointer input. Host, dialogue, Search,
services, battle and window recovery hide them; the world remains at96px cells. Wider windows retain
capped widths. Only the current owner receives input; no click moves an actor or camera.

English/zh-CN use existing catalogs and optional companion display-name projection. Copy without a
companion has zero name operands. Proper names retain the pilot’s canonical UI spellings; no new
proper-name approval is implied. Adopted story terms distinguish Ancient Tower / 古代之塔 from Ancient
Shrine / 古代神殿. The arity dictionary is in `GameText.Keys.cs`; catalog loading/formatting stays in
`GameText.cs`, keeping both responsibilities below the source-size boundary.

## Verification and direct replay

`FieldGuidanceTests` retains generic read-only/ambiguity/anchor/follower behavior.
`PreVoyageGuidanceTests` exercises the actual finite content rules at the above boundaries, selected
variants, packed rooms, optional-member independence, detours, defeat/EGRESS flags, changed story,
genuine withholding and boarding cleanup. These are engine query behavior, not observer tests.
Run `uv run sf2 verify engine`, affected `uv run sf2 verify adapter`, and the clean committed planner.
Keep completed failures and rerun only affected owning tests plus newly invalidated checks.

The representative native observer is
`res://probes/engine_pre_voyage_guidance_observation.gd`. It uses the existing host, actual native
input/state/projection/errors and ordinary selected world, not screenshots or active-state setters.
Its three cases are:

- `opening`: actual New Game in a fresh slot; both surfaces/Off/On; real house exit and mother caller
  change daily advice to school without future-story disclosure.
- `prison`: compatible pre-session Map17 wake-return state, then actual doors, stairs warning,
  shortcut, same-map tunnel/kitchen transition and menu reading. No battle replay or fake door copy.
- `voyage`: exact copy of the retained real Map58 save into a fresh disposable namespace; ordinary
  Continue compares all saved progress at attach; actual reverse tower/castle/quake/dock route,
  repeat visits, No/cancel/Yes, boarding and usable Map14 with both aids clear. Source slot unchanged.

Load the owning checkout’s ignored environment and select its accepted ordinary world/scene. Set
`SF2_SAVE_ROOT` outside checkouts and a fresh `issue1578-*` `SF2_SAVE_NAMESPACE`, an explicit fresh
`SF2_HOST_UI_OUTPUT`, and `SF2_HOST_UI_CASE`. Only `prison` uses `SF2_HOST_UI_START`, a local JSON with
`start` map17/player entity0/position `(59,5)`/facing1/speed32 and flags
`0,1,2,3,4,32,33,34,35,36,501,502,503,504,611,612,650,660`.
`voyage` reads a copied ordinary `battle07-return` slot; it does not edit the payload or replay
outcomes/healing. Existing held Shift acceleration may shorten automatic scenes; actual input and
completion still belong to the production game.

`res://probes/engine_objective_guidance_observation.gd` remains the current complementary
preference/menu/layout consumer. Its `town` case retains the disclosed Hawel task when F610 is
present, withholding the guide name/entrance until the actual F72 caller; without F610 there is no
invented commission. Use F610/F502 in the ordinary pre-invitation checkpoint to observe that change.
Its `house` and F612 `revisit` cases now retain the disclosed Ancient Shrine goal, with the old
guide/house marker removed. Existing Off/Cancel/write-refusal, reading/Party focus, locale/name,
controller/remapped confirm, resize/recovery and save/Continue/read-only cases remain applicable.
Run affected `town`, `house` and `revisit` cases through fresh disposable `issue1705-*` namespaces
and fresh explicit output; the namespace is only this observer's existing isolation guard. Do not
replay the three campaign cases merely because this complementary observer changes. Older pilot
reports retain their original narrower expectations and do not establish current whole-campaign
acceptance. The local handoff routes both observers and their fresh launch entries.

Reuse the approximately149MB installed local world read-only. No asset copy, seven-battle rebuild,
new tool, route platform or campaign admission is planned. Finite table expansion is below1000
records with a small bounded definition footprint; query is O(rules + local anchor checks), one
record. Native cases run serially; bound live checkpoints to30s and a representative case to210s,
terminal reports to100KiB each and retained report material to2MiB. Record only key control/choice
boundaries, not per-frame world dumps. Replan on a missing admission or required new story ledger.
A local handoff provides fresh one-command replay and ordinary play launchers without changing the
player launcher or original slots.

**Unknown:** human usefulness, wording, default preference, physical-device readability and marker
visual cost until #1578/#1456 review. Engineering observations do not accept those judgments or
resolve original natural traversal/timing/hardware presentation.
