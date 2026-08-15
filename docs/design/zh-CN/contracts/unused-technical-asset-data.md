# 未使用技术资源数据合同

- **已确认原版结构：** 一个含四个有序 Stack 流的 5,694 字节源容器、四个不同 8,192 字节解码结果、两个有序 16 色调色板记录、一个调色板指针、精确 source/H1/ROM parity，以及下文描述的受限注释剥离符号引用清单。
- **推断原版行为：** 此处不提升任何内容。`Unused`、`Cloud` 与 `Base` 作为源身份与分类保留，不作为渲染内容、用途或可达性的证明。
- **未知原版行为：** 原始地址、计算指针或仅调试可达性；帧或动画顺序；调色板赋值；VDP 目标；缓存或生命周期；VInt、DMA 与 CRAM 时序；转移完成；渲染含义；畸形输入行为；替换策略；可访问性处理；以及玩家可见使用。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择公开资源载荷、渲染器、运行时纹理格式、动画模型、替换美术或分发策略。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`unused-technical-asset-data.md`](../../contracts/unused-technical-asset-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义两个源分类技术资源的静态、仅数据导入边界：

1. 单个 `tiles_UnusedCloud` 容器及其四个有序 Stack 流跨度；
2. `palette_UnusedBase` 中的两个有序调色板与独立 `p_palette_UnusedBase` 指针身份；
3. 受限 source/H1/ROM 溯源、聚合解码器结果与静态符号引用事实；
4. 私有无损导入形式与公开仅元数据投影。

此处消费的唯一可执行所有者是 fixture id `sf2-unused-technical-assets-static-v1`，位于 [`tests/fixtures/h2/unused-technical-assets-static-v1.json`](../../../../tests/fixtures/h2/unused-technical-assets-static-v1.json)。其带来源所有者是[Technical Graphics](../../../research/technical-graphics.md)与[Technical Services](../../../research/technical-services.md)。

更宽 `sf2-tech-services-static-v1` fixture 被刻意排除。其通用服务、压缩、显示与资源清单不是本合同的执行依赖。同样，Stack 解码器实现属于既有[graphics-service 合同](../../contracts/graphics-service-state.md)；本数据合同消费解码输出证据而不创建新解压服务关联。

精确未来 research-index 关联集是：

- `tech.services.resource-title`；
- `tech.services.resource-base`。

两条记录都唯一且当前未关联。没有其他 `tech.services.*`、标题画面、基础瓦片、特殊画面、UI、启动、渲染器、中断、DMA、CRAM 或呈现记录获得本合同。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-unused-technical-assets-static-v1
SHA256 E28FC7F30311411B9D1822CF810454674E02F4AB13A7292FE36AFAFFAC0F0F12
CloudStreams 4 / DecodedBytes 32768 / ParityBytes 5762 / PASS
```

fixture 直接绑定恰好上文两条未来关联记录。两者当前都没有设计合同，没有其他记录消费该 fixture。同一记录还携带排除的聚合 technical-services 证据；该额外清单所有者不加宽本合同语义或关联集。

parity 分母精确：

| 存储组件 | 字节 | 边界 |
| --- | ---: | --- |
| `tiles_UnusedCloud` 容器 | 5,694 | 一个含四个有序流的源载荷 |
| `palette_UnusedBase` | 64 | 两个有序 16 色调色板 |
| `p_palette_UnusedBase` | 4 | 一个 longword 指针表示 |
| **总 source/ROM parity** | **5,762** | 全部三个存储组件 |

原版压缩字节、解码美术、完整调色板字、指针字节、渲染捕获与其他私有/生成工件不是可分发合同内容。

## Cloud 容器与有序流边界

### 已确认静态

`tiles_UnusedCloud` 是 ROM 地址 `182176` 的一个 5,694 字节源容器。它不得被归一化为一个压缩流。对每个偶数容器偏移进行产生恰好 8,192 输出字节的 Stack 解码的穷举测试恰好产生四个起点：

| 流 | ROM 地址 | 起点 | 排他终点 | 存储字节 | 逻辑输入位 | 存储跨度尾位 | 解码字节 | 瓦片 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 182176 | 0 | 1328 | 1,328 | 10,587 | 37 | 8,192 | 256 |
| 1 | 183504 | 1328 | 2696 | 1,368 | 10,900 | 44 | 8,192 | 256 |
| 2 | 184872 | 2696 | 4460 | 1,764 | 14,078 | 34 | 8,192 | 256 |
| 3 | 186636 | 4460 | 5694 | 1,234 | 9,829 | 43 | 8,192 | 256 |

四个有序解码结果共 32,768 字节（1,024 个 32 字节瓦片），有四个不同解码 hash。不同 hash 只证明已接受解码字节序列不同；它们不建立动画帧、显示顺序或可见含义。

四个存储跨度在其逻辑 Stack 终止符后保留 `37 + 44 + 34 + 43 = 158` 位。那些是存储跨度尾位。本合同不称它们为填充，也不声称它们是零、重建稳定、被每个可能消费者忽略或视觉不可见。

### 实现自由

导入器可以使用独立实现解码器、预解码私有数据或把四个结果变换为现代私有资源形式。保真要求已接受容器身份、有序边界、四个解码字节结果与 parity 关系保持独立可验证。它不要求原版 Stack 命令解析器、位读取器、复制循环、历史表示、寄存器分配或指令顺序。

聚合命令计数器与其他解码器诊断可用于检测语料漂移。它们是验证元数据，不是编解码器微实现要求，也不是任何特定命令序列作者意图的证据。

## Base 调色板与指针边界

### 已确认静态

`palette_UnusedBase` 是 ROM 地址 `2028966` 的 64 字节载荷，表示为两个有序 16 色 Mega Drive 调色板记录。`p_palette_UnusedBase` 是 ROM 地址 `2023460` 的独立四字节指针身份，在已接受 source/ROM 镜像中指向该调色板载荷。

跨有序 32 调色板字：

- 恰好观察到 17 个不同源字值；
- 恰好四个位置包含零；
- 两个调色板只在颜色索引 1 与 5 不同；
- 索引 1 处，第一个值为 `3822`、第二个为 `1198`；
- 索引 5 处，第一个值为 `1184`、第二个为 `1728`。

计数 17 是观察不同值计数。它不是连续或闭合颜色域、别名关系、调色板角色分类法或替换美术约束。两个有序调色板身份与所有私有字位置即使在值相等处也必须保持可区分。

源名不证明哪个解码流、帧、表面或画面会使用任一调色板。指针身份与 parity 不证明运行时解引用。

## 静态符号引用清单

从固定 `disasm/code` ASM 语料移除注释后，已接受 token 计数：

| 符号 | 注释剥离出现 | 已接受解读 |
| --- | ---: | --- |
| `tiles_UnusedCloud` | 1 | 仅其定义 |
| `palette_UnusedBase` | 2 | 其定义与指针初始化器 |
| `p_palette_UnusedBase` | 1 | 仅其定义 |

因此 fixture 确认 cloud 容器与调色板指针的符号 ASM 消费者为零，而调色板载荷除其定义外只有其指针引用。这是已接受源树中那些精确符号 token 的完整静态清单。

它不是死代码或通用运行时不可达性的证据。原始地址、计算指针、无符号 token 的表、注入状态、仅调试路线、修改构建与其他非符号访问保持静态声称之外。零计数也不提供结果、时序、渲染或硬件行为。

## 实现无关导入模型

一个完整逻辑私有导入可以使用以下闭合结构。名称是说明性的；身份与关系是规范性的。

```text
UnusedTechnicalAssetCorpus {
  fixtureId
  sourceBaseline
  cloudContainer: CloudContainer
  basePaletteSet: BasePaletteSet
  symbolicInventory: SymbolicReferenceInventory
}

CloudContainer {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 5694
  privateStoredBytes
  privateStoredHash
  orderedStreams[4]: CloudStream
}

CloudStream {
  streamIndex
  romAddress
  startOffset
  endOffsetExclusive
  storedByteCount
  logicalInputBitCount
  storedSpanTailBitCount
  decodedByteCount = 8192
  tileCount = 256
  privateStoredSpan
  privateStoredHash
  privateDecodedBytes
  privateDecodedHash
}

BasePaletteSet {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 64
  orderedPalettes[2]: PrivatePalette16
  pointerIdentity: PalettePointer
  privatePayloadBytes
  privatePayloadHash
}

PrivatePalette16 {
  paletteIndex
  orderedWords[16]
}

PalettePointer {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 4
  targetRef
  privateStoredBytes
  privateStoredHash
}

SymbolicReferenceInventory {
  commentStrippedTokenCounts
  rawOrComputedAccessExcluded = true
}
```

模型保持容器与其流不同、流顺序与存储跨度完整、指针与其目标不同。它不要求运行时加载、动画、调色板赋值或渲染。相等调色板字只可在有序源身份与完整私有字数组保持可恢复与可验证时内部去重。

## 公开投影与版权边界

公开合同或报告只能保留已接受 fixture 已暴露的受限元数据与溯源，包括：

- fixture 身份、源基线、ROM 身份、符号、路径与地址；
- 聚合字节、流、瓦片、调色板、颜色、parity 与符号引用计数；
- 四个有序流边界及其逻辑/尾位计数；
- 已受追踪验证 hash；
- 两条已受追踪调色板差异行；
- 显式 已确认、推断、未知 与独立所有者 标签。

公开投影不得包含原版压缩字节、解码瓦片美术、完整调色板字、指针字节、截图、渲染捕获或新扩展载荷表示。私有导入工具只能在忽略本地存储中持有并 hash 那些形式。面向公开分发的测试应使用元数据、fixture 已接受 hash 或合成数据。

## 跨系统分离

- [Graphics Service State](../../contracts/graphics-service-state.md) 拥有受限 Stack 解压服务合同。它不拥有这两个载荷身份，本合同不创建解压记录关联。
- [Special-Screen Asset Data](../../contracts/special-screen-asset-data.md) 拥有其专用标题、女巫、挂起与结尾资源语料。含未使用 cloud 定义的源路径不使该载荷成为该已接受语料的一部分。
- [UI Graphics Asset Data](../../contracts/ui-graphics-asset-data.md)与[UI Layout Data](../../contracts/ui-layout-data.md) 拥有其专用 UI 资源与布局；它们此处不提供调色板或 cloud 消费者证据。
- [Startup Control Flow](../../contracts/startup-control-flow.md)与已接受基础瓦片所有者保留系统初始化与基础瓦片交接。`UnusedBase` 源身份不暗示启动使用。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 拥有受限队列/服务状态，而非转移完成、CRAM 节奏或这些资源的运行时路线。
- 渲染器、动画、呈现、可访问性、替换、许可与分发选择保持未来所有者。本静态合同不选择现代引擎策略。

## 判断边界

### 已确认

- 精确 `sf2-unused-technical-assets-static-v1` fixture 身份与固定溯源；
- 精确源符号、路径、六个已接受地址与 5,762 字节 parity 分母；
- 一个 5,694 字节容器，在已接受偶数偏移处恰好四个有序 8,192 字节 Stack 结果；
- 四个不同解码 hash、32,768 解码字节、1,024 瓦片与共 158 的精确存储跨度尾位计数；
- 两个有序 16 色调色板、一个独立指针身份、精确大小/parity、两条差异行、17 个不同观察值与四个零位置；
- 精确注释剥离 token 出现清单与零符号消费者结果；
- 私有原版内容与受限公开元数据投影分离。

### 推断

- 源标识符与注释把载荷分类为 `Unused`、`Cloud` 与 `Base`；那些标签为溯源保留，不提升为运行时或玩家面向含义。

### 未知

- 是否有任何原版原始地址、计算指针、调试、畸形或修改状态路径到达任一资源；
- 帧/动画排序、调色板到流赋值、VDP 目标、缓存生命周期与运行时修改；
- DMA、CRAM、VInt、转移、硬件、渲染与可见时序行为；
- 存储跨度尾位是填充、稳定、零、被任何 alternate 路线消费还是不可见；
- 玩家可见含义、可访问性、替换、本地化、许可与分发策略；
- 畸形载荷准入、诊断、回退与恢复。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-unused-technical-assets-static-v1`、其固定基线、两条资源记录、源符号/路径与六个已接受地址；
2. 私有保留一个 5,694 字节 cloud 容器与偏移 `0`、`1328`、`2696` 与 `4460` 的四个有序流跨度，而非把容器当作一个流；
3. 从私有输入复现四个已接受 8,192 字节解码结果与不同私有解码 hash，而不要求原版 Stack 微实现；
4. 保留每个流的存储字节计数、逻辑消费位计数与精确存储跨度尾计数 `37`、`44`、`34` 与 `43`，而不把 158 位总计分类为填充；
5. 私有保留两个有序 16 字调色板、所有源字位置、不同指针身份/目标关系与精确 source/ROM parity；
6. 验证两条已接受调色板差异行、17 个不同观察值与四个零位置，而不把它们提升为闭合颜色或别名域；
7. 把静态 token 清单保留为源审计，同时把死代码、可达性、渲染与时序声称保持验收结果之外；
8. 通过私有或合成测试检测容器截断、流重排、边界漂移、解码输出漂移、调色板重排、指针漂移、parity 不匹配与意外公开载荷披露；
9. 只发布受限元数据/溯源面，绝不发布原版压缩字节、解码美术、完整调色板字、指针字节或捕获；
10. 把动画、调色板赋值、运行时可达性、缓存/持久性、VDP/DMA/CRAM、呈现、畸形输入处理、替换、可访问性与许可保持独立所有者或 **未知**。

H4 可以在导入时间、构建时间或其他私有预处理阶段解码。那些选择只在有序身份、边界、私有解码结果、调色板/指针结构、parity 与公开不披露边界保持独立可验证时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| cloud 容器与流 | **已确认静态** | `sf2-unused-technical-assets-static-v1`；[fixture](../../../../tests/fixtures/h2/unused-technical-assets-static-v1.json) | 一个私有 5,694 字节容器、四个有序流/结果；无动画或可见含义 |
| Stack 解码 | 受限解码输出证据 | 同一 fixture；[technical-graphics 研究](../../../research/technical-graphics.md) | 精确结果/诊断；无编解码器微实现要求 |
| base 调色板与指针 | **已确认静态** | 同一 fixture | 两个私有有序调色板加不同指针；无运行时解引用或赋值声称 |
| 符号引用清单 | **已确认静态** | 同一 fixture；[technical-services 研究](../../../research/technical-services.md) | 精确注释剥离 token 计数；零符号消费者不是死代码 |
| 源标签 | **推断分类法 only** | 固定源身份/注释 | `Unused`/`Cloud`/`Base` 保留，无玩家面向或运行时含义 |
| 通用技术服务 | 排除执行所有者 | `sf2-tech-services-static-v1` | 无聚合 fixture 注册或兄弟关联 |
| 渲染器与硬件服务 | 独立所有者 / **未知** | 图形、中断、UI、启动与呈现合同 | 无 VDP/DMA/CRAM 节奏、完成、渲染或可达性声称 |
| 公开载荷 | 禁止 | 版权/私有输入边界 | 仅受限元数据；原版字节、美术、完整调色板、指针与捕获保持私有 |

## 开放问题

1. 未来受限运行时轨道能否在不发布原版载荷的情况下证明对任一资源的原始地址、计算指针或仅调试访问？
2. 如果路线存在，它使用什么有序流选择、调色板赋值、VDP 目标与可见组合？
3. 重制应采纳什么验证、回退、替换、可访问性与分发策略，而不把源分类法当作原版行为？

## 复现

```powershell
uv run sf2 h2 unused-tech-assets
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/unused-technical-assets-static.json` 下。公开验收使用受限元数据、溯源、聚合计数、已接受 hash 与两条受追踪调色板差异行——而非原版压缩字节、解码瓦片美术、完整调色板字、指针字节、捕获或其他再分发内容。
