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
entity diagnostic, and a manual synthetic battle bridge. These capabilities do not execute natural
setup, zone, warp, door, dialogue, music, or original presentation behavior.

The controlled start at tile `(56, 3)` is confined by the currently admitted traversal state to a
22-cell pocket in area record 2. Neither area record 1, which is the only area with a nonzero
second-layer offset, nor the accepted entity-142 interaction stand at `(55, 17)` is reachable from
that pocket. A visible overlay or interaction slice therefore must not invent a teleport or claim a
natural route.

## Ordered Frontier

1. Research closes the exact natural setup, zone, or warp transition that exits the controlled-start
   pocket.
2. Content and Application admit that transition, and `GameSession` applies it atomically while
   preserving the existing state-ownership and fail-closed boundaries.
3. Godot projects the live current area's second layer from the authoritative working layout. Any
   interim composition and draw order are labelled as modern remake policy until original plane,
   priority, palette, camera, and timing behavior is accepted.
4. The accepted entity-142 interaction record gains a request/acknowledgement lifecycle without
   inventing dialogue or program effects.
5. Broader selected-setup entities gain reviewed local assets and bounded visual policies after their
   identities and visibility rules are closed.
6. Natural doors, steps, warps, and setup refinements follow their accepted evidence; Map 3 music and
   audio follow only after scene-use and playback boundaries are closed.

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
