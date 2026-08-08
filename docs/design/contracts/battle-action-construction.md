# Battle Action Construction Contract

- **Confirmed original behavior:** the complete 29-file battle-action inventory, top-level action
  construction order, physical-route branch order, item use/break routing, Taros gate, target sort,
  message-command buffer shape, complete 54-site message-macro corpus, and bounded dynamic message-ID
  domains described below.
- **Unknown original behavior:** animation and message timing, normal caller reachability and
  frequency of message candidates, item-break probability meaning beyond the RNG result gate,
  unmodeled ailment/special helper behavior, and reachability of explicitly unused/null helpers.
- Remake status: implementation-neutral Phase 3 contract; no action-graph architecture, presentation
  system, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract owns the transformation from a committed battle action plus target inputs into an
ordered battle-scene command stream and transient reward/action state. It defines:

1. accumulator initialization and action-family routing;
2. target sorting and per-target processing order;
3. the physical route's branch chronology;
4. item-to-spell delegation and post-use break/consume gates;
5. Taros and Burst Rock special construction boundaries;
6. the physical shape and static source corpus of battle-scene message commands.

It does not own how the player or AI selects an action, how legal targets are formed, the detailed
damage/spell/reward formulas, persistent scene replay, rendered presentation, or battle outcome.
Those remain with the [battlefield-navigation contract](battlefield-navigation.md),
[combat-resolution contract](combat-resolution.md), [spell-resolution contract](spell-resolution.md),
and [battle-control lifecycle contract](battle-control-lifecycle.md).

The executable owner is `sf2-battle-actions-static-v1` in
`tests/fixtures/h2/battle-actions-static-v1.json`. The research owner is
[battle action script construction](../../research/battle-actions.md). This is a static source/ROM
contract. Existing H3 fixtures close downstream formulas and replay only at their own seams; this
document does not aggregate them into a new runtime golden.

## Canonical Construction State

An implementation MUST distinguish these state domains:

| State domain | Contract role |
| --- | --- |
| committed action | actor, action family, item/spell selector, and selected target inputs received from player/AI control |
| target list | ordered combatant bytes and action-family-specific temporary sort metadata |
| transient accumulators | EXP, gold, attack type, and action flags cleared or updated while constructing the scene |
| scene command buffer | ordered word commands and parameters consumed by the separate battle-scene interpreter |
| persistent combatant state | HP, MP, status, item, EXP, and gold results applied by downstream replay owners |

The builder is not the renderer and its temporary calculations are not automatically persistent. A
remake that mutates final combatant state directly while choosing an action cannot demonstrate this
contract without an adapter that reconstructs the same ordered boundary.

## Top-Level Action Pipeline

**Confirmed static order:** `WriteBattlesceneScript` first clears EXP, gold, attack type, and
transient action flags. It then constructs targets according to one of six source-owned action
families:

- physical attack;
- cast spell;
- use item;
- Burst Rock;
- muddled action;
- prism laser.

After target construction, the engine always sorts the target list. Each target is then processed
in this order:

1. switch current target state;
2. apply the action effect;
3. process the enemy-item-drop seam.

After all targets, the engine performs:

1. actor idle;
2. used-item break/consume handling;
3. double-attack validation;
4. counterattack validation;
5. optional Burst Rock explosion re-entry;
6. script termination.

Burst Rock explosion re-enters target and action setup before the final end. The ordered list is a
construction contract, not a claim about frame timing, animation duration, target-selection intent,
or which natural callers reach each family.

## Physical-Route Chronology

**Confirmed static:** the physical action route attempts these stages in order:

1. dodge;
2. base damage;
3. critical determination;
4. damage application;
5. ailment handling;
6. curse damage;
7. double/counter determination.

The source-confirmed early exits are equally important:

- a dodge bypasses base damage, critical, damage application, ailment, and curse handling;
- a direct lethal hit exits before ailment, curse damage, and follow-up construction;
- lethal curse damage exits before double/counter construction.

