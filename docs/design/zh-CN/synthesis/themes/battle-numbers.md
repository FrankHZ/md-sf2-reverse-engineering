# L2 · 核心战斗数值与成长速查

为什么站上平原只能把巨鼠的伤害从 5 压到 4，而不是直接打八五折？为什么威力 15 的治疗（HEAL）1 只补了 5 HP，却先算出 10 点经验？为什么敏捷更高的队员还是可能后出手？为什么升级提高了 HP 上限，却不顺带回满当前血量？

本页把已接受的原版规则和受控运行实测整理成一份可直接手算的数值速查，按玩家关心的五个问题展开：属性刷新、物理伤害与地形、法术抗性与治疗、敏捷与出手顺序、经验与升级阈值。每一项都写明单位、适用条件、整数取整顺序和未知边界，并链接到对应的权威规则与研究来源；不重复展开全部法术/状态生命周期表，不做平衡预设，也不把重制行为反推成原版证据。

## 1. 基础属性、当前属性、状态与装备叠加顺序

战斗中直接生效的面板数值是**当前有效属性（派生属性）**，每次刷新（`UpdateCombatantStats`）都会从基础与职业值重算，而不是在旧面板上反复累加：

| 属性层级 | 保存什么 | 避免算错的关键区分 |
| --- | --- | --- |
| **基础属性与职业值** | 存储的最大 HP/MP、基础 ATT/DEF/AGI、职业 MOV、抗性与特殊攻击概率 | 仅在升级或转职时改写；**基础 ATT 不含已装备武器加成**。 |
| **当前 HP / MP** | 战场上的实时血量与法力 | 升级和属性刷新**不会改动当前 HP/MP**——提高上限不会顺带回血回蓝。 |
| **当前有效属性** | 实际代入战斗公式的 ATT/DEF/AGI/MOV/抗性/特殊攻击概率 | 按“基础/职业值 → 状态修正 → 已装备武器与戒指”顺序重算。读取已刷新好的面板或参考队伍数据时，**不要再次叠加武器 ATT**。 |

**已确认的刷新顺序**（[属性刷新规则（英文源）](../../../contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh)）：

1. 先按最新基础值与职业值重置当前 ATT、DEF、AGI、MOV、抗性与特殊攻击概率。
2. 按**刷新后的基础值**独立计算各状态档位的整数向下取整修正（状态计数 `counter` 为 `1`、`2` 或 `3`，代表档位大小，不是简单的有/无开关）：
   - **强攻（ATTACK）**：`当前 ATT += floor(基础 ATT × counter / 8)`
   - **强化（BOOST）**：`当前 DEF += floor(基础 DEF × counter / 8)`，`当前 AGI += floor(基础 AGI × counter / 8)`
   - **迟缓（SLOW）**：`当前 DEF -= floor(基础 DEF × counter / 8)`，`当前 AGI -= floor(基础 AGI × counter / 8)`
   - **眩晕（STUN）**：`当前 AGI -= 5`，`当前 MOV -= 1`
3. **最后**叠加已装备武器与戒指的属性加成（例如斯莱德 THIF 基础 `ATT 47 / DEF 39 / AGI 40` 在满层 ATTACK/BOOST/SLOW + STUN 且装备 Thieve's Dagger `ATT +17, AGI +5` 时，刷新为 `ATT = 47 + 17 + 17 = 81`、`DEF = 39 + 14 - 14 = 39`、`AGI = 40 + 15 - 15 - 5 + 5 = 40`、`MOV = 6`）。己方佩戴诅咒装备会在刷新时重新附上诅咒（CURSE）；敌人佩戴诅咒装备时仍享受属性加成，但会抑制诅咒状态写入（[敌人诅咒抑制（英文源）](../../../contracts/party/level-up.md#confirmed-enemy-curse-suppression)）。

