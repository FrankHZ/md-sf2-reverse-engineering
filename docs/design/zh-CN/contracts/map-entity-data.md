# 地图实体数据合同

- **已确认原版结构：** 126 个指向 125 个唯一实体列表根的配置指针引用、一个重复根、产生 987 个有序列表记录引用的 980 个物理八字节记录、精确后缀共享与终止符拓扑、已接受记录种类与源宏计数、受限初始地图精灵域，以及下文描述的消费者解码规则。
- **推断原版行为：** 只有源宏与字段名暗示的放置与移动意图；不从那些名称提升任何玩法、呈现或运行时生命周期行为。
- **未知原版行为：** 自然故事选择与可达性、运行时列表准入与重载持久性、畸形或注入流处理、序列方向消费、随从/声明碰撞状态、行走特殊精灵呈现时序、已接受观察之外的实体容量、碰撞、寻路、动作效果、渲染、VDP 时序、对话、AI 与平衡。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择运行时实体模型、地图编辑器格式、移动系统、渲染器、替换数据集或分发许可。
- 证据日期：2026-08-12
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-entity-data.md`](../../contracts/map-entity-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版地图实体列表语料的静态存储、引用、编码与私有导入边界。它拥有五个可分离面：

1. 配置指针引用与唯一实体列表根身份；
2. 物理记录存储对列表遍历引用；
3. 共享后缀、fallthrough 片段与终止符身份；
4. 固定、行走与序列记录种类清单；
5. 受限初始地图精灵值与 `InitializeMapEntities` 消费者规则。

可执行所有者是 fixture id `sf2-map-entities-static-v1`，位于 [`tests/fixtures/h2/map-entities-static-v1.json`](../../../../tests/fixtures/h2/map-entities-static-v1.json)。研究所有者是[Map Data Inventory 与 Setup/Event Surfaces](../../../research/map-data-inventory.md)。本合同消费 fixture 的完整 `expected` 对象与 ROM 地址 `278732`（`0x440CC`）的受限 `InitializeMapEntities` 溯源。

精确未来 research-index 关联只有 `scripting.map.mapfunctions`。函数身份与地址把静态消费者绑定到其源所有者；它们不使本合同成为地图配置选择、运行时实体人口、实体动作、移动或呈现的所有者。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-entities-static-v1
SHA256 BFC583155F1D6EE490877A0B0CA2CBBE13DF145A99EE25CA72228A4EA4A2CA4A
SourceFiles 125 / EntityLists 125 / PhysicalRecords 980 / ListReferences 987 /
FallthroughFragments 9 / PASS
```

H2 fixture 直接绑定恰好一条 research-index 记录：

- `scripting.map.mapfunctions`——唯一、当前未关联，且是本合同唯一未来关联。

相邻 `sf2-entity-population-reload-runtime-v1` fixture 此处不消费。其精确索引链接分母是十条记录：

- 同一候选 `scripting.map.mapfunctions`；
- 八条既有[map-exploration](../../contracts/map-exploration.md)关联保持不变的记录：`scripting.map.mapsetupsfunctions-1`、`map.entity-population.get-entity-address`、`map.entity-population.initialize-new-entity`、`map.entity-population.load-entity-mapsprites`、`map.entity-population.load-from-map-setup`、`map.entity-population.load-map-entities`、`map.entity-population.new-entity` 与 `map.entity-population.reload-entities`；
- 未关联的 `map.setup.entity-list`，保持独立配置选择边界，不获得本合同。

这是精确 `1 + 8 + 1` 分区。H3 成员不扩大本合同证据或关联集。特别是，七条 `map.entity-population.*` 记录只是八条不变 map-exploration 组的一部分；它们不是完整运行时分母。

受追踪 fixture 包含聚合计数、源身份、fallthrough 关系、地址、受限地图精灵统计与解码规则。完整数字实体行、私有源派生输出、载荷 hash、渲染捕获与模拟器状态保持私有/生成。

## 指针与列表根拓扑

**已确认静态：** 配置语料包含解析到 125 个唯一实体列表根的 126 个实体指针引用。唯一重复目标是 `ms_map21_Entities`。导入器必须保留 126 个有序引用与共享目标身份，而非把它们扩展为 126 个匿名列表或缩减为 125 个符号的无序集。

匹配源边界包含 125 个 `s1_entities*.asm` 文件。源文件数、指针数与唯一列表数是不同分母：

| 面 | 已接受计数 | 必需区分 |
| --- | ---: | --- |
| 源文件 | 125 | 物理源所有权，非配置引用基数 |
| 配置指针引用 | 126 | 有序引用，包括重复目标 |
| 唯一实体列表根 | 125 | 解码根身份，非独立终止符 |
| 唯一终止符地址 | 116 | 由 fallthrough 拓扑共享；非每根一个 |

