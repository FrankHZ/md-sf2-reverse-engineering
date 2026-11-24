# L0 · Remake Vision

Lead Bowie and the Shining Force on a journey in which something you learn or find in town can
change what your party can do several battles later. At Ribble, using the Wooden Panel while
facing the tree from the right spot opens a passage. The Achilles Sword still lies in a chest,
and someone needs room to carry it. Later, Rohde can recognize the sword even in a reserve
member's pack. You can turn down his proposal. If you agree, he accompanies the party and the
encounter with Taros becomes available. In that fight it matters who actually wields the sword.
Bowie can strike Taros with it equipped. Winning for the first time brings the Caravan within
reach, and boarding it opens a new way to travel. [Taros and the Caravan](synthesis/themes/story-progression.md#worked-example-taros-and-the-caravan)

That is the experience the remake aims to preserve. A discovery gives the player something to
do with a familiar place. A companion can give the party a reason to return there. Preparing
for a fight means deciding who carries, equips or uses what the party has earned. The payoff for
victory can be a place to go under the player's own control. The Taros example connects original
rules under their stated conditions. The [gameplay overview](synthesis/gameplay-overview.md)
follows the smaller choices from one outing to the next.

## Fidelity and modernization

The USA retail game sets the reference for who can act, what an action costs, what can go wrong,
and how play resumes. The first private remake milestone follows Bowie from exploration through
the first battle and back to a field where he can walk again. Its fidelity target includes the
necessary scenes, battle effects and return of control. Hardware-exact timing, pixels and audio
fall outside that target. [Milestone decision](../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)

The remake gives the player more control over how to read and play that journey.

- Read a command before choosing it. Icons keep their localized labels, the current selection
  stays visible, and the interface explains costs and refusals where they matter. The documented
  game scope supports English and Simplified Chinese. The first milestone uses default keyboard
  layout A for manual play, with layout C as a supplemental option. [Visual design](../../remake/docs/godot/ui-design-language.md) · [Keyboard scope](../decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope)
- Take time to explore or ask for a lead. Optional guidance before boarding the ship shows a
  disclosed objective and its destination when known. Hold Accelerate (default Shift) to speed up
  eligible travel and battle, then release it to return to normal speed. The default multiplier
  is 1.5× and the maximum is 3×. [Guidance](../../remake/docs/godot/objective-guidance.md) · [Held acceleration](../../remake/docs/godot/modernization-design.md#held-15-acceleration-1619)
- Read dialogue at your own pace. Revealing a line still leaves it waiting for your acknowledgement.
  Where battle-entry skip is available, it carries out the required events and hands control to
  the player. Battle Speed changes eligible messages while preserving combat results.
  [Text and speech](../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy) · [Battle entry](../../remake/docs/application/session-and-programs.md#battle-entry-skip-agreement) · [Battle Speed](../../remake/docs/godot/presentation.md#battle-speed-and-selective-messages-1650)
- Look around before committing a turn. During an ally's movement, backing out of a tentative move
  lets you inspect terrain and known units. From empty ground you can open the battle Map for a
  wider view. Closing inspection returns to the same ally without spending the turn.
  [Battle inspection and Map](../../remake/docs/godot/presentation.md#battle-inspection-and-on-demand-map-1633)
- Put down the game and return to it. When the party is on foot in a supported field scene and
  ready for input, Save records the current visit, including the party's position and the people
  present. In a supported battle, save when an ally is ready to move at their original square.
  Continue resumes that turn. Finish the scene, land the vehicle or let the action resolve
  before trying to save. [Field snapshots](../../remake/docs/application/opening-session-lifecycle.md#unified-settled-field-snapshots) · [Battle continuation](../../remake/docs/application/battle-save-and-continue.md#shared-snapshot-boundary)

For the timing detail, revealing text skips speech for skipped letters and lets an existing
sound finish. At supported prompts the player can explicitly wait. A delay in music playback
adds no gameplay time. This timing choice can yield a different random sequence from the
original. [Player waits](contracts/milestones/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits) · [Music timing](contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy)

## Open product decisions

Accessing town services remotely is a proposal. Whether to offer that shortcut remains **Unknown**.
Broader guidance, wider art coverage, release scope and any intentional rebalance need their own
decisions. Private content is for local use. A public build requires project-authored or suitably licensed assets. [Proposed town access](../../remake/docs/godot/modernization-design.md#local-services-and-battle-overview-1543) · [Art direction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1699) · [Content and redistribution](../../remake/docs/content/profiles-and-trust.md#redistribution)

The [design map](design-map.md) leads from the vision to individual rules. The [campaign agreement](../../remake/docs/application/remaining-campaign-risk-baseline.md)
and [capability status](../../remake/docs/capability-status.md) describe which parts of the journey the current remake supports.
