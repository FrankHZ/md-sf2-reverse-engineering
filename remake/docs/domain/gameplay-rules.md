# Gameplay Rules and Composition

## Finite purchased supplies and DETOX1

**Standard finite pre-voyage agreement; existing payment/custody High retained.** Source is pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, consuming the accepted
[service contract](../../../docs/design/contracts/party/service-interactions.md#shop-source-boundary),
[item data](../../../docs/design/contracts/party/item-definition-data.md) and
[DETOX contract](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-detox-status-clearing-subset).
`shopactions.asm` Buy calls DecreaseGold then AddItem; Sell calls IncreaseGold then DropItemBySlot,
using `ITEMSELLPRICE_MULTIPLIER=3` and shift2 (`sf2enums.asm`). The runtime publishes each pair atomically,
using live member and slot, preserving the existing gold cap, packed custody and equipment accessors.
Selection/decline/cancel and failed preparation never spend, grant, remove or draw RNG.

`HealingItemDefinition.CuresPoison` / `HealingSpellDefinition.CuresPoison` explicitly distinguish the
finite poison-removal algorithm from HP recovery within the existing friendly action family. They
survive immutable definition copies. Healing Seed1 uses HEALIN2 power20, no variance or MP cost, and
`min(20, missingHP)`; Herb0 and Wing4 keep their existing effects. Antidote3 delegates to DETOX1 without
MP cost and consumes its selected compacting slot. DETOX1 uses learned-spell legality, MP3/range0–1,
single living same-faction target; marked-spell silence still pays before its no-effect result.
`castspell.asm:spellEffect_Detox` clears poison bit2 and preserves every unrelated bit. An effective
target contributes5 status EXP once; a clean target consumes the source threshold8 range8 ineffective
draw and contributes0 before the common randomized/minimum EXP award. Status EXP respects the existing
halved-experience battle flag. Neither case restores HP or clears stun/curse/sleep/silence. Payment,
status reaction, notice and subsequent turn remain staged; DETOX uses no HEAL fairy. Antidote does not
inherit the spell-only silence gate. Query/cancel, bad range/target, missing MP/accounting and stale
input preserve the complete prior state. Higher DETOX levels and other cure/status families remain
Unsupported. Enemy cure effects (including a `heal/1` authored alias) reject at both finite
HEAL AI decision and spell admission before any automatic movement, payment, effect or RNG publication;
this does not admit enemy DETOX. Field policies remain separate and unchanged.

**Confirmed static operands:** `data/stats/items/shopinventories.asm` stock1 has Short Sword, Short
Spear, Bronze Lance, Short Axe, Wooden Rod, Short Rod and Short Knife; stock16 has Herb, Seed, Antidote,
Wing. `itemdefs.asm` and `spelldefs.asm` own price, type, use-spell, power/range/properties; source
`allystats01.asm` PRST learns DETOX1 at level4. Legal levels are not capped by an expected walkthrough.
The [private admission](../content/profiles-and-trust.md#finite-galam-shop-admission) preserves the
important default/F663 distinction. `PoisonCureTests`, `ShopServiceTests`, `GalamShopContentTests`
and the actual native [recipe](../development-and-verification.md#galam-shop-and-supply-observation)
reproduce these bounded consumers. **Unknown:** natural original first shop/use reach, original
caller/window/audio timing and full later story. No new original observation closes those Unknowns.

## Finite DISPEL, SUPPORT and silence

**Implemented bounded capability, Standard rigor:** `SupportSpellDefinition` and
`Sf2SupportSpellAction` add DISPEL1 (MP5, range1–2, radius1) at the existing action seam.
One packed `BattleActorState.Status` remains authoritative. SILENCE is `0x0300`, counter unit
`0x0100`; it changes no numerical ATT/DEF/AGI. Immutable outcome/deployment copies retain support
definitions alongside the existing capabilities. No buff platform, secondary status map or scene-ID
legality is introduced. Existing transaction and later-failure prefix boundaries remain binding.

The living, placed, initialized, non-neutral opposing center must be in caster range. Actual
Manhattan area recipients use deployment/source processing order. Each recipient consumes one main
range8 draw. Without a known spell, threshold8 makes the target immune; otherwise the threshold is
`5 + ((Resistance >> 14) & 3)`. A roll at least that threshold succeeds. Fresh and already-silenced
targets use the same roll; success ORs `0x0300` with their existing status, refreshing all three
counters, and contributes5 EXP (per-action cap49, existing battle halving and two-draw award).
The source minimum award can still grant1 EXP for an unsuppressed wholly ineffective player cast.
Enemy casts do not award EXP. Preparation publishes neither status nor MP; one cost reaction precedes
ordered per-target status/no-effect results, reward and after-turn. Invalid/unsupported preparation,
queries and cancellation publish no speculative movement, RNG, cost or effects.

Ordinary learned HEAL/AURA, BLAZE/BLAST and DISPEL carry `AFFECTEDBYSILENCE`. They remain selectable
and confirmable under silence, with their usual learning/MP/target admission. The common action
boundary prepares a suppressed cast without target-effect or award draws. Its scene publishes MP
once, then the silenced/no-effect message and action end; HP/status/EXP target commands are absent.
This is a paid legal action, not an illegal-command refusal. Physical attacks, Stay, free Equip,
Give/Drop and items remain unaffected; Herb's HEALIN is unmarked. EGRESS is a source exception:
`ExecuteIndividualTurn @CheckBattleaction_CastEgress` branches to its special return before the
ordinary spell scene even though its table has the silence property. It retains its own8MP payment
and retreat continuation. Unmarked SPOIT is not implemented by this slice.

`EnemySupportDecision` participates at the existing SUPPORT command positions in sets2/6/7/8/9/14,
after standby/earlier successful commands. It requires known DISPEL1, sufficient MP and a reachable
legal station. Score is the number of actual area recipients whose first eligible attack-spell
family has an affordable rank, or who know a healing spell. Healing's getter ignores MP; silence
and existing status/resistance do not remove contributors. Each actor contributes at most1. Areas
below2 are filtered; the last highest candidate in source processing order wins, with existing
station/path construction. SUPPORT consumes no scoring/category RNG and does not update LastTarget.
It never substitutes center damage, a Mage attack tie table or MP alone for this score. A silenced
enemy can select the same action and reaches the same paid suppression as the player.

The admitted source silence prowess has critical1/4, no bonus damage, double1/16 and counter1/16.
A successful physical critical ORs silence only if the surviving target knows a spell; there is no
DISPEL resistance draw. Damage and the later status-message/reaction remain separate publications.
Miss/no-spell/dead cases do not fabricate a silence write. Existing sleep, double/counter and
`ExtraRoundAction` mechanisms remain in use; numerical AGI13 plus the extra entry is not AGI141.

`SilenceRules` and the existing `BattleAfterTurnContinuation` process each living, nonterminal action
entry once: sleep first, then silence, then opted-in poison. Silence draws main range equal to the
current masked field. A masked-zero result expires it with message351; otherwise it subtracts one
unit silently. Expiry status publishes after matching notice delivery. One counter always expires;
two/three can expire early. Combined sleep/silence/poison retain unrelated bits, including lethal
poison's existing death/accounting/queue path. Extra round entries each take their own pass; death
or a resolved action outcome bypasses it. Other status bits and Prism's status-free profile remain
explicitly Unsupported. A repeated engine lifetime is not new original-runtime evidence.

**Confirmed provenance:** the accepted [spell contract](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-dispel-1-spell-gate-and-recast)
and [Battle07 static account](../../../docs/research/chapters/grans-opening/battle07-opening-and-first-control.md)
own original evidence. Additional exact source reads use pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to `disasm/`:
`sf2enums.asm` (DISPEL6, status masks), `data/stats/spells/spelldefs.asm`,
`data/stats/allies/classes/criticalhitdefs.asm`, `data/stats/enemies/enemydefs.asm` (Dark Smoke86),
and these paths under `code/gameflow/battle/`: `ai/command/support.asm`,
`ai/command/support/prioritizetargetsforsupportspells.asm`, `ai/getnextuseableaiaction.asm`,
`ai/gethighestusablespelllevel.asm`, `battleactions/{castspell,inflictailment,determinecriticalhit,earnexp,giveexpandgold}.asm`,
`battlefunctions/executeindividualturn.asm`, and `battleloop/processafterturneffects.asm`.
Under each full `code/gameflow/battle/` path, reproduce an exact object with
`git -C $pinnedSf2Disasm show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`.
SUPPORT's occupancy branch follows the actual CCR-overwriting MOVE/CLR instructions, not just the
table targeting property. This is static support, not proof of a natural first cast or critical.

[Behavior tests](../../tests/Sf2.Remake.Engine.Tests/SilenceSpellTests.cs),
[session tests](../../tests/Sf2.Remake.Engine.Tests/SilenceSessionTests.cs) and
[decision tests](../../tests/Sf2.Remake.Engine.Tests/SupportDecisionTests.cs) cover changed states,
source/private admission, staged writes/cost, mixed lifecycle and queue boundaries. The
[native recipe](../development-and-verification.md#silence-observation) uses an authored small battle
through ordinary inputs in English/Chinese. Private binding, original natural timing/reach and
full Battle07 story connection remain separate; MUDDLE/BOOST/SLOW/full buff behavior is deferred.

## Finite early enemy spell decisions

**Implemented bounded capability, Standard rigor:** activated source enemies can choose BLAZE1 and
HEAL1 through the same selected action rules, movement continuation, MP publication, effects,
death cleanup and next-turn flow used by players. Decisions do not publish damage, recovery or MP.
`IBattleDecisionRule` receives the session's existing action bindings; the common decision boundary
prepares and discards the chosen action before publishing any AI movement/RNG. Missing reached
accounting, unsupported payloads and unbound terminal outcomes reject atomically. Physical and
damage spells share terminal admission; healing uses actual faction, not a priest/ally assumption.
Only allies earn EXP/gold/kills; an enemy damage spell can increment an ally's defeats through the
existing death batch. Enemy healing has no award and retains both RNG streams during construction.
Enemy BLAZE1 requires radius0: a nonzero authored radius rejects as `enemy-spell-radius` before
automatic movement, scoring RNG, MP or effects publish. Player radius variants remain supported;
this finite enemy decision does not implement the source's multi-recipient score/RNG aggregation.

The dedicated commandset byte controls ordering: SENTRY8 attempts ATTACK1, HEAL1, SUPPORT, STAY
and never pursues on its final fallback. HEALER3 is commandset2: HEAL2, MOVE_ORDER4, SUPPORT,
ATTACK3, MOVE2, STAY, with the existing block-non-movable preparation and live follow/dead-follow
promotion. HEAL2 here is an AI command whose parameter is unused; the early cleric still casts
HEAL1 (MP3, range0-1, power15). Existing6/7/9/14 movement/order paths remain in place. Ordinary
inactive standby still runs before these commands. No actor name, battle ID or reference round
selects an algorithm.

HEAL1 scans living, placed teammates with `3*HP <= 2*maxHP` and more than2 missing HP, then tries
stable descending source mover/critical-commandset priority and legal unoccupied casting stations.
The finite priority order includes healer12, mage11, archer8, hovering7, flying6, stealth4,
centaur3, regular2; critical/leader commandsets13/14 receive13. No thinking/main draw is needed.
An unreachable higher-priority target is skipped. Self-healing uses the caster's current station.
No need, insufficient MP or no legal station falls through to the remaining commands.

ATTACK scores reachable spell targets before physical targets, in reverse source processing order.
Normal PRIORITYMOD_2 uses script3's separate thinking RNG; the cleric's PRIORITYMOD_3 selects
script4's deterministic lethality/inclusive one-fifth-HP score. Stable source tie handling reuses
the physical owner: physical class preference follows the attacker's mover, while CAST_SPELL
forces the Mage table regardless of mover (`determinebattleaction.asm` @loc_34/35).
With a reachable BLAZE1 candidate, category selection
always draws main range6:2/4 choose spell when physical is also viable; otherwise physical wins.
A spell-only candidate still consumes that category draw. Insufficient MP or no reachable spell
candidate uses physical without the category draw. Actual critical/variance draws occur only in
the shared prepared damage action. The supported cleric prowess is critical1/32 with half bonus,
double1/32 and defender counter1/16, independent of the attacker's prowess.

**Confirmed static provenance:** pinned `SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to `disasm/`:
`data/battles/global/aicommandsets.asm`, `aipriority.asm`, `data/stats/enemies/enemydefs.asm`
(GALAM_MAGE67/DARK_CLERIC76), `code/gameflow/battle/ai/startaicontrol.asm`,
`command/heal.asm`, its `heal/{calculatehealtargetpriority,determinehealingspelllevel,doescombatantrequirehealing}.asm`,
`command/attack/{prioritizetargets,determinebattleaction,targetprioritizationhelperfunctions}.asm`,
and `code/common/tech/randomnumbergenerator.asm`. Read these exact objects with
`git -C $pinnedSf2Disasm show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<relative-path>`.
The accepted [Battle05](../../../docs/research/chapters/grans-opening/battle05-initialization-and-first-control.md)
and [Battle06](../../../docs/research/chapters/grans-opening/battle06-opening-and-first-control.md)
owners distinguish static availability from natural first casts. Those natural routes/timing remain
**Unknown**. This engine capability does not import Battle05/06, implement higher-rank enemy spells,
item AI, all AI or non-Normal difficulty. DISPEL/SUPPORT and silence use the finite owner above.

[Enemy decision tests](../../tests/Sf2.Remake.Engine.Tests/EnemySpellDecisionTests.cs),
[session tests](../../tests/Sf2.Remake.Engine.Tests/EnemySpellSessionTests.cs) and
[private binding test](../../tests/Sf2.Remake.Engine.Tests/EnemySpellAdmissionTests.cs) own changed
positions/order/MP/need/blocked stations, priorities/RNG, shared effects, failure atomicity and
next control. The [native recipe](../development-and-verification.md#enemy-spell-observation)
uses a small [authored package](../../content/authored/enemy-spells.json). Recovery projection now
switches caster/recipient on their real faction side; supplied private visuals select their matching
ally/class or enemy/actor dictionary. Authored gestures and private resource admission remain separate.

## Finite player attack spells

**Implemented bounded capability, Standard rigor:** learned BLAZE1-2 and BLAST1 use the existing
`IBattleActionRule` query/admission/preparation seam. `HealingSpellDefinition` and `IHealingRule`
continue to own healing only. Separate immutable `AttackSpellDefinition` operands and
`Sf2AttackSpellAction` own damaging spell legality, target resolution and construction;
`SessionAction.AttackSpell` preserves that distinction through selection. No new scheduler,
dispatcher, effect language or mutable state authority is introduced. Existing transaction and
later-failure prefix rules remain in force.

BLAZE1 costs2, range1-2, radius0, power6; BLAZE2 costs6, range1-2, radius1, power10;
BLAST1 costs2, range1, radius0, power6. Power is an input to damage, not final damage.
A living player ally must know the selected rank and afford the cost. The living, placed,
initialized, non-neutral opponent center must satisfy the range from the provisional destination.
Radius uses Manhattan distance around that center, independently of caster range; recipients are
sorted by deployment processing order (the private reader maps original combatant slots to that
order). Dead, absent, allied and neutral occupants do not receive the attack. Unsupported caster
statuses/AI and reached unknown accounting reject explicitly. Queries consume neither RNG stream;
confirmation rechecks the same admission and constructs without publishing speculative changes.

The [spell-resolution contract](../../../docs/design/contracts/battle/spell-resolution.md) owns
resistance, truncated quarter adjustment, 1/32 critical, two downward variance draws, proportional
and lethal EXP contributions capped at49, battle halving and the shared two-draw award. A promoted
physical definition supplies the existing5/4 power/effective-level operand; this does not implement
promotion services. Damage ignores physical defense/land/dodge/double/counter. Each target retains
its unrelated supported status. Per-target ordinary item-drop construction remains before the next
target and award. Prepared state carries movement/RNG/gold/custody; MP/HP/EXP are still unapplied.

The existing scene publishes one MP payment after one cast announcement, then ordered damaging
reactions/results/deaths, one award, later growth/learning, field removal and next control. Stale
commands/waits cannot repeat payment or damage. Later credit/growth failures retain the paid and
applied prefix and retry only the pending phase. Modern fire/wind drawing and bilingual shared
cards/messages provide functional delivery, including private admitted casts; no original magic
animation, hardware timing or effect-asset acquisition is claimed.

**Confirmed static provenance:** pinned `SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to `disasm/`:
`data/stats/spells/spelldefs.asm` (three selected records), `spellelements.asm` (BLAZE FIRE,
BLAST WIND), `sf2enums.asm` (FIRE bits6-7, WIND bits0-1),
`code/gameflow/battle/battleactions/castspell.asm` (`spellEffect_Blaze`, `spellEffect_Blast`),
`calculatespelldamage.asm` (`battlesceneScript_CalculateSpellDamage`, `AdjustSpellPower`), and
`code/gameflow/battle/battlefield/buildactionrangegrids.asm` (`BuildSpellRangeGrid`). `data/stats/allies/stats/allystats04.asm`
learns BLAZE1 at1/BLAZE2 at5; `allystats01.asm` learns BLAST1 at10. These are source rules,
not a pre-voyage level ceiling or proof of natural opportunities. The contract distinguishes its
BLAZE H3 evidence from adjacent static-supported BLAST behavior; full natural reach/timing stays
**Unknown** and does not start a runtime-acquisition queue.

[AttackSpellTests](../../tests/Sf2.Remake.Engine.Tests/AttackSpellTests.cs),
[session tests](../../tests/Sf2.Remake.Engine.Tests/AttackSpellSessionTests.cs) and
[admission/composition tests](../../tests/Sf2.Remake.Engine.Tests/AttackSpellAdmissionTests.cs)
exercise the actual engine, including changed resistance/seeds/levels/stations, failure atomicity,
multi-target death/growth and a supported connected outcome/growth/drop/return/re-entry composition.
`BattleDefinition.WithOutcome`/`WithDeployments` preserve all capability fields during those existing
Content replacements; actor growth/equipment replacements preserve resistance. The
[verification recipe](../development-and-verification.md#player-attack-spell-observation) owns actual
native inputs and resource limits. The [early enemy decision slice](#finite-early-enemy-spell-decisions)
adds BLAZE1/HEAL1; other damage families/ranks, silence,
Kazin recruitment and full Battle03-07 story are outside this outcome.

## Opening field inventory and use

The [Standard field-preparation agreement](../application/opening-session-lifecycle.md#implemented-field-preparation)
owns the finite #1422 outcome, original static policy and scope. `BattleInventory` shares immutable
four-word Add/Remove semantics; `BattleEquipment` shares class/slot choices, loadout changes and
derived attack. Battle callers retain their existing position, turn and profile admission. Application
field transactions consume carried members directly and publish only through `GameSession`.

Field Give supports self/change-hands and explicit full-pack exchange, including source return-slot
adjustment after compaction and same-slot unequip. Give/Drop recompute through the live equipment
accessor; retained progress and source words agree. Rare Drop uses the existing saturating Deals
counter and preserves receipt flags. Unknown custody stays unknown and rejects a rare discard.

`FieldRecovery` consumes separately admitted effects. HEAL/Medical Herb/Healing Seed have original
NoEffect without cost or gameplay RNG. Antidote consumes even on no effect; DETOX1 pays its admitted
MP cost with a zero clamp, even on no effect or low MP. Joined defeated/full-HP members can receive
poison cure; it neither revives nor increases HP. Unrelated status bits, EXP and seeds survive.
These are field caller rules, not battle target/EXP rules. Original runtime timing/reach remains
Unknown. Rings/curses, stat boosters and later effects have no field consumer in this slice.

## Authored Jaro victory recruitment

**High rigor:** [Issue #1371](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1371)
joins retained party resources, activation, membership caches and resumable outcomes. The bounded
outcome is an ordinary authored win, visible recruitment, reserve membership and usable field return.
The source-backed priority is ordered state preservation, including failure/retry prefixes; private
Battle32/Moun, original media/timing, physical alias reconstruction and wider party services are
non-goals. This is a child of the [six-family closure scope](special-mechanics-design.md#current-closure-scope).

**Confirmed static:** the accepted [Battle32 outcome owner](../../../docs/research/chapters/moun-nazca-grans/battle32-outcomes-and-moun-entry.md#physical-population-aliases-and-logical-jaro-state)
and its [victory tail](../../../docs/research/chapters/moun-nazca-grans/battle32-outcomes-and-moun-entry.md#victory-seed-after-loading-and-completion-tail)
use SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. Direct joins are
`data/battles/entries/battle32/cs_afterbattle.asm` commands38/39/45,
`code/common/scripting/map/mapscriptengine_2.asm:csc08_joinForce`,
`mapscriptengine_1.asm:csc54_joinForceAi`, `code/common/stats/battleparty.asm:JoinForce/LeaveBattleParty`,
and `code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm:EndAfterBattleCutscene`.
The zero AI operand clears only bit2 and never calls JoinForce. JoinForce sets joined, rebuilds lists,
then conditionally activates below12; it leaves preheld over-capacity membership intact. Leave clears
active55 and writes only combatant X=-1, without rebuilding lists or changing joined23/resources.
The common JoinForce0 rebuild precedes clear432/set532. Static order does not prove natural timing.

**Implemented authored boundary:** `jaro-victory.json` uses the existing ordinary outcome selection,
healing and saved return anchor. `JoinPartyMember` preserves an already retained actor, including a
dead actor, inventory and progress. Typed `ClearOutcomePartyAi` changes only activation bit2;
`LeaveOutcomeBattleParty` clears only active membership. Neither rebuilds lists. The unchanged
outcome tail joins0 and rebuilds before completing the battle. Held532 skips the entire recruitment
suffix, with no membership repair. Already joined does not suppress first-completion ceremony.

The party returned by `BattleOutcome` already discards battle coordinates. Reserve membership and
absence from the returned authored map are the observable removal here. There is no raw X/Y state,
transient X-only emulation, permanent Jaro follower or promised next active turn. Original physical
following and full Battle32/Moun composition remain unbound.

Acceptance uses actual `JaroVictoryTests` and `JaroVictoryProgramTests`: converted/unconverted,
joined/unjoined, dead/live, capacity below/at/above12, other activation bits, resource preservation,
stale/wrong input, incomplete waits and failures/retries after join, AI clear and leave. The
[session owner](../application/session-and-programs.md#jaro-victory-program-continuation),
[content admission](../content/profiles-and-trust.md#authored-jaro-victory-admission) and
[verification recipe](../development-and-verification.md#jaro-victory-authored-observation) own
interfaces and execution. Preserve prior failures, #1047 Unknowns and the #638 pause. Rollback is
this finite admission/package if acceptance fails; do not change shared source rules or weaken evidence.

## Authored poison after-turn

**High rigor:** the finite poison slice joins action publication, timed notices, death accounting,
outcome and queue ownership. Its acceptance boundary is standalone authored status0/POISON2 with
fixed2-HP damage, the finite sleep/silence composition above, exactly-once ordered completion and preserved failure
prefixes. It reuses the existing action, field-death and outcome owners; no general status scheduler.

**Confirmed, static:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/gameflow/battle/battleloop/processafterturneffects.asm` (`0x24242..0x2448A`)
skips an initially dead actor, emits307, calls `DecreaseCurrentHp` with `POISON_DAMAGE=2`, emits308
on zero HP, records the dead actor and calls `UpdateCombatantStats`. `sf2enums.asm` defines
`STATUSEFFECT_POISON=2` and damage2. Common `stats/combatantstats_2.asm` clamps HP at zero.
Poison alone has no derived-stat modifier, so ordinary ATT-only equipment and admitted growth retain
current HP and recompute their existing values without a new status refresh implementation.

`battleloop_1.asm @IsBattleEnd` runs the defeated hook/action death/count pass first; an action outcome
skips after-turn. It then runs after-turn, death/count and queue advance, with no second defeated hook.
`battleloop/processkilledcombatants.asm` increments the dead ally's defeats or the retained
`BATTLESCENE_FIRST_ALLY` kills for a dead enemy, then clears position/status and refreshes stats.
`battleactions/battleactionsengine_1.asm` initializes that context when a scene is built; no-scene STAY
retains it. `IncreaseKills` returns for a negative source index (explicit FF sentinel). Poison neither
invents an attacker nor grants EXP/gold/drop rewards. `countremainingcombatants.asm` counts placed
living units and forces zero allies for zero-HP leader; defeat precedes victory.

`PoisonTurnRules` supplies fixed clamping and eligibility. In an opted-in encounter, pure poison allows movement, physical,
admitted healing/items, Give, free Equip/Drop and growth. Sleep and silence may combine with poison
through the finite lifecycle above; other mixed statuses, poison infliction, DETOX/cure,
regeneration/buffs and PrismLaser's separate status-free profile remain outside scope. Content uses
an explicit standalone opt-in and initial retained credit; [admission](../content/profiles-and-trust.md#authored-poison-admission)
and [execution](../application/session-and-programs.md#poison-after-turn-continuation) own those boundaries.
Private initialized low status bits do not enable this capability: the physical, source-AI and
reached-growth boundaries keep their existing Unsupported failures. `PoisonTurnTests` reproduces
that path through the actual `NewBattle` initializer using project-authored counterexamples.

**Unknown:** natural original reach, hardware/message timing and general mixed-status lifecycle.
The static rules and authored engine tests do not close these gaps or #1047's retained Unknowns.
Reproduce the bounded engine behavior with `PoisonTurnTests` and `PoisonSessionTests`; the
[native recipe](../development-and-verification.md#poison-authored-observation) owns actual delivery.

## Authored battle discard

**Standard rigor agreement:** standalone authored Drop is a bounded extension of existing packed
loadout, live equipment, custody and typed delivery owners. Select an exact held slot, refuse
protected items or confirm/cancel, then publish removal/compaction and any rare Deals addition once.
Result reveal/ack returns the same actor to the same ActionChoice and movement preview. Drop commits
no movement, action cost, STAY, after-turn, outcome/defeated hook, reward or RNG. Acceptance is varied
actual rule/session behavior, adapter builds, one ordinary-input native witness and exact-head CI.
No generic inventory framework, second authority or field/service/persistence expansion is admitted.

**Confirmed (static):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to
`disasm/`, `code/gameflow/battle/battlefunctions/battlefunctions_2.asm`
`@ItemMenuChoice_Drop` through `byte_25066`: slot cancel returns to the item submenu; equipped-curse
refusal precedes unsellable refusal. The selected packed word and selected slot are distinct operands.
Confirmation-no returns `@StartBattleMenu`. Yes calls `RemoveItemBySlot`, tests the saved item's RARE
bit, conditionally calls `AddItemToDeals`, displays the result and returns `@StartBattleMenu`.
`code/common/stats/itemstats.asm` `RemoveItemBySlot/RemoveAndArrangeItems` shifts later words,
appends Nothing and invokes `UpdateCombatantStats`. The battle caller does not use `DropItemBySlot`
to supply its guards. `code/common/stats/dealsinventory.asm` `AddItemToDeals/GetDealsItemInfo` masks
identity and increments the selected independent nibble up to15; reaching15 still permits removal.
`data/stats/items/itemdefs.asm` identifies0 as CONSUMABLE,61 as WEAPON|RARE and113 as RARE|UNSELLABLE.
Reproduce with `git -C <pinned-upstream> show c834c652:disasm/<named-file>` and the named symbols.
The [battle caller](../../../docs/design/contracts/battle/battle-functions-control-flow.md),
[Deals contract](../../../docs/design/contracts/party/caravan-and-deals-state.md) and
[economy synthesis](../../../docs/design/synthesis/themes/progression-and-economy.md)
remain the consumed contract routes; these are static claims, not natural-route/timing observations.

`BattleDiscard` validates the live actor, supported words, exact slot/word and selected metadata.
Unsellable113 is carryable but refuses removal. Rare61 needs observed custody; null stays Unknown,
never known-empty. `BattleItemCustody.AddDeal` preserves other entries and enemy onceFlags. Shared
`BattleEquipment.RemoveItem` compacts exact words and refreshes ATT from the actor's current base
plus remaining equipped contribution once. Spells, immutable definitions, resources, positions,
queue and seeds remain unchanged; range, subsequent equip/use and growth read the current loadout.
An overlapping enemy-drop catalog must agree about rarity. Four held slots bound selection to O(4);
publication copies the existing actor list O(N), with no new acquisition or unbounded work.

The [admission owner](../content/profiles-and-trust.md#authored-battle-discard-admission),
[session owner](../application/session-and-programs.md#battle-discard-selection-and-completion) and
[native recipe](../development-and-verification.md#battle-discard-authored-observation) own execution.
Modern feedback and menu cancellation are authored interactions. Names/arena/weapon bonus/range
operands are example content. Curses, rings, broken/status effects, wider item use, field/Caravan/shop
services, private/connected imports and save/load remain Unsupported. **Unknown:** natural original
reach and original presentation/timing; the twelve #1047 Unknowns and #638 pause remain unchanged.

## Early classes and ranged physical attacks

**Implemented capability, bounded:** SDMN/KNTE/WARR/PRST/MAGE/ACHR/THIF physical definitions
map to source class bits 0/1/2/4/3/5/10. Archer and Mage retain distinct mover identities even
though their supported terrain costs equal Regular. Enemy physical target class preferences select the
source Regular/Flying/Archer/Mage table by the attacker's mover; a Mage physical definition does
not implement BLAZE or spell AI. THIF's fixed prowess uses critical 1/16 plus quarter damage,
double 1/16 and counter 1/32. Other admitted profiles keep double/counter 1/32.

Player query, confirmation, physical construction and counter validation use the live equipped
minimum/maximum range. Enemy physical candidates query that same selected rule's range and target
set, scan the existing row-major diamond, retain the origin when already in range and otherwise
choose the first cheapest unoccupied reachable station. The scan includes every distance between
the two bounds; bow 2-2 cannot attack from distance1. Existing movement costs, occupancy, commandset
6/7/9/14 ordering and fallback remain authoritative. Range-only AI queries do not require reward
consumers until a reachable attack exists. Source MOVE's existing unsupported cost-domain boundary
remains explicit; this slice does not replace that source branch or its failure contract.

ATT-only ordinary enemy gear is immutable loadout metadata, not a player equipment menu. Fresh
Normal initialization applies the existing source base-ATT adjustment, then the selected weapon
contribution once (clamp200). Player equip/unequip stays free and refreshes the live base once;
carried slot words, gold, growth and both RNG channels retain their existing transaction boundaries.
Unknown usable items, broken/cursed gear and unsupported effects still reject; the
[finite enemy BLAZE1/HEAL1 extension](#finite-early-enemy-spell-decisions) owns the admitted spell decisions.

**Confirmed static:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, paths
relative to `disasm/`: `data/stats/allies/classes/classdefs.asm` rows0-5/10;
`data/stats/allies/allystartdefs.asm` Kazin/Slade; `data/stats/items/itemdefs.asm` records45/72/73/85/103;
`data/battles/global/landeffectsettingsandmovecosts.asm` mover1/2/8/11;
`data/battles/global/aipriority.asm`; `code/gameflow/battle/battlefield/determineattackposition.asm`
`DetermineAttackPosition` (`0xC71A..0xC7F2`), AI `command/attack.asm`,
`battleloop/initializecombatants.asm:InitializeEnemyStats`, and
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats`. The latter initialize current
stats before applying equipped contributions. [Battle05 research](../../../docs/research/chapters/grans-opening/battle05-initialization-and-first-control.md)
owns actual enemy/start composition; these definitions do not establish natural reach or timing.

[early-ranged.json](../../content/authored/early-ranged.json), `EarlyRangedPhysicalTests` and
`EarlyClassAdmissionTests` exercise actual actions, live equipment, changed valid levels/content,
source commandsets, illegal/atomic refusal and controlled pinned early definitions. See
[admission](../content/profiles-and-trust.md#early-physical-definition-admission) and
[native reproduction](../development-and-verification.md#early-ranged-physical-observation).
Full Battle03-07 story, Kazin/Slade recruitment, new private scene assets and original presentation
are separate admissions/Unknowns, not consequences of these capabilities.

## Authored battle weapon equipment

`BattleEquipment` operates on existing `SourceLoadout` slot words. An explicitly opted-in actor has
one immutable starting base ATT; `BattleActorProgress.BaseAttack` takes over after growth. The live
first equipped admitted weapon contributes ATT, clamped to 200. Unarmed contributes zero and uses
range 1. `BattleGrowthRules`, actor reconstruction, physical query/target/resolution and
`BattleActionResolution` validation consume this live contribution/range. Counter range belongs to
the actual counterattacker. Profiles without mutable equipment retain their frozen definition/growth
behavior. Neither repeated confirmation nor growth stacks the old bonus.

Eligibility uses weapon type and the source class bit (`1 << classId`). Offers preserve carried slot
order and duplicate identities. Zero eligible weapons yields no interaction; one to three eligible
slots include Unarmed sentinel4; four eligible slots overwrite the source menu's Unarmed cell.
Selecting a slot clears only admitted weapon equipped bits and sets the chosen slot's bit128. No
inventory removal, compaction, item transfer, other flags, resource payment, movement commit or turn
consumption occurs. Broken weapons, equipped nonweapons, multiple equipped weapons, wrong-class
initial equipment, unsupported classes/status/control and inconsistent live ATT reject explicitly.

**Confirmed static provenance:** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to
`disasm/`: `code/common/stats/itemstats.asm:GetEquippableWeapons/GetEquippableItemsByType`,
`IsItemEquippable/IsWeaponOrRingEquippable`, `GetEquippedWeapon`, `EquipItemBySlot` and
`UnequipItemBySlotIfNotCursed`; `code/gameflow/battle/battlefunctions/battlefunctions_2.asm`
`@CheckItemMenuChoice_Equip`, `sub_24C4E`, `CreatePulsatingAttackRangeGrid`, `EquipNewItemInBattle`
and `loc_24C44`. The source pre-fills the fourth menu cell with Unarmed/slot4, then overwrites entries
sequentially. It restores old preview equipment before the final equip helper, and returns to
`@StartBattleMenu`. The source's cursed refusal exists but is outside this admitted subset.

The [item-definition contract](../../../docs/design/contracts/party/item-definition-data.md),
[level refresh](../../../docs/design/contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh)
and [equipment synthesis](../../../docs/design/synthesis/themes/progression-and-economy.md#carried-gear-becomes-an-equipped-contribution)
own original metadata/refresh evidence. Pure immutable candidate calculation is the remake's preview
implementation, not an observation of original UI timing. `BattleEquipmentTests` and
`BattleEquipmentSessionTests` reproduce engine behavior with varied base stats, masks, names, ranges,
slots, nonzero growth, invalid state and current counter equipment. The original natural route and
full equipment/status fidelity remain **Unknown**.

## Authored AURA group healing

`HealingSpellDefinition.Targets` distinguishes Single, AuraRadius and AuraAllAllies. Ordinary
HEAL/Herb keep their existing scalar behavior. AURA is a finite family; authored IDs are references,
not legality predicates. Source order comes from existing `Initialization.SourceCombatants`,
independently of Prism, processing order or actor names.

| Level | MP | Center range | Radius | Power |
| --- | --- | --- | --- | --- |
| 1 | 7 | 0–3 | 1 | 15 |
| 2 | 11 | 0–3 | 2 | 15 |
| 3 | 15 | 0–3 | 2 | 30 |
| 4 | 20 | 0 | dedicated all-ally scan | full missing HP per target |

AURA1–3 select a living placed non-neutral teammate center using the provisional caster position.
Inclusive Manhattan radius selects living placed non-neutral allies, sorted by source index.
AURA4's range-zero center is self, still selected through the non-neutral grid. Its later slot 0–29
scan includes every living placed ally, including neutral/distant allies. Dead/unplaced actors never
recover. Source grid writes in increasing index order; production admission already forbids living
cell overlap and out-of-map placement, so invalid-memory overwrite behavior is not emulated.

Recovery is min(power, missing HP); AURA4's power 255 sentinel resolves each target's missing HP.
An AURA encounter admits maximum HP 1–32767: bsc0B interprets its delta as signed, and IncreaseAndClampWord
also checks the sign of currentHP+delta. Higher domains are Unsupported rather than emulated.
The ally priest contributes max(10, floor(25*recovery/maxHP)) per target to one accumulator capped at 25.
Full HP still contributes 10. The progression owner emits ordered `healing-exp-contribution` facts
(target, accumulator before/after), then one range 16 plus/minus pair and one award, without teammate
halving. Credit/growth retain their later boundaries.

**Confirmed static provenance:** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`data/stats/spells/spelldefs.asm` AURA rows and these paths under `code/gameflow/battle/`:

- `battlefunctions/battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput` spell MP/range/choice branch;
  `battlefield/buildactionrangegrids.asm:BuildSpellRangeGrid/ApplyRelativeCoordinatesListToGrid`;
  `battlefield/buildtargetsarray.asm:BuildTargetsArray`;
  `battlefield/populatetargetslist.asm:PopulateTargetsList/PopulateTargetsListWithAllAllies`.
- `battleactions/sorttargets.asm:battlesceneScript_SortTargets`;
  `battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`;
  `battleactions/createbattlesceneanimation.asm:battlesceneScript_PerformAnimation`;
  `battleactions/castspell.asm:spellEffect_Heal`.
- `battleactions/earnexp.asm:battlesceneScript_CalculateHealingExp/battlesceneScript_AddExpAndApplyHealingCap`;
  `battleactions/giveexpandgold.asm:battlesceneScript_GiveExpAndGold`;
  `battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction/bsc0F_giveExp`.

The signed addition owner is `code/common/stats/combatantstats_3.asm:IncreaseAndClampWord`.
Reproduce inspection with `git -C <registered-upstream-checkout> show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/<path>`.
The accepted [AURA contract](../../../docs/design/contracts/battle/spell-resolution.md#confirmed-aura-target-and-accumulation-subset)
retains bounded H3 geometry/accumulation evidence. These joins establish structure, not natural
original casting or presentation timing; engine tests are not original-game evidence.

## Jaro conversion and source ally commandset6

The [scope agreement](../capability-status.md#authored-jaro-conversion-agreement) admits one authored
composition. `ConvertBattleActor(enemySlot, allySlot)` binds two existing logical actors through
`SourceCombatants`. It reads retained enemy placement, writes enemy currentHP0/absent placement,
copies only coordinates to the ally, and leaves each actor's definition, maximums, MP, stats,
loadout, status and activation intact. Dead allies remain dead. A null retained coordinate stays
absent; it is never replaced with the enemy definition's fresh starting coordinate.

`MapPartyMembership.Rebuild` uses the shared story flags. Conversion sets joined, rebuilds the list
snapshot, conditionally writes active below12, then explicitly writes active regardless of capacity.
The list can therefore lag the active flag. This named source operation permits thirteen active
flags without changing general party capacity. The alias copy runs last and can fail after those
committed writes. `JoinBattleForceAi(nonzero)` preserves the activation word OR4 across JoinForce's
joined/list/conditional-active writes, then publishes it. Zero clears only bit2 and does not join or
rebuild. These are the source subroutine/opcode orders, not an atomic roster transaction.

`BattleDeploymentDefinition.SourceAllyControl` explicitly selects live activation bit2 as the
player/automatic authority. The immutable strategy is `source-ally-physical`; ordinary authored
control retains its earlier meaning. Queue generation still uses live placement/HP/agility, and
sleep/dead-entry handling remains in ordinary turn flow. No enemy slot is substituted into the queue.

**Confirmed static clarification:** at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to `disasm/`:

- `code/gameflow/battle/ai/startaicontrol.asm:StartAiControl` sends allies directly to
  `@HandleAiCommandset` with6, skipping enemy checked-word clear, activation/standby, special-attacker,
  dead-follow promotion and path-mode preprocessing.
- `data/battles/global/aicommandsets.asm:AiCommandset06` orders ATTACK1, HEAL1, SUPPORT, MOVE1, STAY.
  `ai/command/support.asm:aiCommand_Support` immediately fails for an ally. The admitted loadout has
  four empty item127 slots and four source spell63 slots, no learned spells, ordinary range1 physical
  attack and difficulty0. Item/spell/neutral/MUDDLE/auto-battle and other difficulty paths remain Unsupported.
- `ai/command/attack/prioritizetargets.asm:AdjustTargetPriorityForAttackByDifficulty` selects Script3
  for an ally at difficulty0. Opponent targets follow ascending source slots; reachable priorities
  draw thinking RNG in reverse order. `PhysicalTargetRules.ScriptThree` is reused only for that raw
  score. `ai/command/attack/determinebattleaction.asm:@MultipleTargets` bypasses enemy >=15 class
  preference for allies. The raw-max cohort is collected backwards; the final signed movement
  comparison selects greatest movement, replacing equal values. Output priority caps at15. Selection
  draws no additional RNG. `ai/command/attack.asm:@ValidTarget_Attack` skips the enemy-only LastTarget write.
- `ai/command/move.asm:aiCommand_Move` targets living placed opponents. Its post-sort enemy-bit test
  reads the final cost in d0, not the mover ID. The shared `AiMovementRules.Pursue` is reused only for
  complete0..127 raw target costs, stable nearest-target order, approach and legal occupancy. No
  attack runs HEAL1/SUPPORT failure then MOVE1 without RNG. Unreachable/128 costs remain Unsupported.

The abbreviated AI paths above share `code/gameflow/battle/`. The accepted
[conversion research](../../../docs/research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md#logical-conversion-membership-and-physical-aliases)
and [AI contract](../../../docs/design/contracts/battle/battle-ai-decision.md) retain original evidence.
`JaroConversionTests`, `BattleRegionProgramTests` and `SourceAllyAiTests` exercise actual remake
behavior, including changed IDs/slots/state, prefix failures and automatic-to-manual transition.
Natural original scene reach, original alias timing and original presentation remain **Unknown**.

## Hidden-enemy reset and source regions

**Confirmed static source:** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, relative to
`disasm/`: `code/gameflow/battle/battleloop_1.asm`, `battleloop/initializecombatants.asm`,
`battleloop/populateenemyspawns.asm`, `battleloop/triggerregions.asm`, `battleloop/spawnenemy.asm`,
`battleloop/clearaimemory.asm` and `ai/startaicontrol.asm` (the latter paths share
`code/gameflow/battle/`). The [research owner](../../../docs/research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#regions-checked-bits-hidden-appearance-and-reappearance)
retains original provenance and natural-reach limits. Engine tests prove the remake implementation.

`BattleActorState.Initialized` selects zero effective maximum HP when dormant; initialized maximum HP
still comes from definition/progress. It is not a second numerical maximum. Fresh source initialization
parks modes2/3 with mode bits alone; controlled input explicitly declares its state/activation.
`BattleSpawnRules.Populate` scans explicit enemy source slots, independently of processing/display
order. Dead actors require a held non15 primary or secondary region; primary wins. Activation bits
are set before mode2's maxHP0 test. Mode3 has no maxHP test. Ordinary death preserves initialized
maximum HP; it never invokes spawn reset.

Reset tests the declared enemy bound and every retained coordinate, including self/dead occupants.
Success reloads HP/MP, source attack plus difficulty0 quarter, status, loadout, placement and original
orders, preserving the passed activation word and external `AiMemory`/`LastTarget` arrays. Refusal
sets dormant/HP0 and absent placement without clearing activation or region history, so a later
eligible reset may succeed. The complete reset list is committed before any appearance; subsequent
candidates see earlier successful placements. Definition loadouts are restricted to unequipped Herb0
or empty127 and source spell63; equipment/general difficulty refresh remains closed.

`SourceRegionGeometry` implements the source dominant-axis signed integer projection for coordinates
0..47, where products fit the sign-extended low word. Division truncates toward zero. Triangles use
1/2/3; quads use 1/2/4 then3/2/4. Edge1→2,3→1,3→2 preserve immediate in-bounds returns, exception
branches and comparisons with the opposite vertex. The instruction sequence, rather than its comment,
is authoritative: negative projection uses `move #1,ccr` and returns2; zero denominator uses
`ori #1,ccr`, retains Z and returns3. This deliberately differs from ideal polygon geometry and
retains degenerate/reversed/integer-edge behavior in the admitted finite domain.

`BattleRegionState.Tested` persists across rounds. Living placed enemies supply activation calls;
without one, geometry is not evaluated. Positive signed-HP allies supply candidate points. Held
flags never clear on a false result. Existing ordinary source AI clears the checked word before its
activation/standby work; an authored Stay policy does not imitate that source call. Fresh encounters
start with explicitly empty region state. `HiddenSpawnTests` and `HiddenSpawnSessionTests` exercise
these rules and their ordinary scene/return consumers, including state retained after later failure.

## Finite Prism laser

`PrismLaserDefinition` binds each actor to an explicit source combatant slot (allies0–29,
enemies128–159) and each Flower to a cardinal direction0right/1up/2left/3down. Processing order,
actor name and display facing do not select the ray. `PrismLaserDecision` implements the admitted
kind93 branch under ordinary enemy automatic control and unconditional15/15 activation. Existing
sleep turn handling prevents a sleeping actor from executing; other actor status/control and
non15/standby configurations remain Unsupported.

**Confirmed static source:** SF2DISASM pin `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/ai/startaicontrol.asm` (`StartAiControl`, `ProcessLineAttackerAi`),
`getlaserfacing.asm` (`GetLaserFacing`), `battleactions/battleactionsengine_1.asm`
(`battlesceneScript_DetermineTargetsByAction`, `battlesceneScript_InitializeActors`),
`battleactions/battleactionsengine_2.asm` (`battlesceneScript_ApplyActionEffect`),
`battleactions/inflictdamage.asm` (`battlesceneScript_InflictDamage`), `battleloop/processkilledcombatants.asm` (`ProcessKilledCombatants`) and `code/common/stats/combatantstats_2.asm`
(`IncreaseKills`). The [laser research owner](../../../docs/research/chapters/moun-nazca-grans/battle36-prism-flower-laser.md)
retains the source ray/occupancy/RNG/message boundary; its historical consumer-gap audit describes
its inspected baseline, not this authored implementation. No new original runtime finding is claimed.

Decision scans one cardinal ray over48×48 tactical coordinates, omitting HP0, absent/off-grid and
neutral-bit3 actors, and chooses the nearest eligible ally or Stay without movement or RNG.
Preparation uses current occupancy from both factions, sorts by explicit source slot, and ignores
whether the old focus remains eligible. Walls, terrain, ordinary range and rendered crop do not stop
the ray. Eligible overlap is explicitly rejected instead of approximating source overwrite semantics.
All edge origins, signed-high/malformed HP, unknown activation words, target statuses and special
kinds remain closed. The authored binding declares ordinary initial activation word0; a missing
binding is not an inferred original state.

Each ordinary target receives base16 minus two main-stream `RNG(3)` results, with minimum1 before
HP clamp. The same range is used twice and the stream continues in sorted order; thinking RNG is
unchanged. Construction writes only its RNG, preserving HP/MP/status for reaction playback. The
calculated amount remains distinct from actual HP loss. It never enters ATT/DEF, land, resistance,
MP, dodge/critical, counter/double, reward or item-drop algorithms. Existing reaction-motion RNG
runs later at its own boundary; this is not a claim about total original animation RNG.

The accounting source join is explicit: action6 writes FIRST_ALLY=FF in InitializeActors;
ProcessKilledCombatants calls IncreaseKills for an enemy death, but IncreaseKills tests the signed
byte and returns for FF. `BattleDeathCredit(null)` therefore means **known no ally**; a null context
means **unknown**, while a context containing an ally retains ordinary scene credit. Laser entry
replaces prior credit even with zero targets. Ally deaths increment their own defeats; enemy deaths
and leader sweeps remove without kills, EXP, gold or invented drops. Unknown credit still rejects
before death publication. Ordinary physical and healing validators keep their own target rules.

`PrismLaserTests` and `PrismLaserSessionTests` exercise the rule and actual automatic/scene/field
consumers, including empty reconstruction, changed IDs/order, stale completion and retry after a
later accounting failure. The authored [host observation](../development-and-verification.md#prism-laser-authored-observation)
establishes usable modern delivery; natural original reach and hardware fidelity remain **Unknown**.


## Finite physical-action item drops

`EnemyItemDropRules` is a fixed SF2 physical-construction service. Its evidence owners are the
[combat contract](../../../docs/design/contracts/battle/combat-resolution.md) and
[Taros action-drop custody](../../../docs/research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md#taros-sword-action-drop-custody),
including `sf2-enemy-item-drops-v1` and `sf2-enemy-item-drop-behavior-v1`. **Confirmed static source:**
pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/battleactions/{battleactionsengine_1,dropenemyitem}.asm`,
`code/common/stats/itemstats.asm` (`GetItemSlotContainingIndex`, `RemoveItemBySlot`, `AddItem`),
and `data/stats/items/itemdefs.asm`. Source inspection distinguishes the helper's mutation from
the generated `displayMessage` commands; authored observations do not prove natural original reach.

After each first/second/counter effect, the helper requires ally actor, enemy target, local target
death, matching source battle/combatant slot and an actually held mapped item. Rows retain their
order, including continuing past a matching battle/slot row whose item is absent. The first matching
inventory slot wins. Actor names, processing order, `Physical.Leader` and enemy-definition identities
do not select a drop. Taros Sword110, Iron Ball102 and Counter Sword68 alone draw `RNG(32)` and require
zero; this occurs before testing the once bit. Other admitted items, including rare Achilles61,
do not draw. The RARE attribute controls only fallback.

A clear once bit is set, the enemy's slot is removed/compacted, then a living actor receives the
masked identity in its first empty slot. Enemy equipped/AI/broken status is not received; there is
no automatic equip or other-ally search. Dead/full recipients route rare items to independent
per-item Deals counts capped at15, while non-rare items are discarded. The source emits dropped
message396 and received message395 only after successful direct receipt. Fallback creates no
message/acknowledgement. Leader sweep never invokes this action helper.

Construction retains immutable inventory/custody and the ordered RNG/removal/receipt facts before
end EXP/gold construction and later HP/status/reward playback. `BattleActionResolution` validates
the helper's exact finite effect suffix after each reaction and its resulting state; it does not
accept arbitrary inventory writes from a replacement physical rule. Re-evaluation is pure candidate
validation, not a second publication or RNG stream. Queries do not construct actions. A later wait,
credit, growth or outcome failure retains the accepted prefix and cannot repeat these writes.

`EnemyItemDropTests` and `EnemyItemDropSessionTests` reproduce the actual rule/session behavior.
The [Content owner](../content/profiles-and-trust.md#authored-item-drop-binding) closes admission to
the supported metadata; [Application](../application/session-and-programs.md#battle-item-custody-and-receipt-playback)
owns cross-mode custody. This is physical-only authored receipt support, not private Battle18,
general item acquisition/equip, shops or cross-session persistence.

## Finite enemy-leader cleanup

An authored v8 encounter can bind `LeaderCleanupDefinition` through the
[closed Content binding](../content/profiles-and-trust.md#authored-leader-cleanup-binding).
It maps Bowie and each enemy source slot explicitly. Enemy128 is the defeated-hook gate;
`Physical.Leader`, processing order and Taros enemy-definition88 are separate identities.
`BattleOutcomeRules.DefeatedHook` requires living Bowie and zero-HP gate. Held completion skips
only the empty defeated program, not the leader tail. Nonempty programs and private leader
encounters remain Unsupported.

`BattleDeathBatch` retains action positive-to-zero deaths in reaction order, then appends every
positive-HP enemy in source-slot order, including already-unplaced actors. Forced entries are
explicitly distinguished from action deaths. The sweep never changes HP or constructs attacks.
At accepted field exit, each enemy entry credits a kill when the retained context contains an ally;
known-none laser custody suppresses that increment. Allied action deaths credit their own defeats. The existing capped counter policy clears placement/status.
Admitted sleep, silence and poison have no stat modifier to recompute; wider stat/status refresh remains unsupported.
Only the original action earns damage/end EXP and death gold; the sweep adds no EXP, gold or drops.
Outcome counts placed living combatants after cleanup and checks allied defeat first.

**Confirmed static support:** the [Battle18 outcome owner](../../../docs/research/chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md#outcome-selection-and-shared-battle-boundary)
anchors pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. Under
`disasm/code/gameflow/battle/`, `cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated`
and `battleendcutscenesend.asm:loc_47C48` own eligibility and the source-slot append;
`battleloop/processkilledcombatants.asm:ProcessKilledCombatants` owns spin/exit, kill-or-defeat
bookkeeping and removal. `battleloop_1.asm` performs defeat-before-victory after each killed-list
pass and calls the defeated hook only at the first checkpoint. `battlefunctions/executeindividualturn.asm`
clears the death list each turn; its no-action/dead branches do not initialize first-ally custody.
`battleactions/battleactionsengine_1.asm:battlesceneScript_InitializeActors` assigns that custody
from the admitted scene's initial ally (including the ally target of an enemy physical attack).
These static facts do not establish original natural reach or hardware timing.

## Finite Taros physical effectiveness

The optional `BattleDefinition.TarosEffectiveness` binds one admitted encounter to explicit source
identities. `Gameplay/Sf2/TarosEffectiveness` owns the source-specific definition88/weapon61 tests;
logical `ActorRef` identity and enemy-definition identity are separate from deployment order, name,
map, package and presentation. Content binds Bowie to an ally, enemy definitions to enemy actors,
and weapon item types. Absence of the binding leaves the ordinary physical rule unchanged.
The [authored package](../../content/authored/taros-effectiveness.json) is a controlled semantic
example, not a private Battle18 import. Its numerical ATT is already supplied; this does not add
an equipment/stat-refresh or equip-menu service by itself. The separate
[mutable authored equipment binding](#authored-battle-weapon-equipment) now supplies that bounded interaction.

At `Sf2PhysicalAction.Prepare`, compute one initial ineffective operand from actor, initial target
and live `BattleActorState.SourceLoadout`. Scan slots in order for equipped bit128, non-NOTHING
masked identity and the admitted weapon type. Only the bound Bowie with first equipped weapon61
satisfies the initial conjunction against definition88. Carrying61, another equipped actor,
an earlier equipped different weapon or a changed live loadout matters. Broken bit256 does not
change the masked identity; this getter does not recheck equip classes. Doubles reuse the operand.
Counter construction passes a cleared operand; separately, a mapped Taros original attacker
cannot be countered. Taros as original target can counter when the existing survival/status/range
and rolled-toggle checks allow it. The policy is physical only; it adds no spell/item immunity.

**Confirmed static source ordering:** at SF2DISASM
[`c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions),
`battleactionsengine_1.asm:WriteBattlesceneScript` initializes and calls
`determineineffectiveattack.asm` once. Its second-attack block retains the toggle; the counter
block clears it. `isabletocounterattack.asm` reads the original actor through a5 for the Taros
prohibition. `code/common/stats/itemstats.asm:GetEquippedWeapon/GetEquippedItemByType` owns the
live slot/type scan. The [original research owner](../../../docs/research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md#bowies-equipped-achilles-gate-and-its-lifetime)
retains provenance, Battle18/F112 context and original natural-reach limits.

| Reached physical step | Ineffective consequence |
| --- | --- |
| `attack.asm` / `determinedodge.asm` | Dodge comes first and wins; its existing status skip remains. A dodge does not emit ineffective. |
| `calculatedamage.asm` / `determinecriticalhit.asm` | Calculate and draw critical before the damage helper. Retain the draw; do not display a critical-damage result for an ineffective return. |
| `inflictdamage.asm` | Definition88 plus the initial toggle emits message289 and returns before counter halving, two spread draws, damage EXP, HP/death and kill/gold. Emit `Ineffective` with unchanged HP/MP, amount0, no damage status write or recoil. |
| `attack.asm` / `inflictailment.asm` | The target survives, so supported sleep-critical ailment can still be scheduled after the ineffective message. No new resistance/immunity predicate is inferred. |
| `determinedoubleandcounter.asm` | Surrounding double/counter draws still occur. Retain the current sticky counter flag and finite first/second/counter dispatch. |
| `battleactionsengine_2.asm:End` / `giveexpandgold.asm` | A surviving eligible ally still receives the normal two end-EXP draws and minimum1, even with zero accumulated damage EXP. An ineffective result does not fabricate kill/gold or suppress this source award. |

Sleep is a later script operand, not a construction-time live status mutation:
`sf2battlescenemacros.asm:executeEnemyReaction/executeAllyReaction` write to a6; the later
`battlescenes/battlesceneengine_0.asm:bsc0A/bsc0B` calls `SetStatusEffects`. Therefore newly scheduled
sleep cannot remove an already constructed second dodge draw or legal counter. Ineffective has no
damage status write, so subsequent ineffective playback does not clear earlier sleep. Existing
ordinary damaging reactions retain their independently constructed status operands.

`TarosEffectivenessTests` covers source-identity and live-slot variations, exact construction RNG,
double/counter lifetime, source-supported sleep, real command/scene waits, duplicate delivery and
reward-failure prefix retention. Existing `SleepAndFlyingTests` preserves the construction/playback
split. The [native recipe](../development-and-verification.md#taros-authored-observation) checks the
actual Godot message, HP delivery and resumed input. Full Battle18, all-action immunity, drops/Deals,
private leader cleanup and Caravan remain outside this physical implementation; original natural/hardware behavior
remains **Unknown** at its existing evidence boundary.

## Finite EGRESS action

The same finite retreat continuation also accepts Angel Wing through `Sf2ItemAction`, the ordinary
item-slot action. `BattleDefinition.RetreatItems` holds admitted item definitions separately from
healing items. Content keeps source ID4 in the SF2 binding: CONSUMABLE / EGRESS1 / range0, no equip
flags/effects; absent Wing definitions do not affect existing Herb/EGRESS packages. No MP or learned
spell is required for item use, and any living actionable player ally may confirm self. Query/cancel
is provisional. Confirmation revalidates control, selected packed word and target; preparation
compacts exactly the selected slot, preserving all survivor bits/order and appending127. Validation
allows only that loadout delta and selected movement: no MP/HP, reward, reaction or RNG writes.
`ItemRetreatPayment` and `SpellRetreatPayment` explicitly distinguish the two costs; Wing never uses
a dummy spell or an ordinary recovery scene. Existing Herb preparation/reaction/reward is retained.

Authored v7 items admit `"effect":"consumable-retreat"` with power0 and minimum/maximum range0.
The [source/payment join](../application/session-and-programs.md#paid-retreat-and-field-return) and
[Wing reproduction](../verification/field-programs.md#battle02-angel-wing-and-walking-retry) own the
staged return, controlled initial input and limits. Acquisition, equip/break/curse transactions and
wider item effects remain Unsupported.

`Sf2RetreatAction` implements the existing `IBattleActionRule` boundary for admitted retreat spells.
`BattleDefinition.RetreatSpells` is separate from healing definitions. Private Battle02 admission
checks source EGRESS1/base 10, cost 8, range 0/radius 0, power 0 and
`TYPE_SPECIAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE`; other effects remain explicit Unsupported offers.
The source special action bypasses ordinary CastSpell, so its metadata does not introduce a Silence
gate. Sleeping/dead/unplaced/non-player actors cannot acquire control through this action.

The action requires an actually learned spell and self target. Query does not prepare or draw RNG;
confirmation repeats admission against live MP and target. Finite validation permits only selected
movement in prepared state, with no reaction, reward, inventory, resource or RNG mutation. Application
then owns exactly one payment and the separate retreat continuation. Malformed candidate resources
or RNG reject before payment. Mixed healing/retreat offers preserve learned spell order.

Authored v7 spell input can use `"effect":{"kind":"retreat"}` with explicit `mpCost` and zero minimum/
maximum range; it needs an admitted field outcome route to return. Private bindings retain their
source checks and only admit Battle02. The two finite payment cases share the existing continuation.
[Payment/return](../application/session-and-programs.md#paid-retreat-and-field-return)
and the [capability agreement](../capability-status.md#battle02-egress-implementation-agreement) own
staged delivery, recovery and exclusions.

Domain owns deterministic calculations and immutable state transitions. Application selects one
immutable composition and owns when each prepared effect is published. Content admits data;
Godot presents semantic choices and effects. Start with [architecture](../architecture.md) for
assembly/state authority and [profiles](../content/profiles-and-trust.md) for admission.

## Scope and Fixed Services

The independently accepted finite [#617 scope](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/617)
supports replacing admitted HEAL/physical/Herb/Stay algorithms, automatic decisions, progression,
ordinary outcomes and source-story policy through compiled C# composition. External typed programs
provide conditional story authoring. Rebuild and start a new session; there is no hot reload,
runtime class discovery, general effect language, plugin sandbox or live rule switch.

Refresh, activation/control, movement-cost tables, field motion/text/music and battle-scene logical
services remain fixed source-specific algorithms. They are deliberate exclusions, not evidence
that all gameplay policy is replaceable. Broader replacement needs a separate scope decision.
Save/load, new effects/statuses/outcomes and complete game coverage are outside this boundary.
Compile-free extensible scripting remains **Unknown** as a future requirement.

The implementation used High rigor because rules, RNG, source compatibility and presentation share
commit boundaries. Its current agreement is the finite behavior and exclusions here; completed
dispatch/resource estimates remain in [the historical plan](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/architecture.md#replaceable-gameplay-logic-plan).
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) still awaits UI/UX discussion;
semantic affordances do not choose its layout/art. #638 remains paused and Chinese substantive
synchronization remains deferred.

### Implemented HEAL seam

[`SessionRules`](../../src/Sf2.Remake.Application/Runtime/SessionRules.cs) retains one internal
[`IHealingRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IHealingRule.cs), adapted to the common finite
action interface, for the session's lifetime.
[`RuleCompositions`](../../src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs) owns the default
SF2 and the two authored factories; its `ForGame` method is the ordinary host's compile/restart
selection. Source-start and definition/start overloads retain explicit composition without
mutable selection. Default direct starts use SF2. Diagnostic rule identity is separate from Content
profile and never chooses arithmetic in dispatcher/query/view.

[`Sf2HealingRule`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2HealingRule.cs) owns source admission
and preparation; the single [scalar](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingRules.cs) and
[target helper](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) are shared with Herb.
[`AuthoredHealingRules`](../../src/Sf2.Remake.Domain/Gameplay/Authored/AuthoredHealingRules.cs)
implements capped/proportional award A and injured-only half-missing/no-award B. The default
source class/award assertions remain independent of the demonstrations.

`GameSession.QueryBattleChoices` returns immutable session/revision/actor/stage/destination facts,
spell labels/presentation/range and ordered same-side living candidates with enabled/reason data.
Query runs no preparation or RNG and retains no future seed. The actual view consumes these spell
choices through the common action query described below. Confirmation
rechecks the selected rule and private cast capability, then `ValidateHealing` checks the existing
resolution before `Begin`: only selected movement/main construction RNG, bounded target recovery,
caster MP cost and later caster progress are permitted. Foreign state, queue, thinking RNG,
accounting, definition and observation effects cannot be published by malformed HEAL output.
Construction random facts must form a complete `BattleRandom` main-seed chain; their count,
purpose names and ranges belong to the selected rule. SF2's two range-16 EXP draws are asserted at
its own behavior boundary, rather than imposed on every HEAL implementation.
Unexpected selected-rule errors report `InvariantFailure` with rule/operation context, without
exposing exception details. Expected rule errors retain their existing categories. Source unknown
EXP remains a reached preparation failure; selection does not prematurely spend or require it.

The existing scene owns cost, HP, EXP, fairy work and later growth. Growth consumes the then-current
seed. A reached later growth failure retains the earlier committed resources, progress, token and
queue; no whole-action rollback or successful release is implied. The cursor's source operations
are unchanged. [Verification](../verification/battles.md#replaceable-heal-observation)
owns engine assertions and the bounded actual A/B observation. Original timing and additional action families remain outside this HEAL seam; the selected
AI/progression seams below and [story policy](../application/session-and-programs.md#source-story-policy)
have their own finite replacement boundaries.

### Implemented semantic actions

[`IBattleActionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs) consumes the finite
`BattleActionRef` (Stay, HEAL spell, physical, or inventory slot). Each selected implementation owns
its action offers/admission, ordered target candidates, target admission and preparation.
`SessionRules` binds one immutable instance per admitted family; the accepted HEAL strategy uses an
adapter to this interface, without a second calculator. `IPhysicalActionRule.EstimateDamage` supplies
the existing AI estimate as well as the same rule's preflight and eventual action preparation.

[`Sf2PhysicalAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs) and its single
[`PhysicalStrikeRules`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/PhysicalStrikeRules.cs) own source strike,
reversal/double/counter and land-protection policy. The movement-cost table stays in
`BattleTerrainRules`. [`Sf2ItemAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) owns the
admitted Herb calculation and source numeric item decoding; Content trust and the inventory model
are unchanged. [`Sf2StayAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2StayAction.cs) prepares ordinary
player Stay. Standalone scalar comparisons use explicitly named source-default convenience entries;
production entry, scene, movement and program continuations always pass the selected composition.

`QueryBattleChoices().Actions` orders physical, learned spells, actual inventory slots, then Stay.
Offers carry the action reference, label, presenter kind, range, empty state and enabled/reason data;
targets retain the rule's order, including disabled candidates. `Spells` and `Items` are derived
projections of those same offers. Empty packed source slots remain raw and unchanged in live state,
while the view receives semantic Empty/label data. Unknown or unsupported slots retain a diagnostic.
Selection remains provisional; `SelectBattleAction`, `ChooseAction`, `SelectSpell` and `SelectItem`
join the same admission path. Confirmation rechecks the live envelope, selected actor, destination,
action, target and supported scene before one common preparation/validation call. Godot cycles query
results and formats labels; it does not infer target factions or mask item words.

`BattleActionResolution.Validate` permits only the finite existing publications: selected movement
and main construction RNG, one selected consumed slot/compaction for Herb, and physical death gold;
HP/MP/reward/death accounting and queue consumption retain their later owners. It verifies immutable
state identity, complete main-seed chains, bounded ordered reactions, eligible reward recipients and
matching construction facts. It does not impose a source draw count on replacement algorithms.
Malformed or throwing rules fail before construction publishes; unknown accounting still rejects at
its reached source boundary. A failed post-movement preparation retains the earlier decision and
movement delivery, stops the automatic continuation, and publishes no damage or queue consumption.

The current `SourceEnemyAi`/`AttackThenApproachAi` policies pass the selected physical rule through
estimate, discarded preflight and actual post-movement execution. Their priorities, movement,
activation/memory and thinking RNG are preserved through the selected decision seam below. Reward,
growth and outcome policy use the selected seams below. The [verification owner](../verification/battles.md#semantic-action-selection-observation)
records actual choices/presenter checks and independent source-stage RNG expectations.

### Implemented automatic decisions

[`BattleStrategyRef`](../../src/Sf2.Remake.Domain/Battles/State/BattleStrategyRef.cs) is a content key,
independent of control/faction/processing order. Content parses the key without selecting C# code.
[`SessionRules`](../../src/Sf2.Remake.Application/Runtime/SessionRules.cs) snapshots an explicit composition
into an immutable key-to-rule map; session start checks every encounter, including dead/unreached
actors and battles that programs may enter later. Unknown, duplicate and missing bindings reject
before a session publishes. Selected deployment/start admission retains the physical-only empty
spellbook/item/MOV restriction and source initialization requirement; common status, faction, order,
provenance and supported source commandset/loadout guards remain at their existing boundaries.

[`IBattleDecisionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs) receives the live
immutable battle, actor and the same selected physical calculator used by player/query/scene work.
The scheduler calls the resolved rule; it owns neither target scoring nor strategy-name branches.
Source `AttackThenApproachAi`/`SourceEnemyAi`, scoring/priority and standby/pursuit helpers live in
`Domain/Gameplay/Sf2`. [`Sf2ClassRules`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ClassRules.cs) is the
single named-class/source-ID mapping used by AI and private growth admission; AI rank remains in
`PhysicalTargetRules`. Shared decreasing-cost walk/move-string primitives remain in `BattleMovement`.

The finite `BattleAutomaticAction.Validate` authority admits only the selected actor's decision
memory/last target/activation and carried thinking draws, plus the existing tested-region clear.
It preserves definition/deployments, positions, resources, main seed, queue/round, accounting and
unrelated actors. Routes require matching endpoints, adjacent traversable steps within MOV budget,
an unoccupied destination and a live opposing physical target. Source occupancy/commandset policy
still belongs to the selected source rule. `QueueOnly` expresses the existing no-action Stay result;
it requires the unchanged battle and origin-only route, then consumes one queue entry without an
action publication. Source MOVE1 origin Stay retains ordinary movement/action completion instead.

A selected physical target receives one discarded preflight before movement. Thinking and decision
memory publish once with the route, while actual physical construction waits for movement completion.
No preflight's future main seed is retained, no completion reruns thinking, and the same physical
instance handles estimate/preflight/execution. Throwing rules or malformed decisions become typed
InvariantFailure with identity/operation; unsupported source capability stays explicit. Failure
before decision publication retains earlier actions and the current queue item; a later construction
failure preserves the committed decision/delivery. The accepted HEAL release distinction between
same-step rejection and a subsequent automatic failure is unchanged.

`RuleCompositions.AuthoredFirstLegal()` and `AuthoredLowestHp()` bind the same `authored-priority`
reference in [`decision-rule-demo.json`](../../content/authored/decision-rule-demo.json) to different
ordinary C# algorithms. FirstLegal selects the first reachable physical target by processing order;
LowestHp selects lowest current HP with that same stable tie. Both use selected physical range/target
admission, the existing movement grid/preview and the common presenter; neither copies source scoring.
These are authored demonstration policies, not fidelity profiles or added source action families.
`ForGame()=>Sf2()` is the restored default. [Verification](../verification/battles.md#replaceable-automatic-decision-observation)
owns rebuild/restart selection, actual consumer recipes and independent RNG expectations.

### Implemented progression and outcome policies

`SessionRules` retains one [`IBattleProgressionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleProgressionRule.cs),
[`IBattleOutcomeRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleOutcomeRule.cs) and
[`IOutcomeReturnPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs).
The progression policy computes eligible physical/counter, HEAL and Herb awards, gold/counters,
credit and growth. SF2 arithmetic lives under `Domain/Gameplay/Sf2`; action preparation no longer
calculates source award draws before consulting a replacement. Half-missing HEAL still has no reward.
The common action and AI preflight receives the same selected policy, validates its finite output
on discarded state, and never publishes preview resources or RNG.

Credit precedes EXP text; growth follows acknowledgement using the then-live seed, including HEAL
fairy work. Common validators constrain actor identity, allowed state mutations, matching facts,
admitted spell mappings and a complete main-seed chain. Source caps and numerical draw schedules
belong to source assertions. Malformed/throwing policies report `InvariantFailure` with identity and
operation. A reached credit/growth/death/outcome failure retains the current stage, prior commits,
token and queue. Program policy faults retain the accepted cursor; retry resumes without replaying
acknowledged text or credit. Existing unsupported AI stop behavior is unchanged.

The source outcome policy owns leader-loss precedence and the defeated hook. The source return
policy selects programs/anchors, living/immortal recovery, completed-flag skip, join/flag decisions
and defeat gold. `BattleOutcome` and `ProgramRunner` still publish and execute the existing stages.
One validated finish decision precedes join/flag publications; no later policy call can discard that
partially published tail. No new continuation state or generic effect interpreter is introduced.

`AuthoredProgressionOutcome()` demonstrates fixed7 eligible awards with no award draws and a
saturating10-gold defeat cost. It preserves source growth and ordinary return machinery. The two
small public v8 packages reach real victory/growth and defeat from ordinary field interaction;
their bounded admission and exclusions are in [trust](../content/profiles-and-trust.md#authored-growth-and-ordinary-outcomes).
The [native recipe](../verification/battles.md#replaceable-progression-and-outcome-observation)
uses the same packages under both factories and observes real return movement. This is authored
replaceability evidence, not new original-game or private-route evidence. `ForGame()=>Sf2()` remains
the default. The [source-story compatibility seam](../application/session-and-programs.md#source-story-policy)
has its Application owner.

### Selected module and composition design

**Accepted design:** keep the four assemblies. A gameplay module is an
ordinary set of C# source implementations under `Domain/Gameplay/Sf2` and
`Application/Gameplay/Sf2`, selected by explicit construction. New authored variants live beside
that module under `Gameplay/Authored`; they are project-authored examples, not original-fidelity
profiles. No separately distributed binary or third-party plugin API is required.

The existing assembly friendship already permits Application/Content/tests to use Domain internals.
Use narrow internal strategy interfaces under `Domain/Battles/Rules` and
`Application/Runtime/Exploration`, instantiated by the gameplay module. Engine executors call those
interfaces; only composition/default factories reference concrete SF2 implementations. Domain never
references Application, Content, Godot or reference verification. The Application module can combine
Domain rules and Application story policy using the existing inward dependency. Content decodes
validated definitions, not executable C# names or factories.

A single immutable `SessionRules` composition in Application holds the selected Domain rule bundle
and story policy. `GameSession.Start` accepts it and retains it for the session; default overloads
delegate to one explicit SF2 factory. That default is a composition decision, never a branch
inside arithmetic, dispatch, query, scheduler or view. Existing direct-start callers keep working;
Godot's root and standalone battle-view source starts use the same composition. New project-authored implementation files
and their composition factory can be compiled in the existing projects. A caller selects a factory
before start, not a package-name switch during play. There is no mutable global current profile.

Retain a diagnostic identity for the selected composition independently of the content's format/trust
profile. The authored demonstrations select a C# factory in ordinary startup composition and
rebuild; they need no new package schema or runtime selector. Authored variants must not be reported
as SF2 fidelity. A private source entry keeps its admitted default rule/resource contract; it cannot
silently select an authored fallback. Immutable action/AI content keys resolve once against the
selected bundle, with missing/duplicate bindings rejected at start, not discovered through reflection.

Do not create empty policy interfaces for future families. A new seam needs an actual consumer;
retire the replaced implementation with its final caller. Ordinary
private helper functions stay private. The source modules are independently editable/selectable;
they are not assembly-enforced isolation or a security sandbox. A fifth gameplay assembly would
force currently internal state/transition/program types into cross-assembly contracts without a
current binary-distribution need. If that need becomes real, replan and amend ADR0011's production
assembly section, ADR0017's preserved outer architecture, and ADR0019's four-assembly direction
before changing references. This selected source-module design needs no reversal of those ADRs.

```text
Godot composition -> Content admission -> definition + explicit start
                 -> selected SessionRules -> one GameSession
GameSession executors -> narrow rule contracts -> chosen C# implementations
chosen implementations -> existing numerical/state primitives
GameSession -> ordered state/scene projection -> Godot -> completion token
```

### Implementable API and transaction boundary

HEAL, finite actions, AI, progression/outcome and the linked source-story seam are implemented. Reuse the existing state, reference,
command, resolution, wait and failure types; do not add copies whose only purpose is forwarding.

| Seam | Inputs/result and owner | Required behavior |
| --- | --- | --- |
| HEAL rule | Internal `IHealingRule`: spell admission, target query/check and preparation over `EngineBattleState`, `ActorRef`, provisional `MapPosition`, `SpellRef`, optional target; preparation returns the existing `BattleActionResolution`. | The same selected implementation owns query and confirm legality/formula. No RNG in queries. The default implementation is `Sf2HealingRule`; shared scalar/target helpers have one owner. |
| Action selection (implemented) | `BattleSelection.ActionReference` and `SelectBattleAction` carry a typed action reference and immutable option/target data. `GameSession.QueryBattleChoices` returns choices for its current session/revision/actor/stage/origin, selection step, enabled status and `SessionFailure` reason. | Options carry stable action/spell/item identities, display labels, supported presentation kind and ordered candidate `ActorRef`s/range. Preserve `SelectSpell`/`SelectItem` as semantic inputs while callers migrate. Query is disposable; `CommandEnvelope` and confirm remain authoritative. |
| Action preparation/application | Selected action implementation returns existing prepared/reaction/reward structure; engine preparation validates it before `Begin`. Existing scene continuation and publication apply it. | Validate actor/target references, destination, supported reaction/consumer, HP/MP bounds and permitted field changes. Reject unrelated actor/queue/definition/story changes and malformed effects before publication. Do not execute string observations or accept arbitrary callbacks that mutate the session. |
| AI (implemented) | Internal `IBattleDecisionRule` consumes the immutable battle/actor plus selected action rules; returns the existing `BattleAutomaticAction` shape with candidate state, path and target. | Stable tie order and separate RNG streams; no hidden strategy state. Memory/last target/activation stay in actor/region state. Scheduler owns turn/wait/publication, not strategy selection or target scoring. |
| Progress/outcome (implemented) | Selected progression functions use current actor, award, copied seed and existing effects; selected outcome policy uses current battle/story plus admitted outcome content. | Keep credit, later growth, outcome check and story return as separate calls at their existing commit seams. Policies choose rules/results; generic runner executes the continuation. |
| Source story operation (implemented) | Internal `ISourceStoryPolicy` takes the current definition/snapshot, `SourceStoryInstruction` and `ProgramLocation`, returning only a finite entity-retirement candidate or a `BattleRuleException`. | Only the source-specific instruction family delegates. Generic branch/call/return/wait does not ask a rule-name registry. Missing support fails at the exact PC; policy cannot advance/publish the facade or supply arbitrary active/story state. |

An enabled query option means eligible for preparation under the known prerequisites, not a promise
that every reached random-dependent branch is supported. Query and confirm share those prerequisite
checks; confirm additionally constructs and validates the action. For example, a reached growth branch
may still reject during preparation without publishing its speculative draws. Keep that diagnostic
distinct from a stale or illegal selection. Queries must not run the whole scene, consume RNG, or
cache a prepared result whose seed could later be mistaken for live state.

Action preparation uses the existing typed result, not a universal effect algebra. Its result needs
structural validation because an injected implementation can return an impossible transition even
when trusted project code is not malicious. Validate allowed deltas at this real authority boundary;
do not copy whole state into a new editable request object or add a separate validation service.
For HEAL, prepared deltas are selected-actor movement and construction main seed only; reactions may
change the selected living target's bounded HP, spell cost the selected actor's MP, and reward the
selected actor's progress at the declared later phase. Preserve other actors, thinking seed, queue,
accounting, definition identity and selection ownership. The action slice extends this finite validation to
physical/item construction's existing inventory/gold/death obligations, not arbitrary property writes.

At every call, rules receive immutable definitions/state and local RNG values. Use `BattleRandom`
for deterministic draws; only the rule determines which draw is required and its stable iteration
order. Preserve existing ushort/byte widths, signed comparisons, truncation, saturation and explicit
unchecked source arithmetic. No wall clock, host RNG, unordered collection iteration or persistent
rule-object state may influence results. The local preflight result is discarded. Later growth/AI
must consume the then-current seed at the owning boundary; storing a preview's future seed is wrong.

Failures before preparation publishes preserve state, provisional selection, RNG and observations.
Stale session/revision/actor input rejects before rule invocation. Selection changes may increment
revision but never spend resources or random draws. At a later scene/program boundary, failure keeps
earlier accepted commits and the live wait/cursor/queue; it does not rewind already displayed effects
or rerun a spent award. Map expected rule errors to the existing IllegalCommand/UnsupportedCapability
categories; an invalid implementation result or unexpected rule exception becomes InvariantFailure
with rule identity, operation and existing field/PC context. Catch only around the selected rule call,
not the entire host, and preserve the last committed snapshot. Do not convert failure into Stay or
successful completion. Direct adapter errors remain AdapterError.

Content admission owns schema, references, provenance and known unsupported definition shapes.
Rule admission owns learned action, faction/target/range/resources and accepted class policy.
Application joins that result with implemented reaction/scene capability before starting an effect.
For example, HEAL4 cannot become supported by deleting `healing-animation`; the query must expose
Unsupported and confirm must enforce the same boundary. Private missing cast resources never use
the authored gesture fallback. Actual delivery failure holds the pending token and exposes an adapter
failure. Godot consumes options/targets/semantic inventory and scene requests; it may format labels,
cycle options, and select an existing presenter by effect kind, but cannot decode source inventory
bits, choose factions/ranges, calculate effects, or branch on strategy/module names. That is the #438
interface handoff; it specifies no layout, artwork or new input-device requirement.

### Rule author workflow and diagnostics

Start from the implemented contract and an existing example in the four production assemblies:

| Author's change | Contract and example | Selection |
| --- | --- | --- |
| HEAL eligibility, targets, recovery or award preparation | [`IHealingRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IHealingRule.cs), `Gameplay/Authored/AuthoredHealingRules.cs` | Pass the implementation as the healing argument in `SessionRules`; the finite action adapter uses that same object. |
| Physical, item or Stay offers/targets/preparation | [`IBattleActionRule`/`IPhysicalActionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs), `Gameplay/Sf2/Sf2PhysicalAction.cs`, `Sf2ItemAction.cs`, `Sf2StayAction.cs` | Bind the corresponding constructor argument. Keep query, confirm and physical estimate/preflight on the selected implementation. |
| Automatic priority/decision | [`IBattleDecisionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs), `Gameplay/Authored/AuthoredBattleDecisionRules.cs` | Bind the content's `BattleStrategyRef` once in the explicit factory's decision list; unknown/duplicate bindings reject at start. |
| Credit, counters, gold or later growth | [`IBattleProgressionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleProgressionRule.cs), `Gameplay/Authored/AuthoredBattleProgressionRule.cs` | Pass `progression:`; preserve the separate credit and live post-scene growth calls. |
| Victory/defeat/hook and recovery/return choice | [`IBattleOutcomeRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleOutcomeRule.cs), [`IOutcomeReturnPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs), `Gameplay/Authored/AuthoredOutcomeReturnPolicy.cs` | Pass `outcome:` and/or `outcomeReturn:`; common programs, scene/queue and staged publication remain the consumers. |
| Story branch, call, flag, text and wait | [External typed-program authoring](../application/session-and-programs.md#authoring-conditional-programs) | Change admitted JSON and restart. Both demo files run through `AuthoredStory()` without changing runner or view. |
| Source-only retired scratch compatibility | [`ISourceStoryPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/ISourceStoryPolicy.cs), `Gameplay/Sf2/Sf2StoryPolicy.cs` | `sourceStory:` selects the named policy. `AuthoredStoryPolicy` rejects this family; it is not a custom-opcode extension language. |

Implement a C# rule under the existing `Domain/Gameplay/Authored` or `Application/Gameplay/Authored`
module as appropriate. Reuse numerical/state primitives and an admitted reaction/presenter family.
The rule receives immutable state and returns only the contract's finite candidate. It must not keep
future state/RNG in object fields or mutate the session. Ordinary integer widths, deterministic
ordering and the separate seed channels still matter. Unsupported effects require a separately
scoped capability change; deleting a capability guard or returning a source-name string cannot add one.

Add an explicit factory in [`RuleCompositions`](../../src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs).
For example, a new HEAL implementation can use
`new("my-game", new MyHealingRule(), SourcePhysical(), SourceItem(), SourceStay(), sourceStory: new Authored.AuthoredStoryPolicy())`.
Other optional constructor arguments select the seams above. Their defaults remain SF2; choose each
intended variant explicitly. A project author changes `ForGame()` to that factory for the ordinary
host, or passes it to `GameSession.Start(source, rules)` in a behavior test. The same composition is
retained throughout the session. Content's `ruleProfile`, package IDs and strategy keys do not discover
or instantiate C# classes. There is no runtime environment switch or mutable policy setter.

Write small engine assertions for changed valid actors/states/content, not just the demonstration
endpoint: query/confirmation agreement, allowed candidates, exact RNG/commit boundaries, malformed or
throwing selected rules, and stale/duplicate completion. Run the affected engine tests and adapter
build with the [protected locked workflow](../development-and-verification.md#locked-net-workflow).
Build the existing Godot project in Debug, then restart the ordinary host with its admitted input.
Rebuilding does not replace a running session's rules; JSON is read/admitted once as well, so content
edits also need a fresh start. Use the existing short observer for the affected presenter/flow.
Restore the intended default factory and rebuild before freezing a source-default candidate.
No new assembly, language, installation, view setter or probe case is needed to choose a factory.

Diagnose at the boundary that owns the failure:

| Result | Meaning and next inspection |
| --- | --- |
| ContentError before start | Inspect package shape, all references, provenance/profile and start input. A rule cannot repair rejected Content. |
| IllegalCommand | Inspect the current envelope session/revision/actor, selected action/destination/target and enabled/reason query. A previous query is not authority to confirm. |
| UnsupportedCapability | Inspect the precise rule or consumer capability: source accounting, admitted spell/item/status, source story context or missing private scene binding. Keep the guard and unmet boundary explicit. |
| InvariantFailure with rule identity/operation | Inspect the selected implementation and finite result at the named commit seam; source-story diagnostics also name the exact program/PC. Earlier committed effects, live token and caller context remain. Exception details are not published. |
| AdapterError | Inspect actual projection/resource/delivery and adapter errors. A pending completion token remains pending; do not report successful delivery to unblock it. |

## Battle State and Numerical Rules

The engine implements internal Domain RNG, ordinary priest healing arithmetic, turn-order generation
and Manhattan action range, with a dedicated engine unit project and scoped verification entries.
`Gameplay/Sf2/PhysicalStrikeRules`, `BattleRewards` and `Sf2PhysicalAction` own ordinary physical construction,
settlement and atomic state transition. `Sf2PhysicalAction` constructs at most three source-ordered
hits on temporary HP, carries sticky reaction decisions and aggregates one ally award before publication.
Both player commands and `EnemyPhysicalDecision` use the same session-selected physical calculator; `BattleActionCommitter` is
the single publication/queue-consumption mechanism. Automatic advancement catches failures at each
enemy ACTION, preserving earlier commits and retaining the failed enemy's queue entry. The semantic
attack-then-approach strategy scores all reachable physical targets in reverse processing order and uses
shared `PhysicalTargetRules` for signed raw-priority cohorts, class selection and movement ties.
The existing class definition supplies source identity only for admitted named classes; missing
identity rejects a reached critical comparison. Regular movement fixes the class table; content
cannot supply an independent rank. Source thinking, scoring and selection have one production owner.
`AttackThenApproachAi` sequences that decision and its zero-target continuation: unavailable
HEAL1/SUPPORT fail, then MOVE1 succeeds with movement or origin Stay. `Gameplay/Sf2/AiMovementRules` owns source stable cost and radius station selection;
shared `BattleMovement.Walk`/`MoveString` retain the decreasing-cost route primitives used by movement
and source pursuit/standby. The existing movement commit and action publisher apply
the chosen destination once. MOVE1 draws no RNG and leaves last-target memory and resources unchanged;
physical rewards are required only on the reached attack branch. High or incomplete target costs
remain Unsupported, as does wider AI. Private region activation retains the bounded [private-entry contract](../content/profiles-and-trust.md#private-initialized-common-battle).
Shared strike/reward functions remain the calculation owners. Semantic observations carry both actor and target for reversal. Dead combatants retain
identity/HP/kill-and-defeat accounting but have no
battlefield position; occupancy and presentation read that authoritative state.
The [current M1 boundary](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
adds movement/cancellation, common session/content admission and connected authored battles.
Profile-specific snapshots, fixed import checks, endpoint handlers and old Godot battle dispatch
were retired with the reference runtime at
[M5](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation);
The [historical audits](../evidence/retained-comparisons.md#historical-implementation-and-audits) retain their exact old claims. Current responsibility and capability owners govern further work. Public/private trust and the accepted composed 8D/H4
milestone remain distinct from the unsupported broader capabilities in this migration.

Encounter deployments own explicit `BattleFaction` and unique integer `ProcessingOrder`; intrinsic
actor definitions and session starts do not duplicate those roles. The definition sorts deployments
once for stable round RNG, AI candidates and adapter selection. Runtime actors derive faction/order
from their deployment; queue entries identify actors by `ActorRef` with a nullable sentinel. Faction drives
healing/opposition/rewards independently of order. Deployments also own `BattleControl` separately from
`BattleStrategyRef`; intrinsic actor definitions carry neither assignment. Player requires no strategy;
automatic actors select an immutable content key resolved in `SessionRules` before start. Private source orders remain
SourceOrders and use the bounded source standby/activation/set6/set7 continuation. Shared deployment validation at Content
admission and reusable start preserves ally/player and enemy/automatic bounds plus physical/spellbook/
MOV requirements. All encounters, including unreached/dead deployments, bind against the selected composition. The
advancer invokes the resolved decision without policy-name switches or a missing-binding Stay fallback. The native view projects each actor’s current AI memory and immutable source anchor; there is no second control copy.
Actor definitions separately own numerical `Agility` (0–127) and boolean `ExtraRoundAction`.
Live-start capacity and shared generation consume explicit eligibility; only private Content decodes
the raw source byte's low seven bits and high-bit eligibility. The ordinary three draws and optional two draws at the
truncated five-sixths basis retain source arithmetic and signed sentinel ordering. The existing
advancer consumes both entries without an additional scheduler or physical double/counter changes.

`PhysicalCriticalRule` owns the two supported immutable chance/bonus definitions; Content selects
one from explicit semantic fields rather than a packed prowess number. Every actual hit, including
reversed counters, reads its attacker's selected rule. The scalar strike calculator already takes
mathematical operands and remains unchanged, as do its private source mappings. Other
physical fields retain their existing semantic ownership; no generalized profile system is introduced.

`BattleTerrain` holds immutable surface and independent protection; layout glyphs resolve through
explicit local definitions. `BattleTerrainRules` interprets them for the currently admitted
regular/healer/Centaur/hovering movers and target land reduction. Content/start, preview/commit, AI pursuit/scoring
and physical hits use that owner. Temporary opponent blocking changes only a fresh per-cell cost
array; friendly traversal and occupied stopping remain movement policy. Godot projects surface into
existing placeholder colors without supplying gameplay properties.

`WeightedMovement` consumes decoded signed cell costs, preserving its one first-admission/LIFO bucket
algorithm with logical row boundaries; the original flat-storage probe across a row seam is not
reproduced. Private Content maps the selected raw terrain to these semantic surfaces while retaining
the complete source encounter for later consumers.

## Ordinary Medical Herb battle action

The common battle session accepts `SelectItem(slot)`, `SelectTarget(actor)` and `Confirm`
from ordinary action choice. `Cancel` returns to provisional movement. Item reselection
clears the previous target. Invalid slots, empty inventory, unsupported item effects,
dead/opposing/out-of-range targets and failed reward/growth admission publish no partial
movement, HP, inventory or RNG change. Reusing an old command envelope cannot consume twice.
The shared target validator measures from the provisional destination, including self-targets.

`BattleActorState.SourceLoadout` is the live ordered four-slot loadout. Definition loadouts
supply initial content only; explicit `BattleActorStartInput.SourceLoadout` carries later
state. Growth retains that live inventory while changing learned spells, and `BattleOutcome`
passes it into the exploration party on both victory and defeat. Resource resets heal HP/MP
without replenishing items. `item-consumed` records the original item word in `Before` and
the chosen zero-based slot in `After`; the resulting snapshot exposes the arranged inventory.
HP recovery now executes through the [Medical Herb scene](../godot/battle-scenes.md#medical-herb-scenes),
after construction has consumed inventory. EXP/growth and turn release wait for their scene commands.
Application owns single-side target/actor switching; SFX113 and the source NONE/Nothing selection do
not imply fairy or physical recoil draws. There is no separate inventory service or field inventory UI.

### Original source and effect boundary

**Confirmed static:** the pinned SF2DISASM revision
`c834c652b6862bc5679fd7f69a38a7093206efc6` supplies these dependencies beneath `disasm/`:

| Source / symbol | Consumed rule |
| --- | --- |
| `data/stats/items/itemdefs.asm`, item 0 at `table_ItemDefinitions` (`0x16EA6`) | Medical Herb is CONSUMABLE, with no equip effects; use spell HEALIN-1 and item range 0–1. |
| `data/stats/spells/spelldefs.asm`, HEALIN-1 | Base ID 16, MP cost 0, teammate healing, range 0–1, radius 0, power 10. |
| `code/gameflow/battle/battleactions/useitem.asm`, `battlesceneScript_UseItem` (`0xBBB8`) | Select the held item definition, unpack use spell and delegate to its spell effect. |
| `castspell.asm`, `spellEffect_Heal`; `calculatespelldamage.asm`, `AdjustSpellPower` (same battleactions directory) | Recovery is `min(power, missing HP)`. Item actions skip the spell-only promotion multiplier. HEALIN recovery has no variance draw. |
| `earnexp.asm`, `battlesceneScript_CalculateHealingExp`; `giveexpandgold.asm`, `battlesceneScript_GiveExpAndGold` | Ally healer classes contribute `min(25, max(10, floor(25 * recovered / maxHp)))`; other classes contribute zero. Same-side actions skip battle halving. Two range-16 draws add/subtract one on zero, with final minimum one. |
| `breakuseditem.asm`, `battlesceneScript_BreakUsedItem` (`0xBBE6`); `code/common/stats/itemstats.asm`, `RemoveItemBySlot` / `RemoveAndArrangeItems` | Non-equipment consumes without a break roll, shifts subsequent full item words down one slot and appends NOTHING (127). Identity lookup masks the low seven bits; retained words preserve flags. |

The [item-definition](../../../docs/design/contracts/party/item-definition-data.md),
[spell-definition](../../../docs/design/contracts/party/spell-definition-data.md),
[spell-resolution](../../../docs/design/contracts/battle/spell-resolution.md) and
[action-construction](../../../docs/design/contracts/battle/battle-action-construction.md) contracts
own the accepted boundaries. **Confirmed native:** the
[original herb observations](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md#native-herb-observation-and-completed-branch-recovery)
and winning continuation record the reached item/slot/HEALIN/consumption family. Their bounded
HP facts include 6→11, 9→12, 3→12 and 4→11; these are missing-HP clamps, not four different
item powers. `MedicalHerbTests` checks those minimal facts separately from controlled RNG cases.
It does not relabel authored tests as an original trace replay.

Private Content admits Medical Herb and the separately bounded [Seed/Antidote extension](#finite-purchased-supplies-and-detox1) into this item family.
Other carried item IDs remain visible but reject as `item-effect`; equipped weapons are
not herbs. The current class model admits PRST healing EXP and the existing non-healer
classes. VICR/MMNK class admission, broader item families, equipment beyond the authored weapon subset, Give/Drop beyond the standalone subsets, field inventory,
AI healing-item choice and save/load remain unsupported. Exact original presentation timing remains Unknown. The existing
physical-only AI policy rejects a nonempty item loadout at start. This does not close the
continuous H4 milestone or establish successful native-original cancellation.

## Authored battle Give and exchange

**High rigor agreement (#1363):** two live loadouts, current equipment contributions, movement
and resumable action completion share existing authorities. Implement standalone opted-in Give
with donor-slot, adjacent-recipient and full-pack return-slot selection. Confirm publishes the
finite mutation; authored feedback resumes into ordinary STAY completion exactly once. Cancel
before confirmation is free. Reuse SourceLoadout, BattleEquipment and GameSession, without a
second inventory authority. Acceptance is changed-state rule/session tests, actual adapter build,
one ordinary-input transfer/exchange witness with later equip/use/control, and exact-head CI.

**Confirmed (static):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm`
`@CheckItemMenuChoice_Give` through `@EndGiveAction`: ordinary battle Give removes the donor
before adding to the recipient. Exchange saves the selected words, removes donor then recipient,
adds the recipient's saved item to the donor, then the donor's saved item to the recipient.
`disasm/code/common/stats/itemstats.asm` `RemoveItemBySlot/RemoveAndArrangeItems` compacts
following slots and calls `UpdateCombatantStats`; `AddItem` fills the first empty identity slot
and masks to identity/broken bits, so receipt never auto-equips. This is the battle caller's
order; the field caller differs.

`disasm/code/gameflow/battle/battlefield/buildactionrangegrids.asm`
`BuildSpellRangeGridForItemGive` uses the teammate grid and `SpellRange1`;
`disasm/data/stats/spells/spellranges.asm` defines offsets (0,1), (1,0), (0,-1), (-1,0),
excluding self/diagonals. `buildtargetsarray.asm` filters zero HP, neutral activation bit and
unplaced coordinates. The completed Give writes STAY and exits player control. Modern selection
retains provisional movement until commit; modern result text is authored, not source dialogue
or original timing evidence.

Scope excludes curses, rings, broken items, general status effects, field/service inventory,
private imports, save/load and changed frozen profiles. Carryability is separate from use or class
equip eligibility. **Unknown:** natural original reach, original presentation/timing and wider
status behavior. #1047's twelve Unknowns remain unchanged; #638 remains paused.
