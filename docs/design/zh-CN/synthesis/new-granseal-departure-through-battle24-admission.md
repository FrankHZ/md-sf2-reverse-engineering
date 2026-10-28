# New Granseal 离开至 Battle24 准入

> 本文是[规范英文原文：New Granseal Departure through Battle24 Admission](../../synthesis/new-granseal-departure-through-battle24-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

离开 New Granseal 后又是一段大地图旅行。选定南出口进入 Map73；其东边界滚动到 Map72，保留当前探索行动者。后续区域（事件）回调可请求 Battle24，即前往 North Parmecia 的接近战。**推断：**编队布置了这段接近；它不建立强制旅程或敌人已执行战术。

**已确认（静态）：**旅行、请求产生、有序战斗选择与重新遭遇初始化是独立步骤。Janet 招募与持有 Cannon/Dynamite 均不是选定路线或 Battle24 准入前提。**未知：**自然旅行、传入状态、服务完成、可见遭遇、可用控制与存档/读档持久性。指南止于重新 start 分发器返回之后、`ActivateEnemies` 之前。

## 证据与阅读边界

已接受 Layer A 所有者为 [New Granseal 离开至 Battle24 准入](../../../research/new-granseal-departure-through-battle24-admission.md)，经 [#977 / PR979](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/979)修正并接受。它固定 USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` 与 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6)。以下源码路径相对 `disasm/`；Fnnn 表示标志 nnn。路线图拥有 [Standard 范围协议（英文）](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。解释不引入原版新发现或重制行为。

[Janet 指南](new-granseal-janet-recruitment-and-departure.md)提供此前城镇相遇，不是准入要求。已接受[请求状态](../../../research/map-event-request-state.md)、[请求消费](../../../research/map-event-request-consumption.md)与 [Battle23 名册](../../../research/battle23-initialization-and-roster.md)所有者提供共享机制。[遭遇定义](../contracts/battle-encounter-definition.md)与[过场路由](../contracts/battle-cutscene-routing.md)合同保留结构边界；#977 提供选定路线/名册的深入审查。交战、Battle24 结果/Egress、North Cave 准备/爆破、Rohde 加入及 Battle25 属于独立成果。

## 离开城镇并保留大地图状态

**已确认（静态）：**`data/maps/entries/mapNN/6-warp-events.asm` 中选定传送行请求：

| 步骤 | 匹配 | 目的地与朝向输入 |
| --- | --- | --- |
| Map6 南出口，row1 | 任意 X，Y31 | 重新 Map73 `(29,53)` Down；无滚动 |
| Map73 东边界，row1 | X63，任意 Y | Right 滚动进入 Map72，X1、保留 Y；行中为 Right |

Map6 row2 `(31,14)` 是目的地相同的独立行，不证明地形可通行。Map73 `(29,52)` 返回城镇 `(20,30)` Up。其南边界 row2 `(any,63)` 滚动到 **Map68**：源码符号为 `MAP_OVERWORLD_SOUTHWEST_PARMECIA=68`。在 `(63,63)`，东 row1 首先胜出。这里保留已接受符号到数值修正。选定行均不测试 Janet、joined/active 成员资格、物品、F805/F806 或 F523。

真实传送仍要求角色控制器掩码 `$1000` 布局类型、匹配扫描，以及单个可变待处理槽中请求保留。`WarpIfSetAtPoint` 将内部坐标除384，写 event1、parameters1–4、parameter5 的朝向/填充及 WARP_SFX。负坐标字节是通配。本指南坐标不证明步行或布局可达。分发在处理前清除类型，不清除每个参数。

城镇无滚动请求中，`ProcessMapEventType1_Warp` 提交/清除 WARP_SFX，让 physical0 Idle，退出探索并采样瓦片位置。对非负请求坐标只替换低字节，从 parameter5 取朝向，经 MainLoop 的 SwitchMap/CheckBattle 返回。普通采样高字节是前提，不是表格规范化。Map73 无战斗地图行或切换替代，因此完整请求可到普通探索。两个传送分支都不清除 PLAYER_TYPE。

显式重新 Map73 入场清除待处理，以字运算将 STEP_COUNTER 减20000，负结果钳位到0，治疗存活/不死己方角色，设置地图与 CURRENT_BATTLE255，加载瓦片、重建实体，清除临时 F256–383，设置 F80 后 load/init。持久剧情/队伍标志保留。其 default 配置没有固定居民。启用随从仍填充：F65 在普通随从前选择 Caravan 前缀；否则只用普通表。Peter/F67、Luke/F75、Fairy/F69 与替代 F74/F68/F70 FOLLOWER_B 声明可影响顺序与映射。后续逻辑映射写入不移除旧物理行动者。无行声明 Janet；joined18/active50/F806 不重建其城镇跟随链。仅 F805 不重建 Astral。军团成员资格与地图随从仍有区别。

Map73 init 保留传入历史：F700 未设运行 `cs_503A6` 后设700；700 已设时，F508 未设则返回，F508 已设/F705 未设运行 `cs_50806` 后设705。700/508/705 已设则返回。早期场景仍归其所有者。Map73 default 区域（事件）回调自身立即返回。

向东滚动走另一条路径。它设置 CURRENT_MAP72，运行 `ProcessMapTransition`，加载选定图块/布局/瓦片集并执行大地图滚动。它替换 physical0 当前/目的地 X，`$FF` 时保留 Y，并采样当前朝向：行中 Right 字节**不应用**。X1 假定返回高字节为零，因为乘384 前只替换低字节；Y 寄存器则显式清除。该路径返回探索内部，不调用 MainLoop/CheckBattle、不重建居民/随从、不治疗、不清临时标志，也不运行 Map72 init。

木筏/模式状态也保留。重新人口的 raft-player mode2 改变主精灵、让随从站立，在 `(64,64)` 声明独立木筏。否则 F64 与匹配 RAFT_MAP 允许其存储掩码位置，位于 physical31、lookup offset63，朝 Left；FOLLOWER_B lookup31 独立。滚动/当前地图的 `DeclareRaftEntity` 在 mode2 返回；mode1 跳过普通随从重定位，其他模式在 F64/地图守卫前重定位当前随从列表。不匹配隐藏独立木筏。这些调用不证明登乘、登陆或跟随完成。

两张地图都用 default 配置行与空居民列表。后续显式重新 Map72 入场重建并重置状态；CURRENT255 无战斗重载保留行动者/临时标志，但仍 load 并运行 init。Map72 init 在 F716 未设或 Luke10 已加入时返回；否则调用已接受 Luke 场景。向东滚动跳过 init。重新、当前与挂起历史不能互换。

## 区域（事件）可产生请求

**已确认（静态）：**Map72 区域（事件）分发器选一个首匹配。特定回调拒绝时不落入 default。

| 有序区域（事件）行 | 谓词 | 选定边界 |
| --- | --- | --- |
| 0–2 | X32,33,34；任意 Y | helper26 |
| 3–8 | X25–30，Y37 | 洞穴回调 |
| default row9 | 无特定匹配后的 `$FD` 标记 | F523 未设选 helper8；已设选 helper24 |

洞穴行即使立即返回，仍是独立边界。其 F750/F805/F256 与物品分支不成为 Battle24 前提。本指南不解释洞穴准备或场景。

接续篇 [Battle24 结果至 North Cliff 准备](battle24-outcomes-through-north-cliff-preparation.md)解释实际结束计数、胜利与 Egress 返回、后续缺物警告，直至局部重试/控制边界。实际移除、爆破/Rohde、洞穴内部及 Battle25 仍是独立范围。

步行掩码 `$1400` 路径在后续原始阻挡检查**之前**写 event6 和 parameters1/3 的两个坐标字。被阻挡尾部可留下区域（事件），却没有获准步伐。获准移动设置目的地与行程，仅在 STEP_COUNTER 有符号为正时递减。Mode1 改选 `eas_Raft`/opcode7，mode2 改选 `eas_Caravan`/opcode8；都不使用此步行产生器。这些源码控制器名字与数值 Caravan1/raft2 模式常量不同。坐标不意味着每帧调用 default。

区域（事件）处理器在 physical0 安装 `eas_Init`，将坐标字传给选定配置/表，再关闭立绘/文本、等待 VInt 与 physical0 位置=目的地。它不汇合所有随从，也不归零已有行程。WaitForEvent 仅待处理为零时恢复 camera0 和模式选定控制器。工作布局、模式、请求保留与服务完成仍是前提。

Default 内 F523 只选一个辅助程序。F524 是 `CheckRandomBattle` 中选定 Battle24 完成标志，不是 default 选择器：

| Battle24 辅助程序状态 | 结果 |
| --- | --- |
| F524 未设，任意计数器 | 立即请求；无 RNG |
| F524 已设，非零计数器，包括负值 | 拒绝；保留计数器；无 RNG |
| F524 已设，计数器零；range8 结果0 | 请求；无第二次抽取 |
| F524 已设，计数器零；range8 结果非零 | 拒绝；range4 抽取+2 设置计数器2–5 |

RNG 以乘13/加7 更新16 位种子，再缩放到范围；这些是算术边界，不是观察出的独立概率。Battle8 与26 用同一辅助程序及各自标志。

请求分支设置 F424，在 MAP_EVENT_TYPE 写 `$000100FF`，设置计数器30000，再等待滚动、提交 SFX_BOOST 并请求闪光。Flash 操作数30 经移位/DBF 成为四次 LaunchFading 调用，再 DuplicatePalettes；淡入淡出等待依赖 VInt。这里不写 F524，也不直接调用 CheckBattle。服务中断可留下更早写入。

该 longword 写 type word1、parameter1=0、parameter2=CURRENT255。Parameters3–5 与 WARP_SFX 保留。紧接区域（事件）产生器后，parameter3/4 含 Y 高/低字节，parameter5 是旧状态。因此普通小 Y 可产生请求 X0、Y=区域（事件）Y；不是全255 坐标/朝向元组。其他写入方可留下别的残留。无滚动消费者采样主行动者，应用保留字节并返回 MainLoop。此部分请求没有不可变战斗身份，也不保证重载完成。

## 有序准入与重新入场

**已确认（静态）：**`CheckBattle` 解析 CURRENT255，有序扫描地图/解锁/触发行。Map72 有三个候选：

| 战斗 | 裁切原点与大小 | 触发 | 解锁/完成 |
| --- | --- | --- | --- |
| 8 | `(1,32)`，32x30 | `(255,255)` | F408 / F508 |
| 24 | `(1,32)`，32x30 | `(255,255)` | F424 / F524 |
| 26 | `(28,0)`，36x32 | `(255,255)` | F426 / F526 |

首个解锁行胜出；通配触发不测试矩形包含。F408 可拦截 helper24 请求。408 未设/424 已设时，即使426 已设，24 也胜出。两者均未设时，426 可选26；无解锁则普通探索。选定完成标志在**选择之后**清除该行解锁，该次仍返回该战斗，不继续扫描。传入424 可独立于 F523 或叙事路线准入24。地图改变、待处理被覆盖或424 清除可断开产生器到准入的连接。

Battle24 裁切为 Map72 `(1,32)`、32x30。以下编队坐标全是战斗局部：`(x,y)` 对应 Map72 `(x+1,y+32)`。裁切与通配触发自身不放置主行动者。MainLoop 将选定 map72/battle24 传给 BattleLoop。

BattleLoop 先清除 PLAYER_TYPE，设置399。F88 未设时清除 seconds、保存传入地图/战斗、指定当前值、设置 base VInts 并调用 before。恢复/重新指定地图/战斗后，清除区域标志90–105、治疗己方、初始化己方再敌人、清 AI memory、为 combatant0 加载战斗并调用 start。其间没有探索人口/init，后续失败不回滚已完成前缀。

Before/start 的槽24 条目，在各自48 字表 byte offset48，均选 `ms_Empty`，即 `$FFFF` 终止符，位于 `$47A4E..$47A50`。Before 仅 intro F474 未设时分发，不设置它。Start 在分发**之前**设置474；474 已设跳过两个钩子，但重新治疗、布置与加载仍运行。解释器清除跳过锁存、消费终止符，对话窗口尚在时有条件等待滚动，再清 VIEW_SCROLLING_SPEED。没有 Battle24 局部开场对话、join 或脚本移动。

F88 已设选择传入挂起状态：恢复保存 seconds、清88、清 AI memory、view target0、LoadBattle，再跳到 individual turns。它跳过传入地图/战斗赋值、before、区域重置、治疗、布置与 start；后续重新 @Start 分支不可达。下述重新编队与本指南 start-return 终点不描述任意挂起状态。

## 初始遭遇

**已确认（静态）：**spriteset24 在 `$1B4D2C..$1B4EA6`，378 字节：四个头字节、十二条己方与十六条敌人记录（每条12 字节）、三个区域（每个12 字节）、一个点（2 字节）。各子段从 byte offsets4/148/340/376 开始。实际子段消费者依据 CURRENT_BATTLE*4 与此前计数/步长选择。源码行号不是物理行动者编号。

### 出战布置取决于成员资格与健康

全部三十名己方先设 X=-1。UpdateForce 按己方 ID 升序重建已加入且出战成员。前十二个**已处理成员**限定布置，不是保证十二名存活英雄。零 HP 成员消耗循环槽，却不推进布置指针；下一存活成员使用未消费坐标。超额成员 LeaveBattleParty，清 active 并保留 X=-1，不立即重建缓存列表。首次成员读取前没有初始空列表守卫。

此前治疗已独立于成员资格访问全部30 名。存活己方与 Peter7/Lemon28 获 HP/MP 最大值，状态掩码为既有 STUN/POISON/CURSE，再重建属性/装备；其他死亡己方跳过。这不是全员复活。Janet 可后备、死亡或排序不同；加入不保证出战、健康或槽位。探索随从不因此入伍。

布置索引0–5 为 `(4,18),(3,17),(3,18),(2,18),(3,19),(2,17)`；6–11 为 `(2,19),(2,16),(3,16),(1,16),(1,17),(1,18)`。只复制 X/Y。行中的 HEALER1、NOTHING、NONE 指令与15/15 区域不替换己方身份、物品栏或 AI。Layer A [完整槽位/消费者说明](../../../research/new-granseal-departure-through-battle24-admission.md#complete-ally-slots-and-enemy-configuration)保留精确字段与例外状态。

### 十六名敌人与编写指令

北组以 Dark Bishop128 `(25,7)` 为中心，也是 point0。Dark Sniper129 与 Skeleton130 跟随128。另一组以 Skeleton134 为中心，Bishop135 与 Skeleton136 跟随；更西边 Sniper141 跟随 Skeleton140。其余 demons、snipers、skeletons 携带下列指令。这些是存储指令与区域，不是观察到的移动或战术计划。FOLLOW_TARGET0 使用 combatant operand0，不是固定地图目的地。

| 战斗员/行 | 类型（定义 ID） | 局部 X/Y | AI | 主指令 | 主/次区域 | 编写额外物品 |
| --- | --- | --- | --- | --- | --- | --- |
|128 /0|Dark Bishop81|`(25,7)`|LEADER|MOVE_TO point0|1/15|NOTHING `$007F`|
|129 /1|Dark Sniper62|`(25,5)`|ATTACKER3|follow128|1/15|NOTHING `$007F`|
|130 /2|Skeleton17|`(24,8)`|ATTACKER3|follow128|1/15|Medical Herb `$2000`|
|131 /3|Lesser Demon45|`(28,10)`|ATTACKER1|NONE|1/15|NOTHING `$007F`|
|132 /4|Dark Sniper62|`(18,13)`|ATTACKER1|NONE|1/0|NOTHING `$007F`|
|133 /5|Skeleton17|`(21,14)`|ATTACKER1|NONE|1/0|Medical Herb `$6000`|
|134 /6|Skeleton17|`(17,18)`|ATTACKER1|NONE|15/15|Medical Herb `$2000`|
|135 /7|Dark Bishop81|`(18,19)`|HEALER3|follow134|15/15|Blizzard `$6011`|
|136 /8|Skeleton17|`(18,20)`|HEALER3|follow134|15/15|Medical Herb `$2000`|
|137 /9|Lesser Demon45|`(22,25)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|138 /10|Lesser Demon45|`(21,26)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|139 /11|Skeleton17|`(11,21)`|ATTACKER1|NONE|15/15|Medical Herb `$2000`|
|140 /12|Skeleton17|`(9,25)`|ATTACKER1|NONE|15/15|Medical Herb `$6000`|
|141 /13|Dark Sniper62|`(8,24)`|ATTACKER3|follow140|15/15|NOTHING `$007F`|
|142 /14|Lesser Demon45|`(14,3)`|ATTACKER1|NONE|1/2|NOTHING `$007F`|
|143 /15|Lesser Demon45|`(8,8)`|ATTACKER2|FOLLOW_TARGET0|1/2|NOTHING `$007F`|

所有次指令为 NONE、filler0、spawn STARTING0。源码 FOLLOW_ENEMY0/6/12 成为实际战斗员128/134/140。LEADER14、ATTACKER1/2/3=6/7/9、HEALER3=2 是 AI 输入。敌人槽144–159 则设 activation0、HP0、X=-1；其缺席路径的 trigger 读取不是另一有效源码行。Battle32 的 Jaro 例外不适用。

三个 type4 区域保留有序局部顶点：

| 区域 | 顶点 |
| --- | --- |
|0|`(9,18),(13,29),(31,29),(31,18)`|
|1|`(19,0),(10,18),(31,18),(31,0)`|
|2|`(0,0),(0,10),(10,17),(19,0)`|

其第二字节与尾字节为0；不赋予这些字节几何含义。共享消费者区分多边形测试、region15 哨兵值、point0 `(25,7)` 与战斗员跟随指令。区域测试把 type4 分为三角形1/2/4 和3/2/4；MOVE_TO 的 bit6/低半字节选择点。此终点尚未执行这些指令或触发。

### 复制定义、物品与重复遭遇

F524 未设时，每名敌人在物品/激活、难度与最终属性重建之前，复制选定56 字节定义、HP/MP 最大值、行位置、movetype/AI、索引、指令与区域。重新定义提供：

| 类型 | HP/MP | 难度前基础 ATT/DEF/AGI/MOV | 已装备武器 | 四个法术槽 |
| --- | --- | --- | --- | --- |
|Dark Bishop|39/38|28/38/27/5|Flail|AURA, BLAST LV3, NOTHING, NOTHING|
|Dark Sniper|38/0|11/37/35/5|Hyper Cannon|四个 NOTHING|
|Skeleton|30/0|26/40/30/6|Broad Sword|四个 NOTHING|
|Lesser Demon|40/21|49/37/32/6|无|BLAZE LV3, BOOST LV2, NOTHING, NOTHING|

定义起始 status NONE、其他物品槽为空；自身 level/resistance/prowess/spell-power 字段保留。这是配置，不是法术执行，也不声称零 MP 敌人施法。非 NOTHING 行物品通过 AddItem 的 `$807F` 掩码进入首个空槽。Herbs 变为 `$0000`，Blizzard 变为 `$0011`：编写的 `$2000` USABLE_BY_AI 与 `$4000` UNUSED_ITEM_DROP 位被剥离。重新复制武器留在 slot0；添加项使用 slot1，不装备。编写标志不证明存储 AI 使用或掉落行为。

STARTING0 只保留定义 `$F000`：重新 Bishops 得 `$3000`，其他类型 `$2000`；定义 BIT4/BIT5 被丢弃，普通激活前 active/neutral 位未设。所有难度应用 floor(baseATT*5/4)，SUPER2 再应用一次：Bishop35/43、Sniper13/16、Skeleton32/40、Demon61/76，分别普通/SUPER。这些是初始化**基础 ATT**；装备/当前属性效果随后处理。不是最终攻击或伤害。

Battle24 属于 `list_RandomBattles`。复制前，升级辅助程序检查列表成员资格与完成 F524。524 未设保留原类型，不抽升级 RNG。524 已设允许 Bowie 有效等级/职业与 RNG 选择升级，即使直接传入424 准入。最后未转职职业之后的职业加20；减24 再除10 的商决定后续工作。非正或零商返回原类型。更高值使用 movetype 范围/排除及重试/default 分支。飞行表空排除列表进入减1 扫描，无零守卫。不保证成功、统一升级或单一重复名册/定义/装备。坐标、AI 与物品输入仍属 spriteset；复制定义和初始化结果可不同。见 Layer A [重复状态审查](../../../research/new-granseal-departure-through-battle24-admission.md#fresh-definitions-and-repeat-state-distinction)。

### 物理行动者与最终边界

LoadBattle 清除/重建地图实体。它扫描 allies0–29，准入非负位置，再按战斗员顺序扫描普通敌人，跳过 neutral/负位置。逻辑到物理查找将实际获准行动者映射为连续普通物理槽。Combatant128 不是 physical128。重新敌人 mapsprites145/126/81/109 用普通路径；Battle24 无 neutral 条目。局部坐标以 `$3F` 掩码、乘384，朝 Down，使用 `eas_Standing`。探索随从与 Janet 临时行动者被替换。

LoadBattle 以 combatant0 映射为目标，初始化精灵，加载地图、地图精灵与地形，恢复 base VInts，请求音乐/淡入并返回；不修复缺失 combatant0。随后空 start 包装器返回。**止于 `BattleLoop:@Start` 调用 `ActivateEnemies` 之前。**已清区域标志、存储指令与 STARTING0 不证明激活、行动顺序生成、交战、表现完成或可玩控制。

## 变化状态与中断前缀

| 变化 | 条件后果 |
| --- | --- |
|无 Janet/物品/805/806|选定路线与后续准入仍取决于实际谓词，不取决于这些历史。|
|更早 Map73 历史标志|700/508/705 可在旅行前选择早期 init 场景。|
|战斗解锁已设时向东滚动|滚动仍不选择战斗；后续 MainLoop 扫描可以。|
|特定26/洞穴匹配拒绝|即使523 已设，也不落入 default24。|
|523 未设；另行传入424|Default 选8；独立有序准入仍可选24。|
|524 已设，计数器负/非零|Helper24 拒绝；步行只递减正计数器。|
|424 请求，408 保持|下一 Map72 扫描 Battle8 胜出；424 可保留。|
|424/524 已设，408 未设|该次选24 并清424；升级逻辑可不同。|
|无解锁，CURRENT255 重载|行动者/临时标志保留，但 Map72 init 可经716/10 选 Luke。|
|474 已设，88 未设|跳过钩子；重新治疗/布置/加载仍运行。|
|88 已设|传入挂起状态；没有共同重新名册或 start-return 保证。|
|Janet 死亡/后备、出战列表改变、>12 出战|成员顺序、HP 与已处理成员上限支配布置。|
|闪光/包装器停滞前请求/标志已写|更早 pending/424 可存在，却没有 MainLoop 准入。|
|解释器尾部停滞前474 已设|Intro 标志设置不证明控制完成或持久性。|

**未知：**自然路线/布局与请求产生；传入标志、模式/木筏/随从状态、队伍、HP、职业、等级、RNG、映射/容量有效性；请求保留与滚动/随从/闪光/淡入淡出/DMA/音乐/输入完成；实际升级重复、可见遭遇、可用控制与存档/读档持久性。这些限制明确保留，不授权运行时工作。

## 证据审查与保留结果

本综合对照选定既有公开夹具、schema 与实际消费者，审查完整修正 #977 Layer A。[Map-events zone/routing 所有者](../../../../tests/fixtures/h2/map-events-static-v1.json)、[map-init](../../../../tests/fixtures/h2/map-init-static-v1.json) 与[随机战斗状态](../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)保留区域（事件）选择、回调分支/顺序、init 历史与辅助程序写入/服务。[Spriteset fixture](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)以 `rowContentTracked=false` 保留地址/计数；[routing fixture](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)以 `routeTargetsParsed=false` 保留路由占用。两个聚合均不证明私有详细名册行或精确 slot24 目标。这些选定事实及实际布置/属性/升级/加载消费者来自已接受 #977。结构合同更广未知项不撤销该限定发现，也不支持把指南扩为全部战斗。

所属命令为 `uv run sf2 h2 map-events`、`uv run sf2 h2 map-init`、`uv run sf2 h2 map-event-random-battle-state`、`uv run sf2 h2 battle-spriteset-data`、`uv run sf2 h2 battle-routing-data`，以及 Layer A 保留数据对/消费者配方。原综合切片工作树缺少源码/数据对。公开 schema 与消费者检查不是新的固定源码或数据对比较；不采集、不重新生成。选定范围未发现矛盾。实质矛盾返回研究所有者，不在这里修复。

[#977 修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977#issuecomment-6180317175)与 [PR979 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/979#issuecomment-6180326745)保留最终1a58b934/tree583dfb93/base27bedb07、CI38650800988（scope12s、research29s）、原始1b4b1409/CI38650285331、中间32f9c13d/CI38650658600（scope21s/research40s）。两个不同0.080s 不变数据对回执都保留。Main-gate 的 P2 通过固定枚举关联 `MAP_OVERWORLD_SOUTHWEST_PARMECIA`，将南滚动数值含义修正为 Map68；保留此前失败检查与修正直接检查。[#977 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977#issuecomment-6180256540)保留掩码相对指针预期、34 语句对35 锚点（含声音操作数）、区域行计数、CRLF 字节失败与 PowerShell 编辑失败，均经窄直接检查修正。最初不存在的 `battlecoords.asm` 查找修正为实际 `data/battles/global/battlemapcoords.asm`；截断读取已缩窄。没有修改夹具黄金值或已完成套件来抹去结果。

[#978 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/978#issuecomment-6180262572)与 [PR980 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/980#issuecomment-6180278753)保留 dce4622f/treebecd9b9f/base2c8d441e、CI38650369688（scope18s）、514 链接、独立审查，以及修正的检查器类别键、schema 路径与程序局部序号比较。Janet 指南的[保留审查](new-granseal-janet-recruitment-and-departure.md#证据审查与保留结果)保留 #973 的45records/246bytes、作者0.093s/独立0.096s 数据对回执、CI38648632852 与别名/私有 regex 修正、#974 原始/最终 CI 与 KeyError 修正、#969/#970/#965 结果、Battle19/32 字节步长、#954 回调与 #962 Fairy 修正，以及链接失败和未知项。这些已完成结果不重置、不重分类，也不为替换回执而重跑。

原 Issue 交接记录完整正文/差异/导航/链接/锚点、UTF-8、私有/精确所有权与 `git diff --check`、同进程配置后的干净已提交 planner 及实际精确 head CI。文档工作使用直接检查，不要求全套 Python/.NET/Godot/H3/H4/翻译套件。独立验收与集成仍归 main-gate。
