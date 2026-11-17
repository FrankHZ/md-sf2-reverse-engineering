# Cotton Balloon and Floor World

## Chapter agreement

Rigor remains **High** under the [campaign agreement](remaining-campaign-risk-baseline.md) and
[Issue #1864](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1864). The outcome is
ordinary bilingual continuation of the two independent [Desktop victory saves](desktop-battle22.md):
receive the balloon, pass the guard, use the actual item, finish descent and landing, explore Floor
World and leave a complete cold-restorable save before the Willard cave entrance. Preserve each
history's party, gold, F519, inventory, transport, names and RNG. Map27, Battle23, Creed restoration
and recruitment remain subsequent consumers.

Reuse the shared search, inventory, typed scene, map transition and v1 persistence owners. Story
conditions stay in compiled source content. No alternate engine, comparison framework, state
injection, temporary terrain barrier or roster normalization is introduced. New private output is
budgeted at 450 MiB: one approximately 133 MB world, its contained encounter/WAV dependencies,
a roughly 36 MiB runner and small endpoint receipts. Reuse stable inputs and the managed installation;
move the final bundle to its stable location without a second publication copy. Processing is linear
in selected source/content size, native runs are serial, and no new audio capture is needed. Reassess
before exceeding this budget or adding a shared responsibility. Earlier inputs and failures remain.

Acceptance requires actual engine behavior tests, adapter build, ordinary native input/state and
projection in both languages, complete decoded cold-save equality, an old consumer, committed planner
and actual CI. Independent review and integration belong to main-gate. The source owners below
establish original static behavior; remake observations do not extend original runtime evidence.

## Source and implementation boundary

**Confirmed:** all source references use SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA ROM. The
[acquisition/departure owner](../../../docs/research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md),
[arrival owner](../../../docs/research/chapters/southern-travel-creed/floor-world-first-arrival.md) and
[resident/exit owner](../../../docs/research/chapters/southern-travel-creed/floor-world-interactions-and-exits.md)
retain command tables, callers, addresses and original-runtime Unknowns.

- Map22's F522 layout exposes chest (25,10). Search tests the facing tile; the common chest service
  grants item125 to the first eligible recipient, restores the closed flag on full-party failure,
  and returns empty on repetition. The king's announcement alone grants nothing.
- Guard136 reads F212, then F258. The first successful warning1803 moves him one tile up and sets
  temporary258 after return. F777 and current item custody are not prerequisites. Reload clears258.
- The item event matches **the player's occupied (35,24)** and item125, with any facing. Actual menu
  selection requires a held item. The source lookup scans joined members in order, slots backwards,
  and removes one found item before the scene. The existing `RemovePartyItem` performs this scan and
  no-match no-op; a later scene failure does not refund the item. The compiler binds the conditional
  lookup/removal/rejoin shape rather than inserting a map check in Application.
- `cs_599B2` executes its complete29-command sequence. Directional jumps select the same34-byte
  source action templates as the existing vertical jump, excluding their final idle jump. Inline
  duration operands do not rescale those fixed templates. All actor movement and waits execute.
- Map26 first init calls all41 commands in `cs_59E04`, including six slow descents, the three hides,
  text1739 and restored following. Only return writes779 then EGRESS_MAP26. The independent780 tail
  still executes. No terminal teleport substitutes for either scene.
- F780-clear x14 zone runs text1740/1741, the worker's eight-tile move and hide, then sets780.
  Repeated zone entry skips it. Fresh map/Continue population retains the worker lookup and parks
  him when780 is set; this differs from the same-visit scene hide.

The selected Map26 content includes all six resident callbacks, empty search/item lists and all three
source exits. Randolf16 has1742–1743; Tyrin17 has1744–1745; old man128 has1746–1747 with the fixed
Randolf name; worker129/default is a bare return. Priest133 uses the existing Church service.
Caravan134 still reaches the explicit unsupported `j_CaravanMenu` consumer; this chapter does not
claim depot/party service support. The existing [service extension work](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369)
owns that distinct capability. Map27 remains an explicit frontier, not a fabricated passable interior.

## Persistence and onward play

The `floor-world` recipe accepts settled Map26 input after779 at a supported ordinary position.
The common v1 record retains every existing field, including optional secondary layer2; missing layer
still means0. No version bump or history rewrite is needed. Reconstruction applies ordinary layout,
population and the independent780 parking tail without replaying the landing. F779-clear or unsettled
scene state cannot become an ordinary Floor World save. EGRESS_MAP26 is admitted for this recipe.

The two no-scroll local passages are (32,11) → (12,41), Up, and (12,42) → (32,12), Up. They request
MAP_CURRENT, retaining the existing population and layout. The actual onward cave trigger is (59,3),
requesting Map27 (2,7), Up. Neither the warp nor its selection tests780. At Map27, F423 selects Battle23;
a clear423 admits exploration, and no resident/ring/exit callback here sets423. The next slice begins
with ordinary Up from the stable (59,4), Right save and must implement the destination's own entry/init.

## Reproduction and remaining limits

Load the current worktree's ignored `local/private-inputs.ps1` in the launching process. Use the
[locked SDK workflow](../development-and-verification.md#locked-net-workflow), `uv sync --locked`,
`uv run sf2 verify engine`, `uv run sf2 verify adapter`, and the clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`. Compiler changes here are engine content
production; no research extractor, original observation or legacy aggregate changes.

**Unknown:** original natural reach, hardware-exact timing, raster/audio parity and original SRAM
compatibility. Native finalizer/ObjectDB issues remain separately recorded; a completed gameplay
receipt does not establish leak-free shutdown. The completed observations and stable reproduction commands follow.


### Confirmed remake observations

The registered private handoff is `issue1864`. Run `Play-en.cmd` or `Play-zh-CN.cmd`, then
**Continue → Slot 1**. Each language has an independent player namespace and starts before the cave.
The two `slots/victory-*.json` inputs remain byte-identical to the earlier Desktop victories;
`slots/landing-*.json` and `slots/floor-*.json` are ordinary Save outputs. Use fresh review names:

```powershell
./Play.ps1 -Language en -Sample review-route -Mode route
./Play.ps1 -Language zh-CN -Sample review-route -Mode route
./Play.ps1 -Language en -Sample review-cold -Mode cold -Seed ./slots/floor-en.json
./Play.ps1 -Language zh-CN -Sample review-landing -Mode old -Seed ./slots/landing-zh-CN.json
./Play.ps1 -Language en -Sample review-old -Mode old -Seed ../issue1861/slots/old-repeat.json
./Play.ps1 -Language en -Sample review-layer -Mode secondary -Seed ../issue1862/slots/near-en.json
./Play.ps1 -Language en -Sample review-frontier -Mode frontier -Seed ./slots/floor-en.json
python compare.py
```

`en-route04` and `zh-CN-route03` preserve all11 distinct dialogue IDs:1802,1803,1739,1742–1747,
1740,1741. Both actually receive item125 in Chester's available slot, search again, move the guard,
select Use through the normal item menu and consume exactly one balloon before descent. At1739,
779 is still clear and EGRESS remains22; settled landing is (26,14),779 set, EGRESS26. At1741,
780 remains clear; return hides the worker and sets780. No ring item is granted.

Both speak to Randolf, Tyrin and the wandering old man, enter/cancel Church, traverse both local
passages, complete the ring and reach (59,4), facing Right. `*-cold01` and `*-landing-cold01` compare
**every decoded data field** after a real title Continue → Save: complete party/resources, flags,
names, custody, raft, both seeds, registers and layer. Cold cave continuation then walks to (58,4).
The complete original journey data differs only in recipe/map/position/facing, naturally advanced
seeds, text cursor, EGRESS and the added212/779/780 flags. Gold stays5412 in English and18154 in
Chinese; only Chinese retains519. The `zh-CN-ring-repeat01` return does not replay1740/1741; cold
reconstruction exposes the worker's preserved lookup at raw `$7000,$7000`.

`en-old-battle01`, `zh-CN-old-creed01` and `en-secondary01` also have complete decoded equality.
The existing item behavior tests cover full-party chest rollback/retry, repeated search, wrong
occupied tile/facing, stale/absent selected inventory and joined-member removal order. New
`FloorWorldSaveTests` cover changed legal anchors, the779 admission condition and both780 tails.
The finite catalog selects nine new source texts and both resident names with unchanged control
validation and the337-name/256 KiB budgets. Source and corpus inputs remain read-only.

`en-frontier03` reads the actual `$102B` cave marker and attempts ordinary Up. The unadmitted
warp is refused without changing the playable snapshot; it does not publish an entered Map27 or
claim a visible failure message. The destination tuple and F423 admission condition above are
**Confirmed static** source facts for the next executor.

Preserved failures: `en-route02` consumes the item then stops at uncompiled `jumpRight`; the source
template binding fixes that path. `zh-CN-route01` reaches1739 with missing translation. Catalog02
stops on its unchanged expected-name arithmetic, and the subsequent route02 records unavailable
overlay; catalog03 includes the two admitted names and passes source/control/budget checks.
`en-explore02` chose an unreachable side of the walking resident; explore03 misread a completed
same-map warp as a missing route. The corrected observer reads ordinary returns and final positions.
`en-frontier01/02` expected a published failure but the existing movement refusal preserves the
snapshot; frontier03 records that actual boundary. `en-explore01` completed all intended gameplay
and saved, then hit the existing native finalizer access violation. Final full routes and cold runs
exit0; some retain the existing ObjectDB warning. No clean-shutdown fix is claimed.

To regenerate: copy the handoff's `prepare.py` and `catalog.py` into a fresh ignored
`local/issue1864` beneath the accepted checkout, load its configuration and run each with `uv run python`. Preparation resolves the pinned source from that checkout, reuses registered stable
encounter/WAV inputs and writes one world. Catalog selection uses the registered private corpus.
The runner uses the compiled adapter and source resources; it is not a buildable source checkout.
The handoff records exact commands, hashes of Git objects, completed gate results and actual CI.
