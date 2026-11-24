# L0 · Remake Vision

Preserve the experience of leading the Shining Force through a connected world, preparing a party
with earned resources, making tactical choices in battle, and returning to a world that reflects the
outcome. Encounters draw their meaning from who joined, what the party carries, and which choices
are available when control returns. The [gameplay overview](synthesis/gameplay-overview.md) explains
how these activities depend on each other.

## Fidelity and modernization

The USA retail release is the reference for recognizable progression, encounter rules, costs and
consequences. The first accepted private milestone established the product precedent of an ordinary
playable route, mandatory scene and battle effects, and a usable field return. Its fidelity tier
accepts gameplay and presentation semantics without hardware-exact reproduction. Use the applicable
contract for each later slice and record intentional changes at their decision and acceptance
owners. [First milestone profile](../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment) ·
[Current campaign agreement](../../remake/docs/application/remaining-campaign-risk-baseline.md)

The following decisions define where the remake can change delivery while keeping the underlying
choices and consequences legible:

- **Readable, consistent controls.** The accepted visual language uses familiar icons with persistent
  localized labels and visible focus, with the correct costs and refusal reasons where a player
  needs them. The first milestone requires manual play on default keyboard A and treats keyboard C
  as supplemental. English and Simplified Chinese are supported within the documented game scope.
  [Visual design](../../remake/docs/godot/ui-design-language.md) ·
  [Keyboard scope](../decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope) ·
  [Game localization](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1468)
- **Control over reading and pacing.** Instant or reveal-all text omits speech for skipped reveals
  and preserves an existing speech tail. Reveal alone leaves acknowledgement and gameplay time
  untouched. Battle-entry skip is an explicitly bounded option that runs required logical work and
  preserves the handoff to play. Battle Speed changes eligible message delivery while preserving
  combat results. [Fast text](../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy) ·
  [Battle-entry skip](../../remake/docs/application/session-and-programs.md#battle-entry-skip-agreement) ·
  [Battle Speed](../../remake/docs/godot/presentation.md#battle-speed-and-selective-messages-1650)
- **Optional help and acceleration.** Pre-voyage guidance offers a disclosed objective and, when
  known, a destination. Players can turn it off. Holding Accelerate (Shift by default) speeds up
  eligible exploration and battle at the configured multiplier, 1.5× by default and up to 3×.
  Releasing it restores normal speed. Mandatory work and player choices retain their timing boundaries.
  [Guidance design and scope](../../remake/docs/godot/objective-guidance.md) ·
  [Held acceleration](../../remake/docs/godot/modernization-design.md#held-15-acceleration-1619)
- **Explicit time and waiting.** Finite music distinguishes logical completion from actual playback
  completion, with no logical opportunities added by host delay. This can change the shared RNG
  history, so compare the local rules from matched states and require the admitted route effects.
  A player Wait is available at consumers with the required caller/service/phase binding.
  [Finite music decision](contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy) ·
  [Evidenced waits](contracts/milestones/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits)
- **Continue through explicit save boundaries.** The first milestone proved playable return under a
  narrower profile. The later session agreement supports bounded field and battle save points with
  explicit eligibility and recovery, plus a usable return to control after Continue.
  [Field save eligibility](../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes) ·
  [Battle continuation](../../remake/docs/application/battle-save-and-continue.md)

Reached private content can be used within the accepted local profiles. Public content requires
project-authored or suitably licensed assets. The campaign goal and current support each have a
separate owner. [Content and redistribution](../../remake/docs/content/profiles-and-trust.md#redistribution) ·
[Campaign agreement](../../remake/docs/application/remaining-campaign-risk-baseline.md) ·
[Capability status](../../remake/docs/capability-status.md)

## Open product decisions

Remote town services and a read-only battle overview are proposals. Approval and any wider save
permission remain **Unknown**. Guidance outside the authorized opening-through-boarding scope,
wider art modes, release scope and any intentional rebalance require their own product choices.
[Local services and battle overview](../../remake/docs/godot/modernization-design.md#local-services-and-battle-overview-1543) ·
[Guidance #1578](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578) ·
[Art #1699](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1699) ·
[Release #1825](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1825)

Follow the [design map](design-map.md) for the gameplay, system and behavior owners.
