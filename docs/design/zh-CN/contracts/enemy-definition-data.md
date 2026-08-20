# 敌人定义与呈现索引合同

- 状态：**已确认静态身份、生成基线、呈现索引与地图精灵域**
- 证据日期：2026-08-08
- 范围：103 个原版敌人身份与固定定义、其有序战斗/地图精灵引用、63 行 NPC 地图精灵尾与受限地图精灵查找消费者

> 本文件是 [`enemy-definition-data.md`](../../contracts/enemy-definition-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同重构运行时战斗系统消费的不可变敌人定义数据。它不把源表顺序变成遭遇设计或平衡意图。

- **已确认**：103 个有序名称、103 个固定 56 字节定义、103 个有序战斗精灵行、166 个地图精灵行、完整 source/ROM 字段一致性、零值保留字节、前 103 定义连接、63 行地图精灵尾与未检查敌人地图精灵查找边界。
- **推断**：无。源码标签保持存储词汇；其完整玩法或视觉含义此处不推断。
- **未知**：敌人地图精灵索引 `103..165` 的刻意原版调试或其他非标准可达性；一个已接受名称中内嵌 null 的渲染器可见效果；完整生成变换效果；遭遇与升级选择；AI 行为；回合调度；战斗结果；奖励分配；呈现时序；难度曲线；以及作者或平衡意图。

[battle encounter definition 合同](../../contracts/battle-encounter-definition.md) 拥有战斗局部敌人选择、放置与区域/地形数据。[battle AI 合同](../../contracts/battle-ai-decision.md) 拥有动作与移动决定。[Battle control](../../contracts/battle-control-lifecycle.md)、[交战解决](../../contracts/combat-resolution.md)与[battle-scene presentation](../../contracts/battle-scene-presentation.md) 拥有各自运行时与视觉行为。本文档定义那些系统可以引用的稳定输入数据。

## 证据所有者

`sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)）是主要 H2 清单所有者。它证明五文件敌人数据源边界、代表 H1 地址与四个范围内名称、定义、战斗精灵与地图精灵表的基数。第五个文件包含独立拥有的敌人金币表。其带来源解释是[item、spell 与 enemy data inventory](../../../research/core-stats-data-inventory.md)。

[enemy definitions 研究所有者](../../../research/enemy-promotions.md)、[source manifest](../../../../manifests/extractions/enemy-promotion-data.json)、[source schema](../../../../schemas/enemy-promotion-data.schema.json)、[ROM layout](../../../../manifests/extractions/enemy-promotion-rom-layout.json)与[ROM schema](../../../../schemas/rom-enemy-promotion-data.schema.json) 独立导出源与 ROM 表示。已接受轨道比较 2,722 个字段且零不匹配，并保留下文描述的固定定义布局。该更广所有者中的转职与奖励材料在本合同之外。

`sf2-enemy-map-sprites-static-v1`（[`enemy-map-sprites-static-v1.json`](../../../../tests/fixtures/h2/enemy-map-sprites-static-v1.json)）逐字节比较全部 166 个地图精灵行、证明前 103/尾拆分、审计所有构建战斗与随机升级输入范围，并闭合 `GetCombatantMapsprite` 查找边界。该专用所有者对消费者接缝足够；聚合 common-scripting 清单此处刻意不是证据依赖。

## 敌人身份域

定义索引域恰好是 `0..102`。四个有序表共享该定义身份：

| 表 | 已确认行 | 合同角色 |
| --- | ---: | --- |
| 敌人名称 | 103 | 带原始编码溯源的 身份/显示资源引用 |
| 敌人定义 | 103 | 固定生成基线记录 |
| 敌人战斗精灵 | 103 | 战斗精灵与调色板引用对 |
| 敌人地图精灵 | 166 中前 103 | 探索/战术地图精灵引用 |

跨这些前 103 行的位置连接是原版存储事实。它不证明所有定义出现在构建战斗中、呈现资源唯一，或重制必须用数组位置作其内部外键表示。

一个已接受名称记录（敌人 ID 99）存储以 null 字节结尾的四字节载荷。源与 ROM 导出对该字节一致。无损导入器保留原始载荷与异常标记；显示结果与任何现代化是独立决定。

## 固定生成基线记录

每个敌人定义占用 56 字节，并作为 14 个 longword 复制进生成战斗员条目。已接受解码字段：

| 偏移 | 存储字段 | 宽度或打包 |
| ---: | --- | --- |
| `0` | 源码标记未知值 | 字节 |
| `10` | 源码标记法术威力模式 | 字节 |
| `11` | 等级 | 字节 |
| `12` | 最大 HP | 大端字 |
| `16` | 最大 MP | 字节 |
| `18`、`20`、`22`、`24` | 基础 ATT、DEF、AGI、MOV | 由保留存储分隔的字节 |
| `26` | 抗性 | 大端字 |
| `30` | prowess | 字节 |
| `32` | 四个物品槽 | 四个大端字 |
| `40` | 四个法术槽 | 四个打包字节 |
| `44` | 初始状态 | 大端字 |
| `49` | 移动类型 | 存储字节高半字节 |
| `52` | AI 位域 | 大端字 |

全部 103 个已接受记录中 27 个保留/填充字节的每一个都是零。物品保留七位物品 ID 加装备位。法术保留六位法术 ID 加两位法术等级。全部 103 个初始状态字段存储 `NONE`。这些是存储不变量，不是填充是安全扩展空间或敌人初始化后不能获得状态的声称。

十二条记录存储大于等于 128 的 AGI 字节。源码把高位当作与第二回合路径相关，但本合同保留原始字节而不派生调度语义。回合顺序及其算术边界仍归 battle control。

## 生成变换边界

已接受源码静态 `InitializeEnemyStats` 顺序是：

1. 应用随机战斗升级选择器；
2. 把所选 56 字节定义复制进战斗员条目；
3. 从其最大值初始化当前 HP 与 MP；
4. 把移动类型与战斗局部 AI 指令集状态合并；
5. 应用战斗放置与顺序数据；
6. 按难度调整基础攻击。

因此定义是生成基线，不是最终战斗员状态的保证。重制必须把不可变记录导入与升级选择、战斗局部组合、难度调整、派生属性刷新与状态/装备消费者分开。本文档在保留已接受源码静态交接顺序之外不规定那些变换。

## 地图精灵表与尾

166 字节地图精灵表包含两个不同索引域：

| 索引范围 | 行 | 已确认含义 |
| --- | ---: | --- |
| `0..102` | 103 | 每个敌人定义一行 |
| `103..165` | 63 | NPC 地图精灵尾，不是额外敌人定义 |

尾包含跨 167 到 229 的 62 个唯一地图精灵值。值 189 缺席，值 199 出现两次。没有尾值与定义行值重叠。

构建源域保持低于尾：

- 45 个已接受战斗 spriteset 中的 627 个敌人引用使用索引 `0..102`、触及 102 个唯一定义并省略索引 100；
- 全部五个随机升级范围在索引 84 或以下结束；
- `InitializeEnemyStats` 是已接受源审计中 `SetEnemyIndex` 的唯一具名调用方。

`GetCombatantMapsprite` 检测敌人战斗员、通过 `GetEnemy` 读取其存储敌人索引字节，并在 `table_EnemyMapsprites` 中执行无符号字节查找。查找没有本地边界检查。因此普通构建战斗初始化不能选择尾，而原始、调试、畸形或损坏索引可以。是否有任何原版非标准路线刻意如此保持 **未知**。

## 呈现引用边界

103 个战斗精灵行各存储一个战斗精灵身份与调色板选择器。前 103 个地图精灵行存储地图精灵身份。那些有序引用是 **已确认**；解码图形容器、动画序列、调色板组合、加载器行为与可见时序仍归呈现与图形合同。

两张表都不建立一对一的视觉唯一性。多个敌人定义可以共享精灵族或调色板。现代资源系统可以用稳定资源 ID 替换位置表，但必须为一致性诊断保留原版引用，并把替换或重映射报告为刻意内容决定。

## 实现无关导入模型

完整逻辑导入保持 103 定义域与 166 行地图精灵存储域不同：

```text
EnemyDefinition
  enemyId
  nameResourceRef
  rawNameProvenance
  spawnBaselineRef
  battleSpriteRef
  definitionMapSpriteRef

EnemySpawnBaseline
  raw56ByteProvenance
  unknownByte
  spellPowerMode
  level
  maxHp, maxMp
  baseAtt, baseDef, baseAgi, baseMov
  resistanceBits, prowessBits
  items[4], spells[4]
  initialStatusBits
  movementType
  aiBits

EnemyMapSpriteTable
  definitionRows[103]
  npcTailRows[63]
```

这是逻辑合同，不是必需引擎类布局。导入器不得把尾行暴露为额外敌人定义、丢弃原始名称异常、把保留零字节重新解读为新字段，或把运行时变换压平进不可变基线。

## 原版保真与现代化

原版保真模式保留全部 103 个身份、原始名称编码、56 字节记录字段与填充、物品/法术打包、呈现引用、166 行地图精灵表与精确定义/尾拆分。它还保留基线记录与运行时战斗员之间的区分。

重平衡属性、修订敌人名册、新难度缩放、更改 AI 数据、新奖励、替换名称与替换美术是刻意设计或内容层。未来敌人/玩家数值曲线工作应通过战斗模拟比较导入基线与观察运行时变换；它不得把存储数字描述为预期难度的证据。

生成名称、完整定义行与呈现资源保持私有原版内容。受追踪 fixture 只保留结构元数据、范围、计数、hash 与受限规则。可分发重制需要替换或单独清除的内容。

## H4 验收门

未来重制敌人定义导入器只在以下情况通过本合同：

1. 全部 103 个身份、定义、战斗精灵与定义地图精灵行保留其有序连接与原始数字 ID；
2. 每个定义保留所有解码字段、四个物品、四个法术、打包、27 个零保留字节与完整 56 字节溯源边界；
3. ID-99 名称载荷保留其尾 null 异常而不声称可见结果；
4. 导入定义保持不可变生成基线，与升级、放置/顺序、难度、派生属性、装备与状态变换分开；
5. 全部 166 个地图精灵行保留 `103 + 63` 拆分、尾值范围、缺失/重复事实与未检查查找边界；
6. 普通原版构建输入保持限于定义索引 `0..102`，而非标准尾可达性保持显式且 **未知**；
7. 原版兼容私有数据确定性导入，而公开工件只暴露已清除内容或非表达元数据；
8. 遭遇选择、AI、回合顺序、战斗、奖励、呈现、平衡与现代化由独立所有者测试或报告为刻意偏差。

H4 不需要运行时原版表布局。它要求保留溯源的导入，其变换可回放，其定义 ID 保持可连接到独立战斗、AI、呈现与模拟数据。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 更广五文件敌人源边界内的四个范围内表，103/103/103/166 计数 | **已确认静态** | `sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)） | 敌人金币、完整消费者行为与设计意图 |
| 103 个名称与固定 56 字节定义及 source/ROM 字段一致性 | **已确认静态** | [enemy definitions owner](../../../research/enemy-promotions.md)、[manifest](../../../../manifests/extractions/enemy-promotion-data.json)与[ROM layout](../../../../manifests/extractions/enemy-promotion-rom-layout.json) | 名称渲染与完整生成变换 |
| 166 个地图精灵行、`103 + 63` 拆分、构建输入域与未检查消费者 | **已确认静态** | `sf2-enemy-map-sprites-static-v1`（[`enemy-map-sprites-static-v1.json`](../../../../tests/fixtures/h2/enemy-map-sprites-static-v1.json)） | 非标准尾可达性与可见加载器结果 |
| 战斗选择、放置、区域与局部命令数据 | **独立所有者** | [battle encounter definition](../../contracts/battle-encounter-definition.md) | 运行时准入与故事选择 |
| 动作/移动选择、回合调度、战斗、奖励与呈现 | **独立所有者** | [battle AI](../../contracts/battle-ai-decision.md)、[battle control](../../contracts/battle-control-lifecycle.md)、[交战解决](../../contracts/combat-resolution.md)与[battle-scene presentation](../../contracts/battle-scene-presentation.md) | 端到端多回合行为与玩家可见输出 |
| 敌人数值曲线、名册难度、平衡、替换内容 | **未知 / 刻意设计** | 未来综合、模拟与内容所有者 | 不得从存储定义推断意图 |

## 复现

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 h2 enemy-map-sprites
pwsh ./scripts/Test-EnemyPromotionExtraction.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