有 30 个空所选列表，最大所选列表包含 31 个记录引用。这些是语料观察，不是运行时容量、引擎分配限制、通用地图基数或每个列表自然被选择的证明。

## 物理存储与引用计数

**已确认静态：** 源拥有 980 个物理八字节记录。经其已接受终止符遍历每个唯一列表根产生 987 个有序记录引用。七引用差来自刻意后缀共享；它不是七个额外存储记录。

| 记录种类 | 物理记录 | 逐列表引用 | 共享引用差 |
| --- | ---: | ---: | ---: |
| 固定 | 803 | 808 | 5 |
| 行走 | 174 | 175 | 1 |
| 序列 | 3 | 4 | 1 |
| **总计** | **980** | **987** | **7** |

对应源宏清单：

| 源宏 | 使用 | 存储分类 |
| --- | ---: | --- |
| `entity` | 9 | 固定 |
| `entityRandomWalk` | 5 | 行走 |
| `msFixedEntity` | 794 | 固定 |
| `msWalkingEntity` | 169 | 行走 |
| `msSequencedEntity` | 3 | 序列 |

宏名与分类保留源身份。它们不建立可见移动、随机分布、寻路策略、动画时序、动作结果或 AI 行为。

私有导入器必须把物理记录身份与列表引用分开保留。把共享后缀复制进每个逻辑列表内部可能方便，但原版保真诊断仍必须报告复制引用源自同一物理记录。

## Fallthrough 与终止符拓扑

**已确认静态：** 九个源片段省略局部 `msEntitiesEnd`。八个落入相邻仅终止符片段；一个贡献五记录前缀然后共享七记录后缀。

| 前缀符号 | 前缀记录 | Fallthrough 符号 | 共享后缀记录 |
| --- | ---: | ---: | ---: |
| `ms_map17_Entities` | 5 | `ms_map17_flag505_Entities` | 7 |
| `ms_map20_flag609_Entities` | 8 | `ms_map20_flag506_Entities` | 0 |
| `ms_map21_flag609_Entities` | 1 | `ms_map21_flag506_Entities` | 0 |
| `ms_map27_Entities` | 3 | `ms_map27_flag523_Entities` | 0 |
| `ms_map34_Entities` | 21 | `ms_map34_flag784_Entities` | 0 |
| `ms_map40_flag506_Entities` | 3 | `ms_map40_Entities` | 0 |
| `ms_map43_Entities` | 3 | `ms_map43_flag612_Entities` | 0 |
| `ms_map61_Entities` | 1 | `ms_map61_flag729_Entities` | 0 |
| `ms_map63_Entities` | 1 | `ms_map63_flag29_Entities` | 0 |

fixture 保留每行的精确源路径、fallthrough 地址与两个符号。那些溯源字段保持私有导入验证的一部分，即使本公开合同不复现每个地址。

首字节值 `255` 终止消费者遍历。它是记录流边界，不是第八数据字节、该位置的地图精灵值或通用畸形流恢复策略。发明九个局部终止符的逐文件解析器会擦除原版存储图；在每个文件边界停止的解析器会丢失带记录的 map 17 后缀关系。

## 记录编码与消费者边界

**已确认静态：** 每个物理实体记录占用八字节。已接受字段顺序：

1. X 坐标；
2. Y 坐标；
3. 朝向；
4. 地图精灵身份；
5. 动作或行走载荷。

`InitializeMapEntities` 按流顺序消费记录、用 `63` 掩蔽每个坐标、按符号地图瓦片大小缩放掩蔽值，并把特殊地图精灵路由到特殊实体声明路径。这些只是消费者解码事实。

合同不把现代坐标单位、瓦片维度、世界空间缩放、碰撞含义、生成生命周期或屏幕位置指定给掩蔽/缩放值。它也不定义每个宏族的完整载荷子布局、动作执行、行走状态、序列流消费、槽分配或调用方可视失败行为。

流顺序必须保留为源身份。重制可以把记录编译为类型化私有对象，但兼容导入不能重排外观相等行、按值去重或丢弃玩法含义尚未命名的字节。

## 初始地图精灵边界

**已确认静态：** 980 个物理记录包含观察范围 `1..255` 内的 113 个不同地图精灵字节值。这是稀疏观察集，不是连续或闭合域的证明。

- 977 个物理记录使用低于 240 的普通身份；
- 三个物理记录各一次使用路由特殊身份 `251`、`252` 与 `255`；
- 高普通身份计数 `230:7`、`231:2`、`232:1`、`233:26`、`234:11`、`235:5` 与 `236:29`；
- 共享哨兵普通身份 `237..239` 缺席；
- 无支撑特殊身份 `240..250` 缺席。

