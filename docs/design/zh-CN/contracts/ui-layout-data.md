# UI 布局数据合同

- **已确认原版结构：** 完整组装 vanilla UI 布局语料、有序法术等级指针路由、菱形边框变体、直接瓦片资源、source/H1/ROM parity，以及下文描述的不重叠覆盖地址核算。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 运行时分配与变更、文本或瓦片覆盖、调色板选择、VInt/DMA 行为、移动、裁剪、渲染组合与时序、调用方可达性、玩家面向菜单含义，以及排除 alternate 源的角色。
- 重制状态：实现无关 Phase 3 导入合同；尚未选择 UI 框架、渲染模型、本地化布局、可访问性策略、替换美术或分发许可。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`ui-layout-data.md`](../../contracts/ui-layout-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版 vanilla 构建组装的 UI 布局的静态身份与无损私有导入边界。它拥有：

1. 19 个组装源所有者及其 5,116 字节 source/H1/ROM parity 边界；
2. 27 个有序二维布局字网格、其维度、地址与元数据 hash；
3. 一等 64 字节法术等级指针表，包括其源身份、地址、溯源、hash 与到十个布局目标的有序 16 条目别名关系；
4. 四个 48 字节菱形边框变体与四个直接瓦片资源；
5. 不重叠覆盖 source/ROM 地址范围的精确 5,614 字节并集；
6. 基于身份、维度、地址、计数与 hash 而非版权布局或瓦片载荷的公开 H4 面。

它不拥有运行时窗口引擎、菜单准入、文本组合、调色板或 DMA 选择、动画、输入、音频、本地化、可访问性或最终呈现。相邻[window-system 合同](../../contracts/window-system.md) 拥有已接受八槽运行时分配、坐标、移动状态、组合与 VInt/DMA 调用顺序边界。两个合同单独都不证明玩家可见帧或调用方生命周期。

可执行所有者是 `sf2-ui-layout-static-v1`，位于 [`tests/fixtures/h2/ui-layout-static-v1.json`](../../../../tests/fixtures/h2/ui-layout-static-v1.json)，由 [`src/sf2tool/h2/ui_layouts.py`](../../../../src/sf2tool/h2/ui_layouts.py) 实现。研究所有者是[Common Menu Engines 与 Services](../../../research/common-menus.md)与[Technical Graphics](../../../research/technical-graphics.md)的完整静态 UI 布局段。

## 合同前证据审计

所有者于证据日期从当前 `main` 复现：

```text
Contract sf2-ui-layout-static-v1
SHA256 F3D2F46FAE27C281B7FFE9C2BD11BA536CBC8E0ADBCBE6FA66F1369A46CB7C34
Status PASS
Layouts 27
LayoutWords 2394
Assets 4
```

审计检查了 fixture、验证器、源所有者文章、source/H1/ROM parity 字段、全部 36 个 fixture 表绑定与当前研究索引。它发现那些绑定与 36 条当前未关联记录之间的精确双射：19 条 `auxiliary.data.static` 记录与 17 条 `ui.layout.static` 记录。注册刻意推迟到初步语义接受；本文档不改变索引。

36 个精确表/地址身份分解为 27 个布局、一个指针表、四个边框与四个直接资源。指针表不只是其他资源之间的边列表；它是独立寻址、带溯源的资源。

审计保留以下限制：

- `trackedUniqueByteCount = 5,614` 计数不重叠覆盖地址范围中的字节。它不声称 ROM 包含 5,614 个不同字节值。
- 四个直接资源共 570 字节，但 72 字节字母高亮已在 5,116 字节组装源语料内。只有三个相邻 incbin 资源（共 498 字节）扩展覆盖并集：`5,116 + 128 + 224 + 146 = 5,614`。
- 法术等级表是有序 16 条目路线关系。十个目标集或 `16/10` 计数对不充分，因为任一表示都会丢弃别名位置。
- 两个排除源文件在组装 vanilla parity 语料之外。其排除不是它们未使用、运行时不可达或在每个 alternate 构建中不可达的证据。
- 原版有序字网格与瓦片载荷保持私有输入。受追踪 fixture 保留元数据与 hash，而非再分发那些字节。

Issue #80 与 #81 涉及独立研究所有者，不是本合同的证据依赖。

## 语料与覆盖核算

**已确认静态：** 十九个源所有者组装到 5,116 字节。完整已接受语料有以下逻辑组件：

| 组件 | 已确认计数 | 已确认字节 |
| --- | ---: | ---: |
| 组装源所有者 | 19 | 5,116 |
| 布局网格 | 27 布局 / 2,394 字 | 4,788 |
| 法术等级指针表 | 1 表 / 16 有序条目 / 10 唯一目标 | 64 |
| 菱形边框变体 | 4 | 192 |
| 直接瓦片资源 | 4 | 570 |
| 不重叠覆盖并集 | — | 5,614 |

这些行不是可加分区。布局、指针字节、边框与字母高亮资源包含在 5,116 组装字节内。价格标签空白、价格标签数字与商店物品高亮载荷是组装区间之外的相邻 incbin 资源，恰好再加 498 个覆盖字节。重制导入器必须把覆盖计算为地址范围并集，而非求每个描述性小计之和。

全部 19 个组装源与全部四个资源载荷在已接受所有者中有 source/H1/ROM parity。这闭合其静态存储身份，而非运行时使用。

## 有序布局网格合同

**已确认静态：** 27 条布局记录包含 2,394 个大端 VDP 属性字（4,788 字节）。已接受形状从十个 3×2 法术等级指示器经菜单、立绘、状态、字母、计时器与 32×12 战斗演出背景网格。跨语料 fixture 记录 640 个唯一属性字与 580 个唯一瓦片索引。优先级、水平镜像、垂直翻转与调色板选择器位保留为静态字元数据。

私有原版数据导入器必须为每个布局保留：

- 其源符号与 ROM 地址；
- 宽度、高度与精确行主字顺序；
- 每个完整 16 位属性字，而非只有其瓦片索引；
- source/ROM hash 及其与所属源区间的关系。

字网格要求是无损私有导入模型。公开 fixture 与兼容报告必须使用维度、地址、hash、聚合属性计数器或合成样本；它们不得发布原版有序网格。静态调色板与变换位不建立运行时调色板选择、裁剪、移动、DMA 顺序或可见像素。

## 法术等级指针表资源与路由

**已确认静态：** 指针表占用 64 字节并包含 16 个有序 longword 条目。其源符号是 `pt_layouts_SpellLevelIndicator`，其 ROM 地址是 `0x110A4`（69,796），所有者证明该表 source/H1/ROM parity。规范私有导入独立于其引用的布局保留该身份、地址、大小、源与 ROM hash 与 H1 溯源。

条目解析到十个唯一布局目标。多个位置刻意别名同一目标，因此规范资源与关系是：

```text
pointerTable(pt_layouts_SpellLevelIndicator) {
  route[0..15] -> layout target identity
}
```

导入器必须把表保留为一等资源，并保留全部 16 个路线位置及其顺序与每个位置选择的目标。它不得把表归一化为十个目标集，也不得把它去重为十个匿名资源。现代运行时可以使用共享不可变布局对象，前提是指针表身份、每个原版路线及其别名身份仍可复现与测试。

原始 64 指针字节保持私有原版数据。公开 fixture 与报告保留表身份、源符号、地址、大小、溯源、hash 与有序目标元数据，而不发布其原始载荷。

表本身不定义法术等级算术、调用方验证、菜单准入或任何路线索引的玩家可见含义。那些消费者语义保持独立或 **未知**。

## 边框与直接资源

**已确认静态：** 四个菱形边框变体各占用 48 字节，共 192 字节。四个直接瓦片资源有已接受大小：

| 资源身份 | 已确认字节 | 覆盖关系 |
| --- | ---: | --- |
| 价格标签空白 | 128 | 相邻 incbin；在 5,116 组装区间之外 |
| 价格标签数字 | 224 | 相邻 incbin；在 5,116 组装区间之外 |
| 商店物品高亮 | 146 | 相邻 incbin；在 5,116 组装区间之外 |
| 字母高亮 | 72 | 已在 5,116 组装区间内 |

所有身份、大小、地址与 hash 是导入事实。运行时是否复制、重着色、动画、掩蔽或显示任何资源在本合同之外。

## 排除源边界

已接受 vanilla 段布局恰好排除这两个上游源路径：

- `data/graphics/tech/windowborder/entries.asm`；
- `data/graphics/tech/windowlayouts/fighterministatuswindowlayout.asm`。

它们不获得借用 vanilla 地址或 parity 声称。如果私有源检出包含它们，导入器应把它们保留为显式语料外溯源记录。未来证据可能建立 alternate 构建或运行时角色；在那之前，刻意使用、构建选择与可达性为 **未知**。

## 实现无关导入模型

以下为逻辑合同，不是引擎类处方：

```text
UILayoutCorpus {
  assembledSources[19] {
    sourcePath
    sourceAddressRange
    byteCount
    sourceHash
    romHash
  }

  layouts[27] {
    layoutId
    sourceSymbol
    romAddress
    width
    height
    orderedAttributeWords[]    // private import only
    layoutHash
    attributeMetadata
  }

  spellLevelPointerTable {
    pointerTableId
    sourceSymbol: pt_layouts_SpellLevelIndicator
    romAddress
    byteCount: 64
    sourceHash
    h1Provenance
    romHash
    rawPointerBytes[]          // private import only
    entries[16] {
      routeIndex
      targetLayoutId           // aliases remain explicit
    }
  }

  diamondBorders[4] {
    variantId
    romAddress
    byteCount
    payloadHash
  }

  directAssets[4] {
    assetId
    romAddress
    byteCount
    payloadHash
    coverageRelation
  }

  excludedSources[2] {
    sourcePath
    reason: outside-assembled-vanilla-corpus
  }

  coverage {
    assembledSourceBytes: 5116
    adjacentAssetBytes: 498
    nonOverlappingCoveredBytes: 5614
  }
}
```

公开形式省略 `orderedAttributeWords`、`rawPointerBytes` 与原版资源载荷。它保留相同身份、顺序、维度、地址、大小、关系、溯源与 hash，使验证用户提供的私有导入无需把版权数据变成仓库依赖。

## 跨系统分离

该静态语料与运行时窗口系统只在显式交接处相遇：调用方或窗口操作可以选择布局身份，而运行时系统拥有分配、变更、组合、移动与转移行为。本合同不推断每个静态布局被每个调用方准入或经一个公共路线渲染。

把以下内容保持本合同之外：

- 窗口槽分配、容量、删除与缓冲所有权；
- 运行时文本、数字、图标、立绘或高亮写入；
- 调色板选择、优先级解读、裁剪、Plane-A 组合、VInt、DMA 与时序；
- 控制器输入、菜单状态、调用方返回行为、音频与战役可达性；
- 本地化重排、可缩放 UI、可访问性、响应式布局与替换美术；
- 任何原版布局、瓦片、截图或捕获帧的许可与分发。

## 保真、现代化与版权边界

原版数据兼容要求在导入私有原版语料时保留源符号、地址、维度、有序网格字、一等指针表身份/溯源/hash 及其有序路线与别名、边框与资源身份、精确覆盖关系与已接受 hash。

重制可以刻意选择不同分辨率、坐标系统、widget 工具包、字体、调色板、动画、输入方法、响应式布局、可访问性行为与新编写美术。那些选择必须与原版数据保真分别记录，并在原版 ID 仍相关处带显式适配器或偏差报告。

原版布局字、边框字节、瓦片字节、截图与渲染捕获是私有/生成版权输入。不要提交或再分发它们。公开构建需要新编写或适当许可 UI 资源。

## H4 验收面

重制侧导入器或兼容适配器只在自动化测试证明以下内容时声称本合同：

1. 全部 19 个组装源所有者对私有原版输入复现已接受地址、字节计数与 source/H1/ROM hash；
2. 全部 27 个布局身份保留宽度、高度、2,394 字总计、精确行主字顺序、完整属性字与逐布局 hash；
3. 法术等级指针表保留其独立身份、源符号、`0x110A4` 地址、64 字节大小、source/H1/ROM 溯源与 hash，加全部 16 个精确顺序路线位置及其到十个目标的别名关系；
4. 全部四个 48 字节边框变体与四个直接资源保留身份、大小、地址与 hash；
5. 覆盖被计算为不重叠地址范围：5,116 组装字节加恰好 498 相邻字节等于 5,614，不双计数 72 字节字母高亮；
6. 两个排除源保持显式在组装 vanilla parity 语料之外，不发明未使用或不可达状态；
7. 公开 fixture 与报告只暴露元数据、hash 与合成示例，绝不暴露原版布局网格、原始指针表字节或瓦片载荷；
8. 运行时渲染、本地化、可访问性与刻意呈现变更与静态原版数据 parity 分别测试与报告。

H4 不要求重制使用 Genesis VDP 字或原版布局作为其编写或运行时表示。它要求使用私有原版兼容数据源时确定性、保留溯源的导入。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 19 个源所有者、5,116 组装字节、27 个网格、2,394 字、形状、属性与 parity | **已确认静态** | `sf2-ui-layout-static-v1`（[`ui-layout-static-v1.json`](../../../../tests/fixtures/h2/ui-layout-static-v1.json)） | 运行时选择、变更、渲染与调用方可达性 |
| 一等 64 字节法术等级指针表、有序 16 条目关系与十个目标身份 | **已确认静态** | `sf2-ui-layout-static-v1`（[`ui-layout-static-v1.json`](../../../../tests/fixtures/h2/ui-layout-static-v1.json)） | 原始字节保持私有；路线索引消费者语义与玩家面向含义保持未闭合 |
| 四个边框、四个资源与 5,614 字节不重叠覆盖并集 | **已确认静态** | `sf2-ui-layout-static-v1`（[`ui-layout-static-v1.json`](../../../../tests/fixtures/h2/ui-layout-static-v1.json)） | 运行时复制、调色板使用、动画与可见像素 |
| 排除窗口边框聚合与战斗机迷你状态 alternate | **已确认从 vanilla 组装 parity 语料排除** | `sf2-ui-layout-static-v1`（[`ui-layout-static-v1.json`](../../../../tests/fixtures/h2/ui-layout-static-v1.json)） | alternate 构建意图、使用与可达性保持 **未知** |
| 八槽分配、移动、组合与转移调用顺序 | **独立所有者** | [window-system 合同](../../contracts/window-system.md) | 端到端呈现保持未闭合 |
| 本地化、可访问性、替换美术与可分发内容 | **刻意设计** | 未来产品/内容决定 | 需要溯源、许可与独立验收 |

## 复现

```powershell
uv run sf2 h2 ui-layouts
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/ui-layout-static.json` 下。
