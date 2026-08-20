# 地图瓦片集数据合同

- **已确认原版结构：** 有序 115 条目地图瓦片集指针表、带固定 4,096 字节解码形式的 115 个 Stack 压缩源资源、完整表/载荷一致性、私有 79 地图五槽引用关系、私有 32 动画引用关系，以及下文描述的受限公开使用摘要。
- **推断原版行为：** 此处不提升任何内容。源符号与所有者文章识别地图图形与动画使用，但不证明最终渲染含义或运行时可达性。
- **未知原版行为：** 每个地图与动画引用的自然可达性、对 `MapTileset029` 的动态或编码访问、运行时缓存/重载/修改与持久性、动画调度、调色板与 VRAM 放置、VInt/DMA 节奏、转移完成、帧组合、最终渲染、畸形或替换输入策略、可访问性处理与玩家面向含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择运行时图形格式、渲染器、缓存、动画系统、替换资源策略或分发许可。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-tileset-data.md`](../../contracts/map-tileset-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版地图瓦片集语料的静态存储、解码、引用、一致性与私有导入边界：

1. 顶层指针与有序 115 条目资源表；
2. 115 个不同压缩源身份与 115 个固定大小解码身份；
3. 含五个有序瓦片集槽的 79 个有序私有地图头记录；
4. 引用瓦片集与瓦片计数元数据的 32 个有序私有动画头记录；
5. 受限公开总计、使用计数、分布、一致性结果与溯源。

此处消费的唯一可执行所有者是 fixture id `sf2-map-tileset-decode-v1`，位于 [`tests/fixtures/h2/map-tileset-decode-v1.json`](../../../../tests/fixtures/h2/map-tileset-decode-v1.json)。其带来源所有者是[Map Content Tables 与 Binary Payload 一致性](../../../research/map-content.md)与[Technical Graphics](../../../research/technical-graphics.md)。受限源根是 `data/graphics/maps/maptilesets/entries.asm`、115 个私有压缩资源文件、`data/maps/entries/mapXX/00-tilesets.asm` 下的 79 个地图头源、`data/maps/entries/mapXX/9-animations.asm` 下的 32 个在场动画头与 `code/common/maps/mapload.asm` 中的资源消费者身份。

精确未来 research-index 关联只有 `auxiliary.data.pt-maptilesets`。fixture 的 `p_pt_MapTilesets`、`LoadStackCompressedData`、`LoadMapTilesets` 与 `LoadMapArea` 身份保持受限溯源。它们不获得本合同，也不把私有静态资源合同变成指针接口、解压服务、地图生命周期、动画、转移或渲染合同。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-tileset-decode-v1
SHA256 2EA6AB3485CAE4F92F31647C05233F0E1C07E81CCB02806706A51F9F0C1E087F
Tilesets 115 / DecodedBytes 471040 / UsedTilesets 114 / PASS
```

fixture 直接绑定恰好一条 research-index 记录：

- `auxiliary.data.pt-maptilesets`——唯一、当前未关联，且是本合同唯一未来关联。

该记录还携带宽 `sf2-auxiliary-data-static-v1` 清单所有者。本合同不消费该聚合 fixture、不导入任何兄弟记录，也不把聚合清单用作该瓦片集语料的权威。只有 `sf2-map-tileset-decode-v1` 是本文档的可执行证据。

没有 `tech.interfaces.ptr-s08`、`LoadStackCompressedData`、`LoadMapTilesets`、`LoadMapArea`、`map.data.*`、地图头、动画、图形服务、中断、DMA 或呈现记录获得本合同。那些身份保持溯源、私有引用输入或独立所有者边界，而非语义关联。

受追踪 fixture 只包含受限元数据：地址、语料维度、压缩与解码总计、一致性计数、引用总计、使用摘要、聚合解码器诊断、一个未使用索引、一个动画瓦片计数分布与两个运行时问题。原始压缩字节、解码美术、逐资源 hash、个别压缩大小、完整地图槽赋值、完整动画赋值与渲染捕获保持私有/生成。

