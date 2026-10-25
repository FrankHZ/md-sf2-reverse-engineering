# Progression and Economy

- Status: **Confirmed** party/development rules at the static and bounded observed seams below,
  with **Inferred** connections to later choices and explicit **Unknown** campaign and lifecycle limits.
- Audience: readers following the [Gameplay Overview](gameplay-overview.md#the-force-membership-active-roles-and-character-growth),
  researchers, fidelity implementers and designers.
- Scope: explain membership, class/promotion, equipment, learned magic and reward-driven growth;
  retain bounded resource/service material for the subsequent resource explanation. This is not
  all-roster coverage, a campaign economy, milestone 3 completion or Chinese readiness.

## Reading Route and Current Agreement

The [roadmap's Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
controls this English synthesis. Cross-system rules need source and claim reconciliation, while the
existing thematic owner is sufficient. Start with [party development](#party-development), follow
[battle-driven progression](#battle-driven-progression) into the
[worked example](#worked-example-growth-changes-preparation-without-healing), then use the
[evidence matrix](#evidence-matrix) for exact rules and fixtures. The
[resource/service sections](#gold-and-item-economy-boundaries) retain an interim boundary; detailed
shopping, custody, storage and service-choice synthesis belongs to the sequential resource outcome.
[Story Progression](story-progression.md#read-story-content-in-segments) owns where recruitment and
service opportunities connect to the world; [Tactical Battle Loop](tactical-battle-loop.md) owns
whether a prepared actor's selected action is legal and effective.

The outcome is to explain how a retained member's class, equipment, spells and growth become inputs
to later choices. It does not choose a best party, promotion time, balance curve, new product policy,
engine architecture or acquisition programme. Definitions and controlled observations remain separate
from natural campaign reach, presentation and current remake support.

## Judgment Boundary

This document explains how several accepted contracts connect. It does not replace the
[level-up contract](../contracts/level-up.md), [physical combat contract](../contracts/combat-resolution.md),
[spell contract](../contracts/spell-resolution.md), [service contract](../contracts/service-interactions.md), or
[save contract](../contracts/save-system.md), and it is not independent evidence about the original game.

For the listed fixtures and source-static routes, the original has distinct action-reward
construction, persistent resource mutation, threshold-driven level-up, stat refresh, item-routing
and service-call stages. Their order and units matter.

The following judgments are not supported yet:

- intended encounter difficulty, level curve, price curve, scarcity, or grinding pressure;
- a complete list of campaign income and expenditure sources;
- optimal equipment, promotion timing, party composition, or roster choice space;
- universal shop/service availability, normal-story reachability, or lifetime purchase opportunity;
- universal persistence for service mutations across map changes, save/load, or power cycles;
- player-visible reward, menu, animation, sound, or timing behavior beyond fixture-owned seams.

## Party Development

### Joining, fighting, following and being alive

A recruit can belong to the force without taking an active battle place. **Confirmed static:**
`JoinForce` sets joined membership, rebuilds the joined/active/reserve lists, then sets the active
flag only when that rebuilt active count is below the ordinary maximum. The
[membership contract](../contracts/party-membership-state.md#joinforce-chronology) owns this order;
[Creed's caller review](../../research/creed-return-fairy-and-recruitment-choices.md#join-and-capacity-are-separate-from-following)
binds the original maximum to twelve. Reserve is the joined-but-inactive classification produced by
a rebuild, rather than another independent membership flag.

The rebuild happens **before** conditional activation. Flags can therefore have changed while the
cached active/reserve lists still describe the earlier state. Read a list through its count and its
own rebuild boundary; neither stale tail bytes nor the existence of a joined flag supplies a current
battle formation. This distinction is relevant to ordinary joins, already-joined calls and explicit
active-party changes, not only one recruitment scene.

| Question | State or operation that answers it | Consequence for later choices |
| --- | --- | --- |
| Does the character belong to the force? | Joined membership. | Can be included in the joined list even when inactive or at zero HP. |
| Is the character selected for the active party? | Active flag, with counted lists rebuilt separately. | Ordinary auto-activation observes capacity; battle admission and formation still have their own consumers. |
| Can the character take this turn? | Current HP, position, status and the turn controller. | Joined or active alone does not establish a living, eligible actor. |
| Who chooses the action? | Activation/control state read by the turn dispatcher. | Ally membership does not imply player control. |
| Is someone following on the map? | Follower action and physical entity/list state. | Following alone does not join, activate, heal or initialize the character. |
| Which actor does the scene move or show? | Logical-to-physical lookup, current population and scene actions. | A scene actor can exist separately from battle selection; a lookup alias is not a stat copy. |

**Confirmed static example — Creed:** accepting an initial recruit's offer requests the join,
sets the choice flag only after that command returns, shrinks the three unselected scene actors and
adds the selected actor as a follower. Declining returns without those writes. With twelve other
active members, the recruit still joins, receives the join acknowledgement path and can follow;
the ordinary helper skips setting the new active flag and evicts nobody. HP zero likewise does not
block these commands and is not repaired by them. Later-return offers use a different branch without
the initial shrinks. These are source-confirmed branches, not evidence of a naturally completed
choice/UI session. Use [Story Progression's Creed account](story-progression.md#southern-travel-caravan-and-creed)
for the opportunity and return gates rather than assuming every recruit is always available.

**Confirmed static exception — Jaro:** the admitted Battle32 conversion calls `JoinBattleParty`
explicitly after `JoinForce`, so twelve other active members can become thirteen active flags;
the later AI command rebuild counts them. Enemy JAR is set to zero HP and off-map coordinates;
ally Jaro receives the old coordinates and physical lookup, while retaining his own HP, class,
equipment and stats. The nonzero `joinForceAI` operand sets AI control. The reached victory suffix
later requests an ordinary join, clears AI control, then leaves Jaro inactive without removing
joined membership or reviving him. The after-wrapper's later rebuild sees that cleared active flag.
The [conversion](../../research/battle32-activation-jaro-and-first-turn-dispatch.md#logical-conversion-membership-and-physical-aliases)
and [victory](../../research/battle32-outcomes-and-moun-entry.md#physical-population-aliases-and-logical-jaro-state)
owners explain the exact order and interruption branches; [Story Progression](story-progression.md#jaros-defection-frayjas-joining-and-mouns-survivors)
connects them to the campaign. This exception prevents treating twelve as a universal invariant or
recruitment as one atomic conversion from enemy to healed player-controlled follower.

### Definitions, current class and promotion

**Confirmed static:** an ally's start definition supplies class, stored level and four item bytes;
class data, base stats, spell initialization and derived-stat refresh have separate initialization
steps. The [ally-definition contract](../contracts/ally-definition-data.md) separates those records
from current state and from story availability. The trailing unnamed start slots do not establish
additional recruitable characters. A later join reads current class for its announcement; it does
not itself restore the start record, grant that definition's gear or replay initialization.

Class connects several future rule inputs: growth selects an ally/class block, learning reads its
spell list, stat refresh reads base/class values and equipped effects, and equipment consumers test
class eligibility. These connections do not make all class-derived quantities interchangeable.
Stored level remains the actor's state; an effective level is calculated by a particular consumer.
The [reward vocabulary](#resource-and-level-vocabulary) distinguishes reward comparison from spell
learning. A promoted offset in either calculation does not add twenty to stored level.

**Confirmed source-static promotion route:** the Church first needs a member in the regular-base
promotion table and stored level at least twenty. An unmapped class or a lower level takes a refusal
or reselection path before `SetClass`/`Promote`. Regular and special destinations are paired data
relations, not a numeric addition to the class ID. The selected member is checked again after the
promotable-member count; the count does not make every member eligible.

For a class with a special mapping, the source searches the current target-list members' held items
for the corresponding promotion item; possession is not restricted to the promotion candidate.
A found item offers the special path; missing or declined special choice can fall back to regular
promotion. SORC has an extra confirmation whose refusal restarts selection. On the accepted special
path, the selected holder/slot receives `RemoveItemBySlot` before the class and promotion requests.
The route then calls `SetClass` followed by `Promote`, requests SORC default-spell replacement or
MMNK/NINJ weapon removal where selected, and later requests stored level one and EXP zero.
The promotion route does not contain a gold-decrease call; Raise's price must not be reused as a
promotion price. These are ordered source requests, not a runtime-closed transaction or proof of
all resulting stats, inventory edges, prompt presentation or persistence.

The [service contract](../contracts/service-interactions.md#church-source-boundary),
[common-menu owner](../../research/common-menus.md#static-service-menu-state-machines) and its
`church.routeOperations.promote` / `helperOperations` records in
[`sf2-common-menus-static-v1`](../../../tests/fixtures/h2/common-menus-static-v1.json)
own those gates and calls. Complete `Promote` mutation remains **Unknown** at this synthesis boundary.
The supported conclusion is that class selection can change later growth/learning/equipment inputs;
there is no evidence here for an optimal promotion time or a universal final-stat formula.

### Carried gear becomes an equipped contribution

A carried item occupies inventory; its equipped state determines whether equipment refresh applies
it. **Confirmed storage:** item identity and held status are distinct from definition data. Ally
start bytes pack an equipped bit beside the item ID, while runtime inventory helpers operate on item
words with additional status such as the broken bit. The [item-definition contract](../contracts/item-definition-data.md#item-identity-and-fixed-record)
owns class-mask/type, range and effect fields as stored data, not a complete equip/use rule.

**Confirmed source consumers:** `IsWeaponOrRingEquippable` checks class mask and equipment type;
`EquipItemBySlot` checks Nothing, equippability and curse before setting equipped state, then tails
stat refresh even on rejection. Cursed-aware unequip can refuse removal of an equipped cursed item.
Thus receiving or carrying a weapon does not prove it replaces the old one, grants its effects or
makes an intended action effective. The accepted smith
[equipment continuation](../../research/dwarven-village-and-mithril-smithing.md#fulfillment-inventory-and-optional-equipment)
retains both source order and bounded observations, including a previously equipped Dark Sword
blocking replacement while the newly received weapon remains carried. Detailed fulfillment/custody
belongs to the resource explanation.

Broken and cursed describe different constraints. The
[tactical item route](tactical-battle-loop.md#items-custody-equipment-and-use-are-separate) distinguishes
fresh breakable equipment from already-broken equipment in post-use handling, and equipped-cursed
restrictions from carrying ordinary items. Do not infer that broken means unequipped, that curse is
only a definition flag, or that every item has the same use/break behavior.

**Confirmed bounded refresh:** level-up rebuilds derived ATT/DEF/AGI/MOV, resistance and prowess from
base/class state, applies status adjustments, then equipped effects. It does not stack the new gain
onto stale action-only stats. Equipped cursed definitions can reintroduce CURSE during refresh.
The [level-up owner](../contracts/level-up.md#confirmed-current-and-derived-stat-refresh) also preserves
the original `INCREASE_DOUBLE` prowess defect; this synthesis does not silently correct it. How an
updated stat or equipped identity affects an attack, range or special target remains with
[Tactical Battle Loop](tactical-battle-loop.md#equipment-can-gate-an-effect-taros) and its resolution
owners. For example, entering Taros's battle and merely carrying the Achilles Sword do not satisfy
Bowie's actual equipped-weapon gate.

### Learning adds or upgrades an option

**Confirmed static storage and helper rules:** a learned entry keeps base spell identity and a level
code, using the six-bit identity/two-bit level packing described by
[ally growth](../../research/ally-growth.md#storage-model). Spell level is separate from character
level. The accepted growth fixtures retain four ordered spell slots; learning does not append an
unbounded list of every spell level earned.
[Spellbook state](../contracts/spellbook-state.md#learnspell-chronology) keeps two searches distinct:
first find the same base identity; only if none is known, search for an empty slot.

- A higher incoming level replaces the known entry, retaining that spell's slot.
- The same or a higher already-known level refuses the incoming entry, even if another slot is empty.
- A new identity needs an empty slot; a full book returns no-room instead of choosing a spell to forget.

The result identities are static helper outcomes, not proof of a particular failure message. In
particular, full slots do not block an upgrade to an already-known identity. Learning and upgrading
must therefore be explained separately from an inventory of spell definitions.

**Confirmed bounded caller behavior:** initialization scans thresholds at or below starting effective
level before replaying earlier level-ups. Ordinary `LevelUp` increments stored level and checks an
exact matching effective threshold, not every lower threshold again. The selected ally/class growth
record supplies the list; `$FE` inherits the ally's first block's list rather than an arbitrary
previous class list. The observed Kazin/WIZ case upgrades known BLAZE in place through this inherited
list. Karna's HEAL 3 initialization special branch changes prowess before ordinary level-up replay
learns the spell; it is not a general rule that knowing healing magic grants prowess.

The original spell-learning comparison misclassifies TORT/class 11 as promoted, unlike the class-cap
boundary at 12. The observed Kiwi case changes its internal learning threshold but learns no TORT
spell. A forced missing class can scan into a later ally's block or reach the final sentinel; these
controlled boundaries do not establish ordinary promotion legality. The
[level-up contract](../contracts/level-up.md#confirmed-projection-cap-and-spell-boundaries) owns all
these defects, cap exits and observed cases. No single promoted-level predicate should replace the
separate original reward, cap and learning comparisons.

**Inferred connection to choices:** a learned or upgraded entry can supply a later magic option.
Actually casting still needs the applicable actor/turn, MP, targeting/range and effect rules. A
known spell alone proves neither affordability nor a useful result; higher maximum MP does not
refill current MP. Follow [tactical spell rules](tactical-battle-loop.md#magic-healing-and-support)
for action conditions rather than treating a learning threshold as an automatic combat effect.

## Resource and Level Vocabulary

The same everyday word can refer to different stages. A fidelity implementation and its tests must
keep these values separate.

| Term | Confirmed meaning in this synthesis | Boundary that must remain visible |
| --- | --- | --- |
| action EXP accumulator | Temporary battle-scene value built while an action resolves. Confirmed damage/kill paths saturate at 49; confirmed healing accumulation saturates at 25. | These are action-family boundaries, not one universal EXP cap for every spell or command. |
| final EXP command | The post-action amount emitted after the applicable battle-table adjustment, two ordered random checks, and the confirmed enemy-target minimum. | It is neither the accumulator nor stored EXP. Same-side and unsupported action paths must not inherit the enemy-target rule automatically. |
| stored EXP | Persistent actor byte increased by an EXP command and clamped at 200 before threshold handling. | One command subtracts at most one 100-point threshold; stored EXP may remain 100 afterward. |
| stored level | The class-local value retained on the combatant. Confirmed caps are 40 for classes below ID 12 and 99 from class ID 12 onward. | It is not automatically the level used by every comparison. |
| effective reward level | For the confirmed damage/kill bracket, a promoted actor adds 20 to stored level before comparing with target stored level. | The offset is scoped to the confirmed comparison; do not rewrite persistent level. |
| level-up spell threshold | `LevelUp` increments stored level, then applies its class-dependent effective-level rule for spell learning. | TORT's class-11 comparison defect and `$FE` spell-list inheritance remain original-fidelity facts. |
| current versus maximum/base/derived stats | Level-up changes maxima and base stats, then refreshes derived battle stats and equipment/status effects. | Current HP and MP do not increase merely because their maxima grow. |
| gold | Persistent unsigned 32-bit resource; confirmed increase behavior caps it at 9,999,999 on carry or above-cap addition. | Decrease behavior and non-battle mutation results are not covered by the gold H3 fixture. |
| held item, deals count, and caravan storage | Distinct item destinations with separate capacity and ordering rules. Deals counts are packed four-bit quantities and saturate at 15. | Static service helper order does not by itself prove persistence or final runtime state. |

## Connected Resource Flow

```mermaid
flowchart LR
    action["Resolve a confirmed battle action"] --> build["Build temporary EXP, gold, and drop state"]
    build --> finalize["Finalize reward commands"]
    finalize --> replay["Replay commands into persistent combatant and force state"]
    replay --> threshold{"Stored EXP at least 100?"}
    threshold -->|"No"| retained["Retain level and residual EXP"]
    threshold -->|"Yes, once per command"| levelup["Run one LevelUp call"]
    levelup --> refresh["Refresh base-derived stats, status, and equipment effects"]
    refresh --> future["Later actions consume the refreshed combatant state"]
    replay --> economy["Gold and item destinations become later service inputs"]
    economy --> service["Bounded shop, church, caravan, and blacksmith routes"]
    service --> unknown["Named service seams confirmed; broader lifecycle remains Unknown"]
```

The links into later-action use and later service-choice use are **Inferred system relationships**:
stronger combatant state and retained resources can affect later choices because later systems read
those fields. The diagram does not claim the original designers intended a particular power curve,
spending cadence, or optimal loop.

## Battle-Driven Progression

### 1. Build an action-local reward

**Confirmed for the named physical and attack-spell paths:** reward comparison uses the actor's
consumer-specific effective level against the target's stored level. Damage contributes a
truncated fraction of the resulting bracket, and a lethal result adds its kill contribution to the
same action accumulator. Their shared cap is distinct from the healing cap. Healing instead depends
on eligible caster class and recovered HP relative to target maximum HP; ineligible classes, enemy
actors and a zero-maximum-HP target skip the confirmed contribution. Status and special spells keep
their own bounded rules. The [combat](../contracts/combat-resolution.md) and
[spell](../contracts/spell-resolution.md) owners retain the exact brackets, formulas and caps.

### 2. Finalize the command amount

**Confirmed for the enemy-target Battle01 seam:** a battle-table adjustment halves the accumulator,
then two ordered random checks can increase and decrease the result before the applicable minimum
and EXP command emission. The accumulator therefore need not equal awarded EXP: the connected
example below finalizes 49 into a 24-point command. The
[award fixture](../../../tests/fixtures/h3/award-exp-randomization-v1.json)
owns the random branches and minimum; other battles, same-side awards and unsupported actions
retain their own boundaries. A reward adjustment is not a rewrite of stored level or stored EXP.

### 3. Replay into persistent EXP and process one threshold

The EXP command first increases stored EXP with a cap of 200. It then tests 100, subtracts that
threshold once, and calls `LevelUp` at most once. This ordering produces three important observed
boundaries:

- `75 + 24 -> 99`, with no level-up;
- `76 + 24 -> 100 -> 0`, with one level-up;
- `199 + 24 -> 200 -> 100`, still with only one level-up.

At base level 40 and promoted level 99, the command still subtracts 100 and calls `LevelUp`.
`LevelUp` returns 255, leaves the capped level unchanged, and the command leaves EXP at zero. A
remake cannot replace this sequence with a generic loop or discard-at-cap rule without recording an
intentional deviation.

### 4. Apply level-up and refresh combatant state

For a matching class block below its cap, `LevelUp` processes maximum HP, maximum MP, base ATT, base
DEF, and base AGI in that order; increments stored level; resolves the exact spell threshold; then
refreshes derived combatant state. The [level-up contract](../contracts/level-up.md) owns the random-growth
formula, post-level-30 projection rule, class-block scan, learned-spell payload, caps, clamps, and
known original defects. Growth projections are inputs to curve/random/minimum-stat rules, not a
promise of the same gain every level. Active curves and `NONE` differ, and later levels use a
post-projection branch; these rules do not establish a campaign power curve.

The connected battle fixture demonstrates why this is part of the persistent replay stage rather
than an isolated stat calculator. Bowie starts level 1 at 99 EXP, receives a 24-point command,
passes through stored EXP 123 to residual 23, reaches level 2, and refreshes base and derived stats.
Current HP/MP remain `12/8` even though maximum HP becomes 14. Action-only ATT/AGI values are replaced
by refreshed values rather than carried forward.

Promotion is a separate operation. The [promotion explanation](#definitions-current-class-and-promotion)
separates source admission, class mapping and requested mutations. Complete promotion stat mutation,
inventory edge cases, visible choice flow and persistence remain **Unknown**
at their named owner; this is not an automatic observation queue.

## Worked Example: Growth Changes Preparation Without Healing

**Confirmed bounded observations:** the connected Bowie battle fixture in the preceding section
shows the reward-to-level handoff. The separate Slade case
`slade-thif-level39-equipped-refresh` in
[`sf2-level-up-refresh-v1`](../../../tests/fixtures/h3/level-up-refresh-v1.json)
shows what the changed stats mean with ordinary equipment: THIF level 39 advances to 40, base ATT
rises from 45 to 47, and the equipped Short Knife contributes five to refreshed ATT 52. Maximum HP
rises from 42 to 44 while current HP stays seven. The item is still held/equipped; its effect is
reapplied to the new base, rather than being earned again or cumulatively added to stale ATT.

**Inferred preparation consequence:** for a later admitted physical action, the resolver can read
that refreshed ATT and the actual equipped weapon, while damage/life eligibility still reads current
HP and other action conditions. Slade remains wounded. Recovery or an applicable service is a
separate decision; the higher maximum does not solve the immediate HP constraint. If instead a new
weapon is only carried, its definition's attack value is not automatically part of that refresh.

**Confirmed non-happy path:** the separate `slade-thif-base-cap` case in
[`sf2-level-up-boundaries-v1`](../../../tests/fixtures/h3/level-up-boundaries-v1.json)
starts at level 40 and returns the no-level result without growth. The
[EXP-command boundary](../contracts/level-up.md#confirmed-battle-award-entry) still consumes one
threshold at the cap. More awarded EXP therefore need not produce another level, heal the member or
open another spell option. This is a class-cap consequence, not a claim that level 40 is the best
promotion point.

These cases connect reward, retained state, equipment and later rule inputs. They are separate
controlled seams, not one naturally played Bowie-to-Slade sequence. The full-party Creed branch and
spellbook refusals above check different ordinary valid states; they cannot be repaired by assuming
every successful join or EXP award grants an immediate usable new action.

## Gold and Item Economy Boundaries

### Confirmed battle outcomes

- Enemy gold is a 103-entry big-endian word table aligned with the 103 enemy definitions. A following
  69-word ROM tail is explicitly unused and cannot become additional enemy rows.
- The confirmed runtime addition uses an unsigned 32-bit intermediate. Ordinary `0 + 30` produces
  30; exact-cap, above-cap, and 32-bit-carry cases produce 9,999,999.
- Enemy drops use 30 records keyed by battle, enemy combatant, held item, and one-time flag. Three
  named boss weapons require `RNG(32) == 0`; the other 27 records do not consume drop RNG after their
  preconditions match.
- On a successful drop, the confirmed order is rare roll when applicable, test/set the persistent
  flag, remove the enemy item, then route it. A living actor with room receives it. If direct delivery
  fails, only a rare item enters Deals; a non-rare item is discarded.
- A repeated flag still follows the successful rare roll before aborting. Deals amount `14 -> 15`,
  while 15 remains saturated in its four-bit field.

These are resource mutations, not proof of how or when the player is shown a reward summary.

### Source-static service exchanges

The original service rail confirms route dataflow and helper-call order. The current
[service contract](../contracts/service-interactions.md) additionally owns bounded Raise/Cure/Save
runtime observations, and the [smithing account](../../research/dwarven-village-and-mithril-smithing.md)
owns later source/caller and controlled transaction evidence. The table is an interim route, not a
complete service explanation; each row retains its evidence level.

| Surface | Confirmed source-static price or gate | Confirmed mutation-call order | Runtime boundary |
| --- | --- | --- | --- |
| Shop Buy | 16-bit item price; gold and four-slot checks precede recipient path | `DecreaseGold -> AddItem` | Final gold/item state, cancellation side effects, and persistence are **Unknown**. |
| Shop Sell | `(price * 3) >> 2`; unsellable and rare checks are distinct | `IncreaseGold -> DropItemBySlot`, with a conditional Deals helper | Rounding is source dataflow; helper outcomes are not covered by a service H3 fixture. |
| Shop Repair | `price >> 2`; broken-item gate | `DecreaseGold -> RepairItemBySlot` | Final repaired state is **Unknown** at runtime. |
| Deals purchase | 16-bit item price; gold and four-slot checks | `DecreaseGold -> AddItem -> RemoveItemFromDeals` | Failure rollback/atomicity and persistence are **Unknown**. |
| Church Raise | Level-based price; promoted-data surcharge | Ordered payment, HP increase and mapsprite completion | **Confirmed bounded runtime:** equality-inclusive affordability and final HP `min(maxHP,200)`. Wider UI, caller and persistence limits remain at the [Raise seam](../contracts/service-interactions.md#church-raise-runtime-seam). |
| Church Cure | Distinct poison, stun and item-price curse costs | Poison → stun → curse, with separate payment/status/equipment commits | **Confirmed bounded runtime:** ordered costs and final mutations in the named [Cure cohort](../contracts/service-interactions.md#church-cure-runtime-seam); broader admission, UI and persistence remain unclosed. |
| Church Save | Prompt-dependent save versus initial exit | Accepted branch copies current map to Egress, sets flag 399 and calls `SaveGame`; later continue/suspend split | **Confirmed bounded runtime:** save call/return, with suspend observed only to its entry. [Save seam](../contracts/service-interactions.md#church-save-runtime-seam) does not prove durable storage or complete suspend. |
| Church Promote | level 20 plus regular/special promotion-data gates | `SetClass -> Promote`; special branches include item/spell/weapon handling | Complete stat effects, item edge cases, and persistence are **Unknown**. |
| Caravan | force 12, storage 64, and member inventory 4 are distinct guards | Deposit adds to caravan before member drop; derive/give have distinct normal and exchange sequences | Helper-internal results and persistence are **Unknown**. |
| Blacksmith placement and fulfillment | Mithril, promoted/eligible class and payment; four-order post-placement continuation is not an admission gate | Placement, readiness and fulfillment are separate stages; received-item commitment precedes optional equipment | **Confirmed source and bounded controlled observations:** later [smithing owner](../../research/dwarven-village-and-mithril-smithing.md#smith-entry-party-list-and-order-classification) covers picker/class/RNG order, revisit flag and fulfillment. Natural complete service play, measured RNG distribution and durable persistence remain **Unknown**. |

The confirmed battle-drop route into Deals is runtime evidence. Shop and caravan use of
Deals retains its source-static boundary unless a named observation owns the same path; the smith's
separately observed add/equip seams do not establish Deals runtime behavior. These two evidence
levels must not be merged merely because they touch the same stored field.

## Persistence Boundary

The [save-system contract](../contracts/save-system.md) confirms a two-slot representation, checksums,
save/load/copy/delete helper order, and a bounded in-process action matrix. It does not establish
cross-process SRAM survival, interrupted writes, or the persistence of every service, Deals, caravan,
order, drop-flag, gold, EXP, or combatant mutation across all lifecycle paths.

Therefore this synthesis may identify fields as persistent within an observed replay or original
state structure, but it does not claim that every connected mutation has an end-to-end save/load H3
case. Any future lifecycle claim needs evidence at that lifecycle boundary rather than treating the
existence of `SaveGame` as proof for every subsystem. This does not dispatch a new suite.

## Evidence Matrix

| Boundary | Evidence label and bounded claim | Exact owners | Remaining question |
| --- | --- | --- | --- |
| joined, active, following and control | **Confirmed static** distinct helpers and selected Creed/Jaro caller branches; roster contract retains bounded handler observations | [membership](../contracts/party-membership-state.md), [roster](../contracts/party-roster-state.md), [Creed](../../research/creed-return-fairy-and-recruitment-choices.md), [Jaro conversion](../../research/battle32-activation-jaro-and-first-turn-dispatch.md), [Jaro victory](../../research/battle32-outcomes-and-moun-entry.md) | All-recruit natural lifecycle, UI and capacity safety are not established |
| start/class/promotion inputs | **Confirmed static** definitions, paired mappings and ordered Church requests | [ally definitions](../contracts/ally-definition-data.md), [services](../contracts/service-interactions.md), `sf2-ally-data-static-v1` ([fixture](../../../tests/fixtures/h2/ally-data-static-v1.json)), common-menu promotion statement records | Complete promotion stat/inventory outcomes and persistence |
| learned entries and equipment contribution | **Confirmed static** learning refusal/upgrade helper and class/type equip checks; **Confirmed bounded runtime** inherited upgrade, equipped refresh and selected cursed replacement refusal | [spellbook](../contracts/spellbook-state.md), [level-up](../contracts/level-up.md), [item definitions](../contracts/item-definition-data.md), [smith equipment](../../research/dwarven-village-and-mithril-smithing.md#fulfillment-inventory-and-optional-equipment) | Complete acquisition/equip/use lifecycle and every later action |
| growth storage and level-up | **Confirmed** curves/class blocks plus runtime gain, cap, spell, scan, and refresh cases | [ally-growth research](../../research/ally-growth.md), [growth manifest](../../../manifests/extractions/growth-data.json), [growth schema](../../../schemas/growth-data.schema.json); `sf2-calculate-stat-gain-startup-v1` ([`stat-gain-v1.json`](../../../tests/fixtures/h3/stat-gain-v1.json)), `sf2-level-up-tort-boundary-v1` ([`level-up-v1.json`](../../../tests/fixtures/h3/level-up-v1.json)), `sf2-level-up-boundaries-v1` ([`level-up-boundaries-v1.json`](../../../tests/fixtures/h3/level-up-boundaries-v1.json)), and `sf2-level-up-refresh-v1` ([`level-up-refresh-v1.json`](../../../tests/fixtures/h3/level-up-refresh-v1.json)) | Natural campaign distributions, intended curves, remaining clamp edges |
| battle EXP to level-up | **Confirmed runtime** command replay through one threshold and persistent refresh | [runtime battle-math research](../../research/runtime-rng-and-battle-math.md); `sf2-battle-exp-level-up-v1` ([`battle-exp-level-up-v1.json`](../../../tests/fixtures/h3/battle-exp-level-up-v1.json)) | Other award modifiers and repeated commands |
| reward bracket, finalization, and storage | **Confirmed runtime** effective-level brackets, Battle 01 adjustment/randomization/minimum, stored cap 200, and one threshold | `sf2-kill-exp-level-difference-v1` ([`kill-exp-level-difference-v1.json`](../../../tests/fixtures/h3/kill-exp-level-difference-v1.json)), `sf2-award-exp-randomization-v1` ([`award-exp-randomization-v1.json`](../../../tests/fixtures/h3/award-exp-randomization-v1.json)), and `sf2-exp-command-boundaries-v1` ([`exp-command-boundaries-v1.json`](../../../tests/fixtures/h3/exp-command-boundaries-v1.json)) | Other battles, same-side routing, multiple-action/cap lifecycle |
| physical action construction and replay | **Confirmed runtime subset** damage/kill accumulator cap and persistent command replay | [combat contract](../contracts/combat-resolution.md); `sf2-physical-damage-application-v1` ([`physical-damage-application-v1.json`](../../../tests/fixtures/h3/physical-damage-application-v1.json)) and `sf2-battle-scene-replay-v1` ([`battle-scene-replay-v1.json`](../../../tests/fixtures/h3/battle-scene-replay-v1.json)) | Complete action set and presentation timing |
| spell progression contributions | **Confirmed runtime subsets** attack-spell bracket/cap and healer eligibility/minimum/cap | [spell contract](../contracts/spell-resolution.md); `sf2-spell-damage-exp-v1` ([`spell-damage-exp-v1.json`](../../../tests/fixtures/h3/spell-damage-exp-v1.json)) and `sf2-healing-exp-boundaries-v1` ([`spell-healing-exp-boundaries-v1.json`](../../../tests/fixtures/h3/spell-healing-exp-boundaries-v1.json)) | Unsupported spell families and generalized action policy |
| enemy gold | **Confirmed static/runtime** 103 used rows plus unused tail, and increase/cap/carry cases | [enemy rewards research](../../research/enemy-promotions.md); `sf2-enemy-gold-v1` ([`enemy-gold-v1.json`](../../../tests/fixtures/h2/enemy-gold-v1.json)) and `sf2-gold-boundaries-v1` ([`gold-boundaries-v1.json`](../../../tests/fixtures/h3/gold-boundaries-v1.json)) | DecreaseGold and non-battle callers |
| enemy item drops and Deals routing | **Confirmed static/runtime** 30 records, three random items, one-time flags, recipient routing, and nibble saturation | `sf2-enemy-item-drops-v1` ([`enemy-item-drops-v1.json`](../../../tests/fixtures/h2/enemy-item-drops-v1.json)) and `sf2-enemy-item-drop-behavior-v1` ([`enemy-item-drop-behavior-v1.json`](../../../tests/fixtures/h3/enemy-item-drop-behavior-v1.json)) | Player-visible reward flow and full save lifecycle |
| service economy | **Confirmed static** prices/gates/calls; **Confirmed bounded runtime** Church Raise/Cure/Save and selected smith helper/transaction seams | [service contract](../contracts/service-interactions.md), [common menus](../../research/common-menus.md), [smithing](../../research/dwarven-village-and-mithril-smithing.md); `sf2-common-menus-static-v1` ([fixture](../../../tests/fixtures/h2/common-menus-static-v1.json)) | Complete promotion effects, unobserved caller/admission/return, atomicity and durable persistence |
| shared state helpers | **Confirmed static** helper surface; runtime claims belong to their named clamp/caller fixtures | [common-stats research](../../research/common-stats.md); `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Caller-dependent mutation outcomes outside existing H3 fixtures |
| save handoff | **Confirmed representation and bounded in-process actions** | [save contract](../contracts/save-system.md); `sf2-tech-services-static-v1` ([`tech-services-static-v1.json`](../../../tests/fixtures/h2/tech-services-static-v1.json)) and `sf2-witch-save-actions-runtime-v1` ([`witch-save-actions-v1.json`](../../../tests/fixtures/h3/witch-save-actions-v1.json)) | Cross-process/power-loss behavior and subsystem-complete persistence |

## Pre-Synthesis Evidence Audit

The original resource audit reviewed owning research, contracts, fixture payloads and exact IDs,
schema/verifier registrations, focused tests and CLI ownership before combining their conclusions.
The following narrow commands passed on 2026-08-01:

```powershell
uv run sf2 h2 common-stats
uv run sf2 h2 common-menus
uv run sf2 h2 enemy-gold
uv run sf2 h2 enemy-drops
uv run sf2 h3 growth
uv run sf2 h3 battle-exp
uv run sf2 h3 kill-exp
uv run sf2 h3 award-exp
uv run sf2 h3 exp-command
uv run sf2 h3 gold
uv run sf2 h3 enemy-drops
```

The executable owners agreed on the values and order synthesized here. The original audit also found
two owner-document discrepancies and deliberately did not copy them. Both findings have since been
resolved in their owning documents:

1. [Issue #11](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/11) recorded a stale
   reward queue for rare-drop RNG, full-inventory/deals routing, and an already-set drop flag. The
   merged [owner correction PR #18](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/18)
   removed that queue and explicitly retained the accepted ten-case
   `sf2-enemy-item-drop-behavior-v1` evidence covering those branches.
2. [Issue #12](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/12) recorded an
   over-broad Church Raise summary. The merged
   [owner correction PR #19](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/19) now
   preserves only the source-derived filtered mutation-helper order `j_DecreaseGold` →
   `j_IncreaseCurrentHp` (after `move.w #CHAR_STATCAP_HP,d1`) → `UpdateAllyMapsprite`. That order
   does not assert that no other calls intervene; final current HP and caller-visible runtime outcome
   remained **Unknown** at that audit boundary.

These are resolved audit findings, not pending follow-up work. This synthesis continues to consume the
accepted drop fixture and the historical Raise call seam. Later bounded Raise/Cure/Save observations
are owned by the [current service contract](../contracts/service-interactions.md#church-raise-runtime-seam);
they supersede the old final-HP Unknown only at their named seams, without implying broader reward UX,
service completion, persistence, or player-visible outcomes.

### Party-development evidence review

The current revision reviewed the accepted research/contracts against the selected fixture payloads,
source-shaped statement records and their schema/verifier/CLI owners. It did not rerun the historical
H2/H3 commands above. Exact parameters remain at those owners rather than becoming another catalog.

| Selected explanation | Surfaces checked and disposition |
| --- | --- |
| Ordinary join, follower and Jaro exceptions | Membership/roster contracts, Creed return and both Battle32 research owners; `sf2-common-stats-static-v1` party facts and `stats.py`; map-script fixture/handler ownership and bounded active-party cases. Use later source/caller reviews for twelve and explicit Jaro overflow; do not expand the symbolic helper contract or claim complete recruitment runtime. |
| Definitions and promotion | Ally-definition/growth contracts, ally-data fixture and extraction schema; common-menu `church.routeOperations.promote` plus full `helperOperations`, `common-menus-church.schema.json` and `menus.py:_church_static_contract`. Separate selected class/item/spell/weapon/level/EXP requests from final `Promote` effects; no promotion gold debit appears in that route. |
| Learning, growth and refresh | Spellbook/common-stats, ally-growth and level-up owners; the boundary/refresh/EXP-command fixture inputs, outputs and exact case IDs; `h3/growth.py`, its declared schemas and CLI routes. Known-base-before-empty is a static helper rule; inherited Kazin upgrade, TORT threshold and equipped refresh have bounded runtime owners. No generalized class predicate or unconditional heal follows. |
| Equipment and later action | Item definitions, combatant-state fields, common stats, smith fulfillment/optional-equip review and tactical item/Taros rules. Definition flags are inputs; actual class/type/equipped/curse consumers and the reached action gate determine the claim. Carrying, equipping, breaking and using remain separate. |
| Old service summaries | Current Raise/Cure/Save contract seams and later smithing account supersede absolute Unknowns only within their named boundaries. The original audit above remains historical. No new service acquisition or full resource rewrite is implied. |

These are claim-review dispositions, not new original observations or blanket test PASS. The
[roadmap](../documentation-roadmap.md#authoring-language-policy) retains Chinese drift and deferred
synchronization. Existing completed comparison failures and corrections remain at their
[retained evidence owner](../../../remake/docs/evidence/retained-comparisons.md); this document does
not reset or reclassify them.

## Original Fidelity and Modernization

Original-fidelity behavior keeps the evidence-owned stage boundaries even when a modern
implementation uses different internal structures. In particular, it preserves ordered RNG use,
integer truncation, action-family caps, command replay, the single-threshold rule, current/max stat
separation, the 32-bit carry-to-gold-cap case, one-time drop flags, item-routing order, and the
fixture-owned level-up defects.

A modernization may choose clearer level display, continuous multi-level processing, corrected TORT
classification, corrected prowess handling, atomic service transactions, different inventory
routing, or platform-native save durability. Each is a future product decision with a named expected
deviation and verification appropriate to that behavior under ADR 0019; none is introduced here as
a newly inferred original rule.

## H4 Acceptance and Expansion Gates

This heading remains a stable route for earlier readers. The current development direction is
[ADR 0019](../../decisions/0019-state-and-content-driven-remake-engine.md): live state, validated
content and explicit capabilities govern the common engine. This synthesis does not require a new
reference runtime, universal ordered-trace adapter or a grouped runtime acquisition programme.
Historical fixtures remain evidence for their exact original boundaries; old reproduction commands
are not new obligations merely because this explanation changed.

The [capability owner](../../../remake/docs/capability-status.md) records the bounded Battle01 growth,
spell-upgrade and outcome consumers, unsupported broader rules and deferred save/load. A source
contract, a controlled original observation and an implemented production capability answer different
questions. Adding this party explanation advances none of those executable frontiers. Current
verification follows the [development/verification scope](../../../remake/docs/development-and-verification.md#scope).

The sequential resource outcome may deepen the existing item/gold/recovery/service explanation while
preserving this party-development owner. All-roster choice analysis, optimal growth/promotion,
campaign balance, unclosed promotion effects and broader persistence remain **Unknown** or future
scoped design/research decisions. They are not an automatic emulator queue. Independent English
acceptance and the roadmap's later stability decision precede any separate Chinese synchronization.
