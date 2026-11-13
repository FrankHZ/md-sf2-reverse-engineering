# Pre-voyage Chinese admission plan

The current selector includes later campaign dialogue, Search and the ordinary service greetings.
The [western shrine road](hassan-taros-road.md) selects 1,469 world messages and 126 names, including Battle16 lines 2513–2519, the Hassan cast, the six Battle17 species and shrine lines 3197–3203. Church 110 preserves both W2 controls and Shop 158 preserves its terminal D3.
[Service entry and bilingual reading](opening-session-lifecycle.md#church-and-shop-entry-greetings).
Use `--scope pre-voyage` to reproduce the 806-message selection below.

## Decision and acceptance boundary

**Accepted design from [Issue1505](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1505).**
Use one private world catalog, the existing separate entry namespace, and offline derivation from
the accepted sole message/name corpora. The bounded candidate is **806 world messages**, the existing
**six entry aliases**, **51 display-name identities**, and the already localized semantic UI.
Raise the world message ceiling to **1,024**, retain **256 KiB per file / 8,192 UTF-16 units per
message**, and admit at most **128 optional name rows / 128 UTF-16 units per name**. No new native
text token, chapter loader, cache, database or localization framework is needed for this candidate.
The runtime reader now implements these bounds; entry retains its existing 256-row ceiling.

**Implemented identity prerequisite ([Issue1601](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1601)):**
field display bindings capture optional `ClassId` alongside the member/actor and canonical class
text; custom names remain separate and never replace a class. Party inventory exposes nullable
`ItemId` (null for empty or unknown slots). Give captures `GivenItemId`/`ReturnedItemId` and Discard
captures `ItemId` from the committed transaction. Each scene carries optional `EnemyNameIds`, keyed
by ActorRef within its own encounter, derived directly from the admitted enemy table and placement.
All additions are display metadata with old-constructor defaults; scene JSON, canonical strings,
source text work, gameplay and saves retain their existing contracts. Catalog/name resolution,
whole-row canonical/custom separation and view integration are implemented by the finite catalog
consumer below. The identity prerequisite alone did not expand Chinese runtime coverage.

**Standard rigor:** this investigation fixes a finite selection, display interfaces and delivery
sequence. The existing [High save boundary](opening-session-lifecycle.md#format-compatibility-and-local-storage)
remains binding. No locale, translated text or display-name choice enters canonical execution,
source glyph work, RNG, gameplay revision, ActorId or save schema. No translation is edited here.
The [localization owner](localization-design.md) retains corpus authority and existing consumers;
this document owns the implemented pre-voyage admission and its remaining acceptance boundaries.

The product denominator is the accepted [campaign](pre-voyage-campaign.md): ordinary title/New Game,
Map3, Battles01–07 and their supported field returns/services, tower aftermath, departure/voyage,
and the **first stable Map14 control**. Stop there. Plank choices, later Map14 init, Battle08,
town construction, unimplemented shops/services and later chapters remain outside scope.
The 444-row editorial Grans batch and the 4,267-row `world.texts` table are neither this denominator
nor proof that any particular original-game path naturally executes.

## Implemented candidate and observed consumers

**Confirmed:** the #1602 candidate derives the exact806/6/51 selection from the sole corpora into
fresh ignored files. The world catalog is111,580 UTF-8 bytes; the entry catalog is1,042 bytes.
All selected source strings match both the admitted world and pinned source; control order and
per-segment operand identity pass. The34 historical differences have a private old/new/reason
review: only the already accepted name/class/title calibrations and adjacent spacing change.
The old209 catalog, six approved aliases, source corpora and gameplay world remain unchanged.
This is a worker-verified candidate; independent main-gate integration remains separate.

The runtime catalog resolves captured member/class IDs and optional name rows, preserving explicit
custom values even when equal to canonical English. Search/legacy whole-row bindings now retain
canonical and custom separately. Typed item words, SpellRef/BaseId and encounter-scoped enemy IDs
feed the existing semantic views, party/class details, transaction results, battle field markers,
cards and entry/slot/default-name labels. Empty127 remains the existing localized UI label.
Names are checked against admitted source tables and actual consumer bindings, including Search
placements whose identities need not appear in an encounter's static item subset.

Ordinary New Game and Continue bind the catalog from their already admitted definition before
building name/slot labels, then the normal session mount rebinds it. Settings/observation expose
admitted message/name counts, separate entry status and exact name fallback identities; these
counts describe admission, not natural reach. Missing/invalid names remain canonical with explicit
name diagnostics. Authored labels without pinned source-name identity remain their declared names.

The bounded native state/input/projection checks cover ordinary title and all six aliases,
Settings Apply/Cancel and a subsequent ordinary process, New Game default hints, and Continue's
translated default versus an explicit canonical-equal custom value. Actual Talk displays calibrated480;
the source zone/Jaha chain displays new662/663 and join446, including the complete longest selected
row663 in its actual panel. Kiwi's real choice/no/cancel/name/join chain displays captured NAME/CLASS
and a seven-character wide custom value. The real Search4065 consumer preserves its semantic breaks;
Party and Galam Shop display their typed member/class/item values. Private battle observations
cover the enemy target card/field names, committed item-use message and HEAL actor/spell message.
No selected body/name fallback remains unexplained at these observed seams. Actual longest/wide
panels fit at1080p with their real reading geometry; this does not establish human typography taste.

Standalone authored Give/Discard observations retain canonical project-authored labels outside the
selected private name table. The [connected Give extension](../capability-status.md#connected-battle-give-and-exchange)
now reuses committed item/member IDs and the existing Chinese templates/catalog in Battles01–07.
Actual Map57→Battle01 input observes normal receipt, equipped full-pack exchange and next control;
this is a controlled connected seam, not natural original reach. Connected battle Discard remains Unsupported.

Controlled field placements and an explicitly initialized private battle input prove the actual
consumers, not preceding natural reach. The probe-only startup uses production New Game admission,
then production session-start validation before ordinary mount. It never edits a running session,
injects an overlay, clones the world or relaxes the legacy two-encounter controlled-start reader.
Its battle preparation first obtains real initialized operands through production initialization;
controlled accounting zeros/HP/positions are labeled test inputs, never original-state evidence.
Language Apply preserves the paused session's exact revision/tick/source work/flags/resources/RNG;
ordinary language changes leave both disposable save files byte-identical. Default language remains
English until an ordinary persisted Apply. See the [reproduction owner](../development-and-verification.md#pre-voyage-chinese-consumer-observation).

Natural reach of every optional branch, later chapters, legacy/authored unmigrated text, image
lettering, IME and human Chinese reading remain outside this bounded technical result. Existing
ObjectDB exit leak warnings remain **Unknown**; successful consumer checks do not establish clean
native teardown. Exact heads, commands, controlled inputs, failures and CI belong in the frozen handoff.

## Selection evidence and exact IDs

**Confirmed:** the accepted code/tree at `9e938fdda5647c5319cace57b4c49f44b32fa2bb`,
[`pre-voyage-opening.json`](../../content/private/pre-voyage-opening.json), the ordinary selected
revision4 world, and the pinned source identify the following finite static selection. Source is
`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`, USA ROM
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Every one of the selected world's 4,267 source rows directly equals the sole corpus's pinned source.
Message IDs below are decimal; `gamescript.txt` keys are hexadecimal.

The inventory starts from selected map/setup on-load, input and event locations, and all seven
selected battle before/start/load/outcome locations. It follows `call` with its return stack,
`jump`, both flag/coordinate branches and both choice returns; `text-cursor` sets the cursor,
implicit `show-text` increments it, and explicit text operands leave it unchanged. Native Unsupported
instructions stop that path. Search adds selected description IDs and the actual `FieldSearch`
marker/empty-result selectors. This gives 399 root locations, 825 stage/program/instruction/text
bindings and 806 unique IDs, with no unresolved text cursor or missing program at the traversed seam.

Two product constraints narrow this traversal: Map14 contributes no ordinary init/event roots,
because Map73's scene load reaches its first control without invoking that init; Map73 contributes
only first-arrival init with F700 clear. Default Map40 has no NPC population, so its interaction
table is not an opening dialogue root; the selected F506 population owns its later IDs997–1000.
The latter is directly supported by pinned `map40/mapsetups/s1_entities{,_506}.asm` and
`s2_entityevents.asm`. Map73's `s6_initfunction.asm` owns the F700 branch.

**Inferred:** this union is a conservative admission envelope for the selected consumers. Exploring
both conditional edges does not solve global flag feasibility, spatial approach or natural reach.
Some IDs occur in multiple stages; stage counts must not be added. A message is admitted once.
No result here promotes static source structure to a natural-run or original-hardware claim.

| Stage and selected roots | Unique program-message IDs in the stage | Count |
| --- | --- | ---: |
| Opening: Maps3/19/20/21 default, castle F501, Map40 on-load and Battle01/Map57 | 363, 447, 480–505, 507–573, 575–580, 583–586, 588–596, 2176–2212, 2292–2310 | 170 |
| A: Map66 including Battles02/03/04, Yeel/Map5, Hawel/Map43 default/F612, Map17 default and capture/prison programs | 363, 446, 760–789, 810–842, 880–961, 2311–2322, 2342–2356 | 174 |
| B: Map17 F505, Map16 default/F663 including Battle05, Map18 | 363, 446, 835–842, 844–878, 880–900, 945, 958–996, 2323–2341 | 125 |
| C: selected F609/F506 town/castle/docks, Map40 F506, Maps60/58 and Battle07; Battle06 is dispatched from Map66 in A | 363, 446, 545–547, 566, 581, 598–617, 620–624, 626–651, 657–712, 721–759, 997–1000, 2357–2460 | 261 |
| D: Map19 F507, Map4, Map44 default/F507, Map73 first arrival through Map14 scene load | 625, 713–720, 810, 1001–1020, 2213–2254 | 72 |
| Shared Search: selected Maps3/19/20/21/57/43 and their selected variants | 403–405, 408–410, 412, 423, 425–430, 4035–4059, 4065–4073 | 48 |

The program union is 758 IDs; Search adds 48 distinct IDs. The exact **806-ID union** is:

```text
363, 403–405, 408–410, 412, 423, 425–430, 446–447,
480–505, 507–573, 575–581, 583–586, 588–596, 598–617,
620–651, 657–789, 810–842, 844–878, 880–1020,
2176–2254, 2292–2460, 4035–4059, 4065–4073
```

The 23 stopped root paths reduce to 14 distinct Unsupported program locations: two Map3 shop
entries; the markerless Map21 default-zone body; four unselected Map66 warp exits and its two
raw zone `move.l` frontiers; two Yeel warp exits; Map60's switched-58 and warp-22 frontiers; and
the raw Map44 default-zone displacement. These are existing gameplay frontiers, not missing Chinese
rows. Do not traverse beyond them, translate a fabricated result, or silently make them executable.
The full post-frontier text/operand set is **Unknown** and excluded. Existing localized failure UI
must remain usable. Source-zone marker feasibility and every conditional NPC approach are not fully
enumerated by this design; they are explicitly outside the claim of natural execution.

### Consumer classification

| Class | Concrete scope and disposition |
| --- | --- |
| Historical admitted seed | All 209 historical world IDs are in the current 806 union:170 opening program IDs including defeat363, plus39 Search rows. Six `entry.text` aliases216–221 remain separately bound to the entry profile. The historical artifacts remain unchanged. |
| Additional admitted world rows | The additional 597 world rows comprise 588 program IDs and nine Search descriptions4065–4073. The current candidate admits all 806 under the existing token grammar; representative actual caller projections are recorded above. Admission does not prove every conditional path naturally executes. |
| Existing semantic UI; no per-source-row overlay | Church menus/results, Search item/gold/full-pack result, Shop buy/sell, Party/Prepare, entry/slots/settings, battle command/target/equipment/status, Give/Discard/paid retreat, story choices/return notices and semantic battle feedback reuse typed values and public PO keys. Selected source-name identities resolve through the 51-key table. Connected Give now consumes those IDs; authored Give/Discard labels stay canonical and connected battle Discard remains Unsupported. |
| Implemented operand/display projection | Join446 captures member/custom-name and class identities. Semantic views resolve selected item/enemy/class identities at the named seams while retaining canonical fallback and explicit custom values. These use the 51-key table below without new native tokens; unselected or identity-free authored labels retain their declared values. |
| Not a Chinese admission claim | `W1TextWait`'s legacy English path, authored `BattleStoryView` bodies and raw unmigrated battle fallback remain explicit. In this selected program inventory only 363 lacks `explicitWindows`; it uses the connected `DialogueWait` path. The ordinary configured field context selects `FieldTextWait` for the other 757 program IDs. Alternative authored/legacy contexts do not become Chinese merely because an ID is in the catalog. |
| Outside the product | Unselected setup variants, post-frontier bodies, later Map14/Map73 events, Battle08+, future services and the rest of the4,267-row corpus. The nine corpus non-language rows are not selected world messages. Image lettering and Chinese input/IME are separate capabilities. |

The template boundary is established by `GameText.Search`, `ShopServiceView`, `ChurchServiceView`
and `BattleSceneView.SemanticMessage`. In particular, battle source IDs244/263/266–271/273–275,
284–293/298,306–308/319/338/354/393/395–396 and additional supported status/spell phases do not
need an exploration overlay to translate their existing semantic messages. Those numeric examples
identify the source fallback; the complete consumer denominator is the implemented phase/action/
reaction/growth switch, not a guessed source-ID range. Unsupported abilities remain unavailable.

## Operands and display-name identity

**Confirmed:** the 806 rows use only `{N}`, `{W1}`, `{W2}`, `{D1}`, `{CLEAR}`, `{LEADER}`,
`{NAME}`, `{NAME;n}` and `{CLASS}`. They contain zero `{ITEM}`, `{SPELL}`, `{#}`, `{DICT}`,
`{D2}` or `{D3}`. There are 11 bare-NAME rows: ten Search prefixes403–405/423/425–430,
and join446. Only 446 contains CLASS. Fixed NAME indices are1–6/25/28; LEADER is separately
bound by its consumer. Full-corpus 4,010-compatible/257-unsupported counts are not stage gap counts.

| Operand | Existing production authority | Required display behavior |
| --- | --- | --- |
| Field LEADER/fixed NAME | `ExplorationTextRunner.Begin` captures source-token-indexed `DisplayNames`; LEADER rebuilds active membership and selects its actual first member | Translate the captured member identity, not the speaker or assumed member0. Preserve `CustomName` first. Do not recompute the live party during display. |
| Bare NAME/CLASS in446 | `ShowText.NameMember`: `cs_516A8`→3, `cs_512BA`→6, `cs_540C0`→4, `abcs_battle05`→5. `JoinedClass` reads the generated member's validated `Growth.ClassId` and private class table | Retain that class ID in the display binding alongside its existing canonical string. Translate by `name.class/index`; never infer class from English abbreviation, actor sprite or prose. |
| Search bare NAME | `FieldSearch` uses the controlled leader; `GameText.WholeRow(..., implicitMember:0)` constructs the binding. Search reward recipient is separately typed in `FieldSearchResult` | Whole-row projection now keeps canonical and explicit custom values separate, preserving these distinct identities and custom-name precedence when resolving translated defaults. |
| Defeat363 | Existing legacy `DialogueWait` whole-row LEADER/member0 contract | Retain its existing identity contract and explicit custom value. Do not reuse that legacy shortcut for field LEADER. |
| Item/spell/number in implemented services/battles | Item word/ID, SpellRef, amounts, phases, reward/receipt and payment values already belong to semantic results | Format translated display labels and existing numeric values in PO templates. Keep source fallback/canonical HEAL strings intact. No ITEM/SPELL/# exploration-parser expansion. |
| DICT/D2/D3 and future generic contexts | No selected world row requires them | Defer until a real admitted gameplay caller supplies dictionary/delay/parameter semantics. A translated draft is not that caller. |

The runtime name table is a **display-only projection** of `names.zh-CN.jsonl`; namespace plus table
index is identity. Duplicate visible names stay distinct. The finite 51-key selection is:

| Namespace | Indices | Count / reason |
| --- | --- | --- |
| `name.ally` | 0–6, 25, 28 | 9: seven generated members plus fixed story operands; no recruitment of25/28 is implied |
| `name.class` | 0–4, 10–11 | 7: the seven admitted growth classes; no promotion feature |
| `name.item` | 0–4, 33, 45, 56–57, 71–73, 85–86, 103 | 15: initial seven-member loadouts, selected Search rewards, Galam's two shop stocks and selected enemies' equipment/placement items; enemy equipment need not be obtainable |
| `name.spell` | 0, 2, 4, 6–8, 10–11, 13–14 | 10: all learned spell identities in the seven selected growth tables plus Battle01–07 enemies; includes displayed unavailable choices after legal grinding, not just one observed level |
| `name.enemy` | 0–3, 39–40, 56, 67, 76, 86 | 10: the seven selected encounter placement identities |

Item-triggered HEALIN is an effect identity, not a new spell-name display requirement: current item
messages display the item, while the effect owns its existing behavior. Empty slots use the existing
localized UI label, not item127's technical source text. All 337 names are available as a size
comparison, but blanket runtime admission is unnecessary. The source-marked technical anomaly and
unselected names are not a reason to invent a translation or gameplay consumer.

Use this precedence everywhere: **explicit saved custom value → selected localized table value →
existing canonical/source display fallback with a visible reason**. Even a custom value equal to the
source default remains custom. `NewGameOptions` and `NameMember` already use empty input to retain
or restore the default by omitting/removing the override. Do not write Chinese defaults into Names,
compare strings to guess whether a name was customized, normalize existing names, or enable IME.
Naming controls keep their current admitted alphabet/length; show the translated default as a
display hint, leaving the actual editable/custom value literal. Slot metadata must follow the same
precedence; missing old metadata gets canonical fallback, never a save rewrite.

### Implemented identity interfaces

Application remains locale-free. `DisplayCatalog`/`GameText` own localized labels. Keep existing
canonical strings as fallbacks. The implemented additions retain identities previously dropped
before presentation:

- `DisplayNameBinding` retains source-token index, member, actor, canonical/custom values and
  optional class identity captured by `ExplorationTextRunner`. Resolver layout boundary sampling
  and emitted text must use the same display choice.
- `PartyInventorySlot` exposes its committed item ID from
  `PartyDetailsQuery`; class identity is available from `Deployment.Definition.Growth.ClassId`,
  SpellRef resolves through `PrivateDefinitions.Spells[ref].BaseId`. No text parsing is needed.
- `BattleGiveState` and `BattleDiscardState` retain the given/returned/removed item IDs from `BattleGiveResult`/`BattleDiscardResult` at Begin.
  Looking at the post-transaction slot cannot recover the item reliably. Their canonical `Text`
  properties and command/resource semantics remain unchanged.
- `BattleSceneContentReader` maps each encounter's placement to `EnemyNames[ActorRef]` and retains
  the parallel source enemy table ID in `BattleSceneDefinition`. Scope by encounter: `enemy-0` is reused by different battles. Never map
  enemy English text or ActorRef spelling back to a table row. Old/authored scene definitions can
  omit this optional display map and use visible fallback.
- Reuse Search `ItemWord`, Shop offer/stock ID, field command expected word, equipment selections,
  `HealingDisplay` actor/SpellRef, battle item/receipt IDs and typed retreat payment. Resolve labels
  at the view boundary, including selection lists and completed results. A complete sentence with
  an English item argument is still an explicit name-coverage gap.

This prevents concrete identity loss at three existing query/result seams and the enemy scene map.
It does not require a new cross-engine localization port, canonical parameter interpreter or save
migration. Any newly discovered identity loss must be resolved at its real producer and declared in
the implementation diff; it cannot be filled by a story-ID special case.

## Catalog, line policy and source updates

The implemented reader retains the `package`, `locale`, `source` and `texts` shape and `world.text`
namespace, with an optional `names` array of `{namespace,index,text}` in that **same world catalog**.
Its source pin is the same admitted repository/commit/ROM identity; validate name namespaces/indices
against actual source tables and consumer bindings. Names must be plain text, with no control braces/newlines.
No name index is an ActorId. Unknown rows fail with diagnostics; duplicate identities and malformed
envelopes reject the candidate. Optional names preserve old-overlay readability in the new reader.
Entry continues to use its own existing profile, source identity and `entry.text` reader; do not
merge world and entry pins just because both use numeric message IDs.

Offline selection reads the sole corpora, this exact ID/key selection and the selected source
identity, checks source/control/parameter topology, then writes a fresh private candidate. Sort IDs
and name keys for a reviewable diff. The maintained selector is
`src/sf2tool/remake_localization.py`, reusing existing JSON/source/token helpers; do not modify the
frozen PowerShell compatibility surface or introduce a runtime manifest. Its selection arguments include
`--scope pre-voyage --world <selected-world> --corpus <source.jsonl> --names <names.zh-CN.jsonl> --output <fresh-directory>`.
Entry derivation additionally takes the admitted entry profile for its existing six aliases.
Inputs are read-only; output must be ignored and outside the corpus authority. The delivered module entry and required explicit input paths are documented in the
[verification owner](../development-and-verification.md#pre-voyage-chinese-consumer-observation).

The [accepted layout report](../evidence/dialogue-corpus-layout.md) identifies 34 historical world
targets that differ from the later calibrated corpus. Produce a private old/new/reason review for
exactly those 34 and derive the new 806 from the current authority. Keep the old 209 catalog and
accepted-seed mirror unchanged as historical evidence and rollback inputs. Review must reject an
unexpected additional change; it must not copy old wording back into the sole corpus. Entry216–221
keep their approved wording and the existing newline/leading-CLEAR/terminal-W2 projection exactly.
Custom-name values are runtime operands, never part of corpus synchronization.

Of the full-corpus 107 semantic-break candidates, **29** are in this selection, all Search:
the 21 already admitted plus 4065–4072. The current
[Search layout policy](../evidence/dialogue-corpus-layout.md#current-search-consumer-layout-1608)
preserves their reviewed source-token breaks while reflowing ordinary layout breaks in this finite
selection, including the mixed4041 row. N1 is retained in all29;4043/4047/4048 also retain N2.
Complete description/prefix rows and typed rewards remain separated. The other78 candidates and
all three ambiguities330/3207/3529 are outside this selection. Field prose keeps the existing source-token-aware reflow,
and entry keeps `SourceTextLayout.EntryLines`. Search uses the existing Content/display source-token
mapping with the bounded `SourceTextLayout.SearchBreaks` metadata; older catalogs remain readable
without a new envelope or control grammar. If a future field consumer needs mixed semantic/soft
breaks, that policy belongs in content/display metadata keyed to source token positions, validated by Content; never an engine
story-ID blacklist or a global removal of N. Source tokens and waits remain exact in every case.

## Capacity and work budget

**Confirmed measurements:** UTF-8 serialization with literal Unicode, no BOM, using the existing
world envelope and sorted selected rows. Pretty form uses two-space indentation and final LF.
The #1505 design measured the optional-name shape in memory only. The implemented #1602 derivation
then wrote the fresh selected candidate and confirmed the same 111,580-byte world envelope.

| Candidate | Message / name rows | Compact bytes | Pretty bytes |
| --- | ---: | ---: | ---: |
| Selected world messages | 806 / 0 | 79,598 | 107,044 |
| Selected messages and necessary names | 806 / 51 | 82,393 | 111,580 |
| Full message corpus, comparison only | 4,267 / 0 | 392,985 | 538,105 |
| Full messages and names, comparison only | 4,267 / 337 | 412,108 | 568,693 |

Maximum selected translated message is 147 UTF-16 units / 327 UTF-8 bytes including control syntax;
maximum selected name is 6 units. These are stored-row measurements, not expanded-layout bounds.
The implemented 1,024-row ceiling leaves 218 message rows of headroom;128 name rows leave 77.
The pretty selected catalog uses 43% of 256 KiB. Keep 8,192 units/message for compatibility, rather than tightening old
admission merely because this subset is small. A 128-unit name cap comfortably bounds this plain
display projection without changing custom-name admission. Entry retains its 256-row ceiling.
Both `GameText` pre-read guards and `DisplayCatalogReader` must agree with the chosen byte limit.

The current selected world read measured 118,241,084 bytes; the campaign owner's earlier observation
recorded 117,949,866. Do not relabel that older observation or infer same bytes from its path. This
investigation checked current package/revision/map/battle selectors and source-text identity; it
does not rerun or replace the historical runtime observation. Prefer the retained compact inventory
for review. A fresh inventory may parse one world once, retain only selectors/programs/IDs, then
release assets. Work is O(world bytes + selected graph states + corpus/name bytes); each graph state
includes program counter, cursor and call stack. The measured traversal visits 7,509 states. This
is a bounded graph analysis, not a general path solver. Stop and report an unknown on recursion,
unbounded cursor growth or an unresolved target rather than silently dropping it.

Allow 1 GiB peak memory for that one JSON parse, no content-pack copy, and at most 2 MiB of new
reports/scripts. The compact structure is 850,180 bytes and the closure 231,774 bytes. Corpus reads
are a few MiB; catalog generation is linear in selected rows/tokens and produces under 128 KiB.
Offline derivation requires no network/API/model batch, native launch or package regeneration.
The runtime consumer/build budget is owned by the verification guide.
Replan if the selection changes, reports exceed 2 MiB, the catalog approaches 256 KiB, or real new
token/caller semantics appear. A row-cap increase must have a new measured selection, not an
automatic full-corpus fallback.

## Mounting, switching, failure and rollback

Ordinary launch must select both the current world and its generated catalog via the existing
`SF2_PRIVATE_EXPLORATION_CONTENT`/`SF2_DISPLAY_CATALOG`; entry similarly selects
`SF2_PRIVATE_ENTRY_CONTENT`/`SF2_ENTRY_DISPLAY_CATALOG`. Put these selections in the owning local
input configuration, resolving from configured roots, not checkout-specific paths in tracked code.
An output file beside a package is not implicitly mounted. No filesystem scanning or chapter overlay
stack is needed. `GameRoot.BeginSource` binds the actual admitted scenario, while
`EntryPresentationAssets` binds the actual entry profile before displaying its dialogue.
World-name lookup also runs before BeginSource: New Game/Continue bind the world display candidate
from their already admitted definition before slot labels/default-name hints, then rebind for the
actual started session. This reuses existing admission without parsing another 118 MB world just for
names. Stale bindings clear when changing a source or rejecting a candidate. Entry-only viewing
still works without a world catalog.

Keep the current clean-preferences language default **English**. The private Chinese setup selects
Chinese once through ordinary Settings/Apply; that saved preference controls later ordinary launches
and Continue. Existing user preference always wins. Do not use a permanent read-only
`--input-settings` override as the delivered switching mechanism, or silently change save content
to remember a locale. This is the explicit default choice; changing first-ever product language is
not a prerequisite for complete Chinese availability.

Reuse `SettingsHost`'s prepare → persist → publish sequence and current eligibility/input hold.
Chinese/English refresh changes only displayed text/names/labels and reading projection. It must
not rerun scene init, restart entry audio, change canonical source units/work, generate simulation
ticks or consume an acknowledgement. Resolve captured operands on a same-token redraw; do not
replay the gameplay operation to obtain them. Settings are currently offered at stable host entry
or field-menu boundaries; this plan does not add mid-battle settings or promise a switch during
every wait. Preselected language must still work in every selected consumer.

| Failure | Required behavior and diagnostic |
| --- | --- |
| Missing/unreadable/oversize/wrong-source world or entry overlay | Keep usable canonical English for that namespace, show `overlay-unavailable`, preserve chosen preference; no partial identity match or raw control output |
| Missing/invalid translation row | Whole source-row fallback with `missing-translation` / `invalid-translation`; never splice a half-mapped control segment |
| Missing name row or source name identity | Preserve canonical/custom display fallback and report the exact namespace/index or `display-operands`; expose name coverage separately from sentence locale |
| Missing canonical operand | Existing explicit operand failure; do not guess a caller, member, item or class. A translated row alone cannot repair gameplay context |
| Required English UI resources invalid / preference write failure | Existing recovery UI / Apply rejection; retain the last working candidate and settings |
| Unsupported gameplay frontier | Existing localized unavailable state; no new gameplay or fabricated source text |

`GameText.Coverage` reports admitted world message/name counts and separate entry status in
Settings and the existing observation surface. Diagnostics remain bounded to128 entries. A finite offline full-selection check is the
coverage authority; the 128-entry diagnostic set is not a coverage counter. Do not claim zero
fallback because diagnostics saturated or because only the currently shown sentence is Chinese.
Missing name translations must not be hidden behind a body `EffectiveLocale=zh-CN`.

Deploy new code before selecting the new catalog. The new reader must still accept the old 209-row
world overlay and six entry aliases without names. Old binaries reject the 806-row/new-name shape
and should fall back visibly; they do not gain compatibility by stripping unknown fields. Rollback
selects the retained old 209 overlay with the older executable, or clears only the display selection
for canonical English. Keep the admitted world, saves and sole corpora unchanged. Same-package older
gameplay revisions remain governed by their existing save/content compatibility checks; loading
more display rows neither upgrades their capabilities nor authorizes loading a revision4 save in
an older world. No save migration or destructive in-place overlay overwrite is needed.

## Delivery responsibilities

The two implemented responsibilities preserve display identities and deliver the finite private
Chinese candidate. Neither creates a general localization refactor or future-token queue. `A`, `C`, `G`, `T` below mean `remake/src/Sf2.Remake.Application`,
`remake/src/Sf2.Remake.Content`, `remake/game`, and `remake/tests/Sf2.Remake.Engine.Tests`.
Actual dispatch must freeze the exact touched paths from these owners and serialize shared files.

### 1. Preserve the existing display operands

Implemented as the identity prerequisite described above; catalog/name consumers remain step 2.

Outcome: every selected name-bearing display can identify its value without parsing an English
string or inspecting a mutated slot. Keep current rendering and old overlays working.

Exact producer/interface owners: `A/Runtime/Exploration/ExplorationState.cs` and
`ExplorationTextRunner.cs` (class identity); `A/Runtime/Exploration/PartyDetailsQuery.cs` (item ID);
`A/Runtime/Battles/BattleGiveContinuation.cs` and `BattleDiscardContinuation.cs` (committed IDs);
`A/Content/Scenarios/BattleSceneDefinition.cs` and `C/Scenarios/BattleSceneContentReader.cs`
(encounter-scoped enemy-name IDs). Reuse existing typed Search/Shop/payment/SpellRef interfaces.
Optional display fields keep old/authored definitions viable. No canonical value is translated.

Acceptance: small actual Application behavior assertions for join/member-class identity, explicit
custom precedence, Give exchange/Discard IDs after slot mutation, and encounter-separated enemies;
direct Content admission with the existing controlled packages for the new scene metadata. Use
existing test owners where applicable, `uv run sf2 verify engine` and `uv run sf2 verify adapter`
under the current verification policy, preserving completed failure results and rerunning only
affected failures after corrections. Do not add tests of the offline selector/probe/reader harness.
Rollback is the prior display path; no persisted data changed. A missing authoritative identity at
one of these seams blocks that consumer's Chinese-name claim, not the corpus or unrelated UI.

### 2. Deliver and mount the finite private Chinese catalog

Depends on slice 1. Outcome: selected dialogue, entry and all implemented semantic screens use
the 806 messages/six aliases/51 names through ordinary launch and a persistent language preference.

Exact catalog owners: `C/Localization/DisplayCatalog.cs`, `DisplayCatalogReader.cs`,
`G/src/Localization/GameText.cs`, its focused `DisplayNames.cs`, `src/sf2tool/remake_localization.py`, and this document plus
the localization owner's current-status summary. The private source corpora remain read-only;
dispatch separately names fresh derived world/entry outputs and their private review/handoff.
The limits change only world message count plus the bounded optional-name extension. Existing
`ExplorationTextTokens` and canonical source units require no grammar expansion.

Exact view owners for name formatting and diagnostics: `G/src/Exploration/ExplorationDialogueView.cs`,
`PartyStatusView.cs`, `PartyDetailsPanel.cs`, `FieldActionView.cs`, `ChurchServiceView.cs`,
`ShopServiceView.cs`; `G/src/Battles/BattleCharacterNames.cs`, `BattleSceneView.cs`,
`BattleCommandMenu.cs`, `BattleSessionView.cs`, `BattleGiveView.cs`, `BattleDiscardView.cs`,
`BattleRetreatView.cs` and the marker/roster producer `BattlePresentation.cs`; `G/src/Entry/NameEntryView.cs`, `PartyNameHost.cs`, `NewGameSetupHost.cs`,
`OpeningPersistenceHost.cs`, `SaveSlotView.cs` and `SessionEntryView.cs`.
Resolve existing call sites before dispatch; these are responsibility owners, not permission to
redesign all their UI. `G/src/GameRoot.cs`, `G/src/Entry/SettingsHost.cs` and
`EntryPresentationAssets.cs` own mounting/status only if their existing bindings need adjustment.
Any public diagnostic labels belong to `G/localization/en.po` and `zh_CN.po`; private dialogue/name
prose never enters those public files. Do not modify input alphabet or save serialization.

Acceptance: full selected-source equality, exact ID/key union, source control order and per-segment
parameter identity, all 806 row admissions and all 51 name lookups, entry alias equality, explicit
old 209→new 806: 34-row review, and zero unaccounted selected-row/name fallback. Directly exercise
missing-row/name, wrong source, oversize/duplicate and old-overlay recovery. Reuse the existing
reader/resolver admission harness with the larger selection; do not construct another comparator.
Use current narrow Application behavior assertions for custom/class/item identity; compile adapter.
Publication means only selecting the reviewed private candidate locally, not distributing assets.
Rollback restores the previous selection or English; source and canonical state remain unchanged.

### Independent product acceptance and stopping condition

Use the whole-selection static check plus a few existing real-consumer background observations,
not a replay of the full route. Reuse the owned instance/environment and existing localization,
entry, party/service and battle observation hooks. A necessary updated binary/import launch follows
the environment owner; this design authorizes no takeover or restart of another task's instance.
Choose representative seams that jointly cover:

1. Ordinary entry with the approved six aliases, Settings Apply/Cancel and subsequent persisted
   launch preference; one new world row and one calibrated historical row from ordinary field flow.
2. Join446 with its real NAME/CLASS operands and a literal custom-name variant; a preserved Search
   description4065–4073; one already accepted choice/no/cancel handoff retaining the resolved body.
3. A semantic service/party screen plus a committed Give/Discard result and a battle message/card,
   checking item/spell/enemy identity and fallback diagnostics. One wide custom name and the longest
   selected row need actual panel reading/ReadMore checks, not another full-corpus font sweep.

Controlled seams are allowed when named as such; they prove the actual consumer, not preceding
natural reach. Existing campaign observations remain evidence of the English route. Compare
source units, wait token/index, canonical work, flags/resources/RNG and relevant saved fields across
the display-only variation; compare semantic state at equivalent boundaries rather than wall time.
Use state/input/projection/error observations and no screenshots. Human Chinese reading and image
lettering remain separately reported; technical projection does not establish typography taste.

Only say **“pre-voyage Chinese is connected”** after the complete selected static admission,
identity/display seams, ordinary world/entry mounting, required scoped gates and independent
consumer acceptance pass, with every remaining English surface classified. Report separately:
translated corpus, parser compatibility, mounted catalog, observed consumer/natural-flow scope,
custom/technical/image exclusions, and human reading status. This design alone satisfies none of
the new runtime claims. A failed chosen seam, unexplained English name, source mismatch or missing
operand is a real implementation acceptance blocker. Unknown natural reach of every optional branch,
future DICT/delay behavior and unimplemented later chapters are not blockers to this bounded design.

## Reproduction and retained limits

The local `local/issue1505/` handoff holds `selected-structure.json`, `inventory.py`, `closure.json`,
`name-selection.json`, `metrics.json` and read-only input metadata. These are review aids; the exact
ID/key sets and selection algorithm above are the durable contract. From the owning checkout,
after loading its current private-input configuration in the same process:

```powershell
python -X utf8 local/issue1505/inventory.py
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

For a fresh extraction, read the selected world once and retain `world.programs`, map/setup
onLoad/input/events/battle selectors and Search definitions; omit rasters/audio/layout payloads.
Reapply the explicit Map40/Map73/Map14 constraints above. For name selection, union seven-member
start items/growth spells/classes, selected Search items and Shop stocks, and Battle01–07 placement
enemy identities plus their source item/spell definitions. Compare the current sole corpora with
the pinned text/name tables, never an old translation snapshot. Measure a candidate in memory with
the exact JSON shape/encoding described above; the pretty and compact numbers are reproducible
serializations, not installed-file sizes. Reuse the accepted full-corpus disposition; no second
semantic or native-font sweep is needed.

The #1505 design investigation used direct scope/link/ID/private-boundary checking and its committed
planner/actual CI selection. No SDK, Godot, emulator, research or engine aggregate ran for that design.
The implemented candidate's scoped checks and real consumers are recorded above and in the verification owner. Prior completed toolchain/engine/observer failures and ObjectDB shutdown
Unknown remain with their original owners; this design does not turn them green or restart them.
