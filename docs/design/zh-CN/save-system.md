# 存档系统合同

- **已确认的原版行为：** 静态的两槽位 SRAM 表示、字节交错复制方向、加法校验和/校验顺序、占用标志操作、save/load/copy/delete 辅助序列、女巫菜单选择器/行动路由，以及下文描述的有界进程内 H3 矩阵。
- **未知的原版行为：** 跨进程物理持久化、断电/部分写入结果、已检查字节校验和之外的损坏行为、玩家驱动的 New-game 命名/菜单呈现或输入节奏，以及调用方可见的像素/音频/挂起时序。
- 重制状态：实现无关的合同；进程内服务效果已被观察，而耐久介质行为仍未观察。
- 证据日期：2026-07-29
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/tech-services-static-v1.json` 中的 `sf2-tech-services-static-v1`；`src/sf2tool/h2/services.py`；以及
  `docs/research/technical-services.md`。静态女巫菜单合同是
  `tests/fixtures/h2/special-screens-static-v1.json` 中的 `sf2-special-screens-static-v1`；`src/sf2tool/h2/screens.py`；以及
  `docs/research/special-screens.md`。进程内运行时合同是
  `tests/fixtures/h3/witch-save-actions-v1.json` 中的 `sf2-witch-save-actions-runtime-v1`；`src/sf2tool/h3/witch_save_actions.py`；以及
  `docs/research/special-screens.md`。有界 New-game 运行时合同是
  `tests/fixtures/h3/witch-new-game-lifecycle-v1.json` 中的 `sf2-witch-new-game-lifecycle-runtime-v1`；
  `src/sf2tool/h3/witch_new_game_lifecycle.py`；以及 `docs/research/special-screens.md`。

> 本文件是 [`save-system.md`](../save-system.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的静态合同

有两个逻辑存档槽位。选择器值零寻址槽位 1；任何非零选择器寻址槽位 2。每个槽位把 4,016 个逻辑字节存储为 4,016 个物理存储字节写入。以两字节地址步进，这些写入占据一个保留的 8,032 字节 SRAM 地址区间；该区间不是 8,032 个已存储字节写入。槽位占用标志是 save-flags（存档标志）字段的位 0 与位 1。

`CheckSram` 先验证签名，然后槽位 2，再槽位 1。对于已占用的槽位，它通过交错读取器复制该槽位，把计算出的低字节加法校验和与所选存储校验和比较，并在不匹配时清除占用位。其静态结果是：有效占用槽位为 1，未占用槽位为 0，失败的占用槽位为 -1。签名不匹配会清除全部 8,192 个逻辑 SRAM 字节、写入签名，然后清除存档标志。

存档把战斗员数据复制到所选槽位，存储低校验和字节，然后设置该槽位的占用位。读档把所选槽位复制回战斗员数据，但不本地执行校验和比较。复制加载所选槽位并将其保存到相反槽位；删除只清除所选占用位。该合同刻意区分这些静态辅助操作与耐久介质保证。

## 已确认的进程内运行时矩阵

**已确认：** 一次 BizHawk 2.11.1 / Genesis Plus GX 启动在原版 `CheckSram` 返回后调用原版 `SaveGame`、`LoadGame`、`CopySave` 与 `ClearSaveSlotFlag`。九个直接用例对槽位 1 使用种子 19（存储/计算校验和 71）、对槽位 2 使用种子 20（存储/计算校验和 247）。`LoadGame` 恢复每个选择器的四个投毒并复核的载荷样本。复制选择器 0 以目标选择器 1 与校验和 71 把槽位 1 转移到槽位 2；在恢复第二个来源载荷后，复制选择器 1 以目标选择器 0 与校验和 247 把槽位 2 转移到槽位 1。删除清除占用位 3→2→0 而不改变观察到的载荷样本或存储/计算校验和字节 247。这些是观察值，而非声称 8,032 字节物理地址区间是一个 8,032 字节已存储载荷；每个槽位以两字节地址步进存储 4,016 个物理字节。

**已确认：** 来源标志 88 清除时 Load 到达 `GetSavepointForMap`，作为 30188 处的指令目标与有效目标。标志 88 设置时，它到达 131124 处的指令目标 `j_BattleLoop` 与 146052 处的跳转接口有效目标 `BattleLoop`。来源标签 `flag 88` 被保留；其面向玩家的生命周期含义不被推断。

**未知：** 单进程服务 fixture 不确立跨进程 SRAM 存续、物理断电行为、部分/中断写入恢复、玩家驱动的 New-game 命名/菜单结果、像素、音频、输入节奏或挂起呈现。那些仍是 `docs/research/special-screens.md` 中命名的分组 H3 问题。

## 已确认的 New-game 运行时矩阵

**已确认：** 一次 BizHawk 2.11.1 / Genesis Plus GX 启动在原版 `CheckSram` 返回后保存一个核心状态检查点，并回放四个独立的 New 行动用例。标志前置条件 0、1、2 与 0 进入 page-1 菜单，观察到的选择器/页/可用性为 `1/1/6`、`2/1/4`、`1/1/2` 与 `1/1/6`；注入的 page-1 结果选择槽位 1、槽位 2、槽位 1 与槽位 1。Page-3 注入的难度结果 0/1/2/3 产生标志 78/79 清除/清除、设置/清除、清除/设置与设置/设置。每个用例都调用原版 `SaveGame` 并以 `CURRENT_MAP`/`EGRESS_MAP` 3 与 D0–D4 `3/56/3/3/1` 转移到 `MainLoop`。

**已确认的测试台边界：** 该观察在精确回读证明通过 `M68K BUS` 的相同写入未改变 ROM 指令字节之后使用会话专用 `MD CART` 补丁。它注入两个菜单返回、绕过 `NameAlly` 与 `DisplayText`、为原版配置辅助的 Start 清除分支清除玩家 1 输入，并在文本等待时脉冲 C。因此它不确立玩家在命名或导航菜单时看到或选择什么。fixture 拥有的 4,800 帧截止时间在 120 秒 Python 观察器超时之前记录超时里程碑并以失败退出 BizHawk。

## 重制边界

重制版可以用显式的有效/占用状态、完整性检查与存档复制/删除工作流表示两条独立寻址的存档记录。如果未来运行时工作观察到这些行为，它必须选择自己的原子写入、损坏恢复、平台存储与完成状态政策；本合同不确立其中任何选择。

## 女巫菜单路由边界

**已确认的静态合同：** 女巫行动分发器按此精确顺序有四个 word 表索引：New、Load、Delete、Copy。page-0 行动选择器在 `SAVE_FLAGS & 3` 判定后接收源掩码 1、6 或 15 之一。New 使用由 XOR 该掩码并左移一次形成的 page-1 空闲槽位选择器；Load 与 Delete 使用非反转、左移一次的 page-2 选择器。三者都在写入 `CURRENT_SAVE_SLOT` 之前从返回的选择器减一。Copy 用 3 掩码存档标志、减一，然后在其源码级非零提示结果分支之后调用 `CopySave`。

**已确认的静态合同：** New 执行其命名/配置路径，把 `GAMESTART_MAP` 写入 `CURRENT_MAP`/`EGRESS_MAP`，然后调用 `SaveGame`；随后在分支之前按该顺序设置地图/X/Y/朝向/`d4` 供 `MainLoop` 使用（`GAMESTART_MAP`、`GAMESTART_SAVEPOINT_X`、`GAMESTART_SAVEPOINT_Y`、`GAMESTART_FACING` 与 `1`）。Load 调用 `LoadGame`；来源标志操作数 88 在 `j_BattleLoop` 路径与 `GetSavepointForMap` 路径之间选择，两者都结束于 `alt_MainLoopEntry`。Delete 只在其非零提示结果分支未被采取时到达 `ClearSaveSlotFlag`。这些陈述保留来源分支极性与调用顺序；它们不把面向玩家的含义分配给任一提示结果。

**未知的原版行为：** 此处的静态证据不确立菜单时序、输入去抖、渲染标签、确认 UX、SRAM 耐久性，或这些循环交接的可见后果。
