# Battle24 结果至 North Cliff 准备

> 本文是[规范英文原文：Battle24 Outcomes through North Cliff Preparation](../../../../synthesis/chapters/northern-travel-mitula/battle24-outcomes-through-north-cliff-preparation.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

Battle24 根据场上仍被计数的战斗员结束。打败首个 Dark Bishop 不会自动获胜。胜利把队伍送回探索，位置由当前队伍状态推导；败北与逃脱使用不同目的地选择器。North Cliff 洞穴警告是后续独立获准、使用当前物品栏与当前行动者的相遇，不是自动胜利场景。

**已确认（静态）：**两个缺物分支都点名 Dynamite，包括 Cannon 缺失分支。它们布置同一斥责，再设置临时相遇锁存。不消费任何一种物品，也不自动回城、不提示选择或招募。**推断：**通常阵容彼此不同的情况下，移动与手势布置顾问在洞穴准备前斥责队伍。**未知：**自然旅行与结果、传入状态、服务完成、可见安定、可用控制与存档/读档持久性。指南止于缺物场景局部重试/控制边界。实际移除、爆破、Rohde、洞穴内部与 Battle25 有独立范围。

[North Cliff 打开与 Rohde 招募](north-cliff-opening-and-rohde-recruitment.md)接续双物品路径，说明实际移除、完整爆破/加入场景、局部返回与重新地图后果。它仅指出下一入口传送，保留洞穴内部/Battle25 边界。

## 证据与阅读边界

已接受 Layer A 所有者为 [Battle24 结果至 North Cliff 准备](../../../../../research/chapters/northern-travel-mitula/battle24-outcomes-through-north-cliff-preparation.md)，经 [#981 / PR984](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/984)接受。它固定 USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` 与 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6)。以下源码路径相对 `disasm/`；Fnnn 表示标志 nnn。路线图拥有 [Standard 范围协议（英文）](../../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。解释不增加原版新发现或重制行为。

[准入指南](new-granseal-departure-through-battle24-admission.md)解释此前请求、有序选择与重新编队。[Battle23 结果](../southern-travel-creed/battle23-outcomes-and-burrow-return.md)及已接受[战斗循环](../../../../../research/battle/battle-loop.md)、[过场包装器](../../../../../research/battle/battle-cutscenes.md)、[物品栏搜索](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md)、[请求状态](../../../../../research/world/map-event-request-state.md)、[请求消费](../../../../../research/world/map-event-request-consumption.md)所有者提供共享机制。[遭遇定义](../../../contracts/battle/battle-encounter-definition.md)与[过场路由](../../../contracts/battle/battle-cutscene-routing.md)合同保留结构限制。以下深入选定事实来自 #981。

## 胜利是计数规则

**已确认（静态）：**Battle24 首领存在字节为零。Dark Bishop128 的 `LEADER` AI 是另一操作数。Bowie 存活时，其死亡可到达 defeated 包装器；F524 未设运行 `ms_Empty`，524 已设跳过分发。两条路径都到达零首领尾部，不向死亡列表追加存活敌人。Bishop135 不提供特殊钩子选择器。因此击杀128 后，其他十五名敌人仍依实际状态处理。

`battleloop_1.asm:@IsBattleEnd` 先运行 defeated 钩子，处理当前死亡列表，计数剩余战斗员。先检查零己方，再检查零敌人。双方仍有成员时，应用行动后效果、再次处理死亡并重复这些检查，不再运行 defeated 钩子。

计数器扫描全部30 个己方与32 个敌人槽。非负 X 与非零 HP 字即计数；joined/active 成员资格和敌人类型不是谓词。Bowie HP0 强制己方计数为零，因此同时损失时败北优先。Bowie 存活且己方计数为正时，敌人计数零足以获胜。仅 Bowie 存活不修复零布置己方。传入地图外或非成员战斗员可改变计数。

死亡列表处理不是重新 HP 扫描。它使用当前长度与条目，呈现每次死亡，再做记账，设 X/Y=-1、status0，重算属性，并把映射实体移到 `$7000,$7000`。它既不写 HP0，也不清列表长度。条目/映射一致及声音、旋转、VInt 服务完成仍是前提；不修复陈旧或重复条目。

重新编队与完成/挂起状态仍有区别。F524 可经已接受升级路径改变重新敌人定义。F88 挂起使用传入当前状态，不保证原始十六敌人编队。Intro474、unlock424 与 completion524 各有不同消费者。

## 三种结果、两种目的地机制

### 胜利使用当前队伍位置

**已确认（静态）：**`BattleLoop_Victory` 按顺序执行：

1. 治疗全部30 名中存活己方，以及即使死亡的 Peter7/Lemon28：HP/MP 最大值、既有状态掩码为 STUN/POISON/CURSE，再重算属性/装备效果。成员资格不是治疗谓词；其他死亡己方仍死亡。Battle44 计时器特例不适用。
2. 由当前状态生成返回参数种子。PARAM2 取 CURRENT_MAP；`UpdateForceAndGetFirstBattlePartyMemberIndex` 重建成员资格，选升序首个已加入且出战 ID，不是首个存活成员。其 X/Y 加 BATTLE_AREA_X/Y 成为字节 PARAM3/4，实际 ENTITY_INDEX_LIST 映射提供 PARAM5 朝向。PARAM1=0；种子不写 MAP_EVENT_TYPE。
3. 执行 after 包装器。524 未设分发空 after 脚本，524 已设跳过。两者到达 `EndAfterBattleCutscene` 与 `JoinForce(0)`。Bowie 成为已加入，出战不足十二人时设置 active32。满队不逐出任何人。这不是 Rohde 招募，且加入太晚，不能修复已选返回种子。
4. 清424、设524、清零 d0–d3 字，把 PARAM2–5 字节载入，返回 d4=1。Intro474 不变。

选定 Map72 战斗区域 `(1,32)` 中，局部 `(x,y)` 生成模256 的返回 `(x+1,y+32)`，带当前映射朝向。这既不是初始布置，也不是固定洞穴瓦片。首个成员死亡/地图外、区域/地图/映射变化及其间参数写入都可改变结果。出战列表为空留下陈旧首列表字节；没有空列表或朝向修复。

End/after 表 slot24 目标均为 `ms_Empty`，即字 `$FFFF`，位于 `$47A4E..$47A50`。空脚本仍有包装器。布局选择 cutscenes 的 after-join 表，其 slot24 为0，而非同名 global 表。没有 Battle24 布置覆盖。对话打开时，终止符仍可等待滚动并清 VIEW_SCROLLING_SPEED。After 脚本未完成阻止后续 join/标志；join 未完成阻止 clear/set。早期效果不回滚。

### 败北与逃脱使用 EGRESS_MAP

**已确认（静态）：**败北更新解锁，清 DIALOGUE_NAME_INDEX_1，请求悲伤音乐与文本363，关闭文本，恢复 Bowie 最大 HP，将无符号金币减半，取 Egress 坐标后返回 d4=-1。Battle04 特例不适用。获准 Egress 扣法术 MP；Angel Wing 移除所选物品栏槽。其共享文本/声音/闪光尾部更新解锁、退出行动栈帧、取得坐标并返回 d4=0。逃脱不减半金币，也不执行败北的 Bowie 恢复。进入这些处理器不建立菜单可用性或物品/法术持有。

解锁辅助程序在524 未设时保留424，524 已设时清424。败北/逃脱不设524，不运行胜利 after/join，也不改变474。已完成准入可能已清424。

`GetEgressPositionForBattle` 没有 Battle24 特例。它把 **EGRESS_MAP** 传给 `egressinit.asm`，不是先前位置/朝向。399 已设时，传入6 选 New Granseal `(11,22)` Up。传入72 没有存档点行，保留 Map72、默认 `(1,1)` Up。其他选择器用各自行或 default；警告文本不选择 New Granseal。直接399 未设调用则在木筏重置前返回初始 Map3 `(56,3)` Down；BattleLoop 在重新或恢复入场设置399。

399/F64 已设时，木筏重置使用存档点：Polca/Bedoe 选 Map72 `(43,48)`，Hassan 选 Map69 `(28,55)`，否则 Map69 `(42,15)`，包括传入6/72。F64 未设保留木筏字段。这是保存木筏定位，不是登乘。

## 探索在下一次战斗扫描前返回

**已确认（静态）：**所有结果到达 `alt_MainLoopEntry`，再到 SwitchMap 与 ExplorationLoop，不立即 CheckBattle，也不按 d4 正负分支。Map72 没有标志切换替代。胜利不调用洞穴回调，也不立即重复 Battle24。

普通显式72 返回清待处理，将字 STEP_COUNTER 减20000、负值钳位到0，治疗合格己方，淡出并设 map72 / CURRENT_BATTLE255。它加载瓦片、重建实体/地图精灵，清临时 F256–383，设置80，再地图加载、base VInts、init。持久剧情/队伍标志（包括424/474/524）保留。BattleLoop 清除了 PLAYER_TYPE，空钩子不替换。普通 mode0 与传入/直接 modes1/2 必须保持区别。

Map72 唯一配置没有固定居民。重新初始化清战斗行动者/映射，从 physical1 声明启用标志随从，主行动者在0。因此空居民不等于空阵容。Mode2 改变主精灵、让随从站立，在 `(64,64)` 声明独立木筏；否则 F64 加匹配 RAFT_MAP 可声明存储掩码木筏位置，位于 physical31、lookup63、Left。FOLLOWER_B 逻辑 lookup31 独立。

Map72 init 有六个操作：716 未设返回，Luke10 已加入返回，否则运行已接受 Luke 场景再返回。物品、750/805/524 不替代守卫，init 不调用缺物场景。有条件音乐/淡入先于 WaitForEvent。加载/init 成功自身不是可用控制证据。

CURRENT255 无战斗重载保留行动者与临时标志，仍加载/运行 init。木筏消费者在 mode2 返回；mode1 跳过普通随从重定位，其他模式在 F64/地图守卫前重定位当前列表。边界滚动保留行动者，跳过 init/临时重置。F88 挂起则是战斗入场历史；其传入当前状态必须沿结果追踪。

后续区域（事件）仍有独立产生与准入条件。消费者清待处理，在 physical0 安装 `eas_Init`，把坐标低字节与首个匹配行比较：

| 有序行 | 谓词 | 回调 |
| --- | --- | --- |
| 0–2 | X32,33,34；任意 Y | helper26 |
| 3–8 | 分别 X25–30；Y37 | 洞穴回调 |
| default9 | 无特定匹配后的 `$FD` | F523 未设：helper8；已设：helper24 |

特定26/洞穴回调拒绝绝不落入 default24。步行需要实际掩码布局 kind`$1400`；待处理可在后续阻挡检查前写入。坐标不建立获准步伐/事件。Mode1 用 `eas_Raft`/opcode7，mode2 用 `eas_Caravan`/opcode8，均不用此步行产生器；传入事件仍可到区域（事件）。

524 未设时 helper24 立即请求。524 已设时，非零计数器拒绝；counter0 抽 range8，零则请求，非零抽 range4+2，得 counter2–5。请求在闪光服务前设置424、写部分 longword type1/CURRENT255 与 counter30000，留下残余位置/朝向字段。后续无滚动返回到有序 Map72 准入8/24/26：首个解锁行胜出，不测试裁切包含。完成在选择后清所选解锁，不继续扫描。F408 可拦截24 请求；F426 可留到后续。战后绕过扫描不取消这些后续路径。

## 洞穴检查队伍当前携带物品

**已确认（静态）：**`Map72_ZoneEvent3` 依次在750 已设、805 未设、256 已设时返回。准入时清256，先搜索 Cannon，再 Dynamite，以完整结果字比较 `$FFFF`。524/474/806、实际 Astral、模式、HP/职业或出战容量守卫均不替代这些测试。Cannon 缺失时完全不搜索 Dynamite。

已接受物品栏谓词按升序重建已加入 IDs，包括后备与死亡成员。每名持有者先计数四槽非 NOTHING 项，再从 count-1 降至0 搜索，以 `$7F` 掩码项比较未掩码请求字。首个匹配成员中已检查最高匹配槽胜出。Count<4 时空隙可隐藏 slot3；未加入持有者排除。返回成员字节不规范化 d0 其余部分；穷尽写 d0.w/d1.w=-1。空已加入列表缺安全初始循环守卫。赠礼历史761/804 与随从身份不建立当前物品栏。

| 当前查找结果 | 获准分支 |
| --- | --- |
| Cannon 缺失，无论 Dynamite 有无 | 把 item-name INDEX_1 设为 Dynamite `$74`；运行 `cs_5023E`；再设256。 |
| Cannon 找到，Dynamite 缺失 | 同一名字、场景及后续256；不消费 Cannon。 |
| 两者找到 | Cannon 移除调用、Dynamite 移除调用、`cs_4FFDA`、设750、再设256：独立下一边界。 |

缺物路径只搜索物品栏，不消费物品或招募。双物品调用顺序不证明移除成功：调用方不检查移除返回。本指南止于该被调用方审计与爆破/Rohde 场景之前。750 在该场景返回后设置；缺物路径从不设750。早期守卫保留状态，不选择 default 战斗。

## 谁接受斥责

**已确认（静态）：**场景使用逻辑 A=FOLLOWER_A30、B=FOLLOWER_B31、P=Peter7、Bowie0。这些是查找，不是物理槽或保证身份。它不创建行动者，不修复缺失/重复映射。F805 不要求阵容标志65/67/70。

重新 mode0 Map72 仅启用65/67/70 时，A 映射 physical1 Caravan、P2 Peter、B3 Astral，main0。不启用65，A 保留 lookup0、别名主行动者，P1/B2 仍在。没有相关声明时，三者可都别名0。后期74/77/83 A 声明可把 Caravan 替换为 King/Old Man/King；74/68/70 中最后启用 B 行选 Astral/Oddler/Astral。声明向探索列表追加物理行动者，后续查找覆盖不移除旧行动者。隐藏`$FF` 映射不是共享查找/缩放例程的安全缺失哨兵值。

重新精灵选择可为战斗外 HP0 己方产生 Blue Flame；否则 Bowie 的 HERO/SDMN 与 Peter 的 PHNX/PHNK 职业选各自表精灵。其他传入职业走共享算法。F67 独立于成员资格/HP 声明 Peter，尽管普通结果治疗恢复不死 Peter。CURRENT255 可保留变化精灵/映射，而非这些示例。因此以下编排始终作用于实际映射。

## 缺物场景的完整顺序

**已确认（静态）：**`cs_5023E` 包含40 条地图脚本记录加两条内联行动记录，总编码212 字节。源码序列与生成移动程序是不同表示。

### 接近、手势与首次警告

脚本设置文本游标3357，依次在 P、B、A 安装并等待 Init，把摄像机移到 `(21,33)`。Init 设置加速度因子0、速度32、阻挡/碰撞与自动朝向，朝 Up、禁用浸没，请求 size24/精灵刷新并跳 Idle。它不重置所有目的地或行程字段，也无 HP 守卫。

随后请求目的地 A`(27,39)`、B`(28,37)`、P`(25,37)`、Bowie`(26,37)`。前三者用 bit15 跳过位置等待；Bowie 命令以 VInt 重试等待实际位置等于目的地。这些目的地乘384，安装幅度32 的行程/速度。之后 waitIdle A/B/P 轮询脚本 Idle，不是全部抵达。别名可覆盖旧目的地，因此即使不同角色 Idle 检查也不证明阵容已安定集合。

Bowie、P 与 B 朝 Up。50 ticks 后 Bowie 朝 Right；30 后 B 朝 Left。Bowie 点头、B 发抖、P 朝 Right，20-tick 等待后文本3357。朝向跳过死亡 Bowie/Peter，A/B 绕过该 HP 测试。点头无守卫：counter-1、Sleep10、精灵/DMA 变换、Sleep20、刷新、Sleep10、counter0。发抖保存全局 size/counter，用 size21/counter-1，执行三次 resize 开/关循环，每半段 Sleep5，再恢复保存值。它留下 resize 未设，不恢复任意旧标志。

文本3357 问队伍为何忘记命名物品，并称呼 LEADER。两个缺物分支都提供 **Dynamite**，即使仅 Cannon 缺失。ITEM 经 GetItemName 解析 INDEX_1。LEADER 独立重建军团，命名首个出战成员，未必是映射物理 Bowie；变化或空列表独立于 B 影响替换。

Context`$001F` 为立绘/台词选 B 当前精灵：Astral209 给 Astral/bleep6，Oddler213 给 Oddler/bleep4，否则用实际表行或 default portrait/bleep6。已打开立绘可绕过打开，但仍查 speech。未跳过文本等待滚动，显示游标文本、递增游标、关闭立绘/文本并睡眠10。

### 缩小、一步与两次定时跳跃

脚本停止 Bowie 动画，依次请求 sizes22,20,18，中间10-tick 等待，再给 B 编译的一瓦片左移，等待 Idle。Stop/start 改动画 counter-1/0，不改移动启用；其字行动者守卫跳过死亡 Bowie。Size 命令无守卫：各自临时改变全局 size、设行动者 resize 标志、刷新/等待 VInt，再恢复全局 size。不存永久行动者高度18。

内联 `moveLeft1`/`endActions` 源码为4 字节。编译器生成相对 opcode4 move`(-1,0)`、opcode1 抵达等待与跳 Idle：共14 生成字节，起于地图脚本 scratch 重置处。它以`$9F` 掩码 B 的 FLAGS_A，bit5 未设期间绕过共享其他实体阻挡检查。移动在当前位置加`(-384,0)`；名义 `(28,37)`→`(27,37)` 取决于此前布置已抵达。等待生成程序 Idle 仍依赖移动/行动服务。

随后两次安装并等待 `eas_Jump`。每次禁用自动朝向，请求相对`(0,-1)`、wait5、相对`(0,1)`、wait5、相对`(0,0)`，再启用自动朝向并跳 Idle。它们也用 opcode4，但固定延迟替代编译步伐插入的抵达等待。每个偏移用当时当前位置；最后零移动不恢复保存起点。Idle 不证明渲染跳跃或精确位置恢复。

文本3358 随后斥队伍愚蠢，要求回去取物品。没有 YesNoPrompt、传送、重载或事务。其普通 W1 在 VInt 后接受持有方向/A/B/C，不接受 Start；前段 W2 使用继续/输入和确认音效。N 表示换行。

ExecuteMapScript 清文本跳过锁存。启用 debug 的 P2 Start 可设置：跳过文本既不显示也不递增游标，csWait 睡眠跳过。移动、点头/发抖内部睡眠、size 变化、行动等待与跟随仍执行。跳过不是取消或瞬间完成。缺物程序没有显式音乐/淡入淡出/闪光命令。

### 恢复与分支跟随链

Bowie 接受 Init 与 startEntity，请求 size24/刷新并恢复动画。最终 follow 请求 A→Bowie，slot2 偏移`(-24,0)`；P→A，slot1`(0,-24)`；B→A，slot3`(0,24)`。这是分支链，不是三者直接跟随 Bowie。

Follow 处理器读取随从字的**高字节**作存活守卫。因此字`$001E/$0007/$001F` 全部测试 Bowie0，不是 Peter 或映射随从。Bowie 死亡时，在此前无守卫场景变化之后跳过安装。Bowie 存活时，实际映射支配每个目标，包括别名/自跟随与覆盖。

每次获准 AddFollower 复制42 字节，在探索列表追加去重前安装脚本。三个命令分配126 字节，无容量守卫或安定等待；无关随从保留。直接重复场景即使行动者已在列表也再次分配。重新 ClearEntities 重置步行缓冲区；重置地图脚本 scratch 不回收它。这些生命周期解释变化/重复行为，不假定无限随从容量。

## 局部返回与重试

**已确认（静态）：**终止符在对话打开时有条件等待滚动，并清 VIEW_SCROLLING_SPEED。场景返回后，区域（事件）调用方才设256。它关闭立绘/文本、等待 VInt 与 physical0 位置，不是全阵容。WaitForEvent 仅待处理为零时恢复 camera0 与选定控制器。待处理覆盖、模式/映射及服务未完成可改变接续。局部返回不是通用可玩控制终点。

F256 是临时抑制，区别于洞穴已清750。走开再回来或未重新入场而获取物品，本身不清它。CURRENT255 无战斗重载保留256。显式重新入场清256–383；750 未设/805 已设时可重试当前物品栏。750 已设即使临时重置后也抑制回调。

直接调用 `cs_5023E` 绕过区域（事件）守卫，既不写256/750，也不写物品名字字；ITEM 可继承其他索引。调用方 set256 前中断留下部分行动者效果，无锁存写入。此场景与结果包装器都不回滚已完成前缀。

| 变化状态 | 须保留后果 |
| --- | --- |
| Bishop128 死亡，另一被计数敌人存活 | 无首领清理；战斗可继续。 |
| 所有敌人不计数，Bowie HP0 或无被计数己方 | 败北优先。 |
| 完成524 或挂起88 | 包装器/升级或传入状态规则替代重新假定。 |
| 首个出战成员、区域或映射不同 | 胜利返回种子变化；后续 JoinForce0 不能修复。 |
| 750 已设、805 未设或256 已设 | 有序守卫返回；无搜索、场景或 default 落入。 |
| 有空隙、死亡/后备或未加入持有者 | 实际受计数限制的已加入搜索决定物品存在。 |
| A/B/P 映射缺失/冲突或职业/HP 变化 | 实际行动者、守卫、立绘与跟随不同。 |
| Peter 死亡，Bowie 存活 | Peter 朝向可跳过，最终跟随准入仍通过。 |
| 直接场景中 Bowie 死亡 | 朝向/动画/跟随可跳过；无守卫目的地/手势/size/行动仍执行。 |
| 重复直接调用或手动清256 | 无场景自身守卫；步行缓冲复制累积。 |
| Debug 跳过或服务未完成 | 保留已达效果，不保证文本、抵达、后续标志或控制。 |
| 重新入场与当前重载/滚动 | 人口/临时重置区别于保留阵容/锁存。 |

**未知：**自然战斗/旅行/布局/事件产生；传入标志、物品栏/空隙、成员资格、HP/职业/模式/映射与缓冲容量；输入/文本/声音/调色板/DMA/行动/随从/滚动服务完成；可见阵容安定、可用控制与存档/读档持久性。它们不授权运行时工作，也不把指南扩到双物品场景。

## 证据审查与保留结果

完整已接受 #981 所有者已与准入前序及选定公开 fixture/schema/消费者表面对照审查。[Map-events fixture](../../../../../../tests/fixtures/h2/map-events-static-v1.json)及加载器保留完整 Map72_ZoneEvent3 操作/分支顺序、查找目标、六个洞穴行引用与首匹配路由。[Map-init fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json)保留 Map72 六操作716/Luke 守卫与脚本目标。[随机战斗 fixture](../../../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)保留辅助程序/请求顺序。[过场 fixture](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)保留包装器守卫/共享 join；[routing fixture](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)含 `routeTargetsParsed=false`，[spriteset fixture](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)含 `rowContentTracked=false`。聚合计数不证明 slot24 目标、私有名册行或详细缺物场景。

[地图脚本 fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)及验证器保留命令家族/处理器事实与语料摘要，不公布此私有详细程序。上述源码/生成行动区别、实际 follow 守卫、完整文本/场景与消费者关联消费已接受 #981 选定审查，不由聚合 schema 成功推断。未发现选定矛盾；实质矛盾返回研究，不在 Layer B 修复。

所属命令仍为 `uv run sf2 h2 map-events`、此前缀下的 `map-init`、`map-event-random-battle-state`、`battle-cutscenes`、`battle-routing-data`、`battle-spriteset-data`、`map-script-engine`，加 Layer A 保留数据对配方。原综合切片缺源码/数据对。选定公开 schema 与消费者检查不是新源码/数据对比较；该综合不采集、不重新生成，也不新建环境。

[#981 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/981#issuecomment-6180545109)与 [PR984 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/984#issuecomment-6180565663)保留最终 `e53e678a`、tree `81d1f6ef`、base `1a4d0092`、blob 不变的原始 `a21d8fe9`；CI run38652506094（scope15s/research33s）、作者0.081s/独立0.082s 的不变18,285,979 字节数据对比较，以及40+2 记录/212 字节。字面量终止符、zone-key、跳跃 opcode 与阻挡修正保留于这些回执。Preflight 猜测 Battle23 所有者后已改用链接的实际所有者；缩窄读取取代截断检查输出。

[#982 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/982#issuecomment-6180471079)与 [PR983 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/983#issuecomment-6180498860)保留 `2d3c9072`、tree `f6137ec1`、base `5d707f60`、CI38652007944（scope20s）、521links 与修正的大小写/源码大小 scratch 断言。准入指南的[保留审查](new-granseal-departure-through-battle24-admission.md#证据审查与保留结果)保留 #977 的 Map68 P2 修正、原始/中间/最终 CI、两个不同0.080s 数据对回执，以及链接 #978/#973/#969/#965/#954/#962 失败、修正与未知项。已完成失败与套件不重置，也不为获得替代回执而重跑。

原 #986 交接记录直接正文/差异/导航/链接/锚点、UTF-8、私有/精确所有权与 `git diff --check`、同进程配置后的干净已提交规划及实际精确 head CI。文档范围用直接检查，不要求全套 Python/.NET/Godot/H3/H4/翻译套件。独立验收与集成仍归 main-gate。
