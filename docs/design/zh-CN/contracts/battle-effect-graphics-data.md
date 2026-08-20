# 战斗效果图形数据合同

- 状态：**已确认静态资源身份、容器分区与解码形状**
- 证据日期：2026-08-14
- 范围：原版法术、召唤、状态动画与战斗过渡图形语料作为私有、引擎无关导入

> 本文件是 [`battle-effect-graphics-data.md`](../../contracts/battle-effect-graphics-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同拥有原版战斗效果图形语料的静态身份与私有导入形状。它不拥有效果选择、场景控制、加载器行为、解压服务行为、转移完成或呈现。

- **已确认**：23 个法术容器/流所有者；包含 15 帧与 30 个有序流的四个召唤容器所有者；一个状态动画流；两个战斗过渡流；精确私有容器/偏移/调色板/流关系；聚合压缩流与解码字节计数；ROM 一致性计数；以及专用 fixture 复现的受限聚合解码器诊断。
- **推断，非规范性**：`Spell`、`Invocation`、`StatusAnimation`、`BattlesceneTransition`、`frame` 与 `layer` 等名称只保留源码身份与受限消费者词汇。它们不建立可见用途、组合、空间排列、调色板含义、时序或作者意图。
- **未知 / 独立所有者**：每个资源的自然选择与可达性；无效索引与畸形数据；召唤转移尾的内容、稳定性与可见性；调色板含义；层顺序；帧时序；过渡组合；转移完成；渲染输出；缓存、运行时修改、持久性、替换策略与许可。

## 证据所有者与消费面

本合同消费的唯一可执行数据所有者是 `sf2-battle-effect-graphics-decode-v1`（[fixture](../../../../tests/fixtures/h2/battle-effect-graphics-decode-v1.json)、[verifier](../../../../src/sf2tool/h2/battle_effect_graphics.py)、[schema](../../../../schemas/h2-battle-effect-graphics-decode-fixture.schema.json) 与 [manifest](../../../../manifests/extractions/battle-effect-graphics-decode.json)）。其文章所有者是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)与[Battle Scene Engine](../../../research/battle-scene-engine.md)。

本数据合同消费：

- `table` 中全部八个顶层表/根与指针槽身份；
- `summary` 中的族、流、压缩流字节、解码字节、一致性与聚合解码器字段；
- `spellGraphics[23]`、`invocationContainers[4]`、`invocationStreams[30]`、`statusAnimation` 与 `transitionGraphics[2]` 中的完整生成私有行；以及
- 已接受的上游、ROM 与规范输出溯源。

`function` 身份与 `summary.invocationTransferByteCount` / `summary.invocationTransferTailByteCount` 只作为外部消费者/服务见证元数据保留。效果选择、法术/召唤/状态/过渡消费者接缝、Stack 交接、固定转移请求与未知召唤尾仍归[Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md)。`LoadStackCompressedData` 仍归[Graphics Service State](../../contracts/graphics-service-state.md)，面向硬件的转移服务仍归[Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md)。那些函数记录都不属于本合同。

生成私有行保留在忽略的 `local/derived/battle-effect-graphics-decode.json` 下。其完整图、源符号与路径、资源地址、精确容器与流偏移、逐资源大小与 hash、调色板字节、压缩字节与解码美术都是私有验证输入，不是公开合同载荷。

聚合 `sf2-auxiliary-data-static-v1` 与 `sf2-compression-consumers-static-v1` 测试夹具被显式排除。battle-scene engine 与 animation 测试夹具保持呈现/控制所有者，此处不是数据所有者。

## 直接绑定与关联边界

专用测试夹具直接绑定恰好五条 research-index 记录：

| 记录 ID | fixture 角色 | 合同处理 |
| --- | --- | --- |
| `auxiliary.data.pt-spellgraphics` | ROM 地址 1,830,624 的 `pt_SpellGraphics` | 新关联候选 |
| `auxiliary.data.pt-invocationsprites` | ROM 地址 1,221,368 的 `pt_InvocationSprites` | 新关联候选 |
| `auxiliary.data.tiles-statusanimation` | ROM 地址 1,745,262 的 `tiles_StatusAnimation` | 新关联候选 |
| `auxiliary.data.pt-battlescenetransitiontiles` | ROM 地址 1,745,686 的过渡指针表 | 新关联候选 |
| `battle.scene.load-invocation-frame` | ROM 地址 105,458 的召唤消费者 | 不变；只由 `battle-scene-presentation` 保留 |

本合同不关联其他 `auxiliary.data.*`、`battle.scene.*`、`tech.graphics.*`、法术解决、动画、中断、DMA、调色板、音频或呈现记录。

## 顶层根与指针身份

专用测试夹具追踪四个受限源根：

| 根身份 | ROM 地址 |
| --- | ---: |
| `pt_SpellGraphics` | 1,830,624 |
| `pt_InvocationSprites` | 1,221,368 |
| `tiles_StatusAnimation` | 1,745,262 |
| `pt_BattlesceneTransitionTiles` | 1,745,686 |

