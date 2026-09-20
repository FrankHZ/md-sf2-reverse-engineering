# Map 3 至 Battle 01 就绪台账

- 状态：**未就绪**（针对最终连续里程碑验收）；对于另行授权的实现启动，不构成默认阻碍
- 已接受证据基线：`aabec912372c06a89265369c767399c7573154eb`；原版发现的精确溯源保留在链接的所有者与测试夹具中。
- 里程碑所有者：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)
- 工具边界：[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- 产品画像：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)
- 启动政策修正：[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md)
- 范围：对一个连续可玩场景——从准入的 Map 3 起点到 Battle 01 可观察完成——的 Layer B 就绪核算

> 本文件是 [`map3-battle01-readiness.md`](../../synthesis/map3-battle01-readiness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 当前验收范围

[ADR 0010 当前修订](../../../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)选择 **8D 玩法与呈现语义一致性**，取代旧 8C 硬件精确门槛。逐像素、原版帧节奏/动画时长、波形/芯片及 VInt/DMA/CRAM/VDP 时序不属于本里程碑验收域；改变玩法结果、事件顺序或输入就绪的时序差异仍须比较。7C 私有原版素材、9A 可访问性、10A 偏差、自然路线与 5B 端点继续有效。

下方证据矩阵保留的旧 8C 捕获、原始时序及硬件未知项是研究边界，不再是完成门槛，也不表示已闭合。旧原版回放路径停用，其账本恢复、runtime capability、scenario transport 与完整硬件 API 都不是里程碑前置条件。main-gate 在里程碑 Issue 处置该路径；仅在具体移除/替代实现有必要时拆子 Issue。旧失败与启动禁令不变。新的必要原版观察须针对明确玩法问题，先复用证据，再独立准入用途、方法及谱系/预算；不得通过更名重置。

## 判断边界

本文档是一份就绪台账。它不拥有原版游戏证据、不定义新场景合同、不选择产品体验、不授权 Phase 4，也不替换它所链接的测试夹具与合同。其目的是说明已接受的 `main` 已经能支持什么、什么仍然开放、谁必须拥有每个闭合，以及最终连续里程碑验收必须检查什么。

产品选择槽与 battle-functions 合同现已闭合。已接受运行时证据现已闭合受控 Map 3 起点、旧 fixture 局部前缀，以及独立接受的交互式采集：到达 Map19 `(26,30)` 的首次原版 Up 接受，停在下一格位移之前。已接受静态所有者闭合 R2b 至 R4a 的源码/H1/ROM 拓扑。R2d 增加了至 Battle 01 初始化及首个稳定玩家输入接缝的**已确认**显式 bridge H3 延续。对于最终连续里程碑验收，当前判断仍是 **未就绪**，因为这些所有者并未把该 Map19 边界经剩余城堡/塔路线与自然 Battle 01 准入、完整可玩战斗、已执行的战后程序及一个精确可观察结束状态连接起来。

以下区分是规范性的：

- 受控辅助或调试接缝不是自然剧情路线；
- 静态源码图不是被观察的时间顺序通关；
- fixture 局部 H4 面不是端到端场景黄金值；
- 已索引文件不自动成为未来设计关联；
- 数字 ID 或源码标签不是面向玩家的含义；
- 私有原版资源不是可分发重制资源；
- 就绪闭合不是创建 `remake/` 或开始 Phase 4 的授权。
- 本台账的 **未就绪** 不构成实现启动的默认阻碍；ADR 0016 改为要求单独、明确的用户授权，以及对具体切片依赖项的审查。

任何未合并的研究结果都不计入本台账。未来更新只可在新证据被 `main` 接受后消费它。

保留的 R2d 审阅复用已接受 #303，不是新的模拟器观察。该审阅检查了[准入研究所有者](../../../research/map3-battle01-admission.md)、[H3 fixture](../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json)、[fixture schema](../../../../schemas/h3/map3-battle01-player-ready-fixture.schema.json)及[校验器](../../../../src/sf2tool/h3/map3_battle01_player_ready.py)。所属复现命令是 `uv run sf2 h2 map3-battle01-admission` 与 `uv run sf2 h3 map3-battle01-player-ready`；本 docs-only 核对不复跑它们。fixture 显式记录 `explicit-controlled-harness-bridge`、`naturalR2bContinuity: false`，并停在 `ControlBattleEntity.after-WaitForVInt-before-input-read`。因此接受有界就绪状态，但该 fixture 不证明自然 R2a 至 R2b 连续性。下方独立观察仅闭合至首次 Map19 控制的前缀。

### 已接受的受控 R1 至首次 Map19 观察

**已确认有界原版观察：**本次核对消费 `aabec912372c06a89265369c767399c7573154eb`（tree `5fa4cdae544a7a7215cf416f29ba53901d2b7447`）上的[Research 结果与溯源](../../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result)及[审计处置](../../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed)，包括[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476#issuecomment-5747831234)。这些所有者保留固定源码/H1/ROM/工具身份、实际输入/检查点/顺序记录、私有工件身份与只读复现方式。这是独立接受的观察，没有新证据 ID，也未改变 R1/R2/R2a/R2d fixture 投影。

受控 R1 服务/scratch 恢复后，普通控制器输入到达真实 messenger 文本消费者、原版 YesNoPrompt 返回 `D0=0`、跟随者就绪等待、gate `cs_51652` 返回、原版 F604 false-to-true trap、north warp、Map19 init/`cs_53104` 返回、等待及 `0x52E8` 处原版 Up 接受。终点为 Map19 `(26,30)`、F604 true、事件字 0，无跟踪中的待处理消费者。完成帧仍在该位置；到达下一格为**未知**。本次采集没有 FieldMenu 回调。此次全部 30 个 R1 盟友状态字为 0，仅解决本次尝试的继承 POISON 问题；这不建立通用重置起点状态，也不更新旧 R1 fixture。

采集器不可变的 `OBSERVATION-COMPLETE-UNREVIEWED` 标签描述其原始输出；main-gate 独立接受了有界事实。该标签与验收都不是冻结回放、完整 8D 或 H4 PASS。post-447 等待的精确入口仍为**推断**；过时文本轮询不构成该观察。未观察的提示分支及通用后端/EOF/abort/partial-batch 行为仍为**未知**。自然 New/读档、剩余城堡/塔路线、Battle01 准入/动作、胜利及 5B 均未建立。受控历史启动总计 **3**，无剩余运行额度；保留两次已完成受控 **FAIL** 观察与停用回放谱系。本次文档审阅不执行采集、回放、准备或输入修改。

当前重制行为由 [ADR 0019](../../../decisions/0019-state-and-content-driven-remake-engine.md)与[能力台账](../../../../remake/docs/capability-status.md)单独跟踪。已实现公共会话准入、战斗结果及探索返回不建立原版自然连续性或完整 8D/H4 验收。本审阅消费上述有界审计处置，不复跑证据，也不关闭下游缺口。

## 就绪分类

每个依赖被赋予以下精确分类中的一种或多种：

| 分类 | 在本台账中的含义 |
| --- | --- |
| **合同就绪** | 已接受的实现无关合同闭合了具名本地输入、顺序、状态或输出边界。这不意味着连续场景就绪。 |
| **综合就绪** | 已接受所有者可以在不创造新证据或场景级主张的情况下支持受限 Layer B 解释。 |
| **运行时/自然闭合未知或推迟** | 已接受静态或有界运行时所有者存在，但里程碑需要的自然到达调用顺序、结果、持久性、呈现或连续行为尚未观察，或依 ADR 0014/0016 有条件推迟。 |
| **缺失设计合同** | 已接受研究存在，但尚无证据绑定设计合同拥有里程碑所需的实现无关面。 |
| **显式产品决定** | 答案是重制范围、体验、资源、可访问性、保真或偏差选择，而非可恢复的原版游戏事实。 |

一行可以在局部合同就绪，同时仍包含场景级研究或决定缺口。这是预期的：本里程碑需要组合，而不仅仅是子系统文件的存在。

## ADR 门

[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md) 接受 Godot 4.7.2 .NET、C#、CLI-first 工具链、纯 C# 确定性领域层与薄 Godot 适配器。它不安装 Godot、不选择 MCP 适配器、不选择可分发资源、不创建重制项目，也不授权实现。首个实现验收画像保持仅 CLI。

[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md) 接受恰好一个首里程碑：从 Map 3 到 Battle 01 **完成**的连续可玩场景。其最终验收要求研究与设计缺口闭合、主门禁就绪报告与独立用户启动动作。用户已于 2026-08-28 完成该历史实现启动门，见 [`remake/README.md`](../../../../remake/README.md)。战斗进入、初始化、有界实现切片或孤立机制都不能满足该里程碑。

[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md) 接受精确画像 `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`。它选择仅限私有本地的原版资源画像、禁止公开再分发，并要求玩法/呈现语义一致性。这些选择闭合产品槽，但不建立缺失研究事实，也未使场景就绪。

因此，在以下所有剩余闭合行被 `main` 接受之前，本台账对于最终连续里程碑保持 **未就绪**。在 ADR 0016 下，其状态本身并不否决另行取得用户授权的有界实现启动。

[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md) 控制这项单独的启动政策。它保留本台账的最终验收目标，同时允许用户授权的实现切片只要求其具体所需的已接受所有者。自然连续性、必要原版行为证据、必要 8D 观察、连续场景合同与 H4 完成仍是**开放**的验收工作，而不是默认的启动前阻碍。

## 精确已接受索引审计

### Map 3 聚合行

已接受研究索引包含恰好 26 条 `sourcePath` 以 `data/maps/entries/map03/` 开头的记录。全部 26 条当前未关联，且每条只携带聚合 `sf2-map-data-static-v1` 证据所有者。

| 已索引 Map 3 源码角色 | 记录数 |
| --- | ---: |
| setup 指针表 | 4 |
| entity 表 | 4 |
| entity-event 表 | 4 |
| zone-event 表 | 4 |
| area-description 表 | 2 |
| item-event 段 | 2 |
| setup init 函数 | 4 |
| script 源码容器 | 2 |
| **合计** | **26** |

该分母只证明已接受文件清单。它不建立从准入起点选择哪些行、其时间顺序执行、其自然效果、到 Battle 01 的完整路线或未来关联集。后续设计切片**不得**自动关联全部 26 条记录。它必须从专用已接受证据所有者派生其精确记录集。

### battle-functions 合同闭合

`sf2-battle-functions-static-v1` 直接绑定恰好 15 条研究索引记录。全部 15 条现在都与[战斗函数控制流](../../contracts/battle-functions-control-flow.md)关联：

| 精确记录 ID | 已接受静态面 |
| --- | --- |
| `battle.functions.pulsating-grid` | 受限共享函数清单 |
| `battle.functions.angel-wing` | Angel Wing 退出/控制路径 |
| `battle.functions.update-targets` | 目标状态更新边界 |
| `battle.functions.relative-move-table` | 相对移动表身份 |
| `battle.functions.execute-turn` | 单回合控制路线 |
| `battle.functions.load-battle` | 有序战斗加载交接 |
| `battle.functions.move-sfx` | 移动命令身份选择 |
| `battle.functions.control-cursor` | 光标/格子控制流 |
| `battle.functions.choose-target` | 目标列表导航与结果 |
| `battle.functions.set-cursor-target` | 下一实体光标目标选择 |
| `battle.functions.player-input` | 玩家动作控制状态机 |
| `battle.functions.battlefield-menu` | 战场菜单分支面 |
| `battle.functions.ai-target-visual` | AI 目标可视交接 |
| `battle.functions.equip-in-battle` | 受限战斗装备分支 |
| `battle.functions.check-gold-chest` | 受限宝箱/金币分支 |

[battle-functions 研究所有者](../../../research/battle-functions.md) 还报告一个 16 记录源码路径成员连接，因为 `map.camera-control.destination-service` 共享一个源文件。该跨所有者记录不是 `sf2-battle-functions-static-v1` 的直接绑定，仍属于[map-camera update 合同](../../contracts/map-camera-update-control-flow.md)。它不是本 battle-functions 合同的候选。

### Battle 01 路线与结果行

`battle.cutscene.data.battle01.beforebattle` 与 `battle.cutscene.data.battle01.afterbattle` 仍与[战斗过场路由](../../contracts/battle-cutscene-routing.md)关联。该合同闭合路线表、准入与静态程序语料事实；它显式保留完整 MAPSCRIPT 效果、自然可达性、持久性、可见排序与故事后果开放。

[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md) 闭合通用胜利变更顺序：治愈合格队伍、运行战后接缝、清除解锁标志、设置完成标志并返回 `D4=1`。较早 Battle 01 调试 H3 入口使用 Debug Battle Test 并跳过开战/开战前过场。下方独立 R2d 显式 bridge 观察确实观察了这些程序返回，但两种入口均不建立自然 Battle 01 准入、战后程序效果或里程碑的可观察端点。

### 已接受场景证据链刷新

已接受的场景专用所有者现形成以下精确有界链。运行时与静态标签不可互换。每行保留边界描述其自己的 fixture 或观察，并不否定独立 R2d 或受控 R1 至首次 Map19 结果：

| 阶段 | 已接受所有者 | 已闭合面 | 保留边界 |
| --- | --- | --- | --- |
| R1 | `sf2-map3-admitted-start-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-admitted-start-v1.json)；[研究所有者](../../../research/map3-admitted-start.md) | 受控 Map 3 状态至首个 `WaitForEvent` | 不是自然 New/读取路线、后续 Map 3 行为、原始时序黄金值或 8C 捕获 |
| R2 | `sf2-map3-battle01-natural-route-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json)；[研究所有者](../../../research/map3-battle01-natural-route.md) | 自然开场至 `cs_5149A` 进入函数体之前；`FieldMenu` **未到达** | messenger 函数体、后续路线、效果、Battle 01 准入与呈现仍开放 |
| R2a | `sf2-map3-messenger-acceptance-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json)；[研究所有者](../../../research/map3-messenger-acceptance.md) | 已接受 messenger 延续至 follower-ready `WaitForEvent`；`FieldMenu` **未到达** | 自然延续至静态城堡/战斗路线、后续效果与 Battle 01 仍开放 |
| 受控 R1 至首次 Map19 控制（独立观察） | [已接受结果/溯源](../../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result)；[审计处置](../../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed) | **已确认**真实 messenger/prompt/gate/F604/north-warp/init-return 链，至 Map19 `(26,30)` 的原版 Up 接受 | 不证明下一格位移、剩余城堡/塔路线、自然战斗准入、胜利轨迹、5B、冻结回放或完整 8D/H4；未改变任何 fixture 投影 |
| R2b | `sf2-map3-castle-battle-unlock-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json)；[研究所有者](../../../research/map3-castle-battle-unlock.md) | 合法源码派生路线与解锁拓扑 | 自然执行、调用方顺序、端点及 R2c 连续性为**未知** |
| R2c | `sf2-map3-battle01-admission-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json)；[研究所有者](../../../research/map3-battle01-admission.md) | 合法准入/初始化主干 | 自然准入、过场执行、初始化快照、首个行动者与玩家就绪状态为**未知** |
| R2d | `sf2-map3-battle01-player-ready-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json)；[研究所有者](../../../research/map3-battle01-admission.md) | **已确认**显式 harness bridge 从 R2a 至保留 R2b 终点，随后原版控制经过 Maps 21/40/57、准入、开战前/开战程序返回、初始化、回合生成、行动者 1 分发及首个稳定输入接缝 | 自然 R2a 至 R2b 连续性、完全自然快照/行动者选择、接缝后输入/动作/结果、呈现、胜利及完整 8C 仍为**未知** |
| R3a | `sf2-map3-battle01-turn-control-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-turn-control-static-v1.json)；[研究所有者](../../../research/map3-battle01-turn-control.md) | 回合/控制源码拓扑 | 到达的玩家/AI 分支、命令、移动、目标、动作与结果为**未知** |
| R3b | `sf2-map3-battle01-action-effect-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-action-effect-static-v1.json)；[研究所有者](../../../research/map3-battle01-action-effect.md) | 动作/效果分发器与调用方拓扑 | 实际分支选择、解决、状态、死亡、EXP、金币、掉落、后续与胜利为**未知** |
| R3c | `sf2-map3-battle01-action-completion-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-action-completion-static-v1.json)；[研究所有者](../../../research/map3-battle01-action-completion.md) | 动作完成与回放返回拓扑 | 到达的完成、回放、后续、回合后及下一回合分发为**未知** |
| R3d | `sf2-map3-battle01-turn-finalization-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json)；[研究所有者](../../../research/map3-battle01-turn-finalization.md) | 回放/收尾/重载/回合后/下一回合控制主干 | 实际结果、玩家就绪、下一回合、多回合游玩与胜利为**未知** |
| R4a | `sf2-map3-battle01-victory-return-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json)；[研究所有者](../../../research/map3-battle01-victory-return.md) | 静态 Victory 至战后程序、return/SwitchMap/Exploration 主干 | 胜利/程序到达与完成、标志/加入结果、探索重入、稳定端点、R4b 与 H4 为**未知/推迟** |

已接受聚合 `sf2-map-data-static-v1` 仍拥有 26 条 Map 3 源码行。上述场景 fixture 不会让全部 26 行变成已到达、必需或批量 Design 关联候选。

## 连续场景依赖矩阵

| 场景段 | 已接受所有者面 | 就绪分类 | 所需闭合 |
| --- | --- | --- | --- |
| 准入的 Map 3 起点 | [新游戏状态初始化](../../contracts/new-game-state-initialization.md)、[存档系统](../../contracts/save-system.md)、[故事推进](../../synthesis/story-progression.md)、[Map 3 受控准入](../../contracts/map3-controlled-admission.md)、R1 所有者 | **合同就绪 / 有界运行时**，闭合受控准入快照至首个探索等待；**已接受产品决定** 选择 1A | 不是自然 New/读取流程或后续路线；自然连续性为**未知**，8C 与原始时序黄金值保持开放 |
| Map 3 配置与内容 | [地图配置数据](../../contracts/map-setup-data.md)、[地图与探索](../../contracts/map-exploration.md)、上方 R1/R2/R2a、独立 Map19 观察与 R2b 所有者 | 所选默认配置、到达前缀及合法延续拓扑具有**合同就绪静态/有界运行时所有者**；连续场景组合仍**缺失设计合同** | 首次 Map19 Up 接受之后的自然延续、所选后续效果与路线所需完整内容保持**未知/推迟**；不批量关联 26 行 |
| 探索循环与输入 | [探索控制流](../../contracts/exploration-control-flow.md)、[输入系统](../../contracts/input-system.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、上方 R2/R2a 与独立 Map19 观察 | **合同就绪有界运行时前缀**与局部交接；现代逻辑控制/可访问性已有**已接受产品决定** | Map19 Up 接受之后的下一格位移及后续输入/结果时序仍为**未知/推迟** |
| 对话与交互 | [对话系统](../../contracts/dialogue-system.md)、[精灵对话属性数据](../../contracts/sprite-dialogue-property-data.md)、[文本与字体系统](../../contracts/text-and-font-system.md)、[立绘窗口状态](../../contracts/portrait-window-state.md)、上方 R2a 与独立 Map19 观察 | **合同就绪静态接缝与有界 messenger 结果**，另有已接受真实 messenger/gate 文本消费者及返回；私有本地原版文本已有**已接受产品决定** | post-447 等待精确入口仍为**推断**；剩余必需对话/程序与呈现消费/完成及下游连续性仍为**未知/推迟** |
| 野外菜单与 UI | [探索控制流](../../contracts/exploration-control-flow.md)、[窗口系统](../../contracts/window-system.md)、[UI 布局数据](../../contracts/ui-layout-data.md)、[UI 图形资源数据](../../contracts/ui-graphics-asset-data.md) | **合同就绪** 交接/布局/资源接缝；R2/R2a 中 `FieldMenu` **已确认未到达**；独立 Map19 采集没有 FieldMenu 回调；所需页面/呈现是**显式产品决定** | 已到达前缀不需要 FieldMenu 合同；未来自然到达路线若需要，才触发受限条件所有者 |
| 地图资源与摄像机 | [地图布局数据](../../contracts/map-layout-data.md)、[地图调色板数据](../../contracts/map-palette-data.md)、[地图瓦片集数据](../../contracts/map-tileset-data.md)、[地图精灵图形数据](../../contracts/map-sprite-graphics-data.md)、[地图实体数据](../../contracts/map-entity-data.md)、[地图摄像机更新](../../contracts/map-camera-update-control-flow.md) | **合同就绪静态导入/局部控制所有者**；私有原版资源与 8D 已有**已接受产品决定** | 到达的场景/资源/提示身份、顺序、完成/就绪与私有溯源保持**未知/推迟**；硬件精确性不在范围内 |
| 地图到战斗准入 | [探索控制流](../../contracts/exploration-control-flow.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、[战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md)、上方 R2b/R2c/R2d 所有者 | **合同就绪静态路线/准入主干及有界显式 bridge 运行时接缝**；连续场景组合仍**缺失设计合同** | 首次 Map19 控制之后经剩余 R2b 路线的连续性及自然准入/调用方状态仍为**未知/推迟**；R2d 程序返回及首个就绪状态是受控的，不是自然路线闭合 |
| Battle 01 遭遇配置 | [战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战场导航](../../contracts/battlefield-navigation.md)、上方 R2c/R2d/R3a 所有者 | **合同就绪静态遭遇/控制主干及有界初始化/玩家就绪观察** | 完全自然初始化快照与首个行动者选择、接缝后输入及后续回合状态仍为**未知/推迟** |
| 玩家回合与战斗菜单 | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、[输入系统](../../contracts/input-system.md)、上方 R2d/R3a 所有者 | **合同就绪静态分支/请求/局部输出所有者及有界行动者 1 玩家控制分发**；手动能动性与 UI 已有**已接受产品决定** | 就绪接缝后输入、AI 执行、命令、移动、目标、动作、取消与结果仍为**未知/推迟** |
| AI 与导航 | [战斗 AI 决定](../../contracts/battle-ai-decision.md)、[战场导航](../../contracts/battlefield-navigation.md)、上方 R3a/R3b 所有者 | **合同就绪算法与静态调用方拓扑** | 实际 AI 分支、命令、移动、目标、结果与多回合决定保持**未知/推迟** |
| 动作构建与解决 | [战斗动作构建](../../contracts/battle-action-construction.md)、[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)、[随机性](../../contracts/randomness.md)、上方 R3b/R3c 所有者 | **合同就绪受限算法及静态动作/效果/完成拓扑**；一个确定性 H4 轨迹已有**已接受产品决定** | 到达的种子、动作、解决/状态/死亡/EXP/金币/掉落/后续结果、回放与下一回合分发保持**未知/推迟** |
| 战斗呈现 | [战斗演出呈现](../../contracts/battle-scene-presentation.md)、专用 graphics-data 合同及上方 R3c/R3d 所有者 | **合同就绪加载器/静态资源及回放/收尾拓扑**；私有原版资源与 8D 已有**已接受产品决定** | 到达的场景/动画/音频身份、语义顺序、完成/就绪与私有溯源保持**未知/推迟**；硬件精确性不在范围内 |
| 胜利与战后 | [战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md)、上方 R3d/R4a 所有者 | **合同就绪静态胜利/战后程序/返回主干**；可观察连续完成仍**缺失设计合同** | 自然胜利、程序到达/完成、标志/加入结果、SwitchMap/探索重入与稳定端点保持**未知/推迟** |
| 存档/读取范围 | [存档系统](../../contracts/save-system.md)、[全局标志状态](../../contracts/global-flag-state.md)、名册/状态合同 | **合同就绪** 受限服务/存储接缝；里程碑排除存档/读取/检查点/挂起已有**已接受产品决定** | 强制重启回到准入快照，并让后续存档支持保持在本里程碑之外 |
| 端到端 H4 | 所有具名子系统测试夹具与合同 | 台账**综合就绪**；已接受静态链与产品层存在；原版参考/连续运行时依 ADR 0014–0016 为**未知/推迟**；场景组合与可执行定义仍**缺失设计合同** | 添加一个消费而非削弱子系统 fixture 的证据绑定连续场景合同；失败的原版参考候选保持非证据 |

## 既有综合边界

以下 Layer B 文档已能解释局部片段，但不闭合本里程碑：

- [游戏总览](../../synthesis/gameplay-overview.md) 连接顶层动作与子系统交接，同时保留战役、UI、时序与呈现缺口。
- [故事推进](../../synthesis/story-progression.md) 连接受控 Map 3 进入、静态配置/事件/脚本图、对话/名册/状态接缝与存档交接，同时显式拒绝重构正常战役路线。
- [地图设计原则](../../synthesis/map-design-principles.md) 把地图结构与路线质量、节奏、可达性与作者意图分开。
- [战术战斗循环](../../synthesis/tactical-battle-loop.md) 组合局部战斗控制器、玩家/AI、导航、动作、解决、回放与结果所有者，同时拒绝完整战斗模拟与可见时序主张。
- [推进与经济](../../synthesis/progression-and-economy.md) 连接奖励/状态变更，但不建立 Battle 01 路线、平衡或完整持久性。

这些文档是**综合就绪**的台账输入。没有一份是所需的连续场景合同。

## 已接受产品选择

[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md) 闭合产品选择槽，但不把证据所有的精确值变成产品选择。

| 决定槽 | 已接受状态 | 剩余闭合 |
| --- | --- | --- |
| 准入起点 | **已接受：1A 受控准入快照** | R1 拥有至首个等待的精确受控值/溯源；它不是规范自然 New/读取主张 |
| 路线 | **已接受：2A 最小研究证明自然路线** | 独立采集将受控起点运行时前缀延伸至首次 Map19 Up 接受；R2b/R2c 拥有静态延续，R2d 拥有至 PlayerReady 的显式 bridge。下一格位移、剩余城堡/塔连续性、效果与必需回溯仍开放 |
| 自然战斗/过场 | **已接受：3A 时间顺序，占位子条款由 7C/8D 取代** | R2c 拥有静态准入拓扑；R2d 仅在声明 bridge 后观察开战前/开战程序返回及首个就绪状态；自然准入、完全自然状态及呈现完成/就绪仍开放 |
| 完成端点 | **已接受：5B 战后程序之后首个稳定可控状态** | 精确返回地图/位置/状态仍由研究所有；仅 `D4=1` 不充分 |
| 存档/读取 | **已接受：排除 6A** | 重启回到准入快照；后续存档支持是独立里程碑 |
| 玩家控制与 UI | **已接受：4A/9A 手动能动性与现代可访问逻辑控制** | 产品 9A 已实现并直接观察；原版精确到达动作/输入轨迹及连续 H4 可访问性组合/执行仍开放 |
| 资源 | **已接受：7C 仅限私有本地原版资源** | 必须闭合忽略的私有溯源/清单；没有权利/替代品时仍阻止公开分发 |
| 视觉/音频一致性 | **已接受：8D 呈现语义** | 已接受 messenger/gate 消费者返回及首次 Map19 输入接受可复用；剩余到达场景/资源/提示身份、语义顺序/完成/就绪与 H4 定义仍开放；排除硬件精确性 |
| RNG 与动作轨迹 | **已接受：一个确定性 H4 参考轨迹** | R3a–R3d 只拥有静态控制/动作/完成/收尾拓扑；可行种子与到达逻辑轨迹仍开放，普通交互式游玩不被脚本化 |
| 有意偏差 | **已接受：10A 显式台账** | 受控准入、可选范围、现代控制、无存档、固定参考轨迹与域外引擎行为需要具名检查 |
| 可选工具 | **推迟且不阻塞；未采用 MCP** | CLI 门保持权威；任何工具选择都不启动 Phase 4 |

## 剩余验收计划

### 复用既有审计及其处置

[Research 审计](../../../research/map3-battle01-audit.md#research-gap-register)已经拥有 RA-01–RA-12；本台账已经拥有 Design 就绪核算。两份审计都已存在。只有 Research 可在观察被接受后更新其证据登记；Design 更新这里的相关行及未来连续合同。完成本次规划不关闭任何一份审计。

| 既有所有者或缺口 | 可复用的已接受面 | 剩余验收处置 |
| --- | --- | --- |
| RA-01 / 受控起点 | R1 精确受控起点/合同；独立采集读取全部 30 个状态字为 0 | 保留有界 PASS 及仅适用于此次尝试的无 POISON 结果；不要求 1A 已排除的可见 New/读取流程。所有与路线相关的初始字段仍须具备声明的溯源。 |
| RA-02–RA-05 / 路线、准入、状态 | 受控起点至首次 Map19 Up 接受的前缀、R2b/R2c 静态链及 R2d 受控 PlayerReady | 保留各局部闭合边界。下一未观察段从 Map19 下一格位移开始，经剩余城堡/塔路线至 Map21 及自然准入；仍需路线携带的调用方/状态事实。 |
| RA-06 / 完整战斗 | R3a–R3d 静态所有者与既有局部战斗合同 | 完整原版轨迹、可行固定种子及到达的动作/结果仍开放。重制胜利不关闭本行。 |
| RA-07 / RA-12 / 返回与端点 | R4a 静态主干；下文的重制比较 | Research 必须建立原版战后程序效果及首个稳定可控的 5B 端点。Design 随后绑定精确字段、输入就绪以及无待处理程序/模态/传送/战斗。 |
| RA-08 / RA-09 / 菜单与对话 | R2/R2a 的 `FieldMenu` NotReached；独立采集没有 FieldMenu 回调，且有真实 messenger/gate 文本返回 | 复用这些有界结果；post-447 等待精确入口仍为推断。只闭合剩余路线必需覆盖。不建立全面野外菜单或可选对话任务；正文/捕获保持私有。 |
| RA-10 / 持久性 | 已接受的 6A 排除 | 本里程碑不需要存档实现或持久性观察。H4 必须检查重启回到准入状态，以及不存在用户存档/恢复界面。 |
| RA-11 / 原版呈现 | 静态呈现/资源所有者及已接受 messenger/gate 消费者与 Map19 控制 | 复用已观察返回/就绪，不将其当作全部视觉/音频完成。剩余必要 8D 身份/顺序、完成、就绪及溯源仍开放。素材可用本身不证明呈现消费。旧硬件缺口不是里程碑要求。 |
| Design 合同与产品选择 | battle-functions 合同与 ADR 0010 画像 | 保留这些闭合。缺失的连续合同及 H4 定义/执行仍是独立门禁，不因此重开已闭合子系统合同。 |

**已确认重制范围：**已接受的结果实现 [`8a581a82`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/8a581a82e297ea2947cc9837e661752163d2806d) 与[探索执行所有者](../../../../remake/docs/exploration-programs.md#battle01-outcome-after-program-and-return)提供连续公共会话，经过胜利或普通失败后返回可操作野外。已接受的 R4a 比较 [`26e91107`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/26e91107160b2c6f088b01ad31704ce9282fcba6)消费未改变的 R4a fixture，比较程序/加入/标志/返回顺序。它读取静态源码预期，不是自然原版观察。初始 accounting/seeds 与普通失败 Map3 egress 仍是受控输入。失败覆盖是可复用实现覆盖，不是 4A/5B 额外要求的原版参考胜利轨迹。

**未知原版保真 / 显式实现限制：**[能力所有者](../../../../remake/docs/capability-status.md#milestone-and-deferred-surfaces)保留自然 caller/accounting/seed/egress、完整呈现及 H4 缺口。EGRESS 及更广动作、状态、物品/配置事件等未支持消费者，只有被接受路线/动作集需要时才成为工作项。现代 white fades、mosaic、shiver 及项目自有提示音是已实现的呈现服务；其存在本身不建立原版提示身份、语义顺序、完成或 7C 合规。应比较这些 8D 边界。原版像素/帧/音频波形差异不在选定域内，不要求精确后端或硬件捕获；不能把影响玩法的差异或私有原版内容要求当成视觉润色排除。

**独立 7C 音频缺口：**[Research 呈现审计](../../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)及[重制呈现所有者](../../../../remake/docs/exploration-programs.md)说明 `MUSIC_JOIN`/`MUSIC_SAD_JOIN` 是项目自编和弦循环。原版命令 ID 与实际 `AudioStreamPlayer` 启动/淡入淡出/停止不建立私有原版素材身份/溯源及其到已消费提示的绑定。已接受 Map19 采集与 8D 语义比较不关闭该内容缺口；9A/10A 也不豁免它。

**已确认产品实现及直接观察：**已接受的 [9A 实现](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/445)提供跨探索、对话/选择、战斗及返回共用的可配置键盘/手柄动作、确认/取消约定交换、减少白色闪光与即时/可调文本。[能力所有者](../../../../remake/docs/capability-status.md)及[原生 9A 观察所有者](../../../../remake/docs/development-and-verification.md#native-9a-observation)定义已接受边界与复现方式。启动设置暴露这些选项；没有游戏内设置界面或自动保存设置。减少闪光模式抑制白色覆盖层，同时保留服务时长及相同提示 token/kind 的完成。文本显示保留真实确认与选择等待。

已接受的直接观察覆盖默认及重映射/交换的键盘/手柄、两个自编世界/角色、成对闪光/文本行为，以及使用重映射/交换手柄的私有连续开场至 Battle01 胜利再至可操作返回会话。私有案例使用即时文本与普通闪光；成对可访问性比较由自编案例负责。这些是注入的真实 Godot 输入事件，不是物理手柄驱动或热插拔验收。完整导出打包仍未验证。本次复用既有结果，不重新运行。产品 9A 已实现并直接观察；组合到连续 H4 定义及其执行仍开放。9A/10A 的有意偏差不证明原版自然连续性或完整 8D 验收。

已接受 M5 [`9301ddac`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/9301ddac0e07fd2c5a87caeb19c69179025d0698)退役旧 reference 消费者并保留适用移动比较。保留 #431 已完成的呈现输入失败 `PrivateExplorationTests.PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup`，以及使用完整私有世界单独复验的结果（1 通过、0 跳过）。两者不互相覆盖，也都不证明 8D。本计划不恢复旧 runtime 或聚合套件。独立 #434 的 map-exploration 镜像标签失败（英文 13 / 中文 11）不在本台账修正范围内。

### 依赖与完成边界

这是验收依赖图，不是派工队列。每个后续 Issue 都须有具名执行者、精确路径，并依[Project 生命周期](../../../operations/github-project-governance.md#task-lifecycle)获得 main-gate 独立准入。准备工作可消费已接受静态事实；缺失运行时值保持未填。

| 面与负责所有者 | 入口/依赖 | 完成与停止条件 |
| --- | --- | --- |
| 旧回放路径停用 — main-gate | 保留 capability/谱系所有者；不启动 | 里程碑 Issue 负责处置。恢复、runtime 验证与 transport 实现不阻塞 8D/H4；仅在需要具体移除/替代实现时拆工作项，预算不重置。 |
| 原版行为证据 — Research | 已接受静态事实与受控起点至首次 Map19 控制前缀；具体未决玩法主张 | 通过独立准入的证据方法建立下游自然路线/caller/accounting/RNG、战斗结果、战后程序与 5B 状态。新观察前明确字段/检查点及既有证据不足原因。保留未知项与类型化失败，不依赖停用 runner。 |
| 连续场景合同 — Design | 每个主张连续边界所需的已接受 Research 值 | 提议的 `docs/design/contracts/map3-battle01-continuous-scenario.md` 组合准入起点、自然路线/准入、胜利逻辑轨迹、战后程序与精确端点。现在可准备结构；最终证据绑定验收须等待必要观察。关联从证据派生，不能自动取全部 26 条 Map3 聚合行。 |
| H4 定义 — Design | 连续合同、已接受 Research 比较域/溯源及 ADR 0010 偏差 | 指定各层输入、观察、预期值/所有者、精确性/容差、失败/不可用及清理规则。未来 `schemas/h4/` 与 `tests/fixtures/h4/` 身份/注册由该切片选择，不在此虚构。可与合同并行准备定义，但不能声称缺失证据。 |
| 9A 输入/可访问性 — 已接受 Remake 能力；Design/H4 组合仍开放 | 上述已接受实现与直接观察；独立于原版 replay 恢复 | 复用已实现设置及有界输入/状态/投影结果。将可访问性断言组合到未来连续 H4 定义并执行；单独报告可访问性变体及状态等价结果。物理手柄驱动/热插拔及导出限制保留。 |
| H4 实现与执行 — Remake/harness | 定义已被 main 接受；必要引擎行为已支持且私有输入可用 | 消费冻结的已接受证据，不启动原版来生成黄金值。运行实际状态/输入/投影与必要私有比较，报告所有适用层及偏差，包括失败/不可用。Godot 验收不截图。 |
| 最终里程碑审阅 — main-gate | Research 与 Design 闭合，且适用 H4 执行成功 | 独立验收连续可玩的 5B 端点及当前 8D 画像。单凭合同、preflight、replay、引擎测试或子 Issue 关闭都不充分。 |

[Capability 所有者](../../../research/original-reference-replay-capability.md)提供固定的 33 个物理行传输 recipe，不是任意场景运行器。[Scenario API](../../../research/original-reference-replay-scenario-api.md)提供纯数据 descriptor 及被动观察者协议；其通用样例与合成工件身份不是真实 movie 或场景执行。两者均在 [`f00c20f3`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/f00c20f317b3e1c5c63a858df5e5b5abcb4a309d) 被接受。新证据方法若需要 transport，先评估可复用的隔离、收据与观察者政策；更换 CLI 入口不会完成缺失 transport，也不会授予新启动谱系。

### 原版证据方法与已耗尽准入

[ADR 0015 的方法区分](../../../decisions/0015-original-reference-replay-and-h4-boundary.md#distinguish-interactive-acquisition-frozen-replay-and-remake-h4)控制此处全部运行时问题：

- **交互式采集：**Research 的操作者/host 可依据原版观察选择明确的普通控制器输入；Lua 仅传递输入并观察，不选择路线或实现机制。保留实际交付帧，包括中立/bootstrap 帧，并区分回调事实与暂停的帧末状态。受控 R1 初始化及最终恢复是声明的干预；R1 服务/scratch 恢复至清理之间不注入状态。
- **冻结原版回放：**不可变非自适应输入与被动观察者规则适用于此方法。一份输入记录不是回放 PASS。冻结/回放采集须有独立确定性需求并单独准入，不是自动下一步。
- **重制 H4：**通过已接受比较定义消费独立接受的 Research 证据，不能在比较中生成原版真值。依据实时状态选动作的重制观察者提供实现证据，不是原版证据。

唯一追加交互式采集已完成：受控历史启动总计 **3**，包括保留的 #460/#465 **FAIL** 结果。没有授权第四次启动、重试、回放、准备或预算重置。下方停用回放 ordinal-1/2 谱系独立且不变；成功的有界采集没有修复其缺失真实账本/收据及 ordinal-2 超时/清理失败。任何后续运行时问题都需要新的明确用途/方法与谱系/预算决定；问题表不授予启动额度。

### 原版 replay 谱系与启动准入

Capability 所有者把已消耗 diagnostic ordinal 1 固定为候选 `9F8417BC1A515FEB5D9466DCC1BC489B981D97741E44518D572E6B0E63380BDF` 与收据 `BDE38876750E51E59CF1D2897495EFFD8EE42955F7FE87C3F12A9DB853C14CA6`。[当前谱系硬阻断](../../../research/original-reference-replay-capability.md#current-lineage-hard-stop)还记录了已恢复的 Research 与独立 main-gate 工具输出：报告 ordinal-2 进程启动、已完成的超时 FAIL 和清理失败，随后明确禁止 ordinal 3/retry/reset。这些保存的输出是协调记录，不是已恢复的收据/账本字节或原版游戏证据。有界复核没有发现后续真实 ordinal-3 启动；这既不能确立完整历史，也不能证明还有未使用的诊断次数。

任何独立授权的恢复必须找到完整真实账本及两份实际收据，用所属校验器核对既有身份与关系，并保留全部失败。合成 scratch、重建 JSON、空账本、保存的输出或新输出根都不能替代真实字节。旧实体工作区已不存在，实际字节仍不可用。本计划没有新做私有搜索或运行时验证。停用路径只有在完整恢复及 main-gate 独立裁定谱系/预算之后才可能恢复；文件缺失或恢复本身都不会重置消耗，也不能覆盖先前禁令。

已记录候选 preflight `B003732B61A375C7980BDC1F328E5C8B00553E6F38E669746041E9E6BC7BE0EA` 返回 PASS、`ProcessStarts=0`。这是保留的 preflight 结果，不是启动收据或 runtime 兼容性证明，本 docs-only 修正也不复跑它。不得假定任何 ordinal 可用。Ordinal 3 仍要求单一同候选 ordinal-2 PASS、实际收据 hash 校验及匹配 replay digest；恢复一份报告为 ordinal-2 FAIL 的收据不能满足该条件。没有授权额外诊断、第四次启动或通过任务/wrapper/scenario/transport 重置。真正独立的场景切片仍须按 ADR 0015 明确裁定谱系/预算，不能假定重新得到额度。

### 有条件运行时问题

以下是具体验收缺口的准入材料，不是运行授权。ADR 0014/0016 要求在创建任何新 H3 fixture 前证明调用方依赖、实质合同影响，以及既有批量轨道无法覆盖。优先扩展已接受轨道；不按 NPC 或每个 Unknown 建任务。此处适用上方 ADR 0015 方法区分：只有冻结回放要求非自适应输入轨迹；交互式采集允许明确、基于观察的控制器选择。两者均保留私有输出、类型化回调/退出/清理失败及各自独立准入上限。无剩余额度。

| 问题与验收影响 | 要复用的既有静态证据/轨道 | 独立准入与停止边界 |
| --- | --- | --- |
| 新行为观察是否必要且可准入？ | 先复用已接受事实/rails；声明缺失玩法断言及必要检查点。停用 transport 不是前置。 | 启动前独立作出 ADR 0014/0015/0016 方法及谱系/预算决定；保留旧禁令，并停在具名类型化失败或端点。 |
| 在已接受首次 Map19 Up 输入后，下一格位移及剩余城堡/塔路线能否自然把真实 caller/accounting/RNG 状态带入 Battle01，并沿选定胜利路线完成至可控 5B 端点？这决定 2A/3A/4A/5B 与 RA-02–RA-07/RA-09/RA-12。 | 已接受 Map19 终点与 R1/R2/R2a/R2d 观察字段；R2b 剩余 royal/tower 程序及 F605/F607/F608/F401/F256 调用方，随后 R2c/R3a–R3d/R4a 静态 fixture。R2d bridge 无法证明剩余路线上的自然状态；R4a 在探索执行前停止。 | Research 先命名未决调用方/配置/程序返回字段，并说明 Map19 观察加静态拓扑为何不足。优先既有批量轨道；新 fixture 须有轨道复用结论。依 ADR 0015 明确选择采集或冻结回放并单独准入；保留受控起点溯源，禁止实时状态注入。首个类型化失败、准入上限或声明端点即停止；当前无执行授权。 |
| 8D 需要哪些到达呈现提示、顺序、完成及输入就绪状态？这决定 RA-11 与 H4 第 9 层。 | 复用已接受 messenger/gate 真实文本返回及 Map19 输入接受，以及对话/窗口、图形/音频/资源/程序所有者。post-447 等待精确入口仍为推断；仅 ID/返回不能建立后续提示及实际 host 消费。 | 命名已到达提示/资源、调用方、分发/消费者完成或确认、关联状态效果及输入阻塞/可用关系。持续音乐需要到达处的启动/替换/停止语义，不是虚构结束事件。先复用证据；只有实质未决断言才能支持单独准入观察。硬件 API/时钟不是前置；7C 素材溯源仍独立。 |

只有独立接受于 main 的 Research 证据能闭合其明确观察到的有界主张。私有捕获载荷与详细收据保持忽略。完整原版参考播放仍不是 H4 PASS；重制一致性须经过上方独立接受的定义与执行路径。

## H4 组合规则

未来连续适配器应报告独立可观察层，而不是一个通过/失败整体：

1. 准入起始状态身份与溯源；
2. 有序输入与探索交接；
3. 地图、配置、事件、程序、对话、名册与标志过渡；
4. Battle 01 准入与已初始化遭遇状态；
5. 有序回合、移动、目标、玩家动作、AI 动作、RNG、解决、回放与回合后轨迹；
6. 控制器胜利状态与战后程序/交接轨迹；
7. 产品所选可观察端点处的精确最终场景状态；
8. 所选存档排除与 7C 私有本地资源身份/溯源断言；
9. 8D 到达的场景/对白/动画/音频身份与语义顺序、实际 host 消费、完成/确认和输入就绪，以及已接受参考溯源和许可安全公开报告；不含硬件精确断言；
10. 单独命名的预期偏差。

每层必须引用其已接受比较定义及 Research 证据/溯源；有界采集结果本身不提供 H4 fixture。连续适配器不得替换子系统测试夹具、把其预期数字复制进引擎特定测试、在重制中要求原版 RAM/ROM 地址，或发布私有原版文本、图形、音频或捕获。

定义就绪要求完整验收面及可执行检查定义被接受。里程碑验收还要求它们在重制上成功执行。定义就绪不等于比较通过；已实现连续路线也不能替代缺失原版参考或任何必要 8D 层。

### 初始偏差清单到 H4 的映射

这是 ADR 0010 第 10 节既有清单的验收映射，不是新的偏差台账。层号引用本文上方十层列表；ADR 的分组方式不同。每个结果即使通过也须在第 10 层可见。比较拥有 fixture 域规则；固定回合、行动者序列或收据数量不能成为生产玩法合法性。

| 已接受偏差与所有者 | 受影响 H4 层 | 必需预期结果 |
| --- | --- | --- |
| 受控快照替代可见 New/读取 — 1A | 1, 2, 10 | 精确准入快照/溯源匹配；报告省略可见流程。不描述为自然 New/读取。 |
| 排除可选 Map3 交互/菜单 — 2A | 2, 3, 10 | 枚举必需到达路线步骤；显示被排除可选范围。不能借此删除必需交互。 |
| 现代可重映射输入与可访问性 — 9A | 2, 3, 5, 7, 9, 10 | 键盘/手柄产生相同逻辑决定；交换确认/取消仍保留其语义角色。减少闪光模式在不显示被抑制提示的情况下达到相同完成；调整/即时文本保留确认及路线结果。暴露绑定/约定/闪光/文本设置。单独报告这些变体与状态等价检查；设备节奏与改变后的视觉时序不是精确匹配断言。 |
| 无用户存档/读取/检查点/挂起 — 6A | 1, 8, 10 | 不存在用户持久性界面；重启返回准入状态。Harness reset 不是存档功能。 |
| 固定种子/逻辑 H4 轨迹；交互式游玩可分歧 — 4A | 1, 2, 5, 7, 10 | 参考执行声明 Research 证明的种子与不可变逻辑轨迹；普通控制仍可用，有效变化状态/内容遵守引擎规则。绝不为强求一致而在 live play 重置种子。 |
| 准入 fixture 域外的引擎原生安全行为 — 10A | 受影响状态/动作层、10 | 明确命名每个域外用例及安全/Unsupported 结果。不从拒绝推断原版行为，不用此偏差排除域内失败。 |

7C 私有资源与禁止再分发是第 8 层产品/溯源边界，不是额外保真偏差。私有输入缺失产生 Unavailable。其他现代化需要独立接受的决定，不能仅因当前适配器如此运行就加入本表。

## 公开与私有边界

公开就绪工件可以保留其所有者已允许的记录 ID、fixture ID、合同链接、计数、聚合元数据、已接受 hash、状态字段名、分支/顺序摘要、产品选择槽与合成 H4 轨迹形状。

以下保持私有，除非独立许可与分发审查接受：

- ROM、SRAM、存档状态、含版权载荷的轨迹与模拟器捕获；
- 完整提取的地图、对话、图形、音乐、声音、字体或过场内容；
- 原始源码派生资源载荷与私有规范导入图；
- 任何溯源或分发条款未被接受的替代资源。

Phase 4 的受追踪实现与 CI 应消费公开合同与项目自有 fixture。已选择的私有本地 7C 画像可在其溯源/清单被接受后，于本地加载忽略的原版资源与捕获，但这些输入不得成为受追踪依赖、上传内容、公开 CI 要求或可分发构建内容。

## 就绪检查清单

| 门 | 当前结果 | 闭合所有者 |
| --- | --- | --- |
| 精确里程碑与引擎基线已接受 | PASS | ADR 0008 / ADR 0009 |
| 产品验收画像已选择 | PASS | ADR 0010 |
| 受控准入 Map 3 起始状态精确 | PASS | `sf2-map3-admitted-start-runtime-v1` 与 [Map 3 受控准入](../../contracts/map3-controlled-admission.md)；不是自然 New/读取主张 |
| 受控 R1 至首次原版 Map19 Up 接受 | PASS（有界） | [独立接受的采集](../../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result)；停在下一格位移之前，未更新旧 fixture |
| 首次 Map19 控制之后的自然路线连续性 | 开放 | Research，然后场景合同；剩余城堡/塔路线至 Map21 与自然战斗准入 |
| 所需探索/对话/菜单/UI 范围精确 | 开放 | 研究加路线所需条件合同；ADR 0010 固定最小范围规则 |
| 静态 Battle 01 准入主干已接受 | PASS | `sf2-map3-battle01-admission-static-v1`；自然准入由下方独立开放行表示 |
| 显式 bridge Battle 01 初始化与 PlayerReady 接缝 | PASS（有界） | `sf2-map3-battle01-player-ready-runtime-v1`；遗漏的自然 R2a 至 R2b 连续性仍开放 |
| 自然 Battle 01 准入精确 | 开放 | 条件运行时证据，然后场景合同；R2d 不闭合剩余自然 Map19 至准入连续性 |
| 玩家回合合同在场 | PASS | [战斗函数控制流](../../contracts/battle-functions-control-flow.md) |
| R3a–R3d 静态控制/动作/完成/收尾链已接受 | PASS | 已接受静态 fixture；到达分支/结果仍开放 |
| 完整可玩 Battle 01 轨迹精确 | 开放 | 条件运行时证据加既有/扩展战斗合同 |
| R4a 静态胜利/战后程序/返回主干已接受 | PASS | `sf2-map3-battle01-victory-return-static-v1` |
| 战后程序到达、完成与效果精确 | 开放 | 条件运行时证据，然后场景合同 |
| 可观察端点形状已选择 | PASS | ADR 0010 选项 5B |
| 精确端点状态已有证据 | 开放 | 条件运行时证据，然后场景合同 |
| 存档范围已选择 | PASS | ADR 0010 选项 6A 排除存档/读取/检查点/挂起 |
| 可访问性/输入产品接口已选择 | PASS | ADR 0010 选项 9A |
| 9A 输入/可访问性配置已实现并直接观察 | PASS（有界） | 上述已接受 Remake 能力及原生 9A 观察所有者；物理手柄驱动/热插拔与完整导出未验证 |
| 9A 可访问性断言已组合到连续 H4 并执行 | 开放 | Design H4 定义，然后 Remake/harness 执行；单独报告变体结果与状态等价检查 |
| 7C 私有本地资源模式与禁止公开分发边界已选择 | PASS | ADR 0010 |
| 精确私有资源/捕获清单与溯源已接受 | 开放 | 研究/私有输入验收；无载荷进入 Git/公开 CI |
| 公开/可分发资源权利或替代品 | 私有里程碑之外受阻 | 任何公开构建前的独立许可/替代决定 |
| 8D 玩法/呈现语义档位已选择 | PASS | ADR 0010；取代旧 8C 硬件门槛 |
| 必要的到达 8D 证据及比较字段已接受 | 开放 | Research/溯源，然后连续 H4 合同；不要求像素/波形/周期捕获 |
| 连续 H4 验收面与可执行检查定义已接受 | 开放 | 连续合同之后的 Design H4 定义 |
| 旧 replay 谱系/runtime 恢复 | 不要求 / 路径停用 | main-gate 处置；保留失败、缺失账本/收据与启动禁令 |
| 必要自然行为参考证据已接受 | 首次 Map19 控制之后开放 | Research；复用有界采集与已接受事实；无剩余运行额度 |
| 连续场景合同已接受 | 开放 | Design，消费已接受 Research |
| 所有适用重制 H4 层与偏差执行成功 | 开放 | Remake/harness 后由 main-gate 独立验收；与定义就绪分开 |
| 主门禁就绪报告已接受 | 开放 | 主门禁 |
| 独立用户 Phase 4 启动动作 | PASS | 用户授权已记录于 [`remake/README.md`](../../../../remake/README.md)；仅满足实现启动，不代表里程碑就绪 |

只要有任何所需行开放，台账对于最终连续里程碑验收就保持 **未就绪**。默认情况下，这些行不阻碍另行取得用户授权的具体实现切片。

## 证据矩阵

fixture 行保留各自限制；独立 Map19 观察不更新其投影。

| 台账陈述 | 分类 | 已接受所有者 | 保留边界 |
| --- | --- | --- | --- |
| 受控 Map 3 起点以精确有界状态到达首个探索等待 | **合同就绪 / 受限运行时** | `sf2-map3-admitted-start-runtime-v1`、[Map 3 受控准入](../../contracts/map3-controlled-admission.md) | 不是自然玩家可见 New/读取流程或后续 Map 3 路线 |
| 26 条 Map 3 源码路径记录存在且聚合拥有 | **已确认索引清单** | `sf2-map-data-static-v1`、[map-data 研究](../../../research/map-data-inventory.md) | 不是路线时间线、可达性、效果或自动未来关联 |
| 已观察自然开场与 messenger 接受前缀 | **合同就绪有界运行时前缀** | `sf2-map3-battle01-natural-route-runtime-v1`、`sf2-map3-messenger-acceptance-runtime-v1` | 终止于程序入口/跟随者就绪边界；后续连续性未证明，`FieldMenu` 未到达 |
| 受控 R1 经真实 messenger/gate/F604/north-warp/init 返回至首次 Map19 Up 接受 | **已确认有界原版观察** | [Research 结果/溯源](../../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result)与[审计处置](../../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed) | Map19 `(26,30)`，停在下一格位移之前；post-447 等待精确入口为推断；下游连续性、冻结回放及完整 8D/H4 未证明；旧 fixture 不变 |
| R2b/R2c 合法路线、解锁、准入与初始化拓扑存在 | **合同就绪静态链** | `sf2-map3-castle-battle-unlock-static-v1`、`sf2-map3-battle01-admission-static-v1` | 不是自然执行、调用方顺序、过场执行、初始化快照或首个行动者 |
| 显式 bridge 延续到首个 Battle 01 PlayerReady | **已确认有界运行时** | `sf2-map3-battle01-player-ready-runtime-v1`、[准入研究](../../../research/map3-battle01-admission.md) | 自然 R2a 至 R2b 连续性、完全自然快照/行动者、接缝后游玩、胜利、呈现及完整 8C 仍未知 |
| R3a–R3d 回合、动作/效果、完成、回放与收尾拓扑存在 | **合同就绪静态链** | 四个已接受 R3 静态 fixture 及其链接研究所有者 | 不是到达的玩家/AI/动作/结果、回放、下一回合、多回合游玩或胜利 |
| 15 条 battle-functions 记录有已接受静态证据与一个受限设计合同 | **合同就绪** | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、`sf2-battle-functions-static-v1` | 无摄像机所有者重叠，无运行时/输入/呈现泛化 |
| R4a 胜利、战后程序、返回、SwitchMap 与探索调用主干存在 | **合同就绪静态链** | `sf2-map3-battle01-victory-return-static-v1`、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | 胜利/程序到达与完成、标志/加入结果、探索重入、稳定端点、R4b 与 H4 保持开放 |
| 局部战斗合同可在概念上组合 | **综合就绪** | [战术战斗循环](../../synthesis/tactical-battle-loop.md) 与链接合同 | 不是完整预测 Battle 01 模拟或场景黄金值 |
| Godot/C#、里程碑、产品画像与推迟政策已选择 | **已接受决定** | ADR 0008 / ADR 0009 / ADR 0010 / ADR 0016 | 有界实现授权不意味着连续里程碑、MCP、再分发或证据闭合 |
| 路线类别、端点形状、存档排除、UI、私有资源、RNG 策略、8D 一致性与偏差 | **已接受产品决定** | ADR 0010 | 精确场景值、自然时间顺序、私有捕获溯源与一致性事实仍是研究/H4 缺口 |
