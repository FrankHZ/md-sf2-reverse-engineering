# 地图调色板数据合同

- **已确认原版结构：** 有序 16 条目地图调色板指针表、十六个 32 字节源调色板、完整指针/载荷/地图头 parity、有序 79 地图私有引用面与公开使用直方图、已接受 Genesis 颜色字掩码边界，以及下文描述的源到有效首字变换。
- **推断原版行为：** 此处不提升任何内容。源名与平台字段暗示调色板与颜色意图，但不证明最终可见含义。
- **未知原版行为：** 自然地图可达性、运行时调色板修改与缓存生命周期、重载或存档持久性、调色板动画、淡入与过渡行为、CRAM/VInt/DMA 节奏、转移完成、硬件颜色呈现、最终逐地图渲染、畸形或替换输入策略、可访问性重映射与玩家面向含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择渲染器、色彩空间转换、调色板动画模型、替换资源集、可访问性策略或分发许可。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-palette-data.md`](../../contracts/map-palette-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版地图调色板语料的静态存储、引用、parity 与私有导入边界：

1. 顶层调色板表指针与十六个有序指针表条目；
2. 十六个有序源调色板身份及其十六个派生有效身份；
3. 79 个有序私有地图头引用及其公开使用直方图；
4. 受限源颜色字计数与已接受 `0x0EEE` 掩码检查；
5. 源码形状调色板查找/复制/首字清除时间线。

此处消费的唯一可执行所有者是 fixture id `sf2-map-palette-static-v1`，位于 [`tests/fixtures/h2/map-palette-static-v1.json`](../../../../tests/fixtures/h2/map-palette-static-v1.json)。其带来源所有者是[Map Content Tables 与 Binary Payload Parity](../../../research/map-content.md)与[Technical Graphics](../../../research/technical-graphics.md)。受限源根是 `data/graphics/maps/mappalettes/entries.asm`、`data/maps/entries/mapXX/00-tilesets.asm` 下的 79 个地图头源与 `code/common/maps/mapload.asm`。

精确未来 research-index 关联只有 `auxiliary.data.pt-mappalettes`。`LoadMap` 与 `CopyBytes` 身份是该 fixture 内的证据；它们不获得本合同，也不把静态资源合同变成地图生命周期、字节复制实现、DMA 或呈现合同。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-palette-static-v1
SHA256 4F977B4B3EB8E731D2ABB6664F36030487DC186D267E66E9C2DAF3CB211007AB
Palettes 16 / Maps 79 / UsedPalettes 16 / PASS
```

fixture 直接绑定恰好一条 research-index 记录：

- `auxiliary.data.pt-mappalettes`——唯一、当前未关联，且是本合同唯一未来关联。

该记录还携带宽 `sf2-auxiliary-data-static-v1` 清单所有者。本合同不消费该聚合 fixture、不导入其任何兄弟记录，也不把其更宽清单当作该调色板语料的权威。只有 `sf2-map-palette-static-v1` 注册为本文档的可执行证据。

没有 `map.data.*`、地图头、`LoadMap`、`CopyBytes`、图形服务、淡入、中断、DMA 或呈现记录获得本合同。那些身份保持溯源、私有引用输入或独立所有者边界，而非额外语义关联。

受追踪 fixture 包含地址、聚合计数、完整使用计数、parity 计数、有效首字规则与一个运行时问题。完整 source/有效调色板字、载荷字节、逐调色板 hash、逐地图调色板赋值与渲染捕获保持私有/生成。

## 身份与指针拓扑

**已确认静态：** `p_pt_MapPalettes` 是 ROM `0x64004`（`409604`）的顶层表指针。它解析到 ROM `0x9494A`（`608586`）的 `pt_MapPalettes`。表包含十六个有序 longword 条目，按索引顺序解析到 `MapPalette00` 到 `MapPalette15`。

每个条目指一个含十六个大端 16 位字的独立存储 32 字节源载荷。因此完整源语料包含：

| 面 | 已接受计数 |
| --- | ---: |
| 有序调色板身份 | 16 |
| 每源调色板字节 | 32 |
| 每源调色板字 | 16 |
| 总源载荷字节 | 512 |
| 总源字 | 256 |

