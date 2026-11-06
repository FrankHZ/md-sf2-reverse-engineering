# Game Localization Design

## Scope and status

**Implemented bounded M1, M2a, M2b, M2c, M2d, M2e, M2f and M2g consumers; independent integration/product acceptance remains separate.**
The accepted M0 direction from [#1469](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1469)
now has the [#1471](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1471) English/zh-CN
settings, authored FieldTextWait and semantic HEAL consumers, localized Church/Search, entry/save, Party/Prepare and battle command/target/equipment/status and Give/Discard/retreat, story controls/choices/return notices and semantic battle feedback
screen groups under [epic #1468](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1468).
The private six-row M3 [Witch-entry suggestion](#m3-private-witch-entry-review-sample) has approved
human text tone; in-game typography/reading remains pending. The private
[opening-world catalog](#m3-private-opening-world-chinese-text) has 209 source-backed rows
connected to existing consumers, including ordinary-defeat `DialogueWait` row 363. Complete-game bilingual
coverage and human reading acceptance remain separate. The inventory below records
source findings at `9f5c5af81ad0da0e7d84f8033df306accbc4b7cb`; M1 behavior supersedes its named pilot paths.
These describe the remake, not new original-game evidence.

**Standard rigor:** extend the accepted resolver through bounded real consumers of this lasting
Application/Content/Godot capability. The existing [High save/restore boundary](opening-session-lifecycle.md#format-compatibility-and-local-storage)
continues to govern identity changes. Canonical source tokens, glyph units, name expansion, HEAL work,
program acknowledgements, gameplay/RNG and game-save format remain independent of display locale.

The runtime product target remains title/Witch, New Game/naming, supported opening exploration/services,
Battle01 outcomes and independent-process Continue, then other implemented consumers. One pilot
does not establish that coverage. The current full-text authorization below includes static translation
of unimplemented chapters; it does not implement those chapters. ROM patching, voice/TTS, other
languages, a translation platform and UI redesign remain non-goals. Preserve the 1080p minimum,
96px world cells, source grouping, input/reward authority and private-content boundary.

## Current full-text Chinese corpus

**Standard rigor:** [#1495](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1495)
authorizes static Chinese translation of all **4,267 message IDs 0–4266, 173,995 source characters**,
including unimplemented chapters. The independent
[#1506 name corpus](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1506) contains
**337 ally/class/item/spell/enemy entries**, keyed by table category/index in separate namespaces.
Static translation, semantic review, runtime admission and human typography/reading acceptance are
separate states. English fallback is not translated coverage. This agreement does not authorize new
chapters, ROM writeback, publication, image lettering, default-name display or naming/IME support.

**Current static coverage:** all 4,267 message occurrences are handled: **4,258 language translations
and nine source-exact non-language rows; no untranslated rows**. The intake, 337-name working draft
and all seven message batches have passed independent acceptance. The
[#1504 cross-batch static closure](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1504)
has passed independent main-gate acceptance, including old seed name/title calibration to adopted
names/terms and alignment of five devil-tier occurrences. Other existing prose remains exact. All 337 name
entries retain their independently adopted targets: 335 language entries, one translated empty-slot
label and one explicitly retained source-marked technical anomaly. No runtime or human-reading
completion follows from this static coverage.

Provenance remains `ShiningForceCentral/SF2DISASM` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the admitted USA ROM identity recorded below, and
`disasm/data/scripting/text/gamescript.txt`. Direct comparison confirms every admitted `world.texts`
`{id,text}` equals that pinned table. Entry display IDs 216–221 project those world IDs; they do not
add six message IDs. Their human-approved wording remains exact.

The [full-corpus layout review](../evidence/dialogue-corpus-layout.md) separately checks all4,267 pairs,
337 names and six aliases. It records4,010 parser-compatible rows,257 unsupported rows,107 semantic
line-preservation candidates and three ambiguities, with explicit native-layout and caller limits.
All209 historical world rows and six aliases still resolve in the isolated reader check;34 historical
world targets differ from later full-corpus calibration. Neither that check nor corpus-wide glyph
coverage expands runtime admission or completes pre-voyage Chinese coverage.

### One authority and exact ownership

A separately configured persistent private content root outside Git and removable worktrees owns:

- `source.jsonl`: the **sole message-text translation writeback source**, expanded by occurrence ID.
  Namespace/ID/source/category/duplicate links/batch/context are locked. Translation/review writes
  are serialized; cross-batch revisions require exact owned IDs and reasons.
- `names.zh-CN.jsonl`: the **sole name-table translation writeback source**. Category/index,
  source value/expression/file/line, classification and duplicate links preserve table identity.
  `names-review.zh-CN.md` and the 48 name-linked terminology lookups derive from it.
- `terminology.tsv`: 100 adopted working terms or explicit contextual holds. Additional prose-only
  terms have pinned message references; name-linked cells are derived lookups, not another name
  authority. Occurrence references can be lexical matches: the ordinary noun at AURA's indexed
  message is explicitly distinguished from spell identity.
- `batches.json` and `control-tokens.json`: frozen assignment/context and native-token inventory.
  Intake remaining-row values are historical allocation data, never current coverage counters.
- `accepted-seed.zh-CN.jsonl`: immutable historical review mirror of 215 accepted seed translations.
  Explicitly authorized name calibration in the sole corpus may differ from this mirror; all other
  seed wording remains exact. It must never restore old wording over the current authority.
- `review-<issue>.zh-CN.md`: current bilingual views generated from the sole corpus and frozen batch
  IDs. Never edit these views as translations. Per-row delivery labels retain their original state;
  independent acceptance records establish current adoption.
- `README.md`: current ownership, use, boundaries and navigation. Exact selections, corrections,
  failures, reproducible commands and acceptance records stay in the private owning handoffs.

No original/translated prose or private review is published. The existing **209 world rows and six
entry display rows remain frozen admitted runtime subsets**. They are historical admission evidence,
not competing full-text authorities. Calibrated corpus wording is not automatically mounted or copied
to them. A separately scoped admission must derive and validate runtime subsets and real consumers.
[#1505](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1505) owns later capacity/parameter
design; the reader's **256 rows / 256 KiB total / 8,192 UTF-16 units per row** remains unchanged.

The exact ID arrays in private `batches.json` are normative. The
[six reading groups](../../../docs/design/synthesis/themes/story-progression.md#read-story-content-in-segments)
are **Inferred editorial context**, not original chapter boundaries or natural-reach evidence.
Only explicit source-symbol/file references in accepted owners support assignment; map identity alone
does not. Common/cross-group/ambiguous/unassigned rows have one logical owner; other groups may read
them for context. **Unknown:** 1,227 rows lack established chapter context; 23 further rows have an
additional unclassified reference. Both remain common with their original context intact.

| Batch | Reading context | Assigned and handled rows | Source characters | Batch acceptance |
| --- | --- | ---: | ---: | --- |
| #1497 | Common/system/descriptions, cross-group and unassigned | 2,369 | 96,115 | Independently accepted |
| #1498 | Grans opening and departure | 444 | 19,760 | Independently accepted |
| #1499 | New Granseal and eastern Parmecia | 379 | 15,208 | Independently accepted |
| #1500 | Southern travel, Caravan and Creed | 310 | 11,891 | Independently accepted |
| #1501 | Northern travel, Pacalon, Tristan and Mitula | 239 | 10,493 | Independently accepted |
| #1502 | Moun, Nazca and return to Grans | 366 | 13,972 | Independently accepted |
| #1503 | Final confrontations and ending | 160 | 6,556 | Independently accepted |

**Confirmed static structure:** all IDs have exactly one owner; no deduplication/deletion. Six
control-only rows (99/262/264/362/3523/3553), literal `x` rows (406/407/411) and 103 equal-source groups
with 265 occurrences keep their expanded identities. Legitimate repeated sources and context-specific
target differences remain separate. Entry projection maps display newlines to `{N}`, preserves
leading `{CLEAR}` at 216/221 and terminal `{W2}` at 216–220. Source controls are never reconstructed
from guessed visual layout.

### Tokens, terminology and acceptance

Keep exact native syntax, control order and parameter identity/multiplicity **within each control
segment**, including unsupported tags. Do not expand dynamic names to fixed translations or guess
unknown parameters. Player-customized names always retain their exact saved values.

| Native form | Current overlay boundary |
| --- | --- |
| `{N}`, `{W1}`, `{W2}`, `{D1}`, `{CLEAR}` | Parsed/mapped; actual consumer/range still needs its own admission |
| `{LEADER}`, `{NAME}`, `{NAME;n}`, `{CLASS}` | Parsed/mapped; captured typed name/class operand required; missing operands fail explicitly |
| `{D2}`, `{D3}`, `{ITEM}`, `{SPELL}`, `{DICT}`, `{#}` | Unsupported by `ExplorationTextTokens`; reader rejects the row, static draft preserves it |

No new timing, DICT payload/caller or natural-use contract is claimed. Name/prose homographs require
occurrence-level judgment: person versus numerical maximum, default ally name versus modal verb,
different character identities, descriptive holy sword versus item-table name, and ordinary aura
versus spell. Honorific/register differences may be legitimate; consistency does not authorize
rewriting accepted dialogue tone. Deliberately retained product/title/brand identifiers and an
unresolved book-title referent are classified in private QA, rather than forced to zero Latin hits.

The independent name-table sources are `disasm/data/stats/allies/allynames.asm` (30),
`allies/classes/classnames.asm` (32), `items/itemnames.asm` (128), `spells/spellnames.asm` (44) and
`enemies/enemynames.asm` (103), with the last four relative to `disasm/data/stats/`. Item layout operands,
duplicate indexes and source anomalies remain original evidence. Working expanded class labels do
not prove original English expansions; abbreviation/width policy belongs to runtime admission.
Table presence does not establish natural use. No separate canonical location-display table was
identified in these stats tables; selected message references do not establish a full location list.

The resource agreement remains **8 MiB persistent text, 32 MiB cumulative temporary text and at least
1 GiB free storage**. Current corpus/reviews occupy approximately 4.2 MiB; cumulative temporary
records, including the one closure snapshot and retained independent review, remain below 27 MiB.
Direct closure checking is O(small corpus + tokens + names + selected pinned context + review bytes),
with memory O(the same). It compares pinned text and table files without parsing/copying the
125,828,417-byte world or creating a new environment, database, cache or external translation service.
Exact measurements and reproduction commands live in the current private handoff. Replan before
crossing the budgets; retain evidence and failures.

### Current batch review boundary

Each batch supplied full paired semantic review and independent acceptance. Cross-batch closure
checks continuous dialogue across common/chapter owners, naming/titles, joined fragments,
choices/refusals, identity revelations, conditional claims and ending chronology. Every consistency
change retains exact old/new/reason data for main-gate review; all other raw occurrence lines remain
exact. The protected six entry wordings, frozen seed mirror, name authority, source/context fields
and admitted runtime subsets remain unchanged.

Direct checks cover all source IDs and fields, disjoint assignments, all 337 table values/expressions,
duplicates, name-linked terminology, token/parameter topology, nine non-language rows and every
current generated review pair. Placeholder, Latin, non-Han, length and repetition heuristics identify
manual review work; they do not establish semantics or typography fit. Retained technical/brand or
Unknown wording is reported explicitly. Earlier batch checkers/freezers are snapshot-specific and
must not replay writeback over the current corpus. No SDK, Godot, emulator, research suite or
whole-game replay is required for this static work; completed runtime failures retain their original
results in their owning handoffs.

**#1504 static closure independently accepted by main-gate:** the static scope of #1495 is ready for
closure. Unsupported
controls/operands/timing, unknown source reach/speakers, unimplemented game consumers, image text,
custom-name/IME support, default-name display and human reading/typography remain separate boundaries.
Epic #1468, runtime admission and human acceptance remain open. Issue closure and integration belong
to main-gate; this status update starts no runtime admission.

### Existing Chinese terminology: bounded adoption decision

**Standard rigor:** the [#1542 terminology review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1542)
supports choosing familiar Chinese names without replacing the accepted US-based naming direction.
Its acceptance is actual candidate-page reading, stable-identity comparison with the current private
authorities, bounded recommendations and exact consumer impact. It authorizes no corpus writeback,
runtime name admission, save migration, external ROM/patch acquisition or complete-table import.
The full 34-row comparison and reproducible ID impact stay only in `reviews/issue1542/` beneath the
existing configured full-text root. These are read-only review attachments, never another authority.

**Confirmed page observations, accessed 2026-11-04:**

| Source actually read | Version and usable evidence | Limit |
| --- | --- | --- |
| [Ken_Tse old final-release post, first page](https://kxb4u.com/yzzl/viewthread.php?extra=&page=1&tid=19) | The 2011 forum post preserves dated 2005–2007 release notes, including the 2007-01-27 final notice and earlier Japanese/Chinese switching. | Body access succeeded in this review. It establishes an earlier localization effort, not a published term table or exact patch identity. Its older release notes include a redistribution condition; no compatible bulk-import permission was established. |
| [Ken_Tse 2026 recompilation post, first page](https://www.kxb4u.com/yzzl/viewthread.php?extra=page%3D1&tid=115905) | The 2026-02-22 post contains updates through 03-17, describes carrying earlier localization code forward, and describes gameplay/storage/UI alterations. | Distinct from the old final release and from the pinned USA baseline. It supplies isolated names, not a complete versioned glossary. No attachment was accessed. |
| [Chinese Wikipedia, fixed revision 94821718](https://zh.wikipedia.org/w/index.php?title=光明與黑暗續戰篇II_古代的封印&oldid=94821718) | Character entries explicitly place Chinese, Japanese and US names together; profession and spell sections provide additional vocabulary. | A secondary cross-version reference, with no demonstrated attribution of its Chinese names to either Ken_Tse release. The footer states CC BY-SA 4.0; that is not permission to import another project's translation under this repository's license. |
| [GOAL GOAL guide, first page](https://gmerago.com/forum.php?mod=viewthread&tid=1506) | The 2022-10-18 post's opening chapter pairs several Chinese item names with Japanese strings. | Its language header spans Japanese/English/localization without identifying the translation build. Some transcriptions and names differ from the US baseline. Treat as circulation evidence, not an independent vote or source of gameplay rules. |

**Recommendation:** retain accepted names that already agree with these references, and record
other systems as version-qualified lookup aliases in the private review. Do not transplant a whole
Japanese-derived naming system into the US corpus. For example, the encyclopedia explicitly pairs
Chester with Japanese ヒューイ / Chinese 修伊, and Slade with ジッポ / 吉布. Those are identity
cross-references, not spelling corrections to the current US-oriented names. **Inferred:** the
reported JP-to-US associations fit the named roles and pinned US table identities; this review did
not independently inspect a Japanese ROM, patch or original Japanese name table.

Only two target changes are proposed for a later serialized review; neither is applied here:

| Stable identity | Proposed change and reason | Exact writeback and display boundary |
| --- | --- | --- |
| `name.ally:4` / KAZIN | 卡津 → 卡辛: a small phonetic revision with an explicit identity match in the encyclopedia; useful for Chinese-reference lookup, without claiming a universal or official name. | Name authority row, derived `names-review` pair and `terminology.tsv` line 6. Zero current literal occurrences of either source name or current Chinese target in message prose; six numbered-name token rows are enumerated privately. |
| `name.ally:28` / LEMON | 莱蒙 → 雷蒙: this spelling appears in the encyclopedia, guide and 2026 post. Their possible dependence prevents counting them as independent corroboration. Keep the character and Red Baron title distinct. | Name authority row, derived `names-review` pair and `terminology.tsv` line 45. Zero current literal occurrences of either source name or current Chinese target in message prose; 37 numbered-name token rows are enumerated privately. |

**Confirmed static impact:** the paired early-message review preserves Hawel's literal name and
uses `{NAME;4}` / `{NAME;28}` for the two proposed characters. Leave every message and native token
unchanged if only those name targets are later adopted. Numbered-token occurrences are not proof of
natural reach; bare `{NAME}`, `{LEADER}`, `{CLASS}`, `{ITEM}` and `{SPELL}` require caller operands.
The frozen 209-row world overlay and six entry rows contain neither proposed numbered token nor
either current target. They are not automatically regenerated by a terminology decision.

The current [CharacterNames](../../src/Sf2.Remake.Application/Runtime/CharacterNames.cs) and
[text runner](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs) consume
canonical package names and capture custom-name precedence; the
[display catalog](../../src/Sf2.Remake.Content/Localization/DisplayCatalog.cs) projects those captured
bindings. None reads the private name-review table. Party/battle/Church/slot names, typed item
labels and spell operands therefore need separately scoped display admission; changing a draft
does not change a running consumer. Keep canonical work, category/index, ActorRef, saved overrides,
and the existing reader envelope unchanged; #1505 owns the later parameter/directory admission design.

Retain current location spellings: the sources disagree about Galam, while Grans island and the
separate old/new Granseal map identities must stay distinct. A guide's unnamed village is insufficient
evidence for Yeel. Preserve accepted class expansions and spell/item names where only synonyms or
JP-version labels differ; neither translated class labels nor a guide's item descriptions establish
US behavior. The private review records the encyclopedia's inconsistent labels for the same
Japanese lightning spell and the guide's water/wine and generic-material ambiguities.

**Unknown / finite next order:** the review samples 10 ally, eight class, six spell and four item
entries plus six prose terms. It does not externally clear the remaining 309 name entries or the
whole 100-term glossary. Japanese class literals, exact localized-build membership, source
independence and compatible translation-import rights remain unestablished. First decide the two
proposals; next, if separately requested, resolve the Yeel/Galam location aliases and early item
identity questions, then the privately recorded Elric/Eric cross-version trap. No search or runtime
acquisition queue follows from those Unknowns. Direct document/source checks suffice for this slice.

## Implemented M1 boundary

- `GameText` loads exact native PO resources and validates the complete English baseline. Selected
  Chinese rows resolve individually with key/effective-locale/reason metadata. Missing or invalid rows
  use the complete English template; required baseline/catalog/font failure refuses Apply before the
  existing settings write. Compiled English recovery retains distinct usable Apply/Cancel/Close controls.
- The existing `InputSettings` draft/Apply/Cancel/store owns optional `language` (`en`, `zh-CN`). Old
  settings default to English. Apply prepares resources, writes once, then publishes and refreshes
  mounted settings access. Read-only, unreadable, invalid, externally changed or unwritable settings
  retain their existing detailed recovery errors (currently English) with a localized explanation.
  No gameplay command, save operation, scene remount or music restart implements a locale change.
- An explicit absolute `SF2_DISPLAY_CATALOG` selects the optional package overlay once per session
  composition. `DisplayCatalogReader` reuses `ExplorationTextTokens`, enforces the 256 KiB/256-row
  envelope and package/locale/private source identity, and retains complete per-row control/name maps.
  Structural failures reject the overlay; semantic row failures discard that whole row and mapping.
  Missing rows remain distinguishable from invalid rows and producer-operand defects. Diagnostics are
  bounded to 128 distinct key/reason pairs per composition and contain no selected private path.
- Application captures source-token ranges and actor/member/default/custom-name bindings at Begin.
  A saved override remains literal even when it equals the default. Content joins control segments;
  translated token indexes never address English arrays. Both English paths and Chinese use the same
  effective visible range after CLEAR. The view retains it through the open window and uses source
  identity/range for reveal continuity. Missing producer operands stop delivery with `display-operands`.
- HEAL captures caster/target/spell/amount and display names beside unchanged ActionText/RecoveryText.
  The view formats `battle.heal.action` / `battle.heal.recovery`; it does not repair an English prefix.
  Reading/reveal/ReadMore do not add logical opportunities, MP cost, recovery, EXP or RNG draws.
- The public authored `localization-pilot.json` / `.zh-CN.json` contain original prose, simple solid
  diagnostic art and a synthesized tone, without extracted source content. W1/W2/CLEAR/N/D1, captured
  names, mixed-script reveal and ReadMore exercise ordinary host/session consumers. Other dialogue
  paths, default-name display and other UI/results remain English/unmigrated. Static terminology
  adoption is recorded above.
- One unmodified [Noto Sans CJK SC Regular](../../game/fonts/localization/README.md) is bundled under
  OFL 1.1, version 2.004, pinned upstream revision `f8d157532fbfaeda587e826d4cd5b21a49186f7c`.
  It is 16,437,364 bytes; the necessary 4.7.2 font import produced 14,444,319 bytes in about 6.3 seconds.
  The initial wrapper-only memory sample does not measure importer peak memory; necessary native
  consumer runs measured roughly 262–336 MB peak working set. System fallback is disabled. Actual
  glyph coverage, mixed punctuation/wrapping, 1080p geometry, ReadMore and silent instant/reveal-all
  versus normal speech have bounded native observations. Human typography/translation acceptance
  and export packaging are not claimed.

See [native reproduction](../verification/host.md#localization-pilot) for startup, settings and direct
failure entry points. Seven-character ASCII naming and game-save identities remain unchanged; this
is Chinese display, not Chinese IME naming. Older executables reject the new settings property:
retain the file, then use the existing explicit Restore defaults recovery or restore an older
preferences backup for downgrade. Never rewrite game slots to change language.

## Implemented M2a Church/Search boundary

The current slice owns English/zh-CN Church menus, offers, results, choices, errors and hints, plus
Search source-ID rows and typed item/gold/full/empty feedback. Entry/slots, other menus, private script
translation batches, Unicode names, new services, layout redesign and save migration remain outside
scope. Acceptance preserves payment/queue order, atomic reward/flag/chest publication, unavailable
services, ordinary input/release, English fallback and controls recovery. Entry/save consumers have
their separate M2b boundary below; independent integration remains main-gate work.

- Application `ChurchWait.Phase` derives only from typed Action/Result. `ChurchResult` retains
  NoApplicableMember, Declined, InsufficientGold or Paid with the immutable treatment/cost; the curse
  notice is an independent `CurseUnavailable` value at Cure completion. Translated string/nullness
  never chooses Answer versus Continue. Member order, poison-before-stun, payment-before-effect,
  Raise cap, Cure insufficient-funds family skip, unavailable Promote and Save's caller-close order
  remain unchanged. The view keeps only focus/release state and retains failure code/field/details.
- Search retains its existing Kind/Texts/Receipt/Recipient/ItemWord/Gold authority. `ItemName` captures
  the canonical label from the same validated field definition used for the grant; item/full/gold
  feedback no longer generates English `Message` sentences. Message remains only existing unavailable
  destination detail. The view joins admitted source-ID rows through `DisplayCatalog`'s existing
  token/name mapping and whole-row fallback, then formats semantic templates. No second event stream,
  reward computation, source-text execution or source definition mutation is introduced.
- Missing/invalid templates and source rows retain key/reason/effective-locale diagnostics and visible
  English fallback. Literal custom-name precedence uses the existing resolver. Untranslated default
  names/items remain English and are explicitly labelled as such. 中毒 / 眩晕 / 诅咒 are this UI's
  terms; they do not approve a game-wide naming glossary or publish private translated scripts.
- The original public pilot also admits a small Church and Search sample. For connected authored
  packages, existing `battleDiscard.items` undergoes its full id/name/rare/unsellable and loadout
  validation, including same-ID label agreement, and contributes only `FieldPreparation.Items`.
  `BattleDefinition.Discard` remains null. It grants no field Use, spells or map effects. The existing
  Party read-only query uses this same catalog after private source-name priority. Connected ally
  starts can carry the finite none/poison/stun/curse combinations; all other battle-start operands
  still undergo normal validation with neutral status. Standalone status and `battlePoison` algorithm
  restrictions are unchanged; carrying a field status does not admit new battle status effects.
- Actual 1080p native keys and pointer press/release cover English/Chinese Church payment, decline,
  insufficient Cure family skip, curse/no-applicable/Promote and Save closing; Search item/full/repeat,
  gold, named description, missing source/invalid template fallback, field-only item 113 and ordinary
  language Apply before a repeat. Resources/revision/flags/chest/RNG remain authoritative. Search's
  existing panel measures its hint; Church reserves a minimum 192px for the fallback/refusal footer
  and grows its panel from 824px with measured text. No screenshots, world copy, new font/import installation or emulator is used.

[Host reproduction](../verification/host.md#localized-church-and-search) owns the finite original
pre-start variants and observation entry points. Human translation/typography and private-content
coverage remain unaccepted. The existing High save/restore boundary and format are unchanged.

## Implemented M2b entry and save boundary

English/zh-CN now cover title/Witch controls, New Game setup, restricted-name help/errors, slot
status/destination/UTC time, Save/Quit/Continue, overwrite/backup recovery and recoverable failures.
Node/action identity is independent of labels. Custom names, including an explicit `Bowie` override,
stay literal. Frozen runtime overlays retain their historical source-name choices; current static
terminology is recorded above and requires separate runtime admission.
Only the existing Normal difficulty, seven-character restricted alphabet and save recipes are admitted.

`EntryPresentationReader` retains the validated entry profile/upstream commit/ROM identity alongside
its existing images, six dialogue rows and audio. Optional absolute `SF2_ENTRY_DISPLAY_CATALOG`
selects a separate display overlay for that actual entry source; `SF2_DISPLAY_CATALOG` still selects
world text. Both use the existing bounded reader/token/control mapping, with no fabricated gameplay
scenario. Entry package is `private-local-entry`, namespace `entry.text`, IDs 216–221. The source
object requires `repository`, `commit` and `romSha256` matching the admitted entry identity. An absent
overlay, missing row, invalid row or structural/source failure keeps the corresponding complete
English text and distinct effective-locale/reason metadata. No private translated script ships here.

Entry and naming hints and the slot footer expose UI fallback reason/key separately from entry
source fallback. Error rendering retains actual failure/code/field/retry operands. Successful Apply
refreshes existing entry/error and New Game/Save/Quit labels without replaying ShowMenu, audio,
page advancement or session work. A changed menu body resets only its visual reading position.
Long greeting/menu/error text uses the existing ReadMore mechanism: Confirm or a pointer command
reads before activation; a visible Read more control occupies the first menu row until reading
finishes. Settings and Cancel remain accessible, and ordinary short text adds no reading step.

Persistence changes are display-only: destinations derive from `Recipe`, never English Description.
Final confirmation still precedes New Game write; write precedes mount; retry retains the prepared
candidate/seeds; uncertain publication still offers Inspect Continue (检查存档). Backups, stale revision,
Church caller return, UTC timestamps, game-save format and preference write/publish order are unchanged.
[Native reproduction](../verification/host.md#localized-entry-and-save) owns actual input, long text,
fallback and independent-process observations. Image lettering, private dialogue translation, other
menus, Unicode/IME names, full-opening coverage, export and human translation/typography acceptance
remain outside this slice. Standard rigor and the existing High save boundary continue unchanged.

## M3 private Witch-entry review sample

[#1486](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1486) prepares exactly
`entry.text/216–221` from the admitted `private-local-entry` source as one optional Simplified Chinese
suggestion and a private bilingual review table. The existing Standard scope remains appropriate:
this is a finite content/review pilot through admitted consumers, with no code, public PO, schema,
source package, glossary, default-name or Unicode/IME change. This historical sample does not itself
authorize a broader batch; the current full-text agreement above supplies the later authorization.

The source inventory is six rows, 235 characters and 49 simple English word matches, with no brace
control/name tokens. Literal newline counts are `1,2,1,1,1,0`; the draft preserves those counts while
placing breaks at Chinese phrase boundaries. Rows 219–220 carry one conditional meaning across the
existing page boundary. This preserves their source IDs and connected meaning, without claiming
original rhythm, hardware timing or human-approved pacing.

The persistent private owner is outside Git and removable worktrees, with exactly
`entry.zh-CN.json` (existing-reader overlay), `review.zh-CN.md` (six bilingual rows, IDs, rationale and
approval state) and `README.md` (ownership, read-only source identity and finite local reproduction).
It is user content, not cleanup scratch; retiring its consuming worktree must preserve it. No nested
repository, link, complete entry-resource copy or publication of source/translated prose is involved.
The overlay pins the admitted repository/commit/ROM identity through the existing `source` object;
`SF2_ENTRY_DISPLAY_CATALOG` selects its absolute local path independently of the world catalog.
Source inputs stay read-only. Local use and reader admission grant no distribution rights.

**Confirmed technical boundary:** the existing Content reader admits all six rows with exact Chinese
resolution and no diagnostic/fallback. Finite headless ordinary title/greeting input visits every
Chinese and English row, checks body/hint glyphs and geometry, uses keyboard and pointer continuation,
and reaches the existing menu/slot-cancel path. Locale Apply retains the menu page/selection, texture
identities and audio receipts; no gameplay session or save is created. The
[verification owner](../verification/host.md#private-witch-entry-translation-pilot) records the retained
recipe, actual results, caller corrections and shutdown warning. This reuses the retained adapter and
imports, with no SDK/research/full/H3 or campaign/save-compatibility rerun.

Main-gate's independent semantic review passed for this suggestion with the requested Chinese phrase
breaks in rows 218–219. **The user approved this sample's text tone on 2026-11-03. In-game typography
and actual reading approval remain pending** in the private review artifact. That tone approval applies
to this speaker and does not prescribe other characters' voices. The engineering sample task may close after independent integration while epic #1468's human checkpoint
remains open. Terminology, other private dialogue, image lettering,
full-opening Chinese, Unicode naming and publication remain outside this sample.

## M3 private opening-world Chinese text

[#1491](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1491) prepares the complete
bounded text inventory for the implemented Map3–Battle01 opening and usable return: **209 unique
`world.text` rows, 9,995 source characters including controls; 206 additions and the three unchanged,
independently reviewed house-exit rows 510/511/483.** Standard rigor and the existing High save/identity
boundary apply. This is display content, without canonical script, source package, PO, gameplay,
save-format, glossary, default-name or Unicode/IME changes.

The [opening guide](../../../docs/design/synthesis/chapters/grans-opening/opening-through-battle01.md)
and [offered-branch inventory](../content/opening-branch-inventory.md) select default and supported
F501 phases. Transitive program calls/branches retain text-cursor progression; Search also consumes
its marker prefixes and empty-result rows. Exact private per-row program/instruction/source bindings
and bilingual pairs belong to the persistent review and ignored reproduction inventory.

| Existing consumer group | Unique IDs in that group | Rows |
| --- | --- | --- |
| Map3 house, school, messenger Yes/No/re-prompt, joins, gate and local conversation | 447; 480–505; 507–542 | 63 |
| Map19 default/F501 conversations and adviser first/repeat choice | 543–562; 575–578; 583–586 | 28 |
| Map20 default/F501 conversations, royal visit and exorcism | 563–567; 570–573; 588–596; 2176–2212 | 55 |
| Map21 default/F501 guard and speakerless fainted line | 568–569; 579–580 | 4 |
| Battle01 before/after story | 2292–2310 | 19 |
| Ordinary defeat through the legacy `DialogueWait` display | 363 | 1 |
| Shared Search prefixes/results and default/F501 descriptions | 403–405; 408–410; 412; 423; 425–430; 4035–4059 | 39 |

**Confirmed static coverage:** the 209-row inventory includes 30 F501 castle-return alternatives;
those are admitted content, not a claim that this task naturally executed each branch. No text table
is invented for Map40's absent NPCs or returned Map57. F609/F506/F543 setup variants, the markerless
later Map21 zone body, Map66/Battle02 and later campaign are excluded. Church's typed menus/results
remain the existing M2a consumer; this inventory does not invent source dialogue for that service.

One persistent private owner outside Git and removable worktrees contains exactly
`world.zh-CN.json` (the sole writeback source), `review.zh-CN.md` (all pairs and review disposition)
and `README.md` (ownership, scope and reproduction). `SF2_DISPLAY_CATALOG` selects its absolute path;
the separate Witch files and original inputs remain read-only. The overlay pins the admitted package
`private-battle01-to-battle02` and its exact repository URL/commit/ROM source object. Provenance is
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`. No original/translated prose,
private review table or absolute input path is published in Git. Local admission grants no distribution
right, and worktree retirement must preserve these three user-content files.

The measured 209 rows require changing only the existing reader's finite entry ceiling from 64 to
256 (47 rows of headroom); 256 KiB total, 8,192 characters per row, strict UTF-8/shape/source/duplicate
and control-segment name checks remain unchanged. The current catalog is 28,053 UTF-8 bytes. There is
no segment loader, merger, manifest, cache or second content authority. Unapproved proper/default
names and source class abbreviations remain English; captured custom names remain literal. Every
assigned sentence is translated; names alone are not untranslated-sentence permission.

**Confirmed technical boundary:** whole-catalog actual Content admission resolves all 209 rows in
Chinese without diagnostics/fallback and preserves controls/captured names; missing/invalid-row English
fallback and source-mismatch rejection still pass. Strict source/token/name/ID, per-row/total length,
placeholder and repeated-target QA pass. The actual bundled body font covers every selected Chinese
code point. Ordinary New Game observes 29 selected house/school/messenger/join windows, retained
Chinese choice text, reveal/paging/confirmation and usable field return; a separate explicitly
controlled before-battle seam observes rows 2292–2294 through the actual view. These representative
observations do not prove every static alternative's reach, all physical line fits or original timing.
The [verification owner](../verification/field-programs.md#private-opening-world-chinese-text) retains
reproduction, completed failures and the unattributed ObjectDB shutdown warning.

**All 209 rows have connected consumers.** Ordinary-defeat row 363 resolves through the existing
whole-row catalog at the legacy `DialogueWait` view, after retained explicit field projections take
precedence. Its sole `LEADER` token retains member-0/custom-name identity through `CharacterNames`;
unsupported bare `NAME`/`CLASS` contexts use explicit `display-operands` fallback. This adds no logical
text work or engine wait. Controlled native source-body checks confirm Chinese/English, complete
English fallback, reveal, acknowledgement and usable field return; a small authored defeat confirms
ordinary outcome return. These seams do not prove prior natural battle reach. Reader admission alone
is not Chinese rendering proof. Main-gate independently passed all 209 bilingual
pairs after the retained wording corrections. **User text-tone, in-game typography and actual reading
experience remain pending.** The Witch's approved tone does not prescribe other speakers. This does
not complete epic #1468, whole-game Chinese, image lettering, naming/IME or user visual acceptance.

## Current production and consumption paths

**Confirmed by source inspection:** the following is a bounded responsibility inventory, not an
exhaustive string dump. Linked files and symbols locate the claims without relying on line numbers.

| Producer / identity | Current consumer | Consequence for localization |
| --- | --- | --- |
| [SettingsView.Refresh](../../game/src/UI/SettingsView.cs), [SessionEntryView](../../game/src/Entry/SessionEntryView.cs), [NameEntryView](../../game/src/Entry/NameEntryView.cs): migrated UI keys/help/errors | Godot Button/Label/LineEdit and hints | M1/M2b use stable keys and action/node identities. The UseName button reference and entry node names no longer depend on displayed English. |
| [FieldService.Submit/Completion](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldService.cs): M2a `ChurchWait.Result` retains typed outcome/treatment and independent curse notice | [ChurchServiceView.Present](../../game/src/Exploration/ChurchServiceView.cs) | M2a consumes finite result/effect/cost and explicit phase; payment/queue advancement precede display. English wording has no phase authority. |
| [HealingSceneCursor.Create/Enter](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs): names/spell/amount substituted into `ActionText`/`RecoveryText`; private IDs 274/298 | `Enter` creates logical work per character; [BattleSceneView.Message](../../game/src/Battles/BattleSceneView.cs) displays the same strings | Chinese replacement would change work and potentially fairy/RNG progression. Retain execution strings and add semantic display operands for M1. |
| `BattleSceneState`: phase, actor, target, amount, rewards, item and growth values | `BattleSceneView.Message` preserves English source IDs and projects typed Chinese functional templates | Most semantics already exist. Reuse those fields, without a duplicate event bus. Reward/HP publication remains in the continuation. |
| [BattleGiveState.Text](../../src/Sf2.Remake.Application/Runtime/Battles/BattleGiveContinuation.cs), [BattleDiscardState.Text](../../src/Sf2.Remake.Application/Runtime/Battles/BattleDiscardContinuation.cs), [BattleRetreatContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRetreatContinuation.cs) | Respective battle views | M2e views format retained typed actor/item/returned/payment operands through GameText. Canonical Text/TextFor remain unchanged; authored English is not a universal source-text ID. |
| [remake_exploration_content.py](../../../src/sf2tool/remake_exploration_content.py) reads pinned `disasm/data/scripting/text/gamescript.txt` hexadecimal IDs and `allynames.asm`; [ExplorationContentReader.ReadTextTokens](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) tokenizes text | [ProgramRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) selects DialogueWait, W1TextWait or FieldTextWait; [ExplorationDialogueView](../../game/src/Exploration/ExplorationDialogueView.cs) has separate projections | Reuse integer ID plus namespace, not English spelling or list position. The compatibility regex path is not a general token interpreter. |
| [ExplorationTextRunner.Begin/Normalize/Span/Accepted](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs) expands names/classes and builds source glyph units | [FieldTextWait](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs) stores units, projection and logical cursor; host separately reveals/delivers/acknowledges | Characters above 255 or absent from source font mapping reject (`field-text-symbol`). Glyph advances determine logical wrap/scroll work. Chinese must not replace those units/advances. |
| [EntryPresentationReader](../../src/Sf2.Remake.Content/Scenarios/EntryPresentationReader.cs) admits private dialogue keys 216–221 and six named image resources | `SessionEntryView` / [EntryPresentationAssets](../../game/src/Entry/EntryPresentationAssets.cs) | Entry has a separate namespace/profile/preparation path. Title/numeral image lettering is not fixed by a string catalog. |
| [CharacterNames.Member/Battle/Actor](../../src/Sf2.Remake.Application/Runtime/CharacterNames.cs): actor overrides, source names or authored readable identity | Dialogue, party, battle, Church and slots | Reuse identity binding and override precedence. ReadableIdentity/"Name unavailable" are fallbacks, not approved Chinese names. [BattleCharacterNames.HealingText](../../game/src/Battles/BattleCharacterNames.cs) repairs only an authored leading subject today; replace that workaround for migrated semantic consumers. |
| Item/spell/class identities and labels from [PrivateBattleDefinitionReader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), definitions and [BattleSceneDefinition](../../src/Sf2.Remake.Application/Content/Scenarios/BattleSceneDefinition.cs) | Choices, party details, scene messages and offers | Preserve packed item flags/slot, item ID, SpellRef/level, class ID and ActorRef. Never translate IDs or uppercase translated spell names. |
| `SessionFailure.Kind/Code/Field/Message`, Content and storage failures | Recoverable UI and local diagnostics | Map finite codes to user messages; retain stable code/field/stop reason. Unknown failures get safe generic wording plus code. English fallback cannot turn unsupported content into success. |

At the inventory baseline there was no game localization catalog. M1 now supplies the finite
`GameText`/PO/Content overlay composition described above.
[StoryTheme](../../game/src/UI/StoryTheme.cs) sets styles and 30px body/24px hint sizes, without an
explicit CJK font. [StoryDialoguePanel](../../game/src/UI/StoryDialoguePanel.cs) already uses plain
Label, WordSmart, CharsAfterShaping, measured geometry and ReadMore. It owns reading position and
geometry, not waits or simulation.

### Persistence coupling

**Confirmed:** [OpeningSave.Compatibility](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs)
uses profile/package, gameplay revision, rules/revision, recipe revision and private repository,
commit and ROM identity. It does not hash the world export or UI strings.
[OpeningSaveData](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSaveData.cs) stores
actor-keyed Names, semantic TextCursor and ExplorationTextSettings, but no active dialogue string,
FieldTextWait, Church message or healing cursor. Save eligibility excludes pending interpreter work.
Optional Names admits pre-naming version1 saves.

[OpeningRestore.Build](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs) checks
saved nonempty names with [NewGameName.Error](../../src/Sf2.Remake.Application/Runtime/NewGameOptions.cs):
at most seven UTF-16 code units from its explicit ASCII alphabet. Empty new input keeps the default;
[OpeningNewGame](../../src/Sf2.Remake.Application/Runtime/OpeningNewGame.cs) stores supplied names
verbatim. Custom names are progress; default names/translated labels are content/presentation.

**Design implication:** a separate display catalog/preference needs no game-save version or
compatibility revision change. Changing canonical programs/text work/source pins is different and
still follows existing compatibility rules. Translation motivation does not make such changes safe.

## Recommended composition

Use **Godot translation resources for UI and authored dynamic templates, a small Content-read
display overlay for package text/names, and semantic Application display operands**. Reuse C#
formatting, typed source tokens, name binding, settings and presenters. Add no production assembly,
Domain locale dependency, message bus, remote translation service, automatic extractor or database.

| Approach | Benefit | Decision / cost |
| --- | --- | --- |
| Recommended combination | Native PO tools for host text, Content trust for package overlays, plain Application operands | Two shapes match public host versus package-bound control ownership. Cost is one finite resolver/reader and explicit consumer refresh. Missing-key visibility and lost control/name bindings justify them. |
| All text in Godot PO/Translation | One translator format, namespaced numeric keys, native plurals | Viable if Content still validates package identity/control shape and Application supplies semantic operands. PO alone cannot solve expanded FieldTextWait or HEAL work. Flattening freely editable controls would require separate metadata anyway. |
| All text in C# JSON/RESX with Godot bridge | Shared resolver for a future non-Godot frontend | Viable with a real second frontend. Today it duplicates native catalogs/plurals/resources without that consumer. No ICU/SmartFormat package or separate assembly is needed for these two languages and examples. |

Godot 4.7 documents [PO loading](https://docs.godotengine.org/en/4.7/tutorials/i18n/localization_using_gettext.html),
[Translation](https://docs.godotengine.org/en/4.7/classes/class_translation.html) lookup/enumeration,
and [TranslationServer](https://docs.godotengine.org/en/4.7/classes/class_translationserver.html)
locale selection/normalization/plurals. Its [internationalization guide](https://docs.godotengine.org/en/4.7/tutorials/i18n/internationalizing_games.html)
shows C# Tr/TrN with C# formatting. These version-family API facts were checked 2026-11-03; the
project pins Godot.NET.Sdk/4.7.2. The M1 pilot now compiles and exercises those installed bindings
at the bounded native boundary above; the API documentation alone is not execution evidence.

### Modules and flow

```text
Domain: IDs, rules, numbers, RNG                              (unchanged)
   ^
Application: GameSession -> typed waits/results + display operands
   ^                    (no locale, Godot, paths or translated strings)
Content: source admission + bounded display-overlay reader
   ^                    (does not mutate execution definitions)
Godot GameRoot: admitted source + overlay + public PO + preferences
   -> finite GameText resolver -> existing Label/Button/StoryDialoguePanel
   <- actual reveal/read/ack -> existing GameSession commands and tokens
```

Existing references already permit Godot to use Content/Application/Domain and Content to use
Application/Domain. Keep immutable overlay representation with its reader in Content: only Godot
consumes it. Application exposes only missing display facts, adjacent to existing waits/results.
A finite Godot `GameText` class is enough; no interface/service locator is needed merely for three
views. The credible failure is loss of semantic identity before display, not lack of extensibility.

## Identity, loading and formatting

### Catalog ownership

- Stable host keys describe meaning, e.g. `ui.settings.title`, `ui.settings.language`,
  `battle.heal.recovery`. Wording edits do not rename keys; changed meaning/argument contracts do.
  Proposed files: `remake/game/localization/en.po` and `zh_CN.po`, with PO comments identifying
  consumer and argument roles. English is complete for each migrated consumer.
- Package addresses are `(package, namespace, id)`: `world.text/42`, `battle.text/298`,
  `entry.dialogue/216`. Reuse integer source IDs without merging namespaces. Names use existing
  member/actor/item/class/spell identities scoped to package. Map product `zh-CN` to Godot `zh_CN`;
  `en` is English. Admit only these two initially; OS/thread culture never chooses gameplay rules.
- Keep canonical packages immutable. A separately selected UTF-8 overlay names package/locale and,
  for private content, the existing repository/commit/ROM identity. Join admitted metadata; do not
  add a hash protocol or alternate trust root. Read once at composition, with no cwd/other-worktree/
  network search. Never duplicate/rebuild the large world merely to change display language.
- Original UI/templates and an original authored pilot overlay may be public. Extracted text,
  source-derived translations and private names remain private by default. The existing
  [asset/preparation owner](../content/presentation-assets.md) governs promotion. Host selects private
  overlay by explicit absolute configuration and passes it through Content; diagnostics expose no
  private paths. Runtime never reopens ROMs/upstream checkouts.
- For drift use Git/direct source comparison of touched IDs, controls, bindings and meaning. Do
  not add change-detection hashes/dates. A private overlay with a different pinned source identity
  is unavailable until reviewed. Same control shape does not prove a changed source meaning safe.

Reject the entire optional overlay for malformed UTF-8/JSON or object shape, unknown fields,
duplicate IDs/locales, size-limit violations or wrong package/source identity: its membership and
binding context cannot be trusted. Once that structural/context admission succeeds, reject only an
individual row for an unknown target ID/namespace, unbound placeholders, unsupported tags or changed
control topology. Other independently valid rows may remain available. Missing rows mean explicit
partial coverage. Optional overlay failure never invalidates the gameplay package; required source
admission still fails normally. Proposed M1 bounds are 256 KiB/64 entries per overlay with no external
includes; these are tiny-pilot limits, not measured full-game requirements.

### Source controls and names

The existing parser distinguishes Literal, Newline (`{N}`), Wait1, Wait2, Delay1, Clear, MemberName
(`{NAME}`, `{NAME;n}`, `{LEADER}`), ClassName and Unsupported. Reuse these token kinds, not another
tag language. Validate the same ordered W1/W2/D1/CLEAR sequence; preserve explicit N tokens in M1.
Visual wrapping can add lines without adding source instructions. Name/parameter occurrences may
move for grammar within a control segment, but retain identity/multiplicity and cannot cross a
control boundary. Unknown source tags remain unsupported for localization until understood.

Resolve LEADER and instruction-selected NAME when Application begins the message, using current
membership/instruction semantics. Capture identity, custom override and canonical fallback; do not
reselect the leader on redraw. Class/item/spell display uses its typed operand, not a word inferred
from final English. Name precedence is saved custom value verbatim, selected-locale admitted default,
canonical English default, then existing authored readable-identity/unavailable behavior plus a
diagnostic. No dictionary translates a custom name. The documentation [glossary](../../../docs/design/glossary.md)
and zh-meta are references, not approval of game names or a game translation protocol.

Current consumers are plain text. Braces in arguments are data: format a validated template once,
never retokenize its output. Keep Label rather than enable BBCode. A future rich-text consumer must
escape inserted values at its own boundary; catalog text cannot execute markup, paths or commands.

### Failure semantics

| Condition | Runtime behavior | Diagnosis / acceptance |
| --- | --- | --- |
| Selected row missing | Complete English row for that message/name | `missing-translation`, key/effective locale; not counted as translated |
| Structurally valid row with wrong target/parameters/tags/controls | Reject the entire row and its derived mapping; canonical English row/bindings only | `invalid-translation`, not an ordinary missing row |
| Optional overlay missing or structurally corrupt/wrong identity | Reject whole overlay; English display over valid gameplay; bounded unavailable notice | Retain requested versus effective locale and stored preference; no automatic rewrite |
| Required English UI row absent/invalid | Refuse candidate/Apply; startup has small compiled English recovery text and usable recovery controls | Packaging defect; no raw key or empty success message |
| Required canonical source text missing/unsupported | Existing Content/Application failure | Translation cannot invent executable source |
| Runtime operands absent/wrong type | Safe generic message plus code; retain pending delivery when semantic result cannot be established | Producer defect, not missing translation; no acknowledgement of unseen success |

Return text and effective locale/fallback reason for inspection. Settings shows partial/unavailable
coverage; ordinary English fallback need not interrupt every message. Bound diagnostics once per
key/reason per loaded catalog, rather than logging on redraw. No persisted report or global telemetry
system is needed. Require exact per-locale presence using loaded Translation resources/message lists;
do not infer it from `Translate(key) == key` or opaque native fallback behavior. For migrated dynamic
labels resolve explicitly, then avoid a second automatic translation of the formatted result.

Use .NET [composite formatting](https://learn.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting)
with numbered arguments and semantic-role comments. Validate exact allowed argument indexes before
enabling a row; successful formatting does not prove all required arguments occur. Reordering and
repetition follow that row's declared contract. M1 needs strings/nonnegative integers, not nested
expressions or custom format providers. Use explicit display culture; JSON/IDs/provenance stay
invariant. Use native plurals when an actual consumer requires them, not global English plural rules.

## Three real consumer examples

Shapes below are proposed APIs/data, not existing code or approved game translations. Example
dialogue/templates are original miniature content, without private source prose.

### Settings

```text
en.po:    ui.settings.language -> Language: {0}
zh_CN.po: ui.settings.language -> 语言：{0}
argument 0: endonym of draft selection (English / 简体中文)
```

`SettingsView.Refresh` resolves its title/help/Apply/Cancel through GameText. Actions remain indexes
or callbacks, not translated strings. `SettingsHost.ApplySettings` prepares catalog/font before
`SettingsStore.Write`, then publishes the existing InputSettings object and prepared display
selection. Failure retains the draft, old live locale and old file; no GameSession command is sent.

### Field dialogue

```json
{
  "package": "authored-localization-pilot",
  "locale": "zh-CN",
  "texts": [{"namespace": "world.text", "id": 42,
    "text": "{NAME;0}，请稍候。{W1}{CLEAR}门已打开。{W2}"}]
}
```

Canonical authored row: `{NAME;0}, please wait.{W1}{CLEAR}The door is open.{W2}`.
Neither sentence opens a door; only the program can change it. Extract just the existing
ExplorationContentReader tokenization responsibility for reuse by the overlay reader, not a second
script parser. Add the missing projection facts adjacent to FieldTextWait/window ownership:

```csharp
// Shape sketch: accompanies the existing wait, never issues commands.
record FieldDisplayRange(int TextId, int FirstSourceToken, int EndSourceToken,
    IReadOnlyList<DisplayNameBinding> Names);
record DisplayNameBinding(int SourceToken, ActorRef? Actor, int? Member,
    string? CustomName, string CanonicalName);
```

Capture source-token to expanded-unit boundaries during Begin. Span and CLEAR publish the visible
source range, including the current cumulative prefix before W1/W2 and the reset after CLEAR. Keep
the display value on the open window through presentation/choice/close waits as the current resolved
projection is retained. Projection alone has lost name identities/token boundaries; do not reverse
parse it or infer range from English prefixes.

Content joins ranges by canonical control-boundary ordinal, not by applying an English token index
to the translated array: moving a name can change literal-token counts. Within each joined segment,
bind parameter identities/occurrences to their captured source bindings. Retain that small mapping
with the admitted overlay. The range is an execution observation; translated layout is disposable.

GameText selects translated tokens for that ID/range and captured bindings, emitting only visible
literals/names/newlines. Canonical units and FieldTextPhase remain the executor. W1/W2/D1/CLEAR never
execute from translation. D1 retains its logical pause, not a new host timer; CLEAR changes projection
when the runner clears; W1/W2 retain current next-token/ack boundaries. Invalid/missing translation
falls back as a whole row to preserve mapping. Discard all translated fragments and derived
control/name mappings for that rejected row; derive English display solely from canonical tokens
and the captured source bindings. Never combine English literals with a rejected translation's
range or name map, including when only one translated segment is invalid.

Reuse ExplorationSessionView delivery: translated reveal/ReadMore precede CompleteTextReveal, while
logical readiness still gates Acknowledge. Replace English-prefix reveal continuity for this migrated
consumer with `(window identity, text ID, source range)` continuity. Identical text in separate waits
must still deliver. Visual wrap/ReadMore never adds a script acknowledgement. Preserve skip and
release barriers. Legacy `DialogueWait` without a retained explicit projection reads the whole
canonical ID through the same catalog/name resolver as Search. Only explicit member operands are
bound there; Search retains its established implicit-leader context. Missing/invalid overlays retain
complete English body/locale/reason, while missing name/class operands use `display-operands`. The
compatibility regex is outside this migrated branch. Unmigrated W1 and battle-story bodies retain
their existing English boundary.

### Dynamic HEAL result

```csharp
// Captured from HealingSceneCursor.Create's accepted action operands.
record HealingDisplay(ActorRef Caster, ActorRef Target, SpellRef Spell,
    int RecoveredHp, int? ActionSourceText, int? RecoverySourceText);
// Private IDs: 274/298; authored fallback uses null source IDs.
```

For recovery, argument 0 is the target's display name and 1 is RecoveredHp. Original demonstration
templates can be `Recovery for {0}: {1} HP.` / `{0}恢复了{1}点生命。`. Private overlays bind
`battle.text/298` to existing NAME/number semantics. Cast text separately binds caster/spell/level.
Capture any custom name needed by the message from that same accepted state.

Keep ActionText/RecoveryText as execution/source text generating exactly the same HealingWork.
BattleSceneView.Message consumes HealingDisplay for the migrated branch instead of prefix repair.
Reuse token, Delivered, AtTimedInput, LogicalComplete and completion kind. Current host delivery
waits for timed-input/logical completion; preserve that boundary. Display length must not add
AdvanceSimulation calls, fairy work, MP spending, HP recovery or EXP publication. Existing state
already suffices for most other battle results; add records only where facts are actually lost.

For later Church migration use finite result kind/effect/cost and a separate curse-unavailable flag.
Represent `Message != null`'s current phase meaning explicitly. A translated phrase cannot decide
whether the next member receives treatment.

## Locale preference and lifecycle

Recommend **English by default** for missing preferences during partial rollout, with endonym choices
English/简体中文. Do not silently select partial Chinese from OS locale. Reconsider automatic selection
after coverage acceptance, without an `auto` mode now. Persist `language: "en" | "zh-CN"` in the
existing InputSettings/input.json responsibility, separate from progress.

InputSettings.Parse rejects unknown fields today. Add optional language with English default,
validate it with the complete candidate, and copy it in Draft/Publish. New code reads old version1
preferences; an older executable rejects the new property. Preserve those bytes under existing
recovery behavior and document downgrade recovery. This is settings forward compatibility, not a
game-save format problem; no second store or game-save version change is needed.

Keep [current safe access](../godot/presentation.md#basic-preferences-agreement): settled title/idle
field, outside naming, text/choices/services/battle/slots/transitions. Existing host pause freezes
the session. Selection edits only the draft; Cancel/window-close discard it. `--input-settings`
overrides stay read-only. Unconfigured/unwritable storage retains existing Apply refusal.

Apply: validate all settings/bindings; prepare UI catalog/font and optional overlay status; write
once through SettingsStore; publish settings/prepared locale; refresh mounted labels/hints/themes;
close with the existing release barrier. Required UI/font failure refuses before writing. Missing
optional private rows allow English fallback and a partial notice. Never submit gameplay, capture a
save, restart programs, replay audio or remount a scene to refresh text. Do not change process-wide
CurrentCulture. Native locale notifications can assist controls; formatted labels still need their
existing Present/Refresh paths called explicitly.

Startup distinguishes requested from effective language. Missing zh-CN retains the preference and
a readable English recovery route. Continue admits progress normally and projects it in the host's
current language without rewriting the slot. No mid-message Apply or catalog/font disk hot reload
is supported initially, so a locale change cannot reset active reveal, speech or confirmations.

## Implemented M2c Party and field Prepare boundary

**Standard rigor:** reuse the accepted catalogs/font/settings for the existing read-only member screen
and field preparation. English/zh-CN cover stat and availability labels, member/list hints, inventory
status, action choices, confirmations, cancellation, NoEffect and refusal/recovery. Class and missing
values remain unavailable. Valid admitted member/item/spell names and literal custom names retain
their existing resolver precedence; only explicitly missing names receive a localized fallback.
This scope changes no Application/Domain/Content behavior, naming/save compatibility or field effect.

Stat node IDs remain `LEVEL`, `ATTACK`, `DEFENSE`, `AGILITY`, `EXP`, `MOVE`; displayed Text is localized.
Menu stages, option indices, actor/slot/packed-word closures and captured revision still select typed
FieldCommands. Deferred display getters redraw the existing choice without rebuilding commands,
resetting focus or submitting a transaction. No English sentence parsing chooses an operation.
Settings remains unavailable while Party/Prepare is open; close, Apply and reopen to change language.

Known failure codes select finite localized recovery advice; exact code/field/source details remain
in a separately identified diagnostic line. Unknown codes use general refresh/reselect advice.
NoEffect still reads the existing `field-no-effect` observation. The title measures wrapped text and
moves choices below it; unavailable-query Back/Cancel returns to the member screen rather than
reopening the same failed query. The measured footer keeps hints, untranslated-name notice and key/reason
fallback readable. Missing Chinese rows use complete English templates. Existing small triangle
markers lacked bundled glyphs; supported triangle arrows keep the same focus/selector meaning.

The M2a field-only catalog still grants no Use, spell or map effects. Supported native transactions
reuse the admitted opening/retained Prepare route; localized presentation does not establish natural
acquisition or add Antidote/DETOX content. [Native reproduction](../verification/host.md#localized-party-and-field-prepare)
owns readonly/navigation, confirm/cancel, transaction, real refusal and font/geometry observations.
Other battle/result consumers, terminology/private dialogue, Unicode names, human translation and
typography approval remain outside this boundary; integration remains main-gate work.

## Implemented M2d battle menu and status boundary

**Standard rigor:** [#1478](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1478) extends
GameText/PO/font through the existing battle movement/action/target menus, equipment choices and
shared low-HP/defeated suffixes. Preserve the 1080p/96px layout and existing session/input authority.
Acceptance is adapter/catalog checks plus finite English/zh-CN native state/input/geometry/error
observations, followed by the committed plan and selected CI. No Domain/Application/Content, save,
schema, new message system or engine-behavior test is required by this display-only change.

- Diamond Identity/Name and `command-menu.*` raster keys stay Attack/Magic/Item/Stay; localized
  Caption supplies the selected heading and unmounted text fallback. Original mounted image
  lettering remains untranslated. Focus timers, `command:`/`row:`, navigation node IDs, action refs,
  target legality, session/revision/actor boundaries and pointer release remain unchanged.
- BattleActionChoice.Empty/Kind/Spell/ItemSlot select generic wording. Admitted spell/item/member
  names and literal custom names retain the existing resolver; English Label text never selects
  semantics. Equipment Slot4 alone means localized Unarmed; real weapons named Unarmed stay literal.
  Shared BattleStatusCard translates only its suffixes; role callers outside this group retain their
  wording, and HP/MP node/resource IDs, values, gauge colors and geometry remain unchanged.
- Choice failures use codes for finite localized recovery, with Kind/Code/Field diagnostics. Unknown
  codes use general recovery rather than guessing their meaning. Global feedback additionally reads
  actual snapshot Unsupported/Faulted status; startup failure is explicitly stopped. A choice-level
  Unsupported can return to actions, while a stopped session directs the player to retain diagnostics
  and relaunch. Formatting does not promise an unavailable Back/retry operation or issue commands.
- Actual equipment HUD/current choice, ATT/range and ReadMore are localized. `_title`, `_roster` and
  non-equipment spell/item strings in observation are diagnostic projections, not another visible
  localized menu. The separate M2e boundary below covers Give/Discard/Retreat. Unapproved
  source terminology/dialogue and source-image lettering remain English; M2g below owns scene results.
  Language is chosen through existing settings before battle; no mid-battle settings are added.
- Missing/invalid rows use the complete English template and existing key/reason/effective-locale
  diagnostics. Native geometry checks cover long fallback, low/zero HP, zero MP, equipment and
  target navigation, including canceled/rejected/committed choices and stable idle/redraw state.
  These are bounded remake observations, not human translation approval or original-game evidence.

The [native reproduction owner](../verification/host.md#localized-battle-commands-targets-and-status)
records the finite input recipes, coherent retained private icon selection and remaining boundaries.

## Implemented M2e battle transactions and retreat boundary

**Standard rigor:** [#1480](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1480) extends
existing GameText/PO/font consumers through Give/exchange and Discard choices, confirmation/results,
auxiliary recipient/back hints and authored EGRESS/Angel Wing feedback. Acceptance uses direct
catalog/parameter checks, adapter compilation, bounded English/zh-CN native input/state/token/geometry
and error observations, then the clean committed planner and selected CI. No engine, Content, save,
schema, input remapping, message framework or additional gameplay capability is introduced.

- GiveState Giver/Recipient/Item/Returned and DiscardState Actor/Item select complete result templates.
  Selection Stage/Slot/Target/ReturnSlot and typed item metadata retain their authority. Full-pack
  exchange, exact duplicate slots, equipped/rare/protected explanations and explicit confirmation/
  cancel remain. English Label/Text matching never selects semantics. CharacterNames resolves names
  before formatting; valid source/custom names, including template-like literal characters, are
  arguments rather than translated keys or format strings.
- Give commits once before text; reveal and reading retain that inventory, and acknowledgement ends
  the action/advances. Discard commits once before text but acknowledgement retains the same actor,
  action and provisional movement. Selection/cancel and display redraw do not publish a transaction.
- RetreatPayment selects the complete spell or item template: existing Spell/level/MpCost versus
  item Name/word/slot. Canonical Text/TextFor remain unchanged. MP/item payment precedes feedback and
  happens once; Text → Effect → Close → Return retain distinct WaitTokens, reveal and completion
  commands, cue/pulse, reduced-flash and focus suspension. This remains an authored approximation,
  with no new original-game timing/audio or natural-reach claim.
- Result hints use the existing panel. Confirm completes reveal, then reads any remaining pages
  before a fresh acknowledgement; reading does not change revision, resources or the logical wait.
  Display length can alter visual reveal duration, never gameplay work/RNG or return semantics.
  Missing/invalid Chinese rows retain the complete English template with existing diagnostics.
  Language is selected before battle; no mid-result locale switch is added.

The [native reproduction owner](../verification/host.md#localized-battle-transactions-and-retreat)
owns the bounded scenarios, source/custom-name boundary and remaining English/Unknown surfaces.

## Implemented M2f story controls and return notices boundary

**Standard rigor:** [#1481](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1481)
localizes the existing exploration and battle-story reveal/ReadMore/continue/scene hints, eligible
battle-intro skip hints, Yes/No labels and ephemeral battle-return notices through GameText and both
PO catalogs. Settings retains a short partial-support notice; exact coverage lives here and in the
capability owner. Complete templates take existing action bindings; no English body or label selects a
semantic operation. Catalog fallback retains the whole English row and its key/reason diagnostics.

- ExplorationDialogueView changes only RefreshHint. Its FieldTextWait/Search projection, open-window
  identity, source ranges, retained question, visible characters and reading reset remain unchanged.
  In Chinese, an actually visible untranslated window keeps the existing English fallback reason.
  BattleStoryView likewise marks visible DialogueWait bodies `unmigrated-consumer`; camera-only waits
  get no body fallback notice. Definition.Texts, camera completion, wait tokens and acknowledgements
  are unchanged. Chinese controls do not imply translated dialogue or a Chinese opening route.
- StoryChoiceRow translates Text only. Node IDs Yes/No, boolean selection, authored focus and source
  Work/phase remain authoritative. The bundled face lacks the old triangle glyphs; supported `>`
  marks selected choices and Continue. Reveal and all ReadMore pages precede a fresh choice/confirm;
  pointer press alone cannot answer, and complete release/Cancel/right-click No keep existing meaning.
- FieldReturnFeedback retains Begin/Dismiss and the actual `outcome-program-started` Victory/Defeat
  or `retreat-exploration-entry` discriminator. Its finite enum selects six complete outcome/ready
  templates using CanWaitAtInput. It neither parses prose nor commits rewards, resources or outcomes.
  The notice remains during return, becomes ready at field control and clears on ordinary input.
- Language stays in the existing safe settings entry; no story/battle hot switch is added. No engine,
  scenario, save, schema, general message framework or input-controller change belongs to this group.
  Original dialogue bodies and source/default names remain outside this group; M2g below covers
  BattleScene display. The accepted field overlay retains its independent coverage and fallback rules.

The [finite native recipe](../verification/host.md#localized-story-controls-choices-and-return-notices)
owns actual bilingual state/input/glyph/geometry observations, retained outcome evidence and limits.

## Implemented M2g semantic battle feedback boundary

**Standard rigor:** [#1483](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1483)
extends the accepted semantic HEAL display approach through the existing BattleSceneView message,
acting/target cards and reading hints. Chinese uses project-authored functional templates from typed
Phase/ActionKind/ReactionKind/Critical/side/GrowthNotice and existing actor, target, amount, reward,
gold, receipt and SpellRef operands. These public PO rows are not translations of private script.
English preserves the existing source-ID selection and admitted source strings; a missing/invalid
Chinese row uses its complete authored English template, with the current key/reason visible in the
hint and retained in catalog diagnostics. An unmigrated English body is explicitly marked; blank
phases and unrelated catalog failures do not produce body-fallback notices.

- Physical first/second/counter, prism, item use, damage/critical/dodge/ineffective/recovery, side-specific
  death, item drop/receipt, EXP/gold and the seven growth kinds have finite mappings. Spell growth
  uses SpellRef.Level (also the notice Amount), while stat notices use their actual gain. SleepTurnText
  is the existing typed 306-asleep/354-awake discriminator, not parsed English; sleep and poison
  notices retain the existing automatic 0.5-second delivery. AURA reuses HEAL display keys.
- Name resolution and source/default/custom precedence stay with CharacterNames. Item/spell labels
  and resolved names are literal format arguments. English source-token substitution scans the
  source template once, so inserted `{N}`, `{ITEM}`, `{CLEAR}` or delay-like labels are not re-read.
  Canonical HEAL ActionText/RecoveryText and character work remain unchanged.
- Shared card roles reuse battle.acting/battle.target; reveal, ReadMore and Continue reuse existing
  keys, alongside bounded action-in-progress and timed-Wait hints. Same-token redraw, reveal/reading,
  acknowledgement, automatic delivery, animation/audio/focus and continuation algorithms are unchanged.
  No locale enters Application/Domain, rewards, resources, RNG, fairy opportunities or save identity.
- Ownership is BattleSceneView display only, GameText, the two PO catalogs and the four localization/
  presentation/capability/host owners. The verification guide adds only the authorized stable background
  execution preference. No battle.text overlay parser, interpreter or general framework is added.

The [bounded native recipe](../verification/host.md#localized-semantic-battle-feedback) owns direct
state/input/catalog/glyph/layout comparisons. Headless geometry is measured native layout, not a
physical-window/rendering or human readability claim. Original script/terminology translation,
source-image lettering, full-opening Chinese, Unicode naming and human prose/typography acceptance
remain separate. Wider reading comfort for automatic notices remains **Unknown**.

## Names, IME and compatibility

M1 retains the current seven-character ASCII rule and says so in translated help. Chinese display
is not Chinese naming. Keep existing custom names exactly, including case/spaces/punctuation; never
normalize, trim or translate them during load or locale switching.

For a later naming slice recommend seven **extended grapheme clusters** for new names, with an
independent generous UTF-8 bound (proposed 128 bytes), allowing the old alphabet plus supported Han
letters and ordinary letter/combining sequences. Reject unpaired surrogates, newline/control/bidi
formatting characters and source-token delimiters. Reject all-whitespace new input while continuing
to read every formerly valid name unchanged. Empty input remains default. This character/font policy
needs an input pilot before becoming a rule; validate committed text, not transient IME preedit.

Godot 4.7 [LineEdit](https://docs.godotengine.org/en/4.7/classes/class_lineedit.html) exposes IME
presence/apply/cancel methods. Let LineEdit/OS handle composition/candidates: committing Enter must
not also finish naming; first Escape cancels composition, later fresh Escape returns to setup.
Observe candidate mouse selection, Use name during composition, focus loss, paste and Cancel on the
installed Windows IME. Current NameEntryView intercepts Escape and calls Finish from TextSubmitted;
API availability does not establish correct event order. A raw key filter or UTF-16 length property
is insufficient. Use .NET text-element enumeration, not a custom segmentation algorithm.

Unicode names would fail source-font expansion and change HEAL character work. Before enabling
them, separate canonical name expansion from display override. Recommended policy: preserve current
canonical work for every formerly valid name; for a new Unicode name use the admitted source default
only for logical glyph work and display the custom name verbatim. This intentional modern policy
requires High-boundary review and focused tests, not an original timing claim. Relaxing NewGameName
alone is insufficient.

New code should read old version1 saves, including absent Names, without migration. Unicode fits
the current JSON string shape, but old code rejects it via `save-name`. Before writing such saves,
the naming slice must choose an explicit compatibility marker/version under the save owner so
unsupported downgrade is explained, never lossy. Prefer an additive reader preserving original
slot/backup bytes; a required version migration uses a detached validated candidate and existing
write recovery, not rewrite-on-read. M0/M1 change no save schema. Naming policy, not display locale,
is the potential migration trigger.

## Selected source-line display policy

The [#1583 presentation agreement](../godot/presentation.md#source-line-reflow-1583) owns modern
reflow for ordinary exploration and the known entry-v1 projection. `DisplayCatalog`'s optional
`reflow` argument changes only rendered text: default canonical/translated resolution, source-token
mapping, admission and captured name/class bindings remain intact. Individual typed `{N}` between
text operands can become separators; consecutive/control-adjacent breaks and literal paragraphs
remain explicit. Separator choice reads resolved operands rather than placeholder spellings.

Entry source/translated rows retain their exact stored LF and default resolver output. Only
`EntryPresentationAssets` applies the legacy entry-format layout policy after resolution. No
translation row, private corpus, control mapping, source unit, schema or save identity changes.
Display/reveal character totals follow the actual shaped projection; they never replace source
logical character work. W1's existing English fallback is not additional Chinese coverage. Search,
battle and other unselected consumers retain their current layout. The finite unit/native checks
are linked from the presentation agreement. The full-corpus review records every row's disposition;
semantic judgments remain **Inferred**, with three ambiguous rows and human typography acceptance
still **Unknown**. The21 admitted semantic-preservation candidates are existing Search descriptions
whose default projection already preserves their breaks.

## Font, layout and delivery

M1 bundles Noto Sans CJK SC Regular through StoryTheme for Simplified Chinese and the required
Latin/symbol coverage. Upstream
[Sans documentation](https://github.com/notofonts/noto-cjk/blob/main/Sans/README.md) describes the
family and the [license](https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE) is SIL OFL 1.1.
The [bundled asset owner](../../game/fonts/localization/README.md) records the exact revision, file,
version and notices. M1 distributes the unmodified face; future subsets must separately review OFL
modification/reserved-name conditions before replacing it.

Godot documents [Font fallbacks](https://docs.godotengine.org/en/4.7/classes/class_font.html),
[Theme/default font configuration](https://docs.godotengine.org/en/4.7/tutorials/ui/gui_using_fonts.html)
and [system fallback import behavior](https://docs.godotengine.org/en/4.7/classes/class_resourceimporterdynamicfont.html).
Release acceptance must not rely on development-machine system fonts. Reuse an existing suitable
asset pack or explicitly own a public licensed font directory with provenance. Do not copy Windows
fonts or nest the private asset repository in a worktree.

Start with existing typography hierarchy/30px/24px dialogue defaults. Measure CJK ascent/descent,
line spacing, punctuation/no-space wrapping, opening/closing marks, Latin digits, longest labels and
seven-cluster mixed names. Keep WordSmart/ReadMore; visual lines do not add source pages/acks.
Do not shrink every label or crop required choices. Test missing glyphs explicitly. The
[Label API](https://docs.godotengine.org/en/4.7/classes/class_label.html) supplies plain text,
wrapping/language and visible-character controls; actual mixed-script reveal and the host Rune-based
speech loop have bounded M1 observations above. UTF-16 units, code points, grapheme clusters and
shaped glyphs differ; broader naming/typography still needs its own acceptance.

Preserve field alternating nonspace speech with instant/reveal-all silent; do not replay it on
refresh. No new battle speech bleeps. Chinese pacing is a human-readability question, not a reason
to change simulation speed. Title/numeral raster lettering requires a separate licensed/authored
asset decision; retain it and identify the untranslated gap until then. A catalog grants no art rights.

## Implementation sequence and acceptance

The M1 paths below are implemented only at the bounded pilot boundary above. Later phases remain
finite candidates, not an implementation queue. Paths are repository
relative: `A` = `remake/src/Sf2.Remake.Application`, `C` = `remake/src/Sf2.Remake.Content`,
`G` = `remake/game`, `T` = `remake/tests/Sf2.Remake.Engine.Tests`. M1 owns the implemented paths
below; future dispatch must recheck names and single-writer ownership, especially shared
settings/catalogs/reader/theme files, and update closest capability/verification owners.

### M1 integrated sample

| Responsibility | M1 implementation paths |
| --- | --- |
| Selection, lookup, settings | New `G/src/Localization/GameText.cs`, `G/localization/en.po`, `G/localization/zh_CN.po`; existing `G/project.godot`, `G/src/GameRoot.cs`, `G/src/Input/InputSettings.cs`, `G/src/Entry/SettingsHost.cs`, `G/src/UI/SettingsView.cs`, `G/src/Entry/SessionEntryView.cs` (settings access refresh only) |
| Overlay and token reuse | New `C/Localization/DisplayCatalog.cs`, `C/Localization/DisplayCatalogReader.cs`, `C/Scenarios/ExplorationTextTokens.cs`; existing `C/Scenarios/ExplorationContentReader.cs`; new original `remake/content/authored/localization-pilot.json` and `remake/content/authored/localization-pilot.zh-CN.json` |
| Field display | `A/Runtime/Exploration/ExplorationState.cs`, `A/Runtime/Exploration/ExplorationTextRunner.cs`, `G/src/Exploration/ExplorationDialogueView.cs`, `G/src/Exploration/ExplorationSessionView.cs` |
| HEAL display | `A/Runtime/Battles/BattleSceneContinuation.cs`, `G/src/Battles/BattleSceneView.cs`, `G/src/Battles/BattleCharacterNames.cs` |
| Font and focused acceptance | `G/src/UI/StoryTheme.cs`; new `G/fonts/localization/NotoSansCJKsc-Regular.otf`, `G/fonts/localization/LICENSE.txt`, `G/fonts/localization/README.md`; existing `T/ExplorationTextWaitTests.cs`, `T/BattleSceneTests.cs`; new `T/LocalizationDisplayTests.cs` only for actual Application/Domain behavior; new `G/probes/localization_observation.gd` only if existing observations cannot drive the necessary cases |

Dependencies: accepted M0, pinned licensed font, existing settings and field/HEAL delivery owners.
Exercise one original authored miniature through FieldTextWait with name/W1/W2/CLEAR and an ordinary
HEAL result. Add changed-content D1/N, empty span, different actor/amount and custom ASCII name
cases. A translated static Label demo is insufficient. A small private-local check may bind one
already admitted row after metadata inspection; no whole-world rebuild or opening replay is needed.

Acceptance: English/zh-CN Apply/Cancel/read-only/failure retention; identical live identity/revision/
state/seeds across Apply; all visual pages before actual delivery; stale/duplicate completion still
rejected. Changed translation length must preserve canonical units/HEAL work and semantic outcomes
for the same logical command sequence. Observe fast/slow reading without additional simulation debt
or fairy RNG. Cover missing row, wrong parameters/controls/identity and missing font; expose effective
locale/fallback. Unit assertions cover actual Application/Domain behavior: captured display operands,
unchanged logical work/state/RNG and stale/duplicate completion semantics. Exercise overlay reading,
parameter/control validation and fallback directly with bounded input variants; do not add unit
tests of those validators, the probe or other verification programs. Run affected
`uv run sf2 verify engine`, `uv run sf2 verify adapter`, and bounded actual Godot
state/input/geometry/errors, without screenshots. Restart only for necessary changed-code/font
import/startup acceptance.

Stop with a reviewable pilot PR, font/import measurements and remaining English consumers explicit.
If preserving logical work requires a new interpreter/timing policy, private rights are unestablished,
or font/reveal cannot be demonstrated, stop expansion and retain English/slots/original resources.
M1 does not enable Unicode naming or close the epic.

### Next bounded candidates

| Candidate / dependencies | Initial exact ownership | Acceptance, failure and stop |
| --- | --- | --- |
| Implemented M2a Church/Search feedback after M1 | `A/Runtime/Exploration/FieldService.cs`, `A/Runtime/Exploration/FieldSearch.cs`, `G/src/Exploration/ChurchServiceView.cs`, `G/src/Exploration/ExplorationDialogueView.cs`, `G/src/Localization/GameText.cs`, `G/localization/en.po`, `G/localization/zh_CN.po`, `T/FieldSearchTests.cs`, `T/FieldServiceTests.cs` | Finite result values replace concatenation/string-phase coupling. Success/decline/insufficient/full-pack cases preserve payment, party/status/receipt order and confirmation. Actual behavior/adapter checks; display failure retains committed rewards. Stop after these consumers. |
| Implemented M2b Entry/slots/naming help after M1 | `G/src/Entry/SessionEntryView.cs`, `NameEntryView.cs`, `SaveSlotView.cs`, `OpeningPersistenceHost.cs`, `NewGameSetupHost.cs`, `EntryPresentationAssets.cs`; `SettingsHost.cs` and `G/src/GameRoot.cs` existing label refresh only; `G/src/Localization/GameText.cs`, both PO paths; `C/Localization/DisplayCatalogReader.cs`, `DisplayCatalog.cs`, `C/Scenarios/EntryPresentationReader.cs` | Stable action/node identity, actual entry identity for six dialogue bindings, readable errors/slots, source/UI fallback and long menu reading. Old names/saves unchanged; direct entry/Continue observations without refresh-triggered audio restart or schema/image replacement. |
| M3 bounded opening terminology/dialogue after admitted consumers and rights/review | Public owner `remake/docs/application/localization-design.md` only if coverage/boundary changes; private catalog under a separately declared non-worktree content root. Dispatch must name those actual private files before writing | Start with reached IDs/names in one playable segment. Direct source/control/parameter comparison plus bilingual review. No runtime/export edits by translators. Drift/ambiguity stops affected rows; fallback stays explicitly untranslated. Historical opening pilot; current full-text agreement above separately governs static drafting. |
| M4 Chinese naming/IME after M1 and accepted High compatibility policy | `A/Runtime/NewGameOptions.cs`, `A/Runtime/OpeningNewGame.cs`, `A/Runtime/CharacterNames.cs`, `A/Runtime/Exploration/ExplorationTextRunner.cs`, `A/Runtime/Battles/BattleSceneContinuation.cs`, `A/Runtime/Persistence/OpeningSaveData.cs`, `A/Runtime/Persistence/OpeningSave.cs`, `A/Runtime/Persistence/OpeningRestore.cs`, `C/Persistence/OpeningSaveJson.cs`, `G/src/Entry/NameEntryView.cs`, `T/OpeningSaveTests.cs`, `T/ExplorationTextWaitTests.cs`, `T/BattleSceneTests.cs` | Actual composition/commit/cancel/paste/focus, old name/save preservation and Unicode independent-process Save/Continue. Refuse enablement with unresolved canonical work/backward reading/downgrade; no lossy conversion. Stop at named admitted characters/consumers. |

Party/Prepare has the bounded M2c consumer agreement above. M2d battle commands/targets/equipment/status have their separate boundary above. M2e Give/Discard/retreat has its separate boundary above. M2f story controls/choices/return notices have their separate boundary above. Further battle screens are separate
consumer groups, not implicit all-files ownership.
M5 is the epic's opening-route and bilingual human acceptance, not another framework. No candidate
authorizes production edits, new tickets or dispatch during M0.

### Resources and Unknowns

M0 reads named code/readers rather than dumping the approximately 127.44-million-byte private world
noted in PrivateExplorationReader. No package copy, font download, process or cache is created.
Local check/handoff output stays below 5 MiB; no private prose is necessary for these findings.

For M1 budget 64 overlay entries and roughly 32 UI/templates per locale: at an assumed 1 KiB average
entry, two language views are about 192 KiB before object overhead. These are pilot limits, not an
inventory measurement. Admission is O(catalog bytes + tokens), resident text O(catalog bytes),
dictionary lookup expected O(1), formatting O(template + arguments + output). Keep two small locale
resources and no per-frame I/O or duplicate full-world allocation.

Font/export/import size, first-load allocation, atlas growth and reading quality are **Unknown**.
Measure the pinned single face in necessary M1 acceptance before adding weights/subsets/platforms.
Record bytes, peak memory and import duration without a benchmark program. Atlas planning can use
`resident glyphs × raster width × raster height × bytes per pixel` plus packing/engine overhead;
font file size is not resident memory. Replan if import exceeds available worktree budget, overlay
bounds are exceeded or required characters lack coverage. No precise runtime-admission or human-reading estimate
or completion date is supported yet.

For a new runtime admission count its proposed admitted/reached IDs, unique source words/characters, token-bearing
rows and names; sample a dialogue/result with human review and estimate review effort separately
from translation throughput. The historical pilot did not authorize API/model expenditure; current static
full-text processing follows the bounded agreement above. Rights/terminology
and quality are not established by placeholder checks.

Remaining boundaries: human typography/readability and translation approval, terminology/default-name
runtime admission, private translated-content publication, other dialogue/choice consumers, Windows IME
event ordering and a future Unicode save downgrade marker. The bounded installed-font, source-range,
HEAL-invariance and mixed-script observations are described above; they do not establish broader
content coverage or naming support.

## Verification ownership

Review the three real consumers, dependency direction, settings transaction, canonical-work
separation and save identity. Check links/anchors, examples/fences/tables, exact scope and absence
of private content. On a clean committed candidate load the owning ignored configuration and run
`uv run sf2 verify plan --base origin/main --head HEAD`; interpret it under the
[current verification scope](../development-and-verification.md#scope). Record exact head/tree/base,
direct checks and actual CI in the handoff/PR, not a growing history here. M0 did not claim runtime observations. M1 uses actual engine behavior tests, adapter compilation
and directly executed bounded native/catalog variants; no tests of validators or probes are added.
Main-gate owns independent integration.
