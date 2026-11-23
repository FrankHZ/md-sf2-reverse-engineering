# Godot design-system preview

Run `DesignPreview.tscn` in the remake Godot project. Click a tab or press its function key to
open a sample. **L** or the language button switches between Chinese and English. The motion
button holds staged icon animation and sets Reduced Flash for the production battle. Use
directions to navigate, Enter / A to confirm and Esc / B to return.

## Scene ownership

| Tab | Current sample and state ownership | Production owner and migration boundary |
| --- | --- | --- |
| F1 Witch | Staged title, two in-memory slots, backup and replacement. Shares `NameEntryView` and entry artwork selection. | `SessionEntryView`, `SaveSlotView` and the startup/restore callers own entry and return. |
| F2 Dialogue | Staged question and reply on `StoryDialoguePanel`. The sample chooses the lines and handles advancement and answers. | `ExplorationDialogueView`, the field presenter and the current text/choice wait own delivery and acknowledgement. |
| F3 Church | Staged service rows, quote and refusal. Record enters the preview's own slots. | `ChurchServiceView` and field service admission own availability, transactions and the Record return. |
| F4 Battle | Mounts `Main.tscn` and uses live Practice Yard movement, command focus, offer, recipient and playback. | `BattleSessionView`, `BattleCommandMenu` and `BattleSceneView` own the decision, description, action and close-up. |
| F5 Members | Staged roster, numbers and preparation. Shares gauge artwork and F6's in-memory packs. Back returns to the covered sample. | `PartyStatusView`, `PartyDetailsPanel` and the preparation caller own actual roster, inventories and transactions. |
| F6 Shop | Staged Galam stock, gold, pack and quote/receipt decisions. Uses admitted icon artwork when available. | `ShopServiceView` and the field service session own quotes, commitment and return. |
| F7 Field | Staged sign, facing and field commands. Shares the staged packs with F5/F6. | `ExplorationSessionView`, `FieldMenuView` and the item/reading presenters own actual field admission and return. |
| F8 Inspect | Mounts `Main.tscn` and opens production inspection at the acting unit. Back restores the actual parent and focus. | `BattleInspectionView` owns cursor, details, roster, options and map. The embedded `GameRoot` handles Speed and Quit's return. |
| F9 Results | Staged victory, retreat and defeat messages on `StoryDialoguePanel`, followed by a sample destination. | The battle scene/story/retreat and field presenters own actual reward, acknowledgement and continuation. |
| F10 System | Staged system menu, settings draft, slots and window recovery. The covered sample pauses and resumes on Back. | `GameRoot`, `SystemMenuView`, `SettingsView`, `SaveSlotView` and the window recovery caller own real host admission and return. |

F4/F8 share the production scenes. The other tabs still need their production callers. Group
follow-up work around entry/dialogue, field/party/services, and result/host continuations.

## F4 action playback

F4 or `-- --battle` opens the Medic's Movement in the existing public
[Practice Yard package](../../../content/authored/practice-yard.json). Use **Down**, wait for the
step to settle, then **Enter** to open the vertical action list at that stop. **H** opens Magic.
Read the MEND offer in the production description area. Left/Right reads an overflowing
description while preserving the focused row.

1. Confirm MEND. Cycle to **GUARD A** with Right / the next-target input, or hover the eligible
   unit. The current friendly target card shows level and live HP/MP.
2. Use Esc or the visible Back action to return to the same spell row. Re-enter Target and confirm
   GUARD A to start the real recovery scene. A full pointer click on an eligible target also
   commits the action.
3. Advance the on-screen messages. The close-up begins with the caster card on the right, then
   shows the recipient card on the left. There is no enemy card for this allied recovery.

Press **F4 again** to reset positions and resources. The sample uses authored terrain and content,
with production text/cards at the real action phases. Original battle rasters, audio, and private
campaign progression are outside this sample. See the [production battle trace](../../../docs/godot/battle-interaction-baseline.md#production-f4f8f9-contract).

## F8 inspection

F8 or `-- --inspect` starts a fresh Practice Yard session in free inspection. Press Enter on the
acting Medic to see details, then click **Back** to return to the same cell. Move Right to empty
ground and press Enter for the option list. **Member** opens the roster, with a detail child that
returns to the selected member. **Map** returns to Options on Back. **Speed** uses the production
settings draft and restores Options when closed. **Quit** opens the production caller with the
authored profile's save refusal and a Back action.

The toolbar sits above the production viewport. F5/F10 can cover the current battle page. Back
from the cover resumes that session. Re-selecting F4 or F8 creates a fresh independent session.

## Other sample routes

- **F1/F2.** Slot 1 starts with a recoverable Bowie sample and slot 2 is empty. A new name uses
  the existing physical-keyboard editor and seven-character validation. F2 accepts clicks on text
  to advance and on a Yes/No response to answer.
- **F3.** Scan the four services, inspect Promotion, decline a Cure quote or open Record and
  return to Church. The trade and slot outcomes are staged.
- **F5.** Press Prepare on Sarah and offer her slot 2 Medical Herb. Bowie has room in slot 4.
  Chester's full pack opens an exchange. Back before confirmation preserves both packs.
- **F6.** Buy a 10 G Medical Herb for Sarah, then sell her numbered slot 4 for 7 G. The confirmed
  receipt and F5 read the same process-local inventory. Repair and Deals display their refusals.
- **F7.** Search the sign, read its message, turn away and inspect the unavailable Search row.
  Give and confirmed Drop use the same staged packs as F5/F6.
- **F9/F10.** Read staged outcomes with Enter or by clicking the message. F10 retains its sample
  settings and slot paths, including the simulated window-recovery return.

## Run from the repository root

Load the checkout's [registered local environment](../../../../docs/operations/local-private-inputs.md)
and build the adapter in Debug using the [locked SDK workflow](../../../docs/development-and-verification.md#locked-net-workflow).
Import the Godot project once in the checkout. With a disposable app-data and temp directory for
the child process, run:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
& $env:GODOT_BIN --path $project res://probes/previews/DesignPreview.tscn -- --battle
```

Replace `--battle` with `--inspect` for F8, `--field` for F7 or `--shop` for F6. Without a
sample argument, the preview opens F1. The preview instance keeps preferences in memory and
blocks player storage. The original-art selections for the staged tabs use the existing
[world/scene and entry readers](../../../docs/content/presentation-assets.md#final-publication-and-ordinary-selection).
With no private selection those tabs draw their schematic. The public export excludes `probes/`.

For a direct check, read `ReadObservationJson` from the mounted `BattleSessionView` after
ordinary input. Inspect `menu.layout` for ordered command rows, reader and card geometry,
`scene` for the actual disclosed participants, and `inspection` for caller/page, revision and
the visible Back rectangle. For pointer checks, add the 72 logical pixels occupied by the
toolbar to the production coordinates. Human visual review and physical device ergonomics
remain **Unknown**.
