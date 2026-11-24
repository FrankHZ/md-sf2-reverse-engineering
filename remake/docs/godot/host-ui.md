# Host Menu, preferences and save return

## Scope and ownership

**Standard rigor; #1688, parent #1616.** Apply the accepted [F10 design](ui-scene-review.md#7-host-menu-settings-save-and-recovery)
to the ordinary game. This is a bounded presenter change within the existing High-rigor
save/recovery agreements. The outcome is readable host pages with one input owner, explicit
consequences and faithful return to the paused caller through the pre-voyage demo.

The existing [system Menu](presentation.md#system-menu-agreement-1518),
[preferences](presentation.md#basic-preferences-agreement),
[save host](presentation.md#gameplay-save-and-quit-windows-1450),
[window recovery](presentation.md#window-support-and-recovery-1455) and
[battle persistence](../application/battle-save-and-continue.md) retain behavior authority.
The host delegates journey admission and persistence to the existing save contracts. Battle,
field/Party and shop presenters keep their separate owners.

## Implemented composition and return

The standalone gameplay Menu keeps compact symmetric padding. It shares the square normal, hover,
pressed, disabled and focus style boxes with the entry family. Journey rows separately reserve
space for their focus marker. Native binding tooltips use the square reading panel and 24px type,
including the popup created on hover over Menu. The field footer retires on host cover.
[Production observation](ui-scene-review.md#production-controls-audit)

The centered Menu uses the existing square entry theme, an 800px capped width, at least 64px
command rows, quiet Journey/Session headings and a gold outline plus marker on the selected row.
Settings starts selected on each new opening. Cancel, right-click or the Menu binding resumes
the covered caller. Unavailable commands retain reasons and are skipped
by directional selection. The bounded command/reason region scrolls; title and mapped footer
remain outside it. Pointer hit testing intersects the reading viewport, including after scrolling.
Left/Right and the mouse wheel read long reasons without activating unavailable commands.

Settings opens a 960px capped panel with four categories: Display & text, Gameplay aids,
Controls, and Debug & diagnostics. Apply is the root footer button. Mapped Cancel or a completed right-click backs out or closes the page.
Enter a category to edit values; Back restores its launching row and scroll. Left/Right changes
a value or scrolls the choices. A fixed-height area below the choices holds descriptions, errors
and technical statistics. Use the displayed Read details binding or wheel over that area when
text overflows. Reading details keeps the choice focus, scroll and action positions. Statistics
appear on the Debug page when enabled. Controls → Change controls and Controls → Help share the Settings draft. Help reads
installed bindings; only Apply publishes edits. See [changing controls](input-remapping.md)
for capture, review, reset and refusal behavior. Battle Speed remains a separate two-row caller.
Settings owns the viewport focus while open. The host resolves keyboard and pad against the
installed mapping once and consumes their raw events. Pointer picking and wheel scrolling use
Godot GUI dispatch. Release over an eligible button invokes the same row operation as mapped
activation. Resize, window exit and loss of focus cancel a pending click.
[Input ownership](ui-architecture.md#settings-and-its-children) · [Reproduction](../verification/host.md#native-settings-focus-and-gui-picking)

Gameplay aids → Hold speed cycles through 1.5×, 2×, 2.5× and 3×. The row shows the draft
multiplier and its details show the installed Accelerate binding. Apply installs and saves the
selected multiplier, Cancel discards the draft, and Restore defaults stages 1.5×. Existing
format 1 preferences with no `accelerationMultiplier` load as 1.5×. Controls Help shows the
installed multiplier and bindings. The same preference applies to eligible exploration and battle.
[Hold semantics and verification](presentation.md#held-exploration-acceleration-1619-a)

The shared slots use the same square theme for both Witch entry and gameplay callers. Gameplay
Save/Quit has a 1040px capped panel and bounded reading, with its hint outside the scrolling region.
Full name, date, destination, backup and failure information remains available. Ordinary numbered
history supports paging and a new numbered destination on write entry.
Left/Right and mouse scroll read long slot labels. Save confirmation starts on Cancel. Returning
from confirmation restores the chosen slot; Back from slots restores the originating system Save
row. The dedicated preview uses one explicitly selected disposable file.
[Save history contract](../application/battle-save-and-continue.md#manual-save-history-and-developer-files)

Successful Save and return closes the host to gameplay. Quit uses the existing clean/dirty warning and starts on Cancel. Corrupt
primary data remains an explicit failure with a separately offered backup, never an empty slot or
automatic fallback. See the [1080p reading and recovery check](#slot-recovery-and-overwrite-at-1080p).
Publication uncertainty still requires inspection rather than a blind retry.

Recovery hides the entry, slots, name, settings/help and system Menu it covers. The existing host
pauses gameplay and audio. Restoring a supported client shows the retained page and selection;
the name editor regains its retained actual focus. Recovery Quit has its own Cancel-first panel.
Settings/help/Menu cancel pending pointer gestures on both logical relayout and actual client-size
changes, including proportional changes that leave logical geometry unchanged. Entering recovery
also explicitly cancels a settings/help gesture. Existing release barriers prevent the closing
Confirm from moving, acknowledging text or choosing the next gameplay action.

These layouts cap reading width and retain 30-unit body text and 24-unit hints on the design
canvas. At the 1600×900 desktop floor these project to 25 and 20 physical pixels. Below either
minimum dimension, gameplay enters [window recovery](presentation.md#window-support-and-recovery-1455).

## Access through the finite demo

The host queries current session authority when presenting each offer. A paused picture or a
finished animation never establishes save eligibility.

| Current boundary | Settings/help | Save and return / Save and Quit |
| --- | --- | --- |
| Witch action menu | Ordinary preferences, or read-only values/help with `--input-settings`. Back keeps the entry phase. | Continue reads paged history; New Game can choose a fresh numbered destination. |
| Settled walking field, no Party/service/story owner | Preferences and help available. | Only an admitted finite recipe after stable player/followers, story, display and music checks. |
| Moving field/followers | The existing menu-entry settlement runs first; no new timeout or settlement rule. | The same bounded settlement may admit Save; failure keeps its reason and no queued write. |
| Text, choices, services, story motion, fades, transfer or outcome programs | Conditional rows remain unavailable until their real owner returns control. Party/preparation prevents system entry until closed. | Busy state or missing recipe refuses Save. Church's explicit Save caller retains its own completed-parent return. |
| Battle01–07 settled player Movement at the saved actor's origin | Menu opens the shared Settings and Help. Settled inspection Options → Speed also keeps its two-row shortcut. | Existing new-game provenance, encounter/return route, actor/queue, story and music predicates must all pass. |
| Nonzero movement preview, movement delivery, ActionChoice, lists/targets, confirmations, action playback, enemy/round/outcome work | Menu opens shared Settings while the battle presenter owns play. | Refused with the owning preview, selection, movement, story or busy reason. Resume does not undo or commit the retained choice. |
| Unsupported/diagnostic start or field without a save recipe | Safe field preferences may still be available. | Existing profile/route/location refusal remains explicit. A controlled direct-battle start is not an ordinary saveable journey. |
| Undersized client | Only restore/fullscreen/quit recovery owns input. | No automatic save; Cancel from Quit returns to recovery or the retained page after a supported resize. |

Field recipes remain `new-game`, `map3-opening`, `map3-joined`, `battle01-return` and the completed
Battle07 `battle07-return` Map58 anchor, according to current content/persistence authority.
First Map14 control is inside the [campaign demo frontier](../application/pre-voyage-campaign.md),
not an added field-save recipe. Inspection remains read-only; explicit Save alone consumes the
existing live music/capture path. Gameplay/progress never stores preferences.

## Settings from the battle system Menu

The battle view stays paused while Menu, Settings or Help owns input. Cancel discards the draft.
Apply publishes the prepared preferences and returns to the same Settings row in Menu. Resume
keeps the current battle phase, actor and candidate. Release held controls before using them in battle.
Field-owned story and battle entry follow the existing field admission. The dedicated inspection
Options → Speed shortcut keeps its own return to Options.

| Setting | Effect after Apply during battle |
| --- | --- |
| Hold speed | The shared hold uses the selected multiplier after play resumes and a fresh Accelerate press is admitted. |
| Language | Refreshes the Menu, command and inspection projections and the current close-up message. Result lines already opened by Give, Discard, Search or Retreat keep their text until that line closes. Subsequent lines use the applied language where translated content exists. |
| Text mode and speed | The display mode is chosen when a line opens. The new rate applies when gradual text resumes. Existing acknowledgement and presentation tokens stay in place. |
| Reduced flash | Current battle presenters read the applied preference when projecting the selection or delivering cues. |
| Help, controls and Confirm/Cancel convention | Help shows installed bindings. Apply switches bindings and refreshes hints. The activating input is consumed through its release, including when it no longer matches the installed map. |
| Graphics and modern guidance | Battle uses installed HD artwork where available and Original elsewhere. Modern guidance applies during exploration. |
| Debug | Diagnostics follow the applied preference. Auto-win can act once play resumes and the existing player Movement rule admits it. The row explains this before Apply. |

The existing read-only launch override supports viewing preferences and Help. The
[native recipe](../verification/host.md#battle-system-settings-observation) uses an authored
battle through the ordinary Godot host.

## Slot recovery and overwrite at 1080p

**Confirmed.** In the Witch's **Continue** column, the backup prompt identifies the slot, name,
destination and UTC time. The primary failure detail follows at 24px, then any translation fallback
notice at 24px. The reading region starts at its first line. Cancel and Recover Backup sit below
it, followed by the mapped footer. Left/Right scroll the explanation while Up/Down selects an
action. The field Menu's **Save** uses the same order for an error or overwrite. The overwrite
consequence stays at 30px and names the slot and what becomes its backup.

| Production input path | Observed result at 1920×1080 |
| --- | --- |
| Continue → corrupt slot 1 with valid backup | The Chinese backup offer occupied four lines. The primary detail showed `save-malformed`. Cancel returned to the Witch menu with both files byte-identical. Reopening and explicitly choosing Recover repaired the primary from the backup, preserved the backup bytes and attached its full saved field progress. |
| Continue → corrupt or missing primary, valid backup, long Chinese detail | The detail retained the actual primary failure. The deliberately extended 24px text wrapped to 12 lines. The bundled font supplied every glyph. Scrolling the 612×631 reading region reached the final translation notice and returned to the first line. Both 612×76 buttons stayed fully visible above the footer. |
| Continue → empty slot or corrupt slot without backup | An empty slot showed `save-slot-empty`. The invalid slot showed `save-malformed`. Each offered Back and returned to the Witch. The invalid primary remained intact. |
| Restored Ribble field → Menu → Save | The corrupt slot displays its `save-malformed` reason with Back and Create a New Save. Back returns to the selected slot. The occupied valid slot displays its overwrite consequence and all three actions. Cancel returns to that slot, Back restores Save, then Back returns to walking. A fresh Down input moves the leader one tile. |

Run the [slot reading observation](../../game/probes/save_recovery_reading_observation.gd)
with the existing Debug adapter, matched product content, default bindings and instant zh-CN text.
Prepare the source in a disposable validation namespace by saving twice to slot 1 on the settled
Ribble field. This produces a current-recipe primary and backup through the actual Save host.
Use a **new** `issue1769-*` namespace beneath the configured external validation root for each case.
Copy the validation preferences and `sf2-opening/slot1.bak` there. For `corrupt`, also copy the
primary and replace only that copy with the single UTF-8 byte `{`, leaving slot 2 empty.
For `corrupt-stress`, prepare the same damaged primary and put that byte in `slot2.json` with
no slot 2 backup. For `missing-stress`, omit the slot 1 primary and use the same invalid slot 2.
Both stress cases extend `save.details` and remove `slot.matching` from the in-memory Chinese
translation, exercising the actual fallback notice. The `corrupt` case uses the shipped catalog.
Put the exact backup bytes in slot 1 as a healthy primary in a separate disposable namespace.
Continue it normally with the same configuration and capture `ReadPersistenceObservationJson`'s
`progress` object at the on-attach boundary, using `host_ui_observation.gd`'s `observe_node`.
Keep the object in a local JSON file for the recovered-attachment comparison. Set the case,
namespace, isolated app data/TEMP and a fresh ignored output path before each headless launch.

```powershell
$env:SF2_HOST_UI_CASE = $case  # corrupt, corrupt-stress or missing-stress
$env:SF2_SAVE_NAMESPACE = $freshValidationNamespace
$env:SF2_HOST_UI_OUTPUT = Join-Path $runRoot 'native.json'
$env:SF2_HOST_UI_EXPECTED_PROGRESS = $healthyContinueProgress
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
    --path remake/game --script res://probes/save_recovery_reading_observation.gd `
    --log-file (Join-Path $runRoot 'godot.log')
```

The native JSON records shaped label bounds, line counts, action rectangles and outcomes.
The direct byte comparison includes the untouched second slot. The original 20px projection put
the primary detail after the actions. An early full-progress comparison used an older recipe-2
Ribble record that recaptures at revision 4 with two additional allies. The current-recipe fixture compares complete progress between two attachment observations.
The disk check compares exact bytes separately. The headless runs retain the known two-instance
ObjectDB exit warning. Physical display/DPI reading comfort remains **Unknown**.

### Incompatible and protected slots

**Confirmed** in short headless sessions on the production host. Continue, New Game and field
Save showed the slot and file diagnostics below. Cancel and writer-lock failure kept the files
byte-identical. Explicit recovery left the compatible backup intact. An ordinary Save after the
incompatible overwrite preserved the old primary copy through normal backup rotation.

Use the registered Galam town world with its paired entry, scene and opening inputs, the Debug
adapter and a fresh disposable save namespace. Use instant text in English or Simplified Chinese.
Create one v1 `new-game` record through ordinary Witch New Game and keep it as the seed. To build
the cases below, copy that record, set the header slot to the destination number, and recompute
the envelope checksum after editing the decoded payload. For damage, write the single UTF-8
byte `{` in the synthetic primary. Run with the existing
[host input observer](../../game/probes/host_ui_observation.gd) helpers or drive the same input
sequence by hand.

| Slot | Primary / backup | Expected selection |
| --- | --- | --- |
| 1 | Gameplay revision 1 / matching seed | Continue reports `save-incompatible` and has Back. New Game and Save show the incompatible overwrite consequence with Cancel selected. |
| 2 | Absent / absent | Continue shows Empty and Back. New Game and Save can select it. |
| 3 | Format version 99 / matching seed | Continue reports `save-version` with Back. Write entry also offers a fresh numbered slot. |
| 4 | Damaged / gameplay revision 1 | Continue and write entry report `save-malformed`. Write entry offers Back and a fresh numbered slot. |
| 5 | Damaged / matching seed | Continue offers explicit Recover Backup, with Cancel first. Recovery restores playable progress and replaces only the primary with the exact backup bytes. |
| 6 | Matching seed / format version 99 | Continue can load the primary. Write entry shows the backup's diagnostic and a fresh-slot action. |

Use the registered controlled Galam `return-writer` start copied into worktree-local scratch
to reach a settled field without replaying the route. Open F10 → Save. Compare primary and backup
bytes before and after canceling slot 1. Confirm an overwrite and compare the displaced file to
the original primary byte-for-byte. Save to slot 1 again and check that the displaced copy
survives ordinary backup rotation. In a fresh copy, put a directory at `slot1.lock` to force an
early storage failure. Confirm the overwrite and verify the storage diagnostic and unmodified
primary and backup. For the New Game path, choose Create a New Save from the format error,
check the name entry, cancel, then overwrite slot 1 through the final Start confirmation.

## Reproduction and finite evidence

Use the existing selected SDK/Godot installation and product content, worktree-local runtime/cache
state and a fresh external disposable `issue1688-*` namespace. Never select player slots. Run
`uv run sf2 verify adapter`; native observation also needs the existing Debug adapter build under
the [protected SDK workflow](../development-and-verification.md#locked-net-workflow).
Import the two changed PO resources with the existing editor only when their imports are stale.

The tracked [`host_ui_observation.gd`](../../game/probes/host_ui_observation.gd) mounts ordinary
`Main.tscn`, submits actual keyboard/pad/mouse events, changes the actual client dimensions and
reads existing host/session projections and control geometry. It does not set a live session,
manufacture receipts, collect frames or take screenshots. Private setup uses the existing
`StartPreVoyageLocalizationObservation` admission before any session exists, then production hosts.

After loading the checkout's ignored tool/input configuration and the current complete product-art
world/entry selections, choose a new ignored report directory and these explicit values:

```powershell
$env:SF2_SAVE_NAMESPACE = 'issue1688-fresh-run'
# SF2_SAVE_ROOT is the configured external disposable validation root.
$env:SF2_HOST_UI_CASE = 'field'
$env:SF2_HOST_UI_OUTPUT = Join-Path $runRoot 'native.json'
$env:SF2_HOST_UI_START = Join-Path $runRoot 'start.json'
# Explicit detached initial input; no active game state is edited.
@{ start = @{ map='map-3'; player='entity-0'; position=@{x=32;y=13}; facing=1; speed=32;
    flags=@(0,1,2,32,33,34,66,67,80,600,602,603) } } |
    ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $env:SF2_HOST_UI_START -Encoding utf8
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --path remake/game `
    --script res://probes/host_ui_observation.gd
```

`field` performs real slot2 writes, overwrite Cancel with direct byte equality, backup publication,
Settings Cancel/Apply, Help, Quit Cancel, recovery and retained focus. `continue` uses the resulting
namespace in a fresh ordinary title process and compares every saved progress field at attach.
`backup` uses a fresh disposable copy with a deliberately corrupt primary and the actual generated
backup. `failure` holds only that namespace's `slot2.lock` externally during publication. `title`
checks the ordinary Settings/help route; add the existing `--input-settings` override for read-only
coverage. `battle` uses a fresh local copy of the accepted Map57 Battle01 approach from the
[battle save owner](../application/battle-save-and-continue.md); it reaches battle through the real
field program and observes Movement/ActionChoice, not a fabricated direct battle profile.
`SF2_HOST_UI_STRESS=1` changes only in-memory Translation resources before admission to exercise
long reasons, descriptions, help, slot records and Quit warnings. It never changes PO files or saves.

**Confirmed:** the finite English/zh-CN field, title/read-only help, real Continue, explicit backup
recovery and locked-slot refusal cases pass. Field settings geometry is checked at 1920×1080,
2560×1440,3840×2160 and2560×1080. English/Chinese connected Battle01 Movement/ActionChoice and
English long-reason battle cases retain exact pause/save predicates and release behavior. The
Chinese source-story reading case also verifies unavailable actions, long-reason keyboard reading,
recovery and held Resume at an actual settled text boundary. Real connected Battle01 save and
ordinary fresh-process battle Continue retain the saved actor's Movement boundary. The
existing accepted [system-menu observations](../verification/host.md#system-menu-observation) and
the save/entry owners retain the other unchanged host semantics.

Preserve failed attempts: an initial observer parse error; the reproduced settings click surviving
proportional resize (fixed by actual-window subscriptions); an invalid direct-battle eligibility
assumption (corrected to ordinary field admission); and the fresh-Continue observer initially
omitting the reveal input after persisted gradual text. Provisional story starts that missed the real source event, and a Resume assertion taken before
text had settled, are not host-pause evidence. The corrected approach stops at actual text input.
The battle-Continue observer also initially compared a wire actor object with its string value;
its failed run and deadline are retained before the corrected actor comparison. Individual native reports and exact
commands belong in the ignored execution handoff; failures are not replaced by later successes.

**Unknown / remaining demo gaps:** physical OS Restore/Fullscreen and focus delivery, physical
controller hardware and final human/aesthetic acceptance remain outside headless evidence. Headless
recovery measures real Window size events but cannot certify desktop fit. Native private/title
shutdowns retain the existing two/four ObjectDB warnings; no leak closure is claimed. No full
Battle01–07 or first-Map14 playthrough is repeated for this layout change. Sibling battle/Party/shop
coverage and their own remaining visual gaps remain with #1686/#1687. New save recipes and later
campaign admission remain outside this slice.
