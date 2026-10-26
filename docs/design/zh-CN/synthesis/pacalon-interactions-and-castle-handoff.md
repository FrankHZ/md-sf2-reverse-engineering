# Pacalon：对话、补给与城堡交接

> 本文是[规范英文原文：Pacalon: conversations, supplies and the castle handoff](../../synthesis/pacalon-interactions-and-castle-handoff.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

Pacalon 提供两组居民对话、相同武器与物品商店、一名祭司、调查文本与两种可能的非宝箱拾取。北出口记录请求城堡；南出口记录请求大地图。**已确认：** 这些是独立本地互动与请求路径。出口记录不要求任何对话、购买、教堂服务或拾取。**未知：** 自然抵达玩家可触及哪些互动、哪些屏上完成，以及哪些持久保留。

本指南在 Map02 探索分支上接续 [Battle26 返回至有条件 Pacalon 入口](battle26-return-through-pacalon-entry.md)。前序可能的 Battle31 分流不提供城镇互动。止于离开无滚动传送向 MainLoop 返回操作数，早于目的地战斗选择、配置或初始化。解释已接受原版证据，不增加重制版行为或新观察战役前沿。

三个北向请求续见 [Pacalon 城堡：入口与首次旱灾状态访问](pacalon-castle-entry-and-first-visit.md)。该指南消费目的地准入/配置/初始化、默认城堡内容与 CURRENT 楼梯生命周期；本指南请求终点不变。

## 证据与阅读边界

**已确认的证据所有者：** [Pacalon 互动与城堡交接](../../../research/pacalon-interactions-and-castle-handoff.md)，通过 [#1021 / PR1023](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1023)在 `ac35fe192edba718a3261fef67d92874a477fd76` 接受，提供选定本地表、完整文本字面量与消费者连接。已接受前序 [PR1024](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1024)提供入口/人口前提。以下源码身份相对 SF2DISASM `master` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6` 的 `disasm/`；研究所有者保留 USA retail ROM 与源码/地址溯源。F530 指标志530，索引从零起，地址区间为左闭右开。

Standard 严谨级别适合此有界英文合成：当前表选择、保留行动者、服务提交与可变请求具有不同生命周期。[#1026](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1026)拥有其五个 Markdown 路径与直接验收。城堡内容、后续战斗、通用服务审计、源码/布局采集、运行时、工具与中文同步在该原切片范围外。并行城堡研究与 UI692 不提供证据。证据分歧返回研究所有者，不在此解释中修补。

## 找到居民并选择当前对话

**已确认：** `data/maps/mapsetups.asm` 选择 `ms_map2` 或其 F530 覆盖 `ms_map2_flag530`。六指针表在人口、实体事件与描述上不同；共享 zone 事件、物品事件与空初始化。`GetCurrentMapSetup` 每次读当前地图与选择器。因此人口、对话与调查未必使用同一 F530 值。本地回调不改 F530。

普通 C 互动在 `ProcessPlayerAction` 的 Idle 与移动/视野滚动等待之后；A 先走场地菜单分支。调试与重叠 physical1 Caravan 分支先于普通行动者查找。view target0 时，`GetActivatedEntity` 用位置/朝向，在 mask $3C00 下穿过布局 kind $2800 的桌台，按物理行动者顺序扫描距目标小于384 者。跳过被观察行动者与随从列表行动者。层读取不是层相等测试；负或其他摄像机状态不证明普通激活。

`GetEntityEventIndex` 随后从 `ENTITY_INDEX_LIST` 开头反向查找映射。32 个己方条目后，逻辑编号变128。首个别名胜出；失败返回0。若成功声明普通随从 n 个，固定行 j 通常以逻辑 `128+j`、查找偏移 `32+j` 映射到物理 `n+1+j`，Bowie 为 physical0。实际随从、别名、HP/职业、存储与保留映射仍是前提，不是保证阵容。

`RunMapSetupEntityEvent` 将所供玩家朝向 d2 存入 `EVENT_RELATIVE_POSITION`，选择当前配置，再取首个四字节逻辑匹配或 $FD 默认。回调偏移为相对选定表的有符号字。set530 表的向后偏移复用原服务/默认体，不是新服务。

立绘与台词 SFX 来自实际映射精灵。包装器预填 `CURRENT_SPEECH_SFX` 与 `CURRENT_PORTRAIT`，有条件打开立绘，再次解析行动者。下文全部具名行用 UP1：bit0 等一 VInt 并请求 `(playerFacing+2)&3`；bit1 未设，因此回调后不恢复保存朝向。UP 是行动掩码，不是必需接近方向。默认用0，不请求转向或恢复。包装器在回调前禁用 VInt_UpdateEntities，普通返回后关立绘/文本并激活该 VInt 函数。RTS 默认仍有包装器工作；映射失败不是与 Bowie 的有效对话或映射修复。

## 两种完整居民事件

**已确认：** `data/maps/entries/map02/mapsetups/s2_entityevents.asm` 中完整 clear530 表位于 `$5E938..$5E9C8`，含十具名行与默认。M/F 指声明的男性/女性 centaur 精灵233/234；商人193、祭司192 是不同精灵。原点与行走声明属于[入口人口表](battle26-return-through-pacalon-entry.md#城镇人口是有条件内容)，不把行动者冻结在原点。

| 逻辑 / 声明行动者 | Map2_ 中回调 | 完整本地工作与对话主题 |
| --- | --- | --- |
|128 /M|EntityEvent0|3583 →3584：问来客是否恶魔、如何安全抵达。|
|129 /M|EntityEvent1|3585：饥饿。|
|130 /F|EntityEvent2|3586：请强壮来客杀死恶魔。|
|131 /M|EntityEvent3|3587 →3588：称 Pacalon 骄傲士兵是躲在城堡的懦夫。|
|132 /F|EntityEvent4|3589：害怕失败。|
|133 /F|EntityEvent5|3590 →3591 →3592：问 Geshp，称他友好，想是否所有恶魔都如此。|
|134 /M|EntityEvent6|3593 →3594：问 Pacalon 能否赢，要求肯定回答，赞扬其战士。|
|135 /merchant|EntityEvent7|写武器商店索引10；调 j_ShopMenu。|
|136 /merchant|EntityEvent8|写物品商店索引25；调 j_ShopMenu。|
|137 /priest|EntityEvent9|调 j_ChurchMenu。|
|$FD /default|DefaultEntityEvent|仅 RTS。|

**已确认：** `s2_entityevents_530.asm` 完整 set530 表位于 `$5E9C8..$5EA20`，含九具名行与默认：

| 逻辑 / 声明行动者 | 回调 | 完整本地工作与对话主题 |
| --- | --- | --- |
|128 /M|Map2_212_EntityEvent0|3595 →3596：介绍优秀 centaur 王国并欢迎来客。|
|129 /M|Map2_212_EntityEvent1|3597：将降雨归于神的礼物。|
|130 /M|Map2_212_EntityEvent2|3598：雨后不再渴，仍饥饿。|
|131 /F|Map2_212_EntityEvent3|3599 →3600：问来客是否救了 Mitula、为何不击败恶魔。|
|132 /F|Map2_212_EntityEvent4|3601 →3602：问恶魔是否占领 Tristan，并将其与缺雨相连。|
|133 /M|Map2_212_EntityEvent5|3603：称常见 Geshp 凝视城堡。|
|134 /merchant|Map2_EntityEvent7|写10；调 j_ShopMenu。|
|135 /merchant|Map2_EntityEvent8|写25；调 j_ShopMenu。|
|136 /priest|Map2_EntityEvent9|调 j_ChurchMenu。|
|$FD /default|Map2_DefaultEntityEvent|仅 RTS。|

每个文本序列与服务调用有本地 RTS。17 个唯一回调体均不招募、不写剧情/成员标志、不安装过场、不提交地图请求，也不直接调 YesNoPrompt。3593–3594 的问题和所要求回答是文字，无选择结果分支。共享服务保留条件效果；这不使调用成为已完成事务或无副作用回调。

人口构建后改 F530 有实际意义。保留 clear530 阵容而选 set530 事件时，逻辑135 武器商人可调用物品店、136 物品商人调教堂、137 祭司走默认。保留 set530 阵容而选 clear530 时，134 调旧 Event6 文本、135 武器店、136 物品店。当前映射与选择解释这些可能；此处未观察自然标志转换。

**推断：** 干旱/焦虑与降雨恢复主题连接这些对话。居民关于 Geshp、神、士兵、Tristan 或 Mitula 的说法仍是对话，不是独立世界状态证明或旅行要求。降雨不消除所有表达的恐惧或饥饿。

研究所有者提供 gamescript 键0DFF..0E13 的全部21 个精确文本字面量。每组多文本首条以 W2 结束；3591 也以 W2 结束。全部末条以 W1 结束。居民字符串无姓名/物品替换。`txt` 发出 TEXTBOX trap 与文本字；`trap5_textbox.asm` 调 DisplayText。每次 DisplayText 将替换指针重置到 `DIALOGUE_NAME_INDEX_1`，解码选定 bank/Huffman 文本。连续 txt 调用不蕴含关闭或 sleep，也不增加过场文本游标。

[对话合同](../contracts/dialogue-system.md#轮询顺序与不同消费者)区分 W1 的 RNG 抽取/复制 →VInt 等待 →掩码输入测试与打字恢复，以及 W2 的抽取/复制/游标辅助/等待/输入循环、确认音效和清理。两者均非 YesNoPrompt。文本终止、CLEAR 与包装器 clsTxt 是独立操作。这些源码规则不增加 Pacalon 呈现、输入读取或时序观察。

## 两个商店与祭司

**已确认：** Event7 向 `CURRENT_SHOP_INDEX` 写字节 `SHOP_WEAPON_PACALON=10`；Event8 写 `SHOP_ITEM_PACALON=25`。两者均调 j_ShopMenu。`GetShopInventoryAddress` 跳过对应数量的长度前缀行；`PopulateShopInventoryList` 复制选定计数/物品。已接受 `data/stats/items/shopinventories.asm` 行如下：

| 索引 / 源码注释 | 有序目录（ITEM_ 前缀） |
| --- | --- |
|10 /Weapon shop11|BUSTER_SWORD, GREAT_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|25 /Item shop11|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

十/六种商品不是已授予物品或独立本地商店类型字节。两种事件复用相同目录。ShopMenu 有条件开当前立绘，请求158，关文本/立绘，进入购买/出售/修理/特卖选择。文本158 保证状况良好，以延迟命令 D3 结束，不是购买或确认。

[服务合同](../contracts/service-interactions.md#shop-源码边界)拥有共享门槛与顺序。选定物品定义/价格、金币、成员四槽容量、装备、不可售/珍稀/损坏标志与特卖库存均有关。购买/特卖在 AddItem 前扣金币；特卖随后移除特卖条目。出售先给金币再丢槽，并可将珍稀物品加特卖。修理先收费再修选定槽。可取消与继续菜单循环。目录选择不证明成功购买、装备变化或持久资源结果。

Event9 直接调 j_ChurchMenu，无本地首次访问警告或标志。ChurchMenu 有条件开立绘，请求含两个 W2 控制的110，关文本/立绘，抵达复活/治疗/转职/存档选择。接触不会自动治疗、存档或改 EGRESS_MAP。已接受 Raise/Cure/Save 观察是有界共享服务边界，不是自然 Pacalon 观察。

选定 Save 路径中，首个 YesNoPrompt 结果0 抵达 @DoSaveGame：将当时 CURRENT_MAP 字节（名义2）复制到 EGRESS_MAP，加载 CURRENT_SAVE_SLOT，设399，调 SaveGame。首结果非零避开存档写入，抵达 @ExitSave 文本112 “Do you have another desire?”（是否还需其他服务）：零循环到 @StartMenu 选另一服务，非零退出菜单。存档后，音乐/输入工作先于文本115/116 与独立继续冒险/挂起提示：零到 @ExitMenu，非零请求117 并到 FadeOutToBlack/WitchSuspend，而非普通回调完成。这些是实际选择分支，不同于居民要求的回答。

既有 Map02 存档点为 `(41,10)`、Up。它是返回数据，不在接触祭司时传送访客。已接受存档点消费者在399 下用表，399 未设时用初始回退。[Save 边界](../contracts/service-interactions.md#church-源码边界)观察受控用例中原 SaveGame 入口/返回与受控 Fade 返回/WitchSuspend 入口。既不提供 Pacalon 存档完成观察，也不提供跨进程持久性。

## 查看招牌、雕像与井

**已确认：** 两个区域描述包装器各16 字节，之后七条六字节行与两字节 $FD00 终止符。clear530 为 `$5EA26..$5EA62`，set530 为 `$5EA62..$5EA9E`；均用 base $1030=4144。每条本地行 condition0、text payload-kind0。完整有序两表仅井的描述偏移不同：

| 行 / 坐标 | 调查偏移 →ID | Clear530 描述偏移 →ID / 内容 | Set530 描述偏移 →ID / 内容 |
| --- | --- | --- | --- |
|0 /8,13|2 →425，招牌|0 →4144，宣传内售武器|0 →4144，相同|
|1 /15,10|2 →425，招牌|1 →4145，宣传内售工具|1 →4145，相同|
|2 /17,14|9 →432，雕像|2 →4146，优秀士兵|2 →4146，相同|
|3 /17,18|9 →432，雕像|3 →4147，优秀士兵|3 →4147，相同|
|4 /25,18|9 →432，雕像|4 →4148，优秀士兵|4 →4148，相同|
|5 /6,5|5 →428，井|5 →4149，`A sturdy well.{N}No water there.{W1}`（结实的井，里面无水）|6 →4150，`A very deep well.{W1}`（很深的井）|
|6 /25,14|9 →432，雕像|7 →4151，优秀士兵|7 →4151，相同|

调查文本425/428/432 在最终 W1 描述前用 NAME、W2 与 CLEAR 描述招牌/井/雕像。`CheckArea` 将 DIALOGUE_NAME_INDEX_1 与台词 SFX 清0：NAME 选成员0 姓名，不是被激活居民。井的措辞变化不授予水，也不写 F530；本地描述均无函数载荷或选择。

普通 view target0 下，调查先求前方瓦片。布局 kind $1800 宝箱、$2C00 花瓶、$3000 木桶、$3400 书架与 $1C00 区域物品先于描述包装器。坐标不建立已解码物体、碰撞或可行走调查路径。普通无实体 C 时 d6=1。`DisplayAreaDescription` 按序扫描压缩坐标；非零 condition 要求 d6=0，这些 condition0 行仍符合。首个获准行显示423+调查偏移，再显示4144+描述偏移，关窗口并返回 d7=-1。无匹配返回 d7=0；d6=1 的 CheckArea 返回0，允许场地菜单回退。所供 d6=0 在无匹配时改选通用423/412。无激活行动者、默认实体回调与无匹配描述是不同结果。

## 两种可能拾取与失败边界

**已确认：** `7-chest-items.asm`、`$96A7E..$96A80` 仅 endWord。其他物品表 `$96A80..$96A8A` 含两条四字节行与 endWord：

| 候选坐标 | 收集标志 | 物品符号 / 索引 |
| --- | --- | --- |
|33,6|172|ITEM_CHEERFUL_BREAD /13 ($0D)|
|27,24|173|ITEM_MEDICAL_HERB /0|

标志在临时256–383 外，独立于 F530。空宝箱数据不禁用通用调查。两坐标本身既不标识物体放置，也不标识物品使用、交付或行走路径。`CheckArea` 仅经符合布局分支到 `CheckNonChestItem`。`GetNonChestItem` 用当前地图偏移38 的其他物品指针；`GetItem` 按行序比较坐标低字节。变化 CURRENT_BATTLE≠255 的裁剪路径保留固定源码错误 stride5，而非七字节战斗坐标记录大小。不假定修正查找。匹配返回原坐标布局偏移、标志与物品；负终止符/无匹配返回 -1。

`CheckNonChestItem` 对已持有收集标志替换 ITEM_NOTHING。两个匹配路径均在返回前设标志，因此未设172/173 会在容量或 AddItem 成功前提交。物品13/0 走普通物品分支，不是金币宝箱。调用方设物品替换索引2，请求413，再试成员0 四槽物品栏。满时 UpdateForce 构建已加入军团列表，有序 OTHER_FORCE_MEMBERS_LIST 回退尝试有空位成员。不限出战队伍，也无 HP 门槛。列表从 TARGETS_LIST 后一字节开始，假定已加入 Bowie 为首：变化为 Bowie 未加入时仍单独尝试0，但回退省去实际首个已加入成员。

首个容量匹配时调用 AddItem，保留标志，请求物品音乐与 Bowie 文本415，或带收取者姓名索引3 的416，再到 FadeOut_WaitForP1Input。413 用 W2/CLEAR；415/416 均无 W1 或 W2，因此不能将调用方独立音乐/普通输入边界发明成文本等待。收面包/药草不应用属性/治疗使用。AddItem 写首空槽，无槽则返回1；此调用方不测试返回 d2。容量检查后物品栏变化不证明插入成功。

全部满时请求417，调用 CloseChest 与保存候选坐标的 RefillNonChestItem。Refill 重找行并清标志，允许重试。已持有标志/无物品选空物体文本，不重复授予。物品0 是 Medical Herb；127 是无物品哨兵值，也可由无匹配 -1 掩码得到。AddItem/refill 前中断可留下有标志无物品；AddItem 后可留下已变物品栏而表现未完成。收集不是原子事务。

两配置的 zone 与物品使用表各仅默认 RTS。zone 包装器仍关窗口并等一 VInt/physical0 目的地。物品使用包装器清台词，掩码所用物品，将 player-Y 存 EVENT_RELATIVE_POSITION，初始化 d6=0 并清理/等待。默认返回把通用场地菜单处理留给既有所有者。物品使用与其他物品收集不同；两默认均不产生传送。

## 向北去城堡，或向南去大地图

**已确认：** `6-warp-events.asm`、`$96A5C..$96A7E` 存四条八字节记录，之后 endWord。全为 warpNoScroll/type0；warpFacing 发出朝向字节与零填充。精确记录先于北向通配：

| 顺序 | 源 X,Y | 请求地图 | 目的地 X,Y | 朝向 |
| --- | --- | --- | --- | --- |
|0|19,0|MAP_PACALON_CASTLE=36|20,30|Up1|
|1|23,0|MAP_PACALON_CASTLE=36|22,30|Up1|
|2|255,0|MAP_PACALON_CASTLE=36|255,30|Up1|
|3|255,31|MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT=71|22,16|Down3|

`esc02_controlCharacter` 在 `WarpIfSetAtPoint` 前要求掩码布局 kind $1000；筏与 Caravan 控制有独立 $1000 调用位置，各有移动/碰撞准入。生产者符号扩展内部坐标字，除384，扫描当前地图行。字 $FFFF 结束扫描；负源字节为通配，否则低字节必须匹配。首匹配胜出。因此符合19,0 与23,0 先于北通配选精确目的地 X20/22；Y0 其他获准 X 选择保留 X。Y31 走南向。无匹配保留既有待处理状态。这些行均不测试 F530、对话、服务、收集或城堡剧情状态；通配不证明每个边缘单元格都是可达传送瓦片。

生产者向 MAP_EVENT_TYPE（$FFA84A）写字 MAP_EVENT_WARP=1，再向 PARAM1..4（$FFA84C..$FFA850）写长字 type/map/X/Y，再向 PARAM5 及后续字节（$FFA850..$FFA852）写字 facing/padding，最后向 WARP_SFX 写字 SFX_WARP。事件类型先于参数，无已有事件守卫或排队不可变快照。写入中断或替换可改变实际消费载荷。

WaitForEvent 在 A/C 轮询前读待处理事件字。零允许 physical0 控制安装：numeric mode0 角色、1 筏、其他非零 Caravan；非零跳安装。ProcessMapEvent 在分发已读 d0 前清存储 type。type1 且 PARAM1 零时，无滚动消费者：

1. 提交 WARP_SFX，清该字，使 physical0 Idle。
2. 移除探索返回地址，清 d0..d4 低字，将 PARAM2 地图字节载入 d0。
3. 采样 physical0 当时 X/Y 字与朝向；符号扩展并除384 得消费时坐标字。
4. 分别读目的地 X/Y。负字节保留各采样坐标字；非负只替换其低字节。变化高字节保留。因此行2 的255 保留采样 X，不是冻结触发坐标。普通较小正种子给出表中普通目的地；这不是通用整字规范化。
5. 用 PARAM5 字节替换朝向，读 PARAM1 到 d4，RTS 回 MainLoop。未变载荷产生 d0=36/71、d3=1/3、d4=0。

朝向是输出操作数，不是城镇行动者已完成转向。参数在音效/Idle/采样前后重读，因此未必仍匹配最初选中行。无滚动本身不写目的地 CURRENT_MAP，也不构建人口。

**这就是终点：** MainLoop 后续 @Start 会先 SwitchMap，再 CheckBattle/配置/初始化，但目的地路径在本指南外。已接受切换规则之后可依新 F530 读取替换71→78；已提交南向请求仍是71。未建立城堡准入、南向过渡完成或可用目的地控制。

## 变化状态与保留审阅

| 变化前提或中断 | 必需解读 |
| --- | --- |
|530 在人口/事件/描述读取间改变|当前路由可与保留精灵不一致；无自动重新标记/重建。|
|别名、随从、HP/职业、存储或映射不同|物理激活与首反向别名决定；行公式有条件。|
|摄像机、移动、桌台/布局或模式不同|不保证抵达/激活；UP1 转向不恢复。|
|行动者缺失、选默认或描述无匹配|包装器、CheckArea 与场地菜单结果各异。|
|金币、容量、装备、特卖、取消或服务中断不同|入口/目录不是已完成资源事务。|
|教堂拒绝、Save、存档后继续/挂起不同|只有已到达存档分支写 EGRESS_MAP/399；持久性仍有界。|
|描述 kind/坐标/d6 不同|布局优先与首个获准行先于显示文本声明。|
|172/173 已持有、全满或加入列表不同|重复、成员0/回退与已到达 refill 决定；不保证交付。|
|容量读取后物品栏变化或拾取中断|未测试 AddItem 结果及标志/物品/表现顺序不是原子事务。|
|精确北、通配北、南或无匹配不同|有序行选择与待处理保留仍独立。|
|负字节、高字节或玩家运动不同|源通配、消费采样与低字节写入不同。|
|待处理替换或下一 MainLoop 前改 F530|可变请求与之后切换不承诺一种目的地结果。|

合成前审阅消费完整已接受 Pacalon 研究、前序入口指南、更窄[互动](../../../research/map-event-interaction-state.md)、[对话](../../../research/map-event-dialogue-state.md)、[请求写入者](../../../research/map-event-request-state.md)、[请求消费者](../../../research/map-event-request-consumption.md)、[Ketto 共享服务/拾取路径](../../../research/ketto-interactions-and-merchant-relocation.md)，以及服务/对话/描述合同。直接检查选定公共载荷、schema 与实际验证器消费者：

| 公共测试夹具与消费者 | 已检查边界与限制 |
| --- | --- |
|`sf2-map-events-static-v1:entity-programs`；[entity-programs.json](../../../../tests/fixtures/h2/map-events/entity-programs.json)、[map_events.py](../../../../src/sf2tool/h2/map_events.py)|全部17 本地体、有序文本操作数、服务别名目标与返回。程序体本身不证明表偏移、自然激活或实际显示。|
|`sf2-map-event-interaction-state-static-v1`；[map_event_interaction_state.py](../../../../src/sf2tool/h2/map_event_interaction_state.py)|分发/复用朝向与物品 Y 输入、实际获取/反向映射/包装器范围。聚合计数不证明 Pacalon 调用方状态。|
|`sf2-map-event-dialogue-state-static-v1`；[map_event_dialogue_state.py](../../../../src/sf2tool/h2/map_event_dialogue_state.py)|实际分发查找/预填与到达定义消费者；选定语料证据不提供全部本地文本或自然表现。|
|`sf2-map-event-request-state-static-v1` 与 `sf2-map-event-request-consumption-static-v1`；[请求状态](../../../../src/sf2tool/h2/map_event_request_state.py)、[消费](../../../../src/sf2tool/h2/map_event_request_consumption.py)|本地商店写入/别名交接、实际字节商店读取、待处理字读取/清除/分发上下文。有界访问形状不证明完成服务，也不进入完整传送被调用方。|
|`sf2-common-menus-static-v1`；[menus.py](../../../../src/sf2tool/h2/menus.py)|实际商店变更/容量/价格守卫与教堂 Save 操作/提示极性。选定本地目录与存档点仍是已接受研究连接，不是聚合菜单事实。|
|`sf2-map-descriptions-static-v1`；[map_descriptions.py](../../../../src/sf2tool/h2/map_descriptions.py)|六字节/终止符/首匹配/文本基址/d6 事实与实际消费者守卫。公共投影不暴露两张本地表或已解码物体布局。|
|`sf2-church-save-lifecycle-runtime-v1`；[church_save_lifecycle.py](../../../../src/sf2tool/h3/church_save_lifecycle.py)|五个已接受受控拒绝/继续/挂起用例与时序。不推出 Map02 自然存档、持久存储或完整 Witch 生命周期。|

所属命令为 `uv run sf2 h2 map-events`、`map-event-interaction-state`、`map-event-dialogue-state`、`map-event-request-state`、`map-event-request-consumption`、`common-menus`、`map-descriptions` 与 `uv run sf2 h3 church-save-lifecycle`。审阅了选定公共表面；未重跑私有源码比较或运行时命令。未发现阻塞分歧。未来矛盾在此指南扩展前交研究所有者。

[#1021 冻结交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1021#issuecomment-6182880836)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1023#issuecomment-6182901348)保留干净固定源码/表/文本/消费者审阅、作者与独立0.091s 结果、之前0.090s 通过、精确提交头 CI38669951857，以及已修正私有路径正则/源码路径/文件名/分类检查。未变18,285,979 字节配对的已接受0.083s 回执仍按溯源复用，不再生成。全部所链 #1017/#1018 回执、已完成失败与可选压缩布局限制保留。

[#1022 修正冻结交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1022#issuecomment-6182928256)与 [PR1024 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1024#issuecomment-6182986240)保留 Map71 与 flag71 区分修正、精确最终 head5dd17992/treeb78c4384/baseac35fe19、已配置0.905s/独立0.878s 检查、最终 CI38670343968、独立早期 CI 与原诊断。此设计环境中私有源码与配对仍不可用；不由此采集、复制、新比较或重置完成套件。

此原切片 Issue 交接记录完整文字/变化状态/五路径审阅、选定 schema 与消费者连接、链接/锚点/编码/私有检查、git diff --check、已配置干净提交头 planner 与实际精确提交头 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4 与翻译套件。**未知：** 自然抵达/布局、所供标志、映射/随从/HP/职业/模式、物品栏/金币/待处理状态、已完成服务/拾取/旅行、表现/时序、可用控制与持久性。这些限制不创建运行时队列或推进重制版验收。
