# Battle43 outcomes, Zeon aftermath and ending admission

Evidence date: 2026-10-22. **Confirmed (static):** defeat has priority.
Zeon's death hook removes128 from the death queue without restoring HP;
the leader sweep then queues other living enemies. Complete after43 hides
his battle entity, reverses four map-block stages, requests59, 12, 11/Up
and clears four follower flags. After it returns, victory joins0, clears443
and sets543. Ordinary Map59 rebuild can then call `cs_615E6`. This is
ending **admission**, not completed ending or restored free walking.

Continue with [Zeon aftermath, sealing and the Granseal return](./zeon-aftermath-and-granseal-return.md)
for the complete admitted scene and its ordinary-map return boundary.

## Agreement and evidence route

Standard rigor for [Issue #1182](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1182)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
close battle-result→aftermath→next-story entrance behavior. Separate death
queues, event parameters, live flags and entity rebuilds justify this level.
Accepted base `5745ba75bf5820ca1a58d2dc5cbe50fa109762c7`, tree
`ca613c7f2164ef67317ffce052718e99f4ed1a52`. Own this document and
navigation only in [laser results](./battle43-zeon-guard-laser.md).
No stacked dependency, shared index/schema/fixture/contract or engine work.

Reuse [initialization](./battle43-initialization.md),
[first control](./battle43-activation-and-first-control.md),
[laser](./battle43-zeon-guard-laser.md),
[Battle42 outcomes](./battle42-outcomes-and-zeon-admission.md),
[battle loop](./battle-loop.md), [actions](./battle-actions.md),
[script engine](./common-scripting.md),
[item transactions](./map-event-item-transactions.md),
[request consumption](./map-event-request-consumption.md) and
[common maps](./common-maps.md). Recheck selected43 and actual consumers.
Stop at `ms_map59_flag543_InitFunction`→`cs_615E6` entry;
zone36→`cs_62658` identifies only the next connection. Entire ending/
credits/kiss, optional branches, Zeon AI, whole rounds, translation, new
extraction machinery and runtime acquisition are outside this slice.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` means
`code/common/scripting/map/`. Fnnn is a story flag, enemy ordinals are
zero-based, intervals half-open, addresses/packed words hexadecimal.
sf2const.asm/sf2enums.asm and original macros own widths/encoding.
Registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality or decoded geometry is claimed.

Method: linear selected source/consumer reads and small supplied-state
branches. Reuse the immutable18,285,979-byte init/script pair and
`local/issue1005/pair.txt` provenance for **one** after43 lowering:
its first unsupported operation and retained tail cannot be established
from after42's special-sprite exception. No pair regeneration/final world
preparation. Per direct recipe<60s/<512MiB; peak memory **unmeasured**.
Small records<2MiB, total output<4MiB in ignored `local/issue1182/`.
Reassess material growth/new platforms; Unknowns are not a collection queue.

## Death hook, leader sweep and terminal priority

**Confirmed:** `battle/battleloop_1.asm: BattleLoop` (`$23A84..$23BB4`)
returns from the individual turn/debug branch, then calls defeated hook→
ProcessKilledCombatants→CountRemainingCombatants. Ally count0 takes
Defeat **before** enemy count0 takes Victory. Only a nonterminal result
runs actor after-turn effects, cleanup and the same priority checks;
there is no second defeated hook. Another nonterminal result advances
the byte turn cursor by2. Admitted retreat unwinds before this suffix.
Reuse the accepted action/replay/queue boundaries, not a simulated round.

| Complete table / selected43 entry | Exact interface |
| --- | --- |
|data/battles/cutscenes/battleendcutscenes.asm, 48 relative words, base47BE8|slot43/byte86 at47C3E→edcs_battle43.|
|data/battles/global/enemyleaderpresence.asm, 45 bytes, base47C8E|byte43 at47CB9=−1; enables sweep independently of AI LEADER.|
|data/battles/cutscenes/afterbattlecutscenes.asm, 48 relative words, base47CF4|slot43/byte86 at47D4A→abcs_battle43.|
|data/battles/cutscenes/afterbattlejoins.asm, 52 bytes, base47D6A|byte43 at47D95=0.|
|data/battles/global/afterbattlepositions.asm|Only battle5 override; none for43.|

`battle/cutscenes/battleendcutscenesstart.asm` (`$47B92..$47BE8`)
requires Bowie0 HP.w≠0 and enemy128 HP.w=0. Selected128 is Zeon98;
physical47 is not another combatant. Clear543 executes `edcs_battle43`
(`$4F358..$4F35E`): **reviveAlly128; end**, six bytes. Held543 skips
that body but still reaches the leader-table suffix.

Despite its name, `mapscriptengine_1.asm: csc21_reviveAlly` reads a word
ID, scans the original death-list length, compacts nonmatching bytes and
decrements live length for every matching byte. It restores no HP/MP/
status/position, declares no entity and writes no new sentinel. Length0
exits; `[128, 143, 128]` becomes length1/prefix`[143]`, not extra stale-tail
entries. Zeon can remain HP0 at his battle position/alias for after43,
without his queued spin/cleanup/kill-stat increment. Earlier action-side
EXP/gold calculation is separate and is not undone.

`battleendcutscenesend.asm` (`$47C48..$47C8E`) rereads battle and
scans all32 IDs128–159. Every HP.w≠0 appends to the existing queue,
adds FF and increments length. No position, roster, activation, neutral or
duplicate filter; 128 HP0 is not re-added. ProcessKilledCombatants
(`$24518..$24642`) requests effects/waits, credits ally defeats or
BATTLESCENE_FIRST_ALLY kills, writes listed X/Y=−1, status0, refreshes
stats and parks aliases at7000, 7000. It neither writes HP0 nor calls
drops nor clears queue length. A statistic does not establish causality.

CountRemainingCombatants (`$23C58..$23CBA`) tests signed X.b≥0 and
HP.w≠0 across30 allies/32 enemies; Y, membership, activation and neutral
are ignored. BowieHP0 forces ally count0. The position helper's
Bowie/128 gates find no43 override.

| Supplied state at relevant reads | Conditional result if services return |
| --- | --- |
|Bowie alive, 128 HP0, 543 clear, queue contains128|Remove128; sweep/clean other living enemies; Victory if ally count remains nonzero.|
|Bowie alive, 128 HP0, 543 held|Skip removal; sweep still runs. Queued128 can clean normally; after43 has a separate guard.|
|128 alive/positioned, other enemies gone|Subordinate deaths alone do not make Victory.|
|128 alive but X<0, every enemy fails X/HP|Victory without Zeon death hook is possible.|
|Neutral159 at authored position with HP0|Neither counts nor sweeps. Supplied nonzeroHP can count with X≥0 and sweeps regardless of X.|
|BowieHP0 with no countable enemies, or no countable ally|Defeat wins; BowieHP0 also skips hook.|
|128 dies only in after-turn effects, another countable enemy remains|No second hook that turn; combat can continue.|

These are changed-state branches, not natural party/HP/queue assumptions.
Victory does not require physically setting every enemy HP to zero.

## Conditional Healing Rain transaction

**Confirmed:** `data/battles/global/enemyitemdrops.asm` (`$BE52..$BECC`)
has30 four-byte rows plusFFFF. Its sole43 row, ordinal19 at
`$BE9E..$BEA2`, is battle43, enemy ordinal15→**143**, item8, drop flag19.
`enemyEntity` adds128. The independent bit is FFF7A6 bit3 within
ENEMY_ITEM_DROPPED_FLAGS(FFF7A4), not F443/F493/F543.

The [initialization owner](./battle43-initialization.md) identifies143
at12, 7 as BLUE_SHAMAN_0 **definition102**, not85 or unknownByte0.
Copied equipped Holy Staff97, Rain8 and placement6008 yield actual words
**00E1, 0008, 0008, 007F**. AddItem masks placement with807F; 2000/4000
metadata is not stored. Neither one bottle nor unchanged inventory at
death is assumed. After use removes one Rain, another can still match;
with none the row gives no drop. No Shaman AI turn is simulated.

Three direct BSR sites in battleactionsengine_1.asm call
`battleactions/dropenemyitem.asm: battlesceneScript_DropEnemyItem`
(`$BD24..$BE52`) with current pointers. Admission requires allied actor,
enemy target, targetDies.b≠0, live battle43, target byte143 and a Rain in
its **actual four slots**. GetItemSlotContainingIndex masks indices and
selects first match. Leader sweep/field cleanup never calls this transaction.

Rain is outside the three1/32 items (Taros Sword/Iron Ball/Counter Sword),
so its matched route draws no drop RNG. BSET19 occurs **before** removal/
capacity; held bit stops without removal/award. First admission removes
one slot, arranges items and refreshes target; another Rain remains.
ActorHP.w≠0 then AddItem tries the actor's first empty slot, stores bare
0008, without class/equip or party/Caravan search. Success emits396→395.
HP0/full instead tests RARE and adds Rain to Deals, capped15; removal/bit
already happened. Rain is RARE|CONSUMABLE, price10000, use AURA4,
equipFlags NONE. No personal pickup can still consume the one-time drop.

| Changed transaction input | Result |
| --- | --- |
|Ally kills143, 19 clear, Rain present, actor alive/free slot|Set19, remove first Rain, award one0008; not two drops.|
|Same, actor full or HP0|Set19/remove first, add one to Deals up to15; no pickup pair.|
|19 already held|Keep target items, no award/Deals increment.|
|No Rain, wrong target/battle, or targetDies clear|No selected transaction/bit write.|
|Guard laser/another enemy kills143|Enemy-actor early return: no ally-earned drop/EXP/gold from that action; later statistics separate.|
|143 survives until Zeon sweep|Cleanup can remove its position with HP≠0; no Rain transaction.|

Messages, author paraphrases and ordered tokens only:274 Egress cast
`{NAME}{N}{SPELL}{#}`; 275 Wing use `{NAME}{N}{ITEM}`; 363 exhausted leader
`{LEADER}{W1}`; 396 drop `{NAME}{N}{ITEM}{D1}`; 395 receive
`{NAME}{N}{ITEM}`. After43 has **no dialogue request**. Tokens do not
prove acknowledged text or completed pickup presentation.

These writes occur during WriteBattlesceneScript construction, before
message/reaction replay. Its End restores the HP snapshot, not inventory,
drop bits or Deals. A later playback failure is not a proved rollback.

## Victory, defeat, retreat and repeated completion

**Confirmed:** `battle/battleloop_2.asm: BattleLoop_Victory`
(`$23CBA..$23D98` file) first heals living/immortal allies. Its30-slot
helper restores maxima, preserves STUN/POISON/CURSE and refreshes stats;
Peter7/Lemon28 heal even at HP0, other dead allies skip. It saves live
CURRENT_MAP into PARAM2, rebuilds force lists, reads first active member's
X/Y plus crop origin by **byte arithmetic**, and physical alias facing
into PARAM3–5. PARAM1=0 does not post an event. Empty/stale membership
has no managed native fallback.

After wrapper tests live500+CURRENT_BATTLE. For unchanged43, clear543
calls the complete scene below; held543 skips it. Both reread join byte0
and call JoinForce: set joined0, rebuild lists, set active32 only below 12
active. Existing active32 survives full capacity; newly activated0 is
absent from the just-built list snapshot. This is Bowie, not recruitment
of Elis/Peter/Lemon/Zynk. Only after wrapper return does victory reread
CURRENT_BATTLE and **clear443 then set543**. F493 is untouched; selector/
flag mutations are not rolled back or latched to initial43. It clears
d0–d3 words, returns PARAM2–5 low bytes, d4=1; upper long halves unestablished.

Defeat calls UpdateBattleUnlockedFlag (clear443 only if543 held), requests
sad music/text363/close, restores BowieHP=max, halves gold via lsr.l1,
returns GetEgressPositionForBattle, d4=−1. No after43/join/new543; the
battle4 losable exception does not apply. Admitted Egress/Wing pays
MP/removes selected item first, requests274/275, cast sound/flash/close,
updates unlock and unwinds action frame; same egress helper, d4=0.
It bypasses defeat recovery/gold and victory flags.

43 matches no fixed-town or forced38/39-clear egress exception: read
**live EGRESS_MAP**. Map59's earlier default999-clear setup writes
DOJO45; the outcome does not freshly establish it. Church/savepoint can
replace it. With399 held, GetSavepointForMap (`$75EC..$764E`) scans23
rows: DOJO45→12, 25/Up; absent maps keep map with1, 1/Up. Clear399 gives
3, 56, 3/Down. Held64 separately updates raft state. These are reused
branches, not a hardcoded43 town or natural return observation.

| Stable543 / outcome | Ordered writes and return |
| --- | --- |
|Clear, victory, full after43 returns|Warp parameters59/12/11/Up→clear67/70/71/76→join0→clear443→set543; d4=1.|
|Held, victory|Skip scene: no new warp/hide/follower clears; join0→clear443→set543; return member seed, d4=1.|
|Clear, defeat/admitted retreat|No forced443 clear; retain543 clear/follower flags; no after43.|
|Held, defeat/admitted retreat|Clear443, retain543/follower flags; no after43.|
|Interrupt before warp, between follower clears, or clear443/set543|Distinct memory prefixes; no atomic scene/outcome/save commit proved.|

543 does not clear itself. Repeated held543 victory still joins/sets
completion; subsequent ordinary543 setup can again admit its init.
No separate played-ending/checkpoint guard is proved at this entrance.
Later ending bodies may change state; they remain excluded.

## Complete after43 and retained battle identities

**Confirmed:** `data/battles/entries/battle43/cs_afterbattle.asm`
occupies `$4F35E..$4F47A`: **67 top-level commands, 284 bytes**, including
waits/end; no inline entity table/motion payload. Unlike after42, there is
no map load or population rebuild. Let `R` mean SFX_BIG_DOOR_RUMBLE93;
`B` means SFX_BLAST_SPELL114 then flashScreenWhite20, in that order.

| Source ordinals | Complete ordered operations |
| --- | --- |
|0–2|Camera destination8, 2 and wait; music fade-out; wait40.|
|3–14|R; B; B; B; R; B; B.|
|15–24|hide128; R; B; B; R; B; copy12, 54 size7×4→10, 3.|
|25–32|B; B; R; B; copy12, 49 size7×4→10, 3.|
|33–40|B; B; R; B; copy12, 44 size7×4→10, 3.|
|41–49|B; B; quake3; R; B; copy12, 39 size7×4→10, 3.|
|50–56|B; B; quake5; R; wait120.|
|57–66|Fade out to white; wait40; fade in from white; warp59, 12, 11/Up; quake0; clear67; clear70; clear71; clear76; end.|

Totals: eight R, nineteen B, one music fade, three explicit waits. Hide's
word128 resolves through **battle** alias32→physical47, ENTNUM32, Zeon
sprite251. This is not ordinary-map128. csc2E resolves then HideEntity
parks current/destination XY at7000, tombstones matching event aliases
FF, and removes a matching follower-list entry. Loop bound is
ENTITIES_TOTAL_COUNTER, not proof all64 alias bytes were scanned. It
does not change combat128 HP/X/Y or delete/revive a combat record.
No later scene command uses128. Battle neutral159/logical31 Elis is
also distinct from ordinary setup Elis132.

Each csc34 copy passes packed source0C36/0C31/0C2C/0C27, size0704,
destination0A03, and sets both view-plane update bits. CopyMapBlocks
reads **current 64×64 RAM layout**, left→right/top→bottom, 64 words/row.
Each writes28 words/56 bytes to the same rectangle; four write112 words/
224 bytes cumulatively, only28 distinct destination words. They reverse
Battle42's stages; final12, 39 is not F542's source12, 54. Battle crop28×38
does not remove source rows from working RAM. No decoded block values,
collision, graphics or visible collapse is claimed.

Camera detaches VIEW_TARGET and waits for scrolling. Each flash20 gives
`(20>>3)+1=3` LaunchFading calls: **57 launches** across nineteen flashes,
each F/1/FLASH_QUICKLY_2, then DuplicatePalettes. This is not twenty
observed frames or the adapter four-pulse preset. LaunchFading waits
VInts until FADING_SETTING clears, then another VInt; restores old period
and selects all palettes. White out/in use1/1/OUT_TO_WHITE and1/1/
IN_FROM_WHITE. Plain quake3→5→0 writes words without ramp bits.
Wait40/120/40 uses low-byte Sleep, subject to debug text-skip. FFFF
conditionally waits for scrolling if dialogue is open, clears view speed
and restores registers; not a global idle barrier, automatic warp or save.
Requests/native return dependencies do not prove rendered completion,
measured timing, input availability or completed audio/palette/scroll work.

## Return parameters and ordinary Map59 reconstruction

**Confirmed:** ordinal60's csc07_warp writes event type.w=1, then bytes
0, 59, 12, 11, Up to PARAM1–5. It posts and returns; it does not transfer the
map there. These are the **same parameters** victory seeded before the
scene, so full after43 replaces member-based return with59, 12, 11/Up.
Quake/follower writes, join/clear443/set543 precede BattleLoop return, d4=1.

MainLoop (`$75C4..$75EC`) resumes alt_MainLoopEntry: SwitchMap then
ExplorationLoop, **without immediate CheckBattle**. Its three-row switch
table replaces58/F999→59, plus Granseal/F640 and drought/F530; explicit59
stays59. ExplorationLoop (`$257C0..$25A94` file) **clears MAP_EVENT_TYPE
on entry**, then heals/fades, sets CURRENT_MAP59/CURRENT_BATTLEFF, loads
tilesets, selects/populates setup, loads sprites, clears temporary256–383,
sets80, loads layout/services and runs setup init. Thus after43 warp's
parameters return via victory, while its posted type clears here. This
path does **not** call ProcessMapEvent for that request; do not invent
two warps.

For a later init/ordinary script event, WaitForEvent sees type and
ProcessMapEvent clears it **before** dispatch. Type1's no-scroll handler
idles player0, unwinds exploration and returns parameters/d4=0 to MainLoop.
That separate interface does not prove any excluded ending event ran.

GetMapSetupEntityList→GetCurrentMapSetup reads `data/maps/mapsetups.asm`:
map59 default ms_map59 has one override, 543→ms_map59_flag543. Selector
starts default, replaces on each held flag in order; 543 is sole variant.
RunMapSetupInitFunction selects **again**, and selected543's init checks
543 a **third** time. Population, init selection and body admission are
separate reads. With stable543 held, pointertable_543 at6153E has six
longs: entities61556, entity events615B0, zone events615B6, descriptions615CE,
item events615D0, init615D8. Guard/branch/script/rts occupies
`$615D8..$615E6`; held543 calls **cs_615E6**, clear returns615E4.
Stop at that call: adjacent textCursor3760 is not a completed text request
or the full ending's contract.

InitializeMapEntities clears49 physical records, 64 aliases and walking
scratch, creates ordinary followers first, fixed rows next, Bowie0 last.
Map59 is not overworld, so65 does not select Caravan prefix. Full after43
cleared67/70/71/76, excluding those four follower rows; other follower
flags/membership remain. Peter/Lemon still appear as **fixed allies**.
Other valid follower flags can allocate actors or alias31; do not assume
absence. Combat entities/neutral159 are not an ordinary population snapshot.

| Fixed row / address / logical identity | Position, facing, sprite, script |
| --- | --- |
|0 /61556 /128|63, 63, D, Effect3(178), eas_Init.|
|1 /6155E /129|63, 63, D, Object1(173), eas_Init.|
|2 /61566 /130|63, 63, L, Flame1(171), eas_Init.|
|3 /6156E /131|63, 63, L, Object1(173), eas_Init.|
|4 /61576 /132|11, 15, U, Elis(204), eas_Init.|
|5 /6157E /ally7|14, 11, U, Peter's current ally sprite, eas_Init.|
|6 /61586 /ally28|11, 10, R, Lemon's current ally sprite, eas_Init.|
|7 /6158E /133|15, 10, L, Astral(209), eas_Init.|
|8 /61596 /134|13, 9, L, King Galam(214), eas_Init.|
|9 /6159E /135|63, 63, U, Mitula(215), eas_InitSlow.|
|10 /615A6 /136|63, 63, U, Woman(195), eas_Init.|
|End /615AE|FFFF; eleven8-byte rows+2=90 bytes, no mainEntity header.|

All fixed sprites<240; no special47 allocation here. With n followers
and p/l=1 when Peter/Lemon allocate anew, nonally128–132 occupy n+1..n+5;
133–136 occupy n+6+p+l..n+9+p+l. Nonzero Peter/Lemon aliases reuse the
follower physical slot and skip fixed position/script. Full after43's
stable clears give p=l=1. Bowie0 uses returned12, 11/Up, eas_Idle before
init. 63, 63 is a valid six-bit declaration, not deletion. Battle128 and
ordinary128 have no implied physical identity continuity.

Fresh ordinary LoadMap applies F542's7×4 copy12, 54→10, 3 **before**
chests, and skips battle crop with CURRENT_BATTLEFF. F543 has no layout
copy row. It reconstructs source54 although after43 last wrote source39;
restoration by excluded ending is outside this boundary. Persistent
543/542/443/follower flags survive temporary256–383 reset.

| Supplied543 across rebuild | Admission boundary |
| --- | --- |
|Stable held|543 population; reselected init; body held→cs_615E6 before WaitForEvent/player control.|
|Clear at population, held at init/body|Default population can pair with543 init; not one snapshot.|
|Held at population, clear at init selection|543 population can pair with default init, which reads999/443.|
|543 init selected, but clear at body check|Guard returns; no control/rendering claim.|
|Repeated stable held|No separate checkpoint/played flag at entrance; later-story execution unproved.|

Default999/443 branches stay with the
[Battle42 return owner](./battle42-outcomes-and-zeon-admission.md#return-loop-and-conditional-battle43-handoff).
Only another exploration return lets MainLoop CheckBattle run: held442
can select42 first; clear442/held443 selects43, even if held543 then clears
443 on selection. Normal full victory clears443, not442. No fixed incoming
flag set replaces those reads. The543 zone table has wildcardX255/Y36→
Map59_21F_ZoneEvent0; its separate543 guard calls cs_62658, default returns.
Movement/event admission is required; the init entrance does not prove
zone36 ran. This names the next connection only.

## Actual producer and current consumers

**Confirmed about accepted code:** one diagnostic registers only after43,
scene_maps=[59], reads the retained pair and compiles once. It returns
**without exception**, retains68 instructions including end-map-script;
camera destination plus separate wait accounts for the extra instruction.
23 are native-call: nineteen flashes/four setBlocks. First is typed6 /
source5, flashScreenWhite. Preceding0–5: camera-target, CameraWait, SoundFade,
wait40, Sound93, Sound114. First native setBlocks is typed25/source24.
No actions or scene population compile. End, warp and typed flags exist
in memory, not a successful runtime path. This is distinct from after42's
exceptional five-op prefix.

| Actual owner | Selected43 expression and refusal boundary |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [final prepare](../../src/sf2tool/remake_exploration_content.py)|Typed camera/sound/wait/hide/quake/white fades/queued warp/flags/end exist. flashScreenWhite, setBlocks and reviveAlly lack branches and become native-call. Program dictionaries insert before compilation; later exception can leave partial state. This diagnostic has no exception, but invokes no final prepare/world write. Actual prepare validates canonical identity/pin, compiles programs/routes/maps/setups/materials, then serializes at the end; not disk rollback proof.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), final prepare above|Default59/requested543 share map-59-setup key. 543's four-op chkFlg/beq/script/rts qualifies for compiling cs_615E6; this task deliberately does not request it. Non-event59 also retains native flag-layout-copy frontier. Prior **Inferred** variant-overwrite risk remains; no543 preparation/publication/ending lowering claimed.|
|[Routes](../../src/sf2tool/remake_exploration_battles.py), [encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [scenario reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [field reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Preparation admits2/3, private reader/scenario1/2/3. 43 wildcard coordinates pass that predicate alone; nonempty edcs lowers to native reviveAlly. Field reader separately requires private EnemyLeaderPresent=false and an empty/end-only defeated program: 43 violates both. Earlier package/pin/source failures can occur first. No43/59 content/material world published.|
|[Outcome rules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [scene continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)|Represented living/positioned actors and ally-leader defeat priority exist. Hook reports enemy-defeated-program-none; no Zeon queue removal/native32-slot sweep. Existing action/cleanup accounting does not bind Rain19/Deals. Generic outcome types do not admit43.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [return policy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs)|Guarded after-program, join→clear→set→return exist with per-stage publication. First-member seed uses validated First/Single, managed positions rather than unchecked byte wrapping/aliases. Configured egress differs from live EGRESS_MAP/savepoint/raft variants; nonzero return status refresh refuses. No43 return observation.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [StoryState](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|native-call reads as UnsupportedInstruction; refuses before Next/Commit, retaining earlier committed prefixes. Flags materialize as distinct sorted arrays; clears do not mutate earlier snapshots or existing population. QueueMapWarp commits ScriptWarp; null-cursor dispatch checks it **before** VictoryProgramFinished. A hypothetical admitted after43 tail could therefore transfer before completion flags; not native parameter-reuse→completion→rebuild order.|
|Same MapTransfer, [allocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Apply selects setup/Build from current story flags, then publishes supplied continuation. Generic rebuild/hide/layout primitives do not bind source58 switch or543 world. Native hide need not cancel scripts; typed Hide also clears Actions/Follower/visibility.|
|[Adapter](../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../remake/docs/capability-status.md)|Actual sound/white-fade/quake projection exists. FlashWhite requires white-four-pulses, not source flash20's three launches. No reached43 scene, selected layout/identities, service/input completion or playable ending entrance observed.|

MapTransfer.Apply takes flags from **current.Story** while publishing a
supplied continuation afterward. Actual ReturnBattleMap only copies
clearWarp; outcome join/flag stages already committed to current. That
path does not lose a deferred flag write. Conversely queued script warp
is serviced first, so typed flags/return stages alone cannot certify its
hypothetical after43 ordering. No unsupported path was executed here.

**Inferred:** later integration can reuse programs, flags, population and
layout/presentation mechanisms, but needs selected content, queue-preserving
hook/sweep/drop semantics and correct outcome/warp/setup order. Allowlist
expansion or ignoring native-call cannot supply those rules. No engine
or tooling feature is authorized by this finding.

## Reproduction, retained failures and open boundary

Ignored `local/issue1182/observe.py` retains the sole selected lowering
in observe-01.json; do **not** lower again for ordinary review.
`local/issue1182/review.py` rechecks selected source/consumer snapshots,
complete67-command order/encoding, table/queue/flag/identity branches,
retained lowering, links/anchors/UTF8/LF/private scope. This local review
aid is not a maintained parser/schema or tool test. Tracked prose, source
symbols and exact Git objects own durable findings. Run in the assigned
checkout with fresh nonexistent receipt names:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1182/review.py <configured-source-disasm-root> --receipt local/issue1182/direct-reproduction-01.json --snapshots local/issue1182/analysis-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Source argument ends in disasm. Review reuses observe-01/snapshots; no
pair regeneration, world preparation, ending execution or runtime launch.
The observation reused18,285,979 bytes, unchanged size/mtime before/after,
with small selected output. Actual durations/bytes, head/tree/base,
planner, exact-head scoped CI, process state/output totals belong to the
frozen Issue/PR handoff. Normal/full, SDK/Godot/emulator/H3/H4, old suites
and screenshots are **SKIPPED** by documentation-only scope, not passed.
ADR0004's provenance/date, width/order/polarity, alias, changed-state,
actual-consumer/private checklist applies; maintained parser/schema
mutation and H3 callback obligations are N/A.

Preserve [#1180's retained failures](./battle43-zeon-guard-laser.md#reproduction-retained-failures-and-stop):
KeyError98 from unknownByte-as-kind and correction; #1176 ENTNUM32/
scene declaration62, 62 versus later/combat13, 6; #1172 Sleep/Stun/publication
corrections; after42 special251 refusal/five-op partial/14-action eas_Init
cache without published world; Map54 overwrite, Map53 failed import/zero
partial and twelve completed old comparison failures. None is rerun,
relabelled interrupted, hidden by new direct/CI passes or made a repair queue.

**Unknown:** natural43 reach, actual party/HP/status/inventory/queue,
flags/positions/RNG/debug input, service returns/intermediate mutation,
source-ROM equality, rendered timing/audio/palette/scroll completion,
persistence/save checkpoints, execution of cs_615E6 or zone36/cs_62658,
restored free walking, entire ending completion and original/remake
playability. Static branches/in-memory lowering do not close them.
Freeze clean pushed Draft PR for independent main-gate review; same task
owns corrections. No merge, next dispatch, environment/ref cleanup or
archival is authorized.
