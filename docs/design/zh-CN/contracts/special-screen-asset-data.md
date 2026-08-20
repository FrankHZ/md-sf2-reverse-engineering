# 特殊画面资源数据合同

- **已确认原版结构：** 完整九资源 Stack 压缩特殊画面瓦片语料、十二资源非压缩调色板/布局语料、witch 菜单调色板与有序气泡表、source/H1/ROM 一致性边界，以及下文描述的受限转移元数据。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 五个固定转移尾的内容、初始化、稳定性与可见使用；调色板上传顺序；淡入；布局变更与滚动；像素填充时间线；VInt、DMA 与 CRAM 节奏；渲染组合；精确动画节奏；调用方可达性；输入、音频与玩家面向画面含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择渲染器、资源格式、分辨率、动画系统、替换美术、可访问性策略或分发许可。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`special-screen-asset-data.md`](../../contracts/special-screen-asset-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义三个专用特殊画面资源轨道的静态身份与私有导入边界：

1. 九个 Stack 压缩瓦片或字体资源及其解码长度/消费者转移边界；
2. 十二个非压缩调色板与布局，包括两个 ASM 扩展标题布局；
3. witch 选择调色板、有序四选项/三帧气泡表、指针身份、字变换与源静态计时器阶段表。

它还定义基于符号、地址、维度、计数、所选关系与 hash 而非版权图形载荷的公开 H4 面。

它不拥有 logo、标题、witch、挂起或结尾画面引擎；存档服务或新游戏生命周期；标题/logo 输入与作弊处理；窗口分配；调色板上传或转移服务语义；淡入；音频；像素填充执行；或最终呈现。相邻[graphics-service 合同](../../contracts/graphics-service-state.md) 只拥有其受限解压与显示服务状态，而[window-system 合同](../../contracts/window-system.md) 拥有共享窗口状态。两个合同都不证明渲染特殊画面帧。

可执行所有者是：

- `sf2-special-screen-graphics-decode-v1`，位于
  [`tests/fixtures/h2/special-screen-graphics-decode-v1.json`](../../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json)；
- `sf2-special-screen-presentation-static-v1`，位于
  [`tests/fixtures/h2/special-screen-presentation-static-v1.json`](../../../../tests/fixtures/h2/special-screen-presentation-static-v1.json)；
- `sf2-witch-menu-graphics-static-v1`，位于
  [`tests/fixtures/h2/witch-menu-graphics-static-v1.json`](../../../../tests/fixtures/h2/witch-menu-graphics-static-v1.json)。

研究所有者是[Special Screens](../../../research/special-screens.md)与[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)中的相关资源语料。聚合 `sf2-special-screens-static-v1` fixture 刻意不被消费。

## 合同前证据审计

三个专用所有者于证据日期从当前 `main` 复现：

```text
sf2-special-screen-graphics-decode-v1
SHA256 4FCD9DCA7ED4FA4D5D667B1E1A85FC0A6D1A786DA78D24699EC883062A55604C
Resources 9 / DecodedBytes 50176 / OversizedTransfers 5 / PASS

sf2-special-screen-presentation-static-v1
SHA256 56E0FBEC2B6F917AD8916B2ABC9226EE24A9732F25D408D7876A460651A57E84
Resources 12 / PaletteColors 240 / ParityBytes 8832 / PASS

sf2-witch-menu-graphics-static-v1
SHA256 12C8B6818CDE7A8F8A808893AB32711F9DC7DA7A9380DC3999C3991A97B2DE15
PaletteColors 16 / BubbleFrames 12 / ParityBytes 1000 / PASS
```

审计检查了三个 fixture、验证器、所属研究文章、生成私有输出与当前 research-index 绑定。三个 fixture 当前绑定十一条索引记录。八条未关联，构成本合同精确未来关联集：

- `screens.endkiss.resources`；
- `screens.jewelend.resources`；
- `screens.suspend.resources`；
- `screens.title.resources`；
- `screens.title.compressed-tiles`；
- `screens.witch.resources`；
- `screens.witch.menu`；
- `screens.witchend.resources`。

其他三条 fixture 链接记录已属于已接受合同，必须保持语义不变：`tech.graphics.stack-decompression`、`tech.interfaces.ptr-s06` 与 `tech.services.resource-graphics`。仅聚合画面记录（包括 `screens.title.font` 的 `LoadTitleScreenFont` 函数身份）保持本数据合同之外。

审计保留以下限制：

- fixture 字段 `transferPaddingByteCount = 27,648` 只被解读为五个固定转移长度与其解码输出之间的算术差。它不证明填充字节、零、初始化内存、稳定性或不可见性；
- witch 表包含 480 个有序字与 240 个不同源字值。位置间重复相等可以由私有逻辑模型保留，但计数不证明源级别名抽象；
- 源静态调色板、优先级、镜像、翻转、计时器与目标事实不证明精确硬件节奏或可见像素；
- 受追踪 fixture 保留元数据与 hash。原版压缩字节、解码瓦片、调色板字、布局字、菜单字、截图与渲染捕获保持私有/生成。

## Stack 压缩资源语料

**已确认静态：** 九个资源占用 23,296 压缩字节并解码到 50,176 字节。七个资源直接进入 Stack 解码器，而语音气泡与 Sega logo 瓦片使用已接受压缩 DMA 包装器。六个直接源指针与全部九个源范围有已接受 H1/ROM 一致性。

| 资源符号 | 解码字节 | 已接受消费者边界 | 固定转移字节 | 尾差 |
| --- | ---: | --- | ---: | ---: |
| `tiles_TitleScreen` | 8,192 | 直接 Stack、立即 DMA | 8,192 | 0 |
| `font_TitleScreen` | 4,096 | 直接 Stack、立即 DMA | 4,096 | 0 |
| `tiles_SuspendString` | 448 | 直接 Stack、排队 DMA | 2,048 | 1,600 |
| `tiles_EndingKissPicture` | 6,144 | 直接 Stack、像素填充消费者 | — | — |
| `tiles_EndingWitch` | 7,808 | 直接 Stack、立即 DMA | 16,384 | 8,576 |
| `tiles_EndingJewels` | 1,856 | 直接 Stack、立即 DMA | 16,384 | 14,528 |
| `tiles_Witch` | 13,568 | 直接 Stack、立即 DMA | 16,384 | 2,816 |
| `tiles_SpeechBalloon` | 1,920 | 压缩立即 DMA 包装器 | 2,048 | 128 |
| `tiles_SegaLogo` | 6,144 | 压缩立即 DMA 包装器 | 6,144 | 0 |

恰好八个资源有固定转移长度：三个等于解码输出，五个更大。那八个转移长度共 71,680 字节。五个正差共 27,648 字节。`tiles_EndingKissPicture` 有源静态像素填充消费者且无可比固定 DMA 长度，因此不得被强迫进入精确转移或超大转移类。

私有导入器必须保留每个资源符号、ROM 地址、压缩范围、解码字节计数、source/ROM 一致性结果、消费者边界身份与可选固定转移长度。对超大边界，它必须保留解码范围与不同尾范围；它不得把尾物化为零，也不得在新运行时证据前把解码器输出复制超过其已接受长度。

fixture 还闭合聚合 Stack 流结构：964 个命令组、12,185 个字面字、产生 12,903 个复制字的 3,161 个复制命令、34 到 52 尾位、最大复制偏移 2,008 字与最大复制长度 33 字。这些事实验证导入流。它们不使编解码器微实现、畸形流恢复或 Genesis 转移服务成为本合同的一部分。

## 非压缩调色板与布局语料

**已确认静态：** 十二个资源占用 8,832 字节并匹配 source、H1 与 ROM。七个调色板包含 480 字节中的 240 个大端颜色字；五个布局包含 8,352 字节中的 4,176 个大端字。

| 资源类 | 计数 | 字 | 字节 | 已确认静态边界 |
| --- | ---: | ---: | ---: | ---: |
| 调色板 | 7 | 240 | 480 | 地址、大小、hash、107 个不同值、25 个零值 |
| 布局 | 5 | 4,176 | 8,352 | 地址、大小、hash、精确 source/H1/ROM 一致性 |
| 完整非压缩语料 | 12 | 4,416 | 8,832 | 十个直接 incbin 加两个 ASM 扩展布局 |

五个布局身份是 `layout_TitleScreenA`、`layout_TitleScreenB`、`layout_Witch`、`layout_EndingWitch` 与 `layout_EndingJewels`。两个标题布局从 `vdpTile` 源组装，而非作为直接 incbin 处理。其 1,792 与 768 字节扩展共 2,560 字节，匹配两个上游二进制镜像。导入器必须保留该溯源区分；二进制镜像一致性不把 ASM 源转换成 incbin 拥有资源。

无损私有导入保留完整有序调色板与布局字，因为顺序与完整 16 位值是数据身份的一部分。公开合同只保留符号、地址、大小、hash、聚合计数器与溯源。静态字值不建立上传顺序、淡入行为、布局写入、滚动、裁剪、层放置或最终组合。

## Witch 选择与气泡表

**已确认静态：** witch 菜单呈现边界包含两个数据资源与两个 longword 指针：

- 一个含 15 个不同值与两个零条目的 32 字节 16 色选择调色板；
- 一个含四个选项组、每选项三帧与每帧四十个字的 960 字节动画表；
- 两个有序四字节指针，产生完整 1,000 字节 source/ROM 一致性边界。

十二帧是有序 5×8 字网格。跨所有帧位置，表包含 480 个有序字与 240 个不同源字值。私有导入必须保留每个位置与位置间相等/重复关系。它不得把语料缩减为 240 个值，也不得仅从重复相等发明源级别名对象。

`DrawWitchMenuBubble` 对每个写入字应用 `-$5D00`。在已接受表中，全部 480 个调整字选择带优先级的调色板 2；240 个带镜像标志、240 个带翻转标志，六十个不同瓦片索引跨 1,024 到 1,083。这些是源静态字变换事实，不是最终硬件像素的声称。

所选选项计时器重置为 20，并按该顺序把状态映射到帧索引：

| 计时器状态 | 帧索引 |
| --- | ---: |
| 1..4 | 0 |
| 5..9 | 1 |
| 10..14 | 2 |
| 15..20 | 1 |

未选选项使用帧零。四个选项源偏移是 `0`、`240`、`480` 与 `720`；其目标偏移是 `392`、`4`、`36` 与 `432`。这只闭合静态选择器与写入表。菜单重绘节奏、CRAM 上传时序、窗口运动、控制器输入、感知节奏与渲染输出保持 **未知**。

## 实现无关导入模型

以下为逻辑数据合同，不是引擎类处方：

```text
SpecialScreenAssetCorpus {
  compressedResources[9] {
    resourceId
    sourceSymbol
    romAddress
    compressedByteCount
    decodedByteCount
    sourceRomParity
    privateCompressedBytes[]
    privateDecodedBytes[]
    consumerBoundary
    optionalFixedTransferByteCount
    optionalTailExtent {
      start: decodedByteCount
      byteCount: fixedTransferByteCount - decodedByteCount
      contents: unknown
    }
  }

  presentationResources[12] {
    resourceId
    sourceSymbol
    kind: palette | layout
    provenance: direct-incbin | asm-expanded
    romAddress
    wordCount
    byteCount
    payloadHash
    privateOrderedWords[]
    optionalBinaryMirrorParity
  }

  witchMenuPresentation {
    palette {
      sourceSymbol
      romAddress
      colorCount: 16
      payloadHash
      privateOrderedWords[16]
    }
    bubbleTable {
      sourceSymbol
      romAddress
      optionCount: 4
      framesPerOption: 3
      frameShape: 5x8
      privateOrderedWords[4][3][5][8]
      privatePositionalEqualityRelation
      payloadHash
    }
    pointers[2] {
      pointerIdentity
      romAddress
      targetResourceId
    }
    selectedTimerPhases[4]
    unselectedFrameIndex: 0
    writeWordAdjustment: -0x5D00
  }
}
```

公开形式省略每个 `private*` 字段与原版载荷，包括完整位置相等/重复映射。它保留身份、地址、维度与顺序计数、240 不同值计数、溯源、hash、指针与偏移元数据、字变换聚合、转移范围与阶段表事实，使验证用户提供的私有语料无需把版权图形变成仓库依赖。

## 跨系统分离

资源合同在显式数据与消费者边界身份处结束。把以下内容保持其所属系统或未来证据切片：

- Stack 解码器实现、无效流行为与全局显示初始化；
- 立即或排队 DMA/CRAM 服务执行、VInt 调度与硬件节奏；
- logo 校验和/输入/作弊流与标题 Start 轮询、滚动循环、淡入或返回结果；
- witch 存档菜单准入、存档/读档/复制/删除服务、新游戏生命周期、对话、输入与窗口运动；
- 挂起睡眠/重置流与结尾 witch、结尾宝石或结尾之吻渲染器执行；
- 调色板上传顺序、布局变更、滚动、像素填充顺序、层组合、音频与可见时序；
- 本地化、可访问性、替换美术、分辨率策略与许可分发。

聚合[Special Screens](../../../research/special-screens.md) 所有者可以描述那些相邻控制流身份。本合同不消费其聚合 fixture，也不把那些身份变成资源导入要求。

## 保真、现代化与版权边界

原版数据兼容要求在导入私有原版语料时确定性保留已接受资源符号、地址、大小、顺序、一致性元数据、私有载荷、解码长度、转移范围、标题布局溯源、witch 指针/表顺序、位置相等/重复关系、字变换与静态阶段表。

重制可以刻意选择新图像、调色板、布局、分辨率、动画时序、过渡、输入流、音频、响应式组合与可访问性行为。那些决定必须与原版数据一致性分开追踪。适配器可以把私有原版资源转码为现代格式，前提是它能复现已接受元数据与 hash 并报告刻意偏差。

原版压缩流、解码瓦片、字体、调色板、布局、气泡帧字、截图与渲染捕获是私有/生成版权输入。不要提交或再分发它们。公开构建需要新编写或适当许可替换资源。

## H4 验收面

重制侧私有导入器或兼容适配器只在自动化测试证明以下内容时声称本合同：

1. 全部九个压缩资源身份、源符号、地址、压缩范围、解码字节计数、消费者边界、聚合流计数器与 source/H1/ROM 一致性匹配已接受所有者；
2. 八个固定转移资源保留其精确长度与 `3 exact + 5 oversized` 分类，而结尾之吻资源保持像素填充消费者，无发明固定 DMA 长度；
3. 每个超大资源保留其解码范围与尾范围，共 27,648 尾字节，而不断言零、填充、初始化、稳定性或可见使用；
4. 全部十二个呈现资源保留身份、符号、H1/ROM 地址、类型、字/字节计数、source/ROM hash 与一致性与精确私有字顺序；
5. 两个标题布局保留 ASM 扩展溯源、精确 `1,792 + 768 = 2,560` 字节形状与二进制镜像一致性，而不被重新标记为直接 incbin；
6. witch 调色板、两个指针、四个选项组、十二个有序 5×8 帧、480 个有序字、240 个不同源字值、私有派生位置相等/重复关系、字调整、调整元数据、偏移与计时器阶段表匹配已接受所有者；
7. 公开 fixture 与报告只暴露元数据、hash 与合成示例，绝不暴露原版压缩字节、解码瓦片、调色板/布局字、气泡表字、完整位置相等/重复映射或渲染捕获；
8. 渲染、转移节奏、菜单/控制流、本地化、可访问性与刻意呈现变更与静态原版数据一致性分别测试与报告。

H4 不要求现代渲染器在运行时使用 Genesis 瓦片或颜色格式。它要求使用私有原版兼容输入时保留溯源的导入与显式偏差边界。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 九个 Stack 资源、流计数器、解码长度、消费者类与 source/H1/ROM 一致性 | **已确认静态** | `sf2-special-screen-graphics-decode-v1`（[`special-screen-graphics-decode-v1.json`](../../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json)） | 编解码器实现、畸形输入、转移执行与渲染像素 |
| 八个固定转移、三个精确与五个超大，加不同结尾之吻像素填充边界 | **已确认静态** | `sf2-special-screen-graphics-decode-v1`（[`special-screen-graphics-decode-v1.json`](../../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json)） | 尾内容、初始化、稳定性与可见使用保持 **未知** |
| 七个调色板、五个布局、8,832 一致性字节与标题 ASM/镜像关系 | **已确认静态** | `sf2-special-screen-presentation-static-v1`（[`special-screen-presentation-static-v1.json`](../../../../tests/fixtures/h2/special-screen-presentation-static-v1.json)） | 上传顺序、淡入、变更、滚动、组合与呈现 |
| witch 调色板、指针、有序 4x3x5x8 表、位置重复、变换与计时器阶段表 | **已确认静态** | `sf2-witch-menu-graphics-static-v1`（[`witch-menu-graphics-static-v1.json`](../../../../tests/fixtures/h2/witch-menu-graphics-static-v1.json)） | CRAM/DMA 节奏、重绘、窗口运动、感知时序与像素 |
| logo/标题/witch/挂起/结尾控制流与存档生命周期 | **独立所有者** | [Special Screens](../../../research/special-screens.md)与已接受兄弟合同 | 聚合 fixture 此处不消费 |
| 渲染器架构、可访问性、替换美术、本地化与可分发内容 | **刻意设计** | 未来产品/内容决定 | 需要溯源、许可与独立验收 |

## 复现

```powershell
uv run sf2 h2 special-screen-graphics
uv run sf2 h2 special-screen-presentation
uv run sf2 h2 witch-menu-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/special-screen-graphics-decode.json`、`local/derived/special-screen-presentation-static.json` 与 `local/derived/witch-menu-graphics-static.json` 下。
