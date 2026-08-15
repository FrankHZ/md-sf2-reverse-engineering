# Map 3 至 Battle 01 就绪台账

- 状态：**未就绪**（针对 Phase 4 实现）
- 审计日期：2026-08-14
- 已接受 main 审计基线：commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`，tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- 里程碑所有者：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)
- 工具边界：[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- 范围：对一个连续可玩场景——从准入的 Map 3 起点到 Battle 01 可观察完成——的 Layer B 就绪核算

> 本文件是 [`map3-battle01-readiness.md`](../../synthesis/map3-battle01-readiness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本文档是一份就绪台账。它不拥有原版游戏证据、不定义新场景合同、不选择产品体验、不授权 Phase 4，也不替换它所链接的测试夹具与合同。其目的是说明已接受的 `main` 已经能支持什么、什么仍然开放、谁必须拥有每个闭合，以及后续 Phase 4 启动门必须检查什么。

当前判断是 **未就绪**，因为已接受证据尚未把受控 Map 3 交接、自然时间顺序探索路线、Battle 01 准入、完整可玩战斗、战后程序与一个精确可观察结束状态连接起来。

以下区分是规范性的：

- 受控辅助或调试接缝不是自然剧情路线；
- 静态源码图不是被观察的时间顺序通关；
- fixture 局部 H4 面不是端到端场景黄金值；
- 已索引文件不自动成为未来设计关联；
- 数字 ID 或源码标签不是面向玩家的含义；
- 私有原版资源不是可分发重制资源；
- 就绪闭合不是创建 `remake/` 或开始 Phase 4 的授权。

任何未合并的研究结果都不计入本台账。未来更新只可在新证据被 `main` 接受后消费它。

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

[ADR 0008](../../../decisions/0008-godot-csharp-cli-first-remake-tooling.md) 接受 Godot 4.7.1 .NET、C#、CLI-first 工具链、纯 C# 确定性领域层与薄 Godot 适配器。它不安装 Godot、不选择 MCP 适配器、不选择可分发资源、不创建重制项目，也不授权实现。首个实现验收画像保持仅 CLI。

[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md) 接受恰好一个首里程碑：从 Map 3 到 Battle 01 **完成**的连续可玩场景。它要求研究与设计缺口闭合、主门禁就绪报告与独立的用户启动动作。战斗进入、初始化或孤立机制不能满足该里程碑。

因此本台账在以下所有闭合行被 `main` 接受且产品选择槽被后续用户接受决定解决之前，保持 **未就绪**。

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

### battle-functions 合同缺口

`sf2-battle-functions-static-v1` 直接绑定恰好 15 条研究索引记录。全部 15 条当前未关联：

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

[battle-functions 研究所有者](../../../research/battle-functions.md) 还报告一个 16 记录源码路径成员连接，因为 `map.camera-control.destination-service` 共享一个源文件。该跨所有者记录不是 `sf2-battle-functions-static-v1` 的直接绑定，仍属于[map-camera update 合同](../../contracts/map-camera-update-control-flow.md)。它不是未来 battle-functions 合同的候选。

### Battle 01 路线与结果行

`battle.cutscene.data.battle01.beforebattle` 与 `battle.cutscene.data.battle01.afterbattle` 仍与[战斗过场路由](../../contracts/battle-cutscene-routing.md)关联。该合同闭合路线表、准入与静态程序语料事实；它显式保留完整 MAPSCRIPT 效果、自然可达性、持久性、可见排序与故事后果开放。

[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md) 闭合通用胜利变更顺序：治愈合格队伍、运行战后接缝、清除解锁标志、设置完成标志并返回 `D4=1`。已接受 Battle 01 H3 入口使用 Debug Battle Test 并跳过开战/开战前过场。这两个事实都不建立自然 Battle 01 准入、战后程序效果或里程碑的可观察端点。

## 连续场景依赖矩阵

| 场景段 | 已接受所有者面 | 就绪分类 | 所需闭合 |
| --- | --- | --- | --- |
| 准入的 Map 3 起点 | [新游戏状态初始化](../../contracts/new-game-state-initialization.md)、[存档系统](../../contracts/save-system.md)、[故事推进](../../synthesis/story-progression.md) | **合同就绪** 受控初始化与 Map 3 交接；**缺失研究所有者** 精确准入快照；**显式产品决定** 进入 UX | 识别起始溯源与每个场景相关的地图、位置、朝向、标志、队伍、属性、物品、法术、金币、难度、RNG 与时间字段；决定进入是规范快照、可见 New 流程还是读取 |
| Map 3 配置与内容 | [地图配置数据](../../contracts/map-setup-data.md)、[地图与探索](../../contracts/map-exploration.md)、已接受聚合地图清单 | **合同就绪** 选择器与通用结构；**缺失研究所有者** 所选 Map 3 行与时间线；**缺失设计合同** 场景内容 | 从准入状态观察所选配置/事件/程序链，并在不批量关联聚合行的情况下添加专用证据绑定场景/数据合同 |
| 探索循环与输入 | [探索控制流](../../contracts/exploration-control-flow.md)、[输入系统](../../contracts/input-system.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md) | **合同就绪** 局部优先级/交接规则；**缺失研究所有者** 自然输入与结果；**显式产品决定** 控制 | 记录路线有序玩家输入与自然循环交接；选择平台映射、重复/取消/可访问性策略而不把它归于原版 |
| 对话与交互 | [对话系统](../../contracts/dialogue-system.md)、[精灵对话属性数据](../../contracts/sprite-dialogue-property-data.md)、[文本与字体系统](../../contracts/text-and-font-system.md)、[立绘窗口状态](../../contracts/portrait-window-state.md) | **合同就绪** 命令/存储/窗口接缝；**缺失研究所有者** 路线内容/效果；**显式产品决定** 可见文本与本地化 | 识别所需对话/交互程序、游标/状态效果与完成边界；决定原版文本是否仅私有，以及出现什么可分发替代/本地化 |
| 野外菜单与 UI | [探索控制流](../../contracts/exploration-control-flow.md)、[窗口系统](../../contracts/window-system.md)、[UI 布局数据](../../contracts/ui-layout-data.md)、[UI 图形资源数据](../../contracts/ui-graphics-asset-data.md) | **合同就绪** 交接/布局/资源接缝；**缺失设计合同**（若路线需要 FieldMenu 行为）；**显式产品决定** 所需页面与呈现 | 显式包含或排除野外菜单、物品、状态、选项与取消路径；若包含，从已接受证据创建受限野外菜单控制合同 |
| 地图资源与摄像机 | [地图布局数据](../../contracts/map-layout-data.md)、[地图调色板数据](../../contracts/map-palette-data.md)、[地图瓦片集数据](../../contracts/map-tileset-data.md)、[地图精灵图形数据](../../contracts/map-sprite-graphics-data.md)、[地图实体数据](../../contracts/map-entity-data.md)、[地图摄像机更新](../../contracts/map-camera-update-control-flow.md) | **合同就绪** 私有导入与局部服务/控制面；**显式产品决定** 可见保真与资源 | 选择占位/许可呈现与验收层；不得让私有原版载荷可分发 |
| 地图到战斗准入 | [探索控制流](../../contracts/exploration-control-flow.md)、[地图入口路由状态](../../contracts/map-entry-routing-state.md)、[战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | **合同就绪** 静态交接；**缺失研究所有者** 自然路线与过场效果；**缺失设计合同** 场景交接 | 观察进入 Battle 01 的精确地图/配置/事件/标志路径、开战/开战前过场执行与首个战斗就绪状态 |
| Battle 01 遭遇配置 | [战斗遭遇定义](../../contracts/battle-encounter-definition.md)、[战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战场导航](../../contracts/battlefield-navigation.md) | **合同就绪** 放置、地形、激活、首回合与控制器接缝；**缺失研究所有者** 自然完整遭遇状态 | 把自然进入快照绑定到场景实际使用的精确名册/属性/物品/法术/位置/标志与后期回合状态 |
| 玩家回合与战斗菜单 | 已接受 [battle-functions 研究](../../../research/battle-functions.md)、[输入系统](../../contracts/input-system.md) | **缺失设计合同** 精确 15 记录 fixture 集；**显式产品决定** 所需能动性与 UI | 创建 `battle-functions-control-flow` 作为独立已接受证据合同；决定所需玩家动作、取消路径、可选菜单与平台控制 |
| AI 与导航 | [战斗 AI 决定](../../contracts/battle-ai-decision.md)、[战场导航](../../contracts/battlefield-navigation.md) | **合同就绪** 受限算法；**缺失研究所有者** 完整自然到达的多回合决定 | 捕获每个到达的 Battle 01 AI/导航分支，只闭合已接受通关所需的 fixture 缺口 |
| 动作构建与解决 | [战斗动作构建](../../contracts/battle-action-construction.md)、[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)、[随机性](../../contracts/randomness.md) | **合同就绪** 受限子集；**缺失研究所有者** 任何到达的不受支持分支；**显式产品决定** 确定性验收 | 选择验收种子/输入策略、按顺序记录到达动作，只为通关所需扩展所有者；不泛化子集测试夹具 |
| 战斗呈现 | [战斗演出呈现](../../contracts/battle-scene-presentation.md) 及其专用 graphics-data 合同 | **合同就绪** 命令/加载器/静态资源接缝；**缺失研究所有者**（若需要原版渲染保真）；**显式产品决定** 视觉/音频层 | 决定仅状态、结构、截图、动画与音频预期；只为所选保真层收集运行时呈现证据 |
| 胜利与战后 | [战斗控制与战斗员生命周期](../../contracts/battle-control-lifecycle.md)、[战斗过场路由](../../contracts/battle-cutscene-routing.md) | **合同就绪** 通用胜利顺序；**缺失研究所有者** 自然胜利、战后程序效果与最终路线；**缺失设计合同** 可观察完成 | 观察经普通控制器的胜利、战后 MAPSCRIPT 执行、返回路由与最终场景相关状态 |
| 存档/读取范围 | [存档系统](../../contracts/save-system.md)、[全局标志状态](../../contracts/global-flag-state.md)、名册/状态合同 | **合同就绪** 受限进程内服务/存储接缝；**缺失研究所有者**（若需要耐用性）；**显式产品决定** | 显式排除存档/读取，或选择检查点、进程内、挂起或耐用范围；若包含，证明每个使用场景字段在所选边界存活 |
| 端到端 H4 | 所有具名子系统测试夹具与合同 | **综合就绪** 用于台账；**缺失研究所有者** 连续原版轨迹；**缺失设计合同** 场景组合；**显式产品决定** 可观察验收 | 添加一个消费而非削弱子系统测试夹具的证据绑定连续场景合同，并分别记录已声明偏差 |

## 既有综合边界

以下 Layer B 文档已能解释局部片段，但不闭合本里程碑：

- [游戏总览](../../synthesis/gameplay-overview.md) 连接顶层动作与子系统交接，同时保留战役、UI、时序与呈现缺口。
- [故事推进](../../synthesis/story-progression.md) 连接受控 Map 3 进入、静态配置/事件/脚本图、对话/名册/状态接缝与存档交接，同时显式拒绝重构正常战役路线。
- [地图设计原则](../../synthesis/map-design-principles.md) 把地图结构与路线质量、节奏、可达性与作者意图分开。
- [战术战斗循环](../../synthesis/tactical-battle-loop.md) 组合局部战斗控制器、玩家/AI、导航、动作、解决、回放与结果所有者，同时拒绝完整战斗模拟与可见时序主张。
- [推进与经济](../../synthesis/progression-and-economy.md) 连接奖励/状态变更，但不建立 Battle 01 路线、平衡或完整持久性。

这些文档是**综合就绪**的台账输入。没有一份是所需的连续场景合同。

## 产品选择槽

以下条目都刻意未解决。它们需要后续用户接受决定（提议为 `docs/decisions/0010-map3-battle01-product-acceptance.md`），在就绪台账呈现已接受证据与可行选择之后。

| 决定槽 | 当前状态 | 决定必须说明 |
| --- | --- | --- |
| 准入起点 | **未决定** | 规范快照、可见 New 流程或读取；首个可观察状态与所需溯源 |
| 路线 | **未决定** | 强制地图、交互、对话、菜单、过渡，以及允许的可选/回溯行为 |
| 完成端点 | **未决定** | Battle 01 后精确成功观察；控制器返回本身不静默充分 |
| 存档/读取 | **未决定** | 排除、仅检查点、进程内、挂起战斗或耐用跨进程行为 |
| 玩家控制与 UI | **未决定** | 所需野外/战斗菜单、取消路径、设备映射、可访问性与本地化 |
| 资源 | **未决定** | 占位或适当许可替代、溯源、分发条款与私有输入分离 |
| 视觉/音频 parity | **未决定** | 状态/结构、截图、动画/帧、调色板、音频与时序验收层 |
| RNG 与动作轨迹 | **未决定** | 固定种子/输入轨迹、受限不变量集或其他可复现策略 |
| 有意偏差 | **未决定** | 每个允许的规则、安全、UI、时序、资源或呈现差异加 expected-deviation 覆盖 |
| 可选工具 | **未决定且不阻塞** | ADR 0008 bakeoff 后是否有可移除 MCP 适配器值得采用；CLI 门保持权威 |

本表中没有默认值由省略暗示。排除功能也需要已接受决定，证明剩余范围仍是连续可玩里程碑。

## 有序闭合计划

### 切片 0：持久就绪台账

本文档是其初始切片中唯一拥有的工件。它没有可执行 fixture 注册，也没有 research-index `designContracts` 关联。初步语义审查后，其唯一共享注册应是 `docs/README.md` 综合索引与 `manifests/zh-translation-index.json` 中的一个待处理条目。

### 切片 1：battle-functions 控制合同

下一个已接受证据设计候选是 `docs/design/contracts/battle-functions-control-flow.md`。它应只消费 `sf2-battle-functions-static-v1`，并关联本台账所列恰好 15 条 `battle.functions.*` 记录。它必须把运行时输入、完整取消、呈现、调用方效果与自然 Battle 01 可达性保持为独立或 **未知**。该候选未由当前切片启动或拥有。

### 切片 2：显式产品验收决定

后续决定必须解决产品选择槽，而不把它们改写为原版行为。设计可以准备替代方案，但需要用户验收。闭合选择不会启动 Phase 4。

### 切片 3：研究闭合

研究必须合并以下专用证据：

1. 精确准入的 Map 3 起始溯源与进入 Battle 01 的自然时间顺序路线；
2. 该路线上所选配置/事件/程序/对话/菜单/状态效果；
3. 自然 Battle 01 进入，包括所需的开战/开战前过场行为；
4. 一条经胜利的完整可玩多回合路径，识别每个到达的玩家、AI、导航、动作、解决、奖励与状态分支；
5. 战后程序效果、返回路由与精确可观察结束状态；
6. 呈现或持久性仅在已接受产品决定要求的范围内。

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
- battle-functions 与连续场景合同被接受；
- 所有路线所需条件所有者被接受；
- 产品决定解决每个槽或显式排除它；
- 可分发资源与私有输入边界闭合；
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
8. 所选存档、视觉、音频与资源断言；
9. 单独命名的预期偏差。

每层必须引用其所属已接受测试夹具。连续适配器不得替换子系统测试夹具、把其预期数字复制进引擎特定测试、在重制中要求原版 RAM/ROM 地址，或发布私有原版文本、图形、音频或捕获。

在 Phase 4 之前，就绪要求该验收面及其可执行检查定义完整并被接受，而非针对尚不存在的重制成功执行。构建重制适配器并获得 H4 PASS 结果是独立用户启动动作之后 Phase 4 实现与里程碑门。

## 公开与私有边界

公开就绪工件可以保留其所有者已允许的记录 ID、fixture ID、合同链接、计数、聚合元数据、已接受 hash、状态字段名、分支/顺序摘要、产品选择槽与合成 H4 轨迹形状。

以下保持私有，除非独立许可与分发审查接受：

- ROM、SRAM、存档状态、含版权载荷的轨迹与模拟器捕获；
- 完整提取的地图、对话、图形、音乐、声音、字体或过场内容；
- 原始源码派生资源载荷与私有规范导入图；
- 任何溯源或分发条款未被接受的替代资源。

Phase 4 应消费公开合同与项目自有测试夹具。私有不可变输入可支持本地验证，但不得成为受追踪重制依赖。

## 就绪检查清单

| 门 | 当前结果 | 闭合所有者 |
| --- | --- | --- |
| 精确里程碑与引擎基线已接受 | PASS | ADR 0008 / ADR 0009 |
| 准入 Map 3 起始状态精确 | 开放 | 研究，然后场景合同与产品决定 |
| 自然 Map 3 路线精确 | 开放 | 研究，然后场景合同 |
| 所需探索/对话/菜单/UI 范围精确 | 开放 | 研究加产品决定；条件合同 |
| 自然 Battle 01 准入精确 | 开放 | 研究，然后场景合同 |
| 玩家回合合同在场 | 开放 | `battle-functions-control-flow` 设计切片 |
| 完整可玩 Battle 01 轨迹精确 | 开放 | 研究加既有/扩展战斗合同 |
| 战后效果精确 | 开放 | 研究，然后场景合同 |
| 可观察端点已选择并有证据 | 开放 | 产品决定加场景合同 |
| 存档范围已选择并有证据 | 开放 | 产品决定；仅包含时研究 |
| 占位/许可资源已接受 | 开放 | 产品/许可决定 |
| 视觉/音频 parity 层已接受 | 开放 | 产品决定；仅原版保真处研究 |
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
| 15 条 battle-functions 记录有已接受静态证据但无设计合同 | **缺失设计合同** | `sf2-battle-functions-static-v1`、[battle-functions 研究](../../../research/battle-functions.md) | 无摄像机所有者重叠，无运行时/输入/呈现泛化 |
| 战后路线/程序身份存在 | **合同就绪 路线结构** | [战斗过场路由](../../contracts/battle-cutscene-routing.md) | 程序效果、自然可达性、持久性与可见顺序保持开放 |
| 局部战斗合同可在概念上组合 | **综合就绪** | [战术战斗循环](../../synthesis/tactical-battle-loop.md) 与链接合同 | 不是完整预测 Battle 01 模拟或场景黄金值 |
| Godot/C# 与里程碑已选择 | **已接受决定** | ADR 0008 / ADR 0009 | 无 Phase 4 启动、资源选择、MCP 采用或产品验收画像 |
| 路线、端点、存档、UI、资源、RNG、parity 与偏差 | **显式产品决定** | 未来用户接受 ADR | 不得从原版源码标签或沉默推断 |
