# 战斗过场路由合同

- **已确认原版结构：** 四个独立的 48 槽战斗过场路由表、59 个构建过场源程序、十个路由包装文件、intro/completion/leader/region 门顺序、共享战后加入交接，以及下文描述的受限清理与位置准备接缝。
- **推断原版行为：** 空路由目标的调用方可视含义，以及源码名、标志与程序内容的叙事含义。
- **未知原版行为：** 渲染命令时序、完整 MAPSCRIPT 效果、自然可达性与可重复性、已播放/已完成状态的持久性，以及端到端故事后果。
- 重制状态：实现无关 Phase 3 合同；尚未选择过场运行时、编写语言、时间线模型、跳过策略或刻意兼容偏差。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`battle-cutscene-routing.md`](../../contracts/battle-cutscene-routing.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义战斗生命周期钩子与战斗编写的 map-script 程序之间的静态选择与准入边界。它拥有：

1. 路由表身份、槽形状与空/非空计数；
2. 战前与开战包装器使用的共享 intro 标志门；
3. 战后与败敌包装器使用的已完成战斗与队长死亡门；
4. 区域记录扫描与标志/触发准入顺序；
5. 源程序语料边界，包括其构建与排除文件；
6. 显式闭合的有序死亡列表、加入调用与位置准备接缝。

它不拥有外层 `BattleLoop` 调度、MAPSCRIPT opcode 语义、对话或名册变更、呈现、战役时间线或产品级叙事设计。那些仍属于相邻合同、研究所有者或 **未知**。

可执行证据所有者是：

- `sf2-battle-cutscene-data-static-v1`，位于
  [`tests/fixtures/h2/battle-cutscene-data-static-v1.json`](../../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json)；
- `sf2-battle-cutscenes-static-v1`，位于
  [`tests/fixtures/h2/battle-cutscenes-static-v1.json`](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)；
- `sf2-battle-routing-data-static-v1`，位于
  [`tests/fixtures/h2/battle-routing-data-static-v1.json`](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)。

研究所有者是[battle cutscene data](../../../research/battle-cutscene-data.md)、[battle cutscene routing](../../../research/battle-cutscenes.md)与[battle routing 与 terrain data](../../../research/battle-routing-data.md)的仅过场部分。

## 合同前证据审计

本综合对照三个测试夹具、其 H2 验证器、全部十个固定路由源文件、六个过场数据表与存储 include 图检查。审计保留以下限制：

- routing-data 测试夹具中的 `routeTargetsParsed` 为 false；表占用已确认，但精确公开路由槽到程序映射没有；
- 87 个不同命令名是跨源语句计数的首 token，不是闭合的 MAPSCRIPT opcode 注册表，也不是所有 token 经同一解释器执行的证明；
- 受追踪程序测试夹具保留地址、聚合计数与 hash，而非对话、编排或其他提取内容；
- 未汇编的 Battle 01 区域源是孤儿，不是第四个构建区域程序；
- 外层生命周期调用顺序仍归[battle-control 合同](../../contracts/battle-control-lifecycle.md)所有。

审计最初在 leader-position 所有者中发现两个聚合过度声称。Issue #59 修正了可执行合同，本文档现在消费其精确词汇：`offscreenLoop` 为己方 0 到 29 与敌人 128 到 157 设置 X 为 `-1`；`hpZeroLoop` 为那 30 个敌人槽调用 `SetCurrentHp(0)`；`positionOnlyTail` 为敌人槽 158 与 159 设置 X 为 `0`，且两者都不写 HP。这些范围与尾部值现在是 fixture 绑定事实。

## 身份域

实现必须保持以下身份分离：

| 域 | 已确认原版边界 |
| --- | --- |
| 路由表 | 战前、开战、败敌与战后是独立表 |
| 路由槽 | 每个相对指针表包含 48 个有序槽 |
| 战斗槽 | 遭遇/地形主干包含 45 个槽，不是路由命名空间 |
| 区域路由记录 | 一条 8 字节准入记录，扫描直到字终止符 `-1` |
| 构建过场程序 | layout 拥有的存储容器 include 的 59 个源文件之一 |
| 孤儿源 | Battle 01 `cs_regiontriggered_1.asm`，有标签但不在构建图中 |
| map-script 命令 | 程序引用的解释器拥有行为，不由路由准入定义 |

四个路由表可以共享数字槽索引，但不得折叠成一种路由种类。48 个路由槽不得被位置截断或描述为与 45 个遭遇槽相同的命名空间。[遭遇定义合同](../../contracts/battle-encounter-definition.md)从遭遇侧拥有该区分。

## 路由表形状

已接受 routing-data 所有者建立：

| 路由表 | 槽 | 非空目标 |
| --- | ---: | ---: |
| 战前 | 48 | 27 |
| 开战 | 48 | 1 |
| 败敌 | 48 | 3 |
| 战后 | 48 | 25 |

空与非空状态是保真事实。精确目标身份、空目标执行行为、域外索引与正常剧情可达性不由该测试夹具闭合。

区域表包含四个带 longword 的路由记录，后接字终止符。程序清单包含三个构建的区域触发源文件。因为目标解析显式缺席，重制不得仅从这些聚合计数推断一对一路由/程序对应。

## 构建程序语料

layout 拥有的存储容器跨 34 个战斗索引 include 59 个带标签的过场文件。已确认类型计数：

| 源程序类型 | 构建文件 |
| --- | ---: |
| 战前 | 27 |
| 战后 | 25 |
| 战斗结束 / 败敌 | 3 |
| 开战 | 1 |
| 区域触发 | 3 |

解析器计数 5,672 条源语句、87 个不同首 token 名与 59 个 `csc_end` token。源码审计确认 59 个构建文件中各有一个 `csc_end` token，而公开 fixture 只保留聚合。常见 token 包括文本推进、等待、朝向/位置变更、动作脚本与实体动作。

这些是结构清单事实。它们不建立：

- 闭合的可执行 opcode 集；
- 语句时序、并发、跳过行为或呈现；
- 对话、标志、加入、死亡、地图变更或实体动作的含义；
- 每个构建程序自然可达；
- 同一解释器拥有内嵌子程序或实体定义语句。

完整生成命令/文件细节在忽略的 `local/derived/` 输出下保持私有。公开重制仓库不得从聚合计数重构程序内容，也不得再分发提取的源行。

## 共享 Intro 门

战前与开战包装器通过把当前战斗索引加到 `BATTLE_INTRO_CUTSCENE_FLAGS_START` 来计算相同的逐战斗 intro 标志。

**已确认静态战前顺序：** 包装器检查共享标志，已设置时返回。否则从战前相对指针表选择当前战斗条目并调用 `ExecuteMapScript`。它不设置共享标志。

**已确认静态开战顺序：** 包装器检查同一标志，已设置时返回。否则在选择当前战斗条目并调用 `ExecuteMapScript` 之前设置标志。

这证明标志极性与调用顺序。它不证明两个生命周期钩子为何共享一个标志、空目标是否立即返回、标志何时持久到存储，或两个钩子之间的可见时序。外层新战斗顺序仍归[battle control](../../contracts/battle-control-lifecycle.md)。

## 战后路由与加入尾

战后包装器计算当前战斗的已完成标志。标志清除时选择战后路由并调用 `ExecuteMapScript`；标志设置时跳过该脚本。两条路径都到达同一尾部。

共享尾部：

1. 读取当前战斗索引；
2. 从 `table_AfterBattleJoins` 读取一个字节；
3. 用该字节调用 `JoinForce`；
4. 恢复包装器状态并返回。

layout 拥有的表包含 52 个字节且每个字节为零。源码把该特性标记为未使用，但控制流仍到达读取/调用接缝。重复传递零的可见效果、与已在场己方角色的交互、持久性与叙事含义保持 **未知**。保真适配器若建模该包装器，必须保留可观察交接；它不得发明 52 个不同加入结果。

## 败敌准入与清理尾

败敌包装器首先要求 Bowie 当前 HP 非零且敌人槽 128 当前 HP 为零。任一门失败在脚本分发前与共享清理尾之前返回。

两个生命/死亡门都通过时，已完成战斗标志只控制脚本分发：

- 清除标志选择当前败敌路由、调用 `ExecuteMapScript`，然后到达清理尾；
- 设置标志跳过脚本并直接到达同一清理尾。

在尾部，逐战斗敌人队长标志决定是否扫描全部 32 个敌人槽。每个当前 HP 非零的敌人被追加到既有死亡战斗员列表；下一字节写为 `0xFF`，列表长度递增。包装器不建立该列表何时被处理或 HP 何时随后清除。那些归[battle control](../../contracts/battle-control-lifecycle.md)所有。

## 队长死亡位置准备

位置监听器共享 Bowie-HP-非零/敌人-128-HP-零门。它扫描六字节战斗记录直到字终止符 `-1`，然后选择四字节战斗员位置记录表。

已接受可执行边界是：

- `offscreenLoop`：己方 0 到 29 与敌人 128 到 157 接收 X `-1`；
- `hpZeroLoop`：敌人 128 到 157 接收当前 HP 零；
- `positionOnlyTail`：敌人槽 158 与 159 接收 X `0` 且不写 HP；
- 位置表以 `-1` 终止；
- 源码在无条件循环分支后保留不可达死亡列表写入。

公开 fixture 不闭合自然调用方可达性、第四个位置字节、完整逐记录资格、最终可见实体状态或呈现时序。那些保持 **未知**。

## 区域准入

区域包装器按表顺序扫描 8 字节记录。对每条记录执行以下测试：

1. 首字为 `-1` 时停止；
2. 把记录的战斗字节与当前战斗比较；
3. 记录的已播放标志已设置时跳过；
4. 其触发区域标志清除时跳过；
5. 设置已播放标志；
6. 加载程序指针并调用 `MAPSCRIPT` trap。

包装器在被准入 trap 之后没有重扫边；它 fall-through 到恢复并返回。这是静态控制流，不是自然触发顺序、标志持久性、脚本返回行为、可重复性或渲染排序的运行时证明。

区域准入不同于敌人区域激活与生成准入。那些仍归[battle-control 合同](../../contracts/battle-control-lifecycle.md)，而非本过场路由表。

## MAPSCRIPT 与跨系统边界

路由包装器把控制转移到共享 map-script 系统。本合同在所选指针、调用/trap 接缝、分发前标志与显式确认的分发后尾部停止。

地图加载、块/实体操作、摄像机与屏幕命令仍归[地图探索](../../contracts/map-exploration.md)。对话处理器仍归[对话合同](../../contracts/dialogue-system.md)，force/active-party 变更仍归[队伍/名册合同](../../contracts/party-roster-state.md)。资源准备与渲染战斗演出行为仍归[battle-scene presentation](../../contracts/battle-scene-presentation.md)。

程序名与注释不是每个被引用效果都已被解析或观察的证据。端到端故事副作用在对应命令所有者与自然调用方路径被显式连接之前保持 **未知**。

## 保真与现代化边界

原版保真路由层必须保留：

- 四个独立 48 槽路由表及其空/非空状态；
- 共享 intro 标志，包括战前仅检查与开战先设置后分发；
- 战后完成门加无条件共享加入尾；
- 败敌路由生命/死亡/完成门与有序死亡列表追加/终止符写入；
- 区域表扫描顺序、已播放/触发标志极性及先设置后 trap 顺序；
- 59 文件构建语料与存储容器及 Battle 01 孤儿作为程序的排除；
- 路由准入与 MAPSCRIPT 执行之间的分离。

未来重制可以使用类型化路由记录、时间线编辑器、去重程序资源、可跳过场景、新触发类型或编写叙事状态。那些是产品决定。新的或变更的路由必须表示为重制内容或显式兼容偏差，而不是静默归于原版。

## H4 验收面

未来 H4 适配器应使用合成路由在场与状态，而非提取的对话或图形。它应比较：

1. 四个 48 槽在场向量与四记录区域表；
2. 清除与设置 intro 标志的战前/开战决定与标志写入顺序；
3. 战后分发/跳过决定加共享加入调用参数；
4. 败敌路由提前退出、完成相关分发与死亡列表追加顺序；
5. 队长位置记录选择加精确 `offscreenLoop`、`hpZeroLoop` 与 `positionOnlyTail` 范围与值；
6. 区域扫描决定与 MAPSCRIPT trap 前立即的标志状态；
7. 构建/排除程序身份、类型计数与聚合命令形状元数据。

H4 不得要求受追踪提取对话、编排、地图资源或源程序。自然故事可达性、呈现时序、持久性与完整脚本效果在单独证据或刻意设计之前保持在该适配器之外。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 构建/排除程序语料、类型与命令形状计数 | **已确认静态** | `sf2-battle-cutscene-data-static-v1`（[`battle-cutscene-data-static-v1.json`](../../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json)） | 程序内容、可达性、命令语义、时序、故事效果 |
| intro/战后/败敌/区域包装器顺序与受限变更接缝 | **已确认静态** | `sf2-battle-cutscenes-static-v1`（[`battle-cutscenes-static-v1.json`](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)） | 自然调用方、持久性、后期位置资格与 MAPSCRIPT 效果 |
| 四个路由表形状、区域计数与零填充加入表 | **已确认静态** | `sf2-battle-routing-data-static-v1`（[`battle-routing-data-static-v1.json`](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)） | 精确路由目标与空目标行为；地形记录排除 |
| 外层战斗生命周期 | **独立所有者** | [Battle-control lifecycle](../../contracts/battle-control-lifecycle.md) | 不得从路由表推断包装器调用时序 |
| 对话、实体/force/地图效果、呈现与故事含义 | **独立所有者 / 未知** | 专用合同与未来证据 | 无聚合过场测试夹具闭合端到端行为 |

## 复现

```powershell
uv run sf2 h2 battle-cutscene-data
uv run sf2 h2 battle-cutscenes
uv run sf2 h2 battle-routing-data
uv run sf2 design-contracts test
uv run sf2 research-index test
```
