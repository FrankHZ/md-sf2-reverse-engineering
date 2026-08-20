# 字节复制服务合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对下文描述的受限条目身份、方向事实与源码形状循环/ABI 时间线）
- 现代化：**允许** 在准入域内使用引擎原生缓冲与平台 `memmove` 等价物
- 未知：零长度支持、跨符号边界顺序、地址/长度回绕、畸形或硬件映射范围、调用方可达性、原子性、并发、时序与硬件效果

> 本文件是 [`byte-copy-service.md`](../../contracts/byte-copy-service.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同定义已接受 technical-services 证据支持的最小实现无关字节复制原语。它拥有原版 `CopyBytes` 条目表示的逻辑操作，而不把原版 68000 指令序列变成必需重制实现。

合同刻意比通用内存 API 更窄。它只对已准入正长度、不回绕、原版带符号地址排序具有预期关系的域指定结果等价。不受支持源边界保持显式，而非被归一化为现代库保证。

## 判断边界

**已确认静态：** 已接受 fixture 把 `CopyBytes` 绑定到 ROM 地址 `0x16D6`（`5846`）并记录两个方向事实。固定源保存完整 `d7`、`a0` 与 `a1`、用 `cmpa.l a0,a1` 比较两个地址寄存器、取 `bgt.w @Decrement`、否则执行前向后递增字节循环，并在返回前恢复三个已保存寄存器。递减路径把 `d7.w` 加到两个地址，然后使用前递减字节循环，之后同一恢复/返回接缝。

**已确认受限结果：** 对长度 `1..0x7FFF`（`1..32767`）、非回绕源与目标范围，以及带符号比较按预期排序两个范围起始的地址表示，操作有普通保留重叠的移动结果。向后路径处理目标起始高于源的情况；向前路径处理目标起始等于或低于源的情况。这是受限结果陈述，不是对原版分支的无符号地址重新解读。

**推断：** 源名、小寄存器接口与复用形状暗示通用 memmove 类工具。该工程角色不是玩家面向含义、完整调用方合同，或重制必须暴露同名公开服务的要求。

**未知或排除：** 零长度是否刻意支持；向后路径上 `0x8000` 到 `0xFFFF` 长度的行为；跨符号边界地址对；源或目标回绕；畸形、不可访问或内存映射 I/O 范围；完整调用方准入；中断或 DMA 交互；原子性；周期计数；性能；以及任何调用方可视 UI、图形、音频、持久性或呈现结果。

## 证据合同

本合同只消费以下来自[`sf2-tech-services-static-v1`](../../../../tests/fixtures/h2/tech-services-static-v1.json)的字段：

- `function.CopyBytes`；
- `expected.serviceFacts.byteCopyChoosesBackwardWhenDestinationIsHigher`；
- `expected.serviceFacts.byteCopyChoosesForwardOtherwise`；
- fixture 的 ROM 与固定上游溯源。

fixture 中“destination is higher”措辞只通过原版源条件解读：`cmpa.l a0,a1` 后接带符号大于 `bgt.w`。它不得被静默加宽为任意 32 位地址表示上的无符号比较。

受限源时间线对照固定[`bytecopy.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/bytecopy.asm)检查，并由所属[`technical-services.md`](../../../research/technical-services.md)描述。可执行验证器仍是[`services.py`](../../../../src/sf2tool/h2/services.py)。

本合同**不**消费 `expected.resourceFacts`、`expected.soundDriverFacts`、`expected.inputFacts`、`expected.randomServicesFacts`、`expected.sramFacts`、音乐等待事实或聚合 fixture 的任何其他兄弟字段。

### 精确 research-index 分母

已接受 fixture 直接链接十条研究记录。本合同恰好改变一条的语义关联：

| 记录 | 本合同后的设计所有权 |
| --- | --- |
| `tech.services.byte-copy` | 本合同；注册前当前未关联 |
| `tech.services.music-wait` | 保持未关联且在本合同之外 |
| `tech.services.resource-icon` | 不变：`ui-graphics-asset-data` |
| `tech.services.resource-graphics` | 不变：`text-and-font-system` |
| `tech.services.resource-text-trees` | 不变：`text-and-font-system` |
| `tech.services.resource-title` | 不变：`unused-technical-asset-data` |
| `tech.services.resource-base` | 不变：`unused-technical-asset-data` |
| `tech.services.input` | 不变：`input-system` |
| `tech.services.sram` | 不变：`save-system` |
| `tech.services.thinking-rng` | 不变：`randomness` |

没有调用方记录、资源记录、声音记录、SRAM 记录、输入记录或 RNG 记录仅因共享 fixture 而获得本合同。

## 原版静态操作

### 逻辑输入

源注释与指令识别三个操作输入：

| 源身份 | 逻辑角色 | 已接受边界 |
| --- | --- | --- |
| `a0` | 源起始 | 私有原版地址；重制中的引擎原生源引用 |
| `a1` | 目标起始 | 私有原版地址；重制中的引擎原生目标引用 |
| `d7.w` | 字节长度 | 仅已准入公共域 `1..0x7FFF` |

原版例程保存完整 `d7`、`a0` 与 `a1` 值，使用的指令形式是 `movem.l d7-a1,-(sp)`，并在 `rts` 前恢复相同范围。其临时栈使用在两个分支上平衡。这建立源接缝处那三个完整寄存器值与栈平衡的保留。它不建立 CCR 中性或全寄存器 ABI 保证。

### 带符号方向决定

源执行 32 位地址寄存器比较并取 `bgt`（带符号大于条件）。因此合同保留两个不同陈述：

1. 原版私有保真记录精确保留 `cmpa.l a0,a1; bgt.w @Decrement`；
2. 公开逻辑结果合同只适用于表示的带符号排序给出两个范围起始预期相对顺序之处。

最简单已准入原版地址用例是两个起始都位于同一带符号顺序区域，因此带符号与普通单调排序一致。跨符号边界地址对在本合同之外，即使平台的无符号指针比较会排序它们。

### 前向源时间线

未取带符号大于分支时，源：

1. 从 `d7.w` 减一；
2. 把一字节从当前源复制到当前目标；
3. 后递增两个地址；
4. 用 `dbf` 重复直到已准入计数耗尽；
5. 恢复完整 `d7`、`a0` 与 `a1` 并返回。

对已准入正长度，该路径与前向字节移动结果等价。它保留更低目标重叠，因为字节在后期源位置可被覆盖前读取。相等源与目标引用机械地把每个字节重写为其自身；逻辑目标结果不变。

### 后向源时间线

取带符号大于分支时，源：

1. 把 `d7.w` 通过 `adda.w` 加到两个地址寄存器；
2. 从 `d7.w` 减一；
3. 前递减两个地址并复制一字节；
4. 用 `dbf` 向范围起始重复；
5. 恢复完整 `d7`、`a0` 与 `a1` 并返回。

`adda.w` 符号扩展其字操作数。因此公共准入域在 `0x7FFF` 停止；`0x8000..0xFFFF` 不得在本合同中被描述为正向后向复制长度。在准入域内，从末端移动保留更高目标重叠。

### 不受支持源边界

源在进入任一 `dbf` 循环前减一。本文档刻意不把机械零字边界变成受支持 API 保证。同样，它不定义更广仅前向长度域，因为公共合同意在跨两个方向路径对称。

原版指令模拟器可以自然复现额外边界。重制适配器在独立证据所有者定义其准入与预期结果前不要求接受它们。

## 实现无关导入模型

合规逻辑模型可以使用以下形状：

```text
ByteCopyRequest {
  sourceRef
  destinationRef
  lengthBytes       // admitted: 1..0x7FFF
}

ByteCopyDomain {
  sourceRangeDoesNotWrap
  destinationRangeDoesNotWrap
  signedStartOrderingIsApplicable
  rangesAreReadableAndWritableOrdinaryMemory
}

ByteCopyResult {
  destinationBytes
  sourceRefPreserved
  destinationRefPreserved
  lengthValuePreserved
}

PrivateOriginalProvenance {
  sourceSymbol
  sourcePath
  pinnedUpstreamCommit
  sourceChronologyIdentity
  h1ResolvedEntryAddress
  acceptedRomSha256Provenance
}
```

`sourceRef`、`destinationRef` 与 `lengthBytes` 是逻辑值，不是公开 ROM 指针或 68000 寄存器。未来适配器可以使用 span、切片、数组、原生内存或其他受限缓冲类型。两个逻辑范围之间的别名必须保持可表示，因为重叠是已接受结果合同的一部分。

在 fixture 已接受 ROM 溯源下验证固定源时间线与 H1 解析条目身份后，重制可以用平台 `memmove` 等价物实现该操作。它不需要复现前向/后向微循环、`dbf`、栈帧、寄存器名、大端存储、Mega Drive 地址空间或原版指令时序。该证据不建立 `CopyBytes` 指令体的逐字节 H1/ROM 一致性。

## 公开与私有投影

公开合同可以保留：

- fixture ID 与固定溯源；
- `CopyBytes` 符号与已接受地址元数据；
- 转述形式的原始带符号比较/分支身份；
- 已准入 `1..0x7FFF`、非回绕域；
- 逻辑重叠/结果要求；
- 供未来 H4 测试的小型项目编写合成字节向量。

已接受私有溯源由固定源身份与时间线、H1 解析条目地址、上游 commit 与 fixture 拥有的 ROM 身份/SHA 组成。精确源文本、指令编码或函数体字节比较可以保持为未来更强验证器的私有输入，但它们不是本合同中的已确认一致性事实或 H4 要求。原版地址值与任何完整调用方语料同样保持私有，可分发重制不要求它们。本合同不拥有原版美术、文本、音频、地图或其他内容载荷。

## 跨系统分离

- [`map-palette-data`](../../contracts/map-palette-data.md) 拥有其源码形状 32 字节复制交接与调色板规则，而非通用 `CopyBytes` 微实现。
- [`save-system`](../../contracts/save-system.md) 拥有 `CopyBytesToSram` 与 `CopyBytesFromSram`，包括其交错物理字节/校验和行为。那些是不同例程。
- [`graphics-service-state`](../../contracts/graphics-service-state.md) 拥有解压服务边界，而非通用字节复制行为。
- [`interrupt-dma-and-trap-state`](../../contracts/interrupt-dma-and-trap-state.md) 拥有已接受 DMA、VInt 与 trap 接缝。本合同不建立转移调度或中断行为。
- 窗口、菜单、标题、挂起、实体与图形合同保留其自身调用方事务与可见结果。对 `CopyBytes` 的源调用不把那些语义转移到这里。
- `tech.services.music-wait` 与[`audio-system`](../../contracts/audio-system.md) 保持独立；本合同不拥有等待、邮箱、驱动或可听结果规则。

## H4 验收面

在准入域内，未来 H4 适配器必须对覆盖以下内容的小型合成用例验证目标结果等价：

1. 目标排序低于源的前向非重叠；
2. 需要原版向后结果的高目标重叠；
3. 需要原版向前结果的低目标重叠；
4. 相等源与目标引用；
5. 呈现给适配器的逻辑源引用、目标引用与长度值的保留。

测试向量必须是项目编写且不包含原版游戏载荷。适配器可以调用平台重叠安全移动原语。H4 不得要求观察平台复制方向、指令计数、栈操作、寄存器分配、CCR 状态或时序。

H4 必须拒绝或分类在本合同之外，而非猜测零长度、高于 `0x7FFF` 的长度、回绕范围、跨符号边界原版地址、不可访问内存或内存映射 I/O 的结果。未来刻意更广 API 是现代化决定，需要其自身规定行为；它不是关于原版例程的证据。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 条目身份/地址 | **已确认静态** | `sf2-tech-services-static-v1`、`function.CopyBytes` | 自然调用方与可达性 |
| 方向事实 | **已确认静态** | 同一 fixture、两个具名字节复制字段 | 带符号顺序表示边界 |
| 保存/比较/循环/恢复时间线 | **已确认静态源** | 固定 `bytecopy.asm` 与 technical-services 文章 | CCR、周期时序、不受支持长度/范围 |
| memmove 类工具用途 | **推断** | 源身份与可复用调用形状 | 公开 API 放置与调用方策略 |
| UI、图形、音频、持久性、DMA/VInt、呈现 | **未知 / 独立所有者** | 调用方与子系统合同 | 完整运行时与玩家可见结果 |

## 复现

```powershell
uv run sf2 h2 tech-services
uv run sf2 design-contracts test
uv run sf2 research-index test
```

聚焦 H2 命令必须继续报告 fixture `sf2-tech-services-static-v1`、`CopyBytes` 地址 `5846` 与已接受方向事实。生成报告保留在忽略的 `local/derived/` 下。