## 身份与指针拓扑

**已确认静态：** `p_pt_MapTilesets` 是 ROM `0x64000`（`409600`）的顶层指针。它解析到 `pt_MapTilesets`，后者位于 ROM `0x6400C`（`409612`）。表包含按索引解析到 `MapTileset000` 到 `MapTileset114` 的 115 个有序 longword 条目。

保留为溯源的已接受加载器/服务身份：

| 身份 | ROM 地址 | 合同含义 |
| --- | ---: | --- |
| `LoadStackCompressedData` | 7752 | 仅 Stack 解码器条目身份 |
| `LoadMapTilesets` | 10722 | 仅地图瓦片集消费者身份 |
| `LoadMapArea` | 11756 | 仅地图区域/动画消费者身份 |

全部 115 个指针表条目与全部 115 个压缩载荷范围匹配已接受 source、H1 与 ROM 边界。fixture 把表与载荷一致性保留为两个独立 `115/115` 计数器；指针一致性不替代载荷一致性。

私有导入器必须保留每个资源的顺序、数字索引、源符号、源路径、源地址与 source/解码身份。它不得仅因压缩大小、解码 hash 或可见外观内容匹配而去重或重编号资源。公开证据可以暴露表身份、维度、计数与一致性，但不得暴露资源载荷或逐资源 hash。

## 私有压缩与解码语料

**已确认静态：** 115 个私有源资源中的每一个都是 Stack 压缩且精确解码到 4,096 字节。完整已接受语料总计：

| 面 | 已接受值 |
| --- | ---: |
| 有序压缩身份 | 115 |
| 有序解码身份 | 115 |
| 聚合压缩字节 | 198,514 |
| 每资源解码字节 | 4,096 |
| 聚合解码字节 | 471,040 |
| 指针表一致性 | 115 |
| 载荷一致性 | 115 |

fixture 还记录聚合解码器诊断：

- 8,418 个命令组；
- 111,246 个字面字；
- 22,485 个复制命令与 124,274 个复制字；
- 32 到 47 尾位；
- 最大复制偏移 2,000 字；
- 最大复制长度 33 字。

这些诊断确认已接受语料与验证器边界。它们不是重制必须复现原版位读取器、历史存储、命令分组、复制循环、寄存器分配、指令顺序或尾位处理的要求。另一私有解码器在复现已接受有序解码输出与溯源而不暴露原版内容时合规。

私有导入保留压缩与解码字节及其 hash 供验证。公开合同只保留聚合字节计数、固定解码大小、一致性、诊断与溯源。两种形式都不把瓦片语义、调色板选择、布局位置、透明度、动画角色或玩家可见含义指定给个别解码字节。

## 有序地图引用边界

**已确认静态：** 79 个私有地图头各包含五个有序瓦片集槽。跨完整 `79 * 5 = 395` 个位置：

| 面 | 已接受计数 |
| --- | ---: |
| 真实瓦片集引用 | 326 |
| 缺席槽哨兵位置 | 69 |
| 唯一普通引用瓦片集索引 | 100 |

每个真实引用在索引 `0..114` 内；每个缺席位置存储已接受 `255` 哨兵。全部 79 个六字节调色板/瓦片集头记录匹配专用所有者检查的 source/H1/ROM 边界。

私有导入器必须保留每个地图索引与全部五个槽位置（包括每个哨兵）的源顺序。它不得把关系折叠为 100 个引用索引集、压缩五个位置，或仅从序号位置给槽指定发明玩法角色。

公开投影只保留 79 地图、395 位置、326 引用、69 哨兵与 100 唯一索引总计。它不发布完整地图到槽赋值图。静态引用不建立自然故事可达性、访问顺序、实际加载频率或最终渲染。

