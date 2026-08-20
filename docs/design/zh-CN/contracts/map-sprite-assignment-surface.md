# 地图精灵赋值面合同

- 状态：**已确认静态赋值身份、源域计数与连接构建输入排除**
- 证据日期：2026-08-14
- 范围：原版实体地图精灵赋值起源与它们准入的受限域

> 本文件是 [`map-sprite-assignment-surface.md`](../../contracts/map-sprite-assignment-surface.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同描述可以写入或派生实体地图精灵身份的源静态面。它不定义精灵图形、呈现、故事放置或任意运行时变更。

- **已确认**：五个完整实体地图精灵写入器身份；五个索引辅助/写入器条目身份；跨三个宏形式的 81 个构建脚本赋值；`UpdateEntityProperties` 的全部 20 个直接调用方；用作外部见证的已接受己方、敌人与初始实体域摘要；以及专用 fixture 的连接结论——其已接受原版构建输入域不包含地图精灵 ID `237..250`。
- **推断**：该证据等级没有规范性行为。`player`、`raft`、`ally`、`vehicle`、`regular-backed`、`routed-special` 与 `blue-flame NPC` 等术语保留源或验证器分类词汇，而不建立故事目的、渲染或设计意图。
- **未知**：刻意畸形脚本、直接 RAM 变更、损坏战斗员状态或仅调试注入路线能否引入 ID `237..250`；此类注入后的消费者行为；加载器失败或哨兵行为；队列饱和；调用方可视结果；运行时频率；时序；DMA；渲染；呈现；持久性；以及替换策略。

`originalBuiltDomainsContainReservedIds=false` 是专用 fixture 在其已接受输入域上拥有的 **已确认静态连接结论**。它不是任意运行时状态永不含那些值的声称、畸形数据保证，或用作见证的己方、敌人或初始实体语料的独立所有权。

## 证据所有者与消费面

本合同消费的唯一可执行所有者是 `sf2-map-sprite-assignments-static-v1`（[fixture](../../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json)、[verifier](../../../../src/sf2tool/h2/map_sprite_assignments.py)、[schema](../../../../schemas/h2-map-sprite-assignments-static-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/map-sprite-assignments-static.json)）。其带来源文章所有者是[Common Scripting](../../../research/common-scripting.md)。

合同消费以下受追踪 fixture 字段：

- `table` 中五个所选索引条目身份与地址；
- `summary` 中的聚合计数；
- `writerSites` 中五个受限行；
- `scriptAssignmentFacts`，包括宏/域计数、空保留 ID 集、最高普通值与 routed-special 值身份；
- `updateCallerFacts`，包括完整输入种类计数分区；
- `derivedDomainFacts` 中的范围与排除摘要；以及
- 保留为显式 未知 的两个 `runtimeQuestions`。

验证器还生成完整 `scriptAssignments[81]` 与 `updateCallers[20]` 目录，位置在忽略的 `local/derived/map-sprite-assignments-static.json` 下。那些目录是私有验证输入。它们刻意缺席公开 fixture，不由本合同发布。

聚合 `sf2-common-scripting-static-v1` fixture 不被消费。ally-data、enemy-definition、map-entity、map-sprite decode、special-sprite decode、map-data 或运行时 H3 fixture 也不被消费。其已接受结果只通过下文的显式独立所有者边界出现。

## 直接绑定与关联边界

专用 fixture 直接绑定六条 research-index 记录。五条是本合同的精确未来关联候选；第六条只保持其既有数据所有者。

| 记录 ID | 源身份 | ROM 条目 | 合同处理 |
| --- | --- | ---: | --- |
| `scripting.entity.declarenewentity` | `DeclareNewEntity` | 280,010 | 新关联候选 |
| `scripting.entity.esc17-setspritenumber` | `esc17_setSpriteNumber` | 23,420 | 新关联候选 |
| `scripting.entity.getallymapsprite` | `GetAllyMapsprite` | 281,030 | 新关联候选 |
| `scripting.entity.updateentityproperties` | `UpdateEntityProperties` | 24,658 | 新关联候选 |
| `scripting.map.csc1a-setentitysprite` | `csc1A_setEntitySprite` | 289,352 | 新关联候选 |
| `ally.data.map-sprites` | `table_AllyMapsprites` | 281,182 | 不变；只由[Ally Definition 与 Growth Data](../../contracts/ally-definition-data.md)拥有 |

本合同不关联其他 `scripting.entity.*`、`scripting.map.*`、`ally.data.*`、`enemy.*`、`map.entity-population.*`、`map.data.*`、`tech.graphics.*` 或 auxiliary-data 记录。

## 完整写入器身份面

验证器扫描完整固定源树查找实体地图精灵字段的写入。它找到四个偏移形式写入加一个直接玩家字段写入，并证明每个受限指令形状恰好一个源出现。

| 源所有者 | 源路径 | 已确认写入形状 |
| --- | --- | --- |
| `DeclareNewEntity` | `code/common/scripting/entity/entityfunctions_1.asm` | `d4` 字节到实体定义地图精灵字段 |
| `esc17_setSpriteNumber` | `code/common/scripting/entity/entityscriptengine_2.asm` | 脚本字节 `3(a1)` 到该字段 |
| `UpdateEntityProperties` | `code/common/scripting/entity/entityscriptengine_2.asm` | `d3` 字节到该字段 |
| `csc1A_setEntitySprite` | `code/common/scripting/map/mapscriptengine_1.asm` | `d0` 字节到所选实体字段 |
| `direct-player-raft-write` | `code/common/scripting/map/followersfunctions_2.asm` | 具名 `MAPSPRITE_RAFT` 字节到直接玩家字段 |

`GetAllyMapsprite` 是单独索引派生辅助，不是第六个写入器。公开写入器行记录源身份与受限指令摘要；它们不发布完整源体，也不建立每个写入器在普通游玩中自然到达。

## 构建脚本赋值域

完整固定源扫描发现三个宏族中的 81 个赋值：

| 宏族 | 赋值计数 |
| --- | ---: |
| `setSprite` | 56 |
| `newEntity` | 18 |
| `ac_setSprite` | 7 |
| **总计** | **81** |

那些赋值使用 40 个不同数字地图精灵值。验证器把 76 个赋值分类为 `regular-backed`、五个为 `routed-special`。全部五个 routed-special 赋值使用值 `255`。该脚本面最高普通值为 `230`。没有脚本赋值使用 `237..250` 中的值，因此 `scriptReservedCount=0` 且 `reservedIdsPresent=[]`。

这些是源语料计数，不是执行计数。重复源赋值不暗示运行时频率，`regular-backed`/`routed-special` 类别不定义解码、呈现或替换行为。

## `UpdateEntityProperties` 输入分区

`UpdateEntityProperties` 的每个直接源调用方都有受限前导 `d3` 输入。完整 20 调用方目录闭合为：

| 输入分类 | 调用方计数 | 合同含义 |
| --- | ---: | --- |
| 保留既有 | 12 | 源传递保留哨兵而非替换身份 |
| 己方表派生 | 5 | 受限调用方窗口派生己方地图精灵值 |
| 己方或字面车辆 | 1 | 调用方窗口包含已接受己方/车辆拆分 |
| 字面地图精灵 | 2 | 调用方窗口提供具名字面身份 |
| **总计** | **20** | 每个直接调用方都被分类 |

表只记录输入溯源。它不声称调用成功、另一状态字段不变、图形加载完成或所选精灵可见。完整调用方路径、行号与表达式保留在私有派生目录中。

## 连接已接受构建输入结论

专用验证器连接四个已接受源静态域：

1. 上文总结的 81 个构建脚本赋值；
2. 20 个分类 `UpdateEntityProperties` 输入；
3. 独立[Map Entity Data](../../contracts/map-entity-data.md)所有者的 980 个已接受初始实体记录；
4. 外部拥有的己方与敌人定义表。

受追踪见证摘要：

- 980 个初始记录不包含哨兵普通或未支撑特殊 ID；
- 30 行己方表有 `1..58` 中的值；
- 166 行敌人表有 `52..229` 中的值；
- 己方派生只从已接受表值减去或使用具名 blue-flame NPC 回退；
- 全部 20 个属性更新调用方被分类；以及
- 构建脚本面本身在 `237..250` 中零 ID。

一起产生 fixture 已接受连接域的 `originalBuiltDomainsContainReservedIds=false`。该结论不重新定义 980 条记录、30 行己方、166 行敌人或其完整内容。它也不扩展到原始 RAM、畸形脚本、损坏状态、未发布 mod 或未来重制数据。

## 实现无关逻辑模型

完整私有导入可以使用等价模型：

```text
MapSpriteAssignmentSurface {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    writerIdentities[5] {
        logicalWriterId
        sourcePath
        boundedWriteForm
        optional indexedRecordRef
    }
    indexedDerivationHelpers[1] {
        logicalHelperId = GET_ALLY_MAPSPRITE
        indexedRecordRef
    }
    privateScriptAssignments[81] {
        sourceOriginRef
        macroFamily
        expressionIdentity
        logicalMapSpriteValue
        sourceDomainClass
    }
    privateUpdateCallers[20] {
        sourceCallerRef
        inputClassification
        privateExpressionIdentity
    }
    externalDomainWitnesses {
        initialEntityRecordOwnerRef
        allyTableOwnerRef
        enemyTableOwnerRef
        acceptedRangeAndExclusionSummaries
    }
    reservedIdRange = 237..250
    originalBuiltDomainsContainReservedIds = false
}
```

完整私有赋值与调用方目录的源起源与路径、完整表达式、原始 source/ROM 材料与其他非公开验证细节是溯源与私有往返输入。公开投影中命名的五个所选写入器源路径与行、索引符号与条目地址、受限写入摘要、hash 与溯源、计数与见证摘要保持公开元数据。完成私有验证后，合规重制可以使用引擎原生实体与精灵引用。它不需要复现 Mega Drive RAM 偏移、字节字段放置、宏编码、指令形式、ROM 地址或原版写入器微实现。

逻辑模型必须保持赋值起源与所选逻辑精灵身份不同。它还必须保留源赋值、保留当前值的调用方、派生表值与外部拥有数据见证之间的差异。

## 公开与私有投影

公开投影只能保留 fixture 与本合同已代表的受限受追踪元数据：

- fixture 身份、输出 hash、上游修订与已接受 ROM 身份；
- 所选索引符号与条目地址；
- 五个受限写入器行；
- 聚合赋值、不同值、宏族、域与调用方计数；
- 最高普通值与 routed-special 值身份；
- 空保留 ID 摘要、外部表范围与连接构建域结论；以及
- 两个显式运行时问题。

公开投影不得包含完整 81 赋值行、完整 20 调用方行、完整脚本程序、初始实体记录、己方/敌人表行、私有表达式、原版精灵载荷或美术、ROM 摘录、模拟器轨迹或捕获呈现。私有导入器可以在本地验证那些材料而不使其成为可再分发合同载荷。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 五个逻辑写入器起源与一个索引派生辅助身份被表示而不合并其角色；
2. 完整私有 81 赋值目录确定性导入并保留其精确 `56 + 18 + 7` 起源分区；
3. 完整私有 20 调用方目录保留精确 `12 + 5 + 1 + 2` 输入分区；
4. 已接受构建脚本面中的值保留其逻辑身份与精确 `76 regular-backed + 5 routed-special` 分类；
5. 外部初始实体、己方表与敌人表见证保持对其自身合同的引用而非复制所有权；
6. 连接已接受构建域测试报告 `237..250` 中零 ID，而不把该结果泛化到畸形、注入或任意运行时状态；
7. 引擎原生引用可以复现相同赋值关系，而不要求原版 RAM、宏、指令或地址布局；以及
8. 公开报告只暴露受限元数据，而完整目录与版权载荷保持私有。

H4 不要求原版写入循环、调用时序、图形队列、DMA 行为、渲染结果、故事出现或不受支持注入 ID 的处理。

## 跨系统分离

- [Ally Definition 与 Growth Data](../../contracts/ally-definition-data.md) 保持 30 行己方地图精灵表、其身份顺序与行内容的唯一所有者。
- [Enemy Definition Data](../../contracts/enemy-definition-data.md) 保留 166 行敌人地图精灵表与普通对尾定义边界。
- [Map Entity Data](../../contracts/map-entity-data.md) 保留 980 条物理初始实体记录、记录打包、列表拓扑与初始地图精灵字段。
- [Map-Sprite Graphics Data](../../contracts/map-sprite-graphics-data.md) 保留 720 个普通源槽、670 个载荷身份、别名关系、Basic 流与哨兵结构。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留特殊精灵指针/分发与解压服务边界。
- [Map 与 Exploration](../../contracts/map-exploration.md) 与 Common Scripting 保留运行时实体变更、移动、动作分发、生命周期、摄像机与呈现交接。
- [Sprite Dialogue Property Data](../../contracts/sprite-dialogue-property-data.md) 保留地图精灵到立绘与语音 SFX 属性查找。
- 故事选择、存档持久性、可见朝向/动画、图形加载、VInt/DMA 节奏、本地化、可访问性与替换策略保持独立所有者、未知或刻意重制设计。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 五个写入器身份与受限写入形式 | **已确认静态** | `sf2-map-sprite-assignments-static-v1` | 自然执行、结果、排队、时序 |
| 81 个赋值、宏/域计数、零保留值 | **已确认静态** | 同一 fixture | 运行时频率、呈现、畸形输入 |
| 20 个分类 `UpdateEntityProperties` 调用方 | **已确认静态** | 同一 fixture | 完整调用方行为与可见效果 |
| 初始/己方/敌人范围与排除见证 | **独立所有者已确认静态见证** | 由专用 fixture 连接的 map-entity、ally-definition 与 enemy-definition 所有者 | 不转移语料所有权 |
| 已接受连接构建域排除 ID `237..250` | **已确认静态** | 专用赋值 fixture | 原始 RAM、损坏/调试注入、任意运行时状态 |
| 源标签开发者/故事用途 | **推断，非规范性** | 仅源码词汇 | 产品意图与玩家面向含义 |
| 注入 ID 消费者失败、时序、渲染、持久性 | **未知 / 独立所有者** | 未来受限运行时或呈现证据 | 此处不是 H4 要求 |

## 复现

```powershell
uv run sf2 h2 map-sprite-assignments
uv run sf2 design-contracts test
uv run sf2 verify
```

生成完整赋值与调用方目录保留在忽略的 `local/derived/map-sprite-assignments-static.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
