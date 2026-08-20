# 调试控制流合同

- 状态：**已确认静态调试路由与受限状态写入合同**
- 证据日期：2026-08-09
- 范围：原版战斗测试、配置与调试战斗动作控制面的实现无关重构，不把源标签或辅助身份提升为普通玩家可达性、UI、运行时效果、持久性、音频或战斗解决含义

> 本文件是 [`debug-control-flow.md`](../../contracts/debug-control-flow.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从三个源条目开始：`DebugModeBattleTest`、`CheatModeConfiguration` 与 `DebugModeActionSelect`。它保留其已接受初始化写入、分支谓词、提示边界、相对路线、交接身份与辅助局部栈写入。它在控制传给菜单、战斗、声音、输入、标志、队伍、属性、显示或其他子系统时结束。

- **已确认**：三个源文件包含八个已接受全局条目，在精确 ROM 地址；战斗测试配置写两个切换、把有序 29 非 Bowie 己方身份提交给加入辅助、通过八个具名 Bowie 属性设置器应用源值 99、注册窗口 VInt 指针，并存储精确字节序列 `0..31` 旁的 30 长度通用列表声明；其提示与服务交接保留已接受源顺序，包括在前置结果测试与分支下静态不可达的教堂调用块；配置保留其 Start、Up、完成位与配置切换门加四个有序选择写入；调试战斗动作选择保留七路线相对表、受限操作数、目标选择与对具名栈别名的四个 `seq` 写入。
- **推断**：源标签、注释与路线形状强烈暗示开发者工具。该推断不建立玩家如何到达它或任何提示长什么样。
- **未知**：自然或普通玩家准入；精确控制器采样与帧；提示渲染、已检查分支之外的取消含义与用户可见结果；每个零直接调用方条目是否间接到达；每个交接后的战斗、菜单、声音测试、属性、队伍、标志、存档、物品与动作效果；持久性；显示/音频时序；畸形或注入状态；模拟器与硬件行为；以及重制对调试工具的刻意暴露。

原版调试面是证据，不是公开重制必须发布玩家面向调试菜单的要求。现代实现可以在保留已接受控制与数据边界供原版保真测试的同时，把这些路线隔离在仅开发工具之后。

## 证据所有者与关联审计

`sf2-remaining-core-static-v1`（[`remaining-core-static-v1.json`](../../../../tests/fixtures/h2/remaining-core-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`remaining_core.py`](../../../../src/sf2tool/h2/remaining_core.py)，所属解释是[ROM Header、Window Engine 与 Special Debug Flows](../../../research/remaining-core.md)。本合同只消费 `expected.debugFacts` 与以下三条研究记录：

- `debug.battle-test`；
- `debug.configuration`；
- `debug.battle-actions`。

fixture 总共直接绑定五条研究记录。`core.window-engine` 保留其既有[window-system](../../contracts/window-system.md)合同。`core.rom-header` 保持未关联。两条记录都不因共享执行所有者而获得本合同。

源审计使用固定上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6`。其三个所选源文件包含八个已接受条目：

| 源文件 | 条目 | ROM 地址 |
| --- | --- | ---: |
| `battletest.asm` | `DebugModeBattleTest` | `0x769C` / 30,364 |
| `battletest.asm` | `LoadAllyStatsDecimalDigits` | `0x78BC` / 30,908 |
| `battletest.asm` | `LevelUpWholeForce` | `0x7920` / 31,008 |
| `battletest.asm` | `GetDecimalDigits` | `0x7930` / 31,024 |
| `configurationmode.asm` | `CheatModeConfiguration` | `0x7E3A` / 32,314 |
| `debugmodebattleactions.asm` | `DebugModeActionSelect` | `0x9A9A` / 39,578 |
| `debugmodebattleactions.asm` | `DebugModeSelectTargetEnemy` | `0x9B44` / 39,748 |
| `debugmodebattleactions.asm` | `DebugModeSelectHits` | `0x9B58` / 39,768 |

下文的精确地址、有序身份、边界、标签与分支是公开元数据。原版文本字符串、图形、音频、ROM 字节、活跃 RAM 与捕获帧保持私有或独立所有者材料。

## 战斗测试配置

**已确认静态：** `DebugModeBattleTest` 以写入 `-1` 开始，目标依次是 `DEBUG_MODE_TOGGLE` 与 `SPECIAL_TURBO_TOGGLE`。然后为 fixture 的精确有序 29 标签非 Bowie 名册调用 `j_JoinForce`。本合同保留该有序符号序列，而不推断结果活跃队伍、计数列表同步、地图放置或后续名册状态。那些属于[party-membership state 合同](../../contracts/party-membership-state.md)或保持 **未知**。

源接下来选择 `ALLY_BOWIE`、加载源值 99，并按顺序调用这些设置器：

1. `j_SetBaseAgi`；
2. `j_SetBaseAtt`；
3. `j_SetBaseDef`；
4. `j_SetMaxHp`；
5. `j_SetCurrentAgi`；
6. `j_SetCurrentAtt`；
7. `j_SetCurrentDef`；
8. `j_SetCurrentHp`。

这是静态调用与操作数合同。它不重新定义由[combatant-state access](../../contracts/combatant-state-access.md)拥有的夹断、选择器有效性、派生属性一致性或调用方可视结果。

在本合同之外的其他源交接后，函数注册 `VInt_UpdateWindows`，使用源 `VINT_FUNCTIONS`/`VINTS_ADD` 形式，并调用 `InitializeWindowProperties`。指针与交接身份是已确认静态。中断节奏、回调执行、窗口组合、DMA 与可见呈现在本合同之外。

通用列表配置刻意保留两个不同已接受事实：

- 存储长度声明是 `COMBATANT_ALLIES_NUMBER`，其已接受值为 30；
- 源写入的支撑字节恰好是有序值 `0..31`。

30 长度声明不得仅因在场 32 字节而被归一化为 32。两个尾部存储字节保留为源数据，不提升为计数成员或名册基数规则。

## 战斗测试提示与服务路线

**已确认静态：** `CheatModeConfiguration` 返回后，战斗测试循环保留这些源码形状决定与交接：

1. 数字提示准入 `0..49`；
2. 负结果分支到成员/升级路线；
3. 非负结果保留，同时获得第二个 `0..1` 提示；
4. 非零第二结果通过标志辅助设置所选战斗索引加 `BATTLE_INTRO_CUTSCENE_FLAGS_START`；
5. 随从标志辅助接收 `FLAG_INDEX_FOLLOWERS_ASTRAL`；
6. 战斗地图坐标寻址使用源步长 `BATTLEMAPCOORDINATES_ENTRY_SIZE_FULL`，其已接受值为七；
7. 控制按源顺序交接给 `j_BattleLoop`、`j_ChurchMenu`、`0..100` 商店提示与 `CURRENT_SHOP_INDEX` 写入、`j_ShopMenu`、`j_FieldMenu` 与 `j_CaravanMenu`；
8. 源返回战斗测试提示循环。

该序列只拥有谓词、操作数、写入与交接顺序。它不声称战斗成功完成、教堂或商店动作发生、所选索引是有效内容，或任何服务返回特定结果。战斗生命周期、服务行为、地图坐标含义、标志、呈现与玩家意图保持独立。

负结果路线首先构建属性显示源缓冲，然后调用成员摘要交接。已接受结果结构精确：

- `tst.b d0` 后接非零分支回到战斗提示；
- 因此 fallthrough 值为零；
- 后续 `bpl` 到达 `LevelUpWholeForce`；
- 教堂调用块在分支之间文本存在，但在此前置 `tst`/`bne`/`bpl` 结构下静态不可达。

教堂块保持源清单的一部分。它不是运行时路线、负结果含义、隐藏服务规则或执行死控制流的保真要求。

`LoadAllyStatsDecimalDigits` 循环 30 个己方选择器。对每个选择器，它在 16 字节记录的偏移 0、2、4、6、8 与 10 存储六个打包十进制字。已接受调用序列读取等级、最大 HP、最大 MP、基础攻击、基础防御与基础敏捷；它在对应最大获取器后通过其设置器写当前 HP 与 MP。`LevelUpWholeForce` 单独把 30 个选择器提交给 `j_LevelUp`。格式化、属性变更结果、升级规则、可见成员画面与缓冲生命周期此处不拥有。

## 配置门与写入

**已确认静态：** `CheatModeConfiguration` 除非 Start 输入位设置否则立即返回。Start 设置时，源测试 Up 然后完成存档标志位 7。两个源测试都选择该边时，`bne.w j_SoundTest` 执行直接转移而不压返回地址。本合同只拥有该目标身份与转移形式。它不拥有目标实现、返回行为、声音枚举、可听输出或呈现。

未取声音测试边时，零 `CONFIGURATION_MODE_TOGGLE` 返回。否则源按该精确顺序处理四个选择：

| 文本身份 | 零结果写入 | 非零结果路线 |
| ---: | --- | --- |
| 450 | 写 `-1` 到 `SPECIAL_TURBO_TOGGLE` | 无该写入继续 |
| 451 | 写 `-1` 到 `CONTROL_OPPONENT_TOGGLE` | 无该写入继续 |
| 452 | 写 `-1` 到 `AUTO_BATTLE_TOGGLE` | 无该写入继续 |
| 455 | 设置存档标志位 7 | 清除存档标志位 7 |

这些是源静态结果测试与写入。它们不建立提示按钮含义、初始切换值、互斥、持久性、存档有效性、可访问性、本地化文本或切换名暗示的运行时效果。[special-screen control-flow 合同](../../contracts/special-screen-control-flow.md) 拥有独立 Sega-logo 调试序列与配置处理器边界；它不把该准入证据转移给本合同。

## 调试战斗动作构建

**已确认静态：** `DebugModeActionSelect` 在源范围 `0..6` 获得值、把返回字节与 `-1` 比较，并在比较相等时不写所选动作即返回。否则它写所选字并按该精确有序相对表分发：

| 索引 | 相对目标 | 已接受辅助局部写入形状 |
| ---: | --- | --- |
| 0 | `Attack` | 一个目标字 |
| 1 | `Magic` | 一个打包法术字，然后一个目标字 |
| 2 | `Item` | 物品字、目标字、值字 |
| 3 | `EndTurn` | 该辅助无额外字 |
| 4 | `BurstRock` | 该辅助无额外字 |
| 5 | `Muddle` | 该辅助无额外字 |
| 6 | `PrismLaser` | 写源标记战斗值 |

`Magic` 提示 `1..4`、减一、把结果左移六，并在存储打包字前把它加到 `0..42` 中的第二提示结果。`Item` 获得 `0..127`，然后敌人目标，然后 `0..3`。`DebugModeSelectTargetEnemy` 准入源值 `128..159`。

范围与打包步骤不是法术、物品、目标或战斗语义的验证。它们不建立每个准入数字表示内容、每个嵌套提示后取消被处理，或下游战斗引擎接受结果字。普通[battle-action construction 合同](../../contracts/battle-action-construction.md)、物品与法术数据合同与运行时战斗证据保持独立。

`DebugModeSelectHits` 按源顺序执行四个提示/测试/`seq` 组，并写入这些栈帧别名：

| 顺序 | 源别名 | 栈偏移 |
| ---: | --- | ---: |
| 1 | `debugDodge` | -23 |
| 2 | `debugCritical` | -22 |
| 3 | `debugDouble` | -21 |
| 4 | `debugCounter` | -20 |

本合同保留别名、偏移、顺序与源 `seq` 操作。它不声称特定提示响应、概率、攻击结果、dodge/critical/后续行为或调用方可视效果。[randomness 合同](../../contracts/randomness.md) 拥有独立调试 RNG 证据；它不为此处手动栈写入提供证据。

## 直接调用方清单

受限注释剥离扫描为八个所选条目找到恰好这些外部直接调用方出现：

- `battleactionsengine_1.asm` 各含一个 `DebugModeActionSelect` 与 `DebugModeSelectHits` 直接站点；
- `witchstart.asm` 含两个 `CheatModeConfiguration` 直接站点。

`DebugModeBattleTest`、`LoadAllyStatsDecimalDigits`、`LevelUpWholeForce`、`GetDecimalDigits`、`DebugModeSelectTargetEnemy` 与其他辅助局部条目的已接受直接调用计数在其受限源文件之外为零。零直接计数从不建立死代码或不可达性：计算转移、源局部分支、alternate 链接、修改构建与运行时准入在该清单之外。受限扫描中没有找到外部 longword 指针出现。

## 跨系统分离

调试路线编排许多相邻系统而不拥有它们：

- [party-membership state](../../contracts/party-membership-state.md) 拥有已接受加入与计数前缀行为；
- [combatant-state access](../../contracts/combatant-state-access.md) 拥有属性访问、选择器与夹断边界；
- [global-flag state](../../contracts/global-flag-state.md) 拥有标志存储与包装器结构；
- [window-system](../../contracts/window-system.md) 拥有窗口分配、运动、VInt 组合与 DMA 调用边界；
- [special-screen control flow](../../contracts/special-screen-control-flow.md) 拥有 Sega-logo 与 Witch 控制面；
- [battle-action construction](../../contracts/battle-action-construction.md) 拥有普通动作构建；
- [item-definition data](../../contracts/item-definition-data.md)与[spell-definition data](../../contracts/spell-definition-data.md) 拥有静态身份与打包定义；
- [randomness](../../contracts/randomness.md) 拥有基础/调试 RNG 行为，而非手动调试提示。

菜单内部、服务事务、战斗结果、输入采样、音频、文本资源、呈现、地图数据、存档与故事状态保持其专用所有者或 **未知**。没有相邻研究记录仅因交接被命名而获得本合同。

## 实现无关控制模型

最小逻辑模型是元数据与控制投影：

```text
DebugControlSurface
  evidenceOwner: sf2-remaining-core-static-v1.expected.debugFacts
  sourceCommit
  sourceFiles[3]
  entries[8]:
    symbol
    romAddress

  battleTest:
    initialToggleWrites[2]
    orderedJoinLabels[29]
    bowieStatValue: 99
    orderedStatSetters[8]
    windowVIntPointerIdentity
    genericList:
      declaredLength: 30
      storedBytes: ordered 0..31
    battlePromptRange: 0..49
    negativeRoute: memberAndLevelUpControl
    cutscenePromptRange: 0..1
    battleCoordinateStride: 7
    orderedServiceHandoffs
    statDisplay:
      selectorCount: 30
      recordStrideBytes: 16
      wordOffsets: [0, 2, 4, 6, 8, 10]
    unreachableChurchBlockRetainedAsSourceStructure: true

  configuration:
    startGate
    upAndCompletedBitDirectSoundTestTransfer
    configurationToggleGate
    choices[4]:
      textIdentity
      zeroWrite
      nonzeroRoute

  battleActions:
    topLevelRange: 0..6
    cancelComparison: returnedByte == -1
    orderedRelativeRoutes[7]
    magicPacking
    itemOperands
    enemyTargetRange: 128..159
    orderedHitWrites[4]:
      alias
      stackOffset
      operation: seq

  externalDirectCallerOccurrences
  directCallerZeroesRetainedWithoutReachabilityClaim
```

模型不包含原版字符串、布局、图形、音乐、代码字节、ROM、存档、RAM 转储、轨迹或模拟器状态。公开 fixture 与报告只保留符号身份与结构元数据。私有保真适配器可以从许可或用户提供输入重构源码形状缓冲而不发布其载荷。

## 原版保真与现代化

原版保真测试保留已接受初始化操作数、`30` 声明加存储 `0..31` 字节、源分支结构、提示边界、路线顺序、交接身份与四个栈写入。它不发明普通玩家路线、不执行静态不可达教堂块、不推断下游效果。

现代引擎可以暴露类型化开发命令而非复现原版提示、从发布构建省略工具，并在分发前验证标识符。此类选择是现代化。兼容工具仍应能发出已接受源面向控制轨迹并分别报告刻意偏差。

## H4 验收门

未来调试控制适配器只在以下情况通过本合同：

1. 三个源身份、八个条目身份与精确 ROM 地址保持可追溯；
2. 战斗测试配置保留两个切换写入、有序 29 加入标签、源值 99、八个设置器身份与窗口 VInt 指针交接，而不声称其运行时结果；
3. 通用列表保留 30 声明长度与全部 32 个存储字节 `0..31`，不归一化任一事実；
4. 战斗测试提示范围、过场标志操作数、七字节坐标步长与服务交接顺序保持可复现，而不导入下游战斗、菜单或服务行为；
5. 成员结果分支把教堂块保持为静态不可达源结构，绝不要求它执行；
6. 配置保留 Start/Up/完成位/配置切换门、直接 SoundTest 转移身份、四个选择身份与精确写入，而不指定 UI 或持久性；
7. 七条相对动作路线、受限操作数、魔法打包、目标范围与四个有序栈写入保持可复现，而不声称战斗效果；
8. 直接调用方出现与零保持精确，而零计数从不变成不可达性声称；
9. 关联边界保持恰好三条 `debug.*` 记录；`core.rom-header`、`core.window-engine` 与所有相邻记录保持语义不变；
10. 公开工件只包含元数据与合成状态，绝不包含 ROM、源载荷、文本、图形、音频、活跃内存、存档、轨迹或模拟器状态内容。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 三个源、八个条目、标签、地址与直接调用方清单 | **已确认静态** | `sf2-remaining-core-static-v1`（[`remaining-core-static-v1.json`](../../../../tests/fixtures/h2/remaining-core-static-v1.json)） | 间接可达性、普通准入、修改构建 |
| 战斗测试初始化、通用列表拆分、提示、分支结构与交接 | **已确认静态** | 同一 `expected.debugFacts` 所有者 | 队伍/属性/战斗/菜单效果、UI、时序、持久性 |
| 配置门、直接 SoundTest 转移与四个选择写入 | **已确认静态** | 同一 `expected.debugFacts` 所有者 | 输入帧、提示含义、声音实现、存档结果 |
| 七条动作路线、操作数打包、目标范围与四个栈写入 | **已确认静态** | 同一 `expected.debugFacts` 所有者 | 嵌套取消、动作有效性、战斗解决、呈现 |
| 开发者工具用途 | **推断** | 源标签、注释与路线形状 | 玩家面向产品意图与发布策略 |
| 运行时行为与可见体验 | **未知 / 独立所有者** | 未来分组运行时证据与相邻合同 | 不得从静态控制推断端到端行为 |

## 复现

```powershell
uv run sf2 h2 remaining-core
uv run sf2 design-contracts test
uv run sf2 verify
```

生成 JSON 保留在忽略的 `local/derived/remaining-core-static.json` 下。
