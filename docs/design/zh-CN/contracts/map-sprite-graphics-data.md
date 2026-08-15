# 地图精灵图形数据合同

- **已确认原版结构：** 有序 720 槽普通地图精灵指针表、670 个源载荷身份、50 个指针别名、带固定 576 字节解码形式的 669 个有效 Basic 压缩流、共享双字节哨兵身份及其九个指针槽、精确表/载荷 parity，以及下文描述的受限普通对特殊消费者接缝。
- **推断原版行为：** 此处不提升任何内容。源名识别地图精灵资源与消费者操作，但不证明玩家可见方向、动画或呈现。
- **未知原版行为：** ID `237..250` 的非标准注入、哨兵解码与失败结果、自然运行时可达性与加载频率、缓存生命周期、朝向或槽含义、沉浸效果、动画与调色板选择、VRAM 放置、VInt/DMA 节奏、转移完成、最终渲染、畸形或替换输入策略、可访问性处理与玩家面向含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择运行时纹理格式、渲染器、缓存、精灵动画模型、替换资源策略或分发许可。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-sprite-graphics-data.md`](../../contracts/map-sprite-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义普通原版地图精灵图形语料的静态存储、解码、别名、parity 与私有导入边界：

1. 排列为 240 个逻辑 ID、各三个有序源槽的 720 个有序指针槽；
2. 670 个独立具名源载荷身份与完整指针别名关系；
3. 带固定 576 字节解码形式的 669 个有效 Basic 压缩 source/解码对；
4. 由九个有序指针槽共享的一个不同 `0xFFFF` 哨兵载荷身份；
5. 不导入运行时呈现或转移含义的受限消费者选择溯源；
6. 与私有原版内容分离的公开聚合元数据/溯源。

此处消费的唯一可执行所有者是 fixture id `sf2-map-sprite-decode-v1`，位于 [`tests/fixtures/h2/map-sprite-decode-v1.json`](../../../../tests/fixtures/h2/map-sprite-decode-v1.json)。其带来源所有者是[Technical Graphics](../../../research/technical-graphics.md)。受限源根是 `data/graphics/mapsprites/entries.asm` 与 `code/common/scripting/entity/entityscriptengine_2.asm` 中的普通消费者接缝。

精确未来 research-index 关联只有 `auxiliary.data.pt-mapsprites`。同一 fixture 还绑定 `tech.graphics.decompression`，它只与[graphics-service-state](../../contracts/graphics-service-state.md)关联。本合同不把自己添加进该记录，也不把私有资源语料变成通用 Basic 解压服务合同。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-sprite-decode-v1
SHA256 48C09CA7F523DDEC289D0CD954BEA12E5A47824DE4A4BEC5697B8EEFF54FDB3E
Pointers 720 / DecodedPayloads 669 / DecodedBytes 385344 / PASS
```

fixture 直接绑定恰好两条 research-index 记录：

- `tech.graphics.decompression`——已与 `graphics-service-state` 关联；不变；
- `auxiliary.data.pt-mapsprites`——唯一、当前未关联，且是本合同唯一未来关联。

后一记录还携带宽 `sf2-auxiliary-data-static-v1` 清单所有者。本合同不消费该聚合 fixture、不导入任何兄弟记录，也不把聚合清单用作该图形语料的权威。

独立 `sf2-map-sprite-assignments-static-v1` 所有者确认完整已接受原版构建输入域不写 ID `237..250`。该结果通过[Common Scripting](../../../research/common-scripting.md)作为独立所有者静态边界链接；赋值 fixture 不是注册给本合同的可执行证据。其结果不证明通用运行时不可达性、哨兵安全或原始 RAM、畸形、损坏、调试或修改输入下的行为。

没有特殊精灵、地图实体、己方/敌人定义、对话属性、地图生命周期、服务、中断、DMA 或呈现记录获得本合同。所有既有关联保持不变。

