# Change controls in Settings

**Implemented for [#1774](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1774).**
Open Settings at the Witch menu or from a settled field's system Menu. Display & text, Gameplay
aids, Controls and Debug & diagnostics share a draft. Controls → Change controls edits keyboard
and controller alternatives; installed navigation remains active until top-level Apply. Back
returns to the source row, and Back at the top discards the visit. The [E2 feature row](../evidence/opening-ui-gaps.md#e2-settings)
tracks the player outcome. The [host handoff](ui-architecture.md#one-event-from-device-to-page-and-back) owns the High-rigor native input boundary.

Focus loss cancels held movement, acceleration, repeat and pointer gestures. The host drains device
edges received in the background before gameplay or UI dispatch. Returning to the window reconciles
neutral input and discards elapsed pacing debt. The [background preference](host-ui.md#run-in-background)
controls automatic time and audio. The explicitly enabled [debug endpoint](../verification/debug-port.md)
routes one semantic action to the current controller while the window is unfocused. A debug
action has no physical key, mapping or held/repeat lifetime.

## Current production boundary

**Confirmed** by reading the following production owners. The existing [production controls audit](ui-scene-review.md#production-controls-audit) separately records ordinary input with already installed settings, including the Ribble field and controlled Galam shop and Battle01. Those runs used defaults or preconfigured mappings.

| Owner | Behavior the editor must preserve |
| --- | --- |
| [GameInput.Prepare, Apply and Resolve](../../game/src/Input/GameInput.cs) | Format 1 has **17** actions. Directional actions are Up, Right, Down and Left. Confirm and Cancel form the swappable pair. Attack, Spell, Target, Stay, Item, Wait, Equipment, Give, Discard, Menu and Accelerate complete the set. Sparse, lowercase `bindings` entries override whole default rows. Prepare expands them and swaps the base pair once. Apply publishes to the existing settings object, releases old Godot actions, installs the prepared events and signals hint consumers. |
| [Movement and action edges](../../game/src/Input/GameInput.cs) and [acceleration](../../game/src/Input/GameInput.Acceleration.cs) | Direction holds drive movement/navigation where admitted. Confirm/Cancel and shortcuts use fresh edges. Wait has its own held/release gate. Accelerate is a separate hold, default Shift (either location), optional pad, with the applied [Hold speed](host-ui.md#implemented-composition-and-return) during eligible exploration and battle and 1× on last release. It never submits a choice. Shift normalization applies only while Shift is bound to Accelerate. Wait and Accelerate are excluded from the general `GameplayHeld` guard. |
| [InputSettings.Draft](../../game/src/Input/InputSettings.cs) and [SettingsView](../../game/src/UI/SettingsView.cs) | Drafts own a copy of their sparse bindings and arrays. The fixed footer resets the current category, or all preferences and bindings from the root, in the draft; see the reset scope below. Battle Speed edits its own two preferences. Shared Settings also opens from the battle system Menu and offers grouped categories, installed Help, [capture and review](../../game/src/UI/SettingsView.Bindings.cs), Apply and Cancel. |
| [SettingsHost](../../game/src/Entry/SettingsHost.cs) and [SettingsStore](../../game/src/Input/SettingsStore.cs) | The host prepares input, language and applicable field graphics before writing. Ordinary settings use `<SF2_SAVE_ROOT>/<SF2_SAVE_NAMESPACE>/sf2-preferences/input.json`. The store takes an exclusive lock, compares loaded bytes, flushes a same-directory temporary and atomically moves/replaces the file. Explicit `--input-settings` selects a read-only whole file. Invalid overrides fail startup. Missing ordinary preferences use defaults without creating a file on read. |
| [Hint projection](../../game/src/Input/GameInput.Hints.cs) and [grouped Help](../../game/src/UI/ControlsHelpView.cs) | Both device families can operate at once. CompactHint selects the first installed alternative in the active family (pad buttons precede axes), or explicitly names the keyboard fallback. Help lists every installed alternative after the swap and preserves its page and scroll on hint refresh. Mouse shares keyboard hints and existing pointer gestures. Its clicks, wheel and motion have no entries in the bindings schema. Name editing retains its own keyboard controls. |

Mapped navigation and pointer picking use the same mounted production rows. The host sends
the actual Confirm key, button or axis synchronously to capture or Apply. A GUI activation
arrives on mouse release. The visible Capture Back button and right-click return through
the page's normal pointer route. Unmapped physical keys, including a retired Enter, cannot
fall through to Godot’s default GUI actions while Settings is open.
[Native Settings observation](../verification/host.md#native-settings-focus-and-gui-picking)

## Editing a binding

1. **Choose an action and device.** Open **Change controls** inside general Settings. Show localized action meaning, installed controls and pending changes. For Confirm/Cancel, label the base rows and show which effective action each will operate under the draft swap. The selected editing device stays selected when the latest input changes the hint family. Use installed navigation, Confirm, Cancel and deliberate pointer controls throughout. A read-only launch keeps Help and Close available. Battle Options → Speed keeps its limited caller.
2. **Choose the alternative.** List all keys, buttons and signed axes in their stored order. Replace a selected entry, append an alternative, or remove the selected entry. Preserve other entries and untouched sparse overrides. Replacing in the same array keeps the slot. Changing a pad button to an axis or vice versa appends to the destination array. Show the resulting first compact hint before acceptance. Retain every valid alternative on reopening and on restart.
3. **Capture once.** With Settings owning the modal, the host routes raw events to the capture child before ordinary Settings navigation or action resolution. Consume the opener through release/neutral (including the opening click), then accept a fresh non-echo key press or a fresh pad button/axis from the chosen family. A stick must start neutral (absolute value below 0.35), then reach the mapped deflection threshold (0.5); latch its device, axis and sign and wait for clear neutral before reuse. Consume unrelated input, duplicate presses, mouse gestures and releases in the modal. Keep the existing focus/window recovery and pacing bookkeeping, with acceleration disarmed. The [host input order](../../game/src/GameRoot.cs) observes acceleration before routing. Consuming a release must still clear a held source.
4. **Review or abort.** Physical Escape, pad Back and a visible Cancel control abort armed capture. Offer named Escape and pad Back choices so these valid bindings remain assignable. Invalid candidates show the offending identity and reason. Drain rejected and aborting inputs too, so holding Escape cannot back out of the parent screen. After capture, drain the captured event through actual release/neutral before installed Confirm accepts the proposal or installed Cancel discards it. A complete pointer gesture can do either. Focus loss or capture-device disconnect abandons the in-flight proposal and requires fresh input on return. Window close unwinds Settings before the existing Quit prompt. No capture edge can also move, accelerate, open a menu, acknowledge text, choose a target or confirm its own review.
5. **Return to Settings.** Accepting review changes only the draft. Back from the list returns to the same Settings row and scroll position with edits staged. Back/Cancel from Settings abandons the whole draft. Help always describes installed actions. Draft review alone describes pending ones. Installed bindings, including the current effective Cancel, remain usable to leave even an incomplete draft.

### Admission and format 1 compatibility

Use the existing `Prepare` rules for both candidate preview and final admission. Expose the resolved **base** rows before the swap from the same default expansion used by Prepare. Editing a default starts from that entire row. Store only explicit row overrides and the single `confirmCancel` value. Never serialize the swapped InputMap back as base rows. Detach the draft dictionary and clone all three arrays of each edited row before mutation. A category reset retains every unrelated draft value; Controls includes the convention and all bindings.

- A row has `keys`, `buttons` and `axes` arrays and at most 32 events total. Keys use canonical logical Godot Keycode names. None, Ctrl, Alt, Meta and chords are ineligible. Shift alone is valid only for Accelerate. Ignore echoes and refuse unsupported captures with feedback. Pad choices are the four face positions, four D-pad directions, shoulders, Back/Start and stick clicks, plus either sign of LeftX, LeftY, RightX or RightY. The installed pad mappings apply to any device. Trigger axes, mouse rebinding, physical keyboard aliases and OS/controller profiles are outside this editor.
- Every row needs at least one key. Every explicit row except Accelerate also needs a button or signed axis. Missing Accelerate inherits Shift with empty pad arrays. Missing Menu now prefers Escape/Start with the contextual admission and compatibility fallback described below. An exhausted implicit Menu may remain keyboard-only. Keep inherited rows sparse on unrelated saves. Editing Menu creates an explicit row; preview changes to inherited Menu or Equipment before accepting it.
- Reject repeated identities within a row and across actions, except Menu with **effective Cancel after the swap**. That pair has distinct base/child routing. Cancel must retain at least one keyboard and one pad alternative distinct from Menu so the field action menu and battle inspection remain reachable. Confirm, movement, Equipment and other actions cannot share Menu. The capture editor and Prepare use the same pair rule; final Prepare also checks reachability. Report identity and owning action(s). A removal may temporarily leave a row incomplete; keep installed Back usable, and refuse Apply until repaired. Refuse a 33rd event without dropping alternatives.
- **Reset this page** stages only the category listed below. **Reset all settings** on the root includes bindings. Neither writes. Only root Reset all settings → Apply authorizes corrupt-file recovery and retains the invalid bytes in the existing backup; a page reset cannot authorize recovery.

## System Menu defaults and input precedence (#2287)

**High rigor**, retaining the [native input agreement](ui-architecture.md#one-event-from-device-to-page-and-back):
one overlapping binding must not execute two player actions across a page transition. The scope is
host/adapter routing, preference expansion and installed hints. No engine rules, save legality,
content or new input framework change. Acceptance is adapter compilation plus bounded ordinary
keyboard/pad input, migration and live remap checks below. Hardware/OS delivery remains Unknown.

Fresh preferences and old format-1 files with no explicit rows inherit Menu **Escape / Start**.
Cancel remains Escape/X and pad East; the field Adventure entry and its Help row display only
the non-Menu alternatives (normally X / East). Equipment keeps E and inherits the first free
signed stick axis (normally RightX+) when an inherited or explicit Menu needs Start. An explicit
old Menu that does not use Start keeps the inherited Equipment Start. Every explicit row is retained,
including an explicitly stored old F10/RightX+ Menu or Start Equipment. No file is rewritten on load,
and no heuristic guesses whether a stored row was once a default or a deliberate choice.

For absent Menu, Escape may share effective Cancel only when that row has another keyboard key.
If Escape belongs to Confirm (including swapped defaults), movement or another action, choose the
first unused F10/F9/F8/F7, then a canonical key. If Start is already assigned, choose the first free
signed axis in RightX+, RightX−, RightY−, RightY+, LeftX+, LeftX−, LeftY+, LeftY− order. With no
axis free, implicit Menu is keyboard-only and the existing no-pad notice applies. If Equipment
cannot vacate Start because all axes are occupied, retain its Start and apply the same Menu fallback.
Unrelated Apply preserves sparse inheritance. Reset stages current defaults; only Apply writes.

| Active input owner | Mapped Menu | Cancel-only input |
| --- | --- | --- |
| Ordinary field movement, no child | Opens system Menu through the existing field settlement path | Opens field action menu |
| Field action/Member/Prepare or church/shop/Caravan page | Returns through that page's Cancel operation | Same page-owned Cancel |
| Field text/choice, automatic story or battle presentation | No Menu-only operation; a shared Cancel identity retains its original text/choice semantics | Original reveal/choice/skip semantics; never opens system Menu |
| Battle player Movement, no movement delivery or inspection | Opens system Menu without moving or undoing the preview | Existing undo/inspection entry |
| Battle ActionChoice, item/magic list, target or other selection | Returns through the active selection's Cancel; target → list → action root → Movement | Same selection-owned Cancel |
| Battle Inspection and details | Returns one inspection layer | Same inspection-owned Cancel |
| System Menu, slots, replacement entry, Settings and its children | Returns the active host page; root Menu closes | Existing Cancel, including swap |

The top-right pointer button retains its existing cover/admission behavior, including viewing
unavailable actions during story or a battle decision. Keyboard/pad Menu follows the table; it
does not bypass a child to cover it. Naming and binding capture retain their dedicated input owners.
There is no raw global Escape shortcut. A retired Escape stops operating Menu; if it remains Cancel,
it continues that action. Direction holds, Wait, Accelerate, the host Apply drain, field/inspection
release barriers and focus handling remain on their existing owners. Resolve chooses one action
after the physical edge guard; a close cannot resolve again against the newly exposed base.

### Bounded ordinary-input observation

[`system_menu_input_observation.gd`](../../game/probes/system_menu_input_observation.gd) mounts Main
with a disposable `issue2287-*` namespace and no screenshots. **Confirmed:** fresh and sparse old
defaults, explicit old Menu, custom Menu/Cancel, swapped defaults and occupied Escape/Start load
with matching hints and byte-identical preferences. Field Menu opens/closes with Escape/Start;
Settings returns one layer while held. The default case edits Menu to F6 through capture/review/Apply,
checks persisted explicit bindings and the now Cancel-only Escape. Synthetic focus signals retain
the held opener until release. Connected Battle01 uses the offered Medical Herb target without
committing it, returns through Items/Main/Movement and inspection, then admits a fresh host opener.
NPC text consumes its own Cancel while Menu-only Start neither covers nor acknowledges it.
A Galam shop child returns through service Cancel with held Escape and Start, never opening host Menu.

Use the shared SDK/Godot, worktree-local build/import state and package selections from
[host sample setup](host-ui.md#sample-setup-and-reset), selecting accepted `derived/issue2293/flame-v1`
for world/scenes and the unchanged opening2252/witch2255/catalog2266 packages. Build Debug with
`UseSharedCompilation=false` / `nodeReuse:false`; `uv run sf2 verify adapter` is the compilation gate.
Create a fresh external disposable namespace for each case. Set `SF2_HOST_UI_OUTPUT` to an ignored
absolute JSON path, `SF2_HOST_UI_CASE` to the case below, `SF2_HOST_UI_START` to the initial file,
and leave `SF2_HOST_UI_STILLS` empty. Run from the repository root:

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --path remake/game `
    --script res://probes/system_menu_input_observation.gd --log-file "$run/godot.log"
```

Use the [in-game Load initial-field recipe](host-ui.md#in-game-load-observation-2286) at Map3 (32,13),
or its Map57 Battle01 start for `battle`, and Map3 (28,13) for `story`. For `service`, use the
[accepted Galam start](field-and-shop.md#verification-boundary-and-remaining-gaps): Map16 (35,26),
facing1, speed32, flags0/1/2/32/33/34/663. These are initial detached
admissions, never live state edits. Except `default`, create a format-1 preferences file with `language`.

| Case | Additional starting preferences |
| --- | --- |
| `default` | No file; includes live F6 remap and synthetic focus-signal check |
| `old-default`, `battle`, `story`, `service` | No explicit bindings |
| `legacy` | `menu: {keys:[F10], buttons:[], axes:[RightX+]}` |
| `custom` | Menu F6/Start; Cancel Q,X/East |
| `swapped` | `confirmCancel: swapped` |
| `occupied` | Confirm Escape,Enter/South; Cancel Q,X/East; Equipment E/Start |

Each binding row uses JSON string arrays for `keys`, `buttons`, `axes` (empty when unused).
Each process has a 55-second deadline and retains only selected state/logs; budget under 64 MiB,
serial launches and no campaign sweep. Retain failed attempts: an observer Variant type annotation,
wrong Settings row ID, mistaken Stay-target assumption, first actor without Magic, and an omitted
Item-list return layer, and a shop initial facing0 instead of the accepted facing1. These were observer
corrections, not gameplay exceptions. The first import
used a relative log destination rejected by Godot; the corrected import uses an absolute local log.
Existing ObjectDB shutdown warnings remain #1716. Headless input is not physical-controller,
OS-focus or human visual acceptance. Older F10-based host probes retain historical results but
are not current default-input acceptance: an explicit legacy Menu row restores that binding, while
checks that intentionally cover text/ActionChoice must use the existing pointer entry.

## Apply, persistence and live feedback

Submit the complete candidate through the existing SettingsHost Apply path. Prepare input, localization and graphics, then call SettingsStore.Write. Publish into the existing GameInput and refresh mounted consumers only after that succeeds. A validation or write failure leaves the editor/draft available with its reason and preserves the installed map, effective hints and existing file. External changes since load require relaunch. An unreadable file or unsafe/unconfigured store cannot be overwritten. A malformed ordinary file uses defaults pending explicit recovery, which retains the old bytes in a uniquely named invalid backup. Apply never touches journey slots. Hold the activating key/button or device axis behind the existing Apply release barrier, disarm movement/Wait/Accelerate and require fresh input at the caller.

| Player surface | Refresh and observable result |
| --- | --- |
| [Field status bar](../../game/src/Exploration/ExplorationDialogueView.cs) | At eligible field input, show installed Accelerate beside the current Talk/Search cue and keep the Adventure target inside the frame. Replacing Shift with Q removes Shift from the effective row. Holding Q yields 1.5× and releasing it restores 1×. Adding RightY− shows the signed stick on pad and admits the same hold. Without a pad alternative show the actual keyboard fallback. Dialogue, menus and disallowed battle waits retain their pacing gates. |
| [Shop](../../game/src/Exploration/ShopServiceView.cs) | Present recomputes the current direction/Confirm/Cancel hints as the player browses services, stock, recipient, sell slot, quote or result. Changing family while a quote is open refreshes the cues with the same offer, selection, gold and inventory. Only an intentional mapped action submits the choice. |
| [Battle](../../game/src/Battles/BattleSessionView.cs) | The host's HintsChanged refresh updates cached decision, inspection and auxiliary cues. Changed direction/Confirm/Cancel operate the existing command or target. Hint refresh itself preserves actor, target, selection, turn and resources. Read the installed mapping on battle entry even when Apply occurred in the field. |
| [Settings, Help and entry](../../game/src/Entry/SettingsHost.cs) | The host refreshes mounted views on Apply or family/layout change. Help shows all installed alternatives and uses current-family compact navigation. Preserve page, scroll and draft focus across family changes. Use the [existing device policy](presentation.md#mappings-first-use-and-reading-limits) for keyboard/mouse, recognized controller labels, generic positions and active-pad disconnect fallback. A device change alone never dispatches a second action. |

Keep operation hints at the existing fixed **24px** native role in English and 简体中文. Wrap or allocate space for long labels and keyboard fallbacks. Preserve the field footer hit region and shop reading/transaction roles. [Field and shop layout](field-and-shop.md#reading-input-and-return)

## Implementation order and acceptance

1. **Prove one Settings transaction.** Extend the existing draft/base-row preparation, then add Change controls → Accelerate → Keyboard → replace Shift with Q → review. At the Witch menu and a settled field, read the installed Shift in Help while Q is staged. Cancel, compare the preference file bytes (or continued absence), then use the old Shift hold in the field. Repeat and Apply, operate the new hold, release it and try Shift. Relaunch the same disposable namespace and repeat using persisted Q without an override.
2. **Complete the editor in that transaction.** Exercise installed keyboard and pad navigation plus pointer Cancel, opener/capture/review/Apply held through transitions, focus loss, disconnect, signed-axis chatter and reserved Escape/Back. Check multi-key/button/axis rows, replace/add/remove order, same- and cross-row duplicates, missing required family, 32 accepted/33 refused and Shift on an ineligible row. Use a custom base Confirm/Cancel pair, stage swap, verify effective Back before and after Apply and after restart. Load an older sparse file with occupied Menu defaults and all stick directions claimed, save an unrelated edit and verify its implicit keyboard-only Menu survives. Respect keyboard-only Accelerate. Exercise page and whole-settings resets with staged bindings and unrelated preferences.
3. **Verify refusal and return in ordinary hosts.** Use disposable storage for invalid-file recovery, locked/unwritable storage and an external edit since load. Compare bytes, live behavior and draft status before/after refused Apply. Confirm both ordinary file and override stay untouched during a read-only launch. Check session identity, revision, RNG, progress and slot bytes across field Settings, Help, cancel and Apply. Stage held Wait/direction/Accelerate inputs on the covered field and verify fresh-edge return. This field observation leaves gameplay slots untouched. The [battle observation](../verification/host.md#battle-system-settings-observation) covers the same release barrier over a retained target and a real action message.
4. **Follow the installed map to its consumers.** Repeat the [field/merchant production route](ui-scene-review.md#reproduce-the-field-controls) with actual editor Apply and ordinary inputs at the Ribble counter. Check the controlled Galam shop phases and a controlled Battle01 decision/target/Back. In battle, hold the replacement Accelerate during eligible simulation, then reach a reading boundary and verify 1× and a fresh acknowledgement. In Help, field, shop and battle, switch keyboard → pad → mouse with an otherwise inert input and verify labels/geometry and retained state, then operate the displayed binding. Cover EN/zh-CN, long labels at native 1080p and the existing 720p Settings reader. Check fixed font size, wrapping, control bounds and live errors. A controlled start establishes the named seam. Physical-device legends and human readability remain **Unknown** pending [player review](../verification/host.md#later-physical-focus-human-checklist-1456).

## Reproduce the installed controls

Build the Debug adapter after loading the [registered environment](../../../docs/operations/local-private-inputs.md).
Use an intact title set, compatible private world/scene/catalogs, and a fresh external
`issue1774-*` namespace with a copy of Ribble F708 in slot 1. Keep logs and reports in a fresh
ignored directory. Controls are injected events on a running Godot host; a focus signal or
synthetic pad disconnect cannot certify physical hardware.

```powershell
$env:SF2_HOST_UI_CASE = 'pilot'
$env:SF2_HOST_UI_OUTPUT = Join-Path $freshRun 'result.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
  --path remake/game --script res://probes/settings_remap_observation.gd --log-file (Join-Path $freshRun 'godot.log')
```

Start with instant text, standard controls and no binding overrides. Repeat with `restart`
in a fresh process on the same namespace. The [pilot](../../game/probes/settings_remap_observation.gd)
checks staged Q against installed Shift in Help; Witch Cancel retains preference bytes. From
Ribble, Apply preserves session/revision/progress, advertises Q at 24px, changes its hold to
1.5× and restores 1× on release. Shift no longer accelerates, including after relaunch.

Use [the editor observer](../../game/probes/settings_binding_edges_observation.gd) with the same
command and different script/case. Each case starts in a separate disposable namespace unless
it verifies a restart.

| Case | Initial condition and observed boundary |
| --- | --- |
| `all`, `edges`, then `swap-restart` | Standard defaults for the first two; restart on the `edges` namespace. Edit all 17 action rows; replace/add/remove alternatives, reject conflicts/invalid Shift, repair a missing family, exercise category and whole-settings resets. Held capture and signed-axis chatter cannot confirm review. Focus loss, disconnect, pointer Cancel and physical Escape/Back discard the proposal. Custom base Confirm/Cancel and staged swap operate after Apply and restart. |
| `32`, `compat` | Give Accelerate 31 unique function keys (F1–F31); the 32nd installs, the 33rd is refused. Separately give it Shift/F10/F9/F8/F7 and RightY−/RightX−/RightX+ with no explicit Menu. An unrelated edit preserves the keyboard-only implicit Menu. |
| `override`, `recovery`, `locked`, `external` | Supply a separate read-only startup override; malformed ordinary bytes; a disposable input.lock held exclusively; or allow the observer to edit the disposable file after load. Compare bytes and installed/draft state. Controls reset cannot authorize recovery; explicit root Reset all settings + Apply retains a byte-identical invalid backup. |
| `consumer` | The editor installs Accelerate Q and RightY−, Confirm F5, Cancel F6 and Down J. Help family changes preserve page/scroll. Reuse these ordinary preferences for the consumer runs below. |
| `reader` | At Witch Settings, resize to 1280×720 with an elongated draft label. Check the independent details scroll, fixed actions, 24px hint, wrapping, Back and Help. Gameplay minimum size belongs to the window owner. |

Use the edited consumer namespace for [ordinary Ribble Continue and counter](../../game/probes/settings_counter_observation.gd),
the [controlled Galam shop](../../game/probes/engine_field_service_ui_observation.gd) (`SF2_FIELD_MENU_CASE=shop`,
`SF2_FIELD_UI_REMAP=1`, merchant approach as `SF2_INSPECTION_START`), and the
[Battle01 approach](../../game/probes/engine_compact_commands_observation.gd) (`SF2_COMPACT_CASE=battle`,
`SF2_COMPACT_START`). The latter two use their existing output variables `SF2_FIELD_MENU_OUTPUT`
and `SF2_VIEWPORT_OUTPUT`. At the quote and battle command list, inert pad/mouse changes refresh
the cue without spending an item, moving selection or drawing RNG. Use [the battle hold observer](../../game/probes/settings_battle_hold_observation.gd)
with `-- --authored-package <absolute stone-court.json>` to follow edited Q through provisional
movement and a real attack message: reading stays at 1× until fresh mapped Confirm acknowledges it.
These observations establish adapter behavior, not natural campaign reach or human readability.

## Reset footer (#2290)

The existing High-rigor host input boundary remains in force: this bounded change alters only
Settings draft interaction and its fixed footer. No new action, binding format, UI framework,
gameplay reset or save-slot operation is introduced. Acceptance is page scope, unchanged bytes
on Cancel, correct Apply/restart, installed remapped hints, and child/read-only input ownership.

The footer is outside `SettingsReading`. Root offers **Reset all settings** beside Apply;
category pages offer **Reset this page**. Click, focus + Confirm, or use the displayed shortcut.
The context reuses **Magic / Spell** (default H / north face button), which is otherwise idle
on Settings categories. Remap that action in Controls → Change controls. The footer follows
`CompactHint` for the active family and installed map, including while reset or remapping is
still staged. Draft bindings never take over navigation before Apply.

| Page | Reset draft scope |
| --- | --- |
| Root | Every preference plus all custom bindings; explicit invalid-file recovery authorization |
| Display & text | Language, graphics, text mode and characters per second |
| Gameplay aids | Reduced flash, modern guidance and hold-speed multiplier |
| Controls | All binding overrides and confirm/cancel convention |
| Debug & diagnostics | Both debug flags |
| Battle Speed | Text mode/rate and hit/coin message suppression; these are the same shared text preferences |

Binding editor pages, capture/review and Help do not offer or route Reset. Capture may read
its physical key as a candidate binding and reject an existing assignment. Read-only launch
shows disabled reset actions. Category Back preserves the draft; root Cancel abandons it.
Only ordinary Apply writes through SettingsHost/SettingsStore. In Battle Speed, Confirm on a
value or Attack applies; Confirm on its Reset footer stages reset. The hint follows that focus.

### Bounded reproduction

Load the registered worktree environment, build the Debug adapter using the locked .NET workflow,
and import this fresh checkout once. Select the registered Witch entry bank/catalog (the
[entry selection](../content/presentation-assets.md#witch-continue-entry-bank-2255)); the title
Settings transaction requires no campaign slot or opening sequence. Leave opening content unset.
Keep APPDATA, LOCALAPPDATA, TEMP and output in this worktree's ignored run directory. Set
`SF2_SAVE_ROOT` to a disposable external directory and use a new `issue2290-*` namespace per case.
Never point the observer at player storage.

For `main`, seed only that namespace's `sf2-preferences/input.json` with:

```json
{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":70,"modernGuidance":false,"reducedFlash":true,"suppressBattleHitCoinMessages":true,"bindings":{"spell":{"keys":["Q"],"buttons":[],"axes":["RightY-"]}}}
```

```powershell
$env:SF2_HOST_UI_CASE = 'main'
$env:SF2_HOST_UI_OUTPUT = Join-Path $freshRun 'result.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
  --path remake/game --script res://probes/settings_reset_observation.gd
```

Use `restart` on the completed main namespace with a distinct report. Use `override` with the
same initial JSON in a fresh ordinary namespace and a separate `-- --input-settings <file>`.
Use `recovery` with malformed `{invalid` bytes instead of JSON. Use `battle` with the initial
JSON and `-- --authored-package <absolute remake/content/authored/stone-court.json>`; it opens
inspection → Speed and compares session/revision/RNG across reset, Cancel and Apply.
Use `visual` without `--headless`, a fresh namespace and output directory: it mounts only the
dedicated DesignPreview and saves EN/zh root and pad Controls stills locally. The short visual
run brings its own window into focus before each language/page; do not run another native
input observer concurrently. Production `main`, `restart`, `override`, `recovery` and `battle`
cases never capture images.

**Confirmed:** main 41 checks, restart 6, override 8, recovery 7 and authored Battle Speed 9
passed. Dedicated preview review found legible separated list/footer in both languages and
actual installed keyboard/pad captions. The first report reader failed GBK decoding after
main had exited 0; reading the existing UTF-8 result confirmed all 41 checks without rerunning
that completed observation. A later visual run completed with four navigation failures (focus loss was the inferred cause); its report/stills remain local. Explicit focus before each preview segment
passed the bounded rerun. Several exits retained the existing two-ObjectDB-instance warning.
**Unknown:** physical controller/desktop focus interactions and human reading comfort beyond
the inspected preview remain with player review. No full gameplay or research suite is claimed.
