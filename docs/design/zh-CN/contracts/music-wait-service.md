# 音乐等待服务合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对下文描述的受限条目身份、有序源宏操作数与睡眠后谓词循环）
- 现代化：**允许** 在源兼容轨迹背后使用引擎原生异步完成、事件、future 或回调
- 未知：调用方准入、命令传输与接受、声音侧标志生命周期、可听完成、过渡行为、调度、经过时间、失败处理与呈现

> 本文件是 [`music-wait-service.md`](../../contracts/music-wait-service.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同定义 `PlayMusicAfterCurrentOne` 已接受静态证据支持的最小实现无关服务形状。它保留原版源顺序与重试谓词，而不把 Mega Drive 声音命令 trap、`Sleep`、VInt 调度、Z80 状态或可听播放变成本文档拥有的证据。

合同刻意是控制边界合同。两条 `sndCom` 语句是有序源命令请求/宏操作数身份。它们不是任一请求被声音侧传输、排队、接受或完成的证明。同样，被测试字节是具名源谓词。其玩家可见或可听含义保持在该证据之外。

## 判断边界

**已确认静态：** 已接受 fixture 把 `PlayMusicAfterCurrentOne` 绑定到 ROM 地址 `0x16BE`（`5822`）并记录源睡眠参数 `3`。固定源按顺序放置以下形式：

1. `sndCom SOUND_COMMAND_WAIT_MUSIC_END`；
2. `sndCom SOUND_COMMAND_GET_D0_PARAMETER`；
3. `moveq #3,d0`；
4. `bsr Sleep`；
5. `tst.b WAIT_FOR_MUSIC_END`；
6. `bne @Wait`；
7. 零路径上的 `rts`。

循环是睡眠后与请求后：它不在首次 `Sleep(3)` 请求前测试谓词。如果首次观察为零，源码形状轨迹仍恰好包含一个等待请求。如果首次零观察前有 `k` 次非零观察，轨迹恰好包含 `k + 1` 个等待请求。

**推断：** 源符号、命令名与注释暗示一个旨在把请求音乐变更推迟到当前音乐条件完成的工具。这只是工程意图。它不确认普通调用方含义、声音驱动解读或玩家听到的任何东西。

**未知或排除：** 已准入调用方集；源注释 `d0.w` 音乐索引与 `$FB` 上一音乐含义作为受支持公开 API；命令数字编码、trap 传输、禁用声音行为、接受、排队与处理；哪个组件设置或清除 `WAIT_FOR_MUSIC_END`；字节是否对应可听完成；淡入、过渡、恢复或静音行为；`Sleep`/VInt 节奏；经过时间；中断延迟；调度；可重入性；并发；取消；死锁与恢复；可见源写入与调用之外的寄存器或 CCR 行为；UI、场景、故事、持久性、可访问性与呈现效果。

## 证据合同

本合同只消费以下来自[`sf2-tech-services-static-v1`](../../../../tests/fixtures/h2/tech-services-static-v1.json)的面：

- `function.PlayMusicAfterCurrentOne`；
- `expected.serviceFacts.musicWaitSleepFrameCount`；
- `upstreamCommit` 与 `romSha256` 溯源。

fixture 字段名包含“FrameCount”，但本合同只把其值解读为提供给 `Sleep` 的源参数。精确帧节奏与墙钟时长不由该 fixture 建立。

受限时间线直接在固定[`music.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/sound/music.asm)中审查，并由所属[`technical-services.md`](../../../research/technical-services.md)总结。可执行静态验证器是[`services.py`](../../../../src/sf2tool/h2/services.py)。

验证器解析 H1 条目身份，并在已接受 ROM SHA 溯源下检查具名源片段。它不建立该函数体、宏展开或指令编码的逐字节 H1/ROM parity。那些更强比较此处不是已确认事实或 H4 要求。

本合同**不**消费 `expected.resourceFacts`、`expected.soundDriverFacts`、`expected.inputFacts`、`expected.randomServicesFacts`、`expected.sramFacts`、任何其他 `expected.serviceFacts` 字段或 `expected.runtimeQuestions`。

### 精确 research-index 分母

已接受 fixture 直接链接十条研究记录。本合同恰好改变一条的语义关联：

| 记录 | 本合同后的设计所有权 |
| --- | --- |
| `tech.services.music-wait` | 本合同；注册前当前未关联 |
| `tech.services.byte-copy` | 仍归[`byte-copy-service`](../../contracts/byte-copy-service.md) |
| `tech.services.resource-icon` | 仍归 `ui-graphics-asset-data` |
| `tech.services.resource-graphics` | 仍归 `text-and-font-system` |
| `tech.services.resource-text-trees` | 仍归 `text-and-font-system` |
| `tech.services.resource-title` | 仍归 `unused-technical-asset-data` |
| `tech.services.resource-base` | 仍归 `unused-technical-asset-data` |
| `tech.services.input` | 仍归 `input-system` |
| `tech.services.sram` | 仍归 `save-system` |
| `tech.services.thinking-rng` | 仍归 `randomness` |

共享聚合 fixture 不把任何兄弟事实或关联转移给本合同。

## 源码形状控制序列

### 有序命令请求身份

两个前导 `sndCom` 形式保留该源顺序：

1. `SOUND_COMMAND_WAIT_MUSIC_END`；
2. `SOUND_COMMAND_GET_D0_PARAMETER`。

这些是 68000 源接缝处的宏操作数身份。本合同中的“Request”只意味着源用该操作数调用宏。它不意味着 trap 传输了请求、邮箱改变、Z80 接受它或音频队列存在。

数字命令值、内联 trap 编码、参数载荷与源注释输入域不是本合同运行时模型的一部分。现代实现可以使用类型化命令或直接服务调用，同时保留带相同两个身份、相同顺序的抽象兼容轨迹。

### 睡眠后谓词顺序

两个源请求后，每个源码形状循环迭代都有该顺序：

1. 用源等待参数 `3` 替换 `d0`；
2. 调用 `Sleep`；
3. 把 `WAIT_FOR_MUSIC_END` 观察为字节；
4. 该观察非零时重复；
5. 该观察为零时返回。

谓词从来不是进入首次等待的前置条件。这产生精确抽象轨迹关系：

| 观察谓词序列 | 等待请求 | 源码形状结果 |
| --- | ---: | --- |
| `[0]` | 1 | 首次等待与零观察后返回 |
| `[nonzero, 0]` | 2 | 一次重试，然后返回 |
| `k` 次非零观察后接 `0` | `k + 1` | 只在最终零观察后返回 |

无零的观察流在该受限模型中没有已确认返回。合同不发明超时、取消、错误或回退策略。

### 独立所有者等待语义

[interrupt/DMA/trap 合同](../../contracts/interrupt-dma-and-trap-state.md) 拥有 `Sleep` 与 `WaitForVInt` 的已接受静态行为：正源参数重复其等待握手。本合同只保留调用顺序与参数 `3`。它不消费中断 fixture、不定义 VInt 节奏，也不把三个源等待迭代转换成墙钟时间。

同样，[audio-system 合同](../../contracts/audio-system.md) 拥有声音驱动数据、命令选择、受限播放状态、通道行为与既有可听/时序 Unknown。本合同不把其谓词重新解读为可听完成。

## 实现无关模型

合规导入可以把静态证据暴露为：

```text
MusicWaitServiceEvidence
  identity
    sourceSymbol = PlayMusicAfterCurrentOne
    sourcePath
    h1ResolvedEntryAddress = 0x16BE
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  orderedSourceCommandRequests[2]
    macroName = sndCom
    operandIdentity

  retryControl
    waitArgument = 3
    order = WAIT_REQUEST_THEN_PREDICATE_TEST
    predicateIdentity = WAIT_FOR_MUSIC_END
    repeatCondition = NONZERO
    returnCondition = ZERO
    waitRequestCountForKNonzeroThenZero = k + 1

  excludedRuntimeMeaning
    commandTransport
    commandAcceptance
    soundSideFlagLifecycle
    audibleCompletion
    elapsedTiming
```

该模型区分导入证据与重制运行时。完整或精确源体文本与转储、完整宏展开或指令字节、私有 ROM 摘录与其他非公开往返验证材料保持私有输入。下文公开投影中列出的受限符号、H1 条目、宏操作数名、等待计数与顺序、hash 与溯源保持公开元数据。验证后，重制可以使用引擎原生命令对象、引用、事件、future、promise 或回调。它不需要复现 Mega Drive 地址空间、trap 编码、原版轮询循环、VInt 调度或 Z80 内存。

等待事件的引擎原生实现仍可以提供发出抽象有序命令请求与等待/谓词轨迹的兼容适配器。此类轨迹只证明模型等价；它不声称原版使用事件，或任一实现有相同实时时长。

## 公开与私有投影

公开合同可以保留：

- 源符号与 H1 解析条目地址；
- 两个有序宏操作数身份名；
- 源等待参数 `3`；
- 睡眠后谓词顺序与 `k + 1` 轨迹关系；
- fixture 身份、hash、上游修订与受限溯源。

公开形式不得发布原版源体字节、指令编码、宏展开体字节、私有 ROM 摘录、捕获声音状态、音乐数据、解码音频、模拟器轨迹或版权音视频内容。完整或精确源体比较与指令字节比较可以由未来更强验证器私有使用，但此处不是已接受 parity 证据。

## 跨系统分离

- [`audio-system`](../../contracts/audio-system.md) 保留声音命令、Z80 驱动/通道状态、受限播放观察与所有可听、过渡、淡入、恢复、优先级、混音与时序边界。
- [`interrupt-dma-and-trap-state`](../../contracts/interrupt-dma-and-trap-state.md) 保留声音 trap 传输事实与 `Sleep`/`WaitForVInt` 控制语义。本合同不消费该 fixture。
- 菜单、服务、战斗、地图、故事与特殊画面所有者保留其自身调用方与场景含义。直接调用方清单与自然可达性在本合同之外。
- `byte-copy-service`、输入、随机性、SRAM、文本/字体、UI 图形与未使用技术资源保持兄弟合同，其事实与关联不变。
- 禁用声音行为、命令丢失、队列压力、可重入性、取消与失败恢复保持 **未知**，而非从源循环推断。

## H4 验收面

未来实现满足本合同时：

1. 已接受 fixture 身份、上游 commit、ROM SHA 溯源、源符号与 H1 解析条目保持可追溯；
2. 导入静态模型保留恰好两个有序源命令请求/宏操作数身份，而不声称成功传输、排队或接受；
3. 兼容轨迹保留 `WAIT_REQUEST_THEN_PREDICATE_TEST` 顺序；
4. 立即零谓词序列恰好产生一个等待请求，绝不零；
5. `k` 次非零谓词观察后接零恰好产生 `k + 1` 个等待请求，且只在零观察后返回；
6. 每个源兼容等待请求携带源参数 `3`，而不把该计数转换成墙钟或可听时长 parity；
7. 合成用例覆盖立即零、一次非零后零与多次非零后零，并报告有序抽象轨迹；
8. 允许引擎原生事件/future/回调实现，且不需要复现原版 trap、轮询、`Sleep`、VInt、Z80、地址或指令微实现；
9. 命令处理、标志生命周期、调用方准入、可听完成、时序、失败、寄存器/CCR 行为与呈现保持独立证据或显式 **未知**；
10. 公开报告保持仅元数据并尊重私有/版权边界。

该验收面刻意对无尽非零谓词流沉默。重制可以采用超时或取消策略作为文档化现代化，但不得把该策略报告为已确认原版行为。

## 证据矩阵

| 主张 | 证据等级 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 条目 `0x16BE` / `5822` 与等待参数 `3` | **已确认静态** | `sf2-tech-services-static-v1` | 无函数体字节 parity |
| 两个有序 `sndCom` 操作数身份 | **已确认静态源** | 固定 `music.asm` 加 technical-services 所有者 | 传输、排队、接受、数字编码 |
| 睡眠后测试与非零重试 / 零返回 | **已确认静态源** | 同一受限源时间线 | 标志生产者、生命周期、运行时可达性 |
| 立即零给一个等待；`k` 次非零后零给 `k + 1` | **已确认静态派生控制关系** | 已接受源顺序的直接后果 | 真实经过时长与调度器行为 |
| “当前音乐后播放请求音乐”工程角色 | **推断** | 源符号、操作数与注释 | 声音侧与可听确认 |
| 调用方含义、音频输出、时序、过渡、失败、UI | **未知 / 独立所有者** | audio、interrupt 与调用方合同 | 需要受限证据或刻意设计 |

## 复现

```powershell
uv run sf2 h2 tech-services
uv run sf2 design-contracts test
uv run sf2 verify
```

生成输出保留在忽略的 `local/derived/` 下。没有 ROM、源体转储、音频捕获、音乐载荷、模拟器状态或其他私有/生成工件属于公开合同。