受追踪 fixture 暴露聚合计数、地址、parity、受限解码器诊断与小型哨兵结构。原始压缩字节、解码美术、逐资源 hash 与大小、完整 720 槽别名图与渲染捕获保持私有/生成。

## 有序指针与载荷拓扑

**已确认静态：** `pt_Mapsprites` 在 ROM `0xC8000`（`819200`）开始。指针表包含 720 个有序 longword 槽。源构建把那些槽分组为 240 个逻辑数字 ID，每 ID 三个有序源槽。

术语 **source slot** 是刻意的。本合同把位置 `0`、`1` 与 `2` 保留为表身份。它不指定玩家可见左/右/上/下含义、动画阶段、朝向语义或渲染器坐标系。消费者源在选择槽前变换朝向状态字节，但可见解读与所有调用方状态保持独立或 **未知**。

720 个指针槽解析到 670 个独立定义源载荷符号。五十个槽是别名：其槽形预期符号与指针实际选择的载荷身份不同。私有导入器必须保留两个身份：

- 有序指针槽及其逻辑 ID/源槽坐标；
- 所选载荷所有者符号、路径与地址。

它不得把表归一化为 670 条目载荷集、合成重复载荷擦除别名，或因多槽共享一个所有者而推断语义等价。

全部 720 个指针表条目与全部 670 个源载荷范围匹配已接受 source、H1 与 ROM 边界。指针 parity `720` 与载荷 parity `670` 保持独立结果。

## Basic 压缩载荷语料

**已确认静态：** 670 个源身份中 669 个是有效 Basic 压缩流。它们共消费 225,542 压缩字节，每个流精确解码到 576 字节（`0x240`）。因此完整解码语料共 385,344 字节。

| 面 | 已接受值 |
| --- | ---: |
| 有序指针槽 | 720 |
| 源载荷身份 | 670 |
| 有效 Basic 流 | 669 |
| 指针别名 | 50 |
| 含哨兵的聚合源字节 | 225,544 |
| 聚合有效压缩字节 | 225,542 |
| 每有效流解码字节 | 576 |
| 聚合解码字节 | 385,344 |
| 指针表 parity | 720 |
| 载荷 parity | 670 |

专用 fixture 还记录聚合解码器诊断：

- 6,946 个命令字；
- 87,031 个字面字；
- 18,125 个复制命令与 105,641 个复制字；
- 1,500 个重复最后字命令；
- 最大复制偏移 273 字；
- 最大复制长度 33 字。

这些值验证已接受私有语料。它们不要求重制复现原版 Basic 位读取器、命令解析器、历史表示、重复实现、复制循环、寄存器分配或指令顺序。独立私有解码器在产生已接受有序解码输出并保留溯源而不发布原版内容时合规。

私有导入保留每个压缩流、解码结果、源路径/地址与私有 hash。公开投影只保留维度、聚合字节计数、诊断、parity 与溯源。两个投影都不从字节形状单独提升瓦片布局、调色板、透明度、动画或可见精灵含义。

## 哨兵身份与九个槽

**已确认静态：** 唯一非 Basic 载荷身份是 `Mapsprite237_0`。其完整存储结构是双字节字 `0xFFFF`。九个有序指针槽选择同一哨兵：

```text
711, 712, 713, 714, 715, 716, 717, 718, 719
```

按已接受三槽表形状，那些槽占据逻辑 ID `237`、`238` 与 `239` 的全部三个源位置。哨兵是一个带九条传入指针边的私有载荷身份，不是九个独立载荷，也不是有效 Basic 压缩流。

私有导入器必须保留：

- `Mapsprite237_0` 符号/路径/地址身份；
- 精确双字节哨兵值；
- 全部九个有序指针槽及其共享所有者关系；
- 669 个有效 Basic 流与该哨兵身份之间的区分。

公开合同可以保留该小型哨兵结构，因为它已是已接受受追踪 fixture 的一部分。它不得暴露任何普通精灵载荷、解码美术、逐资源 hash 或完整别名图。

