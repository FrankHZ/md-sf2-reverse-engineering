# Tactical Battle Loop and State Handoffs

A battle connects three kinds of choice and consequence: where a combatant can stand, what action
can be committed from that state, and what remains after its effects have resolved. The controller
then decides whether another turn is possible or an outcome must return to the caller. **Inferred
design connection:** positioning, action eligibility and persistent resources form the common tactical
loop. They explain the decision surface without establishing optimal strategy, balance or enemy intent.

This guide expands the [Gameplay Overview](../gameplay-overview.md#tactical-battle-legal-choices-become-effects-and-outcomes)
for readers understanding rules and later design or implementation choices. It consumes accepted
original evidence; it adds no original-game finding. **Confirmed** identifies a source-static rule
or a named bounded observation, **Inferred** a supported connection, and **Unknown** an unestablished
boundary. Static call order establishes requests and local state, not that an animation, service or
field return visibly completed. Exact parameters and proof remain in the linked owners.

Read entry and scheduling → movement and action choice → resolution → after-turn checks → outcome
and return. [Story Progression](story-progression.md#read-story-content-in-segments) provides encounter
admission and chapter context. After the common rules, the
[encounter comparison](#how-encounter-rules-change-the-common-loop) explains equipment gates, side
changes, special attacks, repeat trials and a reset caller. Original rules, observed play, accepted
remake choices and production support have [different boundaries below](#original-fidelity-and-modernization).

## Entering Battle and Selecting a Turn

### Fresh entry and resume are different routes

**Confirmed static:** a fresh battle clears elapsed seconds and requests the before-battle program,
then clears battle-region flags, heals the eligible party, initializes ally and enemy rosters,
clears AI memory, loads the battlefield and requests the battle-start program, in that order.
Setup programs can have conditional gates; entering the controller does not guarantee every scene
body runs. Healing restores HP/MP and refreshes stats for living allies and the Peter/Lemon exceptions,
retaining only its specified persistent-status mask. It does not revive every fallen member or define
what an ordinary healing spell does.

**Confirmed static:** suspended entry instead restores saved elapsed seconds, clears the suspend
flag and AI memory, reloads battle state and resumes the individual-turn loop. It does not repeat
fresh-battle roster construction and round entry as though a new encounter had started. This proves
an in-code resume route; cross-process suspended-save restoration and power-loss recovery remain
**Unknown**. The [battle lifecycle contract](../../contracts/battle/battle-control-lifecycle.md#entry-and-round-scheduling)
and [save contract](../../contracts/system/save-system.md) own those separate boundaries.

### A round activates, admits and schedules

**Confirmed static:** each new round activates enemies, requests the region program, admits and
animates spawning enemies, then generates turn order. Activation changes an enemy's control state;
it is separate from being placed, scheduled and choosing an action. An inactive but placed, living
enemy can enter the schedule and later take its standby route. A tested region is also distinct
from an activated region.

Hidden/respawning candidates have separate admission gates. **Confirmed static reset refusals:**
a candidate must be within the encounter's declared enemy entries, and its starting square must not
match any ally or enemy's stored coordinates. That occupancy check includes dead combatants and the
candidate itself. Refusal leaves the candidate unplaced with zero current and maximum HP and omits
it from the spawning list. A later attempt still depends on the spawn mode and its applicable
region-flag and maximum-HP gates; failure does not promise an unconditional retry or eventual
appearance. The newer [spawn/reset inspection](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#regions-checked-bits-hidden-appearance-and-reappearance)
qualifies the older lifecycle summary. Natural occurrence of these refusals and completed spawn
presentation remain **Unknown**.

Only placed, living combatants enter the generated order. Agility supplies a base, with ordered
random adjustments; the high agility bit can produce a second entry. Sorting preserves equal-score
insertion order and uses the original wrapped, signed score. Nominal agility alone therefore does
not promise the next actor, and a roster or initial formation does not prove first player control.
At the turn-order terminator the controller starts another round. Exact eligibility, signed edges,
second-entry arithmetic and RNG order belong to
[battle lifecycle](../../contracts/battle/battle-control-lifecycle.md#turn-order-construction).

**Confirmed bounded observations:** Battle01 activation and scheduling fixtures close their supplied
positions, seeds and helper boundaries. They are not complete played encounters. The later accepted
[original Map3–Battle01 frontier](../milestones/map3-battle01-readiness.md#accepted-original-frontier) separately
reaches natural first actor **2**, winning actions, victory and field readiness; the older seeded
actor-1 bridge remains controlled evidence. Neither result supplies every encounter's initiative,
activation history or AI decisions.

### Who controls the scheduled actor?

**Confirmed static:** a dead scheduled actor is skipped. For a living actor, MUDDLE, an AI-control
flag, ally auto-battle and ordinary enemy control can route the turn to AI; the opponent-control
toggle provides an enemy-to-player exception. Faction membership and control ownership answer
different questions. SLEEP, STUN and committed STAY consume the action without constructing a
battle scene. A skipped scene does not remove the later controller checks.

MUDDLE has two distinct meanings. A nonzero MUDDLE counter hands control to AI; the additional
level-2 flag together with that counter enables the confusion predicate used by side-flipped
selection. **Confirmed at the predicate/action-guard seams:** MUDDLE 1 is not automatically
level-2 confusion, and a confused action can still be refused for a protected same-side target or
by its self-target roll. These rules do not establish natural production of every confused target.
See [player/control routing](../../contracts/battle/battle-functions-control-flow.md#individual-turn-control)
and [MUDDLE's separate consumers](../../contracts/battle/combat-resolution.md#1a-distinguish-muddle-controlled-turns-from-level-2-confusion).

## Movement, Target, and Control Ownership

### A reachable destination is not an eligible target

**Confirmed static and bounded movement observation:** movement spends a terrain-weighted budget
from the actor's origin. Current MOV supplies the ordinary budget in the pathfinder's units.
Terrain type selects a cost within the actor's movement table; occupancy/obstruction can reject a
neighboring space even when budget remains. A destination reached exactly at the budget is admitted without
further expansion. Equal travel distance need not mean equal movement cost. The fixed battlefield
grid, controlled weighted examples and original propagation/tie rules belong to
[battlefield navigation](../../contracts/battle/battlefield-navigation.md#movement-initialization-and-admission).
The controlled flat-row-crossing edge does not establish normal shipped reachability or require
unsafe memory access in a remake.

Action range is a separate test from travel. Attack and ordinary spell ranges use Manhattan rings;
weapons, spell definitions and named unarmed exceptions supply their ranges. A reachable destination
need not put the actor in the selected action's minimum/maximum range. A ranged target can be
eligible without the actor occupying its tile. The ordinary target grid filters for placed, living,
non-neutral combatants; spell properties select the target side. Obstruction can suppress a
range-grid mark while the occupant lookup still finds a target. A space mark and a legal entity
list must therefore not be treated as interchangeable.

The exceptions matter. AURA 4 and SHINE enumerate a whole supplied faction rather than the ordinary
rings and neutral filter; Burst Rock uses both factions and excludes its center. AURA's accepted
geometry cases establish their own target generation and ordering. Other multi-target arithmetic
cases often receive an already ordered list, so they do not prove natural targeting geometry. Read
[range and target construction](../../contracts/battle/battlefield-navigation.md#range-and-target-construction)
and [spell resolution](../../contracts/battle/spell-resolution.md) before extending a rule to another effect.

### Player choice, cancellation and commitment

**Confirmed static player-control surface:** the actor can choose attack, magic, item or search/stay
through the battle action route. Cursor tile confirmation stores coordinates; target selection
consumes a supplied candidate list. An empty list returns no target, B cancels that selection and
A/C confirms it; directional navigation wraps among candidates. These input identities describe
original branches, not modern bindings or complete cursor feel.

For example, a player can provisionally move, open the action choice and then cancel the
movement/action route. The source restores the original local position and leaves action `-1`
(uncommitted). The later action builder has no committed attack to resolve from that result.
Selecting Attack likewise does not manufacture an opponent when its target list is empty.
This is a **Confirmed static cancellation/no-target boundary**, not a claim that all nested menu
cancellations have identical rollback or that the turn has already spent a spell/item cost.
The [control contract](../../contracts/battle/battle-functions-control-flow.md#player-control-surface) owns
the route; original input timing, previews, highlights and every nested re-entry remain **Unknown**.

STAY is a committed result rather than cancellation. It passes through later status and outcome
processing without constructing an ordinary scene. Search can instead take chest-specific routes:
a trap selects a trapped-chest action and requests spawning; resolved non-trap cases select STAY,
while full inventory requests chest closure and returns to the menu. These are source branch and
request facts, not evidence that every chest transaction or animation completed.

### AI attempts commands, then chooses among viable actions

**Confirmed static:** AI-controlled allies use commandset 6; enemies select among the source
commandsets. Commands run in order and stop at the first success. Inactive enemies use standby,
with movement/memory conditions and randomness of its own. An AI result of STAY can still carry
a movement string, so neither STAY nor command success alone says the unit remained in place.
Temporary terrain used for AI path selection is cleared on exit; it is not permanent map collision.

Availability depends on resources, filters, targets and a legal position. Attack-spell getters check
spell category and usable level; item getters also check battle usability, equipment and AI-use
conditions. Rejection is not uniform: some attack-item failures end that search instead of trying a
later inventory slot. The healing command requires a permitted spell and sufficient MP (minimum 3 MP
for HEAL, 7 MP for AURA) before its spell route; otherwise it can fall back to an eligible healing item.
The ordinary healing-target scan admits living same-side candidates at or below two thirds HP;
Healing Rain retains its separate earlier admission gate in the AI owner.
A caster with no affordable healing spell and no qualifying item cannot create a heal merely because
the commandset contains Healing. These are **Confirmed static** gates, not a universal player-menu
refusal model or runtime proof of every threshold/packed-entry edge.

Support has its own restrictions. Its normal command admits only selected enemy support spells;
confused enemies Stay, and a chosen center without a valid attack position can end in Stay rather
than trying the next ranked center. Source branches for other support effects do not make them
reachable through that entry. Special attackers retain dedicated routes.

AI scores candidate outcomes instead of running the real damage resolver. Physical estimates use
ATT/DEF and terrain; spell estimates use power and resistance, while priority scripts can also use
HP, prior targets, class, movement and randomness. These scores select an action/target, not promised
damage, fairness or intent. **Confirmed bounded runtime:** the final attack-choice matrix covers its
seven nonempty physical/spell/item viability combinations and ordinary priority/movement ties.
It does not establish every commandset, signed edge or natural-map path. The
[AI contract](../../contracts/battle/battle-ai-decision.md) retains those gates, the no-viable-action Stay result,
resource quirks and separate static/runtime limits.

## What the Committed Action Can Change

### Physical attacks

**Confirmed for the owned physical subsets:** dodge is decided before damage; a successful dodge
skips the damage route. Otherwise current ATT/DEF, land protection and eligible anti-air bonuses
feed integer damage stages, followed by critical and downward spread where admitted. The defender's
HP is clamped at zero. Terrain protection and airborne movement type are distinct inputs; an AI
estimate cannot substitute for this calculation.

A first hit can lead to a second hit and then a reversed counter, but the request is not a guarantee.
Target death rejects follow-ups. The double validator also rejects muddled/same-side cases; counters
have additional range, sleep, stun, same-side and directional special-enemy exclusions. Several of
those are controlled validator observations, not naturally selected actions. A lethal physical hit
also skips later ailment/curse/follow-up construction stages specified by the action owner.
Exact eligibility, prowess cases, arithmetic and the difference between natural RNG decisions and
forced-valid validator inputs remain in [combat resolution](../../contracts/battle/combat-resolution.md)
and [action construction](../../contracts/battle/battle-action-construction.md#physical-route-chronology).

### Magic, healing and support

**Confirmed at the named spell seams:** damaging magic uses definition power and consumer-specific
resistance, with promoted-power, critical, spread and summon-division rules at their admitted
boundaries. Resistance setting 3 can mean weakness for damage yet immunity for a status consumer;
there is no universal resistance percentage that explains all spells. Healing caps recovery by
missing HP, with distinct normal/full-recovery powers and healer/reward conditions. The listed healing
cases do not establish revival of an unplaced dead ally.

Status and support effects can change stored counters and derived stats at different stages.
SLEEP, MUDDLE, DISPEL, SLOW, ATTACK, BOOST and DETOX have separate effectiveness, existing-status,
level or cure gates. A failed effect is not uniformly a no-op: the confirmed ATTACK/BOOST recast
quirks can write a refreshed counter before failure while leaving old derived stats until refresh.
DISPEL instead has its own known-spell gate and replay-time status mutation. Named
[spell-resolution subsets](../../contracts/battle/spell-resolution.md) support those statements; they do not
complete every spell, recast, natural area shape or repeated lifetime.

### Items: custody, equipment and use are separate

**Confirmed static:** battle Use delegates the item's defined spell/level to the cast-spell route;
possessing an item does not by itself establish its battle-use eligibility. AI eligibility is a
further filter, separate from player ownership. The post-use builder consumes non-equipment;
equipment enters the break route only if breakable and used by an ally. Already-broken equipment is
destroyed; fresh breakable equipment breaks on the owning random gate's zero result. This does not
supply a universal player-facing break probability. See
[item use and break routing](../../contracts/battle/battle-action-construction.md#item-use-and-break-routing).

Give/equip/drop operate on custody and equipment state through their own branches. Equipped cursed
items block give/drop and the exchange route. Transfer clears the local equipped bit before the
add-item request; a full recipient inventory selects trading, completed Give selects STAY, and Drop
requires confirmation with a separate rare-item Deals request. The
[control owner](../../contracts/battle/battle-functions-control-flow.md#equipment-items-and-chests) establishes
those local branches and requests, not completed inventory/economy transactions. Use, transfer,
equipped eligibility and post-use destruction must not be collapsed into one generic item effect.

## Action Commit, Construction, and Replay

**Confirmed static:** an ordinary committed action enters the scene builder, which clears transient
reward/action accumulators, constructs and sorts targets, then processes each target's effect and
item-drop seam. Actor idle, used-item handling and follow-up validation occur afterward, with special
re-entry for Burst Rock. Selection order, construction order and rendered order have different proof
boundaries. The [action contract](../../contracts/battle/battle-action-construction.md#top-level-action-pipeline)
owns the exact sequence and special cases.

**Confirmed in the physical/spell replay subsets:** the original can calculate against temporary
state, construct reactions and restore saved HP before executing commands. That restoration is not
healing. The scene interpreter then applies ordered HP/MP/status/reward commands to persistent
state. Some effects write status during construction, others during playback; no single rule that
"all mutation waits for animation" covers them. A scene/action request also does not prove its text,
input wait, graphics, audio or cleanup completed. The
[scene engine](../../../research/battle/battle-scene-engine.md#confirmed-engine-boundary) records static
command dispatch; actual completion requires the named observation/consumer boundary.

**Confirmed static continuation:** after ordinary scene execution, turn control requests scene end
and battlefield reload before returning to the controller suffix. That request sequence does not
establish complete rendering or reload effects for every caller.

### Follow one failed cast across the boundaries

In the controlled **BLAZE 1 under SILENCE** case, a supplied committed cast reaches resolution with
20 MP and a spell carrying `AFFECTEDBYSILENCE`. Construction queues the 2 MP cost before taking the
silenced branch. It emits the silenced-message command, creates no target effect and awards no EXP.
At construction return the caster still has 20 MP; playback applies the cost and leaves 18 MP, with the
target unchanged. **Confirmed bounded original observation:** failure of the effect does not cancel
the committed cost. Under the same status, the separately observed unmarked SPOIT spell executes;
SILENCE is a property-dependent gate, not a block on all magic.

This example connects commitment, spell properties, command construction, persistent resources and
failure without assuming a naturally learned Bowie spell or complete presentation. It is different
from cancelling before commitment or receiving no target. The
[SILENCE cost-order owner](../../contracts/battle/spell-resolution.md#confirmed-silence-cast-gate-and-cost-order)
and [`sf2-silenced-caster-blocks-blaze1-v1`](../../../../tests/fixtures/h3/spell-silence-gate-v1.json)
retain the exact controlled setup and assertions.

### Rewards become persistent through their own routes

**Confirmed for the combat/spell reward subsets:** action effects accumulate EXP/gold, with caps,
effective-level comparisons, target-side/battle adjustments and ordered random award adjustments
where applicable. Healing/status rewards have their own conditions; damage, successful status,
kill and final command award are different quantities. Battle01's EXP reduction is not a universal
gold reduction. EXP command replay can cross a level threshold and refresh growth without healing
current HP/MP. A kill's item-drop lookup, one-time flag, removal and recipient/Deals routing are
separate from both damage and gold award. Full inventory can therefore affect item delivery without
undoing an already resolved kill. Exact arithmetic and bounded delivery observations remain in
[combat rewards](../../contracts/battle/combat-resolution.md#9-award-confirmed-battle-01-exp) and
[spell rewards](../../contracts/battle/spell-resolution.md); wider growth/economy context belongs to
[Progression and Economy](progression-and-economy.md).

## Post-Action, After-Turn, and Outcome

### Terminal checks surround after-turn effects

**Confirmed static controller order:**

1. Run the defeated-combatant program hook, process killed combatants and count remaining factions.
2. If nonterminal, process the acting combatant's after-turn effects.
3. Process killed combatants and count factions again, without a second defeated-program hook.
4. If still nonterminal, advance to the next scheduled entry; the terminator starts a new round.

HP reaching zero and later dead-list cleanup are distinct steps. Cleanup records applicable
kill/defeat credit, removes listed combatants from battlefield positions, then clears status and
refreshes stats. The count requires placement and positive HP. Bowie's zero HP forces the ally count
to zero; zero
allies selects defeat **before** zero enemies selects victory. Defeating the last enemy therefore
does not override simultaneous leader loss. Enemy-leader cleanup can alter remaining enemies through
a battle-specific hook, so "kill every initially placed enemy" is not a universal victory recipe.
The [lifecycle contract](../../contracts/battle/battle-control-lifecycle.md#post-action-and-after-turn-order)
owns the common order; the later accepted
[terminal-priority inspection](../../../research/chapters/final-confrontations-ending/battle43-outcomes-and-ending-admission.md#death-hook-leader-sweep-and-terminal-priority)
checks the actual shared controller and its encounter-specific hook separately.

**Confirmed bounded after-turn observation:** MUDDLE and SILENCE can expire or decrement through
their random counter rules; SLOW, ATTACK and BOOST decrement deterministically at the observed step.
Expiry messages and final derived-stat/equipment normalization have an order of their own. These
one-step cases do not establish a full natural multi-round lifetime or every ailment. The first
terminal check matters: an action that already ends battle does not continue through an otherwise
scheduled after-turn pass. See the
[status transition matrix owner](../../contracts/battle/spell-resolution.md#confirmed-after-turn-counter-transition-matrix).

### Victory, defeat and admitted retreat leave different state

**Confirmed static common outcome rules:** victory heals the eligible party, requests the
after-battle program and, when that path returns, clears the battle's unlocked flag, sets its
completed flag and returns `D4=1`. That program can have first/repeat gates and encounter-specific
mutations. A completed flag is distinct from an enemy death, a requested aftermath or the ability
to act on a returned map.

Ordinary defeat updates repeat/unlocked state, restores Bowie's HP, halves gold with integer
floor division, obtains the egress position and returns `D4=-1`. It does not perform victory's new
completion/after-program sequence or promise full-party revival. The
[Battle4 special-loss guide](../chapters/grans-opening/battle04-outcomes-through-battle05.md) explains its hardcoded completion/
upgrade route and zero result **after the loss costs**. A zero return is not synonymous with retreat,
free recovery or a forced-loss story requirement.

An **admitted EGRESS or Angel Wing action** exits before ordinary scene construction and before the
post-action terminal suffix. The spell path requests its defined MP cost; the item path requests
removal. The shared exit requests window closure, updates repeat/unlocked state, obtains egress
parameters and returns `D4=0`, bypassing defeat's HP/gold and victory's new completion suffixes.
Admission still requires applicable actionable-actor, spell/item selection and payment conditions;
cancel, no target or insufficient MP does not admit retreat. A menu label alone grants no
unconditional escape. The [Battle0 admission inspection](../../../research/chapters/final-confrontations-ending/battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
retains those ordinary selection/payment gates even under its special caller. The
[local control contract](../../contracts/battle/battle-functions-control-flow.md#egress-and-angel-wing)
proves request order and the local code, while later
[outcome/callee inspection](../../../research/chapters/final-confrontations-ending/battle43-outcomes-and-ending-admission.md#victory-defeat-retreat-and-repeated-completion)
qualifies the actual cost, repeat state and live egress source. Caller-specific destinations remain
with their chapter owners; they are not all one fixed town or savepoint.

### Returning from battle is not yet usable exploration

The caller consumes the result and destination state. It can request a map switch, run map setup or
continue another program before field input becomes available. **Confirmed bounded original
observation:** the selected Battle01 lineage reaches victory, the full after-program, flag changes,
map return and stable field readiness; its separate ordinary movement extension proves one usable
input/effect boundary. That is stronger than `D4=1`, and narrower than every campaign return.

There is also a caller exception: hidden **Battle0** returns to EndGame. On every actual victory,
defeat or admitted-retreat return, that caller discards the returned parameters and takes the
software-reset transfer rather than routing to exploration. Its Suspend choice returns to the menu
without saving or leaving battle. Actual hardware/SRAM retention and the post-reset menu remain
**Unknown**. [Story Progression](story-progression.md) and the
[Battle0 reset owner](../../../research/chapters/final-confrontations-ending/battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
retain that exception; this common-rule explanation does not extend its encounter profile.

Elsewhere the normal suspend route copies elapsed seconds, sets flag 88, requests SaveGame and
transfers toward WitchSuspend. Those source operations do not prove successful durable saving or
restoration. Suspend, an outcome return and a usable map return remain separate claims. Read
[save-system boundaries](../../contracts/system/save-system.md) for persistence rather than assuming every
battle exit saves the force.

## How Encounter Rules Change the Common Loop

An encounter can change who is present, who controls a turn, which effects are admitted, what ends
battle and what the caller does afterward. **Inferred design connection:** these are independent
axes of the common loop. A boss appearance, an activated region or a completion flag cannot stand
in for all of them. The five comparisons below explain distinct rules, rather than rank difficulty
or prescribe a party. **Confirmed static** applies to their conditions and source-defined
consequences; none is a newly observed complete encounter or a production-support claim.

| Encounter | Distinctive condition | Consequence for the shared loop |
| --- | --- | --- |
| [Taros / Battle18](#equipment-can-gate-an-effect-taros) | Admission and Bowie's equipped Achilles attack are separate tests. | Entering battle does not make an attack effective; conditional loot and acquired transport have later, independent gates. |
| [Jaro / Battle32](#a-side-change-is-not-manual-control-jaro) | A region scene converts an enemy into an AI-controlled ally; first victory has another recruitment scene. | Position, side, scheduling, control and lasting party state can change at different points. |
| [Prism Flowers / Battle36](#a-special-action-replaces-ordinary-ai-prism-flowers) | Flower kind and laser direction select a dedicated path before ordinary commands. | An ally on the ray can trigger a shot whose later hit list also contains enemies; the Flowers are not the leader objective. |
| [Fairy Woods / Battle44](#a-repeat-trial-adds-result-and-retry-state-fairy-woods) | Optional admission, hidden/reappearing enemies and a separate result/record flow. | A leader-based terminal rule coexists with a timed challenge; a better record is unnecessary for another attempt. |
| [Hidden Battle0](#the-caller-changes-the-destination-hidden-battle0) | EndGame admits the encounter and consumes every actual battle return. | Zeon has no leader sweep here; victory, defeat and admitted retreat all reach software reset. |

### Equipment can gate an effect: Taros

**Confirmed static:** Battle18 admission tests its unlock, not Sword custody or equipment. The
[Taros/Caravan chapter example](story-progression.md#worked-example-taros-and-the-caravan) explains
how the exploration choices can supply that unlock. Fresh combat then places the actual joined,
active party using formation coordinates; the twelve printed ally templates do not recruit twelve
healers or replace the party's gear and AI. Taros's special sprite is presentation mapping. With
completion clear, the neutral Caravan has zero HP and does not count as a remaining enemy.
Neither a visible Caravan nor Rohde's follower role means transport has been acquired. See the
[roster and neutral owner](../../../research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md#twelve-templates-and-the-actual-party).

At initial action-script construction in Battle18, the effective conjunction is **Bowie + ATTACK +
a target with Taros's enemy definition + an actually equipped Achilles Sword**. Carrying it without
equipping it, another ally attacking with it, or Bowie choosing spell/item use does not satisfy that
conjunction. For example, Bowie's attack while merely carrying the Sword sets the ineffective toggle;
when its Taros damage helper is reached, it requests the ineffective message and returns before HP
loss and damage/kill rewards. An eligible attack still needs the later hit/damage path. The
[equipment-gate owner](../../../research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md#bowies-equipped-achilles-gate-and-its-lifetime)
bounds this initial test and its lifetime: it is not a claim of immunity to every possible effect,
every later target or every counter path.

The terminal rule is separate again. With Bowie alive and enemy128/Taros at zero HP, the defeated
hook can queue the remaining living enemies for cleanup; returning cleanup can remove them from
the count. The common defeat-first priority still applies, with no second defeated hook in the
after-turn suffix. This is not a requirement to defeat every initial enemy individually. Taros
Sword loot instead needs its own action-side death, actual held item, successful random gate and
clear once-bit. A living recipient with room receives it; zero HP or full inventory takes the
rare-item Deals fallback. Leader cleanup does not manufacture a successful item-drop transaction; the
[loot owner](../../../research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md#taros-sword-action-drop-custody)
keeps those conditions distinct from victory.

First victory with completion clear runs the acquisition scene: its reached terminal writes grant
Caravan acquisition and clear Rohde's ordinary following flag, without joining Rohde to the force.
Repeat victory skips that scene and preserves the supplied acquisition state; it does not repair a
missing Caravan flag. Victory returns a shrine-map seed taken before the scene, while defeat and
admitted retreat use the supplied savepoint and their different costs. Neither loss nor retreat
runs the acquisition scene. Later boarding has its own traversal conditions. The
[outcome/acquisition guide](../chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md) owns that handoff and the
warning that may still precede shrine exploration; victory alone does not promise usable transport
or immediate field control.

### A side change is not manual control: Jaro

**Confirmed static:** Battle32's two region conditions can admit the same Jaro scene. A held region
flag and clear shared played flag F386 are required; fresh placement alone does not prove either
region was reached. The wrapper sets F386 **before** running the scene, and ordinary fresh entry
does not clear it. If the scene never reaches conversion, or is interrupted after that flag write,
a later call with F386 held skips it. This is a retained-state boundary, not guaranteed completion
or an automatic retry. The [region admission owner](../../../research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md#region-hook-admission-and-complete-jaro-scene)
keeps geometry, activation and scene history separate.

When reached, conversion takes enemy143's coordinates, zeros its current HP and removes its battle
coordinates, then positions ally Jaro23 there. It sets joined/active membership and copies the
physical actor lookup, without copying enemy HP/stats, healing Jaro or allocating a new sprite.
The explicit active-party write can exceed the ordinary twelve-member capacity; it is not proof
of a general capacity-safe recruitment rule. The scene's later nonzero `joinForceAI` operand sets
AI-control bit2. A visible Jaro, a converted ally and a manually controlled turn are therefore
different facts. The [logical/physical conversion owner](../../../research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md#logical-conversion-membership-and-physical-aliases)
retains the precise prefix and interruption limits.

Scheduling then tests Jaro's **own** placement, HP and current agility. A dead ally Jaro is not
revived by conversion and contributes no turn; the former enemy's agility is not inherited.
If eligible and routed through his new AI bit, he takes ally AI rather than ordinary manual input.
No fixed first actor or one-for-one schedule replacement follows. Already-joined Jaro also causes
the fresh enemy initializer to skip143 **without clearing its old fields**; if a conversion is
subsequently admitted, it still reads that retained slot. The
[initialization owner](../../../research/chapters/moun-nazca-grans/battle32-initialization-and-opening.md#jaro-joined-list-empty-list-and-skipped-old-slot)
and [queue owner](../../../research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md#eligibility-agirng-and-first-call)
bound these changed premises instead of assuming the normal-looking scene repaired them.

First victory's later scene can recruit Jaro even when the region conversion never occurred.
Its reached suffix joins him, clears only the AI-control bit, removes him from the active battle
party and requests following: joined membership survives, while active membership is clear. This
permits future party selection; it does not leave him automatically active for the next battle.
Already-held completion skips the whole scene, including those repairs, and the shared join tail
is Bowie's. Defeat/admitted retreat do not run the victory recruitment. See the
[outcome/party owner](../../../research/chapters/moun-nazca-grans/battle32-outcomes-and-moun-entry.md#physical-population-aliases-and-logical-jaro-state)
and the [Moun chapter](story-progression.md#jaros-defection-frayjas-joining-and-mouns-survivors)
for the separate onward story conditions. A completed cutscene, a follower and permanent force
membership must not be inferred from one another.

### A special action replaces ordinary AI: Prism Flowers

**Confirmed static:** Battle36 fresh initialization places twenty STARTING enemies, including eight
Prism Flowers. All ordinary spawn candidates are absent through the unchanged mode scan; that does
not exclude later event/debug requests or changed spawn modes. Three regions can affect other
enemies, but no Battle36 region scene exists in the selected table. Placed living enemies can be
scheduled before activation, and first dispatch still depends on actual party, current agility and
RNG. The [initialization owner](../../../research/chapters/moun-nazca-grans/battle36-initialization-and-opening.md#map-full-formation-and-definitions)
and [activation/spawn owner](../../../research/chapters/moun-nazca-grans/battle36-activation-and-first-turn-dispatch.md#complete-activation-rows-and-normal-spawn-candidates)
separate those declarations from executed control.

For a living Flower actually routed to AI, its unchanged 15/15 trigger pair bypasses the ordinary
inactive-enemy standby gate. The kind93 branch then selects line-attacker AI **before** ordinary
commandset execution. HEALER1, zero MOV and a LASER spell-list entry do not implement this rule.
Changed non15 triggers with no active bit can instead select standby; opponent player control can
bypass AI preparation. Sleep/Stun can also stop later action execution after preparation. The
[special control owner](../../../research/chapters/moun-nazca-grans/battle36-activation-and-first-turn-dispatch.md#first-individual-turn-control-and-the-flower-kind-branch)
explains why no single placement or AI label guarantees a shot.

The laser's direction comes from the battle/ordinal table, independently of display facing. From
the actor's current position it scans a one-square-wide cardinal ray in fixed 48-by-48 tactical
storage, without testing walls, ordinary spell range or the narrower battle crop. The AI first inserts
eligible living, positioned, non-neutral **allies** into occupancy. With Flower130 at `(6,11)`, an
otherwise lone ally at `(8,12)` misses its rightward ray: AI chooses STAY without searching another
direction or repositioning. Move that ally to `(8,11)` and the ray admits action6. These are
**Confirmed static supplied-state deductions**, not recorded natural turns; unchanged initial party
placements miss all eight rays, but earlier actors can change the first actually reached decision.

The selected first ally serves later targeting focus; it is not the final damage list. Action
construction rebuilds occupancy from **both factions**, collects the current ray and sorts by
combatant ID. An enemy at `(10,11)` can therefore be hit along with the triggering ally. If all
targets have left or died by reconstruction, action6 can retain its message/animation requests
with no target effects, rather than becoming a fresh STAY. For ordinary valid targets, the laser
uses base power 16 and two ordinary variance draws, yielding 12–16 damage before the HP clamp.
It does not use ATT/DEF, terrain protection, ordinary spell resistance or an MP charge. Forced debug inputs
have separate exceptions. Temporary HP calculation, restored construction state and later reaction
playback remain distinct. The [laser owner](../../../research/chapters/moun-nazca-grans/battle36-prism-flower-laser.md#occupancy-decision-and-changed-state-examples)
connects ray, reconstruction and damage, without proving completed animation or a played round.

The Flowers are also distinct from the objective: enemy128 is the **Executioner**, not a Flower or
the later Geshp scene actor. His action-side death with Bowie alive can trigger the leader sweep;
destroying all eight Flowers while other enemies still count does not by itself win. First victory
runs its Geshp aftermath and returns to Map53 using the pre-scene party seed; held completion skips
the scene. The [outcome owner](../../../research/chapters/moun-nazca-grans/battle36-outcomes-and-galam-return.md#terminal-selection-leader-cleanup-and-accounting)
and [Galam-region chapter](story-progression.md#prism-flowers-red-baron-and-geshp-returns)
retain the separate reward, return and onward-admission rules. Friendly fire or leader cleanup is
not proof of an ally-earned kill/drop transaction.

### A repeat trial adds result and retry state: Fairy Woods

**Confirmed static:** the optional trial is Map24/Battle44, reached from Elven Village's east exit,
not Fairy Cave. Without the battle unlock or completion-result branch, map initialization invites
an attempt. Confirming Yes requests the start warp and sets F444; No or B requests return to the
village without starting. MainLoop checks battle admission before map initialization, so a supplied
unlock can bypass the invitation. Even held completion F544 does not reject a simultaneously
unlocked battle: selection clears the unlock but admits that invocation. The
[invitation/admission owner](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#east-exit-admission-and-invitation)
keeps the prompt, pending warp, admission and fresh battle distinct. Formation again places the
actual party; it does not select a fixed twelve-character trial team.

This encounter changes the lifetime of enemies. Region-gated HIDDEN mode permits a first appearance
only while maximum HP is zero; after successful appearance and ordinary death it does not reappear
in that unchanged trial. Literal mode3 can reappear after later death while its region stays held
and reset succeeds, at a later round prefix rather than immediately during the kill. Refusals at
the declared bound or occupied starting square preserve the common failed-reset state, including
the dead/self occupancy case explained above. Later attempts still depend on mode and HP/region
conditions; there is no unconditional spawning promise. A fresh retry reconstructs definitions/regions separately. See the
[spawn lifetime owner](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#regions-checked-bits-hidden-appearance-and-reappearance).

The invitation asks how fast all hidden monsters can be defeated, but the terminal predicate is
not an all-fifteen-kills requirement. Enemy128 is **Bubbling Ooze**, with leader sweep enabled.
Its action-side death while Bowie lives can remove the remaining living enemies through cleanup,
even if hidden entries never appeared. Defeat still has priority if both factions are exhausted;
a leader death from after-turn effects does not gain an extra defeated hook in that same suffix.
The empty hooks and record congratulations add no trial completion item prize. This
[objective/reward boundary](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#queue-control-terminal-and-rewards)
qualifies the challenge wording without assigning difficulty or an intended strategy.

Timing introduces several states, not one stopwatch. Fresh battle resets the seconds longword,
but leaves the separate frame subcounter unchanged. Every sixty enabled contextual updates advance
the seconds counter. The timer window callback copies changed seconds into a result snapshot;
closing/removing that callback can freeze the snapshot while the counter continues. Ordinary action scenes close/reopen the window,
so hiding it does not establish a clock pause. Victory heals first and then closes it; the trial
result is the last callback copy, potentially updated during the close wait, not an exact sampled
leader-kill instant. Displayed digits have their own minute clamp, unlike the dialogue formatter.
These are source counter/callback operations, **not hardware duration measurements**. The
[timer owner](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#timer-lifetime-snapshot-record-and-save-boundary)
keeps counters, visible formatting, snapshot and best record separate.

Victory returns to Map24 result initialization. Only an **unsigned strictly smaller** snapshot
updates the best record; equal/slower results leave it unchanged. That update is RAM state, with
no SaveGame call in this result branch. The record lies in the separately serialized save range,
which does not prove a completed save or durable best time. Both result branches request village
return and then clear completion. With the victory unlock also cleared, a later east-exit visit
can invite another attempt regardless of record success. The
[result/retry owner](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#victory-result-record-comparison-and-retry)
bounds this repeat route without claiming a naturally played return-and-retry cycle.

Defeat or admitted Egress/Angel Wing produces no new completion record and retains its ordinary
recovery/gold or payment rules. Its supplied savepoint is not necessarily the invitation/result's
village exit. Ordinary Suspend is allowed here, unlike Battle0; an admitted saved-state entry
resumes the saved queue/seconds instead of rebuilding a fresh trial. Actual save completion,
hardware timing and persistence remain **Unknown**. The
[loss/retreat/suspend owner](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md#defeat-escape-suspend-and-the-village-stopping-boundary)
retains those separate retry/return conditions.

### The caller changes the destination: hidden Battle0

**Confirmed static:** following the ending, EndGame clears entities, sets F400, supplies map63 and
calls the battle selector. Row0's coordinates are wildcards; held completion F500 clears the unlock
but still selects Battle0 on that call. This is the concrete special caller, not a freely entered
Magic Tunnel encounter or permission to start it from any map. The
[ending admission owner](../../../research/chapters/final-confrontations-ending/ending-credits-finale-and-bonus-admission.md#battle0-admission-and-normal-return-reset)
and [ending chapter](story-progression.md#credits-finale-completion-storage-and-the-hidden-battle)
keep the preceding completion-storage/display sequence separate from battle entry. Inherited F88
has a supplied-state resume route; the ordinary Battle0 Suspend choice cannot create it.

Boss identity does not determine the terminal rule. Although enemy128 is Zeon, Battle0's leader
presence byte is zero: his death does **not** sweep other enemies. Remaining placed, living enemies
can keep the battle running. The common Bowie-loss override and defeat-first priority still apply.
First victory requests the short game-over text, reaches the shared Bowie join tail and records
completion; held completion skips that text. Defeat instead restores Bowie and halves gold;
admitted retreat pays its spell/item cost and bypasses the ordinary victory/defeat suffixes.
Cancel, no target or insufficient MP grants no escape. Suspend is refused before its prompt,
flag write or save request and returns to the battle menu, rather than resetting the game.

Unlike the ordinary MainLoop returns illustrated above, **every actual victory, defeat or admitted
retreat return goes back to EndGame**, which ignores the result and map parameters. It requests
white fade, clears contextual VInts, disables display/interrupts, restores the initial stack and
jumps through the start vector: a software reset. A calculated retreat savepoint is therefore not
field travel under this caller. A waiting/interrupted/nonreturning service does not prove the reset
suffix ran. The [Battle0 outcome/reset owner](../../../research/chapters/final-confrontations-ending/battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
ends at that transfer; post-reset menus, new-game-plus behavior and durable retention of later HP,
gold, items or outcome flags remain **Unknown**. The earlier SRAM-mapped ending bit is not a save
of the hidden encounter's outcomes.

## Original Fidelity and Modernization

### Original knowledge and observed play

The source-backed rules above retain their static limits even when later chapter research closes a
specific caller, flag or destination. An older global statement that all outcome campaign meaning
is Unknown is now too broad: [Story Progression](story-progression.md) and
[content coverage](../../../research/story-content-coverage.md#four-different-frontiers) link accepted
chapter connections through ending/reset. Natural full-campaign play, all legal actions, full
suspend persistence and hardware presentation still remain **Unknown**. The accepted
[original frontier](../milestones/map3-battle01-readiness.md#accepted-original-frontier) does not upgrade controlled
H3 arithmetic/selection cases into natural action histories.

### Accepted remake choices

**Confirmed accepted decisions:** [ADR0019](../../../decisions/0019-state-and-content-driven-remake-engine.md)
selects the state/content-driven engine direction and its behavior-test policy. The
[modern finite-music clock](../../contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy)
separates deterministic logical work from matching actual playback completion; late host delivery
adds no logical opportunities or debt. It can change whole-history RNG, turn order and later resources.
The [modern-clock comparison policy](../../contracts/milestones/map3-battle01-continuous-scenario.md#modern-clock-applicability)
therefore compares applicable local rules at matched state while retaining mandatory route effects,
manual agency and usable return. It permits no live production reseed and does not explain away an
unresolved mismatch merely because time policy changed.

The main RNG and live thinking state remain distinct. Original text/diamond polls can overwrite the
thinking byte before AI reads it; the current modern text path binds its copy to the live thinking
image. An initial-byte overwrite does not establish the original route's last effective writer
before first AI, and captures from before the modern correction do not establish the corrected
executable's trajectory. The [randomness owner](../../contracts/system/randomness.md#initial-byte-versus-live-caller-state)
retains both the corrected mechanism and those original/consumer **Unknowns**. Neither an absent
field nor an old capture becomes zero or corrected evidence by assumption.

Evidenced player Wait, remappable input, reduced flash and adjustable/instant text also have accepted
owners. The [overview's decision route](../gameplay-overview.md#made-remake-choices-and-remaining-product-choices)
and [current milestone](../milestones/map3-battle01-readiness.md#accepted-current-milestone) define their scope;
default keyboard A and the modern-clock/7C/8D/10A composition are the current accepted boundary.
Other UI redesign, rebalance, durable user saves and broader action coverage remain separate product
choices. Describing the original does not select those choices here.

### Production support and verification

The [capability ledger](../../../../remake/docs/capability-status.md) owns what runs: common movement/
cancel, automatic rounds, selected physical/AI/reward rules, HEAL 1–3 and Medical Herb consumers,
and bounded connected outcome/return behavior. The accepted private Map3–Battle01 milestone is not
a full-game simulation. EGRESS, broader spells/items/statuses, special enemies and encounter-specific
programs retain Unsupported or narrower limits there; static knowledge does not implement them.

This synthesis specifies no reference-shaped production architecture, universal fixture loader,
new H4 row or blanket verification obligation. Original fixtures remain evidence; production rules
follow the current ADR0019 boundary and selected direct comparisons. Keep temporary calculation,
persistent mutation, consumer completion and field readiness distinguishable wherever a supported
claim needs them, without requiring the original RAM layout or command-buffer implementation.

The [retained-failure owner](../../../research/story-content-coverage.md#unknowns-retained-failures-and-next-decision)
preserves #1047's twelve completed unresolved comparisons and other original/modern failures.
They remain completed results, not passes or interrupted runs. The modern correction does not
retroactively validate old captures. English explanation also leaves existing Chinese mirrors and
known drift intact; their later synchronization follows the [roadmap](../../documentation-roadmap.md).

## Evidence Navigation and Review Boundary

Exact fixture IDs, parameters, provenance, schemas and reproduction commands live with these owners.
Use this table to move from a rule to its proof, not as a second action/fixture catalogue.

| Reader question | Owning contract and original evidence |
| --- | --- |
| Entry, activation, scheduling, cleanup and common outcomes | [Battle lifecycle](../../contracts/battle/battle-control-lifecycle.md), [battle-loop research](../../../research/battle/battle-loop.md); `sf2-battle-control-static-v1`, `sf2-battle-loop-static-v1` and named Battle01 activation/turn-order H3 cases. |
| Player/AI control, no target, cancel, item/menu and suspend requests | [Battle functions control flow](../../contracts/battle/battle-functions-control-flow.md), [shared functions research](../../../research/battle/battle-functions.md); `sf2-battle-functions-static-v1`. |
| Movement cost, range and target legality | [Battlefield navigation](../../contracts/battle/battlefield-navigation.md), [pathfinding research](../../../research/battle/battlefield-pathfinding.md); `sf2-battlefield-static-v1` and `sf2-battlefield-movement-runtime-v1`. |
| AI command/resource filters and scoring | [AI decision](../../contracts/battle/battle-ai-decision.md), [AI research](../../../research/battle/battle-ai.md); static action/healing/support/priority/movement owners and `sf2-battle-ai-action-choice-runtime-v1`. |
| Action sorting, item use and scene construction | [Action construction](../../contracts/battle/battle-action-construction.md), [action research](../../../research/battle/battle-actions.md); `sf2-battle-actions-static-v1`. |
| Damage, magic, support, rewards and persistent replay | [Physical combat](../../contracts/battle/combat-resolution.md), [spells](../../contracts/battle/spell-resolution.md), [runtime math](../../../research/battle/runtime-rng-and-battle-math.md); each contract's named H3 subsets, including the SILENCE failure example above. |
| Command requests versus presentation | [Scene engine](../../../research/battle/battle-scene-engine.md); `sf2-battle-scene-engine-static-v1`. Actual current consumers remain in the capability ledger and its linked owners. |
| A chapter's terminal priority, caller destination or special return | [Story Progression](story-progression.md) and its accepted outcome investigations; the shared terminal-priority and Battle0 owners linked above qualify older aggregate summaries. |

**Scope agreement — Standard:** cross-system dependencies require source/claim review; the bounded
outcome is readable common tactical rules and five representative encounter comparisons, with
necessary conditions and non-happy paths. Acceptance follows overview → rule/comparison → named
proof, including cancellation, ineffective/no-target, optional retry and special-caller returns,
with evidence labels, incoming anchors and original/modern/production distinctions intact. No
acquisition, exhaustive encounter catalogue, simulator, engine change, translation or new checker
is included.

The pre-synthesis review inspected relevant research, all six core contracts plus the stricter
battle-functions control contract, selected fixture payloads and their schema/verifier boundaries,
and current observation/decision/capability owners. The review record on
[Issue #1268](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1268) identifies the exact
surfaces and direct checks. Older research/fixture shorthand about completed menu, inventory,
suspend and presentation effects is read at the stricter contract's local-write/request boundary.
Older grouped runtime queues remain unresolved questions under
[static-first deferral](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md), not
new acquisition obligations. The AI fixture's older thinking "low byte" wording is qualified by the
accepted randomness owner's high-byte operand; its observations are preserved. No Layer A owner,
fixture or historical failure is silently repaired by this explanation.

The encounter comparison reviewed the selected Battle18, Battle32, Battle36, Battle44 and Battle0
research owners and their chapter contexts. It reuses their accepted source/consumer reviews;
shared action, AI, lifecycle, local-control and save contracts/fixtures establish narrower seams,
not complete encounter observations. In particular, older Taros shorthand about an "ally" attack
is read through the newer Bowie's-identity/equipped-weapon inspection, and laser "first target"
means decision/focus input rather than the reconstructed hit list. The review record on
[Issue #1269](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1269) names the selected
surfaces and dispositions. Timer storage, interrupted Jaro conversion, repeat gates and EndGame's
reset retain their explicit limits; these comparisons create no runtime-closure queue or new
production admission.
