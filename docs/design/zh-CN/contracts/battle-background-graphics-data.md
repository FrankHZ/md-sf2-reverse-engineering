# 战斗背景图形数据合同

- 状态：**已确认静态资源身份、容器分区、别名与解码形状**
- 证据日期：2026-08-14
- 范围：原版战斗背景指针与容器语料作为私有、引擎无关导入

> 本文件是 [`battle-background-graphics-data.md`](../../contracts/battle-background-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同拥有原版战斗背景语料的静态数据身份与私有导入形状。它不拥有场景选择、加载器控制、解压服务行为、转移完成或呈现。

- **已确认**：一个解析到 27 个有序容器身份的 30 槽指针表；精确三个指针别名；每容器 38 字节流前前缀与两个有序压缩流身份；精确聚合字节/解码/parity 计数；以及专用 fixture 复现的受限聚合解码器诊断。
- **推断，非规范性**：`Background`、`tileset 1` 与 `tileset 2` 等名称只保留源码身份与顺序。它们不建立可见层角色、场景含义、空间排列或作者意图。
- **未知 / 独立所有者**：每个槽的自然选择与可达性；渲染瓦片与调色板含义；组合、坐标、调色板优先级、淡入、CRAM/VInt/DMA 节奏或完成、时序、缓存、运行时修改、持久性、畸形输入行为、替换策略与呈现 parity。

## 证据所有者与消费面

本合同消费的唯一可执行所有者是 `sf2-battle-background-decode-v1`（[fixture](../../../../tests/fixtures/h2/battle-background-decode-v1.json)、[verifier](../../../../src/sf2tool/h2/battle_backgrounds.py)、[schema](../../../../schemas/h2-battle-background-decode-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/battle-background-decode.json)）。其文章所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

本数据合同消费：

- `table.backgroundTableAddress`；
- `summary` 中的每个聚合字段；
- `aliases` 中的三条受追踪行；以及
- 已接受的上游、ROM 与规范输出溯源。

`function` 身份只作为外部消费者/服务见证保留。`LoadBattlesceneBackground` 选择/加载器/调色板接缝仍归[Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md)，而 `LoadStackCompressedData` 仍归[Graphics Service State](../../contracts/graphics-service-state.md)。两个函数记录都不属于本合同。

完整生成 `payloads[27]` 行保留在忽略的 `local/derived/battle-background-decode.json` 下。其源路径、地址、精确偏移、大小、调色板 hash、压缩流细节与解码 hash 是私有验证输入，不是公开合同载荷。

聚合 `sf2-auxiliary-data-static-v1` 测试夹具被显式排除，即使目标研究记录也携带该证据。

## 直接绑定与关联边界

专用测试夹具直接绑定恰好三条 research-index 记录：

| 记录 ID | fixture 角色 | 合同处理 |
| --- | --- | --- |
| `auxiliary.data.pt-backgrounds` | ROM 地址 1,056,480 的 `pt_Backgrounds` | 唯一新关联候选 |
| `battle.scene.load-background` | ROM 地址 105,344 的 `LoadBattlesceneBackground` | 不变；只由 `battle-scene-presentation` 保留 |
| `tech.graphics.stack-decompression` | ROM 地址 7,752 的 `LoadStackCompressedData` | 不变；只由 `graphics-service-state` 保留 |

本合同不关联其他 `auxiliary.data.*`、`battle.scene.*`、`tech.graphics.*`、调色板、精灵、武器、地面、特殊画面、UI、地图、中断或呈现记录。

## 有序指针与容器身份

完整表有 30 个有序指针槽，解析到 27 个有序容器定义。三个槽复用早期定义：

| 背景槽 | 载荷所有者槽 |
| ---: | ---: |
| 21 | 12 |
| 22 | 12 |
| 29 | 13 |

完整身份行保持私有。指针表对全部 30 个槽有精确原版 ROM parity，27 个源载荷中的每一个都有精确原版 ROM parity。

这些是逻辑资源身份与别名，不是每个槽自然被选择或原版数字顺序是合适重制面向 API 的证明。

## 容器前缀与流分区

每个私有容器以 38 字节流前前缀开始：

1. 一个包含三个大端相对字的六字节头；
2. 从容器偏移 6 开始的 32 字节源调色板；以及
3. 从容器偏移 38 开始的第一个压缩流。

验证器字段精确地解析私有偏移：

```text
tileset1Offset = word@0 = 38
tileset2Offset = 2 + word@2
paletteOffset  = 4 + word@4 = 6
paletteEnd     = 38
```

`summary.headerByteCount=1026` 是跨 27 个容器的完整 38 字节流前前缀聚合：`27 × (6 相对字字节 + 32 调色板字节)`。它不是 1,026 字节相对字头。`summary.paletteByteCount=864` 是该前缀计数的子集，不得再加一次。

完整字节核算闭合为：

```text
prefix bytes       = 1,026
compressed bytes   = 163,742
payload bytes      = 164,768
```

每个容器中的两个有序流是源身份。本合同不把它们指定为可见半部、平面、方向、动画阶段或层优先级。

## 解码形状与聚合诊断

27 个容器包含 54 个 Stack 压缩流。每个私有流精确解码到 6,144 字节，因此完整私有解码语料为 331,776 字节。

专用验证器还记录以下聚合诊断：

| 诊断 | 已接受值 |
| --- | ---: |
| 命令组 | 7,002 |
| 字面字 | 93,129 |
| 复制命令 | 18,472 |
| 复制字 | 72,759 |
| 最大复制偏移 | 2,014 字 |
| 最大复制长度 | 33 字 |
| 观察到的尾跨度 | 32..47 位 |

这些值对照维护的解码器验证本语料。它们不要求重制复现原版 Stack 微实现。尾跨度只是每个逻辑终止符后的存储跨度；它不是填充、零填充数据、稳定性或不可见性的证明。

## 实现无关逻辑模型

完整私有导入器可以使用等价模型：

```text
BattleBackgroundCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    pointerSlots[30] {
        logicalBackgroundId
        payloadOwnerId
    }
    privatePayloads[27] {
        logicalPayloadId
        privateSourceIdentity
        privateSourceAddress
        privatePrefixBytes[38]
        privatePaletteWords[16]
        orderedStreams[2] {
            privateCompressedBytes
            privateDecodedBytes[6144]
            privateHashesAndDecodeDiagnostics
        }
    }
}
```

逐容器 source/H1/ROM 地址、相对字存储、原始前缀/调色板字节、压缩字节、解码美术、逐资源 hash、完整载荷源路径与其他非公开细节是私有导入与往返证据。公开投影中命名的受限表与外部见证符号/地址、聚合溯源与元数据保持公开。验证后，合规重制可以使用引擎原生资源引用、调色板、资源与存储。它不需要复现 Mega Drive 地址空间、大端相对字、Stack 编解码器、原版暂存缓冲或原版文件/容器布局。

导入器必须保持指针槽身份、载荷所有者身份、两个有序流身份与源调色板身份不同。别名指针槽不变成重复载荷所有者。

## 公开与私有投影

公开投影只能保留：

- fixture、上游、ROM 与规范输出溯源 hash；
- `pt_Backgrounds` 符号与表地址；
- 受限外部 `LoadBattlesceneBackground` 与 `LoadStackCompressedData` 见证身份与地址；
- 聚合指针/载荷/别名、字节、解码、parity 与解码器诊断计数；
- 三条受追踪别名元数据行；以及
- 受限 38 字节前缀分区与每容器双流规则。

它不得发布原始指针、完整非别名赋值、容器符号/源路径或地址、逐资源偏移/大小/hash、调色板字、压缩字节、解码美术、ROM 摘录、模拟器捕获或渲染呈现。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 其私有导入保留 30 个有序逻辑槽与 27 个有序载荷所有者；
2. 精确 `21→12`、`22→12` 与 `29→13` 别名关系在不复制所有者的情况下保留；
3. 全部 27 个私有容器保留精确 38 字节前缀分区与两个有序流身份；
4. 全部 54 个私有流确定性复现其已接受 6,144 字节解码身份；
5. 完整私有字节核算以 `1,026 + 163,742 = 164,768` 闭合，而不双计数 864 调色板字节；
6. 引擎原生资源可以替换原版指针、相对字、Stack 存储与地址布局，而不改变逻辑身份或别名；以及
7. 公开报告只暴露受限聚合/溯源面，而版权载荷与完整私有身份材料保持私有。

H4 不要求原版选择规则、加载器微实现、暂存地址、调色板操作时间线、DMA/CRAM/VInt 行为、渲染输出或时序。那些接缝由其所属呈现/服务合同测试。

## 跨系统分离

- [Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md) 消费本合同的规范记录，并保留背景选择、`LoadBattlesceneBackground`、两个连续暂存目标、调色板字 0 清除/复制 15 时间线、转移/呈现边界及其 Unknown。它不再独立拥有或重新验证本目录。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留 `LoadStackCompressedData`、其 ABI 与服务边界。此处的聚合编解码器诊断不转移该所有权。
- battle AI、动作构建、交战解决与战斗生命周期合同保留场景选择输入、战斗效果与结果。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 保留 VInt、DMA 与 CRAM 服务/时序边界。
- 行动者战斗精灵、武器、地面、效果、召唤、状态/过渡图形、特殊画面/UI/地图图形、本地化、可访问性与渲染仍归其自身合同或保持 Unknown。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 30 个有序槽、27 个载荷所有者、三个别名 | **已确认静态** | `sf2-battle-background-decode-v1` | 自然选择、场景含义、渲染排列 |
| 每容器 38 字节前缀与两个有序流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 视觉角色、平台存储、畸形输入 |
| 聚合字节、解码形状、诊断、parity | **已确认静态** | 同一 fixture/验证器 | Stack 微实现、尾位含义、渲染美术 |
| 加载器/暂存/调色板时间线 | **独立所有者已确认静态见证** | `battle-scene-presentation` | 转移完成、时序、可见调色板 |
| Stack 服务行为 | **独立所有者已确认静态** | `graphics-service-state` | 硬件/运行时时序 |
| 源码标签视觉意图 | **推断，非规范性** | 仅源码词汇 | 层角色、方向、作者意图 |
| 可达性、呈现、持久性、替换 | **未知 / 独立所有者** | 未来受限证据或产品决定 | 此处不是 H4 数据保真要求 |

## 复现

```powershell
uv run sf2 h2 battle-backgrounds
uv run sf2 design-contracts test
uv run sf2 verify
```

完整载荷行保留在忽略的 `local/derived/battle-background-decode.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
