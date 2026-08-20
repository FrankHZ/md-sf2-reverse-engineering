# 战斗精灵图形数据合同

- 状态：**已确认静态资源身份、容器分区与解码形状**
- 证据日期：2026-08-14
- 范围：原版己方/敌人战斗精灵指针与载荷语料作为私有、引擎无关导入

> 本文件是 [`battle-sprite-graphics-data.md`](../../contracts/battle-sprite-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同拥有原版己方与敌人战斗精灵图形语料的静态身份与私有导入形状。它不拥有行动者选择、动画序列选择、加载器控制、解压服务行为、转移完成或呈现。

- **已确认**：两个有序指针表，含 32 个己方与 54 个敌人槽，各按源顺序解析到相同数量的源载荷定义；这些定义内的固定字段、相对调色板边界、相对帧字、调色板记录与 Stack 流；精确聚合字节/解码/一致性计数；以及专用 fixture 复现的受限聚合解码器诊断。
- **推断，非规范性**：`AllyBattlesprite`、`EnemyBattlesprite`、`frame`、`palette` 与 `status offset` 等名称只保留源码身份与有序字段。它们不建立可见行动者身份、姿态、动画时序、组合、放置或作者意图。
- **未知 / 独立所有者**：每个槽与帧的自然选择与可达性；无效索引与畸形数据；渲染瓦片、调色板与状态图标含义；帧顺序与时序；放置、分层、镜像、武器组合、CRAM/VInt/DMA 节奏或完成、缓存、运行时修改、持久性、替换策略、许可与呈现一致性。

## 证据所有者与消费面

本合同消费的唯一可执行所有者是 `sf2-battle-sprite-decode-v1`（[fixture](../../../../tests/fixtures/h2/battle-sprite-decode-v1.json)、[verifier](../../../../src/sf2tool/h2/battle_sprites.py)、[schema](../../../../schemas/h2-battle-sprite-decode-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/battle-sprite-decode.json)）。其文章所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

本数据合同消费：

- `table.allyBattlespriteTableAddress` 与 `table.enemyBattlespriteTableAddress`；
- `summary` 中的每个聚合字段与 `sideSummaries` 中的两个有序行；以及
- 已接受的上游、ROM 与规范输出溯源。

`function` 身份只作为外部消费者/服务见证保留。己方/敌人属性选择、调色板操作、帧查找、Stack 交接与固定 DMA 请求仍归[Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md)。`LoadStackCompressedData` 仍归[Graphics Service State](../../contracts/graphics-service-state.md)。那些函数记录都不属于本合同。

完整生成 `payloads[86]` 行保留在忽略的 `local/derived/battle-sprite-decode.json` 下。其源符号、路径、地址、精确相对字、逐载荷大小、调色板与解码 hash、压缩流细节与原始或解码字节是私有验证输入，不是公开合同载荷。

聚合 `sf2-auxiliary-data-static-v1` 测试夹具被显式排除，即使两个目标研究记录都携带该证据。

## 直接绑定与关联边界

专用测试夹具直接绑定恰好七条 research-index 记录：

| 记录 ID | fixture 角色 | 合同处理 |
| --- | --- | --- |
| `auxiliary.data.pt-allybattlesprites` | ROM 地址 1,572,892 的 `pt_AllyBattlesprites` | 新关联候选 |
| `auxiliary.data.pt-enemybattlesprites` | ROM 地址 1,245,188 的 `pt_EnemyBattlesprites` | 新关联候选 |
| `battle.scene.load-enemy-sprite-properties` | 104,816 的敌人属性/调色板加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `battle.scene.load-enemy-sprite-frame` | 104,862 的敌人帧加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `battle.scene.load-ally-sprite-properties` | 104,926 的己方属性/调色板加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `battle.scene.load-ally-sprite-frame` | 104,972 的己方帧加载器 | 不变；只由 `battle-scene-presentation` 保留 |
| `tech.graphics.stack-decompression` | 7,752 的 `LoadStackCompressedData` | 不变；只由 `graphics-service-state` 保留 |

本合同不关联其他 `auxiliary.data.*`、`battle.scene.*`、`tech.graphics.*`、己方/敌人定义、动画序列、武器、地面、背景、效果、立绘、地图精灵、中断、DMA 或呈现记录。

## 有序表与载荷身份

完整私有导入有两个独立有序域：

| 阵营 | 指针槽 | 源载荷定义 | 表地址 |
| --- | ---: | ---: | ---: |
| 己方 | 32 | 32 | 1,572,892 |
| 敌人 | 54 | 54 | 1,245,188 |
| 总计 | 86 | 86 | 独立表 |

对每侧，fixture 验证器闭合源有序一对一指针对应/定义关系。32 与 54 载荷计数意味着 fixture 接受的不同源定义与所有者身份。它们不是每个存储字节序列、压缩流、调色板或解码 hash 互不相同的声称。

完整身份行保持私有。两个指针表对全部 86 个槽有精确原版 ROM 一致性，86 个源载荷定义中的每一个都有精确原版 ROM 一致性。这些事实不证明每个身份的自然选择，也不使数字表顺序成为公开重制 API。

## 容器分区

每个私有源载荷以该有序形状开始：

1. 一个两字节动画速度字；
2. 两个源码具名状态偏移各一个字节；
3. 偏移 4 处解析调色板边界的两字节相对字；
4. 从偏移 6 开始每个帧一个两字节自相对字；
5. 一到四个有序 32 字节调色板；以及
6. 每个帧一个有序 Stack 压缩流。

聚合头大小字段精确：

```text
fixed six-byte prefixes       = 86 x 6  =    516
two-byte frame-relative words = 408 x 2 =    816
header bytes                              1,332
```

5,344 调色板字节与 1,332 头字节分开。它们不得被视为头的子集或计数两次。完整存储字节核算闭合为：

```text
header bytes       =   1,332
palette bytes      =   5,344
compressed bytes   = 492,594
payload bytes      = 499,270
```

侧特定聚合边界是：

| 阵营 | 头字节 | 调色板/字节 | 帧 | 压缩字节 | 载荷字节 |
| --- | ---: | ---: | ---: | ---: | ---: |
| 己方 | 498 | 59 / 1,888 | 153 | 169,856 | 172,242 |
| 敌人 | 834 | 108 / 3,456 | 255 | 322,738 | 327,028 |

己方定义包含三到六帧与一到四个调色板。敌人定义包含二到七帧与一到四个调色板。受追踪动画速度与状态偏移最小值和最大值只是观察到的源字段范围；它们不是时序、坐标或可见放置域。

## 解码形状与聚合诊断

每个己方帧流精确解码到 4,608 字节，给出 705,024 己方解码字节。每个敌人帧流精确解码到 6,144 字节，给出 1,566,720 敌人解码字节。因此完整私有解码语料为 2,271,744 字节。

专用验证器还记录以下聚合诊断：

| 诊断 | 已接受值 |
| --- | ---: |
| 命令组 | 15,889 |
| 字面字 | 204,635 |
| 复制命令 | 46,243 |
| 复制字 | 931,237 |
| 最大复制偏移 | 2,007 字 |
| 最大复制长度 | 33 字 |
| 观察到的尾跨度 | 32..47 位 |

这些值对照维护的解码器验证本语料。它们不要求重制复现原版 Stack 微实现。尾跨度只是每个逻辑终止符后的存储跨度；它不是填充、零填充数据、稳定性或不可见性的证明。

## 实现无关逻辑模型

完整私有导入器可以使用等价模型：

```text
BattleSpriteGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    sides[2] {
        sideIdentity
        pointerTableIdentity
        pointerSlots[] {
            logicalSpriteId
            payloadOwnerId
        }
        privatePayloadDefinitions[] {
            logicalPayloadId
            privateSourceIdentity
            privateSourceAddress
            privateAnimationSpeedWord
            privateStatusOffsetBytes[2]
            privatePaletteBoundaryWord
            privateFrameRelativeWords[]
            privatePaletteWords[][16]
            orderedFrameStreams[] {
                privateCompressedBytes
                privateDecodedBytes
                privateHashesAndDecodeDiagnostics
            }
        }
    }
}
```

逐定义 source/H1/ROM 地址、大端相对字存储、原始头与调色板、压缩字节、解码美术、完整源路径、逐资源大小/hash 与其他非公开细节是私有导入与往返证据。公开投影中命名的受限表与外部见证符号/地址、聚合溯源与元数据保持公开。验证后，合规重制可以使用引擎原生资源引用、调色板、纹理、动画记录与存储。它不需要复现 Mega Drive 地址空间、大端指针或相对字存储、Stack 编解码器、原版缓冲或原版文件/容器布局。

导入器必须保持阵营身份、指针槽身份、载荷所有者身份、头字段身份、调色板身份与有序帧流身份不同。外观相同或解码相同的资源不会仅仅因为本公开合同省略其私有 hash 而合并。

## 公开与私有投影

公开投影只能保留：

- fixture、上游、ROM 与规范输出溯源 hash；
- 两个受限表符号与地址；
- 四个加载器与一个 Stack 服务外部见证身份与地址；
- 聚合与侧特定指针/载荷、头/调色板/帧、字节、解码、一致性、源字段范围与解码器诊断计数；以及
- 受限头/调色板/帧流分区。

它不得发布原始指针、完整指针到定义行、载荷符号/源路径或地址、逐资源偏移/大小/hash、调色板字、压缩字节、解码美术、ROM 摘录、截图、模拟器捕获或渲染呈现。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 其私有导入保留独立有序 32 槽己方与 54 槽敌人指针域及其对应 32 与 54 个源载荷所有者；
2. 每个私有载荷保留精确固定字段、相对调色板边界、有序帧相对字、调色板与帧流身份；
3. 全部 153 个己方流确定性复现其已接受 4,608 字节解码身份，全部 255 个敌人流复现其已接受 6,144 字节解码身份；
4. 完整私有核算以 `1,332 + 5,344 + 492,594 = 499,270` 闭合，解码 2,271,744 字节，且不把调色板字节折进头计数；
5. 引擎原生资源可以替换原版指针、相对字、Stack 存储与地址布局，而不改变逻辑阵营、所有者、调色板或帧身份；
6. 聚合编解码器诊断保持验证事实而非运行时编解码器要求；以及
7. 公开报告只暴露受限聚合/溯源面，而版权载荷与完整私有身份材料保持私有。

H4 不要求原版选择器或动画序列规则、加载器微实现、调色板复制操作、暂存或 DMA 操作数、CRAM/VInt 行为、渲染输出或时序。那些接缝由其所属呈现/服务合同测试。

## 跨系统分离

- [Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md) 消费本合同的规范记录，并保留行动者选择、四个属性/帧加载器身份、动画速度/状态偏移消费、调色板选择与字 0 清除/复制 15 时间线、相对帧查找、Stack 交接、固定 DMA 请求、场景时间线、呈现接缝及其 未知。它不再独立拥有或重新验证本静态目录。
- 不同的行动者动画序列语料与选择器规则仍归 `battle-scene-presentation` 与 `sf2-battle-sprite-animation-static-v1`。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留 `LoadStackCompressedData`、其 ABI 与服务边界。此处的聚合编解码器诊断不转移该所有权。
- 己方/敌人定义合同保留其精灵与调色板选择器字段；它们不拥有这些图形载荷。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 保留 VInt、DMA 与 CRAM 服务/时序边界。
- 背景、地形、武器、地面、效果、立绘、特殊/地图精灵、UI、本地化、可访问性、许可、替换资源与渲染仍归其自身合同或保持 未知。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 两个有序表、32/54 源载荷所有者 | **已确认静态** | `sf2-battle-sprite-decode-v1` | 自然选择、字节唯一性、可见身份 |
| 固定字段、相对字、调色板、有序帧流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 视觉角色、时序、畸形输入 |
| 聚合字节、解码形状、诊断、一致性 | **已确认静态** | 同一 fixture/验证器 | Stack 微实现、尾位含义、渲染美术 |
| 选择器、加载器、调色板操作、Stack 交接、DMA 请求 | **独立所有者已确认静态见证** | `battle-scene-presentation` | 转移完成、时序、可见呈现 |
| 动画序列语料与选择规则 | **独立所有者已确认静态** | `battle-scene-presentation` | 运行时可达性与可见动画 |
| Stack 服务行为 | **独立所有者已确认静态** | `graphics-service-state` | 硬件/运行时时序 |
| 源码标签视觉意图 | **推断，非规范性** | 仅源码词汇 | 行动者/姿态含义、放置、作者意图 |
| 可达性、呈现、持久性、替换 | **未知 / 独立所有者** | 未来受限证据或产品决定 | 此处不是 H4 数据保真要求 |

## 复现

```powershell
uv run sf2 h2 battle-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

完整载荷行保留在忽略的 `local/derived/battle-sprite-decode.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
