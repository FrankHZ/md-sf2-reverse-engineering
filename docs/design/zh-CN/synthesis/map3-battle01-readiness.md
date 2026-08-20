# Map 3 至 Battle 01 就绪台账

- 状态：**未就绪**（针对 Phase 4 实现）
- 初始缺口审计日期：2026-08-14
- 初始缺口审计基线：commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`，tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- 产品决定日期：2026-08-19
- 已接受状态刷新：2026-08-20，commit `9a7cbcb44322e309ef10d8afac76d9a98be76f98`，
  tree `28c5f9c00a2b095d8b990eb8adc5249ede911704`
- 里程碑所有者：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)
- 工具边界：[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- 产品画像：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)
- 范围：对一个连续可玩场景——从准入的 Map 3 起点到 Battle 01 可观察完成——的 Layer B 就绪核算

> 本文件是 [`map3-battle01-readiness.md`](../../synthesis/map3-battle01-readiness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本文档是一份就绪台账。它不拥有原版游戏证据、不定义新场景合同、不选择产品体验、不授权 Phase 4，也不替换它所链接的测试夹具与合同。其目的是说明已接受的 `main` 已经能支持什么、什么仍然开放、谁必须拥有每个闭合，以及后续 Phase 4 启动门必须检查什么。

产品选择槽与 battle-functions 合同现已闭合，但当前判断仍是 **未就绪**，因为已接受证据尚未把受控 Map 3 交接、自然时间顺序探索路线、Battle 01 准入、完整可玩战斗、战后程序与一个精确可观察结束状态连接起来。

以下区分是规范性的：

- 受控辅助或调试接缝不是自然剧情路线；
- 静态源码图不是被观察的时间顺序通关；
- fixture 局部 H4 面不是端到端场景黄金值；
- 已索引文件不自动成为未来设计关联；
- 数字 ID 或源码标签不是面向玩家的含义；
- 私有原版资源不是可分发重制资源；
- 就绪闭合不是创建 `remake/` 或开始 Phase 4 的授权。

任何未合并的研究结果都不计入本台账。未来更新只可在新证据被 `main` 接受后消费它。

已接受状态刷新针对具名的当前 main 基线，记录已集成的 battle-functions 合同与 ADR 0010。它不是对完整初始缺口审计的重新执行，也不提升任何未合并的研究或工具结论。

## 就绪分类

每个依赖被赋予以下精确分类中的一种或多种：

| 分类 | 在本台账中的含义 |
| --- | --- |
| **合同就绪** | 已接受的实现无关合同闭合了具名本地输入、顺序、状态或输出边界。这不意味着连续场景就绪。 |
| **综合就绪** | 已接受所有者可以在不创造新证据或场景级主张的情况下支持受限 Layer B 解释。 |
| **缺失研究所有者** | 已接受 `main` 缺少里程碑所需的自然调用方、运行时、路线、持久性或呈现证据。 |
| **缺失设计合同** | 已接受研究存在，但尚无证据绑定设计合同拥有里程碑所需的实现无关面。 |
| **显式产品决定** | 答案是重制范围、体验、资源、可访问性、保真或偏差选择，而非可恢复的原版游戏事实。 |

一行可以在局部合同就绪，同时仍包含场景级研究或决定缺口。这是预期的：本里程碑需要组合，而不仅仅是子系统文件的存在。

## ADR 门

[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md) 接受 Godot 4.7.2 .NET、C#、CLI-first 工具链、纯 C# 确定性领域层与薄 Godot 适配器。它不安装 Godot、不选择 MCP 适配器、不选择可分发资源、不创建重制项目，也不授权实现。首个实现验收画像保持仅 CLI。

[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md) 接受恰好一个首里程碑：从 Map 3 到 Battle 01 **完成**的连续可玩场景。它要求研究与设计缺口闭合、主门禁就绪报告与独立的用户启动动作。战斗进入、初始化或孤立机制不能满足该里程碑。

[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md) 接受精确画像 `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8C + 9A + 10A`。它选择仅限私有本地的原版资源画像、禁止公开再分发，并要求帧/音频/硬件精确一致性。这些选择闭合产品槽，但扩大了研究、私有溯源与 H4 工作；它们并未使场景就绪。

因此本台账在以下所有剩余闭合行被 `main` 接受之前，保持 **未就绪**。

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

## 连续场景依赖矩阵

| 场景段 | 已接受所有者面 | 就绪分类 | 所需闭合 |
| --- | --- | --- | --- |
| 准入的 Map 3 起点 | [新游戏状态初始化](../../contracts/new-game-state-initialization.md)、[存档系统](../../contracts/save-system.md)、[故事推进](../../synthesis/story-progression.md) | **合同就绪** 受控初始化与 Map 3 交接；受控准入快照已有**已接受产品决定**；其精确值/溯源仍**缺失研究所有者** | 识别每个场景相关的地图、位置、朝向、标志、队伍、属性、物品、法术、金币、难度、RNG 与时间字段，不把它描述为规范原版 New/读取状态 |
| Map 3 配置与内容 | [地图配置数据](../../contracts/map-setup-data.md)、[地图与探索](../../contracts/map-exploration.md)、已接受聚合地图清单 | **合同就绪** 选择器与通用结构；**缺失研究所有者** 所选 Map 3 行与时间线；**缺失设计合同** 场景内容 | 从准入状态观察所选配置/事件/程序链，并在不批量关联聚合行的情况下添加专用证据绑定场景/数据合同 |
| 探索循环与输入 | [探索控制流](../../contracts/exploration-control-flow.md)、[输入系统](../../contracts/input-system.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md) | **合同就绪** 局部优先级/交接规则；现代逻辑控制/可访问性已有**已接受产品决定**；自然输入与结果仍**缺失研究所有者** | 记录路线有序玩家输入与自然循环交接；让产品映射、重复与可访问性保持区别于原版行为 |
| 对话与交互 | [对话系统](../../contracts/dialogue-system.md)、[精灵对话属性数据](../../contracts/sprite-dialogue-property-data.md)、[文本与字体系统](../../contracts/text-and-font-system.md)、[立绘窗口状态](../../contracts/portrait-window-state.md) | **合同就绪** 命令/存储/窗口接缝；仅限私有本地的原版文本已有**已接受产品决定**；路线内容/效果仍**缺失研究所有者** | 识别所需对话/交互程序、游标/状态效果与完成边界；保留忽略的私有输入，并在没有权利/替代品时阻止公开分发 |
| 野外菜单与 UI | [探索控制流](../../contracts/exploration-control-flow.md)、[窗口系统](../../contracts/window-system.md)、[UI 布局数据](../../contracts/ui-layout-data.md)、[UI 图形资源数据](../../contracts/ui-graphics-asset-data.md) | **合同就绪** 交接/布局/资源接缝；**缺失设计合同**（若路线需要 FieldMenu 行为）；**显式产品决定** 所需页面与呈现 | 显式包含或排除野外菜单、物品、状态、选项与取消路径；若包含，从已接受证据创建受限野外菜单控制合同 |
| 地图资源与摄像机 | [地图布局数据](../../contracts/map-layout-data.md)、[地图调色板数据](../../contracts/map-palette-data.md)、[地图瓦片集数据](../../contracts/map-tileset-data.md)、[地图精灵图形数据](../../contracts/map-sprite-graphics-data.md)、[地图实体数据](../../contracts/map-entity-data.md)、[地图摄像机更新](../../contracts/map-camera-update-control-flow.md) | **合同就绪** 私有导入与局部服务/控制面；私有原版资源与 8C 一致性已有**已接受产品决定**；完整到达的视觉/硬件行为仍**缺失研究所有者** | 建立忽略的私有资源/捕获溯源与精确像素/调色板/帧/硬件验收，不使原版载荷可分发 |
| 地图到战斗准入 | [探索控制流](../../contracts/exploration-control-flow.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、[战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | **合同就绪** 静态交接；**缺失研究所有者** 自然路线与过场效果；**缺失设计合同** 场景交接 | 观察进入 Battle 01 的精确地图/配置/事件/标志路径、开战/开战前过场执行与首个战斗就绪状态 |
| Battle 01 遭遇配置 | [战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战场导航](../../contracts/battlefield-navigation.md) | **合同就绪** 放置、地形、激活、首回合与控制器接缝；**缺失研究所有者** 自然完整遭遇状态 | 把自然进入快照绑定到场景实际使用的精确名册/属性/物品/法术/位置/标志与后期回合状态 |
| 玩家回合与战斗菜单 | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、已接受 [battle-functions 研究](../../../research/battle-functions.md)、[输入系统](../../contracts/input-system.md) | **合同就绪** 静态分支/请求/局部输出面；手动能动性与 UI 已有**已接受产品决定**；完整自然到达轨迹仍**缺失研究所有者** | 识别已接受胜利轨迹实际到达的精确动作族与取消路径，不泛化 fixture 局部行为 |
| AI 与导航 | [战斗 AI 决定](../../contracts/battle-ai-decision.md)、[战场导航](../../contracts/battlefield-navigation.md) | **合同就绪** 受限算法；**缺失研究所有者** 完整自然到达的多回合决定 | 捕获每个到达的 Battle 01 AI/导航分支，只闭合已接受通关所需的 fixture 缺口 |
| 动作构建与解决 | [战斗动作构建](../../contracts/battle-action-construction.md)、[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)、[随机性](../../contracts/randomness.md) | **合同就绪** 受限子集；一个确定性 H4 参考轨迹已有**已接受产品决定**；其可行种子与到达的不受支持分支仍**缺失研究所有者** | 按顺序记录到达动作，只扩展通关所需所有者；不得约束其他交互式游玩或泛化子集 fixture |
| 战斗呈现 | [战斗演出呈现](../../contracts/battle-scene-presentation.md) 及其专用 graphics-data 合同 | **合同就绪** 命令/加载器/静态资源接缝；仅限私有本地的原版资源与 8C 帧/音频/硬件精确性已有**已接受产品决定**；完整到达的一致性仍**缺失研究所有者** | 闭合像素/调色板/帧节奏、动画/时序、波形/芯片/时序、VInt/DMA/CRAM/VDP、私有捕获溯源、精确容差与许可安全报告 |
| 胜利与战后 | [战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | **合同就绪** 通用胜利顺序；**缺失研究所有者** 自然胜利、战后程序效果与最终路线；**缺失设计合同** 可观察完成 | 观察经普通控制器的胜利、战后 MAPSCRIPT 执行、返回路由与最终场景相关状态 |
| 存档/读取范围 | [存档系统](../../contracts/save-system.md)、[全局标志状态](../../contracts/global-flag-state.md)、名册/状态合同 | **合同就绪** 受限服务/存储接缝；里程碑排除存档/读取/检查点/挂起已有**已接受产品决定** | 强制重启回到准入快照，并让后续存档支持保持在本里程碑之外 |
| 端到端 H4 | 所有具名子系统测试夹具与合同 | **综合就绪** 用于台账；可观察层/偏差已有**已接受产品决定**；连续原版/8C 轨迹仍**缺失研究所有者**；场景组合与可执行定义仍**缺失设计合同** | 添加一个消费而非削弱子系统 fixture、并单独报告已声明偏差的证据绑定连续场景合同 |

## 既有综合边界

以下 Layer B 文档已能解释局部片段，但不闭合本里程碑：

- [游戏总览](../../synthesis/gameplay-overview.md) 连接顶层动作与子系统交接，同时保留战役、UI、时序与呈现缺口。
- [故事推进](../../synthesis/story-progression.md) 连接受控 Map 3 进入、静态配置/事件/脚本图、对话/名册/状态接缝与存档交接，同时显式拒绝重构正常战役路线。
- [地图设计原则](../../synthesis/map-design-principles.md) 把地图结构与路线质量、节奏、可达性与作者意图分开。
- [战术战斗循环](../../synthesis/tactical-battle-loop.md) 组合局部战斗控制器、玩家/AI、导航、动作、解决、回放与结果所有者，同时拒绝完整战斗模拟与可见时序主张。
- [推进与经济](../../synthesis/progression-and-economy.md) 连接奖励/状态变更，但不建立 Battle 01 路线、平衡或完整持久性。

这些文档是**综合就绪**的台账输入。没有一份是所需的连续场景合同。

## 已接受产品选择

[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md) 闭合产品选择槽，但不填入任何研究所有的精确值。

| 决定槽 | 已接受状态 | 剩余闭合 |
| --- | --- | --- |
| 准入起点 | **已接受：1A 受控准入快照** | 精确值与溯源仍由研究所有；它不是规范原版 New/读取主张 |
| 路线 | **已接受：2A 最小研究证明自然路线** | 精确有序路线、强制内容、效果与回溯仍由研究所有 |
| 自然战斗/过场 | **已接受：3A 时间顺序，占位子条款由 7C/8C 取代** | 精确自然准入、开战前/开战效果、渲染时序与首个战斗就绪状态仍开放 |
| 完成端点 | **已接受：5B 战后程序之后首个稳定可控状态** | 精确返回地图/位置/状态仍由研究所有；仅 `D4=1` 不充分 |
| 存档/读取 | **已接受：排除 6A** | 重启回到准入快照；后续存档支持是独立里程碑 |
| 玩家控制与 UI | **已接受：4A/9A 手动能动性与现代可访问逻辑控制** | 精确到达动作/输入轨迹与可执行可访问性断言仍开放 |
| 资源 | **已接受：7C 仅限私有本地原版资源** | 必须闭合忽略的私有溯源/清单；没有权利/替代品时仍阻止公开分发 |
| 视觉/音频一致性 | **已接受：8C 帧/音频/硬件精确** | 完整到达的像素/调色板/帧/音频/芯片/VInt/DMA/CRAM/VDP 证据与 H4 定义仍开放 |
| RNG 与动作轨迹 | **已接受：一个确定性 H4 参考轨迹** | 可行种子与逻辑轨迹仍由研究所有；普通交互式游玩不被脚本化 |
| 有意偏差 | **已接受：10A 显式台账** | 受控准入、可选范围、现代控制、无存档、固定参考轨迹与域外引擎行为需要具名检查 |
| 可选工具 | **推迟且不阻塞；未采用 MCP** | CLI 门保持权威；任何工具选择都不启动 Phase 4 |

## 有序闭合计划

### 切片 0：持久就绪台账

本文档是其初始切片中唯一拥有的工件。它没有可执行 fixture 注册，也没有 research-index `designContracts` 关联。初步语义审查后，其唯一共享注册应是 `docs/README.md` 综合索引与 `manifests/zh-translation-index.json` 中的一个待处理条目。

### 切片 1：battle-functions 控制合同

**已闭合。**[战斗函数控制流](../../contracts/battle-functions-control-flow.md)只消费 `sf2-battle-functions-static-v1`，并关联本台账所列恰好 15 条 `battle.functions.*` 记录。运行时输入、完整取消、呈现、调用方效果与自然 Battle 01 可达性按要求保持独立或 **未知**。

### 切片 2：显式产品验收决定

**已闭合。**[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)接受上文记录的精确画像，但不把产品选择改写为原版行为。闭合这些选择不会启动 Phase 4。

### 切片 3：研究闭合

研究必须合并以下专用证据：

1. 精确准入的 Map 3 起始溯源与进入 Battle 01 的自然时间顺序路线；
2. 该路线上所选配置/事件/程序/对话/菜单/状态效果；
3. 自然 Battle 01 进入，包括所需的开战/开战前过场行为；
4. 一条经胜利的完整可玩多回合路径，识别每个到达的玩家、AI、导航、动作、解决、奖励与状态分支；
5. 战后程序效果、返回路由与精确可观察结束状态；
6. 完整到达的 8C 呈现与硬件行为：像素/调色板输出、帧节奏、动画/时序、音频波形/芯片/时序、VInt/DMA/CRAM/VDP 及其他可观察行为；
7. 私有参考捕获溯源、确定性捕获条件、精确或字段特定容差，以及许可安全的公开报告。

研究可以把这些观察分组到一个或多个测试夹具。设计不得提前命名未接受 fixture ID 或消费未合并结论。

### 切片 4：连续场景合同

所需研究所有者合并后，最小连贯场景合同提议为 `docs/design/contracts/map3-battle01-continuous-scenario.md`。其精确 fixture 与关联集必须从那些已接受所有者派生。它不得自动声称 26 条 Map 3 聚合行或复制既有战斗所有者。

该合同应定义准入状态、有序路线、过渡、完整已接受战斗轨迹、战后效果、可观察端点与 H4 组合。它必须保留每个局部测试夹具作为权威子系统黄金值，而不是把所选预期复制进一个更弱的聚合测试夹具。

### 切片 5：条件合同扩展

只有已接受路线与产品画像可以触发以下内容：

- 路线需要时创建野外菜单控制合同；
- 对实际到达缺口的有界 AI、战斗、法术、存档、对话或呈现扩展；
- 不批量闭合无关可选内容。

### 切片 6：最终就绪更新

只有在以下全部完成之后，本台账才可从 **未就绪** 变为 **就绪，可作阶段转换决定**：

- 每个所需研究闭合被 `main` 接受；
- 连续场景合同被接受（battle-functions 合同已闭合）；
- 所有路线所需条件所有者被接受；
- 已接受 ADR 0010 画像与每个场景/H4 所有者保持内部一致；
- 私有本地资源清单、溯源、忽略输入处理与禁止公开分发边界闭合；可分发构建在获得权利/替代品前仍单独受阻；
- 完整 H4 验收合同与矩阵、可执行检查定义、可观察层、容差与已声明预期偏差在 `main` 上完整指定并接受；
- 主门禁独立报告就绪。

即便如此，Phase 4 也只在 ADR 0009 要求的独立用户启动动作之后开始。

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

在 Phase 4 之前，就绪要求该验收面及其可执行检查定义完整并被接受，而非针对尚不存在的重制成功执行。构建重制适配器并获得 H4 PASS 结果是独立用户启动动作之后 Phase 4 实现与里程碑门。

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
| 准入 Map 3 起始状态精确 | 开放 | 研究，然后场景合同与产品决定 |
| 自然 Map 3 路线精确 | 开放 | 研究，然后场景合同 |
| 所需探索/对话/菜单/UI 范围精确 | 开放 | 研究加路线所需条件合同；ADR 0010 固定最小范围规则 |
| 自然 Battle 01 准入精确 | 开放 | 研究，然后场景合同 |
| 玩家回合合同在场 | PASS | [战斗函数控制流](../../contracts/battle-functions-control-flow.md) |
| 完整可玩 Battle 01 轨迹精确 | 开放 | 研究加既有/扩展战斗合同 |
| 战后效果精确 | 开放 | 研究，然后场景合同 |
| 可观察端点形状已选择 | PASS | ADR 0010 选项 5B |
| 精确端点状态已有证据 | 开放 | 研究，然后场景合同 |
| 存档范围已选择 | PASS | ADR 0010 选项 6A 排除存档/读取/检查点/挂起 |
| 可访问性/输入产品接口已选择 | PASS | ADR 0010 选项 9A |
| 可访问性可观察检查已组合 | 开放 | 连续 H4 合同；偏差与 8C 精确参考运行分开 |
| 7C 私有本地资源模式与禁止公开分发边界已选择 | PASS | ADR 0010 |
| 精确私有资源/捕获清单与溯源已接受 | 开放 | 研究/私有输入验收；无载荷进入 Git/公开 CI |
| 公开/可分发资源权利或替代品 | 私有里程碑之外受阻 | 任何公开构建前的独立许可/替代决定 |
| 8C 视觉/音频/硬件一致性层已选择 | PASS | ADR 0010 |
| 完整到达的 8C 证据、捕获域与容差已接受 | 开放 | 研究，然后连续 H4 合同 |
| 连续 H4 验收面与可执行检查定义已接受 | 开放 | 场景合同 |
| 主门禁就绪报告已接受 | 开放 | 主门禁 |
| 独立用户 Phase 4 启动动作 | 开放 | 用户 |

任何所需行开放时，台账保持 **未就绪**。

## 证据矩阵

| 台账陈述 | 分类 | 已接受所有者 | 保留边界 |
| --- | --- | --- | --- |
| 受控 New 动作以当前/退出地图 3 到达 MainLoop | **合同就绪 / 受限运行时** | [故事推进](../../synthesis/story-progression.md)、[存档系统](../../contracts/save-system.md) | 不是自然玩家可见 New 流程或精确 Map 3 起始快照 |
| 26 条 Map 3 源码路径记录存在且聚合拥有 | **已确认索引清单** | `sf2-map-data-static-v1`、[map-data 研究](../../../research/map-data-inventory.md) | 不是路线时间线、可达性、效果或自动未来关联 |
| 静态探索、选择器、地图、输入、对话、UI 与服务接缝存在 | **合同就绪 局部面** | 依赖矩阵中链接的合同 | 不是完整 Map 3 体验 |
| Battle 01 放置、地形、区域激活、首回合与通用结果顺序存在 | **合同就绪 静态/运行时子集** | [战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗控制](../../contracts/battle-control-lifecycle.md) | 调试入口跳过过场；完整自然遭遇与端点保持开放 |
| 15 条 battle-functions 记录有已接受静态证据与一个受限设计合同 | **合同就绪** | [战斗函数控制流](../../contracts/battle-functions-control-flow.md)、`sf2-battle-functions-static-v1` | 无摄像机所有者重叠，无运行时/输入/呈现泛化 |
| 战后路线/程序身份存在 | **合同就绪 路线结构** | [战斗过场路由](../../contracts/battle-cutscene-routing.md) | 程序效果、自然可达性、持久性与可见顺序保持开放 |
| 局部战斗合同可在概念上组合 | **综合就绪** | [战术战斗循环](../../synthesis/tactical-battle-loop.md) 与链接合同 | 不是完整预测 Battle 01 模拟或场景黄金值 |
| Godot/C#、里程碑与产品画像已选择 | **已接受决定** | ADR 0008 / ADR 0009 / ADR 0010 | 无 Phase 4 启动、MCP 采用、公开再分发或证据闭合 |
| 路线类别、端点形状、存档排除、UI、私有资源、RNG 策略、8C 一致性与偏差 | **已接受产品决定** | ADR 0010 | 精确场景值、自然时间顺序、私有捕获溯源与一致性事实仍是研究/H4 缺口 |
