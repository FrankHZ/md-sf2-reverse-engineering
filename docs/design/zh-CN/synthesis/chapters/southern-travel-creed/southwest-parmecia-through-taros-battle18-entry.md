# Southwest Parmecia、Taros 的警告与有条件的 Battle18 进入

South Parmecia 选定的西侧出口通过滚动进入 Southwest Parmecia，探索继续。神殿入口采用另一条路径：返回 MainLoop；在那里，已解锁的 Battle18 会先被选中，随后才可能运行神殿的探索警告。未获该战斗准入时，Rohde 的接受标志决定队伍是否听到 Taros 的警告并请求撤退。全新 Battle18 有自己的对峙场景，之后另行重建交战行动者。

**已确认：**已接受的[研究所有者](../../../../../research/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md)确立这些条件转移、完整警告与战前场景，以及全新／已保存进入边界。**推断：**对话将 Taros 呈现为取得 Caravan 的障碍。**未知：**自然旅程、实际传入状态、服务完成、表现、可用输入及持久性。Fnnn 表示标志 nnn；坐标单位为地图瓦片，表索引从零开始。等待计数、摄像机速度和移动操作数描述源码请求，不是实测时长。

前一选定区域见 [Hassan 离开与 Battle17 返回（中文）](hassan-departure-and-battle17-outcomes.md)，独立的接受／跟随写入见 [Rohde 的提议（中文）](hassan-ancestry-and-rohde-caravan-choice.md)，有条件的物品持有见 [Achilles Sword 谜题](ribble-tree-opening-and-achilles-sword.md)。这些链接不确立必需的时间顺序旅程。本指南停止于有条件的探索返回、无关战斗准入或 Battle18 激活前的开始交接。独立的结果／场景及紧接的返回边界见 [Battle18 结果与 Caravan 获取](battle18-outcomes-and-caravan-acquisition.md)。Caravan 操作、Rick 与后续旅行需要各自的所有者；此处未观察交战或可玩回合（单次行动）。

## 两种转移，不同后果

**已确认的选定记录：**Map69 row5 匹配 X0 与任意 Y，请求向 Left 滚动至 Map68、X62/Y255、面朝 Left。较早记录不匹配 X0；在 `(0,0)`，该行先于后续任意 X／Y0 记录胜出。Map68 row0 则匹配 `(55,43)`，请求 Taros Shrine Map12 的 `(12,30)`、Up、无滚动。首匹配表选择既不能确立地形可通行，也不能确立这些点之间存在连续步行路径。

传送生产方以 384-unit 瓦片扫描传入位置，允许负坐标字节作为通配符。它写入待处理 type1 请求、参数与传送声音。不检查 Sword、Battle17 完成、F725、F418 或成员资格。紧邻的步行／载具调用方仍要求各自的移动／布局条件。实际 mode1 为 Caravan，mode2 为木筏，尽管上游动作名分别为 `eas_Raft`/opcode7 与 `eas_Caravan`/opcode8。这不能证明任一模式下的可达性。

| 使用方 | 选定转移执行的操作 | 控制可继续的位置 |
| --- | --- | --- |
| Map69 → Map68 滚动 | 清除待处理状态，使 main0 空闲，将 CURRENT_MAP 改为 68，加载选定瓦片／布局并请求 Left 平面／精灵转移。该服务之后，main0 的当前／目标 X 变为 62；负 Y255 保留传入 Y。 | 在现有 ExplorationLoop 内返回。没有 MainLoop 战斗扫描、重新生成实体、Map68 初始化、治疗或临时标志清除。此处不改写 CURRENT_BATTLE。 |
| Map68 → Map12 无滚动 | 清除待处理状态，提交／清除传入的传送声音，使 main0 空闲，移除探索返回栈帧并返回 `(12,12,30,Up,d4=0)`。非负请求坐标替换采样位置。 | MainLoop 切换地图、扫描战斗，随后选择战斗或探索。 |

滚动使用方不根据请求的朝向参数赋予朝向；当前／控制器朝向仍为传入值。滚动期间的精灵位置调整与实体数据坐标相互独立。两种转移均不改变 PLAYER_TYPE 或已保存的 RAFT_MAP/X/Y 元组。

