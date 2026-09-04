# Map 3 Playability Plan

Status: Active

## Goal

Advance the private-local Map 3 profile toward direct play with a modern high-DPI presentation while
keeping original-game evidence, modern remake policy, and unsupported fidelity claims visibly
separate.

## Current Baseline

The accepted runtime already admits the controlled private Map 3 state, authoritative working
layout, traversal policy, current area, block and visual-resource data, selected setup entity
population, reviewed local base atlas, player locomotion, a project-authored camera, one bounded
entity diagnostic, a manual synthetic battle bridge, and two exact private same-map warp records.
`GameSession` evaluates their candidate targets before ordinary passability and atomically relocates
the authoritative position, facing, area, and step without executing opaque original targets. These
capabilities do not execute natural setup, zone, door, dialogue, music, or original presentation
behavior.

The first accepted warp exits the controlled-start pocket in area record 2 and lands in area record
1. That landing has a separately evidenced roof-on-load clear which is not part of the warp itself.
The current runtime does not apply that copy, so presenting the area-1 overlay as natural gameplay
would leave a materially false roof state. The accepted entity-142 interaction stand at `(55, 17)`
also remains unreachable: the route between the two admitted warps still depends on an accepted
zone transition, two door copies, and a Sarah interaction/temporary flag lifecycle that are not yet
runtime capabilities.

## Ordered Frontier

1. Content and Application admit the exact roof-on-load clear as a distinct current-layout mutation;
   do not bundle it into the same-map warp or call it natural setup/init execution.
2. Godot projects the live current area's second layer from the authoritative working layout. Any
   interim composition and draw order are labelled as modern remake policy until original plane,
   priority, palette, camera, and timing behavior is accepted.
3. The accepted zone, Bowie-door, school-door, and Sarah temporary-flag boundaries are admitted in
   evidence order before claiming a route to the second same-map warp.
4. The accepted entity-142 interaction record gains a request/acknowledgement lifecycle without
   inventing dialogue or program effects.
5. Broader selected-setup entities gain reviewed local assets and bounded visual policies after their
   identities and visibility rules are closed.
6. Remaining natural doors, steps, warps, and setup refinements follow their accepted evidence; Map
   3 music and audio follow only after scene-use and playback boundaries are closed.

## Decision Rules

- Prefer the next player-visible capability and reuse the existing Domain, Application, Content, and
  thin Godot boundaries before adding abstractions.
- Keep original facts evidence-owned. Label authored composition, cadence, scaling, and usability
  choices as modern remake policy rather than original behavior.
- Use actual reviewed game art or music only through the local zero-remote asset repository; do not
  synthesize substitutes. Raster presentation may use nearest or supersampled 2x/4x high-DPI output,
  and HUD chrome may use reviewed SVG reconstruction where it preserves the established style.
- Keep private assets out of the main repository and public PCK by default. A local/private success
  does not authorize redistribution or an original-fidelity claim.
- Do not bypass an evidence gap with a guessed route, hidden fallback, or diagnostic behavior
  relabelled as gameplay.
