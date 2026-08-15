# 未使用 Mapload 控制流合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对下文描述的受限源身份、条目地址、调用与操作数顺序、循环分支与直接审查内部辅助）
- 现代化：**允许** 只把它保留为档案兼容元数据、用引擎原生服务替换其协作者，或省略生产端点，除非后续证据建立必需调用方
- 未知：自然或调试可达性、被调方效果、循环活性、RNG 结果、VDP/VInt 与摄像机行为、时序、呈现，以及已接受辅助局部字保存之外的调用方可视 ABI

> 本文件是 [`unused-mapload-control-flow.md`](../../contracts/unused-mapload-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同保留已接受 common-map 清单中最小的剩余源所有者：`unused_mapload.asm`。其索引条目是 `sub_2EC0`。文件请求四个随机值、暂存八个字、调用独立显示辅助、等待 VInt，然后在源码具名滚动位域保持非零时重复受限源码形状请求序列。其内部 `sub_2F24` 辅助条件递增四个源码具名平面滚动速度字。

上游文件名与注释把这些例程称为未使用地图加载函数。本合同保持该词汇可见，而不把它当作死代码、正常玩法目的或运行时不可访问性的证明。它记录原版控制流身份，使保真适配器能解释或测试源接缝，而不强迫现代重制复现其背后的 Mega Drive 服务。

## 判断边界

**已确认静态：** [`sf2-common-maps-static-v1`](../../../../tests/fixtures/h2/common-maps-static-v1.json) 把 `sub_2EC0` 绑定到 ROM 地址 `0x2EC0`（`11968`）并识别 `unused_mapload.asm` 为其代表源。已接受 source/H1 边界把 `sub_2EC0` 放在 `0x2EC0..0x2F24`，直接审查内部辅助 `sub_2F24` 放在 `0x2F24..0x2F6A`。

源码形状顶层顺序也是已确认静态：四个 `GenerateRandomNumber` 请求身份及其操作数/数据流；八个暂存 `d0..d7` 字；`sub_36B2` 调用；初始 `WaitForVInt` 调用；以及含两个有序 `SetVdpReg` 请求字、另一个 `WaitForVInt`、`sub_2F24` 与滚动位域分支的循环。辅助的四个有序字更新、带符号 `bgt` 决定与平衡 `d0.w` 栈保存是直接源事实。

这些声称识别原版指令与请求顺序。它们不确认任何被调方接受、完成或可见实现请求。

**推断：** 源文件名、注释与清单标签把例程分类为未使用与随机化地图加载代码。该词汇暗示开发或实验起源，但不建立意图、完整地图加载操作或任何玩家面向功能。

**未知或排除：** 自然、调试、间接、表驱动或原始地址可达性；调用方准入与输入状态；RNG 分布、种子使用与返回值；`sub_36B2` 行为；VDP 命令传输或接受；VInt 节奏或完成；谁改变被测试位域；循环终止；摄像机与滚动结果；带符号溢出状态；MMIO、中断、DMA、渲染、时序、持久性与呈现；顶层返回值；精确辅助局部 `d0.w` 保存之外的栈、寄存器与 CCR 行为；以及重制是否需要可调用运行时端点。

## 证据合同

本合同只消费来自[`sf2-common-maps-static-v1`](../../../../tests/fixtures/h2/common-maps-static-v1.json)的这些字段与身份：

- `function.unusedMaploadAddress`；
- `expected.representativeSymbols["unused_mapload.asm"]`；
- `expected.mapFacts.inventoryBoundary.unusedRandomMaploadInventoried`；
- `upstreamCommit` 与 `romSha256` 溯源；
- `code/common/maps/unused_mapload.asm` 的源路径成员与已接受摘要绑定所有者行。

合同显式**不**消费：

- `expected.mapFacts.mapSwitch`；
- `expected.mapFacts.battleTrigger`；
- `expected.mapFacts.egress`；
- `expected.mapFacts.mapLayout`；
- `expected.mapFacts.vint`；
- 清单边界中的 `cameraStateMachineInventoried` 或 `cameraAndVdpTimingRemainQueued`；
- 任何 H3 摄像机、地图、中断、图形或呈现 fixture。

所属[common-map 研究](../../../research/common-maps.md)、可执行[`maps.py`](../../../../src/sf2tool/h2/maps.py)与提取[`manifest`](../../../../manifests/extractions/common-maps-static.json) 保留完整七文件清单与已接受输出摘要。所选源的摘要绑定生成行记录：

| 清单字段 | 已接受值 |
| --- | ---: |
| 源 SHA-256 | `6852E300E9705C57A77456FA5CE028686493AF0E4D592B644FE073FEC40C2C55` |
| 源行 | `86` |
| 解析语句 | `51` |
| 全局标签 | `7` |
| 局部标签 | `0` |
| 传出直接调用站点 | `10` |
| 不同直接调用目标 | `5` |
| 间接调用站点 | `0` |

十个传出站点是四个 `GenerateRandomNumber` 请求、两个 `SetVdpReg`、两个 `WaitForVInt`、一个 `sub_36B2` 与一个 `sub_2F24`。这些是传出调用计数。它们不计数传入调用方，也不建立 `sub_2EC0` 不可达。

受限源形状直接在固定[`unused_mapload.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/maps/unused_mapload.asm)中审查。H1 列表提供条目与排他结束边界。本合同不从 H2 fixture 声称逐字节指令体 parity。

### 精确 research-index 分母

fixture 的源成员面包含跨七个源路径的八条记录。六条携带直接 `sf2-common-maps-static-v1` 证据；两条是仅成员行，其可执行证据属于他处。

| 记录 | 与本 fixture 的关系 | 本合同后的设计所有权 |
| --- | --- | --- |
| `maps.unused-mapload` | 直接 H2 绑定 | 本合同；注册前当前未关联 |
| `maps.camera` | 直接 H2 绑定 | 保持未关联且在本合同之外 |
| `maps.animations` | 直接 H2 绑定 | 不变：`map-exploration` |
| `maps.switch-map` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `maps.battle-trigger` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `maps.savepoint` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `map.camera-control.wait-for-view-scroll-end` | 仅源成员 | 不变：`map-exploration`；专用 H3 所有者 |
| `maps.map-layout` | 仅源成员 | 不变：`map-layout-data`；专用布局所有者 |

未来语义关联恰好是 `maps.unused-mapload`。没有辅助、RNG、VDP、VInt、摄像机、地图布局、地图路由或聚合 map-data 记录获得本合同。

## 源静态控制流

### 随机请求与暂存前缀

`sub_2EC0` 按顺序执行以下源操作：

| 步骤 | 源请求或操作 | 暂存结果身份 |
| ---: | --- | --- |
| 1 | 设置 `d6.w = 0x20`；调用 `GenerateRandomNumber` | 把返回 `d7.w` 复制到 `d0.w` |
| 2 | 设置 `d6.w = 4`；调用 `GenerateRandomNumber` | 把返回 `d7.w` 复制到 `d1.w`，然后加 `0x1C` |
| 3 | 设置 `d6.w = 0x10`；调用 `GenerateRandomNumber` | 把返回 `d7.w` 复制到 `d2.w` |
| 4 | 设置 `d6.w = 4`；调用 `GenerateRandomNumber` | 把返回 `d7.w` 复制到 `d3.w` |
| 5 | 把 `4` 写入 `d4.w`、`d5.w`、`d6.w` 与 `d7.w` 各一次 | 八个暂存字现在在 `d0..d7` |
| 6 | 调用 `sub_36B2` | 独立所有者显示辅助交接 |
| 7 | 调用 `WaitForVInt` | 独立所有者等待交接 |

常量与数据流是调用站点事实。本合同不断言 `GenerateRandomNumber` 返回任何特定范围或分布、`sub_36B2` 按其名称推断消费寄存器，或等待完成帧或显示操作。

### 源码形状循环

前缀后，源进入 `loc_2F04`。每次迭代有该顺序：

1. 把请求字 `0x8721` 写入 `d0.w` 并调用 `SetVdpReg`；
2. 把请求字 `0x8700` 写入 `d0.w` 并调用 `SetVdpReg`；
3. 调用 `WaitForVInt`；
4. 调用内部辅助 `sub_2F24`；
5. 测试源码具名 `VIEW_SCROLLING_PLANES_BITFIELD` 字节；
6. 该字节非零时分支回步骤 1，否则执行 `rts`。

这是辅助后测试顺序：只要控制到达 `loc_2F04`，体在初始前缀等待后至少运行一次。合同不把两个请求字重新解读为成功 VDP 写入、不定义谁清除位域，也不保证循环终止。

### 内部四字辅助

`sub_2F24` 首先压入 `d0.w`。然后按精确顺序处理这些源码具名字：

1. `PLANE_A_SCROLL_SPEED_X`；
2. `PLANE_A_SCROLL_SPEED_Y`；
3. `PLANE_B_SCROLL_SPEED_X`；
4. `PLANE_B_SCROLL_SPEED_Y`。

对每个字，辅助把它加载进 `d0.w`、加一、把字结果与 `128` 比较，并在结果带符号大于 `128` 时用带符号 `bgt` 跳过存储。否则写回结果。最后弹出 `d0.w` 并返回。

该规则刻意不被总结为无条件饱和递增。对普通受限值，`127` 存储 `128`，而当前值 `128` 计算 `129` 并让存储字不变。负输入、字溢出与此类受限合成检查之外的状态保留其精确源分支形状，但无已接受运行时含义。

辅助证明一个平衡双字节栈槽与调用方 `d0.w` 值的恢复。它不证明 CCR 中性、全寄存器保留、中断安全或任何顶层 ABI。

## 跨系统分离

- [`randomness`](../../contracts/randomness.md) 拥有已接受 RNG 算法与运行时矩阵。本合同只拥有四个调用站点请求操作数与结果暂存顺序。
- [`graphics-service-state`](../../contracts/graphics-service-state.md)与所属[technical-graphics 研究](../../../research/technical-graphics.md) 保留显示辅助、VDP 寄存器服务与图形效果证据。其效果此处不导入。
- [`interrupt-dma-and-trap-state`](../../contracts/interrupt-dma-and-trap-state.md) 拥有已接受 `WaitForVInt`/VInt 握手与 DMA/中断边界。此处的源调用身份不复制该合同。
- [`map-exploration`](../../contracts/map-exploration.md) 拥有摄像机/滚动状态、地图 VInt 与运行时/呈现接缝。本合同不给四个速度字或被测试位域指定含义。
- [`map-entry-routing-state`](../../contracts/map-entry-routing-state.md) 拥有切换、战斗触发与存档点选择。[`map-layout-data`](../../contracts/map-layout-data.md) 拥有静态解码布局语料。
- `sf2-map-data-static-v1`、地图记录、碰撞、实体状态、故事状态与可见地图内容被排除。

源标签 `sub_36B2` 与原始 RAM 符号是追踪锚点，不是新拥有服务或状态记录。

## 实现无关模型

私有保真/导入层可以把已接受面表示为：

```text
UnusedMaploadControlFlow {
  identity {
    fixtureId
    sourcePath
    sourceSha256
    entryAddress
    helperAddress
    exclusiveEndAddress
    upstreamCommit
    romSha256
  }
  randomRequests[4] {
    orderedBoundOperand
    orderedDestinationWord
    postAddend
  }
  stagedConstantWords[4]
  prefixHandoffs[2]
  loop {
    orderedVdpRequestWords[2]
    waitHandoff
    helperHandoff
    postHelperBitfieldTest
    repeatOnNonzero
  }
  speedHelper {
    orderedWordIdentities[4]
    addend
    signedThreshold
    storeWhenNotSignedGreater
    savedWordIdentity
  }
}
```

公开合同可以保留上文命名的受限源路径与符号、所选 H1 地址、常量、调用计数摘要、源 hash、分支/数据流规则、fixture 摘要与溯源。完整 source/H1/ROM 体、指令字节与其他非公开验证材料不是公开投影的一部分。固定上游链接是溯源，不是复制源所有权。

在 fixture 已接受 ROM 溯源下验证固定源时间线与 H1 条目/边界身份后，重制可以用引擎原生回调、类型化状态或仅轨迹档案适配器表示抽象请求。这不声称指令体 parity。重制不需要在生产代码中复现 Mega Drive 地址、68000 寄存器文件、字栈机制、VDP 寄存器、硬件中断循环或原版指令序列。

## 保真与现代化

原版保真证据要求保留以下区分：

- 索引 `sub_2EC0` 条目对直接审查 `sub_2F24` 辅助；
- 四个有序 RNG 请求操作数对任何 RNG 结果语义；
- 暂存源寄存器对被调方效果；
- 一个初始等待对辅助后测试循环内的等待；
- 两个有序 VDP 请求字对传输或渲染成功；
- 四个有序速度字操作对通用夹断抽象；
- 一个辅助局部 `d0.w` 保存对通用 ABI 承诺；
- 源词汇对运行时可达性。

现代引擎可以省略生产端点、把它内联进私有兼容测试或使用注入服务。如果保留兼容接缝，合成轨迹可以覆盖：

- 首次位域观察为零：前缀请求、一个循环体，然后返回；
- 非零观察后接零：同一前缀与两个有序循环体；
- 辅助输入 `127` 与 `128`：第一个存储 `128`，第二个让存储字保持 `128`；
- 辅助完成后提供的逻辑 `d0.w` 值恢复。

那些是已接受源关系上的现代化测试。它们不是原版运行时可达性、经过帧、可见输出或自然状态分布的观察。

## H4 验收检查清单

1. 保留字段闭合 fixture 身份、条目地址、源溯源与已接受所有者行，而不消费兄弟地图事实子树。
2. 保留四个 RNG 请求操作数与 `d0..d3` 结果暂存顺序，后接 `d4..d7 = 4`，仅作为源静态调用站点事实。
3. 保留 `sub_36B2` 然后初始 `WaitForVInt`，后接精确两请求/等待/辅助/辅助后测试循环顺序。
4. 保留内部辅助的四个字身份、加一操作、与 128 的带符号比较、条件存储与精确 `d0.w` 保存/恢复边界，而不声称通用饱和或 CCR/全寄存器中性。
5. 不把调用身份转换成 RNG、显示、VDP、VInt、摄像机、转移完成或呈现声称。
6. 保持自然可达性、活性、畸形状态、溢出含义、时序与生产 API 必要性为未知，除非更强已接受证据闭合它们。
7. 把完整 source/H1/ROM 体与指令编码保持公开投影之外；只暴露本合同列出的受限元数据与溯源。
8. 只注册 `maps.unused-mapload`；保持 `maps.camera`、两条仅成员行、每个既有兄弟关联与每个独立服务记录不变。

## 证据矩阵

| 主张 | 证据 | 标签 |
| --- | --- | --- |
| `sub_2EC0` 身份与 `0x2EC0` 条目 | common-map H2 fixture 与 H1 列表 | Confirmed static |
| `sub_2EC0..sub_2F24..0x2F6A` 边界 | 固定源与 H1 列表 | Confirmed static source |
| 86/51/7/0 与 10 调用所有者行清单 | 已接受摘要下的 H2 生成所有者行 | Confirmed static |
| 四个 RNG 请求操作数与寄存器暂存 | 固定 `unused_mapload.asm` | Confirmed static source |
| 显示/等待/VDP/辅助/位域循环顺序 | 固定 `unused_mapload.asm` | Confirmed static source |
| 四个有序辅助字、带符号分支、`d0.w` 保存 | 固定源与 H1 列表 | Confirmed static source |
| “unused/randomized mapload”含义 | 仅上游词汇 | Inferred |
| 被调方效果、运行时可达性、活性、时序、可见结果 | 未被所选所有者建立 | Unknown |
| 精确单记录关联边界 | research-index 与 fixture 成员审计 | Confirmed metadata |

## 复现

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成清单保留在忽略的 `local/derived/common-maps-static.json` 下。私有 ROM、H1、源体、模拟器、轨迹与捕获呈现材料保持受追踪合同之外。