共享六字节地图头的调色板字节属于[map-palette-data](../../contracts/map-palette-data.md)。本合同验证瓦片集引用边界，而不声称或复制调色板身份、颜色零变换或呈现语义。

## 有序动画引用边界

**已确认静态：** 32 张地图有私有四字节动画头。每个头贡献一个瓦片集索引与一个已接受瓦片计数值。引用面包含：

| 面 | 已接受计数 |
| --- | ---: |
| 带动画地图 | 32 |
| 动画瓦片集引用 | 32 |
| 唯一动画瓦片集索引 | 15 |

公开瓦片计数分布：

| 瓦片计数 | 头计数 |
| ---: | ---: |
| 4 | 1 |
| 16 | 1 |
| 32 | 3 |
| 64 | 13 |
| 96 | 14 |
| **总计** | **32** |

私有导入器保留每个动画头的有序地图索引、源路径、头地址、瓦片集索引与瓦片计数。公开投影只保留总计与分布，而非完整赋值列表。

动画头证明静态引用与受限元数据形状。它不证明回调节奏、缓存布局、替换顺序、VRAM 目标、可见动画、帧时序、正常故事可达性或已接受 source/消费者边界之外的瓦片计数字段含义。那些行为保持[map-exploration](../../contracts/map-exploration.md)及其运行时所有者。

## 组合静态使用边界

**已确认静态：** 普通地图槽与动画头引用的并集到达 115 个资源索引中的 114 个。两个完整静态源面都缺席的唯一索引是 `29`，身份 `MapTileset029`。

这只是静态缺席结果。它不得被改写成死代码、运行时不可达性、每个原版模式中的未使用内容或删除/重编号资源的许可。动态或编码写入、调试路径、原始 RAM 注入、修改内容与其他调用方行为保持 **未知**。

合规私有导入保留全部 115 个身份，包括索引 29。它可以报告已接受 `combinedUsedTilesetCount=114`、`unusedTilesetCount=1` 与 `unusedTilesetIndices=[29]` 作为公开元数据，但不得基于那些计数器丢弃私有资源。

## 实现无关导入模型

最小完整逻辑导入把私有载荷与赋值和公开元数据分开：

```text
MapTilesetCorpus {
  privateResources[115]: PrivateMapTilesetResource
  privateMapHeaders[79]: PrivateMapTilesetHeader
  privateAnimationHeaders[32]: PrivateMapAnimationTilesetHeader
  publicSummary: MapTilesetPublicSummary
}

PrivateMapTilesetResource {
  tilesetIndex
  sourceSymbol
  sourcePath
  sourceAddress
  privateCompressedBytes
  privateDecodedBytes[4096]
  privateCompressedHash
  privateDecodedHash
}

PrivateMapTilesetHeader {
  mapIndex
  sourcePath
  mapAddress
  tilesetSlots[5]  // ordered indices or accepted 255 sentinel
}

PrivateMapAnimationTilesetHeader {
  mapIndex
  sourcePath
  headerAddress
  tilesetIndex
  tileCount
}

MapTilesetPublicSummary {
  fixtureId = "sf2-map-tileset-decode-v1"
  topLevelPointerAddress = 409600
  pointerTableAddress = 409612
  resourceCount = 115
  fixedDecodedBytesPerResource = 4096
  aggregateCompressedByteCount = 198514
  aggregateDecodedByteCount = 471040
  tableParityCount = 115
  payloadParityCount = 115
  mapCount = 79
  mapSlotCount = 395
  mapReferenceCount = 326
  absentMapSlotCount = 69
  uniqueMapReferenceCount = 100
  animationMapCount = 32
  animationReferenceCount = 32
  uniqueAnimationReferenceCount = 15
  animationTileCountDistribution
  combinedUsedResourceCount = 114
  unusedResourceIndices = [29]
  aggregateDecoderDiagnostics
  fixtureProvenance
}
```

