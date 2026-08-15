# 探索控制流合同

- 状态：**已确认静态主循环与探索分发控制**
- 证据日期：2026-08-09
- 范围：原版顶层探索交接、地图事件与玩家动作优先级、交互准入、物品补充边界与受限探索操作清单的实现无关重构，不导入启动、地图数据、输入时序、战斗结果、下游服务行为、呈现或故事含义

> 本文件是 [`exploration-control-flow.md`](../../contracts/exploration-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从源码形状的 `MainLoop` 与已接受探索控制辅助开始。它在辅助局部分发选择、候选身份、受限状态写入或下游交接处结束。它不定义那些交接到达的系统。

- **已确认**：`MainLoop` 在战斗候选选择前请求地图切换、把战斗索引 `-1` 当作无战斗哨兵，并在探索前把返回的 `BattleLoop` 调用经另一次地图切换请求发送；探索等待循环在动作输入前轮询地图事件，外层循环在玩家动作前分发地图事件；待处理地图事件在选择六个具名处理器交接之一前被清除；未知事件类型发出 `SFX_BATTLEFIELD_DEATH` 命令身份；玩家动作测试给 A 高于 C 的优先级并保留野外菜单回退；激活实体选择有 48 个候选槽、跳过玩家与随从身份，并使用已接受 384 内部定点距离限制；五个区域块种类码、满物品栏地图物品补充事实、Pacalon 分支标志身份 `530` 与门/屋顶/宝箱/平面更新清单是已接受静态事实。
- **推断**：无。玩家意图、故事含义与可见响应不从静态分支顺序或操作身份推断。
- **未知**：精确 VInt 边界发布对输入采样时序；自然调用方与故事可达性；全部 48 个候选槽的有效性与生命周期；384 内部距离值到屏幕像素或现代世界单位的转换；私有地图与区域内容；事件处理器、菜单、对话、音频、物品、队伍与战斗结果；状态持久性；滚动、门、屋顶、宝箱、传送与车辆帧；渲染呈现；畸形或注入状态；以及平衡、可访问性或战役意图。

[map-entry routing 合同](../../contracts/map-entry-routing-state.md) 拥有已接受 `SwitchMap` 与 `CheckBattle` 辅助局部规则。本合同只记录其顶层调用顺序与结果交接。[map 与 exploration 合同](../../contracts/map-exploration.md) 保留地图导入、构建、工作布局变更、map-script、实体、摄像机与已接受运行时轨道。两个兄弟合同都不被静默吸收。

## 证据所有者与源审计

`sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`gameflow.py`](../../../../src/sf2tool/h2/gameflow.py)，其带来源解释是[Startup、Main Loop 与 Exploration Core](../../../research/gameflow-core.md)。本合同只消费 `expected.explorationFacts` 与以下六条记录身份：

- `gameflow.main-loop`；
- `gameflow.exploration.engine`；
- `gameflow.exploration.interaction`；
- `gameflow.exploration.item-handoff`；
- `gameflow.exploration.loop`；
- `gameflow.exploration.actions`。

固定源只读审计确认相同受限顺序与身份事实。审计不把源注释提升为运行时或故事语义，也不扩展 fixture 拥有的声称集。

fixture 的完整源成员集包含 14 条记录，而 13 条记录携带直接 `sf2-gameflow-core-static-v1` 证据。本合同刻意只选择上文六条探索与主循环记录。五条未关联启动记录保持本合同之外，供独立启动边界：

- `gameflow.start.game-init`；
- `gameflow.start.intro`；
- `gameflow.start.cold-start`；
- `gameflow.start.region`；
- `gameflow.start.system-init`。

两条直接 fixture 链接记录保持其既有合同而不获得本合同：`gameflow.start.base-tiles` 仍归[UI graphics asset data](../../contracts/ui-graphics-asset-data.md)，`gameflow.start.z80-init` 仍归[audio system](../../contracts/audio-system.md)。仅成员 `map.block-mutation.copy-helper` 保留其 H3 所有者与[map-exploration](../../contracts/map-exploration.md)关联。这些区分防止一个聚合 fixture 成为吸收无关启动、资源、音频或地图变更面的借口。

## 主循环交接顺序

**已确认静态：** `MainLoop` 保留这些辅助与子系统交接：

1. 请求地图切换选择；
2. 请求战斗候选选择；
3. 战斗索引为 `-1` 时进入探索；
4. 否则用候选身份调用 `BattleLoop`；
5. 该 `BattleLoop` 调用返回后，在进入探索前再次请求地图切换选择。

第一与第五步保留 `SwitchMap` 操作身份，而非其私有表扫描。第二步保留 `CheckBattle` 操作身份与已接受无战斗哨兵，而非其坐标/标志准入规则。那些细节保持 `map-entry-routing-state`。

“Returning `BattleLoop` invocation”刻意比“completed battle”窄。该 fixture 证明源调用/返回顺序，而非战斗为何返回、选择哪个结果、是否发生存档变更或玩家接下来看到什么。

| 主循环边 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 初始路线 | 地图切换请求先于战斗候选请求 | 辅助局部规则与私有表独立 |
| 无战斗路线 | 战斗索引 `-1` 进入探索 | 无效原始索引与调用方可视诊断 **未知** |
| 战斗路线 | 非 `-1` 候选被交给 `BattleLoop` | 准入、回合、结果、奖励与呈现独立 |
| 返回战斗路线 | 地图切换请求先于探索 | 不声称返回意味着胜利、败北或持久性 |

## 事件与动作优先级

**已确认静态：** 探索等待辅助按该顺序轮询两个状态面：

1. `mapEvent`；
2. `actionInput`。

辅助返回后，外层探索循环按该顺序测试与分发：

1. `mapEvent`；
2. `playerAction`。

因此当两个值在一个源静态轮询/分发迭代中已可见时，地图事件胜出。这是分支优先级合同，不是精确同时性的观察。fixture 把实体脚本发布事件与控制器采样之间的 VInt 边界显式保持 **未知**。

[input-system 合同](../../contracts/input-system.md) 保留采样、当前/重复状态与等待辅助行为。本合同不重新定义控制器端口、按下/释放时序、重复策略或动作输入值的溯源。

## 地图事件分发边界

**已确认静态：** `ProcessMapEvent` 在选择处理器前清除待处理地图事件状态。已接受分发身份有序且闭合到这六个具名交接：

1. `Warp`；
2. `GetIntoCaravan`；
3. `GetIntoRaft`；
4. `GetOutOfCaravan`；
5. `GetOutOfRaft`；
6. `ZoneEvent`。

那六个之外的事件类型发出 `SFX_BATTLEFIELD_DEATH` 命令身份并返回。[audio-system 合同](../../contracts/audio-system.md) 拥有已接受声音命令域与播放状态边界。此处不声称可听结果、音量、时序、回退意图或呈现含义。

warp 路线包含硬编码 Pacalon 分支标志身份 `530`。本合同只保留原始标志身份与该分支存在。它不给标志 530 指定战役含义、不证明自然可达性、不定义持久性或导入下游地图过渡时间线。

处理器名是分发目标，不是完整行为合同。车队/木筏状态变更、区域事件脚本效果、warp 位置变更、淡入与返回路线保持其专用地图、脚本、服务与呈现所有者。

## 玩家动作与实体交互边界

**已确认静态：** 已接受玩家动作优先级是 A 先于 C。源码形状回退在没有更早已接受 C 路线消费动作时打开野外菜单交接，而 A 直接到达野外菜单交接。

该优先级不导入完整调试、车队、实体事件、区域查看或菜单时间线。它也不证明哪个按钮状态在 VInt 边界可见。[service-interactions 合同](../../contracts/service-interactions.md)、菜单合同与未来运行时证据保留其下游行为。

`GetActivatedEntity` 暴露该受限准入面：

- 考虑 48 个候选槽；
- 跳过玩家身份；
- 跳过随从身份；
- 已接受距离限制为 384 内部定点单位。

48 槽计数是迭代面，不是名册大小或每个槽被填充、有效、可见或可达的证明。数字 384 保留原版内部单位。本合同不把它重新标记为像素、现代引擎瓦片大小、世界米或供其他系统的通用交互半径。

实体朝向、桌子/柜台扩展、精确候选扫描状态、事件索引查找、对话选择与最终调用方可视结果保持 fixture 拥有事实集之外，除非兄弟所有者闭合它们。

## 区域与物品交接边界

**已确认静态：** 区域查看分类这些已接受块种类码：

| 种类身份 | 已接受码 | 此处未建立 |
| --- | ---: | --- |
| 宝箱 | 6144 | 私有宝箱内容、动画、对话、持久性 |
| 通用 | 7168 | 语义区域分类法或本地化 |
| 花瓶 | 11264 | 可见美术、包含物品或重置行为 |
| 木桶 | 12288 | 可见美术、包含物品或重置行为 |
| 书架 | 13312 | 对话/文本载荷或故事含义 |

码是源静态分类值，不是每个原版地图都包含每种或值应成为现代渲染器材质 ID 的证明。

已接受满物品栏事实很窄：当物品交接因相关物品栏已满而无法放置找到的物品时，地图物品被补充。这不定义物品栏容量、接收方优先级、物品身份、事务回滚、存档持久性、对话、声音或可见宝箱状态。物品定义、可变队伍状态与服务/菜单行为保持其独立合同。

## 受限探索操作清单

**已确认静态清单：** 所有者把探索源边界内的门、屋顶、宝箱与平面更新分类。本合同保留那四个操作族身份，使未来兼容适配器不静默丢弃交接。

清单不是完整变更序列。它不建立精确调用顺序、地址、帧节奏、地图内容可达性、VInt/DMA 时序、渲染瓦片、碰撞效果、声音或可见性。那些问题保持[map exploration](../../contracts/map-exploration.md)、摄像机、地图动画、图形与呈现所有者。

## 跨系统分离

探索控制连接系统而不拥有全部：

- map-entry routing 拥有地图切换与战斗候选辅助规则；
- 战斗合同拥有 `BattleLoop`、回合、结果、奖励与战斗呈现；
- map exploration 拥有导入、布局构建/变更、脚本、实体、摄像机与已接受运行时轨道；
- input-system 证据拥有原始采样与重复/等待行为；
- 全局标志保留其寻址与持久性问题；
- 物品、队伍、车队、菜单与对话所有者保留事务与玩家面向语义；
- audio 拥有声音命令与播放边界；
- 私有地图、区域、事件、物品与文本载荷保持公开合同之外；
- VInt 边界时序、滚动/门/屋顶/宝箱/传送/车辆帧、UI、音频时序、本地化、可访问性与平衡保持独立或 **未知**。

[story-progression 综合](../synthesis/story-progression.md) 可以把已接受交接放入更大解释，但不得把该静态控制图变成自然可达性或精确可见时间线的证明。

## 实现无关控制模型

```text
MainLoopControl
  initialOrder:
    - requestMapSwitch
    - requestBattleCandidate
  noBattleSentinel: -1
  noBattleRoute: enterExploration
  battleRoute:
    - invokeBattleLoop(candidateIndex)
    - requestMapSwitch
    - enterExploration

ExplorationIteration
  waitPollOrder:
    - mapEvent
    - actionInput
  outerDispatchOrder:
    - mapEvent
    - playerAction
  precedenceScope: valuesAlreadyVisibleWithinOneIteration

MapEventDispatcher
  clearPendingBeforeDispatch: true
  orderedTargets:
    - Warp
    - GetIntoCaravan
    - GetIntoRaft
    - GetOutOfCaravan
    - GetOutOfRaft
    - ZoneEvent
  unknownTypeCommandIdentity: SFX_BATTLEFIELD_DEATH
  pacalonBranchFlagIdentity: 530

PlayerActionRouter
  buttonPriority:
    - A
    - C
  fallbackHandoff: FieldMenu

ActivatedEntityBoundary
  candidateSlotCount: 48
  excludedIdentities:
    - player
    - followers
  distanceLimit:
    value: 384
    unit: originalInternalFixedPoint

AreaKindCodes
  chest: 6144
  generic: 7168
  vase: 11264
  barrel: 12288
  bookshelf: 13312

FullInventoryBoundary
  mapItemRefilled: true

ExplorationOperationInventory
  - doorUpdate
  - roofUpdate
  - chestUpdate
  - planeUpdate
```

这是逻辑 parity 模型，不是必需引擎循环、内存布局或线程模型。模型刻意把交接身份与优先级和下游效果分开存储。`invokeBattleLoop` 不是战斗结果，`FieldMenu` 不是 UI 实现，地图事件目标名不是完整处理器合同。

## 原版保真与现代化

原版保真模式保留已接受主循环顺序、两个事件先于动作优先级层、清除先于分发规则、六个目标身份、动作按钮优先级、交互准入元数据、区域种类码、补充事实、标志身份与受限更新清单。它把时序与下游结果保持为独立测试或显式未知。

现代引擎可以使用事件队列、类型化命令、不可变状态快照、验证实体句柄、显式事务、异步场景加载或输入动作而非原始按钮。那些是刻意设计选择。兼容适配器必须仍复现已接受源面向优先级与交接事实，刻意分歧必须分别记录。

公开 parity fixture 只需要结构元数据、身份、码与合成状态。原版地图、事件、物品、对话、图形与音频载荷保持私有/生成输入。

## H4 验收门

未来重制探索控制适配器只在以下情况通过本合同：

1. 其兼容路线在战斗候选选择前请求地图切换，并保留战斗索引 `-1` 为无战斗哨兵；
2. 返回的 `BattleLoop` 调用在探索前到达地图切换请求，而不把返回当作胜利、败北或任何其他结果的证明；
3. 等待轮询与外层分发都为一次迭代内已可见的值保留地图事件先于动作优先级，而精确 VInt 边界同时性保持分别测试；
4. 待处理地图事件在分发前清除，六个目标身份保持有序且不同，未知事件类型保留回退命令身份而不发明可听行为；
5. 玩家动作兼容性保留 A 先于 C 优先级与野外菜单回退身份，而不重新定义控制器采样或 UI 行为；
6. 激活实体兼容性保留 48 个候选槽、玩家/随从排除与 384 原版内部定点距离限制，而不把那些事实转换成名册大小、像素或通用交互半径；
7. 五个区域种类码、满物品栏地图物品补充事实、Pacalon 分支标志身份 `530` 与门/屋顶/宝箱/平面清单保持可复现，而不导入私有内容或未接受事务/帧语义；
8. 启动、map-entry 辅助规则、地图导入/变更、战斗、输入、物品、队伍、菜单、对话、音频、持久性、呈现、畸形状态行为、故事含义与平衡保持分别测试或显式 **未知**；
9. 公开工件包含结构元数据与合成状态，而非版权载荷。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| MainLoop 地图切换/战斗/探索调用与返回顺序 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 辅助规则、战斗结果、地图加载、可见过渡 |
| 地图事件先于动作轮询与分发 | **已确认静态优先级** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 精确 VInt 边界发布/采样时序 |
| 清除先于分发、六个目标、未知类型命令、标志 530 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 处理器效果、可听结果、故事与持久性含义 |
| A 先于 C 优先级与野外菜单回退身份 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 输入采样、调试/服务/菜单时间线、UI 行为 |
| 48 候选、排除、384 内部单位限制 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 槽有效性、名册含义、坐标转换、调用方可视结果 |
| 区域码、满物品栏补充、受限更新清单 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 私有内容、事务、持久性、帧与呈现 |
| 启动五、base-tile/Z80 既有所有者、block-copy 仅成员记录 | **排除兄弟记录** | 独立既有或未来合同；不消费额外 fixture | 不扩展六记录关联边界 |
| 运行时时序与下游地图/战斗/输入/服务/呈现含义 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得从静态控制推断完整探索体验 |

## 复现

```powershell
uv run sf2 h2 gameflow-core
uv run sf2 design-contracts test
uv run sf2 verify
```
