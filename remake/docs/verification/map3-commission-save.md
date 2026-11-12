# Map3 commission Save and Continue

The [High-rigor persistence agreement](../application/opening-session-lifecycle.md#implemented-savepoint-boundary)
owns this field recovery boundary. This observation concerns remake savepoints, not original SRAM.
Use the existing private content/tool selections, owning isolated worktree and a fresh `issue1711-*`
save namespace under the configured external save root. Preserve player saves, preferences, instances
and windows. The dedicated `engine_map3_commission_save_observation.gd` runs with `--headless` and
dummy audio; it never requests native focus or takes screenshots.

The finite consumer cases are `baseline`, `route`, `continue`, `church` and `legacy`:

1. At a declared controlled Map3 F603/F609/F610 `(32,13)` start, the accepted baseline rejects
   `save-location-unavailable` and disables system Save. This is the before-fix refusal witness.
2. `route` starts at an explicitly controlled pre-exorcism Map20 royal-room boundary, before any
   session exists. It then uses ordinary input through the zone event, exorcism, F609 transition,
   commission/F610, castle exit and Map3 church approach. This is a bounded consumer continuation,
   not a replay of New Game/Battle01 or proof of natural original reach. It opens system Save,
   asserts the slot and Cancel-first confirmation, and chooses Save and Quit. Require publication
   of the actual slot and an exited process with code 0; its pre-quit intent report alone is insufficient.
3. Start a genuinely new process with the same namespace for `continue`. Use Witch Continue and
   compare all captured progress to the saved payload, allowing only declared map-entry flag resets.
   Check the church anchor, F609/F610, selected NPC population, two followers and party. Move and
   reach Jaha's actual zone/join, confirming F3/F35 without replaying the commission.
4. A separate fresh `church` process continues the same retained slot, talks to the actual priest,
   selects Record, waits for the caller return, and writes slot2 through the common save entry.
5. `legacy` uses a byte-identical disposable copy of a pre-fix revision4 default-Map3 record.
   Continue and movement must work without rewriting that source copy. Older gameplay revisions
   still require their own compatible content; this slice adds no cross-revision migration.

Run with the locked host environment and existing editor; supply `SF2_HOST_UI_CASE`, the absolute
`SF2_HOST_UI_OUTPUT`, and, for controlled-start cases, `SF2_HOST_UI_START` under the owning worktree's
ignored `local/`. The observer's startup file uses the existing pre-session localization admission.
Use existing immutable private selections; no active-session setters or save-payload edits are allowed.
The application host observation exposes availability, detached progress and bounded entity work;
the slot view supplies actual panel states. Store terminal reports, stdout/stderr, process identity,
exit status, source-copy provenance and failed attempts in the ignored Issue handoff.

Affected engine behavior can be checked directly with the existing protected SDK launch:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore `
  --filter 'FullyQualifiedName~Map3SaveSetupTests|FullyQualifiedName~OpeningRestoreTests|FullyQualifiedName~OpeningSaveTests|FullyQualifiedName~PreVoyageSaveTests'
uv run sf2 verify adapter
```

The private family test must select the admitted world and six battle/static inputs. Public tests
use authored changed states and reject an unknown overriding setup. Follow the owner's recovery
authoring rule when story/content changes alter persistent effects, population or initialization;
the witness is not an all-maps comparator or a per-dialogue test requirement.