哨兵不建立死内容或通用运行时不可达性。已接受构建输入所有者从其完整原版构建域排除 ID `237..250`，但原始 RAM 写入、畸形脚本、损坏状态、调试路径、编码值与修改内容保持该证明之外。把哨兵传给 Basic 解码器的效果保持 **未知**。

## 受限消费者选择接缝

**已确认静态源形状：** `LoadBasicCompressedData` 绑定在 ROM `0x1A84`（`6788`）。普通源消费者 `ChangeEntityMapsprite` 与 `DmaEntityMapsprite` 保留以下受限选择接缝：

1. 获得地图精灵字节与源朝向派生槽选择器；
2. 把低于符号特殊精灵截止 `240` 的值当作普通表候选；
3. 为普通 ID 计算三个有序指针槽之一；
4. 通过 `pt_Mapsprites` 解析所选 longword；
5. 把所选私有源交给 `LoadBasicCompressedData` 与加载空间目标；
6. 在 `DmaEntityMapsprite` 中，把 240 或以上的值路由到独立特殊精灵加载器；
7. 在普通 DMA 路径保留源可见 `0x120` 字转移操作数。

这是源码形状选择与交接溯源，不是完整运行时合同。它不声称每个数字普通候选有效、哨兵安全解码、所有源面向状态自然发生或 DMA 完成。它不拥有沉浸效果行为、实体状态、VRAM 槽计算、缓存容量、动画、VInt 调度、硬件时序或可见帧。

截止也不使 ID `240..255` 成为该普通语料的一部分。特殊精灵资源及其不对称指针/分发路由保持[graphics-service-state](../../contracts/graphics-service-state.md)与专用特殊精灵所有者。

## 实现无关导入模型

最小完整逻辑导入把私有槽、别名、载荷与解码输出和公开元数据投影分开：

```text
MapSpriteGraphicsCorpus {
  privatePointerSlots[720]: PrivateMapSpritePointerSlot
  privatePayloads[670]: PrivateMapSpritePayload
  publicSummary: MapSpriteGraphicsPublicSummary
}

PrivateMapSpritePointerSlot {
  pointerSlotIndex
  logicalMapSpriteId
  sourceSlotIndex  // ordered 0..2 identity; no player-visible direction claim
  payloadOwnerRef
}

PrivateMapSpritePayload =
  | PrivateBasicMapSpritePayload
  | PrivateMapSpriteSentinel

PrivateBasicMapSpritePayload {
  payloadOwnerId
  sourceSymbol
  sourcePath
  sourceAddress
  privateCompressedBytes
  privateDecodedBytes[576]
  privateCompressedHash
  privateDecodedHash
}

PrivateMapSpriteSentinel {
  payloadOwnerId
  sourceSymbol = "Mapsprite237_0"
  sourcePath
  sourceAddress
  privateBytes = 0xFFFF
  incomingPointerSlots[9]
}

MapSpriteGraphicsPublicSummary {
  fixtureId = "sf2-map-sprite-decode-v1"
  pointerTableAddress = 819200
  basicDecoderEntryAddress = 6788
  pointerSlotCount = 720
  logicalIdCount = 240
  sourceSlotsPerId = 3
  payloadIdentityCount = 670
  validBasicStreamCount = 669
  sentinelIdentityCount = 1
  aliasPointerCount = 50
  sentinelPointerCount = 9
  sourceByteCount = 225544
  compressedByteCount = 225542
  decodedBytesPerBasicStream = 576
  decodedByteCount = 385344
  pointerTableParityCount = 720
  payloadParityCount = 670
  aggregateDecoderDiagnostics
  sentinelSymbol = "Mapsprite237_0"
  sentinelBytes = 0xFFFF
  sentinelPointerSlots[9]
  fixtureProvenance
}
```

该模型是私有导入/溯源边界，不是必需渲染器 API、纹理图集、动画控制器、缓存、VRAM 分配器、实体组件或资源包布局。重制只有在已接受槽顺序、别名图、所有者身份、哨兵区分、解码输出与变换溯源保持可验证时，才可以把私有解码数据变换为另一运行时形式。

