# Battle18 结果与 Caravan 获取

首次完成 Taros 战斗后，Rohde 在一段场景中检查小巧的 Caravan，似乎进入其中，并解释为何其尺寸会造成误解。Peter 质疑他的说法；Rohde 自称驾驶者，旁白则说他随军团同行。实际的末尾变更是设置 F65、清除 F84。战斗完成、战斗成员资格、重建的探索行动者及当前运输控制器各有独立使用方。

**已确认：**已接受的[研究所有者](../../../../../research/chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md)确立条件结果路径、完整首次胜利程序、选定变更及紧接的返回。**推断：**其对话与调度将 Rohde 呈现为 Caravan 的驾驶者，以缩小作为进入其中的解释。**未知：**自然结果、实际传入状态、渲染出的消失／返回、服务完成、可用控制及持久性。

此前的条件准入见 [Taros 警告与 Battle18 进入](southwest-parmecia-through-taros-battle18-entry.md)，独立的接受／跟随写入见 [Rohde 的提议（中文）](hassan-ancestry-and-rohde-caravan-choice.md)。两者都不提供自然获胜路线。本指南停止于紧接的条件探索／控制边界或无关战斗准入。不包含 Sword 伤害、交战模拟、Caravan 操作／菜单／存储、Rick 及后续旅行。独立的[可选 Rick 招募指南（中文）](hassan-rick-recruitment.md)说明有条件的 Hassan 实体生成及当前成员资格／跟随，不确立必需的神殿至 Hassan 返回。独立的 [Caravan 登乘与 Southwest Parmecia 旅行指南](caravan-boarding-and-southwest-parmecia-travel.md)说明选定神殿出口／准入、mode1 登乘／移动／登陆及步行的独立 Battle19 请求；其条件控制／准入边界不提供自然行程或菜单事务。Fnnn 表示标志 nnn；坐标单位为地图瓦片，索引从零开始。下文等待与速度均为源码操作数，不是实测时间。对话采用转述。

## 战斗如何结束

**已确认：**Battle18 的击败路由为空的 `ms_Empty`，但其首领记录行为 -1，共享首领尾部仍执行工作。在回合（单次行动）后击败钩子处，Bowie 必须 HP 非零，enemy128（Taros）必须 HP 为零。F518 未设置时分发空脚本；F518 已设置时跳过分发。两个符合条件的分支均将 32 个槽位中每个 HP 非零的敌人追加到击杀列表，保留末尾 -1。该尾部不检查放置，也不直接将这些敌人的 HP 归零。

随后，击杀列表使用方执行死亡表现／记账、写入 X/Y=-1、清除状态、重建属性值并将映射实体移出地图。之后的计数只接纳 X 非负且 HP 非零的战斗员；若 Bowie 的 HP 为零，则强制己方计数为零。先检查己方为零，再检查敌方为零。因此 Taros 死亡可通过清理移除其余敌人，但 Bowie 同时死亡时并不覆盖败北。若战斗继续，回合（单次行动）后效果还会经过第二个击杀列表／计数／先败北后胜利检查点，但那里不再调用击败钩子。这说明结果边界，不说明 Sword 如何伤害 Taros 或哪条交战轨迹获胜。

| 选定结果 | 后果与返回 |
| --- | --- |
| F518 未设置时胜利 | 治疗符合条件的己方角色；预置返回参数；运行完整获取程序；运行共享 Bowie 加入；清除 F418、设置 F518；返回 d4=1。 |
| F518 已设置时胜利 | 治疗并预置返回参数；跳过获取程序；仍运行 Bowie 加入；清除 F418、保留／设置 F518；返回 d4=1。F65/F84 保持传入值。 |
| 败北 | 更新解锁状态，请求疲惫文本／音乐，恢复 Bowie 的 HP，以无符号右移将金币减半并选择传入存档点；返回 d4=-1。Battle4 的特殊败北不适用。 |
| 获准的 Egress 或 Angel Wing | 消耗 MP 或移除所选物品槽位，执行其所属文本／闪光／关闭及解锁更新，展开动作栈帧并选择传入存档点；返回 d4=0。之后不执行胜利场景／加入／完成，也不扣除败北金币。 |

