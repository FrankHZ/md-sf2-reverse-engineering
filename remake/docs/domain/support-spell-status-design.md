# Sarah's SLOW and Karna's BOOST in battle

**Accepted design, [#1966](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1966).** Sarah's SLOW 1/2 and Karna's BOOST 1/2 use the ordinary Magic command. Use the existing packed status word for duration. Derive current DEF and AGI from earned bases, the counters used at the last stat refresh and equipped items. A failed recast can advance duration without refreshing either stat. This difference can survive into player input and a battle save. High rigor applies because losing it changes later damage, initiative and cold Continue. [Campaign agreement](../application/remaining-campaign-risk-baseline.md) · [Battle save agreement](../application/battle-save-and-continue.md)

## Current reach

The ordinary Magic command offers Sarah’s learned SLOW ranks 1 and 2 and Karna’s learned BOOST ranks 1 and 2 when the encounter supplies their source terms. Engine cases exercise movement, cancellation, per-target results, MP and EXP, then follow effective stats through combat, expiry, death, return and battle Save/Continue. [Magic and per-target behavior cases](../../tests/Sf2.Remake.Engine.Tests/SupportSpellActionTests.cs) · [Engine behavior cases](../../tests/Sf2.Remake.Engine.Tests/SupportStatusCoreTests.cs)

**Confirmed** in the remake at `d45e2764`. Two existing Pacalon castle field slots (v3, Map36) have active PRST40 Sarah with packed spell byte 68 (ID 4, highest rank 2). The independent Chinese party also has active PRST28 Karna with byte 3 (ID 3, rank 1). English chose Eric. To reproduce from either private `issue1955/slots/castle-*.json`, decode the envelope's base64 `payload` as UTF-8 JSON, verify the envelope SHA-256, select `data.allies` by `actor` (`ally-1`, `ally-15`) and check `data.activeAllies`, class, level and spells. Both checksums matched. The pinned `data/stats/allies/stats/allystats01.asm` and `allystats15.asm` PRST lists teach Sarah SLOW at 19/29 and Karna BOOST at 12/30. These field slots establish the learned ranks. Each encounter binds the supported ranks independently. [Recruitment](../content/campaign-characters.md#creed-and-the-mine) · [Pacalon inputs and replay route](../application/pacalon-first-visit.md#reproduction) · [Private bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs)

### Battle26 ordinary input and reproduction

**Confirmed** in the remake with Godot 4.7.2 .NET. Two ordinary Battle26 saves from the same Karna party cover all three learned ranks. The Magic menu showed the selected spell icon, localized name, rank and MP cost. Canceling target confirmation preserved both seeds, queue cursor, placement and actor values. The player then confirmed the cast through the same menu.

| Ordinary input | Visible results and subsequent control |
| --- | --- |
| Chinese road entry, Karna’s BOOST 1 | MP 57→55, EXP 7→17. Bowie and Karna each received AGI then DEF. On the next cast, Bowie and Karna showed ineffective while Gerhalt and Janet gained stats. MP 55→53, EXP 17→27. |
| Same battle, Sarah’s SLOW 1 | MP 56→53, EXP 40→50. Dark Bishop lost 10 AGI and 14 DEF. Dark Sniper lost 13 of each. |
| English round-six suffix, Sarah’s SLOW 2 | MP 16→10, EXP 82→92. Lesser Demon resisted. Dark Bishop and Dark Sniper received the same ordered decreases. Karna’s English BOOST 1 had already resolved on three allies for 14 EXP. |
| Next-round initiative, physical damage and expiry | Round 3 generated altered initiative scores 38 for Bowie and 22 for Dark Bishop. Karna’s BOOST expiry appeared on her own turn. In round 4 Gerhalt hit the still-slowed Dark Bishop at effective DEF 34, HP 39→18. Engine cases isolate the counterfactual damage and initiative inputs. |
| Battle save, cold Continue and EGRESS | At round 2 cursor 15, Bowie retained stored BOOST 3, last-refresh counter 2 and effective DEF/AGI 41/38. A fresh process restored those values and re-saved identical complete v5 battle data. Bowie’s EGRESS debited MP 15→7 before the party returned to usable Map36 and made a v3 field save. |

Use the registered private `handoffs/issue1966/Review.ps1` from the review checkout. Preparation mounts four original spell rasters in a local copy of `issue1941/content` using the pinned source, listing and verified USA ROM. The unchanged English/Chinese name catalogs and other material come from that package. The `cast` and `rank2` defaults copy `issue1955/receipts/zh-CN-road01/battle-ready-slot.json` and `issue1955/receipts/zh-CN-fight01/level-slot.json` respectively. Use a fresh journey name per branch. Run the processes serially.

```powershell
$repo = (Get-Location).Path
$review = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1966/Review.ps1'
& $review -Repo $repo -Mode prepare -Upstream $pinnedSf2Disasm
& $review -Repo $repo -Mode cast -Language zh-CN -Journey review-a
& $review -Repo $repo -Mode cold -Language zh-CN -Journey review-a
$castSlot = Join-Path $repo 'local/issue1966/receipts/zh-CN-review-a-cast/cast-slot.json'
& $review -Repo $repo -Mode effects -Language zh-CN -Journey review-b -Seed $castSlot
& $review -Repo $repo -Mode rank2 -Language en -Journey review-c
```

Each process writes its native exit code and log locally. Check the save receipt and process result separately. The Chinese cast wrote the complete battle slot and acknowledged product Quit, then exited with `0xC0000005` in `GodotObject.Finalize`. The cold, physical/expiry and English rank-2 processes exited 0. Cold Continue also emitted an ObjectDB leak warning. The shutdown cause remains **Unknown**. [Godot exit investigation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

### BOOST 2 admission and observation

[#2150](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2150) is a Standard-rigor
extension of the existing action. Its acceptance boundary runs from a legally learned private
book through player input, playback and settled Continue. The pinned Karna PRST growth row in
`data/stats/allies/stats/allystats15.asm` teaches packed `0x43` at level 30. The private reader
expands the packed word into ranks 1 and 2 and binds each to its own source definition. The only
runtime additions are the private selection and the Domain tuple for 5 MP, inclusive center
distance 0–2 and radius 2. [Pinned spell definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L123-L136) · [Private bindings](../../src/Sf2.Remake.Content/Scenarios/PrivateSupportSpellBindings.cs)

The target offer and range overlay already read the same admitted rank. Target resolution and
commit validation use the moved caster's square and source processing order. The shared action
captures the original caster word for the single cost, writes the detached duration words, and
refreshes each successful recipient during playback. A failed self-recast restores the old word
at payment before normal after-turn decay. Another active recipient keeps the advanced duration
with the previous effective stats until its next refresh. Same-side successes contribute 5 EXP
apiece before the normal award adjustments. Failed targets contribute zero. A canceled or
unaffordable selection spends nothing. A valid silenced cast pays once without constructing
reactions or EXP, as specified by the shared cost-before-silence rule below. Initiative and
physical reads use effective stats from the same actor, including after v5 restore. Protect
Ring Use selects BOOST 1 explicitly. Enemy books can carry `0x43`. The original SUPPORT gate
rejects it, and the remake's current enemy SUPPORT selector admits DISPEL 1 only.
[Target and effect](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs) ·
[Resolution](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) ·
[Original enemy admission](../../../docs/research/battle/battle-ai.md#unreachable-attack-and-boost-2-support-routes)

**Confirmed** in the remake with Godot 4.7.2 .NET and a controlled Battle26 slot. The Chinese
Magic row showed 强化 2 · 5 MP. Kazin was a legal center two cells west of Karna. The adapter
projected the 13-cell effect diamond. Cancel preserved the queue, positions, RNG and actor
values. Kazin and Peter each received AGI then DEF. Peter was on the radius-two edge. Gerhalt was
outside at distance three. Rhode and Karna showed ineffective. Karna paid 17→12 MP,
earned 48→58 EXP and settled at BOOST count 1 after her turn. The next player input was
Rhode at round 6 cursor 19. Lesser Demon still carried the packed BOOST 2 book.

Save and cold Continue restored Rhode's stored count 3, last-refresh count 1 and effective
DEF/AGI 45/30. Re-saving advanced sequence 8→9 with equal complete decoded v5 data. Rhode's
next ordinary Stay advanced the cursor 19→20, reduced his count 3→2 and refreshed DEF/AGI to
50/33 before Janet's Movement. Both headless processes exited 0. The cold process emitted an
ObjectDB shutdown leak warning. Its cause and natural play from learning the rank to this
positioning remain **Unknown**. The shared cast and status messages are available. Retail
`BUFF1|VARIATION2` presentation remains with [#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).

For reproduction, mount the registered `derived/issue1957/pgnt-v3` world, scenes, entry and
opening-inputs, `derived/issue1957/catalogs-v1` Chinese names and the #1927 Chinese entry
catalog. Use the locked Debug adapter and the #1955 Chinese input settings. Derive a disposable
copy of the #1966 `zh-CN-learned01` Battle26 slot using the [save envelope](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs).
Preserve both live seeds and the save header time. Apply the two PRST growth steps to Karna
using a separate zero RNG word and the mounted growth rows. At level 30 her maxima are
51 HP and 61 MP, with base ATT/DEF/AGI 29/30/30 and equipped ATT 44. Keep current HP/MP.
Update the packed book in the actor and progress loadouts to `[128,142,67,63]` and append
BOOST 2 to the expanded ranks. Place Kazin at (5,24). Give Karna and Rhode stored BOOST counts
2 and 1 respectively, with matching refresh counts. Use v5 with class terms. Swap the actor
identities in queue slots 18 and 20, and 19 and 22, keeping the existing scores and cursor 18.

Reuse the #1966 native driver's Magic selection, target confirmation and scene input helpers.
Continue the disposable slot, confirm movement, open Magic, select BOOST 2, tab to Kazin and
confirm the target. Read the view's action range, cancel at confirmation, reselect and cast.
Acknowledge the results until Rhode has Movement. Save in Options and keep playing. In a fresh
process Continue the same disposable slot, re-save before acting, compare the full decoded
`data` and send Stay. Follow the [native save procedure](../verification/battle-save.md#method-and-reproduction)
for isolated settings and logs.

## Original rules and evidence

**Confirmed** below means static structure at SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` unless a controlled observation is named. Use the [USA retail identity](../../../manifests/roms/sf2-us.json), [fixed disassembly](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) and [spell-resolution contract](../../../docs/design/contracts/battle/spell-resolution.md). Read the following paths under `disasm/` at that commit. For example, from the configured source checkout run `git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battleactions/castspell.asm`. The three linked battle fixtures record their ROM identity, BizHawk 2.11.1 / Genesis Plus GX observation boundary and expected results.

| Cast (packed entry) | MP | Select a center from caster destination | Affected side and inclusive Manhattan radius |
| --- | ---: | --- | --- |
| BOOST 1 (3) | 2 | 0–1, including self | Teammates within 1 of center |
| BOOST 2 (67) | 5 | 0–2, including self | Teammates within 2 of center |
| SLOW 1 (4) | 3 | Exactly 1 | Opponents within 1 |
| SLOW 2 (68) | 6 | 1–2 | Opponents within 2 |

**Confirmed.** `data/stats/spells/spelldefs.asm` gives all four POWER 15, TYPE_SUPPORT and AFFECTEDBYSILENCE. Only BOOST sets TARGET_TEAMMATES. BOOST uses BUFF1 and SLOW uses DEBUFF1. Both rank-2 entries set VARIATION2. `battlefield/buildactionrangegrids.asm:BuildSpellRangeGrid` selects the center's faction and distance. `PopulateTargetsListForSpell → BuildTargetsArrayWithTeammatesOfTarget → PopulateTargetsList` takes the center's side, skips zero HP, neutral and invalid coordinates, and expands the radius using the relative coordinate lists. `battlesceneScript_DetermineTargetsByAction → battlesceneScript_SortTargets` sorts by combatant index (special Burst Rock tail outside this party route). An ally in the diamond can be affected even when outside the caster's center-selection range. Use the caster's chosen square for both self selection and self membership. The rank is split from the base ID in `initbattlesceneproperties.asm`. Both SLOW ranks dispatch to the same function. Neither effect reads POWER for magnitude.

| Target at construction | Main RNG and status writes in source order | Target command and EXP contribution |
| --- | --- | --- |
| Fresh BOOST | Save old word, write old OR `0x3000`. No effectiveness draw or resistance gate. | Full-word reaction (flags 2), AGI then DEF increase messages 332/341, +5 for ally caster. |
| Active BOOST | Same write, then one draw in [0,7] against 8. Always fails. | Message 295, no target reaction, +0. |
| Any SLOW, first gate fails | One draw in [0,7]. With STATUS setting 0/1/2/3, require roll ≥ 0/6/7/8 respectively. Failure precedes all target writes. | Message 295, no reaction, +0. |
| Fresh SLOW, first gate passes | Write old OR `0x0C00`. No second draw. | Full-word reaction (flags 1), AGI then DEF decrease messages 333/342, +5. |
| Active SLOW, first gate passes | Same write, then a second draw against 8. Always fails. | Message 295, no reaction, +0. |

**Confirmed.** Follow `battleactions/getresistancetospell.asm` (STATUS at bits 15–14 of current resistance), `castspell.asm:spellEffect_Boost`, `spellEffect_Slow` and `battlesceneScript_DetermineSpellEffectiveness`. Failure unwinds the effect caller and continues with the next target. Even guaranteed SLOW success/immunity consumes the first draw. A fresh effect sets its two-bit field to 3 and preserves the other field and unrelated status. DISPEL's known-spell check and threshold 5 at resistance zero do not apply. Direct H3 covers BOOST fresh/recast and SLOW 1 fresh against all four settings. **Inferred** for a natural SLOW recast and SLOW 2 action from the shared caller. [BOOST fixture](../../../tests/fixtures/h3/spell-boost-v1.json) · [SLOW fixture](../../../tests/fixtures/h3/spell-slow-v1.json)

**Confirmed.** `createbattlesceneanimation.asm:battlesceneScript_PerformAnimation` queues the caster's MP debit **with its pre-effect status word**, before the target loop. An admitted silenced cast pays once and ends with the silence message, no target roll, effect or award. Otherwise `earnexp.asm → battlesceneScript_End → giveexpandgold.asm` caps cumulative status EXP at 49. A living ally caster receives the normal award even for all misses (zero contribution, minimum 1). Halve the accumulated value only for opposing targets in a listed battle, then apply the two range-16 rolls (zero adds one, then zero subtracts one) and the minimum. Same-side BOOST bypasses halving. The controlled Battle01 BOOST example earns 6 from one success and one failed recast. SLOW earns 8 from three successes and one immune target. Costs and awards play once per cast.

### When the stats change

**Confirmed.** At construction the word write alone changes no stats. Playback of either side's reaction (`battlescenes/battlesceneengine_0.asm:bsc0A_executeEnemyReaction`, `bsc0B_executeAllyReaction`) applies the saved word and calls `UpdateCombatantStats` for that combatant. A caster targeting itself first receives its cost reaction and then any successful target reaction. In particular a failed self-recast has no target reaction to reapply the constructed word after the cost restores the pre-cast word. On a *different* target the failed recast write survives. The BOOST fixture ends with a non-caster at counter 3, current DEF/AGI 45/27 (bases 40/24, last refreshed at counter 1). Next refresh at counter 3 would give 55/33. If the next refresh is the target's own after-turn, it first decrements 3→2 and gets 50/30.

**Confirmed.** `common/stats/updatecombatantstats.asm:InitializeCurrentStats → ApplyStatusEffectsOnStats → ApplyItemOnStats` rebuilds from bases. For each of DEF and AGI add floor(base × BOOST counter / 8), then subtract floor(base × SLOW counter / 8). Compute both deltas from the original base. The message reports the full three-counter delta even when a cap limits the actual increase. Apply STUN next (AGI −5, MOV −1), then equipped effects in slot order. DEF clamps at each operation to 0…200. Thus base DEF 195 with both counters 3 produces 127 before equipment (200 − 73). A spent MP or gained level never turns a temporary bonus into an earned base. The growth-refresh observations include mixed counters, STUN and equipment. [Level-up contract](../../../docs/design/contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh) · [Observed stat arithmetic](../../../docs/research/battle/runtime-rng-and-battle-math.md)

AGI has a packed edge. `GetBaseAgi` reads the entire byte for the multiply. `IncreaseCurrentAgi` adds to the low seven bits, caps at 127 and restores bit 7. `DecreaseCurrentAgi` uses whole-byte subtraction with bounds 0…200, including when the SLOW delta is zero. Reconstruct the raw base from numeric AGI and the source two-turn bit before this calculation. Derive both the next round's numeric speed and extra-entry eligibility from the resulting **current byte**. For instance raw 148 slowed at three counters gives 93 (148 − 55) and one future entry on a susceptible legal definition. Existing two-entry queues still execute in order. The example exercises a legal input to the calculation. Natural reach on retail enemies depends on their STATUS resistance. `combatantstats_1/2/3.asm` and `battleloop/turnorderfunctions.asm:AddCombatantAndRandomizedAgiToTurnOrder` own these operations.

**Confirmed** in the engine. A direct scenario start with neutral declared current AGI 210 (82 plus the extra-entry bit) generates at 82 until that actor's first real refresh. An ordinary Stay refreshes it to 200, so its next round generates at 72 with two entries. Direct neutral starts also preserve DEF above 200. Inspection, AI estimates and strikes use the declared DEF until the first refresh, which caps it at 200. The same initial-state flag covers DEF and packed AGI when either exceeds 200. Source new-battle enemy initialization ends in `UpdateCombatantStats` (`battleloop/initializecombatants.asm:InitializeEnemyStats`). Connected battle saves require the new-game route and already-refreshed allies. [Engine behavior cases](../../tests/Sf2.Remake.Engine.Tests/SupportStatusCoreTests.cs) · [Save eligibility](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs)

**Confirmed.** After action and dead-list processing, `BattleLoop` checks terminal outcomes, then reads the **current queue entry** for `ProcessAfterTurnEffects`. A living actor's own completed entry counts, including standby, skipped action and each extra entry. HP zero bypasses the entire routine. After STUN/SLEEP/MUDDLE/SILENCE, subtract SLOW unit `0x0400`, process ATTACK if present, then subtract BOOST unit `0x1000`. Each nonzero counter takes 3→2→1→0 without RNG. Only the 1→0 transition displays expiry (349 SLOW, 348 BOOST), before its write. These writes precede equipment recovery/poison, then **one final refresh**. Dead-list cleanup and the next terminal check precede cursor advance. A self-BOOST can therefore already be at 2 when the next ally gains control. H3 confirms the individual transitions and ordered refresh. [After-turn fixture](../../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json)

**Inferred.** A non-caster with a failed recast can remain at old effective stats across settled Movement input, into physical damage or next-round generation. Scene return's `LoadBattle` positions entities/loads the field. For example, B finishes its entry at counter 1, Karna recasts on B, and C next receives Movement input. Karna's after-turn touches Karna. B is inspectable and savable with the old effective operand. An attack on B computes damage before the HP reaction refreshes B. The loop ticks the queued actor alone. Neither step globally refreshes the untouched target. A later reaction on that target (including a cost or HP reaction), equip/remove refresh, successful level-up or after-turn refresh ends the discrepancy. Projected stats, capture, preview, round generation and merely selecting that actor do not perform such a refresh. **Unknown** at original runtime are a natural complete SLOW 2/recast lifetime and a save/reload with a naturally outstanding recast. The controlled evidence suffices to select the behavior without a new acquisition run.

**Confirmed.** `battleloop/processkilledcombatants.asm` removes listed deaths, clears status and refreshes after kill/defeat accounting. Do not erase status early at the zero-HP damage command. Victory calls `HealLivingAndImmortalAllies`. Defeat restores the leader and adjusts gold before field return. Exploration entry and fresh battle entry heal living allies and Peter/Lemon, retaining only STUN/POISON/CURSE before refresh (curse is rederived from gear). Ordinary dead allies are skipped there. A normally processed casualty already lost these counters. EGRESS uses its existing return and field recovery. A suspended original battle follows flag 88 through `LoadBattle` into the individual-turn loop and skips fresh-entry healing/queue generation. Original SRAM copies combatant data. Remake saves have their own settled-input contract. [Original save contract](../../../docs/design/contracts/system/save-system.md)

## Status lifetime and recovery review

**Confirmed** at the original controller boundary. `BattleLoop` reloads the current queue
entry after action settlement, invokes `ProcessAfterTurnEffects` and advances the cursor
only after the second death and outcome pass. The after-turn routine skips HP zero. For a
living actor it processes STUN, SLEEP, MUDDLE and SILENCE in that order, then SLOW, ATTACK
and BOOST before the equipment/poison tail and stat refresh. The retained Battle01 fixture
observes the individual counter branches at controlled RNG seams.
[Controller contract](../../../docs/design/contracts/battle/battle-control-lifecycle.md#post-action-and-after-turn-order) ·
[Original runtime fixture](../../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json)

**Confirmed** in the remake. The controlled ally starts at `0x0358` with two consecutive
entries in one round. SLEEP skips the first action and wakes (`0x0318`), MUDDLE expires
(`0x0300`), then SILENCE decrements (`0x0200`). The same ally receives Movement and takes
an ordinary Stay. Its second entry clears SILENCE before the next ally receives Movement
in the same round. The queue and thinking RNG stay fixed. Spell, item and physical status
effects use the packed actor word and the existing reaction refresh. The ordinary BOOST
recast on a teammate writes `0x3000` while DEF/AGI remain 45/27 from the applied `0x1000`.
The recipient's Stay reduces duration to `0x2000` and refreshes DEF/AGI to 50/30, clearing the
stored last-refresh operand. The existing controlled Battle26 observation covers the same
refresh through native input.
[After-turn continuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs) ·
[Action reactions](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)

**Confirmed** at the controlled recovery boundaries. A processed casualty loses its temporary
status in the death batch. Field recovery heals eligible living returnees and the two immortal
allies, clearing temporary bits. An ordinary ally supplied already fallen retains its word
through return and Raise. On fresh battle entry a living ally receives the same temporary-bit
clear as in the source's `HealLivingAndImmortalAllies`. With admitted word `0x5138`, the remake
previously cleared the word and carried the pre-refresh ATT of 21. Initialization now takes the
existing stat calculation from base, loadout and the resulting status, giving ATT 19, DEF 7
and AGI 40 in that case.
[Source recovery](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleloop/heallivingandimmortalallies.asm) ·
[Source stat refresh](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm) ·
[Church Raise contract](../../../docs/design/contracts/party/service-interactions.md#church-raise-runtime-seam) ·
[Connected casualty review](gameplay-rules.md#casualty-reward-custody-and-outcome-review) ·
[Field recovery and Raise cases](hostile-spells-design.md#public-rank-2-admission)

Run the following from `remake/` after the [locked SDK setup](../development-and-verification.md#locked-net-workflow)
and test-project restore.

```powershell
$project = 'tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj'
$cases = @(
    'FullyQualifiedName~ContinuingSilenceTicksAgain'
    'FullyQualifiedName~FailedBoostRecastKeepsOldStats'
    'FullyQualifiedName~NewBattleRefreshClearsTemporaryStatus'
    'FullyQualifiedName~OutcomesRetainLivePlayer'
    'FullyQualifiedName~RestoredFallenCasterKeepsTheWord'
    'FullyQualifiedName~ConnectedConfusedSpellSettlesBothDeaths'
) -join '|'
& $env:DOTNET_BIN test $project --configuration Release --no-restore -p:UseSharedCompilation=false --filter $cases
```

**Unknown** in the original runtime are natural acquisition of the exact mixed word on
consecutive entries and natural arrival at fresh battle entry carrying the controlled mixed
ATTACK word. The remake checks establish behavior at those admitted boundaries.

## Implementation decisions

1. **Bind each supported rank through the existing action entry.** Extend `PrivateSupportSpellBindings` and `SupportSpellDefinition` with the verified finite rows and target side. `PrivateBattleContentReader.SelectOpening` already selects the party growth books. `PrivateBattleDefinitionReader.Decode` expands each packed entry into ranks 1 through the learned rank and reads each source row by family and level. The presence of a higher rank in that catalog grants no action. Look up each independently so an encounter can bind BOOST or SLOW without DISPEL. Reuse the existing encounter spell dictionary, learned book, MP checks and silence gate. Resolve the same positioned target set in query, prepare and commit. Allocate each target's draws, construction writes and reaction in deployment processing order, as for DISPEL and attack spells. The private reader assigns original combatant indices to that order. An authored initialization or AI slot mapping can differ. `EnemySupportDecision` remains the admitted DISPEL AI selector. This slice grants no automatic BOOST/SLOW casting. Use `ValidateSupport` to check one ordered target outcome per target with zero, one or two draws according to the table. Only actual reactions refresh. In `ValidateState`, validate the masked write and retained operand only for targets that passed the first gate, preserving all other actor-state comparisons. In `Sf2BattleProgressionRule.Award`, only opposing support targets qualify for halving in a marked battle. Keep construction writes on the detached prepared actors, capture the cost word before them and validate the exact permitted transitions. Canceled/stale/invalid selections spend no MP, RNG or action. [Action](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs) · [Resolution and replay](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)

2. **Keep duration in `BattleActorState.Status`.** Add explicit base DEF/AGI accessors over `Progress` with definition fallback. Read effective stats through the actor, using a pure calculation alongside the existing [equipment owner](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs). Retain one optional actor-local operand containing the BOOST/SLOW counter bits used by its last refresh **only while those differ from stored duration**. On construction-only writes keep the old effective operand (also on repeated failed recasts). On a source refresh use the written word, then discard the operand. Preserve it across unrelated immutable actor copies. Use the same distinction while after-turn writes await final refresh. Full current-stat copies would need synchronization on every level/equip/restore. Computing solely from the live word would turn the confirmed 45/27 into 55/33 too early. The optional operand carries only the irreducible timing difference, never ticks and never decides spell eligibility. [Actor state](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs)

3. **Use each stat at its actual reader.** `BattleGrowthRules.Grow` and `IBattleProgressionRule`'s level gain comparison read earned bases. Real growth replaces progress, then refreshes using the new bases and current gear. Keep ATT's existing difficulty/loadout inputs. Equip, remove, transfer and class rebind through the accepted owner all refresh only where their source counterparts do. Field promotion preserves earned stats/class/book as specified. Normal return already clears these battle counters before church. Physical damage **and** AI damage estimation read effective DEF. Inspection reads effective DEF/AGI. `BattleTurnFlow.GenerateQueue` supplies current speed and current extra-entry bit to the existing generator. Keep the old queue/cursor until round generation. Include currently admitted mixed statuses at the shared refresh, with source STUN ordering where relevant. Keep ordinary action legality, movement and status gates specific to their existing effects. [Growth](../../src/Sf2.Remake.Domain/Gameplay/Sf2/BattleGrowthRules.cs) · [Physical estimate and strike](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs) · [Turn flow](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) · [Promotion](../application/church-promotion-design.md)

4. **Advance existing continuations.** Extend the admitted status mask in `SilenceRules.Supported` and audit the narrower guards used by commands, AI, growth, restore and return. `BattleAdvancer`'s queue-only AI branch and `BattleActionCommitter`'s lifecycle predicate must route timed actors through `BattleAfterTurnContinuation`. Insert the ordered decrements after silence, continue through `BattlePoisonContinuation`, refresh at the final seam, clear the operand at death cleanup. Keep unrelated counters and existing turn/reward waits. Return must admit these bits in both `Sf2OutcomeReturnPolicy.Heal` and `IOutcomeReturnPolicy`'s postcondition and apply each caller's actual recovery. Reuse the existing scene waits for costs, per-target messages, reward and expiry. Include both stat amounts in order, actor names and correct friendly display slot for BOOST. The scene puts a friendly BOOST recipient in the ally display slot. The view chooses result text from the effect and reaction. Verify rank names, icons and EN/ZH text at the actual Magic input, including `IdentityIconReader`, `remake_identity_icons.py` and `remake_localization.py`. [After-turn owner](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs) · [Scene](../../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs) · [Adapter](../../game/src/Battles/BattleSceneView.cs)

### Save, compatibility and recovery

Keep the settled player Movement boundary. Battle entry retains undeployed members as field party data, outside the encounter. Capture and Continue reject either battle counter on those retained members in every battle version. Capture the stored word, base progress, loadout, queue/cursor and seeds through the existing continuation. Include the sparse last-refresh operand on a divergent target. At a settled save a differing field can only be stored 3 with previously applied 1 or 2 from a failed recast. Reject other bits, impossible differences, redundant operands and unfinished construction states. A fresh construction with applied 0 belongs to the scene. The absence of an operand means use stored counters. Restore and compare the complete detached state, including this operand, before publishing. Recompute derived DEF/AGI without triggering an in-game refresh. A post-recast save must reproduce 45/27 and the next consumer of those values. [Battle entry](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)

Battle v5 encodes a record containing either timed field or its divergent operand. The record carries the class-term mode and each divergent actor carries its last-refresh counters. It supports both unchanged-class and promoted parties, retaining the v4 class terms for the latter and consistent class presence across active and retained progress. Keep the compatible existing v2/v4 writer for records needing neither field, legacy v1/v3 field reads, encounter recipe and class resolution. Prior writers emitted neither timed field in admitted battle saves. Do not reinterpret malformed earlier records as new-version ones. Carry the operand through `SavedBattleActor`, `BattleContinuationValues.Capture/Same`, `BattleContinuationRestore` and `BattleSaveWire`. A dropped value must fail detached reconstruction. Preserve bare inventory words. Old readers stop at version dispatch before binding or offering corrupt-primary recovery. Keep the existing store's checksum, primary/backup publication and explicit recovery. Reject incompatible or malformed candidates without replacing the live session. For rollback retain original slot bytes and restore a compatible backup explicitly. Never silently downgrade an active effect. No field payload expansion for normally cleared counters. [Battle continuation](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationRestore.cs) · [Wire and class terms](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) · [Format dispatch](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs)

## Serial ownership and acceptance

Implement on accepted main after design review. Coordinate shared Content, scene, localisation, save and index files with [#1964](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1964) before taking them. A coherent first code slice owns the finite actor/refresh rule, duration, readers and cold continuation with actual engine cases. The next slice completes ordinary casting, presentation and the end-to-end observation. Publish the new ranks as playable only with the full action/return path. Update [campaign characters](../content/campaign-characters.md), [gameplay rules](gameplay-rules.md) and [capability status](../capability-status.md) at implementation, distinguishing learned, bound, actually cast and cold-continued ranks.

| Neighbor | Interface and merge order |
| --- | --- |
| [#1946 static rings](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1946) | Single writer for equipment/refresh. First merged slice establishes the shared operation. Rebase the second. Ring effects enter after both counters, using actual equipped words. Protect Ring's +5 DEF stays outside the fraction. Current weapon and unarmed cases can ship before rings. |
| [#1952 ring Use](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1952) | After #1946 and the accepted BOOST effect. Protect Ring's `useSpell BOOST` calls the same per-target construction/replay and duration. Item caller owns its range/admission, payment, durability, break/removal, Repair and save. No learned spell or caster MP requirement for the item effect. Power Ring uses ATTACK, outside these ranks. |
| [#1965 DETOX](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1965) | Separate cure masks and equipment/curse consequences. Preserve BOOST/SLOW through unrelated cures and apply ordinary reaction refresh when emitted. Serialize shared masks, scene, progression and indexes. DETOX 2/3 are accepted in Field and Battle Magic. DETOX 4 remains outside the admitted ranks. [Ordinary input and cold Continue](detox-ranks-design.md#ordinary-input-and-cold-continue) |
| [#1967 hostile effects](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967) | Reuse center/area and ordered reactions. MUDDLE control, SLEEP, DESOUL death and distinct resistance/recast rules stay with that issue. No general buff registry or new enemy AI family here. |

**Implementation acceptance**

- With varied legal actors, move then self-center BOOST, move into/out of a teammate's area, and center each SLOW rank at its range limits. Include mixed eligible, neutral, dead/unplaced and outside-diamond units. Confirm target order, MP once, prior-word cost, fresh success and both recast gates, all resistance thresholds, same-side/hostile EXP, and silenced/insufficient-MP/cancel outcomes. Check actual RNG continuation after mixed targets. Test self-recast separately from an already-affected non-caster.
- Compare the next actual physical strike/AI estimate and generated queue for odd bases, clipping, unequal overlapping counters, live weapon and changed base after growth. Include one packed high-bit input. Existing queue remains byte-for-byte stable until its normal boundary. Exercise owner-only decay (including standby/skip), dual expiry, opposing side's turn, terminal action, removal and recovery. When #1946 lands, rerun its equip/unequip behavior at a partial counter with a DEF/AGI ring.
- At a settled player input after a failed recast on another actor, Continue with the remaining duration, old effective stats, nontrivial queue, same RNG and class. Compare the next damage/order and first refresh against uninterrupted play. Also exercise an ordinary partially expired active status, a legacy slot and a protected unknown version. Reach victory/defeat or EGRESS through existing return owners. Check ordinary casualty, living/immortal and next entry, persistent MP/EXP at the appropriate stage. Field Save/Continue keeps earned bases and loadout.
- For the shortest ordinary observation, reuse the Chinese castle slot (both casters active), or its existing Battle26 road continuation with the same books. Use the ordinary repeat-battle route documented for Pacalon. Cast BOOST 1 with a second ally in range, SLOW 1 and SLOW 2 from Sarah on legal enemies. Record selected rank, MP, target status/stats and EXP, then a physical hit and next round's queue, an active recast/expiry and battle Save → cold Continue → usable return (EGRESS is sufficient). Inspect running state, input, projection and errors. One bounded suffix or separate branches of the same slot suffice. Record precisely which outcomes were observed versus exercised in engine cases. Reuse matching installed resources, keep original slots read-only and publish only compact facts. No repeated leveling or new original runtime acquisition is needed.

For code acceptance, run `uv run sf2 verify engine` with the new behavior cases in the dedicated engine project and `uv run sf2 verify adapter` when the presentation changes. Use the ordinary input suffix above for the player-visible boundary. Run the clean-head `uv run sf2 verify plan --base origin/main --head HEAD` and report actual CI for each slice. This design itself requires direct document, source and link checks only.
