# Full message corpus layout review

## Scope and result

**Standard rigor:** this #1583 review checks every pinned English message and its accepted Chinese
counterpart, the separate name tables and entry aliases. Acceptance separates exact source/control
integrity, per-row semantic review, production parser/resolver compatibility, native font/layout and
actual consumer admission. It does not translate or mount new content, implement missing controls,
replay the campaign, or establish human reading approval. The
[display policy](../godot/presentation.md#source-line-reflow-1583) remains unchanged.

**Confirmed:** all 4,267 IDs 0–4266 were reviewed, not just the opening subset. Production parsing
supports 4,010 rows; 257 retain unsupported controls/operands. Native measurement of the supported
projections, plus separately identified literal/name glyph checks, found no missing glyph or
character-bounds overflow at the measured widths. This is not a claim that all game text has been
admitted, that every original line break is expendable, or that every caller/screen has been tested.

| Boundary | Denominator and result |
| --- | --- |
| Exact source and Chinese controls | 4,267 rows; 173,995 source / 87,515 target characters; zero mismatches |
| Source `{N}` | 2,742 rows / 3,616 occurrences; zero literal LF in either message corpus |
| Separate name tables | 337 entries: ally30, class32, item128, spell44, enemy103; source-expression checks pass |
| Production-compatible messages | 4,010: 3,426 without name/class operands, 584 with controlled bindings |
| Unsupported messages | 257 unique rows; literal glyph checks only, not successful message consumption |
| Existing Chinese world subset | 209 rows resolve as `zh-CN`, zero diagnostics/fallbacks in the isolated check |
| Entry aliases | IDs216–221; six aliases of existing message IDs, not six additional messages; zero fallback |
| Godot Label measurements | 40,676 records / 1,870 distinct characters; zero missing glyphs or bounds overflow |

The accepted implementation baseline is `0b6398dbedffc1ccdcfbc1eadffb3e9af9005148`.
Source identity is `ShiningForceCentral/SF2DISASM` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/data/scripting/text/gamescript.txt`, with USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Source table keys are hexadecimal; message IDs below are decimal. The private full-text authority,
name namespaces and frozen runtime subsets remain owned by the
[localization agreement](../application/localization-design.md#current-full-text-chinese-corpus).
No private source/translated prose, assets or review transcripts accompany this report.

## Semantic line breaks

Every source/translation pair was read in two non-overlapping slices, IDs0–2133 and2134–4266;
the coordinator independently read all 107 preservation candidates and the three ambiguities.
Classification is **Inferred editorial interpretation**, not new evidence of original caller intent.
Source token locations and coverage are **Confirmed** by direct comparison. No global deletion of
`{N}` follows from this review.

| Classification | Rows |
| --- | ---: |
| No source line break | 1,525 |
| Ordinary prose layout | 2,564 |
| Existing explicit/control-adjacent boundary | 68 |
| Preserve a semantic line break | 107 |
| Ambiguous semantic line break | 3 |

The following compact index identifies the exact preservation candidates. `N1` means the first
`{N}` occurrence within that source row, not token index1. The private per-ID disposition additionally
records zero-based token indexes, unsupported/unbound operands and frozen Chinese admission status.

| Suggested breaks | Rows | IDs |
| --- | ---: | --- |
| N1 and N2 | 7 | 1350, 4043, 4047–4048, 4135, 4137, 4141 |
| N1 and N3 | 2 | 1709, 4134 |
| N1 | 98 | 1361, 3484–3487, 4035–4036, 4039–4042, 4044–4046, 4049–4053, 4055–4057, 4059, 4061–4063, 4065–4072, 4074–4075, 4079–4081, 4083–4089, 4091, 4093, 4097–4100, 4105–4111, 4114, 4117–4130, 4132–4133, 4136, 4138–4140, 4142–4145, 4152–4153, 4162–4163, 4165–4166, 4171–4176, 4184–4185 |

These include title/body, inscription/byline and verse boundaries. Mixed rows such as4041,4128,4134
and4138 also contain ordinary internal wrapping; future admission should preserve the relevant
semantic breaks instead of applying a whole-row blacklist. Existing Search intentionally retains
all source line breaks through the default whole-row resolver. The **21 currently admitted**
preservation candidates are Search descriptions:
`4035–4036, 4039–4053, 4055–4057, 4059`. Their current policy already preserves those boundaries;
they are not newly discovered reflow defects. The remaining86 candidates are outside the frozen
Chinese subset. That status does not establish absence of an English consumer or natural reach.

**Unknown:** row330 N1 may distinguish narration from a vocalization; rows3207 and3529 N1/N2 may
express chant or refrain cadence. All three are outside the frozen Chinese subset. Their original
intent, caller and timing remain unresolved; a blanket prose-folding pass is not semantic acceptance.
No story-ID blacklist or new rendering mechanism is introduced for these future admission questions.

Search uses `GameText.Search` with default whole-row preservation. Ordinary exploration opts into
reflow; W1 still has its separate English boundary. `BattleStoryView` displays its authored body,
while `BattleSceneView`'s raw source fallback explicitly maps `{N}` to LF and semantic battle messages
have their own localization. The full-corpus hypothetical field projection does not change or prove
those consumers' real caller bindings.

## Unsupported and unbound context

These token groups overlap; their union is **257**, not the sum of the row counts. They remain
unsupported by the production exploration parser/resolver in this check. Glyph coverage of their
literal fragments does not validate delay semantics or item/spell/dictionary/number expansion.

| Token | Rows | IDs |
| --- | ---: | --- |
| `{D2}` | 83 | 238–239, 4186–4266 |
| `{D3}` | 34 | 2, 6–8, 12, 158, 177, 194, 306–309, 326–329, 348–360, 397, 421, 448, 463, 3338 |
| `{ITEM}` | 72 | 25–26, 29–30, 37, 40–42, 44–48, 50–51, 54–55, 62, 65–67, 69–74, 84, 87–88, 96, 143, 163, 207, 213–215, 257, 275, 316, 372–391, 394–396, 413, 415–416, 420–421, 2022, 3357, 3373, 3375 |
| `{SPELL}` | 14 | 107, 243, 271–272, 274, 283, 348–353, 370–371 |
| `{DICT}` | 4 | 98, 362, 471, 476 |
| `{#}` | 69 | 101, 123, 130, 150–155, 163, 178, 183, 187, 202, 237, 242–244, 263, 265–270, 272, 274, 284–285, 287–288, 298–300, 307, 311, 314, 317, 332–334, 341–345, 356–357, 360, 364–369, 371, 392–393, 398–402, 414, 463, 470–471, 475–476 |

Of the584 supported rows requiring name/class operands,484 identify fixed members; **100** contain
generic `{NAME}`/`{CLASS}` whose natural bindings are not established by this audit:
`17, 19, 21–22, 34, 39, 59, 64, 77, 80, 121–122, 126–127, 129, 131–133, 138–142, 145–146,
149, 156–157, 167–168, 175–176, 185, 208, 211–212, 241, 260–261, 273, 276–282, 286, 289–296,
301–305, 310, 313, 315, 318, 322–325, 330–331, 335–340, 346–347, 361, 403–405, 418–419,
423–433, 442, 445–446, 2874`.

The isolated resolver uses pinned default member names, a legal synthetic generic-name/class binding
and a seven-character wide Latin custom-name variant. These are controlled fixtures, not evidence
that every caller supplies those values or that every possible name/class expansion fits. In total264
corpus rows contain generic operands when unsupported item/spell/dictionary/number forms are included.

## Admission and typography limits

**Confirmed:** the historical209-row world overlay remains independently readable. Thirty-four of its
translations differ from the later accepted full corpus:
`447, 480, 483, 487, 491, 495, 497, 504, 520, 538, 544, 545, 551, 553, 560, 568, 569, 579,
590, 591, 2178, 2187, 2192–2193, 2197, 2203, 2206, 2211–2212, 2299, 2304, 4044, 4048, 4052`.
They are frozen historical admission evidence, not an alternate full-text authority. Both forms were
measured separately; this review does not overwrite them. Entry216–221 source and target mappings
match exactly after the existing entry-format control projection. Resolving the209 plus six aliases
offline does not replay all actual callers. Pre-voyage/full-game Chinese admission remains incomplete.

The native sweep uses the actual ordinary host's Theme, WordSmart Label settings and30px Noto Sans
CJK SC body font, with1392px no-portrait and1176px portrait body widths; entry uses592px. A measurement
Label grows to its shaped minimum height. It checks `font.has_char` and each nonempty character bound
against width/height with1px tolerance. The largest result is7 lines /326px. This is **not** proof of
whole-screen clipping, all page/input states, every name-widget width, physical raster appearance or
human readability. Existing finite reading/page observations retain those separate boundaries.

The40,676 layout records comprise37,972 corpus projections,1,504 historical-subset projections,
514 unsupported-row literal checks,674 standalone name checks and12 entry aliases. Name checks use
the body font only, not every actual name UI. The corpus reader batches128 rows beneath the unchanged
256-row/256KiB limits; no full-game overlay is mounted and no capacity change is implied.

## Reproduction and retained evidence

The one-off scripts and private reports remain under ignored `local/issue1583-full/` in the assigned
review checkout; they are not a new verification platform. Configure the current private inputs
through the [local input owner](../../../docs/operations/local-private-inputs.md). Inspect the retained
scripts' explicit selections before reuse in another checkout, keep canonical inputs read-only, and
choose fresh output names. Reuse the existing configured SDK, compiled Content/Application/Domain
and Godot imports; no production source changed in this review.

1. `static.py` compares every message with the pinned table, exact control order and per-control-segment
   parameter multisets; checks337 name source expressions and exact ID coverage. `semantic-a.json`
   and `semantic-b.json` retain the complete pairwise reviews; `disposition.json` is the merged per-ID
   index. `heuristic.json` has no visible placeholders, literal control codes or over-limit rows.
   Repeated-target candidate213/395/415 was read directly and denotes the same result, not template
   contamination. These heuristic checks supplement, rather than replace, semantic review.
2. `projection/Program.cs` uses actual `AuthoredScenarioPackageReader`, `ExplorationTextTokens`,
   `DisplayCatalogReader`, `DisplayCatalog` and `EntryPresentationReader`. It constructs an offline
   immutable text definition with the pinned strings/provenance and a tiny authored map skeleton.
   Reflection reaches internal constructors only; it never mutates a live session. It projects wait/
   append/CLEAR source-token ranges, separate default-preserving rows, historical overlays and aliases.
3. `run-layout.ps1` runs `layout.gd` on the ordinary authored host with the actual panel typography,
   isolated worktree-local writable paths, a90s deadline and no screenshot. The pilot's first32 shapes
   matched the following process frame. `layout-summary.py` checks every recorded bound/glyph result.
4. `admission01/admission.json` separately asserts `zh-CN`, zero fallback reasons and zero reader
   diagnostics for all209 historical world rows and six entry aliases. `entry-alias-check.json`
   records the exact alias transformation. Input size/mtime checks preserve the read-only boundary.

From the owning checkout, load its current ignored input configuration **in the same process**, set
absolute `DOTNET_BIN`/`DOTNET_CLI_HOME`, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and direct
TEMP/TMP to this worktree's ignored review directory. The retained runner has already been restored:

```powershell
python -X utf8 local/issue1583-full/static.py
$reviewRoot = (Get-Location).Path
Push-Location remake
& $env:DOTNET_BIN run --project ../local/issue1583-full/projection/Projection.csproj --no-restore -- $reviewRoot <fresh-projection>
& $env:DOTNET_BIN run --project ../local/issue1583-full/projection/Projection.csproj --no-restore -- $reviewRoot <fresh-admission> admission
Pop-Location
pwsh -NoProfile -File local/issue1583-full/run-layout.ps1 -Run <fresh-layout> -Projection <fresh-projection>
python -X utf8 local/issue1583-full/layout-summary.py
```

The summary helper selects the retained `layout-full01`; adjust that explicit report selection to
inspect a fresh run. Full production projection produced20,938 text records. Pilot layout:840 records,
1,227ms, peak185.4MiB. Full native layout:40,676 records,3,144ms, peak198.3MiB, exit0, stopped,
no timeout and empty stderr. Initial budgets were32MiB local outputs and512MiB native peak; measured
outputs are about26.3MiB including local compilation artifacts. Work is linear in corpus characters
times the finite projection/width variants; no world acquisition, network download or shared write is
needed. Replan before exceeding those bounds rather than deleting retained evidence.

Completed setup failures remain failures in the local handoff/logs: an inline inventory syntax error;
decimal parsing of hexadecimal source IDs; local runner constructor/namespace compilation errors;
the wrong accepted-result variant; and a fixture provenance repository spelling rejected by the
actual catalog reader. Their narrow corrections changed the observer/fixture, not production
validation. Earlier #1583 long-observation failures and ObjectDB warning Unknown remain with their
existing owner; this successful separate process does not erase them. Final documentation uses direct
scope/link/private-boundary checks and the committed dependency plan; no aggregate suite is rerun.