此选定分发未确立逃脱可用性、物品持有或 MP 充足。败北／Egress 不改变 F518 和 F468。其解锁更新在 F518 未设置时不改变 F418，在 F518 已设置时清除它。它们不会获取 Caravan。

## 返回位置与场景的新行动者

**已确认：**胜利治疗存活己方角色与不死的 Peter/Lemon，保留受支持的状态位；其他死亡己方角色不复活。场景之前，重建当前按标志选定的出战列表，并以 CURRENT_MAP、首名成员的战斗员 X/Y 加战斗区域原点（字节运算）及该成员映射实体的朝向预置返回参数；parameter1 为零。对于一致的 Battle18 状态，这是原点 `(0,0)` 的 Map12。所选成员不必是 Bowie、首名存活己方角色或初始放置位置。空／不一致列表与查找映射不获修复。

预置先于场景重建及共享加入。选定场景命令或紧邻动作体均不写入此元组、CURRENT_BATTLE、PLAYER_TYPE 或已保存木筏元组。因此下文戏剧性调度位置不会变成返回位置。

战后包装器只检查 F518。未设置时加载 Map12、摄像机 `(7,5)` 及 `ce_4B6BE`（地址 `0x4B6BE`）：main0 的三字头部给出 `(12,9)`、Up；唯一后续记录给出 Caravan `(12,5)`、Down、sprite62、`eas_Init`。头部不是 NPC，该加载也不运行探索初始化。行动者／查找表被重建，当前按标志选定的随从先于固定记录，main0 另行创建。此场景无 Taros 记录或新声明的战斗员。

| Caravan 身份 | 所属实体生成 |
| --- | --- |
| 战前场景 logical129 | 此前对峙中的 Caravan，Down；被战斗重建替换。 |
| 战斗 neutral159 | F518 未设置时交战重建中独立的 Left/HP0/`eas_Motionless` Caravan；不是获取结果。 |
| 战后场景 logical128 | 此次新实体生成中唯一固定记录，Down；复用 128 不会变更 Taros combatant128。 |
| 缩小程序体中的原始 physical1 | 直接 ENTITY_DATA 操作数；分配决定其行动者。它不是 logical128 查找。 |

即使独立设置 F65，Map12 仍使用普通随从表。F67 声明 Peter、F84 声明 Rohde、F68 声明 Oddler/FOLLOWER_B31；其他传入标志保留有序效果，包括 Astral 标志 74/70 覆盖 FOLLOWER_B。场景本身不声明或检查这些说话者。仅设置 F67/F68/F84 时，physical1 为 Peter、2 为 FOLLOWER_B、3 为 Rohde、4 为 Caravan128。这是有条件的分配，不是自然出战队伍断言。缺失映射默认为 0，可能别名到主实体；失效值 255 则不同。容量安全和身份冲突均不获修复。

## Rohde 的演示与 Peter 的反应

**已确认的有序程序：**`abcs_battle18`（地址 `0x4B2F2`） 包含 212 条命令、16 个内联移动体、四个自定义动作体及 15 个文本位置。下列阶段覆盖完整的剧情、动作和摄像机含义。开始动作会安装工作而不等待；等待动作只轮询该映射行动者的空闲指针。两种形式均不确立所有行动者或表现完成。

### 检查小巧的 Caravan（命令 0-40）

程序依次初始化／等待 Bowie、Peter、FOLLOWER_B 和 Rohde；将 Peter 放在 `(13,9)` Up、FOLLOWER_B 放在 `(14,9)` Up、Rohde 放在 `(5,9)` Right；停止 Caravan 动画、淡入并等待 60。Rohde 称赞具名领队的实力（2528）。Bowie/Peter/FOLLOWER_B 转向 Left，等待 30；摄像机跟随 Rohde，等待 120；Rohde 先 Right6 再 Up4，分别等待该行动者，而其余三人转向 Up。Rohde 随后面朝 Right。

