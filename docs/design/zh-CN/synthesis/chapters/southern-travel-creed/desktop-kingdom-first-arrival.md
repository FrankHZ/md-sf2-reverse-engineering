# Desktop Kingdom：首次抵达与条件控制

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/desktop-kingdom-first-arrival.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**Map22 全新探索抵达构建新实体群，编排出战队伍降落和讨论，再安装跟随，有条件地返回探索。
**推断：**场景表现 Goliath 将缩小的队伍放到桌上；Peter 四处查看的建议给出下一局部目标。
**未知：**自然抵达、实际传入状态、动作/服务完成、可见或可听演出、可用控制及持久性。

本指南从[魔法门转移](creed-magic-door-and-desktop-transfer.md)获准的全新 Map22 探索端点接续；
基线首次抵达要求 F422 未设置，F522/F776 未设置。Fnnn 表示标志 nnn。
仅使用[已接受抵达研究](../../../../../research/chapters/southern-travel-creed/desktop-kingdom-first-arrival.md)，依据
[Issue #903](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903) 及
[Standard 路线图协议](../../../documentation-roadmap.md#当前基线与近期综合)。
止于条件局部首次抵达返回/重复，在局部交互、独立 `cs_5996E` 分支或 Battle22 开场、名册及结果之前。
不包含新游玩流程、原始源码重新调查、重制或本地化工作。

## 全新桌面实体群

**已确认：**准入与初始化独立。F422 未设置避免前篇 Battle22 选择；F422 已设置时即使请求 `(0,0)` 也可选择该战斗。
F522 已设置不能取消已选战斗。本指南只从全新探索路径开始。

该路径清除待处理事件类型、调整踩踏计数器、治疗符合条件己方角色、淡出并禁用摄像机跟随。
显式 map 22 设置 CURRENT_MAP=22/CURRENT_BATTLE=255，加载瓦片集、等待淡入淡出、清除 VInts、选择并初始化实体、加载其精灵。
清除临时标志 256–383、设置 F80、初始化精灵帧状态、指向 physical 0、加载地图、安装基础 VInts 并调用选定 init 回调。
治疗不保证复活。宅邸停放/缩小的 physical 行动者不会作为该新实体群保留。

Map22 有默认配置和 F522 覆盖，无后续覆盖。实体指针不同，但 EntityEvents、ZoneEvents、AreaDescriptions、Section5 和 InitFunction 共享。
每个访问器重新选择当前配置。F776 不选择实体群；F522 替代名册是另一传入状态边界，在下述基线之外。

分配器清除实体记录及查找字节，从 physical 1 开始分配选定随从，再按源码顺序处理 30 个配置行。
它在这些行**之后**，以请求 `(0,0)`、Down、`eas_Idle` 在 physical 0 声明 Bowie。
共享 SPRITE_SIZE 变为 16；随后随从终止及模式/木筏处理。因此 Bowie 不是首个声明记录。

全部基线行如下。P 是条件 physical 槽位示例：仅选择随从标志 F67/Peter 和 F68/B，Eric/Karna 无已有查找，且木筏处理不覆盖槽位。
`Init` 表示 `eas_Init`；`Fixed` 表示 `eas_InitFixedSprite`。逻辑 NPC 名称是脚本身份，不是 physical 槽位或战斗员。

| 行 | 逻辑身份 | P | 地图精灵 / 动作 | 位置、朝向 |
| --- | --- | --- | --- | --- |
| 0 | Eric 14 | 3 | 己方查找；步行原点 `(15,14)`，range 1 | `(15,14)`，Down |
| 1 | Karna 15 | 4 | 己方查找；Init | `(5,15)`，Down |
| 2 | NPC128 | 5 | CARAVAN=62；Init | `(3,6)`，Down |
| 3 | NPC129 | 6 | YOUNG_MAN=196；Init | `(29,21)`，Right |
| 4 | NPC130 | 7 | YOUNG_WOMAN=197；Init | `(30,21)`，Left |
| 5 | NPC131 | 8 | OLD_MAN=198；步行原点 `(24,22)`，range 1 | `(24,22)`，Down |
| 6 | NPC132 | 9 | PRIEST=192；Init | `(5,6)`，Down |
| 7 | NPC133 | 10 | MAID=208；Init | `(14,3)`，Down |
| 8 | NPC134 | 11 | KING_PONPEI=188；Init | `(15,2)`，Down |
| 9 | NPC135 | 12 | GUARD=206；Init | `(8,7)`，Left |
| 10 | NPC136 | 13 | GUARD=206；Init | `(33,24)`，Left |
| 11 | NPC137 | 14 | KING=153；Init | `(24,5)`，Down |
| 12 | NPC138 | 15 | QUEEN=135；Init | `(26,5)`，Down |
| 13 | NPC139 | 16 | BISHOP=144；Init | `(22,5)`，Down |
| 14 | NPC140 | 17 | BISHOP=144；Init | `(28,5)`，Down |
| 15 | NPC141 | 18 | KNIGHT=78；Init | `(20,5)`，Down |
| 16 | NPC142 | 19 | KNIGHT=78；Init | `(30,5)`，Down |
| 17 | NPC143 | 20 | ROOK=125；Init | `(18,5)`，Down |
| 18 | NPC144 | 21 | ROOK=125；Init | `(32,5)`，Down |
| 19 | NPC145 | 22 | QUEEN=135；Init | `(24,6)`，Down |
| 20 | NPC146 | 23 | QUEEN=135；Init | `(26,6)`，Down |
| 21 | NPC147 | 24 | PAWN=77；Init | `(22,6)`，Down |
| 22 | NPC148 | 25 | PAWN=77；Init | `(28,6)`，Down |
| 23 | NPC149 | 26 | PAWN=77；Init | `(20,6)`，Down |
| 24 | NPC150 | 27 | PAWN=77；Init | `(30,6)`，Down |
| 25 | NPC151 | 28 | PAWN=77；Init | `(18,6)`，Down |
| 26 | NPC152 | 29 | PAWN=77；Init | `(32,6)`，Down |
| 27 | NPC153 | 30 | ORB=172；Fixed | `(63,63)`，Down |
| 28 | NPC154 | 31 | ORB=172；Fixed | `(63,63)`，Down |
| 29 | NPC155 | 32 | ORB=172；Fixed | `(63,63)`，Down |

Eric 14 和 Karna 15 的精灵操作数小于 30，因此使用己方查找槽位。已有非零己方查找会完整跳过对应行，不再分配 physical 槽位；
两行均不要求已加入/出战成员资格。当前随从表既不选择 Eric，也不选择 Karna。
职业和 HP 仍影响精灵选择，包括战斗外死亡角色的 BLUE_FLAME。
Eric 和 NPC131 获得上述原点/范围的步行程序：初始 wait 30，速度/加速度/碰撞配置，随后重复随机步行、目的地等待和 wait 20。
其声明不固定后续随机位置。

其他行从偏移 32 追加 NPC 查找；logical 128–155 减去 96 解析。Eric/Karna 不消耗 NPC 游标。
双随从示例中，Peter 7 为 physical 1，B 31 为 physical 2，初始在请求位置/朝向，使用 `eas_Follower1`/`eas_Follower2`。
Map22 不是大地图，因此 F65 不添加 Caravan/A 前缀。其他随从选择器独立分配；重复 B/A/D 身份不去重。
较后查找写入替换映射，较早 physical 记录保留。缺失 Peter/B 查找可解析到 physical 0。
仅军团成员资格不提供这两个随从，P 列也不是通用名册。

NPC128 是新地图的 **CARAVAN62**，不是宅邸 WORKER。选定抵达命令均不改变其精灵或位置。
对话将其台词归于 Goliath；这不声明巨人精灵、不证明其自然在场，也不将宅邸行动者带入地图。

选定入场/场景均不设置 PLAYER_TYPE 或已保存木筏位置。步行 mode 0 是一致前篇的前提，不是身处桌面的效果。
F64 加 RAFT_MAP=22 可在 physical 31 声明木筏，坐标以 `$7F` 掩码，覆盖示例 ORB154 记录，而其 NPC 查找仍指向 31。
Mode 2 则让主角色使用 RAFT 精灵、停放随从，并不检查已保存地图便在 `(64,64)` 声明 slot31。
初始精灵加载包含非空 physical32；后续精灵变换跳过 ENTNUM=32。分配、查找和精灵更新行为是不同事实。

## 降落与辨认处境

**已确认：**共享 init 分发器先检查 F776。未设置则运行完整 `cs_59656`；只有返回后调用方才可设置 F776。
已设置跳过整个抵达，不证明其曾完成。场景包含 136 个顶层命令、七个自定义体和十三个内联体。
下文命令索引从零开始；B 为 logical FOLLOWER_B=31。三人组按 Bowie、Peter、B 顺序。
每个内联体以 `endActions` 结束；每个自定义体以 `ac_end` 结束。坐标、速度和等待是字面源码操作数，不是实测演出。

开场为成对下降编排六名行动者，移除三人影子并淡入。ORB 隐藏后，为讨论准备出战队伍的 orientation 和动画。
这支持预期降落图像，不证明搬运或同时抵达。

| 命令 | 完整有序动作 |
| --- | --- |
| 0–10 | 文本游标 1748；reloadMap `0,19`；三人组依次带等待 Init；三人组依次 stopEntity；三个带等待自定义体，各为 `ac_setSpeed 64,64; ac_jump eas_Idle`。 |
| 11–20 | setPos Bowie `(4,8)`、Peter `(3,7)`、B `(5,7)`、ORB153 `(12,16)`、ORB154 `(11,15)`、ORB155 `(13,15)`，均 Down；三人组依次 removeShadow；fadeInB。 |
| 21–29 | 不等待内联 Down 16，依次 Bowie、Peter、B；再不等待 ORB153、ORB154 DownLeft 8；等待 ORB155 DownLeft 8；依次 hide 153、154、155。 |
| 30–39 | 三个带等待自定义体：Bowie 为 `ac_motion OFF; ac_orientLeft`，Peter 和 B 为 `ac_orientRight`，各自再 `ac_updateSprite; ac_jump eas_Idle`。wait 100；三人组依次 startEntity；三人组依次带等待 Init。 |

`reloadMap 0,19` 是**当前 Map22** 的摄像机/显示工作。它压缩摄像机字，将负 d1 传给 LoadMap，重用既有图块/布局，同时重新建立区域、摄像机、视差、动画及显示状态。
显式摄像机输入在区域偏移前提供 `(0,7296)`，等价于 384 单位坐标中的 tile Y19。
它既不请求 map0、不将主角色放在 `(0,19)`、不重建实体，也不再次调用 init。
CURRENT_BATTLE=255 选择完整地图加载，不是 Battle22 裁剪。

SetPos 按每块 384 单位同时写入当前和目的地坐标、朝向及精灵更新请求。
内联移动编译为相对运动和目的地等待，再空闲。安装器 `csc2D` 先对 FLAGS_A 应用 `andi.b #$9F`，清除位 5/6。
其 esc04 使用方调用 HasSameDestinationAsOtherEntity；后者初始 bit-5 测试在扫描竞争目的地前，经 `clr.w d4` 返回并设置 Z；esc04 的 `bne` 冲突分支不采用。
这些**未修改的选定移动绕过目的地扫描**，包括三人组/ORB 重合目标。普通碰撞不能解释成对目的地。

在所述放置及未修改移动完成的条件下，名义配对为 Bowie/153 在 `(4,24)`、Peter/154 在 `(3,23)`、B/155 在 `(5,23)`。
隐藏 153,154,155 前，只等待 ORB155 内联动作。别名、动作替换或服务停滞仍使六者同时抵达无法得到保证。
Hide 写入当前/目的地 `$7000,$7000`，将全部匹配查找字节变为 `$FF`，并移除匹配随从条目。
它保留动作指针；这比宅邸 `(63,63)` 停放更强。木筏覆盖时，隐藏 154 可隐藏该木筏并使其其他查找失效。这里未观察可见结果。

## 意识到所在之处

**已确认：**B 回忆奇怪的门和被向上托起的感觉。Peter 说众人变小、调查，并描述如村庄般巨大的桌子。
NPC128 回答；Peter 认出 Goliath 并抗议。后续交流将降落与 Goliath 警告连接，但不实现通用魔法门或保护规则。

| 命令 | 完整有序动作 |
| --- | --- |
| 40–51 | Bowie、Peter、B 依次 headshake；分别朝 Up、Right、Left；wait 20；B 文本 1748–1750、Peter 文本 1751–1752。 |
| 52–54 | 带等待 B 自定义 `ac_setSpeed 48,48; ac_jump eas_Idle`；带等待 B 内联 Left 1；B 文本 1753。 |
| 55–66 | wait 5；不等待 Peter RightLeft；wait 100；带等待 Peter 内联 Down 2 再 Right 1；wait 5；不等待 Peter RightLeft；wait 130；Peter 朝 Up、Bowie Down、B Down；Peter 文本 1754；NPC128 文本 1755。 |
| 67–76 | 不等待 Bowie 内联 Left 1；带等待 Peter 内联 Right 1；wait 5；不等待 Bowie RightLeft；wait 10；wait 5；不等待 Peter RightLeft；wait 5；不等待 B RightLeft；wait 130。 |
| 77–89 | Peter、Bowie、B 朝 Down；带等待 Bowie Jump、不等待 Peter Jump；先 shiver Bowie、再 Peter；先 Bowie、再 Peter 带等待 DeactivateAutoFacing；不等待 Bowie 内联 Up 1、带等待 Peter 内联 Up 1；先 Bowie、再 Peter 带等待 Init。 |
| 90–96 | Peter 继续文本 1756；连续两次带等待 Peter Jump；wait 10；Peter 单文本 1757；NPC128 继续 1758、再单文本 1759。 |

完整文本序列如下。C 表示继续 `nextText`；S 表示 `nextSingleText`，显示后关闭文本/立绘并 sleep 10。
`$C0` 选择右侧和镜像立绘位；`$0` 均不选择。没有 Yes/No 提示。
**推断：**B 预期为 Oddler，取决于实际查找。NPC128 台词**推断**为 Goliath 声音，不是精灵替换。

| ID | 形式；修饰符；说话者 | 精确字面文本 |
| --- | --- | --- |
| 1748 | C; $C0; B | `What happened?{W2}` |
| 1749 | C; $C0; B | `A strange feeling swept over{N}me when I entered that door.{W2}` |
| 1750 | S; $C0; B | `Then, it felt like...I was{N}lifted upward!{W1}` |
| 1751 | C; $0; Peter | `It's very difficult to explain{N}what's happened to us....{W2}` |
| 1752 | S; $0; Peter | `Anyway, we're very small now.{W1}` |
| 1753 | S; $C0; B | `Small?  How small?{W1}` |
| 1754 | S; $0; Peter | `Let's see...now we're on a{N}desk...and it's very large,{N}like a village.{W1}` |
| 1755 | S; $0; 128 | `Exactly.{W1}` |
| 1756 | C; $0; Peter | `Goliath!{W2}` |
| 1757 | S; $0; Peter | `Hey, what did you do to us?{W1}` |
| 1758 | C; $0; 128 | `I told you not to go in{N}there.  You didn't listen{N}to my warning.{W2}` |
| 1759 | S; $0; 128 | `You guys look so cute!{N}Ha, ha!{N}Bye, little kiddies!{W1}` |
| 1760 | C; $0; Peter | `{LEADER}, cheer up!{W2}` |
| 1761 | S; $0; Peter | `Let's look around this{N}Desktop Kingdom.{W1}` |
| 1762 | S; $0; B | `I agree!  We can meet Creed{N}later.{W1}` |

**已确认：**立绘/台词选择读取**实际映射精灵**，不是叙事名称。
B 映射到 ODDLER 时选择 Oddler 立绘和 bleep 4；Peter 基础/转职后精灵选择各自立绘和 bleep 4/5。
完整对话属性表没有 CARAVAN 条目：基线 128 回退到 PORTRAIT_DEFAULT `$FFFF`（无新立绘窗口）及 bleep 6。
已打开立绘不重新打开。HP/职业、重复/缺失映射或其他选定随从可改变表现；仅 modifier 0 不保证立绘。

`{N}` 推进一行。W1/W2 等待方向/A/B/C 输入；W2 额外使用箭头及验证声音路径。
`{LEADER}` 运行 UpdateForce，命名首个战斗出战成员，而非不可变 Bowie 字符串。
调试模式加 P2 Start 可锁存 SKIP_CUTSCENE_TEXT，绕过选定文本命令和 csWait sleep。
名义序列假设跳过未设置；源码等待既不确立传入输入，也不确立实际显示时长。

## 告别与探索邀请

**已确认：**Goliath 戏谑告别后，脚本请求两组声音并让三人转向。
Peter 鼓励领队四处查看 Desktop Kingdom；B 同意稍后见 Creed。这是局部叙事目标。
声音调用不证明听到巨人脚步或离开完成：此段没有给 NPC128 移动命令。

| 命令 | 完整有序动作 |
| --- | --- |
| 97–109 | 五次重复 wait 20 再 playSound DIALOG_BLEEP_6；Bowie、Peter、B 朝 Right。 |
| 110–120 | 五次重复 wait 20 再 playSound DIALOG_BLEEP_8；wait 50。 |
| 121–129 | 带等待 Peter 内联 Left 2；Bowie 朝 Down、Peter Up、B Down；Peter 继续 1760 再单文本 1761；Bowie 朝 Right、B Left；B 单文本 1762。 |
| 130–135 | 依次带等待 Init Bowie、Peter、B；followEntity Peter/Bowie/2、B/Peter/2；csc_end。 |

五次 bleep 6 请求（constant 74）和五次 bleep 8 请求（76）是显式 playSound 调用，独立于逐字台词声音。
每次之前 wait 20；第二组之后 wait 50。处理器把操作数交给声音命令服务，不等待可听播放完成。
文本/声音请求与可听演出仍相区分。

## 临时动作及其完成限制

**已确认：**动作名称描述不同机制。stop/start 写动画计数器 -1/0；不冻结移动或全局禁用输入。
自定义 `ac_motion OFF` 也写动画计数器 -1，不清零速度或取消目的地。orientation 独立于 facing 改变低 FLAGS_B 位；速度写入影响 X/Y 速度字段。

Init 重置加速度因子 0/0、速度 32/32、关闭加速/减速、开启可阻挡、关闭 entity/map uncollidable、开启自动朝向、orientation Up、关闭浸没、
共享尺寸 24，随后更新精灵并跳至空闲。它既不放置/复活行动者，也不打开动画。InitFixedSprite 额外关闭自动朝向和动画。
开场自定义速度 64 和 B 后续速度 48 是临时值；最终 Init 恢复 32。较早 startEntity 和 headshake 还原是独立动画写入。

RightLeft 指四次 Right/Left/Right/Left 朝向变化，每次更新精灵并 wait 30，再空闲。
Jump 关闭自动朝向，移动 `(0,-1)`、wait 5、移动 `(0,1)`、wait 5、移动 `(0,0)`、还原自动朝向并空闲。
DeactivateAutoFacing 仅关闭该属性并空闲。即使先前不等待的转向或跳跃未完成，安装后续动作仍会替换行动者指针。
带等待形式**只轮询解析行动者的指针是否为 eas_Idle**；安装器计时器播种使用 physical 索引。
它们不提供全行动者屏障或超时。累加等待不证明精确时长、位移或服务完成。

RemoveShadow 经 576 字节临时缓冲变换映射精灵的两个帧，DMA 写入并等待一次 VInt；不创建持久影子状态字段。
后续精灵更新可重载像素。FadeInB 对全部调色板请求 FadeInFromBlack，并等待淡入淡出服务及额外一次 VInt。
服务返回不是已观察图像。

Headshake 同步执行七次像素重排迭代，含精灵加载/DMA、两次 VInt、更新及每半段后额外一次 VInt，再还原动画计数器 0。
它不移动存储位置或朝向。Shiver 保存共享尺寸/动画，写尺寸 21 和动画 -1，再三次启用 resize/update/Sleep 5、禁用 resize/update/Sleep 5。
它还原已保存尺寸和动画，不还原较早 resize bit。这两个命令都不确立永久尺寸。

尺寸是单个共享字，在行动者 resize bit 已设置时由变换使用。orientation 和选定效果也在此应用；
特殊精灵和 ENTNUM=32 跳过该变换，而精灵加载容量可延迟动作进展。
全新尺寸 16、Init 24、shiver 21 和前篇 18 是不同共享写入，不确立持久微型队伍、改变几何、属性值、物品栏或拾取附着关系。

## 跟随、标志与条件返回

**已确认：**最终 Init 后是 Peter 跟随 Bowie、B 跟随 Peter，operand 2。
处理器解析 physical 身份、复制 42 字节随从程序、修补目标和有符号偏移 `(-24,0)`、安装，并在 physical 随从不存在时添加至 EXPLORATION_ENTITIES。
既不授予成员资格，也不传送至已完成队形。安装程序反复跟随目标/目的地；命令返回不证明跟随结果。

存活守卫不同。放置、朝向和 stop/start 可跳过死亡己方角色。
动作安装、headshake、shiver、removeShadow 和 hide 不共享该守卫。
跟随处理器读取**字随从操作数之前的高字节**：编码 `$0007`/`$001F` 均提供零，因此两个选定跟随调用都测试 Bowie HP。
属性查找把战斗员索引掩码至低字节；之后才解析完整随从操作数。
因此不能用统一存活/缺失行动者政策概括 Peter/B HP 和缺失/重复映射。

场景终止符在存在对话窗口时有条件地等待视野滚动，清除滚动速度并还原寄存器；不等待并发行动者全部完成。
init 包装器仅调用选定回调并还原寄存器，没有区域包装器的 physical 0 坐标等待或文本/立绘关闭。
只有 `cs_59656` 返回后，其调用方才写 F776。

**独立下一检查**是 F522：未设置则返回；已设置则检查 F778，已设置时返回，否则调用排除的 `cs_5996E` 再设置 F778。
F776 已设置会绕过首次抵达，但不绕过第二次检查。两个配置指针都使用此分发器。
这只标识后续内容，不解释其程序体，也不把 F776 当成实体群/完成历史保证。

抵达场景内部没有剧情标志/F422/F522/F776、出战队伍成员资格、PLAYER_TYPE、已保存木筏、传送或战斗准入写入。
F80/临时标志工作属于较早调用方。init 后，探索比较调色板字、有条件地请求音乐/淡入淡出、设置移动声音并进入 WaitForEvent。
只有 MAP_EVENT_TYPE 零才允许在 physical 0 安装摄像机目标 0 和当前模式控制器。
Mode 0 使用 eas_ControlledCharacter；mode 1 使用源码名 eas_Raft（Caravan 控制）；否则 eas_Caravan（木筏控制），不验证任意模式。
非零待处理工作跳过控制器安装，优先于 A/C 输入。因此场景返回不保证无条件可用控制或稳定事件。

全新显式地图入场不同于运行 UpdateMainEntityProperties 和 DeclareRaftEntity 的 map-index-255 返回分支。
摄像机重载和 init 返回均不进入该分支，不重建实体群，也不将主角色恢复至 `(0,0)`。
传入模式和实际场景变更仍有影响。本指南止于此条件首次抵达/重复边界，在交互、战后场景或 Chessboard 准入/内容之前。

后续独立阅读 [Desktop 对话与 Chessboard 挑战（中文）](desktop-kingdom-interactions-through-battle22-entry.md)，
了解居民/服务、King Ponpei 许可及条件 Battle22 准入。本首次抵达指南保留局部返回/重复停止点。

## 证据审阅与改变状态限制

**已确认溯源：**已接受 main `e120a9b6890527eb018e081f1c5b1907c21155bc` 包含
[PR902](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902) 修正研究。
其[研究所有者](../../../../../research/chapters/southern-travel-creed/desktop-kingdom-first-arrival.md#agreement-and-provenance)固定
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA 零售 ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
它负责完整 30 行实体群、136 命令场景、七个自定义/十三个内联体、十五个文本/控制符/说话者关联及具名直接使用方。
本指南审阅这些已接受关联，不审阅原始源码或新增运行时。

有边界的既有可执行审阅覆盖 [map-init fixture sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)、
其 [schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json) 和[验证器](../../../../../../src/sf2tool/h2/map_init.py)，
以及 [map-script fixture sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、
其 [schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json) 和[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)。
既有所有者为 `h2 map-init` 和 `h2 map-script-engine`；已接受[保留比较配方](../../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
使用 map-init 相等、`_verify_map_script_engine_fixture` 和 `_cutscene_facts` 相等，不重新生成载荷。
Map-init 提供选定分发器/指针顺序。Map-script 汇总及动作桥事实确立具名流/处理器边界，不确立每项局部动作、文本、分配或服务声明。
完整已接受选定直接源码关联补充它们；任何汇总都不证明自然到达、表现或组合运行时成功。此边界未发现冲突的已接受所有者。

保留 [#900 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174161863)
（head `d076ad3b35c97f1ee3af71ffa20872f18c0329b7`，tree `288535602cb5b6a7a8e03123731f631cfef339c8`）、
[修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174261324)
（head `6a37579b1bb5e97f119ca933459e63e12060c4ad`，tree `f8760697e748a395b4d041771be3e6a5ac6b7f11`）
及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174272544)。
两者基于 `95d55721b7234c7c8e877174fd8f9ff65e2bbca8`。未改变的保留对共 18,285,979 字节，在 0.081s 内完成比较。
原始 [CI38605731097](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38605731097)
及修正 [CI38606392847](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38606392847)保持独立。
保留[已解决 P2 审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174194697)：
安装器清除 FLAGS_A5/6，使这些移动绕过目的地扫描；本指南保留该修正，不采用原始无条件冲突解释。
段落修正未重跑原始完整关联和比较。

保留 [#899 前篇交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/899#issuecomment-6174047117)
及 [PR901 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/901#issuecomment-6174065903)，
包括原始/变基后 #896 收据、#895 结果及更早链接失败。
#900 有边界检查/路径/常量解码/CRLF 失败及修正、#899 私有路径误报收据及修正通过、独立审阅有界读取/附件失败仍保留在这些链接。
本指南不替换它们，也不将其改称失败的游戏套件。

| 改变的传入前提 | 已接受静态审阅保留的后果 |
| --- | --- |
| F422 已设置 | 可在此探索前提之前选择 Battle22。 |
| F776 已设置、F522 未设置 | 加载基线实体群，但跳过抵达/放置/跟随工作；不证明曾完成。 |
| F522 已设置、F778 未设置/已设置 | 替代实体群，再在 F776 检查后独立战后调用/返回；不属此基线范围。 |
| 其他/缺失/重复随从映射、HP 或职业 | physical 分配、说话者、守卫及替换指针不同；逻辑名称不保证独立存活行动者。 |
| Mode 2 或 F64 加 RAFT_MAP=22 | 主角色/随从/slot31 行为不同；名义 ORB154 实际可能指木筏。 |
| 共享尺寸改变、延迟/跳过更新、替换/未完成并发动作 | 不推导持久尺寸或六行动者同时完成；未修改移动仍适用安装器目的地扫描绕过。 |
| 文本/淡入淡出/动作服务停滞 | 先前写入可存在，而无后续对话、F776 或控制；无回滚/全行动者屏障。 |
| WaitForEvent 前有待处理请求 | 待处理工作优先于安装控制器；无受保护可用控制区间。 |

这些是静态区分条件，不是已执行场景。**未知：**自然到达；传入标志、配置、出战队伍/HP/职业/查找、模式/木筏/待处理状态；
服务和并发动作完成；渲染 ORB/影子/尺寸/姿态/NPC 演出；可听声音；可用控制及存档持久性。
依 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)，这些不创建自动运行时队列。
后续局部交互和战后/Battle22 内容仍独立；战役及 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)均未完成。
