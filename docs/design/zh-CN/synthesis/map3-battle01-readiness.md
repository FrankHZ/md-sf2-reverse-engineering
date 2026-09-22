<a id="map-3-to-battle-01-readiness-ledger"></a>
# Map 3 至 Battle 01 就绪台账

- 状态：连续里程碑验收**未就绪**；不默认阻塞另行授权的实现。
- 已接受证据基线：`58c5a94a4c5349fd221a130a0970222962715528`，包含 [PR #504](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/504)。
- 里程碑：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)；画像：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)。
- 启动政策：[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md)；引擎方向：[ADR 0019](../../../decisions/0019-state-and-content-driven-remake-engine.md)。
- 定义所有者：[连续场景合同](../contracts/map3-battle01-continuous-scenario.md)。

本文件同步[英文台账](../../synthesis/map3-battle01-readiness.md)，英文为审阅基线；证据标签按 AGENTS 保留精确英文 Confirmed / Inferred / Unknown，可附中文解释；其他术语遵循[术语表](../../glossary.md)，源码标识符保持原样。

<a id="current-acceptance-scope"></a>
## 当前验收范围

选定画像为 `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`。8D 比较玩法与呈现语义，包含 actual host consumption、completion、acknowledgement 和 input readiness。不比较 pixel/frame/waveform/chip 及 VInt/DMA/CRAM/VDP；影响玩法、因果顺序或输入可用性的 timing 仍必需。私有原版内容、自然连续性、5B、独立报告的 9A/10A 仍必需。

本台账核算证据与验收，精确比较规则由合同拥有。原版证据、定义交付/审阅、actual remake H4 PASS 是三个不同状态。不消费未合并 Research/Remake 结果，不注册 fixture/schema/index association。Godot 验收读取实际运行 state/input/projection，禁止截图。本次文档变更不运行 normal/full/native suites。

<a id="accepted-original-frontier"></a>
## 已接受原版前沿

**Confirmed（已确认）有界原版观测**：[最终采集所有者](../../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)与[审计](../../../research/map3-battle01-audit.md)建立受控 R1 经 messenger、Map19、royal/guard、城堡/塔、自然 Battle01 准入、reached actions、自然胜利至战后程序返回后的稳定 field readiness。已接受源码为 `9c3ea03ac5f5b467ee744f1ac624870da2408443`，PR #504 / `5102804b` 接受结果。选定 fresh prepared-68..86 链验证 18 个 parent pairs，只用成功获胜请求。这是 savestate-linked 连续性，不是不中断墙钟或自然 New/load。

自然 first actor 为 **2**，R2d seeded actor 1 仍是显式受控 bridge。最终链到 round **14** / turn **103**，victory frame **58657**、`abcs_battle01` return **60945**，随后 shared-tail/enclosing return、F401 clear、F501 set、BattleLoop D4=1、SwitchMap、ExplorationLoop、Map57 `ms_Void`。**67** 个 reached operation pairs 不等于静态语料 80 条记录。

两个完成的 neutral frames **61009/61010** 确认 Map57、player `(5,12)`、DOWN、battle255、F401=false/F501=true、roster `[0,1,2]`、gold **420**、motion/camera settled，且无阻塞 script/modal/transfer/battle/consumer。精确 ally HP/MP/level/items/spells/status 与 RNG readback 见[合同端点](../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint)及原版所有者。generic window byte 1 不是 blocking dialogue。已观测 original input poll，但值为 neutral：**Unknown（未知） / OPEN RA-12** 是下一次 nonneutral input 的接受与效果。名为 `controllable-5b` 的证据 terminal 不证明完整 RA-12 或里程碑 PASS，且不可续跑。末段 106 audio dispatch/mailbox pairs 不证明完整 8D。

旧 R1/R2/R2a/R2d fixtures、Map19/first-player-ready 观测保留各自有界投影，不因末链而自动扩展。精确 post-447 wait entry 仍为**Inferred（推断）**；公开 R1 缺 status/完整 item slots。使用获胜链 readback，不复制别的 attempt 的 zero status 或 R2d seeded order。历史失败与 cleanup **Unknown（未知）**（含 21/41/61/67）保留在采集所有者；已完成 defeat/transport/observer failure 不是中断运行，也不会被成功抹去。

<a id="readiness-checklist"></a>
## 就绪检查表

