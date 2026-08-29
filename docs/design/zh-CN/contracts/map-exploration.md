# 地图与探索合同

- **已确认的原版行为：** 79 个地图定义、共享图块/布局所有权、源码形式的区域/事件/物品/动画、64x64 解码布局、配置选择、有记录的首匹配分发规则、静态过渡使用方优先级、按加载路径的布局持久性、源码形状的 map-script 实体填充/重载、`cloneEntity`、camera-control 与 entity-placement 命令记录、有界静态 map-event 状态/控制/调用方/内容形状，以及分批的帧级实体移动/动作时序
- **未知的原版行为：** 非空地图 52 直接 `rts` 事件配置的正常剧情可达性、精确的 VDP 可见滚动时序、硬件级动画扫描线时序，以及最终的 VDP 可见渲染一致性
- 重制状态：实现无关的 Phase 3 合同；尚未选择引擎

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
| `sf2-map-event-random-battle-state-static-v1` | `CheckRandomBattle` 周围的静态调用方库存与源码有序局部请求/控制形状 |
| `sf2-map-event-combatant-state-static-v1` | 调用方侧角色 getter/setter 顺序、选择器与局部谓词形状 |
| `sf2-map-event-tactical-base-quote-state-static-v1` | 静态调用方/被调用方分支与服务形状，以及不含解码文本的数字引文行 ID 域 |
| `sf2-map-event-scripted-transition-state-static-v1` | 一个有界程序流的命令/载荷嵌套、处理器连接、指针目标与终止形状 |
| `sf2-map-event-flag-lifecycle-state-static-v1` | `sourceContext` 溯源/分母及 `flagLifecycleState.selectionSummary`、`sourceFiles`、`programFlows`、`lifecycleRelations`、`flagTotals`、`intervalCoverage`、`digests`，以及顶层聚合 `summary` |
| `sf2-map-event-cross-program-flag-state-static-v1` | `sourceContext` 溯源/分母及 `crossProgramFlagState.programDomain`、读写访问点与 cohorts、`partitions`、`crossProgramCandidates`、`categoryPairTotals`、`physicalContextCoverage`、`digests`，以及顶层聚合 `summary` |
| `sf2-map-event-flag-route-selection-static-v1` | `sourceContext` 溯源/分母及 `flagRouteSelection.programRouteContexts`、`classifiedCandidates`、`topologyCategoryTotals`、`selectorWriterRelations`、`domainDenominators`、`physicalCoverage`、`digests`，以及顶层聚合 `summary` |

对于三个 flag fixture，`retainedOwners` 完整性哈希以及 `serviceDefinitions`、
`dispatchEntries`、`sourceMacroDefinitions`、`categoryRoles`、`serviceJoin`、
`retainedIdentities` 中的服务/分发器/表同一性连接仍只属于跨所有者连接。它们不会把通用 flag
服务、trap、分发器、selector 表或 map-setup 数据保真所有权转移给本合同。

这些所有者增加的是可追踪深度，而不是已观察执行。它们不确立程序的自然可达性；实际采取的
路径或分支；调用方入口或读取时 flag 值；mutation 可达性或 mutation 后的值；生产者/使用方或
跨程序时间顺序；中间 mutation；实际 map-setup selector 求值或选中的 pointer table/event
record；被调用方成功或效果；状态生命周期或存档/读档持久性；对话或剧情含义；输入/音频/呈现/
时序；地图或战斗完成；R4b/H4/8C 一致性；或任何其他未观察的运行时行为。源码名称、数字文本
ID、调用、写入与分支标签仍是结构同一性。完整源码/H1/ROM 材料与私有生成连接仍是验证输入；
公共合同只保留已接受 fixture 已公开的有界结构记录、计数、哈希与溯源。

canonical-import fixture 是本合同的可执行序列化。其完整生成载荷在 `local/derived/` 下保持私有；只有聚合结构与溯源受追踪。

canonical import 现在解析全部 64 条配置路由与 126 个配置定义。一个配置定义引用六个独立共享的资源：实体、实体事件、区域（事件）事件、区域描述、物品事件与初始化函数。没有原始路由的 15 个地图 ID 保持空路由。选择按源码顺序扫描每个标志变体并保留最后设置的标志；直接返回处理器保持为显式空处理器，而不是被猜测的默认值替换。十案例 H3 矩阵确认选择器本身对缺失地图、默认路由、单个与多个已设置标志、最后设置标志胜出，以及恢复默认指针的后续别名返回 H2 建模的指针。它重放一次自然的调试 Map Test 提示，并只在选择器入口更改 `CURRENT_MAP` 与游戏标志位集；原始扫描与返回原样执行。六案例 init-dispatch 矩阵单独确认缺失地图跳过间接 init 调用，而五个默认/标志选择的配置每个都恰好一次调用其 H2 建模的 init 目标并通过原始包装器返回。它覆盖活动、脚本化与直接返回目标，而不声称合成的地图/标志组合复现其剧情副作用。初始化资源保留 130 条有序选择器路由连接，穿过 126 张 setup 表到 90 个目标配置文件。每个配置文件保留其物理源码操作边界、有序操作索引、精确标志操作数、直接指令/有效调用同一性，以及脚本目标解析；597 次物理操作、973 次指针表加权出现与 1,100 次路由加权出现是不可互换的计数。解析后的分发器记录保留链接 enum、指针布局行与符号化加载使用位置证据，用于字节偏移 20（第六个四字节槽位）处的 init 函数指针，加上缺失配置的比较与分支、间接调用、恢复与返回顺序。重制导入器可以将已识别的源码形式转换为类型化命令 IR，但必须保留未知操作数文本，且不得仅从操作码或目标名称推断脚本持久性、实体可见性或帧时序。独立脚本资源同样保留全部 178 个标签之间的有序命令、操作数文本与已解析引用。这些是可导入的命令图，不是现代引擎可以跳过原版解释器的状态、等待、摄像机、对话或呈现时序的证明。

