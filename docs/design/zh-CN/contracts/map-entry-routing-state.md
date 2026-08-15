# 地图入口路由状态合同

- 状态：**已确认静态地图切换、战斗候选与存档点选择控制**
- 证据日期：2026-08-09
- 范围：三个原版地图入口路由辅助的实现无关重构，不导入其私有表语料、故事含义、持久性、地图加载、战斗生命周期、呈现或畸形输入行为

> 本文件是 [`map-entry-routing-state.md`](../../contracts/map-entry-routing-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同在调用方把地图或位置查询提供给三个已接受辅助身份之一时开始：`SwitchMap`、`CheckBattle` 或 `GetSavepointForMap`。它在辅助局部所选地图、候选战斗索引、坐标/朝向输出或受限状态写入处结束。它不建立调用方为何发出查询或调用方用结果做什么。

- **已确认**：`SwitchMap` 扫描六字节条目到负源地图终止符，并选择标志设置的第一个匹配源地图条目；`CheckBattle` 把 `-1` 地图输入解析为当前地图、要求解锁标志、接受 `-1` 触发坐标通配、在完成检查前写入战斗区域字段、为已完成匹配清除解锁标志，并在无行匹配时报告战斗索引 `-1`；`GetSavepointForMap` 在标志 399 之前使用符号游戏开始常量，否则扫描四字节存档点条目到 `-1` 终止符、保留已接受 `(1, 1, UP)` 缺失地图回退，且只在标志 64 设置时咨询独立四字节木筏重置表。
- **推断**：无。战役意图、玩家面向含义与下游控制流不从这些静态选择器推断。
- **未知**：私有表内容与基数；自然故事与调用方可达性；标志持久性与存档语义；返回候选后的战斗准入、战斗完成含义与战斗结果；地图加载、过渡、碰撞与生成行为；已接受值之外的调用方可视错误与返回约定；畸形、截断或未终止表；调试或注入状态；运行时时序；UI、音频与呈现；以及平衡或战役意图。

[global-flag state 合同](../../contracts/global-flag-state.md) 拥有低级标志寻址，而非路由标志的语义含义或持久性。[battle-encounter definition 合同](../../contracts/battle-encounter-definition.md) 拥有已接受遭遇数据，而非该候选选择辅助。[map-design principles 综合](../../synthesis/map-design-principles.md) 可以把已接受地图交接放入更宽玩家面向解释，但不得把那些静态选择器变成运行时可达性或可见过渡顺序的证明。

## 证据所有者与综合前审计

`sf2-common-maps-static-v1`（[`common-maps-static-v1.json`](../../../../tests/fixtures/h2/common-maps-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`maps.py`](../../../../src/sf2tool/h2/maps.py)，其带来源解释是[Common Map Engine](../../../research/common-maps.md)。本合同只消费这三条记录及其对应 `expected.mapFacts` 段：

- `maps.switch-map` 对应 `SwitchMap`；
- `maps.battle-trigger` 对应 `CheckBattle`；
- `maps.savepoint` 对应 `GetSavepointForMap`。

固定源只读审计确认相同受限控制形状与顺序事实。审计不把源注释提升为故事含义、不导入相邻表内容，也不扩展 fixture 拥有的声称集。

`sf2-map-data-static-v1` 聚合与全部 `map.data.*` 研究记录在本合同消费证据之外，包括标志切换地图、存档点坐标与木筏重置坐标表身份。[Issue #99](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/99) 保留该聚合的修正轨迹；其状态此处不提供证据，解决它也不自动扩展本合同。因此本合同拥有辅助行为而不声称喂养它的私有表语料。

同一 common-maps fixture 还包含摄像机、动画、布局与未使用加载器事实。此处不消费。特别是 `maps.camera`、`map.camera-control.wait-for-view-scroll-end`、`maps.animations`、`maps.map-layout` 与 `maps.unused-mapload` 保持其既有或未来所有者。[map-layout data 合同](../../contracts/map-layout-data.md) 与既有摄像机运行时证据保持语义不变。

## 标志切换地图选择

**已确认静态：** `SwitchMap` 消费原版地图身份并扫描有序六字节行。每个源码形状行携带源地图身份、标志引用与替换地图身份。负源地图值终止扫描。

对源地图与传入地图不同的行，扫描继续。对匹配源地图，辅助检查该行标志。标志设置的首个匹配行替换结果并结束扫描。如果没有已接受行在终止符前选择替换，原版地图值保持辅助结果。

顺序是合同的一部分：同一源地图的行不能被表示为只按源地图键控的无序字典。同样，为每个源地图预计算一个替换会擦除首设置标志规则。

| 属性 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 行存储 | 有序六字节源码形状条目 | 精确语料、行数与载荷保持独立 |
| 终止符 | 负源地图值 | 畸形或缺失终止符行为 **未知** |
| 准入 | 源地图匹配且行标志设置 | 标志含义与持久性 **未知** |
| 优先级 | 首个准入行胜出 | 不推断战役优先级 |
| 回退 | 无行准入时保留传入地图 | 下游加载与可见过渡 **未知** |

“replacement”只描述辅助输出值。它不证明地图资源被加载、实体被生成、碰撞状态改变或玩家看到过渡。

## 战斗候选选择

**已确认静态：** `CheckBattle` 接受地图查询加 X 与 Y 坐标。地图输入 `-1` 使用当前地图状态匹配。候选行要求其战斗解锁标志设置。触发 X 与触发 Y 独立测试，任坐标存储值 `-1` 是该轴通配。

地图、解锁标志与坐标准入成功后，辅助写入所选行的战斗区域 X、Y、宽与高字段。那些写入发生在完成标志检查前。所选候选已完成时，辅助清除其解锁标志。无行匹配时，已接受战斗索引结果为 `-1`。

这是候选选择与受限变更合同，不是战斗入口合同。返回索引本身不证明战斗循环开始、已完成战斗被重放，或调用方消费区域字段。解锁与完成标志的含义、生命周期与持久性也保持本合同之外。

| 阶段 | 已接受顺序或规则 | 此处未建立 |
| --- | --- | --- |
| 地图归一化 | 输入 `-1` 选择当前地图状态 | 其他原始地图值的有效性 |
| 路线准入 | 地图匹配、解锁标志，然后 X/Y 测试 | 故事可用性与调用方频率 |
| 坐标匹配 | 存储 `-1` 是每轴独立通配 | 碰撞几何或寻路 |
| 受限状态写入 | 区域 X/Y/宽/高在完成检查前写入 | 后期生命周期或消费者使用 |
| 完成分支 | 已完成匹配清除其解锁标志 | 存档持久性或玩家可见含义 |
| 无匹配 | 战斗索引为 `-1` | 调用方可视分支、重试或错误策略 |

私有战斗坐标行及其总数保持独立所有者数据。特别是，既有 `battle.data.map-coordinates` 关联不由本合同复制。

## 存档点与木筏重置选择

**已确认静态：** `GetSavepointForMap` 有两条顶层选择路线。

标志 399 设置前，它返回源的符号游戏开始地图、X、Y 与朝向常量。本合同把那些身份保留为成组路线，而不导入其数字值或给标志 399 指定故事含义。

否则，辅助初始化已接受缺失地图回退 `(x=1, y=1, facing=UP)` 并扫描有序四字节存档点条目，直到找到查询地图或到达 `-1` 地图终止符。匹配行提供地图、X、Y 与朝向输出。无行匹配时保持回退。

木筏重置是独立条件状态交接。只在标志 64 设置时，辅助咨询第二张四字节地图/坐标表并应用所选木筏地图/X/Y 值。本合同保留条件表咨询而不声称表内容、行数、世界状态含义或持久性。

| 路线 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 399 前 | 成组符号游戏开始地图/X/Y/朝向输出 | 数字常量与故事含义 |
| 普通匹配 | 有序四字节行提供地图/X/Y/朝向 | 私有行语料与基数 |
| 普通未命中 | `(1, 1, UP)` 坐标/朝向回退 | 输入地图本身是否有效 |
| 木筏重置 | 标志 64 门控第二张四字节表咨询 | 表载荷、持久性与可见木筏状态 |

“Savepoint”保留原版辅助与表身份。它不证明存档写入、存档槽选择、SRAM 变更、检查点耐用性或断电后恢复。那些是独立存档系统问题。

## 跨系统分离

这些辅助是窄路由服务，不是完整地图入口管线：

- 全局标志保留其自身寻址与生命周期边界；
- 私有 `map.data.*` 表保持本合同消费证据之外；
- 静态布局语料仍归[map-layout data](../../contracts/map-layout-data.md)，而构建与工作布局变更仍归[map exploration](../../contracts/map-exploration.md)；
- 碰撞、实体放置、摄像机状态与 VInt/DMA 呈现保持其独立地图、摄像机、运行时与呈现所有者；
- 战斗遭遇组合与 battle-loop 控制仍归战斗合同；
- 存档槽选择、SRAM 格式与持久性仍归 save-system 合同；
- 故事推进可以消费已接受结果，但不得从它们推断自然可达性；
- UI、音频、淡入、时序、本地化、可访问性与平衡不从静态辅助时间线派生。

引擎可以在一个地图加载事务中组合这些服务，但该组合在已接受证据闭合调用方与运行时连接前是重制设计决定。

## 实现无关状态模型

```text
MapSwitchQuery
  incomingMap
  orderedRows[]: MapSwitchRow

MapSwitchRow
  sourceMap
  flagRef
  replacementMap
  storedSizeBytes: 6

MapSwitchResult
  selectedMap
  selectedRowRef: optional

BattleCandidateQuery
  rawMap
  currentMap
  x
  y

BattleCandidateRowView
  mapRef
  unlockedFlagRef
  triggerX
  triggerY
  battleArea: x, y, width, height
  completionFlagRef

BattleCandidateResult
  selectedBattleIndex: index | -1
  selectedAreaWrite: optional
  unlockedFlagClear: optional

SavepointQuery
  incomingMap
  flag399State
  flag64State

SavepointRowView
  map
  x
  y
  facing
  storedSizeBytes: 4

SavepointResult
  map
  x
  y
  facing
  source: gameStartConstants | matchingRow | missingMapFallback
  raftResetWrite: optional map, x, y
```

这是逻辑 parity 模型，不是必需引擎内存布局，也不是私有原版表的公开投影。`orderedRows` 与行视图可以在本地验证期间从私有输入填充；公开 fixture 应只使用合成行与结构元数据。

模型保持选择结果与下游效果分离。`selectedMap` 不是已加载地图，`selectedBattleIndex` 不是已开始战斗，`SavepointResult` 不是持久存档记录。

## 原版保真与现代化

原版保真模式保留行顺序、终止规则、标志/坐标准入、战斗区域先于完成的精确变更顺序、已完成匹配标志清除与两条存档点路线。它暴露未解决调用方、持久性与畸形输入问题，而非用假设行为填充它们。

现代引擎可以使用类型化路由定义、验证集合、显式选项/结果类型、事务、不可变状态快照或具名故事条件。它也可以在运行时前拒绝畸形私有输入。这些是刻意设计选择。兼容层仍必须保留已接受原版面向顺序与输出，任何分歧必须被记录而非呈现为原版证据。

原版表载荷不要求出现在公开 parity 工件中。公开 fixture 与报告保留函数身份、结构大小、已接受常量、操作顺序与合成用例，而不再分发私有地图、遭遇或坐标内容。

## H4 验收门

未来重制路由适配器只在以下情况通过本合同：

1. `SwitchMap` 兼容性扫描有序六字节行到负源地图终止符，并保持标志设置的首个匹配源地图行；
2. 无选择地图切换用例保留传入地图，而不声称发生地图加载；
3. 战斗候选查询通过当前地图状态解析地图 `-1`、要求解锁标志，并保留触发 X 与 Y 的独立 `-1` 通配；
4. 已准入战斗候选在检查完成前写入区域 X/Y/宽/高，已完成匹配清除其解锁标志；
5. 无匹配战斗查询报告索引 `-1`，而不发明调用方可视错误或重试策略；
6. 399 前存档点路线保留成组符号游戏开始常量，而普通路线扫描四字节行到 `-1` 终止符并保留 `(1, 1, UP)` 缺失地图回退；
7. 标志 64 门控独立四字节木筏重置表，而不指定未接受持久性或世界状态含义；
8. 私有表内容/基数、故事可达性、存档持久性、战斗与地图生命周期、加载、碰撞、呈现、时序与畸形输入行为保持分别测试或显式 **未知**；
9. 公开 parity 工件使用合成行与结构元数据，而非原版版权表载荷。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 有序六字节地图切换扫描、负终止符、首个准入替换 | **已确认静态** | `sf2-common-maps-static-v1`（[`common-maps-static-v1.json`](../../../../tests/fixtures/h2/common-maps-static-v1.json)） | 私有行/计数、标志含义、下游地图加载 |
| 当前地图哨兵、解锁/通配准入、区域写入/完成顺序、无匹配 `-1` | **已确认静态** | `sf2-common-maps-static-v1`（[`common-maps-static-v1.json`](../../../../tests/fixtures/h2/common-maps-static-v1.json)） | 坐标语料、调用方分支、战斗生命周期/结果 |
| 399 前游戏开始路线、四字节存档点扫描、回退、标志-64 木筏门 | **已确认静态** | `sf2-common-maps-static-v1`（[`common-maps-static-v1.json`](../../../../tests/fixtures/h2/common-maps-static-v1.json)） | 数字游戏开始常量、私有行、持久性与可见状态 |
| 完整 map-data 表语料 | **排除聚合所有者** | `sf2-map-data-static-v1` 与全部 `map.data.*` 记录不被消费；[Issue #99](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/99) 仅修正轨迹 | 解决不自动扩展本合同或其关联 |
| 故事、存档、战斗、加载、碰撞、UI、音频、时序、畸形输入 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得从辅助局部静态控制推断完整地图入口体验 |

## 复现

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 verify
```