| Gate | 当前结果 | 所有者 / 精确剩余边界 |
| --- | --- | --- |
| 里程碑、引擎、产品选择 | PASS | ADR0008/0009/0010；本次没有新产品决定 |
| 受控准入 | 有界 PASS | [准入合同](../../contracts/map3-controlled-admission.md)；连续链 status/完整 slots/NPC/RNG 投影缺失部分仍需绑定 |
| 自然必经路线及 encounter 准入 | 有界原版证据 PASS | 最终采集、actual actor 2；不是 R2d bridge |
| 获胜动作与 consumed results | 有界原版证据 PASS | 选定获胜链；完整 logical input/seed/per-draw 比较投影及 cancel/reselect 仍 OPEN |
| 胜利、战后程序、flag/return spine | 有界原版证据 PASS | 67 reached operation pairs 与最终链；不是完整呈现声明 |
| 精确 neutral settled endpoint | 有界原版证据 PASS | 合同端点与 original terminal；其余完整记录必须从私有证据消费 |
| 完整可控 5B / RA-12 | OPEN | Research：下一次 nonneutral input 及实际效果；不能用 remake 提供 expected truth |
| 连续合同与十层定义 | 定义已接受；缺失绑定 OPEN | 合同明确字段、来源、actual mapping、failure/unavailable；executable binding 与完整定义就绪仍 OPEN |
| 所有断言原版 expected 完整 | OPEN | 缺 selected trace/RNG/continuation/cancel 字段及必需 8D consumption/ack；不构成新 native 授权 |
| 6A save 政策 | 已选择；连续 H4 执行 OPEN | 无用户 persistence surface；restart 回到准入状态 |
| 7C 内容/溯源 | OPEN | 完整 reached 原版场景清单，尤其原版音频；authored JoinCue chords 或 mute 不能 PASS。现代 HUD/theme/input glyphs/fonts 按已接受作者/许可与 9A 检查，不要求 ROM 原版字体 |
| 8D 呈现语义 | OPEN | 必需 identity/order、real host use、completion/ack/readiness；source dispatch pairs 不足；BattlePresentation board markers/status/roster 不实现 battle-scene consumers |
| 9A 配置与有界直接观测 | 有界实现 PASS | [9A 所有者](../../../../remake/docs/development-and-verification.md#native-9a-observation)；不是全部连续 variants |
| 9A variants / 10A deviations 组合 | 定义已接受；缺失绑定与执行 OPEN | 合同要求 baseline/variant 分开结果及 state/ack equivalence |
| 必需 reached action 支持 | OPEN | 检查到的 SessionAction 没有手动 Medical Herb（#518）；保留原版 reached 要求 |
| 所有适用 H4 layers 成功执行 | OPEN | 定义接受后的 Remake/harness；实际连续 session 至完整 5B |
| 独立里程碑就绪接受 | OPEN | Main-gate；Issue 关闭或有界实现 PASS 均不足 |
| 单独实现启动授权 | PASS | [Remake README](../../../../remake/README.md)中的用户授权；不等于本里程碑接受 |
| 公开发行 | 私有里程碑范围外 BLOCKED | 另行 rights/licensed replacement 决定；私有 assets 不跟踪 |

<a id="h4-composition-rules"></a>
## H4 组合规则

[合同十层](../contracts/map3-battle01-continuous-scenario.md#ten-h4-comparison-layers)：(1) admission/provenance，(2) logical input/route，(3) world/story transitions，(4) encounter admission，(5) turn/action/RNG/consumed effects，(6) victory/after-program/return，(7) exact endpoint/ordinary input effect，(8) save exclusion/private assets，(9) presentation identity/order/consumption/completion/ack/readiness，(10) explicit deviations。

每条断言分别保留 expected source 和 actual observation。缺原版字段或 private inputs 时报告缺失侧/字段并输出 Unavailable，不得 PASS、填零或隐含排除。mismatch/已观测 required action unsupported 为 FAIL。原版Unknown（未知）使定义就绪保持 OPEN。整体 PASS 要求每个适用 assertion、variant、deviation 结果通过。Production legality 必须来自 state/content，不来自固定 actor sequence/round count/reference receipt history。既有 subsystem fixture 继续由原所有者维护。

<a id="initial-deviation-inventory-mapped-to-h4"></a>
### 初始偏差清单与 H4 映射

合同映射全部 ADR0010 deviation：受控构造（1A）、optional-route exclusion（2A）、有证据 fixed reference seed/trace 和手动 play（4A）、no save（6A）、logical remapping/accessibility（9A）、明确 out-of-domain safe behavior（10A）。所有结果在 layer 10 单独可见，含 PASS。7C private handling 是产品边界，不是 fidelity waiver；缺证据/内容不能改称 deviation。

<a id="dependency-and-completion-boundaries"></a>
## 依赖与完成边界

| 所有者 | 剩余工作 / 依赖 |
| --- | --- |
| Research | 缺失原版字段与 RA-12 effect 的接受；PR #519 仅是 source preparation，不是 native 结果；后续 #515 观测待 main 接受 |
| Design | 比较定义已独立接受；缺失 executable bindings 仍 OPEN；已接受原版修正留在同一结果中纳入 |
| Remake/content | 必需 manual action、原版音频/private provenance/actual host consumption；#517 独立拥有实现，不假定未合并结果 |
| H4 executor | 绑定已接受记录，通过既有 actual state/input/presentation 执行十层与 9A variants；保留失败/Unavailable |
| Main-gate | 独立接受定义、证据闭合与最终完整 H4；串行整合 |

已接受[胜利返回实现](../../../../remake/docs/exploration-programs.md#battle01-outcome-after-program-and-return)与 R4a 比较仅证明有界 common-session/static-spine 行为，不定义原版 expected 或本 H4 run。[能力台账](../../../../remake/docs/capability-status.md)保留其他 unsupported consumers。除非选定路线需要，不扩展到 EGRESS 或无关 item/menu branch。既有 9A 观测含 remapped/swapped input 与 authored paired flash/text；physical driver/hot-plug、完整 export 未验证。缺 continuous variants 不能继承有界 PASS。

<a id="conditional-runtime-questions"></a>
### 条件式 runtime 问题

仅命名的缺失原版语义断言可在 ADR0014/0016 与 [ADR0015](../../../decisions/0015-original-reference-replay-and-h4-boundary.md) 下支持另行准入观测。目前问题为 RA-12 nonneutral effect、未解决 selected input/RNG/state 字段、必需 8D consumption/ack。先复用 accepted static rules/bounded observations。[研究呈现审计](../../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)记录 DisplayText bypass，program return 不证明 unshimmed delivery。Persistent music 要求 reached start/replacement/stop，不造结束事件。本台账不授权 native launch，也不自动建立 per-Unknown 队列。

<a id="original-replay-lineage-and-launch-admission"></a>
### 原 replay 谱系与启动准入

旧 replay 保持 **DISABLED / NOT REQUIRED**。[谱系 hard stop](../../../research/original-reference-replay-capability.md#current-lineage-hard-stop)保留已消费 diagnostic identities、ordinal-2 completed timeout FAIL、cleanup failure、缺 actual ledger/receipts、禁止 ordinal-3/retry/reset。Recovery/transport/hardware APIs 不构成 H4 前提，synthetic reconstruction、task rename 或新 runner 不重置历史。当前 segmented-acquisition authority 由 ADR0015 管理，不由被替代的累计 ceilings 管理；本文不消费或续授 runtime 权限。

<a id="evidence-matrix"></a>
## 证据矩阵

| 既有所有者 | 继续拥有的范围 |
| --- | --- |
| [准入](../../contracts/map3-controlled-admission.md)、[研究审计](../../../research/map3-battle01-audit.md#accepted-evidence-and-exact-field-mapping) | R1/R2/R2a/R2b/R2c/R2d/R3a–d/R4a fixture 身份、精确字段与 bounded/static 区分 |
| [Battle functions](../../contracts/battle-functions-control-flow.md)、[camera](../../contracts/map-camera-update-control-flow.md) | 既有 15 个直接 battle-function associations 及独立 camera record；连续合同不新增 index association |
| [Battle lifecycle](../../contracts/battle-control-lifecycle.md)、[cutscene routing](../../contracts/battle-cutscene-routing.md) | 局部 victory/control/program 规则；自然顺序由已接受观测拥有 |
| [采集](../../../research/map3-messenger-acceptance.md) | 精确 source/ROM/tool/parent 身份、选定获胜记录、独立审阅、复现、既往失败/成本 |
| [连续合同](../contracts/map3-battle01-continuous-scenario.md) | 组合与比较语义；缺失原版值保持显式 |

Map3 aggregate inventory 不是 scenario association set，不得把 26 行全部关联。研究证据不由 remake 输出改写。M5 已退休 consumers/aggregate tests 不恢复；保留 #431 已完成 `PrivateExplorationTests.PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` failure 与独立 complete-private-world rerun，见[验证所有者](../../../../remake/docs/development-and-verification.md)。

<a id="public-and-private-boundary"></a>
## 公共与私有边界

ROM/save/trace/capture、完整 text/graphics/audio、generated exports 保持 private/ignored/local；不公开绝对路径，不要求 public CI 消费。公开报告仅含有权使用的最少语义事实、安全 provenance/results。7C PASS 前必须本地检查完整私有清单与 resource binding。文档验收仅检查 scope/link/双语语义；本 slice 不产生原版或 remake 输出。
