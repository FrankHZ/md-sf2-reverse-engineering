# Composed opening-loop acceptance

## Outcome and evidence boundary

**High rigor:** this is the final cross-system state/recovery acceptance for
[#1425](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1425) under the existing
[opening lifecycle agreement](../application/opening-session-lifecycle.md). Its endpoint is usable
Battle01 return and resumed play after Save, actual Quit and a distinct-process Continue.
The same ordinary New Game must include preparation, a reachable pickup/description and the real
Church caller. The accepted [branch inventory](../content/opening-branch-inventory.md) defines
required opening consumers and future boundaries; success on this route does not admit every
optional source branch or later campaign. [UI gaps](opening-ui-gaps.md) retains human/aesthetic scope.

The runtime base is accepted `cf7a511f2d909c40c74cb72f2c828319d81f9d29`, tree
`97f7693249ef33c08e18a97b21c05ed7552a26b2`. The final acceptance changes documentation only;
production, admitted content rules and the retained built host are unchanged. The observations below
are executor evidence. [PR1432](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1432) owns
independent review and integration status; executor completion does not assert main-gate approval
or close #1418.

This is remake product evidence. Original facts retain their named source/H2/H3 owners and pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` plus the registered USA input identity.
No original reset, hardware restart, menu raster, exhausted-sprite or timing claim is added.

## Retained dependencies

| Boundary reused without another full run | Accepted owner and supporting evidence |
| --- | --- |
| Genuine initialization and title replacement | [New Game](../verification/host.md#opening-new-game-observation); accepted #1420/PR1427. Only ordinary input joins Sarah/Chester; diagnostic checkpoints are not product starts. |
| Save compatibility, atomic write/recovery, ordinary defeat and retry | [Save/Continue](../verification/host.md#opening-save-and-continue); accepted #1421/PR1428. Separate native/UI and storage controls cover cancellation, overwrite, corrupt-primary recovery and fresh-process retry. |
| Preparation legality and changed valid states | [Field preparation](../verification/host.md#opening-field-preparation); accepted #1422/PR1429. Actual behavior assertions and retained native inputs cover Unarmed, Give/full-pack exchange, Drop/Cancel, field NoEffect and caller-specific poison cure. |
| Optional branches, receipts and safe future stops | [Search observation](../verification/field-programs.md#opening-search-and-receipts) and [inventory](../content/opening-branch-inventory.md); accepted #1423/PR1430. Direct-castle refusal/return, reachable chests, full-pack retry and reconstruction retain their own scope. |
| Required services | [Church](../verification/host.md#opening-church-services); accepted #1424/PR1431. Real battle-earned gold, defeat, dead Sarah, decline/retry, paid Raise, settled service Save and distinct-process Continue preserve recovery/payment. Engine cases vary levels, caps, status, funds and stale/duplicate answers. |

The live opening has no Shop caller: only excluded F609 tables contain the scoped Shop calls.
The fixed merchant obstructs the empty shop chest's opening approach. Opening docks are blocked;
the returned Map57 stair row has no opening warp marker. Future-destination preflight rejects before
committed transfer. These source/inventory and engine boundaries are reused, not replaced by
teleporting a player or changing flags to manufacture native reach. F506 and later campaign stay out.

## Reproduce the composed route

Load the owning checkout's ignored `local/issue1045/inputs.ps1` in the launching process; keep absolute
configured SDK/Godot selections and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Select the existing
Search-and-Church-complete `world-services-issue1424.json` through `SF2_PRIVATE_EXPLORATION_CONTENT`.
Reuse the installed Godot 4.7.2 .NET host, project and imports. No rebuild or asset export is required
for this documentation-only acceptance. Unset `SF2_PRIVATE_CONTROLLED_START`, provide no game-entry
argument, and use a fresh disposable namespace under the configured external save root.

The retained `local/issue1425/opening-loop.gd` extends the existing #1424/#1423/#1422 native drivers.
It reads actual state/projection and sends mapped keyboard/controller/pointer events to ordinary
controls. Existing fixtures/navigation data supply directions only; no snapshots, seeds, expected
results or later gameplay states are installed into the session.

1. Choose New Game. In Party Status → Prepare, select Unarmed and then re-equip the displayed Wooden
   Sword. Use Medical Herb and verify the source field NoEffect without payment or RNG changes.
2. Complete the actual house/Sarah/school/messenger sequence. Give Chester's herb to Bowie. Search
   the reachable school pickup, repeat it without a second grant, then read the school description
   and dismiss it to usable field control.
3. Reach the priest through ordinary counter interaction. Inspect Raise and Cure with no applicable
   target, dismiss them and leave through the actual caller. Service wait forbids Save; returning
   leaves the carried party, gold and both RNG values unchanged and field input usable.
4. Walk the actual castle/palace/Astral/tower route and finish before/load/start into Battle01.
   Read live joined actors and compare their complete item/spell words, resources and attained
   progression against the prepared field party. Execute legal movement, attack and HEAL commands.
   If defeated, preserve the real outcome; ordinary Raise/retry may continue that same game. A
   defeat is not victory acceptance. No reward, death, recovery or outcome state is injected.
5. Finish the real victory program and require settled Map57 field control. Compare the final
   battle resources/progress/custody with the returned party and verify the retained pickup receipt.
   Save slot 1 through the ordinary field menu, confirm its result, then use the actual Quit button,
   inspect its confirmation panel and submit Quit. Wait for the process to end.
6. Launch a distinct OS process with the same content/root/namespace. Continue slot 1. Compare every
   detached progress field, including all retained allies, full item/spell words, status, EXP,
   kills/defeats, levels/maxima, gold, flags/receipts, custody/Deals, RNG, save recipe/anchor and music.
   Require a new session identity, then perform a legal move and Search/dismissal. These actions must
   not replay pickup, reward, payment, joins, initialization or victory. Quit normally again.

```powershell
./local/issue1425/run-native.ps1 -Run <fresh-composed-output> -Namespace <fresh-namespace>
./local/issue1425/run-native.ps1 -Run <fresh-continue-output> -EntryMode continue -Namespace <same-namespace>
```

The existing 125.8 MB world is read in place. Serial processes, selected semantic boundary records
and compact logs suffice; reports are budgeted below 30 MB with a 1 GB free-space replan floor.
A genuine defeat/retry can extend the finite route; at most four battle attempts are allowed, with
the existing 80-mark cap and no concurrent runtime. There is no screenshot, per-frame capture, new comparator/framework or test
of a helper. Direct state comparisons inspect the recorded boundary values themselves.

## Observed result

**Confirmed, bounded remake execution:** `composed-05` completed one ordinary New Game through
preparation, pickup/repeat/description, actual Church return, Battle01 victory, Map57 Save and actual
Quit (659 mapped keys; 103,864 ms). It won its first battle attempt; the optional retry branch was not
needed in the passing run. `continue-01` launched a distinct process, restored slot 1, moved from
Map57 `(7,8)` to `(7,9)`, opened/dismissed Search and quit normally (6 mapped keys; 965 ms).
Both native stderr logs are empty. The composed process ended before Continue launched; its wrapper
had yielded after 45 seconds, so a final OS exit code was not retained. Continue reported exit 0.
Actual Quit confirmation was observed in both; no Godot/testhost remained afterward.

Direct comparison of selected real boundary values in `local/issue1425/native-summary.json` establishes:

- Unarmed changes the equipped word; re-equipping restores the starting party. Field Herb NoEffect
  preserves the party and both seeds. Give moves Chester's herb into Bowie's pack; the school pickup
  adds one item and source receipt 128. Repeat gives nothing; description and Church return preserve it.
- The same session enters Battle01 with all three joined members' carried HP/MP, EXP, kills/defeats,
  status, level/maxima, defense and complete item/spell words. Gold and custody match the field.
- Actual `battle-outcome: Victory` precedes usable Map57 with F501 and the pickup receipt. Gold 360,
  earned EXP/kills, Sarah's attained level 2, items/spells and custody survive the outcome. The existing
  [victory policy](../application/session-and-programs.md#battle01-outcome-after-program-and-return)
  restores eligible living HP/MP; this is not an equality assertion across intentional healing.
  The unjoined reserve record stays unchanged. Story/outcome execution may advance RNG; those seeds
  are compared exactly at the subsequent save/restore boundary, not frozen across gameplay.
- All 24 detached progress fields match between saved and fresh-continued state, including every
  retained ally, full words, resources/progression/status, gold, story/item receipts, battle once-flags,
  Deals, both seeds, compatibility, recipe/anchor, text settings and music. Session identities differ.
  The subsequent move and Search return preserve the party, 360 gold and pickup/victory receipts;
  no item, reward, payment, join, initialization or outcome is repeated.

Preserved attempts: `composed-01/02` stopped in preparation because the driver selected the wrong
weapon label; the corrected input uses the displayed Wooden Sword. `composed-03/04` completed genuine
ordinary defeats and therefore failed the required victory condition. Their raw reports remain;
03's boundary label says “victory-return” but its actual Map3/defeat values establish a loss, not a
pass. Earlier-healing input planning in 04 was discarded. The passing 05 run uses the retained ordinary
battle planner and real inputs, with no seed override, state injection or production correction.
These failures do not invalidate the accepted child controls or become successes through this report.

## Remaining limits and verification selection

**Unknown:** original natural optional-route and service reach/timing; positive poison/stun naturally
arising in this opening; original exhausted sprites; original SRAM/power-loss parity; physical-device
and whole-opening human/aesthetic acceptance. Promotion, curse removal, Repair, buy/sell/Deals UI,
arbitrary saves, battle suspend, F506 and later campaign are outside this outcome. #438 remains open.

Preserve the completed inherited private aggregate (1692 pass/20 fail/1 skip) and its nine remaining
controlled-start `full-fade-state` diagnostics in `local/issue1423/inherited-failures.txt`, with the
#1421–#1424 handoffs. Those starts lack required display initialization. This ordinary product
route neither changes their fixtures nor turns those failures into passes. Accepted child runtime,
behavior and CI evidence remains valid by dependency; no blanket legacy aggregate is repeated.

For this exact documentation scope, use direct diff/ownership, links/anchors/tables/fences and
private-boundary checks, plus the new composed native observation. On the clean committed candidate,
run `uv run sf2 verify plan --base origin/main --head HEAD`; report its actual selection and exact-head
CI in the handoff. A documentation-only CI skip is not a runtime pass. Main-gate independently reviews
the evidence and performs its necessary focused observation before integration.
