<a id="map-3-to-battle-01-continuous-scenario-contract"></a>
# Map 3 至 Battle 01 连续场景合同

- 状态：**Accepted comparison definitions（比较定义已接受）**；已有受限离线 reference binding；剩余绑定与 H4 执行 OPEN。
- 已接受证据基线：`57d6cc296b77283eb5ee8a00b5121ecdfd132e1a`，包含 PR #504 的 neutral endpoint、PR #526 已接受的有界战后 Down extension、PR #528 保留的首次比较，以及 PR #533 修正后的 selected-R1 inputs 与比较。PR #504 acquisition source 保持为 `9c3ea03ac5f5b467ee744f1ac624870da2408443`。PR #526 拥有原版 extension；PR #528/#533 拥有实际比较结果。
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
- `Unavailable`：必需私有输入/溯源、原版 expected 字段或 actual 观测能力缺失。列明哪一侧/字段；不得填零、空列表或 remake 输出。原版值仍**Unknown**（**未知**）时定义为 OPEN，执行为 `Unavailable`，不能 skipped/PASS。已观测 FAIL 与 unavailable 断言同时保留。

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
| 7 — endpoint | 已接受的有界原版端点与 RA-12 input/effect 证据；PR #526 extension 的 binding/actual comparison 仍开放 | 比较全部 scenario state，不能有 pending battle/script/modal/transfer；在两个 host update 边界观测 settled player/camera，不造原版帧相等要求。settled endpoint 之后，接受有独立证据支持的 Down，并比较实际 displacement/state effect。PR #526 提供原版 expected effect，但当前 projector/run 尚未绑定或到达该边界；此断言与完整 5B 仍 OPEN。 |
| 8 — save/7C | 6A restart；每个 reached original scene dialogue/map/sprite/portrait/animation/music/SFX identity/binding 的私有清单与 ROM/source/extraction 溯源 | 无用户 save/load/suspend/checkpoint surface；restart 重建 layer 1。每个 consumed original scene resource 绑定已准入原版私有内容。缺私有输入为 Unavailable；必需原版场景内容的已观测 authored substitute 为 7C FAIL。MUSIC_JOIN/MUSIC_SAD_JOIN chord loops 和 host mute 不满足原版音频。公开发行仍在范围外。 |
| 9 — 8D presentation | reached program/operation 与 scene/dialogue/animation/audio resource identities、dispatch/consumer/ack boundaries、blocking/resulting state，来自 accepted source 与 bounded observation | 匹配语义 identity/因果顺序；按下节观测实际 host use 与 completion/ack。request/mailbox pair、程序 return 或 counter 单独不能使 delivery PASS。缺原版消费证据 OPEN；缺 host 观测 Unavailable。不使用截图。 |
| 10 — deviations | ADR0010 1A/2A/4A/6A/9A/10A 与下述清单 | 每个已接受 deviation 及其 expected behavior 独立命名输出，即使 PASS。不允许隐含排除、缺输入豁免或新增未接受 deviation。 |

