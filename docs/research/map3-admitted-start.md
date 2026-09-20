# Map 3 Admitted-Start Evidence

## Scope and provenance

This owner records one **controlled** continuity seam for R1 of the ADR 0009
Map 3 through Battle 01 audit. It is not a claim that a player naturally
reaches Map 3 by this route, nor a substitute for R2's route evidence.

- Canonical private input: US ROM SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source baseline: `SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Runtime: BizHawk 2.11.1 / Genesis Plus GX, one deterministic launch of
  `uv run sf2 h3 map3-admitted-start --timeout-seconds 180`.
- Public evidence: fixture state facts, source/ROM hashes, and trace shape
  only. The ROM, session ROM, SRAM, captures, and emulator artifacts stay
  ignored under `local/`; no asset or capture payload is tracked.

The fixture is
[`map3-admitted-start-v1.json`](../../tests/fixtures/h3/map3-admitted-start-v1.json).
Its verifier independently derives the source, H1, and ROM seams before
launch; the Lua config contains no accepted observation or golden corpus.

## Controlled admission boundary

**Confirmed:** one case, `controlled-new-map3-default`, reaches the first
original `WaitForEvent` after this exact observed chronology:

```text
CheckSram (0x006EA6)
→ witchMenuAction_New (0x007406)
→ NewGame (0x009736)
→ SaveGame (0x006F6A)
→ MainLoop (0x0075C4)
→ ExplorationLoop (0x0257C0)
→ RunMapSetupInitFunction (0x0474FC)
→ setup-resolution return (0x047504)
→ original indirect init call (0x047512)
→ ms_map3_InitFunction (0x051382)
→ original indirect init return (0x047514)
→ WaitForEvent (0x02591C)
```

The harness reaches the original `CheckSram` entry, replaces only that call's
post-return continuation with a generated work-RAM checkpoint, and then admits
one original Witch/New action. Its menu returns are controlled to
`initial=1`, `difficulty=0`, with save flags `0`; name/text waits are
session-only return seams. `NewGame`, `SaveGame`, `MainLoop`, `ExplorationLoop`,
the setup wrapper, and the selected init call/return remain original code and
are callback-observed. This is therefore **Confirmed** as a controlled seam,
while normal title/menu input, naming, text pacing, and natural story reachability
remain **Unknown**.

## Admitted snapshot and setup join

**Confirmed:** at the first observed exploration wait, `CURRENT_MAP` and
`EGRESS_MAP` are both `3`; the original MainLoop handoff registers are
`D0/D1/D2/D3/D4 = 3/56/3/3/1`. The fixture records a bounded admission
projection: player entity `(x=0x5400, y=0x0480, facing=3)`, current gold,
difficulty flags, all 30 joined/active bits, all 30 ally records including
class/level/current-and-max HP/MP, and source-faithful current battle fields:
HP values are words, while MP, attack, defense, agility, and move are bytes;
it also records four consecutive item-storage bytes, four spell bytes, and RNG/time state.
The item projection is not four complete two-byte item slots. Status effects and
internal pending-state fields are not emitted in this projection. `RANDOM_SEED`
is observed as its exact four-byte span. `FRAME_COUNTER` and
`SECONDS_COUNTER_FRAMES` are each one byte, while `SECONDS_COUNTER` is a
longword. The snapshot and restoration proof preserves those four spans
separately; it does not sweep the unrelated bytes between the frame counter
and RNG storage.

**Confirmed (controlled normalization):** VInt can advance its byte and
longword counters between otherwise identical launches. The observer reads the
three original spans at the first wait, then, outside every callback and only
after that original boundary, writes and readbacks zero to those three exact
spans before emitting the public fixture. The subsequent scoped core-state
restore proves the session returns to the pre-write state. The golden's zeroed
time values are therefore an explicit controlled normalization, not a claim
that the raw VInt values are fixed or a natural-game time snapshot. The pinned
`vint.asm` byte/long updates, longword timer-window read, and H1/ROM opcode
guards define the widths;
the original RNG generator's word update remains distinct from the separately
observed four-byte RNG span.
In this controlled snapshot only ally 0 is joined and active; the exact values
are fixture-owned rather than duplicated here.

**Confirmed:** the static sourcePath corpus contains exactly 26
`data/maps/entries/map03/*` records. The observed state has every guarded
selector flag clear, so the source-ordered Map 3 selection chooses the default
setup at `0x050AE8`; its original init pointer is `ms_map3_InitFunction` at
`0x051382`. During that selected init execution, callbacks fail closed on both
guarded script targets and the guarded `MoveEntityOutOfMap` helper entry; none
occur before the first `WaitForEvent` (`programRequest = "none"`). The verifier
checks the default and flag-506/543/609 selector rows from the static source,
but does not claim the latter rows were traversed.

**Unknown:** the narrative meanings and later effects of flags 506/543/609,
normal route selection, dialogue/menu/event chronology, persistence, rendered
output, audio/timing, and any map-to-battle consequence. R2 owns natural route
evidence; this slice neither associates the 26 Map 3 records with design
contracts nor promotes a controlled seam into natural reachability.

## Static completion of the admission projection

**Confirmed (static, 2026-09-19):** these deductions concern the observed controlled
suffix, with the pinned original code and the three session patches unchanged.
They are not new runtime fields. Restoring complete ally records does not reveal
an omitted byte's value. All source paths below are relative to `disasm/` at the
[pinned SF2DISASM revision](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2const.asm` and `sf2enums.asm` own addresses, offsets and constants.

### Ally status: a conditional value, not an implicit reset

**Confirmed (static):** status is a **word** at
`COMBATANT_DATA + 56*i + 44`, for initialized allies `i=0..29`.
The base is `0xFFE800`; the first span is `0xFFE82C..0xFFE82D`, the last
`0xFFEE84..0xFFEE85`. Under `code/common/stats/`,
`combatantstats_1.asm:GetStatusEffects` and `combatantstats_2.asm:SetStatusEffects`
select offset 44 and call `combatantstats_3.asm:GetCombatantWord/SetCombatantWord`,
whose actual accesses are `move.w`. Offset 52's activation word is a distinct
battle-facing field; it is not status and need not be zero to admit exploration.

| Reached stage / source owner | Status write boundary |
| --- | --- |
| `code/common/stats/newgame.asm:NewGame` (`0x9736`) | Counter 29 with inclusive `dbf` initializes selectors 0–29. InitializeGameSettings clears flags and selected globals, **not** the ally record area. InitializeAllyCombatantEntry writes name, class/level, four **word** item slots and spells, then calls LoadAllyClassData, InitializeAllyStats, UpdateCombatantStats. |
| `code/common/stats/levelup.asm:InitializeAllyStats/LevelUp` | Initializes starting HP/MP, growth, level and spells; it does not reset status. |
| `code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` (`0x89CE`) | Reads status, masks with `0xFFFB` (all bits except CURSE), applies current-stat effects and visits four item words. ApplyItemOnStats returns the cursed-type test for an equipped, non-NOTHING item; the caller adds CURSE (`4`), then writes status. STUN/POISON survive. |
| `code/specialscreens/witch/witchstart.asm:witchMenuAction_New` → `code/common/tech/sram/sramfunctions.asm:SaveGame` (`0x6F6A`) → `code/gameflow/mainloop.asm:MainLoop` (`0x75C4`) | Name/DisplayText/menu retain the controlled seams. Configuration changes named mode bytes and SAVE_FLAGS; difficulty changes F78/F79. Save copies **from** COMBATANT_DATA to SRAM and updates SRAM checksum/SAVE_FLAGS, not RAM status. MainLoop reaches exploration, not BattleLoop. |
| `code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` → `code/gameflow/battle/battleloop/heallivingandimmortalallies.asm:HealLivingAndImmortalAllies` (`0x23BFC`) | Visits all 30 without a joined/active filter. Peter/Lemon always take healing; other allies do so when HP is nonzero. Restores HP/MP, masks status with `7` (STUN/POISON/CURSE), then UpdateCombatantStats recalculates CURSE. All 30 R1 allies have positive observed HP. Entity/window/map loading, the selected no-script init and its base VInt services contain no later ally-status writer before this wait. |

The first 30 records of `data/stats/allies/allystartdefs.asm`, joined to
`data/stats/items/itemdefs.asm:itemType`, contain **no equipped cursed item**.
Lemon's DARK_SWORD is carried without EQUIPPED. This uses the full source start
definitions, not the fixture's four-byte item projection. Thus, if `S_i` is
status at the controlled NewGame entry:

```text
status_at_first_wait[i] = S_i & 0x0003      (STUN=1, POISON=2)
```

All other status bits are zero by this chain. A different caller with a dead
non-immortal ally skips healing; a different equipment set recalculates CURSE
from that set. Neither exception is generalized into R1.

**Confirmed (source plus existing R1 projection):** STUN can be eliminated
without another observation. `InitializeCurrentStats` copies base MOV to current
MOV; `ApplyStatusEffectsOnStats` subtracts 1 only when STUN bit 0 is set.
`combatantstats_2.asm:DecreaseCurrentMov` selects byte offset 25, bounds 0–200,
and `combatantstats_3.asm:DecreaseAndClampByte` implements the subtraction/clamp.
Every observed `allies[i]/move` equals that ally's class MOV (4–7) in
`data/stats/allies/classes/classdefs.asm`. The source starting equipped items
have no INCREASE_MOV/DECREASE_MOV effect. The clamp cannot hide a decrement in
that range. Hence STUN=0 for all 30; the remaining conditional is:

```text
status_at_first_wait[i] = S_i & 0x0002      (POISON only)
```

**Confirmed (static conditional):** `code/gameflow/start/systeminit.asm`
InitializeSystem→InitializeVdp clears `0x3FFF` longwords using counter
`0x3FFE`, covering `0xFF0000..0xFFFFFB`, including every ally status word.
A reset path with no subsequent STUN/POISON writer therefore supplies
`S_i & 3 = 0`. `gamestart.asm:Start` reaches InitializeSystem after both its
hardware-setup and skip branches; this is not just Start's optional earlier clear.

**Inferred:** zero status for the ordinary fresh bootstrap is consistent with
that reset, the observer's held-Start prefix, and
`gameinit.asm:InitializeGame→AfterGameIntro` in `gameintro.asm`, followed by
StartWitchScreen→CheckSram. **Unknown:** public R1 chronology begins at CheckSram
and treats earlier NewGame callbacks as diagnostic no-ops. It does not establish
the complete reset-to-checkpoint writer history or expose the surviving POISON bit. The
admitted suffix cannot turn that bootstrap inference into 30 observed zero
words. Existing MOV closes STUN; the remaining dependency is inherited POISON per ally.

### Route flags through the first wait

**Confirmed (static):** InitializeGameSettings stores zero in 32 longwords
(counter 31, inclusive `dbf`), covering `GAME_FLAGS=0xFFF686..0xFFF705`.
`code/common/stats/gameflags.asm:GetFlag` masks the index with 1023, divides
by 8 and uses `0x80 >> remainder`; Set/Clear change one byte. In addition to
R1's six observed guards, this resolves the following omitted bits at **R1**:

| Flag(s) | RAM byte and mask | Value / boundary |
| --- | --- | --- |
| F604 / F605 / F607 | `0xFFF6D1`, `0x08 / 0x04 / 0x01` | Clear; no gate, bedroom or Astral program has run. |
| F608 | `0xFFF6D2`, `0x80` | Clear; no Astral agreement program has run. |
| F401 | `0xFFF6B8`, `0x40` | Clear; no battle-unlock program has run. |
| F256 | `0xFFF6A6`, `0x80` | Clear twice: settings reset, then ClearMapSetupTempFlags clears F256–F383 before init. No entity/zone interaction has run. |
| F501 / F507 / F982 | `0xFFF6C4 / 0xFFF6C5 / 0xFFF700`, `0x04 / 0x10 / 0x02` | These additional castle setup selectors are clear; no writer occurs in this suffix. |

The bounded intervening writer audit is: `battleparty.asm:JoinForce(0)` sets
F0 and JoinBattleParty(0) sets F32; Witch difficulty return zero sets neither
F78 nor F79; SaveGame changes SRAM SAVE_FLAGS rather than GAME_FLAGS;
SwitchMap/CheckBattle read flags. The provided-map exploration branch clears
F256–F383 and sets F80. Map/entity/display loading does not execute entity/zone
interaction handlers. The selected Map3 init tests F1/F602/F603, all observed
clear, so neither script nor MoveEntityOutOfMap runs. Base VInt services update
entities/view/windows, not these story bits; player control/event publication is
not installed until WaitForEvent's body. No listed bit changes after its clear.

Later writers lie outside this interval:
`data/maps/entries/map03/mapsetups/s3_zoneevents.asm:Map3_ZoneEvent4/Map3_ZoneEvent7`
set F604 after their gate script; `data/maps/entries/map20/mapsetups/s6_initfunction.asm`
sets F605 after its script; `data/maps/entries/map19/mapsetups/s2_entityevents.asm` sets F607 after
the Astral script and its `cs_52F40` continuation sets F608. The
[castle static owner](map3-castle-battle-unlock.md) retains F401's tower unlock
and F256's map-local interaction uses: Map21_EntityEvent0 and cs_53EF4 live in
`data/maps/entries/map21/mapsetups/s2_entityevents_506.asm` even for the default
table. cs_53EF4 uses setStoryFlag 1 (F401); the caller commits F256 after return. These future writers prohibit carrying
R1 zeros forward as observations of the R2a endpoint or castle traversal.

### Pending state and inactive consumers

**Confirmed (static):** the sufficient boundary is the first original wait
**entry** after returned init, not every RAM cursor being zero.

| State / source owner | Sufficient invariant; excluded numeric readbacks |
| --- | --- |
| Event/transfer: `explorationfunctions_2.asm:ExplorationLoop/WaitForEvent/ProcessMapEvent` | MAP_EVENT_TYPE is a word at `0xFFA84A`, cleared at exploration entry. `code/common/scripting/map/mapfunctions.asm:InitializeMapEntities` creates player 0 at the supplied position with eas_Idle, unchanged by the selected init. `entityscriptengine_2.asm` publishes warp/zone/vehicle events from control commands 02/07/08 (including WarpIfSetAtPoint), absent from this idle loop. Default NPC init/walking scripts do not execute those commands. Before entity initialization, inherited Witch VInt services are windows/blink; exploration clears those services before creating map entities, then installs base services. No foreground ProcessMapEvent/CheckRandomBattle/script-warp call is reached. Thus event type remains 0. Params 1–5 are bytes at `0xFFA84C..0xFFA850`; only a nonzero event selects their consumers. Old parameter bytes do not imply pending transfer. |
| Map script: `code/common/scripting/map/mapscriptengine_2.asm:ExecuteMapScript` | Cursor A6 is established from A0 inside a saved-register call and restored on return, not a global pending-program queue. The observed init returns without its guarded scripts. Stale A6/text-index scratch does not imply an active program. |
| Windows/modal: `code/gameflow/battle/battlevints.asm:SetBaseVIntFunctions` (`0x25A94`) → `code/common/windows/windowengine.asm:InitializeWindowProperties` (`0x47C6`) | After LoadMap, resets 8 × 16-byte entries (`0xFFA87E..0xFFA8FD`), end pointer to WINDOW_TILE_LAYOUTS (`0xFFB800`), and dialogue/portrait/timer index words to 0. Init creates no new window. VInt_UpdateWindows returns on that end pointer and also skips null layout pointers. MOVING_WINDOWS_BITFIELD need not be zero: the early return precedes its clear. Old animation/layout cursors are not pending work. |
| Text/name/menu: `code/common/scripting/text/textfunctions_1.asm:DisplayText`; R1 session patches | DisplayText is RTS at `0x6260`; NameAlly's alias is RTS; Witch menu returns are controlled. Compressed/ASCII/name pointers and typewriting storage are not executing text consumers here. ProcessPlayerAction/FieldMenu follows a returned wait, not this entry. These seams do not establish visible text, naming or natural modal completion. |
| Input/entity scripts: `code/common/scripting/entity/entityfunctions_2.asm:SetControlledEntityActScript`; `data/scripting/entity/eas_main.asm:eas_Idle` | The player's idle wait/branch cursor and byte timer may vary with VInt. With event=0, WaitForEvent next sets VIEW_TARGET_ENTITY=0 and replaces the idle cursor with the controlled script; it is not already installed at `0x2591C`. VInt_UpdateEntities skips X≥`0x7000` slots before script reads and skips null script pointers. **Active** NPC walking scripts do not satisfy those inactive guards: their position/phase/RNG effects remain live. |
| Battle: MainLoop dispatch and provided-map ExplorationLoop branch | Bypasses BattleLoop and writes CURRENT_BATTLE byte=`255` (NOT_CURRENTLY_IN_BATTLE). Old battle-turn/activation scratch does not select a battle here; later battle admission owns its consumers. |

Raw player coordinates `(0x5400,0x0480)` remain the fixture values.
Source MAP_TILE_SIZE=384 yields tile `(56,3)` for this pair, a derived view,
not another readback. The four-byte RNG span is distinct from its word update.
The three normalized time spans remain controlled zeros, not evidence of equal
VInt phase, NPC timers or natural elapsed time.

### Smallest remaining readback

For the missing **R1 admission** fields scoped here, the remaining numeric
dependency is POISON for each of 30 allies. A future independently admitted
batch needs 30 bit readbacks: low byte at `0xFFE800 + 56*i + 45`, mask `2`
(30 byte reads total; an existing word getter may read the 60-byte word spans).
Collect these at the first wait. No full flags/combatant dump, inactive cursor inventory or
separate launch is needed. If exact active-NPC continuation is required, select
the relevant entity position/destination/script phase and RNG at that same
boundary; R1 does not promise a complete executable save state. Later R2a
readbacks still include reached flags/active-state guards such as F604 under the
[audit proposal](map3-battle01-audit.md). Nothing here admits that observation,
changes replay lineage/budget, or closes natural-route/presentation Unknowns.

### Read-only reproduction

Use `git show` at the pinned revision for every exact source path named above;
inspect complete routines and their calls, including `j_` jump-interface aliases:

```powershell
$pin = 'c834c652b6862bc5679fd7f69a38a7093206efc6'
$upstream = 'local/upstream/SF2DISASM'
git -C $upstream show "${pin}:disasm/code/common/stats/newgame.asm"
git -C $upstream show "${pin}:disasm/code/common/stats/updatecombatantstats.asm"
git -C $upstream show "${pin}:disasm/code/gameflow/battle/battleloop/heallivingandimmortalallies.asm"
git -C $upstream show "${pin}:disasm/code/gameflow/exploration/explorationfunctions_2.asm"
git -C $upstream show "${pin}:disasm/code/common/windows/windowengine.asm"
```

Repeat with the flag, item, reset, entity and text source paths named in the tables.
Without a local checkout, the equivalent read-only retrieval is
`gh api -H 'Accept: application/vnd.github.raw+json'
"repos/ShiningForceCentral/SF2DISASM/contents/disasm/<path>?ref=$pin"`.
Keep saved source in ignored scratch. This slice used pinned-source retrieval;
it did not recreate an H1 build environment.

The existing R1 fixture `/static/function`, `/static/map3`, `/static/ram` and
session-patch original bytes retain the accepted H1/ROM joins. These additional
narrow opcode assertions reproduce status masks and wait/window anchors, after
loading and verifying current private input selection:

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
if ($LASTEXITCODE -ne 0) { throw 'ROM identity verification failed' }
@'
from sf2tool.private_inputs import private_input_path
rom = private_input_path("roms/sf2-us.bin").read_bytes()
checks = {
    0x00842A: "7E2C",       # GetStatusEffects: offset 44
    0x00869A: "7E2C",       # SetStatusEffects: offset 44
    0x009304: "32307000",   # GetCombatantWord: move.w (a0,d7.w),d1
    0x0092EA: "31817000",   # SetCombatantWord: move.w d1,(a0,d7.w)
    0x00984E: "7E1F20C0",   # settings: counter 31; move.l d0,(a0)+
    0x0089DA: "0243FFFB",   # UpdateCombatantStats: remove CURSE
    0x023C3C: "02410007",   # HealLivingAndImmortalAllies: lasting mask
    0x0257C0: "4278A84A",   # ExplorationLoop: clr.w MAP_EVENT_TYPE
    0x02591C: "3038A84A",   # WaitForEvent: move.w MAP_EVENT_TYPE,d0
    0x0047F8: "4278AF6C",   # InitializeWindowProperties: clr.w dialogue index
}
for address, expected in checks.items():
    value = bytes.fromhex(expected)
    assert rom[address:address + len(value)] == value, hex(address)
print("PASS: R1 admission ROM anchors")
# Table bases/ranges: allystartdefs.asm, classes/classdefs.asm, itemdefs.asm.
# Widths/fields: ALLYSTARTDEF_ENTRY_SIZE=6, CLASSDEF_ENTRY_SIZE=5,
# ITEMDEF_SIZE=16, TYPE=8, equip effects start at 10 (three 2-byte pairs).
import json
from pathlib import Path
allies = json.loads(Path("tests/fixtures/h3/map3-admitted-start-v1.json")
                    .read_text(encoding="utf-8"))["expectedObservation"]["records"][0]["scenarioState"]["allies"]
assert len(allies) == 30
for i, ally in enumerate(allies):
    assert ally["id"] == i and ally["hpCurrent"] > 0
    base_mov = rom[0x1EE890 + 5 * ally["class"]]
    assert 4 <= base_mov <= 7 and ally["move"] == base_mov
    start = 0x1EE7D0 + 6 * i
    for entry in rom[start + 2:start + 6]:
        item = entry & 0x7F
        if entry & 0x80 and item != 127:
            definition = 0x16EA6 + 16 * item
            assert not rom[definition + 8] & 0x40  # ITEMTYPE_CURSED
            effects = rom[definition + 10:definition + 16:2]
            assert not set(effects) & {9, 13}  # INCREASE_MOV / DECREASE_MOV
print("PASS: 30 living allies; no equipped curse/MOV effects; observed MOV excludes STUN")

'@ | uv run python -X utf8 -
```

The existing source guard functions in
`src/sf2tool/h3/map3_admitted_start.py` (`_new_game_use_sites`,
`_main_loop_use_sites`, `_exploration_use_sites`, `_map3_init_use_sites`,
`_ram_contract`) also pass against this pinned source. These checks do not replay
R1 or observe intervening instructions. No normal/full suite, emulator, observer
change, trace or hardware gate is required solely by this documentation change.

## Runtime integrity and private-reference foundation

**Confirmed:** callbacks use one deterministic registration per physical PC;
pre-admission bootstrap callbacks are explicitly diagnostic no-ops, while every
post-admission transition is phase-checked. Callback exceptions defer to one
structured failure status outside the memory-callback context. The success
status is one exact, unique ordered sequence through the snapshot, core-state
checkpoint, original call chain, init return, first wait, callback cleanup, and
terminal exit. The passing run ended with `callbacks-cleared:0`, no Lua Console
error, and a deleted disposable session ROM. Failure status is structurally
closed by phase/role, case, PC, callback-count/output cleanup, scoped-restoration
truth, and a typed first mismatch when restoration fails. Restoration is scoped
to game flags, the 30 complete ally records, map/battle bytes, player entity,
gold, the four independently scoped time/RNG spans, and the generated work-RAM
span; it makes no all-RAM, SRAM-persistence, or presentation claim.

This establishes only the reached RA-11 foundation: fixture provenance records
the private ROM hash, pinned source commit, BizHawk/core, controlled input
condition, and first-wait timing condition. It deliberately does not capture or
publish pixels, palette/VDP/DMA state, animation cadence, audio, or any private
payload. Those 8C fields remain **Unknown** until an accepted route/battle slice
defines private capture and comparison conditions.
