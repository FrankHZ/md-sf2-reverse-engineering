# Gameplay Overview and System Boundaries

In town, Bowie can talk to people, search for supplies and prepare the force. Out on the road,
the next encounter tests the party he brings along. Where an ally ends a move matters twice:
can they reach the enemy now, and can a friend reach them if they get hurt? After the fight,
winning, losing and choosing to retreat leave different work to do on the next visit to town.
This loop is an **Inferred** connection between original rules. The Battle02 example below shows
one possible exchange in detail.

## Follow the loop and choose a reading depth

### Exploration: moving, facing and discovering a result

Face a resident to speak, or face a searchable object to look for an item. If Bowie's four slots
are full when he opens a chest, another eligible force member can take the item. If nobody has
room, the chest closes with the item available for another attempt. You can make room and return.
In town, buying supplies uses gold and pack space. Placing a Medical Herb with a member who will
fight gives that member a recovery option on their turn. Which shops and services are open
depends on the place and the story so far. [Map Design Principles](themes/map-design-principles.md) · [Items and services](themes/progression-and-economy.md)

### The force: membership, active roles and character growth

Someone walking behind Bowie may be a travelling companion. Joining the force and taking an
active battle place are separate steps. Check who can fight, what they have equipped and how
much HP and MP they can use. Experience from actions can lead to a level gain or a new spell.
If a level raises maximum HP or MP, the increase alone leaves current HP or MP where it was.
A larger maximum gives a healer more room to restore what is missing. [Party and roster](../contracts/party/party-roster-state.md) · [Level-up rules](../contracts/party/level-up.md)

### Tactical battle: legal choices become effects and outcomes

On an ally's turn, terrain, occupied squares and movement allowance limit where that ally can
stand. From there, check the range for an attack, spell or item. You can back out during
selection and reconsider the move before committing. You can also stay. The next opportunity
to help depends on whose turn comes up and who is still standing. [Tactical Battle Loop](themes/tactical-battle-loop.md)

Consider Jaha facing the southern Huge Rat in Battle02. Use the worked example's four healthy
level 1 allies on Normal difficulty. If the rat advances to the specified square, Jaha can step
onto the plains or go around by the path. From either square, his axe can hit the rat for 5 HP
on an ordinary hit. The difference comes when the rat hits back.

| If the rat lands two ordinary hits on Jaha before anyone heals him | First hit | Second hit | What Sarah can do next |
| --- | --- | --- | --- |
| Jaha fights on the plains | 9 → 5 HP | 5 → 1 HP | From an adjacent square, HEAL 1 can restore all 8 missing HP. |
| Jaha fights on the path | 9 → 4 HP | 4 → 0 HP | Jaha has fallen. That healing opportunity is gone. |

If Jaha lands both of his attacks, he can finish the 10-HP rat before its second hit. If he
misses once, Sarah's position and turn matter. From her starting square she is out of range
of both destinations. She can move alongside Jaha and, if she acts between the incoming hits,
heal the first wound on either terrain. Casting costs her action and 3 MP. Healing at full HP
gives him no protection against a later blow. These are **Inferred** outcomes from **Confirmed**
rules for the stated loadout, single ordinary hits and no other damage. The rat can choose
another route or strike Sarah. [Battle02 first contact and turn order](analyses/battle02-first-contact.md)

### World return and persistence: continuing play is distinct from saving it

