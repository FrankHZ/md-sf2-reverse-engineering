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
Rows show slot number, full protagonist name, saved level and local date/time; backup and failure
information remains independently readable. Ordinary numbered
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

System Menu now includes Load between Save and Settings. Settings remains the initial selection.
Load shares Save's settled-boundary admission and displayed refusal reason. Dirty progress opens
a Cancel-first warning, then the existing paged history with level/local-time summaries. Back or
an error restores Load focus and the original session. Successful load closes the host directly to
the selected field/battle snapshot. The [lifecycle owner](../application/opening-session-lifecycle.md#in-game-load-2286)
defines replacement, compatibility, explicit recovery and held-input semantics.

The host queries current session authority when presenting each offer. A paused picture or a
finished animation never establishes save eligibility.

| Current boundary | Settings/help | Save and return / Save and Quit |
| --- | --- | --- |
| Witch action menu | Ordinary preferences, or read-only values/help with `--input-settings`. Back keeps the entry phase. | Initial focus is Continue with an admitted primary and New Game otherwise. Continue reads paged history after the Witch's question; New Game can choose a fresh numbered destination. |
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

<a id="dedicated-numbered-save-and-native-recovery-demo"></a>

## Numbered-save Main state and input checks

**Accepted design #2215 / #2223, implemented in #2224. Standard rigor** covers the interaction
between numbered storage, host input and the native client. Run [Main.tscn](../../game/Main.tscn)
in an isolated process at the real root window. This is ordinary Main state/input verification,
not the dedicated DesignPreview visual demo. Do not capture screenshots here.
The ordinary title, Witch and field Menu reach
the sample. The [save contract](../application/battle-save-and-continue.md#manual-save-history-and-developer-files)
and [window contract](presentation.md#window-support-and-recovery-1455) own the behavior.

### Sample setup and reset

Use a fresh PowerShell 7 session at the repository root, an imported project, and the Debug
adapter from the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
For the Debug build pass `-p:UseSharedCompilation=false -nodeReuse:false`. Set
`$disposableSaveRoot` to a dedicated existing absolute validation directory outside the worktrees
and player storage. Each launch creates its own namespace and uses the ordinary preferences file in
that namespace. App data, TEMP, logs, seed and baseline stay in ignored worktree-local scratch.

```powershell
$ErrorActionPreference = 'Stop'
. ./local/private-inputs.ps1
$package = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2282/buff-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2255/witch-entry-v1/entry.json'
$env:SF2_PRIVATE_OPENING_CONTENT = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2252/pre-voyage-opening-v1/opening/opening.json'
$env:SF2_ENTRY_DISPLAY_CATALOG = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2255/witch-entry-v1/entry.zh-CN.json'
foreach ($file in Get-ChildItem -LiteralPath "$package/opening-inputs" -Filter 'SF2_PRIVATE_*.json' -File) {
    [Environment]::SetEnvironmentVariable($file.BaseName, $file.FullName, 'Process')
}
$env:SF2_DISPLAY_CATALOG = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2266/catalogs-v1/world.zh-CN.json'
if (![IO.Path]::IsPathFullyQualified($disposableSaveRoot) -or
    !(Test-Path -LiteralPath $disposableSaveRoot -PathType Container)) { throw 'Select an external disposable save root' }
$env:SF2_SAVE_ROOT = $disposableSaveRoot
$review = (New-Item -ItemType Directory -Path "local/ui-host-review-$([guid]::NewGuid())").FullName
$historyFiles = @(2..7 | ForEach-Object { "slot$_.json" }) + 'slot7.bak'

function Invoke-NumberedSaveReview {
    param(
        [Parameter(Mandatory)][ValidateSet('seed', 'stage', 'continue', 'save', 'summary', 'launch')][string]$Case,
        [string]$From,
        [ValidateSet('en', 'zh-CN')][string]$Language = 'en',
        [string]$Resolution = '1600x900'
    )
    $space = "ui-host-review-$([guid]::NewGuid())"
    $run = (New-Item -ItemType Directory -Path "$review/$Case-$space").FullName
    $env:SF2_SAVE_NAMESPACE = $space
    $save = Join-Path $env:SF2_SAVE_ROOT $space
    $slots = (New-Item -ItemType Directory -Path "$save/sf2-opening").FullName
    $prefs = (New-Item -ItemType Directory -Path "$save/sf2-preferences").FullName
    if ($Case -ne 'seed') {
        $files = if ($Case -eq 'summary') {
            @(Get-ChildItem -LiteralPath $From -File | Where-Object Name -Match '^slot[0-9]+\.(json|bak)$' | Select-Object -ExpandProperty Name)
        } elseif ($Case -eq 'stage') { @('slot2.json') } else { $historyFiles }
        foreach ($name in $files) {
            $copy = Join-Path $slots $name
            Copy-Item -LiteralPath (Join-Path $From $name) -Destination $copy
            (Get-Item -LiteralPath $copy).IsReadOnly = $false
        }
    }
    @{ formatVersion=1; language=$Language } | ConvertTo-Json |
        Set-Content -LiteralPath "$prefs/input.json" -Encoding utf8NoBOM
    $env:APPDATA = (New-Item -ItemType Directory -Path "$run/appdata").FullName
    $env:LOCALAPPDATA = (New-Item -ItemType Directory -Path "$run/localappdata").FullName
    $env:TEMP = (New-Item -ItemType Directory -Path "$run/temp").FullName
    $env:TMP = $env:TEMP
    $env:SF2_HOST_UI_CASE = $Case
    $env:SF2_HOST_UI_OUTPUT = Join-Path $run 'native.json'
    $env:SF2_HOST_UI_STILLS = ''
    if ($Case -eq 'seed') {
        $env:SF2_HOST_UI_START = Join-Path $run 'start.json'
        @{ start = @{ map='map-3'; player='entity-0'; position=@{x=32;y=13}; facing=1; speed=32;
            flags=@(0,1,2,32,33,34,66,67,80,600,602,603,609,610) } } |
            ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $env:SF2_HOST_UI_START -Encoding utf8NoBOM
    }
    $godotArgs = @('--path', 'remake/game', '--resolution', $Resolution, '--max-fps', '120',
        '--audio-driver', 'Dummy', '--log-file', "$run/godot.log")
    if ($Case -eq 'launch') { $godotArgs += 'res://Main.tscn' }
    else { $godotArgs += @('--script', 'res://probes/numbered_save_review.gd') }
    & $env:GODOT_BIN @godotArgs | Out-Host
    if ($LASTEXITCODE) { throw "Review process failed. See $run" }
    return $slots
}
```

If you already have a compatible **disposable** format-6 field slot 2, set `$seed` to a read-only
copy of its directory containing `slot2.json`. Otherwise run the existing controlled Map3 admission once and
freeze its slot 2. The stage process cold-continues slot 2, verifies saveable walking, then uses
Menu → Save to create slots 3–7 and overwrites slot 7 once to create the backup. On a seed mismatch,
generate a new seed with the selected package.

```powershell
$generated = Invoke-NumberedSaveReview -Case seed
$seed = (New-Item -ItemType Directory -Path "$review/seed").FullName
Copy-Item -LiteralPath "$generated/slot2.json" -Destination $seed
(Get-Item -LiteralPath "$seed/slot2.json").IsReadOnly = $true

$staged = Invoke-NumberedSaveReview -Case stage -From $seed
$baseline = (New-Item -ItemType Directory -Path "$review/baseline").FullName
foreach ($name in $historyFiles) {
    Copy-Item -LiteralPath (Join-Path $staged $name) -Destination $baseline
}
[IO.File]::WriteAllBytes((Join-Path $baseline 'slot7.json'), [byte[]]@(0x7B))
Get-ChildItem -LiteralPath $baseline -File | ForEach-Object { $_.IsReadOnly = $true }
```

Slot 1 is empty. Page 1 offers slots 1–6, page 2 exposes the damaged slot 7 and its recoverable
backup, and Save offers new slot 8. Each valid file keeps the slot identity assigned by the
production writer. Open the sample and reset with the same command after closing the owned window.
Each invocation copies the read-only baseline to another fresh namespace.

```powershell
$null = Invoke-NumberedSaveReview -Case launch -From $baseline
```

Choose `-Language zh-CN` for Simplified Chinese. Keep the ordinary Main client visible. Choose
`-Resolution 1920x1080` when a decorated client fits. The root window owns startup fit, resize
admission and native Fullscreen.

### Routes and acceptance

| Production route | State and input acceptance |
| --- | --- |
| Title → Witch → Continue → Next → slot 7 | Read the second page and Previous action. Verify the actual damaged-primary reason, backup identity and Cancel-first prompt. Cancel returns to the Witch caller with both files unchanged. Reopen the prompt and explicitly recover. Compare the installed primary and retained backup to the original backup bytes, then check the restored field progress and usable control. Use a fresh copy for the write review. |
| Continue slot 2 → field → F10 → Save → Next → slot 7 | Read the protected-write reason and fresh-slot action. Back restores slot 7 on page 2, then the Save row in Menu, then field control. File bytes stay unchanged. On page 1, slot 6 shows the separate Cancel-first overwrite and its backup consequence. Cancel preserves bytes and selection. Confirm Save and Return on the reopened prompt and check that the previous slot 6 primary becomes its backup and the saved acknowledgement returns to the field. |
| Continued field → F10 → Save, with page 2 selected | On a desktop that can fit a decorated 1600×900 client, shrink the actual window to an undersized client such as 1280×720. Read actual client/mode, pause and visibility from the running host. Only recovery owns input. Open Quit, verify Cancel starts selected, then cancel and choose Restore Window. Require actual supported readback, the same session, Save page and row, unchanged revision and files, and fresh input before activation. A pointer press started before resize must expire. |

The [numbered-save observer](../../game/probes/numbered_save_review.gd) reuses the ordinary host
input and projections. It mounts Main in the native root, reads the actual client/mode and checks
attached landing, names, gold and flags against the selected record. Review the report, native
log and exit code; leave `SF2_HOST_UI_STILLS` empty. For visual review, use the existing
[DesignPreview](../../game/probes/previews/README.md) and its supported page routes under the
[preview visual rules](../development-and-verification.md#dedicated-ui-preview-visual-review).
Only that dedicated preview permits necessary local screenshots. If it lacks the affected state,
record the visual coverage gap rather than capturing ordinary Main.

```powershell
$null = Invoke-NumberedSaveReview -Case continue -From $baseline -Language zh-CN -Resolution 1920x1080
$null = Invoke-NumberedSaveReview -Case save -From $baseline
```

**Historical completed observation:** on a native Windows desktop with a 3586×2160 usable area,
the following was **Confirmed**. Retained stills are past evidence, not authorization for new
ordinary-Main screenshots. Chinese Continue at
1920×1080 kept the damaged status, Previous, backup identity, primary detail and both actions
readable. English Save at 1600×900 kept the protected-write reason, fresh slot and overwrite
consequence above the fixed actions. The actual 1280×720 recovery cover hid Save and field,
kept Restore/Fullscreen/Quit visible, and scrolled the last explanation line independently of
the actions. Quit Cancel and Restore returned to the 1600×900 decorated client with the same
session, revision, slot, page and files. The pre-resize press reached the selected row and its
release after Restore left the row unchanged. A fresh Confirm opened slot 7's protected prompt.

The two runs changed only their expected slot pairs. The frozen source bytes stayed identical.
The first controlled start lacked a saveable boundary. The working seed uses the settled
F609/F610 setup. Earlier observations also retain a script parse error, an attempted comparison
between the field wire record and its narrower public progress projection, and a resize check
that compared JSON numbers as dictionary variants. Fullscreen startup entered the actual
3840×2160 mode. The 1920×1080 stills use a decorated client. Successful native
processes returned 0. Some retained the existing two-instance ObjectDB exit warning. Physical
fit on smaller work areas, DPI variations and display reading comfort remain **Unknown**.

## Slot summary observation

**Confirmed** by the `numbered_save_review.gd` `summary` case in English and Simplified Chinese,
using the current package selections above and a disposable Map3 seed. It observes production
`Label`/`SavedTime`, native button text/width, ordinary keyboard selection, refusal/recovery panels,
page changes, Witch Continue, gameplay Save and overwrite Cancel. No screenshot or real player save
is used. The two native runs each passed 89 checks. Ordinary `Bowie` and maximum-length `WWWWWWW`
names remain complete; labels fit the reading width. All copied files remain byte-identical.

Typed formatter controls deliberately differ from content defaults: field v6 level 41, legacy
field v1 level 42, battle actor level 43 and retained ally level 44. They test display extraction,
not gameplay admission or reach. An absent battle progress value gives the localized unknown-level
label; an omitted legacy level fails the existing codec with `save-malformed`. Only the v6 Map3
slot is continued through the ordinary reader. The synthetic battle rows are never mounted.

On a host with `TimeZoneInfo.Local.Id == "Central Standard Time"`, the known instant
`2026-01-01T02:30:00Z` displays `2025-12-31 20:30`. A direct typed `+08:00` representation of that
instant gives the same result. This does not relax the disk codec's existing UTC-offset requirement.
Spring inputs `2026-03-08T07:59Z` and `08:00Z` display `01:59` and `03:00`; autumn inputs
`2026-11-01T06:30Z` and `07:30Z` both display `01:30`. These checks use the operating system's zone
without changing machine configuration. On another host, supply independently calculated expected
local values in the case file rather than changing the user's timezone.

The existing read-only C# observation `ReadSaveSummaryObservationJson(path, savedAt)` accepts bounded
worktree-local records. `savedAt` is an optional typed formatter control, not a write or a new load
path. `SF2_SLOT_SUMMARY_CASES` selects a local JSON object with `language`, six expected `choices`
and `records` entries (`id`, absolute local `path`, optional `savedAt`, and `expected` fields among
`kind`, `label`, `level`, `time`, `zone`, `error`). The fixture recipe below builds these controls from
one disposable seed using the existing codec shapes; its checksums are required save-envelope data.

Use the environment and `Invoke-NumberedSaveReview` function above, then:

```powershell
$generated = Invoke-NumberedSaveReview -Case seed
New-Item -ItemType Directory -Path local/issue2285 -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $generated 'slot2.json') -Destination local/issue2285/seed.json
```

Prepare the bounded local controls with Python (run from the repository root):

```python
from pathlib import Path
import json,base64,hashlib,copy
root=Path('local/issue2285'); baseline=root/'baseline'; baseline.mkdir(exist_ok=True); cases=root/'cases';cases.mkdir(exist_ok=True)
r=json.loads(base64.b64decode(json.loads((root/'seed.json').read_bytes())['payload']))
r['savedAt']='2026-01-01T02:30:00+00:00'; r['data']['registers']['names']={'ally-0':'WWWWWWW'}
r['data']['party']['allies'][0]['level']=41
p=r['data']['party']; reg=r['data']['registers']; ally=p['allies'][0]
def write(path,obj):
    raw=json.dumps(obj,ensure_ascii=False,separators=(',',':')).encode();path.write_text(json.dumps({'payload':base64.b64encode(raw).decode(),'sha256':hashlib.sha256(raw).hexdigest().upper()}),encoding='utf-8')
for n in [2,3,5,6,7]:
    row=copy.deepcopy(r);row['slot']=n
    if n==3:row['data']['compatibility']['rules']='incompatible-summary-control'
    if n==6:row['data']['registers']['names']={'ally-0':'Bowie'}
    write(baseline/f'slot{n}.json',row)
(baseline/'slot5.bak').write_bytes((baseline/'slot5.json').read_bytes());(baseline/'slot5.json').write_text('{');(baseline/'slot4.json').write_text('{')
write(cases/'field.json',r)
old={k:copy.deepcopy(v) for k,v in r.items() if k!='data'};old['formatVersion']=1
old['data']={k:copy.deepcopy(v) for k,v in p.items() if k!='depot'}
old['data'].update({k:copy.deepcopy(reg[k]) for k in ['names','flags','egressMap','stepCounter','textCursor','textSettings','fadePeriod']})
old['data'].update(compatibility=r['data']['compatibility'],recipe='map3-opening',map='map-3',x=32,y=13,facing=1,music={'cue':'MUSIC_TOWN','previous':[]})
old['data']['allies'][0]['level']=42;write(cases/'old.json',old)
missing=copy.deepcopy(old);del missing['data']['allies'][0]['level'];write(cases/'old-missing-level.json',missing)
absent={'present':False}
def yes(value):return {'present':True,'value':value}
progress={k:ally[k] for k in ['level','maxHp','maxMp','baseAttack','defense','agility']};progress.update(level=43,spells=[],sourceLoadout=absent)
a={k:ally[k] for k in ['hp','mp','status','initialized']};a.update(actorId='ally-0',position=absent,deploymentPosition={'x':1,'y':1},facing=1,attack=6,aiMemory=0,exp=yes(0),kills=yes(0),defeats=yes(0),activationWord=yes(0),primaryOrder=absent,secondaryOrder=absent,lastTarget=absent,progress=yes(progress),loadout=absent)
battle=dict(encounter=p['encounter'],actors=[a],mainSeed=1,thinkingSeed=2,round=1,queue=[],cursor=0,gold=60,options=dict(difficulty=0,allyAutoBattle=False,opponentControl=False,missingCandidateAllyWord=absent),regions=absent,deathCredit=absent,custody=dict(onceFlags=[],deals=[]),visuals=absent)
axis=dict(position=0,destination=absent,speed=0)
story={k:copy.deepcopy(reg[k]) for k in ['flags','names','stepCounter','egressMap','textCursor','textSettings','windowFixPending']}
story.update(display=dict(period=3,base=reg['palette'],current=reg['palette'],visibility=0),logicalText=dict(open=False,x=0,y=0,row=0,animationLength=0,animationCounter=0,moving=False,indicator=0,indicatorVisible=False,originY=0,destinationY=0),logicalView=dict(area={k:0 for k in ["minX","minY","maxX","maxY","foregroundX","foregroundY","backgroundX","backgroundY","parallaxAX","parallaxAY","parallaxBX","parallaxBY","autoscrollAX","autoscrollAY","autoscrollBX","autoscrollBY","layer"]},targetSlot=absent,ax=axis,ay=axis,bx=axis,by=axis,followCounter=0,hideWindows=False),speaker=absent,entityServices=absent,saveAnchor=absent,music=dict(map='map-3',area=0,cue='MUSIC_TOWN',previous=[]),player=dict(entity='entity-0',speed=32,retainedAllies=[]))
b={k:v for k,v in r.items() if k!='data'};b.update(formatVersion=2,kind='battle-player-input',data=dict(compatibility=r['data']['compatibility'],battleRecipeRevision=1,route=dict(map='map-3',ordinal=0),battle=battle,story=story));write(cases/'battle.json',b)
u=copy.deepcopy(b);u['data']['battle']['actors'][0]['progress']=absent;write(cases/'battle-unknown.json',u)
t=copy.deepcopy(b);t['data']['battle']['actors']=[]
ret={k:ally[k] for k in ['hp','mp','status','absent']};ret.update(actor='ally-0',exp=yes(0),kills=yes(0),defeats=yes(0),positionOverride=absent,progress=yes(progress|{'level':44}),sourceLoadout=absent,activationWord=yes(0),initialized=yes(True),retainDeadPosition=False)
t['data']['story']['player']['retainedAllies']=[ret];write(cases/'battle-retained.json',t)
for lang in ['en','zh-CN']:
    slot='Slot' if lang=='en' else '槽位'; lv='Lv.' if lang=='en' else '等级'
    records=[]
    for file,kind,level in [('field','FieldSnapshotRecord',41),('old','OpeningSaveRecord',42),('battle','BattleSaveRecord',43),('battle-retained','BattleSaveRecord',44),('battle-unknown','BattleSaveRecord',None)]:
        leveltext=f'{lv} {level}' if level else ('Level unknown' if lang=='en' else '等级未知')
        records.append(dict(id=file,path=str((cases/f'{file}.json').resolve()),expected=dict(kind=kind,level=leveltext,time='2025-12-31 20:30',label=f'{slot} 2 · WWWWWWW\n{leveltext} · 2025-12-31 20:30')))
    records.append(dict(id='missing-old-level',path=str((cases/'old-missing-level.json').resolve()),expected={'error':'save-malformed'}))
    for name,instant,expected in [('offset','2026-01-01T10:30:00+08:00','2025-12-31 20:30'),('spring-before','2026-03-08T07:59:00Z','2026-03-08 01:59'),('spring-after','2026-03-08T08:00:00Z','2026-03-08 03:00'),('fall-before','2026-11-01T06:30:00Z','2026-11-01 01:30'),('fall-after','2026-11-01T07:30:00Z','2026-11-01 01:30')]:
        records.append(dict(id=name,path=str((cases/'field.json').resolve()),savedAt=instant,expected=dict(time=expected,zone='Central Standard Time')))
    states=['Empty','Incompatible content or rules','Damaged save','\nRecoverable backup'] if lang=='en' else ['空槽位','存档内容或规则不兼容','存档已损坏','\n有可恢复的备份']
    choices=[f'{slot} 1 · {states[0]}',f'{slot} 2 · WWWWWWW\n{lv} 41 · 2025-12-31 20:30',f'{slot} 3 · {states[1]}',f'{slot} 4 · {states[2]}',f'{slot} 5 · {states[2]}{states[3]}',f'{slot} 6 · Bowie\n{lv} 41 · 2025-12-31 20:30']
    (root/f'cases-{lang}.json').write_text(json.dumps(dict(language=lang,records=records,choices=choices),ensure_ascii=False,indent=2),encoding='utf-8')
print('Prepared bounded copied field and synthetic typed controls.')
```

After preparing the controls, run one fresh namespace per language with screenshot capture disabled:

```powershell
foreach ($language in @('en', 'zh-CN')) {
    $env:SF2_SLOT_SUMMARY_CASES = (Resolve-Path "local/issue2285/cases-$language.json").Path
    $null = Invoke-NumberedSaveReview -Case summary -Language $language -From (Resolve-Path local/issue2285/baseline).Path
}
```

The probe skips the opening with ordinary input, waits for the Witch's save check and acknowledges
Continue's question/recognition/farewell. Earlier attempts omitted those newer entry phases and
failed before opening Menu or reaching the Witch menu; they did not establish a production prefetch
failure. An initial synthetic battle fixture also used the wrong view-area/axis JSON shape and was
rejected before formatting; the typed shape above corrects that fixture. Completed failures are
retained in the Issue/PR handoff, not reclassified as interrupted runs. Passing native exits still
report two `ObjectDB` instances leaked at shutdown; no adapter exception, game error or save mutation
was observed. This slice does not claim to fix that shutdown warning or certify broader startup,
performance, story progression or human aesthetic acceptance.

## In-game Load observation (#2286)

**Confirmed, bounded remake host behavior:** English and Chinese field and Battle01 ordinary
input load through the top-right Menu without title/Witch. Field witnesses cover dirty confirmation
and history cancellation, changed-file refusal, unsupported future version, incompatible rules,
corrupt primary, backup Cancel and explicit recovery, valid primary and held Confirm. The battle
witness saves the actual player turn, rejects ActionChoice, cancels history without mutation,
compares the complete captured/restored battle payload and accepts a fresh Confirm only after release.
A separate ordinary NPC Talk witness checks the unavailable Load reason and held Cancel at story text.
Old field/battle nodes are freed after replacement preflight. Field snapshots compare landing,
names, map and gold; this is not a new exhaustive field-codec proof. Headless state/input checks
establish no physical controller, OS focus, human visual/audio or whole-campaign acceptance.

Use the selected SDK/Godot and immutable `issue2282/buff-v1`, `opening2252`, `witch2255` and
`catalog2266` inputs from [Sample setup](#sample-setup-and-reset). Retain those exact package
selections; an unaccepted art candidate is not a substitute. No player backup is needed: the
observer creates a production slot in its disposable namespace, then modifies only that copy.
The existing High-rigor lifecycle agreement controls scope; no new persistence abstraction,
format, content admission, input defaults or unit tests of verification helpers are introduced.

Build the Debug adapter with `UseSharedCompilation=false` and `nodeReuse:false` after loading
`local/private-inputs.ps1`; use the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
Import the changed PO resources with the selected editor only when stale. Start from the repository
root and use the package/environment selection in Sample setup through `$env:SF2_SAVE_ROOT`.
Set `$disposableSaveRoot` to an external validation directory, never player storage. Then run:

```powershell
function Invoke-InGameLoadObservation {
    param([ValidateSet('field','battle','story')][string]$Case='field',
          [ValidateSet('en','zh-CN')][string]$Language='en')
    $space = "issue2286-$Case-$Language-$([guid]::NewGuid())"
    $run = (New-Item -ItemType Directory -Path "local/issue2286/$space" -Force).FullName
    $env:SF2_SAVE_NAMESPACE = $space
    $save = Join-Path $env:SF2_SAVE_ROOT $space
    New-Item -ItemType Directory -Path "$save/sf2-opening" -Force | Out-Null
    $prefs = (New-Item -ItemType Directory -Path "$save/sf2-preferences").FullName
    @{formatVersion=1; language=$Language} | ConvertTo-Json |
        Set-Content -LiteralPath "$prefs/input.json" -Encoding utf8NoBOM
    foreach ($name in @('APPDATA','LOCALAPPDATA','TEMP')) {
        [Environment]::SetEnvironmentVariable($name,
            (New-Item -ItemType Directory -Path "$run/$name").FullName, 'Process')
    }
    $env:TMP = $env:TEMP
    $env:SF2_HOST_UI_CASE = $Case
    $env:SF2_HOST_UI_OUTPUT = Join-Path $run 'native.json'
    $env:SF2_HOST_UI_STILLS = ''
    $env:SF2_HOST_UI_START = Join-Path $run 'start.json'
    $start = @{map='map-3'; player='entity-0'; position=@{x=32;y=13}; facing=1; speed=32;
        flags=@(0,1,2,32,33,34,66,67,80,600,602,603,609,610)}
    if ($Case -eq 'story') { $start.position.x=28 }
    if ($Case -eq 'battle') {
        $start=@{map='map-57';player='entity-0';position=@{x=8;y=14};facing=3;speed=32;
            flags=@(0,1,2,32,33,34,66,80,401,451)}
    }
    @{start=$start} | ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $env:SF2_HOST_UI_START -Encoding utf8NoBOM
    & $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --path remake/game `
        --script res://probes/ingame_load_observation.gd --log-file "$run/godot.log"
    if ($LASTEXITCODE) { throw "Load observation failed: $Case / $Language" }
}
Invoke-InGameLoadObservation -Case field
Invoke-InGameLoadObservation -Case field -Language zh-CN
Invoke-InGameLoadObservation -Case battle
Invoke-InGameLoadObservation -Case battle -Language zh-CN
Invoke-InGameLoadObservation -Case story
```

Run serially in the owned project; each process has a 55-second deadline and exits itself. One
existing content admission per process, a few small save copies and selected state samples suffice.
There is no acquisition, image capture or frame-history export. Keep reports/logs below 64 MiB and
reassess on unexpected growth; no large processing run is required.

Completed failures remain discovery evidence: first menu navigation ran before movement settlement;
new localization keys initially lacked their required key-registry entries; preparing a field beside
its old namesake exposed a stable-node-name collision. Those were corrected before passing runs.
Early observer assertions compared changing presentation/ordinary field-clock revisions, expected a
clean field after resumed clock ticks, or used an English slot prefix in Chinese. The battle approach
has no text wait, so it cannot prove story refusal; an initial Map3 story coordinate was rejected.
The corrected story start above uses ordinary Talk to the adjacent NPC. Final error-classification review also found that Godot JSON reserialization changed integer wire spelling in the incompatibility control; the observer now edits only the original rules string and checks the specific compatibility message in both languages. Existing two/four ObjectDB
shutdown warnings remain #1716. No production exception or save mutation outside the disposable
selected namespace was observed. Local reports retain each completed failure; no slow aggregate
suite or full campaign was rerun to replace them.

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
