# Battle Action Script Construction

- Status: **Confirmed** for the pinned 29-file source inventory, representative symbols/hashes,
  call edges, top-level action pipeline, physical-attack ordering, item-use/break routing, the Taros
  ineffective-attack gate, target sorting, and explicitly unused/null helpers
- Status: **Inferred** for presentation roles still based only on upstream labels/comments
- Status: **Unknown** for animation/message timing, item-break probability semantics beyond the RNG
  call contract, ailment sub-routes not already covered by H3, and reachability of unused helpers
- Evidence date: 2026-08-02
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Scope

The H2 rail inventories all 29 files under `code/gameflow/battle/battleactions`. Fourteen files were
already connected to focused H3 combat/spell/reward contracts; this batch adds static footholds for
the other fifteen and pins the entire directory's file set, hashes, labels, and direct calls. File
reach means an executable evidence connection, not complete semantics for every local branch.

## Confirmed Action Pipeline

`WriteBattlesceneScript` zeroes EXP, gold, attack-type, and transient action flags before building
targets and scene properties. Target construction distinguishes attack, cast-spell, use-item, Burst
Rock, muddled, and prism-laser actions, then always sorts the resulting list. Each target runs
switch-targets, apply-effect, and enemy-drop handling. After the list, the engine idles the actor,
handles used-item breakage, validates double/counter attacks, optionally re-enters setup for a Burst
Rock explosion, and ends the script.

The physical route orders dodge, base damage, critical, damage application, ailment, curse damage,
and double/counter determination. A dodge bypasses damage, critical, ailment, and curse handling. A
direct lethal hit exits before ailment/curse/follow-ups; lethal curse damage also exits before
double/counter. Existing H3 fixtures own the detailed damage, critical, dodge, double/counter, EXP,
spell, and drop semantics; this H2 rail confirms how those pieces are sequenced.

## Items and Special Gates

Using an item unpacks its definition's spell index/level and delegates to the ordinary cast-spell
route. Non-equipment is consumed after use. Equipment must be breakable and ally-used to enter the
break path; already-broken equipment is destroyed, while fresh breakable equipment calls the shared
random/debug generator and breaks only on result zero.

Taros immunity is battle-specific. Only an ally physical attack against Taros with the Achilles Sword
sets transient flag 112 and avoids the ineffective toggle; other Taros attacks in that battle are
marked ineffective. The flag is cleared before reevaluation.

Target sorting first orders unsigned combatant bytes. Burst Rocks receive a temporary sort bit so
they follow ordinary targets; the secondary pass orders marked Burst Rocks with higher HP before
lower HP, leaving the weakest later, then clears the sort bit from every entry.

`nullsub_BBE4` and the sleep/NOP helper file are inventoried but not claimed reachable. Presentation
helpers for action/death messages, spell/physical animation, curse damage, and ailment dispatch now
have H1-bound footholds; deeper behavior remains queued for static parsing before any shared H3 run.

## Battle-Scene Message Commands

**Confirmed:** the two source macros in `sf2battlescenemacros.asm` construct command words `$10`
(`displayMessage`) and `$11` (`displayMessageWithNoWait`). The source-built battle-scene dispatcher in
`code/gameflow/battle/battlescenes/battlesceneengine_0.asm` independently resolves those command words
to `bsc10_displayMessage` and `bsc11_displayMessageWithNoWait`, respectively. Both emit the same six
runtime output words in this order: command, message, combatant, item-or-spell, reserved zero, and number. This is
12 bytes in the battle-scene command buffer. `writeBscParam` emits one runtime word for each supplied
parameter; its `(a` source-form branch is two byte writes, while the other source-form branch is one
word write. The H2 contract separately records the site-specific assembled 68000 instruction bytes so
they are not confused with the runtime command-buffer bytes.

The complete 29-file battle-action inventory has 54 direct uses in 11 positive and 18 zero-use files:
49 `displayMessage` and five `displayMessageWithNoWait`. All 54 source forms are bound in the pinned
H1 listing. Forty-three message operands are immediate `#MESSAGE_*` symbols, resolved through the
parsed `sf2enums.asm` map and checked against the 4,267-line `gamescript.txt` ID domain.

**Confirmed:** all eleven remaining dynamic operands now have finite, source-derived candidate domains:
four `createbattlescenemessage.asm` sites cover attack-type (3), spell selector (9), muddled base-plus-
bounded-offset (10), and Prism Laser enemy selector (2); `inflictdamage.asm` covers the five
attacker/critical/cutoff assignments; `displaydeathmessage.asm` covers the two side assignments; and
`castspell.asm` covers Muddle (1), Desoul (2), and Absorb MP (2). The two `breakuseditem.asm` callers
pass `d0 = 0` (break) or `d0 = 1` (destroy) into `battlesceneScript_GetItemBreakMessage`; its `dodge(a2)`
base selection plus the parsed 25-row `itemBreakMessage` table (`ITEM_` / `ITEMBREAK_` shorthand,
`tableEnd.w` sentinel) gives ten candidates at each caller. The source-derived `(item ID, message
offset)` bytes plus the `FFFF` sentinel exactly match H1 range `$BCF0..$BD24`; the verifier joins the
accepted item-auxiliary owner on pinned provenance, table address, row count, and its consumer rule.
Across all sites that is 56 candidate
occurrences and 56 distinct line IDs; none remains statically unresolved. Candidate records retain the
source symbol when one exists and otherwise only the derived in-domain line ID. The tracked corpus
contains symbols and numeric IDs only; it does not decode or reproduce dialogue text.

The spell selector first compares the battle-scene spell index for Spoit through Atlas, then reloads
`BATTLEACTION_OFFSET_ITEM_OR_SPELL(a3)` specifically for Aqua and Aqua level 2 before its default
assignment. This is a static source-order fact; it does not claim normal-game reachability or rendered
presentation.

## Runtime Question Queue

One future grouped H3 queue remains for normal caller reachability; observed `$10`/`$11` wait, input,
and service-completion behavior; selection frequencies; and rendered text, portrait/layout, timing,
persistence, and caller-visible completion. The static macro names, dispatcher join, buffer layout,
message IDs, operand registers, selector/default/override ordering, muddle bound, and item-break lookup
only bound candidates; they do not establish which normal-game caller paths reach each case, selection
frequencies, or how the battle-scene renderer presents them.

## Reproduction

```powershell
uv run sf2 h2 battle-actions
uv run sf2 research-index test
```

Generated JSON is written only to ignored `local/derived/battle-actions-static.json`.
