# Progression and Economy

Between battles, Shining Force II asks how to turn limited roster slots, four-item packs, gold,
and town services into stronger choices on the next battlefield. Recruitment adds options to the
force, combat converts actions into EXP, gold, and drops, and shops, churches, the Caravan, and
the smith turn those resources into gear, recovery, and class upgrades.

## Party development

### Joining, fighting, following, and being alive

A character's relationship to the party is tracked across four separate states—never assume one
implies another:

| Player question | What decides it | Practical consequence |
| --- | --- | --- |
| Is this character in my Force? | **Joined** membership | Available to the force even when in reserve or at 0 HP. |
| Will they deploy in battle? | **Active** party flag (ordinary cap: 12) | Ordinary recruitment activates the recruit only when fewer than 12 members are active; otherwise they join into **reserve**. |
| Are they walking behind Bowie right now? | Temporary **follower** state | Following on a town/field map does not join, deploy, or heal a character. |
| Can they take a turn in battle? | Placement, positive HP, status, and control | Being joined and active still does not guarantee manual control or a living actor. |

Two campaign cases illustrate why these states stay distinct:

- **Creed recruitment with a full active roster** (**Confirmed** static): accepting one of Creed's
  offers while 12 other members are active still joins the recruit and lets them follow, but skips
  activating them and evicts nobody (0 HP also neither blocks joining nor gets healed by it).
- **Jaro in Battle32** (**Confirmed** static exception): mid-battle region conversion explicitly
  activates Jaro on top of a 12-member party (making 13 active flags) under AI control using his
  own ally HP and stats. First victory later clears AI control and moves him to reserve as a
  permanent joined member—showing that 12 is the ordinary auto-activation cap, not a universal
  hard invariant.

### Definitions, current class, and promotion

Each ally starts with a base class, stored level, and initial items; class drives growth blocks,
spell lists, movement type, and equipment eligibility. **Stored level** stays on the character,
while **effective level** is computed separately by specific consumers (reward comparisons add 20
for promoted units; spell learning and level caps use their own class rules).

**Church promotion rules:**

- Requires a joined member (including reserves or fallen allies) whose current class is promotable
  and whose stored level is **at least 20**.
- **Regular promotion** is free.
- **Special promotion** searches all joined members' inventories (not just the candidate) for the
  matching promotion item. Declining the special choice falls back to regular promotion; SORC adds
  a spellbook-overwrite confirmation whose refusal preserves both item and book. Only final
  acceptance removes the item from its holder.
- On success, the member takes the new class at **level 1, EXP 0**, keeping earned stats and
  current HP/MP while refreshing base/class stats and worn gear. SORC replaces its book with
  DAO 1; MMNK and NINJ unequip their weapon without losing it from inventory.
- Original complete `Promote` stat mutations and SRAM round-trip persistence remain **Unknown**;
  the remake preserves promoted classes across field, battle, return, and saves (see
  [church promotion design](../../../../remake/docs/application/church-promotion-design.md)).

### Carried gear becomes an equipped contribution

Possessing a weapon or ring fills one of a member's four inventory slots; only **equipping** it
(subject to class mask and equipment type) applies its stat/range/effect contribution during
derived-stat refresh. A member may wear one weapon and one ring independently.

- **Broken vs. cursed:** broken gear is damaged by battle Use and can be repaired at a shop;
  equipped cursed gear blocks unequipping, replacement, Give, Drop, and exchange until cured at
  Church (and can re-apply CURSE on refresh).
