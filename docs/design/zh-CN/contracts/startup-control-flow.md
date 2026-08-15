# 启动控制流合同

- 状态：**已确认静态启动路由与受限初始化范围**
- 证据日期：2026-08-09
- 范围：原版 `Start`、`InitializeSystem`、`InitializeGame`、intro/标题交接与区域门控制面的实现无关重构，不把源静态循环范围转换成硬件时序，也不导入资源、音频、新游戏、输入、呈现、持久性或平台生命周期语义

> 本文件是 [`startup-control-flow.md`](../../contracts/startup-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从源码形状的 `Start` 条目开始。它跟随可选初始配置块、公共 DMA 忙等待、系统与游戏初始化交接、受限 intro/标题返回路线与区域准入分支。它在每个下游子系统交接或源局部终止分支处结束。

- **已确认**：初始配置块被原版 `CTRL1`/`CTRL3` 测试条件跳过；执行时，其已接受静态写入与循环范围是 24 个 VDP 寄存器写入、38 个 Z80 引导字节、65,536 字节 RAM、128 字节 CRAM、80 字节 VSRAM 与四个 PSG 写入；两条配置路线都到达源 DMA 忙等待然后 `InitializeSystem`；系统初始化保留有序 `InitializeVdp`、`InitializeZ80`、`InitializeVdpData`、`InitializeGame` 交接与 19 个维护 VDP 条目；游戏初始化保留有序 `LoadBaseTiles`、`CheckRegion`、`NewGame`、`DisplaySegaLogo` 交接；非零 logo 结果绕过 intro；`GameIntro` 存储其继续指针并在标题交接前于普通辅助返回路径清除它；标题结果分离非零 Witch 交接与零结果 `InitialStack`/`p_Start` 重置路线；`CheckRegion` 用 `0xC0` 掩蔽 `HW_Info`、接受 `0x80`，否则到达其源局部无限循环。
- **推断**：无。硬件生命周期、玩家意图与可见启动含义不从寄存器测试、源注释、分支名或辅助身份推断。
- **未知**：`CTRL1`/`CTRL3` 值是否可靠区分冷启动、软重置或任何其他平台生命周期；重置与 TMSS 变体；Z80 总线与 VDP/DMA 节奏；静态写入循环在不同硬件上产生的状态；产生 logo 或标题结果的精确控制器采样；调试路线可达性；真实硬件区域值与兼容性；被拒绝区域渲染；intro、标题、Witch、logo、淡入、音频与输入时序；持久性；畸形或注入状态；以及玩家可见呈现。

合同描述源码形状控制与元数据，不是现代引擎必须模拟 Mega Drive 启动硬件的要求。原版保真适配器可以暴露该面向硬件面；可移植重制可以用验证平台服务替换它，同时保留已接受路线与交接事实。

## 证据所有者与源审计

`sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`gameflow.py`](../../../../src/sf2tool/h2/gameflow.py)，其带来源解释是[Startup、Main Loop 与 Exploration Core](../../../research/gameflow-core.md)。本合同消费所选 `expected.startupFacts` 与以下五条记录身份：

- `gameflow.start.cold-start`；
- `gameflow.start.system-init`；
- `gameflow.start.game-init`；
- `gameflow.start.intro`；
- `gameflow.start.region`。

fixture 的完整源成员集包含 14 条记录。十三条有直接 `sf2-gameflow-core-static-v1` 证据。精确合同分区是：

- 上文选出的五条启动记录；
- 两条直接 fixture 链接启动记录，保留其既有合同；
- 六条已归[exploration-control-flow](../../contracts/exploration-control-flow.md)的主循环与探索记录；
- 一条仅成员地图块记录，其 H3 证据与[map-exploration](../../contracts/map-exploration.md)关联保持不变。

`gameflow.start.base-tiles` 仍归[UI graphics asset data](../../contracts/ui-graphics-asset-data.md)，而 `gameflow.start.z80-init` 仍归[audio system](../../contracts/audio-system.md)。本合同只把 `LoadBaseTiles` 与 `InitializeZ80` 命名为有序交接身份。它不给任一条研究记录添加本合同。

已接受源审计在固定上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6` 检查这些代表条目：

| 所选记录 | 代表条目 | ROM 地址 |
| --- | --- | ---: |
| `gameflow.start.cold-start` | `Start` | `0x2DE` / 734 |
| `gameflow.start.system-init` | `InitializeSystem` | `0x200` / 512 |
| `gameflow.start.game-init` | `InitializeGame` | `0x70D2` / 28,882 |
| `gameflow.start.intro` | `j_GameIntro` | `0x71C0` / 29,120 |
| `gameflow.start.region` | `CheckRegion` | `0x7EC6` / 32,454 |

审计审查 `gamestart.asm`、`systeminit.asm`、`gameinit.asm`、`gameintro.asm` 与 `regioncheck.asm`。源审查确认下文受限分支与调用时间线。它不把注释或辅助名提升为运行时、硬件或呈现结论。

map-data 聚合与每个 `map.data.*` 表记录在本合同消费证据之外。对该聚合的修正不自动扩展本合同。

## 源码形状初始配置

**已确认静态：** `Start` 首先测试 `CTRL1` 的 longword。该测试为零时，它测试 `CTRL3` 的字。初始配置块只通过两个测试都留下零结果的源路径执行；两个测试值任一非零到达 `@SkipSetup`。

这是分支事实，不是平台检测器合同。两个地址保留为源身份。其值不被重新定义为可靠冷启动、重置、主机型号或硬件健康信号。

初始配置块执行时，已接受 fixture 与源循环建立这些精确静态范围：

| 配置操作 | 已接受范围 | 证据含义 |
| --- | ---: | --- |
| 初始 VDP 寄存器写入 | 24 | 源循环迭代 |
| Z80 引导复制 | 38 字节 | 源字节复制迭代 |
| 68000 RAM 清除 | 65,536 字节 | 源 longword 循环范围以字节表示 |
| CRAM 清除 | 128 字节 | 源 longword 循环范围以字节表示 |
| VSRAM 清除 | 80 字节 | 源 longword 循环范围以字节表示 |
| PSG 写入 | 4 | 源字节写入迭代 |

这些不是经过周期、设备完成保证、音频静音时长、可见消隐或每个主机与模拟器上相同效果的证明。公开合同保留计数与操作身份，而非原版引导字节或其他私有载荷。

条件执行块后，两条路径到达共享继续。源读取 VDP 控制端口、在已接受 DMA 忙位测试保持非零时重复，然后分支到 `InitializeSystem`。分支顺序是已确认静态；轮询节奏、硬件完成与失败行为保持 **未知**。

## 系统初始化交接

**已确认静态：** `InitializeSystem` 保留该精确源顺序：

1. 调用 `InitializeVdp`；
2. 调用 `InitializeZ80`；
3. 调用 `InitializeVdpData`；
4. 尾跳转到 `InitializeGame`。

`InitializeVdp` 源循环消费 19 个维护 VDP 初始化条目。这是表与循环基数，不是可移植显示规范或可见状态声称。

`InitializeZ80` 在本合同只是交接身份。生成声驱载荷、复制长度解读、总线/重置协议、活跃 Z80 状态、首命令、可听结果与失败行为保持[audio-system](../../contracts/audio-system.md)或 **未知**。

`InitializeVdpData` 同样是交接身份。队列布局、滚动缓冲、调色板、精灵表、DMA 处理、控制器端口配置、完成与渲染在本合同之外。现代实现可以用一个验证服务替换三个平台辅助，前提是其兼容轨迹能在请求原版路线 parity 时复现已接受有序交接。

## 游戏初始化交接

**已确认静态：** `InitializeGame` 保留该精确顶层调用顺序：

1. `LoadBaseTiles`；
2. `CheckRegion`；
3. `NewGame`；
4. `DisplaySegaLogo`。

本合同不消费 `baseTileCount` 或 `baseTileCompressionMode`。源操作数 `4096`、压缩资源身份、解码器行为、转移形式、目标与渲染结果保持[UI graphics asset data](../../contracts/ui-graphics-asset-data.md)及其专用证据。`LoadBaseTiles` 只是上文命名的有序交接。

[new-game state initialization 合同](../../contracts/new-game-state-initialization.md) 拥有已接受 `NewGame` 变更及其内部顺序。本合同只拥有交接发生在区域辅助返回后、logo 交接前的事实。它不定义存档创建、战役状态、持久性或玩家可见新游戏行为。

`DisplaySegaLogo` 返回后，已接受源分支把非零结果直接发送到 `AfterGameIntro`。零结果继续到调试切换与 intro 路由源。[special-screen control-flow 合同](../../contracts/special-screen-control-flow.md) 拥有 logo 内部及其受限输入序列面。本合同不声称 Start 按下或任何精确输入采样如何创建结果，也不声称分支在特定帧可见。

其余调试模式分支不被提升为完整调试合同。其自然可达性、输入时序、地图/战斗测试语义与下游状态为 **未知** 或属于独立所有者。

## Intro 与标题返回路由

**已确认静态：** `j_GameIntro` 分支到 `GameIntro` 块。该块：

1. 把当前栈指针存储到源备份位置；
2. 把 `AfterGameIntro` 存储到 intro 继续指针位置；
3. 交接给 intro/结束过场辅助；
4. 辅助经普通源路径返回后清除继续指针；
5. 稍后到达 `StartTitleScreen` 交接。

已接受时间线不证明过场辅助在每条路线如何使用指针、指针何时对中断可见、alternate 跳转是否绕过清除或玩家看到什么。那些保持独立运行时问题。

`StartTitleScreen` 返回后，源区分两条路线：

- 非零结果分支到 `StartWitchScreen`；
- 零结果提高源中断掩码、从 `InitialStack` 重载栈、通过 `p_Start` 加载目标并跳转到它。

第二条路径保留为源码形状启动向量重置路线，不是硬件重置或完整平台重新初始化的证明。标题循环、输入轮询、Witch 准入、存档动作、淡入、音乐与可见过渡行为保持[special-screen-control-flow](../../contracts/special-screen-control-flow.md)、其他专用合同或 **未知**。

## 区域准入分支

**已确认静态：** `CheckRegion` 读取 `HW_Info`、应用 `0xC0`、把掩蔽字节与 `0x80` 比较并在相等时返回。非相等分支到达源警告工作然后其局部无限循环。

已接受合同只是掩码、已接受比较值、返回对局部循环拆分与代表辅助身份。它不定义：

- 所有真实或模拟硬件产生的值集；
- 通用区域分类法；
- 与其他版本或主机修订的兼容性；
- 警告文本、字体、布局、颜色、转移完成或渲染结果；
- 恢复、超时、可访问性、本地化或用户面向错误策略。

现代引擎可以用显式平台/内容兼容检查替换原版门。任何刻意变更必须记录为现代化，而非关于原版硬件行为的证据。

## 跨系统分离

启动路线是编排器。具名交接不转移被调方所有权：

- [UI graphics asset data](../../contracts/ui-graphics-asset-data.md) 拥有基础瓦片资源与转移元数据；
- [audio-system](../../contracts/audio-system.md) 拥有声驱与活跃音频边界；
- [new-game state initialization](../../contracts/new-game-state-initialization.md) 拥有已接受 `NewGame` 状态事实；
- [special-screen control flow](../../contracts/special-screen-control-flow.md) 拥有 logo、标题与 Witch 内部；
- [input-system](../../contracts/input-system.md) 拥有已接受控制器采样与等待辅助行为；
- [exploration-control-flow](../../contracts/exploration-control-flow.md) 拥有后续主循环与探索面。

地图数据表、地图加载、工作布局变更、战斗结果、存档持久性、对话、菜单、呈现资源与故事含义保持本合同之外。没有相邻研究记录因隐含获得本合同。

## 实现无关控制模型

最小逻辑模型是控制与溯源投影，不是硬件模拟器：

```text
StartupControlPlan
  sourceIdentity
  sourceCommit
  selectedEntries:
    Start
    InitializeSystem
    InitializeGame
    j_GameIntro
    CheckRegion

  initialSetupAdmission:
    firstTest: CTRL1 longword
    secondTestWhenFirstZero: CTRL3 word
    executeSetupWhenBothZero: true
    otherSourcePathsSkipSetup: true

  initialSetupExtents:
    vdpRegisterWrites: 24
    z80BootstrapBytes: 38
    ramClearBytes: 65536
    cramClearBytes: 128
    vsramClearBytes: 80
    psgWrites: 4

  commonContinuation:
    pollVdpDmaBusyBit
    handoff: InitializeSystem

  systemHandoffs:
    - InitializeVdp
    - InitializeZ80
    - InitializeVdpData
    - InitializeGame
    maintainedVdpEntryCount: 19

  gameHandoffs:
    - LoadBaseTiles
    - CheckRegion
    - NewGame
    - DisplaySegaLogo

  logoReturn:
    nonzero: AfterGameIntro
    zero: debugAndIntroRouting

  introRoute:
    continuationTarget: AfterGameIntro
    storeBeforeIntroHandoff: true
    clearOnOrdinaryReturn: true
    titleHandoff: StartTitleScreen

  titleReturn:
    nonzero: StartWitchScreen
    zero:
      reloadStackFrom: InitialStack
      jumpTargetFrom: p_Start

  regionGate:
    sourceByte: HW_Info
    mask: 0xC0
    acceptedValue: 0x80
    equalRoute: return
    otherRoute: sourceLocalInfiniteLoop
```

该模型保留身份、有序交接、分支谓词与循环范围。它不存储原版代码、引导字节、区域警告内容、图形、音频或其他版权载荷。公开 fixture 与报告只保留元数据与合成测试状态。

模型刻意不包含基础瓦片计数、压缩模式、声驱长度、输入帧、硬件时钟、渲染画面、存档状态或战役状态。那些事实要么有另一个所有者，要么保持 **未知**。

## 原版保真与现代化

原版保真模式保留源码形状可选配置分支、已接受静态范围、公共 DMA 等待交接、系统与游戏调用顺序、非零 logo 绕过、intro 继续指针时间线、标题结果拆分与区域掩码/比较/终止拆分。硬件时序与可见结果保持显式独立测试，而非静默假设。

现代引擎可以通过主机 API 初始化图形与音频、省略硬件引导工作、替换区域门、使用类型化场景结果，并通过状态机路由标题选择。此类变更是允许设计选择。兼容适配器仍必须能发出或验证已接受源面向路线事实，刻意偏差必须分别记录。

## H4 验收门

未来重制启动控制适配器只在以下情况通过本合同：

1. 保留源码形状 `CTRL1`/`CTRL3` 条件配置准入，而不把两个值呈现为通用冷启动或重置检测器；
2. 原版保真元数据保留精确 24、38、65,536、128、80 与四静态写入/循环范围，而不把它们转换成时长、完成或可见结果声称；
3. 两条初始路线都到达公共 DMA 忙等待与 `InitializeSystem` 交接，而硬件节奏与失败行为保持分别测试或 **未知**；
4. 系统交接顺序与 19 个维护 VDP 条目保持可复现，`InitializeZ80` 在本合同只保留为有序身份；
5. 游戏交接顺序保持可复现，而基础瓦片数据/转移与 `NewGame` 变更保持其专用所有者；
6. 非零 logo 结果绕过 intro，而不把控制器采样、logo 内部或可见时序指定给本合同；
7. intro 继续指针存储/普通返回清除、标题交接、非零 Witch 路线与零结果 `InitialStack`/`p_Start` 路线保持不同且有序；
8. 区域掩码 `0xC0`、已接受值 `0x80`、相等返回与非相等局部循环路线保持可复现，而不泛化硬件兼容性或警告呈现；
9. 精确关联边界保持五条所选启动记录；base-tiles、Z80 init、六条探索记录、地图块成员记录、map-data 与所有其他兄弟记录保持语义不变；
10. 公开工件只包含结构元数据与合成状态，绝不包含 ROM、引导、图形、音频、文本、轨迹、模拟器状态或存档载荷。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 条件初始配置与精确写入/循环范围 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 硬件生命周期含义、节奏、完成、可见状态 |
| 公共 DMA 忙等待与系统交接 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 硬件时序、锁死与失败行为 |
| 系统顺序与 19 个维护 VDP 条目 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 驱动数据、队列、总线/重置、显示/音频结果 |
| 游戏初始化交接顺序与 logo 返回绕过 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 资源载荷、NewGame 变更、输入生成、调试路线 |
| intro 继续指针与标题结果路线 | **已确认静态加固定源时间线** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 标题/Witch 内部、可达性、重置硬件效果、呈现 |
| 区域掩码、已接受值、返回/局部循环拆分 | **已确认静态** | `sf2-gameflow-core-static-v1`（[`gameflow-core-static-v1.json`](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)） | 真实硬件域、兼容性、警告输出、恢复 |
| base-tile 与 Z80 启动记录 | **独立所有者已确认静态** | [UI graphics asset data](../../contracts/ui-graphics-asset-data.md)与[audio-system](../../contracts/audio-system.md) | 此处仅交接身份；无新关联 |
| 运行时、硬件、持久性、输入、调试、资源、音频与呈现含义 | **独立所有者 / 未知** | 相邻合同与未来运行时工作 | 不得从静态控制推断完整启动体验 |

## 复现

```powershell
uv run sf2 h2 gameflow-core
uv run sf2 design-contracts test
uv run sf2 verify
```

生成 JSON 保留在忽略的 `local/derived/gameflow-core-static.json` 下。