The detailed arithmetic, HP clamps, dodge/critical meaning, reaction replay, reward booking, and
follow-up validation remain owned by [combat resolution](combat-resolution.md). This contract
preserves how those owners are sequenced; it does not duplicate or broaden their runtime cases.

## Item Use and Break Routing

**Confirmed static:** using an item unpacks the item definition's spell index and spell level, then
delegates to the ordinary cast-spell route. Post-use handling separates equipment from ordinary
items:

- non-equipment is consumed after use;
- equipment enters the break path only when it is breakable and used by an ally;
- already-broken equipment is destroyed;
- fresh breakable equipment calls the shared random/debug generator and breaks only when its result
  is zero.

The zero-result gate is a fidelity fact. It does not establish a player-facing probability without
the owning RNG input/range/caller state, nor does it establish animation, message timing, economic
value, or intended risk. Item ownership, inventory capacity, Deals, and service behavior remain with
their separate contracts.

## Taros and Burst Rock Gates

### Taros

**Confirmed static:** the ineffective-attack gate is battle-specific. Only an ally physical attack
against Taros with the Achilles Sword sets transient flag 112 and avoids the ineffective toggle.
Other attacks against Taros in that battle are marked ineffective. The transient flag is cleared
before reevaluation.

This does not establish normal-story reachability, player knowledge, rendered feedback, or a general
"boss weakness" system. A remake MUST keep this exact conjunction separate from generic resistance
or damage formulas unless it records a deliberate design change.

### Target Sort and Burst Rock

**Confirmed static:** the primary target sort orders unsigned combatant bytes ascending. Burst Rocks
receive a temporary sort bit that places them after ordinary targets. A secondary pass orders marked
Burst Rocks with higher HP before lower HP, leaving the weakest later, then clears the temporary sort
bit from every entry before returning.

The sort controls construction order. It does not, by itself, define target-selection intent,
simultaneous-effect semantics, visible animation order, or persistent mutation order outside the
downstream replay contract.

## Battle-Scene Message Command Shape

The source macros `displayMessage` and `displayMessageWithNoWait` emit command words `0x10` and
`0x11`. The independently parsed battle-scene dispatcher maps them to
`bsc10_displayMessage` and `bsc11_displayMessageWithNoWait`.

Both macros emit six runtime words, or 12 bytes, in this order:

| Word | Runtime slot |
| ---: | --- |
| 0 | command |
| 1 | message ID |
| 2 | combatant |
| 3 | item or spell |
| 4 | reserved zero |
| 5 | number |

`writeBscParam` contributes one runtime word. Its address-register source form emits two byte writes;
the ordinary source form emits one word write. These assembled 68000 instruction forms MUST remain
distinguishable from the 12-byte runtime command-buffer record.

Command `0x10` versus `0x11` is a static dispatcher distinction. The macro names do not prove the
observed wait, input, service-completion, or presentation behavior; those remain **Unknown** until the
queued grouped runtime observation exists.

## Complete Message-Site Corpus

**Confirmed static corpus:** across all 29 battle-action files, there are 54 direct message-macro
uses in 11 positive and 18 zero-use files:

- 49 `displayMessage` sites;
- 5 `displayMessageWithNoWait` sites.

All 54 source forms bind to the pinned H1 listing. Forty-three operands are immediate
`#MESSAGE_*` symbols resolved against the contiguous 4,267-line ID domain. The remaining eleven
dynamic operands have finite, source-derived candidate domains:

- four action-message sites cover attack type, spell selector, muddled bounded offset, and prism
  laser selector;
- damage, death, and spell routes retain their source-derived assignment sets;
- the two item-break callers pass break (`d0=0`) or destroy (`d0=1`) into the 25-row
  `itemBreakMessage` table.

The item-break table plus its `0xFFFF` sentinel occupies the verified H1 range and gives ten message
candidates at each of the two callers. Across the dynamic sites the fixture records 56 candidate
occurrences and 56 distinct in-domain line IDs. The tracked contract retains symbols and numeric IDs
only; it does not decode or reproduce copyrighted dialogue text.

