# 物理交战解决合同

- 合同版本：`0.1`
- 范围：普通物理攻击、MUDDLE（混乱）判定谓词、成功的闪避、会心一击伤害、二连击、反击、HP 反应回放、击杀 EXP 等级，以及 Battle 01 的 EXP 入账
- 证据状态：**已确认子集**；不完整的系统保持 **未知** 且不在本文档中作默认假设
- 证据所有者：[`runtime-rng-and-battle-math.md`](../../research/runtime-rng-and-battle-math.md)

> 本文件是 [`combat-resolution.md`](../combat-resolution.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

本文档把已复现的原版行为转化为实现无关的合同。它不规定引擎、UI、动画时序或资源格式。重制版可以用不同方式实现内部细节，但原版保真的规则适配器必须为已提交的 fixtures 产生相同的有序事实。

## 合同边界

该子系统接收一份战斗状态快照、一个物理行动请求，以及一个确定性的 RNG 来源。它返回一条包含以下内容的有序解决记录：

- 尝试的攻击及其类型（`first`、`second` 或 `counter`）；
- 闪避判定，以及在未闪避时每个整数伤害阶段；
- 用于构建战斗演出期间的临时 HP 变更；
- 有序的反应（reaction）与 EXP 指令；
- 指令回放后的持久 HP 与 EXP；
- 按消耗顺序排列的 RNG 调用，以 `(range, result)` 成对表示。

该合同刻意同时暴露临时构建状态与持久回放状态。原版在播放生成的指令之前恢复已保存的 HP；把该恢复当作治疗处理会产生错误的最终状态。

## 必需输入

| 输入 | 必需行为 |
| --- | --- |
| 战斗员 | 当前/最大 HP、当前 ATT/DEF、等级、移动类型、特殊攻击概率字段、阵营、位置，以及活跃规则所需的状态 |
| 行动 | 行动者、目标与攻击类型；类型 2 标识已确认半伤规则下的反击 |
| 地形 | 以解码后的规则值表示的目标地形效果设置，而非显示百分比 |
| 战斗规则 | 战斗标识与 EXP 修正，如 Battle 01 的减半规则 |
| RNG | 一个有状态的 `next(range)` 操作，其调用严格按原顺序保留 |

数据消费者不得从呈现资源推断缺失的特殊攻击概率、移动、状态或地形值。这些值属于规范化的战斗数据或显式的行动上下文。

## 已确认的解决管线

### 1. 在计算伤害之前决定闪避

**已确认：** 成功的闪避会完全绕过伤害计算例程。在 airborne-target（空中目标）fixture 中，一个非弓手的陆基攻击者攻击悬浮目标时选择范围 8；掷出 0 即成功。未发生伤害调用，双方战斗员均保持 100 HP。

非闪避链对首击、二击与反击分别观察到掷出 `7/8`、`7/8` 与 `31/32`。这确认了这些场景的范围，以及非零掷出会继续进入伤害。它尚未定义每一个移动/武器资格分支。

### 1a. 区分 MUDDLE 控制回合与二级混乱

打包的 MUDDLE 字段有两个独立的消费者。`ExecuteIndividualTurn` 把任何非零的 `0x0030` 计数器字段视为 AI 控制。`IsCombatantConfused`（其输出喂养阵营翻转的目标选择与受限的 AI 选择）只在两个条件同时成立时返回一：

```text
confused = (status & 0x0030) != 0 and (status & 0x0008) != 0
```

已确认的 H3 真值表对状态字 `0x0000`、`0x0030`、`0x0008` 与 `0x0038` 返回 `0,0,0,1`。因此 MUDDLE 1 可以把军团成员交给 AI 而不使二级混乱谓词成立；单独的二级混乱标志同样不足。重制版不得把“AI 拥有本回合”与“翻转友/敌语义”合并成一个布尔值。

行动守卫在目标选择之后消费最终的攻击者/目标配对。对于己方攻击者，瞄准 Bowie 会无 RNG 地返回无行动；瞄准另一个非 Bowie 的己方成员允许该行动且无 RNG。自我瞄准单独消耗 `RNG(2)`：掷出 0 允许行动，掷出 1 返回无行动。敌方分支与此完全镜像：敌方 128（第一个敌方槽位，通常是 Boss）受保护；不同的敌方目标继续；自我瞄准掷出 0/1 产生行动或无行动。因此受保护目标测试在两侧都先于身份/随机测试。

判定谓词 fixture 拥有阵营翻转资格，而行动守卫 fixture 拥有这些最终的配对检查。返回值为 1 的无行动会走原调用方的非零分支，并把 `BATTLEACTION_MUDDLED`（5）写入 `CURRENT_BATTLEACTION`；该转换是同一个 fixture 的一部分。自然的阵营翻转目标枚举、可用行动过滤，以及零结果延续到所选攻击/物品/法术，仍是独立的行动选择合同。

### 2. 用整数运算计算基础物理伤害

**已确认：** 从以下开始：

```text
damage = max(currentAttack - currentDefense, 1)
```

按向零截断的方式应用目标地形效果设置：

| 设置 | 运算 |
| --- | --- |
| 0 | `damage = floor(damage * 256 / 256)` |
| 1 | `damage = floor(damage * 230 / 256)` |
| 其他 | `damage = floor(damage * 205 / 256)` |

如果目标在飞行或悬浮，且攻击者是 brass gunner、archer、centaur archer 或 stealth archer，则在地形削减后加 `floor(damage / 4)`。已验证向量为 `ATT 99 - DEF 20 = 79`，然后 `floor(79 * 205 / 256) = 63`，再 `63 + floor(63 / 4) = 78`。

原版保真适配器中不应出现浮点百分比。顺序与每个截断边界都是可观察行为。

### 3. 应用已确认的会心一击修正

**已确认对特殊攻击概率定义 0 与首击：** 范围 32 且掷出 0 即成功，并加上当前伤害的一半：

```text
damage = damage + floor(damage / 2)
```

已验证向量把 78 变为 117。已提交用例之外的其他会心一击定义，以及二击/反击上的会心一击资格，仍在本合同版本之外。

### 4. 在 spread（散布）之前应用反击半伤

**已确认：** 攻击类型 2 在方差之前把伤害减半。已验证的反击以 10 进入伤害应用，在它的 spread 调用之前变成 5。

### 5. 应用两次掷出的向下 spread

**已确认：** 对观察到的物理路径：

```text
range = floor(damage / 8) + 1
damage = damage - rng.next(range)
damage = damage - rng.next(range)
```

两次调用都使用在任一减法之前派生的范围。已验证向量包括 `117 - 0 - 0 = 117`、`24 - 3 - 3 = 18` 与 `5 - 0 - 0 = 5`。

### 6. 钳位临时 HP 并追加反应

**已确认：** 从临时当前 HP 中减去最终伤害并钳位到零。按攻击顺序追加一条带符号的负 HP 变更反应。致命 fixture 使 HP `100 -> 0` 并记录 `-117`；非致命链记录敌方 `-18`、敌方 `-18`，然后己方 `-5`。

### 7. 按原版顺序构建后续攻击

**已确认对有效的相邻 fixture：** 成功的二连与反击决定产生：

```text
first attack -> second attack -> counterattack
```

第二击读取目标已削减的临时 HP。反击反转行动者与目标并使用攻击类型 2。**已确认：** 目标死亡会拒绝两个后续攻击。致命 fixture 在自然首击设置 `targetDies` 之后、于验证边界提供真实的 double/counter 开关；返回时每个均为 false，且只有一次伤害计算。成功的链另行拥有自然的特殊攻击概率/RNG 决定。同侧与特殊敌人的自然行动产生尚未泛化；调用方不得把这一条成功 fixture 视为每个请求的后续攻击都有效。

**已确认：** 当目标无法够到原行动者时，原本合格的反击也会被拒绝。range fixture 保持目标存活、禁用二连击、提供一个真实的 counter 开关，并在原验证器执行之前移动行动者使曼哈顿距离为 25；反击返回 false 且不再发生伤害调用。

**已确认：** 睡眠同样会拒绝原本合格的反击。sleep fixture 保持目标存活且相邻、禁用二连击、提供真实的 counter 开关，并在验证前立即把状态字 `0x00C0` 写入文档记录的战斗员状态偏移。原版状态 getter 识别出睡眠，反击返回 false 且不再发生伤害调用。

**已确认：** 眩晕（stun）独立拒绝原本合格的反击。stun fixture 使用相同的存活相邻目标与强制有效反击接缝，但写入状态字 `0x0001`。原版状态 getter 在独立的睡眠检查之后识别出眩晕，清除反击且不进行第二次伤害计算。

**已确认（验证接缝）：** 同侧标志拒绝反击。fixture 观察到自然的敌对侧标志为 false，然后在原验证器执行前只把 `targetIsOnSameSide` 设为 true。在存活相邻目标与强制有效反击下，验证器清除该开关且不进行第二次伤害计算。后续 fixture 仍应演练自然的同侧行动产生器；此结果拥有验证器分支，而非目标选择可达性。

**已确认（验证接缝）：** Burst Rock 无法执行反击。fixture 观察到原目标为敌方索引 39（Gizmo），然后在验证前只把其战斗员敌方索引字段改为 32（Burst Rock）。原版 `GetEnemy` 路径清除强制有效的反击且不进行第二次伤害计算。这确立了该排除的方向：原攻击目标就是潜在的反击者。

**已确认（验证接缝）：** 其余硬编码的敌人排除是有方向的。Kraken Head（87）、Prism Flower（93）与 Zeon Guard（38）与 Burst Rock 一样，在它们是原攻击目标时无法执行反击。Taros（88）在相反的指针上检查：当 Taros 是原攻击者时，目标不能反击它。每个矩阵用例都从相同的存活相邻攻击开始，在验证前立即改变相关战斗员/敌人身份，清除强制有效的反击，并保留一次伤害计算。Kraken Arm（59）不在此表中；名称相似的 Kraken Head 在表中。

**已确认（验证接缝）：** 二连击验证器在一个真实开关之后恰好有三个拒绝输入：目标死亡、被 muddle 的行动者与同侧目标。死亡由致命 fixture 拥有。二连验证矩阵保持目标存活，观察到两个剩余标志自然为 false，然后独立强制 `muddledActor` 或 `targetIsOnSameSide` 为 true。每个用例在 `0xA486` 清除二连并保留一次伤害计算。这完成了验证器本身；两个受控标志的自然行动产生仍然独立。

### 8. 恢复快照，然后持久地回放指令

**已确认：** 战斗演出构建在播放之前恢复已保存的 HP 快照。命令解释器随后按列表顺序应用带符号的反应且零钳位。对于链，恢复的 HP 为 `ally=200, enemy=200`；回放结束时为 `ally=195, enemy=164`。对于致命攻击，恢复的敌方 HP 100 在回放 `-117` 后变成持久 HP 0。

重制版可以在内部避免实际恢复，但其可观察的解决轨迹与最终状态必须保留这个两阶段合同，使动画/UI 消费者不会意外持久化临时快照。

### 9. 发放已确认的 Battle 01 EXP

**已确认对致命同等级 fixture：** 伤害与击杀 EXP 以每次行动 49 的上限累积。Battle 01 以整数截断把 49 减半为 24。两次随后的 `RNG(16)` 掷出 4 使入账不变，指令回放使行动者 EXP `0 -> 24`。

**已确认对连接的 99-EXP fixture：** 回放首先加上同样的 24 点指令（`99 -> 123`），然后减去一个 100 点阈值（`123 -> 23`）并调用一次 `LevelUp`。源码建模的 Bowie/SDMN 1 级用例以属性增益 HP/MP/ATT/DEF/AGI `[2,0,1,1,1]` 达到 2 级。当前 HP/MP 不被治疗；派生 ATT/DEF/AGI 从新基础刷新。最终快照在 `bsc0F_giveExp` 返回时、升级结果载荷被战斗演出路径消费之后获取。

**已确认对击杀 EXP 矩阵：** `battlesceneScript_GetKillExp` 把行动者的有效等级与目标的存储等级比较。差小于 3 与恰好 2 都返回 50 EXP；恰好差 3/4/5/6 返回 40/30/20/10，7 及以上返回零。转职后的行动者职业在减法前给其存储等级加 20，因此 HERO 1 级对 18 级走差 3 并返回 40。每个矩阵行从相同的自然 Battle 01 物理攻击开始；内存内核心状态回放只避免重复启动/UI 路径。

**已确认对最终入账随机化：** Battle 01 中面向敌人的入账先把累积值右移一位。两次有序的 `RNG(16)` 掷出随后在第一次为零时独立加一、第二次为零时独立减一。因此累积 49 对掷出 `4/4` 变成 24、`0/3` 变成 25、`14/0` 变成 23、`0/0` 变成 24。零累积器在掷出 `4/4` 时被钳位到最低指令入账 1。fixture 为全部五行回放一个自然的行动前核心状态，而不替换原控制流。

**已确认对 EXP 指令持久性：** 存储的 EXP 加指令金额饱和于 200。值 75+24 与 76+24 在等级/EXP `1/99` 与 `2/0` 结束。饱和用例 199+24 达到 200，减去一个阈值，调用一次 `LevelUp`，并在 `2/100` 结束；该指令不会在第二个阈值上循环。在基础/转职后上限，76+24 仍减去 100 并调用 `LevelUp`；结果 255 使等级 40/99 不变且最终 EXP 为 0。观察器使用原指令的位 15 呈现标志来跳过文本，同时把其掩码后的算术金额保持在 24。

**已确认对敌方金币数据：** 击杀路径按敌方 ID 索引一张 big-endian 字表。规范范围有 103 个值，与 103 个敌方定义对齐。其后 69 字的 ROM 尾部被显式标记为未使用，不得成为敌方行。敌方索引 0 提供 10 金币；DESOUL fixture 确认三个成功目标累积并回放 30。

**已确认对金币持久性：** 加法使用无符号 32 位中间值并把存储金币上限钳位到 9,999,999。普通 0+30 产生 30；恰好上限、超上限与 32 位进位输入都产生 9,999,999。兼容性实现必须同时保留数值比较与进位到上限分支，即使其宿主整数类型无法自然溢出。

**已确认对敌人物品掉落数据与来源政策：** 30 条记录把战斗 + 敌方实体 + 持有物品映射到持久标志 0-29。三个具名 Boss 武器使用仅零的 `RNG(32)` 门；其余 27 条记录在它们的查找与持有前置条件通过后是确定的。该例程设置一次性标志、移除敌方物品、尝试把它交给一个存活的行动者，并且只在直接交付不可用时把珍稀物品路由到特卖。重制版数据加载器必须在 `0xFFFF` 终止符处停止并保留全部四个记录字段。

**已确认对核心物品掉落运行时行为：** 珍稀掷出 8 失败而不设置标志或移除物品；珍稀掷出 0 设置标志、移除物品并把它交付到空的行动者物品栏。如果该标志已被设置，珍稀掷出仍被消耗，但移除与交付被跳过。一个普通的 Short Rod 行在交付时不消耗掉落 RNG。在满物品栏或死亡行动者下，移除仍会发生：Taros Sword 递增其打包的特卖计数，而非常稀的 Short Rod 被丢弃。原版保真代码必须保留此顺序：查找/持有、适用时珍稀 RNG、测试并设置（test-and-set）标志、移除，然后接收者路由。

**已确认对特卖饱和：** 每个物品计数占用一个四位 nibble。失败的 Taros Sword 交付把计数 14 递增到 15；已有 15 保持 15。兼容性代码必须按物品钳位，而不是进位到相邻的打包 nibble。

其他战斗修正、金币减法与非战斗金币调用方仍在本合同版本之外。

## 参考适配器形状

只要呈现等价的测试接缝，任何语言或引擎都可以使用：

```text
resolvePhysicalAction(
  initialBattleState,
  physicalAction,
  deterministicRng,
  battleRules
) -> {
  attacks[],
  temporaryState,
  commands[],
  persistentState,
  rngTrace[]
}
```

`attacks[]` 必须保留 fixture 使用的每个整数中间量：闪避范围/掷出、基础伤害、spread 前伤害、spread 范围/结果、最终伤害，以及前后的 HP。生产构建可以编译掉丰富的轨迹，但 H4 测试需要它。

## H4 Fixture 矩阵

现代适配器必须直接消费已提交的 JSON，或通过一个薄的共享加载器。它不得把预期数值复制到单独的引擎特定测试套件中。

| Fixture ID | 文件 | 必需的一致性 |
| --- | --- | --- |
| `sf2-physical-damage-land-archer-v1` | `tests/fixtures/h3/physical-damage-v1.json` | 基础、地形削减、对空加成、结果 |
| `sf2-physical-damage-application-v1` | `tests/fixtures/h3/physical-damage-application-v1.json` | 会心一击、spread、HP 钳位、EXP 累积器 |
| `sf2-battle-scene-replay-v1` | `tests/fixtures/h3/battle-scene-replay-v1.json` | 快照恢复、EXP 修改、持久回放 |
| `sf2-battle-exp-level-up-v1` | `tests/fixtures/h3/battle-exp-level-up-v1.json` | 99 + 24 阈值、一次 `LevelUp` 调用、载荷、持久等级/属性/EXP |
| `sf2-kill-exp-level-difference-v1` | `tests/fixtures/h3/kill-exp-level-difference-v1.json` | 有效等级减法；50/40/30/20/10/0 档位；转职 +20 偏移 |
| `sf2-award-exp-randomization-v1` | `tests/fixtures/h3/award-exp-randomization-v1.json` | Battle 01 减半；有序 +1/-1 RNG 分支；抵消；最低 1 |
| `sf2-exp-command-boundaries-v1` | `tests/fixtures/h3/exp-command-boundaries-v1.json` | EXP 上限 200；一次阈值/调用；残余 EXP 100；基础/转职后上限结果 255 |
| `sf2-enemy-gold-v1` | `tests/fixtures/h2/enemy-gold-v1.json` | 103 个使用的字条目；69 字未使用尾部边界；源码/ROM 一致性 |
| `sf2-enemy-item-drops-v1` | `tests/fixtures/h2/enemy-item-drops-v1.json` | 30 条四字节记录；标志 0-29；三个 `RNG(32)` 物品；`0xFFFF` 终止符 |
| `sf2-gold-boundaries-v1` | `tests/fixtures/h3/gold-boundaries-v1.json` | 普通/恰好/超上限加法；32 位进位；9,999,999 上限 |
| `sf2-enemy-item-drop-behavior-v1` | `tests/fixtures/h3/enemy-item-drop-behavior-v1.json` | 珍稀失败/成功；重复标志；保证转移；满/死亡接收者路由；特卖 14/15 饱和 |
| `sf2-attack-chain-double-counter-v1` | `tests/fixtures/h3/attack-chain-v1.json` | 攻击顺序、闪避落空、二连击/反击、半伤、反应 |
| `sf2-muddle-action-guard-both-sides-v1` | `tests/fixtures/h3/muddle-action-guard-v1.json` | Bowie/首个敌人保护；不同目标直通；己方/敌方自我瞄准 `RNG(2)` 结果；最终无行动到行动 5 的转换 |
| `sf2-muddle-confusion-truth-table-v1` | `tests/fixtures/h3/muddle-confusion-v1.json` | 仅计数器、仅二级标志、两者皆无与合并真值表；混乱要求 `0x0038` |
| `sf2-successful-airborne-dodge-v1` | `tests/fixtures/h3/dodge-v1.json` | 成功闪避、零伤害调用、HP 不变 |
| `sf2-lethal-followup-validation-v1` | `tests/fixtures/h3/lethal-followup-v1.json` | 目标死亡对强制有效二连/反击开关的拒绝 |
| `sf2-counter-range-validation-v1` | `tests/fixtures/h3/counter-range-v1.json` | 超范围对强制有效反击开关的拒绝 |
| `sf2-counter-sleep-validation-v1` | `tests/fixtures/h3/counter-sleep-v1.json` | 睡眠目标对强制有效反击开关的拒绝 |
| `sf2-counter-stun-validation-v1` | `tests/fixtures/h3/counter-stun-v1.json` | 眩晕目标对强制有效反击开关的拒绝 |
| `sf2-counter-same-side-validation-v1` | `tests/fixtures/h3/counter-same-side-v1.json` | 在原反击验证接缝处同侧标志拒绝 |
| `sf2-counter-burst-rock-validation-v1` | `tests/fixtures/h3/counter-burst-rock-v1.json` | Burst Rock 作为潜在反击者的拒绝 |
| `sf2-counter-special-enemies-validation-v1` | `tests/fixtures/h3/counter-special-enemies-v1.json` | 有方向的 Taros、Kraken Head、Prism Flower 与 Zeon Guard 排除 |
| `sf2-double-validation-gates-v1` | `tests/fixtures/h3/double-validation-v1.json` | 被 muddle 行动者与同侧对强制有效二连开关的拒绝 |

H4 仅在有序 RNG 消耗、中间整数、指令顺序与持久状态匹配时通过。渲染、动画时长、输入手感与视听资源不是 H4 断言。

## 原版保真与现代化

默认兼容性适配器保留已确认的算术、RNG 顺序、钳位、攻击顺序与回放结果。现代化只有在决策记录指明面向玩家的理由并添加独立的 expected-deviation（预期偏差）测试之后，才可以有意替换其中任何一项。现代行为绝不得覆盖原版 fixture，也不得被描述为新发现的原版规则。

## 未知 / 合同扩展门禁

以下内容对一般实现保持 **未知**，并阻塞声明物理交战完成：

- 所有闪避资格与范围选择分支；
- 已确认验证接缝之前的自然 MUDDLE 阵营翻转目标/行动选择、同侧可达性与特殊敌人行动可达性；
- 已验证用例之外的其他会心一击定义，以及二击/反击上的会心一击；
- 抗性、状态效果、法术伤害、治疗、吸取与即死路径；
- 额外的 spread 种子与所有输入范围内的确切下界行为；
- 其余 EXP 修正与金币减法/非战斗调用方；
- 已确认 HP 反应与 EXP 入账子集之外的其他战斗演出指令类型。

每项扩展都必须先增加或扩展 H3 证据、更新本合同，然后成为 H4 fixture。引擎工作必须把不受支持的分支表示为显式的不完整行为，而不是静默发明一条方便规则。