禁用跟踪，摄像机移至 `(7,1)`，随后等待 30。六次放置呈现 Rohde 从不同侧面检查：`(12,6)` Up、`(13,5)` Left、`(12,6)` Up、`(11,5)` Right、`(12,6)` Up、`(13,5)` Left，每次后等待 20。**推断：**此调度将注意力引向他的检查，而不确立由玩家控制的接近。

### 进入、载具运动与消失（命令 41-83）

Rohde 评论尺寸之小并尝试进入（2529）。开始 Left1，等待 3，再开始 `eas_452BA`，等待 3 再等待 60。Caravan 动画开始，等待 30，停止，等待 60，再开始并等待 120。它按 Left/Up/Right/Down 转向两次，除最后 Down 外各朝向间等待 10，然后开始 Jump。等待 10 后，将 Rohde 放在 `(63,63)` Down。一个被等待的自定义 Caravan 程序体设置速度 48/48 并空闲；提交坠落 SFX，Caravan 开始 Down9，脚本等待 20。

一个被等待的自定义 Bowie 程序体设置速度 64/64 并空闲。等待其关闭自动朝向程序体之后，Bowie 开始 Left1 并面朝 Right，Peter/FOLLOWER_B 面朝 Left。等待 60；摄像机目标 `(7,4)`；等待 Bowie Init。**推断：**此顺序调度了看似消失及移动的 Caravan，以供目击者作出反应。源码请求不能证明渲染效果，也不能证明 Rohde 缩小时克隆了场景 Caravan；物理操作数见下文。

### 等待并寻找 Rohde（命令 84-139）

Bowie/Peter/FOLLOWER_B 面朝 Down。Peter 对消失作出反应（2530），等待 60。Bowie 面朝 Right，另外两人面朝 Left；Peter 希望 Rohde 回来，并质疑其诚实（2531），等待 30。三人均面朝 Down，等待 150。等待 Caravan 关闭自动朝向，开始 Up9，提交坠落 SFX，等待 20；Caravan/FOLLOWER_B 面朝 Left，等待 20。Bowie/Peter 按 Up/Up、Left/Right、Down/Down、Right/Left 循环两次，每对之后等待 10。

先等待自定义 Bowie 程序体，再等待 Peter 程序体：motion OFF、orientLeft、精灵更新、跳转到空闲。停止两者动画，等待 30；Caravan 面朝 Down，等待 60；停止 Caravan 动画；等待 FOLLOWER_B BumpLeft 两次，等待 30。**推断：**张望与碰撞动作强调 Rohde 离开期间的不确定感。这些动画变更既不移除交战成员资格，也不确立此前行进已经完成。

### 返回并解释古代装置（命令 140-170）

将 Rohde 放在 `(12,5)` Right，等待 `eas_4536C`，再等待 3。等待 Bowie Init，再等待 Peter Init；使三名目击者面朝 Up，并重启 Bowie/Peter 动画。等待 Rohde Down3。Rohde 否认说谎（2532），Left2 并等待，面朝 Down 并赞颂 Ancients（2533）。等待 Right1；面朝 Up；他解释缩小的人与物品使 Caravan 可以如此小巧（2534）。等待 Up2；面朝 Down；他自称会驾驶，并请求以驾驶者身份同行（2535）。

等待 30 后，旁白文本说具名历史学家随军团同行（2536）。这是唯一打包 -1/ENTITY_NONE 说话者。Peter Up1 并等待，表示不信（2537），Down1 并等待，然后面朝 Left，而 Bowie 面朝 Right、FOLLOWER_B 面朝 Left。Peter 提议带上许多士兵（2538）；等待 Rohde Down1。这里没有 Yes/No 提示或成员资格检查。

### 分歧、最后进入与末尾写入（命令 171-211）

