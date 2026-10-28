# Battle26 结果与 Higins 招募

> 本文是[规范英文原文：Battle26 outcomes and Higins recruitment](../../synthesis/battle26-outcomes-and-higins-recruitment.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

Battle26 已接受的后续有三种不同结果：战斗结束、一次行动转移 Black Ring，以及首次胜利场景招募 Higins。**已确认：** 各自具有独立的谓词与写入。首领清理不授予戒指，加入也不恢复 Higins 的 HP。营救对话请求去 Pacalon 帮忙，但战斗返回的是该对话之前选定的位置，并不将队伍运送到那里。

本文接续[激活与首次单次行动交接](battle26-activation-and-first-turn-dispatch.md)，不补充首次调用与结果之间的交战记录。消费 [#1018](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1018)下已接受的 [Battle26 结果研究](../../../research/battle26-outcomes-and-higins-recruitment.md)。除非明确指出既有受控观察，下文**已确认**指具名静态源码/消费者边界。**推断：** 云团离去表现了附身威胁被解除。**未知：** 自然抵达 Battle26、实际交战/奖励/状态、已完成服务、已呈现营救、可用探索与持久性。本指南不建立重制版能力或之后的城镇准入。

## 结束战斗

**已确认：** 一次单次行动返回后，BattleLoop 可先运行调试 kill-all 分支，再调用败北钩子，处理当前死亡列表并统计剩余战斗员。先测试己方为零，再测试敌方为零。两者均耗尽时结果为败北。若均不成立，则读取实际当前行动者，运行行动后效果，处理死亡并重复计数测试。只有非终局结果才将当前单次行动字节偏移增加2。

状态处理是另一次结束战斗的机会。其效果辅助在入口清空死亡列表长度，跳过 HP0 行动者；致命中毒时，将该当前行动者写作新列表首成员。不重放行动先前造成的死亡。此轮没有第二次败北钩子调用：此时被杀的首领先被处理和计数，之后才可能再次追加首领清理列表。

Battle26 首领清理涉及初始首敌 **Lizardman128**，而非 Geshp 或战斗中立 Higins159。Bowie HP 非零且128 HP 为零时，败北包装器检查完成 F526。未设526 则运行空脚本；持有526 则跳过。两路均抵达共享首领存在尾部。Battle26 的非零首领字节使其扫描128–159，将每个非零 HP 槽追加到所供死亡列表，附加末尾255 并增加长度。不测试位置、配置行、激活或中立状态，不清 HP，不清旧列表，也不去重。新中立159 HP0；变化后的 HP 可使其参与。具有相似守卫的位置监听器没有 Battle26 覆盖行，因此不对此战斗作重定位。

列表处理使所列行动者旋转/发声，更新己方败北或计入 BATTLESCENE_FIRST_ALLY 的敌人击杀，将 X/Y 清为 -1、状态清为0，重算属性，并将映射实体停放于 `$7000,$7000`。既不将 HP 清零，也不清列表长度。对清理的描述不能修复中断服务或所供陈旧/重复列表。

之后计数使用不同谓词：30 个己方槽与32 个敌方槽中，X.b 非负且 HP.w 非零即计入。不看 Y、加入/出战成员资格、AI、区域或 neutral 位。Bowie HP0 强制己方计数为0。因此无需首领死亡也可由敌方为零而胜利；首领尾部则可通过清 X，将仍存活敌人移出计数。零计数或 HP 值均不证明所有表现/死亡服务完成。F88 延续消费所供槽、列表与标志，不重建新的十五敌人编队。

共享[战斗生命周期](../../../research/battle-loop.md)与[过场路由合同](../contracts/battle-cutscene-routing.md)拥有通用机制；选定研究拥有这些精确 Battle26 分支。

## Black Ring 属于独立的行动事务

**已确认：** 选定掉落表行10 是 `(battle26, target130, item24, dropFlag10)`，位于 `$BE7A..$BE7E`。源码敌人序号2 成为战斗员130：初始化物品栏含 Black Ring 的初始 Dark Bishop。其角色独立于首领128。成功初始化剥离物品条目标志；之后掉落规则匹配掩码后的物品索引，因此这些标志不决定是否符合条件。

行动引擎在每个获准目标效果后，以及获准第二次/反击攻击后，用实际行动者/目标调用 `battlesceneScript_DropEnemyItem`。每个目标开始时 targetDies 清零；伤害辅助在 HP 减少/读取返回零时设它。这连接条件调用方与掉落辅助，不证明某次具体攻击发生。

选定 Black Ring 分支按以下顺序操作：

1. 要求己方行动者、敌方目标与非零 targetDies。匹配当前战斗/目标，再在目标四个物品栏槽中找索引24。搜索时掩去装备、损坏、AI-use 与掉落条目位。物品缺失不会因战斗结束而成为奖励。
2. 采用无条件掉落分支：Black Ring 不属于使用 RNG(32) 的三种武器用例。此处不用掉落 RNG，也不测试升级/完成；无条件分支后的升级指令不可达。
3. 测试并设置 ENEMY_ITEM_DROPPED_FLAGS 的 bit10：byte1/bit2，mask4，位于 `$FFF7A5`。已设置则返回。这既不是加入 F10，也不是 F526。此写入早于移除与收取。
4. 移除目标槽，压紧后续物品，将末槽填为 NOTHING，并重算目标属性。然后测试行动己方角色的 HP 与容量。
5. 有空槽的存活行动者在首个空槽收到纯索引24。只有 AddItem 成功才追加目标掉落与行动者收取消息命令。之后呈现另属服务。辅助不装备戒指、不授予金币、不选择 Bowie，也不搜索其他队伍物品栏。
6. 已死或满容量行动者走珍稀物品回退。Black Ring 是 RING/RARE/CURSED/BREAKABLE，因此其 DEALS_ITEMS+12（`$FFF610`）高半字节特卖计数递增，最高15。已有15 保持15，而物品与掉落标志已消费。此路不发出两条成功收取消息中的任何一条。

首领列表清理不调用掉落事务：130 可从计数中消失而未授予戒指。行动后死亡与胜利也不是替代调用方。持有掉落 bit10 会阻止再次转移，即使重生、持有新戒指副本或使用不同行动者。设位后中断可在移除前阻止重试；移除后中断可导致没有交付戒指。此处无回滚或已证明存档持久性。戒指的 BLAZE2 使用和装备职业规则不改变此查找。

[交战合同](../contracts/combat-resolution.md)保留通用掉落政策。既有受控观察在 Battle01 行动边界替换例程输入，并非 Battle26 或 Black Ring 的观察。

## 胜利在安排营救前准备返回

**已确认：** 胜利先治疗全部符合条件的己方槽0–29，独立于加入/出战标志。HP 非零即可；Peter7 与 Lemon28 在 HP0 时也符合。符合槽获得最大 HP/MP，状态掩码为 STUN/POISON/CURSE，随后重算属性。其他死亡己方角色保持死亡。因此存活但未加入的己方19 可在招募前被治疗，而死亡 Higins19 不会复活。独立战斗中立159 不向己方19 提供属性。

接着 PARAM2 接收 CURRENT_MAP。军团重建选取升序出战成员列表首字节，不检查长度或 HP。该成员 X/Y 加上战斗区域原点，以字节相加写入 PARAM3/4；其映射实体为 PARAM5 提供朝向。PARAM1 变为0，不写 MAP_EVENT_TYPE。普通 Map72/Battle26 原点 `(28,0)` 下，种子为 `Map72, ((x+28) mod256, y mod256), actual facing`。不一定选择 Bowie 或存活/屏上成员。空出战列表暴露其保留首字节；地图/裁剪/位置/映射变化会改变结果。

之后 after 包装器才测试 F526。未设526 分发 `abcs_battle26`；持有526 跳过营救脚本。两路均到共享 JoinForce0 尾部。加入 F19 不是场景门槛：所供 Higins 即使已加入，526 未设时仍可执行场景与加入表现。持有526 不重放营救、join19，也不重建其临时随从。

区分三个战斗标志。更早请求辅助设置解锁 F426；有序 CheckBattle 可在已持有完成526 时清除它。场景前分发检查 intro F476 而不设置；新 start 包装器之后在空 start 脚本前设476。F88 延续绕过新的 before/start 配置。这些均不证明介绍或招募完成。完成526 只在 after 包装器返回后写入。

## 营救场景中有哪些行动者

**已确认：** 场景向黑色淡出，以乘3 的摄像机操作数 `13,11` 加载 Map72，并安装列表 `ce_4BE4A`。加载地图不清 CURRENT_BATTLE26、不改 PARAM2–5，也不运行 ExplorationLoop 完整重置/人口/治疗/配置初始化生命周期。摄像机坐标不是玩家返回坐标。

行动者初始化清实体查找/状态与行走临时区，从物理1 创建符合条件的随从，处理固定列表，再在物理0 声明 Bowie。战斗的中立159/B31 映射被替换；它不是营救的逻辑己方19。

| 固定列表记录 | 瓦片位置 / 朝向 | 实际分配 |
| --- | --- | --- |
| Bowie0 | 19,16 / Up | physical0，初始 Idle |
| Peter7 | 17,14 / Down | 仅当查找缺失时在随从之后分配；否则保留其随从行动者 |
| Higins19 | 18,14 / Down | 下一个物理槽；无持久 Higins 随从行 |
| EVIL_CLOUD106 | 63,63 / Up | NPC logical128，查找偏移32；下一个物理槽 |

InitializeMapEntities 将位置乘384。声明安装精灵、行动、朝向、位置/目的地与实体编号/层，清除行程与速度；固定行使用 Init。行动者存在不设置成员资格。若准入随从有 N 个，且只有 Peter 需要固定行时 P=1，则 Higins 为 physical N+P+1，Cloud 为 N+P+2。仅持有连贯 F65/F67/F70 时，Caravan A 为 physical1、Peter2、随从 B3、Higins4、Cloud5。

其他所供标志改变阵容。Map72 上，F65 启用 Caravan A30；其余随从行按源码顺序为66/Sarah、67/Peter、72/Kazin、73/Slade、74/B/Astral、66/Chester、68/B/Oddler、75/Luke、69/C/Fairy、70/B/Astral、74/A/King、71/Lemon、74/D/Minister、76/Zynk、77/A/Old Man、82/Frayja、83/A/King、83/D/Minister、84/Rohde。别名 A/B/C/D 指 logical30/31/158/159，而非相应物理槽。重复别名行分配并覆盖查找；仅固定己方角色去重。五个随从行动指针与有限无守卫存储，不保证任意大量启用标志时安全。F64 配合 RAFT_MAP72 可独立声明 raft physical31；它不是 logical B31。

因此对话的 **B** 说话者是 FOLLOWER_B31，在所述连贯前提下通常是 Astral，但也可能是 Oddler 或其他所供别名。缺失 A/B 的 lookup0 别名到 physical0；隐藏255/损坏查找并非安全空操作。其 setPos 可移动主行动者。BattleLoop 已清 PLAYER_TYPE；变化后的 mode2 使用独立人口尾部。职业决定己方地图精灵；保留 CURRENT_BATTLE26 绕过死亡蓝焰分支。

SetPos、setFacing 与 startEntity 跳过实际死亡己方0–29。随从别名/NPC 绕过此 HP 守卫；行动安装、摇头、点头、priority、文本与 addNewFollower 不继承它。因此死亡/缺失行动者不会让整场统一跳过。Higins priority0 清其物理 priority 字节；Cloud 非零操作数存1。精灵链接消费者先输出非零条目，再输出零条目。这是绘制顺序输入，不是有符号层、交战优先级或已呈现重叠的证明。

## 营救与请求随军

**已确认：** 未中断脚本有72 个顶层命令、三个内联行动与五个压缩移动。下表覆盖完整序列；对话描述转述实际文本字面量2637–2657，而不将源码注释当作台词。未编码选择或目的地传送。

| 命令 | 相连场景与对话 |
| --- | --- |
| 0–12 | 设置游标2637；加载 Map72/列表；等待 Bowie Init、Higins motion-off/left-orientation 与 Peter Init；放置 Peter17,14/Right，等待 B Init，放置 B18,16/Up 与 A19,17/Up；设置 Higins/Cloud priority；淡入。 |
| 13–19 | Peter 移动 Down2 并面朝 Right。Peter 问 Higins 的状况；B 警告即将发生邪恶附身（2637–2638）。闪烁 B，闪屏，摄像机移至13,9。 |
| 20–29 | Cloud 滑出，放至18,13/Down，再滑入；Peter 面朝 Up。Cloud 移动 Up3，安装 speed64/64，异步开始盘旋；csWait200 后以等待完成的 Die 替换该行动，并将摄像机返回13,11。 |
| 30–40 | Higins motion-off/up-orientation，面朝 Down/摇头；他问发生了什么（2639），移动 Up1、跳跃，描述险遭附身（2640），再移动 Down2 并道谢（2641）。B 询问恢复与负伤旅程（2642–2643）。 |
| 41–51 | Higins 说明 Pacalon 战争、North Parmecia 兵力不足与南下寻找盟友（2644–2646）。Peter 移动 Up1/Right，说不必再远行（2647）。Higins 面朝 Left，问是否来自南方；B 指明 Granseal（2648–2649）。Higins 面朝 Down，请求去 Pacalon 帮忙（2650）。 |
| 52–61 | Higins 点头；Peter 提及对 Tristan 的责任（2651）。Higins 面朝 Left 恳求，接受以后有机会再去，随后自愿同行（2652–2654）。执行 join19。B 提及他的伤（2655）；Higins 面朝 Down，说感觉好些（2656）。 |
| 62–71 | 异步启动 Higins RotateRight，csWait40，以等待完成的 Jump 替换，面朝 Down，再 Jump，等待 Init，并 startEntity19。Higins 再请去 Pacalon（2657），随后 addNewFollower19 并终止。 |

对话请求表达意图，不是实际旅行。**推断：** 安排的云团离去与 Higins 陈述描绘了摆脱附身的营救。源码建立指令与文字，不证明云团完成可见盘旋，也不证明任何效果治愈疾病/状态。

即便概括交流，也需保留文本字面量边界：

| 实际 ID | 说话者 | 命令形式 | 最终文本控制 |
| --- | --- | --- | --- |
| 2637 | Peter | Single | W1 |
| 2638 | B | Single | W1 |
| 2639 | Higins | Single | W2 |
| 2640 | Higins | Single | W1 |
| 2641 | Higins | Single | W1 |
| 2642 | B | Text | W2 |
| 2643 | B | Single | W1 |
| 2644 | Higins | Text | W2 |
| 2645 | Higins | Text | W2 |
| 2646 | Higins | Single | W1 |
| 2647 | Peter | Single | W1 |
| 2648 | Higins | Single | W1 |
| 2649 | B | Single | W1 |
| 2650 | Higins | Single | W1 |
| 2651 | Peter | Single | W1 |
| 2652 | Higins | Text | W2 |
| 2653 | Higins | Text | W2 |
| 2654 | Higins | Single | W1 |
| 2655 | B | Single | W1 |
| 2656 | Higins | Single | W1 |
| 2657 | Higins | Single | W1 |

Single/Text 指 nextSingleText/nextText；Single 命令不蕴含 W1。全部21 个实际源码键为 `$0A4D..$0A61`。Higins 立绘控制为 $C0；Peter/B 用 $00。2655 的姓名操作数解析实际己方19 姓名，而非 physical actor19 或固定英文拼写。立绘/bleep 使用映射精灵/职业：Peter 基础/转职后以及 B 的实际别名均可不同；普通 HIGINS_PROMO 选择 Higins/bleep6。既有立绘窗口可影响立绘打开。

普通文本路径等待滚动，显示并增加游标。Single 还关闭窗口并 sleep10；Text 保留窗口。未中断游标结束为2658，独立于固定加入消息446。解释器入口清 skip 锁存；调试加 P2 Start 可设置。被跳过的 Single/Text 消费操作数，不显示、不增加游标、不关闭、不 sleep，而场景行动/效果/加入仍运行。负 csWait 字跳过 Sleep。场景中途 skip 留下前缀游标，不跳过加入的音乐/固定文本/输入等待服务。

## 移动完成前可替换行动

**已确认：** 等待行动形式轮询已解析物理行动者的行动指针，直至等于 Idle。异步形式安装后即返回。三个内联体以 `$8080` 结束，区别于地图脚本 `$FFFF`。压缩移动生成相对移动/目的地等待对，之后接 Idle 尾部。五个单移动程序从20 个压缩字节生成70 字节。移动使用实际位置和有符号瓦片增量*384。Peter 名义路径为17,14 →17,16，之后17,15；Cloud 为18,13 →18,10。Higins 的 Up1 名义抵达18,13；Down2 开始于跳跃之后，因此18,15 不是保证终点。

盘旋行动安装自身速度/加速度，覆盖 speed64。它经过三个相对路段，再分支到入口标签前的第四段，之后重复而无 Idle 终止符。csWait200 不等待该行动完成；Die 替换它当时的进展。Die 将四朝向轮换三次，请求死亡音效，改为 EFFECT1，并在 Idle 前停放63,63。它不杀死战斗员、不删除行动者：63*384 为 `$5E80`，低于非活动 X `$7000`。滑出/滑入同样只改变裁剪/DMA 请求而不删除行动者；隐藏存储仍可活动。

Higins motion-off 改变动画计数器，orientation 与 facing 不同。B 闪烁操作数60 展开为16 次迭代/64 次显式 VInt 等待；screen flash30 作四次淡入淡出调用与调色板复制，并非测得三十帧效果。摄像机请求等待滚动。摇头与点头通过等待/计数器写入包围精灵变换，不改 HP 或状态。

RotateRight 无限循环朝向/更新/等待指令。csWait40 不证明两圈；之后 Jump 替换它。Jump 使用定时相对移动请求，无目的地屏障即到 Idle。因此等待 Jump 不证明精确位移或运动已稳定。最终 Init 恢复移动因子/speeds32、阻挡、自动朝向与精灵设置，但不恢复 HP/状态或动画计数器。startEntity 仅在 Higins 存活时写 counter0。之后随从行动再次替换指针。所请求等待不建立已流逝表现时长、已完成 DMA/音频/输入服务或最终物理姿态。

## 加入、出战资格与临时跟随

**已确认：** 命令58 的 join19 清台词 SFX、等待滚动、选择普通加入音乐并调用 JoinForce19。它读实际职业，设对话姓名/职业索引，显示固定446 招募模板，再等淡入淡出/输入、关文本并 sleep10。这不是 operand128 Sarah/Chester 特殊路径。成员写入先于这些表现服务；后续对话/跟随要求它们返回。

JoinForce 设置加入 F19，重建升序加入/出战/预备列表，仅当重建出战计数低于12 时设出战 F51。满队不驱逐成员；已有出战资格者保持出战，预备成员有空位可成为出战成员。所供 F51 可使新加入成员在重建时出战。之后 JoinBattleParty 只设置标志：没有第二次即时重建来同步新激活列表/计数。

此处加入不初始化 HP/MP/职业/等级/物品，不移除状态、不清 AI、不修位置。死亡 Higins 可在死亡状态加入。他宣称恢复不补足缺失属性操作。F19 已设而526 未设时重放场景，仍重复调用/表现。[成员资格合同](../contracts/party-membership-state.md)保留标志/重建时序；[名册合同](../contracts/party-roster-state.md)区分表现与随从命令。

命令70 的 addNewFollower19 解析实际物理行动者，扫描 EXPLORATION_ENTITIES 至255。向 AddFollower 传最后随从物理 ID（空列表为0）、offset -24 与0。辅助先将42 字节 Follower1 行动复制到行走临时区，推进指针并修补目标/偏移，再测试行动者是否已在列表。重复成员仍消费临时区并替换行动；最后随从可指向自身。没有 HP、成员资格、出战计数、容量或安全映射守卫。不增加持久 Higins 随从标志/表行。安装关系为后续提供目的地/速度，不是传送或已证明最终跟随位置。

csc_end 可能等待滚动/清速度后，共享 after 尾部用表 byte0 调用 JoinForce0，在场景与持有526 两路均如此。它设 Bowie F0，重建包含已到达 Higins 写入的列表，容量未满时设 F32。不是第二次 Higins 加入。无场景/加入/随从命令改写 PARAM2–5 或修复之前返回种子。

## 完成、撤退与返回交接

**已确认：** 只有 after 包装器返回后，胜利才清426、设526。476 保持所供值，不写目的地城镇标志，清 d0–d3 字并复制 PARAM2–5 字节，返回 d4=1。clr.w 不建立长字高半部。名义胜利因此返回 Map72 与场景前成员派生坐标/朝向，不是 camera13,11、scene Bowie19,16 或 Pacalon。任意脚本/加入/服务/标志写入中断只保留已到达前缀，无回滚或完成返回声明。

| 其他终局路径 | 消耗与返回边界 |
| --- | --- |
| 败北 | 更新解锁；清 dialogue name1；请求悲伤音乐/text363/关闭；恢复 Bowie HP 至最大；无符号金币减半；选择 egress 位置；d4=-1。Battle04 特殊尾部不适用。 |
| 获准 Egress | 扣所选法术 MP 消耗；关窗口，text274 与通用音效/闪烁/关闭；更新解锁，退出行动栈帧并选 egress 位置；d4.w=0。 |
| 获准 Angel Wing | 在 text275 前移除所选行动者物品槽并重算属性；使用同一逃脱尾部/d4.w=0。两种逃脱均不增加败北金币惩罚/Bowie 恢复。 |

UpdateBattleUnlockedFlag 在526 未设时保留426，持有526 时清426。败北/逃脱不新设526、不改476，也不运行 after/加入场景。不撤销之前物品/掉落/队伍写入。后续探索治疗在终点外，因此已扣 MP/已恢复 HP 不承诺最终探索值。

无 Battle26 特定 egress 覆盖。新或续接 BattleLoop 入口设 F399 时，GetSavepointForMap 使用 **EGRESS_MAP**，而非胜利种子或所存 egress X/Y/朝向。Ketto25 选 `(12,11)` Up；New Granseal6 选 `(11,22)` Up；未匹配72 保留72 并默认 `(1,1)` Up。其他所供选择器用自身行/回退。这不证明教堂访问。辅助表写入是字节；不可将胜利清字合同施加到所有逃脱寄存器。

399/F64 均持有时，筏重置为 Polca/Bedoe 选72 `(43,48)`，Hassan 选69 `(28,55)`，其他包括选择器6/25/72 选69 `(42,15)`。64 未设则保留筏字段。直接从399 未设入口进入辅助，在筏重置前返回初始 Map3 `(56,3)` Down；这在普通已抵达 Battle26 前提外。这些写入既不建立登筏，也不建立探索准入。

全部返回抵达 MainLoop 的 alt_MainLoopEntry。之后调用 SwitchMap、ExplorationLoop，无 d4 符号测试或中间 CheckBattle。**本指南止于此返回交接。** 地图配置/人口、可用控制、之后移动、Pacalon/Tristan 抵达、另一场战斗与存档/读档持久性仍在范围外。续读 [Battle26 返回至有条件 Pacalon 入口](battle26-return-through-pacalon-entry.md)，了解新重建、保留滚动状态与独立城镇/战斗准入。

## 证据审阅与保留结果

合成前审阅检查完整已接受 #1013、更窄 Battle26 准入/初始化/激活所有者、共享生命周期/控制、行动/掉落、过场与队伍/随从合同，以及以下公共测试夹具载荷、schema 和实际消费者。复现固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`；USA ROM 身份与精确源码路径/地址保留在研究所有者。

| 已审阅测试夹具 / 消费者 | 已接受证明与更严格限制 |
| --- | --- |
| `sf2-battle-loop-static-v1`；[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py) | `_build_lifecycle_facts` 及其 fixture/schema 覆盖名册/治疗/死亡结构。通用正 HP 措辞不能替代选定非零 HP 指令测试；调试 kill-all 独立于首领追加。 |
| `sf2-battle-control-static-v1`；[battle_control.py](../../../../src/sf2tool/h2/battle_control.py) | `_build_control_facts` 保留入口/回合/胜利/败北顺序。精确终局优先级、状态处理与字节/字种子/返回连接来自已接受研究。 |
| `sf2-battle-cutscenes-static-v1`；[battle_cutscenes.py](../../../../src/sf2tool/h2/battle_cutscenes.py) | `_cutscene_facts` 覆盖 intro/after/defeated 包装器。完成门控败北脚本，而非共享追加尾部；通用重定位文字不创建 Battle26 表行。 |
| `sf2-enemy-item-drops-v1`；[H2 enemy_drops.py](../../../../src/sf2tool/h2/enemy_drops.py) | 源码/ROM 行比较与消费者验证，含30 行聚合公共测试夹具。聚合本身不标识行10 或实际调用方；所选26/130/24/10 连接仍由研究拥有。 |
| `sf2-enemy-item-drop-behavior-v1`；[H3 enemy_drops.py](../../../../src/sf2tool/h3/enemy_drops.py) | 十个受控 Battle01 入口用例与实际观察器比较，含已有标志、死亡/满容量行动者与 deals14/15。无自然 Battle26/Black Ring 奖励观察。 |
| `sf2-common-stats-static-v1`；[stats.py](../../../../src/sf2tool/h2/stats.py) | 队伍/列表与物品栏/特卖事实；数值容量、精确时序和选定场景调用方需要已接受源码连接。自动激活摘要不蕴含立即第二次重建。 |
| `sf2-map-script-engine-static-v1`；[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py) | forceStateCommandFacts/activePartyCommandFacts、精确形式/处理器守卫与源码身份连接。公共结构不提供私有 abcs_battle26 完整程序或实际文本字面量。 |
| `sf2-force-state-roster-death-runtime-v1` 与 `sf2-force-state-active-party-runtime-v1` | 既有14/九用例处理器矩阵保留有界成员/列表/随从行为。狭窄存档域证据不是自然 Battle26 招募、容量安全或持久场景跟随。 |

所属命令包括 `uv run sf2 h2 battle-loop`、`battle-control`、`battle-cutscenes`、`enemy-drops`、`common-stats` 与 `map-script-engine`；`uv run sf2 h3 enemy-drops`、`force-state-roster-death` 与 `force-state-active-party` 拥有各自受控观察。它们是复现路由，并非此设计检出中新执行的源码/运行时比较。更广所有者的静态问题队列与呈现/容量未知项不覆盖已接受选定源码分支。未发现阻塞证据矛盾；采用上述更窄声明，而非继承计数摘要。矛盾需要研究修正，不是 Layer B 修补或新采集。

[#1013 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1013#issuecomment-6182436816)与 [PR1016 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1016#issuecomment-6182452296)保留完整源码/程序/文本/行动者/事务审阅、最终 head12a6ab08/tree125e38de/baseee85e039、作者0.143s 与独立0.132s 检查、原始0.129s/0.128s 回执、原 CI38666515728 与变基 CI38666866738（scope16s/research33s）。未变18,285,979 字节配对的已接受0.083s 回执按溯源复用，无再生成或新比较。空白/include/掩码表与发布守卫诊断、前序失败与未知项仍在这些交接。

[#1014 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1014#issuecomment-6182346394)保留 schema/表/链接检查、CI38666201758（scope21s；其他任务跳过）、源码/配对缺失与已完成检查/工具诊断，包括所链 #1009/#1010 结果与修正。此处本地仍无源码与保留配对。不采集/复制私有数据，不声称新私有比较，也不重置已完成套件。

此原切片 Issue 交接记录直接完整文字/变化状态/diff、五路径/导航/链接/锚点/UTF-8/私有边界检查、schema 检查、git diff --check、已配置干净提交头 planner 与实际精确提交头 CI。仅文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4 与翻译套件。**未知：** 自然终局/状态/奖励、全部所供行动者/标志/HP/物品/地图/参数/映射/容量前提、已完成服务、行动进展、呈现、可用探索与持久性。这些不创建运行时队列，也不扩展已接受重制版前沿。
