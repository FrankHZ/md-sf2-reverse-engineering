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
native acceptance. The implementation handoff tracks the delivered inspection movement and roster
and the remaining spell descriptions and queue. Original-game evidence and
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
| Battle entry, round program or appearance | Current scene, or the current message when it needs acknowledgement | Board and authored motion when delivery owns them | Existing continuation. Decision cards, command hints, queue and board selection marks are hidden. |
| Player movement or its delivered preview/return step | One cell cursor following the unit's displayed position through the delivered step | Acting card, round/gold, queue reservation, one movement field and movement hint | Mapped movement holds continue on arrival. Confirm uses the real legal stop. Cancel from an unsettled move finishes undo before opening inspection. While movement is delivering, keep the display, with input governed by its existing step boundary. |
| Main action | One focused row in the vertical command list | Acting card, round/gold, queue reservation, footer with consequence/refusal and Back | Initial Attack when enabled, otherwise the Stay/Search row. Up/Down wraps focus and Confirm or a complete row click chooses. Mapped shortcuts choose their existing command. Main Back returns through the real movement undo. Choosing Stay commits the current legal stop in one gesture. |
| Magic or Item | One row, keyed by spell and level or actual inventory slot | Acting card, round/gold, queue reservation, one selected-offer reader in the footer | Opening either child is available even with no usable offer. Child rows replace Main and keep focus visible while scrolling. A disabled row is readable. Choose an enabled row to enter target selection. Back restores the parent command. |
| Target, including self, single- or multi-target | One board cursor at the current legal candidate | Recipient card replaces acting card. The one action field shows reach and the current effect. The footer shows a concise action name, Confirm/Back and refusal when needed. Round/gold and queue reservation stay quiet. | Each fresh direction or Target shortcut cycles the live candidates once. Hover previews a candidate. A complete stable legal click uses the existing commit path. An invalid cell clears the cursor/card/effect and disables commitment even if the session still holds an older target. An invalid cell with eligible alternatives offers retargeting and Back. With zero eligible targets, the reason and Back remain. Back clears target state and restores the same command or item/spell row. |
| Search confirmation, Equip, Give/exchange or Drop | Current transaction choice, slot, recipient or confirmation | Acting identity where relevant, the one existing auxiliary reader and its own instructions | Existing eligibility, selection, read-before-confirm, receipt and return contracts. The transaction replaces the command/target reader, board cursor and range. |
| Free inspection, Cursor page | One board cursor on the inspected cell | One hover card, one framed instruction and return panel, and the inspected unit's movement field when a living unit is disclosed | Directions and board pointer select a cell for reading. Confirm opens details on a unit or Options on empty ground. Empty ground clears the range and still shows terrain. Cancel restores live Movement. |
| Inspection Options, Map, Members or details | The open panel's option, map selection, member row or detail pane | One framed instruction and return panel with data from the existing read-only query | Retain cursor coordinates and caller selection for Back, hide board cursor and range. Full → Cursor, MemberFull → Members, Map/Members → Options, Options → Cursor. An on-demand Map selection can locate its disclosed unit and return to Cursor. |
| Enemy action, close-up, field notice/death, result or battle exit | Actual delivered action or current message/return prompt | Playback cards for the actual participants, message reader, board only when owned by the continuation | Existing completion token and acknowledgement. All decision surfaces and board marks retire. Rebuild from live control after delivery. Field return uses field chrome. A failure shows once at its owning view. |
| Host Menu, Save, Speed or recovery cover | Host/modal's current choice or reader | Only the permitted covered backdrop | Suspend battle gestures, holds and pacing using existing ownership. Restore the same eligible caller, with its current layout and fresh input after release. A resize while covered may recompute geometry. |

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
quiet round/gold panel at the existing upper-left gameplay inset below host chrome and reserve a
left stack under it for the queue. Use the existing roughly 560px right-side decision column,
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
would cover that cell, keep the footer and card aligned in the alternate clear stack and suppress
the secondary queue for that placement if needed. Fit a bounded reader and scroll before
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

### Shared member roster (#2082)

