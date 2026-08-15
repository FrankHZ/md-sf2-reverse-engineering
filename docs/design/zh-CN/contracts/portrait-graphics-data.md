# 立绘图形数据合同

- 状态：**已确认静态资源身份、容器分区、别名与解码形状**
- 证据日期：2026-08-14
- 范围：原版立绘指针与载荷语料作为私有、引擎无关导入

> 本文件是 [`portrait-graphics-data.md`](../../contracts/portrait-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同拥有原版立绘图形语料的静态身份与私有导入形状。它不拥有立绘选择、加载器控制、窗口状态、解压服务行为、转移完成或呈现。

- **已确认**：一个解析到 52 个源载荷定义的 56 槽指针表；精确四个指针别名；计数四字节眼与嘴条目序列、每个定义一个 32 字节调色板与一个 Stack 流；精确聚合字节/解码/parity 计数；以及专用 fixture 复现的受限聚合解码器诊断。
- **推断，非规范性**：`Portrait`、`eye` 与 `mouth` 等名称只保留源码身份与有序字段。它们不建立可见面部含义、动画时序、表情语义、场景使用或作者意图。
- **未知 / 独立所有者**：每个槽的自然选择与可达性；无效索引；畸形数据行为；渲染瓦片与调色板含义；动画、组合、镜像、CRAM/VInt/DMA 节奏或完成、时序、缓存、运行时修改、持久性、本地化、可访问性、许可、替换策略与呈现 parity。

## 证据所有者与消费面

本合同消费的唯一可执行所有者是 `sf2-portrait-graphics-decode-v1`（[fixture](../../../../tests/fixtures/h2/portrait-graphics-decode-v1.json)、[verifier](../../../../src/sf2tool/h2/portraits.py)、[schema](../../../../schemas/h2-portrait-graphics-decode-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/portrait-graphics-decode.json)）。其文章所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

本数据合同消费：

- `table.portraitTableAddress`；
- `summary` 中的每个聚合字段；
- `aliases` 中的四条受追踪行；以及
- 已接受的上游、ROM 与规范输出溯源。

`function` 身份只作为外部消费者/服务见证保留。`LoadPortrait` 选择、解析、调色板复制、Stack 调用、VRAM/CRAM 交接与窗口状态接缝仍归[Portrait Window 与 State](../../contracts/portrait-window-state.md)。`LoadStackCompressedData` 仍归[Graphics Service State](../../contracts/graphics-service-state.md)。两个函数记录都不属于本合同。

完整生成 `payloads[52]` 行保留在忽略的 `local/derived/portrait-graphics-decode.json` 下。其源路径、地址、精确头条目、逐载荷大小、元数据/调色板 hash、压缩流细节与解码 hash 是私有验证输入，不是公开合同载荷。

聚合 `sf2-auxiliary-data-static-v1` 测试夹具被显式排除，即使目标研究记录也携带该证据。

## 直接绑定与关联边界

专用测试夹具直接绑定恰好三条 research-index 记录：

| 记录 ID | fixture 角色 | 合同处理 |
| --- | --- | --- |
| `auxiliary.data.pt-portraits` | ROM 地址 1,867,780 的 `pt_Portraits` | 唯一新关联候选 |
| `menus.load-portrait` | ROM 地址 87,594 的 `LoadPortrait` | 不变；只由 `portrait-window-state` 保留 |
| `tech.graphics.stack-decompression` | ROM 地址 7,752 的 `LoadStackCompressedData` | 不变；只由 `graphics-service-state` 保留 |

本合同不关联其他 `auxiliary.data.*`、`menus.*`、`tech.graphics.*`、精灵对话、UI 布局、map-script、battle-scene、中断、DMA、调色板、窗口、对话或呈现记录。

## 有序指针与载荷身份

完整表有 56 个解析到 52 个有序源载荷定义的有序指针槽。此处 52 意味着 fixture 接受的不同源定义与所有者身份。它不是所有载荷字节序列或解码 hash 互不相同的声称。

四个槽复用早期定义：

| 立绘槽 | 载荷所有者槽 |
| ---: | ---: |
| 35 | 33 |
| 53 | 52 |
| 54 | 52 |
| 55 | 52 |

完整身份行保持私有。指针表对全部 56 个槽有精确原版 ROM parity，52 个源载荷定义中的每一个都有精确原版 ROM parity。

这些是逻辑资源身份与别名，不是每个槽自然被选择、槽顺序是合适公开重制 API 或无效选择器有回退的证明。

## 载荷头与流分区

每个私有载荷定义有该有序源形状：

1. 一个大端字计数后接那么多四字节眼条目；
2. 一个大端字计数后接那么多四字节嘴条目；
3. 一个 32 字节源调色板；以及
4. 一个 Stack 压缩瓦片流。

跨全部 52 个定义，已接受条目计数为 261 个眼条目与 218 个嘴条目。那些条目中的每个字节大小坐标都在观察 `0..7` 范围内。这些名称与边界描述已接受源记录；它们不证明可见动画、帧顺序、面部含义或时序。

完整聚合字节核算：

```text
header bytes       = 3,788
compressed bytes   = 61,046
payload bytes      = 64,834
```

`summary.paletteByteCount=1664` 是已包含在 `summary.headerByteCount=3788` 内的 52 个调色板。它不得第二次加进头分母。最小与最大完整头大小为 36 与 100 字节。

## 解码形状与聚合诊断

52 个私有 Stack 流各精确解码到 2,048 字节，因此完整私有解码语料为 106,496 字节。

专用验证器还记录以下聚合诊断：

| 诊断 | 已接受值 |
| --- | ---: |
| 命令组 | 2,510 |
| 字面字 | 37,017 |
| 复制命令 | 2,665 |
| 复制字 | 16,231 |
| 最大复制偏移 | 950 字 |
| 最大复制长度 | 33 字 |
| 观察到的尾跨度 | 32..47 位 |

这些值对照维护的解码器验证本语料。它们不要求重制复现原版 Stack 微实现。尾跨度只是每个逻辑终止符后的存储跨度；它不是填充、零填充数据、稳定性或不可见性的证明。

## 实现无关逻辑模型

完整私有导入器可以使用等价模型：

```text
PortraitGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    pointerSlots[56] {
        logicalPortraitId
        payloadOwnerId
    }
    privatePayloadDefinitions[52] {
        logicalPayloadId
        privateSourceIdentity
        privateSourceAddress
        privateEyeEntries[]
        privateMouthEntries[]
        privatePaletteWords[16]
        privateCompressedBytes
        privateDecodedBytes[2048]
        privateHashesAndDecodeDiagnostics
    }
}
```

逐定义 source/H1/ROM 地址、大端计数存储、原始条目/调色板字节、压缩字节、解码美术、逐资源大小/hash、完整载荷源路径与其他非公开细节是私有导入与往返证据。公开投影中命名的受限表与外部见证符号/地址、聚合溯源与元数据保持公开。验证后，合规重制可以使用引擎原生资源引用、动画记录、调色板、纹理与存储。它不需要复现 Mega Drive 地址空间、大端计数字、Stack 编解码器、原版缓冲或原版文件/容器布局。

导入器必须保持指针槽身份、载荷所有者身份、条目序列身份、调色板身份与解码瓦片身份不同。别名指针槽不变成重复载荷所有者。

## 公开与私有投影

公开投影只能保留：

- fixture、上游、ROM 与规范输出溯源 hash；
- `pt_Portraits` 符号与表地址；
- 受限外部 `LoadPortrait` 与 `LoadStackCompressedData` 见证身份与地址；
- 聚合指针/载荷/别名、条目、字节、解码、parity 与解码器诊断计数；
- 四条受追踪别名元数据行；以及
- 受限计数条目/调色板/单流分区。

它不得发布原始指针、完整非别名赋值、载荷符号/源路径或地址、逐载荷条目/偏移/大小/hash、调色板字、压缩字节、解码美术、ROM 摘录、截图、模拟器捕获或渲染呈现。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 其私有导入保留 56 个有序逻辑槽与 52 个有序源载荷所有者；
2. 精确 `35→33` 与 `53/54/55→52` 别名关系在不复制所有者的情况下保留；
3. 全部 52 个私有载荷保留计数四字节眼与嘴记录身份、一个 32 字节调色板身份与一个有序 Stack 流身份；
4. 全部 52 个私有流确定性复现其已接受 2,048 字节解码身份；
5. 完整私有核算以 `3,788 + 61,046 = 64,834` 闭合，解码 106,496 字节，且不双计数 1,664 调色板字节；
6. 引擎原生资源可以替换原版指针、计数字存储、Stack 存储与地址布局，而不改变逻辑身份或别名；以及
7. 公开报告只暴露受限聚合/溯源面，而版权载荷与完整私有身份材料保持私有。

H4 不要求原版选择器规则、加载器微实现、窗口状态、暂存或 DMA 操作数、CRAM/VInt 行为、可见动画、渲染输出或时序。那些接缝由其所属窗口/服务/呈现合同测试。

## 跨系统分离

- [Portrait Window 与 State](../../contracts/portrait-window-state.md) 消费本合同的规范记录，并保留立绘选择、`LoadPortrait` 解析/复制/调用顺序、窗口状态、眼/嘴更新控制、名称窗口行为、转移边界及其 Unknown。它不再独立拥有或重新验证本静态目录。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留 `LoadStackCompressedData`、其 ABI 与服务边界。此处的聚合编解码器诊断不转移该所有权。
- [Sprite-Dialogue Property Data](../../contracts/sprite-dialogue-property-data.md) 保留实体地图精灵到立绘/SFX 查找身份与行为。
- [UI Layout Data](../../contracts/ui-layout-data.md) 保留普通/镜像立绘窗口布局载荷。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 保留 VInt、DMA 与 CRAM 服务/时序边界。
- 对话、菜单、battle-scene、map-script、文本、本地化、可访问性、许可、替换资源与渲染仍归其自身合同或保持 Unknown。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 56 个有序槽、52 个源载荷所有者、四个别名 | **已确认静态** | `sf2-portrait-graphics-decode-v1` | 自然选择、字节唯一性、可见身份 |
| 计数眼/嘴条目、调色板、每载荷一个流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 可见动画、时序、畸形输入 |
| 聚合字节、解码形状、诊断、parity | **已确认静态** | 同一 fixture/验证器 | Stack 微实现、尾位含义、渲染美术 |
| 选择器、加载器、窗口、更新与名称窗口时间线 | **独立所有者已确认静态见证** | `portrait-window-state` | 运行时准入、转移完成、可见帧 |
| Stack 服务行为 | **独立所有者已确认静态** | `graphics-service-state` | 硬件/运行时时序 |
| 源码标签视觉意图 | **推断，非规范性** | 仅源码词汇 | 面部含义、场景角色、作者意图 |
| 可达性、呈现、持久性、替换 | **未知 / 独立所有者** | 未来受限证据或产品决定 | 此处不是 H4 数据保真要求 |

## 复现

```powershell
uv run sf2 h2 portraits
uv run sf2 design-contracts test
uv run sf2 verify
```

完整载荷行保留在忽略的 `local/derived/portrait-graphics-decode.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