滚动后，`DeclareRaftEntity` 使用采样的玩家状态。Mode0 将列表中随从的当前／目标坐标收拢到该位置；mode1 跳过此循环；mode2 立即返回。对于适用的非木筏模式，F64 与匹配的已保存木筏地图可将独立的 physical31、lookup63 放在其保存位置。不匹配时走移除路径；F64 未设置时跳过放置／移除。这既不将已保存木筏移动到 Map68，也不创建随从列表。只有不存在另一待处理事件时，WaitForEvent 才跟随 main0 并安装当前控制器。在这次原地滚动期间，仅 F419 不会准入 Battle19；后续事件生产是另一边界。

## 战斗选择先于神殿警告

**已确认：**完整的标志切换地图表没有 source12/68/69 覆盖。Map12 唯一的战斗记录行为 18，即 `BATTLE_VERSUS_TAROS`，区域原点 `(0,0)`、大小 `26x32`、通配触发坐标 255/255。CheckBattle 要求 F418。矩形提供战斗几何；进入 Y30 和玩家是否位于区域内都不是准入检查。选中时写入区域字段。F518 已设置时清除 F418，但本次调用仍返回 18。

| Map12 扫描时传入的标志 | 结果 |
| --- | --- |
| F418 已设置；F725 任意 | 准入 Battle18，不先运行 Map12 探索初始化或其警告。F518 可在选择之后清除解锁状态，但不取消本次准入。 |
| F418 未设置；F725 未设置 | 进入探索；初始化运行警告，然后检查 F518。 |
| F418 未设置；F725 已设置 | 进入探索；初始化跳过警告，然后检查 F518。控制返回仍有条件。 |

F468、F725、Battle17 完成、Sword 持有、Rohde 跟随和成员资格都不是 Battle18 准入检查。Rohde 已接受的 Yes 路径一起写入 F725/F84/F418，但这些使用方不会修复独立传入的标志。F725 是接受，F418 是解锁，F468 是共享开场门槛，F518 是完成；它们互不替代。

## 探索行动者与完整警告

**已确认的实体生成：**按传入地图进入探索会清除待处理状态，将 STEP_COUNTER 减少 20000 并按有符号字在负值处钳位，治疗符合条件的己方角色，淡出并写入 map12/battle255。随后加载瓦片、选择唯一 Map12 配置、重建实体／查找表、清除临时标志 256–383、设置 F80、加载地图、安装基础 VInt 并运行初始化。治疗涵盖存活己方角色及不死的 Peter/Lemon；它不会复活所有死亡己方角色，也不确立成员资格。

Map12 唯一的固定配置记录将 Taros 放在 `(12,8)`、Down、特殊 mapsprite255、`eas_Init`。Logical128 映射到特殊 physical47，不消耗普通槽位。这是场景行动者，不是战斗员 128 的属性值。Main0 在请求的位置／朝向另行创建。没有固定 Peter、Rohde、FOLLOWER_B 或 Caravan 记录。

即使 F65 已设置，普通随从表仍适用。F67 提供 Peter，F84 提供 Rohde，F68 在 FOLLOWER_B31 提供 Oddler。其他传入标志 74/70 可向同一逻辑身份写入 Astral 记录；表顺序决定冲突查找写入的结果。F725 不生成这些行动者。探索跟随与已加入／出战战斗成员资格仍相互独立。Mode2 使 main0 成为木筏并隐藏随从；其他模式保留 F64／匹配的已保存木筏放置规则。初始化不获取或规范化任何载具。

位于 `0x57D04` 的初始化先检查 F725。若未设置，运行 `cs_57D22`；只有返回后才检查 F518，并在其设置时将 logical128 移出地图。该移动将当前／目标坐标改为原始 `$7000`；它既不清除查找映射、成员资格或 HP，也不报告击败。因此 F725 未设置／F518 已设置时，仍先请求警告／撤退，再隐藏 Taros。反过来，F725 已设置／F518 未设置可使他留在场上，却没有警告或战斗。初始化不写入这四个剧情／战斗标志中的任一个。

### 警告与撤退请求

