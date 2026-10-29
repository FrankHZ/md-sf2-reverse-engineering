# Battle27：初始编队与限制对峙

> 本文是[规范英文原文：Battle27: initial formation and the restrictions confrontation](../../../../synthesis/chapters/northern-travel-mitula/battle27-initialization-and-opening.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

Battle27 首次激活前，三名敌人质问军团。**已确认：** 两名 Dark Soldier 问访客从何而来、是否知道限制；Lizardman 不接受“不知道”的借口。问题无玩家选择命令。新入口在开场前构建实际队伍与十六敌人，但对话与放置均不证明它们已激活、可见或准备单次行动。

本指南接续 [Pacalon 离开与有条件 Battle27 准入](pacalon-departure-to-battle27-admission.md)。新入口终点为 `BattleLoop:@Start`，**早于 ActivateEnemies**。F88 延续则止于 `@IndividualTurns_Loop`，早于单次行动执行。**推断：** 源码名 TO_TRISTAN 与交流将遭遇框定为旅行限制。**未知：** 自然抵达、所供状态、已完成服务、表现、可用控制与持久性。观察或重制版前沿均不推进。

## 新入口在对峙前做什么

**已确认：** 前序有序扫描提供 Map74/Battle27 与 crop `(2,16,32,48)`。Battle27 先于 Battle32；持有完成标志527 可清解锁427，而该扫描仍准入27。准入尚未赋值 CURRENT_BATTLE。以下要求实际74/27 交接；初始化不重跑准入，也不修变化裁剪字节。

`BattleLoop` 清 PLAYER_TYPE，设 F399，检查 F88。F88 未设时：

1. 清 SECONDS_COUNTER，保存传入地图/战斗**字**，将其低字节赋 CURRENT_MAP/CURRENT_BATTLE，安装基础 VInt。
2. 调 before 包装器。检查450+当前战斗，即 F477。持有 F477 则跳；F477 未设则经48 字 before 表槽27 分发 `ms_Empty`。其唯一字是终止符。Before 不设 F477。
3. 恢复传入字并重赋当前地图/战斗字节，再清**全部**区域标志90–105，不只此处四区。
4. 治疗符合己方角色，初始化实际队伍位置，初始化敌人，清 AI 内存。
5. 清 d0.w，以战斗员0 调 LoadBattle，再调 start 包装器。返回后抵达新入口的 ActivateEnemies 前终点。

空 before 仍进 ExecuteMapScript，清文本 skip 锁存并走共享终止符尾部。源码调用以服务返回为条件。中断保留已到达写入，不回滚。持有 F477 跳两个钩子，但不跳新重置、治疗、编队或加载。Before 与 start 独立采样；中间改标志可改第二决定。后续消费者独立重读 CURRENT_BATTLE。Before 后恢复参数修复陈旧新入口选择器，不修全部后续选择器变化。

## 十二位置不指定十二成员

**已确认：** 治疗独立于成员资格扫描全部30 己方。存活者以及始终符合的 Peter7/Lemon28 获最大 HP/MP；其他死者跳过。先将状态掩为 STUN|POISON|CURSE（`$0007`），再重算派生属性、状态/装备效果与诅咒。中间掩码不承诺最终诅咒。治疗不招募、转职、替换物品栏或选择出战队伍。HP 为字；MP 与位置字段为字节。

己方初始化先将每名己方 X 设 -1，再调 UpdateForce。成员标志按己方 ID 升序重建加入/出战/预备列表；所供陈旧列表不具权威。头声明 **12 个己方放置槽、16 敌行、4 区域与1 点**。[完整十二位置表](../../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#actual-party-and-all-twelve-placement-slots)将存活出战成员放在局部 X14–17/Y44–46 附近。每行向下一个符合成员提供 X/Y，不提供己方身份。其 HEALER1、NOTHING、NONE/region15、filler0 与 STARTING 字段不覆盖己方 AI、物品栏或其他状态。

死亡出战成员消费一个已处理成员槽，但不推进放置指针，因此下一存活者用较早未用位置。十二限制已处理成员数，不保证十二行动者。超额出战成员经 LeaveBattleParty 清出战位并设 X=-1，不立即重建列表/计数。符合成员较少则行动者较少。零成员计数在首次成员读取前递减，无空列表守卫；任意不一致空状态未建立安全性。编队数不保证 Bowie，Higins19 仅在实际成员资格与 HP 允许时参与。

## 六种敌人，十六初始化行

**已确认：** BattleSpriteset27 为390 字节：头4、己方行144、敌行192、区域48、点2。CURRENT_BATTLE 选指针；子段偏移来自所选头。每战斗员行12 字节，包括类型/序号、X/Y、指令集、物品字、两组命令/区域对，以及作为字读取的 filler/spawn 对。[完整十六行表与编码结果](../../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#complete-enemy-declarations-and-initialization)仍是行权威。此处按身份紧凑分组：

| 类型 / 存储 ID | 行（战斗员128 + 行） | 此处声明作用 |
| --- | --- | --- |
|Lizardman (19)|0,8,14|行0 为 LEADER，MOVE_TO point0/region3；其他为 ATTACKER1 命令。|
|Dark Soldier (18)|3,4,5|三个独立战斗员；行4/5 是两名 Soldier 说话者。|
|Wyvern (46)|1,6|两个飞行战斗员，存储命令/区域不同。|
|Dark Sniper (62)|7,9,11,15|四个独立跟随命令声明。|
|Wizard (72)|10|一个 ATTACKER3 声明，跟随敌行4。|
|Dark Bishop (81)|2,12,13|HEALER5/HEALER4 声明，各有主/次引用。|

指令集名称、数量与引用不证明战术或行动。全部行 item NOTHING、filler0/STARTING0，使所有初始化字为 `$0000`。全部 spawn 半字节低于 HIDDEN2，因此128–143 调 InitializeEnemyStats。Battle32 的 Jaro 例外不跳 Battle27 行15。新初始化不检查起始位置占用；ResetSpawningEnemyStats 属另一个后续消费者。

六种[定义、武器、法术与属性表](../../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#definitions-weapons-spells-and-stats)解释实际初始化输入。每56 字节定义按14 长字复制，当前 HP/MP 设最大值，写位置/类型，movetype/指令集装入不同半字节。全部 ENHANCED 法术威力、无初始状态。Soldier/Lizardman 保留已装备 Large Axe，Sniper 保留 Hyper Cannon，Wizard/Bishop 保留 Flail；Wyvern 无武器。行 NOTHING 在 AddItem/装备前返回，不抹去复制武器，也不加 Black Ring。Soldier ATTACK、Wyvern FLAME、Wizard BLAZE LV3 与 Bishop AURA/BLAST LV3 是声明，不是使用法术证据。

难度调整在 SUPER 比较后有连续 `beq` 指令。所有难度均应用 floor(base ATT*5/4)，SUPER 应用两次。装备效果再为 Axe/Cannon/Flail 加25/40/19 ATT。例如 Soldier 基础/当前 ATT 普通为38/63，SUPER 为47/72；所属表列全部六种结果。无初始状态定义下 DEF/AGI/MOV 保持声明值。这建立条件初始化属性，不建立伤害、命中率或预期难度。

激活保留定义 `$F000`，与旋转初始化字掩 `$0FFF` 作 OR：Bishop 行2/12/13 从 `$3000` 开始，其他 `$2000`。定义 BIT4/BIT5 丢弃；主/次 active、neutral 与 spawn-mode 位未设。触发字节与移动命令字是打包存储，不是已执行激活。敌槽144–159 获 activation0、max/current HP0 与 X=-1，但未完全清除：未变子段指针读取行0 触发对，得 `$33`；Y/MP/物品等不普遍重置。ClearAiMemory 独立将48 个 last-target 字节填 `$FF`，48 个 memory 字节填0；不生成行动顺序。

Battle27 不在完整随机升级列表，升级选择因此在完成/等级/RNG 检查前拒绝。持有 F527 不升级这些定义。也不在中立表：F527 未设查不到组，持有 F527 则跳扫描。两路均不创建中立 Higins。不能将 Battle26 升级/中立事实转用此处。

[四区域与点0](../../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md#four-regions-and-one-point)是战斗局部声明。区域0–3 对应 F90–93；type4 消费者用四顶点字构成三角形1/2/4 与3/2/4。第二/尾字节不是几何操作数。Region15 是无触发哨兵值，不是另一多边形。Point0=`(15,23)` 为行0 MOVE_TO 引用。FOLLOW_ENEMY 操作数指战斗员128/137/136/133/132/138/142。本终点内不执行区域求值、目标选择、路径或移动。

## 重建战场，而非保留大地图阵容

**已确认：** LoadBattle 保存 d0 战斗员参数，选实际 CURRENT_MAP，请求淡出、设 view target-1、载瓦片集、等淡入淡出、清 VInt、等一 VInt 并调 PositionBattleEntities。ClearEntities 重置49 个物理32 字节记录、全部64 查找字节与行走分配状态。先前探索行动者、别名和脚本丢弃。

物理扫描覆盖30 己方与32 敌方。有符号字节 X/Y 非负即允许声明；跳 neutral 标记敌人。**此扫描无 HP 测试。** 新初始化通常提供 HP/位置关系；任意所供状态未必。位置变为 `(X & $3F)*384,(Y & $3F)*384`，Down3 与 eas_Standing。声明写行动者字段、清行程/速度/加速度、设 flags `$7040` 并初始化动画/等待计数器。安装行动不证明姿态完成。

实际己方职业选精灵，含 Rohde 成员例外；战斗选择不用探索死亡己方 BLUE_FLAME 分支。敌存储类型选 Soldier82、Lizardman83、Wyvern110、Sniper126、Wizard136、Bishop145。均顺序分配；Wyvern 行1/6 aerial layer1，其余 layer0。声明己方 N 个时，战斗员128+r 映到 physical N+r；N=12 时敌人在12–27。普通新人口最多28，无中立组。此上限不是容量守卫：DeclareNewEntity 没有守卫，任意所供位置可超限。

跳过槽获 lookup-1；己方额外偏移30/31 保留 ClearEntities 的零。敌 ID128–159 减96，使用查找偏移32–63。逻辑战斗员、物理行动者与精灵是不同身份。LoadBattle 恢复 d0=0，用 Bowie 查找作 VIEW_TARGET_ENTITY 并采样所选瓦片 X/Y。负查找在这些读取中回退 physical0，不创建 Bowie 或修成员资格。

LoadMap 接收 **d0=-1** 作基于目标的视野放置，但 **d1=74** 作完整地图加载，不同于负 d1 CURRENT 重载。Map74 选 palette0、tilesets101/102/109/110/105 与单描述符：0..63 边界、零层原点/自动滚动、parallax256/256、layer type254 与 MAIN_THEME1。描述符选择将目标坐标与边界*384 比较，不加裁剪；唯一描述符也是回退。图块/布局、标志复制与宝箱覆盖加载后，再将所供战斗矩形复制至工作原点0/0。

已接受裁剪按 **64 宽存储行**复制32*48=1536 字/3072 字节。战斗区域末局部 `(31*384,47*384)`；其他层/音乐元数据仍来自描述符。不修变化 BATTLE_AREA 字节。编队、敌人、区域与点位置已是局部：对应源布局需加 `(2,16)`。Placement0 `(14,44)` 对应 `(16,60)`，敌行0 `(15,19)` 对应 `(17,35)`，point0 对应 `(17,39)`。算术连接不解码可通行路径。

尾部等一 VInt、载地图精灵、装基础 VInt，将 CURRENT_BATTLE 所选 BattleTerrain27 载 BATTLE_TERRAIN_ARRAY，请求地图音乐/淡入。MAIN_THEME1 不替换，因此提交 music1；Battle44 计时器分支不适用。地形选择不发布地形语料，也不建立移动合法性。源码顺序不证明呈现/音频完成或可用控制。

## 完整开场：两名 Soldier，随后 Lizardman

**已确认：** start 包装器检查450+当前战斗，**分发前**设 F477，再选独立48 字表当前战斗槽。槽27 选 `bscs_battle27`。标志与后续表选择器独立重读 CURRENT_BATTLE；变化可使标志身份与程序身份分离。场景要求选择时为27。持有 F477 跳分发。

完整34 字节程序有九命令，无本地行动者创建、移动、伤害、招募、物品栏、结果、剧情标志或地图请求：

| 命令索引 | 请求 |
| --- | --- |
|0|设文本游标2658。|
|1|设摄像机速度字 `$30`（48）。|
|2|设摄像机目标为逻辑战斗员133。|
|3|显示下一单文本，portrait-control 字节 `$00`，说话者133。|
|4|设摄像机目标132。|
|5|显示下一单文本，`$00`，说话者132。|
|6|设摄像机目标128。|
|7|显示下一单文本，`$00`，说话者128。|
|8|结束（`$FFFF`）。|

已接受 gamescript 文本保留拼写 **to bad**。`{N}` 为换行、`{W1}` 为文本等待；无变量替换：

| 文本 | 新状态逻辑说话者 | 文本字面量 |
| --- | --- | --- |
|2658|133，行5 Dark Soldier `(17,38)`|`Where did you come from?{N}What are you doing here?{W1}`|
|2659|132，行4 Dark Soldier `(8,34)`|`Don't you know about the{N}restrictions?{W1}`|
|2660|128，行0 Lizardman `(15,19)`|`Even if you didn't know,{N}that's just to bad.{W1}`|

文本依次质问来客来源/目的、提醒限制，随后不接受不知情。**推断：** 这将军团描述为在前往源码所名 Tristan 途中遭遇限制执行。脚本不指定限制发布权威或地理范围。之后无是/否提示、回答分支或玩家决定。

逻辑133/132/128 使用 lookup37/36/32 与 physical N+5/N+4/N；十二声明己方时为17/16/12，精灵82/82/83。完整精灵对话属性表无 Soldier 或 Lizardman 行，新解析因此回退 portrait `$FFFF`（无）与 speech bleep6=74。解析**当前物理精灵**，不是文本提供 NPC 名；变化/别名精灵可不同。辅助不查 HP。

缺失说话者不安全跳过。摄像机选择写缺失 lookup `$FF`，为负目标，摄像机更新器避免解引用。对话 GetEntityAddressFromCharacter 则无哨兵守卫，将无符号 `$00FF` 左移5，得这些操作数的 ENTITY_DATA offset `$1FE0`。不建立有效行动者、立绘或台词读取。此无守卫查找不同于摄像机负目标处理与 LoadBattle physical0 回退。

摄像机 speed48 是存储字，不是48 帧或像素。VInt_UpdateViewData 读非负物理目标，应用阈值/边界并请求目的地。非零覆盖值与描述符视差/自动滚动馈入滚动速度字。WaitForViewScrollEnd 跨 VInt 检查。这些消费者解释请求，不证明特定滚动或视野完成。

通常每个 nextSingleText 先调用立绘处理，再解析台词，写 CURRENT_SPEECH_SFX、等滚动并调 DisplayText。然后增游标、关立绘/文本并调 Sleep10。说话者字 `$0085/$0084/$0080` 不是 `$FFFF` 无说话者哨兵；portrait control bits15/14 为0。打开立绘还要求立绘窗口空、解析立绘不为 -1。既有立绘状态可绕打开，即使台词重新解析。仅全部文本正常完成才留游标2661。Sleep10 独立于 `{W1}`；均非测得呈现时序。

实际 `{W1}` 消费者保存/清打字状态，循环 RNG 种子复制工作与 WaitForVInt。接受**当前** UP/DOWN/LEFT/RIGHT/B/C/A 输入，不一定新按下；Start 不在掩码中。因此按住输入与所供状态有关。文本不保证请求一次新确认。

ExecuteMapScript 入口清文本 skip，但调试加 P2 Start 可在命令循环锁存。被跳单文本消费操作数，不显示、不增游标、不关闭、不 Sleep10。摄像机命令仍运行；中途 skip 可留前缀游标。终止符在对话窗口打开时有条件等滚动，清摄像机速度并恢复解释器寄存器。不恢复 Bowie 视野目标：三次摄像机命令后，目标保持行0 映射，依实际状态而定。

F477 是包装器写入，**不证明开场完成**。写后中断可抑制之后新入口重放未完成场景。只有服务与包装器返回才到新的 ActivateEnemies 前终点；不证明阵容已激活、玩家单次行动或战斗结果。

## F88 延续使用所供战斗状态

**已确认：** 持有 F88 仍清 PLAYER_TYPE、设399、恢复保存秒数、清88 与 AI 内存、清 VIEW_TARGET_ENTITY，并在未显式清 d0 下调 LoadBattle。直接分支 `@IndividualTurns_Loop`；之后去 `@Start` 的分支不可达。不将传入74/27 赋给当前选择器。无区域重置、治疗、成员/位置/敌人初始化或钩子重放，即使477 未设。当前地图/战斗/裁剪、HP、物品、位置、激活与单次行动状态均为所供输入。Load 仍重建物理行动者与视野，因此既非逐字节保留，也非新状态修复。

在此**特定74/27 准入入口**，标志 trap/辅助与 ClearAiMemory 保留 d0。因此强制 F88 将传入低字节74 带到 GetEntityIndexForCombatant，读**64 字节列表外的 lookup offset74**，而非选 Bowie0。改变传入 d0 改此读取；LoadBattle 的负结果回退不先验证索引。条件寄存器结果不声称自然挂起/恢复使用此入口或这些寄存器。

止于单次行动执行前。随后指令以所供 CURRENT_BATTLE_TURN 为 BATTLE_TURN_ORDER 字节偏移。选中 `$FF` 哨兵分支到 `@Start`；否则接 ExecuteIndividualTurn。仅抵达标签不保证有效行动者或立即单次行动。LoadBattle 自身不重置回合或重采样行动顺序。

只有新开场包装器返回后才续读 [Battle27 激活与首次单次行动交接](battle27-activation-and-first-turn-dispatch.md)。该指南区分已检查区域、持久标志、敌人激活、生成与单次行动准入，止于首次 ExecuteIndividualTurn 调用、早于其函数体；同时保留 F88 初始所供顺序绕过与哨兵重入。

## 变化状态与受支持边界

| 变化前提 | 必须保留条件的后果 |
| --- | --- |
|传入地图/战斗、陈旧选择器或后续 CURRENT_BATTLE 变化|新入口恢复参数；续接用当前选择器；后续各读取者独立选择。|
|F477 未设/持有或写后中断|钩子可独立跳过；新构建仍运行；标志不是场景完成。|
|F527 未设/持有|均不创建 Battle27 升级或中立组；完成不是 intro 标志。|
|死亡/较少/超额/空成员；变化 HP/职业/状态/物品|实际标志/列表、已处理成员数、放置指针、属性与精灵读取者决定；不保证具名十二人。|
|难度、敌类型/位置/neutral 位、裁剪或 F88 变化|条件定义/加载/人口不同；物理扫描无 HP 或容量守卫。|
|缺 Bowie、死亡/缺失/别名说话者|视野回退、负摄像机目标与无守卫对话指针是独立机制。|
|已有立绘、按住获准输入或调试 skip|立绘打开、等待、游标与可见文本可不同，而摄像机命令继续。|
|重置/治疗/加载/文本/摄像机/包装器中断|只支持已到达前缀；不推出表现完成或可用控制。|
|F88 所供单次行动索引/顺序|哨兵可分支向激活；此处不建立单次行动/回合结果。|

激活、区域过场、生成、行动顺序生成/执行、AI、行动、交战、结果与后续探索/剧情不受本指南支持。正初始 HP、行名与数量不建立可见性、自然参与、可达性或战术。可选压缩布局缺失仍是无解码路径限制，不是 ROM 阻塞或采集授权。自然所供标志/状态、时序/中断、表现完成、可用控制与存档/读档持久性仍**未知**。

## 证据审阅与保留结果

完整已接受 [Battle27 研究](../../../../../research/chapters/northern-travel-mitula/battle27-initialization-and-opening.md)是**已确认的静态证据所有者**，以 [#1033 / PR1036](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1036)在 `85016e1c27af3fd91cd82d0468b37c896c69f3b4` 接受。固定 SF2DISASM `master` 为 `c834c652b6862bc5679fd7f69a38a7093206efc6`；溯源绑定根 README 的 USA retail ROM 身份。源码路径、地址、完整表、文本与私有比较仍由其拥有。Fnnn 指标志 nnn；行/查找索引从零起。

完整已接受前序提供准入，不证明新初始化。[遭遇合同](../../../contracts/battle/battle-encounter-definition.md)、[战斗控制合同](../../../contracts/battle/battle-control-lifecycle.md)与[对话合同](../../../contracts/interface/dialogue-system.md)保留结构/运行时限制。更广未知队列不覆盖更深已接受 Battle27 谓词，也不建立自然抵达。未发现阻塞证据冲突；未来冲突返回研究，不作 Layer B 修补。

合成前审阅检查完整 Layer A 文字与每条变化状态行，再选实际公共载荷、schema 形状与验证器消费者。**未**重跑私有提取：

| 测试夹具 / 实际消费者 | 已审阅连接与限制 |
| --- | --- |
|`sf2-battle-spriteset-data-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)|BattleSpriteset27 地址/下一地址范围与45 槽结构；`rowContentTracked=false` 不证明其28 战斗员行、区域或点，后者用已接受更深审阅。|
|`sf2-battle-global-data-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_global_data.py)|45 条七字段地图行与摘要形状；公共中立/随机计数不含实际战斗列表。不由计数推出27 归属。|
|`sf2-battle-loop-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_loop.py)|实际治疗/状态掩码/30 与32 槽/48 字节内存/地形事实与有序源码片段守卫；不是完整队伍、物理或单次行动模拟。|
|`sf2-battle-control-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_control.py)|F88、区域重置90–105、十二字节条目与单次行动哨兵255；紧凑顺序事实不证明本准入 d0、安全查找或执行。|
|`sf2-battle-cutscenes-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_cutscenes.py)|Before 检查不设位；start 先设位后脚本；实际当前选择器与有序守卫。不含九命令体或文本字面量。|
|`sf2-battle-routing-data-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_routing_data.py)|独立48 槽钩子表/45 地形槽、terrain32 别名27；`routeTargetsParsed=false`。聚合一个非空 start 计数不单独标识此程序。|

所属命令为 `uv run sf2 h2 battle-spriteset-data`、`battle-global-data`、`battle-loop`、`battle-control`、`battle-cutscenes` 与 `battle-routing-data`。此设计检出中源码与保留18,285,979 字节 map-init/script 配对不可用。已接受0.083s 比较仅为溯源，不在此复制源码、采集、再生成或新私有比较。

[#1033 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1033#issuecomment-6183638591)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1036#issuecomment-6183661030)保留最终直接0.202s/独立0.195s/CI38675636173，与原0.207s/CI38675382781、更早0.215s 及 schema-path/local-$ref 守卫失败分别保留。独立的不存在区域文件名探测已改为既有所有者，未改数据。变基研究逐字节相同。[#1034 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1034#issuecomment-6183549178)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1035#issuecomment-6183600028)保留作者1.414s/独立1.433s/CI38674976874 及已修正检查/检查器诊断。前序保留 #1029 不同原/最终结果、失败与所链更早未知项；无完成套件重置。

[#1038](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1038)的 Standard 严谨级别区分新/续接状态、成员资格、物理身份与介绍完成。[当前协议](../../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)拥有五 Markdown 路径与验收边界。交接记录完整文字/diff/变化状态审阅、实际公共 schema/消费者连接、导航/链接/锚点/UTF-8/私有检查、已配置干净提交 planner 与实际精确提交头范围 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4/翻译套件。原切片未消费激活研究与 UI692；无新运行时队列。
