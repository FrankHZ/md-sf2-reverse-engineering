# 中断、DMA 与 Trap 状态合同

- 状态：**已确认静态中断调度、转移控制、淡入与 trap 清单**
- 证据日期：2026-08-09
- 范围：原版 VInt 调度器、上下文函数槽、等待/睡眠握手、DMA 控制路线、淡入状态机与受限 trap 服务的实现无关重构，不把源静态顺序转换成硬件时序、可见呈现、队列安全、输入/UI 含义或下游子系统效果

> 本文件是 [`interrupt-dma-and-trap-state.md`](../../contracts/interrupt-dma-and-trap-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从已接受中断与 trap 条目身份开始。它保留调度器门与顺序、状态握手、受限队列变更、淡入控制谓词、上下文槽动作与所选 trap 传输事实。它在面向硬件写入或任何音频、输入、标志、对话、map-script、图形、窗口或其他子系统交接处结束。

- **已确认**：21 文件源清单有 105 个全局标签与 50 个直接调用站点；VInt 使用启用门、保留八阶段更新顺序、上下文函数后清除等待标志，并在更新被跳过时仍递增其帧计数器；八个上下文槽使用启用位域与 60 帧秒计数器；Trap 9 暴露五个已接受动作；`WaitForVInt` 与 `Sleep` 保留其已接受握手；立即与排队 DMA 路线有不同中断/VInt 控制、请求、队列大小、精灵顺序与指针重置事实；四个淡入模式共享已接受设置清除加一次额外 VInt 等待；声音、文本、map-script 与标志 trap 清单事实保留其精确受限形式。
- **推断**：源寄存器与总线操作结构清晰处的面向硬件意图。不推断精确设备时序、转移完成与可见或可听结果。
- **未知**：Z80/VDP 总线延迟；DMA 队列容量、溢出、部分处理与失败恢复；精确中断节奏与嵌套；上下文槽调用方激活；自然 intro 逃逸可达性；可见淡入时长与帧；控制器延迟与 UI 含义；trap 调用方准入与下游结果；精确标志 trap 操作数/返回行为；名义未使用辅助可达性；畸形、修改或注入状态；硬件/模拟器差异；持久性；以及故事或平衡含义。

合同捕获可移植调度与服务边界。它不要求现代引擎模拟 Mega Drive 寄存器或周期时序，除非原版硬件适配器显式拥有该保真目标。

## 证据所有者与关联审计

`sf2-tech-interrupts-static-v1`（[`tech-interrupts-static-v1.json`](../../../../tests/fixtures/h2/tech-interrupts-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`interrupts.py`](../../../../src/sf2tool/h2/interrupts.py)，其带来源解释是[Technical Interrupt、DMA 与 Trap Services](../../../research/technical-interrupts.md)。本合同消费 `expected.interruptFacts` 的所选字段，不消费 H3 fixture。

可执行所有者直接绑定 21 条研究记录。精确未来关联分区是：

- 20 条当前未关联 `tech.interrupts.*` 记录；
- 一个刻意重叠 `tech.interrupts.trap-flags`，其既有[global-flag state](../../contracts/global-flag-state.md)关联保持完整。

重叠很窄。本合同只消费 fixture 拥有的 `flagTrapCount=4` 清单事实。global-flag 合同保持代表 Trap 4 地址与受限 Check/Set/Clear 分组及存储接缝的所有者。内联操作数解码、保存返回移动、调用方可视结果、条件码行为、精确逐 trap 映射与运行时可达性此处不消费，保持 **未知**。

已接受源基线是固定上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6`。20 条新关联记录保留这些代表条目身份与地址：

| 研究记录 | 代表条目 | ROM 地址 |
| --- | --- | ---: |
| `tech.interrupts.z80-fade-input` | `ApplyZ80BusUpdates` | `0x08DE` / 2,270 |
| `tech.interrupts.vram-fill` | `ApplyVramDmaFill` | `0x140E` / 5,134 |
| `tech.interrupts.enable-dma-queue` | `EnableDmaQueueProcessing` | `0x0F2A` / 3,882 |
| `tech.interrupts.errors` | `Int_AddressError` | `0x0490` / 1,168 |
| `tech.interrupts.fading` | `FadeInFromBlack` | `0x0CD6` / 3,286 |
| `tech.interrupts.hint` | `HInt` | `0x0592` / 1,426 |
| `tech.interrupts.trap-sound` | `Trap0_SoundCommand` | `0x045C` / 1,116 |
| `tech.interrupts.trap-text` | `Trap5_TextBox` | `0x0556` / 1,366 |
| `tech.interrupts.trap-map-script` | `Trap6_TriggerAndExecuteMapScript` | `0x057A` / 1,402 |
| `tech.interrupts.trap-contextual` | `Trap9_ManageContextualFunctions` | `0x07CE` / 1,998 |
| `tech.interrupts.unused-palette` | `SetBasePalette1` | `0x0CC4` / 3,268 |
| `tech.interrupts.unused-vint-request` | `RequestVdpCommandQueueProcessing` | `0x0F1A` / 3,866 |
| `tech.interrupts.unused-vint-queue` | `sub_F3A` | `0x0F3A` / 3,898 |
| `tech.interrupts.unused-vint-read` | `sub_13C0` | `0x13C0` / 5,056 |
| `tech.interrupts.scroll-data` | `sub_1234` | `0x1234` / 4,660 |
| `tech.interrupts.vdp-control` | `WaitDmaEnd` | `0x0B96` / 2,966 |
| `tech.interrupts.vint` | `VInt` | `0x0594` / 1,428 |
| `tech.interrupts.vint-engine-core` | `ClearVsramAndSprites` | `0x0DBA` / 3,514 |
| `tech.interrupts.vint-engine-dma` | `ApplyImmediateVramDma` | `0x10DC` / 4,316 |
| `tech.interrupts.vint-engine-compressed` | `ApplyImmediateVramDmaOnCompressedTiles` | `0x1382` / 4,994 |

`tech.interrupts.trap-flags` 刻意缺席该地址表，因为其地址与包装器分组保持 global-flag 所有者。共享 fixture 不把该证据转移给本合同。

## VInt 调度

**已确认静态：** 已接受 VInt 更新块只在其源启用位设置时执行。启用时，其高层顺序恰好是：

1. 等待 DMA；
2. 禁用显示；
3. 处理 VDP 队列；
4. 启用显示；
5. 处理 VRAM 读取阶段；
6. 应用淡入；
7. 执行 Z80/输入阶段；
8. 管理上下文函数。

上下文函数后，VInt 清除 `WaitForVInt` 等待的标志。门控更新块被跳过时帧计数器仍递增。这些是源静态顺序与状态事实，不是周期计数、帧完美呈现、设备完成或每个阶段每次调用都执行工作的证明。

源还包含通过配置的一次性指针在观察到 Start 时的条件 intro 逃逸。本合同只保留分支能力。指针配置、精确输入采样、正常 intro 可达性、继续行为、可见过渡与玩家意图保持[startup control flow](../../contracts/startup-control-flow.md)、[special-screen control flow](../../contracts/special-screen-control-flow.md)、输入所有者或 **未知**。

## 上下文函数、等待与睡眠

**已确认静态：** 上下文函数使用由八位启用字段管辖的八个指针槽。其帧计数器在已接受 60 帧阈值后推进秒计数器。阈值是源计数器，不是每个主机上的墙钟保证。

Trap 9 恰好暴露五个源动作：

1. 清除指针；
2. 设置函数并触发；
3. 清除函数并触发；
4. 清除触发；
5. 设置触发。

这是动作清单，不是槽准入、回调 ABI、调用方特定调度、可重入性、执行频率或可见行为的声称。

`WaitForVInt` 设置更新启用位并自旋直到 VInt 清除其等待标志。`Sleep(0)` 不等待即返回；正源参数为请求帧数重复 VInt 握手。已接受关系不建立精确经过时间、主机线程行为、中断延迟、跳过 VInt 顺序或失败恢复。

[input-system 合同](../../contracts/input-system.md) 拥有已接受直接等待辅助 H3 进展与静态/运行时 24 帧初始输入重复延迟及六帧重复节奏。尽管那些 `inputRepeat` 事实在同一 H2 fixture 中，本合同刻意不把它们变成 H4 保真面，也不添加输入特定研究关联。

## DMA 控制路线

**已确认静态：** 立即 VRAM DMA 路径掩蔽中断并请求 Z80 总线。排队路径相反临时禁用 VInt，同时追加其命令并递增队列大小。这些是不同控制路线；fixture 不建立任一者对任意工作负载更优或安全。

队列处理保留两个已接受事实：

- 精灵表更新发生在排队转移前；
- 处理后重置队列指针。

源谓词要求处理请求，除非 DMA 已活跃。合同不推断队列容量、溢出保护、精确命令布局、原子性、队列大小重置、部分完成、转移成功、总线释放时序或无效条目恢复。

`ApplyVramDmaFill`、`WaitDmaEnd`、`ApplyImmediateVramDma` 与 `ApplyImmediateVramDmaOnCompressedTiles` 保持条目/控制身份。压缩载荷布局、解码器微实现、资源溯源、解码字节、目标与渲染结果保持[graphics-service state](../../contracts/graphics-service-state.md)、适当资源合同或 **未知**。

## 淡入控制

**已确认静态：** 已接受淡入条目身份选择四个模式：

- 从黑淡入；
- 淡出到黑；
- 从白淡入；
- 淡出到白。

淡入执行把调色板选择位域初始化为 15、等待直到 VInt 清除淡入设置，然后额外等待一个 VInt。每个源颜色分量在颜色更新后排队 CRAM DMA 前被夹断到半字节范围。

这些事实只定义控制状态与顺序。它们不定义可见时长、调色板内容、色彩空间等价、DMA 节奏或完成、可访问性、场景所有权或玩家在条目与返回之间看到什么。现代渲染器可以在保留已接受模式与设置清除加一次额外 VInt 控制握手的兼容轨迹的同时实现主机原生过渡。

## Trap 清单与交接

### 声音命令 trap

**已确认静态：** 声音 trap 有四个命令槽。内联参数 `-1` 从 `d0` 选择值，禁用声音命令导致命令被丢弃。这是受限命令传输事实，不是音频队列容量证明、命令域合同、可听输出声称或驱动时序保证。那些保持[audio-system](../../contracts/audio-system.md)或 **未知**。

### 标志 trap

**已确认静态清单 only：** fixture 记录 `flagTrapCount=4`。此处不消费额外标志 trap 行为。[global-flag state 合同](../../contracts/global-flag-state.md) 保持其已接受地址/分组/存储接缝的唯一设计所有者，所有内联操作数、返回地址、结果、条件码、映射与运行时问题保持本合同之外。

### 文本与 map-script trap

**已确认静态：** 文本 trap 把源文本索引 `-1` 当作其关闭对话路线。其他索引交接给文本显示。map-script trap 在其 map-script 执行交接前激活实体 VInt 函数。

`-1` 路线与交接顺序不导入文本解码、对话窗口状态、命令时序、map-script 分发、实体行为、故事含义、呈现或调用方可达性。那些保持[dialogue-system](../../contracts/dialogue-system.md)、[map-exploration](../../contracts/map-exploration.md)、其证据所有者或 **未知**。

## 仅清单处理器

完整源边界保留错误与 HInt 处理器加一个调色板辅助与三个显式未使用 VInt 辅助。其代表身份与地址是已确认静态。源名与清单分类不证明代码在间接调用、修改构建、调试路线、注入状态或替代平台条件下不可达。

没有错误画面、HInt 节奏、滚动呈现、调色板结果、恢复策略或未使用辅助效果被提升为重制要求。兼容适配器可以保留身份级可追溯性，而不在可移植引擎 API 中暴露这些例程。

## 跨系统分离

中断代码是传输与调度面，不是每个被调方的所有权：

- [input-system](../../contracts/input-system.md) 拥有控制器采样、重复节奏与直接等待辅助 H3 行为；
- [global-flag state](../../contracts/global-flag-state.md) 拥有已接受标志存储与包装器边界；
- [audio-system](../../contracts/audio-system.md) 拥有声音命令域、驱动状态与可听边界；
- [dialogue-system](../../contracts/dialogue-system.md) 拥有文本命令与调用方接缝；
- [map-exploration](../../contracts/map-exploration.md) 拥有已接受 map-script、实体、布局与呈现行为；
- [graphics-service state](../../contracts/graphics-service-state.md) 拥有图形服务与解压条目边界；
- [window-system](../../contracts/window-system.md) 拥有窗口组合及其转移调用顺序；
- [startup control flow](../../contracts/startup-control-flow.md)与[special-screen control flow](../../contracts/special-screen-control-flow.md) 拥有启动与 intro/标题路线。

本合同不因隐含而关联任何输入、标志、音频、对话、地图、图形、窗口或启动研究记录。唯一刻意重叠是 `tech.interrupts.trap-flags`，且限于上文描述的四 trap 清单计数。

## 实现无关状态模型

最小逻辑投影存储控制元数据而非硬件载荷：

```text
InterruptServicePlan
  evidenceOwner: sf2-tech-interrupts-static-v1.expected.interruptFacts
  sourceCommit
  sourceFiles: 21
  representativeEntries[20]:
    researchRecordId
    symbol
    romAddress

  vint:
    updatesRequireEnableBit: true
    orderedStages[8]
    clearWaitingFlagAfterContextualFunctions: true
    frameCounterIncrementsWhenUpdateBlockSkipped: true
    introEscapePointerBranchIdentity

  contextualSlots:
    slotCount: 8
    enableBitfield
    secondsCounterThresholdFrames: 60
    trap9Actions[5]

  waitAndSleep:
    waitSetsEnableBit: true
    waitUntilVintClearsFlag: true
    zeroSleepReturns: true
    positiveSleepRepeatsHandshake: true

  dmaControl:
    immediateMasksInterruptsAndRequestsZ80Bus: true
    queuedTemporarilyDisablesVint: true
    queuedEntryIncrementsQueueSize: true
    processingRequestPredicate
    spriteBeforeQueuedTransfers: true
    processingResetsQueuePointer: true

  fadeControl:
    modes[4]
    initialPaletteBitfield: 15
    waitForSettingClear: true
    oneAdditionalVint: true
    componentNibbleClamp: true
    queueCramDmaAfterColorUpdate: true

  traps:
    soundSlots: 4
    soundMinusOneUsesD0: true
    disabledSoundDropsCommand: true
    flagTrapInventoryCount: 4
    textMinusOneCloseRoute: true
    mapScriptActivatesEntityVintFirst: true

  inventoryOnlyHandlerIdentities
```

公开模型省略源代码、ROM 字节、队列内容、调色板、压缩或解码资源、文本、音频、活跃 RAM、轨迹、模拟器状态与捕获帧。私有原版保真工具可以从许可或用户提供输入重构那些值而不发布它们。

## 原版保真与现代化

原版保真测试保留调度器顺序、状态握手、受限队列变更、淡入控制、trap 清单与代表条目身份。它把硬件时序、下游效果与调用方准入保持为独立观察，而非静默填充。

现代引擎可以使用主机事件循环、类型化回调注册表、受限转移队列、渲染器原生淡入与显式服务接口。此类选择是现代化。兼容适配器仍应能发出已接受控制轨迹并识别刻意偏差，而不暴露私有载荷。

## H4 验收门

未来中断服务适配器只在以下情况通过本合同：

1. 精确 21 记录关联边界保持可复现为 20 条新记录加一个刻意 `tech.interrupts.trap-flags` 重叠，无相邻记录变更；
2. 20 个代表条目身份与地址保持可追溯，而标志 trap 地址/分组只保持 global-flag 所有者；
3. VInt 保留启用门、八阶段顺序、上下文后等待标志清除与跳过更新帧计数器递增，而不把它们转换成周期或呈现声称；
4. 八个上下文槽、其启用位域、60 帧源计数器与五个 Trap 9 动作保持可复现，而不断言调用方特定激活；
5. 等待/睡眠握手保留零对正行为，而输入重复与直接 H3 进展保持 input-system；
6. 立即与排队 DMA 控制事实、请求谓词、精灵先于队列顺序与指针重置保持不同，容量、溢出、命令布局、时序与完成分别测试或 **未知**；
7. 四个淡入模式、位域 15、设置清除加一次额外 VInt 等待、半字节夹断与 CRAM-DMA 队列顺序保持可复现，而不声称可见帧；
8. 声音、文本与 map-script trap 事实保持仅传输/交接事实，而标志 trap 在本合同只暴露 `flagTrapCount=4`；
9. 错误、HInt、调色板、滚动与未使用辅助身份保持清单事实，无死代码或运行时效果声称；
10. 公开工件只包含元数据与合成状态，绝不包含 ROM、代码、队列、调色板、图形、文本、音频、活跃内存、轨迹、存档或模拟器状态载荷。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 21 文件清单与 20 个所选条目身份/地址 | **已确认静态** | `sf2-tech-interrupts-static-v1`（[`tech-interrupts-static-v1.json`](../../../../tests/fixtures/h2/tech-interrupts-static-v1.json)） | 间接可达性、alternate 构建、运行时效果 |
| VInt 门/顺序、上下文槽与等待/睡眠握手 | **已确认静态** | 同一 `expected.interruptFacts` 所有者 | 节奏、嵌套、回调激活、失败行为 |
| 立即/排队 DMA 控制与处理顺序 | **已确认静态** | 同一所有者 | 容量、溢出、命令布局、硬件时序/完成 |
| 淡入状态与设置清除加一次额外 VInt 控制握手 | **已确认静态** | 同一所有者 | 转移完成、调色板内容、可见淡入完成/时长、CRAM-DMA 节奏 |
| 声音/文本/map-script trap 传输与交接 | **已确认静态** | 同一所有者 | 下游音频/对话/地图行为与调用方准入 |
| 四个标志 trap | **已确认静态清单 only** | 同一所有者计数；[global-flag state](../../contracts/global-flag-state.md) 其已接受地址/分组/存储接缝 | 操作数、返回移动、结果/CCR、精确映射、运行时可达性 |
| 输入重复与直接等待辅助运行时进展 | **独立所有者已确认** | [input-system](../../contracts/input-system.md) | 此处不是 H4 面 |
| 面向硬件意图 | **推断** | 源寄存器与总线操作 | 精确设备时序保持 **未知** |
| 可见与可听结果 | **未知** | 未来受限运行时证据 | 不得从静态顺序推断呈现 parity |

## 复现

```powershell
uv run sf2 h2 tech-interrupts
uv run sf2 design-contracts test
uv run sf2 verify
```

生成 JSON 保留在忽略的 `local/derived/tech-interrupts-static.json` 下。