它还追踪地址 1,802,252、1,048,580、1,572,868 与 1,572,872 处的四个源指针槽身份。这些受限身份与地址是公开溯源元数据。完整资源图与资源地址保持私有。

四个根不都有相同物理形状。法术与召唤根通向有序容器表，状态根标识一个直接流，过渡根通向有序双条目表。合规导入器必须保留这些区分。

## 法术容器身份

私有法术表有 23 个有序源容器定义。每个容器保留：

1. 其源解码大小头身份；
2. 其六字节源调色板前缀；以及
3. 从八字节流前区域之后开始的一个 Stack 压缩流。

每个私有流解码到其自身源头存储的计数。23 计数是源定义/所有者身份计数，不是每个压缩字节序列或解码 hash 互不相同的证明。完整解码大小序列、调色板值、流大小、hash 与字节保持私有。

## 召唤容器、帧与流身份

四个有序源召唤容器定义在以下内容之间保留完整私有关系：

- 一个容器身份；
- 其源帧偏移区域；
- 一个 32 字节调色板区域；
- 其有序帧身份；以及
- 每帧两个有序源流槽。

四个容器包含 15 帧，因此有 30 个有序流。每个召唤流解码到 4,096 字节。`frame` 与两个源槽位置是逻辑源身份；本合同不把它们提升为可见动画、前景/背景、层优先级或时序语义。四个容器所有者与 30 个流槽是源身份，不是每个容器或解码流都有互不相同字节 hash 的主张。

两个源码形状消费者路径为召唤流请求 4,608 字节。该固定请求是呈现/消费者接缝，不是此处拥有的额外解码数据。精确差异是：

```text
4,608 requested - 4,096 decoded = 512 bytes per stream
30 * 512 = 15,360 aggregate tail bytes
```

尾内容、来源、初始化、稳定性、转移完成与可见性为 **未知**。私有导入器不得从相邻调色板、流、内存或容器数据推断或合成那些字节。

## 状态与过渡流身份

状态动画根标识一个解码到 1,248 字节的私有 Stack 流。过渡表标识两个各解码到 6,144 字节的有序源流定义。那些是源资源身份与解码大小事实，不是运行时可达性、可见状态含义、过渡方向、排序意图或字节 hash 唯一性的证明。

呈现合同保留状态消费者的固定 `0x270` 字请求与过渡选择器/指针交接。本数据合同只拥有那些接缝消费的规范私有记录。

## 字段精确聚合核算

已接受流分母闭合为：

```text
23 spell + 30 invocation + 1 status + 2 transition = 56 streams
```

那些流占用 46,364 压缩字节并总共解码到 200,992 字节。`compressedStreamByteCount=46,364` 只计数压缩流跨度。它不是完整源容器分母，不得被描述为包含法术头、法术调色板、召唤偏移区域、召唤调色板、指针表或其他容器材料。

召唤消费者元数据单独闭合：

```text
30 * 4,608 requested bytes = 138,240
30 *   512 unknown tail    =  15,360
```

资源 ROM 一致性计数为 30，分化为 23 个法术容器、四个召唤容器、一个状态资源与两个过渡资源。指针槽 ROM 一致性为四。表 ROM 一致性为三：法术、召唤与过渡。直接状态流不是第四个指针表。

## 解码形状与聚合诊断

专用验证器跨 56 个流记录以下聚合诊断：

| 诊断 | 已接受值 |
| --- | ---: |
| 命令组 | 1,630 |
| 字面字 | 19,091 |
| 复制命令 | 6,480 |
| 复制字 | 81,405 |
| 最大复制偏移 | 1,998 字 |
| 最大复制长度 | 33 字 |
| 观察到的尾跨度 | 32..53 位 |

这些值对照维护的解码器验证本语料。它们不要求重制复现原版 Stack 微实现。尾跨度只是每个逻辑终止符后的存储跨度；它不是填充、零填充数据、稳定性或不可见性的证明。

## 实现无关逻辑模型

完整私有导入器可以使用等价模型：

```text
BattleEffectGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    spellRoot
    privateSpellContainers[23] {
        logicalSpellResourceId
        privateSourceIdentity
        privateDecodedSizeHeader
        privatePaletteBytes[6]
        privateStream
    }
    invocationRoot
    privateInvocationContainers[4] {
        logicalInvocationResourceId
        privateFrameOffsetRelation
        privatePaletteBytes[32]
        privateFrames[] {
            logicalFrameId
            privateLayerStreams[2]
        }
    }
    privateStatusStream
    transitionRoot
    privateTransitionStreams[2]
}
```