[#2082](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2082) delivers the shared
`MemberRosterView` in battle Member and field Party. This **Standard** adapter slice reuses
the current content, input and detail owners. Each row binds an actor, localized name and condition,
and an optional portrait from the caller's admitted sprite art. Names wrap, focused rows carry a
marker, and unavailable rows stay readable in muted text. Mapped navigation reveals the focused
row, and the pointer wheel scrolls independently. A complete battle row click selects and tries
to open it. A field row click selects the member beside the sheet.

The list preserves the actor and scroll across details/Back or Prepare/Back. If refreshed membership
removes the focused actor, it searches the previous order for the nearest surviving row and reveals
it, preferring the following row on ties. If all prior rows disappear, it uses the former ordinal
clamped to the new list. A fresh entry starts at the first row.

Battle retains `QueryBattleMembers` and its active-force order. An initialized member who is
defeated or absent can open read-only `PartyDetailsPanel`; an uninitialized row is visible but
cannot open a sheet. The battle detail header gives wrapped names and class text their measured
height and leaves the existing stat, spell and item areas below. Field retains `QueryPartyDetails`,
including its own membership and Prepare eligibility. A field row with unavailable detail data
remains readable and uses the ordinary Prepare query. Direct inspection of a disclosed enemy
still opens the inspection sheet.
[Shared roster](../../game/src/UI/MemberRosterView.cs) ·
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

Generate the large battle package from the repository root with an existing ignored `local/issue2082`
directory. All normal deployment and input admission rules apply.

```python
import copy, json
from pathlib import Path

package = json.loads(Path("remake/content/authored/stone-court.json").read_text(encoding="utf-8"))
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
    initial.update(actor=actor["id"], hp=0 if i == 1 else 35, absent=i == 2)
    deployed.update(actor=actor["id"], x=1+i%7, y=1+i//7, processingOrder=i)
Path("local/issue2082/member-large.json").write_text(json.dumps(package), encoding="utf-8")
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

**Confirmed, bounded native observation.** The two- and fourteen-member battle routes passed.
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

#### Reproduce registered portraits

Use the same Debug adapter, disposable saves, original graphics mode, and fresh output/settings
from the authored recipe. Load `local/private-inputs.ps1` in the launch process. Select the
registered world, entry and battle scenes in place. Each case is an independent controlled start
through the existing host observer, with selection and Back driven by ordinary mapped keys.

From the repository root, select `$case` below, use an absolute `$start` path under this
checkout's ignored `local/`, and copy that case's tracked input unchanged to `$start`.
Use the registered opening party. These cases need no separate party input.

| `$case` | Copy from `remake/reference/inputs/` | Input and observation |
| --- | --- | --- |
| `member-field-portraits` | `yeel-to-battle03-start.json` | From settled Map66 Party, select two members, open Prepare and Back, then close/reopen Party. Read the selected row and summary textures directly. |
| `member-battle-portraits` | `engine-battle02-egress-start.json` | Accept the initial Battle02 entry, open Member, select a second row, open details and Back, then close/reopen Member. No turn is committed. |

```powershell
$pack = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2035/galam-town-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $pack 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $pack 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $pack 'entry/entry.json'
Get-ChildItem -LiteralPath (Join-Path $pack 'opening-inputs') -File -Filter 'SF2_*.json' |
    ForEach-Object { [Environment]::SetEnvironmentVariable($_.BaseName, $_.FullName, 'Process') }
$env:SF2_INSPECTION_START = $start
$env:SF2_INSPECTION_ROUTE = 'keyboard'
$env:SF2_VIEWPORT_CASE = $case
$env:SF2_VIEWPORT_OUTPUT = $report
Push-Location remake
& $env:GODOT_BIN --verbose --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path game --log-file $nativeLog --script res://probes/engine_battle_member_observation.gd -- `
    --input-settings $settings
$nativeExit = $LASTEXITCODE
Pop-Location
```

**Confirmed, bounded native observation.** Both cases exited 0 with `passed:true` and empty
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
notice. Main Back before choice restores Movement through existing undo. A faced chest substitutes
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

The inspection footer contains instructions on every page. Cursor and the two detail sheets
retain a measured Back button. Options, Map and Members show the mapped return in their
instructions. Mapped Cancel, a complete right-click and an eligible Back button follow the
same local parent and retain cursor or roster focus. The footer shields the board from left
clicks. Cursor shows the name or empty-ground caption and coordinates in the hover card.
Resizing or changing page cancels an old capture.

Movement, Target, hover inspection and playback use a project-authored double-gold frame
with stepped corner accents. The mini-status composition puts the name and admitted ally
class and level in the header, then stacks HP and MP with aligned bars and right-side values.
Inspection adds coordinates and land effect in the bottom row. The game preserves the live
resource and participant boundaries. This composition follows the placement of information in
the [pinned mini-status routine](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/ministatuswindow.asm#L572-L664).

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
Spell effect wording, the round queue and Target rule geometry remain with their linked owners.

## Selected spell descriptions

Pending implementation under [#2083](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2083).

Reuse the fixed-height reader from `ItemDescriptionView` in battle Magic and field Prepare Magic.
Use one typed spell-description projection and localized formatter for both, keyed by exact
`SpellRef` including level. Derive it from the same admitted effect/offer data as the action.
Battle uses the typed choices, healing/attack/support/retreat definitions, and live availability
reason. Field contributes the actual field effect, member/target requirements, cost and map/party
availability from its recovery rules. Share meanings and names where they match. Explain a
no-effect field use as free and distinguish a nominal field MP cost from the amount deducted when
consumption clamps at zero. Reserve the insufficient-MP refusal for a context that enforces it. The battle cost, target team, min/max reach and area radii apply on the battlefield.
Distinguish self, single target, selected-area and all applicable allies. Explain current refusal
such as insufficient MP, Silence, no eligible target or missing effect support as returned by the
context. Use actual level effects and omit invented damage or availability.
[Battle offers](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) ·
[Field recovery](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs)

The row keeps label, level, icon and cost. The reader below it provides the details and resets
reading position on selection, while row bounds and focus stay stable. Enter Target with the
short action identity and applicable hint in the same footer, yielding the description area.
Back restores the same level/slot and its description. Keep the old item behavior and reuse the
PO catalogs for English and Chinese, including localized refusal reasons. Field retains its
own eligible choice, target and Confirm flow.
[Battle layout](../../game/src/Battles/BattleMenuLayout.cs) ·
[Field preparation](../../game/src/Exploration/FieldActionView.cs) ·
[Reader](../../game/src/UI/ItemDescriptionView.cs)

## Current-round order (#2065)

Reserve a quiet readout immediately below round/gold during Movement, Main, Magic/Item and
Target. Default to current actor plus up to three subsequent eligible entries, with the count of
additional entries when truncated. Use distinct current/next roles, allied/enemy identity and a
disambiguator for same-name units. Repeated slots for one unit stay distinct in sequence. Keep
the strip read-only and outside the input focus order. Use a compact two-line block at 1080p
with the current entry above the ordered upcoming chips. A slot's side, current cell or stable encounter ordinal
can disambiguate duplicate names in the strip. Wrap names within the width budget. If a layout needs room,
reduce the number of later chips before shrinking the text or hiding the first eligible entry.
The overflow count keeps the first version usable without another input mode. At the real end of the generated slots show a localized
next-round-undetermined message. Once the next round is generated, rebuild from that queue.

Read `EngineBattleState.Queue` from `Cursor` to the first null-actor sentinel or end. Apply the
same current-state dispatch classification as `BattleAdvancer` for dead/unplaced and skipped
sleep/stun entries. The displayed eligible order can change after a committed status update,
including skip processing or waking before a repeated slot. Refresh from the new snapshot and
make the strip's current-round, current-state meaning clear. Reading the queue leaves skip side effects to actual dispatch. Apply existing disclosure to
identities. An otherwise undisclosed actor can occupy an unnamed slot. Read actor identity/control from the
live state so disabling, departure, revival, appearance and control changes are reflected.
Continue uses the restored queue and cursor. Projection preserves engine order, RNG, AI decisions,
round generation and persistence.
[Turn advancement](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
[Queue owner](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) ·
[Existing implementation ticket](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2065)

## Implementation handoff

Main-gate owns acceptance and dispatch. Slice 1 settles composition. Slices 2 and 3 share battle
session input/presentation paths with it and with each other, so schedule their writes serially.
Slice 4 consumes the reader and phase layout from slice 1. #2065 consumes that same layout.
Serialize any overlap with other writers of its battle layout or trace. Declare exact paths
before each implementation. The following are principal owners and distinct acceptance outcomes.

| Slice | Player behavior and principal owners | Acceptance and risk |
| --- | --- | --- |
| 1. [#2079](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2079) Board focus and decision layout | One cursor/field/card owner per phase, the #2079 Main diamond (later corrected by [#2090](#vertical-battle-main-2090)), stable child and footer regions, auxiliary and close-up retirement. `BattleSessionView`, `BattleMapViewport`, range/cursor views, `BattleCommandMenu`/`BattleMenuLayout`, `BattleInspectionView`, `BattleSceneView`. | Short real Move → command → Attack/Magic/item target → Back → Movement and inspection/options/return. Include a rejected mouse target, ally crossing, multi-target and field delivery. Inspect visible/hit rectangles, focus, map clipping and errors in both languages, at supported minimum/1080p/larger wide viewport, with wide values/long text and Reduced Flash. Verify private source cursor and public fallback. Resize a retained close-up token while paused and check art/card positions and unchanged delivery on return. |
| 2. [#2080](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2080) Target edges and ordinary Stay | One selection per fresh direction, one gesture to Stay, continuous Movement. `BattleSessionView.Navigation`, `BattleCommandMenu`, current `GameInput` release tracking and `BattleCommandDispatcher`. | Keyboard and pad/d-pad or stick single target and multiple targets for physical, Magic and Item. Compare hold, neutral/repress, fast taps, menu/move → target with inherited direction, target Back/reentry, host cover, and continued held movement. Origin and moved Stay commit once on key/pad/click. Exercise next actor or after-turn delivery while holding Confirm. Search and Give/Drop confirmations, refusal and backwards routes still work. |
| 3a. [#2081](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2081) Inspected movement | Delivered through `QueryBattleInspectionRange`, `BattleInspectionView` and the shared board overlay. | [Authored corridor observation](#reproduce-the-inspected-movement-field) checks ally → enemy → empty, traversal versus stops, details/Map/Members/host transitions, cursor exit and state/seed equality. |
| 3b. [#2082](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2082) Shared member presentation | Delivered shared scrollable roster rows and existing details through `MemberRosterView`, `PartyStatusView`, `BattleMemberListView` and `PartyDetailsPanel`. | [Short authored observations](#reproduce-the-shared-roster) cover roster size, long names, retained conditions, pointer and mapped focus, Back/scroll and field Prepare. |
| 4. [#2083](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2083) Describe spells by level and context | Actual MP/effect/targets/range and current reason in the shared reader. `BattleChoices`, field recovery/choice owners, `BattleCommandMenu`/`BattleMenuLayout`, `FieldActionView`, reader and PO catalogs. | Compare two levels, a multi-target effect, self or EGRESS, lack of MP/Silence/unavailable content, legal field recovery and field-limited effect. Focus or scroll changes do not spend MP or move the rows/footer. Confirm/cancel still use their existing action path and restore the chosen row. |
| 5. [#2065](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2065) Show the actual queue | Current and next scheduled actors in the reserved left stack. Read projection beside `BattleAdvancer`/`BattleTurnFlow`. Render through the battle HUD/layout and document in the feature trace. | Compare the next actual eligible actor, repeated slots, same names, status skips and changes, dead/unplaced entries, sentinel and empty remainder, new round and cold Continue. Verify hide/show by phase and equality of seeds, resources and queue after repeated projection and cover/return. |

[#2068](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2068) owns formal BLAZE
playback assets and shares the playback writer only as needed. [#2071](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2071)
owns the field Prepare command icons and its six actions. [#2070](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2070)
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

For the [mini-status geometry observer](../../game/probes/engine_battle_vitals_observation.gd),
set `SF2_VITALS_CASE=wide` and give the authored center enemy the same wide HP values and a
similarly long ID (change the definition, placement and start actor references). This covers wide
values in both the acting and target cards, including the enemy header's disclosure boundary.
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
