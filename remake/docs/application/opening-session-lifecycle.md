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

Use **two manually selected local slots and savepoint restoration**, initially for the
supported Map3 phases and settled Battle01 return on Map57. Save is explicit from a field menu;
Continue chooses a slot. A save records progress now, but loading relocates to the declared safe
anchor and rebuilds ambient NPCs/doors/camera. Show that destination before saving. It neither
rolls the party back to arrival nor promises exact position, NPC phase or uninterrupted RNG results.
Castle/palace/tower interiors initially explain “Save after returning to a supported location.”
They remain playable. Adding a save location requires its own bounded reconstruction acceptance.

This persistence policy is a remake choice and does not reconstruct original SRAM. The
[save-system contract](../../../docs/design/contracts/system/save-system.md) confirms two logical
slots and bounded in-process original helpers, but leaves physical restart/power-loss and normal
player save UX Unknown. No arbitrary story/battle snapshot, autosave, cloud save, SRAM import,
copy/delete UI, migration framework, whole-party naming, promotion or Battle03 ending is promised here.
New Game selects a slot and writes only after final confirmation, including explicit occupied-slot
replacement consent. Ordinary field Save remains explicit. Quit warns about unsaved
progress and offers cancel; it does not secretly autosave.

Current acceptance endpoint: the composed opening loop through usable Battle01 return and restart. Main-gate
owns Issue dispatch, exact production ownership and integration. Reassess High
rigor at the persistence review and final restart acceptance; ordinary inventory/UI slices can
use Standard once their state boundary is fixed. No unresolved aesthetic choice gates this design.

## Current feature map

The feature map reflects the accepted entry, persistence, preparation, Search and Church consumers;
exact runtime objects and the composition boundary are in the [acceptance owner](../evidence/opening-loop-acceptance.md).
“Confirmed structure” below is a code/document claim, not new runtime or original-game evidence.
Implemented, standalone-only, missing integration, missing behavior and Unknown are distinct.

