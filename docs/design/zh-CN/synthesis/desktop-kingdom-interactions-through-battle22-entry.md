# Desktop Kingdom：对话与棋盘挑战

> 本文是[规范英文原文](../../synthesis/desktop-kingdom-interactions-through-battle22-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**King Ponpei 的最后一句战前台词返回后，他才授予靠近棋盘的许可。独立区域（事件）检查该许可并启动遭遇场景；其存续传送请求随后可准入 Battle22。**推断：**King 将打败其军队作为离开的条件。**未知：**自然行程、实际传入状态、已完成服务、表现与可用控制。单凭对话不能确立这些结果。

本指南从 [Desktop Kingdom 首次抵达](desktop-kingdom-first-arrival.md)的条件局部返回接续。仅使用
[已接受对话/准入研究](../../../research/desktop-kingdom-interactions-through-battle22-entry.md)，依据
[Issue #907](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/907) 和
[当前 Standard 约定](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
选定前提是 F522 未设置，每次交互都重新解析实际行动者映射。
F777 可以已经设置；F776 不作为这些普通对话的门控。

端点是**进入 BattleLoop、尚未进行战斗开场或名册工作**，或条件区域（事件）绕过/局部返回。
战后脚本、Cotton Balloon 获取/使用、Floor World、交战、奖励和退出均不属本指南。
保留 King 提及隧道的对话，但不引入后续内容。获准进入 BattleLoop 后，继续阅读
[棋盘初始化与初始名册](battle22-initialization-and-roster.md)；该指南使用已接受 #908 研究，
并将全新激活与恢复的回合（单次行动）留在其自身边界之外。

## 可以与谁交谈

**已确认：**默认 `ms_map22` 与 F522 选中的 `ms_map22_flag522` 共享实体及区域（事件）指针。
它们的实体群不同。配置访问器再次解析传入标志；F776 和首次抵达场景完成均不构成额外局部对话门控。
[抵达所有者](desktop-kingdom-first-arrival.md)负责实体群、随从分配、HP/职业、别名和木筏覆盖限制。
事件行不能保证行动者存在，也不能保证其在当前布局中自然可达。

普通行动调用方让 physical 行动者 0 空闲，并等待其移动及视野滚动。
A 选择场地菜单。C 可在调试和重叠 Caravan 替代分支之后选择附近行动者。
NPC128 的回调独立于该 Caravan 快捷入口。摄像机目标为 0 时，激活逻辑按玩家朝向偏移候选位置，
穿过桌子/柜台图块，并选择 Manhattan 距离小于一瓦片（MAP_TILE_SIZE=384）的首个合格 physical 行动者。
它跳过被观察行动者和当前随从。负视野目标直接返回，不选择行动者。这些是激活条件，不是步行行程。

选中的 physical 行动者通过 ENTITY_INDEX_LIST 反向映射；越过己方角色段后切换至从 128 开始的 logical ID。
首个反向匹配胜出；无匹配则回退至 logical 0。事件包装器将玩家朝向记录为 EVENT_RELATIVE_POSITION，
选择首个匹配的四字节行或默认行，并从映射后的行动者取得立绘/台词声音。
朝向操作时又将该 logical 身份解析回 physical 行动者。Logical NPC ID 和 physical 槽位是查找身份，不是战斗名册位置。

行的第二操作数是**行动位域**，尽管宏名称看起来像朝向：

| 行值 | 回调返回前后的操作 |
| --- | --- |
| RIGHT=0 | 既不转向，也不还原朝向。 |
| UP=1 | 一次 VInt 后，将行动者转至 `(playerFacing+2)&3`；没有 bit-1 还原。 |
| DOWN=3 | 如上转向，返回后再还原行动者保存的朝向。 |

这些值不要求从特定方向接近，也不描述初始朝向。包装器在回调之前禁用实体更新。
返回后，有条件地还原朝向，关闭立绘/文本，再启用更新。该清理不回滚服务变更，也不证明服务、文本或回调已经返回。

## 居民、Eric 与 Karna

**已确认：**完整表有 27 个显式行和一个默认行，选择 17 个具名处理器及默认处理器。
下文 Event N 表示 `Map22_EntityEventN`。示例 physical 槽位仅适用于前篇的 F67/F68 随从、
没有更早 Eric/Karna 别名且没有木筏覆盖的情况。其他传入随从或映射需要重新查找。

| Logical 行动者；基线精灵 | 示例 physical 槽位 | 行 / Event | 选定战前工作 |
| --- | --- | --- | --- |
| Eric14；己方角色精灵查找 | 3 | UP1 / 0 | 文本 1772,1773；返回。 |
| Karna15；己方角色精灵查找 | 4 | UP1 / 1 | 文本 1774,1775,1776；返回。 |
| NPC129；YOUNG_MAN196 | 6 | DOWN3 / 3 | 文本 1766,1767；返回。 |
| NPC130；YOUNG_WOMAN197 | 7 | DOWN3 / 4 | 文本 1768,1769；返回。 |
| NPC131；OLD_MAN198 | 8 | UP1 / 5 | 文本 1770,1771；返回。 |

年轻男子说 Creed 在自然灾难中救了他们。年轻女子描述魔族据说不能用魔法影响人类生命的规则。
老人描述一个没有时间、衰老或死亡的王国，并问那里生命的意义是什么。
**推断：**这些叙述解释了居民的处境。它们不确立魔法、不死或时间的普遍规则。

Eric 说灾难时他正在寻找传说中的剑，并讨厌这里。Karna 询问来访者，接受误入的解释，并建议拜见 King Ponpei。
**已确认：**两个回调都只发送选定文本并返回。均不招募、不写成员资格、不授予物品，也不证明先前自然对话曾发生。

## 车队与教堂服务

| Logical 行动者；基线精灵 | 示例 physical 槽位 | 行 / Event | 直接交接 |
| --- | --- | --- | --- |
| NPC128；CARAVAN62 | 5 | RIGHT0 / 2 | `j_CaravanMenu`；服务返回时才随后返回。 |
| NPC132；PRIEST192 | 9 | UP1 / 6 | `j_ChurchMenu`；服务返回时才随后返回。 |

**已确认：**Event2 直接调用 CaravanMenu，不添加局部 F65、出战队伍成员资格、载具模式或移动前提。
服务保存寄存器、分配局部栈帧，发送开场消息，然后进入 MENU_CARAVAN。
选择 -1 进入 ExitCaravan，发送退出消息、释放栈帧、还原寄存器并返回。其他行动返回服务菜单循环。
独立 C 按键快捷入口的缩小、音乐及视野更新工作不属于此直接回调。

Caravan 消息例程选择 **F70 未设置时 Rohde 立绘 11，F70 已设置时 Astral 立绘 31**。
该服务立绘不同于 NPC128 的分发器回退。更早场景在叙事上将其归为 Goliath，
既不改变 CARAVAN 精灵，也不把此回调变成宅邸的 WORKER 交互。

Event6 直接调用 ChurchMenu。它在条件开场中使用 CURRENT_PORTRAIT，显示欢迎文本，关闭窗口，然后进入 MENU_CHURCH。
选择 -1 进入 ExitMenu：条件立绘、告别、窗口关闭、栈帧/寄存器还原和返回。
其他选择仍由[通用菜单研究所有者](../../../research/common-menus.md)负责。

局部回调只确立这些有边界的交接与可能返回。它们不确立已完成存档、治疗、转职、仓储/出战队伍交易或服务后玩家控制，
也不添加局部 EGRESS/存档点写入。返回的服务到达实体包装器；未返回的服务不会。包装器清理不撤销服务变更。

## King Ponpei 与守卫

| Logical 行动者；基线精灵 | 示例 physical 槽位 | 行 / Event | 选定工作 |
| --- | --- | --- | --- |
| NPC133；MAID208 | 10 | UP1 / 7 | F522 未设置：文本 1777；返回。 |
| NPC134；KING_PONPEI188 | 11 | DOWN3 / 8 | 先测试 F522，再测试 F777；许可分支见下文。 |
| NPC135；GUARD206 | 12 | DOWN3 / 9 | F777 未设置：1783,1784；已设置：1782；返回。 |
| NPC136；GUARD206 | 13 | DOWN3 / 10 | F212 未设置：1802；无论 F258 如何均返回。 |

女仆推荐 King。**已确认：**King 的 Event8 先检查 F522。其已设置分支选择排除的战后文本 1797，
并在许可写入之前返回。F522 未设置时，由 F777 选择介绍或重复：

| King 回调运行时的 F777 | 对话顺序 | 文本返回后的状态变化 |
| --- | --- | --- |
| 未设置 | 1778–1780，然后共享 1781 | 1781 返回后设置 F777；返回。 |
| 已设置 | 仅 1781 | 1781 返回后再次设置 F777；返回。 |

首次对话询问出战队伍为什么未能见到 Creed，提及 Floor World 内的隧道，并提出只要战胜 King 的军队便可离开。
两个分支均以允许前往棋盘结束。**推断：**这把下一挑战表述为离开条件。
**已确认：**在此边界，回调仅写许可标志，不写 F422、传送、招募、奖励或 F522 完成，也没有 Yes/No 分支。
单独交谈不会解锁或启动战斗。若最后文本未返回，后续写入仍未确立。
传入的 F777 也可选择重复文本，不能因此证明先前交谈。

Guard135 仅测试 F777。许可前，他说军队强大、适合训练；许可后，他说队伍到棋盘中心前军队不会攻击。
这是对话。实际触发使用 Y16 上的通配 X，随后另作准入检查；这句台词不提供普遍的中心或战斗区域规则。

Guard136 的许可台词使用 **F212，独立于 F777**。F212 未设置时即使 F777 已设置，仍选择 1802，且不受 F258 影响。
已标识但排除的 F212 已设置后续选择 1803，然后测试 F258：已设置则返回；未设置则调用 `cs_599A4`，
并仅在其返回后设置 F258。本指南不使用该脚本程序体、Cotton Balloon 持有/使用或后续退出。
F212/F258 不门控棋盘区域（事件）。

女仆排除的 F522 已设置后续测试 F256：未设置则选择 1798–1799 后设置 F256；已设置则选择 1800–1801。
它们与 King1797 都是已标识的替代分支，不是选定战前内容。Init 所有者的 F522/F778 分发至 `cs_5996E` 也仍在本指南之外。
F522 并不一概禁止所有其他对话。

## 棋子

| Logical 行动者；基线精灵 | 示例 physical 槽位 | 行 / Event | 选定战前工作 |
| --- | --- | --- | --- |
| NPC137；KING153 | 14 | RIGHT0 / 11 | 1790,1791；返回。 |
| NPC138,145,146；QUEEN135 | 15,22,23 | RIGHT0 / 12 | 1789；返回。 |
| NPC139,140；BISHOP144 | 16,17 | RIGHT0 / 13 | 1788；返回。 |
| NPC141,142；KNIGHT78 | 18,19 | RIGHT0 / 15 | 1787；返回。 |
| NPC143,144；ROOK125 | 20,21 | RIGHT0 / 17 | 1785；返回。 |
| NPC147–152；PAWN77 | 24–29 | RIGHT0 / 21 | 1786；返回。 |
| 未匹配的 logical ID | 取决于映射 | Default0 | `Map22_DefaultEntityEvent`：仅返回。 |

至此覆盖全部 27 个显式行：三个 Queen、两个 Bishop、两个 Knight、两个 Rook、六个 Pawn、棋子 King 及上文十一个行动者。
ORB153–155 没有显式事件行。**已确认：**选定棋子回调均为文本及返回。
除 King 的许可分支外，选定纯文本回调不写剧情/请求状态；包装器朝向、立绘/台词声音、事件相对位置及更新状态仍会改变。

**推断：**棋子台词刻画了一支骄傲且多样的军队。Rook 的盔甲、Pawn 的攻击夸耀、Knight 的备战、Bishop 的治疗笑话、
Queen 的领导及 King 的平衡宣言，均不定义交战属性值、能力或后续战斗名册。

若激活未找到行动者，普通调用方转而检查区域，并可能落入 FieldMenu。
Map22 的 AreaDescriptions 回调仅为 `rts`；包装器 d7 初始为零时，它不提供已处理的描述。
这不是对全部搜索奖励的审计。Section5 的物品处理器是独立的未来退出，不属本结果。

## 实际说话者与完整选定文本

**已确认：**立绘/台词声音查找使用映射后行动者的实际 MAPSPRITE 字节。
己方角色查找槽位直接使用；logical 128+ 减去 ENTITY_ENEMY_INDEX_DIFFERENCE=96。
零脚本查找解析为 physical 0，没有单独的缺失行动者拒绝。因此别名和传入映射会改变朝向及台词声音使用的行动者。

| 基线映射下的实际精灵 | 立绘；台词声音请求 |
| --- | --- |
| Eric33/34/35；Karna36/37/38 | 分别为 Eric14；Karna15，均为 bleep5。 |
| PRIEST192 | Priest34；bleep5。 |
| YOUNG_MAN196 / YOUNG_WOMAN197 / OLD_MAN198 | NONE；bleep2 / bleep3 / bleep6。 |
| MAID208 / KING_PONPEI188 / GUARD206 | NONE；bleep3 / bleep6 / bleep5。 |
| CARAVAN62；选定棋子精灵 | 无匹配条目：DEFAULT；bleep6。 |

NONE=255 符号扩展为 -1；DEFAULT=$FFFF 也是 -1。两者均不请求新立绘窗口。
不要为 Ponpei 或棋子 King 使用 KING_GRANSEAL 的立绘。Eric/Karna 的职业变体各自共享其立绘，
但探索中零 HP 选择 BLUE_FLAME，别名/职业变化也会改变实际精灵。它们的回调不另加存活测试。
精灵改变本身不能证明对话不能运行。Caravan 服务覆盖仍独立于此分发器结果。

选定回调集恰为 1766–1791 加 1802，由完整选定分支导出；1792–1793 添加两句遭遇台词，
共 **29 个不同的字面文本/控制符关联**。普通 `txt` 使用 TEXTBOX trap 和 ID，沿用分发器说话者状态。
没有选定字面文本替换动态名称。`{N}` 为换行控制符；`{W1}`/`{W2}` 保留为显式控制符，
没有测量等待或已证明的输入推进。下表保留已接受所有者的原文，包括文本 1788：

| ID | 说话者 / 条件 | 精确字面文本 |
| --- | --- | --- |
| 1766 | Young man129 | `Creed is a fine gentleman.{W2}` |
| 1767 | Young man129 | `His magic saved us a long time{N}ago during a natural disaster.{W1}` |
| 1768 | Young woman130 | `Devils cannot use their magic{N}to affect human lives.{W2}` |
| 1769 | Young woman130 | `Supposedly, it's one of their{N}rules.{W1}` |
| 1770 | Old man131 | `This Kingdom was made by{N}Creed.  There's no concept{N}of TIME here.{W2}` |
| 1771 | Old man131 | `We never get old.{N}We never die.{N}What is the meaning of life?{W1}` |
| 1772 | Eric14 | `I was there during the{N}disaster.{W2}` |
| 1773 | Eric14 | `I was looking for the{N}legendary sword.{N}I hate it here!{W1}` |
| 1774 | Karna15 | `Are you from the outside?{N}Why did you come here?{W2}` |
| 1775 | Karna15 | `Ah, a mistake.  I see.{W2}` |
| 1776 | Karna15 | `I suggest you see King{N}Ponpei.{N}He's a good man.{W1}` |
| 1777 | Maid133；F522 未设置 | `I'm sure King Ponpei will{N}help you.{W1}` |
| 1778 | Ponpei134；首次许可 | `So, you were not able to see{N}Creed?{W2}` |
| 1779 | Ponpei134；首次许可 | `Somewhere in Floor World,{N}there's a tunnel that leads{N}to Creed.{W2}` |
| 1780 | Ponpei134；首次许可 | `OK, I'll let you go if you{N}win a game against my army.{W2}` |
| 1781 | Ponpei134；两个战前分支 | `You may go to the chessboard{N}now.{W1}` |
| 1782 | Guard135；F777 已设置 | `The army won't attack until{N}you reach the center of the{N}board.{W1}` |
| 1783 | Guard135；F777 未设置 | `The Chess Army is very{N}strong.{W2}` |
| 1784 | Guard135；F777 未设置 | `It will be good training for{N}you.{W1}` |
| 1785 | Rooks143,144 | `Mr. Creed gave me this solid,{N}armored body!{W1}` |
| 1786 | Pawns147–152 | `My attack power is the best{N}in the army!{W1}` |
| 1787 | Knights141,142 | `We knights are always ready{N}to fight!{W1}` |
| 1788 | Bishops139,140 | `Is healing is my only ability?{N}Ha, ha!  You have a lot to{N}learn!{W1}` |
| 1789 | Queens138,145,146 | `I lead the army with the King.{W1}` |
| 1790 | Chess King137 | `The Chess Army of Desktop{N}Kingdom is a well balanced{N}force!{W2}` |
| 1791 | Chess King137 | `If it is King Ponpei's wish,{N}we will accept your{N}challenge!{W1}` |
| 1792 | Ponpei134；nextText $0 | `Are you ready?{W2}` |
| 1793 | Ponpei134；nextSingleText $0 | `Now, start the battle!{N}Do your best!{W1}` |
| 1802 | Guard136；F212 未设置 | `Halt!  I can't let you{N}pass without the King's{N}permission.{W1}` |

## 请求遭遇

**已确认：**唯一的区域（事件）行是 `(255,16)` → Map22_ZoneEvent0，后接默认 `rts`。
字节 255 是 X 通配符；Y 必须为 16。Y16 上任意传入 X 均匹配包装器行。
这不证明该行每个 physical 瓦片都可自然请求事件。普通控制行动脚本识别目的地图块类型 `$1400`，
写 MAP_EVENT_TYPE=6，并由目的地加尝试偏移、除以 384 导出 X/Y 字参数。
布局、移动和可达性仍有影响；该行不添加局部朝向或 F776 门控。

WaitForEvent 优先处理待处理事件，再处理 A/C。ProcessMapEvent 在分发 type 6 之前清除类型。
Type 6 使用方在 physical 0 安装 init 行动脚本，将 PARAM1/PARAM3 的 X/Y 字传给区域（事件）包装器。
回调先测试 F777，未设置则返回；再测试 F522，已设置则返回。
仅 **F777 已设置/F522 未设置**写 EGRESS_MAP=22 并调用 `cs_5994E`。

EGRESS_MAP 仅提供地图选择器。字节写入既不提供撤离坐标或朝向，也不改变地图事件参数。
后续传送提供自己的目的地元组。这不是已完成的撤离、存档点或服务交易；
后续场景命令未能完成时，地图字节可能已经改变。

### 完整八命令场景

**已确认：**`cs_5994E` 有八个命令、32 编码字节。索引从零开始。
序列询问队伍是否准备好，宣布开始，再写解锁和传送：

| 索引 | 精确命令 | 直接效果与完成边界 |
| --- | --- | --- |
| 0 | `textCursor 1792` | 设置 CUTSCENE_DIALOG_INDEX=1792。 |
| 1 | `csWait 50` | 消费编码的负等待；关闭文本跳过时 Sleep 使用低字节 50。 |
| 2 | `setCamDest 10,0` | 设置 VIEW_TARGET_ENTITY=-1，请求摄像机目的地并等待滚动。 |
| 3 | `nextText $0,134` | 解析 NPC134 实际立绘/台词声音，等待滚动，显示 1792，推进游标。 |
| 4 | `nextSingleText $0,134` | 显示 1793，推进游标，关闭立绘/文本，然后 Sleep 10。 |
| 5 | `setStoryFlag 22` | 加 BATTLE_UNLOCKED_FLAGS_START=400 并设置 F422；既非 F22，也非 F522。 |
| 6 | `warp MAP_DESKTOP_KINGDOM,0,0,RIGHT` | 写待处理 type 1 及 PARAM1–5 字节 `0,22,0,0,0`。 |
| 7 | `csc_end` | 结束脚本；条件滚动等待，清 VIEW_SCROLLING_SPEED，还原调用方寄存器。 |

摄像机 `(10,0)` 是按 384 缩放的请求边界，不是行动者放置或传送目的地。
滚动使用方以 VInt 等待轮询并重新检查，没有超时。文本处理器也等待滚动。
基线 KING_PONPEI 不请求新立绘，两句均使用 bleep6。Modifier$0 既不添加右侧位，也不添加镜像位；
已打开的立绘不会重新打开。场景不转动或移动 NPC134，也不招募任何人。

尽管询问“Are you ready?”，仍**没有 Yes/No 命令或回答分支**。
普通不跳过执行仅在前面的等待/摄像机/文本调用返回后才到达 F422 和传送。
调试 P2-Start 文本跳过路径可跳过编码等待及两个文本调用，但不跳过摄像机、标志或传送。
源码等待不是测得的表现时长，也不证明完成。

脚本返回后，区域（事件）包装器关闭窗口，等待一次 VInt，并等到 physical 0 当前 X/Y 等于其目的地。
它不等待每个实体/行动。此时待处理传送已存在；包装器未完成会使其尚未消费。
EGRESS_MAP、F422、待处理传送和包装器完成是独立的部分状态边界，没有回滚。

## 消费传送与准入 Battle22

**已确认：**存续请求优先于摄像机目标 0/当前模式控制重新安装。
ProcessMapEvent 清除待处理类型，因 PARAM1=0 而走 type 1 不滚动路径。
它消费/清除 WARP_SFX，让 physical 0 空闲，并从探索展开返回 MainLoop。
UpdatePlayerPosFromMapEvent 起初读取 physical 0 的瓦片；显式非负 PARAM3/PARAM4 以 `(0,0)` 覆盖，
PARAM5 提供 Right=0。因此请求元组为 `(22,0,0,Right)`，独立于摄像机 `(10,0)`。
这些阶段之间的可用局部控制未确立。

MainLoop 调用 SwitchMap，然后 CheckBattle。完整切换地图表没有 Map22 条目。
唯一 Map22 战斗坐标行是从零计数的 Battle22，元组 `22,9,0,30,30,255,255`。
CheckBattle 依次测试**地图、F422，再测试两个 255 通配触发字节**。
它不检查玩家是否在战斗区域矩形内。选中时写区域 `(9,0,30,30)`，然后检查 F522。
F522 已设置会清 F422，但不撤销本次调用的选择。

| 消费时实际状态 | 条件结果 |
| --- | --- |
| 存续 Map22 传送；F422 已设置、F522 未设置 | 即使 `(0,0)` 在区域外，通配仍匹配。写区域，保留 F422，返回 22；MainLoop 以 22 调用 BattleLoop。 |
| CheckBattle 前独立清除 F422 | 在区域/完成检查前跳过该行；返回 -1 并进入全新显式 Map22 探索。此处未确立后续 init/控制。 |
| 区域（事件）准入后独立设置 F522；F422 仍已设置 | 写区域，清 F422，仍返回 22，本次仍准入 BattleLoop。 |
| 请求被替换、未完成或目的地改变 | 消费实际存续字段；不保证具名 Map22/Battle22 结果。 |

F777 未设置/F522 已设置的区域（事件）绕过不写 EGRESS_MAP、F422 或传送。
包装器返回后，零待处理类型允许 WaitForEvent 以 physical 0 为目标并安装当前模式控制；待处理替换请求优先。
该绕过本身从不调用 CheckBattle，即使传入 F422 已设置。
反过来 CheckBattle 不查询 F777/F776/F212/F258；这些标志不会否决独立获准且带 F422 的 Map22 请求。

**止于 BattleLoop 准入、开场/名册之前，或该条件局部绕过/返回。**
此边界不导出 F522 完成写入、奖励、战后授予、物品获取或退出。
场景被调用和 King 邀请都不证明战斗启动已完成。

## 证据与审阅边界

**已确认溯源：**已接受 main `ccf4a57360fea5b62160fe959a5d2dbca6dd8736` 包含
[PR906](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/906)。其
[研究所有者](../../../research/desktop-kingdom-interactions-through-battle22-entry.md#agreement-and-provenance)
固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
及 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
本综合指南审阅已接受关联，不重新调查原始源码、不提取或运行运行时。

有边界的可执行审阅使用既有
[map-init fixture](../../../../tests/fixtures/h2/map-init-static-v1.json)、
[schema](../../../../schemas/h2-map-init-static-fixture.schema.json) 和
[验证器](../../../../src/sf2tool/h2/map_init.py)，以及
[map-script fixture](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、
[schema](../../../../schemas/h2/map-script-engine-fixture.schema.json) 和
[验证器](../../../../src/sf2tool/h2/map_script_engine.py)。保留配方使用 map-init 相等、
`_verify_map_script_engine_fixture` 及 `_cutscene_facts` 源码相等；本指南不重跑或再生成未改变的 18,285,979 字节对/0.080s 收据。

[entity-program 分片](../../../../tests/fixtures/h2/map-events/entity-programs.json)含 18 个选定 Map22 程序/76 操作；
[zone-program 分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)含两个/八个。
其 [schema 所有者](../../../../schemas/README.md)及
[fixture 加载器](../../../../src/sf2tool/h2/map_events_fixture.py)定义封闭分片边界；
[验证器](../../../../src/sf2tool/h2/map_events.py)保留完整相等。
选定 `programCorpus.programs` 条目关联全部八个场景命令和终止符。
Map-init 分发器/指针顺序及 map-script 汇总证明其具名静态边界。
完整已接受直接源码关联补充这些汇总未独立守护的局部回调、说话者、服务和准入细节。
汇总不证明自然到达、表现或组合运行时成功。本边界未发现冲突的已接受所有者。

保留 [#904 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174520666)：
head `eba9181e8505e38daa261311281b3aab6164af3f`、tree `6ea40ea61af5063d14422570283a55d8b78d1800`、
base `e120a9b6890527eb018e081f1c5b1907c21155bc`、
[CI38608369060](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608369060)。
其[最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174585157)
保留字节相同的变基 head `ee2bf3ff5d00da75e6913ba5b2813c5ee9b49c29`、
tree `7933d0b8dd61953fb9fb40a1584d11cf7478a86e`、
[CI38608730439](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608730439)，
及最终 EGRESS_MAP 澄清 head `4b6595ea854670a9f12a985d3583750b46c217d9`、
tree `edb7ff22fd26365d8432baa32ff978d7883fd831`、
[CI38608904864](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608904864)。
变基/最终 base 是 `affc5679674521f8ce26649763fd749a2e178095`。
三次 CI 的 scope/research-public 均成功；engine、adapter 及 H4 作业按范围跳过。
[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/906#issuecomment-6174618486)适用于最终身份。
原始截断读取、HTTPS 私有路径误报、表范围/King 写入修正及 CRLF 相等检查仍随这些收据保留。

保留 [#903 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174446531)、
[修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174502720)及
[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/905#issuecomment-6174512539)。
这些交接保留原始/修正身份、CI38607861678/38608377291、检查失败及已解决修正：
负 d1 重载复用既有图块/布局；RightLeft 改变朝向。抵达指南的
[证据段](desktop-kingdom-first-arrival.md#证据审阅与改变状态限制)还保留 #900 原始/修正 head、
CI38605731097/38606392847、未改变对/0.081s、已解决的 csc2D FLAGS_A5/6 目的地扫描绕过，
及全部 #899/#896/#895 链接失败与修正。本综合指南不替换或回放任何已完成结果。

直接验收检查完整自有正文/diff、全部行动者/事件及 29 个字面文本/控制符关联、分支/写入顺序、八个命令、
链接/片段、英文/私有边界及空白。仅运行已配置的干净已提交依赖规划器和实际最终 head CI；
单独文档不要求局部 normal/full/engine/Godot/H3/H4/翻译套件。

**未知：**自然局部激活/区域（事件）可达性/行程；传入标志、HP/职业、出战队伍、别名/映射、模式和待处理状态；
已完成服务/摄像机/文本/包装器工作；可见或可听演出、可用控制和存档持久性。
上文改变状态示例是静态反例，不是已执行场景；依
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)及
[ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)，也不构成自动运行时队列。
本有边界指南既未完成战役，也未完成 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
