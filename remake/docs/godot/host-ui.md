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
No game rule, persistence format, savepoint, auto-save, binding capture or device detector is added.
Battle, field/Party and shop presenters keep their separate owners.

## Implemented composition and return

The centered Menu uses the existing square entry theme, an 800px capped width, at least 64px
command rows, quiet Journey/Session headings and a gold outline plus marker on the selected row.
Resume starts selected on each new opening. Unavailable commands retain reasons and are skipped
by directional selection. The bounded command/reason region scrolls; title and mapped footer
remain outside it. Pointer hit testing intersects the reading viewport, including after scrolling.
Left/Right and the mouse wheel read long reasons without activating unavailable commands.

Settings uses a 960px capped panel. Each preference has its current value and a wrapped description.
Defaults remains in the reading area; Apply, Back/Close and Help stay in a fixed footer outside it.
Keyboard/pad selection and deliberate pointer activation use the same draft. Help retains all 13
existing control pages, admitted bindings, paging and scrolling, now with English/zh-CN prose.
Its field/battle instructions describe the accepted Up/Down command lists. Back retains the settings
draft and Help row. The existing two-row battle Speed caller still edits only its own preferences.
With a footer action selected, Left/Right scrolls the settings reading area; on a preference row
the existing Left/Right value editing keeps precedence.

The shared slots use the same square theme for both Witch entry and gameplay callers. Gameplay
Save/Quit has a 1040px capped panel and bounded reading, with its hint outside the scrolling region.
Full actual name, date, destination, backup and failure information remains available. Slots support
Left/Right reading as well as the existing mouse scroll path. Save
confirmation starts on Cancel. Returning from confirmation restores the chosen slot, including
slot2; Back from slots restores the originating system Save row. Successful Save and return closes
the host to gameplay. Quit uses the existing clean/dirty warning and starts on Cancel. Corrupt
primary data remains an explicit failure with a separately offered backup, never an empty slot or
automatic fallback. Publication uncertainty still requires inspection rather than a blind retry.

Recovery hides the entry, slots, name, settings/help and system Menu it covers. The existing host
pauses gameplay and audio. Restoring a supported client shows the retained page and selection;
the name editor regains its retained actual focus. Recovery Quit has its own Cancel-first panel.
Settings/help/Menu cancel pending pointer gestures on both logical relayout and actual client-size
changes, including proportional changes that leave logical geometry unchanged. Entering recovery
also explicitly cancels a settings/help gesture. Existing release barriers prevent the closing
Confirm from moving, acknowledging text or choosing the next gameplay action.

These layouts cap reading width and retain 30px body/24px hints. Gameplay still requires a client
of at least 1920×1080. A smaller client enters recovery; it does not scale gameplay down.

## Access through the finite demo

The host queries current session authority when presenting each offer. A paused picture or a
finished animation never establishes save eligibility.

| Current boundary | Settings/help | Save and return / Save and Quit |
| --- | --- | --- |
| Title or available Witch entry | Ordinary preferences, or read-only values/help with `--input-settings`. Back keeps the entry phase. | Continue/New Game retain their existing two-slot entry routes. |
| Settled walking field, no Party/service/story owner | Preferences and help available. | Only an admitted finite recipe after stable player/followers, story, display and music checks. |
| Moving field/followers | The existing menu-entry settlement runs first; no new timeout or settlement rule. | The same bounded settlement may admit Save; failure keeps its reason and no queued write. |
| Text, choices, services, story motion, fades, transfer or outcome programs | Conditional rows remain unavailable until their real owner returns control. Party/preparation prevents system entry until closed. | Busy state or missing recipe refuses Save. Church's explicit Save caller retains its own completed-parent return. |
| Battle01–07 settled player Movement at the saved actor's origin | General Settings is unavailable. The separate settled inspection Options → Speed caller retains its existing two-row/read-only behavior. | Existing new-game provenance, encounter/return route, actor/queue, story and music predicates must all pass. |
| Nonzero movement preview, movement delivery, ActionChoice, lists/targets, confirmations, action playback, enemy/round/outcome work | General Settings remains unavailable, with its existing reason. | Refused with the owning preview, selection, movement, story or busy reason. Resume does not undo or commit the retained choice. |
| Unsupported/diagnostic start or field without a save recipe | Safe field preferences may still be available. | Existing profile/route/location refusal remains explicit. A controlled direct-battle start is not an ordinary saveable journey. |
| Undersized client | Only restore/fullscreen/quit recovery owns input. | No automatic save; Cancel from Quit returns to recovery or the retained page after a supported resize. |

Field recipes remain `new-game`, `map3-opening`, `map3-joined`, `battle01-return` and the completed
Battle07 `battle07-return` Map58 anchor, according to current content/persistence authority.
First Map14 control is inside the [campaign demo frontier](../application/pre-voyage-campaign.md),
not an added field-save recipe. Inspection remains read-only; explicit Save alone consumes the
existing live music/capture path. Gameplay/progress never stores preferences.

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
