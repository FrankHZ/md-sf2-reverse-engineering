# 战斗武器与地面图形数据合同

- 状态：**已确认静态资源身份、容器分区、别名与解码形状**
- 证据日期：2026-08-14
- 范围：原版战斗武器、武器调色板与地面图形语料作为私有、引擎无关导入

> 本文件是 [`battle-weapon-ground-graphics-data.md`](../../contracts/battle-weapon-ground-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同拥有原版武器精灵、武器调色板与战斗地面图形数据的静态身份与私有导入形状。它不拥有武器或地面选择、加载器控制、解压服务行为、转移完成或呈现。

- **已确认**：一个与 23 个源流定义成源序一对一的 23 槽武器指针表；42 个连续四字节武器调色板条目；一个解析到 27 个源头所有者（含三个精确别名）的 30 槽地面指针表；解析到十个源流定义的 27 个八字节地面头；精确聚合字节/解码/parity 计数；以及专用 fixture 复现的受限聚合解码器诊断。
- **推断，非规范性**：`Weapon`、`Ground`、`Palette`、`Tiles` 与 `view` 等名称只保留源码身份与受限消费者词汇。它们不建立可见装备、角度、地形、空间排列、组合、调色板含义或作者意图。
- **未知 / 独立所有者**：每个索引的自然选择与可达性；无效索引与畸形数据；可见角度或视图选择；地面/背景组合；渲染瓦片与调色板含义；放置、分层、CRAM/VInt/DMA 节奏或完成、时序、缓存、运行时修改、持久性、替换策略、许可与呈现 parity。

## 证据所有者与消费面

本合同消费的唯一可执行所有者是 `sf2-battle-weapon-ground-decode-v1`（[fixture](../../../../tests/fixtures/h2/battle-weapon-ground-decode-v1.json)、[verifier](../../../../src/sf2tool/h2/battle_weapon_ground.py)、[schema](../../../../schemas/h2-battle-weapon-ground-decode-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/battle-weapon-ground-decode.json)）。其数据文章所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)，受限加载器上下文在[Battle Scene Engine](../../../research/battle-scene-engine.md)。

本数据合同消费：

- `table.groundTableAddress`、`table.weaponSpriteTableAddress` 与 `table.weaponPaletteAddress`；
- `summary` 中的每个聚合字段与 `sideSummaries` 中的两个有序行；
- `groundAliases` 中的三条受追踪行；以及
- 已接受的上游、ROM 与规范输出溯源。

`function` 身份只作为外部消费者/服务见证保留。武器与地面选择、调色板消费、流查找、Stack 或压缩 DMA 交接与转移请求仍归[Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md)。`LoadStackCompressedData` 与面向硬件转移服务仍归[Graphics Service State](../../contracts/graphics-service-state.md)与[Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md)。那些函数记录都不属于本合同。

完整生成 `weaponSprites[23]`、`weaponPalettes[42]`、`groundHeaders[27]` 与 `groundTiles[10]` 行保留在忽略的 `local/derived/battle-weapon-ground-decode.json` 下。其完整图、源符号、路径、地址、精确相对字、逐资源大小、调色板与解码 hash、压缩流细节与原始或解码字节是私有验证输入，不是公开合同载荷。

聚合 `sf2-auxiliary-data-static-v1` 测试夹具被显式排除，即使全部三个目标研究记录都携带该证据。

## 直接绑定与关联边界

专用测试夹具直接绑定恰好七条 research-index 记录：

| 记录 ID | fixture 角色 | 合同处理 |
| --- | --- | --- |
| `auxiliary.data.pt-grounds` | ROM 地址 1,802,280 的 `pt_Grounds` | 新关联候选 |
| `auxiliary.data.pt-weaponsprites` | ROM 地址 1,809,050 的 `pt_Weaponsprites` | 新关联候选 |
| `auxiliary.data.weaponpalette00` | ROM 地址 1,830,456 的连续武器调色板根 | 新关联候选 |
| `battle.scene.load-weapon-palette` | 105,036 的武器调色板加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `battle.scene.load-weapon-sprite` | 105,052 的武器精灵加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `battle.scene.load-ground` | 105,092 的战斗地面加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `tech.graphics.stack-decompression` | 7,752 的 `LoadStackCompressedData` | 不变；只由 `graphics-service-state` 保留 |

本合同不关联其他 `auxiliary.data.*`、`battle.scene.*`、`tech.graphics.*`、物品、己方/敌人定义、行动者精灵、动画序列、背景、地形、效果、立绘、地图精灵、中断、DMA 或呈现记录。

## 武器精灵与调色板身份

