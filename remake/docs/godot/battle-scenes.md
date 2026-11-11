# Battle Presentation

The [held-rate contract](presentation.md#held-battle-acceleration-1619-bc) owns modern 1×/1.5×
scheduling: one parent 60 Hz budget, mandatory work retained, real-time reading and normal-pitch
audio. Its [finite observations](../verification/host.md#held-battle-rate-1619-bc) cover HEAL,
story, appearance, death and EGRESS; they do not expand the original-scene claims below.

## Pre-voyage battle presentation audit

The [current campaign agreement](../application/pre-voyage-campaign.md) admits Battles01–07
through first Map14 control. Its resource/route evidence supersedes the former Battle04 frontier.
The selector table below preserves original identity; its current consumers do not imply that every
natural attack, counter, spell or equipment combination was newly observed. The
[production UI coverage](battle-interaction-baseline.md#production-f4f8f9-contract) owns the accepted
F4/F8/F9 rollout; the focused playback contract follows.

## Production playback and result layout

**Confirmed bounded remake, #1686:** compact live status cards use the accepted F4 placement.
An ordinary ally/enemy close-up puts the ally upper right and enemy lower left, clearing the actual
story reader beneath it. The lower position follows the reader's real height, including mapped hints.
Friendly HEAL/item recovery shows only the currently displayed participant: caster upper right,
then the other ally upper left at Application's actual target-switch phase; self recovery retains
one caster window. Return-to-caster phases restore its side. AURA uses the same per-recipient
projection. Physical, attack-spell, support and special scenes keep current actor/reaction-target
identity; after-turn notices retain only the actor and field deaths hide close-up cards.

Cards never predict damage, interpolate resources, pay costs or advance continuations. Their compact
mode is explicit per battle consumer; shared field/full cards keep their existing geometry. Names
retain two-line shaping and measured growth so an expanded name also reserves height above the
reader. Commands, lists, range and round/gold retire during playback. The map remains hidden for
full close-ups and visible for field-owned death/notices/return. Message paging and the actual
session's confirmation/completion tokens retain their existing owners.

The [direct production observer and initial recipes](battle-interaction-baseline.md#direct-production-reproduction)
read actual Battle01 attack/damage/death/reward and Sarah-to-Bowie HEAL, including HP recovery,
one MP payment, card switch and usable subsequent control. The existing authored BLAZE2 and AURA4
packages check real multi-target result delivery, current recipient and one cost. Long authored
identifiers exercise shaped names and decision/reader containment. The authored victory/defeat
packages enter by normal field interaction, deliver real result/growth/outcome reading and return
to usable field movement. Connected source Battle02 separately checks ordinary EGRESS cancellation,
payment and usable Map3 return. No preview timeline supplies a gameplay result.

These are state/input/projection/error observations at explicit controlled boundaries, not a full
private campaign replay or human visual acceptance. Unreached natural enemy HEAL, special proc,
item/loadout and species combinations retain their prior campaign/resource evidence and limits;
no new original-effect art or timing is inferred. Some exits still report ObjectDB leaks. Their
cause, physical-device/window/DPI coverage and final foreground appearance remain **Unknown**.

## Original target and inspection cursor publication

**Confirmed bounded remake, #1694:** the ordinary Battle01 scene and world-referenced
Battles02–07 can publish `battle-cursor.single-cell` through the existing named-raster dictionary.
`BattleInspectionCursor` renders that same admitted original image for F8 inspection and F4's
selected physical, HEAL or item recipient. Production never reads a preview file. The separate
private library's modern `hud.tactical-selection-cursor` remains a distinct SVG-derived asset;
this preparation does not promote or change that repository.

**Standard scope:** one optional presentation resource, unchanged gameplay/content/save identities,
existing publication and ordinary input selection. No new acquisition, asset platform, mouse walking
or larger original-radius artwork. `BattleSelectionView` retains the modern acting-unit cue;
`BattleActionRangeView` retains actual reach and affected cells, including multi-target effects.
The single-cell image denotes only the current center/recipient.

### Source and publication

The shared `single_cell_cursor` decoder in
[`remake_battle_scene_content.py`](../../../src/sf2tool/remake_battle_scene_content.py) verifies the
US ROM SHA256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlefunctions_0.asm:DmaCursorTiles/sprite_Cursor/tiles_Cursor` and
`displayinit.asm:palette_Base` own the source selectors. The compressed range
`0x23658..0x23757` consumes1941 bits and expands to2048 bytes. The first column-major4×4-tile
sprite and base palette at `0x309E..0x30BD` yield32×32 RGBA8, with a transparent center and
four painted corner regions. Its4096 decoded bytes have SHA256
`3F2F8EE6F6FDD1B9889FE8595666D407B58CD1D3FF7E0E33FCDD01C192EF5090` and reproduce the
[accepted private preview](../../game/probes/previews/README.md#single-cell-cursor) PNG exactly.
This is a source/pixel identity claim, not a new original-runtime observation.

The normal scene producer includes the cursor in its existing candidate manifest/source spans,
master and nearest2×/4× derivatives, and selects the32px RGBA raster for runtime. For an existing
complete world/scene publication, use the existing
[`remake_command_menu`](../../../src/sf2tool/remake_command_menu.py) command with `--target-cursor`:

```powershell
# Load the registered inputs/tools in this same process; use absolute selections and a fresh ignored output.
uv run python -m sf2tool.remake_command_menu --rom $rom --upstream $upstream --listing $listing --rom-assets --world $world --scene $scene --menu all --target-cursor --out-dir $output
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
& $env:GODOT_BIN --path remake/game
```

Keep the caller's full static/entry/world selections and explicit external save namespace. No
controlled start, preview scene or probe argument belongs in ordinary play. The
[complete publication owner](../content/presentation-assets.md#final-publication-and-ordinary-selection)
explains additive menu options. All seven selected scenes receive identical cursor bytes;
`provenance.json.targetCursor` records ROM/source/range/palette/layout/pixel identity.
An existing conflicting cursor namespace rejects before output creation. Runtime admits whole
absence for old packages, or exactly one32×32 `battle-cursor.single-cell`; wrong names/shapes reject
with `scene-target-cursor`. Public authored and old private packages retain their existing cyan
inspection frame/cross and four-edge target outline. Declared invalid artwork cannot claim fallback
success. No save format or gameplay revision changes.

### Placement and bounded acceptance

Nearest4× drawing occupies128×128px centered on a96×96px cell, overhanging16px per side.
The cursor borrows no hit area; existing board/world clipping and actual UI input ownership apply.
Invalid or canceled targeting clears it. Inspection hides decision ranges; Back restores Movement.
Playback retires targeting. A host/detail cover preserves the underlying cursor and session rather
than letting covered pointer input retarget a unit.

[`engine_battle_cursor_observation.gd`](../../game/probes/engine_battle_cursor_observation.gd)
reuses the production root, current input bindings and existing controlled-admission observer.
After a protected adapter/Debug build, set `SF2_COMPACT_START` to an ignored prelaunch input,
`SF2_CURSOR_ORIGINAL=1`, `SF2_CURSOR_SHA256` to the decoded identity above,
`SF2_VIEWPORT_OUTPUT` to a fresh report and `SF2_INSPECTION_ROUTE=keyboard` or `pad`:

```powershell
& $env:GODOT_BIN --headless --path remake/game --audio-driver Dummy --max-fps 120 --resolution 1920x1080 --script res://probes/engine_battle_cursor_observation.gd -- --input-settings $settings
```

The private controlled Battle01 start places ally0 at(8,14), HP6; ally1 at(9,14); ally2 HP0;
enemy0 at(8,13), HP1. Other values come from ordinary full-world admission. The observer uses real
input for physical, Medical Herb and HEAL selection, target cycling, invalid clicks, Back,
1080p/1440p resize, both map edges, details/host cover and subsequent actual attack/turn delivery.
Normal keyboard and Reduced Flash/pad pass. Old full private selection passes with
`SF2_CURSOR_ORIGINAL=0`; public `attack-spells.json` with `SF2_DEMO_BATTLE_CASE=public` and no
controlled start checks its target/inspection fallback. `area` and `aura` run the existing actual
BLAZE2/AURA4 consumers, preserving affected cells, ordered results and one cost.
Direct ordinary content admission checks all seven scenes, old absence and name/shape refusals.
Ordinary title Continue restores an isolated copy of a prior-package battle save at its exact
queued actor without changing either saved payload.

Measured publication is148,965,375 bytes, longest selected path120 characters in the owning checkout,
prepared in about2s. World data, other scene values and26 unchanged dependencies compare equal to
the prior complete pack. Work remains O(input plus declared dependencies), one world copy,
assumed peak<3GiB and scratch<1GiB. The decoder adds only256 compressed bytes,2048 decoded bytes,
4096 RGBA bytes and small receipts; no second large input pipeline or language copies are created.
Replan if the selected package exceeds these bounds. Old packages and all validation failures stay local.

**Unknown:** final foreground/human aesthetic approval, broader device/DPI combinations, natural
original reach/timing, and larger original-radius arrangements. These observations do not replay
the full campaign. The retained early observer failure selected the unavailable sword row instead
of Herb; selecting the actual admitted Herb row fixes the driver, without a product-rule change.
Some early/ordinary-Continue exits report existing ObjectDB leaks; their cause remains Unknown.

### Which combatants need a scene

Read each row with the previous row's **party**, subject to real membership, HP, equipment and
deployment. Enemies are listed afresh for each encounter. Numbers in parentheses are original
close-up sprite/palette selectors, independent of map sprites, party IDs and weapon item IDs.
The original column is **Confirmed** static for the US retail baseline and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. Natural levels, actions and timing are **Unknown**.
The remake column describes accepted code and the named observation only.

| Battle and evidence | Original party, starting gear and opposing species | Remake consumer, evidence and disposition |
| --- | --- | --- |
| 01 · [opening acceptance](../evidence/opening-loop-acceptance.md) | Bowie SDMN (0/0) Wooden Sword 71, Sarah PRST (1/0) Wooden Rod 85, Chester KNTE (2/0) Wooden Stick 56 vs Gizmo (22/2). | Ordinary opening through victory and usable return. Map57 movement/death sprites and the Tower close-up are bound. Player and enemy hits, Chester's live stick, friendly Herb and Sarah's HEAL have bounded private witnesses below. Preserve these combinations. |
| 02 · [formation](../../../docs/research/chapters/grans-opening/battle02-initialization-and-first-control.md), [outcomes and retry](../verification/field-programs.md#battle02-outcomes-and-retry) | Add joined Jaha WARR (3/0) Short Axe 33. Huge Rat (1/0), Ooze (0/1). | The selected Map66 board and private scene admit all four starting class/weapon pairs. Connected entry and selected outcome, defeat and retreat/retry have their own evidence. Binding a pair does not establish every attack, dodge or counter was played. |
| 03 · [formation](../../../docs/research/chapters/grans-opening/battle03-initialization-and-first-control.md), [Yeel witness](../verification/field-programs.md#yeel-guide-through-battle03) | Same battle party. Kazin may follow in the field. Huge Rat, Ooze, Huge Bat (23/0) with sleep critical. | Selected Battle02-return start connects Yeel to Battle03 control. The real bat can inflict sleep, skip an entry and release control after waking in the bounded variation. Twelve formation rows are capacity; current connected route/outcome evidence belongs to the campaign agreement. |
| 04 · [formation and weapons](../../../docs/research/chapters/grans-opening/battle04-initialization-and-first-control.md) | After Hawel add Kazin MAGE (4/0) Wooden Rod 85. Galam Soldier (2/0) Short Axe, Galam Archer (31/0) Wooden Arrow at distance 2, Huge Bat. | Connected campaign content supplies Kazin and the admitted Soldier/Archer/bat bindings. Sharing Sarah's rod never substitutes for Kazin's body or sequences; the campaign owner retains the actual route/resource evidence. The bow user here is an enemy. |
| 05 · [formation and weapons](../../../docs/research/chapters/grans-opening/battle05-initialization-and-first-control.md) | Retain Kazin. Slade follows before his formal post-victory join. Galam Knight (3/0) Short Spear at 1–2, Archer, Soldier, Galam Mage (36/0) Short Rod and BLAZE1. | The campaign admits Knight/Mage and retained Soldier/Archer on this board. Mage's ordinary physical/defensive poses still need binding even though its fire effect renders independently. Slade's scripted positioning in the Knight outcome does not require a pre-join player attack. |
| 06 · [formation and weapons](../../../docs/research/chapters/grans-opening/battle06-opening-and-first-control.md) | Add Slade THIF (5/0) Short Knife 103. Actual equipment may now include Galam purchases. Dark Cleric (40/0) Short Rod and HEAL1, Galam Mage, Soldier, Knight, Archer. | The campaign admits the post-shop loadout, Slade physical and cleric content. Authored enemy HEAL proves only its named semantic consumer, not every private cleric cast/recovery combination. |
| 07 · [formation and weapons](../../../docs/research/chapters/grans-opening/battle07-opening-and-first-control.md) | Optional joined Kiwi TORT (6/0), unarmed. Dark Smoke (22/0), Galam Mage, Dark Cleric, Galam Archer, Galam Knight. Smoke has DISPEL1, silence critical and a possible extra round entry. | The campaign admits optional TORT class/scene/field bindings and Smoke with its own palette despite sharing Gizmo's sprite index. Keep the absent-Kiwi path and story followers distinct. Victory follows ordinary faction counts. No forced Smoke cast, critical or boss kill. |

To reproduce the original selectors without generating art, use the ROM identity and table offsets
in the [ally fixture](../../../tests/fixtures/h2/ally-data-static-v1.json) and
[enemy fixture](../../../tests/fixtures/h2/core-stats-data-static-v1.json). For members 0–6 read
class, level and four packed items at `table_AllyStartDefinitions + 6 * member` (equipped bit 7,
item low seven bits). Their first class/sprite/palette triple is at `table_AllyBattlesprites + 9 * member`.
Enemy sprite/palette pairs are at `table_EnemyBattlesprites + 2 * species`; species IDs are in the
linked formation tables. These are selectors from read-only inputs. The [asset policy](../content/presentation-assets.md#product-design-and-distribution-limit)
keeps original frames, palettes, terrain, audio and derived packs private. Public visuals require
project authorship or a compatible license.

### Where private play stops

**Confirmed in production code at `a5e68a53`.** Three consumers have different requirements.

1. **Entry and board.** The private [scenario reader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)
   admits region programs for 1–3. The [scene producer](../../../src/sf2tool/remake_battle_scene_content.py)
   selects those same encounters. The [board](../../game/src/Battles/BattleMapViewport.cs) needs the
   visual map, playable region and each placed actor's field sprite from the encounter and world.
   The field-death selectors also serve ordinary movement. A close-up texture alone cannot get a
   new board to player input. For Battle04 reuse the Battle03 terrain alias and Map66 region,
   with encounter-specific placements and terrain-selected scenery. Battle05 needs Map16 and
   Castle background 8, Battle06 uses a different Map66 region and terrain-selected scenery,
   and Battle07 uses Map58 with Tower Interior 9. The original rows are linked above.
2. **Physical, friendly actions and recipients.** The [scene reader](../../src/Sf2.Remake.Content/Scenarios/BattleSceneContentReader.cs)
   admits ally sprites 0–3, one visual per class, and four enemy species (Gizmo, Ooze, Huge Rat, Huge Bat)
   bound to actual placements. The [presenter](../../game/src/Battles/BattleSceneView.cs) selects
   the displayed ally by class, checks its live equipped item against that visual, and selects the
   enemy by ActorRef. An unknown class/species rejects admission. A missing displayed actor or
   mismatching equipped ally stops delivery with `battle-scene-actor-unavailable` or
   `battle-scene-weapon-unavailable`. Galam stock includes Short Sword, Short Spear, Bronze Lance,
   Short Axe, Wooden Rod, Short Rod and Short Knife. Bind reachable **compatible** choices including
   the correct attack/idle/item/weapon frames and unequipped state. Chester's starting stick is
   range 1. A Short Spear changes both range and attack selection. Enemy equipment controls legality,
   while the current separate weapon overlay belongs to the ally. Reuse the enemy's selected visual.
3. **Enemy healing.** [HealingSceneCursor.RequireContent](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)
   requires healing resources, a cast sequence for the caster and positive idle ticks on caster and
   recipient. The selected private producer supplies admitted caster sequences, including the campaign's
   cleric. An idle/attack sprite alone never satisfies `healing-scene-content`. Retain the actual
   enemy-side switch/cast consumer and its validated operands.

These are implementation boundaries for the chapter that needs them. **Inferred** risks for the
newly connected route include a wrong identity after a recruit or equipment change, a stale target
card during a switch, or stranded input after death. The representative checks below distinguish
these risks from an observed failure. The existing 4 MiB private scene-document cap applies per
file. A chapter owner should size its selected pack before expanding generation.

### Which actions can be reused

| Action and direction | Working consumer and bounded evidence | Remaining decision or minimum check at first use |
| --- | --- | --- |
| Physical, ally → enemy and enemy → ally | [Physical scenes](#physical-battle-scenes) use the acting side's attack and the target's reaction, including dodge, damage recoil, second/counter role changes, result and death. Private samples cover actual hits in both directions. [Ranged sample](../development-and-verification.md#early-ranged-physical-observation) covers changed equip/range, distance-2 attacks and continued input with authored content. | Reuse the combat rule. For newly bound archers and knights check a legal station, actual attacker/defender and equipped presentation. For a follow-up that occurs, cards and reaction must follow the current pair. No projectile asset or exhaustive per-actor dice outcomes required. |
| HEAL, ally → ally or self | [Private HEAL1–3](#heal-spell-scenes) has cast/idle, healing fairy, recovery, recipient switch where required, restore caster, EXP and release. | Reuse for Sarah. A new recipient needs its own bound idle and current equipment, even if that member never casts. |
| HEAL1, enemy → wounded enemy | The [enemy-spell witness](../development-and-verification.md#enemy-spell-observation) reads one 3 MP payment, 8→23 HP, no enemy reward and later player movement in the authored package. | #1539 needs the private cast/idle binding above. Check enemy-side recipient switch, recovery text/card and return after a legal heal. Full-health enemies do not establish this case. |
| BLAZE1–2 / BLAST1, learned ally → enemy; BLAZE1, Mage → ally | [AttackSpellView](../../game/src/Battles/AttackSpellView.cs) draws modern fire or wind. Cards and messages carry identity and damage. [Player witness](../development-and-verification.md#player-attack-spell-observation) covers BLAZE2's ordered pair and BLAST1's single target; the enemy witness covers BLAZE1. | Keep the effects. Marks show affected targets and the current card/text identifies the reaction. Check Kazin or Mage in the actual chapter and a reached change of recipient. Recipient-specific highlighting and original spell art can wait. |
| DISPEL1 / suppressed spell / physical silence | [Silence witness](../development-and-verification.md#silence-observation) covers area results, one cost, a paid suppressed cast, expiry, effective later cast and movement. Support/suppressed scenes intentionally show messages/cards on black, with no character gesture. Physical silence uses the physical hit and a notice. | #1539 checks the displayed recipient and status/no-effect, or paid suppression as reached, then continued play. A quiet backdrop between acknowledged messages is expected. A stuck wait, wrong name or missing result would be a functional defect. |
| Sleep / poison / removal | [Battle03 sleep](../verification/field-programs.md#yeel-guide-through-battle03) is private. [Poison notices](#authored-poison-notices) have authored damage/exhaustion and lethal field removal. [Field death](#battlefield-death-batches) hides close-ups, spins/exits the actual batch and settles before subsequent control or outcome. | Retain after-turn notices and ordered deaths on their own board. Check a reached new species disappears only after its exit and the correct next actor/field accepts input. Natural opening poison remains Unknown. |
| Medical Herb / Healing Seed, friendly | [Herb](#medical-herb-scenes) has private self/other-ally item frames, switch back and recovery. The [purchased Seed witness](../development-and-verification.md#galam-shop-and-supply-observation) consumes the actual slot and restores 20 HP in its authored sample. | Reuse ordinary item, no fairy or MP. Check slot, named user/recipient and capped HP on the post-shop route. |
| Antidote / DETOX1, friendly | Same supply witness covers purchased item removing poison and a learned ordinary cast spending 3 MP. [RequireCureContent](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) checks caster/target/equipment and scenery for private DETOX. Its cast has no HEAL fairy. | Antidote consumes one slot, DETOX pays MP. Check cure versus ineffective text and preserved unrelated status when applicable. A naturally poisoned pre-voyage target is not required to prove the route. |
| EGRESS / Angel Wing, actor → retreat | [Retreat delivery](#authored-egress-delivery) uses text, a modern pulse/tone and fade, then field input. Private Battle02 witnesses distinguish spell payment (8 MP) from one Wing slot, including walking retry. | Reuse only where the encounter permits retreat. Check the actual return destination and resources through re-entry. A successful Wing never enters Herb or HEAL playback. |
| Result → next actor, victory, loss or escape | [Shared cards](#shared-action-status-cards), [message/return owner](presentation.md#battle-messages-return-and-party-status) and opening observations cover HP, death, EXP/growth/gold and resumed control. | Keep outcome ownership with the chapter. A finished animation alone cannot certify a first victory, special loss, repeat or usable field return. |

**Confirmed feedback gap, authored English.** In the retained #1527 English supply witness,
the Seed and Antidote action labels both call item use an attack. Seed's result names the recipient
and amount without saying HP. Both effects and the next movement succeed. Reproduce with
`engine_supplies_observation.gd` and inspect the `supply-scene` messages. In
`BattleSceneView.Message`, absent English source text falls back to the generic action/amount
strings. Private scenes bind item text 275 and recovery text 298. #1538 should check those actual
product labels. Correct the shared item message if the product misidentifies an action. Authored
fallback wording alone does not stall the campaign.

The modern visuals have bounded **engineering** acceptance. Whole-route legibility and personal
preference require human judgment. Original frame/audio alignment, final pixels and naturally
occurring proc order remain **Unknown**. Prior [capability results and retained failures](../evidence/engine-script-boundary-review.md#accepted-corrections-and-explicit-continuation-limits)
and the [F4/F5 repair evidence](../evidence/structure-and-quality-review.md#f4f5-bounded-repair-disposition)
keep their own scopes, including the ObjectDB exit Unknown.

## Message and outcome presentation

Action/result/reward/growth text now uses the shared native 30px blue/gold story panel and mapped
input hints. Existing scene tokens, reveal/acknowledgement semantics and source message operands
remain authoritative. New message tokens reset reading scroll; reading overflow does not acknowledge
unread content. The [E2 owner](presentation.md#battle-messages-return-and-party-status) describes
window retirement, actual field return and the optional read-only party page. No rewards are replayed.

### Battlefield visibility during close-ups

The map branch is hidden during full action close-ups and restored for ordinary control,
field-death delivery, after-turn notices and field-owned return. This matters because map sprites
use Z1 and terrain foreground uses higher priorities: a later sibling black backdrop at Z0 cannot
occlude them. Hiding the branch suppresses actual map sprite/terrain rendering without deleting
actors, textures or changing gameplay. `engine_battle_scene_visibility_observation.gd` reuses the
E1 ordinary inputs and reads native map/sprite visibility at semantic scene checkpoints.

**Confirmed headless:** the pre-fix private HEAL run fails the close-up visibility assertion;
fixed private HEAL and authored physical damage pass map suppression and subsequent control/map
restoration. The physical case also traverses field-death delivery. Reproduce with the E1 status
recipe, substituting that observer script and using `private` or `damage`; damage uses the unchanged
`stone-court.json`. Local issue1398 `run-visibility.ps1` owns fresh outputs. The failed pre-fix run
retains six ObjectDB exit warnings; fixed private HEAL retains two. These shutdown Unknowns and
windowed joypad interference are unchanged. No screenshots or original timing claim is involved.

## Shared action status cards

Command, target and action scenes share the [E1 card](presentation.md#battle-status-card-agreement).
The current [compact playback layout](#production-playback-and-result-layout) follows actual
participants: ordinary scenes project Actor/current ReactionTarget; recovery projects the single
displayed caster or recipient. Multi-target Prism and AURA therefore switch at the actual reaction
boundary, including ally/enemy switches and defeated targets. The native
[status witness](../verification/host.md#battle-status-card-observation) reads all reached Prism
TargetEnter/Reaction/ResultMessage/DeathMessage targets rather than retaining the first victim.
After-turn notices show the actor only; field-death delivery hides the close-up scene and its cards.
No speculative final HP, MP interpolation, extra acknowledgement or resource/progression owner exists.
Scene messages use the same centralized readable names; diagnostic IDs remain in observations.

## Authored poison notices

The ordinary scene view projects the typed poison notices over the battlefield with readable authored
text: fixed2 damage, then exhaustion for lethal HP. The finite no-input notice delivery uses the same
modern0.5-second presentation service as sleep; focus loss or a hidden host pauses completion.
Actual matching `CompletePresentation` releases each step. The existing battlefield spin/exit/settle
consumer displays lethal removal. No second battle scene, artificial attack, reward or original text
asset is synthesized. `BattlePresentation` displays the standalone terminal Victory/Defeat result.

The [native poison witness](../development-and-verification.md#poison-authored-observation) reads
actual state, label text, focus/hide behavior and continued usable control. This is authored semantic
presentation, with no original hardware/frame/audio fidelity or screenshot claim. Private field-death
content and sleep continue through their existing boundaries.

## Authored equipment feedback

The existing semantic battle HUD displays current equipment, ATT and physical range. At battle
`ActionChoice`, E / Pad Start opens the finite weapon selection. Directions (or E) cycle eligible
slots; Enter confirms and Escape cancels back to battle actions. Each candidate displays its slot,
name, prospective ATT and range; Unarmed appears only under the source slot-count policy.

The view submits `BeginEquipment`/`SelectEquipment`/ordinary Confirm/Cancel to the session. Its
preview does not replace authoritative actor state. Focus loss and hiding block equipment input;
resuming retains the pending choice. Confirmed equipment remains through later movement/action
cancellation. This is authored text feedback through the existing HUD, not the frozen PR692 diamond
redesign, original equipment-window graphics, pulsating original range grid or source timing parity.
The [native observation](../development-and-verification.md#battle-equipment-authored-observation)
reads actual choices, label visibility/text, state, input results and subsequent scene feedback.

## Authored AURA recovery

`AuraRecoveryView` draws a small authored pulse; the shared `BattleSceneView` cards show readable
actor/recipient names and live HP/MP. One cast is followed by each source-ordered recipient, including zero recovery.
Target-specific recovery text and one EXP message require ordinary confirmation. Cast/reaction and
switch stages complete after actual elapsed delivery; the same session then accepts next-turn
movement. AURA does not use the HEAL1–3 fairy cursor, private rasters or original animation-selector
claims. This is an intentional modern presentation boundary.

The scene clock and acknowledgement stop while hidden or focus-suspended, preserving paid resources
and the wait token. Focus re-entry resumes without catch-up or duplicate completion. Presenter errors
retain their wait. Observation reports target list, displayed recipient, HP/amount, elapsed time and
actual draw count. No screenshot or synthetic completion is used. See the
[source rules](../domain/gameplay-rules.md#authored-aura-group-healing) and
[native recipe](../development-and-verification.md#aura-authored-observation).

## Authored Prism laser

`PrismLaserView` draws a small deterministic tactical-ray diagram using Godot primitives. A purple
origin charges for0.35s, a visible beam fires for0.30s, and settles for0.25s. These are authored modern
presentation durations, not original animation timing. The actual view clock releases the scene's
ActionAnimation token; ordinary Enter input acknowledges the laser-specific announcement and results.
Empty shots use the same charge/fire/settle and complete without a fabricated target. Each subsequent
reaction highlights its real target and faction; the shared card shows that target's readable name
and current/max HP/MP. Existing field death delivery removes
the ordered list and returns control.

The semantic source selection is message319 (authored paraphrase), animation21/enemy selector95(hex),
loading sound103, firing sound108 and enemy music5. This slice supplies its own visual only: it does
not deliver or claim the original animation, those sounds/music, original assets or hardware timing.
Diagnostic `laser` fields expose actual stage/beam visibility, origin/direction, focus and targets,
plus current target side. `BattleSessionView.deathCredit` is read-only and preserves its existing
`itemCustody` field. No menu/input/layout redesign is included.

The [native recipe](../development-and-verification.md#prism-laser-authored-observation) observes
actual ordinary-input on/off-ray play, announcement, charge/fire/settle, both target factions,
ordered deaths, accounting and subsequent movement without screenshots or injected completion.


## Leader survivor field removal

The existing battlefield projection consumes `DeadActors` from the finite field continuation for
ordinary deaths and positive-HP swept enemies alike. It applies the same spin/exit phases and keeps
placed actors visible until removal commits, then hides their markers. Unplaced listed enemies
still receive bookkeeping without inventing a map position. `BattleSceneState` exposes field-only
metadata safely without a physical action, reward or displayed combat-scene sides. The presenter
chooses no source identities or kill policy.

The [authored leader-cleanup observation](../development-and-verification.md#leader-cleanup-authored-observation)
uses ordinary interaction and ATTACK input, actual spin/exit/settle state and marker visibility,
then the ordinary victory program and field movement. No Godot menu/layout or presentation code
change was required; the existing field consumer accepts the extended worklist. Original timing,
pixels and full Battle18 remain outside this modern authored observation.

## Ineffective physical feedback

`BattleSceneView` consumes the semantic `Ineffective` reaction through the existing physical scene.
It selects source message289 when admitted text exists and otherwise the authored message
"The attack is ineffective against {NAME}." The actual result label is visible and uses the
ordinary reveal/acknowledgement input. It has amount0 without presenting a zero-damage, dodge or
critical-damage result. No damaging recoil, hit sound or damage-reaction RNG is requested for it.
Supported later sleep and ordinary end EXP feedback keep their own continuation boundaries.

The [authored Taros observation](../development-and-verification.md#taros-authored-observation)
drives ordinary movement/ATTACK/target/confirm for both the wrong actor and bound equipped actor,
reads actual label/projection/HP/events/errors, and proves usable movement/cancel after both scenes.
It uses existing authored presentation with no imported Taros assets, screenshots or UI redesign.
This proves modern delivery, not original Battle18 reach, hardware timing or full encounter parity.

## Authored EGRESS delivery

The selected retreat does not construct a `BattleSceneState` or play an ordinary attack scene.
`BattleRetreatView` projects `ActiveBattle.Retreat` through the existing battle host: actual text
reveal, fresh Confirm acknowledgement, a soft blue effect and authored tone, then a close fade.
The scene remains in battle until the matching close completion; field return uses the ordinary
exploration presenter. Existing menu/input layout is unchanged.

Angel Wing uses the same consumer with a distinct item payment and `CHESTER used Angel Wing!` text
for the controlled native party. Retreat resolves the existing active scene's admitted member name,
using the actor's processing order just like the ordinary scene presenter; authored packages without
that content retain the actor ID fallback. Shared EGRESS formatting uses the same name (Bowie in the
private start). There is no separate naming registry.
Normal Item input cycles held slots and selects the eligible self target. The projection exposes the
actual selected item payment; `paidMp` is absent for Wing. Inventory is already compacted before the
text request. It never claims a spell cast or starts the Herb recovery scene. The same authored tone,
pulse and fade are a deliberate modern approximation for both exits. The
[Wing observer](../verification/field-programs.md#battle02-angel-wing-and-walking-retry) checks actual
item input/cancel, duplicate delivery, focus/process suspension and retained inventory through walking
retry. It does not inject successful completions or acquire original media.

`authored-egress-tone-v1` is project-authored PCM16 mono at 22,050 Hz, 7,718 samples, with a sine envelope
and rising phase `2π(440t+220t²)`, amplitude 12,000 and playback −18 dB. The pulse lasts 450 ms and closure
200 ms in this modern presenter; these are not original timing or recorded audio. Reduced-flash mode
uses a low constant tint. Instant/adjustable text settings govern delivery only. Original EGRESS
sound/flash/pixel parity remains **Unknown**; no new capture/import platform or unrelated source cue
is substituted.

Focus loss pauses retreat input, elapsed delivery and its tone. Process suspension cannot complete
a wait. The presenter emits one completion per token; the session independently rejects duplicates.
`RepeatRetreatDeliveryObservation` can replay only a completion already emitted by the real consumer
for the bounded rejection observation; the successful acceptance route uses actual delivery.
The [native recipe](../verification/field-programs.md#battle02-egress-and-walking-retry) reads real text,
effect/tone, tokens, paid resources and field/next-battle control without screenshots.

Application owns staged gameplay publication and wait tokens. Godot consumes the current scene,
movement and death state, displays admitted resources and submits actual completion. Rule selection
and legality remain in the [engine](../domain/gameplay-rules.md); runnable observations belong to
[battle verification](../verification/battles.md).

Private rendering remains bounded to admitted encounter, actor and scene resources. `BattleSceneView`
matches source class/actor visuals and checks the equipped source item against the admitted weapon
visual; it also chooses source ground/animation presentation. This resource compatibility is not
general mod/schema support and does not grant Godot gameplay mutation or provenance authority.
Missing private resources fail visibly; they do not silently use authored gestures.

## Battlefield Maps

The ordinary private Battle01–03 board consumes the admitted exploration atlas, nine-tile blocks
and layout through the same `MapBlockTextures` decoder as exploration. Content retains an optional
`BattleMapRegion` on each battle definition, including its visual-map identity and source-cell origin;
growth and outcome definition copies preserve it. This is presentation metadata: gameplay positions,
terrain costs and ranges remain local battle coordinates. The existing `BattleDefinition.Map` identity
is unchanged. A missing region is distinct from the valid origin(0,0).

Battle01 uses map57(0,0),16×20; Battle02 uses map66(16,19),22×18; Battle03 uses
map66(0,0),22×30. Each source24-pixel block maps to the existing40-pixel board cell. Board framing
applies one translation/scale to tiles, sprites, movement and selection. Ground/foreground tile
passes retain the source priority bit: low tiles at z0, actor sprites at z1, high tiles at z2,
movement indicators and the cyan accepted-target outline at z3. A rejected target candidate never
changes that outline; cancellation removes it. The connected regions have no separate packed
foreground offset and no high-priority tiles; the affected exploration paths retain their existing
foreground behavior. Fixed plane offsets are supported; unconsumed plane motion fails explicitly.

Private missing map/region/block resources fail through the existing adapter error UI. A failed
hidden BattleLoad retains its wait and cannot report completion. Private startup requires a
prepared world with map resources; standalone private inputs without those resources no longer
draw placeholder terrain. Public authored practice retains its intentional colored terrain.

The battle view is retained and reparented during outcome/field handoff. Cached textures survive
transient tree exits; rebuilding for a different definition releases the old board nodes/textures,
and final owner destruction releases current resources. See the
[bounded native recipe](../verification/battles.md#battlefield-map-observation) for actual input,
draw/resource geometry, return/reentry and negative-case observations. This establishes bounded
rendering and lifecycle behavior, with no original pixel/timing parity or human aesthetic approval.

## Physical Battle Scenes

The common session separates physical action construction from scene execution. Domain prepares
ordered first/second/counter reactions and the action award. Application owns each continuation and
wait token: initialization, action text, animation, HP reaction, result/death text, EXP credit and
text, growth and its notices, gold text, then scene end. HP changes at reaction entry; EXP credit
precedes its text and growth follows acknowledgement. Kill/defeat accounting, after-turn, outcome
programs and the next actor wait for scene end. Wrong, stale or unrelated input cannot release a
wait. AI passes its prepared action to this same consumer without selecting or rolling twice.

**Confirmed, static source and engine boundary:** the pinned US baseline and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` support construction/replay separation in
`disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`,
`battleactionsengine_2.asm`, `attack.asm` and `battlescenes/battlesceneengine_0.asm` beneath that battle
source directory. The admitted physical damaging reaction consumes twelve pairs of shared-main RNG
draws (range5 for the ally and range7 for the enemy). Source loops `loc_18E6E`/`loc_19004` follow the
reaction-mode and `d1 == 0x8000` guards; this direct-draw rule is not every hit or scene's total RNG.
Growth consumes the carried main image after
those reactions; an old scalar comparison that omitted scene draws is not final-session parity.
`BattleSceneTests` checks phase boundaries, ordered followups, continuous reaction seeds, growth,
deferred accounting and input release; migrated action tests retain their construction assertions.

The private source content candidate contains the selected SDMN/PRST/KNTE and GIZMO frames,
Wooden Sword/Rod and Wooden Stick, Tower Interior background9 and ground9. The generator composes
source hardware tile chunks and background layout, preserves source frame timing/weapon flags,
and writes the existing pack-v1 master/2x/4x structure plus `battle-scenes.json`. Raw palette
provenance remains intact; displayed base CRAM words use mask `0x0EEE`, including the retail low
bits in words `0x0DB0` and `0x0E50`. Canonical inputs remain read-only and generation only accepts a
new ignored worktree-local output. This is a review candidate, not asset-library promotion.

After loading the [private input/tool selections](../../../docs/operations/local-private-inputs.md),
use absolute selected ROM/upstream paths and a fresh ignored destination:

```powershell
uv run python -m sf2tool.remake_battle_scene_content --rom $selectedRom --upstream $pinnedUpstream --output $candidateOutput
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $candidateOutput 'battle-scenes.json'
$env:SF2_BATTLE_SCENE_OBSERVATION_OUTPUT = Join-Path $observationOutput 'observation.json'
& $env:GODOT_BIN --path remake/game --audio-driver WASAPI --script res://probes/engine_battle_scene_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
```

Build the actual adapter first using the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
The isolated scene consumer/generator uses `uv run sf2 verify plan --scope engine --base origin/main --head HEAD`
on a clean committed head. Its exact generator path selects engine behavior, adapter compilation and
the existing direct public checks; research inputs still reject that explicit scope. This classification
does not extend to other generators or change research verification requirements.
The normal host also consumes this content through `--private-exploration-start`; source private
physical scenes fail explicitly when content is missing. The external observer reads actual node
resources, frame/position state, session effects and audio receipts, and submits ordinary keys.
It accepts `SF2_BATTLE_SCENE_WORLD=1` for the controlled Map40 navigation/warp route with an admitted
source world. `SF2_BATTLE_SCENE_REWARD=1` additionally exercises the adjacent player attack and
requires an explicitly prepared party with Bowie HP40, attack30, defense20 and EXP99. These are
controlled consumer observations, not original natural-entry fixtures. World, party and output
paths remain private. No screenshots or runtime state setters are used.

**Confirmed, bounded host observation:** source actor/weapon/environment textures, the 22-iteration
entrance projection, message waits, enemy and player hits, death/EXP/growth/gold, music2/5, SFX81/83/102,
253 requests and resumed battlefield music execute through the running Godot/WASAPI consumers.
Normal/reduced-flash runs preserve the same reaction draws, final seeds, resources and cursor while
the setting suppresses recoil/flashing. Battle text clears speech bleeps, matching source bsc10/11.
The host uses the existing modern half-second fade and instant/adjustable text settings. Hardware
timing, exact final pixels, continuous opening/outcome acceptance and original audio mixing remain
**Unknown**; these observations grant no H4 or deviation waiver.

### Weapon frame composition (#1620)

**Standard scope:** diagnose the reported knight weapon/body overlap using the accepted private
frames and pinned source; correct the responsible projection and retain legal front/back layers,
frame selection, flips, offsets, hit delivery and the separate battle UI. No asset replacement,
world occlusion redesign, original frame capture or full-weapon survey belongs to this slice.

**Confirmed, static:** at SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, beneath `disasm/`:

- `data/graphics/battles/battlesprites/allies/animations/allybattlespriteanimations.txt` defines
  weapon layer1 as under the character and layer2 as over it; bits4/5 of the frame byte mirror/flip,
  and signed weapon offsets are relative to the current body position.
- `code/gameflow/battle/battlescenes/battlesceneengine_0.asm`,
  `LoadBattlesceneAllyVdpSprites` writes toggle1 for ally slots1–9.
  `LoadBattlesceneWeaponVdpSprites` / `loc_1958E` writes `0x01010101 << (layer-1)` for slots10–13.
  `code/common/tech/graphics/graphics_2.asm:sub_1942/sub_196C` links toggle2 before toggle1,
  and increasing slot order within each group. Layer1 therefore follows the body in the sprite
  chain; layer2 precedes it. This agrees with the format, rather than inferring priority from the
  name of the field. `battlesceneengine_4.asm:sprite_BattlesceneWeapon` preserves the four 32×32
  chunk arrangement and the horizontal/vertical flip combinations within the same 64×64 footprint.
- `code/gameflow/battle/battlescenes/getallyanimation.asm:GetAllyAnimation` selects KNTE sequence2 for the stick/lance
  and special80 for the Short Spear. USE_ITEM and dodge retain their distinct selectors.

The [accepted animation fixture](../../../tests/fixtures/h2/battle-sprite-animation-static-v1.json)
pins the US ROM identity and ally pointer table `0x1AAC3A`. Read only the selected pointer and
`8 * (count + 1)` bytes: sequence0 at `0x1AAD96` (40 bytes), sequence2 at `0x1AADDE` (32),
sequence42 at `0x1AB2FE` (24), sequence80 at `0x1AB68E` (32). Their decoded operands match the
existing selected scene's corresponding sequences. No private payload or generated art is tracked.

| Selected combination | Body and weapon operands | Layer interpretation |
| --- | --- | --- |
| KNTE, Wooden Stick56 / Bronze Lance73, idle | Body0; weapon0, no flip, relative(12,-1) |1, behind body |
| KNTE, sequence2 attack | Body3 at(-2,0), then hold15 at(-10,0); weapon0 at(-7,-6), no flip | Both1, behind body |
| KNTE, Short Spear72, sequence80 | Body4 at(0,0), weapon0 at(-23,-16); hold15 with weapon at(127,127) | Both1; preserve the source off-body placement |
| KNTE dodge42 | Body2, weapon0 at(16,-7), no flip |1 |
| SDMN, Wooden Sword71, idle / sequence0 | Idle weapon17 (frame1 + horizontal flip) at(-16,10); attack weapon1,0,50 (frame2 + both flips), including body hold15 | All selected rows1 |

**Confirmed, bounded remake:** the adapter formerly projected layer1 at Z3 above the body's Z2;
the actual knight idle reproduced that mismatch. `BattleSceneView.Weapon` now maps layer1→Z1 and
layer2→Z3. It keeps the raw frame mask, flips, signed offsets, body animation and resource binding.
This changes the meaning of the admitted weapon-layer field only, not canvas ordering or all scene Z.
The selected early KNTE/SDMN rows contain no layer2 pose. A separately labelled valid-content layer2
variation checks the front path; it is not an original pose or original-runtime witness.

The [native observation](../development-and-verification.md#weapon-composition-observation) owns
actual input, node operands, hit/return and UI checks. The precise character/frame in the user's
private image remains **Unknown**; the reproduced inversion is a concrete cause of weapons covering
the body, not proof that every reported pixel had this cause. Original natural reach, VDP output,
all later weapons, hardware timing and human/aesthetic approval remain **Unknown**.

### Medical Herb scenes

The admitted item family uses the same typed scene continuation. Inventory compaction and its two
range16 award draws occur during construction; HP/MP remain unchanged until the recovery command.
Recovery applies HP once without spending MP. The actor earns EXP after recovery text and restoration
of the actor's displayed side, then growth follows the EXP acknowledgement. Missing growth rejects
the whole preparation before inventory, movement or RNG can publish. Self-use, another living ally,
full HP and PRST/non-PRST awards share this path. Application supplies the displayed ally and absent
enemy, including outgoing/incoming target and actor phases; Godot never represents an ally as an enemy.

**Confirmed, static caller selection:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the following chain beneath `disasm/` distinguishes a
Medical Herb from casting the HEAL spell. This consumes the existing
[battle-scene](../../../docs/design/contracts/battle/battle-scene-presentation.md) and
[action-construction](../../../docs/design/contracts/battle/battle-action-construction.md) contracts; it does
not extend their natural timing claims.

| Source / symbol | Selected behavior |
| --- | --- |
| `data/stats/items/itemdefs.asm`; `code/common/stats/itemstats.asm:GetEquipmentType` | Medical Herb is non-equipment; equipment type is zero. |
| `code/gameflow/battle/battleactions/getspellanimation.asm:battlesceneScript_GetSpellanimation` | Non-equipment skips the use-spell animation field, retaining `SPELLANIMATION_NONE=0`. HEALIN's effect calculation does not imply its fairy animation. |
| `battleactions/createbattlesceneanimation.asm`; `battlescenes/getallyanimation.asm`; `battlescenes/battlesceneengine_0.asm:sub_184B0` (under `code/gameflow/battle/`) | USE_ITEM type2 selects the ordinary class sequence and preserves the caller's selector, rather than substituting its header default. Candidate headers remain raw; item selector zero belongs to the action. |
| `battlescenes/battlesceneengine_2.asm:SetupSpellanimation`; `battlescenes/animation/nothing.asm` | Selector zero dispatches to Nothing, which returns without fairy setup/update. |
| `battleactions/battleactionsengine_1.asm:InitializeActors`; `animateaction.asm:SwitchTargets`; `battleactionsengine_2.asm:battlesceneScript_End` | Begin with the actor alone; switch to another ally before recovery and back to the actor before EXP. Self-use needs neither switch. |
| `battleactions/castspell.asm:spellEffect_Heal`; `battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction` | Mode2 applies recovery then sends SFX113 and returns; no damaging-reaction recoil loop. Message298 follows recovery; item message275 names the consumed item. |

Recheck any named source directly using the selected read-only upstream root, for example:

```powershell
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battleactions/getspellanimation.asm'
& git -C $pinnedUpstream show 'c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlescenes/animation/nothing.asm'
```

The source starting Chester loadout is `WOODEN_STICK|EQUIPPED` in `allystartdefs.asm`:
word184 masked by127 gives item56. `getweaponspriteandpalette.asm:table_WeaponGraphics` at ROM0x1F9E2
selects sprite10/palette19 for item56. The class KNTE sequence is index2, including item type2;
weapon sprite10 does not select the spear-throw specialization. The candidate admits this actual
starting binding. Runtime checks the live equipped item against the candidate and rejects missing
weapon support; it does not assign a class-wide weapon or silently replace equipment. An older
Short Spear candidate does not establish starting Chester presentation. Preserved physical observations
only prove the actor/loadout combinations they actually reached.

**Confirmed, bounded host observations:** ordinary keys consume source item frames/text, one-sided
status/resources, target/actor switches, SFX113, EXP/growth and return to battlefield control. The
Map40 source-world startup heals the party before battle, so that observation covers full-HP use;
a separate ordinary-input battle run first obtains actual enemy damage, then covers wounded
self/other-ally recovery without claiming world audio.
Normal/reduced-flash source-world runs agree on gameplay, inventory and bounded RNG records. A narrow
Chester physical observation consumes the corrected live Wooden Stick resource and sequence2.

The switch projection uses `bscWait $1E`, `bsc07_switchAllies` coordinates and
`SwitchAllyBattlesprite`'s capped16 steps through the existing presentation timing mechanism.
DMA/VInt/input/audio alignment remains **Unknown**. Neither those durations nor recovery sound
playback add invented shared-RNG opportunities. No fairy or24 recoil draws means only those two
branches are absent, not that the whole scene has zero RNG. HEAL continuous-opportunity parity and the pending waiting-policy
decision remain separate; no original timing/pixel/H4 acceptance or asset-library promotion follows.

For the affected engine behavior, select `MedicalHerbTests|BattleSceneTests` with the locked SDK's
`dotnet test --filter` (fully qualified names), then build the adapter. The same committed engine-scope
planner applies. The existing native observer accepts `SF2_BATTLE_SCENE_HERB=1`; it uses Chester
self-use and Sarah-to-Bowie only as controlled observation inputs, not gameplay legality rules.
Select the normal source battle inputs and the fresh candidate as above. For the source-world/audio
pair, copy the tracked opening party to a fresh ignored JSON, retain
all inventory words and set Sarah EXP99. Set `SF2_PRIVATE_CONTROLLED_START` to that absolute path and
`SF2_PRIVATE_EXPLORATION_CONTENT` to the existing world without copying it, and set
`SF2_BATTLE_SCENE_WORLD=1`. Use `--private-exploration-start $explorationStart` with the controlled
Map40 boundary below, plus a fresh output path for each run. The world program's healing is retained.

```json
{"formatVersion":1,"controlledBoundary":"Controlled Map40 scene-consumer observation; not natural entry","start":{"map":"map-40","player":"entity-0","position":{"x":4,"y":30},"facing":1,"speed":32,"flags":[0,1,2,32,33,34,401,451]}}
```

Pass `--input-settings $settingsPath` containing `{"formatVersion":1,"reducedFlash":true}` for the
paired accessibility run. `SF2_BATTLE_SCENE_CHESTER=1` instead selects the narrow physical binding
observation with ordinary Tower entrance movement; its controlled party retains the starting
inventory and gives Chester HP/maxHP40 and defense20. For positive recovery, set `SF2_BATTLE_SCENE_HERB=1` and
`SF2_BATTLE_SCENE_WOUNDED=1` with the same controlled Chester HP/maxHP40/defense20 party,
Sarah EXP0 and `--private-battle-start $partyPath`. The observer walks Chester into actual enemy
attacks, brings Sarah alongside using ordinary movement, then uses Sarah's herbs on Chester and herself.
Each action must begin below live maxHP and increase HP. Supplying a low starting HP alone does not
establish injury: the observed battle initialization restores HP. Chained enemy scenes are identified
by their new initialization token and the preceding scene-ended event, without requiring a hidden
frame between scenes. These modes use the same fresh-output/error
contract and no screenshots, original emulator, runtime state setters or fixed comparison seeds.

## HEAL spell scenes

The bounded HEAL1–3 consumer prepares two award draws, then retains scene/turn ownership through
MP cost, casting, optional ally switch, HP recovery, recovery text, ordinary idle, fairy retirement,
cleanup, actor restoration, EXP/growth and input release. Full HP still casts and awards the healing
minimum; self-healing preserves the already charged caster MP. Growth consumes the seed carried
through the scene, not the construction seed. Private scenes require admitted PRST cast content,
idle parameters and healing rasters; authored packages use an explicitly authored small gesture.
Private AURA and HEAL4 remain unsupported. Public authored AURA and the
[finite attack, support and cure spells](#which-actions-can-be-reused) use their own delivery paths.
The historical #523/#437 records do not establish full scene or hardware parity; the
[current composed milestone](../../../docs/design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone)
has its separately bounded acceptance.

`HealingFairy` implements source integer word arithmetic, signed motion residuals, wing/dust state,
level-dependent one/one/two instances, setup ranges32/30/12 and conditional update draws. Lifetime
FFFF decrements before update. X-boundary ranges28/32 and periodic dust12 can co-occur; phase3
retirement under control2, clear-before-cleanup-wait and later restoration remain separate boundaries.
The existing private scene extractor supplies source body/wing/dust rasters and PRST cast sequence
without promoting assets. Godot projects that state and plays source SFX77 at cast setup and113 at
recovery. Reduced flash and text reveal alter delivery only.

The scene-local `HealingSceneCursor` separates mandatory operations, explicit neutral timed-input
reads and host delivery. Its logical message setting is speed2/no-messages0, independent of modern
text settings. `bsc10` tests acknowledgement before the next VInt opportunity; a neutral read advances
one opportunity. Neither reveal nor acknowledgement injects W2/range256 draws. Ordinary field
`WaitAtInput` and plain-dialogue `WaitForText` retain their own eligibility guards. Scene commands
require the current session, revision and scene token, with exactly one logical step per command.
Completion delivery may arrive early or late, but cannot add steps after logical completion.
For HEAL action/recovery messages, `CompletePresentation` reports text readiness independently of
`Acknowledge`. The source speed2 neutral timeout completes without an additional confirmation once
text is ready; if reveal finishes later, its delivery alone releases the expired message, with no
new logical opportunity or RNG. The host reports readiness at the timed-input boundary or after
timeout, and idle/reveal callbacks do not supply neutral Wait. Early acknowledgement still returns
before the next input-loop VInt. Reward/growth messages retain their existing acknowledgement policy.

Source owners are pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlescenes/animation/healingfairy.asm`, `animation/update/healingfairy.asm`, `battlesceneengine_0.asm`
(`bsc01`, `bsc05`, `bsc0B`, `bsc0D`, `bsc10`), `battlesceneengine_1.asm`, `common/tech/graphics/graphics_1.asm`
(`sub_179C/table_1840`) and the window/text helpers identified in the
[retained HEAL evidence](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#retained-heal-consumer-evidence).
Paths start below `disasm/code/gameflow/battle/` except the common helpers. PR550's accepted
prepared-04 instrumentation uses execution `3e69c2d49c8105c0e485dcbea70c688b57a86902` and its explicitly
rebound Issue515 prepared-11 parent. It does not reinterpret the incompatible older parent.

### Separate comparison boundaries

**Confirmed — function comparison:** `local/issue523/heal-scene/comparison/Compare.csproj` calls the
production kernel with HEAL1 and the externally observed post-award seed5C0D0000. Setup produces
seed00920000 and the observed properties. It then supplies exactly one update per recorded
`heal:fairy:after` and requests control2 from the trace's stop/control events. All244 returns match
property words, lifetime/control/toggle, dust age/frame/clock and carried seed, including29 conditional
update draws. Counts are comparison outcomes, not production budgets. This proves the update function
for that externally supplied opportunity/control schedule; it does not prove that the application
admits the original schedule, sprite coordinates/pixels, or every possible entry state.

**Confirmed — completed continuous-cursor comparison failure:**
`local/issue523/heal-scene/cursor-comparison/Compare.csproj` runs the production cursor from the same
external post-award seed, admitted PRST cast/idle content, recovery text `Sarah recovered\n6 hit points.`
and neutral timed reads. Subsequent opportunities and control2 come from the cursor, not trace events.
The first named divergence is recovery `bsc0B`, prepared-04 CP2059–2091 (one-based checkpoint lines),
observer frames32364–32367. Entry has actor/target1, HEAL1; the source action supplies HP+6, MP+0,
status0, reaction mode2. At CP2059, a6=FF0022, seed00920000, control1, lifetimeFFFE, toggle1,
fairy age2/phase7/delay23/dustClock1. Source `OpenAllyBattlesceneMiniStatusWindow` contains an explicit
move/VInt and a movement-end wait. The current cursor ends recovery after two opportunities with
lifetimeFFFC, age4, delay21. The observed caller returns after three, with lifetimeFFFB, age5,
delay20 (CP2084/2091). Neither has drawn yet, so both seeds are00920000 at this first divergence.

At recovery text return, the cursor has seed43EE0000/lifetimeFF80; original CP3458/3459 has
D8800000/FF7F. This is an actual intermediate RNG divergence, not a waived display difference.
At make-idle exit the cursor has seedECE20000/lifetimeFF6A, while CP3727 has ECE20000/FF65;
the equal seed does not restore the missing state/opportunity correspondence. At final stop return,
both reach seed8E2D0000/control0; this particular capped recovery and construction-time award remain
HP5→11, MP10→7, EXP+13. That endpoint coincidence does not establish general result equality:
other seeds, content or stop phases may alter conditional draws and later gameplay/growth.

The PRST idle header is20. `bsc05` performs a real frame load on each side of Sleep; each ends in
`WaitForDmaQueueProcessing`, whose helper unconditionally branches to `WaitForVInt`. The cursor
therefore retains those two operations around the content-provided sleep. Weapon queue enable and
`sub_1942` do not themselves wait. The observed26-frame bsc05 interval is not a source sleep value.
An earlier hand-calculated153-versus154 estimate incorrectly used that observed interval; it is
superseded by direct cursor execution:149 active opportunities before stop versus154 observed.
No149/153/154/26 budget or seed padding is used in production.

**Confirmed — bounded opportunity readback:** the
[research attribution](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#recovery-and-make-idle-opportunity-attribution)
records three top-level recovery VInts: the first has A6=`FFFFDE80` (the source number buffer), the
next two have restored script A6=`00FF0028`. Source numeric conversion has no waits; window movement
has an explicit VInt followed by a wait that repeats conditionally until movement ends. Make-idle
has26 top-level VInts: four with stack A6 in two pairs, two with restored script A6/d0=`$900`, and20
with the Sleep counter19…0. The source performs two decompressions and DMA waits around Sleep(20).

**Inferred:** recovery's extra opportunity interrupts numeric window construction; make-idle's four
extra opportunities interrupt the two decompressions. These are CPU-work interruptions outside the
explicit waits, not evidence for fixed +1/+4 waits. The pinned source/register mapping and exact
readback command belong to the research owner. The audited `HealingSceneCursor` opportunity rules
are unchanged between the retained comparison at8e6881ea and accepted PR557 main
`bfcb819fe61cf6b7f2a3a45822f6de51e411e559`; its neutral timeout correction changes completion handling,
not these opportunity rules. The completed continuous-cursor **FAIL** remains.

**Unknown — gameplay-opportunity admission:** exact interrupted PCs, live busy/window/graphics gates,
cross-state/content timing, and a general semantic treatment remain unproven. Conditional window
completion alone does not account for interruptions during CPU work. Wider content/gate coverage
and full continuous scene conformance remain Unknown at the [retained comparison boundary](../evidence/retained-comparisons.md).
[ADR0010 Option A/8D](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)
still controls shared RNG and selected gameplay results. This attribution does not waive the
intermediate RNG difference or establish that hardware simulation is necessary. A next proposal
must identify concrete gameplay effects and feasible semantic handling under that policy; the
observed counts alone supply neither a new product decision nor permission for new capture.

**Confirmed — separate native settings comparison:** actual Godot inputs in the existing scene
observer exercise full-HP HEAL3, wounded other-target HEAL1 and wounded self-target HEAL2, alongside
physical scenes. Each completes real fairy retirement/cleanup and releases input, with actual77/113
audio and live projected nodes. Normal/instant versus reduced-flash/adjustable text uses the same
semantic inputs; final `native-timeout-normal-01` / `native-timeout-reduced-01` records each finish
six scenes and match all2311 ordered semantic events, RNG and final resource/turn state (delivery
metadata excluded). Both action and recovery messages also exhaust65 neutral inputs with zero
Confirms after natural text readiness; their final-input receipts contain no extra RNG or delivery.
Later HEAL cases retain early acknowledgements. This proves bounded settings conformance, not original neutral-input conformance.
The reproduction and completed failure records belong to the
[verification owner](../verification/battles.md#heal-scene-verification).

## Battlefield death batches

The finite field continuation retains ordinary action deaths on their battlefield cells until
its field exit completes. The explicit authored leader binding also appends positive-HP enemies,
including unplaced entries; see [leader survivor removal](#leader-survivor-field-removal). The close-up panel hides after `scene-ended`; the same action still blocks
input, automatic advancement and outcome programs. The defeated hook runs before the batch, then
all members turn twelve times, one sound116 request starts the three effect63 directions, and only
then are positions/status and kills/defeats cleaned. A final settle releases the action. Empty batches
have no presentation or sound. Old corpses are never rediscovered by an HP-zero roster scan.

The private board binds original map sprites through source ally/enemy assignments, using the
selected world's `ExplorationVisuals.Sprites`. The selected scene document supplies only the missing
63 direction sheets and field selectors. These are mapsprites, distinct from close-up battlesprites.
Settled units use a static field pose in the diagnostic board; full battlefield composition, terrain
art and ordinary idle animation remain outside this consumer. The field phase freezes each dead
sprite's first walking frame, while whole-batch direction changes select up/side/down sheets and
right-facing mirroring. Existing whole-PCM116/BD playback may outlast modern visual delivery; its
sample count does not control gameplay or RNG. Reduced animation may shorten display duration.

**Confirmed static source boundary:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/battleloop/processkilledcombatants.asm` (`0x24518..0x24642`, sound caller
`0x24574`) supplies the list traversal, facing sequence, effect63, and cleanup ordering.
`sf2enums.asm` defines twelve turns/Sleep3, effect direction1/2/3 with Sleep8 and final Sleep10.
`entityscriptengine_1.asm:VInt_UpdateSprites` uses signed ANIMCOUNTER=-1 to freeze frame1;
`entityscriptengine_2.asm:ChangeEntityMapsprite` supplies direction-sheet loading. The host explicitly
models those presentation stages, not an asserted universal seventy-VInt timeline.

The bounded RNG argument follows `executeindividualturn.asm` -> `LoadBattle` ->
`PositionBattleEntities` -> `eas_Standing/eas_Idle`: ordinary combatants are stationary and execute
speed/flags setup followed by wait/branch without random operations. Battle01's neutral Mist Demon
and Astral also select `eas_Standing` in `data/battles/global/battleneutralentities.asm`.
`SetBaseVIntFunctions` restores battlefield entity/map/view/scroll/sprite/window/map-animation
services, not the close-up reaction/fairy RNG callbacks. Death animation itself changes entity
facing/graphics with zero travel and frozen animation counters; it does not start random-walk or
random-branch scripts. No gameplay RNG call is added at a field delivery completion.

**Unknown:** exact interrupt opportunities, initial sprite-load queue occupancy, asynchronous service
interleavings and hardware timing. The source can add waits when queued sprite loads reach seven;
those are not an unconditional frame allowance. Source timing, natural multi-death, special entity32,
random scripted battlefield entities and after-turn status deaths are not established by this
bounded consumer. Current physical actions produce at most one casualty; the explicit authored
leader sweep supplies a real multi-member field worklist without inventing multi-kill attacks. Option A, 8D/H4 and the retained
HEAL recovery opportunity mismatch remain open; no padding, trace-count fitting or reseeding is used.

Actual enemy-death and ally-counter-death normal/reduced consumers exercise sprite63,116BD and
cleanup/control release with equal ordered semantic events and final gameplay state within each
pair. The counter uses the finite selection policy above: music2 requests timerC6 and the selected
pack's unique81/CC recording plays unchanged. The original missing81/C6 failure remains retained;
modern PCM reuse resolves host delivery, not hardware-exact counter audio. Direct adapter observations
cover exact preference, unique reuse, named playback, missing/ambiguous rejection and actual finite
completion without gameplay changes. The world/audio pack and #517's116 recording remain unchanged.

## Battlefield selection evidence

**Confirmed, static source only:** the USA baseline and SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` separate moving an actor, blinking that actor,
displaying a range grid and selecting a target. The [baseline identity and evidence limits](battle-interaction-baseline.md#evidence-and-reproduction-boundary)
apply. These source facts inform [#1617's modern selection treatment](presentation.md#battle-selection-and-preview-feedback-1617);
they do not establish original screen colors or timing.

| Pinned source beneath `disasm/code/` | Directly supported boundary |
| --- | --- |
| `gameflow/battle/battlefunctions/battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput` | `@Start` clears actor blinking before `ControlBattleEntity`. A legal accepted position sets blinking before the action menu. Menu Cancel rebuilds movement range and returns to `@Start`. This is not a persistent selected-actor palette replacement. |
| `gameflow/battle/battlefunctions/battlefunctions_0.asm:ControlBattleEntity`, `SetEntityBlinkingFlag`, `ClearEntityBlinkingFlag` | Movement controls the actual entity, updates its position/mapsprite and returns chosen coordinates. The blink helpers set/clear `ENTITYDEF_FLAGS_B_BLINKING`; they do not recolor its sprite. |
| `common/scripting/entity/entityscriptengine_1.asm:VInt_UpdateSprites` | When the blink bit is set and the frame-counter/threshold condition holds, rendering takes `@MoveSpriteOffScreen`. This establishes intermittent sprite suppression, not a gold tint; no modern duration is inferred from it. |
| `gameflow/battle/battlefunctions/battlefunctions_0.asm:ControlCursorEntity_ChooseTarget`, `UpdateCursorSprites`, `sprite_Cursor` | Target browsing updates a separate cursor and mini status, accepts A/C or cancels B, then hides the cursor. The sprite table has radius 0/1/2 variants and explicit palette selectors. Its artwork/color appearance is not established by these selectors alone. |
| Same file: `CreatePulsatingBlocksForGrid`, `DeletePulsatingBlocks` | The renderer consumes the grid at `FF4D00_LOADING_SPACE`, changes map tile palette/priority bits and installs `FADING_SETTING=PULSATING_1`; deletion clears those bits/fade state. This is grid feedback, not a traversal of `BATTLE_ENTITY_MOVE_STRING` to draw path underlines. |

Reproduce in the registered read-only pinned checkout:

```powershell
git rev-parse HEAD
git status --short
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlefunctions/battlefunctions_0.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/scripting/entity/entityscriptengine_1.asm
```

**Inferred:** preserving sprite colors and separating acting-unit, legal-stop and target cues better
reflects these distinct responsibilities than a gold sprite plus a gold breadcrumb trail.
**Unknown:** natural caller/frame timing, original blink duty cycle in a reached turn, cursor pixels,
range palette appearance and a whole-game absence of other path effects. This inspection supports a
small modern rendering choice; it does not require original runtime acquisition or admit new gameplay.

## Battlefield movement and walking audio

Application owns a finite route and one completion token per adjacent segment. Player steps change
the provisional destination; committed cells and resources remain unchanged until the action.
Friendly cells may be traversed but cannot be selected as a final stop. Cancel walks the source
decreasing-cost return route to the committed origin, rather than reversing the input history.
AI retains its chosen route, thinking stream and memory from one decision. A pure, discarded action
preflight preserves atomic Unsupported admission, including random-dependent reward capability;
actual action construction publishes main RNG only after arrival. Damage, rewards and turn advance
retain their existing action continuation. No future battle snapshot is retained alongside live state.

Godot interpolates only the current legal segment, selects direction and both halves of the admitted
map-sprite sheet, and delivers that segment's token once. Display durations are modern 0.20 seconds,
or 0.10 with reduced animation; delta, redraw, audio completion and arrival add no gameplay RNG.
Busy gameplay input cannot acknowledge movement. Each new segment requests79 once. Blocked input,
origin Cancel, origin Stay and repeated projection have no walking cue. The private consumer reuses
the selected world's finite `SFX_WALKING_TIMER_BD`, command79/timer189, 44,100 Hz, 30,006 frames,
SHA256 `181C8EDDCD877D9AB031366B88F926A44F97684CDAB44697307FEFAAE8014F9B`.
No new extraction, resource copy or promotion is needed. Whole-PCM replacement remains the accepted
audio policy; modern segment duration need not equal PCM duration.

**Confirmed static source boundary:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, beneath `disasm/`:

- `code/gameflow/battle/battlefunctions/battlefunctions_0.asm` supplies
  `MoveBattleEntityByMoveString`, whose sound-command call site `0x23078` starts each segment's sound
  before travel, along with destination waiting and
  control/temporary land-effect coordinate updates; `battlefield/buildmovestringfunctions.asm`
  supplies `BuildCancelMoveString`, and `battlefunctions/executeindividualturn.asm` selects the
  control boundary. `battlefunctions/setmovesfx.asm:SetMoveSfx` (`0x25790..0x257C0`) selects79 in battle, except an
  equipped Chirrup Sandals ring (`0x7E`) selects99. Existing private ATT-only weapon admission
  excludes rings; arbitrary loadouts and99 are outside this consumer.
- `code/gameflow/battle/battleloop_1.asm` clears `PLAYER_TYPE`; `SetControlledEntityActScript` in
  `code/common/scripting/entity/entityfunctions_2.asm` therefore selects the ordinary controlled
  character script. `data/scripting/entity/eas_main.asm` runs `ac_checkMapBlockCopy`,
  `esc02_controlCharacter`, destination wait and branch. Neither a vehicle nor random walk is
  selected. The AI movement-string path also waits on ordinary entity displacement.
- `CreatePulsatingBlocksForGrid` in `battlefunctions_0.asm` sets nonzero
  `FADING_SETTING=PULSATING_1` before movement. In
  `code/common/scripting/entity/entityscriptengine_2.asm`, `esc40_checkMapBlockCopy` checks that
  setting first and skips copying while the battle grid pulsates. This contextual guard matters:
  roof copying can mutate layout in other contexts. Map57's roof/step tables contain only `endWord`.
- `esc02` vehicle branches and `OpenDoor` have battle guards. Warp/zone branches may still write
  `MAP_EVENT_TYPE/PARAM` scratch; battle control consumes button/destination state and `BattleLoop`
  has no map-event dispatcher. Victory writes its return parameters from combatants. This is not
  a claim that every interrupt has no side effects.
- `LoadBattle` restores `SetBaseVIntFunctions`: map planes, entity scripts, view, scroll, sprites,
  windows and map animations. The admitted movement update and controlled/standing/idle scripts
  do not draw RNG. Battle01's other combatants and neutral entities use those stationary scripts;
  close-up reaction/fairy and random entity scripts are not active in this boundary.

Reproduce the audit with `git -C $pinnedUpstream show '<commit>:disasm/<path>'` for the named
symbols and `rg -n 'Random|RANDOM|SEED'` over the reached bodies, checking each caller's guards.
Absence of a random call alone does not prove natural reach or interrupt parity.
**Unknown:** exact VInt opportunities, sprite-load queue stalls, acceleration, held-input cadence,
broader scratch effects and hardware audio. Normal/reduced native equality establishes only the
bounded modern consumer. Option A, the retained HEAL CP2059–2091 recovery mismatch and 8D/H4 remain
open. No logical padding, reseeding or fitting to retained79 receipt counts is part of movement.

## Selective hit/coin message delivery (#1650)

**Implemented, bounded:** Application `BattleSceneState.CanSuppressHitCoinMessage` marks only
`GoldMessage` and noncritical `ResultMessage` with Damage from physical-first/second/counter optional.
Every other phase defaults required: attack announcement, critical/miss/no-effect, spell/item damage
and recovery, death, EXP, growth/learned spells, dropped/received items, all status/after-turn notices
and battle story. Future phases remain required without an explicit policy change.

Godot consumes the separate default-false preference at the [safe Speed caller](presentation.md#battle-speed-and-selective-messages-1650).
An optional hidden message projects no text and returns one ordinary `Acknowledge` for the current
token on its actual processing opportunity. It does not fabricate presentation completion, remove
logical scene phases or bypass action construction, damage, MP, reward, RNG, death or turn suffixes.
The existing session/revision/token guard refuses stale or duplicate delivery. Focus/host pause blocks
consumption and resumes once without catch-up. Required messages retain reveal/acknowledgement rules.
HEAL's logical speed2/no-messages0 work remains unchanged and is never suppressed by this policy.
The accepted [fast-text audio policy](audio.md#accepted-fast-text-speech-policy) remains: omitted
characters add no speech; existing tails and legitimate confirmation/replacement keep their owners.
No speech backfill, whole-message skip, Shift acceleration or original timing parity is introduced.

**Confirmed, bounded remake:** six engine cases cover retained critical/miss messages and real optional-token refusal/acknowledgement
through damage, death, EXP, gold and next turn. Actual native default/suppressed attacks expose the
same phase sequence and final gameplay/RNG. EXP and death still display while ordinary damage/coin
text disappears and completes without input or deadlock. Existing public engine behavior checks pass.
See the [reproduction and limits](../development-and-verification.md#battle-speed-and-message-observation).
Pinned original S8/bsc10/bsc11 and the manual retain original authority in the
[battle source trace](battle-interaction-baseline.md); this conservative mapping is not a claim that
every original bsc10 message is hidden. Natural original reach/timing and human visual approval remain Unknown.