**已确认的源码合同：** 保留 map-script 命令图的导入器必须保留 `sf2-map-script-engine-static-v1`、`tests/fixtures/h2/map-script-engine-static-v1.json` 字段 `expected.scriptControlCommandFacts` 中源码忠实的控制/音频形式：`csWait`、`playSound`、`csc06`、`executeSubroutine`、`jump`、零字节源码形式 `cscNop` 与两字节 `$FFFF` 的 `csc_end` 形式。`playSound` 保留其原始源码 enum 操作数及其维护的声音数据同一性；绝不能仅仅根据其源码标签被规范化成可听结果、音乐、效果或时序字段。

**已确认的源码合同：** 导入的解释器模型必须保持静态 A6 游标边界与编码字节存储不同。命名的分发循环读取一个字，将带符号 `-1` 与 `csc_end` 字比较，有一个经过 `BYTE_MASK`/`Sleep` 序列的源码负分支，并在表分发之前将非负选择器加倍。`executeSubroutine` 在其保存/调用/恢复序列之前以 post-increment 传递一个四字节目标；`jump` 将一个四字节目标传递进 A6，游标推进为零。这些源码控制流事实需要保真测试，但它们不确立计时器单位、声音播放、剧情可达性、持久性或玩家可见呈现。

**已确认的运行时边界：** `sf2-map-script-control-audio-runtime-v1` 保留六次单启动解释器观察：D0=1 与一次 `WaitForVInt` 入口对比调试 P2-START 跳过、`$06` 分发/返回、`$05` 原始声音字与 trap 边界、`$0A` 游标/栈/被调用方返回边界、`$0B` 游标替换，以及 `$FFFF` 结束/返回。其仅会话的入口接缝在观察器记录完成之前保留源码包装器尾声；它是 H3 测试台边界，而不是游戏玩法合同。在共享物理回调 PC 处，观察器在保留一次注册的同时按精确 fixture 用例角色选择失败诊断；这同样不是适配器行为。原版保真适配器必须保留这些控制与游标事实，而不把源码 enum 或观察到的 trap 转换成可听、时序、持久性或玩家可见合同。

