# 己方定义与成长数据合同

- 状态：**已确认静态定义拓扑、存储形状、计数与表不变量**
- 证据日期：2026-08-08
- 范围：原版己方身份、起始记录、职业元数据、呈现引用、转职映射、成长投影、法术学习列表与指针拓扑

> 本文件是 [`ally-definition-data.md`](../../contracts/ally-definition-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同把不可变己方定义数据与消费或变更它的系统分开。

- **已确认**：完整 42 文件己方数据源边界与传递 include 图；表地址与维度；30 个具名身份；32 个起始记录与属性指针槽；32 个职业记录；转职表形状；成长曲线不变量；逐己方职业记录与法术列表结构；以及 30 行己方地图精灵表。
- **推断**：无。movement、resistance、prowess、class type 与 promotion 等源码标签作为存储词汇保留，但本文档不从那些标签推断完整行为。
- **未知**：起始记录槽 30 与 31 的自然可达性；战斗精灵职业条目为 `NONE` 时的可见回退；故事加入顺序；运行时职业/转职准入；名册成员与活跃队伍选择；完整呈现选择；平衡意图；以及任何己方、职业、成长曲线或法术列表的玩家面向价值。

[party 与 roster state 合同](../../contracts/party-roster-state.md) 拥有运行时成员与活跃队伍变更。[升级合同](../../contracts/level-up.md) 拥有属性增益、职业块扫描、法术学习、上限、夹断与战斗员刷新。[service-interactions 合同](../../contracts/service-interactions.md) 拥有受限教堂转职路径。那些动态行为都不在此重新定义。

## 证据所有者

主要可执行所有者是 `sf2-ally-data-static-v1`（[`ally-data-static-v1.json`](../../../../tests/fixtures/h2/ally-data-static-v1.json)）。它清单 `data/stats/allies` 下全部 42 个文件、证明 12 个直接布局 include 加 30 个嵌套属性 include 覆盖完整源边界、每文件解析一个代表 H1 地址，并重新检查下文总结的静态事实。

其详细带来源所有者是[ally 与 class data inventory](../../../research/ally-data-inventory.md)。该清单交叉检查两个更早规范轨道而不替换它们：

- [static core data](../../../research/static-core-data.md)、其[manifest](../../../../manifests/extractions/static-data.json)与[schema](../../../../schemas/static-data.schema.json) 拥有名称、起始记录、职业记录、打包与 source/ROM 一致性；
- [ally growth 与 spell learning](../../../research/ally-growth.md)、其[manifest](../../../../manifests/extractions/growth-data.json)与[schema](../../../../schemas/growth-data.schema.json) 拥有局部生成的行级成长与习得法术表示。

`sf2-map-sprite-assignments-static-v1`（[`map-sprite-assignments-static-v1.json`](../../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json)）是次级消费者所有者。它确认 30 行己方地图精灵表并分类已接受写入器/调用方边界。它不证明故事放置、呈现时序或任意注入地图精灵 ID 的有效性。

## 稳定身份与槽拓扑

原版表暴露三个相关但非同一的域：

| 域 | 已确认形状 | 合同后果 |
| --- | --- | --- |
| 具名己方身份 | 30 个有序名称行、30 个地图精灵行与 30 个属性源文件 | 稳定己方 ID 是 `0..29`；呈现与属性引用可以使用同一有序身份而不暗示故事可用性。 |
| 起始定义存储 | 32 个固定六字节记录 | 无损导入全部 32 条记录；不为槽 30 与 31 发明名称。 |
| 属性指针存储 | 32 个指针、30 个唯一目标 | 槽 `0..29` 指向对应 `AllyStatsNN`；槽 30 与 31 都复用 `AllyStats29`。 |

两个尾部起始记录存储职业 `RDBN`、等级 1 与四个空物品值。其字节与位置是 **已确认**。其目的与运行时可达性是 **未知**。它们也不同于两个尾部属性指针：共享索引位置不证明共享语义。

己方起始记录存储一个职业字节、一个等级字节与四个物品字节。物品位 7 记录装备标志，位 `0..6` 记录物品 ID。这是导入/存储规则，不是特定故事事件原样使用记录或初始装备平衡的证明。

## 职业与转职定义拓扑

源码包含 32 个有序职业名、32 个职业类型行与 32 个固定五字节职业定义。每个定义存储：

1. 一个移动字节；
2. 一个大端抗性字；
3. 存储字节高半字节中的移动类型值；
4. 一个 prowess 字节。

字段边界与值是 **已确认存储事实**。其完整运行时效果由相关移动、战斗、状态与推进合同拥有；重制导入器不得把枚举注释变成额外行为。

关联静态表还包含 16 个两字节会心定义与 15 个铁匠合格职业 ID。这些是共享引用数据。会心解决与铁匠准入/经济保持独立消费者合同。

转职数据是有序关系而非隐含算术规则：

| 段 | 存储行 |
| --- | ---: |
| 常规基础职业 | 12 |
| 常规转职职业 | 12 |
| 特殊基础职业 | 5 |
| 特殊转职职业 | 5 |
| 特殊转职物品 | 5 |

配对段长度与五个物品引用是 **已确认**。调用方是否准入转职、消费物品、更改装备或呈现选择在本静态合同之外。

## 成长与法术列表存储

五个存储成长曲线各包含等级 2 到 30 的 29 行。每行在 256 点标度上存储累积分数与本级分数。对每个已接受行，累积值等于前一累积值加当前增益，且每条曲线以 256 结束。这些是数据不变量，不是随机方差或运行时升级应用量的声称。

30 个己方属性文件包含 59 个职业记录。每个职业记录以职业 ID 开始，后接 HP、MP、攻击、防御与敏捷顺序的五个三字节属性投影。投影存储曲线 ID、起始值与投影 30 级值；曲线 ID 0 是存储 `NONE` 值。

投影后，记录存储两种法术列表形式之一：

- 以 `0xFF` 终止的学习等级与打包法术字节显式序列；或
- 控制字节 `0xFE`，复用首职业记录的显式法术列表。

全部 30 个首职业记录拥有显式列表。跨语料有 52 个显式列表、七个继承列表与 122 个习得法术条目。法术字节保留六位法术 ID 与两位法术等级。已接受指针、哨兵与打包规则必须在导入中存活，即使重制使用更显式内部表示。

## 呈现引用

原版己方定义区域包含 30 个地图精灵行与 90 个战斗精灵/职业/调色板条目，每个具名己方恰好三个战斗精灵条目。这些表建立有序引用，而非通用可见选择规则。

地图精灵赋值所有者确认己方表有 30 行，已接受调用方可以从它派生地图精灵值。脚本与字面精灵写入保持独立路径。以下在此保持 **未知**：

- 哪个故事状态选择每个呈现行；
- 职业条目为 `NONE` 时的战斗精灵行为；
- 调色板、动画、DMA 与帧时序；
- 畸形状态或注入 ID 是否到达加载器；
- 可访问性、替换资源与现代呈现策略。

## 实现无关导入模型

重制可以归一化原版字节布局，但导入边界必须保留独立可寻址记录与溯源。至少，导入定义模型需要：

```text
AllyDefinition
  allyId
  nameResourceRef
  startSlotRef
  statPointerSlotRef
  mapSpriteRef
  battleSpriteEntries[]

AllyStartSlot
  slotId
  allyId?
  classId
  level
  itemBytes[4]

AllyStatPointerSlot
  slotId
  targetRef

ClassDefinition
  classId
  classType
  movement
  resistanceBits
  movementType
  prowessBits

PromotionTables
  regularBaseClassIds[12]
  regularPromotedClassIds[12]
  specialBaseClassIds[5]
  specialPromotedClassIds[5]
  specialPromotionItemIds[5]

AllyClassGrowth
  allyId
  classId
  statProjections[HP, MP, ATT, DEF, AGI]
  spellList = Explicit(entries[]) | InheritFirst
```

该记法是逻辑合同，不是必需引擎类布局。30 个具名 `AllyDefinition` 记录可以引用槽 `0..29`，但两个 32 槽集合保持一等：未命名起始槽保留无发明己方 ID，指针槽 30 与 31 尽管共享一个目标仍保留其独立槽身份。`PromotionTables` 保留五个存储数组而不断言调用方准入或变更行为。原版数字 ID 与原始值必须保持可用于一致性诊断；本地化名称、显示标签与现代化平衡元数据属于独立层。

## 原版保真与现代化

原版保真模式保留已接受身份、表顺序、原始值、指针别名、转职关系、曲线行、投影、列表继承与呈现引用。它还必须保留已知数据与未知可达性之间的区分。

现代名册选择、重平衡成长、修订转职路径、新职业、重命名身份或替代呈现可以是刻意设计工作。此类变更必须表示为显式覆盖或替换数据集并分别测试；它们不得重写导入的原版合同或被呈现为逆向工程意图。

局部生成的行级己方、名称、属性、法术与呈现内容是私有源材料。公开 fixture 只保留结构元数据、地址、计数、hash 与不变量。可分发重制需要替换或单独清除的名称、数字内容与资源。

## H4 验收门

未来重制己方定义导入器只在以下情况通过本合同：

1. 全部 30 个具名身份、32 个起始记录、32 个属性指针与 30 个唯一属性目标保留其已接受顺序与原始身份；
2. 起始记录保留职业、等级、四个物品字节与装备位打包，而槽 30 与 31 保持未命名且不获得发明可达性；
3. 全部 32 个职业名/类型/定义、16 个会心定义、15 个铁匠职业引用、四个转职段与五个特殊转职物品引用保留其源值；
4. 全部五条 29 行曲线满足累积增益与末端 256 不变量；
5. 全部 59 个职业记录保留五个有序属性投影、曲线 ID、起始/投影值、52 个显式法术列表、七个 `0xFE` 继承列表、122 个条目与 `0xFF` 终止；
6. 30 个地图精灵行与 90 个有序战斗精灵/职业/调色板条目保持无损，不发明 `NONE` 回退；
7. 原版兼容私有数据可以确定性导入，而公开工件只暴露已清除内容或非表达元数据；
8. 运行时名册状态、升级变更、转职事务、故事可用性、平衡、本地化与呈现行为由其独立所有者测试或报告为刻意偏差。

H4 不要求重制复现原版源文件或字节表布局。它要求确定性、保留溯源的变换，能解释每个导入身份、关系、别名与原始值。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 42 文件边界、H1 地址、表维度、职业/转职/成长/属性/指针摘要 | **已确认静态** | `sf2-ally-data-static-v1`（[`ally-data-static-v1.json`](../../../../tests/fixtures/h2/ally-data-static-v1.json)） | 完整运行时消费与设计意图 |
| 名称、32 个起始记录、32 个职业记录、固定打包与 source/ROM 一致性 | **已确认静态** | [static-core owner](../../../research/static-core-data.md)、[manifest](../../../../manifests/extractions/static-data.json)与[schema](../../../../schemas/static-data.schema.json) | 槽 30/31 可达性与完整字段语义 |
| 五条曲线、59 个职业记录与 122 个习得法术条目 | **已确认静态** | [growth owner](../../../research/ally-growth.md)、[manifest](../../../../manifests/extractions/growth-data.json)与[schema](../../../../schemas/growth-data.schema.json) | 运行时增益、扫描、学习、上限、夹断与刷新行为由[level-up](../../contracts/level-up.md)拥有 |
| 30 个己方地图精灵行与已接受派生调用方边界 | **已确认静态** | `sf2-map-sprite-assignments-static-v1`（[`map-sprite-assignments-static-v1.json`](../../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json)） | 故事选择、注入 ID、加载器失败模式、可见时序 |
| 成员、活跃队伍选择、加入/移除/重入变更 | **独立所有者** | [party 与 roster state](../../contracts/party-roster-state.md) | 战役时间线与玩家选择空间保持未闭合 |
| 转职事务与玩家面向服务流 | **独立所有者** | [service interactions](../../contracts/service-interactions.md) | 完整转职变更、物品边界、持久性与 UX |
| 名册构成、数值曲线、平衡与现代推进策略 | **未知 / 刻意设计** | 未来综合与模拟所有者 | 不得从原版表推断意图 |

## 复现

```powershell
uv run sf2 h2 ally-data
uv run sf2 h2 map-sprite-assignments
uv run sf2 design-contracts test
uv run sf2 verify
```
