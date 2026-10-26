# Battle26 初始编队与中立 Higins

> 本文是[规范英文原文：Battle26 Initial Formation and Neutral Higins](../../synthesis/battle26-initialization-and-roster.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

**已确认：** Geshp 对峙的 before 包装器返回后，新 Battle26 入场治疗符合资格的伙伴、布置实际出战军团与十五名敌人、替换场景人口并加载战场。完成未设的加载添加独立中立 Higins 行动者。他出现不招募 ally19。**推断：** 此编队布置 Ketto 外遭遇。**未知：** 自然抵达、传入状态、已完成服务、可见/可听表现、可用控制与持久性。

此英文指南接续 [Battle26 准入与 Geshp 对峙](battle26-admission-and-geshp-confrontation.md)，使用 [#1010](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1010) 和 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)下完整已接受[初始化研究](../../../research/battle26-initialization-and-roster.md)。止于 `BattleLoop:@Start`，紧邻**首次 ActivateEnemies 之前**。不建立已激活演员表、首个行动者、移动、伤害、掉落、结果或 Higins 招募。F88 传入继续是独立对照，止于读取或执行其单次行动循环之前。

Fnnn 表示标志 nnn；以下 ID 与坐标是源码操作数或条件初始化结果，不是运行时观察。源码身份与路径经研究所有者[协议与溯源](../../../research/battle26-initialization-and-roster.md#agreement-and-provenance)解析：USA retail 基线与 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`。此综合消费已接受 Layer A，不增加原版证据或重制规则。

## 从对峙到新初始化

**已确认：** 前序止于 before 包装器恢复 d1 并返回。尚未治疗队伍、构建战斗员、设 intro476 或加入 Higins。调用者已清 PLAYER_TYPE、设 F399 并选择 F88 未设分支。before 返回后，有序后缀为：

1. 恢复保存的传入地图/战斗字，将其低字节重新赋值到当前 map72/battle26。不恢复或重算战斗裁切。
2. 清区域标志90–105，包括三个配置区域的 F90–92。
3. 按序运行 HealLivingAndImmortalAllies、InitializeAllAlliesBattlePositions、InitializeAllEnemiesBattlePositions 与 ClearAiMemory。
4. 清 d0.w 为零，为 combatant0 调用 LoadBattle。
5. ExecuteBattleStartCutscene，再到 ActivateEnemies 前的 `@Start`。

[有序后缀所有者](../../../research/battle26-initialization-and-roster.md#return-from-before-and-ordered-fresh-initialization)保留选定 BattleLoop 源码与地址。Unlock426、intro476、completion526 与 continuation88 各有角色。仅当更早 Map72 候选408/424 失败，准入才能选26；持有完成的清理会清426，但仍准入该次选定调用。初始化不重复选择。持有476 跳过 before/start 分发，但仍运行新重置、治疗、名册和加载。每步要求先前服务返回；中断保留已完成前缀写入，不回滚。

## 谁获得治疗与初始位置

**已确认：** 治疗遍历全部三十己方槽，不论军团成员资格。存活伙伴及始终符合的 Peter7/Lemon28，以最大 HP/MP 作为当前值。其他死亡伙伴跳过。服务将状态掩码为 STUN、POISON、CURSE（`$0007`），然后 UpdateCombatantStats 结合状态/装备效果重建当前属性并重算诅咒。中间掩码不保证最终保留诅咒。不替换物品栏、职业、成员资格或转职。HP 为字；MP 与位置为字节，即便调用传字寄存器。

先将全部三十己方 X 设为 -1。UpdateForce 按实际标志、己方 ID 升序重建 joined、active 和 reserve 列表。因此布置表提供**重建出战列表中的序数**，不是十二名指定角色。只有处理到的存活成员获得下一 X/Y 行。布置行 AI/物品字段不覆盖己方状态。

十二个初始位置均为战斗局部瓦片：

| 序数 | X/Y | 序数 | X/Y |
| --- | --- | --- | --- |
| 0 | `(7,25)` | 6 | `(4,26)` |
| 1 | `(6,25)` | 7 | `(6,27)` |
| 2 | `(5,25)` | 8 | `(5,27)` |
| 3 | `(4,25)` | 9 | `(4,27)` |
| 4 | `(6,26)` | 10 | `(7,26)` |
| 5 | `(5,26)` | 11 | `(7,27)` |

列表中死亡成员消费十二个处理成员槽之一，但不推进布置指针。之后存活成员用下个未消费位置。符合成员更少则行动者更少。多余出战成员经 LeaveBattleParty 清 active 标志与 X；该辅助不立即重建列表/计数。不一致的零成员数在首次读取前递减，无空列表守卫，因此不建立任意空状态安全。

变化职业、HP、装备与状态影响资格、属性和精灵。陈旧传入出战列表不是权威，十二行不暗示十二名存活行动者。若传入 joined/active 标志已包括 Higins19，则独立按己方处理；之后中立行动者不加入他。[实际己方消费者审查](../../../research/battle26-initialization-and-roster.md#actual-allies-healing-and-formation)与[出战队伍/名册合同](../contracts/party-roster-state.md)保留成员资格、处理器和持久性边界。

## 十五名配置敌人

**已确认：** BattleSpriteset26 声明 **12 个己方布置、15 名敌人、3 个区域和 1 个点**。初始未升级构成为一 Lizardman、一 Wizard、两 Dark Bishop、四 Lesser Demon、四 Dark Soldier、三 Dark Sniper。完整[行所有者](../../../research/battle26-initialization-and-roster.md#complete-spriteset-and-fifteen-enemy-rows)解析槽26、366 字节范围、全部子节偏移和宏宽度。本指南保留有意义行，不复制编码布局。

敌人行 r 初始化 combatant128+r。每行用 STARTING=0 和 filler0；除 row2 编码 Black Ring 外，全部行物品为 NOTHING。Region15 表示无触发，不是第十五多边形。NONE 与 FOLLOW_ENEMY 是配置命令，不证明等待、追逐或已执行战术。

| 行 / 战斗员 | 类型 / 定义 ID | X/Y | 指令集 | 主要命令 / 区域 | 次要命令 / 区域 |
| --- | --- | --- | --- | --- | --- |
| 0 / 128 | Lizardman / 19 | `(22,7)` | LEADER | MOVE_TO\|0 / 15 | NONE / 15 |
| 1 / 129 | Wizard / 72 | `(21,8)` | ATTACKER3 | FOLLOW_ENEMY\|0 / 15 | NONE / 15 |
| 2 / 130 | Dark Bishop / 81 | `(24,7)` | HEALER3 | FOLLOW_ENEMY\|0 / 15 | NONE / 15 |
| 3 / 131 | Lesser Demon / 45 | `(17,3)` | HEALER3 | FOLLOW_ENEMY\|0 / 1 | NONE / 1 |
| 4 / 132 | Dark Soldier / 18 | `(22,9)` | ATTACKER1 | NONE / 1 | NONE / 15 |
| 5 / 133 | Dark Sniper / 62 | `(23,11)` | ATTACKER1 | NONE / 1 | NONE / 15 |
| 6 / 134 | Dark Sniper / 62 | `(15,12)` | ATTACKER3 | FOLLOW_ENEMY\|7 / 2 | NONE / 2 |
| 7 / 135 | Dark Soldier / 18 | `(17,13)` | ATTACKER1 | NONE / 2 | NONE / 15 |
| 8 / 136 | Lesser Demon / 45 | `(4,6)` | ATTACKER1 | NONE / 2 | NONE / 15 |
| 9 / 137 | Dark Soldier / 18 | `(24,17)` | ATTACKER3 | FOLLOW_ENEMY\|10 / 0 | NONE / 0 |
| 10 / 138 | Dark Soldier / 18 | `(23,18)` | ATTACKER1 | NONE / 0 | NONE / 15 |
| 11 / 139 | Lesser Demon / 45 | `(24,27)` | ATTACKER1 | NONE / 0 | NONE / 15 |
| 12 / 140 | Lesser Demon / 45 | `(9,19)` | ATTACKER1 | NONE / 15 | NONE / 15 |
| 13 / 141 | Dark Bishop / 81 | `(13,20)` | HEALER4 | FOLLOW_ENEMY\|14 / 15 | FOLLOW_ENEMY\|12 / 15 |
| 14 / 142 | Dark Sniper / 62 | `(14,23)` | ATTACKER1 | NONE / 15 | NONE / 15 |

初始化扫描敌人槽128–159。这十五行走 InitializeEnemyStats：解析类型、复制 56 字节定义、从最大值设置当前 HP/MP、写位置、打包 movetype/commandset、存类型/触发/命令并处理行物品。槽143–159 得 activation0、最大/当前 HP0、X=-1。这是缺席槽部分重置，不清全部 Y/MP/物品栏字段。因子节指针未变，触发读取仍用 row0 的 15/15。无额外敌人行。Battle32/Jaro 例外不适用；加载之后将159 改作中立。

激活写入保留定义位 `$F000`，组合旋转并掩码的初始化字。对这些未升级定义与零生成字，Bishop 得 `$3000`，其余行 `$2000`。定义 BIT4/BIT5 被丢弃；primary-active、secondary-active、neutral 与 spawn-mode 位未设。这是激活存储初始化，首次激活例程尚未运行。

### 定义、装备与初始当前属性

**已确认：** completion526 未设时，选定定义如下。HP/MP 最大值也成为新当前 HP/MP。每定义有四物品槽和四法术槽：下面仅首物品已填；未列物品/法术槽为 NOTHING。此处 ATT/DEF/AGI/MOV 是难度/装备处理前复制的**基础**值。

| 类型 | 等级 | HP / MP | 基础 ATT / DEF / AGI / MOV | Movetype | 复制的首物品 | 法术 |
| --- | --- | --- | --- | --- | --- | --- |
| Dark Soldier18 | 23 | 35 / 17 | 31 / 46 / 34 / 4 | REGULAR1 | 已装备 Large Axe | ATTACK |
| Lizardman19 | 24 | 40 / 0 | 34 / 45 / 38 / 6 | REGULAR1 | 已装备 Large Axe | 无 |
| Lesser Demon45 | 22 | 40 / 21 | 49 / 37 / 32 / 6 | FLYING5 | NOTHING | BLAZE3, BOOST2 |
| Dark Sniper62 | 24 | 38 / 0 | 11 / 37 / 35 / 5 | STEALTH_ARCHER10 | 已装备 Hyper Cannon | 无 |
| Wizard72 | 26 | 37 / 37 | 32 / 40 / 44 / 5 | MAGE11 | 已装备 Flail | BLAZE3 |
| Dark Bishop81 | 23 | 39 / 38 | 28 / 38 / 27 / 5 | HEALER12 | 已装备 Flail | AURA, BLAST3 |

全部六种初始状态为 NONE，法术威力 ENHANCED。Lizardman 抗性字段为 ICE_WEAKNESS，Demon 为 STATUS_MINOR，Wizard 为 FIRE_MINOR；其余为零。Soldier/Demon 特殊攻击概率字段保留 critical150 chance1in32、double1in32、counter1in16；Lizardman 将会心改为1in8，Sniper 二连击改为1in16，Wizard/Bishop 反击改为1in32。优先字段除 Bishop 为 PRIORITYMOD_3 外均为 PRIORITYMOD_2。这些仍是复制字段，不给出伤害、法术、先手或平衡结论。

实际难度例程在首次 SUPER 比较后有连续 `beq` 分支。SUPER 跳 Continue；所有其他值也经第二个未跳转分支顺落 Continue。因此 NORMAL0、HARD1、OUCH3 均应用一次 floor(baseATT*5/4)；SUPER2 应用两次。不能仅凭标签声称只有 SUPER 增加。UpdateCombatantStats 再将基础复制为当前属性并应用已装备物品。Large Axe 加 25 ATT，Hyper Cannon 加 40，Flail 加 19。当前 DEF/AGI/MOV 等于表值，因为这些初始状态/装备定义无相关效果。条件 ATT 结果为：

| 类型 | NORMAL / HARD / OUCH 调整基础 ATT | 当前 ATT | SUPER 调整基础 / 当前 ATT |
| --- | --- | --- | --- |
| Dark Soldier | 38 | 63 | 47 / 72 |
| Lizardman | 42 | 67 | 52 / 77 |
| Lesser Demon | 61 | 61 | 76 / 76 |
| Dark Sniper | 13 | 53 | 16 / 56 |
| Wizard | 40 | 59 | 50 / 69 |
| Dark Bishop，两行 | 35 | 54 | 43 / 62 |

这些是未升级前提下的初始化结果，不是之后单次行动中不可变属性。[定义/属性所有者](../../../research/battle26-initialization-and-roster.md#selected-definitions-equipment-and-initialized-stats)点名精确敌人/物品定义、属性设置器和难度消费者。

### Black Ring 为什么不增加攻击

**已确认：** row2 编码 BLACK_RING，带 EQUIPPED、USABLE_BY_AI、UNUSED_ITEM_DROP，得到 `$6098`。复制 Bishop 定义已有槽0 已装备 Flail 与空槽1。AddItem 成功时，消费者用 `$807F` 掩码**存储值与 d1 两者**，在槽1 插 `$0018`。调用者再测试返回 d1 的 equipped 位；已清，因此不调 EquipItemBySlot。

结果四物品为 `$00DA,$0018,$007F,$007F`：已装备 Flail、未装备 Black Ring、NOTHING、NOTHING。编码的 AI 使用/掉落位也被剥离。Ring 定义 +10 ATT 不贡献上述初始化 ATT。BLAZE2 仍是定义 use-spell 字段；不建立之后使用/掉落/奖励。其余行精确 NOTHING 操作数在 AddItem 前返回，保留复制装备。修改后满定义物品栏走 AddItem 失败路径，调用者不检查。成功插入不保证另一分支也被规范化。

### 完成可改变返回定义

**已确认：** OUTSIDE_KETTO26 属于十一项随机战斗列表。DetermineBattleUpgrade 测 completion526：未设保留声明类型；持有允许在定义复制前运行 UpgradeRandomBattleEnemies。独立于 intro476，也独立于526 另行抑制中立人口。重复介绍不是敌人升级条件。

辅助对 Soldier/Lizardman 选 melee，Demon 选 airborne，Sniper 选 ranged，Wizard 选 mage，Bishop 选 healer。使用 Bowie0 等级，class>11 时加20，减 battle26，并要求至少10 的正差。较小差保留原类型。类别范围参数5/2/2/1/2、RNG、边界、排除与重试/后备分支支配返回定义。其属性、物品栏、movetype、精灵和优先级可改变；Battle26 行坐标、指令、区域、命令及物品操作数仍为所选值，定义复制后仍处理难度。

Airborne 排除列表为空，但消费者递减零计数并无零长度守卫地开始扫描。高等级或类别标签不保证安全升级 Demon。保留不确定性属于[全局数据升级所有者](../../../research/battle-global-data.md#semantics-and-runtime-queue)及[选定 Battle26 升级审查](../../../research/battle26-initialization-and-roster.md#completion-held-upgrade-alternative)。不据此扩展升级/交战审计，也不保证确定重复名册。

## 区域与目标点配置后续工作

**已确认：** 三个战斗局部区域为：

| 区域 / 标志 | 类型 | 顶点 1、2、3、4 |
| --- | --- | --- |
| 0 / F90 | 4 | `(21,0)`, `(12,31)`, `(35,31)`, `(35,0)` |
| 1 / F91 | 3 | `(4,0)`, `(34,31)`, `(34,0)`, `(0,0)` |
| 2 / F92 | 4 | `(0,0)`, `(0,21)`, `(35,21)`, `(35,0)` |

Type3 用顶点1/2/3；另一分支用三角形1/2/4 和3/2/4。区域1 第四坐标对不是该处几何操作数。每记录第二字节及尾字节为零，在选定路径不作几何操作数。新重置清相关标志，不表示事件已触发。之后区域/激活工作在终点外。

Point0 为 `(23,7)`。Lizardman 的 MOVE_TO\|0 通过命令低半字节引用它。FOLLOW_ENEMY 操作数0/7/10/14/12 分别指战斗员128/135/138/142/140。例如 row13 引 rows14 和12，不是 ally14 或 ally12。这些是配置的点/战斗员引用，不建立所选战术目标或已完成移动。[区域/点消费者所有者](../../../research/battle26-initialization-and-roster.md#three-regions-and-one-point)保留记录宽度、单位与选定读取路径。

## 用物理战斗行动者替换场景

**已确认：** LoadBattle 保留战斗员参数、选当前 map72、请求淡出、禁用视野跟踪、加载瓦片集、等淡出完成、清 VInt、等一个 VInt，再调 PositionBattleEntities。ClearEntities 重置 49 条物理记录和 64 查找字节，再重置步行分配状态。替换对峙行动者、行动与随从别名。场景 NPC128 是 Geshp；新 combatant128 是 Lizardman。复用数字不证明变身或败北。

普通分配扫描全部 30 己方和 32 敌方槽。有符号 X/Y 非负允许声明；neutral 位将敌人排除在普通遍历外。**无额外 HP 测试**。新初始化建立通常 HP/位置关系，传入继续则未必。位置字节变为 `(X & $3F)*384,(Y & $3F)*384`；行动者初始 Down、eas_Standing。飞行/悬浮 movetype 选 layer1，其余 layer0。声明初始化位置/目标、精灵、行动、速度/行程/加速度、标志与计数器；安装行动不证明表现完成。

实际职业支配己方精灵，包括 Rohde 例外；战斗模式不用探索死亡己方 BLUE_FLAME 分支。存储敌人类型选精灵。未升级 Soldier82、Lizardman83、Demon109、Sniper126、Wizard136、Bishop145 低于特殊精灵阈值240，顺序分配。若声明 N 名己方，row r 用物理 N+r：十二己方时敌人占物理12–26。缺失/特殊/变化槽必须走实际分支，不套用条件映射。

敌人 ID128–159 解析到查找偏移32–63；脚本别名 A30/B31 为独立项。被跳过普通查找保持 -1。49 条物理重置记录和固定扫描不是分配守卫；DeclareNewEntity 无容量检查。普通新入场最多用 12+15+1=28 行动者。不能承诺任意传入位置对扫描全部 62 战斗员槽都安全。[物理实体所有者](../../../research/battle26-initialization-and-roster.md#battle-load-and-replacement-of-physical-entities)保留精确字段、哨兵及精灵/查找消费者。

### 中立 Higins 的三种身份

**已确认：** 普通分配后，completion526 未设准入中立表唯一 OUTSIDE_KETTO 行：`(18,14)`、**Down**、HIGINS_PROMO48、eas_LyingLeft。持有526 跳过中立扫描。新声明不保留场景 actor19。

| 身份 | 新完成未设结果 |
| --- | --- |
| 战斗员 state159 | 最大/当前 HP0、status0、X18/Y14、中立激活 `$0008`；不重写全部其他字段。 |
| 物理行动者 | 下一分配，条件 N+15（十二己方时为27），layer0，带表中精灵/朝向/行动。 |
| 脚本查找 B31 | 接收该物理索引；combatant159 普通查找偏移63 保持 -1。 |

中立写入者将查找指针重置到敌人列表起点，再预递减一次：实际目标为偏移31、B，而非 combatant159 偏移63。因此 GetEntityIndexForCombatant(159) 不能找回中立行动者。A30 保持清零0；通用 C158/D159 敌人别名不变为己方。即便可见角色叫 Higins，State159、物理行动者与脚本 B31 仍须区分。

无 JoinForce、ally19 属性复制、己方治疗或 joined/active 标志写入。HP0 中立声明不证明 Higins 已死、加入或改变阵营。eas_LyingLeft 请求 orientLeft/updateSprite，顺落 motion OFF 并到 eas_Idle。Left orientation 不同于初始 Down facing；行动安装不是观察到的卧姿。LoadBattle 之后重新安装 VInt，行动可在 start 钩子前推进，但精确到达帧/行动为未知。

持有526 时，新159 保持缺席，HP0/X=-1/activation0；B31 保持0，不加中立行动者。传入 F88 时，中立标记159 被普通遍历跳过，之后未设526 重写它。不一致的非中立159 若位置有效，可先分配普通行动者，再被中立遍历改用途，而不移除更早声明。不据此建立通用一一映射或传入状态修复。[中立行与查找证明](../../../research/battle26-initialization-and-roster.md#neutral-higins-slot159-script-b31-physical-n15)拥有表元组与精确写入。

## 战场裁切、视野、地形与音乐

**已确认：** row26 选 Map72，裁切原点 `(28,0)`、大小36x32、通配触发坐标255/255。裁切不同于地图区域描述器及地形。LoadBattle 不重跑 CheckBattle；场景后重新赋值72/26 不能修复变化 BATTLE_AREA 字段。

LoadBattle 恢复 d0=0、解析 combatant0、写 VIEW_TARGET_ENTITY，并将其物理瓦片位置采样到 BATTLE_ENTITY_CHOSEN_X/Y。负查找用物理0 后备读坐标，不创建 Bowie 或修复成员资格。它初始化精灵，以 **d0=-1** 调 LoadMap 表示按目标放置视野，并保留 **d1=72** 表示完整加载。负 d1 才表示独立重载路线；参数目的不同。

Map72 选调色板 0 与瓦片集 106/102/107/108/105。唯一描述器覆盖 `(0,0)` 至 `(63,63)`，前/背景起点 0/0、视差对 256/256、零自动滚动、图层类型 254、MUSIC_MAIN_THEME1。描述器选择将目标内部坐标与乘 384 的边界比较，不加裁切 28/0；后备选首描述器，此处也是唯一描述器。[地图/裁切源码所有者](../../../research/battle26-initialization-and-roster.md#map72-crop-descriptor-terrain-music-and-view)保留完整源码元组、范围和精确加载消费者。

基础图块/布局接收标志复制与已开宝箱覆盖，再将传入战斗矩形复制到工作原点0/0。裁切沿宽 64 的字行复制 1152 字/2304 字节，不建立新 36 宽存储步幅。战斗区域使用 layer1 原点0/0 与终点 `(35*384,31*384)=(13440,11904)`，其余描述器字段仍适用。所有编队、敌人、中立、区域与点坐标已是局部值；初始化不减28。源码布局对应为 `(localX+28,localY)`：placement0→`(35,25)`、Lizardman→`(50,7)`、point0→`(51,7)`、中立 Higins→`(46,14)`。这些关系不建立合法步行路径。

地图加载后，LoadBattle 等一 VInt、加载实体地图精灵、安装基础 VInt、将槽26 的 BattleTerrain26 解压至 BATTLE_TERRAIN_ARRAY，再请求地图音乐与淡入。地形指针选择既有私有载荷；本指南不添加地形语料或移动比较。PlayMapMusic 从描述器 music1、MAIN_THEME 开始，不匹配任何战斗替换分支，因此提交1，不假定替换战斗主题。Battle44 计时器分支不适用。有序加载/淡入淡出/音频请求不建立渲染输出、可听时序、异步服务完成或可用玩家输入。

## 空 start 钩子与传入继续

**已确认：** 48 项 start 表槽26 选 ms_Empty，完整主体为 `$FFFF` 终止符。start 包装器检查476；未设时，**先设476**再保存 d0/a0 并分发 ExecuteMapScript。解释器返回后恢复 d0/a0，尾部恢复 d1 并返回。持有476 跳过此分发和标志写入，不跳过更早新配置。

空分发仍保存解释器状态、初始化工作指针并清文本跳过。终止符在有对话窗口时有条件等待滚动、清 VIEW_SCROLLING_SPEED 并恢复寄存器。无局部文本、移动、选择、加入或地图加载。停滞共享尾部可使476 在返回前已设。只有包装器完成返回才到 ActivateEnemies 紧前的新终点。[start/继续所有者](../../../research/battle26-initialization-and-roster.md#empty-start-hook-supplied-continuation-and-changed-premises)保留精确表/目标地址和调用者分支。

持有 F88 时，BattleLoop 改为恢复保存秒数、清88 和 AI 内存、清 VIEW_TARGET_ENTITY，并在未显式清 d0 的情况下调用 LoadBattle。它跳 `@IndividualTurns_Loop`；之后到 `@Start` 的分支不可达。两种 AI 清除都写 48 个 last-target 字节255 和 48 个 memory 字节0。此传入路线不赋值 incoming72/26、不重置区域标志、不治疗/重建军团、不初始化敌人，也不执行任一钩子。当前地图/战斗/裁切、HP/状态/物品、位置、激活与单次行动状态均为传入值，同时受 LoadBattle 实体重建和中立写入影响。既不建立逐字节保留，也不建立新状态修复。

| 变化状态或中断 | 必须保留的后果 |
| --- | --- |
| 426 缺席，或更早408/424 获准 | 无无条件 Ketto 至 Battle26 初始化。 |
| 476 未设 / 持有 | 钩子分发改变；before 返回后仍有新重置/治疗/编队/加载。 |
| 526 未设 / 持有 | 基础定义与中立，对比可能升级与中立抑制；独立于476。 |
| joined/active 成员死亡、更少、过多、空或变化 | 实际排序成员资格、处理成员上限与 HP 检查支配布置；无固定指定十二人。 |
| 职业、物品、状态、最大值、Bowie 等级或 RNG 变化 | 实际属性/精灵/升级消费者不同；固定表保留所述前提。 |
| Bowie/查找缺失，位置或裁切变化 | 视野后备与实际映射/裁切适用，不修复成员资格/准入。 |
| 159/neutral 位、完成或 F88 变化 | 普通分配与中立重写不同；state159 和 B31 不共用查找。 |
| F88 且当前/保存地图/战斗/单次行动不一致 | Incoming72/26 不强制新配置或首回合状态。 |
| 治疗、加载、中立声明、淡入淡出或 start 尾部中断 | 已完成前缀写入保留；476 可在 start 返回前持有。 |

终点建立条件初始状态与已完成新 start 包装器，不执行激活或授予控制。接续 [Battle26 激活与首次单次行动交接](battle26-activation-and-first-turn-dispatch.md)，消费已接受 #1009，从 ActivateEnemies 到首次单次行动调用。区分区域标志、生成模式与行动资格，包括未变新状态无触发/无生成及条件特卖存储效果。不保证实际行动者。已执行单次行动、结果、战后对话、Higins 招募及后续探索均在本指南外。

## 证据审查与保留结果

完整已接受 #1005 研究及变化状态矩阵已审阅，包括全部 12 布置行、15 敌人行、六定义/当前属性结果、成功物品掩码对比失败、升级不确定性、全部三区域/一点、中立159/B31 和地图/start 参数。已接受 #1006 对峙指南提供连续性，不把场景行动者当战斗员。[遭遇合同](../contracts/battle-encounter-definition.md)、[战斗生命周期合同](../contracts/battle-control-lifecycle.md)与[出战队伍/名册合同](../contracts/party-roster-state.md)保留更广未闭合行为。选定公开夹具、schema 与维护消费者也已检查：

| 表面 / 精确夹具 ID | 已审查证明与限制 |
| --- | --- |
| `sf2-battle-spriteset-data-static-v1` | 头/指针/源码身份结构；rowContentTracked=false 不能证明27 行主体、中立元组、区域或点。 |
| `sf2-battle-global-data-static-v1` | 公开 factSummary 不是私有 facts/Map72 行；中立组计数既不证明 Higins 元组，也不证明 B31 写入。 |
| `sf2-battle-loop-static-v1` | 三十/32 槽、治疗/状态掩码、48 字节 AI 重置和地形指针单位；不是新战斗名册观察。 |
| `sf2-battle-control-static-v1` | F88、区域重置、十二字节项、难度标志选择、VInt/音乐控制事实；不是运行时完成或遭遇意图。 |
| `sf2-battle-cutscenes-static-v1` | 共享 before 检查/不设，start 先设后脚本语义；本身不证明选定槽26 目标。 |
| `sf2-battle-routing-data-static-v1` | 地形选择/别名和路由结构；routeTargetsParsed=false 不能建立 ms_Empty。 |
| `sf2-map-init-static-v1`、`sf2-map-script-engine-static-v1` | 选定加载/人口/布置/行动/解释器消费者与标量单位；不是自然演员表、已完成行动、渲染姿势或控制。 |

维护的夹具/schema 消费者为[精灵集数据](../../../../src/sf2tool/h2/battle_spriteset_data.py)、[全局数据](../../../../src/sf2tool/h2/battle_global_data.py)、[战斗循环](../../../../src/sf2tool/h2/battle_loop.py)、[战斗控制](../../../../src/sf2tool/h2/battle_control.py)、[过场](../../../../src/sf2tool/h2/battle_cutscenes.py)、[路由](../../../../src/sf2tool/h2/battle_routing_data.py)、[地图 init](../../../../src/sf2tool/h2/map_init.py)和[地图脚本](../../../../src/sf2tool/h2/map_script_engine.py)。所有命令为 `uv run sf2 h2` 配合 `battle-spriteset-data`、`battle-global-data`、`battle-loop`、`battle-control`、`battle-cutscenes`、`battle-routing-data`、`map-init` 与 `map-script-engine`。此处 schema/公开审阅**不是私有源码、行、中立元组或目标的新比较**。接受原设计环境缺少源码/数据对；不由此采集、重生成、运行时、环境变化或新维护机制。未发现范围内矛盾；源码分歧返回研究。

[#1005 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1005#issuecomment-6181975872)与 [PR1008 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1008#issuecomment-6181990706)保留 head `033ebbdd`、tree `2daf762b`、base `f1958d33`、CI38663398489（scope14s/research42s）、原 CI38663279580、0.083s 保留数据对检查与完整源码/数据/消费者连接。修正的场景 Higins ID、中立成员资格对比元组证明、inspect.py 遮蔽标准模块，以及精确 before/start 指针表达式断言，连同原诊断与修正回执仍在交接中。

[#1006 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1006#issuecomment-6181920077)与 [PR1007 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1007#issuecomment-6181933475)保留 head `bed5ccec`、tree `ba76c6b2`、CI38663004252（scope15s）、八 schema/548 链接、已接受源码/数据对缺席及已完成检查诊断。[对峙保留审查](battle26-admission-and-geshp-confrontation.md#证据审查与保留结果)保留 #1001/#1002 与链接前序对象、数据对/CI、factSummary 对比行、HTTPS、actor0/服务和已移除 ignored Markdown 上限修正。链接的 #997/#993/#989 已完成失败和未知项仍由各所有者保留；新任务既不重置证据，也不重置已完成套件。

#1010 Issue/PR 交接记录完整正文/diff/导航/链接/锚点、UTF-8/私有边界/精确五路径检查、git diff --check、已配置干净提交 planner 及实际精确 head CI。文档范围使用直接检查，不启动一揽子 normal/full/Python/.NET/Godot/H3/H4/翻译套件。自然可达、传入状态、服务完成、表现/控制及持久性仍为**未知**。独立验收与集成仍由 main-gate 负责。
