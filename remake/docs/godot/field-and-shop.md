# Field, party and shop UI

This is the production F5–F7 owner for [#1687](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1687).
It consumes the accepted [scene review](ui-scene-review.md#3-exploration-world-speech-choices-and-search)
and [design language](ui-design-language.md). **Standard** implementation rigor: the parent #1616
controls the accepted visual scope; this bounded adapter change keeps the existing session, content,
transactions, pause and modal boundaries. Acceptance is the real field/service input, current state,
geometry and errors, followed by independent main-gate review.

## Current composition

- `FieldMenuView` presents Member / Magic / Item / Search in the right-hand list. `FieldServiceMenu`
  directly consumes the accepted `CompactCommandList`, original command frames at nearest 3×,
  80px minimum rows and reserved markers. Up/Down follows the list; Back has its own row.
  The list retains its initiating selection while a member/preparation page owns input.
- `PartyStatusView` keeps the member column visible beside the selected sheet. Portrait, identity,
  vitals, attributes, learned spells and inventory remain grouped. Records and gold sit between
  the sheet and its fixed bottom hint panel. Inside that panel the reading hint takes the space on
  the left; Prepare (Magic or Item at those entries) and Close sit together at the right inset.
  Each uses its own visible rectangle for press and release. The field sheet opts into `PartyDetailsPanel.FieldLayout`;
  the battle inspection caller retains its existing composition and 4× identity art.
- Item operations use the same right-hand list. Use / Give / Equip / Drop, item/level lists,
  recipients, exchange slots and confirmation use the existing field consumers. Spell rows include
  their MP cost; equipment rows retain the prospective attack value. Empty, unavailable and
  duplicate items retain their actual numbered positions. Inspecting an empty slot cannot submit a
  previously selected item. Unsupported/no-effect behavior remains attributed by the real query/result.
- Give names the donor and item while showing the focused recipient's four-slot pack. A full pack
  opens its numbered return slots before the real exchange confirmation. A refusal's Back returns
  to the same provisional page and index. Committed actions refresh their query before another action.
- `ShopServiceView` shows Buy / Sell / Repair / Deals and separate Leave. The service list yields
  to stock, member, slot, quote or result in the same region. The chosen person's four slots remain
  to its left during member, slot and quote decisions. Quotes name the item/member/price and show
  current → prospective gold, including the existing sale ceiling. A known funds/capacity/unsupported
  refusal shows unchanged prospective funds; only the revalidated session command can commit.

The four inventory positions reserve their icon/marker/text columns even without art. Moving focus
changes appearance, never the slot bounds; scrolling can move the whole reading region. Item and
spell identity art is 48×72px in field lists. Unavailable focus remains muted and inspectable.
Original pixels are not included in the public repository.

## Reading, input and return

The field footer owns one horizontal row with a flexible reading label, a 24px gap and a 320px
right-aligned Adventure target inside the panel insets. With ordinary field input available,
the reading label combines the current Talk/Search action with the installed hold-to-accelerate
binding. A pad without an assigned acceleration input names the keyboard fallback. The field menu
uses the actual child's rectangle for the full click gesture. Resize cancels an incomplete press.

Operation hints use 24px in the shop root, stock, recipient, sell-slot, quote and result pages.
Previously the root wrote the keys into its 30px reason label. The field, item and shop root lists
now give prose and controls separate wrapping labels in the existing scroll area. Deals uses
the same recipient and quote flow as Buy and shows
localized empty stock with Back when its list is exhausted. Shop footer copy
contains the actions available to the player. Item artwork and name resolution keep their existing
display rules. [Production measurements](ui-scene-review.md#production-controls-audit)

`FieldChoicePanel` measures the full heading/quote and rows inside a native scroll region; hints and
Back remain outside it. `FieldPackPanel` scrolls its own complete pack when needed. A command refusal
scrolls below the fixed command/Back rows. Existing English/Chinese text and fallback notices remain.
F10 Help also describes Up/Down selection and Left/Right field/shop reading. Only its corresponding
English/Chinese help text fields change; no global theme, localization framework or new input mapping is introduced.

Use mapped Up/Down for focus and Confirm for the selected action. Mapped Left/Right (the `↕` reading
hint) scroll the current field/service reading region, including the adjacent pack. The pointer wheel
scrolls the region beneath it. A complete pointer gesture activates a row; pointer motion updates
focus without submitting. Keyboard/pad, right-click Back, focus/relayout cancellation and opening
release barriers keep their existing input ownership. Holding Confirm cannot commit through a new page.

Back unwinds provisional preparation and restores its selection, then the originating field choice.
Use/Drop/Give and completed purchases/sales remain committed after acknowledgement. Shop Back follows
`ShopService.Back`: a quote returns to member selection (including Sell), a sale slot returns to its
owner, a purchase recipient returns to its Buy or Deals stock, and Leave completes the actual
source caller. The UI retains the corresponding owner/stock/slot focus when that phase is
re-entered. Search continues to use
its actual faced consumer and existing reader: meaningful line breaks, receipt/full-pack feedback,
Cancel dismissal and subsequent usable control are unchanged.

The gameplay list and preparation pause field time and retain their host-menu exclusion. Services
retain their actual wait owner. Repair opens member and held-slot selection with damage and
worn indicators. Damaged Power and Protect Rings quote 750 gold. Indra Staff quotes 800.
Confirm checks current
funds and the selected slot, then charges and clears damage together. Cancel at the quote
and dismissal after a result return to the member selection. Back at the member selection
returns to the shop menu. [Ordinary New Granseal Repair and Continue](../domain/equipment-use-repair-design.md#implemented-ring-loop-and-ordinary-reproduction)

## Reproduce the bounded observations

Load the owning ignored environment in the launching process. Override its inherited save selection
**before launching any game**, selecting a fresh disposable namespace outside every worktree; never
select a player's namespace. Keep app data, TEMP and reports local to this checkout. Use the existing
Godot installation/project and actual Debug assembly; no screenshots or new capture platform.

```powershell
. ./local/private-inputs.ps1
$env:SF2_SAVE_ROOT = $disposableExternalRoot
$env:SF2_SAVE_NAMESPACE = $freshNamespace
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv run sf2 verify adapter
Push-Location remake
& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
Pop-Location
```

The direct observer is `res://probes/engine_field_service_ui_observation.gd`. Its default field case
can use this small variation of tracked, project-authored content (no copied media):

```powershell
$case = Get-Content remake/content/authored/localization-pilot.json -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$case.start.Remove('program')
$case.start.position = @{ x = 2; y = 2 }
$case.start.facing = 2
$case.battle.actors[0].items = @(0, 0)
$case.battle.actors[1].items = @(0, 0, 0, 0) # use @() for the free-slot Give case
$case | ConvertTo-Json -Depth 100 | Set-Content $ignoredAuthoredInput -Encoding utf8
$env:SF2_FIELD_MENU_CASE = 'field'
$env:SF2_FIELD_UI_FULL = '1'
$env:SF2_FIELD_MENU_OUTPUT = $freshIgnoredReport
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
  --path remake/game --script res://probes/engine_field_service_ui_observation.gd -- `
  --authored-package $ignoredAuthoredInput --input-settings $instantEnglishOrChineseSettings
```

For remapped Confirm/Back, supply actual settings binding Confirm to F5 and Cancel to F6 and set
`SF2_FIELD_UI_REMAP=1`. Pad inputs still use the installed bindings. `SF2_FIELD_UI_LONG=1` supplies an
explicit **UI translation variation** before localization loads: approximately 1,150-character
quote/refusal/help strings with the original operand arity. For long slot reading, replace every
`Practice Leaf` name in the authored variation consistently with a legal ≤64-character long name.
This exercises presentation; it neither changes the shipped translations nor original-game evidence.

For the actual private Galam caller, select the registered complete pre-voyage world, matching scene,
static inputs and name catalog read-only. Use the existing
[`StartPreVoyageLocalizationObservation`](../../game/probes/PreVoyageLocalizationObservation.cs) envelope:
Map16/entity0 at (35,26), facing1, speed32, flags0/1/2/32/33/34/663. Set its worktree-local input path
as `SF2_INSPECTION_START`, `SF2_FIELD_MENU_CASE=shop`, `SF2_FIELD_UI_ART=1`, and omit `--authored-package` from the command.
The direct observer admits the ordinary production session, talks to the real merchant, verifies
quotes/cancel/funds/full/actual purchase and sale, then leaves and moves in that same session. The art case also opens the real field/Item owner, checks original command and identity art, inspects Use and returns without spending party resources. Paused menu RNG/flags/time are checked before resuming ordinary field time.
Controlled initial admission does not prove natural preceding campaign reach.

For the [member footer](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1788),
use that same private field start, with `"names":{"ally-0":"WWWWWWW"}` inside `start` (seven
wide letters, the legal name limit). Set `SF2_FIELD_MENU_CASE=party-footer`, select the existing
English or zh-CN input settings and a fresh `SF2_FIELD_MENU_OUTPUT`, and use
[`res://probes/party_footer_observation.gd`](../../game/probes/party_footer_observation.gd)
as the script. At 1920×1080 it checks the whole member sheet, the inset hit rectangles and
fully visible text, live gold and records, and the actual Member/Magic/Item and direct Party
return paths. Mouse, keyboard and mapped pad drive the ordinary field UI, including opening
equipment and backing out without changing the party.

No acquisition or asset/world copy is required. Work is bounded by the selected party, four slots and
finite shop stock; reports contain only named semantic boundaries. Budget 55 seconds / 2 MiB per
native case, serial ownership, and 16 MiB for small authored inputs and reports (existing build/import
state excluded). Completed cases take roughly 7–11 seconds; replan if a case exceeds those bounds.
Build outputs, private inputs and retained prior reports remain ignored.

### Hassan Deals and cold Continue

**Confirmed, bounded native.** Continue a disposable copy of the registered #1941 Hassan
Search slot (`hassan-en-final`) with the matching #1941 world, scene and name catalog. This
slot carries Search receipt F201 and Bowie holds Mithril in slot four. Use Field → Item →
Bowie → Drop and confirm that piece. His pack loses the item and Deals gains one copy.
Approach the weapon counter in Map9 from (20,10), facing Up. Talk, acknowledge the greeting
and select Deals. Select Mithril and Bowie, decline, then reselect the recipient and confirm
the same 2,000-gold quote. Gold moves from 9,174 to 7,174, Mithril returns to slot four, and
the next visit to Deals shows its localized empty state. Back and Leave finish the merchant
script. Walking remains available. F10 → Save uses the existing Hassan anchor.

Both English and Simplified Chinese shop projections passed this flow. A fresh process
continued each post-purchase save and saved identical decoded gameplay data, including the
party, gold, location, flags, depot and Deals. In a separate Chinese Continue from the Save
made just after Drop, the same merchant still listed one Mithril before purchase.
The checked inputs, trade rules and remaining coverage are in the
[session owner](../application/session-and-programs.md#rare-stock-and-deals-repurchase).

The registered `handoffs/issue1947` entry stages the observer and its required #1955 helpers in
the selected checkout. Load the worktree copy of the
[private input configuration](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup)
and build the Debug adapter as above. On a fresh Godot project or after a localization edit,
import the resources once before running the observer. Run from the owned review checkout.
Use a fresh sample for Trade, and reuse it for the two cold modes. Each mode keeps its reports in
the selected checkout and its save copies in a disposable namespace outside the worktree.

```powershell
$repo = (Get-Location).Path
. ./local/private-inputs.ps1
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1947/Review.ps1'
& $review -Repo $repo -Mode prepare -Language zh-CN
& $review -Repo $repo -Mode trade -Language zh-CN -Sample review01
& $review -Repo $repo -Mode cold-stock -Language zh-CN -Sample review01
& $review -Repo $repo -Mode cold -Language zh-CN -Sample review01
```

The runner uses keyboard input, live state, UI projection and error output. It reuses the
existing roughly 102 MB world read-only. Budget one native process at a time, 60 seconds and
2 MiB of receipts per process. Some successful exits produced ObjectDB warnings.
Physical input hardware and a native rare Sell or undelivered-reward buyback are **Unknown**.

## Verification boundary and remaining gaps

**Confirmed, bounded native:** the real field menu, persistent member sheet, spells, fixed empty and
duplicate slots, Give with room/full-pack exchange, equipment cancellation, confirmed Drop, Search
and return, pointer/pad/keyboard and remapped confirmation, 1080p/1440p/2160p list bounds, and long
reading with a fixed footer. The real Galam shop exercises insufficient funds, full-pack refusal,
exact sale/payment/slot changes, actual purchase and subsequent movement. Original command art and
Chinese source names are checked when their existing private publication is selected. Shop behavior
changes also run the relevant [engine tests](../../tests/Sf2.Remake.Engine.Tests/ShopServiceTests.cs).
Exact candidate identities, native receipts, completed failures and CI belong to the Issue/PR handoff.

**Unknown:** human visual acceptance, physical controller hardware, native focus/DPI/ultrawide coverage,
all campaign/script combinations, and every unusual long name/status combination. The complete-world item inspection retains a two-instance ObjectDB shutdown warning; its ownership is unresolved. Passing observations contain no native ERROR, which does not certify clean shutdown. This is bounded
F5–F7 acceptance, not a fresh continuous Battles01–07-to-Map14 playthrough. Original natural reach and
hardware timing are not established. Future content/translation changes require their actual reader
check; the stress variants do not admit unlimited command-caption/footer sizes. Original unavailable
services and the existing campaign/save boundaries remain with their capability owners.

## Current verification entry points

The [host verification route](../verification/host.md#exploration-gameplay-menu-observation) keeps
`engine_field_menu_observation.gd` current for opening/release, pause, close-edge, pointer focus recovery,
host/Save exclusion, pad shortcut, disabled Search, remap and Drop paths. It shares only input/report
helpers with the F5–F7 observer. `engine_party_details_observation.gd` retains the independent public
14-member/value/gesture/scroll witness; its recipe is in the same host owner. The
[shop route](../development-and-verification.md#galam-shop-and-supply-observation) retains
`engine_shop_observation.gd` and its supply consumer against the current list/page projection.

Use `SF2_FIELD_MENU_CASE=help` with the F5–F7 observer to open actual F10 → Settings → Help,
read the Exploration/Menus pages in the selected language and return to usable field control.
Older combined field-art observers are explicitly historical at6c2d0982 under their presentation
owner; their4×/cardinal geometry is not a current gate. No removed observation field is restored.

## Caravan formation

The [ordinary Caravan service](../application/caravan-services-design.md#current-formation-implementation)
uses the field choice and pack panels. Up/Down selects a row, Left/Right scrolls, Confirm commits the
current choice and Back returns to its owning selection or exits the root. Pointer rows/footer and
controller actions use the same commands. The member list shows current HP/MP and the selected pack;
dead members require confirmation. Bowie refusal is readable, and Depot/Item stay visibly unavailable.
Application owns phases and member validation; the view retains only focus, scroll and input release.
Each new token requires release before another confirmation; service input excludes movement,
ordinary field menus and automatic field time. Source greeting/exit dialogue completes before field
movement and Save resume. Native evidence covers keyboard/held-input traversal in both languages;
no new controller hardware or screenshot acceptance is claimed.