武器精灵表有 23 个与 23 个源流定义和所有者身份成源序一对一的 23 个有序指针槽。计数 23 不声称每个压缩字节序列或解码 hash 互不相同。

每个私有流精确解码到 8,192 字节。23 个流占用 21,314 压缩字节并总共解码到 188,416 字节。解码记录包含四个 64 瓦片视图的源码格式陈述仍是[Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md)中的受限消费者/源见证；它不是本数据合同中的渲染布局、角度选择或可见性规则。

武器调色板根标识 42 个连续源条目。每个条目恰好四字节，因此该独立语料占用 168 字节。这些条目是源码具名调色板记录，不是完整 16 色调色板或颜色含义的证明。选择器与任何调色板条目之间的完整私有关联仍是呈现/消费者关注点。

## 地面指针、头与流身份

地面指针表有 30 个解析到 27 个源头所有者的有序槽。三个槽复用早期所有者：

| 地面槽 | 头所有者槽 |
| ---: | ---: |
| 21 | 12 |
| 22 | 12 |
| 29 | 13 |

每个私有源头占用八字节：

1. 一个六字节源调色板前缀；以及
2. 一个两字节自相对瓦片集字。

因此 27 个头占用 216 字节。其 162 调色板字节是该头计数的子集，不得再加一次。

完整私有头到瓦片集图解析到十个源流定义与所有者身份。计数十不建立互不相同字节或解码 hash。每个流精确解码到 1,536 字节；十个流占用 6,434 压缩字节并总共解码到 15,360 字节。

精确别名与头到流关系是逻辑导入身份。它们不证明全部 30 个槽自然被选择、别名槽有不同呈现含义，或源数字顺序是合适公开重制 API。

## 字段精确聚合核算

fixture 的聚合域保持不同：

```text
pointer slots             = 23 weapon + 30 ground                 = 53
graphic stream owners     = 23 weapon + 10 ground                 = 33
source-named palettes     = 42 four-byte entries + 27 six-byte prefixes = 69
palette bytes             = 168 weapon + 162 ground               = 330
```

162 地面调色板字节已包含在 216 地面头字节内。因此完整存储源对象核算闭合为：

```text
compressed stream bytes   = 27,748
standalone weapon palette =    168
complete ground headers   =    216
source payload bytes      = 28,132
```

把 `paletteByteCount=330` 加进该分母是错误的：那会把地面调色板前缀计数两次。

指针表 ROM parity 计数为 53。源对象 ROM parity 计数为 102，分化为：

```text
23 weapon streams + 42 weapon palettes + 27 ground headers + 10 ground streams = 102
```

## 解码形状与聚合诊断

23 个武器与十个地面流总共解码到 203,776 字节。专用验证器还记录以下聚合诊断：

| 诊断 | 已接受值 |
| --- | ---: |
| 命令组 | 753 |
| 字面字 | 7,308 |
| 复制命令 | 4,417 |
| 复制字 | 94,580 |
| 最大复制偏移 | 2,000 字 |
| 最大复制长度 | 33 字 |
| 观察到的尾跨度 | 32..47 位 |

这些值对照维护的解码器验证本语料。它们不要求重制复现原版 Stack 微实现。尾跨度只是每个逻辑终止符后的存储跨度；它不是填充、零填充数据、稳定性或不可见性的证明。

## 实现无关逻辑模型

完整私有导入器可以使用等价模型：

```text
BattleWeaponGroundGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    weaponPointerSlots[23] {
        logicalWeaponGraphicId
        weaponStreamOwnerId
    }
    privateWeaponStreams[23] {
        logicalStreamId
        privateSourceIdentity
        privateSourceAddress
        privateCompressedBytes
        privateDecodedBytes[8192]
        privateHashesAndDecodeDiagnostics
    }
    privateWeaponPaletteEntries[42] {
        logicalPaletteEntryId
        privateSourceIdentity
        privateSourceAddress
        privateWords[2]
        privateHash
    }
    groundPointerSlots[30] {
        logicalGroundId
        groundHeaderOwnerId
    }
    privateGroundHeaders[27] {
        logicalHeaderId
        privateSourceIdentity
        privateSourceAddress
        privatePaletteWords[3]
        privateRelativeTilesetWord
        groundStreamOwnerId
    }
    privateGroundStreams[10] {
        logicalStreamId
        privateSourceIdentity
        privateSourceAddress
        privateCompressedBytes
        privateDecodedBytes[1536]
        privateHashesAndDecodeDiagnostics
    }
}
```

