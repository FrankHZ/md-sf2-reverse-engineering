# 物品定义与辅助目录合同

- 状态：**已确认静态物品身份、记录打包、辅助目录与受限消费者**
- 证据日期：2026-08-08
- 范围：128 个原版物品身份与固定定义、商店/调试/宝箱/损坏/秘银/车队/野外使用表、武器图形引用及其已接受静态查找边界

> 本文件是 [`item-definition-data.md`](../../contracts/item-definition-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同定义不可变物品数据与辅助查找目录。它不定义完整事务、物品栏变更、持久性、呈现、随机结果或经济平衡。

- **已确认**：128 个有序物品名与带 source/ROM parity 的 16 字节定义；总计 768 字节的九个辅助 ROM 范围；其完整表计数；以及已接受静态商店、宝箱、损坏消息、秘银、车队、野外使用与武器图形消费者规则。
- **推断**：无。源码标签与枚举名保持存储词汇，而非玩家面向含义或设计意图的证据。
- **未知**：全部 30 个商店索引的故事/调试准入；特殊车队呈现；观察到的铁匠频率与订单持久性；畸形索引效果；完整物品使用/装备/损坏流；事务原子性；存档持久性；价格进展；以及平衡意图。

[service-interactions 合同](../../contracts/service-interactions.md) 拥有受限商店、车队/仓库、教堂与铁匠动作顺序。[推进与经济综合](../../synthesis/progression-and-economy.md) 连接已接受资源流而不声称平衡意图。[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)与[battle-scene presentation](../../contracts/battle-scene-presentation.md) 拥有运行时效果与可见战斗行为。本合同提供稳定物品/目录输入。

## 证据所有者

`sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)）证明完整十文件物品源边界、H1 地址、128 个名称、128 个定义与辅助表基数。其带来源所有者是[item、spell 与 enemy data inventory](../../../research/core-stats-data-inventory.md)。

更早的[static core owner](../../../research/static-core-data.md)、[manifest](../../../../manifests/extractions/static-data.json)、[schema](../../../../schemas/static-data.schema.json)与[ROM layout](../../../../manifests/extractions/rom-static-layout.json) 拥有规范物品记录与独立 source/ROM parity。已接受轨道在其更广范围内比较全部 281 个固定宽度记录且零不匹配；本合同只消费其 128 个物品行。

`sf2-item-auxiliary-static-v1`（[`item-auxiliary-static-v1.json`](../../../../tests/fixtures/h2/item-auxiliary-static-v1.json)）是专用辅助所有者。它逐字节比较九个地址范围、清单八个源文件与七个消费者文件，并记录下文总结的完整查找边界。生成行级内容在 `local/derived/` 下保持私有。

## 物品身份与固定记录

原版物品身份域恰好是 `0..127`。有序名称与定义表各包含 128 行。每个定义占用 16 字节：

| 存储字段 | 宽度或打包 |
| --- | --- |
| 装备标志 | 大端 32 位值 |
| 最大与最小范围 | 各一个字节 |
| 价格 | 大端字 |
| 物品类型 | 字节 |
| 使用法术 | 六位法术 ID 加两位法术等级 |
| 三个效果/参数对 | 共六字节 |

这些是存储事实。装备标志含义、范围几何、法术执行、效果分发与价格使用保持独立消费者行为，除非专用所有者确认它们。

物品 ID 127 保留一个重要表示区分：其枚举码是 `NOTHING`，而名称/定义注释使用 `Empty`。无损导入保持稳定数字 ID、枚举码、原始名称表达式与显示资源分离，而非把它们折叠成一个语义字符串。

## 商店与调试目录

商店目录包含 30 条计数前缀记录：15 个武器商店与 15 个物品商店。跨那些记录有 235 个物品引用与 15 个唯一物品栏行内容。索引 0 选择首记录；更高索引从列表开头跳过那么多条计数前缀记录。这建立存储遍历，而非故事可用性或菜单行为。

调试商店目录存储一个计数字节后接全部 128 个物品索引。零售玩家路线能否进入它，以及那里适用什么事务策略，在本合同之外。

实现无关导入应把每个计数前缀行变成显式有序物品列表，同时保留原版商店索引与行溯源。它不得仅因只有 15 个行内容唯一就去重 30 个商店身份。

## 宝箱、损坏消息与野外使用表

辅助所有者确认：

- 13 个宝箱金币档位。消费者选择 `word[(itemIndex-128)&127]` 而无本地边界检查；
- 25 条物品损坏消息规则。匹配物品字节把其存储偏移加到已选基础消息；
- 线性字节允许列表中以 `255` 终止的九个野外可用物品 ID。

这些规则是受限查找合同。它们不证明调用方验证、消息渲染、金币转移原子性、消费策略、目标资格或持久性。重制可以使用验证类型化查找，但原版保真测试必须保留已接受索引算术、终止与偏移关系。

## 秘银目录

秘银数据包含九个职业组与八个武器行。每个武器行存储四个选择，共 32 个存储选择。已接受消费者边界是：

1. 职业组 `0..7` 选择其对应武器行；
2. BRN 与 RDBN 占据组 8，在直接 `0..7` 扫描之外；
3. 该回退选择行 0 或行 2；
4. 所选行按该顺序测试分母 `16, 8, 4, 1`。

这是静态选择算法，不是观察分布或完整铁匠订单生命周期。RNG 状态、呈现、价格/支付、物品移除、订单持久性与履行仍归他处所有或 **未知**。

## 特殊车队与武器图形

特殊车队表包含一个已接受描述条目。匹配显示其存储的连续消息计数。消息内容、窗口时序、正常可达性与普通仓库描述的交互在本数据合同之外。

武器图形表包含对应物品索引 `26..109` 的 84 行。每行存储两个精灵与调色板选择的带符号字节；18 行使用无精灵值。已接受消费者只准入该包含范围内带已装备物品的己方角色、使用 `itemIndex-26`，并为每个被拒绝用例返回 `-1/-1`。因此存储字节 `255` 解码为带符号 `-1`。

该表建立引用，而非解码美术、动画、调色板组合、手对齐或可见时序。那些仍是呈现/图形关注点。

## 实现无关导入模型

完整逻辑导入保持物品身份与目录和消费者不同：

```text
ItemDefinition
  itemId
  enumCode
  rawNameExpression
  displayResourceRef
  equipFlags
  minRange, maxRange
  price
  itemType
  useSpellRef
  effectParameterPairs[3]

ShopCatalog
  shopIndex
  category
  orderedItemIds[]
  sourceRowProvenance

DebugShopCatalog
  orderedItemIds[128]

MithrilCatalog
  classGroups[9]
  weaponRows[8][4]

ItemAuxiliaryCatalog
  chestGoldTiers[13]
  breakMessageRules[25]
  fieldUseAllowlist[9]
  specialCaravanDescriptions[1]
  weaponGraphicsRows[84]
```

该记法是逻辑合同，不是必需引擎类布局。原版 ID、顺序、打包、重复商店行、哨兵、带符号值与源溯源必须保持可用于 parity 诊断。运行时服务通过独立接口消费这些记录。

## 原版保真与现代化

原版保真模式保留全部 128 个身份与定义字段、全部辅助行顺序、尽管内容重复的商店身份、查找算术、哨兵、秘银选择顺序与武器图形带符号值。它还保留不可变数据与运行时变更之间的区分。

现代物品栏 UX、分类商店、显式边界检查、修订价格、重平衡物品效果、确定性制作、新装备槽与替换内容是刻意设计层。它们必须分别测试与报告，而非呈现为关于原版表的证据。

生成物品名、完整定义、清单、消息引用与图形保持私有原版内容。公开 fixture 只保留结构元数据、计数、hash 与受限规则。可分发重制需要替换或单独清除的内容。

## H4 验收门

未来重制物品数据导入器只在以下情况通过本合同：

1. 全部 128 个物品身份、名称、定义、数字 ID 与 16 字节字段值保持无损；
2. 物品 ID 127 保留独立数字、枚举码、原始名称与显示资源表示；
3. 全部 30 个商店身份、235 个引用、计数前缀顺序、15 个唯一内容与完整 128 物品调试目录确定性导入；
4. 全部 13 个宝箱档位、25 条损坏消息规则与九个终止野外使用 ID 保留其已接受查找算术与哨兵/偏移边界；
5. 全部九个秘银职业组、八个四选择行、回退行与分母顺序保持可复现，而不声称观察 RNG 分布；
6. 一个特殊车队条目与全部 84 行武器图形保留身份、带符号字节解码、物品索引范围、被拒绝结果与 18 个无精灵行；
7. 原版兼容私有数据可以确定性导入，而公开工件只暴露已清除内容或非表达元数据；
8. 事务顺序、物品栏变更、效果、RNG、持久性、呈现、经济平衡与现代化由独立所有者测试或报告为刻意偏差。

H4 不需要运行时原版字节表。它需要保留溯源的导入，能解释每个身份、目录行、关系与已接受查找结果。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 十文件物品源边界与完整表基数 | **已确认静态** | `sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)） | 完整运行时消费者与设计意图 |
| 128 个名称、固定 16 字节定义与 source/ROM parity | **已确认静态** | [static core owner](../../../research/static-core-data.md)、[manifest](../../../../manifests/extractions/static-data.json)与[ROM layout](../../../../manifests/extractions/rom-static-layout.json) | 完整字段语义与物品使用/装备行为 |
| 九个辅助范围、768 parity 字节、表计数与受限消费者 | **已确认静态** | `sf2-item-auxiliary-static-v1`（[`item-auxiliary-static-v1.json`](../../../../tests/fixtures/h2/item-auxiliary-static-v1.json)） | 调用方准入、呈现、RNG 观察、持久性 |
| 商店/车队/铁匠事务顺序 | **独立所有者** | [service interactions](../../contracts/service-interactions.md) | 完整运行时边界、持久性与 UX |
| 战斗效果、法术使用、奖励与图形呈现 | **独立所有者** | [交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)与[battle-scene presentation](../../contracts/battle-scene-presentation.md) | 完整物品效果分发与可见输出 |
| 定价曲线、物品平衡、制作策略、替换内容 | **未知 / 刻意设计** | 未来综合、模拟与内容所有者 | 不得从存储目录推断意图 |

## 复现

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 h2 item-auxiliary
pwsh ./scripts/Test-StaticExtraction.ps1
pwsh ./scripts/Test-RomStaticParity.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
