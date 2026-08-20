# 队伍成员状态合同

- 状态：**已确认静态队伍成员状态与受限辅助时间线**
- 证据日期：2026-08-08
- 范围：已加入部队与活跃队伍成员状态、计数前缀重建与源审查成员辅助的实现无关重构，不导入 map-script 命令行为、持久性、UI、名册选择策略、呈现或平衡含义

> 本文件是 [`party-membership-state.md`](../../contracts/party-membership-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从 `battleparty.asm` 中的源码形状队伍状态辅助开始。它不定义故事脚本如何选择成员、玩家如何编辑名册，或成员变更何时变得持久或可见。

- **已确认 fixture 拥有事实**：已加入成员与活跃战斗队伍成员使用独立标志范围；`UpdateForce` 构建已加入部队、活跃队伍与后备列表；`JoinForce` 在源定义部队最大值以下自动激活；`LeaveForce` 把所选战斗员移出地图。
- **已确认直接源码审查**：六个辅助身份及其受限指令/调用顺序；`UpdateForce` 写入计数前缀及其计数而无观察到的后备尾清除；`JoinForce` 设置已加入成员、重建、检查容量并条件调用 `JoinBattleParty`；三个活跃队伍辅助检查、设置或清除活跃标志；两个离开辅助保留不同源码形状 X 写入。
- **未知**：数字标志索引、部队容量、己方基数、畸形选择器行为、调用方可视条件码含义、`JoinBattleParty`、`LeaveForce` 或 `LeaveBattleParty` 后的自动列表同步、正常剧情可达性、死亡/状态交互、存档/读档持久性、map-script 命令结果、AI/随从行为、UI 与玩家名册选择策略、呈现与平衡意图。

[party 与 roster state 合同](../../contracts/party-roster-state.md) 拥有十个已接受 map-script 命令形式及其受限处理器局部运行时观察。[global flag 合同](../../contracts/global-flag-state.md) 拥有通用标志索引寻址，[new-game initialization 合同](../../contracts/new-game-state-initialization.md) 只拥有其已接受初始化顺序边。本合同在借用那些相邻合同关联的情况下拥有更低队伍成员状态面。

## 证据所有者与源审计

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。

fixture 绑定 `UpdateForce`，其十进制地址为 `39168`，并拥有恰好四个 `expected.statsFacts.party` 事实。那四个语义事实不被静默扩展成完整运行时合同。

对固定上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6` 的独立只读审计审查了 `code/common/stats/battleparty.asm`，从 `UpdateForce` 到 `LeaveBattleParty`。该直接源审查提供下文辅助子结构与时间线。它不把运行时可达性、调用方解读、持久性或呈现升级为已确认证据。

活跃 Issue #73 battle-functions 聚合与 fixture `sf2-battle-functions-static-v1` 被刻意排除。Common scripting、map-script 命令记录、新游戏、通用标志、combatant-state、存档、菜单与 gameflow 记录也在本合同 research-index 关联边界之外。唯一未来关联是 `stats.party`。

## 成员域

**已确认静态：** 已加入部队成员与活跃战斗队伍成员是独立标志域。因此成员可以已加入而不活跃。后备成员在已接受源形状中不是第三个持久标志域；`UpdateForce` 从活跃标志清除的已加入成员派生后备计数前缀。

合同使用符号标志范围。它不把数字标志基址、成员计数或容量值复制进实现无关层。那些值保持导入数据或显式未合同证据，而非硬编码设计含义。

| 逻辑状态 | 已接受源角色 | 刻意边界 |
| --- | --- | --- |
| 已加入成员 | 选择部队/目标前缀中的成员 | 故事可用性与持久性 **未知** |
| 活跃成员 | 把活跃成员与后备分开 | 玩家选择、死亡规则与容量策略 **未知** |
| 后备分类 | 已加入设置且活跃清除时在 `UpdateForce` 期间派生 | 不声称独立后备标志 |

## 计数前缀重建

**已确认静态：** `UpdateForce` 扫描源己方域并重建三个计数前缀：

1. 每个已加入成员被追加到部队/目标前缀；
2. 带活跃标志的已加入成员被追加到活跃队伍前缀；
3. 无活跃标志的已加入成员被追加到后备前缀；
4. 扫描后写入三个结果计数。

这为一条完成静态例程路径建立前缀内容与计数写入。它不建立任何计数前缀后未使用字节被清除。消费者必须把写入计数当作边界；陈旧后备数组尾不得被提升为成员。

源扫描顺序可以决定前缀顺序，但本合同不导入数字己方基数或推断玩家面向名册排序。并发变更、中断、无效状态与调用方可视部分重建行为保持 **未知**。

## `JoinForce` 时间线

精确已接受源时间线是：

1. 设置所选己方的已加入标志；
2. 调用 `UpdateForce`；
3. 把重建活跃队伍计数与源定义容量常量比较；
4. 低于该容量时调用 `JoinBattleParty` 设置活跃标志。

重建先于条件活跃标志设置。因此本合同不得声称活跃计数前缀或其写入计数在 `JoinForce` 返回时已包含新激活成员。该辅助中没有第二次 `UpdateForce` 调用。调用方是否执行后期重建，以及任何列表何时与新活跃标志同步，保持 **未知**。

“Auto-activates below the force maximum”保留 fixture 拥有关系与源分支顺序。它不提供数字最大值、解释满队伍用户体验、选择替换成员或证明调用方可视成功结果。

## 离开与活跃队伍辅助

直接源审查保持这些操作分离：

- `LeaveForce` 清除已加入成员，然后通过战斗员-X 设置器写入符号 `MAP_NULLPOSITION` 值。它在审查辅助中不调用 `UpdateForce`。
- `IsInBattleParty` 寻址活跃标志并调用共享标志检查。本合同保留操作身份，但不创建新调用方可视 Boolean 或条件码合同。
- `JoinBattleParty` 设置活跃标志。它在审查辅助中不重建任何计数前缀。
- `LeaveBattleParty` 清除活跃标志，然后通过战斗员-X 设置器写入源字面量 `-1`。它在审查辅助中不重建任何计数前缀。

`MAP_NULLPOSITION` 与 `LeaveBattleParty` 字面量 `-1` 保持不同源身份。本合同不假设数字等价、共享意图或相同调用方可视行为。同样，fixture 措辞“moves combatant off map”只为 `LeaveForce` 保留；任一 X 写入的可见或碰撞含义归地图/战斗员所有者与未来运行时证据。

`JoinBattleParty`、`LeaveForce` 与 `LeaveBattleParty` 后的列表与计数同步为 **未知**。现代实现可以维护更强内部不变量，但原版保真兼容必须暴露已接受时间线，且不得假装原版辅助执行了未观察重建或后备尾清除。

## 跨系统分离

队伍成员状态不是完整名册系统：

- map-script 名册/死亡命令保留其自身流布局、处理器时序与 H3 边界；
- new-game 初始化拥有其起始成员加入操作何时被请求，而非最终计数列表结果；
- 全局标志拥有通用存储寻址而非队伍生命周期含义；
- combatant state 拥有低级 X 设置器面而非地图可见性或碰撞；
- 故事招募、名册菜单、队伍容量 UX、存档/读档、AI/随从、死亡/复活与战斗准入需要其自身所有者；
- 呈现、本地化、可访问性与平衡是刻意设计层，不是从这些静态辅助推断的事实。

## 实现无关状态模型

```text
PartyMembershipState
  joinedFlags: symbolic joined-membership domain
  activeFlags: symbolic active-membership domain
  countedPrefixes:
    joinedMembers: { entries, count }
    activeMembers: { entries, count }
    reserveMembers: { entries, count }

rebuildMembershipPrefixes()
  scan source ally domain
  append joined members to joinedMembers
  partition joined members by active flag
  write all three counts
  backingTailClear: notContracted

joinForce(member)
  set joinedFlags[member]
  rebuildMembershipPrefixes()
  if activeMembers.count < sourceCapacity:
    joinBattleParty(member)
  postJoinActivePrefixSynchronization: unknown

leaveForce(member)
  clear joinedFlags[member]
  setCombatantX(member, symbolic MAP_NULLPOSITION)
  prefixSynchronization: unknown

isInBattleParty(member)
  perform activeFlags membership check
  callerVisibleInterpretation: unknown

joinBattleParty(member)
  set activeFlags[member]
  prefixSynchronization: unknown

leaveBattleParty(member)
  clear activeFlags[member]
  setCombatantX(member, source literal -1)
  prefixSynchronization: unknown
```

这是逻辑一致性模型，不是必需引擎内存布局。重制可以在内部使用集合、稳定向量、派生查询、事务或更强同步。其保真适配器必须仍复现两个成员域、计数前缀重建边界、精确 `JoinForce` 时间线与不同离开操作身份。

## 原版保真与现代化

原版保真模式保留四个 fixture 拥有事实、直接审查辅助顺序与代表 `UpdateForce` 身份/地址。它报告调用方、运行时、同步与持久性问题，而非把注释或辅助名当作完整玩家面向名册规范。

重制可以选择即时列表同步、动态容量、显式结果类型、事务名册编辑或不同名册 UI。那些是显式产品决定。如果原版保真适配器保持更强内部不变量，它仍必须在下游兼容测试依赖激活前重建时模拟已接受可观察顺序。

公开一致性 fixture 需要结构元数据、符号身份与合成成员索引；它们不需要版权名称、对话、立绘或其他原版资源。

## H4 验收门

未来重制队伍成员适配器只在以下情况通过本合同：

1. 已加入与活跃成员保持独立逻辑域，而后备成员在已接受重建期间派生，而非发明为必需第三标志范围；
2. `UpdateForce` 重构三个计数前缀并写入其计数，而不要求或声称未观察后备尾清除；
3. `JoinForce` 保留设置已加入 → 重建 → 容量检查 → 条件设置活跃的时间线，且不断言重建活跃前缀已包含新活跃成员；
4. `LeaveForce` 保留符号 `MAP_NULLPOSITION`，而 `LeaveBattleParty` 保留其不同字面量 `-1`，不假设等价；
5. 辅助后列表同步、调用方可视结果、畸形输入、故事可达性、死亡/状态交互、持久性、UI、呈现与平衡保持分别测试或显式 **未知**；
6. 相邻 map-script、标志、新游戏、战斗员、存档与 gameflow 合同保持独立可测试，而非被折叠进本状态层；
7. 公开 fixture 使用结构元数据与合成值，而非版权内容。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 独立已加入/活跃标志；三个重建列表；条件自动激活；`LeaveForce` 离图交接 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 数字常量、运行时结果、调用方含义 |
| 六个辅助身份与受限指令/调用顺序 | **已确认静态源审查** | 固定 `battleparty.asm`，上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6` | 运行时可达性与调用方可视语义 |
| 计数前缀内容与计数写入，无后备尾清除 | **已确认静态源审查** | `UpdateForce` 源体 | 并发/无效状态与可见顺序 |
| 激活后/离开后同步 | **未知** | 未来分组运行时/调用方证据 | 不得发明隐含重建 |
| 故事、存档/读档、名册 UI、AI/随从、死亡/复活、呈现、平衡 | **独立所有者 / 未知** | 相邻合同与未来综合/运行时工作 | 不得推断完整名册体验 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