These candidate domains are static possibility bounds. They do not prove normal caller
reachability, frequency, chosen text in a particular playthrough, portrait/layout behavior, or
rendered timing.

## Spell-Selector Source Order

The message selector first compares the battle-scene spell index for Spoit through Atlas, then
reloads `BATTLEACTION_OFFSET_ITEM_OR_SPELL(a3)` specifically for Aqua and Aqua level 2 before the
default assignment. An adapter that exposes source-compatible message selection MUST preserve this
selector/default/override order at the fixture seam.

This is not evidence that every selector is naturally reachable or that a message name describes
the complete spell effect. Spell resolution remains owned by the dedicated contract.

## Unused and Presentation Helpers

`nullsub_BBE4` plus the source-owned sleep/NOP helper file are inventoried but not claimed reachable.
Action/death message, spell/physical animation, curse-damage, and ailment helper entries have H1-bound
footholds; their deeper presentation and unmodeled sub-routes are not promoted by this contract.

An implementation MUST NOT create required gameplay behavior solely because an unused helper has a
source label or address. Conversely, omitting a reachable presentation effect cannot be justified by
the static inventory alone; that question remains **Unknown**.

## Fidelity and Modernization Boundary

An original-fidelity action builder MUST preserve:

- distinct committed-action, target-list, accumulator, command-buffer, and persistent-state seams;
- accumulator reset, action-family routing, target sort, per-target, and post-target order;
- physical-route early exits;
- item-to-spell delegation and exact equipment break/consume gates;
- Taros conjunction/flag behavior and Burst Rock temporary sorting;
- message command words, six-word layout, parameter emission units, and source-derived candidate
  domains.

A future remake MAY use typed commands, a data-driven action graph, different internal collections,
parallel visual effects, faster/skippable presentation, richer logs, or redesigned special gates.
Those are product decisions, not original facts. Fixture-visible deviations require an explicit
decision and H4 expected-deviation coverage.

## H4 Acceptance Surface

A future H4 adapter should consume `sf2-battle-actions-static-v1` and compare:

1. initial accumulator state for every supported action family;
2. sorted target bytes and cleared Burst Rock sort metadata;
3. ordered per-target and post-target event traces;
4. physical early-exit traces without reimplementing downstream arithmetic in this adapter;
5. item delegation and break/consume branch results for source-shaped inputs;
6. Taros conjunction/flag results;
7. exact message command records and static candidate-domain selection.

Downstream HP/MP/status/EXP/gold/item persistence MUST be compared with the combat, spell, and replay
fixtures that own those results. H4 MUST NOT substitute one aggregate battle golden for these
separate boundaries.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| complete inventory, action pipeline, physical order, items, Taros, target sort | **Confirmed static** | `sf2-battle-actions-static-v1` ([`battle-actions-static-v1.json`](../../../tests/fixtures/h2/battle-actions-static-v1.json)) | Natural caller reachability, unmodeled sub-routes, timing/presentation |
| message macros, dispatcher join, layout, 54-site corpus, candidate IDs | **Confirmed static** | same `sf2-battle-actions-static-v1` executable owner | Runtime wait/input/completion behavior, frequency, rendered text/layout |
| physical arithmetic and persistent replay | **Separate confirmed subsets** | [combat-resolution contract](combat-resolution.md) | Its itemized **Unknown** boundaries; no aggregate completion claim |
| spell/item effects and status/reward replay | **Separate confirmed subsets** | [spell-resolution contract](spell-resolution.md) | Unsupported spells, natural multi-target order, complete encounter state |
| action choice, target intent, tactics, presentation, outcomes | **Unknown / separate owner** | No aggregate executable owner | AI/player control, renderer, battle controller, and future product decisions |

## Reproduction

```powershell
uv run sf2 h2 battle-actions
uv run sf2 design-contracts test
uv run sf2 research-index test
```
