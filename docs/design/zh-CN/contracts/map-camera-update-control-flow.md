# 地图摄像机更新控制流合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对受限 `VInt_UpdateViewData` 身份、带符号分支拓扑、源顺序目标交接、速度选择优先级与下文描述的四字宽视差更新路径）
- 现代化：**允许** 在已准入兼容输入上使用引擎原生摄像机状态与调度，同时保留等价抽象决定/更新轨迹
- 未知：自然调用方准入、运行时目标域、被调方效果、寄存器/CCR ABI、中断节奏、滚动轨迹、VDP 可见输出、帧时序与呈现

> 本文件是 [`map-camera-update-control-flow.md`](../../contracts/map-camera-update-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同保留已接受 common-map 清单中剩余的未关联函数：`VInt_UpdateViewData`，位于 `camerafunctions.asm`。源首先选择是否运行实体目标调整路径、条件请求新视图目标，然后通过有序覆盖与逐轴门序列派生四个平面轴滚动速度字。

函数名与源注释暗示摄像机/视图目标跟随行为。该词汇不是玩家可见意图、完整摄像机子系统或特定回调列表执行节奏的证据。合同只拥有受限源静态更新算法。摄像机命令、区域数据、目标服务行为、回调注册与渲染呈现保持其既有所有者。

## 判断边界

**已确认静态：** [`sf2-common-maps-static-v1`](../../../../tests/fixtures/h2/common-maps-static-v1.json) 把 `VInt_UpdateViewData` 绑定到 ROM 地址 `0x45C2`（`17858`）并识别 `camerafunctions.asm` 为其代表源。H1 列表把函数放在排他区间 `0x45C2..0x4708`（`326` 字节）。独立 `WaitForViewScrollEnd` 条目在 `0x4708` 开始；其循环不是本合同的一部分。

固定源直接审查确认带符号目标分支、带符号字比较分支、目标请求顺序、计数器更新、速度值优先级、四个自动滚动门与精确 `move.w`/`mulu.w`/`lsr.w`/`move.w` 宽度序列。受限函数内有一个 `IsMapScrollingToViewTarget` 直接调用与一个 `SetViewDestination` 直接调用。

这些是源操作与分支身份。它们不确认被调方含义、请求接受、目标完成、滚动运动、VInt 频率或可见输出。

**推断：** 源身份与注释暗示自动摄像机目标更新与滚动速度准备角色。前景/背景视觉含义、视口意图与玩家面向行为不被所选所有者确认。

**未知或排除：** 自然准入的 `VIEW_TARGET_ENTITY` 与实体记录域；带符号负字节代表一个还是多个哨兵类；调用方配置与可达性；`word_FFA828` 在其精确源数据流之外的含义；字溢出与畸形状态；寄存器、栈与 CCR 保证；`IsMapScrollingToViewTarget` 与 `SetViewDestination` 的运行时效果；命令处理器与等待行为；回调激活与顺序；中断原子性；VDP 状态；渲染轨迹；帧节奏；持久性；呈现；以及调试/原始 RAM 注入行为。

## 证据合同

本合同只消费来自[`sf2-common-maps-static-v1`](../../../../tests/fixtures/h2/common-maps-static-v1.json)的这些字段与身份：

- `function.cameraAddress`；
- `expected.representativeSymbols["camerafunctions.asm"]`；
- `expected.mapFacts.inventoryBoundary.cameraStateMachineInventoried`；
- `expected.mapFacts.inventoryBoundary.cameraAndVdpTimingRemainQueued`；
- `upstreamCommit` 与 `romSha256` 溯源；
- `code/common/maps/camerafunctions.asm` 的源路径成员关系。

合同显式**不**消费：

- `expected.mapFacts.mapSwitch`、`battleTrigger`、`egress`、`mapLayout` 或 `vint`；
- `unusedRandomMaploadInventoried`；
- `sf2-tech-graphics-static-v1.graphicsFacts.viewDestination`；
- 任何 H3 摄像机、地图、中断、图形或呈现 fixture；
- `sf2-map-data-static-v1` 或任何聚合 map-data 记录。

所属[common-map 研究](../../../research/common-maps.md)、可执行[`maps.py`](../../../../src/sf2tool/h2/maps.py)与提取[`manifest`](../../../../manifests/extractions/common-maps-static.json) 保留完整七文件清单与已接受摘要。生成清单行 hash 完整双函数源文件，因此其文件级调用计数不得被误认为该函数受限双调用面。

受限源形状直接在固定[`camerafunctions.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/maps/camerafunctions.asm)中审查。H1 列表提供条目与排他结束身份。当前 H2 所有者不证明对 H1 与 ROM 的逐字节指令体一致性，本合同不声称它。

### 精确 research-index 分母

fixture 的源成员面包含跨七个源路径的八条记录。六条携带直接 `sf2-common-maps-static-v1` 证据；两条是仅成员行，其可执行证据属于他处。

| 记录 | 与本 fixture 的关系 | 本合同后的设计所有权 |
| --- | --- | --- |
| `maps.camera` | 直接 H2 绑定 | 本合同；注册前当前未关联 |
| `maps.animations` | 直接 H2 绑定 | 不变：`map-exploration` |
| `maps.switch-map` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `maps.battle-trigger` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `maps.savepoint` | 直接 H2 绑定 | 不变：`map-entry-routing-state` |
| `maps.unused-mapload` | 直接 H2 绑定 | 不变：`unused-mapload-control-flow` |
| `map.camera-control.wait-for-view-scroll-end` | 仅源成员 | 不变：`map-exploration`；专用 H3 所有者 |
| `maps.map-layout` | 仅源成员 | 不变：`map-layout-data`；专用布局所有者 |

未来语义关联恰好是 `maps.camera`。没有等待、目标服务、实体、区域数据、VInt 分发、地图动画、图形服务、地图路由或地图数据记录获得本合同。

## 源静态更新流

### 带符号目标分支与平面字选择

函数清除 `d0.w`、把原始 `VIEW_TARGET_ENTITY` 字节加载进 `d0.b` 并执行 `bmi.w loc_468C`。取该带符号负分支时，控制直接进入速度派生。它绕过所有实体读取、`IsMapScrollingToViewTarget` 调用、每个 `SetViewDestination` 路径与无调整 `clr.w word_FFA828` 路径。因此既有 `word_FFA828` 值在速度选择前保持不变。

对非负目标字节，源把字左移 `ENTITYDEF_SIZE_BITS`（`5`）、把该偏移加到 `ENTITY_DATA` 并把两个有序字读进 `d4` 与 `d5`。源注释称那些字为实体 X 与 Y。函数随后选择当前 `d2`/`d3` 对，并通过测试 `MAP_AREA_LAYER_TYPE` 完成选择：零读取 View Plane B 像素字；非零读取 View Plane A 像素字。这是原始源选择词汇，不是渲染前景/背景合同。

清除 `d6.w` 后，函数调用 `IsMapScrollingToViewTarget`。其 `bne.w return_4706` 分支在非零条件时立即返回，在目标调整、计数器变更、速度选择或四个速度字写入之前。本合同只保留该分支结果；它不把语义指定给被调方。

### 精确带符号阈值与边界分支

以下所有比较是字比较后接带符号 `bge` 或 `ble` 分支。十进制常量与 `MAP_TILE_SIZE = 384` 保持原版内部源单位。它们不是声明的屏幕像素、瓦片维度、视口大小或任意输入的安全域。

| 轴路径 | 精确比较与带符号分支 | 仅在分支不跳过时操作 |
| --- | --- | --- |
| X 第一阈值 | 比较 `d4` 与 `d2 + 1536`；`bge.s loc_4616` | 继续下侧候选 |
| X 下界 | 比较 `d2` 与 `MAP_AREA_LAYER1_STARTX`；`ble.w loc_4638` | 减 `MAP_TILE_SIZE` 并递增 `d6.w` |
| X 第二阈值 | 比较 `d4` 与 `d2 + 2304`；`ble.s loc_4638` | 继续上侧候选 |
| X 上界 | 比较 `d2` 与 `MAP_AREA_LAYER1_ENDX - 3840`；`bge.w loc_4638` | 加 `MAP_TILE_SIZE` 并递增 `d6.w` |
| Y 第一阈值 | 比较 `d5` 与 `d3 + 1536`；`bge.s loc_4654` | 继续下侧候选 |
| Y 下界 | 比较 `d3` 与 `MAP_AREA_LAYER1_STARTY`；`ble.w loc_4676` | 减 `MAP_TILE_SIZE` 并递增 `d6.w` |
| Y 第二阈值 | 比较 `d5` 与 `d3 + 2304`；`ble.s loc_4676` | 继续上侧候选 |
| Y 上界 | 比较 `d3` 与 `MAP_AREA_LAYER1_ENDY - 3456`；`bge.w loc_4676` | 加 `MAP_TILE_SIZE` 并递增 `d6.w` |

表保留源分支极性，而非用无符号或实现选择几何谓词替换它。字溢出、跨符号比较、无效实体索引与畸形边界不保留已接受运行时含义。

### 目标交接与计数器路径

两轴收敛后，函数测试 `d6.w`：

1. 非零把 `d2.w`/`d3.w` 复制到 `d0.w`/`d1.w`、调用 `SetViewDestination`、递增 `word_FFA828` 一次并分支到速度派生；
2. 零执行 `clr.w word_FFA828` 并 fall through 到速度派生。

计数器每个目标交接递增一次，而非每调整轴一次。更早带符号负目标分支在不取任一计数器路径的情况下到达速度派生，而非零 `IsMapScrollingToViewTarget` 分支在速度派生前返回。这三条路线必须保持不同。

`SetViewDestination` 调用是交接身份。目标轴计算、下游状态写入、运行时命令行为与可见滚动仍归[`map-exploration`](../../contracts/map-exploration.md)及其已接受摄像机证据。

## 速度选择与四个宽度受限写入

速度选择源有序，后续适用值替换更早值：

1. 加载 `word_FFA828`；带符号 `cmpi.w #6` 加 `ble.s` 选择 `24`，否则选择 `32`；
2. 原始目标字节等于 `ENTITY_CURSOR`（`0x30`）时用 `64` 替换值；
3. `FADING_SETTING` 等于 `PULSATING_1`（`5`）时用 `32` 替换它；
4. 非零 `VIEW_SCROLLING_SPEED` 用该存储字替换它。

合同保留该优先级，而不给值指定物理速度、帧率或呈现含义。

源然后按精确顺序处理这四个轴：

1. Layer 1 X → `PLANE_A_SCROLL_SPEED_X`；
2. Layer 1 Y → `PLANE_A_SCROLL_SPEED_Y`；
3. Layer 2 X → `PLANE_B_SCROLL_SPEED_X`；
4. Layer 2 Y → `PLANE_B_SCROLL_SPEED_Y`。

对每个轴，其自动滚动字节上的 `tst.b` 后接 `bne` 到下一轴或终点。因此非零字节保留既有速度字。只有零路径执行该精确宽度序列：

1. `move.w d7,d0`；
2. 无符号 `mulu.w` 乘对应视差字，在 `d0.l` 产生乘积；
3. `lsr.w #BYTE_SHIFT_COUNT,d0`，只把该乘积低字右移八；
4. `move.w d0` 到对应速度字。

高乘积字不移入存储结果。这不得被归一化为通用全 32 位 `(speed * parallax) >> 8` 公式。兼容模型要么保留指令宽度序列，要么为其已准入输入域证明精确等价低字结果。

## 跨系统分离

- [`map-exploration`](../../contracts/map-exploration.md) 保留摄像机命令记录、H3 目标/目标/速度观察、`SetCameraDestination`、`SetViewDestination` 行为、`WaitForViewScrollEnd`、区域视差/自动滚动输入与地图生命周期。本合同只拥有受限更新函数。
- [`graphics-service-state`](../../contracts/graphics-service-state.md) 继续排除 `graphicsFacts.viewDestination`；它把该源静态更新算法委托此处，同时保留其图形服务职责。
- [`interrupt-dma-and-trap-state`](../../contracts/interrupt-dma-and-trap-state.md) 拥有回调注册、VInt/中断传输、DMA 接缝与时序边界。`VInt_` 源前缀不复制该合同。
- [`map-entity-data`](../../contracts/map-entity-data.md) 与实体状态所有者保留记录身份、人口与运行时状态。此处的两次字读取不创建第二实体 schema。
- [`map-layout-data`](../../contracts/map-layout-data.md)、[`map-entry-routing-state`](../../contracts/map-entry-routing-state.md)、[`unused-mapload-control-flow`](../../contracts/unused-mapload-control-flow.md)与地图动画所有者保留其既有数据与控制流记录。
- 渲染器组合、可访问性策略、替换内容、呈现与硬件保真保持未来刻意设计或 **未知**。

## 实现无关模型

私有兼容层可以把已接受面表示为：

```text
MapCameraUpdateControlFlow {
  identity {
    fixtureId
    sourcePath
    sourceSymbol
    entryAddress
    exclusiveEndAddress
    upstreamCommit
    romSha256
  }
  targetRoute {
    signedNegativeBranchTarget
    negativeLeavesCounterUnchanged
    nonnegativeEntityOffsetShift
    orderedCoordinateWordReads[2]
    layerTypePlaneWordSelection
    scrollingNonzeroEarlyReturn
  }
  signedAdjustmentBranches[8] {
    comparisonWidth
    branchCondition
    branchTarget
    sourceUnitOperand
    adjustmentWhenNotSkipped
  }
  destinationRoute {
    adjustmentCountWord
    handoffOnNonzero
    incrementCounterOnce
    clearCounterOnZero
  }
  speedPrecedence[4]
  axisUpdates[4] {
    autoscrollByteGate
    speedWordInput
    parallaxWordInput
    unsignedWordMultiply
    lowWordLogicalShiftBy8
    wordStore
  }
}
```

公开合同可以保留受限源路径/符号、所选 H1 地址、原始常量、分支极性、有序操作身份、fixture 摘要与溯源。完整 source、H1 或 ROM 指令体、完整编码与其他非公开验证材料保持私有或可选未来证据。

在已接受 ROM 溯源下验证固定源时间线与 H1 条目/边界后，重制可以使用类型化引擎原生状态、引用与回调。它不需要复现 Mega Drive 地址、68000 寄存器文件、回调表、VInt 微调度或原版指令序列。兼容性在抽象分支/更新轨迹与已准入字结果边界测量，而非可见帧或指令字节。

## 保真与现代化

原版保真证据要求保留以下区分：

- 带符号负目标绕过对非负实体处理；
- 负目标计数器保留对无调整清除对调整递增；
- 非零滚动结果提前返回对速度派生路线；
- 八个精确带符号字分支身份对无符号或几何归一化测试；
- 目标交接身份对下游目标行为；
- 四个有序速度覆盖对所选物理速度模型；
- 自动滚动非零保留对零路径重算；
- `mulu.w` 乘积后接仅低字 `lsr.w` 与 `move.w` 对全乘积缩放；
- 源静态回调代码对 VInt 节奏与渲染呈现。

现代引擎可以通过普通摄像机系统代码表达相同已准入轨迹。合成兼容用例应至少覆盖：

- 带已播种 `word_FFA828` 的带符号负目标，证明直接速度派生且无目标/计数器清除路径；
- 其滚动检查在所有后期写入前分支到终点的非负目标；
- 每个 `bge`/`ble` 阈值与边界身份的取与不取侧，而不把它们重新解读为无符号比较；
- 无调整清除与一次目标交接/计数器递增的一或两轴调整；
- 每个适用速度覆盖优先级边；
- 每个自动滚动门，以及高字会区分低字移位与全 32 位移位的乘积。

那些是已接受源关系上的兼容测试，不是自然运行时状态、经过帧或可见摄像机运动的观察。

## H4 验收检查清单

1. 保留字段闭合 fixture 身份、`0x45C2..0x4708` 函数边界、源溯源与已接受所有者关系，而不消费兄弟地图事实子树。
2. 保留带符号负目标分支直接到速度派生，包括绕过 `SetViewDestination` 与无调整计数器清除，让 `word_FFA828` 不变。
3. 保留非负目标的有序实体/平面读取与非零滚动结果提前返回，而不导入实体 schema、被调方含义或可见摄像机行为。
4. 保留全部八个字比较操作数与精确带符号 `bge`/`ble` 分支极性、源单位常量与调整操作，不做无符号/几何归一化。
5. 保留不同的调整非零交接/递增与调整零清除路径；计数器每个交接递增一次。
6. 保留速度优先级 `24/32 -> cursor 64 -> pulsating 32 -> nonzero explicit speed` 作为源数据流，而非时序或物理速度。
7. 保留全部四个有序自动滚动门与精确 `move.w`、无符号 `mulu.w`、低字 `lsr.w #8`、`move.w` 存储序列；除非为已准入兼容域证明结果等价，否则不替换通用全乘积公式。
8. 把命令/H3、目标服务行为、区域输入、VInt 调度、硬件效果、呈现、畸形状态与运行时可达性保持其独立所有者或未知。
9. 把完整指令体与编码保持公开投影之外；只暴露此处列出的受限元数据、操作关系与溯源。
10. 只注册 `maps.camera`；保持五条既有直接兄弟关联、两个仅成员所有者与每个其他 research-index 对象不变。

## 证据矩阵

| 主张 | 证据 | 标签 |
| --- | --- | --- |
| `VInt_UpdateViewData` 身份与 `0x45C2` 条目 | common-map H2 fixture 与 H1 列表 | 已确认 static |
| `0x45C2..0x4708` 排他区间与独立等待条目 | 固定源与 H1 列表 | 已确认 static source |
| 带符号目标路线与不同计数器路径 | 固定 `camerafunctions.asm` | 已确认 static source |
| 八个带符号阈值/边界分支身份 | 固定源与 H1 列表 | 已确认 static source |
| 速度值优先级与四个自动滚动门 | 固定 `camerafunctions.asm` | 已确认 static source |
| 字乘、低字移位与字存储宽度 | 固定源与 H1 列表 | 已确认 static source |
| 自动摄像机跟随/玩家可见含义 | 仅源码词汇 | 推断 |
| 被调方效果、运行时可达性、VInt 节奏、轨迹、可见结果 | 未被所选所有者建立 | 未知 |
| 精确单记录关联边界 | research-index 与 fixture 成员审计 | 已确认 metadata |

## 复现

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成清单保留在忽略的 `local/derived/common-maps-static.json` 下。私有 ROM、H1、完整源体、模拟器、轨迹与捕获呈现材料保持受追踪合同之外。
