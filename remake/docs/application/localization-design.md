# Game Localization Design

## Scope and status

**Proposed design; localization is Planned / Not implemented.** This is the M0 investigation for
[#1469](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1469), under the
[game localization epic #1468](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1468).
Acceptance selects an implementation direction, not bilingual product acceptance. Source findings
use accepted commit `9f5c5af81ad0da0e7d84f8033df306accbc4b7cb`; they describe this remake, not new
original-game evidence. Inspect named symbols with `rg -n`, or reproduce the baseline using
`git show <commit>:<path>` for the linked files.

**Standard rigor:** this is a lasting cross-Application/Content/Godot capability. Prove three real
consumers before expansion. The existing [High save/restore boundary](opening-session-lifecycle.md#format-compatibility-and-local-storage)
still governs format/identity changes. M0 changes documentation only. M1 should establish
English/zh-CN settings, explicit fallback, a deliverable CJK font, one controlled dialogue and a
dynamic battle result without making translations inputs to gameplay execution.

The product target remains title/Witch, New Game/naming, supported opening exploration/services,
Battle01 outcomes and independent-process Continue, then other implemented consumers. One pilot
does not establish that coverage. Unimplemented chapters, ROM patching, voice/TTS, other languages,
full string export, a translation platform and UI redesign are non-goals. Preserve the 1080p minimum,
96px world cells, source grouping, input/reward authority and private-content boundary.

## Current production and consumption paths

**Confirmed by source inspection:** the following is a bounded responsibility inventory, not an
exhaustive string dump. Linked files and symbols locate the claims without relying on line numbers.

| Producer / identity | Current consumer | Consequence for localization |
| --- | --- | --- |
| [SettingsView.Refresh](../../game/src/UI/SettingsView.cs), [SessionEntryView](../../game/src/Entry/SessionEntryView.cs), [NameEntryView](../../game/src/Entry/NameEntryView.cs): hardcoded labels/help/errors | Godot Button/Label/LineEdit and hints | Resolve stable UI keys when views refresh. `NameEntryView.AddAction` currently uses `text == "Use name"` to identify a button; change that to an action/reference before translating. Keep node names derived from labels stable too. |
| [FieldService.Submit/Completion](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldService.cs): `ChurchWait.Message` stores English, paid cost and an appended curse notice | [ChurchServiceView.Present](../../game/src/Exploration/ChurchServiceView.cs) | Retain result kind/effect/cost and the independent notice as values. Payment/queue advancement precede display. Do not recover semantics by parsing the sentence or let translated nullness decide the service phase. |
| [HealingSceneCursor.Create/Enter](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs): names/spell/amount substituted into `ActionText`/`RecoveryText`; private IDs 274/298 | `Enter` creates logical work per character; [BattleSceneView.Message](../../game/src/Battles/BattleSceneView.cs) displays the same strings | Chinese replacement would change work and potentially fairy/RNG progression. Retain execution strings and add semantic display operands for M1. |
| `BattleSceneState`: phase, actor, target, amount, rewards, item and growth values | `BattleSceneView.Message` maps source IDs and replaces tags; AURA uses English interpolation | Most semantics already exist. Reuse those fields, without a duplicate event bus. Reward/HP publication remains in the continuation. |
| [BattleGiveState.Text](../../src/Sf2.Remake.Application/Runtime/Battles/BattleGiveContinuation.cs), [BattleDiscardState.Text](../../src/Sf2.Remake.Application/Runtime/Battles/BattleDiscardContinuation.cs), [BattleRetreatContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRetreatContinuation.cs) | Respective battle views | Migrate each real consumer using retained actor/item/spell operands. Authored English is not a universal source-text ID. |
| [remake_exploration_content.py](../../../src/sf2tool/remake_exploration_content.py) reads pinned `disasm/data/scripting/text/gamescript.txt` hexadecimal IDs and `allynames.asm`; [ExplorationContentReader.ReadTextTokens](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) tokenizes text | [ProgramRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) selects DialogueWait, W1TextWait or FieldTextWait; [ExplorationDialogueView](../../game/src/Exploration/ExplorationDialogueView.cs) has separate projections | Reuse integer ID plus namespace, not English spelling or list position. The compatibility regex path is not a general token interpreter. |
| [ExplorationTextRunner.Begin/Normalize/Span/Accepted](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs) expands names/classes and builds source glyph units | [FieldTextWait](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs) stores units, projection and logical cursor; host separately reveals/delivers/acknowledges | Characters above 255 or absent from source font mapping reject (`field-text-symbol`). Glyph advances determine logical wrap/scroll work. Chinese must not replace those units/advances. |
| [EntryPresentationReader](../../src/Sf2.Remake.Content/Scenarios/EntryPresentationReader.cs) admits private dialogue keys 216–221 and six named image resources | `SessionEntryView` / [EntryPresentationAssets](../../game/src/Entry/EntryPresentationAssets.cs) | Entry has a separate namespace/profile/preparation path. Title/numeral image lettering is not fixed by a string catalog. |
| [CharacterNames.Member/Battle/Actor](../../src/Sf2.Remake.Application/Runtime/CharacterNames.cs): actor overrides, source names or authored readable identity | Dialogue, party, battle, Church and slots | Reuse identity binding and override precedence. ReadableIdentity/"Name unavailable" are fallbacks, not approved Chinese names. [BattleCharacterNames.HealingText](../../game/src/Battles/BattleCharacterNames.cs) repairs only an authored leading subject today; replace that workaround for migrated semantic consumers. |
| Item/spell/class identities and labels from [PrivateBattleDefinitionReader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), definitions and [BattleSceneDefinition](../../src/Sf2.Remake.Application/Content/Scenarios/BattleSceneDefinition.cs) | Choices, party details, scene messages and offers | Preserve packed item flags/slot, item ID, SpellRef/level, class ID and ActorRef. Never translate IDs or uppercase translated spell names. |
| `SessionFailure.Kind/Code/Field/Message`, Content and storage failures | Recoverable UI and local diagnostics | Map finite codes to user messages; retain stable code/field/stop reason. Unknown failures get safe generic wording plus code. English fallback cannot turn unsupported content into success. |

There is no game localization service/catalog configured in the current host.
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
project pins Godot.NET.Sdk/4.7.2. No import/runtime was run. M1 must compile and exercise the installed
4.7.2 bindings; documentation is not project execution evidence.

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

Reject duplicate IDs/locales, wrong identity, unknown fields, malformed UTF-8/shape, unbound
placeholders and altered controls. Missing rows mean explicit partial coverage. Optional overlay
failure deactivates that overlay, not the valid gameplay package. Required source admission still
fails normally. Proposed M1 bounds are 256 KiB/64 entries per overlay with no external includes;
these are tiny-pilot limits, not measured full-game requirements.

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
| Wrong placeholders/controls | Reject row during admission; valid English fallback | `invalid-translation`, not an ordinary missing row |
| Optional overlay missing/corrupt/wrong identity | English display over valid gameplay; bounded unavailable notice | Retain requested versus effective locale and stored preference; no automatic rewrite |
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
falls back as a whole row to preserve mapping.

Reuse ExplorationSessionView delivery: translated reveal/ReadMore precede CompleteTextReveal, while
logical readiness still gates Acknowledge. Replace English-prefix reveal continuity for this migrated
consumer with `(window identity, text ID, source range)` continuity. Identical text in separate waits
must still deliver. Visual wrap/ReadMore never adds a script acknowledgement. Preserve skip and
release barriers. Other DialogueWait/W1/Search/battle-story consumers remain explicitly English
until their own migration is accepted.

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

## Font, layout and delivery

Recommend one bundled Simplified Chinese sans face with existing Latin/symbol coverage through
StoryTheme and necessary overrides. Start M1 selection with Noto Sans CJK SC Regular: upstream
[Sans documentation](https://github.com/notofonts/noto-cjk/blob/main/Sans/README.md) describes the
family and the [license](https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE) is SIL OFL 1.1.
This is a candidate, not a pinned/vendored asset. Acquisition must pin release/file/license, retain
notices, check modification/reserved-name conditions for any subset, and declare distribution
ownership before committing bytes. M0 downloads no font.

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
speech loop need observation. UTF-16 units, code points, grapheme clusters and shaped glyphs differ.

Preserve field alternating nonspace speech with instant/reveal-all silent; do not replay it on
refresh. No new battle speech bleeps. Chinese pacing is a human-readability question, not a reason
to change simulation speed. Title/numeral raster lettering requires a separate licensed/authored
asset decision; retain it and identify the untranslated gap until then. A catalog grants no art rights.

## Implementation sequence and acceptance

These are finite candidates, not authorized implementation or a new queue. Paths are repository
relative: `A` = `remake/src/Sf2.Remake.Application`, `C` = `remake/src/Sf2.Remake.Content`,
`G` = `remake/game`, `T` = `remake/tests/Sf2.Remake.Engine.Tests`. New paths are proposals; no
scaffolding exists in M0. Dispatch must recheck names and single-writer ownership, especially shared
settings/catalogs/reader/theme files. Update closest capability/verification owners when behavior lands.

### M1 integrated sample

| Responsibility | Exact proposed paths |
| --- | --- |
| Selection, lookup, settings | New `G/src/Localization/GameText.cs`, `G/localization/en.po`, `G/localization/zh_CN.po`; existing `G/project.godot`, `G/src/GameRoot.cs`, `G/src/Input/InputSettings.cs`, `G/src/Entry/SettingsHost.cs`, `G/src/UI/SettingsView.cs`, `G/src/Entry/SessionEntryView.cs` (settings access refresh only) |
| Overlay and token reuse | New `C/Localization/DisplayCatalog.cs`, `C/Localization/DisplayCatalogReader.cs`, `C/Scenarios/ExplorationTextTokens.cs`; existing `C/Scenarios/ExplorationContentReader.cs`; new original `remake/content/authored/localization-pilot.json` and `remake/content/authored/localization-pilot.zh-CN.json` |
| Field display | `A/Runtime/Exploration/ExplorationState.cs`, `A/Runtime/Exploration/ExplorationTextRunner.cs`, `G/src/Exploration/ExplorationDialogueView.cs`, `G/src/Exploration/ExplorationSessionView.cs` |
| HEAL display | `A/Runtime/Battles/BattleSceneContinuation.cs`, `G/src/Battles/BattleSceneView.cs`, `G/src/Battles/BattleCharacterNames.cs` |
| Font and focused acceptance | `G/src/UI/StoryTheme.cs`; new `G/fonts/localization/NotoSansCJKsc-Regular.otf`, `G/fonts/localization/LICENSE.txt`, `G/fonts/localization/README.md`; existing `T/ExplorationTextWaitTests.cs`, `T/BattleSceneTests.cs`; new `T/LocalizationDisplayTests.cs` for actual engine/content behavior; new `G/probes/localization_observation.gd` only if existing observations cannot drive the necessary cases |

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
locale/fallback. Use actual engine/content behavior tests, affected `uv run sf2 verify engine`,
`uv run sf2 verify adapter`, and bounded actual Godot state/input/geometry/errors. Do not test the
probe or capture screenshots. Restart only for necessary changed-code/font import/startup acceptance.

Stop with a reviewable pilot PR, font/import measurements and remaining English consumers explicit.
If preserving logical work requires a new interpreter/timing policy, private rights are unestablished,
or font/reveal cannot be demonstrated, stop expansion and retain English/slots/original resources.
M1 does not enable Unicode naming or close the epic.

### Next bounded candidates

| Candidate / dependencies | Initial exact ownership | Acceptance, failure and stop |
| --- | --- | --- |
| M2a Church/Search feedback after M1 | `A/Runtime/Exploration/FieldService.cs`, `A/Runtime/Exploration/FieldSearch.cs`, `G/src/Exploration/ChurchServiceView.cs`, `G/src/Exploration/ExplorationDialogueView.cs`, `G/src/Localization/GameText.cs`, `G/localization/en.po`, `G/localization/zh_CN.po`, `T/FieldSearchTests.cs`, `T/FieldServiceTests.cs` | Finite result values replace concatenation/string-phase coupling. Success/decline/insufficient/full-pack cases preserve payment, party/status/receipt order and confirmation. Actual behavior/adapter checks; display failure retains committed rewards. Stop after these consumers. |
| M2b Entry/slots/naming help after M1 | `G/src/Entry/SessionEntryView.cs`, `G/src/Entry/NameEntryView.cs`, `G/src/Entry/SaveSlotView.cs`, `G/src/Entry/OpeningPersistenceHost.cs`, `G/src/Entry/EntryPresentationAssets.cs`, `G/src/Localization/GameText.cs`, both PO paths above, `C/Localization/DisplayCatalogReader.cs` for entry namespace | Stable node/action identity, six entry dialogue bindings, readable errors/slots, old names/saves unchanged. Direct entry/Continue observations; no refresh-triggered music restart or schema/image replacement. Stop with untranslated image lettering explicit. |
| M3 bounded opening terminology/dialogue after admitted consumers and rights/review | Public owner `remake/docs/application/localization-design.md` only if coverage/boundary changes; private catalog under a separately declared non-worktree content root. Dispatch must name those actual private files before writing | Start with reached IDs/names in one playable segment. Direct source/control/parameter comparison plus bilingual review. No runtime/export edits by translators. Drift/ambiguity stops affected rows; fallback stays explicitly untranslated. No whole-game batch. |
| M4 Chinese naming/IME after M1 and accepted High compatibility policy | `A/Runtime/NewGameOptions.cs`, `A/Runtime/OpeningNewGame.cs`, `A/Runtime/CharacterNames.cs`, `A/Runtime/Exploration/ExplorationTextRunner.cs`, `A/Runtime/Battles/BattleSceneContinuation.cs`, `A/Runtime/Persistence/OpeningSaveData.cs`, `A/Runtime/Persistence/OpeningSave.cs`, `A/Runtime/Persistence/OpeningRestore.cs`, `C/Persistence/OpeningSaveJson.cs`, `G/src/Entry/NameEntryView.cs`, `T/OpeningSaveTests.cs`, `T/ExplorationTextWaitTests.cs`, `T/BattleSceneTests.cs` | Actual composition/commit/cancel/paste/focus, old name/save preservation and Unicode independent-process Save/Continue. Refuse enablement with unresolved canonical work/backward reading/downgrade; no lossy conversion. Stop at named admitted characters/consumers. |

Further Party/Prepare/battle screens are separate consumer groups, not implicit all-files ownership.
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
bounds are exceeded or required characters lack coverage. No precise runtime/translation estimate
or completion date is supported yet.

Before M3 count only proposed admitted/reached IDs, unique source words/characters, token-bearing
rows and names; sample a dialogue/result with human review and estimate review effort separately
from translation throughput. No API/model expenditure is authorized by this plan. Rights/terminology
and quality are not established by placeholder checks.

Outstanding boundaries: installed 4.7.2 catalog/font behavior; display-range mapping through
CLEAR/D1/choice/close; HEAL invariance under slow translated reading; mixed-script reveal/speech/
punctuation; exact licensed font pin; game terminology/private translation publication; Windows IME
event ordering; Unicode save downgrade marker. Runtime projection/font/invariance are M1 stopping
gates; naming questions belong to M4. None authorizes runtime work in this docs-only M0.

## M0 review and verification

Review the three real consumers, dependency direction, settings transaction, canonical-work
separation and save identity. Check links/anchors, examples/fences/tables, exact scope and absence
of private content. On a clean committed candidate load the owning ignored configuration and run
`uv run sf2 verify plan --base origin/main --head HEAD`; interpret it under the
[documentation-only scope](../development-and-verification.md#scope). Record exact head/tree/base,
direct checks and actual CI in the handoff/PR, not a growing history here. No engine/adapter/full
suite or runtime observation is an M0 acceptance claim. Main-gate owns independent integration.