逐列表引用直方图对这些高值匹配物理直方图。三个特殊身份此处只保留其已接受声明路线分类。特殊精灵载荷、指针/分发支撑、渲染器行为、可见时序与呈现保持[graphics-service-state](../../contracts/graphics-service-state.md)或 **未知**。

该域只闭合初始实体列表源记录。后续过场、实体动作、战斗员派生、调试、畸形与直接 RAM 赋值面保持独立。该语料缺席不证明身份全局不可达或不被每个原版调用方支持。

## 实现无关导入模型

最小完整逻辑导入把存储、引用、遍历与公开元数据分开：

```text
MapEntityCorpus {
  setupPointerReferences[126]: EntityListPointerReference
  listRoots[125]: EntityListRoot
  physicalRecords[980]: PrivateEntityRecord
  terminators[116]: TerminatorIdentity
  publicSummary: MapEntityPublicSummary
}

EntityListPointerReference {
  setupIdentity
  orderedSlotIdentity
  listRootRef
}

EntityListRoot {
  sourcePath
  sourceSymbol
  privatePrefixRecordRefs[]
  optionalFallthroughRootRef
  terminatorRef
}

PrivateEntityRecord {
  physicalAddress
  sourceMacroIdentity
  kind: FIXED | WALKING | SEQUENCED
  privateRawBytes[8]
  privateDecodedFields
}

TerminatorIdentity {
  physicalAddress
  firstByte = 255
}

MapEntityPublicSummary {
  sourceFileCount = 125
  pointerReferenceCount = 126
  uniqueListRootCount = 125
  physicalRecordCount = 980
  listRecordReferenceCount = 987
  sharedSuffixReferenceCount = 7
  uniqueTerminatorCount = 116
  fallthroughFragmentCount = 9
  emptyListCount = 30
  maximumListRecordCount = 31
  physicalKindCounts = { fixed: 803, walking: 174, sequenced: 3 }
  referenceKindCounts = { fixed: 808, walking: 175, sequenced: 4 }
  sourceMacroCounts
  mapSpriteSummary
  consumerRules
  fixtureProvenance
}
```

该模型是导入合同，不是引擎实体组件布局或运行时生成 API。`private*` 字段保持用户 source/ROM 验证过程本地。公开投影可以保留受限计数、符号、源路径、地址、拓扑、范围、直方图与 fixture 溯源。它不得发布完整数字行、原始记录字节、私有 hash、地图放置、动作载荷或渲染捕获。

## 跨系统分离

本合同不拥有：

- 配置选择、配置指针槽语义、地图切换、存档点或战斗准入，仍归[map-entry-routing-state](../../contracts/map-entry-routing-state.md)、地图配置证据或 **未知**；
- 运行时人口、重载、新实体分配、map-script 放置、工作状态或持久性，仍归[map-exploration](../../contracts/map-exploration.md)及其已接受 H3 所有者；
- block/layout 载荷与不可变地图几何，仍归[map-layout-data](../../contracts/map-layout-data.md)；
- 事件表匹配、实体动作程序、移动执行、碰撞、寻路、AI、对话、物品、队伍、战斗或故事行为；
- 初始实体列表记录之外的地图精灵赋值域、特殊精灵载荷、图形路由、渲染器状态、VDP/DMA 节奏、动画或最终呈现；
- 私有原版实体行、地图内容、图形、对话或音频载荷；
- 畸形、截断、未终止、注入、替换或 mod 流准入；
- 可访问性、本地化、难度、平衡或战役意图。

[map-design principles 综合](../synthesis/map-design-principles.md) 可以在保留这些分离的同时消费已接受静态计数与拓扑。它不得把列表成员变成自然故事可达性，也不得用聚合计数作为运行时容量声称。

## 判断边界

### 已确认

- 通过 `sf2-map-entities-static-v1` 与 `scripting.map.mapfunctions` 的 fixture/源溯源；
- 125 个源文件、126 个指针引用、125 个唯一列表根与精确 `ms_map21_Entities` 重复目标身份；
- 980 个物理记录、987 个列表记录引用与精确固定/行走/序列物理与引用计数；
- 116 个唯一终止符、九个 fallthrough 片段、一个七记录共享后缀、30 个空列表与最大所选列表长度 31；
- 精确源宏计数、八字节记录形状、字段顺序、终止符、坐标掩码/缩放、流顺序与特殊声明规则；
- 113 个不同初始地图精灵值、受限高值直方图、三个路由特殊 ID 与已接受缺席哨兵/无支撑范围；
- 公开元数据/私有原版行分离。

### 推断