完整 source/H1/ROM 地址、大端指针与相对字存储、原始头与调色板、压缩字节、解码美术、源路径、逐资源大小/hash 与其他非公开细节是私有导入与往返证据。公开投影中命名的受限根与外部见证符号/地址、聚合溯源与元数据保持公开。验证后，合规重制可以使用引擎原生资源引用、调色板、纹理、格式与存储。它不需要复现 Mega Drive 地址空间、大端存储、Stack 编解码器、原版缓冲或原版文件/容器布局。

导入器必须保持武器指针、武器流、武器调色板、地面指针、地面头、地面调色板与地面流身份不同。别名地面槽不变成重复头所有者，共享地面流不变成重复流所有者。

## 公开与私有投影

公开投影只能保留：

- fixture、上游、ROM 与规范输出溯源 hash；
- 三个受限表/根符号与地址；
- 三个加载器与一个 Stack 服务外部见证身份与地址；
- 聚合与侧特定指针、所有者、别名、调色板/头/流、字节、解码、parity 与解码器诊断计数；
- 三条受追踪地面别名行；以及
- 受限武器流、四字节调色板条目与地面头/流分区。

它不得发布原始指针、完整指针/头/流图、资源符号/源路径或地址、逐资源偏移/大小/hash、调色板字、压缩字节、解码美术、ROM 摘录、截图、模拟器捕获或渲染呈现。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 其私有导入保留 23 个有序武器指针槽与 23 个源流所有者；
2. 全部 23 个私有武器流复现其已接受 8,192 字节解码身份，与 42 个连续四字节武器调色板条目分开；
3. 其私有导入保留 30 个有序地面槽、27 个头所有者、精确 `21/22→12, 29→13` 别名关系与完整头到十流图；
4. 每个地面头保留其六字节调色板身份，与自相对瓦片集字身份分开，且全部十个流复现其已接受 1,536 字节解码身份；
5. 完整私有核算以 `27,748 + 168 + 216 = 28,132` 闭合，解码 203,776 字节，且不把 162 字节地面调色板子集加两次；
6. 引擎原生资源可以替换原版指针、相对字、Stack 存储与地址布局，而不改变逻辑所有者、别名、调色板、头或流关系；以及
7. 公开报告只暴露受限聚合/溯源面，而版权载荷与完整私有身份材料保持私有。

H4 不要求原版选择器、加载器微实现、调色板写入、暂存或 DMA 操作数、CRAM/VInt 行为、渲染输出或时序。那些接缝由其所属呈现/服务合同测试。

## 跨系统分离

- [Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md) 消费本合同的规范记录，并保留仅己方/无效武器选择、武器调色板最终两色消费、受限四视图消费者接缝、武器 Stack 交接、地面调色板写入与自相对查找、压缩 DMA 交接与 `0x300` 请求、场景时间线、呈现边界及其 Unknown。它不再独立拥有或重新验证本静态目录。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留 Stack 解压与压缩 DMA 服务边界。此处的聚合解码器诊断不转移该所有权。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 保留 VInt、DMA 与 CRAM 服务/时序边界。
- 物品与己方定义合同保留武器身份/装备字段；它们不拥有这些图形载荷或调色板条目。
- 行动者战斗精灵与动画、背景、地形、效果、立绘、特殊/地图/UI 图形、本地化、可访问性、许可、替换资源与渲染仍归其自身合同或保持 Unknown。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 23 个武器指针/流与 42 个调色板条目 | **已确认静态/私有导入** | `sf2-battle-weapon-ground-decode-v1` | 选择、字节唯一性、视觉含义 |
| 30 个地面槽、27 个头、三个别名、十个流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 自然可达性、共享流含义 |
| 聚合字节、解码形状、诊断、parity | **已确认静态** | 同一 fixture/验证器 | Stack 微实现、尾位含义、渲染美术 |
| 选择器、加载器、调色板、相对查找、服务/DMA 交接 | **独立所有者已确认静态见证** | `battle-scene-presentation` | 转移完成、角度、放置、组合 |
| Stack/压缩 DMA 服务 | **独立所有者已确认静态** | `graphics-service-state` | 硬件/运行时时序 |
| 源码标签视觉意图 | **推断，非规范性** | 仅源码词汇 | 装备/地形含义、作者意图 |
| 可达性、呈现、持久性、替换 | **未知 / 独立所有者** | 未来受限证据或产品决定 | 此处不是 H4 数据保真要求 |

## 复现

```powershell
uv run sf2 h2 battle-weapon-ground
uv run sf2 design-contracts test
uv run sf2 verify
```

完整私有行保留在忽略的 `local/derived/battle-weapon-ground-decode.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