全部十六个指针表条目与全部十六个载荷范围匹配已接受 ROM。parity 计数器单独保留为 `16/16`；通过指针表不替代载荷 parity，外观相等颜色字不折叠资源身份。

私有导入器必须保留表顺序、每个调色板索引、源符号/路径/地址与 source/有效关系。它不得仅因某些字、行或完整载荷外观相等而去重调色板或重排它们。公开合同可以暴露受限表与地址元数据，但不得暴露原版载荷或逐调色板 hash。

## 源颜色字边界

**已确认静态：** 全部 256 个源字满足已接受 Genesis 颜色掩码 `0x0EEE`。语料包含 69 个不同观察源字值。

数字 69 只是值集计数。它不是：

- 从零到 68 的连续域；
- 硬件、引擎或修改内容可接受的所有值的闭合集；
- 颜色名、语义角色、视觉等价或别名关系；
- 相等数字字在不同地图上服务相同玩家面向用途的证明。

私有导入保留每个有序源字，即使其数字值重复。公开证据只保留 256 字总计、69 不同值计数、掩码结果、载荷维度、parity 计数与溯源。

掩码检查是静态格式证据。它不定义模拟输出、亮度、伽马、显示校准、模拟器渲染、透明度、背景行为或现代引擎色彩空间。

## 有序地图引用边界

**已确认静态：** 79 个地图头各包含一个已接受调色板索引。每个头字节匹配 ROM，每个引用在索引 `0..15` 内，每个调色板至少被引用一次。按调色板索引排序的精确公开使用直方图：

| 调色板索引 | 地图引用 |
| ---: | ---: |
| 0 | 47 |
| 1 | 2 |
| 2 | 3 |
| 3 | 6 |
| 4 | 6 |
| 5 | 2 |
| 6 | 2 |
| 7 | 1 |
| 8 | 1 |
| 9 | 1 |
| 10 | 2 |
| 11 | 1 |
| 12 | 2 |
| 13 | 1 |
| 14 | 1 |
| 15 | 1 |
| **总计** | **79** |

完整私有导入保留每个地图索引到其调色板索引的有序映射。公开合同只发布直方图与聚合 parity 事实，不发布逐地图映射。

地图头的静态引用不建立自然故事可达性、地图访问顺序、每张地图是否在普通游玩中渲染，或运行时代码是否后来替换或修改所选调色板。`usedPaletteCount=16` 与 `unusedPaletteCount=0` 只是完整静态头引用事实。

## 源到有效首字规则

**已确认静态源形状：** 已接受 `LoadMap` 条目在 ROM `0x2A8C`（`10892`），已接受 `CopyBytes` 身份在 `0x16D6`（`5846`）。在受限调色板加载源序列内，代码：

1. 通过 `p_pt_MapPalettes` 获得调色板表并选择索引源载荷；
2. 把目标身份设置为 `PALETTE_1_BASE`；
3. 把转移计数设置为符号 32 字节调色板大小；
4. 把 source、目标与计数交给 `CopyBytes`；
5. 交接通过普通源路径返回后清除 `PALETTE_1_BASE` 首字。

十五个源调色板有非零首字。一个已有零首字。应用已接受清除规则产生十六个首字为零的有效调色板。

即使清除让特定首字不变，源与有效调色板也是不同私有身份。私有导入器必须保留原版源形式并派生或验证有效形式。它不得覆盖唯一存储源表示然后声称无损往返。

该时间线不导入 `CopyBytes` 的微实现、性能、寄存器保留、错误行为或通用合同。它也不证明 CRAM 转移、DMA 完成、VInt 发布、渲染颜色、淡入完成、可见帧或呈现时序。字清除是源可见 RAM 变换，不是完整图形输出观察。

## 实现无关导入模型

最小完整逻辑导入把私有 source/有效载荷、私有地图引用与公开摘要分开：

