# Hostile battle spells through Battle26

[Issue #1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967) owns the design for
player MUDDLE 1/2, DESOUL 1/2 and SLEEP 1. Rigor is **Standard**. The learned Magic command crosses
target admission, RNG, scene playback, turn ownership or death, and battle Save/Continue. Accept each
implementation slice through its real consumers before enabling the ranks it owns. The campaign
boundary is Battle26 and first Pacalon. Enemy acquisition of these casting decisions, future ranks,
Field and Caravan Magic, original audiovisual timing and later campaign travel are outside this design.
[Campaign agreement](../application/remaining-campaign-risk-baseline.md) ·
[Engine direction](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md)

## Rank selection and commit

**Confirmed.** These are the fixed USA source rows. `range` is Manhattan distance from the chosen
casting square to a living, positioned opposing center. Radius is a second Manhattan diamond around
that center. Center range does not constrain other members of the diamond. The common spell target
builder excludes dead, neutral and unplaced actors, takes the center's side, and sorts the selected
targets by source combatant index for the ordinary action. It does not test resistance for eligibility.
All five entries have POWER 0, clear TARGET_TEAMMATES and set AFFECTEDBYSILENCE. The dispatch indexes
by family. Only MUDDLE's effect branches on rank. [Spell geometry and record](../../../docs/design/contracts/party/spell-definition-data.md) ·
[Rank rows][defs] · [Target builder][targets] · [Cast dispatch][cast] ·
[Shared neutral targeting](detox-ranks-design.md#implementation-decision-and-sequence)

| Entry | Packed ID | MP | Center range | Radius | Type | Source animation |
| --- | ---: | ---: | --- | ---: | --- | --- |
| MUDDLE 1 | `0x07` | 6 | 1–2 | 1 | Support | DEBUFF1 |
| MUDDLE 2 | `0x47` | 11 | 1–2 | 2 | Support | DEBUFF1 |
| DESOUL 1 | `0x08` | 8 | 1–2 | 0 | Attack | DESOUL |
| DESOUL 2 | `0x48` | 13 | 1–2 | 1 | Attack | DESOUL and VARIATION2 |
| SLEEP 1 | `0x09` | 4 | 1–2 | 1 | Support | DEBUFF1 |

Use the current learned book, available MP and selected rank. Cancellation and stale or invalid
commit leave position, resources, RNG, queue and scene state at the appropriate existing input
boundary. After a valid confirmation, stage the MP reaction once with the caster's prior status.
Silence then ends the committed cast with its message and consumes the turn. It spends MP and
skips all target and award draws. Valid, unsilenced casts spend MP even if every target resists.
[Source action loop][loop] · [Cost reaction][cost] ·
[Common support cast sequence](support-spell-status-design.md#original-rules-and-evidence)

## Per-target effectiveness

**Confirmed.** Use the current resistance bits 15–14 for STATUS settings 0–3. On an ordinary cast,
the effectiveness helper takes one main RNG draw in `[0,8)` per target, including an immune or
guarded one. Success is `roll >= threshold`. Failure displays the ineffective result and unwinds
to the next target, with no target reaction, refresh or contribution. The ordinary engine need
not expose the source debug controls that can force threshold 8. [Element table][elements] ·
[Resistance lookup][resistance] · [Effect handlers and effectiveness helper][cast]

| Effect | Threshold | Successful reaction |
| --- | --- | --- |
| MUDDLE 1 | 5 for every STATUS setting, 8 if the target already has bit `0x0008` | OR `0x0030` into the current word |
| MUDDLE 2 | 5 + setting | OR `0x0038` into the current word |
| SLEEP 1 | 5 + setting | OR `0x00C0` into the current word |
| DESOUL 1/2 | 5 + setting | Instant death, with the prior status in the reaction |

Both MUDDLE ranks and SLEEP award 5 raw EXP per successful target for an ally caster, even if the
successful OR leaves the same word. A new MUDDLE 2 upgrades MUDDLE 1. A successful MUDDLE 2 or SLEEP
recast refills the counter. A success copies unrelated bits and stages the new word for playback and
the existing reaction refresh. A miss leaves both the word and the last-applied stat operands intact.
These handlers have no BOOST/SLOW construction-write quirk. The thresholds, fresh applications,
MUDDLE 1 reapplication, guard and ordered effects have controlled observations on BizHawk 2.11.1
with Genesis Plus GX. A MUDDLE 2 or SLEEP recast is **Confirmed** from the same unconditional OR
after a successful roll. Its natural retail action sequence is **Unknown**. [MUDDLE 1 fixture](../../../tests/fixtures/h3/spell-muddle1-v1.json) ·
[MUDDLE 2 fixture](../../../tests/fixtures/h3/spell-muddle-v1.json) ·
[SLEEP fixture](../../../tests/fixtures/h3/spell-status-sleep-v1.json) ·
[DESOUL fixture](../../../tests/fixtures/h3/spell-desoul-v1.json)

The shared scene award saturates at 49 before the final adjustment. A surviving ally acting through
ordinary hostile Magic on a listed battle uses the half-EXP table, then two range-16 draws
(zero adds 1, then zero subtracts 1) and minimum 1. This includes an all-miss cast. The USA halving
table selects Battle01, so ordinary Battle26 casts skip the halving. Use the shared award owner
for target side and allied DETOX/item status effects. Physical confusion uses its own source
same-side flag below. Enemy spell casts have no player EXP or gold award. [Scene end][end] ·
[Award and contribution][award] · [Kill/status EXP][exp] · [Halved battles][halved] ·
[Shared status award decision](detox-ranks-design.md#implementation-decision-and-sequence)

## Sleep, Muddle and turn ownership

**Confirmed.** The critical-sleep physical path derives its ailment from prowess and the physical
critical/attack sequence. It writes the same `0x00C0` bits as the spell. The spell has its own
STATUS roll, MP debit and successful status EXP. Once applied, both use the existing sleeping
action skip and after-turn wake behavior. Sleep skips action before MUDDLE control and prevents
normal dodge and counter. A hit does not itself wake the actor through these consumers. In a
living actor's after-turn, after STUN, use the current packed sleep field as the main RNG range.
If `roll & 0xC0` is zero, clear sleep and show 354. Otherwise show 306 and subtract `0x40`.
Use the actual packed count, including guaranteed wake at `0x40`. [Physical ailment][ailment] ·
[Turn owner][turn] · [Dodge][dodge] · [Counter][counter] · [After-turn][after]

**Confirmed.** Any nonzero MUDDLE counter makes a would-be player turn automatic after the
sleep/stun skip checks. MUDDLE 1 keeps normal opposing targets. Confusion for AI targeting needs
both a nonzero counter and bit `0x0008`. It flips the reachable attack/item/spell target side
without changing actor faction, formation or save control identity. The normal source enemy
activation and commandset still apply. An automatic ally uses commandset 06
(ATTACK1, HEAL1, SUPPORT, MOVE1, STAY). The regular confused branch skips Heal and Support and
the MOVE fallback changes its pursuit operand to the first source combatant slot on the actor's
own side. Ordinary MOVE still uses terrain, occupancy and the source movement decision. The
attack-spell getter limits allies and confused enemies to its six named elemental families. For
example, an ally's newly bound DESOUL never becomes eligible through that getter. Apply the
ordinary candidate priorities and item eligibility before the selected-target guard.
[Turn owner][turn] · [Commandset table][commandsets] ·
[Confusion predicate][confused] · [Reachability][reachable] · [Movement][move] ·
[AI filtering and command evidence](../../../docs/research/battle/battle-ai.md#spell-filtering)

**Confirmed.** The attack command guard runs after candidate selection. Ally attackers selecting
slot 0 and enemy attackers selecting slot 128 produce inaction without a guard draw. Otherwise
a selected self target consumes one main range-2 roll (1 gives inaction), and other targets
pass. This is the guard on the selected target, including an area center. It grants no blanket
immunity to area membership. A guarded action clears movement and produces source action 5.
Its empty target list still runs the scene message draw (range 16, values above 9 select the
first line) and the existing eligible ally scene-end award. The same-side/scene flags come from
actual combatants and the MUDDLE 2 flag. Use the normal no-candidate movement/Stay route when no
attack is selected. [Attack command][aiattack] · [Action guard][guard] ·
[Scene targets][loop] · [Action message][message] ·
[Controlled predicate and guard observations](../../../docs/research/battle/runtime-rng-and-battle-math.md#confirmed-muddle-confusion-predicate-truth-table)

Both MUDDLE ranks make a normal physical attack dodgeable on main range-2 roll zero, except
when the target's SLEEP or STUN bypasses the dodge. The scene's MUDDLE 2 flag gates ranged
physical presentation and prevents ordinary doubles/counters. The regular same-side gate also
suppresses doubles/counters. Preserve actual faction for kill, drop and victory accounting.
When the MUDDLE 2 scene flag bypasses initialization of `targetIsOnSameSide`, keep that source
award behavior for this automatic branch. Its guard, message and effect rolls use main RNG;
AI scoring retains the existing thinking stream. A natural retail sequence from cast through
selected same-side action, guard and control restoration is **Unknown**.
[Scene flag initialization][init] · [Physical dodge][dodge] · [Double gate][end] · [Counter gate][counter]

**Confirmed.** After a living actor's own consumed entry, tick MUDDLE between SLEEP and SILENCE.
With `count = status & 0x0030`, draw main RNG with range `count` only when nonzero. If
`roll & 0x0030` is zero, clear count and bit `0x0008` together with message 355. Otherwise
subtract `0x0010` and keep the flag. A one-counter status necessarily clears. Each extra entry
counts independently. A dead actor skips this pass, and terminal checks remain in the existing
order. The ordinary after-turn stat refresh still runs after the complete status/equipment/poison
sequence. Standby, inaction and skip paths must reach the same continuation.
[After-turn source][after] ·
[Controlled MUDDLE transitions](../../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json) ·
[Turn finalization](../../../docs/design/contracts/battle/battle-control-lifecycle.md)

## DESOUL death, rewards and return

**Confirmed.** On success the signed HP operand is `0x8000` (−32768); normal admitted HP
clamps to zero. Treat this as a spell-specific death outcome. The effect reads no damage
power, damage resistance multiplier, HP fraction, spell critical, damage variance or damage EXP.
Both ranks share this handler. Resistance setting 3 is immunity. There is no boss flag or leader
slot test in the handler. Taros and Zeon source definitions have setting 3, whereas Willard
has setting 2. Battle26's initial slot 128 is a Lizardman with STATUS setting 0 before any
repeat upgrade. Confirm the actual live definition in a repeat. Source eligibility follows that
definition. Natural retail DESOUL victory on a boss remains **Unknown**. [Effect and threshold][cast] ·
[Enemy definitions][enemies] ·
[Battle26 repeat alternatives](../../../docs/research/chapters/northern-travel-mitula/battle26-initialization-and-roster.md#completion-held-upgrade-alternative)

On each success emit the side-specific death result, award kill EXP once (50/40/30/20/10/0
from relative level, including the caster's promotion offset), and accumulate enemy gold once
for an ally caster killing an enemy. An ally caster killing an ally would take kill EXP without
enemy gold through the general source handler. That is outside ordinary player targeting.
The per-target death marker is reset before the next target, so a miss after a kill cannot
inherit rewards or custody. Run the shared action-side drop helper after each qualifying
kill, before moving to the next target or awarding the aggregate. Preserve conditional drop
RNG, source pack removal, one-time flag, living caster capacity and rare-to-Deals fallback.
For Battle26's source row 2 (combatant 130, Dark Bishop before upgrades), a successful qualifying
kill with the ring held and flag10 clear takes the Black Ring route without drop RNG.
The controlled DESOUL 1 multi-target fixture uses a supplied list, so its ordered reactions
support the death/award contract while the DESOUL 2 legal diamond comes from the definitions.
A natural retail DESOUL rare-drop transaction is **Unknown**. [Effect][cast] · [Target loop][loop] ·
[Drop helper][drop] · [Kill EXP][exp] ·
[Black Ring custody](../../../docs/research/chapters/northern-travel-mitula/battle26-outcomes-and-higins-recruitment.md#the-independent-black-ring-drop)

Replay the zero-HP reaction once through the existing death batch. The battlefield cleanup
does kill/defeat accounting, removes position, clears status and refreshes the dead actor.
The qualifying ally scene gives enemy kill credit through the existing first-ally seam. Battle26
leader cleanup can append other living enemies, so action casualties and forced cleanup
casualties have distinct provenance. Forced rows keep the existing kill bookkeeping but
receive no second spell EXP, gold or action-side item drop. Complete the whole committed
multi-target scene before terminal selection. Bowie's death uses the existing defeat priority;
a normal leader death or enemy count of zero uses the encounter's existing victory path.
The first victory can hand off Higins; a completed repeat follows its established return.
Use the same outcome and field return for damage kills and spell kills. [Battle26 cleanup and return](../../../docs/research/chapters/northern-travel-mitula/battle26-outcomes-and-higins-recruitment.md) ·
[Source death accounting][deaths] · [Remake Battle26 return](../application/battle26-higins.md)

## Implementation order and ownership

**Confirmed** in the current remake. Kazin's learned MUDDLE and DESOUL ranks can be retained
without an action binding. The existing support bindings include DISPEL, BOOST and SLOW, and the
attack bindings use elemental damage rules. The physical sleep lifecycle already exists through
battle input and after-turn. None of these grants SLEEP 1 to a learned Magic command.
[Current support binding](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs) ·
[Current attack binding](../../src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs) ·
[Existing sleep behavior](../../tests/Sf2.Remake.Engine.Tests/SleepAndFlyingTests.cs)

Use the finite effects on the existing learned-spell entries. Putting all three families on
the damage path would conflate status EXP and instant death with modified damage. A parallel
hostile action framework would duplicate target admission, cost, ordered reactions, award and
save guards. Extend support for the two status families and attack for DESOUL, sharing the
existing rules and source-specific result operands. Keep class/learned-rank data in Content
and selection in the session. A binding only becomes available when both its effect and
required consumers are admitted.

1. **SLEEP and shared status admission.** Add one finite support effect in
   [SupportSpellDefinition](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SupportSpellDefinition.cs),
   [Sf2SupportSpellAction](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs) and
   [PrivateSupportSpellBindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs).
   Reuse the shared Magic selection, silence and area handling. Make validation in
   [BattleActionResolution](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)
   distinguish success, a successful repeat and ineffective feedback. Route the existing sleep
   state through [SleepTurnRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SleepTurnRules.cs),
   action/physical eligibility and [BattleAfterTurnContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs).
   Compose the neutral-center and award changes with the DETOX owner. Verify current action
   readers accept the legal stored word even when targeting a sleeping actor.
2. **DESOUL 1 then 2.** Consume the accepted [#1944 attack and item split](offensive-items-design.md).
   Keep instant death on the learned spell path and preserve elemental attack-item resolution.
   Extend [AttackSpellDefinition](../../src/Sf2.Remake.Domain/Gameplay/Sf2/AttackSpellDefinition.cs),
   [Sf2AttackSpellAction](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) and
   [PrivateAttackSpellBindings](../../src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs).
   Use a finite instant-death effect and a validated death reaction in the current spell scene.
   Keep the instant-death outcome available to reward calculation, so it adds exactly kill
   EXP. Pass death through [Sf2BattleProgressionRule](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs),
   [EnemyItemDropRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemyItemDropRules.cs),
   [BattleDeathBatch](../../src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs) and
   [BattleOutcomeRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs).
   Keep effects, custody receipts and scene waits within the existing commit/replay authority.
   Admit rank 2 via the same effect after varied ordered outcomes and its legal area pass.
3. **MUDDLE 1 then 2.** Pair the first support binding with after-turn, effective automatic
   ownership and the physical dodge consumer. Pair the second with same-side AI decisions
   and guarded and real same-side actions. Route temporary automatic control in
   [BattleAdvancer](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs).
   Extend the existing commandset/action decisions
   ([physical](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemyPhysicalDecision.cs),
   [spell](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemySpellDecision.cs)) and automatic
   action validation only for bounded supported actor/action combinations. Reuse the selected
   move, validated commit and real scene for inaction. Preserve applicable enemy commandsets,
   ally commandset 06 and their respective inputs. Neither blanket Stay nor temporarily
   swapping faction can express movement, selected-target guards, RNG and rewards correctly.
   Confirm the variants needed by the currently reachable formations before enabling each rank.
   If a current legal combination needs a new shared AI or death contract, bring the concrete
   case to main-gate before binding the effect. Existing unconnected special enemy AI and
   arbitrary automatic spell/item combinations remain explicit boundaries.

Keep one packed status word. The existing last-refresh operand remains scoped to stat-affecting
counters. Reaction and after-turn refresh recompute stats using the existing support rules.
Extend status predicates where they currently reject the newly legal combinations, including
pending automatic work from a standby. Keep active MUDDLE/SLEEP, queue and cursor, both RNG
words, HP/MP and growth/books, first-ally/death accounting where applicable, and drop flags,
inventory and Deals through the [settled Movement save](../application/battle-save-and-continue.md).
No transient reaction, wait or automatic-choice state enters a save. When present, MUDDLE 2
needs a live count in an admitted save; old saves without the bits retain their existing
interpretation. Validate deployed and retained states through the existing
[BattleSave](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs) and
[BattleSaveWire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs).
Restore the current word and last-applied operands without replaying a cast, refresh or AI draw.
Processed deaths clear their word on cleanup. The existing return and fresh-battle recovery
clear sleep and confusion on living allies, preserve the existing lasting mask, and rederive
equipment effects. Original suspend copies combatant data and resumes the saved battle without
fresh healing. [Stat/save authority](support-spell-status-design.md#save-compatibility-and-recovery) ·
[Original save](../../../docs/design/contracts/system/save-system.md) · [Return recovery][heal]

Reuse the existing battle scene, spell name catalog, battle map projection and input waits.
Extend the [scene continuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)
and Godot [scene view](../../game/src/Battles/BattleSceneView.cs) for the actual effect, ineffective,
inaction and death messages where current projection needs them. Before binding, confirm the
selected resource package supplies the needed IDs and locate any missing effect art or text.
Existing generic presentation can carry the rules with an explicit presentation boundary.
After implementation, the responsible writer updates [characters](../content/campaign-characters.md),
[capabilities](../capability-status.md) and connected campaign owners with distinct learned,
bound, ordinarily cast and restored reach.

## Acceptance and bounded inputs

Use small engine behavior cases at the normal action and turn entry points.

- Show legal centers at distances 1 and 2 and ordered radius membership outside center range.
  Exercise dead, unplaced and neutral exclusion, stale targets, wrong side, insufficient MP,
  cancel, silence and all-miss. Check payment and pre-effect cost word, mixed target rolls,
  feedback, unchanged failure state, seed chaining and one ally EXP award. Vary source resistance
  and positions. Use a real lower-rank selection from a higher learned entry.
- For SLEEP, check successful same-counter recast and refresh from one counter, skip, dodge and
  counter gating, then wake/continue with STUN, MUDDLE or other independent status coexisting.
  Prior physical sleep uses the same wake route. For MUDDLE, check rank 1 on setting 3, rank 1
  blocked by the rank 2 flag, a rank 2 upgrade and recast, both after-turn branches and later
  regained control. Exercise both factions' physical dodge, real same-side action, protected
  center, other target and guard draws for a reachable self target or the guard's own legal input.
  Include no reachable attack, skipped turns, standby and a pending second queue entry.
- For DESOUL, vary HP, resistance, ally level/class, formation and kind of enemy. Check a
  susceptible leader and an immune ordinary foe to separate resistance from outcome policy.
  Use an ordered rank 2 kill/miss/kill. Check exact kill-only EXP before saturation (choose a
  raw kill award below 49), summed gold, first-ally kill credit, no stale death marker, live
  caster room/full-pack Deals and pre-set flag. Compare casualties and field return via the
  normal leader, ordinary zero-enemy and defeat paths. A forced cleanup victim with a rare
  item supplies the no-drop control.
- At the next legal player Movement, save and cold Continue with a pending sleep or MUDDLE
  count and with completed DESOUL rewards/death. Compare decoded battle data, the next decision
  and queue, seed and resulting action. Verify a usable field return and subsequent field
  Continue with temporary status gone and retained growth, pack and custody. Exercise a
  legacy clean slot. Run the affected engine cases and adapter gate when these slices land.

**Confirmed reach.** Kazin's MAGE growth learns MUDDLE 1 at 9, DESOUL 1 at 18, MUDDLE 2 at 25,
DESOUL 2 at 29. Keep both castle histories intact. English MAGE 27 is fallen; use a disposable
copy, ordinary church Raise and active formation before entering Battle26. Chinese MAGE 29 is
living and has both DESOUL ranks. Check actual deployment, learned book and resources in each
copy. Use the existing Pacalon road recipe for reentry and record actual resistance, held ring
and bit10 in the repeat. If a natural reward witness lacks a ring or successful roll, keep that
branch **Unknown** and use a controlled engine case for the custody requirement. The earlier
ordinary Battle26 drop and an unmodified pre-victory slot may supply a shorter legal ring branch
if the caster has the needed rank. A repeat can witness actual Magic, one successful status and
its later turn, or one DESOUL death. Other outcomes can use separate bounded branches.
[Kazin growth][kazin] · [Castle seeds and class audit](../application/ordinary-promotion-classes.md#spell-and-attack-boundaries) ·
[Pacalon reproductions](../application/pacalon-first-visit.md#reproduction) ·
[Battle26 ordinary reward](../application/battle26-higins.md)

**Confirmed reach.** The initial Tyrin choice was made through ordinary Creed input. He joins
as a level-24 MAGE; source growth puts SLEEP at effective level 25. Neither Pacalon history
contains Tyrin. If a registered legal Tyrin branch can deploy and earn the next level on a
short supported encounter, use that branch and confirm SLEEP through the learned-book path.
Otherwise exercise legal Tyrin-25 state in engine acceptance and leave ordinary Tyrin SLEEP
**Unknown**. Neither an edited castle slot nor physical sleep establishes that witness.
[Tyrin growth][tyrin] · [Creed branch and persistence](../application/creed-recruitment.md)

Measure the first slot and receipt before copying more. Budget up to eight slots at 128 KiB
each and up to 2 MiB of compact state/input/projection/error receipts, roughly 3 MiB before
build output. Open one owned debug instance at a time, reuse the accepted Pacalon package
(~102 MB) and registered Creed input in place, and budget no generated replacement worlds,
original captures, video, screenshots or frame traces. Observe the selected ranks, payments,
outcomes and errors from ordinary input at settled boundaries. If a rehearsed suffix requires
a campaign replay, a search for favorable seeds or a new resource export, report the missing
witness and its specific dependency before expanding the run. Any normal shutdown failure is
a completed process result separate from observed save and return. No runtime was acquired
for this design. [Existing package and observation budget](detox-ranks-design.md#acceptance-and-input-budget)

## Evidence reproduction

**Confirmed** source conclusions use the [USA ROM identity](../../../manifests/roms/sf2-us.json)
and fixed [SF2DISASM c834c652][pin]. Read sources with the tracked provenance and an existing
pinned checkout. For example, from that checkout run
`git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battleactions/castspell.asm`.
The [English spell contract](../../../docs/design/contracts/battle/spell-resolution.md) and
[research runtime owner](../../../docs/research/battle/runtime-rng-and-battle-math.md) give the
controlled fixture boundaries, addresses, tool version and reproductions for those observations.
Static branches and supplied fixture inputs leave natural sequencing and timing **Unknown** where
specified above.

[pin]: https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm
[defs]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm
[targets]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildtargetsarray.asm
[cast]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/castspell.asm
[loop]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm
[cost]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlesceneanimation.asm
[elements]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spellelements.asm
[resistance]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/getresistancetospell.asm
[end]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_2.asm
[award]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/giveexpandgold.asm
[exp]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/earnexp.asm
[halved]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/halvedexpearnedbattles.asm
[ailment]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/inflictailment.asm
[turn]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm
[dodge]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/determinedodge.asm
[counter]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/isabletocounterattack.asm
[after]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/processafterturneffects.asm
[commandsets]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aicommandsets.asm
[confused]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/iscombatantconfused.asm
[reachable]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/getreachabletargets.asm
[move]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/move.asm
[aiattack]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/attack.asm
[guard]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/attack/determinemuddledbattleaction.asm
[message]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlescenemessage.asm
[init]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/initbattlesceneproperties.asm
[enemies]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/enemies/enemydefs.asm
[drop]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/dropenemyitem.asm
[deaths]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/processkilledcombatants.asm
[heal]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/heallivingandimmortalallies.asm
[kazin]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm
[tyrin]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats17.asm
