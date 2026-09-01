# Remake Capability Status

## Reading This Matrix

This matrix groups coherent runtime capabilities. It is not a pull-request chronology and does not
create evidence for the original game.

Status terms:

- **Implemented:** available through the named bounded profile and covered by owning tests or smoke.
- **Admitted prerequisite:** trusted typed data can enter the named inner boundary but is not consumed
  by a runnable product path.
- **Unsupported:** the current profile deliberately has no implementation for the capability.
- **Unknown:** accepted evidence or product behavior is still incomplete at another owner.

## Current Capabilities

| Capability | Owner and surface | Status | Retained boundary |
| --- | --- | --- | --- |
| Public-synthetic Map 3 admission and exploration | Content package, Domain movement, Application `GameSession`, Godot viewport | **Implemented** | synthetic layout and walkability only; no original collision, route, or coordinates claim |
| Public-synthetic context and interaction stack | setup/area/zone selection plus event/effect, local transition, entity/dialogue, search/discovery, and item-acquisition lifecycles | **Implemented** | project-authored identities, text, flags, items, cues, and effects; no original meaning or natural reach |
| Public-synthetic outbound map transition | Application transition lifecycle and exact runtime catalog lookup | **Implemented** | destination is a tiny synthetic shell, not original Map 21 or an original warp |
| Public-synthetic tactical battle completion | pure Domain 3-by-2 micro-loop, Content definition, Application `GameSession` lifecycle, and thin Godot projection | **Implemented** | project-authored actor/enemy/grid/damage/cues only; not Battle 01, original combat, natural admission, AI, rewards, or fidelity |
| Public-synthetic Godot and export smoke | thin Godot host and maintained local official-engine gate | **Implemented** | stable synthetic receipt only; no online Godot job and no release authorization |
| Private canonical Map 3 admission and traversal | private Content import, Application private session, Domain `OriginalMapTraversal` | **Implemented** | controlled start and semantic movement only; no natural setup/init/event or presentation claim |
| Private traversal diagnostics | typed viewport, controlled one-shot working-layout copy, current area, current block catalog | **Implemented** | project-authored diagnostic display; no natural trigger, OpenDoor, camera, layer, or visible-original claim |
| Private visual resource references | canonical import selection for palette and ordered tileset slots | **Implemented as typed metadata** | identities only; no payload, placement, cache, animation, or renderer semantics |
| Private base visual payload | sibling private Content port and immutable Application definition | **Admitted prerequisite** | decoded base buffers and palette forms remain local/in-memory; animation and final composition unsupported |
| Private visual runtime binding | Application cross-port compatibility and existing private session construction | **Admitted prerequisite** | no Godot composition, renderer, PCK, status, or smoke consumes the binding |
| Original visual presentation | no current product owner | **Unsupported** | block/tile/layer composition, camera, color display, animation, UI, text, assets, audio, timing, and final pixels |
| Continuous Map 3-through-Battle 01 milestone | Research, design contracts, future Application/Domain/Godot work | **Unknown / NOT READY** | natural route, battle admission/playthrough/victory, endpoint, private presentation, and complete acceptance remain open |
| Save/load and persistence | future milestone and save-system owner | **Unsupported / deferred** | no player-facing save, checkpoint, suspend, migration, or persistence adapter |
| Complete H4 and 8C reference parity | future layered observation owners | **Unknown** | exact reached frame, palette, audio, hardware chronology, capture conditions, and accepted tolerances remain open |

## Stable Compatibility Surfaces

Existing public-synthetic and private-local smoke observations remain compatibility surfaces until a
separately reviewed migration changes them. The legacy `SF2_MAP3_SMOKE` receipt remains byte-stable
and precedes the additive public-synthetic battle receipt. Internal refactors must preserve their
marker bytes, order, profile disclosures, path-free output, and process-cleanup result.

Future diagnostic facts should extend one bounded inspector or observation model. They do not receive a
new capability, receipt, or matrix row unless they independently cross a trust, authority, versioned-port,
or stable-observation boundary under
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md).

## Current Engineering Frontier

The bounded Godot host extraction is complete: input, public/private presentation, and public/private
smoke drivers have focused owners. Profile selection and typed public/private startup remain
intentionally inline in the composition root because they join distinct ports and results. File size
alone is not a reason to reopen those boundaries.

The next engineering frontier is product capability behind `GameSession`, with internal Application or
Content decomposition only when an owning behavior change makes it necessary. The implemented
public-synthetic battle micro-loop establishes one executable flow seam; mapping it to original Battle
01 admission, state, actions, victory, return, and endpoint remains evidence-bound and **Unknown / NOT
READY**. See [Architecture](./architecture.md).