公开投影不得包含普通压缩载荷、解码精灵美术、逐资源 hash 或大小、完整 720 槽别名图、渲染捕获或其他原版内容。公开报告可以保留受限元数据、聚合计数、parity、地址、解码器诊断、小型哨兵结构与溯源。

## 跨系统分离

本合同不拥有：

- Basic 解压服务 ABI 或编解码器微实现，仍与[graphics-service-state](../../contracts/graphics-service-state.md)关联；
- ID `240..255` 的特殊精灵资源或路由，仍归特殊精灵所有者与 graphics-service 合同；
- 原版构建赋值域，仍归[Common Scripting](../../../research/common-scripting.md)中描述的地图精灵赋值所有者；
- 初始实体列表记录及其地图精灵值，仍归[map-entity-data](../../contracts/map-entity-data.md)；
- 己方/敌人定义地图精灵表、职业派生、NPC 尾可达性或战斗身份，仍归[ally-definition-data](../../contracts/ally-definition-data.md)与[enemy-definition-data](../../contracts/enemy-definition-data.md)；
- 地图精灵到立绘/语音 SFX 查找，仍归[sprite-dialogue-property-data](../../contracts/sprite-dialogue-property-data.md)；
- 实体生命周期、移动、朝向含义、沉浸效果、动画、地图持久性或缓存；
- VInt 调度、VRAM DMA、中断节奏、转移完成与硬件时序，仍归[interrupt-dma-and-trap-state](../../contracts/interrupt-dma-and-trap-state.md)；
- 调色板、瓦片解读、纹理布局、渲染帧、UI、对话、音频或最终呈现；
- `sf2-auxiliary-data-static-v1`、`sf2-map-sprite-assignments-static-v1`、`map.data.*` 或任何兄弟 research-index 关联；
- 私有原版载荷、解码美术、hash、个别大小与完整别名；
- 畸形、注入、损坏、调试或替换输入策略；
- 可访问性重映射、本地化、故事含义、平衡或分发策略。

## 判断边界

### 已确认

- 通过 `sf2-map-sprite-decode-v1` 与 `auxiliary.data.pt-mapsprites` 的 fixture/源溯源；
- 精确 `pt_Mapsprites` 与 `LoadBasicCompressedData` 溯源身份/地址；
- 排列为 240 个逻辑 ID、各三个有序源槽的 720 个有序指针槽；
- 670 个私有载荷身份与完整 50 指针别名关系；
- 669 个有效 Basic 流、225,542 压缩字节、固定 576 字节解码大小与 385,344 解码字节；
- 一个由槽 `711..719` 选择的私有 `Mapsprite237_0` `0xFFFF` 哨兵身份；
- 完整 720 指针与 670 载荷 parity；
- 聚合解码器诊断作为语料验证元数据，而非必需编解码器算法；
- 截止 240 以下的源码形状普通表选择与截止或以上的独立特殊精灵交接；
- 独立所有者静态排除完整已接受原版构建输入域中的 ID `237..250`；
- 公开聚合/哨兵元数据与私有原版内容分离。

### 推断

- 本合同不提升任何内容。

### 未知

