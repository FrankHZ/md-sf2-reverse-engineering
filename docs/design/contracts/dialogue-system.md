# Dialogue Commands and Reached Text-Wait Contract

- **Confirmed original behavior:** the six source command layouts, handler dispatch, cursor/name-index
  state writes, source-labelled modifier bits, close/clear call order, and the bounded
  entity-to-map-sprite dialogue-property seam below; and the separately evidenced W1 polling
  rule and reached text 483 binding described below.
- **Inferred original behavior:** the explicitly bounded opening ancestry described below.
- **Unknown original behavior:** reachability outside the named observations, complete enabled-service
  and portrait state at the reached wait, rendered timing, and broader completion/repeat/persistence.
- Evidence dates: 2026-07-31 (command matrix); 2026-09-24 (existing reached-wait evidence binding).
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-map-script-engine-static-v1` in
  `tests/fixtures/h2/map-script-engine-static-v1.json`; and
  `sf2-map-script-dialogue-runtime-v1` in
  `tests/fixtures/h3/map-script-dialogue-v1.json`; `src/sf2tool/h2/map_script_engine.py`;
  `src/sf2tool/h3/map_script_dialogue.py`; and `docs/research/common-scripting.md`.
  The reached-wait binding instead consumes the retained observation and pinned source in
  [opening dialogue evidence](../../research/map3-messenger-acceptance.md#opening-dialogue-return-and-subsequent-actions).

## Confirmed Static Contract

The map-script dialogue command family has six primary macro forms: `nextSingleText`,
`nextSingleTextVar`, `nextText`, `nextTextVar`, `textCursor`, and `hideText`. Their source-defined
opcodes are `$00` through `$04` and `$09`; the physical command widths are respectively 4, 6, 4, 8,
4, and 2 bytes. The fixture retains both the emitted operand fields and those physical widths, rather
than treating handler reads as a substitute for stored bytes.

All 2,883 invocations are retained as ordered references to their existing map-script program commands,
and zero-inclusive totals cover all 304 programs. The source corpus contains 2,058 `nextSingleText`,
zero `nextSingleTextVar`, 577 `nextText`, zero `nextTextVar`, 234 `textCursor`, and 14 `hideText`
commands. `textCursor` source operands range from 240 through 4,233; the independently source/ROM
checked text-line domain is contiguous from 0 through 4,266. This is an ID-domain validation, not a
claim about decoded dialogue content or display order.

`csc00_displaySingleTextbox` and `csc02_displayTextbox` test the cutscene-text skip flag before their
display path. The four display handlers compare the packed modifier/entity word with `-1`; all call the
portrait helper before the direct entity dialogue-property consumer, call `DisplayText`, then increment
`CUTSCENE_DIALOG_INDEX`. The two `nextSingle*` handlers subsequently call the portrait-close path,
clear text, and call `Sleep` with source value 10, while the two continuing handlers do not contain that
close/sleep sequence. Both `*Var` handlers contain two word reads into the source-named dialogue name
index states. The zero-use `nextSingleTextVar` macro's four operand bytes and its handler's two word
reads remain distinct static facts; this contract does not invent a runtime interpretation for that
unused form.

The static caller audit keeps all six dialogue handlers and `csc1D_showPortrait`, including the two
zero/zero caller rows. It preserves direct-instruction and resolved-effective target identities even
where they are equal, and classifies the helper as internal versus the entity consumer as external from
their parsed source paths rather than assigning a behavioral role.

`textCursor` writes its one word to `CUTSCENE_DIALOG_INDEX`. `hideText` calls the portrait-close target
before its text-clear macro. `csc1D_showPortrait` reads the same packed word and tests word bits 15 then
14. Those handler use-sites derive high-byte `handlerTestedModifierByteMask` `$C0`; observed modifier
bytes outside the packed-word `$FFFF` sentinel are checked against that use-site-derived mask. The macro
comments label modifier byte `$80` as `display on right` and `$40` as `mirrored`; those original labels
are retained as labels, not recast as a renderer contract. The `$FF` `undisplayed` label remains separate
from the handlers' confirmed full-word `-1` comparison.

The direct entity seam is `GetEntityPortaitAndSpeechSfx` at ROM address 284,216. Its named section masks
`d0` with parsed `COMBATANT_MASK_ALL` (255), then obtains the entity address and loads its map-sprite
byte. The map-script contract joins the existing 119-row, 478-byte sprite-dialogue table only through
the sibling contract's ID, pinned commit, ROM hash, source path, and addresses; it does not copy decoded
text or use the sibling golden fixture as evidence.

## Confirmed Runtime Boundary

The one-launch `sf2-map-script-dialogue-runtime-v1` fixture retains 21 handler-local cases: all six
entry/return PC pairs, A6/stack boundaries, skip admission, source-partitioned packed inputs, the two
controlled zero-source `*Var` layouts, cursor bounds, close path, ordered direct call identities and
zero-inclusive target counts, direct state writes, and session-controlled call register words. The H3
observer captures entry/call/target/return PCs first and resolves call labels only through the unique
guarded source/H1 address map. A remake adapter MUST preserve those facts as command/service seams. The
explicit D0/D1/D2 trampoline inputs and the RTS service shims are harness controls, not original
caller/service behavior.

The three `map-script-dialogue/*` queues in `docs/research/common-scripting.md` retain the
handler matrix's outstanding questions. A remake MUST NOT infer visual portrait placement, audio
playback, text completion, controller timing, story reachability, or persistence from that matrix.
The separately sourced binding below does not enlarge what its shims observed.

## Reached W1 consumer binding

**Confirmed (pinned source and retained observation):** the opening `cs_5145C` displays text
510, then 511, then 483. Their ordered source tokens contain W2, W2 and W1 respectively.
The existing observation pairs `DisplayText` target 483 with `symbol_wait1`, the accepting
`loc_65B4` input read and that text call's return. Later trap-based displays also target 483
while the cutscene cursor remains 484; the cursor is not the active text identity. The
[research owner](../../research/map3-messenger-acceptance.md#reached-text-wait-source-binding)
retains the exact boundaries and reproduction route. This proves the named reached consumer,
not all natural text waits or their intervening service state.

Preserve the complete ordered token stream and each wait's position within it. Do not infer
one acknowledgement per displayed line, one wait per `ShowText`, or a wait from `Single` versus
`Continued` mode. Multiple waits in one text remain distinct consumers. Text decoding and glyph
data remain with [text and font data](text-and-font-system.md); private dialogue need
not be copied into a public fixture to identify a control token.

### Poll order and distinct consumers

**Confirmed (static source):** at the pinned revision,
`disasm/code/common/scripting/text/textfunctions_1.asm:symbol_wait1` saves the typewriting byte
and clears it. Every iteration at `loc_659C` then:

1. calls `GenerateRandomNumber` with range 256 against the shared main seed;
2. writes the result byte to `RANDOM_SEED_COPY`;
3. calls `WaitForVInt`;
4. reads `CURRENT_PLAYER_INPUT` at `loc_65B4`, masking directional and A/B/C buttons;
5. repeats for zero input, or restores typewriting and resumes token parsing for nonzero input.

The accepting iteration includes the draw, copy and wait. Input acceptance cannot bypass that
preamble. Any enabled services in the wait occur after this poll's copy and before its input
test; their own gates and order require caller-specific binding. This does not establish an
exactly-one entity/portrait update count from a callback frame or from the helper name alone.

| Consumer | Confirmed rule | Observation limit |
| --- | --- | --- |
| W1 | Draw/copy, wait, input test; restore typewriting on acceptance. | Text 483's named accepting read is observed. Complete live service/portrait state is not. |
| W2 | `ParseSpecialTextSymbol:@wait2/loc_6472` draws/copies, calls `sub_64A8`, waits and tests input; acceptance requests `SFX_VALIDATION`, calls `sub_64A8` with zero and restores typewriting. | Text 510/511 source tokens and text returns are supported. Their exact W2 accepting reads are not observed by this W1 instrumentation; a W2 loop-entry callback is not its later input read. |
| Plain JOIN input | `input.asm:WaitForPlayerInput` tests input before its wait; accepted input adds no W1/W2 preamble or accepting-poll tick. | [Natural JOIN evidence](../../research/map3-messenger-acceptance.md#natural-join-audio-and-input-boundary) binds the named helper after music handling; audio end/interleaving and complete service gates remain separate. |

JOIN text 446/447 has no W1/W2 token. Its caller supplies the separate plain input helper after
`FadeOut_WaitForP1Input`'s music handling. Do not add a text acknowledgement before that helper,
apply W1's RNG preamble to it, or infer audio completion from entry into a text wait.

### Logical-input ownership and implementation gap

Under [ADR 0010 Option A](../../decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)
and the [continuous Wait contract](map3-battle01-continuous-scenario.md#evidenced-gameplay-waits),
the accepting W1 poll is mandatory work for its consumed acknowledgement. An additional
nonaccepting poll may be a player Wait only at the identified, eligible consumer with admitted
service gates/order. Reveal-only input, presentation delivery completion, the accepting poll and
an optional nonaccepting Wait are distinct events. Delivery latency supplies no extra polls or
tick debt and cannot erase mandatory work. These rules specify no wall-clock rate or historical
poll quota; they preserve shared RNG and all selected gameplay assertions.

**Confirmed (bounded remake implementation):** ordered W1 spans in the suppressed entity-event
consumer and the explicit [bound field-text profile](../../../remake/docs/application/session-and-programs.md#bound-field-text-work)
now own their accepting draw/copy/service/input sequence. The latter also executes regular glyph,
window, view-helper and W2 work, independently of actual display delivery. Generic unbound
`ShowText` still uses `DialogueWait`; it does not establish original timing. JOIN emits
`waitForAcknowledgement: false` and later `wait-text-input`; its input-first helper remains
separate. The completed finite-JOIN diagnostic recorded5,313 service results and572 minimal
LCG-equivalent advances during audio delivery, with per-entity attribution and a universal
cross-clock schedule still **Unknown**; preserve the
[diagnostic owner](../../../remake/docs/evidence/retained-comparisons.md#retained-first-control-opportunity-alignment).
The earlier5,000-frame timeout is a completed failure, not an interrupted run to restart.

**Confirmed (selected controlled opening):** the
[bounded R1 readback](../../research/map3-messenger-acceptance.md#controlled-opening-scalar-readback)
observes mouth0/view override0 before input and at their first applicable source readers, before
the script-return view clear. This supplies direct evidence for those controlled admission values.
**Inferred (natural ancestry):** preceding StartWitchScreen/reset ancestry remains outside that
observation; the earlier speed2/settings construction and later saved-byte corroboration retain
their [binding evidence](../../research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding).
**Unknown:** complete original portrait/service timing outside that profile, hardware presentation
and whole-route9A/H4 remain open. A speaker hint, portrait identity or cleared typewriting byte
alone cannot admit an active or unknown portrait. The source-bound entity-event rules below
admit a registered portrait explicitly. Remake tests/native observations prove the consumer,
not additional original runtime observations.

Later behavior acceptance must use an admitted source-driven consumer and check different seeds
and actor phases, enabled/disabled services, zero/one/multiple additional Waits followed by the
mandatory accepting poll, stale tokens/revisions, and multiple wait tokens in one text. Compare
identical semantic Wait/ack streams under instant/adjustable reveal and reveal-only Confirm;
state/RNG must agree while actual presentation conditions still hold. Keep W2 and plain input
cases distinct. No existing failure, golden or unresolved first-warp timing field is waived here.

### W2 composed semantic acceptance

The current semantic obligation can be established by the conjunction of independent original
handler rules, evidence for the applicable caller/service gates, and actual token/input/service/
indicator/validation-sound/continuation observations. A direct historical internal accepting-read
callback is not a prerequisite for that composed proof. It remains a separate **Unknown**;
text510/511 returns and `loc_6472` entry must never be relabelled as the later input read. If a
missing caller or gate can change the logical result, keep that occurrence **Unknown** rather than
inferring it from a text ID or a passed test.

**Confirmed (static source):** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`textfunctions_1.asm:@wait2/loc_6472/sub_64A8`, establishes the draw/copy → indicator → wait →
masked input read sequence above. The indicator starts at20, is visible for counter>=7, decrements
and resets at0; HIDE_WINDOWS forces its hidden branch. Acceptance requests validation67 and clears
the indicator before token resumption. The indicator helper has no extra wait or random draw.
The entity wrapper's CLEAR_FLAG suppresses entity updates, while
`code/common/tech/interrupts/trap6_mapscript.asm:Trap6_TriggerAndExecuteMapScript` reactivates them
before executing a nested script. Its return does not undo that activation. Consequently an
EntityEventContext alone is insufficient to decide whether NPC services run.

**Confirmed (bounded actual consumers):** the retained successful default-keyboard A cohort
(`local/issue534/caller-resource-cohort-01/variant-A-02/actual.json`) contains these16 W2 accepts.
Program positions are admitted content cursor positions; token/ordinal values identify evidence,
not production admission rules:

| Source caller/content | Reached W2 text IDs | Actual entity services and caller |
| --- | --- | --- |
| map03 `scripts_1.asm:cs_5145C` | 510,511 | Enabled zone caller; closed portrait |
| map03 `s2_entityevents.asm:Map3_EntityEvent0/15` | 512,500 | Suppressed entity wrapper; registered open portrait |
| map03 `s3_zoneevents.asm:byte_50E96` and `scripts_1.asm:cs_5149A` | 514,515,517,518,526 | Enabled zone caller; registered open portrait |
| map20 `s6_initfunction.asm:cs_53996` | 2193 | Init script through Trap6; enabled, registered open portrait |
| map19 `s2_entityevents.asm:Map19_EntityEvent12/cs_52F0C` | 575 twice,576 | Entity wrapper followed by Trap6 activation; enabled, registered open portrait |
| battle01 `cs_beforebattle.asm:bbcs_01` | 2292,2299,2303 | Enabled before-battle script; registered open portrait |

All16 accepts have an eligible delivered FieldTextWait, actual keyboard Confirm, exactly one
`rng-text-w2`, one accepting service opportunity, `text-w2-input(accept)`, `text-w2-accepted`,
and a new continuation token. The14 deliberate neutral polls remain distinct. The16 matching
validation67 receipts report real playback `started`, joined by session and the **whole submit
result** revision. `ExplorationSessionView.Send` requests the sound after Submit returns; its
receipt revision can therefore follow the internal accepted observation. Requiring equality to
that internal event revision would discard a valid receipt.

The retained same-submit samples show indicator0/hidden for14 accepts. The final two before-battle
accepts have their next text2300/2304 input samples showing indicator0/hidden; they do not record
an intermediate same-submit projection. The explicit acceptance event, production clear/resume
behavior, and subsequent hidden state compose the semantic boundary; an exact intermediate host
draw/time remains **Unknown**. All selected ready states have HideWindows=false/Scrolling=false;
changed view/indicator states retain their separate engine behavior coverage.

For offline review, select only W2 token/input/result records and surrounding relevant states from
the existing A cohort, and reuse its already retained audio receipts. Preserve original array
indices and source metadata. The bounded selection is retained at
`local/issue534/closure-preflight-01/current-A-w2.jsonl` with its selection receipt and four late
context samples in `current-A-w2-tail.jsonl`; `semantic-closure-01/w2-occurrences.json` binds the
16 accepts. No new original or complete route run is required to reproduce those selected facts.

`ExplorationTextWaitTests.PollCopySurvivesNpcThenBlinkAndMouthDraws` exercises both neutral and
accepting polls with enabled NPCs and simultaneous blink/mouth draws in zone/entity contexts.
It independently checks the poll copy, final shared seed, single service/draw, indicator clear and
continuation. Existing suppressed-service, indicator-cycle, raw-wrapper and delivery tests cover
the complementary cases. Reproduce using the configured SDK and Engine.Tests filter
`FullyQualifiedName~ExplorationTextWaitTests`. Tests prove engine behavior; the cohort proves
actual consumers. Neither creates a new original runtime observation or clears unrelated H4 rows.

## Bound neutral text delay and narration

**Confirmed (static source):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/common/scripting/text/textfunctions_1.asm:symbol_delay1/loc_65CC/loc_65D8/loc_65F0`
loads counter21, saves and clears `CURRENTLY_TYPEWRITING`, performs `WaitForVInt` followed by DBF
under neutral input and mouth-control0, then restores the saved value. This means22 source wait
services, not22 independently measured natural hardware frames. Nonzero mouth-control skips the
waits; masked held input can shorten the source loop.

The bound remake consumer MUST preserve D1 as a typed token, without a glyph, cursor advance,
first-glyph change, W1/W2 draw/copy or acknowledgement. Its neutral delay MUST execute22 existing
common services with typewriting cleared and restore the actual saved logical flag before the
next token. Nonzero mouth-control contributes no delay services. Token order, repeated/end/around-
input placement, eligible entities/portrait RNG and single/batched service equivalence retain their
existing owners. Reveal completion and reveal-only Confirm MUST NOT substitute for source held
logical input. Held-input shortening remains unadmitted by this modern consumer; D3 and other
unbound controls remain Unsupported. The legacy suppressed W1 shortcut cannot consume D1 by
ignoring its services.

**Confirmed (static source):** `mapscriptengine_2.asm:csc00_displaySingleTextbox` skips portrait
lookup for operandFFFF, writes `CURRENT_SPEECH_SFX=0`, then performs view wait/text and the ordinary
close/Sleep10 tail. Explicit sourceFF/null/no-event-speaker narration MAY use bound text without an
actor; arbitrary missing/invalid actors remain errors. A skipped lookup MUST preserve an already
open portrait rather than prove it absent. Actual speech must remain silent even with a retained
portrait; explicit source closes still own its lifecycle. [Bound engine execution](../../../remake/docs/application/session-and-programs.md#bound-map-initialization-lifecycle)
and [native acceptance](../../../remake/docs/verification/field-programs.md#bound-map-initialization-observation)
exercise the admitted consumer. Original natural cadence, speech scheduling and whole-route H4
remain **Unknown**.

## Entity-event portrait service

**Confirmed (static source):** at SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`portraitwindow.asm` opens before the entity wrapper's facing wait; registration follows movement
completion and removal precedes closing movement. An already-open portrait is retained. Fresh
counters are blink20/mouth6. `portraitfunctions.asm:VInt_PerformPortraitBlinking` updates eyes at
blink3/0 and draws main RNG120+30 at0. Typewriting advances mouth, selecting alternate tiles at5
and resetting normal tiles with RNG5+10 at0; without typewriting, mouth<=5 resets immediately.
Blink precedes mouth. Typewriting and mouth-control input shortening are independent gates.
`textfunctions_1.asm:DisplayText` sets typewriting only after `CreateDialogueWindow` returns.
Fresh-window clear/move waits MUST preserve the incoming value, while blink and the existing
non-typing mouth<=5 reset remain active. A reused window returns immediately; empty/W-only text
can set then clear typewriting without an intervening service.
The [research binding](../../research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
owns exact symbols, source order and the reached example.

An admitted implementation MUST preserve the single live portrait identity, packed placement and
mirror flags, eye/mouth tile mappings, registration lifetime and shared RNG effects. Unknown
portrait state cannot become Closed or a static face as a fallback. W1/W2 copy the poll result
before enabled NPC/portrait services; later draws MUST NOT overwrite that copy. Opening/closing
movement requests the existing menu-switch65 cue. This establishes no new speech policy.

The live entity-enable flag changes at wrapper entry/suppression and Trap6 activation; it MUST
survive a script return. Source map-script end conditionally waits for view while dialogue exists,
then clears the override; native subroutine returns remain distinct. Wrapper return restores
facing, removes/closes portrait, closes dialogue, and finally releases control with entities enabled.
If no script activated entities, suppression persists through the close tail.

**Confirmed (bounded remake):** the [entity-event consumer](../../../remake/docs/application/session-and-programs.md#bound-entity-event-portrait)
uses those rules for supported live state/content and reaches Sarah's first classroom caller
return under equal semantic inputs across three display settings. The source-zone section below
admits the first introduction separately. **Unknown:** subsequent zone portrait,
camera, JOIN and battle consumer service schedules, original DMA/presentation timing and the
existing whole-route acceptance gaps. This admission adds no original runtime observation.

## Source zone caller through return

**Confirmed (static source):** the pinned `ProcessMapEventType6_ZoneEvent` installs player
`eas_Init` and immediately calls `RunMapSetupZoneEvent`. The request is produced during the
player entity service; the remaining entities complete their pass before the handler runs.
Entity obstruction precedes the request; map passability follows it. A blocked map marker can
therefore call its handler while an entity-obstructed target cannot. Init installation MUST
preserve current position, destination, velocity and timer; its actions execute on later services.
The [spatial owner](../../research/map3-controlled-start-egress-transition.md#source-zone-request-order)
records the source symbols and legal staircase example.

The zone caller MUST remain distinct from entity interaction: no interaction facing/suppression
or facing restoration is implied. Carry the actual live entity-enable flag through native code
and script calls. Reuse the admitted portrait/text/view work, including incoming typewriting,
registered portrait retention, Unknown rejection and poll-copy preservation.

After the handler returns, close/remove portrait, then close dialogue, then perform one
unconditional VInt opportunity. Only afterward compare the player's physical X/Y with its
X/Y destination; repeat VInt/recheck while unequal. Script-idle and aggregate Busy MUST NOT
replace that predicate. An empty/skipped handler or already-arrived player still owns the
unconditional opportunity. Ordinary field control resumes only after this caller tail completes.

**Confirmed (bounded remake):** the [source-zone consumer](../../../remake/docs/application/session-and-programs.md#source-zone-caller)
uses one live caller authority and reaches the first Astral introduction return from the retained
bound start across three display settings. This includes the earlier opening zone's same rule.
**Unknown:** later zone/camera/choice/JOIN/battle consumer schedules and original hardware timing.
No new original runtime observation is implied.

## Remake Boundary

A remake may model command decoding, dialogue-line selection, name-index substitution, portrait lookup,
and presentation scheduling as separate services. It must preserve the confirmed command/state order
and the bounded input-consumer rules above when that consumer is admitted. Rendering and host timing
choices remain subject to accepted 8D/9A semantics; they do not authorize dropping gameplay-affecting
poll work or inventing a scheduler where the caller/service binding is still Unknown. The handler-local
matrix alone supplies no additional whole-consumer or rendered-fidelity guarantee.
