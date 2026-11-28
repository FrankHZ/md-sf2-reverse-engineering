# Tactical Battle Loop and State Handoffs

Every Shining Force II battle revolves around three connected decisions: where a unit can move,
what action it can commit from that tile, and what resources, statuses, or objectives remain once
that action resolves.

Throughout this page, three distinct time and command levels stay separate:

- **Round (`round`):** one full battle cycle covering all eligible placed combatants.
- **Individual turn (`turn`):** one scheduled combatant's opportunity to act inside a round.
- **Action (`action`):** the specific command committed on that turn (Attack, Magic, Item, Stay,
  or Search).

## Concrete examples: positioning vs. committing a blocked spell

Before walking through the full loop, two bounded examples show how rules interact in practice:

1. **Positioning and healing windows** (worked **Inferred** example — Battle02):
   Assuming Normal difficulty and a fresh level-1 party, when the southern Huge Rat advances to
   `(10,13)`, Jaha (9 HP, Short Axe) can reach it from either a plains tile `(11,13)` or a road
   tile `(10,12)`. Both tiles deal 5 damage on an ordinary hit, but plains terrain reduces the
   Rat's ordinary hit from 5 damage to 4. If Jaha misses once and takes two hits before Sarah acts,
   standing on plains leaves him at **1 HP**—alive for Sarah's 3-MP HEAL 1—whereas standing on the
   road leaves him defeated at **0 HP**. Natural enemy targeting and movement remain **Unknown**
   (the Rat could move elsewhere or target Sarah instead). See
   [Battle02 first contact](../analyses/battle02-first-contact.md).