| Surface | Current state and owner | Closure disposition |
| --- | --- | --- |
| Entry/initialization | **Implemented:** ordinary title/New Game uses admitted static ally starts, growth and complete fresh progress; Continue reconstructs supported field savepoints. Explicit authored/controlled entries remain diagnostic. | #1420/#1421; [content/start boundary](../content/profiles-and-trust.md#opening-new-game-admission), [native entry](../verification/host.md#opening-new-game-observation) and [restart](../verification/host.md#opening-save-and-continue). |
| Persistence/Continue | **Implemented, bounded:** two explicit local slots, detached capture, strict disk/restore admission, title Continue and explicit compatible-backup recovery. Four savepoint recipes cover pre-messenger Map3, joined/ordinary-defeat Map3, completed Battle01 Map57 return and completed Battle07 Map58 aftermath. | #1421; [implementation boundary](#implemented-savepoint-boundary) and [restart verification](../verification/host.md#opening-save-and-continue). Other interiors, battle/story suspend, arbitrary later setups, autosave, migration and original SRAM parity remain outside scope. |
| Walk/Talk/party inspection | Implemented in the connected route: semantic movement/interaction and read-only roster/details. [PartyDetailsQuery](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs) projects carried state; absent loadout/accounting stays unknown. | Keep the same authority. A readable unknown label is acceptable for optional presentation metadata, not for gold/EXP needed to transact or save. |
| Equip/Give/Drop | **Implemented, bounded:** Party Status → Prepare changes the carried party through revision-checked field commands, shared equipment/word rules and existing custody. | #1422; [field boundary](#implemented-field-preparation). Source class/slot legality, full-pack exchange and confirmed Drop are explicit; private battle command capabilities stay separate. |
| Field Use/magic | **Implemented, bounded:** original NoEffect for Medical Herb/Healing Seed and HEAL; Antidote/DETOX1 cure poison with caller-specific payment. | [Field policy](#implemented-field-preparation). The earlier “field HP recovery” wording was a planning assumption, corrected from the pinned field caller; no modern healing deviation or battle EXP/RNG is introduced. |
| Search/descriptions/chests/ground items | **Implemented, bounded:** explicit faced-cell Search consumes validated opening descriptions and placements, shared carried inventory and source item flags. Full packs retain the reward; revisits and savepoint reconstruction retain opened chests. | #1423; [Search boundary](#implemented-opening-search) and [branch inventory](../content/opening-branch-inventory.md). Original input/presentation cadence remains Unknown. |
| Receipts/Deals | [BattleItemCustody](../../src/Sf2.Remake.Domain/Battles/State/BattleItemCustody.cs) owns authored battle drop once-flags and Deals; carried through field party. Null means unobserved, not empty. | **Distinct authorities:** field receipts use the source placement flag in durable story progress; battle custody retains its own drop flags/Deals. Field Search neither adds battle once-flags nor replaces null custody. Church uses the same carried resources; see its service boundary below. |
| Church/shop | **Implemented, bounded:** Map3 event13 opens typed Church Raise/Cure and a common Save entry after caller return. | #1424; [service boundary](#implemented-opening-church). No shop caller is admitted in the bounded opening phases; buy/sell remains deferred. |
| Battle01/outcomes | Connected before/load/start, combat, growth, victory Map57 return and ordinary defeat Map3 return are bounded implemented capabilities. | Preserve reward/join/flag order and once-only outcome commits. The [composed observation](../evidence/opening-loop-acceptance.md) carries prepared/acquired items through victory and fresh-process resumed play. |
| Later encounters | Selected Battle02/Yeel behavior exists independently. | Do not claim later campaign, private battle equipment or battle suspend from this design. Shared records must remain compatible or reject explicitly. |

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
| Map19 castle and Map20 palace/Astral revisit | First/repeat visits, refusal/re-prompt and follower variants have bounded acceptance in [session programs](session-and-programs.md#castle-palace-astral-and-tower). Other NPC branches/alternate map setups are not promoted to accepted by that group. Interior save is deferred, not disabled traversal. |
| Map21 guard, tower and Battle01 | Accepted conditional guard and battle admission remain authoritative. A save attempt during guard motion, entry or combat rejects; it never completes the program or sets unlock flags. |
| Map57 victory / Map3 defeat | Usable returns are implemented. Map57 F506 layout-copy remains a guarded frontier; reject that unsupported restore phase. Map3 defeat retains unlock, halves gold and restores the leader under the existing outcome policy. Loading must not run either outcome again. |
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
session construction/replacement. Continue uses the existing slot/backup diagnostics. Replacement
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

#### Title lifecycle investigation (#1447)

**Standard rigor, investigation only:** distinguish ordinary first entry, Witch, return and idle
using the accepted consumers and prepared package. Production/assets remain read-only. A static
title or finite score is not itself a defect; this investigation recommends retaining current
behavior pending independent review, not adding idle motion or changing the title score to a loop.

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
idle animation or audio service. #1447 remains open for independent investigation acceptance.

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
| Flags/membership/following | `Story.Flags` plus content party mapping; persist flags and any authoritative non-flag membership/order for authored packages. Rebuild `PartyLists`; validate rather than overwrite inconsistent `ActiveAllies`. Follower selection/order survives, physical allocation/offsets restart by map recipe. |
| Gold/Deals/receipts | Party gold and `ItemCustody` are authoritative. Persist current amounts, once-flags and Deals counts. Add field receipt keys to their owning state when #1423 implements acquisition; keep the key namespace separate. Pending transaction state is never saveable. |
| Map/position/geometry | Content owns base layout/population/setup and restoration recipe. Persist map, selected supported phase/recipe ID, safe landing anchor, and durable event flags/receipts. Do not persist whole layout, NPC script PCs or source RAM. Doors/roof copies and ambient positions reset as a map reload; persistent layout effects must be reproducible from saved flags/recipe. |
| RNG and world counters | Persist both live seeds and supported step counter/battle-request state (request must be false). Preserve EgressMap and any future gameplay counter actually read after restoration. Reject unknown required counters; do not confuse wall-clock time with simulation state. |
| Story execution | Cursor, callers, waits, event caller, warp, battle entry/outcome and skip must be absent at save. No tokens/continuations serialized. Persist text cursor or other scalar semantic registers if subsequent programs read them without assignment; clear only under a documented recipe invariant. |
| Logical display/audio | Close windows; reconstruct logical view, base/restored palette, player/follower sprite requests and required text settings from admitted map and saved semantic settings. Preserve current music cue and known previous-cue history where later music instructions consume it; start a fresh playback generation at step0. Reject unknown required history. No audio position or completion token is resumed. |
| Host/presentation | New session ID/revision/observation sequence, input release barrier, menu focus, camera smoothing, nodes/textures/PCM handles and caches. Never persist resources, services, private paths or completion receipts. Settings persistence remains separate from game progress. |

### Eligibility and restoration recipes

Application exposes a query returning allowed/typed reason and a capture operation that rechecks
the same session ID/revision. `CanWaitAtInput` alone is insufficient. Capture requires:

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

These are implemented product recipes. Their acceptance includes
ordinary legal variations within all three required boundaries: reordered optional conversations,
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
ambient programs begin on the next deliberate service. Repeated loads with the same content/rules
and semantic input/poll sequence must agree in gameplay results. Comparison with uninterrupted play
is limited to persistent resources/progress: NPC phases, elapsed music and subsequent draw timing
can differ because this is a savepoint reload. This distinction must appear in player help and tests.

### Implemented savepoint boundary

`GameSession.QuerySave` and revision-checked `CaptureSave` export detached progress; neither services
the world nor changes its seeds. `OpeningRestoreReader` admits immutable definitions through the
same private Content readers without invoking new-game party initialization. `OpeningRestore`
validates the DTO, delegates location semantics to `Sf2OpeningRestore`, rebuilds through the existing
map allocator/layout helpers and publishes a fresh session only after success. Ordinary settled
savepoints do not execute on-load; the explicit initial recipe reconstructs the ordinary fresh start.
Encounter-excluded retained allies survive field/battle/field transitions;
completed joins update the existing party membership projection before field control returns.

The SF2 recipe admits map-default ambient programs and an already-entered closed
random-walk/wait/back-jump cycle on Map3. The completed messenger installs such a loop at a new
origin, so object identity or equality to the initial map script cannot establish safety. Pending
setup, story-installed movement, sprite work and follower motion still reject. A completed
Battle01 return records its own Map57 anchor; later movement cannot overwrite that anchor.

Private product definitions bind supported adjacent weapons from trusted item/class data to the
existing numerical equipment accessor. Changed carried loadouts therefore derive attack from
attained base stats plus the equipped weapon. Field commands have their own explicit admission below;
private battle equipment commands remain disabled. Unsupported weapon effects still reject.

Same-context unknown music history rejects capture. The native audio owner can supply its actual
bounded cue/history through `ObserveFieldMusic`, a session command equivalent to other host
observations. It never guesses the default cue for an unknown current context. Existing known
clock/completion state is retained, and identical knowledge is a no-op. Continue starts the saved
cue/history on a fresh logical/native clock; it does not claim to resume PCM position.

The host pauses dispatch while the slot panel is open. Save displays the recipe destination,
defaults confirmation to Cancel, and records success only after storage readback. Title Continue
admits the configured content and offers explicit compatible-backup recovery. Candidate audio is
prepared before releasing the old session; Content validates raster/sprite links before mounting.
Quit always has a cancel path and warns when the live revision differs from the last save/load.
The [native and storage recipe](../verification/host.md#opening-save-and-continue) owns reproduction
and the limits of failure testing. Wider campaign, exact-position suspend, services and field
receipts remain outside this implementation.

### Save and Quit presentation

The [gameplay window agreement](../godot/presentation.md#gameplay-save-and-quit-windows-1450)
uses the same SaveSlotView for field Save, Church Save, overwrite/result/error, backup and Quit.
Gameplay retains the paused scene beneath a translucent scrim and a centered shared-style window;
entry retains its Witch-safe arrangement. Slots show the saved character, the known recipe's
readable destination and UTC timestamp, without promising exact-position or elapsed-time recovery.
Unknown recipe labels remain unavailable. A backup's presence alone never promises recovery:
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
these commands do not enable private battle Give/Drop/Equip.

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

Antidote3 clears poison and consumes the selected item even when no poison was present.
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

## Format, compatibility and local storage

Use a small explicit version1 JSON record: format version, slot, sequence number, save timestamp for
display only, content profile/package and gameplay revision, rule-composition identity/revision,
recipe revision, then the owned party/world values above. The host maps a selected package ID to an
already configured source; saved data cannot select a path, assembly, executable rule or trust root.
Content still enforces existing pinned private provenance. Save files containing private gameplay
state remain local and untracked, with no resource/export attachment to Issues or PRs.

Wrap the payload with an integrity digest over its exact stored UTF-8 bytes and verify it before
semantic admission. This checksum detects accidental in-range data damage that JSON/range checks
would miss; it is not change detection, authentication or an asset/content hash. Keep one documented
encoding and existing SHA-256 support, without a canonicalization library or signatures. Bound the
whole file before parsing; duplicate payload/checksum keys reject. A checksum-valid but semantically
invalid record still rejects at ordinary domain/content validation.

Compatibility is exact for format, gameplay-content revision, rule revision and recipe revision.
Use explicit revisions maintained with semantic changes, plus the existing private provenance identity;
do not add a hash of the entire asset export or Git commit as a universal compatibility gate. Same ID
alone is insufficient. Pure portrait/UI/audio-encoding changes can retain gameplay compatibility if
logical cues/resources remain valid. Changed stats/program meaning/receipt IDs require a new gameplay
revision. Unknown versions or identities reject with “This save needs the matching content/rules”; no
best-effort load, inferred migration or fallback to a new game. Future formats require a separately
reviewed migration only if a real retained save needs one.

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
raise only with a measured legitimate record. A compact opening save should be tens of KiB, not the
roughly 128 MiB private world export. CPU/memory are O(saved actors + flags + receipts), not O(assets).

Use an explicitly configured absolute save root outside removable worktrees, with a per-checkout
namespace by default; no source-root/cwd inference, C-drive fallback, cache copying or shared mutable
test slots. Production installation chooses its persistent root once. Acceptance uses a separate
temporary H-local namespace, preserving previous evidence and user slots. Content profile namespaces
separate authored and private saves. Slot names are fixed by the adapter, never supplied paths.

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

The linked outcome Issues are Backlog boundaries, not concurrent writers or automatic tasks. Main-gate
fills exact dispatch ownership after this design is accepted. Paths below are likely direct owners;
new names are recommendations, not committed scaffolding. Shared `GameRoot`, `GameSession`, readers
and capability/navigation docs are serialized. No slice requires every later service first.

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
5. Try Save during dialogue/choice, movement, an ambient-versus-story NPC move, sprite/fade wait,
   map transfer, service transaction, battle and after-program. Reject with no mutation/write and
   recover normal control. Ambient reset is tested independently of forbidden pending story work.
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
durable SRAM behavior and exact presentation timing remain with their original owners. **Inferred:**
savepoint normalization is simpler and safer for this opening than arbitrary interpreter snapshots;
the implemented finite recipes have bounded restart observations, not arbitrary snapshot acceptance.
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

## Implemented opening Church

**Standard transactional scope; #1424, within this owner's High recovery agreement.** The outcome
is the real Map3 default/F501 Church caller, ordered Raise/supported Cure, usable return and the
existing Save/Continue boundary. The [caller inventory](../content/opening-branch-inventory.md#opening-service-callers)
finds no admitted opening shop. Buy/sell, Deals trading, Repair, Promotion, curse removal, Caravan,
later setups/campaign and original service presentation/timing are excluded.

Content lowers `j_ChurchMenu` to `OpenChurch(ChurchDefinition)` with immutable source operands.
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
shared two slots and existing save-point limits. Closing its save flow after cancel, success or
failure returns to usable exploration; another priest interaction opens services again. Menu Save
keeps its own existing Back-to-Menu hierarchy. Both use identical slot/destination/overwrite rules;
neither grants arbitrary saving. A settled but ineligible location
or transient save condition explains the existing reason and preserves the live session. Savepoint
locations/recipes and format/rule revisions remain unchanged: service payments/resources already
belong to the saved party. Lifecycle replacement discards old view/save intent; focus/hide/pause
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

## Host preferences boundary

The [basic preferences agreement](../godot/presentation.md#basic-preferences-agreement) owns
Issue1451's four-field draft, safe application, independent file and recovery behavior. The host
opens this modal only outside entry dialogue or at settled exploration input without another menu.
It pauses existing presentation and retains the same session/input identities. Settings operations
do not capture/write progress or submit a session command. Persisted story text settings retain
their original meaning; adapter preferences remain outside game progress and its High-risk save
transaction agreement. Explicit launch settings override ordinary preferences read-only.
