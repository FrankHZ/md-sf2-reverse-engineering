# Godot Host and Presentation

The implemented bounded [game localization design](../application/localization-design.md) owns English/zh-CN
catalogs, safe settings refresh, name/IME and font boundaries. Translated
display remains separate from Application's canonical text work and game-save identity.

The [local services and battle overview proposal](modernization-design.md) owns #1543's bounded
modernization choices, real service/disclosure limits, menu/return sketches and minimum acceptance.
Near-town remote services and a read-only overview are recommendations awaiting product selection,
not implemented capabilities or changes to the current agreements below.

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
| Journey / Save | Exactly the current `QuerySave` offer; unsupported controlled/authored starts and busy/non-savepoint states show a reason. The existing field Save shortcut and Church save owner remain. Save confirmation Back returns slots; slots Back returns Menu. |
| Journey / Settings | Existing `CanOpenSettings`: title/Witch or settled exploration outside other menus. No battle/story access is added. Settings/help retain draft, Apply and Back ownership; closing Settings returns Menu. |
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
At settled field input Cancel opens the [gameplay diamond](#exploration-gameplay-diamond-agreement-1588); title Cancel still opens Settings. Battle Cancel
continues to unwind gameplay, Start remains Equipment and Back remains Item. Within Menu, mapped
Menu toggles Resume; Confirm/Cancel use the existing swap. Subpages keep their own Cancel semantics.

Old preferences retain every binding. If Menu is absent, choose an unclaimed F10/F9/F8/F7 then an
unclaimed named key, and the first free signed axis in RightX+,RightX−,RightY−,RightY+,LeftX+,LeftX−,
LeftY+,LeftY− order. When all axes are assigned, implicit Menu remains keyboard/mouse accessible
and visibly explains that pad access requires editing bindings. No Guide fallback or claim that a
host delivers a system-reserved pad button is made. An explicit `menu` binding follows ordinary
keyboard+pad and duplicate validation. Effective defaults do not rewrite old preferences; Settings
Apply/Defaults retain the binding dictionary. The existing Settings UI edits preferences and displays
help, **not** binding capture: Menu remapping uses the existing JSON configuration mechanism, and
capture UI remains deferred with the other actions.

The owned implementation is GameRoot, the existing persistence/settings/window-support partials,
one `Entry/SystemMenuView`, GameInput/ControlsHelp and the two existing locale catalogs/argument
registry. No Domain/Application, progress format, savepoint legality or new navigation authority is
changed. A directly observed invalid-settings early exit now disconnects window-support signals
only if initialization actually subscribed them; this does not hide other shutdown warnings.

## Exploration gameplay diamond agreement (#1588)

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

## Basic preferences agreement

**Standard rigor; [#1451](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1451).**
Deliver the four existing `InputSettings` preferences: instant/adjustable text, 1–240 characters
per second, standard/swapped Confirm/Cancel and reduced flash. Existing blue/gold controls expose
Settings and control help directly on the title and Witch menu. At settled exploration input,
The mapped Menu action opens the [system Menu](#system-menu-agreement-1518), then Settings; field Cancel opens the gameplay diamond. Settings remains
unavailable during active text, choices, Church, party menus, movement and battle decisions. The host pauses the existing
view/audio while the modal is open; no gameplay command, session replacement, revision, RNG or game
save write belongs to this operation. Battle settings can be changed on the next safe field return.

The view owns a disposable draft. Cancel closes without publication or writes; Defaults changes
only the four draft preferences, preserving custom bindings. Apply validates the entire candidate,
including device bindings, writes it, then updates the existing settings and input objects in place.
All mounted consumers retain those objects. Reduced flash and reveal policy apply to subsequent
presentation at this safe boundary. Hint refresh and a held-input release barrier prevent the old
Confirm event, pad deflection or key echo from accepting a subsequent screen. Installing admitted
bindings explicitly releases old Godot action state. A temporary barrier tracks only the activating
Apply event until its release/neutral edge; window focus loss clears it and the settings-held action
state. Global input admission retains its existing `GameplayHeld` rule without historical key sets.
Native focus loss suspends pointer gestures; an orphan release cannot activate a control. Control help reads the admitted bindings.

`--input-settings` is the highest-priority, read-only whole-file override. Settings explains that
controlled launch, displays its values/help and offers only Close; neither the override nor ordinary
preferences are written. Otherwise load `<SF2_SAVE_ROOT>/<SF2_SAVE_NAMESPACE>/sf2-preferences/input.json`,
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
Reassess before broader acquisition or cross-engine changes. Volume/resolution/remapping remain
future presentation/input work; human visual acceptance and original timing/media remain Unknown.

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
override, and open Settings using mapped Cancel at the title. Edit all four values, choose Defaults,
then Cancel; reopen and Apply the changes. Hold the old Confirm through Apply, release, and use the
new mapping. Relaunch in the same namespace to check persistence; use a disposable malformed file,
unreadable path or blocked writer lock for the explicit refusal/recovery cases. A separate launch
with `--input-settings <path>` must show read-only values/help without editing either file. Use the
ordinary New Game route for a settled field comparison, then active dialogue/Church to check refusal.
Do not change user saves or canonical inputs to construct these disposable cases. Exact local receipts,
retained failed observations and CI/head identities belong to the Issue/PR handoff.

## Minimal contextual hints proposal (#1544)

**Standard rigor; design only.** The selected user outcome is intuitive operation with minimal
contextual instruction. This section recommends the concrete default for a later, explicitly selected
screen group; production controls and their existing agreements remain implemented as documented.
Acceptance here is a bounded source/document review and an implementable information hierarchy.
No UI change, new tutorial/hint platform, device service, Help entrance or gameplay rule is selected.
Main-story delivery under #1538 retains priority. The
[local-services/overview proposal](modernization-design.md) supplies no implemented capability.

### Current consumers and evidence boundary

**Confirmed — source structure at accepted `6020e968`:** the table names the actual hint consumers
and installed English/Chinese catalog keys. It is reproducible by reading those symbols and the
two [existing](../../game/localization/en.po) [catalogs](../../game/localization/zh_CN.po);
it adds no Godot, original-game or human observation. **Inferred:** simultaneous shortcut lists and
repeated action labels can obscure the next step. Whether the proposed reductions improve discovery
or reading comfort remains **Unknown** under [#1456](#human-opening-review-boundary-1456).

| Current consumer / source fact | Recommended default and finite exception |
| --- | --- |
| [ExplorationDialogueView.RefreshHint](../../game/src/Exploration/ExplorationDialogueView.cs): `field.controls` appends Hold Wait and Party to legal Talk/Search. | At settled field input show only `[Confirm]: Talk` / `交谈`, otherwise legal Search / `搜索`; omit an empty interaction cue. Keep the existing visible Menu entry. Wait and Party stay in Help; a first-use contextual cue may expose Party/Wait as described below. No click-to-walk or mouse Talk is implied. |
| The same consumer and [BattleStoryView.Hint](../../game/src/Battles/BattleStoryView.cs) select reveal, read-more, continue, automatic progress and eligible intro-skip hints; `story.choice-help` lists direction/Confirm/Cancel/right click. | Show the current reading step only: Reveal / `显示全文`, Read more / `阅读下文`, then Continue / `继续`. Keep focused Yes/No rows and `[Confirm]: Choose · [Cancel]: No` / `选择 · 否`; directional instruction belongs in first-use Help. Cancel means No here, never a generic Back. Retain Skip battle intro only while the real entry service admits it; automatic progress offers no false Continue. |
| [BattleCommandMenu.Refresh](../../game/src/Battles/BattleCommandMenu.cs): movement has its own hint; target-help is followed by confirm-help, repeating commit/back; the main diamond appends Equipment/Give/Drop shortcuts. | Movement: `[Confirm]: Actions` / `指令`, with the existing range/current actor. Main diamond/list: current selection plus one Choose/Back cue. Target: one action-specific Execute/Back cue, retaining selected actor, action, MP/item cost, range and disabled reason. Preserve a compact Equipment/Give/Drop access cue on the main diamond: these have no equivalent visible action rows and Help cannot open here. Do not hide their only discoverable entrance. |
| [BattleSessionView](../../game/src/Battles/BattleSessionView.cs) builds a full `_battleHelp` string as well as the diamond hint; auxiliary Give/Drop/Equipment and unread text have their own instructions. | Remove the general action catalogue from ordinary battle chrome only when its necessary current-step information is retained in the active menu/message. Keep recipient/slot/commit distinctions and any current Read more, timed Hold Wait or refusal. AI/round progress needs no command list. Existing result/reward sequence remains intact. |
| [ShopServiceView.Present/Build](../../game/src/Exploration/ShopServiceView.cs): `shop.hint` repeats Buy/unequipped/Party guidance through all phases; actual offer text already names item/member/price. | Stock/member/slot: one Choose/Back cue. Keep current gold, prices, seller/recipient and selected slot. Confirm: retain Buy/Sell terms and unequipped consequence there; result: actual paid/received outcome and Continue. Deals/Repair unavailability remains readable. This is the bounded controlled Galam consumer, not ordinary campaign shop access. |
| [ChurchServiceView.Present](../../game/src/Exploration/ChurchServiceView.cs): navigation, transient refusal, selected Save explanation and localization notes share a growing footer. | Service menu: Choose/Leave; offer: Confirm treatment/Decline with member, effect, cost and gold; result: outcome/payment and Continue. Keep unavailable Promotion/curse-removal reasons and no-applicable-member/insufficient-gold feedback. Selected Record adventure keeps the common-slot, savepoint and return explanation from #1519; it is decision content, not dispensable navigation. |
| [PartyStatusView.Present/Handle](../../game/src/Exploration/PartyStatusView.cs) combines Prepare, focus-pane navigation, scrolling, two Close bindings and pointer/wheel notes; [FieldActionView](../../game/src/Exploration/FieldActionView.cs) repeats row navigation. | Keep visible member/pane focus and `[Confirm]: Prepare · [Cancel]: Close` / `整备 · 关闭`. Retain `[Target]: Change pane` and, when its list overflows, focused Up/Down scroll guidance: pane switching has no equivalent visible button today. Prepare uses its actual selected action and Back, with resource/eligibility details beside the row. Extra Close aliases and wheel instructions move to Help. |
| [SystemMenuView](../../game/src/Entry/SystemMenuView.cs), [SaveSlotView](../../game/src/Entry/SaveSlotView.cs), [SettingsView](../../game/src/UI/SettingsView.cs) already have labeled controls plus compact mapped footers. | Use one shared Choose/Back cue per modal, without repeating the selected button's caption in another instruction bar. Preserve every disabled reason, save destination/ambient reset, overwrite/backup/quit/replacement consequence, storage failure/recovery notice and Settings draft/Apply/read-only distinction. Keep the visible Control help row and existing Menu pad-unavailable explanation. |

### Recommended information hierarchy

Use the existing owning panel and its focused control as the default source of meaning. The normal
context cue names the current legal action and adds Back/Decline or a non-obvious operation only when
needed to proceed or recover. Keep one contextual action area in the active panel; hide underlying
gameplay instructions beneath a modal. This is a placement/default rule, not a fixed line, character
or action-count budget. Costs, consequences, unread text and recovery can require more space.

Keep decision information beside the selected item, target or confirmation, above optional controls:
what changes, who is affected, what is paid/lost and why an action is unavailable. A button caption
may carry its binding directly, or a shared cue can explain Confirm/Back once; do not add both a
second caption and a duplicate footer. Existing supported focus markers and textual unavailable
reasons accompany color. Keep selected-but-disabled inspection where supported; System Menu skips
disabled rows, so its reasons must remain visible without requiring focus or hover.

Do not reduce these cases to a generic Confirm or hide them in Help:

- Paid Shop/Church offers retain exact item/treatment, member, amount, currency and applicable
  consequences, then explicit accept/decline. Opening or browsing never pays. Preserve each service's
  actual default selection and transaction authority; this proposal does not change them.
- Save/overwrite/backup recovery, New Game replacing a journey and unsaved Quit retain explicit
  consequences and their existing Cancel-first confirmations. Drop and other irreversible actions
  retain their actual selected item/target and explicit commit boundary. No new destructive shortcut.
- Invalid movement retains an actionable correction such as “Choose an unoccupied tile in the
  highlighted range” / “请选择高亮移动范围内未被占用的格子”. Failures retain the actual reason and
  available recovery; uncertain save publication still says to check Continue before retrying.
  A stopped scene retains its failure/code and existing exits, without inventing Retry or skip.

Context changes replace the old cue; refusal feedback clears on its existing valid context/focus
change, not an arbitrary timeout. Save/storage outcomes and unresolved failures remain until their
own explicit return/recovery. Preserve text reveal → read remaining pages → acknowledge, gesture
completion, focus recovery and fresh-input/release barriers across every modal/choice transition.

### Mappings, first use and reading limits

**Concrete default:** reuse `GameInput.CompactHint`: one installed effective keyboard name and one
installed pad alternative, if present, for each retained action; full alternatives use `GameInput.Hint`
in the [existing grouped Help](#grouped-controls-help-and-remapping-boundary). Thus an illustrative
standard cue is `Enter/South: Talk` / `Enter/South：交谈`; after a swap or custom configuration it
must name the effective Confirm, not a hard-coded Enter. Battle's current `KeyHint` selects only the
first keyboard name; unifying that consumer is proposed work, not already delivered pad annotation.
An absent implicit Menu pad binding must never produce a fictitious glyph.

Device switching does not change this default: the same small cue remains usable for keyboard/pad;
do not append all aliases or an entire device's controls. Automatic last-device switching is deferred,
as in #1452; no platform glyph detector or device preference is needed for this design. Use supported
textual logical key names and pad positions/axis signs; branded controller icons and physical-layout
aliases are unverified. Mouse users use visible labeled controls and complete clicks; the supported
right-click return is described in Help. Keep Cancel=No explicit on choices. No mouse-only campaign
or keyboard-free naming claim follows. Name entry keeps its keyboard-only editor, fixed Enter/Escape
and seven-character restricted ASCII alphabet; Unicode/IME names remain deferred under the
[localization owner](../application/localization-design.md).

For a selected first-use implementation, show one dismissible, context-local instruction when the
non-obvious control first becomes usable: for example hold directions to walk, or `[Equipment]:
Party` at settled field input. Keep it through the first successful relevant input or deliberate
dismissal; do not cover a choice/transaction, consume the opener as confirmation, or time out essential
reading. An ordinary per-view lifetime is sufficient; cross-session “seen” storage and a tutorial
sequence are outside scope. Until tested for that screen, retain its necessary access cue rather than
assuming a new player read Help earlier. Supplemental battle/Party cues above remain while their
actions lack equivalent visible entrances; first-use dismissal must not erase those entrances.

Detailed movement/menu shortcuts, pointer gestures, full bindings and control semantics stay in #1452's
scrollable Help with fixed Previous/Next/Back and keyboard/pad reading. The existing route is title/
Witch → Settings → Control help, or settled field → Menu → Settings → Control help. Battle/story can
open Menu, but Settings/Help is unavailable there; do not show “Help now” or remove a necessary
current-step control on that assumption. Help is currently authored in English, including its fixed
navigation text; a Chinese Settings label does not establish fully localized Help. A later screen
slice must retain necessary Chinese contextual text until any needed Help translation is delivered.
Closing Help preserves draft, origin focus and scroll and submits no gameplay command or Apply.

Chinese labels, maximum admitted names, long bindings and current English fallback notices must be measured as
shaped text in the actual allotted panel. Wrap and grow within the supported bounds; use the existing
scroll/reading-page mechanism for overflow, with reachable controls and keyboard/pad reading before
commit. Never ellipsize a price, target, overwrite warning or disabled reason to meet a hint quota,
or shrink below the owning theme's readable sizes. Preserve visible current-consumer fallback notices;
moving repeated name-policy boilerplate requires an equally discoverable existing location and the
localization owner's agreement. This proposal does not silently waive those localization contracts.
Gameplay follows the current minimum 1920×1080 client; existing scaled entry/Settings/Help 720p
observations do not admit 720p gameplay. Arbitrary font scaling, screen-reader support, all physical
controllers/layouts/DPI and human Chinese reading comfort remain **Unknown**. Existing text speed,
instant text, reduced flash and Confirm/Cancel swap retain their supported scope.

### Finite implementation and acceptance boundary

After main-story priority, recommend one pilot: settled exploration Talk/Search → one dialogue
reveal/read-more/choice → safe field return. Check discovery and recovery before selecting battle
movement/diamond/target, then service/Party and Save/Settings groups. This order is a recommendation,
not a dispatched implementation queue. No whole-interface redesign or new runtime authority follows.

Each later selected group must read actual input/state/projection/errors through its normal caller:
standard and swapped/custom bindings, pad alternative/missing Menu pad, deliberate pointer input,
held/focus/modal return, current English/Chinese and long text, unavailable actions and one relevant
failure. Paid/overwrite cases retain preview, decline, explicit commit and resource/file preservation
at the existing boundary. Verify that a first-use cue can be read/dismissed and that no necessary
operation disappears while Help is inaccessible. Reuse the affected existing observation owners;
no screenshot, new comparison framework or tests of verification tools. Human #1456 review must still
answer whether a new player finds the action, understands No/Back/commit and can read long Chinese
without a manual. Source review and green CI cannot answer those questions.

This design's checks are source/caller/catalog correspondence, Markdown targets/anchors/tables,
scope/private boundary, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual scoped CI. SDK, Godot, H3,
aggregate tests and new runtime/experience evidence are outside this documentation-only acceptance.

## Grouped controls help and remapping boundary

**Standard rigor; [#1452](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1452).**
The outcome is complete keyboard/pad reading of installed controls through the existing title/Witch
and settled-field Settings entry. A separate help view owns only reading position and pointer gestures.
Settings retains its draft, Defaults flag, message, selected Control help row and scroll position;
Back returns there without Apply, storage access or a gameplay command. Help pages cover exploration,
dialogue/choices, battle and menus, including keyboard-only naming. Page labels explain context;
a binding does not promise that its command is legal in every state. Existing visible disabled
reasons remain authoritative. Active battle/story settings access is not added.

Left/Right turn pages; Confirm also advances. Up/Down scroll the current page, including unusually
long custom bindings. Fixed Previous/Next/Back controls also support deliberate pointer gestures.
Every binding string comes from `GameInput.Hint`, after the installed Confirm/Cancel swap; pending
draft edits never masquerade as installed controls. The four existing accessibility preferences
remain the capability: instant/gradual text, text speed, Confirm/Cancel swap and reduced flash.
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
open Settings at title/Witch or settled field, stage a preference/swap/defaults change and select
Control help. Read every page via Left/Right or Confirm and Up/Down, then Back to the same draft/row.
Repeat with pad navigation and visible pointer controls, native 720p field and installed swapped/custom
bindings; pending swaps must never change the binding text. Compare session/progress/slot bytes across
field help. For capacity, use 31 unique nonconflicting keys plus one pad event on one action; then
add one key in a separate disposable namespace. The latter must show recovery/defaults, retain its
file and refuse ordinary Apply. Do not modify canonical inputs or user preferences for these cases.

### Deferred per-action remapping design

This is an implementable boundary, **not implemented UI**. A separate bounded follow-up may expose
the existing finite 16 actions: Up, Right, Down, Left, Confirm, Cancel, Attack, Spell, Target, Stay,
Item, Wait, Equipment, Give, Discard and Menu. Display context labels from this help and installed bindings;
name editing keeps its explicit LineEdit keyboard contract outside this action inventory.

- Edit one whole candidate draft using existing `InputSettings`/`ActionBinding`. Clone the bindings
  dictionary and arrays before any edit: current four-field drafts intentionally share that immutable
  data. Capture only supported `GameInput.Prepare` key/button/signed-axis identities, after the opener
  is released; modifier-only keys, unknown identities and unsupported axes are refused visibly.
  At most 32 total key/button/signed-axis events may belong to one action.
- Keep the existing global unique-identity policy, including within one action. Report both action
  names and the colliding identity; do not silently steal a binding or infer safe context overlaps.
  Each explicit action binding requires at least one keyboard binding and one pad button or signed axis;
  retain #1518's [implicit Menu migration exception](#system-menu-agreement-1518). Removing the
  last required-device binding leaves an invalid draft with a reason and disabled Apply. Capture has
  an explicit Cancel control; physical Escape or Pad Back aborts armed capture, independent of the
  action being edited. Reserved identities remain assignable through explicit named choices instead
  of capture. After any captured candidate, require release/neutral before a review screen accepts
  it with the installed Confirm or discards it with installed Cancel. Aborting changes no bindings.
- Confirm/Cancel swap is a view of the configured base pair: label base and effective bindings, store
  the existing `confirmCancel` value once and apply the swap once in `Prepare`. Conflict reporting
  uses base identities; installed help/menus always use effective ones. Use existing logical Godot Keycode names and pad positions; do not infer physical keyboard layout
  aliases or platform-specific glyphs. No automatic device/layout service.
- Validate the complete candidate with `GameInput.Prepare` before installation or writing. Reuse
  `SettingsStore.Write` atomic publication/error/conflict path, then `GameInput.Apply`; failures leave
  the previous runtime/file intact. Ordinary preferences persist and explicit launch overrides stay
  read-only. No partial InputMap installation, second store or game-save fields.
- Existing Restore defaults continues to reset only four preferences and preserve bindings. A future
  explicitly named **Reset bindings** stages removal of overrides in the candidate; Apply validates
  and publishes all settings together. Cancel discards the entire candidate without writes.
- Reuse the Apply-event release/neutral and focus-loss barriers; drain the key/button/axis used to
  finish capture before accepting another UI action. Refresh all mounted help/context hints only
  after successful install. Acceptance must cover conflict/illegal rejection, Cancel, defaults,
  restart, swap with custom pairs, held capture/apply events and write refusal with unchanged state.

Full capture/rebinding UI is deferred; this agreement adds no scaffolding. Broader device/layout and
accessibility policy requires its own consumer and acceptance decision, not an automatic continuation.

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

## Localized story controls and return notices

The [M2f consumer agreement](../application/localization-design.md#implemented-m2f-story-controls-and-return-notices-boundary)
extends the existing GameText/PO/font through ExplorationDialogueView hints, BattleStoryView hints,
StoryChoiceRow labels and FieldReturnFeedback notices. The shared panel measures the hint including
its Chinese English-fallback line; body, hint and attached choices remain disjoint at 1080p.
Both old triangle markers lack bundled coverage; `>` is used for selected Yes/No and Continue.
Stable Yes/No nodes, booleans and source-owned selection retain input authority. Reveal, ReadMore,
complete pointer release and fresh confirmation still precede choice delivery.

The exploration body/window/range and retained FieldTextWait projection are unchanged. Battle-story
Definition.Texts remains English, with a visible `unmigrated-consumer` fallback notice only while a
DialogueWait body is shown in Chinese. Camera/tick delivery and close remain unchanged. Typed actual
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

`DisplayCatalog` defaults to its original projection; only these consumers request reflow. Search,
battle and compatibility-only raw-string consumers keep their current policies. Entry is a separate
narrow compatibility boundary: the pinned `private-local-entry` v1 generator maps source `{N}` to
LF in IDs216–221. `EntryPresentationAssets` reflows those isolated LF breaks for the selected
English/Chinese row after resolution, preserving blank lines. It never normalizes arbitrary UI
strings, mutates the source/overlay or changes `ResolveEntry`'s default output. Locale Apply resolves
again in the same entry; existing Reveal/More/Continue and release guards remain.

**Confirmed:** the generator `src/sf2tool/remake_entry_content.py` projects `{N}` to LF. In the pinned
source represented by admitted entry row217 and ordinary opening row480, a break splits an unfinished
clause. This supports identifying those breaks as original layout; it does **not** prove that every
single `{N}` in the complete catalog is nonsemantic. Reflow for the selected consumers is an explicit
modern policy. Private wording and the pinned inputs remain read-only. **Unknown:** semantic intent
of every unreviewed source break and human approval of the resulting reading experience (#1456).

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
adds title/Witch controls, New Game/restricted-name help, slots, Save/Quit/Continue and recovery.
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

## Presentation Policy and Current Layout

The common design baseline is 1920×1080 (16:9). One map cell and ordinary 24×24 sprite
frame occupy 96×96 output pixels: both consumers use nearest sampling at four output pixels
per source pixel. The world fills the viewport (480×270 source pixels); UI overlays never resize
or zoom it. The [minimum-window policy](#window-support-and-recovery-1455) also admits larger native
clients, including 4K fullscreen at that fixed scale; sub-1080p gameplay and free zoom remain outside
the bounded world-layout work. The shared visual/story treatment above supplies the current finite Theme consumers.

### Unified world viewport agreement

Standard rigor: this changes Godot presentation only. Application retains entity coordinates,
logical camera axes, collision, scripts, clocks and completion tokens. `WorldViewportLayout` owns
only disposable source-pixel camera geometry, shared by field and battle through `GameRoot`.
It does not store gameplay state. Both authored and private consumers use the same cell scale.

Field coordinates are sixteenth-source-pixel motion values divided by16. A battle cell is mapped
to the source map by adding `SourceMapRegion.Origin`, then multiplying by24. Sprite top-left and
foot alignment stay on that same transform. Battle terrain uses the inverse region translation.
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

**Standard rigor; selected implementation under [#1455](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1455).**
Gameplay requires actual native client width ≥1920 and height ≥1080. Larger windows, maximize and
native fullscreen including 4K remain admitted at 96px cells/ordinary frames and 4× nearest sampling.
The world extent expands through the existing layout; UI never shrinks the world. Entry retains its
logical 1080p CanvasItems canvas; gameplay views retain Disabled scaling/native coordinates.
Existing High save/recovery guarantees and the four Settings preferences remain unchanged.

| Boundary | Current behavior / acceptance limit |
| --- | --- |
| Native gameplay client at least 1920×1080 | Admitted, including larger aspect ratios/windows and native 4K fullscreen. Bounded 1080p field/menus/battle/return, 2560×1440 edge placement and 4K field/battle-menu observations do not certify every screen/state/device. |
| Either gameplay client dimension below minimum | Suspend processing/audio and show the responsive recovery shell. Restore window, Use fullscreen and Quit are the finite actions; mode requests must satisfy actual client readback before gameplay resumes. 720p gameplay is not supported. |
| Entry/slot/name forms at smaller native sizes | Keep the existing logical canvas and pending form. Final New Game publication, Continue mount and explicit backup recovery recheck native admission before writing/replacing progress. Controlled diagnostic starts wait behind recovery rather than mounting below minimum. |
| Placement and mode selection | Work area/decorations choose a fitting centered window or qualifying native fullscreen only at startup/explicit Restore. A valid-sized window dragged partly offscreen/across screens never pauses or recenters. No position polling, display-mode changes or persistent display preference. |
| Smaller physical desktop / mixed DPI or monitors | Startup can fit an entry shell when neither a baseline decorated window nor native fullscreen meets minimum. Actual 1080p physical-display fit, smaller-screen/DPI/multi-monitor experience remain **Unknown** for later human checks; no hardware pass is inferred from 4K observations. |

[WindowSupportHost](../../game/src/Entry/WindowSupportHost.cs) owns native client admission and the
finite resume decision alongside [GameRoot](../../game/src/GameRoot.cs). At startup an already
valid window is centered when its outer dimensions fit; prefer qualifying ordinary fullscreen when
a baseline decorated window cannot fit, including an OS-clamped startup client. Otherwise choose a
fitting baseline window when needed. If neither fits, retain a smaller entry shell instead of forcing
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

The chosen minimum policy reuses native expansion and existing overlay anchors. Exactly 1080p-only
would add equality/resize restrictions and reject usable 4K fullscreen without a correctness need.
UI-only adaptation below 1080p would instead require separate Party/Church/dialogue/battle/host reflow,
font/card/portrait choices and exactly one inverse pointer transform. Root CanvasItems scaling would
also change world pixels. That wider scope remains deferred; no new Settings row or UI-scale promise.

Use the [finite host recipe/results](../verification/host.md#window-boundary-design-observation-1455):
adapter build, clean committed dependency plan, direct source/docs/private-boundary checks and actual
native state/input/geometry/errors. No engine/full suites, helper tests, screenshots, full route or
new collection platform. Physical off-focus release, actual 1080p display/DPI and human readability
remain **Unknown** with the later [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456)
checklist. Existing failures remain intact; the
[#1447 investigation](../application/opening-session-lifecycle.md#title-lifecycle-investigation-1447)
adds no physical-focus acceptance or production change.

### Battle movement range agreement

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
ground at Z0; actor sprites/labels remain Z1; original foreground walls remain Z2; preview/path/target
marks remain Z3. Preserving foreground occlusion is part of acceptance. No new art, zoom, camera or
menu geometry is introduced. Source-original range pixels, animation and hardware cadence remain
**Unknown**; this is an accepted modern affordance, not an original presentation-parity claim.

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

The ordinary host now implements a first playable iteration through `BattleCommandMenu` and
`BattleMenuLayout`, using a shared selection-name window and the original 24×24 normal/selected
command rasters from a local private scene input. Icons use nearest sampling at integral scale;
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
window (#1446), with seven-character validation, visible admitted alphabet/default/error help and
retained draft/Back semantics. The controller alphabet grid and its key-theme variants are removed.
The existing entry editor-focus style and mouse Back/Use name buttons remain.

One compact contextual line shows the primary keyboard binding and one mapped pad alternative for
the currently available actions (for example `Enter/South: Choose • Escape/East: Back`);
full alternatives remain in existing binding help/tooltips. It
reads the installed mapping after Confirm/Cancel swap. No device detection or duplicated key table.
Errors retain their diagnostic and safe retry/back actions; long slot labels wrap within the window.
The name editor always identifies fixed Enter/Escape editing semantics and keyboard-only input;
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
character name, known savepoint destination and UTC save timestamp. Only the four already supported
recipe identities receive readable destination labels; unknown identities say location unavailable.
No current NPC/position, elapsed playtime or original-game state is inferred from a slot.

State sequence: Save explains the safe return destination and ambient reset, then offers both slots,
Cancel and Quit. Slot selection opens a cancel-first save/overwrite confirmation, with backup wording
only for replacement. Success offers Return to game. Unsupported/busy/configuration/storage errors
have a readable reason, explicit file-retention wording, Back and Quit; the existing diagnostic code
is secondary. Uncertain publication remains distinct and directs the player to check Continue before
retrying. Missing/damaged/incompatible slots and available backups have readable summaries; explicit
backup recovery retains Cancel first. Ordinary and unsaved Quit share the same window and explicit
Cancel-first Quit action. Opening, browsing and canceling never write a slot; no implicit retry or save.
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