- ID `237..250` 的动态、编码、畸形、损坏、调试、原始 RAM 或修改内容注入；
- 提供 `0xFFFF` 哨兵时 Basic 解码器行为与可见失败；
- 自然可达性、加载频率、缓存生命周期、重载行为与持久性；
- 源槽位置与朝向变换的玩家可见含义；
- 沉浸效果、动画选择、调色板、VRAM 放置、VInt/DMA 节奏、转移完成、硬件时序与渲染帧；
- 畸形或替换输入准入、诊断与回退行为；
- 现代运行时格式、渲染器、缓存、可访问性处理、替换资源与分发策略。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-map-sprite-decode-v1`、固定基线与已接受表/解码器溯源身份；
2. 私有保留全部 720 个有序指针槽为 240 个逻辑 ID 乘三个源槽，而不指定不受支持玩家可见方向语义；
3. 私有保留全部 670 个载荷所有者身份与完整 50 指针别名关系；
4. 从私有输入复现 669 个已接受 Basic 解码输出，各恰好 576 字节，聚合 225,542 压缩 / 385,344 解码字节总计；
5. 保留不同 `Mapsprite237_0` `0xFFFF` 哨兵与全部九个传入槽，而不尝试把它解码为普通 Basic 流；
6. 验证完整 720 指针与 670 载荷 parity，同时保持原版资源、hash、大小与别名私有；
7. 通过私有或合成测试检测指针重排、所有者重分配、别名压平、载荷截断、解码大小漂移、哨兵替换、哨兵解码与私有源丢失；
8. 允许独立解码器，而不要求原版 Basic 命令解析器、复制循环、历史表示、重复实现、寄存器使用或指令顺序；
9. 保留受限普通对特殊选择接缝，而不导入特殊精灵资源、赋值域、实体生命周期、DMA 完成或可见渲染；
10. 把普通压缩字节、解码美术、逐资源 hash/大小、完整别名图、截图与其他原版内容保持公开 fixture 与报告之外；
11. 通过独立所有者报告注入、哨兵失败、运行时可达性、缓存/持久性、动画、调色板、VRAM/DMA、呈现、畸形输入、替换与可访问性策略，或作为 **未知**。

H4 可以在导入、惰性或运行时前解码与变换私有地图精灵数据。那些选择只在已接受身份/槽/别名图、哨兵区分、解码输出证据与公开不披露边界保持独立可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 指针与载荷语料 | **已确认静态** | `sf2-map-sprite-decode-v1`；[fixture](../../../../tests/fixtures/h2/map-sprite-decode-v1.json) | 720 私有槽、670 所有者、50 别名、精确 parity；资源/hash 保持私有 |
| 有效 Basic 流 | **已确认静态** | 同一 fixture；[technical-graphics 研究](../../../research/technical-graphics.md) | 669 个固定大小解码形式与聚合诊断；无必需编解码器微实现 |
| 哨兵身份 | **已确认静态** | 同一 fixture | 一个 `0xFFFF` 所有者与槽 711..719；不是有效 Basic 流或运行时不可达性证明 |
| 构建赋值排除 | 独立所有者 **已确认静态** | [common-scripting 研究](../../../research/common-scripting.md) | 完整构建域省略 237..250；畸形/调试/原始 RAM 注入保持未知 |
| 解压服务 | 既有独立关联 | [graphics-service-state](../../contracts/graphics-service-state.md) | `tech.graphics.decompression` 保持不变且不获得新设计合同 |
| 特殊精灵路由 | 独立所有者证据 | [graphics-service-state](../../contracts/graphics-service-state.md) | ID 240..255 在该普通资源语料之外 |
| 实体生命周期、DMA 与渲染 | 独立所有者 / **未知** | [map exploration](../../contracts/map-exploration.md)；[interrupt 合同](../../contracts/interrupt-dma-and-trap-state.md) | 源交接不证明缓存、时序、完成或可见呈现 |
| auxiliary 聚合 | 排除执行所有者 | `sf2-auxiliary-data-static-v1` | 宽清单此处不提供注册或兄弟关联 |

## 开放问题

1. 未来受限运行时轨道能否在不发布资源的情况下注入 ID `237..239`，并确定原版 Basic 解码器/状态路径如何报告共享哨兵？
2. 哪些原版运行时路径在解码后缓存、重载、替换或变换普通地图精灵图形，源槽选择如何映射到可见动画？
3. 重制导入器应对越界索引、截断 Basic 输入、哨兵访问、别名变更或修改资源使用什么显式验证与替换策略？

## 复现

```powershell
uv run sf2 h2 map-sprites
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/map-sprite-decode.json` 下。公开验收使用受限元数据、聚合诊断与小型哨兵结构——而非原版载荷、解码精灵美术、逐资源 hash/大小或完整指针别名图。
