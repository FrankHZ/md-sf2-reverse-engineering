# Optional pre-voyage text guidance

This is the current implementation and coverage owner for
[#1578](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578).
The merged [#1705 pilot](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1706) introduced the
shared preference/query, objective text and entrance marker. The user authorized expanding coverage
from ordinary New Game through boarding. [#2072](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2072)
removes the destination circles, labels and coordinate bindings. The pilot and subsequent expansion
retain their original acceptance records in Git. Human acceptance stays in #1578/#1456.

**Standard rigor:** finite content disclosure across existing Application, preferences and field UI.
Deliver a useful current objective and known destination/action during every supported main-story
free-control segment. Choose text precision from the disclosed story. Include ordinary detours,
defeats, EGRESS, revisits, eligible Continue and the dock decision. Do not make optional recruitment mandatory.
Current coverage ends when boarding clears the aid. Future chapters can extend authored text
where needed. They require no destination marker coordinates or per-map marker coverage.

The [visual language](ui-design-language.md), F7 field controls and existing Modern guidance setting
remain in force. The [design agreement](modernization-design.md#optional-objective-and-destination-guidance-1578)
sets the text-only scope. [#1710](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1710)
established one independent objective panel that remains visible across ordinary exploration and
root gameplay-menu toggles.

## Ownership and disclosure

`GameSession.QueryFieldGuidance` reads current session/revision/map, ordinary field phase, selected
setup, existing flags and any content-specified physical area/follower. It returns one result or
no coverage; ambiguity never selects the first rule. It changes no clock, RNG, resources, encounters,
party, flags or saves. Ordinary motion retains the aid; dialogue, automatic scenes, battle, services,
stopped scenes and host/window owners hide it. Return/Continue recompute from current state.

`PrivateFieldGuidance` owns the finite story rules and copy keys.
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

This table describes the remake's modern guidance. Regions and actions use the precision the
reached story supports. `Fnnn` identifies internal story flags.

| Reached free-control boundary | Current goal and next destination/action | Precision / withholding |
| --- | --- | --- |
| Initial home, before F601/F602/F603 | Start the day; leave home. Outside, speak with mother near home. | Current room / known home; no King, messenger or mission spoiler. |
| Mother’s completed caller F601, before F602/F603 | Go to school; join classmates, or speak with the teacher when already downstairs. | School/room advice. Source483 discloses school; no future visitor. |
| F602, before F603 | Return to the classroom and join the lesson. | Class area/action, not the undisclosed messenger. |
| F603, before royal F605 | Visit Granseal Castle; north town exit / royal quarters. | Known castle and room role. |
| F605, before F608 | Join Astral in the castle hall. | Return from the royal room to the hall. Refusal does not advance it. |
| F608, before F401 | Follow Astral toward the tower; western stairs and real guard. | Castle passage advice; no enemy/encounter disclosure. |
| F401, before F501 | Enter Ancient Tower through the admitted castle/tower passage. | Same on defeat/EGRESS; no fabricated victory. |
| F501, before F609/F610 | Return to the King; tower return → royal room. | Actual changed setup, not the earlier guard invitation. |
| F609, before F610 | Speak with the royal party in the hall. | Hawel withheld until the commission returns. |
| F610, before F502 | Find Hawel near Yeel; leave town west, then travel toward Yeel. | The commission already disclosed Hawel; Jaha’s on-route scene stays ordinary. No Battle02 spoiler. |
| F502/F610, before F72/F612 | Find Hawel near Yeel; enter the village and ask villagers. | Withhold Kazin's offer until its caller returns. |
| F72, before F4/F612 | Visit Hawel with Kazin. In Yeel, use the north exit. On the overworld before F503, show the known house and “Route not available”. | Withhold the encounter and the approach through it. |
| F72/F503, before F4/F612 | Same goal. Head for Hawel's house entrance. | The admitted house route owns the actual warp. |
| F4/F612, before F504 | Go to the Ancient Shrine; leave Hawel’s house / continue south outside. | Source last line834 names **Ancient Shrine**, not Galam. No capture/ambush spoiler; coarse onward leg. |
| F504, settled prison before F661 | Find a way out of the cells; approach other prisoners. | Do not disclose the hidden tunnel or future Slade recruitment. Victory and special defeat converge only after real wake-up. |
| F661, before F662/F73 | Seek a way out; approach the stairs outside the cells. | The warning, not the guide, reveals a shortcut. |
| F662, before F73 | Find the known shortcut near the cells. | The warning has disclosed the passage. |
| F73 in prison interiors | Follow tunnel to kitchen; then leave kitchen / find a door to the castle grounds. | Actual physical area changes the local action on this same map. Revisit does not invent restored layout copies. |
| Castle grounds before F505, including Battle05 defeat/EGRESS | Leave Galam; head for the western castle exit. | Live current setup. Do not send an escaped party back through the original closed cells. |
| F505, before F506, including Battle06 retreat | Return to Granseal; southern Galam gate → drawbridge → countryside. | No mandatory Church event from Slade’s advice and no tower goal from F407 alone. |
| F506, before F620/F621/F457 | Find out what happened at Granseal Castle; town north exit / castle people. | Optional Kiwi accepted, declined or absent gives the same main goal. No early Elis destination from the full catalog. |
| F620, F621 or F457, before F507, including Battle07 EGRESS | Go to the tower; castle passage and connected tower portals. With the royal scene not yet heard, the hall advice can recommend speaking with that group. | Each is actual disclosure. A reached intro/retreat keeps the tower goal even without royal following or F506; do not restore old home/castle questions. |
| F507/F74/F640, including Map58 Continue | Leave the tower with the royal party; retrace passages toward the castle. | Restore the existing aftermath; no new aftermath save recipe. |
| Castle F507 before/after warning F622 | Cross castle; after the warning, leave through the south exit to get everyone safe. | No harbor instruction before the actual warning/town scene. F623 does not prematurely finish the escape. |
| Quake town after F624 | Reach Granseal docks through the safer east exit. | Source717–720 disclose the ship and east route. |
| Docks F507/F74/F640/F624 | Depart by ship; speak with the man beside the ship. | Actual actor138/old-man handler. |
| Docks No/cancel/revisit | Same disclosed goal; usable field requery. | No departure consumed, no new choice ledger or auto-acceptance. |
| Boarding / F74 clear / successor Map14 | No objective or destination. | Hide through the scene and remain clear after usable successor input. No Battle08 goal. |

On other admitted pre-voyage travel maps the already disclosed goal remains, with the appropriate
return-to-castle/countryside/exit action. Partial/unrelated profiles, unadmitted setups, genuine
missing disclosure and inconsistent ambiguous rules remain uncovered. This does not assert that a
missing main-story task exists nowhere. It does not silently admit the later Yeel F650 setup or
other absent content to manufacture revisit coverage.

Next steps describe the disclosed route. Story progress can select “Route not available”, as it does
on Map66 before F503. Moving actors or local approach collision no longer replace authored text.
The ordinary map and interaction systems own travel legality, entrances and their labels.

## UI, language and persistence

Modern guidance / 现代引导 remains one default-On general preference, including old format1 files
missing it. Existing Draft/Apply/Back, atomic write/conflict/failure retention, defaults and read-only
overrides control the panel and pickup glints. Explicit Off survives New Game, Continue and restart. Gameplay saves
contain no guidance. #1703 GraphicsMode and all existing settings/input behavior are unchanged.

Ordinary exploration and the root Member/Magic/Item/Search menu share the same passive objective
panel, containing the current goal, known destination and next action. Opening/closing/reopening the
root menu keeps the panel instance and current text; command help contains only its command, reasons
and input hints. The objective takes no keyboard, controller or pointer focus. Names, initial command
focus, Search reasons and Back remain. Party/Prepare hides the root and objective; Back restores the
same command and requeries the objective. There is no separate objective-reading focus or Back step.

At supported1920×1080 and larger windows, the panel measures its longest unwrapped localized line,
uses at most448px of reading width plus32px horizontal padding, and grows vertically for natural
word/CJK wrapping. Its body stays30px, including custom companion names. Short copy shrinks below
the480px cap. Preferred placement is the top-left safe edge, then bottom-left/right corners as needed
to avoid the player (24px clearance) and actual root command bounds (24px clearance). The panel stays
inside safe edges, with no camera/world zoom change. Host, dialogue, Search results, services,
battle and window recovery hide the panel. Off hides the panel in both root-menu states.
The world remains at96px cells; no objective click moves an actor or camera.

The English and Simplified Chinese catalogs own the authored objective, destination and next action.
The [localization agreement](../application/localization-design.md#fixed-names-in-modern-guidance)
owns the spelling source. When a rule supplies a companion, `DisplayNames.Member` resolves its
display name and honors player-entered names. Copy without a companion has zero name operands.
Applying the language preference refreshes the current field projection. Continue recomputes it
from restored field state with the currently applied language. `GameText.Keys.cs` owns placeholder
arity and `GameText.cs` loads and formats catalog text.

## Field pickup glints

**Standard scope ([#2024](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2024)).**
Modern guidance also marks unclaimed field items, including hidden Search items. The existing
default-On preference controls both aids. Assist gives the selected row a short description in
English and Chinese. Pickup and ordinary Search remain independent of the preference.

The [#2062](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2062) four-point star uses
a stepped, opaque dark outline, warm fill and a pale center. Its 9×9
source-pixel footprint follows the pickup's map plane. At 1920×1080, each source pixel draws as
4×4 screen pixels, for a 36×36 mark inside a 96×96 world cell. Only the fill and center pulse
over 2.4 seconds. Reduced flash holds both at full opacity with the same footprint.

Read the selected map's Search placements, working item markers and existing receipt flags.
An item that the party cannot carry yet still qualifies. Empty placements, gold-only rewards and
description callbacks have no item glint. Use the current traversal area, a clear local sightline
to a walkable side from which Search could face the object, and the field draw clip. The sightline
checks collision cells because distinct rooms or locked passages can share an area rectangle.
It does not route around blockers or follow warps. [World draw order](display-composition-contract.md#field-world-priority-is-not-one-number)
owns foreground occlusion. Show the aid only in ordinary field control with the menus closed.

Acceptance uses the same acquisition and receipt through success, Full and repeat, plus a bounded
check of closed space, foreground, settings and Save/Continue. The admitted world and saved
progress provide all inputs. There is no additional discovery or pickup record.

### Pickup observation

Use the owned Godot .NET Debug project, default English keyboard controls, and
`res://probes/engine_field_pickup_glints_observation.gd`. Select the admitted
[fixed battle-backdrop candidate](battle-scenes.md#fixed-backdrops-for-the-private-campaign) and its matching
world, entry, main-scenes and six opening inputs.
Select a new external `issue2024-*` save namespace. The `continue` case reuses the `shrine`
namespace. Use the [worktree configuration](../../../docs/operations/local-private-inputs.md)
and worktree-local TEMP, APPDATA and LOCALAPPDATA. Set one `SF2_HOST_UI_CASE` and a fresh ignored
`SF2_HOST_UI_OUTPUT` per process. Controlled start coordinates and facing are under `start`.
Facing uses 0 for Right, 1 for Up, 2 for Left and 3 for Down.

```powershell
& $env:GODOT_BIN --headless --path remake/game --audio-driver Dummy --max-fps 120 `
    --resolution 1920x1080 --script res://probes/engine_field_pickup_glints_observation.gd
```

The observer reads the running draw commands and session. It frees its mounted host before exit.
Require a passing report with a nonzero check count, exit zero and a clean engine error log.
In each visible witness, `marks` reports `pixelSize` as 4 and `width` and `height` as 36.
`left` and `top` plus the footprint fit the reported legal screen clip, and the mark center
is 18px from each edge. The `hold-foreground` witness relates the star's upper arm to the
later high-priority foreground draw. These checks cover projection and input. Player visual
acceptance stays in [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).

| `SF2_HOST_UI_CASE` | Controlled prerequisite and ordinary observation |
| --- | --- |
| `shrine` | Prepare slot 1 from the accepted #2053 Map42 v6 shrine slot at (19,23), facing Down. Use the registered `handoffs/issue2053/snapshot-mechanisms` shrine writer to regenerate the source if needed. On a detached copy, remove F133 and layout delta index1555 to restore the base closed chest. Replace the granted item9 and the empty slots of the five joined members with item0. Keep the UTC save header and recalculate the normal save envelope. Title Continue admits the result. Observe the post-Apply Off and Reduced flash policies and hidden marks under field commands and Search feedback. Search with Full leaves the same packs and closed chest. Drop one herb, Search for item9/F133, repeat empty, then save into slot 2. |
| `continue` | New process in the same namespace, with no controlled start. Title Continue slot 2 preserves exactly one Power Water and F133, with no glint. Repeat Search and climb the CURRENT stair to (2,8). With F901 clear, (4,3) in the hall lacks its item marker. Exit to Map41 and check the new projection. |
| `foreground` | Copy the #2035 `before-writer.json` controlled input under worktree `local/` and select it via `SF2_HOST_UI_START`. Set Map17 (59,5), Up. The hidden F205 mark appears in the prison area. Up, Left, Left and Search grant item126 and clear the mark. |
| `wall` | Likewise use #2035 `return-writer.json` at Map16 (32,23), Right. An in-frame working wall at (33,23) blocks the view of the marked chests at (34–35,23). |
| `docks` | Use the #2053 `dock-write.json` start at Map44 (27,7), Up. The A-plane mark appears on the deck barrel, the separate hold mark is absent, and Search grants item11/F139. |
| `hold-foreground` | Use the same dock start at (59,9), Left. The B-plane star's upper arm intersects the later high-priority A-plane draw for block201 at source cell (58,22). Atlas alpha at block pixel (12,12) is opaque. Compare the bounded layout draw projection and this pixel in the mounted block definition. |

**Confirmed in the remake** at these controlled boundaries. The sightline uses current collision
cells and does not claim that walking to every marked point will succeed through moving actors
or scripted movement. Ordinary item acquisition, eligible save locations and later map admission
keep their existing owners. These draws provide no evidence about original-game rendering or timing.

## Verification and direct replay

`FieldGuidanceTests` covers text selection, ambiguity, follower requirements and read-only behavior.
`PreVoyageGuidanceTests` exercises the actual finite content rules at the above boundaries, selected
variants, packed rooms, optional-member independence, detours, defeat/EGRESS flags, changed story,
genuine withholding and boarding cleanup. These are engine query behavior, not observer tests.
Run `uv run sf2 verify engine`, affected `uv run sf2 verify adapter`, and the clean committed planner.
Keep completed failures and rerun only affected owning tests plus newly invalidated checks.

For a catalog-only change, compare every `guidance.*` key used by `PrivateFieldGuidance` and the
adapter against both PO catalogs. Compare placeholder counts and indices. Include
`save.destination.yeel-field` in the bilingual check.

For the Yeel name and Continue check, use the [default-setup guide approach and private input
selection](../application/opening-session-lifecycle.md#yeel-save-and-continue-observation) in a fresh
save namespace. Supply `start.names` with a seven-character custom `ally-4` name. With English
applied, observe the initial Hawel goal, then Talk to the guide and verify the changed goal at F72.
Apply Simplified Chinese in field Settings and verify that the current objective, destination and
next action reflect the new locale and retain the exact custom name. Save at the settled field
boundary. Start a fresh process and open title Continue. Verify the Chinese Yeel slot label and
the same guidance and companion name after attachment. Compare the saved progress at attachment
and check for field and adapter errors.

**Confirmed:** a controlled default-setup Yeel visit changed the English objective after the
guide caller. Applying Simplified Chinese refreshed the same session with a custom companion
name. Fresh-process Continue showed 耶尔 on the slot and restored the localized goal, companion
name and complete saved progress at attachment.

The representative native observer is
`res://probes/engine_pre_voyage_guidance_observation.gd`. It uses the existing host, actual native
input/state/projection/errors and ordinary selected world, not screenshots or active-state setters.
Its three cases are:

- `opening`: actual New Game in a fresh slot, with the panel checked both Off and On. The real house
  exit and mother caller change daily advice to school without future-story disclosure.
- `prison`: compatible pre-session Map17 wake-return state, then actual doors, stairs warning,
  shortcut, same-map tunnel/kitchen transition and menu reading. No battle replay or fake door copy.
- `voyage`: exact copy of the retained real Map58 save into a fresh disposable namespace; ordinary
  Continue compares all saved progress at attach; actual reverse tower/castle/quake/dock route,
  repeat visits, No/cancel/Yes, boarding and usable Map14 with the previous goal cleared. Source slot unchanged.

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
present, withholding the guide name until the actual F72 caller; without F610 there is no
invented commission. Use F610/F502 in the ordinary pre-invitation checkpoint to observe that change.
Its `house` and F612 `revisit` cases retain the disclosed Ancient Shrine goal after the house scene.
The town and house approaches observe the text-only projection at former marker locations.
Existing Off/Cancel/write-refusal, command/Party focus, locale/name,
controller/remapped confirm, resize/recovery and save/Continue/read-only cases remain applicable.
The old root-menu objective and overflow-reading assertions are replaced by same-panel continuity,
no duplicated command help, passive command focus and Party return checks. The `compact` case uses
an admitted pre-session checkpoint to check short or long actual localized copy at1920×1080 and
2560×1440, repeated root open/close/reopen, full wrapping/padding, Off/On and below-minimum recovery.
Run it with both English and zh-CN, including a short opening goal and a longer disclosed goal;
it uses the same existing `issue1705-*` isolation guard and no source/catalog mutation.

### Text-only observation

Reuse the Debug launcher and `derived/issue2035/galam-town-v1` inputs from
[Pickup observation](#pickup-observation). Select `derived/issue2001/catalogs-v1/world.zh-CN.json`
with `SF2_DISPLAY_CATALOG`. Run the objective guidance observer serially, with a fresh output
for each case and separate `issue1705-*` save namespaces except for the Save/Continue pair.
For controlled starts, copy [the post-Battle02 checkpoint](../../reference/inputs/yeel-to-battle03-start.json)
to ignored scratch and apply the changes below.

| Case | Setup and observed boundary |
| --- | --- |
| `town` | Use the default Yeel guide approach at (7,7), Up, with F610/F502 set and F399 clear. Supply a custom seven-character `ally-4` name and English/HD preferences. Talk and finish the actual F72 caller. Toggle guidance, apply Chinese, then walk out the real north exit. The field projection contains only the text panel. It shows the localized north exit instruction and then replaces it with the Map66 route-unavailable text. The custom name survives the locale switch. |
| `house` | Use the same four-member checkpoint family at Map66 (6,6), Up, with F72/F503 set and F399/F403 clear. The approach shows the named house in text. The actual warp and house scene replace it with the Ancient Shrine and the instruction to leave the house. |
| `save`, `continue` | Use the pre-invitation Yeel setup for Save. Compare the recorded `saved-guidance` and `continued-on` samples. A fresh title Continue restores the same three text lines and initially honors the saved Off preference. Applying On shows the objective bound to the restored field. |
| Pickup `shrine`, `continue` | Copy the retained #2024 full/unclaimed shrine slot into a fresh `issue2024-*` namespace and use the pickup observer. The shared preference still hides the glint, Reduced flash holds it steady, and Search updates the mark through Full, grant, repeat and fresh Continue. |

**Confirmed in the remake** at these controlled boundaries with native input, live projection and
process error checks. An initial house attempt carried F403 and failed to reach the intended
field. The corrected post-victory input clears F403.

**Confirmed:** bounded #1710 headless native English/zh-CN runs retain one panel/text
through repeated root open/close/reopen and Party return, preserve command input and paused gameplay
state, hide for host/Off/recovery, and restore on return. Short opening copy measures345px in English
and242px in Chinese; longer English and Chinese copy (including the seven-character custom guide
name) wraps to four lines at480×209px. The reached right-edge town checkpoint also moves commands
left and the objective right without covering the player. The affected invitation, house-scene and
F612 revisit observers pass. These are actual input/state/geometry observations, not foreground,
physical-device or human visual acceptance. Retained early runs include a cross-pause RNG comparison
failure: free-field ambient steps resume after root Back. The corrected observer compares the full
save payload and seeds while the menu is still paused; it does not discard RNG fields.

Run affected `town`, `house` and `revisit` cases through fresh disposable `issue1705-*` namespaces
and fresh explicit output; the namespace is only this observer's existing isolation guard. Do not
replay the three campaign cases merely because this complementary observer changes. Older pilot
reports retain their original narrower expectations and do not establish current whole-campaign
acceptance. Their menu-hides-world/goal-in-command-help observations are superseded by #1710's
explicit human feedback; they are not current acceptance assertions. The local handoff routes both
observers and their fresh launch entries.

Reuse the approximately149MB installed local world read-only. No asset copy, seven-battle rebuild,
new tool, route platform or campaign admission is planned. Finite table expansion is below1000
records with a small bounded definition footprint; query is O(rules), one
record. Native cases run serially; bound live checkpoints to30s and a representative case to210s,
terminal reports to100KiB each and retained report material to2MiB. Record only key control/choice
boundaries, not per-frame world dumps. Replan on a missing admission or required new story ledger.
A local handoff provides fresh one-command replay and ordinary play launchers without changing the
player launcher or original slots.

**Unknown:** human usefulness, wording, default preference and physical-device readability
until #1578/#1456 review. Engineering observations do not accept those judgments or
resolve original natural traversal/timing/hardware presentation.
