# Creed 恢复与主房间离开

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

**推断：**Creed 的法术让出战队伍脱离微型困境；Peter 随后询问 Zeon，Creed 和 Fairy 离开去咨询 Evil Spirit。**已确认：**选定源码按顺序安排该对话、精灵工作与离开，然后标记场景完成并选择 Hassan 供后续 Egress 使用。**未知：**自然抵达、传入行动者与状态、动画/服务完成、可见恢复、可用控制及持久性。所覆盖故事止于主房间，早于任何地下室访问或招募。

本指南从 [Battle23 结果与返回 Willard Burrow](battle23-outcomes-and-burrow-return.md)继续，依据 [#946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/946) 及 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。它消费已接受[恢复研究](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md)并解释条件结果。Fnnn 表示标志 nnn；命令和居民索引从零计。下文故事名字假设一致的逻辑至物理行动者映射。示例区分源码分支，不报告已游玩场景。

## 返回为场景准备的房间

**已确认：**洞穴既有两个前往 Creed Mansion33 的出口请求 `(15,9)`、Right：Y0 任意 X，或 X21 任意 Y，首匹配胜出。均不测试 F523。准入仍需符合条件的布局、请求消费者与加载服务；显式目的地字节仅替换坐标低字节。它们不修复任意继承高字节，也不证明自然走过洞穴。见前篇[完整退出边界](battle23-outcomes-and-burrow-return.md#既有出口仍是条件请求)。

显式 Map33 抵达执行新探索加载：清待处理事件类型、调整步数计数器、治疗符合条件的存活/不死己方角色、请求淡出、禁用视野跟随，并设置 CURRENT_MAP33/CURRENT_BATTLE255。它加载瓦片集、重建行动者/地图精灵、清临时标志256–383、设置 F80，并在调用 init 前加载完整地图。下述标志不受临时重置影响。传入 CURRENT255 入场则保留实体群和工作布局，更新主角/木筏属性；不会创建新七行房间。场景不改变 PLAYER_TYPE；一致的 Battle23 前篇提供步行模式0。其他传入模式/木筏状态保留 [Creed 入口所有者的限制](../../../../../research/chapters/southern-travel-creed/creed-mansion-entry-and-first-goliath-scene.md)。

配置选择扫描全部覆盖，保留**最后已设置**的一项：

| 选择顺序 | 实体群与 init |
| --- | --- |
| 默认 | 基线六居民；普通 init |
| F523 | 七名返回居民；普通 init |
| F783 | 基线六居民；普通 init |
| F784 | 七名后期居民；普通 init |
| F786 | 三名后期居民；仅返回的替代 init |
| F22，最后 | 基线六居民；普通 init，也覆盖 F786 |

返回前提为 F523 已设置且 F783/F784/F786/F22 未设置。F774 既不选择该配置，也不门控恢复。每个配置访问器依当前标志重新选择；其间标志改变可使实体群与 init 不一致。四个指针表共享事件、描述和 Section5。已接受[选择/实体群关联](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md#admission-setup-selection-and-population)保留精确表、分配与跳转别名。

该返回前提下，完整居民声明为：

| 行 | 逻辑身份 / 精灵来源 | 初始位置、朝向与行动 |
| --- | --- | --- |
| 0 | Goliath128 / WORKER202 | `(63,63)`、Up、eas_Init |
| 1 | Creed129 / CREED212 | `(19,6)`、Left、eas_Init |
| 2 | Fairy130 / FAIRY219 | `(63,63)`、Up、eas_InitSlow |
| 3 | Eric14 / 实际己方职业与 HP | `(63,63)`、Down、eas_Init |
| 4 | Randolf16 / 实际己方职业与 HP | `(63,63)`、Up、eas_Init |
| 5 | Tyrin17 / 实际己方职业与 HP | `(63,63)`、Down、eas_Init |
| 6 | Karna15 / 实际己方职业与 HP | `(63,63)`、Up、eas_Init |

停放 `(63,63)` 保留已分配行动者与查找，不是招募或隐藏。既有非零己方查找跳过整个居民行，不测试已加入/出战。职业和 HP 选择实际己方地图精灵；战斗外死亡己方角色可用蓝焰。探索治疗不复活所有死亡己方角色，虽然例外包括 Peter 和 Lemon。

随从在居民前从 physical1 分配，Bowie 为 physical0。普通随从选择器中仅有 F67/F68 时，Peter7 与 B31/Oddler 占 physical1/2，Goliath/Creed/Fairy 占3/4/5，Eric/Randolf/Tyrin/Karna 占6/7/8/9。这些槽位有条件。其他选择器移动槽位；F74 Astral、F68 Oddler、F70 Astral 的重复 B 行不去重，最后一行覆盖 B 查找。缺席 Peter 或 B 留查找0，别名到 Bowie。F69 的 FOLLOWER_C Fairy 不同于 resident130。Map33 不在大地图列表，因此 F65 不在此添加大地图 Caravan 前缀。仅成员资格不提供场景 Peter，也没有 newEntity 回退。

改变标志的影响不止场景重复。基线有 Goliath、Creed 及相同四名己方角色，均停放，无 resident130。F784 为 Goliath `(17,14)` Up、Creed `(17,5)` Down、Fairy `(17,10)` Up 和四名停放己方角色。F786 则为行走 WORKER `(22,4)` Up、原点 `(22,4)`/范围1，CREED `(17,5)` Down 及 ODDLER `(17,7)` Up。其名称易误导的 ms_map33_flag312_InitFunction 仅为 rts。F22 可覆盖该替代 init。普通 init 独立于后期标志测试 **F523 已设置且 F783 未设置**：可在 F784 或 F22 实体群下运行恢复，不验证演员。缺席 Fairy130 可别名到主角，后续 hide130 因而可隐藏错误行动者。

## Goliath 的收藏与 Creed 听到的声音

**已确认源码序列，命令0–51 / 文本1840–1854：**Creed 呼叫 Goliath，再大声呼叫。Goliath 应答、靠近并称呼他。Creed 评论喧闹收藏；Goliath 犹豫，承认增加收藏。Creed 的怒气由 shiver 与 quake3/0 对布置。Goliath 道歉，解释新增者跳过魔法门；Creed 指责他没锁门，重申锁门命令。随后要求看新增者，Goliath 答应去取并向 Down5 移动。

准备工作有实际意义。起始文本游标1840；reloadMap `(12,2)` 复用此地图工作布局与行动者，不初始化新房间，也不使出战队伍变小。之后按顺序将 Bowie、Peter、B 朝 Down 停放；淡入与等待40 先于 Creed 首次呼叫。Creed 初始 Down1/Left2 相对于当前坐标。Goliath 显式放置 `(17,14)` Up，摄像机发往 `(12,4)`，其 Up2 先于 stepEvent `(17,11)` 和 Up3。摄像机目的地不同于行动者布置。

战斗外，该踩踏事件请求从 `(26,0)` 至 `(17,11)` 的单字当前布局复制，外加门声与平面更新，不写持久标志。完整标志事件表仅含 F22 独立25x32 复制；F783 不重放开门。复制请求不确立可见开门、碰撞清除或未来穿行。Creed 等待的自定义速度48、Down1，及后面速度16/Up1、朝向与点头交流完成取回命令；Goliath 速度48 先于 Down5。操作数速度/睡眠不是实测像素速率或经过时长。

**命令52–88 / 文本1855–1867：**向上看并等待120 后，Creed 对声音作出反应，四下看、Down1/Left1，听到无归属插话。他再看，报告听到声音并 Left1。Peter 问他是否 Creed。Creed 跳起，认出收藏中有声音，认定新增者是 Goliath 的，并听 Peter 说他们来找他。Peter 请求恢复原来大小。Creed 再次惊讶他们找他并问原因；Peter 请求放大，抱怨脖子吃力。Creed 道歉，随后减速、禁用自动朝向、Up1，在间隔等待中 shiver 两次并念咒。

研究的[完整命令表](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md#complete-scene-and-embedded-actions)固定这些阶段及余下场景的每个等待、朝向和移动。24 个移动主体与六个自定义主体包含自身终止符；没有跳入下一程序 cs_5B466。六个自定义主体为等待的速度赋值，后接 idle 跳转/ac_end。移动使用实际当前坐标的有符号增量并检查目的地冲突，不假定前一故事终点已抵达。行动安装以解析物理索引初始化等待计时器。等待形式无超时地轮询该行动者指针至 eas_Idle，不等待无关行动者。RightLeft/UpDown 各执行四个朝向，每次精灵更新/等待30，然后 idle。Jump 禁用自动朝向，Up1/等待5、Down1/等待5，再相对0/0，恢复自动朝向并 idle。DeactivateAutoFacing 清该设置并跳 idle；无关后续行动字节不是其延续。

## 恢复法术的具体效果

**命令89–124 / 文本1868–1871：**四个操作数4 的白闪调用以等待40、30、20、10 分隔，后接 flash30。Bowie 置于 `(16,9)` Down 并获 TwirlGrow，然后依次为 Peter `(15,9)` Down、B `(17,9)` Down 赋相同行动。等待60 和5 后，Bowie 行动替换为 RightLeft；再等待5 后 Peter 获 UpDown，再等待5 后 B 获 RightLeft，随后等待120。Peter 朝 Right，说大家恢复正常并回忆变小。Bowie 点头两次并朝 Right，B 点头两次，Creed 询问目的。三人依次朝 Up，Peter 询问 Zeon，Fairy 在显式放置前对名字作出反应。

**推断：**这是叙事恢复。具体实现是地图精灵工作。TwirlGrow 启用该行动者 resize 位，再按以下十步各自执行朝向、共享大小、精灵更新和等待5：

| 朝向序列 | 同顺序共享大小 |
| --- | --- |
| Left, Down, Right, Up, Left, Down, Right, Up, Left, Down | 6,8,10,12,14,16,18,20,22,24 |

完整尾部禁用 resize、写共享大小18，并跳经 **eas_Init**。完成的 Init 设置因子0/0、速度32/32、禁用加速/减速、启用可阻挡、禁用实体/地图不可碰撞、启用自动朝向、Up 方向、禁用浸没、大小24、精灵更新及 idle。不重置坐标。因此大小18 是中间写入，不保证持久大小。InitSlow 使用速度16/16、因子1/1 和加速/减速 `-2,ON`，再走相同尾部；不证明实测半速动画。

SPRITE_SIZE 是共享临时工作值；resize 是解析所得行动者上的位。精灵消费者在相应分支适用时调整解压行走帧像素大小，绕过特殊精灵/entity32；精灵加载容量可推迟更新。这不确立战斗员身体比例、通用碰撞大小规则、属性恢复或持久的每行动者大小。三个 TwirlGrow 安装均**未等待**。其睡眠不在后续替换前等待全部行动者，并发行动者使用同一大小字。别名、行动停滞、加载压力或调试跳过可阻止相同渲染长大或最终 Init 尾部。此处没有显式 removeShadow 或独立恢复阴影观察。Peter 台词与点头不证明长大完成。

Flash 操作数也需消费者解释：右移3 加 DBF 使每个4 对应一次 LaunchFading，30 对应四次启动。各自选择全部调色板、最大值1 和 FLASH_QUICKLY_2，等待淡入淡出加一个 VInt，并恢复保存最大值；之后处理器复制调色板。四/三十不是帧数保证。Quake3/4/0 直接赋振幅。Shiver 同步保存大小/动画，使用大小21，三次切换 resize/update 并各 Sleep5，最后恢复保存值；不保留原先已设置的 resize 位。Nod 停动画、睡眠10、变换/DMA 精灵、睡眠20、更新、睡眠10，再设置 counter0。Headshake 用七次交替变换/更新循环和 VInt 等待，结束于 counter0。这些服务等待时其他行动可继续，不提供全局行动完成屏障。

## Zeon 问题转为更广泛危机

**命令125–147 / 文本1872–1879：**Fairy 被置于 `(16,0)` Down、Down7 靠近并重复 Zeon 问题。Creed 说他是封印于 Arc Valley 的 devil king。她 Down1，问他是否复活；Bowie 点头，叙述使用运行时首领名字。她难以置信地四下看。Creed 的 Init 和 Down1 先于让她朝 Down/自己朝 Right 并让她安静。Fairy 禁用自动朝向，未等待地启动 Right1，同时 Creed 的 Right1 被等待。Creed 朝 Down 问报告是否属实；Bowie 点头和第二次首领名字叙述回答。等待 Creed 不等待 Fairy 的并发移动。

**命令148–177 / 文本1880–1885：**Creed 减速、Up1，预言 Zeon 毁灭性的归来。Goliath 以 Up4/Right1/Up3 靠近、朝 Left 并跳两次，报告新增者失踪。Fairy 朝 Up，Creed 朝 Right；他说知道。Creed 朝 Down，Goliath Left1 并问他如何知道，Creed 转 Right、shiver 并赶他走。Quake4、Goliath shiver、quake0、再两次跳跃和 Right1/Down2 布置其反应。等待20 后，Fairy 的 InitSlow、Up1 及 Left 朝向引出危机回应；Creed 朝 Right。

**命令178–200 / 文本1886–1896：**Creed 报告 Grans 上空乌云与 North Parmecia 干旱，与 South Parmecia 对比。Fairy 归功于 Volcanon 并担忧 Mitula。Creed 说 Zeon 是他们不受伤害的例外，强调其力量。Goliath 朝 Up、shiver、禁用自动朝向、获得速度16，未等待地启动 Down1，对 Zeon 作恐惧反应。Creed 朝 Down，质问他为何还在。Quake4 伴随 Goliath 后面的 Init/Left1/Up 朝向，再 quake0。Creed 推迟训斥，转 Left 并提议咨询 Evil Spirit。

**推断：**交流将个人恢复请求扩大为共同忧虑和求知计划。说话者关于天气、Zeon、Volcanon、Mitula 的陈述是对话，不是独立确立的世界状态事实或新玩法规则。F68 下 B 意指 Oddler，但在此程序无文本。没有同意菜单或招募命令。

### 谁说话、等待什么、文本证明什么

已接受 [58 行文本表](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md#text-identities-and-immediate-consumers)将每个 ID1840–1897 关联命令、说话者/修饰符、含义转述与有序控制，以及 gamescript 键0730–0769。它是上述阶段的精确配套，含无归属1856 和叙述1875/1879。后两者使用 UpdateForce 后首个战斗队伍成员的 `{LEADER}`，点头命令则寻址 Bowie；传入成员资格不一定使名字一致。

Creed 至1880 使用修饰符 `$80`，1882/1884/1886–1888/1891–1892/1894 使用0，1895–1896 再用 `$80`。Goliath、Peter、Fairy 用0。`$80` 请求右侧、不镜像立绘；0 两位均不设置。立绘/SFX 查找使用实际映射地图精灵，现有立绘状态可抑制打开。仅 logical129 不证明 Creed 立绘。无归属/叙述 `$FFFF` 调用绕过实体查找并设置语音 SFX0。

nextSingleText 关闭立绘/文本并睡眠10；nextText 保留续文打开。续文调用为1849、1851、1859、1861、1863、1886、1887、1889、1891、1895。其 `{W2}` 输入等待包括续文箭头/确认声音路径；其余用 `{W1}`。`{N}` 控制换行，包括1892 的 `{N}{W1}` 及1868/1891 的双换行。两种等待均非选择。显示调用等待视野滚动，文本后推进游标。ExecuteMapScript 入场清调试文本跳过锁存，但调试模式/P2 Start 可设置：分发随后跳过 csWait，显示处理器跳过文本**与游标推进**。所有顺序文本含义均假设未锁存跳过；对话覆盖或足够的并发行动时间都非观察保证。

## 离开主房间并返回探索

**已确认源码序列，命令201–211 / 文本1897：**Creed 接收等待的 Init 和等待的 Left3/Up2/Left4 离开，再 hide。Fairy 接收等待的 InitSlow 和 Left1/Down1，告诉众人跟随 Creed，然后执行等待的 Up1/Left3/Up2/Left4 离开并隐藏。此前移动一致且完成时，两条路径均在 hide 前止于 `(9,5)`；未等待的 Fairy 步仍是前提。后续 Goliath Init 可替换其未等待完成的 Down1。这些相对路径不确立完成下楼路线。

Hide 写当前/目的地 `$7000,$7000`，使每个匹配查找条目失效，并在存在时将行动者移出探索。既不改变成员资格，也不写剧情标志或传送。它比停放 `(63,63)` 更强。Fairy 给最后指示时 Creed 已隐藏。

脚本随后先安装 Peter→Bowie，再 B→Peter，方向均2，映射为 `(-24,0)`。每个 followEntity 首先检查**编码随从字的高字节**：字7 和31 都提供0，因此**两者均检查 Bowie HP**。只有准入后完整字才解析行动者。这不同于 setPos/setFacing：后两者跳过零 HP 且小于30 的逻辑己方角色，但不跳过 NPC128–130 或 B31。行动安装、nod、headshake 和 shiver 无此 HP 跳过。因此死亡 Peter 可跳过布置但仍接收行动；Bowie HP0 可抑制两个跟随关系而不抑制手势。

AddFollower 安装复制的42 字节行动模板、设置目标/偏移，仅在行动者缺席时加入探索。执行的随从行动重置速度32/32 和移动标志，再通过目的地等待跟随。这些调用替换现有行动并建立关系；不招募、不瞬移、不等待稳定，也不设置持久随从标志。缺失或别名查找可引起自我跟随或替换另一个关系。

csc_end 在对话窗口索引非零时条件等待视野滚动，清滚动速度并返回，不等待每个行动者。普通 init 再完成以下有序尾部：

| 按顺序的守卫 | 结果 |
| --- | --- |
| F523 已设置且 F783 未设置 | 运行场景；**返回后**设置 F783，再写 EGRESS_MAP=Hassan9 |
| F22 已设置 | 将 Goliath 置于 `(14,14)` Up；绕过下面三个备选 |
| 否则 F784 已设置 | 跳过这些备选 |
| 否则 F783 已设置 | 将 Goliath 置于 `(17,10)` Down |
| 否则 F774 已设置 | 将 Goliath 置于 `(17,12)` Down |
| 独立尾部：F784 已设置 | 对每个未设置的已加入标志依次处理：F14 Eric `(15,14)`、F16 Randolf `(16,15)`、F17 Tyrin `(18,15)`、F15 Karna `(19,14)`，均 Down |
| 结束 | 选定小布置脚本之后返回 |

普通完成执行新 F783 的 Goliath 布置，跳过 F784 己方尾部。该尾部未设置的已加入标志独立于布置 HP 守卫：死亡己方角色不一定被放置。F783 不追溯重建行动者。既有七行房间保留，Creed/Fairy 显式隐藏；未来访问器可选择基线。新普通重复则重建基线、放置 Goliath，并跳过场景及 Hassan 赋值。传入 F783 不证明过去恢复，也不修复陈旧 EGRESS_MAP。F523 未设置同样跳过两个写入；其他后期标志未设置时，仅 F774 选择更早 Goliath 布置。F786 选定替代 init 绕过此尾部。

RunMapSetupInitFunction 恢复寄存器并直接返回，既不增加区域（事件）包装器的立绘/文本关闭，也不增加 physical0 停止等待。探索比较调色板、条件请求音乐/淡入淡出、设置移动声音并进入 WaitForEvent。只有 MAP_EVENT_TYPE=0 才选择摄像机 physical0 并安装实际 PLAYER_TYPE 控制器；待处理工作优先。场景不写传送/待处理事件，但不提供防止其他写入者的原子保护。

映射一致、Bowie/Peter 存活、服务完成，且无介入位移或待处理事件时，显式出战队伍位置为 Bowie `(16,9)`、Peter `(15,9)`、B `(17,9)`，Goliath `(17,10)` Down，Creed/Fairy 隐藏。队伍最后显式朝向为 Up；活动随从行动可改变它们。可用控制仍要求调用方和控制器完成。Hassan9 是后续 Egress 存档点查找选择器，不运输任何人，也不保存新 XYZ/朝向元组。

中断前缀不回滚。已完成布置、复制、效果或隐藏可在未到调用方写入时保留。F783 与 EGRESS_MAP 间中断可留下已设置完成但旧选择器；更晚失败可保留两个写入而无探索控制。均不确立存档持久性、地下室抵达、Evil Spirit 场景、招募选择或 Oddler 离开。本指南止于条件主房间探索交接。继续阅读 [Evil Spirit Shrine 入口与咨询准入](evil-spirit-shrine-entry-and-scene-admission.md)，了解独立房间路线、实体群与场景请求门槛。

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `2eddb2c6d00a748315d9a4671dca8429a8a1ea67` 包含 #941/PR944 和 #942/PR943。研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md#reproduction-and-review-boundary)保留源码符号/地址与精确命令配方。选定程序为 `$5B016` 的 cs_5B016，scripts.asm247–535，以自身 csc_end 结束于 cs_5B466 前。完整普通 init 为 `$5AB7C..$5AC56`；替代 init 为 `$5AC56` 的 rts。原英文切片不消费运行中的 #945 研究或未接受结论。

原英文切片的对抗审阅覆盖完整已接受恢复研究及前篇，经精确已接受关联检查各阶段0–51、52–88、89–124、125–147、148–177、178–200、201–211 的全部212 命令、全部58 条含义/说话者/控制行和30 个内嵌主体。检查七名居民身份、完整覆盖优先级、改变 HP/别名/标志/加载前提、长大的 Init 尾部、等待/替换区别、编码随从守卫和每个调用方布置/写入顺序。实际公共 fixture 载荷、schema 与验证器比较检查如下：

| 可执行所有者 | 选定证据与限制 |
| --- | --- |
| sf2-map-init-static-v1，tests/fixtures/h2/map-init-static-v1.json | Map33 的 expected.routeJoins/pointerTableJoins、两个 init 路径的 primarySourceBodies、每个布置的 embeddedPrograms。h2-map-init-static-fixture.schema.json 与 map_init.py verify_map_init_contract 比较完整预期语义合同与溯源。rts/init 顺序不证明控制完成。 |
| sf2-map-script-engine-static-v1，tests/fixtures/h2/map-script-engine-static-v1.json | expected.largestPrograms 包括 cs_5B016/212；对话、转换、布置、行动桥、生命周期、手势/关系及表现事实保留处理器顺序/操作数边界。map_script_schema.py 组合 schemas/h2/map-script-engine-*.schema.json；map_script_engine.py _verify_map_script_engine_fixture 比较字段。这不执行场景。 |

schema 在 schemas/，验证器在 src/sf2tool/h2/。原英文切片直接对两个公共 fixture 做 schema 验证。尤其保留的 followEntity 处理器先读字节再读完整字，flash 使用右移3/DBF；“随从”或“时长”等标签不能替代编码消费者。精确场景/文本/主体关联及未表示的更深服务仍属已接受研究。窄复现命令为 `uv run sf2 h2 map-init` 与 `uv run sf2 h2 map-script-engine`，配合研究保留的对比较配方。原英文设计切片检出缺少固定 upstream/对：已接受比较仍是保留结果，不是新重跑、再生成载荷或新运行时声明。

[#941 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802)保留0.080s 未改变对结果、精确源码/文本/主体关联、修正的1892 控制和编码随从 HP 解释、原始检查诊断及链接前篇失败。[#942 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/942#issuecomment-6177635072)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/943#issuecomment-6177652147)保留前篇检查、缺输入限制和早先失败记录。已完成失败仍为已完成；本指南不重启它们。

原英文切片执行直接完整正文/diff、阶段/文本/行动/顺序与改变状态审阅、链接/锚点、英文/私有边界和 git diff --check，然后检查已配置干净已提交 planner 及实际精确 head CI。不单因正文选择局部一揽子 Python/.NET/Godot/H3/H4 套件或翻译检查。

**未知：**自然退出可达性；实际标志、出战队伍/HP/职业/映射、模式/木筏、布局和待处理状态；文本、移动、精灵、摄像机、淡入淡出、音频和控制器服务完成；渲染长大/阴影/手势；可用控制和持久性。恢复/离开的**推断**叙述不填补缺口。[ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)与 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)不创建自动运行时队列。后续地下室/招募内容和重制验收仍在本指南外。
