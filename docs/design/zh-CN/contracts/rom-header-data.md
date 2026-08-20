# ROM 头数据合同

- **已确认原版结构：** 64 条目向量表边界、已接受 HInt/VInt 等级锚点与具名 trap 范围摘要，以及下文列出的精确产品、存储校验和、ROM 末端、声明 SRAM 与区域元数据。
- **推断原版行为：** 只有源头字段标签暗示的平台面向意图；本合同不从那些标签提升任何启动、中断、trap、存档、区域或硬件行为。
- **未知原版行为：** 完整逐向量目标映射与 ABI、启动/重置使用、中断与 trap 运行时行为、校验和生成或平台接受、SRAM 启用与持久性、区域兼容性，以及所有玩家可见结果。
- 重制状态：实现无关 Phase 3 溯源/导入合同；尚未选择模拟器头 API、持久性后端、区域策略或可分发原版载荷。
- 证据日期：2026-08-12
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`rom-header-data.md`](../../contracts/rom-header-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同拥有两个相关但不同的静态结构：

1. 控制台头之前 64 条目向量表的元数据；
2. 可执行所有者接受的所选字面控制台头字段。

可执行所有者是 fixture id `sf2-remaining-core-static-v1`，位于 [`tests/fixtures/h2/remaining-core-static-v1.json`](../../../../tests/fixtures/h2/remaining-core-static-v1.json)。本合同只消费 `expected.headerFacts`。研究所有者是[ROM Header、Window Engine 与 Special Debug Flows](../../../research/remaining-core.md)，受限源所有者是固定基线处的 `code/romheader.asm`。

research-index 溯源记录是 `core.rom-header`，绑定到源符号 `InitialStack`，其 ROM 地址为 `0`。该符号/地址对只是证据锚点。它不建立初始栈值、栈配置、重置语义或任何调用方可视行为。

向量表与控制台头共享一个源文件与一个合同，但它们必须保持独立逻辑结构。消费者不能用控制台头元数据替换向量表摘要，也不能从产品、内存范围、校验和或区域字段推断向量目标。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-remaining-core-static-v1
SHA256 E786D25EF9EABCC12A997227AF4B3CA32E29F2D79E4DB77EE45417ADB1B39224
Files 5 / WindowSlots 8 / DebugActions 7 / RuntimeQuestions 2 / PASS
```

fixture 直接绑定恰好五条 research-index 记录：

| 记录 | 既有设计所有者 |
| --- | --- |
| `core.rom-header` | 无；本合同精确未来关联 |
| `core.window-engine` | [window-system](../../contracts/window-system.md) |
| `debug.battle-test` | [debug-control-flow](../../contracts/debug-control-flow.md) |
| `debug.configuration` | [debug-control-flow](../../contracts/debug-control-flow.md) |
| `debug.battle-actions` | [debug-control-flow](../../contracts/debug-control-flow.md) |

只有 `core.rom-header` 未关联。其他四条记录保留其既有合同与语义。本合同不因那些段共享 fixture 而消费 `expected.windowFacts` 或 `expected.debugFacts`。

已接受头事实与仓库[README](../../../README.md)中总结的独立 H0 身份轨道一致。该一致是佐证，不是合并证据所有者：本合同中的存储头校验和保持与 H0 独立计算校验和验证不同。

## 向量表元数据

**已确认静态：** 源码在控制台头之前恰好包含 64 个 longword 向量条目。在已接受摘要内：

- 水平中断锚点在中断等级 4；
- 垂直中断锚点在中断等级 6；
- `namedTrapRange` 恰好是 `[0, 9]`。

`namedTrapRange` 是范围摘要，不是该区间内每个 trap 槽都有不同具名实现、已证明传输 ABI 或已接受运行时路径的声称。可执行 fixture 不发布完整有序向量目标表。因此本合同不要求或声称：

- 64 个条目中每个的精确目标身份；
- 每个条目非零、唯一、可调用或可达；
- 逐向量寄存器、栈、返回或条件码行为；
- 中断优先级、节奏、确认或设备行为；
- 精确逐 trap 服务映射或内联操作数解码。

计数与三个已接受摘要字段对公开可追溯性足够。私有源审计可以检查有序向量字，但那些字不成为公开 fixture 载荷或本合同下的 H4 一致性要求。

## 控制台头元数据

**已确认静态：** 所选字面头字段是：

| 字段 | 已接受值 | 保留边界 |
| --- | ---: | --- |
| 产品码 | `GM MK-1315 -00` | 精确存储标识符；无平台接受声称 |
| 存储校验和 | `35105`（`0x8921`） | 仅存储字；无算法或验证声称 |
| ROM 末端地址 | `2097151`（`0x1FFFFF`） | 仅声明末端值 |
| SRAM 起始地址 | `2097153`（`0x200001`） | 声明奇数起始精确保留 |
| SRAM 末端地址 | `2113535`（`0x203FFF`） | 仅声明末端值 |
| 区域码 | `U` | 精确存储码；无兼容性或玩家区域声称 |

SRAM 区间是头元数据。其奇数起始不得被实现的首选地址归一化、舍入、修正或静默替换。保留它不证明 SRAM 被启用、可访问、以特定方式字节或字寻址、持久、供电或被任何已接受存档路径使用。

同样，存储校验和不是校验和算法。它不证明原版值如何生成、平台何时验证它、哪些字节参与或失配后发生什么。H0 可以独立计算相同值而不改变本合同更窄的存储字段边界。

## 实现无关导入模型

最小公开模型保持向量元数据与控制台头元数据分离：

```text
RomHeaderContract {
  provenance: RomHeaderProvenance
  vectorTable: VectorTableMetadata
  consoleHeader: ConsoleHeaderMetadata
}

RomHeaderProvenance {
  fixtureId = "sf2-remaining-core-static-v1"
  sourcePath = "code/romheader.asm"
  sourceAnchorSymbol = "InitialStack"
  sourceAnchorAddress = 0
  upstreamCommit
  romIdentityReference
}

VectorTableMetadata {
  entryCount = 64
  horizontalInterruptLevel = 4
  verticalInterruptLevel = 6
  namedTrapRange = [0, 9]
}

ConsoleHeaderMetadata {
  productCode = "GM MK-1315 -00"
  storedChecksum = 35105
  romEndAddress = 2097151
  declaredSramStartAddress = 2097153
  declaredSramEndAddress = 2113535
  regionCode = "U"
}
```

这是溯源与导入验证模型，不是硬件抽象。它刻意没有栈状态、向量目标集合、trap 分发器、SRAM 设备、存档后端、校验和服务、区域开关或呈现状态。

公开投影可以保留上述精确受限元数据、fixture 身份、固定源溯源与通过/失败诊断。它不得再分发原始向量字、原始控制台头、标题字符串、版权字符串、备忘录字段或其他原版头载荷。私有验证可以从用户拥有的原版输入读取那些字节而不发布它们。

## 跨系统分离

本合同不拥有：

- `InitialStack`/`p_Start` 启动与重置时间线，仍归[startup-control-flow](../../contracts/startup-control-flow.md)；
- 中断注册、VInt/HInt 处理、DMA 队列、淡入控制、设备面向意图或 trap 运行时接缝，仍归[interrupt-dma-and-trap-state](../../contracts/interrupt-dma-and-trap-state.md)；
- 标志存储与受限标志 trap 分组，仍归[global-flag-state](../../contracts/global-flag-state.md)；
- 声音命令传输与驱动状态，仍归[audio-system](../../contracts/audio-system.md)；
- 存档布局、初始化、SRAM 持久性、挂起行为或断电恢复，仍归[save-system](../../contracts/save-system.md)；
- 区域准入、平台兼容性、校验和拒绝、模拟器策略或硬件时序；
- 来自共享 remaining-core fixture 其他段的窗口或调试语义。

实现可以在使用不同运行时抽象的同时消费此处的元数据。只有它不把运行时行为重新标记为本静态合同提供的证据时，该选择才合规。

## 判断边界

### 已确认

- 通过 `sf2-remaining-core-static-v1` 与 `core.rom-header` 的 fixture/源溯源；
- 控制台头之前 64 个向量条目；
- HInt 等级 4、VInt 等级 6 与 `namedTrapRange=[0,9]` 作为受限摘要字段；
- 精确产品码、存储校验和、ROM 末端、声明 SRAM 范围与区域码值；
- 公开元数据/私有原始载荷分离。

### 推断

- 源标签指示平台面向向量与头意图，但不从该意图提升任何运行时行为。

### 未知

- 完整逐向量目标图、精确 trap 映射、ABI、可达性与运行时结果；
- 启动/重置栈行为与所有向量条目和活跃执行之间的关系；
- 中断节奏、优先级效果、DMA/设备行为与可见或可听结果；
- 校验和生成、验证、拒绝与兼容性行为；
- SRAM 启用、可访问性、持久性、硬件行为与存档系统集成；
- 区域兼容性、本地化选择与任何玩家可见结果；
- 畸形、修改、扩展或替换头准入策略。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 识别 fixture `sf2-remaining-core-static-v1`、固定上游 commit、源路径与 `InitialStack` 地址零溯源锚点，而不把该锚点解读为栈行为；
2. 把向量表元数据与控制台头元数据表示为不同结构；
3. 复现向量条目计数 64、HInt 等级 4、VInt 等级 6 与具名 trap 范围 `[0,9]`，而不发明完整逐向量映射、非 null 规则或 ABI；
4. 精确保留产品码 `GM MK-1315 -00`；
5. 把存储校验和 35105 与任何独立计算校验和结果或校验和算法分开保留；
6. 精确保留 ROM 末端 2097151 与声明 SRAM 范围 2097153 到 2113535（包括奇数起始），而不把它们当作已证明运行时内存行为；
7. 保留区域码 `U` 而不指定兼容性或本地化行为；
8. 通过合成元数据测试检测缺失字段、变更字面量、结构混淆、归一化地址与发明向量语义；
9. 把原始向量字、原始头字节、标题/版权字符串与其他原版载荷保持出公开 fixture 与报告；
10. 通过其独立所有者报告启动、中断、trap、校验和、SRAM、存档、区域、硬件与呈现行为，或作为 **未知**。

H4 验收不要求重制在内部复现原版硬件头。它要求私有导入/溯源适配器忠实地保留已接受元数据，并显式报告刻意替换或省略。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 溯源锚点 | **已确认静态** | `sf2-remaining-core-static-v1`；[fixture](../../../../tests/fixtures/h2/remaining-core-static-v1.json) | `InitialStack` / 地址 0 仅溯源；无栈或重置语义 |
| 向量表摘要 | **已确认静态** | 同一 fixture；[remaining-core 研究](../../../research/remaining-core.md) | 64 条目、HInt 4、VInt 6、范围 `[0,9]`；无完整映射、ABI 或可达性 |
| 控制台头字面量 | **已确认静态** | 同一 fixture | 精确产品/校验和/ROM 末端/SRAM 范围/区域字段；无硬件或平台行为 |
| 存储校验和佐证 | 独立所有者证据 | 仓库[H0 摘要](../../../README.md) | 计算验证保持与存储字段所有权和算法语义不同 |
| 启动/重置控制 | 独立所有者证据 | [startup-control-flow](../../contracts/startup-control-flow.md) | `InitialStack` 与 `p_Start` 活跃时间线此处不复制 |
| 中断与 trap 行为 | 独立所有者证据 / **未知** | [interrupt 合同](../../contracts/interrupt-dma-and-trap-state.md)；[flag 合同](../../contracts/global-flag-state.md) | 已接受受限接缝留在其所有者；未观察映射、ABI、时序与结果保持开放 |
| SRAM/存档行为 | 独立所有者证据 / **未知** | [save-system](../../contracts/save-system.md) | 声明范围被保留；启用、持久性、断电与集成不被声称 |
| 公开载荷 | **未知** / 排除 | 无消费再分发所有者 | 仅元数据与溯源；原始版权头/标题载荷保持私有 |

## 开放问题

1. 未来平台适配器合同是否应在不把其原始目标加入公开 fixture 的情况下验证私有读取的完整向量表？
2. 可分发导入工具应支持哪种校验和算法与平台接受规则，以及如何报告刻意修改 ROM 值？
3. 重制在使用平台中性存档后端时，应如何把声明 SRAM 范围保留为溯源？

## 复现

```powershell
uv run sf2 h2 remaining-core
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/remaining-core-static.json` 下。公开验收使用 fixture 元数据与溯源，而非原始原版头载荷。
