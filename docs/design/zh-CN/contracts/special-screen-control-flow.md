# 特殊画面控制流合同

- **已确认原版结构：** 下文描述的受限 Sega-logo、标题、witch 入口/菜单、挂起与结尾控制事实、代表函数身份、源静态计数器与分发接缝。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 正常玩家驱动可达性、控制器节奏、墙钟或可见时长、淡入、VInt/DMA/CRAM 时序、渲染像素、音频时序、跨进程存档持久性、中断后恢复、alternate 构建行为、本地化、可访问性与玩家面向含义。
- 重制状态：实现无关 Phase 3 兼容合同；未选择画面框架、渲染器、输入模型、存档后端、替换呈现或分发许可。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`special-screen-control-flow.md`](../../contracts/special-screen-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同为五个特殊画面族定义受限静态控制面：

1. Sega-logo 校验和、配置/调试序列所有权与 Start 提前退出结构；
2. 标题入口、两个滚动循环身份、受限 Start 轮询、退出选择与标题字体加载器身份；
3. witch SRAM 结果准入、顶层动作可用性/取消/分发与通用四页菜单导航；
4. 挂起源计数器、Start 提前退出分支与重置向量交接；
5. 结尾之吻像素填充与结尾 witch 落宝石/控制所有权身份。

可执行所有者是 `sf2-special-screens-static-v1`，位于 [`tests/fixtures/h2/special-screens-static-v1.json`](../../../../tests/fixtures/h2/special-screens-static-v1.json)。研究所有者是[Special Screens](../../../research/special-screens.md)，它记录固定 source/H1 审查，并显式把静态控制流与运行时呈现分开。

本合同不拥有原版图形字节、调色板、布局、解压、转移执行、SRAM 变更语义、逐动作存档选择器与服务调用、新游戏生命周期、玩家驱动输入或可见呈现。那些是独立合同或保持 **未知**。

## 合同前证据审计

专用 H2 所有者于证据日期从当前 `main` 复现：

```text
sf2-special-screens-static-v1
SHA256 DC8AB072E69417A0D2C33C71FF34BF80519B68469FB5DBD468359A28B7403E04
Files 19 / ScreenGroups 7 / Resources 18 / RuntimeQuestions 3 / PASS
```

三个分母必须保持不同。

### 源根成员：22 条记录

fixture 的已接受 `expected.indexedRecordIds` 成员语料包含 22 条记录。十九条有直接 `sf2-special-screens-static-v1` research-index 证据绑定。三条成员记录刻意使用更特定可执行所有者：

- `screens.title.compressed-tiles` 由 `sf2-special-screen-graphics-decode-v1` 与[special-screen-asset-data](../../contracts/special-screen-asset-data.md)拥有；
- `screens.witch.new-game-lifecycle` 由 `sf2-witch-new-game-lifecycle-runtime-v1` 与[save-system](../../contracts/save-system.md)拥有；
- `screens.witch.save-menu-actions` 由 `sf2-witch-save-menu-actions-runtime-v1` 与[save-system](../../contracts/save-system.md)拥有。

`code/specialscreens/witch/witchstart.asm` 源关系恰好是 `screens.witch.new-game-lifecycle`、`screens.witch.save-menu-actions` 与 `screens.witch.start`。这些更特定所有者不是本合同的证据依赖或关联目标。

### 直接 fixture 绑定：19 条记录

19 条直接 research-index 绑定精确分为三组。

十二条控制记录当前未关联，构成本合同新候选集：

- `screens.endkiss.engine`；
- `screens.segalogo.debug-cheat`；
- `screens.segalogo.engine`；
- `screens.suspend.engine`；
- `screens.suspend.witch`；
- `screens.title.engine`；
- `screens.title.font`；
- `screens.witch.functions`；
- `screens.witch.sound-test`；
- `screens.witch.start`；
- `screens.witchend.engine`；
- `screens.witchend.init`。

`screens.witch.menu` 已通过专用 witch 图形所有者属于[special-screen-asset-data](../../contracts/special-screen-asset-data.md)。它是唯一刻意重叠：该资源合同保留调色板/帧数据，而本合同只保留 `ExecuteWitchMainMenu` 控制/页/导航接缝。

其余六条直接绑定是资源记录，其既有资源关联必须保持语义不变：

- `screens.endkiss.resources`；
- `screens.jewelend.resources`；
- `screens.suspend.resources`；
- `screens.title.resources`；
- `screens.witch.resources`；
- `screens.witchend.resources`。

因此最终注册恰好改变 13 条记录：十二条新关联加一个刻意菜单重叠。22 记录源根语料的其他九条成员保持不变：上文六条资源记录加三条更特定所有者记录。

### 审计限制

审计还保留这些边界：

- fixture 清单七个组的 19 个源文件与十八个资源身份，但本控制合同不声称资源载荷所有权；
- fixture 布尔值与标量计数器建立受限事实，而非每个代表函数的详尽指令顺序；
- 逐动作 New/Load/Delete/Copy 选择器变换、`CURRENT_SAVE_SLOT` 写入、服务调用顺序、读档后路由与 `MainLoop` 交接保持[save-system](../../contracts/save-system.md)；
- 源静态值 60 与 600 是计数器操作数，不是观察墙钟或可见帧时长；
- 受追踪 fixture 只含小型元数据。ROM、存档、截图、图形、音频、轨迹与模拟器状态保持私有/生成。

## Sega-Logo 控制边界

**已确认静态：** 专用 fixture 与所属研究文章建立 Sega-logo 源组：

- 包含代表 `DisplaySegaLogo` 条目；
- 计算 ROM 校验和；
- 拥有配置模式与调试模式输入序列处理器；
- 可以在按下 Start 时提前返回；
- 包含 `VInt_CheckDebugModeCheat`，其源一次一个字节推进已接受调试序列，并在序列终止时激活调试切换。

可执行所有者保留代表符号、源路径与 ROM 地址。兼容适配器必须保留那些身份与主 logo 条目和 VInt 调试序列辅助之间的区分。

这不是完整控制器合同。精确输入采样、防抖、VInt 节奏、序列呈现、配置画面、校验和失败呈现、正常玩家可达性与 alternate 构建行为保持 **未知** 或独立所有者。

## 标题控制边界

**已确认静态：** 已接受标题源组有 `StartTitleScreen` 条目、两个不同滚动循环函数与多阶段使用的受限 Start 轮询辅助。其源退出区分重置与继续到 witch 画面。已接受所有者确认该控制形状，而非滚动/淡入帧计数或渲染过渡。

`screens.title.font` 只贡献 `LoadTitleScreenFont` 函数身份、源路径与地址。字体载荷字节、编解码器行为、布局、转移大小、VRAM 目标、字形含义与最终渲染在本合同之外。标题压缩瓦片成员记录保持[special-screen-asset-data](../../contracts/special-screen-asset-data.md)。

兼容适配器可以完全替换标题渲染，但原版路线模式必须把条目、双循环区分、受限轮询身份与重置对 witch 继续结果保持为独立可观察路线事实。

## Witch 入口与菜单控制

witch 边界刻意在顶层控制（此处拥有）与动作内部（他处拥有）之间拆分。

### 入口与动作页准入

**已确认静态：** 对 `code/specialscreens/witch/witchstart.asm` 的固定审查（记录在所属研究文档中）建立该受限入口接缝：

1. `StartWitchScreen` 调用 `CheckSram`；
2. 它在到达动作页前依次测试 `d0`、`d1`，并使用有序 `bpl.s` 分支；
3. 动作页掩蔽 `SAVE_FLAGS`，掩码为 `3`；
4. 已接受可用性用例按 `zero`、`allSet` 与 `otherNonzero` 有序，分别提供掩码 `1`、`6` 与 `15`；
5. 负菜单结果分支回 witch 文本/菜单循环；
6. 非负结果加倍以索引四行字分发表。

这些事实只定义顶层准入与交接。它们不定义存档有效性含义、渲染可用性、玩家输入节奏或动作成功。

### 有序分发身份

**已确认静态：** `rjt_WitchMenuActions` 有四个有序目标：

| 分发索引 | 目标身份 | H1 地址 |
| ---: | --- | ---: |
| 0 | `witchMenuAction_New` | `0x7406` |
| 1 | `witchMenuAction_Load` | `0x74E2` |
| 2 | `witchMenuAction_Del` | `0x7574` |
| 3 | `witchMenuAction_Copy` | `0x754C` |

本合同只把这些保留为分发交接身份。它不消费或重述动作的选择器变换、`CURRENT_SAVE_SLOT` 写入、提示、服务调用、调用顺序、读档后标志路线或 `MainLoop`/`alt_MainLoopEntry` 交接。那些保持[save-system](../../contracts/save-system.md)中的独立所有者事实。

### 通用菜单控制

**已确认静态：** 对 `code/specialscreens/witch/witchmainmenu.asm` 的固定审查建立 `ExecuteWitchMainMenu`：

- 用 `15` 掩蔽其起始选择器；
- 在文档化 B 按钮路径返回 `-1`；
- 检查可用位位置 0 到 3；
- 用掩码 `3` 回绕导航；
- 区分四个源标记页：动作、新槽名称、已加载槽名称与难度。

页身份、选择器掩码、回绕掩码、可用性位域与取消结果构成本合同刻意 `screens.witch.menu` 重叠。调色板数据、选项帧、气泡动画、渲染标签、输入时序、感知导航与动作后果此处不拥有。

`screens.witch.functions` 只保留代表 `InitializeWitchSuspendVIntFunctions` 身份及其源组所有权。聚合 fixture 本身不提升完整回调顺序。`screens.witch.sound-test` 保留 US `j_SoundTest` 仅返回身份；它不证明另一版本实现或玩家可见路线。

## 挂起控制边界

**已确认静态：** 已接受 fixture 与研究所有者记录的直接源审查建立：

- `SuspendGame` 与 `WitchSuspend` 为不同代表条目；
- 挂起呈现工作前有源操作数 60；
- 后期重启等待有源操作数 600；
- 可以提前结束后者等待的 Start 分支；
- 通过原版起始向量的重置交接。

两个操作数只是源静态计数器。本合同不称它们为秒、墙钟时长、保证显示帧或观察时序。它不建立输入采样节奏、淡入、资源转移、存档持久性、硬件重置行为或可见挂起组合。

## 结尾控制边界

**已确认静态：** 专用所有者与研究文章建立受限所有权身份：

- `DisplayEndingKissPicture` 拥有数据驱动像素填充渲染器；
- `WitchEnd` 是结尾 witch 初始化源的代表身份；
- `EndGame` 是代表结尾条目，而结尾 witch 源组拥有落宝石与 witch 眨眼控制身份并连接到结尾序列。

这些是函数/源/地址与操作所有权事实，不是完整指令时间线。像素填充顺序、宝石轨迹、眨眼节奏、VInt 回调、音频、最终组合、正常故事可达性与可见一致性保持 **未知** 或独立所有者。

## 实现无关控制模型

以下为逻辑兼容模型，不是引擎类处方：

```text
SpecialScreenControlCorpus {
  sourceMembership[21] {
    recordId
    sourcePath
    executableOwner
    optionalExistingContract
  }

  directStaticBindings[19] {
    recordId
    representativeSymbol
    representativeAddress
    role: control | menu-control | resource-separate-owner
  }

  segaLogo {
    entryRef
    debugSequenceHelperRef
    computesRomChecksum
    configurationAndDebugHandlersPresent
    startEarlyExitPresent
  }

  title {
    entryRef
    scrollLoopCount: 2
    boundedStartPollRef
    resetOrWitchExitKinds[2]
    titleFontLoaderIdentityRef
  }

  witchEntry {
    checkSramResultOrder[2]: d0, d1
    saveFlagsMask: 3
    availabilityCaseOrder[3]: zero, allSet, otherNonzero
    availabilityMasks[3]: 1, 6, 15
    cancelReturnsToTextLoop
    dispatchIndexScale: 2
  }

  witchDispatch[4] {
    dispatchIndex
    targetIdentity
    targetAddress
  }

  witchMenu {
    initialSelectorMask: 15
    cancelResult: -1
    availableBitPositions[4]: 0, 1, 2, 3
    navigationWrapMask: 3
    pageKinds[4]: actions, newSlotNames, loadedSlotNames, difficulties
  }

  suspend {
    entryRefs[2]
    sourceCounterBeforePresentation: 60
    sourceRestartWaitCounter: 600
    startEarlyExitPresent
    resetVectorHandoffPresent
  }

  endingOwnership {
    endingKissPixelFillRef
    endingWitchInitRef
    endGameEntryRef
    endingWitchFallingJewelAndBlinkGroupRef
  }
}
```

模型把 21 成员源语料与 19 直接 H2 证据绑定区分开。它不为专用标题瓦片或 witch 新游戏记录制造特殊画面证据。它还把路线身份与渲染或玩家面向含义分开。

## 跨系统分离

把以下系统保持本合同之外：

- [special-screen-asset-data](../../contracts/special-screen-asset-data.md)：压缩与非压缩资源、调色板、布局、witch 帧、hash 与私有载荷；
- [graphics-service-state](../../contracts/graphics-service-state.md)：解压/显示服务状态及其面向硬件 未知；
- [save-system](../../contracts/save-system.md)：动作选择器、SRAM 变更、校验和、逐动作服务顺序、新游戏生命周期、读档后路由与持久性边界；
- [input-system](../../contracts/input-system.md)：控制器采样、重复状态与等待辅助行为；
- 更宽 gameflow/story 合同：普通路线准入、故事可达性与调用方含义；
- 未来呈现工作：淡入、音频、时序、像素、可访问性、本地化与替换内容。

铁匠运行时研究与本合同无关，此处不提供证据。

## 保真、现代化与版权边界

原版路线兼容要求保留上文命名的受限身份、分支/顺序事实、选择器/页域、源计数器与分离。它不要求现代实现复现原版渲染器、输入循环、存档后端或物理时序。

重制可以替换标题/logo/witch/挂起/结尾呈现、菜单、过渡、时序、可访问性、本地化与内容。那些选择必须保持刻意设计决定，而非关于原版游戏的声称。

原版图形、调色板、布局、文本、音频、截图、存档、轨迹与渲染捕获保持私有/生成版权输入。公开 fixture 与测试只使用符号、地址、计数、小型控制元数据、他处拥有的 hash 与合成状态。

## H4 验收面

重制侧兼容适配器只在自动化测试证明以下内容时声称本合同：

1. 源根成员分母保持恰好 21、直接静态绑定分母保持恰好 19，两条更特定所有者成员记录保持不同；
2. 19 条直接绑定保留十二条新控制记录、一个菜单控制重叠与六条不变资源记录，而不重新分配资源所有权；
3. Sega-logo 条目/调试辅助身份、校验和/配置/调试所有权与 Start 提前退出在场匹配已接受所有者，而不声称控制器时序；
4. 标题条目、双循环区分、受限轮询身份、重置对 witch 退出种类与仅身份 `LoadTitleScreenFont` 边界匹配已接受所有者；
5. witch 入口保留有序 `d0`/`d1` 检查、掩码 `3`、有序可用性用例/掩码、负结果返回、分发倍率与精确四行目标顺序；
6. 通用 witch 菜单控制保留选择器掩码 `15`、取消结果 `-1`、可用位 0..3、导航掩码 `3` 与四个页身份，而逐动作内部保持独立所有者；
7. 挂起保留两个代表条目、源操作数 60 与 600、Start 提前退出在场与重置向量交接，而不提升可见时序；
8. 结尾之吻像素填充所有权、结尾 witch 初始化与 end-game 代表身份，以及结尾 witch 落宝石/眨眼源组身份保持受限事实而非完整渲染时间线；
9. 公开测试不暴露原版图形、布局、文本、音频、存档、轨迹、截图或渲染捕获，所有运行时/呈现偏差分别报告。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 21 成员源语料与 19 直接绑定拆分 | **已确认静态** | `sf2-special-screens-static-v1`（[`special-screens-static-v1.json`](../../../../tests/fixtures/h2/special-screens-static-v1.json)）加精确 research-index 审计 | 两条更特定所有者记录保持本合同之外 |
| Sega-logo 与标题受限控制事实 | **已确认静态** | 专用 fixture 与[Special Screens](../../../research/special-screens.md) source/H1 审查 | 输入节奏、淡入、滚动时序、呈现与正常可达性 |
| witch SRAM 结果/动作页/分发/菜单接缝 | **已确认静态** | 专用 fixture 与[Special Screens](../../../research/special-screens.md) 固定三源审查 | 逐动作选择器、写入、服务、生命周期、持久性与玩家驱动行为是独立所有者 |
| 挂起计数器、Start 分支与重置交接 | **已确认静态** | 专用 fixture 与[Special Screens](../../../research/special-screens.md) 直接源审查 | 墙钟/可见时长、输入节奏、淡入、转移与硬件行为 |
| 结尾操作所有权身份 | **已确认静态** | 专用 fixture 与[Special Screens](../../../research/special-screens.md) 所有者文章 | 完整时间线、VInt 回调、像素、音频、故事可达性与可见一致性 |
| 图形、存档/新游戏行为、输入与呈现 | **独立所有者** | 上文命名的已接受兄弟合同 | 此处不作为证据消费 |
| 现代化、可访问性、本地化与替换内容 | **刻意设计** | 未来产品/内容决定 | 需要独立验收与许可 |

## 复现

```powershell
uv run sf2 h2 special-screens
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/special-screens-static.json` 下。