**未知：** [`common-scripting.md`](../../../research/common-scripting.md#confirmed-map-script-controlaudio-macro-boundary) 中分组的 `map-script-control-audio/*` 队列保留更长的时长时序、声音驱动/可听结果、任意被调用方效果、正常剧情可达性与呈现行为。

直接地图事件 `txt` 与无操作数的 `clsTxt` 形式同样是源码同一性，而不是显示合同。导入器必须把每个有序位置保留为数字文本行标识符或字面量 `$FFFF` 哨兵源码形式，加上其调用方与独立命名的物理/setup/route 引用权重。它必须保留完整的 0–4,266 已声明 ID 域，包括零引用 ID，而不把原版文本复制或解码进地图合同。这不规定显示内容、说话者、窗口、等待、输入、剧情推进或呈现行为；它们保持在这个静态导入规则之外。

导入器还必须把五个源码命名的 map-script 过渡形式 `warp`、`resetMap`、`loadMapFadeIn`、`reloadMap` 与 `mapLoad` 保留为有序命令记录，而不是用猜测的场景操作替换它们。每条记录必须保留其物理操作数宽度、存在时的原始目标地图操作数文本、已解析的已声明地图 ID 或独立的 `MAP_CURRENT` 哨兵，以及源码命令/程序同一性。命名的处理器边界必须保留源码确认的 A6 游标读取、直接服务调用同一性/顺序，以及 `csc37_loadMapAndFadeIn` 落入 `csc48_loadMap` 的 fall-through；它必须把解析后的地图事件值、D1 立即数与压缩坐标乘数保留为源码事实。重制只有在定义了自己的行为之后，才可以把这些形式映射到引擎特定的过渡 IR；原版源码合同不确立事件消耗、摄像机状态、淡入淡出时序、显示时序或玩家可见过渡结果。

**已确认的有界运行时边界：** `sf2-map-script-transition-runtime-v1`（位于 `tests/fixtures/h3/map-script-transition-v1.json`）通过原版解释器增加一次五案例启动。原版保真适配器必须把观察到的操作码/A6/处理器返回顺序、csc37 到 csc48 的 fall-through、直接服务接缝顺序、csc07 事件字节，以及显式播种的 map/view-target/plane-A 状态事实保留为独立的适配器数据。源码在 csc37 入口写入 `OUT_TO_BLACK` 值 2，而这个有界运行在 `LoadMapTilesets` 之后第一次 `WaitForVInt` 入口处把 `FADING_SETTING` 读为 0；这两个事实都不定义淡入淡出时长或可见结果。适配器不得把这些合成 Map Test 结果变成正常剧情可达性、持久性、碰撞/寻路、摄像机呈现或显示行为。

Map-script 导入必须分别保留 `sf2-map-script-engine-static-v1`（位于 `tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapCameraControlCommandFacts` 中源码忠实的 `setCameraEntity`、`setCamDest` 与 `cameraSpeed` 记录。每条记录必须保留其源码操作码、物理操作数宽度、原始宏注释、程序/命令排序与含零的程序总数域。导入边界还必须保留精确的静态处理器记录：csc24 的推进字读取、两个分支极性与解析后的目标语句、解析后的常量与源码命名的写入；csc32 的字面量状态写入、两个推进字、别名调用后接等待调用与返回；以及 csc45 的推进源码字写入与返回。直接指令目标与已解析的有效目标必须保持独立同一性，包括逐处理器零计数行，而 `SetCameraDestination` 中两个解析后的 `MAP_TILE_SIZE` 使用位置必须保持独立的源码记录。重制可以独立定义引擎特定的摄像机接口；这个静态合同既不确立目标/目的地含义、坐标单位、速度效果、时序、可达性，也不确立呈现。

有界 H3 记录 `sf2-map-camera-control-runtime-v1`（位于 `tests/fixtures/h3/map-camera-control-v1.json`）增加七个单启动命令观察，而不把源码标签变成呈现设计：它保留负/己方角色/敌人目标分支记录、目的地输入字到传递字的值、两个速度字值、direct/service/wait 回调顺序与处理器返回边界。原版保真适配器必须把这些测得的状态与调用顺序事实保持独立于正常剧情可达性与 VDP/玩家可见行为，后者保持 **未知**。

Map-script 导入必须分别保留 `sf2-map-script-engine-static-v1`（位于 `tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPlacementCommandFacts` 中源码命名的 `setPos`、`setPosFlash`、`setFacing` 与 `setDest` 记录。每条记录必须保留其操作码、编码/操作数字节宽度、原始宏注释、程序/命令同一性与紧凑完整源码顺序/哈希边界。导入边界还必须保留精确的命名处理器记录：`csc19`/`csc23` 的非推进选择器读取加上存活状态游标调整调用与推进读取；`csc17` 的局部分支目标与 `csc19` 共享尾部边；`csc29` 的三个局部分支目标；解析后的 `MAP_TILE_SIZE` 乘数使用位置；源码形状的状态读/写操作数；以及含零的直接/有效调用方映射。这些源码记录不得被规范化成放置、朝向、移动、可见性、动画、坐标单位、碰撞、持久性、时序或渲染模型。有界 H3 合同 `sf2-map-script-entity-placement-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-placement-v1.json`）记录七个单启动用例：`setPos` 与 `setFacing` 的存活/死亡当前-HP 游标结果、源码缩放的实体记录字/朝向、完整的 31 次迭代局部 flash 回调序列及其独立的共享尾部回调，以及两个带符号目的地增量极性与 bit-15 等待/旁路。原版保真适配器必须保留这些测得的 RAM/游标/回调事实，而不把它们提升为呈现或地图移动设计。正常剧情可达性、完整动画/可见性/呈现与碰撞/寻路/持久性保持 **未知**。重制可以独立定义自己的实体状态接口。

Map-script 导入必须分别保留六个源码命名的桥接形式 `setActscriptWait`、`setActscript`、`customActscriptWait`、`customActscript`、`entityActionsWait` 与 `entityActions`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityActionBridgeCommandFacts`。每条记录必须保留其源码操作码、编码与操作数字节宽度、第一个源码选择器字段、精确的 `$FF` 或零源码控制字段、程序/命令排序与紧凑全语料顺序/哈希边界。内联载荷必须保留其源码形式类别、有序命令字节、源码终止符拼写、终止符字节计数，以及每个分别命名的 primary/payload/terminator 游标推进；导入器不得折叠这些物理量或用语义动作序列替换它们。`customActscript*` 记录必须保留单独的 csc14 两字节扫描传递与编码字节派生的、字对齐的扫描迭代次数；这些源码事实与 csc2D 的两字节解释命令读取不同。精确的 csc14/csc15/csc2D 处理器守卫，包括 csc2D 末端块、已解析的尾部传递目标、分支/调用顺序、源码常量使用位置与含零的直接/有效调用方同一性，仍属于导入边界。到地图事件与 entity-action fixtures 的连接只是溯源记录。有界 H3 合同 `sf2-map-entity-action-bridge-runtime-v1`（位于 `tests/fixtures/h3/map-entity-action-bridge-v1.json`）在一次会话中记录全部六个别名：精确的处理器/回调 PC、源码形状的实体字段与游标结果、csc14 内联终止符钩子、csc2D 索引目标与末端入口，以及在其空闲载荷写入之后紧跟解析后 PC 处的精确 csc2D 缓冲区记录。该快照是一个写入时记录，而其全局缓冲区指针与实体指针字段是处理器后观察；原版保真适配器必须保留那个观察边界，而不是把快照当作持久动作状态。重制可以定义自己的脚本/动作 IR，但正常剧情可达性、完整动作/运动/碰撞效果、自然时序、持久性与呈现保持 **未知**。

Map-script 导入必须分别保留八个源码命名的形式 `hide`、`startEntity`、`stopEntity`、`waitIdle`、`setSprite`、`setPriority`、`removeShadow` 与 `setSize`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityLifecyclePresentationCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释、完整程序/命令顺序与含零的程序域。导入边界必须保留精确的命名处理器指令顺序：推进对非推进 A6 读取、存活状态指针调整字面量/调用、源码形状的字段操作数、解析后的 `COMBATANT_ALLIES_NUMBER` 与 `%1000` 使用位置、分支极性/目标同一性、直接指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成可见性、动画、精灵、优先级、阴影、尺寸、碰撞、持久性、时序或呈现模型。有界 H3 fixture `sf2-map-entity-lifecycle-presentation-runtime-v1`（位于 `tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json`）额外保留 11 条精确的 Map Test 0 记录：回调顺序、存活对零当前-HP 的 start/stop 游标边界、受控的第二次比较空闲接缝、两个精灵选择器侧、优先级字节、remove-shadow 回调链，以及源码支持的临时/恢复尺寸字与 flags-B 状态。适配器必须保留那个观察边界，而不把它当作玩家可见或持久模型。剩余的原版问题是精确的 `map-script-entity-lifecycle-presentation/normal-story-reachability`、`map-script-entity-lifecycle-presentation/full-entity-state-callback-effects` 与 `map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence`。

Map-script 导入必须分别保留七个源码命名的形式 `shiver`、`nod`、`followEntity`、`faceEntity`、`moveNextToPlayer`、`fly` 与 `moveEntityAboveAnother`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityGestureRelationshipMotionCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释（包括两条空的 `moveEntityAboveAnother` 注释）、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递对非推进探测宽度、源码操作数/字面量使用位置、分支极性与目标同一性、循环目标记录、直接指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成手势、关系、位置、跟随、移动、图层、朝向、动画、时序、碰撞、持久性或呈现模型。有界 H3 fixture `sf2-map-entity-gesture-relationship-motion-runtime-v1`（位于 `tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json`）保留 17 条精确的 Map Test 0 记录：全部七个处理器入口、直接/有效回调计划与观察到的回调顺序、源码局部的 shiver/nod/fly 写入接缝、非推进 follow HP 字节探测、face/move 字边界，以及 `moveEntityAboveAnother` 寄存器记录。适配器必须保留那个受控观察边界，而不把它当作玩家可见、碰撞、时序或持久模型。剩余的原版问题正是 `map-script-entity-gesture-relationship-motion/normal-story-reachability`、`map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects` 与 `map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence`。

Map-script 导入必须分别保留十二个源码命名的形式 `setQuake`、`fadeInB`、`fadeOutB`、`slowFadeInB`、`slowFadeOutB`、`tintMap`、`flickerOnce`、`mapFadeOutToWhite`、`mapFadeInFromWhite`、`flashScreenWhite`、`fadeInFromBlackHalf` 与 `fadeOutToBlackHalf`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.screenPresentationCommandFacts`。每条记录必须保留操作码与物理操作数宽度、原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、源码立即数与存储操作数记录、分支极性/目标同一性、循环目标记录、指令目标加 PC 相对/直接寻址形式、有效目标调用方映射与返回边界。这些是源码布局与控制流记录。重制不得把它们规范化成屏幕效果、地图效果、视觉、调色板、VDP、时序、持久性或可达性模型；所有原版运行时后果都刻意保留在 H2 导入之外。有界 H3 fixture `sf2-map-script-screen-presentation-runtime-v1`（位于 `tests/fixtures/h3/map-script-screen-presentation-v1.json`）保留全部十二个处理器入口、源码派生的 quake 与 flash 操作数分区、直接目标/调用/返回时间线、A6/栈边界、直接处理器局部 RAM 写入与源码设置的调用寄存器字。适配器必须保留那个接缝，而不把其服务入口垫片当作原版服务行为或视觉/调色板、VDP、时序、持久性、可达性或地图/实体模型。剩余的原版问题正是 `docs/research/common-scripting.md` 中四个精确的 `map-script-screen-presentation/*` 队列。

Map-script 导入必须分别保留三个源码命名的形式 `animEntityFX`、`headshake` 与 `entityFlashWhite`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPresentationFxCommandFacts`。每条记录必须保留操作码、物理操作数宽度、直接对 `ENTITY_TRANSITION_` 简写编码、原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、立即数/源码操作数记录、单独标记的 `loc_46BE2` 分支块目标、循环目标记录、指令/有效目标调用方映射与返回边界。这些是源码布局与控制流记录。有界单启动 H3 fixture `sf2-map-script-entity-presentation-fx-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-presentation-fx-v1.json`）额外要求源码观察到的过渡选择器 2–7、flash 时长边界 10/57/180、全部三个处理器入口 PC、A6 游标边界、H1 返回 PC、局部分支/循环计数、紧凑的精确调用位置/目标/返回时间线，以及两个直接实体字节写入接缝。重制不得把其中任何记录规范化成实体效果、头部运动、颜色变化、过渡含义、视觉、时序、持久性或可达性模型。剩余的原版问题正是 `docs/research/common-scripting.md` 中四个分组的 `map-script-entity-presentation-fx/*` 队列。

Map-script 导入必须分别保留三个源码命名的 primary 形式 `showPortrait`、`hidePortrait` 与 `menu`，位于 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapScriptUiPrimaryCommandFacts`。每条记录必须保留操作码、物理操作数宽度、包括 `menu` 空注释在内的原始宏注释、完整命令/程序顺序与含零的程序域。导入边界必须保留命名的处理器指令顺序：A6 传递宽度、源码立即数/操作数记录、分支目标、源码栈指针传递记录、带别名的指令/有效目标调用方映射、返回边界，以及到 `dialogueCommandFacts.portraitHelper` 的溯源连接。有界运行时 fixture `sf2-map-script-ui-primary-runtime-v1`（位于 `tests/fixtures/h3/map-script-ui-primary-v1.json`）额外保留十一条处理器局部记录：四个精确的 H2 源码行输入、busy 字直接返回边界、一个受控的 `d1=$FFFF` 比较分支、hide 的直接调用时间线，以及选择器 `0`/`1`/`2`/其他 menu 分区与 A6/栈恢复。其解析后的指令/有效同一性仍是溯源，而其实际调用位置、垫片目标角色/PC 与回调返回 PC 是有界的控制流观察。仅会话的 ROM 观察那些 PC 并只替换解析后的垫片入口区间；别名从其指令目标垫片返回，之后其解析后的有效目标才执行。服务效果未被观察。重制不得把该 fixture 当作立绘绘制、菜单/输入行为、用户选择、时序、持久性、存档行为或可达性证据。这些原版问题在 `docs/research/common-scripting.md` 的四个 `map-script-ui-command/*` 队列下显式保持 **未知**。

共享解释器合同在 234 字节上定义 82 个 primary 命令布局，含 133 个有序操作数字段。导入器必须保留每个字段的字节宽度与流偏移，包括简写编码字，并且必须显式表示顺序、绝对跳转、条件绝对跳转与内联动作程序游标结果，而不是把每条命令压平成一个线性列表。

原版语料包含 304 个这样的程序与 348 个标签。导入验证必须把每条受追踪命令分配到恰好一个程序，保留 303 个 `csc_end` 与一个绝对跳转终止形状，并解析所有同程序/跨程序脚本目标。汇编器子程序调用保持显式的外部边；现代导入器不得仅仅根据其符号名内联或重新解释它们。

导入器应当把引用状态与地址状态分开保留：297 个程序有一个传入源码引用，七个没有，八个缺少 H1 入口地址。引用对可达性规划有用，但不得被提升为证明正常存档状态路线会执行该程序。

剧情状态导入必须把七个源码形式 `jumpIfFlagSet`、`jumpIfFlagClear`、primary `csc10`、`setF`、`clearF`、`yesNo` 与 `setStoryFlag` 保留为独立的源码同一性。primary `csc10` 载体及其 `setF`/`clearF` 别名必须保留各自单独的物理字布局；别名不会仅仅因为 primary 形式当前源码位置计数为零而抹掉它。导入器必须保留条件标志极性、直接设置/清除操作、yes/no 结果到 flag-89 的映射、战斗解锁转换 `flag = 400 + battleIndex`、分支目标/游标形状，以及直接对已解析服务目标同一性。有界十案例 H3 fixture 额外保留每个处理器局部的 A6/调用时间线与最终 GAME_FLAGS 位，覆盖两种条件极性、两个别名、两种 yes/no 结果与战斗解锁 base/wrap 输入。这些是命令图与会话局部变更事实；全局剧情排序、存档/读档持久性与玩家可见提示呈现/时序保持在导入器合同之外。

Map-script 导入必须把 `setBlocks` 与 `setBlocksVar` 保留为两个独立的源码命令形式，而不是替换为猜测的地图编辑操作。每条记录必须保留其两字节操作码、六个源码标记的单字节字段（`source x`、`source y`、`width`、`height`、`destination x`、`destination y`）、源码程序/命令顺序，以及三次配对的 A6 字读取进入 `d0`、`d1` 与 `d2`。静态适配器边界必须保留精确的直接 `CopyMapBlocks` 调用与只在 `csc34_setBlocks` 之后出现的源码命名的置位序列；它必须分别保留 helper 的解析后 8 位移位、6 位行移位、2 字节内部偏移、128 字节外部偏移与循环计数器指令。这些是源码布局与指令顺序事实。**已确认（H3）：** 有界运行时合同 `sf2-map-block-mutation-runtime-v1`（位于 `tests/fixtures/h3/map-block-mutation-v1.json`）额外要求两种形式的精确前向 FF0000-layout 字复制时间线/回读、一个跨行矩形，以及两个水平/垂直重叠方向；它还要求 `$34` 观察到的复制后切换位顺序与 `$35` 缺失的切换回调。重制必须把碰撞/寻路使用方效果、正常剧情可达性与地图重载/存档持久性，以及可见 VDP 呈现/周期像素时序保留在本合同之外，直到三个显式 `map-block-mutation/*` 未知问题被分别观察。

源码命名的实体动作 `ac_checkMapBlockCopy` 必须与 `setBlocks`/`setBlocksVar` 保持为独立的生命周期边界。**已确认（H3）：** `sf2-map-block-copy-lifecycle-runtime-v1` 位于 `tests/fixtures/h3/map-block-copy-lifecycle-v1.json`，并保留 `$40` 分发器的 fade 跳过、掩码后的 `$0800` show 与 `$0C00` hide 选择、动作游标推进，以及原始 helper 调用/目标/返回边界。这个有界合同只记录 busy、保存矩形元数据/缓冲区/哨兵和选定 FF0000 布局字的正源复制、负源清除与 active restore；busy 是匹配 roof 行的一基扫描序号而不是布尔值，保存缓冲区哨兵位于其矩形之后。它不能推断碰撞、导航、正常剧情可达性、持久化/重载或 VDP/fade/audio/时序。它们仍是分组的 `map-block-copy-lifecycle/*` 未知问题。研究 harness 的 source/H1/ROM guard 和仅对选定测试写入进行的范围恢复属于证据控制，不是重制状态要求。

Map-script 导入必须把四个源码命名的形式 `newEntity`、`loadMapEntities`、`reloadEntities` 与 `loadEntitiesFromMapSetup` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityPopulationCommandFacts` 中独立的有序命令记录。每条记录必须保留其物理操作码/操作数宽度、源码注释（包括一条刻意的空注释）、源码程序/命令同一性，以及精确的处理器游标读取、VInt、直接调用与源码常量记录。直接指令目标与已解析的有效目标同一性必须保持不同：特别是 `j_InitializeMapEntities` 不是 `InitializeMapEntities` 有效目标的替代拼写。重制只有在独立定义该接口之后，才可以把这些记录转换成自己的实体加载接口。**已确认（H3）：** 有界运行时合同 `sf2-entity-population-reload-runtime-v1`（位于 `tests/fixtures/h3/entity-population-reload-v1.json`）要求来自一次 BizHawk 启动的全部 12 条精确有序记录：三个 `newEntity` 同一性列表高水位种子、一次直接表加载、一次经同一性列表选择记录的 reload，以及全部七条 `loadEntitiesFromMapSetup` 源码输入行。每次观察保留处理器同一性/返回、脚本游标偏移、直接回调时间线与寄存器快照、所选同一性列表/实体字段，以及 49 记录清除区间的非空计数。该 fixture 是处理器局部 RAM/回调合同，不是重制实体生命周期、渲染或场景模型。重制必须把观察高水位种子之外的容量、正常剧情与存档/地图重载持久性、玩家可见渲染/动画/VDP 时序，以及碰撞/寻路使用方效果保留在本合同之外，直到四个显式 `entity-population-reload/*` 未知问题被分别观察。

Map-script 导入必须把源码命名的 `cloneEntity` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.entityCloneCommandFacts` 中独立的有序记录。该记录必须保留操作码 `$25`、其六字节物理布局、两条原始源码注释、有序源码位置同一性，以及通过其紧凑顺序/哈希合同的完整 304 行含零程序域。它还必须保留完整的 `csc25_cloneEntity` 区段：两次推进的两字节 A6 读取、有序的 `GetEntityAddressFromCharacter` 调用、源码命名的 `ENTITYDEF_OFFSET_ENTNUM` offset-18 字节读取进 D1、随后的查找、从 D1 的匹配字节写入与返回。导入器必须把四个操作数字节与单字节字段传递分开。它不得推断存储记录跨度、循环/计数器、整条记录复制、实体生命周期、分配、碰撞/寻路、可见性、持久性、时序、渲染或正常剧情可达性。

**已确认的有界运行时边界：** `sf2-map-script-entity-clone-runtime-v1`（位于 `tests/fixtures/h3/map-script-entity-clone-v1.json`）保留一次 Map Test 0 启动的全部九个源码有序字对。使用方必须保留精确的 H1 处理器入口/RTS、A6 偏移 4/8、两次输入字读取、两个调用位置/查找入口/返回恢复 PC 三元组，以及 offset-18 源码字节/目标字节前后记录。两个相邻的目标字节哨兵也是每条精确记录的一部分；保留它们只证明有界的相邻字节条件。查找体在这个仅会话的 trampoline 测试台中原样执行。该 fixture 不创建生命周期、整条记录、分配、呈现、碰撞/寻路、持久性、时序或剧情模型。

**未知：** `map-script-entity-clone/further-runtime-state-matrix`、`map-script-entity-clone/further-runtime-external-consumer-matrix` 与 `map-script-entity-clone/further-runtime-context-matrix` 仍然是唯一的分组队列。重制必须把未观察的状态、使用方与上下文问题保留在本合同之外，直到被独立观察。

Map-script 导入必须把两个源码命名的形式 `roofEvent` 与 `stepEvent` 保留为 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapInteractionTriggerCommandFacts` 中独立的有序记录。每条记录必须保留其两个字宽操作数、原始 `trigger X`/`trigger Y` 注释、源码程序/命令同一性、两次推进的 A6 字读取、两个解析后的 `MAP_TILE_SIZE` 使用位置、直接目标同一性/顺序与返回边界。含零的直接/有效调用方映射与到 79 表/94 记录踩踏语料和 79 表/114 记录屋顶语料的源码专属链接必须保持独立于八个命令位置。重制只能通过独立指定的接口适配这些记录。

**已确认的有界运行时边界：** `sf2-map-interaction-trigger-runtime-v1`（位于 `tests/fixtures/h3/map-interaction-trigger-v1.json`）在一次启动中记录六次 Map 02 处理器调用。使用方必须保留每个 fixture 用例及其闭合的静态与运行时记录形状，包括其精确的处理器/调用位置同一性、D0/D1 字对、哈希坐标、所选表、步幅/终止符地址、`currentMapSeed`、处理器返回、匹配/终止符边界、标记结果、切换位、busy 字与战斗字节。record-0 命中、终止符未命中与 busy/battle 门控行是有界的合成输入；`currentMapSeed` 是输入同一性，而 `currentMapAfter` 是观察到的处理器后值。两个标记探测不是完整布局、碰撞/寻路或被调用方服务效果合同，直接 H1 JSR 位置命中也不是服务效果记录。

以下原版行为保持未知而不是从这些检查点推断：`map-interaction-trigger/full-layout-collision-pathfinding-effects`、`map-interaction-trigger/presentation-audio-timing-hardware-effects` 与 `map-interaction-trigger/persistence-story-reachability`。

Map-script 导入必须分别保留 `sf2-map-script-engine-static-v1`（`tests/fixtures/h2/map-script-engine-static-v1.json`）字段 `expected.mapLifecycleCommandFacts` 中源码忠实的地图生命周期记录 `resetMap`、`loadMapFadeIn`、`reloadMap` 与 `mapLoad`。四个形式必须保持不同，保留操作码/操作数宽度、源码注释、完整程序排序，以及精确的命名处理器事实：游标传递对非推进探测、VInt 操作记录、分支极性/目标同一性、直接指令/有效目标同一性、调用顺序，以及物理上 `csc37_loadMapAndFadeIn` 延续进 `csc48_loadMap`。

**已确认的运行时边界：** `sf2-map-lifecycle-runtime-v1`（位于 `tests/fixtures/h3/map-lifecycle-v1.json`）在一次启动中记录五次有界的处理器重放。使用方必须保留每个精确逐用例 fixture 字段：`id`、`handlerAddress`、`handlerReturned`、`currentMapAfter`、`directCallSiteOrder`、`loadMapD0WordAtCall`、`loadMapD1WordAtCall`、`tilesetD1WordAtCall`、`resetTailLoadMapD0WordAtTransfer`、`resetTailLoadMapD1WordAtTransfer`、`viewTargetEntityAfter`、`viewPlaneAPixelX`、`viewPlaneAPixelY`、`layoutClearStartMarkerCleared`、`layoutClearStartMarkerReplaced`、`layoutClearEndMarkerCleared` 与 `layoutClearEndMarkerReplaced`。两条 `mapLoad` 行是独立的输入操作数，不是相等分支模型。标记行不是完整布局或资源内容合同，直接 JSR 位置命中也不是服务效果记录。有界的 fade 行在其第一次观察到的 `WaitForVInt` 处清除 `FADING_SETTING`；它不是时序或可见淡入淡出规则。

重制只有在独立定义自己的生命周期接口之后，才可以引入它。以下原版行为保持未知而不是从这些检查点推断：`map-lifecycle/layout-collision-pathfinding-effects`、`map-lifecycle/entity-reload-player-placement`、`map-lifecycle/presentation-fade-hardware-timing` 与 `map-lifecycle/story-reachability-persistence`。

init 源码中内嵌的 201 个非配置标签使用相同表示，因此一条 init `script` 命令的全部 75 个目标都能跨两个资源族解析。

## 几何与图块数据

规范原版布局是有序的 4,096 字数组，寻址为 64 行乘 64 列。低十位选择 3x3 地图图块。其余六位保留为布局标志。所有解码引用必须满足 `blockIndex < blockset.length`；完整原版语料已经通过这一不变量。

图块集是有序的 3x3 瓦片字记录数组。原版图块索引 0-2 是加载器在压缩命令之前构建的内置空宝箱、闭宝箱与开宝箱图块。导入管线必须在解码之后把这些暴露为普通索引图块，这样事件与碰撞逻辑就不需要特殊的负值或带外同一性。

现代渲染器只有在每个位的含义被确认之后，才可以规范化瓦片与图块标志为命名字段。在此之前，canonical import 保留原始 16 位字与任何被证明的解读并列。渲染便利不得破坏未知位或重写源码证据。

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

同一 `sf2-map-events-static-v1` fixture 记录源码拥有的事件合同，而不把路由当作重复记录：1,134 条物理宏/ROM 记录连接到 915 个目标配置文件；378 条指针表类别连接与 390 条有序选择器路由类别连接引用它们的表配置文件。导入器必须保留每条记录的有序操作数文本、表相对目标表达式、已解析地址、源码/H1 所有者同一性，以及物理、指针加权与路由加权计数之间的区别。同地址标签仍是证据，不是选择语义别名的许可。map44 原始区域（事件）默认表达式是一个显式例外边界，必须保持独立于一个带标签的目标。本合同只确立选择与目标同一性；绝不得用来推断所选脚本生命周期、过渡持久性、朝向或可见呈现。

对于精确分类为实体事件的 684 个配置文件，同一 fixture 还每个已解析目标定义一个源码/H1 程序边界。导入器或未来的执行适配器必须保留入口同一性、源码/H1 跨度、有序非注释操作与标签、最终 return/direct-jump 形式、指令对有效控制流目标同一性，以及独立的物理、指针加权与路由加权引用计数。它必须保留同地址别名标签与九个已解析的跳转接口同一性，而不把别名当作语义行为。静态内部/外部目标分类只是物理跨度关系；运行时分支可达性、操作效果、持久性、对话时序与呈现保持在这个设计合同之外。

对于所有实体、区域（事件）与物品目标程序，适配器还必须保留每个操作的原始源码助记符、中性源码族、可空定义同一性与有序载荷上下文同一性栈。fixture 的 54 助记符词汇把原始 68000/数据形式与事件服务、map-script、entity-action 包装器/命令/载荷与流终止符源码形式分别连接；这是溯源/导入规则，不是按名称分配行为的许可。它必须保留独立的物理/setup/route 加权操作总数。特别是 Map 21 继承的 `entityActionsWait` 载荷及其后续 `entityActions`/`customActscriptWait` 区段必须保持源码嵌套，因此 `ac_*` 载荷条目不会被压平成同级调用。重制不得从这个静态连接推断宏副作用、持久性、对话文本、时序或呈现。上下文关系本身从源码别名、游标解析与终止符记录导入；它不是包装器名称描述生命周期或用户可见行为的声称。

对于同一完整目标程序语料，导入器必须把每个直接数字 `chkFlg`/`setFlg`/`clrFlg` 源码使用保留为源码形状的记录：解析后的宏/服务定义同一性、原始操作数文本与数值、类别/程序/源码/H1 操作同一性，以及独立的物理记录/setup 引用/路由引用权重。带立即静态条件使用方的 `chkFlg` 记录还必须保留源码顺序、原始分支助记符/后缀、源码极性与指令/有效目标同一性。`read`、`set` 与 `clear` 标签只保持源码分类。重制不得从这个静态合同推断存档持久性、剧情状态、标志生命周期、操作效果或呈现。

导入器必须把每个直接 `script` 源码引用保留为独立的源码形状边：解析后的服务定义同一性、原始操作数标签、调用方程序/源码/H1 操作同一性、指令标签 H1 地址、有效 map-script 所有者程序同一性、终止，以及四个独立的调用方/引用权重。指令标签与有效所有者程序必须保持独立同一性，这样别名就不会被折叠。完整声明的标签与程序域（包括零引用行）是导入数据。这个静态图绝不得被当作引用会执行的证据，也不得当作关于时序、效果、持久性、剧情或呈现的推断。

对于区域（事件）与物品目标，导入器或未来的执行适配器必须保留同样的源码形状程序记录：入口同一性、物理跨度、有序非注释操作与标签、终止形式、指令对有效目标同一性、别名，以及独立的物理/指针加权/路由加权引用。区域（事件）合同有 150 条程序记录，外加 151 个配置文件中的一个显式原始表达式排除；物品合同为 80 个配置文件对应 80 条程序记录。Map 44 原始边界保持为无标签排除，而不是虚构程序。`Map21_DefaultZoneEvent` 的源码 `csc_end` 边界在 `csub_54714` 之前的下一个 H1 地址处结束；它只是源码结构规则。这些记录不得在没有运行时证据的情况下被提升为关于效果、对话、时序、持久性或生命周期的声称。

直接 `rts` 实体事件目标是显式空处理器，而不是记录数组。Map 55 与 flag-512 地图 52 配置用空实体列表配对它们。默认地图 52 配置反而初始化四个非己方角色实体，它们接收干净状态事件索引 128-131；原版交互调用链可以在一个相邻且不是随从的实体处到达其包装器。重制导入器必须保留空处理器类型，且不得把其 `rts` 操作码当作事件数据解析。原版剧情路线是否总是阻止这种相邻仍为 **未知**，并且在没有路线级 fixture 的情况下不能成为重制碰撞规则。

标志、踩踏与屋顶记录描述进入工作布局的矩形图块复制。传送记录保留触发坐标、滚动模式、目标地图、目标坐标与朝向。其原版使用方不构成一个含混的优先级列表：标志复制在布局构造期间全部按源码顺序运行，加载时屋顶使用第一条包含记录，踩踏/传送扫描使用第一个坐标匹配。受控行走按进入车队、进入木筏、门、传送、区域（事件）、可通过性的顺序检查一个互斥的掩码标记。门处理在传送与区域（事件）检查之前变更并重新读取目标图块。重制必须保留这些阶段与排序规则。上述加载路径规则决定所产生的工作布局变更是否存活；只有其 VDP 可见帧时序保持在这个静态合同之外。

探索等待循环在 A/C 之前轮询一个待处理地图事件，外层循环在玩家动作之前分发该事件。如果两者在同一轮询中可见，地图事件胜出。原版 VInt 边界处精确的发布对输入采样时序是呈现/运行时证据，保持在这个静态优先级规则之外。

## 实体移动与动作时序

原版保真实体更新必须把位置、速度、原始移动距离、目的地、加速因子、移动标志、朝向、图层、精灵标志、动画计数器、等待计时器与动作脚本指针保持为独立状态。每个启用的 VInt 在实体的动作命令被分发之前执行移动更新。因此一条移动命令在一个 tick 中安装其目的地、行程与带符号速度，位置从下一个开始改变。

加速应用于原版轴向行程的外侧四分之三，减速在最后四分之一。速度只对行程非零的轴积分。主轴朝向使用已确认的 +/-8 量级边界；动画按 `(abs(xVelocity)+abs(yVelocity)) >> 5` 推进，保留字节 `-1`，并在 30 以上清除正计数器。零增量或符号交叉把轴吸附到其目的地并清除该轴的行程。

实体阻挡在所有其他非空槽位上比较候选目的地。启用阻挡标志时，低于一个 384 单位地图格子的曼哈顿距离会同时阻挡相对与绝对移动命令：命令指针保持在移动处，实体让步。清晰的目的地推进到下一条命令并安装移动状态。等待命令同样在计数时保留其指针，且只有达到其阈值后才推进。合成或重制脚本不得把 `ac_pass` 当作停止操作码；它推进四字节并重新分发。

在两个行程字都清除之后，布局标记 `$2000` 选择图层 2，`$2400` 选择图层 0，`$3400` 设置沉浸，而其他受控标记清除它。重制可以把这些值表示为类型化标志，但必须保留原始布局字，并在添加插值或呈现适配器之前复现 13 用例/20 tick 状态向量。

## 动画与呈现

动画表包含一个瓦片集加上缓存瓦片计数头部与有序替换条目。上游宏的 `speed?` 注释不是合同：使用方把那个字乘以 32，并把它用作复制进动画缓存的字节长度。每个条目拥有替换起点、瓦片计数、目标起点与一个逻辑计数器。地图加载把计数器初始化为一。每个启用的基础 VInt 回调递减它，当它到达零时提交该条目，并重载该条目的计数器；`$FFFF` 回绕到当前地图表的第一条条目。

每个提交的瓦片把 16 个字（32 字节）从缓存排队到 VRAM。动画是最后一个基础上下文回调，而当前 VInt 的 DMA 队列更早处理，因此该传递的最早处理点是下一个启用的 VInt。现代引擎必须在原版保真模式下保留这个逻辑节拍。四案例 H3 矩阵确认目标 VRAM 在提交帧保持不变，并在下一个启用 VInt 之后匹配所选缓存切片。它减去一个两帧的动画禁用对照，因为其他基础回调每个观察帧自然排队三条 DMA 命令。硬件级扫描线差异仍属于共享图形 fixture，但不使帧级传递延迟成为未知。

导入验证必须拒绝替换源范围超过其缓存瓦片计数的条目。原版语料已经满足全部 108 条条目；缓存大小为 4-96 个瓦片，条目传递为 1-48 个瓦片，完整逻辑周期为 40、44 或 80 个启用回调。

摄像机插值、平面合成、调色板应用、窗口交互与 VDP 可见输出是这个合同上的呈现适配器。它们不拥有地图内容或事件状态。

## 重制验收

第一个重制地图切片在能以下能力时是可接受的：

- 导入一个规范生成的地图而不提交原版资源；
- 保留共享图块/布局引用与所有原始标志；
- 以每个图块索引都在范围内实例化 64x64 布局；
- 用相同的实现无关记录选择区域与有序事件；
- 对隔离的工作布局应用一次脚本化图块复制；
- 把刻意的呈现偏差与原版事实分开报告。

未来的 H4 测试应复用源自上述 H2 fixtures 的紧凑用例。渲染截图或提取地图转储不是黄金夹具；小索引、状态转换、对用户本地生成输出的哈希，以及占位资源渲染是允许的一致性表面。
