<a id="map-3-to-battle-01-continuous-scenario-contract"></a>
# Map 3 至 Battle 01 连续场景合同

- 状态：**Accepted comparison definitions（比较定义已接受）**；executable binding 与 H4 执行 OPEN。
- 已接受证据基线：`58c5a94a4c5349fd221a130a0970222962715528`，包含 PR #504 / `5102804b`；已接受观测源码为 `9c3ea03ac5f5b467ee744f1ac624870da2408443`。
- 产品：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)，`1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`。
- 范围：受控准入、自然必经路线、获胜战斗与战后 field 边界；组合既有所有者，不注册 fixture、schema 或研究关联。

本文件同步[英文合同](../../contracts/map3-battle01-continuous-scenario.md)，英文为审阅基线。证据标签按 AGENTS 保留精确英文 Confirmed / Inferred / Unknown，可附中文解释；其他术语遵循[术语表](../../glossary.md)，源码标识符保持原样。

<a id="evidence-and-result-rules"></a>
## 证据与结果规则

原版 expected 来自[准入合同](../../contracts/map3-controlled-admission.md)、[研究字段审计](../../../research/map3-battle01-audit.md#accepted-evidence-and-exact-field-mapping)及[已接受最终采集](../../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)。后者拥有私有成功 prepared-68..86 链、18 个已验证 parent pair、实际 input/checkpoint/observer 记录和只读复现命令。源码固定于上述对象；ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`，SF2DISASM 为 `c834c652b6862bc5679fd7f69a38a7093206efc6`，模拟器为 BizHawk 2.11.1 / Genesis Plus GX。

精确逐步值由私有记录提供，本合同不创造公开 golden。旧败局和失败请求不属于获胜输入流，其失败/成本保留在研究所有者。prepared-86 是 terminal/nonresumable，旧 first-player-ready terminal 也不能用作 continuation parent。

每条比较结果必须标明 layer、assertion、原版所有者/Git 对象/记录位置、逻辑 checkpoint、expected/actual 语义值、输入序号、remake 观测序号与结果/原因。私有 payload 留在本地，公开结果只含安全身份与有界差异。下述字段名是比较词汇，不是新 wire schema。

- `PASS`：双方值存在，命名边界上的断言成立。
- `FAIL`：已观测不匹配、禁止的转换、runtime error、必需动作不受支持、在已有可观测执行中缺少必需 host completion，或有归属的 timeout。
- `Unavailable`：必需私有输入/溯源、原版 expected 字段或 actual 观测能力缺失。列明哪一侧/字段；不得填零、空列表或 remake 输出。原版值仍**Unknown（未知）**时定义为 OPEN，执行为 `Unavailable`，不能 skipped/PASS。已观测 FAIL 与 unavailable 断言同时保留。

十层全部必需；整体 PASS 要求全部必需断言和 accessibility variants 通过。证据就绪、定义审阅、实际 H4 执行分别报告。准入后重启 session、注入 story state、重设 seed、直接进入战斗或跳过战后程序均违反连续性。全程保持同一 session identity 与单调观测，保留拒绝/error 记录。不要求 frame/pixel/waveform/chip/hardware 相等，但影响玩法的 timing、因果顺序和输入阻塞仍在范围内。

<a id="admitted-state-and-ordered-route"></a>
## 准入状态与有序路线

使用 R1 fixture `sf2-map3-admitted-start-runtime-v1` / case `controlled-new-map3-default` 在首次 `WaitForEvent` 的逻辑投影。按已记录宽度与含义比较 map、位置、朝向、flow、party/roster、gold、flags、stats、items、spells。[投影限制](../../../research/map3-battle01-audit.md#what-the-admitted-projection-does-not-contain)仍有效：公开 R1 缺 status，四个 item bytes 不是四个完整双字节槽位，normalized time 不是 raw time。获胜链的继承 status、完整装备、RNG/copy 和相关 NPC phase 必须来自已接受私有 readback。不能假定 reset POISON=0，不能使用 R2d seeded actor/order，也不能强加 `0x1234` 为自然 seed。缺失 readback 保持 OPEN。

选定顺序：

1. 受控 Map3 opening → 真实 messenger/prompt 接受 → follower-ready closure/F603 → gate `cs_51652`/F604 → north warp 与 Map19 init return。
2. Map19 `(26,30)` 输入 → 实际位移 `(26,29)` → royal/guard programs/returns → 自然城堡/塔 Maps21/40/57。必须保留选定链每个中间 source operation/warp checkpoint，不能只比较最终 map 列表。可选互动/FieldMenu 不属于该参考路线，但不可遗漏必需 dialogue/choice/trigger。
3. CheckBattle/new-battle branch → before `bbcs_01` → LoadBattle → start `ms_Empty`/F451 → activation/region/spawn/order generation → 实际首个 actor **2**，`ControlBattleEntity` 在 WaitForVInt 后、input read 前。脚本 `loadMapFadeIn` 场景变化不是额外玩家 warp。
4. 成功逻辑决策至 round **14** / turn **103**：movement、STAY、physical attack、HEAL 1、Medical Herb，包含该链实际 target/resource/scene/effect/after-turn 结果。较早 herb/Heal/defeat 分支的值仅证明本分支，不得拼入获胜 expected。4A 仍要求一次 cancel/reselect；未观测的原版分支（含 herb cancel）保持 OPEN。
5. 自然胜利 → `abcs_battle01` → shared tail/enclosing return → F401 clear → F501 set → BattleLoop D4=1 → SwitchMap return → ExplorationLoop → Map57 `ms_Void` → 稳定 field 边界。

最终原版链在 frame 58657 胜利、60945 战后程序返回、61009/61010 稳定。这些是证据定位，不是 remake 时钟目标。**67 个匹配的 reached operation pairs** 不等于静态 80 记录的 dispatcher iterations；静态语料还含嵌入 entity actions/data。R2b/R3/R4a 静态所有者解释规则/结构，只有选定观测证明自然到达。已验证 savestate-linked 连续性不声称不中断墙钟或自然 New/load。

<a id="ten-h4-comparison-layers"></a>
## 十层 H4 比较

各行共同输入为声明的准入、成功逻辑输入流和该行已接受原版记录。相等指语义值，不指对象布局或 PC。后节列出 actual 既有 surface 和缺口。

| Layer | 字段及原版 expected 来源 | 对 actual remake 的必需断言 |
| --- | --- | --- |
| 1 — admission | R1 投影与选定链 readback：map、player position/facing、party/joined/active lists、flags、gold、逐 actor level/HP/MP/stats/status/items/spells、RNG、相关 continuation phase；ROM/source/configuration/parent identities | 首次用户命令前逻辑起点精确相等；分别校验溯源、完整槽位/status。按 1A 报告受控构造。fixed seed 必须来自已接受链；未映射 RNG/time dependency 保持 OPEN。 |
| 2 — input/route | 有序成功 input records：checkpoint、logical action、actor、direction、target、choice、accept/reject、movement/transition 结果 | 逻辑决策与上述路线因果 handoff 相同。比较接受的 destination/trigger 和 cancel/reselect 效果，不比较按键持有帧数。每个 ally turn 均可由玩家操作 Confirm/Cancel。物理请求缺逻辑解码则 OPEN，不能猜。 |
| 3 — world/story | R2/R2a 与选定链：map/setup selection、program/operation entry-return、dialogue ID/speaker/choice、entity position/facing/visibility、roster/followers、flag before/after | reached mutation 相等，因果顺序相同；map load、entity motion、dialogue 在规定 wait 完成。比较实际选定 operation/branch，静态 corpus membership 不足。真实 YesNoPrompt return 有证据，shimmed DisplayText consumption 没有。 |
| 4 — natural encounter | 选定 CheckBattle/load/start/first-control records；R2c/R2d 仅供字段结构：battle ID、before/start programs、F88/F451、region flags 90–105、party/combatants、position/stats/status/equipment、activation/spawn、turn scores/order/cursor、first actor、readiness guards | 自然路线建立 Battle01，程序完成后才交给手动控制；比较 actor 2 与选定精确初始值。无阻塞 script/modal/transfer/action/target/scroll。原版 window count 2/palette mode 5 是允许的非阻塞呈现，不要求 host 字节同值。 |
| 5 — battle | 选定 action/checkpoint/scene records 与 R3a–R3d/局部规则：round/order/actor/control、movement origin/path/destination、action/resource/slot/target、AI choice/memory、RNG before/range/value/after、follow-up kind、逐 target HP/MP/status/death、item removal、EXP/level/stats/spells/gold/drop、after-turn/outcome | 逐项匹配 reached 决策与 consumed effect 顺序，包括资源消耗和 RNG 结果。比较 WriteBattlesceneScript 前 HP 与 EndBattlescene 消费后 HP，不能使用脚本计算临时 HP。有证据的每次 RNG draw/effect 必须配对；draw mapping 缺失即使末端 HP 相同也 OPEN。round 14/actor history 不能成为 gameplay legality。 |
| 6 — victory/return | 选定末段与 R4a：winning condition、eligible-party healing、reached after-program operations/effects、joins、F401/F501、controller result、transfer/setup | 自然胜利和全部 reached entry/return 成对；shared tail 在 enclosing return 前，继而 flags clear/set、D4=1 等效结果与 exploration handoff。源码 `0x477E8` 的 `ms_Void` 是 Map57 精确 fallback。单个 return 不替代战后消费。 |
| 7 — endpoint | 下述精确有界值、pending consumers/readiness、RA-12 ordinary-input 证据 | 比较全部 scenario state，不能有 pending battle/script/modal/transfer；在两个 host update 边界观测 settled player/camera，不造原版帧相等要求。随后接受独立有证据的 nonneutral logical input 并比较效果。当前原版效果**Unknown（未知） / OPEN**，neutral readiness 不能使完整 5B PASS。 |
| 8 — save/7C | 6A restart；每个 reached original scene dialogue/map/sprite/portrait/animation/music/SFX identity/binding 的私有清单与 ROM/source/extraction 溯源 | 无用户 save/load/suspend/checkpoint surface；restart 重建 layer 1。每个 consumed original scene resource 绑定已准入原版私有内容。缺私有输入为 Unavailable；必需原版场景内容的已观测 authored substitute 为 7C FAIL。MUSIC_JOIN/MUSIC_SAD_JOIN chord loops 和 host mute 不满足原版音频。公开发行仍在范围外。 |
| 9 — 8D presentation | reached program/operation 与 scene/dialogue/animation/audio resource identities、dispatch/consumer/ack boundaries、blocking/resulting state，来自 accepted source 与 bounded observation | 匹配语义 identity/因果顺序；按下节观测实际 host use 与 completion/ack。request/mailbox pair、程序 return 或 counter 单独不能使 delivery PASS。缺原版消费证据 OPEN；缺 host 观测 Unavailable。不使用截图。 |
| 10 — deviations | ADR0010 1A/2A/4A/6A/9A/10A 与下述清单 | 每个已接受 deviation 及其 expected behavior 独立命名输出，即使 PASS。不允许隐含排除、缺输入豁免或新增未接受 deviation。 |

Layer 8 区分原版场景内容与现代界面资源。按既有[呈现所有者](../../../../remake/docs/presentation-and-assets.md#fonts-theme-and-input-glyphs)与 9A 边界，现代 HUD/theme、语义 input glyphs 和 fonts 检查已接受作者/许可、准入 asset binding 及配置的 input/accessibility 行为；不额外要求 ROM 原版字体或 UI 资源。这是既有边界，不新增 deviation，也不豁免原版场景 dialogue/graphics/animation/music/SFX 溯源。

<a id="exact-observed-endpoint"></a>
## 精确已观测端点

**Confirmed（已确认）**，仅限已接受原版末段：map **57**、player tile **(5,12)**、raw **(1920,4608)**、facing **3/DOWN**、battle sentinel **255**、F401=false、F501=true；party/joined/active roster **[0,1,2]**，gold **420**。三名 ally 的 status-effect word 均为零。

| Actor | Level | HP current/max | MP current/max | Raw item slots | Raw spell slots |
| --- | ---: | --- | --- | --- | --- |
| Bowie / 0 | 1 | 12/12 | 8/8 | `[199,127,127,127]` | `[10,63,63,63]` |
| Sarah / 1 | 2 | 12/12 | 12/12 | `[213,127,127,127]` | `[0,63,63,63]` |
| Chester / 2 | 2 | 12/12 | 0/0 | `[184,127,127,127]` | `[63,63,63,63]` |

item identity 按 `0x7F` mask 解码并保留 equipped state；127 是 empty。用既有 source decoder 保留 spell identity/rank 和 empty 63，不以名称/字符串比较。没有剩余 herbs。其他 stats/flags/records 从已接受 private terminal 比较，不从摘要补齐。player field entity position 不等于 ally battle-record position。RNG bytes `[188,203,0,0]`、copy 188 和 raw time `(frame=170, seconds=674, secondsFrames=40)` 是溯源/readback 值；需要合理的语义 RNG mapping，不要求 raw host-clock 相等。

movement/camera settled，map-event word/typewriting/pending returns/active consumers 为零，无 map program/battle return/transfer/modal。两帧都有原版 input poll，末次 player poll `0x4FF8` 读到 0。generic window-state byte 1 不是阻塞 dialogue。**Unknown（未知）**：下一次 nonneutral input 的接受及状态效果（RA-12）；remake movement 不能提供原版 expected。

<a id="mapping-to-existing-actual-observations"></a>
## 既有 actual 观测映射

以下只读映射描述已接受实现，不是原版证据，也不声称 observer 完整。优先复用：

| 既有 surface | 比较用途及限制 |
| --- | --- |
| [SessionContract.cs](../../../../remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs) | `CommandEnvelope` SessionId/ExpectedRevision/Actor/Command；`SessionResult` Failure/StopReason/Observations；`SessionSnapshot` Mode/Active/Story/Selection。`SessionObservation` Sequence/Revision/Kind/Actor/Target/Before/After/From/To/RandomRange/RandomValue/Program 提供有序语义变化。按 source meaning 映射；序号只需单调，不必等于原版 callback count。采集每个 result，不能只取最新 snapshot。 |
| [ExplorationSessionView.ReadObservationJson](../../../../remake/game/src/Exploration/ExplorationSessionView.cs) | `sessionId`、`map`、`party`、`partyLists`、`gold`、`flags`、`mainSeed`、`entities` position/facing/moving/busy、`cursor`、`wait`、`token`、`stop`、`battleMounted`、`textId`、`speaker`、`speakerFlags`、`visibleCharacters`、`totalCharacters`、`observations`、failure fields。按 content identity 将 `map-57` 映射为 map 57。无 story wait/cursor、entity settled、battle view released 支持 readiness，但单独不证明 input delivery 或全部 stats。 |
| [BattleSessionView.ReadObservationJson](../../../../remake/game/src/Battles/BattleSessionView.cs) | `round`、`turnOrder`、`queueCursor`、`actor`、`stage`、`target`、`spell`、`previewX/Y`、`actors`、`mainSeed`、`thinkingSeed`、`gold`、`regionFlags`、`aiMemory`、`observations`、failure fields。host 投影缺值时使用 typed snapshot，不可伪造完整 inventory/per-draw/scene-consumption record。 |
| [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs) 与 exploration projection | `presentation.activeCue`、`completedCueToken/Kind`、sprite request/ready、gesture/fade/mosaic counters、`soundStarts/Fades`、`error`。token/kind 必须关联真实 `CompletePresentation`、actual resource/node、state transition。aggregate counter 不标识 cue，也不证明完整播放/asset provenance；缺 correlation 保持实现 OPEN。 |
| [既有 outcome probe](../../../../remake/game/probes/engine_battle01_outcome_observation.gd)、[9A 所有者](../../../../remake/docs/development-and-verification.md#native-9a-observation) | 复用 actual input、single-session、wait/token、after-program、return、movement 观测方法。既有 PASS 是有界实现证据，不是本获胜原版 trace 或连续 H4 PASS。physical driver/hot-plug 和完整 export 未验证。 |

<a id="presentation-and-accessibility-assertions"></a>
## 呈现与无障碍断言

每个 reached cue 按因果顺序比较 `(program, operation, resource, subject, occurrence)`。保留 request → actual consumer start/use → required completion/ack → resumed program/input 的边。独立并发 cue 不强造 hardware total order；Unknown（未知） dependency 明示。presentation wait 仅能由匹配 token/kind 和必需 consumer completion 结束；scene committed HP/resource effect 要出现在对应 resolution edge。

- Dialogue：比较 text ID、speaker/portrait、choice result、实际显示的 private content binding、reveal completion、acknowledgement、wait release。[研究呈现审计](../../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)记录 `0x6260` 的 `display-text-rts`；ID/return 不证明 unshimmed original acknowledgement。即使 remake Label 正常，该原版边界仍 OPEN。
- Animation/scene：比较 source entity/resource/action identity、可见 host consumer，以及依赖 program/input 恢复前必需的 motion/gesture/fade completion。被裁剪/缺席的 subject 必须遵循有证据的语义规则，不能默算 rendered completion。
- Audio：command namespace/resource 绑定 original private asset 和实际 player start；检查 reached replacement/fade/stop/resume。persistent music 保持至必需 replacement/stop，不造 track-ended event。末段 **106 dispatch/mailbox pairs** 仅证明有界 source seam，不证明完整消费或 7C 溯源。

Battle scene 需要独立 consumer 证据。对每个 reached action/target/follow-up，将已接受 `InitializeBattlescene` → `ExecuteBattlesceneScript` → `EndBattlescene` 观测绑定到[battle-scene 合同](../../contracts/battle-scene-presentation.md)：scene occurrence、actor/target、background/actor/weapon/spell-animation resource IDs、有序 scene command/operands、reached dialogue identity、HP/MP/resource effect edge、blocking/wait/completion boundary。actual host 必须标识对应 mounted scene/resources、开始/完成的 animation/dialogue consumers、匹配 completion token 或等效 occurrence identity、committed effect 与 battlefield input 返回。要求 initialization 先于 command consumption，required wait 先于依赖推进，consumed effects 先于 EndBattlescene 等效释放，blocking scene 未完成时不得释放输入。缺 original resource/order binding 保持 OPEN；缺 actual observation 为 Unavailable，已观测遗漏必需 scene 为 FAIL。当前 [BattlePresentation](../../../../remake/game/src/Battles/BattlePresentation.cs)仅投影 board markers/status/roster，不证明 battle-scene animation/dialogue consumption。这是显式 layer-9 实现缺口，独立于 exploration cue services、battle-state correctness 与 private audio 工作。不需要 original pixels/frame durations。

9A 分别报告 variant identity/settings，并将相同 layer 2/3/5/7 玩法决策/状态与 baseline 比较：default/remapped keyboard/gamepad、standard/swapped Confirm/Cancel、reduced-flash/normal、instant/adjustable text。未执行必需 variant 为 Unavailable。swapping 只改物理绑定，不改命令含义。reduced-flash 抑制 white overlay，但完成相同 token/kind/state effect。adjustable text 首次 Confirm 揭示未完成文字而不消费 wait，后一次才 acknowledge；instant 仍需 acknowledge，choice 保留 Yes/No。original device cadence/text duration 不要求相等。必须观察 actual host settings/input/projection，不能仅注入 SessionCommand；既有 authored paired observation 不替代 continuous variant execution。

Layer 10 独立报告：受控构造（1A/layers1–2）；排除 optional 但保留 mandatory route（2A/layers2–3）；有证据 fixed seed/logical trace、manual agency、禁止 live reseed（4A/layers1,2,5,7）；无 save surface 与 restart equivalence（6A/layers1,8）；所有 9A variants 的 acknowledgement/state equivalence（layers2,3,5,7,9）；每个明确 out-of-domain safe/Unsupported 行为（10A/affected layer）。out-of-domain safety 不得豁免 in-domain required action。7C private-only handling 是产品边界，不是 deviation。

<a id="remaining-acceptance-work"></a>
## 剩余验收工作

**Unknown（未知） / OPEN 原版字段**：尚未绑定比较记录的完整 selected logical trace/seed/draw-to-effect projection、缺失 status/continuation、cancel/reselect、unshimmed required dialogue 和其他不完整 8D consumer boundary、RA-12 nonneutral effect。这是逐字段 evidence/adaptation 要求，不是新 native run 授权。已接受 [PR #519 source preparation](../../../research/map3-messenger-acceptance.md#post-victory-ordinary-input-preparation-issue-515) 仅提供 collector 能力，不是 native RA-12 结果；nonneutral acceptance/effect 仍 OPEN，后续 Research #515 观测须 main 独立接受后消费。

**OPEN 内容/实现**：7C 音频与完整 reached asset provenance、必需手动 Medical Herb input（检查到的 `SessionAction` 仅有 Stay/Heal/PhysicalAttack）、缺失 snapshot/cue correlation、上述 actual battle-scene consumers、continuous comparator、全部适用 actual host/9A 执行。音频/host（#517）与 Medical Herb（#518）是独立实现所有者，未合并结果不闭合这些条目。不得用当前 remake 限制删除原版 reached action。独立审阅已接受比较定义及其明确 OPEN 边界，不代表完整定义就绪或里程碑就绪。[就绪台账](../synthesis/map3-battle01-readiness.md)记录闭合，main-gate 独立验收。本 slice 不实现 H4、不采集 native、不运行 suite。