**已确认的数值钳位（上下限截断）**（[钳位边界（英文源）](../../../contracts/party/level-up.md#confirmed-stat-clamp-boundaries)）：基础 ATT/DEF 上限 **200**；基础 AGI 低 7 位上限 **100**（同时保留第 7 位的二次出手标记）；当前属性减法截断到 **0**（不下溢回绕）。极端边界（如 `IncreaseAndClampWord` 在 `65535 + 2` 时回绕成 `1`，以及 Ninja Katana 的 `INCREASE_DOUBLE` 覆盖反击位缺陷）详见[升级与成长规则](../../contracts/party/level-up.md)。**未知：**当前 ATT 减法下溢与未覆盖的辅助钳位路径尚无对应运行实测。

## 2. 物理伤害、地形防御与追击

一次普通物理攻击严格按整数顺序分步结算，原版内部不存在浮点百分比（[物理交战规则（英文源）](../../../contracts/battle/combat-resolution.md#confirmed-resolution-pipeline)）：

| 步骤 | 整数公式与规则（向零取整） | 边界与注意事项 |
| --- | --- | --- |
| 1. 闪避 | **先于伤害**判定；闪避成功直接跳过全部伤害步骤（地面非弓箭手打浮空目标已确认 `RNG(8) == 0` 闪避）。 | 其余武器/移动类型的完整闪避条件仍为**未知**（[闪避步骤（英文源）](../../../contracts/battle/combat-resolution.md#1-decide-dodge-before-calculating-damage)）。 |
| 2. 基础攻防差 | `damage = max(当前 ATT - 当前 DEF, 1)` | 直接使用已刷新好的当前 ATT 与 DEF（[基础伤害（英文源）](../../../contracts/battle/combat-resolution.md#2-calculate-base-physical-damage-with-integer-arithmetic)）。 |
| 3. 防御方地形效果 | 档位 `0`（面板显示 `0%`）：`floor(damage × 256 / 256)`<br>档位 `1`（面板显示 `15%`）：`floor(damage × 230 / 256)`<br>档位 `2` / 其余（面板显示 `30%`）：`floor(damage × 205 / 256)` | **不能把面板 `15%` / `30%` 直接当减伤乘数**：实际因子是 `230/256 ≈ 89.8%`（约减伤 10.2%）与 `205/256 ≈ 80.1%`（约减伤 19.9%），且取**防御方所站地格**。 |
| 4. 对空加成 | 飞行或浮空目标面对 Brass Gunner / Archer / Centaur Archer / Stealth Archer：`damage += floor(damage / 4)` | 在地形减伤**之后**计算（实测例：`79 → floor(79×205/256) = 63 → 63 + floor(63/4) = 78`）。 |
| 5. 会心一击（暴击） | 已确认特殊攻击定义 0 的首次攻击：`RNG(32) == 0` 时 `damage += floor(damage / 2)`（`78 → 117`）。 | 其余暴击定义以及二连击/反击能否暴击仍为**未知**（[暴击步骤（英文源）](../../../contracts/battle/combat-resolution.md#3-apply-the-confirmed-critical-modifier)）。 |
| 6. 反击减半 | 反击（attack type 2）在随机浮动**之前**减半：`damage = floor(damage / 2)`（`10 → 5`）。 | 仅反击生效（[反击减半（英文源）](../../../contracts/battle/combat-resolution.md#4-apply-counter-half-damage-before-spread)）。 |
| 7. 两次向下浮动 | `range = floor(damage / 8) + 1`；连续减两次 `RNG(range)`（两次共用同一个 `range`）。 | **低伤害无浮动**：只要浮动前 `damage < 8`，就有 `range = 1`，两次 `RNG(1)` 必为 `0`，伤害不会再往下掉（[浮动步骤（英文源）](../../../contracts/battle/combat-resolution.md#5-apply-the-two-roll-downward-spread)）。 |
| 8. HP 截断与回放 | 临时 HP 扣减并截断到 `0`；演出回放前先恢复快照再写入持久伤害（不是治疗）。 | 更多随机种子与全输入区间的下限边界仍为**未知**。 |

**单次攻击 vs 二连击 vs 反击**：物理交战按固定顺序`首次攻击 → 二连击 → 反击`依次构建（[追击构建（英文源）](../../../contracts/battle/combat-resolution.md#7-construct-follow-up-attacks-in-original-order)）。每次出手各自独立判闪避和算伤害，二连击读取目标已扣减后的临时 HP。**一击致命会直接取消二连击与反击**；二连击还会被混乱行动者或同阵营目标拦截；反击则会因超出射程、催眠（SLEEP）、眩晕（STUN）、同阵营或特殊敌人规则被拒绝（Burst Rock、Kraken Head、Prism Flower、Zeon Guard 不能反击；塔罗斯 Taros 不可被反击）。

### 手算示例 1：Battle02 站平原还是站道路？（条件推演，**推断**）

**声明条件**（`private-battle02-controlled-party`，普通难度 0、首次全新入场；属性**已含已装备武器**，不要再加短斧 ATT；详见 [Battle02 首次接敌](../analyses/battle02-first-contact.md) 与[英文操作数（英文源）](../../../synthesis/analyses/battle02-first-contact.md)）：

- **贾哈（Jaha）**：`9 HP`，`ATT 14`，`DEF 7`，已装备短斧（Short Axe，射程 1–1），`MOV 5`（REGULAR）。
- **巨鼠（Huge Rat，槽位 131）**位于平原 `(10,13)`：`10 HP`，`ATT 12`，`DEF 8`，射程 1–1。
- **莎拉（Sarah）**支援：`11 HP`，`10 MP`，治疗（HEAL）1（`3 MP`，射程 0–1，威力 `15`）。

假设均为普通命中单次攻击（无闪避、暴击、二连、反击或第三方伤害）：

| 交手方向与防御地格 | 整数演算步骤 | 单击伤害与生存后果 |
| --- | --- | --- |
| **贾哈 → 巨鼠**（平原，档位 1） | `max(14 - 8, 1) = 6` → `floor(6 × 230 / 256) = 5` → `range = floor(5/8)+1 = 1` → `5 - 0 - 0 = 5` | **5 点伤害**（巨鼠 `10 → 5 → 0`，两击倒下；两处站位输出相同） |
| **巨鼠 → 贾哈**站平原 `(11,13)`（档位 1，面板 15%） | `max(12 - 7, 1) = 5` → `floor(5 × 230 / 256) = 4` → `range = 1` → `4 - 0 - 0 = 4` | **4 点伤害**（挨两下：`9 → 5 → 1 HP`，**存活**并能吃到莎拉 3 MP 的治疗 1） |
| **巨鼠 → 贾哈**站道路 `(10,12)`（档位 0，面板 0%） | `max(12 - 7, 1) = 5` → `floor(5 × 256 / 256) = 5` → `range = 1` → `5 - 0 - 0 = 5` | **5 点伤害**（挨两下：`9 → 4 → 0 HP`，若治疗晚到则**直接倒下**） |

*为什么不能直接用面板百分比心算：*基础差为 `25` 时，平原真实算式 `floor(25 × 230 / 256) = 22`，按 `× 0.85` 误算会得到 `21`；基础差为 `20` 时，草地（面板 30%，真实因子 `205/256`）得出 `16`，而不是按 `× 0.70` 误算的 `14`。**未知：**自然通过 Battle01 后的实际队伍等级与装备、真实随机种子，以及巨鼠 131 是否自然走到 `(10,13)` 并选择打贾哈而非莎拉均仍为**未知**——这是有条件对比，不是必然发生的开局剧本。

## 3. 法术伤害、属性抗性与治疗上限

伤害法术与普通治疗共用转职后 `5/4` 威力缩放，但**抗性档位与上限规则和物理攻击完全不同**（[法术结算规则（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-damage-pipeline)）：

| 步骤 | 伤害法术整数规则 | 关键注意与规则来源 |
| --- | --- | --- |
| 1. 转职威力加成 | 施法者职业 ID `>= 12` 时：`adjusted = floor(基础威力 × 5 / 4)`（如 `10 → 12`、`18 → 22`）。 | 未转职保持原威力（[威力调整（英文源）](../../../contracts/battle/spell-resolution.md#1-resolve-base-power-and-caster-adjustment)）。 |
| 2. 召唤术人数分摊 | `DAO`、`APOLLO`、`NEPTUN`、`ATLAS` 对每个目标按目标总数整数均分：`floor(adjusted / 目标数)`（如 `22 / 4 → 5`）。 | 圈住的目标越多、单体威力越低；APOLLO/NEPTUN/ATLAS 完整自然施放路径仍为**未知**。 |
| 3. 属性抗性 | 令 `quarter = floor(adjusted / 4)`，按对应元素的 2 位抗性档位分支：<br>• `0`（中性）：`adjusted`<br>• `1`（轻度抗性）：`adjusted - quarter`<br>• `2`（重度抗性）：`floor(adjusted / 2)`<br>• `3`（**弱点**）：`adjusted + quarter` | 用的是先取整后的 `quarter`，**不是**浮点 `75%` / `125%`。**务必区分使用方**：档位 `3` 对伤害法术是**弱点**，但对催眠、混乱 2、迟缓、封魔、摄魂等状态法术却是**免疫**（`RNG(8)` 门槛 `8` 不可达）（[抗性结算（英文源）](../../../contracts/battle/spell-resolution.md#2-apply-the-element-resistance-with-integer-truncation)）。 |
| 4. 法术暴击 | 烈焰（BLAZE）已确认：`RNG(32) == 0` 时再加一次 `quarter`（`15 → 18`）。 | 其余伤害法术暴击分支仍为**未知**（[法术暴击（英文源）](../../../contracts/battle/spell-resolution.md#3-roll-spell-critical-before-shared-variance)）。 |
| 5. 共用向下浮动 | 复用 `InflictDamage`：`range = floor(damage / 8) + 1`，`max(damage - RNG(range) - RNG(range), 1)`。 | 浮动后最低保留 `1` 点伤害（[共用浮动（英文源）](../../../contracts/battle/spell-resolution.md#4-reuse-the-common-downward-damage-spread)）。 |

**治疗规则**（[治疗 1 与边界（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-heal-1-subset)）：普通治疗（HEAL 1–3）与群体治疗（AURA 1–3）先算转职 `5/4`（例如 `VICR`/`MMNK` 施放治疗 1 为 `15 → 18`，治疗 3 为 `30 → 37`），再以 `缺失 HP = 最大 HP - 当前 HP` 封顶——不会溢出加血，也不能复活 `0 HP` 倒下队友。治疗 4 与群体治疗 4 的威力 `255` 是**满血哨兵值**，跳过转职缩放，直接恢复每个存活目标的全部缺失 HP。各类状态法术的独立门槛、重施机制与回合后倒计时详见[法术结算规则](../../contracts/battle/spell-resolution.md)，不在此重复建表。

### 手算示例 2：烈焰（BLAZE）2 抗性档位与治疗（HEAL）1 缺失 HP 封顶（原版受控实测，**已确认**）

**2A. 烈焰（BLAZE）2 四档火属性抗性**（`sf2-spell-damage-resistance-v1`，基础威力 `10`，耗 `6 MP`，四个 `100 HP` 目标，受控浮动抽得 `[0, 0]`）：

| 目标火抗档位 | 施法者职业 | `adjusted` → `quarter` | 抗性后威力 | 暴击（`RNG(32)`） | 浮动 `range` | 最终伤害 |
| --- | --- | --- | --- | --- | --- | --- |
| `0`（中性） | 未转职（`0`） | `10` → `2` | `10` | 抽得 `29`（否） | `2`（`0, 0`） | **10** |
| `1`（轻抗） | 未转职（`0`） | `10` → `2` | `10 - 2 = 8` | 抽得 `29`（否） | `2`（`0, 0`） | **8** |
| `2`（重抗） | 未转职（`0`） | `10` → `2` | `floor(10/2) = 5` | 抽得 `29`（否） | `1`（`0, 0`） | **5** |
| `3`（弱点） | 转职（`12`） | `floor(10×5/4) = 12` → `3` | `12 + 3 = 15` | 抽得 `0`（`+3`） | `3`（`0, 0`） | **18** |

*注意整数取整结果：*未转职轻抗（`10 - floor(10/4)`）等于 **8**，而不是 `7.5`。**边界与未知：**本实测在初始化前注入四目标列表并在第四次调用切至职业 12；自然流程中的转职烈焰全过程，以及多目标攻击法术的自然地图选序仍为**未知**。

**2B. 治疗（HEAL）1 缺失 HP 封顶与奶妈保底经验**（`sf2-heal1-self-recovery-v1`，Battle 01 受控自施，未转职祭司 `PRST`，治疗 1 威力 `15`、耗 `3 MP`，目标 `95 / 100 HP`）：

1. 缺失 HP 只有 `100 - 95 = 5`，因此实际回复量从 `15` 被截断为 **`5 HP`**（回放后 HP `95 → 100`，MP `20 → 17`）。
2. 施法者是己方 `PRST`（仅 `PRST`/`VICR`/`MMNK` 拿治疗经验），原始比例 `floor(25 × 5 / 100) = 1`，经保底门槛抬升至 **`10`**（单次行动累计上限 `25`）。
3. 同阵营施法**跳过** Battle 01 的经验减半；结算时两次 `RNG(16)` 抽得 `14` 和 `0`，触发 `-1`，最终写入 **`9 EXP`**。

## 4. 敏捷与出手顺序：为什么高 AGI 也不保证先动

每一整轮回合开始时，只有已出场且存活的单位进入出手队列（[出手顺序规则（英文源）](../../../contracts/battle/battle-control-lifecycle.md#turn-order-construction)）：

1. 取当前敏捷低 7 位：`base = 当前 AGI & 0x7F`，并算扰动幅度 `spread = base >> 3`（即 `floor(base / 8)`）。
2. 计算修正得分：`score = (base + RNG(spread) - RNG(spread) + RNG(3) - 1)`，结果按 **8 位有符号字节（`-128..127`）**回绕。
3. **同轮第二次出手**：若原始 `当前 AGI >= 128`（第 7 位置 1），额外加入第二条队列项，基数 `base2 = floor(base × 5 / 6)`，做同样幅度的加减扰动，但**不加** `RNG(3) - 1`。
4. 全部条目按有符号字节**降序稳定冒泡排序**——同分保持入队顺序（己方槽位在敌方之前）。

因为 `RNG(spread)` 取值 `0 .. spread - 1`（`spread == 0` 时恒为 `0`），正常未回绕单位的得分会落在 `[base - spread, base + spread]`（`base < 8` 时为 `[base - 1, base + 1]`）。

### 手算示例 3：敏捷区间重叠、同分先后与有符号字节回绕

1. **低敏捷重叠与同分先手**（Battle02 数值——规则**已确认**，区间**推断**；[治疗窗口（英文源）](../../../synthesis/analyses/battle02-first-contact.md#when-does-sarah-actually-get-a-window)）：
   巨鼠（`AGI 7`）、莎拉（`AGI 5`）、贾哈（`AGI 4`）的 `base >> 3` 都是 `0`，得分就是 `AGI + RNG(3) - 1`：巨鼠 `6..8`、莎拉 `4..6`、贾哈 `3..5`。巨鼠一定早于贾哈，但当**莎拉 `6` 与巨鼠 `6` 同分**时，莎拉因己方入队在先而抢先出手；而贾哈抽到 `5` 时也能压过抽到 `4` 的莎拉。
2. **中段敏捷反超**（由已确认公式**推断**）：一旦 `AGI >= 8`，`spread = floor(AGI / 8)` 扩大波动带。`AGI 16`（`spread = 2`，每次抽 `{0, 1}`）得分跨 **`14..18`**，而 `AGI 14`（`spread = 1`，抽 `{0}`）得分跨 **`13..15`**——因此 `AGI 14` 抽到 `15` 时，会比抽到 `14` 的 `AGI 16` 更先行动。
3. **极端回绕与二次出手**（`sf2-turn-order-boundaries-v1`，Battle 01 种子 `0x0000` 受控实测，**已确认**）：
   - 己方 1 号设为 `AGI 127` 时，原始字节算得 **`135`**，按有符号字节读成 **`-121`**，反而排在得分 `6、5、4` 的低敏敌人之后！
   - 己方 0 号设为 `AGI 128`（第 7 位置 1、`low = 0`）排入**两次出手**，得分 `0` 与 `255`（有符号 `-1`）；同时倒下的己方 2 号与未上场的敌人 128 被排除。

**未知：**带状态修正的敏捷、多名 `AGI >= 128` 单位同场交互，以及 Battle 01 之外的自然战场回绕情况仍为**未知**。

## 5. 行动经验、等级差档位、阈值与升级刷新

战斗奖励分轨结算，不要把伤害/击杀经验、治疗经验、状态经验与金币混成一条规则（[物理经验（英文源）](../../../contracts/battle/combat-resolution.md#9-award-confirmed-battle-01-exp)、[法术经验（英文源）](../../../contracts/battle/spell-resolution.md#5-accumulate-and-award-attack-spell-exp)、[战中升级（英文源）](../../../contracts/party/level-up.md#confirmed-battle-award-entry)）：

| 奖励轨 | 累计公式（整数向下取整） | 单次行动上限与修正 |
| --- | --- | --- |
| **伤害与击杀经验** | 有效等级差 `diff = 行动者等级 + (转职 ? 20 : 0) - 目标等级`（职业 ID `>= 12` 为转职）。<br>击杀基准：`diff < 3 → 50`，`3 → 40`，`4 → 30`，`5 → 20`，`6 → 10`，`>= 7 → 0`。<br>伤害经验：`floor(击杀基准 × 最终伤害 / 目标最大 HP)`。致命一击先加伤害经验、再加完整击杀基准。 | 每次累加后均截断到 **`49`** 上限。 |
| **治疗经验** | 仅己方 `PRST`、`VICR`、`MMNK` 且 `目标最大 HP > 0`：每名目标 `max(floor(25 × 回复量 / 目标最大 HP), 10)`。 | 整次行动共用 **`25`** 上限（如群体治疗 2 治疗 3 名伤员时累计 `0 → 10 → 20 → 25`）。 |
| **状态 / 吸魔经验** | 每名生效目标 `+5`（含对 `0 MP` 目标放吸魔 SPOIT；无效目标 `+0`）。 | 计入同一次行动累计器。 |
| **最终 EXP 指令** | Battle 01 对敌行动先整除减半 `floor(acc / 2)`（同阵营行动不减半）；随后两次有序 `RNG(16)`：首抽 `0` 加 `1`，次抽 `0` 减 `1`（双 `0` 抵消）。 | 最终指令保底 **`1 EXP`**（哪怕累计为 `0`，如转职达奥打 1 级敌人）。 |
| **金币** | 击杀按 103 条敌人金币表入账，全队余额上限 **`9,999,999`**。 | **不被** Battle 01 减半，也**不参与**随机波动（[金币上限（英文源）](../../../../research/battle/runtime-rng-and-battle-math.md#confirmed-gold-addition-and-cap-boundaries)）。 |

**存储经验与单次阈值升级：**回放 EXP 指令时，角色存储经验先截断到 **`200`**，再**最多检查并扣除一次 `100` 点阈值**——单条指令不会循环连升两级。未转职职业（ID `< 12`）上限 **40 级**，转职职业（ID `>= 12`）上限 **99 级**；满级后凑满 100 EXP 仍会扣掉 100 点但不升级（返回 `255`）。每条非空成长曲线消耗两次 `RNG(128)` 并带最低成长保底 `+1`（30 级起改为固定 1.5 点基数 `384/256` 对照投影值，见[升级序列（英文源）](../../../contracts/party/level-up.md#confirmed-original-sequence)）。**升级绝不会顺带补满当前 HP 或 MP。**

### 手算示例 4：跨 100 阈值、200 封顶与升级不回血（原版受控实测，**已确认**）

四组 `24 EXP` 指令边界（`sf2-exp-command-boundaries-v1` 与 `sf2-battle-exp-level-up-v1`）：

| 初始状态 | `+24 EXP` 指令 | 饱和后总值 | 结算结果 |
| --- | --- | --- | --- |
| 1 级，`75 EXP` | `75 + 24` | `99`（`< 100`） | 保持 1 级，**`99 EXP`**（不升级） |
| 1 级，`76 EXP` | `76 + 24` | `100`（`>= 100`） | 扣 100 → 升至 2 级，**`0 EXP`**（升 1 级） |
| 鲍伊 1 级，`99 EXP`，HP `12/12`、MP `8/8` | `99 + 24` | `123`（`>= 100`） | 扣 100 → 升至 2 级，**`23 EXP`**，最大 HP `12 → 14`，**当前 HP/MP 仍为 `12/8`** |
| 1 级，`199 EXP` | `199 + 24` | 截断于 **`200`** | 仅扣**一次** 100 → 升至 2 级，余 **`100 EXP`**（仍只升 1 级） |

**带装备刷新与触顶对照**（`sf2-level-up-refresh-v1` / `sf2-level-up-boundaries-v1`）：斯莱德（`THIF`，`39 → 40` 级）初始基础 `ATT 45`、最大 HP `42`、**当前 HP `7`**，已装备短刀（Short Knife，`+5 ATT`）。升到 40 级后基础 ATT 涨到 `47`、最大 HP 涨到 `44`，`UpdateCombatantStats` 从新基础重算出当前 ATT = **`47 + 5 = 52`**（不会把 `+5` 加两次），但**当前 HP 仍停在 `7`**——他依然重伤，必须另行治疗。到达未转职 40 级上限后，再吃一次 `76 + 24` 仍会扣掉 100 EXP（归 `0`）而不再升级。**未知：**除 Battle 01 之外，其余战场的自然经验修正表在自然游玩中的完整表现仍为**未知**。

## 6. 原版事实与已接受的重制边界

- **不把原版缺陷或重制改动混为一谈**：上述公式与边界基于锁定美版 ROM 与 `SF2DISASM` 源码基线。陆龟（TORT，职业 11）法术学习误判、Ninja Katana `INCREASE_DOUBLE` 清除反击位、`ATTACK`/`BOOST`/`SLOW` 重施失败后计数与属性暂不同步，以及 `65535 + 2 → 1` 的字长回绕均属**原版保真事实**；重制版若要刻意修正，必须单独记录决策与预期偏离，不能悄悄改写原版结论。
- **已接受的重制偏离**：
  - **有限音乐时钟**（[音乐等待策略（英文源）](../../../contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy)）：替代原版硬件等待时序，可能使整局随机序列与原版分岔，但不改变单次交战或升级公式。
  - **战斗 Continue 保留未刷新重施状态**：重制版在战斗读档继续中保留 `BOOST`/`SLOW` 计数与旧有效属性的暂时差异，直到下次刷新（[设计地图](../../design-map.md#状态法术)）。
- **当前可玩范围与 Wiki 说明**：重制引擎当前实际可玩范围以[重制能力状态（英文源）](../../../../../remake/docs/capability-status.md#campaign-feature-trace)为准。本页先在主仓完成规范化与验收，线上 Wiki 同步属于合并后的独立发布步骤，**当前尚未发布到线上 Wiki**。

## 依据与边界

- **英文规范源**：[Core Battle Numbers and Growth Quick Reference（英文规范源）](../../../synthesis/themes/battle-numbers.md)。
- **核心 L3 规则与研究来源**：[物理交战](../../contracts/battle/combat-resolution.md) · [法术结算](../../contracts/battle/spell-resolution.md) · [战场寻路](../../contracts/battle/battlefield-navigation.md) · [战斗生命周期](../../contracts/battle/battle-control-lifecycle.md) · [升级与属性刷新](../../contracts/party/level-up.md) · [运行时 RNG 与战斗数学（英文源）](../../../../research/battle/runtime-rng-and-battle-math.md)。
- **关联 L2 专题与战例**：[战术战斗循环](tactical-battle-loop.md) · [成长与经济](progression-and-economy.md) · [Battle02 首次接敌](../analyses/battle02-first-contact.md) · [渡海前七战数值覆盖（英文源）](../../../synthesis/analyses/pre-voyage-battle-model-coverage.md) · [重制设计地图](../../design-map.md)。
