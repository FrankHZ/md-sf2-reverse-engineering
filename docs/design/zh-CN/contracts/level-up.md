# 升级与属性成长合同

- 状态：**已确认** 核心升级路径、扫描边界、职业等级上限与派生属性刷新
- 证据日期：2026-07-18
- 范围：原版属性增益、等级递增、习得法术阈值与升级结果载荷

> 本文件是 [`level-up.md`](../../contracts/level-up.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 证据所有者

- `tests/fixtures/h3/stat-gain-v1.json` / `sf2-calculate-stat-gain-startup-v1` 拥有 18 次受控 `CalculateStatGain` 调用，覆盖无曲线、随机化成长与最低成长补偿。
- `tests/fixtures/h3/level-up-v1.json` / `sf2-level-up-tort-boundary-v1` 拥有 Kazin 的 MAGE 对照与 Kiwi 的 TORT 边界的自然 `InitializeAllyStats → LevelUp` 调用。
- `tests/fixtures/h3/level-up-boundaries-v1.json` / `sf2-level-up-boundaries-v1` 拥有七次受控自然 `LevelUp` 调用，覆盖投影后成长、两个职业等级上限、两个紧邻前一等级、转职后有效等级、继承法术列表、跨己方角色职业块扫描、最终缺失职业哨兵值，以及一次成功的法术升级。
- `tests/fixtures/h3/level-up-refresh-v1.json` / `sf2-level-up-refresh-v1` 拥有八次受控 Slade 刷新调用，经过 `UpdateCombatantStats`，包括当前/基础分离、完整与部分状态计数器、普通/被诅咒装备，以及原版物品表引用的全部四种特殊攻击概率（会心/二连/反击）效果。用 `uv run sf2 h3 growth-refresh` 独立运行它。
- `tests/fixtures/h3/ally-initialization-prowess-v1.json` /
  `sf2-karna-heal3-prowess-v1` 拥有 Karna 未修改的启动路径，加上覆盖 `InitializeAllyStats` 中完整 HEAL 3 double/counter 高半字节矩阵的十五个受控输入。
- `tests/fixtures/h3/stat-clamp-boundaries-v1.json` / `sf2-stat-clamp-boundaries-v1` 在 Slade 一次自然升级期间拥有九次受控包装器调用，覆盖字节进位饱和、字回绕、普通上限与三次字节下溢钳位。
- `tests/fixtures/h3/enemy-curse-suppression-v1.json` / `sf2-enemy-curse-suppression-v1` 拥有一次带受控被诅咒装备的自然 Battle 01 敌人刷新。
- `tests/fixtures/h3/battle-exp-level-up-v1.json` / `sf2-battle-exp-level-up-v1` 拥有从一次 24 点 EXP 指令经 100 点阈值、一次源码建模的 Bowie/SDMN `LevelUp` 与最终持久战斗员状态的自然 Battle 01 连接路径。
- `tests/fixtures/h3/exp-command-boundaries-v1.json` / `sf2-exp-command-boundaries-v1` 拥有 99 以下/精确 100/EXP 上限 200 的指令边界、基础/转职后上限结果 255，并证明一条指令至多处理一次 100 点阈值。用 `uv run sf2 h3 exp-command` 独立运行它。
- [`../research/ally-growth.md`](../../../research/ally-growth.md) 拥有静态成长曲线与职业块存储合同；[`../research/runtime-rng-and-battle-math.md`](../../../research/runtime-rng-and-battle-math.md) 拥有地址与运行时解读。

## 已确认的原版顺序

对于一个在其职业等级上限之下的匹配己方角色/职业属性块，`LevelUp` 按顺序处理 HP、MP、基础攻击、基础防御与基础敏捷。`NONE` 曲线返回零而不消耗 RNG。每条活动曲线消耗两次范围为 128 的 `GenerateRandomNumber` 调用：

```text
randomizedGain = floor((projection * thisLevelFraction + rng1 - rng2 + 128) / 256)
expectedMinimum = start + floor((projection * cumulativeFraction + 128) / 256)
gain = randomizedGain + (current + randomizedGain < expectedMinimum ? 1 : 0)
```

在应用全部五项增益之后，例程递增等级，计算有效法术学习等级，最多习得阈值恰好等于该有效等级的法术，并刷新派生战斗员属性。`LEVELUP_ARGUMENTS` 是一个七字节结果载荷：

```text
[new level, max HP gain, max MP gain, base ATT gain, base DEF gain, base AGI gain, learned spell]
```

未习得法术编码为 `0xFF`。如果职业已处于其上限，或前向职业块扫描在找到匹配之前到达负控制字节，例程写入无等级结果（`level=0xFF`、零增益、法术 `0xFF`）。`LEVELUP_ARGUMENTS` 是共享的临时状态：断言它的 fixtures 在其所属调用内部捕获它，而不是在周围初始化恢复之后。

在当前等级 30 或更高时，活动曲线不再读取它们的 29 条目成长投影表。例程改用固定的 1.5 点基础，并把存储的投影值当作预期最小值：

```text
randomizedGain = floor((384 + rng1 - rng2 + 128) / 256)
gain = randomizedGain + (current + randomizedGain < projected ? 1 : 0)
```

`NONE` 曲线在这个边界逻辑之前仍然返回零，且不消耗 RNG。

## 已确认的成长投影、等级上限与法术边界

边界 fixture 让原版启动路径自然调用 `LevelUp`，然后只在函数入口控制所选战斗员条目与种子。它不跳转 PC 或写入 CPU 寄存器。七个源码建模用例通过：

- 30 级的 Randolf/GLDT 从存储的投影属性开始。31 级调用走投影后分支，应用 HP/MP/ATT/DEF/AGI 增益 `[2,0,2,1,2]`。
- 转职后 98 级的 Gyan/GLDT 应用 `[2,0,2,1,2]` 并到达 99 级；这与从 99 级开始并走上限退出的 Chaz/WIZ 不同。
- 从 39 级开始的 Slade/THIF 到达基础等级 40。refresh fixture 用下文描述的完整战斗员条目独立覆盖同一边界。
- 基础等级 40 的 Slade/THIF 与转职后 99 级的 Chaz/WIZ 都走上限退出，保持属性与种子不变，并发出 `[255,0,0,0,0,0,255]`。
- 1 级的 Kazin/WIZ 走转职后偏移到有效等级 22。WIZ 的 `$FE` 控制字节复用 Kazin 的第一个属性块法术列表，因此他现有的 BLAZE 1（`0x0B`）变成 BLAZE 3（`0x8B`），载荷以 139 结尾。
- 被迫转成 WIZ 的 Peter 没有自己的 WIZ 块。原版不强制己方角色局部边界；它继续穿过连续表，直到 `0x1EE653` 处 Tyrin 的 WIZ 块。因为该借用块包含 `$FE`，法术查找重定向到 Peter 的第一个法术列表，而该列表为空。
- 被迫转成 SDMN 的 Claude 没有更后面的匹配块。扫描到达最终负哨兵值，走缺失职业退出，保留状态/种子，并发出无等级载荷。

因此原版使用职业 ID 12 作为上限边界（其下 40、从它起 99），但对有效法术学习等级使用下文描述的有缺陷的 class-11 比较。

## 已确认的当前与派生属性刷新

refresh fixture 让 Slade/THIF 从 39 级开始，投影基础属性为 `[HP 42, MP 0, ATT 45, DEF 38, AGI 38]`、当前 HP 7、刻意过期的当前 ATT/DEF/AGI/MOV/抗性/特殊攻击概率（会心/二连/反击）值，以及一件装备的 Short Knife。种子 `0x1234` 产生基础增益 `[2,0,2,1,2]` 与等级 40。`0x95BA` 处的调用位置在 `0x89CE` 进入 `UpdateCombatantStats`；调用、函数入口及其 `0x8A24` 返回都被观察。

原版在提高最大 HP/MP 与基础 ATT/DEF/AGI 的同时保持当前 HP/MP 不变。`UpdateCombatantStats` 随后在重新应用状态与已装备物品效果之前，从新基础与职业值重置当前 ATT/DEF/AGI/MOV/抗性/特殊攻击概率（会心/二连/反击）。在无状态效果时，Slade 结束于当前 ATT 52（基础 47 加 Short Knife 5）、DEF 39、AGI 40、MOV 7、抗性 0 与特殊攻击概率（会心/二连/反击）`0x13`；当前 HP 保持 7。物品、法术、状态与 EXP 保持不变。

第二个用例组合上限计数的 ATTACK、BOOST 与 SLOW（各 `3/8`）与 STUN，然后装备一件 Thieve's Dagger。状态调整使用刷新后的基础值：ATT 47 加 17；DEF 39 加并减 14；AGI 40 加并减 15，然后 STUN 减 5；MOV 7 减 1。装备在状态之后应用，因此匕首最后加 ATT 17 与 AGI 5。最终当前值为 ATT 81、DEF 39、AGI 40 与 MOV 6，同时 `0xFC01` 状态字被保留。这确认了观察到的最大计数器组合的排序与逐步骤向下取整，而不是每一种可能的计数器值。

部分计数器用例使用 ATTACK `1/8`、BOOST `2/8` 与 SLOW `1/8`。从刷新后的基础 ATT/DEF/AGI `47/39/40` 出发，独立的向下取整操作产生当前 `52/44/45`；因此两位字段是量级，而不是存在/缺失标志。

被诅咒用例同时装备一件 Black Ring 与 Short Knife。ATT +10 与 +5 产生当前 ATT 62，然后 Black Ring 使 CURSE（`0x0004`）出现在最终状态字中。因此诅咒在刷新期间再次从当前已装备物品定义派生。

第五个用例让 Slade/NINJ 处于 98 级，装备一把 Ninja Katana。99 级增益为 `[2,2,1,2,2]`；katana 加 ATT 39 并递增二连击特殊攻击概率（会心/二连/反击）。NINJ 基础特殊攻击概率（会心/二连/反击）`0x94` 包含会心 1/8、二连 1/16 与反击 1/8，但 `INCREASE_DOUBLE` 在插入二连 1/8 之前只保留会心半字节。当前特殊攻击概率（会心/二连/反击）变成 `0x24`，无意中把反击重置为 1/32。这是已确认的原版装备缺陷，不是自动的重制默认值。

另外三条自然物品表路径保留相同的 NINJ `0x94` 源码特殊攻击概率（会心/二连/反击）与升级增益。Critical Sword 的 `INCREASE_CRITICAL 1` 产生 `0x95` 同时保留二连/反击；Counter Sword 的 `INCREASE_COUNTER 1` 产生 `0xD4` 同时保留会心/二连；Gisarme 的 `SET_CRITICAL 6` 产生 `0x96` 同时保留两个高半字节字段。它们的 ATT 效果从刷新后的基础 54 独立产生当前 ATT 86、93 与 96。

固定的物品表引用 `INCREASE_CRITICAL` 四次，`INCREASE_DOUBLE`、`INCREASE_COUNTER` 与 `SET_CRITICAL` 各一次。它引用 `SET_DOUBLE` 与 `SET_COUNTER` 零次，尽管两个处理器都存在于分发表中。那两个假设的处理器语义保持源码推断，且不是通过未修改物品定义可达的原版游戏运行时行为。

重制应把最大/当前资源与基础/派生战斗属性建模为独立字段。升级不得仅仅通过把新最大值复制进当前 HP/MP 来治疗，装备效果必须从新基础重算，而不是增量堆叠到过期的派生值上。

## 已确认的属性值钳位边界

一次 Slade/THIF 39→40 调用提供自然增益 ATT +2、DEF +1 与 AGI +2。在相应包装器入口处，钳位 fixture 只替换目标字节。基础 ATT `199+2` 与 DEF `199+1` 都在 200 饱和。基础 AGI 从 `0xE3` 开始：原版分离第 7 位，把低七位 `99+2` 钳位到基础 AGI 上限 100，然后恢复标志，产生 `0xE4`。

同一次刷新装备源码定义的物品到达当前属性值包装器。Running Ring 使 MOV `199+2→200`；Evil Axe 当前 ATT `250+50` 设置字节进位并钳位到 200。受控低输入确认无符号减法钳位而不是回绕：Evil Axe DEF `3-5→0`、Evil Lance MOV `1-2→0` 与 Demon Rod AGI `5-10→0`。观察器在每次匹配包装器入口之前重新应用每个受控输入，然后确认原版 `IncreaseAndClampByte`、`IncreaseAndClampWord`、`IncreaseAndClamp7Bits` 与 `DecreaseAndClampByte` helper 都被到达。这些是验证接缝事实，不是声称这样的极端值在未修改战役中自然出现。

自然 HP +2 包装器也到达 `IncreaseAndClampWord`。与字节 helper 不同，它通过带符号负标志而非进位检测溢出。受控最大 HP `65535+2` 回绕为正 1，因此它绕过 200 上限并存储 1。这是在一个极端验证接缝处的已确认原版行为；保真代码不得静默地用理想的饱和加法替换它。

保真实现必须在基础 AGI 饱和期间单独保留 AGI 标志，并使用源码定义的逐字段上限。它不得依赖宿主语言溢出行为进行减法操作。

## 已确认的敌人诅咒抑制

Battle 01 自然为战斗员 `0x80` 调用 `InitializeEnemyStats`，它到达 `UpdateCombatantStats`。在该入口处，fixture 提供基础 ATT 10、过期的 CURSE `0x0004` 与一件已装备 Black Ring。`ApplyItemOnStats` 在 `0x8AA2` 走其敌人分支。戒指的 ATT +10 仍然应用，产生当前 ATT 20，但 CURSE 以零结束：初始状态掩码移除过期位，敌人分支阻止被诅咒装备再次添加它。

因此原版把物品属性效果与面向玩家的被诅咒状态分开给敌人。重制不得仅仅因为穿戴者是敌人就跳过整个物品定义；它应用受支持的效果并只抑制 CURSE 插入。

## 已确认的战斗奖励入口

连接的战斗 fixture 让 Bowie/SDMN 从 1 级 99 EXP 开始，带源码建模的基础属性与空装备。自然 Battle 01 解决提供一次 24 点指令。`bsc0F_giveExp` 在测试阈值之前应用它，因此存储 EXP 经过 `99 -> 123 -> 23`；只有到那时才调用 `LevelUp`。种子 `0x1234` 产生载荷 `[2,2,0,1,1,1,255]`、等级 2、最大 HP 14 与基础 ATT/DEF/AGI `7/5/5`。当前 HP/MP 保持 `12/8`，仅动作的当前 ATT/AGI 值被刷新后的值 `7/5` 替换。

连接的 fixture 观察一次调用，并在包含 EXP 指令返回处退出。其伴随边界矩阵确认 `75+24 -> 99` 无升级、`76+24 -> 0` 有升级，以及饱和路径 `199+24 -> 200 -> 100` 恰好一次升级。因此一条指令即使在 100 EXP 剩余时也绝不处理超过一个阈值。在 SDMN 40 级与 HERO 99 级，指令仍然减去 100 并调用 `LevelUp`；结果 255 使上限等级不变且 EXP 为 0。随机的 +1/-1 奖励分支与金币保持在本节之外。

## 已确认的 Karna HEAL 3 初始化规则

`InitializeAllyStats` 在重放早期升级之前扫描每个阈值处于或低于己方角色起始有效等级的法术。当自然新游戏初始化到达起始等级 24 的 Karna/PRST 时，HEAL 3 条目（`0x80`，阈值 22）在 `0x967A` 走一个专用分支。它把基础特殊攻击概率（会心/二连/反击）从 `0x03`（会心 1/16、二连 1/32、反击 1/32）改为 `0x13`（会心 1/16、二连 1/16、反击 1/32）。观察器在 `0x969E` 处 `SetBaseProwess` 之后记录该写入；自然用例不执行状态或寄存器变更。

这个特殊分支在初步扫描期间改变特殊攻击概率（会心/二连/反击），但刻意跳过 `LearnSpell`。随后的 `LevelUp` 重放到达有效等级 22，并通过普通路径习得 HEAL 3。因此重制必须保留所得的特殊攻击概率（会心/二连/反击）与法术状态，而不依赖这个两阶段初始化实现。

在同一个验证接缝处，十五次受控运行注入两个 double 位与两个 counter 位的每一种其他合法组合，同时保留 Karna 的会心半字节 `0x3`。加上自然输入，全部十六个高半字节值都被确认。原版把整个半字节当作一个标量：值 0–6 与 8–14 递增，7 保持 7 因为专用守卫拒绝 8，15 在字结果被存储为字节时回绕到 0。代表性转换是 `0x33→0x43`、`0x43→0x53`、`0x73→0x73` 与 `0xF3→0x03`；每种情况下会心半字节保持不变。这不是孤立的“增加二连”规则：跨越半字节边界可以改变 counter 位，最终回绕同时清除 double 与 counter 设置。修正的重制规则必须显式记录那个差异，而不是替换这个原版保真矩阵。

## 已确认的 TORT 边界缺陷

原版比较使用 `class < CHAR_CLASS_LASTNONPROMOTED` 来跳过转职后等级偏移。因为 TORT 等于 `CHAR_CLASS_LASTNONPROMOTED`（11），`InitializeAllyStats` 与 `LevelUp` 都把它误分类为转职后，尽管第一个转职后职业是 12。

对于自然的 Kiwi 启动用例，初始化把有效法术等级从 7 → 27 改变，第一次升级把它从 2 → 22 改变。Kazin 的 MAGE 对照保持 4 与 2。Kiwi 没有 TORT 法术列表，因此已确认的缺陷改变内部阈值但在此场景中不产生习得法术副作用。它的四条活动属性曲线仍然产生增益 `[1,0,1,1,1]`、等级 2 与载荷 `[2,1,0,1,1,1,255]`。

这是原版保真事实，不是自动的重制选择。保真模式可以保留该比较；修正规则模式可以把转职后职业从 12 起分类。项目必须在 H4 把任一种行为当作规范之前显式记录那个选择。

## 未知与未来 H4 用例

**未知** 仍需专用 fixtures 的运行时边界：

- 当前 ATT 递减下溢、剩余字边界与未使用的 long 钳位 helpers；
- 自然引用物品/职业组合之外的关键病变与会心上限输入。

未来的重制成长模块应首先消费同样的九个 fixtures，然后扩展它们，而不是把未经测试的曲线或职业假设嵌入引擎代码。
