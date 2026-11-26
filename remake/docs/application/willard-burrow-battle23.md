# Willard Burrow and Battle23

## Chapter agreement

Rigor remains **High** under the [campaign agreement](remaining-campaign-risk-baseline.md) and
[Issue #1867](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1867). Continue the two
independent ordinary [Floor World saves](cotton-balloon-floor-world.md) through the actual cave,
confrontation, Battle23 and usable victory return. Include paid EGRESS, defeat, recovery and natural
reentry, complete battle/field cold persistence and an old consumer. Stop at a natural save before
the actual Creed exit; Map33 restoration, Evil Spirit and recruitment remain outside this chapter.

Reuse source-compiled programs, generic combat and finite SF2 persistence recipes. Story conditions
belong in content, not Domain. Preserve independent party, F519, Elric, custody, transport and RNG;
no debug outcomes, manual journey edits or temporary terrain barriers. Native acceptance uses normal
input and live state/projection, never screenshots. Existing shutdown failures remain recorded.

Budget one new world and contained encounter resources, a reused runner and compact receipts at
450 MiB new private output. Preparation is linear in selected input/output size, with approximately
1 GiB peak memory and under 700 MiB peak additional disk including builds. Existing free space is
sufficient for this bounded run; use task-owned H scratch if measurements approach the estimate.
Run serially, reuse registered immutable inputs, retain failures, and move the final bundle to a
stable root without a duplicate world. Reassess before expanded acquisition or a shared engine change.

Acceptance requires affected actual behavior unit tests, engine and adapter gates, bilingual ordinary
native routes with real enemy turns/AQUA2 and leader defeat, complete cold-record equality and
post-Continue movement, an old consumer, source/document/private-boundary checks, clean committed
dependency selection and actual CI. Independent acceptance and integration remain main-gate owned.

## Source owners

- [Exploration and confrontation](../../../docs/research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md)
- [Roster](../../../docs/research/chapters/southern-travel-creed/battle23-initialization-and-roster.md)
- [Initialization, AI and AQUA2](../../../docs/research/chapters/southern-travel-creed/battle23-initialization-and-first-control.md)
- [Outcomes and return](../../../docs/research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md)

## Content and rule boundaries

**Confirmed** against the pinned US source and actual admitted content: Map26 `(59,3)` requests
Map27 `(2,7)`, Up. The baseline population is Rat128, Willard129 and Rat130 at `(43,20..22)`, Left.
Their shared entity callback is a bare return, not a separate conversation or battle trigger. F523
selects the empty resident table; both setups share the return-only init. No chest/item callback grants
loot. The eight source warp records remain unchanged, including both same-map passage pairs,
the one-way `(54,2)` record and the wildcard Creed exits. A record alone does not prove traversal.

The entrance corridor leads through `(11,10)` to `(30,8)`, then to the two actual Y19 zone tiles.
F781 alone guards the 41-command confrontation. All movement/actions, waits and texts1733–1738
execute; its tail sets F423 and requests Map27. Only after script return does the caller set F781.
There is no victory or recruitment prerequisite. Natural approach reaches this scene before the
resident positions; their silent callbacks are source/compiled coverage, not fabricated dialogue.

Battle23 imports the original twelve formation positions and twelve interleaved enemies: Willard,
five Rats and six Bubbling Oozes, one region and one point. The shared definition reader consumes
ordinal Willard90, ENHANCED, extra action, resistance, prowess and raw AI bits without a new enemy
model. LEADER and ATTACKER1/2/3 use the existing point/follow/order and terrain consumers. The
visual reader adds only source Rat sprite1/palette1 and Willard sprite1/palette2 bindings. BUBBLING_OOZE
and AQUA2 already had effect and artwork bindings. Native battle records retain each deployed row.

The discovered generic correction is in enemy spell-category selection. Source compares the packed
spell word with AQUA1 (`$28`), so AQUA2 (`$68`) must consume RNG6; 2/4 select spell when physical
is also possible. Spell-only AQUA2 still draws. AQUA1 retains its shortcut. Tests vary rank, seed and
candidate availability and execute selected damage at zero MP while silenced. No map/story predicate
enters Domain; the existing spell/action/turn implementations remain shared.

Selected before/start/defeated/after scripts are empty. Common services still set F473, remove the
remaining enemies after Willard defeat, join Bowie, clear423, set523 and derive the victory return
from the first active member's position/facing. Explicit Map27 reload uses the full layout and empty
resident table, not the battle crop. It adds no local reward, after-scene, F781 repair or EGRESS write.

Escape/defeat use live EGRESS_MAP26: Map26 `(4,36)`, Up. Paid EGRESS spends MP without halving gold;
defeat restores Bowie and halves gold, rounded down. Both retain incomplete423, leave523 clear and
apply the existing F64 raft reset to Map69 `(42,15)`. Ordinary church Raise and reentry through the
same cave are the consumers; no scene replay is required when781 is set.

## Persistence

The finite `willard-burrow` recipe accepts settled positions within Map27's actual active areas and
the inherited Map26 return register. The ordinary v1 field record, including optional layer, remains
unchanged. Restore selects the saved flags' population and preserves position/facing, party,
resources, transport and RNG without running init or repairing independent423/473/523/781 states.
An absent resident lookup retains the shared source alias to physical0; it is not proof of a resident.
The existing v2 battle record preserves complete combat state. No schema bump or journey normalization
is introduced. The host names the save destination in both core languages.

## Ordinary observations and reproduction

**Confirmed** in the remake through title Continue and ordinary input. The private bundle is
`handoffs/issue1867` beneath the registered shared-input root. Its `HANDOFF.md` records exact Git
objects, CI, receipts and stable input dependencies. `Play-en.cmd` and `Play-zh-CN.cmd` open separate
player namespaces seeded from `slots/victory-en.json` and `slots/victory-zh-CN.json`. Continue selects
the ordinary save. Review runs use a fresh `-Sample` and separate namespaces, never replace a player
slot, and use no controlled start. From the bundle directory:

```powershell
./Play.ps1 -Language en -Sample review-route -Mode route
./Play.ps1 -Language zh-CN -Sample review-route -Mode route
./Play.ps1 -Language en -Sample review-battle -Mode cold -Seed ./slots/battle-en.json
./Play.ps1 -Language zh-CN -Sample review-field -Mode cold -Seed ./slots/victory-zh-CN.json
./Play.ps1 -Language en -Sample review-win -Mode win -Seed ./slots/retry-en.json
./Play.ps1 -Language zh-CN -Sample review-win -Mode win -Seed ./slots/retry-zh-CN.json
```

Both entry receipts (`en-route03`, `zh-CN-route01`) contain texts1733–1738 with423/473/523/781
clear during dialogue. At battle control423/473/781 are set and523 is clear. Normal and skipped
English entry produce identical complete battle data. All twelve enemies deploy; ordinary enemy
turns execute AQUA2. Physical attacks, allied BLAST and ordinary item use lead to real leader defeat.

The English winning history is route03 → late01 ordinary midbattle Save → escape02 paid EGRESS →
retry03 church recovery → win10. The Chinese history is route01 → win01 defeat → retry01 recovery →
win05 defeat → retry02 recovery → win07. These are independent inherited parties; no slot was edited.
The first victories return at `(38,20)`, Down with10,182 gold and `(40,19)`, Down with6,451 gold.
Both equal the first active member's final battle position plus the source crop offset `(23,14)`.
Only Bowie, Peter and Oddler field entities remain;423 is clear and473/523/781 are set.

`en-escape01` spends8 MP, keeps5,412 gold and returns to Map26 `(4,36)`, Up. `en-loss01` observes
Bowie's actual defeat,5,412 →2,706 gold and the same town return. Both retain423, leave523 clear and
reset the F64 raft to Map69 `(42,15)`. Ordinary paid Raise and cave reentry (`en-retry01`, the winning
retry histories) preserve live resources and skip the already-completed confrontation.

Both `after02` routes walk back through the first passage, leave to Floor World, reenter the empty
completed cave without a battle, and traverse both directions of the upper passage. The final
ordinary save is Map27 `(20,15)`, Right. From there **Right** reaches the wildcard exit toward
Map33 `(15,9)`, Right. `en-frontier01` observes `Unsupported / map-setup / ms-map33-flag523`:
that retained failure identifies the next consumer. The [restoration candidate](creed-restoration.md) now consumes this ordinary exit and reaches Map34; its ordinary return/reentry observations and remaining chapter limits are explicit there.

`comparisons.json` compares all decoded save `data`, not a selected-field approximation: both
battle-cold01 and field-cold01 pairs, old Battle19, Desktop secondary-layer and initial cave records
are equal after cold Continue/Save. The changed midbattle English late01 record also equals its
late-cold01 roundtrip. Both battle cold runs accept a movement segment; both final field cold runs
move from `(20,15)` to `(19,15)`. The independent F519 difference, Elric membership, character names,
prior flags, once-only chest receipts and deals survive. Combat legitimately changes HP/MP, growth,
custody, money and RNG; the complete cold comparison preserves each resulting value, including
transport, inventories, layer, queues and seeds.

## Verification and limits

The affected behavior selection (`WillardBurrowSaveTests`, `EnemySpellDecisionTests`,
`KrakenBattleTests`) passes79 tests. Adapter compilation, Python lint, design-contract and research
index checks pass. The clean committed default
planner selects engine-unit, adapter-build, private-admission-build, live-inspection-build,
research-public and conservative public-core. Its explicit `--scope engine` rejects the research
document path; that refusal is preserved, not treated as a successful plan. The shared Python edits
only extend remake content/catalog admissions; the research document corrects its obsolete
remake-consumer assessment without changing original evidence. Under the current verification
scope policy, this documentation correction uses direct source/document checks and the three
research-public commands above, not the normal/legacy research aggregate. No research full profile
is invalidated. Remote CI and the frozen head belong to the Issue/PR handoff.

Preserved failures: engine02 completed with2,659 passed,2 failed and45 private skips; both failures
were new test assumptions about absent entity lookup, corrected to the source alias behavior and
covered by the passing focused selection. An earlier AQUA test used an unsupported status bit;
its correction uses the actual Silence mask. Preparation failed once because a local `inspect.py`
shadowed Python's standard library; the renamed inspection script and prepare03 succeed. Earlier
route/after observers requested an oversized layout region or approached a sloped warp from a wall;
corrected ordinary paths succeed. Combat attempts include real defeats and bounded tactical stalls,
all retained. One completed defeat (`en-win05`) ends in the existing shutdown access violation
tracked by [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716); ObjectDB leak
warnings also remain. Do not describe those processes as clean shutdowns or erase their receipts.

The source documents establish original static facts only. Original natural reach, hardware
timing/audio parity and original SRAM remain **Unknown**. This is automated ordinary-input native
acceptance, not a claim of human play or visual review. Broader services, Map33 restoration and
recruitment remain outside this chapter.

## Prepared AQUA2 presentation

[AQUA delivery #2315](../godot/spell-presentation.md#aqua-family-playback-2315) binds
Bubbling Ooze's actual body0/palette2, seven growing bubble frames and rank2 color operands
through existing attack-scene playback. Willard is the encounter's leader, not its breath
caster; source enemy90 has no spell. The short controlled Continue/Stay witness retains the
living leader and activates an admitted Ooze beside Bowie. HD/Reduced Flash/held-speed and
incomplete-HD fallback pass with unchanged AI, zero MP cost, damage and returned control.
This supplements the natural chapter evidence above without changing admission or boss rules.