这是私有导入/溯源模型，不是必需渲染器 API、GPU 纹理格式、缓存、场景图、动画调度器或资源包布局。重制只有在仍能验证已接受身份、顺序、引用关系、解码结果与刻意变换溯源时，才可以把私有解码数据变换为另一内部表示。

公开投影不得包含压缩载荷、解码美术、逐资源 hash、个别压缩大小、完整地图槽赋值、完整动画赋值、渲染捕获或其他原版内容。公开报告只能保留受限元数据、聚合计数、分布、一致性结果、未使用索引结果、地址与溯源。

## 跨系统分离

本合同不拥有：

- 地图选择、加载时间线、重载、工作状态、区域选择、运行时块变更、动画调度或持久性，仍归[map-exploration](../../contracts/map-exploration.md)及其证据所有者；
- blockset、解码 64×64 布局、别名、碰撞与可通过性，仍归[map-layout-data](../../contracts/map-layout-data.md)与相邻地图所有者；
- 调色板身份、颜色字、颜色零变换、淡入或调色板呈现，仍归[map-palette-data](../../contracts/map-palette-data.md)与图形所有者；
- Stack 解压服务 ABI 或任何必需编解码器微实现，仍归[graphics-service-state](../../contracts/graphics-service-state.md)与 technical-graphics 所有者；
- VInt 调度、CRAM/VRAM DMA、中断节奏、转移完成与硬件时序，仍归[interrupt-dma-and-trap-state](../../contracts/interrupt-dma-and-trap-state.md)；
- 地图精灵图形、特殊精灵、UI 图形、战斗图形、立绘、瓦片集到布局组合、瓦片语义、动画帧或最终可见呈现；
- `sf2-auxiliary-data-static-v1`、`tech.interfaces.ptr-s08`、`map.data.*`、函数/服务记录与所有兄弟关联；
- 私有原版压缩字节、解码美术、hash 与完整赋值图；
- 畸形、截断、越界、注入、修改或替换输入准入；
- 可访问性重映射、本地化、故事含义、平衡或分发策略。

[map-design principles 综合](../synthesis/map-design-principles.md) 可以在保留这些分离的同时消费受限静态资源事实。它不得把静态引用计数变成正常故事可达性，也不得把 source/ROM 一致性变成渲染等价。

## 判断边界

### 已确认

- 通过 `sf2-map-tileset-decode-v1` 与 `auxiliary.data.pt-maptilesets` 的 fixture/源溯源；
- 精确顶层指针、表、解码器条目、`LoadMapTilesets` 与 `LoadMapArea` 溯源身份/地址；
- 115 个有序私有压缩与解码资源身份；
- 固定 4,096 字节解码大小、198,514 压缩字节与 471,040 解码字节；
- 完整 115 指针与 115 载荷一致性；
- 79 个有序私有五槽地图头：395 位置、326 引用、69 哨兵与 100 个唯一普通索引；
- 32 个有序私有动画头、15 个唯一动画索引与精确公开瓦片计数分布；
- 114 个资源的组合静态使用与索引 29 在两个完整引用面的静态缺席；
- 聚合解码器诊断作为语料验证元数据，而非必需解码器算法；
- 公开元数据/私有原版内容分离。

### 推断

- 本合同不提升任何内容。

### 未知