三名目击者面朝 Up。Rohde 说在内部缩小的士兵无法参战（2539）；Peter 保留判断（2540）。Bowie 面朝 Right，Peter/FOLLOWER_B 面朝 Left；Peter 表达不满（2541）。等待 Rohde Up1，面朝 Down，等待 20，等待其 Jump 两次。目击者面朝 Up；Rohde 期待与 Caravan 一起冒险（2542），等待 30。

开始 Rohde Up1，等待 3，开始 `eas_452BA`，等待 3 再等待 60。开始 Caravan 动画，将 Rohde 放在 `(63,63)` Down，等待 60，再等待 Caravan Init。开始 Caravan Down9，等待 30。Bowie 面朝 Right，Peter/FOLLOWER_B 面朝 Left，等待 10；三人均面朝 Down，等待 180。最后设置 F65、清除 F84 并结束。**推断：**第二次进入与载具驶离调度结束 Rohde 的驾驶者介绍。最终写入只确立各自特定的剧情／实体生成谓词。

全部 15 条消息使用 `nextSingleText` 及键 `09E0`-`09EE`；行动者修饰符为 `$0`，旁白则为 `$FF`。LEADER 选择按标志重建的出战成员中首人的姓名；NAME;11 选择 Rohde 的姓名。W1 确认及 2534 内部的 W2 是文本控制，不是额外场景选择。下一段 Oddler 场景不在此程序内。驾驶者、缩小及士兵参战资格是归属于说话者的台词，不是经过验证的运输／存储规则或 Rohde 交战招募。

## 缩小、移动与服务限制

**已确认：**两次 `eas_452BA` 安装均从前面的 `eas_452B2` 移动指令之后进入；它们不发出该指令的移至 Caravan 请求。进入的程序体等待 6、启用尺寸调整、提交传送 SFX，以连续尺寸间 wait1 刷新 22/20/18/16/14/12，等待既有目标位置，克隆原始物理 `ENTITY_CARAVAN=1`，然后空闲。克隆使用方直接索引 ENTITY_DATA，复制 physical1 的当前／目标坐标、行进及朝向，然后刷新 Rohde 的精灵。在仅 F67/F68/F84 的分配下，该源为 Peter，而 Caravan128 为 physical4。没有获准随从时 Caravan 可占 1，但 Rohde 缺席。其他传入标志改变分配。此关系不确立已观察到的视觉错误，也不能将该符号操作数改名为逻辑 Caravan 目标。

返回安装进入 `eas_4536C`，入口位于 `byte_45368` 的克隆指令之后。它相对 Rohde 当前朝向请求一个瓦片、启用尺寸调整、提交传送 SFX，以每次后 wait1 刷新 12/14/16/18/20/22，禁用尺寸调整、刷新 size16、等待目标并空闲。若前面的 `(12,5)`、Right 放置有效，则请求 `(13,5)`；它并非固有的向下离开，也不是克隆 logical128。两个动作体都不调用改变运输模式的外层例程。复用载具动作标签不能确立登乘或离乘。

内联移动按 384 缩放偏移，并经过目标冲突／行进配置，随后等待目标相等并空闲。Jump 请求 Up1/wait5、Down1/wait5，再请求零偏移并恢复自动朝向；BumpLeft 使用 Left1/Right1/zero，等待相同。Caravan 已开始的 Jump 可被后续自定义速度体替换，而没有独立的完成汇合。已开始的 Down9/Up9 移动及自定义目击者／Init 安装同样不能证明此前移动完成。Speed48/48 和 64/64 是成对 X/Y 速度字节，不是时长。

`ac_motion OFF` 与开始／停止动画写入动画状态，不是载具控制状态或运动取消。`orientLeft` 选择 orientation1，不同于 Left facing2。Init 在精灵刷新和空闲之前恢复 speed32/32、加速／碰撞／自动朝向／朝向取向／浸没及 size24；等待 Init 不是等待旧目标。`setPos` 写入当前／目标坐标并刷新，不取消已安装动作。将 Rohde 移至 `(63,63)` 既不删除他，也不改变 HP／成员资格；未完成动作随后仍可再次写入其位置。

