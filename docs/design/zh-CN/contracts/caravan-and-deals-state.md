# 车队与 Deals 状态合同

- 状态：**已确认车队与 Deals 状态的静态存储与边界行为**
- 证据日期：2026-08-08
- 范围：已接受车队物品归一化/压缩边界与打包 Deals 计数边界的实现无关表示，不指定服务、运行时、持久性、呈现或平衡含义

> 本文件是 [`caravan-and-deals-state.md`](../../contracts/caravan-and-deals-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同定义两个低级物品状态存储。它不定义物品如何获取、出售、掉落、装备、显示、保存或估值。

- **已确认**：车队添加剥离物品状态位；对满车队的添加被忽略；车队移除压缩列表并在尾部写入 `ITEM_NOTHING`；Deals 每字节存储两个四位计数；Deals 添加饱和；Deals 零移除被忽略。
- **推断**：无。服务意图与调用方可视行为刻意不从存储辅助推断。
- **独立所有者已确认**：同一已接受 common-stats 所有者确认 `newGame.clearsFlagsDealsAndCaravan=true`、标志、Deals 与车队的源码静态 `NewGame` 清除路线。该相邻排序/集成事实仍归 `stats.new-game`，不由本合同消费或关联。
- **未知**：数字物品状态掩码；车队容量；Deals 物品域、字节数与物品到打包字段映射；数字返回值与条件码使用；自然/运行时调用方可达性；事务排序或原子性；调用方可视/运行时重置结果；存档/读档持久性；菜单、消息与其他呈现；以及经济或平衡意图。

[item-definition-data 合同](../../contracts/item-definition-data.md) 拥有物品身份与固定数据，而非可变所有权。[service-interactions 合同](../../contracts/service-interactions.md) 拥有受限源码静态商店、车队、教堂与铁匠动作顺序，而非这些存储的最终状态。[交战解决合同](../../contracts/combat-resolution.md) 拥有其已接受战斗掉落与 Deals 路由子集。[存档系统合同](../../contracts/save-system.md) 拥有已接受存档结构与动作回放，而非每个车队或 Deals 变更在每个生命周期路径存活的证明。

## 证据所有者

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。

fixture 拥有六个已接受 `expected.statsFacts.inventories` 事实，并绑定代表 `AddItemToCaravan` 地址 `39484` 与 `GetDealsItemAmount` 地址 `39390`。所有者文章提供显式双四位计数措辞与符号 `ITEM_NOTHING` 尾身份。这些是静态存储事实，不是经菜单、战斗奖励、存档或新游戏路径的运行时观察。

common-scripting 与 battle-functions 聚合被刻意排除。common-stats 所有者中的兄弟队伍、物品清单、名称/物品/法术查找、新游戏与未使用记录也在本合同 research-index 边界之外。排除 `stats.new-game` 把它已接受的静态清除事实保留为独立所有者证据；它不使该事实 未知。

## 车队存储边界

**已确认静态：** 进入车队添加辅助的物品在其存储物品身份被使用前剥离状态位。已接受所有者不通过本合同暴露数字掩码。因此兼容适配器必须同时保留调用方提供的值与归一化存储身份，而不发明掩码或给被移除位指定含义。

满边界处的添加被忽略。这只是低级存储结果：证据不定义返回码、调用方重试、消息、替代去向、退款或回滚。

移除保留有序压缩列表：剩余条目被压缩，空出的尾部用符号 `ITEM_NOTHING` 写入。已接受边界不提供数字容量、`ITEM_NOTHING` 数字值或调用方可视所选槽合同。重制必须保持压缩与尾清除可区别于无序集删除。

| 车队面 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 添加归一化 | 存储前剥离物品状态位 | 数字掩码与被移除位含义 **未知** |
| 满边界添加 | 忽略该添加 | 容量、返回值、消息与回退路线 **未知** |
| 移除 | 压缩剩余条目 | 选择溯源与调用方可视顺序 **未知** |
| 空出尾部 | 写入符号 `ITEM_NOTHING` | 数字哨兵与总槽数 **未知** |

## Deals 打包计数边界

**已确认静态：** Deals 在每个字节存储两个四位计数。即使现代实现不使用打包字节，两个计数也必须保持独立可寻址逻辑字段。本合同不指定物品 ID 范围、数组长度或物品到字段映射。

已接受上边界处的添加饱和。零处移除被忽略。这些是静态辅助行为。已接受所有者不建立哪些调用方在运行时到达任一边界、它们返回什么，或包围事务是否成功。

| Deals 面 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 存储打包 | 每字节两个四位计数 | 字节数、物品域与字段映射 **未知** |
| 添加边界 | 饱和所选计数 | 调用方可视成功与包围事务 **未知** |
| 零移除 | 忽略该移除 | 返回值、消息与重试策略 **未知** |
| 相邻计数 | 保持独立打包字段 | 不暗示物品身份或经济关系 |

## 跨系统分离

低级存储本身不构成经济：

- 物品定义决定哪些身份与标志存在，而非谁拥有物品；
- 服务菜单可以按源码静态顺序调用存储辅助，但其最终状态与取消行为此处未在运行时闭合；
- 战斗掉落证据只拥有其观察接收方与 Deals 路线，而非每个生产者；
- 成员持有物品栏与装备是独立存储；
- 已接受源码静态 `NewGame` 清除路线与排序/集成仍归其独立所有者，此处不消费；调用方可视/运行时重置结果与持久性为 **未知**；
- UI、对话、音频、替换资源、定价、稀有度与平衡需要其自身合同或显式现代设计决定。

不应仅因调用路径共享物品身份或存储辅助名而把它提升进本合同。

## 实现无关状态模型

```text
CaravanStore
  orderedEntries[]
  tailEmptyIdentity: ITEM_NOTHING

CaravanAddOperation
  rawItemValue
  normalizedStoredIdentity
  admissionBoundary: available | full
  fullBoundaryOutcome: ignore

CaravanRemoveOperation
  selectedStoredEntry
  compactRemainingEntries
  writeTailIdentity: ITEM_NOTHING

DealsStore
  packedBytes[]
  countFieldsPerByte: 2
  countFieldWidthBits: 4

DealsMutation
  kind: add | remove
  selectedCountField
  addUpperBoundary: saturate
  removeZeroBoundary: ignore
```

这是逻辑一致性模型，不是必需引擎内存布局。数组长度、数字掩码、哨兵值、物品域映射与调用方返回形状刻意缺席，因为已接受合同不闭合它们。

现代引擎可以把车队条目存储为类型化物品身份、把 Deals 计数存储为普通整数。其兼容适配器仍必须复现状态位归一化、有序压缩、尾清除、独立四位字段边界、饱和与零移除行为。

## 原版保真与现代化

原版保真模式保留六个已接受存储事实与代表所有者身份/地址。它报告未知容量、映射、调用方与持久性行为，而非用来自高层菜单或经济设计的假设填充它们。

现代重制可以选择更大存储、显式结果类型、非打包计数器、事务服务命令、事件日志、更清晰失败消息或不同经济平衡。除非适配器复现已接受原版面向边界，否则那些选择是刻意偏差。

原版物品名、描述与其他版权内容对本公开合同不必要。公开一致性数据应只保留结构元数据与合成值。

## H4 验收门

未来重制车队/Deals 适配器只在以下情况通过本合同：

1. 车队添加把调用方提供的物品值与状态剥离存储身份分别保留，而不发明合同级数字掩码；
2. 满边界车队添加被忽略，而不指定未确认返回、消息或回退去向；
3. 车队移除保留有序压缩并在空出尾部写入符号 `ITEM_NOTHING`；
4. 每个 Deals 存储字节暴露两个独立四位计数字段，而不发明物品域或字段映射；
5. Deals 添加饱和其所选字段，零移除被忽略；
6. 单独已接受的源码静态 `NewGame` 清除排序/集成保持追溯到 `stats.new-game` 且在本合同之外，而运行时可达性、服务/掉落集成、调用方可视重置结果、持久性、UI 与平衡保持分别测试或显式 **未知**；
7. 公开 fixture 与测试使用结构元数据与合成值，而非版权物品内容。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 状态位剥离与满添加忽略 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 数字掩码、容量、返回/调用方结果 |
| 车队压缩与 `ITEM_NOTHING` 尾写入 | **已确认静态** | `sf2-common-stats-static-v1` 加[所有者文章](../../../research/common-stats.md) | 数字哨兵、所选槽 ABI、运行时结果 |
| 每字节两个四位 Deals 计数 | **已确认静态** | `sf2-common-stats-static-v1` 加[所有者文章](../../../research/common-stats.md) | 物品域、总字节数、物品到字段映射 |
| Deals 添加饱和与零移除忽略 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 运行时可达性、返回值、事务结果 |
| 源码静态 `NewGame` 清除标志、Deals 与车队 | **独立所有者已确认静态** | `sf2-common-stats-static-v1`；`stats.new-game` 保持排除 | 排序/集成此处不消费；运行时重置结果与持久性保持 **未知** |
| 服务/掉落/存档/UI/平衡与调用方可视重置语义 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得从存储辅助推断高层行为 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
