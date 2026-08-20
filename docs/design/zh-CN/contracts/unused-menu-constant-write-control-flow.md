# 未使用菜单常量写入控制流合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对受限 `sub_15268` 身份、源区间、常量写入顺序、两个不相交已准入内存范围与十三次迭代 `DBF` 循环）
- 现代化：**允许** 在独立已接受调用方要求生产端点前省略它，或在私有兼容适配器中使用引擎原生类型化写入关系
- 未知：自然或计算准入、源词汇含义、RAM 用途、后期消费者、寄存器/CCR/栈行为、中断可见性、时序与玩家可见输出

> 本文件是 [`unused-menu-constant-write-control-flow.md`](../../contracts/unused-menu-constant-write-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同保留已接受 common-menu 清单中最小的完整未关联源文件：`sub_15268`，位于 `unusedsub_15268.asm`。例程把同一源常量在隔离负偏移处写一次，然后通过有序后递增循环写十三次，并返回。

上游路径与注释把函数称为“unused”与“menu engine function”。那些标签是档案词汇，不是用途、死代码状态、自然不可达性或可见菜单行为的证明。本合同只拥有受限静态写入算法及其溯源。

## 判断边界

**已确认静态：** [`sf2-common-menus-static-v1`](../../../../tests/fixtures/h2/common-menus-static-v1.json) 把 `sub_15268` 绑定到 ROM 地址 `0x15268`（`86632`）。`code/common/menus/unusedsub_15268.asm` 的已接受 H2 清单行记录：

- SHA-256 `349BAB7EDDEC2739965C898EEB4415CEB3AF934EC8835CBE8A0E89EF42A054D4`；
- 22 条源行与六条语句；
- 两个全局标签 `sub_15268` 与 `loc_15278`；
- 零局部标签、零传出直接调用与零间接调用站点。

H1 列表把完整函数放在排他区间 `0x15268..0x15284`（28 字节跨度）。独立 `YesNoPrompt` 源在 `0x15284` 开始。该相邻既不是调用也不是 fallthrough：受限例程以 `rts` 结束。

固定源直接审查确认一个隔离常量写入、一个十三体循环与无调用或交接。完整源树 token 搜索只在定义与结束注释处找到 `sub_15268`。这是受限符号出现清单，不是对间接、计算、原始地址、调试或外部注入调用的反证。

**推断：** 只有上游 `unused` 与 `menu engine` 词汇。常量、目标符号与位置不建立预期布局内容、清除行为、初始化、呈现或产品暴露。

**未知或排除：** 自然调用方准入；间接或计算可达性；调用方状态；例程为何存在；`byte_FFCC86` 的语义含义；后期读取器是否观察任何写入；两个已准入范围之外的内存所有权；`a0` 与 `d7` 后状态作为可移植 ABI；CCR 效果；栈与返回地址行为；中断或原子性；硬件总线效果；VInt、DMA、VDP 或渲染行为；可见瓦片、窗口或菜单；时序；持久性；畸形调用；以及 source、H1 与 ROM 之间的指令体一致性。

## 证据合同

本合同只消费：

- `function.sub_15268`，来自[`sf2-common-menus-static-v1`](../../../../tests/fixtures/h2/common-menus-static-v1.json)；
- 已接受 H2 生成所有者行身份、源 hash、计数与无传出调用清单；
- `upstreamCommit` 与 `romSha256` 溯源；
- 固定[`unusedsub_15268.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/unusedsub_15268.asm)中的受限时间线；
- H1 条目、指令边界与排他结束身份。

其可执行所有者是[`menus.py`](../../../../src/sf2tool/h2/menus.py)，其文章所有者是[common-menu 研究](../../../research/common-menus.md)。

合同不消费 fixture 的任何 `expected.menuFacts`、根 `menuFacts` 或 `alternateSource` 字段。特别是它不消费菱形菜单、yes/no 提示、立绘、服务、物品、商店、成员画面、时序、输入或呈现事实。它也不消费 H3 测试夹具或 UI-graphics、UI-layout、window、graphics-service、interrupt 或 technical-interface 测试夹具。

所选 H2 所有者解析代表条目，并在固定 ROM 溯源下验证已接受源清单。它不逐字节比较该函数完整编码体与 H1 与 ROM。完整指令编码可以由未来更强验证器私有检查，但此处不是已确认，也不是 H4 要求。

### 精确 fixture 链接分母

新 H2 连接包含 42 条 research-index 记录：

- 41 条记录携带直接 `sf2-common-menus-static-v1` 证据；
- `menus.load-portrait` 是唯一仅成员记录，带专用立绘证据与[`portrait-window-state`](../../contracts/portrait-window-state.md)所有权。

41 条直接绑定精确分化为：

| 分区 | 计数 | 本合同后的所有权 |
| --- | ---: | --- |
| `menus.unused-15268` | 1 | 仅本合同 |
| 已关联直接记录 | 11 | 不变 |
| 其他未关联直接记录 | 29 | 保持未关联 |

十一条既有直接关联精确保持：

- `menus.blacksmith-actions`、`menus.caravan-actions`、`menus.church-actions` 与 `menus.shop-actions` 归[`service-interactions`](../../contracts/service-interactions.md)；
- `menus.ally-portrait`、`menus.combatant-portrait`、`menus.name-under-portrait`、`menus.portrait-functions` 与 `menus.portrait-window` 归[`portrait-window-state`](../../contracts/portrait-window-state.md)；
- `menus.diamond` 与 `menus.tile-pointers` 归[`ui-graphics-asset-data`](../../contracts/ui-graphics-asset-data.md)。

`menus.unused-12606`、`menus.unused-156a8`、`menus.yes-no-prompt` 与其他 26 条未关联直接兄弟保持未关联。最终语义关联差异恰好是：

```text
menus.unused-15268
  + docs/design/contracts/unused-menu-constant-write-control-flow.md
```

## 源码形状写入关系

源建立该精确顺序：

1. 把源符号 `byte_FFCC86`（`0xFFCC86`）加载进 `a0`；
2. 写 longword `0xC020C020` 到 `-0x50(a0)`，即半开字节范围 `0xFFCC36..0xFFCC3A`；
3. 执行 `moveq #0xC,d7`；
4. 在 `loc_15278`，把 longword `0xC020C020` 写到 `(a0)+`；
5. 执行 `dbf d7,loc_15278`；
6. 循环终止后以 `rts` 返回。

循环体在每次 `DBF` 测试前运行。因此低字从 `0x000C` 开始产生十三次体执行，索引 `i=0..12`，而非十二次。循环在 `0xFFCC86 + 4*i` 写入，覆盖半开范围 `0xFFCC86..0xFFCCBA`。

因此完整已准入内存写入投影是：

| 区域 | Longword | 字节 | 存储值 |
| --- | ---: | ---: | --- |
| 隔离 `base-0x50` 范围 | 1 | 4 | `0xC020C020` |
| 有序 `base+4*i`、`i=0..12` 范围 | 13 | 52 | `0xC020C020` |
| 总计 | 14 | 56 | 同一源常量 |

两个范围不相交。源不写中间字节。这是精确源内存关系；它不是常量代表空白瓦片、窗口布局、清除值、调色板或任何其他玩家可见概念的证据。

## 实现无关模型

私有证据/导入模型可以保留：

```text
UnusedMenuConstantWriteEvidence
  identity
    sourceSymbol = sub_15268
    sourcePath
    sourceSha256
    h1EntryAddress = 0x15268
    exclusiveEndAddress = 0x15284
    upstreamCommit
    acceptedRomSha256Provenance

  sourceInventory
    sourceLineCount = 22
    statementCount = 6
    globalLabels = [sub_15268, loc_15278]
    localLabelCount = 0
    outgoingDirectCallCount = 0
    indirectCallSiteCount = 0
    externalSymbolicCallerOccurrenceCount = 0

  admittedMemoryProjection
    baseIdentity = byte_FFCC86
    baseSourceAddress = 0xFFCC86
    constantLongword = 0xC020C020
    isolatedWriteOffset = -0x50
    loopInitialLowWord = 0x000C
    loopBodyCount = 13
    loopStrideBytes = 4
    orderedLoopIndexes = 0..12
    totalLongwordWrites = 14
    totalWrittenBytes = 56
```

该记法是溯源与兼容模型，不是必需运行时类、公开内存布局或 68000 模拟器 API。在 fixture 已接受 ROM 溯源下私有源与 H1 身份验证后，重制可以用类型化数组、列表填充或仅轨迹档案适配器表达已准入写入结果。

它不需要在生产代码中复现 Mega Drive 地址空间、`a0`、`d7`、`DBF`、大端 longword 存储或原版指令序列。精确指令体一致性不是本合同已接受前提。

## 公开与私有投影

公开合同可以保留：

- fixture ID、源路径/hash、上游 commit 与 ROM 身份溯源；
- 源符号、条目、排他结束、物理跨度与受限所有者行计数；
- `byte_FFCC86`、所选源地址、相对偏移、常量身份、循环计数/步长、两个半开范围与聚合 14 longword/56 字节总计；
- 受限零传出调用与零外部符号调用方摘要；
- 精确 42/41 关联分区。

完整源与 H1 体、编码指令字节、ROM 摘录、周围 RAM 内容、模拟器状态、轨迹、捕获与任何后期读取数据保持私有。公开工件不得发布原版 UI、文本、图形、音频或其他版权载荷。

## 原版保真与现代化

原版保真证据保留以下区分：

- 一个隔离写入与十三个连续循环写入；
- `moveq #0xC` 与十三次体执行；
- 源常量与任何未证明视觉含义；
- 零符号调用方与通用运行时不可达性；
- 已准入内存投影与完整机器状态。

现代引擎可以在独立已接受调用方合同要求前省略、擦除或内联生产端点。它可以为档案测试保留私有兼容接缝。如果此类接缝存在，合成已准入内存可以验证两个精确写入范围、顺序、计数与常量。

任何“两个范围之外已准入字节不变”的声称只限于合成已准入内存投影。它永远不是全机器状态不变量。原版源显式变更 `a0` 与 `d7` 并通过 `rts` 返回；寄存器后状态、CCR、栈、返回地址机制与并发机器状态保持未知与排除。

## H4 验收面

未来私有兼容适配器或档案导入器满足本合同时：

1. fixture、源路径/hash、上游 commit、已接受 ROM 溯源、条目与排他结束保持可追溯；
2. 源清单保留 22 行、六条语句、两个全局标签、零局部标签、零传出直接调用与零间接调用站点；
3. 已准入内存关系保留一次 `base-0x50` 写入后接十三次有序 `base+4*i` 写入，其中 `i=0..12`，全部使用 `0xC020C020`；
4. 测试保持两个范围不相交并精确计数 14 longword/56 已准入字节；
5. 如果实现合成兼容检查，它只断言其已准入内存投影中两个范围之外的字节不变，绝不断言寄存器、CCR、栈、时序、中断或完整机器状态不变；
6. 引擎原生实现可以替换源循环，同时保留已准入写入结果与溯源，而不复现 Mega Drive 地址或指令机制；
7. 在独立已接受调用方合同闭合准入与可达性前，生产端点省略保持允许；
8. 零符号调用方出现不被报告为死代码或通用不可达性；
9. `YesNoPrompt` 相邻不创造调用、fallthrough、数据依赖或所有权声称；
10. 只注册 `menus.unused-15268`，而全部 41 条兄弟/成员记录保持其既有关联状态。

## 跨系统分离

- [`window-system`](../../contracts/window-system.md) 保留窗口分配、移动、更新与生命周期行为。本合同不把任一写入范围识别为窗口缓冲。
- [`ui-layout-data`](../../contracts/ui-layout-data.md) 保留规范 UI 布局资源及其私有导入保真。此处不消费布局载荷或赋值。
- [`graphics-service-state`](../../contracts/graphics-service-state.md)与[`interrupt-dma-and-trap-state`](../../contracts/interrupt-dma-and-trap-state.md) 保留图形服务与 VInt/DMA/转移边界。该源不发出此类请求。
- `menus.yes-no-prompt` 保持未关联。其独立源在 `0x15284` 开始；与 `rts` 终止例程的相邻不建立行为接缝。
- `menus.unused-12606` 与 `menus.unused-156a8` 保持未关联。相似上游名不建立共享行为。
- 文本、声音、输入、UI 呈现、持久性与硬件时序保持其既有所有者或 未知。

## 证据矩阵

| 主张 | 证据 | 标签 |
| --- | --- | --- |
| `sub_15268` 身份与 `0x15268` 条目 | common-menu H2 fixture | 已确认 static |
| 所有者行 hash 与 22/6/2/0/0/0 计数 | 已接受 H2 生成清单 | 已确认 static |
| 排他 `0x15268..0x15284` 区间 | 固定源与 H1 列表 | 已确认 static source |
| 隔离写入、`0xC` 循环配置、十三次写入与 `rts` 顺序 | 固定源与 H1 列表 | 已确认 static source |
| 零外部符号调用方出现 | 完整已接受源树 token 搜索 | 已确认 bounded inventory |
| “unused menu engine”用途 | 仅上游路径/注释词汇 | 推断 |
| 间接可达性、RAM 含义、机器状态、时序、可见结果 | 未被所选所有者建立 | 未知 |
| 精确单记录未来关联 | H2 连接与 research-index 审计 | 已确认 metadata |

## 复现

```powershell
uv run sf2 h2 common-menus
uv run sf2 design-contracts test
```

生成清单保留在忽略的 `local/derived/common-menus-static.json` 下。私有 ROM、H1、完整源体、模拟器、轨迹、RAM 与呈现工件保持受追踪合同之外。
