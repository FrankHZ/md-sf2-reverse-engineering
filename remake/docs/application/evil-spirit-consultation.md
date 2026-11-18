# Evil Spirit consultation and return to Creed

**Confirmed** in the private remake candidate: both unchanged shrine saves continue through all73
messages, shared special graphics, sixteen effect actors and eight complete cyclic orbits. Both
journeys regain control with caller F784/F802, return normally to Map33 and save before recruitment.
Four cold Continue → Save comparisons match their complete decoded data. Fresh shrine reentry is
empty, crosses Y13 without replay and returns normally. Independent main-gate acceptance is separate.

## Agreement and provenance

Rigor remains **High** under the [remaining-campaign agreement](remaining-campaign-risk-baseline.md)
and [Issue1875](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1875). This chapter starts
from the unchanged bilingual [restoration shrine saves](creed-restoration.md), uses ordinary title
Continue and walking, completes the consultation, returns through the ordinary passages to Map33,
and leaves a cold-restorable save before any recruitment decision. Party resources, item custody,
depot, transport, independent F519 histories and both RNG words belong to each inherited journey.

The accepted [entry](../../../docs/research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md),
[consultation](../../../docs/research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md)
and [mansion return](../../../docs/research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md)
owners bind the USA retail ROM and SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`.
Their selectors, resident allocation and instruction order are **Confirmed** static evidence.
Remake observations do not upgrade original natural reach, hardware timing, graphics/DMA consumption
or human visual acceptance; those remain **Unknown**. Recruitment Yes/No, Fairy receipt/following
and Oddler's departure belong to the next chapter.

## Content and shared graphics boundary

The [selection](../../content/private/pre-voyage-opening.json) admits Map33's F784 population and
consumes Map34's existing Y13 zone callback. F784 alone guards consultation. The compiled caller
calls the complete `cs_5B6C0`, then writes F784 followed by F802 after return. No engine predicate,
checkpoint edit or replacement flag-only program supplies completion. Source `nop` has no operation
in this logical scheduler; no original CPU-cycle claim follows.

The special actor remains logical130, mapsprite252, physical47 and shared graphics slot32. Logical131
remains the parked BEASTMAN2 speaker. Neither a Zeon text nor a shared tile request creates another
actor or replaces its logical/mapsprite identity. The three local graphics helpers become the same
generic slot-resource operation. Their15 ordered requests select original, alternate and Zeon tile
payloads; the retained Evil Spirit palette supplies all three, as the source helper does not reload
palette4. ROM spans start at `$26912`, `$26DB8` and `$27284`; the existing72-tile/two-pose layout is
reused. Both ordinary follower allocations remain ahead of the non-special residents.

All sixteen effects retain separate logical IDs133–148 and physical records. Eight receive the
five ray groups; eight receive cyclic orbit actions. The shared labels fall through the other
quarters and branch back to the loop root. The compiler retains that complete cycle for each entry;
ordinary observations record four distinct destinations for every orbit and its retained loop after
hide. Existing asynchronous action installation and
waited actor semantics remain intact. Hide preserves action pointers/flags/sprite state, removes
lookup aliases and current-list membership, and parks the record at `$7000`. The entity service skips
those dormant records before motion or script dispatch. Hiding is not an all-actor completion join.
The source's final follower requests remain separate from recruitment.

The Map07 palette helpers lower to a generic color-lookup request and COPY restoration. The compiler
consumes the pinned24-byte table and weighted/rounded source formula. The engine snapshots current
colors, selects BASE, replaces COPY and interpolates over32 logical services. The adapter projects
that lookup on world draws through one reusable texture/material; UI children retain their existing
materials. Flash/fade services retain their existing modern
presentation boundary. A true map palette reload clears the lookup, while CURRENT255 passages retain
it. This is deterministic modern presentation, not an observation of original CRAM, VInts or DMA.
`ac_transparency` uses the existing flags-B bit128 operation. Normal exploration rendering alternates
its visibility on the two-tick modern cadence; Reduced Flash keeps the actor visible.

## Saves and return

The `evil-spirit-shrine` recipe admits settled positions on either side of F784 without inventing
F523/F783/F802 prerequisites. A fresh F784 entry selects no residents; the leader and ordinary
followers remain. The `creed-mansion` recipe admits the F784 stage. Cold reconstruction uses the
selected seven-resident population and the source init positions for each still-unjoined offer actor,
without replaying consultation/restoration or executing an offer. Goliath, Creed and Fairy retain the
F784 population's base positions; earlier F774/F783 recipes retain their own reconstruction.
The [recruitment chapter](creed-recruitment.md) extends these recipes with the actual settled room
population, current follow relationships and live layout progress. F785/F787 no longer cause the
previous temporary refusal. Fresh map entry still runs the original population selector, so the
current Fairy130 and the next population's Fairy158 remain distinct. Old pre-recruit slots keep
working without a scene field; older fabricated later-stage records without their required local
progress remain unavailable. The retired short stage-reason key and routing have no remaining
consumer. Recruitment observations and later cold comparisons belong to that chapter.

The existing [skip boundary](desktop-arrival.md) applies only to entering battles. This exploration
scene requires its normal performance. No exploration skip or skip-as-flag-write path is added.

## Audio dependency and acquisition limits

MUSIC_WITCH retains TimerB193 and its accepted external WAV. Ordinary portrait/acknowledgement and
Peter/Beastman speech require commands65/67/74 in that context; consultation additionally requests
rumble93 and battlefield death116. The registered library lacked those C1 variants. Five bounded
exports reuse the existing finite original-driver observer, source/ROM sound readers and
`prepare_audio`/`prepare_visuals`; no capture or comparison platform is introduced.

Each export dispatches at243, validates its active channel cursor against the named source stream,
observes channel termination and60 subsequent quiet frames, restores owned emulator memory, and
exits natively0. Shared installation and canonical ROM checks pass before/after. The total raw WAV
size is20,606,740 bytes. Candidate samples are the exact captured intervals with441 unchanged samples
on each side; no normalization, resampling, waveform replacement or arbitrary truncation occurs.

| Command | Inactive / tail frame | Retained sample interval, half-open |
| --- | --- | --- |
| 65 | 261 / 321 | 179205–194680 |
| 67 | 280 / 340 | 179115–209358 |
| 74 | 248 / 308 | 179223–187506 |
| 93 | 380 / 440 | 179211–282739 |
| 116 | 286 / 346 | 179114–214436 |

All five use existing runtime-WAV admission to preserve world-document headroom. Speech69 and76
retain their accepted single-finite fallback recordings. They are modern finite reuse, not C1 or
hardware-pitch equality; no export was made for those optional variants. Source contracts,
launch/config/native/channel/tail receipts, capture/candidate identities and PCM equality checks stay
in the registered private chapter handoff. Natural scene sound and hardware-phase equality remain
**Unknown**.

## Reproduction and verification

The private `handoffs/issue1875` bundle owns the candidate, unchanged seed copies, ordinary saved
outputs, small recipes and native receipts. Use fresh disposable save namespaces. The configured
checkout's `Run.ps1` consumes title Continue and semantic keyboard input; its `route`, `cold`, `revisit`
and `old` modes select ordinary seeds. It reads running state/input/projection/errors and exits via
product Menu → Quit. No screenshot, arbitrary checkpoint or flag injection is used. The stable
handoff gives exact commands and separates native exit, ObjectDB warnings and gameplay results.

Both final headless scene routes and all four cold runs exit natively0 through the product menu.
The English route separately runs on an isolated, invisible Windows desktop with the actual
Compatibility renderer: OpenGL3.3, NVIDIA GeForce RTX3080Ti. Its shader/material and reusable lookup
texture mount, all scene steps and the ordinary return complete without shader/render failure, and
Menu → Quit exits0. The native log retains the NVAPI profile-already-in-use diagnostic and two
ObjectDB warnings. This is a renderer consumer observation; human art/fidelity acceptance remains
Unknown. The empty-room revisits and old Battle23/Desktop continuations also use Menu → Quit0.
All party records, joined/active state, packs/equipment, gold, once receipts, Deals, depot, transport
and names remain equal across the scene. Both RNG words start at the inherited values, advance under
ordinary play and equal the live state at Save; cold continuation preserves every data field. English
retains F519 clear and Chinese retains F519 set. The older slots use only the existing absent-to-empty
depot compatibility default.

Actual-engine tests cover shared slot identity and party preservation, asynchronous palette request
continuation, intermediate COPY restoration, hidden cyclic-action dormancy/reactivation, palette
reload versus CURRENT255 preservation, and save admission with varied caller flags. Affected existing
save/follower/map-transfer assertions and the adapter supply their own scope. Verification tools are
run directly; no tests of compilers, observers or recipes are introduced.

Completed failures remain in their original receipts. The first route lacked C1 window feedback;
the next reached unsupported `ac_transparency`; a later attempt exposed a palette instruction that
did not advance its cursor; the full consultation then reached an unlowered Map33 `nop`. The completed
engine aggregate found one superseded F784 save-refusal assertion; the owning assertion was updated
and its affected tests rerun. None is relabeled interrupted or replaced by another aggregate.
Retained failures also include a full per-draw observer deadline and the incomplete shared-label
orbit lowering caught by the direct data check. The final actor-resource observation uses existing
compact projection selectors without a full per-frame resource graph.
The handoff records exact failing nodes, corrections, scoped checks, native results, clean committed
plan and actual CI. [Issue1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
and the [finalizer owner](../verification/map3-commission-save.md#later-finalizer-faults-and-ordinary-quit)
retain their independent open native-exit risk. This chapter changes no audio/theme/root lifecycle.
