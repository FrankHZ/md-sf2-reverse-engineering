# Elven Village 对话与局部探索

> 本文是[规范英文原文](../../synthesis/elven-village-conversations-and-local-exploration.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Elven Village 提供关于 仙女 与 Elric 的对话、可搜索的书籍和物品、一名 祭司 及一对内部楼梯。这些机会的后果不同。居民选定对话体读取或描述剧情状态；它们不招募 Elric，也不开通前往 Creed 的路线。获准物品搜索可改变物品栏及收集标志。楼梯在当前地图内移动，同时保留已有居民与工作布局。

**已确认：**已接受证据确立这些静态选择及紧接的使用方。**推断：**它们构成可选的村庄对话与探索段落。**未知：**自然行程、实际调用方状态、服务成功、表现完成与响应输入。下述坐标标识内容和条件触发，不是攻略或重建的地形布局。

这份实现中立的英文指南使用已接受的[局部研究](../../../research/elven-village-conversations-and-local-exploration.md)及[抵达指南](battle19-outcomes-through-elven-village-entry.md)。按 [Issue #863](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/863)，交互、物品栏、请求消费与地图重载分别有所有者，适合 Standard 严谨度。止于获准教堂调用及条件返回，或局部对话、搜索、楼梯之后的条件 Map23 控制。排除外部出口、Map24/Battle20、Fairy Cave/Creed 旅行、Elric 救援或招募、F762 生产方、物品使用／转职及教堂事务。本指南不选择重制规则、运行时采集或中文同步。

## 选择对话

**已确认：**Map23 默认选择 `ms_map23`，传入 F762 已设置时选择 `ms_map23_flag762`。两种配置仅实体及实体事件目标不同；区域、描述、物品及 init 目标共享。F13 选择一个对话分支，不选择实体生成。全新配置给 NPC 分配从 128 开始的逻辑 ID，独立于随从之后占用的物理槽位。

普通 `ProcessPlayerAction` 使主实体 0 空闲并等待移动／视野滚动完成。A 走场地菜单路径。C 在独立 debug 和重叠 Caravan 分支之后到达交互。`GetActivatedEntity` 要求视野目标非负，按其朝向计算候选位置。桌子／柜台图块可延长候选距离。物理扫描跳过被观察实体和当前随从，再接纳与候选位置曼哈顿距离小于 384 内部单位的首个条目。因此 NPC 初始坐标不证明某种接近有效。

`GetEntityEventIndex` 通过 `ENTITY_INDEX_LIST` 解析物理条目；无匹配则回退至 0。`RunMapSetupEntityEvent` 选择首个匹配逻辑 ID 行，否则使用 `$FD` 默认行。它将玩家传入朝向存入 `EVENT_RELATIVE_POSITION`，经 `GetEntityPortaitAndSpeechSfx` 获取立绘／SFX 状态，并暂停实体更新后调用处理器。实际行动者映射、立绘、台词和动作完成仍有条件。

### 行动作位不是接近方向

第二个 `msEntityEvent` 操作数是动作位字段，尽管宏注释称它为朝向且源码使用类似方向的名称。UP 是字节 1；DOWN 是字节 3：

| 行字节 | 选定动作 |
| --- | --- |
| UP /1 | 位 0 在一次 VInt 后将行动者转向 `(playerFacing+2)&3`；位 1 未设置，故分发器不恢复已保存朝向。 |
| DOWN /3 | 位 0 以同样方式转向行动者；位 1 在处理器返回后恢复已保存朝向。 |
| 默认 /0 | 这些位均不请求转向或恢复。 |

这些值不要求 Bowie 从 Up 或 Down 接近，也不替代 NPC 初始朝向。返回路径关闭立绘／文本并恢复实体更新；祭司 路径仅在教堂调用返回时到达此清理。

### 默认居民及其字面对话

**已确认：**下列行将默认实体群连接到 `Map23_EntityEventN`，位于 `s2_entityevents.asm`。放置和朝向描述初始行，不是永久位置。下文转述已接受注释，不是观察到的显示对话。

| 逻辑行动者／初始放置 | 动作字节／处理器 | 有序内容 |
| --- | --- | --- |
| 128 MAN、`(18,9)` Down、行走范围 1 | UP /1、Event 0 | 1690 欢迎停下休息的旅行者（`{N}`、`{W2}`）；1691 允许想住多久就住多久（`{N}`、`{W1}`）。 |
| 129 DWARF、`(17,11)` Right、固定 | DOWN /3、Event 1 | 1692 说附近住着一名 仙女，并问听者是否认识她（`{N}`、`{N}`、`{W1}`）。 |
| 130 OLD_MAN、`(18,11)` Left、固定 | DOWN /3、Event 2 | 1693 询问 仙女（`{W2}`）；1694 说她尚未从 Creed 的宅邸回来（`{N}`、`{W1}`）。 |
| 131 WORKER、`(54,3)` Down、固定 | UP /1、Event 3 | 1695 说只有 仙女 知道前往 Creed Mansion 的路（`{N}`、`{N}`、`{W2}`）；1696 提议她的朋友 `{NAME;13}` 也许知道（`{NAME;13}`、`{N}`、`{N}`、`{W1}`）。 |
| 132 ELF_FEMALE、`(8,12)` Right、行走范围 1 | UP /1、Event 4 | 读取 F13，选择下述两行分支。 |
| 133 PRIEST、`(61,3)` Down、固定 | UP /1、Event 5 | 调用 `j_ChurchMenu`，服务返回才返回；无局部文本序列。 |

Events 0–3 为所列文本调用后返回。矮人 的问题是文本，不是 `YesNoPrompt` 或回答可改变标志的选择。提及 仙女、Creed 或 Elric 不接纳旅行、不证明这些事件，也不产生剧情变更。

### F13 改变 精灵 的措辞，不改变 Elric 成员资格

**已确认：**Event 4 执行 `chkFlg 13` 和 `bne.s byte_59B94`。未设置则顺序进入 1697/1698 后分支至返回；已设置则选择 1699/1700：

| 传入 F13 | 有序内容 |
| --- | --- |
| 未设置 | 1697 说 `{NAME;13}` 外出寻找 仙女，以帮助生病的 矮人（`{NAME;13}`、`{N}`、`{N}`、`{W2}`）；1698 警告西边森林危险（`{N}`、`{W1}`）。 |
| 已设置 | 1699 描述森林 精灵 报恩（`{N}`、`{W2}`）；1700 告诉 `{LEADER}`，`{NAME;13}` 会帮忙（`{LEADER}`、`{N}`、`{NAME;13}`、`{W1}`）。 |

已接受 common-stats 源码连接中，ally 13 是 Elric，F13 是其加入标志。此处理器只读取它，不加入 Elric、不设置 F13、不带回 仙女、不治疗 矮人，也不写地图请求。替换项需要传入姓名状态。`{N}`、`{W1}` 和 `{W2}` 保留控制身份；此段既不确立帧时长，也不确立按键推进已完成。

### 传入 F762 的居民

**已确认：**`s2_entityevents_762.asm` 绑定四个替代实体生成行。其三个文本体返回，不检查 F13，也不变更成员资格、标志或请求。

| 逻辑行动者／初始放置 | 动作字节／处理器 | 有序内容 |
| --- | --- | --- |
| 128 MAN、`(18,9)` Down、行走范围 1 | UP /1、`Map23_2FA_EntityEvent0` | 1701 询问听者是否见过 Creed（`{N}`、`{W1}`）。 |
| 129 WORKER、`(56,6)` Down、行走范围 1 | UP /1、替代 Event 1 | 1702 说森林的邪恶已完全消散（`{N}`、`{W1}`）。 |
| 130 OLD_MAN、`(18,11)` Up、固定 | DOWN /3、替代 Event 2 | 1703 问生病的 矮人 是否好些（`{W2}`）；1704 问他现在能否看见 仙女（`{W1}`）。 |
| 131 PRIEST、`(61,3)` Down、固定 | UP /1、默认 Event 5 | 相同教堂调用与条件返回。 |

这些陈述是说话者内容，不是已经拜访 Creed 或 矮人 康复的证据。F762 生产方不在范围内。分发重新解析传入配置状态；既有实体群可在当前地图楼梯时保留，不按已改变标志重建。Logical 131 是默认 工人，却是替代 祭司。应使用获准逻辑／物理映射，不能假设精灵名或旧配置确定处理器。

`Map23_DefaultEntityEvent` 仅为 `rts`，由替代表共享。两个配置还共享仅返回的 `Map23_DefaultZoneEvent` 和 `Map23_DefaultItemEvent0`，这两张表均无选定特定行。物品使用默认处理器与宝箱／非宝箱搜索不是同一分发器：它既不授予 Secret Book，也不证明下游通用物品使用无效果。

祭司 的别名连接为 `j_ChurchMenu`（`0x20004`）到 `ChurchMenu`（`0x20A02`），位于 `code/common/menus/church/churchactions_1.asm`。**未知：**服务返回、存档点改变、治疗、复活、转职、价格及完成事务。本指南止于调用与条件返回，不引入这些服务结果。

## 搜索先选择种类，再选择描述

**已确认：**C 首先尝试实体；获准实体处理器消费该交互，不搜索。无激活实体时，以 d6=1 调用 `CheckArea`。FieldMenu 搜索行动则独立于该实体查找，以 d6=0 到达 `j_CheckArea`。

搜索要求视野目标非负，清除 `DIALOGUE_NAME_INDEX_1` 与台词 SFX，并按方向偏移定位面前一个瓦片。它将坐标除以 384，索引宽度 64 的工作布局，以 `$3C00` 掩码其字。它不会像 NPC 激活那样跨桌子／柜台图块重复。实际视野目标、姿势和工作布局种类是必需输入。

| 掩码后种类／分发顺序 | 使用方 | 初始／空文本 ID |
| --- | --- | --- |
| 宝箱 `$1800` | `OpenChest` | 403 /408 |
| 花瓶 `$2C00` | `CheckNonChestItem` | 404 /409 |
| 木桶 `$3000` | `CheckNonChestItem` | 405 /410 |
| 书架 `$3400` | `CheckNonChestItem` | 427 /412 |
| 可搜索区域 `$1C00` | `CheckNonChestItem` | 423 /412 |
| 其他任意掩码值 | `j_RunMapSetupAreaDescription` | 匹配描述（若有）。 |

每个物品种类分支均经物品交付或空结果及共享关闭结束，不会落入描述。已处理描述返回 d0=-1。无描述匹配时，d6=1 返回零，使普通 C 调用方可打开菜单；d6=0 输出 423 再 412 并返回 -1。较早区域描述合同关于唯一调用的讨论覆盖其无实体 d6=1 路径；后来的场地搜索所有者明确记录 FieldMenu 对 `CheckArea` 的普通 d6=0 调用方。此静态调用连接不确立条件回调的自然执行。

### 三个描述行

**已确认：**共享 `s4_descriptions.asm` 使用描述基址 `$1015` /4117 及表 `byte_59BE8`。每个六字节 `msDesc` 行的条件和函数种类字节均为零，调查偏移为 4，描述偏移如下。这三行没有 d6 限制或回调。

| 坐标 | 偏移／文本 ID | 字面主题 |
| --- | --- | --- |
| `(5,20)` | 0 /4117 | 一本题为 AN APPLE A DAY 的书。 |
| `(51,2)` | 1 /4118 | 一本题为 HOME-MADE POTIONS 的书。 |
| `(52,2)` | 2 /4119 | 一本题为 FAIRIES 的书。 |

`DisplayAreaDescription` 取首个压缩 X/Y 匹配，输出调查 `423+4=427`（`{NAME}`、`{N}`、`{W2}`、`{CLEAR}`），再输出 `4117+offset`（每个书名有 `{N}`、`{W1}`）。它关闭立绘／文本并返回 d7=-1；无匹配保留 d7=0。包装器在恢复寄存器前检查 d7，保留 `CheckArea` 使用的条件码。这些是可调用内容行，不是每次搜索坐标都保证产生的结果。

### 为什么 `(52,2)` 有两行，却只选择一个结果

在 `(52,2)`，获准书架种类搜索选择 other-item 表。F158 未设置则选择 SECRET_BOOK；F158 已设置则报告 427 再 412。物品栏全满走下述回滚。无论此次调用还是自动重试，均不显示 4119。其他非宝箱物品种类具有相同优先级，使用各自初始／空文本。宝箱种类查找在此无宝箱行，给出其空结果。

只有绕过全部五个物品分支的工作布局种类才允许匹配 427/4119 描述。此路径既不查询也不设置 F158。**未知：**自然图块值、实际接近、收集或后续布局改变。两个坐标匹配不表示一次搜索产生两个效果，也不表示收集 Secret Book 后下次搜索就显示 FAIRIES 描述。

## 物品可用性、收件人选择与回滚

**已确认：**四字节 `mapItem` 格式存储 X,Y,flag byte,item byte。`7-chest-items.asm` 和 `8-other-items.asm` 各有两行，后接 `endWord`：

| 表／从零开始行号 | 坐标 | 收集标志 | 物品字节／身份 |
| --- | --- | --- | --- |
| Chest 0 | `(29,27)` | F202 | `$7B` /123 MITHRIL |
| Chest 1 | `(2,3)` | F221 | `$7A` /122 VIGOR_BALL |
| Other 0 | `(24,9)` | F157 | `$05` /5 FAIRY_POWDER |
| Other 1 | `(52,2)` | F158 | `$79` /121 SECRET_BOOK |

标志字节已经是游戏标志索引。不要加宝箱基址或换成行号。四个物品均不同于 ITEM_NOTHING 127 且低于金币阈值 128。仅凭名称不能确立使用或转职效果；other-item 表也不确定每个自然布局种类。

`GetChestItem` / `GetNonChestItem` 选择当前地图表，`GetItem` 取首个 X/Y 匹配。获准 Map23 探索为 CURRENT_BATTLE=255，故跳过战斗坐标调整。行 X 为负则终止；无匹配返回 -1，后续 `$7F` 掩码将其归为 ITEM_NOTHING。传入战斗模式输入不属于此村庄探索合同。

`OpenChest` 在匹配收集标志已设置时替换为 ITEM_NOTHING。匹配宝箱设置该标志，并将工作布局字替换为打开宝箱 `$D802` /55298，请求所选平面更新。关闭 `$D801` /55297 与打开宝箱均掩码为 `$1800`；重复仍可选择宝箱分支及其 403/408 结果。无匹配行则无标志／图块变更。`CheckNonChestItem` 同样检查／替换并设置匹配标志，但不改变布局。标志保持设置时，获准重复既不能再授予一份，也不能落入描述。

### 先领队，再已加入成员

非空物品时，`loc_239EE` 设置其名称参数并输出 413（`{NAME}`、`{N}`、`{ITEM}`、`{W2}`、`{CLEAR}`）。它检查领队索引 0，物品栏容量为 4。有空位则调用 `j_AddItem`、请求物品音乐、输出 415（`{NAME}`、`{N}`、`{ITEM}`；无注释 W-token），再调用 `FadeOut_WaitForP1Input`。领队满时，经 `UpdateForce` 重建军团列表，扫描 `OTHER_FORCE_MEMBERS_LIST`，使用计数器 `TARGETS_LIST_LENGTH-2` 与 DBF，给首名有空位成员。它设置 name 3，请求相同音乐，输出 416（`{NAME}`、`{N}`、`{ITEM}`、`{NAME}`），使用相同等待后共享关闭／返回 -1。

该列表是按己方索引排序的已加入成员，不是出战成员或可见村庄随从。此搜索无 HP 资格检查。OTHER_FORCE_MEMBERS_LIST 别名为 TARGETS_LIST+1，普通 Bowie 已加入状态下跳过 Bowie。传入缺失 Bowie 标志时，仍先检查 leader 0，重建列表仍跳过首个条目；调用方不引入成员资格修复。因此在获准静态扫描中，后备或当前不可见的已加入成员也可能符合条件；实际收件人状态仍未知。

槽位按掩码物品 ID 是否为 127 计数。`AddItem` 写首个空槽位，保留索引／损坏位并返回 d2=0；无槽位则返回 d2=1。搜索调用方在容量检查后不按该返回分支。成功交付以此边界之间物品栏稳定为条件；源码不确立先前空位检查与 AddItem 结果不同时的恢复。

### 所有扫描物品栏均满时

若无符合条件且有空位的收件人，包括初始扫描计数器为负时，文本 417 报告双手已满（`{N}`、`{W1}`）。调用方恢复所搜索 X/Y，调用 `CloseChest` 再 `RefillNonChestItem`，随后关闭／返回 -1。CloseChest 清除匹配宝箱标志、恢复 `$D801` 并请求平面更新。RefillNonChestItem 清除匹配 other-item 标志。这些表无重叠宝箱／其他物品坐标，因此无关查找不做任何事。

回滚后，选定物品可供后续获准搜索。这保留源码的重试边界；不证明自然收集物品、渲染宝箱动画、完成物品栏写入、输入等待或存档／读档持久性。

## 内部楼梯保留当前地图

**已确认：**`6-warp-events.asm` 的选定传送 rows 4/5 是无滚动当前地图配对：

| 源码注释楼梯 | 触发 | 类型／地图 | 目的地／朝向 |
| --- | --- | --- | --- |
| Up | `(21,8)` | 0 / MAP_CURRENT 255 | `(57,3)`、Left 2 |
| Down | `(57,3)` | 0 / MAP_CURRENT 255 | `(21,8)`、Right 0 |

步行仅在移动／碰撞检查后调用 `WarpIfSetAtPoint`，且仅针对候选种类 `$1000`；载具使用方有各自前置检查。生产方将候选 X/Y 除以 384，按顺序扫描行，发布 MAP_EVENT_TYPE=1 和类型／地图／目的地／朝向参数，并设置 WARP_SFX。它本身不重定位队伍。两个楼梯均不匹配此前外部行；四个外部出口不在此局部继续范围内。任何模式下自然到达楼梯均未知。

`ProcessMapEvent` 在分发前清除待处理状态。普通无滚动使用方请求／清除 WARP_SFX，使 0 空闲并展开至 MainLoop，携带 d0=255、选定目的地、朝向及 d4=0。SwitchMap 无 255 行。CheckBattle 为扫描将 255 解析为当前 23；完整已接受战斗表无 Map23 行，因此恢复传入 255。局部继续保留哨兵值，除非中途待处理状态改变实际消费的请求。

### 既有行动者与布局在重载中保留

ExplorationLoop 仍清除待处理状态，将踩踏计数器减去 20000 并以零为下限，调用 HealLivingAndImmortalAllies 并开始淡出。在其低字节 255 分支中，等待淡出完成，更新 main 0 的正值目的地／当前坐标与朝向，并刷新 Bowie 传入职业／HP 精灵。它调用 DeclareRaftEntity，但不重建配置实体或地图精灵、不重置临时标志 256–383，也不经全新地图分支新设置 F80。CURRENT_MAP=23 和既有 CURRENT_BATTLE 保留。这不是新的默认／F762 实体生成。

| 传入模式 | 既有随从与独立木筏 |
| --- | --- |
| 0 / 步行 | 重定位既有 EXPLORATION_ENTITIES 列表中的每个物理条目；再应用 F64／已保存木筏选择。 |
| 1 / Caravan | 跳过上述随从重定位；到达 F64／已保存木筏选择。 |
| 2 / 木筏 | DeclareRaftEntity 立即返回。 |

其他模式值走非2／非1的重定位分支，无已确立修复。重定位更新当前和目的地坐标，不重建列表；不添加或清除成员资格／随从标志。仅当前随从被保留，普通 NPC 不被此列表操作重定位。非2模式下，F64 未设置则返回。F64 已设置且已保存 RAFT_MAP=23 时，在掩码后已保存 X/Y 声明独立木筏；地图不匹配则清除 lookup 63，并将原始当前／目的地位置停放在 `$70007000`。已保存木筏坐标／地图和 PLAYER_TYPE 不被改写。

共享尾部初始化精灵帧计数器、选择视野目标 0、以 255 调用 LoadMap、安装基础 VInts 并运行当前所选 init。LoadMap 的负字节分支跳过图块／布局解码，在区域／屋顶评估前保留工作布局。两个配置的 init 均为同一仅含一个 `rts` 的 `ms_map23_InitFunction`：无场景、请求、行动者、标志或运输修复。因此仍发生地图加载／init，却无全新实体生成或临时标志清除。四个收集标志、F13 和 F762 不被这些选定调用清除。

### 有条件地返回控制

条件音乐／淡入淡出工作后，WaitForEvent 仅在待处理为零时安装控制。它跟随 physical 0 并选择 `SetControlledEntityActScript`：mode 0 使用 eas_ControlledCharacter；mode 1 使用 eas_Raft，实际为 Caravan；其他值使用 eas_Caravan，对 mode 2 实际为木筏。尽管源码标签如此，数字映射决定解释。楼梯不使其下船或规范化模式；刷新 Bowie 精灵不证明步行。中途请求优先于控制安装。

普通返回的对话／搜索继续既有探索循环，受待处理状态约束，不强制全新入场。**未知：**完成服务、动作、淡入淡出、地图渲染、可用输入及持久性。止于此处；不附带必需村庄行程或后续旅行。

独立的 [Elven Village 离开至 Harpy Pond 入场](elven-village-departure-through-harpy-pond-entry.md)解释选定外部出口、独立 Devil's Tail 滚动及两段初始求救场景。它保留本指南局部停止边界，不提供必经路线或前提村庄活动。

## 证据追踪与审阅边界

**已确认溯源：**已接受研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。其[复现所有者](../../../research/elven-village-conversations-and-local-exploration.md#reproduction-and-retained-limits)保留源码路径、地址、字面控制、比较及紧接的使用方连接。此处 Map23 配置文件名相对于 `data/maps/entries/map23/mapsetups/`，地图表相对于 `data/maps/entries/map23/`，均位于固定的 `disasm/` 下。不重新调查源码。

| 解释 | 合同／精确可执行证据 | 有意保留的限制 |
| --- | --- | --- |
| 实体优先输入、逻辑映射、朝向及动作位 | [探索流程](../contracts/exploration-control-flow.md)、[地图探索](../contracts/map-exploration.md)；[交互](../../../../tests/fixtures/h2/map-event-interaction-state-static-v1.json)中的 `sf2-map-event-interaction-state-static-v1`、sourceContext 和 interactionState.dispatchContexts/producerWrites | 固定状态／分发器边界不是实际 Map23 调用方状态。选定行位及实体生成连接来自已接受局部研究。 |
| 局部文本／F13／教堂／默认处理器 | `sf2-map-events-static-v1` [索引](../../../../tests/fixtures/h2/map-events-static-v1.json)；[实体分片](../../../../tests/fixtures/h2/map-events/entity-programs.json)、expected.entityTargetPrograms programOrder 278–287；[区域分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)、expected.zoneTargetPrograms[47]；[物品分片](../../../../tests/fixtures/h2/map-events/item-programs.json)、expected.itemTargetPrograms[22]；[对话合同](../contracts/dialogue-system.md) | 有序操作／目标是公开事实；字面转述／控制使用已接受研究注释。它们不证明显示内容、剧情事件或教堂事务。 |
| 种类优先级、d6 调用方、四槽位交付／回滚 | [探索流程](../contracts/exploration-control-flow.md)；[搜索](../../../../tests/fixtures/h2/field-search-control-static-v1.json)中的 `sf2-field-search-control-static-v1`、fieldSearchSpine.callers/blockDispatch/itemRecipientPath/fullInventoryRollback/returnContract | 共享源码／H1／ROM 主干不序列化选定村庄行。PR862 的后续复用只比较其源码投影；不是新的 H1／ROM 运行。 |
| 描述行选择 | [区域描述](../contracts/map-area-description-routing.md)；[描述](../../../../tests/fixtures/h2/map-descriptions-static-v1.json)中的 `sf2-map-descriptions-static-v1`、expected.consumerFacts | 公开摘要／条件元数据不序列化这三个书名／坐标。选定六字节行及 d7 结果使用已接受研究，不使用聚合计数。 |
| 已加入与出战收件人列表、物品栏标志 | [成员资格](../contracts/party-membership-state.md)；[属性值](../../../../tests/fixtures/h2/common-stats-static-v1.json)中的 `sf2-common-stats-static-v1`、expected.statsFacts.party/flags/inventoryBoundary | 四个出战队伍摘要事实不编码此完整收件人扫描。数字 F13／物品标志、槽位、AddItem 及回滚细节使用已接受场地搜索／局部源码连接。 |
| 无滚动请求及当前地图保留 | [入场路由](../contracts/map-entry-routing-state.md)、[地图探索](../contracts/map-exploration.md)；[请求](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-event-request-consumption-static-v1`、eventRequestConsumption.consumerContexts/accessSites | 共享发布／轮询／清除结构不是运行时楼梯。精确 row 4/5 方向、无 Map23 战斗及既有实体群／布局使用方采用已接受研究与抵达证据。 |
| 共享仅返回 init 与生命周期 | [配置](../contracts/map-setup-data.md)、[探索流程](../contracts/exploration-control-flow.md)；[init](../../../../tests/fixtures/h2/map-init-static-v1.json)中的 `sf2-map-init-static-v1`、expected.primarySourceBodies sourceOrder 38、routeJoins[59]/[60]、pointerTableJoins[62]/[63]；[引擎事实](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)中的 `sf2-map-script-engine-static-v1`、expected.mapLifecycleCommandFacts | 保留的完整 init／script 比较守护各自定义事实；不组合局部楼梯、渲染或可用输入。 |

schema／使用方配对为 [交互 schema](../../../../schemas/h2/map-event-interaction-state-static-fixture.schema.json)/[map_event_interaction_state.py](../../../../src/sf2tool/h2/map_event_interaction_state.py)、
[事件索引 schema](../../../../schemas/h2/map-events-fixture-index.schema.json)及 [entity](../../../../schemas/h2/map-events-entity-programs.schema.json)、
[zone](../../../../schemas/h2/map-events-zone-programs.schema.json)、[item](../../../../schemas/h2/map-events-item-programs.schema.json) 分区 schema／[map_events_fixture.py](../../../../src/sf2tool/h2/map_events_fixture.py) 和 [map_events.py](../../../../src/sf2tool/h2/map_events.py)、
[搜索 schema](../../../../schemas/h2/field-search-control-static-fixture.schema.json)/[field_search_control.py](../../../../src/sf2tool/h2/field_search_control.py)、
[描述 schema](../../../../schemas/h2-map-descriptions-static-fixture.schema.json)/[map_descriptions.py](../../../../src/sf2tool/h2/map_descriptions.py)、
[属性值 schema](../../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../../src/sf2tool/h2/stats.py)、
[请求 schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)、
[init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../src/sf2tool/h2/map_init.py)，以及
[引擎 schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)。现有命令所有者为 `uv run sf2 h2` 搭配 `map-event-interaction-state`、`map-events`、`field-search-control`、`map-descriptions`、`common-stats`、`map-event-request-consumption`、`map-init` 和 `map-script-engine`。受控复现前须在同一进程加载被忽略的 worktree 配置。这些路径标识已有证据，不选择为纯文字开展新运行。

综合前审阅检查了已接受研究、更严格共享合同、这些选定测试夹具操作／字段及 schema／使用方结构。公开摘要、保留的完整比较与 PR862 仅源码的场地搜索复用保持区别。较早描述及成员资格合同的较窄未知项不被其聚合测试夹具静默扩大；后续选定源码连接在此提供有边界内容。无需 Layer A 修正。

审阅反例包括 F13 未设置／已设置独立于 F762、配置标志改变而行动者保留、C 被实体截获、柜台与搜索定位之别、`(52,2)` 的非物品种类、重复／物品栏全满、缺失 Bowie 成员资格、跨 AddItem 的物品栏改变、失效当前随从、modes 0/1/2、已保存木筏不匹配和被替换待处理状态。它们约束静态结论，不是已执行场景。

保留 [PR862 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860#issuecomment-6162414060)、[变基后交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860#issuecomment-6162442630)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/862#issuecomment-6162494122)，包括未改变的保留比较、原始／最终 CI、已修正截断／查找／表检查及所链接前序失败。保留 [PR861 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/861#issuecomment-6162400984)及其[交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/859#issuecomment-6162382778)。此处不重新生成或替代任何测试夹具、schema、黄金夹具、私有载荷或已完成失败。

**未知：**自然访问／地形、实际行动者查找／朝向／柜台／布局、调用方标志、成员资格／物品栏／收件人、服务返回、教堂结果、文本替换／节奏、物品收集／持久性、当前模式／随从／木筏、已完成楼梯／加载／init 及响应输入。这些缺口按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 和 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md) 继续延后，不是自动采集队列。此可选段落既不完成设计 epic，也不完成一条战役路线。
