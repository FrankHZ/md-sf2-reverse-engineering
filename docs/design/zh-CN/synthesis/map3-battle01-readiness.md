# Map 3 至 Battle 01 就绪台账

- 状态：**未就绪**（针对最终连续里程碑验收）；对于另行授权的实现启动，不构成默认阻碍
- 初始缺口审计日期：2026-08-14
- 初始缺口审计基线：commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`，tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- 产品决定日期：2026-08-19
- 已接受状态刷新：2026-08-20，commit `9a7cbcb44322e309ef10d8afac76d9a98be76f98`，
  tree `28c5f9c00a2b095d8b990eb8adc5249ede911704`
- 静态所有者刷新：2026-08-30，commit `1647ea15c3fabd900d451d5e2bc9c52699137a62`，
  tree `dddc48d1c0e1d87016b35d9d8f79bf40c1ceef3f`
- 里程碑所有者：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)
- 工具边界：[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- 产品画像：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)
- 启动政策修正：[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md)
- 范围：对一个连续可玩场景——从准入的 Map 3 起点到 Battle 01 可观察完成——的 Layer B 就绪核算

> 本文件是 [`map3-battle01-readiness.md`](../../synthesis/map3-battle01-readiness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本文档是一份就绪台账。它不拥有原版游戏证据、不定义新场景合同、不选择产品体验、不授权 Phase 4，也不替换它所链接的测试夹具与合同。其目的是说明已接受的 `main` 已经能支持什么、什么仍然开放、谁必须拥有每个闭合，以及最终连续里程碑验收必须检查什么。

产品选择槽与 battle-functions 合同现已闭合。已接受运行时证据现已闭合受控 Map 3 起点与两个有界自然路线前缀，已接受静态所有者则闭合 R2b 至 R4a 的源码/H1/ROM 拓扑。对于最终连续里程碑验收，当前判断仍是 **未就绪**，因为这些所有者并未把最后一个运行时前缀与自然 Battle 01 准入、完整可玩战斗、已执行的战后程序及一个精确可观察结束状态连接起来。

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

2026-08-20 刷新记录已集成的 battle-functions 合同与 ADR 0010。2026-08-30 刷新只记录后续已接受的 R1/R2/R2a 运行时所有者及 R2b 至 R4a 静态链，并绑定上方具名 accepted-main 基线。两次刷新都不是对完整初始缺口审计的重新执行，也不提升任何未合并研究、忽略/私有输入、失败回放或工具结论。

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

[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md) 接受精确画像 `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8C + 9A + 10A`。它选择仅限私有本地的原版资源画像、禁止公开再分发，并要求帧/音频/硬件精确一致性。这些选择闭合产品槽，但扩大了研究、私有溯源与 H4 工作；它们并未使场景就绪。

因此，在以下所有剩余闭合行被 `main` 接受之前，本台账对于最终连续里程碑保持 **未就绪**。在 ADR 0016 下，其状态本身并不否决另行取得用户授权的有界实现启动。

[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md) 控制这项单独的启动政策。它保留本台账的最终验收目标，同时允许用户授权的实现切片只要求其具体所需的已接受所有者。自然连续性、原版参考回放、完整 8C 捕获、连续场景合同与 H4 完成仍是**开放**的验收工作，而不是默认的启动前阻碍。

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

[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md) 闭合通用胜利变更顺序：治愈合格队伍、运行战后接缝、清除解锁标志、设置完成标志并返回 `D4=1`。已接受 Battle 01 H3 入口使用 Debug Battle Test 并跳过开战/开战前过场。这两个事实都不建立自然 Battle 01 准入、战后程序效果或里程碑的可观察端点。

### 已接受场景证据链刷新

已接受的场景专用所有者现形成以下精确有界链。运行时与静态标签不可互换：

| 阶段 | 已接受所有者 | 已闭合面 | 保留边界 |
| --- | --- | --- | --- |
| R1 | `sf2-map3-admitted-start-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-admitted-start-v1.json)；[研究所有者](../../../research/map3-admitted-start.md) | 受控 Map 3 状态至首个 `WaitForEvent` | 不是自然 New/读取路线、后续 Map 3 行为、原始时序黄金值或 8C 捕获 |
| R2 | `sf2-map3-battle01-natural-route-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json)；[研究所有者](../../../research/map3-battle01-natural-route.md) | 自然开场至 `cs_5149A` 进入函数体之前；`FieldMenu` **未到达** | messenger 函数体、后续路线、效果、Battle 01 准入与呈现仍开放 |
| R2a | `sf2-map3-messenger-acceptance-runtime-v1`；[fixture](../../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json)；[研究所有者](../../../research/map3-messenger-acceptance.md) | 已接受 messenger 延续至 follower-ready `WaitForEvent`；`FieldMenu` **未到达** | 自然延续至静态城堡/战斗路线、后续效果与 Battle 01 仍开放 |
| R2b | `sf2-map3-castle-battle-unlock-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json)；[研究所有者](../../../research/map3-castle-battle-unlock.md) | 合法源码派生路线与解锁拓扑 | 自然执行、调用方顺序、端点及 R2c 连续性为**未知** |
| R2c | `sf2-map3-battle01-admission-static-v1`；[fixture](../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json)；[研究所有者](../../../research/map3-battle01-admission.md) | 合法准入/初始化主干 | 自然准入、过场执行、初始化快照、首个行动者与玩家就绪状态为**未知** |
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
| Map 3 配置与内容 | [地图配置数据](../../contracts/map-setup-data.md)、[地图与探索](../../contracts/map-exploration.md)、上方 R1/R2/R2a 与 R2b 所有者 | 所选默认配置、到达前缀及合法延续拓扑具有**合同就绪静态/有界运行时所有者**；连续场景组合仍**缺失设计合同** | 自然 R2a 至 R2b 执行、所选后续效果与路线所需完整内容保持**未知/推迟**；不批量关联 26 行 |
| 探索循环与输入 | [探索控制流](../../contracts/exploration-control-flow.md)、[输入系统](../../contracts/input-system.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、R1/R2/R2a 所有者 | **合同就绪局部规则与有界运行时前缀**；现代逻辑控制/可访问性已有**已接受产品决定** | 后续自然输入、结果、重复/同时性/节奏与完整路线保持**未知/推迟** |
| 对话与交互 | [对话系统](../../contracts/dialogue-system.md)、[精灵对话属性数据](../../contracts/sprite-dialogue-property-data.md)、[文本与字体系统](../../contracts/text-and-font-system.md)、[立绘窗口状态](../../contracts/portrait-window-state.md)、R2/R2a 所有者 | **合同就绪命令/存储/窗口接缝**与两个到达程序边界；私有本地原版文本已有**已接受产品决定** | 程序函数体内容、文本/效果、后续交互、可见时序与完整路线保持**未知/推迟** |
| 野外菜单与 UI | [探索控制流](../../contracts/exploration-control-flow.md)、[窗口系统](../../contracts/window-system.md)、[UI 布局数据](../../contracts/ui-layout-data.md)、[UI 图形资源数据](../../contracts/ui-graphics-asset-data.md) | **合同就绪** 交接/布局/资源接缝；已接受 R2/R2a 前缀中 `FieldMenu` **已确认未到达**；所需页面/呈现是**显式产品决定** | 已到达前缀不需要 FieldMenu 合同；未来自然到达路线若需要，才触发受限条件所有者 |
| 地图资源与摄像机 | [地图布局数据](../../contracts/map-layout-data.md)、[地图调色板数据](../../contracts/map-palette-data.md)、[地图瓦片集数据](../../contracts/map-tileset-data.md)、[地图精灵图形数据](../../contracts/map-sprite-graphics-data.md)、[地图实体数据](../../contracts/map-entity-data.md)、[地图摄像机更新](../../contracts/map-camera-update-control-flow.md) | **合同就绪静态导入/局部控制所有者**；私有原版资源与 8C 已有**已接受产品决定** | 到达的像素/调色板/帧/硬件行为、私有捕获溯源与精确容差保持**未知/推迟** |
| 地图到战斗准入 | [探索控制流](../../contracts/exploration-control-flow.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、[战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md)、上方 R2b/R2c 所有者 | **合同就绪静态路线/准入主干**；连续场景组合仍**缺失设计合同** | 自然 R2a 至 R2b 至 R2c 连续性、调用方顺序、过场执行与首个战斗就绪状态保持**未知/推迟** |
| Battle 01 遭遇配置 | [战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战场导航](../../contracts/battlefield-navigation.md)、上方 R2c/R3a 所有者 | **合同就绪静态遭遇/控制主干** | 自然初始化快照、首个行动者、玩家就绪状态与后续回合状态保持**未知/推迟** |
| 玩家回合与战斗菜单 | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、[输入系统](../../contracts/input-system.md)、上方 R3a 所有者 | **合同就绪静态分支/请求/局部输出所有者**；手动能动性与 UI 已有**已接受产品决定** | 到达的玩家/AI 分支、命令、移动、目标、动作、取消与结果保持**未知/推迟** |
| AI 与导航 | [战斗 AI 决定](../../contracts/battle-ai-decision.md)、[战场导航](../../contracts/battlefield-navigation.md)、上方 R3a/R3b 所有者 | **合同就绪算法与静态调用方拓扑** | 实际 AI 分支、命令、移动、目标、结果与多回合决定保持**未知/推迟** |
| 动作构建与解决 | [战斗动作构建](../../contracts/battle-action-construction.md)、[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)、[随机性](../../contracts/randomness.md)、上方 R3b/R3c 所有者 | **合同就绪受限算法及静态动作/效果/完成拓扑**；一个确定性 H4 轨迹已有**已接受产品决定** | 到达的种子、动作、解决/状态/死亡/EXP/金币/掉落/后续结果、回放与下一回合分发保持**未知/推迟** |
| 战斗呈现 | [战斗演出呈现](../../contracts/battle-scene-presentation.md)、专用 graphics-data 合同及上方 R3c/R3d 所有者 | **合同就绪加载器/静态资源及回放/收尾拓扑**；私有原版资源与 8C 已有**已接受产品决定** | 到达的场景、帧、音频、硬件时间线、私有捕获与精确容差保持**未知/推迟** |
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
| 路线 | **已接受：2A 最小研究证明自然路线** | R2/R2a 只拥有到达的运行时前缀，R2b/R2c 拥有静态延续；完整自然连续性、效果与回溯仍开放 |
| 自然战斗/过场 | **已接受：3A 时间顺序，占位子条款由 7C/8C 取代** | R2c 只拥有静态准入拓扑；自然准入、开战前/开战执行、渲染时序与首个战斗就绪状态仍开放 |
| 完成端点 | **已接受：5B 战后程序之后首个稳定可控状态** | 精确返回地图/位置/状态仍由研究所有；仅 `D4=1` 不充分 |
| 存档/读取 | **已接受：排除 6A** | 重启回到准入快照；后续存档支持是独立里程碑 |
| 玩家控制与 UI | **已接受：4A/9A 手动能动性与现代可访问逻辑控制** | 精确到达动作/输入轨迹与可执行可访问性断言仍开放 |
| 资源 | **已接受：7C 仅限私有本地原版资源** | 必须闭合忽略的私有溯源/清单；没有权利/替代品时仍阻止公开分发 |
| 视觉/音频一致性 | **已接受：8C 帧/音频/硬件精确** | 完整到达的像素/调色板/帧/音频/芯片/VInt/DMA/CRAM/VDP 证据与 H4 定义仍开放 |
| RNG 与动作轨迹 | **已接受：一个确定性 H4 参考轨迹** | R3a–R3d 只拥有静态控制/动作/完成/收尾拓扑；可行种子与到达逻辑轨迹仍开放，普通交互式游玩不被脚本化 |
| 有意偏差 | **已接受：10A 显式台账** | 受控准入、可选范围、现代控制、无存档、固定参考轨迹与域外引擎行为需要具名检查 |
| 可选工具 | **推迟且不阻塞；未采用 MCP** | CLI 门保持权威；任何工具选择都不启动 Phase 4 |

## 有序闭合计划

### 切片 0：持久就绪台账

本文档是其初始切片中唯一拥有的工件。它没有可执行 fixture 注册，也没有 research-index `designContracts` 关联。初步语义审查后，其唯一共享注册应是 `docs/README.md` 综合索引与 `manifests/zh-translation-index.json` 中的一个待处理条目。

### 切片 1：battle-functions 控制合同

**已闭合。**[战斗函数控制流](../../contracts/battle-functions-control-flow.md)只消费 `sf2-battle-functions-static-v1`，并关联本台账所列恰好 15 条 `battle.functions.*` 记录。运行时输入、完整取消、呈现、调用方效果与自然 Battle 01 可达性按要求保持独立或 **未知**。

### 切片 2：显式产品验收决定

**已闭合。**[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)接受上文记录的精确画像，但不把产品选择改写为原版行为。闭合这些选择不会启动 Phase 4。

### 切片 3：已接受静态链与条件运行时闭合

上方列出的 R1/R2/R2a 运行时 fixture 与 R2b 至 R4a 静态 fixture 已被接受。最终连续里程碑验收仍需要有界闭合：

1. 自然 R2a 至 R2b 至 R2c 连续性及进入 Battle 01 的调用方顺序；
2. 有界运行时前缀未闭合的所选后续配置/事件/程序/对话/菜单/状态效果；
3. 自然 Battle 01 进入，包括所需开战/开战前过场执行与初始化状态；
4. 一条经胜利的完整可玩多回合路径，识别每个到达的玩家、AI、导航、动作、解决、奖励与状态分支；
5. 战后程序效果、返回路由与精确可观察结束状态；
6. 完整到达的 8C 呈现与硬件行为：像素/调色板输出、帧节奏、动画/时序、音频波形/芯片/时序、VInt/DMA/CRAM/VDP 及其他可观察行为；
7. 私有参考捕获溯源、确定性捕获条件、精确或字段特定容差，以及许可安全的公开报告。

只有在 ADR 0014 的即时三项门禁准入调用方相关问题时，研究才可以把这些观察分组到一个或多个测试夹具。已在 `main` 接受的静态所有者必须复用，不得重新开启。失败的 R2b 与原版参考候选依 ADR 0015 保持非证据。设计不得提前命名未接受 fixture ID 或消费未合并结论。此列表不是默认的实现启动队列。

### 切片 4：连续场景合同

最终里程碑实际需要的运行时/自然闭合被准入并接受后，最小连贯场景合同提议为 `docs/design/contracts/map3-battle01-continuous-scenario.md`。其精确 fixture 与关联集必须从那些已接受所有者派生。它不得自动声称 26 条 Map 3 聚合行或复制既有战斗所有者。

该合同应定义准入状态、有序路线、过渡、完整已接受战斗轨迹、战后效果、可观察端点与 H4 组合。它必须保留每个局部测试夹具作为权威子系统黄金值，而不是把所选预期复制进一个更弱的聚合测试夹具。

### 切片 5：条件合同扩展

只有已接受路线与产品画像可以触发以下内容：

- 路线需要时创建野外菜单控制合同；
- 对实际到达缺口的有界 AI、战斗、法术、存档、对话或呈现扩展；
- 不批量闭合无关可选内容。

### 切片 6：最终就绪更新

只有在以下全部完成之后，本台账才可从 **未就绪** 变为 **就绪，可作阶段转换决定**：

- 每个所需运行时/自然闭合被 `main` 接受，或由已接受所有者显式排除且不削弱连续里程碑；
- 连续场景合同被接受（battle-functions 合同已闭合）；
- 所有路线所需条件所有者被接受；
- 已接受 ADR 0010 画像与每个场景/H4 所有者保持内部一致；
- 私有本地资源清单、溯源、忽略输入处理与禁止公开分发边界闭合；可分发构建在获得权利/替代品前仍单独受阻；
- 完整 H4 验收合同与矩阵、可执行检查定义、可观察层、容差与已声明预期偏差在 `main` 上完整指定并接受；
- 主门禁独立报告就绪。

ADR 0009 要求的独立用户启动动作已于 2026-08-28 为 ADR 0016 下的有界 Phase 4 实现发生。它满足实现启动门，但不改变本台账的**未就绪**状态，也不豁免上方任何最终连续里程碑门。

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
9. 8C 像素/调色板/帧节奏、动画/时序、音频波形/芯片/时序、VInt/DMA/CRAM/VDP、其他到达的硬件可观察断言、确定性捕获条件、精确或字段特定容差与许可安全公开报告形状；
10. 单独命名的预期偏差。

每层必须引用其所属已接受测试夹具。连续适配器不得替换子系统测试夹具、把其预期数字复制进引擎特定测试、在重制中要求原版 RAM/ROM 地址，或发布私有原版文本、图形、音频或捕获。

在连续里程碑被验收之前，就绪要求该验收面及其可执行检查定义完整并被接受，而非针对尚未闭合该场景的重制成功执行。构建连续重制适配器并获得 H4 PASS 结果，仍是已记录独立用户启动动作之后的 Phase 4 实现与里程碑门。

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
| 自然 Map 3 路线精确 | 开放 | 研究，然后场景合同 |
| 所需探索/对话/菜单/UI 范围精确 | 开放 | 研究加路线所需条件合同；ADR 0010 固定最小范围规则 |
| 静态 Battle 01 准入主干已接受 | PASS | `sf2-map3-battle01-admission-static-v1`；自然准入由下方独立开放行表示 |
| 自然 Battle 01 准入精确 | 开放 | 条件运行时证据，然后场景合同 |
| 玩家回合合同在场 | PASS | [战斗函数控制流](../../contracts/battle-functions-control-flow.md) |
| R3a–R3d 静态控制/动作/完成/收尾链已接受 | PASS | 已接受静态 fixture；到达分支/结果仍开放 |
| 完整可玩 Battle 01 轨迹精确 | 开放 | 条件运行时证据加既有/扩展战斗合同 |
| R4a 静态胜利/战后程序/返回主干已接受 | PASS | `sf2-map3-battle01-victory-return-static-v1` |
| 战后程序到达、完成与效果精确 | 开放 | 条件运行时证据，然后场景合同 |
| 可观察端点形状已选择 | PASS | ADR 0010 选项 5B |
| 精确端点状态已有证据 | 开放 | 条件运行时证据，然后场景合同 |
| 存档范围已选择 | PASS | ADR 0010 选项 6A 排除存档/读取/检查点/挂起 |
| 可访问性/输入产品接口已选择 | PASS | ADR 0010 选项 9A |
| 可访问性可观察检查已组合 | 开放 | 连续 H4 合同；偏差与 8C 精确参考运行分开 |
| 7C 私有本地资源模式与禁止公开分发边界已选择 | PASS | ADR 0010 |
| 精确私有资源/捕获清单与溯源已接受 | 开放 | 研究/私有输入验收；无载荷进入 Git/公开 CI |
| 公开/可分发资源权利或替代品 | 私有里程碑之外受阻 | 任何公开构建前的独立许可/替代决定 |
| 8C 视觉/音频/硬件一致性层已选择 | PASS | ADR 0010 |
| 完整到达的 8C 证据、捕获域与容差已接受 | 开放 | 条件研究/私有参考验收，然后连续 H4 合同 |
| 连续 H4 验收面与可执行检查定义已接受 | 开放 | 场景合同 |
| 主门禁就绪报告已接受 | 开放 | 主门禁 |
| 独立用户 Phase 4 启动动作 | PASS | 用户授权已记录于 [`remake/README.md`](../../../../remake/README.md)；仅满足实现启动，不代表里程碑就绪 |

只要有任何所需行开放，台账对于最终连续里程碑验收就保持 **未就绪**。默认情况下，这些行不阻碍另行取得用户授权的具体实现切片。

## 证据矩阵

| 台账陈述 | 分类 | 已接受所有者 | 保留边界 |
| --- | --- | --- | --- |
| 受控 Map 3 起点以精确有界状态到达首个探索等待 | **合同就绪 / 受限运行时** | `sf2-map3-admitted-start-runtime-v1`、[Map 3 受控准入](../../contracts/map3-controlled-admission.md) | 不是自然玩家可见 New/读取流程或后续 Map 3 路线 |
| 26 条 Map 3 源码路径记录存在且聚合拥有 | **已确认索引清单** | `sf2-map-data-static-v1`、[map-data 研究](../../../research/map-data-inventory.md) | 不是路线时间线、可达性、效果或自动未来关联 |
| 已观察自然开场与 messenger 接受前缀 | **合同就绪有界运行时前缀** | `sf2-map3-battle01-natural-route-runtime-v1`、`sf2-map3-messenger-acceptance-runtime-v1` | 终止于程序入口/跟随者就绪边界；后续连续性未证明，`FieldMenu` 未到达 |
| R2b/R2c 合法路线、解锁、准入与初始化拓扑存在 | **合同就绪静态链** | `sf2-map3-castle-battle-unlock-static-v1`、`sf2-map3-battle01-admission-static-v1` | 不是自然执行、调用方顺序、过场执行、初始化快照或首个行动者 |
| R3a–R3d 回合、动作/效果、完成、回放与收尾拓扑存在 | **合同就绪静态链** | 四个已接受 R3 静态 fixture 及其链接研究所有者 | 不是到达的玩家/AI/动作/结果、回放、下一回合、多回合游玩或胜利 |
| 15 条 battle-functions 记录有已接受静态证据与一个受限设计合同 | **合同就绪** | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、`sf2-battle-functions-static-v1` | 无摄像机所有者重叠，无运行时/输入/呈现泛化 |
| R4a 胜利、战后程序、返回、SwitchMap 与探索调用主干存在 | **合同就绪静态链** | `sf2-map3-battle01-victory-return-static-v1`、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | 胜利/程序到达与完成、标志/加入结果、探索重入、稳定端点、R4b 与 H4 保持开放 |
| 局部战斗合同可在概念上组合 | **综合就绪** | [战术战斗循环](../../synthesis/tactical-battle-loop.md) 与链接合同 | 不是完整预测 Battle 01 模拟或场景黄金值 |
| Godot/C#、里程碑、产品画像与推迟政策已选择 | **已接受决定** | ADR 0008 / ADR 0009 / ADR 0010 / ADR 0016 | 有界实现授权不意味着连续里程碑、MCP、再分发或证据闭合 |
| 路线类别、端点形状、存档排除、UI、私有资源、RNG 策略、8C 一致性与偏差 | **已接受产品决定** | ADR 0010 | 精确场景值、自然时间顺序、私有捕获溯源与一致性事实仍是研究/H4 缺口 |
