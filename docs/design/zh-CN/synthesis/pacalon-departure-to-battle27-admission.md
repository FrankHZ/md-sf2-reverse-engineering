# Pacalon 离开：北向滚动与有条件 Battle27 准入

> 本文是[规范英文原文：Pacalon departure: the north scroll and conditional Battle27 admission](../../synthesis/pacalon-departure-to-battle27-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

离开 Pacalon 不立即开始前往 Tristan 路上的战斗。**已确认：** 城堡南向请求先允许城镇战斗检查；城镇南向请求随后构建新大地图探索。向北滚动加载 Map74，同时保留该探索的行动者与脚本。独立获准 zone 回调可解锁战斗并请求重载。只有之后 MainLoop 扫描才选 battle27 或32。

本指南接续 [Pacalon 城堡：入口与首次旱灾状态访问](pacalon-castle-entry-and-first-visit.md)。止于 BattleLoop 调用，**早于初始化或执行**。**推断：** 北向请求与源码名 BATTLE_TO_TRISTAN27 描述朝 Tristan 旅行，不证明自然完成行走。**未知：** 所供状态、解码路径、已完成移动/清理/显示、可用控制与持久性。观察或重制版前沿均不推进。

续读 [Battle27：初始编队与限制对峙](battle27-initialization-and-opening.md)。该指南在此 BattleLoop 交接后消费已接受初始化/开场研究，区分新队伍/敌人构建、完整三人场景与 F88 所供状态延续。止于激活或单次行动执行前，保留自然路径/服务限制。

## 证据与范围

完整已接受[离开研究](../../../research/pacalon-departure-to-battle27-admission.md)是**已确认的静态证据所有者**，经 [#1029 / PR1032](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1032)在 `369757a59864cef7d4c0bf0e40769659fbab219d` 接受。已接受城堡指南与其[研究所有者](../../../research/pacalon-castle-entry-and-first-visit.md)、[城镇离开请求](../../../research/pacalon-interactions-and-castle-handoff.md#ordered-exits-and-outgoing-mainloop-request)、[入口/生命周期](../../../research/battle26-return-through-pacalon-entry.md)、[请求写入](../../../research/map-event-request-state.md)、[请求消费](../../../research/map-event-request-consumption.md)、[共享地图](../../../research/common-maps.md)与[随机战斗所有者](../../../research/map-event-random-battle-state.md)保留各自证据边界。[地图/探索合同](../contracts/map-exploration.md)区分静态记录、受控观察与自然进展。

源码路径相对 SF2DISASM `master` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6` 的 `disasm/`；研究保留 USA retail ROM 身份与源码/地址连接。Fnnn 指标志 nnn；行从零起。此设计检出无源码/配对。18,285,979 字节保留配对的已接受0.083s 比较仅属溯源，不在此复制源码、采集或作新私有比较。

[#1034](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1034)的 Standard 严谨级别区分新构建、保留滚动、可变事件字节与有序准入。五个所属 Markdown 路径解释已接受证据，不改研究、合同、测试夹具、schema 或引擎。Battle31 仅是城镇准入替代。后续 Frayja/cs_5DA28 历史、Battle27 初始化、后续战斗、服务、运行时与中文同步仍在原切片范围外。未合并 Battle27 开场研究与 UI692 不为本指南提供证据。

## 经城镇返回，再构建新大地图

**已确认：** 城堡 Map36 行0 从 `(255,31)` 请求无滚动 Map02，目的地 `(255,1)`、Down3。处理器采样 physical0 当前有符号坐标字除384。负目的地字节保留整采样字，非负只替换低字节。因此 X 是消费时 X，不是保存触发位置；非零采样 Y 高字节在替换1 后保留。输出 facing3、d4=0。待处理替换改变 MainLoop 实收请求。

MainLoop 先 SwitchMap 再 CheckBattle。Map02 无替换。唯一战斗候选31：通配触发、crop `(0,0,48,32)`、unlock431/completion531。431 未设允许探索；持有431 准入31；持有531 清431 但**本次调用仍准入**。该分支止于 BattleLoop 前。F530、国王话语、F810、拾取或服务都不是城镇准入前提。

若抵达探索，显式2 开始新生命周期。ExplorationLoop 清待处理类型，对有符号字 STEP_COUNTER 减20000 并条件回退零，治疗符合己方角色，请求淡出并设 view target-1。写 CURRENT_MAP2/CURRENT_BATTLE255，载瓦片集、等淡入淡出、清 VInt、选择/重建行动者和地图精灵、清临时256–383 并设80。之后精灵计数器、LoadMap、基础 VInt 与当前初始化，再有条件音乐/淡入淡出与 WaitForEvent。治疗覆盖存活己方以及始终符合的 Peter7/Lemon28，不是招募操作。

当前530 选 `ms_map2` 或 `ms_map2_flag530`，固定阵容不同，RTS 初始化相同。ClearEntities 丢弃城堡行动者、映射与行走临时区；标志重建随从。Town2 非大地图，无 F65 Caravan 前缀。主角用所供坐标/朝向与 Bowie 实际 HP/职业精灵规则。不重置 PLAYER_TYPE。非临时剧情/成员/物品标志保留；城堡当前 Frayja 移出状态不作为行动者记录带入重建。

城镇行3 在三个城堡行之后，从 `(255,31)` 请求无滚动71 `(22,16)`、Down3。调用方仍需适用布局/移动状态。行不测试国王访问、F810、对话或物品栏。MainLoop 的 SwitchMap 在530 持有时替换71→78，530 未设保持71。无反向78→71。两图均无战斗坐标行，因此完整71/78 抵达另一新探索生命周期，丢弃城镇人口。

两者均为大地图：F65 可在普通启用随从前加 Caravan。当前成员资格、HP/职业、标志与模式决定既有声明消费者；此处无己方加入。两图均无 MapSetups 行：GetCurrentMapSetup 返回 `ms_Void`/$FFFF，固定列表选择遇哨兵值，跳初始化。共享主角/随从/筏构建仍运行。无配置不等于有人口默认配置。

两头部均选 palette0/tilesets101–105、area `(0,0)..(63,63)`、layer type254、主旋律、parallax256/256、零自动滚动与其他层原点。不同图块/布局资源阻止推断地形相同。WaitForEvent 中待处理零则为 physical0 装控制：numeric mode0 角色、1 筏、其他非零 Caravan。即使枚举名称暗示另一解读，仍由消费者分支决定。非零待处理绕安装，优先于 A/C。构建不是已观察可用控制。

## 北向滚动保留探索人口

**已确认：** Map71 与78 各有五条有序八字节传送行。两城镇入口后，行2 为 `(255,0)` → Map74 `(255,62)`，scroll UP/$11，记录 facing Up1 加填充。行3 向南70，行4 向西72。首匹配有意义。这些记录不解码可行走北边缘。

普通 `esc02_controlCharacter` 在 WarpIfSetAtPoint 前用布局 kind$1000。所供坐标字含移动增量；生产者符号扩展/除384，先查$FFFF 终止符，再比源低字节。任意负源字节都是传送通配。依次写事件字1、type/map/X/Y 长字、facing/padding 字、WARP_SFX。无匹配不动待处理。筏/Caravan 另有调用方；普通角色控制不证明其路径。

ProcessMapEvent 在分发前清存储 type。PARAM1 非零时，滚动处理器让 physical0 Idle，从 PARAM2 写 CURRENT_MAP，再调 ProcessMapTransition。完整74 无71/F530 替换。过渡加载所选瓦片集/图块/布局，读/缩放首区域矩形，用当前 PARAM1 方向位作 Up 过渡，包括投影/平面/DMA/VInt 工作。这是源码顺序，不是测得呈现。

之后坐标更新用可变 PARAM3/4。负 X 字节255 跳 X/XDEST 两写入；变化为非负 X 字节时，将结果整字乘384，不隐式清继承高字节。处理器随后在读 Y 前清 d0.w：完整62 写 Y/YDEST62*384；变化为负 Y 字节则独立跳该轴。采样实际主角位置/朝向，调 DeclareRaftEntity 并返回探索。所记录 PARAM5/Up1 **不应用**。实际 facing 保持当前值。

滚动既不调 MainLoop/CheckBattle，也不开始新探索。不治疗、不减20000、不清临时标志、不替换 CURRENT_BATTLE、不重建行动者/映射/行走临时区、不调用 Map74 初始化。WARP_SFX 不经无滚动分支提交/清除。新随从标志不自动重建保留列表，但主角行动指针已变 Idle。

DeclareRaftEntity 在 numeric mode2 返回；mode1 跳随从重定位；其他模式将当前所列随从实际/目的地坐标移到采样主角瓦片。脚本/映射保留。F64 门控筏工作：RAFT_MAP 匹配时由掩码存储坐标声明 physical31；不匹配清查找并移出地图。64 未设跳修复。逻辑随从 B31 与物理筏31 是不同身份。

滚动不调 LoadMapArea。除显式过渡变更外，保留选定区域/音乐/层元数据与临时标志。LoadMapBlocksAndLayout 仍读实际 CURRENT_BATTLE：普通255 避免裁剪；陈旧非255 可应用所供裁剪。人口保留不证明每个行动者字段或已载资源均未变。

## 候选区域，不是已证明抵达

**已确认：** Map74 只有一个无标志 `ms_map74` 配置，无530/527 变体。固定列表空，初始化 RTS。两者均不作为滚动入口初始化运行。之后事件选择重新解析实际 CURRENT_MAP/配置。头部选 palette0、tilesets101/102/109/110/105、单个0..63 区、layer254/主旋律、parallax256/256、零原点/自动滚动。标志/踩踏/屋顶表仅 endWord。不禁用独立 zone 生产者或普通移动计数器。

Map74 出口是侧边界：两个 Moun31 请求、Tristan15、两个 bridge47 请求、两个重复向南滚动71 `(255,1)` 与向西滚动75 `(62,255)`。较早重复胜出；消费时南71 可变78。均不证明抵达、可用通道或目的地访问。

普通 zone 生产者是 `esc02_controlCharacter` 的**布局 kind$1400** 分支，位于候选移动/重叠及布局查找之后。先写事件字6，再将有符号瓦片商 `(XDEST+d4)/384` 以字写 PARAM1/2，将 `(YDEST+d5)/384` 以字写 PARAM3/4。d4/d5 是选定移动增量。这些是候选目的地，不是采样实际坐标。无待处理守卫或原子载荷；PARAM5 与 WARP_SFX 保留。后续碰撞/调试/flagsA-bit6 逻辑仍可跳移动写入。继续移动路径设速度/行程/目的地并递减正 STEP_COUNTER。提交事件不证明抵达。

Type6 分发在 physical0 安装 eas_Init，读两参数字作 X/Y。RunMapSetupZoneEvent 解析当前 zone 表，先查默认 marker$FD，接受精确$FF 通配或匹配坐标低字节，再将首匹配有符号相对目标加表基址。此通配比传送生产者任意负字节规则窄。Map74 行0 `(255,61)` 对任意所供 X 与 Y 低字节61 选 Map74_ZoneEvent0。否则默认返回。不证明61 行每个 X 都是 zone-kind 单元格，或从北边缘有可通行路径。

回调返回后包装器关立绘/文本，等一 VInt，再等 physical0 实际坐标等于目的地，之后恢复寄存器。不清新待处理事件。缺失配置绕选择/窗口清理，仍走通用 VInt/运动等待。地图、模式、候选字节或外部事件写入变化会改选择；仅回调返回不证明清理或控制。

## 回调改变什么，重载保留什么

**已确认：** Map74_ZoneEvent0 全部分支如下：

| 读取时标志 | 有序效果 |
| --- | --- |
|527 未设|设427 → 重载长字 → 设428 → 设429 → 设430 → 返回。|
|527 持有，811 未设|返回，无这些写入。|
|527 持有，811 持有，532 持有|返回，无这些写入。|
|527 持有，811 持有，532 未设|设432 → 重载长字 → 返回。|

首分支从不查811/532。无分支清427/432 或写527/530/532。无写入路径保留既有解锁；527 未设时即使427 已持有也重复写。F428–430 是解锁，不是完成。F811 源码注释指 Frayja 请求去 Moun，但读取不证明延后场景/历史、恢复降雨或结果。不测试 F530。

重载常量$000100FF 在 MAP_EVENT_TYPE 作**四字节写入**：

| 字段 | 写入后字节 |
| --- | --- |
|Event word, $FFA84A..$FFA84B|$0001：warp|
|PARAM1, $FFA84C|$00：no-scroll|
|PARAM2, $FFA84D|$FF：CURRENT|
|PARAM3/4, $FFA84E/$FFA84F|未变 zone Y 字，通常$003D|
|PARAM5、padding 与 WARP_SFX|未变|

保留 Y 字$003D 之后被读成 **warp X byte0 / Y byte61**，不是两个新哨兵坐标。变化 Y 高字节改变 X 字节，而低字节61 仍匹配 zone。若无写入替换，朝向可能仍是北记录 Up1；音效同样是陈旧状态，不是新回调指令。事件槽可变，可替换另一请求，不是不可变排队旅程。

427 后、重载前中断留下有解锁无此请求。重载后、428–430 前中断留下标志写入前缀。432 分支同样先设标志再提交。每个回调写入之后都可在清理中断；后续写入者可独立替换类型或参数。无回滚或原子标志/请求提交。

回调不调 CheckRandomBattle、不抽 RNG、不重置计数器30000、不等视野滚动、不请求 boost 音效或闪烁。通用辅助有自身完成/计数器/RNG 路径与请求序列。普通移动计数递减与之后探索相减是不同消费者，不使确定性回调成为随机遭遇规则。

## 重载消费与有序战斗决定

**已确认：** 回调/包装器返回后，探索设移动音效并重入 WaitForEvent。完整 event1 绕控制安装，优先 A/C。分发清存储类型；PARAM1 零则提交/清当前 WARP_SFX，使主角 Idle，退出探索回 MainLoop。清 d0–d4 字、读 map255、采样实际主角坐标字/朝向。非负 PARAM3/4 字节替换坐标低字节，负字节保留采样字；PARAM5 替换朝向，PARAM1 给 d4=0。字段在不同时间读，因此替换必须按实际消费评估，不能从早先生产者重建。

普通较小采样坐标且$003D 未变时，输出寄存器种子为 CURRENT255 `(0,61)` 与保留 PARAM5。请求本身不将行动者移至 X0。所供采样坐标高字节保留；变化为负目的地字节则保留对应整字。

MainLoop 先 SwitchMap（无255 替换），再 CheckBattle。保存原地图/坐标寄存器，从实际 CURRENT_MAP 解析 CURRENT，按源码顺序扫描45 条七字节记录。恰两条用 Map74：

| 战斗 | Crop X,Y,width,height | Trigger X,Y | 解锁 / 完成 |
| --- | --- | --- | --- |
|27，较早|2,16,32,48|255,255|427 /527|
|32，较后|2,16,32,48|255,255|432 /532|

地图相等且持有解锁即准入；触发用精确字节或255 通配匹配。两者完全通配。候选 Y61、输出 X0 与裁剪包含性均**不是准入谓词**。裁剪在选中后输出。其他地图解锁含428–430 不提供 Map74 行。

| 实际扫描时状态 | 结果 |
| --- | --- |
|427 持有，432 任意|27 胜出；写裁剪；持有527 清427，但现在仍准入27。|
|427 未设，432 持有|32 胜出；写裁剪；持有532 清432，但现在仍准入32。|
|两者未设|d7=-1；不写裁剪；恢复原地图操作数255。|

完成不是拒绝门槛。即使回调设432，已有427 时仍可导向27。无写入回调路径本身不提交扫描，但独立无滚动请求可扫描保留解锁。变化 CURRENT_MAP 重定向解析。CheckBattle 不设/重置 CURRENT_BATTLE。成功返回解析地图74 与 d7=27/32；MainLoop 将 d7 移 d1 并调 j_BattleLoop。**这就是停止边界。** 名册、开场脚本、初始化、单次行动与结果属独立工作。

无匹配时，CURRENT255 抵达保留探索：清待处理、调整计数器、符合治疗/淡出、主角朝向/精灵/坐标更新、DeclareRaftEntity、精灵计数器、LoadMap、基础 VInt 与当前初始化。低字节非负时整个坐标字乘384。行动者/映射/行走临时区、临时标志与 CURRENT_BATTLE 保留。74 仍当前时 Map74 初始化 RTS；变化地图/状态依其所有者。

LoadMap 的 CURRENT 分支绕过新调色板/瓦片集与图块/布局加载，保留已载资源，但刷新区域/视野/显示/屋顶处理。可刷新先前滚动遗留的 Map74 元数据。陈旧非255 CURRENT_BATTLE 可使 LoadMapArea 用所供战斗尺寸；CURRENT 不修复。有条件音乐/淡入淡出及待处理/控制门槛仍适用。若替换为显式地图，则走其准入/新生命周期。仅无匹配不建立可用控制。

## 变化状态与保留证据审阅

| 变化前提或中断 | 必需解读 |
| --- | --- |
|530 在选择前/后改变|71→78 采样当前530；无追溯重载或北74 替换。|
|城堡返回时431/531|城镇探索有条件；Battle31 交接独立。|
|行动者/随从/模式/映射/HP/职业变化|新重建不同于滚动/CURRENT 保留；重定位与 Idle 写入仍改行动者。|
|布局/候选/增量/碰撞不同|Warp$1000、zone$1400、表匹配与实际抵达独立。|
|527/811/532 在读取间变化|实际分支顺序胜出；无写入分支保留既有解锁。|
|427/432 均持有/均未设；完成持有|27 优先、清解锁但当次仍准入，或保留 CURRENT 回退。|
|428–430 仅部分到达|保留写入前缀；不推断完成或新 Map74 候选。|
|CURRENT_MAP/CURRENT_BATTLE 陈旧或变化|实际地图解析扫描；陈旧战斗可影响滚动裁剪与回退区域。|
|高/哨兵字节、朝向/音效或待处理替换|候选 zone 字变为不同解释的 warp 字节；消费采样实际位置。|
|回调/清理/过渡/控制中断|已到达变更保留；脚本已返回、等待完成与可用控制是不同声明。|

合成前审阅消费完整已接受 #1029 与城堡指南、必需入口/请求/共享地图/随机辅助所有者，以及下列实际公共载荷、schema 与验证器消费者。未发现阻塞分歧。未来矛盾交研究；Layer B 不静默修补。

| 公共测试夹具 / 消费者 | 已审阅连接与限制 |
| --- | --- |
|`sf2-map-init-static-v1`；[测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)、[消费者](../../../../src/sf2tool/h2/map_init.py)|实际 Map74 路由126、指针与 RTS 体；无71/78 路由。这不是观察滚动入口执行。|
|`sf2-map-events-static-v1:routing-setup` / `:zone-programs`；[路由](../../../../tests/fixtures/h2/map-events/routing-setup.json)、[程序](../../../../tests/fixtures/h2/map-events/zone-programs.json)、[加载器](../../../../src/sf2tool/h2/map_events_fixture.py)、[验证器](../../../../src/sf2tool/h2/map_events.py)|Map74 zone/默认目标、首匹配事实、全部15 回调操作/分支与两重载写入。schema 有效结构记录不证明自然生产者到消费者路径。|
|`sf2-map-event-request-state-static-v1`；[测试夹具](../../../../tests/fixtures/h2/map-event-request-state-static-v1.json)、[消费者](../../../../src/sf2tool/h2/map_event_request_state.py)|两 Map74 长字定义与调用方局部控制/到达交接。被调用方算法与运行时消费仍在紧凑关系外。|
|`sf2-map-event-request-consumption-static-v1`；[测试夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)、[消费者](../../../../src/sf2tool/h2/map_event_request_consumption.py)|实际探索重置、WaitForEvent 读取、分发前清除/分支与筏访问。有界上下文不进入完整传送/滚动/地图被调用方。|
|`sf2-common-maps-static-v1`；[测试夹具](../../../../tests/fixtures/h2/common-maps-static-v1.json)、[消费者](../../../../src/sf2tool/h2/maps.py)|SwitchMap、有序触发/完成与资源加载事实。完整选定坐标/滚动/zone 生产者与回退连接依已接受 #1029，不在此新查私有源码。|
|`sf2-map-event-random-battle-state-static-v1`；[测试夹具](../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)、[消费者](../../../../src/sf2tool/h2/map_event_random_battle_state.py)|实际七选定程序/八辅助调用方排除 Map74；有守卫计数器/RNG/请求体不同于其确定性回调。未观察自然随机遭遇。|

所属命令为 `uv run sf2 h2 map-init`、`map-events`、`map-event-request-state`、`map-event-request-consumption`、`common-maps` 与 `map-event-random-battle-state`。直接检查公共表面，不重跑私有源码提取/运行时命令。紧凑测试夹具计数不替代已接受相连源码审阅，也不解码地形。

[#1029 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1029#issuecomment-6183401642)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1032#issuecomment-6183422393)保留最终直接0.143s/独立0.136s 与 CI38673862670，与原0.132s/0.134s、CI38673722838 及已修正 KeyError('path') 检查诊断分别保留。研究文档在仅合成变基前后逐字节相同。已接受0.083s 配对结果复用，不重置。

[#1030 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1030#issuecomment-6183352710)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1031#issuecomment-6183367265)保留直接3.075s/独立3.229s 与 CI38673538882，检查器导入/字段/链接诊断独立。前序指南保留所链 #1025/#1026 与更早结果、修正、失败和未知项。

此原 Issue 交接记录完整文字/diff/变化状态审阅、实际公共 schema/消费者连接、精确五路径、导航/链接/锚点/UTF-8/私有检查、git diff --check、已配置干净提交头 planner 与实际精确提交头范围 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4/翻译套件。可选压缩布局缺失仍是无解码路径限制，不是采集授权或 ROM 阻塞。自然抵达、所供标志/状态、中断/时序、已完成移动/清理/呈现、可用控制与存档/读档持久性仍**未知**，不创建运行时队列。