**已确认的完整警告：**`cs_57D22`（地址 `0x57D22`） 有 51 条命令，从文本 3197 开始。下列阶段保留完整有序的动作／对话含义；等待动作只汇合其指定行动者的空闲指针。已经开始的动作可能重叠，或在完成前被替换。

| 阶段 | 调度、对话与交接 |
| --- | --- |
| 集合与接近 | 设置摄像机速度 `$30`；依次初始化／等待 Bowie、Peter 和 FOLLOWER_B。将他们分别放在 `(12,31)`、`(13,31)`、`(11,31)`，面朝 Up，并淡入。开始 Bowie/Peter Up5，等待 FOLLOWER_B Up5。Taros 命令他们停下（3197）；等待 10。 |
| 反应 | 开始 Bowie/Peter 跳跃，等待 FOLLOWER_B 跳跃，然后重复该顺序。等待 30，再等待 5。开始 Bowie 的 Right/Left 张望两次，等待 5；Peter 的 Up/Down 张望两次，等待 5；FOLLOWER_B 的 Right/Left 张望两次；等待 80。 |
| 发现说话者 | 等待 Peter Up1；使 Bowie/FOLLOWER_B 面朝 Up。Peter 问是谁在说话（3198）。将摄像机移至 `(7,5)`；Peter 评论对方体型巨大（3199）。等待 30；Taros 命令他们离开并威胁攻击（3200）。 |
| 决定离开 | 将摄像机移至 `(7,21)`；等待 Peter Down1。使 Bowie/FOLLOWER_B 面朝 Right，Peter 面朝 Left。Peter 对威胁作出反应，催促具名领队离开（3201），随后说他们无意交战（3202）。第一条消息以 `nextText` 继续；第二条通过 `nextSingleText` 关闭。 |
| 撤退 | 等待 20；Bowie 点头。Peter 催促赶快（3203）。开始 Bowie/Peter Down5，等待 FOLLOWER_B Down5。排入无滚动 Map68 `(55,44)`、Down 请求，然后结束。 |

没有 Yes/No 提示、Rohde 台词、物品栏检查或标志写入。传入的 Rohde 随从仍可存在；他在此程序中无台词不等于被删除。Bowie 点头是脚本姿态，不是记录的玩家同意。领队替换在 UpdateForce 后读取当前出战队伍首名成员的姓名；它既非硬编码 Bowie，也不是经过验证的空列表回退。

### 消费撤退请求

脚本传送写入待处理请求，不立即加载 Map68、退出脚本或写入 WARP_SFX。普通终止返回初始化的 F518 检查，随后进入探索的条件调色板／音乐／淡入淡出工作。初始化返回时没有所有行动者／主实体位置屏障。撤退仍待处理时，WaitForEvent 在重新安装普通控制之前返回该请求；无滚动使用方提交／清除当时传入的传送声音，并向 MainLoop 返回 `(68,55,44,Down,d4=0)`。警告与撤退之间不保证存在可玩的神殿暂停。另一次待处理写入可替换请求；这不是原子队列。

Map68 唯一的战斗记录行为 19，区域 `(5,19)`、大小 `48x32`、通配 255/255。传入 F419 会准入它；F519 可在选择后清除其解锁状态。本指南在这一无关准入处停止。否则，全新 Map68 探索重建实体、清除临时标志并运行只返回的初始化。其配置无固定实体，也无替代标志记录；随从使用当前标志，F65 选择大地图表，模式／F64／已保存木筏规则仍适用。警告不写入这些状态中的任何一个。若无后续待处理事件，可进行普通跟随／控制器安装。自然返回、服务完成与可用输入仍属未知。

## Battle18 有独立的战前场景

**已确认的进入：**BattleLoop 先写入 PLAYER_TYPE=0 和 F399，再检查 F88。全新进入在战前包装器之前清除秒数、设置 map12/battle18 并安装基础 VInt。Slot18 选择 `bbcs_18`（地址 `0x4B1D8`）；开始槽位选择 `ms_Empty`（地址 `0x47A4E`）。战前包装器检查 F468 而不设置它。开始包装器在 F468 未设置时，先设置它再执行空终止符。F468 已设置的全新进入会跳过两个脚本，但仍重建战斗员。F725 和 F518 不是开场守卫，此场景加载也不调用 Map12 探索初始化／警告。

