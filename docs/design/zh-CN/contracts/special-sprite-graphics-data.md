# 特殊精灵图形数据合同

- **已确认原版结构：** 有序十槽特殊精灵指针目录、五个初始载荷所有者身份、五个别名、六个源载荷定义、五个带调色板资源加一个仅动画流、精确聚合 source/压缩/解码核算、完整指针/载荷 parity，以及下文描述的受限私有导入面。
- **推断原版行为：** 此处不提升任何内容。源分类与资源身份不建立玩家可见用途、帧顺序、动画、调色板使用或呈现。
- **未知原版行为：** 自然或强制运行时准入、畸形/调试/原始 RAM 行为、缓存与资源生命周期、帧选择、调色板外观、VInt/DMA/CRAM 节奏、转移完成、渲染、替换策略、可访问性处理与玩家面向含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择运行时纹理格式、渲染器、动画模型、缓存、替换资源策略或分发许可。
- 证据日期：2026-08-14
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`special-sprite-graphics-data.md`](../../contracts/special-sprite-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义已接受原版特殊精灵图形语料的静态目录、parity、解码、别名与私有导入边界：

1. 解析到五个初始载荷所有者身份的十个有序源指针槽；
2. 完整五指针别名关系；
3. 六个有序源载荷定义：五个带调色板资源与一个仅动画流；
4. 精确聚合 source、调色板、压缩、解码、解码器诊断与 parity 元数据；
5. 与受限公开投影分离的私有 source/H1/ROM 往返验证。

此处消费的唯一可执行所有者是 fixture id `sf2-special-sprite-decode-v1`，位于 [`tests/fixtures/h2/special-sprite-decode-v1.json`](../../../../tests/fixtures/h2/special-sprite-decode-v1.json)，由 [`src/sf2tool/h2/special_sprites.py`](../../../../src/sf2tool/h2/special_sprites.py) 实现。其带来源所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

消费字段闭合。本数据合同消费：

- fixture 溯源与三个 `table` 身份；
- `summary` 中恰好这些数据语料字段：`pointerCount`、`uniquePointerPayloadCount`、`aliasPointerCount`、`resourceCount`、`paletteCount`、`animationOnlyStreamCount`、`battleSizedStreamCount`、`explorationSizedStreamCount`、`sourceByteCount`、`paletteByteCount`、`compressedByteCount`、`decodedByteCount`、`commandGroupCount`、`literalWordCount`、`copyCommandCount`、`copiedWordCount`、`minimumTrailingBits`、`maximumTrailingBits`、`maximumCopyOffsetWords`、`maximumCopyLengthWords`、`pointerTableRomParityCount` 与 `payloadRomParityCount`；
- 私有规范输出的有序 `aliases` 与 `resources` 用于导入验证。

它不把其余分发、路线、源引用或普通哨兵摘要字段，以及 `function`、`routing`、`regularSentinelReferences` 或 `runtimeQuestions` 作为数据保真消费。那些字段支持[graphics-service-state](../../contracts/graphics-service-state.md)中的服务/路由所有权或保留开放研究边界。宽 `sf2-auxiliary-data-static-v1`、聚合 `sf2-tech-graphics-static-v1` 与独立 `sf2-map-sprite-assignments-static-v1` fixture 不是本合同的执行所有者。

精确未来 research-index 关联只有：

- `auxiliary.data.pt-specialsprites`；
- `auxiliary.data.specialsprite-taros`。

同一专用 fixture 还绑定四条既有服务记录。它们保持不变，只与 `graphics-service-state` 关联：

- `tech.graphics.animate-special-sprite`；
- `tech.graphics.special-sprite-anims`；
- `tech.graphics.special-sprites`；
- `tech.graphics.stack-decompression`。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
Contract sf2-special-sprite-decode-v1
SHA256 E3DF0CEDBA48E8A5BB30D639868B9CB90C6C4FFA660D8ADA56DAB9969CEEFCA7
Pointers 10
Streams 6
DecodedBytes 16704
FullyRoutedIds 9
Status PASS
```

fixture 链接 research-index 分母恰好是六：上文两个当前未关联数据候选加四个已关联服务记录。没有聚合 auxiliary 记录、地图精灵赋值记录、实体记录、对话记录、渲染器记录或呈现记录获得本合同。

分母区分是规范性的：

- 十个指针意味着五个初始载荷所有者身份加五个别名；
- 六个资源意味着五个带调色板载荷加一个仅动画流；
- 资源定义与所有者身份不证明字节 hash 唯一性；
- 路线清单保持九个完整路由 ID、一个仅指针 ID 与六个无支撑 ID，但该分类属于服务合同而非该数据 H4 面。

## 有序指针与载荷目录

**已确认静态：** `pt_SpecialSprites` 在 ROM 地址 `154620` 开始。其十个有序 longword 槽解析到五个初始载荷所有者身份。五个槽是到更早所有者的别名。私有导入器必须保留完整有序槽到所有者关系，而非把它压平为无序五资源集或合成重复资源擦除别名。

第一个源载荷在 ROM 地址 `155126` 开始，已接受连续载荷语料在 ROM 地址 `161868` 结束。这三个地址是溯源与往返边界。重制运行时可以在私有验证后使用引擎原生标识符与引用；它不需要复现 Mega Drive 地址、大端指针存储或原版内存表布局。

六个有序源定义由五个带调色板载荷与一个不同仅动画流组成。五个解码流有 fixture 的战斗大小分类，一个有探索大小分类。这些是受限 source/语料类别，不是自然调用方、可见战斗/探索行为、动画状态或呈现的声称。

完整指针图、资源符号、源路径、个别地址与大小、私有 hash、调色板字、压缩字节与解码美术保持私有导入材料。

## 语料核算与解码器诊断

**已确认静态：** 私有源语料有该精确聚合核算：

| 面 | 已接受值 |
| --- | ---: |
| 有序指针槽 | 10 |
| 初始指针载荷所有者身份 | 5 |
| 指针别名 | 5 |
| 源载荷定义 | 6 |
| 带调色板资源 | 5 |
| 仅动画流 | 1 |
| 战斗大小解码流 | 5 |
| 探索大小解码流 | 1 |
| 源字节 | 6,742 |
| 调色板字节 | 160 |
| 压缩字节 | 6,582 |
| 解码字节 | 16,704 |
| 指针表 parity | 10 |
| 载荷 parity | 6 |

字节分母精确：`160 + 6582 = 6742`。调色板字节包含在源字节内，不得再加一次。

已接受规范输出还记录聚合解码器诊断：

- 262 个命令组；
- 3,491 个字面字；
- 653 个复制命令与 4,861 个复制字；
- 尾位范围 36 到 44；
- 最大复制偏移 960 字；
- 最大复制长度 33 字。

这些值验证已接受私有语料。它们不要求重制复现原版 Stack 位读取器、历史表示、命令解析器、复制循环、寄存器分配或指令顺序。36 到 44 尾位是逻辑终止符后的存储跨度尾；它们不是填充、零、稳定性、不可见性或呈现数据的证明。

## 实现无关导入模型

最小逻辑模型把私有目录保真与公开元数据分离：

```text
SpecialSpriteGraphicsCorpus {
  privatePointerSlots[10]: PrivateSpecialSpritePointerSlot
  privateResources[6]: PrivateSpecialSpriteResource
  publicSummary: SpecialSpriteGraphicsPublicSummary
}

PrivateSpecialSpritePointerSlot {
  orderedSlotIndex
  payloadOwnerRef
  aliasOwnerSlot
}

PrivateSpecialSpriteResource {
  orderedResourceIndex
  sourceIdentity
  sourcePath
  sourceAddress
  sourceByteCount
  paletteByteCount
  privatePaletteBytes
  privateCompressedBytes
  privateDecodedBytes
  privateSourceHash
  privatePaletteHash
  privateDecodedHash
  aggregateDecoderContribution
}

SpecialSpriteGraphicsPublicSummary {
  fixtureId = "sf2-special-sprite-decode-v1"
  pointerTableAddress = 154620
  firstPayloadAddress = 155126
  corpusEndAddress = 161868
  pointerSlotCount = 10
  initialPayloadOwnerCount = 5
  aliasPointerCount = 5
  resourceCount = 6
  paletteCount = 5
  animationOnlyStreamCount = 1
  battleSizedStreamCount = 5
  explorationSizedStreamCount = 1
  sourceByteCount = 6742
  paletteByteCount = 160
  compressedByteCount = 6582
  decodedByteCount = 16704
  pointerTableParityCount = 10
  payloadParityCount = 6
  aggregateDecoderDiagnostics
  fixtureProvenance
}
```

该模型是私有导入/溯源边界，不是渲染器 API、纹理图集、动画控制器、DMA 计划、地图实体组件或运行时资源包布局。合规导入器只在有序槽/别名图、资源身份、解码输出、parity 与变换溯源保持可验证时，可以把私有内容解码并变换为引擎原生形式。

公开投影不得包含原始调色板、压缩流、解码美术、逐资源 hash 或大小、完整资源路径/地址或完整指针/别名图。公开 fixture 与报告只能保留上文列出的受限聚合计数、范围、三个表/溯源地址、规范摘要、parity、解码器诊断与溯源。

## 跨系统分离

本合同不拥有：

- 特殊精灵加载/更新函数、精确 `9 + 1 + 6` 路线分类、战斗对探索分发、palette-4 时间线、立即对排队转移接缝或 `table_2784C` 服务身份，那些仍归[graphics-service-state](../../contracts/graphics-service-state.md)；
- Stack 解压服务 ABI 或编解码器微实现，也由 `graphics-service-state` 拥有；
- 构建地图精灵赋值域，仍归[map-sprite-assignment-surface](../../contracts/map-sprite-assignment-surface.md)及其已接受 Common Scripting 所有者；该结果此处不作为可执行证据消费；
- 普通地图精灵图形、特殊画面图形、战斗/UI/立绘语料、实体定义、地图人口、对话属性或调用方准入；
- 动画序列、帧选择、调色板应用、缓存生命周期、持久性、VInt/DMA/CRAM 调度、转移完成、硬件时序、渲染或可见呈现；
- `sf2-auxiliary-data-static-v1`、`sf2-tech-graphics-static-v1`、`sf2-map-sprite-assignments-static-v1` 或任何兄弟 research-index 关联；
- 畸形、注入、损坏、调试、原始 RAM 或替换输入策略；
- 可访问性、本地化、故事含义、平衡、替换资源或分发策略。

独立构建赋值所有者只确认其完整已接受原版输入域不写 ID `237..250`。那是独立所有者 **已确认静态** 边界，不是通用运行时不可达性，也不是本数据合同 H4 要求的一部分。

## 判断边界

### 已确认

- 通过 `sf2-special-sprite-decode-v1` 与两个精确 auxiliary data 关联的 fixture/源溯源；
- 三个已接受表/语料溯源地址；
- 十个有序私有指针槽、五个初始载荷所有者身份与完整五指针别名关系；
- 由五个带调色板资源与一个仅动画流组成的六个私有源定义；
- 精确 `6742 = 160 + 6582` 源核算与 16,704 解码字节；
- 完整 10 指针与 6 载荷 parity；
- 聚合解码器诊断作为语料验证元数据，而非必需编解码器逻辑；
- 公开聚合元数据与私有原版内容分离。

### 推断

- 本合同不提升任何内容。

### 未知

- 自然或强制运行时准入、加载频率、缓存生命周期、重载行为与持久性；
- 源身份、大小类、调色板、解码形式或仅动画流的视觉含义；
- 动画/帧选择、DMA/VInt/CRAM 节奏、转移完成、硬件时序与最终渲染；
- 畸形、调试、损坏、原始 RAM 或修改内容行为；
- 替换输入准入、诊断、回退、可访问性与分发策略。

## H4 验收合同

重制面向 H4 导入器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-special-sprite-decode-v1`、固定基线与三个已接受表/语料溯源地址；
2. 私有保留全部十个有序指针槽、五个初始载荷所有者身份与完整五指针别名关系，而不压平或复制所有者；
3. 私有保留全部六个有序源定义，把五个带调色板资源与一个仅动画流区分开，而不声称字节 hash 唯一性；
4. 复现已接受私有解码输出与精确聚合 `6742 = 160 + 6582` source、16,704 解码、10 指针 parity 与 6 载荷 parity 结果；
5. 通过私有或合成测试检测指针重排、别名重分配、资源重排、调色板边界漂移、载荷截断、解码输出漂移与溯源丢失；
6. 只把尾位当作存储跨度尾、把解码器计数器当作语料诊断，而非要求原版 Stack 微实现；
7. 在私有往返验证后允许引擎原生引用与运行时格式，而不要求 Mega Drive 地址、大端指针存储或原版表布局；
8. 把原始调色板、压缩流、解码美术、逐资源 hash/大小、完整源路径/地址与完整别名图保持公开 fixture 与报告之外；
9. 从 `graphics-service-state` 消费路由与加载器行为，而非把 fixture 服务字段提升为第二个数据所有者；
10. 通过独立所有者报告准入、缓存/持久性、动画、调色板使用、VInt/DMA/CRAM、呈现、畸形输入、替换与可访问性策略，或作为 **未知**。

H4 可以在运行时前或惰性导入、解码与变换私有特殊精灵数据。那些选择只在已接受目录身份、别名拓扑、解码证据、parity、溯源与公开不披露边界保持独立可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 指针/资源目录与 parity | **已确认静态** | `sf2-special-sprite-decode-v1`；[fixture](../../../../tests/fixtures/h2/special-sprite-decode-v1.json) | 10 私有槽、5 初始所有者、5 别名、6 资源；完整图与资源保持私有 |
| 语料核算与解码器诊断 | **已确认静态** | 同一 fixture；[technical-graphics 研究](../../../research/technical-graphics.md) | `6742 = 160 + 6582`、解码 16,704、parity 10/6；无必需 Stack 微实现 |
| 加载/更新路线、分发、调色板与转移接缝 | 独立所有者 **已确认静态** | [graphics-service-state](../../contracts/graphics-service-state.md) | 精确 `9 + 1 + 6` 服务分类；此处无重复路线 H4 |
| 完整构建赋值排除 ID `237..250` | 独立所有者 **已确认静态** | [map-sprite-assignment-surface](../../contracts/map-sprite-assignment-surface.md)与[Common Scripting](../../../research/common-scripting.md) | 非消费证据；畸形/调试/原始 RAM 可达性保持未知 |
| 运行时准入、动画、DMA/VInt/CRAM 与呈现 | **未知** | 未来运行时/呈现证据 | 源身份与解码形式不证明可见行为 |
| 替换资源、可访问性与分发 | 刻意设计 | 未来产品/内容决定 | 需要独立溯源与验收 |

## 复现

```powershell
uv run sf2 h2 special-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

详细别名、资源记录、原版调色板/流、解码美术与私有 hash 保留在忽略的 `local/derived/special-sprite-decode.json` 下。公开受追踪证据只保留上文描述的受限元数据、摘要、聚合诊断、parity 与溯源。