```text
MapPaletteCorpus {
  privatePalettes[16]: PrivateMapPalette
  privateMapReferences[79]: PrivateMapPaletteReference
  publicSummary: MapPalettePublicSummary
}

PrivateMapPalette {
  paletteIndex
  sourceSymbol
  sourcePath
  sourceAddress
  privateSourceWords[16]
  privateEffectiveWords[16]
  privateSourceHash
  privateEffectiveHash
}

PrivateMapPaletteReference {
  mapIndex
  sourcePath
  mapAddress
  paletteIndex
}

MapPalettePublicSummary {
  fixtureId = "sf2-map-palette-static-v1"
  topLevelPointerAddress = 409604
  pointerTableAddress = 608586
  paletteCount = 16
  paletteByteCount = 512
  colorsPerPalette = 16
  sourceColorWordCount = 256
  distinctSourceWordValueCount = 69
  validColorMask = 0x0EEE
  validColorMaskCount = 256
  nonzeroSourceFirstWordCount = 15
  clearedEffectiveFirstWordCount = 16
  pointerTableParityCount = 16
  payloadParityCount = 16
  mapReferenceCount = 79
  mapHeaderParityCount = 79
  usedPaletteCount = 16
  unusedPaletteCount = 0
  usageCountsByPaletteIndex[16]
  fixtureProvenance
}
```

这是私有导入/溯源模型，不是必需渲染器 API、GPU 格式、颜色对象、资源包布局、缓存或场景生命周期。重制只有在仍能验证源顺序、引用、有效首字规则与刻意变换时，才可以把私有导入字转换为不同运行时格式。

公开投影不得包含原始 source/有效字、调色板载荷、逐调色板 hash 或完整逐地图赋值表。公开报告可以保留受限元数据、聚合计数、精确使用直方图、地址、parity 结果、颜色零规则与非内容诊断。

## 跨系统分离

本合同不拥有：

- 地图定义解析、地图选择、调色板槽运行时选择、构建顺序、工作布局状态、重载行为或持久性，仍归[map-exploration](../../contracts/map-exploration.md)及其证据所有者；
- blockset、64×64 布局、别名、碰撞或可通过性，仍归[map-layout-data](../../contracts/map-layout-data.md)与其他地图所有者；
- 调色板插值、过渡计时器、队列交接、显示初始化或 flash 状态，仍归[graphics-service-state](../../contracts/graphics-service-state.md)；
- 淡入等待/控制状态、VInt 调度、CRAM/VRAM DMA、中断节奏或硬件时序，仍归[interrupt-dma-and-trap-state](../../contracts/interrupt-dma-and-trap-state.md)；
- 瓦片集、地图精灵图形、特殊精灵、UI 调色板、战斗调色板、立绘或特殊画面资源；
- 渲染组合、颜色零视觉语义、动画、最终帧、截图或玩家面向呈现；
- 私有原版调色板字、载荷、hash 或逐地图赋值；
- 畸形、截断、越界、注入、修改或替换输入策略；
- 可访问性重映射、本地化、平衡、故事含义或战役可达性。

[map-design principles 综合](../synthesis/map-design-principles.md) 可以在保留相同边界的同时消费这些静态事实。它不得用非零使用计数作为正常故事可达性的证明，也不得用 source/ROM parity 作为渲染等价的证明。

## 判断边界

### 已确认

- 通过 `sf2-map-palette-static-v1` 与 `auxiliary.data.pt-mappalettes` 的 fixture/源溯源；
- 精确 `p_pt_MapPalettes`、`pt_MapPalettes`、`LoadMap` 与 `CopyBytes` 身份/地址；
- 十六个有序源调色板身份、各 32 字节与 16 字，共 512 字节与 256 字；
- 完整十六指针与十六载荷 source/ROM parity；
- 69 个不同观察源字值与全部 256 字在掩码 `0x0EEE` 内；
- 79 个有序私有地图引用、完整 79 头 parity、精确公开使用直方图、全部十六个索引已用与零未用调色板；
- 源码形状查找/复制/首字清除时间线、十五个非零源首字与十六个清除有效首字；
- 公开元数据/私有原版载荷分离。

### 推断

- 本合同不提升任何内容。

### 未知

