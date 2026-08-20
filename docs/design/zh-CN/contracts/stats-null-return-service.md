# 属性 Null 返回服务合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对下文描述的受限源/文件/条目身份与唯一立即返回指令）
- 现代化：**允许** 擦除、内联或保留引擎原生 no-op 兼容接缝
- 未知：自然可达性、调用方准入、运行时结果、原版栈/寄存器/CCR 行为、时序、玩法含义，以及重制是否需要可调用端点

> 本文件是 [`stats-null-return-service.md`](../../contracts/stats-null-return-service.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同保留已接受 common-stats 清单中最小的完整源所有者：`unusedsub_9482.asm`。该文件包含一个全局条目 `nullsub_9482` 与一条解析语句 `rts`。合同记录该源身份，而不把上游词“unused”当作死代码或通用运行时不可达性的证明。

证据作为溯源与兼容边界有用。它闭合战斗员距离辅助与升级源之间的双字节区间，并让未来私有保真适配器解释原版条目为何存在，即使现代引擎选择不暴露它。

## 判断边界

**已确认静态：** `sf2-common-stats-static-v1` 把 `nullsub_9482` 绑定到 ROM 地址 `0x9482`（`38018`），并识别 `unusedsub_9482.asm` 为其代表源。固定源与 H1 列表把条目限定到 `0x9482..0x9484`，唯一主体指令身份 `rts` 在 `0x9482`。已接受源清单记录该文件 13 条源行、一条解析语句、一个全局标签、零局部标签、零传出直接调用与零间接调用站点。

区间边界精确且不重叠：`GetDistanceBetweenCombatants` 在排他地址 `0x9482` 结束；`nullsub_9482` 占用 `0x9482..0x9484`；下一个 include 的 `LevelUp` 源在 `0x9484` 开始。

**推断：** 源文件名、符号与注释把条目分类为未使用 null 子程序。单指令体与工程 no-op 返回接缝兼容。这些只是源码词汇与工程解读，不是观察运行时可达性或意图。

**未知或排除：** 自然或调试可达性；间接调用或地址表使用；调用方准入；条目及其跳转接口见证为何存在；观察运行时执行与结果；调用方可视栈、返回地址、寄存器或 CCR 行为；无效返回上下文；中断、周期计数与时序；持久性；玩法、UI、呈现或可访问性含义；以及重制是否需要对应此源身份的任何公开或运行时端点。

## 证据合同

本合同只消费以下来自[`sf2-common-stats-static-v1`](../../../../tests/fixtures/h2/common-stats-static-v1.json)的公开面：

- `function.unusedAddress`；
- `expected.representativeSymbols["unusedsub_9482.asm"]`；
- `upstreamCommit` 与 `romSha256` 溯源。

所属[common-stats 研究](../../../research/common-stats.md)、可执行[`stats.py`](../../../../src/sf2tool/h2/stats.py)与提取[`manifest`](../../../../manifests/extractions/common-stats-static.json) 保留完整源清单与已接受输出摘要。`code/common/stats/unusedsub_9482.asm` 的摘要绑定生成行记录：

| 清单字段 | 已接受值 |
| --- | ---: |
| 源 SHA-256 | `1E729E16223F79D95DB1B77D4FB5E0C369E4DF52CF8B687FB85C31F1FA97EF7A` |
| 源行 | `13` |
| 解析语句 | `1` |
| 全局标签 | `1` |
| 局部标签 | `0` |
| 传出直接调用 | `0` |
| 间接调用站点 | `0` |

受限源形状直接在固定[`unusedsub_9482.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/unusedsub_9482.asm)中审查。已接受 H1 列表独立解析条目与下一排他地址。本合同不声称指令体的逐字节 H1/ROM 一致性，也不发布编码指令字节。

本合同不消费 `expected.statsFacts` 子树。队伍、标志、车队、Deals、战斗员访问、名称查找、法术、新游戏、物品清单、物品属性与其他兄弟行为保持在该源/文件边界之外。

### 精确 research-index 分母

已接受 fixture 直接链接十二条研究记录。本合同恰好改变一个未来语义关联：

| 记录 | 本合同后的设计所有权 |
| --- | --- |
| `stats.unused-null` | 本合同；注册前当前未关联 |
| `stats.caravan` | 不变：`caravan-and-deals-state` |
| `stats.deals` | 不变：`caravan-and-deals-state` |
| `stats.combatant-setters` | 不变：`combatant-state-access` |
| `stats.combatant-type` | 不变：`combatant-state-access` |
| `stats.flags` | 不变：`global-flag-state` |
| `stats.names` | 不变：`name-table-lookup-service` |
| `stats.new-game` | 不变：`new-game-state-initialization` |
| `stats.party` | 不变：`party-membership-state` |
| `stats.spell-stats` | 不变：`spellbook-state` |
| `stats.item-inventory` | 保持未关联且在本合同之外 |
| `stats.item-stats` | 保持未关联且在本合同之外 |

共享聚合 common-stats fixture 不把任何兄弟事实或关联转移给本合同。

## 静态源边界

### 完整所有者文件

完整源体有该受限形状：

```text
entry nullsub_9482 at 0x9482
  immediate-return instruction identity
exclusive end at 0x9484
```

“Immediate return”在本合同中是静态源身份。它意味着 `rts` 之前没有独立源语句、局部分支、显式域内存访问或传出调用。它不意味着该例程在已接受运行时用例中被调用、其调用方提供了有效返回上下文，或所有架构状态不变。

生成行零传出调用计数描述该文件发出的调用。它不说明传入调用、跳转存根、间接可达性或地址表引用。

### 相邻源区间

条目也是持久所有权边界：

| 区间 | 所有者边界 |
| --- | --- |
| 在 `0x9482` 结束 | `GetDistanceBetweenCombatants`，由[`combatant-state-access`](../../contracts/combatant-state-access.md)拥有 |
| `0x9482..0x9484` | `nullsub_9482`，由本合同拥有 |
| 在 `0x9484` 开始 | `LevelUp`，由[`level-up`](../../contracts/level-up.md)合同与推进证据拥有 |

共享边界不把战斗员距离 ABI 扩展进 null 条目，也不把 null 条目变成升级行为的一部分。

### 独立跳转接口见证

固定 `s02_jumpinterface.asm` 包含 `j_nullsub_9482`，其地址为 `0x81F4`，源体跳转到 `nullsub_9482`。[technical-interface 研究](../../../research/technical-interfaces.md) 保留该聚合接口源。其存在是外部源见证，不是普通调用方调用该条目的证据。

本合同**不**消费 `sf2-tech-interfaces-static-v1`、不拥有 S02 接口，也不关联 `tech.interfaces.jump-s02`。该记录保持不变且未关联。见证被精确记录以阻止“unused”被改写成“no exported seam”。

## 实现无关模型

私有证据模型可以保留：

```text
StatsNullReturnEvidence
  identity
    sourceSymbol = nullsub_9482
    sourcePath
    h1EntryAddress = 0x9482
    exclusiveEndAddress = 0x9484
    sourceSha256
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  staticBody
    statementCount = 1
    instructionIdentity = IMMEDIATE_RETURN
    localLabelCount = 0
    outgoingDirectCallCount = 0
    indirectCallSiteCount = 0

  separateWitness
    sourceSymbol = j_nullsub_9482
    sourceAddress = 0x81F4
    consumedFixture = NONE
```

该记法是溯源模型，不是必需引擎类型或可调用接口。重制可以擦除条目、内联为无工作或保留类型化 no-op 兼容端点。如果私有兼容适配器暴露此类端点，项目编写合成状态可以验证适配器在返回前不执行域状态变更。

该抽象检查不复现或断言原版栈移动、返回地址读取、寄存器保留、CCR 行为、指令编码、周期时序或无效调用行为。那些架构/运行时问题保持在本静态证据合同之外。

## 公开与私有投影

公开合同可以保留：

- fixture ID 与已接受溯源；
- 源路径与源 SHA-256；
- `nullsub_9482`、条目 `0x9482` 与排他结束 `0x9484`；
- 无编码字节的唯一立即返回指令身份；
- 受限源清单计数；
- 独立 `j_nullsub_9482` 见证身份/地址与显式非消费边界。

公开形式不得发布原始 source/H1/ROM 体、编码指令字节、栈或内存捕获、模拟器轨迹或私有 ROM 摘录。本切片不拥有原版文本、图形、音频、地图或其他内容载荷。

## 原版保真与现代化

原版保真工具保留源/文件/条目身份、区间、单语句形状与溯源。它报告上游“unused”分类而不把它提升为已证明运行时事实。

现代引擎不要求为没有已接受消费者合同的条目分配地址或可调用服务。为诊断保留具名 no-op 适配器同样允许。无论选择什么，它都是现代化/架构决定，不得被描述为原版条目可达或不可达的证据。

## H4 验收面

未来适配器在以下情况满足本合同：

1. fixture 身份、源路径/SHA、上游 commit、已接受 ROM 溯源、条目地址与排他结束保持可追溯；
2. 导入静态模型对完整所有者文件包含恰好一个全局条目、无局部标签、一条立即返回语句、无传出直接调用与无间接调用站点；
3. `0x9482` 与 `0x9484` 边界保持与相邻战斗员距离与升级所有者不相交；
4. 可选引擎原生兼容端点可以通过项目编写合成无域变更检查，而不复现 68000 调用/返回机制；
5. 运行时端点的省略、擦除或内联保持允许，直到独立已接受调用方合同要求它；
6. 没有测试声称死代码状态、自然不可达性、调用方准入、全寄存器/CCR 中性、栈等价、时序或无效返回行为；
7. `j_nullsub_9482` 保持独立接口见证，其 fixture 与 `tech.interfaces.jump-s02` 都不在此消费或关联；
8. 公开输出保持受限元数据，不包含原始源、列表、ROM 或运行时转储。

H4 不需要原版版权载荷。合成适配器状态对可选逻辑 no-op 检查足够。

## 跨系统分离

- [`combatant-state-access`](../../contracts/combatant-state-access.md) 在 `0x9482` 结束其距离辅助，并保留战斗员获取器、设置器、夹断、距离与类型编码。本合同不添加战斗员 ABI。
- [`level-up`](../../contracts/level-up.md) 与推进所有者保留从 `0x9484` 开始的所有行为。
- [Technical-interface 研究](../../../research/technical-interfaces.md) 保留 S02 跳转表与 `j_nullsub_9482` 见证。此处不注册 technical-interface fixture。
- [`graphics-service-state`](../../contracts/graphics-service-state.md)、[`debug-control-flow`](../../contracts/debug-control-flow.md)、脚本所有者与战斗所有者保留其自身名义未使用或 null 条目。相似标签不创造共享行为或所有权。
- `stats.item-inventory` 与 `stats.item-stats` 保持未关联。其大源文件与调用方相关行为不由该完整单语句切片暗示。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| `nullsub_9482` 身份/地址与代表源 | **已确认静态** | `sf2-common-stats-static-v1` | 体字节一致性与运行时调用 |
| 精确 `0x9482..0x9484` 区间与唯一立即返回语句 | **已确认静态 source/H1** | 固定源、H1 列表与 common-stats 所有者 | 栈/寄存器/CCR/运行时行为 |
| 13/1/1/0/0/0 源清单计数与源 SHA | **已确认静态清单** | 摘要绑定 common-stats 生成行 | 传入与间接可达性 |
| “unused null subroutine”工程分类 | **推断** | 上游文件名、符号与注释 | 死代码状态与设计意图 |
| `j_nullsub_9482` 导出接口见证 | **独立所有者静态源** | technical-interface 源/研究 | 调用方准入与运行时使用 |
| 玩法、持久性、UI、呈现、时序 | **未知 / 独立所有者** | 未来调用方/运行时证据 | 完整观察结果 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```

生成清单保留在忽略的 `local/derived/` 下。没有源体转储、列表转储、ROM 摘录、运行时捕获或其他私有/生成工件属于公开合同。