2. **Canceling vs. committing a silenced spell** (**Confirmed** controlled original observation):
   In the controlled **BLAZE 1 under SILENCE** case (`sf2-silenced-caster-blocks-blaze1-v1`), a
   caster with 20 MP commits BLAZE 1 (2 MP, flagged `AFFECTEDBYSILENCE`). Unlike canceling before
   confirmation (which spends 0 MP), committing the silenced cast still deducts 2 MP during
   playback (`20 → 18 MP`) while producing no damage and no EXP. Under the same SILENCE status, an
   unmarked SPOIT cast still executes—SILENCE checks the spell's property rather than blocking all
   magic. See [SILENCE cost order](../../contracts/battle/spell-resolution.md#confirmed-silence-cast-gate-and-cost-order).

## Entering battle and building a round

### Fresh entry vs. suspended resume

Starting a fresh battle clears elapsed seconds, runs any admitted before-battle scene, clears
battle-region flags, heals eligible living allies (plus the Peter/Lemon exceptions, keeping the
persistent status mask—not reviving ordinary fallen members), builds ally and enemy formations,
clears AI memory, loads the battlefield, and runs any admitted battle-start scene.

Resuming from an original suspended battle instead restores saved elapsed seconds, clears the
suspend flag and AI memory, reloads battle state, and re-enters the individual-turn loop without
re-running fresh setup or pre-battle healing. Cross-process original SRAM suspend survival remains
**Unknown**; remake battle Save/Continue follows its own [snapshot boundary](../../../../remake/docs/application/battle-save-and-continue.md).

### Round setup, spawning, and turn order

Each new round activates eligible enemies, runs any triggered region script, checks hidden or
respawning enemies, and generates that round's turn order.

- **Placement, activation, and scheduling are separate:** an enemy can be placed and living (and
  therefore enter the turn queue) while still inactive, taking its standby route until activated.
- **Spawn refusals:** hidden or respawning enemies are refused if out of bounds or if their
  starting tile matches any stored ally or enemy coordinates—including dead units or the candidate
  itself. Refusal leaves the unit unplaced at 0 max HP; later retries still depend on spawn mode,
  region flags, and max-HP gates. Natural occurrence of these refusals remains **Unknown**.
- **Turn order:** only placed, living combatants enter the queue. Agility plus ordered random
  adjustments determine the score (equal scores keep insertion order), and a high agility bit can
  schedule a second individual turn in the same round. Higher nominal agility alone therefore does
  not guarantee acting first.

### Who controls an individual turn

Dead combatants in the queue are skipped. For living combatants, **faction membership and control
ownership are separate**: an ally can be routed to AI by auto-battle, an AI-control flag, or
MUDDLE, while the opponent-control toggle gives the player enemy control.

- **MUDDLE 1 vs. MUDDLE 2:** a nonzero MUDDLE counter hands control to AI; only when combined with
  the level-2 confusion flag does it enable side-flipped targeting (which still enforces protected
  same-side and self-target guards).
- **Skipped scenes:** SLEEP, STUN, and committed STAY consume the individual turn without playing
  an action scene, but still run end-of-turn checks afterward.

## Moving, targeting, and choosing an action

### Movement range vs. attack and spell range

Movement spends a terrain-weighted budget (current MOV against the unit's movement table), blocked
by impassable terrain or occupying units. Action range is a separate check—usually Manhattan rings
defined by the equipped weapon, unarmed rule, or spell. Standing on a reachable tile does not
automatically put a target in range, and ranged actions can target tiles the actor cannot occupy.

Ordinary target lists admit placed, living, non-neutral combatants matching the action's target
side. Notable exceptions: **AURA 4** and **SHINE** target an entire faction regardless of ordinary
range rings, and **Burst Rock** affects both factions while excluding its center tile.

### Player choice, cancellation, and commitment

On a player-controlled turn, the actor moves provisionally and chooses Attack, Magic, Item, or
Stay/Search:

- **Cancellation is free before commitment:** backing out of target selection or canceling movement
  restores the actor's turn-start position and leaves no action committed (spending no MP or item).
  Choosing Attack with no valid target does not manufacture a target. Original nested-menu timing
  remains **Unknown**.
- **STAY is a committed action:** unlike Cancel, STAY spends the individual turn and proceeds
  through post-action and end-of-turn checks.
- **Search in battle:** handles chests separately—a trap triggers the trapped-chest route, an
  ordinary successful pickup commits STAY, and a full inventory closes the chest and returns to the
  menu.

### How AI chooses an action

AI-controlled allies use commandset 6; enemies use their assigned commandsets, testing commands in
order until one succeeds. Inactive enemies use standby.

- AI checks resources and legal positions before committing: ordinary healing requires an
  affordable spell (at least 3 MP for HEAL, 7 MP for AURA) and a living same-side ally at or below
  two-thirds HP, or falls back to an eligible healing item.
- AI scores candidate moves using estimates (ATT/DEF/terrain or spell power/resistance) rather than
  running the final combat resolver; an AI STAY result can still include movement.

## What a committed action changes

### Physical attacks

Dodge is rolled before damage; a dodge skips damage entirely. Otherwise current ATT, DEF, terrain
land effect, and eligible anti-air bonuses feed integer damage stages, followed by critical hits
and downward variance where admitted, clamping HP at 0.

A first hit can trigger a double attack and then a counterattack, but neither is guaranteed: a
lethal hit stops follow-ups, double attacks reject muddled/same-side cases, and counterattacks have
their own range, sleep, stun, same-side, and special-enemy exclusions.

### Magic, healing, and support

Damaging spells combine spell power with consumer-specific resistance, promoted-power scaling,
criticals, variance, and summon target-count division where applicable. Resistance setting 3 means
**weakness** for damage spells but **immunity** for status consumers. Healing is capped by missing
HP and does not revive fallen allies.

Status and support spells do **not** share one uniform rule:

- **SLOW and BOOST** separate counter updates from derived-stat refreshes: a failed recast can
  update the counter while leaving previous effective stats in place until the next refresh.
  Original natural SLOW 2 recast lifetimes and reloading with an unrefreshed recast remain
  **Unknown**; the remake preserves the unrefreshed operand across battle Continue.
- **SLEEP, MUDDLE, SILENCE, DISPEL, DETOX, and DESOUL** each have distinct resistance, target,
  and cure/effect gates (see [spell resolution](../../contracts/battle/spell-resolution.md)).

### Items: custody, equipment, and use are separate

Carrying an item, equipping it, and using it in battle are three separate checks:

- **Using items:** battle Use invokes the item's linked spell effect without spending MP. Ordinary
  consumables are removed on commit; breakable equipment used by an ally can become broken (and
  already-broken equipment is destroyed).
- **Recovery consumables:** Healing Water (single ally) and Healing Rain (user-centered, living
  placed allies in slot order) restore full missing HP; Fairy Tear restores up to 20 MP to one
  ally. Committing a valid Item action consumes the slot even when actual recovery is 0 (original
  full natural play and suspend recovery for these items remain **Unknown**; remake supports them
  through its admitted campaign range). See
  [recovery consumables](../../../../remake/docs/domain/recovery-items-design.md).
- **Give, Equip, and Drop:** equipped cursed items block Give, Drop, and full-pack exchange;
  completing Give commits STAY; dropping a rare item requests Deals storage.

## Action commit, construction, and replay

Once an action commits, the engine builds target effects and rewards before replaying persistent
HP, MP, status, and reward commands. Some calculations temporarily read intermediate HP and restore
saved HP before playback (not healing), while some status mutations happen at construction and
others during playback—there is no single rule that all state changes wait for animation.

### Rewards become persistent through their own routes

EXP, gold, and item drops settle through separate routes rather than one combined transaction.
Battle01's EXP reduction does not halve gold; EXP replay can cross one 100-point level-up threshold
and raise max HP/MP without healing current HP/MP; and a kill's item-drop lookup can route a rare
item to Deals (or discard a non-rare item) on a full pack without undoing the kill. See
[Progression and Economy](progression-and-economy.md).

## Post-action, after-turn, and outcome

Every committed action follows a fixed four-step controller suffix:

1. Run any defeated-combatant hook, clean up killed units (recording kill/defeat credit, removing
   them from the board, clearing status, and refreshing stats), and count remaining factions.
2. If the battle has not ended, process the acting combatant's **after-turn effects**.
3. Clean up any newly killed units and recount factions (without a second defeated-unit hook).
4. If still ongoing, advance to the next scheduled turn; reaching the queue end starts a new round.

**After-turn status lifetimes differ by status**—never treat them as one shared countdown:

- **SLOW, ATTACK, and BOOST** decrement deterministically at the observed after-turn step.
- **MUDDLE and SILENCE** expire or decrement via their own randomized counter rules.
- **SLEEP** uses separate randomized wake/continue rules; poison damage and equipped turn recovery
  (such as Holy Staff / Life Ring) follow their own ordered steps.

**Terminal priority:** Bowie reaching 0 HP forces the ally count to 0, and **defeat is checked
before victory**. Killing the last enemy while Bowie also falls still results in defeat.

### Victory, defeat, and admitted retreat leave different state

| Outcome | What changes | What does not happen |
| --- | --- | --- |
| **Victory** | Heals eligible allies, runs any after-battle scene, clears the battle unlock, and sets completion. | Leader-sweep battles do not require killing every enemy individually; returning from battle does not automatically save. |
| **Ordinary defeat** | Restores Bowie's HP, halves gold (floor division), updates repeat/unlock state, and returns to the egress town. | Does not revive other fallen allies or mark victory (Battle04's special loss applies loss costs first, then runs its scripted progression). |
| **Admitted retreat** (EGRESS or Angel Wing) | Spends spell MP or consumes the item and returns to the egress town without halving gold. | Does not mark victory; canceling, lacking MP, or having no valid target does not retreat. |

Returning to a map still passes through caller setup and any post-battle scenes before field
movement resumes. Normal battle Suspend saves elapsed seconds and sets flag 88 toward the Witch
suspend flow (original durable SRAM recovery remains **Unknown**).

## How encounter rules change the common loop

Encounters can override individual axes of the loop—equipment gates, side changes, special AI,
timed retries, or special callers—without changing the shared framework:

| Encounter | Rule exception | Impact on player choices |
| --- | --- | --- |
| **Taros (Battle18)** | Bowie must actually **equip** the Achilles Sword for his Attack to pass Taros's gate. | Carrying the sword or letting another ally attack is ineffective; Taros Sword loot has its own drop RNG/once-flag; first victory grants Caravan (boarding is separate). |
| **Jaro (Battle32)** | Stepping into either trigger region converts enemy Jaro into an **AI-controlled** ally (can exceed 12 active members) using his own ally stats/HP. | Scene flag F386 sets before conversion (interruption skips retry); first victory recruits him permanently into reserve and clears AI control. |
| **Prism Flowers (Battle36)** | Flower AI checks a fixed cardinal laser ray for allies before ordinary commands, then hits **both factions** on that ray (12–16 damage, ignoring ATT/DEF/terrain/MP). | Allies off the ray make the Flower Stay; objective is the Executioner (enemy128), not destroying all eight Flowers. |
| **Fairy Woods (Battle44)** | Optional timed trial from Elven Village's east exit with HIDDEN vs. reappearing enemies and a RAM best-time snapshot. | Defeating Bubbling Ooze (leader sweep) ends the trial without spawning every hidden enemy; strictly faster times update RAM only (no auto-save); clearing completion lets players retry; Suspend is allowed. |
| **Hidden Battle0** | Entered from EndGame after credits; Zeon has no leader sweep and Suspend is refused. | Remaining enemies continue the fight after Zeon falls; every victory, defeat, or admitted retreat runs software reset (post-reset durable retention remains **Unknown**). |

### Equipment can gate an effect: Taros

In Battle18, entering the fight tests the battle unlock, not sword custody. At attack construction,
only the exact conjunction **Bowie + Attack + Taros + equipped Achilles Sword** passes the gate;
merely carrying the sword triggers the ineffective branch with no HP loss or damage/kill reward.
First victory runs the Caravan acquisition scene (leaving Rohde unjoined), while repeat victory
skips it. See [Taros and the Caravan](story-progression.md#worked-example-taros-and-the-caravan)
and [Battle18 research](../../../research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md).

## Original fidelity, remake choices, and current coverage

- **Original evidence vs. natural play:** static rules and controlled runtime seams support the
  loop above; full natural playthroughs, every AI edge, hardware timing, and original SRAM suspend
  durability remain **Unknown** outside named observations (such as the accepted
  [Map3–Battle01 original frontier](../milestones/map3-battle01-readiness.md#accepted-original-frontier)).
- **Accepted remake modernizations:** deterministic state/content architecture
  ([ADR 0019](../../../decisions/0019-state-and-content-driven-remake-engine.md)), finite music
  clock ([music-wait policy](../../contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy),
  which can diverge whole-run RNG from original hardware timing), live thinking-RNG binding,
  remappable input, battle inspection and on-demand map, adjustable battle speed, held
  acceleration, and settled player-Movement battle Save/Continue.
- **Current playable support:** see [capability status](../../../../remake/docs/capability-status.md#campaign-feature-trace)
  and [retained comparisons](../../../../remake/docs/evidence/retained-comparisons.md) for what
  runs today versus unsupported late mechanics (`#1370`) and preserved historical failures.

## Basis and evidence map

| Topic | Canonical contract & original evidence owners |
| --- | --- |
| Entry, round setup, turn order, cleanup & outcomes | [Battle lifecycle](../../contracts/battle/battle-control-lifecycle.md), [battle-loop research](../../../research/battle/battle-loop.md) |
| Player/AI control, cancel, chests, items & retreat | [Battle functions control flow](../../contracts/battle/battle-functions-control-flow.md), [shared functions](../../../research/battle/battle-functions.md) |
| Movement cost, ranges & target lists | [Battlefield navigation](../../contracts/battle/battlefield-navigation.md), [pathfinding](../../../research/battle/battlefield-pathfinding.md) |
| AI commandsets, resource filters & scoring | [AI decision](../../contracts/battle/battle-ai-decision.md), [AI research](../../../research/battle/battle-ai.md) |
| Action pipeline, item break & replay | [Action construction](../../contracts/battle/battle-action-construction.md), [scene engine](../../../research/battle/battle-scene-engine.md) |
| Combat math, spells, statuses & rewards | [Physical combat](../../contracts/battle/combat-resolution.md), [spell resolution](../../contracts/battle/spell-resolution.md), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md) |
| Encounter exceptions (Taros, Jaro, Prism Flowers, Fairy Woods, Battle0) | [Story Progression](story-progression.md), [Battle18](../../../research/chapters/southern-travel-creed/battle18-initialization-and-first-control.md), [Battle32](../../../research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md), [Battle36](../../../research/chapters/moun-nazca-grans/battle36-prism-flower-laser.md), [Battle44](../../../research/chapters/eastern-parmecia/battle44-fairy-woods-trial-and-records.md), [Battle0](../../../research/chapters/final-confrontations-ending/battle00-bonus-battle-and-reset.md) |
