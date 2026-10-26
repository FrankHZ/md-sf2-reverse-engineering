# North Cliff 打开与 Rohde 招募

> 本文是[规范英文原文：North Cliff Opening and Rohde Recruitment](../../synthesis/north-cliff-opening-and-rohde-recruitment.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

North Cliff 洞穴守卫准入队伍后，同时携带 Cannon 与 Dynamite 会选择不同于缺物警告的场景。调用方尝试各移除一个，再布置炮击、改变入口布局并让 Rohde 加入。Rohde 请求使用武器，但场景不提供玩家 yes/no 选择。加入不转移或装备物品栏 Cannon。

**已确认（静态）：**消费尝试、实时地图变更、成员资格、撤离、跟随与调用方标志是条件不同的有序效果。**推断：**通常彼此不同的阵容下，Rohde 打开 North Cliff 通道，并自愿使用炮参加战斗。**未知：**自然抵达、传入状态、表现完成、可用控制与存档/读档持久性。指南跟随完整场景、局部返回与重新地图后果，再指出下一入口传送。止于洞穴内部或 Battle25 准入之前。

## 证据与阅读边界

已接受 Layer A 所有者为 [North Cliff 打开与 Rohde 招募](../../../research/north-cliff-opening-and-rohde-recruitment.md)，经 [#985 / PR987](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/987)接受。它固定 USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` 与 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6)。源码路径相对 `disasm/`；Fnnn 表示标志 nnn。路线图拥有 [Standard 范围协议（英文）](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。英文规范源解释已接受证据，不增加原版发现或重制行为。

[Battle24 结果至 North Cliff 准备](battle24-outcomes-through-north-cliff-preparation.md)解释此前结果、探索返回、洞穴准入与缺物重试。已接受[物品栏搜索](../../../research/new-granseal-inventor-and-dynamite-gift.md)、[物品事务边界](../../../research/map-event-item-transactions.md)、[Caravan 消费者](../../../research/caravan-boarding-and-southwest-parmecia-travel.md)、[请求状态](../../../research/map-event-request-state.md)、[请求消费](../../../research/map-event-request-consumption.md)所有者提供共享机制。这里详细场景与被调用方事实消费 #985 完整选定审查。单凭公开结构夹具不证明私有场景、文本或布局画面。

## 检查两种物品，再重新搜索移除

**已确认（静态）：**Map72 洞穴回调先拒绝 F750 已设，再拒绝 F805 未设，再拒绝 F256 已设。其特定区域（事件）行覆盖 Y37、X25–30，位于 default 随机战斗行之前。特定回调拒绝不落入 default。实际步行/布局/请求产生仍是条件；仅站在坐标上不证明事件发生。

准入时清256，搜索 Cannon `$72`、再 Dynamite `$74`，各以完整结果字测试 `$FFFF`。任一谓词失败，已接受警告点名 Dynamite，均不消费。不以战斗完成、Astral 在场、HP、职业或出战容量测试补充守卫。

两次搜索成功后，调用方尝试移除 Cannon、尝试移除 Dynamite、运行 `cs_4FFDA`、设置750、再256。它忽略两个移除返回值。它传物品 IDs，不保留此前谓词的持有者或槽，因此每次移除重新搜索当前物品栏。

选定服务是 section07 选择的 `code/common/stats/iteminventory.asm:RemoveItemFromInventory`，地址 `$4F542`。跳转别名通向 section02 的 `itemstats.asm` 移除/属性链。同名 `stats/items/itemfunctions_s7_0.asm` 不是选定实现。

搜索按升序重建已加入军团 IDs，包括后备与死亡成员。每个持有者先计数四槽非 NOTHING 项，再从 count-1 降至0，以 `$7F` 掩码项匹配未掩码请求字。首个匹配成员与已检查最高匹配槽胜出。未加入持有者排除；空隙可隐藏高槽；空已加入列表缺安全初始循环守卫。成功返回持有者字节，不规范化整个 d0，并返回槽字。调用方完整字失败测试不能简化为有符号字节测试。

找到物品后，`RemoveItemBySlot` 读取完整槽字。NOTHING 返回 status3；否则 `RemoveAndArrangeItems` 将所有后续完整字左移，末槽填 NOTHING，返回0。Slot3 直接成为 NOTHING。没有诅咒物品或可装备性否决。移位的装备/损坏位随物品字移动。两条路径随后都按基础/状态及剩余已装备物品重算持有者属性，包括诅咒状态。即使 NOTHING 路径也更新。包装器把 status3 映为 -1，否则0，恢复保存的 d1/d2 字；不把移除持有者/槽作为装备交接返回。

物品栏紧密排列且不变、军团状态有效时，移除一个 Cannon 和一个 Dynamite，可能来自不同后备/死亡持有者。重复副本保留。Dynamite 在 Cannon 紧缩之后搜索。持有变化或后续移除失败时，只要调用返回，场景仍运行。首次移除后中断可只消费该物品。没有双物品事务、回滚、修复或自动重新获取。后续叙述称 LEADER 为赠予者，不识别实际持有者。

## 当前阵容加三个新分配

**已确认（静态）：**下文 A=FOLLOWER_A30、B=FOLLOWER_B31、P=Peter7、R=Rohde11、O=object128、E=effect129。这些是逻辑句柄。场景不重载 Map72，也不修复 Bowie/A/B/P。仅普通大地图标志65/67/70 且无冲突行时，physical0 是主行动者，A 映到 Caravan1，P 到2，B 到 Astral3。仅 F805 不保证这些行动者。其他启用随从行可添加行动者或替换 A/B 映射。

查找将句柄掩码为字节，对带敌人位句柄减96（128→32、129→33），读物理字节并乘32。清为0 的缺失条目别名 physical0；隐藏 `$FF` 不安全拒绝。别名可覆盖共享行动者目的地、朝向或脚本。逻辑 B lookup31 独立于木筏 lookup63/physical31。

场景首批命令无条件在 `(63,63)` 分配 R、O、E，R 朝 Up，O/E 朝 Left。没有既有映射、joined、HP 或容量守卫。分配器扫描 lookup0–62，取最大有符号物理字节加一，替换选定句柄映射。Lookup63 木筏排除。因此普通最大值3 产生 physical4/5/6；变化映射改变结果。重复调用可替换映射、遗留旧物理行动者，不复用或去重既有 Rohde，也不保证无限容量。

声明按384 单位瓦片设置位置/目的地，归零速度、行程与加速度，安装异步 Init 并请求属性刷新。R 的 `GetAllyMapsprite` 在战斗外 HP0 时先选 Blue Flame；否则未加入11 选 NPC_Rohde170，已加入11 用实际职业/表，包括 BRGN22→sprite26。其他传入职业走同一算法。O/E 用 OBJECT4=181、EFFECT3=178。后续 join 不刷新 R 当前地图精灵，因此立绘/台词仍可使用更早精灵。

Init 请求 speed32/32、零加速度因子，关闭加速/减速，开启阻挡/碰撞与自动朝向，Up，关闭浸没，size24、刷新与 Idle。其异步执行区别于后续替换脚本。场景显式等待 P、B、A Init；停止 O/E 动画并等待关闭自动朝向脚本。Stop/start 改 animation counter -1/0，不改移动启用。这些事实不证明所有行动者开始时安定，或所有精灵服务完成。

## 布置与炮击

已接受程序有114 条顶层记录（含终止符），覆盖 `$4FFDA..$5023E`/612 编码字节。[完整命令清单](../../../research/north-cliff-opening-and-rohde-recruitment.md#complete-scene-program)说明每条命令与内联主体。以下连接阶段保留有意义顺序，不把编码字节当作玩法时长。

**已确认（静态）：**场景设普通文本游标3341，准备上述行动者。摄像机请求 `(21,33)`。A、B、P 接受不等待目的地 `(27,39)`、`(28,37)`、`(25,37)`；Bowie 接受 `(26,37)` 并等待位置。目的地写行程与幅度32 有符号速度。Bowie 抵达及随后固定60-tick 脚本等待不汇合另外三者抵达。别名可覆盖旧目的地。

O 放在 `(27,36,Left)`。Bowie、P、B 朝 Up；50 ticks 后 B 朝 Down，在3341 呼叫 Rohde。Rohde 回答3342。R 放在 `(27,39,Right)`，接受等待的 `eas_4536C`：沿当前朝向移一瓦片、resize、传送声音、sizes12/14/16/18/20/22（各刷新/wait1）、关闭 resize、global size16、刷新、目的地等待与 Idle。3 ticks 后 R 朝 Up，B 询问3343/3344，R 回答3345。

R 存活、布置获准且移动完成时，该脚本请求 `(28,39)`。R 随后的等待 Up1/Left2 请求 `(26,38)`。Bowie/P 朝 Down，R 朝 Up，Bowie 点头，叙述3346 命名 Dynamite 赠予者。R 等待的 Right1/Up1 随后请求 `(27,37)`。这些坐标是条件推导：死亡 R 可跳过有守卫布置/朝向，无守卫行动仍安装。SetPos 写位置/目的地/朝向并刷新，不清所有速度/行程字段。朝向守卫也区分存活 Bowie/P/R 与 B；点头自身无 HP 守卫，执行10/20/10 睡眠、变换精灵/DMA 与刷新。

Bowie/P/B 朝 Up，显示3347，即 Rohde 要求退后。摄像机请求 `(21,32)`。O 接受等待的 BumpDown：关闭自动朝向，相对 Down1/wait5、Up1/wait5、零位移，开启自动朝向并 Idle。这是从当时位置出发的定时后坐步骤；零位移不是恢复保存坐标。仅到 Idle 不建立 O 安定返回。

E 放在 `(27,35,Left)`。等待的自定义行动设 speed80/80 后 Idle。其后等待的 Up2 是带目的地等待的编译移动，有条件到 `(27,33)`。场景直接设 quake amplitude3，把 E 改成 FLAME1=171，带 VInt/刷新，启动动画、安装不等待 AnimSpeedx2，请求 BATTLEFIELD_DEATH sound116。40 ticks 后复制入口格，隐藏 E、设 quake0、等60 ticks。这些是声音/精灵/震动/显示请求，不是观察到的成功爆破。没有显式地图淡入淡出或调色板闪光命令。

实时复制宽一布局格、高两格：当前 RAM 格 `(0,0)/(0,1)` → `(27,33)/(27,34)`，使用64 列字行。`CopyMapBlocks` 复制布局字，命令设置两个 view-plane 更新位。它不写 ROM，也不直接复制 VRAM 瓦片。这里不建立精确画面/碰撞内容；重复调用读取当时可变源格。变化先于招募及后续 F750 写入。

移动区别贯穿全程：编译的四正方向行动在 Idle 前插入相对位移与目的地等待。Flags 掩码 `$9F` 清地图碰撞，bit5 未设期间绕过共享其他实体阻挡检查。九个内联主体有12 个移动；分配150 生成字节，独立于42 编码内联字节。ExecuteMapScript 在入场重置行动 scratch 游标。行动安装不受 HP 守卫，将解析物理索引写入 action-wait timer；等待形式轮询脚本指针至 Idle。不等待安装可在完成前被替换。等待 Idle 不普遍等于等待渲染动画或全阵容集合。

## Rohde 自愿参战并加入

复制后，B 等待 Up1 并说3348。40 ticks 后 R 接受等待 Jump 并说3349。P/Bowie 朝 Right，B 朝 Left。P、Bowie 接受不等待 JumpLeft，B 接受等待 JumpRight。普通 Jump 使用 Up1/Down1，带5-tick 等待；侧跳使用对角偏移、6-tick 等待，再零位移、开启自动朝向并 Idle。偏移使用当前位置及实际 flags/阻挡行为。等待 B 不汇合 P/Bowie 行动，也不证明精确恢复。

R 被显式重新定位六次，每次后等25 ticks：`(26,36,Right)`、`(27,37,Up)`、`(28,36,Left)`、`(27,37,Up)`、`(26,36,Right)`、`(27,37,Up)`。R 说3350，朝 Left，问3351/3352，再说3353。随后 Join，再3354。他的问题是对话，整个程序没有 YesNoPrompt 或按回答分支。

**已确认字面量上下文：**完整普通游标为3341–3356。B 通常代表 Astral，但实际当前 B/R 地图精灵提供立绘与台词。Astral209 给 Astral/bleep6；NPC_Rohde170 或 sprite26 给 Rohde/bleep5；替代精灵使用实际行/default。既有打开立绘可绕过打开，speech 仍查找。旁白无说话者，speech0。源码措辞与替换如下：

| 文本 | 上下文 | 字面措辞 |
| --- | --- | --- |
|3341|B，单段|`{NAME;11}, {NAME;11}!{W1}`|
|3342|R，单段|`Yes?{W1}`|
|3343|B，连续|`You know about devices like{N}this, right?{W2}`|
|3344|B，单段|`{LEADER} has a cannon and{N}some dynamite here.  Can you{N}remove those rocks?{W1}`|
|3345|R，单段|`Ah...OK, let me try.{W1}`|
|3346|无说话者，单段|`{NAME;11} gets the dynamite{N}from {LEADER}.{W1}`|
|3347|R，单段|`Stand back.  Ready?{W1}`|
|3348|B，单段|`Perfect!  We can now go to{N}North Parmecia.{W1}`|
|3349|R，单段|`Wow!  This cannon is great!{W1}`|
|3350|R，单段|`I can attend the battles{N}with this weapon.{W1}`|
|3351|R，连续|`{LEADER}, do you need{N}this any more?{W2}`|
|3352|R，连续|`May I use it?  Huh?{W2}`|
|3353|R，单段|`Wow, now I can fight along{N}side you!{W1}`|
|3354|R，单段|`Please call me when you{N}need my help.{W1}`|
|3355|B，单段|`{NAME;11} is hooked.{N}Ha, ha!{W1}`|
|3356|B，单段|`OK, {LEADER}.  Let's go{N}to North Parmecia!{W1}`|

`{NAME;11}` 使用 Rohde 当前名字。LEADER 重建军团，命名首个出战成员，独立于实际物品持有或映射物理 Bowie。没有 ITEM 替换。N 为换行。W1 在 VInt 后接受按住方向或 A/B/C，不接受 Start；W2 使用继续/输入与确认音效。未跳过命令等待滚动、显示并递增游标。单段文本额外关闭立绘/文本并睡眠10；连续文本保留打开。这些等待推进文本，不回答 Rohde 问题。

Join 时，`csc08` 等待滚动、清 speech、请求 MUSIC_JOIN、调用 `JoinForce(11)`。它设置 joined11、重建军团列表，再在出战计数不足12 时经 JoinBattleParty 设置 active43。12 或更多时跳过写入，不清既有43。已加入后备 Rohde 有空间可成为出战；已出战 Rohde 即使传入超额状态也保持出战。辅助程序 UpdateForce 先于 active 写入，没有第二次重建；后续 UpdateForce 刷新缓存列表。

Join 不测试 HP/职业、不复活、治疗、初始化职业/物品栏/装备、不逐出成员、不转移 Cannon、不设 follower84，也不改当前映射。随后读取 R 实际职业并显示446：`{NAME} the {CLASS} {N}has joined the force.` 名字/职业索引为11 与该职业。即使 Rohde 已加入也重复。

选定 `FadeOut_WaitForP1Input` 请求当前之后播放先前音乐，等待 CURRENT_PLAYER_INPUT：按住方向/A/B/C/Start 即可。既非调色板淡出，也非新按键边沿或 yes/no 等待。它关闭文本、睡眠10，返回时普通游标仍为3354。成员资格、宣布完成与后续出战使用是独立边界。

ExecuteMapScript 入场清文本跳过锁存。启用 debug 的 P2 Start 可锁存：普通文本绕过显示，**不递增游标**；csWait 睡眠也跳过。因此部分跳过可令后续未跳过文本落后于名义3341–3356 序列。Join 没有普通文本跳过检查：音乐、成员资格、446、输入等待与 Sleep10 仍执行。点头/缩小/跳跃内部睡眠、移动/Idle/目的地等待、复制、跟随与标志保留。跳过不是取消，也不保证完成。

## 撤离使用物理 Caravan 状态

**已确认（静态）：**3354 后，O 接受等待的关闭自动朝向，再不等待 Down2。R 接受等待 Down1，再不等待 Down1。3 ticks 后 R 接受不等待 `eas_452BA`，再隔3 ticks。O 接受不等待 Down1，等3，接受同一缩小脚本，再等3。O 第二移动可覆盖首次。间隔不证明抵达。

缩小脚本等6、启用 resize、请求传送声音，依次 global sizes22/20/18/16/14（各刷新/wait1），再12/刷新，等待目的地，并从 **physical1** 克隆位置/目的地/行程/朝向后 Idle。它不解析逻辑 A，也不禁用 resize。变化 physical1 可独立于当前 A 映射行动者改变撤离。隐藏 R/O 前没有最终等待任一缩小脚本。

Hide 将物理位置/目的地设为 `$7000,$7000`，将所有匹配查找项改成 `$FF`，通过移位直到哨兵值，移除首个匹配探索列表条目。它不清行动指针、行程/速度、HP、物品栏、成员资格或随从缓冲分配。别名共用隐藏；因替换映射而遗孤的旧行动者独立。关键是普通 VInt 更新在有符号 X >= `$7000` 时跳过实体数据与行动分发。因此保留隐藏指针不会在该更新中继续缩小/克隆，直到位置重新合格。不保证精确最终 global size、缩小完成或安定离场。

隐藏 R/O 后，B 等待 Down1/Left2，说3355，朝 Down，说3356。Bowie/P 朝 Down，Bowie 接受不等待 Jump，P 接受等待 Jump。P 等待同样不建立 Bowie 行动完成。

最终跟随安装 A→Bowie slot2 偏移 `(-24,0)`、P→A slot1 `(0,-24)`、B→A slot3 `(0,24)`，从 A 分支。处理器存活守卫读取随从字 `$001E/$0007/$001F` 的高字节，因此**三者都测试 Bowie0 HP**，不是命名随从。Bowie 死亡时，在此前无守卫工作后跳过三者。当前别名可覆盖脚本或产生自跟随；不移除无关随从。

每次获准 AddFollower 复制42 字节，写物理目标与偏移，然后才仅对探索列表追加去重。三个命令消费126 字节，无容量守卫或安定等待。ClearEntities 重置步行缓冲；ExecuteMapScript 独立行动 scratch 重置与 hide 不回收。直接重复场景即使行动者已在列表，也可消费更多缓冲；成员资格/标志自身不令重复安全。

## 局部返回、中断前缀与重新入场

**已确认（静态）：**到达顺序为实时布局复制 → joined11 与条件43 → 撤离/跟随 → unlock425 → 场景返回 → 调用方750 → 256 → 区域（事件）physical0 位置等待 → 条件控制。`setStoryFlag25` 加400 设置425，既不启动 Battle25，也不设置 completion525。

场景终止符在对话打开时有条件等待滚动，清 VIEW_SCROLLING_SPEED 并恢复寄存器。区域（事件）包装器关闭立绘/文本，等待 VInt 与 physical0 位置，不汇合所有行动者/行动。ProcessMapEvent 在分发前清待处理；WaitForEvent 仅待处理为零时恢复 camera0 与模式选定控制器。待处理覆盖、模式与服务未完成可改变接续。场景/调用方均不存档，也不证明可用控制。

中断前缀保留已达效果。行动者出现前物品即可被移除；震动/火焰可先于复制；750 未设时入口可已变化；撤离或调用方锁存前成员资格即可写入；425 先于750，750 先于256。即使256 未写，750 已设也可抑制后续洞穴回调。没有回滚或修复，后续标志不认证更早请求表现成功。

重新 Map72 加载重新读取图块/布局，再由唯一 F750 flag-copy 记录重建 `(27,33)/(27,34)`，来源为重载 `(0,0)/(0,1)`。实时场景则复制当前可变格，显式设置视野更新位。重新加载器使用普通地图显示/平面初始化。任意实时编辑后，坐标相同不保证内容相同。

显式重新入场清临时256–383，清实体、查找与缓冲，再重建随从/主行动者。Map72 有一个 default 配置、无固定居民；不重新创建场景 O/E。Rohde 重新随从行测试 **84**，独立于 joined11、750 或425。84 未设时，已加入 Rohde 不在该人口；84 已设、11 未设时，他仍可按当前 HP/职业/joined 精灵选择声明。场景、join 与调用方从不写84。

F65 选择大地图 Caravan/A，继续进入普通随从表；依次测试67 Peter、70 Astral/B、84 Rohde 及冲突 A/B 行。启用行支配 physical1 起分配与当前查找覆盖；main0 独立。成员资格不替代这些剧情谓词。木筏/模式例外保留准备指南边界。这里不验证任意行动者计数或缓冲容量。

Map72 init 仍先测试716，再 joined-Luke10，仅716 已设/10 未设时运行 Luke 场景。不重放爆破，也不用750 修复行动者。无战斗 CURRENT255 重载加载布局/init，保留行动者与256；边界滚动保留行动者/锁存，跳过 init/重置。仅步行或获取物品不清256。重新入场清256，但750 仍抑制洞穴回调。中断后750 未设时，重新入场可允许对剩余物品栏重试，不恢复任一已消费物品。

下一静态路线为传送 `(27,33)` → Map48 `(55,29)` Up，采用无滚动语义。场景不直接传送，也不证明自然抵达/跨复制格事件产生。洞穴内部、Battle25 准入/名册与后续旅行仍是独立范围；原切片并行的 #989 不是本指南证据。

接续 [North Cliff 入场至 Battle25 准入（英文回退）](../../synthesis/north-cliff-entry-through-battle25-admission.md)，了解独立请求、精确坐标/解锁准入、重新己方/敌人初始化及裁切/视野配置。该指南止于敌人激活之前；上述打开既不保证旅行，也不保证准入。

| 变化状态 | 须保留后果 |
| --- | --- |
|750 已设 /805 未设 /256 已设|有序早期返回；直接场景调用绕过调用方守卫。|
|重复、有空隙、后备/死亡或未加入持有|实际已加入/受计数限制搜索选择一个副本；后续移除重新搜索。|
|移除失败或两次之间中断|后续工作可继续；保留已消费/紧缩前缀，不恢复双物品。|
|R 已加入/已映射，或直接重复场景|仍新分配/重映射并宣布加入；容量有条件。|
|当前句柄缺失、隐藏或别名|无安全拒绝/修复；目的地、脚本、隐藏与跟随可影响共享/非预期条目。|
|R 死亡或职业变化|精灵/有守卫位置/朝向不同；无守卫行动与 join 保留。|
|Bowie 死亡、P 存活|全部最终跟随守卫跳过；早期无守卫场景效果保留。|
|出战计数11 对12+，或既有43|条件 active 写入、不逐出/清除与缓存列表顺序仍区别于 joined11。|
|逻辑 A 不同于 physical1|最终跟随与缩小克隆用不同目标。|
|部分跳过或行动/输入/滚动服务未完成|普通游标可落后；join/输入/内部等待与已达状态工作保留。|
|复制/join/425/750/256 后中断|不同当前/持久前缀，无表现/存档保证。|
|750 已设、84 未设；或84 已设、11 未设|打开重建、重新跟随与成员资格相互独立。|
|CURRENT / 滚动 / 重新72|布局、行动者、锁存与分配器生命周期不同。|

**未知：**自然可达/事件/路线与精确格画面；传入标志、物品栏/排列、HP/职业/成员资格、模式/映射/行动者顺序及分配器容量；文本/输入/音乐/声音/精灵/DMA/滚动/行动与跟随完成，可见爆破/撤离/安定，可用控制与存档/读档持久性。这些是保留限制，不是自动运行时队列。

## 证据审查与保留结果

完整已接受 #985 所有者已与 #986 准备指南共同审查。选定公开 [map-events fixture](../../../../tests/fixtures/h2/map-events-static-v1.json)及组合加载器保留29 操作洞穴回调、六个洞穴行引用与有序守卫/搜索/移除调用/场景/标志。[物品事务 fixture](../../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)锚定选定服务别名与 `$4FECC/$4FED4` 调用位置。调用方记录不证明被调用方紧缩/属性语义。[Map-init fixture](../../../../tests/fixtures/h2/map-init-static-v1.json)保留六操作716/Luke 路径，不是自动重放爆破。

[地图脚本 fixture](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、schema 与实际验证器/加载器消费者保留命令家族/处理器事实，包括实际守卫与 follow 打包字节访问及语料摘要，不公布详细私有场景。完整114 命令/612 字节核算、150 生成行动字节、字面量、选定移除/join 主体、隐藏 X 更新、重新复制与 follower84 消费 #985 已接受选定源码/数据对审查。结构计数/schema 成功不是这些细节的新独立证明。未发现范围内矛盾；实质矛盾返回研究。

所属命令仍为 `uv run sf2 h2 map-events`、此前缀下的 `map-event-item-transactions`、`map-init`、`map-script-engine`，以及 Layer A 链接的保留数据对配方。已接受边界下，原综合环境缺少源码/数据对。选定公开 schema/消费者检查与正文审查不是新比较。原切片不采集、不重新生成、不运行时观察、不安装，也不新建维护工具。

[#985 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/985#issuecomment-6180719770)与 [PR987 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/987#issuecomment-6180735932)保留 head `02991c0d`、tree `8c4eca33`、base `e96f6faa`、成功 CI38653869752（scope15s/research33s）、作者0.082s 的不变18,285,979 字节数据对比较、114 命令/612 编码字节/150 生成行动字节。初始 LF-only 断言在语义检查后失败；仅规范化笔记 CRLF 后，限定检查通过。猜测源码路径、错误命令记录显示键及物品夹具 `expected` KeyError 已改为实际路径/字段，未改证据。直接 VInt 审查修正草稿隐藏指针执行暗示；X=$7000 暂停普通更新。这些已完成诊断继续保留。

[#986 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/986#issuecomment-6180738037)、[rebase 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/986#issuecomment-6180773526)与 [PR988 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/988#issuecomment-6180781344)保留原始 `f7c38893`/tree `a4e11c72`/CI38653916276（scope18s）、最终 `a8c678d5`/tree `3e30b70f`/base `1238d4d5`/CI38654343691（scope32s）、字节一致五份文档与530 链接。Ignored scratch 检查的任意 Markdown 长度上限已移除，未缩短正文。准备指南的[保留审查](battle24-outcomes-through-north-cliff-preparation.md#证据审查与保留结果)保留 #981/#982 身份、数据对/CI 结果、字面量/字段/opcode/阻挡修正及 #977 Map68 P2，以及链接前序失败与未知项。已完成套件/失败不重置，也不为替代回执而重跑。

原 #990 Issue/PR 交接记录完整正文/差异/导航/链接/锚点、UTF-8/私有/精确五路径检查及 `git diff --check`、同进程配置后的干净已提交规划与实际精确 head CI。文档范围使用直接检查，不要求全套 normal/full/Python/.NET/Godot/H3/H4/翻译套件。独立验收与集成仍归 main-gate。
