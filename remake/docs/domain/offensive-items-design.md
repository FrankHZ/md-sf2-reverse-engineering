# Offensive consumables in battle

**Proposed for [#1944](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1944).**
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

## Proposed remake boundary

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

### Implementation handoff

These are proposed paths for the implementation slice after #1946 hands back the shared
battle surfaces. The implementation owner should recheck the accepted ring landing path and
declare ownership on that baseline.

| Responsibility | Exact candidate files |
| --- | --- |
| Definition and admission | [EngineBattleState.cs](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) for the item definition and registry, including definition copies. [IBattleActionRule.cs](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs), [Sf2ItemAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) and [BattleChoices.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) for the bounded Item admission, enemy choices and range. [BattleCommandDispatcher.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs) supplies the confirmed-stop and expected-word path. Change it only if the accepted path fails to cover Item. |
| Damage and inventory | [Sf2AttackSpellAction.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) and [AttackSpellDefinition.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/AttackSpellDefinition.cs) for shared attack resolution and the item-only supported effect shape. [BattleActionResolution.cs](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) for validated origin, drop then removal, and the scene handoff. Pass the existing attack-damage reward kind to [Sf2BattleProgressionRule.cs](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs). |
| Source binding | [PrivateBattleDefinitionReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) selects the two usable rows and their spell rows before building per-encounter definitions, including where the item is only carried in a predecessor or old save. [PrivateBattleActionBindings.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) validates the two item/effect joins and emits typed definitions. [PrivateBattleScenarioReader.cs](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) attaches them. The existing static-data input contains both rows and linked spell definitions. |
| Scene and player projection | [BattleSceneContinuation.cs](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) projects the resolved item and skips Magic cost. [BattleSceneView.cs](../../game/src/Battles/BattleSceneView.cs) uses the item gesture and localized identity and routes damage reactions. [BattleCommandMenu.cs](../../game/src/Battles/BattleCommandMenu.cs) and [BattleActionRangeView.cs](../../game/src/Battles/BattleActionRangeView.cs) are the existing input/range consumers. Change them only if the ordinary Item projection needs it. |
| Behavior and documentation | Extend [AttackSpellSessionTests.cs](../../tests/Sf2.Remake.Engine.Tests/AttackSpellSessionTests.cs) where its setup fits, or use a focused `AttackItemSessionTests.cs` in the same project. Update this design, [gameplay rules](gameplay-rules.md), the affected [item rows](../content/campaign-items.md#special-equipment-and-attack-items), [Battle24 owner](../application/battle24-north-parmecia.md#observed-remake-play) and [campaign feature trace](../capability-status.md#campaign-feature-trace) with accepted results. |

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

1. Prefer the existing `handoffs/issue1955/receipts/zh-CN-road01/battle-ready-slot.json`.
   This ordinary Battle26 Movement save has both holders alive and deployed. Peter has Ball in
   slot 0 and 0 MP. Kazin has Blizzard in slot 2. It retains once bits 6 and 9. The
   corresponding `en-road01/battle-ready-slot.json` has a living Peter with Blizzard in slot 0
   and 0 MP. Use separate disposable copies from the shared root and the accepted
   `issue1966/content/package` and `issue1941/content/catalog-final`. The on-disk inputs establish
   custody and eligibility. The first legal approach on each actor's turn still needs observation.
   [Registered setup](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup) ·
   [Battle26 seed producer](support-spell-status-design.md#battle26-ordinary-input-and-reproduction)
2. In one short Chinese journey, drive ordinary movement and Item on the two holders as they
   reach a legal neighboring enemy. On Ball, Back from target choice, then reselect the same
   slot and confirm. Capture cancel and commit results, localized item identity, cardinal range,
   actor gesture, target damage, MP, EXP and exact pack change. Check ordinary next control
   and Save/cold Continue after use. In English, use the retained Peter Blizzard seed for the
   same item-name and control seam if needed to cover both catalogs through ordinary input.
   Read live state, input, projection and errors through the existing observer with no screenshots.
   Compare a settled re-save's gameplay data and accept the next normal command. Record any
   completed exit failure with its process result and the independently observed save outcome.
3. Use small engine cases for the expensive counterexamples. Check both item/element bindings
   with a seeded critical/variance result, promotion without power scaling and damage EXP with
   the effective level, and an active silenced zero-MP holder without a book. Check wrong
   faction and diagonal/range/dead/neutral targets or a stale expected word for no mutation.
   A duplicate in a non-selected slot and a qualifying kill with a full pack test selection and
   drop-before-removal. For example, a Ball hit on a Fire-weak target uses power 20,
   rises to 30 on a critical and deals 26 with variance draws 1 and 3 from range 4.
   Against max HP 50 and a kill bracket of 40, that contributes 20 damage EXP.
   Exercise the confirmed-stop contract with reduced MOV after equipment
   change using the accepted ring owner. A lethal and nonlethal hit can share these cases.
   Retain existing Magic, Taros, retreat and healing admission. Run the scoped engine and
   adapter checks, the clean-head plan and actual applicable CI before implementation handoff.

Reuse the measured 101,835,469-byte world and its existing media, name catalog and pinned static
input. Expected generated world/media delta is zero bytes. The named field inputs are about
6–9 KB each and battle starts about 42 KB each. Budget a few disposable input/output saves and
a bounded handful of state/input/presentation receipts, about 1 MiB total apart from the normal
incremental adapter build/import. Reassess if the shared scene cannot supply the existing item
gesture or if the bounded ordinary turns fail to reach a useful target. A full chapter replay
or a new extraction platform is outside this design.