- 全部十六个有效调色板是否以及如何经原版淡入、过渡与逐地图呈现路径渲染；
- 每个地图引用的正常故事可达性与运行时调色板选择/修改；
- 缓存、重载、存档、挂起与跨进程持久性行为；
- 调色板动画、CRAM/VInt/DMA 节奏、转移完成、中断时序与最终帧；
- 硬件颜色转换、颜色零透明度/背景含义、亮度、伽马与显示相关输出；
- 畸形或替换输入准入、诊断与回退行为；
- 现代色彩空间转换、可访问性重映射、替换资源、本地化与分发策略。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-map-palette-static-v1`、固定基线与已接受指针、表、`LoadMap` 与 `CopyBytes` 溯源身份；
2. 私有保留十六个有序源调色板身份与十六个有序有效身份，而不压平重复字或外观相等载荷；
3. 从私有已接受输入复现精确 32 字节/16 字逐调色板形状、512 字节/256 字总计与完整十六指针加十六载荷 parity；
4. 对照掩码 `0x0EEE` 验证每个源字并复现 69 个不同观察源字值，而不把该计数变成闭合、连续或语义颜色域；
5. 私有保留全部 79 个有序地图到调色板引用，同时公开只复现精确 16 条目使用直方图、79 头 parity 计数、全用结果与零未用结果；
6. 保留源码形状表查找、32 字节 `CopyBytes` 交接与后续首字清除，而不要求原版复制循环微实现或声称转移完成；
7. 保留十五个非零源首字与十六个零有效首字，同时保留无损私有源形式；
8. 通过合成或私有导入测试检测指针重排、调色板重编号、引用重分配、载荷截断、原始字丢失、掩码违规、缺失清除与 source/有效混淆；
9. 把原始字、载荷、逐调色板 hash、完整逐地图赋值、截图与其他原版内容保持公开 fixture 与报告之外；
10. 通过独立所有者报告地图生命周期、持久性、动画、淡入/呈现、CRAM/VInt/DMA、硬件输出、畸形输入、可访问性与替换策略，或作为 **未知**。

H4 可以在导入构建、惰性或运行时前解码或变换私有调色板数据。那些选择只在已接受身份、引用图、source/有效区分与公开不披露边界保持可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 指针与载荷语料 | **已确认静态** | `sf2-map-palette-static-v1`；[fixture](../../../../tests/fixtures/h2/map-palette-static-v1.json) | 十六个有序资源、精确维度与 parity；原始载荷/hash 保持私有 |
| 源颜色字边界 | **已确认静态** | 同一 fixture；[technical-graphics 研究](../../../research/technical-graphics.md) | 256 掩码有效字与 69 个不同值；无连续域、别名或视觉含义 |
| 地图引用面 | **已确认静态** | 同一 fixture；[map-content 研究](../../../research/map-content.md) | 79 个私有有序引用与精确公开直方图；无自然可达性或运行时生命周期声称 |
| 源到有效规则 | **已确认静态时间线** | 同一 fixture 与受限 `mapload.asm` 源 | 32 字节复制交接后首字清除；无 CopyBytes 微实现、DMA、转移或渲染完成 |
| auxiliary 聚合 | 排除执行所有者 | `sf2-auxiliary-data-static-v1` | 宽清单此处不提供注册或兄弟关联 |
| 地图构建与重载 | 独立所有者证据 | [map-exploration](../../contracts/map-exploration.md) | 调色板语料不拥有地图选择、构建、缓存、变更或持久性 |
| 淡入、中断、DMA 与渲染 | 独立所有者 / **未知** | [graphics service](../../contracts/graphics-service-state.md)；[interrupt 合同](../../contracts/interrupt-dma-and-trap-state.md) | 状态/控制身份不证明最终颜色、节奏、完成或可见呈现 |

## 开放问题

1. 未来分组呈现轨道能否经代表性淡入与过渡路径比较全部十六个私有有效调色板，而不发布原版颜色字或帧？
2. 哪些运行时路径在已接受初始源到有效变换后修改、缓存、重载或动画地图调色板？
3. 重制导入器应对越界地图引用、无效颜色位或刻意修改调色板载荷使用什么显式验证与替换策略？

## 复现

```powershell
uv run sf2 h2 map-palettes
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/map-palette-static.json` 下。公开验收使用受限元数据与溯源，而非原始原版调色板内容或逐调色板 hash。
