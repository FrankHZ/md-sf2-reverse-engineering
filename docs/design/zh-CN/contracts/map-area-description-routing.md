# 地图区域描述路由合同

- **已确认原版结构：** 两个区域描述分发器身份、到 75 个可调用目标的 126 个有序配置引用、37 个固定包装器、38 个直接返回存根、37 个终止私有表、227 个物理条目、461 个配置扩展引用，以及下文描述的受限首匹配消费者规则
- **推断原版行为：** 源符号与宏词汇识别区域描述或查看角色，但本合同不从那些标签提升玩家可见含义
- **未知原版行为：** 非标准或刻意修改 `d6=0` 可达性、所选函数效果与持久性、自然故事准入、可见文本/立绘/窗口行为、时序与畸形或注入表处理
- 证据日期：2026-08-13
- 重制状态：实现无关 Phase 3 私有导入与路由合同；未选择对话、渲染器、故事、存档或地图生命周期实现

> 本文件是 [`map-area-description-routing.md`](../../contracts/map-area-description-routing.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义从所选地图配置的区域描述槽经一个可调用目标，对非空目标经有序六字节记录扫描的静态路线。它拥有：

1. `RunMapSetupAreaDescription` 与 `DisplayAreaDescription` 身份与 H1 绑定地址；
2. 到 75 目标拓扑的有序 126 引用；
3. 37 个固定 16 字节包装器与 38 个双字节直接返回存根之间的区分；
4. 私有 37 表、227 条目存储语料及其 461 个配置扩展引用；
5. 六字节条目、双字节终止符、打包坐标、条件、载荷种类与首匹配规则；
6. 专用所有者接受的文本索引与函数目标算术；
7. 源值 `d6=1` 的唯一组装普通探索调用路径边界；以及
8. 排除原版表内容与完整引用图的公开元数据/溯源投影。

它不拥有配置选择、实体/区域/物品分发、探索循环准入、文本内容、对话呈现、立绘/窗口行为、所选函数效果、故事含义、持久性、本地化、可访问性、时序或畸形输入恢复。

精确未来 research-index 关联只有：

- `map.setup.area-description`；
- `map.setup.display-area-description`。

每条其他记录保持不变。特别是 `map.setup.selector`、`map.setup.entity-list`、所有实体/区域/物品分发器记录、每个 `map.data.*` 记录与所有文本、对话、立绘、窗口、故事与呈现记录都不从本合同获得关联。

## 证据所有者与合同前审计

此处消费的唯一可执行所有者是 `sf2-map-descriptions-static-v1`：

- [受追踪 fixture](../../../../tests/fixtures/h2/map-descriptions-static-v1.json)；
- [维护验证器](../../../../src/sf2tool/h2/map_descriptions.py)；
- [所属研究文章](../../../research/map-data-inventory.md)。

已接受基线上新鲜复现产生：

```text
Contract          : sf2-map-descriptions-static-v1
SHA256            : BA7010853112B995E3DF4A8D8A207CAC4EA4F355C7E73845E7F677DEA5C4A5F7
SourceFiles       : 75
Wrappers          : 37
DirectReturnStubs : 38
PhysicalEntries   : 227
SetupReferences   : 461
Status            : PASS
```

fixture 恰好绑定两条研究记录：

| 研究记录 | 源身份 | ROM 地址 | 当前合同状态 |
| --- | --- | ---: | --- |
| `map.setup.area-description` | `RunMapSetupAreaDescription` | `0x47702` / 292,610 | 未关联；此处未来关联 |
| `map.setup.display-area-description` | `DisplayAreaDescription` | `0x47722` / 292,642 | 未关联；此处未来关联 |

没有其他研究记录携带该 fixture。宽 `sf2-map-data-static-v1` 所有者与所有 `map.data.*` 表记录被显式排除。它们既不提供可执行证据，也不给本合同提供语义关联。

生成详细输出包含私有源行、表条目、文本索引、函数目标、hash 与忽略 `local/derived/` 下的完整配置引用图。受追踪 fixture 暴露聚合计数与规则，加已接受公开验证的三个受限条件函数元数据行。

## 可调用目标拓扑

**已确认静态：** 126 个有序配置槽引用 75 个唯一可调用目标。其中 35 个目标被多个配置槽选择。因此引用身份与可调用目标身份是不同域：导入器不得为每个传入引用复制共享目标，也不得把有序配置引用折叠为无序目标集。

75 个可调用目标精确分化：

| 可调用目标种类 | 唯一目标 | 源形状 |
| --- | ---: | --- |
| 包装器 | 37 | 固定 16 字节包装器后接一个私有表 |
| 直接返回存根 | 38 | 精确双字节 `rts` 体 |
| **总计** | **75** | 完整源文件边界 |

每个包装器按顺序保留这些源静态身份：

1. 把逐包装器描述文本基础加载进 `d3`；
2. 把其私有表地址加载进 `a0`；
3. 保留已接受 `nop` 位置；
4. 直接转移到 `DisplayAreaDescription`。

包装器表紧跟其 16 字节体开始。`nop` 是已接受 source/ROM 身份，不是重制中的必需优化屏障或时序事件。直接返回存根是无表的不同可调用目标；除非原版存根身份保持独立可恢复，否则它不得被表示为发明的空表。

126 引用分母是配置指针语料事实。它不证明所有引用配置、包装器或存根在一次原版通关中自然可达。

## 私有表语料

**已确认静态：** 37 个包装器拥有 37 个私有 `$FD00` 终止表。其物理存储包含 227 个六字节条目与 37 个双字节终止符，共 1,436 字节。最大表有 23 个条目。

源宏与物理条目计数相同：

| 条目形式 | 物理条目 | 配置扩展引用 |
| --- | ---: | ---: |
| 文本（`msDesc`） | 206 | 426 |
| 函数（`msDescFunction`） | 18 | 31 |
| 条件函数（`msDescFunctionD6`） | 3 | 4 |
| **总计** | **227** | **461** |

恰好有 37 个 `msDescEnd` 终止符。物理计数描述一次存储的字节；扩展计数描述经全部 126 个配置引用观察的同一条目。这些分母不得相加或当作两个独立内容语料。

私有导入器为每个包装器与表保留：

- 源路径、包装器与表符号、H1 地址与描述文本基础；
- 有序六字节记录与每条记录物理地址；
- 精确终止符身份与地址；
- 完整传入配置引用身份与顺序；
- 文本偏移与派生索引，或函数相对偏移与解析目标；以及
- 源/H1/ROM 一致性需要的原版私有字节与 hash。

这些字段是私有保留数据。公开工件不得揭示完整地图到表赋值图、完整文本索引集、完整函数目标集、原版表字节或私有 hash。

## 六字节记录与首匹配扫描

**已确认静态：** 每条普通记录有该受限逻辑形状：

| 字节范围 | 已接受角色 | 边界 |
| --- | --- | --- |
| `0..1` | 一起作为打包坐标字比较的 X/Y 字节 | 不推断瓦片、像素或碰撞含义 |
| `2` | 条件字节 | 非零准入由 `d6` 限定；故事含义未知 |
| `3` | 载荷种类字节 | 已接受值只区分文本与函数路线 |
| `4..5` | 两个文本偏移或一个带符号函数相对偏移 | 私有载荷值保持不披露 |

消费者把其传入 X/Y 值打包进相同字形状、初始化扫描偏移并按源顺序检查记录。在每个位置它：

1. 把首字节 `$FD` 识别为双字节终止符并报告无匹配；
2. 比较打包坐标字；
3. 字节 2 非零时，在 `d6` 非零时拒绝该行；
4. 按字节 3 分发准入行；或
5. 推进六字节并继续。

首个准入坐标匹配是权威的。重制可以构建索引，但必须保留源顺序并复现首匹配选择。它不得把重复归一化为无序坐标字典。

已接受语料不包含其他载荷种类。该闭合语料事实不定义修改值、缺失终止符、截断记录、越界指针或注入状态的恢复；那些用例保持 **未知**。

## 文本与函数载荷路线

### 文本路线

**已确认静态：** 对载荷种类零，已接受物理语料也有条件字节零。字节 4 加到查看文本基础 423。字节 5 加到所选包装器 `d3` 基础。两个结果索引按源顺序交给既有文本显示接缝。

本合同保留索引算术与交接顺序，而非被引用字符串。它不拥有文本解码、本地化、窗口布局、立绘选择、字体选择、清除节奏或玩家看到什么。

### 函数路线

**已确认静态：** 载荷种类一把字节 4..5 解读为当前表基础的带符号相对偏移。匹配与条件门后调用解析地址。合同保留表相对解析、目标身份与调用交接；它不给所选函数指定含义、副作用、持久性或返回值合同。

任一所选路线后的源码形状清理交接不是可见完成的证据。对话/窗口服务、最终渲染状态与调用方可视时序保持独立所有者或 **未知**。

## 条件函数边界

受追踪公开 fixture 恰好保留三条物理条件函数行：

| 包装器 | 表 | 地址 | X | Y | 条件字节 | 载荷种类 | 相对偏移 | 解析目标 |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `ms_map31_AreaDescriptions` | `byte_5D584` | 382,376 | 8 | 3 | 1 | 1 | 58 | 382,398 |
| `ms_map41_AreaDescriptions` | `byte_5F41E` | 390,174 | 6 | 7 | 255 | 1 | 14 | 390,188 |
| `ms_map42_AreaDescriptions` | `byte_5FE34` | 392,756 | 39 | 2 | 255 | 1 | 52 | 392,808 |

**已确认静态调用图边界：** `j_RunMapSetupAreaDescription` 在已接受 `code/**` 源语料中恰好有一个组装调用。普通探索路径设置 `d6=1`，然后进入 `CheckArea`；`CheckArea` 到达该唯一调用。因为消费者在 `d6` 非零时跳过非零条件字节，所以那三条物理条件函数行——以及对其的全部四个配置扩展引用——在该唯一源码形状路径上被跳过。

这不是通用不可达性。直接调用方、修改源、注入寄存器状态或其他 `d6=0` 非标准路线可以穿过已接受门，但此处不建立此类原版运行时路线。那些目标是否执行、做什么、其效果是否持久保持 **未知**。

附加到 `d6=1` 的源注释只保留为源标签 `no-entity-event`。它不证明输入边同时性、玩家意图、精确帧时序或 `d6` 的通用语义类型。

## 实现无关导入模型

最小完整私有导入把配置引用、可调用目标、表、条目与公开投影分开：

```text
MapAreaDescriptionCorpus {
  setupReferences[126]: PrivateDescriptionSetupReference
  callableTargets[75]: PrivateDescriptionTarget
  privateTables[37]: PrivateDescriptionTable
  publicSummary: MapAreaDescriptionPublicSummary
}

PrivateDescriptionSetupReference {
  orderedSetupIdentity
  callableTargetRef
}

PrivateDescriptionTarget =
  WrapperTarget {
    sourcePath
    wrapperSymbol
    wrapperAddress
    descriptionTextBase
    tableRef
    privateWrapperBytes[16]
  }
  | DirectReturnStubTarget {
    sourcePath
    symbol
    address
    privateStubBytes[2]
  }

PrivateDescriptionTable {
  tableSymbol
  tableAddress
  orderedEntries[]: PrivateDescriptionEntry
  terminatorAddress
  privateTerminatorBytes[2]
}

PrivateDescriptionEntry {
  physicalAddress
  x
  y
  conditionByte
  payload: PrivateTextPayload | PrivateFunctionPayload
  privateRawBytes[6]
}

PrivateTextPayload {
  investigationTextOffset
  investigationTextIndex
  descriptionTextOffset
  descriptionTextIndex
}

PrivateFunctionPayload {
  relativeOffset
  resolvedTargetAddress
}

MapAreaDescriptionPublicSummary {
  sourceFileCount = 75
  setupPointerReferenceCount = 126
  uniqueTargetCount = 75
  aliasedTargetCount = 35
  wrapperCount = 37
  directReturnStubCount = 38
  physicalEntryCount = 227
  setupEntryReferenceCount = 461
  physicalKindCounts = { text: 206, function: 18, conditionedFunction: 3 }
  expandedKindCounts = { text: 426, function: 31, conditionedFunction: 4 }
  terminatorCount = 37
  physicalTableByteCount = 1436
  maximumTableEntryCount = 23
  consumerRules
  conditionedFunctionMetadata[3]
  fixtureProvenance
}
```

这是导入与路由模型，不是引擎对话 API、事件脚本语言、地图编辑器 schema 或持久性格式。公开报告可以保留受限摘要、函数身份、地址、消费者规则、溯源与三条受追踪条件行。它们必须省略所有 `private*` 字段、完整表内容、完整赋值、完整文本/函数索引图、原版字符串与渲染捕获。

## 跨系统分离

- [Map Exploration](../../contracts/map-exploration.md) 拥有 `CheckArea`、探索交互循环及其已接受运行时生命周期。本合同只拥有被调用描述选择器/扫描。
- [Map Entry Routing State](../../contracts/map-entry-routing-state.md)、配置选择证据与[Story Progression](../synthesis/story-progression.md) 保留配置与故事状态选择。配置引用不是自然故事准入的证明。
- 实体、区域与物品选择器保留其自身表格式、匹配规则、效果与关联。它们此处不获得合同。
- [Text 与 Font System](../../contracts/text-and-font-system.md)、[Dialogue System](../../contracts/dialogue-system.md)、立绘、窗口与 UI 所有者保留文本资源、解码、呈现与时序。
- 排除 `sf2-map-data-static-v1` 聚合与每个 `map.data.*` 记录保持不变。
- 地图布局、调色板、瓦片集、精灵、碰撞、寻路、摄像机、VInt/DMA、音频与渲染保持其专用合同或 **未知**。
- 存档/读档持久性、故事含义、本地化、可访问性、内容替换与产品策略是刻意后期设计面。

[Map Design Principles 综合](../synthesis/map-design-principles.md) 已总结已接受聚合区域描述证据。它可以稍后链接本合同，但本拥有文档切片不编辑或重新解读该综合。

## 判断边界

### 已确认

- 唯一可执行所有权 `sf2-map-descriptions-static-v1`；
- 两个分发器身份与地址；
- 126 个有序配置引用、75 个目标与 35 个复用目标；
- 37 个 16 字节包装器、38 个双字节直接返回存根与精确包装器交接形状；
- 37 个私有表、227 个物理条目、461 个扩展引用、37 个终止符、1,436 字节、最大 23 条目与精确种类计数；
- 打包坐标、终止符、条件、载荷种类、表相对函数、文本索引与首匹配规则；
- 三条条件函数元数据行；
- 唯一组装普通探索调用路径的 `d6=1` 值及其对那行的跳过结果；
- source/H1/ROM 一致性与公开元数据/私有内容分离。

### 推断

- 源符号与宏暗示区域描述与查看角色。不从那些名称提升玩家面向含义、叙事意图或可见结果。

### 未知

- 直接、调试、修改或注入 `d6=0` 调用方可达性；
- 所选函数行为、副作用、返回含义、过渡生命周期与持久性；
- 个别引用与行的自然配置与故事可达性；
- 文本内容、立绘/窗口选择、本地化、呈现、帧与时序；
- 畸形、截断、未终止、越界或替换表行为；
- 引擎编辑器策略、可访问性、替换内容与其他产品决定。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 `sf2-map-descriptions-static-v1`、固定溯源与两个已接受函数地址，而不添加另一可执行所有者；
2. 私有保留全部 126 个有序配置引用与 75 个可调用目标身份，包括全部 35 个复用目标关系；
3. 把 37 个包装器身份与精确 16 字节源形状与 38 个精确双字节直接返回存根分开保留；
4. 私有保留 37 个有序表、227 个物理条目、37 个终止符、其地址与字节，并复现 1,436 字节与最大 23 条目总计；
5. 分别复现 461 个配置扩展引用与精确 `206/18/3` 物理对 `426/31/4` 扩展种类计数；
6. 复现打包坐标首匹配选择、`$FD00` 终止符、条件字节门、载荷种类分支、文本索引算术与表相对函数解析；
7. 保留精确三条条件行并验证唯一组装普通路径的 `d6=1` 跳过它们，而不声称通用运行时不可达性；
8. 通过私有或合成测试检测引用重排、别名压平、包装器/存根混淆、丢失终止符、条目顺序变更、偏移漂移与私有源丢失；
9. 公开只暴露受限计数、规则、身份、地址、溯源与已接受三条条件行——而非完整表、赋值、文本/函数图、字节、hash、字符串或捕获；
10. 恰好关联 `map.setup.area-description` 与 `map.setup.display-area-description`，保持选择器/实体/区域/物品/map-data 与每条其他记录不变；以及
11. 通过独立所有者报告调用方准入、效果、持久性、文本/对话/窗口呈现、时序、畸形输入、本地化、可访问性与故事含义，或作为 **未知**。

H4 可以把私有表编译为索引结构或其他引擎原生表示。该表示只在有序 source/引用拓扑与往返身份保持可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 分发器身份与地址 | **已确认静态** | `sf2-map-descriptions-static-v1` | 恰好两个未来 research-index 关联 |
| 126 引用 / 75 可调用目标 | **已确认静态** | 同一 fixture 与[map-data 研究](../../../research/map-data-inventory.md) | 完整图保持私有；无自然可达性声称 |
| 包装器、存根、表、条目与终止符 | **已确认静态** | 同一 fixture/验证器 | 原始字节、完整赋值、索引、目标与 hash 保持私有 |
| 坐标/条件/载荷/首匹配规则 | **已确认静态 source/ROM** | 同一所有者 | 无畸形输入或调用方可视恢复合同 |
| 唯一组装 `d6=1` 调用路径 | **已确认静态调用图** | 同一所有者 | 直接/修改 `d6=0` 可达性保持 **未知** |
| 探索准入与 `CheckArea` 生命周期 | **独立所有者** | [map-exploration](../../contracts/map-exploration.md) | 此处无输入边、帧或交互结果声称 |
| 配置/故事选择 | **独立所有者** | 地图配置证据与[story progression](../synthesis/story-progression.md) | 配置成员不是故事可达性 |
| 文本/对话/立绘/窗口结果 | **未知 / 独立所有者** | 文本、对话、立绘、窗口与呈现合同 | 静态交接不证明可见内容或时序 |
| 宽 map-data 语料 | **排除执行所有者** | `sf2-map-data-static-v1` | 所有 `map.data.*` 记录保持不变 |
| 区域描述用途 | **推断（仅源分类法）** | 源符号/宏 | 玩家面向与叙事含义保持未声称 |

## 开放问题

1. 受限原版调用方或受控探针能否以 `d6=0` 到达条件函数？
2. 所选相对函数目标执行什么状态变更，任何是否跨过渡或存档/读档持久？
3. 哪些配置引用与表行在原版故事游玩中自然可达？
4. 每个已接受路线后可见什么文本、立绘、窗口与时序序列？
5. 重制应对畸形或替换表使用什么显式策略？

## 复现

```powershell
uv run sf2 h2 map-descriptions
uv run sf2 design-contracts test
uv run sf2 verify
```

详细生成输出保留在忽略的 `local/derived/map-descriptions-static.json` 下。公开验收使用受追踪 fixture、聚合元数据、三条受限条件行与合成/私有导入器测试，而非再分发原版表、文本或渲染数据。
