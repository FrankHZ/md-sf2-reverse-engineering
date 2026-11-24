# Offensive consumables in battle

**Implemented for [#1944](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1944).**
Standard rigor applies because one action crosses battle selection, effect construction, custody,
presentation and Continue. The player selects and consumes a held Shining Ball or Blizzard to
damage an eligible enemy and returns to ordinary battle control. This design covers the connected
campaign through Battle26 and first Pacalon. The accepted ring movement design and the existing
item, damage and save contracts supply the shared boundaries.
[L2 item loop](../../../docs/design/synthesis/themes/tactical-battle-loop.md#items-custody-equipment-and-use-are-separate) ·
[Action construction](../../../docs/design/contracts/battle/battle-action-construction.md#item-use-and-break-routing) ·
[Ring movement decision](ring-equipment-design.md#remake-loadout-actions-and-stats)

## Original rules

**Confirmed (static).** The rows and callers below are from the USA retail baseline and
`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`. To inspect the
same operands, use `git -C <pinned-checkout> show <commit>:disasm/<path>` on the named files.
The Battle19 and Battle24 evidence owners supply the verified ROM identity, placement and drop
provenance. The original claims use static source and previously accepted evidence.
[Battle19 item and drop](../../../docs/research/chapters/southern-travel-creed/battle19-initialization-and-first-control.md#shining-ball-declaration-retained-bits-and-use) ·
[Battle24 item and drops](../../../docs/research/chapters/northern-travel-mitula/battle24-initialization-and-first-control.md#equipment-masked-items-and-independent-drops)

| Carried item | Use effect | Center range and radius | Element and base power | Definition animation |
| --- | --- | --- | --- | --- |
| Shining Ball 16 | FBALL 1, base ID 37 | 1–1, radius 0 | Fire, 20 | BLAZE variation 3 |
| Blizzard 17 | BREZAD 1, base ID 38 | 1–1, radius 0 | Ice, 33 | FREEZE variation 3 |

Both item rows are `RARE|CONSUMABLE`, have item range 0–1, class mask `NONE` and three empty
equip effects. Both linked effects are `TYPE_ATTACK` with cost 0 and no silence flag.
[Item rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L183-L202) ·
[Spell rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L650-L664) ·
[Element rows](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spellelements.asm#L45-L46)

- The player item menu records the carried word and slot. `IsItemUsableByCombatant` admits
  non-equipment without the weapon/ring class check. The use-spell grid supplies the opposing
  faction and the four orthogonally adjacent center cells from the actor's chosen stop.
  Zero HP, neutral activation bit and out-of-bounds coordinates exclude a combatant from the
  target array. Target confirmation writes `USE_ITEM`. No targets or Back returns to selection
  before effect construction. Radius zero gives the one selected enemy. Taros Sword goes through
  the equipment class check and its empty mask prevents ordinary player Use.
  [Player menu and slot](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L377-L430) ·
  [Class gate](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L911-L937) ·
  [Grid](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildactionrangegrids.asm#L68-L153) ·
  [Target eligibility](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildtargetsarray.asm#L66-L94) ·
  [Target construction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/populatetargetslist.asm#L11-L23) ·
  [Taros custody and player eligibility](../content/campaign-items.md#special-equipment-and-attack-items)
- `UseItem` unpacks the effect from the item definition and enters the spell effect dispatcher
  with the action still `USE_ITEM`. Learned rank is irrelevant. MP payment, the silence cast
  check and the 5/4 promoted damage multiplier are `CAST_SPELL` branches. FBALL and BREZAD
  bypass the four invocation-only power divisions. Ordinary turn eligibility and inaction
  still apply. In particular, the source checks stun before the action message and effect loop.
  [Use dispatch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/useitem.asm#L8-L20) ·
  [Cast gates](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/initbattlesceneproperties.asm#L104-L144) ·
  [Power adjustment](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/calculatespelldamage.asm#L89-L122)
- FBALL enters Blaze damage and BREZAD enters Freeze damage. Let `q = floor(power/4)`. The two
  bits of the matching target element (Fire at shift 6, Ice at shift 4) select `power`,
  `power - q`, `floor(power/2)` or `power + q` in order. Both then roll main RNG(32), adding
  `q` on zero. For an ordinary target, compute `range = floor(damage/8) + 1` once after that
  roll. Subtract two main RNG draws of that range and floor the result at one. HP bottoms out
  at zero. This route shares the existing target exceptions in `InflictDamage`.
  [Effect dispatch and critical callers](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm#L58-L65) ·
  [Critical constants](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2enums.asm#L1670-L1671) ·
  [Resistance and critical](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/calculatespelldamage.asm#L34-L70) ·
  [Target exceptions, variance and HP](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/inflictdamage.asm#L35-L95)
- Damage EXP is the common kill bracket times final rolled damage divided by target **max**
  HP, rounded down. The bracket comes from actor level plus 20 if promoted, minus target level.
  Differences below 3, then 3/4/5/6 and above 6 yield 50/40/30/20/10/0. A kill adds the full bracket after the damage
  contribution, with the cap of 49 applied after each addition. Ordinary opposing kills retain
  kill credit and gold. The award uses two main RNG(16) draws, plus one if the first is zero and
  minus one if the second is zero, minimum one. The existing battle-specific halving precedes
  those draws and does not apply in Battles19, 24 or 26. Promotion still changes the EXP bracket.
  [Damage and kill EXP](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/earnexp.asm#L50-L123) ·
  [Effective levels](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/earnexp.asm#L153-L185) ·
  [Award contract](../../../docs/design/contracts/battle/spell-resolution.md#5-accumulate-and-award-attack-spell-exp)
- After each target effect, the original evaluates the drop against the pack as it then stands.
  After the target loop `BreakUsedItem` removes the selected slot and refreshes stats. The
  following full words shift left and word 127 fills the tail. A full pack can therefore send a
  rare drop to Deals even though the action will free a slot. Non-equipment removal draws no
  break RNG. A failed selection or an early inaction/empty-target exit never reaches removal.
  [Construction order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm#L80-L131) ·
  [Use removal](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/breakuseditem.asm#L8-L53) ·
  [Compaction and refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L482-L527)
- Presentation writes an item-name message and chooses the item-use actor animation. For
  non-equipment, `GetSpellanimation` leaves the default `SPELLANIMATION_NONE`. The effect
  rows' variations therefore carry no playback requirement for these items.
  [Message selector](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlescenemessage.asm#L85-L92) ·
  [MP and actor animation](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlesceneanimation.asm#L36-L57) ·
  [Use pose](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlesceneanimation.asm#L127-L135) ·
  [Overlay default](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm#L36-L43) ·
  [Equipment-only Use overlay](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getspellanimation.asm#L134-L153)
- Field and Caravan both call `IsItemUsableOnField`. Neither ID is in its usable table, and the
  pinned item-event tables supply no field Use for either. Field tries a map event then reports
  no effect. Caravan takes its unusable branch before target selection and removal. The two
  items remain ordinary carried property in those contexts.
  [Field usability table](../../../docs/research/world/field-item-effects.md#confirmed-static-contract) ·
  [Field caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/main/mainactions.asm#L242-L271) ·
  [Caravan caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/caravan/caravanactions_1.asm#L723-L758)

**Confirmed (static).** Battle Suspend marks flag 88 and saves the current slot from
`COMBATANT_DATA`. The save layout covers pack words and the item-drop flag bank. The
complete original ordinary use, animation timing and suspension/cold-load round trip for these
two items remain **Unknown**.
[Suspend caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L1162-L1185) ·
[Save and load routines](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/sram/sramfunctions.asm#L96-L143) ·
[RAM layout](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2const.asm#L664-L687) ·
[Save layout](../../../docs/design/contracts/system/save-system.md#confirmed-static-contract) ·
[Battle resume boundary](../../../docs/design/contracts/battle/battle-control-lifecycle.md#resumed-battle)

<a id="proposed-remake-boundary"></a>

## Remake item action

Keep `SelectItem(slot)` and the Item action through selection and validation. Bind a typed
battle attack item to its item identity, display name and an existing `AttackSpellDefinition`
instance for the effect. Content supplies the two source bindings. Keep these definitions off
the learned spell list and out of existing enemy spell/item selection. Other equipment Use,
breakage, repair and AI consumable Use have separate scope.

Reclassifying the selected slot as a Magic action would reuse damage but also route the action
through learned spell validation, spell projection and the Taros-only slot exception. Copying
the damage loop into the Item rule would split responsibility for RNG, rewards and drops.
Keep Item as the originating command and let its attack branch call the factored
`Sf2AttackSpellAction` target and damage preparation with an explicit item origin. This keeps
origin-specific payment and presentation at the callers. The shared damage continuation
selects the existing attack-damage EXP path. Preserve the approved Taros enemy branch as held
equipment with no consumption.

- Item admission validates active player actor and live pack slot. Retain the session's
  revision, actor, slot and expected **full word** through target selection. At commit verify
  them again, together with the trusted definition, target eligibility and range. Reuse the
  confirmed movement stop from the ring agreement for projection and validation. Checking the
  route again under a changed MOV budget could reject an already reached stop. Back to Movement
  recalculates reach and keeps any committed gear change.
- Before confirmation target and range previews leave pack, actor position, HP, rewards and both
  random streams untouched. No legal target, a wrong faction, dead/unplaced/neutral target,
  out-of-range center or stale slot rejects through the existing action boundary. The holder
  can have zero MP, no book or applicable Silence and still use the item where ordinary
  player control is admitted.
- Resolve target damage and drop capacity while the consumed word is still in the pack. Apply
  one compact removal to the resulting pack, then continue through the shared damage award and
  turn pipeline. Keep the drop receipt and once bit even if compaction moves the received item.
  Carry the validated item identity and slot into the existing scene handoff so the label does
  not read the newly shifted slot. Publish the prepared inventory and its `item-consumed`
  observation once on accepted construction, after drop construction and before scene playback.
  Preserve the existing ordered HP/kill, reward, growth and next-control semantics.
- Reuse the current scene waits and item gesture with an item-name message and enemy damage
  reactions. Use no spell-cost phase and no elemental overlay for these consumables. Keep the
  current Taros enemy presentation and the ordinary Magic presentation. Use existing English
  names and the `name.item` entries for 16 and 17 in the accepted Chinese catalog.
- Keep the existing four-slot and battle Continue formats. Save at settled player Movement
  after scene and turn completion. Cold Continue restores committed packs, RNG, HP, EXP, rewards,
  drop custody and current actor. It does not resume provisional commands or an in-flight
  attack. Field and Caravan retain existing denied-Use and custody behavior.
  [Common command and transaction boundary](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#provisional-selection-atomicity-and-stale-input) ·
  [Settled Save](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs) ·
  [Battle save wire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs)

### Implementation owners

Item targeting and mutation use the accepted ring landing path. The existing movement
confirmation and save formats carry the resulting state.

| Responsibility | Implementation surfaces |
| --- | --- |
| Definition and admission | [EngineBattleState.cs](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) for the item definition and registry, including definition copies. [IBattleActionRule.cs](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs), [Sf2ItemAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) and [BattleChoices.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) for the bounded Item admission, enemy choices and range. [BattleCommandDispatcher.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs) supplies the confirmed-stop and expected-word path. |
| Damage and inventory | [Sf2AttackSpellAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) and [AttackSpellDefinition.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/AttackSpellDefinition.cs) for shared attack resolution and the item-only supported effect shape. [BattleActionResolution.cs](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) for validated origin, drop then removal, and the scene handoff. Pass the existing attack-damage reward kind to [Sf2BattleProgressionRule.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs). |
| Source binding | [PrivateBattleDefinitionReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) selects the two usable rows and their spell rows before building per-encounter definitions, including where the item is only carried in a predecessor or old save. [PrivateBattleActionBindings.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) validates the two item/effect joins and emits typed definitions. [PrivateBattleScenarioReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) attaches them. The existing static-data input contains both rows and linked spell definitions. |
| Scene and player projection | [BattleSceneContinuation.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) projects the resolved item and skips Magic cost. [BattleSceneView.cs](../../game/src/Battles/BattleSceneView.cs) uses the item gesture and localized identity and routes damage reactions. [BattleCommandMenu.cs](../../game/src/Battles/BattleCommandMenu.cs) and [BattleActionRangeView.cs](../../game/src/Battles/BattleActionRangeView.cs) are the existing input/range consumers. |
| Engine behavior | [AttackItemSessionTests.cs](../../tests/Sf2.Remake.Engine.Tests/AttackItemSessionTests.cs) covers Item-origin damage and EXP, refresh, the exact slot and ordered custody through actual session commands. |

## Retained acquisition and ordinary acceptance

**Confirmed (remake save inspection).** The existing ordinary Battle19 victory saves
`handoffs/issue1834/slots/after-battle19-{en,zh-CN}.json` have Peter (`ally-7`) carrying the Ball
in slot 0 and drop once bit 6 set. Both continuations are already recorded.
[Battle19 victory and retained saves](../application/battle19-village.md#victory-and-the-next-slot)

**Confirmed (remake save inspection).** The accepted Battle24 departure saves have no Blizzard
in allied packs and bit 9 is clear. At `handoffs/issue1904/slots/north-cliff-en.json` on
Map72 (27,38), Peter holds Blizzard in slot 0. The corresponding `north-cliff-zh-CN.json`
holds Blizzard in Kazin's slot 2 and Ball in Peter's slot 0. Both have bit 9 set. The retained
English `review-en-continue02` and Chinese `review-zh-CN-continue01` re-saves under that
handoff's `player-saves` have the same decoded gameplay data as their corresponding cliff saves.
The same handoff's `receipts/en-checkpoint02/actions.jsonl` records Peter dealing 32 damage
to `enemy-7` (source slot 135) in round 5, followed by death, Blizzard drop and receipt.
`receipts/zh-CN-checkpoint02/actions.jsonl` records Kazin's 1-damage hit on that slot in
round 8 with the same receipt sequence. The Chinese chain later lost and retried with the
once bit and eventual cliff custody intact. These action records and endpoint saves establish
ordinary remake acquisition and cold custody for both languages. The immediate post-kill
battle save remains **Unknown** in the inspected inputs. Verify the combat rows by target,
phase and item identity. Reproduce the cold comparison by base64-decoding each named slot's
`payload`, verifying its existing envelope `sha256`, comparing `data.allies[].items` and
`data.onceFlags` against the departure, then comparing the entire cold `data` with the endpoint.
[Battle24 route and cold handoff](../application/battle24-north-parmecia.md#observed-remake-play)

The shortest acquisition path for acceptance is to reuse those already earned copies. If a
fresh receipt is needed, start before Battle24's first completion with bit 9 clear. Confirm
the Bishop in source slot 135 still holds Blizzard. Defeat it with a living ally who has an
open pack slot, then save at the first ensuing settled Movement. This is the earliest source-backed
candidate in scope and does not require victory. A full pack routes the rare item to Deals.
Map38's later chest is outside the current campaign. The ordinary original acquisition and
action remain **Unknown**.
[Selected drop predicates](../../../docs/research/chapters/northern-travel-mitula/battle24-initialization-and-first-control.md#equipment-masked-items-and-independent-drops)

## Ordinary remake observations

**Confirmed.** The Chinese journey started at Battle26 Movement from the retained `zh-CN-road01`
slot. Peter canceled Shining Ball's target selection, then reselected the same slot through the
Item menu. Back preserved the holder, live board, pack and both RNG streams. Each successful
action targeted an adjacent enemy and published one consumption receipt. The Chinese journey
used both items in one battle. The English seed supplied a separate Blizzard item presentation.

| Language and holder | Item and zero-based slot | Target HP | Holder MP and EXP | Pack after use | Next player |
| --- | --- | --- | --- | --- | --- |
| Chinese Peter | Shining Ball, slot 0 | enemy-14, 38 → 20 | MP 0 → 0, EXP 3 → 4 | `[0,0,127,127]` | ally-18, round 1 |
| Chinese Kazin | Blizzard, slot 2 | enemy-12, 40 → 12 | MP 55 → 55, EXP 5 → 6 | `[213,0,0,127]` | ally-9, round 2 |
| English Peter | Blizzard, slot 0 | enemy-12, 40 → 10 | MP 0 → 0, EXP 29 → 30 | `[0,0,0,127]` | Bowie, round 1 |

The projected action and damage messages used each language's item name. The actor played its
item gesture, with item animation indexes 7 for Peter and 4 for Kazin in these scenes.
The spell overlay selector was 0 and the damage scene had no MP payment phase.

Save at the next battle Movement, then use Bowie's paid EGRESS from that journey to return to
Map36 and save again. With the entire producing `local/` moved aside, fresh title Continue
and immediate Save produced equal complete decoded `data` in battle (sequence 7 → 8) and
field (8 → 9). The next input opened ally-9's Action Choice and moved the field party from
(40,5) to (39,5). Both cold processes and the Chinese producer exited 0. The English producer
completed Use, next control and Save before finalization exited with `0xC0000005`.
The Chinese producer and cold battle emitted ObjectDB shutdown warnings.
[Paused shutdown follow-up #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

### Reproduce with the retained inputs

Restore the registered configuration into the owning checkout and use its existing Debug
adapter. The local handoff scripts are under `LocalApplicationData/Sf2OffensiveReview/issue1944/driver`.
Copy those scripts into the ignored `local/issue1944` directory. They reuse the registered
Pacalon title, movement and Save driver, the unchanged `issue1966/content/package` and the
`issue1941/content/catalog-final` display names. Each sample writes a new disposable player
namespace and records live input, state, projection and errors.
[Fresh worktree configuration](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup) ·
[Registered Pacalon inputs](../application/pacalon-first-visit.md#continue-after-worktree-cleanup)

```powershell
$operator = [Environment]::GetFolderPath('LocalApplicationData')
$review = Join-Path $operator 'Sf2OffensiveReview/issue1944'
$sample = 'offensive-' + [guid]::NewGuid().ToString('N').Substring(0,8)
pwsh -NoProfile -File ./local/issue1944/Play.ps1 -Mode attack-items -Language zh-CN -Sample $sample
$journey = Join-Path $review $sample
$battle = Join-Path $journey 'battle-after-use.json'
$field = Join-Path $journey 'offensive-review/sf2-opening/slot1.json'
pwsh -NoProfile -File ./local/issue1944/Play.ps1 -Mode battle-continue -Language zh-CN -Sample "$sample-battle" -Seed $battle
pwsh -NoProfile -File ./local/issue1944/Play.ps1 -Mode continue -Language zh-CN -Sample "$sample-field" -Seed $field
```

For the cleanup check move the entire producing `local/` aside before the last two commands.
Restore the ignored configuration from its registered template and the two driver files from
the handoff into a fresh `local/`. Use the same built adapter. Compare each seed and
`immediate-resave-slot.json` as complete decoded `data` and inspect `after-continue.json`.
For the English seam use `-Language en` on a separate `attack-items` sample. The world and
media were reused. The four successful functional samples wrote about 0.94 MiB of top-level
receipts, scripts and save copies.

### Engine behavior checks

The focused cases exercise both elements with seeded damage and EXP, silenced zero-MP use by
a promoted holder, real stat refresh on consumption, the duplicate/changed slot, ineligible
targets, drop capacity before consumption and Running Ring removal after a confirmed stop.
The selected slot is offered at the provisional stop. Query and target choice keep the live pack
and both RNG streams intact. Confirm consumes slot 2 while the identical item in slot 0 remains.
Scene completion applies damage once with the compacted pack and unchanged MP.
[Action and payment review](gameplay-rules.md#action-admission-construction-and-payment-review)
The maintained engine check passed 2,936 cases with 49 configured private-input skips.
The Release adapter and the incremental Debug build passed. Early focused runs found invalid
controlled test inputs and a pre-movement identity assertion. The native rehearsals exposed
an input shortcut assumption and a wrong command selection before the observer verified the
visible Item menu and the actual slot. Their results remain in the ignored handoff.
[Offensive item engine cases](../../tests/Sf2.Remake.Engine.Tests/AttackItemSessionTests.cs)

The original ordinary action, animation timing and original Suspend/Continue remain **Unknown**.
The connected remake witness reaches the supported items and settled continuation through
Battle26. The original field event and acquisition boundaries remain documented above.
