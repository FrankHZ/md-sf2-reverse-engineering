# Floor World 居民、巨环与对外退出

> 本文是[规范英文原文](../../synthesis/floor-world-interactions-and-exits.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**Floor World 有六个居民回调及一个无声默认回调、独立的 F780 门控区域（事件）场景，
以及三个不滚动传送记录。场景以 resident129 为说话者，请求向上八瓦片移动，等待该行动者并隐藏它；
仅场景返回后调用方才可设置 F780。选定退出均不测试 F780。
**推断：**场景描绘一次未成功举起巨环的尝试。
**未知：**自然到达、实际传入状态、服务/行动完成、表现、可用控制及持久性。

本指南从[首次抵达与条件探索](floor-world-first-arrival.md)的局部控制/待处理事件边界接续。
使用已接受修正的[交互与退出研究](../../../research/floor-world-interactions-and-exits.md)，依据
[Issue #930](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/930)及
[Standard 约定](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
止于条件目的地地图/战斗准入，在目的地实体群/init、隧道内容、Battle23 初始化或后续招募之前。
Fnnn 表示标志 nnn；索引从零开始。坐标、速度字节、等待为源码操作数，不是测得运动、像素或已过时间。

## 与居民交谈不推进巨环场景

**已确认：**Map26 只有一个默认配置。F779/F780 不选择不同居民、区域（事件）或物品表。
抵达指南负责九个实体群行及分配。仅 Peter/B 随从、没有跳过己方声明且查找一致互异时，
Randolf16、Tyrin17、128、129、130–132、133、134 占 physical 槽位 3–11。
这些是条件示例，不是稳定行动者身份。随从、受 HP/职业影响的精灵、跳过声明和别名仍是传入前提；
Randolf/Tyrin 出现不等于招募。

普通居民激活从 ProcessPlayerAction 使 physical0 空闲并等待其目的地和视野滚动开始。
A 打开场地菜单；C 在调试及重叠 physical1-Caravan 替代分支后到达此路径。
普通视野 target0 下，GetActivatedEntity 使用当前玩家位置/朝向，跨过桌子/柜台图块，
选择距候选点 Manhattan 距离小于 MAP_TILE_SIZE=384 的首个合格 physical 行动者。
它跳过被观察行动者/当前随从。负视野目标不准入激活；任意非零视野状态未确立为普通居民访问。

GetEntityEventIndex 反向搜索 ENTITY_INDEX_LIST，越过己方段后切换至 logical128，无匹配时回退 logical0。
别名可选择另一 logical 身份。激活返回以 d1 提供保存的行动者朝向，以 d2 提供玩家朝向。
RunMapSetupEntityEvent 将 d2 存入 EVENT_RELATIVE_POSITION，选择首个 logical-ID 匹配或默认。
因此此处该字节携带玩家朝向，不是区域（事件）坐标。

完整表及回调位于 `$59D6A..$59DB6` 的 `s2_entityevents.asm`。
下文 EventN 表示 Map26_EntityEventN。第二记录操作数为行动位域，尽管宏注释称其为 facing。
UP1 在一次 VInt 后将行动者转为 `(playerFacing+2)&3`，不还原保存朝向；DOWN3 转向并在返回后还原；
RIGHT0 两者都不做。它们不是必需接近方向。

| 居民；基线放置 | 记录 / 回调 | 完整有序回调工作 |
| --- | --- | --- |
| Randolf16；`(27,22)`，Right | UP1 / Event0 | txt1742；txt1743；返回 |
| Tyrin17；`(29,22)`，Left | UP1 / Event1 | txt1744；txt1745；返回 |
| Old man128；在 `(50,12)` 周围步行，radius3 | UP1 / Event2 | txt1746；txt1747；返回 |
| Young man129；`(5,24)`，Down | UP1 / Event3 | 无声返回 |
| Priest133；`(4,35)`，Down | DOWN3 / Event4 | 调用 j_ChurchMenu；服务返回后返回 |
| Caravan134；`(11,35)`，Down | RIGHT0 / Event5 | 调用 j_CaravanMenu；服务返回后返回 |
| 未匹配 logical 身份 | default0 / Event3 | 无声返回 |

Objects130–132 没有显式回调行。直接与 129 交互不启动巨环场景。
无声回调仍经过立绘、台词声音和朝向工作。包装器解析实际地图精灵，再次解析 physical 行动者，
在回调前清 VInt_UpdateEntities，返回时可选地还原朝向、关闭立绘/文本并重新激活更新。
它不添加行动者目的地等待。更早的 physical0 激活等待是独立条件。

### 对话提供背景，不授予许可或招募

全部六个选定对话 ID 来自 gamescript keys06CE–06D3。
没有局部对话标志，故会重复；每个回调都有相同有序两句。

| ID | 居民叙述；文本终止 |
| --- | --- |
| 1742 / 1743 | Randolf 质疑想见 Creed 的念头（W2），再警告不要进入通往他的墙洞（W1）。 |
| 1744 / 1745 | Tyrin 说下来是为逃离，却发现隧道内有魔族（W2），再说它们太凶猛，他要留在 Floor World（W1）。 |
| 1746 / 1747 | 老人说很久前离开 Desktop Kingdom（W2），再称 `{NAME;16}` 为勇敢且可信赖的领袖（W1）。 |

NAME16 是显式 ally16/Randolf 名称索引。文本使用方读取后续符号并调用 j_GetCombatantName；
不选择当前出战队伍领袖。这些回调均不加入己方、不更新出战队伍、不写标志、不授予物品、
不请求地图，也不写 EGRESS_MAP。其话语提供叙事背景，不证明早先逃离或离开许可。

GetEntityPortaitAndSpeechSfx 使用解析行动者的实际 MAPSPRITE 字节。
基线 Randolf sprites39–41 选择 portrait16/bleep6；Tyrin42–44 为 portrait17/bleep6；
old man198 为 NONE/bleep6；young man196 为 NONE/bleep2；priest192 为 portrait34/bleep5。
Caravan62 无匹配属性行，取得 DEFAULT/bleep6。NONE255 符号扩展为 -1，DEFAULT 亦然；均不请求新立绘。
已有立绘不证明新选中立绘。己方精灵或映射改变可改变这些属性；没有回调 HP 守卫修复它们。
NPC129 经 lookup129−96=33 解析。零缺失查找寻址 physical0，而非安全地拒绝缺失居民。

### 教堂与车队仍为调用及返回边界

Event4 调用 S05 别名 j_ChurchMenu `$20004` → ChurchMenu `$20A02`；
Event5 调用 j_CaravanMenu `$20010` → CaravanMenu `$21FD2`。
两个回调均不测试 F65、F780、行进模式或剧情前提。
Event5 不包括独立 C 按键 Caravan 快捷入口的缩小、音乐或视野更新工作。

[服务合同](../contracts/service-interactions.md)负责教堂和车队选择、取消及交易算法。
其受控教堂接缝不证明该 Map26 调用方的自然到达或成功返回。
局部无 EGRESS 写入不排除 Church Save 自身的 EGRESS 变更。
一次调用不确立治疗/存档/转职完成、仓库转移、出战队伍变化、持久性或控制还原。这些结果仍独立。

## 巨环尝试有自身准入与完成条件

**已确认：**`$59DB6..$59DD2` 的 `s3_zoneevents.asm` 选择
`msZoneEvent 14,255,Map26_ZoneEvent0` 加默认返回。
区域（事件）包装器比较 d1/d2 低字节，以 `$FF` 为通配符：X14、任意 Y。
这不表示瓦片 `(14,255)`、第 14 列的每个当前瓦片，或直接与年轻男子交谈。

esc02_controlCharacter 中，当前位置提供移动候选种子；方向、区域边界、斜坡及实体阻挡先于布局事件分类。
候选图块以 `$3C00` 掩码后必须等于 `$1400`，才写区域（事件）event6。
字参数由 `(XDEST+d4)/384` 和 `(YDEST+d5)/384` 导出，使用选定步进增量。
这些从目的地导出的坐标不同于当前位置读取和 EVENT_RELATIVE_POSITION。
写入先于其余地形/调试/碰撞工作及移动赋值；结构准入不确立已完成一步或到达。

WaitForEvent 优先处理既有非零事件，再处理 A/C 输入，存在待处理事件时跳过控制器安装。
ProcessMapEvent 在分发前清采样类型。Type6 在 physical0 安装 eas_Init，不等待完成，
从 PARAM1 加载 d1、PARAM3 加载 d2，再调用区域（事件）包装器。

| 选定回调检查时的状态 | 条件结果 |
| --- | --- |
| X14，F780 未设置 | 执行 cs_59F20；仅返回后才允许 setFlg780；返回 |
| X14，F780 已设置 | 返回，不运行新场景或 hide |
| 无特定行匹配 | 默认回调返回 |

没有 F779、成员资格、环持有或居民位置测试。传入未设置 F780 可准入不一致/缺失行动者；
传入已设置 F780 可在没有更早完成场景时抑制尝试。
检查不原子地预留场景，也不确立历史事件。

### 完整尝试：视野、话语、移动与隐藏

`$59F20..$59F42` 的 `cs_59F20`，scripts.asm lines4–13，含八个顶层命令及下文完整内联体。
脚本 trap 在 ExecuteMapScript 之前激活 VInt_UpdateEntities。

| 索引 | 完整命令 / 程序体 |
| --- | --- |
| 0 | textCursor1740 |
| 1 | setCamDest0,20 |
| 2 | nextSingleText modifier0, actor129：未跳过路径的 ID1740 |
| 3 | csWait50 |
| 4 | nextSingleText modifier0, actor129：未跳过路径的 ID1741 |
| 5 | entityActionsWait129：moveUp8；endActions |
| 6 | hide129 |
| 7 | csc_end |

textCursor 设置 CUTSCENE_DIALOG_INDEX。setCamDest 写视野目标 -1，将 0/20 传给 SetCameraDestination 并等待滚动。
该使用方乘以 384；SetViewDestination 应用区域视差/图层/背景偏移，仅在不受自动滚动控制的轴写目的地，
并调用滚动数据辅助。这请求视野；既不传送玩家，也不定义 129 原点。
实际区域状态、滚动完成及取景仍为前提，不是已观察表现。

文本操作数编码 `$0081`，不是 `$FFFF` 无说话者哨兵值。
Modifier0 不请求镜像或右侧放置。立绘/台词声音经实际 129 映射解析；
基线年轻男子不打开新立绘，选择 bleep2。csc1D 可保留已打开立绘，而 csc00 仍解析台词声音。
每句未跳过台词都等待滚动、显示游标文本、递增游标、关闭立绘/文本并 sleeps10。

ID1740/key06CC 为 `Ungh!{W2}`。ID1741/key06CD 说对象太重、他举不起来并放弃，以 W1 结束。
W2 和 W1 均通过 VInt 等待轮询方向/A/B/C 输入。
W2 还更新提示标记，并在接受时请求 SFX_VALIDATION；W1 不添加该请求。
csWait50 是 sleep，不是物理/行动完成条件或测得时长。

ExecuteMapScript 入口清 SKIP_CUTSCENE_TEXT。调试模式加 P2 Start 可在分发期间锁存它：
被跳过的 csc00 消费操作数，不显示或递增游标，脚本 sleep 也被绕过。
摄像机、移动、隐藏及调用方标志工作仍执行。两个文本命令之间发生跳过可改变后续游标/值；两句均非无条件可见。

csc2D 解析 129，以 physical 索引初始化计时器、清 FLAGS_A bits5/6，
将 moveUp8 编译为 esc04 相对 `(0,-8)`、esc01 目的地等待，以及追加的 esc34 跳至 eas_Idle。
`$FF` 等待字节仅轮询该行动者 idle 行动指针。没有 HP 守卫、超时或全行动者屏障。
esc04 将有符号增量乘以 384，并在执行时加至实际当前位置，不是实体群行或先前目的地。
Bit5 未设置绕过选定目的地冲突分支。目的地、行进及有符号速度分量使用继承的速度字节；场景不设置它们。
基线 eas_Init 已完成且未改变时提供 32/32，但区域（事件）对 physical0 的 Init 不是对 129 的 Init。
仅当前 `(5,24)` 未改变才产生请求 `(5,16)`；该端点和移动完成均非无条件。

esc01 比较行动者当前位置与当前目的地，不预留不变目标。
被替换指针/目的地、更新停滞或已经停放的原始 X≥`$7000` 均可改变或阻止进展。
一致、无中断状态下完成不证明时序或碰撞物理。
场景不实现环质量、举起、附着、力量测试或物品栏收取。失败举环的表现是由话语/背景作出的**推断**。

### 隐藏、后续停放与控制是不同边界

hide129 再次解析并调用 HideEntity：当前/目的地 X/Y 变为原始 `$7000,$7000`；
**全部**匹配 ENTITY_INDEX_LIST 条目变为 `$FF`；通过移动 EXPLORATION_ENTITIES 移除匹配随从列表条目。
它不授予物品、不招募，也不请求传送。别名改变可重定向隐藏，或使多个 logical 身份失效。
随后访问失效映射不是安全的缺失行动者无操作。

csc_end 在仍有对话窗口时有条件地等待滚动，清 VIEW_SCROLLING_SPEED 并还原调用方寄存器。
仅返回后才允许 F780。区域（事件）包装器随后关闭窗口，等待一次 VInt，并等**physical0** 当前位置等于当前目的地。
这独立于 129 行动等待，不汇合每个行动者。
探索循环 SetMoveSfx/WaitForEvent 边界仅在无待处理事件时允许 target0 及当前模式控制器；待处理工作优先。
单凭回调返回不确立可用控制。

抵达所有者的 F780 init 尾部则使用 MoveEntityOutOfMap → resolver → SetEntityPosition，
仅写当前/目的地 X/Y，保留查找/随从列表。后续显式全新 Map26 加载在该仅位置停放之前重建实体群。
同次访问 HideEntity 失效是另一状态；MAP_CURRENT 保留传入映射，包括已失效者。
未确立安全的隐藏到停放连续性。后续 init 执行不属本指南，其行为由抵达所有者保留。

返回前失败可留下摄像机/文本/行动/隐藏效果而 F780 未设置；
隐藏后、setFlg780 前失败，可准入另一次带失效查找的尝试。
反之，传入已设置 F780 可跳过对现有居民的隐藏。没有通用回滚或修复。
这些是静态状态及顺序区分，不是已观察中断或存档/读档结果。

## 空局部列表不确立普遍没有奖励

**已确认：**`s4_descriptions.asm` 仅返回；包装器从 d7=0 开始，故不提供已处理描述。
`s5_itemevents.asm` 只有默认返回；包装器从 d6=0 开始，因此局部回调不提供已处理物品使用、变更或请求。
包装器仍清台词声音、将物品调用方 d2/Y 存入 EVENT_RELATIVE_POSITION、关窗口并等待 physical0。
这不是该共享字节在居民交互中的朝向含义。

Map26 宝箱和其他物品列表各仅含 endWord。GetItem 看到负首字节，返回 -1 作为缺失坐标/标志/物品结果。
这些列表不提供局部物品记录；CheckArea 有其他依赖布局的路径，场地/菜单/物品服务各有所有者。
不能据此推导普遍没有搜索响应、持有物品、收集物或服务奖励。叙事中的环不是此场景授予的物品。

## 三个退出请求有各自目的地

**已确认：**`$A8CD2..$A8CEC` 的 `6-warp-events.asm` 恰含三个八字节记录及 endWord。
每个使用 warpNoScroll/type0。UP1 是目的地朝向，不是接近限制；全部触发均精确匹配。

| 记录 | 触发 X/Y | 请求地图 | 目的地 X/Y/朝向 |
| --- | --- | --- | --- |
| 0 | `(59,3)` | WILLARD_BURROW27 | `(2,7)`，Up1 |
| 1 | `(32,11)` | CURRENT255 | `(12,41)`，Up1 |
| 2 | `(12,42)` | CURRENT255 | `(32,12)`，Up1 |

普通 esc02 步行中，候选布局分类 `$1000` 调用 WarpIfSetAtPoint。
它将传入有符号原始 X/Y 除以 384，选择 CURRENT_MAP 传送表，止于首匹配或字 -1。
匹配写 event type1，将 type/map/X/Y 复制至 PARAM1–4、朝向/填充复制至 PARAM5–6，并请求 WARP_SFX=SFX_WARP。
站在列出瓦片不证明该使用方曾运行；布局、候选/行动和状态仍为前提。
其他载具路径共享匹配器，但这里不确立其移动合法性。

### 共享待处理请求先于输入被消费

请求存储可变，不是不可变的排队传送。WaitForEvent 优先处理非零类型；
ProcessMapEvent 在分发前清类型，使用方读取当时存在的参数。覆盖或传入请求可改变结果。

三个 PARAM1 值均选择 ProcessMapEventType1_Warp 的不滚动分支。
它请求存储声音、清 WARP_SFX、让 physical0 空闲、移除调用方返回地址以离开 ExplorationLoop，并清零 d0–d4。
它加载地图，然后 UpdatePlayerPosFromMapEvent 采样实际当前玩家 X/Y/朝向。
非负目的地字节仅替换 X/Y **低字节**；朝向总会加载，d4=0。有符号除法已经填充坐标字。
因此与表精确相等的对外元组需要一致的非负局部坐标：传入原始 X=-384 得商 `$FFFF`，
再写目的地 2 会产生 d1.w=`$FF02`，不是 `$0002`。Y 有同样限制。
该分支返回 MainLoop，不执行目的地 init；不同于非零 type 的滚动转换分支。

### 局部路线保留状态；Map27 准入有条件

MainLoop 先调用 SwitchMap，再 CheckBattle，然后选择探索/战斗。
没有标志切换行替换 26/27/255。CheckBattle 为查找通过 CURRENT_MAP 解析 MAP_CURRENT，
但无战斗返回保留原始 255 请求。CURRENT_MAP26 时，无匹配战斗坐标行；
两个局部记录均以 d0=255 准入探索，不是显式 Map26 重载。

该探索入场边界的 MAP_CURRENT 保留 CURRENT_MAP、实体群/查找和工作布局。
它跳过显式加载的瓦片集/实体分配、临时标志清除及 F80 设置，在共用入场工作和后续 init 分发前使用
UpdateMainEntityProperties 和 DeclareRaftEntity。行动者不重置至实体群行。
坐标前提一致时，主角色当前/目的地/朝向接收目的地值。
UpdateMainEntityProperties 测试各坐标低字节的负性，再将**整个字**乘以 384 并存低乘积字；
它不规范异常的保留高字节。因此 MAP_CURRENT 既不是全新实体群，也不是仅位置快捷路径。
它可保留同次访问失效映射。

Map27 有带通配触发的 Battle23 行。战斗解锁 F423 未设置则跳过，准入 Map27 探索；F423 已设置则选择战斗路径。
CheckBattle 的完成 F523 测试可清 423，而不取消当前选择。
这些仅为准入事实：居民、巨环或选定传送回调均不设置 423。
显式地图探索选择新地图/实体群、设置 CURRENT_BATTLE255，并在后续 init 前清临时标志 256–383；
F779/F780 在范围外。战斗准入有独立所有者。

选定传送、普通匹配器/使用方、SwitchMap 行或战斗谓词均不测试 F780。
因此其他输入相同时，F780 未设置或已设置都产生相同结构退出准入。
自然几何/可达性及后续目的地行为仍独立。这些退出既不消费环，也不消费 balloon，
不写 EGRESS_MAP、PLAYER_TYPE、已保存木筏状态或新剧情/招募标志。
本指南止于目的地实体群/init、隧道脚本及 Battle23 钩子、名册、激活或结果之前。
已接受后续 [Willard Burrow 探索与 Battle23 准入（中文）](willard-burrow-through-battle23-entry.md)
解释新实体群、局部穿行及独立获准对峙，直至 BattleLoop 调用。
它保留行动者/别名、坐标及待处理请求限制，止于战斗初始化或目的地 Floor World/Creed init 之前。

## 证据审阅与保留限制

**已确认溯源：**已接受 main `b9405e3d2649af6aee3ace4397ecbfa6faa9121e` 包含已修正 #924/PR927 和 #926/PR929。
研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
和 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
上文源码路径相对于 disasm；局部配置文件在 data/maps/entries/map26/mapsetups 下。
已接受[研究复现所有者](../../../research/floor-world-interactions-and-exits.md#reproduction-and-retained-limits)
保留直接源码关联和未改变载荷比较配方。本综合指南不增加提取、运行时观察或重制决策。

对抗审阅阅读完整修正研究、抵达指南及必需的
[交互状态](../../../research/map-event-interaction-state.md)、
[配置选择](../../../research/map-event-flag-route-selection.md)、
[请求消费](../../../research/map-event-request-consumption.md)、
[探索](../contracts/map-exploration.md)和服务边界。选定公共 fixture/schema/验证器所有者为：

| 声明 | 可执行所有者 / 选定边界 |
| --- | --- |
| 居民/区域（事件）/默认记录 | `sf2-map-events-static-v1:routing-setup`，tests/fixtures/h2/map-events/routing-setup.json：全部七个 Map26 实体行、两个区域（事件）行及默认物品行；schemas/h2/map-events-routing-setup.schema.json。 |
| 完整返回回调 | `sf2-map-events-static-v1:entity-programs`，entity-programs.json programOrder298–303；`:zone-programs`，zone-programs.json programOrder50/51；schemas/h2/map-events-entity-programs.schema.json、map-events-zone-programs.schema.json 和共享 map-events-target-program.schema.json。这些是语料身份，不是游戏顺序；src/sf2tool/h2/map_events.py 及 map_events_fixture.py 负责解析和分片验证。 |
| 朝向/寄存器与待处理事件边界 | tests/fixtures/h2 中的 `sf2-map-event-interaction-state-static-v1` 和 `sf2-map-event-request-consumption-static-v1`；匹配 schemas/h2/*-static-fixture.schema.json 及 src/sf2tool/h2/map_event_interaction_state.py / map_event_request_consumption.py。后者验证保留请求所有者和精确消费投影，不验证自然连续性。 |
| 场景命令/使用方 | `sf2-map-script-engine-static-v1`，tests/fixtures/h2/map-script-engine-static-v1.json：对话、摄像机、行动桥、生命周期及脚本控制事实；经 map_script_schema.py 使用 schemas/h2/map-script-engine-{dialogue,map-control,entity,script-control}.schema.json；map_script_engine.py `_verify_map_script_engine_fixture`。保留语料选择 cs_59F20 的八命令及完整内联体。 |

在研究具名接缝阅读固定的完整居民/区域（事件）/场景/传送源码及选定激活、包装器、摄像机/行动/隐藏、不滚动及准入使用方。
场景表和回调操作与更严格静态边界一致。保留修正的有符号商/低字节替换限制，不凭普通表值将其推断消除。
既有窄复现命令为 `uv run sf2 h2 map-events`、`uv run sf2 h2 map-script-engine`、
`uv run sf2 h2 map-event-interaction-state` 和 `uv run sf2 h2 map-event-request-consumption`；
汇总检查不独立守护每项局部正文或被调用方声明。当前审阅不再生成载荷，也不回放完整提取命令。

保留 [#924 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176361906)、
[修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176448210)、
[P2 审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176402766)和
[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176460244)。
收据保留独立原始/修正对象和 CI38622136093/38622845061、未改变 18,285,979 字节对比较/0.082s、
检查失败、源码探针修正及前篇未知。
[#926 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/926#issuecomment-6176550578)及
[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/929#issuecomment-6176569261)
保留 CI38623499458 及链接的 #920/#923/#919/#916 结果/失败。
这些链接记录仍具权威；本指南不替换已完成结果。

直接验收检查完整自有正文/diff、选定数量/顺序、链接/片段/表/代码围栏、溯源、英文/私有边界及
`git diff --check`，然后检查已配置的干净已提交
`uv run sf2 verify plan --base origin/main --head HEAD` 和实际最终 head CI。
不单因本正文选择局部一揽子 normal/full/engine/Godot/H3/H4 或翻译套件、获取、新 fixture/工具或载荷再生成。

**未知：**自然抵达/激活/区域（事件）/退出可达性；实际标志、布局、查找/别名、HP/职业/出战队伍、
行进模式、请求和当前/目的地状态；已完成服务、摄像机/文本/输入、运动、隐藏及可用控制；
可见巨环演出、时长/音频和存档/读档持久性。静态改变状态示例不是已游玩场景。
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)及
[ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)保留这些限制，不创建运行时队列。
这既未完成战役，也未完成设计 epic，不选择期望游戏玩法/UI 政策或重制范围。