F88 已设置的进入则恢复已保存秒数、清除 F88 和 AI 内存、清除视野目标、加载已保存战斗状态，并直接分支到单次行动循环。它绕过传入地图／战斗赋值、战前／开始、全新治疗／放置及新行动顺序生成，且不具备全新进入在 LoadBattle 前显式设置 d0=0 的步骤。已保存地图、区域、名册和回合（单次行动）状态必须已经一致；这不是修复过的 Taros 开场，也不是持久存档／恢复的证明。本指南在该分支处停止。

### 场景实体生成

`loadMapFadeIn Map12,7,22` 启动／等待淡出、加载瓦片／地图并恢复显示／VInt 服务。可见淡入是之后的显式命令。`ce_4B2DA` 以一个**三字主实体头部**开始：main0 `(12,32)`、Up。其余两行为 Taros `(12,8)`、Down、特殊 sprite255，以及 Caravan `(12,5)`、Down、sprite62，两者均使用 `eas_Init`。头部不是第三个 NPC。Taros 映射到 physical47；Caravan 占下一个普通物理槽位及 lookup129，具体取决于此前随从数量。Caravan129 既非 FOLLOWER_A、战斗员，也非运输控制器。

加载这些实体会清除旧探索行动者并重建 Map12 普通的按标志选定随从。场景表不提供 Peter、FOLLOWER_B 或 Rohde。F84 未设置／F418 已设置不会合成 Rohde，台词也不能修复缺失映射。调度出的载具不授予 F65、运输模式、物品栏或成员资格；没有场景命令检查或消耗 Achilles Sword。

### 完整对峙

**已确认：**`bbcs_18` 有 51 条命令，从文本 2520 开始。每个内联动作体包含一次移动，随后为 `endActions`；空开始目标后没有隐藏的额外开始对话。八条消息均使用 `nextSingleText`。

| 阶段 | 调度、对话与交接 |
| --- | --- |
| 集合 | 加载 Map12／摄像机 `(7,22)` 及场景实体。依次初始化／等待 Bowie、Peter、FOLLOWER_B 和 Rohde。将 Peter 放在 `(13,33)`、FOLLOWER_B 放在 `(12,33)`、Rohde 放在 `(11,33)`，均面朝 Up。停止 Caravan129 动画、请求神殿音乐、淡入、设置摄像机速度 `$40`。 |
| 接近与反应 | 开始 Bowie/Peter/Rohde Up6，等待 FOLLOWER_B Up6。Taros 命令停下（2520）；等待 5。开始 Bowie 的 Right/Left 张望两次，等待 5；Peter 的 Right/Left 张望两次，等待 5；Rohde 的 Up/Down 张望两次，等待 5；FOLLOWER_B 的 Right/Left 张望两次；等待 30。 |
| 认出 Taros | 等待 Peter Up1；他问这个大个子是谁（2521）。等待 Rohde Up1；Rohde 问 Peter 是否认识他（2522）。使 Bowie/Peter/FOLLOWER_B 面朝 Up；等待 30；摄像机跟踪 actor128。Rohde 认定 Taros 是古代神殿的守卫（2523）；Taros 重复警告并威胁攻击（2524）。 |
| 指出 Caravan | 设置摄像机目标 `(7,22)`；使 Bowie 面朝 Right、Peter 面朝 Left、Rohde 面朝 Right。Peter 问该怎么办（2525）。使 Bowie 面朝 Left；Rohde 指出 Taros 身后的 Caravan 并坚持取得它（2526）。 |
| 结束交谈 | 使 Bowie/Peter/Rohde 面朝 Up。Taros 宣布时间已到、战斗开始（2527）；结束。没有传送或剧情标志写入。 |

这些是归属于说话者的陈述：Rohde 的说法不能独立证明 Taros 的历史，他的要求不会取得载具，Taros 的宣告也不完成战斗服务。行动者阵容及对话均不能确立必需的出战队伍组成或 Sword 伤害效果。

## 行动者与服务仍须满足各自条件

