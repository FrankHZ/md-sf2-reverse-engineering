# 地图与探索合同

- **已确认的原版行为：** 79 个地图定义、共享图块/布局所有权、源码形式的区域/事件/物品/动画、64x64 解码布局、配置选择、有记录的首匹配分发规则、静态过渡使用方优先级、按加载路径的布局持久性、源码形状的地图脚本实体填充/重载、cloneEntity、摄像机控制与实体放置命令记录、有界静态地图事件状态/控制/调用方/内容形状，以及分批的帧级实体移动/动作时序
- **未知的原版行为：** 非空地图 52 直接 `rts` 事件配置的正常剧情可达性、精确的 VDP 可见滚动时序、硬件级动画扫描线时序，以及最终的 VDP 可见渲染一致性
- 重制状态：实现无关的 Phase 3 合同；实现方向与能力状态由 [ADR 0019](../../../decisions/0019-state-and-content-driven-remake-engine.md) 和[重制能力台账](../../../../remake/docs/capability-status.md) 所有。

> 本文件是 [`map-exploration.md`](../../contracts/map-exploration.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

重制地图导入器必须从研究拥有的结构化输出构造 `MapDefinition`，绝不能从渲染器或场景代码中内嵌的假设构造。每个定义有以下独立拥有的引用：

1. 调色板与五个瓦片集槽位；
2. 一个图块集与一个 64x64 布局；
3. 有序区域记录；
4. 有序标志、踩踏、屋顶与传送事件记录；
5. 有序宝箱与其他物品记录；
6. 一个可选动画表；
7. 一个单独选择的六指针地图配置定义，通过有序的逐地图默认/标志路由解析。

引用是同一性，而不是隐式复制。地图 24 与 46 复用地图 23 与 7 的图块集/布局同一性。除非显式重制变换请求复制，导入器必须保留该共享。由 `$FFFFFFFF` 表示的可选原始指针变成缺失值，而不是虚构的空表。

证据可通过以下方式执行：

- `sf2-map-content-static-v1`，位于
  `tests/fixtures/h2/map-content-static-v1.json`；
- `sf2-map-layout-decode-v1`，位于
  `tests/fixtures/h2/map-layout-decode-v1.json`；
- `sf2-canonical-map-import-v1`，位于
  `tests/fixtures/h2/canonical-map-import-v1.json`；
- `sf2-map-events-static-v1`，位于
  `tests/fixtures/h2/map-events-static-v1.json`；
- `sf2-map-event-direct-state-static-v1`，位于
  `tests/fixtures/h2/map-event-direct-state-static-v1.json`；
- `sf2-map-event-direct-control-static-v1`，位于
  `tests/fixtures/h2/map-event-direct-control-static-v1.json`；
- `sf2-map-event-direct-handoff-static-v1`，位于
  `tests/fixtures/h2/map-event-direct-handoff-static-v1.json`；
- `sf2-map-event-predicate-results-static-v1`，位于
  `tests/fixtures/h2/map-event-predicate-results-static-v1.json`；
- `sf2-map-event-dialogue-state-static-v1`，位于
  `tests/fixtures/h2/map-event-dialogue-state-static-v1.json`；
- `sf2-map-event-request-state-static-v1`，位于
  `tests/fixtures/h2/map-event-request-state-static-v1.json`；
- `sf2-map-event-request-consumption-static-v1`，位于
  `tests/fixtures/h2/map-event-request-consumption-static-v1.json`；
- `sf2-map-event-interaction-state-static-v1`，位于
  `tests/fixtures/h2/map-event-interaction-state-static-v1.json`；
- `sf2-map-event-item-transactions-static-v1`，位于
  `tests/fixtures/h2/map-event-item-transactions-static-v1.json`；
- `sf2-map-event-random-battle-state-static-v1`，位于
  `tests/fixtures/h2/map-event-random-battle-state-static-v1.json`；
- `sf2-map-event-combatant-state-static-v1`，位于
  `tests/fixtures/h2/map-event-combatant-state-static-v1.json`；
- `sf2-map-event-tactical-base-quote-state-static-v1`，位于
  `tests/fixtures/h2/map-event-tactical-base-quote-state-static-v1.json`；
- `sf2-map-event-scripted-transition-state-static-v1`，位于
  `tests/fixtures/h2/map-event-scripted-transition-state-static-v1.json`；
- `sf2-map-event-flag-lifecycle-state-static-v1`，位于
  `tests/fixtures/h2/map-event-flag-lifecycle-state-static-v1.json`；
- `sf2-map-event-cross-program-flag-state-static-v1`，位于
  `tests/fixtures/h2/map-event-cross-program-flag-state-static-v1.json`；
- `sf2-map-event-flag-route-selection-static-v1`，位于
  `tests/fixtures/h2/map-event-flag-route-selection-static-v1.json`；
- `sf2-map-init-static-v1`，位于
  `tests/fixtures/h2/map-init-static-v1.json`；
- `sf2-map-script-engine-static-v1`，位于
  `tests/fixtures/h2/map-script-engine-static-v1.json`；
- `sf2-map-setup-selection-runtime-v1`，位于
  `tests/fixtures/h3/map-setup-selection-v1.json`；
- `sf2-map-init-dispatch-runtime-v1`，位于
  `tests/fixtures/h3/map-init-dispatch-v1.json`；
- `sf2-map-event-dispatch-runtime-v1`，位于
  `tests/fixtures/h3/map-event-dispatch-v1.json`；
- `sf2-map-animation-vdp-runtime-v1`，位于
  `tests/fixtures/h3/map-animation-vdp-v1.json`；
- `sf2-entity-movement-runtime-v1`，位于
  `tests/fixtures/h3/entity-movement-matrix-v1.json`；
- `sf2-map-lifecycle-runtime-v1`，位于
  `tests/fixtures/h3/map-lifecycle-v1.json`；
- `sf2-map-script-control-audio-runtime-v1`，位于
  `tests/fixtures/h3/map-script-control-audio-v1.json`；
- `sf2-map-script-transition-runtime-v1`，位于
  `tests/fixtures/h3/map-script-transition-v1.json`；
- `sf2-map-interaction-trigger-runtime-v1`，位于
  `tests/fixtures/h3/map-interaction-trigger-v1.json`；
- `sf2-map-script-ui-primary-runtime-v1`，位于
  `tests/fixtures/h3/map-script-ui-primary-v1.json`；
- `sf2-map-script-entity-presentation-fx-runtime-v1`，位于
  `tests/fixtures/h3/map-script-entity-presentation-fx-v1.json`；
- `sf2-map-script-entity-clone-runtime-v1`，位于
  `tests/fixtures/h3/map-script-entity-clone-v1.json`；
- `sf2-map-script-screen-presentation-runtime-v1`，位于
  `tests/fixtures/h3/map-script-screen-presentation-v1.json`；
- `sf2-map-entity-lifecycle-presentation-runtime-v1`，位于
  `tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json`。

### 静态地图事件证据边界

以上新增的十六个 map-event H2 fixture 是字段闭合的静态证据所有者。本合同只消费其中
以下源码/H1/ROM 形状：

| Fixture | 消费的静态形状 |
| --- | --- |
| `sf2-map-event-direct-state-static-v1` | 直接固定 RAM 读写同一性、宽度、操作数顺序、上下文/物理计数与溯源 |
| `sf2-map-event-direct-control-static-v1` | 调用方侧直接调用/尾传递同一性、别名、词法延续与溯源 |
| `sf2-map-event-direct-handoff-static-v1` | 相邻调用方准备形状与返回后的第一个词法延续形状 |
| `sf2-map-event-predicate-results-static-v1` | 源码形状的生产者/测试/分支对、分支极性、目标与 fallthrough |
| `sf2-map-event-dialogue-state-static-v1` | 有界对话状态访问、数字文本/哨兵同一性、局部控制形状与 reaching-definition 同一性 |
| `sf2-map-event-request-state-static-v1` | 请求状态写入类别，以及有界 handoff/return 接缝处的调用方局部 reaching definitions |
| `sf2-map-event-request-consumption-static-v1` | 使用方侧固定状态访问、局部分支与直接 handoff 拓扑 |
| `sf2-map-event-interaction-state-static-v1` | 有界 `ENTITY_FACING`/`EVENT_RELATIVE_POSITION` 生产者、使用方、谓词与接缝 |
| `sf2-map-event-item-transactions-static-v1` | 调用方侧服务顺序、谓词形状、源码常量与 FieldMenu/`d6` 返回接缝 |
| `sf2-map-event-random-battle-state-static-v1` | `CheckRandomBattle` 周围的静态调用方清单与源码有序局部请求/控制形状 |
| `sf2-map-event-combatant-state-static-v1` | 调用方侧战斗员 getter/setter 顺序、选择器与局部谓词形状 |
| `sf2-map-event-tactical-base-quote-state-static-v1` | 静态调用方/被调用方分支与服务形状，以及不含解码文本的数字引文行 ID 域 |
| `sf2-map-event-scripted-transition-state-static-v1` | 一个有界程序流的命令/载荷嵌套、处理器连接、指针目标与终止形状 |
| `sf2-map-event-flag-lifecycle-state-static-v1` | `sourceContext` 溯源/分母及 `flagLifecycleState.selectionSummary`、`sourceFiles`、`programFlows`、`lifecycleRelations`、`flagTotals`、`intervalCoverage`、`digests`，以及顶层聚合 `summary` |
| `sf2-map-event-cross-program-flag-state-static-v1` | `sourceContext` 溯源/分母及 `crossProgramFlagState.programDomain`、读写访问点与 cohorts、`partitions`、`crossProgramCandidates`、`categoryPairTotals`、`physicalContextCoverage`、`digests`，以及顶层聚合 `summary` |
| `sf2-map-event-flag-route-selection-static-v1` | `sourceContext` 溯源/分母及 `flagRouteSelection.programRouteContexts`、`classifiedCandidates`、`topologyCategoryTotals`、`selectorWriterRelations`、`domainDenominators`、`physicalCoverage`、`digests`，以及顶层聚合 `summary` |

对于三个 flag fixture，`retainedOwners` 完整性哈希以及 `serviceDefinitions`、
`dispatchEntries`、`sourceMacroDefinitions`、`categoryRoles`、`serviceJoin`、
`retainedIdentities` 中的服务/分发器/表同一性连接仍只属于跨所有者连接。它们不会把通用 flag
服务、trap、分发器、selector 表或 map-setup 数据保真所有权转移给本合同。

这些所有者增加的是可追溯深度，而不是已观察执行。它们不确立程序的自然可达性；实际采取的
路径或分支；调用方入口或读取时 flag 值；变更可达性或变更后的值；生产者/使用方或
跨程序时间顺序；中间变更；实际 map-setup selector 求值或选中的 pointer table/event
record；被调用方成功或效果；状态生命周期或存档/读档持久性；对话或剧情含义；输入/音频/呈现/
时序；地图或战斗完成；R4b/H4/8C 一致性；或任何其他未观察的运行时行为。源码名称、数字文本
ID、调用、写入与分支标签仍是结构同一性。完整源码/H1/ROM 材料与私有生成连接仍是验证输入；
公共合同只保留已接受 fixture 已公开的有界结构记录、计数、哈希与溯源。

规范导入 fixture 是本合同的可执行序列化。其完整生成载荷在 `local/derived/` 下保持私有；只有聚合结构与溯源受追踪。

规范导入现在解析全部 64 条配置路由与 126 个配置定义。一个配置定义引用六个独立共享的资源：实体、实体事件、区域（事件）、区域描述、物品事件与初始化函数。没有原始路由的 15 个地图 ID 保持空路由。选择按源码顺序扫描每个标志变体并保留最后设置的标志；直接返回处理器保持为显式空处理器，而不是被猜测的默认值替换。十案例 H3 矩阵确认选择器本身对缺失地图、默认路由、单个与多个已设置标志、最后设置标志胜出，以及恢复默认指针的后续别名返回 H2 建模的指针。它重放一次自然的调试 Map Test 提示，并只在选择器入口更改 `CURRENT_MAP` 与游戏标志位集；原始扫描与返回原样执行。六案例 init-dispatch 矩阵单独确认缺失地图跳过间接 init 调用，而五个默认/标志选择的配置每个都恰好一次调用其 H2 建模的 init 目标并通过原始包装器返回。它覆盖活动、脚本化与直接返回目标，而不声称合成的地图/标志组合复现其剧情副作用。初始化资源保留 130 条有序选择器路由连接，穿过 126 张 setup 表到 90 个目标概要。每个概要保留其物理源码操作边界、有序操作索引、精确标志操作数、直接指令/有效调用同一性，以及脚本目标解析；597 次物理操作、973 次指针表加权出现与 1,100 次路由加权出现是不可互换的计数。解析后的分发器记录保留链接 enum、指针布局行与符号化加载使用位置证据，用于字节偏移 20（第六个四字节槽位）处的 init 函数指针，加上缺失配置的比较与分支、间接调用、恢复与返回顺序。重制导入器可以将已识别的源码形式转换为类型化命令 IR，但必须保留未知操作数文本，且不得仅从操作码或目标名称推断脚本持久性、实体可见性或帧时序。独立脚本资源同样保留全部 178 个标签之间的有序命令、操作数文本与已解析引用。这些是可导入的命令图，不是现代引擎可以跳过原版解释器的状态、等待、摄像机、对话或呈现时序的证明。

**已确认的源码合同：** 保留 地图脚本命令图的导入器必须保留 `sf2-map-script-engine-static-v1`、`tests/fixtures/h2/map-script-engine-static-v1.json` 字段 `expected.scriptControlCommandFacts` 中源码忠实的控制/音频形式：`csWait`、`playSound`、`csc06`、`executeSubroutine`、`jump`、零字节源码形式 `cscNop` 与两字节 `$FFFF` 的 `csc_end` 形式。`playSound` 保留其原始源码 enum 操作数及其维护的声音数据同一性；绝不能仅仅根据其源码标签被规范化成可听结果、音乐、效果或时序字段。

**已确认的源码合同：** 导入的解释器模型必须保持静态 A6 游标边界与编码字节存储不同。命名的分发循环读取一个字，将带符号 `-1` 与 `csc_end` 字比较，有一个经过 `BYTE_MASK`/`Sleep` 序列的源码负分支，并在表分发之前将非负选择器加倍。`executeSubroutine` 在其保存/调用/恢复序列之前以 post-increment 传递一个四字节目标；`jump` 将一个四字节目标传递进 A6，游标推进为零。这些源码控制流事实需要保真测试，但它们不确立计时器单位、声音播放、剧情可达性、持久性或玩家可见呈现。

**已确认的运行时边界：** `sf2-map-script-control-audio-runtime-v1` 保留六次单启动解释器观察：D0=1 与一次 `WaitForVInt` 入口对比调试 P2-START 跳过、`$06` 分发/返回、`$05` 原始声音字与 trap 边界、`$0A` 游标/栈/被调用方返回边界、`$0B` 游标替换，以及 `$FFFF` 结束/返回。其仅会话的入口接缝在观察器记录完成之前保留源码包装器尾声；它是 H3 测试台边界，而不是游戏玩法合同。在共享物理回调 PC 处，观察器在保留一次注册的同时按精确 fixture 用例角色选择失败诊断；这同样不是适配器行为。原版保真适配器必须保留这些控制与游标事实，而不把源码 enum 或观察到的 trap 转换成可听、时序、持久性或玩家可见合同。

**未知：** [`common-scripting.md`](../../../research/common-scripting.md#confirmed-map-script-controlaudio-macro-boundary) 中分组的 `map-script-control-audio/*` 队列保留更长的时长时序、声音驱动/可听结果、任意被调用方效果、正常剧情可达性与呈现行为。

直接地图事件 `txt` 与无操作数的 `clsTxt` 形式同样是源码同一性，而不是显示合同。导入器必须把每个有序位置保留为数字文本行标识符或字面量 `$FFFF` 哨兵源码形式，加上其调用方与独立命名的物理/setup/route 引用权重。它必须保留完整的 0–4,266 已声明 ID 域，包括零引用 ID，而不把原版文本复制或解码进地图合同。这不规定显示内容、说话者、窗口、等待、输入、剧情推进或呈现行为；它们保持在这个静态导入规则之外。

导入器还必须把五个源码命名的 地图脚本过渡形式 `warp`、`resetMap`、`loadMapFadeIn`、`reloadMap` 与 `mapLoad` 保留为有序命令记录，而不是用猜测的场景操作替换它们。每条记录必须保留其物理操作数宽度、存在时的原始目标地图操作数文本、已解析的已声明地图 ID 或独立的 `MAP_CURRENT` 哨兵，以及源码命令/程序同一性。命名的处理器边界必须保留源码确认的 A6 游标读取、直接服务调用同一性/顺序，以及 `csc37_loadMapAndFadeIn` 落入 `csc48_loadMap` 的 fall-through；它必须把解析后的地图事件值、D1 立即数与压缩坐标乘数保留为源码事实。重制只有在定义了自己的行为之后，才可以把这些形式映射到引擎特定的过渡 IR；原版源码合同不确立事件消耗、摄像机状态、淡入淡出时序、显示时序或玩家可见过渡结果。

**已确认的有界运行时边界：** `sf2-map-script-transition-runtime-v1`（位于 `tests/fixtures/h3/map-script-transition-v1.json`）通过原版解释器增加一次五案例启动。原版保真适配器必须把观察到的操作码/A6/处理器返回顺序、csc37 到 csc48 的 fall-through、直接服务接缝顺序、csc07 事件字节，以及显式播种的 map/view-target/plane-A 状态事实保留为独立的适配器数据。源码在 csc37 入口写入 `OUT_TO_BLACK` 值 2，而这个有界运行在 `LoadMapTilesets` 之后第一次 csc48 `WaitForVInt` 入口处把 `FADING_SETTING` 读为 0；这两个事实都不定义淡入淡出时长或可见结果。适配器不得把这些合成 Map Test 结果变成正常剧情可达性、持久性、碰撞/寻路、摄像机呈现或显示行为。

地图脚本导入必须分别保留 `sf2-map-script-engine-static-v1`（位于 `tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapCameraControlCommandFacts` 中源码忠实的 `setCameraEntity`、`setCamDest` 与 `cameraSpeed` 记录。每条记录必须保留其源码操作码、物理操作数宽度、原始宏注释、程序/命令排序与含零的程序总数域。导入边界还必须保留精确的静态处理器记录：csc24 的推进字读取、两个分支极性与解析后的目标语句、解析后的常量与源码命名的写入；csc32 的字面量状态写入、两个推进字、别名调用后接等待调用与返回；以及 csc45 的推进源码字写入与返回。直接指令目标与已解析的有效目标必须保持独立同一性，包括逐处理器零计数行，而 `SetCameraDestination` 中两个解析后的 `MAP_TILE_SIZE` 使用位置必须保持独立的源码记录。重制可以独立定义引擎特定的摄像机接口；这个静态合同既不确立目标/目的地含义、坐标单位、速度效果、时序、可达性，也不确立呈现。

有界 H3 记录 `sf2-map-camera-control-runtime-v1`（位于 `tests/fixtures/h3/map-camera-control-v1.json`）增加七个单启动命令观察，而不把源码标签变成呈现设计：它保留负/己方角色/敌人目标分支记录、目的地输入字到传递字的值、两个速度字值、direct/service/wait 回调顺序与处理器返回边界。原版保真适配器必须把这些测得的状态与调用顺序事实保持独立于正常剧情可达性与 VDP/玩家可见行为，后者保持 **未知**。

地图脚本导入必须分别保留 `sf2-map-script-engine-static-v1`（位于 `tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPlacementCommandFacts` 中源码命名的 `setPos`、`setPosFlash`、`setFacing` 与 `setDest` 记录。每条记录必须保留其操作码、编码/操作数字节宽度、原始宏注释、程序/命令同一性与紧凑完整源码顺序/哈希边界。导入边界还必须保留精确的命名处理器记录：`csc19`/`csc23` 的非推进选择器读取加上存活状态游标调整调用与推进读取；`csc17` 的局部分支目标与 `csc19` 共享尾部边；`csc29` 的三个局部分支目标；解析后的 `MAP_TILE_SIZE` 乘数使用位置；源码形状的状态读/写操作数；以及含零的直接/有效调用方映射。这些源码记录不得被规范化成放置、朝向、移动、可见性、动画、坐标单位、碰撞、持久性、时序或渲染模型。有界 H3 合同 `sf2-map-script-entity-placement-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-placement-v1.json`）记录七个单启动用例：`setPos` 与 `setFacing` 的存活/死亡当前-HP 游标结果、源码缩放的实体记录字/朝向、完整的 31 次迭代局部 flash 回调序列及其独立的共享尾部回调，以及两个带符号目的地增量极性与 bit-15 等待/旁路。原版保真适配器必须保留这些测得的 RAM/游标/回调事实，而不把它们提升为呈现或地图移动设计。正常剧情可达性、完整动画/可见性/呈现与碰撞/寻路/持久性保持 **未知**。重制可以独立定义自己的实体状态接口。

[Map21 查找澄清](../../../research/map3-castle-battle-unlock.md#map-21-entity-135-unchecked-lookup-and-conditional-continuation)
增加的是**已确认**的条件性源码边界，并非新的自然路线运行时证据。
不得把 `GetEntityAddressFromCharacter` 描述为缺失实体搜索或缺席时跳过的服务：它对选择器字应用掩码，
把其带符号字节编码转换为同一性表偏移，读取一个字节，再用五位的字移位推导记录地址。
表字节为零时选择物理 slot0；`0xFF` 不被拒绝，并产生常规已清除实体记录之外的地址。
这些不同状态不得折叠为通用的实体缺失空操作。
在 Map21 的默认填充下，若中间没有表写入，alias135 未分配的表字节仍为零；
`setFacing 135,DOWN` 写入 slot0 的朝向并调用精灵服务。其非己方角色选择器绕过死亡己方角色的 HP 跳过逻辑。
F401 仍取决于此前动作等待/朝向处理完成，周围的 F256 写入仍取决于 F608-set/F256-clear
处理器分支和整个脚本返回。任何导入器或参考驱动都不得跳过朝向命令或强制设置这些标志，
再把该捷径报告为原版证据。

Map3 隐藏后的 alias142 属于另一种生命周期状态：`HideEntity` 把匹配映射替换为 `0xFF`，
F603 的 `MoveEntityOutOfMap` 链也有其自身不做检查的表/位置使用方。
重制可以为这些原版内存效果定义安全表示或显式不支持边界；它必须区分该政策与原版行为，
并且不得为已移除的142推断回退到玩家槽位。本澄清不要求引擎采用原始内存访问，
不关闭自然调用状态或呈现方面的未知问题，也不准入另一项运行时场景。

对于 [F603 后的重载边界](../../../research/map3-castle-battle-unlock.md#post-f603-reload-the-writes-target-inactive-window-composition-storage)，
四次 `0x7000` 存储的目标是合并的 Plane-A/窗口布局缓冲区，而非 entity255 或仅地图布局。
**已确认的条件性静态结论：** 正常重载初始化一个空窗口列表；其 VInt 使用方在读取该缓冲区前就返回，
首次窗口分配则在发布活动窗口前把整个仅地图布局复制到其中。
在正常队列处理且没有待处理合并缓冲区 DMA 的条件下，可将其抽象为对非活动呈现暂存区的变更，
返回输入时不再增加地图/实体/摄像机/控制过渡。该抽象必须保留这些前提及此前真实的 `hide142` 效果；
不得变成通用的实体缺失空操作、玩家槽位回退或对自然观察帧的断言。
活动窗口、延迟 DMA 和其他指针/复用情况仍在这个有界结论之外。

地图脚本导入必须分别保留六个源码命名的桥接形式 `setActscriptWait`、`setActscript`、`customActscriptWait`、`customActscript`、`entityActionsWait` 与 `entityActions`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityActionBridgeCommandFacts`。每条记录必须保留其源码操作码、编码与操作数字节宽度、第一个源码选择器字段、精确的 `$FF` 或零源码控制字段、程序/命令排序与紧凑全语料顺序/哈希边界。内联载荷必须保留其源码形式类别、有序命令字节、源码终止符拼写、终止符字节计数，以及每个分别命名的 primary/payload/terminator 游标推进；导入器不得折叠这些物理量或用语义动作序列替换它们。`customActscript*` 记录必须保留单独的 csc14 两字节扫描传递与编码字节派生的、字对齐的扫描迭代次数；这些源码事实与 csc2D 的两字节解释命令读取不同。精确的 csc14/csc15/csc2D 处理器守卫，包括 csc2D 末端块、已解析的尾部传递目标、分支/调用顺序、源码常量使用位置与含零的直接/有效调用方同一性，仍属于导入边界。到地图事件与 entity-action fixtures 的连接只是溯源记录。有界 H3 合同 `sf2-map-entity-action-bridge-runtime-v1`（位于 `tests/fixtures/h3/map-entity-action-bridge-v1.json`）在一次会话中记录全部六个别名：精确的处理器/回调 PC、源码形状的实体字段与游标结果、csc14 内联终止符钩子、csc2D 索引目标与末端入口，以及在其空闲载荷写入之后紧跟解析后 PC 处的精确 csc2D 缓冲区记录。该快照是一个写入时记录，而其全局缓冲区指针与实体指针字段是处理器后观察；原版保真适配器必须保留那个观察边界，而不是把快照当作持久动作状态。重制可以定义自己的脚本/动作 IR，但正常剧情可达性、完整动作/运动/碰撞效果、自然时序、持久性与呈现保持 **未知**。

地图脚本导入必须分别保留八个源码命名的形式 `hide`、`startEntity`、`stopEntity`、`waitIdle`、`setSprite`、`setPriority`、`removeShadow` 与 `setSize`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityLifecyclePresentationCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释、完整程序/命令顺序与含零的程序域。导入边界必须保留精确的命名处理器指令顺序：推进对非推进 A6 读取、存活状态指针调整字面量/调用、源码形状的字段操作数、解析后的 `COMBATANT_ALLIES_NUMBER` 与 `%1000` 使用位置、分支极性/目标同一性、直接指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成可见性、动画、精灵、优先级、阴影、尺寸、碰撞、持久性、时序或呈现模型。有界 H3 fixture `sf2-map-entity-lifecycle-presentation-runtime-v1`（位于 `tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json`）额外保留 11 条精确的 Map Test 0 记录：回调顺序、存活对零当前-HP 的 start/stop 游标边界、受控的第二次比较空闲接缝、两个精灵选择器侧、优先级字节、remove-shadow 回调链，以及源码支持的临时/恢复尺寸字与 flags-B 状态。适配器必须保留那个观察边界，而不把它当作玩家可见或持久模型。剩余的原版问题是精确的 `map-script-entity-lifecycle-presentation/normal-story-reachability`、`map-script-entity-lifecycle-presentation/full-entity-state-callback-effects` 与 `map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence`。

地图脚本导入必须分别保留七个源码命名的形式 `shiver`、`nod`、`followEntity`、`faceEntity`、`moveNextToPlayer`、`fly` 与 `moveEntityAboveAnother`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityGestureRelationshipMotionCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释（包括两条空的 `moveEntityAboveAnother` 注释）、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递对非推进探测宽度、源码操作数/字面量使用位置、分支极性与目标同一性、循环目标记录、直接指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成手势、关系、位置、跟随、移动、图层、朝向、动画、时序、碰撞、持久性或呈现模型。有界 H3 fixture `sf2-map-entity-gesture-relationship-motion-runtime-v1`（位于 `tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json`）保留 17 条精确的 Map Test 0 记录：全部七个处理器入口、直接/有效回调计划与观察到的回调顺序、源码局部的 shiver/nod/fly 写入接缝、非推进 follow HP 字节探测、face/move 字边界，以及 `moveEntityAboveAnother` 寄存器记录。适配器必须保留那个受控观察边界，而不把它当作玩家可见、碰撞、时序或持久模型。剩余的原版问题正是 `map-script-entity-gesture-relationship-motion/normal-story-reachability`、`map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects` 与 `map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence`。

地图脚本导入必须分别保留十二个源码命名的形式 `setQuake`、`fadeInB`、`fadeOutB`、`slowFadeInB`、`slowFadeOutB`、`tintMap`、`flickerOnce`、`mapFadeOutToWhite`、`mapFadeInFromWhite`、`flashScreenWhite`、`fadeInFromBlackHalf` 与 `fadeOutToBlackHalf`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.screenPresentationCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、源码立即数与存储操作数记录、分支极性/目标同一性、循环目标记录、指令目标加 PC 相对/直接寻址形式、有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成屏幕效果、地图效果、视觉、调色板、VDP、时序、持久性或可达性模型；所有原版运行时后果都刻意保留在 H2 导入之外。有界 H3 fixture `sf2-map-script-screen-presentation-runtime-v1`（位于 `tests/fixtures/h3/map-script-screen-presentation-v1.json`）保留全部十二个处理器入口、源码派生的 quake 与 flash 操作数分区、直接目标/调用/返回时间线、A6/栈边界、直接处理器局部 RAM 写入与源码设置的调用寄存器字。适配器必须保留那个接缝，而不把其服务入口垫片当作原版服务行为或视觉/调色板、VDP、时序、持久性、可达性或地图/实体模型。剩余的原版问题正是 `docs/research/common-scripting.md` 中四个精确的 `map-script-screen-presentation/*` 队列。

地图脚本导入必须分别保留三个源码命名的形式 `animEntityFX`、`headshake` 与 `entityFlashWhite`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPresentationFxCommandFacts`。每条记录必须保留操作码、物理操作数宽度、直接对 `ENTITY_TRANSITION_` 简写编码、原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、立即数/源码操作数记录、单独标记的 `loc_46BE2` 分支块目标、循环目标记录、指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。有界单启动 H3 fixture `sf2-map-script-entity-presentation-fx-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-presentation-fx-v1.json`）额外要求源码观察到的过渡选择器 2–7、flash 时长边界 10/57/180、全部三个处理器入口 PC、A6 游标边界、H1 返回 PC、局部分支/循环计数、紧凑的精确调用位置/目标/返回时间线，以及两个直接实体字节写入接缝。重制不得把其中任何记录规范化成实体效果、头部运动、颜色变化、过渡含义、视觉、时序、持久性或可达性模型。剩余的原版问题正是 `docs/research/common-scripting.md` 中四个分组的 `map-script-entity-presentation-fx/*` 队列。

地图脚本导入必须分别保留三个源码命名的 primary 形式 `showPortrait`、`hidePortrait` 与 `menu`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapScriptUiPrimaryCommandFacts`。每条记录必须保留操作码、物理操作数宽度、包括 `menu` 空注释在内的原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、源码立即数/操作数记录、分支目标、源码栈指针传递记录、带别名的指令/有效目标调用方映射、返回边界，以及到 `dialogueCommandFacts.portraitHelper` 的溯源连接。有界运行时 fixture `sf2-map-script-ui-primary-runtime-v1`（位于 `tests/fixtures/h3/map-script-ui-primary-v1.json`）额外保留十一条处理器局部记录：四个精确的 H2 源码行输入、busy 字直接返回边界、一个受控的 `d1=$FFFF` 比较分支、hide 的直接调用时间线，以及选择器 `0`/`1`/`2`/其他 menu 分区与 A6/栈恢复。其解析后的指令/有效同一性仍是溯源，而其实际调用位置、垫片目标角色/PC 与回调返回 PC 是有界的控制流观察。仅会话的 ROM 观察那些 PC 并只替换解析后的垫片入口区间；别名从其指令目标垫片返回，此时其解析后的有效目标尚未执行。服务效果未被观察。重制不得把该 fixture 当作立绘绘制、菜单/输入行为、用户选择、时序、持久性、存档行为或可达性证据。这些原版问题在 `docs/research/common-scripting.md` 的四个 `map-script-ui-command/*` 队列下显式保持未知。

共享解释器合同在 234 字节上定义 82 个 primary 命令布局，含 133 个有序操作数字段。导入器必须保留每个字段的字节宽度与流偏移，包括简写编码字，并且必须显式表示顺序、绝对跳转、条件绝对跳转与内联动作程序游标结果，而不是把每条命令压平成一个线性列表。

原版语料包含 304 个这样的程序与 348 个标签。导入验证必须把每条受追踪命令分配到恰好一个程序，保留 303 个 `csc_end` 与一个绝对跳转终止形状，并解析所有同程序/跨程序脚本目标。汇编子程序调用保持显式的外部边；现代导入器不得仅仅根据其符号名内联或重新解释它们。

导入器应当把引用状态与地址状态分开保留：297 个程序有一个传入源码引用，七个没有，八个缺少 H1 入口地址。引用对可达性规划有用，但不得被提升为证明正常存档状态路线会执行该程序。

剧情状态导入必须把七个源码形式 `jumpIfFlagSet`、`jumpIfFlagClear`、primary `csc10`、`setF`、`clearF`、`yesNo` 与 `setStoryFlag` 保留为独立的源码同一性。primary `csc10` 载体及其 `setF`/`clearF` 别名必须保留各自单独的物理字布局；别名不会仅仅因为 primary 形式当前源码位置计数为零而抹掉它。导入器必须保留条件标志极性、直接设置/清除操作、yes/no 结果到 flag-89 的映射、战斗解锁转换 `flag = 400 + battleIndex`、分支目标/游标形状，以及直接对已解析服务目标同一性。有界十案例 H3 fixture 额外保留每个处理器局部的 A6/调用时间线与最终 GAME_FLAGS 位，覆盖两种条件极性、两个别名、两种 yes/no 结果与战斗解锁 base/wrap 输入。这些是命令图与会话局部变更事实；全局剧情排序、存档/读档持久性与玩家可见提示呈现/时序保持在导入器合同之外。

地图脚本导入必须把 `setBlocks` 与 `setBlocksVar` 保留为两个独立的源码命令形式，而不能以猜测的地图编辑操作替换。每条记录必须保留其两字节操作码、六个源码标记的单字节字段（`source x`、`source y`、`width`、`height`、`destination x`、`destination y`）、源码程序/命令顺序，以及三次配对的 A6 字读取进入 `d0`、`d1` 与 `d2`。静态适配器边界必须保留精确的直接 `CopyMapBlocks` 调用与只在 `csc34_setBlocks` 之后出现的源码命名的置位序列；它必须分别保留 helper 的解析后 8 位移位、6 位行移位、2 字节内部偏移、128 字节外部偏移与循环计数器指令。这些是源码布局与指令顺序事实。**已确认（H3）：** 有界运行时合同 `sf2-map-block-mutation-runtime-v1`（位于 `tests/fixtures/h3/map-block-mutation-v1.json`）额外要求两种形式的精确前向 FF0000-layout 字复制时间线/回读、一个跨行矩形，以及两个水平/垂直重叠方向；它还要求 `$34` 观察到的复制后切换位顺序与 `$35` 缺失的切换回调。重制必须把碰撞/寻路使用方效果、正常剧情可达性与地图重载/存档持久性，以及可见 VDP 呈现/周期像素时序保留在本合同之外，直到三个显式 `map-block-mutation/*` 未知问题被分别观察。

源码命名的实体动作 `ac_checkMapBlockCopy` 与 `setBlocks`/`setBlocksVar` 保持为独立的生命周期边界。**已确认（H3）：** `sf2-map-block-copy-lifecycle-runtime-v1` 位于 `tests/fixtures/h3/map-block-copy-lifecycle-v1.json`，并保留 `$40` 分发器的 fade 跳过、掩码后的 `$0800` show 与 `$0C00` hide 选择、动作游标推进，以及原始 helper 调用/目标/返回边界。这个有界合同只记录 busy、保存矩形元数据/缓冲区/哨兵和选定 FF0000 布局字的正源复制、负源清除与 active restore；busy 是匹配 roof 行的一基扫描序号而不是布尔值，保存缓冲区哨兵位于其矩形之后。它不能推断碰撞、导航、正常剧情可达性、持久化/重载或 VDP/fade/audio/时序。它们仍是分组的 `map-block-copy-lifecycle/*` 未知问题。研究 harness 的 source/H1/ROM guard 和仅对选定测试写入进行的范围恢复属于证据控制，不是重制状态要求。

地图脚本导入必须把四个源码命名的形式 `newEntity`、`loadMapEntities`、`reloadEntities` 与 `loadEntitiesFromMapSetup` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPopulationCommandFacts` 中独立的有序命令记录。每条记录必须保留其物理操作码/操作数宽度、源码注释（包括一条刻意的空注释）、源码程序/命令同一性，以及精确的处理器游标读取、VInt、直接调用与源码常量记录。直接指令目标与已解析的有效目标同一性必须保持不同：特别是 `j_InitializeMapEntities` 不是 `InitializeMapEntities` 有效目标的替代拼写。重制只有在独立定义该接口之后，才可以把这些记录转换成自己的实体加载接口。**已确认（H3）：** 有界运行时合同 `sf2-entity-population-reload-runtime-v1`（位于 `tests/fixtures/h3/entity-population-reload-v1.json`）要求来自一次 BizHawk 启动的全部 12 条精确有序记录：三个 `newEntity` 同一性列表高水位种子、一次直接表加载、一次经同一性列表选择记录的 reload，以及全部七条 `loadEntitiesFromMapSetup` 源码输入行。每次观察保留处理器同一性/返回、脚本游标偏移、直接回调时间线与寄存器快照、所选同一性列表/实体字段，以及 49 记录清除区间的非空计数。该 fixture 是处理器局部 RAM/回调合同，不是重制实体生命周期、渲染或场景模型。重制必须把观察高水位种子之外的容量、正常剧情与存档/地图重载持久性、玩家可见渲染/动画/VDP 时序，以及碰撞/寻路使用方效果保留在本合同之外，直到四个显式 `entity-population-reload/*` 未知问题被分别观察。

地图脚本导入必须把源码命名的 `cloneEntity` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityCloneCommandFacts` 中独立的有序记录。该记录必须保留操作码 `$25`、其六字节物理布局、两条原始源码注释、有序源码位置同一性，以及通过其紧凑顺序/哈希合同的完整 304 行含零程序域。它还必须保留完整的 `csc25_cloneEntity` 区段：两次推进的两字节 A6 读取、有序的 `GetEntityAddressFromCharacter` 调用、源码命名的 `ENTITYDEF_OFFSET_ENTNUM` offset-18 字节读取进 D1、随后的查找、从 D1 的匹配字节写入与返回。导入器必须把四个操作数字节与单字节字段传递分开。它不得推断存储记录跨度、循环/计数器、整条记录复制、实体生命周期、分配、碰撞/寻路、可见性、持久性、时序、渲染或正常剧情可达性。

**已确认的有界运行时边界：** `sf2-map-script-entity-clone-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-clone-v1.json`）保留一次 Map Test 0 启动的全部九个源码有序字对。使用方必须保留精确的 H1 处理器入口/RTS、A6 偏移 4/8、两次输入字读取、两个调用位置/查找入口/返回恢复 PC 三元组，以及 offset-18 源码字节/目标字节前后记录。两个相邻的目标字节哨兵也是每条精确记录的一部分；保留它们只证明有界的相邻字节条件。查找体在这个仅会话的 trampoline 测试台中原样执行。该 fixture 不创建生命周期、整条记录、分配、呈现、碰撞/寻路、持久性、时序或剧情模型。

**未知：** `map-script-entity-clone/further-runtime-state-matrix`、`map-script-entity-clone/further-runtime-external-consumer-matrix` 与 `map-script-entity-clone/further-runtime-context-matrix` 仍然是唯一的分组队列。重制必须把未观察的状态、使用方与上下文问题保留在本合同之外，直到被独立观察。

地图脚本导入必须把两个源码命名的形式 `roofEvent` 与 `stepEvent` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapInteractionTriggerCommandFacts` 中独立的有序记录。每条记录必须保留其两个字宽操作数、原始 `trigger X`/`trigger Y` 注释、源码程序/命令同一性、两次推进的 A6 字读取、两个解析后的 `MAP_TILE_SIZE` 使用位置、直接目标同一性/顺序与返回边界。含零的直接/有效调用方映射与到 79 表/94 记录踩踏语料和 79 表/114 记录屋顶语料的源码专属链接必须保持独立于八个命令位置。重制可以适配这些记录，但只能通过独立指定的接口。

**已确认的有界运行时边界：** `sf2-map-interaction-trigger-runtime-v1`（位于 `tests/fixtures/h3/map-interaction-trigger-v1.json`）在一次启动中记录六次 Map 02 处理器调用。使用方必须保留每个 fixture 用例及其闭合的静态与运行时记录形状，包括其精确的处理器/调用位置同一性、D0/D1 字对、哈希坐标、所选表、步幅/终止符地址、`currentMapSeed`、处理器返回、匹配/终止符边界、标记结果、切换位、busy 字与战斗字节。record-0 命中、终止符未命中与 busy/battle 门控行是有界的合成输入；`currentMapSeed` 是输入同一性，而 `currentMapAfter` 是观察到的处理器后值。两个标记探测不是完整布局、碰撞/寻路或被调用方服务效果合同，直接 H1 JSR 位置命中也不是服务效果记录。

以下原版行为保持未知而不是从这些检查点推断：`map-interaction-trigger/full-layout-collision-pathfinding-effects`、`map-interaction-trigger/presentation-audio-timing-hardware-effects` 与 `map-interaction-trigger/persistence-story-reachability`。

地图脚本导入必须分别保留 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapLifecycleCommandFacts` 中源码忠实的地图生命周期记录 `resetMap`、`loadMapFadeIn`、`reloadMap` 与 `mapLoad`。四个形式必须保持不同，保留操作码/操作数宽度、源码注释、完整程序排序，以及精确的命名处理器事实：游标传递对非推进探测、VInt 操作记录、分支极性/目标同一性、直接指令/有效目标同一性、调用顺序，以及物理上 `csc37_loadMapAndFadeIn` 延续进 `csc48_loadMap`。

**已确认的运行时边界：** `sf2-map-lifecycle-runtime-v1`（位于 `tests/fixtures/h3/map-lifecycle-v1.json`）在一次启动中记录五次有界的处理器重放。使用方必须保留每个精确逐用例 fixture 字段：`id`、`handlerAddress`、`handlerReturned`、`currentMapAfter`、`directCallSiteOrder`、`loadMapD0WordAtCall`、`loadMapD1WordAtCall`、`tilesetD1WordAtCall`、`resetTailLoadMapD0WordAtTransfer`、`resetTailLoadMapD1WordAtTransfer`、`viewTargetEntityAfter`、`viewPlaneAPixelX`、`viewPlaneAPixelY`、`layoutClearStartMarkerCleared`、`layoutClearStartMarkerReplaced`、`layoutClearEndMarkerCleared` 与 `layoutClearEndMarkerReplaced`。两条 `mapLoad` 行是独立的输入操作数，不是相等分支模型。标记行不是完整布局或资源内容合同，直接 JSR 位置命中也不是服务效果记录。有界的 fade 行在其第一次观察到的 `WaitForVInt` 处清除 `FADING_SETTING`；它不是时序或可见淡入淡出规则。

重制只有在独立定义自己的生命周期接口之后，才可以引入它。以下原版行为保持未知而不是从这些检查点推断：`map-lifecycle/layout-collision-pathfinding-effects`、`map-lifecycle/entity-reload-player-placement`、`map-lifecycle/presentation-fade-hardware-timing` 与 `map-lifecycle/story-reachability-persistence`。

init 源码中内嵌的 201 个非配置标签使用相同表示，因此一条 init `script` 命令的全部 75 个目标都能跨两个资源族解析。

## 几何与图块数据

规范原版布局是有序的 4,096 字数组，寻址为 64 行乘 64 列。低十位选择 3x3 地图图块。其余六位保留为布局标志。所有解码引用必须满足 `blockIndex < blockset.length`；完整原版语料已经通过这一不变量。

图块集是有序的 3x3 瓦片字记录数组。原版图块索引 0-2 是加载器在压缩命令之前构建的内置空图块、闭宝箱与开宝箱图块。导入管线必须在解码之后把这些暴露为普通索引图块，这样事件与碰撞逻辑就不需要特殊的负值或带外同一性。

现代渲染器只有在每个位的含义被确认之后，才可以规范化瓦片与图块标志为命名字段。在此之前，规范导入 保留原始 16 位字与任何被证明的解读并列。渲染便利不得破坏未知位或重写源码证据。

解码布局与图块内容保持私有/生成。可分发的构建消费用户拥有的导入或项目拥有的替换资源；仓库存储 schema、聚合 fixture 与行为规则，而不是原版地图数据。

## 加载与选择顺序

对于原版保真行为，一次新的地图加载执行以下概念步骤：

1. 解析地图条目并加载调色板/瓦片集资源；
2. 解码/加载所选图块集，然后解码其布局；
3. 把标志触发的图块复制记录应用到工作布局；
4. 选择包含所请求或当前位置的区域；
5. 加载区域的图层原点、视差、自动滚动、图层类型、音乐与动画状态；
6. 在初始平面更新之前评估第一条匹配的屋顶记录；
7. 在 `LoadMap` 返回之后运行所选配置的 init 函数。

实现可以缓存不可变的解码图块集与布局，但标志/踩踏/屋顶复制作用于独立的 8 KiB 工作布局。非负地图参数与滚动传送从源码重建该布局并重放持久标志/宝箱状态。负的当前地图重载在屋顶评估之前保留工作布局。显式重置操作清除完整工作布局，然后使用那条保留式重载路径。重制必须显式建模所选路径；缓存复用不能仅根据地图 ID 是否改变来选择保留。

上游 enum `MAPDATA_OFFSET_LAYOUT = 8` 不是本合同的一部分。已确认的条目布局把指针放在偏移 10 处；没有原版代码引用那个有缺陷的常量。

## 区域、事件与物品

区域记录是有序的，并按坐标范围选择。其规范字段保留 layer-1 范围、layer-2/背景原点、两对视差、两对自动滚动、图层类型与默认音乐。使用方必须使用已确认的 30 字节逻辑形状，而不是把加载器的部分读取游标当作替换 schema。

事件数组保留源码顺序，因为原版分发对顺序敏感：

- 配置实体、区域（事件）、物品与描述分发使用第一条匹配条目；
- 地图配置变体扫描每个已设置标志，源码顺序中最后设置的标志胜出；
- 坐标 `$FF` 值在所属使用方确认之处保持为显式通配符；
- `$FFFF` 终止符是序列化细节，不成为游戏玩法记录；
- 可搜索物品保留坐标、标志、物品同一性与宝箱/非宝箱所有权。

事件 fixture 在完整解码表上包含九个可执行选择用例：实体特定/默认、区域（事件）精确/通配符/重叠首个/默认，以及物品索引掩码、朝向不匹配/默认与通配符朝向行为。重制事件选择器必须在呈现或剧情脚本接入之前复现这些用例。一次单启动 H3 矩阵确认全部九个原版包装器选择相同的记录偏移、目标地址、实体标志与掩码物品值。它使用私有插桩 ROM，其 50 字节 trampoline 只提供有记录的包装器输入；每个所选脚本条目被替换为 `rts`，因此脚本副作用与呈现保持在这个已确认的选择合同之外。

同一 `sf2-map-events-static-v1` fixture 记录源码拥有的事件合同，而不把路由当作重复记录：1,134 条物理宏/ROM 记录连接到 915 个目标概要；378 条指针表类别连接与 390 条有序选择器路由类别连接引用它们的表概要。导入器必须保留每条记录的有序操作数文本、表相对目标表达式、已解析地址、源码/H1 所有者同一性，以及物理、指针加权与路由加权计数之间的区别。同地址标签仍是证据，不是选择语义别名的许可。map44 原始区域（事件）默认表达式是一个显式例外边界，必须保持独立于一个带标签的目标。本合同只确立选择与目标同一性；绝不得用来推断所选脚本生命周期、过渡持久性、朝向或可见呈现。

对于精确分类为实体事件的 684 个概要，同一 fixture 还为每个已解析目标定义一个源码/H1 程序边界。导入器或未来的执行适配器必须保留入口同一性、源码/H1 跨度、有序非注释操作与标签、最终 return/direct-jump 形式、指令对有效控制流目标同一性，以及独立的物理、指针加权与路由加权引用计数。它必须保留同地址别名标签与九个已解析的跳转接口同一性，而不把别名当作语义行为。静态内部/外部目标分类只是物理跨度关系；运行时分支可达性、操作效果、持久性、对话时序与呈现保持在这个设计合同之外。

对于所有实体、区域（事件）与物品目标程序，适配器还必须保留每个操作的原始源码助记符、中性源码族、可空定义同一性与有序载荷上下文同一性栈。fixture 的 54 助记符词汇把原始 68000/数据形式与事件服务、map-script、entity-action 包装器/命令/载荷与流终止符源码形式分别连接；这是溯源/导入规则，不是按名称分配行为的许可。它必须保留独立的物理/setup/route 加权操作总数。特别是 Map 21 继承的 `entityActionsWait` 载荷及其后续 `entityActions`/`customActscriptWait` 区段必须保持源码嵌套，因此 `ac_*` 载荷条目不会被压平成同级调用。重制不得从这个静态连接推断宏副作用、持久性、对话文本、时序或呈现。上下文关系本身从源码别名、游标解析与终止符记录导入；它不是包装器名称描述生命周期或用户可见行为的声称。

对于同一完整目标程序语料，导入器必须把每个直接数字 `chkFlg`/`setFlg`/`clrFlg` 源码使用保留为源码形状的记录：解析后的宏/服务定义同一性、原始操作数文本与数值、类别/程序/源码/H1 操作同一性，以及独立的物理记录/setup 引用/路由引用权重。带紧邻的静态条件使用方的 `chkFlg` 记录还必须保留源码顺序、原始分支助记符/后缀、源码极性与指令/有效目标同一性。`read`、`set` 与 `clear` 标签只保持源码分类。重制不得从这个静态合同推断存档持久性、剧情状态、标志生命周期、操作效果或呈现。

导入器必须把每个直接 `script` 源码引用保留为独立的源码形状边：解析后的服务定义同一性、原始操作数标签、调用方程序/源码/H1 操作同一性、指令标签 H1 地址、有效地图脚本所有者程序同一性、终止，以及四个独立的调用方/引用权重。指令标签与有效所有者程序必须保持独立同一性，这样别名就不会被折叠。完整声明的标签与程序域（包括零引用行）是导入数据。这个静态图绝不得被当作引用会执行的证据，也不得当作关于时序、效果、持久性、剧情或呈现的推断。

对于区域（事件）与物品目标，导入器或未来的执行适配器必须保留同样的源码形状程序记录：入口同一性、物理跨度、有序非注释操作与标签、终止形式、指令对有效目标同一性、别名，以及独立的物理/指针加权/路由加权引用。区域（事件）合同有 150 条程序记录，外加 151 个概要中的一个显式原始表达式排除；物品合同为 80 个概要对应 80 条程序记录。Map 44 原始边界保持为无标签排除，而不是虚构程序。`Map21_DefaultZoneEvent` 的源码 `csc_end` 边界在 `csub_54714` 之前的下一个 H1 地址处结束；它只是源码结构规则。这些记录不得在没有运行时证据的情况下被提升为关于效果、对话、时序、持久性或生命周期的声称。

直接 `rts` 实体事件目标是显式空处理器，而不是记录数组。Map 55 与 flag-512 地图 52 配置用空实体列表配对它们。默认地图 52 配置反而初始化四个非己方角色实体，它们接收干净状态事件索引 128-131；原版交互调用链可以在一个相邻且不是随从的实体处到达其包装器。重制导入器必须保留空处理器类型，且不得把其 `rts` 操作码当作事件数据解析。原版剧情路线是否总是阻止这种相邻仍为未知，并且在没有路线级 fixture 的情况下不能成为重制碰撞规则。

标志、踩踏与屋顶记录描述进入工作布局的矩形图块复制。传送记录保留触发坐标、滚动模式、目标地图、目标坐标与朝向。其原版使用方不构成一个含混的优先级列表：标志复制在布局构造期间全部按源码顺序运行，加载时屋顶使用第一条包含记录，踩踏/传送扫描使用第一个坐标匹配。受控行走按进入车队、进入木筏、门、传送、区域（事件）、可通过性的顺序检查一个互斥的掩码标记。门处理在传送与区域（事件）检查之前变更并重新读取目标图块。重制必须保留这些阶段与排序规则。上述加载路径规则决定所产生的工作布局变更是否存活；只有其 VDP 可见帧时序保持在这个静态合同之外。

探索等待循环在 A/C 之前轮询一个待处理地图事件，外层循环在玩家动作之前分发该事件。如果两者在同一轮询中可见，地图事件胜出。原版 VInt 边界处精确的发布对输入采样时序是呈现/运行时证据，保持在这个静态优先级规则之外。

## 实体移动与动作时序

原版保真实体更新必须把位置、速度、原始移动距离、目的地、加速因子、移动标志、朝向、图层、精灵标志、动画计数器、等待计时器与动作脚本指针保持为独立状态。每个启用的 VInt 在实体的动作命令被分发之前执行移动更新。因此一条移动命令在一个 tick 中安装其目的地、行程与带符号速度，位置从下一个 tick 开始改变。

加速应用于原版轴向行程的外侧四分之三，减速在最后四分之一。速度只对行程非零的轴积分。主轴朝向使用已确认的 +/-8 量级边界；动画按 `(abs(xVelocity)+abs(yVelocity)) >> 5` 推进，保留字节 `-1`，并在 30 以上清除正计数器。零增量或符号交叉把轴吸附到其目的地并清除该轴的行程。

实体阻挡在所有其他非空槽位上比较候选目的地。启用阻挡标志时，低于一个 384 单位地图格子的曼哈顿距离会同时阻挡相对与绝对移动命令：命令指针保持在移动处，实体让步。未受阻挡的目的地推进到下一条命令并安装移动状态。等待命令同样在计数时保留其指针，且只有达到其阈值后才推进。合成或重制脚本不得把 `ac_pass` 当作停止操作码；它推进四字节并重新分发。

在两个行程字都清除之后，布局标记 `$2000` 选择图层 2，`$2400` 选择图层 0，`$3400` 设置浸没状态，而其他受控标记清除它。重制可以把这些值表示为类型化标志，但必须保留原始布局字，并在添加插值或呈现适配器之前复现 13 用例/20 tick 状态向量。

## 动画与呈现

动画表包含一个瓦片集加上缓存瓦片计数头部与有序替换条目。上游宏的 `speed?` 注释不是合同：使用方把那个字乘以 32，并把它用作复制进动画缓存的字节长度。每个条目拥有替换起点、瓦片计数、目标起点与一个逻辑计数器。地图加载把计数器初始化为一。每个启用的基础 VInt 回调递减它，当它到达零时提交该条目，并重载该条目的计数器；`$FFFF` 回绕到当前地图表的第一条条目。

每个提交的瓦片把 16 个字（32 字节）从缓存排队到 VRAM。动画是最后一个基础上下文回调，而当前 VInt 的 DMA 队列更早处理，因此该传递的最早处理点是下一个启用的 VInt。现代引擎必须在原版保真模式下保留这个逻辑节拍。四案例 H3 矩阵确认目标 VRAM 在提交帧保持不变，并在下一个启用 VInt 之后匹配所选缓存切片。它减去一个两帧的动画禁用对照，因为其他基础回调每个观察帧自然排队三条 DMA 命令。硬件级扫描线差异仍属于共享图形 fixture，但不使帧级传递延迟成为未知。

导入验证必须拒绝替换源范围超过其缓存瓦片计数的条目。原版语料已经满足全部 108 条条目；缓存大小为 4-96 个瓦片，条目传递为 1-48 个瓦片，完整逻辑周期为 40、44 或 80 个启用回调。

摄像机插值、平面合成、调色板应用、窗口交互与 VDP 可见输出是这个合同上的呈现适配器。它们不拥有地图内容或事件状态。

### 已绑定的场地点头生命周期

**已确认（静态绑定）：** 具有显式逻辑文本/视图设置的已准入场地配置，
可以使用[点头源码所有者](../../../research/map3-messenger-acceptance.md#source-bound-nod-lifecycle)
描述的、已独立检查的 `csc26_entityNodHead` 链。
这项单独绑定不扩大上述结构导入或受控 H3 声明。
解析所得实体的动画计数器必须在十次公共服务之前变为 FF。
随后其所选精灵必须在二十次服务中使用点头变换，再在十次服务中恢复正常选择，返回前把计数器置零。
每次服务先使用携带的实体服务标志，再按既有顺序执行视图/窗口工作和已注册立绘工作。
共享 RNG 保持实时有效；这些服务不重写前次输入轮询的复制字节。
相邻点头命令各自拥有完整序列和独立的完成令牌。

逻辑工作与实际呈现完成必须都结束后才可返回。
提前完成不能跳过工作；迟到完成、剔除和渲染延迟不能增加工作。
类型错误、重复和过期的完成通知不能推进调用方。点头持有控制权时，场地 Wait/Ack/移动保持不可用。
宿主用解析所得行动者当前的精灵/朝向及既有变换/正常资源投影语义阶段。
未绑定呈现保留原有合同。**未知：** 四十次显式 `WaitForVInt` 机会不能确立恰好四十个自然硬件帧、
隐藏 DMA/中断延迟或整个 Messenger 的验收。

### 已绑定的场地摄像机生命周期

**已确认（静态绑定）：** 显式场地视图配置可以使用已单独检查的
[目的地/滚动/等待链](../../../research/map3-messenger-acceptance.md#source-bound-camera-lifecycle)。
瓦片目的地必须禁用跟随，把每个坐标乘以384逻辑单位，并以已准入的前景/背景偏移安装四个目的地。
它必须保留当前位置、继承的跟随计数器，以及新目的地等于当前位置时已经活动的轴。
相等且非活动的轴保持非活动。已准入配置的自动滚动为零，layer0或255，
主平面（0时为B，255时为A）采用单位视差和原点0。
每个次级轴准入视差128或256及0–63瓦片偏移。
两个输入坐标及变换后的位置/目的地都必须保持在0–32767；带符号字回绕仍不受支持（Unsupported）。
每个轴必须先乘以自己的视差，再右移8位，最后加上自己的瓦片原点。
LoadMap 必须在此变换之前把共享原点限制并量化到128逻辑单位。

**已确认（静态源码与项目编写的行为）：** 在固定的 SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` 中，
`disasm/data/maps/entries/map20/2-areas.asm` 选择 `mainLayerType 255`、零原点、单位视差和零自动滚动。
`code/common/maps/camerafunctions.asm:VInt_UpdateViewData` 在跟随和滚动之前，
为 layer0 选择 B 坐标，为非零图层类型选择 A 坐标。
`code/common/tech/graphics/display.asm:SetViewDestination/loc_36BE` 在设置每个目的地之前应用各自原点和视差；
其窗口位置工作先于摄像机等待的再次检查。本次准入保留源码255字节及 A 跟随分支。
对于准入的零偏移配置，初始化、显式目的地和公共服务产生相等的 A/B 轴；
行动者投影选择权威主平面 A，它与基准 B 原点重合，没有重复前景。
引擎行为覆盖 A/B 起始状态不同的情况，以确立哪个平面驱动跟随决策，
随后在滚动/等待/返回过程中覆盖初始化后相等的轴。
这不确立原版自然可达性、中断时序或最终 VDP 像素。

**已确认（静态绑定）：** 固定的 `map40/2-areas.asm` 具有 layer255、backgroundY32
及背景视差128/128。`code/common/maps/mapload.asm:LoadMap/@loc_19` 在原点量化之后，
对每个偏移先应用无符号乘法/移位。`VInt_UpdateViewData` 用主平面判断死区/限制，
并按各轴视差缩放其速度24/32。在此配置中，主平面保持单位视差/原点0，故其位置就是共享跟随坐标。
`code/common/scripting/entity/entityscriptengine_1.asm:VInt_UpdateSprites` 在实体循环前选择区域的主平面。
普通实体的 `Layer` 不选择坐标平面；它与窗口是否存在之间的带符号比较选择精灵优先级。
这项现代投影不扩大特殊玩家精灵路径或原版滚动表/VDP 光栅细节。
`code/common/tech/graphics/specialsprites.asm:UpdateSpecialSprite` 保留这些所选坐标；
其 Nazca 特殊更新还额外应用 Layer 位移。该特殊图形/载具映射仍不在普通实体投影的此次准入范围内。
次级平面的偏移或视差不同时必须绘制，包括偏移相等而视差不同的情况。
实际行动者几何必须减去主平面坐标，而 A/B 瓦片几何保留各自独立原点。

每次公共服务必须保留当前实体服务标志，按实体工作、视图/滚动/窗口、已注册立绘工作的顺序执行。
无跟随模式在每次服务（包括活动滚动期间）准备速度24；若继承的计数器按带符号字解释时超过6，
则准备速度32；各轴按视差/256缩放。
各轴独立推进，到达或越过目的地时吸附并清除活动状态。
窗口隐藏使用该次滚动之前的 A 轴活动位。
共享 RNG 和前次轮询复制字节保留其既有服务所有权。

摄像机等待必须先完成滚动、等待一次、再次检查，若仍活动则重新开始，之后再等待一次才返回。
渲染时间、场地输入及呈现确认都不能解除这一逻辑等待。
已稳定的摄像机位置在对话及嵌套返回期间保持。
只有实际返回普通场地控制时才恢复玩家跟随，不重置位置、待处理目的地或计数器。
下一次服务应用常规跟随规则。

整个已绑定渲染器必须用 B 投影背景瓦片，用 A 投影前景瓦片。
行动者与公开的摄像机状态必须使用区域的权威主原点（layer0时为B，255时为A），
在更新和绘制回调间保持一致，不引入第二套居中/限制/平滑权威，也不重复应用前景偏移。
现代视口/布局与既有光栅资源仍是呈现选择。
遮挡必须由逐单元的瓦片优先级和 alpha 决定，不能把整个图块或整个平面一并提升优先级。
平面显示优先级必须与精灵间排序保持独立；高优先级地图像素遮盖低优先级精灵着色像素，
透明像素则露出下一表面。参见[源码/数据绑定](../../../research/map3-messenger-acceptance.md#bound-tile-priority-and-teacher-occlusion)。
未绑定呈现保留原有合同。**未知：** 自然中断/VDP/DMA 时序、字回绕目的地、显式速度、
光标/脉动覆盖，以及上述主/次级数值域之外的其他图层/视差/自动滚动配置仍被排除。
较早的七用例 H3 接缝不确立物理目标跟踪；当前的重制准入在下文规定。

### 已绑定的地图初始化窗口

已准入的绑定初始化脚本可以使用既有摄像机、显式文本和已注册立绘服务，
同时保留 `MapLoaded`、自身游标和真实嵌套调用栈。
不得把它重新标成场地输入，也不得赋予虚构的实体/Zone 调用方。
立绘打开只在移动完成后注册，关闭在移动前注销。
文本输入只有在交付完成后到达实际逻辑输入边界时才可用，普通场地动作仍保持关闭。
嵌套脚本返回保留外层初始化延续，包括窗口打开时的视图等待；
只有外层初始化器完成才恢复玩家跟随和场地控制。
此次扩展不准入战前及结果延续、未知/未注册立绘或未绑定的旧文本。
[引擎所有者](../../../../remake/docs/application/session-and-programs.md#bound-map-initialization-lifecycle)
记录其有界执行和验收；原版呈现节拍仍为**未知**。

### 已绑定的黑场景替换与战前窗口

**已确认（静态源码）：** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 的
`code/common/scripting/map/mapscriptengine_1.asm:csc37/csc48/csc42`、
`code/common/maps/mapload.asm:LoadMap`、`code/common/windows/windowengine.asm:InitializeWindowProperties`
及 `code/gameflow/battle/battlevints.asm:SetBaseVIntFunctions` 把淡入淡出、场景加载、实体分配和启用的服务分开。
csc37 开始黑色淡出；csc48 先解除跟随再等待淡出，临时停用已安装的 VInt 服务，
按显式原点加载地图，重新启用服务并等待一次后返回。
LoadMap 按该原点选择区域，变换两个平面，清除滚动速度/掩码和窗口记录，
替换 PALETTE_1_BASE 并保留 word_FFA828（跟随计数器）。
停用不等于 VINTS_CLEAR。此加载不把新基底字复制到当前调色板字中。
csc42 单独替换物理实体并挂载精灵。

重制可以准入窗口关闭、当前显示稳定为**黑色**、目标调色板有效且使用既有数值视图配置的场景替换。
无覆盖的显式黑色提示使用经验证的当前淡入淡出周期；非零临时覆盖在结束后恢复该周期。
替换基底对时必须保留黑色当前颜色，清除跟随/活动滚动/速度，保留跟随计数器，
独立于玩家位置选择显式原点，并保留已安装的实体服务政策。
不得调用普通场地初始化、隐式分配实体、调用 OnLoad、重新选择战斗、重置出战队伍/RNG，
或把新基底复制到当前颜色。窗口打开、加载时可见/正在过渡或普通传送仍活动的情况，
必须在发布新场景前失败。这一有限重制域不声称原版无法加载可见场景。

真实的 `BeforeBattleFinished` 加 `EnteringBattle`、有效游标、无事件调用方及绑定的文本/视图状态，
可以使用既有显式窗口/输入服务。真实调用栈和路线必须在 W1/W2 输入及嵌套脚本返回期间存续；
场地动作仍不可用。结果及不完整战前上下文继续排除。合法性不由地图、程序、文本或 PC 操作数选择。
绑定的 null 摄像机解除跟随必须只清除目标，保留各轴/目的地/计数器/服务政策/RNG，且不消耗语义时间。
源码 loadMapFadeIn 转换为执行命令时，在有限淡入淡出辅助之前发出该显式解除跟随，
表示在其第一次启用服务前已为 FF。源码先设置淡入淡出设置再解除跟随；这个原子辅助边界不是 CPU 周期一致性。
普通独立淡入淡出必须保留跟随；不得从相邻操作码推断行为。
源码转换必须保留加载后、随后实体替换之前的一次服务等待；加载前的淡入淡出等待不能覆盖该边界。
即使保留的玩家位于新地图所有区域之外，绑定绘制也必须使用所选视图区域，而不移动该玩家。
原版自然节拍/像素仍为**未知**。
[执行边界](../../../../remake/docs/application/session-and-programs.md#bound-before-battle-scene-and-windows)。

### 已绑定的物理摄像机目标

**已确认（静态源码）：** 固定的 csc24_setCameraTargetEntity 通过 ENTITY_INDEX_LIST
解析非负选择器，存储物理 VIEW_TARGET_ENTITY 字节，并且不等待就返回。
其负字路径保留负目标字节。VInt_UpdateViewData 即使在精灵不可见时也读取物理记录。
活动滚动绕过跟随和速度/计数器准备；稳定后的跟随先使用所选主平面、既有死区/限制和24/32速度，
再独立滚动各平面。已接受数值配置排除光标速度64、脉动覆盖、非零显式速度/自动滚动以及字回绕泛化。

已准入的绑定场地、地图初始化或真实战前调用方，可以通过既有同一性解析器安装有效物理目标。
安装必须不消耗语义时间或 RNG，保留区域/各轴/目的地/速度/跟随计数器/窗口/服务政策/出战队伍/调用方，
并以 LogicalView.TargetSlot 为唯一权威。缺失、已移除/超出表的别名、保留光标及不完整/结果上下文，
必须在发布前拒绝。有效的 slot0 默认别名仍有效；仅凭精灵不可见不得判定别名已移除。
后续服务必须跟随实时物理槽位，不能重新解析逻辑选择器。
滚动期间重新指定目标必须保留活动掩码和已准备速度，直到该次滚动完成。
null 解除跟随与显式目的地保留既有规则，并清除过期的兼容摄像机元数据。

普通 W1 输入和窗口关闭仍使用既有服务；安装不得绕过它们。
绑定白色调色板提示使用下述有限服务规则。
之后的战前主体（before-body）/效果/战斗初始化/加载仍不在此次准入范围内。
重制观察只确立各自命名的输入/服务/投影边界；原版自然节拍/像素仍为**未知**。
## 重制验收

第一个重制地图切片具备以下能力时即可验收：

- 导入一个规范生成的地图而不提交原版资源；
- 保留共享图块/布局引用与所有原始标志；
- 以每个图块索引都在范围内实例化 64x64 布局；
- 用相同的实现无关记录选择区域与有序事件；
- 对隔离的工作布局应用一次脚本化图块复制；
- 把刻意的呈现偏差与原版事实分开报告。

未来的 H4 测试应复用源自上述 H2 fixtures 的紧凑用例。渲染截图或提取地图转储不是黄金夹具；小索引、状态转换、对用户本地生成输出的哈希，以及占位资源渲染是允许的一致性表面。

## 源码绑定的选择使用方

**已确认（静态源码）：** 固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 中，
`code/common/scripting/map/mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow`0x47490 通过普通入口
调用 `YesNoPrompt`0x15284。返回0/Yes时设置 flag89，返回-1/No时清除它，随后 Sleep10 并返回。
所保留的实现无关命令的标志参数可复用；源码脚本恰好选择89。

完整能力包括创建/打开、入口按住状态释放、defaultYes、条件化选择/中性循环、确认/取消、
关闭/删除、结果写入和完整延迟/返回。布尔标志变更不能代表该使用方。
在条件化输入边界，Left/Right 先于 B/C/A；B 取消到No，C/A 确认所选值。
重新选择仍是一个动作。中性/导航有一次显式循环尾等待；确认/取消绕过该尾部。
窗口移动完成在每次服务后读取全局已表示的移动谓词，而非经过的帧配额。
窗口隐藏必须保留待处理位置修正。在有已表示窗口的第一次未隐藏服务中，先推进几何，
再以 length1/counter0 刷新每个几何上已稳定的窗口，且不改变该次服务已计算的移动结果。
几何稳定必须与计数器完成区分，包括静止刷新。
空窗口服务与普通地图初始化必须保留待处理修正状态。
选择、对话和立绘等待都必须使用公共的已表示移动结果；立绘 RNG 仍位于稍后的回调。
自身窗口删除先于标志变更及随后十次服务。

已接受的 ADR0010 语义边界使用刻意发起的新鲜条件化输入和显式 Wait。
映射的按住状态释放是独立门禁。其零语义服务过渡属于现代准入，不证明原版自旋没有 VInt。
不从宿主按键重复重建原版传入的输入重复/方向历史。
焦点/显示/音频交付不得增加工作、重设种子、重写轮询复制字节或替换实时实体/立绘状态。
公共服务顺序和实时启用状态贯穿打开、轮询、关闭和延迟。
提示在完整返回前保留调用方；指令返回与区域（事件）或普通场地控制返回有别。

当前绑定在既有场地/立绘/视图配置中准入正常移动，且不含金钱窗口。
金钱/调试/原始控制器输入重复、精确原版菜单光栅和硬件时序在此重制边界仍为**未知**。
源码调用位置65/66的请求复用既有 PCM 政策；确认并不隐含额外67。
原版运行时证据仍限于观察到的 Yes 路径；重制 No 运行不是原版 No 观察。
参见[溯源](../../../research/map3-messenger-acceptance.md#source-bound-choice-lifecycle)、
[执行](../../../../remake/docs/application/session-and-programs.md#source-bound-yesno-lifecycle)
及[验证](../../../../remake/docs/verification/field-programs.md#source-bound-choice-observation)。

## 原始场地文本与音乐边界

**已确认（静态源码）：** 固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 中，
`code/common/scripting/text/textfunctions_1.asm:DisplayText` 创建对话窗口，
执行其字形/控制/打字机工作，并在末尾清除 Typewriting。
没有隐式确认的包装器仍必须执行真实 W1/W2 令牌。
若最终边界无此令牌，绑定使用方必须完成必要工作和实际揭示，再自动推进。
不得增加接受输入的轮询、RNG 抽取、复制字节写入或合成延迟。

显式无说话者的原始显示是静音的。
它必须保留任何已准入的注册立绘，以及既有的实体 → 视图/滚动/窗口 → 立绘服务顺序与实时启用状态。
未解析的事件说话者或无效的显式说话者必须仍作为错误处理。
此规则适用于已准入场地文本配置，独立于文本 ID、程序、名称、宽度、种子或路线。

`code/common/scripting/map/mapscriptengine_2.asm:csc08_joinForce` 在原始文本之前开始音乐和成员资格工作，
之后调用音乐/输入辅助。后面的辅助另有独立逻辑时钟义务：
`code/common/tech/sound/music.asm:PlayMusicAfterCurrentOne` 总是在检查 `WAIT_FOR_MUSIC_END`
之前睡眠三个 VInt，并按需重复。**未知：** 原版音乐驱动进度、此前文本服务、门控采样和剩余轮询之间的映射。
[已接受的现代有限音乐政策](../../contracts/music-wait-service.md#accepted-modern-finite-music-policy)
现提供确定性配置：语义请求 generation0、每次公共服务一步、先激活再采样、完整的3步组，
以及独立的实际有限完成门控。JOIN 端点505是现代选择，并非505个原版 VInt 或 PCM 换算。
无配置等待必须保留已完成状态并以 Unsupported 停止。
有配置的 JOIN 继续经过此前播放、普通输入、关闭/Sleep10，以及既有随从/位置/Zone 返回规则。
这一有界实现不确立连续 H4。参见[溯源](../../../research/map3-messenger-acceptance.md#raw-display-and-unbound-music-progress)
及[执行](../../../../remake/docs/application/session-and-programs.md#raw-field-display)。

### 已绑定的地图白色有限服务

**已确认（静态源码）：** 固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 中，
`mapscriptengine_1.asm:csc3F/csc40/LaunchFading`、`ApplyFadingEffect`、
`fadingcommands.asm:ApplyCurrentColorFadingValue` 和 `data/tech/fadingdata.asm`
定义阻塞式地图调色板白色辅助：调色板 bitfield1、临时 period1、白色淡出 offsets1..7
或白色淡入6..0、受周期控制的终止符、一次额外 VInt、恢复此前周期和恢复 mask1111。
`vint.asm` 在上下文函数之前应用淡入淡出。这些是源码结构事实；
原版自然节拍、调用方阶段和像素仍为**未知**。
既有 H3 服务入口垫片不是完整原版服务的证据。

绑定重制必须从保留的基底派生每个有效当前颜色，保留唯一调色板状态权威，
并把逻辑完成与匹配的实际令牌结合后才推进程序。
有效 CRAM 存储偶数通道值0..14（`0xEEE` 掩码）；
原版原始饱和 nibble15 由其有效值14表示，而非原始 RAM 相等。
即使基底本身就是白色，稳定的 White 也仍与 BaseRestored 区分；零基底 Black 同样独立。
源码地图白色强制临时1，并在额外服务后恢复此前周期。
它必须遵从当前脚本实体启用状态，在每个既有机会中，于淡入淡出和已接受现代音乐服务之后，
按实体→视图/窗口→已注册立绘的顺序服务一次。
传送淡入淡出保留其已安装的场地实体上下文。
回执或呈现延迟均不增加服务/RNG 抽取。
对白色应用矛盾的黑色配置、无效/正在过渡的显示以及其他未准入调色板命令，
必须在辅助发布前拒绝。黑场景/传送规则不变。

实际白色交付使用既有现代半秒视口叠加层，并具有显式正常和减少闪光政策。
进入每个颜色服务时必须重置其父级调制和过期的相反叠加层。
整个视口覆盖是对源码仅选择地图调色板的有意近似；
不声称精灵/窗口调色板像素或原版 VDP 合成一致。
游戏玩法合法性不由场景、文本 ID、tick/种子或指令端点决定。

### 已绑定的战前主体（before-body）与战斗加载器所有权

**已确认（原版静态结构）：** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 的
`code/gameflow/battle/battleloop_1.asm` 在初始化和 `LoadBattle` 之前运行 `ExecuteBeforeBattleCutscene`。
`loadBattle.asm` 开始 `FadeOutToBlackAll`，解除视野目标，等待完成，替换上下文 VInt 并加载战斗场景。
`explorationfunctions_2.asm:FadeOutToBlackAll` 禁用滚动数据服务；
`WaitForFadeToFinish` 等待淡入淡出设置。这不同于脚本 `ExecuteFading` 的额外服务配置。
原版回调机会和呈现节拍仍为**未知**。

**已接受的重制政策：** 逻辑场地调色板辅助必须要求活动探索世界。
仅保留场地 Display/设置/视图，不得在战斗初始化后据此选择场地服务。
无配置的战斗加载器黑色淡出/淡入使用既有现代、仅交付的呈现服务；
交付本身保留战斗行动者/资源/RNG，不发布探索世界，也不执行场地实体/视图/窗口/立绘服务。
探索之外显式的仅场地 FullBlack 必须在辅助发布前拒绝。
持久场地设置和冻结的旧视图投影仍可供加载器门面及未来返回使用；
不要求第二个可变场地世界或调色板时钟。
回执延续可以运行既有加载/启动/介绍及首个队列所有者，包括它们的普通队列 RNG 抽取；
这些变化不属于场地服务，也不是另一次交付 tick。

绑定通用呈现效果和加载器交付必须按既有宿主焦点/可见性规则挂起，
丢弃暂停期间积欠，并在恢复时不追赶执行。未绑定及音乐政策保留既有所有者。
马赛克/颤抖继续经过实际交付和恢复；
这不确立源码 DMA/像素、原版机会或完整胜利/H4 验收。

### 已绑定的结果世界与返回

**已接受的重制政策：** 创建结果探索世界时，必须通过既有场地初始化器初始化其逻辑视图及关闭的文本/立绘窗口。
保留的战前视图/窗口工作不得成为新世界的权威。
Display、实时出战队伍资源、进度、装备配置、RNG、结果路线和已捕获返回锚仍是携带状态。

显式结果窗口必须保留其实际胜利/败北/返回延续和调用方。
其准入和输入就绪要求活动场地世界、真实结果路线、返回锚和游标、已初始化文本/视图设置，
以及既有窗口/立绘/揭示阶段守卫。
在战后程序、加入/标志、返回和地图初始化完成前，场地控制仍不可用。
存活/不朽治疗与脚本的全部重置保留不同的死亡己方角色规则。
这些引擎规则不确立原版自然连续性，也不消除 H4 比较缺口。