每个私有流记录保留其完整源身份、偏移、压缩字节、解码字节、hash 与解码器诊断。完整 source/H1/ROM 地址、大端指针与偏移存储、原始头与调色板、压缩字节、解码美术、源路径、逐资源大小/hash 与其他非公开细节是私有导入与往返证据。公开投影中命名的受限根、指针槽与外部见证身份/地址加聚合溯源与元数据保持公开。

验证后，合规重制可以使用引擎原生资源引用、调色板、纹理、格式与存储。它不需要复现 Mega Drive 地址空间、大端指针/容器布局、Stack 编解码器、原版缓冲或原版源文件。

## 公开与私有投影

公开投影只能保留：

- fixture、上游、ROM 与规范输出溯源 hash；
- 四个受限根身份/地址与四个指针槽地址；
- 六个外部函数见证身份/地址；
- 族/流、压缩流、解码、一致性、召唤请求/尾与解码器诊断聚合计数；
- 受限 `23 + 30 + 1 + 2` 族分区与 `4 containers / 15 frames / 30 streams` 召唤形状；以及
- fixture 已追踪的显式运行时问题与 未知 边界。

它不得发布完整指针/容器/帧/流图、资源符号/源路径或地址、逐资源偏移/大小/hash、调色板值、原始头、压缩字节、解码美术、ROM 摘录、截图、模拟器捕获或渲染呈现。

## H4 重制验收面

未来 H4 实现符合要求时能表明：

1. 其私有导入保留四个不同根形状与所有受限指针/表身份；
2. 全部 23 个私有法术容器所有者保留其解码大小头、六字节调色板与单流关系，且每个流复现其已接受私有解码身份；
3. 全部四个私有召唤容器所有者保留 15 个有序帧身份、每帧两个有序流身份、完整私有偏移关系与独立 32 字节调色板区域；
4. 全部 30 个召唤流复现其已接受 4,096 字节私有解码身份，而不发明独立的 512 字节消费者尾；
5. 私有状态流复现其已接受 1,248 字节解码身份，两个私有过渡流复现其已接受 6,144 字节解码身份；
6. 聚合流核算以 56 流、46,364 压缩流字节与 200,992 解码字节闭合，带已接受一致性/解码器诊断；
7. 引擎原生资源可以替换原版指针、偏移、Stack 存储与地址布局，而不改变逻辑所有者、容器/帧/流关系或私有解码身份；以及
8. 公开报告只暴露受限聚合/溯源面，而版权载荷与完整私有身份材料保持私有。

H4 不要求原版选择器、加载器微实现、召唤转移尾内容、转移完成、调色板含义、层组合、CRAM/VInt/DMA 行为、渲染输出或时序。那些接缝由其所属呈现/服务合同测试。

## 跨系统分离

- [Battle Scene Command 与 Presentation Data](../../contracts/battle-scene-presentation.md) 消费本合同的规范记录，并保留召唤/法术加载器、状态初始化消费、过渡选择、Stack 交接、固定转移请求、512 字节未知召唤尾边界、场景时间线与呈现 未知。它不再独立拥有或重新验证本静态目录。
- [Graphics Service State](../../contracts/graphics-service-state.md) 保留 Stack 解压。此处的聚合解码器诊断不转移服务所有权。
- [Interrupt、DMA 与 Trap State](../../contracts/interrupt-dma-and-trap-state.md) 保留 VInt、DMA、CRAM、转移完成与硬件时序边界。
- 法术解决与战斗动作合同保留效果选择与玩法后果；它们不拥有这些图形载荷。
- 行动者精灵/动画、背景、武器/地面、立绘、地形、特殊/地图/UI 图形、本地化、可访问性、替换资源、许可与渲染仍归其自身合同或保持 未知。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 23 个法术容器/流所有者 | **已确认静态/私有导入** | `sf2-battle-effect-graphics-decode-v1` | 选择、字节唯一性、可见法术含义 |
| 4 个召唤容器、15 帧、30 流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 层含义、时序、自然可达性 |
| 1 个状态与 2 个过渡流 | **已确认静态/私有导入** | 同一 fixture/验证器 | 可见状态/过渡含义与组合 |
| 聚合压缩/解码计数、一致性、诊断 | **已确认静态** | 同一 fixture/验证器 | 完整源容器分母、Stack 微实现、尾位含义 |
| 加载器、Stack 交接、转移请求、召唤尾边界 | **独立所有者已确认静态见证** | `battle-scene-presentation` / `graphics-service-state` | 转移完成与尾内容/稳定性/可见性 |
| 源码标签视觉意图 | **推断，非规范性** | 仅源码词汇 | 组合、调色板/层含义、作者意图 |
| 可达性、呈现、持久性、替换 | **未知 / 独立所有者** | 未来受限证据或产品决定 | 此处不是 H4 数据保真要求 |

## 复现

```powershell
uv run sf2 h2 battle-effect-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

完整私有行保留在忽略的 `local/derived/battle-effect-graphics-decode.json` 下。它们是可复现私有证据，不是受追踪或可分发合同内容。