**已确认的共享限制：**清空的逻辑查找默认指向 physical0。因此，缺席的 Peter、Rohde 或 FOLLOWER_B 可别名到 main0；失效值 255 是另一种无效状态。这些脚本不会为冲突的随从写入或过长的传入列表修复存在性／容量。`setPos`/`setFacing` 跳过打包己方角色值小于 30 且 HP 为零的角色，而 FOLLOWER_B31 和 Taros128 绕过该守卫。动作安装与点头没有等效的 HP／存在性守卫。探索在警告前治疗；全新战斗在战前场景之后治疗。后续治疗不能确立死亡己方角色此前已被放置。Caravan 的动画计数器 -1 写入不是通用 AI／载具停止。

`eas_Init` 选择速度 32、零加速因子，禁用加速／减速，设置阻挡／碰撞、自动朝向、Up 朝向、关闭浸没及大小 24，刷新精灵并空闲。跳跃禁用自动朝向，请求 `(0,-1)`、等待 5、请求 `(0,1)`、等待 5，再请求 `(0,0)`，恢复自动朝向并空闲。张望动作请求四次交替朝向，每次后等待 30。内联移动从当前位置按 384 缩放相对位移，检查目标冲突并安装行进／速度；目标相等先于空闲。行动者专属等待不汇合所有并发开始的移动、跳跃或张望。后续写入可替换未完成动作，包括警告中未汇合的 Bowie/Peter 工作。

警告的 Taros 修饰符为 `$0`，Peter 为 `$80`；战斗中的 Peter 使用 `$C0`，Taros/Rohde 使用 `$0`。它们选择立绘侧别／镜像位，并带有既有窗口守卫。映射精灵选择 Taros 的立绘／特殊提示音、Peter 的基础／转职后变体以及 Rohde 的特殊精灵属性。`nextSingleText` 解析说话者服务、等待滚动、显示／推进文本、关闭立绘／文本并 Sleep 10；`nextText` 没有该关闭／休眠尾部。文本控制 N、LEADER 和 W1/W2 是换行／替换／确认控制，不是额外场景命令。W1/W2 在 VInt 等待中轮询方向／A/B/C 输入，同时更新 RNG／种子副本；W2 还处理箭头与确认声音。Typewriting0 不表示普通控制就绪。此处未观察对话或声音。

摄像机目标禁用实体跟踪并等待滚动；camera-follow128 使用其映射物理目标，后续文本会等待滚动。`fadeInB` 请求所有调色板以因子 6 从黑色半淡入，并通过 VInt 等待。音乐提交不等待整段曲目。普通 `csWait` 将其低字节用作 VInt 计数。进入时清除跳过状态之后，调试模式加 player2 Start 可设置脚本跳过；被跳过的文本处理器消费参数而不显示／递增游标，被跳过的等待省略 Sleep。End 在对话窗口索引非零时有条件地等待滚动，并清除摄像机速度。点头有独立的动画／精灵／DMA 及 10/20/10 休眠，不检查同意或 HP。以上衔接均不保证所有行动者、渲染、音频或可用输入完成。

## 重建的战斗员与空开始程序

**已确认的全新顺序：**战前包装器返回后，BattleLoop 恢复 map12/battle18、清除区域标志 90–105、治疗符合条件的己方角色、初始化己方／敌方位置、清除 AI 内存、以 d0=0 加载战斗并调用开始程序。治疗将存活己方角色及不死的 Peter/Lemon 的 HP/MP 恢复至最大值，保留既有 STUN/POISON/CURSE 位；其他死亡己方角色不复活。

`BattleSpriteset18`（地址 `0x1B4546`） 提供 12 个己方放置、14 个敌人、五个 AI 区域和零个 AI 点。首个放置为 `(12,27)`，区域原点为 `(0,0)`，不同于神殿请求 `(12,30)` 和场景主实体头部 `(12,32)`。己方初始化清除全部 30 个 X 值并重建当前已加入／出战队伍列表。死亡成员消耗列表／循环槽位，却不推进放置记录；超额成员可退出出战队伍。十二行不强制十二名存活己方角色、Peter、Rohde 或 Oddler 进入交战。首个列表条目在零成员退出检查之前读取；空／不一致的传入列表不保证安全，也不保证被修复。

