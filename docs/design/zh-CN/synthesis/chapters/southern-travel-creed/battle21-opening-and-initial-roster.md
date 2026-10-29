# Battle21 开场与初始名册

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/battle21-opening-and-initial-roster.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

在获准的 Devil's Tail 遭遇中，Skeleton 要求来者离开 Mr. Creed 的领地。Peter 请求通行；Skeleton 拒绝，
随后在另外三名场景行动者出现时下令攻击。之后的战斗配置以传入军团及选定敌人记录替换该角色群。
Fairy Cave 抵达和对峙行动者列表都不选择玩家的交战出战队伍。

**已确认：**已接受静态研究确立下述选定场景、独立标志门槛、坐标域及条件初始化。
**推断：**这段交流表现先拒绝通行、再威胁攻击。**未知：**自然到达、传入状态、服务完成、实际初始名册、表现及可用控制。
这是内容和配置说明，不是已游玩遭遇、战术建议，也不声称攻击威胁成功执行。

本指南接续 [Fairy Cave 探索与 Devil's Tail 返回](fairy-cave-exploration-and-devils-tail-return.md)，
使用[已接受 Battle21 研究](../../../../../research/chapters/southern-travel-creed/battle21-opening-and-initial-roster.md)。
全新入场止于 `BattleLoop:@Start`，在 **ActivateEnemies 之前**；挂起入场止于 `@IndividualTurns_Loop` 前。
交战执行、结果、Creed 入场/室内及地形重建不在本指南范围内。Fnnn 表示标志 nnn；行索引从零开始。
坐标、等待及速度是源码操作数，不是观察位置或实测时长。
后续见 [Battle21 结果与 Devil's Tail 返回](battle21-outcomes-and-devils-tail-return.md)。

## 准入不决定是否运行场景

**已确认：**前篇负责两个洞穴出口、独立默认区域随机请求，以及压缩重载中保留/替换的待处理状态。
请求被使用后，CheckBattle 选择唯一 Map67 行：battle 21、源码区域 `(21,0)`、大小 32x42、触发 `(255,255)`。
传入 F421 即可接纳，不要求抵达位于矩形内。F521 已设置在选择后清除 F421，仍返回 battle 21；F521 未设置则保留 F421。
抵达和 F471 都不会修复缺失的 F421。

相关标志回答不同问题：

| 传入状态 | 此边界的后果 |
| --- | --- |
| F421 未设置 | 无论 F471/F521 如何，CheckBattle 均不接纳 row 21。 |
| F421 已设置 | 接纳 battle 21；F521 控制准入后是否清除 F421。 |
| F88 未设置 | 全新 BattleLoop 入场赋值当前地图/战斗，运行受门槛控制的 before/start 接缝并初始化战斗员。 |
| F88 已设置 | 加载传入已保存战斗状态，在单次行动前恢复；不重跑全新场景或名册初始化。 |
| 全新入场时 F471 未设置 | before 包装器选择 bbcs_21；后续 start 包装器在分发空 start 程序前设置 F471。 |
| 全新入场时 F471 已设置 | 跳过两个选定脚本，但保留全新治疗、初始化和战斗加载。 |
| 全新敌人初始化时 F521 已设置 | 独立于 F421/F471，接纳条件随机升级路径。 |

BattleLoop 首先将 PLAYER_TYPE 清为步行 mode 0，并设置 F399，之后才测试 F88。
全新路径清除秒数、保存传入地图/战斗字、赋值 CURRENT_MAP=67 和 CURRENT_BATTLE=21、安装基础 VInts 并执行 before 包装器。
返回后还原传入字并重写当前地图/战斗，清除区域标志 90–105，治疗符合条件的己方角色，先初始化己方、再初始化敌人，
清除 AI 内存，设置 d0=0 并调用 LoadBattle。此后才执行 start 包装器并到达全新停止点。

before 表 slot 21 选择 `0x4B790` 的 `bbcs_21`；start 表 slot 21 选择 `0x47A4E` 的 `ms_Empty`，完整程序体为 `$FFFF`。
空程序仍进入并终止 ExecuteMapScript，使用共享服务；这不意味着有 Battle21 专属 start 文件。
两个包装器均测试 F450+21=F471。before 从不设置它，bbcs_21 没有标志写入、招募、请求或选择。
因此，部分 before 场景不确立开场标志过渡已完成。

F88 已设置的入场还原已保存秒数，清除 F88 和 AI 内存，清除视野目标，调用 LoadBattle 并直接分支到单次行动循环。
它绕过全新赋值、before/start、治疗及放置；那里不显式清除 d0。传入当前地图、战斗、区域、名册及继续状态必须已相互一致。
传入 battle 21 值不修复不一致存档状态，下述全新表也不能预测挂起名册。

## 裁剪战场与字面场景坐标

**已确认：**源码区域 `(21,0)` 和工作布局原点 `(0,0)` 描述不同域。
CURRENT_BATTLE 为 21 时，完整 Map67 加载器应用基础布局、标志复制及宝箱覆盖，然后 CopyMapBlocks 将 32x42 源码矩形复制到工作原点 0,0。
工作布局每行步长为 64 个字。LoadMapArea 令 layer 1 从 0,0 开始，到 `(31,41)*384` 结束；其他层参数保留选定地图区域值。
这确立裁剪，不确立地形或碰撞。

| 操作数域 | 已接受使用方如何使用 |
| --- | --- |
| Fairy Cave 出口 `(51,23)` / `(57,24)` | 抵达/触发输入，不是战斗员放置。通配可接纳 `(57,24)`，即使它超出源码矩形 X=21..52。 |
| 场景加载摄像机 `(5,18)` | 压缩坐标转为图块单位再左移 7，等价于瓦片操作数乘以 384。 |
| 场景实体行与 setPos | 字面工作坐标；固定行 X/Y 用 `$3F` 掩码，再按 MAP_TILE_SIZE=384 缩放。既不加也不减原点偏移。 |
| 内联行动者移动 | 从当前映射行动者计算相对目的地；无抵达至部署转换。 |
| setCamDest `(23,18)` / `(5,18)` | 字面摄像机瓦片操作数乘以 384，经图层/视差视野状态提交并等待滚动完成。它们不是行动者传送。 |
| 己方/敌人放置行 | 保存字面名册 X/Y；后续 PositionBattleEntities 对非负位置以 `$3F` 掩码，再乘以 384。 |

因此，场景 Bowie `(27,23)` 与 ally placement row 0 `(27,23)` 指同一裁剪坐标。按复制对应源码 `(48,23)`；该对应不是另一条移动命令。
停放的场景行 `(63,63)` 位于战斗矩形之外。摄像机速度 `$30` 直接写入滚动速度字段，不保证实测时长或最终画面。
后续 LoadBattle 使用相同地图/战斗裁剪规则；仅场景加载不会重新计算或修复传入几何。

## 对峙角色群

**已确认：**`0x4B84C..0x4B88C` 的 `ce_4B84C` 包含主头部、七个固定行及终止符。
加载时禁用实体 VInts/显示、清除实体和查找、声明按标志选择的随从、处理固定行，最后以 eas_Idle 在 physical 0 声明 Bowie。
它加载地图精灵并重新启用显示/VInts，不运行 Map67 普通配置 init。

| 逻辑场景行动者 | 声明位置 / 朝向 | 精灵与初始动作 |
| --- | --- | --- |
| Bowie 0，主角色 | `(27,23)`，Left | 按职业选择己方精灵；eas_Idle。 |
| Peter 7，固定己方角色 | `(28,24)`，Left | 按职业选择己方精灵；eas_Init，除非已有查找使其跳过此重复行。 |
| NPC128 | `(8,22)`，Left | Skeleton 精灵 81；eas_Init。 |
| NPC129 | `(9,21)`，Left | Dark Cleric 精灵 140；eas_Init。 |
| NPC130 | `(9,23)`，Left | Dark Madam 精灵 134；eas_Init。 |
| NPC131 | `(63,63)`，Right | Arrow Launcher 精灵 124；eas_Init。 |
| NPC132 | `(63,63)`，Right | Harpy 精灵 108；eas_Init。 |
| NPC133 | `(63,63)`，Right | Orc 精灵 76；eas_Init。 |

Map67 是大地图。F65 选择 Caravan 为 A=30，F67 选择 Peter 7，F68 选择 Oddler 为 B=31。
仅传入这些标志时，physical 1/2/3 为 Caravan/Peter/Oddler；固定 Peter 行跳过，NPC128–133 占据 physical 4–9。
若 F67 未设置而其他两个保留，固定行转而在下一槽位声明 Peter。F66 添加 Sarah/Chester；其他选择器可添加行动者或随后替换 A/B 查找。
没有剧情假设保证这些标志或示例 physical 槽位。

固定 Peter 重复检查测试非零己方查找，不测试成员资格或 HP。已清除的缺失查找是 0；GetEntityAddressFromCharacter 将其解析为 physical 0，没有缺失行动者守卫。
因此缺失 A 的 setPos 可移动 Bowie；缺失 B 的命令也别名到他。固定 Peter 在其随从缺失时给 logical 7 建立映射，不使他加入战斗出战队伍。

CURRENT_BATTLE 为 21 而非 255，因此即使 HP 为零，战斗精灵仍按职业选择。
对小于 30 的真实己方角色，setPos/setFacing 跳过零 HP 行动者；IDs 30/31 和 NPC 绕过该守卫。
动作脚本等待、内联动作及文本没有等效 HP 检查。死亡 Peter 可因 setPos 跳过而保留随从起始位置，之后仍收到移动和台词。
场景后治疗来不及改变已执行前缀。缺失/错误映射行动者及无法到达空闲的等待，都不保证场景成功。

共享木筏尾部仍保留：mode 0 排除玩家作为木筏，但传入 F64 和 RAFT_MAP=67 可在掩码后的已保存坐标声明独立 raft physical 31。
它既不按战斗原点转换也不重写坐标。任意随从容量、映射冲突及可见木筏状态仍为未知。

## 拒绝、Peter 请求与威胁

**已确认：**bbcs_21 完整有序命令流有 37 个条目。已接受研究另负责两个完整内联动作体及实体表；仅顶层命令数不包含这些程序体。
未跳过路径按下列顺序推进：

| 命令 | 场景推进 |
| --- | --- |
| 0–2 | 选择文本 2576，以摄像机 `(5,18)` 加载 Map67，重建对峙角色群。 |
| 3–8 | 先给 Bowie 安装 eas_Init 并等待，再处理 Peter。将 Peter 置于 `(28,24)` Left。给 B 安装 eas_Init 并等待；将 B 置于 `(28,22)` Left、A 置于 `(28,23)` Left。此处 A 不获新动作脚本。 |
| 9–12 | fadeInB、摄像机速度 `$30`，随后 NPC128 的文本 2576 和 single-text 2577。 |
| 13–18 | 依次令 NPC128、NPC129、NPC130 朝 Right；等待 30；摄像机移至 `(23,18)` 并等待滚动；NPC128 说 2578。 |
| 19–21 | Peter 的内联 moveLeft 2、endActions 和空闲等待，随后文本 2579、2580。 |
| 22–25 | 摄像机返回 `(5,18)` 并等待滚动；NPC128 的内联 moveRight 1、endActions 和空闲等待；文本 2581，再等待 50。 |
| 26–32 | 在 `(10,24)` Right 显示 NPC131；等待 40。在 `(10,20)` Right 显示 NPC132；等待 30。在 `(10,22)` Right 显示 NPC133；等待 20。NPC128 说 2582。 |
| 33–36 | 给 NPC131 和 NPC132 安装 eas_Jump，不等待；给 NPC133 安装它并等待空闲，然后 csc_end。没有分别等待 NPC131/NPC132 完成。 |

映射正确且存活、位于 `(28,24)` 的 Peter，moveLeft 2 请求 `(26,24)`；位于 `(8,22)` 的 Skeleton 128 请求 `(9,22)`。
这些相对目的地取决于实际映射/动作进展，既不确立无阻移动，也不确立后续交战部署。

七个源码文本键 `0A10`–`0A16` 提供完整交流：

| 文本 ID | 说话者查找 | 内容 / 控制符 |
| --- | --- | --- |
| 2576 /0A10 | NPC128 | `(Sniff, sniff)...I smell{N}more invaders from the east.{W2}` |
| 2577 /0A11 | NPC128 | `This is the territory of{N}Mr. Creed.{W1}` |
| 2578 /0A12 | NPC128 | `Go away!{W1}` |
| 2579 /0A13 | Peter 7 | `But we have to see him.{W2}` |
| 2580 /0A14 | Peter 7 | `Please let us pass!{W1}` |
| 2581 /0A15 | NPC128 | `No!  Leave now!{W1}` |
| 2582 /0A16 | NPC128 | `Never mind.  Get 'em boys!{W1}` |

**推断：**Peter 请求与拒绝将遭遇表现为不获通行后受到威胁。没有 YesNoPrompt、选择结果或同意分支，
这些文本也不确立 Creed 本人的意图、室内访问或玩家选择。

**已确认服务边界：**文本操作数上字节为 0，下字节选择行动者。
nextText/nextSingleText 根据当前精灵解析立绘和台词，等待滚动，显示游标文本并推进游标。
nextSingleText 额外关闭立绘/文本并 sleep 10。NPC128 是逻辑查找，不是独立保证的立绘。
`{N}` 为换行；W1/W2 命名不同输入等待服务，不代表实测计时或已确认输入。

eas_Init 设置零加速度因子、速度 32/32、禁用加速/减速、可阻挡/可碰撞设置、自动朝向、orientation-up、无浸没、尺寸 24 及精灵更新，随后进入 eas_Idle。
orientation 与 facing 相区分。eas_Jump 禁用自动朝向，提交相对 `(0,-1)`、等待 5、提交 `(0,1)`、等待 5、提交 `(0,0)`，重新启用自动朝向并进入空闲。
最后行动者的等待不证明三者同时完成或均可见跳跃。

动作等待持续至映射行动者到达 eas_Idle；内联动作附加跳至空闲。csWait 用其低字节计数调用 Sleep。
调试 P2-Start/text-skip 路径可省略普通文本和 csWait 服务。终止符在对话窗口索引非零时有条件地等待滚动，清除摄像机速度并还原寄存器。
淡入淡出/加载/显示/VInt、移动、滚动及文本/窗口服务必须返回，才可到达后续命令。
服务停滞时，先前变化无统一回滚；F471 只在后续 start 包装器写入。

## 场景后才放置军团

**已确认：**`0x1B4950..0x1B4AA4` 的 spriteset21 有 12 个己方记录、12 个敌人记录、4 个 AI 区域和 0 个 AI 点。
其战斗员和区域记录均为 12 字节。每个己方行的源码字段为 HEALER1/NOTHING、primary NONE/15、secondary NONE/15、filler 0 和 STARTING。
己方初始化仅依序使用这些位置：

| 放置行 | X/Y | 放置行 | X/Y |
| --- | --- | --- | --- |
| 0 | `(27,23)` | 6 | `(28,23)` |
| 1 | `(27,24)` | 7 | `(28,24)` |
| 2 | `(27,25)` | 8 | `(29,24)` |
| 3 | `(28,26)` | 9 | `(29,23)` |
| 4 | `(28,25)` | 10 | `(30,23)` |
| 5 | `(28,22)` | 11 | `(30,24)` |

这些行索引不是具名己方角色。before 场景后，治疗将存活角色及不死 Peter 7/Lemon 28 恢复至最大 HP/MP，
仅保留已有 STUN/POISON/CURSE 位，并更新属性值。不使任何人加入，也不复活其他死亡角色。
InitializeAllAlliesBattlePositions 将全部 30 个己方 X 清为 -1；UpdateForce 按己方 ID 升序建立传入已加入/出战列表。
死亡成员消耗出战队伍/循环槽位，但不推进放置行。超过 12 个已处理出战槽位的成员离开出战队伍。
符合条件成员更少则定位行动者更少；后备和场景随从不自动包含。空/不一致列表不保证安全空情形，因为在检查计数耗尽前已读取首个条目。

## 十二个敌人记录与四个区域

**已确认：**选定敌人输入如下。全部有 secondary NONE/15、filler 0 和 STARTING=0。
指令集 HEALER1/HEALER3/ATTACKER1/ATTACKER3 编码 0/2/6/9。FOLLOW_ENEMY|n 编码 `$80|n`；NONE 为 `$FF`。
场景精灵 ID 与这些敌人定义 ID 不同：scene NPC129 使用 Dark Cleric 精灵，combatant 129 的基础定义却是 Dark Madam。

| 行 / 战斗员 | 基础类型 / ID | X/Y | 指令集 | 原始物品字 | 主指令 / 区域 |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | Skeleton 17 | `(7,23)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE /1 |
| 1 /129 | Dark Madam 70 | `(8,22)` | ATTACKER3 | PROTECT_RING\|EQUIPPED\|UNUSED_ITEM_DROP `$4094` | FOLLOW_ENEMY\|0 /1 |
| 2 /130 | High Priest 79 | `(9,24)` | HEALER3 | NOTHING `$007F` | FOLLOW_ENEMY\|0 /1 |
| 3 /131 | Dark Madam 70 | `(13,18)` | ATTACKER1 | NOTHING | NONE /0 |
| 4 /132 | High Priest 79 | `(15,19)` | HEALER3 | NOTHING | FOLLOW_ENEMY\|3 /0 |
| 5 /133 | Orc 12 | `(23,14)` | ATTACKER1 | NOTHING | NONE /15 |
| 6 /134 | Orc 12 | `(19,16)` | ATTACKER3 | NOTHING | FOLLOW_ENEMY\|7 /15 |
| 7 /135 | Skeleton 17 | `(21,17)` | ATTACKER1 | MEDICAL_HERB\|USABLE_BY_AI `$2000` | NONE /15 |
| 8 /136 | Harpy 44 | `(15,25)` | HEALER1 | NOTHING | NONE /2 |
| 9 /137 | Harpy 44 | `(13,31)` | HEALER1 | NOTHING | NONE /3 |
| 10 /138 | Orc 12 | `(20,29)` | ATTACKER1 | NOTHING | NONE /15 |
| 11 /139 | Arrow Launcher 60 | `(26,33)` | ATTACKER1 | NOTHING | NONE /15 |

每个区域前导字节为 4,0，随后按保存顺序记录下列四个坐标对，尾字节为 0,0：

| 区域 | 有序坐标 |
| --- | --- |
| 0 | `(0,0)`、`(0,40)`、`(31,15)`、`(31,0)` |
| 1 | `(0,21)`、`(9,32)`、`(20,23)`、`(11,13)` |
| 2 | `(0,15)`、`(0,41)`、`(23,41)`、`(23,15)` |
| 3 | `(0,1)`、`(0,41)`、`(31,41)`、`(31,37)` |

这些保存的指令、指令集及区域操作数不确立实际跟随、治疗、攻击选择、多边形可达或激活。
敌人初始化扫描 128–159：12 个 STARTING 行进入 InitializeEnemyStats；缺失槽位取得 max/current HP 0 和 X=-1。
初始化器解析可能升级，复制选定定义、填充 HP/MP、写入行坐标，将定义 movetype 与行指令集合并，保存类型/触发/指令，处理物品，
构建激活位，应用共享难度调整并更新属性值。这里不声称派生属性或 AI 结果。

### 成功添加会移除戒指装备位

**已确认：**原始物品字不等于已安装物品栏。InitializeEnemyItems 遇 NOTHING 即返回，保留定义物品栏。
否则实际 j_AddItem 目标寻找四槽中首个掩码为 127 的空槽。成功时 AddItem 自身用 `$807F` 掩码 d1，写入该字并返回 d2=0。
满物品栏返回 d2=1，不执行该掩码或写入。调用方不按 d2 分支；随后测试 d1 的装备位 7，只有已设置才扫描/装备匹配物品。

未改变的基础 Skeleton 有已装备 Broad Sword 和三个空槽；Dark Madam 有已装备 Power Stick 和三个空槽。
因此成功添加为 rows 0/7 安装普通 Medical Herb `$0000`，为 row 1 安装普通 Protect Ring `$0014`。
掩码在调用方测试装备前清除 AI `$2000`、drop `$4000` 及 equipped `$80`，所以未改变的 row 1 Dark Madam 沿此路径获得**未装备戒指**。
升级/改变定义或满物品栏保留实际成功/失败分支。行标签和此直接初始化不推导下游 AI 使用、掉落/奖励、物品效果或派生属性。

### F521 允许升级，不保证替换

**已确认：**battle 21 在完整十一项随机战斗列表中。IsBattleUpgradable 接纳它，DetermineBattleUpgrade 测试 F500+21=F521。
F521 未设置保留原始类型；F521 已设置则使用原始 movetype、Bowie 按职业调整后的有效等级与 battle 21 的相对值、选定范围、RNG 及排除/回退逻辑。
正等级差除以 10 的商为零时仍保留原始类型；进入该路径不保证敌人改变。

Regular Skeleton/Orc 及 Brass Gunner Arrow Launcher 使用 melee 表；Mage Dark Madam 使用 mage，Healer High Priest 使用 healer，Flying Harpy 使用 airborne。
airborne 排除列表为空：使用方在排除 DBF 循环前从零长度减去 1，源码注释警告升级失败。
保留该边界，不保证成功处理空列表或最终 RNG 结果。升级后名册的安装仍以代码返回为条件；即使复制定义改变，行位置、指令集、指令及物品仍是输入。
准入时清除 F421 不禁用此处 F521；F471 不控制该使用方。

## 战斗加载替换场景角色群

**已确认：**LoadBattle 淡出、加载瓦片集、清除 VInts 并调用 PositionBattleEntities。
它清除旧实体/查找，为非负战斗员 X/Y 声明行动者，在普通敌人遍历中排除带中立标记敌人。
普通行动者使用站立动作及 Down 朝向。先前 physical 映射、停放/显现 NPC 和动作脚本不是新名册。
完整中立表无 Battle21 行：F521 未设置的扫描到达终止符，F521 已设置的入场跳过它。
两者均不添加中立者，也不把场景 Oddler/Caravan/Peter 作为中立者带入战斗。

全新 d0=0 选择视野目标；缺失映射在坐标读取时回退 physical 0，不修复成员资格。
LoadBattle 初始化精灵、加载当前地图/视野及实体精灵、安装基础 VInts、调用既有地形加载器、提交地图音乐并淡入。
之后空或跳过的 start 接缝到达上述停止点。地形解码、可见/可听结果、响应控制及后续激活/单次行动均在已证明边界之外。

## 证据与剩余问题

**已确认溯源：**本指南使用 main `0a4e8c6980086ad712ce1bcde740583d21f69a87` 上已接受研究，
源码为 [SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA 零售 ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
[研究复现边界](../../../../../research/chapters/southern-travel-creed/battle21-opening-and-initial-roster.md#counterexamples-reproduction-and-limits)
负责具名源码路径、地址、完整选定命令/实体/文本/放置及使用方关联。本指南不重新调查原始源码，不重新生成比较或采集运行时。

| 已审阅所有者 | 支持内容与范围外事项 |
| --- | --- |
| 已接受 Battle21 研究、前篇返回及共享 Battle20 入场使用方 | 完整选定准入、before/start、裁剪布局、随从/HP、源码行、随机/中立及实际物品掩码关联。局部内容由这些已接受选定关联确立，不由公开汇总数量确立。 |
| `sf2-battle-cutscenes-static-v1`，[fixture](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，expected.cutsceneFacts.intro；[路由合同](../../../contracts/battle/battle-cutscene-routing.md) | 共享开场检查/写入顺序。旧合同对精确路由目标/空执行的未知项是汇总限制；已接受 Battle21 研究提供这些选定关联，不关闭其他路由。 |
| `sf2-battle-loop-static-v1`，[fixture](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)，expected.lifecycleFacts.rosters/betweenBattleHealing；[生命周期合同](../../../contracts/battle/battle-control-lifecycle.md#入口与回合调度) | 共享槽位/治疗边界。共享入场顺序摘要与已接受精确研究轨迹一致：before → reset/heal/ally then enemy initialization/AI clear/load → start。Battle01 激活/单次行动观察不证明 Battle21 行为。 |
| `sf2-battle-spriteset-data-static-v1`，[fixture](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，table.BattleSpriteset21 和 expected.facts.rowContentTracked=false；[遭遇合同](../../../contracts/battle/battle-encounter-definition.md) | 仅地址与汇总形状。汇总总数和宏计数均不序列化全部 24 个选定战斗员行或 48 个区域字节。 |
| `sf2-map-script-engine-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，expected.programSummary 与命令族 | 共享脚本/使用方上下文。已接受保留语料选项 entryLabel=bbcs_21 有 37 个命令；内联程序体/实体行及字面文本仍由研究分别负责。 |

已检查 schema/验证器边界为 [cutscene fixture schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json) 和
[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、
[loop fixture schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json) 和 [battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、
[spriteset fixture schema](../../../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json) 和 [battle_spriteset_data.py](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)，
以及 [script fixture schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json) 和 [map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)。
既有源码/溯源/模型检查保留自身范围；不重新生成 H2 载荷。

保留 [PR886 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172444717)、
[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172589433)
及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/886#issuecomment-6172605301)，
包括保留比较收据、原始/最终 CI、已解决检查失败及链接的前序结果。审阅记录定位结果；原版声明由跟踪研究及精确已接受 Git 对象负责。
本指南的检查、planner 和最终 CI 属于 Issue 887 交接。

**未知：**自然准入；实际标志、出战队伍、HP/职业、随从/木筏映射及冲突；淡入淡出/加载/文本/窗口/滚动/动作服务完成；
依赖 RNG 的升级结果（包括空 airborne 排除边界）；实际初始名册、表现/声音、可用控制及存档持久性。
F421 缺失、F471 已设置而仍全新初始化、F521 已设置而开场未见、缺失 A/B、后续治疗前死亡 Peter、改变的出战队伍/容量及不一致 F88 状态，
是约束静态过度声明的传入状态反例，不是已执行场景。缺失观察不会自动创建运行时队列，也不表示战役或设计 epic 完成。
