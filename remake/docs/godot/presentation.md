# Godot Host and Presentation

The [shared UI design language](ui-design-language.md) and
[player-path scene review](ui-scene-review.md) own #1616's design decision to replace directional
diamonds with labeled commands using the admitted original art. The scene review records the user-approved Godot layouts and their preview coverage. The agreements below describe the controls and behavior in the current game.

The implemented bounded [game localization design](../application/localization-design.md) owns English/zh-CN
catalogs, safe settings refresh, name/IME and font boundaries. Translated
display remains separate from Application's canonical text work and game-save identity.

The [local services and battle overview proposal](modernization-design.md) owns #1543's bounded
modernization choices, real service/disclosure limits, menu/return sketches and minimum acceptance.
Near-town remote services and a read-only overview are recommendations awaiting product selection,
not implemented capabilities or changes to the current agreements below.

The [battle interaction baseline](battle-interaction-baseline.md) is the maintained battle feature
trace: source/accepted need → player decision and return → layer responsibility and Content admission
→ ordinary connected entry → acceptance and remaining Issue. It owns the accepted, unimplemented
inspection contract (#1633) and distinguishes delivered battle Save/Continue from authored-only
transactions and #1616's visual proposal. Update that entry with its detailed owner when a feature merges.

## Production entry and field dialogue (#1606)

**Standard rigor.** Apply the accepted [F1/F2 design](ui-scene-review.md#1-title-and-the-witch)
to the ordinary game entry and real field questions. This is a bounded presentation migration:
save publication/recovery, seven-character keyboard naming, source choice defaults and result
flags retain their existing owners. F3–F10, gameplay rules, content admission and hint redesign
remain outside this slice.

F1 opts into square blue action surfaces, a reserved focus marker column, gold selection and the
existing editor focus outline. Host and slot rows share the square journey action treatment. The standalone gameplay Menu
uses compact padding and the same square style family. Entry actions wrap
and scroll inside the right column; reading a long greeting owns the action area until its pages
are read. Name prompt and validation text and slot/message/detail content have internal scrolling, with
mapped hints and the name actions outside that scrolling region. The name editor retains its
horizontal Back/Use actions, actual alphabet, draft and fixed Enter/Escape semantics.

The entire Witch rectangle and right-column reservation remain as specified in the
[entry agreement](#entry-windows-and-hints-1445). No veil or form covers her. Entry still uses its
1080p scaled canvas; gameplay remains native. Body/action text stays 30px and hints 24px.

F2 already uses the same `StoryDialoguePanel` as the accepted preview: retain measured reading,
portrait columns, stable speaker avoidance and coordinated close. Only the field presenter opts
its real Yes/No row into square surfaces and a 64px action region. The existing source/authored
choice and pointer owners still decide focus, release admission, Cancel=No, commit and return.
Other shared dialogue callers keep their prior choice geometry. No source text, original art,
translation catalog, script or save format changes.

**Confirmed — bounded remake observations:** the ordinary title/New Game path accepts English and
real #1602 Chinese entry/world catalogs at 1920×1080 and 2560×1440. Native controls verify square
styles, the unchanged Witch bounds, contained fixed hints, mapped pad focus, pointer New Game,
invalid length/alphabet, retained name draft, safe final Back and real initial-save publication.
A copied occupied disposable slot remains byte-identical through confirmation Back. A separate
fresh process with a deliberately corrupt disposable primary offers explicit backup recovery,
defaults to Cancel, returns safely and restores the actual saved field only after confirmation.
An in-checkout save-root selection exercises the existing explicit configuration refusal and
Cancel; it is not a simulated successful write or an operating-system disk-failure claim.

A display-only Chinese entry stress variant reads 1,450 characters through the existing pages.
A separate pre-admission UI translation variant wraps a long New Game action, an occupied slot
description and name prompt; the real slot/name scroll containers retain their fixed hints/actions.
It changes only in-memory display resources, never the tracked catalogs or save data.
The actual Map3 source approach reaches text531's question with its supplied portrait. English
1920×1080 covers directional selection and Cancel=No; Chinese 2560×1440 covers a 1,341-character
question, retained control tokens, body growth/reading, separate choices/hints and a deliberate
pointer Yes. Both verify the actual source result flag and choice retirement before continuation.
The controlled start is not a claim of natural campaign reach. No screenshot, active snapshot
setter, new comparison system, engine-rule test or full campaign replay is involved.

The reusable observer is [`entry_ui_observation.gd`](../../game/probes/entry_ui_observation.gd).
Load the owning checkout's current ignored environment and registered prepared inputs, including
both final world/scene selections. Use a fresh ignored report/settings directory and an external
disposable `SF2_SAVE_ROOT`; never select player slots. For Chinese, select both actual display
catalogs and use a settings file with `language: "zh-CN"` (the switch alone is insufficient).

```powershell
$env:SF2_SAVE_NAMESPACE = 'issue1606-<fresh-case>'
$env:SF2_ENTRY_UI_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
$env:SF2_ENTRY_UI_LARGE = '0' # 1 selects 2560×1440
$env:SF2_ENTRY_UI_ZH = '0' # 1 asserts the admitted Chinese question
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --path remake/game --script res://probes/entry_ui_observation.gd -- --input-settings $settings
```

For a real source question set `SF2_ENTRY_UI_CHOICE=no` or `yes`, remove `--headless`, and set
`SF2_ENTRY_UI_START` to a fresh worktree-local controlled start consumed by the existing
`StartPreVoyageLocalizationObservation`: Map3, entity-0, (42,9), facing3, speed32,
flags0/32/256/260/261/601/602. The observer takes the owned native window off-screen, requests focus,
walks Down/Right, reads the actual script and stops immediately after the answer. It checks focus
and declared language. `SF2_ENTRY_UI_RECOVER=1` instead requires a valid copied `slot1.bak` and
corrupt `slot1.json` in this disposable namespace. `SF2_ENTRY_UI_ERROR=1` expects the deliberately
invalid save-root configuration. Unset case switches between runs. Adapter compilation, necessary
Debug compilation for native loading, direct scope/private checks, the committed planner and scoped
CI are the engineering gates.

`SF2_ENTRY_UI_LABELS=1` selects that finite Chinese UI stress variant before host initialization;
use it with a copied valid occupied slot and Chinese settings. It is layout input, not an accepted
translation or a new naming capability.

**Unknown / remaining demo UI gaps:** final visual harmony in the ordinary game still needs human
review. Physical keyboard/controller behavior across devices, OS focus/DPI combinations, actual
disk-full/permission/publication-uncertain failures and unobserved script/Search/long-label
combinations retain their existing boundaries. The runs report ObjectDB shutdown warnings (two
instances for entry, six for these controlled source questions); their exact ownership is unresolved.
F2's separate field command list and F3–F10 production migration remain with #1616's later slices.

## Entry confirmation observation

The [entry lifecycle agreement](../application/opening-session-lifecycle.md#entry-confirmation-responsiveness)
owns #1605's bounded behavior. **Confirmed:** on accepted base `cba8bb41`, ordinary first/repeated
New Game and Continue slot entry blocked the host frame for approximately 1,016/974/647ms.
Boundary timing attributed 642–1,035ms to source admission, 6–20ms to display binding and less than
2ms to empty-slot reads. Disabling the entry node also automatically paused its child music player:
the playback position stopped, `StreamPaused` became true, and the cue receipt sequence did not change.

With detached admission and explicit audio pause ownership, the WASAPI headless observation measured
first/repeated New Game and Continue slot readiness at approximately 1,263/850/766ms, with maximum
host frame intervals 46/21/21ms. Loading feedback appears before admission; music stays unpaused,
its position advances and its receipt sequence remains unchanged through slot entry/return.
This improves responsiveness rather than promising shorter source admission. Final New Game/Continue
world mounting remains synchronous Godot work: approximately 190–210ms inside `MountSession`, with
observed first-world frame gaps around 238–256ms. Loading feedback precedes this tail and legal
world music replaces entry music. Full entry startup without any frame stall is not claimed.

The bounded native cases cover ordinary New Game/Continue, cancellation and repeated confirmation,
bad-source retry, empty/corrupt slots and explicit backup recovery, focus loss, Settings/window-recovery
pause, host replacement and deferred close during reading/publication. Temporary fault injection
also confirmed current-owner exceptions reach recovery UI and stale-owner exceptions are cancelled;
that diagnostic API is not shipped. **Unknown:** human listening/foreground aesthetic acceptance.
WASAPI playback state/position is software evidence, not a human listening result. The retained
ObjectDB exit warnings are distinct from input/audio acceptance.

Reproduce with the existing Debug build and imported localization resources, the registered read-only
inputs, a fresh disposable save root outside all checkouts and an explicit new namespace. Do not
select player slots or an earlier save-validation destination. The small
[`entry_confirmation_observation.gd`](../../game/probes/entry_confirmation_observation.gd) reuses
ordinary semantic input and live state; it refuses to run without an explicit namespace marker and
report path. It accumulates frame-gap maxima, not per-frame state captures. Allow 55 seconds and
under 200KiB of report/log output per serial native process; no world/media copy or screenshots.

```powershell
. ./local/private-inputs.ps1
$env:SF2_SAVE_ROOT = '<absolute-fresh-disposable-root-outside-checkouts>'
$env:SF2_SAVE_NAMESPACE = '<fresh-namespace>'
$env:SF2_ENTRY_CONFIRMATION_NAMESPACE = $env:SF2_SAVE_NAMESPACE
$env:SF2_ENTRY_CONFIRMATION_OUTPUT = '<absolute-ignored-report-file>'
$env:SF2_ENTRY_CONFIRMATION_CASE = 'timing'
& $env:GODOT_BIN --headless --resolution 1920x1080 --path remake/game --max-fps 60 `
  --audio-driver WASAPI --script res://probes/entry_confirmation_observation.gd
```

Run `new` then `restore` in separate processes for durable creation/restart; `cancel`, `bad-source`,
`focus`, `window` and `detach` are separate short cases. `quit` and `quit-write` report the request
before actual process exit; require exit 0 without a timeout and inspect the resulting disposable
slot through ordinary `restore`. `bad-slot` requires a valid disposable backup and deliberately
invalid primary plus an empty second slot; it verifies explicit recovery without preparing or
mutating that fixture itself. Use Dummy when only input/state acceptance is required, and preserve
failed reports. Adapter compilation and the clean committed planner are the affected gates; no
gameplay rule or engine behavior changed.

`active-quit` and `active-quit-write` cover close during replacement source admission and publication
with an already mounted, dirty session. Set `SF2_ENTRY_CONFIRMATION_START` to an existing controlled
field-start JSON beneath this checkout's ignored `local/`; the existing private observation bootstrap
mounts the real session, then ordinary system-menu, replacement, naming and close inputs drive the
case. This controls initial placement, not preceding natural reach. Both cases require the original
session ID/revision and unsaved warning at quit confirmation, then cancel and observe usable original
field input/audio. The discovered failure (loading panel swallowed deferred quit) and its corrected
passes are retained with the other worker reports.

## System menu agreement (#1518)

**Standard rigor:** consolidate the existing gameplay system entries and preserve their eligibility,
confirmation and input lifetimes. One `SystemMenuEntry` in `HostUi` replaces NewGameEntry, SaveEntry,
QuitEntry and SettingsEntry. At 1080p it occupies160×56 at right margin20/top12, with24px text;
the existing font/style minimums are included in its measured dimensions. The centered800px menu
uses the existing blue/gold theme,64px action rows and readable unavailable reasons. World96px/4×
scale, battle diamond/status responsibilities and title/Witch presentation are unchanged.
[Finite state/input/geometry observations](../verification/host.md#system-menu-observation) own
engineering acceptance; final raster, physical device access and human aesthetic approval remain
**Unknown**. Church behavior (#1519) and a general navigation/pause service are excluded.

| Group / action | Availability and return |
| --- | --- |
| Resume Game (initial selection) | Always safe; closes Menu and resumes only when no other host modal covers play. |
| Journey / Save | Field uses `QuerySave`; admitted battle uses `QueryBattleSave` at settled player Movement/origin. Unsupported profiles, busy states and nonzero previews show their real reason without cancellation or queued save. The field shortcut and Church caller remain. Save confirmation Back returns slots; slots Back returns Menu. [Battle contract](../application/battle-save-and-continue.md#implemented-ordinary-host-boundary-c). |
| Journey / Settings | Existing `CanOpenSettings`: Witch menu or settled exploration outside other menus. No battle/story access is added. Settings/help retain draft, Apply and Back ownership; closing Settings returns Menu. |
| Session / New Game | Existing settled-field `CanWaitAtInput` and Party exclusion, followed by the existing replacement entry with Cancel selected, slot/name and final explicit Start/save confirmation. Cancel unwinds those existing pages to Menu. |
| Session / Quit | Existing clean/dirty confirmation, Cancel initially selected; Cancel returns Menu. Window recovery retains its own Quit confirmation and returns to recovery first. |

Menu can cover field story or battle presentation without delivering a gameplay command. It is
unavailable over Party; other host modals hide the sole entry. The finite `ApplyHostPause` predicate
now includes Menu and pauses view/audio. Subpages hide and disable Menu; window recovery hides it
and restores the same selection/modal before gameplay can resume. Existing `DisarmInput`, remap
release guards and movement/wait disarming apply on every host transition. Menu's pointer gestures
reuse `MenuPointer`; only complete same-row press/release activates. Right click and mapped Cancel
return one level; hidden global buttons cannot intercept gameplay pointers. Directional navigation
skips unavailable actions, whose reasons remain visible. Recovery/focus loss cancels an unfinished
Menu mouse gesture before restoring the same panel.

`GameAction.Menu` is integrated in existing `GameInput`/version1 `bindings`, held-state guards,
actual hints and the existing Controls Help. Defaults are F10 and right stick right (`RightX+`).
At settled field input Cancel opens the [gameplay diamond](#exploration-gameplay-diamond-agreement-1588); Cancel leaves the title unchanged. Battle Cancel
continues to unwind gameplay, Start remains Equipment and Back remains Item. Within Menu, mapped
Menu toggles Resume; Confirm/Cancel use the existing swap. Subpages keep their own Cancel semantics.

Old preferences retain every binding. If Menu is absent, choose an unclaimed F10/F9/F8/F7 then an
unclaimed named key, and the first free signed axis in RightX+,RightX−,RightY−,RightY+,LeftX+,LeftX−,
LeftY+,LeftY− order. When all axes are assigned, implicit Menu remains keyboard/mouse accessible
and visibly explains that pad access requires editing bindings. No Guide fallback or claim that a
host delivers a system-reserved pad button is made. An explicit `menu` binding follows ordinary
keyboard+pad and duplicate validation. Effective defaults do not rewrite old preferences; Settings
retains implicit Menu on unrelated edits. Controls → Change controls edits Menu as an explicit row.

The owned implementation is GameRoot, the existing persistence/settings/window-support partials,
one `Entry/SystemMenuView`, GameInput/ControlsHelp and the two existing locale catalogs/argument
registry. No Domain/Application, progress format, savepoint legality or new navigation authority is
changed. A directly observed invalid-settings early exit now disconnects window-support signals
only if initialization actually subscribed them; this does not hide other shutdown warnings.

## Ordinary menu resource acceptance (#1666)

**Standard rigor:** repair the existing three private diamond paths by publishing/selecting the
ordinary world and battle resources together. No gameplay, save format, modern96px world scale,
font/translation or new list consumer is changed. The [publication and source inventory](../content/presentation-assets.md#ordinary-menu-publication-and-coverage-1666)
owns exact resource identities, preparation, size bounds and #1667–#1669 handoff seams.

**Confirmed:** the inspected ordinary world had no field/church menu sets and the selected battle
scene had no command set. Actual title New Game and Continue showed compatible text squares.
The same ordinary entry paths with the new publication draw the supplied original24×24 normal/
selected rasters at nearest4×. The failure was missing assembled resources; dedicated scene-only
success did not establish ordinary startup coverage. Public-authored scenes intentionally retain
text fallback; private empty sets/partial sets are not silently called original presentation.

**Current route:** F5–F7 uses [field/service observation](field-and-shop.md#reproduce-the-bounded-observations).
The older combined `engine_menu_assets_observation.gd` field-new/field-continue command below,
`engine_identity_icons_observation.gd` field subroute and `engine_inventory_art_observation.gd`
field/field-controlled subroutes reproduce the earlier4×/cardinal composition only in a checkout
pinned to [`6c2d0982`](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/6c2d098212efebb26d026d335eb45c369914cff4).
Their source publication/pixel/error evidence is retained; do not run those historical field commands
against the current presenters. Current fixed-slot, held confirmation, owner/recipient/Back and source
art checks are in the F5–F7 observer; the [field-menu witness](../verification/host.md#exploration-gameplay-menu-observation)
retains focus recovery, host exclusion, malformed field art and exact close-edge checks. Separate
battle/Church histories keep their named owners rather than becoming this field correction's gates.

**Historical command (pinned checkout above):** use `game/probes/engine_menu_assets_observation.gd` against the ordinary `Main.tscn`. It starts the
actual title, submits mapped New Game/slot/name/Normal Start or Continue input, then reads existing
state and `_Draw` resource projections. It never assigns a gameplay state or bypasses restoration.
After the normal protected environment and both publication selections, use a fresh disposable save
namespace outside checkouts and explicitly selected English or Chinese instant-text settings:

```powershell
$env:SF2_MENU_ASSETS_CASE = 'field-new'
$env:SF2_MENU_ASSETS_EXPECT = 'original'
$env:SF2_MENU_ASSETS_OUTPUT = Join-Path $run 'native.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 --path remake/game --script res://probes/engine_menu_assets_observation.gd -- --input-settings $settings
```

`field-new` covers the four field commands, existing Member/Magic/Equip text consumers and layered
Back, then walks from the real New Game upstairs through the stairs to the Map3 priest at(32,13).
Bounded input planning reads the live layout, including slope movement; only semantic samples are
retained. Church covers four services, normal/selected images, refused Promotion without state/
resource change, healthy Cure result, and caller return. `battle-continue` uses a disposable copy of
an actually written compatible battle save and title Continue, then the main diamond, spell list,
existing equipment HUD and return to Movement. `authored` additionally takes
`--authored-package <absolute authored package>` and expects `fallback`; it needs no original art.
For the missing-private-resource comparison, select the original unaugmented world/scene and expect
`fallback`. `invalid-scene` uses Continue with one command raster removed from a disposable scene;
startup must show the existing error and never mount gameplay. No player saves are changed.

**Confirmed** bounded native results: English/Chinese ordinary field and church; English/Chinese
ordinary battle Continue; missing-resource private input; public authored fallback; partial private
scene rejection. Observations retain actual selected labels, resources, dimensions/filter/cardinals,
consumer pages and parents. The private publication also preserves all gameplay data and unchanged
dependency bytes. Completed setup/route failures remain in `local/issue1666/HANDOFF.md`: old external
WAV assumption, route planner omission of stairs/slopes, and one deliberately terminated expensive
observer attempt. None is relabeled as a passing run. Some exits retain2–4 ObjectDB warnings; this
work does not establish leak-free shutdown.

**Unknown:** human aesthetic/readability approval, physical OS-device/focus behavior, original
natural route/timing/pixel parity. Member/Give/Drop art has the separate #1668 acceptance below; shop/options art has the [#1669 acceptance](#shop-and-options-art-acceptance-1669);
#1667 identity consumers have their separate acceptance below. No screenshot, full
campaign run or original emulator observation is claimed. The contextual authored chest Search
caption remains a text override; the ordinary admitted Battles01–07 do not expose that variant.

## Exploration gameplay diamond agreement (#1588)

Current F5–F7 layout/navigation is owned by [Field, party and shop UI](field-and-shop.md).
The source/capability boundaries and retained earlier layout observations below keep their stated scope.

Standard rigor: a bounded multi-consumer presentation change with existing command authority.
Outcome: ordinary field control exposes a four-cardinal gameplay menu without another top bar.
Cancel (including the current remap/pad alternative) or the visible bottom-right Adventure entry
opens it. Equipment (default E / Pad Start) keeps direct Party Status. Menu retains its independent
system menu binding and Settings/Save/Quit; field Cancel now belongs to gameplay. Existing title,
story and battle Cancel semantics remain with their consumers.

**Confirmed:** pinned `SF2DISASM` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/menus/menustringspointertable.asm:pt_FieldMenuNames`, `diamondmenu.asm:ExecuteDiamondMenu`
and `menutilespointertable.asm:pt_tiles_Menu` bind Up Member, Left Magic, Right Item, Down Search.
The packed field entry `0x85010204` selects main-menu icon indices5/1/2/4; each576-byte icon has
normal/selected288-byte halves. `main/mainactions.asm:FieldMenu` starts Member with the main-summary
member list, Magic with the magic-summary list then spell/target, Item with the Use/Give/Equip/Drop
submenu then owner/item/recipient or equipment, and Search through the faced location. These are
static source claims, not original natural reach, hardware timing or visual approval.

The modern mapping reuses `PartyStatusView` member selection and `FieldActionView`: Member retains
status and general Prepare; Magic opens the magic pane, then Confirm opens that member's spell list;
Item opens inventory, then Confirm offers Use/Give/Equip/Drop before their existing item/target and
confirmation pages. This deliberately chooses the member before the item operation. Cancel unwinds
one provisional page to its parent, then member selection, then the same selected root diamond,
then field control. Successful preparation uses the existing result page and refreshed root actions.
Search submits the same Application `Search` as contextual Confirm; it does not substitute Talk.
Every transaction uses `QueryFieldActions` and the existing revision/command/result path. No UI rule,
party, flag, inventory or resource authority is introduced. Member availability uses the party query; Magic/Item require at least one member with a successful
`QueryFieldActions`, and Search uses `CanSearch`. Unavailable actions remain inspectable and cannot submit; unsupported effects retain the preparation refusal/no-effect behavior.

Opening, selecting and cancelling submit no session command, discard clock debt and disarm movement
and held input. While this menu or its member pages is open the exploration clock does not advance.
Story/Church/Shop/battle and system modals cannot coexist with it. Menu and Save shortcuts stay
unavailable until gameplay pages close. Focus loss, pause and page transitions suspend complete
pointer gestures and require release before new actions. Shared `DiamondCommandButton`, geometry,
theme and `MenuPointer` own their established presentation/input responsibilities. Optional complete
`presentation.fieldMenu` supplies original icons; absent art uses measured short text captions.
Malformed native images stop through the existing presentation failure, without repeated decoding.

Acceptance is the affected Content admission behavior tests, adapter compilation, committed scope
plan/actual CI and a bounded actual Godot input/state/projection/error observation. First accept one
representative scene before extending to keyboard/mouse/pad, remap, Chinese, disabled actions,
release/focus and layered return. No screenshots or full campaign rerun. Remote town services,
objectives/quest data, minimap/navigation/auto-walk and global hint redesign (#1544) are excluded;
#1578 may later consume the existing selected-detail layout. Human visual approval remains #1456.

## Church and shared Save agreement (#1519)

**Standard rigor:** the current #1587 outcome is a four-service diamond followed by the existing
ordered quotation/confirmation/result flow, within the
[High recovery agreement](../application/opening-session-lifecycle.md#implemented-opening-church).
The actual Map3 caller, carried party, typed `ChurchWait`, costs and save lifecycle remain authoritative.
The [native observation](../verification/host.md#church-diamond-observation) owns the finite acceptance.

- The four cardinal slots are Raise (up), Cure (left), unavailable Promotion (right), and Record
  adventure… (Save) (down). Mapped directions focus a service; Confirm or a complete fresh icon click
  selects it. The full selected service and unavailability/save explanation appear beside the diamond.
  Leave is an independent Back control; Cancel/right-click also leaves from services.
- Existing `ChurchWait` supplies ordered eligible-member offers directly. Confirm pays that exact
  offer once; Decline/Cancel rejects it without payment. Results acknowledge into the next existing
  offer or services. No extra member selector, transaction state machine, refund or service is added.
  No-applicable-member, insufficient-gold and curse-unavailable feedback retain their meanings.
- The [private church resources](../content/presentation-assets.md#private-church-menu-icons) provide
  eight unchanged24px rasters at nearest4×. `DiamondCommandButton` and `DiamondMenuGeometry` share
  only pixel/focus/cardinal geometry with battle; battle rules and navigation remain battle-owned.
  Public content without these resources uses short text squares, with full labels beside them.
  A supplied set must be complete. A PNG that passes header admission but fails native decoding
  releases temporary textures and stops through the existing presentation-failure boundary once.
- The measured, wrapped title, selected description and hints determine the centered panel height.
  Quote/result choices remain separate readable rows. English/Chinese fallback labels fit96px
  buttons; long save help remains beside the diamond at1080p/1440p. The bundled font remains required.
- A service token change, command, hide or pause suspends the old gesture and requires neutral input.
  A complete fresh pointer click works after pause; held Confirm cannot continue into another charge.
  Focus changes clear transient refusal. The existing reduced-flash preference controls focus emphasis.
- Record adventure uses the existing `LeaveChurch(Save: true)`, completes the real caller/event/portrait
  return and finite field settlement, then opens the same SaveSlotView and eligibility query as Menu
  → Save. The same two slots, destination, safe Cancel-first overwrite, backups and format apply.
  Save cancel/success/failure return to exploration; a fresh priest interaction reopens services.
  A controlled/authored profile still explains its existing `save-profile-unavailable` refusal.

No new save permission, original service timing, source-pixel window reproduction, whole-game visual
approval or physical device delivery is claimed. Human aesthetic approval remains with #1456.
Existing ObjectDB shutdown warnings remain **Unknown**; this slice does not claim leak closure.

## Display layer boundaries (#1517)

The [display composition audit and contract](display-composition-contract.md) maps viewport,
field-priority and close-up responsibilities, current consumers, evidence limits and bounded repair
candidates for #1621. It retains the implemented canvas agreement below; its proposed geometry
refresh and review constraints are not implemented changes or visual acceptance.

**Standard rigor:** make the existing battle dialogue and status readable over world sprites, then
bound the current field, battle and host overlay consumers. This slice owns composition and
visibility lifetime only. Menu redesign (#1518), Church behavior (#1519), gameplay, save data,
translation, source-world priority and all-scene aesthetic approval remain outside it.
The [finite observation recipe](../verification/host.md#display-layer-boundary-observation) owns
acceptance, retained failures and the rendering limits.

| Canvas | Owner and responsibility | Local ordering |
| --- | --- | --- |
| 0 (viewport default) | Field draw/planes/entities/white fade; battle map and close-up visuals | Existing map actors 1, foreground 2, markers 3; close-up ally/weapon/fairy/effects 1–4 remain local. The existing close-up map visibility rule remains. |
| 1 (`BattleUi`) | `BattleSessionView`: command/HUD, scene cards/messages, Give/Discard/Retreat, battle story/appearance and feedback | Existing sibling order; scene cards/messages have one `SceneUi` container bound to the close-up's visibility. |
| 2 (`FieldUi`) | `ExplorationSessionView`: story/context, Party, Church and stopped-presentation feedback | Story precedes Party/Church, then failure feedback. Existing phase/visibility rules decide which is active. |
| 3 (`HostUi`) | `GameRoot`: entry/name/slots, system Menu, Settings, Save/Quit and window recovery | Existing creation order; recovery explicitly hides covered host views and restores them on close. |

`PresentationCanvas` is a small viewport composition helper with these three fixed roles. Each
session owns its canvas as a child; it does not register/reparent live gameplay owners, select input,
or introduce a layer manager. There is at most one live canvas per role. Canvas-layer ordering
isolates world Z from UI even where geometry overlaps; local Z and tree order apply within a canvas.
See Godot's [CanvasLayer ordering](https://docs.godotengine.org/en/stable/classes/class_canvaslayer.html)
and [CanvasItem relative Z](https://docs.godotengine.org/en/stable/classes/class_canvasitem.html).
World fades, white flashes and world modulation stay on canvas 0; readable UI is outside that
modulation chain, including stopped-presentation feedback. This is a modern presentation boundary,
not a change to logical source fade state or timing.

The new roots fill the viewport, use identity transforms and ignore mouse filtering. Existing views
retain their own themes, geometry, pointer routing and action/release guards. Canvas boundaries do
not grant modal input protection: `ApplyHostPause`, focus suspension and each existing input owner
still provide it. Canvas children inherit process mode through the scene tree. A session canvas
explicitly mirrors its owning Control's inherited visibility, since a CanvasLayer does not inherit
CanvasItem visibility/modulation. `SceneUi` similarly mirrors the close-up's visibility while sharing
the battle session's lifetime. Enter/exit subscriptions survive battle reparenting; Hide/Show does
not recreate UI or clear reading state. Freeing the owning session frees its canvas and UI.

Acceptance covers the named controlled/authored consumers, actual state/input and overlap geometry,
not the unidentified version in the user's original report. Final raster appearance, physical
focus/device coverage and whole-game visual approval remain **Unknown**.

## Shared visual rules and finite consumers (#1453)

**Standard rigor:** harmonize existing readable gameplay controls in independently reviewed stages.
The shared-theme/battle-list pilot and finite consumer migration are independently accepted and
integrated in [PR1462](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1462) and
[PR1464](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1464). The inventory below
owns only existing host-button, Church/Prepare/Party and battle result/auxiliary-HUD presentation,
this owner, the [gap disposition](../evidence/opening-ui-gaps.md#shared-visual-pilot-1453)
and the [necessary native recipe](../verification/host.md#finite-consumer-migration).
Representative menu/result observations precede sibling checks. No new UI kit, font/art acquisition,
gameplay rules, input authority or world rendering belongs to this work. Preserve existing fonts,
blue/gold assets, diamond icons/animation, source-grouped Party layout and 96px/4× world scale.

Current shared rules:

- Existing dark-blue frames use a thin pale-gold border, 14px horizontal/12px vertical content
  margins. Keep role-specific headings/body/hints, card gauges and portrait reservations; do not
  force every label into a single font size. Dialogue's 30px body/24px hints remain its own defaults;
  battle rows remain 30px with their existing 64px minimum and native wrapping/scrolling.
- `StoryTheme.Selected` (`ffe19a`) marks an available current choice. The shared
  `Unavailable` (`8e9bab`) also serves existing entry-button disabled text. Battle labels override
  the native font color directly, avoiding multiplication by the inherited body color. Ordinary
  rows inherit the existing theme body color (`f4f5fc`).
- Focus and availability are separate: a selected battle row always retains `›`, including an
  unavailable row. Unavailable text stays muted regardless of focus, keeps its literal
  `unavailable` suffix and exposes the actual reason in the description. Unavailable target Confirm
  keeps its compact label and the description's explicit refusal reason; an enabled Confirm uses shared gold. Color alone never promises
  legality. Numeric HP/MP and existing Low HP/Defeated text remain the status-card authority.
- Existing native buttons retain hover/pressed/focus/disabled boxes from `CreateEntry`.
  Custom Label menus keep their existing deliberate pointer gestures, keyboard/pad focus,
  scroll clipping and release guards. They do not become native Buttons or acquire a second
  activation/focus authority. A stationary pointer cannot steal keyboard selection.

**Confirmed, source inventory and bounded implementation:** the selected consumers below use
existing mechanisms; other consumers do not become an automatic migration checklist. The finite
migration has bounded engineering acceptance; it does not assert human acceptance.

| Consumer | Current treatment and disposition |
| --- | --- |
| Entry, SaveSlot, NameEntry, Settings/control help, gameplay Save/Quit | Already consume shared native button/editor treatment. Regression consumers; no Stage1 rewrite. Preserve their separate persistence and release boundaries. |
| Gameplay system entry | The [system Menu](#system-menu-agreement-1518) consolidates the former NewGame/Save/Quit/Settings buttons using `CreateEntry`; existing action hosts retain lifecycle ownership. |
| `BattleMenuLayout` non-diamond rows/navigation | Stage1 actual consumer: shared selected/unavailable colors and independent marker/reason, including unavailable target feedback. Existing geometry, fonts, scroll and command authority remain. |
| `ChurchServiceView` and `FieldActionView` (Prepare) | Shared font-color overrides keep unavailable rows muted even when focused, with the existing marker/explanation/refusal. Church derives availability from its nullable command; Prepare uses actual `Use`/`CanDrop` offers. Action/menu/confirmation rows stay inspectable; no English-string parsing or new legality. The [finite feedback agreement](#gameplay-feedback-and-recovery-wording-1454) owns Church wording/transient refusal. |
| `PartyStatusView` / `PartyDetailsPanel` | Member focus has shared gold plus `▸`; Prepare/Close retain ordinary body text rather than permanent selected accents. Source grouping, gauges, pane hints, typography and detail scrolling remain. `PartyDetailsPanel` is unchanged. |
| `StoryChoiceRow` | Already uses shared gold plus a selection marker and the caller's phase/release rules. No change justified by this inventory. |
| Dialogue, ContextHint, status/detail panels, battle messages/results | Already use shared readable frames with distinct content roles. No Stage1 change; preserve portraits, numeric warnings and the existing reward sequence without another summary. |
| `BattleGiveView` / `BattleDiscardView` | Real results now reuse `StoryDialoguePanel` frame/body and existing reading pages over their retained veil. Reveal completion and token acknowledgement remain with each view; a fresh Confirm reads remaining pages before acknowledgement. |
| `BattleRetreatView` | Text uses the same panel/body/reading guard; effect/closure hide that panel. Cue/pulse, reduced-flash, focus suspension, payment and delivery phases remain. No original timing claim. |
| `BattleAppearanceView` | Explicitly deferred outside this opening route: default-size gold text/raw actor notice remain the finite later-consumer gap [#1463](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1463). No production or emergence-timing change here. |
| `BattleSessionView` auxiliary HUD | Actual Give/Discard/equipment choices now use one shared reading panel, projecting the current typed offer and index/count with existing mapped cycle/confirm/back guidance. Every offer remains reachable. Irrelevant spell/item/equipment stacks no longer crowd the current choice. Title/roster strings stay diagnostic. |

The auxiliary HUD replaces its separate label/scroll geometry with the existing `StoryDialoguePanel`,
not another paging implementation. Candidate/phase text changes reset reading; Cancel, directions
and Target remain command-owned and usable while unread. Only Confirm first reads remaining content,
then a later fresh Confirm reaches the original command. This also makes long current-choice text
readable with keyboard/pad. Existing battle pointer coverage remains the diamond/list/target menu;
the auxiliary keyboard/pad controls do not acquire a second pointer or input authority. World framing
is unchanged; the panel consumes the existing subject rectangle for stable top/bottom placement.

Acceptance uses adapter/necessary Debug builds, a clean committed dependency plan, direct
source/doc/private-boundary checks and representative actual 1080p menu/result consumers before
finite sibling checks. Observe real colors/fonts/geometry, explanatory unavailable focus/refusal,
current-choice reading, reveal/acknowledgement once and subsequent control without unintended resource
mutation. Reuse existing inputs/observers; fresh consumer outputs belong under `local/issue1453/stage2`,
with a 100 MiB budget and serial native runs of at most 45 seconds. No engine/full
suite, helper tests, screenshots, per-frame capture or whole route is needed for that recipe. These
are the migration's acceptance checks, not a new #1456 rerun requirement. Human aesthetics, physical release, actual
1080p-display/DPI approval stay with #1456; original media/timing and existing WAV teardown warnings
remain **Unknown**. The #1447 investigation below adds no human approval.

## Gameplay feedback and recovery wording (#1454)

**Standard rigor:** a finite presentation correction after the accepted shared visual consumers.
The outcome is an actionable battle movement refusal and an understandable Church offer/result,
with usable navigation after refusal. Scope is `BattleCommandMenu`, `ChurchServiceView`, the existing
`FieldService` message producer, this owner, the [gap disposition](../evidence/opening-ui-gaps.md#gameplay-feedback-disposition-1454)
and [necessary native recipe](../verification/host.md#finite-gameplay-feedback-observation-1454).
The Application remains the sole service/command authority; no price, status mask/order, legality,
rewards, story, input binding, storage or shared style changes belong to this slice.

- `movement-range` asks for a tile within the highlighted movement range; `occupied-destination`
  asks for an unoccupied tile within that range. The typed failure/code remains diagnostic, the
  failed command retains progress/resources, and ordinary legal input continues without a Retry.
- Church offers explicitly say Remove poison / Remove stun; paid results say Poison removed /
  Stun removed with the actual payment. Raise retains its existing meaning. These are direct typed
  effect mappings, not English-string parsing or additional supported treatments.
- Promotion stays inspectable, marked and muted. Its refusal says it is unavailable here and points
  to another service or Leave. Mapped Choose/Select/Decline/Back guidance remains visible alongside
  the refusal. Changing the focused row clears the transient error through the same focus update
  for keyboard/pad selection and an accepted pointer selection. Pointer hover/gesture and release
  guards retain their existing behavior. Compact installed hints and reserved three-line space keep
  refusal and navigation inside the existing panel.

Talk/Search already share interaction authority and Party offers Prepare. Dialogue already separates
reveal/read/continue/automatic states; accepted result/reward/return ordering stays intact. No guessed
speaker name, automatic timeout/skip, reward summary, UI framework or new recovery operation is added.
Acceptance uses existing FieldService behavior assertions, adapter/necessary Debug builds, a clean
committed dependency plan and direct scope/docs/private checks. Native evidence is limited to battle
rejection/legal continuation and Church refusal/focus/normal treatment, with actual revision/resources/
RNG and errors checked. Reuse short cases and observers; pilot before expanding, serial runs <=45s,
fresh `local/issue1454` output <=50 MiB, replan below 1 GiB free. No full route, screenshots, acquisition,
helper tests or unrelated suites. Main-gate independently reviews/merges the frozen Draft PR.
The finite correction is independently accepted and integrated in
[PR1465](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1465).
Human readability/physical devices/DPI remain **Unknown** under #1456; #1447 findings await
independent review and #1463 stays deferred.

## Human opening review boundary (#1456)

The [current play preparation](../evidence/opening-loop-acceptance.md#current-human-play-preparation-1456)
owns ordinary launch, isolated disposable saves and short New Game/dialogue/Prepare/Church/battle/
return/Save-Quit/Continue segments. Accepted #1451 basic preferences, #1452 grouped Controls help,
#1453 finite shared presentation, #1454 feedback and #1455 minimum-window recovery are integrated;
their native observations establish finite engineering behavior. The
[gap matrix](../evidence/opening-ui-gaps.md) records every E/D/B/R/X disposition and retains #689's
actual limited human approval. They do not establish current whole-opening human acceptance.

Readability, selected versus unavailable states, dialogue/control return, battle feedback, save
clarity and feel await the user's actual play. Real keyboard/controller off-focus release, an actual
1080p display with fullscreen/decorations and DPI use the
[human checklist](../verification/host.md#later-physical-focus-human-checklist-1456). Current results
remain **Unknown/pending**; injected events, node geometry and available builds cannot fill them.
Keep #1456 open for the explicit experience decision after engineering integration. No new font/art
direction, all-map UI or gameplay extension is authorized. #1447 has a bounded
[investigation-only finding](../application/opening-session-lifecycle.md#title-lifecycle-investigation-1447)
awaiting independent review; Appearance #1463 remains deferred.

## Battle Speed and selective messages (#1650)

**Standard rigor**, bounded [#1650](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1650).
Existing preference storage retains its consistency contract. The settled Movement → B inspection →
empty-ground A/C → Options → Speed caller opens the existing SettingsHost/SettingsView transaction
with only speed and ordinary hit/coin display. General Settings remains unavailable in battle,
including through the system Menu. The query must still admit current inspection; active movement,
scene, story, action and target stages gain no direct settings entry. Closing returns to the same
Options selection/cursor. The host pauses the battle/audio and reuses input release and focus barriers.
No session command, revision, RNG, resource, progress slot or session replacement belongs to editing.

Left/Right changes the selected draft; Up/Down selects its two rows. A or C accepts both and writes
through the existing SettingsStore before live publication; B abandons both. Read-only launch
settings retain their existing no-write policy. Write failures retain the draft/error and prior file
and live values. The preference `suppressBattleHitCoinMessages` defaults false for existing files;
it stays separate from journey saves and survives general Settings drafts. Bindings, language,
flash policy and arbitrary 1–240 cps remain intact unless explicitly edited in their owning settings.

| Original selector label | Accepted modern reveal mapping |
| --- | --- |
| 0 | adjustable,15 characters/s |
| 1 | adjustable,30 characters/s |
| 2 | adjustable,60 characters/s |
| 3 | instant; retain unused cps |
| Custom | existing adjustable cps outside those presets; retain exactly until Left/Right chooses a preset |

This mapping is an intentional modern presentation policy, not original VInt or speed equivalence.
Opening/accepting only a suppression edit never normalizes custom cps. From Custom, Right selects0
and Left selects3; presets wrap. S8's pinned source confirms two drafts, B rollback, A/C acceptance
and speed0–3 masking; the [source trace](battle-interaction-baseline.md#evidence-and-reproduction-boundary)
retains original authority. The finite [message policy](battle-scenes.md#selective-hitcoin-message-delivery-1650)
keeps critical, EXP, growth, items, status and story messages. This does not implement Shift acceleration.

**Confirmed, bounded actual native host:** ordinary authored stone-court input covers custom47.5,
B cancel, A acceptance preserving custom cps, all four presets, C acceptance, held input/focus release,
unchanged gameplay/RNG during the modal and return to Options. A new process loads the physically
written preference file, then completes the same attack with required EXP delivery. English/default
and Chinese/remapped inputs pass. Default/suppressed runs retain the same scene phase sequence and
final gameplay values, including RNG, gold, EXP, HP, inventories and next actor. The
[reproduction owner](../development-and-verification.md#battle-speed-and-message-observation) bounds
this observation; it is not natural original timing, physical pad hardware or human visual approval.

## Basic preferences agreement

**Standard rigor; [#1451](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1451).**
Deliver the four existing `InputSettings` preferences: instant/adjustable text, 1–240 characters
per second, standard/swapped Confirm/Cancel and reduced flash. Existing blue/gold controls expose
Settings and control help on the Witch menu. The title shows only Start. At settled exploration input,
the mapped Menu action opens the [system Menu](#system-menu-agreement-1518), then Settings; field Cancel opens the gameplay diamond. Settings remains
unavailable during active text, choices, Church, party menus, movement and battle decisions. The host pauses the existing
view/audio while the modal is open; no gameplay command, session replacement, revision, RNG or game
save write belongs to this operation. The separate [battle Speed caller](#battle-speed-and-selective-messages-1650) permits only its two preferences at settled inspection Options.

One draft spans Display & text, Gameplay aids, Controls and Debug & diagnostics. Back restores
the source row; top-level Back closes without writing. Gameplay aids → Restore defaults resets
preferences while retaining staged bindings. Controls → Reset bindings clears only overrides. Apply validates the entire candidate,
including device bindings, writes it, then updates the existing settings and input objects in place.
All mounted consumers retain those objects. Reduced flash and reveal policy apply to subsequent
presentation at this safe boundary. Hint refresh and a held-input release barrier prevent the old
Confirm event, pad deflection or key echo from accepting a subsequent screen. Installing admitted
bindings explicitly releases old Godot action state. A temporary barrier tracks only the activating
Apply event until its release/neutral edge; window focus loss clears it and the settings-held action
state. Global input admission retains its existing `GameplayHeld` rule without historical key sets.
Native focus loss suspends pointer gestures; an orphan release cannot activate a control. Control help reads the admitted bindings.

`--input-settings` is the highest-priority, read-only whole-file override. Settings explains that
controlled launch. Players can open Help and close Settings. Both files retain their contents.
For ordinary preferences load `<SF2_SAVE_ROOT>/<SF2_SAVE_NAMESPACE>/sf2-preferences/input.json`,
then existing model defaults only when absent or unreadable. The absolute root and 1–64 character
alphanumeric/underscore/hyphen namespace follow existing persistent-save conventions; the root must
be outside checkouts and link paths. There is no OS-directory or removable-worktree fallback.
Settings are separate from `sf2-opening` progress slots and never enter their schema. WM-close
first cancels the settings draft, then enters the existing Quit owner; settings and Quit never coexist.

Missing configuration/read failures show an explanation while current defaults remain usable.
Invalid content or bindings retain the original file and require explicit Restore defaults, then
Apply to recover; the replaced bytes are retained in a uniquely named `input.*.invalid.json` file.
Other read/write failures refuse Apply and retain the prior live settings and files. Each write
uses the existing style of exclusive file lock, same-directory flushed temporary, and atomic
replace/move; no settings framework, manifest, checksums or autosave is introduced. A settings file
changed by another process since load is refused until relaunch, using direct byte comparison.

The pilot covers draft/Cancel/Apply/write refusal before finite native observations expand to fresh
process reading, corrupt recovery, read-only override/custom bindings, held-remap and actual reveal/
reduced-flash consumers. Use `uv run sf2 verify adapter`, a committed dependency plan and bounded
windowed state/input/geometry/error reads. No new engine behavior or helper tests, full suite,
screenshots, per-frame acquisition or platform. Keep new local evidence under `local/issue1451`,
reuse existing tools/media/caches, budget 100 MiB and serial native runs of at most 45 seconds.
Reassess before broader acquisition or cross-engine changes. Volume is outside these settings; human visual acceptance and original timing/media remain Unknown.

Bounded implementation observations cover keyboard/pad/mouse entry and help, draft/Defaults/Cancel,
held keyboard/pad remapping, fresh-process loading, preserved custom bindings, explicit corrupt-file
recovery, unreadable storage, write refusal and externally changed-file refusal. At one stable field
input, Apply and Cancel retain session identity, revision, captured progress and the existing slot
bytes; the same mounted presenter reads changed settings. Gradual Witch text and suppressed mouth/
blink drawing consume the new values. Settings fits 1920×1080 and native 1280×720 with scrolling.
A real secondary native-window focus cycle omits old key-up and still admits a fresh new Confirm;
this is controlled input/focus evidence, not physical controller hardware approval. A direct Godot
WM-close notification confirms cancel-draft → existing Quit → Cancel → retained field control; real
OS WM-close delivery remains unobserved. Selected teardown runs emitted two ObjectDB-leak warnings;
a verbose focused repeat did not reproduce them. No memory-lifetime or all-device approval is claimed.

To reproduce the product boundary, load the existing private-input/entry selections and configure a
fresh persistent namespace outside the checkout, launch the existing Godot project without an input
override, and open Settings from the Witch action menu. Edit all four values, choose Defaults,
then Cancel; reopen and Apply the changes. Hold the old Confirm through Apply, release, and use the
new mapping. Relaunch in the same namespace to check persistence; use a disposable malformed file,
unreadable path or blocked writer lock for the explicit refusal/recovery cases. A separate launch
with `--input-settings <path>` must show read-only values/help without editing either file. Use the
ordinary New Game route for a settled field comparison, then active dialogue/Church to check refusal.
Do not change user saves or canonical inputs to construct these disposable cases. Exact local receipts,
retained failed observations and CI/head identities belong to the Issue/PR handoff.

### Optional Settings diagnostics (#1709)

The Standard preferences agreement also covers **Debug diagnostics**, default Off for new and
older formatVersion1 files. Its draft, Apply, Cancel, Defaults and read-only override use the same
transaction and storage. Defaults stages Off; Apply persists it. The row follows Modern guidance
without changing existing action identities. Graphics, guidance, bindings, help and progress remain
independent.

Off hides world/entry text loading status and dialogue/name counts from Settings. On exposes them
in either UI language after Apply. Ordinary instructions, partial-Chinese and graphics capability
information, and actionable startup/preferences/save failures remain visible in both states.
Coverage, catalog counts, errors and diagnostic observation interfaces remain available regardless
of this presentation preference; English coverage now also reports the loaded Chinese catalogs.

Reproduce with `res://probes/engine_settings_diagnostics_observation.gd`, existing selected private
entry/world/catalog inputs and an owned ignored controlled field start. Set explicit
`SF2_HOST_UI_OUTPUT`, `SF2_HOST_UI_START` and a fresh `issue1709-*` save namespace outside checkouts.
Use old formatVersion1 preferences without `showDebugDiagnostics`, then run `SF2_HOST_UI_CASE`
`transaction`, `restore`, `off` in separate headless processes in that disposable namespace;
repeat in English and Chinese. `override` uses an explicit read-only On file. The observer uses
mapped input and actual projections, including storage refusal, paused progress/RNG and help.
Each run is bounded to55 seconds/<2 MiB with finite samples; no screenshots, focus requests,
new helper tests, export or original-game fidelity claim. Build the adapter and inspect the clean
committed verification plan. Exact receipts and completed failures belong to the Issue/PR handoff;
human visual approval remains Unknown.

### Debug auto-win battles (#1720)

In Witch or field Settings, turn on **Debug: auto-win battles** (简体中文 **调试：战斗自动获胜**)
and Apply. It defaults Off, including older preferences. The Debug diagnostics display is independent.
Each battle plays its entry/mandatory presentation and ends at the first available player movement
turn. The campaign uses the encounter's ordinary victory program, recruitment, flags and field return.
An encounter with a first-time enemy-defeated scene plays that script before its after-battle program.
Held combat resources pass to the ordinary outcome policy. Skipped actions grant no kill credit,
drops, gold or experience. Normal story acknowledgements still need player input. In standalone
battle practice the terminal result is Victory. Turning the setting off restores ordinary play.

For native replay use the selected pre-voyage world, a disposable `issue1720-*` namespace,
old version-1 preferences and `res://probes/engine_debug_victory_observation.gd`. Set
`SF2_HOST_UI_OUTPUT` to a fresh ignored report and `SF2_HOST_UI_START` to a worktree-local copy
of `remake/reference/inputs/battle02-outcome-start.json` with `start.partyInput` set to
`remake/reference/inputs/battle02-outcome-party.json`. Run `SF2_HOST_UI_CASE` Off (`off`, English)
and On (`on`, Chinese) separately. The observer uses ordinary Settings input, checks the live
Battle02 turn or completion and compares four members’ earned resources at usable Map66 return.
`DebugBattleVictoryTests` covers interrupted commands, terminal practice and staged defeated dialogue.

## Minimal contextual hints (#1544)

**High rigor, bounded implementation under the shared UI agreement.** Both input families remain
usable at all times. Ordinary entry/host, exploration/story and battle cues show one installed binding
for the most recently effective keyboard/mouse or pad input. This replaces the earlier deferral of
automatic switching. Gameplay admission, source timing, transactions, save formats and physical-keyboard
naming remain with their existing owners. No new device service, glyph dependency, tutorial storage,
binding-capture UI, click-to-walk or Help entrance is added.

### Ownership and acceptance

The shared semantic boundary is Godot input observation and disposable hint projection. Exact owned
paths, relative to `remake/`, are:

- `game/src/Input/GameInput.cs`, `GameInput.Hints.cs` and `MenuPointer.cs`;
- `game/src/GameRoot.cs` and `game/src/Entry/SettingsHost.cs`;
- `game/src/Entry/SessionEntryView.cs`, `SaveSlotView.cs` and `SystemMenuView.cs`;
- `game/src/UI/SettingsView.cs`, `ControlsHelpView.cs` and `WindowRecoveryView.cs`;
- `game/src/Exploration/ExplorationDialogueView.cs`;
- `game/src/Battles/BattleCommandMenu.cs`, `BattleSessionView.cs`, `BattleInspectionView.cs`,
  `BattleStoryView.cs` and `BattleSceneView.cs`;
- `game/src/Localization/GameText.cs`, `game/localization/en.po` and `zh_CN.po`;
- `game/probes/engine_active_hints_observation.gd`, this agreement, the hint entry in
  [opening UI gaps](../evidence/opening-ui-gaps.md#minimal-contextual-hints-1544), and the affected
  [scene-review findings](ui-scene-review.md#9-remaining-differences-in-play).

Domain/Application, private inputs, player saves and delivered demo checkouts are outside ownership.
Existing Party/preparation, field/service lists and Church/Shop directly consume `CompactHint` from
their regular presentations; no second binding table or focus authority is introduced there.

Acceptance requires current English/Chinese entry, system/Settings/Help, field reading/choice and
battle decision/target inputs; cached cues must refresh without submitting an action or changing
selection, target, reading position or name focus. Verify installed remapping and actual swap Apply,
unknown layout, no attached pad, another pad identity, disconnect, drift/micro-motion/echo and native
focus recovery. Preserve full Help, costs, refusal/recovery reasons, explicit payment/overwrite/quit
consequences and existing Cancel-first confirmation owners. Adapter build, actual input/state/geometry
observations, direct scope/private checks, committed planner and actual scoped CI are the gates.
Independent main-gate integration and #1456 human hardware acceptance remain separate.

### Mappings, first use and reading limits

`GameInput` owns only the active hint family, pad identity/layout and fresh-edge bookkeeping. Fresh
non-echo keyboard presses, real mouse presses/wheel or obvious relative movement (at least4px), pad
button presses and axis deflections beyond0.5 select the family. Releases, action echoes, emulated
mouse events, sub4px motion, drift, unchanged holds, connection alone and focus transitions do not.
Axis neutral below0.35 rearms both hint and existing action edges; gameplay retains its0.5 admission,
so one deflection cannot execute Menu twice through dead-zone chatter. Focus return
seeds held pad controls and mouse position rather than making a device choice. Disconnecting the
active pad falls back to keyboard/mouse; idle controllers do not become active. Another effective pad
updates identity even while the family remains pad. Mouse hover in native entry/slot buttons and
existing `MenuPointer` consumers uses the same intentional-motion minimum.

Use the installed binding dictionary after Confirm/Cancel swap. `CompactHint` chooses only the current
family's first effective alternative; if an action has no pad alternative it explicitly says Keyboard
plus its real key. `Hint` retains all alternatives in Help. The existing native
[Godot Input API](https://docs.godotengine.org/en/stable/classes/class_input.html#class-input-method-get-joy-name)
supplies known status and mapped controller name. Recognized Xbox360/Xbox/XInput, PS3/PS4/PS5 and
Switch Pro names select their face/shoulder/menu labels. Ambiguous names use localized face-position,
D-pad, shoulder and stick-direction words, never internal South/East enums or a guessed brand.
Physical layout recognition and OS/Steam remapping remain bounded by what Godot exposes.

The host refreshes mounted entry, slots, Menu, Settings/Help and recovery cues directly on a hint
change or Apply. Battle decision/auxiliary footer caches refresh directly without resolving input again; active field
and service consumers read the current hint in their existing frame presentations. Settings Help retains its page/scroll, the draft keeps
its ordinary owner, and name entry keeps its physical-keyboard editor and fixed Enter/Escape.

At settled exploration show legal Talk or Search only; retain the visible gameplay Menu and its Member
entrance. Dialogue shows Reveal, Read more, Continue or automatic progress as actually admitted. Ordinary
short Dialogue/W1/field spans retain #1582's one-fresh-Confirm Reveal / Continue; choices and overflowing
readers retain their separate reveal/read/answer boundaries;
choices explicitly say Choose/No. Battle Movement shows Actions; command/target contexts retain the
selected action/cost/range/reason and one current choose/execute/back cue. Equipment/Give/Drop stay
visible where they have no equivalent action rows and Help is unavailable. The full battle catalogue
no longer occupies ordinary chrome. Keyboard/mouse targeting additionally states that hover previews
and a complete legal-target click executes immediately. Existing invalid-target, press/release and
single-action rules are unchanged. No first-use persistence or automatic tutorial is introduced.

Long text wraps and uses the owning reader/scroll mechanism, with existing30px body/24px hints and
1920×1080 logical design baseline. The cue may grow to retain a necessary reason or consequence. Help remains
available through existing Witch menu or settled-field Settings, with full localized English/Chinese
prose. Story/battle Menu does not make Settings/Help newly available. Screenshots are prohibited.

### Finite implementation and acceptance boundary

The focused [production observer](../../game/probes/engine_active_hints_observation.gd) reuses existing
native input/state/geometry helpers and registered prelaunch content. It has `SF2_HINT_CASE=entry`,
`field` or `battle` (`menu` stops after field modal/Apply checks; `recovery` checks the real undersized-client/Cancel route; `reading` stops after the first real partial-text Reveal); output uses a fresh ignored `SF2_VIEWPORT_OUTPUT`. Start with the
[ordinary environment](../development-and-verification.md#locked-net-workflow), build the Debug adapter
and import changed localization resources once. Select both final world/battle-scene publications.
Use an explicit fresh disposable external save root/namespace and its `sf2-preferences/input.json`,
never a player namespace. No game save is published by the observer. For remapped cases set real
version1 preferences before startup (for example Confirm=Q/left face and Attack=F/lower face);
field's actual Settings Apply then swaps Confirm/Cancel.

```powershell
$env:SF2_HINT_CASE = 'entry' # field / battle select the corresponding controlled start below
$env:SF2_VIEWPORT_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
$env:SF2_COMPACT_START = $controlledStart
& $env:GODOT_BIN --path remake/game --audio-driver Dummy --max-fps 120 `
  --resolution 1920x1080 --script res://probes/engine_active_hints_observation.gd
```

`entry` drives real title/Witch → New Game → empty disposable slot → keyboard name → Back. It checks
noise/echo, another pad ID, a native focus-window round trip and disconnect-signal routing. `field`
uses the existing prelaunch input: Map3/entity-0 at(42,9), facing3/speed32, flags0/32/256/260/261/601/602;
ordinary Down/Right reaches the source531 question after its actual preceding scene. `battle` uses the
existing initialized controlled Battle01 input and runs keyboard Actions → pad Attack → mouse target
preview/full click. Controlled placement is not natural campaign-reach evidence.

Budget one native process at a time,55s per launch (the existing observer has a50s deadline), under2MiB
of new reports/logs per case, and no media copies or per-frame dumps. Field's source scene needs about
46s, whereas entry/battle checks are a few seconds. Preserve failed pilots and rerun the affected case
only. An initially shorter source deadline showed ordinary scene progress, not a gameplay deadlock.
The local launch records and complete failures remain in the independent ignored `local/issue1544`
namespace; only the reusable observer and reproduction contract are tracked.

**Confirmed — bounded remake observations:** actual English/Chinese Witch/slots/name, ordinary
Settings swap Apply and Help reading, source531 Cancel=No, private Battle01 keyboard→pad→mouse,
axis Menu chatter, missing implicit Menu pad and native recovery/quit Cancel pass. Pure hint refresh
retains their observed choices/readers/resources. Keyboard and pad remapping use the real installed
InputMap; both families remain operable after the label changes.

**Unknown:** physical hardware/glyph recognition, two actual connected controller replacement,
OS/DPI/Steam combinations, human reading/discovery and aesthetic approval remain with #1456. Injected
native pad events and disconnect signals establish software routing, not hotplug hardware acceptance.
Existing ObjectDB-at-exit warnings remain separately attributed; a successful cue observation does not
resolve their ownership. Paid-service, storage-error and full-campaign gates retain their earlier
owners; this input-display change does not repeat or widen those claims.

## Grouped controls help and remapping boundary

**Standard rigor; [#1452](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1452).**
The outcome is complete keyboard/pad reading of installed controls through the existing Witch menu
and settled-field Settings entry. A separate help view owns only reading position and pointer gestures.
Settings retains its draft, Defaults flag, message, selected Control help row and scroll position;
Back returns there without Apply, storage access or a gameplay command. Help pages cover exploration,
dialogue/choices, battle and menus, including keyboard-only naming. Page labels explain context;
a binding does not promise that its command is legal in every state. Existing visible disabled
reasons remain authoritative. Active battle/story settings access is not added.

Left/Right turn pages; Confirm also advances. Up/Down scroll the current page, including unusually
long custom bindings. Fixed Previous/Next/Back controls also support deliberate pointer gestures.
Every binding string comes from `GameInput.Hint`, after the installed Confirm/Cancel swap; pending
draft edits never masquerade as installed controls. The basic accessibility controls cover
instant/gradual text, text speed, Confirm/Cancel swap and reduced flash.
No device service, new persistence, arbitrary scale, screen reader or new device promise is added.

Acceptance uses adapter build, clean committed dependency plan, direct docs/diff checks and finite
native input/state/control-geometry/error observations. Pilot one page and Back/draft/focus first;
then read every page with keyboard and pad, standard/swapped/custom bindings, pointer return and
1920×1080/native1280×720. Retain session/progress/slot bytes across field help. Reuse installed tools,
project and existing local observers; ignored issue1452 reports budget 100 MiB, serial runs 45 seconds.
No screenshots, whole route, aggregate suite, helper tests or new verification platform. Existing
AudioStreamWAV/AudioStreamPlaybackWAV teardown warnings remain **Unknown**, outside this slice.
Human readability/aesthetic approval remains [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).

A custom-help sample exposed a concrete pre-existing admission defect: Godot accepted an InputMap
with more than 32 events but failed when a late event was parsed. Main-gate authorized the bounded
`GameInput.Prepare` correction within #1452: reject any action with more than 32 total key, button
and signed-axis events before returning a prepared candidate. This follows Godot 4.7.2
[`Input::MAX_EVENT`](https://github.com/godotengine/godot/blob/4.7.2-stable/core/input/input.h) and its
[event-index guard](https://github.com/godotengine/godot/blob/4.7.2-stable/core/input/input.cpp).
The existing load/Apply paths validate before installation and, for Apply, before persistence.
An invalid saved configuration retains its bytes and uses the existing explicit recovery notice;
invalid overrides fail startup. No storage policy or binding-conflict rule changes. The original
failed sample/log is preserved in local issue1452 evidence; this is not a remapping UI expansion.

**Confirmed, bounded executor observations:** all 12 pages are readable through keyboard and pad
page/scroll input with standard, swapped and custom installed mappings; a read-only launch override
also exposes complete help. Native 1280×720 and 1920×1080 controls retain fixed navigation and a
scrollable reading area. Keyboard/pad/mouse Back retains the pending draft, Defaults status, selected
help row and Settings scroll. A real New Game field visit retains session identity, revision,
captured progress and slot bytes throughout help and Settings Cancel. No command/save write occurs.
At the capacity boundary, 31 keys plus one pad event install and the final pad event works; a saved
33-event configuration retains its bytes, shows invalid-preferences recovery, installs defaults as
a whole and refuses Apply before reset. Exact commands, retained failures and CI/head are in the
Issue/PR handoff. These observations do not establish physical controller or human visual approval.

Reproduce after the existing entry/private-input setup: use a fresh disposable persistent namespace,
open Settings at Witch menu or settled field, stage a preference/swap/defaults change and select
Control help. Read every page via Left/Right or Confirm and Up/Down, then Back to the same draft/row.
Repeat with pad navigation and visible pointer controls, native 720p field and installed swapped/custom
bindings; pending swaps must never change the binding text. Compare session/progress/slot bytes across
field help. For capacity, use 31 unique nonconflicting keys plus one pad event on one action; then
add one key in a separate disposable namespace. The latter must show recovery/defaults, retain its
file and refuse ordinary Apply. Do not modify canonical inputs or user preferences for these cases.

<a id="deferred-per-action-remapping-design"></a>
### Change controls

Witch Settings or settled field Menu → Settings → Controls → Change controls edits any of the
17 base actions. Choose an action and device, replace/add/remove an alternative, review it,
then return to the top level to Apply or discard the visit. Installed controls operate the
editor until Apply succeeds. [Format and reproduction](input-remapping.md) and the
[E2 feature row](../evidence/opening-ui-gaps.md#e2-settings) own the current boundaries.

## Menu pointer agreement

**Standard rigor; [#1408](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1408).**
This bounded input change covers the existing battle command diamond, spell/item lists, ordered
targets and Stay confirmation, followed by the existing field Yes/No choices. It retains the
accepted 1920×1080 geometry, original admitted command rasters and shared story panel. World movement,
map/actor picking, title/settings menus, supplementary Equipment/Give/Drop and Party Status mouse
redesign, new targeting rules and full-game mouse-only play are outside this agreement.

Actual pointer movement over an option takes focus and updates its description. Keyboard/pad
navigation takes focus back; redraw and a stationary pointer never select another option. Disabled
entries remain inspectable and explain their refusal. A complete left press/release on the same
current control activates that option. Blank clicks do nothing. Dragging out, double-click tails,
focus loss, hiding, control replacement, keyboard handoff and changed session/menu boundaries cancel
an old press. A release without its own press cannot activate a newly attached menu or question.
Native wrapped-label geometry settles independently of input before hit regions are used.

Action/list clicks enter the existing target or Stay flow. Previous/Next cycle the existing ordered
candidate set, including unavailable candidates. Confirm commits only the current enabled candidate;
inspecting an unavailable candidate cannot commit an earlier valid target. Back and right click
unwind one layer, preserving the existing movement-preview, resource and RNG rules. Wheel scrolling
is confined to the open list. Existing keyboard/pad commands retain their ordinary bindings.

Yes/No starts with Yes focused. Pointer hover, then directional selection, share the displayed focus;
Confirm answers that focus. A deliberate click answers the clicked option. Cancel and right click
mean No, explicitly labeled in the hint. Authored focus is disposable UI state; the answer still uses
the existing `ChooseDialogue` command. Source focus remains `ChoiceWork.Yes`; the same command now
accepts an explicit answer only in its admitted Input phase, using the existing choice runner's
selection and closing path. Entry, held-button Release, Opening, Closing, result publication and
program continuation remain Application-owned. Reading/reveal and matching wait-token guards
precede pointer admission. A completed answer never writes a story flag from a widget.

The [finite host recipe](../verification/host.md#menu-pointer-observation) owns controlled inputs,
windowed focus/drag/layer observations, headless battle and source/authored choice observations,
completed failures and remaining physical-device limits. Small actual Application behavior tests,
adapter compilation and committed scope selection are the gates. No research suite, H3/H4 comparison,
new input framework, recorder or asset pipeline is introduced. Independent main-gate integration and
broader human/aesthetic acceptance remain separate.

## Diamond focus emphasis

**Standard rigor; [#1412](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1412).**
The existing diamond has one short focus-change emphasis shared by keyboard/pad and pointer focus.
The selected drawing rises four display pixels, then settles through two and one pixels to zero
over 180 ms. It uses the existing normal/selected 24×24 resources, four-times nearest sampling and
integer draw coordinates. Text-only public squares use the same drawing offset. This is a modern
presentation choice; original animation cadence remains **Unknown** and creates no acquisition task.

Only a new selection starts the emphasis. Idle, refresh, repeated current-direction input and a
stationary pointer never restart it. Another focus cancels the previous item's motion. Closing,
list/target handoff, hiding, pause, native focus loss and scene retirement cancel pending emphasis;
showing or regaining focus cannot resume it. Reopening starts from the current menu selection.
Reduced-flash always draws the stable selected image without motion.

Only drawing changes: cardinal controls, hit rectangles, description, disabled reasons, resources,
RNG, input admission and explicit command confirmation keep their existing authorities. Frame
processing advances a button's short timer; it submits no command and plays no sound. No general
animation framework, art pipeline or gameplay state was added. The
[native recipe](../verification/host.md#diamond-focus-emphasis-observation) owns actual drawing,
lifecycle observations and mounted-original-icon acceptance evidence. Merged
[PR1413](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1413) owns bounded engineering acceptance;
whole-opening human/aesthetic approval remains Unknown.

## Battle messages, return and Party Status

**Standard rigor; [#1398](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1398).**
This bounded presentation slice consumes accepted E1 and C. Existing action, damage/heal, status,
death, EXP/gold/growth message order, tokens, payment and outcome programs remain session-owned.
`BattleSceneView` uses the shared blue/gold `StoryDialoguePanel` at native 30px, with mapped Reveal,
Continue and eligible Wait hints. New tokens reset reading position; same-token refresh retains it.
Display-only reading pages precede acknowledgement. E1 cards retain their live actor resources.

Battle chrome distinguishes ready selection, “Battle in progress…”, rejected commands with their
reason, and “The battle has stopped” with the retained failure. A visible command menu owns its
recoverable IllegalCommand reason; the global feedback panel stays hidden to avoid covering the
menu description and controls. Errors without a menu and fatal/startup failures retain global feedback. Startup/adapter errors use the same
visible feedback panel; `ContextHint.Arrange` explicitly shows nonempty text. Fatal stopped sessions
cannot continue automatic or ordinary input. Diagnostic IDs/stages remain in observations.

At the actual battle-to-field handoff, battle windows retire while the battlefield may remain beneath
field-owned after/return effects. Existing reward messages and outcome programs finish normally.
A transient Victory/Defeat/Returned notice becomes “Back in the field” only at `CanWaitAtInput`;
it introduces no acknowledgement or reward summary. Ready return also releases any retained battle
mount. The first fresh field command dismisses the notice and executes normally.

At settled field input, the existing Equipment binding (default E / Pad Start) opens **Party Status**.
The E7 extension's agreement below owns the roster/details layout and read-only data projection.
Opening, browsing and closing submit no session commands. Busy or failed field state cannot open it;
a changed field boundary closes it. Existing return feedback and reward messages retain their own
ordering and accounting.

Acceptance is adapter compilation and the clean-head scope plan, plus the
[bounded host witness](../verification/host.md#battle-return-and-party-status-observation).
No Domain/Application/Content behavior changes require new unit tests. Native observations use
headless Godot because external joypad interference remains unresolved in windowed runs; this is
not a new windowed acceptance claim. Main-gate independently accepted the bounded victory/defeat,
return, private/physical close-up visibility, startup/program failure and recoverable illegal-move
feedback observations. The host owner records the exact evidence and retained limits; integration
is tracked by [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400).

## Party details agreement

Current F5–F7 layout/navigation is owned by [Field, party and shop UI](field-and-shop.md).
The source/capability boundaries and retained earlier layout observations below keep their stated scope.

**Standard rigor; [#1416](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1416).**
This bounded E7 extension lets the player inspect preparation and returned progression. The user's
current direction replaces the persistent left roster/right prose page with original member-screen
information grouping, at a readable 1920×1080 scale and the existing blue/gold style. A compact member
selector preserves keyboard/pad/mouse browsing. Left summary: reliably bound admitted portrait,
carried kills/defeats, gold. Right: name/class availability, aligned Level/Attack, HP/Defense,
MP/Agility, EXP/Move rows; below, magic and four-slot inventory in parallel columns. Shared HP/MP
gauges remain; long lists scroll independently, and long names wrap. Missing metadata remains
unavailable, distinct from empty slots or no learned magic. No new class/status/jewel system or art.

**Confirmed static arrangement:** pinned
[`memberscreen.asm:BuildMemberScreen/BuildMemberStatusWindow`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/memberscreen.asm)
and [`sf2enums.asm` member-window constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm#L2227)
place portrait/KD/gold left, main status right. The 42-byte row stride gives name(row1,col1),
Level/Attack(row3), HP/Defense(row5), MP/Agility(row7), EXP/Move(row9), then magic/items at
row13,col1/10 with three-row entry strides. The accepted
[layout owner](../../../docs/design/contracts/interface/ui-layout-data.md) binds ROM92770 and546words;
its historical fixture width26/height21 conflicts with the source's21-column stride/26 rows
(`SIZE=$151A`, `NEXT_LINE=$2A`). Preserve that discrepancy; no research fixture is repaired here.
These source placements prove static grouping, not original runtime pixels, timing or natural reach.

Readable spacing, numeric values instead of original tile-level spell indicators, shared modern
gauges, explicit unavailable class/portrait labels and a compact dropdown selector are intentional
accessibility/presentation deviations. Class display names and complete status/jewel presentation metadata are not admitted. Status/jewel
decoration and the animated map-sprite preview are outside this bounded layout and are omitted. Existing
`ExplorationPresentation.cs` plus `ExplorationRasterImages.cs` share the unchanged portrait crop;
no pixel acquisition or animation lifecycle is added. `PartyDetailsPanel.cs` replaces prose formatting
with aligned rows and independent lists; Application/Domain queries/rules stay unchanged.

`GameSession.QueryPartyDetails` reads the current carried party and admitted definitions. The existing
`MapPartyMembership.Rebuild` with current flags supplies joined membership where source member
bindings exist; reserve membership is included. Seeded future allies and enemy records are excluded.
Authored packages without source member bindings retain their carried-ally fallback. Current progress
and carried loadout precede admitted baselines. The existing Domain equipment owner computes attack
for both battle construction and the query; no temporary battle or mutable actor copy is created.
Private item/spell display names come from admitted `PrivateBattleDefinitions`; authored definitions
supply weapon/item labels and readable learned spell identities. There is no UI catalog keyed by
source IDs, class-name invention or second roster authority.

With member focus, Up/Down changes member; Left/Right always changes member. Target (default
Tab / Pad RB) cycles member, magic and items focus; Up/Down scrolls the focused list.
Cancel/Equipment closes; Confirm opens the existing field Prepare overlay. A completed left click opens the compact selector, selects a member,
uses Previous/Next or closes the page; selecting a dropdown row closes that dropdown. Wheel scrolling
stays inside the open selector or its magic/items pane. The existing
`MenuPointer` rejects orphan releases, drag-out, stale selection, double clicks and suspended presses.
Opening/visibility/focus suspension requires released gameplay input; the field input owner clears
movement holds, and modal input cannot reach the field. A fresh command after closing resumes field
control. Selection/focus/scroll are disposable UI state; no gold, resources, growth, inventory, flags,
RNG or session revision is written. Use/Give/Drop/Equip/field magic, saves, shops, title/settings,
new content admission and full inventory services are outside this agreement.

Acceptance uses small actual engine behavior checks, adapter compilation, the clean-head scope plan
and [native party-details witness](../verification/host.md#party-details-observation). Executor
headless observations establish the named input/projection boundary; [PR1417](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1417) owns independent
main-gate windowed acceptance and integration status. Physical-device release, whole-opening human/aesthetic approval and
pinned Godot audio shutdown warnings retain their existing Unknowns.

Party details and the existing Prepare overlay use the [M2c English/zh-CN boundary](../application/localization-design.md#implemented-m2c-party-and-field-prepare-boundary).
Displayed stat titles are independent of stable node IDs. Member/item/spell identities stay admitted
or literal; missing data remains unavailable. Menus retain typed command closures, confirmation,
release/pointer state and revision authority. Known error codes select localized recovery while
retaining exact diagnostics; measured title/footer height keeps the return choice readable. No field
capability or member-screen redesign is added. Settings can be applied after closing Party, then
reopening it. [Native observation](../verification/host.md#localized-party-and-field-prepare) owns the
bounded 1080p geometry/input evidence; human translation/typography remains unaccepted.

## Localized battle command and equipment display

The [M2d consumer agreement](../application/localization-design.md#implemented-m2d-battle-menu-and-status-boundary)
owns English/zh-CN movement, command/list/target/commit labels, hints, finite failure recovery and
actual equipment HUD. BattleDiamondButton separates stable Identity/Name/raster lookup from Caption;
original mounted raster lettering is retained and the selected heading/unmounted fallback translates.
Typed action Empty/Kind/Spell/ItemSlot and equipment Slot4 select generic wording, preserving admitted
source labels and custom names. Refresh retains command/target focus and does not restart animation,
submit work, advance a round or consume RNG. Existing list/ReadMore/subject avoidance own layout.

BattleStatusCard translates low-HP/defeated suffixes for every shared caller. HP/MP gauge identities,
resources, numbers, colors and geometry remain unchanged. Global recovery uses actual snapshot
Unsupported/Faulted/startup status; choice Unsupported remains cancellable. Code/field diagnostics
stay visible. Give/Discard/Retreat and other dynamic result text remain outside M2d; diagnostic
observation strings are not a claim of visible translation. Settings chooses language before battle.
See the [finite native recipe](../verification/host.md#localized-battle-commands-targets-and-status).

## Localized battle transactions and retreat display

Connected Battles01–07 use the same localized equipment HUD and installed Equipment mapping
(default E / Pad Start) as authored equipment. The ordinary command diamond advertises that binding;
directions select eligible carried slots/Unarmed, Confirm commits and Cancel returns to the same
action menu/preview. Authority and free-return semantics remain in Application. The
[connected admission](../content/profiles-and-trust.md#connected-battle-equipment-admission) excludes
unsupported gear and alone does not enable battle Give/Drop. The
[native observation](../development-and-verification.md#connected-battle-equipment-observation)
checks actual English/Chinese projection and mapped input, with no new presenter state or visual
fidelity claim.

The separate [connected Give binding](../content/profiles-and-trust.md#connected-battle-give-admission)
uses the existing advertised G/installed mapping and localized slot/recipient/return-slot/result
consumer. Its [native observation](../development-and-verification.md#connected-battle-give-observation)
checks English and Chinese remapped input, committed selected item/member names, free cancellation,
equipped ordinary/full transfer and exactly one Stay suffix. The presenter keeps no inventory authority.
The separate [connected Drop binding](../content/profiles-and-trust.md#connected-battle-discard-admission)
uses advertised R/installed mapping and the existing localized selector/confirmation/result.
Its [native observation](../development-and-verification.md#connected-battle-discard-observation)
checks ordinary/equipped removal, free return and outer movement undo retaining committed inventory;
Chinese remapped input displays selected source names. The presenter owns no inventory or Deals state.

The [M2e consumer agreement](../application/localization-design.md#implemented-m2e-battle-transactions-and-retreat-boundary)
owns English/zh-CN Give/exchange and Discard current item/recipient/confirmation/results, protected/
rare/equipped explanations, auxiliary hints and authored EGRESS/Angel Wing feedback. Views format
existing typed operands/payment branches through GameText, with whole English template fallback;
canonical Text/TextFor and source/custom name precedence remain. They never parse an English label.
The auxiliary HUD reuses the existing equipment cycle/select/back hint plus the Give recipient hint.

Results keep the existing StoryDialoguePanel/reveal/ReadMore guard. Give acknowledgement ends the
action; Discard preserves action/movement preview; retreat retains Text/Effect/Close/Return, separate
tokens, once-only MP/item payment, cue/pulse/reduced-flash and focus suspension. Display refresh and
reading cannot submit another transaction, consume RNG or reconstruct a result. Settings chooses
language before battle. Other dynamic results, private dialogue/terminology and source-image
lettering remain outside this group. The [native recipe](../verification/host.md#localized-battle-transactions-and-retreat)
records finite state/input/geometry/error observations; human translation/typography approval and
original-game timing/natural reach remain separate.

## Battle status-card agreement

**Current production composition:** #1686 uses compact decision/playback HP/MP cards; class/level/status remain in full details and terrain in hover inspection. The older full-card geometry/acting-card drivers in this section describe their retained [pre-migration implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes). Current input/geometry reproduction is the [F4/F8/F9 production observer](battle-interaction-baseline.md#direct-production-reproduction); disclosure and original evidence below retain their scope.

**Standard rigor; [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).**
E1 is a bounded presentation change spanning command, target and action-scene consumers of the
same authoritative session. `BattleStatusCard` supplies one readable name, labeled HP/MP image
frame/track/fill and current/max numbers. No new rule, resource authority, rename feature, animation
clock, generic UI framework or asset pipeline is introduced. E2's party/result/field information,
rewards, mouse, command animation and additional statuses remain outside this agreement.

`BattleCharacterNames` centralizes presentation names. Admitted scene member names (including
selected custom names) use the source party-member identity where available; admitted enemy names
use the content's actor binding. Public-authored packages deliberately title-case their hyphenated
content identities, for example `guard-a` → “Guard A”. They need no private content or invented
original character mapping. No runtime rename operation exists in this slice. Diagnostic actor IDs,
scene target/displayed-side fields and observations remain stable. Existing authored healing wording
keeps its engine clock; the view replaces only its known leading subject with the readable name.

At the accepted 1920×1080 viewport, cards begin at 500×192 and the 32px safe edges. Native smart
wrapping keeps complete ordinary names and valid 64-character IDs in two lines at 26–22px, widening
the card where needed. Name lookup normalizes whitespace only for display. Width expansion has at
most 19 steps and stops at 800px; exceptional metadata beyond that capacity retains its normalized
text in observation but uses a two-line native ellipsis fallback. Original name/story data is unchanged.
The menu retains its existing subject-avoidance algorithm, diamond/list geometry, controls,
fixed 96px world scale and movement-range visibility. A target card appears only on its target page;
Cancel and actor changes clear it. Context moves below a visible target card. Action scenes use
the same component at the safe top edges without changing the scene canvas or world framing.
The existing native minimum-size notification resets hidden/wrapped controls after actual sorting,
preventing a temporary width-1 measurement from retaining an oversized frame.

Cards project current `BattleActorState` HP/MP and progress maxima on every session presentation,
including updates within a single wait token. They never use prepared final reaction operands as
vitals. MP changes at the existing SpellCost commit; damage/recovery changes at Reaction. A full-HP
heal can spend MP without an HP event. Bars crop the fill image immediately; they cannot delay a
wait or progression. Zero maximum MP displays “—” and an empty track. HP at/below one quarter has
an explicit “! Low HP” cue; zero HP has “Defeated”. Growth uses the then-current maxima.

The asset inspection found no admitted gauge in the local asset repository at
`293c9460818f4768905be809e83488d77f698fba`. Four minimal public-authored CC0 PNG segments now live
under the ordinary game's [status art directory](../../game/assets/ui/battle-status/README.md),
with lossless/no-mipmap imports and nearest sampling. They contain no original game pixels and
make no original-art or hardware-fidelity claim. Candidate SVG meters are not mounted resources.
The [asset owner](../content/presentation-assets.md#public-authored-battle-gauges) records that boundary.

Acceptance uses adapter compilation, the committed-head scope plan and the
[bounded native recipe](../verification/host.md#battle-status-card-observation): public long/low/full/
dead/zero-MP, target/cancel/actor changes, damage/heal/cost phases, multi-target AURA/Prism and a
necessary private-local menu/HEAL seam. No engine/data behavior changes require new unit tests.
Retain completed observer failures and the existing focus/release/shutdown Unknowns. Independent
main-gate integration and broader aesthetic/all-map approval remain separate.

### Ordinary acting-card information (#1655)

**Standard rigor:** a bounded extension of the existing card/query lifecycle, spanning Application
disclosure and its Godot consumer. The ordinary player card now includes ally class/level and
actor-qualified land effect, retaining the existing HP/MP image gauges, names, input authority and
96px world cells. Enemy class/level are not disclosed. Missing class/name values remain unavailable;
live private HP/MP values at or above 100 remain unavailable, while authored values keep their policy.
No member list, transaction admission, action-order prediction or #1616 visual redesign is included.

`GameSession.QueryBattleActingStatus` reads any current ordinary player selection through the existing
`HasBattleControl` boundary, including Equipment/Give/Discard selection and its own actual player/undo
delivery. It rejects automatic,
uninitialized, dead, unplaced/hidden actors and other scene/program/outcome/fault/field boundaries.
`BattleUnitProjection` is the single shared scalar/name/class/land owner for this query and settled
inspection; neither query initializes an actor or reads prepared reaction results. Inspection's
settled-origin admission is unchanged. The acting query accepts no arbitrary tile or inspected unit.
`BattleCommandMenu` consumes that disposable result alongside the current choices; next actor,
inspection dismissal and ordinary return presentation requery the live session rather than retain
another stats cache. Existing auxiliary shells still own their visibility; selection retains the
acting projection, while actual Give/Discard presentation waits remain unavailable. Inspection hides
the ordinary card and its existing return restores it.

During delivery the land subject is `BattleMovement.From`, the last fully delivered tile of the
current segment. After matching completion it becomes the next segment's From or the settled
preview destination. This applies to each undo segment too: the preview already requests the origin,
and committed actor coordinates still describe the turn origin, so neither alone gives the shown
tile. `BattleTerrainRules.LandEffect` supplies the 0/15/30 setting by that actor's mover and tile's
protection. The localized line names its coordinates within the actor's card; it is not a physical
damage-reduction percentage or an empty tile's actor-independent attribute. Only acting cards add
the 26px land row (ordinary starting frame 500×218); other cards retain the 500×192 base and all
gauges retain their existing live values, image crops and zero-MP track behavior.

**Confirmed static:** the [pinned S1/S3/S4/S6 sources](battle-interaction-baseline.md#evidence-and-reproduction-boundary)
at `c834c652b6862bc5679fd7f69a38a7093206efc6` open mini status/land effect before player movement,
read live ally class/level, and build the land setting from the moving combatant's mover/coordinates
multiplied by 15. S3 `ControlBattleEntity` calls `UpdateBattleEntityPosition`; that routine temporarily
substitutes the displayed entity tile, rebuilds through `HideLandEffectWindow`, then restores
combatant coordinates. This supports displayed-position ownership; it does not prove the remake's
delivery cadence. Updating at completed segment boundaries is the explicit conservative remake policy.
Source window positioning, hide-toggle timing, sub-tile threshold and natural hardware cadence remain
**Unknown** at this acceptance boundary.

**Confirmed engine/native:** `BattleActingStatusQueryTests` exercises Regular/Flying/Hovering with
Light/Heavy protection, requested-versus-delivered preview/undo tiles, query purity, live progress,
private 99/100/101 disclosure, unavailable names/classes and excluded boundaries. Actual command-driven
Equipment, all Give/Discard selection stages and ordinary target/commit
stages retain actor/resources/header; actual transaction presentation rejects the card. The existing
inspection behavior tests pass with the shared projection. The bounded
[`engine_battle_acting_status_observation.gd`](../../game/probes/engine_battle_acting_status_observation.gd)
uses actual ordinary host input and reads actual card nodes/draws: public `practice-yard` variants
use Regular and Hovering across Light → Heavy → None terrain, multi-segment undo, same-actor full
inspection/return, explicit Stay confirmation and next-actor handoff. The Hovering case uses Chinese
and remapped/swapped controls; actual hints consume those bindings. A focused authored `battle-give`
case also opens actual Equipment/Give inventory shells and the visible Item submenu, verifies retained
acting projection and Back, and preserves resources/RNG without committing a transaction. A connected
private-local
Battle01 controlled-start case separately observes Chester's KNTE/live level, shown tile/undo,
inspection return and Bowie handoff in Chinese. These are bounded consumer checks, not full-campaign,
physical-device/focus, original-runtime or human visual acceptance.

Reproduce with the existing environment, engine/adapter commands and ordinary authored-package entry
using this probe; `SF2_ACTING_ZH=1` checks Chinese, and `SF2_VIEWPORT_OUTPUT` selects the ignored report.
The connected case uses the existing pre-voyage controlled-start observation admission and
`SF2_ACTING_PRIVATE=1` / `SF2_ACTING_START`, without player saves or new asset extraction. Exact local
variant/launch inputs, completed failures and process identities are retained in the Issue handoff.
The first full engine run completed with four new-test setup failures; correcting those fixtures
passed the affected acting/inspection tests, without repeating the aggregate. An initial native
run exposed an unregistered locale message, corrected in the existing GameText catalog. Private
observation initially selected an occupied allied tile; Confirm correctly refused it, and the
corrected route uses an unoccupied neighbor. Successful private exit retains the known four-instance
ObjectDB warning; this slice neither audits audio lifetime nor claims that warning resolved.

## Localized story controls and return notices

The [M2f consumer agreement](../application/localization-design.md#implemented-m2f-story-controls-and-return-notices-boundary)
extends the existing GameText/PO/font through ExplorationDialogueView hints, BattleStoryView hints,
StoryChoiceRow labels and FieldReturnFeedback notices. Body, hint and attached choices remain
disjoint at 1080p, including the optional text diagnostics described in the
[player-facing copy review](ui-scene-review.md#player-facing-copy).
Both old triangle markers lack bundled coverage; `>` is used for selected Yes/No and Continue.
Stable Yes/No nodes, booleans and source-owned selection retain input authority. Reveal, ReadMore,
complete pointer release and fresh confirmation still precede choice delivery.

The exploration body/window/range and retained FieldTextWait projection are unchanged. Battle-story
Definition.Texts remains English, with a `unmigrated-consumer` notice only when Debug diagnostics is on
and a DialogueWait body is shown in Chinese. Camera/tick delivery and close remain unchanged. Typed actual
Victory/Defeat/retreat events plus CanWaitAtInput select complete pending/ready notices; Begin/Dismiss
and first-input dismissal retain their existing lifetime, without a new outcome or reward authority.
The [native recipe](../verification/host.md#localized-story-controls-choices-and-return-notices)
records bounded bilingual observations. Remaining dialogue/names and
opening-wide Chinese coverage are not admitted here. Language stays in the existing settings entry.

## Localized semantic battle feedback

The [M2g agreement](../application/localization-design.md#implemented-m2g-semantic-battle-feedback-boundary)
keeps English BattleScene source-ID selection and uses authored typed-state templates for Chinese
physical/item/prism actions, reactions, side-specific death, receipt/drop, EXP/gold/growth and existing
sleep/poison notices. AURA shares accepted HEAL keys. Spell-growth level is distinct from a stat gain;
SleepTurnText retains its existing finite discriminator. Names and item/spell labels stay literal;
a single source-template token scan prevents inserted values from becoming controls.

Acting/Target, reveal/ReadMore/Continue, in-progress and timed-Wait hints use complete catalog rows.
Only the current body/hint's fallback adds a visible key/reason notice; missing Chinese rows use a
whole authored English template, while unmigrated English bodies are explicitly identified. Blank
phases and unrelated missing rows produce no false notice. Catalog diagnostics remain available.
The existing panel measures the full hint/footer before allocating its clipped reading area.

Canonical HEAL text/work, animation/audio, focus suspension, 0.5-second automatic status delivery,
reveal/ReadMore-before-ack, same-token continuity and result/resource ordering are unchanged. The
[native recipe](../verification/host.md#localized-semantic-battle-feedback) separates headless layout/
input evidence from physical-rendering and human reading acceptance; wider automatic-notice reading
comfort remains Unknown. There is no new parser, overlay interpreter or engine/save behavior.

## Shared story dialogue agreement

**Standard rigor; [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).**
This bounded multi-consumer presentation change uses the accepted 1920×1080/96px world and
existing session text, portrait, choice, wait, input and audio authorities. Ordinary field talk,
source story text and existing battle-story dialogue share a blue/dark-blue panel, thin pale-gold
border, 30px body and binding-derived hints. Initial dialogue geometry is 1440×280 with a 32px
safe edge. UI overlays never resize or zoom the world. Extract field UI rather than growing its
controller; native Godot Theme values serve these actual consumers, with no general UI framework.

Acceptance covers private portrait/real Zone8 text, no-portrait authored text, reveal/all/instant,
actual input pauses, both choice answers and release gates, automatic/failure feedback, stable
subject-aware placement, close and fresh movement, and existing battle-story camera/waits. Measure
native text bounds; expand the panel for reached content and use display-only reading pages for
screen-sized overflow before acknowledging any script input. A window chooses top/bottom once
for its lifetime. Portrait side/mirror/eyes/mouth/resources stay with the existing presenter.
Names require admitted speaker-name metadata; raw entity/sprite identities are not names.

No gameplay, source text/clock/input/audio policy, general skip, mouse, settings/history/autoplay,
new font/art/export, whole-game migration or original hardware fidelity is included. Release adapter,
necessary Debug/native load, direct source/doc/private checks and committed-head scoped CI are the
gates; no new engine or observer tests and no research/full/H4 suite. Reuse the owned installation,
project and prepared private content. Pilot one private portrait seam and one authored choice/return;
then several finite semantic cases (minutes, small-MB output) with logs checked for runtime errors.
Repeated observer repairs without useful evidence or a new cross-system authority require replanning.
Retain prior focus-loss Unavailable, physical-release Unknown, missed-skip timing Unknown and exit
warnings. Independent main-gate review owns integration; this agreement is the approved modern
presentation direction and does not assert final aesthetic or all-map acceptance.

### Source line reflow (#1583)

The [full-corpus review](../evidence/dialogue-corpus-layout.md) covers all4,267 pinned English/Chinese
message pairs,337 separate names and six entry aliases. It distinguishes semantic review, production
compatibility and actual Godot layout from runtime admission. Of4,010 compatible messages, controlled
projections pass measured glyph/bounds checks;257 unsupported rows do not count as rendering passes.
The107 semantic-preservation candidates and three ambiguities have explicit dispositions. The21
currently admitted candidates use Search's existing preservation policy; no new reflow defect was
found. The frozen209-row Chinese world subset and six aliases remain unchanged. Full-game/pre-voyage
Chinese admission and human reading acceptance are not established by this audit.

**Standard rigor:** a bounded display-only change reuses the current Content resolver and shared
panel. The source glyph/window service, source ranges, RNG, script waits and the separate High
[Confirm agreement](#ordinary-dialogue-confirm) are unchanged. No theme, geometry, font-size,
save format, input schema or content rewrite is included.

Selected exploration `DialogueWait`, `FieldTextWait` and English `W1TextWait` projections opt into
modern wrapping. A single typed `{N}` between literal/name/class text contributes a word separator
in English, or no separator for Chinese prose. Mixed Latin boundaries use the **resolved display
name/class**, including a custom name, to retain a word space. Existing whitespace is not doubled.
Consecutive `{N}`, row/control-adjacent breaks and literal newlines (including paragraphs) remain
explicit. No punctuation heuristic identifies paragraphs. W1/Delay/CLEAR still belong to the
original program; display ranges and append-prefix/reading ownership are preserved. An attached
choice retains the already resolved question and locale instead of reconstructing raw source text.
W1 keeps its
existing English/unmigrated boundary, not a newly claimed Chinese consumer.

`DisplayCatalog` defaults to its original projection. Search opts into the
[finite source-token layout policy](../evidence/dialogue-corpus-layout.md#current-search-consumer-layout-1608),
retaining reviewed semantic breaks and complete-row/result separation; unreviewed rows retain their
original policy. Battle and compatibility-only raw-string consumers keep their current policies.
Entry is a separate
narrow compatibility boundary: the pinned `private-local-entry` v1 generator maps source `{N}` to
LF in IDs216–221. `EntryPresentationAssets` reflows those isolated LF breaks for the selected
English/Chinese row after resolution, preserving blank lines. It never normalizes arbitrary UI
strings, mutates the source/overlay or changes `ResolveEntry`'s default output. Locale Apply resolves
again in the same entry; existing Reveal/More/Continue and release guards remain.

**Confirmed:** the generator `src/sf2tool/remake_entry_content.py` projects `{N}` to LF. In the pinned
source represented by admitted entry row217 and ordinary opening row480, a break splits an unfinished
clause. This supports identifying those breaks as original layout; it does **not** prove that every
single `{N}` in the complete catalog is nonsemantic. Reflow for the selected consumers is an explicit
modern policy. Private wording and the pinned inputs remain read-only. The full review's semantic
judgments are **Inferred**; original intent for the three ambiguous rows and human approval of the
resulting reading experience (#1456) remain **Unknown**.

The [finite observation owner](../verification/field-programs.md#source-line-reflow-observation)
records native widths/line counts, bilingual paragraphs, names, portrait reservations, long pages,
append/control/release boundaries and language Apply. The panel's30px body,24px hint, entry safe
region and legal viewport rules are retained; native wrapping determines final visual lines.

### Ordinary dialogue Confirm

**High; bounded host clock/read-intent change.** One mapped Confirm on ordinary exploration
`DialogueWait`, `W1TextWait` or `FieldTextWait` requests the next segment or end, including while
characters are still revealing. Portrait animation is not an additional admission condition.
This supersedes reveal-first for these consumers only. Entry, battle presentation, Search results,
transaction results and choice questions retain their separate reading/confirmation contracts.
No layout/theme, binding, engine command, save format, content admission or source rule changes.

A single-screen segment may be dismissed directly. A segment overflowing the screen first reveals
its current page; further fresh presses move through every remaining page. The press exposing the
last page never also acknowledges it. Choice questions retain reveal/read coverage and a distinct
fresh confirmation. Native key echo, pad edge guards and choice release prevent a held press from
crossing segments or selecting an answer. Cancel does not dismiss ordinary text.

The controller's Reading partial owns at most one current token/display-span intent. W1/field
text keeps the full projection mounted for a process boundary, then submits its real delivery and
eligible acknowledgement separately. Field text executes the existing neutral mandatory services
in batches of at most600 opportunities per host frame, stopping at the same span's input/end.
This is an explicit modern latency choice: logical entity/window/portrait work, RNG and accepting
poll still execute in their original order. It does not synthesize completion or shorten a service.
An explicit `{D1}` pause retains ordinary elapsed-time service. CLEAR replaces the projection and
cancels old intent even when the token is unchanged. No intent crosses another token, choice,
map/battle ownership, focus loss, hiding, modal suspension or failure. Subsequent close, movement,
audio and script waits retain their existing authorities and elapsed/delivery boundaries.

**Confirmed (bounded host witness):** an authored no-portrait ordinary dialogue on the previous
policy consumed its first Confirm solely revealing55 characters, with no simulation tick; a second
press reached its choice. That establishes one reveal-first cause, not the user's exact scene.
**Unknown:** the user's precise original wait and human acceptance of the revised experience.
Existing source text/window services remain evidence for the original; this host latency policy
is a modern choice, not a claim about original natural button timing. The
[field observation owner](../verification/field-programs.md#ordinary-dialogue-confirm-observation)
owns reproduction and the finite acceptance matrix.

Acceptance: actual mapped input through no/with portrait, revealing/all-visible, nonfinal/final,
long reading pages, W1, explicit pause/CLEAR, choice and held-input boundaries; adapter build,
committed dependency plan and actual CI. No full route, emulator capture, screenshots or new
comparison system. Serial native processes<=45seconds each and new reports<=10MiB; existing
installations/materials reused. Release is independent Draft-PR review. Recovery is reverting this
host policy; original evidence, completed failures and protected sessions are retained. No new
engine behavior tests are required for unchanged Application commands; observers run directly.

### Coordinated dialogue exit (#1613)

**Standard rigor:** a bounded adapter projection correction, with independent logical text and
portrait services. The visible portrait remains with its existing open text window through the
portrait-close/text-close handoff. After logical portrait closure, the view clears that mounted
texture when the text window ends; a new window without a portrait cannot inherit it. A newly
opened portrait still comes from the existing presenter's resource and facial-state selection.
CLEAR, reading pages and attached choices do not themselves close the text window.

The scope is `ExplorationDialogueView` and this agreement; the shared `StoryDialoguePanel`,
`PresentationCanvas`, source scripts, Application commands, acknowledgement count and logical
window waits retain their owners. No theme, global layer, text service or engine rule is added.
Acceptance uses short real Godot state/input/control observations of ordinary close, portraitless
text, CLEAR/pages, consecutive speakers and the actual Kiwi choice branches, plus the affected
adapter build, clean committed planner and actual CI. Shared battle consumers need only their
applicable no-portrait dialogue boundary. Human visual acceptance and original hardware timing
remain **Unknown**; no screenshot, full-route or emulator acquisition is required.

**Confirmed (bounded native observation):** ordinary Map3 text480/481 reaches `TextCloseWait`
with a closed logical portrait and an open text window. Both native controls now remain visible
through that wait and disappear together when the panel lifetime ends. The registered current
private-world observations also cover Jaha662–664, Zone8 through525, Kiwi Yes/No and subsequent
portraitless text. Before/after semantic checkpoints, confirmation/choice counts and logical waits
agree; new portraitless windows have a null mounted texture. Existing authored `field-clear`,
`long`, `field-portrait` and Jaro `battle` story observations pass without changed helpers.

For the ordinary seam, load the registered private inputs and Debug adapter, start ordinary Main,
then use the existing `StartPreVoyageLocalizationObservation` bootstrap with a checkout-local
controlled-start JSON: Map3, entity0 at(42,9), facing1, speed32, flags[0,32,256]. This controls initial
placement, not preceding natural reach. Send ordinary Confirm to Talk; finish each actual text
input through mapped Confirm. Read `ExplorationSessionView.ReadObservationJson` at the text/window
transitions: `wait`, `textWindow`, `portraitWindow`, `dialoguePanel.visible`, `portraitVisible` and
`portraitTexture`. Require the retained texture at the described close seam and null texture/hidden
controls after field return. Reuse the [ordinary Confirm recipe](../verification/field-programs.md#ordinary-dialogue-confirm-observation)
for CLEAR/pages/portrait; use the [shared story recipe](../verification/host.md#shared-story-dialogue-observation)
with `jaro-conversion.json` for the unchanged battle consumer. Retain native exit logs separately:
the observed ObjectDB exit warnings are not resolved by this projection correction.

### Current story presentation

`ExplorationDialogueView` owns field text-to-label projection. The existing field controller still
owns incremental speech, reveal delivery and all commands; `BattleStoryView` retains its camera,
tick and acknowledgement boundary. `StoryDialoguePanel`, `StoryChoiceRow` and `ContextHint` own
only native controls/layout/reading position. `StoryTheme` supplies the native Theme also used by
the existing battle menu frames. There is no gameplay/wait/clock service in these widgets.

At 1080p the panel begins at 1440×280 (304px high with a portrait), uses 30px body/24px hints, and measures full shaped text
before incremental reveal. It expands within the 32px safe area; screen-sized overflow uses
Confirm-driven reading pages with a line of overlap. Ordinary exploration dialogue now follows the
[explicit reading agreement](#ordinary-dialogue-confirm). Other consumers retain reveal-first:
reveal-all omits incremental speech and a later fresh Confirm reads remaining pages before
acknowledgement. Field `CompleteTextReveal` remains distinct from script `Acknowledge`.
Ordinary Cancel remains unable to skip dialogue.
Reading resets on an explicitly new window/segment, including identical text; source append spans
and temporary window hiding retain it. Attached choice rows cannot submit Confirm past unread
content. Source Yes/default/Cancel=No and release phases remain session-owned; authored choices
retain direct `ChooseDialogue` answers without a fabricated source selection phase. The current
[menu pointer agreement](#menu-pointer-agreement) adds shared focus and Confirm-on-highlight behavior.

Placement is selected once when a live window first becomes visible. It uses the projected speaker,
retained camera subject or player and moves to the top only if the bottom covers that subject and
the top is clear. Temporary `HideWindows` or parent hiding does not end the window. Opening/closing,
text changes and reveal never move/resize the world. An expanding long panel may occupy more of
the screen; it never chases the actor on every tick. Body, portrait, choices and measured hint rows
have separate reserved space. The portrait TextureRect is a child **after** the background, using
the existing presenter's cached texture, mirror/eye/mouth selector and source Work.Y movement
(mapped into its reserved column). The presenter retains resource lifetime and source lifecycle;
a second portrait service is not introduced.

The portrait's visible source area is 48×64, mounted at 192×256 with nearest sampling. The accepted
normal/mirrored 8×10 source window layouts contain a border around six columns and eight rows;
the two remaining atlas columns supply alternate animation tiles. Composition applies eye/mouth
replacements before cropping and mirroring. The last eight source rows contain intended visible
pixels for Sarah (1), Chester (2) and Astral (40); their earlier 48×56 crop discarded those pixels.
Their admitted atlases and animation entries match direct ROM decoding, so preparation stays
unchanged. The portrait-bearing panel minimum preserves 24px top/bottom margins at rest; source
movement remains inside that column. The [focused pixel recipe](../verification/host.md#complete-portrait-raster-observation)
checks the actual mounted textures and native bounds, including both sides and facial states.

There is no reliable admitted entity-to-speaker-name mapping in the current field definition.
Member-name substitutions in dialogue are not speaker attribution. Names are omitted deliberately;
no raw identity or sprite heuristic is displayed. Ordinary field chrome hides package/debug names
and shows only legal Talk and Hold Wait bindings. An automatic scene says “Scene in progress…”;
Continue appears only at a real eligible input. Only the existing eligible entry service offers
Skip battle intro. A failure replaces that chrome with “The scene has stopped” and the retained
message/code above world fades, with no invented retry or timeout. Diagnostic observations retain
package, tokens, raw identities and failure detail.

The [finite host recipe](../verification/host.md#shared-story-dialogue-observation) owns reproduction,
measured native results and limits. This is bounded modern presentation, not original timing,
all-map coverage or final aesthetic approval.

## Authored battle discard presentation

R / right stick down starts Drop from the ordinary battle ActionChoice; directions cycle exact
slot labels, including duplicates. Protected items show a readable refusal reason. Enter selects
and confirms; Escape keeps the item and returns to actions. The confirmation explicitly states
that the action and movement preview remain available. Bind `discard` through ordinary input
settings to change its keyboard/gamepad control.

`BattleDiscardView` displays authored result text and reports CompleteTextReveal only after actual
delivery; Confirm then acknowledges it. Parent focus/hide suspension blocks input and delivery,
preserving the accepted loadout/Deals prefix. It resumes without timing debt. The view chooses no
inventory/turn policy and acknowledgement returns to the same actor/action; later outer Cancel
returns movement. No new media or original dialogue/hardware timing is claimed. The
[native recipe](../development-and-verification.md#battle-discard-authored-observation) exercises
ordinary keys and reads live state, labels and errors without screenshots or gameplay injection.

## Authored battle Give presentation

In the ordinary battle action menu, G / left-stick press starts Give. Directions cycle the held
slots; Confirm selects the item. Directions or Target cycle eligible adjacent recipients; Confirm
selects the recipient. A full pack displays its return slots. A final confirmation states that the
transfer ends the action; Cancel backs up one stage. Slot numbers distinguish duplicate identities.
The query/selection state remains authoritative; the adapter does not calculate transfer legality.

`BattleGiveView` displays project-authored result text and completes the existing reveal token only
after delivery. Confirm then acknowledges it. The parent battle view suspends Give input and
reveal while unfocused/hidden; resumption advances only the pending stage. The existing instant or
adjustable text setting applies. This result text is a modern authored addition, not an original
Give scene, sound or timing claim. No new media assets are introduced.

The [compact native witness](../development-and-verification.md#battle-give-authored-observation)
uses ordinary host keys, visible labels and read-only session facts. It covers both custody routes,
selection cancellation, suspension and later usable control; it emits no screenshots.

## Authored Caravan projection

The ordinary exploration host draws the Caravan package with public geometric markers. Vehicle
sprite 62 uses a brown rectangle; player/followers use circles and facing marks. Blue tiles admit
vehicle travel; the green marker is a later ordinary walking zone. The presenter reads live raw
coordinates, appearance and per-refresh RenderSize, so off-map actors are clipped rather than removed
from gameplay. Ordinary Confirm finds adjacent targets using raw motion coordinates, so parked
physical actors never require an invalid layout position. `authoredDraws` readback carries the actual draw frame, position, size, visibility,
sprite and sprite-request identity. Sprite-ready completion follows a matching real draw, including
an intentionally clipped off-map marker. No arbitrary acknowledgement skips the typed program.

The same session supplies travel mode, current physical list, motion and waits. The host owns no
vehicle position/controller authority. Hidden/unfocused views suspend automatic ticks and sprite
completions; resume discards timing debt. Existing input envelopes and request IDs reject duplicate
or stale completion. The authored package has no audio assets: source warp-sound observations are
retained, but no original audible SFX parity is claimed. The finite witness reads actual state/input/
draw/errors without screenshots, session injection or synthetic presentation completion.

## Authored battle-region scene

`BattleStoryView` consumes shared StoryState dialogue/camera/tick waits while the mode remains Battle.
The authored scene shows project-written paraphrased text, focuses the existing map transform on the
retained visual slot, and accepts ordinary Confirm. A mounted presentation must survive a process
opportunity before completion or acknowledgement; hidden/suspended views do not deliver. CameraWait
completes after applying the actual map transform. This is modern semantic delivery, not original
camera speed, portrait, audio or frame timing. No menu/layout/input redesign is part of this slice.

`BattleMapViewport` maps logical actors to retained physical appearances. Conversion reuses the
existing marker/sprite node rather than allocating an actor or visual at the conversion boundary.
Initial placement resolves live coordinates once; acknowledged movement alone updates that physical
slot's position/facing. Logical conversion writes do not move the old ally slot before alias copy,
and departed subjects retain their last physical position. Normal movement animates that binding; dead-ally camera/text can retain the physical scene subject
without reviving gameplay HP. Effective control/activation, aliases, story cursor/callers/wait,
actual text visibility, camera slot/focus, marker direction indicator and geometry and failures are exposed by the existing
semantic observation API. Unsupported source physical services/private Battle32 media remain explicit.
See the [ordinary-key witness](../development-and-verification.md#jaro-conversion-authored-observation).

## Authored enemy appearance

`BattleAppearanceView` consumes the pending round's actor/list/token through the ordinary battle
view. The map focuses the current actor, hides later pending list members, and delivers an authored
0.6-second spin/fade,0.2-second settle and0.7-second paraphrased appearance notice. Reduced-flash mode
omits rotation. Focus loss suspends delivery. Only the actual view's completion submits the matching
`EntityEffect` token; the view never resets actors or generates a queue. Ordinary commands are blocked
during this finite wait and resume after appearances and automatic work.

The semantic observation includes current stage/token and actual marker rotation/alpha/visibility,
plus future-member hiding and final control. The [native recipe](../development-and-verification.md#hidden-spawn-authored-observation)
uses ordinary keys and verifies intermediate/final node values. This is a project-authored visual
with no original sprites, SFX, frame timing or natural-source reach claim. It adds no menu/layout or
input redesign to the separately owned UI work.

Godot is the outer composition, semantic input and presentation adapter. Its nodes/textures/audio
are projections; Application owns gameplay state, program/scene continuation and completion tokens.
See [audio](audio.md), [battle scenes](battle-scenes.md), [Content assets](../content/presentation-assets.md)
and [host observations](../verification/host.md).

## Bilingual pilot presentation

The [M1 localization owner](../application/localization-design.md#implemented-m1-boundary) supplies
English/zh-CN settings, original authored FieldTextWait and HEAL templates. The
[M2a boundary](../application/localization-design.md#implemented-m2a-churchsearch-boundary) adds Church
menus/offers/results/hints and Search source-ID/typed feedback through the same resolver. Language is a draft row in
the existing safe Settings access; Apply prepares the native PO and licensed font before storing,
then refreshes settings access without remounting or issuing gameplay. Cancel retains the old
language. Chinese missing/invalid rows retain exact fallback metadata and complete English wording.
Settings recovery details are currently English with a localized explanation.

The [M2b entry/save boundary](../application/localization-design.md#implemented-m2b-entry-and-save-boundary)
adds title/Witch controls, New Game/name validation, slots, Save/Quit/Continue and recovery.
Actual entry IDs 216–221 use their admitted profile/source through optional
`SF2_ENTRY_DISPLAY_CATALOG`; no private translation ships with the UI. Entry source fallback and
UI-key fallback remain independently visible/diagnosable. Long menu/error text consumes ReadMore
before a command; Settings/Cancel stay accessible. Apply refreshes labels/errors and resets visual
reading only when their body changes, retaining page/selection/failure/audio and paused progress.
Slot destinations format the existing Recipe, custom names stay literal, and UTC stays explicit.
Image lettering and game terminology remain source English pending separate work.

StoryTheme uses the bundled Noto Sans CJK SC at the existing 30px body/24px hint hierarchy, with system
fallback disabled. Plain Label rendering retains literal names; visual wrap/ReadMore never adds source
controls or acknowledgements. Field reveal continuity uses window/text/source range; HEAL consumes
captured semantic operands while its original work strings stay unchanged. Other menus/messages
remain explicitly English. [Native recipe](../verification/host.md#localization-pilot) includes
geometry/glyph/input observations; human typography acceptance remains separate.

## Ordinary Godot Host

[`Main.tscn`](../../game/Main.tscn) instantiates [`GameRoot`](../../game/src/GameRoot.cs).
The project references only Application, Content and Domain. Startup accepts one of
`--authored-package <path>`, `--private-battle-start <path>` or `--private-exploration-start <path>`,
plus optional `--input-settings <path>`. When no session-entry option is supplied, startup selects the authored practice yard,
including when only input settings are supplied. Unknown/positional, duplicate, conflicting and
missing-path options fail explicitly before creating a session. Diagnostic observer settings belong
to external probes, not game argument routing.

`SessionPresentationOwner` chooses the presenter from the current mode and retained entry
continuation. Field owns exploration and the unfinished before/load/start programs, including load
and start waits that already carry `ActiveBattle`. Once entry finishes, battle owns every battle
snapshot: initial round programs, appearances, ordinary/automatic control, terminal outcomes and
failures. This decision is shared by initial attachment, field handoff and field input dispatch;
`GameSession` supplies all lifecycle state.

The root installs entry and return callbacks before choosing the initial presenter. An admitted
connected package may start directly in battle, win through ordinary input and return to one usable
field view in the same session. Pending round delivery still uses the existing consumers and matching
tokens; routing does not complete waits or generate queues. The
[bounded native recipe](../development-and-verification.md#ordinary-host-lifecycle-observation) owns
startup, pre-battle and return controls. The accepted cardinal battle menu uses this same lifecycle
boundary for attachment, ordinary input and field return.

Both source-start paths use `RuleCompositions.ForGame()` and the same `GameSession`. The root routes
each input event once to the active view. Battle view pumps `AdvanceSimulation` only at the
Application's simulation wait; it does not choose rounds, enemy policy or control. Field/scene views
submit actual matching delivery/completion, retaining pending state when an adapter fails.

External scripts under `game/probes` drive ordinary input and read state. `ObservationCapture` is
diagnostic support: callbacks copy primitive/immutable facts to the ordered writer; its worker does
not read live nodes or session state. Writer failure retains an explicitly incomplete prefix.
The [retained capture contract](../evidence/retained-comparisons.md#bounded-h4-capture) is historical
protocol evidence, not authorization to resume bulk capture.

The export preset excludes `probes/*`. No current gate establishes complete ordinary export/package
contents; project configuration or a successful C# build cannot make that claim.

## Semantic Interface

This is the current Application/adapter contract for future art/UI work, with no UX or layout choice:

| Surface | Consumer obligation |
| --- | --- |
| `QueryBattleChoices()` | Consume the immutable `BattleChoicesSnapshot` session/revision/actor/stage/origin and ordered `Actions`. Each action supplies its typed reference, label, presenter kind, range, Empty, Enabled, Reason and ordered target references with their own Enabled/Reason. `Spells` and `Items` derive from those same offers. |
| Inventory | Present the returned slot/label/Empty/reason. Raw source words remain engine/content data; the view never masks item bits or infers usable items. Slot identity participates in the typed action and is rechecked at commit. |
| Selection and confirmation | Submit semantic `SelectBattleAction`/`SelectSpell`/`SelectItem`, `SelectTarget`, Move/Confirm/Cancel through the current `CommandEnvelope`. The engine owns live target legality and preparation. Requery after state changes; a disabled candidate or missing consumer retains its diagnostic. |
| State and effect projection | Render the current single-session snapshot and ordered scene/program requests. Selecting an existing presenter by `BattlePresentationKind` or cue kind is allowed; selecting rules by package/module/strategy name, calculating target factions/ranges or replaying observation strings as effects is not. |
| Completion/input | Submit actual matching token/kind delivery, acknowledgement, text reveal and eligible Wait separately. The engine owns PC/stack/timers, resource publication, queue and control release. Duplicate/stale completion rejects; displaying text does not acknowledge it or finish a timer. |
| Diagnostics | Display the typed failure and relevant context; preserve the reached pending scene/program state. Rule, Content and adapter failures have the separate ownership described above. |

The actual [`BattleSessionView`](../../game/src/Battles/BattleSessionView.cs) already consumes semantic
offers/targets/inventory through this interface. Domain has no outward Application/Content/Godot
dependency; Content admits data and its typed source opcode, while factories alone select concrete
implementations. The generic scheduler/runner and Godot have no new policy-name switches.
There is one current snapshot authority and one selected calculator per migrated seam.
S1 is resolved for implemented formulas; S2 for semantic actions/targets/inventory; S3 retains explicit
legality-versus-consumer support; S4 has the guarded source-story policy; S5 separates rule binding
from state/control/faction/order for the finite admitted set. Fixed refresh/activation/control,
movement-cost, field text/music/motion and battle-scene source-service algorithms remain the
explicit exclusions in the independently accepted bounded scope. #438 remains open for UI/UX
discussion with the user; Git integration and Epic closure remain main-gate responsibilities.

### Initial battle command focus (#1644)

**Standard rigor:** on fresh entry to Main from legal Movement confirmation, select Attack only
when the current `BattleChoicesSnapshot` has no query failure and offers an enabled Physical action
with an enabled target; otherwise select Stay. `BattleCommandMenu.Present` consumes the existing live
query at that confirmed stop. Godot does not infer factions, distance, weapon range or target legality.
The rule also applies to a newly mounted actor/session entering ActionChoice. Ordinary refreshes and
child Back retain the player's chosen Main focus; Main Cancel restores Movement, whose next confirmed
stop gets a fresh decision. Focus submits no command and never bypasses target/action confirmation.
Search substitution remains outside this rule, under #1649.

**Confirmed static source:** trace [S2](battle-interaction-baseline.md#evidence-and-reproduction-boundary),
pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput` accepts the unoccupied chosen stop and
writes actor X/Y before `@StartBattleMenu`. `@CheckTargetReachableByAttack` rebuilds the attack range
and tests `TARGETS_LIST_LENGTH`, supplying diamond index 0 when nonempty, 3 when empty. The linked
[`BuildAttackRangeGrid`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildactionrangegrids.asm)
clears prior targets, builds opposite-affiliation occupancy, obtains current min/max attack range and
applies each radius. Its `ApplyRelativeCoordinatesListToGrid` admits living occupants; the
[`BuildTargetsArray`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildtargetsarray.asm)
helper excludes zero HP, neutral activation and out-of-map placement. Reproduce from the registered
read-only pinned checkout with `git show <pin>:disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm`
and the two named battlefield files. This proves source structure, not natural original reach or timing.
The remake additionally requires its existing supported/enabled offer, rather than promising an
unavailable action from geometry alone.

**Confirmed bounded engineering observation:** the
[actual-host focus probe](../../game/probes/engine_battle_initial_focus_observation.gd) reads live
query/revision, menu selection/heading and diamond projection after mapped input. Authored range 1,
range 2 and range 1–2 variants cover both boundaries, a nearby defeated enemy with a living off-range
enemy, provisional movement, cancellation and origin re-entry. Item/Stay/Attack child Back retains
manual focus, held Confirm cannot advance from the item list, and idle refresh retains focus. Custom
directions with swapped Confirm/Cancel also pass. The existing [Map57 controlled approach](../verification/battle-save.md#method-and-reproduction)
admits Battle01 (`battle-1` in the selected world) and covers no-target origin, a legal left preview and re-entry, with child returns. All observed browsing
retains committed positions, HP/MP, inventories, queue cursor and both RNG seeds. Connected near-target
variants, live target death during an open menu, physical OS input and human/aesthetic acceptance are
**Unknown**; authored controlled starts do not establish those boundaries.

After the locked environment setup and adapter Debug build, reproduce the tracked authored example
with `SF2_FOCUS_EXPECTED=0,0,3`, `SF2_FOCUS_STEPS=down,down+down`, and a fresh ignored
`SF2_VIEWPORT_OUTPUT`, then run the selected Godot with `--headless --max-fps 60 --resolution 1920x1080
--path remake/game --script res://probes/engine_battle_initial_focus_observation.gd -- --authored-package
<absolute repository path>/remake/content/authored/battle-equipment.json`. Use an explicit run-local
log and admitted settings, as in the [native input recipe](#battle-directional-hold-and-mode-isolation-1615).
Comma-separated paths restart at the committed origin; `+` separates steps in one preview. Expectations
are Attack=0, Stay=3; the probe separately checks the live offer at every entry. Range/dead-target
variants change only a small ignored authored input; private inputs and observations stay local.

## Presentation Policy and Current Layout

The common design baseline is 1920×1080 (16:9). One map cell and ordinary 24×24 sprite
frame occupy 96×96 logical pixels with nearest sampling. At 1080p the viewport covers 480×270
source pixels. Larger native clients reveal more world. At 1600×900 the shared canvas scales to
five-sixths, including world cells, panels, text and pointers. UI overlays keep the same map extent.
See the [window policy](#window-support-and-recovery-1455). The shared visual/story treatment above supplies the current finite Theme consumers.

### Unified world viewport agreement

Standard rigor: this changes Godot presentation only. Application retains entity coordinates,
logical camera axes, collision, scripts, clocks and completion tokens. `WorldViewportLayout` owns
only disposable source-pixel camera geometry, shared by field and battle through `GameRoot`.
It does not store gameplay state. Both authored and private consumers use the same cell scale.

Field coordinates are sixteenth-source-pixel motion values divided by16. A battle cell is mapped
to the source map by adding `SourceMapRegion.Origin`, then multiplying by24. Textured exploration
actors use the field sprite anchor below. Battle terrain uses the inverse region translation.
A same-area, same-map handoff retains the presentation origin and pans toward the new subject; a different
map initializes its own frame under the existing transition. Disjoint packed rooms on the same
map also initialize within that transition; the camera never pans across unopened storage. Bounds may clamp a handoff when its
legal region changes; continuity never authorizes displaying packed neighboring rooms.

Ordinary exploration follows the player through a small dead zone with bounded linear panning.
An explicit followed story entity has priority. A detached logical camera maps its original
240×216 logical frame center (origin +120,+108) into the modern frame; logical scroll progress
remains authoritative. Plane origins are reconstructed from the presentation origin using each
plane's admitted parallax and offset, preserving the two-plane relationship. Display displacement
and quake do not write back to logical axes. CameraWait still requires actual settled delivery.

The current logical view area bounds the displayed world (traversal area for unbound authored
content), independently of packed 64×64 storage. Explicit scene origins select their own area,
independently of the retained player; non-player story follow targets bypass the player dead zone.
Its inclusive cell bounds become a half-open pixel rectangle. Rendering clips every plane and
actor to that rectangle and the viewport. Small areas center on exterior base color, never zoom
or reveal another area. High tile priority, actor ink masks and draw order remain in their owners.

#### Castle barracks and field sprite anchor (#1609)

**Confirmed** in the [pinned US source](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Map19 area record 1 spans (0,0)–(40,31), with main plane B and plane A offset (0,32).
The default `ms_map19_Entities` at `0x52BC2` places entity130 at (17,25) and entity131
at (14,25). Both use guard206, Down (3) and `eas_Init`. Their Talk handlers select texts
547 and 548. The F501 setup has a different population. `InitializeNewEntity` allocates
physical slots after existing entities and initializes layer 0. `eas_Init` sets normal
orientation, size 24 and immersion off, refreshes the sprite and enters idle.
`ChangeEntityMapsprite` selects direction index 2, with two animation halves.

**Confirmed** for source draw geometry. `VInt_UpdateSprites` selects the area's main plane.
After subtracting the view in sixteenths and rounding to pixels, it writes sprite attribute
table coordinates with X +128 and Y +112. The hardware screen bias is 128 on each axis.
In the same scrolling coordinate system, `LoadMap` initializes the vertical counter at
view Y +8 and `UpdateVdpPlane` writes the corresponding map row at counter Y −8.
Thus sprite and cell coincide horizontally, and a settled 24px sprite starts 8 source pixels
above its cell. The field renderer applies that offset to its texture destination, visibility
and opaque repair mask. Movement, collision and sprite ordering use the motion position.
Camera follow uses the cell center. The Talk hint and command share `ExplorationInteractionQuery`.
The authored marker fallback and `BattleMapViewport` draw their own actors.

**Confirmed** from the verified retail ROM and running Godot 4.7.2 .NET. To reproduce the
source comparison, decode Map19 blocks and layout at `0xA4698` and `0xA5076` with
`decode_map_blocks` and `decode_map_layout`. Read both bed columns at rows 24–25. Decode
guard206 Down at `0xF900C` with Basic and tileset44 at `0x75F28` with Stack. Intersect
nonzero sprite ink with opaque high-priority tile pixels, applying tile flips. With the
quarters roof open, plane B block281 supplies the blanket. Its upper six tiles have priority
and its lower three do not. Plane A is empty at the open beds. With zero windows both guards
retain low display priority.

| Guard frame | Exposed with the former cell-top draw | Exposed with the source anchor |
| --- | --- | --- |
| First | 119 pixels, rows 16–23 | 83 pixels, rows 1–7 |
| Second | 118 pixels, rows 16–23 | 84 pixels, rows 0–7 |

Rows are zero-based within the sprite. The source and admitted tile/sprite alpha masks agree
at both beds. The actual Godot repair rectangles leave the head above the blanket.

For a short native reproduction, select the ordinary playtest pair and an isolated save
namespace. New Game mounts Map3 (56,3). Separately, start the joined Map3 church state
through `StartPreVoyageLocalizationObservation` (32,13, Up, flags 0/1/2/32/33/66/80/600/602/603/604/605),
save slot1 through the menu, and use Continue from a fresh title. Walk out of the church,
through Map3 (28,1) to Map19 (26,30), then to (16,29). Before entering, plane A roof blocks
354/360 and 357/363 repaint the two guards. Walk Up through (16,28) to (16,26).
Roof record 7 clears the 7×5 region at (12,55). Use `ReadObservationJson` and
`ObserveLayoutRegion` / `ReadLayoutRegion` around the beds (B rows 24–25, A rows 56–57).
The two followers put entities130/131 in slots 5/6 on this route. Direct controlled room
entry yielded slots 3/4. From (15,25), face left and confirm the Talk hint to receive text548.
The Map3 church projection places Bowie (32,13) and priest141 (32,11) at the same X,
48 source pixels apart vertically, with both tops 8 pixels above their cells.

**Unknown** for the historical report's exact party, flags and animation phase, and for
natural US hardware timing. Native reach was observed from the controlled save through
ordinary Continue and movement. The New Game check ended at its initial field mount.

#### Scene visibility coordinates (#1589)

**Standard rigor:** this bounded adapter correction keeps the existing area/display policy and
source camera commands. The camera movement limit and drawable screen rectangle have different
origins when a detached source view extends beyond the modern camera clamp. `ScreenAt` projects
the existing inclusive area with the **actual main-plane/actor origin**, including quake displacement;
all A/B passes, actor visibility, occlusion restores and exterior masks consume that same rectangle.
Layer0 selects B; admitted nonzero layer values254/255 select A. Independent plane offsets/parallax are retained; a secondary
plane is not clipped against untransformed main-layout coordinates. No content/schema boundary,
collision/trigger/entity coordinate, global scale, camera target or completion rule changes.

`WorldViewportLayout.Screen` and `worldGeometry.legalScreen` describe the **clamped camera geometry**.
`cameraProjection.legalScreen` describes the **actual draw clip** and can differ during explicit
story views. Consumers inspecting visibility must use the latter together with the emitted plane
origins. `Bounds` still describes the existing display area in main-world coordinates.

**Confirmed:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/common/maps/{mapload,camerafunctions}.asm` (`LoadMapArea`, `VInt_UpdateViewData`),
uses area start/end in followed-camera calculations. Map3 `2-areas.asm` separates X0–50/Y0–31
from X51–61/Y0–9 and X51–61/Y10–19. Its `mapsetups/scripts_1.asm:cs_5149A` explicitly sets
camera destinations (38,7), then (38,6). These are source structure/operand claims, not natural
original runtime or proof of a universal original drawable-boundary rule. Reusing the admitted
area as a widescreen display boundary is the existing **modern presentation policy** above.
The [finite source-scene and legal-transition observation](../verification/field-programs.md#scene-visible-area-observation)
owns the reproduced mismatch and acceptance; original raster/cadence and user visual approval
remain **Unknown**. No per-scene mask, new visibility dataset or wider campaign coverage is claimed.

#### Battle framing and overlays

Battle regions bound their own legal draw rectangle. Movement/target/story subjects pan into the
central safe area above the bottom UI; distant objects never cause a zoom change. When a legal
edge clamp prevents that placement, the overlapping corner UI panel moves vertically to avoid
the subject and the other currently visible panels. Menu visibility itself never changes the camera. A subject change
may move the camera, while menu/description visibility alone cannot do so.

At 1080p, body text is 30px, safe edges 32px and field dialogue reserves a 260px bottom overlay.
The diamond keeps its cardinal arrangement with crisp 96px icons. This is a first readable layout,
not a new component kit or an aesthetic acceptance claim. Acceptance is an adapter build and a
bounded native witness of private room/edge/occlusion, map/region handoff, battle movement/target
and UI geometry plus the existing input/reveal/entry-skip boundaries. No full/H4 replay or new
Domain tests are needed for this presentation-only change. The
[host recipe](../verification/host.md#unified-world-viewport-observation) records reproduction and limits.

The earlier 960-by-540 logical grid, 4x authored masters/2x and4x buckets, SVG chrome, product font
theme, UI-scale choices and safe-area model are retained presentation design/proposal context.
They are not an implemented ordinary-host layout or a new #438 UX decision. The legacy private
window/catalog consumers were retired. Keep simulation coordinates, presentation units, asset
pixels and physical output pixels distinct; none may change gameplay collision or rules.

Current configurable input, text speed/reveal, reduced flash and finite audio policies have their
specific owners, including the minimum-client/fullscreen recovery policy below. Broader art/theme,
sub-1080p reflow, configurable resolution and UI-scale work remains
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); its selected initial
experiment is described below, with remaining product choices still open.
The [original proposal and legacy consumers](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#decision-summary)
preserve the exact design and receipts without declaring them current functionality.

### Window support and recovery (#1455)

**Standard rigor.** The host admits gameplay at a physical client width of at least 1600 and height
of at least 900. Window borders and the desktop work area affect placement. The project canvas
is 1920×1080 logical pixels. Entry always fits it uniformly inside the client, with centered bars
where needed. Gameplay fits the same canvas when either client dimension falls below the design
size. When both dimensions meet the design size, gameplay uses native coordinates and reveals more
world as the window grows. Godot converts client pointer positions to logical coordinates before
GUI and gameplay hit testing. [Representative clients and input](ui-scene-review.md#desktop-clients)

| Boundary | Current behavior / acceptance limit |
| --- | --- |
| Supported clients | At 1600×900, each 96-unit cell occupies 80 physical pixels. At 1920×1080, 1920×1200 and 2560×1440 it occupies 96. Native 4K fullscreen uses the same native layout. |
| Either gameplay client dimension below minimum | Suspend processing/audio and show the responsive recovery shell. Restore window, Use fullscreen and Quit are the finite actions; mode requests must satisfy actual client readback before gameplay resumes. 720p gameplay is not supported. |
| Entry/slot/name forms at smaller native sizes | Keep the existing logical canvas and pending form. Final New Game publication, Continue mount and explicit backup recovery recheck native admission before writing/replacing progress. Controlled diagnostic starts wait behind recovery rather than mounting below minimum. |
| Placement and mode selection | Work area/decorations choose a fitting centered window or qualifying native fullscreen only at startup/explicit Restore. A valid-sized window dragged partly offscreen/across screens never pauses or recenters. No position polling, display-mode changes or persistent display preference. |
| Smaller physical desktop / mixed DPI or monitors | Startup can fit an entry shell when neither a baseline decorated window nor native fullscreen meets minimum. Actual 1080p physical-display fit, smaller-screen/DPI/multi-monitor experience remain **Unknown** for later human checks; no hardware pass is inferred from 4K observations. |

[WindowSupportHost](../../game/src/Entry/WindowSupportHost.cs) owns native client admission and the
finite resume decision alongside [GameRoot](../../game/src/GameRoot.cs). At startup an already
valid window is centered when its outer dimensions fit; prefer qualifying ordinary fullscreen when
a minimum-sized decorated window cannot fit, including an OS-clamped startup client. Otherwise choose a
fitting 1600×900 window when needed. If neither fits, retain a smaller entry shell instead of forcing
an oversized default window. On desktops that can fit a baseline window, smaller entry launches can retain their size until
gameplay is requested.

Restore reads the actual windowed borders after its request, including the first transition from a
fullscreen launch whose border size was unknown. If actual client or outer dimensions cannot fit,
restore the prior mode and, for windowed mode, its prior size/position. A still-invalid client keeps
its recovery and explanation; a valid prior fullscreen may resume. Placement failure alone never
suspends a valid-sized client. Godot [ordinary fullscreen](https://docs.godotengine.org/en/stable/classes/class_window.html#enum-window-mode)
uses native screen size without changing video mode; a 1080p request before fullscreen does not make
a 4K monitor 1080p. Restore/fullscreen availability and refusals are explained directly in the recovery
body, accessible without hover/tooltips. No global window/device service is introduced.

[WindowRecoveryView](../../game/src/UI/WindowRecoveryView.cs) uses the existing blue/gold Theme,
a separately scrollable explanation, fixed action buttons/navigation hints, installed compact hints,
keyboard/pad edges and complete pointer gestures. Left/Right reads above/below without changing
the selected action; disabled choices cannot strand reading at the bottom. It retains
the mounted session, revision/progress, files, pending modal/draft, visibility and editor focus.
Underlying entry/name/Settings/slot controls are hidden and paused while covered, cancelling old GUI
presses. Field pause cancels future walking/wait/skip; the existing input release barrier drains held
activation, and a fresh edge is required after release/neutral. GameInput and world-layout policy
are unchanged. A failed window request sends no gameplay command or save write.

The existing entry, Settings, slots and recovery share one finite host pause predicate; closing one
cannot resume gameplay still covered by another. [SettingsHost](../../game/src/Entry/SettingsHost.cs)
and [OpeningPersistenceHost](../../game/src/Entry/OpeningPersistenceHost.cs) consume that decision.
Recovery Quit has priority over covered modals but reuses the existing SaveSlotView and dirty-progress
Quit calculation. Cancel returns to recovery and preserves the covered Settings draft/Save panel;
then valid size restores the former modal. It does not dismiss that modal or auto-save. WM-close
retains its explicit Settings cancel-draft → existing Quit ordering. New Game's
[final commit](../../game/src/Entry/NewGameSetupHost.cs) cannot write before window admission; restoring
size leaves confirmation pending for fresh input, rather than automatically starting a journey.

The two baseline default-binding overflows found during design are corrected only in
[PartyStatusView](../../game/src/Exploration/PartyStatusView.cs) and
[FieldActionView](../../game/src/Exploration/FieldActionView.cs): installed compact hints wrap within
their existing fixed hint rectangles. Full bindings remain in Controls help. Actual standard/swapped
1080p text bounds pass. Party/Prepare/Church still have native fixed coordinates, so their existence
never establishes 720p support. The finite visual migration under
[#1453](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1453) is integrated; it does not
establish full-game visual completion.

Use the [finite host recipe/results](../verification/host.md#window-boundary-design-observation-1455):
adapter build, clean committed dependency plan, direct source/docs/private-boundary checks and actual
native state/input/geometry/errors. No engine/full suites, helper tests, screenshots, full route or
new collection platform. Physical off-focus release, actual 1080p display/DPI and human readability
remain **Unknown** with the later [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456)
checklist. Existing failures remain intact; the
[#1447 investigation](../application/opening-session-lifecycle.md#title-lifecycle-investigation-1447)
adds no physical-focus acceptance or production change.

### Battle movement range agreement

**Current reproduction:** #1686 changes range opacity/rims and adds real action/effect range. The B8 geometry/alpha assertions below and `engine_movement_range_observation.gd` are historical at [the retained implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes/engine_movement_range_observation.gd). Use the [current production observer](battle-interaction-baseline.md#direct-production-reproduction) for movement/undo, range, target and lifecycle UI. Original movement legality, clocks and source evidence are unchanged.

Standard rigor applies to B8 / [#1395](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1395):
a bounded read-only engine query and battlefield projection. The outcome is visible legal stopping
tiles during player movement selection, retaining the accepted 1920×1080 world and 96px cells.
Movement rules, world-click input, danger ranges, command animation and broader UI work are outside
this agreement. [Host observation](../verification/host.md#battle-movement-range-observation) owns
reproduction and its headless/windowed acceptance boundary.

`GameSession.QueryBattleMovementRange` returns immutable actor/origin/destination information only
for a living player actor in Movement, including that actor's preview/return animation. Domain
`BattleMovement.Destinations` builds the existing weighted mover/terrain/budget/enemy grid once,
then reuses Preview's path validation with that grid. Living allies remain traversable but cannot
be stops; enemies block traversal; the actor's own committed origin is included. Map bounds,
impassable terrain and budget remain authoritative. Unexpected path-validation failures propagate;
they are never silently relabeled as unreachable tiles. The query changes no session state, RNG,
resources, revision or observation sequence.

The origin is the committed actor position, independent of the provisional cursor and moving sprite.
Action, equipment, list, target and commit stages have no range. Automatic movement, rounds, story,
scenes, outcomes, retreat and field mode have no range. Cancel restores it from current state; next
actor and rebuilt map/session presentation compute fresh information. No new cache or invalidation
protocol is used. One grid plus bounded path checks runs when session state is presented, never per
render frame; camera animation only transforms the retained drawing. Work is bounded by the existing
48×48 movement grid and reachable paths, with no per-destination grid propagation.

`BattleMovementRangeView` draws quiet cyan fill (alpha 0.20) with an edge (alpha 0.55) inside each legal
96px cell, using the existing clipped board and camera transform. Ground is Z0; the range follows
ground at Z0; actor sprites/labels remain Z1; original foreground walls remain Z2; selection/target
marks remain Z3. Preserving foreground occlusion is part of acceptance. No new art, zoom, camera or
menu geometry is introduced. Source-original range pixels, animation and hardware cadence remain
**Unknown**; this is an accepted modern affordance, not an original presentation-parity claim.

### Battle selection and preview feedback (#1617)

**Standard rigor.** This bounded presentation change removes the selected actor's gold modulation and
the gold underline at every preview-path cell. It preserves the accepted 96px scale, legal movement
query, actual segment delivery, target selection, input boundary and accessibility. Inspection
(#1633), the whole visual redesign (#1616), gameplay/path legality and original frame/pixel recreation
are outside this agreement. The [pinned original evidence](battle-scenes.md#battlefield-selection-evidence)
distinguishes movement, actor blinking, grid feedback and target cursor; it does not justify the old
gold treatment or supply exact original visuals.

`BattleMapViewport` now leaves every map sprite's `Modulate` white and retains faction colors on
authored labels. `BattleSelectionView` draws one unfilled set of white corner brackets with a dark
outline around the acting unit, following its actual interpolated position through preview and undo.
The cue consumes the selected projected marker, not another gameplay position. At rest it identifies
the provisional destination; no breadcrumb trail is drawn. The existing cyan legal-stop overlay
remains based on the committed origin during Movement, and the separate cyan full-cell border
identifies the accepted target during target/commit selection. Back clears that target border
through the ordinary parent return; the acting-unit cue remains separate. The board/viewport clip
both cues; selection and target are Z3, above source foreground Z2, without changing sprite/terrain
ordering.

The brackets are steady in both normal and reduced-flash modes. They preserve sprite ink and use
shape plus a contrasting outline rather than color or blinking alone. This is an explicit modern
accessibility decision, not reproduction of the source menu blink or cursor art. No timer, alpha
animation, asset, state cache, new input or gameplay service is added. `ReadTerrainJson` exposes the
actual cue position/draw count/style and existing range/target operands; actor observation includes
actual sprite modulation. Existing preview geometry now describes this single acting-unit cue,
not the removed trail. Human legibility/aesthetic approval and original pixels/timing remain
**Unknown** beyond the bounded engineering observation.

Acceptance uses adapter compilation, a Debug build of the owned Godot project, direct scope/source
checks and the clean committed planner. The focused
[`engine_battle_selection_observation.gd`](../../game/probes/engine_battle_selection_observation.gd)
drives ordinary mapped input and reads actual actor/range/target nodes: initial actor, interpolated
preview, action menu, undo, range rejection, target switching and Back. It checks retained committed
positions, HP/MP, both seeds and queue position across browsing. No engine rules change, so no new
engine unit test or broad suite is required. Use the existing selected installation and local caches;
no original runtime, screenshot, frame capture, comparator or asset/world copy is needed.

After the [locked environment setup](../development-and-verification.md#locked-net-workflow), supply
`SF2_VIEWPORT_OUTPUT` as a fresh ignored report path and launch the ordinary host with
`--script res://probes/engine_battle_selection_observation.gd -- --input-settings <settings>` plus
the ordinary authored content entry. Use 1920×1080, `--max-fps 60` and an explicit local `--log-file`.
The authored input is garden-watch with care's maximum range raised to 3 so both living allies can
be browsed; the retained #1615 navigation input also works. Run with `reducedFlash: false` and `true`.
For the mounted private sprite check, set `SF2_SELECTION_PRIVATE=1` and `SF2_SELECTION_START` to the
controlled Map57 approach at (8,14), facing Down, flags 0,1,2,32,33,34,66,80,401,451. Reuse the
admitted connected world, private static/scene selections and existing
`StartPreVoyageLocalizationObservation` startup; the older private-exploration CLI entry does not
admit this multi-encounter world. Actual input then enters Battle01 and browses Chester's slot1
Medical Herb targets; this is not natural full-route evidence. The ignored
`local/issue1617/HANDOFF.md` owns exact local selections, commands, results, preserved probe failures
and process state. Reports are compact semantic checkpoints; four bounded normal/reduced
authored/private observations suffice, with a 100MiB local-output budget and no large acquisition.
Reassess a startup/consumer failure before expanding the run.

**Confirmed, bounded headless native observations:** the authored healer and connected Battle01
Chester both retain one visible acting-unit cue through real preview/return, hide/restore movement
range with its unchanged destination set, reject the range edge without moving the accepted preview,
switch real Magic/Herb targets and clear target borders on Back. Mounted private sprites retain
white modulation and their admitted textures. Normal/reduced-flash runs preserve the compared
committed positions, HP/MP, queue position and both RNG seeds; the 96px cue stays steady at idle.
No script/native error was reported in the passing runs. Some private exits report the existing
class of ObjectDB leak warning (4–6 instances); the final normal verbose run reports none. Its
intermittent cause remains **Unknown**, not a resolved lifecycle claim. Authored input and controlled
Map57 admission establish this consumer boundary, not natural original reach or human visual approval.

### Battle inspection and on-demand Map (#1633)

**Current UI override and reproduction:** #1686 adds hover cards, pointer blocking, edge scrolling and labeled vertical Options; A/Confirm opens details and Back restores any inspected cell. The mini-status/current-actor direct-return/cardinal Options and no-pointer-Map UI below describe the retained [#1633 driver at its old implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes/engine_battle_inspection_observation.gd), not a current runnable acceptance recipe. Use [production F8 reproduction](battle-interaction-baseline.md#direct-production-reproduction). The query, disclosure, real undo, stale-session and original S2 contracts remain valid.

**Standard rigor:** the accepted [minimum contract](battle-interaction-baseline.md#inspection-contract-and-original-return-distinctions)
owns scope and caller returns. This implementation connects original-style free inspection to the
common battle host, including admitted connected Battles01–07. It does not admit a later encounter,
Speed, empty-ground Quit, a preview-preserving shortcut, action
prediction or a visual redesign. The empty-ground options now offer Map and the
[Member extension](#battle-member-roster-and-read-only-details-1645).
Connected Equip/Give/Drop have separate finite product admissions in the
[feature trace](battle-interaction-baseline.md).

`GameSession.QueryBattleInspection` reads the current settled player Movement boundary. Optional
session/revision and requested-unit identity reject stale or undisclosed requests explicitly.
Movement delivery, provisional paths, AI, scenes, Give/Discard, round/appearance/program, outcome,
fault and exploration are unavailable. `BattleVisibility` is the extracted ordinary-board staged
entity filter shared with `BattleSnapshotProjection`; no reveal history or second battle is stored.
The result contains admitted board dimensions/region, known living placed/initialized unit markers,
live public scalar details and inspected-actor land effect. It exposes no queue, AI memory, reserve
list or predictions. Unknown private names stay unavailable. Private numeric stats use the pinned
source `UNKNOWN_STAT_VALUE_THRESHOLD = $64` (100), including current/max HP/MP, ATT/DEF/AGI/MOV;
authored values retain their authored policy. Enemy class/LV/EXP are N/A. Admitted curse, poison,
stun, sleep and silence are named; other status details and durations remain unavailable. Shared
`BattleTerrainRules.LandEffect` owns 0/15/30 by mover/protection; the combat multiplier consumes the
same interpretation. This displayed setting is not the damage-reduction percentage.

`BattleSessionView.Inspection` intercepts B only in settled Movement. With no provisional move it
enters directly without Submit. Otherwise it submits the existing Cancel once and waits for every
actual return segment delivered by `BattleMapViewport`; only then does it open at the actor’s origin.
Action-menu Cancel still returns to Movement. The query identity binds the disposable adapter mode
to session/revision/actor; an ineligible boundary retires it. While active, directions and details
never submit gameplay or advance automatic work. The purity baseline starts after undo delivery.

Mode-local **A** uses the configured Attack binding (default F / Pad West) for a full sheet; **C**
uses configured Confirm (default Enter/Z / Pad South) for short status; **B** uses configured Cancel.
Hints use the installed, potentially swapped bindings. Current-actor C returns directly to Movement;
current-actor A’s full sheet returns there on A/B/C dismissal. Other units’ A/C sheets return to their
cursor. Full sheets reuse `PartyDetailsPanel` scalar widgets; short status reuses `BattleStatusCard`
and names the actor/location of its land effect; mover semantics remain in the query. Tab/Target switches Magic/Items list focus,
Up/Down scrolls it, and wheel input over either list uses the existing detail widget. Browsing never
opens a transaction. A/C on empty ground opens options; configured Up/Down chooses Map or Member,
then A/C opens that choice. B from Map returns to options, then B returns to cursor, then B restores
the same actor’s Movement.

`BattleInspectionView` retains only cursor, panel/caller and selected public unit. It uses
`BattleSessionView.Navigation`’s existing repeat clock and `GameInput`’s held direction. Page/entry/exit
changes disarm it and require mapped release before another action. Focus/pause/hide retain the
existing isolation. Host Menu pause/return skips its usual save-music observation while inspection
is active, preventing a revision-only invalidation. Its Save offer supplies the actual live audio
observation to `QueryBattleSave` for a pure eligibility assessment through the existing music and
strict save/restore validators; an unbound history cannot dead-end the fresh inspection menu.
This publishes no revision or music state. Explicit Save still publishes the real observation and
performs strict capture at its existing boundary before writing. Host Menu Save-and-return retires inspection
when that explicit publication changes its revision. The dedicated [Options Quit caller](#battle-options-quit-1651)
rebinds its same-session/actor inspection after the paused explicit save instead. Other independent mutations or session
replacement also invalidate inspection.

`BattleMapViewport` owns a temporary cursor subject, so ordinary Frame cannot replace it. Exit
removes that override and resumes actor framing. `WorldViewportLayout` retains 96px cells,
SourceMapRegion offset, legal clipping and small-board centering. Opening panels never resizes the
world. `BattleOverviewView` draws the whole admitted area with a schematic semantic-terrain layer, disclosed ally circles/enemy diamonds,
active-unit ring and clipped camera rectangle. Only Width×Height local terrain cells are published,
using the existing 48-cell storage stride; neighboring packed cells are excluded. The localized
legend pairs readable terrain-class names with distinct swatches. Configured directions navigate its bounded known-unit list and
highlight without centering the camera or issuing any action. No Map pointer control is offered.

**Confirmed bounded engineering observations:** the committed
[`engine_battle_inspection_observation.gd`](../../game/probes/engine_battle_inspection_observation.gd)
uses actual host input/state and projected nodes. Controlled Map57 approach reaches ordinary Battle01
player Movement with registered content and disposable save namespace; it is not a natural opening
replay. The existing `hidden-spawns` authored case establishes reserve/appearance admission separately.
These observe moved and no-move entry, real return delivery, the caller-specific A/C/B returns,
live ally/enemy cards/sheets, an off-camera disclosed enemy, Map/options/cursor, edge framing, held
controls, installed remapping and swapped Confirm/Cancel, pad input, focus-handler signals, host
pause/return and actual host replacement. Post-undo comparisons include session/revision/sequence,
selection/path, both seeds, queue/cursor, resources, loadouts/status and story state. A supplementary
existing controlled Battle02 boundary checks nonzero origin `(16,19)` and opposing clipped edges;
the small authored board stays centered. Engine behavior tests cover query purity, stale/busy and
hidden rejection, changed live values/committed equipment, and Regular/Heavy, Flying/Heavy and
Hovering/Light land effects. No screenshots, per-frame export, emulator acquisition or new comparator.

Reproduce after the [locked environment setup](../development-and-verification.md#locked-net-workflow),
affected engine checks and adapter Release/Debug build. Reuse the owning project/installation. Set
`SF2_VIEWPORT_OUTPUT` to a fresh ignored report, `SF2_INSPECTION_ROUTE` to `keyboard`, `pad` or `axis`,
and `SF2_INSPECTION_HIDDEN=1` for the existing authored `hidden-spawns.json` package. The ordinary host
uses `--authored-package <absolute-package>` and optional `--input-settings <absolute-settings>`.
For the connected check use the registered world/action/scene/presentation selections, no startup CLI
package, `SF2_INSPECTION_PRIVATE=1` and `SF2_INSPECTION_START` pointing to the existing controlled Map57
approach. Set `SF2_INSPECTION_REGION_START` to a local controlled-start document with
`{"formatVersion":1,"start":{"battle":"battle-2"}}` for the replacement geometry-only boundary.
Run the probe with `--headless --max-fps 60 --resolution 1920x1080`, explicit `--log-file`, and the
ordinary `--path`/`--script res://probes/engine_battle_inspection_observation.gd` options. Require a
completed passing report, process exit and no script/native errors. Each run is bounded to75 seconds;
only compact semantic checkpoints are written. Three finite input routes and affected retries fit
within a 100MiB local output budget; no large processing/acquisition is authorized.

A separate `SF2_INSPECTION_SAVE_ENTRY=verify` private route starts before any earlier host Menu or
music observation. It checks actual Save enabled/reason against ordinary Movement, pure inspection
Menu open/close, then a fresh inspection → Save → slot confirmation → disposable Save and return.
The ordinary strict query/capture still rejects unknown history before the explicit Save; choosing
Save binds actual `MUSIC_BATTLE_THEME_1` history, strictly captures, writes, and returns to the same
actor/origin/resources/queue/seeds without passing its held confirmation into gameplay. The
pre-correction disabled fresh Save entry is preserved as a completed failing observation.

The focused `SF2_INSPECTION_UI_ONLY=1` private route checks en/zh-CN key admission, actual Chinese
full/short/options/Map/terrain labels, remapped direction hints and actor-qualified land effect
without raw mover wording. It uses the same registered Map57/Battle01 and Battle02 region-only
inputs and does not repeat the gameplay route. The Map draw count and per-class histogram match
the admitted Width×Height projection, with multiple classes, distinct colors and camera/session
purity. The native probe reads actual projected text/draw consumers; it is not a test of a report.

Ignored `local/issue1633/HANDOFF.md` owns exact heads, selected commands/reports, preserved failed
attempts, CI and process state. Initial probe parse/parent-return assumptions, a real private Menu
music-revision failure and a zero-origin fixture assumption are retained there. Some private failed
exits and the focused Chinese passing exit emitted the previously observed ObjectDB warning (two instances); its intermittent cause remains
**Unknown**. These checks do not establish physical OS focus switching, full campaign coverage,
original free-cursor cadence/pixels, every source minimap category or human visual approval. Member’s
list-specific return is covered by the separate [#1645 acceptance](#battle-member-roster-and-read-only-details-1645).


### Battle Member roster and read-only details (#1645)

**Current reproduction:** the retained member driver assumes pre-#1686 cardinal Options and full-card UI. Its [historical script](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes/engine_battle_member_observation.gd) remains evidence for that named boundary. Use the [current production F8 observer](battle-interaction-baseline.md#direct-production-reproduction), case `retreat`, for connected joined-member detail/row return. Membership/disclosure and source caller rules remain unchanged.

**Standard rigor:** this bounded multi-layer extension completes Options → Member → selected full
sheet → same selected roster row → same Options choice → cursor → the same actor's settled Movement.
It reuses the inspection session/revision/actor lifecycle, mapped input/release barriers and
`PartyDetailsPanel`; no transaction, new battle authority or persistence format is introduced.
Scope is the current connected private Battles01–07 admission, with actual connected input observation
on Battle01. Later campaigns, Speed/Quit and connected Drop remain separate; connected Equip/Give
have their own [product admissions](battle-interaction-baseline.md).

**Confirmed static disclosure:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, S2 `BattlefieldMenu` calls `UpdateForce`, copies
`BATTLE_PARTY_MEMBERS` into the generic list and loops from `BuildMemberScreen` back to that list.
`common/stats/battleparty.asm:UpdateForce` puts only joined **and active** IDs into that list, in member
index order; joined reserve IDs go to `RESERVE_MEMBERS`. It does not test HP or map position. S5
`BuildMemberScreen` also handles zero current HP. Reproduce with `git show <pinned-commit>:disasm/code/`
plus `gameflow/battle/battlefunctions/battlefunctions_2.asm`, `common/stats/battleparty.asm` and
`common/menus/memberscreen.asm` in the registered upstream checkout. This static boundary supports
active defeated/unplaced members; it does not authorize revealing hidden enemies or reserve actors.
Original timing/pixels and natural original reach remain **Unknown**.

`GameSession.QueryBattleMembers` shares the small settled-origin eligibility check with
`QueryBattleInspection`, without materializing Map terrain/markers. For private/connected content it
rebuilds joined/active membership from the live flags using the admitted party layout (the existing
source layout for battle-only private content). It selects only matching live ally actors and orders
by member index. Explicit authored encounters without source membership use their live allies in
processing order. It never calls field `QueryPartyDetails`, starts a battle, or initializes a missing
member. Dead/unplaced initialized members retain their actual details with nullable position;
uninitialized active entries show unavailable and cannot open a fabricated sheet. An empty roster
shows an explicit empty message and Back remains available. Missing, enemy or reserve detail requests
reject explicitly. No live actor means no invented row or scalars.

`BattleUnitProjection.Details` now supplies both map inspection and Member from the same live scalar,
status, spell and loadout readers, retaining private ≥100 stat unavailability and missing inventory
semantics. `BattleInspectionView` uses a distinct disposable `MemberFull` caller: A/B/C returns to
`Members` with its selected index, including the current actor. Direct cursor A on that actor still
closes to Movement. Options retains the Member choice after list Back; Map remains reachable.
`BattleMemberListView` presents a bounded moving list window, defeated/absent/unavailable annotations,
and localized en/zh-CN copy. Up/Down, A/C/B and detail reading hints come from `GameInput`; browsing
never submits gameplay. Changed revision, actor, unavailable control or host replacement retires the
same inspection context. No Member state is saved.

**Confirmed bounded engineering acceptance:** the finite engine behavior selection covers live scalar
reuse/purity, active versus joined/reserve/unjoined disclosure, dead/unplaced and uninitialized states,
empty membership, missing/enemy requests, stale revision/session and story/field/busy rejection.
The actual connected observer
[`engine_battle_member_observation.gd`](../../game/probes/engine_battle_member_observation.gd)
reuses the existing inspection input and signature methods. Its controlled Map57 `(8,14)` approach
reaches ordinary Battle01 Movement; it is not a natural opening replay. English/default and Chinese
remapped keyboard runs verify current/other member sheets, all A/B/C current-sheet returns, selection
retention/wrapping, held entry/close release barriers, detail scrolling, host Menu pause/return,
Member/Map parent choice, final same-actor Movement and actual host/session replacement. The unchanged
post-entry signature covers session/revision/sequence, selection/path, queue/cursor, resources,
loadout/status, story and both seeds. Chinese labels and hints are read from actual projected nodes.
Defeated/unplaced/empty/unavailable shapes are engine checks, not claimed natural connected encounters.

Reproduce after the existing locked environment, affected engine checks, adapter and Debug build.
Use the registered world/action/scene/presentation selections; set `SF2_INSPECTION_START` to a
worktree-local controlled Map57 approach document and `SF2_INSPECTION_ROUTE=keyboard`.
Set `SF2_MEMBER_ZH=1` with ordinary `--input-settings <absolute-settings>` selecting `language=zh-CN`
and the desired existing remapping for the second run. Set `SF2_VIEWPORT_OUTPUT` to a fresh ignored
report; run the ordinary host with `--headless --max-fps 60 --resolution 1920x1080`, explicit
`--log-file`, `--path` and `--script res://probes/engine_battle_member_observation.gd`.
Each run has the inherited 75-second deadline and compact checkpoints; two routes and an affected
retry stay below 100MiB. No screenshots, world copies, original runtime acquisition or new comparator.
Require a completed passing report, exit 0 and no script/native errors. The initial completed observer
failure read a nonexistent diagnostic `ally` field; it was corrected to consume the public disclosed
marker, with its failed report retained. Both passing runs retain two ObjectDB exit leak warnings;
the earlier WAV/playback warning history remains unresolved. Human/aesthetic acceptance, broader
natural route evidence and hardware fidelity remain **Unknown**.

### Modern UI experiment

The user-selected direction for #438 retains the original diamond command arrangement and spatial
input relationships. The [common-menu input contract](../../../docs/research/presentation/common-menus.md#confirmed-input-contracts)
maps up/left/right/down to choices 0/1/2/3; the
[battle action contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md)
orders attack, magic, item and search-or-stay. Search admission remains a gameplay/content decision;
an interface experiment showing Stay does not remove the source-specific Search route.

Modernization separates world framing, UI dimensions and drawing resolution. The current experiment
uses the original square command rasters directly, arranged up/left/right/down. No additional button
frame or per-icon caption surrounds those rasters. One compact native-text window identifies the
focused command, with an unavailable reason when needed; main-menu keyboard instructions and generic
description paragraphs are hidden. Do not uniformly enlarge the entire original menu or preserve its
tiny physical size. Icons keep their original selected state; selected-item costs and unavailable reasons
sit against the right safe edge, leaving the middle open. Magic and inventory use list submenus, with explicit confirmation and a
cancel path back to the preceding selection. A vertical list replacing the main diamond is not the
selected direction.

The current 1920×1080 layout uses 96×96 icons (4× source pixels) in a compact 288×224 cardinal
cluster, with 64-pixel vertical steps between the top, side and bottom positions. This is a modern
spacing choice inspired by the source arrangement, not a pixel-exact original layout claim.
The battle map occupies the full game viewport; command menus, lists, hints and supplementary HUD
draw above it. Opening or closing UI must not resize the map, alter its zoom or reframe its current
focus. Gameplay movement/target changes use the shared fixed-scale pan policy above.
The earlier1280×720 menu observation remains historical acceptance of its own UI slice.

The existing graphics-remake experiment at commit `172662709b31cb296b55f51894754ce810c30f6c`,
owned by its `docs/ui-treatment-01.md` and `src/remaster/vector_ui.py`, supplies the window-chrome
reference. Its Yes/No sample is not a complete UI kit: placeholder text and the embedded private item
image must be separated from reusable frame geometry. Private art is not licensed by this selection.

The ordinary host supports the first playable iteration through `BattleCommandMenu` and
`BattleMenuLayout`, using a shared selection-name window and the original 24×24 normal/selected
command rasters **when the selected scene contains them**. The [ordinary publication repair](#ordinary-menu-resource-acceptance-1666)
separately proves current startup selection. Icons use nearest sampling at integral scale;
icon positions and the shared text window scale independently. No authored replacement glyph is used. Public scenes
without these rasters show text-only squares. The [asset owner](../content/presentation-assets.md#private-command-menu-icons)
defines preparation and admission through the existing scene raster collection.
Standard rigor applies: one bounded interaction slice, with the existing semantic interface as its
dependency. Acceptance covers cardinal focus, spell/item lists, costs and unavailable reasons,
target selection, explicit commit, layered cancellation and actual resized node geometry. Engine
behavior tests cover back navigation; the [host recipe](../verification/host.md#diamond-menu-observation)
checks real keyboard input and projection. Final visual/user acceptance and wider Epic completion
remain separate from these engineering checks. Original pixels are private local inputs; neither the
tracked UI implementation nor public builds include them. The graphics sample remains a chrome reference.

Confirm at movement opens the diamond. Up/left/right/down focus Attack/Magic/Item/Stay; Confirm
enters the focused choice. F/H/I/Space are shortcuts from the main diamond; H/I now open lists,
not immediate selections. Up/down inspect list rows, including disabled rows. Confirm selects an
enabled offer and its first enabled target (self when offered). Arrows or Tab cycle all offered
targets; a disabled candidate shows its reason and cannot commit the previously accepted target.
Target confirmation commits through the session. Stay has its own confirmation step.
Equipment/Give/Discard retain their existing shortcuts and presenters from the main menu. Spell and
item lists group by the semantic offer's spell/item metadata, including admitted retreat offers.

Cancel returns target selection to its list or main diamond through `ReturnToBattleActions`, which
clears provisional action/target while preserving movement preview, resources and RNG. List cancel
returns to the same cardinal focus; main cancel uses the existing `Cancel` to return to movement and
undo the preview. Menu focus is disposable adapter state; actions and targets remain session-owned.
The UI never calculates range, faction or effect legality. Unsupported Search is not added by the
Stay slot. Actor labels retain current content identities; a complete naming/portrait treatment is
outside this iteration. Pointer activation and new controller acceptance are also outside scope.

Continuous exploration movement retains its [accepted control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk).
Its [successful-step continuity policy](../verification/field-programs.md#successful-step-continuity-1585)
uses the existing ordinary clock: after an owned step completes, a still-held legal direction
submits the next Move at that same service boundary. Successful steps have no separate repeat
deadline; a refused attempt retains120 ms backoff. Late turning, release/fallback, story/map/battle
ownership and lifecycle release barriers remain live-input decisions. This bounded Standard adapter
change preserves motion speed/animation and menu repetition. It is a modern control choice;
original natural timing and #1456 subjective acceptance remain Unknown.
The shared 1080p world viewport is defined above; the earlier 640-by-360 proposal is not a runtime constant.

### Battle directional hold and mode isolation (#1615)

**Current navigation:** #1686 uses Up/Down vertical command/Options lists and cycles only legal targets. Its [production observer](battle-interaction-baseline.md#direct-production-reproduction) replaces the old cardinal/disabled-target menu route in [`engine_battle_navigation_observation.gd` at this pin](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes/engine_battle_navigation_observation.gd). The held-direction clock, release barriers and source movement rules below remain binding.

**Standard rigor:** battle movement and existing directional selection need a consistent held-input
boundary across provisional movement, menu pages, target candidates and actor changes. The finite
scope is adapter navigation; engine legality, action costs, RNG, field speed, Shift acceleration and
inspection retain their existing owners.

`GameInput` remains the single mapped keyboard/button/signed-axis edge and held-direction authority.
For menus and inspection, `BattleSessionView.Navigation` applies a fresh direction immediately, then
repeats at most once per 120 ms deadline. For provisional movement, `BattleMapViewport.ConsumeMovement`
delivers the current step. The battle view then submits the still-held direction on that frame.
Turning during delivery replaces the direction used at arrival. The
[held preview observation](battle-interaction-baseline.md#held-preview-step-handoff) covers that handoff.
Exploration's successful-step cadence and rejected-attempt backoff retain their field owner.

Directions repeat in Movement, the cardinal command menu, spell/item lists, offered targets and
existing admitted Equipment/Give/Drop selectors. TargetChoice and CommitReady share their target
navigation page; a different candidate does not create a new mode. Stay/transaction confirmation and
result/scene/round/return phases are ineligible. Disabled target candidates retain their actual reason
and cannot commit a previously accepted target. The engine still validates each submitted Move or
selection against live state.

Release of the active direction stops future repeats and does not resume an older held direction.
A fresh turn in timed navigation restarts the deadline. Any non-direction action or pointer input disarms the hold;
rejection, session/actor/stage/page changes, hide/pause and focus handlers also clear it. Host modal
return keeps the existing release barrier. Only a new mapped input edge can resume navigation.
Confirm, Cancel, Tab/Target and shortcuts remain edge-only, including keyboard Echo and repeated pad
button/axis events; holding them cannot confirm another page or spend another actor's action.

**Confirmed bounded engineering observation:** the ordinary authored host accepts standard keyboard,
custom directions with swapped Confirm/Cancel, mapped D-pad/buttons and signed stick input. The
[finite consumer probe](../../game/probes/engine_battle_navigation_observation.gd) reads actual state,
movement/preview rectangles and menu candidates, with release/turn before rejection, range rejection
and fresh recovery, list/target/back, repeated Confirm/Cancel edges, host pause/release return,
hide/show, focus-handler return and real next-actor isolation. Existing authored equipment/Give/Drop
examples separately exercise their actual selector, projected candidate and free cancellation.
Menu/provisional movement preserve both seeds; cancellation preserves the observed resources.
These observations establish the common adapter boundary, not private campaign replay, physical OS
focus switching, original repeat timing or subjective controller/human acceptance.

Reproduce after the [locked environment setup](../development-and-verification.md#locked-net-workflow)
and adapter Release/Debug compilation. Use fresh ignored output, explicit `--log-file`, 1920×1080 and
the existing Godot installation. No private input or new installation is required. For the main
probe, derive one small ignored variation from `remake/content/authored/garden-watch.json`: keep its
start/terrain/actors; replace `spells` with `care` levels 1–3, each retaining its original effect/cost
and range 0–3, and give `herbalist-b` those three spell references. This supplies multiple real list
rows and both admitted and unavailable target candidates without changing production content.

```powershell
$env:SF2_BATTLE_NAVIGATION_OUTPUT = Join-Path $freshIgnoredOutput 'navigation.json'
$env:SF2_BATTLE_NAVIGATION_ROUTE = 'keyboard' # also pad or axis
$env:SF2_BATTLE_NAVIGATION_AUX = ''
& $env:GODOT_BIN --headless --max-fps 60 --resolution 1920x1080 `
  --log-file (Join-Path $freshIgnoredOutput 'godot.log') --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_battle_navigation_observation.gd -- --authored-package $navigationPackage
```

Add ordinary `--input-settings <absolute-path>` for admitted custom/swapped mappings; the probe reads
installed bindings rather than duplicating a key table. `pad` uses device 0 D-pad/buttons; `axis` uses
device 0 signed axes with mapped keyboard fallback for actions lacking axes. For each auxiliary
check set `SF2_BATTLE_NAVIGATION_AUX` to `equipment`, `give` or `discard` and select the corresponding
tracked `remake/content/authored/battle-<kind>.json`. The probe bounds each run to 35 seconds, writes
compact boundary samples and exits nonzero on failed assertions. Require a completed passing report
and no native/script errors in addition to process exit; a script exception can otherwise exit 0.
Screenshots, full-route replay and tests of the observer are unnecessary for this adapter change.

### Ordinary movement camera continuity (#1607)

**Standard rigor:** remove a reproduced ordinary-field camera/actor phase mismatch. The existing
60 Hz service count, movement admission, held-input barriers, collision/story rules, animation
counters, velocity and RNG remain the acceptance boundary. Own only the camera update in
`ExplorationPresentation`, its ordinary-clock call site and this agreement. No interpolation,
extra service, movement-distance change, repeat-deadline change or original hardware-timing claim.

The camera consumes the final snapshot of the ordinary callback before drawing its actors.
A `finally` exit also covers the last settling service that opens a paused menu, and a service that
hands to a scene or transfer. Retired/battle-handoff and failed fields cannot drive that camera.
The existing dead zone, clamp, camera speed and integral output-pixel rounding remain unchanged;
there is one camera advance per callback, including callbacks with no due logical service.

**Confirmed (bounded remake):** before this correction, camera focus read the pre-service actor,
while drawing read the post-service actor. At ordinary speed32 and4× projection, a repeated straight
step changed actual actor draw Y between444 and452 at the install-only boundary. Moving camera
projection after the service keeps Y444, including the final service opening the real system menu.
The menu case settles at tick13 and retains the final actor/camera projection while paused. Short
turn/release/wall observations and the existing continuous-input observer retain story/menu,
hide/pause, map and battle handoff barriers. Matched fixed callback/input sequences preserve every
recorded logical entity, destination, velocity, animation counter, revision, flag and RNG seed.

The ordinary movement service still installs a target before later services integrate travel;
a32-speed cell retains13 services, including installation. Its zero-displacement installation is
not removed or treated as a new input-repeat defect. Animation counters carry across steps. Both
the movement increment and the sprite-phase increment have static owners in pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/common/scripting/entity/entityscriptengine_2.asm:UpdateEntityData` and
`entityscriptengine_1.asm:VInt_UpdateSprites/@DetermineWalkingFrame`. This source structure does
not establish modern/original natural frame cadence; neither counter nor pose selection changes here.

For the finite camera witness, use a prelaunch controlled visual field with area(0,0)–(30,30),
player(15,15), speed32 and a wall at(15,20). Freeze automatic callbacks at attach as in the existing
[ordinary-clock observer](../development-and-verification.md#native-reproduction), then drive the
actual view `_Process` at1/60 with live Down, Up, Right and release edges. Read logical entities and
`worldGeometry` through `ReadObservationJson`; after a real draw, match `cameraProjection`'s tick
and actor coordinates. The first four uninterrupted steps exercise the dead-zone edge. Request the
real system menu during the first step to exercise the settling/paused exit. Ordinary-speed
`--max-fps60` sampling retains frame-time statistics separately; the bounded native sample had
median16.666ms, without establishing a worst-case performance guarantee. No frame files or images
are required. The existing [successful-step observer](../verification/field-programs.md#successful-step-continuity-1585)
provides the input/story/transfer suffix using its unchanged speed32 prelaunch variant.

Compile the adapter and inspect the committed dependency plan and actual CI. The ignored Issue1607
handoff retains exact commands, input selection, compact reports and process completion. Subjective
smoothness, an independent animation-specific defect, hardware pixels and original natural cadence
remain **Unknown**; this correction establishes the reproduced camera/actor phase defect only.


## Entry windows and hints (#1445)

The [title lifecycle finding](../application/opening-session-lifecycle.md#title-lifecycle-investigation-1447)
distinguishes the short instance-lifetime entrance from static title idle/return and finite title
music. Reduced-flash selects static frames. Native state/draw progression does not establish
perceived animation, human audibility or aesthetic approval; no presentation change is authorized
by that investigation alone.

Standard rigor: this bounded presentation slice makes title, Witch choices, slot/recovery/error
windows and name entry consistent. The existing High-rigor save/session agreement is unchanged.
The selected modern blue/gold chrome grows through an opt-in entry theme: readable typography,
spaced controls and distinct normal, selected, hover, pressed, disabled and editor-focus states.
Existing story/gameplay consumers retain their current theme. No general widget or device service
is introduced. Menu selection remains disposable adapter state; bindings remain `GameInput` authority.

At the existing1920×1080 canvas, reserve the Witch's entire4× art rectangle (160,92,1024,896).
Use the separate right region (1216,64,640,952) for ordinary choices, slots and confirmations;
no full-screen shade covers the art. Greeting text reuses the story reading/pagination component
with entry-only bounds. Naming now uses a focused physical-keyboard `LineEdit` in the same right
window (#1446), with seven-character validation, the current default and immediate length/character feedback and
retained draft/Back semantics. The controller alphabet grid and its key-theme variants are removed.
The existing entry editor-focus style and mouse Back/Use name buttons remain.

One compact contextual line shows the primary keyboard binding and one mapped pad alternative for
the currently available actions (for example `Enter: Choose • Escape: Back`, or the active pad labels);
full alternatives remain in existing binding help/tooltips. It
reads the installed mapping after Confirm/Cancel swap. No device detection or duplicated key table.
Errors retain their diagnostic and safe retry/back actions; long slot labels wrap within the window.
The name editor shows the available fixed Enter/Escape actions;
window focus recovery or the next key restores text focus without changing the draft. All naming
key events bypass mapped game actions; pad events update existing release bookkeeping but are
consumed before either GUI navigation or the hidden slot/entry menu. Leaving naming still uses the
existing held-input barrier and subsequent menus retain mapped keyboard/pad access, including swaps.
Normal-only, overwrite, backup recovery and safe publication remain owned by the
[session lifecycle](../application/opening-session-lifecycle.md). Title animation/audio work is #1447.

Acceptance requires actual Godot input/state/projection/error readback at1280×720 and supported
resizing, including panel/art separation, text/control containment, mapped keyboard/pad and pointer
selection, disabled/failure states, draft Back and safe overwrite/recovery. Reuse the existing local
installation, project, inputs and observers, with disposable saves and fresh ignored reports.
The entry temporarily uses the project's canvas scaling and restores the previous window mode on
exit. This covers reopening it from gameplay views, which use native coordinates; it does not change
the gameplay viewport policy. Adapter compilation and the committed dependency plan select code checks; no screenshots or tests
of probes. Engineering integration does not establish user aesthetic/playability approval; that
acceptance remains tracked by [the gap owner](../evidence/opening-ui-gaps.md).

**Confirmed, executor observation:** the actual1280×720 native window retains1920×1080 entry
coordinates. Art is (160,92,1024,896); normal slot/name/confirmation windows are (1216,64,640,952),
with32px between their horizontal extents. Actual child controls/text remain contained at1280×720,
1600×900 and1100×800 letterboxing. Mouse hover/press, mapped keyboard/pad focus, adjustable greeting
reveal/advance, disabled invalid-name/entry-error states, draft Back, failed publication/Retry,
occupied-slot decline/overwrite, initial Continue and explicit backup recovery were observed.
These are modern adapter observations, not original-game or human aesthetic evidence.

Reproduce after the existing [entry/private-input setup](../verification/host.md#title-and-witch-entry-observation):
launch `GODOT_BIN --resolution 1280x720 --path remake/game` with disposable save root/namespace.
Read `SessionEntryView.ReadEntryObservationJson` for art/dialogue/selection, existing save/name
observations for state, and each visible entry window/control's `GetGlobalRect` for containment.
Resize the same window to1600×900 and1100×800, then restore1280×720. Follow New Game → slot →
invalid eight-character name → valid draft → Back/reopen → final Back → confirm; reject storage
using a disposable non-directory save root, restore configuration and Retry. From settled play,
reopen New Game and decline/confirm an occupied slot; compare only those disposable primary/backup
bytes. Relaunch Continue; in a separate disposable namespace use the resulting valid backup with an
invalid primary to exercise Cancel then explicit Recover. Repeat entry with `confirmCancel: swapped`
and with missing entry content; use adjustable text for mouse Reveal/Continue through the greeting.
Retained narrow automation: `pwsh -NoProfile -File local/issue1445/run-native.ps1 -Run <fresh-name> -Phase <phase> -Namespace <disposable-name>`, phases `pilot`, `continue`, `recover`, `reading`,
`missing`, or `swapped` (also `-Swapped`). Its local driver reuses existing input/state observers;
it records semantic checkpoints, actual node geometry/styles and native errors, with no screenshots.
Independent integration and final user visual approval remain separate gates. The retained #1445
observations describe that accepted grid-era implementation; their results are unchanged. For the
current keyboard-only editor, use `local/issue1446/run-native.ps1` with fresh disposable outputs;
its owning handoff records keyboard editing, invalid/default/draft handling, pad isolation while
focused/unfocused, held button/axis release across exit, final Start and fresh-process Continue.
No persistence/engine semantics changed, so earlier name/save compatibility evidence remains applicable.

## Battle Options Quit (#1651)

**Confirmed in bounded remake observations; Standard entry scope, existing High save consistency.**
Empty-ground Options offers Map, Member, Speed and Quit. A/C on Quit enters the same shared two-slot
Save/Save and Quit window as the host Menu. This entry requires current settled inspection Options;
it does not make preview, movement delivery, scene or standalone diagnostic content saveable.
Unavailable content reports the existing reason and Back returns to the same caller.

Slot Cancel, overwrite Cancel → slots → Cancel, error Back, uncertain Back, uncertain Inspect →
Continue slots → Cancel, and successful Save → Return restore the selected Quit row and legal free
cursor. The host freezes callbacks before publishing music. On return, only this save caller rebinds
the current revision for the same live session/actor through the strict inspection query. Independent
stale refresh still retires inspection. B then returns Options → cursor → actor Movement. Successful
Continue replaces the old caller through normal mounting. Save and Quit exits only after verified
publication. Actual held confirmation cannot navigate Options or move the actor until released.

The [save owner](../application/battle-save-and-continue.md#empty-ground-options-quit-caller-1651)
and [finite native observation](../verification/battle-save.md#options-quit-caller-1651) own reproduction
and limits. Modern host Menu stays usable, including its preview refusal and pure inspection pause.
No Shift behavior, global UI redesign, new persistence authority or original timing claim is added.

## Gameplay Save and Quit windows (#1450)

Standard rigor: a bounded presentation change in the existing SaveSlotView and persistence host;
the High-rigor [save contract](../application/opening-session-lifecycle.md#agreement-and-decision)
retains query/capture, publication/backup, failure retention and explicit confirmation authority.
No save schema, supported location, transaction, engine rule, autosave or new UI framework changes.

Gameplay uses a centered, content-height window up to1040px wide with32px side clearance in native
coordinates. A translucent navy scrim preserves the paused field/battle behind it; entry continues
to use its Witch-safe right window and existing canvas scaling. Both use the accepted shared blue/gold
button normal/hover/pressed/disabled/selection treatment and compact mapped hints. Gameplay adds a
short state heading; entry keeps its accepted vertical arrangement. Slot rows wrap the complete
character name, known destination and UTC save timestamp. Field records retain their finite recipe
labels; battle-v2 records use the admitted encounter/round/actor. The displayed time is the saved UTC
timestamp, not inferred elapsed playtime. Unknown
identities say location unavailable; no current NPC/position or original-game state is inferred.
The [implemented battle host contract](../application/battle-save-and-continue.md#implemented-ordinary-host-boundary-c)
extends this shared window with Save and return / Save and Quit and fresh-process battle Continue;
its [native verification](../verification/battle-save.md) owns that bounded acceptance.

State sequence: field Save explains its safe return destination and ambient reset; battle Save
explains continuation at the same actor's settled turn. The shared flow then offers both slots
and Cancel. Slot selection opens a cancel-first save/overwrite confirmation with Save and return /
Save and Quit, and backup wording only for replacement. Successful Save and return offers Return to
game; Save and Quit exits only after successful publication. A failed write does not mark progress
clean or exit. Unsupported/busy/configuration/storage errors show a readable reason, retention
wording and **Back only**; the diagnostic code is secondary. Uncertain publication instead offers
**Inspect / Back**. Inspection reads Continue before another write or Quit is permitted; Back alone
does not clear that requirement. `RequestQuit` preserves this guard, and `PresentQuitChoices` offers
Inspect / Cancel while it is pending. Missing/damaged/incompatible slots and available backups have
readable summaries; explicit backup recovery retains Cancel first. Outside that uncertainty guard,
ordinary and unsaved Quit use the same window and Cancel-first Quit confirmation. Opening, browsing
and canceling never write a slot; no implicit retry or save.
Church Save closes its real caller and enters this same settled Save path.

Acceptance: adapter compilation, clean committed scope plan, direct document/diff checks and bounded
actual native input/state/control geometry/error observations. Pilot Save, cancel and failed write
before expanding to both slots, replacement, success, refusal, backup, Church, control return and
clean/dirty Quit; use disposable namespaces and direct file comparison. Observe1920×1080 gameplay
and the existing entry scaling transition in the same owned project, with mapped keyboard/pad/mouse.
Reuse existing local observers and tool/input selections; reports under local/issue1450, compact
semantic checkpoints only, routine output budget100MiB and bounded native runs. No aggregate suite,
screenshots, media collection, #1447 work or new verification platform. Stop expansion for repeated
observer repair without useful product evidence. Engineering checks do not establish human visual
approval or original timing/natural reach; those remain Unknown.

**Confirmed, executor native observation:** at1920×1080, the one-occupied-slot window is
(440,219,1040,641), and both occupied slots produce (440,205,1040,669). The scene remains visible
and disabled beneath a scrim with alpha below1; control returns on close. Keyboard, pad and complete
pointer clicks cover cancel-first overwrite, two saves, success, same-slot write failure with unchanged
primary/backup, and clean/dirty Quit. Explicit Quit exits without changing either saved file. Real
Church input returns through its caller before opening this same Save window; Save during Church
refuses and preserves its wait/files. Entry keeps (1216,64,640,952) logical bounds at1280×720 and
1100×800 letterboxing, then restores native gameplay coordinates. Backup Cancel/recovery and the
unavailable-primary compatibility exclusion were observed separately; Save/New Game never offer
recovery. These observations establish the named adapter boundary, not user visual acceptance.

After the [existing private entry setup](../verification/host.md#title-and-witch-entry-observation),
reproduce retained automation with `pwsh -NoProfile -File local/issue1450/run-native.ps1 -Run <fresh>
-Phase <phase> -Namespace <disposable>`. Phases `pilot`, `full` and `church` create their own initial
save through ordinary entry; `recover`, `blocked-recovery` and `slot-context` use disposable fixtures
from that successful run. Recovery uses a damaged primary plus the valid backup; the blocked case
uses a correctly enveloped version2 primary plus a version1 backup. Slot-context continues slot2
then inspects damaged slot1 through Save/New Game. The ignored handoff names exact results, fixture
preparation, retained observer/startup/navigation failures and unobserved boundaries. No storage
contract evidence is replaced, and no screenshot or original-game reach claim is made.

## Field Projection

Content validates the prepared world's raster shape/identity and required map/sprite/portrait
references before startup. `ExplorationPresentation` consumes those admitted visuals and current
layout/entity state. It does not admit a pinned asset repository, fixed comparison case or entity142
diagnostic trajectory. Logical source camera/text/motion services remain intentionally bounded.
Unknown source palette/layer/timing or final pixels remain unproved by modern node projection.

### Bound map-init foreground projection

The bound camera admits zero-autoscroll layer0/255, with unity/origin0 on the main plane and
secondary per-axis parallax128/256 and tile offsets0–63, within nonnegative signed-word positions.
[Source/behavior contract](../../../docs/design/contracts/world/map-exploration.md#bound-field-camera-lifecycle)
owns initialization, transforms, independent scroll speeds and main-plane follow. A and B logical
origins remain source state. Their modern draw origins derive from the shared presentation origin
with each plane's admitted parallax and offsets; foreground presence compares both offsets and parallax.
Ordinary actors use the area's main plane (B for0, A for255), selected before the source entity
loop; entity Layer selects signed window priority rather than coordinates. Observation exposes
each actor's actual origin. Equal unity planes still suppress duplicate foreground. Tile alpha,
clipping, priority and sprite ordering retain their existing owners. Readback compares draw tick,
logical state, modern draw origins, actual resources and geometry throughout changing plane separation.
Historical original-viewport camera readbacks retain their original scale expectations; the unified
viewport witness checks the modern mapping without rewriting those evidence records. Other layer,
autoscroll, word-wrap and raster/scroll-table profiles remain Unsupported or Unknown; this does
not establish original interrupt timing or final hardware pixels.

## Bound before-battle scene presentation

Closed-window bound scene loading admits the settled black fade/load/fade domain. It replaces the
map's logical base pair and retains black Current until actual fade service restores colors.
Map57 uses the existing map atlas/blocks and source135 sprite209/portrait31 bindings; the narrow
palette producer supplies its missing base pair in a fresh private start. The narrowly recompiled bbcs_01 row adds explicit null detach before the finite fade and one enabled common service after loading; other
start/world/party/asset/music inputs remain unchanged. Explicit scene origin selects the area's main-plane camera;
entity replacement does not re-center. Bound A/B drawing retains that origin even when the old
player is outside every new area; only unbound drawing selects legacy overlays by player location.
Actual text/portrait projection and input delivery use the
ordinary adapter. Bound physical target installation reuses this projection and the existing common
service; no player recentering or render smoothing is added. Valid invisible allocated targets remain
physical camera targets. Actual target movement precedes view/window/portrait service; idle input
presentation performs no such service. Bound white uses the finite helper projection below;
bound effects/first-input acceptance is described below. Later battle/return families remain open. [Tracking rule](../application/session-and-programs.md#bound-physical-camera-tracking).
[Behavior/limits](../application/session-and-programs.md#bound-before-battle-scene-and-windows).

### Bound white palette projection

`FullFadeWait.Color` drives the existing half-second fade delivery; logical service and the matching
actual token must both complete before script release. White resets parent Modulate to white so a
previous black endpoint cannot suppress the child ColorRect or leave fade-in black. Black clears
white alpha before delivery so an earlier white endpoint cannot cover it. Endpoints persist across
ordinary waits; reduced-flash suppresses only white opacity while retaining completion and gameplay.
The ordinary six-white/W1 boundary and authored mixed normal/reduced observations read actual nodes,
state, projection, input and errors. A newly published logical wait can precede its first projection
by one host frame; that delivery latency is not an extra logical opportunity.

The overlay covers the viewport as an accepted modern approximation. Logical PalettePair concerns
the source map palette; source-selected sprite/window colors, CRAM/VDP pixels and natural cadence
remain Unknown. Bound fade opportunities now use the existing live entity→view/window→registered
portrait service with explicit script enablement and one modern music admission; renderer duration
never supplies another service. [Behavior](../application/session-and-programs.md#bound-white-palette),
[verification](../verification/field-programs.md#bound-white-palette-observation).

### Bound generic effects and battle-loader facade

The existing mosaic/shiver renderer consumes the same effect tokens and restores engine fields on
actual completion. Bound generic presentation waits suspend logical services and actual delivery
while hidden/unfocused; the host clears debt and resumes with zero delta. The retained profile also
suspends the battle loader's modern delivery-only fades. No extra presentation clock is introduced.

Field palette helpers require an active exploration world. After initialization, retained field
Display and frozen LogicalView serve the old-scene facade without driving field services. Existing
black out → BattleLoad mount → black in remains a modern delivery-only sequence; matching completion
precedes start/first input. It does not claim original LoadBattle callback opportunities or hardware
palette pixels. Asset bindings, source null detach/post-load wait, viewport approximation and inputs
remain unchanged. [Execution](../application/session-and-programs.md#bound-before-body-and-battle-entry),
[observation](../verification/field-programs.md#bound-before-body-and-battle-entry-observation).

### Bound outcome projection lifecycle

At outcome entry the new exploration world owns a freshly initialized logical view and closed
text/portrait windows; the loader's frozen old-field projection is not reused as that authority.
Carried Display and the live party/RNG retain their existing owners. Explicit after/return windows
keep the actual outcome continuation and anchor through map initialization; confirmation requires
the existing reveal/input phase and admitted registered portrait. The existing renderer delivers
scene effects and fades. Original pixels, timing and audio fidelity remain Unknown.

The ordinary bound winning observation consumes actual movement, scene messages, automatic turns,
growth/death and after-program effects before releasing the battle facade. The result signal at the
battle→field boundary precedes installation of the returning field view: observers retain that
payload and read the new field projection after attach, rather than query the old battle view in
exploration mode. Target-browse range rejections retain accepted gameplay state while the attempted
UI candidate changes, as specified by the existing architecture owner.

### Continuous mounted-resource and audio readback

`BattleSceneView.Bind` records a resource ID on its existing Sprite2D only after successful raster
decode/size validation and texture assignment. `Observe` exposes background, backgroundWrap and
ground node-bound IDs, texture presence and actual tree visibility, alongside existing actor/weapon
IDs and completion state. A configured content ID or retained metadata on a hidden node is not a new
consumption observation. The H4 input probe records changed scene facts, distinguishing the result
signal before Present from later host polls. Completion must be actually observed; elapsed time or
the next phase does not synthesize a receipt.

The same probe uses `GameRoot.ReadAudioObservationJson` throughout an H4 session to retain every
unseen receipt from the existing 64-entry window. Sequence checks include initial overflow and the
terminal read, with receipt revision/time/wait token distinct from poll session/input/frame context.
Started, stopped, finished and fade-command remain distinct facts. Cue totals can reconcile with
terminal voices; a wait token is not a playback identity, same-cue overlap is not assigned invented
generations, and ongoing field music is not required to end at observation termination. The older
D speech subset stays separately available. Missing receipt ranges and unobserved scene edges remain
explicit. This readback changes neither the audio buffer nor scene/audio playback behavior.

See [same-session observation](../evidence/retained-comparisons.md#same-session-actual-observation) for
input freezing and session boundaries. New A observations do not fill omitted fields in old A-D02
records or prove complete original resource provenance and dependent consumer bindings.

### Bounded working-layout and draw operands

`ExplorationSessionView.ObserveLayoutRegion(x,y,width,height)` explicitly enables a local observation
of at most256 cells inside the64×64 working layout. `ReadLayoutRegion` reads authoritative row-major
words, current flags and intersecting saved roof words; it does not advance the session. The latest
successful draw carries request, session, map, revision, observation-sequence and fresh draw identity.
Readers must match those identities and rectangle, and reject missing draws or overflow.
`StopLayoutObservation` releases the observation. Disabled rendering allocates no added observation
collections.

`ExplorationPresentation.DrawLayer` records coordinate-specific use immediately after the actual
`DrawTextureRectRegion` call. Records retain selected block/tile words, texture instance and selector,
background/foreground/occlusion, pass, priority, actor-mask identity, offsets and clipped screen/source
rectangles. The cap is4096 records per draw; overflow is explicit. These are actual draw operands, not
GPU pixels or a coverage guarantee for every cell in the requested rectangle. Existing deduplicated
resource requirements are not the expected-value authority for this observation.

**Confirmed:** the private `map-layout` case in
[`engine_private_exploration_observation.gd`](../../game/probes/engine_private_exploration_observation.gd)
uses real GameRoot controlled starts and keyboard input. From Map3(4,7), DOWN opens the(4,8) door;
DOWN restores the roof, UP clears it, and DOWN restores it again. Door post-state is the pre-operation
source word at(62,0). Roof restoration uses the56 original words in(2,32),7×8, independently retained
from the selected content before roof-on-load clears them. Repeated reads/redraws preserve revision,
observation sequence and working words while requiring new draw identities. The retained local case
joins the door cell and21 visible restored roof coordinates to actual block/tile use; all56 roof
words are checked, but offscreen cells have no draw claim. Cleared roof cells remain in the viewport
and issue no texture draw. The separate flag-off controlled load at(28,24) observes both destination
cells at(28,22),1×2 and stable repeated reads.

Source operands are the first records in pinned SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, under
`disasm/data/maps/entries/map03/{4-step-events,5-roof-events,3-flag-events}.asm`:
copy(62,0)→(4,8),1×1; clear/save/restore(2,32),7×8 (source255,255 denotes clear);
and flag506 copy(23,23)→(28,22),1×2. Inspect those named records with `git show` in the pinned checkout.
The independently selected content supplies the pre-load layout words and block tile table; applying
these source operations gives expected working words and `word & 1023` gives the expected block.
This local check does not prove original natural reach, original display cadence or raster fidelity.

**Confirmed:** the flag506-on attempt fails at startup with `UnsupportedCapability: map-setup
(ms-map3-flag506)`. `MapTransfer.ValidateSetup` rejects the non-default setup before layout loading.
No variant substitution or setup bypass is admitted. **Unknown:** successful flag-on copy/draw use,
including the second flag506 copy into(57,23), and complete H4 map/layer comparison. The unsupported
alternate setup is not automatically a required gap for the selected H4 route. Applicability needs
the actual caller/load flags at every reached Map3 load joined to the source506 gate; final flags,
one controlled start and static metadata cannot establish that route-wide result. The retained small
W2 selections do not cover every Map3 load, so that binding remains **Unknown**. Providing these
operands does not close H4 item9 or the other outstanding obligations.


## Exploration Search and results

At settled opening field input, the existing Confirm binding offers Talk for a faced NPC, otherwise
visible Search. This modern shortcut invokes explicit menu Search; it does not reproduce the
original no-entity C fallback. The common story panel shows source investigation/description text
and typed acquisition, full-pack or unavailable feedback. Confirm/click reveals then continues;
Cancel/right click closes it. Field movement, Party Status and save controls stay closed while the
result owns input. The display uses the retained typed result and admitted source-ID mapping, with explicit whole-row
English fallback and untranslated-name notes. A dismissed acquisition remains acquired. Result delivery adds no gameplay/RNG
services and claims no original typewriter, item-music or menu timing. Final UI aesthetics remain
outside this behavior slice. [State and acceptance](../application/opening-session-lifecycle.md#implemented-opening-search).

## Opening Church panel

The actual priest interaction opens `ChurchServiceView` over the field. Keyboard/controller mapped
Up/Down and Confirm choose Raise, Cure, explicit unavailable Promotion, Save or Leave; mouse rows
use the existing press/release pointer guard. The source-ordered member/status offers show current
gold and exact cost before a separate confirm/decline, with explicit poison/stun removal wording.
No applicable member, insufficient funds,
refusal and success are readable results. Cancel declines the current offer or leaves the menu.
The Application typed phase/result owns offers and progression; the panel stores only focus/gesture state.
Localized text never selects the transaction phase. The independent curse-unavailable notice and
diagnostic code/field remain visible. Church reserves a minimum 192px hint footer, growing its panel from 824px with measured text
at 1080p to keep fallback/name notes and refusal text clear of the choice rows.

The panel pauses automatic field service, movement, party preparation and Search. Focus loss,
hiding or pause suspends its gesture and requires release. Service results, including real caller
continuation, use the established session-result/audio/lifecycle consumer. Save closes Church and
waits for real settled caller return before the existing slot menu opens. Replacement discards old
service/save intent. These are bounded modern controls, not original menu layout, timing, service
music, exhausted-sprite or human aesthetic parity. [Policy and acceptance](../application/opening-session-lifecycle.md#implemented-opening-church).

## Identity art acceptance (#1667)

Battle Magic/Item lists, the current Equip HUD, and exploration Magic/Use/Equip rows consume the
[bounded source identity pairs](../content/presentation-assets.md#spell-and-item-identity-art-1667).
This is the Standard resource-consumer slice; current gameplay/confirmation rules and modern
localized level/cost/equipment text stay authoritative. Public authored inputs and older private
packs without this optional namespace retain explicit text mode. Declared bundles reject missing/invalid bindings through Content; preparation rejects conflicting
source resources. A defensive runtime marker or typed decoding error preserves navigation.
Member/inventory/Give/Drop and Item operations have the separate #1668 acceptance below; shop/options art has the [#1669 acceptance](#shop-and-options-art-acceptance-1669).

**Confirmed functional/resource boundary:** ordinary title New Game exercises field Magic/Use/Equip
and their parent returns; fresh title Continue exercises battle Magic/Item/Equip and returns to the
same Movement boundary. English and zh-CN use the same publication. Chinese observations select the
actual #1602 world and entry display catalogs, including source item/spell names. Held opening Confirm
cannot submit the field child confirmation. Availability and localized explanations remain distinct
from original normal/AND-selected pixels. Equipment covers the command menu; its16×24 image is64×96
beside the existing preview text. The world remains1920×1080 with96px cells.

**Historical field subroute (pinned6c2d0982 above):** `probes/engine_identity_icons_observation.gd` extends the ordinary menu observer. Read-only projection
records resource keys, actual ImageTexture instance IDs,4×/Nearest geometry, focus/visibility and
errors. Only the explicit probe pixel observation reads native texture bytes/digests; ordinary host
state reads do not. Compare those explicit digests with the publisher's source receipt.
Its independent `IdentityIconsObservation.cs` directly composes the actual `BattleCommandMenu` with
an admitted controlled private party for multi-level/duplicate-slot coverage; that diagnostic omits
the battlefield renderer and is not ordinary campaign reach. No screenshot or active state setter is
used. Missing half-pair/full-pair, bad index/digest, legacy absence and public authored fallback are
separate direct observations, not tests of the probe itself.

**Unknown:** original natural reach, original timing/window/pixel parity, physical input-device focus,
and human/aesthetic approval. Existing and observed ObjectDB shutdown warnings retain their recorded
limits. Human visual acceptance remains #1456/#1616 and is not signed by engineering evidence.

## Member inventory art acceptance (#1668)

**Standard rigor:** extend the existing [source inventory](../content/presentation-assets.md#member-inventory-and-item-operations-1668)
to actual field/battle Member sheets, field Give/Drop/full-pack return slots and battle transaction
HUDs. Keep the existing layouts, inventory authority, available offers, localized labels/reasons and
real transactions. Field Item operations now use the original separate24×24 art at4×/Nearest:
Up Use, Left Give, Right Equip, Down Drop, then Confirm; mapped Cancel/right-click/Back returns to
the same member caller. Child Back restores the selected operation and exact item/return slot.
Only this new operation page hides the underlying Member layer, both veils and the list-page panel.
The four icons are freestanding over the visible world; a separate adjacent explanation/Back frame
leaves32px of world after the288px diamond in the deployed layout.
Existing item/transaction lists retain their separate current layouts.
The existing general Prepare list and battle auxiliary key entries remain working modern consumers;
no unavailable fifth operation is drawn as an original command.

`PartyDetailsPanel` uses shared borrowed16×24 textures in its independently scrolling magic/inventory
columns. Normal icons accompany read-only rows; pane focus, equipped/broken text, levels, slot numbers
and names remain separate. `PartyStatusView` configures one identity owner per active definition,
shared with its preparation child. A decode error stays explicit in the Member hint/observation,
retains the original read-only caller and exits through ordinary Back. Item operation decoding occurs
only on Item preparation entry; its typed error page returns to that same member and cannot reopen itself
from Member refresh. Battle Member, equipment and Give/Drop share the existing menu owner, retaining
textures through temporary field/battle reparenting and releasing on content replacement/disposal.
The battle HUD shows given/returned images separately outside the existing story panel text column;
its one immutable projection reuses a single choices query, with no cross-frame cache.

**Confirmed historical field boundary (pinned6c2d0982 above):** `probes/engine_inventory_art_observation.gd` reuses ordinary
title New Game and fresh Continue. English/zh-CN select the actual #1602 world/entry catalogs and the
same publication. Actual native state/draw reads cover Member rows, inventory scrolling, four cardinal
operation images and selected/normal pixels, unavailable Use explanation, Give/Drop slot/confirmation
Back, hidden parent and caller return. Continue covers battle Member list → selected sheet → same list,
then Options/cursor → same actor Movement, and uncommitted Give/Drop with unchanged inventory.
A local display-catalog stress variant changes only one item label to a long Chinese name; wrapped
Member/Give rows retain measured text height and image separation. It is layout evidence, not an
accepted translation or a new source item. Native projections verify the Item page's hidden common
backgrounds, separate diamond/detail rectangles and viewport-fitting explanation.

The same observer reuses `StartPreVoyageLocalizationObservation` for a declared three-member Map3
field start: actual Give fills a recipient, full-pack exchange shows all live return slots, duplicate
herbs borrow one texture and confirmation Back retains the second duplicate slot before real commit.
This is a controlled presenter boundary, not natural membership/progression. Separately,
`probes/engine_inventory_exchange_art_observation.gd` reuses #1647's actual connected Map57 approach
and two-turn Give/exchange caller, including equipped transfers, real result/next-turn suffix and
Chinese/remapped input. It proves identity textures survive the actual field-to-battle reparenting.
Public authored and old private runs retain explicit text mode. One corrupt-PNG world variant proves
typed Member/operation failures, repeated Member exit without redirect/reopen, and unchanged party
resources; its expected codec errors remain recorded. Explicit bounded pixel reads compare textures
with source receipts; ordinary Observe stays lightweight. No screenshots or full/H3 suite.

**Unknown:** original natural reach, original timing/window/pixel parity, physical device/focus behavior,
later catalog/unsupported states, broken overlays and human/aesthetic approval. Source choices do not
grant gameplay legality. Observed ObjectDB shutdown warnings remain explicit. [#1669](#shop-and-options-art-acceptance-1669) supplies shop and
empty-ground options art; #1456/#1616 retain human review without blocking this engineering boundary.

## Shop and Options art acceptance (#1669)

**Standard rigor:** the existing service/options callers now consume the
[source-owned menu families](../content/presentation-assets.md#shop-and-battlefield-menu-resources-1669).
Shop is Up Buy / Left Sell / Right Repair / Down Deals; empty ground is Up Member / Left Map /
Right Speed / Down Quit. Directions select the cardinal slot, then Confirm opens it. Attack still
opens battlefield options where previously admitted. Cancel and an independent pointer Leave/Back
restore the actual parent. Repair/Deals retain their unavailable reason and cannot mutate resources;
no unavailable image is invented beyond the original normal/selected pair.

The shared `DiamondMenuView` is the extracted Item-operation layout: four96px buttons over visible
world and a separate explanation/Back frame with32px between them. Shop hides its old common panel
and veil only on its service page; stock, member, slot, offer and result pages keep their readable
lists, gold/prices, live source slot numbers and equipped text. Stock and sale/confirmation rows
borrow semantic item textures from the shop's `IdentityIcons` owner and retain localized text.
No new UI platform, global panel revision or texture ownership layer is introduced. Existing buttons
own decoded menu textures; temporary tree exit/reparenting suspends input without releasing content.

**Confirmed bounded remake observations:** the existing controlled-start presenter admits the same
ordinary product world at Map16/F663 `(35,26)`, source-facing1 (Up), with explicitly joined opening
members. Actual Talk reaches the existing item counter and the `(39,26)` weapon counter. English and real #1602 world/entry Chinese
catalogs cover all normal/selected services, unavailable Repair/Deals, item selection, prices,
equipped sale slot0, a real sale and purchase receipt, recipient/stock/service Back, pointer Buy/Leave
and usable field return. The seven-item weapon stock also scrolls to Back, accepts pointer selection on the image and preserves resources on its actual insufficient-gold result. This controlled approach is not natural campaign reach. The existing Galam
shop probe's original ordinary-start recipe remains valid; its service Sell input is now Left.

A fresh process uses a copied real disposable Continue through ordinary title/slot input. Its actual
saved route is Map57 / battle-1, despite the older seed's Battle02 nickname. The same actor reaches
Movement B → cursor → empty-ground Confirm → Options. English and Chinese with remapped cardinal/
confirm/cancel inputs observe actual source draws, Member → full sheet → roster → Options, Map →
Options, Speed draft Cancel and Quit's shared two-slot Cancel with preserved focus. The independent
pointer Map/Back returns through cursor to the same actor/revision. Held service Confirm does not
choose a child stock item; held battlefield Confirm stays in Map. Gestures begun beneath host Menu
cannot execute after return. No gameplay setter, screenshot, GPU readback/hash or full campaign replay
is used. Normal `Observe` reads only state, resource identity and draw projection.

Source-free namespaces remain explicit text through the same controls. The representative old
private resource pack and public authored entry retain their admitted behavior. A corrupt declared
PNG reports the resource error at its own shop/options menu and permits independent exit; expected
codec errors remain recorded. Missing shop or battlefield halves reject Content before gameplay.
The source-to-publication comparison preserves every unrelated world/scene value and dependency.
The retained Item/Member/Map/Speed/Quit field-art records used their then-current cardinal inputs; their older
reports retain their historical scope and are not claimed as rerun here.

For direct native reproduction, load the selected environment/private inputs and both final pack
selections in the same process; choose fresh ignored report/settings files and an external disposable
save namespace. `$shopStart` uses the existing controlled presenter envelope with `start` Map16,
entity-0, `(35,26)` (item) or `(39,26)` (weapon), facing1, speed32 and flags0/1/2/32/33/34/663.
This sets initial admitted inputs only, not an active gameplay snapshot. Select both real #1602
world/entry display catalogs for Chinese; a language toggle alone is not that coverage.

```powershell
$env:SF2_INSPECTION_START = $shopStart
$env:SF2_SHOP_ART = '1'
$env:SF2_SHOP_CASE = 'item' # weapon selects that counter's expected stock
$env:SF2_MENU_ASSETS_EXPECT = 'original'
$env:SF2_OBSERVATION_OUTPUT = $shopReport
& $env:GODOT_BIN --headless --path remake/game --script res://probes/engine_shop_observation.gd -- --input-settings $settings
# Separate ordinary Continue over a disposable real saved turn:
$env:SF2_MENU_ASSETS_CASE = 'battle-continue'
$env:SF2_MENU_ASSETS_OUTPUT = $battleReport
& $env:GODOT_BIN --headless --path remake/game --script res://probes/engine_menu_assets_observation.gd -- --input-settings $settings
```

`SF2_MENU_REMAPPED=1` selects the finite U/K/L/J/Q/B input witness with those actual bindings in
`$settings`; defaults use arrows/Enter/Escape. `SF2_MENU_FAULT=1` requires an explicitly malformed
selected resource and checks its error/exit rather than treating it as text success. Legacy command
art can be declared separately with `SF2_COMMAND_ART=original` while new options remain text.
The existing authored diagnostic mode exercises shared command text fallback; it does not assert
private equipment admission. No prior transaction/save suite is rerun solely to update navigation.

**Unknown:** physical OS/device focus, original natural reach/cadence/pixel parity, later campaign,
full resource coverage and human/aesthetic approval. Observed ObjectDB shutdown warnings remain an
open boundary; neither #1456 nor #1616 is signed off by these engineering observations.


## Held exploration acceleration (#1619 A)

**High rigor at the accepted timing boundary; exploration consumer.** Hold Shift for focused
exploration at 1.5×; release the last acceleration binding to restore 1×. Both Shift locations and
an optional mapped pad input share a transient hold. The
[battle consumer below](#held-battle-acceleration-1619-bc) implements the other domain of
[the accepted design](modernization-design.md#held-15-acceleration-1619).

[GameInput](../../game/src/Input/GameInput.cs) and its
[acceleration partial](../../game/src/Input/GameInput.Acceleration.cs) own admission. Old format 1
settings gain Shift without changing existing bindings. The optional `accelerate` row requires a
keyboard key but may have empty pad arrays; uniqueness and the 32-event limit remain. Shift is
allowed only on that row. Matching ignores its Shift modifier only while Shift remains an
acceleration binding; other exact-match/modifier checks remain. Temporary matching copies are
disposed. Shift before/after a direction preserves the same held movement; it supplies no Move,
Confirm, choice poll or entry skip. It is excluded from `GameplayHeld` and has its own release latch.

The [existing exploration clock](../../game/src/Exploration/ExplorationSessionView.Clock.cs) receives
one scaled delta. Player motion, NPC/random walking, followers, animation and bound view service
retain their per-tick values/order. Autonomous field/story waits and modern cues use that budget
too. Ordinary and touched automatic scheduling cap at four 60 Hz opportunities/callback and discard
excess whole time debt, retaining fractional time only. Mandatory engine work stays pending.
Successful held-step continuity and real 120 ms refused retry remain; text reveal, explicit Wait and
choice input keep their real-time boundaries.

Host modals, Party/field menus, actual dialogue/choice input, Save settling, map/scene transfer,
focus/hide/pause, stopped failures and session replacement disarm and discard debt. In-flight
progress is not undone. A held key cannot restart acceleration on return; release/neutral and a
new non-echo edge are required. Pacing input is observed before host modal/release-barrier routing,
so a consumed release still clears its source. Native focus return reconciles reported device
neutrality without arming; the first fresh press works even if the host missed key-up. Menu entry
settles at 1× with its existing 120-step guard. Rate/debt are not saved.

Text cps, Speed's existing 0–3 labels/presets, suppression preferences, sample pitch/rate and music
completion are unchanged. Fade envelopes may follow visual cues; finite sound/music still requires
actual completion. Existing entry skip disarms this separate hold. EN/zh-CN Controls help describes
both exploration and battle; there is no persistent speed setting or engine/content rule change.

The [native recipe and retained failures](../verification/field-programs.md#held-exploration-rate-1619-a)
own observed scope. Matched logical opportunities/actions preserve state/RNG; equal real hold
durations intentionally advance more NPC time and can change later collisions/seeds. Native window
focus with injected mapped events is observed; human physical input, all devices, comfort, original
natural timing remain **Unknown**. Existing ObjectDB shutdown warnings remain explicit.
The sprite anchor and #1614 viewport/resize owners are unchanged by this implementation.

## Held battle acceleration (#1619 B/C)

[BattleSessionView.Clock](../../game/src/Battles/BattleSessionView.Clock.cs) owns one 60 Hz budget
for the visible active battle. Its typed boundary identity includes session, movement/scene/retreat/
round/story tokens, selected actor/stage and actual reading eligibility. Boundary or hold-generation
changes discard debt; external input discards time predating the new request. Each callback services
at most four opportunities and keeps only fractional unused time. Mandatory work remains pending.
Prepared/retired battle views neither drive the shared camera nor disarm the field owner's hold.

The parent routes movement, automatic simulation, battle-story ticks, appearance, scene animation,
mandatory HEAL steps and retreat effects through that budget. HEAL submits each required operation
in order rather than scaling an engine step count. `BattleMapViewport.AdvanceCamera` is called by
this parent only; its inner viewport advances once per opportunity. Inspection and actual reading
use real delta for camera delivery. The viewport policy and sprite anchor are unchanged.

Scene text reveal is separate real-time `BattleSceneView.Read`; actual acknowledgement eligibility,
including HEAL's timed input boundary, disarms the hold. Required autonomous HEAL work before that
boundary still runs at the selected rate. Story dialogue, Give/Discard/Search results and retreat
text retain real cps and fresh confirmation. Battle command-list/target/inspection navigation keeps
real 120 ms repeats; provisional movement uses gameplay time. Shift creates no action command.

Focus/hide/pause, inspection, host modals, stopped failure, attach/retire and field↔battle transfer
clear debt and disarm. Existing release barriers still require neutral and a fresh hold. Normal
pitch and audio completion are unchanged: EGRESS cannot close while its real cue is playing.
Speed presets/custom cps, suppression, Reduced Flash, saves and entry skip retain their own owners.
EN/zh-CN Controls help now describes the two-domain hold without adding a persistent setting.

The [native acceptance](../verification/host.md#held-battle-rate-1619-bc) confirms matched HEAL
work/RNG/resources at 30/60/120 FPS, finite story/appearance/death, real reading, focus-handler/stall
bounds and one representative private Map3→Battle02→EGRESS→usable Map3 route at both rates.
**Unknown:** all devices, physical battle OS-focus behavior, human comfort, original natural timing
and unobserved campaign combinations. Injecting focus signals exercises handlers, not OS focus.