- 源宏与字段名暗示放置与移动角色，但本合同不从那些标签提升任何运行时结果。

### 未知

- 三个 fixture 拥有的运行时问题：序列实体方向流消费、随从与地图实体声明碰撞状态、行走特殊精灵/实体呈现时序；
- 自然配置/故事选择、运行时准入、重载/存档持久性与实体槽容量；
- 畸形或注入流行为与调用方可视诊断；
- 动作、移动、碰撞、寻路、AI、对话、战斗与故事效果；
- 图形载荷、渲染器行为、VDP/DMA 时序、动画与可见呈现；
- 现代替换数据准入、编辑器行为、可访问性、本地化与平衡。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-map-entities-static-v1`、固定基线与地址 `278732` 的 `InitializeMapEntities`，而不声称运行时生命周期行为；
2. 保留到 125 个列表根的 126 个有序配置指针引用，包括精确重复 `ms_map21_Entities` 目标；
3. 把 980 个物理记录与 987 个有序列表引用分开保留，并复现精确固定/行走/序列物理与引用计数；
4. 保留全部九个 fallthrough 关系、116 个终止符身份与 map 17 七记录共享后缀，而不发明局部终止符或压平物理身份；
5. 把 30 个空列表与最大所选列表长度 31 保留为语料事实，而不把它们呈现为引擎容量或自然可达性；
6. 本地保留每个私有记录的八个原始字节与源顺序，加已接受字段顺序、首字节终止符、坐标掩码/缩放与特殊声明路线；
7. 复现已接受 113 不同值初始地图精灵摘要、高值直方图、`251/252/255` 特殊集与缺席 `237..250` 子范围，而不把稀疏观察值转换成连续全局域；
8. 通过合成或私有导入测试检测指针扩展、列表去重、无溯源后缀复制、记录重排、丢失原始字节、发明终止符与改变聚合计数；
9. 把完整数字实体行、原始字节、私有 hash、地图放置、动作载荷与渲染输出保持公开 fixture 与报告之外；
10. 把精确 H3 分母分开为一个候选、八条不变 map-exploration 记录与一条不变配置选择记录，而不在此添加那些兄弟关联；
11. 通过其独立所有者报告运行时人口/重载、配置选择、移动/动作、碰撞/寻路、持久性、故事、渲染、时序、畸形输入与玩家面向行为，或作为 **未知**。

H4 可以把导入记录存储为类型化组件、不可变 blob、编译生成描述符或其他私有表示。那些选择只在完整存储/引用图与往返身份保持可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| fixture 与消费者溯源 | **已确认静态** | `sf2-map-entities-static-v1`；[fixture](../../../../tests/fixtures/h2/map-entities-static-v1.json) | `InitializeMapEntities` 身份/地址与解码规则；无生命周期或第二条关联 |
| 指针/列表根拓扑 | **已确认静态** | 同一 fixture；[map-data 研究](../../../research/map-data-inventory.md) | 126 引用、125 根、精确重复目标；无配置选择含义 |
| 物理/引用语料 | **已确认静态** | 同一 fixture | 980 物理对 987 引用记录与精确种类/宏计数；无运行时容量 |
| fallthrough/终止符 | **已确认静态** | 同一 fixture | 九个片段、116 个终止符、一个七记录共享后缀；畸形恢复保持开放 |
| 初始地图精灵值 | **已确认静态** | 同一 fixture | 113 个不同值、精确高值计数与受限特殊/缺席集；无全局赋值或呈现声称 |
| 实体人口/重载分母 | 排除运行时所有者 | `sf2-entity-population-reload-runtime-v1`；[map-exploration](../../contracts/map-exploration.md) | 精确 `1 + 8 + 1` 索引链接分区；不消费 H3 证据或兄弟关联 |
| 配置选择与地图布局 | 独立所有者证据 | 地图配置所有者；[map-layout-data](../../contracts/map-layout-data.md) | 实体列表数据不定义配置选择、布局、碰撞或地图生命周期 |
| 移动、动作、持久性、图形、时序与呈现 | 独立所有者 / **未知** | 相邻合同与未来运行时/呈现轨道 | 静态行与标签不证明效果、可达性、帧或玩家可见结果 |

## 开放问题

1. 原版消费者如何在已接受运行时状态间推进序列实体方向流？
2. 随从状态与地图实体声明竞争人口槽时适用什么碰撞或优先级规则？
3. 行走记录使用路由特殊精灵身份时适用什么运行时与呈现时序？
4. 哪些畸形流用例需要显式重制拒绝策略，而非未指定兼容结果？

## 复现

```powershell
uv run sf2 h2 map-entities
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/map-entities-static.json` 下。公开验收使用受限元数据与溯源，而非完整原版实体行。
