# Battle26 准入与 Geshp 对峙

> 本文是[规范英文原文：Battle26 Admission and the Geshp Confrontation](../../synthesis/battle26-admission-and-geshp-confrontation.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

**已确认：** 获准的新 Battle26 可在战斗初始化之前布置 Geshp 与 Higins 及两名 Pacalon 士兵的对峙。场景表现 Geshp 的残忍、他对 Higins 身体的觊觎，以及 Granseal 军团的打断。场地行动者与表面死亡不同于交战记录、HP、阵营和招募。

**推断：** 这是 [Ketto 交互与商人搬移](ketto-interactions-and-merchant-relocation.md)的候选接续。**未知：** 从 Ketto 自然旅行、实际传入标志与队伍、已完成服务、可见时序、可用控制与持久性。本指南解释已接受源码行为直到 before-battle 包装器返回，止于新治疗、编队、战斗加载与 start 之前。

证据所有者为完整已接受 [Battle26 准入与 Geshp 对峙研究](../../../research/battle26-admission-and-geshp-confrontation.md)。其固定源码为 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`，对照该处记录的 USA ROM 身份。以下源码路径相对该固定版本的 `disasm/`。Fnnn 表示标志 nnn；索引从零开始。对话 ID 标识 `data/scripting/text/gamescript.txt` 实际字面量；场景程序为 `data/battles/entries/battle26/cs_beforebattle.asm:bbcs_26`。正文转述对话，保留完整有意义内容与说话顺序；精确控制/字面量由研究所有者保留。

## 从 Ketto 出口到选定遭遇

**已确认：** Ketto 唯一出口传送行匹配通配 X255/Y31，请求无滚动 Map72、名义目标 (31,27)、Down。不测试购物、F800、胜利 525 或解锁 426。普通步行仍需符合传送布局类型 `$1000` 与位置。坐标行不证明出口可达。

生产者写可变 type 1/PARAM1–5 与传送声音。消费在分发前清事件类型、消费/清声音、令物理行动者 0 Idle 并退出探索。无滚动消费者采样位置、替换非负目标低字节、应用朝向并返回 d4=0。名义 (31,27) 假定普通采样高字节。替换请求或中断消费者可改变结果。已接受[请求消费者](../../../research/map-event-request-consumption.md)拥有此接缝。

MainLoop 运行 SwitchMap 再 CheckBattle；Map72 无已接受标志切换替代。CURRENT255 经 CURRENT_MAP 解析。Map72 完整候选顺序为：

| 候选 | 解锁 / 完成 | 输出裁切 X,Y,宽,高 | 触发 X,Y |
| --- | --- | --- | --- |
| Battle8 | 408 / 508 | 1,32,32,30 | 255,255 |
| Battle24 | 424 / 524 | 1,32,32,30 | 255,255 |
| Battle26 | 426 / 526 | 28,0,36,32 | 255,255 |

CheckBattle 选择首个已解锁匹配行并写其裁切。矩形是输出，不是空间包含门槛。若持有所选行完成，选择器清解锁，**仍返回该战斗**，不继续扫描。F426 不优先于持有的 408/424；持有 526 不抑制已解锁 26。无行获准时，MainLoop 进入探索。

之后 Map72 的 X32/33/34、任意 Y zone 选择辅助 26。步行 zone 生产要求布局类型 `$1400`，写 type 6/坐标字；首个匹配行胜出。被拒绝的具体回调不顺落到默认。其他洞穴行与 F523 的默认辅助 8/24 选择由[前序路线](battle25-outcomes-through-ketto-entry.md#独立洞穴出口与-map72-准入)保留。

| 辅助 26 状态 | 结果 |
| --- | --- |
| 526 未设 | 立即请求；不测试计数器或抽 RNG。 |
| 526 已设，STEP_COUNTER 非零 | 拒绝。 |
| 526 已设，计数器零，范围 8 抽取零 | 请求。 |
| 526 已设，计数器零，范围 8 抽取非零 | 抽范围 4 + 2，存计数器 2–5 并拒绝。 |

请求设置 426，在 MAP_EVENT_TYPE 写 `$000100FF`（type 1、PARAM1=0、PARAM2=CURRENT255），存 STEP_COUNTER=30000。PARAM3–5 保留传入值。之后才等待滚动并请求 boost 声音/闪光。写入不依赖表现完成。之后扫描仍可先选 8/24。F800 和胜利 525 既不设 426，也不替代辅助 26 的 526 测试。zone 坐标与 Ketto 完成都不证明实际穿行。

## 新入场、持有介绍与传入继续

**已确认：** BattleLoop 在测试 88 前清 PLAYER_TYPE 并设 399。88 未设时，清秒数、保存传入地图/战斗字、存 CURRENT_MAP=72/CURRENT_BATTLE=26、设基础 VInt 并调用 before 包装器。这些是更早调用者效果，不同于场景局部命令。

| 钩子 | 实际槽 26 选择 | 标志行为 |
| --- | --- | --- |
| Before | `$47ABC` 的相对字、表基址 `$47A88`，解析到 `$4B958` 的 `bbcs_26`。 | 检查 476；持有则跳过。Before 不设 476。 |
| Start | `$47B60` 的相对字、表基址 `$47B2C`，解析到 `$47A4E..$47A50` 的 `ms_Empty`。 | 检查 476；未设时先设置再分发。 |

位移有符号，相对各表基址。[路由合同](../contracts/battle-cutscene-routing.md#共享-intro-门)拥有共享介绍规则；场景特定目标证明属于已接受研究，不由公开汇总路由计数提供。持有 476 跳过两个脚本，但不跳过全部后续新初始化。

88 已设时，BattleLoop 恢复保存秒数、清 88/AI、设视野目标 0、加载传入当前战斗状态并跳到单次行动循环。不赋值传入 72/26、不重建对峙、不运行 before/start，也不进行新治疗/编队/区域重置。之后跳转 @Start 不可达。此继续不是通过新 before 返回终点的第二条路线。

## 场景演员与开场布置

**已确认：** 程序先设文本游标 2596，以摄像机 (2,20) 加载 Map72，并重建场景实体。加载/淡入淡出序列等待淡入淡出、在加载前后禁用/重新启用显示/VInt，并等待一个 VInt。这不是 ExplorationLoop 人口、治疗、临时标志重置或地图配置 init。摄像机坐标不布置 Bowie，加载不改变战斗 26。

加载器清行动者与 64 字节查找，从物理 1 声明符合随从，再声明场景列表，最后在物理 0 声明主角/Bowie。列表声明 Peter、Higins 和七个普通 NPC 精灵。即便数字像战斗员 ID，它们也是**逻辑场地选择器**：

| 逻辑行动者 | 名义声明 | 场景角色 |
| --- | --- | --- |
| Bowie 0 | (7,25), Right；初始 Idle | 主角；接收显式等待式 Init。 |
| Peter 7 | (6,26), Right | 固定己方行，除非现有随从查找去重；接收等待式 Init 及之后 setPos。 |
| Higins 19 | (18,14), Up | 被 Geshp 质问的 Pacalon 领袖；声明不要求已加入。 |
| Geshp 128 / GESHP 159 | (18,11), Down | 质问、折磨并离去的顾问。 |
| Demon 129 / LESSER_DEMON 109 | (17,11), Down | 逼近第二士兵，并对打断作反应。 |
| Bishop 130 / DARK_BISHOP 145 | (19,11), Down | 已声明随员；无独立对话或移动命令。 |
| Lizardman 131 / LIZARDMAN 83 | (20,10), Down | 接近第一士兵。 |
| Cloud 132 / EVIL_CLOUD 106 | (63,63), Down | 初始屏外，之后布置在 Higins 附近。 |
| 第一 centaur 133 / CENTAUR_MALE 233 | (17,15), Up | 维护 Higins、接近敌方并经历第一次表面死亡。 |
| 第二 centaur 134 / CENTAUR_MALE 233 | (19,15), Up | 反抗侮辱并经历第二次表面死亡。 |

列表行动为 eas_Init。固定己方按现有非零己方查找去重；随从别名不去重。令 N 为实际先前随从数；Peter 不在该查找中时 P=1，否则 0。需新分配时 Peter 行为 N+1，Higins 为 N+P+1，NPC 128..134 占 N+P+2 至 N+P+8。公式不验证任意传入标志，也不保证随从容量。

开场显式等待 Bowie/Peter/FOLLOWER_B Init，将 Peter 放 (6,26)、B 放 (6,24)、A 放 (6,25)，均 Right，再请求 fadeInB 和摄像机速度 48。FOLLOWER_A (30) 与 FOLLOWER_B (31) 标识当前别名，不是在每种传入状态中都对应指定姓名。F65 可加 Caravan/A；B 可来自 Astral 或 Oddler 行。其他已接受随从行包括 A/King 或 Old Man 及重复 B/Astral；重复声明单独分配却覆盖查找。每张随从表仅五个行动指针，无计数守卫。过多同时标志不建立支持的演员表。

例如仅一致标志 65/67/70 时 N=3,P=0：A 物理 1、Peter 2、B 3、Higins 4，NPC 128..134 为 5..11。这是分配示例，不证明实际传入此状态。缺失 A/B 查找字节保持 0，可别名到 Bowie。脚本查找掩码逻辑字节，对高位 ID 减 96，并使用所得物理字节，不验证缺失/损坏。`$FF` 不是安全空操作。重建通常移除旧 Ketto 映射，但重复随从与后续查找变更仍有影响。

F64 加 RAFT_MAP72 可独立在物理 31 声明已保存木筏，保存坐标用 `$7F` 掩码。新 PLAYER_TYPE=0 排除模式 2 覆盖，不清这些保存事实。物理 31 与逻辑 FOLLOWER_B (31) 是不同命名空间。

实际职业选择己方地图精灵。死亡蓝火分支要求 CURRENT_BATTLE=255，因此新 26 即便零 HP 也绕过；这不复活任何人。setPos、setFacing、stopEntity 跳过实际死亡己方 0..29。别名 30/31 与 NPC 绕过守卫；行动安装、shiver、查找与文本无对应 HP 守卫。死亡 Peter 可接收 Init 却跳过 setPos；死亡 Higins 可跳过朝向/stop，却仍接收 shiver 与最终内联行动。

## Geshp 质问 Higins

**已确认源码序列，命令 11–26 / 对话 2596–2606：** Geshp 要求回答，问 Higins 去哪里。摄像机移至 (13,9)。Higins 迟疑；Geshp 问他是否想去 South Parmecia 求援。Higins 再次结巴。Geshp 称 Pacalon paladins 为懦夫，直呼 Higins，质疑其自尊并称他窝囊。

第一 centaur 在一条等待流中 Up 2 再 Right 1，名义 (17,15)→(17,13)→(18,13)，然后朝 Up。他让 Geshp 闭嘴，称 Mr. Higins 为世界上最勇敢士兵。Higins 说自己能辩护；Geshp 以杂种辱骂回答。Geshp 朝 Up，问是否害怕他，背过身让懦夫逃跑。士兵执行等待式 Jump，宣称 Geshp 太过分。

这些问题无 YesNoPrompt 或选择分支。对话描述对峙，不提供逃离选择，也不变更勇气、疾病或阵营。名义接近以声明原点和正常推进为前提；相对运动消费者使用实际位置。

## 第一士兵的表面死亡

**已确认源码序列，命令 27–56 / 对话 2607–2608：** 士兵接收等待式成对速度 48，再开始异步 Up 3。csWait 5 后，Geshp 接收 MOSAIC_OUT (6)；解释器显式等待士兵 Idle 指针，等待 5，开始其异步 Right/Left/Right/Left 张望，再等待 120。该张望是行动脚本，不是 headshake。

Lizardman 朝 Left，士兵朝 Down。等待 20 后，Lizardman 接收等待式速度 64，并开始异步 Left 1。等待 3 后，士兵接收等待式速度 64。Quake 2 伴随等待式 JumpLeft；随后 quake 0，再另一次等待式 JumpLeft。士兵喊叫（2607），执行等待式 Die。两次 JumpLeft 不证明精确水平终点。

摄像机移至 (13,13)；Geshp 被放到 (18,19)、Up，并接收 MOSAIC_IN (7)。他谴责 Pacalon 士兵在他背身时攻击、不讲荣誉（2608）。第二 centaur 执行等待式 Jump；Higins 与该士兵朝 Down。等待 30 后，第二士兵在等待流中 Left 1 再 Down 1，名义 (19,15)→(18,15)→(18,16)。

第一士兵 Die 播放场景死亡行动并停放于 (63,63)。不写战斗员 HP/状态/死亡列表或结果。Mosaic 请求也不删除 Geshp。表面消失与死亡保留以下场地分配和服务边界。

## 第二士兵挑战恶魔

**已确认源码序列，命令 57–77 / 对话 2609–2615：** 第二士兵难以置信，说若 Mr. Higins 没病就能打败 Geshp。Geshp 问 Higins 是否病了，再邀士兵自己挑战。士兵说再也受不了侮辱；Higins 两次反对。

Demon 129 被放到 (13,18)、Right，接收等待式速度 80；士兵接收等待式速度 40。士兵开始异步 Down 2，再 Demon 开始异步 Right 4。等待 20 后，士兵新等待式速度 64 程序未经 Idle 汇合就替换早先流。Quake 3 伴随等待式 JumpRight；随后 quake 0，再第二次等待式 JumpRight。士兵呻吟（2615）、执行等待式 Die，Demon 朝 Down。Geshp 在等待流中 Up 2。

Demon 与士兵异步运动可重叠。等待 20 或后续程序替换均不证明两条原路径到达最终瓦片。第二次表面死亡具有与首次相同的场景限定边界；Higins 疾病仍是口头背景，不是可执行状态写入。

## Geshp 身份与对 Higins 身体的觊觎

**已确认源码序列，命令 78–101 / 对话 2616–2628：** Geshp 评论 Higins 的悲伤。Higins 认出他不只是普通恶魔士兵。Geshp 同意、承认 Higins 领导 Pacalon Army，并自称 Zeon 顾问 Geshp。Higins 问他如何知道自己姓名；Geshp 自称无所不知。文本 2622–2623 组成 Higins 要求解释为何仍留他活着的质问。Geshp 大笑，称他聪明（2624）。

Cloud 132 仍在传入屏外位置时接收 SLIDE_OUT (4)；Geshp 接收 flashWhite 128,30。摄像机移至 (13,11)，Cloud 放到 (18,12)、Down，接收 SLIDE_IN (5)。Higins 问 Geshp 做了什么（2625）。Cloud 在等待流中 Down 1，并接收 SLIDE_OUT (4)。Higins 说身体不能动（2626），再接收 shiver 与 stopEntity。

Geshp 说来借 Higins 身体，并欢迎他们的新领袖（2627–2628）。第二次 stopEntity 后是等待式 Higins 内联程序：motion OFF、orientLeft、updateSprite、跳到 Idle。stopEntity 与 motion OFF 都禁用动画；两者都不是通用速度停止。orientation 位不同于 facing。对话与视觉序列支持附身威胁；命令不创建附身、麻痹、敌方阵营或成员状态，也不招募 Higins。

## Granseal 打断与 Geshp 离去

**已确认源码序列，命令 102–129 / 对话 2629–2636：** FOLLOWER_B 喊 Geshp 住手（2629）。名义返回队伍背景可使其为 Astral，但实际操作数 B (31) 可解析 Oddler、其他传入精灵或缺失查找别名。Peter 与 Bowie 都没有独立台词；其场地行动者属于开场布置。

Demon 接收等待式速度 32 并开始异步 Jump。Geshp 执行等待式 Jump，摄像机移至 (13,15)，Geshp 在等待流中 Down 3。等待 5 后，他开始异步张望，再等待 120；然后在等待流中 Left 1。Geshp 问谁在说话（2630）。摄像机返回 (2,20)、等待 60，再移回 (13,15)。

Geshp 质疑南方军团怎会在此，问其来自何处（2631–2632）。B 回答来自 Granseal（2633）。Geshp 开始未说完的反驳，说 Granseal 在另一边（2634）。程序等待 Geshp 的 DeactivateAutoFacing 行动、安装等待式速度 48，并在等待流中将他 Right 2。

Geshp 决定该离开（2635）、朝 Up，命令恶魔士兵阻止 Granseal 军团（2636）。他朝 Down；Demon 开始另一次异步 Jump；Geshp 接收 MOSAIC_OUT (6)，脚本结束。无最终汇合等待 Demon 的 Jump。Geshp 离去是视觉请求，不是删除、战斗胜利或控制授予。

开场与五个对峙阶段保留全部 41 个对话 ID 2596–2636 及有意义的有序行动序列。研究所有者[完整有序程序](../../../research/battle26-admission-and-geshp-confrontation.md#complete-ordered-program)保留详尽 130 命令表与源码宽度计量；精确编码细节沿此阅读，不再复制另一张编码表。

## 等待、移动与效果的实际含义

**已确认：** 等待式行动安装和 waitIdle 轮询行动者行动指针是否等于 eas_Idle。依赖实体服务推进，本身不是目标测试。打包移动流向实际当前位置加有符号瓦片增量×384，从存储速度导出行程/速度，在移动之间等待位置=目标，再跳 Idle。安装这些流清 FLAGS_A 位 5/6；位 5 原控制阻挡辅助。

eas_Init 配置速度 32/32、清加速度因子、禁用加速/减速、启用实体阻挡、禁用实体/地图可穿透设置、启用自动朝向、orient Up、清浸没、设共享大小 24、更新精灵并到 Idle。后续程序速度 48/64/80/40/32 改成对速度字节，不是距离或帧数。替换行动流可放弃更早目标序列。

Jump 临时禁用自动朝向，请求相对 (0,-1)、(0,1)、(0,0)，计时器等待 5/5。JumpLeft 用 (-1,-1)/(-1,1)，JumpRight 用 (1,-1)/(1,1)，等待 6/6。各段之间不等目标完成：每次新请求用当时已到达位置。等待式行动返回仅确立该次轮询的 Idle 指针，不确立精确瓦片位移或已观察时长。张望交替 Right/Left/Right/Left，每次后等待 30；任意计时器/服务状态下，等待 120 不汇合它。DeactivateAutoFacing 是 OFF 再跳 Idle；该入口之后的字节不可达。

setPos 写位置/目标/朝向并请求精灵更新，不清速度/行程或替换行动。setFacing 写 facing/更新。stopEntity 写 ANIMCOUNTER=-1。Higins 的 motion OFF 使用动画禁用消费者；orientLeft 在显式精灵更新前改 orientation 位。这些区别在变化状态或中断行动推进后仍有意义。

Die 禁用动画、循环 Left/Up/Right/Down 三次，带显式更新与十一次等待 3，发送战场死亡音效、将地图精灵改为 EFFECT1、循环 Up/Left/Down 并更新/三次等待 8、设位置和目标 (63,63)、清行程并跳 Idle。不写 HP、状态、成员资格或战斗结果。`$5E80`（63×384）仍低于 `$7000` 非活动 X 阈值，因此停放的两名士兵和初始 Cloud 仍被分配、可参与实体更新。

摄像机目标设 view target-1 并等待视野滚动；速度写 VIEW_SCROLLING_SPEED=48。Quake 2/3/0 直接写振幅，无渐变位。正常路径含两次零清除；在非零写入后中断未必清除。Mosaic 6/7 执行 16 次掩码/加载/DMA 迭代，每次三次 VInt 等待。Slide 4/5 执行 23 次裁切/变换/DMA 迭代，每次两次等待；slide-out 再加最终全掩码 DMA 请求。都不移除/搬移行动者、不清行动指针，也不停止其更新。

flashWhite 128,30 产生八次设置/更新/等待/清除/更新/等待迭代，含 32 次显式 VInt 等待。Shiver 临时用共享大小 21/counter-1，运行三次标志位 3 开/关循环，每次 Sleep 5，再恢复保存大小/计数器；无己方 HP 守卫。这些是有序请求与有界源码循环，不是测量总时长、DMA/音频完成或保证渲染结果。隐藏 Geshp 或 Cloud 仍可接收其他精灵更新。

## 对话名称、窗口与跳过状态

**已确认：** `{NAME;19}` 读取己方 19 实际战斗员名称。不查物理场景行动者 19，也不保证未变英文名 Higins。研究比较实际十六进制键 0A24..0A4C，不仅看对话注释。[对话合同](../contracts/dialogue-system.md)拥有名称、命令、窗口与独立已到达文本等待边界；这些观察不是本场景新运行时证据。

nextText 保留窗口；nextSingleText 显示后关闭立绘/文本并 Sleep 10。每次普通调用准备立绘/台词、等滚动、显示实际字面量并递增游标。从 2596 开始，全部未跳过调用返回后留下 2637。宏选择不替代字面控制：2611 是带 W1 的 nextText，2622 是尾随换行且无等待控制的 nextText，2635 是带 W2 的 nextSingleText。其余 nextText 为 2600、2609、2618、2627、2631；其余全部 nextSingleText。W1/W2 确认不创建选择。此程序无选择操作码或 YesNoPrompt。

Geshp 用立绘控制 `$00`，其余说话者用 `$C0`。立绘/提示音解析遵循实际逻辑查找、物理地图精灵与精灵对话属性。名义 Geshp 和 HIGINS_PROMO 选择各自命名立绘/提示音 6；CENTAUR_MALE 无立绘/提示音 6。B 可选 Astral/提示音 6 或 Oddler/提示音 4；缺失属性使用默认立绘/提示音 6。变化职业/精灵/别名改变该上下文。已打开 PORTRAIT_WINDOW_INDEX 可阻止打开新立绘，即便台词再次解析；单凭逻辑名称不证明渲染立绘。

ExecuteMapScript 入场清 SKIP_CUTSCENE_TEXT；调试模式加 P2 Start 可在命令循环设置它。被跳过 nextText/nextSingleText 消费说话者操作数，却不显示、不递增游标、不关窗口或 Sleep 10。负 csWait 操作数同样跳过 Sleep。加载、运动、效果和摄像机命令仍执行。中途跳过只保留已完成游标前缀，并可改变异步重叠；不证明等效时序或保证 2637 游标。

## 直接写入与返回边界

**已确认：** 局部场景无剧情设/清标志、JoinForce、HP/状态变更、战斗员创建、结果、物品栏/付款、存档、地图事件请求或选择/分支。早先准入可设/清解锁，BattleLoop 设 399；before 场景不设 476 或完成 526。它通过上述消费者改变场地人口、位置、脚本/速度、动画/orientation、摄像机/淡入淡出/效果/文本字段。无回滚。

csc_end 在有开放对话窗口时有条件等待滚动，清 VIEW_SCROLLING_SPEED、恢复解释器寄存器并返回。before 包装器恢复 d1，在 loc_47AE8 返回。尾部不恢复全部行动者或速度/效果、不移除场景分配、不汇合 Demon 最终异步 Jump、不初始化交战，也不授予玩家控制。

下一已接受调用者边界是[新 BattleLoop 初始化](../../../research/battle26-admission-and-geshp-confrontation.md#before-dispatcher-and-exact-endpoint)：只有此返回之后，调用者才恢复/重新赋值保存地图/战斗、重置区域标志 90–105、治疗存活/不死伙伴、先初始化己方再敌方位置、清 AI、为战斗员 0 LoadBattle 并调用 start。场景不建立初始战斗员/中立名册、已激活敌人、单次行动、结果或 Higins 招募。

[Battle26 初始编队与中立 Higins](battle26-initialization-and-roster.md)从此返回继续解释实际队伍/敌人初始化、物理替换、完成门控的中立 Higins 与空 start 钩子。止于 ActivateEnemies 紧前，保留传入状态、升级、查找、服务与表现限制。激活、单次行动、结果与招募仍是独立范围。

| 变化状态或中断 | 必须保留的后果 |
| --- | --- |
| 更早 408/424 持有或 426 缺席 | 请求 Map72 未必选 26。 |
| 526 持有 | 选择器解锁清理仍返回所选战斗；辅助 RNG/计数器门控是不同操作。 |
| 请求被替换，地图/布局/模式变化 | 无无条件 Ketto→26 路线。 |
| 476 持有 | 跳过 before 人口/对峙；仍有之后新初始化。 |
| 88 已设 | 加载传入继续，无此场景或新终点。 |
| 随从/A/B 别名变化，Peter 存在/缺席 | 实际映射、去重与容量支配演员表。 |
| 己方死亡或职业变化，查找缺失/损坏 | 精灵选择与逐命令守卫不同；零可别名到主角。 |
| 原点/速度/计时器变化或运动中断 | 相对目标/异步重叠不同；新脚本可替换旧脚本。 |
| Die 完成或 Cloud 在 63,63；精灵视觉隐藏 | 已分配行动者在非活动哨兵以下继续更新。 |
| 已有立绘或中途文本跳过 | 窗口、台词和游标不同；非文本命令仍执行。 |
| 请求/加载/速度/quake/效果前缀中断 | 更早写入保留，无服务完成或回滚。 |
| Before 包装器返回 | 不建立名册、start 标志、单次行动、胜利、控制或 Higins 加入。 |

## 证据审查与保留结果

完整已接受研究已对照有序程序、全部九个声明/主角头、全部 41 个字面项与变化状态矩阵审阅。分阶段解释保留两名士兵、实际 A/B 别名、职业/HP/查找区别、异步替换、仅停动画、跳过游标和直接写入限制。选定既有公开表面已连同 schema 和维护消费者检查：

| 表面 | 已审阅所有者与公开限制 |
| --- | --- |
| 准入/辅助 | `sf2-battle-global-data-static-v1` 与 `sf2-map-event-random-battle-state-static-v1`；全局 factSummary 不是私有 Map72 行，辅助规则不证明穿行。 |
| Before/start 路由 | `sf2-battle-cutscenes-static-v1` 与 `sf2-battle-routing-data-static-v1`；共享介绍检查/写序公开，但 routeTargetsParsed=false 不证明槽 26 目标。 |
| 加载/人口 | `sf2-map-init-static-v1` 与 `sf2-map-script-engine-static-v1`；生命周期、人口和布置消费者不建立自然演员表或交战名册。 |
| 行动/摄像机/效果 | 选定 map-script action-bridge、lifecycle-presentation、gesture、camera 和 FX 事实；等待与场地写入不证明动画/表现完成。 |
| 请求/对话 | `sf2-map-event-request-consumption-static-v1`、`sf2-map-event-dialogue-state-static-v1` 与对话合同；可变请求和传入文本上下文不证明交付/控制。 |

[战斗验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)、[路由验证器](../../../../src/sf2tool/h2/battle_routing_data.py)、[全局数据验证器](../../../../src/sf2tool/h2/battle_global_data.py)、[随机战斗消费者](../../../../src/sf2tool/h2/map_event_random_battle_state.py)、[地图脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)、[地图 init 验证器](../../../../src/sf2tool/h2/map_init.py)、[请求消费者](../../../../src/sf2tool/h2/map_event_request_consumption.py)与[对话状态消费者](../../../../src/sf2tool/h2/map_event_dialogue_state.py)拥有这些夹具及 schema。聚焦所有命令为 `uv run sf2 h2` 配合 `battle-cutscenes`、`battle-routing-data`、`battle-global-data`、`map-event-random-battle-state`、`map-script-engine`、`map-init`、`map-event-request-consumption` 与 `map-event-dialogue-state`。

公开验证与正文审阅**不是私有行、场景目标或字面量的新比较**。接受原设计环境缺少源码/数据对。不由此进行采集/重生成/运行时/环境变化或新维护机制。未发现范围内矛盾；源码分歧返回研究，不在 Layer B 修复。

[#1001 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181719337)与 [PR1004 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1004#issuecomment-6181778859)保留 head `1ac04fb9`、tree `dab707d3`、base `af894656`、CI 38661497541（scope 11s/research 34s）、原 CI 38661384831、0.082/0.080s 保留数据对检查及全部 130 命令/9 实体/41 字面量连接。夹具布局与 HTTPS 检查修正仍记录，并保留[已移除 ignored Markdown 上限](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181773472)。

[#1002 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1002#issuecomment-6181676037)与 [PR1003 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1003#issuecomment-6181691009)保留 head `1d73f70d`、tree `f5a58e2a`、CI 38661190951（scope 15s）、八 schema 加组合 map-events/539 链接、不可用源码/数据对及已完成诊断修正。[Ketto 保留审查](ketto-interactions-and-merchant-relocation.md#证据审查与保留结果)随各所有者保留更早行动者 0 等待、库存索引修正、已完成失败和未知项。无已完成套件被重置。

#1006 Issue/PR 交接记录完整正文/diff/导航/链接/锚点、UTF-8/私有边界/精确五路径检查、git diff --check、已配置干净提交 planner 及实际精确 head CI。文档范围不启动一揽子 normal/full/Python/.NET/Godot/H3/H4/翻译套件。自然可达、传入状态、服务完成、渲染时序/控制及持久性仍为**未知**，不产生自动采集队列。独立验收与集成仍由 main-gate 负责。
