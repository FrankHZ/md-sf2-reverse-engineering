# Hostile battle spells through Battle26

[Issue #1967](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967) owns the design for
player MUDDLE 1/2, DESOUL 1/2 and SLEEP 1. Rigor is **High** under the issue agreement.
MUDDLE changes AI control and targeting. DESOUL connects death, rewards and item custody to
battle outcomes and return. Follow the [three implementation stages](#implementation-order-and-ownership)
and their [acceptance criteria](#acceptance-and-bounded-inputs). Enable each rank only after its
effect and required downstream consumers pass. The campaign boundary is Battle26 and first Pacalon.
Enemy acquisition of these casting decisions, future ranks,
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
for target side and allied DETOX/item status effects. Confused offensive scenes use the source
flag described below. Enemy spell casts have no player EXP or gold award. [Scene end][end] ·
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
[Controlled guard observations](../../../docs/research/battle/runtime-rng-and-battle-math.md#confirmed-muddle-ally-and-enemy-action-guards)

An attacker affected by either MUDDLE rank gives the defender a dodge on main range-2 roll
zero. A sleeping or stunned defender skips the dodge draw. The scene's MUDDLE 2 flag gates ranged
physical presentation. The source double and counter validations suppress follow-up strikes
for the confusion or same-side flag after the qualifying first attack consumes its ordinary
double and counter main RNG draws. Preserve actual faction for kill, drop and victory accounting.
When the MUDDLE 2 scene flag bypasses initialization of `targetIsOnSameSide`, the field stays
zero. This leaves the half-EXP battle check enabled for an eligible ally scene award. The
guard, message and effect rolls use main RNG. AI scoring retains the thinking stream. A natural retail sequence from cast through
selected same-side action, guard and control restoration is **Unknown**.
[Scene flag initialization][init] · [Physical dodge][dodge] · [Double and counter rolls][doublerolls] ·
[Double gate][end] · [Counter gate][counter]

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

## SLEEP 1 implementation and evidence

**Confirmed** in the remake engine. A learned SLEEP 1 uses Support Magic with the existing area
selector, reaction refresh, silence, EXP and sleep turn flow. An authored level-25 mage reaches
the action through normal engine commands. The case observes cancel, a paid cast, a successful
reaction and the enemy's later skipped entry and wake. Controlled targets cover the four STATUS
settings, a successful reapplication at the same count and a partial-count refill. The existing
sleep-consuming physical tests cover dodge, counter and ordinary hit behavior. A separate
settled battle wire round trip restores an active sleep count and matches the next queue and
RNG results. [Learned spell and save cases](../../tests/Sf2.Remake.Engine.Tests/SleepSpellTests.cs) ·
[Existing sleep behavior](../../tests/Sf2.Remake.Engine.Tests/SleepAndFlyingTests.cs)

**Confirmed** in the private campaign reader. SLEEP 1 binds from source spell ID 9 with
entry SLEEP, DEBUFF1, 4 MP, center range 1–2, radius 1 and the opposing silence-affected
Support properties. A live learned book supplies the rank to Battle Magic. [Private spell binding](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs)

### Architecture decision

Direct reuse is sufficient for SLEEP 1. Content admits the rank independently of the icon
inventory, and Application derives the action and target choices from the admitted definition and
the live book. The Domain builds one ordered effect roll per target. Resolution validation
re-evaluates the same rule against those recorded draws, keeping construction and playback
consistent without consuming RNG again. Application pays the caster before reactions and publishes
the target word only when the successful reaction plays. Physical and spell sleep share the packed
counter, turn skip, wake and existing save representation. Godot reads the choices and projects the
icon, effect text and resulting state. This keeps the learned rank, RNG and status in their existing
owners. The [shared DEBUFF1 presentation](../godot/spell-presentation.md#debuff-family-playback-2280) now covers learned SLEEP/MUDDLE and Giant Knuckles Use; broader presentation remains under
[#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).
[Resolution and playback](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) ·
[Turn continuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs) ·
[Spell description](../../src/Sf2.Remake.Application/Runtime/SpellDescriptionFacts.cs)

### Controlled Tyrin 25 input and cold Continue

**Confirmed** in the remake. Using prepared #2001 content, a controlled MAGE 25 Tyrin enters
Battle26 Movement through title Continue. Chinese Magic shows 催眠 1 and 4 MP, both spell-9
icon states and the opposing-area description. Cancel returns to the same row with the payment
and both seeds unchanged. Select Dark Bishop (enemy-13) at (11,23). The ordered reactions apply
sleep 192 to Lesser Demon (enemy-12) and report no effect on the bishop. Tyrin's MP changes
49→45 and EXP 0→5. Janet gains Movement. Ordinary Save, fresh-process Continue and immediate
re-save retain the active count, caster resources, queue and RNG. All three decoded battle data
objects are equal at sequences 8, 9 and 10. Bowie's ordinary EGRESS then returns usable control
on Map36. Both successful processes exit 0 with ObjectDB shutdown warnings. Tyrin's natural
level-25 acquisition and cast from an unchanged campaign history remain **Unknown**.

Use the registered #2001 priority-v1 world, main-scenes, entry and opening-inputs with the
[ordinary private input selection](../content/presentation-assets.md#bedoe-audio-package).
Select catalogs-v1/world.zh-CN.json for display names and the registered #1941 Chinese entry
catalog. Both icon variants are already in the Battle26 scene. The verification mounts these
files in place. It needs no replacement world or asset extraction. Use the locked Debug adapter,
the issue1955 Chinese instant-text settings and a fresh disposable save namespace on G: outside
the checkout. Keep the controlled input and receipts in the checkout's ignored local directory.
The controlled slot is about 42 KiB and the four compact native receipts total about 292 KiB.

Derive the controlled slot from a copy of issue1966's zh-CN-learned01 Battle26 slot. Keep both
battle seeds, positions of other units, and the rest of the round-6 queue. Change only these
party and staging operands.

1. Use the existing undeployed MAGE 24 Tyrin row. Give him Karna's deployment square and
   current position (11,24). Set level 25, max HP/MP 28/50, base ATT/DEF/AGI 21/26/30 and
   current HP/MP 27/49. These are one legal growth result from Tyrin's level-24 values using
   seed word 0 for the controlled growth calculation. Keep ATT 36 and his equipment. In
   progress and loadout use packed [75,76,9,63] and append learned SLEEP 1 to expanded ranks.
2. Use Karna's unjoined level-24 values, equipment and packed book from issue1927's castle-en
   slot. Remove BLAST 3 from her expanded ranks, set current ATT 41 and give her Tyrin's former
   deployment square with no current position. In the story flags remove 15 and 47 and add 17
   and 49, leaving Tyrin as the only Creed recruit. Keep Janet deployed.
3. Map Janet's queue entry to Tyrin and Karna's queue entry to Janet. Redirect any last-target
   or death-credit references to Karna to Tyrin. Encode the controlled payload in the same
   [save envelope](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs) with the matching
   SHA-256, and put the new copy in disposable slot 1. The published slots remain read-only.

Run the private behavior case with SF2_SLEEP_CONTROLLED_SAVE pointing to this copy. For native
verification, use title Continue and open Magic from Tyrin's Movement. Select SLEEP, back out
at the target screen, reselect the rank and target enemy-13, then acknowledge the results.
At Janet's Movement, open Options with F10, choose Save with Up and Confirm, confirm slot 1
and select the option to keep playing. Open a fresh process, Continue slot 1, immediately
re-save and compare complete decoded data. Use Stay until Bowie's turn and select 撤离
from Magic. Observe usable control on Map36.

The first native attempt reached both SLEEP results and returned Movement, then the older
Options key sequence timed out before Save. The first cold run successfully re-saved before its
observer missed the localized EGRESS label. Both attempts exited 1. The corrected save and
cold-return runs retained the same cast/save state and exited 0. These were observation-input
failures. They required no runtime or save-workflow changes.

**Confirmed** ordinary compatibility on the unchanged published issue1966 zh-CN-learned01
Battle26 slot. A disposable copy cold-continued in English with the registered issue1966 world
and issue1941 catalogs. At round 6, Janet's Movement was usable, re-save advanced the sequence
7→8 with identical decoded battle data, and Stay returned to Karna's Movement. The seed had
zero active sleep counts. The tracked [Save observer](../../game/probes/engine_battle_save_observation.gd)
ran with SF2_SAVE_CASE=shape using the Pacalon player-config inputs and issue1955 English settings
under the [native save procedure](../verification/battle-save.md#method-and-reproduction).
One 42,030-byte input slot and about 1.6 MB of state/input/projection receipts sufficed.
Godot exited 0 with an ObjectDB shutdown warning. Historical Battle03 physical sleep separately
observed a skipped turn and restored ordinary control. [Existing physical-sleep run](../verification/field-programs.md)

## DESOUL 1/2 implementation and evidence

**Architecture.** The learned offensive action already admits the opposing center, sorts
the diamond, stages one MP cost and owns the ordered award and custody transaction. Extending
that action with a typed `InstantDeath` reaction keeps those owners and lets the result carry
HP zero, its prior status, and no damage amount. A separate action would repeat that transaction.
Encoding the source operand as damage would enter damage EXP, variance and hit motion.
`BattleActionResolution` validates the exact per-target STATUS draw and reaction before
publication. The elemental and consumable branches retain their existing damage route.

A successful reaction enters the existing death message, drop receipt, field removal and
after-action continuation. The shared death batch owns actual and forced deaths and their
accounting. The settled save persists the existing fields, including the next queue position,
both RNG values, MP, EXP, kills, gold and custody. No new persisted effect field is needed.
Private Content admits the exact source ranks and flags, including their source animation
expressions. That admission is independent of the icon mount. Godot renders the ordinary
localized cast, death, ineffective and award messages on black. Source DESOUL animation v1/v2
is tracked by [#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).

**Confirmed** in [engine behavior cases](../../tests/Sf2.Remake.Engine.Tests/DesoulSpellTests.cs).
Rank 1 checks each resistance threshold, the all-miss award and halving, and the old HP/MP/status
through a paid reaction. Rank 2 checks a paid silenced cast, unlearned and short-MP refusal, and
legal area settlement. The connected cases exercise a leader kill, one chance drop before the
next target draw, a pre-set flag, full-pack delivery to Deals, reward and forced cleanup before
field return. A separate kill/miss/kill verifies two kill EXP and gold contributions, actual
kill credit, and the caster's existing lethal after-turn poison producing defeat when the immune
enemy survives. The zero-enemy variant reaches victory before that poison tick. Ordinary player
admission excludes friendly targets. Enemy/automatic DESOUL and item-origin DESOUL are outside
this binding.

### Controlled Kazin input and cold Continue

**Confirmed** in the remake. Start with the registered issue1966 zh-CN-learned01 Battle26 slot used
above. Kazin already has MAGE 29, 55 MP and packed `[139,71,6,72]`. Keep the class, party, book,
progress, deployment square, story and both seeds. In a disposable copy only, move Kazin from
(5,25) to the free tile (11,24). At round 6 cursor 18, swap the actor identities at zero-based
queue positions 2 and 18 (Kazin and Janet). Keep the stored queue scores. Write two copies of the
result in separate disposable namespaces using the existing save envelope. Each staged slot is
42,037 bytes. This setup shortens the input to a cast from an existing legal book.

Use the same #2001 priority-v1 world, scenes, entry and opening-inputs, Chinese catalogs and
issue1955 Chinese instant-text settings as the [Tyrin reproduction](#controlled-tyrin-25-input-and-cold-continue).
Use title Continue for each copy. From Kazin's Movement, Confirm then Magic opens the learned
rows. The normal and selected spell-8 icons are visible. Choose 摄魂 1 and Lesser Demon
(enemy-12, two tiles away) in one copy. Choose 摄魂 2 and Dark Bishop (enemy-13, one tile away)
in the other. Acknowledge each result. Rank 2's radius 1 includes the demon before the bishop
even though the bishop was the selected center. The demon falls (HP 40→0 and removed from the
field), and the bishop takes no effect (HP 39). Rank 1 pays 8 MP (55→47) and rank 2 pays 13 MP
(55→42). Both give 1 EXP (5→6), 360 gold (8747→9107), one kill (58→59), and return to
ally-15's Movement at cursor 20.

Open Options with F10 and choose Save with Up and Confirm. Save slot 1 and keep playing. For rank
2, start a fresh process, Continue slot 1 and Save again before acting. The decoded records at
sequences 8 and 9 have equal complete `data`, including ranks, removal, HP/MP, EXP, kills, gold,
custody, queue and both RNG words. Use Stay until Bowie's Movement and choose 撤离 1 through Magic.
Map36 resumes field input. All three Godot processes exited 0 without in-session errors.
Rank 2 and cold Continue emitted ObjectDB warnings on shutdown. The compact receipts total
about 1.38 MiB, within the 2 MiB budget.

Natural unchanged-history DESOUL casting, a native rare-drop DESOUL kill, and natural retail
boss victory by DESOUL remain **Unknown**. The engine cases exercise custody and terminal
behavior. The native segments exercise ordinary selection, result messages, settled save/restore
and return after a controlled stage.

## MUDDLE turn prerequisites

**Confirmed** in [controlled engine cases](../../tests/Sf2.Remake.Engine.Tests/MuddleTurnTests.cs).
The packed count and confusion bit stay in the combatant's status word. The existing physical
impact uses range 2 for the draw when the attacker is muddled. The usual sleeping or stunned
defender bypass takes precedence. The after-turn continuation consumes the current count
between SLEEP and SILENCE. An expiry clears count and flag with notice 355 using the existing
timed status delivery. A decrement retains the flag and advances without a message. Skipped and
queue-only entries use the same continuation, including a second entry in the same round.

### Temporary ally commandset 06

[SourceAllyAi](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SourceAllyAi.cs) owns commandset 06
for both the activation-driven physical caller and the temporary ally turn. The
[advancer](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) derives the
temporary decision from the status word after the SLEEP/STUN skip. The
[automatic action validator](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)
uses the same predicate. Permanent control, faction, slot, commandset input and activation
word remain the combatant's own. After expiry the same entry stream can return a player
to Movement, even on their next entry in the same round.

Both sides share the [physical](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemyPhysicalDecision.cs)
and [learned spell](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemySpellDecision.cs) selectors.
Automatic allies use source-slot order, skip enemy class preference and preserve LastTarget.
The shared ally spell filter serves ATTACK and DISPEL threat scoring. Spell candidates and
physical candidates consume thinking RNG in source order. Category selection uses the
two source channels described below. Chosen actions go through existing movement, admission
and scenes. A failed
attack tries healing, then the existing allied SUPPORT failure and MOVE route. Controlled
engine entries with empty packs complete physical Attack, BLAZE with DESOUL earlier in the
book, HEAL, paid silenced HEAL, AURA 1 and MOVE. A temporary ally with learned HEAL 4
uses the same packed-key MP exception as the enemy selector. The HEAL 3 and 4 regressions
start at 3 and 14 MP and each pays once, clamping to zero. Ordinary manual selection still
requires the chosen rank's full cost. AURA 1 uses the shared roster and caster admission
at choice and action resolution. [Behavior cases](../../tests/Sf2.Remake.Engine.Tests/MuddleAllyTurnTests.cs)

Temporary ally Attack scores the held item before the learned spell and physical choices.
For the admitted packed inventory, the source getter skips empty slots and stops at the first
occupied slot. The item must be worn and cast BLAZE, FREEZE, BOLT or BLAST. Black Ring meets
those conditions in admitted campaign content. When both ranged choices can reach, one
thinking RNG(2) after the scores selects spell (0) or item (1). Damage and wear still start
from the unchanged main seed. A single ranged choice uses main RNG(6), with the source
AQUA 1 exception. [Source and runtime choice evidence](../../../docs/research/battle/battle-ai.md) The selected item uses ordinary movement, reaction delivery
and equipment wear or removal. It spends no MP.

The Heal command can select Healing Rain without the normal item AI bit. If the combatant at
source slot 128 is at or below half HP, the caster uses Rain from its own tile. Otherwise the
first affordable HEAL/AURA entry retains its priority. If there is no such entry, the held
Rain is the fallback at or below two-thirds caster HP. The existing item action heals the
placed party and consumes the exact slot once. Engine turns cover the competing spell and
item categories, the occupied-slot stop, Black Ring wear and destruction, and the Rain
priority and fallback thresholds.

[Finite Prism Laser](gameplay-rules.md#finite-prism-laser) is authored-only, with no private
binding through Battle26. Its existing ray and ordered reactions now accept the MUDDLE 1
counter on the Flower or its occupants. The Flower completes the shared MUDDLE after-turn
step. A controlled engine turn covers the shot with both shooter and target affected.
The registered #2001 priority-v1 Battle26 scene has both spell-7 icons and the Chinese
catalog has its name.
[Source item getter](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/getnextuseableaiaction.asm#L196-L341) ·
[Source heal command](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/heal.asm#L18-L120) ·
[Source attack category](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/attack/determinebattleaction.asm#L69-L141)

Battle Start and settled battle Save/Continue admit rank-1 counters with their applicable
turn consumers and live confusion counters under the [finite rank-2 profile](#public-rank-2-admission).
A confusion flag without a live MUDDLE counter still rejects. Restoring a Movement decision
requires a current actor eligible for manual control. Other units keep their pending counters
and queue entries, and both RNG words survive. The battle and field wire formats carry the packed word.
Both temporary and activation-driven automatic allies use the shared battle growth and refresh
path. Level-up preserves the current rank-1 word until the after-turn tick. The ally commandset
requires physical operands for its actor and opposing source slots, and an encounter reward
table. Start, spell targets and Continue check the same live opponent occupancy contract
before entering this commandset. A placed opponent with no activation word or the neutral bit
keeps the automatic ally outside the finite profile. The source MOVE list includes living
on-map opponents, while the source target grid skips neutral occupants. This combination
requires movement and target-grid behavior beyond the admitted commandset. Authored manual
Movement and Stay remain available where the existing manual rules allow them.
[Source MOVE list](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/move.asm#L42-L90) ·
[Source target grid](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/buildtargetsarray.asm#L64-L93)
The battle return recovery policy clears the rank-1 counter when an eligible ally recovers,
regardless of its control mode. Victory recovery precedes the outcome program, retreat recovery
follows action closure, and defeat recovery follows dialogue and leader revival. Allies
ineligible for recovery retain the values supplied to recovery. Field growth and field-only
recovery keep their existing admission rules.
Ordinary enemy orders, enemy HEAL/AURA and existing Support use the same admitted rank-1
word. An enemy can still act under its normal commandset and then tick its status. Battle death
cleanup clears the status word of every processed casualty. Separate compatibility tests supply
an already fallen retained ally with a counter. Field Save/Continue preserves that input, Church
Raise restores HP and leaves the word intact, and fresh battle entry clears the temporary bits
on refresh. Ordinary scene reset retains its narrower admission. [Source death cleanup][deaths] ·
[Field save test](../../tests/Sf2.Remake.Engine.Tests/BattleSaveReturnTests.cs) ·
[Raise test](../../tests/Sf2.Remake.Engine.Tests/FieldServiceTests.cs) ·
[Fresh entry test](../../tests/Sf2.Remake.Engine.Tests/PrivateBattleInitializationTests.cs)

## MUDDLE 1 implementation and evidence

The private binding admits source slot 7, rank 1, DEBUFF1, 6 MP, range 1–2 and radius 1.
The learned book supplies the rank. The shared Support action stages an ordered STATUS draw
for each opposing recipient, pays silenced and all-miss casts once, and delivers the packed
word and status EXP through ordinary playback. The modern result, inspection status and
expiry expose the effect in English and Chinese. Full original animation remains with
[issue #2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).

Reuse the [Kazin controlled setup](#controlled-kazin-input-and-cold-continue) from #1966
zh-CN-learned01. Make the same position and queue swap, keeping both seeds and the original
UTC save header. Mount #2001 priority-v1 and its catalogs as above. With ordinary Chinese
Magic choose 混乱 1 and center on Dark Bishop (enemy-13). Before paying, Cancel and reselect the
same rank. The Lesser Demon (enemy-12) is first in the area and reaches status 48. The Dark
Bishop takes no effect. Kazin's MP 55→49 and EXP 5→10. Karna returns to Movement, round 6
cursor 20.

At that Movement open Options with F10, Up and Confirm. Save slot 1 and return to play.
In a new process Continue and save again without taking a turn. The decoded battle data
is equal in full, including status 48, queue, cursor and both RNG words. Save and cold
Continue used the original 22-HP Bowie setting. For a separate controlled short suffix only,
set Bowie's current HP to his existing maximum 62 in a disposable copy of the saved
battle. Keep both seeds and all other data. From the restored Movement use Confirm then
Stay until the affected enemy's turn has completed. The Lesser Demon attacks normally and
its range-48 roll of 20 reduces its count 48→32 at round 7. Continue to Bowie's Movement,
cast EGRESS 1 and save after usable Map36 field control. Engine cases also cover count
expiry, recast at the current word and status-specific admission refusals.

The unchanged 22-HP suffix had a legitimate defeat before after-turn. The ordinary cast
and re-save succeeded even though process shutdown failed after the cast. The cold suffix
first stopped in ActionChoice because the old driver sent an extra Confirm after Stay.
The controlled 62-HP suffix completed the countdown, EGRESS and field Save and then exited
2 in the old quit-input wrapper. Earlier driver/slot preparation failures used missing
inherited script dependencies, a non-UTC rewritten header and old Options ordering.
Natural history from a MUDDLE cast through countdown and return remains **Unknown**.

## MUDDLE 2 automatic action design

**Decision.** First refactor the existing automatic offensive action boundary. Carry an internal
target relation through selection, detached preflight and delivery. Derive it from the turn owner
and live packed status, and validate it against the selected action at commit. Default to opposing
targets for ordinary actions. Only an admitted confused automatic offensive action uses its own
side. Per-action status checks would leave ordered candidates, preflight and replay free to
disagree. A second AI or action path would duplicate movement, cost, reactions and settlement.
Swapping actor factions would change occupancy, source-slot identity and outcomes. The relation
is transient and needs no saved field or manual command.

| Owner | Required change |
| --- | --- |
| Source AI and shared attack selectors | Search eligible source slots on the chosen side, in source order. Preserve enemy preprocessing, order promotion, commandset and priority inputs, and ally commandset 06. Skip Heal and Support while confused. Reuse the bounded learned-spell and equipped-item scans with the six-family spell and four-family item filters. The attack-position grid still uses actual opponents to obstruct movement and checks occupied stops. |
| Physical, spell and shared attack-item rules | Validate the selected center against the same relation. Spell and item areas follow the center's actual side and existing effect order. Keep center range, area eligibility and area radius separate. A protected slot can receive collateral damage. A reachable self uses one actor identity. Check range against the selected pre-move center when reconstructing the source search and movement, then use actual post-move positions for the scene area. |
| Automatic decision, movement and scene | Run the selected-center guard once, after category selection and before movement. Action 5 carries no move, targets, MP cost or item use. It still runs the inaction message, eligible scene-end award and turn closure. Leave enemy `LastTarget` intact on a guard. A completed attack writes it under the existing category rules, including the source item self-write. Detached preflight and live delivery share preparation and validate the same decision and reaction operands. Playback applies the prepared reactions once and projects the actual affected combatant, including when only one side is shown. |
| MOVE fallback | Keep ordinary order-specific movement, restrictions and terrain. For the confused MOVE command, supply the fixed own-side slot 0 or 128 to the existing pursuit owner, separately from the offensive list. Retain the Kraken approach-cost variant. Exercise a live fixed slot outside attack reach. The remake mapping for an absent or unplaced fixed slot is **Unknown**. Resolve a reachable unsupported fallback before admitting it. |

Replace the opposing-only roster in `SourceAllyAi` and the opposing-only offensive target queries
and validation in `BattleAutomaticAction` and the reaction validator. Keep path validation,
thinking-stream scores and the dual item/spell category draw, main-stream single ranged category
and guard draws, and the existing physical/ranged suppression rules. Source search and selected
movement use distinct unarmed range checks, so verify self selection and movement together.
The enemy's priority-15 class tie branch skips enemy candidates. A tied cohort with only
enemies yields no selected attack in the fixed source. Pass the already consumed scoring and
category draws on to the next command. The current `PhysicalTargetRules.Select` assumes ranked
candidates have an ally class. Exercise both a tied cohort and a single high-priority enemy.
[Source class tie][attacktie] · [Reachability][reachable] · [Source attack range][attackrange] ·
[Attack command][aiattack] ·
[Source area][area] · [MOVE][move] ·
[Decision preflight](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs) ·
[Action validation](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) ·
[Automatic delivery](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs)

Use raw `SPELLDEF_POWER` adjusted for target resistance in the shared
`EnemySpellDecision.Score` for learned spells and attack items, including ordinary and
confused enhanced enemies. Live learned-spell damage still applies eligible enhancement
before resistance. Correct any ordinary enemy scoring expectations that used execution power
as the estimate. [Source AI estimate][aipower] · [Source spell damage][castpower]

### Settlement and current combinations

**Confirmed.** An ally can earn damage and kill EXP contributions against an ally. An eligible
surviving actor receives the scene award with the confusion halving rule above. A lethal self hit
therefore skips the final EXP award. Kill gold and action drops require an ally striker and an
actual enemy casualty. Enemy-on-enemy damage has no EXP recipient, scene first ally, kill credit,
gold or drop. An ally casualty still gets its own defeat entry. Leader and last-enemy checks use
the actual casualties, including a leader hit as collateral. [Damage and kill EXP][exp] ·
[Scene initialization][init] · [Scene actor selection][sceneactors] · [Scene award][end] ·
[Drop helper][drop] · [Death processing][deaths] · [Battle loop][battleloop]

Replace the paired ally/enemy assumption in `Sf2PhysicalAction` with actual striker, casualty
and eligible recipient. In `Sf2AttackSpellAction`, require an actual enemy casualty for
kill-gold construction and use the caster's resulting HP for EXP eligibility. A self reaction
edits one actor across physical recoil, spell payment and equipment wear. Derive halving in
`Sf2BattleProgressionRule` from the scene award rules, with the confusion exception scoped to
the offensive or inaction scene. Use the same operands in construction and validation.
`EnemyItemDropRules`, `BattleDeathBatch`, `BattleOutcomeRules` and outcome return retain their
existing roles. Preserve opposing counter credit, support/DETOX halving and settlement order.

| Current coverage that exercises the boundary | Decision before binding |
| --- | --- |
| Battle26 Lesser Demon, Dark Bishop and Wizard | Exercise actual enemy physical and BLAST 3 or BLAZE 3 area turns. Dark Bishop's AURA is skipped while confused. The carrier's Black Ring initializes bare and cannot enter that enemy's attack-item list. Preserve Wizard move orders. The first encounter's neutral Higins is a scene sprite without a turn or target entry. Use the existing controlled Kazin setup for the short cast witness. [Battle26 formation][battle26roster] · [Enemy definitions][enemies] · [Battle26](../application/battle26-higins.md) · [Rank-1 witness](#muddle-1-implementation-and-evidence) |
| Black Ring, offensive books and promoted Kiwi | Use the existing controlled ally commandset with eligible Black Ring wear and a real learned elemental cast. Healing Rain, DESOUL and enemy AQUA supply excluded-choice controls. The bound MNST physical-to-breath conversion inherits the relation after the guard. No learned KIWI entry enters the six-family scan. [Item and category rules](#temporary-ally-commandset-06) · [Monster conversion][turn] · [Promoted Monster](../application/ordinary-promotion-classes.md#monster-controlled-reproduction-and-ordinary-pilot) |
| Battle16 Kraken and Battle18 Taros | Kraken arm/head reach and SWARM orders are current consumers. Keep the live slot-128 guard and Kraken approach table. Taros has STATUS immunity, so ordinary MUDDLE 2 casts cannot confuse him into using the sword on his side. His existing BOLT 2 and target effectiveness keep their owners. [Kraken formation][kraken] · [Enemy definitions][enemies] · [Taros](../application/taros-caravan.md) |
| Authored profiles and campaign limits | Use one finite turn check for a temporary player-controlled ally and a supported activation-driven ally. A Move/Stay-only actor without required operands, the unconnected Prism Laser and later special AI are outside this binding. Natural ally confusion, a selected self action in ordinary play and natural leader collateral remain **Unknown**. Use controlled legal inputs for those consumers. |

### Internal automatic turns

The source automatic commandsets now carry the offensive target relation through preflight,
movement and the existing scene. Controlled engine turns cover attacks on both factions'
own sides, paid and guarded self actions, Black Ring wear and growth, the MNST conversion,
fixed-slot pursuit, area casualties and terminal cleanup. Eligible double and counter draws
still precede suppression. Enemy class ties keep the draws the search has consumed before it
continues the commandset. A point order that stays in place can finish one unsuccessful
ATTACK and reach another. Validation keeps the ordered main and thinking RNG chains and
checks the category within each attempt. Enemy learned spells and attack items share the
raw-power estimate. For each spell center, AI scores the recipients in source coordinate order
from the outermost ring to the center. Construction still uses the existing scene effect and
reward order. [Spell range contract](../../../docs/design/contracts/battle/battlefield-navigation.md#range-and-target-construction)

The inaction scene records the selected source message ID and uses generic English and Chinese
feedback. Its targetless presentation accepts a fresh Confirm and continues to the award
and after-turn. The same-side close-up shows one card for the displayed combatant, including
self, spell and MNST reactions. Opposing close-ups retain separate cards. A controlled
headless presenter observation exercised these paths with authored inputs and placeholder
MNST rasters. Original effect animation and prose variants remain with
[issue #2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).

With the registered worktree SDK environment loaded, run the engine project with
`--filter 'FullyQualifiedName~Muddle|FullyQualifiedName~EnemySpellDecisionTests'`.
[Actual turn cases](../../tests/Sf2.Remake.Engine.Tests/MuddleConfusedTurnTests.cs) ·
[Estimate and execution cases](../../tests/Sf2.Remake.Engine.Tests/EnemySpellDecisionTests.cs) ·
[Public entry cases](../../tests/Sf2.Remake.Engine.Tests/MuddleTurnTests.cs)

### Identity, control and target relation review

**Confirmed by static inspection and retained controlled cases.** The reviewed boundary runs from
queued actor dispatch through source-command selection and offensive reaction admission in the
playable profile through Battle26 and the named authored cases.

- **Ordinary opposition.** Manual Attack, learned damage and offensive items default to opponents.
  Ordinary enemy commands and activation-driven allies also score opponents. Slot 0 or 128 can
  be an ordinary opposing center even when confusion protects it from its own side. HEAL and
  friendly Support apply their own targeting rules.
  [Ordinary enemy decisions](../../tests/Sf2.Remake.Engine.Tests/EnemySpellDecisionTests.cs) ·
  [Spell admission](../../tests/Sf2.Remake.Engine.Tests/AttackSpellAdmissionTests.cs) ·
  [Item turns](../../tests/Sf2.Remake.Engine.Tests/AttackItemSessionTests.cs)
- **Temporary control.** After absent, sleep and stun checks, a player ally with a nonzero count
  takes temporary commandset 06. Rank 1 searches opponents and may try HEAL when ATTACK fails.
  Rank 2 searches its own side in source-slot order and skips healing and support. The enemy
  keeps its normal source strategy. The temporary ally decision preserves deployment identity
  and the activation bit that governs permanent control.
  [Dispatch](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
  [Ally commandset](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SourceAllyAi.cs) ·
  [Enemy commandsets](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) ·
  [Rank-1 turns](../../tests/Sf2.Remake.Engine.Tests/MuddleAllyTurnTests.cs)
- **Protected center and collateral.** The selectors feed the chosen physical target, spell
  center or item center to the same post-selection guard. A protected center triggers inaction
  with no guard draw. The actor keeps its origin and LastTarget and bypasses damage, cost and
  wear. A legal center passes to the shared spell/item builder. The builder and reaction
  validator use the resolved side and post-move caster position to enumerate the area.
  [Guard](../../src/Sf2.Remake.Domain/Battles/Rules/BattleOffense.cs) ·
  [Spell area](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) ·
  [Item dispatch](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs)
- **Self target and delivery.** A selected unprotected self consumes one main range-2 draw.
  Validation checks its place in the recorded seed chain. Detached preflight discards prepared
  state. The movement continuation carries the decided side into action preparation. Roll 1
  produces a targetless inaction scene with the message draw and eligible ally award. Roll 0
  allows the same actor to receive the reaction and retain its caster or holder identity.
  [Decision preflight](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs) ·
  [Delivery](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs) ·
  [Decision and reaction validation](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)

The protected slot test also takes precedence for a protected actor selecting itself. In the
controlled enemy area case, the slot-128 caster hits itself through the adjacent enemy's center.
The guard examines that selected center, so the caster receives its collateral reaction without a
self draw. The [scripted Jaro case](gameplay-rules.md#jaro-conversion-and-source-ally-commandset6)
uses distinct enemy and ally actor refs and replaces their participation by slot. It writes
activation control explicitly, outside temporary MUDDLE dispatch.

The original [confusion predicate fixture](../../../tests/fixtures/h3/muddle-confusion-v1.json)
establishes the count/flag distinction. The controlled [action guard fixture](../../../tests/fixtures/h3/muddle-action-guard-v1.json)
establishes slot protection, the self roll on either side and the branch to action 5.
The pinned [reachability][reachable], [ATTACK][aiattack] and [area][area] routines establish static
selection and area order. Natural original turns from cast to selected self action or leader
collateral remain **Unknown**. Later special AI and unadmitted slots remain outside this profile.

The retained [confused turn cases](../../tests/Sf2.Remake.Engine.Tests/MuddleConfusedTurnTests.cs)
distinguish protected centers on both sides, slot 128 collateral, physical self pass/fail and
successful learned spell and equipment self-use. Reproduce these controls with the
[engine project and filter](#internal-automatic-turns). This review reused that evidence and found
no new rule defect within its boundary. Movement legality, settlement, status expiry and
Save/Continue receive separate review under the [mechanism order](gameplay-rules.md#battle-mechanism-review-order).

### Public rank-2 admission

The private binding takes source entry `MUDDLE|LV2` from the existing learned book.
Support owns 11 MP, range 1–2, radius 2, one STATUS draw per recipient, the current
word and the existing EXP award. It tests the prospective word when the target can be
affected. A failed roll leaves the target's current word intact. A successful roll sets
or refreshes the count to three with the confusion flag.

Battle Start and battle Save/Continue check the current word with the same turn-admission
rule. It uses the existing source strategy, slots, loadout and live occupancy.
Both factions require the physical definition of potential source teammates before
admitting a confused turn. Scoring can reach those teammates after movement and
estimates damage before checking a protected center or the self guard.
The authored reader parses `muddle1`–`muddle3` for rank-1 counts and
`confusion1`–`confusion3` for rank-2 counts, then uses that same Start check.
Content and Godot reuse already admitted spell identity icons across campaign scenes.
Both ranks use the spell-7 pair; a later learned book can carry it into an earlier battle.

The existing offensive relation and source commandsets own execution. Their fixed-slot
MOVE distinguishes a placed combatant, a cleaned casualty, and an unavailable slot.
The battle snapshot represents a cleaned slot with `Initialized = true`, HP 0, no
position and status 0. This yields empty movement through the existing commandsets,
with the same failed-search RNG and after-turn chain. An unmapped slot or a dormant
slot with `Initialized = false` remains unavailable. The accepted
Battle25 route can clear slot 128 while the battle continues. The engine case actually
kills slot 128 before the next confused enemy consumes its empty MOVE and countdown.
[Battle25 death and cold Continue](../application/battle25-north-cave.md) ·
[Fixed-source analysis](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967#issuecomment-7117627470)
Out-of-grid RAM side effects and unmapped or never-placed slot behavior remain **Unknown**.
Slot-0 defeat continues through the existing outcome check.

Field recovery owns clearing temporary status on living returnees. Battle Save/Continue
retain the current word, decision seeds and fixed-slot casualty state without rerunning
a decision. Field Save/Continue and Raise retain the word of a controlled already-fallen
ally. Fresh battle recovery clears it from the raised ally. Casualties processed by the
battle death batch have already had their word cleared.

Field EGRESS and a riding map rebuild use the source ExplorationLoop recovery. EGRESS
preflights the same recovery that its bound program applies after the completed flash.
A controlled fallen caster retains either rank through field Save/Continue and Raise,
pays EGRESS once, then recovers HP/MP and clears the temporary word before arriving at
the refuge. The ordinary authored reset keeps its existing status check.
[MainLoop](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/mainloop.asm#L19-L35) ·
[ExplorationLoop](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/exploration/explorationfunctions_2.asm#L8-L19) ·
[Source recovery][heal]

### Controlled MUDDLE 2 and cold Continue in Battle25

**Confirmed** in the remake. Use the registered #1918 cave02 Chinese `battlecold01` slot
at round 25 cursor 1. Dark Soldier slot 128 has already died and Dark Bishop still has
39 HP. Kazin already has MAGE 28 and the learned rank, with packed book
`[139, 71, 6, 8]`. In a disposable copy set Kazin's current HP to his stored max of 35
and place him on free tile (18,12), one tile from the Bishop. Keep Kazin's stored
14 MP, 67 EXP and growth/equipment. Set zero-based queue identities 1, 3, 4, 5 and 6
to Kazin, Gerhalt, Dark Bishop, Bowie and Karna, respectively. Keep the stored agility
scores and the dead slot at entry 2. Use controlled main RNG high word 4010, retaining
the low word and the thinking seed. Preserve the record's UTC timestamp when writing the
existing envelope. The prepared slot is about 39 KiB.

Mount the registered #2001 priority-v1 world, scenes, entry and opening-inputs, the
#2001 and #1941 Chinese catalogs, and the #1955 Chinese instant-text settings. Build the
normal game Debug adapter. Title Continue loads Kazin's Movement. Confirm and enter
ordinary Magic, select 混乱 2 (11 MP) with the spell-7 icon and confirm the Dark Bishop.
Acknowledge the result. Kazin pays 14→3 MP and gains 67→72 EXP. The Bishop's word
becomes 56 and Gerhalt gets Movement at cursor 3. Open Options with F10, Up and Confirm,
save over slot 1, then keep playing.

Start an independent Godot process, Continue the slot and save again without taking a
turn. Complete decoded gameplay data is equal across the two saves. From Gerhalt's
Movement confirm Stay, acknowledge the Bishop's guarded self action, then wait for
Bowie's Movement. The Bishop keeps 39 HP, completes its own turn and decrements its
word 56→40 from the range-48 roll of 24. Slot 128 stays unplaced. Bowie's ordinary
EGRESS pays 8 MP and returns usable field input on Map9. The actual cleaned-slot MOVE
case is covered separately by the engine test. The final two observation processes
exited 0. Earlier runs emitted ObjectDB exit warnings. The observation driver uses
its own exit, so normal menu shutdown is outside this evidence. These inputs
establish remake behavior with a controlled caster and queue. A rank-2 cast through unchanged campaign history and natural ally
confusion remain **Unknown**.

The [save/first-consumer review](../application/battle-save-and-continue.md#save-restore-and-first-consumer-review)
rebuilds the same input and compares one uninterrupted Stay with one cold-continued Stay.
Both reach Bowie's Movement at cursor 5 with the same full gameplay data and exactly three
main-stream draws. The cold run also checks that the system menu disables Save during the
guarded message. The initialized casualty remains off the projected board in both runs.

### Delivery sequence

The internal automatic turns preceded the [current public binding](#public-rank-2-admission).
The two stages had these acceptance boundaries.

1. Complete the internal automatic turn for both factions, with physical, admitted spell/item,
   fixed-slot MOVE and guarded inaction. Keep public rank-2 status and spell entry closed. Use small
   actual engine turns for no-target movement, protected-center and self pass/fail, same-side
   kill and area casualty. Check payment/wear, one actor on a self reaction, RNG order and
   single replay. Include an eligible ally award on a half-EXP battle, an enemy kill with no
   allied reward or drop, and a terminal casualty. Verify that the existing death batch removes
   defeated self and same-side targets from the map, clears their status words and refreshes
   their stats. Show eligible double and counter draws with no follow-up strikes. For an
   enhanced enemy, compare ordinary and confused casts. Both use raw power and resistance for
   scoring and enhanced power and resistance for damage. Preserve opposing counter and drop
   controls.
2. Admit the finite rank-2 consumer set together with the effect in the existing support owner.
   Share turn admission across spell targeting, typed Start, battle Save and Continue. Check
   the live operands the selector and action builders need. Status syntax, standby, growth,
   refresh and recovery keep their existing owners. Exercise confused ally and enemy queue
   closure including sleep/stun and an extra entry. Separately test field Save/Continue, Raise
   and fresh-battle recovery with a controlled retained ally supplied already fallen with a
   rank-2 word. Release the private binding when those consumers pass.
   Use the existing Kazin setup for one short ordinary Magic suffix with an automatic action,
   active-counter cold Continue and return. Capture input, state, projection and errors.
   Additional natural witnesses and full effect animation are separate work.

## Implementation order and ownership

Learned SLEEP 1, DESOUL 1/2 and MUDDLE 1/2 have player bindings at the admitted current
campaign boundaries.

Use the finite effects on the existing learned-spell entries. Putting all three families on
the damage path would conflate status EXP and instant death with modified damage. A parallel
hostile action framework would duplicate target admission, cost, ordered reactions, award and
save guards. Extend support for the two status families and attack for DESOUL, sharing the
existing rules and source-specific result operands. Keep class/learned-rank data in Content
and selection in the session. A binding only becomes available when both its effect and
required consumers are admitted.

1. **SLEEP and shared status admission.** The finite support effect uses
   [SupportSpellDefinition](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SupportSpellDefinition.cs),
   [Sf2SupportSpellAction](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs) and
   [AuthoredScenarioPackageReader](../../src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs).
   [PrivateSupportSpellBindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs)
   admits the source rank independently for the private campaign.
   It reuses the shared Magic selection, silence, area handling and DETOX's target-side award
   rule. [BattleActionResolution](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)
   checks the ordered draws and reaction words. [SleepTurnRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SleepTurnRules.cs)
   and [BattleAfterTurnContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs)
   consume the same word for spell and physical sleep. The controlled Tyrin case above
   exercises the private rank through ordinary Magic and cold Continue.
2. **DESOUL 1 then 2.** Consume the accepted [#1944 attack and item split](offensive-items-design.md).
   Keep instant death on the learned spell path and preserve elemental attack-item resolution.
   [AttackSpellDefinition](../../src/Sf2.Remake.Domain/Gameplay/Sf2/AttackSpellDefinition.cs),
   [Sf2AttackSpellAction](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) and
   [PrivateAttackSpellBindings](../../src/Sf2.Remake.Content/Scenarios/PrivateAttackSpellBindings.cs)
   admit both finite ranks. The validated instant-death reaction adds exactly kill EXP through
   [Sf2BattleProgressionRule](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs).
   The same settlement uses
   [EnemyItemDropRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/EnemyItemDropRules.cs),
   [BattleDeathBatch](../../src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs) and
   [BattleOutcomeRules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs).
   The existing commit/replay authority retains effects, custody receipts and scene waits.
   [The observations above](#desoul-12-implementation-and-evidence) cover both ranks.
3. **MUDDLE 1 then 2.** The [turn prerequisites](#muddle-turn-prerequisites) and
   [rank-1 evidence](#muddle-1-implementation-and-evidence) record the accepted first rank.
   The [automatic action design](#muddle-2-automatic-action-design) and its
   [public admission](#public-rank-2-admission) record the accepted rank-2 profile.

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
contains Tyrin. The [controlled Tyrin 25 input](#controlled-tyrin-25-input-and-cold-continue)
establishes cast and restore capability. His level-up and subsequent cast in a natural
campaign history remain **Unknown**.
[Tyrin growth][tyrin] · [Creed branch and persistence](../application/creed-recruitment.md)

Measure the first slot and receipt before copying more. Budget up to eight slots at 128 KiB
each and up to 2 MiB of compact state/input/projection/error receipts, roughly 3 MiB before
build output. Open one owned debug instance at a time, reuse the accepted Pacalon package
(~102 MB) and registered Creed input in place, and budget no generated replacement worlds,
original captures, video, screenshots or frame traces. Observe the selected ranks, payments,
outcomes and errors from ordinary input at settled boundaries. If a rehearsed suffix requires
a campaign replay, a search for favorable seeds or a new resource export, report the missing
witness and its specific dependency before expanding the run. Any normal shutdown failure is
a completed process result separate from observed save and return. The source design used
static evidence and existing controlled runs. [Existing package and observation budget](detox-ranks-design.md#acceptance-and-input-budget)

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
[attacktie]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/attack/determinebattleaction.asm#L255-L421
[battle26roster]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/spritesets/spriteset26.asm#L59-L123
[doublerolls]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/determinedoubleandcounter.asm#L34-L80
[aipower]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/ai/command/attack/prioritizetargets.asm#L425-L456
[castpower]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/calculatespelldamage.asm#L34-L102
[attackrange]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/getactionrange.asm
[area]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/populatetargetslist.asm
[sceneactors]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/battleactionsengine_1.asm#L298-L349
[kraken]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/spritesets/spriteset16.asm#L63-L106
[message]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/createbattlescenemessage.asm
[init]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/initbattlesceneproperties.asm
[enemies]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/enemies/enemydefs.asm
[drop]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/dropenemyitem.asm
[battleloop]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop_1.asm
[deaths]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/processkilledcombatants.asm
[heal]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/heallivingandimmortalallies.asm
[kazin]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats04.asm
[tyrin]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats17.asm
