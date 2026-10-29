# Pacalon 城堡：入口与首次旱灾状态访问

> 本文是[规范英文原文：Pacalon castle: entry and the first drought-state visit](../../../../synthesis/chapters/northern-travel-mitula/pacalon-castle-entry-and-first-visit.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

城堡首次访问对话描述饥饿、干旱与关闭 Moun 大门的代价。国王请访客去 Tristan 救 Mitula；一名 centaur 自愿战斗，随后说自己太饿。祭司、书与雕像、三种可能拾取及四个内部楼梯请求同属此访问。**已确认：** 这些是独立互动。国王请求不创建本地任务标志或选择分支，出口记录也不以任何这些互动为前提。

本指南从三个北向请求接续 [Pacalon 对话、补给与城堡交接](pacalon-interactions-and-castle-handoff.md)，进入 Map36。主要前提是 **F530 未设**。解释有条件探索构建、默认阵容、完整可选 F810 场景、调查与服务，再到内部楼梯与南向请求。南向止于 MainLoop，**早于**城镇战斗选择。**未知：** 自然抵达、所供状态、表现完成、可用控制与持久性。原版观察或重制版前沿均不推进。

## 证据与范围

**已确认的证据所有者：** [Pacalon 城堡入口与首次访问](../../../../../research/chapters/northern-travel-mitula/pacalon-castle-entry-and-first-visit.md)，经 [#1025 / PR1028](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1028)在 `1d62ba1cd233577ddda8275d70ff1b8f9a41572a` 接受，提供完整选定表、文本字面量、命令载荷与实际消费者连接。已接受[城镇指南](pacalon-interactions-and-castle-handoff.md)与其[研究所有者](../../../../../research/chapters/northern-travel-mitula/pacalon-interactions-and-castle-handoff.md)保留传入请求及共享互动/拾取/服务限制。以下源码路径相对 SF2DISASM `master` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6` 的 `disasm/`；研究保留 USA retail ROM 身份与源码/地址溯源。标志用 Fnnn，索引从零起，地址区间左闭右开。

Standard 严谨级别仍适合 [#1030](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1030)：成员资格、跟随、物理身份、当前配置、脚本返回与重载生命周期各有谓词。其五个 Markdown 路径是唯一写入。F530 已设的人口/指针边界是上下文；后续 Frayja 回调、恢复降雨对话与 `cs_5DA28` 延后。离开研究、UI692、运行时、采集、新比较机制、通用服务/战斗审计与中文同步在原切片范围外。此设计环境无源码/配对；复用已接受溯源，不复制输入或声称新私有比较。

## 从城镇请求到有条件城堡控制

**已确认：** 城镇有序北向行提供以下名义种子：

| 城镇源 X,Y | 显式地图 / 目的地 X,Y / 朝向 |
| --- | --- |
|19,0|36 /20,30 /Up1|
|23,0|36 /22,30 /Up1|
|负 X 通配,0|36 /保留采样 X,30 /Up1|

仅当精确行失败才考虑第三行。普通行走要求适用布局 kind/控制消费者；行本身不证明可行走路径。事件槽可变，不是保存旅程：生产者先写类型再写参数，无匹配保留待处理状态，ProcessMapEvent 在分发前清类型。无滚动处理器提交/清 WARP_SFX，使 physical0 Idle，退出到 MainLoop。

处理器清 d0–d4 字，读目的地地图，采样 physical0 **当前有符号坐标字除384**。非负目的地字节仅替换低字节；负字节保留完整采样字。因此20/22/30 不丢所供高字节，通配 X 保留消费时 X，未必触发时 X。facing1、d4=0。参数替换或中间运动改变此输入。

MainLoop 普通 `@Start` 先 SwitchMap 再 CheckBattle。无36 标志切换替换，45 条七字节战斗坐标记录也无36。因此完整显式36 请求返回 d7=-1，无匹配解锁测试或战斗裁剪。不同于绕过 CheckBattle 的战斗返回 `alt_MainLoopEntry`。变化请求/地图仍是实际输入，不修复为城堡探索。

ExplorationLoop 清待处理类型，作有符号字 STEP_COUNTER 减20000/条件清零，治疗符合己方角色，请求淡出，暂选 view-1。显式36 写 CURRENT_MAP36 与 CURRENT_BATTLE255，加载瓦片集、等淡入淡出、清 VInt，选择并重建配置行动者/地图精灵，清临时256–383 并设80。初始化精灵计数器，选 view0，以保存地图字与 view-coordinate-1 调 LoadMap，安装基础 VInt，调用当前所选初始化。调色板比较决定音乐/淡入；随后移动 SFX 与 WaitForEvent。

治疗扫描全部30 己方：Peter7/Lemon28 始终符合，其他仅当前 HP 非零。符合成员获得最大 HP/MP，只保留已持有 STUN/POISON/CURSE，随后重算属性。死亡 Frayja22 仍死亡；joined22 不是治疗谓词。重建清旧行动者、映射与行走临时区，但保留 F22/F82/F530/F810 与物品标志174–176。physical0 使用整字返回坐标乘384、facing1 与 Bowie 实际职业/HP 精灵规则，而非首个战斗出战成员。

初始化后 WaitForEvent 在 A/C 前检查待处理类型。零重置 view0 并安装主角控制：numeric PLAYER_TYPE0 选角色、1 筏、其他非零 Caravan。待处理非零绕安装，先分发。行动者/VInt 可能已推进。这是有条件事件/控制边界，不是已观察可用城堡控制。

## 已加载城堡与变化配置

**已确认：** Map36 头选择 Map36s0_Blocks、Map36s1_Layout 及其区域、标志、踩踏、屋顶、传送、宝箱与其他物品表。Palette10 与 tilesets90/52/50/92/58 是数据选择。两区域为：

| 行 | 主矩形 | 次前景 / 背景原点 | 层类型 / 音乐 |
| --- | --- | --- | --- |
|0|0,0 至47,31|0,32 /0,0|0 /MUSIC_CASTLE|
|1|50,0 至61,15|0,0 /0,0|255 /MUSIC_CASTLE|

两层均视差256,256、零自动滚动。以所供 view-coordinate-1 调 LoadMap 时，根据被观察行动者实际内部位置选择；普通20/22,Y30 种子选行0。保留 X 可选不同区域；无矩形匹配回退首区。空标志事件表、五条踩踏图块复制记录与九条屋顶记录是独立资源。区域元数据不解码碰撞、门准入或可通行路径。

`data/maps/mapsetups.asm` 选 `ms_map36` 或 F530 的 `ms_map36_flag530`。六指针表位于 `$5D756..$5D76E` / `$5D76E..$5D786`，仅实体列表与实体事件不同。zone、描述、物品事件与初始化指针共享。GetCurrentMapSetup 每次重读地图/标志：构建、互动与初始化不消费一份冻结530 快照。

set530 替代人口有11 行：己方22 在27,8 Left；随后 NPC128..137 在25,24 Left；12,25 Down 行走；21,16 Down 行走；8,3 Down；5,22 Left；40,17 Down；king29,3 Down；31,5 Left；31,11 Left；priest40,4 Down。其他 NPC 是男性 centaur。事件指针改为 `$5D924`。构建后改530 会针对保留行动者选择新回调，包括祭司索引变化；不重建或重新标记。后续访问回调与进展仍在本指南外。

## 在场者与 Frayja 标志的实际作用

**已确认：** 默认列表 `s1_entities.asm`、`$5D786..$5D7F0` 有13 条八字节行与哨兵值：**己方22 加十二 NPC**。不从 NPC128 开始。完整列表与默认对话主题如下：

| 行 / 逻辑 | 声明 X,Y / 朝向 / 精灵 | 行动 | Map36_ 回调 / 完整主题 |
| --- | --- | --- | --- |
|0 /22|28,6 /Down /Frayja 己方精灵|eas_Init|Event0，3301 →3302：承认抛弃 Moun 居民以保护 Parmecia。|
|1 /128|18,24 /Down /男性 centaur233|walk range1|Event1，3281：欢迎 South Parmecia 来客。|
|2 /129|33,26 /Left /男性 centaur233|eas_Init|Event2，3282 →3283：将干旱与恶魔相连，怀疑 Mitula 受害。|
|3 /130|7,5 /Right /男性 centaur233|eas_Init|Event3，3284 →3285：否认有士兵能对抗恶魔。|
|4 /131|4,22 /Right /男性 centaur233|eas_Init|Event4，3286/3287：逞强、可选场景，之后饥饿。|
|5 /132|21,16 /Down /男性 centaur233|walk range1|Event5，3288 →3289 →3290：Moun 以西 Nazca、锁门与 Vicar Frayja 的钥匙。|
|6 /133|31,5 /Left /男性 centaur233|eas_Init|Event6，3291 →3292 →3293：经 Moun 入侵、封门与被抛弃居民。|
|7 /134|31,10 /Down /男性 centaur233|walk range1|Event7，3294 →3295：干旱缺粮与盼望一次降雨。|
|8 /135|29,3 /Down /king231|eas_Init|Event8，3296 →3297 →3298：感谢 Higins 的来客，请去 Tristan 救 Mitula，担心入侵前先遭旱灾。|
|9 /136|38,5 /Down /princess232|eas_Init|Event9，3299 →3300：描述北行去 Tristan 的山路。|
|10 /137|16,10 /Down /女性 centaur234|walk range1|Event10，3303 →3304：问 Mitula's Shrine 士兵是否被杀。|
|11 /138|22,9 /Down /女性 centaur234|eas_Init|Event11，3305 →3306 →3307：称见公主在厨房进食，认为不雅。|
|12 /139|40,4 /Down /priest192|eas_Init|Event12：j_ChurchMenu，随后返回。|

Map36 不是大地图，也不排除随从，F65 因此不加 Caravan 前缀。普通启用标志随从先按已接受[随从顺序](battle26-return-through-pacalon-entry.md#城镇人口是有条件内容)分配。准入不测试军团成员资格。每启用行独立分配；重复别名可分配行动者并覆盖查找，有限存储不防护大量启用。

F82 随从行指定22。InitializeMapEntities 见固定行精灵低于30，视为己方；若 lookup22 已非零，消费行动长字但跳声明与物理递增。仅82 时 Frayja 保持 follower1 的位置/脚本，不再次放至28,6。无此查找则无论 joined22 都声明。设实际随从声明数 n，己方行分配时 a=1、去重时0，则 NPC j=0..11 以逻辑128+j / lookup32+j 映射到物理 n+1+a+j。己方不推进 NPC 查找游标。无随从时 Frayja1/NPC2–13；仅82 也得 NPC2–13，但 Frayja 起点不同。公式假定成功有界分配与有效映射。

`$5DA16..$5DA28` 初始化独立测试 **joined22**。未设返回；已设解析 logical22 并调 MoveEntityOutOfMap。SetEntityPosition 将实际**及目的地** X/Y 写 `$7000,$7000`。既不删行动者、不清映射/随从资格、不停脚本、不改 HP/职业、不加入22，也不启用82。VInt_UpdateEntities 跳过 X 至少$7000，因此保留脚本不会立即带回；后续重定位可重新符合。保留 CURRENT 重载也再次运行同初始化。lookup22 缺失不是安全缺席：零无守卫解析 physical0，损坏/缺失映射可移走主角或另一行动者。

Frayja/Bowie 精灵依实际己方职业/HP；战外 HP 零用蓝焰。固定 NPC 精灵不作己方 HP/职业选择。四行走者各复制50 字节到行走临时区，修补原点/range1；随机步请求不证明路径。eas_Init 在 eas_Idle 前建立 speed32/32、运动/碰撞标志、Up orientation、size24 与精灵更新。orientation 与声明 facing 是不同字段。

## 交谈、口述主张与祭司

**已确认：** `s2_entityevents.asm`、`$5D84A..$5D924` 含13 条具名四字节行与 $FD 默认 RTS。每具名行用 UP1：朝所供玩家朝向转向，不恢复。相对回调偏移从表基址解析，不从稳定行动者身份解析。

普通 C 在移动/视野滚动等待与调试/Caravan 替代分支之后。GetActivatedEntity 用位置/朝向，可穿过柜台/桌台布局，扫描前方瓦片附近物理行动者，跳被观察者与随从。反向查找选首别名；失败返回0。包装器将玩家朝向存 EVENT_RELATIVE_POSITION，选当前配置与首逻辑匹配/默认，再从**实际映射精灵**取立绘/台词。回调前清 VInt_UpdateEntities；普通返回关立绘/文本并重新激活。默认 RTS 仍有包装器工作。摄像机/模式/映射变化可独立于名义人口表改变激活。

king/男性 centaur 精灵属性选无立绘与 DIALOG_BLEEP_6；princess/女性 centaur 选无立绘与 DIALOG_BLEEP_2。祭司选 PRIEST/DIALOG_BLEEP_5。FRAYJA_PROMO 选 FRAYJA/DIALOG_BLEEP_3；其他职业/HP 精灵用自身行或回退。立绘或文本标签不证明预期物理行动者被激活。

研究所有者将全部27 个默认文本3281–3307 连接至 gamescript 键0CD1..0CEB。`txt` 调 DisplayText；每次调用将替换指针重置 DIALOGUE_NAME_INDEX_1。3290 显式 `{NAME;22}` 经 GetCombatantName 指名 Frayja，3296 `{NAME;19}` 指名 Higins，独立于说话者、成员资格或首出战成员。多文本序列在条目间保留 W2、结尾 W1。连续 txt 不蕴含关闭。[对话合同](../../../contracts/interface/dialogue-system.md#轮询顺序与不同消费者)区分 W1/W2 输入/RNG/VInt/游标/音效/清理规则；均非 YesNoPrompt，此处文本等待也不是测得表现。

国王请求仅是对话：3296–3298 不写任务标志、不测 F19、不作 YesNo 分支、不授予/加入任何内容、不提交地图或战斗请求。其他纯文本体也返回。仅 Event4 有本地 F810 写入，祭司委托共享条件效果。**推断：** 懊悔、干旱与恐惧中的逞强连接这些主题。Moun 门/钥匙、路径通行、伤亡、神受害与公主行为仍是口述，不是独立证明世界状态或必做目标。士兵问题不确认任何死亡。

Event12 调 ChurchMenu，无本地首次访问标志。Text110 用两个 W2 控制欢迎访客，之后复活/治疗/转职/存档选择。[服务合同](../../../contracts/party/service-interactions.md#church-源码边界)拥有付款、成员/状态/数据门槛与有界 Raise/Cure/Save 观察；单接触既不治疗也不存档。选定 Save 首提示结果0 将当时 CURRENT_MAP（名义36）复制 EGRESS_MAP，设399，以 CURRENT_SAVE_SLOT 调 SaveGame。首答非零避开存档写入，随后112 另一服务提示以零循环 @StartMenu、非零退出。实际存档后独立继续冒险提示以零退出菜单、非零到117/FadeOutToBlack/WitchSuspend。挂起路不是普通回调返回。Map36 存档点40,5/Up 是返回数据，不是立即传送。调用、已到达变更、已完成服务与持久存储是不同边界。

## Centaur 可选移动与动作场景

**已确认：** Event4 持有810 时仅请求3287 “Oh, I can't. I'm too hungry!”（不行，我太饿了）后返回。未设810 时，请求3286 打败恶魔的自荐，关文本，调 `cs_5DA7A`，**然后设810**，请求3287 并返回。不测试饥饿、成员资格或路径前提。F810 在临时256–383 外；普通重载不清。

脚本 trap 在 ExecuteMapScript 前激活 VInt_UpdateEntities，使行动可在包装器先前清除后推进。解释器在 FF9004_LOADING_SPACE 初始化行动临时区，并重置 SKIP_CUTSCENE_TEXT。`$5DA7A..$5DAE4` 完整程序为：

| 索引 | 命令与全部内联/压缩工作 |
| --- | --- |
|0|setCameraEntity131|
|1|setDest ALLY_BOWIE,4,23|
|2|setFacing ALLY_BOWIE,UP|
|3|entityActionsWait131: moveRight2; endActions|
|4|entityActionsWait131: moveDown1; endActions|
|5|setFacing ALLY_BOWIE,RIGHT|
|6|csWait60|
|7|customActscriptWait131: ac_setSpeed16,16; ac_jump eas_Idle; ac_end|
|8|entityActionsWait131: moveRight2; endActions|
|9|headshake131|
|10|shiver131|
|11|csWait120|
|12|shiver131|
|13|customActscriptWait131: ac_setSpeed8,8; ac_jump eas_Idle; ac_end|
|14|entityActionsWait131: moveRight1; endActions|
|15|shiver131|
|16|csc_end|

摄像机经 lookup35 解析131，改 VIEW_TARGET_ENTITY，不等抵达。最终文本前此处无目标重置。Bowie setDest 将4,23 乘384，写目的地、绝对行程与每轴有符号速度32，再经 WaitForEntityToStopMoving/VInt 等实际 X/Y 等于目的地。不传送或寻路。两次朝向命令独立检查己方 HP：零则跳操作数/朝向写入；存活映射 Bowie 收 Up1 再 Right0 与精灵更新。setDest 无同类 HP 门槛。

每个 entityActionsWait 将131 行动指针换成编译临时区，以物理 ID 初始化行动计时器，用$9F 清 flagsA bits5/6。压缩方向/距离发相对目的地 opcode4、有符号字增量与 opcode1（`esc01_waitUntilDestination`，不是一帧等待）。$8080 载荷末尾追加 opcode$34/jump eas_Idle；非零 Wait 字节轮询精确 Idle 指针。不恢复旧行动和标志。esc04 相对**当时实际**位置将增量乘384。重叠辅助由 flagsA bit5 门控，而编译器已清它；loc_55C8 以当前速度字节提供目的地/行程/速度，由 VInt 移动消费。不推出经过碰撞检查的行走保证。

若四次移动从名义4,22 未中断完成，依次目的地为6,22；6,23；8,23；9,23。这是条件算术，不是对变化映射、位置或脚本的强制终点。两内联载荷安装行动地址，以物理 ID 初始化计时器，等 eas_Idle 并按字扫描至$8080。各含10 行动字节加两字节终止符：opcode$10 写相邻压缩速度字节；opcode$34 消费 eas_Idle 绝对指针。完成后第二次向右移动保持 speed16/16，末次8/8，不恢复初始32/32。

Headshake 设动画 counter-1，执行七次 DBF 迭代，每次两个地图精灵变换/DMA/更新序列与2+1 次 VInt 等待，结束 counter0，不恢复保存值。sub_45D1C/sub_45D46 用 sub_45DA4 半字节复制作用于两动画帧。每次 shiver 保存 size/counter，设 size21/counter-1，将 flagsB bit3 开/关三次，每次精灵更新后 Sleep5，再恢复 size/counter。即使 bit3 初始已设，结束也清。这是 RESIZE：普通 ChangeEntityMapsprite 解压朝向图形，有条件以 size21 缩放并排队 VRAM DMA；特殊精灵/entnum32 可跳此路径。Shiver 改图形状态，不改 X/Y。这些请求不证明城堡中已呈现摇头或颤抖。

csWait60/120 编码$803C/$8078：解释器将低字节传 Sleep，调用对应次数 WaitForVInt。调试/P2-Start 可设 SKIP_CUTSCENE_TEXT，跳这些负字 sleep。Headshake 与 shiver 显式等待仍独立。请求计数不是测得秒数或完成动画。csc_end 时非零 DIALOGUE_WINDOW_INDEX 条件触发视野滚动等待；解释器清 VIEW_SCROLLING_SPEED 并返回，不恢复摄像机目标、旧脚本、速度、位置或 flagsA。

脚本返回前中断可留下场景效果而810 未设；最终3287 中断可留下810 已设而对话未完成。回调返回仍需包装器清理；下一 WaitForEvent 仅在待处理零时重置 view0 并装控制。**脚本返回、回调返回与可用控制是三个边界。** 此证据中均非自然城堡运行时观察。

## 书、雕像、锁门与三种可能拾取

**已确认：** 两配置共享 `$5D9C6..$5DA0E` 描述包装器：16 字节、从$5D9D6 起九条六字节记录，之后$FD00。每行均 base$1038=4152、condition0、text payload-kind0：

| 行 / X,Y | 调查偏移 →ID | 描述偏移 →ID / 内容 |
| --- | --- | --- |
|0 /35,3|4 →427，书架|0 →4152，书：PACALON KINGS|
|1 /36,3|4 →427，书架|1 →4153，书：PALADIN HONOR|
|2 /56,8|10 →433，门|2 →4154，“It's locked.”（锁着）|
|3 /19,15|9 →432，雕像|3 →4155，优秀士兵|
|4 /23,15|9 →432，雕像|4 →4156，优秀士兵|
|5 /26,8|9 →432，雕像|5 →4157，优秀士兵|
|6 /32,8|9 →432，雕像|6 →4158，优秀士兵|
|7 /26,11|9 →432，雕像|7 →4159，优秀士兵|
|8 /32,11|9 →432，雕像|8 →4160，优秀士兵|

CheckArea 清姓名槽/台词为0，调查 NAME 因此指 Bowie 姓名。先请求带 W2/CLEAR 的423+调查，再带 W1 的4152+描述，关立绘/文本，首获准匹配返回 d7=-1。condition0 行允许普通 d6=1。无匹配返回0，d6=1 可回退 FieldMenu；所供 d6=0 改用通用区域/空文本。[描述合同](../../../contracts/world/map-area-description-routing.md#六字节记录与首匹配扫描)拥有压缩扫描、条件及文本/函数区别。

实体激活先发生。CheckArea 内实际布局 kind $1800 宝箱、$2C00 花瓶、$3000 木桶、$3400 书架、$1C00 区域物品先于描述。因此标为书的坐标不证明包装器执行。锁门文本不查钥匙、不写解锁标志、不开放通道。本地描述均无函数、选择或剧情变更。表坐标不是解码通行证据。

共享四字节收集记录如下：

| 表 / 行 | 候选 X,Y | 标志 | 物品符号 / 索引 |
| --- | --- | --- | --- |
|Chest0|56,4|176|MITHRIL123 ($7B)|
|Other0|22,4|174|FAIRY_POWDER5|
|Other1|30,19|175|PEGASUS_WING118 ($76)|

宝箱表为 `$AE07A..$AE080`；其他物品 `$AE080..$AE08A`，各以 endWord 结束。174–176 独立于530/810，在临时范围外。GetChestItem/GetNonChestItem 选当前地图指针；GetItem 按行序比候选坐标低字节。变化 CURRENT_BATTLE≠255 保留原错误 stride5 裁剪分支，不修成七字节。布局/调用方仍决定是否使用记录。

实际 chest-kind 调查调 OpenChest：持有176 则替换 NOTHING127；在拾取**之前**设176，写开启图块55298/$D802 并请求平面更新（type0 bit1、其他层类型 bit0）。403 宣告开启；空结果请求408。CheckChestItem 只测标志/布局，不作这些开启写入。重复不建立重复 Mithril；实际保留/加载图块 kind 仍决定调查路由。符合非宝箱分支在返回前设174/175，持有时替换 NOTHING。无匹配 -1 掩为127，选空物体文本。

物品5/118/123 低于金币阈值，走普通物品栏分支。通用尾部请求413（发现物品，W2/CLEAR），查 Bowie 四槽，有空调 AddItem；之后请求物品音乐、415 与独立 FadeOut_WaitForP1Input。Bowie 满时 UpdateForce 构建加入成员，有序 OTHER_FORCE_MEMBERS_LIST 找空位；之后 AddItem、音乐、带收取者姓名替换的416 与输入/淡入淡出。回退不限出战队伍、无 HP 门槛。列表在 TARGETS_LIST 后一字节开始，假定已加入 Bowie 为首：变化为 Bowie 未加入时仍试0，但省略实际首个加入回退成员。415/416 无 W1/W2；普通输入独立。容量读取后变化可令 AddItem 失败，调用方不测返回。听似成功的文本不证明交付。

全满则在保存候选坐标的 **CloseChest 与 RefillNonChestItem 两者之前**请求417。各重找自身表：Mithril 恢复清176，写关闭图块55297/$D801 并更新平面；非宝箱恢复清174/175。只有抵达恢复才可重试。中断可留有标志无物品，或物品已插入而表现未完成。收集非原子事务，不建立 Pegasus Wing 使用、转职或 Mithril 锻造。已接受[拾取所有者](../../../../../research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md)与修正城镇指南保留共享文本/顺序/军团语义。

共享 zone/物品事件表仅默认 RTS。包装器仍清理/等待；物品使用提供 d6=0，与收集不同。两默认均不授物、不加入 Frayja，也不提交离开请求。

## 内部楼梯保留状态；南向请求城镇

**已确认：** `6-warp-events.asm`、`$AE050..$AE07A` 含五条有序八字节无滚动/type0 记录与 endWord。朝向是字节加零填充：

| 行 | 源 X,Y | 地图操作数 | 目的地 X,Y | 朝向 |
| --- | --- | --- | --- | --- |
|0|255,31|PACALON2|255,1|Down3|
|1|2,4|CURRENT255|52,3|Right0|
|2|52,3|CURRENT255|2,4|Left2|
|3|55,11|CURRENT255|5,12|Right0|
|4|5,12|CURRENT255|55,11|Right0|

WarpIfSetAtPoint 在首匹配扫描前查$FFFF 终止符；负源字节为通配。角色行走要求布局 kind$1000；筏/Caravan 另有调用方。无行测试530/810/joined22/物品/服务。各写 event1、PARAM1..4 长字、PARAM5/padding 字与 WARP_SFX。无匹配保留待处理。处理器消费可变参数，使主角 Idle、采样当前字，分别替换非负低字节或保留负值对应整字，以 d4=0 返回 MainLoop。

CURRENT255 既不是滚动，也不由 SwitchMap 解析。CheckBattle 临时解析 CURRENT_MAP；实际36 无战斗行，因此探索前恢复 operand255。变化 CURRENT_MAP 可改变结果。完整楼梯请求中，ExplorationLoop 仍清待处理、调整 STEP_COUNTER、治疗、淡出/等待，更新主角朝向/精灵与非负低字节坐标，调 DeclareRaftEntity，重置精灵计数器，LoadMap、基础 VInt 与当前初始化。仅低字节非负时才缩放整个坐标字；变化为负低字节则跳该坐标更新。

不同于显式入口，CURRENT **不**重建行动者/映射/行走临时区、不载新固定列表、不替换 CURRENT_BATTLE、不清临时标志。DeclareRaftEntity 有实际作用：numeric mode2 返回，mode1 跳随从重定位，其他模式将所列随从实际/目的地坐标移到主角所供瓦片。再用 F64/RAFT_MAP 声明或移出 physical raft31。随从映射/脚本保留。城堡初始化在重定位**之后**运行，joined22 持有时再将映射22 移至$7000。这是保留行动者位移，不是移出队伍或随从列表。

LoadMap 符号扩展 CURRENT255，直接分支到区域指针：无新调色板/瓦片集或 LoadMapBlocksAndLayout。**已加载图块/布局保留**，区域/视野/显示与屋顶刷新。普通行1/4 选50..61 区，行2/3 选0..47 区。显式地图加载则重建图块/布局并将已持有宝箱标志重应用为开启图块。楼梯不重置174–176，也不解锁所描述门。通常保留810 与 centaur 已到达行动/速度/位置，仍受后续行动者执行影响。新显式36 重建即使持有810 也恢复声明位置/脚本；重复饥饿对话不要求持久场景终点。当前530 可改变初始化/事件选择而不重建阵容。调色板/音乐与待处理/控制条件仍适用。

南行0 返回显式 Map02、Down3、目的地 Y 低字节1 与保留消费时 X。**终点为此 MainLoop 返回，早于城镇 SwitchMap/CheckBattle/配置。** 先前城镇入口指南保留可能 Battle31 准入；请求不建立城镇重建或可用控制。显式地图请求仅在后续准入抵达探索时才重建人口/临时标志/布局。该条件生命周期不同于四个 CURRENT 楼梯。

[Pacalon 离开：北向滚动与有条件 Battle27 准入](pacalon-departure-to-battle27-admission.md)接续南向请求，经有条件城镇准入、新大地图构建、保留北向滚动与独立 zone/重载/战斗扫描。止于 BattleLoop 初始化前，保留自然路径、所供状态与控制/持久性未知项。

## 变化状态与保留审阅

| 变化前提或中断 | 必需区别 |
| --- | --- |
|530 未设/已设或配置读取间变化|默认阵容与当前回调可不一致；替代进展仍延后。|
|22/82 独立持有|固定己方分配、随从去重与初始化移出是不同谓词；此处无招募。|
|缺失/别名映射、大量随从、变化 HP/职业|实际行动者、首反向别名、精灵与有限存储决定；零不是安全缺席。|
|810 持有或场景/末文本中断|场景效果早于810；810 早于末文本完成；重复跳脚本。|
|行动者位置/行动/速度/标志、摄像机或调试变化|相对运动、保留速度、被覆盖脚本与等待/图形消费者用当前状态。|
|布局、模式、待处理或描述 d6 不同|激活、首匹配、控制与调查优先仍有条件。|
|174–176 持有、加入列表/容量变化或拾取中断|标志、AddItem、音乐/文本与恢复是独立提交；不保证交付。|
|教堂拒绝、Save、继续/挂起或中断|服务循环不同于存档与挂起；调用不证明持久性。|
|CURRENT/显式地图、有符号哨兵值、高字节或待处理替换|行动者/布局/临时状态保留或重建依实际消费操作数。|

合成前审阅消费完整已接受 #1025 研究与先前互动指南，以及必需[入口](../../../../../research/chapters/northern-travel-mitula/battle26-return-through-pacalon-entry.md)、[互动](../../../../../research/world/map-event-interaction-state.md)、[对话](../../../../../research/world/map-event-dialogue-state.md)、[请求](../../../../../research/world/map-event-request-state.md)、[消费](../../../../../research/world/map-event-request-consumption.md)、[地图](../../../../../research/world/common-maps.md)、[脚本/行动](../../../../../research/world/common-scripting.md)、描述、拾取与服务所有者。检查选定真实公共载荷、schema 与验证器消费者，不以聚合计数替代：

| 公共测试夹具 / 实际消费者 | 已检查边界与限制 |
| --- | --- |
|`sf2-map-init-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)、[map_init.py](../../../../../../src/sf2tool/h2/map_init.py)|实际 Map36 默认/530 路由81/82、指针82/83 连接及五个初始化操作；不是运行时映射或自然初始化观察。|
|`sf2-map-events-static-v1:routing-setup` 与 `:entity-programs`；[路由](../../../../../../tests/fixtures/h2/map-events/routing-setup.json)、[程序](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)、[map_events.py](../../../../../../src/sf2tool/h2/map_events.py)|选定默认目标与全部14 默认体、有序文本、810/脚本/写入/返回及 Church 别名。表与程序体均不证明激活或显示。|
|`sf2-map-script-engine-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、[map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)|实际选定摄像机、放置、行动桥接、动作/表现与解释器处理器守卫/游标/等待消费者。公共紧凑投影不暴露 cs_5DA7A 完整程序或六载荷；精确本地连接是已接受 #1025 证据，不在此新比较。|
|`sf2-entity-action-scripts-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/entity-action-scripts-static-v1.json)、[entity_action_scripts.py](../../../../../../src/sf2tool/h2/entity_action_scripts.py)|紧凑处理器/终止符与更新/图形事实及实际解析器/验证器消费者。精确相对目的地/速度/跳转源码连接仍属已接受研究；此投影不是观察城堡运动或呈现动画。|
|`sf2-map-event-interaction-state-static-v1`、`sf2-map-event-dialogue-state-static-v1`、request-state/consumption 测试夹具；[互动](../../../../../../src/sf2tool/h2/map_event_interaction_state.py)、[对话](../../../../../../src/sf2tool/h2/map_event_dialogue_state.py)、[请求](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)|获取/反向映射/包装器与选定待处理读取/重置/分发上下文。有界上下文不进入每个传送/地图被调用方，也不证明自然生产者到消费者路径。|
|`sf2-map-descriptions-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/map-descriptions-static-v1.json)、[map_descriptions.py](../../../../../../src/sf2tool/h2/map_descriptions.py)|六字节/终止符/d6/首匹配/文本基址消费者。九本地行、文本连接与三收集记录依已接受研究，不是解码公共布局。|
|`sf2-common-menus-static-v1` 与 `sf2-church-save-lifecycle-runtime-v1`；[菜单](../../../../../../src/sf2tool/h2/menus.py)、[Save](../../../../../../src/sf2tool/h3/church_save_lifecycle.py)|实际 Save 路由操作、选择极性与五受控观察时序；不是自然 Map36 服务或持久性观察。|

所属命令为 `uv run sf2 h2 map-init`、`map-events`、`map-script-engine`、`entity-action-scripts`、`map-event-interaction-state`、`map-event-dialogue-state`、`map-event-request-state`、`map-event-request-consumption`、`map-descriptions`、`common-menus` 与 `uv run sf2 h3 church-save-lifecycle`。审阅公共表面，未重跑私有源码提取或运行时。未发现阻塞所有者分歧；未来矛盾在扩展解释前返回研究。

[#1025 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1025#issuecomment-6183213238)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1028#issuecomment-6183221581)保留完整源码/表/文本/载荷/消费者审阅：最终 head2ad7209b/tree048f728d/base936cee7d、直接0.146s/独立0.144s 与 CI38672496689。原5fa5cb21/tree015ecf08/basee9da6214、直接0.145s、CI38672213037 与失败的扩展审阅测试夹具键诊断保持独立。变基后所属文档逐字节相同，不重置完成结果。18,285,979 字节配对的已接受0.083s 溯源继续复用，不在此检出再生成或新比较。

[#1026 修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1026#issuecomment-6183164493)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1027#issuecomment-6183175026)保留 headb0b886fe/tree8ec730a0/basee9da6214、修正直接2.369s / 独立2.357s 与 CI38672157287。原882ec169、直接2.400s、CI38671842716、修正史与检查/检查器诊断保持独立，包括拾取音乐/文本顺序及 Save 菜单循环与存档后挂起区别。所链 #1021/#1022/#1017/#1018 交接保留先前失败、原/变基回执与未知项。

此原 Issue 交接记录全文/diff/变化状态审阅、精确五路径、选定公共 schema/消费者检查、导航/链接/锚点、UTF-8/私有检查、git diff --check、已配置干净提交头 planner 与实际精确提交头范围 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4/翻译套件。**未知：** 所供标志、HP/职业、映射/随从/容量、物品栏/模式/待处理、自然抵达、已完成场景/对话/服务/收集/旅行、可用控制、呈现/时序与持久性。可选压缩布局缺失仍是无解码路径限制，不是采集授权或 ROM 阻塞。这些未知项不创建运行时队列；后续 Frayja 进展仍延后。
