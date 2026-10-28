# Battle-entry skip

## Accepted delivery contract

This design precedes implementation under [Issue 1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386).
The [session agreement](../application/session-and-programs.md#battle-entry-skip-agreement) owns
eligibility, logical equivalence and failure boundaries. Contextual Cancel means **Skip battle
intro** only at eligible entry waits. Show `GameInput.Hint(Cancel)`, respecting the existing mapping
(default Escape/X). Confirm keeps ordinary reveal/advance. Every ChoiceWait excludes skip.

The host yields between bounded batches, observes focus/errors and requires input release before
choice or combat input. Focus loss disarms acceleration. Repeated/held/stale input cannot select a
choice or affect a later entry. Session completion clears skip before round/AI dispatch, not at
an eventual player-input state.

Omitted delivery has the explicit reason `battle-entry-skip`. Existing completion reducers and
wait/generation validation own cleanup. Mount sprite resources and the actual BattleLoad board;
resource errors remain errors. Set the final camera/fade/text/portrait projection and invalidate
old callbacks through their existing tokens. Preserve already-playing short speech tails; suppress
future omitted speech/transient effects. Do not use StopSharedEffects. Reconcile the session audio
owner's cue/context/history after suppressed playback; never invoke ordinary PlayPrevious against
an adapter history which missed intermediate requests. Logical finite music services still run.

## Staged acceptance and budget

1. Small actual-engine tests compare identical request states mid-motion and mid-text, varied IDs,
   nested calls/branches, mandatory service/RNG, party/resources/flags and first battle order. Exercise
   choices, errors, stale/repeated/late completions, cancellation and an enemy-first round.
2. Measure a small authored pilot before native connected work. Initial implementation budget:
   at most 32 wait-reducer operations per host frame, each retaining the runner's 256-instruction
   limit; at most 32,768 operations per request. No unbounded host loop. Tune only with measured
   evidence. Native waits are at most 60 seconds per tool; retain and stop an owned hung process.
3. One ordinary-host connected Battle01 input/state/projection/music witness uses the actual mapped
   key, same session, prepared content and installed Godot. No screenshots, post-start injection,
   frame capture or native-wide loops. Initially budget 60 seconds startup and 60 seconds entry,
   fewer than 100 compact boundary records and under 1 MiB combined logs/report. Reassess on the
   pilot's measured step count; a budget expiry is a retained failure, never success.

From the owning checkout load `local/private-inputs.ps1` in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse configured absolute SDK/shared CLI home, worktree
caches and prepared private content. New output belongs in ignored `local/issue1386`.

Selected gates: `uv run sf2 verify engine`, `uv run sf2 verify adapter`, and on the clean committed
head `uv run sf2 verify plan --base origin/main --head HEAD`; direct document, source-size,
ownership and private-boundary checks. Focused native/test commands and measured results will be
recorded here with implementation. No full, research or emulator suites or tests of probes/helpers.

## Current acceptance boundary

Design accepted; implementation and runtime acceptance pending. The earlier UI 150-second visible
entry timeout is retained historical failure evidence, not a passed observation or a rerun request.
Independent main-gate acceptance owns integration. UI #689/PR692 and the UI gap assessment remain
separate; no contested battle UI files or unmerged UI behavior are dependencies.
