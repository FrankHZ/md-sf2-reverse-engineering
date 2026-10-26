# Willard Burrow 探索与 Battle23 准入

> 本文是[规范英文原文](../../synthesis/willard-burrow-through-battle23-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

**已确认：**Willard Burrow 有三名基线居民、八条局部/出口传送记录，以及独立获准的对峙。完成对峙会解锁 Battle23 并请求返回 Map27；调用方的场景标志与待处理请求消费是之后两个不同边界。**推断：**居民威胁要吃掉出战队伍。**未知：**自然迷宫路线、传入行动者/状态、行动/表现完成、可用控制及持久性。本指南止于 BattleLoop 调用，早于初始化。

从此终点可继续阅读 [Battle23 初始编队与挂起入场](battle23-initialization-and-roster.md)，它消费已接受 #932 研究，止于新激活/回合或已保存单次行动读取之前。本前篇保留自身准入边界。

本指南从 [Floor World 对外准入](floor-world-interactions-and-exits.md)继续，使用已接受的 [Willard Burrow 研究](../../../research/willard-burrow-through-battle23-entry.md)，依据 [#934](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/934) 及 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。它解释原版内容和条件消费者，不提供迷宫解法、期望游戏玩法/UI 政策或重制实现。Fnnn 表示标志 nnn；坐标、速度和等待是源码操作数，不是实测性能。下文源码路径相对于研究固定的 `disasm/` 根目录。

## 入场与两种新实体群

Floor World 出口通常请求 Map27 `(2,7)`、Up，受[坐标限制](#坐标覆盖保留商的高字节)约束。MainLoop 在探索前调用 SwitchMap 和 CheckBattle。SwitchMap 不替换 Map27。战斗解锁 F423 已设置时，CheckBattle 直接选择 Battle23，即使场景标志 F781 未设置；对峙并非必经入场钩子。F423 未设置时，Map27 改为准入探索。

显式地图探索入场中，ExplorationLoop 清待处理类型、调整 STEP_COUNTER、治疗符合条件的存活/不死己方角色、请求淡出并禁用摄像机跟随。它设置 CURRENT_MAP27/CURRENT_BATTLE255，加载瓦片集，等待淡入淡出，清 VInts，选择/初始化实体并加载精灵。它清临时标志256–383 并设置 F80；F423/F523/F781 不受该范围影响。公共入场初始化精灵帧状态，以 physical0 为目标，调用 LoadMap、安装基础 VInts 并分发 init，然后执行条件音乐/淡入淡出工作。init 返回本身不确立可用控制。

### 三名居民或空的新实体群

**已确认：**`data/maps/mapsetups.asm` 默认选择 ms_map27，完成战斗 F523 已设置时选择 ms_map27_flag523。两者六指针表共享 EntityEvents、ZoneEvents、AreaDescriptions、Section5 和 InitFunction，仅实体群指针不同。每个事件访问器重新选择当前配置。完整局部 init 是 `s6_initfunction.asm` 的 `$59FB6..$59FB8`，仅含 rts：不启动场景、不写 EGRESS_MAP，也不修复战斗或场景标志。

基线按顺序声明下列全部三行：

| 逻辑居民 | 声明位置 | 朝向 | 地图精灵 | 行动 |
| --- | --- | --- | --- | --- |
| 128 | `(43,20)` | Left2 | RAT79 | eas_Init |
| 129 | `(43,21)` | Left2 | WILLARD154 | eas_Init |
| 130 | `(43,22)` | Left2 | RAT79 | eas_Init |

这些八字节记录占 s1_entities.asm 的 `$59F72..$59F8A`。该文件自身没有终止符。`data/maps/mapsetupsstorage.asm:380–389` 包含两个指针表，随后包含 s1_entities.asm，紧接 s1_entities_523.asm 和事件文件。后者以 `$59F8A` 的 ms_map27_flag523_Entities 开始，即共享 msEntitiesEnd。因此基线消费三行和该终止符；F523 实体群从终止符开始，消费零行。这依赖实际 include 相邻关系。

InitializeMapEntities 清 49 个物理记录和 64 个查找字节，从 physical1 分配选定随从，然后居民，最后在 physical0 以请求位置/朝向和 eas_Idle 分配 Bowie。DeclareNewEntity 写当前位置/目的地、速度/行进、精灵、朝向、物理 ENTNUM、行动/计时器及标志。共享 SPRITE_SIZE 为 16；之后执行随从终止和木筏处理。这些是声明值，不是观察到的移动。

三个精灵均为 30–239 范围的普通非己方角色值。其查找字节为32–34：GetEntityAddressFromCharacter 对逻辑字节掩码，并对 ID128–255 减96。这些行没有已加入军团、HP、职业或己方行重复跳过。这里 logical129 是 Willard，在显式加载时替换 Floor World 年轻人的实体群。这些行既不确立招募，也不确立战斗敌人激活，并非 Battle23 名册。

### 随从与别名仍是传入前提

场景使用 B=FOLLOWER_B31 和 Peter=ALLY_PETER7。仅有 F67/Peter、F68/B(Oddler)，映射一致且各不相同并无覆盖时，两者以 eas_Follower1/2 占 physical1/2，居民128–130 占 physical3–5。仅己方成员资格不分配 Peter；F67 才是随从选择器。Peter 的 HP/职业影响地图精灵，包括探索中死亡己方角色的 BLUE_FLAME。场景自身既不验证行动者，也不复活他们。

已接受的[完整随从选择](../../../research/willard-burrow-through-battle23-entry.md#admission-setup-and-the-complete-population)顺序为 F66/Sarah、F67/Peter、F72/Kazin、F73/Slade、F74/B(Astral)、F66/Chester、F68/B(Oddler)、F75/Luke、F69/C(Fairy)、F70/B(Astral)、F74/A(King)、F71/Lemon、F74/D(Minister)、F76/Zynk、F77/A(Old Man)、F82/Frayja、F83/A(King)、F83/D(Minister)、F84/Rohde。Map27 既非无随从地图，也非大地图入场，因此 F65 不提供 Caravan/A 前缀。只有选中行推进行动游标。更多随从会移动居民的物理槽位；后续 B/A/D 声明覆盖逻辑映射，早先物理记录则保留。任意宽泛的标志组合并非已证明支持的输入。

F523 已设置的新入场让居民查找32–34 保持零。零解析为 physical0，并非安全跳过缺席行动者。`$FF` 或其他无效查找值同样没有安全的缺席行动者守卫。因此 F523 已设置且 F781 未设置时，场景可获准，其居民命令却别名到主角。即使预期居民存在，逻辑替换也可改变命令实际消费谁的行动、目的地或说话者。

新加载木筏处理在 mode2 让主角成为 RAFT，将随从以 eas_Standing 停放在原始 `$6000,$6000`，并在 `(64,64)` 声明 physical31。其他模式要求 F64 和 RAFT_MAP27 才有已保存木筏，使用经 `$7F` 掩码的保存坐标。查找63 指向 physical31；传入状态可覆盖该记录。C158 使用查找62，D159 与木筏共享查找63，其映射可被木筏声明替换。两者均不重叠居民32–34。选定场景命令均不改变 PLAYER_TYPE、木筏状态或随从成员资格。

### 当前地图穿行保留行动者

无战斗的 MAP_CURRENT255 入场保留 CURRENT_MAP、实体群/查找和工作布局。它跳过显式瓦片集、分配、临时标志清除与 F80，在公共 LoadMap/init 前使用 UpdateMainEntityProperties/DeclareRaftEntity。主角位置/朝向和精灵被更新；木筏处理在 mode2 返回，mode1 跳过随从重新定位，否则先重新定位列出的随从，再处理已保存木筏存在/缺席。这里改变 F523 不重建既有行动者。共享的仅返回 init 不修复映射。新实体群和保留式穿行是不同边界。

## 局部交互与八条穿行请求

**已确认：**实体事件表仅有 flags0 默认记录和返回回调。普通居民激活仍解析实际立绘/语音，在回调前后禁用/恢复实体 VInt 工作，之后关闭窗口。flags0 既不请求朝向改变，也不请求恢复。回调不含文本、招募、物品栏、标志或请求写入；在此表中，与 Willard 交谈不是对峙触发器。

AreaDescriptions 仅为 rts。其包装器从 d7=0 开始，因此不处理局部描述。Section5 同样有默认记录/rts；包装器从 d6=0 开始，清语音，将调用方 Y 存入 EVENT_RELATIVE_POSITION，关闭窗口并等待 physical0。宝箱物品和其他物品均仅含 endWord。这些是空的局部表面，不代表全局物品行为、搜索回应、服务奖励或其他获取路径普遍缺席。直接居民激活与局部物品使用均不调用对峙。

### 全部八条不滚动记录

`data/maps/entries/map27/6-warp-events.asm` 的 `$A93F6..$A9438` 恰含以下八条完整 type0/warpNoScroll 记录，后接 endWord。各记录在目的地朝向后均有零填充。朝向是输出，不是接近条件。

| 行 | 触发 X/Y | 请求地图 | 目的地 X/Y | 朝向 |
| --- | --- | --- | --- | --- |
| 0 | `(2,8)` | FLOOR_WORLD26 | `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 | `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 | `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 | `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 | `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 | `(3,3)` | Right0 |
| 6 | `(255,0)` | CREED_MANSION33 | `(15,9)` | Right0 |
| 7 | `(21,255)` | CREED_MANSION33 | `(15,9)` | Right0 |

行0–5 匹配精确坐标。行6 匹配任意 X/Y0，行7 匹配任意 Y/X21；两者均匹配时首个胜出，目的地相同。不存在反向 `(3,3)` 记录。仅此表不证明迷宫解法、可行走性或完成的战后路线。

普通 esc02_controlCharacter 中，掩码 `$3C00` 下候选布局类别 `$1000` 调用 WarpIfSetAtPoint。匹配器将传入原始候选坐标有符号除以384，按顺序扫描当前地图记录直至 word-1，并将负触发字节作为通配符。匹配时写待处理 type 1，将 type/map/X/Y 复制到 PARAM1–4，朝向/填充复制到 PARAM5–6，并设置 WARP_SFX=SFX_WARP。表相等不证明移动/布局调用方实际产生该请求；其他载具调用方的合法性不因共享匹配器而扩大。

下述[待处理消费者](#场景返回与待处理消费先于-battleloop)及坐标限制同样适用于这些记录。SwitchMap 不替换26/27/33/255；CheckBattle 没有26/33 行。因此 Floor World 和 Creed 在普通前提下准入探索，止于实体群/init 之前。CURRENT255 为战斗查找解析 CURRENT_MAP27：F423 未设置则保留原255 请求以探索；已设置则选择 Battle23，替代局部穿行。没有记录/匹配器测试 F781 或 F523，或消费物品。F523 影响新实体群或完成战斗检查，不是退出许可。选定局部命令均不写 EGRESS_MAP，也不保证战后抵达 Creed。

## 目的地派生的区域（事件）准入对峙

**已确认：**特定区域（事件）为 `msZoneEvent 255,19,Map27_ZoneEvent0`，另有默认返回。RunMapSetupZoneEvent 比较 d1/d2 低字节，将 `$FF` 作为通配符：任意 X/Y19 选择此回调。F781 已设置则返回而不运行场景；未设置则调用 cs_59FB8，仅在它返回后设置781。不匹配通过默认返回。F423/F523 或出战队伍/HP 均不门控它。

Y19 描述请求，不一定是当前行动者 Y 或已完成一步。esc02 从当前坐标建立候选，然后在布局分类前检查方向、区域/斜坡和实体。loc_5294 的掩码类别 `$1400` 写 event6。其参数字来自 `(XDEST+d4)/384` 和 `(YDEST+d5)/384`，使用选定步长增量，早于剩余地形/调试检查与移动赋值。因此布局、模式/行动、目的地和待处理状态仍是前提；仅当前 Y 相等不准入此事件。

WaitForEvent 优先处理待处理类型而非 A/C 输入，有待处理事件时跳过控制器安装。ProcessMapEvent 在分发前清采样类型。Type6 无等待地为 physical0 安装 eas_Init，从 PARAM1/PARAM3 读取 d1/d2 并调用区域（事件）包装器。Trap6 激活实体 VInt 工作并执行脚本，不增加行动者验证或剧情状态修复。

### 按阅读顺序呈现完整场景

cs_59FB8 位于 `$59FB8..$5A09E`，有41 条顶层命令，索引0–40。[已接受精确命令表](../../../research/willard-burrow-through-battle23-entry.md#y19-zone-scene-entry-and-the-complete-confrontation)负责每个源码顺序操作数。下列阶段覆盖完整序列；合并描述的行动者仍按顺序执行命令，并非同时。

第一阶段布置 B 和 Peter，让他们评论气味。视野随后移至 Willard 与两只鼠，表现笑声、旋转和跳跃，再由 Peter 回应以及发出开饭威胁。后面的标志和传送命令将这场布置好的遭遇连接到条件战斗准入；文本自身不实现对出战队伍的伤害。

| 索引 | 有序内容与完成边界 |
| --- | --- |
| 0–9 | 设置文本游标1733；先等待 B 的 Init，再等待 Peter；摄像机速度48，等待目的地 `(22,15)`；等待 B 的绝对目的地 `(26,20)`；csWait5；启动 B 的两轮 Right/Left 转向而不等待完成；csWait120；B 文本1733。 |
| 10–13 | 等待 Peter 绝对目的地 `(27,20)`；让 B 朝 Right；使 Peter shiver；Peter 文本1734。朝向和 shiver 独立于移动。 |
| 14–20 | 等待摄像机目的地 `(36,17)`；等待居民129 的绝对目的地 `(41,21)`；129 文本1735，修饰符`$C0`；按顺序启动128、129、130 的循环高速旋转；csWait40。 |
| 21–32 | 按顺序对128、129、130 以等待的 Init 替换旋转；各自朝 Left；启动128/129 Jump 而不等待，然后等待130 Jump；按顺序 startEntity128、129、130。 |
| 33–37 | Peter 文本1736；129 nextText1737，再 nextSingleText1738，均为`$C0`；再次启动128 Jump，并等待130 Jump。129 没有第二条 Jump 命令。 |
| 38–40 | setStoryFlag 23 写 F423；请求 warp27 `(0,0)`、Right；csc_end 返回。调用方 F781 写入与包装器等待在之后。 |

### 具名行动与等待实际做什么

eas_Init 清零加速度因子，设置速度32/32，禁用加速和减速，设置 entityObstructable，清 entity/mapUncollidable，启用 autoFacing，设置 orientUp，清 immersed，将共享大小设为24，更新精灵并跳至 eas_Idle。其标志写入设置 FLAGS_A bit 7，清 bits 5/6。orientUp 将方向位设为0，不是朝向 Up。Init 不含位置重置、目的地等待或 motion-ON 操作。Idle 等待1，然后分支到自身。

eas_2xRightLeft 依次设置朝向 Right/Left/Right/Left，每次更新精灵并等待30，再跳至 Idle。eas_RotateRightHighSpeed 设置 motion OFF 和 autoFacing OFF，然后朝 Left/Up/Right/Down，每次更新精灵并等待1，再循环。两者改变朝向/精灵而非瓦片位置。Motion OFF 写 ANIMCOUNTER=-1，不是通用速度或位置停止。后续三个 Init 替换按顺序结束旋转，但不重启动画计数器。startEntity 写 ANIMCOUNTER=0，不分配行动者。

csc15 替换解析所得行动者的行动指针，以物理索引作为读取计时器初值。等待形式轮询该记录指针直至 eas_Idle；没有超时、HP 守卫、全行动者等待或固有碰撞位清除。别名或后续替换会改变等待所证明的内容。实体 VInt 工作跳过原始 X 大于等于 `$7000` 的记录，因此无效/停放状态可能阻碍进展。等待 Init 是行动指针边界，不是目的地抵达。

三个 setDest 操作数的 bit 15 均未设置。csc29 将绝对瓦片字乘384，写目的地/行进及有符号轴速度幅度32，然后等待当前 X/Y 等于行动者当前目的地。它既不瞬移，也不重置行动指针；替换可改变等待目标。这些 B、Peter、129 目的地调用没有 HP 守卫。

eas_Jump 禁用 autoFacing，执行 moveRel`(0,-1)`，等待5，执行 moveRel`(0,1)`，等待5，执行 moveRel`(0,0)`，启用 autoFacing 并跳至 Idle。每个 esc04 增量相对于**当时当前位置**，使用当前速度字节，而非实体群行或前一目的地。完成的 Init 提供32/32 并清 FLAGS_A bit 5；位未设置时，经 HasSameDestinationAsOtherEntity 绕过 esc04 的目的地冲突分支。改变 bit 5 可使扫描发生作用。这不是通用碰撞豁免。

Jump 不含 ac_waitDest。其 wait5 使用行动读取计时器；moveRel`(0,0)` 以当时当前位置为目标。它不一定向上/下走满一瓦片，也不一定返回初始 Y。两组 Jump 均仅等待130 的指针到 Idle，从不等待128/129。相同主体不意味着同时完成、最终位置或总场景时长，尤其存在别名、替换或 VInts 中断时。

朝向/start 消费者有选择性存活状态守卫：小于30 且无敌人位的真实己方角色在 HP 零时跳过。B31 和128–130 绕过它，包括全部选定朝向/start 命令。Init/行动、目的地和 shiver 调用不因操作数为 Peter7 就获得该守卫。场景既不治疗、复活，也不招募 Peter 或居民。

Peter 的 shiver 保存共享精灵大小/计数器，使用大小21/计数器-1，三次设置/清除 FLAGS_B bit 3，每次改变后更新精灵并睡眠5，最后恢复保存值。它实现精灵效果，不是气味传感器或伤害规则。摄像机速度写 VIEW_SCROLLING_SPEED=48；每个目的地禁用目标跟随并等待滚动。有效摄像机消费者将瓦片乘384，并应用视野视差/偏移及自动滚动守卫。这些是请求，不是观察到的取景或行动者位置。

### 六条文本、实际说话者与窗口处理

没有调试跳过时，游标1733 恰推进六条文本：

| ID | 说话者 / 修饰符 | 内容含义 | 控制 |
| --- | --- | --- | --- |
| 1733 | B / 0 | 嗅闻并将气味比作沟渠 | W1 |
| 1734 | Peter / 0 | 询问是否是传闻中 devils 的气味 | W1 |
| 1735 | 129 / `$C0` | 笑声 | W1 |
| 1736 | Peter / 0 | 说这些鼠很巨大 | W1 |
| 1737 | 129 / `$C0` | 欢迎 | W2 |
| 1738 | 129 / `$C0` | 宣布开饭时间 | W1 |

研究的[字面文本表](../../../research/willard-burrow-through-battle23-entry.md#six-texts-and-actual-speaker-selection)保留键06C5–06CA 和 `{N}` 换行。**推断：**文本与即将开始的战斗暗示吞食威胁。选定命令均不实现气味检测、生物成长、食物消费或出战队伍伤害。

实际逻辑映射/地图精灵选择立绘与语音。B/Oddler 为立绘40/bleep4；B/Astral 为31/6。Peter 基础职业为7/4，转职后为48/5；HP/职业或改变的映射可改变它们。WILLARD154 不在完整 sprite-dialog 表中，因此查找得到 DEFAULT(-1)/bleep6，而非 Willard 立绘。`$C0` 请求右侧/镜像 bits 15/14（129 为 `$C081`），不会创建缺席立绘；csc1D 保留已打开立绘。均未使用 `$FFFF` 无说话者哨兵值。缺失或别名查找可改变属性，而非安全抑制语音。

nextSingleText/csc00 等待滚动、显示/递增游标、关闭立绘/文本并睡眠10。1737 的 nextText/csc02 等待滚动并显示/递增，但保留窗口供后续 csc00 使用。W1/W2 用 VInt 等待轮询方向/A/B/C 输入；W2 还更新提示标记，并在接受时请求 SFX_VALIDATION。W1 没有该声音请求。字面控制既不证明已接受输入，也不证明可见/可听输出完成。

ExecuteMapScript 入场时清 SKIP_CUTSCENE_TEXT。调试模式加 P2 Start 可在分发时锁存它：跳过的文本命令消费操作数而不显示或递增游标，显式 csWait5/120/40 睡眠被绕过。内部 shiver 睡眠、行动计时器/等待、标志和传送仍运行。中途锁存会改变后续游标值；六行全部显示仍有条件。

## 场景返回与待处理消费先于 BattleLoop

**已确认：**setStoryFlag 23 加战斗解锁基数400，写 F423，而非 F523/F781。之后场景传送写 type 1 和 PARAM1–5=`0,27,0,0,Right0`。它不即时加载地图、不清后续参数，也不设置 WARP_SFX。不同于表传送，其声音消费者使用先前传入的 WARP_SFX。

csc_end 在仍有对话窗口时条件等待滚动，清 VIEW_SCROLLING_SPEED 并恢复寄存器。仅场景返回后调用方才设置 F781。RunMapSetupZoneEvent 关闭窗口、等待一个 VInt，再等待 **physical0** 当前 X/Y 等于当前目的地，之后恢复寄存器。这与130 的 Idle 等待不同，不等待其他行动者。缺席居民别名可让场景命令移动或替换主角，因此包装器完成仍有条件。

这些返回后，ExplorationLoop 到达 SetMoveSfx/WaitForEvent。仍存活的待处理传送优先于输入，跳过控制器安装及摄像机重新指向0。ProcessMapEvent 清采样类型、请求存储的 WARP_SFX 并清它、让 physical0 空闲、移除调用方返回地址以离开 ExplorationLoop，然后清零 d0–d4。它加载请求地图27，通过 UpdatePlayerPosFromMapEvent 采样主角位置/朝向，再应用目的地字节/朝向。在一致的非负局部坐标前提下，返回 `d0=27,d1=0,d2=0,d3=Right0,d4=0`。目的地 init 尚未运行。

### 坐标覆盖保留商的高字节

有符号原始 X/Y 除384 首先填充坐标字；非负目的地字节仅替换低字节。原始 X=-384 得商 `$FFFF`；目的地0 产生 `$FF00`，而非 `$0000`。这适用于场景及全部八条表请求。因此普通目的地元组要求一致的非负局部坐标。保留的 MAP_CURRENT 探索中，UpdateMainEntityProperties 测试低字节负性，但将整个字乘384 并存低乘积字。它不规范异常高字节。通配战斗触发同样不修复畸形坐标。

### 独立标志与条件战斗准入

SwitchMap 保持27 不变。CheckBattle 仅在 F423 已设置时选择唯一战斗行23：`27,23,14,27,16,255,255` 表示地图27、区域原点23/14、宽27/高16 和通配触发 X/Y。它写这些区域字节。F523 已设置时清 F423，但仍返回已经选中的索引23。MainLoop 将该索引移入 d1，并以 d0=27 调用 j_BattleLoop。**此处为终点：**早于 before/start 钩子、初始化、名册、AI、激活、回合或结果。

| 具名接缝的改变状态 | 有条件的结构结果 |
| --- | --- |
| 首次 Map27 准入时 F423 已设置，F781 任意 | 直接选择 Battle23；无需探索场景 |
| 新 F423 未设置/F523 未设置 | 三个居民行，共享返回 init |
| 新 F423 未设置/F523 已设置/F781 未设置 | 零居民行；场景仍可准入，查找缺失/别名 |
| Y19 请求/F781 已设置 | 无场景、新 F423 写入或场景传送；包装器仍有条件地返回 |
| F781 已设置/F423 未设置 | 普通入场保持探索；局部不修复被抑制场景 |
| F781 未设置、行动者一致、场景/包装器完成、传送存活 | 按顺序为 F423、请求、场景返回、调用方 F781、physical0 等待、消费、Battle23 调用 |
| CURRENT255 穿行时 F523 改变 | 保留现有行动者；访问器重新选择共享事件表 |
| CheckBattle 时 F423 与 F523 均已设置 | 清 F423，保留当前 Battle23 选择 |
| 场景或包装器中断 | 早先行动者/文本/摄像机/标志/请求效果可能保留；后续边界未获证明 |
| 产生与消费之间待处理请求被覆盖 | 由共享当前参数决定，而非不可变原始元组 |

F423、请求存储、F781 与包装器之间没有回滚。场景返回前中断可让 F781 未设置且保留早先效果；调用方 F781 后、消费前失败可抑制下次场景，却不证明准入。这些是静态顺序/状态区别，不是复现的中断场景。选定局部命令均不设置完成 F523、不授予物品，也不保证战后 Floor World/Creed 路线。

## 证据审阅与保留限制

**已确认溯源：**已接受 main `53df4b5cbf6d0b75136d5dfa737d125e2f08d551` 包含 #928/PR931 和 #930/PR933。研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[研究复现所有者](../../../research/willard-burrow-through-battle23-entry.md#reproduction-and-remaining-limits)保留源码符号/地址、精确场景/行动/传送关联与未改变载荷比较。本综合指南不增加原版观察或提取。

原英文切片的对抗审阅阅读完整已接受研究和前篇，再按下列选定公共 fixture/schema/验证器边界核对解释。[配置选择](../../../research/map-event-flag-route-selection.md)、[交互状态](../../../research/map-event-interaction-state.md)、[请求消费](../../../research/map-event-request-consumption.md)及[探索](../contracts/map-exploration.md)保留各自声明。

| 声明 | 可执行所有者与选定边界 |
| --- | --- |
| 共享事件表与无声/默认记录 | `sf2-map-events-static-v1:routing-setup`，tests/fixtures/h2/map-events/routing-setup.json：entity row640、zone104/105、item39、六个配置/类别关联；schemas/h2/map-events-routing-setup.schema.json。 |
| 带守卫的调用方与返回 | `:entity-programs` program304/rts，`:zone-programs` programs52/53：chkFlg781、bne default、script cs_59FB8、setFlg781、rts；匹配的 entity/zone/target-program schemas 与 map_events.py/map_events_fixture.py。语料顺序不是游戏时间顺序。 |
| 行动、文本、摄像机与生命周期消费者 | `sf2-map-script-engine-static-v1`，tests/fixtures/h2/map-script-engine-static-v1.json：行动桥、布置、生命周期/表现、对话、摄像机、转换和脚本控制事实；经 map_script_schema.py 使用 schemas/h2/map-script-engine-{entity,dialogue,map-control,script-control}.schema.json；map_script_engine.py `_verify_map_script_engine_fixture`。 |
| 待处理优先级/重置/消费 | `sf2-map-event-request-consumption-static-v1`，tests/fixtures/h2/map-event-request-consumption-static-v1.json：explorationLoop/waitForEvent/processMapEvent；匹配 fixture schema 和 map_event_request_consumption.py。保留的独立消费者不证明自然连接的场景。 |

选定返回程序与更严格的先脚本、后 F781 边界一致。41 命令研究表已核对为完整索引0–40；上述阶段包括每条命令，含第二组缺少129 Jump。保留选择性守卫、别名效果和低字节算术，不假定通用行动者完成或坐标规范化。原英文设计切片的局部检出没有 upstream/载荷对；#928 已接受直接源码和对比较结果为保留证据，并非该切片新重跑。公共 fixture 检查不独立重证每条仅源码行动/正文声明。

既有复现路线为 `uv run sf2 h2 map-events`、`uv run sf2 h2 map-script-engine`、`uv run sf2 h2 map-event-interaction-state` 和 `uv run sf2 h2 map-event-request-consumption`，研究所有者保留比较配方。不为此正文再生成载荷。原英文切片的直接验收审阅完整自有正文/diff、数量/顺序、相对链接/片段/表/代码围栏、英文/私有边界与 `git diff --check`，然后检查已配置干净已提交 planner 及实际最终 head CI。不单因正文选择局部一揽子 Python/.NET/Godot/H3/H4/翻译套件。

保留 [#928 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/928#issuecomment-6176708249)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/931#issuecomment-6176742983)：精确 Git 对象、CI38624813497、未改变 18,285,979 字节对/0.080s、检查失败及已修正 D159/raft63 假设，并链接 #924/#920 前篇修正。保留 [#930 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/930#issuecomment-6176889129)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/933#issuecomment-6176904018)、CI38626011000 及链接的 #926/#923 记录。这些收据不替换任何已完成结果，无需复制历史台账。原英文切片未消费未合并的 Battle23 初始化内容。

**未知：**自然抵达/迷宫/区域（事件）/退出可达性；实际标志、行动者/查找、别名、HP/职业/出战队伍、布局、模式/行动、请求和当前/目的地状态；输入/文本/摄像机/行动完成、可见/可听时序、可用控制及存档/读档持久性。未评估目的地 Floor World/Creed init 或 Battle23 初始化/结果。[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)及 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)保留这些限制，不创建自动运行时队列。这既未完成战役，也未完成设计 epic，不改变重制验收边界。
