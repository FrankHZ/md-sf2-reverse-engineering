# Reference comparison inputs

This directory holds the external controlled inputs that reference comparisons supply to the common
engine. They are verification inputs, not production content and not evidence about the original
game. Production Domain/Application/Content never read them implicitly; each consumer names a file
explicitly.

The transitional `Sf2.Remake.Reference` assembly, its `reference/game` Godot host and the four legacy
test projects were retired at M5 under
[ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation).
Their last accepted state is available in Git at base `8a581a82e297ea2947cc9837e661752163d2806d`.
No parallel engine, legacy session or legacy start binding remains.

## Inputs and consumers

| File | Role | Consumers |
| --- | --- | --- |
| [`inputs/map3-programs.json`](./inputs/map3-programs.json) | Offline program and resource selection for private world preparation | `sf2tool.remake_exploration_content` |
| [`inputs/map3-opening-start.json`](./inputs/map3-opening-start.json) | Accepted R1 controlled Map 3 initialization entry | `PrivateExplorationTests`, `--private-exploration-start` native observations |
| [`inputs/map3-opening-party.json`](./inputs/map3-opening-party.json) | Connected selected R1 party: source NewGame gold and complete four-word starting item loadouts, plus controlled resources and seeds. The separate controlled R1 fixture has gold 0 and only a four-byte item projection. | Private exploration, entry and outcome tests; `SF2_PRIVATE_CONTROLLED_START` for the connected host |
| [`inputs/map3-sarah-start.json`](./inputs/map3-sarah-start.json), [`inputs/map3-followers-start.json`](./inputs/map3-followers-start.json), [`inputs/map21-guard-start.json`](./inputs/map21-guard-start.json), [`inputs/map40-seen-start.json`](./inputs/map40-seen-start.json) | Fresh controlled starts at whole source program entries and alternate live flags | `PrivateExplorationTests` |
| [`inputs/map40-intro-start.json`](./inputs/map40-intro-start.json) | Fresh controlled start before the Battle 01 intro | `PrivateBattleEntryProgramTests`, `PrivateBattleOutcomeProgramTests` |
| [`inputs/battle01-player-ready.json`](./inputs/battle01-player-ready.json) | Controlled PlayerReady party for the standalone private battle entry; accounting stays Unknown | `PrivateBattleScenarioTests`, `PrivateSourceAiTests`, `PrivateBattleEntryProgramTests`, `--private-battle-start` native observations |
| [`inputs/battle01-actions.json`](./inputs/battle01-actions.json) | The same party with explicit controlled initial accounting | `PrivateActionBindingTests` |

Engine.Tests copies the start and party files into its output. Every file declares its controlled
boundary. Initial accounting, seeds and the explicit H3 bridge remain controlled supplements with
Unknown natural producers; no value in these files admits or refuses gameplay.

## Rules

- Add an input only together with its first actual consumer, and delete it with its last one.
- Keep expected trajectories, receipt counts and route endpoints in the comparing test or observer,
  never in production admission.
- Original evidence stays in `tests/fixtures/h2` and `tests/fixtures/h3`; these inputs only select
  controlled starting state for comparisons against that evidence.
- Changes here select `engine-unit` through the planner and the public `scope` job.