全部 14 条敌人记录均为 STARTING，槽位 128–141，TAROS 位于 row0 的 `(12,8)`；其他槽位走隐藏／清除路径。即使传入 F518 已设置，Battle18 仍不在随机战斗升级表中。敌人身份与 AI 数据既不能确立激活、回合（单次行动）、伤害，也不能确立获胜路线。

LoadBattle 淡出、加载瓦片并清除 VInt。PositionBattleEntities 清空实体／查找表并声明已放置战斗员，排除负坐标与中立敌人。旧场景与探索随从阵容被替换。视野目标、精灵／地图资源、VInt、地形、音乐与淡入各有独立使用方。只有 F468 未设置时，开始程序才设置它并执行空终止符。全新返回在 ActivateEnemies、区域／生成处理及行动顺序生成**之前**到达 `@Start`。这是激活前交接，不是首次观察到的可玩回合（单次行动）。

| 全新进入时的 F468 | 实体重建时的 F518 | 战前场景与中立 Caravan |
| --- | --- | --- |
| 未设置 | 未设置 | 调度场景 Caravan129；随后替换为 neutral159。开始程序设置 F468。 |
| 未设置 | 已设置 | 调度场景 Caravan129；重建跳过中立尾部。开始程序设置 F468。 |
| 已设置 | 未设置 | 跳过战前／开始脚本；重建仍添加 neutral159。 |
| 已设置 | 已设置 | 跳过战前／开始脚本及中立尾部；交战重建仍执行。 |

受完成标志控制的中立记录在 `(12,5)` 放置一辆**新的** Caravan，面朝 **Left**、logical159，最大／当前 HP0、status0，且带中立激活位。它用 `eas_Motionless`（关闭运动后空闲）声明／映射新的物理行动者。F518 已设置时跳过该尾部。Scene129 的 Down 朝向和动画停止状态不延续：lookup129 此时属于一条敌人记录。Taros 同样依据战斗员记录重建，即使坐标重复。中立 HP0 是声明输入，不是载具败北／获取结果。两个 Caravan 精灵都不是可用运输授予、Rohde 加入、物品栏变更或 Sword 效果测试。

## 证据追踪与审查边界

**已确认的溯源：**已接受 #840/PR842 绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者（英文）](../../../../../research/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md#reproduction-acceptance-and-deferred-evidence)保留枚举／传送／初始化／指针／头部／命令／行动者／名册／中立关联。本指南使用已接受关联，不重新调查源码、重新生成载荷或获取运行时证据。

