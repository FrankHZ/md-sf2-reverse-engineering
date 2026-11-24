# Battle UI composition

[Issue #2064](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2064) accepted the
**High** rigor design. Several presenters consumed the same battle decision independently, and
misrouted input could commit an action. [Issue #2079](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2079)
implements phase visibility, board marks, decision layout and geometry-only close-up resize.
[Issue #2080](#fresh-target-input-and-direct-stay) delivers fresh target navigation and direct Stay.
[Issue #2090](#vertical-battle-main-2090) restores the vertical Main command list.
The [feature trace](battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
records current behavior. [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456)
owns the player's review of the composed experience.

## Scope agreement

Use the mounted battle presenters and existing canvas, input, movement, action, transaction and
save contracts. The session publishes the active phase, and the battle presenter owns the local
command focus and decision projection. The map owns spatial marks. Command controls measure their
hit rectangles, and inspection owns its read-only local focus. Short controlled input routes supply
native acceptance. The implementation handoff tracks the delivered inspection movement, roster,
spell descriptions and the deferred current-round display. Original-game evidence and
intentional remake features retain their separate provenance.

**Confirmed, pre-#2079 remake source.** The acting bracket, target cursor or rectangle and
inspection cursor had independent lifetimes. Main and the child lists could paint action reach
while the acting bracket was still visible. Inspection subpages retained a visible board cursor.
The origin of each line in the [player report](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616#issuecomment-7085667332)
remains **Unknown**.

## Phase, focus and return

Host cover has precedence for input. Next come authoritative continuation, outcome and failure
states, then the active inspection page, then the live player selection and command page. Player
preview/undo delivery belongs to Movement until it completes. `TargetChoice` and `CommitReady`
share the same target page. Only a legal control or presentation state can supply its corresponding focus. Derive this
on transitions and refreshes, including refusal, settings, resize, restoration and battle exit.
Visibility and hit testing change together.

| Phase | Sole primary focus | Allowed support | Input and exit |
| --- | --- | --- | --- |
| Battle entry, round program or appearance | Current scene, or the current message when it needs acknowledgement | Board and authored motion when delivery owns them | Existing continuation. Decision cards, command hints and board selection marks are hidden. |
| Player movement or its delivered preview/return step | One cell cursor following the unit's displayed position through the delivered step | Acting card, round, one movement field and movement hint | Mapped movement holds continue on arrival. Confirm uses the real legal stop. Cancel from an unsettled move finishes undo before opening inspection. While movement is delivering, keep the display, with input governed by its existing step boundary. |
| Main action | One focused row in the vertical command list | Acting card, round, footer with consequence/refusal and Back | Initial Attack when enabled, otherwise the Stay/Search row. Up/Down wraps focus and Confirm or a complete row click chooses. Mapped shortcuts choose their existing command. Main Back resumes Movement at the displayed preview if it is still in range. Choosing Stay commits the current legal stop in one gesture. |
| Magic or Item | One row, keyed by spell and level or actual inventory slot | Acting card, round, one selected-offer reader in the footer | Opening either child is available even with no usable offer. Child rows replace Main and keep focus visible while scrolling. A disabled row is readable. Choose an enabled row to enter target selection. Back restores the parent command. |
| Target, including self, single- or multi-target | One board cursor at the current legal candidate | Recipient card replaces acting card. The one action field shows reach and the current effect. The footer shows a concise action name, Confirm/Back and refusal when needed. Targeting omits the terrain panel. | Each fresh direction or Target shortcut cycles the live candidates once. Hover previews a candidate. A complete stable legal click uses the existing commit path. An invalid cell clears the cursor/card/effect and disables commitment even if the session still holds an older target. An invalid cell with eligible alternatives offers retargeting and Back. With zero eligible targets, the reason and Back remain. Back clears target state and restores the same command or item/spell row. |
| Search confirmation, Equip, Give/exchange or Drop | Current transaction choice, slot, recipient or confirmation | Acting identity where relevant, the one existing auxiliary reader and its own instructions | Existing eligibility, selection, read-before-confirm, receipt and return contracts. The transaction replaces the command/target reader, board cursor and range. |
| Free inspection, Cursor page | One board cursor on the inspected cell | A unit card on occupied cells, a terrain panel, one framed instruction and return panel, and the inspected unit's movement field when a living unit is disclosed | Directions and board pointer select a cell for reading. Confirm opens details on a unit or Options on empty ground. Empty ground clears the range and still shows terrain. Cancel restores live Movement. |
| Inspection Options, Map, Members or details | The open panel's option, map selection, member row or detail pane | One framed instruction and return panel with data from the existing read-only query | Retain cursor coordinates and caller selection for Back, hide board cursor and range. Full → Cursor, Map/Members → Options, Options → Cursor. An on-demand Map selection can locate its disclosed unit and return to Cursor. |
| Enemy action, close-up, field notice/death, result or battle exit | Actual delivered action or current message/return prompt | Playback cards for the actual participants, message reader, board only when owned by the continuation | Existing completion token and acknowledgement. All decision surfaces and board marks retire. Rebuild from live control after delivery. Field return uses field chrome. A failure shows once at its owning view. |
| Host Menu, Save, Speed or recovery cover | Host/modal's current choice or reader | Only the permitted covered backdrop | Suspend battle gestures, holds and pacing using existing ownership. Restore the same eligible caller, with its current layout and fresh input after release. A resize while covered may recompute geometry. |

### Main Back and movement reselection

Main Back closes the action list and resumes Movement in the same submission when the preview
is still in range. Weighted reach starts at the actor's committed turn origin. Reopening Main
does not extend the range. A subsequent, fresh Movement Cancel delivers the ordinary undo and
enters inspection. Magic and Item Back restore Main. Target Back restores the originating row or
command. Equipment and Search confirmation Back restore Main at the same preview.

Equip and Drop return to the action menu at the already confirmed stop. If the equipment change
put that stop outside current MOV, Main Back delivers the reverse of the validated path to the turn
origin. Movement then uses the new MOV and keeps the gear or custody change.
[Confirmed stop and running gear](../domain/ring-equipment-design.md#remake-loadout-actions-and-stats)

**Confirmed, static original:** `ProcessBattleEntityControlPlayerInput` restores
`BATTLE_ACTOR_X/Y`, rebuilds the movement grid and branches back to movement when the outer
diamond menu returns `-1`. The remake's immediate reselection at the preview is the player-facing
decision in #2153. Original input timing and presentation remain **Unknown**.
[Pinned original branch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L145-L159)

The [allied traversal corridor](battle-interaction-baseline.md#allied-traversal-range-2019)
uses ordinary mapped input to check targeting and its parent returns, one held Back at (5,3),
the restored range drawn from (3,3), and a fresh Right step to (6,3) without spending the turn.

## Vertical battle Main (#2090)

**Standard rigor.** [#2090](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2090)
restores the Main list seen at `277cb0dad2ce7dc7a6f34a7443426d422d48e25f`. The diamond
introduced at `434c2ae93cdb278f0a5fcc727d61d60b1929a38b` exceeded the requested layout
scope. The shared compact list owns row layout and hit bounds. `BattleCommandMenu` owns the
selected row and existing shortcuts. The card, footer, child lists and phase routing retain
their existing owners. Original cardinal input and the Search/Stay branch remain documented
in their [source contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md).

### Reproduce the Main rows

Use the builds and isolated headless setup in [Reproduce slice 1](#reproduce-slice-1). Use a
fresh checkout-local report and log for each case and a disposable save namespace.

For the authored battle, copy `remake/content/authored/battle-give.json` to ignored scratch.
Use the champion, spell and weapon changes from [the Target recipe](#reproduce-target-navigation-and-stay).
Keep one foe and place `reserve` at (1,1). Keep the allied placements and Herb slot. The
[compact command observer](../../game/probes/engine_compact_commands_observation.gd) starts the
real host with this controlled package. Run it once with `language=en` and route `keyboard`
and once with `language=zh-CN`, `reducedFlash=true` and route `pad`. It resizes the client
to 1600×900 and back to 1920×1080 and projects the actual canvas rectangles into each client.

For mounted command art, use the same observer with the private Battle01 source and start
under [Reproduce slice 1](#reproduce-slice-1), English settings and keyboard route. For Search,
use unchanged `remake/content/authored/battle-search.json` with English and keyboard route.

| Case | `SF2_COMPACT_CASE` | Package argument |
| --- | --- | --- |
| Authored battle | `authored-battle` | Controlled `battle-give.json` copy |
| Private Battle01 | `battle` | Set `SF2_COMPACT_START` and select registered assets in place |
| Authored Search | `authored-search` | Tracked `battle-search.json` |

From `remake/`, use absolute report, settings, package and log paths.
For the private case, use an empty `$packageArguments` array. For either authored case use
`@('--authored-package', $package)` and leave private selection unset.

```powershell
$env:SF2_COMPACT_CASE = $case
$env:SF2_INSPECTION_ROUTE = $route
$env:SF2_VIEWPORT_OUTPUT = $report
& $env:GODOT_BIN --headless --path game --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
    --log-file $nativeLog --script res://probes/engine_compact_commands_observation.gd -- `
    --input-settings $settings @packageArguments
```

For ordinary Stay, use the existing [direct Stay confirmation and click cases](#reproduce-target-navigation-and-stay).
Keep Down held on the move from Item to Stay in the confirmation case. Require exit 0, a passing
report and clean script/runtime error output.

**Confirmed, bounded remake.** English keyboard and Chinese D-pad traversed all four rows in both
directions, wrapped, and kept the same footer and revision. All four labels and hit regions fit
between the measured card and footer at both client sizes. The pointer entered Item from a press
at the row edge and a release over its caption. A press started before resize was rejected on
release. Attack Target Back returned to Main. Magic and Item Target Back returned to the same
spell level and inventory slot. Browsing left resources and both RNG seeds intact, and Main Back
restored Movement. Private Battle01 drew the registered 24×24 command art at nearest 3×. The Stay
runs advanced exactly one turn from a single D-pad Confirm or complete click. Search entered
explicit confirmation, allowed Back, and then returned the admitted result when confirmed.

The initial geometry run exited 1 because the observer compared fitted canvas coordinates
directly with physical client dimensions. The first Stay runs exited 1 because the observer
assumed the former cardinal Down jump. The observer now applies the viewport stretch transform
and reaches Stay through the rows. One earlier private run reported six ObjectDB instances at
exit. Its successful rerun emitted no exit warning. These observations use scripted mapped input
and geometry. Physical controller feel and human visual review remain with #1456.

## World marks and panel placement

One board-owned cell cursor serves Movement, Target and inspection Cursor. It draws the
[published 32×32 asset](battle-scenes.md#original-target-and-inspection-cursor-publication) at
nearest 4× centered on 96px cells, with 16px overhang per side. Public and older private packages
use the same board-aligned authored pixel corners. The movement cursor follows the displayed actor
through preview and undo. Subordinate inspection pages hide the mark and retain the inspected cell
and camera. Command and panel pages express focus in their own controls. Actor colors and source
foreground priority are preserved.

Reuse the board-local movement and action drawing owners. Only one field may be visible, below
units and source foreground, using a pixel-aligned tile motif that leaves terrain readable.
Movement uses cyan for allies and red for enemies, with distinct interior fill for legal stops and a simpler edge mark for
traversable-only cells such as occupied friendly cells. In targeting use warm reach tiles and one
lighter inset for affected cells. A cell in both has one outline and a distinct effect interior.
A radius-zero effect shares the candidate cell with the cursor. Rules, costs, target side,
AURA-all behavior and multi-target incidence come from the live offer and effect content.
Range geometry explains reach. Enabled recipients remain the session's admission decision.
When the candidate is invalid, only the valid offer's reach can remain. Keep a restrained
version of the existing pulse, with a steady presentation under Reduced Flash. Exact original
pulse and graphics parity remain **Unknown**. [Original range construction](../../../docs/research/battle/battlefield-pathfinding.md#range-and-target-construction)

Use the [shared visual language](ui-design-language.md) for all panels. At 1920×1080, anchor the
quiet round panel at the existing upper-left gameplay inset below host chrome, with the terrain
readout below it. Use the existing roughly 560px right-side decision column,
32px gameplay inset and below-Menu band. The acting, recipient or hover card takes its top role
as the phase changes. Let wide values grow within the layout budget. Place the Main or child
list below the measured card. Use one independent reading/footer region below the controls
and keep Back and Confirm fixed within their phase. Main uses `CompactCommandList` in
Attack, Magic, Item, Stay/Search order. The existing icon control draws the admitted art
at nearest 3× beside the persistent 24px localized label. Each complete row is a single hit
area. Magic and Item retain their scrollable lists. A blocked command can show its refusal
in the same reader.

Reserve the footer's reading bounds on page entry from the available viewport and keep them
stable across offers. Long rows and descriptions wrap and scroll within their own bounds without moving
the controls. On 1600×900 use the existing fitted canvas. At larger native sizes use the
existing edge anchors and readable panel widths. Reflow by phase, viewport and protected-subject
geometry only. Use `BattleMenuLayout.AvoidSubject` and the existing hover-card clearance to
protect the focused board cell and honor the right-top host exclusion. When the right column
would cover that cell, keep the footer and card aligned in the alternate clear stack and place
the order opposite them, below the protected cell if needed. Fit a bounded reader and scroll before
allowing card, command, hint or host rectangles to intersect. Hit rects and pointer occlusion
follow the actual visible panels.

`BattleSessionView` owns visibility, transition fences and the resize call. `BattleMenuLayout`
owns decision geometry and its card and footer. `BattleMapViewport` owns map-local markers and
ground effects. Keep the existing transaction viewers and restrict the legacy HUD to the
auxiliary or failure information it actually owns. Retire the dormant general equipment/spell/item
text stack from ordinary decisions and show each error once at its current owner. Keep the
`PresentationCanvas` boundaries and the local actor/weapon/effect order. Wire the existing
resize owner into a geometry-only `BattleSceneView` refresh for close-up art, cards and message
bounds, including a paused cover. Keep the same participants, phase, token, reveal state and
elapsed delivery. [Close-up resize finding](display-composition-contract.md#findings-and-minimum-remedies)

## Read-only inspection and the member view

[#2081](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2081) delivers the inspected
unit's movement field on the Cursor page. `QueryBattleInspectionRange` uses the existing
settled-origin, session/revision and disclosure checks before calling `BattleMovement.Range`.
The field starts at the inspected unit's actual cell and uses live Move, mover type, terrain and
occupancy. Both factions can cross a friendly occupied cell without stopping there. The shared
board overlay draws `ReachableCells` as edges and fills the legal `Destinations`.
The inspected actor's faction selects cyan for allies or red for enemies.
Reduced Flash keeps the field steady.
The query runs when the inspected actor or live snapshot changes, or the visible Cursor page
returns. The field clears on empty ground or when the subject is stale, undisclosed, absent or defeated.
Details, Options, Map, Members and host cover hide it while retaining the cursor for Back. Exiting
inspection restores the active actor's ordinary Movement range. The range describes current
geometry under the battle's movement rules.
[Inspection query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleInspectionQuery.cs) ·
[Movement rules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleMovement.cs)

<a id="shared-member-roster-2082"></a>

### Shared member page (#2154)

Options → Member opens the same two-column page as field Party. The roster, portrait, attributes,
vitals, spells, four inventory slots and fixed item reader appear together. Left/Right or the
arrows switch members from any pane. Target cycles the roster, spells and items. Up/Down moves
within the focused pane. A complete row or slot click selects it. The wheel scrolls the list or
reader under the pointer. Switching to an empty slot clears the previous description.

The shared roster preserves the selected actor across refresh, reveals mapped selections and
uses the nearest surviving row when membership changes. Field Prepare returns to its selected
member and scroll. Battle Back returns directly to Options, then to the retained Cursor.
A unit opened directly from the board retains its own Full → Cursor route.

**Standard.** The adapter shares presentation across two callers with distinct read and command
authority. Field uses its joined party, records, gold and preparation actions. Battle uses
`QueryBattleMembers` in active-force order with its settled session/revision guard. Initialized
defeated and unplaced allies retain their disclosed details. An uninitialized ally has a named
row and unavailable details. Empty membership has an explicit message. Neither case initializes
a combatant. Battle has only a Back action in the footer. Its projected spells, inventory and
holder-specific item descriptions have no command callback.

[Shared page](../../game/src/UI/MemberPageView.cs) ·
[Battle query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMemberQuery.cs) ·
[Field owner](field-and-shop.md#current-composition)

#### Reproduce the shared roster

Build the Release adapter with `uv run sf2 verify adapter` and its Debug assembly under
[Reproduce slice 1](#reproduce-slice-1). Use the same isolated English standard-input settings.
All three runs use small project-authored packages, one Godot process at a time, with fresh
absolute report/log paths, isolated app data and TEMP, and disposable saves. Each observer
has a finite deadline of at most 75 seconds. The reports record semantic checkpoints and control
rectangles.

| `SF2_VIEWPORT_CASE` | Observer under `res://probes` | Ordinary `--authored-package` |
| --- | --- | --- |
| `member-small` | `engine_battle_member_observation.gd` | Tracked `remake/content/authored/stone-court.json` |
| `member-large` | `engine_battle_member_observation.gd` | Ignored `member-large.json` prepared below |
| `member-field` | `engine_party_details_observation.gd` | Ignored 14-member field variant described below |

Generate the large battle package from the repository root with an existing ignored `local/issue2154`
directory. All normal deployment and input admission rules apply.

```python
import copy, json
from pathlib import Path

package = json.loads(Path("remake/content/authored/stone-court.json").read_text(encoding="utf-8"))
package["items"] = json.loads(Path("remake/content/authored/battle-equipment.json").read_text())["items"]
placements = package["encounters"][0]["placements"]
tables = package["actors"], package["start"]["actors"], placements
templates = [copy.deepcopy(rows[1]) for rows in tables]
for i in range(14):
    if i < 2:
        actor, initial, deployed = [rows[i] for rows in tables]
    else:
        actor, initial, deployed = copy.deepcopy(templates)
        for rows, row in zip(tables, (actor, initial, deployed)):
            rows.append(row)
    actor["id"] = "deliberately-long-authored-member-identity-to-check-wrapping-001" if i == 0 else f"member-{i:02}"
    actor["items"] = [0, 127, 0, 0]
    initial.update(actor=actor["id"], hp=0 if i == 1 else 35, absent=i == 2)
    deployed.update(actor=actor["id"], x=1+i%7, y=1+i//7, processingOrder=i)
Path("local/issue2154/member-large.json").write_text(json.dumps(package), encoding="utf-8")
```

For `member-field`, generate the 14-member public variant from
[Party details observation](../verification/host.md#party-details-observation). Set
`battle.items[0].name` to `Authored restorative herb with a deliberately long label`.
Set `battle.battleDiscard.items` to one record with the same name, `id: 0`, `rare: false`
and `unsellable: false`. The matching name and field inventory metadata admit Prepare for
that controlled package.

From `remake/`, set `$case` and `$observer` from the table and use absolute `$report`,
`$nativeLog`, `$package` and `$settings` paths.

```powershell
$env:SF2_VIEWPORT_CASE = $case
$env:SF2_VIEWPORT_OUTPUT = $report
$env:SF2_INSPECTION_ROUTE = 'keyboard'
$env:SF2_INSPECTION_PRIVATE = '0'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path game --log-file $nativeLog --script "res://probes/$observer" -- `
    --authored-package $package --input-settings $settings
```

Require exit 0, `passed:true`, empty failures and unavailable lists, and clean runtime logs.
Also run the [#2081 authored corridor](#reproduce-the-inspected-movement-field) for enemy details,
range clearing and return.

**Confirmed, #2154.** Both battle sizes open the sheet on entry. The large route follows the last
row into view, clicks slot four, reads its effect, clears it on the empty slot, and returns through
Options and Cursor with the original battle signature. The field route selects the last of 14
members, opens Prepare and restores the same actor and scroll. Its previous geometry assertion
failed at “Readable groups remain inside1080p frame”: the stat column clears the portrait while
the spell column clears the header. The current check measures each against those bounds.

For visual review open F5 and F8 in [DesignPreview](../../game/probes/previews/README.md).
Add `-- --inspect --authored-package <absolute member-large.json>` for the long battle sample.
The 1600×900 and 1920×1080 English and Chinese comparison covered both callers, the wrapped
name, defeated and absent rows, four slots and the selected description. It exposed roster arrows
drawn behind the frame. Review of the corrected page showed both arrows and the separate footer
actions. The ordinary native witnesses use inputs and projections. UI demo pictures stay local.

**Earlier #2082 native observation.** The two- and fourteen-member battle routes passed.
The large case checked the 64-character name in both list and sheet, defeated and absent detail
eligibility, pointer and mapped input, exact scroll on Back, read-only state and ordinary Movement
return. The fourteen-member field route opened Prepare, returned to the same focused member and
scroll, and resumed ordinary field input. The #2081 corridor passed with Reduced Flash off and on.
The authored start reader rejects uninitialized allies, and these settled menus offer no
membership-changing command. Existing `BattleMemberQueryTests` cover uninitialized detail refusal.
Nearest-survivor selection was reviewed at the source boundary. The registered-art observation
below covers visible portraits and texture lifetime.

The first large run exited 1 after the initial 65-character ID failed admission and the observer
could not reach player control. The first field run exited 1 at “Field roster still opens Prepare”:
without field preparation metadata it reached `field-preparation-unsupported`. The corrected
fixtures above reached the intended boundaries. The first full-name detail check exited 1 at
“The long name remains readable on the full sheet”. The measured header layout passed its rerun.
Human review stays with #1456.

#### Reproduce the native member layout

Use the Debug build and isolated launch environment above. For visual review, launch the production
preview from the repository root with the large battle package generated above:

```powershell
& $env:GODOT_BIN --path remake/game --resolution 1600x900 res://probes/previews/DesignPreview.tscn -- `
    --inspect --authored-package $package
```

Confirm on empty ground, then confirm Member. Inspect the first three rows, cycle Target to Items,
select slot four, then select the empty slot. Switch to F5 to read the field accounting and footer.
L changes the language. **Confirmed** at 1600×900 English and 1920×1080 Chinese: the long battle
name wraps above the class and reading lists, the defeated and absent conditions are readable,
and the fourth slot and its effect fit together. Selecting the empty slot clears the effect.
Both arrows appear inside their frame. F5 shows both records, gold and the separate footer actions.

For ordinary input, use the `member-large` (mapped pad and pointer) and `member-field` commands
above. Use [party-capacity](field-and-shop.md#reproduce-four-slot-capacity-and-accounting) for the
long equipped/damaged item, missing records and four complete rows. Use
[`member-field-portraits`](#reproduce-registered-portraits) with the Map66 local start copy,
`SF2_INSPECTION_ROUTE=pad`, Chinese settings and the registered display catalog
`derived/issue2001/catalogs-v1/world.zh-CN.json` selected through `SF2_DISPLAY_CATALOG`.
Field Prepare opens actual actions and Back restores the borrowed portrait, member and scroll.
The fourteen-member field case exercises this return with nonzero scroll. The
[inspection corridor](#reproduce-the-inspected-movement-field) opens the adjacent direct Full
sheet and returns to the same enemy cell. These input runs use state, projections and errors.

The first layout capped the body's width at zero. The capacity probe failed whole-slot visibility,
`member-large` failed the retained-member click, and `member-field` failed parallel reading columns.
Removing the horizontal cap corrected all three boundaries. Capacity then caught a 44px accounting
value in a 48px row. Filling the row vertically restored the shared accounting alignment.
The corrected observers passed. The registered empty-roster case also returned to Options without a
selectable slot. The Map66 and empty-roster processes each reported two ObjectDB instances at
shutdown. Verbose output identified `AudioStreamWAV` and `AudioStreamPlaybackWAV`.
[Audio lifetime boundary](../verification/host.md#audio-resource-lifetime-observation).
Physical-device and human comfort remain **Unknown**.

#### Native member input

Use the [shared roster commands](#reproduce-the-shared-roster) with the locked Debug adapter.
For `member-large` select `SF2_INSPECTION_ROUTE=pad` and select the following English
version-1 settings. This exercises the installed mapping and an unmapped default UI key.

```json
{"formatVersion":1,"language":"en","textMode":"instant","bindings":{
  "up":   {"keys":["F7"], "buttons":["DpadDown"], "axes":[]},
  "down": {"keys":["F6"], "buttons":["DpadUp"],   "axes":[]}
}}
```

**Confirmed.** In the 14-member battle, Down on the unmapped keyboard arrow leaves focus on the
first row. Pressing a different row retains the current actor until release. Slot four regains actual
viewport focus after host Menu Back and retains it while the roster wheel scrolls and a mapped Right wraps to the first actor. The
previous arrow returns to the last actor with slot focus intact. Clicking Spells transfers focus
to its reader. The held-direction footer return stays on Options, and releasing an older member
press over the returned board leaves Cursor in place. The observer compares revision, turn,
positions, inventories and both seeds with the initial battle signature. The registered empty
roster can cycle panes and return to Options.

Use [Map66](#reproduce-registered-portraits) with the Chinese catalog and pad selection above.
After real Prepare Back, the observer detaches and reattaches the same member layer. It reads
the actual focused row and the summary texture borrowed from that row, and checks the window
subscription count. Window exit during an arrow press cancels it. One fresh click switches to
the next member, and a complete right-click over that arrow closes the sheet.

For appearance, use the [production preview command](#reproduce-the-native-member-layout).
At 1600×900, English and Chinese F8 show the long name, retained conditions, all four slots and
the fourth slot's effect. The second empty slot clears that text. F5 shows the selected row,
accounting and separate Prepare and Close actions. Ordinary gameplay checks use input and state.

The pilot rejected an invalid input-settings field. A clean scratch
needed locked restore before the Debug build. The corridor fixture requires the actor's top-level
`move` and nested `physical.movementType`. An extended member observation initially expected
slot four after changing actors in Spells. Re-enter Items and select slot four before testing the
wrap to an empty slot. Host Menu Back initially restored slot focus but logged a Godot `_can_process` error while the
host propagated its process mode. The page now restores focus on its next processing frame.
These corrected cases passed. The registered empty roster and Map66 can
report the known two-object audio shutdown warning. [Audio boundary](../verification/host.md#audio-resource-lifetime-observation).
Live membership removal during a paused read and physical-device comfort remain **Unknown**.

#### Reproduce registered portraits

Use the same Debug adapter, disposable saves, original graphics mode, and fresh output/settings
from the authored recipe. Load `local/private-inputs.ps1` in the launch process. Select the
registered world, entry and battle scenes in place. Each case is an independent controlled start
through the existing host observer, with selection and Back driven by ordinary mapped keys.

From the repository root, select `$case` below, use an absolute `$start` path under this
checkout's ignored `local/`, and copy the selected initial input there.
Use the registered opening party. For the named Battle02 variant, set `start.names` to
`{"ally-0":"WWWWWWW"}` in the ignored copy, select Chinese settings and use the pad route.

| `$case` | Initial input | Input and observation |
| --- | --- | --- |
| `member-empty` | Direct `battle` [start](battle-interaction-baseline.md#direct-production-reproduction) with `start.activeAllies = ["ally-0","ally-1","ally-2"]` | The direct battle has deployed allies and empty joined/active membership flags. Open Member, cycle the reading panes, confirm, then return to Options with the battle signature intact. Use English settings. |
| `member-field-portraits` | `remake/reference/inputs/yeel-to-battle03-start.json` | From settled Map66 Party, select two members, open Prepare and Back, then close/reopen Party. Read the selected row and summary textures directly. |
| `member-battle-portraits` | `remake/reference/inputs/engine-battle02-egress-start.json` | Accept the initial Battle02 entry, open Member, switch rows and close/reopen Member. Return to Main and check hover, press, window exit and stale release on Stay. |

```powershell
$pack = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2177/pre-voyage-battle-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $pack 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $pack 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $pack 'entry/entry.json'
Get-ChildItem -LiteralPath (Join-Path $pack 'opening-inputs') -File -Filter 'SF2_*.json' |
    ForEach-Object { [Environment]::SetEnvironmentVariable($_.BaseName, $_.FullName, 'Process') }
$env:SF2_INSPECTION_START = $start
$env:SF2_INSPECTION_ROUTE = $route  # keyboard, or pad for the named variant
$env:SF2_VIEWPORT_CASE = $case
$env:SF2_VIEWPORT_OUTPUT = $report
Push-Location remake
& $env:GODOT_BIN --verbose --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path game --log-file $nativeLog --script res://probes/engine_battle_member_observation.gd -- `
    --input-settings $settings
$nativeExit = $LASTEXITCODE
Pop-Location
```

**Confirmed, #2154.** Connected Battle02 retained the seven-character player name on both sides
of the page and borrowed the selected row portrait for the detail sheet. The Map66 field route
opened actual Prepare actions and returned to the same portrait. A first launch supplied the tracked
start directly and received `observation-input`. The worktree-local copy above passed.

The direct empty-roster start initially omitted its deployment selection and received
`ally-formation`. With the three listed allies, the page showed its localized empty message,
no actor, selectable slot or stale description. Back returned to Options. The run exited 0
with an ObjectDB shutdown warning (two instances).

The first Battle02 run passed the input assertions but logged two errors disconnecting the
`mouse_exited` signal on reparent and exit. #2152 had subscribed in `_Ready` and removed the
handler on each `_ExitTree`. Pairing the subscription in `_EnterTree` with that removal restored
hover and press after reparent. The native check delivered window exit during the press and
verified release left Stay uncommitted. The corrected named run exited 0 without disconnect
errors and reported an ObjectDB warning (four instances) at shutdown.

**Earlier #2082 native observation.** Both cases exited 0 with `passed:true` and empty
failures and unavailable lists. The sampled rows contained visible 48×64 textures in 48×72
controls. Two selected members used different textures. Field's 144×192 summary shared the
selected row texture through Prepare/Back and used the first row's texture after reopening.
Battle selected the matching member's sheet and restored the row's live texture and focus
on Back. Reopening mounted the first row again. No disposed texture, adapter or script errors
were reported. The battle process retained the known two-ObjectDB warning at shutdown.
Verbose output identified `AudioStreamWAV` and `AudioStreamPlaybackWAV`.
[Audio lifetime boundary](../verification/host.md#audio-resource-lifetime-observation)

Two initial attempts exited 1 before a portrait checkpoint. The old separate party input
returned `observation-party-rejected` in this package. Using the registered party at (29,32)
entered Battle02 immediately and hit `Deadline: settled field before Battle02`. The two starts
above isolate the actual field and battle callers. These observations cover original graphics
mode; HD and human visual/device review remain outside this check.

### Reproduce the inspected movement field

Build the Debug adapter and use the isolated environment and English standard input settings
under [Reproduce slice 1](#reproduce-slice-1). Use a fresh log and report destination and a
disposable save namespace. Copy `remake/content/authored/stone-court.json` to ignored scratch.
Add the `x` legend symbol with surface `impassable` and protection `none`. Set the terrain rows to the layout below.

```text
#########
#########
#########
#ppppxpp#
#########
#######p#
#########
```

Place swordsman at (2,3), lookout at (3,3), raider at (7,3) and scavenger at (6,3). Keep the
sentry at (7,5). Set raider's `physical.movementType` to `hovering` and `move` to 4.
Select this authored package with the host's ordinary diagnostic launch option.

Run from `remake/` with absolute report, log, package and settings paths.

```powershell
$env:SF2_VIEWPORT_CASE = 'inspected-movement-range'
$env:SF2_VIEWPORT_OUTPUT = $report
$env:SF2_INSPECTION_ROUTE = 'keyboard'
$env:SF2_INSPECTION_PRIVATE = '0'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path game --log-file $nativeLog --script res://probes/engine_battle_inspection_observation.gd -- `
    --authored-package $package --input-settings $settings
```

Run with `reducedFlash` set to false and true in the input settings. Use fresh report and log
destinations for each run. The [observer](../../game/probes/engine_battle_inspection_observation.gd)
requires exit 0,
`passed:true`, empty failures and unavailable lists, and clean engine/adapter logs. It uses
actual mapped keys for ground ally, hovering enemy and empty terrain. The recorded draw
distinguishes same-faction traversal from legal stops for both sides. It checks clearing and
return through details, Options, Map, Members, host Menu and ordinary Movement, with the same
session, revision, selection, queue, actor state and seeds throughout. The focused
`BattleInspectionQueryTests` also check a changed position and Move on a fresh snapshot,
stale bindings, and admission of living disclosed actors. Human visual/device review remains
with #1456. Exact original cursor/range timing and pixel parity remain **Unknown**.

## Fresh target input and direct Stay

**Standard** implementation under [#2080](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2080).
One mapped direction or Target shortcut selects once among live offers on the Attack, Magic and
Item Target page. Keyboard echoes and repeated D-pad presses cannot cycle it. A mapped stick
axis rearms after a sample below 0.35 magnitude. A held deflection or a sampled sign change without
neutral cannot cycle it again. A fresh press on a different source can select independently.
Rapid taps keep the existing order, wrap, initial legal target and explicit Confirm. Target
entry, exit, Back and host cover clear carried navigation and pending pointer gestures. Candidate
updates stay within the same Target phase. Movement keeps its on-arrival repeat. Main, lists and
free inspection keep their existing repeat rules.
[Navigation](../../game/src/Battles/BattleSessionView.Navigation.cs) ·
[Input resolution](../../game/src/Input/GameInput.cs)

**Confirmed, original static source.** At pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the menu calls `ExecuteDiamondMenu`.
With no chest (`CHEST_CONTENTS == -1`), `@CheckChoice_SearchStay` writes `BATTLEACTION_STAY`
and exits control. `@CheckBattleaction_Stay` then takes the no-action suffix. The faced chest
takes a separate branch.
[Menu and Stay branch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L952-L964) ·
[Turn continuation](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm#L166-L172)

**Confirmed, remake behavior.** A fresh Confirm on Stay, its mapped shortcut, or a complete
Stay click sends one `CommitBattleStay` at the legal preview stop. The session uses the existing
admission, commit and after-turn continuation in that submission. Explicit staged action callers
can still select Stay and submit Confirm. The ordinary presenter retains the decision and reason
on refusal. The triggering input cannot operate the next actor or acknowledge an after-turn
notice. Main Back before choice [resumes Movement at the preview](#main-back-and-movement-reselection). A faced chest substitutes
Search with its own confirmation and result. Give and Drop keep their own confirmation and result
handling.
[Command presenter](../../game/src/Battles/BattleCommandMenu.cs) ·
[Dispatcher](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs)

### Reproduce target navigation and Stay

Use the SDK, Debug build, headless Godot and isolated output setup under [Reproduce slice 1](#reproduce-slice-1).
Use the same English standard bindings and instant text. For the target routes, prepare three
ignored copies of `remake/content/authored/battle-give.json`. Give `champion` the
`unpromoted-priest` class, 190 HP, 12 maximum and initial MP, and Care levels 1–3. Copy the Care 1
spell from `garden-watch.json` for each level and use class masks of 17 for both weapons. Clone
`reserve` as `reserve-2` in the actor definitions, start vitals and encounter placements with
processing order 155. Use the following foe placements. Keep the allied positions and Herb slot.

| `SF2_BATTLE_NAVIGATION_AUX` | `SF2_BATTLE_NAVIGATION_ROUTE` | Authored package |
| --- | --- | --- |
| `fresh-targets-2` | `keyboard`, then `pad` and `axis` | Copy with `reserve` at (1,1) and `reserve-2` at (1,3) |
| `fresh-targets-1` | `pad` | Copy with `reserve` at (1,1) and `reserve-2` at (6,5) |
| `fresh-targets-0` | `keyboard` | Copy with `reserve` at (7,5) and `reserve-2` at (6,5) |
| `direct-stay-confirm` | `pad` | Unchanged `garden-watch.json` |
| `direct-stay-click` | `keyboard` | Unchanged `garden-watch.json` |
| `direct-stay-shortcut` | `pad` | Unchanged `garden-watch.json` |
| `direct-stay-notice` | `keyboard` | Unchanged `poison-turn.json` |
| `confirm-give` | `keyboard` | Unchanged `battle-give.json` |
| `confirm-discard` | `keyboard` | Unchanged `battle-discard.json` |

Set both variables from the row. Run from `remake/` with absolute report, log, package and settings
paths. Use a fresh report and native log for each case.

```powershell
$env:SF2_BATTLE_NAVIGATION_OUTPUT = $report
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path game --log-file $nativeLog --script res://probes/engine_battle_navigation_observation.gd -- `
    --authored-package $package --input-settings $settings
```

The [observer](../../game/probes/engine_battle_navigation_observation.gd) bounds each run to 35
seconds. Require exit 0, `passed:true`, an empty failures list and clean script/runtime logs.
It submits actual mapped Godot key events with echoes, device-0 D-pad buttons and signed axes
with dead-zone samples, plus complete and interrupted mouse gestures. It reads live revisions,
candidates, parents, decline reasons, movement stops, action receipts and notice state. Physical
device feel and OS delivery remain with #1456. Use the existing [held preview](battle-interaction-baseline.md#held-preview-step-handoff),
[Search](../development-and-verification.md#battle-search-observation),
[Give](../development-and-verification.md#battle-give-authored-observation) and
[Drop](../development-and-verification.md#battle-discard-authored-observation) routes for the
neighboring boundaries.

## Battle decision projection (#2107)

**High rigor.** Migrate command, target and return behavior together because mapped input, board
activation and visible affordances share the same eligibility. Keep the vertical Main, existing
gameplay rules, separate Search and transaction confirmation, movement/inspection repeat and
delivery ownership. [Scope and later migrations](ui-architecture.md#migration-and-independent-acceptance)

`BattleSessionView` derives one `BattleDecision` from the live snapshot, enabled offers and local
focus. The menu consumes it for captions, hints and hit eligibility. The board uses it for the
target card and marks, and the root takes its fresh direction policy from the presenter. Rows and
the parent list are local. The application still decides action admission, selected target, costs,
result and each completion token.

Magic or Item action entry publishes the action and, where eligible, the initial target as two
ordered submissions. The presenter projects the final candidate after both receipts. Back sends
one return command and projects the originating row with the confirmed stop retained. An illegal
cell clears the current candidate while the session can retain a previous target. Mapped Confirm,
footer Confirm and board release all use the same confirmation. A press on an already selected
board cell reuses the selection. The release issues only the shared Confirm. Wheel and middle
buttons leave battle targeting and inspection focus alone. The wheel scrolls an overflowing reader under the pointer. Main and child lists also read with the active Left/Right mappings.
Right Back uses the menu's matched pointer gesture. Host cover and viewport resize invalidate
incomplete gestures. [Decision projection](../../game/src/Battles/BattleDecision.cs) ·
[Transition and pointer owner](../../game/src/Battles/BattleSessionView.Decision.cs)

### Reproduce the battle decision

Use the [target fixture and native launch](#reproduce-target-navigation-and-stay) at 60 FPS, with
`fresh-targets-2` for `keyboard`, `pad` and `axis`, and `fresh-targets-0` for `keyboard`. The
navigation observer records action entry and Back receipts alongside actual control visibility,
selection and revision. The keyboard case includes wheel and middle over two kinds of board cell,
an illegal click, a legal selection, exact Magic and Item return, inspection, a held direction at
host close and a captured Confirm across a resize out and back. The zero-target Attack offer is
declined in Main. The Target page's empty-set branch is covered by source inspection because normal
action entry requires an enabled offer.

Use the same navigation script for `direct-stay-confirm`, `confirm-give` and `confirm-discard`
with the packages in the table above. For held movement, run `handoff` with unmodified
`garden-watch.json`, keyboard and 60 FPS. Run the [Search observer](../development-and-verification.md#battle-search-observation)
with unmodified `battle-search.json` to see the Search row, its Cancel return, and the real receipt
and completion. For close-up cover and resize, use the private Battle01 setup under
[Reproduce slice 1](#reproduce-slice-1), select the registered pre-voyage BLAZE1 publication and
set `SF2_CURSOR_CLOSEUP_ONLY=1`. This selects the attack and follows its existing message wait
through host cover, resize, return and actual completion.

## Shared description reading and inspection return

[#2113](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2113) and
[#2085](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2085) share the
`ItemDescriptionView` viewport between battle Main/child lists and field Prepare descriptions.
Mapped Left/Right and a wheel over the body scroll the actual overflow. The visible Read hint
follows the active bindings and disappears when the body fits. Up/Down still selects the
command or row. On Target, directions still choose a recipient once per fresh input. The footer
shows the short action identity, applicable refusal and current controls. Prepare's described item
gets the mapped reading request while it is present; the pack no longer scrolls along with it.
Resuming the same battle page preserves scroll and focus. A different page, acting member or
focused offer starts at the top.

Cursor and its unit detail use the inspection footer’s Back button. Options and Map show the
mapped return in their instructions. Members uses the shared page footer and its Back target. Mapped Cancel, a complete right-click and an eligible Back button follow the
same local parent and retain cursor or roster focus. The footer shields the board from left
clicks. Cursor shows the shared unit card on occupied cells and keeps the terrain panel on empty ground.
Resizing or changing page cancels an old capture.

Movement, Target, hover inspection and playback use a project-authored double-gold frame
with stepped corner accents. The mini-status composition puts the name and admitted ally
class and level in the header, then stacks HP and MP with aligned bars and right-side values.
Movement and inspection display coordinates and land effect in a dedicated panel opposite the unit card. During movement it follows the last delivered tile. During inspection it follows the cursor. Target selection and playback omit this terrain readout. The game preserves the live
resource and participant boundaries. This composition follows the placement of information in
the [pinned mini-status routine](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/ministatuswindow.asm#L572-L664).

### Proposed terrain panel placement for playtest

The current layout places the terrain panel across the board from the unit card. At the
1920×1080 canvas width, reading one unit's vitals and land effect requires looking at both
sides of the battlefield.

For the next layout trial, keep the two frames distinct and align the terrain panel with the
leading edge of the unit card, **12px below it**. Keep the round counter in its current column.
This preserves the visual link between the unit and its cell and leaves room in the round
column for the battlefield. A side-by-side pair would extend farther over the playable map.

Use the combined bounds of both panels when choosing the side that clears the focused cell.
In F4 the terrain row takes vertical space above Main or the spell/item list. Check the longest
two-line unit name, all four Main rows, the selected list row and the fixed description footer.
In F8, keep the same column when moving to adjacent empty ground where possible. Show just the
terrain frame there and keep it clear of the cursor. Target selection and playback retain their
existing disclosure. Review both languages at 1600×900 and 1920×1080 before adopting the placement.

### Reproduce shared description reading and inspection return

Use the registered SDK, Debug adapter, isolated app data/TEMP and explicit disposable save scope
from [Reproduce slice 1](#reproduce-slice-1). Run the native commands with a fresh report and log
for each row. The table describes the controlled content and actual input surface.

| Observation | Existing launch and selection |
| --- | --- |
| Long battle description and narrow client | Run the public wide-vitals case in [Reproduce slice 1](#reproduce-slice-1) at 1600×900 once with `SF2_CURSOR_LANGUAGE=en` and once with `SF2_CURSOR_LANGUAGE=zh-CN`, matching the input settings. The observer extends the respective range and no-items strings before mounting the host. Keyboard, mapped D-pad and wheel move only the reader in Main/Magic. The check covers host cover with a direction held across Resume, row reset, short Target, and no revision, selection or resource change from reading. |
| Visible returns, both locales | Use [inspected-movement-range](#reproduce-the-inspected-movement-field). The observer clicks instruction space and the actual button across details, Map, Options and Members, and resizes between press and release. It checks live range/cursor ownership and the unchanged gameplay signature. |
| Retained roster | Run [member-large](#reproduce-the-shared-roster) in English. Navigate to the final row, hold a pointer press in the sheet, return with mapped Cancel, and release over the row. The roster keeps the selected actor and exact scroll. |
| Field Prepare | Run the `party` case from [Observe the selected item pages](ui-design-language.md#observe-the-selected-item-pages) with `SF2_ITEM_DESCRIPTION_LONG=1`. The observer extends the weapon description before the host mounts. In Equip, mapped Right moves only the body. The adjacent short consumable retains the row geometry with no overflow hint. |
| Actual card mount | Use the Battle01 controlled start from [Reproduce slice 1](#reproduce-slice-1) with the registered pre-voyage BLAZE2 publication and `SF2_CURSOR_CLOSEUP_ONLY=1`. The observer opens a friendly HEAL Target, returns without payment, then commits the physical attack. It verifies the allied and enemy cards during the real message wait and reflows the covered scene before resuming delivery. |

Physical controller feel and human visual acceptance remain **Unknown** under #1456.
The linked owners cover Target rule geometry.

## Selected spell descriptions

[#2083](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2083) is a Standard
slice because battle and field display the same learned identity with different admission and
payment rules. Both lists render `SpellDescriptionFacts` for the exact `SpellRef` in the
fixed `ItemDescriptionView`. The adapter formats those facts in English and zh-CN using the
same descriptions and existing name catalog.

Battle choices project the admitted effect, target side, reach and area. Self and all-allies
actions describe their own center selection. Damage wording names the effect without calculating
damage. The MP cost stays in the row. When insufficient MP prevents admission, the reader still
shows the registered level's effect and actual refusal. An unbound effect stays explicit.
When a selectable cast is suppressed by Silence, the reader explains the paid suppression.
EGRESS follows the retreat rule and can select self while silenced.

Field choices use the actual field binding. HEAL can be selected here and resolves to free
NoEffect. Spells without a field binding stay outside the field offer list. DETOX names the
conditions this level clears and the party-member target. The reader shows the actual deduction
when available MP is below the nominal row cost. EGRESS describes refuge return when admitted.
Its map-specific free branch and current route refusal remain distinct. The command still
validates the chosen recipient or destination on confirmation.
[Battle offers](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) ·
[Field recovery](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs) ·
[Shared facts](../../src/Sf2.Remake.Application/Runtime/SpellDescriptionFacts.cs)

The row keeps label, level, icon and cost. Changing the selected spell resets the reader inside
the same bounds. Battle Target replaces the body with the short spell identity and ordinary
target hints. Field Target puts the identity below its target prompt and releases the reader.
Back restores the exact level and description. Focus and reading leave gameplay state, MP and
both RNG streams untouched.
[Battle layout](../../game/src/Battles/BattleMenuLayout.cs) ·
[Field preparation](../../game/src/Exploration/FieldActionView.cs) ·
[Reader](../../game/src/UI/ItemDescriptionView.cs) ·
[Formatter](../../game/src/Localization/SpellDescriptions.cs)

### Reproduce selected spell descriptions

Use the registered SDK, Debug adapter and checkout-local app data/TEMP setup in
[Reproduce slice 1](#reproduce-slice-1). Set `$review` to a fresh absolute directory under
`local/` per run. The spell observer uses read-only startup. Clear `SF2_SAVE_ROOT` and
`SF2_SAVE_NAMESPACE` after loading local configuration and leave `--save-output` unset.
Set `$language` to `en` for 1920×1080 or `zh-CN` for 1600×900.

For battle, start from the tracked authored `attack-spells.json`. This variant uses its first
two ranks, the registered healing and retreat shapes, and a mage with Silence. Use 20 MP in
English and 5 MP in Chinese. From the checkout root:

```powershell
$env:SF2_DEMO_BATTLE_CASE = 'spell-battle'
$env:SF2_SPELL_DESCRIPTION_LOW_MP = [string][int]($language -eq 'zh-CN')
$case = Get-Content -LiteralPath remake/content/authored/attack-spells.json -Raw | ConvertFrom-Json -AsHashtable
$case.start.actors[0].status = 'silence3'
if ($language -eq 'zh-CN') { $case.start.actors[0].mp = 5 }
$case.spells += '[{"id":"mend","level":1,"mpCost":2,"minimumRange":0,"maximumRange":1,"effect":{"kind":"heal","adjustedPower":4,"fullRecovery":false}},{"id":"egress","level":1,"mpCost":8,"minimumRange":0,"maximumRange":0,"effect":{"kind":"retreat"}}]' | ConvertFrom-Json -AsHashtable
$case.actors[0].spells = @($case.actors[0].spells[0], $case.actors[0].spells[1], @{ id='mend'; level=1 }, @{ id='egress'; level=1 })
$package = Join-Path $review 'authored.json'
$case | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $package -Encoding utf8NoBOM
$launchInput = @('--authored-package', $package)
```

For field, select the published [matching world and opening inputs](../content/presentation-assets.md#reuse-a-closed-private-package).
Set `SF2_DEMO_BATTLE_CASE=spell-field`. Copy the registered
`handoffs/issue1847-formation/slots/rescued-en.json` to `$review/seed.json` and set
`$launchInput = @('--load-save', (Join-Path $review 'seed.json'))`. This is the published
[poisoned-party witness](../domain/detox-ranks-design.md#ordinary-input-and-cold-continue).
It uses title Continue to mount the copied seed without an external writable save namespace.

For either case run:

```powershell
$env:SF2_INSPECTION_ROUTE = 'keyboard'
$env:SF2_SPELL_DESCRIPTION_LANGUAGE = $language
$env:SF2_VIEWPORT_OUTPUT = Join-Path $review 'observation.json'
$settings = Join-Path $review 'settings.json'
@{ formatVersion=1; language=$language; textMode='instant'; reducedFlash=$true; bindings=@{} } |
    ConvertTo-Json | Set-Content -LiteralPath $settings -Encoding utf8NoBOM
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --quit-after 6200 --path remake/game `
    --log-file (Join-Path $review 'native.log') --script res://probes/engine_spell_description_observation.gd -- `
    --input-settings $settings @launchInput
```

Require exit 0, `passed:true`, empty failure and unavailable lists, and inspect the native log.
The observer extends one line per locale to force overflow. The battle cases compare two levels,
single and area reach, insufficient MP, paid Silence suppression, unsupported healing and
self-targeted EGRESS. Keyboard, synthetic mapped pad and wheel scrolling preserve the complete
gameplay signature. Field reads HEAL's free branch and DETOX 2, returns from Target and
confirmation, then checks one actual cure and MP deduction with unchanged RNG. Low-MP clamping
is covered by the focused [field recovery tests](../../tests/Sf2.Remake.Engine.Tests/FieldRecoveryTests.cs).
The field observation reports a two-instance ObjectDB warning during shutdown and exits 0.
[Shutdown tracking #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

Physical-device and human visual acceptance remain **Unknown** under #1456.

## Current-round order (#2065)

[#2144](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2144) withdraws the action
preview strip from the shared battle view and defers its redesign. The battlefield uses the space
below the round and terrain stack. The acting unit still has its status card and movement focus.
DesignPreview's Battle and Inspect tabs mount the same production battle view.
[Layout owner](../../game/src/Battles/BattleMenuLayout.cs)

`GameSession.QueryBattleRoundOrder` reads the queue from the cursor through the first null-actor
sentinel or the end. It shares dead, unplaced, sleeping and stunned classification with
`BattleAdvancer` and board disclosure with inspection. Dispatch owns skip effects, AI and RNG.
The battle save stores the queue and cursor. Engine cases compare eligibility with dispatch and
exercise duplicate entries, a fresh round and cold restore of a partly consumed queue.
[Order query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRoundOrderQuery.cs) ·
[Turn advancement](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
[Engine cases](../../tests/Sf2.Remake.Engine.Tests/BattleRoundOrderTests.cs)

The prior display and its native reproduction are recorded at
[accepted #2125](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8240a449bc334987bf05efa3ca7a28074c371a5c/remake/docs/godot/battle-ui-composition.md#current-round-order-2065).

### Observe the battle HUD without the strip

Use the Debug build, isolated settings and native launch under [Reproduce slice 1](#reproduce-slice-1).
Select the unmodified `remake/content/authored/attack-spells.json` as the authored package.
Substitute `res://probes/engine_battle_round_order_observation.gd` as the script. Use
`SF2_INSPECTION_ROUTE=keyboard` for English or `pad` for Simplified Chinese with Reduced Flash.
For the DesignPreview Battle tab, pass `--battle` after the Godot argument separator in place
of the package and settings arguments.
[Native observer](../../game/probes/engine_battle_round_order_observation.gd)

**Confirmed, bounded remake.** Ordinary Main starts Movement and free inspection at 1920×1080.
The board receives mouse hover in free inspection, and returning restores the same live battle
state. Mapped movement reaches a legal adjacent square. At 1600×900 the Attack row click opens
Target and a board click starts delivery. The strip has no mounted control or layout slot in
any of these phases. The surviving decision panels fit and the acting or recipient card appears
as applicable. The DesignPreview Battle tab shows the same strip-free Movement layout.
Physical device and human review remain **Unknown** under #1456.

## Implementation handoff

Main-gate owns acceptance and dispatch. Slice 1 settles composition. Slices 2 and 3 share battle
session input/presentation paths with it and with each other, so schedule their writes serially.
Slice 4 consumes the reader and phase layout from slice 1.
Serialize any overlap with other writers of its battle layout or trace. Declare exact paths
before each implementation. The following are principal owners and distinct acceptance outcomes.

| Slice | Player behavior and principal owners | Acceptance and risk |
| --- | --- | --- |
| 1. [#2079](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2079) Board focus and decision layout | One cursor/field/card owner per phase, the #2079 Main diamond (later corrected by [#2090](#vertical-battle-main-2090)), stable child and footer regions, auxiliary and close-up retirement. `BattleSessionView`, `BattleMapViewport`, range/cursor views, `BattleCommandMenu`/`BattleMenuLayout`, `BattleInspectionView`, `BattleSceneView`. | Short real Move → command → Attack/Magic/item target → Back → Movement and inspection/options/return. Include a rejected mouse target, ally crossing, multi-target and field delivery. Inspect visible/hit rectangles, focus, map clipping and errors in both languages, at supported minimum/1080p/larger wide viewport, with wide values/long text and Reduced Flash. Verify private source cursor and public fallback. Resize a retained close-up token while paused and check art/card positions and unchanged delivery on return. |
| 2. [#2080](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2080) Target edges and ordinary Stay | One selection per fresh direction, one gesture to Stay, continuous Movement. `BattleSessionView.Navigation`, `BattleCommandMenu`, current `GameInput` release tracking and `BattleCommandDispatcher`. | Keyboard and pad/d-pad or stick single target and multiple targets for physical, Magic and Item. Compare hold, neutral/repress, fast taps, menu/move → target with inherited direction, target Back/reentry, host cover, and continued held movement. Origin and moved Stay commit once on key/pad/click. Exercise next actor or after-turn delivery while holding Confirm. Search and Give/Drop confirmations, refusal and backwards routes still work. |
| 3a. [#2081](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2081) Inspected movement | Delivered through `QueryBattleInspectionRange`, `BattleInspectionView` and the shared board overlay. | [Authored corridor observation](#reproduce-the-inspected-movement-field) checks ally → enemy → empty, traversal versus stops, details/Map/Members/host transitions, cursor exit and state/seed equality. |
| 3b. [#2082](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2082) Shared member presentation | [#2154](#shared-member-page-2154) composes the roster, reading panes and footer in the same production page for both callers. | [Short authored observations](#reproduce-the-shared-roster) cover roster size, long names, retained conditions, pointer and mapped focus, Back/scroll and field Prepare. |
| 4. [#2083](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2083) Describe spells by level and context | Implemented through `BattleChoices`, `FieldRecovery`, `SpellDescriptionFacts` and the shared localized reader. | [Battle and field observation](#reproduce-selected-spell-descriptions) covers two ranks, area, self, refusal, cancellation, free field use and recovery payment. |
| 5. [#2144](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2144) Withdraw the current-round strip | The shared battle HUD leaves the strip unmounted. Redesign is deferred. | [Native HUD observation](#observe-the-battle-hud-without-the-strip) covers ordinary input, phase geometry and the DesignPreview Battle tab. |

[#2068](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2068) owns formal BLAZE
playback assets and shares the playback writer only as needed. The [field Prepare command rows](field-and-shop.md#prepare-command-icons)
use the common field list and current Back path. [#2070](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2070)
owns special dialogue portraits, [#2073](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2073)
owns story wait/Confirm, and [#2062](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2062)
owns pickup glints. Schedule shared-path work around those owners.

## Reproduce slice 1

Use the [locked SDK workflow](../development-and-verification.md#locked-net-workflow), the registered
Godot installation and a fresh checkout-local output directory. Load `local/private-inputs.ps1`
in the command process, create its checkout-local TEMP directory if needed and set
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Run `uv run sf2 verify adapter` from repository root.
Build Debug from `remake/` with `& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.

Run the [cursor observation](../../game/probes/engine_battle_cursor_observation.gd) with fresh
APPDATA, LOCALAPPDATA and TEMP/TMP, a disposable `SF2_SAVE_ROOT` and a unique `SF2_SAVE_NAMESPACE`.
Set `SF2_VIEWPORT_OUTPUT` to a fresh absolute report path and `SF2_INSPECTION_ROUTE=keyboard`.
Use UTF-8 input settings with `formatVersion` 1, `confirmCancel` standard, `textMode` instant,
`charactersPerSecond` 24 and empty `bindings`.

| Case | Selection before launch |
| --- | --- |
| Private Battle01 | Select the registered `derived/issue2035/galam-town-v1` world, entry, main-scenes and six opening files as described in the [publication owner](../content/presentation-assets.md#final-publication-and-ordinary-selection). Set `SF2_DEMO_BATTLE_CASE=private`, `SF2_CURSOR_ORIGINAL=1` and `SF2_CURSOR_SHA256` to the decoded identity in [cursor publication](battle-scenes.md#source-and-publication). Set `language` to en and `reducedFlash` to false. Set `SF2_COMPACT_START` to the controlled start below. Use 1920×1080. |
| Public long text and wide vitals | Set `SF2_DEMO_BATTLE_CASE=public`, `SF2_CURSOR_ORIGINAL=0` and clear `SF2_COMPACT_START`. Set `language` to zh-CN and `reducedFlash` to true. Copy `remake/content/authored/attack-spells.json` to run scratch. Change the mage maximum HP to 30,000 and initial HP to 29,999. Rename its actor ID, start actor reference and encounter placement reference to `very-long-controlled-mage-name-for-wrap-and-five-digit-vitals`. Keep its physical movement type. Widen the terrain to the left by eight cells, extending the barrier on the top and bottom rows and inserting eight open cells after the left barrier of each interior row. Shift every placement eight cells right to exercise right-edge target clearance. Pass that copy as the authored package. The observer extends one localized help string before mounting the host. Use 1600×900. |

| Public area completion | Set `SF2_DEMO_BATTLE_CASE=area`, `SF2_CURSOR_ORIGINAL=0` and clear `SF2_COMPACT_START`. Use English and Reduced Flash and select the unchanged `attack-spells.json` at 1920×1080. |

The private controlled start declares the active lineup. Save it under the checkout's ignored
`local/` as UTF-8 JSON.

```json
{
  "start": {
    "battle": "battle-1",
    "activeAllies": ["ally-0", "ally-1", "ally-2"],
    "actors": [
      {"actor": "ally-0", "hp": 6, "position": {"x": 9, "y": 14}},
      {"actor": "ally-1", "position": {"x": 8, "y": 14}},
      {"actor": "ally-2", "hp": 0},
      {"actor": "enemy-0", "hp": 1, "position": {"x": 8, "y": 13}}
    ]
  }
}
```

Run from `remake/` with absolute settings and log paths. Set `$packageArguments` to an empty
array for Battle01 or `@('--authored-package', $packagePath)` for either public case.

```powershell
& $env:GODOT_BIN --headless --path game --audio-driver Dummy --max-fps 120 --resolution $resolution `
    --log-file $nativeLog --script res://probes/engine_battle_cursor_observation.gd -- `
    --input-settings $settings @packageArguments
```

Use the observer's 50-second deadline and a 55-second wall bound. Require exit 0, `passed:true`,
empty failures and unavailable lists, and inspect stderr.

The source-art route uses actual preview and undo, vertical Main focus, a full caption click,
HEAL/Attack/Item Target and Back, a rejected board cell, and a complete legal attack click.
The inspection route covers both map edges, Details, Options and host cover. The private route
checks native wide relayout and a paused close-up resize with stable delivery. The public routes
cover the fitted 1600×900 canvas, authored corners, wide vital values, long text scroll with fixed
navigation, Reduced Flash, and ordered BLAZE2 recipients with one MP payment.

Human readability, device feel and visual approval stay with #1456. Original pulse parity and
natural source reach beyond the linked evidence remain **Unknown**.

## Readable battle status cards

[#2145](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2145) applies the two-size
[gameplay typography](ui-design-language.md#gameplay-typography) agreement to the shared card.
Movement, Main, list and target decisions, free inspection and close-ups show names and vitals
at 30px. Ally cards show class and level on their own 30px line when available. Enemy cards omit
that line. The panel measures the wrapped name and allocates the required height. It widens up
to 800 design units for names. Exceptionally long metadata can use a two-line ellipsis at the
same font size. HP and MP share aligned labels, gauges and value columns. A zero maximum still
shows a dash and an empty gauge. Full details inherit the shared gauge typography in their
existing layout.

Use the locked Debug build and fresh native observation directories from
[Reproduce slice 1](#reproduce-slice-1). Run the same command with
`res://probes/engine_battle_vitals_observation.gd` at 1920×1080, using ordinary `Main.tscn`.
The observer needs no persisted settings or saves. Supply `--input-settings` for each locale.

| `SF2_VITALS_CASE` | Prelaunch input and route |
| --- | --- |
| `private` | Use the controlled private Battle01 start above with English settings. Inspect ally and enemy, open the real Main and Magic list, then select and click the admitted Attack target. Advance only to the first defeated enemy's card. |
| `zh-long` | Use the same Battle01 start with zh-CN settings. Select a small UTF-8 `SF2_DISPLAY_CATALOG` with the world package and repository/commit/ROM identity, locale `zh-CN`, empty `texts`, and two `names` rows. Use `name.ally` index 1 with `北境守望祭司与远行队医伊莉莎白西风谷巡旅护卫队首席治疗师` and `name.class` index 4 with `祭司`. Select a friendly HEAL target and observe single-card playback. |
| `wide` | Use the public authored wide-vitals recipe above in English. Also rename the center enemy to `very-long-controlled-enemy-name-for-wrap-and-five-digit-vitals` in its definition, placement and start row, and set its current/max HP to 29,999/30,000. Observe long-name wrapping and the live opposing BLAZE cards. |

Read the mounted text sizes, visible lines, role disclosure, gauge values and rectangles. The
target card is measured at 1600×900, 1920×1080 and 2560×1440. At 1280×720 the host's window
recovery covers battle, then 1920×1080 restores the pending target without acting. Clicks on the
inspection card stay out of the board, invalid target gestures leave resources alone, and a
complete legal target click enters playback. These checks cover the shared card and gauge.
