# Standalone Map-Script 程序数据合同

- **已确认原版结构：** 47 个有序 standalone `scripts*.asm` 源文件、178 个非空带标签程序、8,058 个有序操作、其完整私有源引用图、下文精确聚合分类与计数，以及固定 source/H1/ROM 溯源。
- **推断原版行为：** 源标签前缀与命令名暗示过场、实体动作、子程序与调色板数据等组织角色。那些名称只作为源分类法保留；它们不建立故事、玩家面向或运行时含义。
- **未知原版行为：** 正常故事准入与路线顺序；执行频率；命令、分支、子程序与目标效果；故事状态持久性；实体、摄像机、文本、声音、调色板、等待与渲染行为；帧时序；畸形输入处理；以及替换或本地化策略。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择解释器 API、脚本语言、调度器、场景系统、故事模型、编辑器格式或公开原版内容格式。
- 证据日期：2026-08-14
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`standalone-map-script-program-data.md`](../../contracts/standalone-map-script-program-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版 `data/maps/entries/*/mapsetups/` 源语料下最后 47 个非指针表 `scripts*.asm` 文件的静态数据与溯源边界：

1. 全部 47 个源文件的有序身份及其代表标签/地址；
2. 178 个源有序、非空带标签程序及其 8,058 个有序操作；
3. 完整私有标签所有权、同文件/跨文件词法引用与解析操作目标边；
4. 更广 init-script 目标集与 12 个本 standalone 语料拥有目标之间的精确静态连接；
5. 私有无损导入形式与受限公开元数据/溯源投影。

此处消费的唯一可执行所有者是 fixture id `sf2-map-scripts-static-v1`，位于 [`tests/fixtures/h2/map-scripts-static-v1.json`](../../../../tests/fixtures/h2/map-scripts-static-v1.json)。其验证器是[`src/sf2tool/h2/map_scripts.py`](../../../../src/sf2tool/h2/map_scripts.py)，其带来源所有者是[Map Data Inventory](../../../research/map-data-inventory.md)的 **Standalone Setup Scripts** 段。

精确未来 research-index 关联只有 `map.data.cs-5e772`。该记录把代表符号 `cs_5E772` 绑定在 ROM `0x5E772`（`386930`）。它是进入完整 47 文件语料的一个追踪锚点，不是语料只包含该程序或该程序有特权运行时含义的陈述。

宽 `sf2-map-data-static-v1` 与 `sf2-map-script-engine-static-v1` fixture 被刻意排除。前者拥有聚合 map-data 成员；后者拥有既有合同使用的完整解释器面向命令/程序/处理器面。两者都不成为第二个可执行依赖或扩展本文档关联集。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-scripts-static-v1
SHA256 7FD49F464181563AC3C1D6FAFBB7671B072A0CC5ADC8F5506BFA74C9C2C78E4E
Files 47 / Labels 178 / Statements 8058 / ExternalReferences 146 / PASS
```

fixture 直接绑定恰好一条 research-index 记录：

- `map.data.cs-5e772`——唯一、当前未关联，且是唯一未来关联。

没有其他 `map.data.*`、`map.setup.*`、`scripting.map.*`、解释器、处理器、实体、对话、音频、故事、过渡、调色板、呈现或服务记录获得本合同。特别是，更宽 map-data 语料中的源成员不使该聚合 fixture 成为此处所有者。

受追踪 fixture 是公开元数据面。它包含溯源、47 个代表地址、聚合计数、标签种类计数、十二行命令频率摘要、十二个 standalone init 目标身份与三个运行时问题身份。完整程序体、操作数文本、完整 178 地址图、完整命令计数、体 hash 与引用源列表保留在忽略的 `local/derived/map-scripts-static.json` 下。

## 源文件与程序身份

### 已确认静态

已接受语料恰好包含：

| 面 | 已接受计数 |
| --- | ---: |
| 有序 standalone 源文件 | 47 |
| 源行 | 8,398 |
| 归一化源语句 | 8,058 |
| 不同源命令身份 | 139 |
| 全局标签 | 178 |
| 非空程序 | 178 |
| 有序程序操作 | 8,058 |
| 代表文件标签地址 | 47 |

每个源文件有一个受追踪代表符号与 H1 地址。178 个全局标签中的每一个都在固定 H1 列表解析并拥有非空有序程序。8,058 归一化语句与 8,058 程序操作之间的相等是构建检查语料边界，不是运行时执行计数。

私有导入器必须为每个文件保留其源路径、源顺序、代表身份与所有拥有标签。对每个程序它必须保留源标签、H1 地址、源路径、源分类与有序操作。它不得合并同形状程序、重编号标签、丢弃看似冗余操作，或把代表符号提升为不同语义类。

公开投影可以保留 fixture 已暴露的 47 个代表身份/地址。它不发布全部 178 个标签与地址或程序到源文件的完整映射。

## 源分类边界

### 已确认静态分类法

验证器按已接受源前缀分类标签，而不指定运行时效果：

| 源分类 | 程序计数 |
| --- | ---: |
| 过场 | 141 |
| 过场子程序 | 6 |
| 过场实体 | 4 |
| 实体动作脚本 | 2 |
| 普通子程序 | 13 |
| 局部控制流 | 8 |
| 调色板数据 | 2 |
| 其他 | 2 |
| **总计** | **178** |

这些是源码形状分类。私有导入器保留它们，因为它们对往返溯源与诊断有用。重制不得仅从分类推断执行准入、实体所有权、可见过场内容、调色板应用、叙事角色或玩家面向行为。

因此 `cutscene` 一词与命令/标签前缀有两个不同状态：

- 精确源拼写与前缀派生分类是 **已确认静态**；
- 该拼写暗示的任何行为、叙事或呈现解读只是 **推断**，此处不是保真要求。

## 有序操作与目标图

### 已确认静态

全部 178 个程序保留其完整私有有序操作列表。每个操作保留：

- 其所属程序内的零基索引；
- 精确源 opcode 身份；
- 私有操作数文本；
- 零个或多个解析目标符号；
- 匹配 H1 目标地址。

恰好 100 个操作包含目标符号，那些操作恰好包含 100 个目标引用。一对一的聚合结果不把通用单目标语法强加给未来或畸形数据；它只描述完整已接受语料。

静态图还保留源终止 token。语料包含 122 个 `csc_end` 语句与 16 个 `rts` 语句。这些计数是源 token 事实，不是每个 token 执行、两种形式语义等价或任一定义公开解释器 API 的证明。

公开 fixture 可以保留聚合操作与目标引用计数。完整 opcode、操作数、目标边、程序体与体 hash 保持私有。合成测试可以练习顺序、目标解析与丢失检测而不发布原版程序。

## 词法引用拓扑

### 已确认静态

引用分析在把所有权指定给 178 个 standalone 标签的同时搜索完整已接受 720 文件 map-setup 源边界。其精确结果：

| 引用面 | 已接受计数 |
| --- | ---: |
| 被另一源文件引用的标签 | 127 |
| 只在定义文件内引用的标签 | 51 |
| 无词法引用的标签 | 0 |
| 跨文件词法引用 | 146 |
| 同文件词法引用 | 92 |

127 与 51 计数划分全部 178 个标签。它们是标签所有权/引用类，而 146 与 92 计数出现次数；任一对都不可替代对方。

私有导入器必须为每个标签保留其所有者路径、同文件出现计数、外部出现计数与有序外部源路径身份。它可以构建反向索引或现代图表示，但必须保持能精确复现已接受所有权与引用计数。

词法引用不是运行时可达性。零未引用结果不证明每个程序在正常游玩中执行，外部引用也不建立路线顺序、条件准入、持久性或可观察效果。

## Init 目标所有权连接

### 已确认静态

专用验证器交叉检查已接受 init-script 目标身份并把 75 个不同目标划分如下：

| 目标所有权 | 计数 |
| --- | ---: |
| 本 standalone 语料拥有的目标 | 12 |
| 非 standalone init 源拥有的目标 | 63 |
| **不同 init 目标总计** | **75** |

十二个公开身份：

`cs_53176`、`cs_570B0`、`cs_58FA4`、`cs_5996E`、`cs_5B016`、`cs_5E320`、`cs_5E346`、`cs_6060E`、`cs_607DE`、`cs_60C42`、`cs_60CA4` 与 `cs_60EB2`。

这是静态目标定义连接。它不建立任何目标在正常游玩中运行、只运行一次、所有调用站点有相同状态或其操作有名称暗示的效果。地图配置选择与 init 分发保持独立所有者。

验证器内部使用已接受 map-init fixture 闭合该目标连接。本设计合同只消费结果 `sf2-map-scripts-static-v1` 可执行 fixture，不把 map-init fixture 注册为第二依赖。

## 公开命令频率边界

### 已确认静态

公开 fixture 保留十二个最频繁命令身份与精确源出现计数：

| 命令 | 计数 |
| --- | ---: |
| `nextSingleText` | 923 |
| `setFacing` | 733 |
| `endActions` | 703 |
| `csWait` | 596 |
| `entityActionsWait` | 478 |
| `setActscriptWait` | 388 |
| `nextText` | 297 |
| `moveDown` | 275 |
| `moveUp` | 244 |
| `entityActions` | 225 |
| `setPos` | 211 |
| `moveLeft` | 198 |

这些值是语料直方图，不是执行频率。完整 139 命令直方图是私有验证面。在本合同下，没有命令名建立处理器行为、时序、可见文本、朝向结果、实体移动、等待时长或音频/呈现效果。

## 实现无关导入模型

一个完整逻辑导入可以使用以下私有/公开拆分。名称是说明性的；身份、顺序与关系是规范性的。

```text
StandaloneMapScriptCorpus {
  privateFiles[47]: PrivateScriptSourceFile
  privatePrograms[178]: PrivateMapScriptProgram
  privateReferences[178]: PrivateLabelReferenceRecord
  privateCommandHistogram[139]
  standaloneInitTargetRefs[12]
  publicSummary: StandaloneMapScriptPublicSummary
}

PrivateScriptSourceFile {
  sourceOrder
  sourcePath
  representativeSymbol
  representativeAddress
  privateBodyHash
  ownedProgramRefs[]
}

PrivateMapScriptProgram {
  programId
  sourcePath
  h1Address
  sourceClassification
  orderedOperations[]: PrivateMapScriptOperation
  privateBodyHash
}

PrivateMapScriptOperation {
  operationIndex
  opcodeIdentity
  privateOperandText
  targetSymbols[]
  targetAddresses[]
}

PrivateLabelReferenceRecord {
  labelId
  ownerPath
  sameFileReferenceCount
  externalReferenceCount
  privateExternalSourcePaths[]
}

StandaloneMapScriptPublicSummary {
  fixtureId = "sf2-map-scripts-static-v1"
  sourceFileCount = 47
  sourceLineCount = 8398
  statementCount = 8058
  uniqueCommandCount = 139
  programCount = 178
  operationCount = 8058
  representativeAddresses[47]
  labelKindCounts
  externallyReferencedLabelCount = 127
  internalOnlyLabelCount = 51
  unreferencedLabelCount = 0
  sameFileReferenceCount = 92
  externalReferenceCount = 146
  operationTargetCounts
  initTargetOwnershipCounts
  topCommandHistogram[12]
  runtimeQuestionIds[3]
  fixtureProvenance
}
```

这是私有导入/溯源模型，不是必需脚本解释器、字节码格式、协程调度器、事件系统、编辑器 schema、场景图或存档状态表示。重制只有在已接受身份、顺序、目标关系、分类与源溯源保持独立可验证时，才可以把私有程序编译为另一内部形式。

## 公开投影与版权边界

公开合同或报告只能保留已接受 fixture 已限定的元数据：

- fixture、上游、ROM、验证器与研究所有者溯源；
- 语料维度与聚合操作/引用计数；
- 47 个代表符号与地址；
- 标签种类计数与十二行命令频率摘要；
- 十二个 standalone init 目标身份；
- 三个已接受运行时问题身份；
- 显式 已确认、推断、未知 与独立所有者 标签。

公开投影不得包含完整原版程序体、有序操作流、操作数文本、完整命令直方图、全部 178 个标签/地址、体 hash、完整目标图、引用源列表、提取对话、调色板字节、捕获或其他版权载荷。私有导入工具只能在忽略本地存储中保留并 hash 那些形式。

## 跨系统分离

- [Map Exploration](../../contracts/map-exploration.md) 拥有已接受解释器面向命令、处理器、实体、摄像机、放置、生命周期、过渡与受限 H3 接缝。本数据合同不复制那些运行时语义，也不注册 `sf2-map-script-engine-static-v1`。
- [Map Entry Routing State](../../contracts/map-entry-routing-state.md) 保留其 SwitchMap、CheckBattle 与存档点辅助接缝。已接受地图配置证据与[Map Exploration](../../contracts/map-exploration.md) 保留配置选择、init 准入与分发器顺序。十二目标连接不是路线准入合同。
- [Story Progression](../synthesis/story-progression.md) 拥有跨子系统故事/状态综合；此处的源标签与引用不定义情节顺序或存档持久性。
- [Dialogue System](../../contracts/dialogue-system.md)、[Party Roster State](../../contracts/party-roster-state.md)、[Audio System](../../contracts/audio-system.md)与实体/呈现所有者保留各自命令族、状态接缝、可见内容、声音与运行时行为。
- [Map Entity Data](../../contracts/map-entity-data.md)、[Map Area Description Routing](../../contracts/map-area-description-routing.md)、地图布局/调色板/瓦片集/精灵所有者与图形/中断合同保留其自身数据、选择、渲染与硬件边界。
- `sf2-map-data-static-v1`、所有其他 `map.data.*` 记录、`map.setup.*`、`scripting.map.*`、服务、处理器与呈现记录保持排除且不变。
- 现代脚本语言、编辑器 UX、本地化、可访问性、替换、许可与分发选择保持未来所有者。

## 判断边界

### 已确认

- 精确 `sf2-map-scripts-static-v1` fixture 身份、固定 source/ROM 溯源、规范摘要、验证器与所有者文章；
- 47 个有序源文件、8,398 条源行、139 个命令身份、178 个标签/程序与 8,058 个有序操作；
- 精确八路源分类计数；
- 完整标签所有权与词法引用拓扑：127 个外部、51 个仅同文件、零未引用标签、146 个跨文件引用与 92 个同文件引用；
- 恰好 100 个目标引用的 100 个操作；
- 划分为 12 个 standalone 拥有与 63 个非 standalone 拥有身份的 75 个 init 目标；
- 122 个 `csc_end` 与 16 个 `rts` 源 token；
- 与完整私有程序/引用内容分开的受限公开元数据；
- `0x5E772` 的唯一 research-index 追踪锚点 `map.data.cs-5e772`。

### 推断

- 源标签与命令名暗示组织角色。其可能叙事、呈现或运行时含义不被提升进合同。

### 未知

- 每个程序的正常故事准入、路线排序、条件可达性、执行频率与调用方状态；
- 命令、分支、子程序与目标效果，包括持久性与生命周期后果；
- 实体朝向/移动、摄像机、文本、音频、等待、自定义子程序、调色板处理、渲染与帧时序；
- 已接受溯源之外的个别程序物理字节跨度与运行时成本；
- 畸形、截断、未解析、注入或替换程序准入与恢复；
- 现代解释器/编辑器格式、本地化、可访问性、替换、许可与分发策略。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-map-scripts-static-v1`、固定基线、验证器、所有者文章与唯一 research-index 追踪锚点；
2. 私有保留全部 47 个有序源文件身份、代表标签/地址与源所有权，而不合并或重编号文件；
3. 私有保留全部 178 个非空程序、其标签/H1 地址/分类与全部 8,058 个源顺序操作；
4. 为 100 个带目标操作保留精确 opcode 身份、私有操作数文本与解析目标符号/地址；
5. 复现完整 178 标签引用拓扑及其 127/51/0 标签分区加 146/92 出现计数；
6. 保留 75 目标所有权连接与精确十二个 standalone 目标身份，而不声称运行时准入；
7. 复现八路源分类与公开十二行命令直方图，而不从名称指定行为含义；
8. 通过私有或合成测试检测缺失/重排文件、丢失标签、空程序、操作重排、未解析目标、引用图漂移、所有权漂移与意外公开内容披露；
9. 允许独立编译器/解释器表示，而不要求原版宏、寄存器使用、指令顺序、处理器实现、调度或存档布局；
10. 只发布受限聚合元数据/溯源，绝不发布原版程序体、操作数、完整图、hash、提取内容或捕获；
11. 把故事、持久性、实体/摄像机/文本/音频/调色板效果、时序、畸形输入、替换、本地化与可访问性留给独立所有者或 **未知**。

H4 实现可以在导入时间解析源码形状私有输入，或消费单独生成的私有中间表示。任一选择只在已接受身份/顺序/引用事实与公开不披露边界保持独立可测试时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 文件/程序语料 | **已确认静态** | `sf2-map-scripts-static-v1`；[fixture](../../../../tests/fixtures/h2/map-scripts-static-v1.json) | 47 个文件、178 个私有程序、8,058 个操作；无运行时准入声称 |
| 标签分类法 | **已确认源分类** / **推断含义** | 同一 fixture 与固定源 | 精确前缀派生计数；无故事、实体、调色板或呈现含义 |
| 目标与引用图 | **已确认静态** | 同一 fixture；[map-data 研究](../../../research/map-data-inventory.md) | 完整私有拓扑与受限公开总计；词法引用不是可达性 |
| init 目标所有权连接 | **已确认静态** | 同一 fixture/验证器 | 75 = 12 standalone + 63 非 standalone；无选择或分发行为 |
| 命令直方图 | **已确认源计数** | 同一 fixture | 公开前十二与私有完整直方图；不是运行时频率或命令效果 |
| 完整解释器/运行时行为 | 独立所有者 / **未知** | [map exploration](../../contracts/map-exploration.md)及其 H2/H3 所有者 | 本合同拥有编写程序数据，而非处理器、效果、调度或呈现 |
| 聚合 map-data 成员 | 排除执行所有者 | `sf2-map-data-static-v1` | 无聚合注册或兄弟 `map.data.*` 关联 |
| 公开原版内容 | 禁止 | 版权/私有输入边界 | 仅聚合元数据；完整程序、操作数、图、hash 与捕获保持私有 |

## 开放问题

1. 哪些 standalone 程序经正常原版故事路线到达、以什么顺序、带什么调用方状态？
2. 哪些命令、实体、摄像机、文本、音频、调色板与等待效果需要既有处理器局部接缝之外的分组运行时观察？
3. 重制应对畸形或刻意修改私有脚本采用什么验证、诊断、替换、本地化与编辑器策略，而不暴露原版内容？

## 复现

```powershell
uv run sf2 h2 map-scripts
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/map-scripts-static.json` 下。公开验收使用受限元数据与溯源，而非原版程序体、操作数文本、完整目标/引用图、体 hash、提取内容或捕获。
