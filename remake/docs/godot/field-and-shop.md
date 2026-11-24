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
  80px minimum rows and reserved markers. Up/Down follows the list and Cancel returns to the field.
  The list retains its initiating selection while a member/preparation page owns input.
- `PartyStatusView` mounts the same [member page](battle-ui-composition.md#shared-member-page-2154) as battle Member.
  The roster and selected sheet share layout, focus, scrolling, row and item-slot input.
  Field membership comes from the live Party query. Portrait, identity, vitals, attributes,
  learned spells and inventory remain grouped. Member kills and defeats share a record frame,
  and Party gold has its own frame. All three values use 48px rows, 24px horizontal insets,
  left-aligned labels and right-aligned values at 30px, including the localized unavailable
  value. The selected member supplies the records and the current party supplies gold, including
  on return from actual shop transactions. The frames sit above the fixed bottom hint panel.
  Inside that panel the reading hint takes the space on
  the left; Prepare (Magic or Item at those entries) and Close sit together at the right inset.
  Each uses its own visible rectangle for press and release. Both member callers use 3× list identity art. Direct board-unit inspection uses its compact sheet and 4× art.
- Member → Prepare uses the same right-hand list for Equip, Give / exchange, Use item, Magic and
  Drop. Its four item commands use the published Item operation frames and Magic uses the Field
  Magic frames. Captions stay visible, and Reduced Flash holds the focused frame. The direct Item
  entry retains Use / Give / Equip / Drop. Both lists use mapped Cancel or right-click for Back.
  Item/level lists, recipients, exchange slots and confirmation use the existing field consumers. Spell rows include
  their MP cost; equipment rows retain the prospective attack value. Empty, unavailable and
  duplicate items retain their actual numbered positions. Inspecting an empty slot cannot submit a
  previously selected item. Unsupported/no-effect behavior remains attributed by the real query/result.
- Give names the donor and item while showing the focused recipient's four-slot pack. A full pack
  opens its numbered return slots before the real exchange confirmation. A refusal's Back returns
  to the same provisional page and index. Committed actions refresh their query before another action.
- `ShopServiceView` shows Buy / Sell / Repair / Deals. B, Esc or a complete right-click returns
  through the current service caller. The service list yields
  to stock, member, slot, quote or result in the same region. The chosen person's four slots remain
  to its left during member, slot and quote decisions. Quotes name the item/member/price and show
  current → prospective gold, including the existing sale ceiling. A known funds/capacity/unsupported
  refusal shows unchanged prospective funds; only the revalidated session command can commit.

The four inventory positions use the full height of the right-hand column below its heading.
Rows reserve 88px before wrapping and retain the icon, marker and text columns in empty slots.
Equipment and damage labels wrap with the item name at 28px. The 196px item reader sits below the
spell list, beside the inventory. Its position and 30px body hold steady as focus changes.
The spell list scrolls independently in the space above it. Long names may grow an inventory row
and long prose scrolls inside the reader. Item and spell identity art is 48×72px in field lists.
Unavailable focus remains muted and inspectable.

[Issue #2094](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2094) is a Standard adapter
slice of the [shared reading and member design](ui-architecture.md#migration-and-independent-acceptance).
The acceptance boundary covers all four slots, stable reader and hits, summary alignment and live
balances on the [production preview](../../game/probes/previews/README.md), plus the shared Battle
Member consumer. Party membership, slot identity and transaction authority stay with their existing
queries and rules.

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

Back unwinds provisional preparation to the same member and roster scroll, then returns to the
originating field choice. [Shared roster observation](battle-ui-composition.md#reproduce-the-shared-roster)

On Prepare Magic, Left/Right and the wheel over the description read the selected spell in the
fixed reader. The row shows its nominal cost. The body describes the field effect and any lower
deduction at the caster's current MP, including free NoEffect. Target shows the chosen level
and the recipient prompt. Back from Target or confirmation returns to that level without payment.
[Spell descriptions and reproduction](battle-ui-composition.md#reproduce-selected-spell-descriptions)

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

### Prepare command icons

The [#2071 adapter slice](ui-architecture.md#prepare-command-rows) uses the existing
[field preparation rules](../application/opening-session-lifecycle.md#implemented-field-preparation).
Build the [Debug adapter](../development-and-verification.md#locked-net-workflow), import once for a
fresh checkout, and load the registered configuration. The observer uses the [published Map 16
world](../application/opening-session-lifecycle.md#reproduce-the-map16-observation). Set
`APPDATA`, `LOCALAPPDATA`, `TEMP` and `TMP` to fresh checkout-local directories.

In PowerShell from the checkout root, set `$review` to a fresh absolute directory under ignored
`local/`. Run once with `$language = 'en'; $reduced = $false` and once with
`$language = 'zh-CN'; $reduced = $true`. The observer selects the 1920×1080 and 1600×900 clients,
respectively.

```powershell
$handoff = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2035/galam-town-v1'
$package = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2035/galam-town-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $package 'entry/entry.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
foreach ($key in @('SF2_PRIVATE_BATTLE01_DATA', 'SF2_PRIVATE_BATTLE01_SCENE', 'SF2_PRIVATE_BATTLE01_TERRAIN',
    'SF2_PRIVATE_STATIC_DATA', 'SF2_PRIVATE_ENEMY_DATA', 'SF2_PRIVATE_ENEMY_GOLD')) {
    [Environment]::SetEnvironmentVariable($key, (Join-Path $package "opening-inputs/$key.json"))
}
$env:SF2_SAVE_ROOT = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/disposable-saves'
$env:SF2_SAVE_NAMESPACE = 'prepare-review-' + [guid]::NewGuid().ToString('N')
$env:SF2_PREPARE_START = Join-Path $review 'start.json'
$env:SF2_VIEWPORT_OUTPUT = Join-Path $review 'observation.json'
$env:SF2_VIEWPORT_CASE = 'prepare'
$env:SF2_PREPARE_LANGUAGE = $language
$env:SF2_PREPARE_REDUCED = [int]$reduced
$inputDoc = Get-Content -Raw -LiteralPath (Join-Path $handoff 'return-writer.json') | ConvertFrom-Json
$inputDoc.start.position = @{ x = 35; y = 26 }
$inputDoc.start.facing = 1
$inputDoc | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $env:SF2_PREPARE_START -Encoding utf8NoBOM
$settings = Join-Path $review 'settings.json'
@{ formatVersion=1; language=$language; textMode='instant'; reducedFlash=$reduced; bindings=@{} } |
    ConvertTo-Json | Set-Content -LiteralPath $settings -Encoding utf8NoBOM
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --quit-after 5200 --path remake/game `
    --log-file (Join-Path $review 'native.log') --script res://probes/engine_prepare_commands_observation.gd `
    -- --input-settings $settings
```

For the English text-only case set `SF2_VIEWPORT_CASE=prepare-authored`. Copy the tracked
`remake/content/authored/localization-pilot.json` to ignored scratch with its `start.program`
property removed. Append `--authored-package $authored` where `$authored` is the absolute path to
that file. Require exit 0, `passed:true`, empty failure and unavailable lists and no runtime errors.

**Confirmed, bounded headless.** The production host reports all five mounted command identities,
localized captions and complete hit rows. The focused art alternates with normal motion and holds
the selected frame with Reduced Flash. All five commands reach their existing choice pages. The
ineligible Use slot stays muted and its refusal keeps resources and revision. Keyboard, caption
click and mapped pad events cover selection and Back. Right-click returns to the retained member,
pane and inventory slot. Direct Magic and Item retain their distinct routes. Authored content
that admits preparation can show text-only rows. Human visual review, physical-device behavior and
desktop focus remain **Unknown** under [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).

### Field and shop

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

### Reproduce four-slot capacity and accounting

Load the checkout-local environment and build the adapter in Release and Debug using the
[locked workflow](../development-and-verification.md#locked-net-workflow). From the repository root,
prepare this controlled variant of the tracked shop sample. The fixture uses synthetic color rasters
to exercise actual identity texture layout without requiring game assets.

```python
from pathlib import Path
import base64
import hashlib
import json

p = json.loads(Path("remake/content/authored/shop-supplies.json").read_text(encoding="utf-8"))
p["start"]["player"] = "entity-0"
battle = p["battle"]
battle["start"]["gold"] = 999999
battle["start"]["actors"][0].update(kills=9999, defeats=4321)

name = "Long-handled practice supply with a reinforced training grip"
battle["items"][0]["name"] = name
for row in p["world"]["programs"]:
    for instruction in row["instructions"]:
        if instruction["op"] == "shop":
            for item in instruction["items"]:
                if item["id"] == 0:
                    item["name"] = name
battle["actors"][0]["items"] = [128 | 256, 0, 1, 3]
battle["actors"][1]["items"] = [0, 127, 127, 127]

def raster(width, height):
    raw = bytes([91, 173, 214, 255]) * (width * height)
    return dict(width=width, height=height, format="rgba8", data=base64.b64encode(raw).decode(),
                sha256=hashlib.sha256(raw).hexdigest())

for field_map in p["world"]["maps"]:
    field_map["population"] = dict(allyCount=30, nonAllyStart=128, playerSprite=0, followers=[], allySprites=[])
    for entity in field_map["entities"]:
        entity["sprite"] = 128
direction = raster(48, 24)
p["world"]["presentation"] = dict(
    maps=[dict(map=m["id"], scale=1, atlas=raster(128, 320),
               blocks=[[0] * 9] * (1 + max(word & 1023 for row in m["layout"] for word in row)))
          for m in p["world"]["maps"]],
    sprites=[dict(sprite=i, directions=[direction] * 3, portrait=None, speech=0) for i in (0, 128)],
    portraits=[],
    identityIcons={
        f"identity-icon.item.{item}.{variant}": raster(16, 24)
        for item in (0, 1, 3) for variant in ("normal", "selected")
    })
Path("local/party-capacity.json").write_text(json.dumps(p), encoding="utf-8")
```

The [sample loader](../../game/probes/PartyCapacitySample.cs) admits this package through the
ordinary authored reader and supplies the second member's unavailable records and a wide first
member name at startup. It mounts the real host. The
[observer](../../game/probes/party_capacity_observation.gd) reads native controls and sends ordinary
key and pointer input.

For each case, set `$review` to a fresh absolute directory under the checkout's ignored `local/`.
Create its `appdata` and `temp` directories before launch. Run in a PowerShell child so these
environment selections are confined to the observation.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:APPDATA = $env:LOCALAPPDATA = Join-Path $review 'appdata'
$env:TEMP = $env:TMP = Join-Path $review 'temp'
$env:SF2_VIEWPORT_CASE = 'party-capacity'
$env:SF2_PARTY_LANGUAGE = 'en'
$env:SF2_PARTY_RESOLUTION = '1600x900'
$env:SF2_PARTY_PACKAGE = (Resolve-Path -LiteralPath 'local/party-capacity.json').Path
$env:SF2_VIEWPORT_OUTPUT = Join-Path $review 'observation.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --path remake/game `
    --log-file (Join-Path $review 'native.log') --script res://probes/party_capacity_observation.gd
```

Run the capacity case at both 1600×900 and 1920×1080 with `en` and `zh-CN`. For the trade case,
set `SF2_VIEWPORT_CASE=party-trade` and append `-- --members`. It drives F5 → F6 → F5 through each
ordinary transaction and checks the balance of the same session. Use the
[member-small route](battle-ui-composition.md#reproduce-the-shared-roster) for the adjacent
battle Member sheet. Require exit 0, `passed:true`, empty failure and unavailable lists, and clean
native logs.

**Confirmed, bounded native.** At both window sizes in both languages, native rectangles and
font measurements show the full four-slot inventory at zero scroll, including the wide member name,
wrapped item name, mounted icon, equipped/damaged labels and the second member's empty slots.
The reader fits the selected text at 30px. Mouse and mapped actions select the same visible slots,
and Prepare Back retains the member, pane, slot and geometry. Member changes retain the six-digit
balance and update the personal record or localized unavailable value. The production preview
observed 500 → 490 → 497 in both languages. The existing battle Member observer reached the details
and returned to the retained roster. Larger content and physical device or human visual review
remain **Unknown**.

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