Layer 8 区分原版场景内容与现代界面资源。按既有[呈现所有者](../../../../remake/docs/presentation-and-assets.md#fonts-theme-and-input-glyphs)与 9A 边界，现代 HUD/theme、语义 input glyphs 和 fonts 检查已接受作者/许可、准入 asset binding 及配置的 input/accessibility 行为；不额外要求 ROM 原版字体或 UI 资源。这是既有边界，不新增 deviation，也不豁免原版场景 dialogue/graphics/animation/music/SFX 溯源。

<a id="exact-observed-endpoint"></a>
## 精确已观测端点

**Confirmed**（**已确认**），仅限已接受原版末段：map **57**、player tile **(5,12)**、raw **(1920,4608)**、facing **3/DOWN**、battle sentinel **255**、F401=false、F501=true；party/joined/active roster **[0,1,2]**，gold **420**。三名 ally 的 status-effect word 均为零。

| Actor | Level | HP current/max | MP current/max | Raw item slots | Raw spell slots |
| --- | ---: | --- | --- | --- | --- |
| Bowie / 0 | 1 | 12/12 | 8/8 | `[199,127,127,127]` | `[10,63,63,63]` |
| Sarah / 1 | 2 | 12/12 | 12/12 | `[213,127,127,127]` | `[0,63,63,63]` |
| Chester / 2 | 2 | 12/12 | 0/0 | `[184,127,127,127]` | `[63,63,63,63]` |

item identity 按 `0x7F` mask 解码并保留 equipped state；127 是 empty。用既有 source decoder 保留 spell identity/rank 和 empty 63，不以名称/字符串比较。没有剩余 herbs。其他 stats/flags/records 从已接受 private terminal 比较，不从摘要补齐。player field entity position 不等于 ally battle-record position。RNG bytes `[188,203,0,0]`、copy 188 和 raw time `(frame=170, seconds=674, secondsFrames=40)` 是溯源/readback 值；需要合理的语义 RNG mapping，不要求 raw host-clock 相等。

movement/camera settled，map-event word/typewriting/pending returns/active consumers 为零，无 map program/battle return/transfer/modal。两帧都有原版 input poll，末次 player poll `0x4FF8` 读到 0。generic window-state byte 1 不是阻塞 dialogue。这是 PR #504 未改变、terminal/nonresumable 的 neutral endpoint。

**Confirmed**（**已确认，有界 RA-12 extension，PR #526**）：新的兼容 natural chain 重现该 pre-input endpoint，然后在 61011 帧观察一个 Down frame。`esc02_controlCharacter` 于 `0x4FF8` 从 `PLAYER_1_INPUT`（`0xFFDE97`）读取值 2，D7=48 表示选择此 input source；`loc_52E8`（`0x52E8`）接受目的地增量 D2=0、D3=32、D4=0、D5=384。经过 13 个 neutral frame，player 在 61023/61024 帧稳定于 Map57 `(5,13)`、raw `(1920,4992)`、facing DOWN，保留原版 raw displacement、battle255、F401=false、F501=true，且无阻塞 consumer。这是独立接受的 terminal pair，不修改 PR #504 terminal，也不使其可恢复。RA-12 的有界 ordinary-input 接受与位移已有证据。现有 projector 仍只绑定 PR #504 neutral endpoint；此 extension 尚无离线 projector binding 或 H4 actual observation。

<a id="mapping-to-existing-actual-observations"></a>
## 既有 actual 观测映射

以下只读映射描述已接受实现，不是原版证据，也不声称 observer 完整。优先复用：

| 既有 surface | 比较用途及限制 |
| --- | --- |
| [SessionContract.cs](../../../../remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs) | `CommandEnvelope` SessionId/ExpectedRevision/Actor/Command；`SessionResult` Failure/StopReason/Observations；`SessionSnapshot` Mode/Active/Story/Selection。`SessionObservation` Sequence/Revision/Kind/Actor/Target/Before/After/From/To/RandomRange/RandomValue/Program 提供有序语义变化。按 source meaning 映射；序号只需单调，不必等于原版 callback count。采集每个 result，不能只取最新 snapshot。 |
| [ExplorationSessionView.ReadObservationJson](../../../../remake/game/src/Exploration/ExplorationSessionView.cs) | `sessionId`、`map`、`party`、`partyLists`、`gold`、`flags`、`mainSeed`、`entities` position/facing/moving/busy、`cursor`、`wait`、`token`、`stop`、`battleMounted`、`textId`、`speaker`、`speakerFlags`、`visibleCharacters`、`totalCharacters`、`observations`、failure fields。按 content identity 将 `map-57` 映射为 map 57。无 story wait/cursor、entity settled、battle view released 支持 readiness，但单独不证明 input delivery 或全部 stats。 |
| [BattleSessionView.ReadObservationJson](../../../../remake/game/src/Battles/BattleSessionView.cs) | `round`、`turnOrder`、`queueCursor`、`actor`、`stage`、`target`、`spell`、`itemSlot`、`inventories`（actor 与持有物品字）、`previewX/Y`、`actors`、`mainSeed`、`thinkingSeed`、`gold`、`regionFlags`、`aiMemory`、`observations`、failure fields。PR #521 已接受 `SelectItem`、live carried inventory/slot 及普通 healing-item 使用后的消耗。host 投影缺值时使用 typed snapshot，不可伪造 per-draw/scene-consumption record。 |
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

<a id="offline-reference-bindings"></a>
## 离线参考绑定

使用维护的[只读投影器](../../../../src/sf2tool/remake_h4_reference.py)，显式选择本地 retained `issue496`，输出必须是自己工作区 `local/` 内的新文件：

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_reference --evidence-root $acceptedEvidenceRoot --output local/issue522/reference.json
```

`$acceptedEvidenceRoot` 只指向已接受的 prepared-68..86，绝不指向运行中的 #515。程序显式读取这 19 个目录；复用 `_read_segment`，仅在私有 globals 副本中将 `repo_path("local")` containment lookup 绑定到证据所在 local 根。原采集模块及可写 helper 不变。终端 86 使用 `require_resumable=False`；不 load、seal、reconcile 或重写旧 state。校验既有文件摘要、prepared config/input identity、固定 ROM/upstream/runner/observer、parent pair link、ordinal/累计 accounting、成功请求与实际 frame delivery。identity 错误或不支持的映射使命令失败，不产生 comparison PASS。记录的原 observer/runner identity 是来源，不用当前 collector 替代。

生成 JSON 为私有输出，不是公开 fixture/schema。每个绑定含 prepared segment、JSONL 文件与一基行号、kind/order/frame 及最近 request ordinal；order 仅定位原版记录，不要求现代时序相等。`logicalInputs` 将 movement poll、prompt choice、menu return、committed player decision 按顺序关联。`requests` 区分 **2,798 controller schedules** 与 **18 acquisition saves**；后者不是 6A 用户存档。neutral schedule 只保留 timing/RNG 来源，不能猜成 STAY/Confirm。request result state 是整次请求结束的实际观测，不是每次先前 movement poll 的独立效果；逐 movement arrival 与完整 field logical-input normalization 仍可能 OPEN。

**Confirmed**（**已确认**），限所选原版链：

| 绑定 | 来源与语义边界 | 覆盖及限制 |
| --- | --- | --- |
| `admission`、`inherited` | prepared-68 的 `natural:r1` 与 `r1:inherited-status-and-live-entities` | map/位置/朝向、具名 flags、party/joined/active、gold、序列化 combatant 字段、四个完整二字节物品槽与 status word；48 个 physical entity 与 logical index table，位置/destination/facing/layer/action-script/wait。R1 实际 seed bytes `[153,23,0,0]`、copy 0。R1 未输出的完整 flags、base-stat/prowess/EXP 不得用之后状态补齐。 |
| `requests/movements/choices/menus/decisions` | 成功 command/result 与真实 source consumer | 两次 Yes；41 个已提交玩家动作：21 Stay、13 Attack、3 CastSpell（HEAL 1）、4 UseItem（Medical Herb）。itemSlot 从零计数，对照 actor 当前四字 inventory；此链 menu/player return 无 cancel sentinel，cancel/reselect 分支仍缺。 |
| `encounter/turns/checkpoints` | natural/load/start/activate/region/spawn/generation/dispatch 与函数前后 | first actor 2、103 dispatched turns、14 generated orders，combatant/status/equipment 及已序列化 AI 前后状态；full combatant records 的 AI memory 尚待解码；main/debug RNG callback 不采集 thinking-helper draw。 |
| `scenes`、decision `effects` | WriteBattlesceneScript 前 → ApplyActionEffect 前 → Initialize/Execute → EndBattlescene 后 | 44 scenes；在 consumed boundary 比较 HP/MP/status/items/EXP/gold。player-return targets 是候选列表；真正 effect target 使用 construction 后列表。此次每个 effect 只有一 target；多目标解码尚不支持，显式失败。 |
| `rng` | base generator 与 debug-aware wrapper 的 entry/return/draw | 2,935 base advances 使用 D6 range/D7 result，全部满足已有 16-bit 更新与缩放规则；250 wrapper 使用 D0 range/result，匹配内部 base call，**不是额外 draw**。caller PC/consumer scope 仅定位来源，不证明具体 draw-to-effect。thinking-copy draw 及这些 callback 外的 frame/menu mutation 仍 OPEN。 |
| `story/operationPairs/audioPairs` | script/operation/text/warp/setup 与真实 sound consumer/mailbox | 全链 363 对 reached operation，其中 after-program 67 对；2,552 dispatch/mailbox pairs，末段 106 对。只证明 source seam，不证明视觉/听觉 delivery。 |
| `victory/endpoint` | victory/after-tail/enclosing return/flag/loop/transfer 与 terminal stop | PR #504 neutral endpoint；terminal accounting 的 item/status/HP/MP 与保留 RAM 独立对照，完整 flags 对照 128 bytes。field 位置与 battle combatant 坐标分开。PR #526 独立接受 Down 到 `(5,13)` 的 extension；当前 projector/reference 表尚未绑定其 payload。 |

数字 action mapping 来自 `map3-battle01-action-effect-static-v1` dispatch 与 battle-AI/function 的 STAY consumer。item mask/slot 使用已接受 herb config 与 item/stat owner；spell identity/rank 使用既有 spell/AI packing。不同 RNG 寄存器合同由 [randomness](../../contracts/randomness.md) 和 `rng-v1` / `debug-rng-v1` fixture 拥有。expected 不来自 remake code。

`coverage` 按十层列出字段组。`decoded` 只表示原版字段有可执行绑定，没有 actual remake 时 `comparisonResult` 始终 `Unavailable`。`retained-not-decoded` 是既有待解码记录，`static-supported` 是静态规则，`missing-semantic-binding` 是解释缺口；`missing-original-detail` / `missing-original-branch` 与 `missing-remake-observation` 分别报告原版细节/分支与 actual 缺失。它们均不自动派生 native 队列。

既有 [dialogue](../../contracts/dialogue-system.md)、[text/font](../../contracts/text-and-font-system.md)、[music wait](../../contracts/music-wait-service.md) 与 [battle scene](../../contracts/battle-scene-presentation.md) 提供静态 identity/解码/command order/resource/wait 规则，可支持继续离线绑定；不能将 shimmed DisplayText return 变成自然 reveal/ack，将 mailbox 变成播放，将 scene entry/end 变成逐 command animation/resource-consumption trace。原版音频资产与实际 host consumer 仍是独立要求，不引入硬件/frame/pixel/waveform equality。

验收采用完整链直接命令、真实 action/scene/RNG/terminal 映射检查、具备配置时的 normal repository check、文档/翻译校验与 committed dependency plan。不增加 verification-code 测试、CLI、公共 payload、schema 或 planner rule；保守 planner 选择不能扩大本 slice 的 native/full 授权。失败/不完整投影保留 ignored，修正记在 Issue handoff。reference projection 永远不等于 H4 PASS。

<a id="remaining-acceptance-work"></a>
## 剩余验收工作

**Unknown / OPEN 原版字段**（**未知**）：R1 未序列化的完整 flags 与基础属性/prowess/EXP、已接受单次 Down extension 以外的完整 field logical-input effect、AI 内部 memory/thinking draw、逐 draw-to-effect 映射与 timing normalization、cancel/reselect、unshimmed required dialogue 与其他不完整 8D consumer boundary。PR #526 仅关闭有界 RA-12 input/effect observation，不证明完整 5B、剩余 8D 或里程碑。当前 projector 仍使用 PR #504 terminal，尚未绑定此 extension payload。

**Confirmed**（**已确认的比较结果**）：PR #528 的已完成 baseline 报告 5,336 PASS、6 FAIL、40 Unavailable；六个失败和原报告作为历史证据保留。PR #533 将 connected selected R1 product inputs 修正为 source NewGame gold 60 和完整起始 item words，同时保留独立 controlled R1 fixture 的 gold 0/four-byte projection。修正后的 actual host run 完成 normalized field route、73 个 reached text ID 与 natural first actor 2，然后因 diagnostic next-actor divergence 停止。报告 5,340 PASS、2 FAIL、40 Unavailable，`milestonePass: false`。Gold 60 在准入时确认；完整 item arrays 在 natural first control 实际观测，且与 pinned NewGame source 支持的起始槽位相符：Bowie `[199,0,127,127]`、Sarah `[213,0,0,127]`、Chester `[184,0,127,127]`。这些后续 inventories 不能证明 admission `SourceLoadout`，actual projection 中该值仍为 null。First-round order 仍不同；首次 diagnostic STAY 后，original next actor 是 Bowie，actual 是 Sarah；host exit 2 与该比较结果互证。PR #526 post-victory extension 未重新绑定且此 run 未到达。NPC phase 与 timing/RNG mapping 仍 Unknown。该修正比较仍不是 H4 接受。

**OPEN 内容/实现**：7C 音频与完整 reached asset provenance、snapshot/cue correlation、actual battle-scene consumers、连续比较剩余覆盖及适用 host/9A 执行。现有 comparator 已有 accepted diagnostic result；其余 layers/variants 尚未完成。Medical Herb 和 carried inventory 观测已由 PR #521（`78c201c3`）接受：普通 `SelectItem`、live inventory、`ReadObservationJson` 的 `inventories`/`itemSlot` 及使用后消耗可供 actual 映射；它不是此连续比较的 PASS。音频（#517）与 battle scenes（#523）仍是独立 OPEN 实现。不得用 remake 限制删除原版 reached action。已接受比较定义不代表完整定义或里程碑就绪。[就绪台账](../synthesis/map3-battle01-readiness.md)记录闭合，main-gate 独立验收。离线投影不执行 H4，不启动 native acquisition。
