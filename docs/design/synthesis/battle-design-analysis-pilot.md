# Battle Design Analysis: Battle01, Taros and Chessboard

These three encounters pose different practical problems: coordinate a small force, protect the
one actor whose equipped weapon can pass a boss gate, and manage a mixed formation whose waiting
rules change as combat develops. **Inferred design reading:** those problems connect the common
battle rules to distinct choices about commitment, support and target priority. They are useful
ways to explain the arrangements, not proof of the original authors' intentions.

This English pilot follows the [roadmap agreement](../documentation-roadmap.md#bounded-battle-design-analysis-pilot).
Read [Tactical Battle Loop](tactical-battle-loop.md) for action/control rules and
[Progression and Economy](progression-and-economy.md#party-development) for how membership,
equipment and learned spells become current resources. Here a design solution means problem →
rule or placement → a simple conditional response, with its cost or failure risk. No optimal route,
turn script, calibrated difficulty, normal-player level distribution or balance change is implied.

**Confirmed** below means an accepted source-static fact or the specifically named bounded
observation. **Inferred** marks the proposed explanation and responses. **Unknown** marks what the
selected evidence cannot establish. Original facts use the pinned USA release and
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, with full provenance at the linked research
owners. Coordinates are battle-local X/Y, with smaller Y described as north for reading the
formation. They are not walking routes. Terrain, current movement type, occupancy and action range
still determine whether a move or attack is legal.

## Battle01: commit the small force together

### What the arrangement asks of the player

**Inferred emphasis:** with only three allied participants facing six enemies of one type, the
distinctive decision is how far to commit the force while keeping attacks and recovery available.
There is no enemy leader shortcut. A lone advance can change enemy activation for everyone, while
losing Bowie can end the battle even if another ally survives.

**Confirmed arrangement:** [Battle01 placement research](../../research/battle01-placement.md)
selects Map57, a 16×20 battle area and the tower-interior battle background. The three starting
positions are `(8,18)`, `(9,18)` and `(7,18)`, grouped along the lower part of that area. The
48×48 terrain storage contains obstructed cells, low sky, plains, road and grass. Its whole-array
counts include padding outside the battle area; they are not percentages of usable ground or a
map of safe lanes. The selected account supports a compact tower encounter, but not an assertion
that every visible floor tile is passable or that a particular corridor is best. The
[navigation contract](../contracts/battlefield-navigation.md#movement-initialization-and-admission)
separates terrain cost, movement type and occupancy.

**Confirmed enemies and activation:** all six are `GIZMO` definition39, with base HP5, ATT7,
DEF/AGI/MOV5 and hovering movement. Those are definition values, not guaranteed inflicted damage.
Four use `ATTACKER1`, two `ATTACKER2`; all are `STARTING`, carry no item and have no stored follow
order. Their primary activation assignments cover three overlapping regions; all secondary
regions are `NONE`. Thus the useful contrast is shared enemy type with different control/activation
assignments, rather than a mixed army of healers, archers and a boss.

**Confirmed bounded observation:** the initialized three positions trigger none of the regions.
In `sf2-battle01-region-activation-v1`, the harness then relocates Bowie directly from `(8,18)` to
`(8,12)` before activation. All three flags and the six primary activation bits become active.
That controlled relocation establishes the boundary consequence, not a naturally walked route or
the order in which enemies approach. Scheduled participation also differs from activation: all six
living placed Gizmos can enter turn order while their AI still has standby handling.

### Preparation and two simple responses

**Explicit preparation assumption:** use the selected opening force, Bowie/Sarah/Chester, alive
and available for their actual control path. Assume Sarah has usable HEAL1 with enough current MP,
and an intended item user actually carries a Medical Herb if that alternative is chosen. This is
a bounded, plausible setup: the [opening guide](opening-through-battle01.md#6-winning-battle-and-the-after-program)
records both recovery actions in one accepted original winning lineage. It does not make its levels,
stats, seed, first actor or resource stock mandatory for all players.

1. **Inferred response — advance in supporting distance.** Move the attackers forward without
   leaving the recovery role unable to reach them, and concentrate available attacks on an enemy
   that can actually be targeted. This connects the small force to a simple aim: remove an opposing
   action source while retaining recovery. The tradeoff is slower forward progress and possible
   congestion; a nearby ally is not automatically a legal healer or attacker after movement,
   occupancy and turn order are considered. Waiting does not freeze standby enemies.
2. **Inferred response — recover before the next commitment.** When a vulnerable ally would
   otherwise advance unsupported, spend an available healing action or Herb instead. This exchanges
   an immediate attack for survival margin, with MP/item consumption and another enemy turn as
   costs. Protecting Bowie has a separate outcome reason, not merely a preference for keeping all
   three alive. No HP threshold or guaranteed survival follows without the current state.

**Confirmed outcome / Unknown route:** ordinary victory requires no living placed enemies; Bowie
HP0 forces defeat even with another survivor. The accepted original lineage demonstrates one
winning battle and return, not either proposed response's reliability across legal states. Exact
standby movement, later region crossings, damage and recovery sufficiency for another party state
remain **Unknown** here. This explains controlled commitment, without claiming a prescribed tutorial.

## Taros / Battle18: support an equipped specialist

### The approach and the guarded objective

**Inferred emphasis:** the encounter separates ordinary group combat from a final actor/equipment
requirement. The force can deal with supporting enemies, but simply bringing more damage sources
does not replace Bowie's specific equipped attack against Taros. Preparation and keeping that
actor usable become part of the combat problem.

**Confirmed arrangement:** the [initialization/first-control owner](../../research/battle18-initialization-and-first-control.md)
selects Map12, a 26×32 crop and `TAROS_SHRINE` background. Twelve ally placement templates cluster
at X11–13, Y27–30; Taros is at `(12,8)`. Between and beside them are enemies near `(5,24)`,
`(19,21)` and `(16,19)`, further groups to either side, and the immediate boss support at
`(11,9)`, `(13,9)` and `(14,10)`. This supports a southern force facing a northern objective with
distributed intervening threats. The crop and background do not establish a clear center lane,
stairs, cover or a passable flank. Exact route geometry is **Unknown** at this explanation boundary.

**Confirmed enemy composition:** fourteen enemies comprise Taros, two Master Mages, one Black
Monk, three Gargoyles, two Arrow Launchers, three Golems and two Soulsowers. The selected definitions
give Mages BLAZE2; the Monk HEAL2/BLAST1; Launchers an equipped range2–2 weapon; Gargoyles flying
movement; and Soulsowers stun prowess. These support the practical role descriptions of spell,
healing, ranged, flying and status threats. They do not promise an actual spell, heal, stun or target.
The Monk's healing consumer can select a different rank from its learned HEAL2; do not assume every
injury is restored by the nominal spell value.

**Confirmed activation:** five regions govern selected flank/intermediate groups; all twelve
fresh ally templates miss them. Other rows have `15/15` and bypass region standby, including Taros,
his immediate support and the three enemies at `(16,19)`, `(19,21)` and `(5,24)`. No initial region
hit therefore does not mean the whole army waits. Primary and secondary region hits have different
bit effects, and incoming tested-region cache state can affect evaluation. The research owner
retains the exact groups and geometry; no clean, permanently staged wave sequence is asserted.

Taros has MOV0, physical range1–2 and two round entries. His equipped Taros Sword can supply an
item-use BOLT2 attack with range1–3 and radius2 despite his lack of learned spells and MP. Those
are declared reach operands, not guaranteed actions or damage. Zero MOV does not make approaching
him safe: the current target, item/spell choice and AI path restrictions still matter. No safe
approach square or fully predictable sequence follows from these definitions.

### Preparation and two simple responses

**Explicit preparation assumption:** Bowie is living, active and actually has Achilles Sword
equipped. The supporting force contains living melee/ranged damage roles and at least one usable
healing source with current MP/items; optional recruits, exact classes, promotion and levels are
not fixed. Twelve templates are capacity, not evidence that twelve named living allies arrive.
The [Ribble acquisition guide](ribble-tree-opening-and-achilles-sword.md) owns obtaining/carrying
the sword, while [equipment rules](progression-and-economy.md#carried-gear-becomes-an-equipped-contribution)
explain why custody and equipment differ. Fresh entry heals eligible living allies; it does not
revive every dead member, remove every persistent status or equip the required sword.

**Confirmed gate:** at initial battle-script construction in Battle18, Bowie0 must use `ATTACK`
against enemy type `TAROS88` while `GetEquippedWeapon(0)` returns Achilles61. Merely carrying it,
another ally attacking with it, or Bowie casting/using an item does not set that gate. The later
damage helper blocks its selected ineffective route; passing the gate still does not promise a
hit, damage amount or survival. This is narrower than universal immunity to every possible effect.

1. **Inferred response — make a supported approach.** Use the rest of the force to remove reachable
   intervening threats and keep healing available while Bowie approaches for his equipped attack.
   Consider ranged/caster threats and the Monk's possible healing when choosing a reachable target.
   The cost is extra actions/resources before the objective, and moving across regions can activate
   additional enemies. Keeping support close enough to help can also put several allies inside an
   area attack; spacing must be checked against current reach rather than a drawn safe lane.
2. **Inferred response — favor the boss once the approach is viable.** If Bowie can reach Taros
   with adequate support, prioritize his legal attacks while others sustain him or contain nearby
   enemies. The [outcome owner](../../research/battle18-outcomes-and-caravan-acquisition.md)
   confirms that Taros's death can trigger leader cleanup and permit victory with other enemies
   still living, provided Bowie survives and the relevant hook/processing completes. This can avoid
   clearing every enemy, but exposes the indispensable actor to Taros's range, item attack and extra
   turn entry. It is a conditional objective priority, not a demonstrated rush or counter loophole.

**Unknown / retained boundary:** the naturally arriving party, later paths/actions, damage and
successful support timing are not established by this static encounter account. Taros Sword loot
is a separate held-item/RNG/once-bit/capacity-or-deals route, not a guaranteed victory reward.
Caravan acquisition and later boarding have their own scene/state consumers. None is needed to
claim that the equipped-specialist rule changes how the force can support its objective.

## Chessboard / Battle22: choose when to widen the fight

### A broad line with mixed roles and conditional waiting

**Inferred emphasis:** the player faces an army whose shape is easy to recognize but whose response
is not ordinary chess. Enemy roles, selective waiting and the leader objective make target choice
matter: damaging or defeating one piece can change which other pieces are able to act normally.

**Confirmed arrangement:** the [roster owner](../../research/battle22-initialization-and-roster.md)
selects Map22, crop origin `(9,0)`, size30×30 and `CHESSBOARD` battle background. The twelve ally
templates span X13–23 on Y17–18. Sixteen enemies occupy two northern rows, Y5–6, spanning X9–23;
King is `(15,5)`, with a declared point0 at that same position. Coordinates are already battle-local,
so the crop origin is not subtracted again. The broad opposing lines support questions about which
part of the formation to engage first. The gap between rows does not prove every intermediate tile
is open, identical-cost terrain, or a safe diagonal. The selected roster/control owners do not
establish detailed passability; this pilot does not derive chess movement from names or artwork.

**Confirmed composition:** one King, three Queens, two Bishops, two Knights, two Rooks and six
Pawns. King learns BLAZE2 and has two round entries; Queens learn FREEZE2; Bishops learn HEAL3
and BLAST2. Knights' equipped spears have range1–2, Rooks' bows range2–3, and Pawns' axes range1–1.
The [first-control owner](../../research/battle22-initialization-and-first-control.md)
binds these definitions to actual equipment/spell consumers. These are ranged pressure, spell
pressure, healing and close-combat roles, not a promise of chess-like movement or a fixed action.
Bishop healing can differ from learned HEAL3 because of the candidate/rank consumer; a nominal
spell power is not a guaranteed heal or damage amount.

**Confirmed waiting rule:** there are no activation regions. All pieces have `15/15`, but only
nine use `SWARM`; the other seven can attempt their ordinary command streams. At full HP, those
nine wait until their respective count thresholds1,3 or6 are met. The count is unusually narrow:
it covers King, all three Queens, both Bishops, both Knights, both Rooks, and only the Pawn directly
below the western Bishop, at `(13,6)`. The five other Pawns do not contribute when reduced to HP0. Thus removing a different
Pawn can reduce a threat without advancing this counter; removing the counted Pawn or a non-Pawn
can admit more waiting pieces. Damaging a SWARM piece bypasses
waiting while its HP differs from maxHP; healing back to full can restore waiting below threshold.
This is neither a permanent activation latch nor a sequence of reinforcements: all sixteen start
placed and can be scheduled. Opponent-control/status conditions still govern actual AI dispatch.

### Preparation and two simple responses

**Explicit preparation assumption:** use a coherent active party of up to twelve available
members, with a viable front line, at least one ranged or offensive-spell option, and usable
healing. Bowie is alive; equipment is actually equipped and chosen spells are learned and affordable.
No particular optional recruit, promoted class, exact level, flawless equipment or arrival seed is
assumed. Fresh healing restores eligible allies' HP/MP, but does not manufacture missing roles or
revive every dead member. This is an illustrative mixed force, not a measured normal party.

1. **Inferred response — resolve a local threat before spreading damage.** Engage a reachable
   portion of the line, concentrate on an actionable threat, and keep the recovery role within useful
   reach while avoiding unnecessary clustering against area spells. Spreading chip damage across
   waiting pieces can bypass several waits; finishing a non-Pawn or the Pawn at `(13,6)` can also
   raise the counted-dead threshold. Concentration can reduce simultaneous damaged threats, but
   Bishops may heal and each counted defeat can admit more full-HP SWARM actors. No square or target
   order is guaranteed safe. The other five Pawns offer a different tradeoff: their defeat removes
   that actor without advancing the counter, but reaching them may expose the force elsewhere.
2. **Inferred response — take an available leader opportunity.** If current paths and resources
   permit a supported attack on King, focus there rather than assuming all sixteen must be killed.
   The [outcome owner](../../research/battle22-outcomes-and-desktop-return.md)
   confirms leader cleanup can remove still-living enemies from counted placement when King dies
   and Bowie survives through the relevant hook. The tradeoff is exposure to the surrounding spell,
   ranged and healing roles and King's extra turn entry. His point order is not a guarantee that
   he remains motionless; a recognizable central objective does not prove an open rushing route.

**Unknown / retained boundary:** natural first control, actual target/movement/healing decisions,
survival margins and a reliable leader approach remain unestablished. Victory, return to Desktop,
the reward announcement and actual Cotton Balloon custody are separate boundaries. The simple
design reading is selective commitment against a changing response, not a scored difficulty claim.

## Evidence review and what this pilot leaves open

The research owners above were read with the encounter, lifecycle, AI, navigation and preparation
contracts, not used as unexamined citations. The following fixture checks limit the explanation:

| Evidence inspected | Consequence for these examples |
| --- | --- |
| `sf2-battle01-region-activation-v1` in [the H3 fixture](../../../tests/fixtures/h3/battle01-region-activation-v1.json), its schema and `Test-H3Battle01RegionActivationFixture.ps1` | `(8,12)` is an injected activation boundary. Tested flags differ from triggered flags; no natural walk or permanent wave order is claimed. |
| `sf2-battle01-turn-order-v1` in [the H3 fixture](../../../tests/fixtures/h3/battle01-turn-order-v1.json) | A fixed debug-entry seed gives participation/order facts only for that seam. It does not fix the first actor in ordinary play. |
| `sf2-battle-spriteset-data-static-v1` in [the H2 fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json) and [encounter contract](../contracts/battle-encounter-definition.md#pre-contract-evidence-audit) | `rowContentTracked:false`: the general fixture proves structure, not all Battle18/22 rows. Their later source/consumer research owns the selected row facts. |
| `sf2-battle-terrain-decode-v1` in [the H2 fixture](../../../tests/fixtures/h2/battle-terrain-decode-v1.json) and [navigation contract](../contracts/battlefield-navigation.md#canonical-battlefield-state) | Static terrain decoding is accepted; corpus totals do not establish encounter-specific safe paths, visible cover or reach from a supplied party state. |
| `sf2-battle-loop-static-v1` in [the H2 fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), [lifecycle contract](../contracts/battle-control-lifecycle.md#entry-and-round-scheduling), and selected later encounter owners | Fresh healing/placement is distinct from suspension; the intermediate preserved-status mask does not replace the later equipment-derived CURSE refresh. |

The accepted Battle01 opening guide records later winning continuity; older static victory-return
research's runtime question queue is not the current whole-project frontier. Conversely, later
Battle18/22 source findings do not become runtime observations because Battle01 was observed.
The encounter owners preserve source addresses, exact lifecycle and effect limits, reproduction
commands and historical failures. This pilot runs no extraction or emulator and adds no evidence.

The Chessboard waiting counter reads HP0 in slots128–138, omitting139–143; that is the source basis
for the piece distinction above, not the outcome helper's living/placed count. Taros's LEADER
prepass reads an absent point through the following spriteset header; this source defect is retained
by his first-control owner and gives no demonstrated destination, safe square or tactical exploit.

**Unknown, nonblocking for these readings:** exact passable routes/terrain advantages for the
selected Battle18/22 setup; natural party levels/classes/resources and timing; full legal-action
coverage and tactical success rates. The responses are conditional on current legal moves, targets
and resources. None of these gaps requires a new collection or simulation to understand the three
problems. Expansion needs a separately named reader decision and evidence boundary.

Chinese synchronization is **pending**: this is the canonical English entry, with no Chinese mirror
or completed translation claim. Return to the [English design index](../../README.md#design) or
[Chinese reading entry](../../README.zh-CN.md) for the accepted thematic and chapter explanations.