- `MapTileset029` 的动态、编码、调试、注入或修改内容可达性；
- 每个引用的自然故事可达性、加载频率、缓存/重载行为与持久性；
- 运行时修改、动画调度、替换顺序与最终瓦片集到布局映射；
- 调色板选择、VRAM 放置、VInt/DMA 节奏、转移完成、帧组合与渲染呈现；
- 畸形或替换输入准入、诊断与回退行为；
- 现代运行时格式、渲染器、缓存、可访问性变换、替换资源与分发策略。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-map-tileset-decode-v1`、固定基线与已接受指针/表/消费者溯源身份；
2. 私有保留 115 个有序压缩身份与 115 个有序解码身份，不去重、重编号或丢弃索引 29；
3. 从私有已接受输入复现每资源精确 4,096 字节解码形状与聚合 198,514 压缩 / 471,040 解码字节计数；
4. 在保持原版载荷与逐资源 hash 私有时验证完整 115 条目指针表与 115 载荷一致性；
5. 私有保留全部 79 个有序五槽地图头（包括 326 引用与 69 哨兵位置），同时公开只保留受限聚合计数；
6. 私有保留全部 32 个有序动画头并公开复现其引用总计、15 唯一索引计数与精确瓦片计数分布；
7. 复现组合 114 已用/一个静态未引用结果，而不声称运行时不可达性或丢弃 `MapTileset029`；
8. 通过私有或合成测试检测指针重排、资源重编号、载荷截断、解码大小漂移、丢失哨兵、地图槽重分配、动画头重分配与私有源丢失；
9. 允许独立解码器实现，而不要求原版命令分组、复制循环、历史表示、尾位行为、寄存器使用或指令顺序；
10. 把原始压缩字节、解码美术、hash、完整赋值图、截图与其他原版内容保持公开 fixture 与报告之外；
11. 通过独立所有者报告地图生命周期、动画、持久性、调色板/VRAM 放置、VInt/DMA、呈现、畸形输入、替换与可访问性策略，或作为 **未知**。

H4 可以在导入构建、惰性或运行时前解码与变换私有瓦片集数据。那些选择只在已接受身份/顺序/引用图、解码输出证据与公开不披露边界保持独立可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 指针与载荷语料 | **已确认静态** | `sf2-map-tileset-decode-v1`；[fixture](../../../../tests/fixtures/h2/map-tileset-decode-v1.json) | 115 个有序私有资源、固定解码形状、精确一致性；载荷/hash 保持私有 |
| 普通地图引用 | **已确认静态** | 同一 fixture；[map-content 研究](../../../research/map-content.md) | 79 个私有五槽头与公开总计；无自然可达性或运行时生命周期声称 |
| 动画引用 | **已确认静态** | 同一 fixture；map-content/technical-graphics 所有者 | 32 个私有头与公开分布；无节奏、缓存、转移或可见帧含义 |
| 组合静态使用 | **已确认静态缺席** | 同一 fixture | 114 已引用且索引 29 缺席静态面；运行时不可达性保持未知 |
| 解码器诊断 | **已确认静态元数据** | 同一 fixture；[technical-graphics 研究](../../../research/technical-graphics.md) | 语料诊断不规定编解码器微实现 |
| auxiliary 聚合 | 排除执行所有者 | `sf2-auxiliary-data-static-v1` | 宽清单此处不提供注册或兄弟关联 |
| 地图构建与动画生命周期 | 独立所有者证据 | [map-exploration](../../contracts/map-exploration.md) | 私有资源语料不拥有加载顺序、变更、缓存、持久性或动画运行时 |
| 解压、DMA 与渲染 | 独立所有者 / **未知** | [graphics service](../../contracts/graphics-service-state.md)；[interrupt 合同](../../contracts/interrupt-dma-and-trap-state.md) | 溯源身份不证明转移完成、时序或最终呈现 |

## 开放问题

1. 未来分组运行时轨道能否测试 `MapTileset029` 是否经任何原版动态或编码索引写入可达，而不发布其载荷或解码美术？
2. 哪些原版运行时路径在初始加载后缓存、替换或重载解码地图瓦片集，动画引用如何与那些路径交互？
3. 重制导入器应对越界索引、截断压缩输入、解码大小漂移或刻意修改资源使用什么显式验证与替换策略？

## 复现

```powershell
uv run sf2 h2 map-tilesets
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/map-tileset-decode.json` 下。公开验收使用受限元数据与溯源，而非原版压缩载荷、解码美术、逐资源 hash 或完整地图/动画赋值图。