- **Equipping vs. merely carrying:** in Battle18, Bowie carrying the Achilles Sword without
  equipping it fails the Taros attack gate—see
  [Taros equipment gate](tactical-battle-loop.md#equipment-can-gate-an-effect-taros).

### Learning spells adds or upgrades an option

Each ally has four ordered spell slots storing base spell identity and learned rank:

- Learning a **higher rank** of an already-known spell upgrades that slot in place—even if all four
  slots are full.
- Learning the same or lower rank is rejected even if another slot is empty.
- Learning a **new spell** requires an empty slot; a full book refuses it rather than asking to
  forget a spell.

Ordinary `LevelUp` checks exact matching thresholds for the ally/class growth block (with `$FE`
inheriting the ally's first block, as observed in Kazin/WIZ upgrading BLAZE). The original
learning check also contains a documented fidelity quirk that treats TORT (class 11) as promoted
for learning thresholds even though base-class caps split at class 12. Knowing a spell still
requires enough current MP and valid targets to cast it.

## Battle-driven growth and a concrete level-up example

Combat turns into lasting strength through four ordered steps:

1. **Action EXP accumulator:** damage/kill actions accumulate up to 49 EXP; eligible healing
   actions accumulate up to 25 EXP based on recovered HP relative to target max HP.
2. **Final EXP command:** battle-table adjustments (e.g., Battle01's halving) and ordered random
   checks produce the awarded command amount.
3. **Stored EXP & single threshold:** stored EXP increases (clamped at 200), then subtracts **at
   most one** 100-point threshold and triggers `LevelUp` at most once per command (`75 + 24 → 99`
   no level-up; `76 + 24 → 100 → 0` one level-up; `199 + 24 → 200 → 100` still only one level-up).
4. **Level-up & stat refresh:** raises max HP, max MP, and base ATT/DEF/AGI, increments stored
   level, checks spell learning, and rebuilds derived stats with status and worn gear. **Current HP
   and MP do not heal when their maxima increase.**

### Worked example: level-up raises maxima without healing (**Confirmed** controlled observations)

Two controlled original runtime cases show how growth, equipment, and caps interact:

- **Bowie's Battle01 level-up (`sf2-battle-exp-level-up-v1`):** starting level 1 with 99 EXP,
  receiving a 24-point EXP command brings Bowie through 123 to **23 residual EXP at level 2**.
  Maximum HP rises to 14, while current HP/MP stay **12/8** and action-only buffs are replaced by
  refreshed stats.
- **Slade's equipped refresh and cap (`sf2-level-up-refresh-v1` / `sf2-level-up-boundaries-v1`):**
  at THIF level 39 → 40, base ATT rises from 45 to 47 and the worn Short Knife (+5) refreshes total
  ATT to **52**. Maximum HP grows from 42 to 44, but current HP remains **7**—Slade is still
  wounded and needs healing or a service. Once at the base-class cap of **40** (classes below ID 12
  cap at 40; classes from ID 12 onward cap at 99), hitting 100 EXP still spends one 100-point
  threshold without granting another level.

These are separate controlled original cases, not one continuous natural playthrough and not a
recommendation on when to promote.

## Gold, item capacity, and services

### Custody, storage, and active membership

Three separate capacity limits shape inventory planning:

| Limit | What it bounds | What it does not do |
| --- | --- | --- |
| **4 slots per member** | Held items (including equipped weapon and ring) | Does not auto-equip or merge across the party. |
| **12 active battle members** | Ordinary battle deployment and Caravan Join relief | Reserves stay in the force and can still hold or receive items in several routes. |
| **64 Caravan Depot items** | Shared Caravan storage (separate from 4-bit Deals counts capped at 15) | Does not expand a member's 4 battle-usable slots. |

### Sources, receipts, and the capacity decision

Enemy gold uses a 103-entry table and saturates at **9,999,999** on addition. Enemy item drops
use 30 keyed records (only three boss weapons require `RNG(32) == 0`). When every eligible pack is
full, **different item sources follow deliberately different fallback policies**:

- **Battle enemy drop:** sets the one-time flag and removes the enemy's item before routing. On a
  full pack, a **rare** item enters shop Deals, while a **non-rare** item is **discarded** (no
  retry).
- **Ordinary field Search:** tries the leader's four slots, then other joined members; if all are
  full, the chest closes or the pickup stays available so the player can make room and return.
- **Mandatory story rewards (Sky Orb, Force Sword):** if all joined packs are full, enters a
  forced-discard loop (rejecting cancel, unsellable items, and equipped cursed gear; discarding a
  rare item sends it to Deals and keeps asking until a non-rare replacement succeeds).
- **Optional custom pickup (Arm of Golem):** can be declined, and a full party leaves it claimable
  without forcing a discard.

### Shops, Church, and Caravan services

- **Shop (Buy, Deals, Sell, Repair):**
  - **Buy** checks gold ≥ price and a recipient with fewer than 4 slots; optional equipping happens
    *after* payment and item addition—declining to equip or being blocked by a cursed weapon does
    not refund the purchase.
  - **Deals** lists rare items with nonzero stored counts that are **absent from that shop's normal
    catalog** (so a rare item is not automatically listed in every shop).
  - **Sell** pays `(price * 3) >> 2` (refusing unsellable or equipped cursed items) and adds sold
    rare items to Deals.
  - **Repair** fixes broken gear for `price >> 2` (separate from curing curses or reviving allies).
  - Original shop UI completion and interrupted-transaction atomicity remain **Unknown**.
- **Caravan (Join/Purge, Depot, Item):**
  - **Join/Purge** adjusts the 12-member active roster without deleting membership or items, cannot
    Purge or swap out the leader, and never revives a 0-HP member.
  - **Depot** supports Look, Deposit (up to 64), Withdraw ("Derive", offering an exchange if the
    recipient is full), and Drop (rare drops enter Deals). Equipped cursed items block deposit and
    exchange.
  - **Item** offers Use, Give (including exchange when the recipient is full), Equip, and Drop.

#### Church: recovery, promotion, and saving

- **Raise** (**Confirmed** bounded observation): revives fallen members in order for
  `stored_level × 10` (+200 if promoted), restoring current HP to `min(maxHP, 200)` without
  restoring MP.
- **Cure** (**Confirmed** bounded observation): processes **poison (10 G) → stun (20 G) → curse
  (`item_price >> 2`, e.g., 4,250 G for Dark Sword)** in order, each with separate confirmation and
  payment (an observed three-affliction case spends `4,280 → 4,270 → 4,250 → 0`).
- **Promote:** free base promotion and item-consuming special promotion as above.
- **Save** (**Confirmed** bounded observation): updates the Egress return map, sets flag 399, and
  calls `SaveGame` with no gold charge and no HP/MP heal.

### Mithril orders: placement, readiness, and collection

At Dwarven Village, the material owner, promoted customer class, and eventual recipient are
independent:

- **Placement:** costs **1 Mithril + 5,000 G** (ordinarily up to 4 pending orders). The customer
  must be an eligible promoted class; the resulting weapon is rolled via sequential RNG **at
  placement**, not collection, and placement clears readiness flag F80.
- **Readiness:** only an explicit fresh exploration entry sets F80—waiting, walking, or internal
  stairs do not finish orders. Placing a new order while leaving ready orders uncollected clears
  F80 and puts the remaining orders back to pending.
- **Collection & cursed-weapon example** (**Inferred** across **Confirmed** seams): ready orders
  are offered in slot order. If the chosen recipient is full or canceled, the order stays ready.
  Once accepted with room, the item is added and the order clears *before* optional equipping. If
  the recipient is wearing a cursed Dark Sword, equipping is refused: the forged weapon stays
  carried in inventory while the order is gone (no refund or Mithril return), until a separate
  Church Cure clears the curse. Natural end-to-end revisit, presentation, and save durability for
  smithing remain **Unknown**.

## Persistence, fidelity, and modernization boundaries

- **Returning does not save:** leaving a shop, Church, smith, or battle changes live state in RAM,
  not durable storage. Although `SaveGame`'s 4,016-byte domain includes allies, items, gold, flags
  (including F80), and Mithril order words, cross-process original SRAM durability for every
  service mutation remains **Unknown**. Remake settled-field and battle Save/Continue follow
  [opening-session-lifecycle.md](../../../../remake/docs/application/opening-session-lifecycle.md)
  and [battle-save-and-continue.md](../../../../remake/docs/application/battle-save-and-continue.md).
- **No balance or tier prescriptions:** this guide explains rules and tradeoffs without claiming
  intended difficulty curves, scarcity targets, optimal promotion levels, or best party builds.
- **Modernization:** intentional changes (clearer displays, atomic service transactions, durable
  modern saves, or bug fixes to original quirks like TORT learning or `INCREASE_DOUBLE` prowess)
  require explicit remake decisions rather than silently rewriting original facts.

## Basis, evidence map, and compatibility links

| Topic | Canonical contract & original evidence owners |
| --- | --- |
| Membership, active cap 12, Creed & Jaro | [Membership](../../contracts/party/party-membership-state.md), [roster](../../contracts/party/party-roster-state.md), [Creed](../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md), [Battle32 Jaro](../../../research/chapters/moun-nazca-grans/battle32-activation-jaro-and-first-turn-dispatch.md) |
| Ally/item/spell definitions & spellbook | [Ally data](../../contracts/party/ally-definition-data.md), [item data](../../contracts/party/item-definition-data.md), [spell data](../../contracts/party/spell-definition-data.md), [spellbook](../../contracts/party/spellbook-state.md) |
| Growth, EXP thresholds, caps & refresh | [Level-up contract](../../contracts/party/level-up.md), [ally growth](../../../research/party/ally-growth.md), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md) |
| Gold, drops & field/story capacity | [Enemy rewards](../../../research/party/enemy-promotions.md), [field Search](../../../research/world/field-search-control.md), [Sky Orb](../../../research/chapters/moun-nazca-grans/battle34-outcomes-and-sky-orb-handoff.md#sky-orb-mandatory-inventory-award), [Force Sword](../../../research/chapters/final-confrontations-ending/battle39-outcomes-and-force-sword-route.md#map42-attraction-extraction-and-mandatory-inventory) |
| Shop, Church, Caravan, Deals & Mithril smith | [Services](../../contracts/party/service-interactions.md), [Caravan & Deals](../../contracts/party/caravan-and-deals-state.md), [common menus](../../../research/presentation/common-menus.md), [Mithril smithing](../../../research/chapters/moun-nazca-grans/dwarven-village-and-mithril-smithing.md) |
| Save domain & remake support | [Save contract](../../contracts/system/save-system.md), [capability status](../../../../remake/docs/capability-status.md#campaign-feature-trace) |

### Source-static service exchanges

Compatibility anchor—see [Shops, Church, and Caravan services](#shops-church-and-caravan-services)
and [Church: recovery, promotion, and saving](#church-recovery-promotion-and-saving) above.