逻辑查找没有存在性守卫。放置／朝向与开始／停止动画跳过小于 30 且 HP 为零的己方角色；FOLLOWER_B31 和 Caravan128 绕过该己方守卫。动作安装／自定义体不使用它。胜利治疗可使 Peter 存活，却不声明他，也不确保死亡 Rohde 被治疗。行动者缺席、HP 和物理分配是相互独立的依赖。

文本等待滚动，使用映射的立绘／台词属性，推进／关闭文本并 Sleep10；打包 -1 旁白绕过行动者立绘选择。确认需要独立的输入／VInt／RNG 服务。已接受的[文本／服务观察（英文）](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md)未观察这 15 条消息。摄像机目标禁用跟踪并等待滚动；跟随 Rohde 使用其映射物理身份，中性 `$FFFF` 禁用跟踪。`fadeInB` 使用共享的从黑色半淡入服务。普通等待使用低字节 VInt 休眠；调试／player2-Start 跳过可省略 Sleep，并跳过文本显示／游标推进。声音提交、文本、淡入淡出和摄像机等待不汇合所有行动者。End 在有对话窗口时有条件地等待滚动、清除摄像机速度并恢复寄存器，不汇合未完成动作。渲染完成仍属未知。

## 获取标志与共享 Bowie 加入

**已确认：**末尾 setF65/clearF84 既不设置 F725、不让 Rohde 加入，也不写入 PLAYER_TYPE、F64 或已保存 RAFT_MAP/X/Y。清除 F84 改变后续实体生成选择，不立即删除场景中的 Rohde。只要 F518 未设置，即使 F65 已设置或 F84 不存在，场景仍运行。F518 已设置时，重复胜利跳过全部场景写入，使不一致的 F65 未设置或 F84 已设置状态保持不变。F468 开场门槛不控制此战后场景。

两个包装器路径均到达 `EndAfterBattleCutscene`。Slot18 的加入值 0 经 `j_JoinForce` 解析到 Bowie 已加入 F0 写入、军团列表重建，以及计数小于 12 时有条件的出战队伍加入。源码表的 unused 注释不会移除这个实际执行的尾部。加入的是 Bowie，不是 Rohde，且发生在返回成员选定之后。只有随后胜利才清除 F418、设置 F518、读取已保存返回参数并返回 d4=1。F468 保留。

**推断的剧情含义：**Rohde 口头提出驾驶、旁白说明同行，以及以 F65 选定大地图内容替代其普通 F84 跟随，共同描述获取交接。**已确认的变更边界：**它们各自都不授予 Rohde 战斗员、当前 Caravan 模式或可操作的物品栏／菜单／存储能力。

## 紧接胜利的返回：神殿控制或警告与撤退

**已确认：**一致的胜利按场景前预置的位置／朝向返回 Map12，而非主实体调度位置 `(12,9)` 或 Caravan 最终位置。MainLoop 从 `alt_MainLoopEntry` 恢复，随后执行 SwitchMap 和 ExplorationLoop，不经过 CheckBattle。完整切换地图表无 source12 记录。独立传入的解锁状态不会在此次直接结果返回时扫描；后续普通请求有独立准入边界。

按传入地图全新进入探索会清除待处理状态，将字 STEP_COUNTER 减少 20000 并在负值处钳位，治疗符合条件的己方角色，淡出／加载瓦片，写入 map12/battle255，重建实体，清除临时标志 256-383，设置 F80，加载地图，安装基础 VInt 并运行初始化。战后场景阵容与未完成动画被替换。Map12 唯一固定记录声明 Taros128 `(12,8)` Down、特殊 physical47；此处没有 Caravan 记录保留。Map12 不是大地图，因此 F65 不添加其 Caravan 随从。首次获取清除 F84，因而不生成 Rohde；其他传入随从标志保留。