Victory restores eligible party members and can bring new people or opportunities to the field.
Ordinary defeat restores Bowie's HP and leaves the party with half its gold, rounded down.
If retreat is available, casting EGRESS spends MP, or using an Angel Wing consumes the item.
Retreat preserves the gold an ordinary defeat would cost, and leaves the victory reward to be
earned. On return, a fallen ally may need raising at a church. Paying for that service affects
what the party can afford for its next outing. Special encounters can have their own aftermath.
[Outcomes and exceptions](themes/tactical-battle-loop.md#victory-defeat-and-admitted-retreat-leave-different-state) · [Church services](themes/progression-and-economy.md#church-recovery-promotion-and-saving)

Walking again after a battle tells you where the party is *now*. Resuming in a later session
depends on what was saved and where that save restores control. The original distinguishes two
save slots and a suspended battle with its own re-entry rules. Original survival across power
loss is **Unknown** at the cited observation boundary. In the remake, an eligible save on foot records this visit, including where Bowie stands.
An eligible battle save returns to the ally who was ready to move. [Original save rules](../contracts/system/save-system.md) · [Field save](../../../remake/docs/capability-status.md#ordinary-field-time-and-save-entry) · [Battle continuation](../../../remake/docs/capability-status.md#battle-continuation-state)

For a longer journey, [Taros and the Caravan](themes/story-progression.md#worked-example-taros-and-the-caravan)
follows a discovery through a conversation, a weapon choice and a new way to travel. Other
chapters follow the [world and story](themes/story-progression.md#read-story-content-in-segments).

## Made remake choices and remaining product choices

The [vision](../game-vision.md#fidelity-and-modernization) explains how controls, pacing and
guidance fit this loop. Its [open product decisions](../game-vision.md#open-product-decisions)
cover proposals. The [design map](../design-map.md) leads to individual rules. For what has been
played or implemented, the [coverage guide](../../research/story-content-coverage.md#four-different-frontiers)
distinguishes original observations from the [current remake](../../../remake/docs/capability-status.md#campaign-feature-trace).

## A demonstrated connected slice

**Confirmed accepted private remake milestone:** the
[current acceptance owner](milestones/map3-battle01-readiness.md#accepted-current-milestone) accepts one continuous
Map 3 → Battle 01 victory → usable field return composition. Its player-facing connection is:

1. Enter the controlled admitted Map 3 state and use ordinary movement and interaction along the
   selected mandatory route. This is a product start seam, not a complete visible original New/load
   flow or unrestricted Map 3 exploration.
2. Continue through messenger, Map 19, royal/guard and castle/tower programs to natural Battle 01
   admission. Dialogue, state changes, movement, waits and before/start presentation are part of that
   route rather than a summary card that substitutes for it.
3. Control the ally turns through movement, target/action selection, confirmation and the admitted
   cancel/reselect path, while source-bound enemy decisions and the battle rules continue play.
   Reached actions, effects, rewards and their actual presentation consumers connect within the
   accepted composition.
4. Win, complete the after-battle program and world return, then reach a settled field that accepts
   ordinary input. Victory result alone is insufficient; usable return is the endpoint.

The [original acquisition owner](../../research/chapters/grans-opening/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
establishes a bounded savestate-linked winning lineage from controlled R1 to natural victory and
stable Map 57 readiness. It is not uninterrupted wall time or a natural New/load route. A separate
[return-input observation](milestones/map3-battle01-readiness.md#accepted-original-frontier) confirms an ordinary
Down input and settled displacement. Terminal original pairs remain nonresumable. These observations
support their named original boundaries; they do not turn the modern clock's trajectory into an
unchanged original golden or establish full-game presentation.

The accepted composition includes gameplay and reached presentation semantics: actual content use,
commands/effects, completion, acknowledgement and input readiness. It excludes pixel/frame/waveform/
chip and original hardware-clock equality. Private content is local only; acceptance grants no public
distribution right. The [composition review](../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)
owns exact dependencies and reproduction routes.

Historical A10/default/matrix reports remain Unavailable with `milestonePass=false`, and scoped outputs
retain their false milestone field. No fresh full executable report or corrected whole-A trajectory
was produced. The milestone was accepted through independent composition review of executed route,
parent and child evidence, not by relabeling those reports. Historical seed, HEAL timing and
provenance/review failures remain preserved; terminal internal completion is **Inferred** and its
delay **Unknown**. Those scientific limits neither invalidate the accepted boundary nor automatically
schedule new runtime acquisition.
