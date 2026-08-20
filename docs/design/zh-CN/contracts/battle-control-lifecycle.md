# 战斗控制与战斗员生命周期合同

- **已确认原版行为：** 下文描述的新/恢复战斗入口顺序、回合调度、战斗员准入与清理、Battle 01 区域/行动顺序观察、动作后阵营检查、单步回合后状态处理与静态结果变更。
- **未知原版行为：** 跨进程挂起战斗持久性、升级与 Jaro/退出边界用例、生成重置失败原因、已接受 Battle 01 运行时接缝之外的自然行为、完整多回合状态演化与渲染/音频/输入时序。
- 重制状态：实现无关 Phase 3 合同；尚未选择战斗模拟架构、调度器表示或刻意兼容偏差。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`battle-control-lifecycle.md`](../../contracts/battle-control-lifecycle.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义战术战斗周围的原版控制器状态与有序生命周期接缝。它拥有：

1. 新与恢复的 `BattleLoop` 入口；
2. 回合激活、生成准入与行动顺序交接；
3. 动作后死亡处理、阵营检查与回合后交接；
4. 胜利、败北与 Battle 4 特殊失败结果；
5. 闭合 Battle 01 激活、行动顺序与单步回合后行为的受限运行时观察。

它不拥有玩家或 AI 动作选择、移动/寻路、伤害或法术公式、战斗演出渲染、战役含义或产品级战斗模拟。那些仍属于其自身合同或 **未知**。

可执行证据所有者是：

- `sf2-battle-loop-static-v1`，位于
  `tests/fixtures/h2/battle-loop-static-v1.json`；
- `sf2-battle-control-static-v1`，位于
  `tests/fixtures/h2/battle-control-static-v1.json`；
- `sf2-battle01-turn-order-v1`，位于
  `tests/fixtures/h3/battle01-turn-order-v1.json`；
- `sf2-turn-order-boundaries-v1`，位于
  `tests/fixtures/h3/turn-order-boundaries-v1.json`；
- `sf2-battle01-region-activation-v1`，位于
  `tests/fixtures/h3/battle01-region-activation-v1.json`；
- `sf2-battle01-secondary-activation-v1`，位于
  `tests/fixtures/h3/battle01-secondary-activation-v1.json`；
- `sf2-after-turn-status-lifecycle-v1`，位于
  `tests/fixtures/h3/after-turn-status-lifecycle-v1.json`。

研究所有者是[battle loop 与生命周期](../../../research/battle-loop.md)、[Battle 01 放置](../../../research/battle01-placement.md)，以及[运行时 RNG 与战斗数学](../../../research/runtime-rng-and-battle-math.md)的受限回合/状态部分。

## 规范控制器状态

实现必须保持以下状态域可区分：

| 状态域 | 已确认原版边界 |
| --- | --- |
| 战斗员名册 | 30 个己方槽与 32 个敌人槽；敌人战斗员索引从 128 开始 |
| 战斗区域 | 新战斗标志 90 到 105；独立的新测试区域与敌人激活字段 |
| 行动顺序 | 有序双字节 `(combatant, altered agility)` 条目，以战斗员字节 `0xFF` 终止 |
| 死亡战斗员工作列表 | 显式长度加追加的战斗员索引，与 HP 写入分开处理 |
| 结果 | 带符号 `D4` 结果：胜利 `1`、普通败北 `-1`、Battle 4 特殊失败 `0` |
| 挂起 | 标志 88 加已保存/当前经过秒数与重载战斗状态 |

单个 `battleState` 枚举无法无损表示这些数组、标志、工作列表与返回值。控制器协调它们；它不折叠其子系统所有权。

## 入口与回合调度

### 新战斗

**已确认静态顺序：** 新战斗清除经过秒数、执行战前与开战过场接缝、清除战斗区域标志 90 到 105、治愈合格队伍、初始化己方与敌人战斗状态并加载战斗。战斗间治愈步骤：

- 跳过其他死亡己方角色，但总是处理 Peter (7) 与 Lemon (28)；
- 把当前 HP 与 MP 恢复到最大值；
- 在重建派生属性之前只保留 STUN/POISON/CURSE 掩码 `0x0007`。

这是控制器生命周期规则。它不是治疗法术规则，也不建立任何配置步骤的可见时序。

### 恢复战斗

**已确认静态顺序：** 挂起入口恢复已保存秒数计数器、清除标志 88、清除 AI 记忆、重载战斗状态并恢复单回合循环。AI 记忆重置用 `0xFF` 填充 48 个最后目标字节，并把 48 个记忆字节清零。

这证明代码内交接，而非跨进程 SRAM 持久性、断电恢复、精确恢复 UI 或自然挂起存档生命周期。那些保持 **未知**，并继续受[存档系统合同](../../contracts/save-system.md)约束。

### 回合顺序

**已确认静态顺序：** 每个新回合执行：

1. 敌人激活；
2. 区域过场接缝；
3. 敌人生成准入与动画；
4. 行动顺序生成。

战斗员字节为 `0xFF` 的单回合条目开始下一回合。哨兵与四步顺序是保真事实；帧时序与呈现是否与这些调用重叠未闭合。

## 区域激活与生成准入

**已确认静态：** 新战斗配置清除十六个区域标志。生成准入扫描全部 32 个敌人槽，识别初始化模式 `0x0100`（重生）、`0x0200`（隐藏/区域触发）与 `0x0300`（两者）。成功的重置候选被追加到 `TARGETS_LIST`；重置失败跳过该候选。重置失败原因与完整生成动画时间线保持 **未知**。

**已确认运行时，限于 Battle 01 首回合接缝：** 已接受测试夹具建立：

- 基线己方位置不触发三个区域多边形中的任何一个，而独立的新测试区域字段为 `0b111`；该字段记录已测试多边形，而非已激活多边形；
- 把 Bowie 从 `(8,18)` 移到 `(8,12)` 激活全部三个标志，并在六个自然主区域敌人上只设置主激活位 0，保留其其他激活位；
- 主区域 `NONE`、次区域 2 的受控敌人把激活改为 `0x2060 -> 0x2063`，同时启用主激活与次激活位；其他五个敌人保持仅主激活。

这些观察不建立后期回合清除、自然次区域数据、区域过场时序或全局遭遇节奏。

## 行动顺序构建

**已确认静态模型：** 只有已放置、存活的战斗员进入列表。对每个被准入战斗员，原版：

1. 把当前敏捷掩蔽到低七位；
2. 使用两次范围 `agility >> 3` 的受限 RNG 结果，加第一次、减第二次；
3. 加 `RNG(3) - 1`；
4. 存储战斗员索引与回绕的 altered-agility 字节；
5. 当原始敏捷至少 128 时，添加第二个条目，其基数为 `floor((agility & 0x7F) * 5 / 6)`，并应用受限加/减对而不含最终 `RNG(3) - 1` 项。

固定大小列表按 altered agility 解释为降序带符号字节进行稳定冒泡排序，然后清除当前回合索引。

两个运行时测试夹具只闭合其精确接缝：

| 运行时用例 | 已确认观察 |
| --- | --- |
| 自然 Battle 01，种子 `0x1234` | 九个条目；有序分数为 `0:109`、`2:8`、`1:6`、`128:6`、`133:6`、`129:4`、`130:4`、`131:4`、`132:4` |
| 受控边界，种子 `0x0000` | 死亡己方 2 与未放置敌人 128 缺席；AGI 128 给战斗员 0 分数 0 与 255；AGI 127 给战斗员 1 分数 135，按带符号 `-121` 排序；相等正分数保留插入顺序 |

实现必须在这些接缝保留字节回绕、带符号比较、稳定平局、第二回合构建与 RNG 消耗顺序。状态修改敏捷、多个 AGI >=128 战斗员、超出已观察用例的溢出与其他战斗保持 **未知**。

## 死亡工作列表与战斗员清理

**已确认静态：** `CountRemainingCombatants` 只准入 X 非负且当前 HP 为正的战斗员。它分别返回己方与敌人计数，并在战斗员 0 有零 HP 时强制己方计数为零。

`KillRemainingEnemies` 首先清除死亡战斗员列表，然后扫描已放置的存活敌人。它在把该敌人当前 HP 写为零之前追加每个索引。

`ProcessKilledCombatants` 在工作列表为空时立即返回。否则其持久清理边界：

- 为死亡己方递增败北，或把敌人死亡计入 `BATTLESCENE_FIRST_ALLY`；
- 把 X 与 Y 清除为 `-1`；
- 清除状态并重建派生属性；
- 把关联实体移到 `0x7000,0x7000`。

源码还包含视觉遍。其显示、动画、音频与时序保持 **未知**；适配器不得从持久变更顺序推断它们。

## 动作后与回合后顺序

**已确认静态控制器顺序：** 动作后，循环：

1. 运行败敌过场接缝；
2. 处理被击杀战斗员；
3. 计数双方并在到达结果时退出；
4. 处理行动战斗员的回合后效果；
5. 再次处理被击杀战斗员；
6. 再次计数双方并在到达结果时退出；
7. 战斗继续时推进回合索引。

重复死亡/阵营检查是合同的一部分。重制若声称对该控制器接缝保真，就不得把每个死亡与胜利/败北决定推迟到状态处理之后。

**已确认运行时，受限：** 五次带受控 RNG 接缝的自然 Battle 01 `ProcessAfterTurnEffects` 调用复现单步 MUDDLE 与 SILENCE 过期/继续、确定性 SLOW/ATTACK/BOOST 计数器递减、有序状态写入/消息与一次最终 `UpdateCombatantStats` 归一化。空装备导致瞬态 CURSE 被该最终刷新移除。

该测试夹具证明一次过渡，而非完整多回合生命周期、自然携带状态、每个回合后分支或玩家可见消息时序。详细计数器单位与精确结果仍归[法术解决合同](../../contracts/spell-resolution.md)所有。

## 结果合同

**已确认静态：** 顶层结果与持久变更边界是：

| 结果 | 返回 | 已确认变更 |
| --- | ---: | --- |
| 胜利 | `D4 = 1` | 治愈合格队伍；运行战后过场接缝；清除解锁标志；在战斗偏移 +100 设置完成标志 |
| 普通败北 | `D4 = -1` | 恢复队长 HP；用无符号向下整除把金币减半；获得战斗退出位置 |
| Battle 4 败北 | `D4 = 0` | 完成/升级硬编码战斗路径而非普通败北 |

这些返回码不定义战役含义、存档时序、渲染后果或完整 Jaro、升级与退出特例面。单回合 EGRESS/Angel Wing 也通过另一个所有者使用零退出结果；它不得与 Battle 4 失败原因混为一谈。

## 相邻静态控制边界

已接受控制测试夹具还建立以下支持事实：

- 难度是 `flag[78] + 2 * flag[79]`，产生 0 到 3；
- 战斗 spriteset 子段是尺寸、己方、敌人、区域与 AI 点；实体与区域条目为 12 字节，缺失起始位置返回 `(-1,-1)`；
- 战斗外保留地图音乐；战斗中输入 0/8/14 选择主题 3、40/38 选择主题 1；
- 战斗 VInt 配置清除先前列表，并按该顺序安装地图平面、实体、视图、滚动、精灵、窗口与地图动画；
- 激光辅助拒绝非激光战斗或朝向 `-1`；否则标记到地图边缘并追加占用战斗员。

这些是静态选择器/顺序边界，不是音频时序、渲染 VInt 输出、激光呈现、难度意图或遭遇平衡的证据。源码拥有的调试自循环被排除在可达游戏行为之外。`PrintAllActivatedDefCons` 与该自循环只保持已索引清单/调试报告立足点；两者都不建立玩家可见保真要求。

## 保真与现代化边界

原版保真控制器必须保留：

- 不同的名册、区域、行动顺序、死亡列表、挂起与结果状态；
- 新/恢复入口与回合调用顺序；
- 激活位极性及受限 Battle 01 运行时结果；
- 行动顺序准入、字节/RNG 算术、第二条目、带符号稳定排序与哨兵；
- 回合后处理周围的两个死亡/阵营检查；
- 清理变更顺序与静态结果返回/变更边界。

未来重制可以替换 RAM 布局、使用类型化状态、暴露回合预览、以不同方式动画配置、加速过渡、添加日志、改变失败惩罚或采用其他调度器。那些是产品决定，不是原版事实。每个改变 fixture 可见结果的偏差都需要显式决定与 H4 expected-deviation 记录。

## H4 验收面

未来 H4 适配器应比较：

1. 新/恢复入口轨迹与四步回合轨迹；
2. 两个 Battle 01 测试夹具的区域标志、已测试区域状态与敌人激活位域；
3. 两个运行时用例的精确行动顺序条目，包括重复战斗员与回绕带符号分数；
4. 死亡列表追加/HP/清理状态与两个阵营计数检查点；
5. 使用既有状态测试夹具而非合成聚合的单步回合后状态；
6. 结果码加显式确认的持久变更。

H4 应比较规范状态与有序事件，而非原版 RAM 地址。完整遭遇、战术选择、动画/音频/输入时序、存档持久性、战役过渡与平衡在单独证据或刻意设计之前保持在该适配器之外。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 入口、回合顺序、死亡检查、结果、相邻选择器 | **已确认静态** | `sf2-battle-control-static-v1`（[`battle-control-static-v1.json`](../../../../tests/fixtures/h2/battle-control-static-v1.json)） | 挂起持久性、升级/Jaro/退出边界、渲染/时序 |
| 名册生命周期、生成、计数、清理、回合/状态入口点 | **已确认静态** | `sf2-battle-loop-static-v1`（[`battle-loop-static-v1.json`](../../../../tests/fixtures/h2/battle-loop-static-v1.json)） | 重置失败原因、视觉遍、未解析辅助行为 |
| Battle 01 区域激活 | **已确认运行时，受限** | `sf2-battle01-region-activation-v1`（[`battle01-region-activation-v1.json`](../../../../tests/fixtures/h3/battle01-region-activation-v1.json)）与 `sf2-battle01-secondary-activation-v1`（[`battle01-secondary-activation-v1.json`](../../../../tests/fixtures/h3/battle01-secondary-activation-v1.json)） | 后期回合、其他战斗、自然次区域数据、过场时序 |
| 行动顺序构建 | **已确认运行时，受限** | `sf2-battle01-turn-order-v1`（[`battle01-turn-order-v1.json`](../../../../tests/fixtures/h3/battle01-turn-order-v1.json)）与 `sf2-turn-order-boundaries-v1`（[`turn-order-boundaries-v1.json`](../../../../tests/fixtures/h3/turn-order-boundaries-v1.json)） | 其他 AGI/状态/调用方状态与战斗 |
| 单步回合后状态生命周期 | **已确认运行时，受限** | `sf2-after-turn-status-lifecycle-v1`（[`after-turn-status-lifecycle-v1.json`](../../../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json)） | 多回合自然状态、其他分支、消息时序 |
| 战术、AI/玩家选择、解决、呈现、战役含义 | **未知 / 独立所有者** | 无聚合可执行所有者 | 消费专用合同与未来产品决定 |

## 复现

```powershell
uv run sf2 h2 battle-loop
uv run sf2 h2 battle-control
pwsh ./scripts/Test-H3Battle01TurnOrderFixture.ps1
pwsh ./scripts/Test-H3TurnOrderBoundariesFixture.ps1
pwsh ./scripts/Test-H3Battle01RegionActivationFixture.ps1
pwsh ./scripts/Test-H3Battle01SecondaryActivationFixture.ps1
uv run sf2 h3 after-turn --timeout-seconds 150
uv run sf2 design-contracts test
uv run sf2 research-index test
```
