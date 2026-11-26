# Opening Session Lifecycle and Feature Closure

## Agreement and decision

**High rigor:** initialization and durable recovery cross Content, the one Application session,
party resources and the host; an apparently successful load can silently lose progress. This owner
defines the implementation contract for [#1418](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1418)
and design [#1419](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1419), with the
slot/name/initial-save extension [#1443](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1443).
New Game, bounded savepoint persistence, field preparation, Search and required Church services are
implemented. The [composed acceptance](../evidence/opening-loop-acceptance.md) joins these consumers
through Battle01 return and fresh-process Continue; independent integration belongs to main-gate.
This product boundary does not establish original SRAM parity.

The outcome is New Game → Map3 exploration → Battle01 → usable victory/defeat return → Save →
process exit → fresh process Continue. Required preparation, pickups and opening services share
that same party. Existing Battle02/Yeel consumers are shared-state regression dependencies, not
an extension of this endpoint. The [capability matrix](../capability-status.md) owns delivered
behavior; [UI gaps](../evidence/opening-ui-gaps.md) owns presentation acceptance.

Use **manually selected local slots**. Field Save and Church Record capture the
[settled field snapshot](#unified-settled-field-snapshots). Continue restores v6 at the physical
player's saved location. Older anchored and scene-bearing records keep their admitted legacy
landings.

This persistence policy is a remake choice and does not reconstruct original SRAM. The
[save-system contract](../../../docs/design/contracts/system/save-system.md) confirms two logical
slots and bounded in-process original helpers, but leaves physical restart/power-loss and normal
player save UX Unknown at that earlier bounded evidence surface. The accepted
[battle interaction baseline](../godot/battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
separately confirms original USA battle Quit/save. The authorized remake battle Save/Continue
[battle persistence owner](battle-save-and-continue.md) owns the implemented stable battle commit
and fresh-process boundary. This document owns the field snapshot and legacy recipes. No arbitrary story/battle snapshot,
autosave, cloud save, SRAM import,
copy/delete UI, migration framework, whole-party naming, promotion or Battle03 ending is promised here.
New Game selects a slot and writes only after final confirmation, including explicit occupied-slot
replacement consent. Ordinary field Save remains explicit. Quit warns about unsaved
progress and offers cancel; it does not secretly autosave.

Current acceptance endpoint: the composed opening loop through usable Battle01 return and restart. Main-gate
owns Issue dispatch, exact production ownership and integration. Reassess High
rigor at the persistence review and final restart acceptance; ordinary inventory/UI slices can
use Standard once their state boundary is fixed. No unresolved aesthetic choice gates this design.

## In-game Load (#2286)

The existing High-rigor recovery agreement also governs system Menu → Load. A settled field
must pass the current field snapshot offer and menu-entry settlement; battle must pass the
existing player Movement save offer. Story/text, services, unfinished movement, action selection,
playback, automatic turns and unsupported profiles retain their existing explicit refusal.
This is a replacement entry, not arbitrary mid-script suspension or a new save format.

The host compares the current session/revision with its last successful save/restore. Dirty
progress requires a Cancel-first warning before opening the shared history. Cancellation and
failed selection retain the paused session and return focus to Load. A clean journey opens history
directly. Ordinary field clocks may make a restored journey dirty under the existing revision rule.

Load uses the same freshly read/version-checked slots, compatibility validation, explicit backup
recovery, detached restore, prepared content and candidate view/audio preflight as Continue.
Only a successfully prepared candidate releases the old scene/audio ownership. It mounts once,
without title or Witch dialogue, and rearms input only after held controls release. A prepared field
uses a temporary node name until the old field is released, then takes the ordinary stable view name.
Backup promotion remains the only Load operation that writes; it is explicit and confined to the
selected store. The [host observation](../godot/host-ui.md#in-game-load-observation-2286) owns bounded
ordinary-input reproduction and failures. #2287 input defaults and global UI changes remain excluded.

## Entry confirmation responsiveness

**Standard rigor** for [#1605](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1605):
the bounded outcome is responsive title New Game/Continue confirmation, slot/name return and
failure recovery, with continuous entry music. The existing High-rigor save/recovery agreement
above still applies. The current host reuses admitted content through
[startup preparation](#startup-content-preparation-2283); it does not cache slot availability or
restored sessions. Save format, gameplay rules and finite-track policy remain unchanged.

The host presents loading feedback while awaiting detached content preparation and slot validation.
Slot-list entry reuses the prepared definition while its selected inputs remain unchanged; changed
inputs require fresh admission. New Game still performs full party/name/seed initialization, and
slot re-read, restoration, publication and explicit backup recovery retain their existing validators.
Background work touches detached content/session data and disk only; display binding, nodes, audio
and publication to the live host remain on the Godot thread. One entry operation runs at a time. Repeated confirmation is
ignored. Cancel acknowledges a return request immediately and returns after the in-flight read;
publication/recovery cannot be cancelled halfway through their atomic disk operation. Window-close
requests visibly wait for that operation before applying the existing quit policy.
After waiting, close releases the entry/name/slot presentation and prepared replacement resources
before invoking that policy. An existing session retains its identity, revision and unsaved-progress
confirmation; cancelling quit returns to that original playable session, including after a completed
replacement save was published without being mounted.

Success and exception results share the original host/tree-lifetime, panel and session boundary.
An exited or replaced owner receives no late UI result or input rearming. Window recovery retains
the original pending result until its cover closes; quitting discards it after detached work ends.
Settings and window recovery explicitly pause entry audio. Slot/name/loading panels disable entry
gestures and animation while music continues. Actual field entry still makes its legal music change.
The [presentation observation](../godot/presentation.md#entry-confirmation-observation) owns the
reproduction, measured boundaries and remaining startup cost; software playback does not establish
human listening acceptance.

## Current feature map

The feature map reflects the accepted entry, persistence, preparation, Search and Church consumers;
exact runtime objects and the composition boundary are in the [acceptance owner](../evidence/opening-loop-acceptance.md).
“Confirmed structure” below is a code/document claim, not new runtime or original-game evidence.
Implemented, standalone-only, missing integration, missing behavior and Unknown are distinct.

| Surface | Current state and owner | Closure disposition |
| --- | --- | --- |
| Entry/initialization | **Implemented:** ordinary title/New Game uses admitted static ally starts, growth and complete fresh progress. Title Continue attaches the saved visit from a settled field snapshot or reconstructs a supported older record. Explicit authored/controlled entries remain diagnostic. | #1420/#1421; [content/start boundary](../content/profiles-and-trust.md#opening-new-game-admission), [native entry](../verification/host.md#opening-new-game-observation) and [field snapshot attachment](#implemented-field-snapshot-cutover). |
| Persistence/Continue | **Implemented, bounded:** ordinary field Save and Church Record write a unified v6 snapshot of the settled walking visit. Title Continue restores its live landing and keeps field v1/v3 readers. Selectable local save history, detached validation and explicit compatible-backup recovery are shared with battle saves. | #2049; [field cutover](#implemented-field-snapshot-cutover), [legacy readers and observations](#implemented-savepoint-boundary) and [battle eligibility and Continue](battle-save-and-continue.md#implemented-ordinary-host-boundary-c). Field capture excludes riding, unfinished work and actor work without an admitted binding. |
| Walk/Talk/party inspection | Implemented in the connected route: semantic movement/interaction and read-only roster/details. [PartyDetailsQuery](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs) projects carried state; absent loadout/accounting stays unknown. | Keep the same authority. A readable unknown label is acceptable for optional presentation metadata, not for gold/EXP needed to transact or save. |
| Equip/Give/Drop | **Implemented, bounded:** Party Status → Prepare changes the carried party through revision-checked field commands, shared equipment/word rules and existing custody. | #1422; [field boundary](#implemented-field-preparation). Source class/slot legality, full-pack exchange and confirmed Drop are explicit; private battle command capabilities stay separate. |
| Field Use/magic | **Implemented, bounded:** original NoEffect for Medical Herb/Healing Seed and HEAL. Antidote and DETOX1 cure poison with caller-specific payment. Fairy Powder clears poison and paralysis and spends the selected holder's slot. | [Field policy](#implemented-field-preparation). The earlier “field HP recovery” wording was a planning assumption, corrected from the pinned field caller; no modern healing deviation or battle EXP/RNG is introduced. |
| Search/descriptions/chests/ground items | **Implemented, bounded:** explicit faced-cell Search consumes validated opening descriptions and placements, shared carried inventory and source item flags. Full packs retain the reward; revisits and savepoint reconstruction retain opened chests. | #1423; [Search boundary](#implemented-opening-search) and [branch inventory](../content/opening-branch-inventory.md). Original input/presentation cadence remains Unknown. |
| Receipts/Deals | [BattleItemCustody](../../src/Sf2.Remake.Domain/Battles/State/BattleItemCustody.cs) owns authored battle drop once-flags and Deals; carried through field party. Null means unobserved, not empty. | **Distinct authorities:** field receipts use the source placement flag in durable story progress; battle custody retains its own drop flags/Deals. Field Search neither adds battle once-flags nor replaces null custody. Church uses the same carried resources; see its service boundary below. |
| Church/shop | **Implemented, bounded:** Map3 event13 opens typed Church Raise/Cure and a common Save entry after caller return. | #1424; [service boundary](#implemented-opening-church). No shop caller is admitted in the bounded opening phases; buy/sell remains deferred. |
| Battle01/outcomes | Connected before/load/start, combat, growth, victory Map57 return and ordinary defeat Map3 return are bounded implemented capabilities. | Preserve reward/join/flag order and once-only outcome commits. The [composed observation](../evidence/opening-loop-acceptance.md) carries prepared/acquired items through victory and fresh-process resumed play. |
| Later encounters | The [campaign owner](pre-voyage-campaign.md) records the connected Battles01–07 route and first damaged-ship control; the [battle persistence owner](battle-save-and-continue.md) records its settled Movement saves. | These later deliveries have their own acceptance boundaries; they do not extend #1418's composed opening observation. Shared records must remain compatible or reject explicitly. |

### Reachable branch disposition

Use the [opening chapter](../../../docs/design/synthesis/chapters/grans-opening/opening-through-battle01.md)
and [Map3 optional interaction inventory](../../../docs/research/chapters/grans-opening/map3-optional-interactions.md)
as the source inventory, then check the current compiled program at each reached branch. Mainline
success is not all-branch closure. The bounded inspection is of these selected maps, not every game map.

| Location/phase | Established implementation versus remaining closure |
| --- | --- |
| Map3 house, Sarah, school stairs, Zone7, messenger and Zone8 | Connected programs and joins exist. Save anchors must distinguish pre-messenger and F603-set phases; no reload may replay JOIN or grant starting items. |
| Map3 optional NPCs/default events, descriptions and two item placements | Source table inventory is established; natural optionality/reach of every branch is **Unknown**. Lowered ordinary dialogue and typed Search are connected. ChurchMenu uses the typed service consumer and returns through its actual handler. The [branch inventory](../content/opening-branch-inventory.md) separates executable consumers, future boundaries and original Unknowns. |
| F600-clear direct castle approach / `cs_51454` | The old blanket `moveNextToPlayer` gap is stale: [compiler](../../../src/sf2tool/remake_exploration_programs.py) emits move/waits/facing; [ProgramRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) executes `MoveEntityNextToPlayer`. **Confirmed bounded remake observation:** #1423 direct-castle input reaches the existing movement consumer and returns; its [inventory](../content/opening-branch-inventory.md) retains original reach limits. No fabricated F600 or duplicate opcode is needed. |
| Map19 castle and Map20 palace/Astral revisit | First/repeat visits, refusal/re-prompt and follower variants have bounded acceptance in [session programs](session-and-programs.md#castle-palace-astral-and-tower). Settled control in the admitted rooms supports ordinary Save and [cold Continue](#granseal-castle-save-and-continue-observation). Other NPC branches and unadmitted setups retain their separate coverage. |
| Map21 guard, tower and Battle01 | Accepted conditional guard and battle admission remain authoritative. The field-save path rejects during guard motion, entry or combat; it never completes the program or sets unlock flags. Battle Save uses the [implemented separate path](battle-save-and-continue.md#implemented-ordinary-host-boundary-c) at settled player Movement, not during entry, motion or an action. |
| Map57 victory / Map3 defeat | Both outcome returns are implemented. F506 opens the separate [settled tower visit](#tower-save-and-continue-observation) on Map57. Map3 defeat retains unlock, halves gold and restores the leader under the existing outcome policy. Loading must not run either outcome again. |
| Open warps beyond this endpoint | Preparation retains warp rows and fails at unadmitted destinations/setups. The [branch inventory](../content/opening-branch-inventory.md#ordinary-warps-and-future-boundaries) identifies future rows, opening obstructions and unavailable preflight before committed transfer. Silently accepting Unsupported as a no-op is forbidden. Later campaign admission remains outside #1418. |

This is an accurate gap disposition, not a claim that all reachable NPCs were exercised. The [bounded inventory](../content/opening-branch-inventory.md) enumerates the offered
entity/default/zone/search/warp surfaces in Maps3/19/20/21 and returned Map57 for the admitted
opening flags. Static lowering and focused remake execution do not prove original natural reach.
Only source uncertainty that prevents a required behavior warrants a focused evidence follow-up.

## New Game construction

Immutable content admission is separate from start construction. `PrivateBattleContentReader`
selects ally capabilities from pinned static content and selected source growth, independently of
`ControlledBattleStartReader`. `PrivateBattleDefinitionReader` retains source initial ally records;
encounters use level-one immutable baselines and progression lives in `BattleActorStartInput`.
`OpeningPartyInitialization` processes retained allies in ascending member order, scans initial
spells and reuses ordinary `BattleGrowthRules` in HP/MP/ATT/DEF/AGI order, then restores current
HP/MP. Curve NONE consumes no draws. Future class growth spells remain admitted capabilities,
not automatically learned spells. Only supported opening starts are constructed; the unneeded
HEAL3 initial-prowess branch rejects explicitly rather than simulating another character's startup.
The controlled adapter still admits its explicit diagnostic baseline overrides.
The same separation applies to controlled `NewBattleStartPolicy.BridgeBoundary` provenance: a genuine
new-game entry must not invent an H3 bridge label to satisfy a diagnostic start gate. Keep real source
difficulty/control/status admission with the selected rules and keep comparison provenance at its
existing diagnostic boundary.
Application receives a validated new-game start, selects the configured `SessionRules`, then uses
the existing exploration/program lifecycle. Neither runtime nor Content reads `remake/reference`,
test fixtures, prior slots, comparison checkpoints or a test's expected end state.

| Initial field | Required source/default and admission |
| --- | --- |
| Identity, name, class, starting level, loadout, spells and growth | Existing private static/growth content from the pinned ally/start/class/name/item tables, under [ally data](../../../docs/design/contracts/party/ally-definition-data.md) and [level-up](../../../docs/design/contracts/party/level-up.md). Store stable actor IDs; retain immutable default names in content and resolve the protagonist override from session state. Do not infer class/name from a sprite or reuse battle-ready stats as new-game stats. |
| Roster | Only member0 joined/active initially, source flags/membership mapping retained; Sarah/Chester join via ordinary programs. Construct the supported opening actors and future join baselines once, including Jaha if retained shared encounter content requires it. Unjoined actors are not displayed or deployed. Do not initialize all 30 merely to satisfy a full-game target. |
| HP/MP/base stats and learned spells | Apply the selected start-level initialization/growth and equipment refresh, with full current HP/MP. Use the existing algorithms where valid; any missing new-game initializer is real #1420 work. Initial growth consumes the declared main seed in source order; do not copy R1 numeric stats. |
| EXP, kills, defeats, status, gold | Explicit supported fresh defaults: EXP/kills/defeats/status = 0; gold = 0. Gold0 is also observed at the controlled R1 seam in [admitted start](../../../docs/research/chapters/grans-opening/map3-admitted-start.md); zero status is a remake fresh-start policy, not closure of R1's inherited POISON Unknown. No nullable transaction-critical accounting in a product start. |
| Items and custody | Decode complete runtime item words, including equipped/broken/auxiliary bits, from initial definitions; four projected item bytes are insufficient for a save. Explicit empty Deals, battle once-flags and field pickup ledger for a fresh game, with supported item identities admitted. These are new-game choices; null controlled evidence remains null. |
| Flags/options | Fresh story flags with only the initialization/membership bits required by admitted source setup; normal difficulty F78/F79 clear, no debug control flags. Initial mode/difficulty is fixed and recorded in the start contract, not inherited from the last session. |
| Location | Map3 `(56,3)`, Down, default setup, source player identity; run actual initial map setup/programs. This handoff is evidenced by the save-system and admitted-start owners. Do not seed messenger/castle/tower completion flags. |
| RNG/settings | Two explicit starting seeds supplied once by the host, then only selected rules advance them; tests can supply fixed seeds, including zero where the existing rules admit it. Default random selection adds no new seed-validity restriction. Preserve complete uint storage, not only a low word. Default text/input/reduced-flash settings use the existing settings owner. This choice is not original reset timing parity. |

The initial-resource data audit in #1420 is an acceptance boundary: names/classes, starting growth,
full item words and accounting must all be admitted before a playable New Game is claimed. Optional
portrait/class-label translations may remain visibly unavailable. Missing mechanical data rejects
startup with an actionable content diagnostic, never a plausible number or a practice-yard fallback.
The existing [R1 owner](../../../docs/research/chapters/grans-opening/map3-admitted-start.md) retains
its controlled menus/naming, omitted POISON and time/NPC-phase limits. New Game adds no original H3
claim. The source-informed title/Witch presentation below precedes this same construction; it adds
no natural original menu/naming or exact presentation-timing claim.

### Title and Witch presentation

**Standard rigor, slice A of [#1440](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1440):**
the outcome is a recognizable, playable title → Witch → existing New Game/Continue/Quit entry at
1920×1080. The High-rigor persistence agreement also owns the delivered slot/name/initial-save flow
below. Normal difficulty is fixed. There is no difficulty selector, Copy/Delete service or intro film;
whole-party naming and original natural naming cadence remain outside this boundary.

The ordinary host renders separate title/numeral layers with a short modern entrance. Confirm or
mapped Party Status/Pad Start enters the source Witch greeting. Confirm first reveals adjustable
text, then advances; mapped Start skips to the action menu, and Cancel returns to the title.
Mouse Start and Reveal/Continue buttons cover the same reading path. The Witch changes from its
arrival head to open eyes and animates its mouth while revealing text, with a small periodic blink.
Reduced-flash keeps static frames. These are presentation choices, not original frame timing.
The source 256×224 composition uses 4× nearest sampling independently of the world's 96px grid;
native30px text uses the [bounded entry windows](../godot/presentation.md#entry-windows-and-hints-1445).
Title/Witch choices, Continue/backup/error, New Game slots, naming and final confirmation now use
that opt-in chrome and the Witch-safe right region. Continue retains the Witch backdrop while
selecting/restoring a slot; Back returns to its menu. The temporary entry canvas mode restores the
previous gameplay mode on exit. Compact hints consume installed mappings; keyboard name editing
retains its existing Enter/Escape semantics. These presentation changes preserve the High-rigor
publication, backup and session-replacement contract; final human visual acceptance remains pending.

`SessionEntryView` owns only presentation/input and disposable resources; `GameRoot` retains
session construction/replacement. Continue uses the existing slot/backup diagnostics and
[Witch continuation](#witch-continue-dialogue-and-default-selection). Replacement
opens on Cancel; failed admission preserves the paused old session. Entry music uses `SessionAudio`;
the old session's players pause during entry and resume on Cancel. Successful mounting releases
entry textures/audio and the old session through their existing hooks. Held mapped actions require
release across entry and mounting. Save's long binding hint is a tooltip so its actual button cannot
cover New Game. No save bytes, name schema, initial party or difficulty rules change.

Private preparation uses `sf2tool.remake_entry_content`, the registered USA ROM and pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. It reads only the named title/Witch/font
spans owned by [special screens](../../../docs/research/presentation/special-screens.md), reuses Stack,
tile, color and PNG decoding, and rejects visible references beyond decoded tiles. It does not
split a ROM, rebuild H1, mutate the read-only source or fill unknown DMA tails. Source comments
216–221 supply the bounded greeting. Omitting music produces a layout sample, not a runnable package.
The actual Content reader requires the complete images/dialogue and validated title/Witch PCM,
including a Witch loop; no missing-input practice world or counterfeit original art is substituted.

After same-process private configuration and the narrow ROM identity check, select reviewed
`sf2tool.remake_audio` candidates and fresh ignored output:

```powershell
uv run python -m sf2tool.remake_entry_content --rom $romPath --upstream $pinnedUpstream `
  --title-audio $titleCandidate --witch-audio $witchCandidate --output $freshEntryOutput
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $freshEntryOutput 'entry.json'
# Preserve the palette-complete world and existing battle/static selections.
& $env:GODOT_BIN --path remake/game
```

**Confirmed, bounded command-only audio:** commands28/10, source `Music_28` C2 / `Music_10` C1
headers, finite title `channel_end` and repeating Witch `mainLoopStart/mainLoopEnd` use the existing
PCM consumer. The title capture falls silent after its finite score. **Inferred:** the Witch's modern
unchanged-PCM loop has period1,281,828 samples at44,100Hz (about29.0664s); four independently sampled
windows retain their correlation results, including the20s outlier, and a low-amplitude matched
seam avoids an arbitrary30s cutoff. The current candidate retains capture samples179,000–1,740,247
and loops279,419–1,561,247 relative to that retained PCM. This is a reviewed modern loop, not a claim
of sample-exact YM phase, an original interrupt boundary or natural menu reach. Preparation and native
reproduction belong to the [entry observation](../verification/host.md#title-and-witch-entry-observation).

#### Witch Continue dialogue and default selection

**Standard rigor** for [#2255](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2255).
The existing High-rigor contract governs save admission, backup publication and attachment.
The slice covers the ordinary Witch menu, selected Continue and the handoff to the admitted
snapshot. Keep the established modern slot history, Settings, Quit, New Game and direct
diagnostic entries. The startup sequence still runs once per ordinary launch.

**Confirmed source structure.** The pinned USA SF2DISASM `witchstart.asm` shares 216–220 at
both occupancies and dispatches after 221 using `SAVE_FLAGS`. `witchMenuAction_Load` uses
225 → selection and `LoadGame` → 226 with member 0 → 224. This slice does not establish
original runtime timing or restore the retail two-slot action menu.

| Current local storage at the action menu | Default action | Continue path |
| --- | --- | --- |
| At least one primary accepted by the current content and record validator | Continue | Existing numbered history and fresh validation of the chosen record |
| Empty history or only damaged, incompatible, unsupported or backup-only records | New Game | The same history stays accessible for diagnosis and explicit compatible-backup recovery |

Persistence checks the numbered primaries in the background and stops on the first accepted
record. Empty and structurally invalid files require no campaign admission. The detached check
only chooses initial focus. The slot flow still rereads and validates the selected record with its
existing ownership and version checks. The menu displays loading feedback while the default is
being resolved. Back from the question or slot/recovery pane returns to the Witch action menu.

The Witch displays 225 before the slots. The restore owner constructs the detached session and
prepares its resources, including existing battle/settled-field attachment and explicit backup
publication when chosen. Only then does the Witch display 226 with the restored story's member-0
display name, followed by 224. The same prepared session mounts after the final acknowledgement.
Cancel before acceptance and restore failures lead to the existing menu or diagnostic without
recognition or farewell. The final two lines use fresh reading acknowledgements and retain
the mapped release barrier through attachment. Start still skips the shared greeting. The direct
`--load-save` path keeps its existing bypass. Ordinary primary Continue is read-only.

[Private bank, commands and observation](../content/presentation-assets.md#witch-continue-entry-bank-2255).

#### Startup content preparation (#2283)

**Standard rigor.** The bounded outcome is to prepare campaign content during ordinary startup
and reuse its admission for Witch default selection, Continue and New Game slot inspection.
The High-rigor storage/restore boundary above still owns records, backups and publication.
No save-format, campaign, asset, dialogue or gameplay admission changes are included.

`GameRoot` starts one detached read after launch configuration/preferences are accepted and before
opening/title. [`EntryPreparationHost`](../../game/src/Entry/EntryPreparationHost.cs) captures the
selected paths on the host thread. [`EntryContentPreparation`](../../game/src/Entry/EntryContentPreparation.cs)
serializes readers and retains one successfully admitted immutable definition. Menu checks still
read current primaries, stop at the first valid record and return only a focus choice; neither
slot availability nor restored sessions are cached. Reopening the menu therefore sees external
slot changes. The existing `LoadEntry` owns loading feedback, cancellation and main-thread
publication. Godot nodes, textures, localization binding and audio stay on that thread.

Reuse requires the same selected paths and unchanged file names/lengths/UTC modification times.
The selected world directory is the package root for relative encounter JSON, HD terrain and PCM;
its recursive file metadata and explicitly selected external input files are compared before
reuse and before/after admission. This conservatively invalidates on unrelated package-file
changes too. Keep immutable inputs together and writable outputs outside that package, as required
by the [input owner](../../../docs/operations/local-private-inputs.md). No new manifest, watcher,
persistent cache or change-detection digest is introduced. This is an ordinary file-change check,
not protection against an adversary deliberately preserving size and timestamps.

Continue rereads the chosen slot and checks that its definition is still the current prepared
content before detached restoration. Changed content refuses the old candidate with
`save-content-changed`; changed slot bytes retain `save-record-changed`. Backup recovery retains
its explicit confirmation and atomic store checks. Failed admissions are not retained, so transient
read failures remain retryable. Cancel abandons the UI candidate, leaving reusable content only;
quit/host disposal releases the retained definition and prevents a late read from retaining or
mounting a result. Detached faults are observed even if their view exits. Full New Game still
performs fresh party/name/seed initialization and its confirmed initial save; that independent
read is serialized with preparation, rather than reusing a restored session.

**Resource plan and measured limits.** The selected `derived/issue2282/buff-v1` package has 175 files,
368,493,196 bytes including the roughly 99 MB world. Admission is linear in input size and decoded
content; metadata inspection is O(package files), with sorting O(F log F). Run one process/read at
a time. The initial 1 GiB single-read / 1.5 GiB overlapping-opening estimate was revised after the
baseline reached about 2 GiB across repeated reads. The bounded runs use a 2.5 GiB observation
budget, under 1 MiB per receipt, and only small copied save/static inputs. No network/model costs
or full-campaign route are involved. Replan if the package expands materially or a run exceeds
that budget. Continue now peaks around 1.1–1.25 GiB in these samples; the separate full New Game
initialization reached 1,618 MiB. Empty/corrupt-only histories now incur one early campaign read
that their default-focus check previously avoided. A fast Skip can still expose its remaining
latency; background preparation is not a guarantee of zero startup delay.

**Confirmed, bounded software observations.** Baseline `af1030857dc0df6f80e63fb158ec5f74c66c3b59`
was instrumented at the existing `OpeningRestoreReader.Read` calls with the same stopwatch/read
counter now exposed by `ReadEntryPreparationObservationJson`. The actual Main scene was driven
at 1920×1080, headless Godot 4.7.2, 120 fps cap, instant English text. The valid input was a read-only
copy of the registered v6 slot4 backup (sequence 1, gameplay revision 4), not the player's runtime.
The route opens slots, cancels, opens them again, selects slot4 and acknowledges 226/224.
The 226 samples include the deliberate held-key/release check (ten frames); they are upper bounds
on recognition availability. Times below are single-process samples, not distribution estimates or OS-cache-flushed results.
“Warm” means a subsequent launch with the same inputs; all sessions themselves start fresh.

| Route / version | Process startup → default menu | First 225 acknowledgement → slots | Selection → 226 | World read/admission count across route | Peak working set |
| --- | ---: | ---: | ---: | ---: | ---: |
| Fast Skip, baseline warm | 2,793 ms | 1,726 ms | 301 ms | 4 | 2,035 MiB |
| Fast Skip, prepared warm | 2,476 ms | 65 ms | 284 ms | 1 | 1,147 MiB |
| Four-second opening dwell, baseline | 7,065 ms | 1,694 ms | 197 ms | 4 | 1,736 MiB |
| Four-second opening dwell, prepared | 4,857 ms | 55 ms | 202 ms | 1 | 1,274 MiB |
| Corrupt-only, baseline | 693 ms | 2,459 ms | refused | 1 | 1,159 MiB |
| Corrupt-only, prepared | 655 ms | 1,752 ms | refused | 1 | 1,005 MiB |

A final pair sampled the first observed 226 before the held-key test: baseline/prepared were
2,821/2,482 ms to default menu, 1,684/72 ms from 225 acknowledgement to slots, and **241/238 ms
from selection to recognition availability**. That isolates the benefit to preparation/menu reuse,
not a claim that session resource attachment itself became faster.

The first process samples gave 2,727/2,655 ms to menu, 1,706/67 ms to slots and 306/286 ms to 226
for baseline/prepared respectively. These do not establish cold-disk performance. With four seconds
of opening dwell, preparation completed while `MUSIC_INTRO` advanced to 3.63 seconds; 457 frames
advanced between the startup samples. Maximum observed frame gaps were 207 ms baseline and 232 ms
prepared, including existing resource publication. Human audibility and absence of visible hitches
remain **Unknown**; this optimization does not close those claims.

The copied slot restored Map19 (4,3), Left with usable field control. Ordinary cancel/reopen kept one
admission; 225/226/224, held-key/echo barriers and no pre-farewell mount passed. Empty and corrupt
histories chose New Game; corrupt-primary/valid-backup offered explicit recovery, and incompatible
primary stayed refused. Controlled slot mutation and an independently copied static-input mutation
refused attachment. A temporary exclusive lock on a disposable static-input copy failed the first
admission; releasing it without changing file bytes/metadata allowed the next ordinary menu check
to retry successfully. Disposing during the first read let detached work finish without retaining
content or publishing. Fresh New Game still initialized, saved and mounted in an isolated namespace.
The first baseline name assertion used `Bowie` instead of the actual `BOWIE`; that failed receipt
is preserved, and the corrected run passed. Some baseline and candidate Godot exits reported two
ObjectDB instances leaked; the retained-definition disposal observation passes, but the cause of
that existing/intermittent native shutdown warning is **Unknown**.

Reproduce with the [locked environment](../development-and-verification.md#locked-net-workflow),
[ordinary host/input selection](../verification/host.md#ordinary-host-startup) and
[Witch bank recipe](../content/presentation-assets.md#witch-continue-entry-bank-2255). Select the
accepted full world plus its six `opening-inputs`, `main-scenes.json`, the independent
`derived/issue2252/pre-voyage-opening-v1/opening/opening.json`, and
`derived/issue2255/witch-entry-v1/entry.json`. Use a fresh authorized disposable save namespace
outside checkouts, never the real player namespace. The existing observer now records stage
milliseconds, frame gaps, read counts, GC counts, memory and retired-owner state:

```powershell
$env:SF2_WITCH_CONTINUE_CASE = 'valid' # also empty, corrupt, abnormal, changed, content-changed, newgame
$env:SF2_WITCH_CONTINUE_LOCALE = 'en'
$env:SF2_WITCH_CONTINUE_HERO = 'BOWIE'
$env:SF2_WITCH_CONTINUE_SLOT_INDEX = '2' # slot1/2 rows, then copied slot4; default remains slot2 index1
$env:SF2_WITCH_CONTINUE_DWELL = '0' # use 4 for opening/BGM overlap
$env:SF2_WITCH_CONTINUE_OUTPUT = Join-Path $runRoot 'witness.json'
& $env:GODOT_BIN --headless --path remake/game --max-fps 120 --quit-after 5400 `
    --log-file (Join-Path $runRoot 'godot.log') `
    --script res://probes/engine_witch_continue_observation.gd -- --input-settings $settings
```

`changed` requires `SF2_WITCH_CONTINUE_CHANGE_FILE` to name the disposable slot4 copy: the observer
corrupts it after listing. Also set `SF2_WITCH_CONTINUE_MUTATION_NAMESPACE` equal to the explicit
disposable `SF2_SAVE_NAMESPACE`; the probe restricts mutation to that namespace's `sf2-opening`.
`content-changed` requires that variable and `SF2_PRIVATE_STATIC_DATA`
to name the same independent disposable static JSON copy under this checkout's `local/`: it appends whitespace after listing.
Never point either mutation case at a registered input or real save. `corrupt` uses malformed slot1;
`retry` uses a valid copied slot and an independent `SF2_PRIVATE_STATIC_DATA` copy under `local/`;
the helper holds that copy read-only/exclusive for the first admission, then releases it before the
menu check. `abnormal` uses malformed slot1 plus a compatible slot1 backup, and incompatible slot2, as in the
existing Witch recipe. `newgame` requires an empty disposable namespace and writes its initial save.
Keep each completed receipt/log and compare source/copy bytes for read-only cases. No screenshot,
full campaign run, engine-test expansion or research/full gate is required for this host-only change.

#### Title lifecycle investigation (#1447)

**Standard rigor, investigation only:** distinguish ordinary first entry, Witch, return and idle
using the accepted consumers and prepared package. Production/assets remain read-only. The
[independently accepted investigation](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1467)
retains the short entrance, static idle title and finite score.

**Confirmed structure:** `EntryBackdrop` advances its instance-lifetime elapsed counter in
`_Process`. The numeral reaches full opacity and its final position after0.9s, from a24px offset;
the title then stays static. `ShowTitle` changes layers and requests title music without resetting
that counter, so returning from the Witch retains the completed entrance. Reduced-flash selects
the completed title immediately and static Witch head frames. This describes the current modern
implementation, not original timing or a newly accepted requirement to replay on return.

**Confirmed bounded software behavior:** the [native investigation](../verification/host.md#title-lifecycle-investigation-1447)
observes visible title entry, progressing draws/playback, finite title completion, Witch arrival/
reading/menu, return with restarted title music, and a second finite completion. The loaded title
is450,995 stereo frames at44,100Hz (10.226644s), with looping disabled. Witch uses1,561,247 frames
(35.402426s), with the existing279,419–1,561,247 loop. Source `Music_28`'s finite channels and
`Music_10`'s repeating channels support that distinction; the modern PCM seam retains the Inferred
limit above. No new original-game runtime observation is made.

**Unknown:** the cause of the user's reported silence, physical audibility, perceived motion and
whether a return should replay the entrance. No software playback flag proves listening approval.
If replay is later chosen, the minimal proposed boundary is resetting the existing backdrop entrance
when title becomes visible, with first-entry/return/reduced-flash acceptance; it needs no new art,
idle animation or audio service. Player visual and listening acceptance remains **Unknown** under
[#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).

<a id="proposed-opening-sequence-2247"></a>
### Ordinary opening sequence (#2247, #2252)

**Standard rigor** for the pre-title presentation and its Content, Application and Godot handoff.
The existing High-rigor save boundary still governs New Game and Continue. The objective is to
present the four boot-story scenes with an ordinary, skippable route to the current title.
[Original chronology](../../../docs/research/presentation/special-screens.md#boot-story-sequence) ·
[Current coverage](../evidence/opening-ui-gaps.md#opening-sequence-coverage-2247) ·
[#2247](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2247).

#### Lifecycle decision

The #2252 implementation uses the existing Standard agreement here. Its owned increment is the
four-scene content selection, disposable ordinary-start host, shared natural/Skip title handoff,
and consumption of compatible installed HD resources. Save publication and restoration remain in
their existing High-rigor owners. No new scene framework, branding, idle attract sequence, AI art
production or broader campaign behavior is admitted.

The finite compiler pilot lowers all four scripts without an unsupported native call. The selected
baseline is344.24MiB; the first full opening candidate is23.75MiB including its18.34MiB PCM and
Map42 HD terrain. Plan for24–32MiB opening input, about370MiB closed successor, under1GiB preparation
and2GiB native working set. Preparation is linear in the selected source/asset bytes; runtime admits
only the opening selection. Reassess for a new resource family, exceeded budgets or unbounded waits.
Verification is one full ordinary run plus distinct short handoff controls, affected engine/adapter
checks and the committed dependency plan. The [package owner](../content/presentation-assets.md#four-scene-startup-package-2252)
records observations; [PR2254](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2254)
owns the independent integration disposition.

- On ordinary process launch, load preferences and prepare the selected opening scene set through
  Content. Play it once before the title, regardless of slot occupancy. Show a responsive loading
  surface and allow Skip during preparation. An entry package without opening scenes goes straight
  to the title. A selected scene set that fails admission or delivery reports a bounded diagnostic
  and reaches the same usable title when the existing title assets are valid.
- Natural finish and Skip converge on one title handoff. The pre-title sequence owns input
  while active. A fresh mapped Start (the entry action currently routed as `Equipment`) or an explicit
  pointer Skip cancels the remaining sequence even during timed dialogue, motion or fade. Keep
  Confirm for reading controls where applicable. Clear the outgoing work, release mapped actions
  and finish pointer capture before the title accepts a new Start/Confirm/click. Late events from
  a cancelled or replaced owner have no successor to advance.
- The title starts its current finite score when presented. Witch Back returns to the title under
  the existing #1447 behavior. Menus, idle title, in-process return from play, Settings and window
  recovery do not rerun startup. A fresh ordinary process can play it again. Keep diagnostic/start
  overrides, preview and explicit seeded Continue on their direct routes.
- Witch New Game keeps slot selection, naming, explicit replacement consent, candidate admission
  and the initial save in their current sequence. Mount the same Map3 start once after successful
  publication. Continue remains the same detached validation and restore, including settled field,
  battle, legacy and diagnosed backup paths. Neither action starts the boot story after the Witch.
  Boot completion and Skip have identical persistence consequences: zero writes, no allocation of a
  journey slot or gameplay seed, and no change to a selected or paused journey. The loaded save
  remains the sole authority for progress. The [Witch Continue dialogue](#witch-continue-dialogue-and-default-selection)
  follows the startup handoff and uses the accepted restore candidate.

A pre-title scene keeps the source order and isolates startup effects from save publication.
Placing it after name/slot confirmation would tie a discardable presentation to a committed
journey and would remove it from the ordinary startup route for players with existing saves. An
authored movie would simplify playback but add a media/localization branch and lose live language,
input, palette and art-mode projection. Use the existing runtime and script content. A licensed
or project-authored branding screen and any later revision to the title phases are independent
presentation decisions. The initial slice uses the current title and omits the source title's
idle-reset attract cycle.

#### Runtime and content boundary

Give the intro one disposable pre-title owner, separate from the durable session. Application runs
its data-selected sequence with `StoryProgram`/`ProgramRunner`, scene-map and scene-actor binding,
existing actor/view, text, tick and projection waits and one explicit completion to the host.
Use the existing scene transfers that bypass ordinary map init and population when the script
supplies its actors. In particular, Map42's campaign setup is a different caller. Use scene-local
actor identities and the admitted default-name data for Slade. Keep save, battle admission, other
field interactions and persistent party/progress mutations unavailable in this entry context.
The host selects and releases the scene, routes input, presents it and mounts the title. Scene order,
actor setup, camera operands and cues belong to Content. Opening New Game and Continue stay their
own entry modes. [Application scene/window contract](session-and-programs.md#bound-before-battle-scene-and-windows) ·
[Scene transfer](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) ·
[Story runner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs).

Use the existing private package composition with a bounded pre-title content selection. Compose
only the required maps, script, text/name rows and projection assets in preparation, then admit
through the existing Content readers and validators. Extend their composition seam where needed.
The ordinary `PrivateExplorationReader` ties its approximately 95 MiB world file in the selected
package to party and encounter admission, so calling it from startup would bring campaign
validation and bulk resource loading forward. Keep the working-set selection explicit, load it
off the Godot thread using the existing detached entry-loading pattern, and bind its Godot objects
on the host thread. Reuse selected assets and external WAVs in the closed package. No dependency
on the ROM checkout at runtime. [Private reader](../../src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs) ·
[Closed package preparation](../content/presentation-assets.md#reuse-a-closed-private-package).

The existing compiler and runtime already express reload/reset, scene actors, camera targets and
waits, clone binding, movement/actions, ordinary fades, block copies, quake and white effects.
Require complete lowering of the selected scripts. The #2250 controlled slice supplies `{D2}`
text/translation topology, half-black/tint and the temporary storm-bank effect described below.
Bind `{D2}` to the source's timed wait with
typewriting inactive and no demand for acknowledgement, using the normal text clock. The Skip
input remains independently live. Admit only any other reached action/effect operand that a
compiler/consumer check shows is missing, including the fixed/transparent actors and the
mosaic/pose actions. Use a typed finite presentation operation if the existing palette path needs
an extension. Review default-name and portrait/speech operands before admission. Never silently
drop an unsupported row. [Compiler](../../../src/sf2tool/remake_exploration_programs.py) ·
[Text tokens](../../src/Sf2.Remake.Content/Scenarios/ExplorationTextTokens.cs) ·
[Text clock](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs).

For Skip, discard the whole scene owner and its outstanding waits, actor/camera work and audio
instead of running the rest of the montage at high speed. Its only successor is the title. Existing
Battle Entry Skip must still execute consequential instructions. Reuse its release and ownership
principles, retaining the separate completion semantics. [Battle-entry skip agreement](session-and-programs.md#battle-entry-skip-agreement) ·
[Presentation and audio handoff](../godot/presentation.md#pre-title-story-presentation-2247).

#### Bounded scene implementation (#2250)

The controlled scene uses the existing map-initialization continuation, StoryProgram call stack,
scene-map/scene-entities operations, actor services and presentation clock. Its map definitions have
no campaign setup, events or persistent-party actions. The initial population supplies the existing
player sprite binding until the original scene entity list replaces it. `EndScene` retains a terminal
wait and releases temporary palette state; it does not return field control or enable saving.
The #2252 ordinary pre-title host reuses this scene mechanism with the complete four-script selection.

`Delay2` waits for 119 normal logical services independently of mouth control and modern instant
reveal. Reading Confirm cannot shorten this delay. Automatic text then continues and closes through
the existing window services. Localization admission preserves the delay and name topology.

Palette state belongs to the scene. The finite `FadeScenePalette` operation applies source-selected
absolute color-level offsets with bank mask 1 (terrain) or 15 (terrain and actors); the existing clock
services each source period and waits for actual presentation completion. `PulseScenePalette`
replaces terrain bank 0 for ten services, restores its exact previous sixteen words, and waits ten
more. Battle Entry Skip still services this logical work before omitting the delivery wait.

The original RGBA atlas loses palette slot identity: equal RGB values in distinct slots cannot be
replaced correctly by a global color lookup. Only the selected maps therefore carry an optional
128×320 byte index plane and sixteen base words. `MapBlockTextures` reconstructs its existing blocks
with these slot colors. No whole-map migration or frame atlas is needed. Actor half-fade uses the
same absolute channel offset on the existing source sprite; terrain-only tint/storm does not affect
actors or UI. Installed HD sprites retain the uniform channel fade. HD terrain uses the
same bounded transform for uniform fades. The [opening HD successor](#opening-hd-completion-2324)
adds Map65 and183/187 resources and finite storm banks; absent optional palette coverage still
falls back to indexed Original terrain for that interval.
Reduced Flash holds a smaller presentation change for a pulse; logical replacement/restoration and
wait completion remain unchanged.

`ScenePaletteRunner` owns the transient state and wait. `ScenePaletteReader` owns finite resource and
operand admission; the scene compiler reads the pinned fade tables and storm palette. Scene reload
preserves the current bank, whereas a black scene-map load binds the new base resource. Ordinary
warp and return-to-field keep their existing full-fade contract and reject transient scene palettes.
Existing busy/cursor save eligibility excludes the running scene and its terminal; there is no save
schema or persistent palette change. `SettledFieldSnapshot` remains read-only.

The private producer `python -m sf2tool.remake_opening_scene` selects the complete IntroCutscene1 and
the unchanged IntroCutscene2 prefix ending after the first post-tint storm pair restores. It prepares
Map65 and effects 183/187, reuses selected prepared actors/portraits and command9 PCM, and derives all
81 IDs4186–4266 from the accepted full-text Chinese authority. The source/default SFX bindings reuse
accepted modern recordings; this does not claim original C4 synthesis parity. The package has no
reference to producer scratch or the full campaign world at runtime.

**Confirmed:** the complete shrine and castle prefix run automatically in English and Chinese;
Reduced Flash retains the six storm restoration pairs. Both published starts reproduce after
producer candidates are moved away, without party/gold changes or save writes. Actual state,
indexed texture samples, actor/effect projection and error checks support this bounded result.
[Commands, package and validation limits](../content/presentation-assets.md#controlled-opening-scene-package-2250).
This bounded pilot was accepted in #2251. Source structure does not prove original natural timing
or human presentation quality; those remain **Unknown**. The original behavior owner remains
[special screens](../../../docs/research/presentation/special-screens.md#boot-story-sequence).

#### Opening HD completion (#2324)

**Standard rigor:** the bounded outcome is HD Map65 terrain, sprites183/187, the
existing four-scene palette transitions and the user's complete opening-camera audit.
Startup, skip, title handoff, dialogue panels,
black exterior and campaign/save behavior retain their accepted owners. Human visual
approval and original hardware timing remain **Unknown**.

The independent successor is produced from the published #2252 opening and the pinned
graphics algorithms; neither a full campaign reconstruction nor a new extraction run is
needed. Map42 and actors/portraits retain their existing resources. Map65's optional
`scenePalette.hdBanks` contains exact sixteen-word palette selections and ordinary
`hdBlocks`/`hdBlockContexts`. The producer keeps original palette-slot identity, freezes
base-palette density/edge geometry, and recolors each generated OKLCH ramp from its original
two slots. This avoids ambiguous RGB matching and prevents storm colors from changing
the filtered geometry. Runtime uses exact palette equality; absent optional banks retain
the indexed Original fallback. Uniform fades retain the accepted channel-offset treatment.
Reduced Flash uses the same bank selection with its existing projected sixteen words.

Owned implementation: `remake_opening_hd.py`, scene-palette definition/admission,
`MapBlockTextures`, exploration's HD selection and an opening-specific native observation.
Shared asset registries and aggregate documentation require serialized main-gate intake.
The camera correction remains in exploration projection; the shared field/battle viewport
and Domain/Application camera speeds and paths are unchanged.

The first pilot covers three Map65 blocks under base plus nine alternate banks and all
six direction strips for183/187. It took0.211s and wrote75,861 bytes. Initial plain locked
Python launch failed before generation because Pillow was absent; use the existing
`uv run --locked --with pillow==11.3.0` graphics dependency selection. Estimate full output
under40MiB including the existing intro audio, under512MiB producer memory, under2GiB
native memory and under2GiB total scratch. Work scales with selected blocks plus the
18×19 Map65 plane for each of ten palettes, not with sequence duration. Run serially and
reassess measured limits before expansion. No network/model/image-generation calls are used
by production. The pilot fits this bounded plan; proceed to complete banks and actual texture
observations, preserving the accepted lifecycle evidence unless host handoff changes.

Acceptance requires mounted96×96 terrain and192×96 effect strips, exact palette selection,
restoration, Original and absent-optional-bank fallback, actual effect draws, black exterior,
scene disposal and error-free resource lifetime. Adapter compilation passed after the first
implementation. HD, Reduced Flash, Original and absent-palette-bank short native observations
passed, with both effects drawn, exact palette selection/restoration and black exterior.
The complete private successor is `derived/issue2324/opening-hd-v1/opening/opening.json`.
Production took2.446s, wrote27,250,723 bytes and peaked at77.91MiB; the world is7,162,097 bytes,
with127 Map65 blocks,166 context rasters and nine additional color banks. It retains the
source scripts, maps, Map42, other actors, portraits, audio and independent catalog unchanged.
[Production and native reproduction](../content/hd-rollout.md#opening-terrain-and-palette-banks-2324).

**Confirmed controlled camera defect:** at1920×1080 a target→entity130→target transition
kept logical X2688 (168 source pixels), but the drawn origin changed from48 to−24 source
pixels, a288-screen-pixel jump. Detached scene cameras used logical planes while entity
following used ordinary field viewport clamping; Map65's432-pixel width is smaller than
the modern480-source-pixel view. Admitted scene-palette maps now use the same logical
plane projection in both modes, including actor origins. This preserves authored follow
thresholds, destination speeds, layer offsets, quake and explicit reloads. It introduces
no new speed multiplier, clamp or widescreen composition. The actual short native trace
matches logical planes across the transition.

**Confirmed static camera operands:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/data/scripting/map/cs_intro1.asm` through `cs_intro4.asm`, agree with the published
programs below. `setCamDest` retains its operands; scene load/reload and entity following
remain distinct. The [camera evidence owner](../../../docs/research/chapters/grans-opening/map3-messenger-acceptance.md)
records `SetCameraDestination`'s coordinate×384 conversion and `SetViewDestination`'s plane
factors/origins. The remake keeps1/16-pixel view units and existing24/32-unit service speeds;
4× display density changes screen displacement, not the logical trajectory or service count.

| Source scene | Load/reload origins | Explicit destinations in source order | Followed source entities |
| --- | --- | --- | --- |
| Intro1 | Map42(2,5); reload(15,0) | (15,5), (21,1) | Slade; neutral between rooms |
| Intro2 | Map65(7,2); reload(7,2) | (7,4), (7,2) | 130; neutral before return |
| Intro3 | Map42(21,1); reload(21,1), (0,3) | None | Slade |
| Intro4 | Map65(7,3) | (0,5), (7,5), (7,5), (7,1) | 130,130,128,128; neutral before the second(7,5) |

**Confirmed complete remake observation:** `run-full-camera-v2/observation.json` under the
owning checkout's `local/issue2324/` ran the unchanged four source scripts through terminal
in416.598s, with27,632 actual camera frames (11,816 Map42;15,816 Map65), including1,558
moving frames. All four scene IDs occurred in order. Every observed foreground/background
and actor origin matched the logical snapshot used by that draw, including quake and layer
offsets. Both effects, HD terrain through pulses, exact palette restoration, unchanged party,
black exterior and terminal palette release passed; the native log has no error or leak.
This is controlled scene execution, not a new ordinary boot/Skip/New Game proof. Existing
#2252 host lifecycle evidence remains applicable. A live sample near completion recorded
830MiB peak working set, within the2GiB budget; final-process peak was not sampled.

Published-input short observations `run-published-hd-v3`, `run-published-small-v3` and
`run-published-reduced-v3` also pass actual uploaded color-bank pixel comparisons at1920×1080
and the supported minimum1600×900. The expected pixels come from independently decoded
published PNGs.1280×720 correctly reaches the existing minimum-size recovery UI. Two earlier
pixel-comparison attempts failed because JSON floating-number arrays and native integer
arrays did not match in GDScript; direct offline digest comparison proved the pixels identical.
The observer now compares all sixteen words numerically before comparing RGBA pixels; no
resource, palette expectation or production behavior was relaxed.

After rebasing onto the accepted AQUA successor, `run-final-hd`, `run-final-original`,
`run-final-missing` and `run-final-missing-art` passed from published-input witnesses with
the original producer scratch path unavailable. The last case removes Map65 HD terrain
and183/187 HD strips as well as storm banks; each absent variant selects Original while
Map42 remains HD. A cold producer rebuild from the published #2252 input is byte-identical
for every payload file; only the elapsed-time preparation receipt differs. No disposable
save directory was created.

The clean committed planner is run with `--scope engine`: the sole shared wiring edit
registers this producer and changes no research extraction or gate execution semantics.
Selected checks are engine-unit, adapter, private-admission, live-inspection and H4 comparison
builds, plus public Ruff/design-contract/research-index checks. All pass locally; the engine
result is3,383 passed and50 optional-private skips. No helper/producer tests were added, and
no original-game normal/full profile is implied. PR required-check results and exact Git
identities belong to the frozen independent-review handoff.

Palette changes retire tinted textures while preserving the already validated immutable
HD resource bank. Map changes/disposal still clear it. The initial implementation decoded
all alternate banks again after every color change; the corrected same-witness run took
4.785s instead of7.198s on this host. This is a bounded regression check, not a benchmark
or portable latency promise. Initial native observer type-inference errors and a full-camera
observer that omitted intentional quake offsets are retained failed observations, not
passed product evidence. The latter now compares the draw's own logical snapshot and quake.

#### Implementation order and acceptance

The bounded scene primitives and first-segment pilot were delivered in #2250/#2251. The #2252
implementation connects all four scripts, ordinary startup and the closed private selection.
[Package, reproduction and retained results](../content/presentation-assets.md#four-scene-startup-package-2252)
own its completion evidence; [PR2254](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2254)
owns the independent main-gate review and integration disposition.

Acceptance requires one complete natural run to a usable title; short loading, D2, motion/effect,
held-input and pointer Skip controls; late/duplicate completion isolation; one actual focus loss and
recovery; Reduced Flash; Witch Back; absent and malformed selections. Disposable New Game controls
compare natural finish with Skip under the same seeds, one initial publication and the same Map3
first control. Continue must preserve existing bytes and restore the saved state. Actual resource
projection establishes Original/HD consumption and the pure-black exterior without screenshots.

Optional branding, expanded title choreography and an idle attract sequence remain outside this
implementation. They require a separately selected product outcome and available art.
Inspect the actual adapter and use small engine behavior checks only where a real rule such as
timed dialogue or terminal scene completion changes. Existing input, state, cue and error
inspection is enough for the short acceptance runs. A full chapter, all save formats, a pixel
comparator and continuous capture add no coverage of the new handoff. Original natural timing,
hardware image and audio correspondence, and player visual/listening acceptance remain **Unknown**.

### Slot, name and initial-save design (#1443)

High rigor remains appropriate for this change: a confirmed New Game must bind names, admitted
content, durable progress and replacement of the paused session without a second state authority.
The supported outcome is Witch New Game → either local slot → protagonist name → final confirmation
→ initial save → actual Map3 opening. Normal is fixed; other difficulty, whole-party naming,
SRAM compatibility, arbitrary suspend, later campaign and the cold-boot film remain outside scope.

Application owns immutable `NewGameOptions` and the validated actor-keyed name overrides in
`StoryState`; its copies retain them through maps, battles and outcomes. A common Application query
resolves an override before content defaults for member/actor names. Dialogue token expansion,
field messages, Party Status, battle cards/messages and slot labels consume that same query.
Stable actor IDs, source names, mechanical definitions and membership never change. UI text is a
draft until final confirmation and is not live party state.

**Confirmed static:** pinned `c834c652b6862bc5679fd7f69a38a7093206efc6` source
`disasm/code/common/menus/nameallyscreen.asm` (`NameAlly`, `NavigateAlphabetWindow`),
`disasm/data/graphics/tech/windowlayouts/alphabetwindowlayout.asm` (`AlphabetStart`) and
`disasm/sf2enums.asm` establish a seven-character displayed limit, uppercase/lowercase letters,
digits, comma, period, minus, exclamation/question marks, number sign, apostrophe and space,
DEL/END controls and empty input retaining the default. Ten-byte ally storage is not the editor's
limit. The remake deliberately uses a physical-keyboard-only `LineEdit` (#1446), with the shared
`NewGameName` validator; unsupported characters/overlength input report an error without truncation.
Rejected Enter submission keeps text editing active so the name can be corrected immediately.
Spaces are preserved; empty selects the immutable default. The field receives initial focus, restores
it when the window regains focus, and recovers it before keyboard GUI dispatch after other focus
loss. Enter uses the name, Escape returns to slots with the draft retained; ordinary typing,
caret movement and deletion stay with `LineEdit`, including letters mapped to gameplay actions.
The allowed letters, digits and punctuation are visible. Mouse focus/Back/Use name remain available.
There is no controller alphabet grid or controller editing/submission; the host consumes pad events
while retaining their button/axis release edges and disarming movement/wait state. Other menus and
the final Start/save confirmation retain their installed keyboard/pad mappings and held-input barrier.
Font-supported ASCII is the admitted set. Original natural navigation and timing remain **Unknown**;
this intentional modern editor is not a new original runtime observation.

The initial save is a distinct finite `new-game` reconstruction recipe at the pre-on-load Map3
boundary. It captures initialized party/progress, seeds after initial growth, fresh flags, settings
and the validated name before any map program executes. Continue validates those values, rebuilds
that same initial start without rerolling growth, then runs the ordinary map on-load program. It
does not serialize a program cursor, fabricate completions, set story-completed flags, acknowledge
dialogue or call the ordinary settled Save capture. `OpeningSave.RequireStable` remains unchanged.
The ordinary pre-messenger/after-join/return recipes keep their existing meanings.

Final confirmation performs admission and detached initial capture/restore validation, constructs
the candidate session and prepares required audio before any write or old-session replacement.
The existing store validates and publishes the selected slot; only confirmed publication mounts
the prepared candidate and enables gameplay. Held/duplicate input is disarmed at page transitions
and during the synchronous commit. Seeds are selected once for that candidate, retained on a safe
write retry. No generic transaction or migration framework is introduced.

Back from final confirmation returns to the retained name draft; Back from naming retains it and
returns to slots; Cancel from slots returns to the Witch (or the old paused session via its Cancel).
Final occupied-slot wording names the slot and explains backup replacement. Cancellation, invalid
names, content/audio rejection and failures before publication preserve the old live session and
old primary/backup. A failed safe write permits Retry/Back/Cancel. The existing
`save-publication-uncertain` result means replacement may already have succeeded: keep the old
session paused, retain files, offer Continue inspection/Cancel and prohibit blind write retry.
Corrupt/incompatible primary recovery remains explicit through the existing Continue mechanism.

Format version1 gains one optional actor-name map; missing means no overrides and uses defaults.
Present data must be a non-null bounded map with valid retained actor keys and valid nonempty names;
unknown fields, duplicate keys, invalid types/characters/length and incompatible content still reject.
New writes include the map. No broad migration support or change to content/rules compatibility.

Acceptance uses actual engine behavior tests for name resolution, old/new save roundtrip, invalid
names, initial reconstruction and unchanged busy-save rejection; adapter build and real Godot input,
state/projection/error observations cover both slots, keyboard editing and pad isolation, Back/Cancel,
overwrite decline/accept, write failure/retry, fresh-process Continue and named opening joins into
Battle01. Use disposable saves and existing assets/environment; no screenshots or aggregate rerun.

### Implemented New Game boundary

The ordinary host displays a title and mapped New Game action. Two seeds are selected once per
confirmed start across the full uint domain. It admits the candidate and required audio metadata
before releasing an existing session. Failed replacement stays cancellable over the old paused
session. Successful replacement detaches old views synchronously, reuses their exit/disposal hooks,
releases session audio, resets camera projection and disarms held movement/input before attaching
the candidate. The New Game button consumes its GUI hit independently of underlying game input.

No checkpoint path is selected by this entry, including additional encounter starts in a private
world. Source initial item bytes are zero-extended into four runtime word slots, preserving the
source equipped bit and explicitly zero broken/auxiliary bits. EXP/kills/defeats/status/gold start at
zero; Deals and battle once-flags are empty. Only Bowie is joined/active; normal programs join Sarah
and Chester. Jaha is initialized only when retained growth/encounter content includes him. Field
pickup receipts use the source item flags in saved story progress. Persistence retains existing
battle custody separately; Search adds no duplicate pickup ledger.

Fresh world counters are step0/battle-request false. EgressMap is Map3: the Witch New Game
caller overwrites the earlier InitializeGameSettings zero with GAMESTART_MAP before its initial
SaveGame/MainLoop handoff. The [accepted Witch evidence](../../../docs/research/presentation/special-screens.md)
and [Map3 start observation](../../../docs/research/chapters/grans-opening/map3-admitted-start.md)
own that boundary. New Game and its initial save carry Map3; old saved zero values are preserved
without migration. Text settings are
message speed2/mouth0/view0. Each map must provide admitted `basePalette`; initial current/base
colors are the Map3 base and fade period3 follows pinned `code/gameflow/start/systeminit.asm` and
`gameintro.asm` writes to `FADING_COUNTER_MAX`. This is static source plus product initialization,
not a new original runtime/timing claim. Missing palette metadata rejects before session creation.
See the [preparation/admission owner](../content/profiles-and-trust.md#opening-new-game-admission).

## State authority and save boundary

[GameSession](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) remains the sole publisher.
[ExplorationState and StoryState](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
and [BattleStartInput](../../src/Sf2.Remake.Domain/Battles/State/BattleStartInput.cs) already own the
carried field state. A save DTO is a detached value exported by Application from one admitted revision,
not a second mutable session and not `SessionSnapshot` serialization.

| State | Authority and persistence treatment |
| --- | --- |
| Ally resources/progression | Field `Party.Actors` / active battle actors; persist every supported retained ally's ID, current HP/MP, EXP, kills/defeats, status, initialization/absence, attained level/base growth/maxima and learned spells. Persist current class if mutable in admitted content; otherwise bind its immutable class identity. Never regenerate attained growth on load. |
| Equipment/inventory | Carried `SourceLoadout` and progression; persist all four complete item words and spellbook consistency. Recompute derived attack/movement from admitted rules; validate against class/equipment limits. No separate UI inventory copy. |
| Flags/membership/following | `Story.Flags` plus content party mapping; persist flags and any authoritative non-flag membership/order for authored packages. Validate `ActiveAllies` against membership rebuilt from flags. V6 retains the separate counted `Story.PartyLists` register, which can lag flags. The settled visit owns physical slots, follower links and follow phase. |
| Gold/Deals/receipts | Save carried gold, inventory and `ItemCustody` (its own once-flags, Deals counts and depot). Field Search commits the pickup's source flag in `Story.Flags` with the grant and any chest layout change. The Search result receipt belongs to its pending wait. V6 preserves the separate settled `Story.ItemReceipt` and `DialogueItem` text registers. Capture requires the active transaction to have finished. |
| Map/position/geometry | Content owns the base layout and definitions. The snapshot binds the loaded body and population, live player and actors, layout differences, active roof backing and supported ambient cursors. Legacy recipes reconstruct their recorded anchors and scenes. |
| RNG and world counters | Persist both live seeds and supported step counter/battle-request state (request must be false). Preserve EgressMap and any future gameplay counter actually read after restoration. Reject unknown required counters; do not confuse wall-clock time with simulation state. |
| Story execution | Cursor, callers, waits, event caller, warp, battle entry/outcome and skip must be absent at save. No tokens/continuations serialized. Persist text cursor or other scalar semantic registers if subsequent programs read them without assignment; clear only under a documented recipe invariant. |
| Logical display/audio | Close windows; reconstruct logical view, base/restored palette, player/follower sprite requests and required text settings from admitted map and saved semantic settings. Preserve current music cue and known previous-cue history where later music instructions consume it; bind a fresh generation. Retain logical phase and completion permission for admitted finite cues. Native attachment uses the available PCM delivery boundary. |
| Host/presentation | New session ID/revision/observation sequence, input release barrier, menu focus, camera smoothing, nodes/textures/PCM handles and caches. Never persist resources, services, private paths or completion receipts. Settings persistence remains separate from game progress. |

### Eligibility and restoration recipes

This section records the historical field v1/v3 writer criteria and the reconstruction rules for
the retained New Game bootstrap, anchored and scene-bearing readers. Ordinary field Save and
Church Record use the [settled snapshot](#unified-settled-field-snapshots).

The legacy writer queried eligibility, then rechecked the offered session ID and revision on
capture. It required all of the following:

1. Active exploration, successful PlayerInput/FieldInput boundary, no stopped failure, active battle or selection.
2. No story cursor/callers/wait/event caller, pending ordinary/script warp, entering/outcome route,
   retained battle player binding or entry skip; no service/receipt/item transaction in flight.
3. Player exactly settled on a legal cell, no player/follower commanded motion or unfinished sprite
   readiness; closed/nonmoving text/portrait/choice, no scrolling logical view, fade/quake or pending
   roof/block-copy transaction. Background music must have no required completion outstanding.
4. The location/flags select one admitted restore recipe; its anchor is legal/unoccupied after rebuild,
   followers fit, and every durable map effect is accounted for. Unrecognized setup/phase or required
   unknown party/accounting state returns an explicit unavailable reason.

Ambient NPC walking need not stop forever to make saving possible. The recipe identifies which
default NPC scripts may be discarded/restarted; only those ambient scripts can be active. A story
installed move, hide or follower change that cannot be reconstructed is a rejection, even if player
input is available. Snapshot acquisition pauses command/tick dispatch for one revision; saving does
not drain waits, poll NPCs to “settle,” draw RNG, heal the party or run an event.

First recipe scope is deliberately finite. The map numbers, story predicates and reconstruction
facts below belong to validated Content or the explicit SF2 profile policy, never to hard-coded
opening checks in generic `GameSession`, save DTO validation or filesystem code. Generic capture
asks the selected profile for a recipe and validates its typed result using existing map invariants.

- **Map3 opening, F603 clear:** restore at `(56,3)`, Down with phase-correct population. This includes
  ordinary stable intermediate opening states and optional conversations, not just the first
  frame. Preserve their one-time flags and completed local effects; reconstruct any required
  phase-dependent hides/positions without replaying the dialogue or join transaction.
- **Map3 joined opening / defeat return:** after the messenger/Zone8 has finished (F603 set), normal
  supported setup and no active story; restore at `(32,13)`, Up, the existing admitted Map3 egress
  anchor. Preserve completed gate/palace/Astral/unlock flags, party and receipts. Rebuild the
  source-selected population, F603 entity142 retirement, follower allocation and flag layout effects;
  clear only the map-local entry bits that the normal reload owns. Do not heal or replay JOIN.
- **Map57 Battle01 victory return:** after full `battle-returned`, F501 set and F506 clear; retain a
  dedicated resume anchor from the actual outcome landing tuple, facing Down. It is not the player's
  later exact position and not `Story.ReturnAnchor`, which can describe the previous map. Add this
  small location-owned anchor at completed return; validate it against Map57 content on capture/load.
  Restore the void setup/player/followers and flags without `abcs_battle01`, stat reset or reward replay.

- **Map58 Battle07 aftermath:** after the complete outcome return, F507/F74/F640 set and
  F407/F999 clear, retain the completed return anchor (ordinary first victory is `(13,35)`, Down).
  Map58 has no setup init to replay. Rebuild its flag-selected layout/population and royal followers;
  retain casualties, equipment, names, gold, status and both RNG streams. The existing stability and
  anchor collision/occupancy checks apply. Egress may retain the admitted early refuge Maps0/3/5/43/16.
  This adds the `battle07-return` recipe to the existing format, without changing revision4 party
  generation. The quake town and scene-only damaged-ship arrival are not save locations.

- **Granseal F506 settled visit:** the `granseal-field` recipe retains the current legal tile,
  facing and layer, the actual actor/alias tables, follower links and working-layout differences.
  Continue restores the visit with the selected Talk, Search and warp bindings. A named Kiwi can
  still follow on that visit. Fresh entry retires his resident actor while retaining his membership.
  Church Record requires the imported Map3 refuge. See [the bounded observation](#granseal-f506-save-and-continue-observation).

- **Galam town settled visit:** the `galam-field` recipe retains the actual Map16 setup,
  legal tile, facing, layer, physical cast, aliases and layout. Continue preserves a default
  visit after the rally, even when F663 is set. An ordinary reentry selects the changed
  population. F505 preserves the completed guard and join state. Both priest callers use the
  imported Map16 refuge for Record. [Bounded observation](#galam-town-save-and-continue-observation)

- **Yeel settled visit:** the `yeel-field` recipe records the current active-area tile, facing
  and layer. Use the default setup or a concrete F650 setup selected by the world. The visit
  snapshot retains settled actors, aliases, follower relationships and the working layout.
  Continue restores that visit without running setup init or Talk again. Fresh entry still
  selects population normally. Church Record uses the selected Map5 church anchor for Egress
  and commits F399 only with successful slot publication. The published `yeel-f650-v1` world
  supplies both early setups. Worlds missing the selected F650 body still reject that visit. F530/F28
  remain outside this recipe. See the [location coverage](pre-voyage-optional-locations.md#pre-voyage-save-coverage).

- **Hawel's house settled visit:** `hawel-field` retains the legal tile, facing and layer after
  Kazin's recruitment and the house caller finish, and on quiet F612 revisits. The first completed
  visit retains the default setup's live cast, Kazin's follow link and the changed layout. A fresh
  revisit uses the F612 population and bindings. Ordinary Save preserves Egress and F399.
  [Bounded observation](#hawel-house-save-and-continue-observation)

- **Granseal Castle settled rooms:** `granseal-castle` keeps the current tile, facing, layer,
  setup bindings, working layout, and physical actor state across Maps19/20/21. It retains
  completed choices, receipts and royal follow links. Normal Save preserves Egress and F399.
  [Bounded observation](#granseal-castle-save-and-continue-observation)

- **Settled ancient tower visits:** `tower-field` captures the current position and loaded scene
  on Map40, the F506-open Map57 staircase, the first five linked Map60 areas, and the Map58
  aftermath under F506/F507/F74/F640 with F407 clear. F999 is excluded. New captures prefer this
  recipe in its admitted areas. Continue selects either older completed-return recipe when the
  slot names it and its original anchor and flags are valid. Ordinary Save leaves Egress and
  F399 as received. [Bounded observation](#tower-save-and-continue-observation)

These are legacy product recipes. Their recorded acceptance includes
ordinary legal variations within each admitted boundary: reordered optional conversations,
refusal/retry, changed party resources/equipment, living/dead members, earned progress and different
valid flags/landing tuples. Missing handling of such a state is an implementation defect or unfinished
acceptance, not permission to disable it as “uncovered.” A genuinely unsupported later Map3 setup
must still reject even when F603 remains set. Avoid enumerating walkthrough IDs: guards are
map/setup and semantic phase capabilities. No universal
snapshot engine or second scripting language is needed: extend the existing map builder with these
finite restoration inputs and reuse its population/layout helpers. #1423 must reassess recipes when
new persistent map mutations become reachable.

Restore builds a candidate off-session: admit content → validate saved state → select recipe →
rebuild world/projections → validate the first field boundary → mount required resources → publish
the new session. These ordinary savepoint recipes must not call `GameSession.Start` with saved flags
and replay on-load. The separate pre-on-load initial recipe above intentionally runs the actual
opening setup, with fresh progress and already initialized party values. Reuse shared pure
construction where necessary. Ordinary savepoint Continue must not trigger a battle, service, heal,
growth draw, one-time script or reward while reconstructing. A failed candidate
leaves the current session/slot usable; disposing candidate resources follows existing host ownership.

The seeds in the candidate equal the saved seeds. Restoration itself has no gameplay RNG draws;
ambient programs begin on the first eligible automatic ordinary step. Repeated loads with the same content/rules
and semantic input/poll sequence must agree in gameplay results. Comparison with uninterrupted play
is limited to persistent resources/progress: NPC phases, elapsed music and subsequent draw timing
can differ because this is a savepoint reload. This distinction must appear in player help and tests.

### Unified settled field snapshots

**High rigor.** [#2045](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2045) adopts
one snapshot policy for saving the current stable visit. The risk is a successful-looking Continue
that changes the next interaction, random draw, return or payment. This design uses the accepted
code at `2cf93119` and the existing observations below. The current field writer/reader uses
v6. The retained recipes own field v1/v3. A map or setup using existing mechanics enters through
configured content admission and the shared stable-visit checks. Capture and Continue must resolve
the exact body, population, action sources and resources. A missing binding or unsupported operand
requires an explicit refusal until its owner provides support. Arbitrary mid-script suspension,
new campaign admission, original SRAM and host lifecycle #1716 remain outside this scope.
[Battle Save](battle-save-and-continue.md#shared-snapshot-boundary) shares the store and admission
model and retains its own settled player Movement variant and v2/v4/v5 reader.

#### Confirmed capture, restore and attachment path

1. `GameRoot.RequestFieldEntry` gives unfinished movement and sprite readiness up to 120 ordinary
   services. Successful entry holds gameplay while the host observes current music and offers a
   revision. `GameSession.QueryFieldSnapshot` checks settled control and validates a detached
   candidate. The host can refuse an unrestorable finite music phase. `CaptureFieldSnapshot` and
   `RecordChurchSnapshot` recheck session identity and revision. Query and candidate capture
   leave gameplay clocks and RNG untouched.
2. `OpeningPersistenceHost.ConfirmSave` sends the snapshot to `LocalSaveStore.Publish`.
   Church Record publishes its admitted refuge/F399 revision only after successful slot readback.
   `OpeningSaveJson` encodes field v6 and continues to read field v1/v3 and battle v2/v4/v5 within
   the same envelope. Configured content owns all bindings. The store validates the candidate,
   serialized temporary file and published readback.
3. `SaveRecordRestore` binds v6 to `SettledFieldSnapshot.Build`. It resolves the saved map/setup,
   population and action source references, reconstructs the player, carried progress and semantic
   registers, and compares the reconstructed values. Existing v1/v3 records still use
   `OpeningRestore.Build`. The New Game bootstrap runs ordinary on-load.
4. `GameRoot.MountSession` prepares restored field audio and a disabled, hidden exploration view.
   The view resolves the camera and loads the initial map planes and visible sprite resources
   through the ordinary renderer. Successful preparation allows explicit backup publication
   where requested, then the host adopts the prepared field and releases the previous session.

See [session entry points](../../src/Sf2.Remake.Application/Runtime/GameSession.cs),
[v6 capture and detached build](../../src/Sf2.Remake.Application/Runtime/Persistence/SettledFieldSnapshot.cs),
[legacy restore](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs),
[codec](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs),
[slot store](../../game/src/Persistence/LocalSaveStore.cs) and
[host entry](../../game/src/Entry/OpeningPersistenceHost.cs).

#### Snapshot operands and counterexamples

This table records the pre-cutover behavior and design targets from #2045. The middle column
describes gaps **Confirmed** in the named producers and readers at that baseline. The right
column records the proposed value contract. See the implemented [physical visit boundary](#implemented-physical-visit-boundary),
[settled field candidate](#implemented-settled-field-candidate) and [field snapshot cutover](#implemented-field-snapshot-cutover)
for current behavior. Linked route examples retain their original observation scope.

| State | Pre-cutover capture/restore and the gap | Proposed settled snapshot |
| --- | --- | --- |
| Content and loaded field bindings | `SaveCompatibility` carries profile/package, gameplay, rule/recipe revisions and pinned provenance. `SavedFieldScene.Setup` is supplied by individual recipes. `MatchesLoadedSetup` compares bodies, and Mine/New Granseal/Creed infer setup from entity lists alone. `Sf2FieldSceneRestore.Rebuild` accepts a default body fallback for unmatched setup IDs if the caller lets one through. Its manually constructed map omits `Caravan` and `ChurchSavepoint`. On the [Galam post-rally visit](#galam-town-save-and-continue-observation), F663 is set with default Talk/zone bindings and retired slots still loaded. A fresh F663 entry uses a different body. `MapTransfer.Scroll` can also change map bindings while retaining the previous cast. | Record the actual map and loaded body at selection/load. Resolve default, named variant or map without a selector strictly against configured content. Keep map/setup bindings distinct from the provenance of retained population if a scroll or `SceneEntities.Reload` changes only one. Bind traversal/view areas, input, Talk, Search, zones, warps, layout events, palette and vehicle capability via the same pure binding used by ordinary load. |
| Position and legal control | Field `Map/X/Y/Facing/Layer` sometimes comes from a legacy anchor. Scene-bearing rows duplicate the player pose and compare it at rebuild. `FieldLayout.Word` reads the current layer through the area offset. `RequireUnoccupiedLanding` only runs on some paths. The Battle02 enclosed return is a blocked cell with a recovery action, despite ordinary input being available. [Enclosed return](pre-voyage-campaign.md#enclosed-battle02-return-and-field-egress) | Use the saved player's physical pose as the Continue landing and derive any display tuple from it. Check area, aligned position, supported layer, collision and occupancy through the live traversal/actor rules. Preserve legal follower overlap. Under the present legal-ground gate, keep the enclosed return's existing EGRESS recovery. Keep secondary-layer addressing and the active area distinct from camera smoothing. |
| Working layout and roof | `SavedFieldScene.Layout` is a sparse difference against source words, including copied cells. `Rebuild` applies it and then calls `RoofOnLoad` with an inactive roof. `MapBlockCopyLifecycleReducer.Activate` backs up the current destination words, and `Restore` writes that backup. Saving under an active roof therefore can back up the already-overlaid words on Continue, or lose the active record if its activation position no longer matches. Flags alone also lose same-visit door/step copies. [Layout reducer](../../src/Sf2.Remake.Domain/Maps/MapBlockCopyLifecycle.cs) | Apply saved differences once. Store active roof record identity and its saved-under words when present. Derive and validate its bounds/count against the bound roof row. Use the exact working words and backing when later movement restores the roof. Derive overlay offsets and redraw requests from content and current area. |
| Physical cast, aliases and vehicle state | Scene rows save identity, slot, X/Y/facing/layer, flags/speeds, visibility, sprite/size/priority and follower leader/offsets/wait rule. Aliases and ordered `CurrentEntities` are saved independently. Current restore assumes dense 0-based slots and identities from current setup/followers plus recipe-supplied allocations. The [castle commission](#granseal-castle-save-and-continue-observation) retains retired `cs-53176` slots after F610, while fresh F610 entry never allocates them. `SceneEntities.Add` computes next slot from aliases and may reuse a physical slot. `RaftRuntime.Declare` can create sparse slot 31 with `entity-159`. Map72 capture currently strips the local raft before rebuilding it from the berth. | Preserve physical slots, player binding, real aliases (zero versus removed), ordered current list and follow graph. Validate profile-specific allocation ranges, unique occupied slots and cycle-free links. Keep source `entity-0`/slot 0 for source populations, with authored player IDs and sparse slots where their producers support them. Bind resident/follower/dynamic identities to their admitted population or actual allocation site, via a content instruction reference or existing engine-owned allocation role. Validate references without running allocation. Persist the berth as the durable state for other maps and the actual local record when present. Attach without declaring it a second time. |
| Entity motion, actions and appearance | `Sf2FieldSceneRestore` sets all destinations to the stored coordinates, drops velocity/travel, acceleration, animation counter and wait timer, and guesses a resident action program by identity. `OpeningSave.RequireStable` sees only visible entities and `CanDiscardEntityProgram` accepts an equal program on another resident or the messenger's loop. `EntityActionRunner.Tick` services physical X below `0x7000` and `RandomWalkEntity` consumes MainSeed. Starting the loop again can change the very next draw. Ketto's idle merchant and Pacalon's idle speed-8 centaur require explicit suppressions today. `FollowerMotion.Tick` treats `Actions == null` differently from installed idle. `SharedSpriteSlot`, `SpriteGraphics`, global `SpriteSize` and completed shadow suppression are absent from scene DTOs. | Preserve no-action versus quiescent idle, and allow only admitted continuing ambient sources with content-bound action origin and validated cursor/timer, plus motion state read on the next step. Keep destination/velocity/travel for an ordinary ambient actor in motion, and retain acceleration where the next movement reads it. Preserve semantic appearance operands read by attachment or later commands, including static frame selection, shared graphics and size. Reissue local sprite readiness instead of writing request counters or handles. Reject required work without a bounded binding. |
| Party, receipts and field registers | `OpeningSave.CaptureValues` carries class, attained/derived-stat inputs, current stats, HP/MP, status, level/EXP, kills/defeats, activation/init/absence, all item/spell words, active order, names, gold, once bits, Deals and depot. `OpeningRestore.Ally` validates class/resources/equipment and reconstructs spells. `MapPartyMembership.Rebuild` uses flags and checks active order. Story flags include earned field pickups and completed choice/join effects. `Story.ItemReceipt` and `DialogueItem` are separate text operands. `ProgramRunner` grants custody before the receipt text and subsequent flag writes. The presence of a retained receipt alone also cannot prove an open transaction. | Reuse carried values, knownness/range checks and roster identity checks. Keep settled flags and custody unmodified by entry or reward code. Rebuild derived party lists and stats consistently. Keep encounter identity and the effective new-battle options. The connected outcome constructs fresh carry without queue, battle visuals or death credit. Preserve any settled receipt, item or speaker register with a later reader before assignment. Gate on active transaction ownership, including grant with caller/receipt still pending. Do not use an item word as a surrogate once flag or a joined flag as a follower link. |
| Clock, RNG, display and music | Both seeds, signed `StepCounter`, text cursor/settings, fade period and cue/history survive. `MapTransfer.Apply` would subtract from the step counter on entry. `ExplorationTextRunner.Initialize` resets logical windows/view and clears `EntityServices`. `ExplorationDispatcher` reads `EntityServices` when deciding which services run. `WindowFixPending` affects a later window pass. `PaletteColorTransition.Request` reads `SimulationTick` relative to its prior mapping. The field rebuild resets these registers/relationships; battle already saves some of them. | Save both RNG streams, counter and live semantic service/text operands. Compare the first ordinary service or input after Continue under the same logical command/tick sequence. Hold the boundary at closed windows, settled view and settled display. Derive ordinary camera/view from area and actor when equivalent, retain bound target or completed color mapping when read later, and refuse an unrepresented live transition. Express relative deadlines as relative counters if needed. Rebuild playback at step zero only for admitted looping cues with known history and no required completion. Derive the debug RNG byte from ThinkingSeed. Reset host pacing debt, input release, smoothing, observations, session ID and tokens. |

**Inferred recommendation.** Extend the existing detached field scene and shared progress
validation into one field value contract. Capture actual binding/provenance at its normal producers
and admit exact references at restore. Compare reconstructed semantic operands against capture,
then take the next normal input or tick in representative controls. A checksum-valid DTO and
`CanWaitAtInput` alone would miss the roof and ambient counterexamples. Reuse the current
bounded scene/layout/party structures, position once, explicit knownness and the existing store.
An entry replay would require reverse-engineering which effects to undo and cannot distinguish the
two F663 visits. Dumping `SessionSnapshot` or serializing interpreter actions would bind files to
runtime object graphs, waits and resources and extend suspension beyond the stable boundary.
Saving only more variants of `SavedFieldScene` through `Select` keeps the same need to author
reconstruction exceptions on every map.

#### Implemented physical visit boundary

**Confirmed (engine).** Ordinary loading records the exact map and setup body. CURRENT placement
retains it, a fresh transfer selects a new body, and a scroll or entity reload can change one
binding while retaining the other. [The shared map binder](../../src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs)
resolves the stored body without flags and supplies the ordinary traversal, view, event, Search,
palette, Church and vehicle definitions. Map, explicit scene-population and allocation references
resolve against the admitted definitions. Entity action installs record the resident, allocation,
story instruction, zone row or engine producer that supplied the program.

[Physical capture and reconstruction](../../src/Sf2.Remake.Application/Runtime/Persistence/FieldVisitSnapshot.cs)
carry the player binding, raw physical motion and animation, sparse slots, retained and retired
actors, alias values and removals, current-list order, follower links, sprite sharing and graphics,
global and per-actor size, priority and completed shadow removal. A local raft record and the
durable berth remain distinct. The working layout stores differences from the loaded body and an
active roof stores its record ordinal and saved-under words. Reconstruction binds and validates
these values without running entry, population allocation, flag writes, chest/step copies, roof
activation or rewards. A later walking step can restore the backed-up words.

The physical visit supplies the map and actors for live field Save and v6 Continue
through the [settled field candidate](#implemented-settled-field-candidate).

Use `uv run sf2 verify engine` for [Galam Talk](../../tests/Sf2.Remake.Engine.Tests/GalamSaveTests.cs),
[castle dynamics and follower motion](../../tests/Sf2.Remake.Engine.Tests/CastleSaveTests.cs),
[secondary-layer steps](../../tests/Sf2.Remake.Engine.Tests/LayeredFieldSaveTests.cs) and
[transfers, sparse raft and roof exit](../../tests/Sf2.Remake.Engine.Tests/FieldVisitSnapshotTests.cs).
These controlled engine visits take an ordinary successor with equivalent story controls and verify
physical or interaction results. Legacy field commands have their own controls in the same project.

#### Implemented settled field candidate

**Confirmed (engine).** [SettledFieldSnapshot](../../src/Sf2.Remake.Application/Runtime/Persistence/SettledFieldSnapshot.cs)
offers the current session and revision, checks them again for capture and rebuilds a detached
PlayerInput candidate using exact content compatibility. Capture and rebuild apply the ordinary
control predicates, physical landing and readiness checks. They compare recaptured values and the
live action stream against its producer binding. These calls run no program, tick, service or
publication. Regional recipes still own existing records. The current player supplies the Continue
position, while the independent Egress register resolves against admitted Church or route refuges
and the configured legacy defeat fallback. Active battle return, transfer and service owners must
release control first. Completed outcome anchors drop out of the field candidate.

The [action binder](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldVisitBindings.cs)
admits closed random-walk and closed relative patrol cycles from a bound resident or story
installation. A relative patrol alternates awaited moves and waits, returns to its starting
offset and jumps back to the start of the cycle. A stationary facing cycle repeats facing, sprite
refresh and timed wait. It can save after the sprite owner releases the current request. The next
refresh receives a new request in the restored session. The reader's appended stop is unreachable
after the closing jump. Configuration-only prefixes and the engine-owned Caravan walking follower
program with its physical leaders are also admitted. The wandering template emitted by `walking_actions` has a timer,
speed, acceleration and flag prefix. A story prefix with other effects must complete before the
closed cycle qualifies. Cursor, timer, destination, travel, velocity and seeds continue together.
The action service uses physical X, so an invisible actor inside the active range can still move
or draw RNG. Retired off-map records retain their bound dormant phase and aliases or removals.
Terminal idle stays installed and takes its own timer service. A missing program leaves the
generic follower scheduler eligible. Unfinished actor motion without a supported continuing
owner, pending sprite readiness and unbound serviced work refuse capture.

| Settled value | Producer and next consumer |
| --- | --- |
| Party progress and story registers | The shared carried-value checks reconstruct admitted classes, spells, equipment, roster, active order and custody. `JoinPartyMember` can publish a counted list before its conditional active bit. The candidate preserves counted lists and validates active order from flags and membership. It retains completed item receipt, dialogue item, speaker, text cursor/settings and names for later text commands. A receipt can survive after its owning grant/caller has completed. |
| Field service and view | Ordinary dispatch reads the nullable entity-service register. The next window pass can read the retained fix bit. The loaded view area and autoscroll, follow and plane phase bind to the stored map body. Closed text geometry is recreated when the next window opens. `ProgramRunner` restores player view ownership on field input. |
| Appearance and clocks | Both RNG streams, signed step count and logical simulation tick survive, along with the active base palette and fade period. A completed color remap retains both lookups because a later color restore reads the previous copy. The palette transition and sprite acknowledgements receive fresh presentation bookkeeping. The high byte read by gameplay comes from ThinkingSeed. The last text-poll diagnostic copy is omitted. |
| Music | The current cue and ordered history survive. A bound finite cue retains logical clock and completion eligibility used by the sound helper, with a fresh generation. An untimed cue has no consumer of its former timing registers. The implicit map default stays implicit. The detached boundary carries no playback handles, sample offsets or delivery waits. |

The accepted mode is walking, including visits with a raft berth and the supported local physical
record. Raft and Caravan riding, the enclosed Battle02 recovery cell, pending motion without a
bound continuation, arbitrary interpreter suspension and unfinished view, resource or color work
remain outside this boundary. Existing Egress behavior continues to handle the recovery cell.

Run `uv run sf2 verify engine` for
[settled field successor controls](../../tests/Sf2.Remake.Engine.Tests/SettledFieldSnapshotTests.cs)
and [battle return controls](../../tests/Sf2.Remake.Engine.Tests/BattleSaveReturnTests.cs). The
controls compare subsequent ordinary RNG and motion in ambient wait and movement phases, retained
idle and following, next receipt text, completed custody/join without replay and a second color
command. They refuse a real in-flight grant, Church service and warp, pending battle/outcome
ownership and stale identity or revision. Victory controls move after detached reconstruction
and preserve the refuge and actual landing even when the prior return anchor differs.
The [cutover controls](#implemented-field-snapshot-cutover) exercise these values through numbered
files and fresh host attachment.

#### Eligibility and return semantics

- **Field input.** Require `CanWaitAtInput` and the applicable stable mode, with no fault, caller,
  interpreter continuation, selection, active text/choice/portrait, script or ordinary warp,
  battle request, entry skip or outcome handoff. A player or following actor with pending
  commanded motion/readiness waits for the existing bounded ordinary settlement. A moving NPC
  controlled by a supported ambient program can continue from its bound phase. Installed story
  actions may survive as completed pose or idle state, or as positively identified ongoing ambient
  work. Unsettled scripted motion, fade/quake, sprite/graphics completion and layout/view
  transactions reject. An active roof with a complete backing is settled state.
- **Transactions.** Reading a system or slot menu selection never advances a service transaction.
  Dialogue, naming, Search receipts, Church/Shop/Caravan actions, field item confirmation and
  battle entry/outcome can wait for input while they still own work. Require those owners to finish
  or cancel. A completed purchase, join or reward is already in flags/custody/actors. A partially
  processed grant or payment still has a caller or completion owner. Classify by those operands
  rather than by the presence of an old receipt, a story flag, an arbitrary slot count or an NPC
  being out of frame. Query and capture never tick, spend, heal, poll waits or run a program.
- **Travel and unsupported values.** Initial cutover preserves settled walking and existing
  berth-bearing walking visits. `FieldVisitSnapshot.Capture` rejects riding. Stable riding is a
  mode/operand gap, including the current vehicle marker, parked slots, follow or terminal idle
  and next boarding/landing rules. Admit it only after its mode contract round-trips, using the
  same scene path. Return a typed reason while a required mode, action origin or live binding
  cannot be represented. The direction does not widen the admitted content frontier.
- **Continue position.** Use the saved scene player. `Story.SaveAnchor` written by the finished
  outcome in `ProgramRunner` serves old `battle01-return`/`battle07-return` capture, and survives
  in old battle payloads. Its earlier landing can differ from the actual current field position.
  The new field writer has no need to synthesize or consume it.
- **Egress refuge.** Preserve the independent `Story.EgressMap` and F399. Validate known
  references by the admitted refuge/route data used by `FindEgress`, including real non-Church
  refuges (Map43), and keep admitted legacy Map0 as a register value with its existing defeat
  fallback, never a fabricated general fallback. A Church refuge row alone is insufficient
  evidence that the visit offers Record. Use the actual Church caller and its admitted refuge or
  existing no-op Record behavior. `RecordChurchSnapshot` writes a candidate with the selected refuge
  and F399 and publishes that story revision only after successful readback. Cancel, stale
  confirmation and definite failures leave live progress and slot bytes intact. An uncertain
  publication requires inspection. `FieldEgress.Begin` keeps its walking/capability and
  F399/F64/F640 guards. Battle Egress/defeat keep their fixed/live, fallback and special destinations.
- **Transfer and outcome.** `MapTransfer.Apply` writes `ReturnAnchor` as previous-map bookkeeping;
  current ordinary command dispatch has no reader that needs it after settled field load.
  `BattleOutcome` and `ProgramRunner`'s `ReturnBattleMap` use the active `OutcomeReturn` and
  battle route. Reconstruct that route in the battle variant. Reject the active field handoff and
  omit stale outcome operands once it finishes. Continue runs neither ordinary entry nor the
  battle outcome. The next ordinary exit or stair still uses its normal warp and load mode.
  CURRENT stairs retain the scene via `FollowerMotion.Arrive`. A fresh entry selects population,
  applies entry flags and runs destination init according to the normal load path. The initial
  New Game slot is the explicit pre-on-load bootstrap. It continues to start the opening once from initialized values.

#### Disposition of regional rules

| Current rule or branch | New writer and reader disposition |
| --- | --- |
| `Select` recipe and location flags, including Map3 default/F609, Battle01/07 returns, Granseal F506, Galam, Hawel, castle, tower and every later branch through Pacalon | Remove from new capture and snapshot restore. Current-area legality, bound content and completed work are checked once. Keep existing gates, relocated anchors and descriptions for their old records. Current maps with missing or unsupported content still fail at the affected binding or operation. |
| `ValidateRegisters` scene and Egress combinations, `CaptureScene` map dispatch and specialized `Rebuild` branches (`Sf2CastleRestore`, `Sf2TowerRestore`, `Sf2NewGransealRestore`, `Sf2MineRestore`, `Sf2CreedRestore` and inline cases) | Scene requirement is intrinsic to the new format. Retain legacy shape/rebuild checks only on old records, including alternate-setup acceptance, old Egress combinations, tower area/F999 gates and Map57 flag-copy equality. Generic content binding replaces setup-name/flag whitelists and validates setup bodies, actual geometry/layers and resource IDs. Existing recipe policy must not be called by the new reader to decide if a scene is legal. |
| `Rebuild` reconstructs Sarah/Jaha, the Hassan cast, Taros/Floor World/Creed positions or a Polca raft boarding probe. Other branches require Hawel's specific follower or suppress Ketto/Pacalon resident actions. `CanDiscardEntityProgram` has a Map3 messenger exception. | These are historical reconstruction workarounds. Live state supplies poses, identity, follower links, berth and action phase. Require a valid general graph/allocation/binding. Keep the actual entry, movement and boarding rules with their gameplay owners. Preserve captured scene actions by their real origin, including the messenger's post-installation loop. |
| `SceneEntities.Add` and regional `AddMapEntity` scans for the castle procession, North Cliff blast and Janet | Keep the 49-record source cap, alias selection/slot replacement, and special vehicle slot rules at their owning producers. Resolve the captured grant of a slot by its admitted instruction/engine role. Retain regional scans only when needed to reconstruct or validate old records. An arbitrary entity ID from a different program is insufficient. |
| Town-specific Church gates in `RecordAtChurch`, and legacy location labels in `SavedDestination` | Carry the Church selection through the real service return and bind the refuge and existing Record effects in the service/content contract. Earlier no-op Record and bound priest callers still have their accepted effect. Keep named route/refuge rules in SF2 gameplay. New labels use bound map/place information with the actual landing when needed. Legacy labels remain tied to their original semantics. |
| F603/F506/F663/F612/F609/F610, F407/F507/F640/F999, later battle flags and overloaded travel/entry bits | Continue carries the exact settled values. Fresh entry, true scene gates, map substitution, event bits, receipt checks, recoveries and battle-route consequences stay with their owners. For example, F407 still starts the tower encounter, F640 still substitutes quake town on travel and gates Egress, and F611 still controls the bridge layout. These flags no longer describe which regional save reconstruction to run. |

#### Reader and writer cutover

Ordinary field Save writes v6 with a required exact visit, party and register record. Required
operands must be present. Declared optional operands use key absence. Source references use
closed tags with only the operands for that tag. The codec reuses the checksum envelope, strict
keys, 1 MiB and depth32 bounds. The shared compatibility revisions still govern content, rules
and historical roster expansion.

Continue reads v1 (including historical nameless and scene-bearing shapes), v3 and battle
v2/v4/v5 with their existing compatibility. The v1 New Game bootstrap remains the pre-on-load
checkpoint. The legacy scene reader binds the selected historical body, its resident and follower
rows, and unique allocation instruction references already admitted by the old recipe. The old
record establishes the starting state of this reconstructed visit. Where the old shape cannot
establish a unique origin, Continue can play the reconstruction and new Save reports the missing
binding. Legacy reads preserve the selected file bytes.

All formats share validated atomic publication and readback. Roll out using fresh numbered slots
and keep an untouched compatible older slot. The pre-cutover binary classifies a v6 primary as
`save-version` even when the backup is an older compatible record. It refuses promotion and writes
to that occupied slot. See the [storage and rollback contract](battle-save-and-continue.md#future-shape-protection-and-binary-rollback).

#### Mechanism implementation and acceptance

These slices require separate implementation authorization and use the existing High recovery
contract. Keep player-owned slots, content and processes out of the acceptance setup.

1. **Exact visit binding and physical state.** Carry loaded binding and allocation provenance
   through normal load, scroll, scene-entity changes and action installation. Reuse map/setup
   binding and field layout/slot validation in a detached candidate. Assert same-visit Galam with
   F663 and fresh reentry, one dynamic allocation/retirement plus follower chain from the castle,
   secondary layer, one active roof which restores different saved-under words, a CURRENT stair
   and one fresh transfer. Include a retained-population scroll or sparse raft slot in a walking
   visit. Compare actual next movement or interaction as well as payload values.
2. **Stability, registers and next work.** Bind the admitted ambient program instead of restarting
   it. Compare the next ordinary ticks and RNG for an actor in a wait or move. Distinguish stopped
   merchant/idle from the original resident program and generic follower scheduling. Exercise
   moved or hidden actors, one committed pickup/join and an interrupted current grant/service,
   pending warp/battle and stale confirmation. Admit only modes with a legal checked successor.
   Finish each extra vehicle mode with a short walking → riding → stable Continue → landing
   control before enabling its capture. Define completion/normalization at the real owners for
   any register that can be read across settled input. Reuse the battle restore's compare-after-
   reconstruction pattern and a finished outcome → field snapshot → normal movement control.
3. **Disk reader then writer/host cutover.** Check accepted v1 anchored Map3 and both tower returns,
   representative named scene v1/v3 (town and castle/tower), historical roster binding and
   battle v2/v4/v5 with matching content. Confirm new reader round-trip and next input before
   enabling new writes. Check actual Save and independent title Continue at a legal visit that was
   previously outside the whitelist. Exercise genuine Church Record, ordinary Save preserving its
   refuge, definite publication failure, corruption with explicit recovery, and incompatible
   version/content/shape refusal with unchanged slot bytes. Check pre-cutover binary refusal using
   a new primary with an older valid backup. Compare current actor/input, next action and a real
   battle or temporary-map return through the existing host observations. Check field resource
   failure using the existing detached attach pattern where the new boundary can fail.

Reuse `GalamSaveTests`, `CastleSaveTests`, `LayeredFieldSaveTests` and `TowerSaveTests` as bounded
behavior controls where their fixtures match. A new roof test must leave the roof and compare
the exposed underlying words. A round-trip check of the overlaid words alone would pass the
wrong reconstruction.

Use one configured world and one disposable slot pair at a time. The admission work follows
the bounded working layout, roof backing, physical cast and carried progress, with references
into already admitted content. Other serviced action families require an identified continuing
owner and a successor control before admission. Original standing cadence and SRAM parity remain
**Unknown**.

#### Implemented field snapshot cutover

**Confirmed in the remake.** [#2049](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2049)
covers ordinary Save and a fresh title Continue at Map17 (18,4), facing west, from the published
controlled kitchen start. The pre-cutover binary
refuses ordinary Save in this kitchen visit. V6 binds the actual body, Slade's physical following
and the residents, including the closed patrol of slot 12. Continue binds the native map camera,
returns to that pose and accepts the next Left input through the ordinary warp to Map16. The
encoded Map17 record is 19,030 bytes. The native Record and ordinary Save in Galam produce
52,206 and 52,202 bytes. The decoded payloads nest six containers deep, within the shared depth32
and 1 MiB encoded-file bounds.

| Control | Result |
| --- | --- |
| Genuine Galam Church Record | Facing and speaking to the priest opens the real service. Record offers the settled visit at (27,7), publishes Map16 Egress and F399, and returns to control. A subsequent ordinary Save preserves those registers. The kitchen Save preserves Map3 and clear F399. |
| Definite publication failure | A disposable lock path that is a directory refuses Record. The selected primary and backup retain their bytes and the live refuge and F399 keep their earlier values. |
| Corrupt and unavailable primary | Explicit confirmation promotes a healthy v6 backup for structural corruption. Content identity, unsupported version and unknown field refusals leave both copies unchanged. |
| Initial field attachment | A checksum-valid missing required sprite rejects before adoption. The live previous session retains control and accepts movement after closing the panel. Both files keep their original bytes. |
| Native cue delivery | An already delivered finite cue stays silent with its fresh bound generation and retains completion eligibility. The host reports `restore-music-phase` for a pending finite phase that has advanced. The logical service step does not specify a native PCM offset. This is an explicit Save and Continue limit. |
| Retained readers | Title Continue reads anchored Map3 and both tower returns, a controlled scene-bearing v1 Yeel file, v3 town/castle/tower files and a v2 battle. The historical Map3 roster extends deterministically. A new Save from the restored castle writes v6 to another slot and leaves the older primary byte-identical. Existing engine controls cover battle v4/v5 promoted and timed stat continuation. |
| Pre-cutover protection | The adapter at `c62a53ef567c98e2e69e87532dcee7e9eddd38b8` opens the same configured world. A v6 primary beside a readable v3 backup shows `save-version`. Both Continue and a separate ordinary Save attempt leave both files byte-identical. |

Use `uv run sf2 verify engine` and `uv run sf2 verify adapter`. The
[settled field engine cases](../../tests/Sf2.Remake.Engine.Tests/SettledFieldSnapshotTests.cs)
compare the next services and movement from the decoded v6 result, including RNG, hidden random
walk, relative patrol and idle actor behavior. The facing-cycle case refuses a live sprite request,
restores its acknowledged wait phase and compares the next facing, refresh and timer service.
Existing battle controls live in
[BattleSaveReturnTests](../../tests/Sf2.Remake.Engine.Tests/BattleSaveReturnTests.cs) and
[BattleSaveProgressTests](../../tests/Sf2.Remake.Engine.Tests/BattleSaveProgressTests.cs).

For the native control, build the Debug Godot adapter using the
[locked SDK environment](../development-and-verification.md#locked-net-workflow). The published
runner selects the existing `derived/issue2035/galam-town-v1` definitions, entry, scenes and opening
inputs. It stages only the alternate startup descriptor in the checkout's ignored output and
runs headless with the Dummy audio driver. The cold reader uses the published inputs and numbered
slot directly. Use fresh identifiers for each run.

```powershell
. ./local/private-inputs.ps1
$checkout = (Get-Location).Path
Push-Location (Join-Path $checkout 'remake')
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
    & $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug -p:RestoreLockedMode=true
    if ($LASTEXITCODE -ne 0) { throw 'Debug adapter build failed' }
}
finally { Pop-Location }
$tools = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2049/unified-field-v6'
$runner = Join-Path $tools 'Run.ps1'
$stamp = [guid]::NewGuid().ToString('N').Substring(0, 10)
$reports = (New-Item -ItemType Directory -Path "local/issue2049-$stamp").FullName
$space = "issue2049-$stamp"
foreach ($case in 'writer', 'reader', 'church') {
    $namespace = if ($case -eq 'church') { "$space-church" } else { "$space-kitchen" }
    & pwsh -NoProfile -File $runner -Checkout $checkout -ReportRoot $reports -Case $case -Namespace $namespace
    if ($LASTEXITCODE -ne 0) { throw "Field snapshot observation failed in $case" }
}
& (Join-Path $tools 'Prepare.ps1') -Checkout $checkout -NamespacePrefix $space -WriterNamespace "$space-kitchen"
```

`Prepare.ps1` copies older controls and creates bounded failure witnesses in new disposable slots.
Run each case in its own `pwsh -NoProfile -File` process and check its exit code.
Use `Run.ps1 -Case` with `legacy`, `legacy-resave`, `legacy-battle`, `bootstrap`, `church-fail`,
`recover`, `refuse`, `resource-live` or `delivered-music` against the matching namespace. Pass
`-ExpectedError` for a refusal. Use `-Project` with the Debug adapter project built from the
pre-cutover commit for `refuse` and `old-overwrite` on the rollback pair. The published handoff
records the completed checks and process results.

#### Mechanism coverage across visits

[#2053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2053) checks the shared
recovery contract through a small set of distinguishing visits. The
[mechanism matrix](pre-voyage-optional-locations.md#remaining-save-coverage) identifies the
representatives, reused evidence, and observations still open. The resident facing-cycle change
uses the same action binding, saved cursor and timer as other settled ambient work. V6 and the
older reader formats retain their existing versions.

<a id="implemented-savepoint-boundary"></a>

### Implemented legacy savepoint boundary

The older observations below use their original wire and recipe assertions. Reproduce those
writer observations at their recorded revision, or at the pre-cutover revision when no other
revision is specified. Use the current cutover controls for v6.

The legacy writer controls use `GameSession.QuerySave` and revision-checked `CaptureSave`
to produce the earlier progress shape without servicing the world or changing seeds.
`OpeningRestoreReader` admits immutable definitions through the same private Content readers
without invoking new-game party initialization. `OpeningRestore`
validates the DTO, delegates location semantics to `Sf2OpeningRestore`, rebuilds through the existing
map allocator/layout helpers and publishes a fresh session only after success. Ordinary settled
savepoints do not execute on-load; the explicit initial recipe reconstructs the ordinary fresh start.
Encounter-excluded retained allies survive field/battle/field transitions;
completed joins update the existing party membership projection before field control returns.

Map3's default and F609 commission setup share the existing `map3-joined` church anchor `(32,13)`,
Up, after F603. Selection uses the same `ExplorationMapDefinition.SelectedSetup` path as ordinary
map allocation. Reconstruction selects that setup before entry-flag/layout/population work. Default
Sarah/messenger initialization belongs only to the default setup; F609 instead retires Jaha after
F3. It keeps the F609 NPC at entity142. No on-load dialogue, healing, join or reward is replayed.
F506 uses the separate settled-visit recipe and requires its matching scene and setup body.
This legacy recipe excludes F543 and unclassified setup families. It retains the save
format, recipe revision and gameplay revision.

`Map3SaveSetupTests` exercises the delivered setup families, Jaha before and after joining,
selected ambient and unfinished story work, progress roundtrip and explicit refusals. Paired
[host observations](../verification/map3-commission-save.md) cover the accepted restart boundary.
Preserve those legacy reader contracts while introducing
[acceptance for each distinct recovery mechanism](#mechanism-implementation-and-acceptance).

#### Granseal F506 Save and Continue observation

[Issue #2034](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2034) uses the settled
field recipe above in the High-rigor recovery contract. Select the published
`derived/issue2034/granseal-f506-v1` world with its matching entry, scenes and opening inputs.
The publication extends `derived/issue2020/south-shrine-v1` at the same package identity and
gameplay revision 4. Its only world change is the imported Map3 Church refuge. The 102,142,485-byte
world fits the 128 MiB reader limit and shares all 145 unchanged files. The bounded rebuild reads
a package inventory and one world at a time, with a 1.5 GiB working-memory budget and one
approximately 98 MiB output. Same-volume links reuse the accepted dependencies.

**Confirmed in the remake.** Use the controlled post-Battle06 predecessor at Map66 (30,45), Up,
with six initialized active members and F399 clear. The normal Search at (30,44) grants one Mithril
and sets F213. Ordinary movement through Map66 (30,32) selects Map3 F506. The writer saves before
talking to Kiwi at (37,22), cancels an occupied overwrite, declines the choice, and saves again.
Separate title Continue processes restore the two visits and reach the offer. The refusal reader
then enters the name SHELL and publishes a third slot. A fresh reader preserves F6/F38, the name,
one joined party record and the in-visit follow link. Walking out west and reentering retires the
joined resident. Captured progress at attachment compares the full party and item words, receipt,
flags, gold, both RNG registers, music, actor slots/aliases, active entities and layout differences.

The priest across the counter at (32,13), Up, opens the actual Church service. Record uses the
common slot confirmation. Holding the disposable slot's writer lock gives `save-storage-unavailable`
and preserves both files and live F399/Egress. After walking away and returning, Record to a new
slot commits F399 and Map3 Egress. The separate title reader consumes that Egress in the ordinary
field Magic menu after exiting to Map66. This priest approach coincides with the refuge.
`Map3SaveSetupTests` separately exercises a non-refuge Continue tile and a different incoming
Map16 register. Its stale confirmation cannot reach publication. Older copied default and F609
Map3 records still restore their anchored landings, and a copied Yeel F650 record retains the
settled scene. Each accepts ordinary movement.

All nine native processes exited successfully. Six logged ObjectDB shutdown warnings. The Record
writer reported six objects and the other warning cases reported two. The two host observation
decisions that matter are to wait for camera settlement and to compare saved flags as numbers
after JSON decoding. Initial observer failures and native logs are retained with the handoff.

**Unknown:** original timing and SRAM parity, and native save/replay of other Granseal branches
or setups. The controlled start begins after Battle06 and the input coverage ends at field movement
and Egress. Other Church services and Battle07 retain their own observation owners.

##### Reproduce the F506 observation

Load the [worktree inputs](../../../docs/operations/local-private-inputs.md) and build the Debug
adapter as in [commission verification](../verification/map3-commission-save.md).
The stable private observer and compatible disposable source copies are under
`handoffs/issue2034/granseal-f506-v1` in the configured shared input root. Each invocation starts
and exits one headless Godot process with isolated app data. Only `writer` supplies a controlled
start. The restart cases use Witch Continue.

```powershell
$repo = (Resolve-Path .).Path
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2034/granseal-f506-v1/Run.ps1'
$sample = Join-Path $env:SF2_SHARED_INPUT_ROOT ('tmp/sf2-granseal-' + [guid]::NewGuid().ToString('N'))
foreach ($case in 'writer','before','refusal','join','record-writer','record-reader',
    'legacy-default','legacy-f609','legacy-yeel') {
    $destination = if ($case.StartsWith('legacy-')) { "$sample-$case" } else { $sample }
    $held = $null
    try {
        if ($case -eq 'record-writer') {
            $lock = Join-Path $sample 'saves/issue2034-granseal/sf2-opening/slot1.lock'
            $held = [IO.File]::Open($lock, 'Open', 'ReadWrite', 'None')
        }
        & pwsh -NoProfile -File $runner -Checkout $repo -SampleRoot $destination -Case $case
        if ($LASTEXITCODE -ne 0) { throw "Native F506 observation failed in $case" }
    }
    finally { if ($held) { $held.Dispose() } }
}
```

##### Rebuild the Church binding

Load the same input configuration. Use the accepted south-shrine publication, a read-only pinned
`c834c652` upstream checkout in `$upstream` and a **fresh** absolute `$target` on the same volume
under the registered shared `derived/` root. The full private importer uses the corresponding
`churchSavepointMaps` selection for new imports.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2020/south-shrine-v1'
@'
from pathlib import Path
from tempfile import TemporaryDirectory
import json
import os
import sys

from sf2tool.remake_asset_build import ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_exploration_battles import savepoints
from sf2tool.remake_exploration_programs import OriginalPrograms
from sf2tool.remake_private_package import include_sources, package_files, private_output, read_json

base, upstream, requested = map(Path, sys.argv[1:])
target = private_output(requested)
if target.exists() or target.is_relative_to(base.resolve()):
    raise ValueError("select a fresh destination outside the base package")
world_path = base / "world.json"
files = package_files(world_path, base / "main-scenes.json", base / "entry/entry.json", base / "opening-inputs")
package = read_json(world_path)
if package["provenance"]["commit"] != ACCEPTED_UPSTREAM_COMMIT:
    raise ValueError("expected a campaign package from the pinned source")
compiler = OriginalPrograms({"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, upstream)
selection = read_json(Path("remake/content/private/pre-voyage-opening.json"))
anchors = savepoints(compiler)
include_sources(package, upstream, {"disasm/sf2enums.asm", "disasm/data/maps/global/savepointmapcoords.asm"})
(town,) = (row for row in package["world"]["maps"] if row["id"] == "map-3")
if 3 not in selection["churchSavepointMaps"] or "churchSavepoint" in town:
    raise ValueError("expected Map3 to receive a new selected Church binding")
town["churchSavepoint"] = anchors[3]
target.parent.mkdir(parents=True, exist_ok=True)
with TemporaryDirectory(dir=target.parent) as scratch:
    staged = Path(scratch) / "package"
    staged.mkdir()
    for name, source in files.items():
        if name == Path("world.json"):
            continue
        path = staged / name
        path.parent.mkdir(parents=True, exist_ok=True)
        os.link(source, path)
    output = staged / "world.json"
    with output.open("w", encoding="utf-8", newline="\n") as stream:
        json.dump(package, stream, separators=(",", ":"))
    size = output.stat().st_size
    if size > 128 * 1024 * 1024:
        raise ValueError("world exceeds the private reader limit")
    staged.rename(target)
print({"worldBytes": size, "unchangedDependencies": len(files) - 1, "church": anchors[3]})
'@ | uv run --locked python -X utf8 - $base $upstream $target
```

#### Galam town Save and Continue observation

[Issue #2035](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2035) uses the
existing High-rigor recovery contract, with a finite Map16 implementation. Select the published
`derived/issue2035/galam-town-v1` world with its matching entry, battle scenes and opening inputs.
It extends `derived/issue2034/granseal-f506-v1` at the same package identity and gameplay revision.
The 102,142,557-byte world adds the selected Map16 Church binding and reuses 145 unchanged inputs.
The rebuild reads one package inventory and a 98 MiB world, with the same 1.5 GiB working-memory
budget as the Granseal publication and same-volume links for the other files. The imported
`(27,7)`, Up tuple equals the existing Battle05/06/07 Egress tuple.

**Confirmed in the remake.** The controlled kitchen predecessor starts on the walking side of
the diagonal stairs after the prison escape with F73 set. Ordinary input enters Map16. A cold
Continue of the pre-rally slot reaches the rally and completes it once. Saving the resulting
visit and loading it in a separate process keeps the hidden army, the removed aliases and the
default priest. The reverse kitchen warp loads the F663 residents and their priest. The captured
progress comparison at attachment includes full party/item words, names, flags, both RNG
registers, Egress, layout differences, actor slots, aliases and follow links.

The controlled settled Battle05 return uses Map16 `(16,18)`, Down, with F505/F663/F664,
one active Slade, and F73/F405 clear. Ordinary walking after Continue crosses the completed guard
zone. Search at the F207 chest adds one Healing Herb. A repeat Search leaves that count unchanged,
and the next slot retains F207. Actual default and F663 priests open Church Record, which reaches
the normal slot confirmation after dialogue closes. Save preserves the incoming F399/Egress.
Confirmed Record publishes F399 and Map16 Egress. Cancel and a held writer lock preserve existing
slot bytes and the incoming church registers. The lock failure keeps both primary and backup and
returns walking control. The unit behavior tests cover stale confirmation and a Record whose
Continue tile differs from its refuge.

Independent title processes read both priests' records, the F505 record containing F207, and
disposable copies of the older Map3 default/F609 and Yeel F650 saves. A post-Battle06 F663 revisit
provides the open route to field Egress. Its cold Record reader walks through Map18 to Map66 and
uses the normal field Magic command to return to Map16 `(27,7)`, Up. The earlier visit's path to
the drawbridge encounters the Battle05 guard. The existing early battle outcome tuples govern
that route.

**Unknown:** Original SRAM and timing parity, plus recovery for unadmitted setup families and
Maps17/18. Busy rally, guard and battle sequences stay outside this recipe. The controlled
predecessors establish restart and next-input behavior after their declared flags and progress.

##### Reproduce the Map16 observation

Load the [worktree inputs](../../../docs/operations/local-private-inputs.md), select the published
package and build the Debug adapter as in [commission verification](../verification/map3-commission-save.md).
The observer, controlled inputs and package publisher are under `handoffs/issue2035/galam-town-v1`
in the configured shared input root. The publisher takes the accepted F506 package, the pinned
`c834c652` upstream checkout and a fresh registered derived destination. It uses the existing
Church selection and source-table importer. From the owning checkout:

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$repo = (Resolve-Path .).Path
$handoff = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2035/galam-town-v1'
$saveRoot = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/disposable-saves'
$run = [guid]::NewGuid().ToString('N').Substring(0, 12)
$cases = [ordered]@{
    pre = @('before-writer', 'before-reader', 'rally-reader', 'default-record-reader', 'rally-record-reader')
    return = @('return-writer', 'return-reader', 'return-record-failure', 'return-record-reader')
    egress = @('egress-writer', 'egress-reader')
    legacy1 = @('legacy-default')
    legacy2 = @('legacy-f609')
    legacy3 = @('legacy-yeel')
}
foreach ($group in $cases.Keys) {
    $sample = Join-Path $repo "local/issue2035/$run-$group"
    foreach ($case in $cases[$group]) {
        $held = $null
        try {
            if ($case -eq 'return-record-failure') {
                $lock = Join-Path $saveRoot "issue2035-$run-$group/sf2-opening/slot1.lock"
                $held = [IO.File]::Open($lock, 'Open', 'ReadWrite', 'None')
            }
            & pwsh -NoProfile -File (Join-Path $handoff 'Run.ps1') -Checkout $repo -SampleRoot $sample -Case $case
            if ($LASTEXITCODE -ne 0) { throw "Galam native observation failed in $case" }
        }
        finally { if ($held) { $held.Dispose() } }
    }
}
```

To rebuild the additional binding from the same inputs, set `$upstream` to the read-only pinned
checkout and `$target` to a fresh destination beside the registered package:

```powershell
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2034/granseal-f506-v1'
uv run --locked python -X utf8 (Join-Path $handoff 'PublishWorld.py') $base $upstream $target
```

#### Hawel house Save and Continue observation

[#2039](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2039) reuses
`derived/issue2035/galam-town-v1`, including its matching entry, scenes and opening inputs.
Map43 already carries both setup bodies, the diary, the two chests and the southern warp.
The observation adds only short inputs, disposable slots and bounded state/input reports.
The world and prepared assets are reused directly.

The first controlled start uses Map66 `(6,6)` Up, after Battle03, with F72/F399 set,
F4/F404/F612/F650 clear and four active members. Play the actual house arrival through the
north warp. Dialogue refuses Save. Normal acknowledgements complete the sequence, joining
Kazin once and selecting Map43 Egress. Ordinary Search from `(11,4)` Up opens the
Antidote chest at `(11,3)`, adding one item and F135. The default visit's slot
retains its opened layout, parked Hawel, retired soldiers and live Kazin follow link.

A separate controlled post-Battle06 approach uses the same Map66 tile, with F506/F612/F650 set,
F72/F404/F399 clear and six active members. The startup seam supplies Map3 Egress.
The normal warp loads the F612 visit. It retains Kazin in the roster and uses the
fresh population without his same-visit follow link. Save keeps the incoming Egress and
F399. These controlled starts bound the observation to the exit, house and restart behavior.

**Confirmed in the remake.** Independent title Continue processes compare the full saved progress at attachment, including
members, items, receipts, gold, both RNG streams, flags, Egress, tile/facing/layer, setup,
actor slots, aliases, links and layout. Both can read the diary from `(3,3)` Up. The
default reader repeats Search without receiving another Antidote and takes the south exit
to Battle04. The revisit reader takes the same exit to Map66 `(6,6)`. Disposable copies
of older Yeel and Galam slots also restore. A checksum-valid default scene relabeled F612
fails detached restore and leaves the original slot bytes intact.

**Unknown:** Ordinary acquisition of the F136 chest on the sampled routes and original
SRAM or hardware timing parity. Active story, recruitment, transactions and transitions
remain outside the settled Save boundary.

##### Reproduce the Map43 observation

Load the [worktree inputs](../../../docs/operations/local-private-inputs.md) and build the
owned Debug adapter as in [commission verification](../verification/map3-commission-save.md).
The observer and the two controlled starts are in `handoffs/issue2039/hawel-save-v1`
under the registered shared input root. Each case launches a fresh native process. The runner
selects the accepted Galam input package and an external disposable namespace, and writes only
bounded case reports to the selected worktree's ignored output.

```powershell
. ./local/private-inputs.ps1
$repo = (Resolve-Path .).Path
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2039/hawel-save-v1/Run.ps1'
$run = [guid]::NewGuid().ToString('N').Substring(0, 12)
$cases = [ordered]@{
    default = @('default-writer', 'default-reader', 'corrupt')
    revisit = @('revisit-writer', 'revisit-reader')
    yeel = @('legacy-yeel')
    galam = @('legacy-galam')
}
foreach ($group in $cases.Keys) {
    $sample = Join-Path $repo "local/issue2039/$run-$group"
    foreach ($case in $cases[$group]) {
        & pwsh -NoProfile -File $runner -Checkout $repo -SampleRoot $sample -Case $case
        if ($LASTEXITCODE -ne 0) { throw "Hawel native observation failed in $case" }
    }
}
```

#### Granseal Castle Save and Continue observation

[#2041](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2041) uses
`derived/issue2035/galam-town-v1` and its matching entry, scenes and opening inputs directly.
Each writer begins at a controlled field
or arrival boundary with stage-appropriate flags. Ordinary acknowledgements, choices, keys and
menu confirmation produce the slot. Each reader independently starts at the title and selects
Continue. The observer compares the entire progress record at attachment, including resources,
RNG, names, F399/Egress, tile/facing/layer and the loaded setup, layout and physical slots.

**Confirmed in the remake.**

| Controlled writer and loaded setup | Input after independent Continue |
| --- | --- |
| Map19 default storeroom at `(9,11)` Up. Search opens the adjacent Angel Wing chest and commits F130. | Repeating Search yields no second Angel Wing. Down is blocked by the storeroom wall. |
| Map20 F501 east landing approach at `(38,41)`. Up then Right takes the diagonal CURRENT stairs to `(39,53)`. The completed bedroom/approach flags stay in the record. | The upper wall blocks Right. Left then Right takes the reverse stairs to `(39,39)`. The real east exit reaches Map19 `(35,7)`. |
| Map19 F609 arrival at `(29,8)`. Save refuses at the commission choice. Accepting commits F89/F610 and leaves retired procession slots. | Ordinary input retains the same visit, with the scene allocations retired and their aliases removed. |
| Map19 F506 at `(29,16)`. Step into the F620 departure zone and reach the independent F621 request. Save the completed F620/F621/F83 state. | The king and minister follow in their original resident slots for entities 130 and 129. Repeating the request keeps that chain and the force membership. A separate engine case exercises fresh entry with F83 and the declared follower entities 30 and 159. |
| Map19 F507 at `(16,6)` Up. Step into the F622 warning. Save is refused during dialogue. Walk to `(25,13)` through the separate F623 warning and save. | Astral, the king and the minister follow in physical slots separate from the force. Reentering both warning zones leaves the text cursor and roster intact. |
| Map21 F609 guard room at `(4,16)` Left. | Left takes its real exit to Map20 `(3,36)`. |

An older Hawel slot continued from a disposable copy. A checksum-valid mismatched castle setup
was rejected without mounting a session or modifying either disposable slot. See
[`CastleSaveTests`](../../tests/Sf2.Remake.Engine.Tests/CastleSaveTests.cs) for engine cases covering
payload reconstruction, physical following, entry flags, busy input and mismatched or missing setup.
The headless reader observes the current collision layer and camera binding. Earlier royal readers
logged a Godot Mono finalizer error during shutdown after the gameplay assertions passed. The
bounded runner now frees its host and services the tree before exiting. Raw logs and the completed
observer failures are retained with the private handoff. [Existing teardown evidence](../verification/map3-commission-save.md#later-finalizer-faults-and-ordinary-quit)

**Unknown:** ordinary runtime traversal and optional Talk at every room approach, individual
F129/F131 receipts on these routes, and original SRAM or hardware parity. The prior main route
owns the whole Battle01 handoff, both F622/F623 warning programs and the royal route out of the
castle. [Opening route](session-and-programs.md#castle-palace-astral-and-tower) ·
[Royal route](pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control)

##### Reproduce the castle observations

Load the [worktree inputs](../../../docs/operations/local-private-inputs.md) and build the owned
Debug adapter. The registered `handoffs/issue2041/granseal-castle-save-v1` holds the bounded runner,
starts and older-slot witness. The runner uses the accepted Galam package, worktree-local reports
and a fresh external disposable namespace per group. It submits key input headlessly and retains
state and native process reports.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug
if ($LASTEXITCODE -ne 0) { throw 'Castle adapter build failed' }
$repo = (Resolve-Path .).Path
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2041/granseal-castle-save-v1/Run.ps1'
$run = [guid]::NewGuid().ToString('N').Substring(0, 12)
foreach ($group in 'opening','upper','commission','occupied','royal','tower','legacy') {
    $sample = Join-Path $repo "local/issue2041/$run-$group"
    $cases = if ($group -eq 'legacy') { 'legacy-hawel' } else { "$group-writer", "$group-reader" }
    if ($group -eq 'opening') { $cases += 'corrupt' }
    foreach ($case in $cases) {
        & pwsh -NoProfile -File $runner -Checkout $repo -SampleRoot $sample -Case $case
        if ($LASTEXITCODE -ne 0) { throw "Castle observation failed in $case" }
    }
}
```

#### Tower Save and Continue observation

**Confirmed in the remake.** Standard batch 5 of #2029 covers only settled tower traversal.
Map40 captures the loaded default or F506 Lemon setup. F507 selects the default on fresh entry.
Map57, Map60 and Map58 bind their declared population, handlers and layout with the void setup
identity. Map57 requires F506's two-block copy and F401 clear. Map60 admits the first five
connected area records. F407 can be armed on the descent and must be clear on the Map58
aftermath. The saved visit includes physical followers separately from the force. Setup entry
bits, closed dialogue progress, take-once receipts and Egress/F399 survive attachment. Active
story, queued transfer, battle entry or outcome and F999 frontiers are outside this recipe. Map56
owns the Ancient Tower Ascent refuge. Ordinary tower Save has no Church Record caller.

| Controlled writer via the field menu | Input in the fresh Continue process |
| --- | --- |
| Map40 F506 at (14,14), before and after Talk to Lemon. | Before-Talk resumes with Lemon present and accepts the real interaction. Completed Talk stays closed, with the prone pose and saved text cursor. The force is unchanged. |
| Map40 F506 at (15,7), facing the Mithril tile (15,6). | The first Search sets F132 and grants one Mithril. Repeating Search after Continue retains the receipt and item count. |
| Map57 (7,5) with the open staircase. | The copied marker and physical followers survive. Up reaches Map60 (53,1). |
| Map60 (61,9) in the early descent. | Down takes the same-map portal to (13,2) with the same physical allocation. |
| Map58 (13,36) with the royal train. | Down takes the normal exit to Map60 (8,52). Up returns to Map58. Astral, king and minister remain separate from force membership. Resources and casualties carry through the regular transfer. |
| Map40 F507 at (4,30) with the default cast. | Right hits the working-layout wall. Down exits to Map21 (9,2), and Up returns to the default exterior. |

The bounded observer uses ordinary Save and independent title Continue with equal full captured
gameplay data at attachment. A checksum-valid mismatched setup was rejected and both disposable
slot files stayed byte-identical. Scene-free v1 controls exercise the Battle01 and Battle07
completed-return readers. They use controlled progress and the existing recipe/anchor gates.
Saving again at a now-admitted Battle07 return selects the current visit. The compatible Hawel
slot is an unchanged prior capture. The observer reads the native camera, player layer, restored
layout and errors without image capture.

The pinned SF2DISASM commit is `c834c652b6862bc5679fd7f69a38a7093206efc6`. The source setup table selects Map40's F506 body and F507 default.
`ms_map40_InitFunction` and `cs_53FE0` own the exterior init and Lemon behavior. The
Map57 F506 copy and Map60 warp tables own the entered layout and stair connections.
`table_Followers` and the Battle07 return own the F74 physical train. These observations use the
existing admitted world. **Unknown** remains exhaustive optional Talk and every legal landing
on the connected route, original timing and SRAM parity.
[Source tower route](../../../docs/research/chapters/grans-opening/battle06-to-battle07-content-route.md) ·
[Original Battle07 return](../../../docs/research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md) ·
[Map60 frontier](pre-voyage-optional-locations.md#southern-shrine-and-excluded-connections)

##### Reproduce the tower observations

Build the Debug adapter under the [locked SDK environment](../development-and-verification.md#locked-net-workflow).
The runner selects the registered `derived/issue2035/galam-town-v1` world and its bundled
presentation, entry and battle inputs directly. It writes reports under the
checkout and slots to new `issue2043-*` namespaces under registered `derived/disposable-saves`.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug
if ($LASTEXITCODE -ne 0) { throw 'Tower adapter build failed' }
$repo = (Resolve-Path .).Path
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2043/tower-save-v1/Run.ps1'
$run = [guid]::NewGuid().ToString('N').Substring(0, 12)
$groups = [ordered]@{
    lemon = 'lemon-writer', 'lemon-before', 'lemon-after'
    receipt = 'receipt-writer', 'receipt-reader'
    stair = 'stair-writer', 'stair-reader', 'corrupt'
    portal = 'portal-writer', 'portal-reader'
    royal = 'royal-writer', 'royal-reader'
    default = 'default-writer', 'default-reader'
    'legacy-b01' = 'legacy-b01'
    'legacy-b07' = 'legacy-b07'
    'legacy-hawel' = 'legacy-hawel'
}
foreach ($group in $groups.GetEnumerator()) {
    $sample = Join-Path $repo "local/issue2043/$run-$($group.Key)"
    foreach ($case in $group.Value) {
        & pwsh -NoProfile -File $runner -Checkout $repo -SampleRoot $sample -Case $case
        if ($LASTEXITCODE -ne 0) { throw "Tower observation failed in $case" }
    }
}
```

#### Yeel Save and Continue observation

Use the published `derived/issue2022/yeel-f650-v1` under `SF2_SHARED_INPUT_ROOT`. The world
is 102,036,676 bytes and shares all 144 unchanged dependencies with `church-anchor-v1`.
Its [private selection](../../content/private/pre-voyage-opening.json) includes the Map5 Church
anchor and `ms-map5-flag650`. The variant changes the entities and Talk bindings while retaining
the default Search table. All 20 program definitions referenced transitively by the setup resolve.

Load [the worktree input configuration](../../../docs/operations/local-private-inputs.md) and use
the existing [host workflow](../verification/map3-commission-save.md) with a fresh test save root
and the Debug adapter. Select the candidate's world, entry, and battle metadata together.

```powershell
. ./local/private-inputs.ps1
$package = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2022/yeel-f650-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $package 'entry/entry.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
foreach ($name in @('SF2_PRIVATE_BATTLE01_DATA', 'SF2_PRIVATE_BATTLE01_SCENE',
    'SF2_PRIVATE_BATTLE01_TERRAIN', 'SF2_PRIVATE_STATIC_DATA', 'SF2_PRIVATE_ENEMY_DATA',
    'SF2_PRIVATE_ENEMY_GOLD')) {
    [Environment]::SetEnvironmentVariable($name, (Join-Path $package "opening-inputs/$name.json"))
}
```

A legal, controlled pre-session guide approach
uses Map5(7,7), Up, speed 32, the membership and story flags from
[`yeel-to-battle03-start.json`](../../reference/inputs/yeel-to-battle03-start.json) with F399 clear, and
the initialized full-roster opening values. The existing `StartPreVoyageLocalizationObservation`
entry admits this case. It does not establish natural reach.

1. Save from the system menu, then cancel an overwrite and compare the slot bytes. Talk to the
   guide and return to settled ordinary input with F72 set and F4 clear.
2. Walk to the aboveground priest approach at (11,6), face Up, open Church and select Record.
   Wait for the caller to return, then publish to a second slot.
3. Use a fresh Godot process for each slot and select ordinary title Continue. Compare the full
   saved progress and scene at attachment, including party, items, flags and the follower link.
   Verify the Yeel slot label and the absence of a field/adapter error.
4. With the first test slot's writer lock already taken, attempt a new Save. Check the storage
   error and that the prior slot still has the same bytes.

**Confirmed** in the preceding `church-anchor-v1` default setup. The ordinary slot retains F399 clear and the
prior Map3 Egress. Record commits F399 and Map5 Egress to the live session and the second slot.
Two independent title Continue processes preserve their respective flags, Egress and scene.
The previous `phoenix-wave-v1` observation covers the locked-slot write failure.
`PreVoyageSaveTests` exercises a complete synthetic replacement setup and refusal during
movement.

**Confirmed in the published F650 world.** The controlled pre-session approach begins at
Map66(19,24), facing Up, after Battle06 and before Kiwi or the royal followers. It selects
six active members including Kazin and Slade, F612/F650 and F506 set, F72/F404/F134/F399
clear. The observer uses initialized roster values and the startup seam’s Map3 Egress.
This samples a legal revisit stage. On the first departure from Hawel, F404 still requests
Battle04; that departure is covered by the [campaign route](pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment).

- Ordinary Up enters south Yeel. All 15 residents appear, with no village Kazin. From (8,7)
  facing Up, Talk to resident 140 displays the replacement sequence 807–809 and leaves the
  party unchanged. Kazin retains one active membership and one party record.
- Search at (24,26) grants one Healing Seed and sets F134. Ordinary Save at (8,7) keeps the
  prior Egress and F399 clear. Church Record through resident 141 saves at (11,5), sets
  F399 and selects the Map5 Egress anchor. The ordinary slot survives Record unchanged.
- Separate title Continue processes match the entire saved progress at attachment, including
  item custody, membership, flags, setup, actors, aliases, follower links and working layout.
  A further Continue leaves through the north exit and enters again from Map66(18,22).
  The replacement residents return and Search cannot award the collected seed again.
  A copied default-setup Church slot from the preceding world also restores its original
  scene and Kazin guide link on the F650-capable world.

Run the short headless observation with the existing Debug project and the stable private
launcher. Use a fresh sample root outside every checkout. Each call starts and exits its
own Godot process. Only the writer uses a controlled start file under the target checkout.

```powershell
$repo = (Resolve-Path .).Path
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$runner = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue2022/yeel-revisit-v1/Run.ps1'
$sample = Join-Path $env:SF2_SHARED_INPUT_ROOT (
    'tmp/sf2-yeel-' + [guid]::NewGuid().ToString('N'))
foreach ($case in 'writer','ordinary','record','return','legacy') {
    $root = if ($case -eq 'legacy') { "$sample-legacy" } else { $sample }
    & $runner -Checkout $repo -SampleRoot $root -Case $case
    if ($LASTEXITCODE) { throw "Yeel observation failed in $case" }
}
```

The launcher's `accepted-sample` directory retains the two F650 slots and native receipts.
A reader-only check can use `-SampleRoot` pointing at that directory and `-Case record`.
The legacy case copies the two registered default-setup slots into its own fresh namespace.

The SF2 recipe admits map-default ambient programs and an already-entered closed
random-walk/wait/back-jump cycle on Map3. The completed messenger installs such a loop at a new
origin, so object identity or equality to the initial map script cannot establish safety. Pending
setup, story-installed movement, sprite work and follower motion still reject. A completed
Battle01 return records its own Map57 anchor; later movement cannot overwrite that anchor.

Private product definitions bind supported ordinary weapons from trusted item/class data to the
existing numerical equipment accessor. Changed carried loadouts therefore derive attack from
attained base stats plus the equipped weapon. Field commands have their own explicit admission below;
product Battles01–07 explicitly admit the existing free weapon/Unarmed commands under the
[connected equipment boundary](../content/profiles-and-trust.md#connected-battle-equipment-admission).
Unsupported weapon effects still reject. Product battle Give has its
[separate admission](../content/profiles-and-trust.md#connected-battle-give-admission);
Drop has its own [product binding](../content/profiles-and-trust.md#connected-battle-discard-admission).

Same-context unknown music history rejects capture. The native audio owner can supply its actual
bounded cue/history through `ObserveFieldMusic`, a session command equivalent to other host
observations. It never guesses the default cue for an unknown current context. Existing known
clock/completion state is retained, and identical knowledge is a no-op. Continue starts the saved
cue/history on a fresh logical/native clock; it does not claim to resume PCM position.

The [legacy native and storage observations](../verification/host.md#opening-save-and-continue)
include the original slot destination labels and the limits of restart/failure testing. The
[current capture and attachment path](#confirmed-capture-restore-and-attachment-path) owns v6
and its readback, content and resource checks. In-flight services or field receipts cannot suspend.

### Save and Quit presentation

The [gameplay window agreement](../godot/presentation.md#gameplay-save-and-quit-windows-1450)
uses the same SaveSlotView for field Save, Church Save, overwrite/result/error, backup and Quit.
Gameplay retains the paused scene beneath a translucent scrim and a centered shared-style window;
entry retains its Witch-safe arrangement. Ordinary slots show their number, saved protagonist name
and level, and the saved instant converted to the user's local date/time.
The [shared history contract](battle-save-and-continue.md#manual-save-history-and-developer-files)
owns typed level selection, the explicit unknown-level label and DST/date-boundary conversion.
Destination details belong to save-offer context rather than history rows. A backup's presence alone never promises recovery:
only Continue labels it as an offered action, using the same existing compatibility/storage/music
exclusions as the recovery consumer. Save and New Game describe backup presence without that action.
Readable refusal/storage/uncertain-publication messages keep full diagnostic codes secondary.
Cancel-first confirmation, explicit success/readback, existing-file retention, no autosave and
explicit Quit remain unchanged. UI observations supplement the existing storage/restart evidence;
they do not replace its transaction or original-parity limits, or establish human aesthetic approval.

## Implemented field preparation

**Standard rigor for #1422:** the party/save boundary is fixed; these finite transactions share
existing item words, numerical rules, custody and `GameSession` publication. #1418 retains High
for overall lifecycle closure. The outcome is ordinary New Game → field preparation → Save → fresh
Continue → legal Battle01 entry consuming that same party. Search/receipts, services, rings/curses,
stat boosters, later effects and UI redesign remain outside this slice. Small engine behavior tests,
direct native input/state/errors and dependency-selected public gates own acceptance; original
runtime reach/timing and human aesthetics remain Unknown.

`FieldInventory` reads only joined, initialized carried members; encounter-excluded members and
defeated allies keep inventory access. No battle position/control/status eligibility is imported.
`FieldEquip`, `FieldGive`, `FieldDrop`, `FieldUseItem` and `FieldCast` are single commands behind
the ordinary session/revision envelope. Item commands also bind selected slot words. Queries,
preview and Cancel do not publish or advance simulation/RNG. Confirm prepares all changes before
one revision publication; stale, unsupported and illegal requests retain the complete old snapshot.
The host holds provisional choices only, pauses field dispatch while browsing, and keeps a rejected
confirmation readable without stopping the game. Close returns to the same member/field.

Equip shares class masks, duplicate-slot identity, source Unarmed sentinel4 and derived ATT with
battle rules. Give clears equipped on moved words, compacts removals and fills the first empty
slot. Full packs require an explicit return slot; field self-exchange retains its source slot
adjustment and equal-slot unequip branch. Other words, spells, attained bases, membership, gold,
receipts/Deals and resources survive. Drop rejects unsellable content; rare items require known
custody and add one saturating Deals entry. Item/class metadata must be admitted before mutation;
these field commands do not themselves enable battle transactions. Product battle Equip has the
separate admission linked above; connected battle Give has its
[own product binding](../content/profiles-and-trust.md#connected-battle-give-admission), as does
[battle Drop](../content/profiles-and-trust.md#connected-battle-discard-admission). Each battle caller
retains its own refusal, action cost and continuation; field Drop alone does not admit battle Drop.

**Confirmed static source policy:** SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/common/menus/main/mainactions.asm` (`FieldMenu`, `byte_21348`,
`@HandleNonAngelWingItems`, `@StartExchange`, `@SkipExchange`),
`disasm/code/common/menus/memberslistscreen.asm`
(`ValidateMembersListScreenConfirmationInput`, `ExecuteExplorationMagicMenu`, `EquipNewItem`),
and `disasm/code/common/stats/itemstats.asm` (`AddItem`, `RemoveItemBySlot`, `DropItemBySlot`).
The accepted [field-menu](../../../docs/research/presentation/field-menu-control.md) and
[field-item](../../../docs/research/world/field-item-effects.md) owners retain H1/ROM provenance.
The field member screens do not filter HP/status or check MP. Medical Herb0 and Healing Seed1
are outside the field-use allowlist and have no opening item event; HEAL takes the non-DETOX,
non-Egress no-effect branch. Their confirmations report “But nothing happened” and preserve
HP/MP/items/EXP and both gameplay seeds, including at zero MP. This is original NoEffect,
distinct from a typed unsupported implementation effect.

Antidote3 clears poison, and Fairy Powder5 clears poison and paralysis. Each consumes the selected
holder's item slot on valid target confirmation, including when no relevant status was present.
[Fairy Powder caller and Continue checks](pacalon-first-visit.md#fairy-powder-use).
DETOX1 clears poison and subtracts its admitted MP cost with the source zero clamp, even on an
ineffective cast or with insufficient MP. Full-HP and defeated joined targets are selectable;
curing never revives or restores HP, and unrelated status bits remain. No field operation awards
battle EXP or draws gameplay RNG. Wider status effects remain unsupported.
Map3's pinned `data/maps/entries/map03/{7-chest-items,8-other-items}.asm` records an empty chest
and Medical Herb; this inspection does not implement Search or establish natural acquisition.

Save/restore retain the same format/revisions and interpret existing values identically; field
commands add ways to reach already admitted loadouts, without changing restore or replaying starts.
The [native recipe and results](../verification/host.md#opening-field-preparation) separate remake
observations from original static policy. No original runtime timing claim follows from them.

## Secondary-layer field saves

The current [v6 visit](#implemented-physical-visit-boundary) captures the physical player's layer
and validates landing against the saved working layout. The following legacy extension and restart
observations describe its predecessor. **High rigor** applied because dropping layer 2 changes
collision and subsequent travel.

Legacy version 1 has an optional byte `layer`: absent or 0 means primary; 2 means secondary.
The legacy writer omitted it for primary-layer saves to retain their prior wire shape. The unchanged envelope/checksum, content and recipe
identities still apply. No format or roster migration is needed: names and raft already use this
optional-field mechanism. Older readers cannot load a new secondary record because their strict
unknown-member admission rejects it; forward compatibility with those binaries is not promised.
Unknown byte values fail with `save-player-layer`; negative, oversized, string and null values fail
the existing strict JSON admission. The ordinary error flow retains the original slot bytes and
shows the matching-content/rules message for an unsupported layer, with the diagnostic code in details.

Capture checks the player's actual layer through the existing `FieldLayout` accessor. A secondary
player cannot be silently relocated by a recipe whose anchor differs from the live map/position/facing.
Restore rebuilds the ordinary flag-selected population, layout copies and roofs without replaying
on-load, assigns the saved layer to the player and reconstructed followers, and validates the offset
landing against that rebuilt layout. Primary terrain above a valid secondary landing is not its
collision surface. Nonfollowers retain their ordinary map layer; transient following offsets still
restart at the leader, as in other field recipes. Existing occupancy and other save guards remain.
Initial `new-game` records still require layer 0. Battle v2 and historical roster upgrading are unchanged.

**Confirmed bounded native acceptance:** both independent ordinary Desktop victory histories walk
from (25,12) to layer2 (26,12), then separately save near the stairs and at (28,12). Fresh title
Continue → Save preserves every decoded field, including the complete party, inventories, flags,
gold, names, raft, both RNGs and scalar registers. Restored followers have layer2. Ordinary movement
then reaches (32,12), rejects Right into the secondary wall, and follows the stairs to (15,3), layer0.
The existing primary Creed/Desktop and Battle22 records in both languages, plus the older Battle19
record, retain complete decoded equality after Continue → Save. Five malformed/unsupported-layer
variants reject without changing the seeded slot bytes. Original SRAM compatibility, natural original
timing and leak-free shutdown remain **Unknown**; existing ObjectDB exit warnings persist.

`LayeredFieldSaveTests` varies both main-plane selectors and secondary offsets, saves actual stepped
states, checks rebuilt followers/full progress and rejects blocked/out-of-layout landings or unknown
layers. Together with the existing movement/save/restore tests, it runs via `uv run sf2 verify engine`;
`uv run sf2 verify adapter` compiles the ordinary host. The stable private `handoffs/issue1862` bundle
reuses `issue1861` world/catalog/encounter inputs read-only, with its own runtime, receipts and save
namespace. Its `Play.ps1 -Language en|zh-CN -Sample <fresh> -Mode near|middle` performs ordinary saves;
`-Mode cold -Seed <that-run>/saved-slot.json` checks cold restoration, collision and return. `-Mode old`
checks a named legacy record, and `-Mode reject` observes a named invalid record. `python compare.py`
compares retained complete records and input-slot preservation. No screenshot, debug state injection
or rewritten gameplay history is used. The small runner is about 36 MiB; receipts/slots are bounded
to a 100 MiB total new-output budget, with sequential native runs and no duplicated 132 MB world.

## Format, compatibility and local storage

The [v6 field record](#reader-and-writer-cutover) includes the slot/sequence and UTC timestamp,
content/rule identity and the detached scene values. The host resolves the selected package
through its configured content source. Existing pinned provenance applies to every restore.
Private save files remain local and untracked.

Wrap the payload with an integrity digest over its exact stored UTF-8 bytes and verify it before
semantic admission. This checksum detects accidental in-range data damage that JSON/range checks
would miss; it is not change detection, authentication or an asset/content hash. Keep one documented
encoding and existing SHA-256 support, without a canonicalization library or signatures. Bound the
whole file before parsing; duplicate payload/checksum keys reject. A checksum-valid but semantically
invalid record still rejects at ordinary domain/content validation.

Require an admitted format and matching gameplay-content, rule and pinned provenance identity.
The [historical roster policy](battle-save-and-continue.md#continuing-across-historical-opening-rosters)
owns the explicit recipe revision exceptions. Semantic changes use the existing revision fields.
Pure portrait/UI/audio encoding changes may retain gameplay compatibility when logical
cues/resources remain valid. Changed stats, program meaning or receipt IDs require a new
gameplay revision. Unsupported versions or identities report that matching content or rules are
required. Keep existing slot bytes available for a compatible reader.

The private opening selection declares its gameplay revision. Existing four-member worlds use
revision 1 and five-member worlds use revision 2. The current six-member world uses revision 3:
New Game generates Kazin's level-4 MAGE and Slade's level-5 THIF stats from the main RNG before
the player moves. Older saves lack those attained stats and store only the later RNG value.
Continue each older slot with its matching world. Start the six-member world in a separate slot
and use the same world when continuing it. Selecting
a mismatched revision shows the existing matching-content message before restoring the party.
The [campaign preparation](pre-voyage-campaign.md#preparing-the-ordinary-selection) identifies the selections.

Application owns DTO capture/validation/restore; Content owns bounded JSON admission/encoding and
binding to admitted definitions; the host owns a small filesystem adapter. Domain has no file/Godot
dependency. Add only the storage boundary needed to return typed results; avoid a plugin store,
save manifest database or generalized transaction framework. Parsing bounds: 1 MiB per slot,
bounded depth32, no duplicate/unknown fields, duplicate actor/receipt keys, invalid ranges, dangling
IDs or incompatible membership/loadouts. Limits are initial engineering bounds, not gameplay facts;
raise only with a measured legitimate record. Admission is bounded by the saved actors, flags, registers and layout words. It reuses already
admitted content.

Use an explicitly configured absolute save root outside removable worktrees, with a per-checkout
namespace by default; no source-root/cwd inference, C-drive fallback, cache copying or shared mutable
test slots. Production installation chooses its persistent root once. Acceptance uses a separate
temporary H-local namespace, preserving previous evidence and user slots. Content profile namespaces
separate authored and private saves. Player filenames follow the numbered convention. [Manual save history and developer files](battle-save-and-continue.md#manual-save-history-and-developer-files) owns history selection, version-bound consent and the separate explicit-file launch.

For each slot retain `slotN.json` and one last valid `slotN.bak`; only one writer holds the slot lock.
Capture immutable bytes and freeze further gameplay commands while the short save operation runs.
Write a uniquely named sibling temporary file, flush file data, close, then re-read/validate it.
For an existing valid primary use the platform same-volume atomic replace with backup; for the first
save atomically rename the completed temporary file. Never delete/truncate the primary first and
never fall back to a cross-volume copy. If the configured filesystem cannot provide this operation,
report unsupported storage and preserve the old slot. Success is reported only after publication and
readback. Do not claim immunity to device failure or all power-loss cases from filesystem API success.

On startup ignore incomplete temporary files. Validate the primary independently; if corrupt, show
the error and offer the valid compatible backup with its timestamp (explicit recovery, no silent
rollback). An incompatible primary is not automatically replaced by an older backup. Missing content
is distinguishable from corruption, empty slot, permission failure and incompatible version. A read
never mutates or repairs files. After explicit backup recovery, preserve that valid backup until a new
validated primary is installed; never rotate the corrupt primary over it. Failed writes preserve the
last known valid record; an uncertain post-replace result reports an uncertain save and revalidates
before retry, rather than claiming that nothing was written. Unreferenced temporary cleanup is scoped
to that adapter's own attempt, not a directory sweep or worktree-cleanup authority.

## First executable slices and acceptance

The original opening lifecycle plan and restart criteria below record the finite recipe scope used
for the initial slices. See [the current feature map](#current-feature-map) for shipped behavior
and the [v6 cutover](#implemented-field-snapshot-cutover) for current restart evidence.

| Outcome | First coherent implementation and likely direct paths | Dependency and stopping acceptance |
| --- | --- | --- |
| [#1420 New Game](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1420) | `remake/game/src/GameRoot.cs`, new `game/src/Entry/SessionEntryView.cs`; Content `Scenarios/PrivateExplorationReader.cs`, `PrivateBattleScenarioReader.cs`, `PrivateBattleDefinitionReader.cs` and a focused `OpeningNewGameReader.cs`; Application `Runtime/GameSession.cs` plus `Runtime/Exploration/OpeningNewGame.cs` only for missing construction. Reuse `BattleStartInput`, initialization rules and accepted static/growth readers. | Accept #1419 first. Separate content definition admission from checkpoint construction at the concrete coupling above; normal title/New Game enters actual Map3 with complete supported state. Two starts are independent; valid varied seeds/data work; missing mechanical content rejects. Existing controlled CLI entries remain explicitly diagnostic. A small authored New Game witness is useful but does not complete private opening acceptance. |
| [#1421 Save/Continue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1421) | New Application `Runtime/Persistence/OpeningSave.cs` and `OpeningRestore.cs`, focused Content `Persistence/OpeningSaveJson.cs`, host `Persistence/LocalSaveStore.cs`; `GameSession.cs`, `ExplorationState.cs`, `MapTransfer.cs`, `BattleOutcome.cs`, entry/field-menu consumers. Content recipe data uses existing exploration definitions/readers. | #1420's admitted state and lifecycle first. First deliver capture/reconstruction of the three recipes with actual engine tests; then disk/entry integration and real restart, in one bounded PR if reviewable or two sequential PRs under this outcome. Capture alone is not completion. No need to wait for field inventory/services. |
| [#1422 Field preparation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1422) | **Implemented:** shared `BattleInventory`/`BattleEquipment` word and numerical rules; Application `FieldCommands`, `FieldInventory`, `FieldRecovery` through `GameSession`; explicit product Content admission; Party Status → `FieldActionView`. [PR1429](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1429) owns review/integration status. | Equip/Unarmed, Give/full-pack exchange, source-correct Use/magic and confirmed Drop mutate the one carried party; actual fresh Continue/Battle01 consumes it. Query/preview/Cancel are free, and stale or illegal confirmation retains the old snapshot. Defeated allies keep inventory access. HEAL/ordinary recovery items are NoEffect without payment; Antidote/DETOX1 pay even on no effect, with DETOX1 MP clamped at zero. Full-HP/dead poison targets do not revive, and field EXP/RNG stays unchanged. [Exact boundary](#implemented-field-preparation). |
| [#1423 Search/branches](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1423) | First inventory the bounded offered branches above; then one complete Map3 Search/receipt path. `src/sf2tool/remake_exploration_maps.py`/`remake_exploration_programs.py`, Content `Scenarios/ExplorationContentReader.cs`, Application exploration dispatch plus focused search reducer, existing inventory/custody rule and host field menu. | Shared #1422 item mutation semantics, not all its UI. Import source descriptions/placements, confirm once-only award, full inventory, repeat visit and reload/save survival. Other branches close one reached missing behavior at a time. Services remain #1424; source-lowering changes retain their affected research verification. |
| [#1424 Required services](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1424) | First Map3 ChurchMenu caller → raise/supported cure → close to same field; use a focused Application service transaction and Content cost/status admission, Domain existing party rules where valid, host service panel. Connect Save to #1421 only after the service caller has returned to an eligible field boundary. | #1420/accounting and required #1422 rule consumers; Save entry needs #1421. Verify actual church offer/return, decline, insufficient gold, no applicable target and successful payment/recovery. Then verify actual opening shop entry before buy/sell with capacity/payment/rare-Deals preservation. Promotion, mithril and Caravan storage deferred. |
| [#1425 Independent loop](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1425) | Existing engine/native observation entry and direct capability/UI/verification owners; no new comparator. | Accepted preceding required features or an explicit disposition that retains the opening promise. Main-gate independently checks the complete route below. Component passes and human aesthetic approval are separate. |

Each implementation adds small tests of actual engine state changes, not tests of JSON helpers,
planners or observation tools. Use direct malformed-file/storage controls for the adapter. Run the
affected `uv run sf2 verify engine` / `uv run sf2 verify adapter` and native state/input/error checks
under the existing locked environment only when implementing those changes. Source-lowering work
selects its own affected research checks. Preserve completed failures; do not restore blanket suites.

### Restart and failure acceptance

1. Use the normal New Game UI with the private opening content, no comparison-start argument or later
   state injection. Move/talk/join; save at eligible Map3. Record a compact semantic summary of party,
   inventory, flags, gold, receipt/Deals state, both seeds and selected resume anchor.
2. Terminate the process; start a genuinely new process using the same configured save root and
   choose Continue. Confirm a new session ID, released input, canonical landing and preserved
   persistent summary. Move/interact, traverse ordinary castle/tower programs, enter and finish
   Battle01; do not replace the journey with teleport or a terminal-state assignment.
3. Save the settled Map57 returned party, exit and load again. Verify attained growth/EXP/items,
   gold, membership/following, F401/F501 and receipt state, then actual movement/interaction or a
   legal return-map transition. Confirm no repeated JOIN, reward, healing or outcome script.
4. Separately exercise actual ordinary defeat and Map3 return/save/load/retry, including depleted
   resources and dead nonleader allies where supported. If a required retreat path is not admitted
   for Battle01, report that boundary; selected Battle02 retreat is not proof of it.
5. Try field Save during dialogue/choice, movement, an ambient-versus-story NPC move, sprite/fade wait,
   map transfer, service transaction, battle and after-program. Reject with no mutation/write and
   recover normal control. Ambient reset is tested independently of forbidden pending story work.
   The separate [battle acceptance](../verification/battle-save.md) admits settled player Movement
   and rejects nonzero preview, pending movement/scene, selection and story work.
6. Directly exercise empty, truncated, malformed, duplicate-key, out-of-range, unknown version,
   mismatched rule/content and unavailable-resource records; invalid input publishes no partial
   session. Locked/read-only destination, interrupted temporary write and replace failure protect
   a known valid slot. Corrupt primary plus valid backup, invalid backup and first-save interruption
   have distinct observable results. Never fault-inject into a user's retained slot.
7. Reload twice with identical semantic commands/polls to compare deterministic continuation; vary
   legal party/flag/content inputs rather than testing only the fixed walkthrough. Include one
   affected existing Battle02 continuity assertion when shared carried-state code changes.

Use existing bounded observations and semantic summaries, not screenshots, per-frame captures or
full content dumps. A save/recovery control needs at most primary+backup+one temporary record (under
3 MiB per slot); clean up only explicitly owned disposable controls after retention obligations.
Replan if a valid save exceeds 1 MiB, reconstruction needs an arbitrary story interpreter, or required
opening branches exceed the finite recipes: fix scope/ownership before building a generic framework.

## Verification and remaining limits

The [restart verification owner](../verification/host.md#opening-save-and-continue) combines actual
engine behavior tests, direct strict-format/filesystem controls and native state/input observation.
Use the affected engine/adapter gates and clean-head dependency plan; no research/full/H3 suite is
implied. Exact Git objects, command results, preserved failures and CI belong to the Issue/PR handoff.

**Unknown:** full optional-route runtime reach, original R1 POISON/bootstrap chronology, original
durable SRAM behavior and exact presentation timing remain with their original owners. Current v6 evidence covers
settled visits and the next admitted input. Legacy recipes retain their bounded restart
observations.
The [composed acceptance](../evidence/opening-loop-acceptance.md) owns the bounded #1418 completion
claim; original optional-route reach and broader campaign acceptance remain outside it.


## Implemented opening Search

**Standard rigor for #1423:** the bounded outcome is safely exploring Maps3/19/20/21 and the
Battle01 return on Map57, using the carried party and the existing savepoints. Source metadata,
Application publication, Content admission and host input have separate owners. Church is implemented at the service boundary below; Map57 F506, later campaign, new save locations
and original presentation timing are excluded.
Acceptance covers faced dispatch, descriptions/empty results, every admitted placement, full packs,
alternate carriers, once/repeat, rejection, map re-entry, save/restart/Continue and Battle01 entry.
The [branch inventory](../content/opening-branch-inventory.md) and
[direct observation route](../verification/field-programs.md#opening-search-and-receipts) own details.

`Search` uses the current view target and faced working-layout cell. Chest, vase, barrel,
bookshelf and generic searchable markers precede ordered area descriptions. Explicit Search may
use MenuOnly descriptions and the ordinary empty fallback; the no-entity action entry may return
Unhandled with no publication. The modern visible Confirm/Search shortcut deliberately selects
explicit menu Search. It does not claim the original no-entity C-button fallback or menu raster.

The shared field inventory selects the leader, then joined members in source member order,
including reserve/defeated members. It keeps complete item words and uses the ordinary AddItem
rule to clear equipped on the received word. Full packs retain inventory, flag and chest block;
making room permits retry. One revision publishes the grant, source item flag and opened block.
An empty registered chest still opens. Gold uses carried gold and the source saturation limit,
although the five admitted opening placements contain no gold. Battle custody is unchanged.

The existing save captures story flags and complete carried loadouts. No receipt array, new
snapshot, savepoint, format or compatibility revision is needed. Rebuild and scene-layout reload
project opened chests from their flags. New Game/Continue require Search metadata on all five maps
and their admitted setups, known reward items and required common/description texts before
publication. Search/result input owns control until dismissal; Cancel closes feedback without
undoing an already committed acquisition. Stale or unsupported commands retain the old snapshot.

Angel Wing is admitted as opening immutable content and can be acquired, given, saved and carried
into Battle01. On these five non-overworld maps only, field Use has the source NoEffect behavior
and consumes nothing. Other maps require their separately admitted effect policy; no new Battle01
retreat profile or later Egress behavior is enabled.

### Tower exterior Search admission

**Standard rigor; bounded content correction (#1612):** the pre-voyage selection admits Search on
Map40 and its selected setup, alongside the existing opening maps and Map43. The outcome is ordinary
Search/empty feedback and the source Mithril pickup at legal field input. Other maps, callback
execution, Search text layout, new save locations and controlled-start OnLoad selection are outside
this correction. `FieldSearch.Available` and the host's existing command mapping remain unchanged.

**Confirmed static**, pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`:
[Map40's description handler](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map40/mapsetups/s4_descriptions.asm)
`ms_map40_AreaDescriptions` at `0x53FC0` directly returns; it has no callback or text rows.
Its [chest table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map40/7-chest-items.asm)
is empty, while the
[other-item table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map40/8-other-items.asm)
at `0xB0AD0` declares Mithril123 at (15,6), receipt flag132. The
[item definition](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm)
declares RARE, no equip effects and no use spell. The accepted
[Search spine](../../../docs/research/world/field-search-control.md) owns marker precedence,
explicit empty423/412 and unhandled no-entity action semantics; these are not new original runtime observations.

The existing compiler lowers those tables without a new opcode or callback fallback. The private
immutable item subset and field catalog include Mithril before possession; ordinary acquisition,
Give and rare Drop/Deals use their existing rules. Use remains Unsupported; smithing and Deals
trading are not admitted. This metadata does not grant Angel Wing a new Map40 NoEffect policy.
Story callers/waits, input programs, warps and battle handoffs still deny Search. The visible
Confirm shortcut still chooses Talk when there is an interaction target, then explicit `Search()`;
it is distinct from the engine's `Search(false)` no-entity action entry.

Acceptance uses `FieldSearchTests` for empty/menu-only/unhandled, receipt/full-pack and modal/handoff
boundaries, and optional `TowerFieldSearchTests` for actual private Content → `GameSession` admission,
Map40 area0 at (14,14) facing Up, empty results, (15,7) facing Up pickup/repeat, Give, unsupported Use
and rare Drop/Deals. Select `SF2_PRIVATE_TOWER_SEARCH_WORLD` and the ordinary authenticated battle
inputs for the private check; public engine verification leaves those selectors absent. The retained
focused producer check also confirms that a real pinned callback table remains withheld.
An authored headless Godot observation separately confirms actual menu Search and Confirm-shortcut
empty feedback, modal input ownership and return to field control. It tests the shared adapter
mapping, not private Map40 resources or original reach; no screenshots or full-route run are used.

**Confirmed in the remake:** the previously prepared campaign world lacks Map40 Search even though
its input program is null. The selection correction affects newly prepared content; an existing
world must be refreshed through the ordinary producer before it gains this definition. The bounded
managed observation composes only generated Map40 Search metadata into existing read-only world
structure, with required immutable dependencies; it is not a newly generated full campaign package.
**Unknown:** the user's exact map/area/story snapshot, original natural reach and original input,
presentation and persistence cadence. No protected running instance or user save was inspected.

## Implemented opening Church

**Standard transactional scope; #1424, within this owner's High recovery agreement.** The outcome
is the real Map3 default/F501 Church caller, ordered Raise/supported Cure, usable return and the
existing Save/Continue boundary. The [caller inventory](../content/opening-branch-inventory.md#opening-service-callers)
finds no admitted opening shop. Buy/sell, Deals trading, Repair, Promotion, curse removal, Caravan,
later setups/campaign and original service presentation/timing are excluded.

The private reader binds the Church caller to its greeting followed by `OpenChurch(ChurchDefinition)`
with immutable source operands.
`ChurchWait` owns the service menu and ordered immutable offers; its cursor retains the actual caller.
`GameSession` admits each answer against session/revision and a matching live token/offer. Querying
or selecting an action cannot change gameplay resources. Decline advances the source-ordered scan;
closing the menu resumes its program and event cleanup. No battle initialization or replacement
party occurs. The view owns focus/gestures only; failures that retain the snapshot remain inline,
while a failed real continuation uses the host's ordinary stopped-scene error path.

Raise skips living members and visits joined members in source member order. Cost is current level
×10 plus200 when the admitted member definition is promoted. Exact funds are affordable. An accepted
transaction deducts gold and sets HP to `min(current maximum HP,200)` together, retaining MP,
status, progress/accounting, item words/custody and both RNG channels. Decline or insufficient gold
keeps all resources; Raise continues to the next dead member. Cure scans all poison members before
all stunned members, including dead members. Poison costs10 and clears only mask `0x0002` (bit index1); stun costs20
and clears only mask `0x0001` (bit index0). Decline advances; insufficient funds skips the rest of that status family,
as in the source. The next family still runs. Curse bits are preserved with an explicit unavailable
notice. Supported status recovery never revives or changes EXP/RNG. Missing accounting, illegal,
stale and duplicate confirmations reject before payment and preserve usable controls.

**Confirmed source/seam boundary:** [service interactions](../../../docs/design/contracts/party/service-interactions.md)
and its named H2/H3 owners establish these ordered cost/payment/recovery rules at pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `churchactions_1.asm`/`churchactions_2.asm` and
`sf2enums.asm`. This implementation does not establish original natural caller reach or presentation.
The admitted opening field allocator already projects normal ally sprites independently of HP;
raising retains those live entities, aliases and sprites. It does not emulate original exhausted
sprite selection or claim the full original `UpdateAllyMapsprite` lifecycle. Authored Caravan's
separate defeated appearance policy is outside this admission.

Save from Church requests close first. The host waits for actual program/event/portrait return to
settled field input, then uses the existing Save menu, music-history observation, eligibility query,
slot confirmation and format. A service wait is never captured. The Church
[Record adventure entry](../godot/presentation.md#church-and-shared-save-agreement-1519) explains the
shared save history and existing save-point limits. Closing its save flow after cancel, success or
failure returns to usable exploration; another priest interaction opens services again. Menu Save
keeps its own existing Back-to-Menu hierarchy. Both use identical slot/destination/overwrite rules;
neither grants arbitrary saving. A settled but ineligible location
or transient save condition explains the existing reason and preserves the live session.
Service payments and resources already belong to the carried party. The
[current field snapshot](#unified-settled-field-snapshots) owns capture after service return. Lifecycle replacement discards old view/save intent; focus/hide/pause
suspends service gestures. New Game/Continue require the admitted Church program and the documented
[world supplement](../content/profiles-and-trust.md#opening-new-game-admission) explicitly clears the
old unavailable marker.

Acceptance combines [actual engine behavior](../../tests/Sf2.Remake.Engine.Tests/FieldServiceTests.cs),
the affected existing inventory/recovery/restore consumers, direct pinned source checks and the
[native Church observation](../verification/host.md#opening-church-services). Changed valid party
levels, caps, promotion, dead/status combinations and content costs are covered; a fixed route does
not define legality. Positive poison/stun naturally arising in the opening and original service
input/music/portrait cadence remain **Unknown**. The [composed opening acceptance](../evidence/opening-loop-acceptance.md) joins these consumers;
main-gate owns independent integration.

### Church and shop entry greetings

**Confirmed** in pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`.
In `disasm/code/common/menus/church/churchactions_1.asm:ChurchMenu` and
`disasm/code/common/menus/shop/shopactions.asm:ShopMenu`, the entry opens the current
speaker's portrait when available, displays 110 or 158, closes text and portrait, then enters the
service loop. The menu return labels are downstream of this sequence.
`disasm/data/scripting/text/gamescript.txt` rows 006E and 009E supply two W2 waits and one final
D3 respectively. In `disasm/code/common/scripting/text/textfunctions_1.asm:symbol_delay3`, DBF
starts at 119. With neutral input the loop waits 120 VInts. Direction or A/B/C input breaks the
delay before another wait. Nonzero mouth control bypasses the waits. The saved typewriting state
is restored on return.

The private reader replaces each native service instruction in place with a call carrying the
original terms. Its body uses the existing event speaker, text, window and service instructions.
This preserves incoming instruction addresses, shop inventory selection and the caller's return.
Authored services keep their supplied program. Legacy controlled starts without field text settings
keep their direct diagnostic menu. The full field reader owns both W2 spans and
the D3 clock. Menu presenters require release and a fresh selection. Chinese text comes from the
private catalog with identical control order. English and Chinese share the session's wait tokens.

**Confirmed** by ordinary slot 2 Continue on Map3 F609, facing north at the priest (32,13) and
weapon counter (7,20). Each of the four English/Chinese × adjustable/instant runs read the actual
localized body with portrait, including two separately acknowledged Church spans. Holding the
Talk edge preserved the first span. Holding the final acknowledgement or D3 interrupt through
menu entry and releasing it left services open. A fresh selection opened the healthy-party Raise
result or Buy stock. Back from stock and Continue from the result reopened services directly.
Leaving and talking again replayed each greeting. Gold stayed 360 and the party's carried state
matched the starting slot.

For a background replay select the combined private world/scene, ordinary entry assets and current
derived display catalog. Copy the F609 church slot into slot 2 in a new external save namespace.
Set that namespace's preference language and text mode, `SF2_HOST_UI_CASE` to their hyphenated
pair (for example `zh-CN-adjustable`), and `SF2_HOST_UI_OUTPUT` to a fresh JSON path. Run the Debug
host from the checkout with `--headless --audio-driver Dummy --fixed-fps 60 --path remake/game`
and `--script res://probes/engine_service_greetings_observation.gd`. The result records the two input boundaries,
menu return, elapsed simulation ticks and gold. Other towns' greeting bindings follow the admitted
service calls. This observation covers F609. Other service speeches, original hardware cadence and
human presentation approval remain outside this observation.

## Host preferences boundary

The [basic preferences agreement](../godot/presentation.md#basic-preferences-agreement) owns
Issue1451's four-field draft, safe application, independent file and recovery behavior. The host
opens this modal only outside entry dialogue or at settled exploration input without another menu.
It pauses existing presentation and retains the same session/input identities. Settings operations
do not capture/write progress or submit a session command. Persisted story text settings retain
their original meaning; adapter preferences remain outside game progress and its High-risk save
transaction agreement. Explicit launch settings override ordinary preferences read-only.