初始化先检查 F725，再检查 F518。未设置时执行完整的已接受警告 `cs_57D22`；已设置时跳过。只有随后，F518 才将 Taros 移出地图。胜利刚设置了 F518，但仅完成并不抑制警告，获取与初始化也都不修复 F725。

| 传入的 F725 | 紧接的继续路径与停止点 |
| --- | --- |
| 已设置 | 跳过警告、移出 Taros、执行条件音乐／淡入淡出。没有新待处理事件时，WaitForEvent 跟随 physical0 并安装当前模式控制；通常战斗进入已留下 mode0。可用输入未观察。 |
| 未设置 | 在移出 Taros 之前运行警告。其无滚动请求指向 Map68 `(55,44)`、Down。初始化／音乐／淡入淡出后消费待处理请求，中间不安装普通控制。 |

[完整警告指南](southwest-parmecia-through-taros-battle18-entry.md#警告与撤退请求)说明集合接近、重复反应、发现／威胁、Peter 请求离开及撤退动作／对话。其最后的行动者专属等待不汇合此前所有动作。传送写入 type1／无滚动参数，不写 WARP_SFX；使用方清除待处理状态、提交／清除传入声音、使 main0 空闲，并向 MainLoop 的普通战斗扫描返回 `(68,55,44,Down,d4=0)`。不保证神殿中存在可玩暂停，被替换的待处理状态也不是原子队列。

Map68 唯一战斗记录行为 19。传入 F419 会准入它；F519 可在选择后清除解锁状态，不取消该次调用。在此无关准入处停止。否则，全新 Map68 探索具有空固定配置及只返回的初始化。F65 在普通随从之前，选择大地图表中 physical1 的 Caravan FOLLOWER_A；后续冲突的 FOLLOWER_A 标志可覆盖其逻辑映射。首次获取清除了 F84；重复胜利保留传入值。若无另一待处理事件，WaitForEvent 到达通常的条件控制边界。声明不选择 mode1。

胜利与警告保留 F64 及已保存木筏状态。在 mode0，只有 F64 已设置且保存地图匹配时，才声明独立的 physical31/lookup63 木筏；mode2 则另行改变主实体精灵并隐藏随从。获取与警告均不产生 mode2。运输模式、随从声明、已保存木筏位置及实际操作仍相互独立。此处不继续登乘或菜单。

## 败北与 Egress：传入存档点

**已确认：**Battle17 在 `GetEgressPositionForBattle` 中有硬编码 Hassan 特例；Battle18 落入 EGRESS_MAP 与 `GetSavepointForMap`。普通战斗进入已设置 F399。匹配的存档点记录提供 X/Y／朝向；无匹配时保留传入地图字节，采用默认 `(1,1)`、Up。这不是经过验证的任意地图保证。异常的 F399 未设置直接调用，在表／木筏选择之前返回最初的 Bowie 房间元组。

F64 未设置时保留已保存木筏坐标。F64 已设置时，存档点选择写入一个重置元组：

| 选定存档点地图 | 已保存木筏元组 |
| --- | --- |
| Hassan9 | Map69 `(28,55)` |
| Polca13 或 Bedoe10 | Map72 `(43,48)` |
| 其他任意表选择 | 哨兵默认 Map69 `(42,15)` |

这些是保存位置写入，不是登乘、模式规范化或立即在返回地图放置。传入 EGRESS_MAP9 时，返回为 Hassan `(8,9)`、Up。其已接受的[实体生成／初始化指南（中文）](battle16-outcomes-through-hassan-entry.md#hassan-实体群与条件控制)保留默认／F518 替代阵容、普通随从、共享初始化及依赖 F65 的 Rohde 移出。首次败北留下 F518 未设置；重复败北保留它。两者都不设置 F65 或清除 F84。重置到 69 的木筏不会被放在城镇 9。初始化／服务之后，没有待处理工作时允许有条件地交还控制。这一已接受初始化不隐含任何 Rick 交互或调查。

直接败北／Egress 返回也绕过 CheckBattle。其他传入地图有各自的 SwitchMap、配置／初始化及待处理行为；其继续路径在此为未知，而非假定等同 Hassan。实际 EGRESS_MAP 仍为传入状态，不是强制城镇或自然败北行程。不一致的 map255 使用既有实体分支，不执行全新重建。停止于紧接的条件返回／控制边界；不准入全地图初始化调查。

## 证据追踪与审查边界

**已确认的溯源：**已接受 #844/PR846 绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[研究复现所有者（英文）](../../../../../research/chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md#reproduction-acceptance-and-deferred-evidence)保留实际 slot18 指针／首领／加入记录、完整场景／实体头部、文本键、紧邻动作体及调用方／分配／存档点关联。这些已接受关联提供下文选定的数量与行为；本综合文档不开展新源码调查。

| 说明的边界 | 合同与确切测试夹具 | 该可执行所有者的限制 |
| --- | --- | --- |
| 首领清理、治疗与结果 | [战斗生命周期](../../../contracts/battle/battle-control-lifecycle.md)、[过场路由](../../../contracts/battle/battle-cutscene-routing.md)；[循环](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)中的 `sf2-battle-loop-static-v1`、[钩子](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)中的 `sf2-battle-cutscenes-static-v1`、[控制](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)中的 `sf2-battle-control-static-v1` | `expected.lifecycleFacts` 区分击杀列表放置写入与 `killRemainingEnemies` 的 HP 写入；`cutsceneFacts` 为首领清理及共享战后加入设置门槛。实际 slot18 与先败北后胜利的调用方关联仍归研究所有。未观察 Battle18 回合（单次行动）。 |
| Slot18 包装器与加入参数 | [过场路由](../../../contracts/battle/battle-cutscene-routing.md)；[路由](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)中的 `sf2-battle-routing-data-static-v1` | `expected.facts.routeTargetsParsed=false`；填零的加入表事实不替代选定分发／调用方。其 unused 标签不否决执行。 |
| 场景实体生成、动作、文本、摄像机与标志 | [地图探索](../../../contracts/world/map-exploration.md)、[对话](../../../contracts/interface/dialogue-system.md)、[名册](../../../contracts/party/party-roster-state.md)；[脚本](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)中的 `sf2-map-script-engine-static-v1` | `expected.entityPopulationCommandFacts`、`entityPlacementCommandFacts`、`entityActionBridgeCommandFacts`、`entityLifecyclePresentationCommandFacts`、`entityCloneCommandFacts`、`mapCameraControlCommandFacts`、`dialogueCommandFacts` 和 `storyStateCommandFacts` 保留处理器／位置／顺序数量。它们不执行 Rohde 的原始物理缩小程序体，也不组合场景完成。 |
| Bowie 加入与 Rohde 跟随的区别 | [成员资格](../../../contracts/party/party-membership-state.md)、[名册](../../../contracts/party/party-roster-state.md)；[属性值](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)中的 `sf2-common-stats-static-v1` | 共享列表／加入事实保留自身数值／容量／普通调用方限制。F0、row18 value0、threshold12 及有序预置／加入后果使用已接受的选定研究，而非笼统的成员资格推断。 |
| 全新神殿初始化与警告 | [地图探索](../../../contracts/world/map-exploration.md)；[初始化](../../../../../../tests/fixtures/h2/map-init-static-v1.json)中的 `sf2-map-init-static-v1` | `expected.embeddedPrograms` 选择 `cs_57D22`；其 67 条操作记录包含内联动作 token，不同于研究的 51 条场景命令。F725 先于 F518 及传入返回阵容使用已接受初始化关联。 |
| 直接结果与普通请求的区别 | [探索流程](../../../contracts/world/exploration-control-flow.md)、[进入路由](../../../contracts/world/map-entry-routing-state.md)；[流程](../../../../../../tests/fixtures/h2/gameflow-core-static-v1.json)中的 `sf2-gameflow-core-static-v1`、[地图](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)中的 `sf2-common-maps-static-v1`、[请求](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-event-request-consumption-static-v1` | 共享返回／事件优先级、地图选择和 `eventRequestConsumption` 不同于实际 Map68 F419 准入、传入存档点、行动者／控制器及初始化／服务状态。未提供组合的自然路线。 |

有界的已跟踪测试夹具选集确认 `abcs_battle18` 在 command2 加载实体、在 209/210 执行 setF65/clearF84，并有 15 个 `nextSingleText` 位置及一个文本游标。对话组的 commandCount16 不是整个程序的 212。初始化的内联 token 数同样不重新定义场景命令数。完整移动／自定义体／对话说明使用更严格的已接受研究程序审查，不从这些精简聚合量外推。

Schema／使用方配对为[循环 schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json)／[battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、[钩子 schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)／[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、[控制 schema](../../../../../../schemas/h2-battle-control-static-fixture.schema.json)／[battle_control.py](../../../../../../src/sf2tool/h2/battle_control.py)、[路由 schema](../../../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)／[battle_routing_data.py](../../../../../../src/sf2tool/h2/battle_routing_data.py)、[脚本 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)／[map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)及其[组件所有权](../../../../../../src/sf2tool/h2/map_script_schema.py)、[属性值 schema](../../../../../../schemas/h2/common-stats-fixture.schema.json)／[stats.py](../../../../../../src/sf2tool/h2/stats.py)、[初始化 schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)／[map_init.py](../../../../../../src/sf2tool/h2/map_init.py)、[流程 schema](../../../../../../schemas/h2-gameflow-core-static-fixture.schema.json)／[gameflow.py](../../../../../../src/sf2tool/h2/gameflow.py)、[地图 schema](../../../../../../schemas/h2-common-maps-static-fixture.schema.json)／[maps.py](../../../../../../src/sf2tool/h2/maps.py)，以及[请求 schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)／[map_event_request_consumption.py](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)。其既有命令依次为 `uv run sf2 h2` 加 `battle-loop`、`battle-cutscenes`、`battle-control`、`battle-routing-data`、`map-script-engine`、`common-stats`、`map-init`、`gameflow-core`、`common-maps` 和 `map-event-request-consumption`。受控复现在同一进程加载被忽略的工作树配置。这些命令用于指明所有权，不是为文字说明新增运行。

综合前审查依据已接受研究、这些合同边界及选定测试夹具／schema／使用方字段，检查独立标志、HP／列表／查找依赖、命令与载荷单位、异步覆盖、原始物理身份及返回顺序。不要求修复 Layer A：其更严格的选定关联优先于精简摘要。Bowie/Taros HP 组合、第二个结果检查点、独立 F518/F468/F65/F84/F725、死亡／缺席 Rohde、冲突随从、physical1 变体、空出战列表、被覆盖／跳过动作、EGRESS_MAP/F399/F64／模式变体及被替换请求是用于辨别边界的审查用例，不是已执行的运行时矩阵。

[PR846 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161451537)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161473505)保留原始／变基后身份与规划器回执、保留比较、已修正／失败检查及最终 CI。它们保留 [PR842 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331)，其中包含前序结果。已接受比较不会仅因文字说明而重放；不弱化任何测试夹具、黄金夹具或 schema，聚合事实不提供组合运行时覆盖。

**未知：**自然到达／结果；实际标志、HP、名册／查找、返回预置、存档点、运输／木筏及待处理／输入状态；所有行动者与精灵／文本／淡入淡出／摄像机／音频完成；渲染出的缩小、消失和返回；可用输入及存档／恢复持久性。非 Hassan 传入地图的初始化继续路径在此边界内仍属未知。它们继续按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md) 延后处理，不自动获取证据，也不主张操作／菜单／存储／Rick／后续旅行能力。