| 说明的边界 | 合同与确切的既有测试夹具 | 审查限制 |
| --- | --- | --- |
| MainLoop 与战斗选择 | [探索流程](../../../contracts/world/exploration-control-flow.md)、[地图进入路由](../../../contracts/world/map-entry-routing-state.md)；[流程](../../../../../../tests/fixtures/h2/gameflow-core-static-v1.json)中的 `sf2-gameflow-core-static-v1`、[地图](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)中的 `sf2-common-maps-static-v1` | 共享调用顺序及解锁／通配／完成事实；选定 Map12/68/69 记录来自已接受研究。 |
| 配置、警告与待处理请求 | [地图探索](../../../contracts/world/map-exploration.md)；[初始化](../../../../../../tests/fixtures/h2/map-init-static-v1.json)中的 `sf2-map-init-static-v1`、[请求](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-event-request-consumption-static-v1` | 初始化的 `expected.embeddedPrograms` 选择 `cs_57D22`；共享待处理事实不能组合成自然滚动／撤退。 |
| 实体生成、动作、摄像机与对话 | [名册](../../../contracts/party/party-roster-state.md)、[对话](../../../contracts/interface/dialogue-system.md)、[地图探索](../../../contracts/world/map-exploration.md)；[脚本](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)中的 `sf2-map-script-engine-static-v1` | `expected.entityPopulationCommandFacts`、`entityPlacementCommandFacts`、`entityActionBridgeCommandFacts`、`mapCameraControlCommandFacts` 和 `dialogueCommandFacts` 拥有处理器／源码关联，而非已完成的 Taros 服务。 |
| 战前／开始与全新／已保存边界 | [过场路由](../../../contracts/battle/battle-cutscene-routing.md)、[战斗生命周期](../../../contracts/battle/battle-control-lifecycle.md)；[钩子](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)中的 `sf2-battle-cutscenes-static-v1`、[控制](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)中的 `sf2-battle-control-static-v1`、[循环](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)中的 `sf2-battle-loop-static-v1` | 共享开场／F88／治疗事实；确切的战前 → 重建 → 开始顺序和 slot18 目标使用更严格的已接受研究关联。 |
| 交战／中立身份与成员资格 | [遭遇定义](../../../contracts/battle/battle-encounter-definition.md)、[名册](../../../contracts/party/party-roster-state.md)；[路由](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)中的 `sf2-battle-routing-data-static-v1`、[精灵集](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)中的 `sf2-battle-spriteset-data-static-v1`、[属性值](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)中的 `sf2-common-stats-static-v1` | `routeTargetsParsed=false` 和 `rowContentTracked=false`；聚合计数不能确立 slot18 记录、Caravan159 或交战结果。这些选定关联仍归研究所有。 |

相应的 schema／使用方配对为[流程 schema](../../../../../../schemas/h2-gameflow-core-static-fixture.schema.json)／[gameflow.py](../../../../../../src/sf2tool/h2/gameflow.py)、[地图 schema](../../../../../../schemas/h2-common-maps-static-fixture.schema.json)／[maps.py](../../../../../../src/sf2tool/h2/maps.py)、[初始化 schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)／[map_init.py](../../../../../../src/sf2tool/h2/map_init.py)、[请求 schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)／[map_event_request_consumption.py](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)、[组合脚本 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)／[map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)及其[组件所有权](../../../../../../src/sf2tool/h2/map_script_schema.py)、[钩子 schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)／[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、[控制 schema](../../../../../../schemas/h2-battle-control-static-fixture.schema.json)／[battle_control.py](../../../../../../src/sf2tool/h2/battle_control.py)、[循环 schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json)／[battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、[路由 schema](../../../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)／[battle_routing_data.py](../../../../../../src/sf2tool/h2/battle_routing_data.py)、[精灵集 schema](../../../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)／[battle_spriteset_data.py](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)，以及[属性值 schema](../../../../../../schemas/h2/common-stats-fixture.schema.json)／[stats.py](../../../../../../src/sf2tool/h2/stats.py)。其所属命令依次为 `uv run sf2 h2` 加 `gameflow-core`、`common-maps`、`map-init`、`map-event-request-consumption`、`map-script-engine`、`battle-cutscenes`、`battle-control`、`battle-loop`、`battle-routing-data`、`battle-spriteset-data` 和 `common-stats`。受控复现在同一进程加载被忽略的工作树输入配置。

综合审查依据已接受研究、共享合同和有界的已跟踪测试夹具、schema／使用方选集，检查标志独立性、行动者／列表／HP 限制、服务顺序及聚合过度主张。生命周期合同中精简的全新进入摘要不是有序的战前／开始安排；其[战前程序体所有权](../../../contracts/world/map-exploration.md#已绑定的战前主体before-body与战斗加载器所有权)与已接受研究提供此处使用的精确顺序。这一较窄说明不要求修复 Layer A。独立传入的 F418/F725/F468/F518、F88、F419、随从冲突、缺失查找／死亡己方角色、不同出战队伍列表、模式／木筏状态及被替换／停滞请求是用于辨别边界的审查用例，不是已执行的运行时矩阵。

[PR842 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116)、[原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161137887)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331)保留原始／变基后 CI、保留比较、已修正／失败检查及链接的前序结果。猜测路径失败或输出过大都不是不存在的证据。已接受结果不会仅因文字说明而重放；聚合测试夹具不会变成组合运行时覆盖。

**未知：**自然旅行／准入／撤退；实际标志、HP、列表、映射、运输／木筏及待处理／输入状态；动作／所有行动者／服务完成、渲染出的摄像机／文本／音频、可用控制及存档／恢复持久性。它们继续按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md) 延后处理，不授权获取证据、后续剧情调查或新的重制版能力。
